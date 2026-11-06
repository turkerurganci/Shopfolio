using System.Net;

using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// What an attempt is counted on: an axis and its value (09 15.4 item 5; 06 4.43). The value is made here and nowhere else - the e-mail is normalized
/// with the identity normalization (09 9.4 item 1), the IP is the axis text of the client address (<see cref="ClientAddress.Axis"/>), a cart or a
/// recognized browser mark is the record's identifier - so every row of the table, whoever wrote it, holds the same text for the same subject. The
/// attempt record takes only a subject (<see cref="AttemptRecord.Create"/>) and <see cref="AttemptCounter"/> is the only code that writes the rows;
/// a source scan keeps both so (D-64).
/// </summary>
internal readonly record struct AttemptSubject
{
    internal AttemptSubject(AttemptAxis axis, string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Axis = axis;
        Value = value;
    }

    public AttemptAxis Axis { get; }

    /// <summary>The value written to the row: a normalized e-mail, an IP axis text, or a cart or recognized browser identifier.</summary>
    public string Value { get; }

    /// <summary>The e-mail axis: the address typed (or the account's), normalized.</summary>
    public static AttemptSubject Email(string email)
    {
        ArgumentNullException.ThrowIfNull(email);
        return new AttemptSubject(AttemptAxis.Email, TextNormalizer.NormalizeIdentity(email));
    }

    /// <summary>The IP axis of an address (an IPv6 address by its /64 prefix).</summary>
    public static AttemptSubject Ip(IPAddress? address) => new(AttemptAxis.Ip, ClientAddress.Axis(address));

    /// <summary>The IP axis of the request's client address.</summary>
    public static AttemptSubject Ip(HttpContext context) => Ip(ClientAddress.Of(context));

    /// <summary>The cart axis: the cart's identifier.</summary>
    public static AttemptSubject Cart(Guid cartId) => new(AttemptAxis.Cart, cartId.ToString("D"));

    /// <summary>The recognized browser axis: the identifier of the mark's record.</summary>
    public static AttemptSubject RecognizedBrowser(Guid markId) => new(AttemptAxis.RecognizedBrowser, markId.ToString("D"));
}
