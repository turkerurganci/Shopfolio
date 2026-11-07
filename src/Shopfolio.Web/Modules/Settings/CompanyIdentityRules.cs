using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Settings;

/// <summary>
/// The rules of the company identity (02 3.1.2, 3.1.3, 3.1.5 condition 1; 06 4.1): which fields a company type requires, which of them are still
/// empty, and what a save may not do. Pure: no database, no clock. The required fields are rules of the application and not of the database,
/// because the row is born empty (06 4.1). The names are the members of <see cref="CompanyIdentityInput"/>; a closed set of type is the
/// <see cref="CompanyType"/> enum, so a type outside it is a code that no list holds.
/// </summary>
internal static class CompanyIdentityRules
{
    /// <summary>The fields only the tradesman must give (02 3.1.3): the owner's name and the tax number.</summary>
    private static readonly string[] TradesmanFields = [nameof(CompanyIdentityInput.LegalName), nameof(CompanyIdentityInput.TaxIdNumber)];

    /// <summary>The fields both merchant types must give (02 3.1.3): the trade title, the MERSIS and the trade registry numbers.</summary>
    private static readonly string[] MerchantFields =
    [
        nameof(CompanyIdentityInput.LegalName),
        nameof(CompanyIdentityInput.MersisNumber),
        nameof(CompanyIdentityInput.TradeRegistryNumber),
    ];

    /// <summary>The fields all three types must give (02 3.1.3; K-1568): the address, the contact details, the chamber and the conduct rules.</summary>
    private static readonly string[] CommonFields =
    [
        nameof(CompanyIdentityInput.ProvinceId),
        nameof(CompanyIdentityInput.District),
        nameof(CompanyIdentityInput.AddressLine),
        nameof(CompanyIdentityInput.Phone),
        nameof(CompanyIdentityInput.Email),
        nameof(CompanyIdentityInput.KepAddress),
        nameof(CompanyIdentityInput.ChamberName),
        nameof(CompanyIdentityInput.ProfessionalConductRules),
    ];

    /// <summary>The fields a type adds to the common set; empty for a type that is not chosen yet.</summary>
    public static IReadOnlyList<string> TypeFields(CompanyType? type)
    {
        return type switch
        {
            CompanyType.Tradesman => TradesmanFields,
            CompanyType.NaturalPersonMerchant or CompanyType.LegalEntity => MerchantFields,
            _ => [],
        };
    }

    /// <summary>Every field the type requires, in the order of the form: the type's own, then the common set. The type itself is not in the list.</summary>
    public static IReadOnlyList<string> RequiredFields(CompanyType? type) => [.. TypeFields(type), .. CommonFields];

    /// <summary>The required fields that are empty in the stored row, the type first when it is not chosen (02 3.1.5 condition 1).</summary>
    public static IReadOnlyList<string> MissingFields(Company company)
    {
        ArgumentNullException.ThrowIfNull(company);
        List<string> missing = [];
        if (company.CompanyType is null || !Enum.IsDefined(company.CompanyType.Value))
        {
            missing.Add(nameof(CompanyIdentityInput.CompanyType));
        }

        missing.AddRange(RequiredFields(company.CompanyType).Where(field => !IsFilled(company, field)));
        return missing;
    }

    /// <summary>
    /// Checks a save against the stored row. The refusals, in the form's order: the type outside the closed list, a chosen type that is cleared, a
    /// filled required field of the new type (or of the common set) that is emptied, the form of a filled e-mail, KEP address, telephone and link,
    /// and a province outside the closed list. An empty required field is not a refusal (02 3.1.3). <paramref name="input"/> is normalized.
    /// </summary>
    public static IReadOnlyList<FieldRejection> Check(Company current, CompanyIdentityInput input, bool provinceExists)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(input);
        List<FieldRejection> rejections = [];
        string typeField = nameof(CompanyIdentityInput.CompanyType);
        if (input.CompanyType is { } chosen && !Enum.IsDefined(chosen))
        {
            rejections.Add(new FieldRejection(typeField, FieldCodes.InvalidListValue));
        }
        else if (input.CompanyType is null && current.CompanyType is not null)
        {
            rejections.Add(new FieldRejection(typeField, FieldCodes.FilledFieldCleared));
        }

        foreach (string field in RequiredFields(input.CompanyType))
        {
            if (IsFilled(current, field) && !IsFilled(input, field))
            {
                rejections.Add(new FieldRejection(field, FieldCodes.FilledFieldCleared));
            }
        }

        AddFormRejection(rejections, nameof(CompanyIdentityInput.Email), input.Email, new InvalidEmailAttribute(), FieldCodes.InvalidEmail);
        AddFormRejection(rejections, nameof(CompanyIdentityInput.KepAddress), input.KepAddress, new InvalidEmailAttribute(), FieldCodes.InvalidEmail);
        AddFormRejection(rejections, nameof(CompanyIdentityInput.Phone), input.Phone, new InvalidPhoneAttribute(), FieldCodes.InvalidPhone);
        AddFormRejection(rejections, nameof(CompanyIdentityInput.EtbisVerificationUrl), input.EtbisVerificationUrl, new InvalidUrlAttribute(), FieldCodes.InvalidUrl);

        // The conduct rules are a link or a short text (02 3.1.3): text that starts like a link must be a whole link; any other text is the rules themselves.
        if (LooksLikeLink(input.ProfessionalConductRules))
        {
            AddFormRejection(
                rejections, nameof(CompanyIdentityInput.ProfessionalConductRules), input.ProfessionalConductRules, new InvalidUrlAttribute(), FieldCodes.InvalidUrl);
        }

        if (input.ProvinceId is not null && !provinceExists)
        {
            rejections.Add(new FieldRejection(nameof(CompanyIdentityInput.ProvinceId), FieldCodes.InvalidProvince));
        }

        return rejections;
    }

    /// <summary>
    /// The form every value is stored in (06 2.8.5, 2.5.1): white space around a text is dropped, an empty text is nothing, and a telephone is
    /// compacted to its ten digits - the text of a telephone that cannot be a number stays as typed so that the rule refuses it.
    /// </summary>
    public static CompanyIdentityInput Normalize(CompanyIdentityInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return input with
        {
            LegalName = Text(input.LegalName),
            TaxIdNumber = Text(input.TaxIdNumber),
            MersisNumber = Text(input.MersisNumber),
            TradeRegistryNumber = Text(input.TradeRegistryNumber),
            ProvinceId = input.ProvinceId == Guid.Empty ? null : input.ProvinceId,
            District = Text(input.District),
            AddressLine = Text(input.AddressLine),
            Phone = Phone(input.Phone),
            Email = Text(input.Email),
            KepAddress = Text(input.KepAddress),
            ChamberName = Text(input.ChamberName),
            BusinessName = Text(input.BusinessName),
            ProfessionalConductRules = Text(input.ProfessionalConductRules),
            EtbisVerificationUrl = Text(input.EtbisVerificationUrl),
        };
    }

    /// <summary>The empty value of an optional field is nothing (06 2.8.5); the text is trimmed.</summary>
    public static string? Text(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    /// <summary>The stored form of a telephone (06 2.5.1): the ten digits when the text is a number of the accepted forms, else the trimmed text.</summary>
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

    private static bool LooksLikeLink(string? value)
    {
        return value is not null
            && (value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || value.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
    }

    private static void AddFormRejection(
        List<FieldRejection> rejections, string field, string? value, FieldRuleAttribute rule, string code)
    {
        // The rule attributes work on a validation context (a rule's own IsValid(value) would pass no context); the value is its own instance here.
        if (value is not null && rule.GetValidationResult(value, new System.ComponentModel.DataAnnotations.ValidationContext(value)) is not null)
        {
            rejections.Add(new FieldRejection(field, code));
        }
    }

    private static bool IsFilled(Company company, string field)
    {
        return field switch
        {
            nameof(CompanyIdentityInput.LegalName) => company.LegalName is not null,
            nameof(CompanyIdentityInput.TaxIdNumber) => company.TaxIdNumber is not null,
            nameof(CompanyIdentityInput.MersisNumber) => company.MersisNumber is not null,
            nameof(CompanyIdentityInput.TradeRegistryNumber) => company.TradeRegistryNumber is not null,
            nameof(CompanyIdentityInput.ProvinceId) => company.ProvinceId is not null,
            nameof(CompanyIdentityInput.District) => company.District is not null,
            nameof(CompanyIdentityInput.AddressLine) => company.AddressLine is not null,
            nameof(CompanyIdentityInput.Phone) => company.Phone is not null,
            nameof(CompanyIdentityInput.Email) => company.Email is not null,
            nameof(CompanyIdentityInput.KepAddress) => company.KepAddress is not null,
            nameof(CompanyIdentityInput.ChamberName) => company.ChamberName is not null,
            nameof(CompanyIdentityInput.ProfessionalConductRules) => company.ProfessionalConductRules is not null,
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, "Not a required field of the identity."),
        };
    }

    private static bool IsFilled(CompanyIdentityInput input, string field)
    {
        return field switch
        {
            nameof(CompanyIdentityInput.LegalName) => input.LegalName is not null,
            nameof(CompanyIdentityInput.TaxIdNumber) => input.TaxIdNumber is not null,
            nameof(CompanyIdentityInput.MersisNumber) => input.MersisNumber is not null,
            nameof(CompanyIdentityInput.TradeRegistryNumber) => input.TradeRegistryNumber is not null,
            nameof(CompanyIdentityInput.ProvinceId) => input.ProvinceId is not null,
            nameof(CompanyIdentityInput.District) => input.District is not null,
            nameof(CompanyIdentityInput.AddressLine) => input.AddressLine is not null,
            nameof(CompanyIdentityInput.Phone) => input.Phone is not null,
            nameof(CompanyIdentityInput.Email) => input.Email is not null,
            nameof(CompanyIdentityInput.KepAddress) => input.KepAddress is not null,
            nameof(CompanyIdentityInput.ChamberName) => input.ChamberName is not null,
            nameof(CompanyIdentityInput.ProfessionalConductRules) => input.ProfessionalConductRules is not null,
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, "Not a required field of the identity."),
        };
    }
}
