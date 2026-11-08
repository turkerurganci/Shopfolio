using System.Reflection;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Membership.Contracts;

namespace Shopfolio.UnitTests.Membership;

/// <summary>
/// The address rules as pure functions (T344; 02 3.14.1-3.14.6; 11 T344 criteria 3-9, KK3-KK6, KK8, KK9): the four required fields, the closed province list, the telephone and
/// the addresses a basket asks for by its item types. No database is needed; the integration tests run the same rules through the real module and a real PostgreSQL.
/// </summary>
public sealed class AddressFieldRulesTests
{
    private static readonly Guid Ankara = new("30000000-0000-0000-0000-000000000006");

    private static readonly Guid Izmir = new("30000000-0000-0000-0000-000000000035");

    private static readonly Guid NotAProvince = new("30000000-0000-0000-0000-0000000000ff");

    private static readonly HashSet<Guid> Provinces = [Ankara, Izmir];

    private static readonly string[] AddressFieldNames = ["RecipientName", "ProvinceId", "District", "AddressLine"];

    // ---- KK5, KK6: the addresses a basket asks for by the types of its items ----

    [Theory]
    [InlineData(new[] { ProductType.Physical })]
    [InlineData(new[] { ProductType.Physical, ProductType.Digital })]
    [InlineData(new[] { ProductType.Service, ProductType.Physical, ProductType.Service })]
    public void RequirementOf_ABasketWithAPhysicalItem_AsksShippingAndBilling_AndOffersTheSameBox(ProductType[] types)
    {
        AddressRequirement need = AddressFieldRules.RequirementOf(types);

        Assert.True(need.AsksShipping);
        Assert.True(need.AsksBilling);
        Assert.True(need.OffersBillingSameAsShipping);
    }

    [Theory]
    [InlineData(new[] { ProductType.Digital })]
    [InlineData(new[] { ProductType.Service })]
    [InlineData(new[] { ProductType.Digital, ProductType.Service })]
    public void RequirementOf_ADigitalAndServiceBasket_ShowsNoShippingField_AndAsksBillingAlone(ProductType[] types)
    {
        AddressRequirement need = AddressFieldRules.RequirementOf(types);

        Assert.False(need.AsksShipping);
        Assert.True(need.AsksBilling);
        Assert.False(need.OffersBillingSameAsShipping);
    }

    [Fact]
    public void RequirementOf_EveryProductType_IsDecided_AndNoItemAtAllIsNoOrder()
    {
        foreach (ProductType type in Enum.GetValues<ProductType>())
        {
            Assert.Equal(type == ProductType.Physical, AddressFieldRules.RequirementOf([type]).AsksShipping);
        }

        Assert.Throws<ArgumentException>(() => AddressFieldRules.RequirementOf([]));
        Assert.Throws<ArgumentOutOfRangeException>(() => AddressFieldRules.RequirementOf([(ProductType)99]));
    }

    // ---- KK8: each of the four fields is required ----

    [Fact]
    public void CheckAddress_AllFourFilled_ReturnsTheTrimmedValues()
    {
        List<MembershipFieldError> errors = [];

        AddressValues? values = AddressFieldRules.CheckAddress("", "  Ali Veli ", Ankara, " Çankaya ", " Atatürk Bulvarı 12/3 ", Provinces, errors);

        Assert.Empty(errors);
        Assert.Equal(new AddressValues("Ali Veli", Ankara, "Çankaya", "Atatürk Bulvarı 12/3"), values);
    }

    [Theory]
    [InlineData(null, "ok", "ok", "RecipientName", ErrorCode.FieldRequired)]
    [InlineData("   ", "ok", "ok", "RecipientName", ErrorCode.FieldRequired)]
    [InlineData("ok", null, "ok", "District", ErrorCode.FieldRequired)]
    [InlineData("ok", "", "ok", "District", ErrorCode.FieldRequired)]
    [InlineData("ok", "ok", null, "AddressLine", ErrorCode.FieldRequired)]
    [InlineData("ok", "ok", " ", "AddressLine", ErrorCode.FieldRequired)]
    public void CheckAddress_OneTextFieldEmpty_IsRefusedAsThatField(string? recipient, string? district, string? line, string field, ErrorCode code)
    {
        List<MembershipFieldError> errors = [];

        AddressValues? values = AddressFieldRules.CheckAddress("", recipient, Ankara, district, line, Provinces, errors);

        Assert.Null(values);
        Assert.Equal(new MembershipFieldError(field, code), Assert.Single(errors));
    }

    [Fact]
    public void CheckAddress_NoProvince_IsRequired_AndAnEmptyKeyIsNoProvince()
    {
        List<MembershipFieldError> errors = [];

        Assert.Null(AddressFieldRules.CheckAddress("", "a", null, "b", "c", Provinces, errors));
        Assert.Null(AddressFieldRules.CheckAddress("", "a", Guid.Empty, "b", "c", Provinces, errors));

        Assert.All(errors, e => Assert.Equal(new MembershipFieldError("ProvinceId", ErrorCode.FieldRequired), e));
        Assert.Equal(2, errors.Count);
    }

    [Fact]
    public void CheckAddress_AProvinceOutsideTheClosedList_IsRefusedAsInvalidProvince()
    {
        List<MembershipFieldError> errors = [];

        Assert.Null(AddressFieldRules.CheckAddress("", "a", NotAProvince, "b", "c", Provinces, errors));

        Assert.Equal(new MembershipFieldError("ProvinceId", ErrorCode.InvalidProvince), Assert.Single(errors));
    }

    [Fact]
    public void CheckAddress_EveryFieldEmpty_ListsAllFourTogether_NamedWithTheFormsPrefix()
    {
        List<MembershipFieldError> errors = [];

        Assert.Null(AddressFieldRules.CheckAddress(AddressFieldRules.BillingPrefix, null, null, null, null, Provinces, errors));

        Assert.Equal(AddressFieldNames.Select(n => "Billing" + n), errors.Select(e => e.Field));
        Assert.All(errors, e => Assert.Equal(ErrorCode.FieldRequired, e.Code));
    }

    [Fact]
    public void CheckAddress_TheFieldNamesAreTheFormsNames_OfTheBookAndOfTheOrderRequest()
    {
        Assert.All(AddressFieldNames, name => Assert.NotNull(typeof(AddressBookInput).GetProperty(name)));
        Assert.All(AddressFieldNames, name => Assert.NotNull(typeof(OrderAddressRequest).GetProperty(AddressFieldRules.ShippingPrefix + name)));
        Assert.All(AddressFieldNames, name => Assert.NotNull(typeof(OrderAddressRequest).GetProperty(AddressFieldRules.BillingPrefix + name)));
        Assert.NotNull(typeof(OrderAddressRequest).GetProperty(nameof(OrderAddressRequest.ShippingPhone)));
    }

    // ---- KK9: the telephone ----

    [Theory]
    [InlineData("5321234567")]
    [InlineData("0532 123 45 67")]
    [InlineData("+90 (532) 123-45-67")]
    [InlineData("  05321234567 ")]
    public void TryStorePhone_ATurkishNumber_IsStoredAsTenDigits(string typed)
    {
        List<MembershipFieldError> errors = [];

        bool ok = AddressFieldRules.TryStorePhone("Phone", typed, required: true, errors, out string? stored);

        Assert.True(ok);
        Assert.Empty(errors);
        Assert.Equal("5321234567", stored);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("abc")]
    [InlineData("532123456")]
    [InlineData("53212345678")]
    [InlineData("0000000000")]
    public void TryStorePhone_NotATurkishNumber_IsInvalidPhone_RequiredOrNot(string typed)
    {
        foreach (bool required in new[] { true, false })
        {
            List<MembershipFieldError> errors = [];

            bool ok = AddressFieldRules.TryStorePhone("Phone", typed, required, errors, out string? stored);

            Assert.False(ok);
            Assert.Null(stored);
            Assert.Equal(new MembershipFieldError("Phone", ErrorCode.InvalidPhone), Assert.Single(errors));
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TryStorePhone_Empty_IsNothingInTheBook_AndRequiredOnAShippingAddress(string? typed)
    {
        List<MembershipFieldError> optional = [];
        Assert.True(AddressFieldRules.TryStorePhone("Phone", typed, required: false, optional, out string? stored));
        Assert.Null(stored);
        Assert.Empty(optional);

        List<MembershipFieldError> required = [];
        Assert.False(AddressFieldRules.TryStorePhone("ShippingPhone", typed, required: true, required, out _));
        Assert.Equal(new MembershipFieldError("ShippingPhone", ErrorCode.FieldRequired), Assert.Single(required));
    }

    // ---- KK9: no postal code, no national identity number, no billing telephone ----

    [Fact]
    public void TheAddressShapes_HaveNoPostalCode_NoIdentityNumber_AndTheBillingAddressNoPhone()
    {
        Type[] shapes =
        [
            typeof(AddressBookInput), typeof(AddressBookEntry), typeof(OrderAddressRequest), typeof(ShippingAddressSnapshot), typeof(BillingAddressSnapshot), typeof(AddressValues),
        ];
        string[] forbidden = ["postal", "zip", "postcode", "identity", "nationalid", "tckn", "tcno", "tckimlik", "taxid"];
        foreach (Type shape in shapes)
        {
            foreach (PropertyInfo property in shape.GetProperties())
            {
                Assert.DoesNotContain(forbidden, word => property.Name.Contains(word, StringComparison.OrdinalIgnoreCase));
            }
        }

        Assert.DoesNotContain(typeof(BillingAddressSnapshot).GetProperties(), p => p.Name.Contains("Phone", StringComparison.Ordinal));
        Assert.DoesNotContain(typeof(OrderAddressRequest).GetProperties(), p => p.Name.StartsWith("Billing", StringComparison.Ordinal) && p.Name.Contains("Phone", StringComparison.Ordinal));
        Assert.Contains(typeof(ShippingAddressSnapshot).GetProperties(), p => p.Name == nameof(ShippingAddressSnapshot.Phone));
    }

    [Fact]
    public void Text_DropsWhiteSpace_AndAnEmptyTextIsNothing()
    {
        Assert.Null(AddressFieldRules.Text(null));
        Assert.Null(AddressFieldRules.Text("  "));
        Assert.Equal("Ev", AddressFieldRules.Text(" Ev "));
    }
}
