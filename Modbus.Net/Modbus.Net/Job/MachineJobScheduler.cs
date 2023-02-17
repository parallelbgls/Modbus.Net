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

    public sealed class MachineJobSchedulerCreator
    {
        public static async Task<MachineGetJobScheduler> CreateScheduler(string triggerKey, int count = 0, double interval = 1)
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();

            ITrigger trigger;
            if (count >= 0)
                trigger = TriggerBuilder.Create()
                    .WithIdentity(triggerKey, "Modbus.Net.DataQuery.Group."+ triggerKey)
                    .StartNow()
                    .WithSimpleSchedule(b => b.WithInterval(TimeSpan.FromSeconds(interval)).WithRepeatCount(count))
                    .Build();
            else
                trigger = TriggerBuilder.Create()
                    .WithIdentity(triggerKey, "Modbus.Net.DataQuery.Group."+ triggerKey)
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

            return new MachineGetJobScheduler(scheduler, trigger);
        }

        public static async Task CancelJob(string triggerKey)
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();
            var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals("Modbus.Net.DataQuery.Group." + triggerKey));
            await scheduler.UnscheduleJob(new TriggerKey(triggerKey, "Modbus.Net.DataQuery.Group." + triggerKey));
            await scheduler.DeleteJobs(jobKeys);         
        }
    }

    public sealed class MachineGetJobScheduler
    {
        IScheduler _scheduler;

        ITrigger _trigger;

        JobKey _parentJobKey = null;

        public MachineGetJobScheduler(IScheduler scheduler, ITrigger trigger)
        {
            _scheduler = scheduler;
            _trigger = trigger;
        }

        public MachineGetJobScheduler(IScheduler scheduler, ITrigger trigger, JobKey parentJobKey)
        {
            _scheduler = scheduler;
            _trigger = trigger;
            _parentJobKey = parentJobKey;
        }

        public async Task<MachineQueryJobScheduler> From(string queryId, IMachineMethodData machine, MachineDataType machineDataType)
        {
            JobKey jobKey = JobKey.Create("Modbus.Net.DataQuery.Job." + queryId, "Modbus.Net.DataQuery.Group." + _trigger.Key.Name);

            IJobDetail job = JobBuilder.Create<MachineGetDataJob>()
                .WithIdentity(jobKey)
                .StoreDurably(true)
                .Build();

            job.JobDataMap.Put("DataType", machineDataType);
            job.JobDataMap.Put("Machine", machine);

            if (_parentJobKey != null)
            {
                var listener = _scheduler.ListenerManager.GetJobListener("Modbus.Net.DataQuery.Chain." + _trigger.Key.Name) as JobChainingJobListenerWithDataMap;
                listener.AddJobChainLink(_parentJobKey, jobKey);

                await _scheduler.AddJob(job, true);
            }
            else
            {
                await _scheduler.ScheduleJob(job, _trigger);
            }

            return new MachineQueryJobScheduler(_scheduler, _trigger, jobKey);
        }

        public Task<MachineQueryJobScheduler> Apply(string queryId, Dictionary<string, double> values, MachineDataType machineDataType)
        {
            return Apply<string>(queryId, values, machineDataType);
        }

        public async Task<MachineQueryJobScheduler> Apply<TMachineKey>(string queryId, Dictionary<string, double> values, MachineDataType machineDataType) where TMachineKey : IEquatable<TMachineKey>
        {
            JobKey jobKey = JobKey.Create("Modbus.Net.DataQuery.Job." + queryId, "Modbus.Net.DataQuery.Group." + _trigger.Key.Name);

            IJobDetail job = JobBuilder.Create<MachineQueryDataJob<TMachineKey>>()
                .WithIdentity(jobKey)
                .StoreDurably(true)
                .Build();

            job.JobDataMap.Put("DataType", machineDataType);
            job.JobDataMap.Put("SetValue", values);
            job.JobDataMap.Put("QueryMethod", null);

            if (_parentJobKey != null)
            {
                var listener = _scheduler.ListenerManager.GetJobListener("Modbus.Net.DataQuery.Chain." + _trigger.Key.Name) as JobChainingJobListenerWithDataMap;
                listener.AddJobChainLink(_parentJobKey, jobKey);

                await _scheduler.AddJob(job, true);
            }
            else
            {            
                await _scheduler.ScheduleJob(job, _trigger);
            }

            return new MachineQueryJobScheduler(_scheduler, _trigger, jobKey);
        }

        public Task<MachineSetJobScheduler> ApplyTo(string queryId, Dictionary<string, double> values, MachineDataType machineDataType)
        {
            return ApplyTo<string>(queryId, values, machineDataType);
        }

        public async Task<MachineSetJobScheduler> ApplyTo<TMachineKey>(string queryId, Dictionary<string, double> values, MachineDataType machineDataType) where TMachineKey : IEquatable<TMachineKey>
        {
            var applyJobScheduler = await Apply<TMachineKey>(queryId, values, machineDataType);
            return await applyJobScheduler.Query();
        }
    }

    public sealed class MachineQueryJobScheduler
    {
        IScheduler _scheduler;

        ITrigger _trigger;

        JobKey _parentJobKey;

        public MachineQueryJobScheduler(IScheduler scheduler, ITrigger trigger, JobKey parentJobKey)
        {
            _scheduler = scheduler;
            _trigger = trigger;
            _parentJobKey = parentJobKey;
        }

        public Task<MachineSetJobScheduler> Query(string queryId = null, Func<DataReturnDef, Dictionary<string, double>> QueryDataFunc = null)
        {
            return Query<string>(queryId, QueryDataFunc);
        }

        public async Task<MachineSetJobScheduler> Query<TMachineKey>(string queryId = null, Func<DataReturnDef, Dictionary<string, double>> QueryDataFunc = null) where TMachineKey : IEquatable<TMachineKey>
        {
            if (queryId == null) return new MachineSetJobScheduler(_scheduler, _trigger, _parentJobKey);

            JobKey jobKey = JobKey.Create("Modbus.Net.DataQuery.Job." + queryId, "Modbus.Net.DataQuery.Group." + _trigger.Key.Name);

            IJobDetail job = JobBuilder.Create<MachineQueryDataJob<TMachineKey>>()
                .WithIdentity(jobKey)
                .StoreDurably(true)
                .Build();

            job.JobDataMap.Put("QueryMethod", QueryDataFunc);

            var listener = _scheduler.ListenerManager.GetJobListener("Modbus.Net.DataQuery.Chain." + _trigger.Key.Name) as JobChainingJobListenerWithDataMap;
            listener.AddJobChainLink(_parentJobKey, jobKey);

            await _scheduler.AddJob(job, true);

            return new MachineSetJobScheduler(_scheduler, _trigger, jobKey);
        }
    }

    public sealed class MachineSetJobScheduler
    {
        IScheduler _scheduler;

        ITrigger _trigger;

        JobKey _parentJobKey;

        public MachineSetJobScheduler(IScheduler scheduler, ITrigger trigger, JobKey parentJobKey)
        {
            _scheduler = scheduler;

            _trigger = trigger;

            _parentJobKey = parentJobKey;
        }

        public async Task<MachineSetJobScheduler> To(string queryId, IMachineMethodData machine)
        {
            JobKey jobKey = JobKey.Create("Modbus.Net.DataQuery.Job." + queryId, "Modbus.Net.DataQuery.Group." + _trigger.Key.Name);

            IJobDetail job = JobBuilder.Create<MachineSetDataJob>()
                .WithIdentity(jobKey)
                .StoreDurably(true)
                .Build();

            job.JobDataMap.Put("Machine", machine);

            var listener = _scheduler.ListenerManager.GetJobListener("Modbus.Net.DataQuery.Chain." + _trigger.Key.Name) as JobChainingJobListenerWithDataMap;
            listener.AddJobChainLink(_parentJobKey, jobKey);

            await _scheduler.AddJob(job, true);

            return new MachineSetJobScheduler(_scheduler, _trigger, jobKey);
        }

        public async Task<MachineQueryJobScheduler> From(string queryId, IMachineMethodData machine, MachineDataType machineDataType)
        {
            return await new MachineGetJobScheduler(_scheduler, _trigger, _parentJobKey).From(queryId, machine, machineDataType);
        }

        public async Task Run()
        {
            await _scheduler.Start();
        }
    }

    public class MachineGetDataJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            object machine;
            object machineDataType;
            context.JobDetail.JobDataMap.TryGetValue("Machine", out machine);
            context.JobDetail.JobDataMap.TryGetValue("DataType", out machineDataType);
            var values = await (machine as IMachineMethodData)!.GetDatasAsync((MachineDataType)machineDataType);

            context.JobDetail.JobDataMap.Put("Value", values);
            await context.Scheduler.AddJob(context.JobDetail, true, false);
        }
    }

    public class MachineQueryDataJob<TMachineKey> : IJob where TMachineKey : IEquatable<TMachineKey>
    {
        public async Task Execute(IJobExecutionContext context)
        {
            object machine;
            object values;
            object QueryMethod;
            context.JobDetail.JobDataMap.TryGetValue("Machine", out machine);
            context.JobDetail.JobDataMap.TryGetValue("Value", out values);
            context.JobDetail.JobDataMap.TryGetValue("QueryMethod", out QueryMethod);
            Func<DataReturnDef, Dictionary<string, double>> QueryMethodDispatch = (Func<DataReturnDef, Dictionary<string, double>>)QueryMethod;

            if (QueryMethod != null && values != null)
            {
                context.JobDetail.JobDataMap.Put("SetValue", QueryMethodDispatch(new DataReturnDef() { MachineId = machine == null ? null : ((IMachineProperty<TMachineKey>)machine).GetMachineIdString(), ReturnValues = (Dictionary<string, ReturnUnit>)values }));
                await context.Scheduler.AddJob(context.JobDetail, true, false);
            }
        }
    }

    public class MachineSetDataJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            object machine;
            object machineDataType;
            object values;
            object valuesSet;
            context.JobDetail.JobDataMap.TryGetValue("Machine", out machine);
            context.JobDetail.JobDataMap.TryGetValue("DataType", out machineDataType);
            context.JobDetail.JobDataMap.TryGetValue("Value", out values);
            context.JobDetail.JobDataMap.TryGetValue("SetValue", out valuesSet);
            if (valuesSet == null && values != null)
                valuesSet = ((Dictionary<string, ReturnUnit>)values).MapGetValuesToSetValues();

            var success = await (machine as IMachineMethodData)!.SetDatasAsync((MachineDataType)machineDataType, (Dictionary<string, double>)valuesSet);
        }
    }
}