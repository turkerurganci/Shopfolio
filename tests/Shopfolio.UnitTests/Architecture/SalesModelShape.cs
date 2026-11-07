using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The shape scan of the sales model (T332; 02 3.2.1, 3.3.5, 3.3.6; 10 KD-6, KD-35; 11 T332 criteria 1, 7, 8): the application has no quote or request line, no "ask for the price"
/// state, no appointment or booking model, no minimum order quantity of a product, and no review, rating, star or recommendation. The scan reads names, not texts: every type, member,
/// enum value and parameter of the application assembly is cut into its words (camel case, snake case, digits dropped) and a forbidden phrase is a run of those words in a name, so a
/// property, a method, a nested type, a field, a parameter and an enum member are all found in whatever spelling. The phrases are the closed list below.
/// </summary>
internal static partial class SalesModelShape
{
    /// <summary>The forbidden phrases, each a run of lower-case words; one phrase per line of the documents' bans.</summary>
    public static readonly string[][] ForbiddenPhrases =
    [
        ["quote"], ["quotes"], ["quotation"], ["quotations"], ["rfq"],
        ["price", "on", "request"], ["price", "upon", "request"], ["ask", "price"], ["ask", "for", "price"], ["request", "price"], ["request", "quote"], ["inquiry"], ["enquiry"],
        ["appointment"], ["appointments"], ["booking"], ["bookings"], ["time", "slot"], ["table", "reservation"], ["service", "reservation"],
        ["min", "quantity"], ["minimum", "quantity"], ["min", "qty"], ["minimum", "qty"], ["min", "order", "quantity"], ["minimum", "order", "quantity"], ["min", "order", "qty"], ["min", "units"], ["minimum", "units"], ["min", "pieces"],
        ["review"], ["reviews"], ["rating"], ["ratings"], ["star", "rating"], ["stars"], ["recommendation"], ["recommendations"], ["recommended", "products"],
        ["related", "products"], ["similar", "products"], ["also", "bought"], ["customers", "also"], ["people", "also"],
    ];

    /// <summary>The names of the application assembly: types, members, enum values and parameters, with the owner so that a finding points at its place.</summary>
    public static IEnumerable<(string Owner, string Name)> NamesOf(Assembly assembly)
    {
        const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        foreach (Type type in assembly.GetTypes().Where(t => t.GetCustomAttribute<CompilerGeneratedAttribute>() is null))
        {
            yield return (type.FullName ?? type.Name, type.Name);
            foreach (MemberInfo member in type.GetMembers(All).Where(m => m.GetCustomAttribute<CompilerGeneratedAttribute>() is null))
            {
                yield return ($"{type.FullName}.{member.Name}", member.Name);
                IEnumerable<ParameterInfo> parameters = member switch
                {
                    MethodBase method => method.GetParameters(),
                    PropertyInfo property => property.GetIndexParameters(),
                    _ => [],
                };
                foreach (ParameterInfo parameter in parameters.Where(p => p.Name is not null))
                {
                    yield return ($"{type.FullName}.{member.Name}({parameter.Name})", parameter.Name!);
                }
            }
        }
    }

    /// <summary>The forbidden phrases a name contains, one entry per phrase found.</summary>
    public static IReadOnlyList<string> FindingsIn(string name)
    {
        List<string> words = Words(name);
        List<string> findings = [];
        foreach (string[] phrase in ForbiddenPhrases)
        {
            for (int start = 0; start + phrase.Length <= words.Count; start++)
            {
                if (words.Skip(start).Take(phrase.Length).SequenceEqual(phrase, StringComparer.Ordinal))
                {
                    findings.Add(string.Join(' ', phrase));
                    break;
                }
            }
        }

        return findings;
    }

    /// <summary>The words of a name: split at underscores, hyphens, spaces and camel-case boundaries (an acronym stays one word), digits dropped, lower case.</summary>
    public static List<string> Words(string name)
    {
        List<string> words = [];
        foreach (string part in NonLetters().Split(name).Where(p => p.Length > 0))
        {
            foreach (Match match in WordInPart().Matches(part))
            {
                words.Add(match.Value.ToLowerInvariant());
            }
        }

        return words;
    }

    [GeneratedRegex("[^A-Za-z]+", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex NonLetters();

    // An upper-case run that is not followed by a lower-case letter (an acronym), an upper-case letter with its lower-case tail, or a lower-case run.
    [GeneratedRegex("[A-Z]+(?![a-z])|[A-Z]?[a-z]+", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex WordInPart();
}
