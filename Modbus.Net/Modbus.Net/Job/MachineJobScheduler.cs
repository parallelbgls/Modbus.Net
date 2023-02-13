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
        public static async Task<MachineGetJobScheduler> CreateScheduler(string triggerKey, int count, double interval)
        {
            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();

            ITrigger trigger;
            if (count >= 0)
                trigger = TriggerBuilder.Create()
                    .WithIdentity("Modbus.Net.DataQuery.Trigger." + triggerKey, "Modbus.Net.DataQuery.Group")
                    .StartNow()
                    .WithSimpleSchedule(b => b.WithInterval(TimeSpan.FromSeconds(interval)).WithRepeatCount(count))
                    .Build();
            else
                trigger = TriggerBuilder.Create()
                    .WithIdentity("Modbus.Net.DataQuery.Trigger." + triggerKey, "Modbus.Net.DataQuery.Group")
                    .StartNow()
                    .WithSimpleSchedule(b => b.WithInterval(TimeSpan.FromSeconds(interval)).RepeatForever())
                    .Build();

            return new MachineGetJobScheduler(scheduler, trigger);
        }
    }

    public sealed class MachineGetJobScheduler
    {
        IScheduler _scheduler;

        ITrigger _trigger;

        public MachineGetJobScheduler(IScheduler scheduler, ITrigger trigger)
        {
            _scheduler = scheduler;
            _trigger = trigger;
        }

        public async Task<MachineQueryJobScheduler> From(string queryId, IMachineMethodData machine, MachineDataType machineDataType)
        {
            JobKey jobKey = JobKey.Create("Modbus.Net.DataQuery.Job." + queryId, "Modbus.Net.DataQuery.Group");

            IJobDetail job = JobBuilder.Create<MachineGetDataJob>()
                .WithIdentity(jobKey)
                .Build();

            job.JobDataMap.Put("DataType", machineDataType);
            job.JobDataMap.Put("Machine", machine);

            await _scheduler.ScheduleJob(job, _trigger);

            return new MachineQueryJobScheduler(_scheduler, _trigger, jobKey);
        }

        public Task<MachineQueryJobScheduler> Apply(string queryId, Dictionary<string, ReturnUnit> values, MachineDataType machineDataType)
        {
            return Apply<string>(queryId, values, machineDataType);
        }

        public async Task<MachineQueryJobScheduler> Apply<TMachineKey>(string queryId, Dictionary<string, ReturnUnit> values, MachineDataType machineDataType) where TMachineKey : IEquatable<TMachineKey>
        {
            JobKey jobKey = JobKey.Create("Modbus.Net.DataQuery.Job." + queryId, "Modbus.Net.DataQuery.Group");

            IJobDetail job = JobBuilder.Create<MachineQueryDataJob<TMachineKey>>()
                .WithIdentity(jobKey)
                .Build();

            job.JobDataMap.Put("DataType", machineDataType);
            job.JobDataMap.Put("Value", values);
            job.JobDataMap.Put("QueryMethod", null);

            await _scheduler.ScheduleJob(job, _trigger);

            return new MachineQueryJobScheduler(_scheduler, _trigger, jobKey);
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

        public Task<MachineSetJobScheduler> Query(string queryId = null, Func<DataReturnDef, Dictionary<string, ReturnUnit>> QueryDataFunc = null)
        {
            return Query<string>(queryId, QueryDataFunc);
        }

        public async Task<MachineSetJobScheduler> Query<TMachineKey>(string queryId = null, Func<DataReturnDef, Dictionary<string, ReturnUnit>> QueryDataFunc = null) where TMachineKey : IEquatable<TMachineKey>
        {
            JobChainingJobListenerWithDataMap listener;
            try
            {
                listener = _scheduler.ListenerManager.GetJobListener("Modbus.Net.DataQuery.Chain") as JobChainingJobListenerWithDataMap;
            }
            catch
            {
                listener = new JobChainingJobListenerWithDataMap("Modbus.Net.DataQuery.Chain", new string[1]{ "Value" });
                _scheduler.ListenerManager.AddJobListener(listener, GroupMatcher<JobKey>.GroupEquals("Modbus.Net.DataQuery.Group"));
            }

            if (queryId == null) return new MachineSetJobScheduler(_scheduler, _trigger, listener, _parentJobKey);

            JobKey jobKey = JobKey.Create("Modbus.Net.DataQuery.Job." + queryId, "Modbus.Net.DataQuery.Group");

            IJobDetail job = JobBuilder.Create<MachineQueryDataJob<TMachineKey>>()
                .WithIdentity(jobKey)
                .StoreDurably(true)
                .Build();

            job.JobDataMap.Put("QueryMethod", QueryDataFunc);

            listener.AddJobChainLink(_parentJobKey, jobKey);

            await _scheduler.AddJob(job, true);

            return new MachineSetJobScheduler(_scheduler, _trigger, listener, jobKey);
        }
    }

    public sealed class MachineSetJobScheduler
    {
        IScheduler _scheduler;

        ITrigger _trigger;

        JobChainingJobListenerWithDataMap _listener;

        JobKey _parentJobKey;

        public MachineSetJobScheduler(IScheduler scheduler, ITrigger trigger, JobChainingJobListenerWithDataMap listener, JobKey parentJobKey)
        {
            _scheduler = scheduler;

            _trigger = trigger;

            _listener = listener;

            _parentJobKey = parentJobKey;
        }

        public async Task<MachineSetJobScheduler> To(string queryId, IMachineMethodData machine)
        {
            JobKey jobKey = JobKey.Create("Modbus.Net.DataQuery.Job." + queryId, "Modbus.Net.DataQuery.Group");

            IJobDetail job = JobBuilder.Create<MachineSetDataJob>()
                .WithIdentity(jobKey)
                .StoreDurably(true)
                .Build();

            job.JobDataMap.Put("Machine", machine);

            _listener.AddJobChainLink(_parentJobKey, jobKey);

            await _scheduler.AddJob(job, true);

            return new MachineSetJobScheduler(_scheduler, _trigger, _listener, jobKey);
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
            context.MergedJobDataMap.TryGetValue("Machine", out machine);
            context.MergedJobDataMap.TryGetValue("DataType", out machineDataType);
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
            context.MergedJobDataMap.TryGetValue("Machine", out machine);
            context.MergedJobDataMap.TryGetValue("Value", out values);
            context.MergedJobDataMap.TryGetValue("QueryMethod", out QueryMethod);
            Func<DataReturnDef, Dictionary<string, ReturnUnit>> QueryMethodDispatch = (Func<DataReturnDef, Dictionary<string, ReturnUnit>>)QueryMethod;

            if (QueryMethod != null)
            {
                context.JobDetail.JobDataMap.Put("Value", QueryMethodDispatch(new DataReturnDef() { MachineId = ((IMachineProperty<TMachineKey>)machine).GetMachineIdString(), ReturnValues = (Dictionary<string, ReturnUnit>)values }));
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
            context.MergedJobDataMap.TryGetValue("Machine", out machine);
            context.MergedJobDataMap.TryGetValue("DataType", out machineDataType);
            context.MergedJobDataMap.TryGetValue("Value", out values);
            Dictionary<string, double> valuesSet = ((Dictionary<string, ReturnUnit>)values).MapGetValuesToSetValues();

            var success = await (machine as IMachineMethodData)!.SetDatasAsync((MachineDataType)machineDataType, valuesSet);
        }
    }
}