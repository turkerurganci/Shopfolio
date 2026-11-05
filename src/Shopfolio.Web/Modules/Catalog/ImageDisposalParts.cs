using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The disposal part of the images (06 4.69, 9.1; 05 7.7.2; T223 criterion 9): an image with no link left and older than a day is deleted - the
/// row first, with its disposal record in the same transaction; the files come after, in <see cref="OrphanImageFilePart"/>. The links are every
/// foreign key that points to the image table, read from the model, so a table that gets a new image link is covered without a change here.
/// </summary>
internal sealed class ImageDisposalPart : IDisposalPart
{
    public string Name => "Images";

    public Task RunAsync(DisposalRun run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        DateTimeOffset cutoff = run.Now - TimeSpan.FromDays(DisposalTimings.UnlinkedFileMinAgeDays);
        return run.ChunksAsync(
            DisposalRecordType.Image,
            (db, max, cancel) => db.Database.ExecuteSqlRawAsync(DeleteStatement(db), [cutoff.ToUniversalTime(), max], cancel),
            ct);
    }

    /// <summary>The statement: the oldest unlinked images, at most the chunk's size. Names come from the model, values are parameters.</summary>
    internal static string DeleteStatement(DbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        IEntityType image = db.Model.FindEntityType(typeof(Image)) ?? throw new InvalidOperationException("The image entity is not in the model.");
        string table = Quote(image.GetTableName()!);
        string notLinked = string.Join(
            " AND ",
            image.GetReferencingForeignKeys().Select(fk =>
                $"NOT EXISTS (SELECT 1 FROM {Quote(fk.DeclaringEntityType.GetTableName()!)} l WHERE l.{Quote(fk.Properties[0].GetColumnName())} = i.id)"));
        return $"DELETE FROM {table} WHERE id IN (SELECT i.id FROM {table} i WHERE i.created_at < {{0}} AND {notLinked} ORDER BY i.created_at, i.id LIMIT {{1}})";
    }

    private static string Quote(string name) => "\"" + name.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
}

/// <summary>
/// The image files that no row names (05 7.7.1, 7.4.4; 06 9.3.4): a file of the image section older than a day whose source copy's key no
/// <see cref="Image"/> row carries - the source copy and its derivatives - is deleted. The disposal record is written before the files go (a
/// larger count is better than a missing record). A file younger than a day is not swept: its row may not be written yet (K-980).
/// </summary>
internal sealed class OrphanImageFilePart : IDisposalPart
{
    private readonly IFileStore _files;

    public OrphanImageFilePart(IFileStore files)
    {
        _files = files;
    }

    public string Name => "OrphanImageFiles";

    public async Task RunAsync(DisposalRun run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        List<string> chunk = [];
        await foreach (string key in _files.ListAsync(FileArea.Image, TimeSpan.FromDays(DisposalTimings.UnlinkedFileMinAgeDays), ct))
        {
            chunk.Add(key);
            if (chunk.Count == DisposalTimings.PartSize)
            {
                await SweepAsync(run, chunk, ct);
                chunk.Clear();
            }
        }

        if (chunk.Count > 0)
        {
            await SweepAsync(run, chunk, ct);
        }
    }

    private async Task SweepAsync(DisposalRun run, List<string> keys, CancellationToken ct)
    {
        string[] sourceKeys = [.. keys.Select(FileKeys.SourceKeyOf).Distinct(StringComparer.Ordinal)];
        HashSet<string> named = [.. await run.Db.Images.AsNoTracking().Where(i => sourceKeys.Contains(i.StorageKey)).Select(i => i.StorageKey).ToListAsync(ct)];
        string[] orphans = [.. keys.Where(k => !named.Contains(FileKeys.SourceKeyOf(k)))];
        if (orphans.Length == 0)
        {
            return;
        }

        await run.RecordAsync(DisposalRecord.Counted(DisposalRecordType.OrphanFile, orphans.Length, run.Now), ct);
        foreach (string key in orphans)
        {
            ct.ThrowIfCancellationRequested();
            await _files.DeleteAsync(FileArea.Image, key, ct);
        }
    }
}
