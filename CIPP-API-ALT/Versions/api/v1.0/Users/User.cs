﻿using System.Net.Http.Headers;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accessingUser"></param>
        /// <param name="tenantFilter"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
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

                returnUsers = Utilities.ParseJson<User>(returnedJson);
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

                try
                {
                    returnUsers[i].LicJoined = returnUsers[i].LicJoined.TrimEnd(new char[] { ',', ' ' });
                }
                catch
                {
                    returnUsers[i].LicJoined = @"{}";
                }

                try
                {
                    returnUsers[i].Aliases = returnUsers[i].Aliases.TrimEnd(new char[] { ',', ' ' });
                }
                catch
                {
                    returnUsers[i].Aliases = @"{}";
                }

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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantFilter"></param>
        /// <param name="userId"></param>
        /// <param name="httpCookieUser"></param>
        /// <returns></returns>
        public static async Task<List<ConditionalAccessPolicy>> GetUserConditionalAccessPolicies(string tenantFilter, string userId, string httpCookieUser = "")
        {
            List<ConditionalAccessPolicy> policies = new();

            using (CippLogs logsDb = new())
            {
                await logsDb.LogRequest("Accessed this API", httpCookieUser, "Debug", "", "ListUsers");
            }

            List<JsonElement> capArrays;
            List<JsonElement> userPolicies = new();

            try
            {
                object json = JsonSerializer.Deserialize<object>(@"{""conditions"":{""users"":{""allUsers"":2,""included"":{""userIds"":[""" +userId+@"""],""groupIds"":[]},""excluded"":{""userIds"":[],""groupIds"":[]}},""servicePrincipals"":{""allServicePrincipals"":1,""includeAllMicrosoftApps"":false,""excludeAllMicrosoftApps"":false,""userActions"":[],""stepUpTags"":[]},""conditions"":{""minUserRisk"":{""noRisk"":false,""lowRisk"":false,""mediumRisk"":false,""highRisk"":false,""applyCondition"":false},""minSigninRisk"":{""noRisk"":false,""lowRisk"":false,""mediumRisk"":false,""highRisk"":false,""applyCondition"":false},""servicePrincipalRiskLevels"":{""noRisk"":false,""lowRisk"":false,""mediumRisk"":false,""highRisk"":false,""applyCondition"":false},""devicePlatforms"":{""all"":2,""included"":{""android"":false,""ios"":false,""windowsPhone"":false,""windows"":false,""macOs"":false,""linux"":false},""excluded"":null,""applyCondition"":false},""locations"":{""applyCondition"":true,""includeLocationType"":2,""excludeAllTrusted"":false},""clientApps"":{""applyCondition"":false,""specificClientApps"":false,""webBrowsers"":false,""exchangeActiveSync"":false,""onlyAllowSupportedPlatforms"":false,""mobileDesktop"":false},""clientAppsV2"":{""applyCondition"":false,""webBrowsers"":false,""mobileDesktop"":false,""modernAuth"":false,""exchangeActiveSync"":false,""onlyAllowSupportedPlatforms"":false,""otherClients"":false},""deviceState"":{""includeDeviceStateType"":1,""excludeDomainJoionedDevice"":false,""excludeCompliantDevice"":false,""applyCondition"":true}}},""country"":"""",""device"":{}}");
                foreach (JsonElement j in (await RequestHelper.NewClassicApiPostRequest(tenantFilter, "https://main.iam.ad.ext.azure.com/api/Policies/Evaluate?", HttpMethod.Post, json, "74658136-14ec-4630-ad9b-26e160ff0fc6")).EnumerateArray())
                {
                    if(j.GetProperty("applied").GetBoolean())
                    {
                        userPolicies.Add(j);
                    }
                }

                List<JsonElement> conditionalAccessPolicyOutput = await RequestHelper.NewGraphGetRequest("https://graph.microsoft.com/beta/identity/conditionalAccess/policies", tenantFilter);
                capArrays = Utilities.ParseJson<JsonElement>(conditionalAccessPolicyOutput);

            }
            catch
            {
                capArrays = new();
            }

            foreach ( JsonElement cap in capArrays)
            {
                if (userPolicies.FindAll(x => x.GetProperty("policyId").GetString().Equals(cap.GetProperty("id").GetString())).Count >= 1)
                {
                    policies.Add(new ConditionalAccessPolicy() { id = cap.GetProperty("id").GetString(), displayName = cap.GetProperty("displayName").GetString() });
                }
            }

            return policies;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantFilter"></param>
        /// <param name="type"></param>
        /// <param name="userUpn"></param>
        /// <param name="httpCookieUser"></param>
        /// <returns></returns>
        public async static Task<List<OneDriveSiteReport>> GetSites(string tenantFilter, string type, string userUpn, string httpCookieUser = "")
        {
            using (CippLogs logsDb = new())
            {
                await logsDb.LogRequest("Accessed this API", httpCookieUser, "Debug", "", "ListSites");
            }
            var report = await RequestHelper.NewGraphGetRequestString(string.Format("https://graph.microsoft.com/beta/reports/get{0}Detail(period='D7')",type),tenantFilter);
            string filename = ApiEnvironment.CacheDir + "/" + string.Format("/site.report.{0}.{1}", DateTime.UtcNow.ToString("yyMMddhhmmss"), userUpn);
            await File.WriteAllTextAsync(filename,report);

            List<OneDriveSiteReport> outRes = Utilities.CsvToObjectList<OneDriveSiteReport>(filename, true);

            if(!userUpn.Equals(string.Empty))
            {
                outRes = new() { outRes.Find(x => x.UPN.ToLower().Equals(userUpn.ToLower())) };
            }

            try
            {
                File.Delete(filename);
            }
            catch
            {
                CippLogs.DebugConsoleWrite(string.Format("Couldn't delete cached file {0}", filename));
            }
            return outRes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tenantFilter"></param>
        /// <param name="userId"></param>
        /// <param name="accessingUser"></param>
        /// <returns></returns>
        public static async Task<List<UserMailboxDetails>> GetUserMailboxDetails(string tenantFilter, string userId, string accessingUser)
        {
            bool blockedForSpam = false;
            List<MailboxUserPermissions> userPerms = new();

            using (CippLogs logsDb = new())
            {
                await logsDb.LogRequest("Accessed this API", accessingUser, "Debug", "", "ListUserMailboxDetails");
            }

            Task<List<JsonElement>> casResponseTask = RequestHelper.NewGraphGetRequest(string.Format("https://outlook.office365.com/adminapi/beta/{0}/CasMailbox('{1}')", tenantFilter, userId), tenantFilter, "exchangeonline", noPagination: true);
            List<JsonElement> mailResponse = await RequestHelper.NewGraphGetRequest(string.Format("https://outlook.office365.com/adminapi/beta/{0}/Mailbox('{1}')", tenantFilter, userId), tenantFilter, "exchangeonline", noPagination: true);
            string email = mailResponse[0].GetProperty("PrimarySmtpAddress").GetString();
            Task<JsonElement> mailboxDetailedRequest = RequestHelper.NewExoRequest(tenantFilter, "Get-Mailbox", JsonSerializer.Deserialize<JsonElement>(@"{""anr"":""" + email + @"""}"));
            Task<JsonElement> blockedSender = RequestHelper.NewExoRequest(tenantFilter, "Get-BlockedSenderAddress", JsonSerializer.Deserialize<JsonElement>(@"{""SenderAddress"":""" + email + @"""}"));

            if(blockedSender.Result.GetArrayLength()>0)
            {
                blockedForSpam = true;
            }

            Task<List<JsonElement>> statsRequest = RequestHelper.NewGraphGetRequest(string.Format("https://outlook.office365.com/adminapi/beta/{0}/Mailbox('{1}')/Exchange.GetMailboxStatistics()", tenantFilter, email), tenantFilter, "exchangeonline", noPagination: true);
            List<JsonElement> permsRequest = await RequestHelper.NewGraphGetRequest(string.Format("https://outlook.office365.com/adminapi/beta/{0}/Mailbox('{1}')/MailboxPermission", tenantFilter, email), tenantFilter, "exchangeonline", noPagination: true);

            permsRequest = permsRequest.FindAll(x => !x.GetProperty("User").GetString().ToLower().Equals("nt authority\\self"));

            foreach (JsonElement j in permsRequest)
            {
                string perms = string.Empty;

                JsonElement[]? raw = j.GetProperty("PermissionList").EnumerateArray().ToArray();
                foreach(JsonElement j2 in raw)
                {
                    JsonElement[] j3 =  j2.GetProperty("AccessRights").EnumerateArray().ToArray();
                    foreach(JsonElement j4 in j3)
                    {
                        perms+=j4.GetString()+", ";
                    }
                }
                MailboxUserPermissions permission = new MailboxUserPermissions() { User = j.GetProperty("User").GetString() ?? string.Empty, AccessRights = perms.TrimEnd(',',' ') };
                userPerms.Add(permission);
            }

            JsonElement mDr = mailboxDetailedRequest.Result[0];

            string? forwardingAddress = string.Empty;
             
            if (!string.IsNullOrEmpty(mDr.GetProperty("ForwardingSmtpAddress").GetString()) &&
                !string.IsNullOrEmpty(mDr.GetProperty("ForwardingAddress").GetString())) 
            {
                forwardingAddress = mDr.GetProperty("ForwardingAddress").GetString() + ' ' +
                    mDr.GetProperty("ForwardingSmtpAddress").GetString();
            }
            else if (!string.IsNullOrEmpty(mDr.GetProperty("ForwardingAddress").GetString()))
            {
                forwardingAddress = mDr.GetProperty("ForwardingAddress").GetString();
            }
            else
            {
                forwardingAddress = mDr.GetProperty("ForwardingSmtpAddress").GetString();
            }

            JsonElement cas = casResponseTask.Result[0];
            JsonElement stats = statsRequest.Result[0];

            return new()
            {
                new()
                {
                    ForwardAndDeliver = mDr.GetProperty("DeliverToMailboxAndForward").GetBoolean(),
                    ForwardingAddress = forwardingAddress,
                    LitiationHold = mDr.GetProperty("LitigationHoldEnabled").GetBoolean(),
                    HiddenFromAddressLists = mDr.GetProperty("HiddenFromAddressListsEnabled").GetBoolean(),
                    EWSEnabled = cas.GetProperty("EwsEnabled").GetBoolean(),
                    MailboxMAPIEnabled = cas.GetProperty("MAPIEnabled").GetBoolean(),
                    MailboxOWAEnabled = cas.GetProperty("OWAEnabled").GetBoolean(),
                    MailboxImapEnabled = cas.GetProperty("ImapEnabled").GetBoolean(),
                    MailboxPopEnabled = cas.GetProperty("PopEnabled").GetBoolean(),
                    MailboxActiveSyncEnabled = cas.GetProperty("ActiveSyncEnabled").GetBoolean(),
                    Permissions = userPerms,
                    ProhibitSendQuota = Math.Round(float.Parse(mDr.GetProperty("ProhibitSendQuota").GetString().Split(" GB")[0]), 2),
                    ProhibitSendReceiveQuota = Math.Round(float.Parse(mDr.GetProperty("ProhibitSendReceiveQuota").GetString().Split(" GB")[0]), 2),
                    ItemCount = stats.GetProperty("ItemCount").GetInt64(),
                    TotalItemSize = Math.Round((decimal)(stats.GetProperty("TotalItemSize").GetInt64()) / 1073741824, 2),
                    BlockedForSpam = blockedForSpam
                }
            };
        }

        public struct UserMailboxDetails
        {
            public bool? ForwardAndDeliver { get; set; }
            public string? ForwardingAddress { get; set; }
            public bool? LitiationHold { get; set; }
            public bool? HiddenFromAddressLists { get; set; }
            public bool? EWSEnabled { get; set; }
            public bool? MailboxMAPIEnabled { get; set; }
            public bool? MailboxOWAEnabled { get; set; }
            public bool? MailboxImapEnabled { get; set; }
            public bool? MailboxPopEnabled { get; set; }
            public bool? MailboxActiveSyncEnabled { get; set; }
            public List<MailboxUserPermissions>? Permissions { get; set; }
            public double? ProhibitSendQuota { get; set; }
            public double? ProhibitSendReceiveQuota { get; set; }
            public long? ItemCount { get; set; }
            public decimal? TotalItemSize { get; set; }
            public bool? BlockedForSpam { get; set; }
        }

        public struct MailboxUserPermissions
        {
            public string User { get; set; }
            public string AccessRights { get; set; }
        }

        public struct OneDriveSiteReport
        {
            public OneDriveSiteReport()
            {
                ReportRefreshDate = string.Empty;
                URL = string.Empty;
                displayName = string.Empty;
                IsDeleted = string.Empty;
                LastActive = string.Empty;
                FileCount = string.Empty;
                ActiveFileCount = string.Empty;
                UsedGB = string.Empty;
                Allocated = string.Empty;
                UPN = string.Empty;
                ReportPeriod = string.Empty;
            }
            public OneDriveSiteReport(string reportRefreshDate, string siteUrl, string ownerDisplayName, string isDeleted, string lastActivityDate, string fileCount, string activeFileCount, string storageUsedBytes, string storageAllocatedBytes, string ownerPrincipalName, string reportPeriod)
            {
                ReportRefreshDate = reportRefreshDate;
                URL = siteUrl;
                displayName = ownerDisplayName;
                IsDeleted = isDeleted;
                LastActive = lastActivityDate;
                FileCount = fileCount;
                ActiveFileCount = activeFileCount;
                UsedGB = (decimal.Parse(storageUsedBytes) / 1073741824).ToString("#.##"); // gives GB
                Allocated = (decimal.Parse(storageAllocatedBytes) / 1073741824).ToString("#.##"); // gives GB
                UPN = ownerPrincipalName;
                ReportPeriod = reportPeriod;
            }
            public string? ReportRefreshDate { get; set; }
            public string? URL { get; set; }
            public string? displayName { get; set; }
            public string? IsDeleted { get; set; }
            public string? LastActive { get; set; }
            public string? FileCount { get; set; }
            public string? ActiveFileCount { get; set; }
            public string? UsedGB { get; set; }
            public string? Allocated { get; set; }
            public string? UPN { get; set; }
            public string? ReportPeriod { get; set; }
            //public string? Template { get; set; }
        }

        public struct ConditionalAccessPolicy
        {
            public string id { get; set; }
            public string displayName { get; set; }
        }
    }

}
