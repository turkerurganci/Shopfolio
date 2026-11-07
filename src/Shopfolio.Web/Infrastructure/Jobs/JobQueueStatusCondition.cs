using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Status;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Jobs;

/// <summary>
/// The job queue as a condition of the status endpoint (05 9.4.16; 09 13.1 item 2): read from the queue itself, so it stands with the queue - the job
/// table is touched only under <c>Infrastructure/Jobs/</c>. A pending row is due when its next attempt is empty or has come; the oldest due row
/// older than the limit opens the condition, and so does any failed row (it does not go away by itself).
/// </summary>
internal sealed class JobQueueStatusCondition : IStatusCondition
{
    private readonly AppDbContext _db;
    private readonly TimeProvider _time;
    private readonly IOptions<StatusThresholdOptions> _options;

    public JobQueueStatusCondition(AppDbContext db, TimeProvider time, IOptions<StatusThresholdOptions> options)
    {
        _db = db;
        _time = time;
        _options = options;
    }

    public string Name => "JobQueue";

    public async Task<bool> IsOpenAsync(CancellationToken ct)
    {
        DateTimeOffset now = _time.GetUtcNow();
        bool failedExists = await _db.Jobs.AsNoTracking().AnyAsync(j => j.JobState == JobState.Failed, ct);
        DateTimeOffset? oldestDue = await _db.Jobs.AsNoTracking()
            .Where(j => j.JobState == JobState.Pending && (j.NextAttemptAt == null || j.NextAttemptAt <= now))
            .Select(j => (DateTimeOffset?)(j.NextAttemptAt ?? j.CreatedAt))
            .OrderBy(moment => moment)
            .FirstOrDefaultAsync(ct);
        return StatusRules.JobQueueStuck(oldestDue, failedExists, now, _options.Value);
    }
}
