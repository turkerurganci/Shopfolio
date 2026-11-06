using System.Collections;
using System.Globalization;
using System.Text;

namespace Shopfolio.Infrastructure.Lists;

/// <summary>
/// The address of a list in a given state (07 2.6.1, 2.6.2, 2.6.8): the list's root, then the fields of the query model that hold a value as camelCase
/// parameters, a repeating field once per value, and <c>page</c> last - written only when the page shown is not the first (07 2.6.2: the first page is
/// parameterless). The values are the ones the request carried, an invalid one included: the address is what the user sees and returns to. The address is
/// built from the root and the model, never from the request's own path, so it is a path of this site. It is the value of <c>returnUrl</c> on a link from a
/// panel list to a record (<see cref="ListReturn"/>) and the base of the page links.
/// </summary>
internal static class ListAddress
{
    private const string PageField = nameof(PagedQuery.Page);

    public static string Of(string listRoot, PagedQuery query, int shownPage)
    {
        ArgumentException.ThrowIfNullOrEmpty(listRoot);
        ArgumentNullException.ThrowIfNull(query);

        StringBuilder address = new(listRoot);
        char separator = listRoot.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        foreach (System.Reflection.PropertyInfo field in QueryParameterNames.FieldsOf(query.GetType()))
        {
            if (field.Name == PageField)
            {
                continue;
            }

            foreach (string value in ValuesOf(field.GetValue(query)))
            {
                address.Append(separator).Append(QueryParameterNames.Of(field)).Append('=').Append(Uri.EscapeDataString(value));
                separator = '&';
            }
        }

        if (shownPage > PageNumber.First)
        {
            address.Append(separator).Append(QueryParameterNames.Of(PageField)).Append('=').Append(shownPage.ToString(CultureInfo.InvariantCulture));
        }

        return address.ToString();
    }

    private static IEnumerable<string> ValuesOf(object? value)
    {
        switch (value)
        {
            case null:
                yield break;
            case string text:
                if (text.Length > 0)
                {
                    yield return text;
                }

                yield break;
            case IEnumerable many:
                foreach (object? item in many)
                {
                    foreach (string text in ValuesOf(item))
                    {
                        yield return text;
                    }
                }

                yield break;
            default:
                yield return Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty;
                yield break;
        }
    }
}
