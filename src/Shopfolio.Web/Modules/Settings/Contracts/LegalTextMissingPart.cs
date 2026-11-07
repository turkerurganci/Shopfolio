namespace Shopfolio.Modules.Settings.Contracts;

/// <summary>
/// What still waits before a legal text counts as complete (02 3.33.2, 3.33.9): the panel names the text and, for the privacy notice, the part. A closed list of named values
/// of the panel's reminder, not a stored value: it has no column and no enum of the data model.
/// </summary>
public sealed record LegalTextMissingPart(string Name)
{
    /// <summary>No version of the text has been published.</summary>
    public static LegalTextMissingPart NotPublished { get; } = new(nameof(NotPublished));

    /// <summary>The hosting and e-mail infrastructure location section of the privacy notice is empty.</summary>
    public static LegalTextMissingPart HostingLocation { get; } = new(nameof(HostingLocation));

    /// <summary>The company identity fields that feed the privacy notice are not all filled.</summary>
    public static LegalTextMissingPart CompanyIdentity { get; } = new(nameof(CompanyIdentity));
}
