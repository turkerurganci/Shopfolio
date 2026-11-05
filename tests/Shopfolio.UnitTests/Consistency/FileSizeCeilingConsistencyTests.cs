using System.Globalization;
using System.Text.RegularExpressions;

using Shopfolio.Infrastructure;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the two file size ceilings of the code equal to the rows of the product constants table in <c>02</c> (T223; 09 21.4 item 1): P-23, the
/// size of one image file, and P-26, the size of a digital product file. <c>ProductConstants</c> is the only place the code has these numbers.
/// The rows are read as megabytes; the table's value is the one the owner changes, so a change in either place fails here. Runs in the Lint step.
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class FileSizeCeilingConsistencyTests
{
    private const long Megabyte = 1024L * 1024;

    private static string Row(string id)
    {
        string[] candidates = Directory.GetFiles(Path.Combine(RepositoryRoot.Find(), "Docs"), "02_*.md");
        string document = File.ReadAllText(Assert.Single(candidates)).ReplaceLineEndings("\n");
        return document.Split('\n').Single(line => line.StartsWith($"| {id} |", StringComparison.Ordinal));
    }

    private static long Megabytes(string row)
    {
        Match match = Ceiling().Match(row);
        Assert.True(match.Success, "The row of the 02 constants table has changed shape; update this check with it.");
        return long.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) * Megabyte;
    }

    [Fact]
    public void P23Row_EqualsTheImageCeiling()
    {
        Assert.Equal(ProductConstants.ImageMaxBytes, Megabytes(Row("P-23")));
    }

    [Fact]
    public void P26Row_EqualsTheDigitalFileCeiling()
    {
        Assert.Equal(ProductConstants.DigitalFileMaxBytes, Megabytes(Row("P-26")));
    }

    [Fact]
    public void TheTwoRows_AreTheOnesThatNameTheFileSizeCeilings()
    {
        Assert.Contains("Tek görsel dosyasının boyut tavanı", Row("P-23"), StringComparison.Ordinal);
        Assert.Contains("Dijital ürün dosyasının boyut tavanı", Row("P-26"), StringComparison.Ordinal);
    }

    [GeneratedRegex(@"\*\*([0-9]+) MB\*\*", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex Ceiling();
}
