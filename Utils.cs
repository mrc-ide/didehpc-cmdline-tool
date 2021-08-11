using System;
using System.Text;
using Microsoft.Hpc.Scheduler;

namespace didehpc
{
    public partial class Didehpc
    {

        private IScheduler Get_scheduler(string name)
        {
            IScheduler scheduler = new Scheduler();
            try
            {
                scheduler.Connect(name);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error connection to scheduler " + name + " - " + e.ToString());
                Environment.Exit(1);
            }
            return scheduler;
        }

        private ISchedulerJob Get_job(IScheduler sch, int id)
        {
            ISchedulerJob job = null;
            try
            {
                job = sch.OpenJob(id);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Scheduler error: " + exception.Message);
                Environment.Exit(1);
            }
            return job;
        }

        private string Strip_dide(string s)
        {
            while (s.ToUpper().StartsWith("DIDE\\"))
            {
                s = s.Substring(5);
            }
            return s;
        }

        private string Base64_decode(string s)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(s));
        }
    }
}
