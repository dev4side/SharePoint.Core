using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using D4S.SharePoint.Core.Models;
using Microsoft.SharePoint;

namespace D4S.SharePoint.Core.SPExtensions
{
    public static class SPUserExtensions
    {
        /// <summary>
        /// Get the Active Directory information of the current user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="ldapRoot">ldap root string connection</param>
        /// <param name="ldapUsername">username to access ldap</param>
        /// <param name="ldapPassword">password to access ldap</param>
        /// <returns></returns>
        public static AdUser GetAdUser(this SPUser user, string ldapRoot, string ldapUsername, string ldapPassword)
        {
            string userLogin = user.LoginName.Contains('\\') ? user.LoginName.Split('\\')[1] : user.LoginName;
            return AdUser.Load(userLogin, ldapRoot, ldapUsername, ldapPassword);
        }
    }
}
