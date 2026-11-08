using Microsoft.Extensions.Options;

using Npgsql;

using Shopfolio.Commands;
using Shopfolio.Endpoints;
using Shopfolio.Endpoints.Files;
using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.FileSecrets;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Lists;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Infrastructure.SearchEngine;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.SignedEndpoints;
using Shopfolio.Infrastructure.StartupDatabase;
using Shopfolio.Infrastructure.Status;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Settings;

// The command mode (05 8.5.1; 09 13.2 item 1): the first argument "admin" runs a management command and builds no web host.
if (CommandHost.IsCommandMode(args))
{
    return await CommandHost.ExecuteAsync(args);
}

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Logging: the one configuration point (09 18.1 item 1) and the audit line writer of this process (09 18.1 item 2).
builder.AddApplicationLogging();
builder.Services.AddAuditLineWriter();

// The address rules (07 2.1): every page template is checked against the closed list of first path segments when the routes are built.
// The input validation pattern (09 3.4): the rule attributes with their codes, the trimming binder, the one page filter and the catalog messages.
builder.Services.AddRazorPages().AddAddressRules().AddSessionAccess().AddFieldValidation().AddFormSecurity().AddListPattern().AddUnexpectedErrorLayer();

// The two identity schemes, their policies and the closed doors (07 2.3.1; 09 15.1 item 2): the panel and the account folders are closed by folder path.
builder.Services.AddSessionAuthentication();

// The search engine data of the pages (05 3.7.2): structured data, preview tags and the closing tag, made from the record's values.
builder.Services.AddSearchEngineData();

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

// The reverse proxy's forwarded headers (09 15.1 item 8) and the general rate limit that counts on the address they give (09 15.4).
builder.Services.AddForwardedHeadersRules();
builder.Services.AddRequestRateLimiting();

// The database side of the start-up check (06 8.2.1): after the settings check above, so it runs second. A missing province or country list
// or a missing single-row record of the start data stops the application before it answers a request.
builder.Services.AddStartupDatabaseCheck();

// The in-memory cache (09 10.4; 05 7.10.1; MK-34): the one cache of the process and the scope's areas to drop after the commit.
builder.Services.AddAppCache();

// The transaction runner and the advisory lock (09 9.1 item 3, 10.2 item 5): scoped, with the scope's one context.
builder.Services.AddScoped<ITransactionRunner, TransactionRunner>();
builder.Services.AddScoped<AdvisoryLocks>();

// The POST handler's one sequence for an operation (07 8.1.2; 09 8.3 item 3): a page owns it, so it is common infrastructure and reaches no module type.
builder.Services.AddScoped<OperationRunner>();

// The counter of the attempt limits L-1 to L-6 and L-9 (09 15.4 items 5 and 6): the one writer of the attempt record.
builder.Services.AddScoped<AttemptCounter>();

// The sign-in record's one writer and the one wrapper of the password hasher (09 15.1 item 5; 02 3.13.19).
builder.Services.AddScoped<LoginRecorder>();
builder.Services.AddSingleton<PasswordService>();
builder.Services.AddScoped<SessionStore>();
builder.Services.AddScoped<PasswordRegime>();

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

// The status endpoint's warning conditions (05 9.4.2): after the scheduler, whose heartbeat client reads the certificate's end, and after the
// modules, which add their own conditions.
builder.Services.AddStatus();

WebApplication app = builder.Build();

// The forwarded headers (09 15.1 item 8) are the first middleware: the client address and the scheme come from the reverse proxy and from nobody else,
// before anything reads them (D-112).
app.UseForwardedHeadersRules();

// The security headers (09 15.2 item 3) come right after: every response carries them, the error response, the machine endpoint and the 429 included.
app.UseSecurityHeaders();

// The closed pages' header (05 3.7.4): a closed answer that is not HTML carries X-Robots-Tag: noindex, whoever answers; an HTML answer is closed by the
// robots meta tag the page head writes. The address decides, so it comes before the routing.
app.UseSearchEngineClosure();

// The general rate limit (09 15.4 item 1): after the forwarded headers and the security headers, before the endpoints are mapped. A refused request
// never reaches the handler.
app.UseRequestRateLimiting();

// The storefront writes no cookie until the cookie policy is published (05 6.7.10): outside every middleware that can write a cookie, the catch layer
// included (09 15.1 item 6). The catch layer (09 8.1 item 2b) writes no cookie and is registered right after it; a failing publication question
// is the one error it does not see - the framework's plain 500 answers it (D-113, T212).
app.UseCookieHeaderFilter();

// The single catching layer's second entry point (09 8.1 item 2b); the first is its page filter.
app.UseUnexpectedErrorLayer();

app.UseRouting();

// The body ceiling of the endpoint's class (07 2.8.1): after the routing, which finds the class, and before the authentication, the anti-forgery
// check and the handler, so a refused body is never read.
app.UseRequestBodyLimits();

// The session schemes read the cookie and the closed doors answer a request without a session (07 2.3.2); after the routing, which finds the endpoint metadata.
app.UseAuthentication();
app.UseAuthorization();

// The static files are served under /statik (07 2.1.2, 3.1.6) with a fingerprint in the file name: a changed file gets a new address (07 2.10.3; 05 3.8.1).
app.MapGroup(AddressRules.StaticArea).MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();
app.MapLivenessEndpoint();
app.MapStatusEndpoint();

// The site map and robots.txt (UC-79; 05 3.7.3, 3.7.4): open machine endpoints, made on request; the map comes from the cache.
app.MapSitemapEndpoints();

// The file endpoints (UC-37, UC-83; 09 15.7): the signed download and the chunked upload under the panel's session.
app.MapFileEndpoints();

// The payment provider's notification (UC-80; 07 4.30.1): the signed machine endpoint, mapped by the Payment module that records it.
app.MapPaymentNotificationEndpoint();

app.Run();
return (int)CommandExit.Success;
