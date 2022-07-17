using System.Text.Json;
using CIPP_API_ALT.Common;
using CIPP_API_ALT.Data;

namespace CIPP_API_ALT.Tenants
{
    public class Tenant
    {
        public Tenant(string customerId, string displayName, string defaultDomainName)
        {
			CustomerId = customerId;
			DefaultDomainName = defaultDomainName;
			DisplayName = displayName;
        }

		/// <summary>
        /// Gets a CustomerId from a known DefaultDomain
        /// </summary>
        /// <param name="defaultDomain">DefaultDomain that we wish to find the matching ClientIdf</param>
        /// <returns>ClientId</returns>
		public async static Task<string> GetClientIdFromDefaultDomain(string defaultDomain)
        {
			return (await GetTenants(false)).Find(x => x.DefaultDomainName.Equals(defaultDomain)).CustomerId;
        }

		/// <summary>
		/// Returns the tenants managed in a partner relationship
		/// </summary>
		/// <param name="exclude">True excludes ExcludedTenants</param>
		/// <returns>Tenants managed in a partner relationship</returns>
		public async static Task<List<Tenant>> GetTenants(bool exclude = true, bool allTenantSelector = false)
		{
			List<Tenant> allTenants = new();
			List<Tenant> outTenants = new();

			FileInfo cacheFile = new(ApiEnvironment.CachedTenantsFile);
			string uri = "https://graph.microsoft.com/beta/contracts?$select=customerId,defaultDomainName,displayName&$top=999";
			
			void CheckExclusions(Tenant[] tenantArray, ref List<Tenant> listTenants, ref List<Tenant> allTenants)
            {
				if(allTenantSelector)
                {
					listTenants.Add(new Tenant("AllTenants", "*All Tenants", "AllTenants"));
                }

				foreach (Tenant t in tenantArray)
				{
					allTenants.Add(t);

					// If we want to exclude from ExcludedTenants from outTenants and it is in ExcludedTenants
					if (exclude && ExcludedTenants.ExcludedTenantsDb.IsExcluded(t.DefaultDomainName))
					{
						// We exclude
						continue;
					}

					listTenants.Add(t);
				}
			}

			if(cacheFile.Exists && cacheFile.LastWriteTimeUtc >= DateTime.UtcNow.AddMinutes(-7))
            {
				//Read tenants from cache as they were cached in last 15m
				CheckExclusions(Utilities.ReadJsonFromFile<List<Tenant>>(cacheFile.FullName).ToArray<Tenant>(),ref outTenants, ref allTenants);
				return outTenants;

			}

			List<JsonElement> tenants = await RequestHelper.NewGraphGetRequest(uri, ApiEnvironment.Secrets.TenantId);

			List<Tenant[]> tenantArrayList = Utilities.ParseJson<Tenant[]>(tenants);

			// We flatten our list of tenant arrays into a list of tenants instead
			foreach(Tenant[] ta in tenantArrayList)
            {
				CheckExclusions(ta, ref outTenants, ref allTenants);
            }

			// We write all tenants to cache not just unexcluded tenants
			Utilities.WriteJsonToFile<List<Tenant>>(allTenants, cacheFile.FullName);
			return outTenants;
		}

		public string CustomerId { get; set; }
		public string DefaultDomainName { get; set; }
		public string DisplayName { get; set; }
	}
}

