using System.Text.RegularExpressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

using Shopfolio.Infrastructure;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>A foreign key of the model: the table's entity, the column's property, the target entity, whether it is required, whether it is one-to-one, and its delete behavior as the model names it.</summary>
internal sealed record ForeignKeyFact(string Table, string Field, string Target, bool Required, bool Unique, string Behavior)
{
    /// <summary>The line of the foreign key without the cardinality, as the table of <c>06 7.2</c> states it.</summary>
    public string Key => $"{Table}.{Field} -> {Target} | {(Required ? "required" : "optional")} | {Behavior}";

    /// <summary>The line of the foreign key as a diagram line of <c>06 6</c> states it.</summary>
    public string DiagramKey => $"{Key} | {(Unique ? "one" : "many")}";
}

/// <summary>An index: the table's entity, the columns in the snake case of the model, the filter as a sorted atom list, and the kind (<c>plain</c>, <c>trigram</c>, <c>expression</c>, <c>single-row</c>).</summary>
internal sealed record IndexFact(string Table, string Columns, bool Unique, string Filter, bool NullsNotDistinct, string Kind)
{
    public string Key => $"{Table} ({Columns}){(Unique ? " unique" : string.Empty)}{(NullsNotDistinct ? " nulls-not-distinct" : string.Empty)}{(Kind == "plain" ? string.Empty : " " + Kind)}{(Filter.Length == 0 ? string.Empty : " where " + Filter)}";
}

internal sealed record CheckFact(string Table, string Name, string Sql);

internal sealed record PropertyFact(string Table, string Name, Type ClrType);

/// <summary>
/// The finished model of the one context read without a database (09 21.4 item 1: an equality check opens no connection and reads no
/// connection string; the string below is a placeholder no test opens), and the indexes that only the migrations create (the single-row
/// indexes and the expression index are raw SQL, <c>06 7.5.2</c>, <c>2.3.2</c>).
/// </summary>
internal sealed partial class ModelFacts
{
    private const string PlaceholderConnectionString = "Host=localhost;Database=shopfolio_consistency_model";

    private ModelFacts(
        IReadOnlyList<ForeignKeyFact> foreignKeys,
        IReadOnlyList<IndexFact> indexes,
        IReadOnlyList<CheckFact> checks,
        IReadOnlyList<PropertyFact> properties,
        IReadOnlyList<string> tables)
    {
        ForeignKeys = foreignKeys;
        Indexes = indexes;
        Checks = checks;
        Properties = properties;
        Tables = tables;
    }

    public IReadOnlyList<ForeignKeyFact> ForeignKeys { get; }

    /// <summary>Every index of the database: the model's and the migrations' raw ones.</summary>
    public IReadOnlyList<IndexFact> Indexes { get; }

    public IReadOnlyList<CheckFact> Checks { get; }

    public IReadOnlyList<PropertyFact> Properties { get; }

    /// <summary>The entity names (the table's entity, not the table name).</summary>
    public IReadOnlyList<string> Tables { get; }

    private static readonly Lazy<ModelFacts> _shared = new(Read);

    /// <summary>The model read once for all checks of the run.</summary>
    public static ModelFacts Shared => _shared.Value;

    public static ModelFacts Read()
    {
        DbContextOptionsBuilder<AppDbContext> options = new();
        AppDbContext.Configure(options, PlaceholderConnectionString);
        using AppDbContext context = new(options.Options);
        IModel model = context.GetService<IDesignTimeModel>().Model;
        IEntityType[] entities = [.. model.GetEntityTypes().OrderBy(e => e.ClrType.Name, StringComparer.Ordinal)];

        List<ForeignKeyFact> foreignKeys = [];
        List<IndexFact> indexes = [];
        List<CheckFact> checks = [];
        List<PropertyFact> properties = [];
        foreach (IEntityType entity in entities)
        {
            string table = entity.ClrType.Name;
            foreach (IForeignKey foreignKey in entity.GetForeignKeys())
            {
                foreignKeys.Add(new ForeignKeyFact(
                    table,
                    string.Join(",", foreignKey.Properties.Select(p => p.Name)),
                    foreignKey.PrincipalEntityType.ClrType.Name,
                    foreignKey.IsRequired,
                    foreignKey.IsUnique,
                    foreignKey.DeleteBehavior.ToString()));
            }

            foreach (IIndex index in entity.GetIndexes())
            {
                bool trigram = index.FindAnnotation("Npgsql:IndexOperators")?.Value is string[] operators && operators.Contains("gin_trgm_ops");
                bool notDistinct = index.FindAnnotation("Npgsql:NullsDistinct")?.Value is false;
                indexes.Add(new IndexFact(
                    table,
                    string.Join(",", index.Properties.Select(p => DataModelDocument.Snake(p.Name))),
                    index.IsUnique,
                    FilterAtoms.FromSql(index.GetFilter()),
                    notDistinct,
                    trigram ? "trigram" : "plain"));
            }

            foreach (ICheckConstraint check in entity.GetCheckConstraints())
            {
                checks.Add(new CheckFact(table, check.Name!, check.Sql));
            }

            foreach (IProperty property in entity.GetProperties())
            {
                properties.Add(new PropertyFact(table, property.Name, property.ClrType));
            }
        }

        Dictionary<string, string> entityOfTable = entities.ToDictionary(e => e.GetTableName()!, e => e.ClrType.Name, StringComparer.Ordinal);
        indexes.AddRange(ReadRawIndexes(RepositoryRoot.Find(), entityOfTable));
        return new ModelFacts([.. foreignKeys.Select(f => f with { Unique = f.Unique || IsOneToOne(f, indexes) })], indexes, checks, properties, [.. entities.Select(e => e.ClrType.Name)]);
    }

    /// <summary>
    /// A link is "one" in a diagram (<c>06 6.1.2</c>, right mark <c>o|</c>) when the table has at most one row per target: a unique index on exactly the
    /// link's column, whole or filtered to the rows where the column is filled.
    /// </summary>
    private static bool IsOneToOne(ForeignKeyFact foreignKey, IReadOnlyList<IndexFact> indexes)
    {
        string column = DataModelDocument.Snake(foreignKey.Field);
        return indexes.Any(i => i.Table == foreignKey.Table && i.Unique && i.Columns == column && (i.Filter.Length == 0 || i.Filter == $"{column} IS NOT NULL"));
    }

    /// <summary>
    /// The indexes that the migrations create with raw SQL (<c>CREATE [UNIQUE] INDEX name ON public.table (columns)</c>): the single-row
    /// index of a one-row table (<c>((true))</c>) and the expression index of the contact request. The text of the migration files is read;
    /// the designer files and the model snapshot are not migrations.
    /// </summary>
    public static IReadOnlyList<IndexFact> ReadRawIndexes(string repositoryRoot, IReadOnlyDictionary<string, string> entityOfTable)
    {
        string folder = Path.Combine(repositoryRoot, "src", "Shopfolio.Web", "Infrastructure", "Migrations");
        List<IndexFact> indexes = [];
        foreach (string file in Directory.EnumerateFiles(folder, "*.cs").Where(f => !f.EndsWith(".Designer.cs", StringComparison.Ordinal) && !f.EndsWith("Snapshot.cs", StringComparison.Ordinal)).Order(StringComparer.Ordinal))
        {
            indexes.AddRange(ReadRawIndexesOf(File.ReadAllText(file), entityOfTable));
        }

        return indexes;
    }

    public static IReadOnlyList<IndexFact> ReadRawIndexesOf(string migrationText, IReadOnlyDictionary<string, string> entityOfTable)
    {
        List<IndexFact> indexes = [];
        foreach (Match match in RawIndex().Matches(migrationText))
        {
            string table = match.Groups["table"].Value;
            string columns = match.Groups["columns"].Value.Trim();
            bool singleRow = columns == "(true)";
            if (!entityOfTable.TryGetValue(table, out string? entity))
            {
                throw new InvalidOperationException($"A migration creates an index on the table '{table}', which the model does not map.");
            }

            string canonical = singleRow ? "(true)" : CanonicalExpressionColumns(columns);
            indexes.Add(new IndexFact(
                entity,
                canonical,
                match.Groups["unique"].Success,
                string.Empty,
                false,
                singleRow ? "single-row" : "expression"));
        }

        return indexes;
    }

    /// <summary>Lower case, one space after each comma, and the parentheses that only wrap one expression dropped.</summary>
    public static string CanonicalExpressionColumns(string columns)
    {
        string lowered = Regex.Replace(columns.ToLowerInvariant(), @"\s*,\s*", ", ", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));
        return Regex.Replace(lowered, @"(?<=^|, )\((?<inner>[a-z_]+\([^()]*\))\)(?=,|$)", "${inner}", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)).Replace(", ", ",", StringComparison.Ordinal);
    }

    [GeneratedRegex(@"CREATE (?<unique>UNIQUE )?INDEX (?<name>\w+) ON public\.(?<table>\w+) \((?<columns>[^;""]+)\);", RegexOptions.CultureInvariant)]
    private static partial Regex RawIndex();
}
