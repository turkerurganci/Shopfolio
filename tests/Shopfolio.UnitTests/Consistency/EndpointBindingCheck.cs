using System.Reflection;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Razor.Hosting;

using Shopfolio.Infrastructure.Binding;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>A page of the application: the route template its <c>@page</c> directive gives and the page model class that holds its handlers.</summary>
internal sealed record PageRoute(string Template, Type PageModel);

/// <summary>
/// The endpoint side of the model tables (T538; 07 3.2, 5.2, 5.2.4, 5.3; 09 5.3 item 2, 21.4 item 1): the use cases of the inventory are resolved to the handlers of the
/// pages by address and <c>handler</c> parameter, and what each handler binds is compared with the "Uçlar" column of the model tables. A problem is a line that says
/// what differs: <c>NoHandler</c> - a use case of a model row (or of the modelless list) has no handler in the code; <c>ModelNotBound</c> - none of a use case's
/// handlers binds the row's model; <c>UseCaseNotListed</c> - a handler binds a model whose row does not name its use case; <c>ModelWithoutEndpoint</c> - a model no
/// handler binds; <c>ModelOnModelless</c> - a handler of 07 5.2.4 binds a model; <c>NoModelRow</c> - a POST of the inventory that is in no row and not in 5.2.4;
/// <c>InputWithoutModel</c> - a handler takes a value that is neither the model nor a route value; <c>EntityBound</c> - a handler binds an entity of 06 (07 5.1.3).
/// </summary>
internal static partial class EndpointBindingCheck
{
    /// <summary>The pages of an assembly with their route templates, from the compiled <c>@page</c> directives.</summary>
    public static IReadOnlyList<PageRoute> PagesOf(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        List<PageRoute> pages = [];
        foreach (RazorCompiledItemAttribute item in assembly.GetCustomAttributes<RazorCompiledItemAttribute>().Where(i => i.Kind == "mvc.1.0.razor-page"))
        {
            string? template = item.Type.GetCustomAttributes<RazorCompiledItemMetadataAttribute>().FirstOrDefault(m => m.Key == "RouteTemplate")?.Value;
            Type? model = item.Type.BaseType is { IsGenericType: true } generic ? generic.GetGenericArguments()[0] : null;
            if (template is not null && model is not null && typeof(PageModel).IsAssignableFrom(model))
            {
                pages.Add(new PageRoute(template, model));
            }
        }

        return pages;
    }

    public static List<string> Problems(
        IReadOnlyList<ModelRow> formRows,
        IReadOnlyList<ModelRow> queryRows,
        IReadOnlyList<string> modellessUseCases,
        IReadOnlyList<EndpointRef> endpoints,
        IReadOnlyList<PageRoute> pages,
        IReadOnlySet<Type> entities)
    {
        ArgumentNullException.ThrowIfNull(formRows);
        ArgumentNullException.ThrowIfNull(queryRows);
        ArgumentNullException.ThrowIfNull(modellessUseCases);
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(pages);
        ArgumentNullException.ThrowIfNull(entities);

        List<string> problems = [];
        Dictionary<string, List<Bound>> boundByUseCase = [];

        foreach (EndpointRef endpoint in endpoints.Where(e => e.Method == "POST" ? e.Handler is not null : e.Handler is null))
        {
            IReadOnlyList<Bound> bound = Resolve(endpoint, pages, entities, problems);
            if (!boundByUseCase.TryGetValue(endpoint.UseCase, out List<Bound>? list))
            {
                boundByUseCase[endpoint.UseCase] = list = [];
            }

            list.AddRange(bound);
        }

        string[] formUseCases = [.. formRows.SelectMany(r => r.UseCases).Distinct(StringComparer.Ordinal)];

        // Every POST page handler of the inventory is in a model row or in 07 5.2.4, and 5.2.4's handlers bind no model (07 5.2.4; KK4).
        string[] postUseCases = [.. endpoints.Where(e => e.Method == "POST" && e.Handler is not null).Select(e => e.UseCase).Distinct(StringComparer.Ordinal)];
        problems.AddRange(postUseCases.Except(formUseCases, StringComparer.Ordinal).Except(modellessUseCases, StringComparer.Ordinal).Select(u => $"NoModelRow:{u}"));
        problems.AddRange(modellessUseCases.Where(u => boundByUseCase.TryGetValue(u, out List<Bound>? b) && b.Any(x => typeof(IFormInput).IsAssignableFrom(x.Type)))
            .Select(u => $"ModelOnModelless:{u}"));

        CheckRows(formRows, boundByUseCase, typeof(IFormInput), problems);
        CheckRows(queryRows, boundByUseCase, typeof(IQueryModel), problems);

        // A use case that no row names but whose handler binds a model of the kind (a query model on a page the table does not list).
        foreach ((string useCase, List<Bound> bound) in boundByUseCase)
        {
            foreach (Type model in bound.Select(b => b.Type).Distinct())
            {
                IReadOnlyList<ModelRow> rows = typeof(IFormInput).IsAssignableFrom(model) ? formRows : typeof(IQueryModel).IsAssignableFrom(model) ? queryRows : [];
                if (rows.Count > 0 && !rows.Any(r => r.Models.Contains(model.Name) && r.UseCases.Contains(useCase)))
                {
                    problems.Add($"UseCaseNotListed:{useCase}:{model.Name}");
                }
            }
        }

        return [.. problems.Distinct(StringComparer.Ordinal)];
    }

    private static void CheckRows(IReadOnlyList<ModelRow> rows, Dictionary<string, List<Bound>> boundByUseCase, Type marker, List<string> problems)
    {
        foreach (ModelRow row in rows)
        {
            foreach (string useCase in row.UseCases)
            {
                if (!boundByUseCase.TryGetValue(useCase, out List<Bound>? bound))
                {
                    problems.Add($"NoHandler:{useCase}:{string.Join('/', row.Models)}");
                }
                else if (!bound.Any(b => row.Models.Contains(b.Type.Name)))
                {
                    problems.Add($"ModelNotBound:{useCase}:{string.Join('/', row.Models)}");
                }
            }

            foreach (string model in row.Models)
            {
                bool bound = boundByUseCase.Values.SelectMany(b => b).Any(b => b.Type.Name == model && marker.IsAssignableFrom(b.Type));
                if (!bound)
                {
                    problems.Add($"ModelWithoutEndpoint:{model}");
                }
            }
        }
    }

    private static List<Bound> Resolve(EndpointRef endpoint, IReadOnlyList<PageRoute> pages, IReadOnlySet<Type> entities, List<string> problems)
    {
        List<Bound> found = [];
        bool resolved = false;
        foreach (PageRoute page in pages.Where(p => Matches(p, endpoint)))
        {
            foreach (MethodInfo method in HandlersOf(page.PageModel, endpoint))
            {
                resolved = true;
                string[] routeValues = [.. RouteValue().Matches(page.Template).Select(m => m.Groups["n"].Value)];
                foreach (ParameterInfo parameter in method.GetParameters().Where(p => p.ParameterType != typeof(CancellationToken)))
                {
                    Type type = Nullable.GetUnderlyingType(parameter.ParameterType) ?? parameter.ParameterType;
                    if (entities.Contains(type))
                    {
                        problems.Add($"EntityBound:{endpoint.UseCase}:{page.PageModel.Name}.{method.Name}({parameter.Name})");
                    }
                    else if (ModelNaming.MarkersOf(type).Any())
                    {
                        found.Add(new Bound(type, page.PageModel));
                    }
                    else if (!routeValues.Contains(parameter.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase))
                    {
                        problems.Add($"InputWithoutModel:{endpoint.UseCase}:{page.PageModel.Name}.{method.Name}({parameter.Name})");
                    }
                }
            }
        }

        if (!resolved)
        {
            problems.Add($"NoHandler:{endpoint.UseCase}:{endpoint.Method} {endpoint.Path}{(endpoint.Handler is null ? string.Empty : "?handler=" + endpoint.Handler)}");
        }

        return found;
    }

    private static IEnumerable<MethodInfo> HandlersOf(Type page, EndpointRef endpoint)
    {
        string verb = endpoint.Method == "POST" ? "OnPost" : "OnGet";
        string[] names = endpoint.Handler is null ? [verb, verb + "Async"] : [verb + endpoint.Handler, verb + endpoint.Handler + "Async"];
        for (Type? type = page; type is not null && type != typeof(PageModel); type = type.BaseType)
        {
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Where(m => names.Contains(m.Name, StringComparer.Ordinal)))
            {
                yield return method;
            }
        }
    }

    private static bool Matches(PageRoute page, EndpointRef endpoint)
    {
        if (endpoint.Path is null)
        {
            // UC-117's "…?handler=UploadImage": the address is the form's own, so any page that has the handler is a candidate.
            return HandlersOf(page.PageModel, endpoint).Any();
        }

        string[] documented = [.. Normalize(endpoint.Path, isDocument: true)];
        return Normalize(page.Template, isDocument: false).Any(documented.Contains);
    }

    /// <summary>The path in the form the two sides compare: lower case, every route value as <c>{}</c>; the order root <c>{kök}</c> is two paths, an optional value two.</summary>
    internal static IEnumerable<string> Normalize(string path, bool isDocument)
    {
        string text = path.StartsWith('/') ? path : "/" + path;
        text = text.ToLowerInvariant();
        if (isDocument)
        {
            text = text.Replace("{kök}", "{root}", StringComparison.Ordinal);
        }

        List<string> variants = [];
        string[] segments = text.Split('/', StringSplitOptions.RemoveEmptyEntries);
        List<List<string>> paths = [[]];
        foreach (string segment in segments)
        {
            if (segment == "{root}")
            {
                List<List<string>> next = [];
                foreach (List<string> prefix in paths)
                {
                    next.Add([.. prefix, "siparis", "{}"]);
                    next.Add([.. prefix, "siparis", "erisim", "{}"]);
                }

                paths = next;
            }
            else if (segment.StartsWith('{') && segment.EndsWith('}'))
            {
                bool optional = segment.Contains('?', StringComparison.Ordinal) && !segment.Contains("regex", StringComparison.Ordinal);
                List<List<string>> next = [];
                foreach (List<string> prefix in paths)
                {
                    next.Add([.. prefix, "{}"]);
                    if (optional)
                    {
                        next.Add(prefix);
                    }
                }

                paths = next;
            }
            else
            {
                paths = [.. paths.Select(p => (List<string>)[.. p, segment])];
            }
        }

        foreach (List<string> segmentsOfPath in paths)
        {
            variants.Add("/" + string.Join('/', segmentsOfPath));
        }

        return variants;
    }

    private sealed record Bound(Type Type, Type Page);

    [GeneratedRegex(@"\{\*?(?<n>[A-Za-z0-9_]+)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RouteValue();
}
