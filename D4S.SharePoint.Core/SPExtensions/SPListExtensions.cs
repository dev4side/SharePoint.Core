using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace D4S.SharePoint.Core.SPExtensions
{
    public static class SPListExtensions
    {
        public static IEnumerable<SPListItem> AsEnumerable(this SPList list)
        {
            return list.Items.Cast<SPListItem>();
        }

        public static void AssignRights(this SPList list, SPPrincipal member, SPRoleDefinition roleDef)
        {
            var roleAssigment =
                list.RoleAssignments.Cast<SPRoleAssignment>().FirstOrDefault(
                    x => x.Member.ID == member.ID) ?? new SPRoleAssignment(member);

            roleAssigment.RoleDefinitionBindings.Add(roleDef);
            list.RoleAssignments.Add(roleAssigment);
        }

        /// <summary>
        /// Get the field display namee
        /// </summary>
        /// <param name="list"></param>
        /// <param name="internalName">internal field name</param>
        /// <returns></returns>
        public static string GetFieldDisplayName(this SPList list, string internalName)
        {
            return list.Fields.GetFieldByInternalName(internalName).Title;
        }
    }
}
