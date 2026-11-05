using Microsoft.AspNetCore.Routing.Patterns;

namespace Shopfolio.Infrastructure.Routing;

/// <summary>
/// The check of a route template against the address rules (07 2.1): every literal segment is lower case ASCII, digits and single hyphens; the first
/// segment is a literal of the closed list; a record address has the form <c>/{prefix}/{slug}</c>; the order root has one of its two forms; and a
/// storefront path carries no internal key (07 2.1.6). The panel is the one area where an internal key may be part of the path (07 2.1.5).
/// The page convention applies the check at the start of the application and the tests apply it to every mapped endpoint.
/// </summary>
internal static class AddressTemplates
{
    /// <summary>The name of the slug parameter of a record address (06 4.72: the record's <c>Slug</c>).</summary>
    public const string SlugParameter = "slug";

    /// <summary>The problems of the template, in the order found; an empty list means the template follows the rules.</summary>
    public static IReadOnlyList<string> Check(string template)
    {
        ArgumentNullException.ThrowIfNull(template);
        string trimmed = template.TrimStart('/');
        if (trimmed.Length == 0)
        {
            return [];
        }

        RoutePattern pattern;
        try
        {
            pattern = RoutePatternFactory.Parse(trimmed);
        }
        catch (RoutePatternException)
        {
            return ["the template is not a valid route template"];
        }

        List<string> problems = [];
        IReadOnlyList<RoutePatternPathSegment> segments = pattern.PathSegments;
        string? first = CheckSegments(segments, problems);
        if (first is null)
        {
            return problems;
        }

        if (AddressRules.RecordPrefixes.Contains(first, StringComparer.Ordinal))
        {
            CheckRecordAddress(segments, problems);
        }
        else if (string.Equals(first, AddressRules.OrderArea, StringComparison.Ordinal) && !OrderRoutes.StartsWithARoot("/" + trimmed))
        {
            problems.Add("a template under /siparis starts with one of the two order roots (07 2.1.3)");
        }

        if (!string.Equals(first, AddressRules.PanelArea, StringComparison.Ordinal))
        {
            CheckNoInternalKey(segments, problems);
        }

        return problems;
    }

    /// <summary>Checks every segment's shape and the first segment's place in the closed list; returns the first segment's literal when it has one.</summary>
    private static string? CheckSegments(IReadOnlyList<RoutePatternPathSegment> segments, List<string> problems)
    {
        string? first = null;
        for (int i = 0; i < segments.Count; i++)
        {
            RoutePatternPathSegment segment = segments[i];
            if (segment.Parts.Count != 1)
            {
                problems.Add($"segment {i + 1} mixes literal text and a parameter (07 2.1)");
                continue;
            }

            if (segment.Parts[0] is RoutePatternLiteralPart literal)
            {
                bool protocolFile = i == 0 && segments.Count == 1 && AddressRules.ProtocolFiles.Contains(literal.Content, StringComparer.Ordinal);
                if (!protocolFile && !AddressRules.IsValidSegment(literal.Content))
                {
                    problems.Add($"segment '{literal.Content}' is not lower case ASCII letters, digits and single hyphens (07 2.1)");
                }

                if (i == 0)
                {
                    first = literal.Content;
                }
            }
            else if (segment.Parts[0] is RoutePatternParameterPart parameter && parameter.IsCatchAll)
            {
                problems.Add($"segment {i + 1} is a catch-all parameter (07 2.1)");
            }
        }

        if (segments.Count > 0 && first is null)
        {
            problems.Add("the first segment is a parameter or has no literal: an address without a prefix is not allowed (07 2.1.10)");
            return null;
        }

        if (first is not null && !AddressRules.IsClosedFirstSegment(first))
        {
            problems.Add($"the first segment '{first}' is not in the closed list (07 2.1.2)");
        }

        return first;
    }

    private static void CheckRecordAddress(IReadOnlyList<RoutePatternPathSegment> segments, List<string> problems)
    {
        bool slugOnly = segments.Count == 2
            && segments[1].Parts.Count == 1
            && segments[1].Parts[0] is RoutePatternParameterPart { IsOptional: false, IsCatchAll: false } parameter
            && string.Equals(parameter.Name, SlugParameter,StringComparison.OrdinalIgnoreCase);
        if (!slugOnly)
        {
            problems.Add($"a record address is exactly /{{prefix}}/{{{SlugParameter}}} (07 2.1)");
        }
    }

    private static void CheckNoInternalKey(IReadOnlyList<RoutePatternPathSegment> segments, List<string> problems)
    {
        foreach (RoutePatternParameterPart parameter in segments.SelectMany(s => s.Parts).OfType<RoutePatternParameterPart>())
        {
            bool nameIsAKey = string.Equals(parameter.Name, "id", StringComparison.OrdinalIgnoreCase)
                || parameter.Name.EndsWith("Id", StringComparison.Ordinal);
            bool constraintIsAKey = parameter.ParameterPolicies.Any(p => IsKeyConstraint(p.Content));
            if (nameIsAKey || constraintIsAKey)
            {
                problems.Add($"the parameter '{parameter.Name}' is an internal key in a storefront path: it travels in the form or the query (07 2.1.6)");
            }
        }
    }

    private static bool IsKeyConstraint(string? content) =>
        content is not null
        && (content.Equals("guid", StringComparison.OrdinalIgnoreCase)
            || content.Equals("int", StringComparison.OrdinalIgnoreCase)
            || content.Equals("long", StringComparison.OrdinalIgnoreCase));
}
