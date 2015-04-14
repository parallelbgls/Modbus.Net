using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;
using ModBus.Net;

namespace ModBus.Net
{
    public static class TimeRestore
    {
        public static int Restore = 0;
    }

    public class LimitedConcurrencyLevelTaskScheduler : TaskScheduler
    {
        /// <summary>
        /// Whether the current thread is processing work items.
        /// </summary>
        [ThreadStatic]
        private static bool _currentThreadIsProcessingItems;
        /// <summary>
        /// The list of tasks to be executed.
        /// </summary>
        private readonly LinkedList<Task> _tasks = new LinkedList<Task>(); // protected by lock(_tasks)
        /// <summary>
        /// The maximum concurrency level allowed by this scheduler.
        /// </summary>
        private readonly int _maxDegreeOfParallelism;
        /// <summary>
        /// Whether the scheduler is currently processing work items.
        /// </summary>
        private int _delegatesQueuedOrRunning = 0; // protected by lock(_tasks)

        /// <summary>
        /// Initializes an instance of the LimitedConcurrencyLevelTaskScheduler class with the
        /// specified degree of parallelism.
        /// </summary>
        /// <param name="maxDegreeOfParallelism">
        /// The maximum degree of parallelism provided by this scheduler.
        /// </param>
        public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism < 1) throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        /// <summary>
        /// Queues a task to the scheduler.
        /// </summary>
        /// <param name="task">
        /// The task to be queued.
        /// </param>
        protected sealed override void QueueTask(Task task)
        {
            // Add the task to the list of tasks to be processed.  If there aren't enough
            // delegates currently queued or running to process tasks, schedule another.
            lock (_tasks)
            {
                _tasks.AddLast(task);
                if (_delegatesQueuedOrRunning < _maxDegreeOfParallelism)
                {
                    ++_delegatesQueuedOrRunning;
                    NotifyThreadPoolOfPendingWork();
                }
            }
        }

        /// <summary>
        /// Informs the ThreadPool that there's work to be executed for this scheduler.
        /// </summary>
        private void NotifyThreadPoolOfPendingWork()
        {
            ThreadPool.UnsafeQueueUserWorkItem(_ =>
            {
                // Note that the current thread is now processing work items.
                // This is necessary to enable inlining of tasks into this thread.
                _currentThreadIsProcessingItems = true;
                try
                {
                    // Process all available items in the queue.
                    while (true)
                    {
                        Task item;
                        lock (_tasks)
                        {
                            // When there are no more items to be processed,
                            // note that we're done processing, and get out.
                            if (_tasks.Count == 0)
                            {
                                --_delegatesQueuedOrRunning;
                                break;
                            }

                            // Get the next item from the queue
                            item = _tasks.First.Value;
                            _tasks.RemoveFirst();
                        }

                        // Execute the task we pulled out of the queue
                        base.TryExecuteTask(item);
                    }
                }
                // We're done processing items on the current thread
                finally { _currentThreadIsProcessingItems = false; }
            }, null);
        }

        /// <summary>Attempts to execute the specified task on the current thread.</summary>
        /// <param name="task">The task to be executed.</param>
        /// <param name="taskWasPreviouslyQueued"></param>
        /// <returns>Whether the task could be executed on the current thread.</returns>
        protected sealed override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            // If this thread isn't already processing a task, we don't support inlining
            if (!_currentThreadIsProcessingItems) return false;

            // If the task was previously queued, remove it from the queue
            if (taskWasPreviouslyQueued) TryDequeue(task);

            // Try to run the task.
            return base.TryExecuteTask(task);
        }

        /// <summary>
        /// Attempts to remove a previously scheduled task from the scheduler.
        /// </summary>
        /// <param name="task">
        /// The task to be removed.
        /// </param>
        /// <returns>
        /// Whether the task could be found and removed.
        /// </returns>
        protected sealed override bool TryDequeue(Task task)
        {
            lock (_tasks) return _tasks.Remove(task);
        }

        /// <summary>
        /// Gets the maximum concurrency level supported by this scheduler.
        /// </summary>
        public sealed override int MaximumConcurrencyLevel { get { return _maxDegreeOfParallelism; } }

        /// <summary>
        /// Gets an enumerable of the tasks currently scheduled on this scheduler.
        /// </summary>
        /// <returns>
        /// An enumerable of the tasks currently scheduled.
        /// </returns>
        protected sealed override IEnumerable<Task> GetScheduledTasks()
        {
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(_tasks, ref lockTaken);
                if (lockTaken) return _tasks.ToArray();
                else throw new NotSupportedException();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_tasks);
            }
        }
    }

    public class BaseMachineEqualityComparer : IEqualityComparer<BaseMachine>
    {
        public bool Equals(BaseMachine x, BaseMachine y)
        {
            return x.BaseUtility.ConnectionToken == y.BaseUtility.ConnectionToken;
        }

        public int GetHashCode(BaseMachine obj)
        {
            return obj.GetHashCode();
        }
    }

    public class TaskManager
    {
        private HashSet<BaseMachine> _machines;
        private TaskFactory<IEnumerable<IEnumerable<object>>> _tasks;
        private TaskScheduler _scheduler;
        private CancellationTokenSource _cts;
        private Timer _timer;

        private bool _keepConnect;
        public bool KeepConnect { get { return _keepConnect; } set { TaskStop(); _keepConnect = value;
            foreach (var machine in _machines)
            {
                machine.KeepConnect = _keepConnect;
            }
        } }

        public delegate void ReturnValuesDelegate(KeyValuePair<string, IEnumerable<IEnumerable<object>>> returnValue);

        public event ReturnValuesDelegate ReturnValues;

        private int _getCycle;

        /// <summary>
        /// 毫秒
        /// </summary>
        public int GetCycle
        {
            get { return _getCycle; }
            set
            {
                if (value == Timeout.Infinite)
                {
                    if (_timer != null)
                    {
                        _timer.Stop();
                    }
                }
                else 
                {
                    if (value > 0)
                    {
                        _getCycle = value;
                    }
                    if (_timer != null) _timer.Interval = _getCycle*1000;
                    else
                    {
                        _timer = new Timer(_getCycle*1000);
                        _timer.Elapsed += MaintainTasks;                        
                    }
                    _timer.Start();
                }
            }
        }

        public int MaxRunningTasks
        {
            get { return _scheduler.MaximumConcurrencyLevel; }
            set
            {
                TaskStop();
                _scheduler = new LimitedConcurrencyLevelTaskScheduler(value);           
            }
        }

        public TaskManager(int maxRunningTask, int getCycle, bool keepConnect)
        {
            _scheduler = new LimitedConcurrencyLevelTaskScheduler(maxRunningTask);
            _machines = new HashSet<BaseMachine>(new BaseMachineEqualityComparer());         
            _getCycle = getCycle;
            KeepConnect = keepConnect;
        }

        public void AddMachine(BaseMachine machine)
        {
            _machines.Add(machine);
            machine.KeepConnect = KeepConnect;
        }

        private void MaintainTasks(object sender, System.Timers.ElapsedEventArgs e)
        {
            foreach (var machine in _machines)
            {
                RunTask(machine);
            }
        }

        public void TaskStart()
        {
            TaskStop();
            _cts = new CancellationTokenSource();
            _tasks = new TaskFactory<IEnumerable<IEnumerable<object>>>(_cts.Token, TaskCreationOptions.None, TaskContinuationOptions.None, _scheduler);
            GetCycle = TimeRestore.Restore;
        }

        public void TaskStop()
        {
            GetCycle = Timeout.Infinite;
            if (_cts != null)
            {
                _cts.Cancel();
            }
            if (_machines != null)
            {
                foreach (var machine in _machines)
                {
                    machine.BaseUtility.DisConnect();
                }
            }
        }

        private void RunTask(BaseMachine machine)
        {
            try
            {
                //var ans = machine.GetDatas();
                var ans = _tasks.StartNew(machine.GetDatas).Result;
                if (ReturnValues != null)
                {
                    ReturnValues(new KeyValuePair<string, IEnumerable<IEnumerable<object>>>(machine.BaseUtility.ConnectionToken, ans));
                }
            }
            catch (Exception e)
            {
                
                if (ReturnValues != null)
                {
                    ReturnValues(new KeyValuePair<string, IEnumerable<IEnumerable<object>>>(machine.BaseUtility.ConnectionToken, null));
                }
            }
        }
    }
}