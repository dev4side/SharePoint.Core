using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace D4S.SharePoint.Core.SPExtensions
{
    public static class SPGroupCollectionExtensions
    {
        /// <summary>
        /// Check if the group exist
        /// </summary>
        /// <param name="groups"></param>
        /// <param name="name"></param>
        /// <param name="grp"></param>
        /// <returns></returns>
        public static bool GroupExists(this SPGroupCollection groups, string name, out SPGroup grp)
        {
            grp = groups.OfType<SPGroup>().FirstOrDefault(g => g.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            return grp != null;
        }
    }
}
