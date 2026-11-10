using System.Net;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.IntegrationTests.Payment;

/// <summary>
/// 3D Secure on the provider's notification, end to end (T406 KK1, KK2, KK5; 08 2.1.3, 2.1.10; 02 3.21.3): the notification carries no 3D mark and the product looks for
/// none - a failed 3D verification comes as <c>failed</c> with <c>failed_reason_code</c> 1, 2 or 10 and the order is cancelled, never paid; success needs <c>status</c> =
/// <c>success</c> and <c>payment_type</c> = <c>card</c>; on the live installation a test notification is marked and changes no order. The server is the live
/// installation, as in the classification tests next to this file.
/// </summary>
public sealed partial class PaymentNotificationTests
{
    // ---- KK1: a failed 3D verification is a failed payment - the order is cancelled and not paid ----

    [Theory]
    [InlineData("1")]
    [InlineData("2")]
    [InlineData("10")]
    public async Task PayTr_3DVerificationThatFailed_CancelsTheOrder_TheCardFailureReason_AndNoPayment_T406_KK1(string reasonCode)
    {
        (Order order, string reference) = await SeedPayTrOrderAsync();
        Dictionary<string, string> fields = Signed(reference, PayTrNotification.FailedStatus, "0", paymentAmount: TotalKurus);
        fields["failed_reason_code"] = reasonCode;
        fields["failed_reason_msg"] = "3d-" + reasonCode;

        using HttpResponseMessage response = await PostPayTrAsync(fields);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        InboundNotificationRecord record = await SingleRecordAsync();
        Assert.Equal(((UnprocessedNotificationReason?)null, PayTrNotification.FailedStatus), (record.UnprocessedReason, record.Outcome));
        Assert.True(await RunNextJobAsync(_payTrApp));

        await AssertOrderAsync(order.Id, OrderStatus.Cancelled, PaymentStatus.Failed, 2);
        await using AppDbContext reader = NewContext();
        Assert.False(await reader.PaymentRecords.AnyAsync(p => p.OrderId == order.Id, Ct));
        List<TransitionRecord> transitions = await reader.TransitionRecords.Where(t => t.OrderId == order.Id).ToListAsync(Ct);
        Assert.Equal(new[] { TransitionCode.O2, TransitionCode.S3 }.Order(), transitions.Select(t => t.TransitionCode).Order());
        Assert.All(transitions, t => Assert.Equal((TransitionReason?)TransitionReason.CardPaymentFailed, t.TransitionReason));
    }

    // KK1: "kodda 3D alanı aranmaz" - a field that looks like a 3D mark changes nothing; status and payment_type alone decide (with the amount, row 7).
    [Theory]
    [InlineData("non_3d", "1")]
    [InlineData("3d_secure", "0")]
    [InlineData("threeds_status", "failed")]
    public async Task PayTr_AFieldThatLooksLikeA3DMark_IsNotRead_TheSuccessIsAppliedAsRow7_T406_KK1(string field, string value)
    {
        (Order order, string reference) = await SeedPayTrOrderAsync();
        Dictionary<string, string> fields = Signed(reference, "success", TotalKurus);
        fields[field] = value;

        using HttpResponseMessage response = await PostPayTrAsync(fields);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null((await SingleRecordAsync()).UnprocessedReason);
        Assert.True(await RunNextJobAsync(_payTrApp));
        await AssertOrderAsync(order.Id, OrderStatus.Preparing, PaymentStatus.Paid, 2);
    }

    // ---- KK2: a payment type other than card is not applied - also in another case, empty or missing ----

    [Theory]
    [InlineData("eft")]
    [InlineData("CARD")]
    [InlineData("Card")]
    [InlineData("")]
    [InlineData(null)]
    public async Task PayTr_SuccessWhosePaymentTypeIsNotCard_IsMarkedUnrecognized_AndTheOrderIsNotPaid_T406_KK2(string? paymentType)
    {
        Row4Seen seen = await PostRow4Async(reference =>
        {
            Dictionary<string, string> fields = Signed(reference, "success", TotalKurus, paymentType: paymentType ?? "card");
            if (paymentType is null)
            {
                fields.Remove("payment_type");
            }

            return fields;
        });

        Assert.Equal(Row4Expected(seen.OrderId), seen);
        await AssertOrderAsync(seen.OrderId, OrderStatus.Placed, PaymentStatus.Pending, 1);
    }

    // ---- KK5: on the live installation a test notification is marked and applies nothing - a failure no more than a success ----

    [Fact]
    public async Task PayTr_TestModeFailureOnTheLiveInstallation_IsMarkedUnrecognized_AndDoesNotCancelTheOrder_T406_KK5()
    {
        Row4Seen seen = await PostRow4Async(reference =>
        {
            Dictionary<string, string> fields = Signed(reference, PayTrNotification.FailedStatus, "0", paymentAmount: TotalKurus, testMode: "1");
            fields["failed_reason_code"] = "2";
            return fields;
        });

        Assert.Equal(Row4Expected(seen.OrderId), seen);
        await AssertOrderAsync(seen.OrderId, OrderStatus.Placed, PaymentStatus.Pending, 1);
    }
}
