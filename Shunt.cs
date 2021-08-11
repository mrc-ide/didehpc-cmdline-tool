using System;
using Microsoft.Hpc.Scheduler;
using Microsoft.Hpc.Scheduler.Properties;

namespace didehpc
{
    public partial class Didehpc
    {

        private int Shunt_jobs(string[] args)
        {
            
            if ((int) args.Length != 4)
            {
                Console.WriteLine("Syntax: didehpc shunt scheduler id1,id2,id3,id4... template");
                return 1;
            }

            string scheduler_name = args[1];
            string[] job_ids = args[2].Split(new char[] { ',' });
            string new_template = args[3];

            IScheduler scheduler = Get_scheduler(scheduler_name);
            for (int i = 0; i < (int) job_ids.Length; i++)
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

                // Need job to be normal priority to move.

                JobPriority orig_priority = scheduler_job.Priority;
                string orig_template = scheduler_job.JobTemplate;

                scheduler_job.Priority = JobPriority.Normal;
                scheduler_job.Commit();
                scheduler.ConfigureJob(scheduler_job.Id);

                scheduler_job.SetJobTemplate(new_template);
                StringCollection node_group = new StringCollection();
                node_group.Add(new_template);
                scheduler_job.NodeGroups = node_group;
                scheduler_job.Commit();
                scheduler_job.Requeue();

                Console.Write(job_id + "\tOK\n");
                
            }
            scheduler.Close();
            return 0;
        }
    }
}
