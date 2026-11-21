using System.ComponentModel.DataAnnotations;

namespace Shopfolio.Infrastructure.Validation;

/// <summary>
/// The rule of the WhatsApp number (02 3.27.12; 06 2.5.1), written once for the modules that carry it (D-1332): optional; a number of the accepted telephone forms is stored as its ten digits,
/// an empty one as nothing, and a text that is not a telephone number is refused with <see cref="FieldCodes.InvalidPhone"/>. The home page section's save (Settings) and the social links'
/// own check (CorporateContent) both ask here, so the two cannot drift apart, and neither calls the other module's internals (09 6).
/// </summary>
public static class WhatsAppNumberRule
{
    /// <summary>The stored form of <paramref name="typed"/> and the code of its refusal - null when the number is accepted (or empty).</summary>
    public static (string? Stored, string? RejectionCode) Check(string? typed)
    {
        string? stored = RequestFormat.StoredPhone(typed);
        bool refused = stored is not null && new InvalidPhoneAttribute().GetValidationResult(stored, new ValidationContext(stored)) is not null;
        return (stored, refused ? FieldCodes.InvalidPhone : null);
    }
}
