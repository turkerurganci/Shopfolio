using System.Net;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Coupons;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Membership.Contracts.Results;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Storefront;

/// <summary>
/// The payment step (T509; 07 4.13 UC-22…UC-27; 04 E-13). One page, one form: the five handlers send the same form and bind the same model (07 5.1.5). The summary, the coupon's verdict, the methods' states and
/// the confirmation section are the server's; the page computes nothing of its own. The coupon and the recalculation write no order (07 2.2.2); the cookie of the step is written only when the customer
/// leaves - for the sign-in or the cart - or when a member's session closed under the form, and only with the fields of 05 6.7.9; the confirmation deletes it. The step that cannot open - sales closed,
/// no orderable line, a physical cart under the minimum amount - sends every handler but the leaving one back to the cart (07 4.13). The response is personal and not stored (07 2.5.6); the page is noindex.
/// </summary>
[NoStore]
internal sealed class CheckoutModel : PageModel, IOutcomeTarget
{
    /// <summary>The prefix of the confirmation's boxes: none, the box names are the model's own (07 5.2).</summary>
    public const string BoxPrefix = "";

    /// <summary>The sign-in with the payment step as the way back (07 UC-26; 04 3.3.16).</summary>
    public const string SignInPath = FramePaths.SignIn + "?returnUrl=" + FramePaths.Checkout;

    /// <summary>The value of <see cref="PlaceOrderInput.LeaveTarget"/> that goes to the sign-in; every other value is the cart (07 UC-26; K-1181 (2)).</summary>
    public const string LoginTarget = "login";

    /// <summary>The value of <see cref="PlaceOrderInput.LeaveTarget"/> that goes back to the cart.</summary>
    public const string CartTarget = "cart";

    private static readonly InvalidEmailAttribute EmailRule = new();

    private readonly ICheckout _checkout;
    private readonly IConfirmationStep _confirmationStep;
    private readonly IPaymentMethods _paymentMethods;
    private readonly ISettingsModule _settings;
    private readonly IMembershipModule _membership;
    private readonly ITransactionRunner _transactions;
    private readonly BrowserCookies _cookies;

    public CheckoutModel(
        ICheckout checkout,
        IConfirmationStep confirmationStep,
        IPaymentMethods paymentMethods,
        ISettingsModule settings,
        IMembershipModule membership,
        ITransactionRunner transactions,
        BrowserCookies cookies)
    {
        _checkout = checkout;
        _confirmationStep = confirmationStep;
        _paymentMethods = paymentMethods;
        _settings = settings;
        _membership = membership;
        _transactions = transactions;
        _cookies = cookies;
    }

    /// <summary>The values the form shows: the cookie's on a GET, the sent ones after a POST.</summary>
    public PlaceOrderInput Form { get; private set; } = new();

    /// <summary>The step as the server judged it in this request.</summary>
    public CheckoutStep? Step { get; private set; }

    /// <summary>The confirmation section's content (04 5.13.9).</summary>
    public ConfirmationStepView? Confirmation { get; private set; }

    /// <summary>A fresh confirmation key at every showing (07 2.5.1).</summary>
    public string ConfirmationKey { get; } = ConfirmationKeys.NewKey();

    /// <summary>The fingerprint of the summary shown (05 4.2.1).</summary>
    public string Fingerprint { get; private set; } = string.Empty;

    /// <summary>True for a signed-in member: the account's e-mail and the address book.</summary>
    public bool IsMember { get; private set; }

    /// <summary>The member's verified e-mail (04 5.13.5); null for a guest.</summary>
    public string? MemberEmail { get; private set; }

    /// <summary>The member's book entries as the step offers them (04 5.13.6).</summary>
    public IReadOnlyList<AddressBookChoice> AddressBook { get; private set; } = [];

    /// <summary>The 81 provinces with their delivery mark (04 2.12.2.3).</summary>
    public IReadOnlyList<ProvinceChoice> Provinces { get; private set; } = [];

    /// <summary>The methods that can be chosen now (07 4.13.1); the card whose provider was unreachable in the last two minutes is not among them (08 2.1.8).</summary>
    public IReadOnlyList<PaymentMethod> OpenMethods { get; private set; } = [];

    /// <summary>The card is open but cannot be chosen now (08 2.1.8; 04 5.13.8).</summary>
    public bool IsCardUnavailableNow { get; private set; }

    /// <summary>
    /// The methods the list draws in the fixed order card, bank transfer (04 5.13.8): the open ones and the card that cannot be chosen now, which stays in the list
    /// unselectable with its reason beside it (07 4.13.1; VAL-E061).
    /// </summary>
    public IReadOnlyList<PaymentMethod> ShownMethods => IsCardUnavailableNow && !OpenMethods.Contains(PaymentMethod.Card) ? [PaymentMethod.Card, .. OpenMethods] : OpenMethods;

    /// <summary>The method the form shows chosen.</summary>
    public PaymentMethod? ChosenMethod { get; private set; }

    /// <summary>The coupon field (04 OB-17).</summary>
    public CouponFieldView Coupon { get; private set; } = CouponFields.Closed(nameof(PlaceOrderInput.CouponCode));

    /// <summary>The guest's e-mail is a verified account's: the reminder under the field (04 5.13.5; 02 3.13.3).</summary>
    public bool ShowsRegisteredEmailReminder { get; private set; }

    /// <summary>The change lines at the head of the confirmation section (02 3.17.2; 04 5.13.20).</summary>
    public IReadOnlyList<string> ChangeLines { get; private set; } = [];

    /// <summary>The message beside the confirm button: the closed sale, the unusable method (04 5.13.21, 5.13.27).</summary>
    public MessageView? ActionMessage { get; private set; }

    /// <summary>What the sent form's boxes keep (04 2.12.7.3).</summary>
    public ConsentReset BoxReset { get; private set; }

    /// <summary>The boxes the customer sent; null on a GET (no box comes ticked).</summary>
    public ConsentSent? SentBoxes { get; private set; }

    /// <summary>The basket the boxes are about.</summary>
    public ConsentBasket Basket { get; private set; } = ConsentBasket.Physical;

    /// <summary>No method can be used: the cart's way back stands in the confirm button's place (04 5.13.21).</summary>
    public bool HasNoUsableMethod => OpenMethods.Count == 0;

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        if (StorefrontFrameBuilder.CartOwnerOf(HttpContext) is not CartOwner owner)
        {
            return FormResults.SeeOther(FramePaths.Cart);
        }

        CheckoutCookieValues? kept = ReadCookie();
        PlaceOrderInput form = kept is null ? new PlaceOrderInput() : PlaceOrderInputs.From(kept);
        return await ShowAsync(owner, form, appliesCoupon: false, ct) ?? Page();
    }

    public async Task<IActionResult> OnPostApplyCouponAsync(PlaceOrderInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (SessionClosedUnder(input) is IActionResult toSignIn)
        {
            return toSignIn;
        }

        CartOwner owner = StorefrontFrameBuilder.CartOwnerOf(HttpContext)!; // non-null: SessionClosedUnder answers a request without a cart owner
        if (await ShowAsync(owner, input, appliesCoupon: !string.IsNullOrWhiteSpace(input.CouponCode), ct) is IActionResult away)
        {
            return away;
        }

        switch (Step!.Coupon?.Outcome) // non-null: ShowAsync sets the step when it shows the page
        {
            case CouponOutcome.AttemptLimitExceeded:
                return FormResults.AttemptLimitExceeded(this, ErrorCode.CouponAttemptLimitExceeded);
            case CouponOutcome.Rejected:
            case CouponOutcome.BelowMinimumBasket:
                ErrorCode code = Step.Coupon.Outcome == CouponOutcome.Rejected ? ErrorCode.CouponRejected : ErrorCode.CouponBelowMinimumBasket;
                HttpContext.RequestServices.GetRequiredService<RejectionLog>().Rejected(code);
                ModelState.AddModelError(nameof(PlaceOrderInput.CouponCode), CouponFields.RefusalText(Step.Coupon.Outcome, Step.Coupon.MinimumBasketAmount));
                return FormResults.Redisplay(this);
            default:
                return Page();
        }
    }

    public async Task<IActionResult> OnPostRemoveCouponAsync(PlaceOrderInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (SessionClosedUnder(input) is IActionResult toSignIn)
        {
            return toSignIn;
        }

        input.CouponCode = null;
        ModelState.Remove(nameof(PlaceOrderInput.CouponCode));
        CartOwner owner = StorefrontFrameBuilder.CartOwnerOf(HttpContext)!; // non-null: SessionClosedUnder answers a request without a cart owner
        return await ShowAsync(owner, input, appliesCoupon: false, ct) ?? Page();
    }

    public async Task<IActionResult> OnPostRecalculateAsync(PlaceOrderInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (SessionClosedUnder(input) is IActionResult toSignIn)
        {
            return toSignIn;
        }

        CartOwner owner = StorefrontFrameBuilder.CartOwnerOf(HttpContext)!; // non-null: SessionClosedUnder answers a request without a cart owner
        if (await ShowAsync(owner, input, appliesCoupon: false, ct) is IActionResult away)
        {
            return away;
        }

        // 04 5.13.23: an undelivered shipping province is told at the field; the recalculation checks nothing else.
        if (input.AddressBookId is null && Step!.HasPhysicalItem && AddressForms.CheckProvince(AddressVariant.Delivery, input.ShippingProvinceId, Provinces) is { IsAccepted: false, Reason: { } message }) // non-null: ShowAsync sets the step when it does not send the request away
        {
            ModelState.AddModelError(nameof(PlaceOrderInput.ShippingProvinceId), message);
        }

        return Page();
    }

    public async Task<IActionResult> OnPostLeaveAsync(PlaceOrderInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        WriteCookie(input);
        if (string.Equals(input.LeaveTarget, LoginTarget, StringComparison.Ordinal))
        {
            return FormResults.SeeOther(SignInPath);
        }

        // 07 UC-26: the return to the cart while no method can be used marks the cart; the cart tells it once (K-1183).
        if (StorefrontFrameBuilder.CartOwnerOf(HttpContext) is CartOwner owner)
        {
            IReadOnlyList<PaymentMethod> open = await _paymentMethods.ListOpenAsync(ct);
            bool cardNow = open.Contains(PaymentMethod.Card) && !await _paymentMethods.IsCardUnavailableNowAsync(ct);
            if (!cardNow && !open.Contains(PaymentMethod.BankTransfer))
            {
                await _transactions.RunAsync(async inner => await MarkAsync(owner, inner), ct);
            }
        }

        return FormResults.SeeOther(FramePaths.Cart);
    }

    [OutcomeUncertain("UC-27")]
    public async Task<IActionResult> OnPostPlaceOrderAsync(PlaceOrderInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (SessionClosedUnder(input) is IActionResult toSignIn)
        {
            return toSignIn;
        }

        CartOwner owner = StorefrontFrameBuilder.CartOwnerOf(HttpContext)!; // non-null: SessionClosedUnder answers a request without a cart owner
        bool isMember = owner.CustomerId is not null;

        // The fields and the boxes are checked before the order's transaction, all together (07 4.13.6; 04 5.13.18).
        if (!isMember)
        {
            if (string.IsNullOrWhiteSpace(input.ContactEmail))
            {
                ModelState.AddFieldError(nameof(PlaceOrderInput.ContactEmail), FieldCodes.FieldRequired);
            }
            else if (!EmailRule.Allows(input.ContactEmail.Trim()))
            {
                ModelState.AddFieldError(nameof(PlaceOrderInput.ContactEmail), FieldCodes.InvalidEmail);
            }
        }

        if (input.PaymentMethod is null)
        {
            ModelState.AddFieldError(nameof(PlaceOrderInput.PaymentMethod), FieldCodes.FieldRequired);
        }

        ConfirmationSummary? confirmed = CheckoutForms.ReadFingerprint(input.SummaryFingerprint);
        ConsentSent sent = PlaceOrderInputs.Boxes(input);
        if (confirmed is not null)
        {
            ConsentAddRefusals(confirmed, sent);
        }

        if (!ModelState.IsValid || confirmed is null)
        {
            await AddAddressRefusalsAsync(owner, input, ct);
            return await RefuseAsync(owner, input, null, ct);
        }

        // D-805: an order needs the client address (L-7 and L-8 count it, 02 8.2.2). A request without one is not counted, so it is refused like a full limit -
        // the neutral 429 that tells no count, time or axis - and never becomes a 500 or an order.
        if (ClientAddress.Of(HttpContext) is not IPAddress address)
        {
            await ShowAsync(owner, input, appliesCoupon: false, ct);
            return FormResults.AttemptLimitExceeded(this, ErrorCode.UnpaidOrderLimitExceeded);
        }

        PlaceOrderRequest request = new(
            owner,
            input.ConfirmationKey ?? string.Empty,
            confirmed with { PaymentMethod = input.PaymentMethod!.Value }, // non-null: a missing method is refused above
            input.PreInformationFormVersionId ?? Guid.Empty,
            input.DistanceSalesContractVersionId ?? Guid.Empty,
            new TickedBoxes(sent.PreInformation, sent.DistanceSales, sent.Digital, sent.Service),
            input.CouponCode,
            isMember ? null : input.ContactEmail?.Trim(),
            PlaceOrderInputs.AddressRequest(input, owner.CustomerId, confirmed.Lines.Select(l => l.ProductType).ToArray()),
            address);

        // 04 5.13.31; 07 6.4.2: an error the step did not expect is the error layer's - the handler is marked and the layer sends the customer to OutcomeScreen.
        PlaceOrderResult result = await _transactions.RunAsync(inner => _checkout.PlaceOrderAsync(request, inner), PlaceOrderRequest.RetryOnUniqueIndexes, ct);
        return await AnswerAsync(owner, input, result, ct);
    }

    /// <summary>The one method that chooses the screen of an uncertain confirmation (09 8.1 item 4; 07 6.4.2): the member's orders, the guest's order tracking.</summary>
    public string OutcomeScreen() =>
        StorefrontFrameBuilder.CartOwnerOf(HttpContext)?.CustomerId is null ? FramePaths.OrderTracking : FramePaths.AccountOrders;

    private async Task<IActionResult> AnswerAsync(CartOwner owner, PlaceOrderInput input, PlaceOrderResult result, CancellationToken ct)
    {
        switch (result.Outcome)
        {
            case PlaceOrderOutcome.Placed:
            case PlaceOrderOutcome.AlreadyPlaced:
                BrowserCookies.DeleteCheckout(HttpContext);
                return FormResults.SeeOther(result.IsGuestOrder
                    ? ConfirmationKeys.AccessConfirmationPath(result.OrderAccessKey!) // non-null: a born order has its access key
                    : ConfirmationKeys.MemberConfirmationPath(result.OrderNumber!)); // non-null: a born order has its number
            case PlaceOrderOutcome.NoOrderableItem:
            case PlaceOrderOutcome.MinimumOrderAmountNotMet:
                return FormResults.SeeOther(FramePaths.Cart);
            case PlaceOrderOutcome.AddressBookEntryNotFound:
                return AccessDenial.NotFound();
            case PlaceOrderOutcome.AttemptLimitExceeded:
                await ShowAsync(owner, input, appliesCoupon: false, ct);
                return FormResults.AttemptLimitExceeded(this, ErrorCode.UnpaidOrderLimitExceeded);
            case PlaceOrderOutcome.AddressRefused:
                AddAddressRefusals(result.AddressFields ?? []);
                return await RefuseAsync(owner, input, ErrorCode.DeliveryProvinceNotServed, ct, logs: result.AddressFields?.Any(f => f.Code == ErrorCode.DeliveryProvinceNotServed) == true);
            case PlaceOrderOutcome.AccountNotAccepted:
                ModelState.AddFieldError(nameof(PlaceOrderInput.ContactEmail), FieldCodes.InvalidEmail);
                return await RefuseAsync(owner, input, null, ct);
            case PlaceOrderOutcome.SalesClosed:
                ActionMessage = Messages.InPlace(MessageMeaning.Warning, SalesClosedMessages.StorefrontText);
                return await RefuseAsync(owner, input, ErrorCode.SalesClosed, ct, checksGate: false);
            case PlaceOrderOutcome.PaymentMethodUnavailable:
                ActionMessage = Messages.InPlace(MessageMeaning.Warning, CheckoutTexts.MethodUnavailable);
                return await RefuseAsync(owner, input, ErrorCode.PaymentMethodUnavailable, ct);
            case PlaceOrderOutcome.ConfirmationStepNotReady:
                if (result.StepCheck is { LegalTextVersionRaised: false } check)
                {
                    ConsentBoxes.AddRefusals(ModelState, BoxPrefix, [.. check.MissingBoxes.Select(KindOf)]);
                    return await RefuseAsync(owner, input, null, ct);
                }

                ChangeLines = [CheckoutTexts.LegalTextChanged];
                return await RefuseAsync(owner, input, ErrorCode.OrderSummaryChanged, ct);
            default:
                ChangeLines = CheckoutTexts.ChangeLinesOf(result.Changes ?? []);
                return await RefuseAsync(owner, input, ErrorCode.OrderSummaryChanged, ct);
        }
    }

    // The page again with the sent values, 422 (07 4.13.6): the named refusal writes its one rejection line (09 8.2 item 3).
    private async Task<IActionResult> RefuseAsync(CartOwner owner, PlaceOrderInput input, ErrorCode? code, CancellationToken ct, bool logs = true, bool checksGate = true)
    {
        if (code is ErrorCode named && logs)
        {
            HttpContext.RequestServices.GetRequiredService<RejectionLog>().Rejected(named);
        }

        if (await ShowAsync(owner, input, appliesCoupon: false, ct, checksGate) is IActionResult away)
        {
            return away;
        }

        if (ChangeLines.Count == 0 && ModelState.IsValid && code is null)
        {
            ChangeLines = [CheckoutTexts.SummaryChangedGeneral];
        }

        return FormResults.Redisplay(this);
    }

    // 07 4.13: a member whose session closed under the form - the form had no e-mail field, the request has no member - has the form kept and goes to the sign-in; a request without a cart goes to the cart.
    private IActionResult? SessionClosedUnder(PlaceOrderInput input)
    {
        CartOwner? owner = StorefrontFrameBuilder.CartOwnerOf(HttpContext);
        bool memberForm = Request.HasFormContentType && !Request.Form.ContainsKey(nameof(PlaceOrderInput.ContactEmail));
        if (memberForm && owner?.CustomerId is null)
        {
            WriteCookie(input);
            return FormResults.SeeOther(SignInPath);
        }

        return owner is null ? FormResults.SeeOther(FramePaths.Cart) : null;
    }

    // The step's read and the view; a step that cannot open answers 303 to the cart (07 4.13).
    private async Task<IActionResult?> ShowAsync(CartOwner owner, PlaceOrderInput form, bool appliesCoupon, CancellationToken ct, bool checksGate = true)
    {
        Form = form;
        IsMember = owner.CustomerId is not null;
        SalesGate gate = await _settings.GetSalesGateAsync(ct);
        if (checksGate && !gate.Permits(SalesGateOperation.ProceedToCheckout))
        {
            return FormResults.SeeOther(FramePaths.Cart);
        }

        OpenMethods = await _paymentMethods.ListOpenAsync(ct);
        IsCardUnavailableNow = await _paymentMethods.IsCardUnavailableNowAsync(ct);
        ChosenMethod = form.PaymentMethod is PaymentMethod chosen && OpenMethods.Contains(chosen) && !(chosen == PaymentMethod.Card && IsCardUnavailableNow) ? chosen : DefaultMethod();

        DeliveryArea area = await _settings.GetDeliveryAreaAsync(ct);
        Provinces = [.. (await _settings.GetProvincesAsync(ct)).Select(p => new ProvinceChoice(p.Id, p.Name, area.IsServed(p.Id)))];
        if (owner.CustomerId is Guid customerId)
        {
            MemberEmail = await _membership.GetVerifiedEmailAsync(customerId, ct);
            AddressBook = [.. (await _membership.GetAddressBookAsync(customerId, ct)).Select(AddressBookChoice.From)];
        }

        PartyAddress? shipping = await ShippingOfAsync(owner, form, ct);
        PartyAddress? billing = form.IsBillingSameAsShipping && shipping is not null
            ? shipping with { Phone = null }
            : form.BillingProvinceId is Guid billingProvince
                ? new PartyAddress(form.BillingRecipientName ?? string.Empty, null, billingProvince, form.BillingDistrict ?? string.Empty, form.BillingAddressLine ?? string.Empty)
                : null;
        CheckoutInput read = new(owner, ChosenMethod ?? PaymentMethod.Card, form.CouponCode, form.ContactEmail, shipping, billing);
        CheckoutStep step = await _transactions.RunAsync(inner => _checkout.ReadAsync(read, appliesCoupon, inner), ct);
        if (step.Block != CartPaymentBlock.None || step.Summary is null)
        {
            return FormResults.SeeOther(FramePaths.Cart);
        }

        Step = step;
        Fingerprint = CheckoutForms.Fingerprint(step.Summary);
        Confirmation = await _confirmationStep.GetViewAsync(step.Summary, ct);
        Basket = BasketOf(step.Summary);
        Coupon = CouponOf(step, form.CouponCode);

        // 04 2.12.7.3: a sent form keeps its ticks unless the texts or the summary changed under it.
        if (Request.HasFormContentType)
        {
            SentBoxes = PlaceOrderInputs.Boxes(form);
            BoxReset = ConsentBoxes.ResetOf(
                new ConsentState(VersionsOf(form.PreInformationFormVersionId, form.DistanceSalesContractVersionId), form.SummaryFingerprint ?? string.Empty),
                new ConsentState(VersionsOf(Confirmation.ShownPreInformationFormId, Confirmation.ShownDistanceSalesContractId), Fingerprint));
        }

        // 02 3.13.3; 07 UC-25: the reminder of a guest e-mail that is a verified account's. It is no attempt of L-3.
        if (!IsMember && !string.IsNullOrWhiteSpace(form.ContactEmail) && EmailRule.Allows(form.ContactEmail.Trim()))
        {
            OrderAccountResolution account = await _membership.ResolveOrderAccountAsync(null, form.ContactEmail.Trim(), ct);
            ShowsRegisteredEmailReminder = account.Kind == OrderAccountKind.GuestBoundToAccount;
        }

        if (HasNoUsableMethod)
        {
            ActionMessage ??= Messages.InPlace(MessageMeaning.Warning, CheckoutTexts.NoMethod);
        }

        return null;
    }

    private PaymentMethod? DefaultMethod()
    {
        if (OpenMethods.Contains(PaymentMethod.Card) && !IsCardUnavailableNow)
        {
            return PaymentMethod.Card;
        }

        return OpenMethods.Contains(PaymentMethod.BankTransfer) ? PaymentMethod.BankTransfer : null;
    }

    private async Task<PartyAddress?> ShippingOfAsync(CartOwner owner, PlaceOrderInput form, CancellationToken ct)
    {
        if (form.AddressBookId is Guid entryId && owner.CustomerId is Guid customerId)
        {
            AddressBookEntry? entry = await _membership.GetAddressBookEntryAsync(customerId, entryId, ct);
            return entry is null ? null : new PartyAddress(entry.RecipientName, entry.Phone ?? form.ShippingPhone, entry.ProvinceId, entry.District, entry.AddressLine);
        }

        return form.ShippingProvinceId is Guid province
            ? new PartyAddress(form.ShippingRecipientName ?? string.Empty, form.ShippingPhone, province, form.ShippingDistrict ?? string.Empty, form.ShippingAddressLine ?? string.Empty)
            : null;
    }

    private static CouponFieldView CouponOf(CheckoutStep step, string? typed)
    {
        const string name = nameof(PlaceOrderInput.CouponCode);
        if (step.Coupon is not CouponApplication application)
        {
            return CouponFields.Closed(name);
        }

        return application.Outcome == CouponOutcome.Accepted && application.Discount <= 0 ? CouponFields.Closed(name) : CouponFields.From(application, name, typed);
    }

    private static ConsentBasket BasketOf(ConfirmationSummary summary) => new(
        [.. summary.Lines.Where(l => l.ProductType == ProductType.Digital).Select(l => l.ProductName)],
        [.. summary.Lines.Where(l => l.ProductType == ProductType.Service).Select(l => l.ProductName)],
        summary.PaymentMethod == PaymentMethod.BankTransfer);

    private static string VersionsOf(Guid? form, Guid? contract) => $"{form:D}|{contract:D}";

    private void ConsentAddRefusals(ConfirmationSummary confirmed, ConsentSent sent) =>
        ConsentBoxes.AddRefusals(ModelState, BoxPrefix, ConsentBoxes.Missing(BasketOf(confirmed), sent));

    private static ConsentKind KindOf(ConfirmationBox box) =>
        box == ConfirmationBox.PreInformationFormRead ? ConsentKind.PreInformation
        : box == ConfirmationBox.DistanceSalesContractAccepted ? ConsentKind.DistanceSales
        : box == ConfirmationBox.DigitalWithdrawalWaiver ? ConsentKind.Digital
        : ConsentKind.Service;

    // The address part's field refusals beside the page's own (04 5.13.18: the errors are marked together); the resolution is rolled back - nothing is written before the order.
    private async Task AddAddressRefusalsAsync(CartOwner owner, PlaceOrderInput input, CancellationToken ct)
    {
        CheckoutStep step = await _transactions.RunAsync(
            inner => _checkout.ReadAsync(new CheckoutInput(owner, input.PaymentMethod ?? PaymentMethod.Card, null, input.ContactEmail, null, null), false, inner), ct);
        ProductType[] types = step.Summary is null ? [] : [.. step.Summary.Lines.Select(l => l.ProductType)];
        if (types.Length == 0)
        {
            return;
        }

        AddressCheck check = await _transactions.RunAsync(
            async inner => new AddressCheck(await _membership.ResolveOrderAddressesAsync(PlaceOrderInputs.AddressRequest(input, owner.CustomerId, types) with { SaveToAddressBook = false }, inner)), ct);
        if (check.Resolution.IsBookEntryNotFound)
        {
            return;
        }

        AddAddressRefusals(check.Resolution.Fields);
    }

    private void AddAddressRefusals(IReadOnlyList<MembershipFieldError> fields)
    {
        AddressForms.AddRefusals(ModelState, "Billing", fields.Where(f => f.Field.StartsWith("Billing", StringComparison.Ordinal)));
        AddressForms.AddRefusals(ModelState, "Shipping", fields.Where(f => !f.Field.StartsWith("Billing", StringComparison.Ordinal)));
    }

    private async Task<MarkDone> MarkAsync(CartOwner owner, CancellationToken ct)
    {
        await _checkout.MarkPaymentUnavailableAsync(owner, ct);
        return new MarkDone();
    }

    private CheckoutCookieValues? ReadCookie()
    {
        ProtectedValueResult kept = _cookies.ReadCheckout(HttpContext);
        return kept.IsValid ? CheckoutForms.ReadCookiePayload(kept.Payload) : null;
    }

    private void WriteCookie(PlaceOrderInput input) => _cookies.WriteCheckout(HttpContext, CheckoutForms.CookiePayload(PlaceOrderInputs.CookieValues(input)));

    // The check is never kept: the address part's write (a book entry) is rolled back.
    private sealed record AddressCheck(OrderAddressResolution Resolution) : IOperationResult
    {
        public bool IsRejected => true;
    }

    private sealed record MarkDone : IOperationResult
    {
        public bool IsRejected => false;
    }
}
