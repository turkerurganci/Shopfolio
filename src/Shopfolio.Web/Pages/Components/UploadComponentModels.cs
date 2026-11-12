using System.Globalization;

using Microsoft.AspNetCore.Mvc.ModelBinding;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Imaging;
using Shopfolio.Infrastructure.Validation;

namespace Shopfolio.Pages.Components;

/// <summary>The places a file is uploaded (04 2.12.8; 04 OB-19): the two galleries, the three single files.</summary>
public enum UploadSurface
{
    /// <summary>A product's gallery (E-33; P-24).</summary>
    ProductGallery,

    /// <summary>A corporate content record's gallery (E-41; P-25).</summary>
    ContentGallery,

    /// <summary>The logo: one file (02 3.29.2).</summary>
    Logo,

    /// <summary>The site icon: one file (02 3.29.4).</summary>
    SiteIcon,

    /// <summary>The digital file of a product or a variant: one file (P-26; 03 8.1.6).</summary>
    DigitalFile,
}

/// <summary>The side of the site that draws a form: only the panel uploads (04 2.12.8.6).</summary>
public enum UploadSide
{
    Panel,

    /// <summary>The customer's side has no upload: the contact form and the defect request take no attachment (02 3.32.2; 03 2.9.2).</summary>
    Customer,
}

/// <summary>Where an upload stands (04 2.12.8.4): waiting to be saved, still going, or complete.</summary>
public enum UploadState
{
    Completed,
    Uploading,
}

/// <summary>The limits of a surface, read from the product constants (P-22…P-26; 04 2.12.8.3) - the component types no number.</summary>
public static class UploadLimits
{
    private const long BytesPerMegabyte = 1 << 20;

    /// <summary>True for the surfaces that take images (the two galleries, the logo and the site icon); the digital file is any file.</summary>
    public static bool IsImage(UploadSurface surface) => surface != UploadSurface.DigitalFile;

    /// <summary>True for the two galleries: several files; the others are one file (04 2.12.8.5).</summary>
    public static bool IsGallery(UploadSurface surface) => surface is UploadSurface.ProductGallery or UploadSurface.ContentGallery;

    /// <summary>The most files the surface holds: the gallery's count limit (P-24, P-25), one for the single files.</summary>
    public static int MaxCount(UploadSurface surface) => surface switch
    {
        UploadSurface.ProductGallery => ProductConstants.ImagesPerProductMax,
        UploadSurface.ContentGallery => ProductConstants.ImagesPerContentRecordMax,
        UploadSurface.Logo or UploadSurface.SiteIcon or UploadSurface.DigitalFile => 1,
        _ => throw new ArgumentOutOfRangeException(nameof(surface), surface, "Not a known upload surface."),
    };

    /// <summary>The largest single file, in bytes: P-23 for an image, P-26 for the digital file.</summary>
    public static long MaxBytes(UploadSurface surface) => IsImage(surface) ? ProductConstants.ImageMaxBytes : ProductConstants.DigitalFileMaxBytes;

    /// <summary>The size limit as the message says it, in whole megabytes ("10 MB").</summary>
    public static string MaxSizeText(UploadSurface surface) => (MaxBytes(surface) / BytesPerMegabyte).ToString(CultureInfo.InvariantCulture) + " MB";

    /// <summary>The <c>accept</c> of the file input: the image media types of P-22 for an image surface, none for the digital file. A hint to the browser; the server reads the bytes (05 6.6.1).</summary>
    public static string? Accept(UploadSurface surface) => IsImage(surface) ? "image/jpeg,image/png,image/webp" : null;
}

/// <summary>A file the component refused and the reason to say at the field (04 2.12.8.3; 04 7.3.2.18).</summary>
/// <param name="Code">The catalog code of the refusal (<c>InvalidImage</c>, <c>ImageLimitExceeded</c>).</param>
/// <param name="Reason">The text at the field: the format, the size or the count, with the limit.</param>
public sealed record UploadRefusal(string Code, string Reason);

/// <summary>The upload rules of a form (T544; 04 OB-19, 2.12.8): what is refused and the sentence that says why.</summary>
public static class UploadChecks
{
    /// <summary>What a form that is saved while an upload goes on says (04 2.12.8.4).</summary>
    public const string SaveBlockedText = "Yükleme sürerken kayıt yapılamaz.";

    /// <summary>The refusal of a file of a format the product does not take.</summary>
    public static string FormatMessage() => $"Dosya desteklenen bir görsel değil; {ProductConstants.ImageFormats} yüklenebilir.";

    /// <summary>The refusal of a file over the size limit of its surface.</summary>
    public static string SizeMessage(UploadSurface surface) => $"Dosya en çok {UploadLimits.MaxSizeText(surface)} olabilir.";

    /// <summary>The refusal of one file too many: the gallery's count, or the single file's one.</summary>
    public static string CountMessage(UploadSurface surface) =>
        UploadLimits.IsGallery(surface)
            ? $"En çok {UploadLimits.MaxCount(surface).ToString(CultureInfo.InvariantCulture)} görsel yüklenebilir."
            : "Tek dosya yüklenebilir.";

    /// <summary>
    /// The image rule (04 2.12.8.3): the format is read from the first bytes (P-22; not from the name), the size is the file's (P-23) and the count is the gallery's (P-24, P-25). The first thing that is wrong is told;
    /// null is an accepted file. <paramref name="existingCount"/> is the images the record already holds and does not count for a single file, which a new one replaces.
    /// </summary>
    public static UploadRefusal? CheckImage(UploadSurface surface, ReadOnlySpan<byte> content, long sizeBytes, int existingCount)
    {
        if (!UploadLimits.IsImage(surface))
        {
            throw new ArgumentException("The digital file is not an image.", nameof(surface));
        }

        if (ImageKinds.Sniff(content) is null)
        {
            return new UploadRefusal(FieldCodes.InvalidImage, FormatMessage());
        }

        if (sizeBytes > UploadLimits.MaxBytes(surface))
        {
            return new UploadRefusal(FieldCodes.InvalidImage, SizeMessage(surface));
        }

        if (UploadLimits.IsGallery(surface) && existingCount >= UploadLimits.MaxCount(surface))
        {
            return new UploadRefusal(FieldCodes.ImageLimitExceeded, CountMessage(surface));
        }

        return null;
    }

    /// <summary>The digital file rule (04 2.12.8.3): any format; the size limit is P-26.</summary>
    public static UploadRefusal? CheckDigitalFile(long sizeBytes) =>
        sizeBytes > UploadLimits.MaxBytes(UploadSurface.DigitalFile) ? new UploadRefusal(FieldCodes.InvalidImage, SizeMessage(UploadSurface.DigitalFile)) : null;

    /// <summary>The count rule of a request: a single-file surface takes one file in a request, a gallery takes what is left of its limit.</summary>
    public static UploadRefusal? CheckCount(UploadSurface surface, int existingCount, int incomingCount)
    {
        int room = UploadLimits.IsGallery(surface) ? UploadLimits.MaxCount(surface) - existingCount : 1;
        return incomingCount > room ? new UploadRefusal(FieldCodes.ImageLimitExceeded, CountMessage(surface)) : null;
    }

    /// <summary>Puts a refusal into the model state as the text the field shows (04 2.3.2): the reason of the refusal is the message.</summary>
    public static void AddRefusal(ModelStateDictionary modelState, string name, UploadRefusal refusal)
    {
        ArgumentNullException.ThrowIfNull(modelState);
        ArgumentNullException.ThrowIfNull(refusal);
        modelState.AddModelError(name, refusal.Reason);
    }

    /// <summary>
    /// The message of the field whose element id is <paramref name="fieldId"/>, taken from the summary of the sent form (04 2.3.2, 2.12.8.3): a refused file and a file that is gone are told at the field.
    /// Null when the summary does not name the field. The message id is the one the field's <c>aria-describedby</c> names.
    /// </summary>
    internal static MessageView? ErrorOf(FormErrorSummary? summary, string fieldId)
    {
        FormFieldError? error = summary?.Fields.FirstOrDefault(f => string.Equals(f.FieldId, fieldId, StringComparison.OrdinalIgnoreCase));
        return error is null ? null : Messages.ForField(fieldId, error.Text);
    }

    /// <summary>The upload a form names is gone - the form was open for more than a day and the nightly disposal deleted it (04 2.12.8.7; K-1100): the field asks for it again and the other entries stay.</summary>
    public static void AddMissingUpload(ModelStateDictionary modelState, string name)
    {
        ArgumentNullException.ThrowIfNull(modelState);
        modelState.AddFieldError(name, FieldCodes.UploadMissing);
    }

    /// <summary>
    /// True when the form may be saved (04 2.12.8.4): no upload of it is still going. Every state that is not complete blocks the save, so a state added later blocks until its author says otherwise.
    /// </summary>
    public static bool CanSave(IEnumerable<UploadState> states)
    {
        ArgumentNullException.ThrowIfNull(states);
        return states.All(s => s == UploadState.Completed);
    }

    /// <summary>The upload surfaces of a side: the panel has all five, the customer none (04 2.12.8.6).</summary>
    public static IReadOnlyList<UploadSurface> SurfacesOf(UploadSide side) => side == UploadSide.Panel ? Enum.GetValues<UploadSurface>() : [];

    /// <summary>The check every view takes at its entrance: the customer's side draws no upload (04 2.12.8.6).</summary>
    public static void RequirePanel(UploadSide side)
    {
        if (side != UploadSide.Panel)
        {
            throw new InvalidOperationException("The customer's side has no file upload.");
        }
    }
}

/// <summary>An upload that goes on: the file's name and how much of it has arrived (04 2.12.8.4).</summary>
public sealed record UploadProgressView(string FileName, long ReceivedBytes, long TotalBytes)
{
    /// <summary>The whole percent that has arrived, 0 to 100; an empty file is complete.</summary>
    public int Percent => TotalBytes <= 0 ? 100 : (int)Math.Clamp(ReceivedBytes * 100 / TotalBytes, 0, 100);

    public UploadState State => ReceivedBytes >= TotalBytes ? UploadState.Completed : UploadState.Uploading;

    /// <summary>The text beside the bar: the file and the percent.</summary>
    public string Text => $"{FileName} — {DisplayFormat.Percent(Percent)}";
}

/// <summary>The uploads of a form and whether its save button can work (04 2.12.8.4; 2.7.2): while one goes on the progress shows and the save button is closed with the words that say why.</summary>
public sealed class UploadStatusView
{
    internal UploadStatusView(IReadOnlyList<UploadProgressView> uploads)
    {
        Uploads = uploads;
    }

    public IReadOnlyList<UploadProgressView> Uploads { get; }

    /// <summary>True while an upload goes on: the save button is closed.</summary>
    public bool SaveBlocked => !UploadChecks.CanSave(Uploads.Select(u => u.State));

    /// <summary>The words under the closed save button.</summary>
    public string SaveBlockedText => UploadChecks.SaveBlockedText;

    /// <summary>The uploads that still go on, the ones the progress is drawn for.</summary>
    public IEnumerable<UploadProgressView> Running => Uploads.Where(u => u.State == UploadState.Uploading);

    public static UploadStatusView Of(IEnumerable<UploadProgressView> uploads)
    {
        ArgumentNullException.ThrowIfNull(uploads);
        return new UploadStatusView([.. uploads]);
    }
}

/// <summary>One image of a gallery as the form shows it: its place, its alternative text as the firm wrote it and the main mark (04 2.12.8.1, 2.12.8.2).</summary>
/// <param name="ImageId">The image's id.</param>
/// <param name="Url">The address the thumbnail is shown from.</param>
/// <param name="AltText">The firm's alternative text; null or white space is none (the system makes one).</param>
/// <param name="IsMain">True for the main image; the mark is independent of the order.</param>
/// <param name="Order">The place the firm gave it; the gallery is shown in this order.</param>
public sealed record GalleryImage(Guid ImageId, string Url, string? AltText, bool IsMain, int Order);

/// <summary>One row of the gallery form: the image, the text the system makes when the firm left the alternative text empty, and the names of the row's members.</summary>
public sealed record GalleryRowView(GalleryImage Image, int Position, int Total, string GeneratedAltText, string IdName, string OrderName, string AltName)
{
    /// <summary>The element id of the order field.</summary>
    public string OrderId => FormErrorSummary.FieldIdOf(OrderName);

    /// <summary>The element id of the alternative text field.</summary>
    public string AltId => FormErrorSummary.FieldIdOf(AltName);

    /// <summary>The element id of the line under the alternative text field.</summary>
    public string AltHintId => AltId + "-hint";

    /// <summary>True when the firm's alternative text is empty: the field says the system's text stands in (04 2.12.8.2).</summary>
    public bool UsesGeneratedText => string.IsNullOrWhiteSpace(Image.AltText);

    /// <summary>The line under the empty alternative text field.</summary>
    public string GeneratedTextHint => $"Boş bırakırsanız şu metin kullanılır: {GeneratedAltText}";
}

/// <summary>The gallery upload (T544; 04 OB-19, 2.12.8.1): the rows in their order, the main mark, the room that is left, and the file input.</summary>
public sealed class GalleryUploadView
{
    internal GalleryUploadView(UploadSurface surface, string namePrefix, IReadOnlyList<GalleryRowView> rows, Guid? mainImageId, FormErrorSummary? summary = null)
    {
        Surface = surface;
        NamePrefix = namePrefix;
        Rows = rows;
        MainImageId = mainImageId;
        Error = UploadChecks.ErrorOf(summary, FilesId);
        Autofocus = Error is not null && string.Equals(summary?.FirstFieldId, FilesId, StringComparison.OrdinalIgnoreCase);
    }

    public UploadSurface Surface { get; }

    public string NamePrefix { get; }

    public IReadOnlyList<GalleryRowView> Rows { get; }

    /// <summary>The image that is main: the one the form marks, not the first in the order unless nothing else is marked.</summary>
    public Guid? MainImageId { get; }

    /// <summary>The member the main mark is sent as; one choice for the whole gallery, so two images are never both main.</summary>
    public string MainName => NamePrefix + "MainImageId";

    /// <summary>The member the file input is sent as.</summary>
    public string FilesName => NamePrefix + "Files";

    /// <summary>The element id of the file input: the framework's id of its name, the one the summary's link points to (04 2.3.2).</summary>
    public string FilesId => FormErrorSummary.FieldIdOf(FilesName);

    /// <summary>The message of the file input - the reason a file was refused, or that an upload is gone - drawn under it; null when the field is right (04 2.12.8.3, 2.12.8.7).</summary>
    public MessageView? Error { get; }

    /// <summary>True when the file input is the first wrong field of the sent form: its tag carries <c>autofocus</c> (04 2.3.2).</summary>
    public bool Autofocus { get; }

    /// <summary>True when the file input has a message.</summary>
    public bool IsInvalid => Error is not null;

    public int MaxCount => UploadLimits.MaxCount(Surface);

    /// <summary>True while the gallery has room for another image.</summary>
    public bool HasRoom => Rows.Count < MaxCount;

    /// <summary>How many images the gallery holds of how many it may hold: "3 / 20".</summary>
    public string CountText => $"{Rows.Count.ToString(CultureInfo.InvariantCulture)} / {MaxCount.ToString(CultureInfo.InvariantCulture)}";

    public string? Accept => UploadLimits.Accept(Surface);
}

/// <summary>The only constructor of a <see cref="GalleryUploadView"/> and the gallery's order and main rules (T544; 04 2.12.8.1, 2.12.8.2).</summary>
public static class GalleryUploads
{
    /// <summary>
    /// The gallery of a record (04 2.12.8.1): the images in the order the firm gave, the generated alternative text of each (the record's name and the image's place, the one rule of <c>GalleryText</c>) and the
    /// main image - the one marked, else the current main image when it is still in the list, else the first.
    /// </summary>
    public static GalleryUploadView Create(UploadSide side, UploadSurface surface, string namePrefix, string recordName, IReadOnlyList<GalleryImage> images, Guid? currentMainImageId = null, FormErrorSummary? summary = null)
    {
        UploadChecks.RequirePanel(side);
        if (!UploadLimits.IsGallery(surface))
        {
            throw new ArgumentException("A gallery is a gallery surface.", nameof(surface));
        }

        ArgumentNullException.ThrowIfNull(namePrefix);
        ArgumentException.ThrowIfNullOrWhiteSpace(recordName);
        ArgumentNullException.ThrowIfNull(images);
        IReadOnlyList<GalleryImage> ordered = InOrder(images);
        List<GalleryRowView> rows = [];
        for (int i = 0; i < ordered.Count; i++)
        {
            string member = $"{namePrefix}Images[{i.ToString(CultureInfo.InvariantCulture)}].";
            rows.Add(new GalleryRowView(ordered[i], i + 1, ordered.Count, GalleryText.Generated(recordName, i + 1, ordered.Count), member + "ImageId", member + "Order", member + "AltText"));
        }

        Guid? main = MainOf(ordered, currentMainImageId);
        return new GalleryUploadView(surface, namePrefix, rows, main, summary);
    }

    /// <summary>The images in the order the firm gave them; two images with the same number keep the order they came in, so the result is the same on every sort (04 2.12.8.1: the order is set by hand).</summary>
    public static IReadOnlyList<GalleryImage> InOrder(IReadOnlyList<GalleryImage> images)
    {
        ArgumentNullException.ThrowIfNull(images);
        return [.. images.Select((image, index) => (image, index)).OrderBy(x => x.image.Order).ThenBy(x => x.index).Select(x => x.image)];
    }

    /// <summary>The gallery after the firm's order is applied: the images are renumbered 1…n in the order the numbers give, with no gaps and no duplicates.</summary>
    public static IReadOnlyList<GalleryImage> Renumber(IReadOnlyList<GalleryImage> images) =>
        [.. InOrder(images).Select((image, index) => image with { Order = index + 1 })];

    /// <summary>The main image (04 2.12.8.1): the mark is independent of the order - the marked image stays main wherever it stands -, and a gallery that has an image has exactly one main image.</summary>
    public static Guid? MainOf(IReadOnlyList<GalleryImage> images, Guid? currentMainImageId)
    {
        ArgumentNullException.ThrowIfNull(images);
        return GalleryText.MainOf([.. images.Select(i => (i.ImageId, i.IsMain))], currentMainImageId);
    }
}

/// <summary>The single file upload (T544; 04 OB-19, 2.12.8.5): the logo, the site icon or the digital file - one file; a new one replaces the current one.</summary>
/// <param name="Surface">Which single file.</param>
/// <param name="Name">The member the file input is sent as.</param>
/// <param name="Label">The visible label.</param>
/// <param name="CurrentFileName">The name of the file the record holds now; null when none (the fallback of 02 3.29.2, 3.29.4 stands).</param>
/// <param name="Hint">A line under the label, when the screen has one.</param>
public sealed record SingleUploadView(UploadSurface Surface, string Name, string Label, string? CurrentFileName, string? Hint)
{
    public string Id => FormErrorSummary.FieldIdOf(Name);

    /// <summary>The message of the field - the reason the file was refused, or that the upload is gone - drawn under it; null when the field is right (04 2.12.8.3, 2.12.8.7).</summary>
    public MessageView? Error { get; init; }

    /// <summary>True when the field is the first wrong field of the sent form: its tag carries <c>autofocus</c> (04 2.3.2).</summary>
    public bool Autofocus { get; init; }

    /// <summary>True when the field has a message.</summary>
    public bool IsInvalid => Error is not null;

    /// <summary>The ids <c>aria-describedby</c> lists: the message first when there is one (it is the tie of the message to the field), then the line of limits.</summary>
    public string DescribedBy => (Error is null ? string.Empty : Error.Id + " ") + Id + "-limit";

    public string? Accept => UploadLimits.Accept(Surface);

    /// <summary>The line that says what the field takes: the format and the size for an image, the size for the digital file.</summary>
    public string LimitText => UploadLimits.IsImage(Surface)
        ? $"{ProductConstants.ImageFormats}; en çok {UploadLimits.MaxSizeText(Surface)}."
        : $"En çok {UploadLimits.MaxSizeText(Surface)}.";
}

/// <summary>The only constructor of a <see cref="SingleUploadView"/>.</summary>
public static class SingleUploads
{
    public static SingleUploadView Create(UploadSide side, UploadSurface surface, string name, string label, string? currentFileName = null, string? hint = null, FormErrorSummary? summary = null)
    {
        UploadChecks.RequirePanel(side);
        if (UploadLimits.IsGallery(surface))
        {
            throw new ArgumentException("A single file is not a gallery.", nameof(surface));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        SingleUploadView view = new(surface, name, label.Trim(), string.IsNullOrWhiteSpace(currentFileName) ? null : currentFileName.Trim(), hint);
        MessageView? error = UploadChecks.ErrorOf(summary, view.Id);
        return error is null ? view : view with { Error = error, Autofocus = string.Equals(summary?.FirstFieldId, view.Id, StringComparison.OrdinalIgnoreCase) };
    }
}
