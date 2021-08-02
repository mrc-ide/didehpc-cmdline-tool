using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Threading;
using Microsoft.Hpc.Scheduler;
using Microsoft.Hpc.Scheduler.Properties;

namespace didehpc
{
    public partial class Didehpc
    {
        private void wait_for(string[] args)
        {
            if ((int)args.Length != 5)
            {
                Console.WriteLine("Syntax: didehpc waitfor scheduler base64_email base64_group_name 123,456,789");
                Environment.Exit(1);
            }
            
            string scheduler_name = args[1];
            string email = base64_decode(args[2]);
            string group_name = base64_decode(args[3]);
            string[] job_ids = args[4].Split(new char[] { ',' });

            IScheduler scheduler = new Scheduler();
            scheduler.Connect(scheduler_name);
            int n_all = 0;
            int n_finished = 0;
            int n_failed = 0;
            int n_canceled = 0;
            int n_other = 0;
            while (n_all != job_ids.Length)
            {
                ISchedulerJob schedulerJob = scheduler.OpenJob(int.Parse(job_ids[n_all]));
                if (schedulerJob == null)
                {
                    n_all++;
                }
                else if ((schedulerJob.State == JobState.Queued || 
                          schedulerJob.State == JobState.Configuring || 
                          schedulerJob.State == JobState.Running || 
                          schedulerJob.State == JobState.Canceling || 
                          schedulerJob.State == JobState.Finishing || 
                          schedulerJob.State == JobState.Validating || 
                          schedulerJob.State == JobState.Submitted ? false : schedulerJob.State != JobState.ExternalValidation))
                {
                    n_all++;
                    n_canceled += (schedulerJob.State == JobState.Canceled) ? 1 : 0;
                    n_failed += (schedulerJob.State == JobState.Failed) ? 1 : 0; 
                    n_finished += (schedulerJob.State == JobState.Finished) ? 1 : 0;
                    n_other += (schedulerJob.State != JobState.Canceled &&
                                schedulerJob.State != JobState.Failed &&
                                schedulerJob.State != JobState.Finished) ? 1 : 0;
                }
                else
                {
                    Thread.Sleep(5000);
                }
            }
            MailMessage mailMessage = new MailMessage("dide-it@imperial.ac.uk", email);
            SmtpClient smtpClient = new SmtpClient()
            {
                Port = 25,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Host = "automail.cc.ic.ac.uk"
            };
            mailMessage.Subject = string.Concat("HPC: Group ", group_name, " Finished");
            mailMessage.Body = string.Concat(new object[] { 
                scheduler_name, " reports that the jobs in the group ", group_name, " have finished", Environment.NewLine, 
                "Finished jobs: ", n_finished, Environment.NewLine, 
                "Failed jobs: ", n_failed, Environment.NewLine, 
                "Canceled jobs: ", n_canceled, Environment.NewLine, 
                "Unaccounted for: ", n_other, Environment.NewLine });
            smtpClient.Send(mailMessage);
            scheduler.Close();
            Environment.Exit(0);
        }
    }
}
