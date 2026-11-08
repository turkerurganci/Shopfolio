using System.Globalization;
using System.Text.Json.Nodes;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Ordering;

/// <summary>
/// The stored form of the defect claim's status history (06 2.5.1, 4.30; 09 12 item 5 and item 8): the history is written by the one payload setting, and every
/// form ever written is still read - the sample files under <c>Payloads/DefectClaimHistoryEntry</c>, one per form; a new form adds a sample, never replaces one.
/// </summary>
public sealed class DefectClaimHistoryPayloadTests
{
    private static readonly DateTimeOffset At = new(2026, 11, 9, 10, 30, 0, TimeSpan.Zero);

    private static string SampleFolder => Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Payloads", "DefectClaimHistoryEntry");

    [Fact]
    public void TheHistory_IsWrittenByThePayloadSettingWithTheMembersNames_AndReadBack()
    {
        List<DefectClaimHistoryEntry> history = [new(At, DefectClaimStatus.Open, ActorKind.Buyer, 2)];

        string json = PayloadJson.Write(history);
        List<DefectClaimHistoryEntry>? back = PayloadJson.Read<List<DefectClaimHistoryEntry>>(json);

        Assert.Contains("\"At\":", json, StringComparison.Ordinal);
        Assert.Contains("\"Status\":\"Open\"", json, StringComparison.Ordinal);
        Assert.Contains("\"ActorKind\":\"Buyer\"", json, StringComparison.Ordinal);
        Assert.Contains("\"Quantity\":2", json, StringComparison.Ordinal);
        Assert.NotNull(back);
        Assert.Equal(history, back);
    }

    [Fact]
    public void EveryFormEverWritten_IsReadWithoutAnError_AndTheEmptyHistoryIsTheEmptyList()
    {
        string[] files = Directory.GetFiles(SampleFolder, "*.json");

        Assert.NotEmpty(files);
        Assert.Contains(files, f => Path.GetFileName(f) == "1.json");
        foreach (string file in files)
        {
            List<DefectClaimHistoryEntry>? read = PayloadJson.Read<List<DefectClaimHistoryEntry>>(File.ReadAllText(file));
            Assert.NotNull(read);
            Assert.NotEmpty(read);
            Assert.All(read, e => Assert.NotEqual(default, e.At));
        }

        List<DefectClaimHistoryEntry>? empty = PayloadJson.Read<List<DefectClaimHistoryEntry>>("[]");
        Assert.NotNull(empty);
        Assert.Empty(empty);
    }

    [Fact]
    public void TheFirstSample_ReadsTheValuesItHolds_AndTheRetentionTakesItsLatestMoment()
    {
        List<DefectClaimHistoryEntry>? read = PayloadJson.Read<List<DefectClaimHistoryEntry>>(File.ReadAllText(Path.Combine(SampleFolder, "1.json")));

        Assert.NotNull(read);
        Assert.Equal(
            [
                new DefectClaimHistoryEntry(new DateTimeOffset(2026, 11, 8, 9, 0, 0, TimeSpan.Zero), DefectClaimStatus.Resolved, ActorKind.Admin, null),
                new DefectClaimHistoryEntry(At, DefectClaimStatus.Open, ActorKind.Buyer, 2),
            ],
            read);
        Assert.Equal(At, DefectClaimRetention.LastChange(At.AddYears(-1), read));
    }

    [Fact]
    public void AFieldTheCodeDoesNotKnow_IsSkipped_AndAMissingMemberTakesItsDefault()
    {
        List<DefectClaimHistoryEntry>? read = PayloadJson.Read<List<DefectClaimHistoryEntry>>(
            "[{\"At\":\"2026-11-09T10:30:00+00:00\",\"Status\":\"Resolved\",\"ActorKind\":\"Admin\",\"FromALaterVersion\":{\"x\":1}}]");

        Assert.NotNull(read);
        DefectClaimHistoryEntry entry = Assert.Single(read);
        Assert.Equal(At, entry.At);
        Assert.Equal(DefectClaimStatus.Resolved, entry.Status);
        Assert.Equal(ActorKind.Admin, entry.ActorKind);
        Assert.Null(entry.Quantity);
    }

    [Fact]
    public void TheFormTheCodeWritesToday_HasTheMemberSetOfTheNewestSample()
    {
        string newest = Directory.GetFiles(SampleFolder, "*.json").OrderBy(f => int.Parse(Path.GetFileNameWithoutExtension(f), CultureInfo.InvariantCulture)).Last();
        List<DefectClaimHistoryEntry> full = [new(At, DefectClaimStatus.Open, ActorKind.Buyer, 2)];

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
