// Deliberate violation sample of the page source scan (07 2.10.1, 2.10.3). The scan reads this file as text; it is not compiled.
// Line numbers are part of the test. A full-line comment that says [ApiController], "/api/x", ApiVersion or api-version is not a finding.
[ApiController]
internal sealed class OrdersApi : ControllerBase
{
    public static void Register(IEndpointRouteBuilder app, IServiceCollection services)
    {
        app.MapControllers();
        services.AddControllers();
        app.MapGet("/api/orders", () => "x");
        app.MapPost("api/orders", () => "x");
        app.MapGet("/v2/orders", () => "x");
        services.AddApiVersioning();
        const string Route = "/api";
    }

    public static string Header => "X-Api-Version";
    public static string Parameter(HttpRequest request) => request.Query["api-version"].ToString();
    public static string Media => "application/vnd.shopfolio.v1+json";
    public static string Fine => "/panel/siparis";
    public static string Vitrin => "/urun/versiyon-kitabi";
}
