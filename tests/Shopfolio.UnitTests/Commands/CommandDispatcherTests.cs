using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Commands;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.UnitTests.Commands;

/// <summary>
/// The dispatcher of the management commands (T303; 09 13.2 items 2, 3, 7; KK2, KK3): an unknown command or option and a missing or malformed value end with exit code 2 and a message
/// that never repeats what was typed; an unexpected error ends with 1 and the error's type; <c>--help</c> lists the commands and their options.
/// </summary>
public sealed class CommandDispatcherTests
{
    // A value that stands for a pasted password: no message may contain it.
    private const string TypedValue = "Parola-Degeri-9137";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static IAdminCommand[] Commands(RecordingCommand command) => [command];

    private static CommandDispatcher Dispatcher(IReadOnlyList<IAdminCommand> commands, FakeTerminal terminal, Func<ServiceProvider>? services = null) =>
        new(commands, terminal, services ?? (() => new ServiceCollection().BuildServiceProvider()));

    [Fact]
    public async Task UnknownCommand_ExitsWithTwo_AndTheMessageDoesNotRepeatTheName()
    {
        FakeTerminal terminal = new();
        RecordingCommand command = new();

        int exit = await Dispatcher(Commands(command), terminal).RunAsync([TypedValue], Ct);

        Assert.Equal((int)CommandExit.UsageError, exit);
        Assert.DoesNotContain(TypedValue, terminal.AllOutput, StringComparison.Ordinal);
        Assert.Equal(0, command.Runs);
    }

    [Fact]
    public async Task NoCommandName_ExitsWithTwo()
    {
        FakeTerminal terminal = new();

        int exit = await Dispatcher(Commands(new RecordingCommand()), terminal).RunAsync([], Ct);

        Assert.Equal((int)CommandExit.UsageError, exit);
    }

    [Theory]
    [InlineData("--unknown")]
    [InlineData("positional")]
    [InlineData("-name")]
    public async Task UnknownOptionOrStrayArgument_ExitsWithTwo_AndTheMessageDoesNotRepeatTheArgument(string argument)
    {
        FakeTerminal terminal = new();
        RecordingCommand command = new();

        int exit = await Dispatcher(Commands(command), terminal).RunAsync(["record", argument + TypedValue, TypedValue], Ct);

        Assert.Equal((int)CommandExit.UsageError, exit);
        Assert.DoesNotContain(TypedValue, terminal.AllOutput, StringComparison.Ordinal);
        Assert.DoesNotContain(argument, terminal.AllOutput, StringComparison.Ordinal);
        Assert.Equal(0, command.Runs);
    }

    [Fact]
    public async Task MissingRequiredOption_ExitsWithTwo_AndNamesTheOption()
    {
        FakeTerminal terminal = new();
        RecordingCommand command = new();

        int exit = await Dispatcher(Commands(command), terminal).RunAsync(["record"], Ct);

        Assert.Equal((int)CommandExit.UsageError, exit);
        Assert.Contains("--label", terminal.Error, StringComparison.Ordinal);
        Assert.Equal(0, command.Runs);
    }

    [Fact]
    public async Task OptionWithoutValue_ExitsWithTwo()
    {
        FakeTerminal terminal = new();

        int exit = await Dispatcher(Commands(new RecordingCommand()), terminal).RunAsync(["record", "--label"], Ct);

        Assert.Equal((int)CommandExit.UsageError, exit);
    }

    [Fact]
    public async Task EmptyValueAndRepeatedOption_ExitWithTwo()
    {
        FakeTerminal terminal = new();
        IReadOnlyList<IAdminCommand> commands = Commands(new RecordingCommand());

        Assert.Equal((int)CommandExit.UsageError, await Dispatcher(commands, terminal).RunAsync(["record", "--label", string.Empty], Ct));
        Assert.Equal((int)CommandExit.UsageError, await Dispatcher(commands, terminal).RunAsync(["record", "--label", "a", "--label", "b"], Ct));
    }

    [Fact]
    public async Task ValidOptions_ReachTheCommand_AndItsExitCodeIsReturned()
    {
        FakeTerminal terminal = new();
        RecordingCommand command = new() { Result = CommandExit.Refused };

        int exit = await Dispatcher(Commands(command), terminal).RunAsync(["record", "--label", "x"], Ct);

        Assert.Equal((int)CommandExit.Refused, exit);
        Assert.Equal("x", command.LastLabel);
        Assert.Equal(1, command.Runs);
    }

    [Fact]
    public async Task UnexpectedError_ExitsWithOne_AndWritesTheTypeOnly()
    {
        FakeTerminal terminal = new();
        RecordingCommand command = new() { Failure = new InvalidOperationException("secret detail " + TypedValue) };

        int exit = await Dispatcher(Commands(command), terminal).RunAsync(["record", "--label", "x"], Ct);

        Assert.Equal((int)CommandExit.UnexpectedError, exit);
        Assert.Contains(nameof(InvalidOperationException), terminal.Error, StringComparison.Ordinal);
        Assert.DoesNotContain("secret detail", terminal.AllOutput, StringComparison.Ordinal);
        Assert.DoesNotContain(TypedValue, terminal.AllOutput, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UsageError_NeedsNoServiceProvider()
    {
        FakeTerminal terminal = new();
        int made = 0;

        int exit = await Dispatcher(Commands(new RecordingCommand()), terminal, () =>
        {
            made++;
            return new ServiceCollection().BuildServiceProvider();
        }).RunAsync(["bogus"], Ct);

        Assert.Equal((int)CommandExit.UsageError, exit);
        Assert.Equal(0, made);
    }

    [Fact]
    public async Task Help_ListsEveryCommandWithItsOptions_AndExitsWithZero()
    {
        FakeTerminal terminal = new();

        int exit = await Dispatcher(Commands(new RecordingCommand()), terminal).RunAsync(["--help"], Ct);

        Assert.Equal((int)CommandExit.Success, exit);
        Assert.Contains("record", terminal.Output, StringComparison.Ordinal);
        Assert.Contains("--label", terminal.Output, StringComparison.Ordinal);
        Assert.Contains("--at", terminal.Output, StringComparison.Ordinal);
        Assert.Equal(string.Empty, terminal.Error);
    }

    [Fact]
    public async Task CommandHelp_ListsTheOptionsOfThatCommand()
    {
        FakeTerminal terminal = new();
        RecordingCommand command = new();

        int exit = await Dispatcher(Commands(command), terminal).RunAsync(["record", "--help"], Ct);

        Assert.Equal((int)CommandExit.Success, exit);
        Assert.Contains("--label", terminal.Output, StringComparison.Ordinal);
        Assert.Equal(0, command.Runs);
    }

    [Theory]
    [InlineData("2026-10-29")]
    [InlineData("2026-10-29T14:30:00")]
    [InlineData("29.10.2026 14:30")]
    [InlineData("not-a-moment")]
    [InlineData("2026-02-31T10:00:00Z")]
    public async Task Moment_WithoutAnOffsetOrMalformed_ExitsWithTwo(string value)
    {
        FakeTerminal terminal = new();
        RecordingCommand command = new();

        int exit = await Dispatcher(Commands(command), terminal).RunAsync(["record", "--label", "x", "--at", value], Ct);

        Assert.Equal((int)CommandExit.UsageError, exit);
        Assert.DoesNotContain(value, terminal.AllOutput, StringComparison.Ordinal);
        Assert.Equal(0, command.Runs);
    }

    [Theory]
    [InlineData("2026-10-29T14:30:00Z", "2026-10-29T14:30:00Z")]
    [InlineData("2026-10-29T14:30:00+03:00", "2026-10-29T11:30:00Z")]
    [InlineData("2026-10-29T14:30-05:00", "2026-10-29T19:30:00Z")]
    public async Task Moment_WithAnOffset_ArrivesAsUtc(string value, string expectedUtc)
    {
        FakeTerminal terminal = new();
        RecordingCommand command = new();

        int exit = await Dispatcher(Commands(command), terminal).RunAsync(["record", "--label", "x", "--at", value], Ct);

        Assert.Equal((int)CommandExit.Success, exit);
        Assert.Equal(DateTimeOffset.Parse(expectedUtc, System.Globalization.CultureInfo.InvariantCulture), command.LastAt);
        Assert.Equal(TimeSpan.Zero, command.LastAt!.Value.Offset);
    }

    private sealed class RecordingCommand : IAdminCommand
    {
        public string Name => "record";

        public string Description => "Test komutu.";

        public IReadOnlyList<CommandOption> Options { get; } =
        [
            new CommandOption("label", "Etiket.", Required: true),
            new CommandOption("at", "An.", Required: false, CommandOptionKind.Moment),
        ];

        public CommandExit Result { get; set; } = CommandExit.Success;

        public Exception? Failure { get; set; }

        public int Runs { get; private set; }

        public string? LastLabel { get; private set; }

        public DateTimeOffset? LastAt { get; private set; }

        public Task<CommandExit> RunAsync(CommandInput input, CancellationToken ct)
        {
            Runs++;
            if (Failure is not null)
            {
                throw Failure;
            }

            LastLabel = input.Text("label");
            LastAt = input.Moment("at");
            return Task.FromResult(Result);
        }
    }
}
