using System.Globalization;
using System.Reflection;

using Shopfolio.Infrastructure.Errors;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The one home of the password policy (09 15.1 item 5; 02 3.13.15, 8.1.6; P-17): the length is counted in text elements against
/// <see cref="ProductConstants.PasswordMinLength"/>, no upper-case letter, digit or symbol is asked for, and a password that is on the list that ships with the product
/// (<c>CommonPasswords.txt</c>, an embedded resource read once at the first use into a case-insensitive set; it reaches no outside service) is refused whatever its letter case.
/// The registration, the reset, the change at the account and the administrator command (09 13.2 item 4) all call <see cref="Validate"/>; the field rule
/// <see cref="Validation.PasswordTooWeakAttribute"/> is a thin wrapper around it. The password is never written to a record, an error message or a returned value (09 8.2, 18.2).
/// </summary>
internal static class PasswordPolicy
{
    /// <summary>The logical name of the embedded list.</summary>
    public const string ListResourceName = "Shopfolio.Infrastructure.Security.CommonPasswords.txt";

    private static readonly Lazy<HashSet<string>> _common = new(LoadList);

    /// <summary>
    /// The policy's verdict: null when the password passes, <see cref="ErrorCode.FieldRequired"/> when it is empty, <see cref="ErrorCode.PasswordTooWeak"/> when it is shorter
    /// than P-17 or is a common password. White space is part of a password and is not trimmed.
    /// </summary>
    public static ErrorCode? Validate(string? password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return ErrorCode.FieldRequired;
        }

        return IsLongEnough(password) && !IsCommon(password) ? null : ErrorCode.PasswordTooWeak;
    }

    /// <summary>True when the password has at least P-17 text elements (a combined character or an emoji with its modifiers counts once).</summary>
    public static bool IsLongEnough(string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        return new StringInfo(password).LengthInTextElements >= ProductConstants.PasswordMinLength;
    }

    /// <summary>True when the password is on the shipped list, whatever its letter case (<see cref="StringComparer.OrdinalIgnoreCase"/>).</summary>
    public static bool IsCommon(string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        return _common.Value.Contains(password);
    }

    /// <summary>The number of entries of the shipped list (a test reads it; nothing else does).</summary>
    public static int CommonCount => _common.Value.Count;

    private static HashSet<string> LoadList()
    {
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ListResourceName)
            ?? throw new InvalidOperationException($"The embedded list {ListResourceName} is missing (09 15.1 item 5).");
        using StreamReader reader = new(stream);
        HashSet<string> set = new(StringComparer.OrdinalIgnoreCase);
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            string entry = line.Trim();
            if (entry.Length > 0 && entry[0] != '#')
            {
                set.Add(entry);
            }
        }

        return set;
    }
}
