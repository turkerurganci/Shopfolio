namespace Shopfolio.Infrastructure.Operations;

/// <summary>The outcome of the POST handler's check of an operation (07 8.1.2, 8.1.3).</summary>
public enum OperationDecision
{
    /// <summary>The operation is open and the request fits: it is applied.</summary>
    Allowed,

    /// <summary>The operation is not open now (or it never was, for a forged or stale request): nothing is written.</summary>
    Closed,

    /// <summary>The operation is open but the requested quantity is more than the open quantity now: the whole request is refused.</summary>
    QuantityExceedsOpen,
}
