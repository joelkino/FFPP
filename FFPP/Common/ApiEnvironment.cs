﻿using Asp.Versioning;
using Asp.Versioning.Builder;

namespace FFPP.Common
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
        public static readonly string PreFetchDir = CacheDir + "/Pre-Fetch";
        public static readonly string UsersPreFetchDir = PreFetchDir + "/Users";
        public static readonly string CachedTenantsFile = WorkingDir + "/Data/Cache/tenants.cache.json";
        public static readonly string LicenseConversionTableFile = WorkingDir + "/Data/ConversionTable.csv";
        public static readonly string LicenseConversionTableMisfitsFile = WorkingDir + "/Data/ConversionTableMisfits.csv";
        public static readonly string ZeroConfPath = WorkingDir + "/Data/api.zeroconf.json";
        public static readonly string ApiVersionFile = WorkingDir + "/version_latest.txt";
        public static readonly string ApiHeader = "api";
        public static readonly string ApiAccessScope = "ffpp-api.access";
        public static readonly string FfppSimulatedAuthUsername = "FFPP Simulated Authentication";
        public static string FfppFrontEndUri = "https://localhost:7074";
        public static List<double> ApiRouteVersions = new(){1.0};
        public static ApiVersionSet? ApiVersionSet { get; set; }
        public static readonly ApiVersion ApiDev = new(1.1);
        public static readonly ApiVersion ApiV10 = new(ApiRouteVersions[0]);
        public static readonly ApiVersion ApiV11 = ApiDev;
        public static readonly ApiVersion ApiCurrent = new(ApiRouteVersions[^1]);
        public static readonly string RemoteFfppAltApiVersion = "https://raw.githubusercontent.com/White-Knight-IT/FFPP/main/FFPP/version_latest.txt";
        public static readonly string RemoteFfppVersion = "https://raw.githubusercontent.com/KelvinTegelaar/CIPP/master/version_latest.txt";
        public static readonly DateTime Started = DateTime.UtcNow;
        public static bool SimulateAuthenticated = false;
        public static bool ShowDevEnvEndpoints = false;
        public static bool ShowSwaggerUi = false;
        public static bool RunSwagger = false;
        public static bool UseHttpsRedirect { get; set; }

        /// <summary>
        /// Build data directories (including cache directories) if they don't exist
        /// </summary>
        public static void DataDirectoriesBuild()
        {
            Directory.CreateDirectory(DatabaseDir);
            CacheDirectoriesBuild();
        }

        /// <summary>
        /// Build cache directories if they don't exist
        /// </summary>
        public static void CacheDirectoriesBuild()
        {
            Directory.CreateDirectory(UsersPreFetchDir);
        }

        /// <summary>
        /// Gets the Api version
        /// </summary>
        /// <returns>Api version object</returns>
        public static FfppVersion GetApiBinaryVersion()
        {
            return new(File.ReadLines(ApiVersionFile).First());           
        }

        /// <summary>
        /// Used to define a Version structure for use in the api
        /// </summary>
        public struct FfppVersion
        {
            public FfppVersion(string rawVersion)
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

