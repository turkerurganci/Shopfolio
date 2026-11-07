using System.ComponentModel.DataAnnotations;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Validation;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>The values of the about page form (02 3.27.3): the short introduction is plain text with a ceiling (P-29), the long text is in the closed format set.</summary>
internal sealed record AboutPageInput(string? ShortIntroduction, string? LongText);

/// <summary>The values of the service introduction form (02 3.27.4): name, short description, text and the home page mark. No price, no stock, no cart field exists.</summary>
internal sealed record OfferingInput(string? Name, string? ShortDescription, string? Text, bool IsShownOnHomepage);

/// <summary>The values of the reference work form (02 3.27.5): title, short description, text and the home page mark. No customer name, year or category field exists.</summary>
internal sealed record PortfolioItemInput(string? Title, string? ShortDescription, string? Text, bool IsShownOnHomepage);

/// <summary>The values of the question form (02 3.27.6): the question is plain text with a ceiling (P-28), the answer is in the closed format set. No heading or category field exists.</summary>
internal sealed record FaqItemInput(string? Question, string? Answer);

/// <summary>
/// The values of the branch form (02 3.27.7): name and address (the province from the closed list), and the optional phone, working hours and map link. The map link is a link
/// the visitor opens; there is no embedded map and no coordinate.
/// </summary>
internal sealed record BranchInput(string? Name, Guid? ProvinceId, string? District, string? AddressLine, string? Phone, string? WorkingHours, string? MapUrl);

/// <summary>The values of the general page form (02 3.27.9): title, text and the top menu mark. The layout is fixed: there is no block, section or layout field.</summary>
internal sealed record CustomPageInput(string? Title, string? Text, bool IsShownInMenu);

/// <summary>
/// The field rules of the corporate content forms (02 3.27.3-3.27.9, 3.27.13, 3.27.15, 3.27.24; 04 7.3.2): the checks go through the form rule attributes of
/// <c>Infrastructure/Validation</c> - the one home of the form of a link, a phone number and a character ceiling - and this class only decides which field takes which rule.
/// A ceiling counts characters, not bytes (P-27..P-29). A text is stored trimmed and an empty one is stored as nothing (06 2.8.5); a plain text is stored as typed - no markup is
/// read out of it or taken away, because the page prints it escaped (09 15.3).
/// </summary>
internal static class ContentInputRules
{
    /// <summary>The stored form of a plain text: white space around it is dropped and an empty text is nothing.</summary>
    public static string? Text(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>The stored form of a telephone (06 2.5.1): the ten digits when the text is a number of the accepted forms, else the trimmed text, which the rule then refuses.</summary>
    public static string? Phone(string? value)
    {
        string? text = Text(value);
        if (text is null)
        {
            return null;
        }

        string compact = RequestFormat.CompactPhone(text);
        return compact.Length == 10 && compact.All(char.IsAsciiDigit) && compact[0] != '0' ? compact : text;
    }

    /// <summary>The refusals of the about page's values (02 3.27.3): only the ceiling of the short introduction; an empty about page is saved as a draft (02 3.27.24).</summary>
    public static List<FieldRejection> Check(AboutPageInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        List<FieldRejection> rejections = [];
        Ceiling(rejections, nameof(AboutPageInput.ShortIntroduction), Text(input.ShortIntroduction), ContentRules.ShortIntroductionMaxLength);
        return rejections;
    }

    /// <summary>The refusals of the service introduction's values: the name is there and within P-27, the short description within P-28.</summary>
    public static List<FieldRejection> Check(OfferingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        List<FieldRejection> rejections = [];
        RequiredWithCeiling(rejections, nameof(OfferingInput.Name), input.Name, ContentRules.TitleMaxLength);
        Ceiling(rejections, nameof(OfferingInput.ShortDescription), Text(input.ShortDescription), ContentRules.ShortDescriptionMaxLength);
        return rejections;
    }

    /// <summary>The refusals of the reference work's values: the title is there and within P-27, the short description within P-28.</summary>
    public static List<FieldRejection> Check(PortfolioItemInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        List<FieldRejection> rejections = [];
        RequiredWithCeiling(rejections, nameof(PortfolioItemInput.Title), input.Title, ContentRules.TitleMaxLength);
        Ceiling(rejections, nameof(PortfolioItemInput.ShortDescription), Text(input.ShortDescription), ContentRules.ShortDescriptionMaxLength);
        return rejections;
    }

    /// <summary>The refusals of the question's values: the question is there and within P-28; the answer may be empty in a draft (02 3.27.24).</summary>
    public static List<FieldRejection> Check(FaqItemInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        List<FieldRejection> rejections = [];
        RequiredWithCeiling(rejections, nameof(FaqItemInput.Question), input.Question, ContentRules.ShortDescriptionMaxLength);
        return rejections;
    }

    /// <summary>The refusals of the general page's values: the title is there and within P-27.</summary>
    public static List<FieldRejection> Check(CustomPageInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        List<FieldRejection> rejections = [];
        RequiredWithCeiling(rejections, nameof(CustomPageInput.Title), input.Title, ContentRules.TitleMaxLength);
        return rejections;
    }

    /// <summary>
    /// The refusals of the branch's values except the province (the closed list is read from the database by the caller): the name is there and within P-27, the working hours are
    /// within P-28, the phone has the form of a telephone number and the map link the form of a link. The address parts are not required to save - a draft saves with the name
    /// alone (02 3.27.24); the publish gate asks for them (02 3.27.13).
    /// </summary>
    public static List<FieldRejection> Check(BranchInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        List<FieldRejection> rejections = [];
        RequiredWithCeiling(rejections, nameof(BranchInput.Name), input.Name, ContentRules.TitleMaxLength);
        Ceiling(rejections, nameof(BranchInput.WorkingHours), Text(input.WorkingHours), ContentRules.ShortDescriptionMaxLength);
        Apply(rejections, nameof(BranchInput.Phone), Phone(input.Phone), new InvalidPhoneAttribute(), FieldCodes.InvalidPhone);
        Apply(rejections, nameof(BranchInput.MapUrl), Text(input.MapUrl), new InvalidUrlAttribute(), FieldCodes.InvalidUrl);
        return rejections;
    }

    private static void RequiredWithCeiling(List<FieldRejection> rejections, string field, string? value, int max)
    {
        string? text = Text(value);
        if (text is null)
        {
            rejections.Add(new FieldRejection(field, FieldCodes.FieldRequired));
            return;
        }

        Ceiling(rejections, field, text, max);
    }

    private static void Ceiling(List<FieldRejection> rejections, string field, string? value, int max) =>
        Apply(rejections, field, value, new TextTooLongAttribute(max), FieldCodes.TextTooLong);

    /// <summary>Runs a form rule on a stored-form value; an empty value passes (the required rule decides). The rule attributes work on a validation context.</summary>
    internal static void Apply(List<FieldRejection> rejections, string field, string? value, FieldRuleAttribute rule, string code)
    {
        if (value is not null && rule.GetValidationResult(value, new ValidationContext(value)) is not null)
        {
            rejections.Add(new FieldRejection(field, code));
        }
    }
}
