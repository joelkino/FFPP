using Microsoft.Identity.Web.Resource;
using CIPP_API_ALT.Common;
using CIPP_API_ALT.Data.Logging;
using CIPP_API_ALT.Api.v10.Dashboards;
using CIPP_API_ALT.Api.v10.Users;
using CIPP_API_ALT.Api.v10.Tenants;

namespace CIPP_API_ALT.Api.v10
{
    /// <summary>
    /// /v1.0 ### THIS IS THE V1.0 ENDPOINTS ###
    /// </summary>
    public static class Routes
    {
        private static readonly string _versionHeader = "v1.0";

        public static void InitRoutes(ref WebApplication app)
        {
            #region API Routes
            /// <summary>
            /// /v1.0/CurrentRouteVersion
            /// </summary>
            app.MapGet(string.Format("/{0}/CurrentRouteVersion",_versionHeader), () =>
            {
                return CurrentRouteVersion();

            }).WithName(string.Format("/{0}/CurrentRouteVersion", _versionHeader)).WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);

            /// <summary>
            /// /v1.0/GetDashboard
            /// </summary>
            app.MapGet("/v{version:apiVersion}/GetDashboard", async (HttpContext context, HttpRequest request) =>
            {
                string accessingUser = string.Empty;

                if (ApiEnvironment.CippCompatibilityMode)
                {
                    accessingUser = await CippLogs.ReadSwaUser(request.Headers["x-ms-client-principal"]);
                }
                else
                {
                    //todo code to get user from JWT token provided in auth bearer
                }

                try
                {
                    return await GetDashboard(context, accessingUser);
                }
                catch (UnauthorizedAccessException)
                {
                    context.Response.StatusCode = 401;
                    return Results.Unauthorized();
                }
            })
            .WithName(string.Format("/{0}/GetDashboard", _versionHeader)).WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);

            /// <summary>
            /// /v1.0/GetVersion
            /// </summary>
            app.MapGet("/v{version:apiVersion}/GetVersion", async (HttpContext context, HttpRequest request, string LocalVersion) =>
            {
                string accessingUser = string.Empty;

                if(ApiEnvironment.CippCompatibilityMode)
                {
                    accessingUser = await CippLogs.ReadSwaUser(request.Headers["x-ms-client-principal"]);
                }
                else
                {
                    //todo code to get user from JWT token provided in auth bearer
                }

                try
                {
                    return await GetVersion(context, LocalVersion, accessingUser);
                }
                catch(UnauthorizedAccessException)
                {
                    context.Response.StatusCode = 401;
                    return Results.Unauthorized();
                }
            })
            .WithName(string.Format("/{0}/GetVersion", _versionHeader)).WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);

            /// <summary>
            /// /v1.0/Heartbeat
            /// </summary>
            app.MapGet("/v{version:apiVersion}/Heartbeat", () =>
            {
                return new Heartbeat();
            })
            .WithName(string.Format("/{0}/Heartbeat", _versionHeader)).WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);

            /// <summary>
            /// /v1.0/ListDomains
            /// </summary>
            app.MapGet("/v{version:apiVersion}/ListDomains", async (HttpContext context, HttpRequest request, string TenantFilter) =>
            {
                string accessingUser = string.Empty;

                if (ApiEnvironment.CippCompatibilityMode)
                {
                    accessingUser = await CippLogs.ReadSwaUser(request.Headers["x-ms-client-principal"]);
                }
                else
                {
                    //todo code to get user from JWT token provided in auth bearer
                }

                try
                {
                    return await ListDomains(context, TenantFilter, accessingUser);
                }              
                catch(UnauthorizedAccessException)
                {
                    context.Response.StatusCode = 401;
                    return Results.Unauthorized();
                }
            })
            .WithName(string.Format("/{0}/ListDomains", _versionHeader)).WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);

            /// <summary>
            /// /v1.0/ListSites
            /// </summary>
            app.MapGet("/v{version:apiVersion}/ListSites", async (HttpContext context, HttpRequest request, string TenantFilter, string Type, string? UserUPN) =>
            {
                string accessingUser = string.Empty;

                if (ApiEnvironment.CippCompatibilityMode)
                {
                    accessingUser = await CippLogs.ReadSwaUser(request.Headers["x-ms-client-principal"]);
                }
                else
                {
                    //todo code to get user from JWT token provided in auth bearer
                }

                try
                {
                    return await ListSites(context, TenantFilter, Type, UserUPN ?? string.Empty, accessingUser);
                }
                catch (UnauthorizedAccessException)
                {
                    context.Response.StatusCode = 401;
                    return Results.Unauthorized();
                }

            })
            .WithName(string.Format("/{0}/ListSites", _versionHeader)).WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);

            /// <summary>
            /// /v1.0/ListTenants
            /// </summary>
            app.MapGet("/v{version:apiVersion}/ListTenants", async (HttpContext context, HttpRequest request, bool ? AllTenantSelector) =>
            {
                string accessingUser = string.Empty;

                if (ApiEnvironment.CippCompatibilityMode)
                {
                    accessingUser = await CippLogs.ReadSwaUser(request.Headers["x-ms-client-principal"]);
                }
                else
                {
                    //todo code to get user from JWT token provided in auth bearer
                }

                try
                {
                    return await ListTenants(context, AllTenantSelector ?? false, accessingUser);
                }
                catch (UnauthorizedAccessException)
                {
                    context.Response.StatusCode = 401;
                    return Results.Unauthorized();
                }

            })
            .WithName(string.Format("/{0}/ListTenants", _versionHeader)).WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);

            /// <summary>
            /// /v1.0/ListUsers
            /// </summary>
            app.MapGet("/v{version:apiVersion}/ListUsers", async (HttpContext context, HttpRequest request, string TenantFilter, string? UserId) =>
            {
                string accessingUser = string.Empty;

                if (ApiEnvironment.CippCompatibilityMode)
                {
                    accessingUser = await CippLogs.ReadSwaUser(request.Headers["x-ms-client-principal"]);
                }
                else
                {
                    //todo code to get user from JWT token provided in auth bearer
                }

                try
                {
                    return await ListUsers(context, TenantFilter, UserId ?? string.Empty, accessingUser);
                }
                catch (UnauthorizedAccessException)
                {
                    context.Response.StatusCode = 401;
                    return Results.Unauthorized();
                }
            })
            .WithName(string.Format("/{0}/ListUsers", _versionHeader)).WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);

            /// <summary>
            /// /v1.0/ListUserConditionalAccessPolicies
            /// </summary>
            app.MapGet("/v{version:apiVersion}/ListUserConditionalAccessPolicies", async (HttpContext context, HttpRequest request, string TenantFilter, string UserId) =>
            {
                string accessingUser = string.Empty;

                if (ApiEnvironment.CippCompatibilityMode)
                {
                    accessingUser = await CippLogs.ReadSwaUser(request.Headers["x-ms-client-principal"]);
                }
                else
                {
                    //todo code to get user from JWT token provided in auth bearer
                }

                try { 
                    return await ListUserConditionalAccessPolicies(context, TenantFilter, UserId ?? string.Empty, accessingUser);
                }
                catch (UnauthorizedAccessException)
                {
                    context.Response.StatusCode = 401;
                    return Results.Unauthorized();
                }
            })
            .WithName(string.Format("/{0}/ListUserConditionalAccessPolicies", _versionHeader)).WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);

            /// <summary>
            /// /v1.0/ListUserDevices
            /// </summary>
            app.MapGet("/v{version:apiVersion}/ListUserDevices", async (HttpContext context, HttpRequest request, string TenantFilter, string UserId) =>
            {
                string accessingUser = string.Empty;

                if (ApiEnvironment.CippCompatibilityMode)
                {
                    accessingUser = await CippLogs.ReadSwaUser(request.Headers["x-ms-client-principal"]);
                }
                else
                {
                    //todo code to get user from JWT token provided in auth bearer
                }

                try
                {
                    return await ListUserDevices(context, TenantFilter, UserId ?? string.Empty, accessingUser);
                }
                catch (UnauthorizedAccessException)
                {
                    context.Response.StatusCode = 401;
                    return Results.Unauthorized();
                }
            })
            .WithName(string.Format("/{0}/ListUserDevices", _versionHeader)).WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);

            /// <summary>
            /// /v1.0/ListUserGroups
            /// </summary>
            app.MapGet("/v{version:apiVersion}/ListUserGroups", async (HttpContext context, HttpRequest request, string TenantFilter, string UserId) =>
            {
                string accessingUser = string.Empty;

                if (ApiEnvironment.CippCompatibilityMode)
                {
                    accessingUser = await CippLogs.ReadSwaUser(request.Headers["x-ms-client-principal"]);
                }
                else
                {
                    //todo code to get user from JWT token provided in auth bearer
                }

                try
                {
                    return await ListUserGroups(context, TenantFilter, UserId ?? string.Empty, accessingUser);
                }
                catch (UnauthorizedAccessException)
                {
                    context.Response.StatusCode = 401;
                    return Results.Unauthorized();
                }
            }).WithName(string.Format("/{0}/ListUserGroups", _versionHeader)).WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);

            /// <summary>
            /// /v1.0/ListUserMailboxDetails
            /// </summary>
            app.MapGet("/v{version:apiVersion}/ListUserMailboxDetails", async (HttpContext context, HttpRequest request, string TenantFilter, string UserId) =>
            {
                string accessingUser = string.Empty;

                if (ApiEnvironment.CippCompatibilityMode)
                {
                    accessingUser = await CippLogs.ReadSwaUser(request.Headers["x-ms-client-principal"]);
                }
                else
                {
                    //todo code to get user from JWT token provided in auth bearer
                }

                try
                {
                    return await ListUserMailboxDetails(context, TenantFilter, UserId ?? string.Empty, accessingUser);
                }
                catch (UnauthorizedAccessException)
                {
                    context.Response.StatusCode = 401;
                    return Results.Unauthorized();
                }
            })
            .WithName(string.Format("/{0}/ListUserMailboxDetails", _versionHeader)).WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);

            /// <summary>
            /// /v1.0/ListUserSigninLogs
            /// </summary>
            app.MapGet("/v{version:apiVersion}/ListUserSigninLogs", async (HttpContext context, HttpRequest request, string TenantFilter, string UserId) =>
            {
                string accessingUser = string.Empty;

                if (ApiEnvironment.CippCompatibilityMode)
                {
                    accessingUser = await CippLogs.ReadSwaUser(request.Headers["x-ms-client-principal"]);
                }
                else
                {
                    //todo code to get user from JWT token provided in auth bearer
                }

                try
                {
                    return await ListUserSigninLogs(context, TenantFilter, UserId ?? string.Empty, accessingUser);
                }
                catch (UnauthorizedAccessException)
                {
                    context.Response.StatusCode = 401;
                    return Results.Unauthorized();
                }
            }).WithName(string.Format("/{0}/ListUserSigninLogs", _versionHeader)).WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);
            #endregion
        }

        public static CurrentApiRoute CurrentRouteVersion()
        {
            return new CurrentApiRoute();
        }

        public static async Task<object> GetDashboard(HttpContext context, string accessingUser)
        {
            if (!ApiEnvironment.CippCompatibilityMode)
            {
                CheckUserIsReader(context);
            }

            return await CippDashboard.GetHomeData(accessingUser);
        }

        public static async Task<object>GetVersion(HttpContext context, string LocalVersion, string accessingUser)
        {
            if (!ApiEnvironment.CippCompatibilityMode)
            {
                CheckUserIsReader(context);
            }

            return await CippDashboard.CheckVersions(accessingUser, LocalVersion);
        }

        public static async Task<object> ListSites(HttpContext context, string tenantFilter, string type, string userUpn, string accessingUser)
        {
            if (!ApiEnvironment.CippCompatibilityMode)
            {
                CheckUserIsReader(context);
            }

            return await User.GetSites(tenantFilter, type, userUpn, accessingUser);
        }

        public static async Task<object> ListTenants(HttpContext context, bool? AllTenantSelector, string accessingUser)
        {
            if (!ApiEnvironment.CippCompatibilityMode)
            {
                CheckUserIsReader(context);
            }

            return await Tenant.GetTenants(accessingUser, allTenantSelector: AllTenantSelector ?? false);
        }

        public static async Task<object> ListUserConditionalAccessPolicies(HttpContext context, string TenantFilter, string UserId, string accessingUser)
        {
            if (!ApiEnvironment.CippCompatibilityMode)
            {
                CheckUserIsReader(context);
            }

            return await User.GetUserConditionalAccessPolicies(TenantFilter, UserId ?? string.Empty, accessingUser);
        }

        public static async Task<object> ListUserDevices(HttpContext context, string TenantFilter, string UserId, string accessingUser)
        {
            if (!ApiEnvironment.CippCompatibilityMode)
            {
                CheckUserIsReader(context);
            }

            return await User.GetUserDevices(TenantFilter, UserId ?? string.Empty, accessingUser);
        }

        public static async Task<object> ListUserGroups(HttpContext context, string TenantFilter, string UserId, string accessingUser)
        {
            if (!ApiEnvironment.CippCompatibilityMode)
            {
                CheckUserIsReader(context);
            }

            return await User.GetUserGroups(TenantFilter, UserId ?? string.Empty, accessingUser);
        }

        public static async Task<object> ListUserMailboxDetails(HttpContext context, string TenantFilter, string UserId, string accessingUser)
        {
            if (!ApiEnvironment.CippCompatibilityMode)
            {
                CheckUserIsReader(context);
            }

            return await User.GetUserMailboxDetails(TenantFilter, UserId ?? string.Empty, accessingUser);
        }

        public static async Task<object> ListUserSigninLogs(HttpContext context, string TenantFilter, string UserId, string accessingUser)
        {
            if (!ApiEnvironment.CippCompatibilityMode)
            {
                CheckUserIsReader(context);
            }

            return await User.GetUserSigninLogs(TenantFilter, UserId ?? string.Empty, accessingUser);
        }

        public static async Task<object> ListUsers(HttpContext context, string TenantFilter, string UserId, string accessingUser)
        {
            if (!ApiEnvironment.CippCompatibilityMode)
            {
                CheckUserIsReader(context);
            }

            return await User.GetUsers(accessingUser, TenantFilter, UserId ?? string.Empty);
        }

        public static async Task<object> ListDomains(HttpContext context, string TenantFilter, string accessingUser)
        {
            if (!ApiEnvironment.CippCompatibilityMode)
            {
                CheckUserIsReader(context);
            }

            return await Domain.GetDomains(accessingUser, TenantFilter);
        }

        private static void CheckUserIsReader(HttpContext context)
        {
            string[] scopes = { "cipp-api-alt.access" };
            string[] roles = { "owner", "admin", "editor", "reader" };
            context.ValidateAppRole(roles);
            context.VerifyUserHasAnyAcceptedScope(scopes);
        }

        private static void CheckUserIsEditor(HttpContext context)
        {
            string[] scopes = { "cipp-api-alt.access" };
            string[] roles = { "owner", "admin", "editor" };
            context.ValidateAppRole(roles);
            context.VerifyUserHasAnyAcceptedScope(scopes);
        }

        private static void CheckUserIsAdmin(HttpContext context)
        {
            string[] scopes = { "cipp-api-alt.access" };
            string[] roles = { "owner", "admin" };
            context.ValidateAppRole(roles);
            context.VerifyUserHasAnyAcceptedScope(scopes);
        }

        private static void CheckUserIsOwner(HttpContext context)
        {
            string[] scopes = { "cipp-api-alt.access" };
            string[] roles = { "owner" };
            context.ValidateAppRole(roles);
            context.VerifyUserHasAnyAcceptedScope(scopes);
        }

        /// <summary>
        /// Defines a Heartbeat object we return when the /api/Heartbeat API is polled
        /// </summary>
        public struct Heartbeat
        {
            public DateTime started { get => ApiEnvironment.Started; }
        }

        /// <summary>
        /// Defines the latest version API scheme when /api/CurrentApiRoute queried (returns dev when dev endpoints enabled)
        /// </summary>
        public struct CurrentApiRoute
        {
            public string api { get => "v" + ApiEnvironment.ApiRouteVersions[^1].ToString("f1"); }
        }

        /// <summary>
        /// Defines the error we send back in JSON payload
        /// </summary>
        public struct ErrorResponseBody
        {
            public int errorCode { get; set; }
            public string message { get; set; }
        }
    }
}

