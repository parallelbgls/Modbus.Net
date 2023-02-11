// See https://aka.ms/new-console-template for more information
using Modbus.Net;
using Modbus.Net.Modbus;
using Modbus.Net.Interface;
using Quartz;
using Quartz.Impl;

List<AddressUnit> _addresses = new List<AddressUnit>
{
    new AddressUnit() { Area = "4X", Address = 1, DataType = typeof(short), Id = "1", Name = "Test1" },
    new AddressUnit() { Area = "4X", Address = 2, DataType = typeof(short), Id = "2", Name = "Test2" },
    new AddressUnit() { Area = "4X", Address = 3, DataType = typeof(short), Id = "3", Name = "Test3" },
    new AddressUnit() { Area = "4X", Address = 4, DataType = typeof(short), Id = "4", Name = "Test4" },
    new AddressUnit() { Area = "4X", Address = 5, DataType = typeof(short), Id = "5", Name = "Test5" },
    new AddressUnit() { Area = "4X", Address = 6, DataType = typeof(short), Id = "6", Name = "Test6" },
    new AddressUnit() { Area = "4X", Address = 7, DataType = typeof(short), Id = "7", Name = "Test7" },
    new AddressUnit() { Area = "4X", Address = 8, DataType = typeof(short), Id = "8", Name = "Test8" },
    new AddressUnit() { Area = "4X", Address = 9, DataType = typeof(short), Id = "9", Name = "Test9" },
    new AddressUnit() { Area = "4X", Address = 10, DataType = typeof(short), Id = "10", Name = "Test10" }
};

IMachine<string> machine = new ModbusMachine<string, string>("ModbusMachine1", ModbusType.Tcp, "192.168.0.172:502", _addresses, true, 2, 1, Endian.BigEndianLsb);


//1.首先创建一个作业调度池
ISchedulerFactory schedf = new StdSchedulerFactory();
//2.实例化调度器工厂
ISchedulerFactory schedulefactory = new StdSchedulerFactory();
//3.实例化调度器
IScheduler scheduler = await schedulefactory.GetScheduler();

//4.创建一个作业
IJobDetail job1 = JobBuilder.Create<Class1>()
    .WithIdentity("demojob1", "groupa")
    .Build();

//5.1:第一种方法直接写死多少秒执行一次
//ITrigger trigger1 = TriggerBuilder.Create()//创建一个触发器
//    .WithIdentity("demotrigger1", "groupa")
//    .StartNow()
//    .WithSimpleSchedule(b => b.WithIntervalInSeconds(5)//5秒执行一次
//    .RepeatForever())//无限循环执行
//    .Build();

//5.2推荐：第二种使用cron表达式
//在线生成cron表达式： http://cron.qqe2.com/
string corn = "*/5 * * * * ?";
ITrigger trigger1 = TriggerBuilder.Create()
    .WithIdentity("demotrigger1", "groupa")
    .WithCronSchedule(corn)//每一小时执行一次
    .Build();

//6.添加参数（键值对），如果不需要传参则忽略这一步
//方法内获取你传的参数： string Name = context.Trigger.JobDataMap.GetString("Name");
trigger1.JobDataMap.Add("Machine", machine);

//7.把作业，触发器加入调度器
await scheduler.ScheduleJob(job1, trigger1);
//8.开始运行
await scheduler.Start();

Console.ReadLine();

public class Class1 : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        object machine;
        context.Trigger.JobDataMap.TryGetValue("Machine", out machine);
        var values = await (machine as IMachine<string>)!.GetDatasAsync(MachineGetDataType.Name);
        if (values != null)
        {
            foreach (var value in values)
            {
                Console.WriteLine(value.Key + " " + value.Value.PlcValue);
            }
        }
    }
}


