using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Commands;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.UnitTests.Commands;

/// <summary>
/// The questions of <c>create-admin</c> that need no database (T303; 05 8.5.2; 09 13.2 items 4, 5; KK5, KK7): a redirected input ends with 2 before anything is read, written or opened;
/// an empty name and two different passwords are refused before the command's line is written; and no output repeats a typed value. The account itself is the integration tests'.
/// </summary>
public sealed class CreateAdminCommandTests
{
    private const string Name = "Ayşe Yılmaz";
    private const string Email = "ayse.yilmaz@example.com";
    private const string Password = "kirmizi-bisiklet-2026";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static (CommandInput Input, RecordingWriter Writer) Input(FakeTerminal terminal)
    {
        RecordingWriter writer = new();
        ServiceCollection services = new();
        services.AddSingleton<IAuditLineWriter>(writer);
        ServiceProvider provider = services.BuildServiceProvider();
        return (new CommandInput(new Dictionary<string, object>(), terminal, provider), writer);
    }

    [Fact]
    public async Task RedirectedInput_ExitsWithTwo_ReadsNothing_AndWritesNoLine()
    {
        FakeTerminal terminal = new() { IsInputRedirected = true };
        terminal.Answer(Name).Answer(Email).Secret(Password).Secret(Password);
        (CommandInput input, RecordingWriter writer) = Input(terminal);

        CommandExit exit = await new CreateAdminCommand().ExecuteAsync(input, Ct);

        Assert.Equal(CommandExit.UsageError, exit);
        Assert.Equal(0, terminal.Reads);
        Assert.Empty(writer.Lines);
        Assert.DoesNotContain(Name, terminal.AllOutput, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task EmptyName_IsRefused_BeforeTheLine(string name)
    {
        FakeTerminal terminal = new();
        terminal.Answer(name).Answer(Email).Secret(Password).Secret(Password);
        (CommandInput input, RecordingWriter writer) = Input(terminal);

        CommandExit exit = await new CreateAdminCommand().ExecuteAsync(input, Ct);

        Assert.Equal(CommandExit.Refused, exit);
        Assert.Empty(writer.Lines);
    }

    [Fact]
    public async Task TwoDifferentPasswords_AreRefused_BeforeTheLine_AndNeitherIsWritten()
    {
        FakeTerminal terminal = new();
        terminal.Answer(Name).Answer(Email).Secret(Password).Secret(Password + "x");
        (CommandInput input, RecordingWriter writer) = Input(terminal);

        CommandExit exit = await new CreateAdminCommand().ExecuteAsync(input, Ct);

        Assert.Equal(CommandExit.Refused, exit);
        Assert.Empty(writer.Lines);
        Assert.DoesNotContain(Password, terminal.AllOutput, StringComparison.Ordinal);
        Assert.DoesNotContain(Email, terminal.AllOutput, StringComparison.Ordinal);
    }

    [Fact]
    public void TheCommand_TakesNoOption_SoNoPersonalValueCanComeFromTheCommandLine()
    {
        Assert.Empty(new CreateAdminCommand().Options);
    }

    private sealed class RecordingWriter : IAuditLineWriter
    {
        public List<AuditLine> Lines { get; } = [];

        public void Write(AuditLine line) => Lines.Add(line);
    }
}
