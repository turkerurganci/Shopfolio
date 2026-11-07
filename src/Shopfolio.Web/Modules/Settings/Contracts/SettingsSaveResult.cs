using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Settings.Contracts;

/// <summary>
/// The outcome of a save of the identity or of a settings section (07 9.16.2; 09 10.2 item 2, 10.3): saved; refused because the version the form
/// carried is no longer the record's (the conflict warning, 07 2.4.5); or refused because a field is wrong (422), with the fields and their codes.
/// A refusal is a value, not an exception (09 8.1): the transaction runner rolls back and hands it to the page.
/// </summary>
public sealed class SettingsSaveResult : IOperationResult
{
    private static readonly FieldRejection[] NoFields = [];

    private SettingsSaveResult(bool isSaved, bool isVersionConflict, IReadOnlyList<FieldRejection> rejections)
    {
        IsSaved = isSaved;
        IsVersionConflict = isVersionConflict;
        Rejections = rejections;
    }

    public static SettingsSaveResult Saved { get; } = new(true, false, NoFields);

    public static SettingsSaveResult VersionConflict { get; } = new(false, true, NoFields);

    public bool IsSaved { get; }

    public bool IsVersionConflict { get; }

    /// <summary>The refused fields; empty unless the save was refused for its values.</summary>
    public IReadOnlyList<FieldRejection> Rejections { get; }

    public bool IsRejected => !IsSaved;

    public static SettingsSaveResult Refused(IReadOnlyList<FieldRejection> rejections)
    {
        ArgumentNullException.ThrowIfNull(rejections);
        if (rejections.Count == 0)
        {
            throw new ArgumentException("A refusal for values names at least one field.", nameof(rejections));
        }

        return new SettingsSaveResult(false, false, rejections);
    }
}
