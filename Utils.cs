using System;
using System.Collections.Generic;
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

        private List<int> Parse_ids(String s)
        {
            // Parse 123:456 into a sequence of ints

            if (s.Contains(":"))
            {
                string[] bits = s.Split(new char[] { ':' });
                if (bits.Length != 2)
                {
                    Console.WriteLine("Parse error on " + s + " - syntax first:last");
                    Environment.Exit(1);
                }

                int id_from, id_to;

                if (!int.TryParse(bits[0], out id_from))
                {
                    Console.WriteLine("Parse error - " + bits[0] + " is not integer");
                    Environment.Exit(1);
                }

                if (!int.TryParse(bits[1], out id_to))
                {
                    Console.WriteLine("Parse error - " + bits[1] + " is not integer");
                    Environment.Exit(1);
                }

                if (id_to < id_from)
                {
                    Console.WriteLine("Range error - " + id_to + " < " + id_from);
                    Environment.Exit(1);
                }

                if (id_to > id_from + 10000)
                {
                    Console.WriteLine("Range error - 10,000 jobs maximum");
                    Environment.Exit(1);
                }
                List<int> res = new List<int>();
                for (int i = id_from; i <= id_to; i++)
                    res.Add(i);
                return res;

            } else 
            {
                string[] ids = s.Split(new char[] { ',' });
                List<int> res = new List<int>();
                for (int i = 0; i < ids.Length; i++)
                {
                    int id;
                    if (int.TryParse(ids[i], out id))
                    {
                        if (res.Contains(id))
                        {
                            Console.WriteLine("Skipping duplicate " + id);
                            continue;
                        }
                        res.Add(id);
                    }
                    else {
                        Console.WriteLine("Skipping non-integer " + id);
                    }
                }
                return res;
            } 
        }
    }
}
