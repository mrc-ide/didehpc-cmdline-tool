using System;
using System.Collections.Generic;
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
                Console.WriteLine("                             or id_from:id_to");

                return 1;
            }

            string scheduler_name = args[1];
            List<int> job_ids = Parse_ids(args[2]);
            string new_template = args[3];

            // TO-DO: Check new_template is valid template and nodegroup

            IScheduler scheduler = Get_scheduler(scheduler_name);
            for (int i = 0; i < job_ids.Count; i++)
            {
                int job_id_int = job_ids[i];
                ISchedulerJob scheduler_job = Get_job(scheduler, job_id_int);

                if (scheduler_job == null)
                {
                    Console.WriteLine(job_id_int + "\tNOT_FOUND");
                    continue;
                }

                if (scheduler_job.State != JobState.Queued)
                {
                    Console.WriteLine(job_id_int + "\tNOT_QUEUED");
                    continue;
                }

                // Need job to be normal priority to requeue, so...

                scheduler_job.Priority = JobPriority.Normal;
                scheduler_job.Commit();

                // Put job into configuring state so we can change template and group
                // Then resubmit the job

                scheduler.ConfigureJob(scheduler_job.Id);
                scheduler_job.SetJobTemplate(new_template);
                StringCollection node_group = new StringCollection();
                node_group.Add(new_template);
                scheduler_job.NodeGroups = node_group;
                scheduler_job.Commit();
                scheduler_job.Requeue();

                Console.Write(job_id_int + "\tOK\n");
                
            }
            scheduler.Close();
            return 0;
        }
    }
}
