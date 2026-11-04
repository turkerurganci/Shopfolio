using Microsoft.Extensions.Options;

using Npgsql;

using Shopfolio.Endpoints;
using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.FileSecrets;
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
    .BindConfiguration(DatabaseOptions.SectionName)
    .ValidateOnStart();
builder.Services.AddSingleton<IValidateOptions<DatabaseOptions>, DatabaseOptionsValidator>();
builder.Services.AddSingleton(provider => NpgsqlDataSource.Create(provider.GetRequiredService<IOptions<DatabaseOptions>>().Value.ConnectionString));
builder.Services.AddDbContext<AppDbContext>((provider, options) =>
    AppDbContext.Configure(options, provider.GetRequiredService<IOptions<DatabaseOptions>>().Value.ConnectionString));

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
