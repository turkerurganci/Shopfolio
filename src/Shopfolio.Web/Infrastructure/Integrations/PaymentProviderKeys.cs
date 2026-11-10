namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The payment provider's keys (08 2.1.2; 02 3.21.4; 05 8.4.3, 8.4.6): three values that are one setting - the merchant id, the merchant key and the merchant salt. They
/// are the whole content of the secret file <c>payment_provider_keys</c>, one <c>name=value</c> line each, under the provider's own field names (<see cref="IdName"/>,
/// <see cref="KeyName"/>, <see cref="SaltName"/>). The setting is defined only when the three lines are there, each once, each with a value, and no other line is: with one
/// missing, the card line is closed (<see cref="InstallationSettings.ConditionalLines"/>) and the link layer makes no call. The key and the salt are only for the signature
/// and never go with a request; no value of this record is written to a log (<see cref="ToString"/> names no value).
/// </summary>
internal sealed record PaymentProviderKeys(string MerchantId, string MerchantKey, string MerchantSalt)
{
    public const string IdName = "merchant_id";

    public const string KeyName = "merchant_key";

    public const string SaltName = "merchant_salt";

    /// <summary>The keys of a secret file's content, or null when the setting is not defined (a line missing, empty, repeated or unknown).</summary>
    public static PaymentProviderKeys? TryParse(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        Dictionary<string, string> values = new(StringComparer.Ordinal);
        foreach (string rawLine in content.ReplaceLineEndings("\n").Split('\n'))
        {
            string line = rawLine.Trim();
            if (line.Length == 0)
            {
                continue;
            }

            int equals = line.IndexOf('=', StringComparison.Ordinal);
            if (equals <= 0)
            {
                return null;
            }

            string name = line[..equals].Trim();
            string value = line[(equals + 1)..].Trim();
            if (name is not (IdName or KeyName or SaltName) || value.Length == 0 || !values.TryAdd(name, value))
            {
                return null;
            }
        }

        return values.Count == 3 ? new PaymentProviderKeys(values[IdName], values[KeyName], values[SaltName]) : null;
    }

    /// <summary>The record's text names no value: the key and the salt are enough to forge a notification and to ask for a refund (08 2.1.2).</summary>
    public override string ToString() => nameof(PaymentProviderKeys);
}
