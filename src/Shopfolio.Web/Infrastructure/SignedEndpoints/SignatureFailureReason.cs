namespace Shopfolio.Infrastructure.SignedEndpoints;

/// <summary>Why a request to a signed machine endpoint is not a notification (08 3.2 principle 2); the only detail its log line carries.</summary>
internal enum SignatureFailureReason
{
    /// <summary>The body is not a form the pipeline can read (wrong type, malformed, a field given twice), so no signature can hold.</summary>
    BodyUnreadable,

    /// <summary>The signature is missing or does not match the one the link layer computes.</summary>
    SignatureInvalid,
}
