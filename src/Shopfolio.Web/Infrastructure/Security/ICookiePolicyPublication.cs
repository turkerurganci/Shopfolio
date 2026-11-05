namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// Whether the cookie policy has a published version (05 6.7.10). The shared infrastructure refers to no module type (09 6 item 2), so the
/// question is put as this interface and the Settings module, which owns the legal texts (05 3.3.10), answers it. The method only reads: it opens no
/// transaction of its own (09 6 item 3).
/// </summary>
internal interface ICookiePolicyPublication
{
    /// <summary>True when at least one version of the cookie policy has been published; a drafted text is not a publication.</summary>
    Task<bool> IsPublishedAsync(CancellationToken cancellationToken);
}
