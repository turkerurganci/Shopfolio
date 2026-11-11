using Shopfolio.Modules.Catalog.Contracts.Categories;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Pages.Frame;

/// <summary>
/// The storefront's frame: the one view model every storefront page response carries and no screen owns (T537; 07 5.5.1; 04 2.2; K-1172 (2)). A page never adds it to its own view; the page filter
/// builds it from the request's context (<see cref="StorefrontFrameBuilder"/>) and the layout reads it. The sales gate and the gate of the data-collecting entries are computed on every
/// request and never stored. At the payment step (04 2.2.7) the frame is reduced: <see cref="Menu"/>, <see cref="Search"/> and <see cref="Announcement"/> are null, the footer stays whole.
/// The frame carries forms and links only for the operations it starts - sign-out, sign-in, the search, the order tracking - and does none of them (07 5.5.2).
/// </summary>
/// <param name="Brand">The brand (04 2.2.3): the logo, else the name, else the domain.</param>
/// <param name="Menu">The top menu; null at the payment step.</param>
/// <param name="Announcement">The announcement strip, null when none shows (04 2.2.2); an administrator's browser also gets a draft, marked; null at the payment step.</param>
/// <param name="Search">The search form of the top section; null at the payment step.</param>
/// <param name="Account">The account area: a visitor's sign-in and order tracking, a member's name (04 2.2.3, 2.9.4).</param>
/// <param name="Cart">The cart link and the number of the browser's cart lines (04 2.2.3, 2.9.4).</param>
/// <param name="Contact">The social links and the WhatsApp number (07 5.5.1).</param>
/// <param name="Footer">The footer, whole at every storefront screen (04 2.2.5, 2.2.6).</param>
/// <param name="Sales">The sales closed state and the gate of the data-collecting entries (04 §2.8; 05 4.1.5).</param>
/// <param name="AdminStrip">The administrator strip, present only while the browser holds a panel session (04 2.9.1); null for everyone else.</param>
/// <param name="MergeAddedProductNames">The products the sign-in merge added, told once in the request that reads them (07 4.34.3); empty when nothing is told.</param>
/// <param name="IsCheckoutStep">True for the reduced frame of the payment step (04 2.2.7).</param>
public sealed record StorefrontFrame(
    FrameBrand Brand,
    FrameMenu? Menu,
    AnnouncementView? Announcement,
    FrameSearch? Search,
    FrameAccount Account,
    FrameCart Cart,
    FrameContact Contact,
    FrameFooter Footer,
    FrameSales Sales,
    FrameAdminStrip? AdminStrip,
    IReadOnlyList<string> MergeAddedProductNames,
    bool IsCheckoutStep);

/// <summary>
/// The brand (06 4.54; 04 2.2.3, 2.2.4): <paramref name="BrandName"/> is the name the firm entered or - until it enters one - the installation's domain; the header shows the logo when
/// <paramref name="LogoId"/> is set and the name as text when not. <paramref name="FaviconId"/> is the site icon, or null and then the <paramref name="FaviconInitial"/> on the brand colour.
/// <paramref name="BrandColor"/> is the firm's colour or the product's default; <paramref name="BrandTextColor"/> is the black or white on it.
/// </summary>
public sealed record FrameBrand(string BrandName, Guid? LogoId, Guid? FaviconId, string FaviconInitial, string BrandColor, string BrandTextColor)
{
    /// <summary>True when the header shows the logo; false when it shows <see cref="BrandName"/> as text.</summary>
    public bool ShowsLogo => LogoId is not null;
}

/// <summary>
/// The top menu (06 4.2, 4.9, 4.51; 02 3.28.4): the layout the home page chose, the items the storefront draws in order - the firm's names, the general pages marked for the menu among them -
/// and the category tree under "Ürünler" (empty while that item is hidden).
/// </summary>
public sealed record FrameMenu(HomepageLayout HomepageLayout, IReadOnlyList<MenuItem> Items, IReadOnlyList<MenuCategory> Categories);

/// <summary>The search form of the top section (UC-03's GET; 07 5.5.2): the address it sends to and the parameter that carries the word.</summary>
public sealed record FrameSearch(string Path, string ParameterName);

/// <summary>
/// The account area (04 2.2.3). <paramref name="MemberName"/> is the signed-in customer's name, null for a visitor - an administrator's session is no session here (04 2.9.4). A visitor gets the
/// <paramref name="SignInPath"/> and the <paramref name="OrderTrackingPath"/>; a member gets the name, <paramref name="AccountPath"/>, <paramref name="OrdersPath"/> and the
/// <paramref name="SignOut"/> form (UC-77), which is null for a visitor.
/// </summary>
public sealed record FrameAccount(string? MemberName, string SignInPath, string OrderTrackingPath, string AccountPath, string OrdersPath, FrameSignOutForm? SignOut)
{
    /// <summary>True when a customer session is open in this browser.</summary>
    public bool IsMember => MemberName is not null;
}

/// <summary>The sign-out form (UC-77; 07 4.34.1): POST to <paramref name="Action"/> with the page the request came from as the hidden <c>ReturnUrl</c>, a local address.</summary>
public sealed record FrameSignOutForm(string Action, string ReturnUrl);

/// <summary>The cart in the top section: the link and the number of live lines in this browser's cart (06 4.16); zero for no cart. At the payment step the link is "Sepete dön" (04 2.2.7).</summary>
public sealed record FrameCart(string Path, int ItemCount);

/// <summary>The social links, one at most per platform in the order of the closed list, and the WhatsApp number - ten digits - or null (06 4.55, 4.2). Nothing floats on the screen (04 2.2.3).</summary>
public sealed record FrameContact(IReadOnlyList<SocialLinkItem> SocialLinks, string? WhatsAppNumber);

/// <summary>
/// The footer (04 2.2.5): the firm's whole identity set under the "İletişim" heading that links to <paramref name="ContactPath"/>, the ETBİS band when the verification address is filled, the
/// published general pages, the published legal texts' links, and the platform signature unless the firm switched it off (06 4.1, 4.51, 4.54, 4.56).
/// </summary>
/// <param name="ContactPath">The contact page, the heading's link; the same place on every page, the payment step included.</param>
/// <param name="Identity">The legal identity set of the company type, as a visitor reads it (02 3.1.4).</param>
/// <param name="EtbisVerificationUrl">The address the ETBİS band points to; null while the field is empty - then there is no band (02 3.1.4).</param>
/// <param name="GeneralPages">Every published general page, in the firm's order (02 3.28.5).</param>
/// <param name="LegalLinks">The legal links that stand: a text appears only while it has a published version; the process guide always does (02 3.24.8, 3.33.5).</param>
/// <param name="ShowsPlatformSignature">The signature at the very bottom, unless the firm switched it off (02 3.29.6).</param>
public sealed record FrameFooter(
    string ContactPath,
    LegalIdentity Identity,
    string? EtbisVerificationUrl,
    IReadOnlyList<GeneralPageLink> GeneralPages,
    IReadOnlyList<FrameLegalLink> LegalLinks,
    bool ShowsPlatformSignature);

/// <summary>One legal link of the footer: the text it shows and the page it opens.</summary>
public sealed record FrameLegalLink(string Title, string Path);

/// <summary>
/// The two gates, read afresh in every request (05 4.1.5; 02 3.1.5): <paramref name="IsSalesClosed"/> is true while the sales gate is not open - the showcase itself stays published -;
/// <paramref name="IsTemporarilyClosed"/> says the firm's own switch is part of it; <paramref name="IsDataCollectionOpen"/> is the gate of the entries that collect personal data - the
/// registration, the contact form (04 2.8).
/// </summary>
public sealed record FrameSales(bool IsSalesClosed, bool IsTemporarilyClosed, bool IsDataCollectionOpen);

/// <summary>The administrator strip above the announcement strip (04 2.9.1): the "Panele dön" link. The "Taslak" band of a draft preview rides on the same condition: a page shows it only while the frame carries this strip (04 2.9.2).</summary>
public sealed record FrameAdminStrip(string PanelPath);
