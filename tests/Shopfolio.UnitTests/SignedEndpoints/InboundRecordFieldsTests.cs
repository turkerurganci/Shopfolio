using System.Reflection;

using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Modules.Payment;

namespace Shopfolio.UnitTests.SignedEndpoints;

/// <summary>
/// The inbound record carries no personal field (T203 criterion 8, KK8; 08 3.2 principle 4; 05 4.5.2): every field of
/// <see cref="InboundNotificationRecord"/> is one of 05 4.5.2's kinds - the provider's reference, the order's reference, the amount, the
/// outcome, the moments - or a link or mark of the record itself, and neither the record nor the verified notice the link layer hands the
/// endpoint (<see cref="PaymentNotice"/>) has a field named like personal data (the log formatter's list, 09 18.2 item 2a). A new field fails the
/// first test until it is given its kind here.
/// </summary>
public sealed class InboundRecordFieldsTests
{
    // 05 4.5.2: "sağlayıcının referansı, siparişin referansı, tutar, sonuç ve anlar"; the record's own id, links and processing mark.
    private static readonly Dictionary<string, string> KindOfField = new(StringComparer.Ordinal)
    {
        [nameof(InboundNotificationRecord.Id)] = "the record's own id",
        [nameof(InboundNotificationRecord.ProviderNotificationId)] = "the provider's reference",
        [nameof(InboundNotificationRecord.ProviderTransactionId)] = "the provider's reference",
        [nameof(InboundNotificationRecord.EventType)] = "the outcome",
        [nameof(InboundNotificationRecord.Outcome)] = "the outcome",
        [nameof(InboundNotificationRecord.OrderReference)] = "the order's reference",
        [nameof(InboundNotificationRecord.Amount)] = "the amount",
        [nameof(InboundNotificationRecord.OrderId)] = "a link of the record",
        [nameof(InboundNotificationRecord.RefundRecordId)] = "a link of the record",
        [nameof(InboundNotificationRecord.CreatedAt)] = "the moments",
        [nameof(InboundNotificationRecord.ProcessedAt)] = "the moments",
        [nameof(InboundNotificationRecord.UnprocessedReason)] = "the processing mark",
    };

    private static IEnumerable<string> FieldsOf(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name);

    [Fact]
    public void EveryFieldOfTheInboundRecord_IsAKindThatIsNotPersonal()
    {
        Assert.Equal(KindOfField.Keys.Order(StringComparer.Ordinal), FieldsOf(typeof(InboundNotificationRecord)).Order(StringComparer.Ordinal));
    }

    [Theory]
    [InlineData(typeof(InboundNotificationRecord))]
    [InlineData(typeof(PaymentNotice))]
    public void NoFieldOfTheRecordOrTheVerifiedNotice_IsNamedLikePersonalData(Type type)
    {
        Assert.All(FieldsOf(type), name => Assert.DoesNotContain(
            ApplicationLogFormatter.ForbiddenPropertyNames,
            forbidden => name.EndsWith(forbidden, StringComparison.OrdinalIgnoreCase)));
    }
}
