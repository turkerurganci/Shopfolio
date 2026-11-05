using Microsoft.Extensions.Options;

using Npgsql;

using Shopfolio.Endpoints;
using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.FileSecrets;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.SignedEndpoints;
using Shopfolio.Infrastructure.StartupDatabase;
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

// Logging: the one configuration point (09 18.1 item 1) and the audit line writer of this process (09 18.1 item 2).
builder.AddApplicationLogging();
builder.Services.AddAuditLineWriter();

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

// The data protection ring: keys in the database, in clear, framework default lifetime (09 15.5 item 1). After the context it persists to.
DataProtectionSetup.Add(builder.Services);

// The cookie rules (09 15.1 item 6; 05 6.7): the anti-forgery cookie, the one cookie writer and the gate of the storefront's cookie headers.
builder.Services.AddCookieRules();

// The database side of the start-up check (06 8.2.1): after the settings check above, so it runs second. A missing province or country list
// or a missing single-row record of the start data stops the application before it answers a request.
builder.Services.AddStartupDatabaseCheck();

// The transaction runner and the advisory lock (09 9.1 item 3, 10.2 item 5): scoped, with the scope's one context.
builder.Services.AddScoped<ITransactionRunner, TransactionRunner>();
builder.Services.AddScoped<AdvisoryLocks>();

// The link layer (09 15.1 items 3 and 4): the key derivation and the reading and consuming of link records.
builder.Services.AddLinkRecords();

// The link layer of the outside services (09 11.1, 11.2): the common call runner, the named HTTP clients without retry and, in development,
// the e-mail gateway that writes to the capture tool (09 4.3 item 2). Modules take only the gateway interfaces.
builder.Services.AddIntegrations(builder.Configuration);

// The common pipeline of the signed machine endpoints (08 3.2; 09 11.1 item 5): body limit, signature, record, "received". The endpoint itself
// is mapped by the task that builds it, with MapSignedEndpoint.
builder.Services.AddSignedEndpoints();

// One registration call per module (09 6 item 3).
// The file store and the image processor are the infrastructure the Catalog module's files stand on (09 6 item 6, 15.6).
builder.Services.AddFileStore();
builder.Services.AddSingleton<Shopfolio.Infrastructure.Imaging.ImageProcessor>();
builder.Services.AddCatalogModule();
builder.Services.AddOrderingModule();
builder.Services.AddPaymentModule();
builder.Services.AddAfterSalesModule();
builder.Services.AddMembershipModule();
builder.Services.AddCorporateContentModule();
builder.Services.AddNotificationsModule();
builder.Services.AddAdministrationModule();
builder.Services.AddSettingsModule();

// The job queue and its one runner service (09 13.1): after the modules, so the runner starts after the start-up checks and the start-up rewrite
// of the pending revert links' digests, and after the handlers the modules register (05 4.3.2, 6.x.20).
builder.Services.AddJobQueue();
builder.Services.AddScheduler();

// The disposal and the clean-up of the temporary records are steps of the scheduler's nightly job (05 5.2.8, 5.2.9): after the scheduler, so the
// queue's clean-up runs before them. There is no second timer or service.
builder.Services.AddDisposal(builder.Configuration);

WebApplication app = builder.Build();

// The storefront writes no cookie until the cookie policy is published (05 6.7.10): outside every middleware that can write a cookie. The forwarded
// headers and the security headers (09 15.1 item 8, 15.2 item 3) come before it when their tasks add them.
app.UseCookieHeaderFilter();

app.UseRouting();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();
app.MapLivenessEndpoint();

app.Run();
