namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The outcome of reading a protected value (09 15.5 item 3): <see cref="Valid"/> with the text payload, or <see cref="Invalid"/>. The caller treats
/// <see cref="Invalid"/> as the value never having arrived; it shows the user no separate error and does not say why the value was refused.
/// </summary>
internal readonly record struct ProtectedValueResult
{
    private readonly string? _payload;

    private ProtectedValueResult(string payload)
    {
        _payload = payload;
        IsValid = true;
    }

    public bool IsValid { get; }

    /// <summary>The text that was protected. Reading it from an invalid result is a programming error.</summary>
    public string Payload => IsValid ? _payload! : throw new InvalidOperationException("An invalid protected value has no payload.");

    public static ProtectedValueResult Valid(string payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        return new ProtectedValueResult(payload);
    }

    public static ProtectedValueResult Invalid => default;
}
