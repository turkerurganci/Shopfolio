using System.Reflection;

using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.UnitTests.InstallationSettings;

/// <summary>
/// The rules of the company identity (T330; 02 3.1.1-3.1.4; 11 T330 criteria 1-5, KK1, KK3-KK5) as pure functions: which fields a type requires, which are
/// missing, what a save may not do, the forms of the contact fields and the shape of the module's surface (no second company, no choice of language
/// or currency, no separate notification address). No database is needed; the integration tests run the same rules through the real module.
/// </summary>
public sealed class CompanyIdentityRulesTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 6, 9, 0, 0, TimeSpan.Zero);

    private static readonly Guid Province = new("30000000-0000-0000-0000-000000000001");

    private static readonly string[] CommonFields =
    [
        "ProvinceId", "District", "AddressLine", "Phone", "Email", "KepAddress", "ChamberName", "ProfessionalConductRules",
    ];

    public static TheoryData<CompanyType, string[]> OwnFieldsByType => new()
    {
        { CompanyType.Tradesman, ["LegalName", "TaxIdNumber"] },
        { CompanyType.NaturalPersonMerchant, ["LegalName", "MersisNumber", "TradeRegistryNumber"] },
        { CompanyType.LegalEntity, ["LegalName", "MersisNumber", "TradeRegistryNumber"] },
    };

    [Fact]
    public void CompanyType_HasExactlyTheThreeValues_AndNoOtherIsDefined()
    {
        Assert.Equal(["Tradesman", "NaturalPersonMerchant", "LegalEntity"], Enum.GetNames<CompanyType>());
        Assert.False(Enum.IsDefined((CompanyType)3));
    }

    [Fact]
    public void MissingFields_OfARowThatIsBornEmpty_ListTheTypeFirst_AndNoTypeFieldYet()
    {
        Company company = Company.Create(Now);

        Assert.Equal(["CompanyType", .. CommonFields], CompanyIdentityRules.MissingFields(company));
    }

    [Theory]
    [MemberData(nameof(OwnFieldsByType))]
    public void MissingFields_OfAnEmptyRowOfAType_ListTheTypesOwnFieldsThenTheCommonSet(CompanyType type, string[] own)
    {
        Company company = Company.Create(Now);
        company.CompanyType = type;

        Assert.Equal([.. own, .. CommonFields], CompanyIdentityRules.MissingFields(company));
    }

    [Theory]
    [MemberData(nameof(OwnFieldsByType))]
    public void MissingFields_OfACompleteRowOfAType_AreEmpty_WithoutTheOptionalFields(CompanyType type, string[] own)
    {
        Company company = Complete(type);

        Assert.Empty(CompanyIdentityRules.MissingFields(company));
        Assert.Null(company.BusinessName);
        Assert.Null(company.EtbisVerificationUrl);
        Assert.DoesNotContain("BusinessName", CompanyIdentityRules.RequiredFields(type));
        Assert.DoesNotContain("EtbisVerificationUrl", CompanyIdentityRules.RequiredFields(type));
        Assert.Equal(own.Length + CommonFields.Length, CompanyIdentityRules.RequiredFields(type).Count);
    }

    [Fact]
    public void MissingFields_TradesmanDoesNotWaitForTheMerchantNumbers_AndTheMerchantDoesNotWaitForTheTaxNumber()
    {
        Company tradesman = Complete(CompanyType.Tradesman);
        tradesman.MersisNumber = null;
        tradesman.TradeRegistryNumber = null;
        Company merchant = Complete(CompanyType.LegalEntity);
        merchant.TaxIdNumber = null;

        Assert.Empty(CompanyIdentityRules.MissingFields(tradesman));
        Assert.Empty(CompanyIdentityRules.MissingFields(merchant));
    }

    [Fact]
    public void MissingFields_AnEmptiedKepAddress_IsListed_ForEveryType()
    {
        foreach (CompanyType type in Enum.GetValues<CompanyType>())
        {
            Company company = Complete(type);
            company.KepAddress = null;

            Assert.Equal(["KepAddress"], CompanyIdentityRules.MissingFields(company));
        }
    }

    [Fact]
    public void Check_AnEmptyRequiredFieldOfAnEmptyRow_IsNotARefusal()
    {
        Company company = Company.Create(Now);

        Assert.Empty(CompanyIdentityRules.Check(company, Input(CompanyType.LegalEntity), provinceExists: true));
        Assert.Empty(CompanyIdentityRules.Check(company, Input(null), provinceExists: true));
    }

    [Fact]
    public void Check_KepAddressEmptyWhileItWasEmpty_IsAccepted_AndFilledKepAddressCannotBeEmptied()
    {
        Company company = Complete(CompanyType.Tradesman);
        company.KepAddress = null;
        CompanyIdentityInput stillEmpty = InputOf(Complete(CompanyType.Tradesman)) with { KepAddress = null };

        Assert.Empty(CompanyIdentityRules.Check(company, stillEmpty, provinceExists: true));

        company.KepAddress = "firma@hs01.kep.tr";
        IReadOnlyList<Shopfolio.Modules.Settings.Contracts.FieldRejection> refused = CompanyIdentityRules.Check(company, stillEmpty, provinceExists: true);

        Assert.Equal([new Shopfolio.Modules.Settings.Contracts.FieldRejection("KepAddress", FieldCodes.FilledFieldCleared)], refused);
    }

    [Theory]
    [MemberData(nameof(OwnFieldsByType))]
    public void Check_EveryFilledRequiredFieldOfTheType_CannotBeEmptied(CompanyType type, string[] own)
    {
        Company company = Complete(type);
        string[] required = [.. own, .. CommonFields];

        foreach (string field in required)
        {
            CompanyIdentityInput input = Without(InputOf(company), field);

            IReadOnlyList<Shopfolio.Modules.Settings.Contracts.FieldRejection> refused = CompanyIdentityRules.Check(company, input, provinceExists: true);

            Assert.Equal([new Shopfolio.Modules.Settings.Contracts.FieldRejection(field, FieldCodes.FilledFieldCleared)], refused);
        }
    }

    [Fact]
    public void Check_AChosenTypeCannotBeCleared_ButChangingItIsNeverRefused()
    {
        Company company = Complete(CompanyType.Tradesman);
        CompanyIdentityInput same = InputOf(company);

        Assert.Equal(
            [new Shopfolio.Modules.Settings.Contracts.FieldRejection("CompanyType", FieldCodes.FilledFieldCleared)],
            CompanyIdentityRules.Check(company, same with { CompanyType = null }, provinceExists: true));

        foreach (CompanyType other in Enum.GetValues<CompanyType>())
        {
            // The new type's own fields are empty in the input, but they were empty in the row too: nothing filled is emptied. LegalName stays.
            Assert.Empty(CompanyIdentityRules.Check(company, same with { CompanyType = other }, provinceExists: true));
        }
    }

    [Fact]
    public void Check_ATypeOutsideTheClosedList_IsRefused()
    {
        Company company = Company.Create(Now);

        Assert.Equal(
            [new Shopfolio.Modules.Settings.Contracts.FieldRejection("CompanyType", FieldCodes.InvalidListValue)],
            CompanyIdentityRules.Check(company, Input((CompanyType)42), provinceExists: true));
    }

    [Fact]
    public void Check_AProvinceOutsideTheClosedList_IsRefused()
    {
        Company company = Company.Create(Now);

        Assert.Equal(
            [new Shopfolio.Modules.Settings.Contracts.FieldRejection("ProvinceId", FieldCodes.InvalidProvince)],
            CompanyIdentityRules.Check(company, Input(CompanyType.Tradesman) with { ProvinceId = Province }, provinceExists: false));
    }

    [Fact]
    public void Check_OptionalFieldsEmpty_PassAtEveryType_AndFilledOnesAreStored()
    {
        Company company = Complete(CompanyType.LegalEntity);
        CompanyIdentityInput withOptional = InputOf(company) with { BusinessName = "Marka", EtbisVerificationUrl = "https://etbis.example.com/dogrula/1" };
        CompanyIdentityInput withoutOptional = InputOf(company) with { BusinessName = null, EtbisVerificationUrl = null };

        Assert.Empty(CompanyIdentityRules.Check(company, withOptional, provinceExists: true));
        Assert.Empty(CompanyIdentityRules.Check(company, withoutOptional, provinceExists: true));
    }

    [Theory]
    [InlineData("Email", "not-an-email", "InvalidEmail")]
    [InlineData("KepAddress", "bir kep", "InvalidEmail")]
    [InlineData("Phone", "12345", "InvalidPhone")]
    [InlineData("EtbisVerificationUrl", "ftp://etbis", "InvalidUrl")]
    [InlineData("EtbisVerificationUrl", "etbis.gov.tr/dogrula", "InvalidUrl")]
    [InlineData("ProfessionalConductRules", "https://", "InvalidUrl")]
    [InlineData("ProfessionalConductRules", "http://bosluk var.com", "InvalidUrl")]
    public void Check_AWronglyFormedContactValue_IsRefusedWithItsCode(string field, string value, string code)
    {
        Company company = Company.Create(Now);
        CompanyIdentityInput input = SetField(Input(CompanyType.Tradesman), field, value);

        Assert.Contains(new Shopfolio.Modules.Settings.Contracts.FieldRejection(field, code), CompanyIdentityRules.Check(company, CompanyIdentityRules.Normalize(input), provinceExists: true));
    }

    [Theory]
    [InlineData("ProfessionalConductRules", "https://www.tobb.org.tr/etik-kurallar")]
    [InlineData("ProfessionalConductRules", "Esnaf ve Sanatkarlar Odası etik kuralları")]
    [InlineData("ProfessionalConductRules", "httpd kuralları")]
    [InlineData("Email", "iletisim@firma.com.tr")]
    [InlineData("KepAddress", "firma@hs01.kep.tr")]
    [InlineData("EtbisVerificationUrl", "https://etbis.gov.tr/dogrula/123")]
    public void Check_AWellFormedValue_IsAccepted(string field, string value)
    {
        Company company = Company.Create(Now);
        CompanyIdentityInput input = SetField(Input(CompanyType.Tradesman), field, value);

        Assert.Empty(CompanyIdentityRules.Check(company, CompanyIdentityRules.Normalize(input), provinceExists: true));
    }

    [Theory]
    [InlineData("5321234567")]
    [InlineData("0532 123 45 67")]
    [InlineData("+90 532 123 45 67")]
    [InlineData("(0532) 123-45-67")]
    [InlineData("  532 123 4567  ")]
    public void Phone_InTheAcceptedForms_IsStoredAsTenDigits_AndPassesTheRule(string typed)
    {
        CompanyIdentityInput normalized = CompanyIdentityRules.Normalize(Input(CompanyType.Tradesman) with { Phone = typed });

        Assert.Equal("5321234567", normalized.Phone);
        Assert.Empty(CompanyIdentityRules.Check(Company.Create(Now), normalized, provinceExists: true));
    }

    [Fact]
    public void Normalize_TrimsTexts_AndTurnsBlankIntoNothing_AndEmptyGuidIntoNoProvince()
    {
        CompanyIdentityInput normalized = CompanyIdentityRules.Normalize(
            Input(CompanyType.Tradesman) with { LegalName = "  Ayşe Yılmaz ", District = "   ", BusinessName = "", ProvinceId = Guid.Empty });

        Assert.Equal("Ayşe Yılmaz", normalized.LegalName);
        Assert.Null(normalized.District);
        Assert.Null(normalized.BusinessName);
        Assert.Null(normalized.ProvinceId);
    }

    [Fact]
    public void IdentityInput_HasNoSeparateNotificationAddress_NoLanguage_NoMarket_NoCurrency_NoCompanyChoice()
    {
        string[] members = [.. typeof(CompanyIdentityInput).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name).Where(n => n != "EqualityContract")];

        Assert.Equal(
            [
                "CompanyType", "LegalName", "TaxIdNumber", "MersisNumber", "TradeRegistryNumber", "ProvinceId", "District", "AddressLine", "Phone", "Email",
                "KepAddress", "ChamberName", "BusinessName", "ProfessionalConductRules", "EtbisVerificationUrl",
            ],
            members);
        Assert.DoesNotContain(members, m => m.Contains("Notification", StringComparison.OrdinalIgnoreCase) || m.Contains("Language", StringComparison.OrdinalIgnoreCase)
            || m.Contains("Currency", StringComparison.OrdinalIgnoreCase) || m.Contains("Market", StringComparison.OrdinalIgnoreCase) || m.Contains("Country", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SettingsModule_HasNoMemberThatAddsSelectsSwitchesOrDeletesACompany_OrChoosesALanguageOrCurrency()
    {
        string[] names = [.. typeof(ISettingsModule).GetMethods().Select(m => m.Name)];

        Assert.NotEmpty(names);
        string[] forbidden = ["Add", "Create", "Select", "Switch", "Delete", "Remove", "Language", "Currency", "Market", "Locale"];
        foreach (string name in names)
        {
            // "Get...Company..." reads are fine; a company is never added, chosen, switched or removed, and nothing here picks a language or a currency.
            // The name is read word by word ("SaveReturnAddressAsync" has the word "Address", not "Add").
            string[] words = System.Text.RegularExpressions.Regex.Matches(name, "[A-Z][a-z0-9]*").Select(m => m.Value).ToArray();
            Assert.DoesNotContain(forbidden, word => words.Contains(word, StringComparer.Ordinal));
        }

        Assert.DoesNotContain(typeof(ISettingsModule).GetMethods(), m => m.GetParameters().Any(p => p.ParameterType == typeof(Guid) && p.Name!.Contains("company", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void RequiredFields_AreEveryOneAMemberOfTheInput_SoTheMissingNamesAreTheFormsNames()
    {
        string[] members = [.. typeof(CompanyIdentityInput).GetProperties().Select(p => p.Name)];

        foreach (CompanyType type in Enum.GetValues<CompanyType>())
        {
            Assert.All(CompanyIdentityRules.RequiredFields(type), field => Assert.Contains(field, members));
        }
    }

    private static Company Complete(CompanyType type)
    {
        Company company = Company.Create(Now);
        company.CompanyType = type;
        company.LegalName = "Yılmaz Ticaret";
        company.TaxIdNumber = type == CompanyType.Tradesman ? "1234567890" : null;
        company.MersisNumber = type == CompanyType.Tradesman ? null : "0123456789012345";
        company.TradeRegistryNumber = type == CompanyType.Tradesman ? null : "123456";
        company.ProvinceId = Province;
        company.District = "Kadıköy";
        company.AddressLine = "Moda Cad. 1";
        company.Phone = "5321234567";
        company.Email = "iletisim@firma.com.tr";
        company.KepAddress = "firma@hs01.kep.tr";
        company.ChamberName = "İstanbul Esnaf ve Sanatkarlar Odası";
        company.ProfessionalConductRules = "https://www.tobb.org.tr/etik-kurallar";
        return company;
    }

    private static CompanyIdentityInput Input(CompanyType? type)
    {
        return new CompanyIdentityInput(type, null, null, null, null, null, null, null, null, null, null, null, null, null, null);
    }

    private static CompanyIdentityInput InputOf(Company company)
    {
        return new CompanyIdentityInput(
            company.CompanyType, company.LegalName, company.TaxIdNumber, company.MersisNumber, company.TradeRegistryNumber, company.ProvinceId, company.District,
            company.AddressLine, company.Phone, company.Email, company.KepAddress, company.ChamberName, company.BusinessName, company.ProfessionalConductRules,
            company.EtbisVerificationUrl);
    }

    private static CompanyIdentityInput Without(CompanyIdentityInput input, string field) => SetField(input, field, null);

    private static CompanyIdentityInput SetField(CompanyIdentityInput input, string field, string? value)
    {
        return field switch
        {
            "LegalName" => input with { LegalName = value },
            "TaxIdNumber" => input with { TaxIdNumber = value },
            "MersisNumber" => input with { MersisNumber = value },
            "TradeRegistryNumber" => input with { TradeRegistryNumber = value },
            "ProvinceId" => input with { ProvinceId = value is null ? null : Province },
            "District" => input with { District = value },
            "AddressLine" => input with { AddressLine = value },
            "Phone" => input with { Phone = value },
            "Email" => input with { Email = value },
            "KepAddress" => input with { KepAddress = value },
            "ChamberName" => input with { ChamberName = value },
            "ProfessionalConductRules" => input with { ProfessionalConductRules = value },
            "EtbisVerificationUrl" => input with { EtbisVerificationUrl = value },
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, null),
        };
    }
}
