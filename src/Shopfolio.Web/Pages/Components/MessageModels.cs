using System.Globalization;

using Microsoft.AspNetCore.Mvc.ModelBinding;

using Shopfolio.Infrastructure.Security;

namespace Shopfolio.Pages.Components;

/// <summary>The four meanings of a message (04 2.3 "Varyantlar"): error, warning, information, success. The meaning never travels by colour alone (02 6.1.6).</summary>
public enum MessageMeaning
{
    Error,
    Warning,
    Info,
    Success,
}

/// <summary>The four places a message shows (04 2.3.1): at the field, in place and lasting, once told at the head of the content, and short-lived at the edge.</summary>
public enum MessagePlace
{
    /// <summary>04 2.3.1.1: under the field, tied to its label.</summary>
    Field,

    /// <summary>04 2.3.1.2: inside the row or the section, for as long as the condition lasts.</summary>
    InPlace,

    /// <summary>04 2.3.1.3: a closable box at the head of the content area, told once.</summary>
    OnceTold,

    /// <summary>04 2.3.1.4: a line at the edge of the screen that goes by itself; success only.</summary>
    ShortLived,
}

/// <summary>
/// The one message view every screen draws with <c>_Message</c> (T541; 04 OB-03): the meaning, the place and the text. Every message carries a symbol and its text; the meaning is also spoken by
/// a hidden word so a reader without the symbol or the colour gets it (02 6.1.6). The text is the screen's own: the closed codes of the model state are turned into texts by the message catalog before
/// a view sees them (09 3.4 item 4, 8.2 item 4) - a message never carries an error code.
/// </summary>
/// <param name="Meaning">What the message means.</param>
/// <param name="Place">Where it shows; <see cref="MessagePlace.ShortLived"/> is built by <see cref="Messages.ShortLivedSuccess"/> only.</param>
/// <param name="Text">The visible text; never empty.</param>
/// <param name="Id">The element id the message is referred to by; the field message's id is the one the field's <c>aria-describedby</c> names.</param>
public sealed record MessageView(MessageMeaning Meaning, MessagePlace Place, string Text, string? Id = null)
{
    /// <summary>The symbol drawn before the text; each meaning has a different shape, so the symbol alone separates them (02 6.1.6).</summary>
    public string Symbol => MessageSymbols.Of(Meaning);

    /// <summary>The word a screen reader hears before the text.</summary>
    public string MeaningWord => MessageSymbols.WordOf(Meaning);

    /// <summary>The style modifier of the meaning (<c>message--error</c> and so on).</summary>
    public string Modifier => "message--" + MessageSymbols.KeyOf(Meaning);

    /// <summary>An error or a warning interrupts the reader (<c>alert</c>); information and success are polite (<c>status</c>).</summary>
    public string Role => Meaning is MessageMeaning.Error or MessageMeaning.Warning ? "alert" : "status";
}

/// <summary>The symbol, the spoken word and the style key of every meaning, in one place (04 2.3; 02 6.1.6).</summary>
public static class MessageSymbols
{
    /// <summary>The symbol of a meaning: a cross, an exclamation mark, an "i" and a tick.</summary>
    public static string Of(MessageMeaning meaning) => meaning switch
    {
        MessageMeaning.Error => "✕",
        MessageMeaning.Warning => "!",
        MessageMeaning.Info => "i",
        MessageMeaning.Success => "✓",
        _ => throw new ArgumentOutOfRangeException(nameof(meaning), meaning, "A message has one of four meanings."),
    };

    /// <summary>The word of a meaning, spoken before the text.</summary>
    public static string WordOf(MessageMeaning meaning) => meaning switch
    {
        MessageMeaning.Error => "Hata:",
        MessageMeaning.Warning => "Uyarı:",
        MessageMeaning.Info => "Bilgi:",
        MessageMeaning.Success => "Başarılı:",
        _ => throw new ArgumentOutOfRangeException(nameof(meaning), meaning, "A message has one of four meanings."),
    };

    /// <summary>The key the style sheet's modifier is named by.</summary>
    public static string KeyOf(MessageMeaning meaning) => meaning switch
    {
        MessageMeaning.Error => "error",
        MessageMeaning.Warning => "warning",
        MessageMeaning.Info => "info",
        MessageMeaning.Success => "success",
        _ => throw new ArgumentOutOfRangeException(nameof(meaning), meaning, "A message has one of four meanings."),
    };
}

/// <summary>
/// The only constructors of a <see cref="MessageView"/> (T541; 04 2.3.1): one for each place. The rules of the places are in the constructors, not in the screens: a short-lived notice is a
/// success and nothing else (04 2.3.4), the panel's blocking message says the reason, the count and the way out and opens no window (04 2.3.3), the neutral limit message says nothing but the
/// sentence (02 6.1.5; 04 2.3.5).
/// </summary>
public static class Messages
{
    /// <summary>04 2.3.1.1: a field's own message, at the field; <paramref name="fieldId"/> is the field's element id and the message's id is <c>{fieldId}-message</c>.</summary>
    public static MessageView ForField(string fieldId, string text) =>
        new(MessageMeaning.Error, MessagePlace.Field, Required(text), FieldMessageId(fieldId));

    /// <summary>The element id of a field's message; the field's <c>aria-describedby</c> names it so the message is tied to the field (04 2.12.1.1).</summary>
    public static string FieldMessageId(string fieldId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldId);
        return fieldId + "-message";
    }

    /// <summary>04 2.3.1.2: a lasting message in place - the sold-out line, the changed price, the refused action; it stays while the condition does.</summary>
    public static MessageView InPlace(MessageMeaning meaning, string text, string? id = null) =>
        new(meaning, MessagePlace.InPlace, Required(text), id);

    /// <summary>04 2.3.1.3: a message told once at the head of the content, closable; it does not come back when the page is reloaded.</summary>
    public static MessageView OnceTold(MessageMeaning meaning, string text, string? id = null) =>
        new(meaning, MessagePlace.OnceTold, Required(text), id);

    /// <summary>
    /// 04 2.3.1.4, 2.3.4: the short-lived notice; only a small success whose result is already on the screen. An error, a warning, information and the result of an irreversible operation
    /// are lasting messages - asking for them here is a programming error and fails.
    /// </summary>
    public static MessageView ShortLivedSuccess(string text) => new(MessageMeaning.Success, MessagePlace.ShortLived, Required(text));

    /// <summary>
    /// 04 2.3.3: the panel's block message, in place where the operation was started: the reason, the number of the records that block it when there is one, and the way out
    /// ("önce taslağa alın"). The count is written as a number; a null count says nothing about records.
    /// </summary>
    public static MessageView PanelBlock(string reason, int? blockingRecordCount, string wayOut, string? id = null)
    {
        string text = Required(reason);
        if (blockingRecordCount is { } count)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
            text += " Engelleyen kayıt sayısı: " + count.ToString("N0", CultureInfo.GetCultureInfo("tr-TR")) + ".";
        }

        text += " " + Required(wayOut);
        return new MessageView(MessageMeaning.Error, MessagePlace.InPlace, text, id);
    }

    /// <summary>
    /// 04 2.3.5, 02 6.1.5: the neutral limit message beside the blocked button; the sentence of <see cref="NeutralMessages.LimitExceeded"/> and nothing more - no remaining attempts, no length of the
    /// block, no axis.
    /// </summary>
    public static MessageView NeutralLimit(string? id = null) => new(MessageMeaning.Warning, MessagePlace.InPlace, NeutralMessages.LimitExceeded, id);

    private static string Required(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);
        return text.Trim();
    }
}

/// <summary>One invalid field of a submitted form, as the summary lists it: the element id the link points to and the field's message.</summary>
public sealed record FormFieldError(string FieldId, string Text);

/// <summary>
/// The summary at the head of a submitted form (T541; 04 2.3.2, K-743): how many fields are wrong, the list of them and the field the focus goes to. The invalid fields are marked together; each
/// keeps its own message at the field; the typed values stay (the form is drawn again with the sent values, 07 2.2.2). The model-level messages of the state - the action's own refusal, the neutral
/// limit message, the unexpected-error message - are not field errors: they are lasting messages in place and are not counted (04 2.3.1.2).
/// </summary>
public sealed class FormErrorSummary
{
    private readonly List<FormFieldError> _fields;

    public FormErrorSummary(IEnumerable<FormFieldError> fields, IEnumerable<string>? formMessages = null)
    {
        ArgumentNullException.ThrowIfNull(fields);
        _fields = [.. fields];
        FormMessages = [.. formMessages ?? []];
    }

    /// <summary>The element id of the summary; it takes the focus when no field can.</summary>
    public const string ElementId = "form-error-summary";

    /// <summary>The invalid fields, in the order the form draws them (the model state's order).</summary>
    public IReadOnlyList<FormFieldError> Fields => _fields;

    /// <summary>The messages at the level of the action, in place (04 2.3.1.2).</summary>
    public IReadOnlyList<string> FormMessages { get; }

    /// <summary>The number of invalid fields; the heading says it.</summary>
    public int ErrorCount => _fields.Count;

    /// <summary>True when a field is wrong; the summary is drawn only then. The messages of the action are not part of the drawing: the screen puts each where the action's own button is (04 2.3.1.2, 2.3.5).</summary>
    public bool HasContent => _fields.Count > 0;

    /// <summary>The element id of the first invalid field, where the focus goes (04 2.3.2); null when no field is wrong.</summary>
    public string? FirstFieldId => _fields.Count > 0 ? _fields[0].FieldId : null;

    /// <summary>The heading: "N alanda hata var" - the number is the number of the fields.</summary>
    public string Heading => ErrorCount == 1 ? "1 alanda hata var" : ErrorCount.ToString("N0", CultureInfo.GetCultureInfo("tr-TR")) + " alanda hata var";

    /// <summary>"autofocus" for the first invalid field and nothing for the others: the form puts the result in the field's tag, so the focus goes to the first wrong field without a script (04 2.3.2).</summary>
    public string AutofocusFor(string fieldId) => fieldId == FirstFieldId ? "autofocus" : string.Empty;

    /// <summary>True when the field is one of the wrong fields (it carries <c>aria-invalid</c>).</summary>
    public bool IsInvalid(string fieldId) => _fields.Exists(f => f.FieldId == fieldId);

    /// <summary>
    /// The summary of a model state: the field entries with an error become the list (the first error of a field is its message), the empty-key entry becomes the action's messages. The element id of a
    /// field is the framework's id of its name (<c>Input.Name</c> to <c>Input_Name</c>), which the field's tag helper writes.
    /// </summary>
    /// <param name="modelState">The state of the submitted form.</param>
    /// <param name="formOrder">
    /// The names of the form's fields in the order the form draws them. The framework walks a model state in the alphabetical order of the names, not in the order of the form, so a screen that wants
    /// "the first wrong field" to be the first one on the page passes its field names; the fields not named keep their place after the named ones.
    /// </param>
    public static FormErrorSummary From(ModelStateDictionary modelState, IReadOnlyList<string>? formOrder = null)
    {
        ArgumentNullException.ThrowIfNull(modelState);
        List<FormFieldError> fields = [];
        List<string> messages = [];
        foreach (KeyValuePair<string, ModelStateEntry> pair in modelState)
        {
            ModelStateEntry? entry = pair.Value;
            if (entry is null || entry.Errors.Count == 0)
            {
                continue;
            }

            if (string.IsNullOrEmpty(pair.Key))
            {
                messages.AddRange(entry.Errors.Select(e => e.ErrorMessage).Where(m => !string.IsNullOrWhiteSpace(m)));
                continue;
            }

            fields.Add(new FormFieldError(FieldIdOf(pair.Key), entry.Errors[0].ErrorMessage));
        }

        if (formOrder is { Count: > 0 })
        {
            // The model state is case-insensitive and a handler's parameter name may spell the prefix in lower case ("input.Name"); the form's own spelling is the element id's.
            List<string> ids = [.. formOrder.Select(FieldIdOf)];
            int RankOf(FormFieldError field) => ids.FindIndex(id => string.Equals(id, field.FieldId, StringComparison.OrdinalIgnoreCase)) is var index and >= 0 ? index : int.MaxValue;
            FormFieldError InFormSpelling(FormFieldError field) => RankOf(field) is var rank and not int.MaxValue ? field with { FieldId = ids[rank] } : field;

            fields = [.. fields.Select(InFormSpelling).OrderBy(RankOf)];
        }

        return new FormErrorSummary(fields, messages);
    }

    /// <summary>The element id the framework writes for a field name: the dots and the brackets of the name become underscores (<c>Variants[0].Sku</c> to <c>Variants_0__Sku</c>).</summary>
    public static string FieldIdOf(string fieldName)
    {
        ArgumentException.ThrowIfNullOrEmpty(fieldName);
        return fieldName.Replace('.', '_').Replace('[', '_').Replace(']', '_');
    }
}
