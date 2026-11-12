namespace Shopfolio.Modules.CorporateContent.Contracts;

/// <summary>
/// The open surface of the CorporateContent module (09 6 item 1; 05 3.4.1): what other modules, the pages and the commands may call. The module owns the corporate pages, branding and contact requests (05 3.3.7).
/// The members arrive with the tasks that own the module's rules, as methods that take and give immutable records and join the caller's
/// database transaction (09 6 item 3).
/// </summary>
public interface ICorporateContentModule
{
    /// <summary>
    /// The page addresses the site map takes from this module (05 3.7.3; 02 3.30.5, 3.30.6): <c>/hizmet/{slug}</c>, <c>/referans/{slug}</c> and
    /// <c>/sayfa/{slug}</c> of every published record, and <c>/hakkimizda</c> when the about page is published. A draft is not listed. A plain read.
    /// </summary>
    Task<IReadOnlyList<string>> ListSitemapPathsAsync(CancellationToken ct);

    /// <summary>
    /// The storefront's menu (T361; 02 3.28.4-3.28.6; 04 2.2.3): the fixed skeleton in the layout's order with the firm's names, each item with its visibility and - when hidden - its reason;
    /// the categories under "Ürünler"; the published general pages for the footer and the menu. A type with no published record is hidden, a category with no published product below it is
    /// not listed. The structure is cached; the names and the layout are read on every call. A plain read.
    /// </summary>
    Task<StorefrontMenu> GetMenuAsync(CancellationToken ct);

    /// <summary>
    /// The home page (T361; 02 3.28.1-3.28.3; 04 5.1): the blocks that show in the order of the chosen layout - the corporate block, the flagged services and references, the newest
    /// published products - each with what it shows. A block with nothing to show is not there; a draft is never on the page. The corporate part is cached; the showcase is read on every
    /// call. A plain read.
    /// </summary>
    Task<HomepageView> GetHomepageAsync(CancellationToken ct);

    /// <summary>
    /// The announcement strip at the top of every page (T360; 02 3.27.21, 3.27.25): the text and the optional link, or null when no strip shows. A published announcement shows only
    /// inside its dates, on the Turkey day of the time source; <paramref name="asAdministrator"/> - the request of a signed-in administrator - also gets a draft, marked
    /// <see cref="AnnouncementView.IsDraft"/>. Never cached and never stored; a plain read.
    /// </summary>
    Task<AnnouncementView?> GetAnnouncementAsync(bool asAdministrator, CancellationToken ct);

    /// <summary>
    /// One page of a content list page (T505; 02 3.27.4, 3.27.5, 3.27.11, 3.27.25; 04 5.7.3): the published service introductions or reference works in the firm's manual order, 24 a page, a
    /// draft never among them, under the type's name as the firm gave it. A type with no published record gives no cards. A page number past the last shows the last. A plain read.
    /// </summary>
    Task<ContentListView> GetContentListAsync(ContentListKind kind, int page, CancellationToken ct);

    /// <summary>
    /// A content record's page (T505; 02 3.27.3-3.27.5, 3.27.9, 3.27.10, 3.27.25; 04 5.8): Hakkımızda (<paramref name="slug"/> is ignored), a service introduction, a reference work or a general page
    /// with its gallery and - for the first two - the linked products that are published. Null when there is no such page: the record does not exist, the address belongs to another type, or the record
    /// is a draft and <paramref name="asAdministrator"/> - the request of a signed-in administrator - is false; the three are not told apart (04 5.8.9). For the administrator a draft opens with
    /// <see cref="ContentRecordPage.IsDraft"/> set. A plain read.
    /// </summary>
    Task<ContentRecordPage?> GetContentPageAsync(ContentPageKind kind, string? slug, bool asAdministrator, CancellationToken ct);

    /// <summary>The brand form as the panel shows it (T362; 02 3.29; 04 9.15): the stored values and the form's version. A plain read; the panel's view of the brand never carries the brand colour on itself (02 3.29.1).</summary>
    Task<BrandingSection> GetBrandingAsync(CancellationToken ct);

    /// <summary>
    /// The brand form's save (T362; 02 3.29; 07 UC-163): in the caller's open transaction, which the method joins and never opens - the record variant of 09 10.2 item 2, then the trail rows in the
    /// same transaction (the four settings as <c>UpdateBranding</c>, the platform signature switch as <c>UpdateSettings</c>, each with the old and the new value). The brand name cannot be emptied
    /// once entered; a colour that is not <c>#RRGGBB</c> and an image that is not an uploaded image are refused by field; the refusals are results and write nothing. A save that changes nothing
    /// writes nothing. Needs the signed-in administrator of the request, as the trail does.
    /// </summary>
    Task<BrandingSaveResult> SaveBrandingAsync(BrandingInput input, int formVersion, CancellationToken ct);

    /// <summary>
    /// The brand as the storefront and the e-mails show it (T362; 02 3.29.2-3.29.6, 9.1.4): the name - or the domain until one is entered -, the logo, the site icon or its fallback letter, the colour -
    /// or the product's default - with the black or white text the system chose for it, and the footer signature switch. Never stored; a plain read.
    /// </summary>
    Task<BrandIdentity> GetBrandIdentityAsync(CancellationToken ct);

    /// <summary>
    /// The social media links the storefront frame shows (T537; 02 3.27.12; 06 4.55): the platforms that have a link, in the order of the closed list (06 5.3.11), at most one each. A
    /// platform with no link is not listed. A plain read; nothing is cached.
    /// </summary>
    Task<IReadOnlyList<SocialLinkItem>> ListSocialLinksAsync(CancellationToken ct);

    /// <summary>
    /// The social links of the home page and menu form (T535; 02 3.27.12; 07 UC-162; 06 4.55), in the caller's open transaction, which the method joins and never opens: <paramref name="links"/> maps a
    /// platform name to the typed address. A filled address is checked for the form of a link and written, an emptied one deletes the platform's row, a platform the form leaves out stays. A platform
    /// outside the closed list or an address of the wrong form refuses the whole save and writes nothing. The rows are tracked, not saved: the transaction's owner saves them with the Settings module's
    /// section save, which takes the returned changes into the section's trail row and version. A save that changes nothing returns no change.
    /// </summary>
    Task<SocialLinksSaveResult> SaveSocialLinksAsync(IReadOnlyDictionary<string, string?> links, CancellationToken ct);

    /// <summary>
    /// The FAQ page's list (T506; 02 3.27.6; 04 5.9): the published questions in the firm's order, one list, no grouping. <paramref name="asAdministrator"/> is true for a browser that holds a
    /// panel session: the draft questions then stand in their place, marked (02 3.27.25). A plain read.
    /// </summary>
    Task<IReadOnlyList<FaqQuestionView>> GetFaqAsync(bool asAdministrator, CancellationToken ct);

    /// <summary>
    /// The contact page's content (T506; 02 3.27.8; 04 5.10): the company's legal identity - the footer's set - and the published branches in the firm's order; an administrator's browser adds the
    /// draft branches, marked. The form is read with <see cref="GetContactFormAsync"/>. A plain read.
    /// </summary>
    Task<ContactPageView> GetContactPageAsync(bool asAdministrator, CancellationToken ct);

    /// <summary>
    /// The contact form as the page shows it (T365; 02 3.32.1, 3.32.3, 3.32.8): open or closed by the gate of the data-collecting entries (with the refusal when closed), the five topics,
    /// and the starting values - the signed-in member's name and e-mail, which can be changed, or empty values. The form needs no sign-in. A plain read.
    /// </summary>
    Task<ContactFormView> GetContactFormAsync(string? memberName, string? memberEmail, CancellationToken ct);

    /// <summary>
    /// The send of the contact form (T365; 02 3.32; 07 UC-15), in the caller's open transaction, which the method joins and never opens: the gate, then the invisible trap field
    /// (a filled one drops the send silently and counts nothing), then the fields (all refusals together, uncounted), then the limit L-4 on <paramref name="ipAddress"/> in a
    /// transaction of its own, then the request written open with the firm's notice F-2. The sender gets no number and no acknowledgement e-mail (02 3.32.6).
    /// </summary>
    Task<ContactFormResult> SendContactRequestAsync(ContactFormInput input, System.Net.IPAddress? ipAddress, CancellationToken ct);

    /// <summary>
    /// The sales summary's contact request count (T383; 02 10.6.3): the requests opened in <c>[periodStart, periodEnd)</c> whose topic is General question, About a product or Other - the KVKK
    /// request and About an order are not the storefront's output and are left out. A plain read that counts; nothing is kept.
    /// </summary>
    Task<int> CountSummaryContactRequestsAsync(DateTimeOffset periodStart, DateTimeOffset periodEnd, CancellationToken ct);

    /// <summary>The panel home's contact request count (T382; 02 10.6.1): the requests still open, whatever their topic. A plain read that counts; nothing is kept.</summary>
    Task<int> CountOpenContactRequestsAsync(CancellationToken ct);

    /// <summary>
    /// Writes the contact request export's content to <paramref name="destination"/> (T384; 02 10.7.4; 09 10.10): a CSV with the header <c>Ad;E-posta</c> and one line for every request still
    /// kept, with the name and e-mail the sender typed in the form; no date range. Returns the number of lines (0: no request is kept - the caller makes no file). The stream stays open; the
    /// writing stops with an exception when the file passes <paramref name="maxBytes"/>. A plain read.
    /// </summary>
    Task<int> WriteContactRequestExportAsync(Stream destination, long maxBytes, CancellationToken ct);
}
