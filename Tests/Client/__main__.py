# Runs all unit tests in the Tests.Client package
import unittest

if __name__ == '__main__':
    suite = unittest.defaultTestLoader.discover(start_dir='Tests/Client', pattern='test_*.py')
    unittest.TextTestRunner().run(suite)
