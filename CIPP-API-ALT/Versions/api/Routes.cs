using CurrentApi = CIPP_API_ALT.Api.v10;
using CIPP_API_ALT.Data.Logging;
using CIPP_API_ALT.Common;

namespace CIPP_API_ALT.Api
{
    /// <summary>
    /// /api is basically a skeleton set of routes that always point to the latest /vX.Y api
    /// </summary>
    public static class Routes
    {
        public static void InitRoutes(ref WebApplication app)
        {
            #region API Routes
            /// <summary>
            /// /api/CurrentRouteVersion
            /// </summary>
            app.MapGet(string.Format("/{0}/CurrentRouteVersion",ApiEnvironment.ApiHeader), () =>
            {
                return CurrentApi.Routes.CurrentRouteVersion();

            }).WithName(string.Format("/{0}/CurrentRouteVersion", ApiEnvironment.ApiHeader)).ExcludeFromDescription();

            /// <summary>
            /// /api/GetDashboard
            /// </summary>
            app.MapGet(string.Format("/{0}/GetDashboard", ApiEnvironment.ApiHeader), async (HttpContext context, HttpRequest request) =>
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
                    return await CurrentApi.Routes.GetDashboard(context, accessingUser);
                }
                catch (UnauthorizedAccessException)
                {
                    context.Response.StatusCode = 401;
                    return Results.Unauthorized();
                }
            })
            .WithName(string.Format("/{0}/GetDashboard", ApiEnvironment.ApiHeader)).ExcludeFromDescription();

            /// <summary>
            /// /api/GetVersion
            /// </summary>
            app.MapGet(string.Format("/{0}/GetVersion", ApiEnvironment.ApiHeader), async (HttpContext context, HttpRequest request, string LocalVersion) =>
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
                    return await CurrentApi.Routes.GetVersion(context, LocalVersion, accessingUser);
                }
                catch (UnauthorizedAccessException)
                {
                    context.Response.StatusCode = 401;
                    return Results.Unauthorized();
                }
            })
            .WithName(string.Format("/{0}/GetVersion", ApiEnvironment.ApiHeader)).ExcludeFromDescription();

            /// <summary>
            /// /api/Heartbeat
            /// </summary>
            app.MapGet(string.Format("/{0}/Heartbeat", ApiEnvironment.ApiHeader), () =>
            {
                return new CurrentApi.Routes.Heartbeat();
            })
            .WithName(string.Format("/{0}/Heartbeat", ApiEnvironment.ApiHeader)).ExcludeFromDescription();

            /// <summary>
            /// /api/ListDomains
            /// </summary>
            app.MapGet(string.Format("/{0}/ListDomains", ApiEnvironment.ApiHeader), async (HttpContext context, HttpRequest request, string TenantFilter) =>
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
                    return await CurrentApi.Routes.ListDomains(context, TenantFilter, accessingUser);
                }
                catch (UnauthorizedAccessException)
                {
                    context.Response.StatusCode = 401;
                    return Results.Unauthorized();
                }
            })
            .WithName(string.Format("/{0}/ListDomains", ApiEnvironment.ApiHeader)).ExcludeFromDescription();

            /// <summary>
            /// /api/ListSites
            /// </summary>
            app.MapGet("/{0}/ListSites", async (HttpContext context, HttpRequest request, string TenantFilter, string Type, string? UserUPN) =>
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
                    return await CurrentApi.Routes.ListSites(context, TenantFilter, Type, UserUPN ?? string.Empty, accessingUser);
                }
                catch (UnauthorizedAccessException)
                {
                    context.Response.StatusCode = 401;
                    return Results.Unauthorized();
                }

            })
            .WithName(string.Format("/{0}/ListSites", ApiEnvironment.ApiHeader)).ExcludeFromDescription();

            /// <summary>
            /// /api/ListTenants
            /// </summary>
            app.MapGet(string.Format("/{0}/ListTenants", ApiEnvironment.ApiHeader) , async (HttpContext context, HttpRequest request, bool? AllTenantSelector) =>
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
                    return await CurrentApi.Routes.ListTenants(context, AllTenantSelector ?? false, accessingUser);
                }
                catch (UnauthorizedAccessException)
                {
                    context.Response.StatusCode = 401;
                    return Results.Unauthorized();
                }
            })
            .WithName(string.Format("/{0}/ListTenants", ApiEnvironment.ApiHeader)).ExcludeFromDescription();

            /// <summary>
            /// /api/ListUserConditionalAccessPolicies
            /// </summary>
            app.MapGet(string.Format("/{0}/ListUserConditionalAccessPolicies", ApiEnvironment.ApiHeader), async (HttpContext context, HttpRequest request, string TenantFilter, string UserId) =>
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
                    return await CurrentApi.Routes.ListUserConditionalAccessPolicies(context, TenantFilter, UserId ?? string.Empty, accessingUser);
                }
                catch (UnauthorizedAccessException)
                {
                    context.Response.StatusCode = 401;
                    return Results.Unauthorized();
                }
            })
            .WithName(string.Format("/{0}/ListUserConditionalAccessPolicies", ApiEnvironment.ApiHeader)).ExcludeFromDescription();

            /// <summary>
            /// /api/ListUserDevices
            /// </summary>
            app.MapGet(string.Format("/{0}/ListUserDevices", ApiEnvironment.ApiHeader), async (HttpContext context, HttpRequest request, string TenantFilter, string UserId) =>
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
                    return await CurrentApi.Routes.ListUserDevices(context, TenantFilter, UserId ?? string.Empty, accessingUser);
                }
                catch (UnauthorizedAccessException)
                {
                    context.Response.StatusCode = 401;
                    return Results.Unauthorized();
                }
            })
            .WithName(string.Format("/{0}/ListUserDevices", ApiEnvironment.ApiHeader)).ExcludeFromDescription();

            /// <summary>
            /// /api/ListUserMailboxDetails
            /// </summary>
            app.MapGet(string.Format("/{0}/ListUserMailboxDetails", ApiEnvironment.ApiHeader), async (HttpContext context, HttpRequest request, string TenantFilter, string UserId) =>
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
                    return await CurrentApi.Routes.ListUserMailboxDetails(context, TenantFilter, UserId ?? string.Empty, accessingUser);
                }
                catch (UnauthorizedAccessException)
                {
                    context.Response.StatusCode = 401;
                    return Results.Unauthorized();
                }
            })
            .WithName(string.Format("/{0}/ListUserMailboxDetails", ApiEnvironment.ApiHeader)).ExcludeFromDescription();

            /// <summary>
            /// /api/ListUserSigninLogs
            /// </summary>
            app.MapGet(string.Format("/{0}/ListUserSigninLogs", ApiEnvironment.ApiHeader), async (HttpContext context, HttpRequest request, string TenantFilter, string UserId) =>
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
                    return await CurrentApi.Routes.ListUserSigninLogs(context, TenantFilter, UserId ?? string.Empty, accessingUser);
                }
                catch (UnauthorizedAccessException)
                {
                    context.Response.StatusCode = 401;
                    return Results.Unauthorized();
                }
            }).WithName(string.Format("/{0}/ListUserSigninLogs", ApiEnvironment.ApiHeader)).ExcludeFromDescription();

            /// <summary>
            /// /api/ListUsers
            /// </summary>
            app.MapGet(string.Format("/{0}/ListUsers", ApiEnvironment.ApiHeader), async (HttpContext context, HttpRequest request, string TenantFilter, string? UserId) =>
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
                    return await CurrentApi.Routes.ListUsers(context, TenantFilter, UserId ?? string.Empty, accessingUser);
                }
                catch (UnauthorizedAccessException)
                {
                    context.Response.StatusCode = 401;
                    return Results.Unauthorized();
                }
            })
            .WithName(string.Format("/{0}/ListUsers", ApiEnvironment.ApiHeader)).ExcludeFromDescription();
            #endregion
        }
    }
}