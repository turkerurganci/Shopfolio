namespace Shopfolio.Infrastructure.Notices;

/// <summary>One row of the table of 07 6.3.1: the endpoint that writes, the notice it redirects with and the screen it redirects to.</summary>
internal sealed record NoticeRoute(NoticeCode Code, string Source, string Target);

/// <summary>
/// The table of the short-lived notices (07 6.3.1; 11 T214 criterion 6): which endpoint redirects with which code, to which screen. The writing endpoints
/// are the tasks of the screens (F3, F5); each takes its code and its target from here, so the pair is written once. A consistency test keeps the table equal
/// to 07 6.3.1 in both directions - a source or a target the document does not name, and a pair the document names that is missing here, both fail.
/// The result states (07 6.3.2) and the reasons (07 6.3.3) name the screen in the tables of their own endpoints and are not listed here.
/// </summary>
internal static class NoticeRoutes
{
    private static readonly NoticeRoute[] _routes =
    [
        new(NoticeCode.CartItemAdded, "UC-05", "UC-04"),

        new(NoticeCode.Saved, "UC-35", "UC-33"),
        new(NoticeCode.Saved, "UC-35", "UC-34"),
        new(NoticeCode.Saved, "UC-64", "UC-63"),
        new(NoticeCode.Saved, "UC-71", "UC-70"),
        new(NoticeCode.Saved, "UC-72", "UC-70"),
        new(NoticeCode.Saved, "UC-114", "UC-88"),
        new(NoticeCode.Saved, "UC-118", "UC-89"),
        new(NoticeCode.Saved, "UC-119", "UC-89"),
        new(NoticeCode.Saved, "UC-122", "UC-90"),
        new(NoticeCode.Saved, "UC-158", "UC-96"),
        new(NoticeCode.Saved, "UC-162", "UC-97"),
        new(NoticeCode.Saved, "UC-163", "UC-98"),
        new(NoticeCode.Saved, "UC-164", "UC-99"),
        new(NoticeCode.Saved, "UC-167", "UC-100"),
        new(NoticeCode.Saved, "UC-168", "UC-101"),
        new(NoticeCode.Saved, "UC-169", "UC-101"),
        new(NoticeCode.Saved, "UC-170", "UC-101"),
        new(NoticeCode.Saved, "UC-171", "UC-101"),
        new(NoticeCode.Saved, "UC-172", "UC-101"),
        new(NoticeCode.Saved, "UC-173", "UC-102"),
        new(NoticeCode.Saved, "UC-178", "UC-104"),

        new(NoticeCode.InvitationSent, "UC-175", "UC-103"),

        new(NoticeCode.ContactRequestClosed, "UC-155", "UC-94"),
        new(NoticeCode.ContactRequestReopened, "UC-156", "UC-94"),
    ];

    /// <summary>Every row of the table.</summary>
    public static IReadOnlyList<NoticeRoute> All => _routes;

    /// <summary>The code the endpoint redirects with; the endpoint has exactly one in 07 6.3.1.</summary>
    public static NoticeCode CodeOf(string source) =>
        _routes.Where(r => r.Source == source).Select(r => r.Code).Distinct().Single();

    /// <summary>The screens the endpoint redirects to - one, or two where the same write belongs to either screen (UC-35).</summary>
    public static IReadOnlyList<string> TargetsOf(string source) =>
        [.. _routes.Where(r => r.Source == source).Select(r => r.Target)];
}
