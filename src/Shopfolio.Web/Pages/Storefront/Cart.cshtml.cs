using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Storefront;

/// <summary>
/// The cart (T508; 07 4.12 UC-19, UC-20, UC-21; 04 E-12). The lines, their prices and availability, the amount's breakdown, the threshold messages and the state of the way to the payment step are the
/// server's: the page reads the Cart and order module's view and the sales gate, and computes nothing of its own. The once-told messages - the merge, the line taken off the showcase, the return for
/// an unusable payment - are taken by the read that tells them, so a reload does not tell them again (K-1179 (3), K-1183). A guest without the cart cookie has an empty cart and the GET writes no
/// cookie; a guest with it has the cart's last access written and the cookie renewed (05 6.7.8). An empty cart deletes the checkout step's cookie (05 6.7.7). The response is personal and not
/// stored (07 2.5.6).
/// </summary>
[NoStore]
internal sealed class CartModel : PageModel
{
    private readonly IOrderingModule _ordering;
    private readonly ISettingsModule _settings;
    private readonly ITransactionRunner _transactions;

    private bool _loaded;

    public CartModel(IOrderingModule ordering, ISettingsModule settings, ITransactionRunner transactions)
    {
        ArgumentNullException.ThrowIfNull(ordering);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(transactions);
        _ordering = ordering;
        _settings = settings;
        _transactions = transactions;
    }

    /// <summary>The cart as the module read it in this request; null for a guest without the cart cookie, whose cart is empty.</summary>
    public CartView? View { get; private set; }

    /// <summary>True when the cart holds no line: the empty state, no breakdown and no button (04 5.12.16).</summary>
    public bool IsEmpty => View is null || View.IsEmpty;

    /// <summary>Why the payment step does not open, or <see cref="CartPaymentBlock.None"/> (04 5.12.10, 5.12.14).</summary>
    public CartPaymentBlock PaymentStepBlock { get; private set; }

    /// <summary>The once-told messages at the head of the content (04 2.3.1.3, 5.12.3, 5.12.15).</summary>
    public IReadOnlyList<MessageView> OnceTold { get; private set; } = [];

    /// <summary>The line a refused quantity change was about (07 UC-20 422); its row carries the refusal.</summary>
    public Guid? RefusedItemId { get; private set; }

    /// <summary>The refusal of the raise on <see cref="RefusedItemId"/>: "Bu adette stok yok" or the limit (03 2.3.3); null for a field error.</summary>
    public string? RefusalText { get; private set; }

    /// <summary>The quantity the customer sent on the refused line: the field shows it again (07 2.2.2).</summary>
    public string? SentQuantity { get; private set; }

    /// <summary>
    /// The validation filter answers a form whose fields break their rules itself, 422 without the handler (09 3.4 item 3; 07 UC-20): the page it shows again still needs the cart as it stands
    /// and the refused line. The cart is read after the filters, so a request the forgery check refused reads and writes nothing.
    /// </summary>
    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        PageHandlerExecutedContext executed = await next();
        if (!_loaded && executed.Exception is null && executed.Result is PageResult { StatusCode: StatusCodes.Status422UnprocessableEntity })
        {
            RefusedItemId = context.HandlerArguments.Values.OfType<UpdateCartItemInput>().FirstOrDefault()?.CartItemId;
            SentQuantity = Request.HasFormContentType ? Request.Form[nameof(UpdateCartItemInput.Quantity)].ToString() : null;
            await LoadAsync(HttpContext.RequestAborted);
        }
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        await LoadAsync(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateCartItemAsync(UpdateCartItemInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (StorefrontFrameBuilder.CartOwnerOf(HttpContext) is not CartOwner owner)
        {
            return AccessDenial.NotFound();
        }

        if (!ModelState.IsValid || input.Quantity is not int quantity)
        {
            return await RefuseAsync(input.CartItemId, null, ct);
        }

        UpdateOutcome outcome = await _transactions.RunAsync(async work => new UpdateOutcome(await _ordering.UpdateCartItemAsync(owner, input.CartItemId, quantity, work)), ct);
        CartItemUpdateResult result = outcome.Result;
        switch (result.Outcome)
        {
            case CartItemUpdateOutcome.Updated:
            case CartItemUpdateOutcome.TakenOffShowcase:
                RenewGuestCookie(owner);
                return FormResults.SeeOther(FramePaths.Cart);
            case CartItemUpdateOutcome.NotInCart:
                return AccessDenial.NotFound();
            case CartItemUpdateOutcome.InvalidQuantity:
                // The module's refusal is a digital line's quantity other than one (02 3.12.4): the message names one as the limit.
                ModelState.AddFieldError(nameof(UpdateCartItemInput.Quantity), FieldCodes.InvalidQuantity, DisplayFormat.Number(1));
                return await RefuseAsync(input.CartItemId, null, ct);
            default:
                CartQuantityProblem problem = result.Outcome == CartItemUpdateOutcome.OverOrderLimit ? CartQuantityProblem.OverOrderLimit : CartQuantityProblem.NoStock;

                // The one rejection line of the named refusal, with 07's code (07 6.1.3, 6.2.3; D-638): the row tells the rule's text, the log keeps the code.
                HttpContext.RequestServices.GetRequiredService<RejectionLog>().Rejected(CartRefusalCodes.CodeOf(result.Outcome));
                return await RefuseAsync(input.CartItemId, CartTexts.QuantityProblem(problem, result.OrderLimit), ct);
        }
    }

    public async Task<IActionResult> OnPostRemoveCartItemAsync(RemoveCartItemInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (StorefrontFrameBuilder.CartOwnerOf(HttpContext) is not CartOwner owner)
        {
            return AccessDenial.NotFound();
        }

        RemovalOutcome outcome = await _transactions.RunAsync(async token => new RemovalOutcome(await _ordering.RemoveCartItemAsync(owner, input.CartItemId, token)), ct);
        if (!outcome.Result.Removed)
        {
            return AccessDenial.NotFound();
        }

        if (outcome.Result.CartIsEmpty)
        {
            BrowserCookies.DeleteCheckout(HttpContext);
        }

        RenewGuestCookie(owner);
        return FormResults.SeeOther(FramePaths.Cart);
    }

    // The page again with the cart as it stands, status 422: the refused line keeps its quantity and carries the refusal in its row (07 UC-20; 03 2.3.3).
    private async Task<IActionResult> RefuseAsync(Guid cartItemId, string? refusal, CancellationToken ct)
    {
        RefusedItemId = cartItemId;
        RefusalText = refusal;
        SentQuantity = Request.HasFormContentType ? Request.Form[nameof(UpdateCartItemInput.Quantity)].ToString() : null;
        await LoadAsync(ct);
        return FormResults.Redisplay(this);
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        _loaded = true;
        ShippingFeeSection shippingTerms = await _settings.GetShippingFeeAsync(ct);
        SalesGate gate = await _settings.GetSalesGateAsync(ct);
        CartOwner? owner = StorefrontFrameBuilder.CartOwnerOf(HttpContext);

        // A guest without the cart cookie has no cart: nothing is read, nothing is written and no cookie is opened (07 UC-19).
        CartView? view = owner is null ? null : (await _transactions.RunAsync(async token => new CartRead(await _ordering.OpenCartPageAsync(owner, shippingTerms, token)), ct)).View;
        View = view;
        if (view is not null)
        {
            PaymentStepBlock = view.PaymentStepBlock(gate.Permits(SalesGateOperation.ProceedToCheckout));
            OnceTold = OnceToldOf(view);
        }

        if (owner is not null && view?.CartId is not null)
        {
            RenewGuestCookie(owner);
        }

        // An empty cart ends what the payment step held (05 6.7.7); a cookie the request did not bring is not touched, so a guest without cookies gets none.
        if (IsEmpty && BrowserCookies.Read(HttpContext, CookieNames.Checkout) is not null)
        {
            BrowserCookies.DeleteCheckout(HttpContext);
        }
    }

    private static List<MessageView> OnceToldOf(CartView view)
    {
        List<MessageView> messages = [];
        if (view.MergeAddedProductNames.Count > 0)
        {
            messages.Add(Messages.OnceTold(MessageMeaning.Info, CartTexts.MergeAdded(view.MergeAddedProductNames)));
        }

        messages.AddRange(view.RemovedProductNames.Select(name => Messages.OnceTold(MessageMeaning.Warning, CartTexts.RemovedFromShowcase(name))));
        if (view.ShowsPaymentUnavailableNotice)
        {
            messages.Add(Messages.OnceTold(MessageMeaning.Warning, CartTexts.PaymentUnavailable));
        }

        return messages;
    }

    // A guest's cart cookie lives again for its full life at every visit that brings it (05 6.7.5, 6.7.8); a member has no cart cookie.
    private void RenewGuestCookie(CartOwner owner)
    {
        if (owner.GuestCartKey is string key)
        {
            BrowserCookies.WriteGuestCart(HttpContext, key);
        }
    }

    private sealed record CartRead(CartView View) : IOperationResult
    {
        public bool IsRejected => false;
    }

    // A refusal writes nothing: the transaction is rolled back (09 9.1 item 3).
    private sealed record UpdateOutcome(CartItemUpdateResult Result) : IOperationResult
    {
        public bool IsRejected => Result.WritesNothing;
    }

    private sealed record RemovalOutcome(CartItemRemovalResult Result) : IOperationResult
    {
        public bool IsRejected => !Result.Removed;
    }
}

/// <summary>The form of UC-20 (07 5.2.1): the line's internal key and its absolute quantity; a digital line has no quantity field.</summary>
public sealed class UpdateCartItemInput : IFormInput
{
    public Guid CartItemId { get; set; }

    /// <summary>
    /// A whole number of one or more (04 7.1.4.1, 7.3.2.10). The field rule's ceiling is the type's: the upper bound of a cart line is the stock and the product's limit, told in the line's row
    /// by the add's check (03 2.3.3), not by the field.
    /// </summary>
    [Display(Name = "Adet")]
    [FieldRequired]
    [InvalidQuantity(int.MaxValue)]
    public int? Quantity { get; set; }
}

/// <summary>The form of UC-21 (07 5.2.1): the line's internal key.</summary>
public sealed class RemoveCartItemInput : IFormInput
{
    public Guid CartItemId { get; set; }
}
