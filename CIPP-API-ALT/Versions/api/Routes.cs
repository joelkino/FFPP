using CurrentApi = CIPP_API_ALT.Api.v10;
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
                string accessingUser = request.Headers["x-ms-client-principal"];
                return await CurrentApi.Routes.GetDashboard(context, accessingUser);
            })
            .WithName(string.Format("/{0}/GetDashboard", ApiEnvironment.ApiHeader)).ExcludeFromDescription();

            /// <summary>
            /// /api/GetVersion
            /// </summary>
            app.MapGet(string.Format("/{0}/GetVersion", ApiEnvironment.ApiHeader), async (HttpContext context, HttpRequest request, string LocalVersion) =>
            {
                string accessingUser = request.Headers["x-ms-client-principal"];
                return await CurrentApi.Routes.GetVersion(context, LocalVersion, accessingUser);
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
                string accessingUser = request.Headers["x-ms-client-principal"];
                return await CurrentApi.Routes.ListDomains(context, TenantFilter, accessingUser);
            })
            .WithName(string.Format("/{0}/ListDomains", ApiEnvironment.ApiHeader)).WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);

            /// <summary>
            /// /api/ListTenants
            /// </summary>
            app.MapGet(string.Format("/{0}/ListTenants", ApiEnvironment.ApiHeader) , async (HttpContext context, HttpRequest request, bool? AllTenantSelector) =>
            {
                string accessingUser = request.Headers["x-ms-client-principal"];
                return await CurrentApi.Routes.ListTenants(context, AllTenantSelector ?? false, accessingUser);
            })
            .WithName(string.Format("/{0}/ListTenants", ApiEnvironment.ApiHeader)).WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);

            /// <summary>
            /// /api/ListUserConditionalAccessPolicies
            /// </summary>
            app.MapGet(string.Format("/{0}/ListUserConditionalAccessPolicies", ApiEnvironment.ApiHeader), async (HttpContext context, HttpRequest request, string TenantFilter, string UserId) =>
            {
                string accessingUser = request.Headers["x-ms-client-principal"];
                return await CurrentApi.Routes.ListUserConditionalAccessPolicies(context, TenantFilter, UserId ?? string.Empty, accessingUser);
            })
            .WithName(string.Format("/{0}/ListUserConditionalAccessPolicies", ApiEnvironment.ApiHeader)).ExcludeFromDescription();

            /// <summary>
            /// /api/ListUsers
            /// </summary>
            app.MapGet(string.Format("/{0}/ListUsers", ApiEnvironment.ApiHeader), async (HttpContext context, HttpRequest request, string TenantFilter, string? UserId) =>
            {
                var accessingUser = request.Headers["x-ms-client-principal"];
                return await CurrentApi.Routes.ListUsers(context, TenantFilter, UserId ?? string.Empty, accessingUser);
            })
            .WithName(string.Format("/{0}/ListUsers", ApiEnvironment.ApiHeader)).ExcludeFromDescription();
            #endregion
        }
    }
}