using Microsoft.Identity.Web.Resource;
using System.Security.Claims;
using FFPP.Common;
using FFPP.Api.v10.Dashboards;
using FFPP.Api.v10.Users;
using FFPP.Api.v10.Tenants;

namespace FFPP.Api.v10
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
            /// /v1.0/.auth/me
            /// </summary>
            app.MapGet("/v{version:apiVersion}/.auth/me", async (HttpContext context, HttpRequest request) =>
            {
                try
                {
                    return await AuthMe(context);
                }
                catch (UnauthorizedAccessException)
                {
                    context.Response.StatusCode = 401;
                    return Results.Unauthorized();
                }

            }).WithName(string.Format("/{0}/.auth/me", _versionHeader)).WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);
            /// <summary>
            /// /v1.0/CurrentRouteVersion
            /// </summary>
            app.MapGet("/v{version:apiVersion}/CurrentRouteVersion", async () =>
            {
                return await CurrentRouteVersion();

            }).WithName(string.Format("/{0}/CurrentRouteVersion", _versionHeader)).WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);

            /// <summary>
            /// /v1.0/GetDashboard
            /// </summary>
            app.MapGet("/v{version:apiVersion}/GetDashboard", async (HttpContext context, HttpRequest request) =>
            {
                try
                {
                    return await GetDashboard(context);
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
                try
                {
                    return await GetVersion(context, LocalVersion);
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
            app.MapGet("/v{version:apiVersion}/Heartbeat", async () =>
            {
                Task<Heartbeat> task = new(() =>
                {
                    return new Heartbeat();
                });

                task.Start();

                return await task;
            })
            .WithName(string.Format("/{0}/Heartbeat", _versionHeader)).WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);

            /// <summary>
            /// /v1.0/ListDomains
            /// </summary>
            app.MapGet("/v{version:apiVersion}/ListDomains", async (HttpContext context, HttpRequest request, string TenantFilter) =>
            {
                try
                {
                    return await ListDomains(context, TenantFilter);
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
                try
                {
                    return await ListSites(context, TenantFilter, Type, UserUPN ?? string.Empty);
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
                try
                {
                    return await ListTenants(context, AllTenantSelector ?? false);
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
                try
                {
                    return await ListUsers(context, TenantFilter, UserId ?? string.Empty);
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
                try
                { 
                    return await ListUserConditionalAccessPolicies(context, TenantFilter, UserId ?? string.Empty);
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
                try
                {
                    return await ListUserDevices(context, TenantFilter, UserId ?? string.Empty);
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
                try
                {
                    return await ListUserGroups(context, TenantFilter, UserId ?? string.Empty);
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
                try
                {
                    return await ListUserMailboxDetails(context, TenantFilter, UserId ?? string.Empty);
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
                try
                {
                    return await ListUserSigninLogs(context, TenantFilter, UserId ?? string.Empty);
                }
                catch (UnauthorizedAccessException)
                {
                    context.Response.StatusCode = 401;
                    return Results.Unauthorized();
                }
            }).WithName(string.Format("/{0}/ListUserSigninLogs", _versionHeader)).WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);
            #endregion
        }

        public static async Task<CurrentApiRoute> CurrentRouteVersion()
        {
            Task<CurrentApiRoute> task = new(() =>
            {
                return new CurrentApiRoute();
            });

            task.Start();

            return await task;
        
        }

        public static async Task<object> AuthMe(HttpContext context)
        {
            if (!ApiEnvironment.SimulateAuthenticated)
            {
                CheckUserIsReader(context);
            }

            Task<Auth> task = new(() =>
            {

               List<string> roles = new();


               // I think we can only have one role but I'll iterate just in case it happens
               foreach (Claim c in context.User.Claims.Where(x => x.Type.ToLower().Equals("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")).ToList())
               {
                   roles.Add(c.Value);
               }
                try
                {
                    return new Auth()
                    {
                        clientPrincipal = new()
                        {
                            identityProvider = "aad",
                            userId = context.User.Claims.First(x => x.Type.ToLower().Equals("http://schemas.microsoft.com/identity/claims/objectidentifier")).Value,
                            name = context.User.Claims.First(x => x.Type.ToLower().Equals("name")).Value,
                            userDetails = context.User.Claims.First(x => x.Type.ToLower().Equals("preferred_username")).Value,
                            userRoles = roles
                        }
                    };
                }
                catch
                {
                    context.Response.StatusCode = 400;
                    return new Auth();
                }
            });

            task.Start();

            return await task;
        }

        public static async Task<object> GetDashboard(HttpContext context)
        {
            string accessingUser = ApiEnvironment.FfppSimulatedAuthUsername;

            if (!ApiEnvironment.SimulateAuthenticated)
            {
                CheckUserIsReader(context);
                accessingUser = await Utilities.UsernameParse(context);
            }

            return await FfppDashboard.GetHomeData(accessingUser);
        }

        public static async Task<object>GetVersion(HttpContext context, string LocalVersion)
        {
            string accessingUser = ApiEnvironment.FfppSimulatedAuthUsername;

            if (!ApiEnvironment.SimulateAuthenticated)
            {
                CheckUserIsReader(context);
                accessingUser = await Utilities.UsernameParse(context);
            }

            return await FfppDashboard.CheckVersions(accessingUser, LocalVersion);
        }

        public static async Task<object> ListSites(HttpContext context, string tenantFilter, string type, string userUpn)
        {
            string accessingUser = ApiEnvironment.FfppSimulatedAuthUsername;

            if (!ApiEnvironment.SimulateAuthenticated)
            {
                CheckUserIsReader(context);
                accessingUser = await Utilities.UsernameParse(context);
            }

            return await User.GetSites(tenantFilter, type, userUpn, accessingUser);
        }

        public static async Task<object> ListTenants(HttpContext context, bool? AllTenantSelector)
        {
            string accessingUser = ApiEnvironment.FfppSimulatedAuthUsername;

            if (!ApiEnvironment.SimulateAuthenticated)
            {
                CheckUserIsReader(context);
                accessingUser = await Utilities.UsernameParse(context);
            }

            return await Tenant.GetTenants(accessingUser, allTenantSelector: AllTenantSelector ?? false);
        }

        public static async Task<object> ListUserConditionalAccessPolicies(HttpContext context, string TenantFilter, string UserId)
        {
            string accessingUser = ApiEnvironment.FfppSimulatedAuthUsername;

            if (!ApiEnvironment.SimulateAuthenticated)
            {
                CheckUserIsReader(context);
                accessingUser = await Utilities.UsernameParse(context);
            }

            return await User.GetUserConditionalAccessPolicies(TenantFilter, UserId ?? string.Empty, accessingUser);
        }

        public static async Task<object> ListUserDevices(HttpContext context, string TenantFilter, string UserId)
        {
            string accessingUser = ApiEnvironment.FfppSimulatedAuthUsername;

            if (!ApiEnvironment.SimulateAuthenticated)
            {
                CheckUserIsReader(context);
                accessingUser = await Utilities.UsernameParse(context);
            }

            return await User.GetUserDevices(TenantFilter, UserId ?? string.Empty, accessingUser);
        }

        public static async Task<object> ListUserGroups(HttpContext context, string TenantFilter, string UserId)
        {
            string accessingUser = ApiEnvironment.FfppSimulatedAuthUsername;

            if (!ApiEnvironment.SimulateAuthenticated)
            {
                CheckUserIsReader(context);
                accessingUser = await Utilities.UsernameParse(context);
            }

            return await User.GetUserGroups(TenantFilter, UserId ?? string.Empty, accessingUser);
        }

        public static async Task<object> ListUserMailboxDetails(HttpContext context, string TenantFilter, string UserId)
        {
            string accessingUser = ApiEnvironment.FfppSimulatedAuthUsername;

            if (!ApiEnvironment.SimulateAuthenticated)
            {
                CheckUserIsReader(context);
                accessingUser = await Utilities.UsernameParse(context);
            }

            return await User.GetUserMailboxDetails(TenantFilter, UserId ?? string.Empty, accessingUser);
        }

        public static async Task<object> ListUserSigninLogs(HttpContext context, string TenantFilter, string UserId)
        {
            string accessingUser = ApiEnvironment.FfppSimulatedAuthUsername;

            if (!ApiEnvironment.SimulateAuthenticated)
            {
                CheckUserIsReader(context);
                accessingUser = await Utilities.UsernameParse(context);
            }

            return await User.GetUserSigninLogs(TenantFilter, UserId ?? string.Empty, accessingUser);
        }

        public static async Task<object> ListUsers(HttpContext context, string TenantFilter, string UserId)
        {
            string accessingUser = ApiEnvironment.FfppSimulatedAuthUsername;

            if (!ApiEnvironment.SimulateAuthenticated)
            {
                CheckUserIsReader(context);
                accessingUser = await Utilities.UsernameParse(context);
            }

            return await User.GetUsers(accessingUser, TenantFilter, UserId ?? string.Empty);
        }

        public static async Task<object> ListDomains(HttpContext context, string TenantFilter)
        {
            string accessingUser = ApiEnvironment.FfppSimulatedAuthUsername;

            if (!ApiEnvironment.SimulateAuthenticated)
            {
                CheckUserIsReader(context);
                accessingUser = await Utilities.UsernameParse(context);
            }

            return await Domain.GetDomains(accessingUser, TenantFilter);
        }

        private static void CheckUserIsReader(HttpContext context)
        {
            string[] scopes = { ApiEnvironment.ApiAccessScope };
            string[] roles = { "owner", "admin", "editor", "reader" };
            context.ValidateAppRole(roles);
            context.VerifyUserHasAnyAcceptedScope(scopes);
        }

        private static void CheckUserIsEditor(HttpContext context)
        {
            string[] scopes = { ApiEnvironment.ApiAccessScope };
            string[] roles = { "owner", "admin", "editor" };
            context.ValidateAppRole(roles);
            context.VerifyUserHasAnyAcceptedScope(scopes);
        }

        private static void CheckUserIsAdmin(HttpContext context)
        {
            string[] scopes = { ApiEnvironment.ApiAccessScope };
            string[] roles = { "owner", "admin" };
            context.ValidateAppRole(roles);
            context.VerifyUserHasAnyAcceptedScope(scopes);
        }

        private static void CheckUserIsOwner(HttpContext context)
        {
            string[] scopes = { ApiEnvironment.ApiAccessScope };
            string[] roles = { "owner" };
            context.ValidateAppRole(roles);
            context.VerifyUserHasAnyAcceptedScope(scopes);
        }

        /// <summary>
        /// Defines a ClientPrincipal returned when /.auth/me is called
        /// </summary>
        public struct Auth
        {
            public ClientPrincipal clientPrincipal { get; set; }
        }

        public struct ClientPrincipal
        {
        
            public string? identityProvider { get; set; }
            public string? userId { get; set; }
            public string? name { get; set; }
            public string? userDetails { get; set; }
            public List<string>? userRoles { get; set; }
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

