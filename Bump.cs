using System;
using System.Collections.Generic;
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
                Console.WriteLine("                           or  id_from:id_to\n");
                Console.WriteLine("        If priority is omitted, then current priority will be increased by 10");
                return 1;
            }

            string scheduler_name = args[1];
            List<int> job_ids = Parse_ids(args[2]);

            IScheduler scheduler = Get_scheduler(scheduler_name);
            for (int i=0; i<job_ids.Count; i++) 
            {
                int job_id_int = job_ids[i];
                ISchedulerJob scheduler_job = Get_job(scheduler, job_id_int);

                if (scheduler_job == null)
                {
                    Console.WriteLine(job_id_int + "\tNOT_FOUND\n");
                    continue;
                }

                int new_priority = (argc == 3) ? Math.Min(4000, scheduler_job.ExpandedPriority + 10) : int.Parse(args[3]);
                scheduler_job.ExpandedPriority = new_priority;
                scheduler_job.Commit();
                Console.Write(job_id_int + "\tOK\n");
            }
            scheduler.Close();
            return 0;
        }
    }
}
