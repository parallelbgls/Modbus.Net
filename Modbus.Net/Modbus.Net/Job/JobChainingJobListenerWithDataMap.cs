using Quartz.Listener;
using Quartz;
using Serilog;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Modbus.Net
{
    public class JobChainingJobListenerWithDataMap : JobListenerSupport
    {
        public JobChainingJobListenerWithDataMap(string name, bool overwrite)
        {
            Name = name;
            OverWrite = overwrite;
            chainLinks = new Dictionary<JobKey, JobKey>();
        }

        private readonly Dictionary<JobKey, JobKey> chainLinks;

        public override string Name { get; }

        public bool OverWrite { get; }

        /// <summary>
        /// Add a chain mapping - when the Job identified by the first key completes
        /// the job identified by the second key will be triggered.
        /// </summary>
        /// <param name="firstJob">a JobKey with the name and group of the first job</param>
        /// <param name="secondJob">a JobKey with the name and group of the follow-up job</param>
        public void AddJobChainLink(JobKey firstJob, JobKey secondJob)
        {
            chainLinks.Add(firstJob, secondJob);
        }

        public override async Task JobWasExecuted(IJobExecutionContext context,
            JobExecutionException? jobException,
            CancellationToken cancellationToken = default)
        {
            chainLinks.TryGetValue(context.JobDetail.Key, out var sj);

            if (sj == null)
            {
                return;
            }

            Log.Information("Job '{JobKey}' will now chain to Job '{Job}'", context.JobDetail.Key, sj);

            try
            {
                var sjJobDetail = await context.Scheduler.GetJobDetail(sj);
                foreach (var entry in context.JobDetail.JobDataMap)
                {
                    if (!OverWrite && !sjJobDetail.JobDataMap.ContainsKey(entry.Key) || OverWrite)
                    {
                        sjJobDetail.JobDataMap.Put(entry.Key, entry.Value);                       
                    }
                }
                await context.Scheduler.AddJob(sjJobDetail, true, false);
                await context.Scheduler.TriggerJob(sj, cancellationToken).ConfigureAwait(false);
            }
            catch (SchedulerException se)
            {
                Log.Error(se, "Error encountered during chaining to Job '{Job}'", sj);
            }
        }
    }
}
