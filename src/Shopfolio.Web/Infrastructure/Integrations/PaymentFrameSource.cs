namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// Where the payment provider's hosted form is loaded from (08 2.1.4; 05 6.5.4; 07 UC-29): <see cref="Origin"/> is the one address the payment page's frame permission
/// (<c>frame-src</c>) names besides <c>'self'</c>, and <see cref="AddressRoot"/> is the root every frame address of a started payment begins with. The provider's
/// implementation registers the one instance, so the content security policy and the payment page never write the provider's address themselves (08 3.1 rule 1).
/// A host without the link layer (the command process) has none, and no response there widens the policy.
/// </summary>
internal sealed record PaymentFrameSource
{
    public PaymentFrameSource(string origin, string addressRoot)
    {
        ArgumentException.ThrowIfNullOrEmpty(origin);
        ArgumentException.ThrowIfNullOrEmpty(addressRoot);
        if (!addressRoot.StartsWith(origin + "/", StringComparison.Ordinal))
        {
            throw new ArgumentException("The frame's address root is under the frame's origin.", nameof(addressRoot));
        }

        Origin = origin;
        AddressRoot = addressRoot;
    }

    /// <summary>The scheme and host of the provider's form, without a path (<c>frame-src</c>'s source expression).</summary>
    public string Origin { get; }

    /// <summary>The root of the frame's address; a started payment's address is this root and the payment's own part.</summary>
    public string AddressRoot { get; }

    /// <summary>Whether <paramref name="address"/> is a frame address of this source: under <see cref="AddressRoot"/>, nothing else.</summary>
    public bool Holds(Uri address)
    {
        ArgumentNullException.ThrowIfNull(address);
        return address.IsAbsoluteUri && address.AbsoluteUri.StartsWith(AddressRoot, StringComparison.Ordinal) && address.AbsoluteUri.Length > AddressRoot.Length;
    }
}
