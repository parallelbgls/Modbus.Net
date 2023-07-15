using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Listener;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Modbus.Net
{
    /// <summary>
    ///     JobChaningJobListener with DataMap passing from parent job to next job
    /// </summary>
    public class JobChainingJobListenerWithDataMap : JobListenerSupport
    {
        private static readonly ILogger<JobChainingJobListenerWithDataMap> logger = LogProvider.CreateLogger<JobChainingJobListenerWithDataMap>();

        /// <summary>
        /// JobChaningJobListener with DataMap passing from parent job to next job
        /// </summary>
        /// <param name="name">Job name</param>
        /// <param name="overwriteKeys">If key is overwritable, parent job will pass the value to next job event next job contains that key</param>
        public JobChainingJobListenerWithDataMap(string name, ICollection<string> overwriteKeys)
        {
            Name = name;
            OverWriteKeys = overwriteKeys;
            ChainLinks = new Dictionary<JobKey, JobKey>();
        }

        /// <summary>
        /// Job chain links
        /// </summary>
        protected readonly Dictionary<JobKey, JobKey> ChainLinks;

        /// <inheritdoc />
        public override string Name { get; }

        /// <summary>
        /// Keys that should overwritable
        /// </summary>
        public ICollection<string> OverWriteKeys { get; }

        /// <summary>
        /// Add a chain mapping - when the Job identified by the first key completes
        /// the job identified by the second key will be triggered.
        /// </summary>
        /// <param name="firstJob">a JobKey with the name and group of the first job</param>
        /// <param name="secondJob">a JobKey with the name and group of the follow-up job</param>
        public void AddJobChainLink(JobKey firstJob, JobKey secondJob)
        {
            ChainLinks.Add(firstJob, secondJob);
        }

#nullable enable
        /// <inheritdoc />
        public override async Task JobWasExecuted(IJobExecutionContext context,
            JobExecutionException? jobException,
            CancellationToken cancellationToken = default)
        {
            ChainLinks.TryGetValue(context.JobDetail.Key, out var sj);

            if (sj == null)
            {
                return;
            }

            logger.LogInformation("Job '{JobKey}' will now chain to Job '{Job}'", context.JobDetail.Key, sj);

            try
            {
                var sjJobDetail = await context.Scheduler.GetJobDetail(sj);
                if (sjJobDetail != null)
                {
                    foreach (var entry in context.JobDetail.JobDataMap)
                    {
                        if (!sjJobDetail.JobDataMap.ContainsKey(entry.Key) || sjJobDetail.JobDataMap.ContainsKey(entry.Key) && OverWriteKeys != null && OverWriteKeys.Contains(entry.Key))
                        {
                            sjJobDetail.JobDataMap.Put(entry.Key, entry.Value);
                        }
                    }
                    await context.Scheduler.AddJob(sjJobDetail, true, false);
                    await context.Scheduler.TriggerJob(sj, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (SchedulerException se)
            {
                logger.LogError(se, "Error encountered during chaining to Job '{Job}'", sj);
            }
        }
#nullable disable
    }
}
