using System.ComponentModel.DataAnnotations;
using System.Globalization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

using FieldRejection = Shopfolio.Modules.Settings.Contracts.FieldRejection;

namespace Shopfolio.Pages.Panel;

/// <summary>The form input of the payment methods form (E-45; 07 9.17.2 <c>SavePaymentMethodsInput</c>; 04 7.2.20): the transfer switch, the IBAN, the section's version and the confirmation mark.</summary>
public sealed class SavePaymentMethodsInput : IFormInput
{
    [Display(Name = "Havale/EFT")]
    public bool IsBankTransferEnabled { get; set; }

    [Display(Name = "IBAN")]
    [InvalidIban]
    public string? BankTransferIban { get; set; }

    /// <summary>The version of the payment methods section the form was opened with (06 4.2; 07 2.4.1).</summary>
    public int PaymentMethodsVersion { get; set; }

    /// <summary>True when the confirmation window's button sent the form (07 2.2.4; K-1194 (2)).</summary>
    [SpecAssignedName]
    public bool Confirmed { get; set; }
}

/// <summary>
/// E-45, the payment methods screen (T536; 04 9.17; 07 UC-100, UC-167): the card's read-only line from the installation, the bank transfer switch and its IBAN in one form. The GET writes nothing.
/// Switching the transfer on and the first IBAN are saved without a confirmation; changing a filled IBAN and switching the transfer off answer the first send with the confirmation (200) that says how
/// many bank transfer orders wait for their payment (K-663, K-664, K-814, K-1194 (2)); the confirmed send saves. The rules are the Settings module's (<see cref="IPaymentMethodSettings"/>).
/// </summary>
[NoStore]
public sealed class PaymentMethodsModel : PageModel
{
    public const string CardOpenText = "Kart ile ödeme açık: sağlayıcı anahtarları kurulumda tanımlı. Kart panelden açılıp kapatılmaz.";

    public const string CardUndefinedText = "Kart ile ödeme kurulumda tanımlı değil; ödeme adımında görünmez.";

    public const string OnlyTransferText = "Satış yalnız havaleyle açık.";

    public const string NoMethodText = "Açık ödeme yöntemi yok: satış kapısının ödeme yöntemi koşulu karşılanmıyor.";

    public const string IbanNote = "IBAN her siparişe onay anındaki hâliyle yazılır; değişiklik yalnız yeni siparişlere işler. IBAN'ın girilmesi ve değişmesi bütün yöneticilere e-postayla bildirilir.";

    public const string PaymentWindowNote = "Havale ödeme süresi Kargo, süreler ve varsayılanlar ekranındadır.";

    public const string FreeTextReminder = "SSS ya da sayfa metinlerinde ödeme yöntemlerinden söz ediyorsanız onları da güncelleyin.";

    /// <summary>K-817: the message of a transfer switched on without an IBAN.</summary>
    public const string IbanRequiredText = "Havaleyi açmak için IBAN girin.";

    public const string ChangeIbanLabel = "IBAN'ı değiştir";

    public const string CloseTransferLabel = "Havaleyi kapat";

    private readonly IPaymentMethodSettings _methods;
    private readonly ICardPaymentCondition _card;
    private readonly IOrderingModule _ordering;
    private SavePaymentMethodsInput? _posted;
    private bool _conflicted;
    private PaymentMethodsSection? _section;
    private (FormErrorSummary Summary, IReadOnlyList<FieldView> Fields)? _built;

    public PaymentMethodsModel(IPaymentMethodSettings methods, ICardPaymentCondition card, IOrderingModule ordering)
    {
        _methods = methods;
        _card = card;
        _ordering = ordering;
    }

    public bool IsCardOpen { get; private set; }

    public bool IsTransferOpen { get; private set; }

    /// <summary>The line under the two methods (04 9.17.11): transfer only, no method, or none when the card is open.</summary>
    public string? StateLine => IsCardOpen ? null : IsTransferOpen ? OnlyTransferText : NoMethodText;

    public bool IsBankTransferEnabledShown { get; private set; }

    public int FormVersion { get; private set; }

    public bool IsConflicted => _conflicted;

    public FormErrorSummary Summary => Built().Summary;

    /// <summary>The IBAN field; its value is the stored IBAN in groups of four (04 9.17.15) or the sent one.</summary>
    public FieldView IbanField => Built().Fields[0];

    public ConfirmView? Confirm { get; private set; }

    public MessageView? Notice { get; private set; }

    public override async Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        await LoadAsync(HttpContext.RequestAborted);
    }

    public IActionResult OnGet() => Page();

    /// <summary>
    /// UC-167: saves the switch and the IBAN. A change that asks for a confirmation answers its first send with the confirmation (200, nothing written); the confirmed send and a change that asks
    /// for none save in one transaction and answer 303 with the notice. 422 by field; 409 on the section's version.
    /// </summary>
    [OutcomeUncertain("UC-167")]
    public async Task<IActionResult> OnPostSavePaymentMethodsAsync(SavePaymentMethodsInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        PaymentMethodsSection current = await _methods.GetAsync(ct);
        string? iban = string.IsNullOrWhiteSpace(input.BankTransferIban) ? null : RequestFormat.CompactIban(input.BankTransferIban);
        bool changesIban = current.BankTransferIban is not null && iban is not null && !string.Equals(current.BankTransferIban, iban, StringComparison.Ordinal);
        // 07 9.17 (K-1194 (2)): a filled IBAN's change asks for the confirmation; emptying it while the transfer stays off is such a change too - the document names only the opening and the
        // first IBAN as unconfirmed. With the transfer on, an emptied IBAN is the 422 below (04 7.2.20.2).
        bool clearsIban = current.BankTransferIban is not null && iban is null;
        bool closesTransfer = current.IsBankTransferEnabled && !input.IsBankTransferEnabled;
        if (!input.Confirmed && (changesIban || clearsIban || closesTransfer) && input.PaymentMethodsVersion == current.Version && (iban is not null || !input.IsBankTransferEnabled))
        {
            int awaiting = await _ordering.CountBankTransferOrdersAwaitingPaymentAsync(ct);
            bool cardOpen = await _card.IsOpenAsync(ct);
            List<string> sentences = [];
            if (changesIban)
            {
                sentences.Add(IbanChangeSentence(awaiting));
            }

            if (clearsIban)
            {
                sentences.Add(IbanClearSentence(awaiting));
            }

            if (closesTransfer)
            {
                sentences.Add(CloseSentence(awaiting, cardOpen));
            }

            string label = closesTransfer ? CloseTransferLabel : ChangeIbanLabel;
            Confirm = Confirmations.ForSettingChange(
                label,
                string.Join(" ", sentences),
                label,
                PanelFramePaths.PaymentMethods,
                "SavePaymentMethods",
                PanelFramePaths.PaymentMethods,
                [
                    new("Input.IsBankTransferEnabled", input.IsBankTransferEnabled ? "true" : "false"),
                    new("Input.BankTransferIban", input.BankTransferIban ?? string.Empty),
                    new("Input.PaymentMethodsVersion", input.PaymentMethodsVersion.ToString(CultureInfo.InvariantCulture)),
                    new("Input.Confirmed", "true"),
                ]);
            _posted = input;
            return Page();
        }

        ITransactionRunner runner = HttpContext.RequestServices.GetRequiredService<ITransactionRunner>();
        PaymentMethodsInput values = new(input.IsBankTransferEnabled, input.BankTransferIban);
        SettingsSaveResult result = await runner.RunAsync(token => _methods.SaveAsync(values, input.PaymentMethodsVersion, token), ct);
        if (result.IsSaved)
        {
            return FormResults.SeeOther(PanelFramePaths.PaymentMethods, NoticeCode.Saved);
        }

        _posted = input;
        _conflicted = result.IsVersionConflict;
        foreach (FieldRejection rejection in result.Rejections)
        {
            AddRejection(rejection, current);
        }

        await LoadAsync(ct);
        return _conflicted ? FormResults.Conflict(this) : FormResults.Redisplay(this);
    }

    /// <summary>K-814: the IBAN change's sentence; with no waiting order the second half of the first sentence drops.</summary>
    public static string IbanChangeSentence(int awaiting)
    {
        string first = awaiting > 0
            ? $"Yeni IBAN yalnız yeni siparişlere işleyecek; ödemesi beklenen {Number(awaiting)} havale siparişi eski IBAN'ı göstermeye devam edecek."
            : "Yeni IBAN yalnız yeni siparişlere işleyecek.";
        return first + " Değişiklik bütün yöneticilere e-postayla bildirilecek.";
    }

    /// <summary>
    /// The emptied IBAN's sentence, on K-814's IBAN change pattern: the waiting orders keep the frozen IBAN; an emptied IBAN tells no one (02 9.3.3 - neither entered nor changed to a value).
    /// </summary>
    public static string IbanClearSentence(int awaiting) => awaiting > 0
        ? $"IBAN silinecek; ödemesi beklenen {Number(awaiting)} havale siparişi eski IBAN'ı göstermeye devam edecek."
        : "IBAN silinecek.";

    /// <summary>K-814: the closing's sentence; the sales gate sentence only when no other method is open.</summary>
    public static string CloseSentence(int awaiting, bool otherMethodOpen)
    {
        string text = "Havale yeni siparişlerde görünmeyecek.";
        if (awaiting > 0)
        {
            text += $" Ödemesi beklenen {Number(awaiting)} havale siparişi etkilenmez; donmuş IBAN'larıyla sürer.";
        }

        if (!otherMethodOpen)
        {
            text += " Açık başka ödeme yöntemi olmadığı için satış da kapanacak.";
        }

        return text;
    }

    /// <summary>The IBAN in groups of four, so it reads without breaking on a narrow screen (04 9.17.15).</summary>
    public static string Grouped(string iban)
    {
        ArgumentNullException.ThrowIfNull(iban);
        return string.Join(' ', iban.Chunk(4).Select(chunk => new string(chunk)));
    }

    private static string Number(int value) => value.ToString(CultureInfo.InvariantCulture);

    private void AddRejection(FieldRejection rejection, PaymentMethodsSection current)
    {
        string field = "Input." + rejection.Field;
        if (rejection.Code == FieldCodes.FieldRequired && rejection.Field == nameof(PaymentMethodsInput.BankTransferIban))
        {
            if (current.BankTransferIban is not null)
            {
                // 04 7.2.20.2: a filled IBAN cannot be emptied while the transfer is on.
                ModelState.AddFieldError(field, FieldCodes.FilledFieldCleared);
                return;
            }

            // K-817: the transfer is not switched on without an IBAN; the text is the decision's, the log line carries the code (09 8.2 item 3).
            HttpContext.RequestServices.GetRequiredService<RejectionLog>().Rejected(ErrorCode.BankTransferIbanRequired);
            ModelState.AddModelError(field, IbanRequiredText);
            return;
        }

        ModelState.AddFieldError(field, rejection.Code);
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        PaymentMethodsSection section = await _methods.GetAsync(ct);
        IsCardOpen = await _card.IsOpenAsync(ct);
        IsTransferOpen = section.IsBankTransferEnabled && section.BankTransferIban is not null;
        _section = section;
        FormVersion = _posted is null || _conflicted ? section.Version : _posted.PaymentMethodsVersion;
        IsBankTransferEnabledShown = _posted?.IsBankTransferEnabled ?? section.IsBankTransferEnabled;
        _built = null;
        NoticeCode? notice = NoticeParameter.Read(Request, NoticeKind.ShortLived);
        Notice = notice is { } code && NoticeTexts.TryShortLived(code, out string? noticeText) ? Messages.ShortLivedSuccess(noticeText) : null;
    }

    private (FormErrorSummary Summary, IReadOnlyList<FieldView> Fields) Built()
    {
        if (_built is { } built)
        {
            return built;
        }

        PaymentMethodsSection section = _section ?? throw new InvalidOperationException("The screen is read before it is loaded.");
        FormErrorSummary summary = FormErrorSummary.From(ModelState, ["Input.IsBankTransferEnabled", "Input.BankTransferIban"]);
        string? value = _posted is not null ? _posted.BankTransferIban : section.BankTransferIban is { } stored ? Grouped(stored) : null;
        FieldView iban = Fields.Text("Input.BankTransferIban", IbanFields.Label, IsBankTransferEnabledShown ? FieldRequirement.Required : FieldRequirement.Optional, value, autocomplete: "off");
        _built = (summary, [Fields.Bind(iban, summary)]);
        return _built.Value;
    }
}
