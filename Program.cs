using System;
using System.Text;

namespace didehpc
{
    public partial class Didehpc
    {
        public Didehpc()
        {
        }
        private int Unknown_command(string command)
        {
            Console.WriteLine("Command " + command + " not found.");
            Console.WriteLine("Commands: bump, cancel, ldap, list, shunt, waitfor");
            return 1;
        }

        public static void Main(string[] args)
        {
            Didehpc D = new Didehpc();
            string command = (args.Length >= 1) ? args[0].ToLower() : "<none>";
            Environment.Exit(
                command == "bump"     ? D.Bump_jobs(args) :
                command == "cancel"   ? D.Cancel_jobs(args) :
                command == "ldap"     ? D.Ldap_madness(args) :
                command == "list"     ? D.List_jobs(args) :
                command == "shunt"    ? D.Shunt_jobs(args) :
                command == "waitfor"  ? D.Wait_for(args) :
                                        D.Unknown_command(command));
        }
    }
}
