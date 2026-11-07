using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scans of the module skeleton (T218 criteria 6 and 9; 09 6 item 3; 05 3.4.3, 3.4.6): <c>Program.cs</c> registers each of the nine
/// modules with one plain call of its <c>Add&lt;Module&gt;Module</c> extension - none missing, none twice, none chained or wrapped - and no
/// application code has an in-process event publisher (an event bus, a mediator, a domain event): the external side effect of a transition is a
/// job row written in the same transaction. Comment lines are dropped; the scan is line based.
/// </summary>
internal static partial class ModuleSourceScan
{
    public const string Missing = "a module has no registration call";

    public const string Twice = "a module is registered more than once";

    public const string NotPlain = "a registration call is chained, wrapped or has arguments (it is one plain statement per module)";

    public const string EventPublisher = "an in-process event publisher (the application has no event bus; side effects are job rows)";

    public static readonly string[] Modules =
    [
        "Catalog", "Ordering", "Payment", "AfterSales", "Membership", "CorporateContent", "Notifications", "Administration", "Settings",
    ];

    /// <summary>Scans <c>Program.cs</c>; the line is 0 for a module-level finding (missing) and the line number otherwise.</summary>
    public static IReadOnlyList<(string Rule, string Module, int Line)> ScanProgram(string text)
    {
        List<(string Rule, string Module, int Line)> findings = [];
        string[] lines = text.ReplaceLineEndings("\n").Split('\n');
        Dictionary<string, int> counts = Modules.ToDictionary(m => m, _ => 0, StringComparer.Ordinal);
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (IsComment(line))
            {
                continue;
            }

            MatchCollection calls = RegistrationCall().Matches(line);
            foreach (Match call in calls)
            {
                string module = call.Groups[1].Value;
                if (counts.TryGetValue(module, out int count))
                {
                    counts[module] = count + 1;
                    if (count == 1)
                    {
                        findings.Add((Twice, module, i + 1));
                    }
                }

                if (calls.Count > 1 || !PlainStatement().IsMatch(line))
                {
                    findings.Add((NotPlain, module, i + 1));
                }
            }
        }

        foreach ((string module, int count) in counts)
        {
            if (count == 0)
            {
                findings.Add((Missing, module, 0));
            }
        }

        return findings;
    }

    /// <summary>Scans one application source file for an event publisher.</summary>
    public static IReadOnlyList<(string Rule, int Line)> ScanEventPublisher(string text)
    {
        List<(string Rule, int Line)> findings = [];
        string[] lines = text.ReplaceLineEndings("\n").Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (!IsComment(lines[i]) && EventPublisherName().IsMatch(lines[i]))
            {
                findings.Add((EventPublisher, i + 1));
            }
        }

        return findings;
    }

    private static bool IsComment(string line)
    {
        string trimmed = line.TrimStart();
        return trimmed.StartsWith("//", StringComparison.Ordinal) || trimmed.StartsWith('*') || trimmed.StartsWith("/*", StringComparison.Ordinal);
    }

    [GeneratedRegex(@"\bAdd([A-Za-z]+)Module\s*\(")]
    private static partial Regex RegistrationCall();

    [GeneratedRegex(@"^\s*builder\.Services\.Add[A-Za-z]+Module\(\);\s*$")]
    private static partial Regex PlainStatement();

    [GeneratedRegex(@"\b(IEventBus|IEventPublisher|IEventHandler|IDomainEvent|DomainEvent|DomainEvents|AddDomainEvent|INotification|IMediator|INotificationHandler|EventAggregator|MediatR)\b")]
    private static partial Regex EventPublisherName();
}
