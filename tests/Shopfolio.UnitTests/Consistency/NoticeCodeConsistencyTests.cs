using System.Text.RegularExpressions;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// The notice code list equals the document in both directions (T214 criteria 1, 4, 5, 6; KK4-KK6; 09 8.2 item 1, 21.4 item 1): the members of
/// <see cref="NoticeCode"/> are the codes of the three tables of 07 6.3.1-6.3.3; the kind of a member is the table it stands in (the short-lived notices,
/// the result states and the reasons of a return are three lists); the names have the form of 07 6.1.1 and none is an error code; and the endpoint table
/// (<see cref="NoticeRoutes"/>) is the "Uçlar → hedef" column of 07 6.3.1. Reads repository files and the application assembly only. Runs in the Lint step.
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class NoticeCodeConsistencyTests
{
    private sealed record DocumentedCode(string Section, string Name, string EndpointCell);

    private static string Document() => File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "07_API_DESIGN.md")).ReplaceLineEndings("\n");

    private static string Section63(string text)
    {
        int start = text.IndexOf("### 6.3 Kısa süreli bildirim kodları", StringComparison.Ordinal);
        int end = text.IndexOf("### 6.4 ", StringComparison.Ordinal);
        Assert.True(start > 0 && end > start, "07 6.3 was not found.");
        return text[start..end];
    }

    /// <summary>The codes of the tables of 07 6.3.1-6.3.3, in order: a row may carry two codes (<c>`A` · `B`</c>), each is one documented code.</summary>
    private static List<DocumentedCode> DocumentedCodes(string text)
    {
        List<DocumentedCode> codes = [];
        string section = string.Empty;
        foreach (string line in Section63(text).Split('\n'))
        {
            Match head = SectionHead().Match(line);
            if (head.Success)
            {
                section = head.Groups[1].Value;
                continue;
            }

            Match row = CodeRow().Match(line);
            if (row.Success)
            {
                string endpoints = line.TrimEnd().TrimEnd('|').Split(" | ")[^1].Trim();
                foreach (Match name in CodeName().Matches(row.Groups[1].Value))
                {
                    codes.Add(new DocumentedCode(section, name.Groups[1].Value, endpoints));
                }
            }
        }

        return codes;
    }

    /// <summary>
    /// The (code, source, target) triples of the "Uçlar → hedef" column of 07 6.3.1. A cell is groups separated by <c>·</c>; a group is
    /// <c>sources → targets</c>; a source list may hold a range (<c>UC-168…UC-172</c>) and a parenthesis naming the handler. A row of two codes pairs its
    /// i-th code with the i-th source of its one group (<c>UC-155, UC-156 → UC-94</c>: closed, reopened).
    /// </summary>
    private static List<(string Code, string Source, string Target)> DocumentedRoutes(string text)
    {
        List<(string, string, string)> routes = [];
        List<DocumentedCode> shortLived = [.. DocumentedCodes(text).Where(c => c.Section == "6.3.1")];
        foreach (IGrouping<string, DocumentedCode> row in shortLived.GroupBy(c => c.EndpointCell))
        {
            DocumentedCode[] rowCodes = [.. row];
            foreach (string group in Parenthesis().Replace(rowCodes[0].EndpointCell, string.Empty).Split(" · "))
            {
                string[] sides = group.Split(" → ");
                Assert.Equal(2, sides.Length);
                List<string> sources = [.. sides[0].Split(',').Select(s => s.Trim()).SelectMany(Expand)];
                string[] targets = [.. sides[1].Split(',').Select(s => s.Trim())];
                for (int i = 0; i < sources.Count; i++)
                {
                    string code = rowCodes.Length > 1 ? rowCodes[i].Name : rowCodes[0].Name;
                    routes.AddRange(targets.Select(t => (code, sources[i], t)));
                }
            }
        }

        return routes;
    }

    private static IEnumerable<string> Expand(string source)
    {
        Match range = Range().Match(source);
        if (!range.Success)
        {
            return [source];
        }

        int from = int.Parse(range.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
        int to = int.Parse(range.Groups[2].Value, System.Globalization.CultureInfo.InvariantCulture);
        return Enumerable.Range(from, to - from + 1).Select(n => "UC-" + n.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    [Fact]
    public void Members_AreTheCodesOfTheTablesOf07_63_InBothDirections()
    {
        List<DocumentedCode> documented = DocumentedCodes(Document());

        Assert.True(documented.Count >= 20, "The tables of 07 6.3 were read incompletely.");
        Assert.Empty(SetComparison.Compare("NoticeCode", documented.Select(c => c.Name), Enum.GetNames<NoticeCode>()));
    }

    [Fact]
    public void EveryCode_IsInOneTableOnly_SoTheThreeListsAreSeparate_KK4()
    {
        List<DocumentedCode> documented = DocumentedCodes(Document());

        Assert.Empty(documented.GroupBy(c => c.Name, StringComparer.Ordinal).Where(g => g.Count() > 1).Select(g => g.Key));
        Assert.Equal(["6.3.1", "6.3.2", "6.3.3"], documented.Select(c => c.Section).Distinct().Order(StringComparer.Ordinal));
    }

    [Theory]
    [InlineData("6.3.1", NoticeKind.ShortLived)]
    [InlineData("6.3.2", NoticeKind.Result)]
    [InlineData("6.3.3", NoticeKind.Reason)]
    public void TheKindOfAMember_IsTheTableItsCodeStandsIn_InBothDirections_KK4(string section, NoticeKind kind)
    {
        string[] documented = [.. DocumentedCodes(Document()).Where(c => c.Section == section).Select(c => c.Name)];

        Assert.NotEmpty(documented);
        Assert.Empty(SetComparison.Compare($"NoticeKinds {kind}", documented, NoticeKinds.Members(kind).Select(c => c.ToString())));
    }

    [Fact]
    public void TheKindTable_HasEveryMember_AndNothingElse()
    {
        Assert.Empty(SetComparison.Compare(
            "NoticeKinds",
            Enum.GetValues<NoticeCode>().Select(c => c.ToString()),
            NoticeKinds.Codes.Select(c => c.ToString())));
    }

    [Fact]
    public void EveryName_IsEnglishPascalCase_NoTwoDifferOnlyInCase_AndTheOnlyErrorCodeNameIsTheSharedState_KK5()
    {
        string[] names = Enum.GetNames<NoticeCode>();

        Assert.All(names, name => Assert.Matches(PascalCase(), name));
        Assert.Equal(names.Length, names.Select(n => n.ToUpperInvariant()).Distinct(StringComparer.Ordinal).Count());
        // The one name both lists hold is the one state 07 6.3.3 calls "§6.2.7'nin aynı hâli": the error code of a panel refusal and the reason of a customer return.
        Assert.Equal([nameof(NoticeCode.QuantityExceedsOpen)], names.Intersect(Enum.GetNames<ErrorCode>(), StringComparer.OrdinalIgnoreCase));
    }

    [Fact]
    public void TheRouteTable_IsTheEndpointColumnOf07_631_InBothDirections_KK6()
    {
        HashSet<(string, string, string)> documented = [.. DocumentedRoutes(Document())];
        HashSet<(string, string, string)> table = [.. NoticeRoutes.All.Select(r => (r.Code.ToString(), r.Source, r.Target))];

        Assert.True(documented.Count >= 20, "The endpoint column of 07 6.3.1 was read incompletely.");
        Assert.Empty(SetComparison.Compare("NoticeRoutes", documented.Select(Describe), table.Select(Describe)));
        Assert.Equal(NoticeRoutes.All.Count, table.Count);
    }

    [Fact]
    public void TheFiveCodesOf07_631_RedirectFromTheirEndpointToTheirTarget_KK6()
    {
        Assert.Equal(["UC-04"], NoticeRoutes.TargetsOf("UC-05"));
        Assert.Equal(NoticeCode.CartItemAdded, NoticeRoutes.CodeOf("UC-05"));
        Assert.Equal(["UC-33", "UC-34"], NoticeRoutes.TargetsOf("UC-35"));
        Assert.Equal(NoticeCode.Saved, NoticeRoutes.CodeOf("UC-35"));
        Assert.Equal(["UC-103"], NoticeRoutes.TargetsOf("UC-175"));
        Assert.Equal(NoticeCode.InvitationSent, NoticeRoutes.CodeOf("UC-175"));
        Assert.Equal(["UC-94"], NoticeRoutes.TargetsOf("UC-155"));
        Assert.Equal(NoticeCode.ContactRequestClosed, NoticeRoutes.CodeOf("UC-155"));
        Assert.Equal(["UC-94"], NoticeRoutes.TargetsOf("UC-156"));
        Assert.Equal(NoticeCode.ContactRequestReopened, NoticeRoutes.CodeOf("UC-156"));
        Assert.Equal(
            Enum.GetNames<NoticeCode>().Where(n => NoticeKinds.Of(Enum.Parse<NoticeCode>(n)) == NoticeKind.ShortLived).Order(StringComparer.Ordinal),
            NoticeRoutes.All.Select(r => r.Code.ToString()).Distinct().Order(StringComparer.Ordinal));
    }

    [Fact]
    public void TheRouteCheck_SeesADroppedAndAnAddedEndpoint_InTheDocumentText()
    {
        string text = Document();
        string dropped = text.Replace(" · UC-173 → UC-102", string.Empty, StringComparison.Ordinal);
        string added = text.Replace("UC-175 → UC-103", "UC-175 → UC-103, UC-104", StringComparison.Ordinal);

        Assert.NotEqual(text, dropped);
        Assert.NotEqual(text, added);
        Assert.Contains(("Saved", "UC-173", "UC-102"), DocumentedRoutes(text).Except(DocumentedRoutes(dropped)));
        Assert.Contains(("InvitationSent", "UC-175", "UC-104"), DocumentedRoutes(added).Except(DocumentedRoutes(text)));
    }

    private static string Describe((string Code, string Source, string Target) route) => $"{route.Code}: {route.Source} -> {route.Target}";

    [GeneratedRegex(@"^\*\*(6\.3\.\d+) ", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SectionHead();

    [GeneratedRegex(@"^\| ((?:`[A-Za-z]+`)(?: · `[A-Za-z]+`)*) \|", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex CodeRow();

    [GeneratedRegex(@"`([A-Za-z]+)`", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex CodeName();

    [GeneratedRegex(@"^[A-Z][A-Za-z0-9]*$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex PascalCase();

    [GeneratedRegex(@"^UC-(\d+)…UC-(\d+)$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Range();

    [GeneratedRegex(@" \(`[A-Za-z]+`\)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Parenthesis();
}
