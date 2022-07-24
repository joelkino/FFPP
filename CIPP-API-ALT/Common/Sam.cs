using System.Dynamic;
using System.Text.Json;

namespace CIPP_API_ALT.Common
{
    public class Sam
    {
        public enum SamAppType { Api, Spa };

        public static async void CreateSAMAuthApp(string appName, SamAppType appType, PreAuthorizedApplication spaToAuth = new(), string spaRedirectUri="")
        {
            dynamic samApp;

            switch (appType)
            {
                case SamAppType.Api:
                    samApp = new ExpandoObject();
                    samApp.displayName = appName;
                    samApp.identifierUris = new List<string>() { string.Format("https://{0}/{1}", ApiEnvironment.CippDomain, Guid.NewGuid().ToString()) };

                    if (!spaToAuth.Equals(new PreAuthorizedApplication()))
                    {
                        samApp.api = new ApiApplication()
                        {
                            acceptMappedClaims = null,
                            knownClientApplications = new List<string>(){},
                            requestedAccessTokenVersion = 2,
                            oauth2PermissionScopes = new List<PermissionScope>()
                        {

                            new PermissionScope
                            {
                                id = Guid.NewGuid().ToString(),
                                adminConsentDescription = "access the api",
                                adminConsentDisplayName = "access the api",
                                isEnabled = true,
                                type = "Admin",
                                userConsentDescription = "access the api",
                                userConsentDisplayName = "access the api",
                                value = "cipp-api-alt.access"
                            }
                        },
                            preAuthorizedApplications = new() { spaToAuth }
                        };
                    }
                    else
                    {
                        samApp.api = new ApiApplication()
                        {
                            acceptMappedClaims = null,
                            knownClientApplications = new List<string>()
                            {
                            },
                            requestedAccessTokenVersion = 2,
                            oauth2PermissionScopes = new List<PermissionScope>()
                        {

                            new PermissionScope
                            {
                                id = Guid.NewGuid().ToString(),
                                adminConsentDescription = "access the api",
                                adminConsentDisplayName = "access the api",
                                isEnabled = true,
                                type = "Admin",
                                userConsentDescription = "access the api",
                                userConsentDisplayName = "access the api",
                                value = "cipp-api-alt.access"
                            }
                        },
                            preAuthorizedApplications = new() { }
                        };
                    }
                    samApp.appRoles = new List<AppRole>()
                    {
                        new()
                        {
                            allowedMemberTypes = new() { "User" },
                            description = "reader",
                            displayName = "reader",
                            id = Guid.NewGuid().ToString(),
                            isEnabled = true,
                            origin = "application",
                            value = "reader"
                        },
                        new()
                        {
                            allowedMemberTypes = new() { "User" },
                            description = "editor",
                            displayName = "editor",
                            id = Guid.NewGuid().ToString(),
                            isEnabled = true,
                            origin = "application",
                            value = "editor"
                        },
                        new()
                        {
                            allowedMemberTypes = new() { "User" },
                            description = "admin",
                            displayName = "admin",
                            id = Guid.NewGuid().ToString(),
                            isEnabled = true,
                            origin = "application",
                            value = "admin"
                        },
                        new()
                        {
                            allowedMemberTypes = new() { "User" },
                            description = "owner",
                            displayName = "owner",
                            id = Guid.NewGuid().ToString(),
                            isEnabled = true,
                            origin = "application",
                            value = "owner"
                        }
                    };
                    samApp.signInAudience = "AzureADMyOrg";

                    var json = await RequestHelper.NewGraphPostRequest("https://graph.microsoft.com/v1.0/applications", ApiEnvironment.Secrets.TenantId, samApp, HttpMethod.Post, "https://graph.microsoft.com/Application.ReadWrite.All", false);
                    break;

                case SamAppType.Spa:
                    samApp = new ExpandoObject();
                    samApp.displayName = appName;
                    samApp.signInAudience = "AzureADMyOrg";
                    samApp.requiredResourceAccess = new List<RequiredResourceAccess>() { new RequiredResourceAccess(){ resourceAccess = new List<ResourceAccess>() { new() { id = new Guid("e1fe6dd8ba314d6189e788639da4683d"), type = "Scope" } }, resourceAppId = new Guid("0000000300000000c000000000000000") } };

                    if (!string.IsNullOrEmpty(spaRedirectUri))
                    {
                        samApp.spa = new Spa() { redirectUris = new() { spaRedirectUri } };
                    }

                    var json2 = await RequestHelper.NewGraphPostRequest("https://graph.microsoft.com/v1.0/applications", ApiEnvironment.Secrets.TenantId, samApp, HttpMethod.Post, "https://graph.microsoft.com/Application.ReadWrite.All", false);
                    break;
            }
        }

        public struct ResourceAccess
        {
            public Guid id { get; set; }
            public string type { get; set; }
        }

        public struct RequiredResourceAccess
        {
            public List<ResourceAccess> resourceAccess { get; set; }
            public Guid resourceAppId { get; set; }
        }

        public struct Spa
        {
            public List<string>? redirectUris { get; set; }
        }

        public struct ApiApplication
        {
            public bool? acceptMappedClaims { get; set; }
            public List<string>? knownClientApplications { get; set; }
            public List<PermissionScope>? oauth2PermissionScopes { get; set; }
            public List<PreAuthorizedApplication>? preAuthorizedApplications { get; set; }
            public int? requestedAccessTokenVersion { get; set; }
        }

        public struct PermissionScope
        {
            public string? id { get; set; }
            public string? adminConsentDisplayName { get; set; }
            public string? adminConsentDescription { get; set; }
            public string? userConsentDisplayName { get; set; }
            public string? userConsentDescription { get; set; }
            public string? value { get; set; }
            public string? type { get; set; }
            public bool? isEnabled { get; set; }
        }

        public struct PreAuthorizedApplication
        {
            public string? appId { get; set; }
            public List<string>? delegatedPermissionIds { get; set; }
        }


        public struct AppRole
        {
            public List<string>? allowedMemberTypes { get; set; }
            public string? description { get; set; }
            public string? displayName { get; set; }
            public string? id { get; set; }
            public bool? isEnabled { get; set; }
            public string? origin { get; set; }
            public string? value { get; set; }
        }
    }
}

