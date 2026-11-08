using System.Globalization;
using System.Text.Json.Nodes;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Ordering;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Ordering;

/// <summary>
/// The stored form of the order's shipping fee VAT breakdown (06 2.5.1, 4.17; 09 12 item 5 and item 8): the breakdown is written by the one payload setting, and every
/// form ever written is still read - the sample files under <c>Payloads/ShippingFeeVatShare</c>, one per form; a new form adds a sample, never replaces one.
/// </summary>
public sealed class ShippingFeeVatSharePayloadTests
{
    private static string SampleFolder => Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Payloads", "ShippingFeeVatShare");

    [Fact]
    public void TheBreakdown_IsWrittenByThePayloadSettingWithTheMembersNames_AndReadBack()
    {
        List<ShippingFeeVatShare> breakdown = [new(20m, 3.33m, 0.56m), new(10m, 6.67m, 0.61m)];

        string json = PayloadJson.Write(breakdown);
        List<ShippingFeeVatShare>? back = PayloadJson.Read<List<ShippingFeeVatShare>>(json);

        Assert.Contains("\"VatRate\":20", json, StringComparison.Ordinal);
        Assert.Contains("\"Part\":3.33", json, StringComparison.Ordinal);
        Assert.Contains("\"Vat\":0.56", json, StringComparison.Ordinal);
        Assert.NotNull(back);
        Assert.Equal(breakdown, back);
    }

    [Fact]
    public void EveryFormEverWritten_IsReadWithoutAnError_AndItsPartsAreAboveZero()
    {
        string[] files = Directory.GetFiles(SampleFolder, "*.json");

        Assert.NotEmpty(files);
        Assert.Contains(files, f => Path.GetFileName(f) == "1.json");
        foreach (string file in files)
        {
            List<ShippingFeeVatShare>? read = PayloadJson.Read<List<ShippingFeeVatShare>>(File.ReadAllText(file));
            Assert.NotNull(read);
            Assert.NotEmpty(read);
            Assert.All(read, s => Assert.True(s.Part > 0m));
        }
    }

    [Fact]
    public void TheFirstSample_ReadsTheValuesItHolds()
    {
        List<ShippingFeeVatShare>? read = PayloadJson.Read<List<ShippingFeeVatShare>>(File.ReadAllText(Path.Combine(SampleFolder, "1.json")));

        Assert.NotNull(read);
        Assert.Equal([new ShippingFeeVatShare(20m, 3.33m, 0.56m), new ShippingFeeVatShare(10m, 6.67m, 0.61m)], read);
        Assert.Equal(10.00m, read.Sum(s => s.Part));
    }

    [Fact]
    public void AFieldTheCodeDoesNotKnow_IsSkipped_AndAMissingMemberTakesItsDefault()
    {
        List<ShippingFeeVatShare>? read = PayloadJson.Read<List<ShippingFeeVatShare>>("[{\"VatRate\":20,\"Part\":10.00,\"FromALaterVersion\":{\"x\":1}}]");

        Assert.NotNull(read);
        ShippingFeeVatShare share = Assert.Single(read);
        Assert.Equal(20m, share.VatRate);
        Assert.Equal(10.00m, share.Part);
        Assert.Equal(0m, share.Vat);
    }

    [Fact]
    public void TheFormTheCodeWritesToday_HasTheMemberSetOfTheNewestSample()
    {
        string newest = Directory.GetFiles(SampleFolder, "*.json").OrderBy(f => int.Parse(Path.GetFileNameWithoutExtension(f), CultureInfo.InvariantCulture)).Last();
        List<ShippingFeeVatShare> full = [new(20m, 3.33m, 0.56m)];

        JsonNode written = JsonNode.Parse(PayloadJson.Write(full))!;
        JsonNode sample = JsonNode.Parse(File.ReadAllText(newest))!;

        Assert.Equal(MemberPaths(sample), MemberPaths(written));
    }

    private static string[] MemberPaths(JsonNode node)
    {
        List<string> paths = [];
        Walk(node, string.Empty, paths);
        return [.. paths.Distinct().Order(StringComparer.Ordinal)];
    }

    private static void Walk(JsonNode? node, string prefix, List<string> paths)
    {
        switch (node)
        {
            case JsonObject obj:
                foreach ((string name, JsonNode? child) in obj)
                {
                    paths.Add(prefix + "/" + name);
                    Walk(child, prefix + "/" + name, paths);
                }

                break;
            case JsonArray array:
                foreach (JsonNode? item in array)
                {
                    Walk(item, prefix + "[]", paths);
                }

                break;
            default:
                break;
        }
    }
}
