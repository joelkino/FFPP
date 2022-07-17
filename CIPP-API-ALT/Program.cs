/// Created by Ian Harris (@knightian) - White Knight IT
/// 2022-07-05
/// Licensed under the MIT License
/// didactic-barnacle is the random name chosen for this project by GitHub

using Microsoft.AspNetCore.Http.Json;
using System.Text.Json;
using CIPP_API_ALT.Data.Logging;
using CIPP_API_ALT.Data;
using CIPP_API_ALT.Common;
using CIPP_API_ALT.Tenants;
using CIPP_API_ALT.Licenses;
using CIPP_API_ALT.Users;
using CIPP_API_ALT.Dashboards;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure JSON options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.IncludeFields = true;
});

var app = builder.Build();
DateTime started = DateTime.UtcNow;

Utilities.DataDirectoryBuild();

if (ApiEnvironment.IsDebug)
{
    Console.WriteLine(String.Format("######################## CIPP-API-ALT is running in DEBUG context! - app.Environment.IsDevelopment():{0}", app.Environment.IsDevelopment().ToString()));

    //In dev env we will get secrets from local environment (use `dotnet user-secrets` tool to safely store local secrets)
    ApiEnvironment.Secrets.ApplicationId = builder.Configuration["ApplicationId"];
    ApiEnvironment.Secrets.ApplicationSecret = builder.Configuration["ApplicationSecret"];
    ApiEnvironment.Secrets.TenantId = builder.Configuration["TenantId"];
    ApiEnvironment.Secrets.RefreshToken = builder.Configuration["RefreshToken"];
    ApiEnvironment.Secrets.ExchangeRefreshToken = builder.Configuration["ExchangeRefreshToken"];

    // Allows us to serve files from wwwroot to customise swagger etc.
    app.UseStaticFiles();

    app.UseSwagger();
    app.UseSwaggerUI( customSwagger =>
    {
        customSwagger.InjectStylesheet("/Swagger-Customisation/Swagger-Customisation.css");
    });
}
else {
    // Todo Key vault stuff for prod
}

//Scrub secrets in config from RAM now they are stored encrypted in Secrets
builder.Configuration["ApplicationId"] = Utilities.RandomByteString();
builder.Configuration["ApplicationSecret"] = Utilities.RandomByteString();
builder.Configuration["TenantId"] = Utilities.RandomByteString();
builder.Configuration["RefreshToken"] = Utilities.RandomByteString();
builder.Configuration["ExchangeRefreshToken"] = Utilities.RandomByteString();

//string pname = License.ConvertSkuName("SPE_E3_RPA1", string.Empty);
//await CippLogs.LogDb.LogRequest("Test Message", "", "Information", "M365B654613.onmicrosoft.com", "ThisIsATest");
//List<Tenant> tenants = await Tenant.GetTenants();
//await RequestHelper.NewTeamsApiGetRequest("https://api.interfaces.records.teams.microsoft.com/Skype.TelephoneNumberMgmt/Tenants/b439f90e-eb4a-40f3-b11a-d793c488b38a/telephone-numbers?locale=en-US", "b439f90e-eb4a-40f3-b11a-d793c488b38a", HttpMethod.Get);
//await RequestHelper.GetClassicApiToken("M365B654613.onmicrosoft.com", "https://outlook.office365.com");
//var code = await RequestHelper.NewDeviceLogin("a0c73c16-a7e3-4564-9a95-2bdf47383716", "https://outlook.office365.com/.default", true, "", "M365B654613.onmicrosoft.com");
//await User.GetCippMsolUsers("M365B654613.onmicrosoft.com");
//var ebay = await CippDashboard.GetHomeData();

//app.UseHttpsRedirection();

#region API Routes
/// <summary>
/// /api/GetDashboard
/// </summary>
app.MapGet(ApiEnvironment.ApiVersionHeader + "/GetDashboard", async () =>
{
    return await CippDashboard.GetHomeData();
})
.WithName("GetDashboard");

/// <summary>
/// /api/Heartbeat
/// </summary>
app.MapGet(ApiEnvironment.ApiVersionHeader + "/Heartbeat", () =>
{
    return new Utilities.Heartbeat(started);
})
.WithName("Heartbeat");

/// <summary>
/// /api/ListTenants
/// </summary>
app.MapGet(ApiEnvironment.ApiVersionHeader + "/ListTenants", async (HttpRequest request, bool? AllTenantSelector) =>
{
    return await Tenant.GetTenants(allTenantSelector: AllTenantSelector ?? false );
})
.WithName("ListTenants");
#endregion

app.Run();
