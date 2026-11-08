using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.CorporateContent.Contracts;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>The result of the social links save; a refusal is a return value (09 9.1 item 3).</summary>
internal sealed record SocialLinksResult(IReadOnlyList<FieldRejection> Rejections) : IOperationResult
{
    public bool IsRejected => Rejections.Count > 0;
}

/// <summary>One link the site shows: the platform and the address.</summary>
internal sealed record SocialLinkView(SocialPlatform Platform, string Url);

/// <summary>
/// The social media links (T358; 02 3.27.12; 06 4.55): the six platforms of the closed list, at most one link each, every one optional. The form sends a link per platform
/// name; a filled one is checked for the form of a link and written, an emptied one deletes the platform's row. A platform outside the list, or a link of the wrong form, refuses
/// the whole save and nothing is written. The links belong to no order: no order table has a column for them (an order freezes what it needs, 02 3.27.12), so a change here never
/// reaches an existing order. The save only changes tracked rows - the owner of the transaction saves them (09 9.1 item 3). The WhatsApp number is the home page section's setting
/// (06 4.2) and is written with it (T361); its rule - ten digits, optional - is <see cref="CheckWhatsAppNumber"/>, so the section's save does not write a second one.
/// </summary>
internal sealed class SocialLinks
{
    private readonly AppDbContext _db;
    private readonly TimeProvider _time;

    public SocialLinks(AppDbContext db, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(time);
        _db = db;
        _time = time;
    }

    /// <summary>The platforms in the order the site lists them: the order of the closed list (06 5.3.11); a link carries no order of its own (06 4.55).</summary>
    public static IReadOnlyList<SocialPlatform> Platforms { get; } = [.. Enum.GetValues<SocialPlatform>()];

    /// <summary>
    /// Saves the form: <paramref name="links"/> maps a platform name to the typed link. A platform the form leaves out is left as it is; a platform sent empty loses its link.
    /// </summary>
    public async Task<SocialLinksResult> SaveAsync(IReadOnlyDictionary<string, string?> links, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(links);
        List<FieldRejection> rejections = [];
        Dictionary<SocialPlatform, string?> wanted = [];
        foreach ((string name, string? typed) in links)
        {
            if (!TryParsePlatform(name, out SocialPlatform platform))
            {
                rejections.Add(new FieldRejection(name, FieldCodes.InvalidListValue));
                continue;
            }

            string? url = ContentInputRules.Text(typed);
            ContentInputRules.Apply(rejections, name, url, new InvalidUrlAttribute(), FieldCodes.InvalidUrl);
            wanted[platform] = url;
        }

        if (rejections.Count > 0)
        {
            return new SocialLinksResult(rejections);
        }

        List<SocialLink> rows = await _db.SocialLinks.ToListAsync(ct);
        foreach ((SocialPlatform platform, string? url) in wanted)
        {
            SocialLink? row = rows.SingleOrDefault(r => r.SocialPlatform == platform);
            if (url is null)
            {
                if (row is not null)
                {
                    _db.SocialLinks.Remove(row);
                }
            }
            else if (row is null)
            {
                _db.SocialLinks.Add(SocialLink.Create(platform, url, _time.GetUtcNow()));
            }
            else
            {
                row.Url = url;
            }
        }

        return new SocialLinksResult([]);
    }

    /// <summary>The links the site shows, in the order of the closed list. A plain read.</summary>
    public async Task<IReadOnlyList<SocialLinkView>> ListAsync(CancellationToken ct)
    {
        List<SocialLink> rows = await _db.SocialLinks.AsNoTracking().ToListAsync(ct);
        return [.. Platforms.SelectMany(p => rows.Where(r => r.SocialPlatform == p).Select(r => new SocialLinkView(p, r.Url)))];
    }

    /// <summary>
    /// The rule of the WhatsApp number (02 3.27.12; 06 2.5.1): optional; a number of the accepted telephone forms is stored as its ten digits, an empty one as nothing. Returns the
    /// stored form and, for a text that is not a telephone number, the rejection of the <paramref name="field"/>.
    /// </summary>
    public static (string? Stored, FieldRejection? Rejection) CheckWhatsAppNumber(string field, string? typed)
    {
        ArgumentNullException.ThrowIfNull(field);
        string? stored = ContentInputRules.Phone(typed);
        List<FieldRejection> rejections = [];
        ContentInputRules.Apply(rejections, field, stored, new InvalidPhoneAttribute(), FieldCodes.InvalidPhone);
        return (stored, rejections.Count == 0 ? null : rejections[0]);
    }

    /// <summary>The platform of a name on the closed list: the exact name only - no number, no other spelling.</summary>
    public static bool TryParsePlatform(string? name, out SocialPlatform platform)
    {
        foreach (SocialPlatform candidate in Platforms)
        {
            if (string.Equals(name, candidate.ToString(), StringComparison.Ordinal))
            {
                platform = candidate;
                return true;
            }
        }

        platform = default;
        return false;
    }
}
