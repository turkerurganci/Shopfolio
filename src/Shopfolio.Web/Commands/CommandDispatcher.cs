using System.Globalization;
using System.Text.RegularExpressions;

using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure.Integrations;

namespace Shopfolio.Commands;

/// <summary>
/// The dispatcher of the management commands (09 13.2 item 2): finds the command by its name, reads its options, runs it and turns the outcome into an exit code. A usage error
/// message (an unknown command or option, a missing or malformed value) names commands and options and <b>never repeats a value the operator typed</b> - a mistyped argument may be a
/// password pasted into the wrong place. The top-level catch ends an unexpected error with exit code 1 and writes only the type of the error (09 13.2 item 2; the fourth place of the
/// broad catch, 09 8.1 item 6): the message of an exception may carry a connection string or a personal value.
/// </summary>
internal sealed partial class CommandDispatcher
{
    private const string HelpOption = "--help";

    private readonly IReadOnlyList<IAdminCommand> _commands;
    private readonly ITerminal _terminal;
    private readonly Func<ServiceProvider> _services;

    /// <summary>
    /// <paramref name="services"/> makes the service provider of a run; it is called only after the command and its options have been read, so a usage error needs no settings
    /// and no provider, and the provider is disposed when the command ends.
    /// </summary>
    public CommandDispatcher(IReadOnlyList<IAdminCommand> commands, ITerminal terminal, Func<ServiceProvider> services)
    {
        ArgumentNullException.ThrowIfNull(commands);
        ArgumentNullException.ThrowIfNull(terminal);
        ArgumentNullException.ThrowIfNull(services);
        _commands = commands;
        _terminal = terminal;
        _services = services;
    }

    /// <summary>Runs the command named by the first argument; <paramref name="arguments"/> are the arguments after <c>admin</c>.</summary>
    public async Task<int> ExecuteAsync(string[] arguments, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        try
        {
            return (int)await DispatchAsync(arguments, ct);
        }
#pragma warning disable CA1031 // 09 13.2 item 2, 8.1 item 6: the dispatcher's top-level catch is one of the places of the wide catch.
        catch (Exception exception)
        {
            _terminal.WriteErrorLine($"Beklenmeyen hata: {exception.GetType().Name}");
            return (int)CommandExit.UnexpectedError;
        }
#pragma warning restore CA1031
    }

    private async Task<CommandExit> DispatchAsync(string[] arguments, CancellationToken ct)
    {
        if (arguments.Length == 0)
        {
            _terminal.WriteErrorLine("Komut adı verilmedi. Komut listesi için: admin --help");
            return CommandExit.UsageError;
        }

        if (arguments[0] == HelpOption)
        {
            WriteCommandList();
            return arguments.Length == 1 ? CommandExit.Success : CommandExit.UsageError;
        }

        IAdminCommand? command = _commands.FirstOrDefault(c => string.Equals(c.Name, arguments[0], StringComparison.Ordinal));
        if (command is null)
        {
            _terminal.WriteErrorLine("Bilinmeyen komut. Komut listesi için: admin --help");
            return CommandExit.UsageError;
        }

        string[] optionArguments = arguments[1..];
        if (optionArguments.Length == 1 && optionArguments[0] == HelpOption)
        {
            WriteCommandHelp(command);
            return CommandExit.Success;
        }

        Dictionary<string, object>? values = ReadOptions(command, optionArguments);
        if (values is null)
        {
            return CommandExit.UsageError;
        }

        await using ServiceProvider services = _services();
        return await command.ExecuteAsync(new CommandInput(values, _terminal, services), ct);
    }

    private Dictionary<string, object>? ReadOptions(IAdminCommand command, string[] arguments)
    {
        Dictionary<string, object> values = new(StringComparer.Ordinal);
        for (int i = 0; i < arguments.Length; i++)
        {
            string argument = arguments[i];
            CommandOption? option = argument.StartsWith("--", StringComparison.Ordinal)
                ? command.Options.FirstOrDefault(o => string.Equals("--" + o.Name, argument, StringComparison.Ordinal))
                : null;
            if (option is null)
            {
                _terminal.WriteErrorLine($"Bilinmeyen seçenek ya da yeri olmayan argüman. Seçenekler için: admin {command.Name} --help");
                return null;
            }

            if (values.ContainsKey(option.Name))
            {
                _terminal.WriteErrorLine($"--{option.Name} seçeneği bir kez verilebilir.");
                return null;
            }

            if (i + 1 >= arguments.Length)
            {
                _terminal.WriteErrorLine($"--{option.Name} seçeneğinin değeri yok.");
                return null;
            }

            i++;
            object? value = Parse(option, arguments[i]);
            if (value is null)
            {
                _terminal.WriteErrorLine($"--{option.Name} seçeneğinin değeri geçersiz: {Expectation(option.Kind)}");
                return null;
            }

            values[option.Name] = value;
        }

        CommandOption? missing = command.Options.FirstOrDefault(o => o.Required && !values.ContainsKey(o.Name));
        if (missing is not null)
        {
            _terminal.WriteErrorLine($"Zorunlu seçenek eksik: --{missing.Name}");
            return null;
        }

        return values;
    }

    private static object? Parse(CommandOption option, string text)
    {
        return option.Kind switch
        {
            CommandOptionKind.Text => text.Length == 0 ? null : text,
            CommandOptionKind.Moment => ParseMoment(text) is { } moment ? moment : null,
            CommandOptionKind.Date => ParseDate(text) is { } date ? date : null,
            CommandOptionKind.Amount => ParseAmountInKurus(text) is { } kurus ? kurus : null,
            _ => null,
        };
    }

    // A Turkey day, yyyy-MM-dd and nothing else (09 13.2 item 3): the exact format keeps an instant or a culture's date out.
    private static DateOnly? ParseDate(string text)
    {
        return DatePattern().IsMatch(text) && DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date) ? date : null;
    }

    // Dotted decimal text with at most two places, turned into whole kuruş (09 13.2 item 3; 09 14.2): the pattern refuses a comma, a sign, a third place and more than ten whole
    // digits before the number is read, so the kuruş always fit.
    private static long? ParseAmountInKurus(string text)
    {
        if (!AmountPattern().IsMatch(text)
            || !decimal.TryParse(text, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out decimal amount))
        {
            return null;
        }

        return PaymentAmounts.ToKurus(amount);
    }

    // ISO 8601 with an explicit offset (09 13.2 item 3): the pattern is checked first because the parser alone accepts a date or an instant with no offset
    // and then reads it in the server's time zone.
    private static DateTimeOffset? ParseMoment(string text)
    {
        if (!MomentPattern().IsMatch(text)
            || !DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTimeOffset moment))
        {
            return null;
        }

        return moment.ToUniversalTime();
    }

    private static string Expectation(CommandOptionKind kind)
    {
        return kind switch
        {
            CommandOptionKind.Moment => "ofsetli ISO 8601 an bekleniyor (yıl-ay-günTsaat:dakika:saniye ve ardından Z ya da +ss:dd)",
            CommandOptionKind.Date => "ISO 8601 tarih bekleniyor (yıl-ay-gün)",
            CommandOptionKind.Amount => "noktalı ondalık tutar bekleniyor (en çok iki hane, ör. 1250.50)",
            _ => "boş olmayan metin bekleniyor",
        };
    }

    private void WriteCommandList()
    {
        _terminal.WriteLine("Kullanım: admin <komut> [--seçenek değer ...]");
        _terminal.WriteLine("Komutlar:");
        foreach (IAdminCommand command in _commands)
        {
            _terminal.WriteLine($"  {command.Name}  {command.Description}");
            foreach (CommandOption option in command.Options)
            {
                _terminal.WriteLine($"      --{option.Name}  {OptionText(option)}");
            }
        }
    }

    private void WriteCommandHelp(IAdminCommand command)
    {
        _terminal.WriteLine($"admin {command.Name}  {command.Description}");
        foreach (CommandOption option in command.Options)
        {
            _terminal.WriteLine($"  --{option.Name}  {OptionText(option)}");
        }
    }

    private static string OptionText(CommandOption option) => option.Required ? $"{option.Description} (zorunlu)" : option.Description;

    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}(:\d{2}(\.\d{1,7})?)?(Z|[+-]\d{2}:\d{2})$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex MomentPattern();

    // ASCII digits only: \d also matches the digits of other scripts.
    [GeneratedRegex(@"^[0-9]{4}-[0-9]{2}-[0-9]{2}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex DatePattern();

    [GeneratedRegex(@"^[0-9]{1,10}(\.[0-9]{1,2})?$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex AmountPattern();
}
