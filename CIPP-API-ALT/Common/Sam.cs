using System.Text.Json;

namespace CIPP_API_ALT.Common
{
    public class Sam
    {
        public enum SamAppType { Api, Spa };

        public Sam()
        {
        }

        public static async void CreateSAMAuthApp(string appName, SamAppType appType)
        {
            switch (appType)
            {
                case SamAppType.Api:
                    break;

                case SamAppType.Spa:
                    break;
            }

            SamApplication samApp = new()
            {
                displayName = appName,
                identifierUris = new() {"api://"+Guid.NewGuid().ToString()},
                api = new()
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
                    preAuthorizedApplications = new List<PreAuthorizedApplication>()
                    {
                    }
                },
                appRoles = new()
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
                    },
                }
            };
            var json = await RequestHelper.NewGraphPostRequest("https://graph.microsoft.com/v1.0/applications", ApiEnvironment.Secrets.TenantId, samApp,HttpMethod.Post, "https://graph.microsoft.com/Application.ReadWrite.All", false);

            var i = 1;
        }

        public struct SamApplication
        {
            public string? displayName { get; set; }
            public List<string>? identifierUris { get; set; }
            public ApiApplication? api { get; set; }
            public List<AppRole>? appRoles { get; set; }
            //public bool? oauth2RequiredPostResponse { get; set; }
            //public string? serviceManagementReference { get; set; }
            //public string? signInAudience { get; set; }
            //public Spa? spa { get; set; }
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

