using System.Text;

namespace Shopfolio.Infrastructure.FileSecrets;

/// <summary>
/// Reads the secrets of the installation from files (05 6.x.1, MK-59): Compose mounts each secret as a file under
/// <c>/run/secrets</c> into the one container that uses it, and the application reads it from there. A secret never travels
/// through an environment variable. The value is never logged: a missing file is reported by its name only.
/// </summary>
internal sealed class SecretFiles
{
    /// <summary>The directory Compose mounts secret files into (05 6.x.1).</summary>
    public const string DefaultDirectory = "/run/secrets";

    private readonly string _directory;

    public SecretFiles(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        _directory = directory;
    }

    /// <summary>
    /// Returns the content of the secret file <paramref name="name"/> without its closing line break, or <see langword="null"/>
    /// when the file does not exist (the secret is not mounted into this container). Only the line break that editors and
    /// <c>echo</c> append is removed: spaces inside or around the value belong to it.
    /// </summary>
    public string? Read(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (name.AsSpan().IndexOfAny(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) >= 0 || name is "." or "..")
        {
            throw new ArgumentException("A secret name is a file name, not a path.", nameof(name));
        }

        string path = Path.Combine(_directory, name);
        if (!File.Exists(path))
        {
            return null;
        }

        string content = File.ReadAllText(path, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        if (content.EndsWith("\r\n", StringComparison.Ordinal))
        {
            return content[..^2];
        }

        return content.EndsWith('\n') ? content[..^1] : content;
    }
}
