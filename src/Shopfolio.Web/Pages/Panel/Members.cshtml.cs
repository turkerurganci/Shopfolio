using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Lists;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Lists;

namespace Shopfolio.Pages.Panel;

/// <summary>
/// The query model of the member record screen (E-40; 07 UC-95; 04 9.12.3): the e-mail searched, and <c>page</c> of the bound orders. A GET: a wrong e-mail is a field message with a 200 and no search
/// (04 9.12.12); an empty field is no search (04 9.12.14).
/// </summary>
public sealed class CustomerQuery : PagedQuery
{
    [SpecAssignedName]
    [Display(Name = "E-posta")]
    [InvalidEmail]
    public string? Search { get; init; }
}

/// <summary>The form of UC-157 (07 9.12; 04 9.12.10): the account to delete, the e-mail the screen was searched with (the way back) and the confirmation mark.</summary>
public sealed class DeleteCustomerInput : IFormInput
{
    /// <summary>The account to delete; the id the record view carried.</summary>
    public Guid CustomerId { get; set; }

    /// <summary>The e-mail the screen was searched with; the screen returns to the search of it.</summary>
    [SpecAssignedName]
    public string? SearchEmail { get; set; }

    /// <summary>False asks for the confirmation state; true is the confirmed operation (04 2.4.4).</summary>
    [SpecAssignedName]
    public bool Confirmed { get; set; }
}

/// <summary>What the search found (04 9.12.13): nothing searched, no account, a registration whose e-mail was never verified, a verified account.</summary>
public enum MemberSearchState
{
    NotSearched,
    NotFound,
    Unverified,
    Account,
}

/// <summary>One address of the address book as the record shows it (04 9.12.5): the name of the address, the recipient, the province, the district, the street address and - when entered - the phone.</summary>
public sealed record MemberAddressView(string Name, string Recipient, string Province, string District, string AddressLine, string? Phone);

/// <summary>One bound order as the record lists it (04 9.12.6): the number, the day, the total, the two badges and the address of the order's screen with the list's address to come back to.</summary>
public sealed record MemberOrderView(string Number, string Date, string Total, OrderBadges Badges, string Path);

/// <summary>The "Hesabı sil" section (04 9.12.7): the button, or - while a revert link is open - the reason with the end date in its place.</summary>
public sealed record MemberDeletionView(Guid CustomerId, string SearchEmail, string? BlockedText);

/// <summary>The record of one searched e-mail as the screen draws it.</summary>
public sealed class MemberRecordView
{
    public required MemberSearchState State { get; init; }

    public string? Email { get; init; }

    public string? Name { get; init; }

    /// <summary>The day the account or the pending registration was made.</summary>
    public string? CreatedDate { get; init; }

    public string? EmailPathLine { get; init; }

    public string? GoogleLine { get; init; }

    public IReadOnlyList<MemberAddressView> Addresses { get; init; } = [];

    public IReadOnlyList<MemberOrderView> Orders { get; init; } = [];

    public Pager OrderPager { get; init; } = new(null, null, []);

    public MemberDeletionView? Deletion { get; init; }
}

/// <summary>
/// E-40, the member record screen (T533; 04 9.12; 07 UC-95, UC-157): the search by e-mail, the read-only record of the one account found and the deletion on the data subject's request. The GET writes
/// nothing and the search is not written to the operation trail (02 10.3.1). The deletion is one marked operation (UC-157): without the confirmation mark the answer is the confirmation state; with it the
/// module deletes in the transaction of this request and the screen returns to the search field with a lasting message (04 9.12.10). A deletion whose outcome is uncertain returns to the search, which shows
/// the account if it stands and "no account" if it was deleted (04 9.12.14).
/// </summary>
internal sealed class MembersModel : PageModel, IOutcomeTarget
{
    /// <summary>The label of the search field (04 9.12.3).</summary>
    public const string SearchLabel = "E-posta";

    /// <summary>04 9.12.9 (K-811): the line of an address with no account.</summary>
    public const string NotFoundText = "Bu e-posta adresiyle kayıtlı üye hesabı yok.";

    /// <summary>04 9.12.4 (K-811): the line of a registration whose e-mail was never verified.</summary>
    public const string UnverifiedText = "E-posta adresi henüz doğrulanmamış kayıt.";

    /// <summary>04 9.12.7: the result of the deletion told before the button.</summary>
    public const string DeletionResultText = "Hesabın adı, giriş bilgileri, oturumları, adres defteri ve sepeti hemen silinir; siparişler sürer.";

    /// <summary>04 9.12.10 (K-811): the lasting message after the deletion.</summary>
    public const string DeletedText = "Hesap silindi; hesabın adresine silindiğine dair bir e-posta gönderildi. Siparişleri sipariş listesinde durur.";

    public const string DeleteButtonLabel = "Hesabı sil";

    /// <summary>The named handler of the deletion (07 2.2.1): the name of <see cref="OnPostDeleteCustomerAsync"/> without the prefix and the suffix.</summary>
    public const string DeleteHandler = "DeleteCustomer";

    public const string EmailPathPresentText = "E-posta ve şifre: şifre var";

    public const string EmailPathAbsentText = "E-posta ve şifre: şifre yok";

    public const string GoogleLinkedText = "Google: bağlı";

    public const string GoogleNotLinkedText = "Google: bağlı değil";

    public const string NoAddressText = "Kayıtlı adres yok.";

    public const string NoOrderText = "Hesaba bağlı sipariş yok.";

    private readonly IMembershipModule _membership;
    private readonly IOrderingModule _ordering;
    private readonly ITransactionRunner _transactions;

    private bool _loaded;
    private string _outcomeAddress = PanelFramePaths.Members;

    public MembersModel(IMembershipModule membership, IOrderingModule ordering, ITransactionRunner transactions)
    {
        ArgumentNullException.ThrowIfNull(membership);
        ArgumentNullException.ThrowIfNull(ordering);
        ArgumentNullException.ThrowIfNull(transactions);
        _membership = membership;
        _ordering = ordering;
        _transactions = transactions;
    }

    /// <summary>The search field as the screen draws it; carries the field message of a wrong e-mail.</summary>
    public ListSearch Search { get; private set; } = new(ListKind.PanelMembers, PanelFramePaths.Members, SearchLabel, null);

    /// <summary>The record of the searched e-mail; null when nothing was searched.</summary>
    public MemberRecordView? Record { get; private set; }

    /// <summary>The lasting message of a finished deletion (04 9.12.10); null otherwise.</summary>
    public MessageView? DeletedMessage { get; private set; }

    /// <summary>The confirmation state of the deletion (04 9.12.10); null outside it.</summary>
    public ConfirmView? Confirmation { get; private set; }

    /// <summary>The screen that shows the result when the outcome of the deletion is uncertain: the search of the e-mail the form carried (09 8.1 item 4).</summary>
    public string OutcomeScreen() => _outcomeAddress;

    // A form that fails to bind is shown again by the validation filter without the handler (09 3.4 item 3); the page it shows needs the record of the sent e-mail.
    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        PageHandlerExecutedContext executed = await next();
        if (!_loaded && executed.Exception is null && executed.Result is PageResult { StatusCode: StatusCodes.Status422UnprocessableEntity }
            && context.HandlerArguments.Values.OfType<DeleteCustomerInput>().FirstOrDefault() is { } input)
        {
            _loaded = true;
            await LoadAsync(input.SearchEmail, PageNumber.First, HttpContext.RequestAborted);
        }
    }

    public async Task<IActionResult> OnGetAsync(CustomerQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        _loaded = true;
        DeletedMessage = NoticeParameter.Read(Request, NoticeKind.Result) == NoticeCode.CustomerDeleted ? Messages.InPlace(MessageMeaning.Success, DeletedText, "member-deleted") : null;

        string? fieldMessage = SearchFieldMessage();
        if (fieldMessage is not null)
        {
            // A wrong e-mail is a field message; no search is made (04 9.12.12).
            Search = Search with { Value = query.Search, Message = fieldMessage };
            return Page();
        }

        await LoadAsync(query.Search, query.RequestedPage(), ct);
        return Page();
    }

    /// <summary>UC-157: deletes the account on the data subject's request. Without the confirmation mark: the confirmation state (200). Confirmed: one transaction; a revert link open is 422 and nothing is deleted; no account is 404.</summary>
    [OutcomeUncertain("UC-157")]
    public async Task<IActionResult> OnPostDeleteCustomerAsync(DeleteCustomerInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        _loaded = true;
        string searched = input.SearchEmail?.Trim() ?? string.Empty;

        // The first line: from here on an error that leaves the outcome uncertain returns to the search of the e-mail (09 8.1 item 4).
        _outcomeAddress = ListAddress.Of(PanelFramePaths.Members, new CustomerQuery { Search = searched }, PageNumber.First);

        if (!input.Confirmed)
        {
            MemberRecord record = await _membership.FindMemberRecordAsync(searched, ct);
            if (!record.IsAccount || record.CustomerId != input.CustomerId)
            {
                return AccessDenial.NotFound();
            }

            if (record.DeletionBlockedUntil is not null)
            {
                await LoadAsync(searched, PageNumber.First, ct);
                return FormResults.Redisplay(this);
            }

            await LoadAsync(searched, PageNumber.First, ct);
            Confirmation = Confirmations.ForIrreversible(
                IrreversibleConfirmations.AccountDeletion(),
                DeleteButtonLabel,
                DeleteButtonLabel,
                PanelFramePaths.Members,
                DeleteHandler,
                _outcomeAddress,
                [
                    new(nameof(DeleteCustomerInput.CustomerId), input.CustomerId.ToString()),
                    new(nameof(DeleteCustomerInput.SearchEmail), searched),
                    new(nameof(DeleteCustomerInput.Confirmed), "true"),
                ]);
            return Page();
        }

        AccountDeletionResult result = await _transactions.RunAsync(token => _membership.DeleteAccountOnRequestAsync(input.CustomerId, token), ct);
        if (result.IsDeleted)
        {
            // The screen returns to the search field with the lasting message (04 9.12.10); the search is not carried.
            return FormResults.SeeOther(PanelFramePaths.Members, NoticeCode.CustomerDeleted);
        }

        if (result.Code == ErrorCode.EmailChangeRevertPending)
        {
            await LoadAsync(searched, PageNumber.First, ct);
            return FormResults.Redisplay(this);
        }

        return AccessDenial.NotFound();
    }

    // The message the validation filter wrote for the search field of a wrong e-mail (04 9.12.12).
    private string? SearchFieldMessage() =>
        ModelState.FirstOrDefault(e => e.Key.EndsWith(nameof(CustomerQuery.Search), StringComparison.OrdinalIgnoreCase) && e.Value?.Errors.Count > 0).Value?.Errors[0].ErrorMessage;

    private async Task LoadAsync(string? email, int page, CancellationToken ct)
    {
        string searched = email?.Trim() ?? string.Empty;
        Search = Search with { Value = searched.Length > 0 ? searched : null };
        if (searched.Length == 0)
        {
            Record = new MemberRecordView { State = MemberSearchState.NotSearched };
            return;
        }

        MemberRecord record = await _membership.FindMemberRecordAsync(searched, ct);
        if (!record.Exists)
        {
            Record = new MemberRecordView { State = MemberSearchState.NotFound };
            return;
        }

        string created = record.CreatedAt is { } at ? DisplayFormat.Date(at) : string.Empty;
        if (!record.IsAccount || record.CustomerId is not { } customerId)
        {
            Record = new MemberRecordView { State = MemberSearchState.Unverified, Email = record.Email, CreatedDate = created };
            return;
        }

        CustomerQuery query = new() { Search = searched };
        CustomerOrderPage orders = await _ordering.ListCustomerOrdersAsync(customerId, page, ct);
        string listAddress = ListAddress.Of(PanelFramePaths.Members, query, orders.PageNumber);
        string? blocked = record.DeletionBlockedUntil is { } until
            ? $"Bu hesabın önceki e-posta adresine gönderilen geri alma bağlantısı {DisplayFormat.Date(until)} tarihine kadar açık. Hesap bu tarihe kadar silinemez."
            : null;

        Record = new MemberRecordView
        {
            State = MemberSearchState.Account,
            Email = record.Email,
            Name = record.Name,
            CreatedDate = created,
            EmailPathLine = record.HasPassword ? EmailPathPresentText : EmailPathAbsentText,
            GoogleLine = record.IsGoogleLinked ? GoogleLinkedText : GoogleNotLinkedText,
            Addresses = [.. record.Addresses.Select(a => new MemberAddressView(a.Name, a.RecipientName, a.ProvinceName, a.District, a.AddressLine, a.Phone is { Length: > 0 } phone ? DisplayFormat.Phone(phone) : null))],
            Orders = [.. orders.Items.Select(o => new MemberOrderView(
                o.OrderNumber,
                DisplayFormat.Date(o.CreatedAt),
                DisplayFormat.Amount(o.Total),
                StatusBadges.ForOrder(o.OrderStatus, o.PaymentStatus),
                ListReturn.LinkTo(PanelFramePaths.Orders + "/" + o.OrderId.ToString(), listAddress)))],
            OrderPager = Pager.Of(PanelFramePaths.Members, query, orders.PageNumber, orders.PageCount),
            Deletion = new MemberDeletionView(customerId, searched, blocked),
        };
    }
}
