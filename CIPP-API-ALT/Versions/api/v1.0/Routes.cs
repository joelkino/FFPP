using Asp.Versioning.Builder;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using CIPP_API_ALT.Common;
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
        private static string _versionHeader = "v1.0";

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
                string accessingUser = request.Headers["x-ms-client-principal"];
                return await GetDashboard(context, accessingUser);
            })
            .WithName(string.Format("/{0}/GetDashboard", _versionHeader)).WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);

            /// <summary>
            /// /v1.0/GetVersion
            /// </summary>
            app.MapGet("/v{version:apiVersion}/GetVersion", async (HttpContext context, HttpRequest request, string LocalVersion) =>
            {
                string accessingUser = request.Headers["x-ms-client-principal"];
                return await GetVersion(context, LocalVersion, accessingUser);
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
                string accessingUser = request.Headers["x-ms-client-principal"];
                return await ListDomains(context, TenantFilter, accessingUser);
            })
            .WithName(string.Format("/{0}/ListDomains", _versionHeader)).WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);


            /// <summary>
            /// /v1.0/ListTenants
            /// </summary>
            app.MapGet("/v{version:apiVersion}/ListTenants", async (HttpContext context, HttpRequest request, bool ? AllTenantSelector) =>
            {
                string accessingUser = request.Headers["x-ms-client-principal"];
                return await ListTenants(context, AllTenantSelector ?? false, accessingUser);
            })
            .WithName(string.Format("/{0}/ListTenants", _versionHeader)).WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);

            /// <summary>
            /// /v1.0/ListUserConditionalAccessPolicies
            /// </summary>
            app.MapGet("/v{version:apiVersion}/ListUserConditionalAccessPolicies", async (HttpContext context, HttpRequest request, string TenantFilter, string UserId) =>
            {
                string accessingUser = request.Headers["x-ms-client-principal"];
                return await ListUserConditionalAccessPolicies(context, TenantFilter, UserId ?? string.Empty, accessingUser);
            })
            .WithName(string.Format("/{0}/ListUserConditionalAccessPolicies", _versionHeader)).WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);

            /// <summary>
            /// /v1.0/ListUsers
            /// </summary>
            app.MapGet("/v{version:apiVersion}/ListUsers", async (HttpContext context, HttpRequest request, string TenantFilter, string? UserId) =>
            {
                var accessingUser = request.Headers["x-ms-client-principal"];
                return await ListUsers(context, TenantFilter, UserId ?? string.Empty, accessingUser);
            })
            .WithName(string.Format("/{0}/ListUsers", _versionHeader)).WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiV10);
            #endregion
        }

        public static CurrentApiRoute CurrentRouteVersion()
        {
            return new CurrentApiRoute();
        }

        public static async Task<CippDashboard> GetDashboard(HttpContext context, string accessingUser, bool simulateAuthed=false)
        {
            if (!simulateAuthed)
            {
                string[] scopes = { "cipp-api-alt.access", "reader" };
                context.VerifyUserHasAnyAcceptedScope(scopes);
            }

            return await CippDashboard.GetHomeData(accessingUser);
        }

        public static async Task<CippDashboard.Versions>GetVersion(HttpContext context, string LocalVersion, string accessingUser, bool simulateAuthed= false)
        {
            if (!simulateAuthed)
            {
                string[] scopes = { "reader" };
                context.VerifyUserHasAnyAcceptedScope(scopes);
            }

            return await CippDashboard.CheckVersions(accessingUser, LocalVersion);
        }

        public static async Task<List<Tenant>> ListTenants(HttpContext context, bool? AllTenantSelector, string accessingUser, bool simulateAuthed = false)
        {
            if (!simulateAuthed)
            {
                string[] scopes = { "cipp-api-alt.access", "reader" };
                context.VerifyUserHasAnyAcceptedScope(scopes);
            }

            return await Tenant.GetTenants(accessingUser, allTenantSelector: AllTenantSelector ?? false);
        }

        public static async Task<bool> ListUserConditionalAccessPolicies(HttpContext context, string TenantFilter, string UserId, string accessingUser, bool simulateAuthed = false)
        {
            if (!simulateAuthed)
            {
                string[] scopes = { "cipp-api-alt.access", "reader" };
                context.VerifyUserHasAnyAcceptedScope(scopes);
            }

            return await User.GetUserConditionalAccessPolicies(TenantFilter, UserId ?? string.Empty, accessingUser);
        }

        public static async Task<List<User>> ListUsers(HttpContext context, string TenantFilter, string UserId, string accessingUser, bool simulateAuthed = false)
        {
            if (!simulateAuthed)
            {
                string[] scopes = { "cipp-api-alt.access", "reader" };
                context.VerifyUserHasAnyAcceptedScope(scopes);
            }

            return await User.GetUsers(accessingUser, TenantFilter, UserId ?? string.Empty);
        }

        public static async Task<List<Domain>> ListDomains(HttpContext context, string TenantFilter, string accessingUser, bool simulateAuthed = false)
        {
            if (!simulateAuthed)
            {
                string[] scopes = { "cipp-api-alt.access", "reader" };
                context.VerifyUserHasAnyAcceptedScope(scopes);
            }

            return await Domain.GetDomains(accessingUser, TenantFilter);
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

    }
}

