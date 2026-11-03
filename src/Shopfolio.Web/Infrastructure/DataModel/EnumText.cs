using System.Collections.Frozen;

using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Shopfolio.Infrastructure.DataModel;

/// <summary>
/// The one place where an enum value becomes its stored text and back (06 2.9.1; 09 10.1 item 3). The stored text is the code name of
/// the member; the numeric value means nothing. An undefined value is never written, and a text that is not a code name of the enum is
/// never read as a value: both fail loudly (09 12 item 3).
/// </summary>
internal static class EnumText<TEnum>
    where TEnum : struct, Enum
{
    private static readonly FrozenDictionary<string, TEnum> ByName =
        Enum.GetValues<TEnum>().ToFrozenDictionary(value => value.ToString(), StringComparer.Ordinal);

    /// <summary>The code names of the enum, in declaration order.</summary>
    public static IReadOnlyList<string> Names { get; } = Enum.GetNames<TEnum>();

    public static string ToText(TEnum value)
    {
        string text = value.ToString();
        if (!ByName.ContainsKey(text))
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, $"The value is not a member of {typeof(TEnum).Name} and cannot be stored.");
        }

        return text;
    }

    public static TEnum FromText(string text)
    {
        if (text is not null && ByName.TryGetValue(text, out TEnum value))
        {
            return value;
        }

        throw new InvalidOperationException($"The stored text '{text}' is not a code name of {typeof(TEnum).Name}.");
    }
}

/// <summary>The value converter every enum property gets from the context's convention (09 10.1 item 3): code name as text.</summary>
internal sealed class EnumTextConverter<TEnum> : ValueConverter<TEnum, string>
    where TEnum : struct, Enum
{
    public EnumTextConverter()
        : base(value => EnumText<TEnum>.ToText(value), text => EnumText<TEnum>.FromText(text))
    {
    }
}
