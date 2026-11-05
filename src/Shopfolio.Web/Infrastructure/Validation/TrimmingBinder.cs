using System.Reflection;

using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Shopfolio.Infrastructure.Validation;

/// <summary>
/// Marks a text field that is not trimmed (09 3.4 item 5): a value whose spaces mean something. A password field is never trimmed either; it is
/// recognised by <c>[DataType(DataType.Password)]</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class NotTrimmedAttribute : Attribute
{
}

/// <summary>
/// The model binder of a text (09 3.4 item 5; 04 7.3.1): the leading and trailing white space is dropped while the value is bound, and a value of
/// white space only becomes the empty string - not <see langword="null"/> - so a required field answers <c>FieldRequired</c>. What the user typed
/// stays in the model state for the form to show again. A password field and a field marked <see cref="NotTrimmedAttribute"/> are bound as they came.
/// </summary>
internal sealed class TrimmingBinder : IModelBinder
{
    private readonly bool _trim;

    public TrimmingBinder(bool trim)
    {
        _trim = trim;
    }

    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        ArgumentNullException.ThrowIfNull(bindingContext);
        ValueProviderResult result = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (result == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, result);
        string? value = result.FirstValue;
        bindingContext.Result = ModelBindingResult.Success(value is null ? null : _trim ? value.Trim() : value);
        return Task.CompletedTask;
    }
}

/// <summary>Gives <see cref="TrimmingBinder"/> to every text; it is first in the provider list.</summary>
internal sealed class TrimmingBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (context.Metadata.ModelType != typeof(string))
        {
            return null;
        }

        return new TrimmingBinder(!IsKeptAsTyped(context.Metadata));
    }

    private static bool IsKeptAsTyped(ModelMetadata metadata)
    {
        if (string.Equals(metadata.DataTypeName, "Password", StringComparison.Ordinal))
        {
            return true;
        }

        if (metadata.ContainerType is { } container && metadata.PropertyName is { } name)
        {
            return container.GetProperty(name, BindingFlags.Instance | BindingFlags.Public)?.IsDefined(typeof(NotTrimmedAttribute), inherit: true) == true;
        }

        return false;
    }
}
