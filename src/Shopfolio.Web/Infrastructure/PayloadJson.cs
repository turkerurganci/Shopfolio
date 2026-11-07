using System.Text.Json;
using System.Text.Json.Serialization;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Shopfolio.Infrastructure;

/// <summary>
/// The one serialization setting of the payload columns (09 12 item 5): the <c>jsonb</c> values that hold a typed list of values (the audit trail's details
/// are the first). The field name is the record member's name (no naming policy), an unknown field is skipped and an enum is written as its code name. The
/// mapping of a payload column uses only <see cref="Options"/>; a second set of options is not opened and the provider's own JSON mapping (<c>ToJson</c>) is
/// not used, because it does not read this setting.
/// </summary>
public static class PayloadJson
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = null,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
        Converters = { new JsonStringEnumConverter() },
    };

    /// <summary>The text of a payload value.</summary>
    public static string Write<T>(T value) => JsonSerializer.Serialize(value, Options);

    /// <summary>The value of a payload text; a field the type does not have is skipped, a field the text lacks keeps the type's default (09 12 item 5).</summary>
    public static T? Read<T>(string json) => JsonSerializer.Deserialize<T>(json, Options);

    /// <summary>
    /// Maps a property as a <c>jsonb</c> payload: a value converter in both directions through <see cref="Options"/>, and a comparer on the serialized text,
    /// so that a list changed in place is seen as a change (09 12 item 5).
    /// </summary>
    internal static PropertyBuilder<T?> HasPayloadConversion<T>(this PropertyBuilder<T?> property)
        where T : class
    {
        ArgumentNullException.ThrowIfNull(property);
        ValueConverter<T?, string?> converter = new(
            value => value == null ? null : Write(value),
            text => text == null ? null : Read<T>(text));
        ValueComparer<T?> comparer = new(
            (left, right) => Equals(left == null ? null : Write(left), right == null ? null : Write(right)),
            value => value == null ? 0 : Write(value).GetHashCode(StringComparison.Ordinal),
            value => value == null ? null : Read<T>(Write(value)));
        property.HasColumnType("jsonb").HasConversion(converter, comparer);
        return property;
    }
}
