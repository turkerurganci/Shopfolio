// Deliberate violation sample of the logging scan (09 18.1 item 1): a module that names the logging library instead of asking for ILogger<T>.
// The scan reads this file as text; it is not compiled. Line numbers are part of the test.
using Serilog;

internal static class LibraryInAModule
{
    // A comment that says Serilog is not a finding.
    public static void Write(string text) => Log.Information(text);
}
