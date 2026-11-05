using Microsoft.AspNetCore.Mvc.ModelBinding;

using Shopfolio.Infrastructure.Validation;

namespace Shopfolio.Infrastructure.Binding;

/// <summary>
/// The rule of a list field that selects items - <c>Items</c>: one row per item (07 5.1.5; UC-40, UC-43, UC-129, UC-130). The form never produces two
/// rows of the same item; a hand-made request that does is an invalid item selection: 422 with a field message, the quantities are not added up and no
/// row is processed. The handler calls <see cref="RejectRepeated"/> before it reads any row and answers the form again when the model state is invalid.
/// </summary>
internal static class ItemRows
{
    /// <summary>
    /// The code the invalid item selection carries (07 6.2.2: a value outside the closed set the form offered; the rows the order has are that set).
    /// The code is the error message of the field; the page filter turns it into the text (09 3.4 item 3) and it is never shown or sent as a code.
    /// </summary>
    public const string InvalidSelectionCode = FieldCodes.InvalidListValue;

    /// <summary>
    /// Adds the field error when two rows carry the same item key. Returns <see langword="true"/> when every key appears once (an empty list included;
    /// whether an empty selection is allowed is the form's own <c>FieldRequired</c>), <see langword="false"/> when the selection is invalid.
    /// </summary>
    public static bool RejectRepeated<TKey>(ModelStateDictionary modelState, string listField, IEnumerable<TKey> itemKeys)
        where TKey : notnull
    {
        ArgumentNullException.ThrowIfNull(modelState);
        ArgumentException.ThrowIfNullOrEmpty(listField);
        ArgumentNullException.ThrowIfNull(itemKeys);

        HashSet<TKey> seen = [];
        foreach (TKey key in itemKeys)
        {
            if (!seen.Add(key))
            {
                modelState.AddFieldError(listField, InvalidSelectionCode);
                return false;
            }
        }

        return true;
    }
}
