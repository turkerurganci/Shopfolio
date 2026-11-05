using System.Text.Json;

namespace Shopfolio.UnitTests.Support;

/// <summary>One common field of the application log line: its name, its JSON type, and whether it sits in an element of the <c>fields</c> array.</summary>
internal sealed record CommonLogField(string Name, JsonValueKind Kind, bool InFieldsElement = false);

/// <summary>
/// The common fields of a log line with their types (T231 criterion 7; 09 18.2 items 1, 7). The names are the first column of the table in 09 18.2
/// item 1 (<c>LoggingConsistencyTests</c> keeps the two equal) plus <c>fields</c>, the array of the field validation rejection that the table's
/// description column names; the line is checked field by field with <see cref="Problems"/>.
/// </summary>
internal static class CommonLogFields
{
    /// <summary>The array of a field validation rejection (09 18.2 item 4); it is named in the description of the <c>field</c> row, not in the first column.</summary>
    public const string FieldsArray = "fields";

    public static readonly IReadOnlyList<CommonLogField> All =
    [
        new("time", JsonValueKind.String),
        new("level", JsonValueKind.String),
        new("category", JsonValueKind.String),
        new("event", JsonValueKind.String),
        new("requestId", JsonValueKind.String),
        new("method", JsonValueKind.String),
        new("path", JsonValueKind.String),
        new("accountType", JsonValueKind.String),
        new("accountId", JsonValueKind.String),
        new("exceptionType", JsonValueKind.String),
        new("innerExceptionTypes", JsonValueKind.Array),
        new("sqlState", JsonValueKind.String),
        new("stackTrace", JsonValueKind.String),
        new("state", JsonValueKind.String),
        new("operation", JsonValueKind.String),
        new("field", JsonValueKind.String, InFieldsElement: true),
        new("code", JsonValueKind.String),
        new(FieldsArray, JsonValueKind.Array),
        new("message", JsonValueKind.String),
    ];

    /// <summary>What is wrong with the line against the table: a field that is missing or has another type. Empty when the line is right.</summary>
    public static IReadOnlyList<string> Problems(JsonElement line)
    {
        List<string> problems = [];
        foreach (CommonLogField field in All)
        {
            JsonElement holder = line;
            if (field.InFieldsElement)
            {
                if (!line.TryGetProperty(FieldsArray, out JsonElement array) || array.ValueKind != JsonValueKind.Array || array.GetArrayLength() == 0)
                {
                    problems.Add($"{field.Name}: the {FieldsArray} array is missing or empty");
                    continue;
                }

                holder = array[0];
            }

            if (!holder.TryGetProperty(field.Name, out JsonElement value))
            {
                problems.Add($"{field.Name}: missing");
            }
            else if (value.ValueKind != field.Kind)
            {
                problems.Add($"{field.Name}: {value.ValueKind}, expected {field.Kind}");
            }
        }

        return problems;
    }
}
