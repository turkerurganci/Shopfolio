namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>
/// The file an image address serves (05 7.7.1, 7.7.2; D-1136, D-1711): the key of the image section's file and the media type it is sent with. The source copy - the
/// address names the image, not a width - and the type is P-22's of the stored format.
/// </summary>
public sealed record ServedImage(string StorageKey, string ContentType);
