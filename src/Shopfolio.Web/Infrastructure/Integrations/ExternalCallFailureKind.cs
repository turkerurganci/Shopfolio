namespace Shopfolio.Infrastructure.Integrations;

/// <summary>Why a call ended without an answer the layer could classify (08 3.1 rules 3 and 4, the paragraph under the table).</summary>
internal enum ExternalCallFailureKind
{
    /// <summary>The call's limit passed: a call whose result is unknown, not a failure (08 3.1 rule 3).</summary>
    TimedOut,

    /// <summary>The service could not be reached: name resolution, connection, TLS, or the transport broke off.</summary>
    Unreachable,

    /// <summary>The service answered with something the service's own table does not list (08 3.1, "tabloda olmayan her yanıt").</summary>
    Unrecognized,
}
