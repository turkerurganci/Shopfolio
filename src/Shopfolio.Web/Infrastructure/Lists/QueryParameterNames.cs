using System.Reflection;

namespace Shopfolio.Infrastructure.Lists;

/// <summary>
/// The name of a list parameter (07 2.6.1; K-1172 (5)): the camelCase of the query model's field name. The framework binds names without regard to case, so
/// a hand-made <c>?MinPrice=</c> binds too; the name the application writes - in an address, a form field, a link - is this one. A Turkish alias such as
/// <c>?sayfa=</c> is no parameter (07 2.6.9).
/// </summary>
internal static class QueryParameterNames
{
    public static string Of(string fieldName)
    {
        ArgumentException.ThrowIfNullOrEmpty(fieldName);
        return char.ToLowerInvariant(fieldName[0]) + fieldName[1..];
    }

    public static string Of(PropertyInfo field)
    {
        ArgumentNullException.ThrowIfNull(field);
        return Of(field.Name);
    }

    /// <summary>The fields of a query model that are parameters: its public instance properties that can be read and set.</summary>
    public static IEnumerable<PropertyInfo> FieldsOf(Type queryModel)
    {
        ArgumentNullException.ThrowIfNull(queryModel);
        return queryModel.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => p.GetMethod is { IsPublic: true } && p.SetMethod is { IsPublic: true });
    }
}
