// Deliberate violation sample of the signature comparison scan (T203 KK2; 09 19.2 item 3). Not compiled; read as text by
// SignatureComparisonSourceScanTests. Every comparison below except the last two is a finding; the comment lines are not.
// A comment that says computedHash == receivedHash is not code.
namespace Shopfolio.Infrastructure.Integrations;

internal static class EarlyExitComparisons
{
    public static bool Operator(string computedHash, string receivedHash) => computedHash == receivedHash;

    public static bool NotEqual(string signature, string received) => signature != received;

    public static bool InstanceEquals(string hmac, string received) => hmac.Equals(received);

    public static bool StaticEquals(string computed, string receivedSignature) => string.Equals(computed, receivedSignature, StringComparison.Ordinal);

    public static bool Bytes(byte[] digest, byte[] received) => digest.SequenceEqual(received);

    public static bool Ordinal(string hash, string received) => string.CompareOrdinal(hash, received) == 0;

    public static bool Prefix(string Hash, string received) => received.StartsWith(Hash, StringComparison.Ordinal);

    public static bool Pattern(string signature) => signature is "known";

    public static bool OverTwoLines(string computedHash, string receivedHash)
    {
        return computedHash
            == receivedHash;
    }

    public static bool Inside(string received, string signature) => received.Contains(signature, StringComparison.Ordinal);

    public static bool Position(string received, string digest) => received.IndexOf(digest, StringComparison.Ordinal) == 0;

    public static bool FixedTime(string computedHash, string receivedHash) => SignatureComparison.Matches(computedHash, receivedHash);

    public static bool Missing(string? receivedHash) => receivedHash is null;
}
