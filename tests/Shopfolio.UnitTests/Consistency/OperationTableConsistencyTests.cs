using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the decision table equal to 03 1.11 row by row (T323 criterion 8, KK1, KK8; 09 21.4 item 1): the row identities are read from the document's two tables - the
/// customer's under "**Müşteri — sipariş sayfası**", the administrator's under "**Yönetici — panel**" - and compared with <see cref="OrderOperationCatalog.Rows"/>, with the
/// side of each. The expected identities come from the document, not from the test; the failure case runs the same comparison on a changed copy.
/// </summary>
[Trait("Consistency", "true")]
public sealed class OperationTableConsistencyTests
{
    private static readonly string Document = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "03_USER_FLOWS.md")).ReplaceLineEndings("\n");

    /// <summary>The (identity, side) pairs of the document's section 1.11, in the document's order.</summary>
    private static List<(string Id, ActorKind Actor)> DocumentedRows(string document)
    {
        string[] lines = document.Split('\n');
        int start = Array.FindIndex(lines, l => l.StartsWith("### 1.11 ", StringComparison.Ordinal));
        Assert.True(start >= 0, "03 1.11 is not found.");

        List<(string, ActorKind)> rows = [];
        ActorKind? side = null;
        for (int i = start + 1; i < lines.Length && !lines[i].StartsWith("## ", StringComparison.Ordinal); i++)
        {
            if (lines[i].StartsWith("**Müşteri", StringComparison.Ordinal))
            {
                side = ActorKind.Buyer;
            }
            else if (lines[i].StartsWith("**Yönetici", StringComparison.Ordinal))
            {
                side = ActorKind.Admin;
            }
            else if (lines[i].StartsWith("| 1.11.", StringComparison.Ordinal))
            {
                Assert.NotNull(side);
                rows.Add((lines[i].Trim('|').Split('|')[0].Trim(), side.Value));
            }
        }

        return rows;
    }

    private static void AssertEqualToTheTable(List<(string Id, ActorKind Actor)> documented)
    {
        Assert.Equal(documented.Select(r => r.Id), OrderOperationCatalog.Rows.Select(r => r.Id));
        Assert.Equal(documented.Select(r => r.Actor), OrderOperationCatalog.Rows.Select(r => r.Actor));
    }

    [Fact]
    public void Rows_AreTheDocumentsFortyOne_CustomerFourteen_AdministratorTwentySeven()
    {
        List<(string Id, ActorKind Actor)> documented = DocumentedRows(Document);

        Assert.Equal(41, documented.Count);
        Assert.Equal(14, documented.Count(r => r.Actor == ActorKind.Buyer));
        Assert.Equal(27, documented.Count(r => r.Actor == ActorKind.Admin));
        AssertEqualToTheTable(documented);
    }

    [Fact]
    public void ADocumentRowMoved_ToTheOtherSide_IsCaught()
    {
        string changed = Document.Replace("**Yönetici — panel**", "**Müşteri — panel**", StringComparison.Ordinal);

        Assert.ThrowsAny<Exception>(() => AssertEqualToTheTable(DocumentedRows(changed)));
    }

    [Fact]
    public void ADocumentRowAdded_OrRemoved_IsCaught()
    {
        string added = Document.Replace("| 1.11.41 |", "| 1.11.42 |", StringComparison.Ordinal);
        string removed = Document.Replace("| 1.11.40 |", "| x |", StringComparison.Ordinal);

        Assert.ThrowsAny<Exception>(() => AssertEqualToTheTable(DocumentedRows(added)));
        Assert.ThrowsAny<Exception>(() => AssertEqualToTheTable(DocumentedRows(removed)));
    }
}
