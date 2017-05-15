/*
 * LimitedConcurrencyLevelTaskScheduler类来自于MSDN官方样例，Modbus.Net的作者不保留对这个类的版权。
 * LimitedConcurrencyLevelTaskScheduler class comes from offical samples of MSDN, the author of "Modbus.Net" "donnot" obtain the copyright of LimitedConcurrencyLevelTaskScheduler(only).
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

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
        public TMachineKey MachineId { get; set; }
        public Dictionary<string, ReturnUnit> ReturnValues { get; set; }
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
#if NET40 || NET45 || NET451 || NET452 || NET46 || NET461 || NET462 || NET47
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

    public class TaskMachine<TMachineKey> where TMachineKey : IEquatable<TMachineKey>
    {
        private TaskFactory _tasks { get; }

        public TaskMachine(IMachineProperty<TMachineKey> machine, TaskFactory taskFactory)
        {
            Machine = machine;
            _tasks = taskFactory;
        }

        public IMachineProperty<TMachineKey> Machine { get; }

        public List<ITaskItem> TasksWithTimer { get; set; }

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

        public bool StopAllTimers()
        {
            bool ans = true;
            foreach (var task in TasksWithTimer)
            {
                ans = ans && StopTimer(task.Name);
            }
            return ans;
        }

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

        public bool PauseAllTimers()
        {
            bool ans = true;
            foreach (var task in TasksWithTimer)
            {
                ans = ans && PauseTimer(task.Name);
            }
            return ans;
        }

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

        public bool ContinueAllTimers()
        {
            bool ans = true;
            foreach (var task in TasksWithTimer)
            {
                ans = ans && ContinueTimer(task.Name);
            }
            return ans;
        }

        public async Task<bool> InvokeOnce<TInterType>(TaskItem<TInterType> task)
        {
            if (Machine.IsConnected)
            {
                var ans = await task.Invoke(Machine, _tasks, task.Params);
                task.Return(ans);
                return true;
            }
            return false;
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

    public interface ITaskItem
    {
        string Name { get; set; }

        bool StartTimer();

        bool StopTimer();
    }

    public class TaskItemGetData : TaskItem<DataReturnDef>
    {
        public TaskItemGetData(Action<DataReturnDef> returnFunc, int getCycle, int sleepCycle)
        {
            Name = "GetDatas";
            Invoke = async (machine, tasks, parameters) =>
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(100000));
                var ans =
                    await tasks.Run(
                        async () => await machine.InvokeMachineMethod<IMachineMethodData,
                            Task<Dictionary<string, ReturnUnit>>>("GetDatasAsync",
                            MachineGetDataType.CommunicationTag).WithCancellation(cts.Token));
                return new DataReturnDef
                {
                    MachineId = machine.GetMachineIdString(),
                    ReturnValues = ans,
                };
            };
            Params = null;
            Return = returnFunc;
            TimerDisconnectedTime = sleepCycle;
            TimerTime = getCycle;
        }
    }

    public class TaskItemSetData : TaskItem<bool>
    {
        public TaskItemSetData(Dictionary<string, double> values)
        {
            Name = "SetDatas";
            Invoke = Invoke = async (machine, tasks, parameters) =>
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(100000));
                var ans =
                    await tasks.Run(
                        async () => await machine.InvokeMachineMethod<IMachineMethodData,
                            Task<bool>>("SetDatasAsync", parameters[0],
                            MachineSetDataType.CommunicationTag).WithCancellation(cts.Token));
                return ans;
            };
            Params = new object[]{values};
        }
    }

    public class TaskItem<TInterType> : ITaskItem, IEquatable<TaskItem<TInterType>>
    {
        public string Name { get; set; }
        private Timer Timer { get; set; }
        public int TimerTime { get; set; }
        private Timer TimerDisconnected { get; set; }
        public int TimerDisconnectedTime { get; set; }
        public Func<IMachinePropertyWithoutKey, TaskFactory, object[], Task<TInterType>> Invoke { get; set; }
        internal Func<bool> DetectConnected { get; set; } 
        public object[] Params { get; set; }
        public Action<TInterType> Return { get; set; }
        internal Func<IMachinePropertyWithoutKey> GetMachine { get; set; }
        internal Func<TaskFactory> GetTaskFactory { get; set; }

        public bool Equals(TaskItem<TInterType> other)
        {
            return Name == other?.Name;
        }

        public bool StartTimer()
        {
            ActivateTimerDisconnected();
            return true;
        }

        private void ActivateTimer()
        {
            Timer = new Timer(async state =>
            {
                if (!DetectConnected()) TimerChangeToDisconnect();
                var ans = await Invoke(GetMachine(),GetTaskFactory(),Params);
                Return(ans);
            }, null, 0, TimerTime);
        }

        private void DeactivateTimer()
        {
            Timer.Dispose();
            Timer = null;
        }

        private void ActivateTimerDisconnected()
        {
            TimerDisconnected = new Timer(async state =>
            {
                await GetMachine().ConnectAsync();
                if (DetectConnected()) TimerChangeToConnect();
            }, null, 0, TimerDisconnectedTime);
        }

        private void DeactivateTimerDisconnected()
        {
            TimerDisconnected.Dispose();
            TimerDisconnected = null;
        }

        private bool TimerChangeToConnect()
        {
            DeactivateTimerDisconnected();
            ActivateTimer();
            return true;
        }

        private bool TimerChangeToDisconnect()
        {
            DeactivateTimer();
            ActivateTimerDisconnected();
            return true;
        }

        public bool StopTimer()
        {
            DeactivateTimer();
            DeactivateTimerDisconnected();
            return true;
        }
    }

    /// <summary>
    ///     任务调度器
    /// </summary>
    public class TaskManager : TaskManager<string>
    {
        public TaskManager(int maxRunningTask, bool keepConnect,
            MachineDataType dataType = MachineDataType.CommunicationTag)
            : base(maxRunningTask, keepConnect, dataType)
        {
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
        ///     正在运行的设备
        /// </summary>
        private readonly HashSet<TaskMachine<TMachineKey>> _machines;

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
        /// <param name="dataType">获取与设置数据的方式</param>
        public TaskManager(int maxRunningTask, bool keepConnect,
            MachineDataType dataType = MachineDataType.CommunicationTag)
        {
            _scheduler = new LimitedConcurrencyLevelTaskScheduler(maxRunningTask);
            _machines =
                new HashSet<TaskMachine<TMachineKey>>(new TaskMachineEqualityComparer<TMachineKey>());
            KeepConnect = keepConnect;
            MachineDataType = dataType;
            _cts = new CancellationTokenSource();
            _tasks = new TaskFactory(_cts.Token, TaskCreationOptions.None, TaskContinuationOptions.None, _scheduler);
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
                    {
                        machine.Machine.KeepConnect = _keepConnect;
                    }
                }
                ContinueTimerAll();
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
                PauseTimerAll();
                _scheduler = new LimitedConcurrencyLevelTaskScheduler(value);
                ContinueTimerAll();
            }
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
                _machines.Add(new TaskMachine<TMachineKey>(machine, _tasks) {TasksWithTimer = new List<ITaskItem>()});
            }
        }

        /// <summary>
        ///     添加多台设备
        /// </summary>
        /// <param name="machines">设备的列表</param>
        public void AddMachines<TUnitKey>(IEnumerable<BaseMachine<TMachineKey, TUnitKey>> machines)
            where TUnitKey : IEquatable<TUnitKey>
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
                TaskMachine<TMachineKey> machine;
                lock (_machines)
                {
                    machine = _machines.FirstOrDefault(p => p.Machine.Id.Equals(id));
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
                TaskMachine<TMachineKey> machine;
                lock (_machines)
                {
                    machine = _machines.FirstOrDefault(p => p.Machine.ConnectionToken == connectionToken);
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
                _machines.RemoveWhere(p=>p.Machine.Equals(machine));
            }
        }

        public bool InvokeTimerAll<TInterType>(TaskItem<TInterType> item)
        {
            lock (_machines)
            {
                foreach (var machine in _machines)
                {
                    machine.InvokeTimer(item);
                }
            }
            return true;
        }

        public bool StopTimerAll()
        {
            lock (_machines)
            {
                foreach (var machine in _machines)
                {
                    machine.StopAllTimers();
                }
            }
            return true;
        }

        public bool StopTimerAll(string taskItemName)
        {
            lock (_machines)
            {
                foreach (var machine in _machines)
                {
                    machine.StopTimer(taskItemName);
                }
            }
            return true;
        }

        public bool PauseTimerAll()
        {
            lock (_machines)
            {
                foreach (var machine in _machines)
                {
                    machine.PauseAllTimers();
                }
            }
            return true;
        }

        public bool PauseTimerAll(string taskItemName)
        {
            lock (_machines)
            {
                foreach (var machine in _machines)
                {
                    machine.PauseTimer(taskItemName);
                }
            }
            return true;
        }

        public bool ContinueTimerAll()
        {
            lock (_machines)
            {
                foreach (var machine in _machines)
                {
                    machine.ContinueAllTimers();
                }
            }
            return true;
        }

        public bool ConinueTimerAll(string taskItemName)
        {
            lock (_machines)
            {
                foreach (var machine in _machines)
                {
                    machine.ContinueTimer(taskItemName);
                }
            }
            return true;
        }

        public async Task<bool> InvokeOnceAll<TInterType>(TaskItem<TInterType> item)
        {
            var tasks = new List<Task<bool>>();
            lock (_machines)
            {
                foreach (var machine in _machines)
                {
                    tasks.Add(machine.InvokeOnce(item));
                }
            }
            var ans = await Task.WhenAll(tasks);
            return ans.All(p=>p);
        }

        public async Task<bool> InvokeOnceForMachine<TInterType>(TMachineKey machineId, TaskItem<TInterType> item)
        {
            var machine = _machines.FirstOrDefault(p => p.Machine.Id.Equals(machineId));
            if (machine != null)
            {
                await machine.InvokeOnce(item);
                return true;
            }
            return false;
        }

        public bool InvokeTimerForMachine<TInterType>(TMachineKey machineId, TaskItem<TInterType> item)
        {
            var machine = _machines.FirstOrDefault(p => p.Machine.Id.Equals(machineId));
            if (machine != null)
            {
                machine.InvokeTimer(item);
                return true;
            }
            return false;
        }

        public bool StopTimerForMachine(TMachineKey machineId, string taskItemName)
        {
            var machine = _machines.FirstOrDefault(p => p.Machine.Id.Equals(machineId));
            if (machine != null)
            {
                machine.StopTimer(taskItemName);
                return true;
            }
            return false;
        }

        public bool PauseTimerForMachine(TMachineKey machineId, string taskItemName)
        {
            var machine = _machines.FirstOrDefault(p => p.Machine.Id.Equals(machineId));
            if (machine != null)
            {
                machine.PauseTimer(taskItemName);
                return true;
            }
            return false;
        }

        public bool ContinueTimerForMachine(TMachineKey machineId, string taskItemName)
        {
            var machine = _machines.FirstOrDefault(p => p.Machine.Id.Equals(machineId));
            if (machine != null)
            {
                machine.ContinueTimer(taskItemName);
                return true;
            }
            return false;
        }
    }
}