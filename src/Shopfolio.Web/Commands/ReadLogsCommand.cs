using System.Net;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.LogRead;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Membership.Contracts;

namespace Shopfolio.Commands;

/// <summary>
/// <c>read-logs</c> (05 8.5.5, 9.3; 09 13.2 items 3, 4, 5; MK-66): reads the sign-in record, the system logs or the disposal record by source, by a range of Turkey days and by one filter,
/// and writes the result as a CSV file in the temporary directory outside the logs and the backup (<see cref="LogExportFile"/>); the panel has no screen of these sources (05 9.3.1).
/// The IP and the account's e-mail are personal data: <c>--filter ip</c> and <c>--filter email</c> take no value - the value is read from the terminal only, and an IP or an e-mail given
/// as an argument is a usage error whose message does not repeat it. The e-mail is turned into the account's internal id (the system logs carry ids, not e-mails); the sign-in record is
/// read by the e-mail's normalized form, the way it keeps it (06 4.44). Before anything is read the command writes its line to its own log file and waits until it is on the disk
/// (05 9.3.4; <see cref="AuditLine.ReadLogs"/>); the filter's value is not in the line, only its kind. The encrypted delivery of the file and its deletion are DEPLOY_RUNBOOK steps.
/// </summary>
internal sealed partial class ReadLogsCommand : IAdminCommand
{
    public const string CommandName = "read-logs";

    public const string SourceOption = "source";

    public const string FromOption = "from";

    public const string ToOption = "to";

    public const string FilterOption = "filter";

    public string Name => CommandName;

    public string Description => "Giriş kaydını, sistem kayıtlarını ya da imha kaydını kaynak, tarih aralığı ve süzgeçle okur; sonucu geçici dizinde CSV dosyasına yazar.";

    public IReadOnlyList<CommandOption> Options { get; } =
    [
        new CommandOption(SourceOption, "login (giriş kaydı), system (sistem kayıtları) ya da disposal (imha kaydı).", Required: true),
        new CommandOption(FromOption, "Aralığın ilk günü (2026-10-29); günün başından başlar.", Required: true, CommandOptionKind.Date),
        new CommandOption(ToOption, "Aralığın son günü (2026-10-29); gün dahildir.", Required: true, CommandOptionKind.Date),
        new CommandOption(
            FilterOption,
            "Süzgeç: event:<olay türü>, request:<istek kimliği>, account:<hesabın iç kimliği>, ip ya da email (IP ve e-posta değersiz verilir, terminalden okunur).",
            Required: false),
    ];

    public async Task<CommandExit> ExecuteAsync(CommandInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        ITerminal terminal = input.Terminal;

        LogSource? source = input.Text(SourceOption) switch
        {
            "login" => LogSource.Login,
            "system" => LogSource.System,
            "disposal" => LogSource.Disposal,
            _ => null,
        };
        if (source is not { } chosenSource)
        {
            terminal.WriteErrorLine($"--{SourceOption} seçeneğinin değeri geçersiz: login, system ya da disposal bekleniyor.");
            return CommandExit.UsageError;
        }

        DateOnly from = input.Date(FromOption)!.Value;
        DateOnly to = input.Date(ToOption)!.Value;
        if (from > to)
        {
            terminal.WriteErrorLine($"--{FromOption} günü --{ToOption} gününden sonra olamaz.");
            return CommandExit.UsageError;
        }

        if (ParseFilter(input.Text(FilterOption), chosenSource) is not { } filter)
        {
            // The message names the forms only; the given value is not repeated (09 13.2 item 2): it may be an IP or an e-mail.
            terminal.WriteErrorLine(FilterUsage(chosenSource));
            return CommandExit.UsageError;
        }

        Guid accountId = filter.Kind == LogFilterKind.Account ? Guid.Parse(filter.Value!) : Guid.Empty;
        string? value = filter.Value;
        IAuditLineWriter writer = input.Services.GetRequiredService<IAuditLineWriter>();
        await using AsyncServiceScope scope = input.Services.CreateAsyncScope();

        if (filter.Kind is LogFilterKind.Ip or LogFilterKind.Email)
        {
            TerminalPrompt prompt = new(terminal);

            // Nothing is read, written or opened before this look (09 13.2 item 4).
            if (!prompt.IsInteractive)
            {
                terminal.WriteErrorLine("IP ve hesabın e-postası yalnız terminalden okunur; girdi yönlendirilmiş. Komutu etkileşimli bir terminalde çalıştırın (docker compose run, -T olmadan).");
                return CommandExit.UsageError;
            }

            if (filter.Kind == LogFilterKind.Ip)
            {
                if (!IPAddress.TryParse(prompt.Ask("IP: ").Trim(), out IPAddress? ip))
                {
                    terminal.WriteErrorLine("Girilen değer bir IP değil.");
                    return CommandExit.Refused;
                }

                value = ip.ToString();
            }
            else
            {
                string email = prompt.Ask("Hesabın e-postası: ").Trim();
                accountId = await FindAccountIdAsync(scope.ServiceProvider, email, ct) ?? Guid.Empty;
                if (chosenSource == LogSource.System)
                {
                    if (accountId == Guid.Empty)
                    {
                        writer.Write(AuditLine.ReadLogs(CommandName, Guid.Empty, chosenSource, filter.Kind, from, to));
                        terminal.WriteErrorLine("Bu e-postayla bir hesap yok; sistem kayıtları hesabın iç kimliğiyle süzülür.");
                        return CommandExit.Refused;
                    }

                    // The system logs carry the account's internal id, never its e-mail (05 9.1.3).
                    filter = filter with { Kind = LogFilterKind.Account };
                    value = accountId.ToString("D");
                }
                else
                {
                    // The sign-in record keeps the entered e-mail in its normalized form (06 4.44), an unknown address included.
                    value = TextNormalizer.NormalizeIdentity(email);
                }
            }
        }

        // The line before anything is read, on the disk when Write returns (05 9.3.4; 09 13.2 item 5). A line that cannot be written goes up to the dispatcher and nothing is read.
        writer.Write(AuditLine.ReadLogs(CommandName, accountId, chosenSource, filter.Kind, from, to));

        LogReadRequest request = new(chosenSource, from, to, filter.Kind, value);
        LogReader reader = scope.ServiceProvider.GetRequiredService<LogReader>();
        (string path, FileStream stream) = LogExportFile.Create(LogExportFile.DirectoryOf(input.Services.GetRequiredService<IConfiguration>()));
        bool written = false;
        LogReadSummary summary;
        try
        {
            await using (stream)
            {
                await using StreamWriter output = new(stream, LogCsv.Encoding);
                summary = await reader.WriteCsvAsync(request, output, ct);
            }

            written = true;
        }
        finally
        {
            if (!written)
            {
                // A half-written file is not left in the directory: its rows are personal data (05 9.3.3).
                File.Delete(path);
            }
        }

        terminal.WriteLine($"Dosya: {path}");
        terminal.WriteLine($"Satır: {summary.Rows}");
        if (summary.UnreadableLines > 0)
        {
            terminal.WriteLine($"Okunamayan satır (JSON ya da zaman yok): {summary.UnreadableLines}");
        }

        terminal.WriteLine("Dosyayı şifreli iletin ve iletimden sonra silin (DEPLOY_RUNBOOK).");
        return CommandExit.Success;
    }

    /// <summary>
    /// The filter of the command line, checked against the source (09 13.2 item 3): null when the form is unknown, when the source has no such filter, when <c>ip</c> or <c>email</c> comes
    /// with a value, or when an event type or a request id looks like an e-mail or an IP - personal data never comes as an argument (05 8.5.1). The account id is checked as a GUID.
    /// </summary>
    internal static ReadFilter? ParseFilter(string? text, LogSource source)
    {
        if (text is null)
        {
            return new ReadFilter(LogFilterKind.None, null);
        }

        int colon = text.IndexOf(':', StringComparison.Ordinal);
        string kind = colon < 0 ? text : text[..colon];
        string? value = colon < 0 ? null : text[(colon + 1)..];
        ReadFilter? filter = kind switch
        {
            "ip" when value is null => new ReadFilter(LogFilterKind.Ip, null),
            "email" when value is null => new ReadFilter(LogFilterKind.Email, null),
            "event" when IsPlainToken(value) => new ReadFilter(LogFilterKind.Event, value),
            "request" when IsPlainToken(value) => new ReadFilter(LogFilterKind.Request, value),
            "account" when Guid.TryParseExact(value, "D", out Guid id) => new ReadFilter(LogFilterKind.Account, id.ToString("D")),
            _ => null,
        };
        return filter is not null && Accepts(source, filter) ? filter : null;
    }

    // Which filter each source has (05 9.3.1): the sign-in record carries the IP, the e-mail and the outcome; the disposal record its record type; the system logs every one but the
    // e-mail itself, which is turned into the account id.
    private static bool Accepts(LogSource source, ReadFilter filter) => source switch
    {
        LogSource.Login => filter.Kind switch
        {
            LogFilterKind.None or LogFilterKind.Ip or LogFilterKind.Email => true,
            LogFilterKind.Event => Enum.GetNames<LoginOutcome>().Contains(filter.Value, StringComparer.Ordinal),
            _ => false,
        },
        LogSource.Disposal => filter.Kind switch
        {
            LogFilterKind.None => true,
            LogFilterKind.Event => Enum.GetNames<DisposalRecordType>().Contains(filter.Value, StringComparer.Ordinal),
            _ => false,
        },
        _ => true,
    };

    // An event type or a request id: letters, digits, '-' and '_' only - no '@', '.' or ':' - and not a value that reads as an IP (a bare number does).
    private static bool IsPlainToken(string? value) => value is not null && PlainToken().IsMatch(value) && !IPAddress.TryParse(value, out _);

    private static string FilterUsage(LogSource source) => source switch
    {
        LogSource.Login => $"--{FilterOption} seçeneğinin değeri geçersiz: giriş kaydında event:Succeeded, event:Failed, ip ya da email bekleniyor; IP ve e-posta argümanla verilmez, terminalden okunur.",
        LogSource.Disposal => $"--{FilterOption} seçeneğinin değeri geçersiz: imha kaydında event:<kayıt türü> bekleniyor ({string.Join(", ", Enum.GetNames<DisposalRecordType>())}).",
        _ => $"--{FilterOption} seçeneğinin değeri geçersiz: event:<olay türü>, request:<istek kimliği>, account:<hesabın iç kimliği>, ip ya da email bekleniyor; IP ve e-posta argümanla verilmez, terminalden okunur.",
    };

    // The administrator account first, then the customer account (the two account tables are separate, 06 4.x); null when neither has the address.
    private static async Task<Guid?> FindAccountIdAsync(IServiceProvider services, string email, CancellationToken ct)
    {
        if (email.Length == 0)
        {
            return null;
        }

        if (await services.GetRequiredService<IAdministrationModule>().FindAdminIdAsync(email, ct) is { } adminId)
        {
            return adminId;
        }

        MemberRecord member = await services.GetRequiredService<IMembershipModule>().FindMemberRecordAsync(email, ct);
        return member.IsAccount ? member.CustomerId : null;
    }

    [GeneratedRegex(@"^[A-Za-z0-9_\-]{1,100}$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex PlainToken();

    /// <summary>The kind of the filter and its value as the reader compares it; no value for <c>ip</c> and <c>email</c> until the terminal gives it.</summary>
    internal sealed record ReadFilter(LogFilterKind Kind, string? Value);
}
