namespace Shopfolio.Infrastructure.EmailComposition;

/// <summary>
/// One block of an e-mail's content (09 11.4 item 1): the typed content model both writers - the HTML writer (<c>EmailFrame</c>) and the plain text writer
/// (<see cref="EmailTextWriter"/>) - read, so "the plain text is the equivalent of the same content" (08 2.2.5 item 1) holds by construction. A template
/// (B-1...B-16, F-1...F-6, the account e-mails) is a function from the event's values to a list of these blocks. The types are public only because
/// the Razor component that renders them is (a component is always public); they live in Infrastructure, not in a module (09 6 item 1).
/// </summary>
public abstract record EmailBlock;

/// <summary>The event's headline inside the body (not the subject; the subject is made from <see cref="EmailContent.Title"/>).</summary>
public sealed record EmailHeading(string Text) : EmailBlock;

/// <summary>A paragraph. The text is a value, never markup: the HTML writer encodes it, the plain text writer turns its line breaks into blanks (08 2.2.4).</summary>
public sealed record EmailParagraph(string Text) : EmailBlock;

/// <summary>A label and its value on one row (an order number, an amount, a date); the value is already in the display format of 04 8.2 (see the factories).</summary>
public sealed record EmailKeyValue(string Label, string Value) : EmailBlock
{
    /// <summary>An amount in the format of 04 8.2.4: "1.200 TL", "266,67 TL".</summary>
    public static EmailKeyValue Amount(string label, decimal amount) => new(label, DisplayFormat.Amount(amount));

    /// <summary>The Turkey day of an instant in the format of 04 8.2.1: "4 Ekim 2026".</summary>
    public static EmailKeyValue Date(string label, DateTimeOffset instant) => new(label, DisplayFormat.Date(instant));

    /// <summary>A whole number in the format of 04 8.2.3: "1.250".</summary>
    public static EmailKeyValue Number(string label, long value) => new(label, DisplayFormat.Number(value));
}

/// <summary>
/// The main action (08 2.2.5 item 4): a button in the HTML, and under it the same address as plain text; in the plain text part the label and the address.
/// The address is a full <c>https</c> address (built by <see cref="EmailLinks"/>, whose root is the installation's domain setting, never the request's
/// <c>Host</c>). <see cref="IsExternal"/> marks another site's address (a carrier's tracking page, 08 2.4), which carries <c>rel="noreferrer"</c>.
/// </summary>
public sealed record EmailButton : EmailBlock
{
    public EmailButton(string label, Uri link, bool isExternal = false)
    {
        ArgumentNullException.ThrowIfNull(label);
        ArgumentNullException.ThrowIfNull(link);
        if (!link.IsAbsoluteUri || !string.Equals(link.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal))
        {
            throw new ArgumentException("An e-mail address is a full https address (08 2.2.5 item 4).", nameof(link));
        }

        Label = label;
        Link = link;
        IsExternal = isExternal;
    }

    public string Label { get; }

    public Uri Link { get; }

    public bool IsExternal { get; }
}

/// <summary>
/// A legal text in full (the two texts of B-1, 08 2.2.6): raw plain text, as it is frozen in the order (06 4.17; 09 11.4 item 3) - the HTML writer encodes it,
/// breaks paragraphs at blank lines and lines at single line breaks, and sets it at 12 pt at least; the plain text part carries it as it is.
/// </summary>
public sealed record EmailLegalText(string Title, string Text) : EmailBlock;

/// <summary>
/// The note of a re-sent e-mail (08 2.2.5 item 6): <see cref="OriginalAt"/> is the creation instant of the first e-mail of the group (the dispatch record's
/// <c>OriginalDispatchId</c> row, 06 4.58). A new event of the same type (the second transfer reminder, the second item cancellation) is not a re-send:
/// the template passes no note for it.
/// </summary>
public sealed record EmailResend(DateTimeOffset OriginalAt)
{
    /// <summary>The one sentence on top of the body: "Bu e-posta 4 Ekim 2026 tarihli bildirimin yeniden gönderilmesidir."</summary>
    public string Sentence => $"Bu e-posta {DisplayFormat.Date(OriginalAt)} tarihli bildirimin yeniden gönderilmesidir.";
}

/// <summary>
/// What a template hands the common pattern (08 2.2.5): the headline of the event (the subject is "{brand}: {Title}"; a template's fixed text, never customer
/// text), the order number of an order-bound e-mail (appended to the subject), the blocks and, on a re-send, the note.
/// <see cref="LogoWidth"/> is the displayed width of the logo, which the template gives (08 2.2.5 item 2).
/// </summary>
public sealed record EmailContent(string Title, string? OrderNumber, IReadOnlyList<EmailBlock> Blocks, EmailResend? Resend = null, int LogoWidth = EmailContent.DefaultLogoWidth)
{
    public const int DefaultLogoWidth = 160;
}

/// <summary>The brand data of the firm (06 4.54): the name (empty until the firm enters it), the path of the logo's source copy and the brand colour (<c>#RRGGBB</c>).</summary>
public sealed record EmailBranding(string? BrandName, string? LogoPath, string? BrandColor);

/// <summary>The firm's identity for the footer (08 2.2.5 item 8): <c>Company.LegalName</c> and <c>Company.Email</c>.</summary>
public sealed record EmailFirm(string LegalName, string ContactEmail);
