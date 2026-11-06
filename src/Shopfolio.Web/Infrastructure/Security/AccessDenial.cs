using Microsoft.AspNetCore.Mvc;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The one answer to a request that cannot reach a record (07 2.3.3; 05 6.2.4): a record that belongs to someone else, a record that does not exist, a draft, a
/// deleted record and a wrong or renewed access key all get the same 404 - never a 403 and never a second answer, so the response does not say whether the
/// record exists, belongs to another account or was deleted. A POST that gets it has written nothing. The body of the 404 is the "page not found" page E-06 (T504,
/// 07 UC-06): the status code is set here, once, and the page that shows it is the one error page of that task. A page or a handler returns this result and no
/// status of its own for a denied access.
/// </summary>
internal static class AccessDenial
{
    /// <summary>The status of every denied access (07 2.3.3).</summary>
    public const int Status = StatusCodes.Status404NotFound;

    /// <summary>The answer to a request that fails the ownership check or asks for a record that does not exist.</summary>
    public static IActionResult NotFound() => new NotFoundResult();
}
