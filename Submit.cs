using System;
using System.Collections.Generic;
using Microsoft.Hpc.Scheduler;

namespace didehpc
{
    public partial class Didehpc
    {

        private int Submit_message_exit()
        {
            Console.WriteLine("Syntax: didehpc submit ");
            Console.WriteLine("                       scheduler      : the name of the headnode");
            Console.WriteLine("                       template       : the name of the job template");
            Console.WriteLine("                       num_resources  : Number of cores or nodes to reserve for each job");
            Console.WriteLine("                       resource_type  : Core or Node");
            Console.WriteLine("                       work_dir       : Full path to working dir, (base64 encoded)");
            Console.WriteLine("                       stdout         : stdout path, full, or relative to workdir. (base64 encoded)");
            Console.WriteLine("                       stderr         : stderr path, full, or relative to workdir. (base64 encoded)");
            Console.WriteLine("                       stdin          : stdin path, full, or relative to workdir. (base64 encoded)");
            Console.WriteLine("                       user           : username of job owner. (base64 encoded)");
            Console.WriteLine("                       password       : password of job owner. (base64 encoded)");
            Console.WriteLine("                       ids            : integers: 1 or 1,3,5  or 3:10 for ranges. Iterated as %hpcid% below :-");
            Console.WriteLine("                       name           : jobname. (base64 encoded) %hpcid% (encoded) will be replaced");
            Console.WriteLine("                       exec           : Rest of line (base64 encoded) to be run on the cluster. %hpcid% will be replaced.");
            return 1;
        }
        private int Submit_jobs(string[] args)
        {
            int argc = args.Length;
            if (argc < 13)
            {
                Console.WriteLine("Need 13 args\n");
                return(Submit_message_exit());
            }
            string scheduler_name = (args[1].Equals(".")) ? System.Environment.MachineName : args[1];
            string job_template = args[2];

            int num_resources;
            if (!int.TryParse(args[3], out num_resources))
            {
                Console.WriteLine("Need integer number of cores/nodes\n");
                return (Submit_message_exit());
            }

            String resource_type = (args[4].ToUpper());
            if (resource_type.StartsWith("NODE")) resource_type = "NODE";
            else if (resource_type.StartsWith("CORE")) resource_type = "CORE";
            else
            {
                Console.WriteLine("Resource type should be Core or Node\n");
                return (Submit_message_exit());
            }
            String work_dir = Base64_decode(args[5]);
            Console.WriteLine("work_dir = " + work_dir);
            String stdout = Base64_decode(args[6]);
            Console.WriteLine("stdout = " + stdout);
            String stderr = Base64_decode(args[7]);
            Console.WriteLine("stderr = " + stderr);
            String stdin = Base64_decode(args[8]);
            Console.WriteLine("stdin = " + stdin);
            String user = Base64_decode(args[9]);
            String password = Base64_decode(args[10]);

            List<int> job_ids = Parse_ids(args[11]);
            String job_name = Base64_decode(args[12]);
            String exec = Base64_decode(args[13]);
            Console.WriteLine("exec = " + exec);
                        
            IScheduler scheduler = Get_scheduler(scheduler_name);

            for (int i=0; i<job_ids.Count; i++) 
            {
                String job_id = job_ids[i].ToString();
                ISchedulerJob job = scheduler.CreateJob();
                job.SetJobTemplate(args[2]);
                ISchedulerTask task = job.CreateTask();

                if (resource_type.Equals("NODE"))
                {
                    job.MaximumNumberOfNodes = num_resources;
                    job.MinimumNumberOfNodes = num_resources;
                    job.UnitType = Microsoft.Hpc.Scheduler.Properties.JobUnitType.Node;
                    job.SingleNode = false;
                    task.MaximumNumberOfNodes = num_resources;
                    task.MinimumNumberOfNodes = num_resources;
                }
                else
                {
                    job.MaximumNumberOfCores = num_resources;
                    job.MinimumNumberOfCores = num_resources;
                    job.UnitType = Microsoft.Hpc.Scheduler.Properties.JobUnitType.Core;
                    job.SingleNode = true;
                    task.MaximumNumberOfCores = num_resources;
                    task.MinimumNumberOfCores = num_resources;
                }
                
                task.WorkDirectory = work_dir;
                task.StdErrFilePath = stderr.Replace("%hpcid%", job_id);
                task.StdOutFilePath = stdout.Replace("%hpcid%", job_id);
                task.StdInFilePath = stdin.Replace("%hpcid%", job_id);
                job.UserName = user;
                task.Name = job_name.Replace("%hpcid%", job_id);
                job.Name = job_name.Replace("%hpcid%", job_id);
                task.CommandLine = exec.Replace("%hpcid%", job_id);

                job.AddTask(task);

                scheduler.SubmitJob(job, user, password);
                Console.Write(job_id + "\tOK\n");
            }
            scheduler.Close();
            return 0;
        }
    }
}
