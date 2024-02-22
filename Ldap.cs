using System;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Text.RegularExpressions;
using SearchScope = System.DirectoryServices.Protocols.SearchScope;

namespace didehpc
{
    public partial class Didehpc
    {
        private int Ldap_madness(string[] args)
        {
            // For given (Base 64 encoded) user and password,
            // prints to the console all the groups that user
            // belongs to according to the domain controller

            if (args.Length != 3)
            {
                Console.WriteLine("Syntax: didehpc ldap base64_user base64_password");
                return (1);
            }

            try
            {
                string user = Base64_decode(args[1]);

                // user should only contain letters and numbers and dash - just to ensure no
                // injection in the LDAP query itself.

                user = Regex.Replace(user, @"[^\w\d-]", "");

                string pw = Base64_decode(args[2]);
                string local_domain = "dide.local";
                string domain_controller = "fi--didedc1.dide.ic.ac.uk";

                // Establish the connection to LDAP as the given user.

                NetworkCredential network_credential = new NetworkCredential(user, pw, local_domain);

                LdapConnection ldapConnection = new LdapConnection(
                    new LdapDirectoryIdentifier(domain_controller, 389),
                    network_credential, AuthType.Negotiate)
                {
                    Credential = network_credential
                };

                ldapConnection.SessionOptions.ProtocolVersion = 3;
                ldapConnection.Bind();

                // Run the search for groups

                SearchRequest searchRequest = new SearchRequest(
                    "OU=Users,OU=DIDE Users,DC=dide,DC=local",
                    "(&(objectCategory=person)(SAMAccountName=" + user + "))",
                    SearchScope.Subtree, new string[] { "SAMAccountName", "memberOf", "cn" });

                SearchResponse searchResponse = (SearchResponse)ldapConnection.SendRequest(searchRequest);

                // Parse / print results

                if (searchResponse.Entries.Count == 1)
                {
                    SearchResultEntry item = searchResponse.Entries[0];
                    for (int i = 0; i < item.Attributes["memberOf"].Count; i++)
                    {
                        string result_part = item.Attributes["memberOf"][i].ToString();
                        string[] result_split = result_part.Split(new char[] { ',' });
                        for (int j = 0; j < (int)result_split.Length; j++)
                        {
                            if (result_split[j].StartsWith("CN"))
                            {
                                Console.WriteLine(result_split[j].Substring(3));
                            }
                        }
                    }
                }
                return(0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message == "The supplied credential is invalid." ?
                                               "CREDENTIAL_ERROR" : e.Message);
                return (1);
            }
        }
    }
}
