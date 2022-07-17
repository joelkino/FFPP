using System.Text;

namespace CIPP_API_ALT.Common
{
    /// <summary>
    /// This class stores Environment variables and constants
    /// </summary>
    public static class ApiEnvironment
    {
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
        public static readonly string ApiVersionFile = WorkingDir + "/version_latest.txt";
        public static readonly string ApiVersionHeader = "/api";
        public static readonly string RemoteCippAltApiVersion = "https://raw.githubusercontent.com/White-Knight-IT/CIPP-API-ALT/main/CIPP-API-ALT/version_latest.txt";
        public static readonly string RemoteCippVersion = "https://raw.githubusercontent.com/KelvinTegelaar/CIPP/master/version_latest.txt";

        /// <summary>
        /// This is used to store the secrets that we will retrieve from user-secrets in dev, or a key vault in prod.
        /// </summary>
        public static class Secrets
        {
#region Private Members
            private static string? _applicationId;
            private static string? _applicationSecret;
            private static string? _tenantId;
            private static string? _refreshToken;
            private static string? _exchangeRefreshToken;
#endregion

#region Properties
            public static string ApplicationId { get => Utilities.XORString(_applicationId, "ApplicationId"); set => _applicationId = Utilities.XORString(value, "ApplicationId"); }
            public static string ApplicationSecret { get => Utilities.XORString(_applicationSecret, "ApplicationSecret"); set => _applicationSecret = Utilities.XORString(value, "ApplicationSecret"); }
            public static string TenantId { get => Utilities.XORString(_tenantId, "TenantId"); set => _tenantId = Utilities.XORString(value, "TenantId"); }
            public static string RefreshToken { get => Utilities.XORString(_refreshToken, "RefreshToken"); set => _refreshToken = Utilities.XORString(value, "RefreshToken"); }
            public static string ExchangeRefreshToken { get => Utilities.XORString(_exchangeRefreshToken, "ExchangeRefreshToken"); set => _exchangeRefreshToken = Utilities.XORString(value, "ExchangeRefreshToken"); }
#endregion

        }
    }

}

