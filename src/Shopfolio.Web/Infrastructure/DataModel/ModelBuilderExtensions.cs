using System.Text.RegularExpressions;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shopfolio.Infrastructure.DataModel;

/// <summary>
/// The model declarations of the entity configurations (09 10.1 item 4; 06 2.5.1). They add no constraint and no type themselves:
/// they write a declaration on the model, and <see cref="ModelRulesConvention"/> turns it into the constraint when the model is finished.
/// </summary>
internal static partial class ModelBuilderExtensions
{
    /// <summary>Money: kuruş precision, at most 9.999.999.999,99 (06 2.5.1).</summary>
    public static readonly (int Precision, int Scale) MoneyType = (12, 2);

    /// <summary>Rate and percent: "20" means 20 percent (06 2.5.1).</summary>
    public static readonly (int Precision, int Scale) PercentType = (5, 2);

    /// <summary>Net quantity in the unit of the product (06 2.5.1).</summary>
    public static readonly (int Precision, int Scale) NetQuantityType = (12, 3);

    /// <summary>The name of an integer property that is the record version (06 2.6.1) and a concurrency token.</summary>
    public const string VersionPropertyName = "Version";

    internal const string RuleAnnotationPrefix = "Shopfolio:Rule:";

    /// <summary>
    /// Declares a check constraint by the name of its rule. The constraint is named <c>ck_&lt;table&gt;_&lt;rule&gt;</c>, so the rule name
    /// is lowercase with underscores; the name is never given by hand (06 2.4.3).
    /// </summary>
    public static EntityTypeBuilder<TEntity> HasRule<TEntity>(this EntityTypeBuilder<TEntity> builder, string ruleName, string sql)
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(sql);
        if (!RuleName().IsMatch(ruleName))
        {
            throw new ArgumentException($"The rule name '{ruleName}' must be lowercase letters, digits and underscores, starting with a letter.", nameof(ruleName));
        }

        return builder.HasAnnotation(RuleAnnotationPrefix + ruleName, sql);
    }

    /// <summary>The column holds money: <c>numeric(12,2)</c>.</summary>
    public static PropertyBuilder AsMoney(this PropertyBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.HasPrecision(MoneyType.Precision, MoneyType.Scale);
    }

    /// <summary>The column holds a rate or percent: <c>numeric(5,2)</c>.</summary>
    public static PropertyBuilder AsPercent(this PropertyBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.HasPrecision(PercentType.Precision, PercentType.Scale);
    }

    /// <summary>The column holds a net quantity: <c>numeric(12,3)</c>.</summary>
    public static PropertyBuilder AsNetQuantity(this PropertyBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.HasPrecision(NetQuantityType.Precision, NetQuantityType.Scale);
    }

    [GeneratedRegex("^[a-z][a-z0-9_]*$")]
    private static partial Regex RuleName();
}
