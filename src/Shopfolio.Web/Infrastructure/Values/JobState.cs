namespace Shopfolio.Infrastructure.Values;

/// <summary>The closed values of JobState (06 5.4.5). Only the code names live here; the behavior of a value lives in the rule that owns it (06 5.1.3).</summary>
public enum JobState
{
    Pending,
    Completed,
    Failed,
}
