using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

using Shopfolio.Infrastructure.Binding;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// The two-directional comparison of a model table of 07 5 and the model types of the code (T538; 07 5.2, 5.3.2, 5.4.2; 09 21.4 item 1). A problem is a line
/// that says what differs: <c>Model</c> - a model name only in the document or only in the code; <c>Field</c> - a field of a model (or of a list's element) only in
/// the document or only in the code. The code's side of a form model is its settable public properties, of a query model the camelCase parameter names
/// (07 2.6.1), of a machine body the JSON names (07 2.9.6).
/// </summary>
internal static class ModelInventoryCheck
{
    public enum Kind
    {
        Form,
        Query,
        Machine,
    }

    /// <summary>The model types of the kind in an assembly: concrete classes carrying the kind's marker.</summary>
    public static IReadOnlyList<Type> ModelsOf(Assembly assembly, Kind kind)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        Type[] markers = kind switch
        {
            Kind.Form => [typeof(IFormInput)],
            Kind.Query => [typeof(IQueryModel)],
            _ => [typeof(IMachineRequest), typeof(IMachineResponse)],
        };

        return [.. assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && markers.Any(m => m.IsAssignableFrom(t)))];
    }

    public static List<string> Problems(IReadOnlyList<ModelRow> rows, IEnumerable<Type> models, Kind kind)
    {
        ArgumentNullException.ThrowIfNull(rows);
        ArgumentNullException.ThrowIfNull(models);

        Type[] types = [.. models];
        List<string> problems = [];

        // Every name of every row is a model of the code (a row that names two models names two classes; a name only in the document is reported, never skipped); a model
        // name that two folders both declare (the panel's ChangeNameInput beside the account's) is one model of the inventory with the fields of each declaration (07 5.2.2).
        string[] documented = [.. rows.SelectMany(r => r.Models)];
        problems.AddRange(SetComparison.Compare("Model", documented, types.Select(t => t.Name).Distinct(StringComparer.Ordinal)));

        // A row none of whose models is in the code has no fields to compare: its names are already reported above.
        foreach (ModelRow row in rows.Where(r => types.Any(t => r.Models.Contains(t.Name))))
        {
            Type[] present = [.. types.Where(t => row.Models.Contains(t.Name))];
            string[] documentedFields = [.. row.Fields.Select(f => f.Name)];
            string label = string.Join('/', row.Models);
            string[][] perModel = [.. present.Select(t => FieldNames(t, kind))];
            string[] union = [.. perModel.SelectMany(n => n).Distinct(StringComparer.Ordinal)];

            // A row of two models (ChangeProductPublishStatusInput · ChangeVariantPublishStatusInput) states the fields of both: each model's fields are in the row, and together they are the row.
            problems.AddRange(documentedFields.Distinct(StringComparer.Ordinal).Except(union, StringComparer.Ordinal).Select(f => $"Field:{label}: only in the document: {f}"));
            for (int i = 0; i < present.Length; i++)
            {
                problems.AddRange(perModel[i].Except(documentedFields, StringComparer.Ordinal).Select(f => $"Field:{present[i].Name}: only in the model: {f}"));
            }

            if (kind == Kind.Form)
            {
                foreach (DocField field in row.Fields.Where(f => f.Children.Count > 0))
                {
                    NestedProblems(label, present, field, problems);
                }
            }
        }

        return problems;
    }

    /// <summary>The failure text: one problem per line, so a failure names every difference at once.</summary>
    public static string Report(IReadOnlyList<string> problems)
    {
        ArgumentNullException.ThrowIfNull(problems);
        return string.Join('\n', problems);
    }

    /// <summary>The field names of a model as its kind reads them.</summary>
    public static string[] FieldNames(Type model, Kind kind)
    {
        ArgumentNullException.ThrowIfNull(model);
        // A list the binder fills in place (<c>IList&lt;Line&gt; Items { get; }</c>) is a field as well as a property with a setter.
        IEnumerable<PropertyInfo> properties = model.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(p => kind == Kind.Machine || p.SetMethod is { IsPublic: true } || (p.GetMethod is { IsPublic: true } && ElementOf(p.PropertyType) is not null && p.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(p.PropertyType)));

        return kind switch
        {
            Kind.Machine => [.. properties
                .Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() is not { Condition: JsonIgnoreCondition.Always })
                .Select(p => p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? JsonNamingPolicy.CamelCase.ConvertName(p.Name))],
            Kind.Query => [.. properties.Where(p => p.GetMethod is { IsPublic: true }).Select(p => JsonNamingPolicy.CamelCase.ConvertName(p.Name))],
            _ => [.. properties.Select(p => p.Name)],
        };
    }

    private static void NestedProblems(string label, Type[] owners, DocField field, List<string> problems)
    {
        Type[] nested = [.. owners
            .Select(o => o.GetProperty(field.Name, BindingFlags.Instance | BindingFlags.Public))
            .OfType<PropertyInfo>()
            .Select(p => ElementOf(p.PropertyType))
            .OfType<Type>()];
        if (nested.Length == 0)
        {
            return;
        }

        string[] documentedNames = [.. field.Children.Select(c => c.Name)];
        string[] codeNames = [.. nested.SelectMany(t => FieldNames(t, Kind.Form)).Distinct(StringComparer.Ordinal)];
        problems.AddRange(documentedNames.Except(codeNames, StringComparer.Ordinal).Select(f => $"Field:{label}.{field.Name}: only in the document: {f}"));
        problems.AddRange(codeNames.Except(documentedNames, StringComparer.Ordinal).Select(f => $"Field:{label}.{field.Name}: only in the model: {f}"));
        foreach (DocField child in field.Children.Where(c => c.Children.Count > 0))
        {
            NestedProblems($"{label}.{field.Name}", nested, child, problems);
        }
    }

    /// <summary>The element type of a list property, or the type of a group property; <c>null</c> for a simple value.</summary>
    private static Type? ElementOf(Type propertyType)
    {
        Type type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
        if (type == typeof(string) || type.IsPrimitive || type.IsEnum || type == typeof(Guid) || type == typeof(decimal) || type == typeof(DateOnly) || type == typeof(DateTimeOffset))
        {
            return null;
        }

        if (type.IsArray)
        {
            return type.GetElementType();
        }

        Type? enumerable = type.GetInterfaces().Concat([type]).FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        if (enumerable is not null)
        {
            return enumerable.GetGenericArguments()[0];
        }

        return typeof(IEnumerable).IsAssignableFrom(type) ? null : type;
    }
}
