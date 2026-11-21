using System.Reflection;

using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure.Security;

namespace Shopfolio.UnitTests.Pages;

/// <summary>
/// The inventory of the pages that have a POST handler and what each does for the page the forgery check's 400 shows again (T604 D-1773; 07 2.5.4; 04 2.7.3.1; K-1627 (2)). The check answers
/// before the handler, so a page's reads have not run; <see cref="IReadsOnForgeryRefusal"/> is the one hook that asks a page for them. Every page with a POST handler is exactly one of:
/// it implements the hook; it reads in its handler selection (before the check, so the 400 page has its content already); it has its own read after the filters (the cart, the payment step; D-1190);
/// its GET reads nothing; or it is on the closed list of the pages still waiting for their own safe read - a page that leaves the list when it is done, and a page cannot join it.
/// </summary>
public sealed class ForgeryRefusalReadInventoryTests
{
    /// <summary>
    /// The pages that read after the filters on their own (D-1190): the page shown again needs the sent form - a handler argument the hook does not see - so the page's own wrapper reads after the 400
    /// (D-1843). Each has an Integration test of its 400 page.
    /// </summary>
    private static readonly string[] OwnReadPages =
    [
        "Shopfolio.Pages.Account.AddressesModel",
        "Shopfolio.Pages.Account.ProfileModel",
        "Shopfolio.Pages.Account.ReauthenticationModel",
        "Shopfolio.Pages.ProductModel",
        "Shopfolio.Pages.Storefront.CartModel",
        "Shopfolio.Pages.Storefront.CheckoutModel",
        "Shopfolio.Pages.Storefront.ContactModel",
        "Shopfolio.Pages.Storefront.PasswordResetModel",
        "Shopfolio.Pages.Storefront.RegisterModel",
        "Shopfolio.Pages.Storefront.SignInModel",
    ];

    /// <summary>
    /// The pages whose GET read depends on a query value, a single-use value, the session's permission or the sent values (the contact form's typed text), so the read for the 400 page needs its own
    /// safe split rather than the GET's call: D-1773 stays open for them. The list only shrinks.
    /// </summary>
    private static readonly string[] WaitingPages =
    [
        "Shopfolio.Pages.Panel.ContentFormModel",
        "Shopfolio.Pages.Panel.CouponsModel",
        "Shopfolio.Pages.Panel.InviteModel",
        "Shopfolio.Pages.Panel.MembersModel",
        "Shopfolio.Pages.Panel.MyAccountModel",
        "Shopfolio.Pages.Panel.OrderDetailModel",
        "Shopfolio.Pages.Panel.ProductFormModel",
        "Shopfolio.Pages.Panel.ReauthModel",
        "Shopfolio.Pages.Panel.RequestDetailModel",
        "Shopfolio.Pages.Panel.ResetModel",
        "Shopfolio.Pages.Panel.RevertModel",
        "Shopfolio.Pages.Panel.SignInModel",
        "Shopfolio.Pages.Panel.VerifyModel",
    ];

    [Fact]
    public void EveryPageWithAPostHandler_HasTheHook_ReadsBeforeTheCheck_ReadsOnItsOwn_ReadsNothing_OrIsOnTheClosedWaitingList_D1773()
    {
        List<string> unaccounted = [];
        foreach (Type page in PagesWithPostHandler())
        {
            string name = page.FullName ?? page.Name;
            if (HasHook(page)
                || DeclaresOverride(page, nameof(PageModel.OnPageHandlerSelectionAsync))
                || (OwnReadPages.Contains(name, StringComparer.Ordinal) && DeclaresOverride(page, nameof(PageModel.OnPageHandlerExecutionAsync)))
                || !HasGetRead(page)
                || WaitingPages.Contains(name, StringComparer.Ordinal))
            {
                continue;
            }

            unaccounted.Add(name);
        }

        Assert.Empty(unaccounted);
    }

    [Fact]
    public void TheWaitingList_HoldsOnlyPagesWithAPostHandlerAndAGetRead_AndNoPageThatHasTheHook_SoItOnlyShrinks_D1773()
    {
        Dictionary<string, Type> pages = PagesWithPostHandler().ToDictionary(t => t.FullName ?? t.Name, t => t, StringComparer.Ordinal);

        foreach (string waiting in WaitingPages)
        {
            Assert.True(pages.TryGetValue(waiting, out Type? page), $"{waiting} is not a page with a POST handler: remove it from the waiting list.");
            Assert.True(HasGetRead(page), $"{waiting} reads nothing in its GET: it needs no hook, remove it from the waiting list.");
            Assert.False(HasHook(page), $"{waiting} has the hook: remove it from the waiting list.");
            Assert.False(DeclaresOverride(page, nameof(PageModel.OnPageHandlerSelectionAsync)), $"{waiting} reads in its handler selection: remove it from the waiting list.");
        }

        Assert.Equal(WaitingPages.Length, WaitingPages.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void ThePagesWithTheHook_AreTheOnesThatReadOnlyInTheirGet_AndTheOwnReadPagesExist_D1773()
    {
        string[] hooked = [.. PagesWithPostHandler().Where(HasHook).Select(t => t.Name).Order(StringComparer.Ordinal)];

        Assert.Equal(["AdminAccountsModel", "CategoriesModel", "ContentListModel", "EmailRevertModel", "VerificationModel"], hooked);
        Assert.All(OwnReadPages, name => Assert.Contains(name, PagesWithPostHandler().Select(t => t.FullName)));
    }

    private static IEnumerable<Type> PagesWithPostHandler() =>
        typeof(IReadsOnForgeryRefusal).Assembly.GetTypes()
            .Where(t => typeof(PageModel).IsAssignableFrom(t) && !t.IsAbstract && t.Namespace?.StartsWith("Shopfolio.Pages", StringComparison.Ordinal) == true)
            .Where(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Any(m => m.Name.StartsWith("OnPost", StringComparison.Ordinal)))
            .OrderBy(t => t.FullName, StringComparer.Ordinal);

    private static bool HasHook(Type page) => typeof(IReadsOnForgeryRefusal).IsAssignableFrom(page);

    // A GET that reads: an OnGetAsync (the trivial ones are a synchronous OnGet that returns the page or a not found).
    private static bool HasGetRead(Type page) =>
        page.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Any(m => m.Name == "OnGetAsync");

    private static bool DeclaresOverride(Type page, string method) =>
        page.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Any(m => m.Name == method);
}
