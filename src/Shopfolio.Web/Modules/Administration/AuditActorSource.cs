using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;

namespace Shopfolio.Modules.Administration;

/// <summary>The administrator an audit row is written for: the id, name and email as they are at the moment of the operation (06 4.63).</summary>
internal sealed record AuditActor(Guid AdminId, string Name, string Email);

/// <summary>Where the trail takes the acting administrator from (09 9.2 item 1). Tests give it a fixed administrator; the application gives it the request.</summary>
internal interface IAuditActorSource
{
    /// <summary>The signed-in administrator, or <see langword="null"/> when there is none (a job, a command, a customer's request).</summary>
    Task<AuditActor?> CurrentAsync(CancellationToken ct);
}

/// <summary>
/// The acting administrator from the request (09 9.2 item 1; 09 15.1 item 2): the identity of the panel's scheme carries the administrator's account key;
/// the name and email are read from the account row at the moment of the operation. A request without an administrator session, an account that no longer
/// exists and a code that runs outside a request have no actor.
/// </summary>
internal sealed class RequestAuditActorSource : IAuditActorSource
{
    private readonly IHttpContextAccessor _accessor;
    private readonly AppDbContext _db;

    public RequestAuditActorSource(IHttpContextAccessor accessor, AppDbContext db)
    {
        _accessor = accessor;
        _db = db;
    }

    public async Task<AuditActor?> CurrentAsync(CancellationToken ct)
    {
        HttpContext? context = _accessor.HttpContext;
        if (context is null)
        {
            return null;
        }

        AuthenticateResult result = await context.AuthenticateAsync(SessionSchemes.AdminScheme);
        // The account key is read the one way the sessions write it (09 24 item 4; D-176).
        if ((result.Succeeded ? result.Principal?.AccountId() : null) is not { } adminId)
        {
            return null;
        }

        var row = await _db.Admins.AsNoTracking()
            .Where(a => a.Id == adminId)
            .Select(a => new { a.Name, a.Email })
            .SingleOrDefaultAsync(ct);
        return row is null ? null : new AuditActor(adminId, row.Name, row.Email);
    }
}
