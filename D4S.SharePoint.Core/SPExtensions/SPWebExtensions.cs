using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace D4S.SharePoint.Core.SPExtensions
{
    public static class SPWebExtensions
    {
        public delegate void CodeToRunElevated(SPWeb elevatedWeb);

        public static void RunElevated(this SPWeb web, CodeToRunElevated secureCode)
        {
            string webUrl = web.Url;
            SPSecurity.RunWithElevatedPrivileges(() =>
            {
                using (SPSite site = new SPSite(webUrl))
                {
                    try
                    {
                        site.AllowUnsafeUpdates = true;
                        using (SPWeb elevatedWeb = site.OpenWeb())
                        {
                            try
                            {
                                elevatedWeb.AllowUnsafeUpdates = true;
                                secureCode(elevatedWeb);
                            }
                            finally
                            {
                                elevatedWeb.AllowUnsafeUpdates = false;
                            }
                        }
                    }
                    finally
                    {
                        site.AllowUnsafeUpdates = false;
                    }
                }
            });
        }

        /// <summary>
        //  Returns the list that is associated with the specified site-relative URL.
        /// </summary>
        /// <param name="web"></param>
        /// <param name="listRelativeUrl">A string that contains the site-relative URL for a list, for example, /Lists/Announcements.</param>
        /// <returns></returns>
        public static SPList GetSPList(this SPWeb web, string listRelativeUrl)
        {
            string listUrl = listRelativeUrl[0] == '/' ? listRelativeUrl.Remove(0, 1) : listRelativeUrl;
            string resultListUrl = web.Url.EndsWith("/") ? web.Url + listUrl : string.Format("{0}/{1}", web.Url, listUrl);

            return web.GetList(resultListUrl);
        }

        /// <summary>
        /// Includes AD groups
        /// </summary>
        public static bool IsUserInGroup(this SPWeb web, string userName, int groupId)
        {
            var group = web.SiteGroups.GetByID(groupId);
            return web.IsUserInGroup(userName, group);
        }

        /// <summary>
        /// Includes AD groups
        /// </summary>
        public static bool IsUserInGroup(this SPWeb web, string userName, string groupName)
        {
            var group = web.SiteGroups[groupName];
            return web.IsUserInGroup(userName, group);
        }

        /// <summary>
        /// Includes AD groups
        /// </summary>
        public static bool IsUserInGroup(this SPWeb web, string userName, SPGroup group)
        {
            var result = false;
            userName = Utilities.Utility.DecodeUserName(userName);
            if (!string.IsNullOrEmpty(userName) && group != null)
                result = group.Users.Cast<SPUser>().Any(user => Utilities.Utility.DecodeUserName(user.LoginName.ToLower()).Equals(userName.ToLower()) || IsInAdGroup(userName, user.Name));
            return result;
        }

        private static bool IsInAdGroup(string loginName, string groupName)
        {
            var result = false;
            var principalContext = new PrincipalContext(ContextType.Domain);
            var userPrincipal = UserPrincipal.FindByIdentity(principalContext, IdentityType.SamAccountName, loginName);
            if (userPrincipal != null)
            {
                //var group = GroupPrincipal.FindByIdentity(principalContext, groupName);
                //result = group != null && userPrincipal.IsMemberOf(group);
                var userSid = userPrincipal.Sid.ToString().ToLower();
                var group = GroupPrincipal.FindByIdentity(principalContext, groupName);
                result = group != null && group.Members.Any(member => userSid.Equals(member.Sid.ToString().ToLower()));
            }
            return result;
        }



        /// <summary>
        /// Check if the current user belongs to at least one of the groups
        /// </summary>
        /// <param name="web"></param>
        /// <param name="groups"></param>
        /// <returns></returns>
        public static bool IsCurrentUserMemberOfAtLeastOneGroups(this SPWeb web, string[] groups)
        {
            bool result = false;
            try
            {
                foreach (string group in groups)
                {
                    try
                    {
                        SPGroup spGroup = web.Groups[group.Trim()];
                        result = web.IsCurrentUserMemberOfGroup(spGroup.ID);
                        if (result)
                            break;
                    }
                    catch (Exception)
                    {
                        try
                        {
                            SPGroup spGroup = web.SiteGroups[group.Trim()];
                            result = web.IsCurrentUserMemberOfGroup(spGroup.ID);
                            if (result)
                                break;
                        }
                        catch (Exception) { }
                    }
                }
            }
            catch (Exception) { }

            return result;
        }

        /// <summary>
        /// Checks if the user has permissions on a least one item, if not, removes the user permissions from the list.
        /// </summary>
        public static void RemovePermissionFromList(this SPWeb web, string listUrl, SPUser user)
        {
            using (var impSite = new SPSite(web.Url, user.UserToken))
            {
                using (var impWeb = impSite.OpenWeb())
                {
                    var list = impWeb.GetSPList(listUrl);

                    if (list.Items.Count == 0)
                    {
                        web.RunElevated(elevatedWeb =>
                        {
                            list = elevatedWeb.GetSPList(listUrl);
                            list.RoleAssignments.Remove(user);
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Check if the group exists in the site
        /// </summary>
        /// <param name="web"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static bool GroupExistsInWebSite(this SPWeb web, string name)
        {
            return web.Groups.OfType<SPGroup>().Count(g => g.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)) > 0;
        }
        /// <summary>
        /// Check if the group exists in the site collection
        /// </summary>
        /// <param name="web"></param>
        /// <param name="name"></param>
        /// <param name="grp"></param>
        /// <returns></returns>
        public static bool GroupExistsInSiteCollection(this SPWeb web, string name, out SPGroup grp)
        {
            return web.SiteGroups.GroupExists(name, out grp);
        }
        /// <summary>
        /// /// Check if the group exists in the site
        /// </summary>
        /// <param name="web"></param>
        /// <param name="name"></param>
        /// <param name="grp"></param>
        /// <returns></returns>
        public static bool GroupExistsInWebSite(this SPWeb web, string name, out SPGroup grp)
        {
            return web.Groups.GroupExists(name, out grp);
        }



    }
}