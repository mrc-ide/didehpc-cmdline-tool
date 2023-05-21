using System;
using System.Text;

namespace didehpc
{
    public partial class Didehpc
    {
        bool b64 = false;
        
        public Didehpc()
        {
        }
        private int Unknown_command(string command)
        {
            Console.WriteLine("didehpc v0.9");
            Console.WriteLine("Command " + command + " not found.");
            Console.WriteLine("Commands: bump      -  Change priority of multiple jobs");
            Console.WriteLine("          cancel    -  Cancel multiple jobs");
            Console.WriteLine("          ldap      -  Talk to LDAP server about user groups");
            Console.WriteLine("          list      -  List jobs in various ways");
            Console.WriteLine("          querydeps -  List states of parent jobs being waited on");
            Console.WriteLine("          shunt     -  Move jobs to different template");
            Console.WriteLine("          submit    -  Submit multiple jobs");
            Console.WriteLine("          waitfor   -  Block until a job finishes");
            return 1;
        }

        public static void Main(string[] args)
        {
            Didehpc D = new Didehpc();
            if ((args.Length >= 1) && (args[0].ToUpper().Equals("/B64")))
            {
                D.b64 = true;
                string[] args2 = new string[args.Length - 1];
                for (int i = 1; i < args.Length; i++)
                {
                    args2[i - 1] = args[i];
                }
                args = args2;
            }

            string command = (args.Length >= 1) ? args[0].ToLower() : "<none>";
            Environment.Exit(
                command == "bump"      ? D.Bump_jobs(args) :
                command == "cancel"    ? D.Cancel_jobs(args) :
                command == "ldap"      ? D.Ldap_madness(args) :
                command == "list"      ? D.List_jobs(args) :
                command == "querydeps" ? D.Query_Deps(args) :
                command == "shunt"     ? D.Shunt_jobs(args) :
                command == "submit"    ? D.Submit_jobs(args) :
                command == "waitfor"   ? D.Wait_for(args) :
                                         D.Unknown_command(command));;
        }
    }
}
