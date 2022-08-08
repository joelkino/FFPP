using System.Text.Json;
using FFPP.Api.v10.Tenants;

namespace FFPP.Common
{
    /// <summary>
	/// This class is used to handle anything we can automatically configure, the
    /// idea is to reduce the amount of configuration the user has to do mandatory
    /// to the absolute minimum necessary, this allows us to setup secure defaults etc
	/// </summary>
    public class ApiZeroConfiguration
    {
        public string? TenantId { get; set; }
        public string? ClientId { get; set; }
        public string? Domain { get; set; }
        public string? Instance { get; set; }
        public string? Scopes { get; set; }
        public string? AuthorizationUrl { get; set; }
        public string? TokenUrl { get; set; }
        public string? ApiScope { get; set; }
        public string? OpenIdClientId { get; set; }
        public string? CallbackPath { get; set; }

        public static bool ZeroConfExists()
        {
            return File.Exists(ApiEnvironment.ZeroConfPath);
        }

        public static async Task<bool> Setup(string ownerTenant)
        {
            // TenantId is GUID (CustomerId) and not domain
            if (!ownerTenant.Contains('.'))
            {
                ownerTenant = await Tenant.GetDefaultDomainFromCustomerId(ownerTenant);
            }

            string domain = ownerTenant;
            string scopes = ApiEnvironment.ApiAccessScope;
            string authorizationUrl = string.Format("https://login.microsoftonline.com/{0}/oauth2/v2.0/authorize",ownerTenant);
            string tokenUrl = string.Format("https://login.microsoftonline.com/{0}/oauth2/v2.0/token", ownerTenant);
            string instance = "https://login.microsoftonline.com/";
            string apiScopeGuid = Guid.NewGuid().ToString();

            // step one - create SAM SPA that the Swagger UI will use to authenticate

            JsonElement samSpa = await Sam.CreateSAMAuthApp("FFPP UI", Sam.SamAppType.Spa, domain, spaRedirectUri: new string[] { string.Format("{0}/swagger/oauth2-redirect.html", ApiEnvironment.FfppFrontEndUri.TrimEnd('/')), string.Format("{0}/index.html", ApiEnvironment.FfppFrontEndUri.TrimEnd('/'))});
            string openIdClientId = samSpa.GetProperty("appId").GetString() ?? string.Empty;
            if (!openIdClientId.Equals(string.Empty))
            {
                // Wait 10 seconds to ensure the SPA gets registered
                await Task.Delay(10000);

                // step two - create SAM that will act as the authentication hub of the API
                JsonElement samApi = await Sam.CreateSAMAuthApp("FFPP API", Sam.SamAppType.Api, domain, openIdClientId, scopeGuid: apiScopeGuid);
                string clientId = samApi.GetProperty("appId").GetString() ?? string.Empty;
                string idUri = samApi.GetProperty("identifierUris").EnumerateArray().ToArray()[0].GetString() ?? string.Empty;
                string apiScope = string.Format("{0}/{1}", idUri, ApiEnvironment.ApiAccessScope);

                if (!clientId.Equals(string.Empty))
                {
                    ApiZeroConfiguration zeroConf = new()
                    {
                        TenantId = ownerTenant,
                        ClientId = clientId,
                        Domain = domain,
                        Instance = instance,
                        Scopes = scopes,
                        AuthorizationUrl = authorizationUrl,
                        TokenUrl = tokenUrl,
                        ApiScope = apiScope,
                        OpenIdClientId = openIdClientId,
                        CallbackPath = "/signin-oidc"
                 
                    };

                    Utilities.WriteJsonToFile<ApiZeroConfiguration>(zeroConf, ApiEnvironment.ZeroConfPath);
                }
            }

            return false;
        }

        public static ApiZeroConfiguration Read()
        {
            return new ApiZeroConfiguration();
        }

        public bool Save()
        {
            
            return false;
        }
    }
}

