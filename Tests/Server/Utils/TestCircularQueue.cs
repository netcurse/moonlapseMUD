using Moonlapse.Server.Utils;

namespace Moonlapse.Server.Tests.Unit.Utils;

public class TestCircularQueue {

    [Fact]
    void TestNewQueueHasZeroCount() {
        var queue = new CircularQueue<int>(10);
        queue = new(10);
        Assert.Equal(0, queue.Count);
    }

    [Fact]
    void TestCountIncreasesWhenEnqueue() {
        var queue = new CircularQueue<int>(10);
        queue.Enqueue(1);
        Assert.Equal(1, queue.Count);
        queue.Enqueue(2);
        Assert.Equal(2, queue.Count);
    }

    [Fact]
    void TestCountDoesNotExceedCapacity() {
        var queue = new CircularQueue<int>(2);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        Assert.Equal(2, queue.Count);
    }

    [Fact]
    void TestOldestItemIsRemovedWhenCapacityExceeded() {
        var queue = new CircularQueue<int>(2);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        Assert.Equal(2, queue.Dequeue());
        Assert.Equal(3, queue.Dequeue());
    }

    [Fact]
    void TestExceptionWhenDequeueEmptyQueue() {
        var queue = new CircularQueue<int>(2);
        Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
    }

    [Fact]
    void TestPeekReturnsOldestItem() {
        var queue = new CircularQueue<int>(2);
        queue.Enqueue(1);
        queue.Enqueue(2);
        Assert.Equal(1, queue.Peek());
    }

    [Fact]
    void TestPeekDoesNotRemoveItem() {
        var queue = new CircularQueue<int>(2);
        queue.Enqueue(1);
        queue.Enqueue(2);
        Assert.Equal(1, queue.Peek());
        Assert.Equal(1, queue.Peek());
    }

    [Fact]
    void TestGetEnumeratorReturnsItemsInCorrectOrder() {
        var queue = new CircularQueue<int>(3);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        var enumerator = queue.GetEnumerator();
        enumerator.MoveNext();
        Assert.Equal(1, enumerator.Current);
        enumerator.MoveNext();
        Assert.Equal(2, enumerator.Current);
        enumerator.MoveNext();
        Assert.Equal(3, enumerator.Current);
    }

    // Test multiple operations together
    [Fact]
    void TestEnqueueDequeuePattern() {
        var queue = new CircularQueue<int>(3);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        Assert.Equal(1, queue.Dequeue());
        queue.Enqueue(4);
        Assert.Equal(2, queue.Dequeue());
    }

    // Test enqueue and dequeue pattern with size 1
    [Fact]
    void TestEnqueueDequeuePatternWithSizeOne() {
        var queue = new CircularQueue<int>(1);
        queue.Enqueue(1);
        Assert.Equal(1, queue.Dequeue());
        queue.Enqueue(2);
        Assert.Equal(2, queue.Dequeue());
    }

    // Test enqueue and dequeue pattern when queue is full
    [Fact]
    void TestEnqueueDequeuePatternWhenFull() {
        var queue = new CircularQueue<int>(3);
        queue.Enqueue(1);
        queue.Enqueue(2);
        queue.Enqueue(3);
        Assert.Equal(1, queue.Dequeue());
        queue.Enqueue(4);
        Assert.Equal(2, queue.Dequeue());
        queue.Enqueue(5);
        Assert.Equal(3, queue.Peek());
        Assert.Equal(3, queue.Count);
    }

    // Test SafeEnqueue and SafeDequeue methods
    [Fact]
    void TestSafeMethods() {
        var queue = new CircularQueue<int>(3);
        queue.SafeEnqueue(1);
        queue.SafeEnqueue(2);
        queue.SafeEnqueue(3);
        Assert.Equal(1, queue.SafeDequeue());
        queue.SafeEnqueue(4);
        Assert.Equal(2, queue.SafeDequeue());
        Assert.Equal(2, queue.SafeCount);
        Assert.Equal(3, queue.SafePeek());
    }

    // Test GetEnumerator with safe methods and exceeding the size
    [Fact]
    void TestSafeGetEnumeratorWhenExceedingSize() {
        var queue = new CircularQueue<int>(3);
        queue.SafeEnqueue(1);
        queue.SafeEnqueue(2);
        queue.SafeEnqueue(3);
        queue.SafeEnqueue(4); // This will kick out the first item (1)
        var enumerator = queue.SafeGetEnumerator();
        enumerator.MoveNext();
        Assert.Equal(2, enumerator.Current); // So the first item is now 2
        enumerator.MoveNext();
        Assert.Equal(3, enumerator.Current);
        enumerator.MoveNext();
        Assert.Equal(4, enumerator.Current); // The last item is 4
    }
}