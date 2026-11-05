// Deliberate violation sample of the exception message scan (09 18.2 item 2c): the data access layer is told to put values into the message.
// The scan reads this file as text; it is not compiled. Line numbers are part of the test.
internal static class SensitiveDataLogging
{
    public static void Configure(DbContextOptionsBuilder options)
    {
        options.EnableSensitiveDataLogging();
        options.UseNpgsql("Host=db;Database=shopfolio;Include Error Detail=true");
        options.UseNpgsql("Host=db;IncludeErrorDetail=true");
        // A comment that says EnableSensitiveDataLogging is not a finding.
    }
}
