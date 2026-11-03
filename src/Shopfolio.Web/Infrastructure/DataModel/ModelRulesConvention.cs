using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Shopfolio.Infrastructure.DataModel;

/// <summary>
/// The model finalizing rule of the one database context (09 10.1 items 1, 2, 4; 10.2 item 1; 06 2.5). It runs when the model is
/// finished, after the naming convention, and gives every entity the same base:
/// <list type="bullet">
/// <item>a relationship whose delete behavior nobody wrote gets <c>NoAction</c> (the framework's default of cascading a required
/// relationship is never applied silently);</item>
/// <item>every <see cref="Guid"/> primary key is <c>ValueGeneratedNever</c>: EF never makes a key;</item>
/// <item>a <c>Version</c> integer is a concurrency token: the update carries the loaded version as its condition;</item>
/// <item>a rule declared with <see cref="ModelBuilderExtensions.HasRule{TEntity}"/> becomes a check constraint named
/// <c>ck_&lt;table&gt;_&lt;rule&gt;</c>;</item>
/// <item>an enum property must be stored as text, and its column gets the check constraint of the allowed code names
/// (<c>ck_&lt;table&gt;_&lt;column&gt;</c>); a subset is declared with <c>HasAllowedValues</c>;</item>
/// <item>a <see cref="decimal"/> must say its type: money, percent or net quantity (06 2.5.1).</item>
/// </list>
/// </summary>
internal sealed class ModelRulesConvention : IModelFinalizingConvention
{
    private static readonly (int Precision, int Scale)[] AllowedDecimalTypes =
    [
        ModelBuilderExtensions.MoneyType,
        ModelBuilderExtensions.PercentType,
        ModelBuilderExtensions.NetQuantityType,
    ];

    public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        foreach (IConventionEntityType entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            ApplyDeleteBehavior(entityType);
            ApplyKeyRule(entityType);
            ApplyPropertyRules(entityType);
            ApplyCheckRules(entityType);
            ApplyEnumRules(entityType);
            ApplyForeignKeyIndexRule(entityType);
        }
    }

    /// <summary>
    /// A foreign key gets the index the framework's own convention would make (it is removed in the context, see <c>AppDbContext</c>),
    /// except on a one-row table, whose only index is the single-row index (06 7.5.2; 7.2 "no index: single row"). An index that already
    /// starts with the key's columns (an explicit one) is not repeated.
    /// </summary>
    private static void ApplyForeignKeyIndexRule(IConventionEntityType entityType)
    {
        if (entityType.FindAnnotation(ModelBuilderExtensions.SingleRowAnnotation) is not null)
        {
            return;
        }

        foreach (IConventionForeignKey foreignKey in entityType.GetDeclaredForeignKeys())
        {
            bool covered = entityType.GetIndexes().Any(i => i.Properties.Take(foreignKey.Properties.Count).SequenceEqual(foreignKey.Properties))
                || entityType.GetKeys().Any(k => k.Properties.Take(foreignKey.Properties.Count).SequenceEqual(foreignKey.Properties));
            if (!covered)
            {
                entityType.AddIndex(foreignKey.Properties)?.SetIsUnique(foreignKey.IsUnique);
            }
        }
    }

    /// <summary>
    /// The one enum that is written as text without a check constraint: the audit trail cannot be converted by a migration and shows an
    /// unrecognized code as it is (09 10.1 item 3; 9.2 item 2). The type is named, not referenced: shared infrastructure does not refer
    /// to a module type (09 6 item 2); a test holds the name equal to the type's.
    /// </summary>
    private const string ValueConverterTypeAnnotation = "ValueConverterType";

    internal const string AuditOperationTypeName = "Shopfolio.Modules.Administration.Contracts.AuditOperation";

    /// <summary>
    /// Whether the property has a conversion to text: set by instance, by provider type, or by converter type (the way the context's
    /// convention sets it, before the model is finished).
    /// </summary>
    private static bool IsStoredAsText(IConventionProperty property)
    {
        Type? provider = property.GetProviderClrType() ?? property.GetValueConverter()?.ProviderClrType;
        if (provider is not null)
        {
            return provider == typeof(string);
        }

        for (Type? converter = property.FindAnnotation(ValueConverterTypeAnnotation)?.Value as Type; converter is not null; converter = converter.BaseType)
        {
            if (converter.IsGenericType && converter.GetGenericTypeDefinition() == typeof(ValueConverter<,>))
            {
                return converter.GetGenericArguments()[1] == typeof(string);
            }
        }

        return false;
    }

    private static void ApplyEnumRules(IConventionEntityType entityType)
    {
        foreach (IConventionProperty property in entityType.GetDeclaredProperties())
        {
            Type type = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
            if (!type.IsEnum)
            {
                continue;
            }

            if (!IsStoredAsText(property))
            {
                throw new InvalidOperationException(
                    $"The enum property '{entityType.DisplayName()}.{property.Name}' is not stored as text; an enum is stored as its code name (06 2.9.1; 09 10.1 item 3).");
            }

            if (type.FullName == AuditOperationTypeName)
            {
                continue;
            }

            string table = entityType.GetTableName()
                ?? throw new InvalidOperationException($"The entity '{entityType.DisplayName()}' is not mapped to a table but has the enum property '{property.Name}'.");
            string column = property.GetColumnName(StoreObjectIdentifier.Table(table, entityType.GetSchema()))
                ?? throw new InvalidOperationException($"The enum property '{entityType.DisplayName()}.{property.Name}' has no column name.");
            string[] allowed = property.FindAnnotation(ModelBuilderExtensions.AllowedValuesAnnotation)?.Value as string[]
                ?? Enum.GetNames(type);
            string list = string.Join(", ", allowed.Select(name => "'" + name.Replace("'", "''", StringComparison.Ordinal) + "'"));
            entityType.AddCheckConstraint("ck_" + table + "_" + column, $"\"{column}\" IN ({list})");
        }
    }

    private static void ApplyDeleteBehavior(IConventionEntityType entityType)
    {
        foreach (IConventionForeignKey foreignKey in entityType.GetDeclaredForeignKeys())
        {
            ConfigurationSource? source = foreignKey.GetDeleteBehaviorConfigurationSource();
            if (source is null or ConfigurationSource.Convention)
            {
                foreignKey.SetDeleteBehavior(DeleteBehavior.NoAction);
            }
        }
    }

    private static void ApplyKeyRule(IConventionEntityType entityType)
    {
        IConventionKey? key = entityType.FindPrimaryKey();
        if (key is null)
        {
            return;
        }

        foreach (IConventionProperty property in key.Properties)
        {
            if (property.ClrType == typeof(Guid))
            {
                property.SetValueGenerated(ValueGenerated.Never);
            }
        }
    }

    private static void ApplyPropertyRules(IConventionEntityType entityType)
    {
        foreach (IConventionProperty property in entityType.GetDeclaredProperties())
        {
            if (property.Name == ModelBuilderExtensions.VersionPropertyName && property.ClrType == typeof(int))
            {
                property.SetIsConcurrencyToken(true);
            }

            if ((property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                && !AllowedDecimalTypes.Contains((property.GetPrecision() ?? 0, property.GetScale() ?? 0)))
            {
                throw new InvalidOperationException(
                    $"The decimal property '{entityType.DisplayName()}.{property.Name}' has no declared type; use AsMoney(), AsPercent() or AsNetQuantity() (06 2.5.1).");
            }
        }
    }

    private static void ApplyCheckRules(IConventionEntityType entityType)
    {
        List<IConventionAnnotation> rules = entityType.GetAnnotations()
            .Where(a => a.Name.StartsWith(ModelBuilderExtensions.RuleAnnotationPrefix, StringComparison.Ordinal))
            .ToList();
        if (rules.Count == 0)
        {
            return;
        }

        string table = entityType.GetTableName()
            ?? throw new InvalidOperationException($"The entity '{entityType.DisplayName()}' is not mapped to a table but declares a rule.");
        foreach (IConventionAnnotation rule in rules)
        {
            string name = "ck_" + table + "_" + rule.Name[ModelBuilderExtensions.RuleAnnotationPrefix.Length..];
            entityType.AddCheckConstraint(name, (string)rule.Value!);
            entityType.RemoveAnnotation(rule.Name);
        }
    }
}
