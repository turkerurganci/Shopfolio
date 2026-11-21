namespace Shopfolio.IntegrationTests.Browser;

/// <summary>
/// The way the tool runs a second version of an engine (D-1709; 12 4.17 "son iki büyük sürüm"): Playwright brings one build of each engine, so the previous major
/// version is run with the same tool in a second call whose environment names that build's executable: <c>SHOPFOLIO_BROWSER_EXECUTABLES</c> =
/// <c>chromium=&lt;path&gt;|firefox=&lt;path&gt;</c> (the pairs are separated by <c>|</c>, since a Windows path has both <c>:</c> and <c>;</c>). An engine with no pair runs the
/// build Playwright brings. The record names the version every run used (<c>ObservationRecord.SetEngineVersion</c>), so the two runs are told apart by the record.
/// </summary>
internal static class ExecutableOverrides
{
    public const string VariableName = "SHOPFOLIO_BROWSER_EXECUTABLES";

    /// <summary>The executable the setting names for the engine, or <see langword="null"/> (the setting is not set, names no such engine, or leaves the path empty).</summary>
    public static string? PathOf(string? setting, string engine)
    {
        if (string.IsNullOrWhiteSpace(setting))
        {
            return null;
        }

        foreach (string pair in setting.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            int equals = pair.IndexOf('=', StringComparison.Ordinal);
            if (equals > 0 && pair[..equals].Trim().Equals(engine, StringComparison.OrdinalIgnoreCase))
            {
                string path = pair[(equals + 1)..].Trim().Trim('"');
                return path.Length == 0 ? null : path;
            }
        }

        return null;
    }
}

public sealed class ExecutableOverridesTests
{
    [Fact]
    public void PathOf_WhenNotSet_IsNull_D1709()
    {
        Assert.Null(ExecutableOverrides.PathOf(null, "chromium"));
        Assert.Null(ExecutableOverrides.PathOf("  ", "chromium"));
    }

    [Theory]
    [InlineData("chromium=C:\\old\\chrome.exe|firefox=D:\\ff\\firefox.exe", "chromium", "C:\\old\\chrome.exe")]
    [InlineData("chromium=C:\\old\\chrome.exe|firefox=D:\\ff\\firefox.exe", "firefox", "D:\\ff\\firefox.exe")]
    [InlineData("Chromium = \"C:\\Program Files\\old\\chrome.exe\" ", "chromium", "C:\\Program Files\\old\\chrome.exe")]
    [InlineData("webkit=/opt/old/pw_run.sh", "WEBKIT", "/opt/old/pw_run.sh")]
    [InlineData("chromium=C:\\a;b\\c.exe", "chromium", "C:\\a;b\\c.exe")]
    public void PathOf_ReturnsThePathOfTheNamedEngine_D1709(string setting, string engine, string expected)
    {
        Assert.Equal(expected, ExecutableOverrides.PathOf(setting, engine));
    }

    [Theory]
    [InlineData("chromium=C:\\old\\chrome.exe", "firefox")]
    [InlineData("chromium=", "chromium")]
    [InlineData("=C:\\old\\chrome.exe", "chromium")]
    [InlineData("chromium", "chromium")]
    [InlineData("chromiumx=C:\\old\\chrome.exe", "chromium")]
    public void PathOf_WhenTheEngineHasNoPath_IsNullAndTheEngineRunsPlaywrightsBuild_D1709(string setting, string engine)
    {
        Assert.Null(ExecutableOverrides.PathOf(setting, engine));
    }

    [Fact]
    public void VariableName_IsTheDocumentedOne_D1709()
    {
        Assert.Equal("SHOPFOLIO_BROWSER_EXECUTABLES", ExecutableOverrides.VariableName);
    }
}
