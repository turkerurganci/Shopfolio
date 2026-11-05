using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the outside services' link layer (T202 KK1, KK2, KK4, KK9; 08 3.1 rules 1 and 2; 09 11.1 item 1, 11.2 items 3 and 5; 09 19.1
/// item 3; 09 19.2 item 3). Five rules:
/// (1) the name of the payment provider or of the e-mail service appears in no application file - comments included - except the layer's own
/// implementation files: a file directly in <c>Infrastructure/Integrations/</c> whose name begins with an implementation's prefix
/// (<see cref="ImplementationPrefixes"/>). The interfaces, the result types and the requests of the layer, the modules, the pages and the
/// endpoints carry no such name. Google is not scanned: it is the product's own sign-in line ("Google ile giriş"), and 09 11.1 names
/// <c>IGoogleIdentityGateway</c> and <c>GoogleIdentityResult</c>.
/// (2) a module, a page or an endpoint names no HTTP or SMTP client type: it calls the gateway interface only (MK-17).
/// (3) no <c>new HttpClient(...)</c> anywhere: a client comes from the factory.
/// (4) no retry or resilience handler and no retry count above zero anywhere (08 3.1 rule 2).
/// (5) the fakes of <c>UnitTests/Fakes/</c> name no network type: they never leave the process.
/// Rules 2-5 skip full-line comments (they keep their line); rule 1 reads the whole text. The scan reads text and writes nothing.
/// </summary>
internal static partial class IntegrationSourceScan
{
    public const string ProviderNameOutsideLayer = "names the payment provider or the e-mail service outside the layer's implementation files (08 3.1 rule 1)";
    public const string ClientTypeOutsideLayer = "names an HTTP or SMTP client type in a module, page or endpoint (08 3.1 rule 1; MK-17: the gateway interface only)";
    public const string NewHttpClient = "creates an HttpClient with new (09 11.2 item 5: a client comes from the factory)";
    public const string RetryHandler = "adds a retry or resilience handler or a retry count above zero (08 3.1 rule 2; 09 11.2 items 1 and 3)";
    public const string FakeUsesNetwork = "a fake names a network type (09 19.1 items 3 and 6: the fakes never leave the process)";

    /// <summary>The folder of the layer; only its own implementation files may carry a provider's name.</summary>
    public const string LayerFolder = "src/Shopfolio.Web/Infrastructure/Integrations/";

    /// <summary>The prefixes of the implementation files' names: the payment provider's gateway and the e-mail service's gateway (09 11.1 item 1).</summary>
    public static readonly string[] ImplementationPrefixes = ["PayTr", "Ses"];

    /// <summary>The folders whose code calls the gateway interfaces and nothing below them.</summary>
    public static readonly string[] CallerFolders = ["src/Shopfolio.Web/Modules/", "src/Shopfolio.Web/Pages/", "src/Shopfolio.Web/Endpoints/"];

    /// <summary>The folder of the hand-written fakes.</summary>
    public const string FakesFolder = "tests/Shopfolio.UnitTests/Fakes/";

    public static bool IsImplementationFile(string path)
    {
        if (!path.StartsWith(LayerFolder, StringComparison.Ordinal))
        {
            return false;
        }

        string rest = path[LayerFolder.Length..];
        return !rest.Contains('/', StringComparison.Ordinal)
            && ImplementationPrefixes.Any(prefix => rest.StartsWith(prefix, StringComparison.Ordinal));
    }

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        string path = relativePath.Replace('\\', '/');
        string whole = text.ReplaceLineEndings("\n");
        string code = BlankCommentLines(whole);
        List<(string Rule, int Line)> findings = [];

        if (path.StartsWith(FakesFolder, StringComparison.Ordinal))
        {
            Add(findings, FakeUsesNetwork, NetworkTypes(), code);
            return [.. findings.OrderBy(f => f.Line)];
        }

        if (!IsImplementationFile(path))
        {
            Add(findings, ProviderNameOutsideLayer, ProviderNames(), whole);
        }

        if (CallerFolders.Any(folder => path.StartsWith(folder, StringComparison.Ordinal)))
        {
            Add(findings, ClientTypeOutsideLayer, ClientTypes(), code);
        }

        Add(findings, NewHttpClient, NewClient(), code);
        Add(findings, RetryHandler, RetrySetting(), code);
        return [.. findings.OrderBy(f => f.Line).ThenBy(f => f.Rule, StringComparer.Ordinal)];
    }

    private static void Add(List<(string Rule, int Line)> findings, string rule, Regex pattern, string text)
    {
        foreach (Match match in pattern.Matches(text))
        {
            int line = text.AsSpan(0, match.Index).Count('\n') + 1;
            if (!findings.Contains((rule, line)))
            {
                findings.Add((rule, line));
            }
        }
    }

    private static string BlankCommentLines(string text)
    {
        string[] lines = text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].TrimStart().StartsWith("//", StringComparison.Ordinal))
            {
                lines[i] = string.Empty;
            }
        }

        return string.Join('\n', lines);
    }

    // The payment provider in any case where a word or a camel-case part begins (not "display tree"), the e-mail service's short name as a
    // word or a type prefix, the cloud's name and the service's long name.
    [GeneratedRegex(@"(?:(?<![A-Za-z])(?i:p)|(?<=[a-z])P)(?i:ay[\s_-]?tr)|\bSES\b|\bSes(?:\b|[A-Z])|(?i:\bamazon)|\bAWS\b|\bAws[A-Z]|(?i:simple\s*email)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ProviderNames();

    [GeneratedRegex(@"\b(?:HttpClient|IHttpClientFactory|HttpMessageHandler|SocketsHttpHandler|HttpClientHandler|SmtpClient)\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ClientTypes();

    // The constructor call, and the target-typed form after a declared HttpClient.
    [GeneratedRegex(@"\bnew\s+(?:System\.Net\.Http\.)?HttpClient\s*\(|\bHttpClient\??\s+\w+\s*=\s*new\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex NewClient();

    [GeneratedRegex(@"\b(?:AddStandardResilienceHandler|AddStandardHedgingHandler|AddResilienceHandler|AddPolicyHandler|AddTransientHttpErrorPolicy|Polly)\b|\bMaxErrorRetry\s*=\s*(?!0\b)\d+", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RetrySetting();

    [GeneratedRegex(@"\b(?:HttpClient|IHttpClientFactory|HttpMessageHandler|SocketsHttpHandler|HttpClientHandler|SmtpClient|TcpClient|UdpClient|Socket|Dns|WebRequest|NetworkStream)\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex NetworkTypes();
}
