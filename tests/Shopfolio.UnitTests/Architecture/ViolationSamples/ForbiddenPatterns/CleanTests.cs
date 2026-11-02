// Clean counterpart of ForbiddenTests.cs: the scan must report nothing here.
// This file is excluded from compilation and from the real scan; the scan test reads it as text.

namespace Shopfolio.Samples;

public sealed class CleanTests
{
    [Fact]
    public void Add_TwoNumbers_ReturnsSum()
    {
        string text = "{ a brace in a string must not confuse the scan";
        Assert.Equal(2, 1 + 1);
        Assert.NotNull(text);
    }

    [Theory]
    [InlineData(1)]
    public void Value_IsPositive(int value) => Assert.True(value > 0);
}
