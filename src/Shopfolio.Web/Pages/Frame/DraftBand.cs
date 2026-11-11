using Shopfolio.Infrastructure.Security;

namespace Shopfolio.Pages.Frame;

/// <summary>
/// The "Taslak" band of a draft preview (T540; 04 2.9.2; 03 8.1.7): it takes the place of the administrator strip, says the record is a draft and carries "Düzenlemeye dön" to the record's form in the
/// panel. <paramref name="EditPath"/> is a local panel address (<c>/panel/…</c>), never a value from the request.
/// </summary>
public sealed record DraftBand(string EditPath);

/// <summary>
/// How a page asks for the draft band and how the layout reads it (T540; 04 2.9.2, 2.9.4). A page that opens a draft record for an administrator - the draft product's page, the draft content
/// record's page - calls <see cref="SetDraftPreview"/>; the layout calls <see cref="GetDraftBand"/>. The band shows only while the frame carries the administrator strip, so a customer's
/// browser never gets it even if a page asked (the page answers "sayfa bulunamadı" to a visitor anyway, 04 2.9.2); when the panel session ends the strip and the band go together.
/// </summary>
public static class DraftBandAccess
{
    internal const string ItemKey = "Shopfolio.DraftBand";

    /// <summary>Asks the layout for the band with the link back to the record's panel form; <paramref name="editPath"/> must be a local address under <c>/panel/</c>.</summary>
    public static void SetDraftPreview(this HttpContext context, string editPath)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrEmpty(editPath);
        if (!ReturnPath.IsLocalPath(editPath) || !editPath.StartsWith("/panel/", StringComparison.Ordinal))
        {
            throw new ArgumentException("The draft band's link is a local address under /panel/ (04 2.9.2).", nameof(editPath));
        }

        context.Items[ItemKey] = new DraftBand(editPath);
    }

    /// <summary>The band to draw, or null: none was asked for, or the browser holds no administrator strip (04 2.9.2: the band takes the strip's place, on the strip's condition).</summary>
    public static DraftBand? GetDraftBand(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        bool hasStrip = context.GetStorefrontFrame()?.AdminStrip is not null;
        return hasStrip && context.Items.TryGetValue(ItemKey, out object? value) ? value as DraftBand : null;
    }
}
