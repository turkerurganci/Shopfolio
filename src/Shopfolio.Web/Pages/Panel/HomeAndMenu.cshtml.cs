using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

using FieldRejection = Shopfolio.Modules.Settings.Contracts.FieldRejection;

namespace Shopfolio.Pages.Panel;

/// <summary>
/// The form input of the home page and menu screen (E-42; 07 9.14.2 <c>SaveHomepageInput</c>; 04 7.2.17): the layout, the six menu names, the WhatsApp number, one address per platform of the closed
/// list and the section's version. The rules are the modules' (the Settings module checks the section, the content module the links), so a refused save answers all of its fields together.
/// </summary>
public sealed class SaveHomepageInput : IFormInput
{
    public string? HomepageLayout { get; set; }

    public string? MenuNameProducts { get; set; }

    public string? MenuNameOfferings { get; set; }

    public string? MenuNamePortfolioItems { get; set; }

    public string? MenuNameAboutPage { get; set; }

    public string? MenuNameFaqItems { get; set; }

    public string? MenuNameContact { get; set; }

    public string? WhatsAppNumber { get; set; }

    /// <summary>One entry per platform of the closed list, in its order: the platform and the address typed for it (07 9.14.2 <c>SocialLinks</c>; 06 4.55).</summary>
    [SpecAssignedName]
    public List<SocialLinkValue> SocialLinks { get; set; } = [];

    /// <summary>The version of the home page section the form was opened with (06 4.2; 07 2.4.1).</summary>
    public int Version { get; set; }
}

/// <summary>One entry of the form's social links: the platform and its address; an empty address removes the platform's link.</summary>
public sealed class SocialLinkValue
{
    public SocialPlatform SocialPlatform { get; set; }

    public string? Url { get; set; }
}

/// <summary>One menu name field of the screen with the item's state: the field, the default name under it and the reason the item is not on the site, when it is not (04 9.14.4).</summary>
public sealed record MenuNameRowView(FieldView Field, string DefaultName, string? HiddenReason);

/// <summary>One social link field of the screen.</summary>
public sealed record SocialLinkRowView(SocialPlatform Platform, FieldView Field);

/// <summary>
/// E-42, the home page and menu screen (T535; 04 9.14; 07 UC-97, UC-162): one form in three sections - the layout, the six menu names and the social links with the WhatsApp number - and one save.
/// The GET writes nothing; the version the form carries is the section's (06 4.2). The social links are written in the same transaction as the section and their changes reach its trail row.
/// </summary>
[NoStore]
public sealed class HomeAndMenuModel : PageModel
{
    public const string ContentFirstLabel = "Tanıtım öncelikli";

    public const string StoreFirstLabel = "Mağaza öncelikli";

    public const string ContentFirstOrder = "Sıra: kurumsal blok (geniş), Hizmetlerimiz, Referanslarımız, ürün vitrini (tek satır).";

    public const string StoreFirstOrder = "Sıra: ürün vitrini (iki satır), kurumsal blok (dar), Hizmetlerimiz, Referanslarımız.";

    public const string LayoutNote = "İki düzende de kurumsal tanıtım ve ürün vitrini birlikte durur.";

    public const string NoPublishedRecordText = "Menüde görünmüyor — yayında kaydı yok.";

    public const string NoPublishedProductText = "Menüde görünmüyor — yayında ürünü olan kategori yok.";

    public const string DefaultNamePrefix = "Varsayılan ad: ";

    /// <summary>The field message of an empty menu name is this, the item's default name and a full stop (K-813).</summary>
    public const string EmptyNameMessagePrefix = "Menü adı boş olamaz; varsayılan ad: ";

    public const string SocialNote = "Bağlantılar sitenin üst bölümünün ince satırında ve altbilgide görünür. Boş bıraktığınız platformun bağlantısı kaldırılır.";

    public const string WhatsAppHint = "On haneli telefon numarası; isteğe bağlı.";

    private readonly ISettingsModule _settings;
    private readonly ICorporateContentModule _content;
    private SaveHomepageInput? _posted;
    private int _formVersion;

    public HomeAndMenuModel(ISettingsModule settings, ICorporateContentModule content)
    {
        _settings = settings;
        _content = content;
    }

    /// <summary>The platforms in the order the form draws them: the closed list's.</summary>
    public static IReadOnlyList<SocialPlatform> Platforms { get; } = [.. Enum.GetValues<SocialPlatform>()];

    public HomepageLayout Layout { get; private set; }

    /// <summary>The menu name fields in the selected layout's menu order, without İletişim (04 9.14.4): the general pages shown in the menu come after them and before <see cref="ContactRow"/>.</summary>
    public IReadOnlyList<MenuNameRowView> MenuRows { get; private set; } = [];

    public MenuNameRowView ContactRow { get; private set; } = null!; // non-null: LoadAsync sets it before any handler returns Page()

    public IReadOnlyList<SocialLinkRowView> SocialRows { get; private set; } = [];

    public FieldView WhatsApp { get; private set; } = null!; // non-null: LoadAsync sets it before any handler returns Page()

    /// <summary>The general pages the firm shows in the menu, read only (04 9.14.4).</summary>
    public IReadOnlyList<GeneralPageLink> MenuPages { get; private set; } = [];

    public int FormVersion { get; private set; }

    public bool IsConflicted { get; private set; }

    public FormErrorSummary Summary { get; private set; } = null!; // non-null: LoadAsync sets it before any handler returns Page()

    public MessageView? Notice { get; private set; }

    /// <summary>The fields in the order the screen draws them: the menu names in the menu's order, then the links and the number.</summary>
    private static List<string> FormOrderOf(IEnumerable<MenuItemKind> kinds) =>
    [
        .. kinds.Select(kind => "Input." + PropertyOf(kind)),
        .. Platforms.Select(SocialFieldName), "Input.WhatsAppNumber",
    ];

    private static string PropertyOf(MenuItemKind kind) => kind switch
    {
        MenuItemKind.Products => nameof(SaveHomepageInput.MenuNameProducts),
        MenuItemKind.Offerings => nameof(SaveHomepageInput.MenuNameOfferings),
        MenuItemKind.PortfolioItems => nameof(SaveHomepageInput.MenuNamePortfolioItems),
        MenuItemKind.AboutPage => nameof(SaveHomepageInput.MenuNameAboutPage),
        MenuItemKind.FaqItems => nameof(SaveHomepageInput.MenuNameFaqItems),
        MenuItemKind.Contact => nameof(SaveHomepageInput.MenuNameContact),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The kind has no name field."),
    };

    private static string DefaultNameOf(string property) => property switch
    {
        nameof(SaveHomepageInput.MenuNameProducts) => MenuNameDefaults.Products,
        nameof(SaveHomepageInput.MenuNameOfferings) => MenuNameDefaults.Offerings,
        nameof(SaveHomepageInput.MenuNamePortfolioItems) => MenuNameDefaults.PortfolioItems,
        nameof(SaveHomepageInput.MenuNameAboutPage) => MenuNameDefaults.AboutPage,
        nameof(SaveHomepageInput.MenuNameFaqItems) => MenuNameDefaults.FaqItems,
        nameof(SaveHomepageInput.MenuNameContact) => MenuNameDefaults.Contact,
        _ => string.Empty,
    };

    public override async Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        await LoadAsync(HttpContext.RequestAborted);
    }

    public IActionResult OnGet() => Page();

    /// <summary>UC-162: saves the layout, the six names, the WhatsApp number and the links in one transaction; a refusal rolls all of it back.</summary>
    public async Task<IActionResult> OnPostSaveHomepageAsync(SaveHomepageInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        Dictionary<string, string?> links = new(StringComparer.Ordinal);
        foreach (SocialLinkValue link in input.SocialLinks)
        {
            links[link.SocialPlatform.ToString()] = link.Url;
        }

        HomepageLayout? layout = Enum.TryParse(input.HomepageLayout, ignoreCase: false, out HomepageLayout parsed) && Enum.IsDefined(parsed) ? parsed : null;
        ITransactionRunner runner = HttpContext.RequestServices.GetRequiredService<ITransactionRunner>();
        SaveOutcome outcome = await runner.RunAsync(
            async token =>
            {
                SocialLinksSaveResult social = await _content.SaveSocialLinksAsync(links, token);
                HomepageInput section = new(
                    layout,
                    input.MenuNameProducts,
                    input.MenuNameOfferings,
                    input.MenuNamePortfolioItems,
                    input.MenuNameAboutPage,
                    input.MenuNameFaqItems,
                    input.MenuNameContact,
                    input.WhatsAppNumber,
                    social.Changes);
                SettingsSaveResult saved = await _settings.SaveHomepageAsync(section, input.Version, token);
                return new SaveOutcome(social, saved);
            },
            ct);
        if (!outcome.IsRejected)
        {
            return FormResults.SeeOther(PanelFramePaths.HomeAndMenu, NoticeCode.Saved);
        }

        _posted = input;
        _formVersion = input.Version;
        bool conflict = outcome.Settings.IsVersionConflict && !outcome.Social.IsRejected;
        if (conflict)
        {
            IsConflicted = true;
            _formVersion = 0;
        }
        else
        {
            foreach (FieldRejection rejection in outcome.Settings.Rejections)
            {
                string defaultName = DefaultNameOf(rejection.Field);
                if (rejection.Code == FieldCodes.FieldRequired && defaultName.Length > 0)
                {
                    // K-813: an empty menu name's message reminds the default name; it is no catalog text, so the page filter leaves it as written.
                    ModelState.AddModelError("Input." + rejection.Field, EmptyNameMessagePrefix + defaultName + ".");
                    continue;
                }

                ModelState.AddFieldError("Input." + rejection.Field, rejection.Code);
            }

            foreach (SocialLinkRejection rejection in outcome.Social.Rejections)
            {
                ModelState.AddFieldError(SocialFieldName(Enum.TryParse(rejection.Field, out SocialPlatform platform) ? platform : Platforms[0]), rejection.Code);
            }
        }

        await LoadAsync(ct);
        return conflict ? FormResults.Conflict(this) : FormResults.Redisplay(this);
    }

    private async Task LoadAsync(CancellationToken ct)
    {
        HomepageSection section = await _settings.GetHomepageAsync(ct);
        string? whatsApp = await _settings.GetWhatsAppNumberAsync(ct);
        StorefrontMenu menu = await _content.GetMenuAsync(ct);
        IReadOnlyList<SocialLinkItem> links = await _content.ListSocialLinksAsync(ct);
        SaveHomepageInput? sent = _posted;
        // The name fields follow the menu's own order (the layout's, 04 9.14.4); the menu carries it, so the screen does not restate it.
        List<MenuItemKind> kinds = [.. menu.Items.Where(i => i.Kind != MenuItemKind.GeneralPage).Select(i => i.Kind)];
        Summary = FormErrorSummary.From(ModelState, FormOrderOf(kinds));
        FormVersion = IsConflicted || sent is null ? section.Version : _formVersion;
        Layout = sent is not null && Enum.TryParse(sent.HomepageLayout, ignoreCase: false, out HomepageLayout chosen) && Enum.IsDefined(chosen) ? chosen : section.Layout;

        MenuNameRowView RowOf(MenuItemKind kind) => kind switch
        {
            MenuItemKind.Products => Row(menu, kind, "Ürünler", MenuNameDefaults.Products, sent?.MenuNameProducts, section.MenuNames.Products, sent is not null),
            MenuItemKind.Offerings => Row(menu, kind, "Hizmetler", MenuNameDefaults.Offerings, sent?.MenuNameOfferings, section.MenuNames.Offerings, sent is not null),
            MenuItemKind.PortfolioItems => Row(menu, kind, "Referanslar", MenuNameDefaults.PortfolioItems, sent?.MenuNamePortfolioItems, section.MenuNames.PortfolioItems, sent is not null),
            MenuItemKind.AboutPage => Row(menu, kind, "Hakkımızda", MenuNameDefaults.AboutPage, sent?.MenuNameAboutPage, section.MenuNames.AboutPage, sent is not null),
            MenuItemKind.FaqItems => Row(menu, kind, "Sık sorulan sorular", MenuNameDefaults.FaqItems, sent?.MenuNameFaqItems, section.MenuNames.FaqItems, sent is not null),
            _ => Row(menu, MenuItemKind.Contact, "İletişim", MenuNameDefaults.Contact, sent?.MenuNameContact, section.MenuNames.Contact, sent is not null),
        };

        MenuRows = [.. kinds.Where(kind => kind != MenuItemKind.Contact).Select(RowOf)];
        ContactRow = RowOf(MenuItemKind.Contact);

        string? Link(SocialPlatform platform) => sent is not null
            ? sent.SocialLinks.FirstOrDefault(l => l.SocialPlatform == platform)?.Url
            : links.FirstOrDefault(l => l.Platform == platform)?.Url;
        SocialRows = [.. Platforms.Select(platform => SocialRow(platform, Link(platform)))];
        WhatsApp = Fields.Bind(
            Fields.Telephone("Input.WhatsAppNumber", "WhatsApp numarası", FieldRequirement.Optional, sent is not null ? sent.WhatsAppNumber : whatsApp),
            Summary);
        MenuPages = [.. menu.FooterPages.Where(p => p.IsInMenu)];
        NoticeCode? notice = NoticeParameter.Read(Request, NoticeKind.ShortLived);
        Notice = notice is { } code && NoticeTexts.TryShortLived(code, out string? noticeText) ? Messages.ShortLivedSuccess(noticeText) : null;
    }

    private MenuNameRowView Row(StorefrontMenu menu, MenuItemKind kind, string label, string defaultName, string? sentName, string storedName, bool wasSent)
    {
        string property = PropertyOf(kind);
        MenuItem? item = menu.Items.FirstOrDefault(i => i.Kind == kind);
        string? reason = item is { IsVisible: false, HiddenReason: { } why } ? (why == MenuHiddenReason.NoPublishedProduct ? NoPublishedProductText : NoPublishedRecordText) : null;
        FieldView field = Fields.Bind(
            Fields.Text("Input." + property, label + " menü adı", FieldRequirement.Required, wasSent ? sentName : storedName, hint: DefaultNamePrefix + defaultName),
            Summary);
        return new MenuNameRowView(field, defaultName, reason);
    }

    private SocialLinkRowView SocialRow(SocialPlatform platform, string? value) =>
        new(platform, Fields.Bind(Fields.Text(SocialFieldName(platform), platform.ToString(), FieldRequirement.Optional, value), Summary));

    /// <summary>The name of a platform's address field: its entry's place in the closed list.</summary>
    public static string SocialFieldName(SocialPlatform platform) =>
        $"Input.{nameof(SaveHomepageInput.SocialLinks)}[{Platforms.ToList().IndexOf(platform)}].{nameof(SocialLinkValue.Url)}";

    /// <summary>The name of a platform's hidden field that tells the entry's platform.</summary>
    public static string SocialPlatformFieldName(SocialPlatform platform) =>
        $"Input.{nameof(SaveHomepageInput.SocialLinks)}[{Platforms.ToList().IndexOf(platform)}].{nameof(SocialLinkValue.SocialPlatform)}";

    /// <summary>The result of the save's one transaction: either part refusing rolls both back.</summary>
    private sealed class SaveOutcome : IOperationResult
    {
        public SaveOutcome(SocialLinksSaveResult social, SettingsSaveResult settings)
        {
            Social = social;
            Settings = settings;
        }

        public SocialLinksSaveResult Social { get; }

        public SettingsSaveResult Settings { get; }

        public bool IsRejected => Social.IsRejected || !Settings.IsSaved;
    }
}
