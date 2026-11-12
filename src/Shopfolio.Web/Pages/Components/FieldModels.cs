namespace Shopfolio.Pages.Components;

/// <summary>How a field is told apart in words (04 2.12.1.1, 7.3.1): a required field, an optional one, or a panel field that only the publication needs.</summary>
public enum FieldRequirement
{
    /// <summary>"(zorunlu)".</summary>
    Required,

    /// <summary>"(isteğe bağlı)".</summary>
    Optional,

    /// <summary>"(yayın için zorunlu)": it blocks the publication, not the save (04 7.3.1; K-584).</summary>
    RequiredForPublish,
}

/// <summary>The kinds of field the form component draws. A closed list is its own kind: it is a <c>select</c> and never a text box (04 2.12.1.3).</summary>
public enum FieldKind
{
    Text,
    Email,
    Telephone,
    TextArea,
    ClosedList,
    Number,

    /// <summary>A date (T544; 04 OB-16): the browser's date input, which sends <c>yyyy-MM-dd</c>.</summary>
    Date,

    /// <summary>A secret typed by the user (T514; 04 7.3.2.2): the input hides it and the form never shows a sent one again.</summary>
    Concealed,
}

/// <summary>One choice of a closed list: the value that is sent, the text that is shown, and - for a choice the screen shows but does not let be chosen - the reason beside it.</summary>
public sealed record FieldOption(string Value, string Text, bool Disabled = false, string? Note = null);

/// <summary>
/// The one field every form draws with <c>_Field</c> (T543; 04 OB-12): a visible label, the words that say whether the field is required, the field itself and - under it, tied to the field by
/// <c>aria-describedby</c> - its message (04 2.12.1.1). A closed list (a province, a topic, a company type, a reason) has options and no text box: free text is not a way to fill it (04 2.12.1.3).
/// A field is built by <see cref="Fields"/> only, so the label and the requirement are never missing. The value is what the form shows: the sent value after a 422, the account's value for a member (04 2.12.1.7).
/// </summary>
public sealed class FieldView
{
    internal FieldView(string name, string label, FieldKind kind, FieldRequirement requirement)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        Name = name;
        Label = label.Trim();
        Kind = kind;
        Requirement = requirement;
    }

    /// <summary>The form's name of the field (<c>Input.RecipientName</c>).</summary>
    public string Name { get; }

    /// <summary>The visible label; never empty.</summary>
    public string Label { get; }

    public FieldKind Kind { get; }

    public FieldRequirement Requirement { get; }

    /// <summary>The value the field shows; for a closed list the value of the chosen option.</summary>
    public string? Value { get; internal init; }

    /// <summary>The character limit written into <c>maxlength</c>; it is a product constant passed by the screen, never a number typed here (09 3.4 item 6).</summary>
    public int? MaxLength { get; internal init; }

    /// <summary>The earliest date the browser's date input offers (<c>min</c>, <c>yyyy-MM-dd</c>); a hint only - the sent value is checked by <see cref="DateFields"/>.</summary>
    public string? Min { get; internal init; }

    /// <summary>The latest date the browser's date input offers (<c>max</c>, <c>yyyy-MM-dd</c>); a hint only.</summary>
    public string? Max { get; internal init; }

    /// <summary>The options of a closed list; empty for the other kinds.</summary>
    public IReadOnlyList<FieldOption> Options { get; internal init; } = [];

    /// <summary>A line under the label that helps filling the field; not a message.</summary>
    public string? Hint { get; internal init; }

    /// <summary>The browser's autofill hint (<c>autocomplete</c>).</summary>
    public string? Autocomplete { get; internal init; }

    /// <summary>A field that is shown and cannot be changed (the panel shows a customer's IBAN to the administrator, 04 2.12.3.2).</summary>
    public bool ReadOnly { get; internal init; }

    /// <summary>The message of the field, drawn under it; null when the field is right (04 2.12.1.1).</summary>
    public MessageView? Error { get; internal init; }

    /// <summary>True for the first wrong field of a submitted form: the field's tag carries <c>autofocus</c> (04 2.3.2).</summary>
    public bool Autofocus { get; internal init; }

    /// <summary>The element id: the framework's id of the name (<c>Input.RecipientName</c> to <c>Input_RecipientName</c>), the id the label, the summary and the message are tied by.</summary>
    public string Id => FormErrorSummary.FieldIdOf(Name);

    /// <summary>The id of the message under the field; the field's <c>aria-describedby</c> names it.</summary>
    public string MessageId => Messages.FieldMessageId(Id);

    /// <summary>The id of the hint; named by <c>aria-describedby</c> next to the message.</summary>
    public string HintId => Id + "-hint";

    /// <summary>The words that say whether the field is required (04 2.12.1.1, 7.3.1).</summary>
    public string RequirementText => FieldRequirements.TextOf(Requirement);

    /// <summary>True when the field has a message.</summary>
    public bool IsInvalid => Error is not null;

    /// <summary>The ids <c>aria-describedby</c> lists: the message when there is one - it is the tie of the message to the field (04 2.12.1.1) -, then the hint; null when the field has neither.</summary>
    public string? DescribedBy => (Error, Hint) switch { (null, null) => null, (not null, null) => MessageId, (null, not null) => HintId, _ => MessageId + " " + HintId };

    /// <summary>The <c>type</c> of the input tag; a closed list and a text area are not inputs.</summary>
    public string InputType => Kind switch
    {
        FieldKind.Email => "email",
        FieldKind.Telephone => "tel",
        FieldKind.Date => "date",
        FieldKind.Concealed => "password",
        _ => "text",
    };

    /// <summary>The <c>inputmode</c> of the input tag; a quantity is a whole number (04 2.12.1.8).</summary>
    public string? InputMode => Kind == FieldKind.Number ? "numeric" : null;

    /// <summary>True when the option is the field's value.</summary>
    public bool IsSelected(FieldOption option)
    {
        ArgumentNullException.ThrowIfNull(option);
        return string.Equals(option.Value, Value, StringComparison.Ordinal);
    }

    /// <summary>True when the value is one of the options; an empty value is not a choice (04 2.12.1.3).</summary>
    public bool HasValidChoice => Kind == FieldKind.ClosedList && ClosedLists.Accepts(Options, Value);
}

/// <summary>The words of the three requirements, in one place (04 2.12.1.1, 7.3.1).</summary>
public static class FieldRequirements
{
    public static string TextOf(FieldRequirement requirement) => requirement switch
    {
        FieldRequirement.Required => "(zorunlu)",
        FieldRequirement.Optional => "(isteğe bağlı)",
        FieldRequirement.RequiredForPublish => "(yayın için zorunlu)",
        _ => throw new ArgumentOutOfRangeException(nameof(requirement), requirement, "A field has one of three requirements."),
    };
}

/// <summary>The closed list's one rule: a value is a choice only when it is the value of an option (04 2.12.1.3, 7.3.2.16).</summary>
public static class ClosedLists
{
    /// <summary>True when <paramref name="value"/> is the value of one of the options and the option can be chosen. A text that only looks like an option (another case, a space around it) is not one.</summary>
    public static bool Accepts(IReadOnlyList<FieldOption> options, string? value)
    {
        ArgumentNullException.ThrowIfNull(options);
        return !string.IsNullOrEmpty(value) && options.Any(o => !o.Disabled && string.Equals(o.Value, value, StringComparison.Ordinal));
    }
}

/// <summary>The only constructors of a <see cref="FieldView"/> (T543; 04 OB-12): one for each kind, so no field is drawn without a label and a requirement.</summary>
public static class Fields
{
    public static FieldView Text(string name, string label, FieldRequirement requirement, string? value = null, int? maxLength = null, string? autocomplete = null, string? hint = null) =>
        new(name, label, FieldKind.Text, requirement) { Value = value, MaxLength = CheckedLimit(maxLength), Autocomplete = autocomplete, Hint = hint };

    public static FieldView Email(string name, string label, FieldRequirement requirement, string? value = null, string? autocomplete = "email") =>
        new(name, label, FieldKind.Email, requirement) { Value = value, Autocomplete = autocomplete };

    public static FieldView Telephone(string name, string label, FieldRequirement requirement, string? value = null, string? autocomplete = "tel") =>
        new(name, label, FieldKind.Telephone, requirement) { Value = value, Autocomplete = autocomplete };

    public static FieldView TextArea(string name, string label, FieldRequirement requirement, string? value = null, int? maxLength = null, string? autocomplete = null, string? hint = null) =>
        new(name, label, FieldKind.TextArea, requirement) { Value = value, MaxLength = CheckedLimit(maxLength), Autocomplete = autocomplete, Hint = hint };

    /// <summary>A secret field (T514): no value is ever put in it, so a form drawn again after a refusal does not show the secret that was sent (04 7.3.2.2; 09 15.1).</summary>
    public static FieldView Concealed(string name, string label, FieldRequirement requirement, string? autocomplete = "new-password", string? hint = null) =>
        new(name, label, FieldKind.Concealed, requirement) { Autocomplete = autocomplete, Hint = hint };

    public static FieldView Number(string name, string label, FieldRequirement requirement, string? value = null) =>
        new(name, label, FieldKind.Number, requirement) { Value = value };

    /// <summary>A date field (T544; 04 OB-16): the value is the sent <c>yyyy-MM-dd</c> text; <paramref name="min"/> and <paramref name="max"/> are the browser's hints.</summary>
    public static FieldView Date(string name, string label, FieldRequirement requirement, string? value = null, DateOnly? min = null, DateOnly? max = null, string? hint = null) =>
        new(name, label, FieldKind.Date, requirement)
        {
            Value = value,
            Min = min?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            Max = max?.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture),
            Hint = hint,
        };

    /// <summary>A closed list (04 2.12.1.3): at least one option; the value is one of the options or empty.</summary>
    public static FieldView ClosedList(string name, string label, FieldRequirement requirement, IReadOnlyList<FieldOption> options, string? value = null, string? hint = null)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (options.Count == 0)
        {
            throw new ArgumentException("A closed list has at least one option.", nameof(options));
        }

        return new FieldView(name, label, FieldKind.ClosedList, requirement) { Value = value, Options = options, Hint = hint };
    }

    /// <summary>
    /// The field with its place in a submitted form (04 2.3.2, 2.12.1.1): the message of the summary that belongs to it - its first error, under the field - and the autofocus when it is the
    /// first wrong field. A field the summary does not name is returned unchanged.
    /// </summary>
    public static FieldView Bind(FieldView field, FormErrorSummary? summary)
    {
        ArgumentNullException.ThrowIfNull(field);
        if (summary is null)
        {
            return field;
        }

        FormFieldError? error = summary.Fields.FirstOrDefault(f => string.Equals(f.FieldId, field.Id, StringComparison.OrdinalIgnoreCase));
        if (error is null)
        {
            return field;
        }

        return new FieldView(field.Name, field.Label, field.Kind, field.Requirement)
        {
            Value = field.Value,
            MaxLength = field.MaxLength,
            Options = field.Options,
            Hint = field.Hint,
            Min = field.Min,
            Max = field.Max,
            Autocomplete = field.Autocomplete,
            ReadOnly = field.ReadOnly,
            Error = Messages.ForField(field.Id, error.Text),
            Autofocus = string.Equals(field.Id, summary.FirstFieldId, StringComparison.OrdinalIgnoreCase),
        };
    }

    /// <summary>The fields with the summary bound to each (see <see cref="Bind"/>).</summary>
    public static IReadOnlyList<FieldView> BindAll(IEnumerable<FieldView> fields, FormErrorSummary? summary)
    {
        ArgumentNullException.ThrowIfNull(fields);
        return [.. fields.Select(f => Bind(f, summary))];
    }

    /// <summary>The field as a read-only one (04 2.12.3.2): the same label, value and requirement, not changeable.</summary>
    public static FieldView AsReadOnly(FieldView field)
    {
        ArgumentNullException.ThrowIfNull(field);
        return new FieldView(field.Name, field.Label, field.Kind, field.Requirement)
        {
            Value = field.Value,
            MaxLength = field.MaxLength,
            Options = field.Options,
            Hint = field.Hint,
            Min = field.Min,
            Max = field.Max,
            Autocomplete = field.Autocomplete,
            ReadOnly = true,
            Error = field.Error,
            Autofocus = field.Autofocus,
        };
    }

    // A character limit is a product constant (09 3.5); a zero or a negative limit is a programming error.
    private static int? CheckedLimit(int? maxLength)
    {
        if (maxLength is { } limit)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(limit, 1, nameof(maxLength));
        }

        return maxLength;
    }
}

/// <summary>
/// One section of a form that is saved by sections (04 2.12.1.5; K-752): its heading, its fields and its own save button. The form names the section's handler (07 2.2.1), so a
/// section saves its own fields and the others stay as they are. The section is its own <c>form</c>: the fields of another section are not sent with it.
/// </summary>
/// <param name="Id">The element id of the section.</param>
/// <param name="Heading">The section's heading.</param>
/// <param name="Fields">The fields of the section, in the order they are drawn.</param>
/// <param name="SaveLabel">The text of the section's own save button.</param>
/// <param name="Handler">The page handler the section's form names (<c>SaveBrand</c>).</param>
public sealed record FormSectionView(string Id, string Heading, IReadOnlyList<FieldView> Fields, string SaveLabel, string Handler)
{
    /// <summary>Builds the sections of a screen: each has a heading, a save button and a handler of its own; two sections never share a handler (04 2.12.1.5).</summary>
    public static IReadOnlyList<FormSectionView> Sections(IEnumerable<FormSectionView> sections)
    {
        ArgumentNullException.ThrowIfNull(sections);
        List<FormSectionView> list = [.. sections];
        foreach (FormSectionView section in list)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(section.Id);
            ArgumentException.ThrowIfNullOrWhiteSpace(section.Heading);
            ArgumentException.ThrowIfNullOrWhiteSpace(section.SaveLabel);
            ArgumentException.ThrowIfNullOrWhiteSpace(section.Handler);
        }

        if (list.Select(s => s.Handler).Distinct(StringComparer.Ordinal).Count() != list.Count)
        {
            throw new ArgumentException("Every section has its own handler: a shared one would save the sections together.", nameof(sections));
        }

        return list;
    }
}
