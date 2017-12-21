/*
 * LimitedConcurrencyLevelTaskScheduler类来自于MSDN官方样例，Modbus.Net的作者不保留对这个类的版权。
 * LimitedConcurrencyLevelTaskScheduler class comes from offical samples of MSDN, the author of "Modbus.Net" "donnot" obtain the copyright of LimitedConcurrencyLevelTaskScheduler(only).
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Modbus.Net
{
    /// <summary>
    ///     返回结果的定义类
    /// </summary>
    public class DataReturnDef : DataReturnDef<string>
    {
    }

    /// <summary>
    ///     返回结果的定义类
    /// </summary>
    public class DataReturnDef<TMachineKey> where TMachineKey : IEquatable<TMachineKey>
    {
        /// <summary>
        ///     设备的Id
        /// </summary>
        public TMachineKey MachineId { get; set; }

        /// <summary>
        ///     返回的数据值
        /// </summary>
        public Dictionary<string, ReturnUnit> ReturnValues { get; set; }
    }

    /// <summary>
    ///     Limited concurrency level task scheduler
    /// </summary>
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
            lock (_tasks)
            {
                return _tasks.Remove(task);
            }
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
    ///     具有任务管理的设备
    /// </summary>
    /// <typeparam name="TMachineKey">设备的Id类型</typeparam>
    public class TaskMachine<TMachineKey> where TMachineKey : IEquatable<TMachineKey>
    {
        /// <summary>
        ///     任务工厂
        /// </summary>
        private readonly TaskFactory _tasks;

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="machine">设备</param>
        /// <param name="taskFactory">任务工厂</param>
        public TaskMachine(IMachineProperty<TMachineKey> machine, TaskFactory taskFactory)
        {
            Machine = machine;
            _tasks = taskFactory;
            TasksWithTimer = new List<ITaskItem>();
        }

        /// <summary>
        ///     设备
        /// </summary>
        public IMachineProperty<TMachineKey> Machine { get; }

        /// <summary>
        ///     任务调度器
        /// </summary>
        public List<ITaskItem> TasksWithTimer { get; }

        /// <summary>
        ///     定时方式启动任务
        /// </summary>
        /// <typeparam name="TInterType">任务返回值的类型</typeparam>
        /// <param name="task">任务</param>
        /// <returns>任务是否执行成功</returns>
        public bool InvokeTimer<TInterType>(TaskItem<TInterType> task)
        {
            task.DetectConnected = () => Machine.IsConnected;
            task.GetMachine = () => Machine;
            task.GetTaskFactory = () => _tasks;

            if (!TasksWithTimer.Exists(taskCon => taskCon.Name == task.Name))
            {
                TasksWithTimer.Add(task);
                task.StartTimer();
                return true;
            }
            return false;
        }

        /// <summary>
        ///     停止任务
        /// </summary>
        /// <param name="taskItemName">任务的名称</param>
        /// <returns>是否停止成功</returns>
        public bool StopTimer(string taskItemName)
        {
            if (TasksWithTimer.Exists(taskCon => taskCon.Name == taskItemName))
            {
                var task = TasksWithTimer.FirstOrDefault(taskCon => taskCon.Name == taskItemName);
                task?.StopTimer();
                TasksWithTimer.Remove(task);
                return true;
            }
            return false;
        }

        /// <summary>
        ///     停止所有任务
        /// </summary>
        /// <returns>是否停止成功</returns>
        public bool StopAllTimers()
        {
            var ans = true;
            foreach (var task in TasksWithTimer)
                ans = ans && StopTimer(task.Name);
            return ans;
        }

        /// <summary>
        ///     暂时任务
        /// </summary>
        /// <param name="taskItemName">任务的名称</param>
        /// <returns>是否暂停成功</returns>
        public bool PauseTimer(string taskItemName)
        {
            if (TasksWithTimer.Exists(taskCon => taskCon.Name == taskItemName))
            {
                var task = TasksWithTimer.FirstOrDefault(taskCon => taskCon.Name == taskItemName);
                task?.StopTimer();
                return true;
            }
            return false;
        }

        /// <summary>
        ///     暂停所有任务
        /// </summary>
        /// <returns>是否暂停成功</returns>
        public bool PauseAllTimers()
        {
            var ans = true;
            foreach (var task in TasksWithTimer)
                ans = ans && PauseTimer(task.Name);
            return ans;
        }

        /// <summary>
        ///     恢复任务
        /// </summary>
        /// <param name="taskItemName">任务的名称</param>
        /// <returns>是否恢复任务</returns>
        public bool ContinueTimer(string taskItemName)
        {
            if (TasksWithTimer.Exists(taskCon => taskCon.Name == taskItemName))
            {
                var task = TasksWithTimer.FirstOrDefault(taskCon => taskCon.Name == taskItemName);
                task?.StartTimer();
                return true;
            }
            return false;
        }

        /// <summary>
        ///     恢复所有任务
        /// </summary>
        /// <returns>是否恢复成功</returns>
        public bool ContinueAllTimers()
        {
            var ans = true;
            foreach (var task in TasksWithTimer)
                ans = ans && ContinueTimer(task.Name);
            return ans;
        }

        /// <summary>
        ///     执行任务一次
        /// </summary>
        /// <typeparam name="TInterType">任务返回值的类型</typeparam>
        /// <param name="task">任务</param>
        /// <returns>任务是否执行成功</returns>
        public async Task<bool> InvokeOnce<TInterType>(TaskItem<TInterType> task)
        {
            var ans = await task.Invoke(Machine, _tasks, task.Params?.Invoke(), task.TimeoutTime);
            task.Return?.Invoke(ans);
            return true;
        }
    }

    internal class TaskMachineEqualityComparer<TKey> : IEqualityComparer<TaskMachine<TKey>>
        where TKey : IEquatable<TKey>
    {
        public bool Equals(TaskMachine<TKey> x, TaskMachine<TKey> y)
        {
            return x.Machine.Id.Equals(y.Machine.Id);
        }

        public int GetHashCode(TaskMachine<TKey> obj)
        {
            return obj.GetHashCode();
        }
    }

    /// <summary>
    ///     任务的接口
    /// </summary>
    public interface ITaskItem
    {
        /// <summary>
        ///     任务的名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        ///     启动计时器
        /// </summary>
        /// <returns></returns>
        bool StartTimer();

        /// <summary>
        ///     停止计时器
        /// </summary>
        /// <returns></returns>
        bool StopTimer();
    }

    /// <summary>
    ///     获取数据的预定义任务
    /// </summary>
    public class TaskItemGetData : TaskItem<DataReturnDef>
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="returnFunc">返回值的处理函数</param>
        /// <param name="getDataType">返回值的键类型</param>
        /// <param name="timeout">任务的超时时间</param>
        public TaskItemGetData(Action<DataReturnDef> returnFunc, MachineGetDataType getDataType, int timeout = 100000)
        {
            Name = "GetDatas";
            TimeoutTime = timeout;
            Invoke = async (machine, tasks, parameters, timeoutTime) =>
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromMilliseconds(timeoutTime));
                var ans =
                    await tasks.StartNew(
                        async () => await machine.GetMachineMethods<IMachineMethodData>()
                            .GetDatasAsync(
                            getDataType).WithCancellation(cts.Token)).Unwrap();
                return new DataReturnDef
                {
                    MachineId = machine.GetMachineIdString(),
                    ReturnValues = ans
                };
            };
            Params = null;
            Return = returnFunc;
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="returnFunc">返回值的处理函数</param>
        /// <param name="getDataType">返回值的键类型</param>
        /// <param name="getCycle">循环间隔(毫秒)</param>
        /// <param name="sleepCycle">设备离线时的循环间隔(毫秒)</param>
        /// <param name="timeout">任务的超时时间</param>
        public TaskItemGetData(Action<DataReturnDef> returnFunc, MachineGetDataType getDataType, int getCycle,
            int sleepCycle, int timeout = 100000) : this(returnFunc, getDataType, timeout)
        {
            TimerDisconnectedTime = sleepCycle;
            TimerTime = getCycle;
        }
    }

    /// <summary>
    ///     写入数据的预定义任务
    /// </summary>
    public class TaskItemSetData : TaskItem<bool>
    {
        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="values">写入的值</param>
        /// <param name="setDataType">写入值的键类型</param>
        /// <param name="returnFunc">返回值的处理函数</param>
        /// <param name="timeout">任务的超时时间</param>
        public TaskItemSetData(Func<Dictionary<string, double>> values, MachineSetDataType setDataType, int timeout = 100000, Action<bool> returnFunc = null)
        {
            Name = "SetDatas";
            TimeoutTime = timeout;
            Invoke = async (machine, tasks, parameters, timeoutTime) =>
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromMilliseconds(timeoutTime));
                var ans =
                    await tasks.StartNew(
                        async () => await machine.GetMachineMethods<IMachineMethodData>().
                            SetDatasAsync(setDataType, (Dictionary<string, double>)parameters[0]
                            ).WithCancellation(cts.Token)).Unwrap();
                return ans;
            };
            Params = () => new object[] {values()};
            Return = returnFunc;
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="values">写入的值</param>
        /// <param name="setDataType">写入值的键类型</param>
        /// <param name="returnFunc">返回值的处理函数</param>
        /// <param name="getCycle">循环间隔(毫秒)</param>
        /// <param name="sleepCycle">设备离线时的循环间隔(毫秒)</param>
        /// <param name="timeout">任务的超时时间</param>
        public TaskItemSetData(Func<Dictionary<string, double>> values, MachineSetDataType setDataType, int getCycle,
            int sleepCycle, int timeout = 100000, Action<bool> returnFunc = null)
            : this(values, setDataType, timeout, returnFunc)
        {
            TimerDisconnectedTime = sleepCycle;
            TimerTime = getCycle;
        }
    }

    /// <summary>
    ///     任务
    /// </summary>
    /// <typeparam name="TInterType">任务返回值的类型</typeparam>
    public class TaskItem<TInterType> : ITaskItem, IEquatable<TaskItem<TInterType>>
    {
        /// <summary>
        ///     定时器
        /// </summary>
        private Timer Timer { get; set; }

        /// <summary>
        ///     定时器的时间
        /// </summary>
        public int TimerTime { get; set; }

        /// <summary>
        ///     超时时间
        /// </summary>
        public int TimeoutTime { get; set; } = 100000;

        /// <summary>
        ///     离线定时器
        /// </summary>
        private Timer TimerDisconnected { get; set; }

        /// <summary>
        ///     离线定时器的时间
        /// </summary>
        public int TimerDisconnectedTime { get; set; }

        /// <summary>
        ///     执行的任务
        /// </summary>
        public Func<IMachinePropertyWithoutKey, TaskFactory, object[], int, Task<TInterType>> Invoke { get; set; }

        /// <summary>
        ///     检测设备的在线状态
        /// </summary>
        internal Func<bool> DetectConnected { get; set; }

        /// <summary>
        ///     任务执行的参数
        /// </summary>
        public Func<object[]> Params { get; set; }

        /// <summary>
        ///     返回值的处理函数
        /// </summary>
        public Action<TInterType> Return { get; set; }

        /// <summary>
        ///     获取设备
        /// </summary>
        internal Func<IMachinePropertyWithoutKey> GetMachine { get; set; }

        /// <summary>
        ///     获取任务工厂
        /// </summary>
        internal Func<TaskFactory> GetTaskFactory { get; set; }

        /// <summary>
        ///     是否相等
        /// </summary>
        /// <param name="other">另一个实例</param>
        /// <returns>是否相等</returns>
        public bool Equals(TaskItem<TInterType> other)
        {
            return Name == other?.Name;
        }

        /// <summary>
        ///     名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     启动定时器
        /// </summary>
        /// <returns>是否成功</returns>
        public bool StartTimer()
        {
            ActivateTimerDisconnected();
            return true;
        }

        /// <summary>
        ///     停止定时器
        /// </summary>
        /// <returns></returns>
        public bool StopTimer()
        {
            DeactivateTimer();
            DeactivateTimerDisconnected();
            return true;
        }

        /// <summary>
        ///     激活定时器
        /// </summary>
        private void ActivateTimer()
        {
            Timer = new Timer(async state =>
            {
                if (!DetectConnected()) TimerChangeToDisconnect();
                var ans = await Invoke(GetMachine(), GetTaskFactory(), Params?.Invoke(), TimeoutTime);
                Return?.Invoke(ans);
            }, null, 0, TimerTime);
        }

        /// <summary>
        ///     反激活定时器
        /// </summary>
        private void DeactivateTimer()
        {
            Timer?.Dispose();
            Timer = null;
        }

        /// <summary>
        ///     激活离线定时器
        /// </summary>
        private void ActivateTimerDisconnected()
        {
            TimerDisconnected = new Timer(async state =>
            {
                await GetMachine().ConnectAsync();
                if (DetectConnected()) TimerChangeToConnect();
            }, null, 0, TimerDisconnectedTime);
        }

        /// <summary>
        ///     反激活离线定时器
        /// </summary>
        private void DeactivateTimerDisconnected()
        {
            TimerDisconnected?.Dispose();
            TimerDisconnected = null;
        }

        /// <summary>
        ///     将定时器切换至在线
        /// </summary>
        /// <returns></returns>
        private bool TimerChangeToConnect()
        {
            DeactivateTimerDisconnected();
            ActivateTimer();
            return true;
        }

        /// <summary>
        ///     将定时器切换至离线
        /// </summary>
        /// <returns></returns>
        private bool TimerChangeToDisconnect()
        {
            DeactivateTimer();
            ActivateTimerDisconnected();
            return true;
        }
        
        /// <summary>
        ///     拷贝实例
        /// </summary>
        /// <returns>拷贝的实例</returns>
        public TaskItem<TInterType> Clone()
        {
            return MemberwiseClone() as TaskItem<TInterType>;
        }
    }

    /// <summary>
    ///     任务调度器
    /// </summary>
    public class TaskManager : TaskManager<string>
    {
        /// <summary>
        ///     构造一个TaskManager
        /// </summary>
        /// <param name="maxRunningTask">同时可以运行的任务数</param>
        /// <param name="keepConnect">读取数据后是否保持连接</param>
        public TaskManager(int maxRunningTask, bool keepConnect)
            : base(maxRunningTask, keepConnect)
        {
        }

        /// <summary>
        ///     添加一台设备
        /// </summary>
        /// <param name="machine">设备</param>
        public void AddMachine(BaseMachine machine)
        {
            base.AddMachine(machine);
        }

        /// <summary>
        ///     添加多台设备
        /// </summary>
        /// <param name="machines">多台设备</param>
        public void AddMachines(IEnumerable<BaseMachine> machines)
        {
            base.AddMachines(machines);
        }

        /// <summary>
        ///     通过Id获取设备
        /// </summary>
        /// <param name="id">设备Id</param>
        /// <returns>获取的设备</returns>
        public BaseMachine GetMachineById(string id)
        {
            return base.GetMachineById<string>(id) as BaseMachine;
        }

        /// <summary>
        ///     通过通讯标志获取设备
        /// </summary>
        /// <param name="connectionToken">通讯标志</param>
        /// <returns>获取的设备</returns>
        public BaseMachine GetMachineByConnectionToken(string connectionToken)
        {
            return base.GetMachineByConnectionToken<string>(connectionToken) as BaseMachine;
        }
    }

    /// <summary>
    ///     任务调度器
    /// </summary>
    /// <typeparam name="TMachineKey">设备Id的类型</typeparam>
    public class TaskManager<TMachineKey> where TMachineKey : IEquatable<TMachineKey>
    {
        /// <summary>
        ///     正在运行的设备
        /// </summary>
        private readonly HashSet<TaskMachine<TMachineKey>> _machines;

        /// <summary>
        ///     全局任务取消标志
        /// </summary>
        private CancellationTokenSource _cts;

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
        ///     构造一个TaskManager
        /// </summary>
        /// <param name="maxRunningTask">同时可以运行的任务数</param>
        /// <param name="keepConnect">读取数据后是否保持连接</param>
        public TaskManager(int maxRunningTask, bool keepConnect)
        {
            _scheduler = new LimitedConcurrencyLevelTaskScheduler(maxRunningTask);
            _machines =
                new HashSet<TaskMachine<TMachineKey>>(new TaskMachineEqualityComparer<TMachineKey>());
            KeepConnect = keepConnect;
            _cts = new CancellationTokenSource();
            _tasks = new TaskFactory(_cts.Token, TaskCreationOptions.None, TaskContinuationOptions.None, _scheduler);
        }

        /// <summary>
        ///     保持连接
        /// </summary>
        public bool KeepConnect
        {
            get { return _keepConnect; }
            set
            {
                PauseTimerAll();
                _keepConnect = value;
                lock (_machines)
                {
                    foreach (var machine in _machines)
                        machine.Machine.KeepConnect = _keepConnect;
                }
                ContinueTimerAll();
            }
        }

        /// <summary>
        ///     最大可执行任务数
        /// </summary>
        public int MaxRunningTasks
        {
            get { return _scheduler.MaximumConcurrencyLevel; }
            set
            {
                PauseTimerAll();
                _scheduler = new LimitedConcurrencyLevelTaskScheduler(value);
                ContinueTimerAll();
            }
        }

        /// <summary>
        ///     强制停止所有正在运行的任务
        /// </summary>
        public void TaskHalt()
        {
            _cts.Cancel();
            _cts = new CancellationTokenSource();
            _tasks = new TaskFactory(_cts.Token, TaskCreationOptions.None, TaskContinuationOptions.None, _scheduler);
        }

        /// <summary>
        ///     添加一台设备
        /// </summary>
        /// <param name="machine">设备</param>
        public void AddMachine<TUnitKey>(BaseMachine<TMachineKey, TUnitKey> machine)
            where TUnitKey : IEquatable<TUnitKey>
        {
            machine.KeepConnect = KeepConnect;
            lock (_machines)
            {
                _machines.Add(new TaskMachine<TMachineKey>(machine, _tasks));
            }
        }

        /// <summary>
        ///     添加多台设备
        /// </summary>
        /// <param name="machines">设备的列表</param>
        public void AddMachines<TUnitKey>(IEnumerable<BaseMachine<TMachineKey, TUnitKey>> machines)
            where TUnitKey : IEquatable<TUnitKey>
        {
            foreach (var machine in machines)
                AddMachine(machine);
        }

        /// <summary>
        ///     通过Id获取设备
        /// </summary>
        /// <typeparam name="TUnitKey">设备地址Id的类型</typeparam>
        /// <param name="id">设备的Id</param>
        /// <returns>获取设备</returns>
        public BaseMachine<TMachineKey, TUnitKey> GetMachineById<TUnitKey>(TMachineKey id)
            where TUnitKey : IEquatable<TUnitKey>
        {
            try
            {
                TaskMachine<TMachineKey> machine;
                lock (_machines)
                {
                    machine = _machines.SingleOrDefault(p => p.Machine.Id.Equals(id));
                }
                return machine?.Machine as BaseMachine<TMachineKey, TUnitKey>;
            }
            catch (Exception e)
            {
                Log.Error(e, $"Device {id} get error, maybe duplicated in taskmanager");
                return null;
            }
        }

        /// <summary>
        ///     通过通讯标志获取设备
        /// </summary>
        /// <typeparam name="TUnitKey">设备地址Id的类型</typeparam>
        /// <param name="connectionToken">通讯标志</param>
        /// <returns>获取的数据</returns>
        public BaseMachine<TMachineKey, TUnitKey> GetMachineByConnectionToken<TUnitKey>(string connectionToken)
            where TUnitKey : IEquatable<TUnitKey>
        {
            try
            {
                TaskMachine<TMachineKey> machine;
                lock (_machines)
                {
                    machine = _machines.SingleOrDefault(p => p.Machine.ConnectionToken == connectionToken && p.Machine.IsConnected);
                }
                return machine?.Machine as BaseMachine<TMachineKey, TUnitKey>;
            }
            catch (Exception e)
            {
                Log.Error(e, $"Device {connectionToken} get error, maybe duplicated in taskmanager");
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
                _machines.RemoveWhere(p => p.Machine.ConnectionToken == machineToken);
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
                _machines.RemoveWhere(p => p.Machine.Id.Equals(id));
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
                _machines.RemoveWhere(p => p.Machine.Equals(machine));
            }
        }

        /// <summary>
        ///     所有设备执行定时任务
        /// </summary>
        /// <typeparam name="TInterType">任务返回值的类型</typeparam>
        /// <param name="item">任务</param>
        /// <returns>所有任务是否执行成功</returns>
        public bool InvokeTimerAll<TInterType>(TaskItem<TInterType> item)
        {
            var ans = true;
            lock (_machines)
            {
                foreach (var machine in _machines)
                {
                    Thread.Sleep(10);
                    ans &= machine.InvokeTimer(item.Clone());
                }
            }
            return ans;
        }

        /// <summary>
        ///     所有设备停止执行所有任务
        /// </summary>
        /// <returns>所有任务是否停止成功</returns>
        public bool StopTimerAll()
        {
            var ans = true;
            lock (_machines)
            {
                foreach (var machine in _machines)
                {
                    Thread.Sleep(10);
                    ans &= machine.StopAllTimers();
                }
            }
            return ans;
        }

        /// <summary>
        ///     所有设备停止执行某一个任务
        /// </summary>
        /// <param name="taskItemName">任务名称</param>
        /// <returns>任务是否停止成功</returns>
        public bool StopTimerAll(string taskItemName)
        {
            var ans = true;
            lock (_machines)
            {
                foreach (var machine in _machines)
                    ans &= machine.StopTimer(taskItemName);
            }
            return ans;
        }

        /// <summary>
        ///     所有设备暂停执行所有任务
        /// </summary>
        /// <returns>任务是否暂停成功</returns>
        public bool PauseTimerAll()
        {
            var ans = true;
            lock (_machines)
            {
                foreach (var machine in _machines)
                    ans &= machine.PauseAllTimers();
            }
            return ans;
        }

        /// <summary>
        ///     所有设备暂停执行某一个任务
        /// </summary>
        /// <param name="taskItemName">任务的名称</param>
        /// <returns>任务是否暂停成功</returns>
        public bool PauseTimerAll(string taskItemName)
        {
            var ans = true;
            lock (_machines)
            {
                foreach (var machine in _machines)
                    ans &= machine.PauseTimer(taskItemName);
            }
            return ans;
        }

        /// <summary>
        ///     所有设备继续执行所有任务
        /// </summary>
        /// <returns>所有任务是否继续执行成功</returns>
        public bool ContinueTimerAll()
        {
            var ans = true;
            lock (_machines)
            {
                foreach (var machine in _machines)
                    ans &= machine.ContinueAllTimers();
            }
            return ans;
        }

        /// <summary>
        ///     所有设备继续执行某一个任务
        /// </summary>
        /// <param name="taskItemName">任务的名称</param>
        /// <returns>任务是否继续执行成功</returns>
        public bool ConinueTimerAll(string taskItemName)
        {
            lock (_machines)
            {
                foreach (var machine in _machines)
                    machine.ContinueTimer(taskItemName);
            }
            return true;
        }

        /// <summary>
        ///     所有设备执行一个一次性任务
        /// </summary>
        /// <typeparam name="TInterType">任务的返回值类型</typeparam>
        /// <param name="item">任务</param>
        /// <returns>任务是否执行成功</returns>
        public async Task<bool> InvokeOnceAll<TInterType>(TaskItem<TInterType> item)
        {
            var tasks = new List<Task<bool>>();
            lock (_machines)
            {
                foreach (var machine in _machines)
                    tasks.Add(_tasks.StartNew(async () => await machine.InvokeOnce(item.Clone())).Unwrap());
            }
            var ans = await Task.WhenAll(tasks);
            return ans.All(p => p);
        }

        /// <summary>
        ///     某个设备执行一个一次性任务
        /// </summary>
        /// <typeparam name="TInterType">任务的返回值类型</typeparam>
        /// <param name="machineId">设备的Id</param>
        /// <param name="item">任务</param>
        /// <returns>任务是否执行成功</returns>
        public async Task<bool> InvokeOnceForMachine<TInterType>(TMachineKey machineId, TaskItem<TInterType> item)
        {
            var machine = _machines.FirstOrDefault(p => p.Machine.Id.Equals(machineId));
            if (machine != null)
                return await machine.InvokeOnce(item.Clone());
            return false;
        }

        /// <summary>
        ///     某个设备执行一个定时任务
        /// </summary>
        /// <typeparam name="TInterType">任务的返回值类型</typeparam>
        /// <param name="machineId">设备的Id</param>
        /// <param name="item">任务</param>
        /// <returns>任务是否执行成功</returns>
        public bool InvokeTimerForMachine<TInterType>(TMachineKey machineId, TaskItem<TInterType> item)
        {
            var machine = _machines.FirstOrDefault(p => p.Machine.Id.Equals(machineId));
            if (machine != null)
                return machine.InvokeTimer(item.Clone());
            return false;
        }

        /// <summary>
        ///     某个设备停止一个定时任务
        /// </summary>
        /// <param name="machineId">任务的Id</param>
        /// <param name="taskItemName">任务的名称</param>
        /// <returns>任务是否停止成功</returns>
        public bool StopTimerForMachine(TMachineKey machineId, string taskItemName)
        {
            var machine = _machines.FirstOrDefault(p => p.Machine.Id.Equals(machineId));
            if (machine != null)
                return machine.StopTimer(taskItemName);
            return false;
        }

        /// <summary>
        ///     某个设备暂停一个定时任务
        /// </summary>
        /// <param name="machineId">任务的Id</param>
        /// <param name="taskItemName">任务的名称</param>
        /// <returns>任务是否暂停成功</returns>
        public bool PauseTimerForMachine(TMachineKey machineId, string taskItemName)
        {
            var machine = _machines.FirstOrDefault(p => p.Machine.Id.Equals(machineId));
            if (machine != null)
                return machine.PauseTimer(taskItemName);
            return false;
        }

        /// <summary>
        ///     某个设备继续进行一个定时任务
        /// </summary>
        /// <param name="machineId">任务的Id</param>
        /// <param name="taskItemName">任务的名称</param>
        /// <returns>任务是否继续运行成功</returns>
        public bool ContinueTimerForMachine(TMachineKey machineId, string taskItemName)
        {
            var machine = _machines.FirstOrDefault(p => p.Machine.Id.Equals(machineId));
            if (machine != null)
                return machine.ContinueTimer(taskItemName);
            return false;
        }
    }
}