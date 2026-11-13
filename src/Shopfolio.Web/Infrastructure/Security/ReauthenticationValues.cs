using System.Globalization;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The single-use value of a reauthentication (07 2.5.2; 05 6.1.5; K-971). A successful reauthentication writes a permission for the one operation it
/// was started for into the session row and binds it to a value that is put into the hidden field <see cref="FieldName"/> of the operation's form
/// (<see cref="Hidden"/>); the row keeps only the value's digest (06 4.39). The form's POST consumes the value with <see cref="TryRedeemAsync"/>:
/// one conditional update that takes the permission only while the session still stands and the operation and the value are the same, in the
/// caller's own transaction, at the point where the operation is applied - a validation error or an unticked confirmation does not consume it. No row
/// updated means no operation: a second submission finds the value consumed and the form asks for the reauthentication again (KK3), and an operation
/// that was sent while a command deleted the sessions finds no row either (K-1084). The value never travels in an address or a header.
/// </summary>
internal sealed class ReauthenticationValues
{
    /// <summary>The name of the hidden field and of the model property that carries the value (07 5.1.6).</summary>
    public const string FieldName = "ReauthenticationToken";

    private readonly AppDbContext _db;

    public ReauthenticationValues(AppDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
    }

    /// <summary>
    /// Writes the permission for <paramref name="operation"/> into the session row and returns the value for the form's hidden field; a permission the
    /// session held before is replaced (the new form is the one that counts). Null when the session no longer exists.
    /// </summary>
    public async Task<string?> GrantAsync(Guid sessionId, ReauthenticationOperation operation, CancellationToken ct)
    {
        string value = SecretKey.Create();
        string digest = SecretKey.Hash(value);
        int updated = await _db.Sessions
            .Where(s => s.Id == sessionId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(x => x.ReauthenticatedOperation, operation).SetProperty(x => x.ReauthenticationTokenHash, digest),
                ct);
        return updated == 1 ? value : null;
    }

    /// <summary>Drops the session's permission and its value (the re-verification page opens: the earlier form's value is void, 05 6.1.5); a session without one is left as it is.</summary>
    public async Task DropAsync(Guid sessionId, CancellationToken ct)
    {
        await _db.Sessions
            .Where(s => s.Id == sessionId && (s.ReauthenticatedOperation != null || s.ReauthenticationTokenHash != null))
            .ExecuteUpdateAsync(
                s => s.SetProperty(x => x.ReauthenticatedOperation, (ReauthenticationOperation?)null).SetProperty(x => x.ReauthenticationTokenHash, (string?)null),
                ct);
    }

    /// <summary>
    /// Consumes the permission with one conditional update: only while the session exists, holds the permission for <paramref name="operation"/> and
    /// its digest is the digest of <paramref name="value"/>. True means this call took it and the operation may run; false means it must not.
    /// </summary>
    public async Task<bool> TryRedeemAsync(Guid sessionId, ReauthenticationOperation operation, string? value, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        string digest = SecretKey.Hash(value);
        int updated = await _db.Sessions
            .Where(s => s.Id == sessionId && s.ReauthenticatedOperation == operation && s.ReauthenticationTokenHash == digest)
            .ExecuteUpdateAsync(
                s => s.SetProperty(x => x.ReauthenticatedOperation, (ReauthenticationOperation?)null).SetProperty(x => x.ReauthenticationTokenHash, (string?)null),
                ct);
        return updated == 1;
    }

    /// <summary>
    /// Whether the session holds the permission for <paramref name="operation"/> and its digest is the digest of <paramref name="value"/> - a plain read that spends nothing (T517; 07 UC-63: the account
    /// screen opens the operation's form only when this is true; the single-use take is <see cref="TryRedeemAsync"/>).
    /// </summary>
    public async Task<bool> HoldsAsync(Guid sessionId, ReauthenticationOperation operation, string? value, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        string digest = SecretKey.Hash(value);
        return await _db.Sessions.AsNoTracking().AnyAsync(s => s.Id == sessionId && s.ReauthenticatedOperation == operation && s.ReauthenticationTokenHash == digest, ct);
    }

    /// <summary>The hidden field of the operation's form (07 2.5.2); <paramref name="prefix"/> is the model path of the input, when it has one.</summary>
    public static IHtmlContent Hidden(string value, string prefix = "")
    {
        ArgumentException.ThrowIfNullOrEmpty(value);
        TagBuilder input = new("input");
        input.Attributes["type"] = "hidden";
        input.Attributes["name"] = string.IsNullOrEmpty(prefix) ? FieldName : $"{prefix}.{FieldName}";
        input.Attributes["value"] = value;
        input.TagRenderMode = TagRenderMode.SelfClosing;
        return input;
    }

    /// <summary>Renders the field as text (a test or a log line).</summary>
    public static string Render(IHtmlContent content)
    {
        ArgumentNullException.ThrowIfNull(content);
        using StringWriter writer = new(CultureInfo.InvariantCulture);
        content.WriteTo(writer, HtmlEncoder.Default);
        return writer.ToString();
    }
}
