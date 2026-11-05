using System.Security.Cryptography;
using System.Text;

using Shopfolio.Infrastructure.Integrations;

namespace Shopfolio.UnitTests.Integrations;

/// <summary>
/// The fixed-time comparison of a signature (T203 criterion 2, KK2; 08 3.2 principle 1; 09 11.1 item 5). The signatures are made here with a
/// placeholder key; no real secret is written.
/// </summary>
public sealed class SignatureComparisonTests
{
    private static readonly byte[] PlaceholderKey = Encoding.UTF8.GetBytes("test-placeholder-signing-material");

    private static string Sign(string text) => Convert.ToBase64String(HMACSHA256.HashData(PlaceholderKey, Encoding.UTF8.GetBytes(text)));

    [Fact]
    public void Matches_TheSameSignature_IsTrue()
    {
        string computed = Sign("order-1success3456");

        Assert.True(SignatureComparison.Matches(computed, Sign("order-1success3456")));
    }

    [Fact]
    public void Matches_ASignatureOfOtherFields_IsFalse()
    {
        Assert.False(SignatureComparison.Matches(Sign("order-1success3456"), Sign("order-1success3457")));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(-1)]
    public void Matches_ASignatureWithOneCharacterChanged_IsFalse_WhereverTheCharacterIs(int position)
    {
        string computed = Sign("order-1success3456");
        int index = position < 0 ? computed.Length + position : position;
        char[] changed = computed.ToCharArray();
        changed[index] = changed[index] == 'A' ? 'B' : 'A';

        Assert.False(SignatureComparison.Matches(computed, new string(changed)));
    }

    [Fact]
    public void Matches_ASignatureOfAnotherLength_IsFalse()
    {
        string computed = Sign("order-1success3456");

        Assert.False(SignatureComparison.Matches(computed, computed[..^1]));
        Assert.False(SignatureComparison.Matches(computed, computed + "A"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Matches_AMissingSignature_IsFalse(string? received)
    {
        Assert.False(SignatureComparison.Matches(Sign("order-1success3456"), received));
    }

    [Fact]
    public void Matches_WithoutAComputedSignature_IsAProgrammingError()
    {
        Assert.Throws<ArgumentException>(() => SignatureComparison.Matches(string.Empty, "x"));
    }
}
