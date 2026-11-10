namespace Shopfolio.Infrastructure.InstallationSettings;

/// <summary>
/// The content of the secret file <c>email_credential</c> (08 2.2 "Kimlik doğrulama"; DEPLOY_RUNBOOK J.2; D-79): two lines, the access key id and then the
/// secret key, the same shape as the backup storage keys; no other content. A line break at the end of the file is allowed. Neither value has a blank
/// inside. The value is never written to a log or to a failure's text.
/// </summary>
internal sealed record EmailCredential(string AccessKeyId, string SecretKey)
{
    /// <summary>What a failed check says the content is expected to be (never the content).</summary>
    public const string Expectation = "expected two lines - the access key id, then the secret key";

    /// <summary>The two values, or null when the content does not have the shape.</summary>
    public static EmailCredential? Parse(string? content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return null;
        }

        string[] lines = content.ReplaceLineEndings("\n").TrimEnd('\n').Split('\n');
        if (lines.Length != 2 || !IsValue(lines[0]) || !IsValue(lines[1]))
        {
            return null;
        }

        return new EmailCredential(lines[0], lines[1]);
    }

    /// <summary>The record's own text would print the secret; this one prints the type's name.</summary>
    public override string ToString() => nameof(EmailCredential);

    private static bool IsValue(string line) => line.Length > 0 && !line.Any(char.IsWhiteSpace);
}
