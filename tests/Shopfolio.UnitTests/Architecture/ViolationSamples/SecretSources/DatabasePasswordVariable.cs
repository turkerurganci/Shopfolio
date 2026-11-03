// Deliberate violation sample of the secret source scan (09 19.2 item 3): the database client's password variable.
// This file is excluded from compilation and from the real scan; the scan test reads it as text.
// The violation is on line 6.
public static class DatabasePasswordVariableSample
{
    public const string Name = "PGPASSWORD";
}
