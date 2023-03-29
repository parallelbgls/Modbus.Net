using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     返回结果的定义类
    /// </summary>
    public class DataReturnDef<TMachineKey, TReturnUnit> where TMachineKey : IEquatable<TMachineKey> where TReturnUnit : struct
    {
        /// <summary>
        ///     设备的Id
        /// </summary>
        public TMachineKey MachineId { get; set; }

        /// <summary>
        ///     返回的数据值
        /// </summary>
        public ReturnStruct<Dictionary<string, ReturnUnit<TReturnUnit>>> ReturnValues { get; set; }
    }

    /// <summary>
    ///     设备调度器创建类
    /// </summary>
    public sealed class MachineJobSchedulerCreator<TMachineMethod, TMachineKey, TReturnUnit> where TMachineKey : IEquatable<TMachineKey> where TReturnUnit : struct where TMachineMethod : IMachineMethod
    {
        /// <summary>
        ///     创建设备调度器
        /// </summary>
        /// <param name="triggerKey">键，全局唯一不能重复，重复会终止并删除已存在的调度器</param>
        /// <param name="count">重复次数，负数为无限循环，0为执行一次</param>
        /// <param name="intervalSecond">间隔秒数</param>
        /// <returns></returns>
        public static async Task<MachineGetJobScheduler<TMachineMethod, TMachineKey, TReturnUnit>> CreateScheduler(string triggerKey, int count = 0, int intervalSecond = 1)
        {
            return await CreateScheduler(triggerKey, count, (double)intervalSecond);
        }

        /// <summary>
        ///     创建设备调度器
        /// </summary>
        /// <param name="triggerKey">调度器键名，全局唯一不能重复，重复会终止并删除已存在的调度器</param>
        /// <param name="count">重复次数，负数为无限循环，0为执行一次</param>
        /// <param name="intervalMilliSecond">间隔毫秒数</param>
        /// <returns></returns>
        public static async Task<MachineGetJobScheduler<TMachineMethod, TMachineKey, TReturnUnit>> CreateSchedulerMillisecond(string triggerKey, int count = 0, int intervalMilliSecond = 1000)
        {
            return await CreateScheduler(triggerKey, count, intervalMilliSecond / 1000.0);
        }

        private static async Task<MachineGetJobScheduler<TMachineMethod, TMachineKey, TReturnUnit>> CreateScheduler(string triggerKey, int count = 0, double interval = 1)
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();

            ITrigger trigger;
            if (count >= 0)
                trigger = TriggerBuilder.Create()
                    .WithIdentity(triggerKey, "Modbus.Net.DataQuery.Group." + triggerKey)
                    .StartNow()
                    .WithSimpleSchedule(b => b.WithInterval(TimeSpan.FromSeconds(interval)).WithRepeatCount(count))
                    .Build();
            else
                trigger = TriggerBuilder.Create()
                    .WithIdentity(triggerKey, "Modbus.Net.DataQuery.Group." + triggerKey)
                    .StartNow()
                    .WithSimpleSchedule(b => b.WithInterval(TimeSpan.FromSeconds(interval)).RepeatForever())
                    .Build();

            var listener = new JobChainingJobListenerWithDataMap("Modbus.Net.DataQuery.Chain." + triggerKey, new string[2] { "Value", "SetValue" });
            scheduler.ListenerManager.AddJobListener(listener, GroupMatcher<JobKey>.GroupEquals("Modbus.Net.DataQuery.Group." + triggerKey));

            if (await scheduler.GetTrigger(new TriggerKey(triggerKey)) != null)
            {
                await scheduler.UnscheduleJob(new TriggerKey(triggerKey, "Modbus.Net.DataQuery.Group." + triggerKey));
            }
            var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals("Modbus.Net.DataQuery.Group." + triggerKey));
            await scheduler.DeleteJobs(jobKeys);

            return new MachineGetJobScheduler<TMachineMethod, TMachineKey, TReturnUnit>(scheduler, trigger);
        }

        /// <summary>
        ///     取消并删除任务调度器
        /// </summary>
        /// <param name="triggerKey">调度器键名</param>
        /// <returns></returns>
        public static async Task CancelJob(string triggerKey)
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals("Modbus.Net.DataQuery.Group." + triggerKey));
            await scheduler.UnscheduleJob(new TriggerKey(triggerKey, "Modbus.Net.DataQuery.Group." + triggerKey));
            await scheduler.DeleteJobs(jobKeys);
        }
    }

    /// <summary>
    ///     获取数据任务
    /// </summary>
    public sealed class MachineGetJobScheduler<TMachineMethod, TMachineKey, TReturnUnit> where TMachineKey : IEquatable<TMachineKey> where TReturnUnit : struct where TMachineMethod : IMachineMethod
    {
        private IScheduler _scheduler;

        private ITrigger _trigger;

        private JobKey _parentJobKey = null;

        /// <summary>
        ///     获取数据任务
        /// </summary>
        /// <param name="scheduler">调度器</param>
        /// <param name="trigger">触发器</param>
        public MachineGetJobScheduler(IScheduler scheduler, ITrigger trigger)
        {
            _scheduler = scheduler;
            _trigger = trigger;
        }

        /// <summary>
        ///     获取数据任务
        /// </summary>
        /// <param name="scheduler">调度器</param>
        /// <param name="trigger">触发器</param>
        /// <param name="parentJobKey">父任务的键</param>
        public MachineGetJobScheduler(IScheduler scheduler, ITrigger trigger, JobKey parentJobKey)
        {
            _scheduler = scheduler;
            _trigger = trigger;
            _parentJobKey = parentJobKey;
        }

        /// <summary>
        ///     从设备获取数据
        /// </summary>
        /// <param name="queryId">任务ID，每个触发器唯一</param>
        /// <param name="machine">要获取数据的设备实例</param>
        /// <param name="machineDataType">获取数据的方式</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public async Task<MachineQueryJobScheduler<TMachineMethod, TMachineKey, TReturnUnit>> From(string queryId, IMachineReflectionCall machine, MachineDataType machineDataType)
        {
            JobKey jobKey = JobKey.Create("Modbus.Net.DataQuery.Job." + queryId, "Modbus.Net.DataQuery.Group." + _trigger.Key.Name);

            IJobDetail job = JobBuilder.Create<MachineGetDataJob<TReturnUnit>>()
                .WithIdentity(jobKey)
                .StoreDurably(true)
                .Build();

            string methodName = typeof(TMachineMethod).Name;
            if (methodName.Substring(0, 14) != "IMachineMethod")
            {
                throw new FormatException("IMachineMethod Name not match format exception");
            }
            job.JobDataMap.Put("DataType", machineDataType);
            job.JobDataMap.Put("Machine", machine);
            job.JobDataMap.Put("Function", methodName.Remove(0, 14));

            if (_parentJobKey != null)
            {
                var listener = _scheduler.ListenerManager.GetJobListener("Modbus.Net.DataQuery.Chain." + _trigger.Key.Name) as JobChainingJobListenerWithDataMap;
                if (listener == null) throw new NullReferenceException("Listener " + "Modbus.Net.DataQuery.Chain." + _trigger.Key.Name + " is null");
                listener.AddJobChainLink(_parentJobKey, jobKey);

                await _scheduler.AddJob(job, true);
            }
            else
            {
                await _scheduler.ScheduleJob(job, _trigger);
            }

            return new MachineQueryJobScheduler<TMachineMethod, TMachineKey, TReturnUnit>(_scheduler, _trigger, jobKey);
        }

        /// <summary>
        ///     直接向任务队列中写一个数据模板
        /// </summary>
        /// <param name="queryId">任务ID，每个触发器唯一</param>
        /// <param name="values">要写入的数据模板</param>
        /// <param name="machineDataType">获取数据的方式</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public async Task<MachineQueryJobScheduler<TMachineMethod, TMachineKey, TReturnUnit>> Apply(string queryId, Dictionary<string, double> values, MachineDataType machineDataType)
        {
            JobKey jobKey = JobKey.Create("Modbus.Net.DataQuery.Job." + queryId, "Modbus.Net.DataQuery.Group." + _trigger.Key.Name);

            IJobDetail job = JobBuilder.Create<MachineQueryDataJob<TMachineKey, TReturnUnit>>()
                .WithIdentity(jobKey)
                .StoreDurably(true)
                .Build();

            job.JobDataMap.Put("DataType", machineDataType);
            job.JobDataMap.Put("SetValue", values);
            job.JobDataMap.Put("QueryMethod", null);

            if (_parentJobKey != null)
            {
                var listener = _scheduler.ListenerManager.GetJobListener("Modbus.Net.DataQuery.Chain." + _trigger.Key.Name) as JobChainingJobListenerWithDataMap;
                if (listener == null) throw new NullReferenceException("Listener " + "Modbus.Net.DataQuery.Chain." + _trigger.Key.Name + " is null");
                listener.AddJobChainLink(_parentJobKey, jobKey);

                await _scheduler.AddJob(job, true);
            }
            else
            {
                await _scheduler.ScheduleJob(job, _trigger);
            }

            return new MachineQueryJobScheduler<TMachineMethod, TMachineKey, TReturnUnit>(_scheduler, _trigger, jobKey);
        }

        /// <summary>
        ///     直接向任务队列中写一个数据模板，并跳过处理数据流程
        /// </summary>
        /// <param name="queryId">任务ID，每个触发器唯一</param>
        /// <param name="values">要写入的数据模板</param>
        /// <param name="machineDataType">获取数据的方式</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public async Task<MachineSetJobScheduler<TMachineMethod, TMachineKey, TReturnUnit>> ApplyTo(string queryId, Dictionary<string, double> values, MachineDataType machineDataType)
        {
            var applyJobScheduler = await Apply(queryId, values, machineDataType);
            return await applyJobScheduler.Query();
        }
    }

    /// <summary>
    ///     处理数据任务
    /// </summary>
    public sealed class MachineQueryJobScheduler<TMachineMethod, TMachineKey, TReturnUnit> where TMachineKey : IEquatable<TMachineKey> where TReturnUnit : struct where TMachineMethod : IMachineMethod
    {
        private IScheduler _scheduler;

        private ITrigger _trigger;

        private JobKey _parentJobKey;

        /// <summary>
        ///     处理数据任务
        /// </summary>
        /// <param name="scheduler">调度器</param>
        /// <param name="trigger">触发器</param>
        /// <param name="parentJobKey">父任务的键</param>
        public MachineQueryJobScheduler(IScheduler scheduler, ITrigger trigger, JobKey parentJobKey)
        {
            _scheduler = scheduler;
            _trigger = trigger;
            _parentJobKey = parentJobKey;
        }

        /// <summary>
        ///     处理数据
        /// </summary>
        /// <param name="queryId">任务ID，每个触发器唯一</param>
        /// <param name="QueryDataFunc">处理数据的函数，输入返回读数据的定义和值，输出写数据字典</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public async Task<MachineSetJobScheduler<TMachineMethod, TMachineKey, TReturnUnit>> Query(string queryId = null, Func<DataReturnDef<TMachineKey, TReturnUnit>, Dictionary<string, TReturnUnit>> QueryDataFunc = null)
        {
            if (queryId == null) return new MachineSetJobScheduler<TMachineMethod, TMachineKey, TReturnUnit>(_scheduler, _trigger, _parentJobKey);

            JobKey jobKey = JobKey.Create("Modbus.Net.DataQuery.Job." + queryId, "Modbus.Net.DataQuery.Group." + _trigger.Key.Name);

            IJobDetail job = JobBuilder.Create<MachineQueryDataJob<TMachineKey, TReturnUnit>>()
                .WithIdentity(jobKey)
                .StoreDurably(true)
                .Build();

            if (QueryDataFunc != null)
                job.JobDataMap.Put("QueryMethod", QueryDataFunc);

            var listener = _scheduler.ListenerManager.GetJobListener("Modbus.Net.DataQuery.Chain." + _trigger.Key.Name) as JobChainingJobListenerWithDataMap;
            if (listener == null) throw new NullReferenceException("Listener " + "Modbus.Net.DataQuery.Chain." + _trigger.Key.Name + " is null");
            listener.AddJobChainLink(_parentJobKey, jobKey);

            await _scheduler.AddJob(job, true);

            return new MachineSetJobScheduler<TMachineMethod, TMachineKey, TReturnUnit>(_scheduler, _trigger, jobKey);
        }
    }

    /// <summary>
    ///     写入数据任务
    /// </summary>
    public sealed class MachineSetJobScheduler<TMachineMethod, TMachineKey, TReturnUnit> where TMachineKey : IEquatable<TMachineKey> where TReturnUnit : struct where TMachineMethod : IMachineMethod
    {
        private IScheduler _scheduler;

        private ITrigger _trigger;

        private JobKey _parentJobKey;

        /// <summary>
        ///     写入数据任务
        /// </summary>
        /// <param name="scheduler">调度器</param>
        /// <param name="trigger">触发器</param>
        /// <param name="parentJobKey">父任务的键</param>
        public MachineSetJobScheduler(IScheduler scheduler, ITrigger trigger, JobKey parentJobKey)
        {
            _scheduler = scheduler;

            _trigger = trigger;

            _parentJobKey = parentJobKey;
        }

        /// <summary>
        ///     向设备写入数据
        /// </summary>
        /// <param name="queryId">任务ID，每个触发器唯一</param>
        /// <param name="machine">写入数据的设备实例</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public async Task<MachineDealJobScheduler<TMachineMethod, TMachineKey, TReturnUnit>> To(string queryId, IMachineReflectionCall machine)
        {
            JobKey jobKey = JobKey.Create("Modbus.Net.DataQuery.Job." + queryId, "Modbus.Net.DataQuery.Group." + _trigger.Key.Name);

            IJobDetail job = JobBuilder.Create<MachineSetDataJob<TReturnUnit>>()
                .WithIdentity(jobKey)
                .StoreDurably(true)
            .Build();

            string methodName = typeof(TMachineMethod).Name;
            if (methodName.Substring(0, 14) != "IMachineMethod")
            {
                throw new FormatException("IMachineMethod Name not match format exception");
            }
            job.JobDataMap.Put("Machine", machine);
            job.JobDataMap.Put("Function", methodName.Remove(0, 14));

            var listener = _scheduler.ListenerManager.GetJobListener("Modbus.Net.DataQuery.Chain." + _trigger.Key.Name) as JobChainingJobListenerWithDataMap;
            if (listener == null) throw new NullReferenceException("Listener " + "Modbus.Net.DataQuery.Chain." + _trigger.Key.Name + " is null");
            listener.AddJobChainLink(_parentJobKey, jobKey);

            await _scheduler.AddJob(job, true);

            return new MachineDealJobScheduler<TMachineMethod, TMachineKey, TReturnUnit>(_scheduler, _trigger, jobKey);
        }

        /// <summary>
        ///     再次获取一个设备的数据
        /// </summary>
        /// <param name="queryId">任务ID，每个触发器唯一</param>
        /// <param name="machine">要获取数据的设备实例</param>
        /// <param name="machineDataType">获取数据的方式</param>
        /// <returns></returns>
        public async Task<MachineQueryJobScheduler<TMachineMethod, TMachineKey, TReturnUnit>> From(string queryId, IMachineReflectionCall machine, MachineDataType machineDataType)
        {
            return await new MachineGetJobScheduler<TMachineMethod, TMachineKey, TReturnUnit>(_scheduler, _trigger, _parentJobKey).From(queryId, machine, machineDataType);
        }

        /// <summary>
        ///     执行任务
        /// </summary>
        /// <returns></returns>
        public async Task Run()
        {
            await _scheduler.Start();
        }
    }

    /// <summary>
    ///     处理写返回任务
    /// </summary>
    public sealed class MachineDealJobScheduler<TMachineMethod, TMachineKey, TReturnUnit> where TMachineKey : IEquatable<TMachineKey> where TReturnUnit : struct where TMachineMethod : IMachineMethod
    {
        private IScheduler _scheduler;

        private ITrigger _trigger;

        private JobKey _parentJobKey;

        /// <summary>
        ///     处理写返回任务
        /// </summary>
        /// <param name="scheduler">调度器</param>
        /// <param name="trigger">触发器</param>
        /// <param name="parentJobKey">父任务的键</param>
        public MachineDealJobScheduler(IScheduler scheduler, ITrigger trigger, JobKey parentJobKey)
        {
            _scheduler = scheduler;

            _trigger = trigger;

            _parentJobKey = parentJobKey;
        }

        /// <summary>
        ///     处理写返回
        /// </summary>
        /// <param name="queryId">任务ID，每个触发器唯一</param>
        /// <param name="onSuccess">成功回调方法，参数为设备ID</param>
        /// <param name="onFailure">失败回调方法，参数为设备ID</param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public async Task<MachineSetJobScheduler<TMachineMethod, TMachineKey, TReturnUnit>> Deal(string queryId = null, Func<string, Task> onSuccess = null, Func<string, int, string, Task> onFailure = null)
        {
            if (queryId == null) return new MachineSetJobScheduler<TMachineMethod, TMachineKey, TReturnUnit>(_scheduler, _trigger, _parentJobKey);
            JobKey jobKey = JobKey.Create("Modbus.Net.DataQuery.Job." + queryId, "Modbus.Net.DataQuery.Group." + _trigger.Key.Name);

            IJobDetail job = JobBuilder.Create<MachineDealDataJob<TMachineKey>>()
                .WithIdentity(jobKey)
                .StoreDurably(true)
            .Build();

            job.JobDataMap.Put("OnSuccess", onSuccess);
            job.JobDataMap.Put("OnFailure", onFailure);

            var listener = _scheduler.ListenerManager.GetJobListener("Modbus.Net.DataQuery.Chain." + _trigger.Key.Name) as JobChainingJobListenerWithDataMap;
            if (listener == null) throw new NullReferenceException("Listener " + "Modbus.Net.DataQuery.Chain." + _trigger.Key.Name + " is null");
            listener.AddJobChainLink(_parentJobKey, jobKey);

            await _scheduler.AddJob(job, true);

            return new MachineSetJobScheduler<TMachineMethod, TMachineKey, TReturnUnit>(_scheduler, _trigger, jobKey);
        }
    }

    /// <summary>
    ///     获取数据任务
    /// </summary>
    public class MachineGetDataJob<TReturnUnit> : IJob where TReturnUnit : struct
    {
        /// <inheritdoc />
        public async Task Execute(IJobExecutionContext context)
        {
            object machine;
            object machineDataType;
            object callFunction;
            context.JobDetail.JobDataMap.TryGetValue("Machine", out machine);
            context.JobDetail.JobDataMap.TryGetValue("DataType", out machineDataType);
            context.JobDetail.JobDataMap.TryGetValue("Function", out callFunction);
            var values = await (machine as IMachineReflectionCall)!.InvokeGet<Dictionary<string, ReturnUnit<TReturnUnit>>>((string)callFunction, new object[] { (MachineDataType)machineDataType });

            context.JobDetail.JobDataMap.Put("Value", values);
            await context.Scheduler.AddJob(context.JobDetail, true, false);
        }
    }

    /// <summary>
    ///     处理数据任务
    /// </summary>
    public class MachineQueryDataJob<TMachineKey, TReturnUnit> : IJob where TMachineKey : IEquatable<TMachineKey> where TReturnUnit : struct
    {
        /// <inheritdoc />
        public async Task Execute(IJobExecutionContext context)
        {
            object machine;
            object values;
            object QueryMethod;
            context.JobDetail.JobDataMap.TryGetValue("Machine", out machine);
            context.JobDetail.JobDataMap.TryGetValue("Value", out values);
            context.JobDetail.JobDataMap.TryGetValue("QueryMethod", out QueryMethod);
            Func<DataReturnDef<TMachineKey, TReturnUnit>, Dictionary<string, TReturnUnit>> QueryMethodDispatch = (Func<DataReturnDef<TMachineKey, TReturnUnit>, Dictionary<string, TReturnUnit>>)QueryMethod;

            if (QueryMethod != null && values != null)
            {
                context.JobDetail.JobDataMap.Put("SetValue", QueryMethodDispatch(new DataReturnDef<TMachineKey, TReturnUnit>() { MachineId = machine == null ? default : ((IMachineProperty<TMachineKey>)machine).Id, ReturnValues = (ReturnStruct<Dictionary<string, ReturnUnit<TReturnUnit>>>)values }));
                await context.Scheduler.AddJob(context.JobDetail, true, false);
            }
        }
    }

    /// <summary>
    ///     写数据任务
    /// </summary>
    public class MachineSetDataJob<TReturnUnit> : IJob where TReturnUnit : struct
    {
        /// <inheritdoc />
        public async Task Execute(IJobExecutionContext context)
        {
            object machine;
            object machineDataType;
            object values;
            object valuesSet;
            object callFunction;
            context.JobDetail.JobDataMap.TryGetValue("Machine", out machine);
            context.JobDetail.JobDataMap.TryGetValue("DataType", out machineDataType);
            context.JobDetail.JobDataMap.TryGetValue("Value", out values);
            context.JobDetail.JobDataMap.TryGetValue("SetValue", out valuesSet);
            context.JobDetail.JobDataMap.TryGetValue("Function", out callFunction);
            if (valuesSet == null && values != null)
            {
                valuesSet = ((ReturnStruct<Dictionary<string, ReturnUnit<TReturnUnit>>>)values).Datas.MapGetValuesToSetValues();
            }

            if (valuesSet == null)
            {
                context.JobDetail.JobDataMap.Put("Success", false);
                return;
            }
            var success = await (machine as IMachineReflectionCall)!.InvokeSet((string)callFunction, new object[] { (MachineDataType)machineDataType }, (Dictionary<string, double>)valuesSet);

            context.JobDetail.JobDataMap.Put("Success", success);
        }
    }

    /// <summary>
    ///     处理写返回任务
    /// </summary>
    public class MachineDealDataJob<TMachineKey> : IJob where TMachineKey : IEquatable<TMachineKey>
    {
        /// <inheritdoc />
        public async Task Execute(IJobExecutionContext context)
        {
            object machine;
            object success;
            object onSuccess;
            object onFailure;
            context.JobDetail.JobDataMap.TryGetValue("Machine", out machine);
            context.JobDetail.JobDataMap.TryGetValue("Success", out success);
            context.JobDetail.JobDataMap.TryGetValue("OnSuccess", out onSuccess);
            context.JobDetail.JobDataMap.TryGetValue("OnFailure", out onFailure);
            ReturnStruct<bool> successValue = (ReturnStruct<bool>)success;
            if (successValue.IsSuccess == true && onSuccess != null)
            {
                await ((Func<TMachineKey, Task>)onSuccess)(((IMachineProperty<TMachineKey>)machine).Id);
            }
            if (successValue.IsSuccess == false && onFailure != null)
            {
                await ((Func<TMachineKey, int, string, Task>)onFailure)(((IMachineProperty<TMachineKey>)machine).Id, successValue.ErrorCode, successValue.ErrorMsg);
            }

            context.JobDetail.JobDataMap.Remove("Success");
            context.JobDetail.JobDataMap.Remove("OnSuccess");
            context.JobDetail.JobDataMap.Remove("OnFailure");
        }
    }
}