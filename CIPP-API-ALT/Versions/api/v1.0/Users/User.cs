using System.Net.Http.Headers;
using System.Text.Json;
using CIPP_API_ALT.Common;
using CIPP_API_ALT.Data.Logging;
using CIPP_API_ALT.Api.v10.Licenses;

namespace CIPP_API_ALT.Api.v10.Users
{
    public class User
    {
        public User()
        {
        }

        public string? id { get; set; }
        public bool? accountEnabled { get; set; }
        public List<object>? businessPhones { get; set; }
        public object? city { get; set; }
        public DateTime? createdDateTime { get; set; }
        public object? companyName { get; set; }
        public object? country { get; set; }
        public object? department { get; set; }
        public string? displayName { get; set; }
        public object? faxNumber { get; set; }
        public object? givenName { get; set; }
        public object? isResourceAccount { get; set; }
        public object? jobTitle { get; set; }
        public string? mail { get; set; }
        public string? mailNickname { get; set; }
        public object? mobilePhone { get; set; }
        public object? onPremisesDistinguishedName { get; set; }
        public object? officeLocation { get; set; }
        public object? onPremisesLastSyncDateTime { get; set; }
        public List<object>? otherMails { get; set; }
        public object? postalCode { get; set; }
        public object? preferredDataLocation { get; set; }
        public object? preferredLanguage { get; set; }
        public List<string>? proxyAddresses { get; set; }
        public object? showInAddressList { get; set; }
        public object? state { get; set; }
        public object? streetAddress { get; set; }
        public object? surname { get; set; }
        public object? usageLocation { get; set; }
        public string? userPrincipalName { get; set; }
        public string? userType { get; set; }
        public object? onPremisesSyncEnabled { get; set; }
        public assignedLicence[]? assignedLicenses { get; set; }
        public string? LicJoined { get; set; }
        public string? Aliases { get; set; }
        public string? primDomain { get; set; }
        public string? LastSigninApplication { get; set; }
        public string? LastSigninDate { get; set; }
        public string? LastSigninStatus { get; set; }
        public string? LastSigninResult { get; set; }
        public string? LastSigninFailureReason { get; set; }

        public struct assignedLicence
        {
            public string[]? disabledPlans { get; set; }
            public string? skuId { get; set; }
        }


        public static async Task<List<User>> GetUsers(string accessingUser = "", string tenantFilter = "", string userId = "")
        {
            using (CippLogs logsDb = new())
            {
                await logsDb.LogRequest("Accessed this API", accessingUser, "Debug", "", "ListUsers");
            }

            string selectList = "id,accountEnabled,businessPhones,city,createdDateTime,companyName,country,department,displayName,faxNumber,givenName,isResourceAccount,jobTitle,mail,mailNickname,mobilePhone,onPremisesDistinguishedName,officeLocation,onPremisesLastSyncDateTime,otherMails,postalCode,preferredDataLocation,preferredLanguage,proxyAddresses,showInAddressList,state,streetAddress,surname,usageLocation,userPrincipalName,userType,assignedLicenses,onPremisesSyncEnabled,LicJoined,Aliases,primDomain";

            List<JsonElement> returnedJson = await RequestHelper.NewGraphGetRequest(string.Format("https://graph.microsoft.com/beta/users/{0}?$top=999&$select={1}", userId, selectList), tenantFilter);
            List<User> returnUsers;

            try
            {

                returnUsers = Utilities.ParseJson<User>(returnedJson[0].EnumerateArray().ToList());
            }
            catch
            {
                returnUsers = Utilities.ParseJson<User>(new() { returnedJson[0] });
            }

            for (int i = 0; i < returnUsers.Count; i++)
            {
                returnUsers[i].LicJoined = string.Empty;
                returnUsers[i].primDomain = returnUsers[i].userPrincipalName.Split("@")[1] ?? string.Empty;

                foreach(assignedLicence al in returnUsers[i].assignedLicenses)
                {
                    if (!string.IsNullOrEmpty(al.skuId))
                    {
                        returnUsers[i].LicJoined += License.ConvertSkuName(skuId: al.skuId) + ", ";
                    }
                }

                foreach (string pa in returnUsers[i].proxyAddresses)
                {
                    returnUsers[i].Aliases += pa + ", ";
                }

                returnUsers[i].LicJoined = returnUsers[i].LicJoined.TrimEnd(new char[] { ',', ' ' });
                returnUsers[i].Aliases = returnUsers[i].Aliases.TrimEnd(new char[] { ',', ' ' });

            }

            if(!string.IsNullOrEmpty(userId))
            {
                HttpRequestMessage requestMessage = new(HttpMethod.Post,string.Format("https://login.microsoftonline.com/{0}/oauth2/token",tenantFilter));
                requestMessage.Content = new StringContent(string.Format("resource=https://admin.microsoft.com&grant_type=refresh_token&refresh_token={0}", ApiEnvironment.Secrets.ExchangeRefreshToken));
                requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                HttpResponseMessage responseMessage = await RequestHelper.SendHttpRequest(requestMessage);

                if(responseMessage.IsSuccessStatusCode)
                {
                    JsonDocument jsonDoc = await JsonDocument.ParseAsync(new MemoryStream(await responseMessage.Content.ReadAsByteArrayAsync()));
                    string accessToken = jsonDoc.RootElement.GetProperty("access_token").GetString() ?? "FAILED_TO_GET_ACCESS_TOKEN";

                    requestMessage = new(HttpMethod.Get, string.Format("https://admin.microsoft.com/admin/api/users/{0}/lastSignInInfo", userId));
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    requestMessage.Headers.TryAddWithoutValidation("x-ms-client-request-id", Guid.NewGuid().ToString());
                    requestMessage.Headers.TryAddWithoutValidation("x-ms-client-session-id", Guid.NewGuid().ToString());
                    requestMessage.Headers.TryAddWithoutValidation("x-ms-correlation-id", Guid.NewGuid().ToString());
                    requestMessage.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");

                    responseMessage = await RequestHelper.SendHttpRequest(requestMessage);

                    if(responseMessage.IsSuccessStatusCode)
                    {
                        try
                        {
                            jsonDoc = await JsonDocument.ParseAsync(new MemoryStream(await responseMessage.Content.ReadAsByteArrayAsync()));
                            returnUsers[0].LastSigninApplication = jsonDoc.RootElement.GetProperty("AppDisplayName").GetString() ?? "";
                            returnUsers[0].LastSigninDate = jsonDoc.RootElement.GetProperty("CreatedDateTime").GetDateTime().ToString() ?? "";
                            returnUsers[0].LastSigninStatus = jsonDoc.RootElement.GetProperty("Status").GetProperty("AdditionalDetails").GetString() ?? "";
                            if (jsonDoc.RootElement.GetProperty("Status").GetProperty("ErrorCode").GetUInt32() == 0)
                            {
                                returnUsers[0].LastSigninResult = "Success";
                                returnUsers[0].LastSigninFailureReason = "Success";
                            }
                            else
                            {
                                returnUsers[0].LastSigninResult = "Failure";
                                returnUsers[0].LastSigninFailureReason = jsonDoc.RootElement.GetProperty("Status").GetProperty("FailureReason").GetString() ?? ""; ;
                            }
                        }
                        catch
                        {
                            returnUsers[0].LastSigninApplication = "Failed to get";
                            returnUsers[0].LastSigninDate = DateTime.UnixEpoch.ToString();
                            returnUsers[0].LastSigninStatus = "Failed to get";
                            returnUsers[0].LastSigninResult = "Failed to get";
                            returnUsers[0].LastSigninFailureReason = "Failed to get";

                        }
                    }
                    else
                    {
                        CippLogs.DebugConsoleWrite("Did not get HTTP 200 success attempting to fetch user last signin details.");
                    }
                }
                else
                {
                    CippLogs.DebugConsoleWrite("Did not get HTTP 200 success attempting to fetch token for user last signin details.");
                }
            }

            return returnUsers;
        }

        public static async Task<bool> GetUserConditionalAccessPolicies(string tenantFilter, string userId, string httpCookeiUser = "")
        {
            using (CippLogs logsDb = new())
            {
                await logsDb.LogRequest("Accessed this API", httpCookeiUser, "Debug", "", "ListUsers");
            }

            try
            {
                string json = "{\"conditions\":{\"users\":{\"allUsers\":2,\"included\":{\"userIds\":[\""+userId+"\"]," +
                    "\"groupIds\":[]},\"excluded\":{\"userIds\":[],\"groupIds\":[]}},\"servicePrincipals\":" +
                    "{\"allServicePrincipals\":1,\"includeAllMicrosoftApps\":false,\"excludeAllMicrosoftApps\":false," +
                    "\"userActions\":[],\"stepUpTags\":[]},\"conditions\":{\"minUserRisk\":{\"noRisk\":false,\"lowRisk\":" +
                    "false,\"mediumRisk\":false,\"highRisk\":false,\"applyCondition\":false},\"minSigninRisk\":{\"noRisk\":" +
                    "false,\"lowRisk\":false,\"mediumRisk\":false,\"highRisk\":false,\"applyCondition\":false}," +
                    "\"servicePrincipalRiskLevels\":{\"noRisk\":false,\"lowRisk\":false,\"mediumRisk\":false,\"highRisk\":" +
                    "false,\"applyCondition\":false},\"devicePlatforms\":{\"all\":2,\"included\":{\"android\":false,\"ios\":" +
                    "false,\"windowsPhone\":false,\"windows\":false,\"macOs\":false,\"linux\":false},\"excluded\":" +
                    "null,\"applyCondition\":false},\"locations\":{\"applyCondition\":true,\"includeLocationType\":2," +
                    "\"excludeAllTrusted\":false},\"clientApps\":{\"applyCondition\":false,\"specificClientApps\":false," +
                    "\"webBrowsers\":false,\"exchangeActiveSync\":false,\"onlyAllowSupportedPlatforms\":false," +
                    "\"mobileDesktop\":false},\"clientAppsV2\":{\"applyCondition\":false,\"webBrowsers\":false," +
                    "\"mobileDesktop\":false,\"modernAuth\":false,\"exchangeActiveSync\":false,\"onlyAllowSupportedPlatforms\":" +
                    "false,\"otherClients\":false},\"deviceState\":{\"includeDeviceStateType\":1,\"excludeDomainJoionedDevice\":" +
                    "false,\"excludeCompliantDevice\":false,\"applyCondition\":true}}},\"country\":\"\",\"device\":{}}";

                var UserPolicies = await RequestHelper.NewClassicApiPostRequest(tenantFilter, "https://main.iam.ad.ext.azure.com/api/Policies/Evaluate?", HttpMethod.Post, json, "74658136-14ec-4630-ad9b-26e160ff0fc6"); //| Where - Object { $_.applied - eq $true })
    //$ConditionalAccessPolicyOutput = New - GraphGetRequest - uri "https://graph.microsoft.com/beta/identity/conditionalAccess/policies" - tenantid $tenantfilter
}
            catch
            {
    //$ConditionalAccessPolicyOutput = @{ }
            }

            return false;
        }
    }

}

