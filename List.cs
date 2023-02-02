using System;
using Microsoft.Hpc.Scheduler;
using Microsoft.Hpc.Scheduler.Properties;

namespace didehpc
{
    public partial class Didehpc
    {
        private string date_squash_tab(StoreProperty sp)
        {
            string s = (sp == null) ? null : sp.Value.ToString();
            return (s == null) ? "\t" :
                 s.Substring(6, 4) + s.Substring(3, 2) + s.Substring(0, 2) +
                 s.Substring(11, 2) + s.Substring(14, 2) + s.Substring(17, 2) + "\t";
        }

        private string thing_or_tab(StoreProperty sp)
        {
            string s = (sp == null) ? null : sp.Value.ToString();
            return (s == null) ? "\t" : s + "\t";
        }

        private int List_jobs(string[] args)
        {
            if ((int)args.Length < 4)
            {
                Console.WriteLine("Syntax: didehpc list scheduler state user no_jobs");
                Console.WriteLine("Output: (TSV) Id Name State Resources User StartTime SubmitTime EndTime JobTemplate");
                return 1;
            }
            string scheduler_name = (args[1].Equals(".")) ? System.Environment.MachineName : args[1];
            string state = args[2];
            string user = args[3];
            int no_jobs = short.Parse(args[4]);
            no_jobs = (no_jobs < 0) ? 32767 : no_jobs;

            IScheduler scheduler = Get_scheduler(scheduler_name);
            IFilterCollection job_filters = scheduler.CreateFilterCollection();
            job_filters.Add(FilterOperator.Equal, PropId.Job_State,
                   (state == "Running") ? JobState.Running :
                   (state == "Queued") ? JobState.Queued :
                   (state == "Finished") ? JobState.Finished :
                   (state == "Canceled") ? JobState.Canceled :
                   (state == "Canceling") ? JobState.Canceling :
                   (state == "Failed") ? JobState.Failed :
                   (state == "Configuring") ? JobState.Configuring :
                   (state == "Finishing") ? JobState.Finishing :
                   JobState.All);

            if (!user.Equals("*"))
            {
                job_filters.Add(FilterOperator.Equal, PropId.Job_UserName, user);
            }

            ISortCollection sort_jobs = scheduler.CreateSortCollection();
            sort_jobs.Add(SortProperty.SortOrder.Descending, PropId.Job_SubmitTime);
            IPropertyIdCollection job_properties = new PropertyIdCollection()
            {
                JobPropertyIds.Id,
                JobPropertyIds.MaxCores,
                JobPropertyIds.MaxNodes,
                JobPropertyIds.MaxSockets,
                JobPropertyIds.UnitType,
                JobPropertyIds.UserName,
                JobPropertyIds.Name,
                JobPropertyIds.StartTime,
                JobPropertyIds.SubmitTime,
                JobPropertyIds.EndTime,
                JobPropertyIds.State,
                JobPropertyIds.JobTemplate
            };
            ISchedulerRowEnumerator job_enum = scheduler.OpenJobEnumerator(job_properties, job_filters, sort_jobs);

            PropertyRowSet rows = job_enum.GetRows(no_jobs);
            if ((rows == null) || (rows.Length == 0)) {
                scheduler.Close();
                return 1;
            }

            for (int i = 0; i < rows.Rows.Length; i++)
            {
                PropertyRow row = rows.Rows[i];
                if (row == null)
                {
                    continue;
                }

                Console.Write(row[JobPropertyIds.Id].Value + "\t");
                Console.Write(thing_or_tab(row[JobPropertyIds.Name]));
                Console.Write(row[JobPropertyIds.State].Value + "\t");

                string resource_unit = (row[JobPropertyIds.UnitType].Value.Equals(JobUnitType.Core)) ? "core" :
                                       (row[JobPropertyIds.UnitType].Value.Equals(JobUnitType.Node)) ? "node" :
                                       (row[JobPropertyIds.UnitType].Value.Equals(JobUnitType.Socket)) ? "socket" : "";

                PropertyId resource_id = (resource_unit == "core") ? JobPropertyIds.MaxCores :
                                         (resource_unit == "node") ? JobPropertyIds.MaxNodes :
                                         (resource_unit == "socket") ? JobPropertyIds.MaxSockets : null;

                Console.Write(row[resource_id].Value + " " + resource_unit);
                Console.Write((short.Parse(row[resource_id].Value.ToString()) > 1) ? "s\t" : "\t");
                Console.Write(row[JobPropertyIds.UserName].Value + "\t");
                Console.Write(date_squash_tab(row[JobPropertyIds.StartTime]));
                Console.Write(date_squash_tab(row[JobPropertyIds.SubmitTime]));
                Console.Write(date_squash_tab(row[JobPropertyIds.EndTime]));
                Console.Write(row[JobPropertyIds.JobTemplate].Value + "\t");
                Console.Write("\n");
            }
            scheduler.Close();
            return 0;
        }
    }
}
