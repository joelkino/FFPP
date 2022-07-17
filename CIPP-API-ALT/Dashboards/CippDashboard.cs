using CIPP_API_ALT.Data.Logging;
using CIPP_API_ALT.Tenants;
using CIPP_API_ALT.Common;

namespace CIPP_API_ALT.Dashboards
{
    public class CippDashboard
    {
        public CippDashboard()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async static Task<DashStats> GetHomeData()
        {
            List<DashLogEntry> last10Logs = new();

            foreach (CippLogs.LogEntry log in CippLogs.LogDb.Top10LogEntries())
            {
                last10Logs.Add(new DashLogEntry() { Tenant = log.Tenant, Message = log.Message });
            }

            return new DashStats()
            {
                NextStandardsRun = DateTime.UtcNow.AddHours(3).ToString("yyyy-MM-ddThh:mm:ss"),
                NextBPARun = DateTime.UtcNow.AddHours(3).ToString("yyyy-MM-ddThh:mm:ss"),
                QueuedApps = 0,
                QueuedStandards = 0,
                TenantCount = (await Tenant.GetTenants(allTenantSelector: false)).Count,
                RefreshTokenDate = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd"),
                ExchangeTokenDate = DateTime.UtcNow.AddDays(7).ToString("yyyy-MM-dd"),
                LastLog = last10Logs,
                Alerts = AlertChecks()
            };
        }

        /// <summary>
        /// Describes the data sent back for the /api/GetDashboard query
        /// </summary>
        public struct DashStats
        {
            public string NextStandardsRun { get; set; }
            public string NextBPARun { get; set; }
            public string NextDomainsRun { get; set; }
            public int QueuedApps { get; set; }
            public int QueuedStandards { get; set; }
            public int TenantCount { get; set; }
            public string RefreshTokenDate { get; set; }
            public string ExchangeTokenDate { get; set; }
            public List<DashLogEntry> LastLog { get; set; }
            public List<string> Alerts { get; set; }
        }

        /// <summary>
        /// Holds Tenant:Message values of a log entry for the Dashboard to consume
        /// </summary>
        public struct DashLogEntry
        {
            public string Tenant { get; set; }
            public string Message { get; set; }
        }

        public static async Task<Versions> CheckVersions(string cippVersion)
        {
            string rawApiVersion = File.ReadLines(ApiEnvironment.ApiVersionFile).First();
            Version apiVersion = Version.Parse(rawApiVersion.Split(":")[0]);
            string displayApiVersion = string.Format("{0}-{1} ({2})", apiVersion, rawApiVersion.Split(":")[1], rawApiVersion.Split(":")[2]);

            HttpRequestMessage requestMessage = new(HttpMethod.Get, ApiEnvironment.RemoteCippAltApiVersion);
            HttpResponseMessage responseMessage = await RequestHelper.SendHttpRequest(requestMessage);

            string remoteRawApiVersion = await responseMessage.Content.ReadAsStringAsync();
            Version remoteApiVersion = Version.Parse(remoteRawApiVersion.Split(":")[0]);
            string displayRemoteApiVersion = string.Format("{0}-{1} ({2})", remoteApiVersion, remoteRawApiVersion.Split(":")[1], remoteRawApiVersion.Split(":")[2]);

            requestMessage = new(HttpMethod.Get, ApiEnvironment.RemoteCippVersion);
            responseMessage = await RequestHelper.SendHttpRequest(requestMessage);

            Version remoteCippVersion = Version.Parse(await responseMessage.Content.ReadAsStringAsync());

            return new Versions()
            {
                LocalCippVersion = cippVersion,
                RemoteCippVersion = remoteCippVersion.ToString(),
                LocalCippApiVersion = displayApiVersion,
                RemoteCippApiVersion = displayRemoteApiVersion,
                OutOfDateCipp = remoteCippVersion > Version.Parse(cippVersion),
                OutOfDateCippApi = remoteApiVersion > apiVersion

            };
        }

        public struct Versions
        {
            public string LocalCippVersion { get; set; }
            public string RemoteCippVersion { get; set; }
            public string LocalCippApiVersion { get; set; }
            public string RemoteCippApiVersion { get; set; }
            public bool OutOfDateCipp { get; set; }
            public bool OutOfDateCippApi { get; set; }
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
            alerts.Add("Fictitious alert! Don't tell Scotty, Scotty doesn't know, Scotty doesn't knowohoh!");
            alerts.Add("Too old to live, too young to dieieie!");
            alerts.Add("Who lives in a pineapple under the sea? Absorbant and yellow and porous is he! If nautical nonsense is " +
                "something you wish, get down on the deck and flop like a fish! Sponge bob square pants, sponge bob square pants" +
                "spooooonge booooob..... squaaare paaaaaaantsssss");

            return alerts;
        }
        #endregion
    }
}

