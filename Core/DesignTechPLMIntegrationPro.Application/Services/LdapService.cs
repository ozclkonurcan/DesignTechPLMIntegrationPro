using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesignTechPLMIntegrationPro.Application.Services
{
    public class LdapService
    {
        public bool AuthenticateUser(string username, string password, string server, int port)
        {
            try
            {
                var ldapConnection = new LdapConnection();
                ldapConnection.SecureSocketLayer = true;
                ldapConnection.Connect(server, port);

                // Kimlik doğrulama
                ldapConnection.Bind(username, password);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
