using Microsoft.AspNetCore.DataProtection;

using Shopfolio.Infrastructure.Security;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The download link (T312 criteria 1, 2 and 4; KK1, KK2, KK4; 09 15.5 item 4; 05 7.7.4): the value carries the item's id and the end of its life, one hour, read against the one
/// time source; a value one second past its end is refused and the last second still opens; an empty, damaged, foreign-purpose or differently shaped value is
/// <see cref="DownloadLinkResult.Invalid"/>, never an exception, and the result does not say why. The clock is a fake: no test waits for real time.
/// </summary>
public sealed class DownloadLinksTests
{
    private static readonly DateTimeOffset Start = new(2026, 11, 7, 9, 0, 0, TimeSpan.Zero);

    private static readonly Guid Item = Guid.Parse("0f1e2d3c-4b5a-6978-8796-a5b4c3d2e1f0");

    private static (DownloadLinks Links, ProtectedValues Values) Create()
    {
        ProtectedValues values = new(new EphemeralDataProtectionProvider());
        return (new DownloadLinks(values), values);
    }

    [Fact]
    public void Lifetime_IsOneHour()
    {
        Assert.Equal(TimeSpan.FromHours(1), DownloadLinks.Lifetime);
    }

    [Fact]
    public void RoundTrip_GivesTheItemsId_AndTheValueIsUrlSafeText()
    {
        (DownloadLinks links, _) = Create();
        ManualTimeProvider clock = new(Start);

        string value = links.Create(Item, clock);
        DownloadLinkResult result = links.Resolve(value, clock);

        Assert.True(result.IsValid);
        Assert.Equal(Item, result.OrderItemId);
        Assert.Matches("^[A-Za-z0-9_-]+$", value);
    }

    [Fact]
    public void Value_IsAliveToTheLastSecondOfItsLife_AndRefusedOneSecondLater()
    {
        (DownloadLinks links, _) = Create();
        ManualTimeProvider clock = new(Start);
        string value = links.Create(Item, clock);

        clock.SetUtcNow(Start + DownloadLinks.Lifetime);
        Assert.True(links.Resolve(value, clock).IsValid);

        clock.SetUtcNow(Start + DownloadLinks.Lifetime + TimeSpan.FromSeconds(1));
        Assert.False(links.Resolve(value, clock).IsValid);
    }

    [Fact]
    public void ResolvingTwice_IsTheSameAnswer_TheValueIsNotConsumed()
    {
        (DownloadLinks links, _) = Create();
        ManualTimeProvider clock = new(Start);
        string value = links.Create(Item, clock);

        Assert.True(links.Resolve(value, clock).IsValid);
        Assert.True(links.Resolve(value, clock).IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("not-a-protected-value")]
    [InlineData("CfDJ8_garbage_garbage_garbage")]
    public void EmptyOrDamagedValue_IsInvalid_NeverAnException(string? value)
    {
        (DownloadLinks links, _) = Create();

        Assert.False(links.Resolve(value, new ManualTimeProvider(Start)).IsValid);
    }

    [Fact]
    public void ValueWithOneCharacterChanged_IsInvalid()
    {
        (DownloadLinks links, _) = Create();
        ManualTimeProvider clock = new(Start);
        string value = links.Create(Item, clock);
        string changed = value[..^2] + (value[^2] == 'A' ? 'B' : 'A') + value[^1];

        Assert.False(links.Resolve(changed, clock).IsValid);
    }

    [Fact]
    public void ValueOfAnotherKeyRing_IsInvalid()
    {
        (DownloadLinks links, _) = Create();
        (DownloadLinks other, _) = Create();
        ManualTimeProvider clock = new(Start);

        Assert.False(other.Resolve(links.Create(Item, clock), clock).IsValid);
    }

    [Fact]
    public void ValueProtectedForAnotherPurpose_IsInvalid_EveryOtherPurposeOfTheClosedList()
    {
        (DownloadLinks links, ProtectedValues values) = Create();
        ManualTimeProvider clock = new(Start);
        string payload = Item.ToString("N") + "." + (Start + DownloadLinks.Lifetime).ToUnixTimeSeconds();

        foreach (string purpose in DataProtectionPurposes.All.Where(p => p != DataProtectionPurposes.DownloadLink))
        {
            Assert.False(links.Resolve(values.Protect(purpose, payload), clock).IsValid, purpose);
        }

        Assert.True(links.Resolve(values.Protect(DataProtectionPurposes.DownloadLink, payload), clock).IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("only-text")]
    [InlineData("0f1e2d3c4b5a69788796a5b4c3d2e1f0")]
    [InlineData("0f1e2d3c4b5a69788796a5b4c3d2e1f0.")]
    [InlineData("0f1e2d3c4b5a69788796a5b4c3d2e1f0.abc")]
    [InlineData("0f1e2d3c4b5a69788796a5b4c3d2e1f0.-5")]
    [InlineData("0f1e2d3c4b5a69788796a5b4c3d2e1f0:4102444800")]
    [InlineData("0f1e2d3c-4b5a-6978-8796-a5b4c3d2e1f0.4102444800")]
    [InlineData("zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz.4102444800")]
    [InlineData("0f1e2d3c4b5a69788796a5b4c3d2e1f0.4102444800.extra")]
    public void ProperlyProtectedPayloadOfAnotherShape_IsInvalid(string payload)
    {
        (DownloadLinks links, ProtectedValues values) = Create();

        Assert.False(links.Resolve(values.Protect(DataProtectionPurposes.DownloadLink, payload), new ManualTimeProvider(Start)).IsValid);
    }

    [Fact]
    public void ReadingTheItemOfAnInvalidResult_IsAProgrammingError()
    {
        Assert.Throws<InvalidOperationException>(() => DownloadLinkResult.Invalid.OrderItemId);
    }
}
