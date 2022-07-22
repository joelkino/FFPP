using Asp.Versioning.Builder;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using CIPP_API_ALT.Common;
using CIPP_API_ALT.v10.Dashboards;
using CIPP_API_ALT.v10.Users;
using CIPP_API_ALT.v10.Tenants;

namespace CIPP_API_ALT.v10
{
    public static class Routes
    {
        public static void InitRoutes(ref WebApplication app)
        {
            #region API Routes
            /// <summary>
            /// /api/CurrentRouteVersion
            /// </summary>
            app.MapGet("/v{version:apiVersion}/CurrentRouteVersion", () =>
            {
                return CurrentRouteVersion();

            }).WithName(string.Format("/v{0}/CurrentRouteVersion",ApiEnvironment.ApiV10.GroupVersion)).WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);

            /// <summary>
            /// /api/GetDashboard
            /// </summary>
            app.MapGet("/v{version:apiVersion}/GetDashboard", async () =>
            {
                return await CippDashboard.GetHomeData();
            })
            .WithName("GetDashboard").WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);

            /// <summary>
            /// /api/GetVersion
            /// </summary>
            app.MapGet("/v{version:apiVersion}/GetVersion", async (HttpContext context, string LocalVersion) =>
            {
                string[] scopes = { "user_impersonation" };
                context.VerifyUserHasAnyAcceptedScope(scopes);
                return await CippDashboard.CheckVersions(LocalVersion);
            })
            .WithName("GetVersion").WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);

            /// <summary>
            /// /api/Heartbeat
            /// </summary>
            app.MapGet("/v{version:apiVersion}/Heartbeat", () =>
            {
                return new Utilities.Heartbeat();
            })
            .WithName("Heartbeat").WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);

            /// <summary>
            /// /api/ListTenants
            /// </summary>
            app.MapGet("/v{version:apiVersion}/ListTenants", [RequiredScope(RequiredScopesConfigurationKey = "AzureAD:Scopes")][Authorize(Roles = "reader")] async (bool? AllTenantSelector) =>
            {
                return await Tenant.GetTenants(allTenantSelector: AllTenantSelector ?? false);
            })
            .WithName("ListTenants").WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);


            app.MapGet("/v{version:apiVersion}/ListUserConditionalAccessPolicies", async (HttpRequest request, string TenantFilter, string UserId) =>
            {
                var user = request.Headers["x-ms-client-principal"];
                await User.GetUserConditionalAccessPolicies(TenantFilter, UserId ?? string.Empty, user);
            })
            .WithName("ListUserConditionalAccessPolicies").WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);

            /// <summary>
            /// /api/ListUsers
            /// </summary>
            app.MapGet("/v{version:apiVersion}/ListUsers", async (HttpRequest request, string TenantFilter, string? UserId) =>
            {
                var user = request.Headers["x-ms-client-principal"];
                return await User.GetUsers(user, TenantFilter, UserId ?? string.Empty);
            })
            .WithName("ListUsers").WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);
            #endregion
        }

        public static Utilities.CurrentApiRoute CurrentRouteVersion()
        {
            return new Utilities.CurrentApiRoute();
        }
    }
}

