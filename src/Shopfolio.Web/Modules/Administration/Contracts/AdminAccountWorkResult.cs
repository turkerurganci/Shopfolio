using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Administration.Contracts;

/// <summary>
/// The outcome of a management command's work on one administrator account (T389; 05 8.5.3, 8.5.4): done, or refused because the account was gone by the time the work took its row lock.
/// A refusal writes nothing and is rolled back by the transaction runner (09 9.1 item 3). It carries no value the caller gave.
/// </summary>
public sealed class AdminAccountWorkResult : IOperationResult
{
    private AdminAccountWorkResult(bool isDone)
    {
        IsDone = isDone;
    }

    public bool IsDone { get; }

    public bool IsRejected => !IsDone;

    internal static AdminAccountWorkResult Done() => new(true);

    internal static AdminAccountWorkResult AccountMissing() => new(false);
}
