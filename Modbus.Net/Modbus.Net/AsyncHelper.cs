using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Modbus.Net
{
    public static class AsyncHelper
    {
        private static readonly TaskFactory _myTaskFactory = new
          TaskFactory(CancellationToken.None,
                      TaskCreationOptions.None,
                      TaskContinuationOptions.None,
                      TaskScheduler.Default);

        /// <summary>
        /// Run async method syncronized
        /// </summary>
        /// <typeparam name="TResult">Return type</typeparam>
        /// <param name="func">Async method with return</param>
        /// <returns>Return value</returns>
        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return AsyncHelper._myTaskFactory
              .StartNew<Task<TResult>>(func)
              .Unwrap<TResult>()
              .GetAwaiter()
              .GetResult();
        }

        /// <summary>
        /// Run async method syncronized.
        /// </summary>
        /// <param name="func">Async method</param>
        public static void RunSync(Func<Task> func)
        {
            AsyncHelper._myTaskFactory
              .StartNew<Task>(func)
              .Unwrap()
              .GetAwaiter()
              .GetResult();
        }

        /// <summary>
        /// Change async task to async task with cancellation token
        /// </summary>
        /// <param name="task">Async task</param>
        /// <param name="token">Cancellation Token</param>
        /// <returns>Task with Cancellation token</returns>
        public static Task WithCancellation(this Task task,
            CancellationToken token)
        {
            return task.ContinueWith(t => t.GetAwaiter().GetResult(), token);
        }

        public static Task<T> WithCancellation<T>(this Task<T> task,
            CancellationToken token)
        {
            return task.ContinueWith(t => t.GetAwaiter().GetResult(), token);
        }
    }

    /// <summary>
    /// AsyncLock locks across one or several await calls.
    /// </summary>
    public class AsyncLock
    {
        private readonly AsyncSemaphore _semaphore;
        private readonly Task<Releaser> _releaser;

        public AsyncLock()
        {
            _semaphore = new AsyncSemaphore(1);
            _releaser = Task.FromResult(new Releaser(this));
        }

        /// <summary>
        /// Lock the async method. Call like: using (await asynclock.LockAsync())
        /// </summary>
        /// <returns></returns>
        public Task<Releaser> LockAsync()
        {
            var wait = _semaphore.WaitAsync();
            return wait.IsCompleted
                ? _releaser
                : wait.ContinueWith((_, state) => new Releaser((AsyncLock) state),
                    this, CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }


        public struct Releaser : IDisposable
        {
            private readonly AsyncLock _toRelease;

            internal Releaser(AsyncLock toRelease)
            {
                _toRelease = toRelease;
            }

            public void Dispose()
            {
                if (_toRelease != null)
                {
                    _toRelease._semaphore.Release();
                }
            }
        }
    }

    /// <summary>
    /// AsyncSemaphore semaphore the multi run tasks.
    /// </summary>
    public class AsyncSemaphore
    {
        private readonly static Task _completed = Task.FromResult(true);
        private readonly Queue<TaskCompletionSource<bool>> _waiters = new Queue<TaskCompletionSource<bool>>();
        private int _currentCount;

        public AsyncSemaphore(int initialCount)
        {
            if (initialCount < 0)
            {
                throw new ArgumentOutOfRangeException("initialCount");
            }
            _currentCount = initialCount;
        }

        public Task WaitAsync()
        {
            lock (_waiters)
            {
                if (_currentCount > 0)
                {
                    _currentCount--;
                    return _completed;
                }
                else
                {
                    var waiter = new TaskCompletionSource<bool>();
                    _waiters.Enqueue(waiter);
                    return waiter.Task;
                }
            }
        }

        public void Release()
        {
            TaskCompletionSource<bool> toRelease = null;
            lock (_waiters)
            {
                if (_waiters.Count > 0)
                {
                    toRelease = _waiters.Dequeue();
                }
                else
                {
                    _currentCount++;
                }
            }
            if (toRelease != null)
            {
                toRelease.SetResult(true);
            }
        }
    }
}
