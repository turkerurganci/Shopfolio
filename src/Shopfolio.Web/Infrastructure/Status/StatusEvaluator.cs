using System.Data.Common;
using System.Text.Json;

namespace Shopfolio.Infrastructure.Status;

/// <summary>
/// One condition's standing for the server's status command (05 8.5.6): its name, whether it is open, and whether it is open only because its source could not be read
/// (05 9.4.10, K-975). A condition that reads the web process's memory is not evaluated by the command process - it has no such memory - and is marked so
/// (<see cref="NotMeasurable"/>); it is never reported open or closed.
/// </summary>
internal sealed record StatusConditionStanding(string Name, bool IsOpen, bool SourceUnreadable, bool NotMeasurable);

/// <summary>
/// Runs every registered condition (05 9.4.2) and tells which are open. A condition whose source cannot be read - a permission, a connection or
/// a file failure, a damaged file - is open (05 9.4.10, K-975). Only these known failure types are turned into "open": any other exception is
/// not a read failure of a source, passes up to the catching layer and answers 500, which the external check also takes as a failure (09 8.1 item 6).
/// A cancelled request passes up as a cancellation; a source that timed out by itself is a failure of the source.
/// </summary>
internal sealed class StatusEvaluator
{
    private readonly IReadOnlyList<IStatusCondition> _conditions;

    public StatusEvaluator(IEnumerable<IStatusCondition> conditions)
    {
        ArgumentNullException.ThrowIfNull(conditions);
        _conditions = [.. conditions];
    }

    /// <summary>The names of the open conditions, in registration order; empty when there is no warning.</summary>
    public async Task<IReadOnlyList<string>> OpenConditionsAsync(CancellationToken ct)
    {
        List<string> open = [];
        foreach (IStatusCondition condition in _conditions)
        {
            if (await EvaluateAsync(condition, ct) is { IsOpen: true })
            {
                open.Add(condition.Name);
            }
        }

        return open;
    }

    /// <summary>
    /// Every condition's standing in registration order, the open and the closed ones (05 8.5.6). The conditions that read the web process's memory are listed and not evaluated
    /// (<see cref="IStatusCondition.ReadsProcessMemory"/>); the others are evaluated by the same rule as the endpoint's.
    /// </summary>
    public async Task<IReadOnlyList<StatusConditionStanding>> StandingsAsync(CancellationToken ct)
    {
        List<StatusConditionStanding> standings = [];
        foreach (IStatusCondition condition in _conditions)
        {
            standings.Add(condition.ReadsProcessMemory
                ? new StatusConditionStanding(condition.Name, IsOpen: false, SourceUnreadable: false, NotMeasurable: true)
                : await EvaluateAsync(condition, ct));
        }

        return standings;
    }

    private static async Task<StatusConditionStanding> EvaluateAsync(IStatusCondition condition, CancellationToken ct)
    {
        try
        {
            return new StatusConditionStanding(condition.Name, await condition.IsOpenAsync(ct), SourceUnreadable: false, NotMeasurable: false);
        }
        catch (DbException)
        {
            return Unreadable(condition);
        }
        catch (IOException)
        {
            return Unreadable(condition);
        }
        catch (UnauthorizedAccessException)
        {
            return Unreadable(condition);
        }
        catch (JsonException)
        {
            return Unreadable(condition);
        }
        catch (FormatException)
        {
            return Unreadable(condition);
        }
        catch (InvalidOperationException)
        {
            return Unreadable(condition);
        }
        catch (TimeoutException)
        {
            return Unreadable(condition);
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return Unreadable(condition);
        }
    }

    private static StatusConditionStanding Unreadable(IStatusCondition condition) =>
        new(condition.Name, IsOpen: true, SourceUnreadable: true, NotMeasurable: false);
}
