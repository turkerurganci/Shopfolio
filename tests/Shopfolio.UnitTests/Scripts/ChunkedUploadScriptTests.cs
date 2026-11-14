using System.Diagnostics;
using System.Text.RegularExpressions;

using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Scripts;

/// <summary>
/// The chunked upload script of the product form (T548; 09 16.2 items 6 and 7; 11 T548 KK1-KK10). The behaviour is run by Node without a browser from
/// <c>chunked-upload.test.mjs</c> (the start, part and status flow; the 409, 404, 400, 413, 422 and 500 answers; the redirect; the three resumes after a network error; the save lock); the
/// source scan is here: the script holds no part size and no size limit, calls no endpoint other than the upload ones and sends nothing when the page is left.
/// </summary>
public sealed partial class ChunkedUploadScriptTests
{
    private const string Module = "src/Shopfolio.Web/wwwroot/js/pages/panel/product-form.js";

    // The part size (5 MB) and the digital file's limit (P-26, 500 MB) as numbers or as products of numbers: both come from the server.
    [GeneratedRegex(@"\b5242880\b|\b524288000\b|\b500\s*\*\s*1024|\b5\s*\*\s*1024\b|\b5\s*\*\s*1_?048_?576\b|\b500\s*\*\s*1_?048_?576\b|\b500\s*MB\b|\b5\s*MB\b", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SizeNumber();

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex BlockComment();

    [GeneratedRegex(@"(?m)^\s*//.*$|(?<=\s)//\s.*$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LineComment();

    // A request to an endpoint of the upload group is the only call, and beforeunload, pagehide and the like send nothing (09 16.2 item 6.5).
    [GeneratedRegex(@"\bbeforeunload\b|\bunload\b|\bpagehide\b|\bvisibilitychange\b|method\s*:\s*[""'](?:DELETE|PUT|PATCH)[""']", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LeaveOrOtherVerb();

    private static List<string> SizeViolations(string script)
    {
        string text = LineComment().Replace(BlockComment().Replace(script, string.Empty), string.Empty);
        return [.. SizeNumber().Matches(text).Select(m => $"size number: {m.Value}"), .. LeaveOrOtherVerb().Matches(text).Select(m => $"leave or other verb: {m.Value}")];
    }

    [Fact]
    public void TheModule_HoldsNoPartSizeNoSizeLimit_AndSendsNothingOnLeaving_KK10()
    {
        Assert.Empty(SizeViolations(File.ReadAllText(Path.Combine(RepositoryRoot.Find(), Module))));
    }

    [Theory]
    [InlineData("const CHUNK = 5242880;")]
    [InlineData("const CHUNK = 5 * 1024 * 1024;")]
    [InlineData("const LIMIT = 524288000;")]
    [InlineData("const LIMIT = 500 * 1024 * 1024;")]
    [InlineData("const LIMIT = 500 * 1_048_576;")]
    [InlineData("const text = 'En çok 500 MB';")]
    [InlineData("window.addEventListener('beforeunload', cancel);")]
    [InlineData("window.addEventListener('pagehide', cancel);")]
    [InlineData("await send(url, { method: 'DELETE' });")]
    public void ASizeNumberOrALeavingCall_IsFound_KK10(string line) => Assert.NotEmpty(SizeViolations(line));

    [Theory]
    [InlineData("const BYTES_PER_UNIT = 1024;")]
    [InlineData("const RETRY_DELAYS_MS = [1000, 2000, 4000];")]
    [InlineData("// the 5242880 and 500 MB of the server are not written here")]
    [InlineData("await send(url, { method: 'GET' });")]
    [InlineData("const received = 5 + 2;")]
    public void ANearMissOfTheSizeRule_IsLeftAlone_KK10(string line) => Assert.Empty(SizeViolations(line));

    [Fact]
    public async Task TheScript_RunsItsFlowTests_KK1_KK9()
    {
        string root = RepositoryRoot.Find();
        string test = Path.Combine(root, "tests", "Shopfolio.UnitTests", "Scripts", "chunked-upload.test.mjs");
        ProcessStartInfo start = new("node") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
        start.ArgumentList.Add(test);
        start.ArgumentList.Add(Path.Combine(root, Module));
        using Process process = Process.Start(start) ?? throw new InvalidOperationException("node did not start.");
        Task<string> output = process.StandardOutput.ReadToEndAsync(TestContext.Current.CancellationToken);
        Task<string> error = process.StandardError.ReadToEndAsync(TestContext.Current.CancellationToken);
        Task finished = process.WaitForExitAsync(TestContext.Current.CancellationToken);
        bool exited = await Task.WhenAny(finished, Task.Delay(TimeSpan.FromSeconds(100), TestContext.Current.CancellationToken)) == finished;
        if (!exited)
        {
            process.Kill(entireProcessTree: true);
        }

        Assert.True(exited, "The script test did not finish in time.");
        Assert.True(process.ExitCode == 0, await output + await error);
    }
}
