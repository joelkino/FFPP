using System.Net.Http.Headers;
using System.Web;
using System.Text.Json;
using CIPP_API_ALT.Tenants;
using CIPP_API_ALT.Data.Logging;

namespace CIPP_API_ALT.Common
{
	/// <summary>
	/// Static class used as the basis of all outgoing HTTP requests
	/// </summary>
	public static class RequestHelper
	{
		private static readonly HttpClient _httpClient = new();

		#region Public Methods
		/// <summary>
		/// Converts a known exception message into something more relatable to the user
		/// </summary>
		/// <param name="message">Exception message to make friendly</param>
		/// <returns>Friendly exception message</returns>
		public static string GetNormalizedError(string message)
		{
			return message switch
			{
				string a when a.Contains("Request not applicable to target tenant.") => "Required license not available for this tenant",
				string b when b.Contains("Neither tenant is B2C or tenant doesn't have premium license") => "This feature requires a P1 license or higher",
				string c when c.Contains("Response status code does not indicate success: 400 (Bad Request).") => "Error 400 occured. There is an issue with the token configuration for this tenant. Please perform an access check",
				string d when d.Contains("Microsoft.Skype.Sync.Pstn.Tnm.Common.Http.HttpResponseException") => "Could not connect to Teams Admin center - Tenant might be missing a Teams license",
				string e when e.Contains("Provide valid credential.") => "Error 400: There is an issue with your Exchange Token configuration. Please perform an access check for this tenant",
				_ => message,
			};
		}

		/// <summary>
		/// Obtain an access_token from refresh_token to query the Graph API
		/// </summary>
		/// <param name="tenantId">tenantId that you wish to query graph for information about</param>
		/// <param name="asApp">The query will be running as an app or delegated user?</param>
		/// <param name="appId">ID of the app that will be executing the query</param>
		/// <param name="refreshToken">refresh_token used to request access_token</param>
		/// <param name="scope">Scope of our query</param>
		/// <param name="returnRefresh">Return the refresh_token as well as the access_token?</param>
		/// <returns>A dictionary containing either the access_token or all values returned by the query for use in query headers</returns>
		public static async Task<Dictionary<string, string>> GetGraphToken(string tenantId, bool asApp, string appId = "", string refreshToken = "", string scope = "https://graph.microsoft.com//.default", bool returnRefresh = false)
		{
			string? authBody;

			if (asApp)
			{
				authBody = string.Format("client_id={0}&client_secret={1}&scope={2}&grant_type=client_credentials", HttpUtility.UrlEncode(ApiEnvironment.Secrets.ApplicationId), HttpUtility.UrlEncode(ApiEnvironment.Secrets.ApplicationSecret), HttpUtility.UrlEncode(scope));
			}
			else
			{
				authBody = string.Format("client_id={0}&client_secret={1}&scope={2}&refresh_token={3}&grant_type=refresh_token", ApiEnvironment.Secrets.ApplicationId, ApiEnvironment.Secrets.ApplicationSecret, scope, ApiEnvironment.Secrets.RefreshToken);
			}

			if (!string.IsNullOrEmpty(appId) && !string.IsNullOrEmpty(refreshToken))
			{
				authBody = string.Format("client_id={0}&refresh_token={1}&scope={2}&grant_type=refresh_token", appId, refreshToken, scope);
			}

			if (string.IsNullOrEmpty(tenantId))
			{
				tenantId = ApiEnvironment.Secrets.TenantId;
			}

			using HttpRequestMessage requestMessage = new(HttpMethod.Post, string.Format("https://login.microsoftonline.com/{0}/oauth2/v2.0/token", tenantId));
			{
				requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", refreshToken);
				requestMessage.Content = new StringContent(authBody);
				requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

				HttpResponseMessage responseMessage = await SendHttpRequest(requestMessage);

				if (responseMessage.IsSuccessStatusCode)
				{

					string responseRawString = await responseMessage.Content.ReadAsStringAsync();
					string[] headersRaw = responseRawString[1..^1].Split(",");
					Dictionary<string, string> headers = new();

					foreach (string headerPairRaw in headersRaw)
					{
						string[] h = headerPairRaw.Split(":");

						if (h[0].StartsWith('"'))
						{
							// Trim the redundant " char from start and end of string
							h[0] = h[0][1..^1];
						}

						if (h[1].StartsWith('"'))
						{
							h[1] = h[1][1..^1];
						}

						headers.Add(h[0], h[1]);
					}

					if (returnRefresh)
					{
						return headers;
					}

					return new Dictionary<string, string> { ["Authorization"] = headers.GetValueOrDefault("access_token", string.Empty) };
				}

				// Write to log an error that we didn't get HTTP 2XX
				await CippLogs.LogDb.LogRequest(string.Format("Incorrect HTTP status code. Expected 2XX got {0}.", responseMessage.StatusCode.ToString()), string.Empty, "Error", tenantId, "GetGraphToken");
				return new Dictionary<string, string> { [string.Empty] = string.Empty };
			}
		}

		/// <summary>
		/// Sends a HTTP GET request to supplied uri using graph access_token for auth
		/// </summary>
		/// <param name="uri">The url we wish to GET from</param>
		/// <param name="tenantId">The tenant the request relates to</param>
		/// <param name="scope"></param>
		/// <param name="asApp">As application or as delegated user</param>
		/// <param name="noPagination"></param>
		/// <returns>A List containing one or more JSON Elements</returns>
		public static async Task<List<JsonElement>> NewGraphGetRequest(string uri, string tenantId, string scope = "https://graph.microsoft.com//.default", bool asApp = false, bool noPagination = false)
		{
			Dictionary<string, string> headers;

			if (scope.ToLower().Equals("exchangeonline"))
			{
				headers = await GetGraphToken(tenantId, asApp, "a0c73c16-a7e3-4564-9a95-2bdf47383716", ApiEnvironment.Secrets.ExchangeRefreshToken, "https://outlook.office365.com/.default");

			}
			else
			{
				headers = await GetGraphToken(tenantId, asApp, string.Empty, string.Empty, scope);
			}

			Utilities.DebugConsoleWrite(string.Format("Using {0} as url", uri));

			string nextUrl = uri;

			List<JsonElement> data = new();

			if (await GetAuthorisedRequest(tenantId, uri))
			{
				do
				{
					try
					{
						using HttpRequestMessage requestMessage = new(HttpMethod.Get, uri);
						{
							requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", headers.GetValueOrDefault("Authorization", "FAILED-TO-GET-AUTH-TOKEN"));
							requestMessage.Headers.TryAddWithoutValidation("ConsistencyLevel", "eventual");

							foreach (KeyValuePair<string, string> _h in headers)
							{
								if (!_h.Key.ToLower().Equals("authorization"))
								{
									requestMessage.Headers.TryAddWithoutValidation(_h.Key, _h.Value);
								}
							}
							HttpResponseMessage responseMessage = await SendHttpRequest(requestMessage);

							if (responseMessage.IsSuccessStatusCode)
							{
								JsonDocument jsonDoc = await JsonDocument.ParseAsync(new MemoryStream(await responseMessage.Content.ReadAsByteArrayAsync()));
								try
								{
									data.Add(jsonDoc.RootElement.GetProperty("value"));
								}
								catch
								{
									data.Add(jsonDoc.RootElement);
								}

								if (noPagination)
								{
									nextUrl = string.Empty;
								}
								else
								{
									try
									{
										nextUrl = jsonDoc.RootElement.EnumerateObject().FirstOrDefault(p => p.Name == "@odata.nextLink").ToString();
									}
									catch
									{
										nextUrl = string.Empty;
									}

								}
							}
							else
							{
								// Write to log an error that we didn't get HTTP 2XX
								await CippLogs.LogDb.LogRequest(string.Format("Incorrect HTTP status code. Expected 2XX got {0}. Uri: {1}", responseMessage.StatusCode.ToString(), uri), string.Empty, "Error", tenantId, "NewGraphGetRequest");
							}

						}
					}
					catch (Exception exception)
					{
						Console.WriteLine(string.Format("Exception in NewGraphGetRequest: {0}, Inner Exception: {1}", exception.Message, exception.InnerException.Message));
						throw;
					}
				}
				while (!string.IsNullOrEmpty(nextUrl));

			}
			else
			{
				await CippLogs.LogDb.LogRequest("Not allowed. You cannot manage your own tenant or tenants not under your scope", string.Empty, "Info", tenantId, "NewGraphGetRequest");
			}

			return data;
		}

		/// <summary>
		/// Sends a HTTP POST or other method supplied request to supplied uri using graph access_token for auth
		/// </summary>
		/// <param name="uri">The url we wish to POST to</param>
		/// <param name="tenantId">The tenant relevant to the operation</param>
		/// <param name="body">The object we wish to send as JSON payload in body</param>
		/// <param name="type">HTTP Method POST/PUT etc.</param>
		/// <param name="scope"></param>
		/// <param name="asApp">As application or delegated user</param>
		/// <returns>The content in any response as JSON</returns>
		public static async Task<JsonElement> NewGraphPostRequest(string uri, string tenantId, object body, HttpMethod type, string scope, bool asApp)
		{
			Dictionary<string, string> headers = await GetGraphToken(tenantId, asApp, string.Empty, string.Empty, scope);

			Utilities.DebugConsoleWrite(string.Format("Using {0} as url", uri));

			if (await GetAuthorisedRequest(tenantId, uri))
			{
				try
				{
					using HttpRequestMessage requestMessage = new(type, uri);
					{
						requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", headers.GetValueOrDefault("Authorization", "FAILED-TO-GET-AUTH-TOKEN"));
						requestMessage.Headers.TryAddWithoutValidation("ConsistencyLevel", "eventual");

						foreach (KeyValuePair<string, string> _h in headers)
						{
							if (!_h.Key.ToLower().Equals("authorization"))
							{
								requestMessage.Headers.TryAddWithoutValidation(_h.Key, _h.Value);
							}
						}

						requestMessage.Content = new StringContent(JsonSerializer.Serialize(body));
						requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

						HttpResponseMessage responseMessage = await SendHttpRequest(requestMessage);

						if (responseMessage.IsSuccessStatusCode)
						{
							JsonDocument jsonDoc = await JsonDocument.ParseAsync(new MemoryStream(await responseMessage.Content.ReadAsByteArrayAsync()));
							return jsonDoc.RootElement;
						}

						// Write to log an error that we didn't get HTTP 2XX
						await CippLogs.LogDb.LogRequest(string.Format("Incorrect HTTP status code. Expected 2XX got {0}. Uri: {1}", responseMessage.StatusCode.ToString(), uri), string.Empty, "Error", tenantId, "NewGraphPostRequest");
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(string.Format("Excpetion in NewGraphPostRequest: {0}, Inner Exception: {1}", ex.Message, ex.InnerException.Message));
					throw;
				}

			}
			else
			{
				await CippLogs.LogDb.LogRequest("Not allowed. You cannot manage your own tenant or tenants not under your scope", string.Empty, "Info", tenantId, "NewGraphPostRequest");
			}

			return new JsonElement();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="tenantId"></param>
		/// <param name="resource"></param>
		/// <returns></returns>
		public static async Task<JsonElement> GetClassicApiToken(string tenantId, string resource)
		{
			string uri = string.Format("https://login.microsoftonline.com/{0}/oauth2/token", tenantId);
			string body = string.Format("resource={0}&grant_type=refresh_token&refresh_token={1}", resource, ApiEnvironment.Secrets.ExchangeRefreshToken);

			HttpRequestMessage requestMessage = new(HttpMethod.Post, uri);
			requestMessage.Content = new StringContent(body);
			requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

			HttpResponseMessage responseMessage = await SendHttpRequest(requestMessage);

			if (responseMessage.IsSuccessStatusCode)
			{
				JsonDocument jsonDoc = await JsonDocument.ParseAsync(new MemoryStream(await responseMessage.Content.ReadAsByteArrayAsync()));
				return jsonDoc.RootElement;
			}

			await CippLogs.LogDb.LogRequest(string.Format("Incorrect HTTP status code. Expected 2XX got {0}.", responseMessage.StatusCode.ToString()), string.Empty, "Error", tenantId, "GetClassicApiToken");
			return new();

		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="tenantId"></param>
		/// <param name="httpMethod"></param>
		/// <param name="resource"></param>
		/// <param name="contentType"></param>
		/// <param name="noPagination"></param>
		/// <returns></returns>
		public static async Task<List<JsonElement>> NewTeamsApiGetRequest(string uri, string tenantId, HttpMethod httpMethod, string resource = "48ac35b8-9aa8-4d74-927d-1f4a14a0b239", string contentType = "application/json", bool noPagination = false)
		{
			return await NewClassicApiGetRequest(uri, tenantId, httpMethod, resource, contentType, noPagination, new Dictionary<string, string> { ["x-ms-tnm-applicationid"] = "045268c0-445e-4ac1-9157-d58f67b167d9" });
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="uri"></param>
		/// <param name="tenantId"></param>
		/// <param name="httpMethod"></param>
		/// <param name="resource"></param>
		/// <param name="contentType"></param>
		/// <param name="noPagination"></param>
		/// <returns></returns> headers
		public static async Task<List<JsonElement>> NewClassicApiGetRequest(string uri, string tenantId, HttpMethod httpMethod, string resource = "https://admin.microsoft.com", string contentType = "application/json", bool noPagination = false, Dictionary<string, string>? headers = null)
		{
			string token = (await GetClassicApiToken(tenantId, resource)).GetProperty("access_token").ToString();

			Utilities.DebugConsoleWrite(string.Format("Using {0} as url in classic API GET request", uri));

			string nextUrl = uri;

			List<JsonElement> data = new();

			if (await GetAuthorisedRequest(tenantId, uri))
			{
				do
				{
					try
					{
						using HttpRequestMessage requestMessage = new(httpMethod, uri);
						{
							requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
							requestMessage.Headers.TryAddWithoutValidation("x-ms-client-request-id", Guid.NewGuid().ToString());
							requestMessage.Headers.TryAddWithoutValidation("x-ms-client-session-id", Guid.NewGuid().ToString());
							requestMessage.Headers.TryAddWithoutValidation("x-ms-correlation-id", Guid.NewGuid().ToString());
							requestMessage.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");

							if (headers != null)
							{
								foreach (KeyValuePair<string, string> _h in headers)
								{
									if (!_h.Key.ToLower().Equals("authorization"))
									{
										requestMessage.Headers.TryAddWithoutValidation(_h.Key, _h.Value);
									}
								}
							}

							HttpResponseMessage responseMessage = await SendHttpRequest(requestMessage);

							if (responseMessage.IsSuccessStatusCode)
							{
								JsonDocument jsonDoc = await JsonDocument.ParseAsync(new MemoryStream(await responseMessage.Content.ReadAsByteArrayAsync()));
								data.Add(jsonDoc.RootElement);

								if (noPagination)
								{
									nextUrl = string.Empty;
								}
								else
								{
									try
									{
										nextUrl = jsonDoc.RootElement.EnumerateObject().FirstOrDefault(p => p.Name == "NextLink").ToString();
									}
									catch
									{
										nextUrl = string.Empty;
									}
								}
							}
							else
							{
								// Write to log an error that we didn't get HTTP 2XX
								await CippLogs.LogDb.LogRequest(string.Format("Incorrect HTTP status code. Expected 2XX got {0}. Uri: {1}", responseMessage.StatusCode.ToString(), uri), string.Empty, "Error", tenantId, "NewClassicApiGetRequest");
								nextUrl = string.Empty;
							}
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine(string.Format("Exception in NewClassicAPIGetRequest: {0}, Inner Exception: {1}", ex.Message, ex.InnerException.Message));
						throw;
					}
				}
				while (!string.IsNullOrEmpty(nextUrl));
			}
			else
			{
				await CippLogs.LogDb.LogRequest("Not allowed. You cannot manage your own tenant or tenants not under your scope", string.Empty, "Info", tenantId, "NewClassicApiGetRequest");
			}

			return data;
		}

		/// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="uri"></param>
        /// <param name="httpMethod"></param>
        /// <param name="body"></param>
        /// <param name="resource"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
		public static async Task<JsonElement> NewClassicApiPostRequest(string tenantId, string uri, HttpMethod httpMethod, string body, string resource = "https://admin.microsoft.com", Dictionary<string, string>? headers = null)
		{
			string token = (await GetClassicApiToken(tenantId, resource)).GetProperty("access_token").ToString();

			Utilities.DebugConsoleWrite(string.Format("Using {0} as url in classic API POST request", uri));
			
			JsonElement data = new();

			if (await GetAuthorisedRequest(tenantId, uri))
			{
				try
				{
					using HttpRequestMessage requestMessage = new(httpMethod, uri);
					{
						requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
						requestMessage.Headers.TryAddWithoutValidation("x-ms-client-request-id", Guid.NewGuid().ToString());
						requestMessage.Headers.TryAddWithoutValidation("x-ms-client-session-id", Guid.NewGuid().ToString());
						requestMessage.Headers.TryAddWithoutValidation("x-ms-correlation-id", Guid.NewGuid().ToString());
						requestMessage.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");

						if (headers != null)
						{
							foreach (KeyValuePair<string, string> _h in headers)
							{
								if (!_h.Key.ToLower().Equals("authorization"))
								{
									requestMessage.Headers.TryAddWithoutValidation(_h.Key, _h.Value);
								}
							}
						}

						requestMessage.Content = new StringContent(JsonSerializer.Serialize(body));
						requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

						HttpResponseMessage responseMessage = await SendHttpRequest(requestMessage);

						if (responseMessage.IsSuccessStatusCode)
						{
							JsonDocument jsonDoc = await JsonDocument.ParseAsync(new MemoryStream(await responseMessage.Content.ReadAsByteArrayAsync()));
							data = jsonDoc.RootElement;

						}
						else
						{
							// Write to log an error that we didn't get HTTP 2XX
							await CippLogs.LogDb.LogRequest(string.Format("Incorrect HTTP status code. Expected 2XX got {0}. Uri: {1}", responseMessage.StatusCode.ToString(), uri), string.Empty, "Error", tenantId, "NewClassicApiPostRequest");
						}
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(string.Format("Exception in NewClassicApiPostRequest: {0}, Inner Exception: {1}", ex.Message, ex.InnerException.Message));
					throw;
				}
			}
			else
			{
				await CippLogs.LogDb.LogRequest("Not allowed. You cannot manage your own tenant or tenants not under your scope", string.Empty, "Info", tenantId, "NewClassicApiPostRequest");
			}

			return data;
		}

		/// <summary>
        /// 
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="body"></param>
        /// <returns></returns>
		public static async Task<JsonElement> NewExoRequest(string tenantId, string body)
		{
			string token = (await GetClassicApiToken(tenantId, "https://outlook.office365.com")).GetProperty("access_token").ToString();
			JsonElement returnData = new();

			if (await GetAuthorisedRequest(tenantId))
			{
				string tenant = (await Tenant.GetTenants()).Find(x => x.DefaultDomainName.Equals(tenantId)).CustomerId;
				string onMicrosoft = (await NewGraphGetRequest("https://graph.microsoft.com/beta/domains?$top=999", tenantId)).Find(x => x.GetProperty("isInitial").GetBoolean().Equals(true)).GetProperty("id").ToString();
				string uri = string.Format("https://outlook.office365.com/adminapi/beta/{0}/InvokeCommand", tenant);
				HttpRequestMessage requestMessage = new (HttpMethod.Post, uri);
				requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
				requestMessage.Headers.TryAddWithoutValidation("X-AnchorMailbox", string.Format("UPN:SystemMailbox{bb558c35-97f1-4cb9-8ff7-d53741dc928c}@{0}",tenant));

				try
				{
					requestMessage.Content = new StringContent(JsonSerializer.Serialize(body));
					requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

					HttpResponseMessage responseMessage = await SendHttpRequest(requestMessage);

					if (responseMessage.IsSuccessStatusCode)
					{
						JsonDocument jsonDoc = await JsonDocument.ParseAsync(new MemoryStream(await responseMessage.Content.ReadAsByteArrayAsync()));
						returnData = jsonDoc.RootElement.GetProperty("value");
					}

					// Write to log an error that we didn't get HTTP 2XX
					await CippLogs.LogDb.LogRequest(string.Format("Incorrect HTTP status code. Expected 2XX got {0}. Uri: {1}", responseMessage.StatusCode.ToString(), uri), string.Empty, "Error", tenantId, "NewExoRequest");
				}
				catch (Exception ex)
				{
					Console.WriteLine(string.Format("Exception in NewExoRequest: {0}, Inner Exception: {1}", ex.Message, ex.InnerException.Message));
					throw;
				}
			}
			else
			{
				await CippLogs.LogDb.LogRequest("Not allowed. You cannot manage your own tenant or tenants not under your scope", string.Empty, "Info", tenantId, "NewExoRequest");
			}

			return returnData;
		}

		/// <summary>
        /// 
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="scope"></param>
        /// <param name="firstLogon"></param>
        /// <param name="device_code"></param>
        /// <param name="tenantId"></param>
        /// <returns></returns>
		public static async Task<JsonElement> NewDeviceLogin(string clientId, string scope, bool firstLogon ,string device_code, string tenantId)
		{
			string encodedscope = Uri.EscapeDataString(scope);
			string uri = "https://login.microsoftonline.com/organizations/oauth2/v2.0/token";
			HttpRequestMessage requestMessage;
			HttpResponseMessage responseMessage;

			if (firstLogon)
			{
				if (!string.IsNullOrEmpty(tenantId))
				{
					uri = string.Format("https://login.microsoftonline.com/{0}/oauth2/v2.0/devicecode", tenantId);
				}
				else
				{
					uri = "https://login.microsoftonline.com/organizations/oauth2/v2.0/devicecode";
				}

				requestMessage = new(HttpMethod.Post, uri);
				requestMessage.Content = new StringContent(string.Format("client_id={0}&scope={1}+offline_access+profile+openid", clientId, encodedscope));
				requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

				responseMessage = await SendHttpRequest(requestMessage);

				if (!responseMessage.IsSuccessStatusCode)
                {
					// Write to log an error that we didn't get HTTP 2XX
					await CippLogs.LogDb.LogRequest(string.Format("Incorrect HTTP status code. Expected 2XX got {0}. Uri: {1}", responseMessage.StatusCode.ToString(), uri), string.Empty, "Error", tenantId, "NewDeviceLogin");
				}
			}
			else
			{
				requestMessage = new(HttpMethod.Post, uri);
				requestMessage.Content = new StringContent(string.Format("client_id={0}&scope={1}+offline_access+profile+openid&grant_type=device_code&device_code={2}", clientId, encodedscope, device_code));
				requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

				responseMessage = await SendHttpRequest(requestMessage);

				if (!(await JsonDocument.ParseAsync(new MemoryStream(await responseMessage.Content.ReadAsByteArrayAsync()))).RootElement.TryGetProperty("refresh_token", out JsonElement refreshToken))
				{

					return (await JsonDocument.ParseAsync(new MemoryStream(await responseMessage.Content.ReadAsByteArrayAsync()))).RootElement.GetProperty("error");
				}
			}

			return (await JsonDocument.ParseAsync(new MemoryStream(await responseMessage.Content.ReadAsByteArrayAsync()))).RootElement;
		}

		/// <summary>
        /// Uses the HttpClient attached to this class to send HTTP request
        /// </summary>
        /// <param name="requestMessage">HttpRequestMessage to send</param>
        /// <returns>HttpResponseMessage is returned</returns>
		public static async Task<HttpResponseMessage> SendHttpRequest(HttpRequestMessage requestMessage)
        {
			return await _httpClient.SendAsync(requestMessage);
		}
		#endregion

		#region Private Methods
		// Makes sure we are authorised to access this tenant
		private async static Task<bool> GetAuthorisedRequest(string tenantId, string uri = "")
		{

			if (uri.ToLower().Contains("https://graph.microsoft.com/beta/contracts") || uri.ToLower().Contains("/customers/") ||
				uri.ToLower().Equals("https://graph.microsoft.com/v1.0/me/sendmail") ||
				uri.ToLower().Contains("https://graph.microsoft.com/beta/tenantrelationships/managedtenants"))
			{
				return true;
			}

			List<Tenant> tenants = await Tenant.GetTenants();

			if (tenants != null && tenants.Count > 0)
			{
				// Check if tenantId exists in any of the properties of our allowed tenants
				if (tenants.Find(x => x.DefaultDomainName.ToLower().Equals(tenantId.ToLower())) != null ||
					tenants.Find(x => x.CustomerId.ToLower().Equals(tenantId.ToLower())) != null ||
					tenants.Find(x => x.DisplayName.ToLower().Equals(tenantId.ToLower())) != null)
				{
					return true;
				}
			}

			return false;
		}
        #endregion
    }
}