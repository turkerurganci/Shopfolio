using System.Security.Cryptography;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

using Verdict = Microsoft.AspNetCore.Identity.PasswordVerificationResult;

namespace Shopfolio.Infrastructure.Security;

/// <summary>What a password check found: whether it matched and, when it did, whether the stored digest should be made again with the current settings.</summary>
internal readonly record struct HashCheck(bool Matched, bool NeedsRehash);

/// <summary>The subject the framework hasher asks for; it carries nothing, since the digest holds the salt.</summary>
internal sealed class HashSubject;

/// <summary>
/// The one wrapper of the framework's password hasher (09 15.1 item 5; 05 6.1.4; K-900): the shop front and the panel hash and check a password only here. When there is no
/// account for an address the check still runs, against a fake digest made once at start-up, and its result is thrown away, so the answer takes about as long for an address
/// that has an account as for one that has not. The policy (length, common passwords) is T342's and is not checked here.
/// </summary>
internal sealed class PasswordService
{
    /// <summary>PBKDF2 iterations of a new digest: the current OWASP figure for HMAC-SHA512, above the framework default (09 15.1 item 5; K-900).</summary>
    internal const int IterationCount = 210_000;

    private readonly PasswordHasher<HashSubject> _hasher;
    private readonly HashSubject _subject = new();
    private readonly string _fakeDigest;

    public PasswordService()
    {
        _hasher = new PasswordHasher<HashSubject>(Options.Create(new PasswordHasherOptions { IterationCount = IterationCount }));
        _fakeDigest = _hasher.HashPassword(_subject, Convert.ToHexString(RandomNumberGenerator.GetBytes(32)));
    }

    /// <summary>The digest of a password, with a salt of its own.</summary>
    public string Hash(string entered)
    {
        ArgumentNullException.ThrowIfNull(entered);
        return _hasher.HashPassword(_subject, entered);
    }

    /// <summary>Checks the password against a digest.</summary>
    public HashCheck Verify(string digest, string entered)
    {
        ArgumentNullException.ThrowIfNull(digest);
        ArgumentNullException.ThrowIfNull(entered);
        Verdict outcome = _hasher.VerifyHashedPassword(_subject, digest, entered);
        return new HashCheck(outcome != Verdict.Failed, outcome == Verdict.SuccessRehashNeeded);
    }

    /// <summary>The check an address with no account (or an account without a password) gets: the same work, never a match.</summary>
    public HashCheck VerifyNobody(string entered)
    {
        ArgumentNullException.ThrowIfNull(entered);
        _hasher.VerifyHashedPassword(_subject, _fakeDigest, entered);
        return new HashCheck(false, false);
    }
}
