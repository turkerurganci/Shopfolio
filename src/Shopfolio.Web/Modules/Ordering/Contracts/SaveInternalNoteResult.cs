using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The outcome of the internal note's write (09 10.9 item 3; 09 8.1 item 2; 06 4.20): saved (the note written, changed or deleted, with its trail row);
/// unchanged (the sent text is the shown text and the note still holds it - nothing is written, no trail row); or refused with the code of the error
/// registry. <see cref="ErrorCode.DisplayedValueConflict"/> carries the note as it is now (the empty text when there is none): the panel says the note
/// changed and shows it (02 3.31.2). <see cref="ErrorCode.OperationNotAvailable"/> is an order that does not exist, whose personal data is disposed
/// (03 1.11.27; K-1095) or a call without an administrator. The transaction runner rolls a refused write back.
/// </summary>
public sealed class SaveInternalNoteResult : IOperationResult
{
    private SaveInternalNoteResult(bool isSaved, ErrorCode? code, string? currentText)
    {
        IsSaved = isSaved;
        Code = code;
        CurrentText = currentText;
    }

    /// <summary>The note was written, changed or deleted and the trail row added.</summary>
    public static SaveInternalNoteResult Saved { get; } = new(true, null, null);

    /// <summary>The sent text is the shown text and the note still holds it; nothing was written.</summary>
    public static SaveInternalNoteResult Unchanged { get; } = new(false, null, null);

    /// <summary>The order takes no note (07 9.9.31: <c>OperationNotAvailable</c>); nothing was written.</summary>
    public static SaveInternalNoteResult OperationNotAvailable { get; } = new(false, ErrorCode.OperationNotAvailable, null);

    public bool IsSaved { get; }

    /// <summary>The refusal's code; empty when the note was saved or nothing changed.</summary>
    public ErrorCode? Code { get; }

    /// <summary>The note as it is now; filled only with <see cref="ErrorCode.DisplayedValueConflict"/> (the empty text when the note is gone).</summary>
    public string? CurrentText { get; }

    public bool IsRejected => Code is not null;

    /// <summary>The note is no longer the text the form showed (07 6.2: <c>DisplayedValueConflict</c>); the current note goes back to the panel.</summary>
    public static SaveInternalNoteResult DisplayedValueConflict(string? currentText) => new(false, ErrorCode.DisplayedValueConflict, currentText ?? string.Empty);
}
