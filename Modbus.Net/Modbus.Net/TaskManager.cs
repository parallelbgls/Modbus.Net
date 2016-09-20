using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Modbus.Net
{
    public enum MachineDataType
    {
        Address,
        CommunicationTag,
    }

    public class TaskReturnDef
    {
        public string MachineId { get; set; }
        public Dictionary<string, ReturnUnit> ReturnValues { get; set; }
    }

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

    public class TaskManager
    {
        /// <summary>
        /// 正在运行的设备
        /// </summary>
        private HashSet<BaseMachine> _machines;
        /// <summary>
        /// 不在运行的设备
        /// </summary>
        private HashSet<BaseMachine> _unlinkedMachines;

        private TaskFactory _tasks;
        private TaskScheduler _scheduler;
        private CancellationTokenSource _cts;

        /// <summary>
        /// 正常读取的计时器
        /// </summary>
        private Timer _timer;
        /// <summary>
        /// 重连计时器
        /// </summary>
        private Timer _timer2;

        /// <summary>
        /// 保持连接
        /// </summary>
        private bool _keepConnect;

        /// <summary>
        /// 保持连接
        /// </summary>
        public bool KeepConnect
        {
            get { return _keepConnect; }
            set
            {
                TaskStop();
                _keepConnect = value;
                lock (_machines)
                {
                    foreach (var machine in _machines)
                    {
                        machine.KeepConnect = _keepConnect;
                    }
                }
            }
        }

        /// <summary>
        /// 返回数据代理
        /// </summary>
        /// <param name="returnValue"></param>
        public delegate void ReturnValuesDelegate(TaskReturnDef returnValue);

        /// <summary>
        /// 返回数据事件
        /// </summary>
        public event ReturnValuesDelegate ReturnValues;

        /// <summary>
        /// 获取间隔
        /// </summary>
        private int _getCycle;

        /// <summary>
        /// 获取间隔，毫秒
        /// </summary>
        public int GetCycle
        {
            get { return _getCycle; }
            set
            {
                if (value == _getCycle) return;

                if (value == Timeout.Infinite)
                {
                    if (_timer != null)
                    {
                        _timer.Change(Timeout.Infinite, Timeout.Infinite);
                        _timer2.Change(Timeout.Infinite, Timeout.Infinite);
                        _timer.Dispose();
                        _timer2.Dispose();
                        _timer = null;
                        _timer2 = null;
                    }
                }
                else if (value < 0) return;
                else 
                {              
                    if (_timer != null)
                    {
                        _timer.Change(Timeout.Infinite, Timeout.Infinite);
                        _timer2.Change(Timeout.Infinite, Timeout.Infinite);
                        _timer.Dispose();
                        _timer2.Dispose();
                        _timer = null;
                        _timer2 = null;
                    }
                    if (value > 0)
                    {
                        _getCycle = value;
                    }
                    _timer = new Timer(MaintainTasks, null, 0, _getCycle);
                    _timer2 = new Timer(MaintainTasks2, null, _getCycle * 2, _getCycle * 2);  
                    //调试行，调试时请注释上面两行并取消下面一行的注释，每台设备只会执行一次数据获取。                
                    //MaintainTasks(null);
                }
            }
        }

        public MachineDataType MachineDataType
        {
            set
            {
                switch (value)
                {
                     case MachineDataType.Address:
                    {
                        GetDataType = MachineGetDataType.Address;
                        SetDataType=MachineSetDataType.Address;
                        break;
                    }
                    case MachineDataType.CommunicationTag:
                    {
                        GetDataType = MachineGetDataType.CommunicationTag;
                        SetDataType = MachineSetDataType.CommunicationTag;
                        break;
                    }
                }
            }
        }

        public MachineGetDataType GetDataType { get; set; }
        public MachineSetDataType SetDataType { get; set; }

        public int MaxRunningTasks
        {
            get { return _scheduler.MaximumConcurrencyLevel; }
            set
            {
                TaskStop();
                _scheduler = new LimitedConcurrencyLevelTaskScheduler(value);           
            }
        }

        /// <summary>
        /// 构造一个TaskManager
        /// </summary>
        /// <param name="maxRunningTask">同时可以运行的任务数</param>
        /// <param name="getCycle">读取数据的时间间隔（秒）</param>
        /// <param name="keepConnect">读取数据后是否保持连接</param>
        /// <param name="dataType">获取与设置数据的方式</param>
        public TaskManager(int maxRunningTask, int getCycle, bool keepConnect, MachineDataType dataType = MachineDataType.CommunicationTag)
        {
            _scheduler = new LimitedConcurrencyLevelTaskScheduler(maxRunningTask);
            _machines = new HashSet<BaseMachine>(new BaseMachineEqualityComparer());
            _unlinkedMachines = new HashSet<BaseMachine>(new BaseMachineEqualityComparer());
            _getCycle = getCycle;
            KeepConnect = keepConnect;
            MachineDataType = dataType;
        }

        /// <summary>
        /// 添加一台设备
        /// </summary>
        /// <param name="machine">设备</param>
        public void AddMachine(BaseMachine machine)
        {
            machine.KeepConnect = KeepConnect;
            lock (_machines)
            {
                _machines.Add(machine);
            }
        }

        /// <summary>
        /// 添加多台设备
        /// </summary>
        /// <param name="machines">设备的列表</param>
        public void AddMachines(IEnumerable<BaseMachine> machines)
        {
            lock (_machines)
            {
                foreach (var machine in machines)
                {
                    AddMachine(machine);
                }
            }
        }

        /// <summary>
        /// 根据设备的连接地址移除设备
        /// </summary>
        /// <param name="machineToken">设备的连接地址</param>
        public void RemoveMachineWithToken(string machineToken)
        {
            lock (_machines)
            {
                _machines.RemoveWhere(p => p.ConnectionToken == machineToken);
            }
            lock (_unlinkedMachines)
            {
                _unlinkedMachines.RemoveWhere(p => p.ConnectionToken == machineToken);
            }
        }

        /// <summary>
        /// 根据设备的id移除设备
        /// </summary>
        /// <param name="id">设备的id</param>
        public void RemoveMachineWithId(string id)
        {
            lock (_machines)
            {
                _machines.RemoveWhere(p => p.Id == id);
            }
            lock (_unlinkedMachines)
            {
                _unlinkedMachines.RemoveWhere(p => p.Id == id);
            }
        }

        /// <summary>
        /// 将设备指定为未连接
        /// </summary>
        /// <param name="id">设备的id</param>
        public void MoveMachineToUnlinked(string id)
        {
            IEnumerable<BaseMachine> machines;
            lock(_machines)
            {
                machines = _machines.Where(c => c.Id == id).ToList();
                if (!machines.Any()) return;
                _machines.RemoveWhere(p => p.Id == id);
            }
            lock(_unlinkedMachines)
            {
                foreach(var machine in machines)
                {
                    _unlinkedMachines.Add(machine);
                }
            }
        }

        /// <summary>
        /// 将设备指定为已连接
        /// </summary>
        /// <param name="id">设备的id</param>
        public void MoveMachineToLinked(string id)
        {
            IEnumerable<BaseMachine> machines;
            lock (_unlinkedMachines)
            {
                machines = _unlinkedMachines.Where(c => c.Id == id).ToList();
                if (!machines.Any()) return;
                _unlinkedMachines.RemoveWhere(p => p.Id == id);
            }
            lock (_machines)
            {
                foreach (var machine in machines)
                {
                    _machines.Add(machine);
                }
            }
        }

        /// <summary>
        /// 移除设备
        /// </summary>
        /// <param name="machine">设备的实例</param>
        public void RemoveMachine(BaseMachine machine)
        {
            lock (_machines)
            {
                _machines.Remove(machine);
            }
            lock (_unlinkedMachines)
            {
                _unlinkedMachines.Remove(machine);
            }
        }

        /// <summary>
        /// 已连接设备更新
        /// </summary>
        /// <param name="sender"></param>
        private void MaintainTasks(object sender)
        {
            AsyncHelper.RunSync(MaintainTasksAsync);
        }

        /// <summary>
        /// 未连接设备更新
        /// </summary>
        /// <param name="sender"></param>
        private void MaintainTasks2(object sender)
        {
            AsyncHelper.RunSync(MaintainTasks2Async);
        }

        /// <summary>
        /// 已连接设备更新
        /// </summary>
        /// <returns></returns>
        private async Task MaintainTasksAsync()
        {
            try
            {
                var tasks = new List<Task>();
                HashSet<BaseMachine> saveMachines = new HashSet<BaseMachine>();
                IEnumerable<BaseMachine> saveMachinesEnum;
                lock (_machines)
                {
                    saveMachines.UnionWith(_machines);
                    saveMachinesEnum = saveMachines.ToList();
                }
                foreach (var machine in saveMachinesEnum)
                {
                    CancellationTokenSource cts = new CancellationTokenSource();
                    cts.CancelAfter(TimeSpan.FromSeconds(_getCycle * 10));
                    var task = _tasks.StartNew(() => RunTask(machine).WithCancellation(cts.Token));
                    tasks.Add(task);
                }
                await Task.WhenAll(tasks);
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// 未连接设备更新
        /// </summary>
        /// <returns></returns>
        private async Task MaintainTasks2Async()
        {
            try
            {
                var tasks = new List<Task>();
                HashSet<BaseMachine> saveMachines = new HashSet<BaseMachine>();
                lock (_unlinkedMachines)
                {
                    saveMachines.UnionWith(_unlinkedMachines);
                }
                foreach (var machine in saveMachines)
                {
                    CancellationTokenSource cts = new CancellationTokenSource();
                    cts.CancelAfter(TimeSpan.FromSeconds(_getCycle * 10));
                    var task = _tasks.StartNew(() => RunTask(machine).WithCancellation(cts.Token));
                    tasks.Add(task);
                }
                await Task.WhenAll(tasks);
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="connectionToken">设备的连接标识</param>
        /// <param name="values">需要设置的数据</param>
        /// <returns>是否设置成功</returns>
        public async Task<bool> SetDatasAsync(string connectionToken,
            Dictionary<string, double> values)
        {
            BaseMachine machine = null;
            lock (_machines)
            {
                machine = _machines.FirstOrDefault(p => p.ConnectionToken == connectionToken);
            }
            if (machine == null) return false;
            return await machine.SetDatasAsync(SetDataType, values);
        }

        /// <summary>
        /// 启动TaskManager
        /// </summary>
        public void TaskStart()
        {
            TaskStop();
            _cts = new CancellationTokenSource();
            _tasks = new TaskFactory(_cts.Token, TaskCreationOptions.None, TaskContinuationOptions.None, _scheduler);
            GetCycle = TimeRestore.Restore;
        }

        /// <summary>
        /// 停止TaskManager
        /// </summary>
        public void TaskStop()
        {
            lock (_machines)
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
                        machine.Disconnect();
                    }
                }
                _tasks = null;
            }
        }

        /// <summary>
        /// 执行对具体设备的数据更新
        /// </summary>
        /// <param name="machine">设备的实例</param>
        /// <returns></returns>
        private async Task RunTask(BaseMachine machine)
        {
            try
            {
                //调试代码，调试时取消下面一下代码的注释，会同步调用获取数据。
                //var ans = machine.GetDatas();
                //设置Cancellation Token
                CancellationTokenSource cts = new CancellationTokenSource();
                //超时后取消任务
                cts.CancelAfter(TimeSpan.FromSeconds(_getCycle));
                //读取数据
                var ans = await machine.GetDatasAsync(GetDataType).WithCancellation(cts.Token);
                if (!machine.IsConnected)
                {
                    MoveMachineToUnlinked(machine.Id);
                }
                else
                {
                    MoveMachineToLinked(machine.Id);
                }
                ReturnValues?.Invoke(new TaskReturnDef()
                {
                    MachineId = machine.Id,
                    ReturnValues = ans
                });
            }
            catch (Exception e)
            {
                if (!machine.IsConnected)
                {
                    MoveMachineToUnlinked(machine.Id);
                }
                ReturnValues?.Invoke(new TaskReturnDef()
                {
                    MachineId = machine.Id,
                    ReturnValues = null
                });
            }
        }
    }
}