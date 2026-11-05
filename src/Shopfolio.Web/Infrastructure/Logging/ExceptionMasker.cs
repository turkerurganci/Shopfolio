using System.Collections;

using Npgsql;

namespace Shopfolio.Infrastructure.Logging;

/// <summary>What a log line says about an exception (09 18.2 item 2c): types, never the message; only the keys of the data dictionary.</summary>
internal sealed record ExceptionFields(
    string ExceptionType,
    IReadOnlyList<string> InnerExceptionTypes,
    string? SqlState,
    string? StackTrace,
    IReadOnlyList<string> DataKeys);

/// <summary>
/// The exception layer of the masking (09 18.2 item 2c; 05 10.3.6): the message of an exception, of the inner ones too, can carry personal data
/// or a parameter value and is never written; the data dictionary gives its keys only; the stack trace is the stack trace, not
/// <c>ToString()</c> (which carries the message).
/// </summary>
internal static class ExceptionMasker
{
    public static ExceptionFields Describe(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        List<string> innerTypes = [];
        string? sqlState = null;
        List<string> dataKeys = [];

        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (!ReferenceEquals(current, exception))
            {
                innerTypes.Add(TypeName(current));
            }

            sqlState ??= (current as PostgresException)?.SqlState;
            dataKeys.AddRange(Keys(current.Data));
        }

        return new ExceptionFields(TypeName(exception), innerTypes, sqlState, exception.StackTrace, dataKeys);
    }

    private static string TypeName(Exception exception) => exception.GetType().FullName ?? exception.GetType().Name;

    private static IEnumerable<string> Keys(IDictionary data)
    {
        foreach (DictionaryEntry entry in data)
        {
            // A key that is not a string is named by its type: its text form could carry a value.
            yield return entry.Key as string ?? entry.Key.GetType().Name;
        }
    }
}
