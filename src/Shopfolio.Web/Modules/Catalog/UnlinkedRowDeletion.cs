using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The deletion statement of a file row that nothing links to (06 4.69, 4.70; 05 7.7.2, 7.7.3): the oldest rows older than the edge that no foreign key of the model
/// points to, at most the chunk's size. The links are every foreign key that points to the row's table, read from the model, so a table that gets a new link is
/// covered without a change here. Names come from the model, values are parameters.
/// </summary>
internal static class UnlinkedRowDeletion
{
    /// <summary>
    /// The statement for <paramref name="entityType"/> (<c>{0}</c> is the edge, <c>{1}</c> the chunk's size). <paramref name="extraCondition"/>, when given, is
    /// ANDed to the age and link conditions; it receives a function that turns a property name into the quoted column of the row alias.
    /// </summary>
    public static string Statement(DbContext db, Type entityType, string idProperty, string createdAtProperty, Func<Func<string, string>, string>? extraCondition)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(entityType);
        IEntityType entity = db.Model.FindEntityType(entityType) ?? throw new InvalidOperationException($"The {entityType.Name} entity is not in the model.");
        StoreObjectIdentifier entityTable = StoreObjectIdentifier.Create(entity, StoreObjectType.Table)
            ?? throw new InvalidOperationException($"The {entityType.Name} entity is not mapped to a table.");
        string table = SqlIdentifier(entityTable.Name);
        string id = SqlIdentifier(ColumnOf(entity, idProperty, entityTable));
        string createdAt = SqlIdentifier(ColumnOf(entity, createdAtProperty, entityTable));
        string notLinked = string.Join(
            " AND ",
            entity.GetReferencingForeignKeys().Select(fk =>
            {
                StoreObjectIdentifier linkTable = StoreObjectIdentifier.Create(fk.DeclaringEntityType, StoreObjectType.Table)
                    ?? throw new InvalidOperationException($"A table that links to the {entityType.Name} table has no store object.");
                return $"NOT EXISTS (SELECT 1 FROM {SqlIdentifier(linkTable.Name)} l WHERE l.{SqlIdentifier(fk.Properties[0].GetColumnName(linkTable)!)} = i.{id})"; // non-null: a foreign key's column is mapped to the table that declares the key
            }));
        string extra = extraCondition == null
            ? string.Empty
            : " AND " + extraCondition(property => "i." + SqlIdentifier(ColumnOf(entity, property, entityTable)));
        string links = notLinked.Length == 0 ? string.Empty : " AND " + notLinked;
        return $"DELETE FROM {table} WHERE {id} IN (SELECT i.{id} FROM {table} i WHERE i.{createdAt} < {{0}}{extra}{links} ORDER BY i.{createdAt}, i.{id} LIMIT {{1}})";
    }

    private static string ColumnOf(IEntityType entity, string property, StoreObjectIdentifier table) =>
        entity.FindProperty(property)?.GetColumnName(table) ?? throw new InvalidOperationException($"The {entity.ClrType.Name} entity has no column for {property}.");

    private static string SqlIdentifier(string name) => "\"" + name.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";
}

/// <summary>
/// The files of one file section that no row names (05 7.7.1, 7.4.4; 06 9.3.4): a file older than a day whose row key no row carries is deleted. The disposal
/// record is written before the files go (a larger count is better than a missing record). A file younger than a day is not swept: its row may not be written
/// yet (K-980).
/// </summary>
internal abstract class OrphanFilePart : IDisposalPart
{
    private readonly IFileStore _files;

    protected OrphanFilePart(IFileStore files)
    {
        _files = files;
    }

    public abstract string Name { get; }

    protected abstract FileArea Area { get; }

    /// <summary>The key a row of the section would carry for a file key: the key itself, or the source copy's key for an image derivative.</summary>
    protected virtual string RowKeyOf(string fileKey) => fileKey;

    /// <summary>Which of the row keys some row of the section carries.</summary>
    protected abstract Task<HashSet<string>> NamedAsync(AppDbContext db, string[] rowKeys, CancellationToken ct);

    public async Task RunAsync(DisposalRun run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        List<string> chunk = [];
        await foreach (string key in _files.ListAsync(Area, TimeSpan.FromDays(DisposalTimings.UnlinkedFileMinAgeDays), ct))
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
        string[] rowKeys = [.. keys.Select(RowKeyOf).Distinct(StringComparer.Ordinal)];
        HashSet<string> named = await NamedAsync(run.Db, rowKeys, ct);
        string[] orphans = [.. keys.Where(k => !named.Contains(RowKeyOf(k)))];
        if (orphans.Length == 0)
        {
            return;
        }

        await run.RecordAsync(DisposalRecord.Counted(DisposalRecordType.OrphanFile, orphans.Length, run.Now), ct);
        foreach (string key in orphans)
        {
            ct.ThrowIfCancellationRequested();
            await _files.DeleteAsync(Area, key, ct);
        }
    }
}
