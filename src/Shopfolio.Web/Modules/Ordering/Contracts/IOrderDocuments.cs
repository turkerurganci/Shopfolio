using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The one face of an order's two documents (T356; 09 9.3 item 4, 09 11.4 item 4): the Pre-Information Form and the Distance Sales Contract of a confirmed order. The frozen copy holds no
/// personal data (the customer's tokens stay as written); this reads it and fills the tokens from the confirmation's parties (the first row of each history, T355) and the province names, so
/// the order page, the panel, the confirmation e-mail's body and the panel's resend all show the same text. The copy is written once, in the confirmation's transaction (T348).
/// </summary>
public interface IOrderDocuments
{
    /// <summary>
    /// The text of one document: <see cref="LegalTextType.PreInformationForm"/> or <see cref="LegalTextType.DistanceSalesContract"/> - another type is an <see cref="ArgumentException"/>.
    /// The tokens of a disposed order's erased fields read as empty text; an order that does not exist is an <see cref="InvalidOperationException"/>.
    /// </summary>
    Task<string> GetTextAsync(Guid orderId, LegalTextType type, CancellationToken ct);
}
