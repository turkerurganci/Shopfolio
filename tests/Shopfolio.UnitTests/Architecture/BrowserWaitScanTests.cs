using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The browser tests wait for what they need, never for a length of time (D-1704; 09 19.2 item 6). The browser folder is scanned with no exception, and the scan
/// is shown (09 19.2 item 3) to see every spelling of a fixed wait and to leave alone the text that only names one.
/// </summary>
public sealed class BrowserWaitScanTests
{
    private const string BrowserFolder = "tests/Shopfolio.IntegrationTests/Browser";

    [Fact]
    public void TheBrowserTests_WaitForNoLengthOfTime_D1704()
    {
        string folder = Path.Combine(RepositoryRoot.Find(), BrowserFolder);
        string[] files = Directory.EnumerateFiles(folder, "*.cs", SearchOption.AllDirectories).ToArray();
        Assert.Contains(files, f => f.EndsWith("BrowserTestBase.cs", StringComparison.Ordinal));
        Assert.True(files.Length >= 10, "The browser folder is read: " + files.Length + " files.");

        List<string> findings = [];
        foreach (string file in files)
        {
            foreach ((string rule, int line) in BrowserWaitScan.Scan(File.ReadAllText(file)))
            {
                findings.Add($"{Path.GetRelativePath(folder, file)}:{line} {rule}");
            }
        }

        Assert.Empty(findings);
    }

    [Theory]
    [InlineData("await Task.Delay(500, Ct);")]
    [InlineData("await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(1), ct);")]
    [InlineData("await Task\n    .Delay(250);")]
    [InlineData("await Delay(250);")]
    [InlineData("Thread.Sleep(100);")]
    [InlineData("System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));")]
    [InlineData("await page.Page.WaitForTimeoutAsync(300);")]
    [InlineData("await page.WaitForTimeout (300);")]
    [InlineData("string script = \"() => new Promise(r => setTimeout(r, 200))\";")]
    [InlineData("string script = \"setInterval(poll, 50)\";")]
    [InlineData("string text = \"a // b\"; await Task.Delay(5);")]
    public void AFixedWait_IsFound_D1704(string code)
    {
        Assert.Contains(BrowserWaitScan.Scan(code), f => f.Rule == BrowserWaitScan.FixedWait);
    }

    [Theory]
    [InlineData("// await Task.Delay(500, Ct);")]
    [InlineData("int a = 1; // a Task.Delay would hide the race")]
    [InlineData("/* Thread.Sleep(100) */ int first = 1;")]
    [InlineData("string note = \"no Task.Delay here\";")]
    [InlineData("string url = \"http://localhost/x\"; await page.Page.WaitForLoadStateAsync(LoadState.Load);")]
    [InlineData("await Assertions.Expect(locator).ToHaveTextAsync(\"x\");")]
    [InlineData("await page.Page.RunAndWaitForNavigationAsync(act, options);")]
    [InlineData("TimeSpan timeout = TimeSpan.FromSeconds(30); bool delayed = false; int sleeping = 0;")]
    public void ACommentAStringOrAnotherWait_IsNotAFixedWait_D1704(string code)
    {
        Assert.Empty(BrowserWaitScan.Scan(code));
    }

    [Fact]
    public void AFixedWait_IsReportedAtItsLine_AndEveryLineOnce_D1704()
    {
        const string code = "int a = 1;\nawait Task.Delay(1);\nint b = 2;\nThread.Sleep(2);\n/* x\nTask.Delay(3) */\nawait page.WaitForTimeoutAsync(4);";

        int[] lines = BrowserWaitScan.Scan(code).Select(f => f.Line).ToArray();

        Assert.Equal([2, 4, 7], lines);
    }
}
