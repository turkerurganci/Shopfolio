
namespace Shopfolio.Infrastructure.Errors;

/// <summary>
/// The four states an unexpected error falls into (09 8.1 item 3; 05 10.3.1-10.3.5). The state is decided by the kind of the request and the
/// mark of the operation, never by the type of the exception (09 8.1 item 1).
/// </summary>
internal enum ErrorState
{
    /// <summary>A GET: 500 and an error page (05 10.3.2).</summary>
    PageLevel,

    /// <summary>A POST: 500, the page again with the sent values and a lasting message; the open transaction is rolled back (05 10.3.3).</summary>
    OperationLevel,

    /// <summary>A POST of a marked operation: 303 to the screen that shows the result; "not done" is not shown (05 10.3.4).</summary>
    OutcomeUncertain,

    /// <summary>A machine endpoint: an empty 500, no screen (05 10.3.5).</summary>
    MachineEndpoint,
}

/// <summary>The table of 09 8.1 item 3 as one pure function: the request method, whether the endpoint is a machine endpoint and whether the operation is marked.</summary>
internal static class ErrorClassifier
{
    public static ErrorState Classify(string method, bool machineEndpoint, bool markedOperation)
    {
        ArgumentNullException.ThrowIfNull(method);
        if (machineEndpoint)
        {
            return ErrorState.MachineEndpoint;
        }

        if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method))
        {
            return ErrorState.PageLevel;
        }

        return markedOperation ? ErrorState.OutcomeUncertain : ErrorState.OperationLevel;
    }

    /// <summary>The code the line of a state records (the "recorded code" column of 09 8.1 item 3; 07 6.2.1, 6.4.2): the machine endpoint records the operation-level code.</summary>
    public static ErrorCode CodeOf(ErrorState state) => state switch
    {
        ErrorState.PageLevel => ErrorCode.UnexpectedPageError,
        ErrorState.OutcomeUncertain => ErrorCode.OutcomeUncertain,
        _ => ErrorCode.UnexpectedOperationError,
    };
}

/// <summary>
/// The mark of a machine endpoint (09 8.1 item 3): endpoint metadata, written where the endpoint is mapped. The distinction is never derived from the
/// request path. The signed endpoints (<c>MapSignedEndpoint</c>), the liveness and the status endpoints
/// carry it too.
/// </summary>
internal sealed class MachineEndpointMetadata
{
    public static MachineEndpointMetadata Instance { get; } = new();

    private MachineEndpointMetadata()
    {
    }
}

/// <summary>
/// The mark of a machine endpoint that a browser opens as a page (T504; 07 UC-06; 09 15.7 item 1): its 404 is the "page not found" page E-06 and not an empty answer - the signed
/// download address is the one such endpoint. Every other machine endpoint answers a 404 without a body (07 2.7.2).
/// </summary>
internal sealed class NotFoundPageMetadata
{
    public static NotFoundPageMetadata Instance { get; } = new();

    private NotFoundPageMetadata()
    {
    }
}

/// <summary>
/// Marks a page handler whose outcome is uncertain when anything goes wrong (09 8.1 item 4; 07 6.4.2): wherever the error falls - inside the operation,
/// after the commit, while the answer is made - the answer is a 303 to the screen that shows the result. The set of marked handlers equals the list of
/// 07 6.4.2 (the equality check is part of the consistency tests of the tasks that add a marked handler).
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
internal sealed class OutcomeUncertainAttribute : Attribute
{
    public OutcomeUncertainAttribute(string operation)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(operation);
        Operation = operation;
    }

    /// <summary>The use case of the operation in 07 6.4.2 (for example <c>UC-27</c>); the log line writes it as <c>operation</c> (05 10.3.6).</summary>
    public string Operation { get; }
}

/// <summary>
/// Implemented by the page model of a marked operation: the one method that chooses the screen showing the result (09 8.1 item 4 - for example the
/// member's order history or the guest's order tracking after an order confirmation). A page model without it sends the browser to its own address.
/// </summary>
internal interface IOutcomeTarget
{
    /// <summary>A path of this site that starts with one slash.</summary>
    string OutcomeScreen();
}

/// <summary>The marked operation of the running request, found when the handler is selected and read by the layer .</summary>
internal sealed record OperationMark(string Operation, IOutcomeTarget? Target)
{
    public const string ItemKey = "Shopfolio.OperationMark";
}
