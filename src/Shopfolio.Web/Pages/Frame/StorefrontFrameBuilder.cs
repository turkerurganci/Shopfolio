using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Pages.Frame;

/// <summary>
/// Builds the storefront frame from the request's context (T537; 07 5.5.1; K-1172 (2)): who the browser is - a customer session, a panel session, neither -, which cart it holds, what the firm has
/// published, and what the gates say now. Everything is read in this request and nothing is kept: the sales gate and the gate of the data-collecting entries are computed here every time (05 4.1.5),
/// and a change of the brand, the menu, the announcement or the cart shows in the next response. The administrator's session is no session of the storefront: the account area and the cart
/// follow the customer cookie and the cart cookie only (04 2.9.4). Reads go through the modules' open surfaces and no more (09 6 item 2).
/// </summary>
internal sealed class StorefrontFrameBuilder
{
    private readonly ICorporateContentModule _content;
    private readonly ISettingsModule _settings;
    private readonly IMembershipModule _membership;
    private readonly IOrderingModule _ordering;
    private readonly ITransactionRunner _transactions;

    public StorefrontFrameBuilder(
        ICorporateContentModule content, ISettingsModule settings, IMembershipModule membership, IOrderingModule ordering, ITransactionRunner transactions)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(membership);
        ArgumentNullException.ThrowIfNull(ordering);
        ArgumentNullException.ThrowIfNull(transactions);
        _content = content;
        _settings = settings;
        _membership = membership;
        _ordering = ordering;
        _transactions = transactions;
    }

    /// <summary>The longest cart cookie value that is looked up; a real key is 43 characters (09 15.1 item 1).</summary>
    private const int CartCookieValueLimit = 256;

    /// <summary>True when the request is the payment step's own page (UC-22): the frame is reduced there (04 2.2.7).</summary>
    public static bool IsCheckoutStep(HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        string path = request.Path.Value ?? string.Empty;
        return string.Equals(path.TrimEnd('/'), FramePaths.Checkout, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// The frame of this request. <paramref name="readsMergeNotice"/> is true for a response that draws a page: the sign-in merge's message is then read and its marks cleared in this request, so it shows
    /// once (07 4.34.3). A redirect, a machine endpoint and the error page pass false and read no mark.
    /// </summary>
    public async Task<StorefrontFrame> BuildAsync(HttpContext context, bool readsMergeNotice, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(context);
        bool isCheckout = IsCheckoutStep(context.Request);

        Guid? customerId = CustomerOf(context);
        string? memberName = customerId is Guid id ? await _membership.GetMemberNameAsync(id, ct) : null;
        if (memberName is null)
        {
            // A session whose account is gone is no session: the frame treats the browser as a visitor.
            customerId = null;
        }

        bool isAdministrator = await context.HasAdminSessionAsync();

        BrandIdentity brand = await _content.GetBrandIdentityAsync(ct);
        StorefrontMenu menu = await _content.GetMenuAsync(ct);
        AnnouncementView? announcement = isCheckout ? null : await _content.GetAnnouncementAsync(isAdministrator, ct);

        CartBadge badge = await ReadBadgeAsync(context, customerId, ct);
        IReadOnlyList<string> mergeAdded = [];
        if (readsMergeNotice && customerId is Guid memberId && badge.HasMergeNotice)
        {
            mergeAdded = (await _transactions.RunAsync(async token => new MergeNoticeTaken(await _ordering.TakeMergeNoticeAsync(memberId, token)), ct)).ProductNames;
        }

        SalesGate gate = await _settings.GetSalesGateAsync(ct);
        LegalIdentity identity = await _settings.GetLegalIdentityAsync(ct);
        IReadOnlyList<LegalTextSummary> legalTexts = await _settings.GetLegalTextsAsync(ct);
        string? whatsApp = await _settings.GetWhatsAppNumberAsync(ct);
        IReadOnlyList<SocialLinkItem> social = await _content.ListSocialLinksAsync(ct);

        return new StorefrontFrame(
            new FrameBrand(brand.DisplayName, brand.LogoId, brand.FaviconId, brand.FaviconInitial, brand.Color, brand.TextColor),
            isCheckout ? null : new FrameMenu(menu.Layout, [.. menu.VisibleItems], menu.Categories),
            announcement,
            isCheckout ? null : new FrameSearch(FramePaths.Search, FramePaths.SearchParameter),
            AccountOf(context, memberName),
            new FrameCart(FramePaths.Cart, badge.ItemCount),
            new FrameContact(social, string.IsNullOrWhiteSpace(whatsApp) ? null : whatsApp),
            FooterOf(identity, menu, legalTexts, brand),
            new FrameSales(gate.ShowsClosedNotice, gate.IsTemporarilyClosed, gate.IsDataCollectionOpen),
            isAdministrator ? new FrameAdminStrip(FramePaths.Panel) : null,
            mergeAdded,
            isCheckout);
    }

    // The customer identity of the request, or null: only the customer scheme counts - the panel's session is no session of the storefront (04 2.9.4).
    private static Guid? CustomerOf(HttpContext context)
    {
        if (context.User.Identity is { IsAuthenticated: true, AuthenticationType: SessionSchemes.CustomerScheme })
        {
            return context.User.AccountId();
        }

        return null;
    }

    private async Task<CartBadge> ReadBadgeAsync(HttpContext context, Guid? customerId, CancellationToken ct)
    {
        if (customerId is Guid memberId)
        {
            return await _ordering.ReadCartBadgeAsync(CartOwner.ForMember(memberId), ct);
        }

        string? key = BrowserCookies.Read(context, CookieNames.Cart);
        if (string.IsNullOrWhiteSpace(key) || key.Length > CartCookieValueLimit)
        {
            return CartBadge.Empty;
        }

        return await _ordering.ReadCartBadgeAsync(CartOwner.ForGuest(key), ct);
    }

    private static FrameAccount AccountOf(HttpContext context, string? memberName)
    {
        string current = CurrentAddress(context.Request);
        string signIn = string.Equals(context.Request.Path.Value?.TrimEnd('/'), FramePaths.SignIn, StringComparison.OrdinalIgnoreCase) || !ReturnPath.IsLocalPath(current)
            ? FramePaths.SignIn
            : $"{FramePaths.SignIn}?{ReturnPath.ParameterName}={Uri.EscapeDataString(current)}";
        return new FrameAccount(
            memberName,
            signIn,
            FramePaths.OrderTracking,
            FramePaths.Account,
            FramePaths.AccountOrders,
            memberName is null ? null : new FrameSignOutForm(FramePaths.SignOut, ReturnPath.IsLocalPath(current) ? current : "/"));
    }

    private static string CurrentAddress(HttpRequest request) => $"{request.Path.Value}{request.QueryString.Value}";

    private static FrameFooter FooterOf(LegalIdentity identity, StorefrontMenu menu, IReadOnlyList<LegalTextSummary> legalTexts, BrandIdentity brand)
    {
        List<FrameLegalLink> links = [];
        if (IsPublished(legalTexts, LegalTextType.PrivacyNotice))
        {
            links.Add(new FrameLegalLink(FramePaths.PrivacyNoticeTitle, FramePaths.PrivacyNotice));
        }

        if (IsPublished(legalTexts, LegalTextType.CookiePolicy))
        {
            links.Add(new FrameLegalLink(FramePaths.CookiePolicyTitle, FramePaths.CookiePolicy));
        }

        // The process guide is the product's own fixed text: it waits for no publication and shows in every state (02 3.24.8).
        links.Add(new FrameLegalLink(ProcessGuide.Title, FramePaths.ProcessGuide));

        string? etbis = string.IsNullOrWhiteSpace(identity.EtbisVerificationUrl) ? null : identity.EtbisVerificationUrl;
        return new FrameFooter(FramePaths.Contact, identity, etbis, menu.FooterPages, links, brand.ShowsPlatformSignature);
    }

    private static bool IsPublished(IReadOnlyList<LegalTextSummary> texts, LegalTextType type) => texts.Any(t => t.Type == type && t.LastVersionNumber is not null);

    /// <summary>The once-told merge message as the transaction's value: nothing here can be refused.</summary>
    private sealed record MergeNoticeTaken(IReadOnlyList<string> ProductNames) : IOperationResult
    {
        public bool IsRejected => false;
    }
}
