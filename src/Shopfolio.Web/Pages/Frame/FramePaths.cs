namespace Shopfolio.Pages.Frame;

/// <summary>The addresses and names the storefront frame points to (07 2.1.2, §3; T537): the fixed pages, the account pages and the three forms the frame starts. Written here once.</summary>
internal static class FramePaths
{
    /// <summary>UC-01: the home page, the way home of the "page not found" page (T504).</summary>
    public const string Home = "/";

    /// <summary>UC-22: the payment step.</summary>
    public const string Checkout = "/odeme";

    /// <summary>UC-19: the cart.</summary>
    public const string Cart = "/sepet";

    /// <summary>UC-03: the search results; the word travels in <see cref="SearchParameter"/> (07 2.6.3).</summary>
    public const string Search = "/arama";

    /// <summary>The query parameter of the search word (07 2.6.3).</summary>
    public const string SearchParameter = "search";

    /// <summary>UC-54: the customer's sign-in page.</summary>
    public const string SignIn = "/giris";

    /// <summary>UC-47: the registration page (E-20).</summary>
    public const string Register = "/kayit";

    /// <summary>UC-52: the base of the verification link landing page (E-21); the link key follows it.</summary>
    public const string Verification = "/dogrulama";

    /// <summary>UC-77: the sign-out page; the form adds the handler.</summary>
    public const string SignOut = "/cikis";

    /// <summary>UC-31: the order tracking entry of a visitor.</summary>
    public const string OrderTracking = "/siparis-takibi";

    /// <summary>UC-63: the account page ("Hesabım").</summary>
    public const string Account = "/hesap";

    /// <summary>UC-74: the order history ("Siparişlerim").</summary>
    public const string AccountOrders = "/hesap/siparisler";

    /// <summary>UC-13: the FAQ page.</summary>
    public const string Faq = "/sss";

    /// <summary>UC-14: the contact page, the footer heading's link.</summary>
    public const string Contact = "/iletisim";

    /// <summary>UC-16: the privacy notice page.</summary>
    public const string PrivacyNotice = "/aydinlatma-metni";

    /// <summary>UC-17: the cookie policy page.</summary>
    public const string CookiePolicy = "/cerez-politikasi";

    /// <summary>UC-18: the process guide page.</summary>
    public const string ProcessGuide = "/islem-rehberi";

    /// <summary>The panel's start, the administrator strip's "Panele dön" (04 2.9.1).</summary>
    public const string Panel = "/panel";

    /// <summary>The footer link's text of the privacy notice (04 2.2.5).</summary>
    public const string PrivacyNoticeTitle = "Aydınlatma metni";

    /// <summary>The footer link's text of the cookie policy (04 2.2.5).</summary>
    public const string CookiePolicyTitle = "Çerez politikası";
}
