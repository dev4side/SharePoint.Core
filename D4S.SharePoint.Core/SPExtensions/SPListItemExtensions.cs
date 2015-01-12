using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;
using System.Globalization;
using System.IO;
using Microsoft.SharePoint.Utilities;

namespace D4S.SharePoint.Core.SPExtensions
{
    public static class SPListItemExtensions
    {
        /// <summary>
        /// Assign item rights 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="member">User or group to be granted rights to the item</param>
        /// <param name="roleDef">Role definition to assign</param>
        public static void AssignRights(this SPListItem item, SPPrincipal member, SPRoleDefinition roleDef)
        {
            var roleAssigment =
                item.RoleAssignments.Cast<SPRoleAssignment>().FirstOrDefault(
                    x => x.Member.ID == member.ID) ?? new SPRoleAssignment(member);

            roleAssigment.RoleDefinitionBindings.Add(roleDef);
            item.RoleAssignments.Add(roleAssigment);
        }

        /// <summary>
        /// Remove item rights 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="member">User or group to take away the rights on the item</param>
        public static void RemoveRights(this SPListItem item, SPPrincipal member)
        {
            item.RoleAssignments.Remove(member);
        }

        /// <summary>
        /// Returns item attachments as a list of SPFile
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static List<SPFile> GetAttachments(this SPListItem item)
        {
            return (from string fileName in item.Attachments select item.Web.GetFile(item.Attachments.UrlPrefix + fileName)).ToList();
        }

        /// <summary>
        /// Add the list of SPFile as attachments on the list item
        /// </summary>
        /// <param name="item"></param>
        /// <param name="attachments"></param>
        public static void AddAttachments(this SPListItem item, List<SPFile> attachments)
        {
            foreach (var attachment in attachments)
            {
                item.Attachments.Add(attachment.Name, attachment.OpenBinary());
                item.Update();
            }
        }

        /// <summary>
        /// Delete a list of attachments
        /// </summary>
        /// <param name="item"></param>
        /// <param name="fileNames"></param>
        public static void DeleteAttachments(this SPListItem item, string[] fileNames)
        {
            foreach (var fileName in fileNames)
            {
                item.Attachments.Delete(fileName);    
            }
            item.Update();
        }

        /// <summary>
        /// Copy the attachments of the current item on another item
        /// </summary>
        /// <param name="fromItem"></param>
        /// <param name="toItem">Destination item</param>
        public static void CopyAttachmentsTo(this SPListItem fromItem, SPListItem toItem)
        {
            CopyAttachments(fromItem, toItem, false);
        }

        /// <summary>
        /// Move the attachments of the current item on another item
        /// </summary>
        /// <param name="fromItem"></param>
        /// <param name="toItem">Destination item</param>
        public static void MoveAttachmentsTo(this SPListItem fromItem, SPListItem toItem)
        {
            CopyAttachments(fromItem, toItem, true);
        }


        public static IEnumerable<string> GetAttachmentUrls(this SPListItem item)
        {
            return from string fileName in item.Attachments
                   orderby fileName
                   select SPUrlUtility.CombineUrl(item.Attachments.UrlPrefix, fileName);
        }

        private static void CopyAttachments(SPListItem fromItem, SPListItem toItem, bool move)
        {
            if (fromItem.Attachments.Count > 0)
            {
                var folder =
                    fromItem.Web.Folders["Lists"].SubFolders[fromItem.ParentList.Title].SubFolders["Attachments"].SubFolders[fromItem.ID.ToString()];
                Stream stream = null;
                byte[] buffer = null;
                foreach (SPFile file in folder.Files)
                {
                    stream = file.OpenBinaryStream();
                    buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, (int)stream.Length);
                    stream.Close();
                    stream.Dispose();
                    toItem.Attachments.Add(file.Name, buffer);
                }

                if (move)
                {
                    int num = fromItem.Attachments.Count;
                    if (num > 0)
                    {
                        for (int i = 0; i < num; i++)
                        {
                            fromItem.Attachments.Delete(fromItem.Attachments[0]);
                        }
                        fromItem.Update();
                    }
                }

                toItem.Update();
            }
        }

        #region "GetValues"

        public static T GetValue<T>(this SPListItem item, string fieldName, T defaultValue)
        {
            T retVal = defaultValue;
            object fieldValue = item[item.Fields.GetFieldByInternalName(fieldName).Id];
            if (fieldValue != null)
            {
                Type nullsafe = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
                retVal = (T)Convert.ChangeType(fieldValue, nullsafe, CultureInfo.InvariantCulture);
            }
            return retVal;
        }

        public static string GetStringValue(this SPListItem item, string internalName)
        {
            if (item != null)
                return item[item.Fields.GetFieldByInternalName(internalName).Id] != null ? item[item.Fields.GetFieldByInternalName(internalName).Id].ToString() : string.Empty;
            else
                return null;
        }

        public static int GetIntValue(this SPListItem item, string internalName)
        {
            if (item != null)
            {
                int tempVal = 0;
                return item[item.Fields.GetFieldByInternalName(internalName).Id] != null && int.TryParse(item[item.Fields.GetFieldByInternalName(internalName).Id].ToString(), out tempVal)
                ? tempVal : 0;
            }
            else
                return 0;
        }

        public static SPFieldLookupValue GetLookup(this SPListItem item, string internalName)
        {
            var value = new SPFieldLookupValue();
            var field = item.Fields.GetFieldByInternalName(internalName) as SPFieldLookup;
            if (field != null && item[field.Id] != null)
            {
                var objField = item[field.Id];
                if (objField != null)
                {
                    var fieldValue = field.GetFieldValue(objField.ToString()) as SPFieldLookupValue;
                    if (fieldValue != null)
                    {
                        value = fieldValue;
                    }
                }
            }
            return value;
        }

        public static string GetLookupValue(this SPListItem item, string internalName)
        {
            string value = string.Empty;
            var field = item.Fields.GetFieldByInternalName(internalName) as SPFieldLookup;
            if (field != null && item[field.Id] != null)
            {
                var objField = item[field.Id];
                if (objField != null)
                {
                    var fieldValue = field.GetFieldValue(objField.ToString()) as SPFieldLookupValue;
                    if (fieldValue != null)
                    {
                        value = fieldValue.LookupValue;
                    }
                }
            }
            return value;
        }

        public static int GetLookupIdValue(this SPListItem item, string internalName)
        {
            int value = 0;
            var field = item.Fields.GetFieldByInternalName(internalName) as SPFieldLookup;
            if (field != null && item[field.Id] != null)
            {
                var objField = item[field.Id];
                if (objField != null)
                {
                    var fieldValue = field.GetFieldValue(objField.ToString()) as SPFieldLookupValue;
                    if (fieldValue != null)
                    {
                        value = fieldValue.LookupId;
                    }
                }
            }
            return value;
        }

        public static Dictionary<int, string> GetLookupMultiValue(this SPListItem item, string internalName)
        {
            Dictionary<int, string> values = new Dictionary<int, string>();
            var field = item.Fields.GetFieldByInternalName(internalName) as SPFieldLookup;
            if (field != null && item[field.Id] != null)
            {
                var objField = item[field.Id];
                if (objField != null)
                {
                    var fieldValue = field.GetFieldValue(objField.ToString()) as SPFieldLookupValueCollection;
                    SPFieldLookupValueCollection LookUpItemCollection = new SPFieldLookupValueCollection(item[internalName].ToString());
                    for (int i = 0; i < LookUpItemCollection.Count; i++)
                    {
                        SPFieldLookupValue Item = LookUpItemCollection[i];
                        values.Add(Item.LookupId, Item.LookupValue);
                    }
                }
            }
            return values;
        }

        public static DateTime GetDateTimeValue(this SPListItem item, string internalName)
        {
            DateTime date = DateTime.MinValue;
            DateTime result = DateTime.MinValue;
            if (item != null)
            {
                if (item[item.Fields.GetFieldByInternalName(internalName).Id] != null && DateTime.TryParse(item[item.Fields.GetFieldByInternalName(internalName).Id].ToString(), out date))
                    result = date;
            }
            return result;
        }

        public static bool GetBoolValue(this SPListItem item, string internalName)
        {
            if (item != null)
                return item[item.Fields.GetFieldByInternalName(internalName).Id] != null ? Convert.ToBoolean(item[item.Fields.GetFieldByInternalName(internalName).Id].ToString()) : false;
            else
                return false;
        }

        public static SPUser GetUserValue(this SPListItem item, string internalName)
        {
            var field = item.Fields.GetFieldByInternalName(internalName) as SPFieldUser;

            if (field != null && item[field.Id] != null)
            {
                var fieldValue = field.GetFieldValue(item[field.Id].ToString()) as SPFieldUserValue;
                if (fieldValue != null)
                {
                    return fieldValue.User;
                }
            }
            return null;
        }

        public static List<SPUser> GetUserMultiValue(this SPListItem item, string internalName)
        {
            var result = new List<SPUser>();
            var field = item.Fields.GetFieldByInternalName(internalName) as SPFieldUser;

            if (field != null && item[field.Id] != null)
            {
                var fieldValue = field.GetFieldValue(item[field.Id].ToString()) as SPFieldUserValueCollection;
                if (fieldValue != null)
                {
                    result.AddRange(fieldValue.Select(x => x.User).ToList());
                }
            }
            return result;
        }

        public static string GetMultiChoiceValue(this SPListItem item, string internalName, string separator)
        {
            string result = string.Empty;
            if (item[item.Fields.GetFieldByInternalName(internalName).Id] != null)
            {
                string rawVal = item[item.Fields.GetFieldByInternalName(internalName).Id].ToString();
                SPFieldMultiChoiceValue fieldValue = new SPFieldMultiChoiceValue(rawVal);
                for (int i = 0; i < fieldValue.Count; i++)
                {
                    result += fieldValue[i] + ((i + 1) == fieldValue.Count ? " " : separator);
                }
            }
            return result;
        }

        #endregion

        #region "SetValues"

        public static void SetValue(this SPListItem item, string internalName, string value)
        {
            item[item.Fields.GetFieldByInternalName(internalName).Id] = value;
        }

        public static void SetValue(this SPListItem item, string internalName, int value)
        {
            item[item.Fields.GetFieldByInternalName(internalName).Id] = value;
        }

        public static void SetValue(this SPListItem item, string internalName, double value)
        {
            item[item.Fields.GetFieldByInternalName(internalName).Id] = value;
        }

        public static void SetValue(this SPListItem item, string internalName, bool value)
        {
            item[item.Fields.GetFieldByInternalName(internalName).Id] = value;
        }

        public static void SetValue(this SPListItem item, string internalName, SPUser user, bool ensureUser)
        {
            if (ensureUser)
            {
                var localUser = item.Web.EnsureUser(user.LoginName);
                item[item.Fields.GetFieldByInternalName(internalName).Id] = new SPFieldUserValue(item.Web, localUser.ID, localUser.LoginName);
            }
            else
            {
                item[item.Fields.GetFieldByInternalName(internalName).Id] = new SPFieldUserValue(user.ParentWeb, user.ID, user.LoginName);
            }
        }

        public static void SetValue(this SPListItem item, string internalName, int ID, string value)
        {
            item[item.Fields.GetFieldByInternalName(internalName).Id] = new SPFieldLookupValue(ID, value);
        }

        public static void SetValue(this SPListItem item, string internalName, SPFieldLookupValue value)
        {
            item[item.Fields.GetFieldByInternalName(internalName).Id] = value;
        }

        public static void SetValue(this SPListItem item, string internalName, DateTime value)
        {
            item[item.Fields.GetFieldByInternalName(internalName).Id] = value;
        }

        public static void SetValue(this SPListItem item, string internalName, DateTime? value)
        {
            item[item.Fields.GetFieldByInternalName(internalName).Id] = value;
        }

        public static void SetValue(this SPListItem item, string internalName, SPFieldLookupValueCollection value)
        {
            item[item.Fields.GetFieldByInternalName(internalName).Id] = value;
        }

        public static void SetUserValue(this SPListItem item, string internalName, int userId)
        {
            var spUser = item.Web.SiteUsers.GetByID(userId);

            if (spUser != null)
            {
                item[item.Fields.GetFieldByInternalName(internalName).Id] = new SPFieldUserValue(spUser.ParentWeb, spUser.ID, spUser.LoginName);
            }
        }

        #endregion


        #region "Utils"

        public static bool SPFielMultiValueContainsValue(this SPListItem item, string internalName, string value)
        {
            SPFieldLookupValueCollection values = item[item.Fields.GetFieldByInternalName(internalName).Id] as SPFieldLookupValueCollection;
            return values.Where(v => v.LookupValue.ToUpper() == value.ToUpper()).Count() > 0;
        }

        public static bool SPFielMultiValueContainsId(this SPListItem item, string internalName, int id)
        {
            SPFieldLookupValueCollection values = item[item.Fields.GetFieldByInternalName(internalName).Id] as SPFieldLookupValueCollection;
            return values.Where(v => v.LookupId == id).Count() > 0;
        }

        #endregion

    }
}
