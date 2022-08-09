using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using FFPP.Common;

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
            public Guid? userId { get; set; }
            public string? identityProvider { get; set; }
            public string? name { get; set; }
            public string? userDetails { get; set; }
            public List<string>? userRoles { get; set; }
            public string? photoData { get; set; }
        }
    }
}

