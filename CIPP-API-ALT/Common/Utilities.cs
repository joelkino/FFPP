using System.Text;
using System.Text.Json;
using CIPP_API_ALT.Data.Logging;

namespace CIPP_API_ALT.Common
{
    public static class Utilities
    {
        /// <summary>
        /// Defines a Heartbeat object we return when the /api/Heartbeat API is polled
        /// </summary>
        public class Heartbeat
        {
            public Heartbeat(DateTime started)
            {
                Started = started;
            }

            public DateTime Started { get; set; }
        }

        /// <summary>
        /// Returns a unicode string consisting of 4098 random bytes (4kb)
        /// </summary>
        /// <param name="length">Length of characters you want returned, defaults to 4098 (4kb)</param>
        /// <returns>4kb string of random unicode chars</returns>
        public static string RandomByteString(int length = 4098)
        {
            byte[] randomFillerBytes = new byte[length];
            new Random().NextBytes(randomFillerBytes);
            return Encoding.Unicode.GetString(randomFillerBytes);
        }

        /// <summary>
        /// Takes raw JSON and a designated type and it converts the JSON into a list of objects of the given type
        /// </summary>
        /// <typeparam name="type">Will parse into a list of objects of this type</typeparam>
        /// <param name="rawJson"></param>
        /// <returns>List of objects defined by given type</returns>
        public static List<type> ParseJson<type>(List<JsonElement> rawJson)
        {
            List<type> objectArrayList = new();

            JsonSerializerOptions options = new()
            {
                PropertyNameCaseInsensitive = true
            };

            foreach (JsonElement je in rawJson)
            {
                objectArrayList.Add(JsonSerializer.Deserialize<type>(je, options));
            }

            return objectArrayList;
        }

        /// <summary>
        /// Submit any JSON object/s to write to file
        /// </summary>
        /// <typeparam name="type">type of JSON object/s e.g. Tenant or List<Tenant></typeparam>
        /// <param name="json">JSON object/s to serialize to file</param>
        /// <param name="filePath">File path</param>
        public static void WriteJsonToFile<type>(object json, string filePath)
        {
            string jsonString = JsonSerializer.Serialize((type)json);
            File.WriteAllText(filePath, jsonString);
        }

        /// <summary>
        /// Return file contents as JSON object of specified type
        /// </summary>
        /// <typeparam name="type">Type of our JSON object to make</typeparam>
        /// <param name="filePath">Path to our file containing JSON</param>
        /// <returns>JSON object of specified type</returns>
        public static type ReadJsonFromFile<type>(string filePath)
        {
            return JsonSerializer.Deserialize<type>(File.ReadAllText(filePath));
        }

        /// <summary>
        /// Converts a supplied CSV file into a List of specified objects
        /// </summary>
        /// <typeparam name="type">type of the object we want returned in the list</typeparam>
        /// <param name="csvFilePath">File path to the CSV file</param>
        /// <param name="skipHeader">First line is a header line (not data) so use true to skip it</param>
        /// <returns>List of objects, each object is a line from the CSV</returns>
        public static List<type> CsvToObjectList<type>(string csvFilePath, bool skipHeader = false)
        {
            List<type> returnData = new();

            foreach (string line in File.ReadAllLines(csvFilePath))
            {
                // Skip first row (header row)
                if (skipHeader)
                {
                    skipHeader = false;
                    continue;
                }

                returnData.Add((type)Activator.CreateInstance(typeof(type), line.Split(",")));
            }

            return returnData;
        }

        /// <summary>
        /// Deletes cache files used by CIPP API ALT
        /// </summary>
        /// <returns>Boolean indicating sucess or failure</returns>
        public static async Task<bool> RemoveCippCache()
        {
            try
            {
                // Delete tenants.cache.json
                File.Delete(ApiEnvironment.CachedTenantsFile);
            }
            catch (Exception ex)
            {
                using (CippLogs logDb = new())
                {
                    await logDb.LogRequest(string.Format("Exception purging CIPP Cache: {0}, Inner Exception: {1}.",
                        ex.Message, ex.InnerException.Message ?? string.Empty), string.Empty, "Error", "None", "RemoveCippCache");
                }
                return false;
            }

            return true;
        }

        /// <summary>
        /// Converts a base64url string into a byte array
        /// </summary>
        /// <param name="arg">string to convert to bytes</param>
        /// <returns>byte[] containing decoded bytes</returns>
        /// <exception cref="Exception">Illegal base64url string</exception>
        static byte[] Base64UrlDecode(string arg)
        {
            string s = arg;
            s = s.Replace('-', '+'); // 62nd char of encoding
            s = s.Replace('_', '/'); // 63rd char of encoding
            switch (s.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 2: s += "=="; break; // Two pad chars
                case 3: s += "="; break; // One pad char
                default:
                    DebugConsoleWrite(string.Format("Illegal base64url string: {0}", arg));
                    throw new Exception(string.Format("Illegal base64url string: {0}", arg));
            }
            return Convert.FromBase64String(s); // Standard base64 decoder
        }


        /// <summary>
        /// Used to describe a JWT v1 Token
        /// </summary>
        public struct TokenDetails
        {
            public TokenDetails(string appId = "", string appName = "", string audience = "", string authMethods = "", string iPAddress = "", string name = "", string scope = "", string tenantId = "", string userPrincipleName = "")
            {
                AppId = appId;
                AppName = appName;
                Audience = audience;
                AuthMethods = authMethods;
                IpAddress = iPAddress;
                Name = name;
                Scope = scope.Split(' ');
                TenantId = tenantId;
                UserPrincipalName = userPrincipleName;
            }

            public string AppId { get; }
            public string AppName { get; }
            public string Audience { get; }
            public string AuthMethods { get; }
            public string IpAddress { get; }
            public string Name { get; }
            public string[] Scope { get; }
            public string TenantId { get; }
            public string UserPrincipalName { get; }
        }

        /// <summary>
        /// Converts a JWT v1 token into a JSON object
        /// </summary>
        /// <param name="token">Token to decode</param>
        /// <returns>JSON object representing the token</returns>
        public static async Task<TokenDetails> ReadJwtv1AccessDetails(string token)
        {


            if (!token.Contains('.') || !token.StartsWith("eyJ"))
            {
                return new TokenDetails();
            }

            byte[] tokenPayload = Base64UrlDecode(token.Split('.')[1]);
            string appName = string.Empty;
            string upn = string.Empty;

            JsonElement jsonToken = (await JsonDocument.ParseAsync(new MemoryStream(tokenPayload))).RootElement;

            if (jsonToken.TryGetProperty("app_displayname", out JsonElement appNameJson))
            {
                appName = appNameJson.GetString();
            }

            if (jsonToken.TryGetProperty("upn", out JsonElement upnJson))
            {
                upn = upnJson.GetString();
            }
            else if(jsonToken.TryGetProperty("unique_name", out upnJson))
            {
                upn = upnJson.GetString();   
            }

            return new(jsonToken.GetProperty("appid").ToString(), appName,
                jsonToken.GetProperty("aud").ToString(), jsonToken.GetProperty("amr").ToString(), jsonToken.GetProperty("ipaddr").ToString(),
                jsonToken.GetProperty("name").ToString(), jsonToken.GetProperty("scp").ToString(), jsonToken.GetProperty("tid").ToString(), upn);
        }

        /// <summary>
        /// Writes to the console only if we are running in debug
        /// </summary>
        /// <param name="content">Content to write to console</param>
        /// <returns>bool which indicates successful write to console</returns>
        public static bool DebugConsoleWrite(string content)
        {
            if(ApiEnvironment.IsDebug)
            {
                Console.WriteLine(content);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Build data directories if they don't exist
        /// </summary>
        public static void DataDirectoryBuild()
        {
            Directory.CreateDirectory(ApiEnvironment.CacheDir);
            Directory.CreateDirectory(ApiEnvironment.DatabaseDir);
        }
    }
}

