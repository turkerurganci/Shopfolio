WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

WebApplication app = builder.Build();

app.UseRouting();

app.MapStaticAssets();
app.MapRazorPages()
    .WithStaticAssets();

app.Run();
