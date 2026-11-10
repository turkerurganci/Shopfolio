using System.Text.RegularExpressions;

using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The internal note never reaches the customer (T386 KK10; 02 10.4.7): the note's entity, set and table are named only by its entity file, the context, the writer
/// (<c>OrderCorrections.Interventions.cs</c>) and its two conditional statements (<c>SaveInternalNote.cs</c>, 09 10.9), the disposal part that deletes it, the rollback audit's table list and the migrations - no page, e-mail template, contract
/// text or reader of the order page can read it. The operation names that contain the word (<c>SaveInternalNote</c>, <c>WriteInternalNote</c>) are not the note.
/// The rule goes red on a deliberate violation sample in every form the scan claims to see and stays quiet on the allowed near-miss.
/// </summary>
public sealed partial class InternalNoteSourceScanTests
{
    private static readonly string[] Extensions = [".cs", ".cshtml", ".razor", ".js", ".html", ".txt", ".json", ".sql"];

    private static readonly HashSet<string> AllowedFiles = new(StringComparer.Ordinal)
    {
        "src/Shopfolio.Web/Modules/Ordering/InternalNote.cs",
        "src/Shopfolio.Web/Infrastructure/AppDbContext.cs",
        "src/Shopfolio.Web/Modules/Ordering/OrderCorrections.Interventions.cs",
        "src/Shopfolio.Web/Modules/Ordering/SaveInternalNote.cs",
        "src/Shopfolio.Web/Modules/Ordering/OrderDisposalPart.cs",
        "src/Shopfolio.Web/Infrastructure/Backup/RollbackAudit.cs",
    };

    [GeneratedRegex(@"\bInternalNotes?\b|\binternal_notes?\b|\bInternalNoteId\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex NoteName();

    [Fact]
    public void OnlyTheEntityTheContextTheWriterAndTheDisposal_NameTheInternalNote_KK10()
    {
        string root = RepositoryRoot.Find();
        List<string> scanned = [];
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(Path.Combine(root, "src"), "*.*", SearchOption.AllDirectories))
        {
            string relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            if (!Applies(relative))
            {
                continue;
            }

            scanned.Add(relative);
            if (!AllowedFiles.Contains(relative) && Names(File.ReadAllText(file)))
            {
                findings.Add(relative);
            }
        }

        // The scan sees the writer, the pages and the e-mail templates; every allowed file still exists.
        Assert.All(AllowedFiles, f => Assert.Contains(f, scanned));
        Assert.Contains(scanned, f => f.EndsWith(".cshtml", StringComparison.Ordinal));
        Assert.Empty(findings);
    }

    [Theory]
    [InlineData("<p>@Model.InternalNote</p>")]
    [InlineData("var notes = _db.InternalNotes.Where(n => n.OrderId == id);")]
    [InlineData("var note = _db.Set<InternalNote>().Find(id);")]
    [InlineData("SELECT text FROM internal_notes WHERE order_id = @id")]
    [InlineData("new { order.Id, InternalNoteId = note.Id }")]
    public void Scan_OfAReaderOfTheNote_IsAFinding(string code) => Assert.True(Names(code));

    [Theory]
    [InlineData("AuditOperation.SaveInternalNote")]
    [InlineData("OrderOperation.WriteInternalNote")]
    [InlineData("// the firm's note on the order")]
    public void Scan_OfTheOperationNames_IsQuiet(string code) => Assert.False(Names(code));

    [Theory]
    [InlineData("src/Shopfolio.Web/Infrastructure/Migrations/20261104051528_CreateCartAndOrderCoreTables.cs", false)]
    [InlineData("src/Shopfolio.Web/bin/Debug/net10.0/appsettings.json", false)]
    [InlineData("src/Shopfolio.Web/obj/Debug/net10.0/Shopfolio.Web.AssemblyInfo.cs", false)]
    [InlineData("src/Shopfolio.Web/Pages/Order/Index.cshtml", true)]
    [InlineData("src/Shopfolio.Web/Modules/Notifications/Templates/B11.txt", true)]
    public void Applies_SkipsOnlyTheMigrationsAndTheBuildOutput(string relative, bool expected) => Assert.Equal(expected, Applies(relative));

    private static bool Names(string text) => NoteName().IsMatch(text);

    private static bool Applies(string relative)
        => Extensions.Contains(Path.GetExtension(relative), StringComparer.OrdinalIgnoreCase)
            && !relative.Contains("/Infrastructure/Migrations/", StringComparison.Ordinal)
            && !relative.Contains("/bin/", StringComparison.Ordinal)
            && !relative.Contains("/obj/", StringComparison.Ordinal);
}
