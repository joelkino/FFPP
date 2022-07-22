using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Asp.Versioning.Conventions;
using Asp.Versioning.Builder;

namespace CIPP_API_ALT.Common
{
    /// <summary>
    /// This class stores Environment variables and constants
    /// </summary>
    public static class ApiEnvironment
    {
        // Roles for managing permissions
        public static readonly string RoleOwner = "owner";
        public static readonly string RoleAdmin = "admin";
        public static readonly string RoleEditor = "editor";
        public static readonly string RoleReader = "reader";
        #if DEBUG
        public static readonly bool IsDebug = true;
        #else
        public static readonly bool IsDebug = false;
        #endif
        public static readonly string WorkingDir = Directory.GetCurrentDirectory();
        public static readonly string DataDir = WorkingDir + "/Data";
        public static readonly string DatabaseDir = DataDir + "/SQLite";
        public static readonly string CacheDir = DataDir + "/Cache";
        public static readonly string CachedTenantsFile = WorkingDir + "/Data/Cache/tenants.cache.json";
        public static readonly string LicenseConversionTableFile = WorkingDir + "/Data/ConversionTable.csv";
        public static readonly string LicenseConversionTableMisfitsFile = WorkingDir + "/Data/ConversionTableMisfits.csv";
        public static readonly string ApiVersionFile = WorkingDir + "/version_latest.txt";
        public static readonly string ApiCurrentVersionHeader = "/api";
        public static readonly List<double> ApiRouteVersions = new(){0.1,1.0,1.1};
        public static ApiVersionSet ApiVersionSet { get; set; }
        public static readonly ApiVersion ApiDev = new(ApiRouteVersions[0]);
        public static readonly ApiVersion ApiV10 = new(ApiRouteVersions[1]);
        public static readonly ApiVersion ApiV11 = new(ApiRouteVersions[2]);
        public static readonly ApiVersion ApiCurrent = new(ApiRouteVersions[^1]);

        public static readonly string RemoteCippAltApiVersion = "https://raw.githubusercontent.com/White-Knight-IT/CIPP-API-ALT/main/CIPP-API-ALT/version_latest.txt";
        public static readonly string RemoteCippVersion = "https://raw.githubusercontent.com/KelvinTegelaar/CIPP/master/version_latest.txt";
        public static readonly DateTime Started = DateTime.UtcNow;
        public static bool UseHttpsRedirect { get; set; }


        /// <summary>
        /// Gets the Api version
        /// </summary>
        /// <returns>Api version object</returns>
        public static CippVersion GetApiVersion()
        {
            return new(File.ReadLines(ApiVersionFile).First());           
        }

        /// <summary>
        /// Used to define a Version structure for use in the api
        /// </summary>
        public struct CippVersion
        {
            public CippVersion(string rawVersion)
            {
                RawVersion = rawVersion;
                Version = Version.Parse(rawVersion.Split(":")[0]);
                DisplayVersion = string.Format("{0}-{1} ({2})", Version, RawVersion.Split(":")[1], RawVersion.Split(":")[2]);
            }

            public string RawVersion { get; set; }
            public Version Version { get; set; }
            public string DisplayVersion { get; set; }
        }

        /// <summary>
        /// This is used to store the secrets that we will retrieve from user-secrets in dev, or a key vault in prod.
        /// </summary>
        public struct Secrets
        {
            public static string? ApplicationId { get; set; }
            public static string? ApplicationSecret { get; set; }
            public static string? TenantId { get; set; }
            public static string? RefreshToken { get; set; }
            public static string? ExchangeRefreshToken { get; set; }
        }
    }

}

