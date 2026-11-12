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

    /// <summary>
    /// The number of contact requests the panel's requests list takes (T532; 07 UC-93): the requests in one of <paramref name="statuses"/> - none named means any status - that are still kept; a request
    /// past its retention is not counted although the night's disposal has not reached it. A plain read that counts.
    /// </summary>
    Task<int> CountPanelContactRequestsAsync(IReadOnlyCollection<ContactRequestStatus> statuses, CancellationToken ct);

    /// <summary>
    /// The first <paramref name="take"/> contact requests of the panel's requests list (T532; 07 UC-93), newest first - the moment the request reached the firm, equal moments by the id as the database
    /// sorts it - in one of <paramref name="statuses"/> (none named: any status), past-retention requests left out. The caller merges them with the defect claims. A plain read.
    /// </summary>
    Task<IReadOnlyList<PanelContactRequestRow>> ListPanelContactRequestsAsync(IReadOnlyCollection<ContactRequestStatus> statuses, int take, CancellationToken ct);

    /// <summary>The contact request's detail (T532; 07 UC-94); null when there is no such request or it was destroyed at the end of its retention (K-1192). A plain read.</summary>
    Task<ContactRequestDetail?> FindContactRequestAsync(Guid requestId, CancellationToken ct);

    /// <summary>
    /// Closes a contact request (T532; 07 UC-155; 03 1.9.2), in the caller's open transaction, which the method joins and never opens: an open request becomes closed and the closing moment is written; a closed
    /// one stays as it is (<see cref="ContactRequestChangeOutcome.Unchanged"/>); a request that is not there is <see cref="ContactRequestChangeOutcome.NotFound"/>. No notice is made.
    /// </summary>
    Task<ContactRequestChangeResult> CloseContactRequestAsync(Guid requestId, Guid adminId, CancellationToken ct);

    /// <summary>
    /// Reopens a contact request (T532; 07 UC-156), in the caller's open transaction: a closed request becomes open; the last closing moment is kept, so the retention keeps running from it (K-1099).
    /// An open one stays as it is; a request that is not there is <see cref="ContactRequestChangeOutcome.NotFound"/>. No notice is made.
    /// </summary>
    Task<ContactRequestChangeResult> ReopenContactRequestAsync(Guid requestId, Guid adminId, CancellationToken ct);

    /// <summary>The panel home's contact request count (T382; 02 10.6.1): the requests still open, whatever their topic. A plain read that counts; nothing is kept.</summary>
    Task<int> CountOpenContactRequestsAsync(CancellationToken ct);

    /// <summary>
    /// Writes the contact request export's content to <paramref name="destination"/> (T384; 02 10.7.4; 09 10.10): a CSV with the header <c>Ad;E-posta</c> and one line for every request still
    /// kept, with the name and e-mail the sender typed in the form; no date range. Returns the number of lines (0: no request is kept - the caller makes no file). The stream stays open; the
    /// writing stops with an exception when the file passes <paramref name="maxBytes"/>. A plain read.
    /// </summary>
    Task<int> WriteContactRequestExportAsync(Stream destination, long maxBytes, CancellationToken ct);
}
