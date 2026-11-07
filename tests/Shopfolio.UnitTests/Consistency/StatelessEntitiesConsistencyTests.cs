using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// The entities that carry no state machine (T315 KK8; 03 1.1, 1.10): the order item, the discount, the coupon, the stock reservation, the account
/// (customer and administrator), the invitation, the session and the company settings (the sales gate is a rule over four conditions, not a status)
/// have no property of the five status enums and none whose name ends in <c>Status</c>. Read from the finished model, so a field added later fails here.
/// The entities that do carry a status are named too, so the check cannot pass over an empty model.
/// </summary>
[Trait("Consistency", "true")]
public sealed class StatelessEntitiesConsistencyTests
{
    private static readonly string[] Stateless =
        ["OrderItem", "Discount", "Coupon", "StockReservation", "Customer", "Admin", "AdminInvitation", "Session", "CompanySettings"];

    private static readonly Type[] StatusEnums =
        [typeof(OrderStatus), typeof(PaymentStatus), typeof(PublishStatus), typeof(DefectClaimStatus), typeof(ContactRequestStatus)];

    [Fact]
    public void TheEntitiesWithoutAStateMachine_HaveNoStatusProperty_KK8()
    {
        ModelFacts model = ModelFacts.Shared;
        Assert.All(Stateless, entity => Assert.Contains(entity, model.Tables));

        IReadOnlyList<PropertyFact> found = [.. model.Properties.Where(p => Stateless.Contains(p.Table)
            && (StatusEnums.Contains(Nullable.GetUnderlyingType(p.ClrType) ?? p.ClrType) || p.Name.EndsWith("Status", StringComparison.Ordinal)))];

        Assert.Empty(found);
    }

    [Fact]
    public void TheEntitiesWithAStateMachine_AreTheOnesThatCarryTheStatusProperty()
    {
        ModelFacts model = ModelFacts.Shared;

        Assert.Contains(model.Properties, p => p.Table == "Order" && p.Name == "OrderStatus" && p.ClrType == typeof(OrderStatus));
        Assert.Contains(model.Properties, p => p.Table == "Order" && p.Name == "PaymentStatus" && p.ClrType == typeof(PaymentStatus));
        Assert.Contains(model.Properties, p => p.Table == "DefectClaim" && p.Name == "DefectClaimStatus" && p.ClrType == typeof(DefectClaimStatus));
        Assert.Contains(model.Properties, p => p.Table == "ContactRequest" && p.Name == "ContactRequestStatus" && p.ClrType == typeof(ContactRequestStatus));
        Assert.Contains(model.Properties, p => p.Table == "Product" && p.Name == "PublishStatus" && p.ClrType == typeof(PublishStatus));
    }

    [Fact]
    public void EveryStatusEnumProperty_BelongsToAnEntityThatIsNotInTheStatelessList()
    {
        ModelFacts model = ModelFacts.Shared;

        IReadOnlyList<string> owners = [.. model.Properties
            .Where(p => StatusEnums.Contains(Nullable.GetUnderlyingType(p.ClrType) ?? p.ClrType))
            .Select(p => p.Table).Distinct().Order(StringComparer.Ordinal)];

        Assert.Empty(owners.Intersect(Stateless));
        Assert.Contains("Order", owners);
    }
}
