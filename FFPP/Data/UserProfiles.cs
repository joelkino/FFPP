using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using FFPP.Common;
using FFPP.Data.Logging;

namespace FFPP.Data
{
    /// <summary>
    /// Entity Framework Class used to create and manage UserProfiles in a DB
    /// </summary>
    public class UserProfiles : DbContext
    {
        public string DbPath { get; }
        private DbSet<UserProfile>? _userprofiles { get; set; }

        public UserProfiles()
        {
            DbPath = ApiEnvironment.DatabaseDir + "/UserProfiles.db";
        }

        #region Public Methods

        public void AddUserProfile(UserProfile user)
        {
            try
            {
                WriteUserProfile(user);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception writing  in UserProfiles: {0}, Inner Exception: {1}", ex.Message, ex.InnerException.Message ?? string.Empty);
                throw;
            }
        }

        public async Task<UserProfile>? GetUserProfile(Guid userId)
        {
            try
            {
                return await _userprofiles.FindAsync(userId);
            }
            catch
            {

            }

            return null;
        }

        public async Task<bool> UpdateUserProfile(UserProfile userProfile)
        {
            try
            {
                UserProfile? foundUser = await _userprofiles.FindAsync(userProfile.userId);

                if (foundUser != null)
                {
                    foundUser.photoData = userProfile.photoData;
                    foundUser.name = userProfile.name;
                    foundUser.defaultPageSize = userProfile.defaultPageSize;
                    foundUser.defaultUseageLocation = userProfile.defaultUseageLocation;
                    foundUser.identityProvider = userProfile.identityProvider;
                    foundUser.lastTenantCustomerId = userProfile.lastTenantCustomerId;
                    foundUser.lastTenantDomainName = userProfile.lastTenantDomainName;
                    foundUser.lastTenantName = userProfile.lastTenantName;
                    foundUser.theme = userProfile.theme;
                    foundUser.userDetails = userProfile.userDetails;
                    SaveChanges();

                    return true;
                }
            }
            catch(Exception ex)
            {
                using (FfppLogs logs = new())
                {
                    await logs.LogRequest(string.Format("Error updating user profile for {0} - {1}: {2} - {3}", userProfile.userId.ToString(), userProfile.name, ex.Message, ex.InnerException.Message ?? string.Empty), "FFPP", "Error", aPI: "UpdateUserProfile");
                }
            }

            return false;
        }
        #endregion

        #region Private Methods

        private async Task<bool> UserExists(Guid userId)
        {
            if (await _userprofiles.FindAsync(userId) == null)
            {
                return false;
            }

            return true;
        }

        // Writes a UserProfile object to the UserProfiles DB
        private async void WriteUserProfile(UserProfile userProfile)
        {
            if (! await UserExists(userProfile.userId))
            {
                Add(userProfile);
                SaveChanges();
            }
        }

        // Updates a UserProfile object to the UserProfiles DB if it exists
        #endregion

        // Tells EF that we want to use Sqlite and where the DB will reside
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite($"Data Source={DbPath}");
        }

        /// <summary>
        /// Represents a UserProfile object as it exists in the UserProfiles DB
        /// </summary>
        public class UserProfile
        {
            [Key] // Public key
            public Guid userId { get; set; }
            public string? identityProvider { get; set; }
            public string? name { get; set; }
            public string? userDetails { get; set; }
            [NotMapped] // We never save roles they may change and relying on old roles is security risk
            public List<string>? userRoles { get; set; }
            public string? theme { get; set; }
            public int? defaultPageSize { get; set; }
            public string? defaultUseageLocation { get; set; }
            public string? lastTenantName { get; set; }
            public string? lastTenantDomainName { get; set; }
            public string? lastTenantCustomerId { get; set; }
            public string? photoData { get; set; }
        }
    }
}

