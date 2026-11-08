using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// There is one delivery way, the carrier (T350 criteria 3, 4; KK3, KK4; 02 3.20.2, 3.20.3): no delivery option, delivery method, store pickup or named option exists as a type, member,
/// enum value, parameter, table or column of the application, an order goes as one parcel and carries one tracking number, and there is no shipment or parcel table. Like the sales model
/// shape scan, the scan reads names cut into words, so a property, a method, a nested type, an enum member, a parameter, a table and a column are all found in whatever spelling.
/// </summary>
public sealed class SingleDeliveryWayTests
{
    private static readonly string[][] ForbiddenPhrases =
    [
        ["delivery", "option"], ["delivery", "options"], ["delivery", "method"], ["delivery", "methods"], ["delivery", "type"], ["delivery", "mode"],
        ["shipping", "option"], ["shipping", "options"], ["shipping", "method"], ["shipping", "methods"], ["shipping", "type"], ["shipping", "mode"],
        ["pickup"], ["pick", "up"], ["store", "pickup"], ["in", "store"], ["click", "collect"], ["click", "and", "collect"], ["collection", "point"], ["pickup", "point"],
        ["parcel"], ["parcels"], ["package"], ["packages"], ["shipment", "item"], ["shipment", "items"], ["split", "shipment"], ["partial", "shipment"],
    ];

    private static readonly string[] ForbiddenTableWords = ["shipment", "shipments", "parcel", "parcels", "package", "packages"];

    [Fact]
    public void NoTypeMemberEnumValueOrParameterOfTheApplication_IsADeliveryOptionAStorePickupOrAParcel_KK3()
    {
        List<string> findings = [.. SalesModelShape.NamesOf(typeof(OrderStatus).Assembly).SelectMany(n => FindingsIn(n.Name).Select(f => $"{n.Owner}: {f}"))];

        Assert.Empty(findings);
    }

    [Fact]
    public void TheModelHasNoTableOrColumnOfADeliveryOptionAPickupOrAParcel_KK3()
    {
        IModel model = NewModel();
        List<string> findings = [];
        foreach (IEntityType entity in model.GetEntityTypes())
        {
            string table = entity.GetTableName() ?? entity.ShortName();
            findings.AddRange(FindingsIn(table).Select(f => $"{table}: {f}"));
            findings.AddRange(SalesModelShape.Words(table).Where(ForbiddenTableWords.Contains).Select(w => $"{table}: {w}"));
            foreach (IProperty property in entity.GetProperties())
            {
                string column = property.GetColumnName();
                findings.AddRange(FindingsIn(column).Select(f => $"{table}.{column}: {f}"));
            }
        }

        Assert.Empty(findings);
    }

    [Fact]
    public void TheOrder_CarriesExactlyOneTrackingNumberColumn_AndNoOtherTableDoes_KK4()
    {
        IModel model = NewModel();
        List<string> withTracking =
        [
            .. model.GetEntityTypes().SelectMany(e => e.GetProperties().Select(p => (Table: e.GetTableName(), Column: p.GetColumnName())))
                .Where(c => SalesModelShape.Words(c.Column).Contains("tracking", StringComparer.Ordinal))
                .Select(c => $"{c.Table}.{c.Column}"),
        ];

        Assert.Equal(["orders.tracking_number"], withTracking);
    }

    [Fact]
    public void TheOrdersCarrierColumns_AreTheCompanyTheOtherNameTheNumberAndTheOwnVehicleDeclaration_AndNothingElse_KK4()
    {
        IEntityType order = NewModel().GetEntityTypes().Single(e => e.GetTableName() == "orders");

        string[] shippingColumns = [.. order.GetProperties().Select(p => p.GetColumnName()).Where(c => c.Contains("carrier", StringComparison.Ordinal) || c.Contains("tracking", StringComparison.Ordinal) || c.Contains("vehicle", StringComparison.Ordinal))];

        Assert.Equal(["carrier", "is_own_vehicle_delivery", "other_carrier_name", "tracking_number"], shippingColumns.Order(StringComparer.Ordinal));
    }

    [Theory]
    [InlineData("DeliveryOption", "delivery option")]
    [InlineData("delivery_options", "delivery options")]
    [InlineData("ShippingMethod", "shipping method")]
    [InlineData("StorePickup", "store pickup")]
    [InlineData("pickUpPoint", "pick up")]
    [InlineData("IsClickAndCollect", "click and collect")]
    [InlineData("Parcel", "parcel")]
    [InlineData("ParcelId", "parcel")]
    [InlineData("package_count", "package")]
    [InlineData("SplitShipment", "split shipment")]
    public void TheScan_FindsEachForbiddenFormInItsSpelling(string name, string phrase)
    {
        Assert.Contains(phrase, FindingsIn(name));
    }

    [Theory]
    [InlineData("TrackingNumber")]
    [InlineData("ShippingFee")]
    [InlineData("ShippingPeriod")]
    [InlineData("ShipOrderRequest")]
    [InlineData("DeliveryDate")]
    [InlineData("DeliveryProvinces")]
    [InlineData("IsOwnVehicleDelivery")]
    [InlineData("shipping_address_line")]
    [InlineData("order_status")]
    public void TheScan_DoesNotMistakeAnOrdinaryNameForAForbiddenOne(string name)
    {
        Assert.Empty(FindingsIn(name));
    }

    private static List<string> FindingsIn(string name)
    {
        List<string> words = SalesModelShape.Words(name);
        List<string> findings = [];
        foreach (string[] phrase in ForbiddenPhrases)
        {
            for (int start = 0; start + phrase.Length <= words.Count; start++)
            {
                if (words.Skip(start).Take(phrase.Length).SequenceEqual(phrase, StringComparer.Ordinal))
                {
                    findings.Add(string.Join(' ', phrase));
                    break;
                }
            }
        }

        return findings;
    }

    private static IModel NewModel()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, "Host=127.0.0.1;Database=never_opened");
        using AppDbContext context = new(builder.Options);
        return context.GetService<IDesignTimeModel>().Model;
    }
}
