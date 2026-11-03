using Microsoft.Extensions.Options;

using Npgsql;

using Shopfolio.Endpoints;
using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.FileSecrets;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

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

WebApplication app = builder.Build();

app.UseRouting();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();
app.MapLivenessEndpoint();

app.Run();
