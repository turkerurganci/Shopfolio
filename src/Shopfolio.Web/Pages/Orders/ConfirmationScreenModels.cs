using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Orders;

/// <summary>
/// E-14 — the order confirmation and waiting screen (07 UC-28, UC-29; 04 5.14; T510). The screen hangs under the root the payment step sent the customer to (07 2.1.3): the member's
/// root by the account's ownership, the access-key root by the key (<see cref="OrderRoots"/>); a root that does not hold is the one 404. It opens only while the order is
/// Alındı + Bekliyor; otherwise - paid, cancelled, whatever the method - it is 303 to the member's order page or to the guest's order tracking (K-1209 (1)). The GET shows the
/// order number and, on the card, the button that opens the payment page; on the transfer, the IBAN frozen on the order, the total, the last payment day and the way to the order
/// (K-776). The screen never redirects by itself. <c>StartPayment</c> (UC-29) asks the provider for a new token at every sending and answers 200 with the payment page - the
/// provider's form in a frame; the address is drawn once and kept nowhere. A provider that cannot be reached cancels the order in the page's transaction and sends the customer
/// to the cart, which tells it once (K-1183, K-1200); the second token refusal reopens the screen without a cancellation (K-1255 (6)); a transfer order reopens its screen.
/// </summary>
[AllowPaymentFrame]
internal abstract class ConfirmationPageModel : PageModel
{
    /// <summary>The handler of the payment page (07 UC-29).</summary>
    public const string StartPaymentHandler = "StartPayment";

    /// <summary>04 5.14.3 (1), 5.14.4 (1): the order is received, with its number in the heading.</summary>
    public const string ReceivedText = "Siparişiniz alındı.";

    /// <summary>04 5.14.3 (2): the payment is not taken yet and is made on the provider's form of the payment page; the card is entered nowhere else (02 8.3.4).</summary>
    public const string CardWaitingText = "Ödemeniz henüz alınmadı. Ödemeyi, ödeme sayfasında ödeme kuruluşunun güvenli formunda kartınızla yapacaksınız; kart bilgileriniz bu sitede girilmez ve saklanmaz.";

    /// <summary>04 5.14.3 (3): the primary button.</summary>
    public const string OpenPaymentLabel = "Ödeme sayfasını aç";

    /// <summary>04 5.14.4: the transfer's instruction.</summary>
    public const string TransferInstructionText = "Ödemenizi aşağıdaki IBAN'a havale ya da EFT ile yapın; açıklamaya sipariş numaranızı yazın.";

    /// <summary>04 5.14.4 (5): the member's way to the order.</summary>
    public const string MemberOrderLinkText = "Siparişime git";

    /// <summary>04 5.14.4 (5): the guest's way - the link of the order e-mail and the order tracking.</summary>
    public const string GuestOrderWayText = "Siparişinize, sipariş e-postanızdaki bağlantıdan ya da sipariş numaranız ve e-posta adresinizle sipariş takibinden ulaşabilirsiniz.";

    /// <summary>04 5.14.4 (5): the guest's link.</summary>
    public const string OrderTrackingLinkText = "Sipariş takibine git";

    /// <summary>04 5.14.6: the payment page's heading line over the frame.</summary>
    public const string PaymentPageText = "Kart bilgilerinizi aşağıdaki güvenli ödeme formuna girin. Ödeme tamamlanınca siparişinizin sayfasına yönlendirilirsiniz.";

    /// <summary>The frame's accessible name.</summary>
    public const string PaymentFrameTitle = "Güvenli ödeme formu";

    private readonly IOrderingModule _ordering;
    private readonly IOrderConfirmationScreen _screen;
    private readonly IPaymentStart _start;
    private readonly ITransactionRunner _transactions;
    private readonly PaymentFrameSource? _frameSource;

    protected ConfirmationPageModel(IOrderingModule ordering, IOrderConfirmationScreen screen, IPaymentStart start, ITransactionRunner transactions, IServiceProvider services)
    {
        _ordering = ordering;
        _screen = screen;
        _start = start;
        _transactions = transactions;
        _frameSource = services.GetService<PaymentFrameSource>();
    }

    /// <summary>Whether the root is the member's (true) or the access key's.</summary>
    protected abstract bool IsMemberRoot { get; }

    public ConfirmationScreenFacts Facts { get; private set; } = null!; // non-null: every handler that returns Page() loads it first

    /// <summary>The root the screen was reached from (07 2.1.3).</summary>
    public string RootPath => OrderRoots.RootOf(Request.Path.Value ?? string.Empty, ConfirmationKeys.ConfirmationScreen);

    /// <summary>The screen's own address, the target of the payment page's form.</summary>
    public string ScreenPath => Request.Path.Value ?? string.Empty;

    /// <summary>True on the member's root: the way to the order is the order page; on the access-key root it is the order tracking (04 5.14.4 (5); K-776).</summary>
    public bool IsMember => IsMemberRoot;

    /// <summary>The member's order page (07 2.1.3).</summary>
    public string MemberOrderPath => OrderRoutes.MemberPath(Facts.OrderNumber);

    /// <summary>The guest's order tracking.</summary>
    public static string OrderTrackingPath => FramePaths.OrderTracking;

    public bool IsCard => Facts.PaymentMethod == PaymentMethod.Card;

    public string TotalText => DisplayFormat.Amount(Facts.Total);

    public string? IbanText => Facts.TransferIban is string iban ? DisplayFormat.Iban(iban) : null;

    public string LastPaymentDayText => DisplayFormat.Date(Facts.PaymentWindowEndsAt);

    /// <summary>The provider's form address of this response; null on every answer but the payment page (07 UC-29). Never stored: it lives for the one response.</summary>
    public Uri? PaymentFrameAddress { get; private set; }

    public bool ShowsPaymentPage => PaymentFrameAddress is not null;

    public async Task<IActionResult> OnGetAsync(CancellationToken ct) => await LoadAsync(ct) ?? Page();

    /// <summary>
    /// UC-29: the payment page. The order is read again (a sending after the order moved on is the screen's 303); a transfer order goes back to its screen; the start is one
    /// token request outside any transaction (05 3.6.1); the decision is the module's.
    /// </summary>
    public async Task<IActionResult> OnPostStartPaymentAsync(CancellationToken ct)
    {
        if (await LoadAsync(ct) is IActionResult refusal)
        {
            return refusal;
        }

        if (!IsCard)
        {
            return FormResults.SeeOther(ScreenPath);
        }

        // 08 2.1.4 user_ip: the request's client address, which the proxy always gives; a request without one starts nothing and reopens the screen.
        if (ClientAddress.Of(HttpContext) is not System.Net.IPAddress address || Facts.OrderAccessKey is not string accessKey)
        {
            return FormResults.SeeOther(ScreenPath);
        }

        PaymentStartAnswer answer = await _start.StartAsync(OrderOf(Facts, address.ToString(), accessKey), ct);
        switch (answer.Decision)
        {
            case PaymentStartDecision.OpenPaymentForm:
                PaymentFrameAddress = FrameAddressOf(answer.FrameAddress);
                PaymentFrameResponse.Mark(HttpContext);
                return Page();

            case PaymentStartDecision.CancelAsUnavailable:
                UnopenedPaymentCancellationResult cancelled = await _transactions.RunAsync(
                    inner => _screen.CancelUnopenedAsync(Facts.OrderId, _start.UnopenedCancellationReason, StorefrontFrameBuilder.CartOwnerOf(HttpContext), inner), ct);
                return cancelled.Outcome == UnopenedPaymentCancellation.Cancelled
                    ? FormResults.SeeOther(FramePaths.Cart)
                    : FormResults.SeeOther(NotAwaitingTarget());

            case PaymentStartDecision.StayOnConfirmation:
                return FormResults.SeeOther(ScreenPath);

            default:
                throw new System.Diagnostics.UnreachableException();
        }
    }

    /// <summary>The start's values from the frozen order (08 2.1.4): a disposed or empty customer value falls back to what the layer accepts; the layer applies its own limits.</summary>
    public static PaymentStartOrder OrderOf(ConfirmationScreenFacts facts, string clientAddress, string accessKey)
    {
        ArgumentNullException.ThrowIfNull(facts);
        return new PaymentStartOrder(
            facts.OrderNumber,
            facts.Total,
            facts.ContactEmail ?? facts.CompanyEmail,
            facts.CompanyEmail,
            facts.BillingRecipientName ?? string.Empty,
            facts.BillingAddressLine ?? string.Empty,
            facts.ShippingPhone,
            facts.CompanyPhone,
            clientAddress,
            facts.PaymentWindowEndsAt,
            accessKey);
    }

    // The frame loads only from the provider's form root (05 6.5.4: frame-src names the provider's origin): any other address is a broken layer, never a frame.
    private Uri FrameAddressOf(Uri? address)
    {
        if (address is null || _frameSource is null || !_frameSource.Holds(address))
        {
            throw new InvalidOperationException("A started payment's frame address is under the provider's form root (05 6.5.4).");
        }

        return address;
    }

    // UC-28's 303 when the order no longer waits for its payment (K-1209 (1)): the member's order page, the guest's order tracking.
    private string NotAwaitingTarget() => IsMemberRoot ? MemberOrderPath : FramePaths.OrderTracking;

    private async Task<IActionResult?> LoadAsync(CancellationToken ct)
    {
        OrderRootResult root = IsMemberRoot ? await OrderRoots.ResolveMemberAsync(this, _ordering, ct) : await OrderRoots.ResolveAccessAsync(this, _ordering, ct);
        if (root.OrderId is not Guid orderId)
        {
            return root.Denial ?? AccessDenial.NotFound();
        }

        ConfirmationScreenFacts? facts = await _screen.ReadAsync(orderId, ct);
        if (facts is null)
        {
            return AccessDenial.NotFound();
        }

        Facts = facts;
        return facts.IsAwaitingPayment ? null : FormResults.SeeOther(NotAwaitingTarget());
    }
}

/// <summary>UC-28, UC-29 under the member's root (07 2.1.3).</summary>
[NoStore]
internal sealed class MemberConfirmationModel(IOrderingModule ordering, IOrderConfirmationScreen screen, IPaymentStart start, ITransactionRunner transactions, IServiceProvider services)
    : ConfirmationPageModel(ordering, screen, start, transactions, services)
{
    protected override bool IsMemberRoot => true;
}

/// <summary>UC-28, UC-29 under the access-key root (07 2.1.3).</summary>
[KeyedLinkPage]
[NoStore]
internal sealed class AccessConfirmationModel(IOrderingModule ordering, IOrderConfirmationScreen screen, IPaymentStart start, ITransactionRunner transactions, IServiceProvider services)
    : ConfirmationPageModel(ordering, screen, start, transactions, services)
{
    protected override bool IsMemberRoot => false;
}
