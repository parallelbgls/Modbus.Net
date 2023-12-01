using Modbus.Net;
using Modbus.Net.Modbus;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using BaseUtility = Modbus.Net.BaseUtility<byte[], byte[], Modbus.Net.ProtocolUnit<byte[], byte[]>, Modbus.Net.PipeUnit>;
using MultipleMachinesJobScheduler = Modbus.Net.MultipleMachinesJobScheduler<Modbus.Net.IMachineMethodDatas, string, double>;

namespace ModbusTcpToRtu
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        private BaseUtility readUtility;

        private BaseUtility writeUtility;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var triggerKey = "Modbus.Net.Job.Utility.SchedulerTrigger";
            var jobKey = "Modbus.Net.Job.Utility.JobKey";

            var intervalMilliSecond = int.Parse(ConfigurationReader.GetValue("Utility", "interval")) * 1000;
            var count = int.Parse(ConfigurationReader.GetValue("Utility", "count"));

            var readWriteGroup = ConfigurationReader.GetContent<List<ReadWriteGroup>>("Utility", "readwrite");

            var readType = Enum.Parse<ModbusType>(ConfigurationReader.GetValue("Utility:read", "type"));
            var readAddress = ConfigurationReader.GetValue("Utility:read", "address");
            var readSlaveAddress = byte.Parse(ConfigurationReader.GetValue("Utility:read", "slaveAddress"));
            var readMasterAddress = byte.Parse(ConfigurationReader.GetValue("Utility:read", "masterAddress"));
            var writeType = Enum.Parse<ModbusType>(ConfigurationReader.GetValue("Utility:write", "type"));
            var writeAddress = ConfigurationReader.GetValue("Utility:write", "address");
            var writeSlaveAddress = byte.Parse(ConfigurationReader.GetValue("Utility:write", "slaveAddress"));
            var writeMasterAddress = byte.Parse(ConfigurationReader.GetValue("Utility:write", "masterAddress"));

            readUtility = new ModbusUtility(readType, readAddress, readSlaveAddress, readMasterAddress, Endian.BigEndianLsb);
            writeUtility = new ModbusUtility(writeType, writeAddress, writeSlaveAddress, writeMasterAddress, Endian.BigEndianLsb);

            IScheduler scheduler = await StdSchedulerFactory.GetDefaultScheduler();

            await scheduler.Start();

            ITrigger trigger;
            if (intervalMilliSecond <= 0)
            {
                trigger = TriggerBuilder.Create()
                .WithIdentity(triggerKey, "Modbus.Net.DataQuery.Group." + triggerKey)
                    .StartNow()
                    .Build();
            }
            else if (count >= 0)
                trigger = TriggerBuilder.Create()
                .WithIdentity(triggerKey, "Modbus.Net.DataQuery.Group." + triggerKey)
                    .StartNow()
                    .WithSimpleSchedule(b => b.WithInterval(TimeSpan.FromMilliseconds(intervalMilliSecond)).WithRepeatCount(count))
                    .Build();
            else
                trigger = TriggerBuilder.Create()
                .WithIdentity(triggerKey, "Modbus.Net.DataQuery.Group." + triggerKey)
                .StartNow()
                    .WithSimpleSchedule(b => b.WithInterval(TimeSpan.FromMilliseconds(intervalMilliSecond)).RepeatForever())
                    .Build();

            IJobListener listener;
            if (intervalMilliSecond <= 0)
            {
                listener = new JobChainingJobLIstenerWithDataMapRepeated("Modbus.Net.DataQuery.Chain." + triggerKey, new string[2] { "Value", "SetValue" }, count);
            }
            else
            {
                listener = new JobChainingJobListenerWithDataMap("Modbus.Net.DataQuery.Chain." + triggerKey, new string[2] { "Value", "SetValue" });
            }
            scheduler.ListenerManager.AddJobListener(listener, GroupMatcher<JobKey>.GroupEquals("Modbus.Net.DataQuery.Group." + triggerKey));

            if (await scheduler.GetTrigger(new TriggerKey(triggerKey)) != null)
            {
                await scheduler.UnscheduleJob(new TriggerKey(triggerKey, "Modbus.Net.DataQuery.Group." + triggerKey));
            }
            var jobKeys = await scheduler.GetJobKeys(GroupMatcher<JobKey>.GroupEquals("Modbus.Net.DataQuery.Group." + triggerKey));
            await scheduler.DeleteJobs(jobKeys);

            var job = JobBuilder.Create<UtilityPassDataJob>()
                .WithIdentity(jobKey)
                .StoreDurably(true)
                .Build();

            job.JobDataMap.Put("UtilityRead", readUtility);
            job.JobDataMap.Put("UtilityReadWriteGroup", readWriteGroup);
            job.JobDataMap.Put("UtilityWrite", writeUtility);

            await scheduler.ScheduleJob(job, trigger);

        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() => MultipleMachinesJobScheduler.CancelJob());
        }
    }

    public class UtilityPassDataJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            object utilityReadObject;
            object utilityWriteObject;
            object utilityReadWriteGroupObject;

            context.JobDetail.JobDataMap.TryGetValue("UtilityRead", out utilityReadObject);
            context.JobDetail.JobDataMap.TryGetValue("UtilityWrite", out utilityWriteObject);
            context.JobDetail.JobDataMap.TryGetValue("UtilityReadWriteGroup", out utilityReadWriteGroupObject);

            var readUtility = (BaseUtility)utilityReadObject;
            var writeUtility = (BaseUtility)utilityWriteObject;
            var utilityReadWriteGroup = (List<ReadWriteGroup>)utilityReadWriteGroupObject;

            if (readUtility.IsConnected != true)
                await readUtility.ConnectAsync();
            if (writeUtility.IsConnected != true)
                await writeUtility.ConnectAsync();
            foreach (var rwGroup in utilityReadWriteGroup)
            {
                var datas = await readUtility.GetDatasAsync(rwGroup.ReadStart / 10000 + "X " + rwGroup.ReadStart % 10000, rwGroup.ReadCount * 2, rwGroup.ReadCount);
                if (datas.IsSuccess == true)
                {
                    var ans = await writeUtility.SetDatasAsync(rwGroup.WriteStart / 10000 + "X " + rwGroup.WriteStart % 10000, ByteArrayToObjectArray(datas.Datas), rwGroup.ReadCount);
                    if (ans.Datas)
                    {
                        Console.WriteLine("success");
                    }
                }
                else
                {
                    Console.WriteLine("failed");
                }
            }
        }

        public static object[] ByteArrayToObjectArray(byte[] arrBytes)
        {
            List<object> objArray = new List<object>();
            foreach (byte b in arrBytes)
            {
                objArray.Add(b);
            }
            return objArray.ToArray();
        }
    }

    public class ReadWriteGroup
    {
        public int ReadStart { get; set; }
        public int ReadCount { get; set; }
        public int WriteStart { get; set; }
    }
}