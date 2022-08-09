using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using FFPP.Common;

namespace FFPP.Data
{
    /// <summary>
    /// Entity Framework Class used to create and manage ExcludedTenants in a DB
    /// </summary>
    public class ExcludedTenants : DbContext
    {
        public string DbPath { get; }
        private DbSet<ExcludedTenant>? _excludedTenantEntries { get; set; }

        public ExcludedTenants()
        {
            DbPath = ApiEnvironment.DatabaseDir + "/ExcludedTenants.db";
        }

        #region Public Methods
        /// <summary>
        /// Saves a tenant in ExcludedTenants DB
        /// </summary>
        /// <param name="tenantDefaultDomain"></param>
        /// <param name="user">User who excluded the tenant (x-ms-s-client-principal header value passed in as Base64 string)</param>
        public async void SetExcludedTenant(string tenantDefaultDomain, string user)
        {
            try
            {

                if (string.IsNullOrEmpty(user))
                {
                    user = "CIPP";
                }

                WriteExcludedTenant(new ExcludedTenant { TenantDefaultDomain = tenantDefaultDomain, Username = user, DateString = DateTime.Now.ToString("dd-MM-yyyy") });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception writing  in CippLogs: {0}, Inner Exception: {1}", ex.Message, ex.InnerException.Message ?? string.Empty);
                throw;
            }
        }

        /// <summary>
        /// All of the tenants we have excluded
        /// </summary>
        /// <returns>List of ExcludedTenant objects</returns>
        public async Task<List<ExcludedTenant>> GetExcludedTenants()
        {
            return await _excludedTenantEntries.ToListAsync() ?? new();
        }

        /// <summary>
        /// Checks a given defaultDomainName to see if it resides in the ExcludedTenants DB
        /// </summary>
        /// <param name="defaultDomainName">defaultDomainName to check</param>
        /// <returns>bool which indicates if defaultDomainName is in ExcludedTenants DB</returns>
        public async Task<bool> IsExcluded(string defaultDomainName)
        {
            if (await _excludedTenantEntries.FindAsync(defaultDomainName) == null)
            {
                return false;
            }

            return true;
        }
        #endregion

        #region Private Methods
        // Writes an ExcludedTenant object to the ExcludedTenants DB
        private async void WriteExcludedTenant(ExcludedTenant excludedTenant)
        {
            if (! await IsExcluded(excludedTenant.TenantDefaultDomain))
            {
                Add(excludedTenant);
                SaveChanges();
            }
        }
        #endregion

        // Tells EF that we want to use Sqlite and where the DB will reside
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite($"Data Source={DbPath}");
        }

        /// <summary>
        /// Represents an ExcludedTenant object as it exists in the ExcludedTenants DB
        /// </summary>
        public class ExcludedTenant
        {
            [Key] // Public key
            public string? TenantDefaultDomain { get; set; }
            public string? DateString { get; set; }
            public string? Username { get; set; }
        }
    }
}

