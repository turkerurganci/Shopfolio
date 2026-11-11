using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Components;

/// <summary>
/// The empty-state line of a list the user opened (T541; 04 2.7.1.2, K-765): one sentence that says what is not there and a way out. An empty state is not an error: it carries no error symbol
/// (<c>_EmptyRow</c> draws none). The way out is either a link back - the storefront's home or the categories, never a "tüm ürünler" page (K-754) - or, in a list the user fills in himself, the button that
/// opens the first record. The other biçim of an empty region - the invisible one (04 2.7.1.1) - has no view: the region is not drawn.
/// </summary>
/// <param name="Sentence">What is not there, one sentence.</param>
/// <param name="ExitLabel">The text of the way out.</param>
/// <param name="ExitPath">Where the way out goes; a local address.</param>
/// <param name="IsFirstRecordButton">True when the way out is the button that opens the first record of a list the user fills in (04 2.7.1.2).</param>
public sealed record EmptyStateView(string Sentence, string ExitLabel, string ExitPath, bool IsFirstRecordButton);

/// <summary>The only constructors of an <see cref="EmptyStateView"/>: both ask for the sentence and a way out (04 2.7.1.2).</summary>
public static class EmptyStates
{
    /// <summary>A list without content on the storefront or the panel: the sentence and the link that leads out of it ("Ana sayfaya dön", "Ürünlere dön").</summary>
    public static EmptyStateView WithExit(string sentence, string exitLabel, string exitPath) => Build(sentence, exitLabel, exitPath, false);

    /// <summary>A list the user opens and fills in himself (the address book, a panel list opened from the panel): the button that opens the first record is the way out.</summary>
    public static EmptyStateView WithFirstRecordButton(string sentence, string buttonLabel, string openPath) => Build(sentence, buttonLabel, openPath, true);

    private static EmptyStateView Build(string sentence, string exitLabel, string exitPath, bool isButton)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sentence);
        ArgumentException.ThrowIfNullOrWhiteSpace(exitLabel);
        if (!Infrastructure.Security.ReturnPath.IsLocalPath(exitPath))
        {
            throw new ArgumentException("The way out is a path of this site (05 6.5.6).", nameof(exitPath));
        }

        return new EmptyStateView(sentence.Trim(), exitLabel.Trim(), exitPath, isButton);
    }
}

/// <summary>
/// The loading state of a region (T541; 04 2.7.2): the frame stays in place and the region that waits keeps its place and carries the text of the waiting - the whole screen is never covered. A
/// button that starts an operation says that it is working and can not be pressed twice (<see cref="LoadingViews.BusyLabelOf"/>).
/// </summary>
/// <param name="Text">What is being waited for, in words; the indicator never stands without text.</param>
public sealed record LoadingView(string Text);

/// <summary>The constructors of the loading state and the text of a busy button (04 2.7.2).</summary>
public static class LoadingViews
{
    /// <summary>The loading state of a region; the text names what is waited for ("Siparişler yükleniyor").</summary>
    public static LoadingView Of(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        return new LoadingView(text.Trim());
    }

    /// <summary>The text a button takes while its operation runs: it says that the operation continues (04 2.7.2; K-744).</summary>
    public static string BusyLabelOf(string label)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        return label.Trim() + " sürüyor";
    }
}

/// <summary>The three states of an unexpected error (04 2.7.3; K-745) as the pages show them.</summary>
public enum UnexpectedErrorKind
{
    /// <summary>04 2.7.3.1: a form or a button operation could not be completed; the lasting message in place; the form keeps its content (the page filter writes it into the page).</summary>
    Operation,

    /// <summary>04 2.7.3.2: the page could not be opened; an error page inside the frame, with "Yeniden dene" and the way home.</summary>
    Page,

    /// <summary>04 2.7.3.3: the result of an operation is unknown; the user is taken to see the result (the layer answers 303), the page below only says where to look.</summary>
    OutcomeUncertain,
}

/// <summary>Which surface an error page is drawn in (04 2.7.3.2): the storefront's frame, the panel's frame, or the bare panel title of the screens that have no frame (04 2.2.9).</summary>
public enum ErrorSurface
{
    Storefront,
    Panel,
    PanelBare,
}

/// <summary>
/// The view model of the error page (T541; 04 2.7.3.2; D-135): the short explanation, "Yeniden dene" (the address of the request that failed) and the way home. No error code, no request id, no
/// technical detail (07 2.7.3). <paramref name="Surface"/> chooses the layout: the page is drawn inside the storefront's frame, inside the panel's frame, or - on the panel screens that have no frame
/// - bare, with the panel title only.
/// </summary>
public sealed record ErrorPageView(ErrorSurface Surface, string Title, string Message, string RetryPath, string HomePath, string HomeLabel);

/// <summary>
/// The sales closed state (T541; 04 2.8). The storefront only says that sales are closed - never which condition is not met (04 2.8.1); the panel says it in four places and names the unmet condition
/// (04 2.8.2). The data-collecting entries have a gate of their own and say it in place (04 2.8.3).
/// </summary>
public static class SalesClosedMessages
{
    /// <summary>04 2.8.1: the storefront's sentence; it names no condition.</summary>
    public const string StorefrontText = "Satış şu anda kapalı.";

    /// <summary>04 2.8.3: the sentence of a data-collecting entry whose gate is closed; the sign-in of existing accounts and the corporate pages are not touched.</summary>
    public const string DataEntryText = "Bu form şu anda kullanıma kapalı.";

    /// <summary>04 2.8.1: the in-place message at the place of the add-to-cart button (E-04), the proceed-to-payment button (E-12) or the confirm button (E-13); null while sales are open.</summary>
    public static MessageView? ForStorefront(FrameSales sales, SalesGateOperation operation)
    {
        ArgumentNullException.ThrowIfNull(sales);
        bool closes = operation is SalesGateOperation.AddToCart or SalesGateOperation.ProceedToCheckout or SalesGateOperation.ConfirmOrder;
        return closes && sales.IsSalesClosed ? Messages.InPlace(MessageMeaning.Info, StorefrontText) : null;
    }

    /// <summary>04 2.8.3: the in-place message of the contact form, the registration and the Google sign-in that opens a new account while their gate is closed; null while it is open.</summary>
    public static MessageView? ForDataEntry(FrameSales sales, DataCollectionEntry entry)
    {
        ArgumentNullException.ThrowIfNull(sales);
        bool collects = entry is DataCollectionEntry.AccountRegistration or DataCollectionEntry.NewAccountGoogleSignIn or DataCollectionEntry.ContactForm;
        return collects && !sales.IsDataCollectionOpen ? Messages.InPlace(MessageMeaning.Info, DataEntryText) : null;
    }
}

/// <summary>One condition of the sales gate as the panel shows it (04 2.8.2): its name, whether it is met and the screen that fixes it.</summary>
public sealed record PanelSalesConditionRow(string Name, bool IsMet, string FixPath);

/// <summary>One line of the panel home's setup checklist (04 2.8.2; 02 10.8.2): the missing item by name and the setting screen it leads to.</summary>
public sealed record PanelChecklistRow(string Name, string FixPath);

/// <summary>
/// The sales state of the panel in the four places of 04 2.8.2, computed once from the sales gate: the top row's indicator (<see cref="IndicatorText"/>), the home page's warning
/// (<see cref="WarningText"/>; null while open), the setup checklist (<see cref="Checklist"/>; empty - the list vanishes - when everything is done) and E-44's "Satış durumu" section
/// (<see cref="Conditions"/>, four rows; the temporary closure switch stays visible always - <see cref="TemporaryClosureSwitchVisible"/>). A temporary closure keeps the switch and alone does
/// not open sales when a condition is missing (04 2.8.2). <see cref="DataEntriesOpen"/> and <see cref="DataEntryConditionNames"/> are the data-collecting gate (04 2.8.3).
/// </summary>
public sealed record PanelSalesStatus(
    bool IsOpen,
    string IndicatorText,
    string? WarningText,
    IReadOnlyList<PanelChecklistRow> Checklist,
    IReadOnlyList<PanelSalesConditionRow> Conditions,
    bool TemporaryClosureSwitchVisible,
    bool DataEntriesOpen,
    IReadOnlyList<string> DataEntryConditionNames)
{
    /// <summary>The four conditions of the gate in the order of 02 3.1.5, each with the screen that fixes it, from one gate value; nothing is derived anywhere else.</summary>
    public static PanelSalesStatus From(SalesGate gate)
    {
        ArgumentNullException.ThrowIfNull(gate);
        bool Unmet(SalesGateCondition condition) => gate.UnmetConditions.Contains(condition);

        SalesGateCondition[] legalParts = [.. new[] { SalesGateCondition.PrivacyNotice, SalesGateCondition.CookiePolicy, SalesGateCondition.ReturnAddress }.Where(Unmet)];
        string legalPath = legalParts.Length > 0 ? PathOf(legalParts[0]) : PanelFramePaths.LegalTexts;
        string legalName = legalParts.Length > 0 ? "Yasal metinler: " + string.Join(", ", legalParts.Select(NameOf)) : "Yasal metinler";
        List<PanelSalesConditionRow> conditions =
        [
            new("Firma kimliği", !Unmet(SalesGateCondition.CompanyIdentity), PanelFramePaths.FirmIdentityAndSales),
            new("Ödeme yöntemi", !Unmet(SalesGateCondition.PaymentMethod), PanelFramePaths.PaymentMethods),
            new("Satışı geçici kapatma anahtarı kapalı", !Unmet(SalesGateCondition.TemporaryClosure), PanelFramePaths.SalesStateSection),
            new(legalName, legalParts.Length == 0, legalPath),
        ];

        List<PanelChecklistRow> checklist = [.. gate.SetupChecklist.Select(c => new PanelChecklistRow(NameOf(c), PathOf(c)))];
        string? warning = gate.IsOpen
            ? null
            : gate.IsTemporarilyClosed && gate.SetupChecklist.Count == 0
                ? "Satış kapalı: satışı geçici kapattınız."
                : "Satış kapalı: " + string.Join(", ", gate.UnmetConditions.Select(NameOf)) + " tamamlanmadı.";
        IReadOnlyList<string> dataNames = [.. new[] { SalesGateCondition.PrivacyNotice, SalesGateCondition.CookiePolicy }.Where(Unmet).Select(NameOf)];

        return new PanelSalesStatus(
            gate.IsOpen,
            gate.IsOpen ? PanelFramePaths.SalesOpenText : PanelFramePaths.SalesClosedText,
            warning,
            checklist,
            conditions,
            TemporaryClosureSwitchVisible: true,
            gate.IsDataCollectionOpen,
            dataNames);
    }

    /// <summary>The name the panel calls a condition by (04 2.8.2): the unmet condition is told by name, the legal texts by the part that waits.</summary>
    public static string NameOf(SalesGateCondition condition) => condition switch
    {
        SalesGateCondition.CompanyIdentity => "Firma kimliği",
        SalesGateCondition.PaymentMethod => "Ödeme yöntemi",
        SalesGateCondition.TemporaryClosure => "Geçici kapatma",
        SalesGateCondition.PrivacyNotice => "Aydınlatma metni",
        SalesGateCondition.CookiePolicy => "Çerez politikası",
        SalesGateCondition.ReturnAddress => "İade adresi",
        _ => throw new ArgumentOutOfRangeException(nameof(condition), condition, "Not a sales gate condition."),
    };

    /// <summary>The screen that fixes a condition: E-44 for the identity and the switch, E-45 for the payment method, E-47 for the two legal texts, E-46 for the return address.</summary>
    public static string PathOf(SalesGateCondition condition) => condition switch
    {
        SalesGateCondition.CompanyIdentity => PanelFramePaths.FirmIdentityAndSales,
        SalesGateCondition.PaymentMethod => PanelFramePaths.PaymentMethods,
        SalesGateCondition.TemporaryClosure => PanelFramePaths.SalesStateSection,
        SalesGateCondition.PrivacyNotice or SalesGateCondition.CookiePolicy => PanelFramePaths.LegalTexts,
        SalesGateCondition.ReturnAddress => PanelFramePaths.ShippingAndDefaults,
        _ => throw new ArgumentOutOfRangeException(nameof(condition), condition, "Not a sales gate condition."),
    };
}
