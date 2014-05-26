using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.Administration;

namespace D4S.SharePoint.Core.Logs
{
    public class LoggingService : SPDiagnosticsServiceBase
    {
        public static string DiagnosticAreaName = "D4S";
        private static LoggingService _Current;
        public static LoggingService Current
        {
            get
            {
                if (_Current == null)
                {
                    _Current = new LoggingService();
                }

                return _Current;
            }
        }

        private LoggingService()
            : base(DiagnosticAreaName, SPFarm.Local)
        { }

        protected override IEnumerable<SPDiagnosticsArea> ProvideAreas()
        {
            List<SPDiagnosticsArea> areas = new List<SPDiagnosticsArea>
        {
            new SPDiagnosticsArea(DiagnosticAreaName, new List<SPDiagnosticsCategory>
            {
                new SPDiagnosticsCategory("D4S", TraceSeverity.Unexpected, EventSeverity.Error)
            })
        };

            return areas;
        }

        public static void LogError(Exception ex, string place)
        {
            LogError(ex, place, false);
        }

        public static void LogError(Exception ex, bool writeStack)
        {
            LogError(ex, ex.TargetSite.Name, writeStack);
        }

        public static void LogError(Exception ex, string place, bool writeStack)
        {
            LogError(ex, place, string.Empty, writeStack);
        }

        public static void LogError(Exception ex, string place, string customMessage, bool writeStack)
        {
            if (customMessage != string.Empty)
                LogError("D4S", place, string.Format("[{0}]", customMessage));
            LogError("D4S", place, ex.Message);
            if (writeStack)
                LogError("D4S", place, ex.StackTrace);
        }

        public static void LogError(string categoryName, string place, string errorMessage)
        {
            SPDiagnosticsCategory category = LoggingService.Current.Areas[DiagnosticAreaName].Categories[categoryName];
            LoggingService.Current.WriteTrace(0, category, TraceSeverity.Unexpected, string.Format("[{0}]: {1}", place, errorMessage));
        }

        public static void LogInfo(string message, string place)
        {
            LogInfo("D4S", place, message);
        }

        public static void LogInfo(string categoryName, string place, string message)
        {
            SPDiagnosticsCategory category = LoggingService.Current.Areas[DiagnosticAreaName].Categories[categoryName];
            LoggingService.Current.WriteTrace(0, category, TraceSeverity.Medium, string.Format("[{0}]: {1}", place, message));
        }
    }
}
