using System.Text;

namespace Shopfolio.Commands;

/// <summary>
/// The terminal as the commands see it (09 13.2 item 4): the one place that touches <see cref="Console"/>, so a test puts a fake in its place. Output goes through this interface;
/// no command writes to the console itself.
/// </summary>
internal interface ITerminal
{
    /// <summary>True when the standard input is a pipe, a file or no terminal at all: personal data is not read from it (05 8.5.1, 8.5.8).</summary>
    bool IsInputRedirected { get; }

    /// <summary>Reads a line with the typed characters shown; null at the end of the input.</summary>
    string? ReadLine();

    /// <summary>Reads one key without showing it.</summary>
    ConsoleKeyInfo ReadKeyHidden();

    void Write(string text);

    void WriteLine(string text);

    void WriteErrorLine(string text);
}

/// <summary>The real terminal: the standard console.</summary>
internal sealed class ConsoleTerminal : ITerminal
{
    public bool IsInputRedirected => Console.IsInputRedirected;

    public string? ReadLine() => Console.ReadLine();

    public ConsoleKeyInfo ReadKeyHidden() => Console.ReadKey(intercept: true);

    public void Write(string text) => Console.Out.Write(text);

    public void WriteLine(string text) => Console.Out.WriteLine(text);

    public void WriteErrorLine(string text) => Console.Error.WriteLine(text);
}

/// <summary>
/// The common helper that reads personal data from the terminal (09 13.2 item 4; 05 8.5.1): it looks at <see cref="ITerminal.IsInputRedirected"/> before it reads anything, so a value
/// can never come from a pipe, a file or an environment variable. The password is read without an echo, key by key.
/// </summary>
internal sealed class TerminalPrompt
{
    private readonly ITerminal _terminal;

    public TerminalPrompt(ITerminal terminal)
    {
        ArgumentNullException.ThrowIfNull(terminal);
        _terminal = terminal;
    }

    /// <summary>False when the input is redirected; the command then refuses to start with exit code 2 and reads nothing.</summary>
    public bool IsInteractive => !_terminal.IsInputRedirected;

    /// <summary>Asks for a visible value; empty when the line is empty or the input ended. Never called when <see cref="IsInteractive"/> is false.</summary>
    public string Ask(string label)
    {
        RequireInteractive();
        _terminal.Write(label);
        return _terminal.ReadLine() ?? string.Empty;
    }

    /// <summary>Asks for a secret value: the keys are read with no echo, Backspace removes the last character and Enter ends the line.</summary>
    public string AskSecret(string label)
    {
        RequireInteractive();
        _terminal.Write(label);
        StringBuilder typed = new();
        while (true)
        {
            ConsoleKeyInfo key = _terminal.ReadKeyHidden();
            if (key.Key == ConsoleKey.Enter)
            {
                _terminal.WriteLine(string.Empty);
                return typed.ToString();
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                RemoveLast(typed);
            }
            else if (!char.IsControl(key.KeyChar))
            {
                typed.Append(key.KeyChar);
            }
        }
    }

    // A character outside the basic plane arrives as two keys; one Backspace removes both halves.
    private static void RemoveLast(StringBuilder typed)
    {
        if (typed.Length == 0)
        {
            return;
        }

        int length = typed.Length >= 2 && char.IsLowSurrogate(typed[^1]) && char.IsHighSurrogate(typed[^2]) ? 2 : 1;
        typed.Length -= length;
    }

    private void RequireInteractive()
    {
        if (!IsInteractive)
        {
            throw new InvalidOperationException("Personal data is read only from an interactive terminal (09 13.2 item 4).");
        }
    }
}
