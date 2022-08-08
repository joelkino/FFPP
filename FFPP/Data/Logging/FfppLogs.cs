using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using FFPP.Common;

namespace FFPP.Data.Logging
{
    /// <summary>
    /// Entity Framework Class used to create and manage FFPP Logs in a DB
    /// </summary>
    public class FfppLogs : DbContext
    {
        public string DbPath { get; }
        private DbSet<LogEntry>? _logEntries { get; set; }

        public FfppLogs()
        {
            DbPath = ApiEnvironment.DataDir+"/SQLite/FfppLogs.db";
        }

        #region Public Methods
        /// <summary>
        /// Writes a LogEntry object to the FfppLogs DB & Console for debug logs
        /// </summary>
        /// <param name="message">Message to write</param>
        /// <param name="user">User who executed the code we are logging about (x-ms-s-client-principal header value passed in as Base64 string)</param>
        /// <param name="severity">info, debug, error</param>
        /// <param name="tenant">Tenant name the log entry applies to</param>
        /// <param name="aPI">API accessed to execute the code related to thios message</param>
        /// <returns>bool indicates successful write</returns>
        public async Task<bool> LogRequest(string message, string user, string severity, string tenant = "None", string aPI = "None")
        {
            try
            {

                if (string.IsNullOrEmpty(user))
                {
                    user = "FFPP";
                }

                if (severity.ToLower().Equals("debug") && !ApiEnvironment.IsDebug)
                {
                    Console.WriteLine("Not writing to log file - Debug mode is not enabled.");
                    return false;
                }

                // Write to console for debug environment
                DebugConsoleWrite(string.Format("[ {0} ] - {1} - {2} - {3} - {4} - {5} - {6}", DateTime.UtcNow.ToString(), severity, message, tenant, aPI, user, "false"));
                
                WriteLogEntry(new LogEntry { Severity = severity, Message = message, API = aPI, Tenant = tenant, Username = user, SentAsAlert = false });

                return true;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception writing log entry in FfppLogs: {0}, Inner Exception: {1}",ex.Message, ex.InnerException.Message ?? string.Empty);
                return false;
            }
        }

        /// <summary>
        /// Returns the Top 10 most recent CippLog entries
        /// </summary>
        /// <returns>10 most recent FfppLog entries</returns>
        public List<LogEntry> Top10LogEntries()
        {
            return _logEntries.OrderByDescending(x => x.Timestamp).Take(10).ToList() ?? new();
        }

        /// <summary>
        /// Writes to the console only if we are running in debug
        /// </summary>
        /// <param name="content">Content to write to console</param>
        /// <returns>bool which indicates successful write to console</returns>
        public static bool DebugConsoleWrite(string content)
        {
            if (ApiEnvironment.IsDebug)
            {
                Console.WriteLine(content);
                return true;
            }

            return false;
        }
        #endregion

        #region Private Methods
        // Writes a LogEntry object to the FfppLogs DB
        private void WriteLogEntry(LogEntry logEntry)
        {
            logEntry.Timestamp = DateTime.UtcNow;
            Add(logEntry);
            SaveChanges();
        }
        #endregion

        // Tells EF that we want to use Sqlite and where the DB will reside
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite($"Data Source={DbPath}");
        }

        // Represents a LogEntry object as it exists in the FfppLogs DB
        public class LogEntry
        {
            [Key] // Public key
            [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // Auto Generate GUID for our PK
            public Guid RowKey { get; set; }
            public DateTime Timestamp { get; set; }
            public string? Severity { get; set; }
            public string? Message { get; set; }
            public string? API { get; set; }
            public bool SentAsAlert { get; set; }
            public string? Tenant { get; set; }
            public string? Username { get; set; }
        }
    }
}

