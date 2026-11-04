namespace Shopfolio.Infrastructure.InstallationSettings;

/// <summary>
/// The wording of a failed setting check (05 8.4.6; 09 3.5 item 2). A failure names the setting by its key and says what is wrong;
/// it never carries the value, because a value can be a credential (T226 KK5).
/// </summary>
internal static class SettingFailures
{
    public static string Missing(string key) => key + " is missing.";

    public static string Malformed(string key, string expectation) => key + " is not valid: " + expectation + ".";

    public static string ProductConstant(string key) =>
        key + " names a product constant; the settings file cannot change a product constant.";
}
