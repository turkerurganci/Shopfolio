namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// Google sign-in behind the link layer (08 3.1 rule 1, 2.3; 05 3.6.1; MK-17; 09 11.1 item 1, 11.3): one job, verifying the identity - the code
/// exchange and the identity token's verification. A call the user waits for (15 seconds per call), not retried: the user starts the flow
/// again. A spent authorization code is never sent again.
/// </summary>
internal interface IGoogleIdentityGateway
{
    Task<GoogleIdentityResult> VerifyIdentityAsync(IdentityCodeExchange exchange, CancellationToken ct);
}

/// <summary>What the return of the sign-in flow hands the layer: the one-time authorization code and the flow's proof key verifier (08 2.3).</summary>
internal sealed record IdentityCodeExchange(string AuthorizationCode, string CodeVerifier);
