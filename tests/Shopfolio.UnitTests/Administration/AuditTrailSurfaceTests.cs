using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Administration;

/// <summary>
/// The surface and the stored form of the audit trail (T380 KK2, KK6, KK8; 09 9.2 items 1 and 2; 09 12 item 5): the list has a date range and an administrator filter and
/// nothing else - no search, no grouping, no export; the trail has no method that changes or removes a row; the details are written by the one payload setting and
/// every form ever written is still read (the sample files).
/// </summary>
public sealed class AuditTrailSurfaceTests
{
    // ---- KK8: the list ----

    [Fact]
    public void TheListQuery_HasTheDateRangeTheAdministratorAndThePageOnly()
    {
        string[] members = [.. typeof(AuditQuery).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.Name != "EqualityContract").Select(p => p.Name).Order(StringComparer.Ordinal)];

        Assert.Equal(["AdminId", "From", "Page", "To"], members);
    }

    [Fact]
    public void TheTrailInterface_WritesAndReads_AndHasNoSearchNoGroupingNoExportNoUpdateNoDelete()
    {
        string[] methods = [.. typeof(IAuditTrail).GetMethods().Select(m => m.Name).Order(StringComparer.Ordinal)];

        Assert.Equal(["ListAdminsAsync", "ListAsync", "WriteAsync"], methods);
        string[] forbidden = ["Search", "Group", "Export", "Csv", "Update", "Delete", "Remove", "Correct", "Edit", "Purge", "Truncate"];
        Assert.DoesNotContain(methods, m => forbidden.Any(f => m.Contains(f, StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void TheListRow_CarriesWhoWhenAndWhatChanged()
    {
        string[] members = [.. typeof(AuditRow).GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.Name != "EqualityContract").Select(p => p.Name).Order(StringComparer.Ordinal)];

        Assert.Equal(["AdminEmail", "AdminId", "AdminName", "Area", "At", "Details", "Id", "Operation", "TargetRecordId", "TargetRecordType"], members);
    }

    // ---- KK6: the entity of the row cannot be changed ----

    [Fact]
    public void TheRowEntity_HasNoPublicSetter_AndTheOneWayToMakeIsCreate()
    {
        Type entity = typeof(Shopfolio.Modules.Administration.AuditLog);

        Assert.All(entity.GetProperties(BindingFlags.Public | BindingFlags.Instance), p => Assert.True(p.SetMethod is null || !p.SetMethod.IsPublic, p.Name));
        Assert.NotNull(entity.GetMethod("Create", BindingFlags.Public | BindingFlags.Static));
    }

    // ---- The payload form ----

    [Fact]
    public void TheDetails_AreWrittenByThePayloadSettingWithTheMembersNames_AndReadBack()
    {
        AuditDetails details = new("Firma", [new AuditValueChange("MaxPreparationDays", "3", "5")], [new AuditValue("Reason", "x")]);

        string json = PayloadJson.Write(details);
        AuditDetails? back = PayloadJson.Read<AuditDetails>(json);

        Assert.Contains("\"DisplayName\":\"Firma\"", json, StringComparison.Ordinal);
        Assert.Contains("\"OldValue\":\"3\"", json, StringComparison.Ordinal);
        Assert.NotNull(back);
        Assert.Equal("Firma", back.DisplayName);
        Assert.Equal(new AuditValueChange("MaxPreparationDays", "3", "5"), Assert.Single(back.Changes));
        Assert.Equal(new AuditValue("Reason", "x"), Assert.Single(back.Values));
    }

    [Fact]
    public void EveryFormEverWritten_IsReadWithoutAnError_AndAMissingMemberTakesItsDefault()
    {
        string folder = Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Payloads", "AuditDetails");
        string[] files = Directory.GetFiles(folder, "*.json");

        Assert.NotEmpty(files);
        Assert.Contains(files, f => Path.GetFileName(f) == "1.json");
        foreach (string file in files)
        {
            AuditDetails? read = PayloadJson.Read<AuditDetails>(File.ReadAllText(file));
            Assert.NotNull(read);
            Assert.NotNull(read.Changes);
            Assert.NotNull(read.Values);
        }

        AuditDetails? empty = PayloadJson.Read<AuditDetails>("{}");
        Assert.NotNull(empty);
        Assert.Null(empty.DisplayName);
        Assert.Empty(empty.Changes);
        Assert.Empty(empty.Values);
    }

    [Fact]
    public void AFieldTheCodeDoesNotKnow_IsSkipped()
    {
        AuditDetails? read = PayloadJson.Read<AuditDetails>("{\"DisplayName\":\"A\",\"FromALaterVersion\":{\"x\":1},\"Changes\":[{\"Field\":\"F\",\"OldValue\":\"1\",\"NewValue\":\"2\",\"Later\":true}]}");

        Assert.NotNull(read);
        Assert.Equal("A", read.DisplayName);
        Assert.Equal("F", Assert.Single(read.Changes).Field);
    }

    [Fact]
    public void TheFormTheCodeWritesToday_HasTheMemberSetOfTheNewestSample()
    {
        string folder = Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Payloads", "AuditDetails");
        string newest = Directory.GetFiles(folder, "*.json").OrderBy(f => int.Parse(Path.GetFileNameWithoutExtension(f), System.Globalization.CultureInfo.InvariantCulture)).Last();
        AuditDetails full = new("A", [new AuditValueChange("F", "1", "2")], [new AuditValue("N", "V")]);

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

    [Fact]
    public void ThePayloadSetting_IsTheNamedOneOfTheSpecification()
    {
        Assert.Null(PayloadJson.Options.PropertyNamingPolicy);
        Assert.Equal(System.Text.Json.Serialization.JsonUnmappedMemberHandling.Skip, PayloadJson.Options.UnmappedMemberHandling);
        Assert.Contains(PayloadJson.Options.Converters, c => c is System.Text.Json.Serialization.JsonStringEnumConverter);
        Assert.IsType<JsonSerializerOptions>(PayloadJson.Options);
    }
}
