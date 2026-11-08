using Microsoft.EntityFrameworkCore;

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
    internal static string DeleteStatement(DbContext db) =>
        UnlinkedRowDeletion.Statement(db, typeof(Image), nameof(Image.Id), nameof(Image.CreatedAt), null);
}

/// <summary>
/// The image files that no row names (05 7.7.1, 7.4.4; 06 9.3.4): a file of the image section older than a day whose source copy's key no
/// <see cref="Image"/> row carries - the source copy and its derivatives - is deleted.
/// </summary>
internal sealed class OrphanImageFilePart : OrphanFilePart
{
    public OrphanImageFilePart(IFileStore files)
        : base(files)
    {
    }

    public override string Name => "OrphanImageFiles";

    protected override FileArea Area => FileArea.Image;

    protected override string RowKeyOf(string fileKey) => FileKeys.SourceKeyOf(fileKey);

    protected override async Task<HashSet<string>> NamedAsync(AppDbContext db, string[] rowKeys, CancellationToken ct) =>
        [.. await db.Images.AsNoTracking().Where(i => rowKeys.Contains(i.StorageKey)).Select(i => i.StorageKey).ToListAsync(ct)];
}
