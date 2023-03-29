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
    public sealed class MultipleMachinesJobScheduler<TMachineMethod, TMachineKey, TReturnUnit> where TMachineKey : IEquatable<TMachineKey> where TReturnUnit : struct where TMachineMethod : IMachineMethod
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
        public static ParallelLoopResult RunScheduler(IEnumerable<IMachine<TMachineKey>> machines, Func<IMachine<TMachineKey>, MachineGetJobScheduler<TMachineMethod, TMachineKey, TReturnUnit>, Task> machineJobTemplate, int count = 0, int intervalSecond = 1)
        {
            _machineCount = machines.Count();
            return Parallel.ForEach(machines, (machine, state, index) =>
            {
                Task.Factory.StartNew(async () =>
                {
                    Thread.Sleep((int)(intervalSecond * 1000.0 / _machineCount * index));
                    var getJobScheduler = await MachineJobSchedulerCreator<TMachineMethod, TMachineKey, TReturnUnit>.CreateScheduler("Trigger" + index, count, intervalSecond);
                    await machineJobTemplate(machine, getJobScheduler);
                });
            });
        }

        /// <summary>
        ///		取消任务
        /// </summary>
        /// <returns></returns>
        public static ParallelLoopResult CancelJob()
        {
            return Parallel.For(0, _machineCount, async index =>
            {
                await MachineJobSchedulerCreator<TMachineMethod, TMachineKey, TReturnUnit>.CancelJob("Trigger" + index);
            });
        }
    }
}
