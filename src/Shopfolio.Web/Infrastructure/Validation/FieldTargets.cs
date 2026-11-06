using System.Reflection;

using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;

namespace Shopfolio.Infrastructure.Validation;

/// <summary>What the page filter needs to know about a model-state key: the field's label and, if its type conversion is a rule's failure, that rule.</summary>
internal sealed record FieldTarget(string Label, FieldRuleAttribute? ConversionRule);

/// <summary>
/// Finds the model field a model-state key stands for (<c>input.Items[0].Quantity</c>, <c>Name</c>): the handler's parameters and the page's bound
/// properties are the roots; the key may or may not start with the root's name, and an index is skipped. The label is the field's display name
/// (<c>[Display(Name = ...)]</c>, else the property name); a key that matches nothing keeps its last segment as the label.
/// </summary>
internal sealed class FieldTargets
{
    private readonly IModelMetadataProvider _provider;
    private readonly List<(string Name, Type Type)> _roots = [];

    public FieldTargets(IModelMetadataProvider provider, PageHandlerExecutingContext context)
    {
        _provider = provider;
        if (context.HandlerMethod is { } handler)
        {
            foreach (HandlerParameterDescriptor parameter in handler.Parameters)
            {
                _roots.Add((parameter.Name, parameter.ParameterType));
            }
        }

        foreach (ParameterDescriptor property in context.ActionDescriptor.BoundProperties)
        {
            _roots.Add((property.Name, property.ParameterType));
        }
    }

    public FieldTarget Resolve(string key)
    {
        string[] names = [.. key.Split('.').Select(StripIndex).Where(n => n.Length > 0)];
        if (names.Length == 0)
        {
            return new FieldTarget(key, null);
        }

        foreach ((string rootName, Type rootType) in _roots)
        {
            if (names.Length == 1 && string.Equals(names[0], rootName, StringComparison.OrdinalIgnoreCase))
            {
                return Describe(_provider.GetMetadataForType(rootType), rootName);
            }

            if (names.Length > 1 && string.Equals(names[0], rootName, StringComparison.OrdinalIgnoreCase) && Walk(rootType, names[1..]) is { } prefixed)
            {
                return Describe(prefixed, names[^1]);
            }
        }

        foreach ((_, Type rootType) in _roots)
        {
            if (Walk(rootType, names) is { } bare)
            {
                return Describe(bare, names[^1]);
            }
        }

        return new FieldTarget(names[^1], null);
    }

    private static string StripIndex(string segment)
    {
        int bracket = segment.IndexOf('[', StringComparison.Ordinal);
        return bracket < 0 ? segment : segment[..bracket];
    }

    private ModelMetadata? Walk(Type root, string[] names)
    {
        ModelMetadata current = _provider.GetMetadataForType(root);
        foreach (string name in names)
        {
            if (current.IsEnumerableType && current.ElementMetadata is { } element)
            {
                current = element;
            }

            ModelMetadata? next = current.Properties.FirstOrDefault(p => string.Equals(p.PropertyName, name, StringComparison.OrdinalIgnoreCase));
            if (next is null)
            {
                return null;
            }

            current = next;
        }

        return current;
    }

    private static FieldTarget Describe(ModelMetadata metadata, string fallbackName)
    {
        FieldRuleAttribute? rule = null;
        if (metadata.ContainerType is { } container && metadata.PropertyName is { } property)
        {
            rule = container.GetProperty(property, BindingFlags.Instance | BindingFlags.Public)
                ?.GetCustomAttributes<FieldRuleAttribute>(inherit: true)
                .FirstOrDefault(a => a.OwnsConversionFailure);
        }

        string label = metadata.DisplayName ?? metadata.PropertyName ?? fallbackName;
        return new FieldTarget(label, rule);
    }
}
