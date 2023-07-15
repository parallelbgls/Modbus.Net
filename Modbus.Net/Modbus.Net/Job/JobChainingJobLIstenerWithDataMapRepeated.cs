using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     Repeated JobChaningJobListener
    /// </summary>
    public class JobChainingJobLIstenerWithDataMapRepeated : JobChainingJobListenerWithDataMap
    {
        /// <summary>
        /// JobChaningJobListener with DataMap passing from parent job to next job
        /// </summary>
        /// <param name="name">Job name</param>
        /// <param name="overwriteKeys">If key is overwritable, parent job will pass the value to next job event next job contains that key</param>
        public JobChainingJobLIstenerWithDataMapRepeated(string name, ICollection<string> overwriteKeys) : base(name, overwriteKeys)
        {
        }

#nullable enable
        /// <inheritdoc />
        public override async Task JobWasExecuted(IJobExecutionContext context,
            JobExecutionException? jobException,
            CancellationToken cancellationToken = default)
        {
            await base.JobWasExecuted(context, jobException, cancellationToken);
            ChainLinks.TryGetValue(context.JobDetail.Key, out var sj);
            if (sj == null)
            {
                var chainRoot = context.JobDetail.Key;
                var chainParent = ChainLinks.FirstOrDefault(p => p.Value == context.JobDetail.Key).Key;               
                while (chainParent != null)
                {
                    chainRoot = chainParent;
                    chainParent = ChainLinks.FirstOrDefault(p => p.Value == chainParent).Key;
                }

                var sjJobDetail = await context.Scheduler.GetJobDetail(chainRoot);
                await context.Scheduler.AddJob(sjJobDetail!, true, false);
                await context.Scheduler.TriggerJob(chainRoot, cancellationToken).ConfigureAwait(false);
            }
        }
#nullable disable
    }
}
