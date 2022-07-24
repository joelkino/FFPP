
using System.Text.Json;
using CIPP_API_ALT.Common;
using CIPP_API_ALT.Data.Logging;

namespace CIPP_API_ALT.Api.v10.Tenants
{
    public class Domain
    {
		public string? authenticationType { get; set; }
		public string? availabilityStatus { get; set; }
		public string? id { get; set; }
		public bool? isAdminManaged { get; set; }
		public bool? isDefault { get; set; }
		public bool? isInitial { get; set; }
		public bool? isRoot { get; set; }
		public bool? isVerified { get; set; }
		public int? passwordNotificationWindowInDays { get; set; }
		public int? passwordValidityPeriodInDays { get; set; }
		public DomainState? state { get; set; }
		public List<string>? supportedServices { get; set; }

		/// <summary>
        /// 
        /// </summary>
        /// <param name="tenantFilter"></param>
        /// <returns></returns>
		public async static Task<List<Domain>> GetDomains(string accessingUser, string tenantFilter)
		{
			List<Domain> outDomains = new();

			using (CippLogs logsDb = new())
			{
				await logsDb.LogRequest("Accessed this API", accessingUser, "Debug", "", "ListDomains");
			}

			List<JsonElement> domainsRaw = await RequestHelper.NewGraphGetRequest("https://graph.microsoft.com/beta/domains", tenantFilter);
			List<Domain[]> domainsArrayList = Utilities.ParseJson<Domain[]>(domainsRaw);

			foreach (Domain[] domainArray in domainsArrayList)
			{
				foreach (Domain dom in domainArray)
				{
					outDomains.Add(dom);
				}
			}

			return outDomains.OrderBy(x => x.isDefault).ToList();
		}

		public struct DomainState
		{
			public string? lastActionDateTime { get; set; }
			public string? operation { get; set; }
			public string? status { get; set; }
		}
	}
}

