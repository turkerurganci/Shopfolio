namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// A result family of a job that calls an outside service (09 11.1 item 2). Each family says, in one pure function, which of its classes a call
/// without a classified answer is (08 3.1 rules 3 and 4, the paragraph under the table): the timeout, the unreachable service and the answer
/// that is not in the service's table. The layer's runner turns every such call into this class, so none of them leaks as an exception.
/// </summary>
internal interface IExternalCallResult<TSelf>
    where TSelf : IExternalCallResult<TSelf>
{
    static abstract TSelf FromFailure(ExternalCallFailure failure);
}
