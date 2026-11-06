using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shopfolio.Infrastructure.Binding;

/// <summary>
/// The JSON of the machine endpoints' small bodies - the multipart upload's start, chunk and status (07 2.9.6, 4.31.2; K-1171 (1), K-1172 (5)): the field
/// names are camelCase, and a byte count is a JSON number both ways - it is written as a number and a number written as a text (<c>"sizeBytes": "12"</c>)
/// is not read. A machine body carries no date (07 2.9.6); the model rule keeps the date and time types out of a machine model (<c>MachineDate</c>) and
/// a byte count in an integer (<c>ByteCountNotNumber</c>). The endpoint that reads or writes such a body uses <see cref="Options"/> and no options of
/// its own. The payment provider's notification has the format of 08 and is not read with these options.
/// </summary>
public static class MachineJson
{
    /// <summary>The options of a machine body: the web defaults (camelCase, case-insensitive names) with the strict number handling.</summary>
    public static JsonSerializerOptions Options { get; } = Create();

    private static JsonSerializerOptions Create()
    {
        JsonSerializerOptions options = new(JsonSerializerDefaults.Web) { NumberHandling = JsonNumberHandling.Strict };
        options.MakeReadOnly(populateMissingResolver: true);
        return options;
    }
}
