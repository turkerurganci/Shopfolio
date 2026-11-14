using Shopfolio.Infrastructure.Errors;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.UnitTests.Ordering;

/// <summary>
/// The internal note's answer as the page's runner reads it (T531 KK4, KK5; 07 UC-152; 09 10.9): saved and unchanged are applied and commit; the shown text no longer the note's is the conflict (409)
/// and rolls back; an order that takes no note is closed (422) and rolls back.
/// </summary>
public sealed class PanelNoteResultTests
{
    [Fact]
    public void ASavedOrUnchangedNote_IsApplied()
    {
        Assert.False(new PanelNoteResult(SaveInternalNoteResult.Saved).Gate.IsRejected);
        Assert.False(new PanelNoteResult(SaveInternalNoteResult.Unchanged).Gate.IsRejected);
        Assert.False(new PanelNoteResult(SaveInternalNoteResult.Saved).IsRejected);
    }

    [Fact]
    public void AChangedNote_IsTheConflict_AndRollsBack()
    {
        PanelNoteResult result = new(SaveInternalNoteResult.DisplayedValueConflict("Güncel not"));

        Assert.True(result.IsRejected);
        Assert.True(result.Gate.IsRejected);
        Assert.True(result.Gate.VersionChanged);
        Assert.Equal(ErrorCode.DisplayedValueConflict, result.Note.Code);
        Assert.Equal("Güncel not", result.Note.CurrentText);
    }

    [Fact]
    public void AnOrderThatTakesNoNote_IsClosed_AndRollsBack()
    {
        PanelNoteResult result = new(SaveInternalNoteResult.OperationNotAvailable);

        Assert.True(result.Gate.IsRejected);
        Assert.False(result.Gate.VersionChanged);
        Assert.False(result.Gate.Verdict.IsAllowed);
    }
}
