// Deliberate violation sample of the secret source scan (09 19.2 item 3): code that reads the whole environment by hand.
// This file is excluded from compilation and from the real scan; the scan test reads it as text.
// The violation is on line 6.
public static class EnvironmentReadAllSample
{
    public static object Read() => System.Environment . GetEnvironmentVariables();
}
