using System.Reflection;
using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Payment;

namespace Shopfolio.UnitTests.Concurrency;

/// <summary>
/// The model side of the version rules (T228 criteria 1-3; 09 10.2 items 1, 6; 06 2.6): <c>BumpVersion</c> raises a version by one, the
/// version and counter setters are private, the order's version scope is the order side less the closed list of 06 2.6.4, and every name the
/// two save guards declare is in the model. The model is read without a database.
/// </summary>
public sealed class VersionRulesTests
{
    private const string PlaceholderConnectionString = "Host=localhost;Database=shopfolio_version_rules";

    /// <summary>The counters of 09 10.2 item 4: written only by their conditional update.</summary>
    private static readonly (Type Type, string Property)[] Counters =
    [
        (typeof(Variant), nameof(Variant.ReservedQuantity)),
        (typeof(Coupon), nameof(Coupon.UsedCount)),
        (typeof(Coupon), nameof(Coupon.ReservedCount)),
        (typeof(OrderItem), nameof(OrderItem.DownloadCount)),
    ];

    /// <summary>
    /// The records 06 2.6.4 names as not raising the order's version: the internal note, the dispatch record and its outcome, the transition
    /// record, the audit trail, the job row and the inbound notification record. A copy of the document's list, not of the code's.
    /// </summary>
    private static readonly Type[] NonRaisingRecordsOf0624 =
    [
        typeof(InternalNote), typeof(DispatchRecord), typeof(TransitionRecord), typeof(AuditLog), typeof(Job), typeof(InboundNotificationRecord),
    ];

    /// <summary>
    /// The order side of 06 2.6.4 and 4.17 "Sürüm": the items, the item records (cancellation, removal, late-delivery termination, withdrawal,
    /// return receipt, return rejection, "goods not returned", defect claim, IBAN request), the payment record, the refund and its record, the
    /// refund IBAN, the address and e-mail correction histories, the reservations and the accepted checkboxes of the confirmation.
    /// </summary>
    private static readonly Type[] OrderSide =
    [
        typeof(OrderItem), typeof(Cancellation), typeof(ItemRemoval), typeof(LateDeliveryTermination), typeof(Withdrawal), typeof(Return),
        typeof(ReturnRejection), typeof(NotReturnedClosure), typeof(DefectClaim), typeof(RefundIbanRequest), typeof(PaymentRecord),
        typeof(Refund), typeof(RefundRecord), typeof(RefundIban), typeof(OrderAddressHistory), typeof(OrderEmailHistory),
        typeof(StockReservation), typeof(CheckboxAcceptance), typeof(CheckboxAcceptanceOrderItem),
    ];

    [Fact]
    public void BumpVersion_OnEveryRecordWithOneVersion_RaisesItByOne()
    {
        IEntityType[] versioned = [.. Model().GetEntityTypes().Where(t => t.FindProperty(ModelBuilderExtensions.VersionPropertyName) is not null)];

        Assert.NotEmpty(versioned);
        foreach (IEntityType type in versioned)
        {
            MethodInfo bump = type.ClrType.GetMethod("BumpVersion", BindingFlags.Public | BindingFlags.Instance, Type.EmptyTypes)
                ?? throw new Xunit.Sdk.XunitException($"{type.ClrType.Name} has a version and no BumpVersion (09 10.2 item 1).");
            PropertyInfo version = type.ClrType.GetProperty(ModelBuilderExtensions.VersionPropertyName)!;
            object record = RuntimeHelpers.GetUninitializedObject(type.ClrType);
            int before = (int)version.GetValue(record)!;

            bump.Invoke(record, null);

            Assert.Equal(before + 1, (int)version.GetValue(record)!);
        }
    }

    [Fact]
    public void VersionAndCounterSetters_AreNotPublic()
    {
        List<(Type, string)> properties = [.. Counters];
        properties.AddRange(Model().GetEntityTypes()
            .Where(t => t.FindProperty(ModelBuilderExtensions.VersionPropertyName) is not null)
            .Select(t => (t.ClrType, ModelBuilderExtensions.VersionPropertyName)));

        foreach ((Type type, string name) in properties)
        {
            MethodInfo? setter = type.GetProperty(name)!.SetMethod;
            Assert.True(setter is null || !setter.IsPublic, $"{type.Name}.{name} has a public setter (09 10.2 items 1, 4).");
        }
    }

    [Fact]
    public void OrderVersionExemptRecords_AreTheClosedListOf0624()
    {
        IEntityType[] exempt = [.. OrderVersionGuard.Scope.ExemptTypes(Model())];

        Assert.Equal(Names(NonRaisingRecordsOf0624), Names(exempt.Select(t => t.ClrType)));
        Assert.Equal(Names(NonRaisingRecordsOf0624), Names(typeof(AppDbContext).Assembly.GetTypes().Where(t => typeof(IOrderVersionExempt).IsAssignableFrom(t) && !t.IsInterface)));
    }

    [Fact]
    public void OrderVersionScope_IsTheOrderSide_AndHoldsNoExemptRecord()
    {
        IReadOnlySet<IEntityType> members = OrderVersionGuard.Scope.Members(Model());

        Assert.Equal(Names(OrderSide), Names(members.Select(m => m.ClrType)));
    }

    [Fact]
    public void FieldsTheGuardsName_AreInTheModel()
    {
        IModel model = Model();
        IEnumerable<(string Type, string Field)> named = OrderVersionGuard.NonRaisingFields
            .Concat(OrderVersionGuard.EmptiedLinks)
            .Concat(ProductVersionGuard.ConditionalFields.Select(f => (VersionScope.NameOf(f.Type), f.Field)));

        foreach ((string type, string field) in named)
        {
            IEntityType entity = model.FindEntityType(type) ?? throw new Xunit.Sdk.XunitException($"{type} is not in the model.");
            Assert.True(entity.FindProperty(field) is not null, $"{type}.{field} is not in the model.");
        }

        Assert.Equal(ProductVersionGuard.FormRecords.Order(StringComparer.Ordinal), ProductVersionGuard.Scope.Members(model).Select(t => VersionScope.NameOf(t.ClrType)).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void VersionScope_WithANameTheModelDoesNotHave_Throws()
    {
        VersionScope wrongMember = new(VersionScope.NameOf(typeof(Product)), ["Shopfolio.Missing"], null, [], []);
        VersionScope wrongRoot = new("Shopfolio.Missing", null, null, [], []);

        Assert.Throws<InvalidOperationException>(() => wrongMember.Members(Model()));
        Assert.Throws<InvalidOperationException>(() => wrongRoot.Members(Model()));
    }

    private static string[] Names(IEnumerable<Type> types)
    {
        return [.. types.Select(VersionScope.NameOf).Order(StringComparer.Ordinal)];
    }

    private static IModel Model()
    {
        DbContextOptionsBuilder<AppDbContext> options = new();
        AppDbContext.Configure(options, PlaceholderConnectionString);
        using AppDbContext context = new(options.Options);
        return context.Model;
    }
}
