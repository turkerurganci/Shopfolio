// Deliberate violation sample of the log file scan (05 9.2.2; 09 18.1 item 3): logging code that deletes a log file, empties one, leaves a limit
// of the library open and writes a disposal record. The scan reads this file as text; it is not compiled. Line numbers are part of the test.
internal static class LogFileDeletion
{
    // A comment that says File.Delete(path) or retainedFileCountLimit: 31 is not a finding.
    public static void Run(string path, string directory, FileInfo info)
    {
        File.Delete(path);
        Directory.Delete(directory, recursive: true);
        info.Delete();
        using FileStream stream = new(path, FileMode.Truncate);
        File.WriteAllText(path, string.Empty);
        var configuration = new LoggerConfiguration().WriteTo.File(path, retainedFileCountLimit: 31, fileSizeLimitBytes: 1073741824, rollOnFileSizeLimit: true);
        var closed = new LoggerConfiguration().WriteTo.File(path, retainedFileCountLimit: null, fileSizeLimitBytes: null);
        context.Add(new DisposalRecord());
        /* File.Delete(path); in a block comment is not a finding either */
    }
}
