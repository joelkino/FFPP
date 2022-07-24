/// Created by Ian Harris (@knightian) - White Knight IT - https://whiteknightit.com.au
/// 2022-07-05
/// Licensed under the MIT License
/// didactic-barnacle is the random name chosen for this project by GitHub

using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using Microsoft.OpenApi.Models;
using CIPP_API_ALT;
using CIPP_API_ALT.Data.Logging;
using CIPP_API_ALT.Data;
using CIPP_API_ALT.Common;
using CIPP_API_ALT.Api.v10.Tenants;
using CIPP_API_ALT.Api.v10.Licenses;
using CIPP_API_ALT.Api.v10.Users;
using CIPP_API_ALT.Api.v10.Dashboards;
using ApiCurrent = CIPP_API_ALT.Api;
using ApiV10 = CIPP_API_ALT.Api.v10;
using ApiDev = CIPP_API_ALT.Api.v11;
using Asp.Versioning;
using Asp.Versioning.Builder;

// Build Data/Cache directories if they don't exist
ApiEnvironment.DataDirectoryBuild();

var builder = WebApplication.CreateBuilder(args);

ApiEnvironment.UseHttpsRedirect = builder.Configuration.GetValue<bool>("ApiSettings:HttpsRedirect");
ApiEnvironment.ShowDevEnvEndpoints = builder.Configuration.GetValue<bool>("ApiSettings:ShowDevEndpoints");
ApiEnvironment.CippDomain = builder.Configuration.GetValue<string>("AzureAd:Domain")
    .Replace("http://",string.Empty).Replace("https://", string.Empty).Replace("api://", string.Empty).TrimEnd('/'); // Protection against some novices entering scheme as well as domain, or trailing /
ApiEnvironment.ShowSwaggerUi = builder.Configuration.GetValue<bool>("ApiSettings:ShowSwaggerUi");

// Expose development environment API endpoints
if (ApiEnvironment.ShowDevEnvEndpoints)
{
    ApiEnvironment.ApiRouteVersions.Add(double.Parse(ApiEnvironment.ApiDev.ToString()));
}

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration, "AzureAd");
builder.Services.AddAuthentication();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("all", policy => policy.RequireRole(ApiEnvironment.RoleOwner,ApiEnvironment.RoleAdmin,ApiEnvironment.RoleEditor,ApiEnvironment.RoleReader));
    options.AddPolicy("edit", policy => policy.RequireRole(ApiEnvironment.RoleOwner, ApiEnvironment.RoleAdmin, ApiEnvironment.RoleEditor));
    options.AddPolicy("admin", policy => policy.RequireRole(ApiEnvironment.RoleOwner, ApiEnvironment.RoleAdmin));
    options.AddPolicy("owner", policy => policy.RequireRole(ApiEnvironment.RoleOwner));
});
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
                AuthorizationUrl = new Uri(builder.Configuration["AzureAd:AuthorizationUrl"]),
                TokenUrl = new Uri(builder.Configuration["AzureAd:TokenUrl"]),
                Scopes = new Dictionary<string, string>
                {
                    { builder.Configuration["AzureAd:ApiScope"], builder.Configuration["AzureAd:Scopes"]}
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
            new[] { builder.Configuration["AzureAd:ApiScope"] }
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

// /api path which uses the latest devenv API specification (will only be accessible if ShowDevEnvEndpoints = true)
ApiDev.Routes.InitRoutes(ref app);

// /api path which always uses the latest API specification
ApiCurrent.Routes.InitRoutes(ref app);

// /1.0 path using API specification v1.0
ApiV10.Routes.InitRoutes(ref app);

app.UseSwagger();

if (ApiEnvironment.ShowSwaggerUi)
{
    app.UseSwaggerUI(customSwagger =>
    {

        foreach (var desc in app.DescribeApiVersions())
        {
            var url = $"/swagger/{desc.GroupName}/swagger.json";
            var name = desc.GroupName.ToUpperInvariant();
            if(desc.ApiVersion.ToString().Contains(ApiEnvironment.ApiDev.ToString()))
            {
                customSwagger.SwaggerEndpoint(url, $"CIPP-API-ALT DEV {name}");
                continue;
            }
            customSwagger.SwaggerEndpoint(url, $"CIPP-API-ALT {name}");
        }

        customSwagger.InjectStylesheet("/Swagger-Customisation/Swagger-Customisation.css");
        customSwagger.OAuthClientId(app.Configuration["AzureAd:OpenIdClientId"]);
        customSwagger.OAuthUsePkce();
        customSwagger.OAuthScopeSeparator(" ");
    });
}

if (ApiEnvironment.IsDebug)
{

    Console.WriteLine(string.Format("######################## CIPP-API-ALT is running in DEBUG context! - app.Environment.IsDevelopment():{0}",
        app.Environment.IsDevelopment().ToString()));

    // In dev env we will get secrets from local environment (use `dotnet user-secrets` tool to safely store local secrets)
    ApiEnvironment.Secrets.ApplicationId = app.Configuration["ApplicationId"];
    ApiEnvironment.Secrets.ApplicationSecret = app.Configuration["ApplicationSecret"];
    ApiEnvironment.Secrets.TenantId = app.Configuration["TenantId"];
    ApiEnvironment.Secrets.RefreshToken = app.Configuration["RefreshToken"];
    ApiEnvironment.Secrets.ExchangeRefreshToken = app.Configuration["ExchangeRefreshToken"];

    // More information on exception page for dev/debug
    app.UseDeveloperExceptionPage();

}
else
{
    // Todo production secrets logic i.e. Azure key vault stuff
}

//string pname = License.ConvertSkuName("SPE_E3_RPA1", string.Empty);
//await new CippLogs().LogDb.LogRequest("Test Message", "", "Information", "M365B654613.onmicrosoft.com", "ThisIsATest");
List<Tenant> tenants = await Tenant.GetTenants(string.Empty);
//await RequestHelper.NewTeamsApiGetRequest("https://api.interfaces.records.teams.microsoft.com/Skype.TelephoneNumberMgmt/Tenants/b439f90e-eb4a-40f3-b11a-d793c488b38a/telephone-numbers?locale=en-US", "b439f90e-eb4a-40f3-b11a-d793c488b38a", HttpMethod.Get);
//await RequestHelper.GetClassicApiToken("M365B654613.onmicrosoft.com", "https://outlook.office365.com");
//var code = await RequestHelper.NewDeviceLogin("a0c73c16-a7e3-4564-9a95-2bdf47383716", "https://outlook.office365.com/.default", true, "", "M365B654613.onmicrosoft.com");
//await MsolUser.GetCippMsolUsers("M365B654613.onmicrosoft.com");
//var ebay = await CippDashboard.GetHomeData();
//CippDashboard.CheckVersions("2.9.0");

//var salt = Utilities.Random2WordPhrase(24);

Sam.CreateSAMAuthApp("CIPP-API-ALT Auth", Sam.SamAppType.Api);
Sam.CreateSAMAuthApp("CIPP-API-ALT Swagger UI", Sam.SamAppType.Spa,spaRedirectUri: "https://localhost:7074/swagger/oauth2-redirect.html");

List<Domain> domains = await Domain.GetDomains("",ApiEnvironment.Secrets.TenantId);

var s = "";

app.Run();