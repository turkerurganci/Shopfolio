using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The upload that never completed (06 4.70, 9.1; 05 7.7.3): a digital file row whose chunked upload has no completion moment and is older than a day goes in the
/// nightly clean-up. It is a temporary record that carries no personal data, so it writes no disposal row (06 9.3.1: only the records of personal data are counted by
/// type); its file, if the store has one, goes in <see cref="OrphanDigitalFilePart"/> the next time the store is swept.
/// </summary>
internal sealed class IncompleteDigitalUploadPart : IDisposalPart
{
    public string Name => "IncompleteDigitalUploads";

    public Task RunAsync(DisposalRun run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        DateTimeOffset cutoff = run.Now - TimeSpan.FromDays(DisposalTimings.UnlinkedFileMinAgeDays);
        return run.ChunksWithoutRecordAsync((db, max, cancel) => DeleteChunkAsync(db, cutoff, max, cancel), ct);
    }

    private static async Task<int> DeleteChunkAsync(AppDbContext db, DateTimeOffset cutoff, int max, CancellationToken ct)
    {
        db.ChangeTracker.Clear();
        List<Guid> ids = await db.DigitalFiles
            .Where(d => d.UploadCompletedAt == null && d.CreatedAt < cutoff)
            .OrderBy(d => d.CreatedAt)
            .ThenBy(d => d.Id)
            .Select(d => d.Id)
            .Take(max)
            .ToListAsync(ct);
        return await db.DigitalFiles.Where(d => ids.Contains(d.Id)).ExecuteDeleteAsync(ct);
    }
}

/// <summary>
/// The disposal part of the digital product files (06 4.70, 9.1; 05 7.7.3; K-1029 (3), (4)): a file whose upload completed, older than a day, that was removed or
/// replaced, or never attached (its product and variant links empty, including when the owner was deleted), and that no order item links to, is deleted - the row
/// first, with its disposal record in the same transaction; the file comes after, in <see cref="OrphanDigitalFilePart"/>. The links are every foreign key that
/// points to the digital file table, read from the model, so a table that gets a new link is covered without a change here.
/// </summary>
internal sealed class DigitalFileDisposalPart : IDisposalPart
{
    public string Name => "DigitalFiles";

    public Task RunAsync(DisposalRun run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        DateTimeOffset cutoff = run.Now - TimeSpan.FromDays(DisposalTimings.UnlinkedFileMinAgeDays);
        return run.ChunksAsync(
            DisposalRecordType.DigitalFile,
            (db, max, cancel) => db.Database.ExecuteSqlRawAsync(DeleteStatement(db), [cutoff.ToUniversalTime(), max], cancel),
            ct);
    }

    /// <summary>The statement: the oldest unlinked digital files that are due, at most the chunk's size. Names come from the model, values are parameters.</summary>
    internal static string DeleteStatement(DbContext db) =>
        UnlinkedRowDeletion.Statement(
            db,
            typeof(DigitalFile),
            nameof(DigitalFile.Id),
            nameof(DigitalFile.CreatedAt),
            column => $"{column(nameof(DigitalFile.UploadCompletedAt))} IS NOT NULL AND ({column(nameof(DigitalFile.DetachedAt))} IS NOT NULL "
                + $"OR ({column(nameof(DigitalFile.ProductId))} IS NULL AND {column(nameof(DigitalFile.VariantId))} IS NULL))");
}

/// <summary>The digital product files that no row names (05 7.7.1, 7.7.3; 06 9.3.4): a file of the digital section older than a day whose key no row carries is deleted.</summary>
internal sealed class OrphanDigitalFilePart : OrphanFilePart
{
    public OrphanDigitalFilePart(IFileStore files)
        : base(files)
    {
    }

    public override string Name => "OrphanDigitalFiles";

    protected override FileArea Area => FileArea.DigitalFile;

    protected override async Task<HashSet<string>> NamedAsync(AppDbContext db, string[] rowKeys, CancellationToken ct) =>
        [.. await db.DigitalFiles.AsNoTracking().Where(d => rowKeys.Contains(d.StorageKey)).Select(d => d.StorageKey).ToListAsync(ct)];
}
