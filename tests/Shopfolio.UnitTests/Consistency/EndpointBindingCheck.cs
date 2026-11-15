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
            Type? model = item.Type.GetProperty("Model", BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)?.PropertyType;
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
        IReadOnlySet<Type> entities,
        IEnumerable<Type> models)
    {
        ArgumentNullException.ThrowIfNull(models);
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

        // A model class of the code that no handler binds has no endpoint (07 5.2; KK1: a model added to the code without its handler).
        HashSet<Type> boundTypes = [.. boundByUseCase.Values.SelectMany(b => b).Select(b => b.Type)];
        problems.AddRange(models.Where(m => !boundTypes.Contains(m)).Select(m => $"ModelWithoutEndpoint:{m.Name}"));

        return [.. problems.Distinct(StringComparer.Ordinal)];
    }

    /// <summary>The parameters 07 5.3.1 names as common: a handler may take them as plain values (the panel's record pages have no query row, 07 5.3.2).</summary>
    private static readonly string[] CommonParameters = ["notice", "returnUrl", "page"];

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

            // A row that names two models is carried by one class (07 5.2.2), so one of its names is a model of the code; a row none of whose names is bound has no endpoint.
            if (!row.Models.Any(model => boundByUseCase.Values.SelectMany(b => b).Any(b => b.Type.Name == model && marker.IsAssignableFrom(b.Type))))
            {
                problems.Add($"ModelWithoutEndpoint:{string.Join('/', row.Models)}");
            }
        }
    }

    private static List<Bound> Resolve(EndpointRef endpoint, IReadOnlyList<PageRoute> pages, IReadOnlySet<Type> entities, List<string> problems)
    {
        List<Bound> found = [];
        bool resolved = false;
        foreach (PageRoute page in Candidates(pages, endpoint))
        {
            foreach (MethodInfo method in HandlersOf(page.PageModel, endpoint))
            {
                resolved = true;

                // A handler of a base class or of a model that two addresses share (the new-record and the edit page) reads the route values of every page it serves.
                string[] routeValues = [.. pages.Where(p => method.DeclaringType!.IsAssignableFrom(p.PageModel))
                    .SelectMany(p => RouteValue().Matches(p.Template).Select(m => m.Groups["n"].Value))];
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
                    else if (!routeValues.Contains(parameter.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase) && !CommonParameters.Contains(parameter.Name ?? string.Empty, StringComparer.Ordinal))
                    {
                        problems.Add($"InputWithoutModel:{endpoint.UseCase}:{page.PageModel.Name}.{method.Name}({parameter.Name})");
                    }
                }
            }
        }

        // A GET that no page serves is a machine endpoint (the download, the site map, the liveness, the chunked upload; 07 5.4): its body, if any, is 5.4's.
        if (!resolved && endpoint.Method == "POST")
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

    /// <summary>
    /// The pages that serve an address: a literal of the document (<c>/panel/kuponlar/yeni</c>) is served by a page of that literal, or - when no page has it - by a page whose
    /// route value stands there (<c>/panel/kuponlar/{key?}</c>); a value of the document (<c>{iç anahtar}</c>) is any value of the page.
    /// </summary>
    private static IEnumerable<PageRoute> Candidates(IReadOnlyList<PageRoute> pages, EndpointRef endpoint)
    {
        if (endpoint.Path is null)
        {
            // UC-117's "…?handler=UploadImage": the address is the form's own, so any page that has the handler is a candidate.
            return pages.Where(p => HandlersOf(p.PageModel, endpoint).Any());
        }

        string[][] documented = [.. Normalize(endpoint.Path, isDocument: true).Select(p => p.Split('/', StringSplitOptions.RemoveEmptyEntries))];
        List<(PageRoute Page, int Literals)> matches = [];
        foreach (PageRoute page in pages)
        {
            int best = int.MaxValue;
            foreach (string[] served in Normalize(page.Template, isDocument: false).Select(p => p.Split('/', StringSplitOptions.RemoveEmptyEntries)))
            {
                foreach (string[] asked in documented.Where(d => d.Length == served.Length))
                {
                    int literals = 0;
                    bool same = true;
                    for (int i = 0; i < asked.Length && same; i++)
                    {
                        if (asked[i] == served[i])
                        {
                            continue;
                        }

                        if (served[i] == "{}")
                        {
                            literals++;
                        }
                        else
                        {
                            same = false;
                        }
                    }

                    best = same ? Math.Min(best, literals) : best;
                }
            }

            if (best != int.MaxValue)
            {
                matches.Add((page, best));
            }
        }

        int fewest = matches.Count == 0 ? 0 : matches.Min(m => m.Literals);
        return [.. matches.Where(m => m.Literals == fewest).Select(m => m.Page)];
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
