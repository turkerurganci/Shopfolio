using Microsoft.Extensions.Options;

using Npgsql;

using Shopfolio.Endpoints;
using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.FileSecrets;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

// Secrets come from files Compose mounts under /run/secrets, never from the environment (05 6.x.1).
builder.Services.AddSingleton(new SecretFiles(SecretFiles.DefaultDirectory));
builder.Services.AddSingleton<IPostConfigureOptions<DatabaseOptions>, DatabasePasswordPostConfigure>();
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
