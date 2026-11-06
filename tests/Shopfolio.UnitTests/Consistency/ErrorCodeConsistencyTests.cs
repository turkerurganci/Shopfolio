using System.Text.RegularExpressions;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// The error code registry equals the document in both directions (T213 criteria 1-5, 7; KK1-KK5, KK7; 09 8.2 items 1 and 5, 21.4 item 1): the members of
/// <see cref="ErrorCode"/> are the codes of the tables of 07 6.2.1-6.2.9 and <c>OutcomeUncertain</c>; the status table is the "HTTP" column; a form-specific rule
/// has its own code in its endpoint table; the field rule codes are members of the registry; and every row of 02 6 is covered by a code or by the table of
/// 07 6.2.10 (07 6.1.5 - the script of the document, run here). Reads repository files and the application assembly only. Runs in the Lint step.
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class ErrorCodeConsistencyTests
{
    private sealed record DocumentedCode(string Section, string Name, int[] Statuses);

    private static string Document(string name) => File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", name)).ReplaceLineEndings("\n");

    /// <summary>The rows of the tables of 07 6.2.1-6.2.9: section, code and the statuses of the "HTTP" column (<c>422 · 303</c> is two).</summary>
    private static List<DocumentedCode> DocumentedCodes()
    {
        string text = Document("07_API_DESIGN.md");
        int start = text.IndexOf("### 6.2 Hata kodları", StringComparison.Ordinal);
        int end = text.IndexOf("**6.2.10 ", StringComparison.Ordinal);
        Assert.True(start > 0 && end > start, "The tables of 07 6.2 were not found.");

        List<DocumentedCode> codes = [];
        string section = string.Empty;
        foreach (string line in text[start..end].Split('\n'))
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
                int[] statuses = [.. row.Groups[2].Value.Split('·').Select(s => int.Parse(s.Trim(), System.Globalization.CultureInfo.InvariantCulture))];
                codes.Add(new DocumentedCode(section, row.Groups[1].Value, statuses));
            }
        }

        return codes;
    }

    [Fact]
    public void Members_AreTheCodesOf07_62_AndOutcomeUncertain_InBothDirections()
    {
        List<DocumentedCode> documented = DocumentedCodes();

        Assert.True(documented.Count > 100, "The tables of 07 6.2 were read incompletely.");
        Assert.Empty(SetComparison.Compare(
            "ErrorCode",
            documented.Select(c => c.Name).Append(nameof(ErrorCode.OutcomeUncertain)),
            Enum.GetNames<ErrorCode>()));
    }

    [Fact]
    public void EveryCodeOf07_62_IsInOneTableOnly_SoASharedStateHasOneCode_KK3()
    {
        List<DocumentedCode> documented = DocumentedCodes();

        string[] repeated = [.. documented.GroupBy(c => c.Name, StringComparer.Ordinal).Where(g => g.Count() > 1).Select(g => g.Key)];

        Assert.Empty(repeated);
        Assert.Equal(documented.Count, documented.Select(c => c.Name.ToUpperInvariant()).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void EveryName_IsEnglishPascalCase_AndNoTwoDifferOnlyInCase_KK1()
    {
        string[] names = Enum.GetNames<ErrorCode>();

        Assert.All(names, name => Assert.Matches(PascalCase(), name));
        Assert.Equal(names.Length, names.Select(n => n.ToUpperInvariant()).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void StatusTable_HasEveryMember_AndNothingElse()
    {
        Assert.Empty(SetComparison.Compare(
            "ErrorStatuses",
            Enum.GetValues<ErrorCode>().Select(c => c.ToString()),
            ErrorStatuses.Codes.Select(c => c.ToString())));
    }

    [Fact]
    public void StatusTable_IsTheHttpColumnOf07_ForEveryCode_KK4()
    {
        List<DocumentedCode> documented = DocumentedCodes();

        foreach (DocumentedCode code in documented)
        {
            ErrorCode member = Enum.Parse<ErrorCode>(code.Name);
            Assert.True(code.Statuses.SequenceEqual(ErrorStatuses.AllOf(member)), $"{code.Name}: the document says {string.Join(" · ", code.Statuses)}, the table {string.Join(" · ", ErrorStatuses.AllOf(member))}.");
            Assert.Equal(code.Statuses[0], ErrorStatuses.Of(member));
        }
    }

    [Fact]
    public void OutcomeUncertain_IsA303_AsThe064_2_SaysAndOnlyTheLogCarriesIt()
    {
        string text = Document("07_API_DESIGN.md");
        int start = text.IndexOf("**6.4.2 Sonucu belirsiz işlem**", StringComparison.Ordinal);
        string paragraph = text[start..text.IndexOf("\n\n", start, StringComparison.Ordinal)];

        Assert.Contains("yanıt 303 ile sonucu gösteren ekrandır", paragraph, StringComparison.Ordinal);
        Assert.Contains("koduyla yazar", paragraph, StringComparison.Ordinal);
        Assert.Equal([Shopfolio.Infrastructure.Forms.FormResults.SeeOtherStatus], ErrorStatuses.AllOf(ErrorCode.OutcomeUncertain));
        Assert.DoesNotContain(DocumentedCodes(), c => c.Name == nameof(ErrorCode.OutcomeUncertain));
    }

    [Fact]
    public void FieldRuleCodes_AreTheCodesOf07_622_AndMembersOfTheRegistry_KK5()
    {
        string[] fieldTable = [.. DocumentedCodes().Where(c => c.Section == "6.2.2").Select(c => c.Name)];

        Assert.Empty(SetComparison.Compare("FieldCodes", fieldTable, FieldCodes.All));
        Assert.All(FieldCodes.All, code => Assert.True(Enum.TryParse(code, out ErrorCode _), $"{code} is no member of ErrorCode."));
    }

    [Theory]
    [InlineData("DuplicateSku", "6.2.6")]
    [InlineData("DiscountedPriceNotBelowReference", "6.2.6")]
    [InlineData("DuplicateCouponCode", "6.2.6")]
    [InlineData("EmailAlreadyRegistered", "6.2.5")]
    [InlineData("DeliveryProvinceNotServed", "6.2.3")]
    public void AFormSpecificRule_HasItsOwnCode_InTheTableOfItsEndpoint_AndIsNoFieldRuleCode_KK5(string code, string section)
    {
        DocumentedCode row = Assert.Single(DocumentedCodes(), c => c.Name == code);

        Assert.Equal(section, row.Section);
        Assert.DoesNotContain(code, FieldCodes.All);
        Assert.True(Enum.IsDefined(Enum.Parse<ErrorCode>(code)));
    }

    [Fact]
    public void EveryRowOf02_6_IsInTheMeaningOfACodeOrInTheTableOfNonErrorRows_KK7()
    {
        string[] uncovered = [.. RowsOf02().Except(RowsCoveredBy07(), StringComparer.Ordinal)];

        Assert.True(RowsOf02().Count > 100, "The rows of 02 6 were read incompletely.");
        Assert.Empty(uncovered);
    }

    [Fact]
    public void TheCoverageCheck_SeesAMissingRow_WhenACodeRowIsDropped()
    {
        string text = Document("07_API_DESIGN.md");
        int start = text.IndexOf("## 6. Hata kodları", StringComparison.Ordinal);
        int end = text.IndexOf("\n## 7. ", start, StringComparison.Ordinal);
        string section = text[start..end];
        string mutated = section.Replace("(`02 §6.2.6`; `03 §3.2.1.6`)", "(`03 §3.2.1.6`)", StringComparison.Ordinal);

        Assert.NotEqual(section, mutated);
        Assert.Contains("6.2.6", RowsOf02().Except(CoveredIn(mutated), StringComparer.Ordinal));
    }

    // The script of 07 6.1.5: the rows of 02 6 ("| 6.x.y |") against the references 07 6 makes to them.
    private static List<string> RowsOf02()
    {
        string text = Document("02_PRODUCT_REQUIREMENTS.md");
        int start = text.IndexOf("\n## 6. ", StringComparison.Ordinal);
        int end = text.IndexOf("\n## 7. ", start + 1, StringComparison.Ordinal);
        Assert.True(start > 0 && end > start, "Section 6 of 02 was not found.");
        return [.. SixRow().Matches(text[start..end]).Select(m => m.Groups[1].Value).Distinct(StringComparer.Ordinal)];
    }

    private static HashSet<string> RowsCoveredBy07()
    {
        string text = Document("07_API_DESIGN.md");
        int start = text.IndexOf("\n## 6. Hata kodları", StringComparison.Ordinal);
        int end = text.IndexOf("\n## 7. ", start + 1, StringComparison.Ordinal);
        Assert.True(start > 0 && end > start, "Section 6 of 07 was not found.");
        return CoveredIn(text[start..end]);
    }

    private static HashSet<string> CoveredIn(string section)
    {
        HashSet<string> rows = new(StringComparer.Ordinal);
        foreach (Match reference in Reference().Matches(section))
        {
            foreach (Match row in SixId().Matches(reference.Value))
            {
                rows.Add(row.Groups[1].Value);
            }
        }

        return rows;
    }

    [GeneratedRegex(@"^\*\*(6\.2\.\d+) ", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SectionHead();

    [GeneratedRegex(@"^\| `([A-Za-z]+)` \| ([0-9]+(?: · [0-9]+)*) \|", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex CodeRow();

    [GeneratedRegex(@"^[A-Z][A-Za-z0-9]*$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex PascalCase();

    [GeneratedRegex(@"^\| (6\.\d+\.\d+) ", RegexOptions.CultureInvariant | RegexOptions.Multiline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SixRow();

    // `02 §6.2.6`, `02 §3.31.2`, §6.7.2, §6.7.14 ... and the last cell of the 6.2.10 table: "| 6.2.1, 6.2.2 |" at the end of a line.
    [GeneratedRegex(@"02 §\d+(?:\.\d+)*`?(?:, §\d+(?:\.\d+)*)*|\| 6\.\d+\.\d+(?:, 6\.\d+\.\d+)* \|$", RegexOptions.CultureInvariant | RegexOptions.Multiline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Reference();

    [GeneratedRegex(@"(?:§| )(6\.\d+\.\d+)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SixId();
}
