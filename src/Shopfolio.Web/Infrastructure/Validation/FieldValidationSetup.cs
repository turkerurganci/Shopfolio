using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.DependencyInjection.Extensions;

using Shopfolio.Infrastructure.Errors;

namespace Shopfolio.Infrastructure.Validation;

/// <summary>
/// The registration of the input validation pattern (09 3.4): the zero-dependency attribute validation of the framework with the project's own rule
/// attributes. (1) The framework's implicit "required" on a non-nullable reference type is off, so that required comes only from
/// <see cref="FieldRequiredAttribute"/>. (2) The framework's own conversion failures get a marker the page filter maps to a code, so no framework text
/// ever reaches a page. (3) Texts are trimmed by the binder; amounts, dates and whole numbers are read by <see cref="RequestValueBinder"/> (09 7.2 item 4). (4) One page filter handles the errors. (5) The framework's <c>data-val-*</c> output is
/// off: client scripts are plain JavaScript (MK-01), and the HTML <c>required</c>, <c>maxlength</c> and <c>type</c> attributes are an improvement the
/// server does not rely on.
/// </summary>
internal static class FieldValidationSetup
{
    public static IMvcBuilder AddFieldValidation(this IMvcBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.TryAddSingleton<RejectionLog>();
        builder.Services.Configure<MvcOptions>(options =>
        {
            options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            options.ModelBinderProviders.Insert(0, new TrimmingBinderProvider());

            // The amounts, dates and whole numbers of 07 2.9 are read by the project's binder, never by the framework's culture-bound conversion (09 7.2 item 4).
            options.ModelBinderProviders.Insert(0, new RequestValueBinderProvider());
            options.Filters.Add<ValidationPageFilter>();
            UseConversionMarker(options.ModelBindingMessageProvider);
        });
        builder.Services.Configure<MvcViewOptions>(options => options.HtmlHelperOptions.ClientValidationEnabled = false);
        return builder;
    }

    private static void UseConversionMarker(DefaultModelBindingMessageProvider provider)
    {
        provider.SetAttemptedValueIsInvalidAccessor((_, _) => FieldError.ConversionFailed);
        provider.SetNonPropertyAttemptedValueIsInvalidAccessor(_ => FieldError.ConversionFailed);
        provider.SetUnknownValueIsInvalidAccessor(_ => FieldError.ConversionFailed);
        provider.SetNonPropertyUnknownValueIsInvalidAccessor(() => FieldError.ConversionFailed);
        provider.SetValueIsInvalidAccessor(_ => FieldError.ConversionFailed);
        provider.SetValueMustBeANumberAccessor(_ => FieldError.ConversionFailed);
        provider.SetNonPropertyValueMustBeANumberAccessor(() => FieldError.ConversionFailed);
        provider.SetValueMustNotBeNullAccessor(_ => FieldCodes.FieldRequired);
        provider.SetMissingBindRequiredValueAccessor(_ => FieldCodes.FieldRequired);
        provider.SetMissingKeyOrValueAccessor(() => FieldCodes.FieldRequired);
        provider.SetMissingRequestBodyRequiredValueAccessor(() => FieldCodes.FieldRequired);
    }
}
