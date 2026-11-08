using Shopfolio.Commands;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.UnitTests.Commands;

/// <summary>
/// The terminal helper of the commands (T303; 09 13.2 items 4, 7; KK5, KK7): a redirected input is looked at before anything is read and nothing is read from it; a secret is read key
/// by key through the hidden reader, never through the line reader that shows the typed characters.
/// </summary>
public sealed class TerminalPromptTests
{
    [Fact]
    public void RedirectedInput_IsNotInteractive_AndAskingReadsNothing()
    {
        FakeTerminal terminal = new() { IsInputRedirected = true };
        terminal.Answer("must-not-be-read");
        TerminalPrompt prompt = new(terminal);

        Assert.False(prompt.IsInteractive);
        Assert.Throws<InvalidOperationException>(() => prompt.Ask("Ad: "));
        Assert.Throws<InvalidOperationException>(() => prompt.AskSecret("Şifre: "));
        Assert.Equal(0, terminal.Reads);
        Assert.Equal(string.Empty, terminal.AllOutput);
    }

    [Fact]
    public void Ask_ReturnsTheLine_AndAnEndedInputIsEmpty()
    {
        FakeTerminal terminal = new();
        terminal.Answer("Ayşe Yılmaz");
        TerminalPrompt prompt = new(terminal);

        Assert.Equal("Ayşe Yılmaz", prompt.Ask("Ad: "));
        Assert.Equal(string.Empty, prompt.Ask("Ad: "));
    }

    [Fact]
    public void AskSecret_ReadsHiddenKeys_NeverTheLineReader_AndEchoesNothing()
    {
        FakeTerminal terminal = new();
        terminal.Secret("gizli-sifre-123");
        TerminalPrompt prompt = new(terminal);

        string typed = prompt.AskSecret("Şifre: ");

        Assert.Equal("gizli-sifre-123", typed);
        Assert.DoesNotContain("gizli", terminal.AllOutput, StringComparison.Ordinal);
        Assert.Equal("gizli-sifre-123".Length + 1, terminal.Reads);
    }

    [Fact]
    public void AskSecret_BackspaceRemovesTheLastCharacter_AControlKeyIsIgnored()
    {
        FakeTerminal terminal = new();
        terminal.Keys(
            FakeTerminal.Char('a'),
            FakeTerminal.Char('b'),
            FakeTerminal.Char('x'),
            FakeTerminal.Backspace,
            FakeTerminal.Backspace,
            FakeTerminal.Backspace,
            FakeTerminal.Backspace,
            FakeTerminal.Char('\t'),
            FakeTerminal.Char('c'),
            FakeTerminal.Enter);
        TerminalPrompt prompt = new(terminal);

        Assert.Equal("c", prompt.AskSecret("Şifre: "));
    }

    [Fact]
    public void AskSecret_OneBackspaceRemovesBothHalvesOfACharacterOutsideTheBasicPlane()
    {
        FakeTerminal terminal = new();
        terminal.Keys(
            FakeTerminal.Char('a'),
            FakeTerminal.Char('\uD83D'),
            FakeTerminal.Char('\uDE00'),
            FakeTerminal.Backspace,
            FakeTerminal.Enter);
        TerminalPrompt prompt = new(terminal);

        Assert.Equal("a", prompt.AskSecret("Şifre: "));
    }
}
