/*
 * LimitedConcurrencyLevelTaskScheduler类来自于MSDN官方样例，Modbus.Net的作者不保留对这个类的版权。
 * LimitedConcurrencyLevelTaskScheduler class comes from offical samples of MSDN, the author of "Modbus.Net" "donnot" obtain the copyright of LimitedConcurrencyLevelTaskScheduler(only).
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     返回结果的定义类
    /// </summary>
    public class TaskReturnDef : TaskReturnDef<string>
    {
        
    }

    /// <summary>
    ///     返回结果的定义类
    /// </summary>
    public class TaskReturnDef<TMachineKey> where TMachineKey : IEquatable<TMachineKey>
    {
        public TMachineKey MachineId { get; set; }
        public Dictionary<string, ReturnUnit> ReturnValues { get; set; }
    }

    /// <summary>
    ///     时间定义
    /// </summary>
    public static class TimeRestore
    {
        public static int Restore = 0;
    }

    public class LimitedConcurrencyLevelTaskScheduler : TaskScheduler
    {
        /// <summary>
        ///     Whether the current thread is processing work items.
        /// </summary>
        [ThreadStatic] private static bool _currentThreadIsProcessingItems;

        /// <summary>
        ///     The maximum concurrency level allowed by this scheduler.
        /// </summary>
        private readonly int _maxDegreeOfParallelism;

        /// <summary>
        ///     The list of tasks to be executed.
        /// </summary>
        private readonly LinkedList<Task> _tasks = new LinkedList<Task>(); // protected by lock(_tasks)

        /// <summary>
        ///     Whether the scheduler is currently processing work items.
        /// </summary>
        private int _delegatesQueuedOrRunning; // protected by lock(_tasks)

        /// <summary>
        ///     Initializes an instance of the LimitedConcurrencyLevelTaskScheduler class with the
        ///     specified degree of parallelism.
        /// </summary>
        /// <param name="maxDegreeOfParallelism">
        ///     The maximum degree of parallelism provided by this scheduler.
        /// </param>
        public LimitedConcurrencyLevelTaskScheduler(int maxDegreeOfParallelism)
        {
            if (maxDegreeOfParallelism < 1) throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
        }

        /// <summary>
        ///     Gets the maximum concurrency level supported by this scheduler.
        /// </summary>
        public sealed override int MaximumConcurrencyLevel
        {
            get { return _maxDegreeOfParallelism; }
        }

        /// <summary>
        ///     Queues a task to the scheduler.
        /// </summary>
        /// <param name="task">
        ///     The task to be queued.
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
        ///     Informs the ThreadPool that there's work to be executed for this scheduler.
        /// </summary>
        private void NotifyThreadPoolOfPendingWork()
        {
#if NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462
            ThreadPool.UnsafeQueueUserWorkItem(_ =>
#else
            ThreadPool.QueueUserWorkItem(_ =>
#endif
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
                        TryExecuteTask(item);
                    }
                }
                    // We're done processing items on the current thread
                finally
                {
                    _currentThreadIsProcessingItems = false;
                }
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
            return TryExecuteTask(task);
        }

        /// <summary>
        ///     Attempts to remove a previously scheduled task from the scheduler.
        /// </summary>
        /// <param name="task">
        ///     The task to be removed.
        /// </param>
        /// <returns>
        ///     Whether the task could be found and removed.
        /// </returns>
        protected sealed override bool TryDequeue(Task task)
        {
            lock (_tasks) return _tasks.Remove(task);
        }

        /// <summary>
        ///     Gets an enumerable of the tasks currently scheduled on this scheduler.
        /// </summary>
        /// <returns>
        ///     An enumerable of the tasks currently scheduled.
        /// </returns>
        protected sealed override IEnumerable<Task> GetScheduledTasks()
        {
            var lockTaken = false;
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

    /// <summary>
    ///     任务调度器
    /// </summary>
    public class TaskManager : TaskManager<string>
    {
        public TaskManager(int maxRunningTask, int getCycle, bool keepConnect,
            MachineDataType dataType = MachineDataType.CommunicationTag)
            : base(maxRunningTask, getCycle, keepConnect, dataType)
        {
        }

        public new delegate void ReturnValuesDelegate(TaskReturnDef returnValue);

        public new event ReturnValuesDelegate ReturnValues;

        /// <summary>
        ///     执行对具体设备的数据更新
        /// </summary>
        /// <param name="machine">设备的实例</param>
        /// <returns></returns>
        protected override async Task RunTask(IMachineProperty<string> machine)
        {
            try
            {
                var ans = await GetValue(machine);
                ReturnValues?.Invoke(new TaskReturnDef
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
                ReturnValues?.Invoke(new TaskReturnDef
                {
                    MachineId = machine.Id,
                    ReturnValues = null
                });
            }
        }

        public void AddMachine(BaseMachine machine)
        {
            base.AddMachine(machine);
        }

        public void AddMachines(IEnumerable<BaseMachine> machines)
        {
            base.AddMachines(machines);
        }

        public BaseMachine GetMachineById(string id)
        {
            return base.GetMachineById<string>(id) as BaseMachine;
        }

        public BaseMachine GetMachineByConnectionToken(string connectionToken)
        {
            return base.GetMachineByConnectionToken<string>(connectionToken) as BaseMachine;
        }
    }

    /// <summary>
    ///     任务调度器
    /// </summary>
    /// <typeparam name="TMachineKey"></typeparam>
    public class TaskManager<TMachineKey> where TMachineKey : IEquatable<TMachineKey>
    {
        /// <summary>
        ///     返回数据代理
        /// </summary>
        /// <param name="returnValue"></param>
        public delegate void ReturnValuesDelegate(TaskReturnDef<TMachineKey> returnValue);

        /// <summary>
        ///     正在运行的设备
        /// </summary>
        private readonly HashSet<IMachineProperty<TMachineKey>> _machines;

        /// <summary>
        ///     不在运行的设备
        /// </summary>
        private readonly HashSet<IMachineProperty<TMachineKey>> _unlinkedMachines;

        private CancellationTokenSource _cts;

        /// <summary>
        ///     获取间隔
        /// </summary>
        private int _getCycle;

        /// <summary>
        ///     保持连接
        /// </summary>
        private bool _keepConnect;

        /// <summary>
        ///     任务调度
        /// </summary>
        private TaskScheduler _scheduler;

        /// <summary>
        ///     任务工厂
        /// </summary>
        private TaskFactory _tasks;

        /// <summary>
        ///     正常读取的计时器
        /// </summary>
        private Timer _timer;

        /// <summary>
        ///     重连计时器
        /// </summary>
        private Timer _timer2;

        /// <summary>
        ///     构造一个TaskManager
        /// </summary>
        /// <param name="maxRunningTask">同时可以运行的任务数</param>
        /// <param name="getCycle">读取数据的时间间隔（毫秒）</param>
        /// <param name="keepConnect">读取数据后是否保持连接</param>
        /// <param name="dataType">获取与设置数据的方式</param>
        public TaskManager(int maxRunningTask, int getCycle, bool keepConnect,
            MachineDataType dataType = MachineDataType.CommunicationTag)
        {
            _scheduler = new LimitedConcurrencyLevelTaskScheduler(maxRunningTask);
            _machines =
                new HashSet<IMachineProperty<TMachineKey>>(new BaseMachineEqualityComparer<TMachineKey>());
            _unlinkedMachines =
                new HashSet<IMachineProperty<TMachineKey>>(new BaseMachineEqualityComparer<TMachineKey>());
            _getCycle = getCycle;
            KeepConnect = keepConnect;
            MachineDataType = dataType;
        }

        /// <summary>
        ///     保持连接
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
        ///     获取间隔，毫秒
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
                    _timer2 = new Timer(MaintainTasks2, null, _getCycle*2, _getCycle*2);
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
                        SetDataType = MachineSetDataType.Address;
                        break;
                    }
                    case MachineDataType.CommunicationTag:
                    {
                        GetDataType = MachineGetDataType.CommunicationTag;
                        SetDataType = MachineSetDataType.CommunicationTag;
                        break;
                    }
                    case MachineDataType.Name:
                    {
                        GetDataType = MachineGetDataType.Name;
                        SetDataType = MachineSetDataType.Name;
                        break;
                    }
                    case MachineDataType.Id:
                    {
                        GetDataType = MachineGetDataType.Id;
                        SetDataType = MachineSetDataType.Id;
                        break;
                    }
                }
            }
        }

        public MachineGetDataType GetDataType { get; set; }
        public MachineSetDataType SetDataType { get; set; }

        /// <summary>
        ///     最大可执行任务数
        /// </summary>
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
        ///     返回数据事件
        /// </summary>
        public event ReturnValuesDelegate ReturnValues;

        /// <summary>
        ///     添加一台设备
        /// </summary>
        /// <param name="machine">设备</param>
        public void AddMachine<TUnitKey>(BaseMachine<TMachineKey, TUnitKey> machine) where TUnitKey : IEquatable<TUnitKey>
        {
            machine.KeepConnect = KeepConnect;
            lock (_machines)
            {
                _machines.Add(machine);
            }
        }

        /// <summary>
        ///     添加多台设备
        /// </summary>
        /// <param name="machines">设备的列表</param>
        public void AddMachines<TUnitKey>(IEnumerable<BaseMachine<TMachineKey, TUnitKey>> machines) where TUnitKey : IEquatable<TUnitKey>
        {
            lock (_machines)
            {
                foreach (var machine in machines)
                {
                    AddMachine(machine);
                }
            }
        }

        public BaseMachine<TMachineKey, TUnitKey> GetMachineById<TUnitKey>(TMachineKey id)
            where TUnitKey : IEquatable<TUnitKey>
        {
            try
            {
                IMachineProperty<TMachineKey> machine;
                lock (_machines)
                {
                    machine = _machines.FirstOrDefault(p => p.Id.Equals(id));
                    if (machine == null)
                    {
                        lock (_unlinkedMachines)
                        {
                            machine = _unlinkedMachines.FirstOrDefault(p => p.Id.Equals(id));
                        }
                    }
                }
                return machine as BaseMachine<TMachineKey, TUnitKey>;
            }
            catch (Exception e)
            {
                Console.WriteLine($"设备返回错误 {e.Message}");
                return null;
            }
        }

        public BaseMachine<TMachineKey, TUnitKey> GetMachineByConnectionToken<TUnitKey>(string connectionToken)
            where TUnitKey : IEquatable<TUnitKey>
        {
            try
            {
                IMachineProperty<TMachineKey> machine;
                lock (_machines)
                {
                    machine = _machines.FirstOrDefault(p => p.ConnectionToken == connectionToken);
                    if (machine == null)
                    {
                        lock (_unlinkedMachines)
                        {
                            machine = _unlinkedMachines.FirstOrDefault(p => p.ConnectionToken == connectionToken);
                        }
                    }
                }
                return machine as BaseMachine<TMachineKey, TUnitKey>;
            }
            catch (Exception e)
            {
                Console.WriteLine($"设备返回错误 {e.Message}");
                return null;
            }
        }

        /// <summary>
        ///     根据设备的连接地址移除设备
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
        ///     根据设备的id移除设备
        /// </summary>
        /// <param name="id">设备的id</param>
        public void RemoveMachineWithId(TMachineKey id)
        {
            lock (_machines)
            {
                _machines.RemoveWhere(p => p.Id.Equals(id));
            }
            lock (_unlinkedMachines)
            {
                _unlinkedMachines.RemoveWhere(p => p.Id.Equals(id));
            }
        }

        /// <summary>
        ///     将设备指定为未连接
        /// </summary>
        /// <param name="id">设备的id</param>
        public void MoveMachineToUnlinked(TMachineKey id)
        {
            IEnumerable<IMachineProperty<TMachineKey>> machines;
            lock (_machines)
            {
                machines = _machines.Where(c => c.Id.Equals(id)).ToList();
                if (!machines.Any()) return;
                _machines.RemoveWhere(p => p.Id.Equals(id));
            }
            lock (_unlinkedMachines)
            {
                foreach (var machine in machines)
                {
                    _unlinkedMachines.Add(machine);
                }
            }
        }

        /// <summary>
        ///     将设备指定为已连接
        /// </summary>
        /// <param name="id">设备的id</param>
        public void MoveMachineToLinked(TMachineKey id)
        {
            IEnumerable<IMachineProperty<TMachineKey>> machines;
            lock (_unlinkedMachines)
            {
                machines = _unlinkedMachines.Where(c => c.Id.Equals(id)).ToList();
                if (!machines.Any()) return;
                _unlinkedMachines.RemoveWhere(p => p.Id.Equals(id));
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
        ///     移除设备
        /// </summary>
        /// <param name="machine">设备的实例</param>
        public void RemoveMachine(IMachineProperty<TMachineKey> machine)
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
        ///     已连接设备更新
        /// </summary>
        /// <param name="sender"></param>
        private void MaintainTasks(object sender)
        {
            AsyncHelper.RunSync(MaintainTasksAsync);
        }

        /// <summary>
        ///     未连接设备更新
        /// </summary>
        /// <param name="sender"></param>
        private void MaintainTasks2(object sender)
        {
            AsyncHelper.RunSync(MaintainTasks2Async);
        }

        /// <summary>
        ///     已连接设备更新
        /// </summary>
        /// <returns></returns>
        private async Task MaintainTasksAsync()
        {
            try
            {
                var tasks = new List<Task>();
                var saveMachines = new HashSet<IMachineProperty<TMachineKey>>();
                IEnumerable<IMachineProperty<TMachineKey>> saveMachinesEnum;
                lock (_machines)
                {
                    saveMachines.UnionWith(_machines);
                    saveMachinesEnum = saveMachines.ToList();
                }
                foreach (var machine in saveMachinesEnum)
                {
                    var cts = new CancellationTokenSource();
                    cts.CancelAfter(TimeSpan.FromSeconds(_getCycle*10));
                    var task = _tasks.StartNew(() => RunTask(machine).WithCancellation(cts.Token));
                    tasks.Add(task);
                }
                await Task.WhenAll(tasks);
            }
            catch (Exception)
            {
                //ignore
            }
        }

        /// <summary>
        ///     未连接设备更新
        /// </summary>
        /// <returns></returns>
        private async Task MaintainTasks2Async()
        {
            try
            {
                var tasks = new List<Task>();
                var saveMachines = new HashSet<IMachineProperty<TMachineKey>>();
                lock (_unlinkedMachines)
                {
                    saveMachines.UnionWith(_unlinkedMachines);
                }
                foreach (var machine in saveMachines)
                {
                    var cts = new CancellationTokenSource();
                    cts.CancelAfter(TimeSpan.FromSeconds(_getCycle*10));
                    var task = _tasks.StartNew(() => RunTask(machine).WithCancellation(cts.Token));
                    tasks.Add(task);
                }
                await Task.WhenAll(tasks);
            }
            catch (Exception)
            {
                //ignore
            }
        }

        /// <summary>
        ///     设置数据
        /// </summary>
        /// <param name="connectionToken">设备的连接标识</param>
        /// <param name="values">需要设置的数据</param>
        /// <returns>是否设置成功</returns>
        public async Task<bool> SetDatasAsync(string connectionToken,
            Dictionary<string, double> values)
        {
            IMachineProperty<TMachineKey> machine = null;
            lock (_machines)
            {
                machine = _machines.FirstOrDefault(p => p.ConnectionToken == connectionToken);
            }
            if (machine == null) return false;
            return await machine.InvokeMachineMethod<IMachineData, Task<bool>>("SetDatasAsync", SetDataType, values);
        }

        /// <summary>
        ///     启动TaskManager
        /// </summary>
        public void TaskStart()
        {
            TaskStop();
            _cts = new CancellationTokenSource();
            _tasks = new TaskFactory(_cts.Token, TaskCreationOptions.None, TaskContinuationOptions.None, _scheduler);
            GetCycle = TimeRestore.Restore;
        }

        /// <summary>
        ///     停止TaskManager
        /// </summary>
        public void TaskStop()
        {
            lock (_machines)
            {
                GetCycle = Timeout.Infinite;
                _cts?.Cancel();
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
        ///     执行对具体设备的数据更新
        /// </summary>
        /// <param name="machine">设备的实例</param>
        /// <returns></returns>
        protected virtual async Task RunTask(IMachineProperty<TMachineKey> machine)
        {
            try
            {
                var ans = await GetValue(machine);
                ReturnValues?.Invoke(new TaskReturnDef<TMachineKey>
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
                ReturnValues?.Invoke(new TaskReturnDef<TMachineKey>
                {
                    MachineId = machine.Id,
                    ReturnValues = null
                });
            }
        }

        protected async Task<Dictionary<string, ReturnUnit>> GetValue(IMachineProperty<TMachineKey> machine)
        {
            //调试代码，调试时取消下面一下代码的注释，会同步调用获取数据。
            //var ans = machine.GetDatas();
            //设置Cancellation Token
            var cts = new CancellationTokenSource();
            //超时后取消任务
            cts.CancelAfter(TimeSpan.FromSeconds(_getCycle));
            //读取数据
            var ans =
                await machine.InvokeMachineMethod<IMachineData, Task<Dictionary<string, ReturnUnit>>>("GetDatasAsync",
                    GetDataType).WithCancellation(cts.Token);
            if (!machine.IsConnected)
            {
                MoveMachineToUnlinked(machine.Id);
            }
            else
            {
                MoveMachineToLinked(machine.Id);
            }
            return ans;
        }
    }
}