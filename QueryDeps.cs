using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Hpc.Scheduler;

namespace didehpc
{
    public partial class Didehpc
    {

        private int Query_Deps_message_exit()
        {
            Console.WriteLine("Syntax: didehpc querypdeps scheduler id");
            return 1;
        }
        
        private int Query_Deps(string[] args)
        {
            int argc = args.Length;
            if (argc != 3)
            {
                return(Query_Deps_message_exit());
            }

            if (b64)
            {
                args[1] = Base64_decode(args[1]);
                args[2] = Base64_decode(args[2]);
            }
            
            string scheduler_name = (args[1].Equals(".")) ? System.Environment.MachineName : args[1];
            int job_id_int = -1;
            int.TryParse(args[2], out job_id_int);
            
            IScheduler scheduler = Get_scheduler(scheduler_name);
            ISchedulerJob scheduler_job = Get_job(scheduler, job_id_int);

            if (scheduler_job == null)
            {
                Console.WriteLine(job_id_int + "\tNOT_FOUND\n");
                return 1;
            }

            foreach (int parent_id in scheduler_job.ParentJobIds)
            {
                ISchedulerJob parent_job = Get_job(scheduler, parent_id);
                if (parent_job != null)
                {
                    Console.WriteLine(parent_id + "\t" + parent_job.State);
                }
            }
            scheduler.Close();
            return 0;
        }
    }
}
