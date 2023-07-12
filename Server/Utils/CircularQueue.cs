using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moonlapse.Server.Utils;

/// <summary>
/// A queue that holds a fixed number of items. When the queue is full, adding a new item will remove the oldest item.
/// </summary>
public class CircularQueue<T> : IEnumerable<T> {
    readonly int size;
    readonly object locker;

    int count;
    int head;
    int rear;
    T[] values;

    /// <summary>
    /// Creates a new instance of <c>CircularQueue</c> with the specified maximum size.
    /// </summary>
    public CircularQueue(int max) {
        this.size = max;
        locker = new object();
        count = 0;
        head = 0;
        rear = 0;
        values = new T[size];
    }

    /// <summary>
    /// Maximum number of items that can be held in the queue.
    /// </summary>
    public int Size { get { return size; } }
    
    /// <summary>
    /// Object for lock during thread-safe operations.
    /// </summary>
    public object SyncRoot { get { return locker; } }

    #region Count
    /// <summary>
    /// Number of items currently in the queue.
    /// </summary>
    public int Count { get { return UnsafeCount; } }
    /// <summary>
    /// Thread-safe version of <c>Count</c>.
    /// </summary>
    public int SafeCount { get { lock (locker) { return UnsafeCount; } } }
    int UnsafeCount { get { return count; } }

    #endregion

    #region Enqueue
    /// <summary>
    /// Adds an item to the end of the queue. If the queue is full, the oldest item will be removed.
    /// </summary>
    public void Enqueue(T obj) {
        UnsafeEnqueue(obj);
    }

    /// <summary>
    /// Thread-safe version of <c>Enqueue</c>.
    /// </summary>
    public void SafeEnqueue(T obj) {
        lock (locker) { UnsafeEnqueue(obj); }
    }

    void UnsafeEnqueue(T obj) {
        values[rear] = obj;

        if (Count == Size)
            head = moduloIncrement(head, Size);
        rear = moduloIncrement(rear, Size);
        count = Math.Min(count + 1, Size);
    }

    #endregion

    #region Dequeue
    /// <summary>
    /// Removes and returns the oldest item in the queue.
    /// </summary>
    public T Dequeue() {
        return UnsafeDequeue();
    }

    /// <summary>
    /// Thread-safe version of <c>Dequeue</c>.
    /// </summary>
    public T SafeDequeue() {
        lock (locker) { return UnsafeDequeue(); }
    }

    T UnsafeDequeue() {
        unsafeEnsureQueueNotEmpty();

        T res = values[head];
        values[head] = default(T);
        head = moduloIncrement(head, Size);
        count--;

        return res;
    }

    #endregion

    #region Peek
    /// <summary>
    /// Returns the oldest item in the queue without removing it.
    /// </summary>
    public T Peek() {
        return UnsafePeek();
    }

    /// <summary>
    /// Thread-safe version of <c>Peek</c>.
    /// </summary>
    public T SafePeek() {
        lock (locker) { return UnsafePeek(); }
    }

    T UnsafePeek() {
        unsafeEnsureQueueNotEmpty();

        return values[head];
    }

    #endregion


    #region GetEnumerator
    /// <summary>
    /// Returns an enumerator that iterates through the queue.
    /// </summary>
    public IEnumerator<T> GetEnumerator() {
        return UnsafeGetEnumerator();
    }

    /// <summary>
    /// Thread-safe version of GetEnumerator.
    /// </summary>
    public IEnumerator<T> SafeGetEnumerator() {
        lock (locker) {
            List<T> res = new List<T>(count);
            var enumerator = UnsafeGetEnumerator();
            while (enumerator.MoveNext())
                res.Add(enumerator.Current);
            return res.GetEnumerator();
        }
    }

    IEnumerator<T> UnsafeGetEnumerator() {
        int index = head;
        for (int i = 0; i < count; i++) {
            yield return values[index];
            index = moduloIncrement(index, size);
        }
    }

    private static int moduloIncrement(int index, int size) {
        return (index + 1) % size;
    }

    private void unsafeEnsureQueueNotEmpty() {
        if (count == 0)
            throw new InvalidOperationException("Empty queue");
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    #endregion
}