using System.Dynamic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FFPP.Data.Logging;

namespace FFPP.Common
{
    public static class Utilities
    {
        public static readonly List<string> WordDictionary = new() { "bruce","bruceson","john","johnson","big","small","stinky",
            "outrageous","valuable","pineapples","jack","jackson","peter","file","juices","spruce","cactus","blunt","sharp","affluent",
            "camel","toe","elbow","knee","old","sponzengeiger","wallace","william","jane","doe","moe","didactic","barnacle","sponge",
            "bob","qwerty","nelson","full","extreme","upstart","fbi","nsa","noah","ark","lancelot","potty","mouth","underpants","spiky",
            "prickly","plaintiff","outlaw","bicycle","dopamine","pepper","trees","space","bananas","monkey","potatoes","crispy","pork",
            "chop","bitcoin","boat","bone","floating","masked","behold","judith","ian","marcus","phoenix","wind","gasses","fluffy","pony",
            "ewe","eyes","star","pole","dance","umpire","rebel","jones","noise","monero","cypher","aes256","aes128","sha256","sha512","xor",
            "hmacsha256","hmacsha512","parrot","cat","dog","squirrel","skunk","farts","width","mountains","lumps","bumps","goose","high",
            "tall","ball","cool","hot","tomato","chips","minimum","maximum","maximus","cease","strange","ugly","derelict","driven","mongoose",
            "hold","grab","foolish","melons","pickles","lemons","sour","grapes","goat","milk","got","plums","spicy","rotating","ending","world",
            "covid","vaccine","pass","fail","stop","fall","in","out","on","beginner","lessons","lapras","snorlax","round","square","cheeses",
            "cheesy","golden","brown","lamb","killer","staged","frightening","laughter","desired","follicles","ultra","uber","plants","paris",
            "bonjour","not","participating","precipitation","carrying","hooves","horse","rhubarb","apples","chilli","burn","heat","cold","pasta",
            "jolting","crabs","rabbit","kangaroo","deal","screaming","zinger","alpha","bravo","charlie","delta","echo","foxtrot","golf","hotel",
            "india","juliet","kilo","lima","mike","november","oscar","papa","quebec","romeo","sierra","tango","uniform","victor","whiskey","xray",
            "zulu","aircraft","bouncing","bobcat","bonkers","nanny","popping","weasel","rolling","atomic","pure","fine","diesel","fishing","puppet",
            "unicorn","epsilon","gamma","beta","thor","tennis","rally","cry","happy","suspicious","panda","bear","smile","frown","skirt","jellyfish",
            "law","tax","criminal","escapade","popcorn","dogma","scared","lifeless","limitless","potential","voltage","amperage","amped","zapped",
            "devil","salamander","frog","carrots","onions","dude","aubergine","appendage","cloudy","scaled","measured","response","excited","flustered",
            "peacock","bin","garbage","trash","taco","beans","burger","alien","illegal","fragrant","floral","food","popsicle","ajar","test","sensual",
            "schooled","varnish","lazy","starfish","belly","ring","of","fire","ice","yacht","russian","spider","web","fierce","furious","fast","factual",
            "fred","nerf","fern","leaf","good","bad","noodles","boy","girl","sleep","thin","major","minor","private","public","nuisance","coffee","fetish"
        };

        /// <summary>
        /// Returns a unicode string consisting of 4098 crypto random bytes (4kb), this is cryptosafe random
        /// </summary>
        /// <param name="length">Length of characters you want returned, defaults to 4098 (4kb)</param>
        /// <returns>4kb string of random unicode chars</returns>
        public static string RandomByteString(int length = 4098)
        {
            byte[] randomFillerBytes = RandomNumberGenerator.GetBytes(length);
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
                PropertyNameCaseInsensitive = true,
                MaxDepth = 64
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

                returnData.Add((type)Activator.CreateInstance(typeof(type), line.Split(',')));
            }

            return returnData;
        }

        /// <summary>
        /// Evaluates JSON boolean and treats null as false
        /// </summary>
        /// <param name="property">JSON boolean to check</param>
        /// <returns>true/false</returns>
        public static bool NullIsFalse(JsonElement property)
        {
            try
            {
                return property.GetBoolean();
            }
            catch
            {

            }

            return false;
        }

        /// <summary>
        /// Deletes cache and pre-fetch files used by FFPP API
        /// </summary>
        /// <returns>Boolean indicating sucess or failure</returns>
        public static async Task<bool> RemoveFfppCache()
        {
            try
            {
                // Delete CacheDir and any subdirs/files then re-create cache dir
                Directory.Delete(ApiEnvironment.CacheDir, true);
                ApiEnvironment.CacheDirectoriesBuild();

            }
            catch (Exception ex)
            {
                using (FfppLogs logDb = new())
                {
                    await logDb.LogRequest(string.Format("Exception purging FFPP API Cache: {0}, Inner Exception: {1}.",
                        ex.Message, ex.InnerException.Message ?? string.Empty), string.Empty, "Error", "None", "RemoveFfppCache");
                }
                return false;
            }

            return true;
        }

        /// <summary>
        /// Decodes a base64url string into a byte array
        /// </summary>
        /// <param name="arg">string to convert to bytes</param>
        /// <returns>byte[] containing decoded bytes</returns>
        /// <exception cref="Exception">Illegal base64url string</exception>
        public static byte[] Base64UrlDecode(string arg)
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
                    FfppLogs.DebugConsoleWrite(string.Format("Illegal base64url string: {0}", arg));
                    throw new Exception(string.Format("Illegal base64url string: {0}", arg));
            }
            return Convert.FromBase64String(s); // Standard base64 decoder
        }

        /// <summary>
        /// Encodes a byte array into a Base64 encoded string
        /// </summary>
        /// <param name="bytes">bytes to encode</param>
        /// <returns>Base64 encoded string</returns>
        public static string Base64Encode(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Used to describe an ApiRandom object used to coinstruct cryptorandom things
        /// used within the API
        /// </summary>
        public class ApiRandom
        {
            private readonly string _phrase;
            private readonly string _hashedPhrase;
            private readonly string _salt;
            private readonly long _iterations;
            private readonly bool _ignoreCryptoSafe;

            /// <summary>
            /// Creates an ApiRandom object that can be used to create a mnemonic phrase with accompanying hashed bytes
            /// which are cryptographically safe as entropy (assuming at least 24 words in the phrase
            /// </summary>
            /// <param name="phrase">The phrase we will be </param>
            /// <param name="salt"></param>
            /// <param name="iterations"></param>
            public ApiRandom(string phrase, string salt = "mmmsalty888", long iterations = 231010, bool ignoreCryptoSafe = false)
            {
                _ignoreCryptoSafe = ignoreCryptoSafe;
                _iterations = iterations;
                _phrase = phrase;
                _salt = salt;
                HMACSHA512 hasher = new(Encoding.Unicode.GetBytes(_phrase + _salt));
                byte[] hashedPhraseBytes = hasher.ComputeHash(Encoding.Unicode.GetBytes(_phrase));

                for (long i = 0; i < _iterations; i++)
                {
                    hashedPhraseBytes = hasher.ComputeHash(hashedPhraseBytes);
                }

                _hashedPhrase = Convert.ToHexString(hashedPhraseBytes);
            }

            private bool CheckCryptoSafe()
            {
                if ((WordDictionary.Count > 299 && ((_phrase.Split('-').Length > 15) || (_phrase.Split('-').Length < 12 && _phrase.Length > 191)) && _iterations > 100000) || _ignoreCryptoSafe)
                {
                    return true;
                }

                return false;
            }

            public string Phrase { get { if (!CheckCryptoSafe()) { throw new("This ApiRandom is not Cryptosafe and we are not instructed to ignore Cryptosafety"); } return _phrase; } }
            public string HashedPhrase { get { if (!CheckCryptoSafe()) { throw new("This ApiRandom is not Cryptosafe and we are not instructed to ignore Cryptosafety"); } return _hashedPhrase; } }
            public byte[] HashedPhraseBytes { get { if (!CheckCryptoSafe()) { throw new("This ApiRandom is not Cryptosafe and we are not instructed to ignore Cryptosafety"); } return Convert.FromHexString(_hashedPhrase); } }
            public string Salt { get { if (!CheckCryptoSafe()) { throw new("This ApiRandom is not Cryptosafe and we are not instructed to ignore Cryptosafety"); } return _salt; } }
            public Guid HashedPhraseBytesAsGuid { get { if (!CheckCryptoSafe()) { throw new("This ApiRandom is not Cryptosafe and we are not instructed to ignore Cryptosafety"); } return new Guid(MD5.HashData(Convert.FromHexString(_hashedPhrase))); } }
            public long Iterations { get => _iterations; }
        }

        /// <summary>
        /// Generates x number of 2 random word phrases from WordDictionary in format [-{0}-{1}], is cryptorandom
        /// so can be used for entropy but need a lot of 2 word phrases to create sufficiently strong entropy.
        /// </summary>
        /// <param name="numberOfPhrases">Number of 2 word phrases to generate in the single line delimited by '-'</param>
        /// <param name="salt">Optional salt to be used during sha512 hashing operation</param>
        /// <returns>List where [0] contains word phrase, [1] contains hex encoded 100,000 pass sha512</returns>
        public static ApiRandom Random2WordPhrase(int numberOfPhrases = 1, string salt= "mmmsalty888", long iterations = 231010, bool ignoreCryptoSafe = false)
        {
            int i = 0;
            string phrase = string.Empty;

            do
            {   // We can't return nothing
                if (numberOfPhrases < 1)
                {
                    numberOfPhrases = 1;
                }

                static string RandomWord()
                {
                    return WordDictionary[RandomNumberGenerator.GetInt32(0, WordDictionary.Count)];
                }

                phrase += string.Format("-{0}-{1}", RandomWord(), RandomWord());

                i++;

            } while (i < numberOfPhrases);

            return new(phrase.Remove(0,1), salt, iterations, ignoreCryptoSafe);
        }

        public static async Task<string> UsernameParse(HttpContext context)
        {
            try
            {
                return context.User.Claims.First(x => x.Type.ToLower().Equals("preferred_username")).Value;
            }
            catch
            {
                return "Illegal Alien";
            }
        }
    }
}

