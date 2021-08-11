using System;
using Microsoft.Hpc.Scheduler;
using Microsoft.Hpc.Scheduler.Properties;

namespace didehpc
{
    public partial class Didehpc
    {

        private int Bump_jobs(string[] args)
        {
            int argc = args.Length;
            if ((argc < 3) || (argc > 4))
            {
                Console.WriteLine("Syntax: didehpc bump scheduler id1,id2,id3,id4... <optional priority>");
                Console.WriteLine("        If priority is omitted, then current priority will be increased by 10");
                return 1;
            }

            string scheduler_name = args[1];
            string[] job_ids = args[2].Split(new char[] { ',' });

            IScheduler scheduler = Get_scheduler(scheduler_name);
            for (int i = 0; i < (int)job_ids.Length; i++)
            {
                string job_id = job_ids[i];
                int job_id_int = -1;
                if (!int.TryParse(job_id, out job_id_int))
                {
                    Console.Write(string.Concat(job_id, "\tID_ERROR"));
                    continue;
                }

                ISchedulerJob scheduler_job = Get_job(scheduler, job_id_int);

                if (scheduler_job == null)
                {
                    Console.WriteLine(job_id + "\tNOT_FOUND\n");
                    continue;
                }

                int new_priority = (argc == 3) ? scheduler_job.ExpandedPriority : int.Parse(args[3]);
                scheduler_job.ExpandedPriority = new_priority;
                scheduler_job.Commit();
                scheduler_job.Refresh();
                Console.Write(job_id + "\tOK\n");
            }
            scheduler.Close();
            return 0;
        }
    }
}
