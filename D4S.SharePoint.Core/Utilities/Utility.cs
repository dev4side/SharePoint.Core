﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint;

namespace D4S.SharePoint.Core.Utilities
{
    public class Utility
    {
        public delegate void CodeToRunElevated(SPWeb elevatedWeb);

        public static void RunElevated(string webUrl, CodeToRunElevated secureCode)
        {
            if (string.IsNullOrEmpty(webUrl))
                return;
            
            SPSecurity.RunWithElevatedPrivileges(() =>
            {
                using (var site = new SPSite(webUrl))
                {
                    try
                    {
                        site.AllowUnsafeUpdates = true;
                        using (var elevatedWeb = site.OpenWeb())
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
    }
}
