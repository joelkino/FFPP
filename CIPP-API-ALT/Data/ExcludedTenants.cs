using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using CIPP_API_ALT.Common;

namespace CIPP_API_ALT.Data
{
    /// <summary>
    /// Entity Framework Class used to create and manage ExcludedTenants in a DB
    /// </summary>
    public class ExcludedTenants : DbContext
    {
        public static readonly ExcludedTenants ExcludedTenantsDb = new();
        public string DbPath { get; }
        private DbSet<ExcludedTenant>? _excludedTenantEntries { get; set; }

        public ExcludedTenants()
        {
            DbPath = ApiEnvironment.DataDir + "/SQLite/ExcludedTenants.db";

            // Create folder for DB if it doesn't exist
            if (!Directory.Exists(ApiEnvironment.DataDir + "/SQLite"))
            {
                Directory.CreateDirectory(ApiEnvironment.DataDir + "/SQLite");
            }
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
                string username = string.Empty;

                if (!string.IsNullOrEmpty(user))
                {
                    // Decoding user from the x-ms-s-client-principal header value passed in as a string
                    JsonDocument jsonDoc = await JsonDocument.ParseAsync(new MemoryStream(Convert.FromBase64String(user)));
                    username = jsonDoc.RootElement.EnumerateObject().FirstOrDefault(p => p.Name == "userDetails").ToString();
                }

                if (string.IsNullOrEmpty(username))
                {
                    username = "CIPP";
                }

                WriteExcludedTenant(new ExcludedTenant { TenantDefaultDomain = tenantDefaultDomain, Username = username, DateString = DateTime.Now.ToString("dd-MM-yyyy") });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception writing  in CippLogs: {0}, Inner Exception: {1}", ex.Message, ex.InnerException.Message);
                throw;
            }
        }

        /// <summary>
        /// All of the tenants we have excluded
        /// </summary>
        /// <returns>List of ExcludedTenant objects</returns>
        public List<ExcludedTenant> GetExcludedTenants()
        {
            return _excludedTenantEntries.ToList();
        }

        /// <summary>
        /// Checks a given defaultDomainName to see if it resides in the ExcludedTenants DB
        /// </summary>
        /// <param name="defaultDomainName">defaultDomainName to check</param>
        /// <returns>bool which indicates if defaultDomainName is in ExcludedTenants DB</returns>
        public bool IsExcluded(string defaultDomainName)
        {
            if (_excludedTenantEntries.Find(defaultDomainName) == null)
            {
                return false;
            }

            return true;
        }
        #endregion

        #region Private Methods
        // Writes an ExcludedTenant object to the ExcludedTenants DB
        private void WriteExcludedTenant(ExcludedTenant excludedTenant)
        {
            if(!IsExcluded(excludedTenant.TenantDefaultDomain))
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

