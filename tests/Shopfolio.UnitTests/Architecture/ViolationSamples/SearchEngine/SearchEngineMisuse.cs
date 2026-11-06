// Deliberate violation sample of the search engine source scan (T219; 09 9.4 item 3, 09 19.2 item 3). The scan reads this file as text; it is not compiled.
// Line numbers are part of the test.
internal sealed class FoldsAndWritesItsOwn
{
    // A comment that says NormalizationForm.FormD, [^a-z0-9], sitemap.xml and X-Robots-Tag is not a finding.
    public string DecomposesTheName(string name) => name.Normalize(NormalizationForm.FormD);

    public string RemovesTheAccents(string name) => RemoveDiacritics(name);

    public string ReplacesByHand(string name) => name.Replace('ç', 'c').Replace("ş", "s");

    public string MakesItsOwnAddress(string name) => Regex.Replace(name.ToLowerInvariant(), "[^a-z0-9]+", "-");

    public string MakesItsOwnAddressWithASpace(string name) => Regex.Replace(name, "[^ a-z0-9]+", "-");

    public string MeansTheSameOnTheLine(string name) => name; // the trailing comment mentions noindex and is not a finding

    public string Compares(string a, string b) => string.Compare(a, b, CultureInfo.InvariantCulture, CompareOptions.IgnoreNonSpace).ToString();
}

internal sealed class FirmFillsTheSearchEngineFields
{
    public string MetaTitle { get; set; } = string.Empty;

    public string? MetaDescription { get; set; }

    public string SeoTitle { get; set; } = string.Empty;

    public string? OgImage { get; set; }

    public string CanonicalUrl { get; set; } = string.Empty;

    [Column("meta_keywords")]
    public string? Keywords { get; set; }

    public string OpenGraphType { get; set; } = "website";
}

internal static class OwnEndpointsAndOwnClosing
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/sitemap.xml", () => "x");
        app.MapGet("/robots.txt", () => "User-agent: *");
    }

    public static void Close(HttpResponse response) => response.Headers["X-Robots-Tag"] = "noindex";

    public static void CloseByAppend(HttpResponse response) => response.Headers.Append("x-robots-tag", "none");

    public static string Meta() => "<meta name=\"robots\" content=\"noindex\">";

    public static string Url() => "https://example.test//not-a-comment/robots.txt";
}
