using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

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
