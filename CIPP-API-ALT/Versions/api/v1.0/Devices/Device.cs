using System.Text.Json;
using CIPP_API_ALT.Data.Logging;
using CIPP_API_ALT.Common;

namespace CIPP_API_ALT.Api.v10.Devices
{
    public class Device
    {
        public static async Task<List<UserDevice>> GetUserDevices(string tenantFilter, string userId, string httpCookieUser = "")
        {
            List<UserDevice> userDevices = new();

            using (CippLogs logsDb = new())
            {
                await logsDb.LogRequest("Accessed this API", httpCookieUser, "Debug", "", "ListUserDevices");
            }

            string? GetEpmId(string deviceId, List<JsonElement> epmDevices )
            {
                try
                {
                    return epmDevices.Find(x => x.GetProperty("azureADDeviceId").GetString().ToLower().Equals(deviceId.ToLower())).GetProperty("id").GetString();
                }
                catch
                {
                    
                }

                return null;
            }

            try
            {
                var epmDevices = RequestHelper.NewGraphGetRequest(string.Format("https://graph.microsoft.com/beta/users/{0}/managedDevices",userId), tenantFilter);
                var devices = await RequestHelper.NewGraphGetRequest(string.Format("https://graph.microsoft.com/beta/users/{0}/ownedDevices?$top=999",userId), tenantFilter);

                foreach (JsonElement j in devices)
                {
                    bool premSync;

                    try
                    {
                        premSync = j.GetProperty("onPremisesSyncEnabled").GetBoolean();
                    }
                    catch
                    {
                        premSync = false;
                    }

                    userDevices.Add(new()
                    {
                        ID = j.GetProperty("id").GetString(),
                        accountEnabled = j.GetProperty("accountEnabled").GetBoolean(),
                        approximateLastSignInDateTime = j.GetProperty("approximateLastSignInDateTime").GetString(),
                        createdDateTime = j.GetProperty("createdDateTime").GetString(),
                        deviceOwnership = j.GetProperty("deviceOwnership").GetString(),
                        displayName = j.GetProperty("displayName").GetString(),
                        enrollmentType = j.GetProperty("enrollmentType").GetString(),
                        isCompliant = j.GetProperty("isCompliant").GetBoolean(),
                        managementType = j.GetProperty("managementType").GetString(),
                        manufacturer = j.GetProperty("manufacturer").GetString(),
                        model = j.GetProperty("model").GetString(),
                        operatingSystem = j.GetProperty("operatingSystem").GetString(),
                        onPremisesSyncEnabled = premSync,
                        operatingSystemVersion = j.GetProperty("operatingSystemVersion").GetString(),
                        trustType = j.GetProperty("trustType").GetString(),
                        EPMID = GetEpmId(j.GetProperty("deviceId").GetString(), epmDevices.Result)
                    });
                }
            }
            catch
            {

            }

            return userDevices;
        }

        public struct UserDevice
        {
            public string? ID { get; set; }
            public bool? accountEnabled { get; set; }
            public string? approximateLastSignInDateTime { get; set; }
            public string? createdDateTime { get; set; }
            public string? deviceOwnership { get; set; }
            public string? displayName { get; set; }
            public string? enrollmentType { get; set; }
            public bool? isCompliant { get; set; }
            public string? managementType { get; set; }
            public string? manufacturer { get; set; }
            public string? model { get; set; }
            public string? operatingSystem { get; set; }
            public bool? onPremisesSyncEnabled { get; set; }
            public string? operatingSystemVersion { get; set; }
            public string? trustType { get; set; }
            public string? EPMID { get; set; }
        }
    }
}

