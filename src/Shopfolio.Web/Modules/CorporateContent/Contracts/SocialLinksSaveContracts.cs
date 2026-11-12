using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.CorporateContent.Contracts;

/// <summary>One social link the save changed (07 UC-162): the platform's name and the old and new address; an emptied link has no new address, a new one no old. It reaches the trail row of the home page section.</summary>
public sealed record SocialLinkChange(string Platform, string? OldUrl, string? NewUrl);

/// <summary>One refused field of the social links save: the platform's name as the form sent it and the field code (07 6.2).</summary>
public sealed record SocialLinkRejection(string Field, string Code);

/// <summary>
/// The outcome of the social links save (T535; 07 UC-162; 06 4.55): the links the save changed with their old and new address - none when the form changed nothing -, or the refused fields. A refusal
/// is a value (09 8.1) and writes nothing. The save changes tracked rows only; the owner of the transaction (the page's runner) saves them with the home page section (09 9.1 item 3), and the section's
/// trail row carries the changes (<c>HomepageInput.SocialLinkChanges</c> of the Settings module).
/// </summary>
public sealed class SocialLinksSaveResult : IOperationResult
{
    private static readonly SocialLinkRejection[] NoFields = [];

    private SocialLinksSaveResult(IReadOnlyList<SocialLinkChange> changes, IReadOnlyList<SocialLinkRejection> rejections)
    {
        Changes = changes;
        Rejections = rejections;
    }

    /// <summary>The links the save added, changed or deleted.</summary>
    public IReadOnlyList<SocialLinkChange> Changes { get; }

    /// <summary>The refused fields; empty unless the save was refused for its values.</summary>
    public IReadOnlyList<SocialLinkRejection> Rejections { get; }

    public bool IsRejected => Rejections.Count > 0;

    public static SocialLinksSaveResult Saved(IReadOnlyList<SocialLinkChange> changes)
    {
        ArgumentNullException.ThrowIfNull(changes);
        return new SocialLinksSaveResult(changes, NoFields);
    }

    public static SocialLinksSaveResult Refused(IReadOnlyList<SocialLinkRejection> rejections)
    {
        ArgumentNullException.ThrowIfNull(rejections);
        if (rejections.Count == 0)
        {
            throw new ArgumentException("A refusal for values names at least one field.", nameof(rejections));
        }

        return new SocialLinksSaveResult([], rejections);
    }
}
