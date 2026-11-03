// Deliberate violation sample of the secret source scan (09 19.2 item 3): the environment added as an extra configuration source.
// This file is excluded from compilation and from the real scan; the scan test reads it as text.
// The violation is on line 6.
public static class EnvironmentConfigurationSample
{
    public static void Add(Microsoft.Extensions.Configuration.IConfigurationBuilder builder) => builder.AddEnvironmentVariables();
}
