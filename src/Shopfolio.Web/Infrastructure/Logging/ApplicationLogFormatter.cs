using System.Buffers;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

using Serilog.Events;
using Serilog.Formatting;

namespace Shopfolio.Infrastructure.Logging;

/// <summary>
/// The line format of the application log (09 18.2 item 1): one JSON object per line, field names in camelCase. It is the project's own formatter
/// (Serilog's <c>JsonFormatter</c> fixes its field names). Besides the fields it writes it never writes: the message of an exception (09 18.2 item
/// 2c), a property with a name of personal data or with the framework's request scope names (the unmasked path), and an event name that is not
/// on the closed list.
/// </summary>
internal sealed class ApplicationLogFormatter : ITextFormatter
{
    public const string TimeFormat = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";

    /// <summary>The property names the request scope of the framework brings; the unmasked path and connection identity stay out of the line.</summary>
    public static readonly IReadOnlyList<string> FrameworkScopeNames = ["RequestPath", "RequestId", "ConnectionId"];

    /// <summary>
    /// The names of personal data (09 18.2 item 2a). A property is left out when its name is one of these or ends with one (<c>CustomerEmail</c>),
    /// compared without regard to case.
    /// </summary>
    public static readonly IReadOnlyList<string> ForbiddenPropertyNames =
        ["Email", "Phone", "Address", "Password", "Token", "Key", "Secret", "Iban", "Card"];

    /// <summary>
    /// The names that contain a word of <see cref="ForbiddenPropertyNames"/> on purpose and carry no personal data (09 18.2 item 2a; D-89): the
    /// start-up refusal names the missing keys of the settings and counts them. The list is exact (whole name, any case) and short; a new entry needs
    /// a reason next to it.
    /// </summary>
    public static readonly IReadOnlyList<string> AllowedPropertyNames = ["MissingKeys"];

    /// <summary>The fields the formatter owns; no property writes under these names.</summary>
    public static readonly IReadOnlyList<string> OwnedFieldNames =
        ["time", "level", "category", "event", "message", "exceptionType", "innerExceptionTypes", "sqlState", "stackTrace", "dataKeys"];

    private const string SourceContextProperty = "SourceContext";

    private const string EventProperty = "Event";

    private static readonly JsonWriterOptions _writerOptions = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public void Format(LogEvent logEvent, TextWriter output)
    {
        ArgumentNullException.ThrowIfNull(logEvent);
        ArgumentNullException.ThrowIfNull(output);

        ArrayBufferWriter<byte> buffer = new();
        using (Utf8JsonWriter json = new(buffer, _writerOptions))
        {
            json.WriteStartObject();
            json.WriteString("time", logEvent.Timestamp.UtcDateTime.ToString(TimeFormat, CultureInfo.InvariantCulture));
            json.WriteString("level", logEvent.Level.ToString());

            if (logEvent.Properties.TryGetValue(SourceContextProperty, out LogEventPropertyValue? context) && context is ScalarValue { Value: string category })
            {
                json.WriteString("category", category);
            }

            if (logEvent.Properties.TryGetValue(EventProperty, out LogEventPropertyValue? eventName)
                && eventName is ScalarValue { Value: string name }
                && LogEvents.IsKnown(name))
            {
                json.WriteString("event", name);
            }

            IReadOnlyList<KeyValuePair<string, LogEventPropertyValue>> properties = WrittenProperties(logEvent);
            foreach (KeyValuePair<string, LogEventPropertyValue> property in properties)
            {
                json.WritePropertyName(FieldName(property.Key));
                WriteValue(json, property.Value);
            }

            json.WriteString("message", RenderMessage(logEvent, properties));

            if (logEvent.Exception is not null)
            {
                WriteException(json, ExceptionMasker.Describe(logEvent.Exception));
            }

            json.WriteEndObject();
        }

        output.Write(Encoding.UTF8.GetString(buffer.WrittenSpan));
        output.Write('\n');
    }

    /// <summary>The camelCase field name of a property name: its first letter in lower case.</summary>
    public static string FieldName(string propertyName) =>
        propertyName.Length == 0 || char.IsLower(propertyName[0]) ? propertyName : char.ToLowerInvariant(propertyName[0]) + propertyName[1..];

    /// <summary>True when a property of this name is never written.</summary>
    public static bool IsLeftOut(string propertyName)
    {
        return propertyName == SourceContextProperty
            || propertyName == EventProperty
            || FrameworkScopeNames.Contains(propertyName, StringComparer.Ordinal)
            || IsForbiddenName(propertyName)
            || OwnedFieldNames.Contains(FieldName(propertyName), StringComparer.Ordinal);
    }

    /// <summary>
    /// The one rule of the personal data names (09 18.2 item 2a, 2c; D-89): a name is forbidden when it <em>contains</em> a word of
    /// <see cref="ForbiddenPropertyNames"/> in any case (<c>PhoneNumber</c>, <c>CardNumber</c>, <c>PasswordHash</c>, <c>TokenValue</c> as well as
    /// <c>CustomerEmail</c>), unless the whole name is in <see cref="AllowedPropertyNames"/>. The source scan of the templates uses this same method.
    /// </summary>
    public static bool IsForbiddenName(string propertyName) =>
        ForbiddenPropertyNames.Any(forbidden => propertyName.Contains(forbidden, StringComparison.OrdinalIgnoreCase))
        && !AllowedPropertyNames.Contains(propertyName, StringComparer.OrdinalIgnoreCase);

    private static List<KeyValuePair<string, LogEventPropertyValue>> WrittenProperties(LogEvent logEvent) =>
        [.. logEvent.Properties.Where(property => !IsLeftOut(property.Key))];

    // A property that is left out because of its name leaves its placeholder in the text: the rendered message is the template with the
    // written properties only, so a value that is not written does not come back through the message.
    private static string RenderMessage(LogEvent logEvent, IReadOnlyList<KeyValuePair<string, LogEventPropertyValue>> written)
    {
        Dictionary<string, LogEventPropertyValue> allowed = new(StringComparer.Ordinal);
        foreach (KeyValuePair<string, LogEventPropertyValue> property in written)
        {
            allowed[property.Key] = property.Value;
        }

        if (logEvent.Properties.TryGetValue(EventProperty, out LogEventPropertyValue? eventName)
            && eventName is ScalarValue { Value: string name }
            && LogEvents.IsKnown(name))
        {
            allowed[EventProperty] = eventName;
        }

        using StringWriter text = new(CultureInfo.InvariantCulture);
        logEvent.MessageTemplate.Render(allowed, text, CultureInfo.InvariantCulture);
        return text.ToString();
    }

    private static void WriteException(Utf8JsonWriter json, ExceptionFields fields)
    {
        json.WriteString("exceptionType", fields.ExceptionType);
        json.WriteStartArray("innerExceptionTypes");
        foreach (string type in fields.InnerExceptionTypes)
        {
            json.WriteStringValue(type);
        }

        json.WriteEndArray();

        if (fields.SqlState is not null)
        {
            json.WriteString("sqlState", fields.SqlState);
        }

        if (fields.DataKeys.Count > 0)
        {
            json.WriteStartArray("dataKeys");
            foreach (string key in fields.DataKeys)
            {
                json.WriteStringValue(key);
            }

            json.WriteEndArray();
        }

        if (fields.StackTrace is not null)
        {
            json.WriteString("stackTrace", fields.StackTrace);
        }
    }

    private static void WriteValue(Utf8JsonWriter json, LogEventPropertyValue value)
    {
        switch (value)
        {
            case ScalarValue scalar:
                WriteScalar(json, scalar.Value);
                break;
            case SequenceValue sequence:
                json.WriteStartArray();
                foreach (LogEventPropertyValue element in sequence.Elements)
                {
                    WriteValue(json, element);
                }

                json.WriteEndArray();
                break;
            case StructureValue structure:
                json.WriteStartObject();
                foreach (LogEventProperty property in structure.Properties)
                {
                    if (!IsLeftOut(property.Name))
                    {
                        json.WritePropertyName(FieldName(property.Name));
                        WriteValue(json, property.Value);
                    }
                }

                json.WriteEndObject();
                break;
            case DictionaryValue dictionary:
                json.WriteStartObject();
                foreach (KeyValuePair<ScalarValue, LogEventPropertyValue> entry in dictionary.Elements)
                {
                    json.WritePropertyName(Convert.ToString(entry.Key.Value, CultureInfo.InvariantCulture) ?? string.Empty);
                    WriteValue(json, entry.Value);
                }

                json.WriteEndObject();
                break;
            default:
                json.WriteStringValue(value.ToString());
                break;
        }
    }

    private static void WriteScalar(Utf8JsonWriter json, object? value)
    {
        switch (value)
        {
            case null:
                json.WriteNullValue();
                break;
            case string text:
                json.WriteStringValue(text);
                break;
            case bool flag:
                json.WriteBooleanValue(flag);
                break;
            case int or long or short or sbyte or byte or ushort or uint or ulong:
                json.WriteRawValue(Convert.ToString(value, CultureInfo.InvariantCulture)!); // non-null: Convert.ToString of a boxed integer is never null
                break;
            case double number when double.IsFinite(number):
                json.WriteNumberValue(number);
                break;
            case float number when float.IsFinite(number):
                json.WriteNumberValue(number);
                break;
            case decimal number:
                json.WriteNumberValue(number);
                break;
            case DateTimeOffset moment:
                json.WriteStringValue(moment.UtcDateTime.ToString(TimeFormat, CultureInfo.InvariantCulture));
                break;
            default:
                json.WriteStringValue(Convert.ToString(value, CultureInfo.InvariantCulture));
                break;
        }
    }
}
