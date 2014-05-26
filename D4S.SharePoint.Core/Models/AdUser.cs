using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;

namespace D4S.SharePoint.Core.Models
{
    public class AdUser
    {
        private AdUser(){}

        // Properties
        public string Firstname { get; set; }
        public string LogonName { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Mail { get; set; }
        public string ManagerRaw { get; set; }
        public string Manager 
        {
            get{ return ManagerRaw.Split(new char[] { ',' })[0].Replace("CN=", string.Empty); }
        }

        public static AdUser Load(string userLogin, string ldapRoot, string ldapUsername, string ldapPassword)
        {
            //strLogonName = strLogonName.ToUpper().Replace(@"ITALY\", string.Empty);
            userLogin = userLogin.Contains('\\') ? userLogin.Split('\\')[1] : userLogin;
            DirectoryEntry searchRoot = new DirectoryEntry(ldapRoot, ldapUsername, ldapPassword);
            DirectorySearcher searcher = new DirectorySearcher(searchRoot);
            searcher.Filter = string.Format("(&(sAMAccountName={0}))", userLogin);
            searcher.PropertiesToLoad.Add("sAMAccountName");
            searcher.PropertiesToLoad.Add("name");
            searcher.PropertiesToLoad.Add("givenname");
            searcher.PropertiesToLoad.Add("sn");
            searcher.PropertiesToLoad.Add("mail");
            searcher.PropertiesToLoad.Add("cn");
            searcher.PropertiesToLoad.Add("Manager");
            SearchResult result = null;
            result = searcher.FindOne();
            var usr = new AdUser();
            usr.LogonName = result.Properties["sAMAccountName"].Count != 0 ? result.Properties["sAMAccountName"][0].ToString() : string.Empty;
            usr.Name = result.Properties["name"].Count != 0 ? result.Properties["name"][0].ToString() : string.Empty;
            usr.Firstname = result.Properties["givenname"].Count != 0 ? result.Properties["givenname"][0].ToString() : string.Empty;
            usr.Surname = result.Properties["sn"].Count != 0 ? result.Properties["sn"][0].ToString() : string.Empty;
            usr.Mail = result.Properties["mail"].Count != 0 ? result.Properties["mail"][0].ToString() : string.Empty;
            usr.ManagerRaw = (result.Properties["Manager"].Count != 0) ? result.Properties["Manager"][0].ToString() : string.Empty;
            return usr;
        }
    }
}
