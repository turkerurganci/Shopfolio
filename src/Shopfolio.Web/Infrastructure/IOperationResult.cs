namespace Shopfolio.Infrastructure;

/// <summary>
/// The common face of the result types (09 9.1 item 3; K-1335): the transaction runner reads only whether the work was refused. A refusal is a
/// return value, not an exception; the runner rolls the transaction back and hands the result to the caller as it is.
/// </summary>
internal interface IOperationResult
{
    bool IsRejected { get; }
}
