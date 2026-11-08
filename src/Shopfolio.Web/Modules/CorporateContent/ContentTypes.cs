using Shopfolio.Infrastructure.Routing;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>
/// The closed list of the corporate content types (T358; 02 3.27.2, 3.27.3, 3.27.8, 3.27.19, 3.27.20): the five ready-made types, the general page and the announcement, and
/// nothing else. There is no gallery type (a photo stays in the content it belongs to), no contact page type (the contact page is built from the company identity, the branches and
/// the form), no testimonial, no blog and no page builder. The list is <see cref="ContentRecordKind"/>; this class says which kind is which, and a test keeps the two equal in both
/// directions so a kind added to the enum without a place here fails.
/// </summary>
internal static class ContentTypes
{
    /// <summary>The five ready-made types the firm fills in (02 3.27.2).</summary>
    public static IReadOnlyList<ContentRecordKind> ReadyMade { get; } =
        [ContentRecordKind.AboutPage, ContentRecordKind.Offering, ContentRecordKind.PortfolioItem, ContentRecordKind.FaqItem, ContentRecordKind.Branch];

    /// <summary>The two that are added to the five: the general page (02 3.27.9) and the announcement (02 3.27.21).</summary>
    public static IReadOnlyList<ContentRecordKind> Added { get; } = [ContentRecordKind.CustomPage, ContentRecordKind.Announcement];

    /// <summary>The kinds that are one record: the about page (02 3.27.3) and the announcement (02 3.27.21).</summary>
    public static IReadOnlyList<ContentRecordKind> SingleRecord { get; } = [ContentRecordKind.AboutPage, ContentRecordKind.Announcement];

    /// <summary>The kind of a name on the closed list: the exact name only - no number, no other spelling (a "Gallery" or "Contact" is not a type).</summary>
    public static bool TryParse(string? name, out ContentRecordKind kind)
    {
        foreach (ContentRecordKind candidate in Enum.GetValues<ContentRecordKind>())
        {
            if (string.Equals(name, candidate.ToString(), StringComparison.Ordinal))
            {
                kind = candidate;
                return true;
            }
        }

        kind = default;
        return false;
    }
}

/// <summary>
/// The fixed words and places of the corporate content's pages that are rules, not styling (T358; 02 3.27.4, 3.27.7, 3.27.8): where the service introduction's "Bize ulaşın" button
/// leads, what the branch's map link says, and the pieces the contact page is built from.
/// </summary>
internal static class ContentPresentation
{
    /// <summary>The button of a service introduction page (02 3.27.4): its text.</summary>
    public const string OfferingContactButtonText = "Bize ulaşın";

    /// <summary>The place the button leads: the contact page, whose form it is (02 3.27.4; 02 3.32).</summary>
    public static readonly string OfferingContactButtonPath = "/" + AddressRules.ContactPage;

    /// <summary>The text of a branch's map link (02 3.27.7): the link opens the map in the visitor's own map application; nothing is embedded in the page.</summary>
    public const string BranchMapLinkText = "Haritada aç";

    /// <summary>What the contact page is built from (02 3.27.8): the company identity's contact details, the published branches and the contact form. It has no record of its own.</summary>
    public static readonly string[] ContactPageParts = ["CompanyIdentity", "Branches", "ContactForm"];
}
