using System;
using System.Collections.Generic;
using Microsoft.Hpc.Scheduler;
using Microsoft.Hpc.Scheduler.Properties;

namespace didehpc
{
    public partial class Didehpc
    {

        private int Cancel_jobs(string[] args)
        {
            
            if ((int) args.Length != 4)
            {
                Console.WriteLine("Syntax: didehpc cancel scheduler user id1,id2,id3,id4...");
                Console.WriteLine("                                   or id_from:id_to");
                Console.WriteLine("Output: (TSV) ID Status");
                Console.WriteLine("                (Status = {OK,NOT_FOUND,WRONG_USER,WRONG_STATE,ID_ERROR})");
                return 1;
            }

            string scheduler_name = (args[1].Equals(".")) ? System.Environment.MachineName : args[1];
            string user_name = Strip_dide(args[2]);
            List<int> job_ids = Parse_ids(args[3]);

            IScheduler scheduler = Get_scheduler(scheduler_name);

            for (int i = 0; i < job_ids.Count; i++)
            {
                int job_id_int = job_ids[i];
                
                ISchedulerJob scheduler_job = Get_job(scheduler, job_id_int);
                
                if (scheduler_job == null)
                {
                    Console.WriteLine(job_id_int + "\tNOT_FOUND\n");
                    continue;
                }
                
                string job_user_name = Strip_dide(scheduler_job.UserName);
                if (!job_user_name.Equals(user_name))
                {
                    Console.Write(job_id_int + "\tWRONG_USER\n");
                    continue;
                }
                else if (!(scheduler_job.State.Equals(JobState.Running) ||
                           scheduler_job.State.Equals(JobState.Queued)))
                {
                    Console.Write(job_id_int + "\tWRONG_STATE\n");
                    continue;
                }

                Console.Write(job_id_int + "\tOK\n");
                scheduler.CancelJob(job_id_int, "Job Canceled");
            }
            scheduler.Close();
            return 0;
        }
    }
}
