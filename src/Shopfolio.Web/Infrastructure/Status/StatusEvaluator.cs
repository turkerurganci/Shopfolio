using System.Data.Common;
using System.Text.Json;

namespace Shopfolio.Infrastructure.Status;

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
            if (await IsOpenAsync(condition, ct))
            {
                open.Add(condition.Name);
            }
        }

        return open;
    }

    private static async Task<bool> IsOpenAsync(IStatusCondition condition, CancellationToken ct)
    {
        try
        {
            return await condition.IsOpenAsync(ct);
        }
        catch (DbException)
        {
            return true;
        }
        catch (IOException)
        {
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return true;
        }
        catch (JsonException)
        {
            return true;
        }
        catch (FormatException)
        {
            return true;
        }
        catch (InvalidOperationException)
        {
            return true;
        }
        catch (TimeoutException)
        {
            return true;
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return true;
        }
    }
}
