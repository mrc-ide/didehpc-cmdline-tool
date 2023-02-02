using System;
using System.Collections.Generic;
using Microsoft.Hpc.Scheduler;

namespace didehpc
{
    public partial class Didehpc
    {

        private int Bump_message_exit()
        {
            Console.WriteLine("Syntax: didehpc bump scheduler id1,id2,id3,id4... <optional integer priority>");
            Console.WriteLine("                           or  id_from:id_to\n");
            Console.WriteLine("        If priority is omitted, then current priority will be increased by 10");
            Console.WriteLine("        If specified, priority should be an integer between 1000..5000");
            return 1;
        }
        private int Bump_jobs(string[] args)
        {
            int argc = args.Length;
            if ((argc < 3) || (argc > 4))
            {
                return(Bump_message_exit());
            }
            int new_priority = -1;
            if (argc == 4)
            {
                if (!Int32.TryParse(args[3], out new_priority))
                {
                    return (Bump_message_exit());
                }

                if ((new_priority < 1000) || (new_priority > 4000))
                {
                    return (Bump_message_exit());
                }
            }

            string scheduler_name = (args[1].Equals(".")) ? System.Environment.MachineName : args[1];

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

                if (argc == 3) new_priority = Math.Min(4000, scheduler_job.ExpandedPriority + 10);
                scheduler_job.ExpandedPriority = new_priority;
                scheduler_job.Commit();
                Console.Write(job_id_int + "\tOK\n");
            }
            scheduler.Close();
            return 0;
        }
    }
}
