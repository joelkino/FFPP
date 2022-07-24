using CIPP_API_ALT.Data.Logging;
using CIPP_API_ALT.Api.v10.Tenants;
using CIPP_API_ALT.Common;

namespace CIPP_API_ALT.Api.v10.Dashboards
{
    public class CippDashboard
    {
        public string? NextStandardsRun { get; set; }
        public string? NextBPARun { get; set; }
        public string? NextDomainsRun { get; set; }
        public int? queuedApps { get; set; }
        public int? queuedStandards { get; set; }
        public int? tenantCount { get; set; }
        public string? RefreshTokenDate { get; set; }
        public string? ExchangeTokenDate { get; set; }
        public List<DashLogEntry>? LastLog { get; set; }
        public List<string>? Alerts { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async static Task<CippDashboard> GetHomeData(string accessingUser)
        {
            using (CippLogs logsDb = new())
            {
                await logsDb.LogRequest("Accessed this API", accessingUser, "Debug", "", "GetDashboard");
            }

            List<DashLogEntry> last10Logs = new();

            using (CippLogs logDb = new())
            {
                foreach (CippLogs.LogEntry log in logDb.Top10LogEntries())
                {
                    last10Logs.Add(new DashLogEntry() { Tenant = log.Tenant ?? string.Empty, Message = log.Message ?? string.Empty });
                }
            }

            return new CippDashboard()
            {
                NextStandardsRun = DateTime.UtcNow.AddHours(3).ToString("yyyy-MM-ddThh:mm:ss"),
                NextBPARun = DateTime.UtcNow.AddHours(3).ToString("yyyy-MM-ddThh:mm:ss"),
                queuedApps = 0,
                queuedStandards = 0,
                tenantCount = (await Tenant.GetTenants(string.Empty, allTenantSelector: false)).Count,
                RefreshTokenDate = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd"),
                ExchangeTokenDate = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd"),
                LastLog = last10Logs,
                Alerts = AlertChecks()
            };
        }

        /// <summary>
        /// Holds Tenant:Message values of a log entry for the Dashboard to consume
        /// </summary>
        public struct DashLogEntry
        {
            public string Tenant { get; set; }
            public string Message { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cippVersion"></param>
        /// <returns></returns>
        public static async Task<Versions> CheckVersions(string accessingUser, string cippVersion)
        {
            using (CippLogs logsDb = new())
            {
                await logsDb.LogRequest("Accessed this API", accessingUser, "Debug", "", "GetVersion");
            }

            ApiEnvironment.CippVersion cippApiVersion = ApiEnvironment.GetApiBinaryVersion();

            HttpRequestMessage requestMessage = new(HttpMethod.Get, ApiEnvironment.RemoteCippAltApiVersion);
            HttpResponseMessage responseMessage = await RequestHelper.SendHttpRequest(requestMessage);

            ApiEnvironment.CippVersion remoteApiVersion = new(await responseMessage.Content.ReadAsStringAsync());

            requestMessage = new(HttpMethod.Get, ApiEnvironment.RemoteCippVersion);
            responseMessage = await RequestHelper.SendHttpRequest(requestMessage);

            Version remoteCippVersion = Version.Parse(await responseMessage.Content.ReadAsStringAsync());

            return new Versions()
            {
                LocalCIPPVersion = cippVersion,
                RemoteCIPPVersion = remoteCippVersion.ToString(),
                LocalCIPPAPIVersion = cippApiVersion.DisplayVersion,
                RemoteCIPPAPIVersion = remoteApiVersion.DisplayVersion,
                OutOfDateCIPP = remoteCippVersion > Version.Parse(cippVersion),
                OutOfDateCIPPAPI = remoteApiVersion.Version > cippApiVersion.Version
            };
        }

        public struct Versions
        {
            public string LocalCIPPVersion { get; set; }
            public string RemoteCIPPVersion { get; set; }
            public string LocalCIPPAPIVersion { get; set; }
            public string RemoteCIPPAPIVersion { get; set; }
            public bool OutOfDateCIPP { get; set; }
            public bool OutOfDateCIPPAPI { get; set; }
        }

        #region Private Methods
        // Checks for alert conditions then builds alerts if the conditions are satisfied
        private static List<string> AlertChecks()
        {
            List<string> alerts = new();

            // ApplicationId does not exist - setup SAM (Azure AD Enterprise Application)
            if (ApiEnvironment.Secrets.ApplicationId == null || ApiEnvironment.Secrets.ApplicationId.ToLower().Equals("longapplicationid"))
            {
                alerts.Add("You have not yet setup your SAM (Azure AD Enterprise Application). Please go to the SAM Wizard in settings to finish setup.");
            }

            // Fake alerts, uncomment when you want to test

            alerts.Add("I can't beleive it's not butter!");
            alerts.Add("I really can't beleive it's not butter!");
            alerts.Add("I still can't beleive it's not butter!");
            alerts.Add("Ok, it's not butter.");
            alerts.Add("It is a fact that Kelvin hates camels.");

            return alerts;
        }
        #endregion
    }
}

