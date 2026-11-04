using Microsoft.Extensions.Options;

using Npgsql;

using Shopfolio.Endpoints;
using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.FileSecrets;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Settings;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// The one time source (09 7.1 item 1; 05 7.2.4).
builder.Services.AddTimeSource();

// Secrets come from files Compose mounts under /run/secrets, never from the environment (05 6.x.1). The connection string of the
// database (password included) is the whole content of one such file and is the value of Database:ConnectionString (09 3.5 item 2).
var mountedFiles = new SecretFiles(SecretFiles.DefaultDirectory);
((IConfigurationBuilder)builder.Configuration).Add(new SecretFileConfigurationSource(mountedFiles));
builder.Services.AddOptions<DatabaseOptions>()
    .BindConfiguration(DatabaseOptions.SectionName);
builder.Services.AddSingleton<IValidateOptions<DatabaseOptions>, DatabaseOptionsValidator>();

// The other settings and the start-up check of all of them, the database connection included (05 8.4.6): a required setting that is
// missing or malformed stops the application before it answers a request; a conditional one closes its line (09 3.5 item 2).
builder.Services.AddInstallationSettings();
// The web host connects with the web session profile (09 10.7 item 2): the session times are written into the connection string once,
// by DatabaseConnection, for the data source and the context alike.
static string WebConnectionString(IServiceProvider provider) =>
    DatabaseConnection.Create(provider.GetRequiredService<IOptions<DatabaseOptions>>().Value.ConnectionString, DatabaseSessionProfile.Web);

builder.Services.AddSingleton(provider => NpgsqlDataSource.Create(WebConnectionString(provider)));
builder.Services.AddDbContext<AppDbContext>((provider, options) => AppDbContext.Configure(options, WebConnectionString(provider)));

// One registration call per module (09 6 item 3).
builder.Services.AddCatalogModule();
builder.Services.AddOrderingModule();
builder.Services.AddPaymentModule();
builder.Services.AddAfterSalesModule();
builder.Services.AddMembershipModule();
builder.Services.AddCorporateContentModule();
builder.Services.AddNotificationsModule();
builder.Services.AddAdministrationModule();
builder.Services.AddSettingsModule();

WebApplication app = builder.Build();

app.UseRouting();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();
app.MapLivenessEndpoint();

app.Run();
