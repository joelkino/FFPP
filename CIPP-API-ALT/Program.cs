/// Created by Ian Harris (@knightian) - White Knight IT - https://whiteknightit.com.au
/// 2022-07-05
/// Licensed under the MIT License
/// didactic-barnacle is the random name chosen for this project by GitHub

using Microsoft.AspNetCore.Http.Json;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using System.Text.Json;
using CIPP_API_ALT.Common;
using CIPP_API_ALT.Api.v10.Tenants;
using CIPP_API_ALT.Api.v10.Licenses;
using CIPP_API_ALT.Api.v10.Users;
using CIPP_API_ALT.Api.v10.Dashboards;
using ApiCurrent = CIPP_API_ALT.Api;
using ApiV10 = CIPP_API_ALT.Api.v10;
using ApiDev = CIPP_API_ALT.Api.v11;
using Asp.Versioning.Builder;

// Build Data/Cache directories if they don't exist
ApiEnvironment.DataDirectoriesBuild();

var builder = WebApplication.CreateBuilder(args);

// Load individual settings
ApiEnvironment.UseHttpsRedirect = builder.Configuration.GetValue<bool>("ApiSettings:HttpsRedirect");
ApiEnvironment.ShowDevEnvEndpoints = builder.Configuration.GetValue<bool>("ApiSettings:ShowDevEndpoints");
ApiEnvironment.ShowSwaggerUi = builder.Configuration.GetValue<bool>("ApiSettings:ShowSwaggerUi");
ApiEnvironment.RunSwagger = builder.Configuration.GetValue<bool>("ApiSettings:RunSwagger");
ApiEnvironment.CippCompatibilityMode = builder.Configuration.GetValue<bool>("ApiSettings:CippUiCampatibility");

// Expose development environment API endpoints if set in settiungs to do so
if (ApiEnvironment.ShowDevEnvEndpoints)
{
    ApiEnvironment.ApiRouteVersions.Add(double.Parse(ApiEnvironment.ApiDev.ToString()));
}

if (ApiEnvironment.IsDebug)
{
    Console.WriteLine(string.Format("######################## CIPP-API-ALT is running in DEBUG context! - app.Environment.IsDevelopment():{0}",
        builder.Environment.IsDevelopment().ToString()));

    // In dev env we will get secrets from local environment (use `dotnet user-secrets` tool to safely store local secrets)
    ApiEnvironment.Secrets.ApplicationId = builder.Configuration["ApplicationId"];
    ApiEnvironment.Secrets.ApplicationSecret = builder.Configuration["ApplicationSecret"];
    ApiEnvironment.Secrets.TenantId = builder.Configuration["TenantId"];
    ApiEnvironment.Secrets.RefreshToken = builder.Configuration["RefreshToken"];
    ApiEnvironment.Secrets.ExchangeRefreshToken = builder.Configuration["ExchangeRefreshToken"];

    builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration, "ZeroConf:AzureAd", subscribeToJwtBearerMiddlewareDiagnosticsEvents: true);

}
else
{
    builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration, "ZeroConf:AzureAd");

    // Todo production secrets logic i.e. Azure key vault stuff
}

// We have yet to complete the Zero Configuration setup
if (!ApiZeroConfiguration.ZeroConfExists())
{
    await ApiZeroConfiguration.Setup(ApiEnvironment.Secrets.TenantId);
}

// Read ZeroConf and load it into app config
ApiZeroConfiguration zeroConf = Utilities.ReadJsonFromFile<ApiZeroConfiguration>(ApiEnvironment.ZeroConfPath);
builder.Configuration["ZeroConf:AzureAd:TenantId"] = zeroConf.TenantId;
builder.Configuration["ZeroConf:AzureAd:ClientId"] = zeroConf.ClientId;
builder.Configuration["ZeroConf:AzureAd:Domain"] = zeroConf.Domain;
builder.Configuration["ZeroConf:AzureAd:Scopes"] = zeroConf.Scopes;
builder.Configuration["ZeroConf:AzureAd:AuthorizationUrl"] = zeroConf.AuthorizationUrl;
builder.Configuration["ZeroConf:AzureAd:TokenUrl"] = zeroConf.TokenUrl;
builder.Configuration["ZeroConf:AzureAd:ApiScope"] = zeroConf.ApiScope;
builder.Configuration["ZeroConf:AzureAd:OpenIdClientId"] = zeroConf.OpenIdClientId;
builder.Configuration["ZeroConf:AzureAd:Instance"] = zeroConf.Instance;
builder.Configuration["ZeroConf:AzureAd:CallbackPath"] = zeroConf.CallbackPath;

// Add auth services
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
string corsUri = builder.Configuration["ApiSettings:WebUiUrl"];
builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
{
    // This allows for our Web UI which may be at a totally different domain and/or port to comminucate with the API
    builder.WithOrigins(corsUri).AllowAnyMethod().AllowAnyHeader().AllowCredentials();
}));

// Add API versioning capabilities
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.DefaultApiVersion = ApiEnvironment.ApiCurrent;
}).AddApiExplorer(options =>
{
    options.SubstitutionFormat = "VV";
    options.GroupNameFormat = "'v'VV";
    options.SubstituteApiVersionInUrl = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
});

// Prep Swagger and specify the auth settings for it to use a SAM on Azure AD
builder.Services.AddSwaggerGen(customSwagger => {

    foreach (double version in ApiEnvironment.ApiRouteVersions)
    {
        if (version.ToString("f1").Contains(ApiEnvironment.ApiDev.ToString()))
        {
            customSwagger.SwaggerDoc(string.Format("v{0}", version.ToString("f1")), new() { Title = "CIPP-API-ALT DEV", Version = string.Format("v{0}", version.ToString("f1")) });
            continue;
        }
        customSwagger.SwaggerDoc(string.Format("v{0}", version.ToString("f1")), new() { Title = "CIPP-API-ALT", Version = string.Format("v{0}", version.ToString("f1")) });

    }

    customSwagger.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "OAuth2.0 Auth Code with PKCE",
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri(builder.Configuration["ZeroConf:AzureAd:AuthorizationUrl"]),
                TokenUrl = new Uri(builder.Configuration["ZeroConf:AzureAd:TokenUrl"]),
                Scopes = new Dictionary<string, string>
                {
                    { builder.Configuration["ZeroConf:AzureAd:ApiScope"], builder.Configuration["ZeroConf:AzureAd:Scopes"]}
                }
            }
        }
    });
    customSwagger.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
            },
            new[] { builder.Configuration["ZeroConf:AzureAd:ApiScope"] }
        }
    });
});

// Configure JSON options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.AllowTrailingCommas = false;

    // Official CIPP-API has absolutely no standards for serializing JSON, we need this to match it, and it hurts my soul immensly.
    options.SerializerOptions.PropertyNamingPolicy = null;
});

var app = builder.Build();

app.UseCors("corsapp");

app.UseAuthentication();
app.UseAuthorization();

if (ApiEnvironment.UseHttpsRedirect)
{
    // Redirect HTTP to HTTPS, seems to use 307 temporary redirect
    app.UseHttpsRedirection();
}

// Allows us to serve files from wwwroot to customise swagger etc.
app.UseStaticFiles();

ApiVersionSetBuilder apiVersionSetBuilder = app.NewApiVersionSet();

foreach (double version in ApiEnvironment.ApiRouteVersions)
{
    apiVersionSetBuilder.HasApiVersion(new(version));
}

ApiEnvironment.ApiVersionSet = apiVersionSetBuilder.ReportApiVersions().Build();

// /x.x (ApiEnvironment.ApiDev) path which uses the latest devenv API specification (will only be accessible if ShowDevEnvEndpoints = true)
ApiDev.Routes.InitRoutes(ref app);

// /api path which always uses the latest API specification
ApiCurrent.Routes.InitRoutes(ref app);

// /v1.0 path using API specification v1.0
ApiV10.Routes.InitRoutes(ref app);

if (ApiEnvironment.RunSwagger)
{
    app.UseSwagger();

    // But we don't always show the UI for swagger
    if (ApiEnvironment.ShowSwaggerUi)
    {
        app.UseSwaggerUI(customSwagger =>
        {

            foreach (var desc in app.DescribeApiVersions())
            {
                var url = $"/swagger/{desc.GroupName}/swagger.json";
                var name = desc.GroupName.ToUpperInvariant();
                if (desc.ApiVersion.ToString().Contains(ApiEnvironment.ApiDev.ToString()))
                {
                    customSwagger.SwaggerEndpoint(url, $"CIPP-API-ALT DEV {name}");
                    continue;
                }
                customSwagger.SwaggerEndpoint(url, $"CIPP-API-ALT {name}");
            }

            customSwagger.InjectStylesheet("/Swagger-Customisation/Swagger-Customisation.css");
            customSwagger.OAuthClientId(app.Configuration["ZeroConf:AzureAd:OpenIdClientId"]);
            customSwagger.OAuthUsePkce();
            customSwagger.OAuthScopeSeparator(" ");
        });
    }
}

//string pname = License.ConvertSkuName("SPE_E3_RPA1", string.Empty);
//await new CippLogs().LogDb.LogRequest("Test Message", "", "Information", "M365B654613.onmicrosoft.com", "ThisIsATest");
//List<Tenant> tenants = await Tenant.GetTenants(string.Empty);
//await RequestHelper.NewTeamsApiGetRequest("https://api.interfaces.records.teams.microsoft.com/Skype.TelephoneNumberMgmt/Tenants/b439f90e-eb4a-40f3-b11a-d793c488b38a/telephone-numbers?locale=en-US", "b439f90e-eb4a-40f3-b11a-d793c488b38a", HttpMethod.Get);
//await RequestHelper.GetClassicApiToken("M365B654613.onmicrosoft.com", "https://outlook.office365.com");
//var code = await RequestHelper.NewDeviceLogin("a0c73c16-a7e3-4564-9a95-2bdf47383716", "https://outlook.office365.com/.default", true, "", "M365B654613.onmicrosoft.com");
//await MsolUser.GetCippMsolUsers("M365B654613.onmicrosoft.com");
//var ebay = await CippDashboard.GetHomeData();
//CippDashboard.CheckVersions("2.9.0");
//var salt = Utilities.Random2WordPhrase(24);
//Sam.CreateSAMAuthApp("CIPP-API-ALT Auth", Sam.SamAppType.Api);
//Sam.CreateSAMAuthApp("CIPP-API-ALT Swagger UI", Sam.SamAppType.Spa,spaRedirectUri: "https://localhost:7074/swagger/oauth2-redirect.html");

List<Domain> domains = await Domain.GetDomains("", ApiEnvironment.Secrets.TenantId);

var s = "";

app.Run();