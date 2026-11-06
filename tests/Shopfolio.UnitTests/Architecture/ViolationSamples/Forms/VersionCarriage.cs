// Deliberate violation sample of the form version scan (07 2.4.7). The scan reads this file as text; it is not compiled.
// Line numbers are part of the test. A full-line comment that says If-Match, ETag, ?Version=1 or asp-route-version is not a finding.
internal sealed class VersionCarriage
{
    public static IActionResult Moves(RecordInput input)
    {
        FormResults.SeeOther($"/panel/urun?Version={input.Version}");
        FormResults.SeeOther($"/panel/urun/{input.Id}/{input.Version}");
        FormResults.SeeOther(QueryHelpers.AddQueryString("/panel/urun", "originalStock", "5"));
        FormResults.SeeOther("/panel/urun?PaymentMethodsVersion=3");
        FormResults.SeeOther(
            $"/panel/urun/{input.Id}"
            + $"?x=1&OriginalText={input.OriginalText}");
        return FormResults.SeeOther("/panel/urun?conversion=1");
    }

    public static void Headers(HttpContext context)
    {
        context.Response.Headers.ETag = "\"3\"";
        string? match = context.Request.Headers["If-Match"];
        context.Response.Headers.Append("X-Record-Version", "3");
        context.Response.Headers["originalStock"] = "5";
        context.Request.Headers.TryGetValue("If-None-Match", out StringValues none);
        EntityTagHeaderValue tag = new("\"3\"");
        string name = HeaderNames.IfMatch;
        context.Response.Headers.Append("Cache-Control", "no-store");
    }

    public static void Templates(IEndpointRouteBuilder app)
    {
        app.MapGet("/panel/urun/{id}/{version:int}", () => "x");
        app.MapGet("/panel/urun/{id:guid}", () => "x");
    }

    public static string Fine => "/panel/urun?page=2";
    public static string FineToo(RecordInput input) => FormResults.SeeOther($"/panel/urun/{input.Id}").ToString();
}
