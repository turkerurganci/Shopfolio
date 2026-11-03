// Clean counterpart of the secret source samples: the scan must report nothing here.
// This file is excluded from compilation and from the real scan; the scan test reads it as text.
// Environment.GetEnvironmentVariable is only named in this comment.
public static class CleanSample
{
    public static string Machine() => System.Environment.MachineName;
}
