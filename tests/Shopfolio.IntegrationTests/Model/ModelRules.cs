using System.Text;
using System.Text.RegularExpressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>
/// The model test of the data model rules (09 5.2 item 5, 10.1 item 5; T101 KK3, KK7, KK8): reads the finished model of the context and lists
/// every place where a rule is broken. An empty list means the model follows the rules. The same check runs over the application's
/// own entities and, as a negative test, over the sample that breaks the rules.
/// </summary>
internal static partial class ModelRules
{
    /// <summary>The one table the framework writes, not the application (06 2.8.4; T118): no creation time.</summary>
    public const string DataProtectionKeyEntity = "DataProtectionKey";

    public const int IdentifierMaxBytes = 63;

    private static readonly DeleteBehavior[] AllowedDeleteBehaviors = [DeleteBehavior.Cascade, DeleteBehavior.SetNull, DeleteBehavior.NoAction];

    /// <summary>The finished (design time) model of a context; conventions have run and every annotation is in place.</summary>
    public static IModel ModelOf(DbContext context)
    {
        return context.GetService<IDesignTimeModel>().Model;
    }

    public static IReadOnlyList<string> Check(IModel model, Func<IEntityType, bool>? include = null)
    {
        List<string> findings = [];
        foreach (IEntityType entityType in model.GetEntityTypes().Where(e => include is null || include(e)))
        {
            string? table = entityType.GetTableName();
            if (table is null)
            {
                findings.Add($"{entityType.DisplayName()}: the entity is not mapped to a table.");
                continue;
            }

            StoreObjectIdentifier store = StoreObjectIdentifier.Table(table, entityType.GetSchema());
            CheckName(findings, $"{table}: table name", table);
            foreach (IProperty property in entityType.GetProperties())
            {
                CheckName(findings, $"{table}: column name", property.GetColumnName(store));
            }

            foreach (IKey key in entityType.GetKeys())
            {
                CheckName(findings, $"{table}: key name", key.GetName());
            }

            foreach (IIndex index in entityType.GetIndexes())
            {
                CheckName(findings, $"{table}: index name", index.GetDatabaseName());
            }

            foreach (IForeignKey foreignKey in entityType.GetForeignKeys())
            {
                CheckName(findings, $"{table}: foreign key name", foreignKey.GetConstraintName());
                CheckDeleteBehavior(findings, table, foreignKey);
            }

            foreach (ICheckConstraint constraint in entityType.GetCheckConstraints())
            {
                CheckName(findings, $"{table}: check constraint name", constraint.Name);
            }

            CheckCreatedAt(findings, entityType, table, store);
            CheckKeyAndVersion(findings, entityType, table);
        }

        return findings;
    }

    /// <summary>The relationships that cascade, as <c>Dependent -> Principal (columns)</c>, for the comparison with the document's list (06 2.2.5).</summary>
    public static IReadOnlyList<string> CascadeRelations(IModel model, Func<IEntityType, bool>? include = null)
    {
        return model.GetEntityTypes()
            .Where(e => include is null || include(e))
            .SelectMany(e => e.GetForeignKeys())
            .Where(f => f.DeleteBehavior == DeleteBehavior.Cascade)
            .Select(f => $"{f.DeclaringEntityType.GetTableName()} -> {f.PrincipalEntityType.GetTableName()} ({string.Join(",", f.Properties.Select(p => p.Name))})")
            .Order(StringComparer.Ordinal)
            .ToList();
    }

    private static void CheckName(List<string> findings, string what, string? name)
    {
        if (name is null || !NamePattern().IsMatch(name))
        {
            findings.Add($"{what} '{name}' does not match ^[a-z][a-z0-9_]*$.");
        }
        else if (Encoding.UTF8.GetByteCount(name) > IdentifierMaxBytes)
        {
            findings.Add($"{what} '{name}' is longer than {IdentifierMaxBytes} bytes.");
        }
    }

    private static void CheckDeleteBehavior(List<string> findings, string table, IForeignKey foreignKey)
    {
        string columns = string.Join(",", foreignKey.Properties.Select(p => p.Name));
        if (!AllowedDeleteBehaviors.Contains(foreignKey.DeleteBehavior))
        {
            findings.Add($"{table}: the delete behavior of ({columns}) is {foreignKey.DeleteBehavior}; only Cascade, SetNull and NoAction are allowed.");
        }

        if (foreignKey.DeleteBehavior == DeleteBehavior.SetNull && foreignKey.Properties.Any(p => !p.IsNullable))
        {
            findings.Add($"{table}: the SetNull relationship ({columns}) has a column that cannot be null.");
        }
    }

    private static void CheckCreatedAt(List<string> findings, IEntityType entityType, string table, StoreObjectIdentifier store)
    {
        if (entityType.ClrType.Name == DataProtectionKeyEntity)
        {
            return;
        }

        IProperty? createdAt = entityType.GetProperties().SingleOrDefault(p => p.GetColumnName(store) == "created_at");
        if (createdAt is null)
        {
            findings.Add($"{table}: there is no created_at column (06 2.8.4).");
        }
        else if (createdAt.IsNullable || createdAt.GetColumnType() != "timestamp with time zone")
        {
            findings.Add($"{table}: created_at is not a required timestamptz.");
        }
    }

    private static void CheckKeyAndVersion(List<string> findings, IEntityType entityType, string table)
    {
        foreach (IProperty property in entityType.FindPrimaryKey()?.Properties ?? [])
        {
            if (property.ClrType == typeof(Guid) && property.ValueGenerated != ValueGenerated.Never)
            {
                findings.Add($"{table}: the Guid key '{property.Name}' is not ValueGeneratedNever.");
            }
        }

        foreach (IProperty version in entityType.GetProperties().Where(p => p.Name == "Version" && p.ClrType == typeof(int)))
        {
            if (!version.IsConcurrencyToken)
            {
                findings.Add($"{table}: the Version property is not a concurrency token (06 2.6.1).");
            }
        }
    }

    [GeneratedRegex("^[a-z][a-z0-9_]*$")]
    private static partial Regex NamePattern();
}
