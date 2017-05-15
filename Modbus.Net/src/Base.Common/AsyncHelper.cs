using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    /// AsyncHelper Class
    /// </summary>
    public static class AsyncHelper
    {
        private static readonly TaskFactory _myTaskFactory = new
            TaskFactory(CancellationToken.None,
                TaskCreationOptions.None,
                TaskContinuationOptions.None,
                TaskScheduler.Default);

        /// <summary>
        ///     Run async method syncronized
        /// </summary>
        /// <typeparam name="TResult">Return type</typeparam>
        /// <param name="func">Async method with return</param>
        /// <returns>Return value</returns>
        public static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return _myTaskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        ///     Run async method syncronized.
        /// </summary>
        /// <param name="func">Async method</param>
        public static void RunSync(Func<Task> func)
        {
            _myTaskFactory
                .StartNew(func)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        ///     Change async task to async task with cancellation token
        /// </summary>
        /// <param name="task">Async task</param>
        /// <param name="token">Cancellation Token</param>
        /// <returns>Task with Cancellation token</returns>
        public static Task WithCancellation(this Task task,
            CancellationToken token)
        {
            return task.ContinueWith(t => t.GetAwaiter().GetResult(), token);
        }

        /// <summary>
        ///     Add a CancellationToken to async task
        /// </summary>
        /// <typeparam name="T">type of a task</typeparam>
        /// <param name="task">Task</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Task with cancellation token</returns>
        public static Task<T> WithCancellation<T>(this Task<T> task,
            CancellationToken token)
        {
            return task.ContinueWith(t => t.GetAwaiter().GetResult(), token);
        }
    }
}