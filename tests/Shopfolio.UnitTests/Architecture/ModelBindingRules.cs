using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The model binding rules (T211; 07 5.1; 09 5.3 items 1-2, 3.4), over the types of an assembly. They read the kind markers
/// (<see cref="IFormInput"/>, <see cref="IQueryModel"/>, <see cref="IMachineRequest"/>, <see cref="IMachineResponse"/>) and the page models:
/// <list type="bullet">
/// <item><c>KindName</c> - a marked model's name ends with its kind (07 5.1.1, 5.1.5); <c>MissingMarker</c> - a type under <c>Shopfolio.Pages</c> whose name ends with a kind carries no marker.</item>
/// <item><c>EntityIsModel</c> - an entity of 06 is no model; <c>EntityInModel</c> - no model field, at any depth, is an entity (07 5.1.3).</item>
/// <item><c>EntityBound</c> and <c>UnmarkedBound</c> - what a handler parameter or a bound property of a page holds is a simple value or a marked model; an entity is never bound (07 5.1.3).</item>
/// <item><c>InputName</c> - a bound form input is named after one of the page's POST handlers, or the same name with the <c>Admin</c> prefix (07 5.1.5; 09 5.3 item 2).</item>
/// <item><c>FieldName</c> and <c>SpecNameShadows06</c> - a model field has the name of a 06 entity's property (or an <c>…Id</c> of an entity), or carries <see cref="SpecAssignedNameAttribute"/> and then has no 06 counterpart (07 5.1.2).</item>
/// </list>
/// The rules run over the application assembly and are shown to fail on the deliberate samples (09 19.2 item 3).
/// </summary>
internal static class ModelBindingRules
{
    private const string PagesNamespace = "Shopfolio.Pages";

    /// <summary>The entities of 06: the types the database context exposes as sets.</summary>
    public static IReadOnlySet<Type> EntityTypesOfContext() =>
        typeof(AppDbContext)
            .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
            .Select(p => p.PropertyType.GetGenericArguments()[0])
            .ToHashSet();

    public static List<string> Violations(Assembly assembly, IReadOnlySet<Type> entities)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        ArgumentNullException.ThrowIfNull(entities);

        HashSet<string> entityPropertyNames = entities
            .SelectMany(e => e.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            .Select(p => p.Name)
            .ToHashSet(StringComparer.Ordinal);
        HashSet<string> entityNames = entities.Select(e => e.Name).ToHashSet(StringComparer.Ordinal);

        List<string> violations = [];
        Type[] types = [.. assembly.GetTypes().Where(t => !t.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false))];

        foreach (Type type in types.Where(t => t.IsClass && !t.IsAbstract))
        {
            Type[] markers = [.. ModelNaming.MarkersOf(type)];
            Type? expected = ModelNaming.MarkerOfName(type.Name);

            if (markers.Length > 0 && (markers.Length > 1 || expected != markers[0]))
            {
                violations.Add($"KindName:{type.Name}");
            }

            if (markers.Length == 0 && expected is not null && (type.Namespace ?? string.Empty).StartsWith(PagesNamespace, StringComparison.Ordinal))
            {
                violations.Add($"MissingMarker:{type.Name}");
            }

            if (markers.Length > 0)
            {
                if (entities.Contains(type))
                {
                    violations.Add($"EntityIsModel:{type.Name}");
                }

                WalkModel(type, type.Name, [], violations, entities, entityPropertyNames, entityNames);
            }
        }

        foreach (Type page in types.Where(t => typeof(PageModel).IsAssignableFrom(t) && !t.IsAbstract))
        {
            CheckPage(page, violations, entities);
        }

        return [.. violations.Distinct()];
    }

    private static void CheckPage(Type page, List<string> violations, IReadOnlySet<Type> entities)
    {
        const BindingFlags all = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

        List<(string Member, Type Type)> bound = [];
        bool bindsAll = page.IsDefined(typeof(BindPropertiesAttribute), inherit: true);
        foreach (PropertyInfo property in page.GetProperties(all))
        {
            if (property.IsDefined(typeof(BindPropertyAttribute), inherit: true) || (bindsAll && property.GetMethod?.IsPublic == true && property.SetMethod?.IsPublic == true))
            {
                bound.Add((property.Name, property.PropertyType));
            }
        }

        List<string> postHandlers = [];
        foreach (MethodInfo method in page.GetMethods(all))
        {
            if (!method.Name.StartsWith("OnGet", StringComparison.Ordinal) && !method.Name.StartsWith("OnPost", StringComparison.Ordinal))
            {
                continue;
            }

            if (method.Name.StartsWith("OnPost", StringComparison.Ordinal) && ModelNaming.HandlerNameOf(method.Name) is { } handler)
            {
                postHandlers.Add(handler);
            }

            foreach (ParameterInfo parameter in method.GetParameters())
            {
                bound.Add(($"{method.Name}({parameter.Name})", parameter.ParameterType));
            }
        }

        foreach ((string member, Type declared) in bound)
        {
            foreach (Type leaf in Leaves(declared))
            {
                if (entities.Contains(leaf))
                {
                    violations.Add($"EntityBound:{page.Name}.{member}:{leaf.Name}");
                }
                else if (!IsSimple(leaf))
                {
                    if (!ModelNaming.MarkersOf(leaf).Any())
                    {
                        violations.Add($"UnmarkedBound:{page.Name}.{member}:{leaf.Name}");
                    }
                    else if (typeof(IFormInput).IsAssignableFrom(leaf) && !postHandlers.Any(h => ModelNaming.IsInputNameOf(h, leaf.Name)))
                    {
                        violations.Add($"InputName:{page.Name}:{leaf.Name}");
                    }
                }
            }
        }
    }

    private static void WalkModel(
        Type model,
        string path,
        HashSet<Type> visited,
        List<string> violations,
        IReadOnlySet<Type> entities,
        HashSet<string> entityPropertyNames,
        HashSet<string> entityNames)
    {
        if (!visited.Add(model))
        {
            return;
        }

        foreach (PropertyInfo property in model.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            string where = $"{path}.{property.Name}";
            bool assigned = property.IsDefined(typeof(SpecAssignedNameAttribute), inherit: false);
            bool known = entityPropertyNames.Contains(property.Name) || EntityKey(property.Name, entityNames);

            if (assigned && known)
            {
                violations.Add($"SpecNameShadows06:{where}");
            }
            else if (!assigned && !known)
            {
                violations.Add($"FieldName:{where}");
            }

            foreach (Type leaf in Leaves(property.PropertyType))
            {
                if (entities.Contains(leaf))
                {
                    violations.Add($"EntityInModel:{where}:{leaf.Name}");
                }
                else if (!IsSimple(leaf) && leaf.IsClass)
                {
                    WalkModel(leaf, where, visited, violations, entities, entityPropertyNames, entityNames);
                }
            }
        }
    }

    // 07 5.1.2: the inner key of a record is the record's name and Id (CartItemId, AddressBookId).
    private static bool EntityKey(string name, HashSet<string> entityNames) =>
        name.Length > 2 && name.EndsWith("Id", StringComparison.Ordinal) && entityNames.Contains(name[..^2]);

    /// <summary>The element types behind arrays, nullable values and collections; the type itself otherwise.</summary>
    private static IEnumerable<Type> Leaves(Type type)
    {
        if (type.IsArray)
        {
            return Leaves(type.GetElementType()!);
        }

        if (Nullable.GetUnderlyingType(type) is { } underlying)
        {
            return Leaves(underlying);
        }

        if (type != typeof(string) && typeof(IEnumerable).IsAssignableFrom(type) && type.IsGenericType)
        {
            return type.GetGenericArguments().SelectMany(Leaves);
        }

        return [type];
    }

    private static bool IsSimple(Type type) =>
        type.IsPrimitive
        || type.IsEnum
        || type == typeof(string)
        || type == typeof(decimal)
        || type == typeof(Guid)
        || type == typeof(DateOnly)
        || type == typeof(TimeOnly)
        || type == typeof(DateTimeOffset)
        || type == typeof(TimeSpan)
        || type == typeof(CancellationToken);
}
