using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Settings.Contracts;

/// <summary>
/// The company identity as the identity form shows it (06 4.1; 02 3.1.2): every field of the single company row and the form's one version. The
/// type is empty in a new installation (04 9.16.11). Immutable; the caller never receives the record the module writes.
/// </summary>
public sealed record CompanyIdentity(
    int Version,
    CompanyType? CompanyType,
    string? LegalName,
    string? TaxIdNumber,
    string? MersisNumber,
    string? TradeRegistryNumber,
    Guid? ProvinceId,
    string? District,
    string? AddressLine,
    string? Phone,
    string? Email,
    string? KepAddress,
    string? ChamberName,
    string? BusinessName,
    string? ProfessionalConductRules,
    string? EtbisVerificationUrl);

/// <summary>
/// The values the identity form sends (07 9.16.2 <c>SaveCompanyInput</c>; 04 7.2.19) without the version, which travels apart. The language, the
/// market and the currency are not inputs: the installation has one of each (02 3.1.1, 12.1.1), and there is no field to choose a company either.
/// The telephone may come in any of the accepted forms; it is stored as ten digits (06 2.5.1).
/// </summary>
public sealed record CompanyIdentityInput(
    CompanyType? CompanyType,
    string? LegalName,
    string? TaxIdNumber,
    string? MersisNumber,
    string? TradeRegistryNumber,
    Guid? ProvinceId,
    string? District,
    string? AddressLine,
    string? Phone,
    string? Email,
    string? KepAddress,
    string? ChamberName,
    string? BusinessName,
    string? ProfessionalConductRules,
    string? EtbisVerificationUrl);

/// <summary>
/// The fields of the identity the sales gate's first condition still waits for (02 3.1.5 condition 1): the names are those of
/// <see cref="CompanyIdentityInput"/>'s members, in the order the form lists them; the type itself comes first when it is not chosen. Empty when
/// the identity is complete for its type. The optional fields (business name, ETBIS verification) are never listed (02 3.1.3, 3.1.4).
/// </summary>
public sealed record CompanyIdentityCompleteness(IReadOnlyList<string> MissingFields)
{
    public bool IsComplete => MissingFields.Count == 0;
}

/// <summary>
/// What a visitor reads under the "İletişim" heading (02 3.1.4; 10 KP-35): the contact details and the legal identity of the company's type - the
/// tax number for a tradesman, the MERSIS and trade registry numbers for the two merchant types - the chamber, the conduct rules, and, when
/// they are filled, the business name or registered brand and the ETBIS verification link that the band points to. A field of another type is
/// not shown even if it holds a value. The province is given by name. Nothing here is secret and no administrator session is needed to read it.
/// </summary>
public sealed record LegalIdentity(
    CompanyType? CompanyType,
    string? LegalName,
    string? TaxIdNumber,
    string? MersisNumber,
    string? TradeRegistryNumber,
    string? ProvinceName,
    string? District,
    string? AddressLine,
    string? Phone,
    string? Email,
    string? KepAddress,
    string? ChamberName,
    string? ProfessionalConductRules,
    string? BusinessName,
    string? EtbisVerificationUrl);

/// <summary>A return address (02 3.1.7; 06 4.2): the four required parts and the optional telephone, ten digits. The value a declaration copies as it is.</summary>
public sealed record ReturnAddressSnapshot(string RecipientName, Guid ProvinceId, string District, string AddressLine, string? Phone);

/// <summary>
/// The return address section of the company settings (06 4.2): the address when all four required parts are filled - null while the setting is
/// empty, which keeps the sales gate closed (02 3.1.5 condition 4) - and the section's own version (06 2.6.3).
/// </summary>
public sealed record ReturnAddressSection(ReturnAddressSnapshot? Address, int Version)
{
    public bool IsComplete => Address is not null;
}

/// <summary>The values the return address form sends (04 7.2.21.6, 7.2.21.7) without the section version, which travels apart.</summary>
public sealed record ReturnAddressInput(string? RecipientName, Guid? ProvinceId, string? District, string? AddressLine, string? Phone);

/// <summary>One refused field of a save: the field's name (the member of the input record) and the code of 07 6.2.2 (<c>FieldCodes</c>); a value outside a fence also says the side it crossed and the fence value (02 4.1.5; <c>PeriodFences</c>), which the page turns into its message.</summary>
public sealed record FieldRejection(string Field, string Code, FenceSide Side = FenceSide.Within, int Fence = 0);
