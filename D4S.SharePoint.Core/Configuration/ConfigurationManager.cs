using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.Utilities;
using System.Xml.Linq;

namespace D4S.SharePoint.Core.Configuration
{
    public class ConfigurationManager
    {
        /// <summary>
        /// Legge il valore della chiave presente nel file di configurazione presente sotto un dato percorso
        /// </summary>
        /// <param name="key">chiave</param>
        /// <param name="configPath">percorso del file di configurazione</param>
        /// <returns>valore</returns>
        public static string GetValue(string key, string configPath)
        {
            string value = string.Empty;
            string spSetupPath = SPUtility.GetGenericSetupPath(string.Empty) + configPath;

            XElement xfile = XElement.Load(spSetupPath);
            value = xfile.Descendants("add")
                            .Where(v => v.Attribute("key").Value == key)
                            .Select(v => v.Attribute("value").Value).FirstOrDefault();

            return value;
        }
    }
}
