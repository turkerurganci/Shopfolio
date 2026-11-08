using System.Text;

using Shopfolio.Commands;

namespace Shopfolio.UnitTests.Fakes;

/// <summary>
/// Hand-written fake of <see cref="ITerminal"/> (09 19.1 item 3, 13.2 item 7): the visible answers and the hidden keys are queued by the test, the output is kept apart for the
/// standard and the error stream, and every read is counted so a test shows that a redirected input was never read.
/// </summary>
internal sealed class FakeTerminal : ITerminal
{
    private readonly Queue<string> _lines = new();
    private readonly Queue<ConsoleKeyInfo> _keys = new();
    private readonly StringBuilder _out = new();
    private readonly StringBuilder _error = new();

    public bool IsInputRedirected { get; set; }

    /// <summary>How many times the code read a line or a key.</summary>
    public int Reads { get; private set; }

    public string Output => _out.ToString();

    public string Error => _error.ToString();

    /// <summary>Everything written, standard and error together.</summary>
    public string AllOutput => Output + Error;

    /// <summary>Queues the answer to the next visible question.</summary>
    public FakeTerminal Answer(string line)
    {
        _lines.Enqueue(line);
        return this;
    }

    /// <summary>Queues the keys of a hidden answer, one key per character, and the Enter that ends it.</summary>
    public FakeTerminal Secret(string text)
    {
        foreach (char c in text)
        {
            _keys.Enqueue(new ConsoleKeyInfo(c, ConsoleKey.A, shift: false, alt: false, control: false));
        }

        _keys.Enqueue(Enter);
        return this;
    }

    /// <summary>Queues raw keys (a Backspace in the middle of a hidden answer).</summary>
    public FakeTerminal Keys(params ConsoleKeyInfo[] keys)
    {
        foreach (ConsoleKeyInfo key in keys)
        {
            _keys.Enqueue(key);
        }

        return this;
    }

    public static ConsoleKeyInfo Enter => new('\r', ConsoleKey.Enter, shift: false, alt: false, control: false);

    public static ConsoleKeyInfo Backspace => new('\b', ConsoleKey.Backspace, shift: false, alt: false, control: false);

    public static ConsoleKeyInfo Char(char c) => new(c, ConsoleKey.A, shift: false, alt: false, control: false);

    public string? ReadLine()
    {
        Reads++;
        return _lines.Count > 0 ? _lines.Dequeue() : null;
    }

    public ConsoleKeyInfo ReadKeyHidden()
    {
        Reads++;
        return _keys.Count > 0 ? _keys.Dequeue() : throw new InvalidOperationException("The test queued no more keys.");
    }

    public void Write(string text) => _out.Append(text);

    public void WriteLine(string text) => _out.AppendLine(text);

    public void WriteErrorLine(string text) => _error.AppendLine(text);
}
