using Microsoft.AspNetCore.Antiforgery;

using Shopfolio.Infrastructure.Errors;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The mark of an endpoint that takes the forgery token from the <c>RequestVerificationToken</c> header and checks it itself (09 15.7 item 4; 07 2.5.4): the two POSTs
/// of the chunked upload. The check is <see cref="HeaderForgeryTokenFilter"/>; the endpoint's path is a member of
/// <see cref="AntiforgeryExemptions.HeaderTokenPaths"/>, and the audit of the mapped endpoints reads this mark.
/// </summary>
internal sealed class HeaderForgeryTokenMetadata
{
    public static HeaderForgeryTokenMetadata Instance { get; } = new();

    private HeaderForgeryTokenMetadata()
    {
    }
}

/// <summary>
/// The endpoint filter of the header token (09 15.7 item 4): the request is valid only when the header's value and the cookie make a pair
/// (<see cref="IAntiforgery.IsRequestValidAsync"/>); otherwise 400 with no body, one rejection line with the code and nothing written. The framework's
/// <c>UseAntiforgery</c> middleware is not in the pipeline, so the check is not left to it. The filter runs after the authorization (the session first, 07 4.31.2).
/// </summary>
internal sealed class HeaderForgeryTokenFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        HttpContext http = context.HttpContext;
        IAntiforgery antiforgery = http.RequestServices.GetRequiredService<IAntiforgery>();
        if (!await antiforgery.IsRequestValidAsync(http))
        {
            http.RequestServices.GetRequiredService<RejectionLog>().Rejected(ErrorCode.AntiforgeryValidationFailed);
            return Results.StatusCode(StatusCodes.Status400BadRequest);
        }

        return await next(context);
    }
}

internal static class HeaderForgeryTokenRegistration
{
    /// <summary>
    /// Puts the header token check on the endpoint and marks it. The path of the endpoint is a member of <see cref="AntiforgeryExemptions.HeaderTokenPaths"/>
    /// (a source scan and the audit keep it so; D-131).
    /// </summary>
    public static TBuilder RequireHeaderForgeryToken<TBuilder>(this TBuilder builder)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.WithMetadata(HeaderForgeryTokenMetadata.Instance);
        EndpointFilterExtensions.AddEndpointFilter<TBuilder, HeaderForgeryTokenFilter>(builder);
        return builder;
    }
}
