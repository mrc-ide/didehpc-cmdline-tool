using System;
using System.Text;

namespace didehpc
{
    public partial class Didehpc
    {
        public Didehpc()
        {
        }

        private string strip_dide(string s)
        {
            while (s.ToUpper().StartsWith("DIDE\\"))
            {
                s = s.Substring(5);
            }
            return s;
        }

        private string base64_decode(string s)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(s));
        }

        public static void Main(string[] args)
        {
            Didehpc D = new Didehpc();
            string command = "<none>";
            if (args.Length >= 1)
            {
                command = args[0].ToLower();
                switch (command)
                {
                    case "ldap":
                        D.ldap_madness(args);
                        Environment.Exit(0);
                        break;
                    case "cancel":
                        D.cancel_jobs(args);
                        Environment.Exit(0);
                        break;
                    case "list":
                        D.list_jobs(args);
                        Environment.Exit(0);
                        break;
                }
            }
            Console.WriteLine("Command " + command + " not found.");
            Console.WriteLine("Commands: cancel, ldap, list, waitfor");
            Environment.Exit(1);
        }
    }
}
