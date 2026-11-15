using Microsoft.AspNetCore.Http;

namespace Shopfolio.Infrastructure.Binding;

/// <summary>
/// What a query model's value means when its parameter is sent more than once (07 2.6.6; T538 K-1616 (2)). The binder takes the first value of a repeated parameter, while the reading
/// by hand before the query models joined them with a comma and so matched no closed list: the screens that read a closed value (the re-authentication screens' operation and single-use
/// value) keep that old answer - a parameter sent twice is no value - through this one helper, so every such screen answers the same way.
/// </summary>
internal static class QueryValues
{
    /// <summary>The bound value of a parameter the request sent once (or not at all); <c>null</c> when the request sent it more than once.</summary>
    public static string? SentOnce(HttpRequest request, string name, string? bound)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrEmpty(name);
        return request.Query[name].Count > 1 ? null : bound;
    }
}
