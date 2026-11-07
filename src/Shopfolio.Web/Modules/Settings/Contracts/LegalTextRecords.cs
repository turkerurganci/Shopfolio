using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Settings.Contracts;

/// <summary>
/// One of the four legal texts as the panel's list shows it (02 3.33.1, 3.33.9): the type, the number of its last published version, whether the firm edits it, and what is
/// missing by name. A text with an empty <see cref="Missing"/> is complete.
/// </summary>
public sealed record LegalTextSummary(LegalTextType Type, int? LastVersionNumber, bool IsEditedByFirm, IReadOnlyList<LegalTextMissingPart> Missing)
{
    /// <summary>True when nothing waits: the text is published and, for the privacy notice, its hosting location and the identity fields are filled.</summary>
    public bool IsComplete => Missing.Count == 0;
}

/// <summary>The draft row of a firm-edited text (02 3.33.1): the text the firm is editing, the hosting location section of the privacy notice, and the form's version.</summary>
public sealed record LegalTextDraft(LegalTextType Type, int FormVersion, string Text, string? HostingLocation);

/// <summary>A published version of a legal text, readable for as long as an order holds it (02 3.33.3).</summary>
public sealed record LegalTextVersionInfo(Guid Id, LegalTextType Type, int VersionNumber, DateTimeOffset PublishedAt, string Text, string? HostingLocation);

/// <summary>
/// The versions in force that an order freezes at the confirmation (02 3.33.4; 06 4.17): the last published form, contract and privacy notice. A missing one is
/// <see langword="null"/>; the sales gate keeps an order from being confirmed in that case.
/// </summary>
public sealed record LegalTextVersionIds(Guid? PreInformationForm, Guid? DistanceSalesContract, Guid? PrivacyNotice);

/// <summary>
/// The outcome of publishing a firm-edited text (02 3.33.2, 3.33.3): published with its number; refused for a value (an empty text, an empty hosting location on the
/// privacy notice); or refused because the draft's version is not the one the form carried.
/// </summary>
public sealed class LegalTextPublishResult : IOperationResult
{
    private LegalTextPublishResult(int? versionNumber, Guid? versionId, bool isVersionConflict, IReadOnlyList<FieldRejection> rejections)
    {
        VersionNumber = versionNumber;
        VersionId = versionId;
        IsVersionConflict = isVersionConflict;
        Rejections = rejections;
    }

    public static LegalTextPublishResult VersionConflict { get; } = new(null, null, true, []);

    /// <summary>The number the new version got; empty when the publication was refused.</summary>
    public int? VersionNumber { get; }

    /// <summary>The new version's id; empty when the publication was refused.</summary>
    public Guid? VersionId { get; }

    public bool IsVersionConflict { get; }

    /// <summary>The refused fields; empty unless the publication was refused for its values.</summary>
    public IReadOnlyList<FieldRejection> Rejections { get; }

    public bool IsPublished => VersionNumber is not null;

    public bool IsRejected => !IsPublished;

    public static LegalTextPublishResult Published(int versionNumber, Guid versionId) => new(versionNumber, versionId, false, []);

    public static LegalTextPublishResult Refused(IReadOnlyList<FieldRejection> rejections)
    {
        ArgumentNullException.ThrowIfNull(rejections);
        if (rejections.Count == 0)
        {
            throw new ArgumentException("A refusal for values names at least one field.", nameof(rejections));
        }

        return new LegalTextPublishResult(null, null, false, rejections);
    }
}

/// <summary>
/// Where the legal text links stand (02 3.33.5; 11 T366 criterion 5): the privacy notice link is on every screen that opens a new personal data field and on the footer of
/// every page; the cookie policy link is on the footer only; there is no cookie banner, no consent box for the notice and no per-user "seen" record - the proof is the text's
/// version and the screens' places, and the order freezes the version of the day. The panel's reminder line next to a setting is <see cref="Reminder"/> (02 3.33.8).
/// </summary>
public static class LegalTextLinks
{
    /// <summary>The screens that carry the privacy notice link, the footer included.</summary>
    public static IReadOnlyList<LegalLinkScreen> PrivacyNoticeScreens { get; } = LegalLinkScreen.All;

    /// <summary>The screens that carry the cookie policy link: the footer.</summary>
    public static IReadOnlyList<LegalLinkScreen> CookiePolicyScreens { get; } = [LegalLinkScreen.Footer];

    /// <summary>
    /// The reminder line the panel shows next to a setting a free text may repeat (the shipping fee, the return period, the shipping period): the product detects no contradiction
    /// and builds no information page from the settings; the binding text is the form and the contract generated from the current settings (02 3.33.8; K-364, K-242).
    /// </summary>
    public const string Reminder =
        "Sıkça sorulan sorular, genel sayfalar ve aydınlatma metni bu değeri yineliyorsa, değeri değiştirdiğinizde o metinleri de güncellemek sizin sorumluluğunuzdadır; "
        + "bağlayıcı olan, ayarların güncel hâlinden üretilen Ön Bilgilendirme Formu ve Mesafeli Satış Sözleşmesi'dir.";
}
