// Deliberate violation sample for the forbidden test pattern scan (09 19.2 item 3 and 6).
// This file is excluded from compilation and from the real scan; the scan test reads it as text.

namespace Shopfolio.Samples;

public sealed class ForbiddenTests
{
    [Fact(Skip = "look at it later")]
    public void Skipped_Test_IsForbidden()
    {
        Assert.Equal(1, 1);
    }

    [Fact(Explicit = true)]
    public void Explicit_Test_IsForbidden()
    {
        Assert.Equal(1, 1);
    }

    [Fact]
    public void RuntimeSkip_Test_IsForbidden()
    {
        Assert.Skip("not today");
    }

    [Fact]
    public void AlwaysTrue_Test_IsForbidden()
    {
        Assert.True(true);
    }

    [Fact]
    public void NoAssertion_Test_IsForbidden()
    {
        int unused = 1 + 1; // a brace in a comment { must not confuse the scan
        _ = unused;
    }

    [Fact]
    public void OnlyNotNull_Test_IsForbidden()
    {
        object result = new();
        Assert.NotNull(result);
    }
}
