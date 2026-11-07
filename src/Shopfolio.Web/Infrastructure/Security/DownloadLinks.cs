using System.Globalization;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The download link of a digital order item (09 15.5 item 4; 05 7.7.4; 07 4.31.1): a value that is not kept in a record. The payload is the item's id
/// and the end of its life (<c>orderItemId.ToString("N") + "." + expiresAtUnixSeconds</c>), protected for the purpose
/// <see cref="DataProtectionPurposes.DownloadLink"/>. The life is one hour (<see cref="Lifetime"/>) and is inside the value; it is checked against the
/// one time source at the moment a request is accepted and never while a transfer runs. The button of the order page makes the value (T511); the
/// download endpoint reads it (T312).
/// </summary>
internal sealed class DownloadLinks
{
    /// <summary>The life of a link: one hour (05 7.7.4).</summary>
    public static readonly TimeSpan Lifetime = TimeSpan.FromHours(1);

    private const char Separator = '.';

    private const int ItemIdLength = 32;

    private readonly ProtectedValues _values;

    public DownloadLinks(ProtectedValues values)
    {
        ArgumentNullException.ThrowIfNull(values);
        _values = values;
    }

    /// <summary>The protected value of a link to the item, alive from now for <see cref="Lifetime"/>.</summary>
    public string Create(Guid orderItemId, TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        long expiresAt = (clock.GetUtcNow() + Lifetime).ToUnixTimeSeconds();
        string payload = orderItemId.ToString("N") + Separator + expiresAt.ToString(CultureInfo.InvariantCulture);
        return _values.Protect(DataProtectionPurposes.DownloadLink, payload);
    }

    /// <summary>
    /// Reads a link. <see cref="DownloadLinkResult.Valid"/> with the item's id, or <see cref="DownloadLinkResult.Invalid"/> for an empty, damaged,
    /// foreign or expired value or one whose payload has another shape; the caller does not say why.
    /// </summary>
    public DownloadLinkResult Resolve(string? value, TimeProvider clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        ProtectedValueResult read = _values.Unprotect(DataProtectionPurposes.DownloadLink, value);
        if (!read.IsValid)
        {
            return DownloadLinkResult.Invalid;
        }

        string payload = read.Payload;
        if (payload.Length <= ItemIdLength + 1 || payload[ItemIdLength] != Separator
            || !Guid.TryParseExact(payload.AsSpan(0, ItemIdLength), "N", out Guid itemId)
            || !long.TryParse(payload.AsSpan(ItemIdLength + 1), NumberStyles.None, CultureInfo.InvariantCulture, out long expiresAt))
        {
            return DownloadLinkResult.Invalid;
        }

        // Alive up to and including the last second of its life; a value one second past its end is refused.
        return clock.GetUtcNow().ToUnixTimeSeconds() > expiresAt ? DownloadLinkResult.Invalid : DownloadLinkResult.Valid(itemId);
    }
}

/// <summary>The outcome of reading a download link (09 15.5 item 4): the item's id, or nothing.</summary>
internal readonly record struct DownloadLinkResult
{
    private readonly Guid _orderItemId;

    private DownloadLinkResult(Guid orderItemId)
    {
        _orderItemId = orderItemId;
        IsValid = true;
    }

    public bool IsValid { get; }

    /// <summary>The item's id; reading it from an invalid result is a programming error.</summary>
    public Guid OrderItemId => IsValid ? _orderItemId : throw new InvalidOperationException("An invalid download link has no item.");

    public static DownloadLinkResult Valid(Guid orderItemId) => new(orderItemId);

    public static DownloadLinkResult Invalid => default;
}
