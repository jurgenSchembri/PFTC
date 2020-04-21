using Google.Api;
using Google.Cloud.Logging.Type;
using Google.Cloud.Logging.V2;
using Google.Cloud.Diagnostics.AspNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1.DataAccess
{
    public class LoggingRepository
    {
        public void ErrorLogging(Exception e)
        {
            string projectId = "jurgen-cloud-project";
            string serviceName = "PFTCAssignment";
            string version = "1";
            var exceptionError = GoogleExceptionLogger.Create(projectId, serviceName, version);

            exceptionError.Log(e);
        }

        public void Logging(string message)
        {
            var logId = "PFTC_logs";
            var client = LoggingServiceV2Client.Create();
            LogName logName = new LogName("jurgen-cloud-project", logId);
            LogEntry logEntry = new LogEntry
            {
                LogName = logName.ToString(),
                Severity = LogSeverity.Info,
                TextPayload = $"{message}"
            };
            MonitoredResource resource = new MonitoredResource { Type = "global" };

            client.WriteLogEntries(logName, resource, null, new[] { logEntry });
        }
    }
}