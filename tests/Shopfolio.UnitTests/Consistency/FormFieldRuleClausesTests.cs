using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// The whole rule cell of a field row is read (T547 correction 5, F1; 04 7 introduction; 09 21.4 item 1): a clause in plain words, a reference to <c>02</c> or <c>03</c>, a decision or a flow is a kind of check the
/// code must make, and a rule text the table does not hold is reported. The checks that must fail read a changed copy of the document or of the sources, never the files (09 19.2 item 3).
/// </summary>
[Trait("Consistency", "true")]
public sealed class FormFieldRuleClausesTests
{
    private static readonly Lazy<string> Document = new(StateRoleDocument.Load);

    private static readonly Lazy<IReadOnlyDictionary<string, string>> Sources = new(() => NavigationSites.Load(RepositoryRoot.Find()));

    private static string Changed(string row, string from, string to)
    {
        string[] lines = Document.Value.Split('\n');
        int at = Array.FindIndex(lines, l => l.StartsWith($"| {row} |", StringComparison.Ordinal));
        Assert.True(at >= 0, row);
        string[] cells = lines[at].Split('|');
        if (from.Length == 0)
        {
            cells[4] = $" {to} ";
        }
        else
        {
            Assert.Contains(from, cells[4], StringComparison.Ordinal);
            cells[4] = cells[4].Replace(from, to, StringComparison.Ordinal);
        }

        lines[at] = string.Join('|', cells);
        return string.Join('\n', lines);
    }

    [Fact]
    public void Clauses_AreSplitAtTheSemicolonAndTheDashOutsideTheParentheses()
    {
        Assert.Equal(["Tutar (7.3.2.8)", "alt değer üst değerden büyük olamaz", "süzgeç uygulanmaz (5.2.7; OB-12)"], FormFieldRuleClauses.Split("Tutar (7.3.2.8); alt değer üst değerden büyük olamaz — süzgeç uygulanmaz (5.2.7; OB-12)"));
        Assert.Equal(["Kapalı liste (`02 §7.3.4`; K-509)".Replace("`", string.Empty, StringComparison.Ordinal)], FormFieldRuleClauses.Split("Kapalı liste (`02 §7.3.4`; K-509)"));
    }

    [Fact]
    public void References_CarryTheDocumentOfTheLastSectionNamedBeforeThem()
    {
        Assert.Equal(["02§3.6.2", "02§3.6.7"], FormFieldRuleClauses.Atoms("Tam sayı; ayrılmış adedin altına inemez (`02 §3.6.2`, §3.6.7)"));
        Assert.Equal(["03§1.11.5", "03§1.11.6", "K-794", "OB-15"], FormFieldRuleClauses.Atoms("Yalnız caymaya açık kalemler (`03 §1.11.5`, §1.11.6; K-794; OB-15)"));
        Assert.Empty(FormFieldRuleClauses.Atoms("Ölçek K-1013'tedir"));
    }

    [Fact]
    public void RealDocumentAndCode_HaveNoRuleCellProblems()
    {
        string[] problems = [.. FormFieldCheck.Problems(Document.Value, Sources.Value).Where(p => p.StartsWith("RuleClause", StringComparison.Ordinal))];
        Assert.Empty(problems);
    }

    // A rule text the table does not hold: a new clause, a changed reference, a changed sentence.
    [Theory]
    [InlineData("7.1.3.1", "Ad (7.3.2.3)", "Ad (7.3.2.3); yeni kural cümlesi")]
    [InlineData("7.1.5.4", "K-680", "K-681")]
    [InlineData("7.2.16.3", "§3.11.6", "§3.11.7")]
    [InlineData("7.2.4.9", "ayrılmış adedin altına inemez", "ayrılmış adedin altına inebilir")]
    [InlineData("7.2.3.5", " (K-822)", "")]
    public void RuleText_NotInTheClauseTable_IsReportedOnItsOwnRow(string row, string from, string to)
    {
        Assert.Contains(FormFieldCheck.Problems(Changed(row, from, to), Sources.Value), p => p.StartsWith($"RuleClauseUnknown:{row}:", StringComparison.Ordinal));
    }

    // A clause that leaves the cell: its lines are of a kind the row no longer names; a cell turned into a dash drops every kind.
    [Theory]
    [InlineData("7.2.10.4", "; işaretsizse kayıt IBAN'sız (`02 §10.4.10`)", "")]
    [InlineData("7.2.13.2", "", "—")]
    public void ClauseLeavingTheCell_LeavesItsLinesWithoutAKindOfTheRow(string row, string from, string to)
    {
        Assert.Contains(FormFieldCheck.Problems(Changed(row, from, to), Sources.Value), p => p.StartsWith($"RuleMissesAttribute:{row}:", StringComparison.Ordinal));
    }

    // A clause the table holds that no cell has.
    [Fact]
    public void ClauseOfTheTable_NoCellHas_IsReported()
    {
        Assert.Contains(FormFieldCheck.Problems(Changed("7.2.18.4", "`02 §3.29.6`", "—"), Sources.Value), p => p.StartsWith("RuleClauseNotInDocument:", StringComparison.Ordinal) || p.StartsWith("RuleMissesAttribute:7.2.18.4", StringComparison.Ordinal));
    }

    // The references and the clauses are demanded of the row: every clause with a kind and every reference has a line, and every line's kind is named by the row.
    [Fact]
    public void EveryClauseKindAndEveryReference_OfTheTable_HasALineOfItsRow()
    {
        string document = Document.Value;
        List<string> problems = [.. FormFieldCheck.Problems(document, Sources.Value)];
        Assert.DoesNotContain(problems, p => p.StartsWith("RuleWithoutEnforcement:", StringComparison.Ordinal) || p.StartsWith("RuleMissesAttribute:", StringComparison.Ordinal));
    }
}
