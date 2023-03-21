using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///		多设备任务调度器
    /// </summary>
    public sealed class MultipleMachinesJobScheduler
    {
        private static int _machineCount = 0;

        /// <summary>
        ///     创建设备调度器
        /// </summary>
        /// <param name="machines">设备的集合</param>
        /// <param name="machineJobTemplate">设备的运行模板</param>
        /// <param name="count">重复次数，负数为无限循环，0为执行一次</param>
        /// <param name="intervalSecond">间隔秒数</param>
        /// <returns></returns>
        public static ParallelLoopResult RunScheduler<TKey>(IEnumerable<IMachine<TKey>> machines, Func<IMachine<TKey>, MachineGetJobScheduler, Task> machineJobTemplate, int count = 0, int intervalSecond = 1) where TKey : IEquatable<TKey>
        {
            _machineCount = machines.Count();
            return Parallel.ForEach(machines, (machine, state, index) =>
            {
                Task.Factory.StartNew(async () =>
                {
                    Thread.Sleep((int)(intervalSecond * 1000.0 / _machineCount * index));
                    var getJobScheduler = await MachineJobSchedulerCreator.CreateScheduler("Trigger" + index, count, intervalSecond);
                    await machineJobTemplate(machine, getJobScheduler);
                });
            });
        }

        /// <summary>
        ///     创建设备调度器
        /// </summary>
        /// <param name="machines">设备的集合</param>
        /// <param name="machineJobTemplate">设备的运行模板</param>
        /// <param name="count">重复次数，负数为无限循环，0为执行一次</param>
        /// <param name="intervalSecond">间隔秒数</param>
        /// <returns></returns>
        public static ParallelLoopResult RunScheduler(IEnumerable<IMachine<string>> machines, Func<IMachine<string>, MachineGetJobScheduler, Task> machineJobTemplate, int count = 0, int intervalSecond = 1)
        {
            return RunScheduler<string>(machines, machineJobTemplate, count, intervalSecond);
        }

        /// <summary>
        ///		取消任务
        /// </summary>
        /// <returns></returns>
        public static ParallelLoopResult CancelJob()
        {
            return Parallel.For(0, _machineCount, async index =>
            {
                await MachineJobSchedulerCreator.CancelJob("Trigger" + index);
            });
        }
    }
}
