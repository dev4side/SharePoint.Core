using System;
using System.Collections.Generic;
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
                    catch (Exception) {
                        try
                        {
                            SPGroup spGroup = web.SiteGroups[group.Trim()];
                            result = web.IsCurrentUserMemberOfGroup(spGroup.ID);
                            if (result)
                                break;
                        }
                        catch (Exception) {}
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
    }
}
