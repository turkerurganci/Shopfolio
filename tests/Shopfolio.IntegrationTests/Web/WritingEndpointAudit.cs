using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;

using Shopfolio.Infrastructure.Security;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>One mapped endpoint as the audit of the forgery protection sees it.</summary>
/// <param name="Pattern">The route pattern.</param>
/// <param name="IsPage">True for a Razor page endpoint (the page filter is what protects it).</param>
/// <param name="HasPolicy">The page carries the protection's policy marker.</param>
/// <param name="HasIgnoreAttribute">The page carries the framework's ignore attribute.</param>
/// <param name="AcceptsWrites">The endpoint answers a writing method (a POST, PUT, PATCH or DELETE, or any method when it names none).</param>
/// <param name="SwitchedOff">The endpoint's metadata says its anti-forgery check is off (the signed endpoint's switch).</param>
public sealed record AuditedEndpoint(string Pattern, bool IsPage, bool HasPolicy, bool HasIgnoreAttribute, bool AcceptsWrites, bool SwitchedOff);

/// <summary>
/// The audit of the mapped endpoints for T209 KK6 (07 2.5.5; 09 15.2 items 1, 2): every page endpoint carries the protection and no page switches it
/// off; a writing endpoint that is no page is on the closed list (<see cref="AntiforgeryExemptions"/>) and switched off exactly when it is on it.
/// The audit runs over plain records, so the tests can also show it red on endpoints made up for the purpose.
/// </summary>
public static class WritingEndpointAudit
{
    private static readonly string[] _writingMethods = ["POST", "PUT", "PATCH", "DELETE"];

    public static List<AuditedEndpoint> Of(IEnumerable<Endpoint> endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        List<AuditedEndpoint> result = [];
        foreach (RouteEndpoint endpoint in endpoints.OfType<RouteEndpoint>())
        {
            PageActionDescriptor? page = endpoint.Metadata.GetMetadata<PageActionDescriptor>();
            IHttpMethodMetadata? methods = endpoint.Metadata.GetMetadata<IHttpMethodMetadata>();
            bool writes = methods is null || methods.HttpMethods.Any(m => _writingMethods.Contains(m, StringComparer.OrdinalIgnoreCase));
            IFilterMetadata[] filters = page is null ? [] : [.. page.FilterDescriptors.Select(d => d.Filter)];
            result.Add(new AuditedEndpoint(
                "/" + (endpoint.RoutePattern.RawText ?? string.Empty).TrimStart('/'),
                page is not null,
                filters.OfType<ForgeryProtectionPolicy>().Any(),
                filters.OfType<Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryTokenAttribute>().Any(),
                page is not null || writes,
                endpoint.Metadata.GetMetadata<IAntiforgeryMetadata>() is { RequiresValidation: false }));
        }

        return result;
    }

    public static List<string> Violations(IEnumerable<AuditedEndpoint> endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        List<string> violations = [];
        foreach (AuditedEndpoint e in endpoints)
        {
            if (e.IsPage)
            {
                if (!e.HasPolicy)
                {
                    violations.Add($"{e.Pattern}: a page without the protection's policy marker");
                }

                if (e.HasIgnoreAttribute)
                {
                    violations.Add($"{e.Pattern}: a page with the ignore attribute");
                }

                continue;
            }

            bool listed = AntiforgeryExemptions.Contains(e.Pattern);
            if (e.AcceptsWrites && !listed)
            {
                violations.Add($"{e.Pattern}: a writing endpoint outside the closed list");
            }

            if (listed && !e.SwitchedOff)
            {
                violations.Add($"{e.Pattern}: on the closed list but its check is not switched off");
            }

            if (!listed && e.SwitchedOff)
            {
                violations.Add($"{e.Pattern}: its check is switched off but it is not on the closed list");
            }
        }

        return violations;
    }
}
