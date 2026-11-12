using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Pages.Components;

namespace Shopfolio.Pages.Storefront;

/// <summary>
/// The five topics of the contact form as the visitor reads them (E-10; T506; 02 3.32.3; 04 5.10.5): the closed list in the order of <see cref="ContactTopic"/>. The value sent is the member's name,
/// so a text typed by hand is no topic (04 2.12.1.3); the defect claim is not among them (it goes from the order page). The firm's own notice names the same topics in its e-mail.
/// </summary>
public static class ContactTopicLabels
{
    /// <summary>The visible text of a topic.</summary>
    public static string TextOf(ContactTopic topic) => topic switch
    {
        ContactTopic.GeneralQuestion => "Genel soru",
        ContactTopic.AboutOrder => "Sipariş hakkında",
        ContactTopic.AboutProduct => "Ürün hakkında",
        ContactTopic.PersonalDataRequest => "KVKK talebi",
        ContactTopic.Other => "Diğer",
        _ => throw new ArgumentOutOfRangeException(nameof(topic), topic, "Unknown contact topic."),
    };

    /// <summary>The options of the closed list: the member's name as the value, the visible text as the text, in the list's order.</summary>
    public static IReadOnlyList<FieldOption> Options { get; } =
        [.. Enum.GetValues<ContactTopic>().Select(t => new FieldOption(t.ToString(), TextOf(t)))];
}
