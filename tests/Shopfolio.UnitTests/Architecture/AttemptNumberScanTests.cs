using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The counting code of the attempt limits writes no threshold and no window (T376 criterion 2, KK2; 09 3.5 item 1): the files that count are scanned, and the
/// scan is shown to see a number written the three ways a counter would write one - a comparison, a time span and a named argument.
/// </summary>
public sealed class AttemptNumberScanTests
{
    private static readonly string[] CountingFiles =
    [
        "src/Shopfolio.Web/Infrastructure/Security/AttemptCounter.cs",
        "src/Shopfolio.Web/Infrastructure/Security/AttemptLimits.cs",
        "src/Shopfolio.Web/Infrastructure/Security/AttemptLimitInventory.cs",
        "src/Shopfolio.Web/Infrastructure/Security/AttemptRecord.cs",
        "src/Shopfolio.Web/Infrastructure/Security/AttemptSubject.cs",
        "src/Shopfolio.Web/Modules/Ordering/OpenUnpaidOrderLimit.cs",
    ];

    [Fact]
    public void TheCountingCode_WritesNoThresholdAndNoWindow_KK2()
    {
        string root = RepositoryRoot.Find();
        foreach (string relative in CountingFiles)
        {
            string path = Path.Combine(root, relative);
            Assert.True(File.Exists(path), relative + " is missing; update the scan with the move.");
            Assert.Empty(AttemptNumberScan.Scan(File.ReadAllText(path)));
        }
    }

    [Theory]
    [InlineData("return count > 5;")]
    [InlineData("DateTimeOffset from = now - TimeSpan.FromMinutes(15);")]
    [InlineData("var rule = new AttemptRule(Threshold: 20, Window: window);")]
    [InlineData("return open >= 3;")]
    [InlineData("const double window = 0.25;")]
    [InlineData("return count > 0x5;")]
    [InlineData("return count > 0b101;")]
    [InlineData("return count > 1_5;")]
    [InlineData("return count > 5L;")]
    public void ANumberWrittenInTheCode_IsFound_KK2(string code)
    {
        Assert.Contains(AttemptNumberScan.Scan(code), f => f.Rule == AttemptNumberScan.NumberWritten);
    }

    [Theory]
    [InlineData("string id = \"P-31\"; // five in 15 minutes")]
    [InlineData("/* 5 per 15 minutes */ int first = 1;")]
    [InlineData("return count > rule.Threshold && count != 0;")]
    [InlineData("AttemptLimit limit = AttemptLimit.L7;")]
    public void ACommentAStringTheIdentifiersDigitAndZeroOrOne_AreNotNumbers_KK2(string code)
    {
        Assert.Empty(AttemptNumberScan.Scan(code));
    }
}
