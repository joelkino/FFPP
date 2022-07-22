using CurrentApi = CIPP_API_ALT.v10;
using CIPP_API_ALT.Common;
using Asp.Versioning.Builder;
using Asp.Versioning;

namespace CIPP_API_ALT.Api
{
    /// <summary>
    /// /api is basically a skeleton set of routes that always point to the latest /vX.Y api
    /// </summary>
    public static class Routes
    {
        public static readonly string ApiSpecVersion = ApiEnvironment.ApiCurrentVersionHeader;

        public static void InitRoutes(ref WebApplication app)
        {
            #region API Routes
            /// <summary>
            /// /api/CurrentRouteVersion
            /// </summary>
            app.MapGet(ApiSpecVersion + "/CurrentRouteVersion", () =>
            {
                return CurrentApi.Routes.CurrentRouteVersion();

            }).WithName("CurrentRouteVersion").WithApiVersionSet(ApiEnvironment.ApiVersionSet).MapToApiVersion(ApiEnvironment.ApiCurrent);
            #endregion
        }
    }
}