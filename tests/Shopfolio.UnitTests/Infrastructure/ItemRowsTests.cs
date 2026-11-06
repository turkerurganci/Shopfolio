using Microsoft.AspNetCore.Mvc.ModelBinding;

using Shopfolio.Infrastructure.Binding;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The one-row-per-item rule of the <c>Items</c> list (T211 criterion 3, KK3; 07 5.1.5): a repeated item key is an invalid selection with a field
/// message - the rows are not added up and the handler processes none of them.
/// </summary>
public sealed class ItemRowsTests
{
    private static readonly Guid _first = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid _second = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public void RejectRepeated_DistinctKeys_IsAccepted_AndAddsNoError()
    {
        ModelStateDictionary state = new();

        bool accepted = ItemRows.RejectRepeated(state, "Items", [_first, _second]);

        Assert.True(accepted);
        Assert.True(state.IsValid);
    }

    [Fact]
    public void RejectRepeated_EmptyList_IsLeftToTheFormsOwnRequiredRule()
    {
        ModelStateDictionary state = new();

        Assert.True(ItemRows.RejectRepeated(state, "Items", Array.Empty<Guid>()));
        Assert.True(state.IsValid);
    }

    [Fact]
    public void RejectRepeated_SameKeyInTwoRows_AddsOneFieldErrorWithTheCode_AndIsRejected()
    {
        ModelStateDictionary state = new();

        bool accepted = ItemRows.RejectRepeated(state, "Items", [_first, _second, _first]);

        Assert.False(accepted);
        Assert.False(state.IsValid);
        Assert.Equal(["InvalidListValue"], state["Items"]!.Errors.Select(e => e.ErrorMessage));
    }

    [Fact]
    public void RejectRepeated_SameKeyInAdjacentRowsWithDifferentQuantities_IsStillRejected()
    {
        ModelStateDictionary state = new();
        (Guid ItemId, int Quantity)[] rows = [(_first, 1), (_first, 2)];

        bool accepted = ItemRows.RejectRepeated(state, "Items", rows.Select(r => r.ItemId));

        Assert.False(accepted);
        Assert.Single(state["Items"]!.Errors);
    }
}
