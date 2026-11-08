namespace Shopfolio.Commands;

/// <summary>
/// The exit codes of a management command (09 13.2 item 2). A usage error (2) is a mistake of the command line or of the way the command is started (an unknown name, a missing option,
/// a redirected input); a refusal (3) is a value or a condition the work does not accept (an empty name, a weak password, an address that is taken).
/// </summary>
internal enum CommandExit
{
    Success = 0,
    UnexpectedError = 1,
    UsageError = 2,
    Refused = 3,
}

/// <summary>The kind of an option's value (09 13.2 item 3). Only a non-personal value ever comes as an option (05 8.5.1).</summary>
internal enum CommandOptionKind
{
    /// <summary>Plain text.</summary>
    Text,

    /// <summary>
    /// An instant in ISO 8601 with its offset (<c>2026-10-29T14:30:00Z</c> or <c>+03:00</c>), read without the culture and kept as UTC (09 7.1 item 2). A date alone or an instant with no
    /// offset is a usage error: it would depend on the server's time zone (09 13.2 item 3).
    /// </summary>
    Moment,
}

/// <summary>One option of a command (09 13.2 item 2): its name without the dashes, its one-line description, whether it must be given and its kind.</summary>
internal sealed record CommandOption(string Name, string Description, bool Required, CommandOptionKind Kind = CommandOptionKind.Text);

/// <summary>
/// A management command (05 8.5; 09 13.2 item 2): one class each, listed in <see cref="AdminCommandCatalog"/>. A command calls the modules' Contracts interfaces through the services
/// it is given and never touches the database context itself.
/// </summary>
internal interface IAdminCommand
{
    /// <summary>The kebab-case name in English (<c>create-admin</c>).</summary>
    string Name { get; }

    /// <summary>One line in Turkish.</summary>
    string Description { get; }

    IReadOnlyList<CommandOption> Options { get; }

    Task<CommandExit> ExecuteAsync(CommandInput input, CancellationToken ct);
}

/// <summary>
/// What a command receives: the values of its options (already checked against their kinds), the terminal and the scope's services. The values are non-personal by rule; a personal value
/// is asked for on the terminal only (09 13.2 item 4).
/// </summary>
internal sealed class CommandInput
{
    private readonly IReadOnlyDictionary<string, object> _values;

    public CommandInput(IReadOnlyDictionary<string, object> values, ITerminal terminal, IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(values);
        ArgumentNullException.ThrowIfNull(terminal);
        ArgumentNullException.ThrowIfNull(services);
        _values = values;
        Terminal = terminal;
        Services = services;
    }

    public ITerminal Terminal { get; }

    public IServiceProvider Services { get; }

    public bool Has(string option) => _values.ContainsKey(option);

    /// <summary>The text of a <see cref="CommandOptionKind.Text"/> option; null when it was not given.</summary>
    public string? Text(string option) => _values.TryGetValue(option, out object? value) ? (string)value : null;

    /// <summary>The instant of a <see cref="CommandOptionKind.Moment"/> option in UTC; null when it was not given.</summary>
    public DateTimeOffset? Moment(string option) => _values.TryGetValue(option, out object? value) ? (DateTimeOffset)value : null;
}
