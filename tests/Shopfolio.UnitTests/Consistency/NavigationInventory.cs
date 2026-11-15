namespace Shopfolio.UnitTests.Consistency;

/// <summary>What proves a row of <c>04 3</c> in the code.</summary>
internal enum NavigationEvidenceKind
{
    /// <summary>A link of a view: the file and the expression of its <c>href</c> (<see cref="NavigationSites"/>).</summary>
    Link,

    /// <summary>A redirect call: the file and the expression of its address.</summary>
    Redirect,

    /// <summary>The way out of an empty list: the file and the expression of the address a call gives the shared empty-state component (<c>EmptyStates.WithExit</c>, <c>new EmptyState</c>); the component's own link is no evidence.</summary>
    EmptyExit,

    /// <summary>A text the file holds (a form's action, a constant of the frame).</summary>
    Text,

    /// <summary>A page whose <c>@page</c> route starts with the text (or equals it); a text ending with <c>*</c> is a prefix.</summary>
    Page,
}

internal sealed record NavigationEvidence(NavigationEvidenceKind Kind, string File, string Text);

/// <summary>One row of <c>04 3</c> as the code has it: the id, the source and the target pins and the evidence. A pin is the set of the screens the cell names (<c>E-04,E-05</c>) and the texts it must hold (<c>~text</c>).</summary>
internal sealed record NavigationEntry(string Id, string From, string To, IReadOnlyList<NavigationEvidence> Evidence);

/// <summary>A site of the code that stays on its screen or is no screen (the document's own rule, <c>04 3</c> introduction), with the reason. It is no list of known differences: a link that leads to another screen is a row.</summary>
internal sealed record NavigationExemption(NavigationSiteKind Kind, string File, string Key, string Reason);

/// <summary>
/// The code side of the navigation map (T546; 04 3; 09 21.4 item 1): each row of the tables is claimed by the places of the code that carry it - the link of a view, the redirect of a handler, the route of a page
/// - and every link and redirect of the web project is either claimed by a row or is a place that stays on its screen. A row that has no claim, a claim that is not in the code, a link or redirect that
/// no row and no exemption names, an exemption that is not in the code: each makes <see cref="NavigationCheck"/> report. The exemptions are only the places that stay on their screen or are no screen (the
/// document's own rule, 04 3 introduction: sorting, paging, in-row editing and the confirmation window are no transitions); every link that leads to another screen is a row, also the ones the screen
/// specifications of 04 5 and 9 write (T546 correction 1; K-1617). There is no list of known differences and no open item.
/// </summary>
internal static class NavigationInventory
{
    private const string Header = "Shared/_StorefrontHeader.cshtml";
    private const string Footer = "Shared/_StorefrontFooter.cshtml";
    private const string PanelLayout = "Shared/_PanelLayout.cshtml";
    private const string PanelBuilder = "Frame/PanelFrameBuilder.cs";
    private const string SessionHandler = "!Infrastructure/Security/SessionAuthenticationHandler.cs";

    public static IReadOnlyList<NavigationEntry> Entries { get; } =
    [
        // 04 3.1: the storefront's frame.
        Row("3.1.1", "~Duyuru şeridi", "~Firmanın girdiği adres", L("Shared/_StorefrontLayout.cshtml", "@announcement.LinkUrl")),
        Row("3.1.2", "~Üst bölüm — logo", "E-01", L(Header, "/")),
        Row("3.1.3", "~Üst bölüm — arama", "E-03", X(Header, "action=\"@search.Path\"")),
        Row("3.1.4", "~Üst bölüm — hesap alanı (oturum yok)", "E-22", L(Header, "@Model.Account.SignInPath")),
        Row("3.1.5", "~Üst bölüm — hesap alanı (oturum yok)", "E-15", L(Header, "@Model.Account.OrderTrackingPath")),
        Row("3.1.6", "~Üst bölüm — hesap alanı (üye)", "E-25", L(Header, "@Model.Account.AccountPath")),
        Row("3.1.7", "~Üst bölüm — hesap alanı (üye)", "E-27", L(Header, "@Model.Account.OrdersPath")),
        Row("3.1.8", "~Üst bölüm — hesap alanı (üye)", "~§3.6.4", X(Header, "?handler=SignOut"), R("Storefront/SignOut.cshtml.cs", "ReturnPath.AfterSignOut(returnUrl)")),
        Row("3.1.9", "~Üst bölüm — sepet", "E-12", L(Header, "@Model.Cart.Path")),
        Row("3.1.10", "E-13", "E-01,E-12", L(Header, "/"), L(Header, "@Model.Cart.Path"), X(Header, "site-header--checkout")),
        Row("3.1.11", "~Menü — ", "E-02", L("Shared/_MenuTree.cshtml", "@node.Path")),
        Row("3.1.12", "~Menü — Hizmetlerimiz", "E-07", L(Header, "@item.Path"), X("!Modules/Settings/Contracts/HomepageSettingRecords.cs", "Offerings = \"Hizmetlerimiz\""), X("!Modules/Settings/Contracts/HomepageSettingRecords.cs", "PortfolioItems = \"Referanslarımız\"")),
        Row("3.1.13", "~Menü — Hakkımızda", "E-08", L(Header, "@item.Path"), X("!Modules/Settings/Contracts/HomepageSettingRecords.cs", "AboutPage = \"Hakkımızda\"")),
        Row("3.1.14", "~Menü — SSS", "E-09", L(Header, "@item.Path"), X("!Modules/Settings/Contracts/HomepageSettingRecords.cs", "FaqItems = \"SSS\"")),
        Row("3.1.15", "~Menü — ", "E-08", L(Header, "@item.Path")),
        Row("3.1.16", "~Menü — İletişim", "E-10", L(Header, "@item.Path"), X("!Modules/Settings/Contracts/HomepageSettingRecords.cs", "Contact = \"İletişim\"")),
        Row("3.1.17", "~Üst bölümün ince satırı ve altbilgi", "~Platformun kendi sayfası (ürünün dışında)", L(Header, "@link.Url"), L(Header, "@whatsApp"), L(Footer, "@link.Url"), L(Footer, "@whatsApp")),
        Row("3.1.18", "~Altbilgi — genel sayfalar", "E-08", L(Footer, "@generalPage.Path")),
        Row("3.1.19", "~Altbilgi — aydınlatma metni", "E-11", L(Footer, "@legal.Path")),
        Row("3.1.20", "~Altbilgi — ETBİS doğrulama bandı", "~ETBİS'in doğrulama sayfası (ürünün dışında)", L(Footer, "@etbis"), L("Storefront/Contact.cshtml", "@etbis")),
        Row("3.1.21", "~Altbilgi — kimlik bloğu", "E-10", L(Footer, "@footer.ContactPath")),

        // 04 3.2: the panel's frame.
        Row("3.2.1", "~Ana sayfa", "E-31", L(PanelLayout, "@entry.Path"), X(PanelBuilder, "PanelFramePaths.Home")),
        Row("3.2.2", "~Siparişler", "E-36,E-37", L(PanelLayout, "@entry.Path"), X(PanelBuilder, "PanelFramePaths.Orders")),
        Row("3.2.3", "~Talepler", "E-37,E-38,E-39", L(PanelLayout, "@entry.Path"), X(PanelBuilder, "PanelFramePaths.Requests")),
        Row("3.2.4", "~Katalog / Ürünler", "E-32,E-33", L(PanelLayout, "@item.Path"), X(PanelBuilder, "PanelFramePaths.Products")),
        Row("3.2.5", "~Katalog / Kategoriler", "E-34", L(PanelLayout, "@item.Path"), X(PanelBuilder, "PanelFramePaths.Categories")),
        Row("3.2.6", "~Katalog / Kuponlar", "E-35", L(PanelLayout, "@item.Path"), X(PanelBuilder, "PanelFramePaths.Coupons")),
        Row("3.2.7", "~İçerik ve site / Kurumsal içerik", "E-41", L(PanelLayout, "@item.Path"), X(PanelBuilder, "PanelFramePaths.CorporateContent")),
        Row("3.2.8", "~İçerik ve site / Ana sayfa ve menü", "E-42", L(PanelLayout, "@item.Path"), X(PanelBuilder, "PanelFramePaths.HomeAndMenu")),
        Row("3.2.9", "~İçerik ve site / Marka", "E-43", L(PanelLayout, "@item.Path"), X(PanelBuilder, "PanelFramePaths.Brand")),
        Row("3.2.10", "~Üyeler", "E-40", L(PanelLayout, "@entry.Path"), X(PanelBuilder, "PanelFramePaths.Members")),
        Row("3.2.11", "~Raporlar / Satış özeti", "E-51", L(PanelLayout, "@item.Path"), X(PanelBuilder, "PanelFramePaths.SalesSummary")),
        Row("3.2.12", "~Raporlar / İşlem izi", "E-52", L(PanelLayout, "@item.Path"), X(PanelBuilder, "PanelFramePaths.OperationTrail")),
        Row("3.2.13", "~Raporlar / Dışa aktarma", "E-53", L(PanelLayout, "@item.Path"), X(PanelBuilder, "PanelFramePaths.Export")),
        Row("3.2.14", "~Ayarlar / Firma kimliği ve satış", "E-44", L(PanelLayout, "@item.Path"), X(PanelBuilder, "PanelFramePaths.FirmIdentityAndSales")),
        Row("3.2.15", "~Ayarlar / Ödeme yöntemleri", "E-45", L(PanelLayout, "@item.Path"), X(PanelBuilder, "PanelFramePaths.PaymentMethods")),
        Row("3.2.16", "~Ayarlar / Kargo", "E-46", L(PanelLayout, "@item.Path"), X(PanelBuilder, "PanelFramePaths.ShippingAndDefaults")),
        Row("3.2.17", "~Ayarlar / Yasal metinler", "E-47", L(PanelLayout, "@item.Path"), X(PanelBuilder, "PanelFramePaths.LegalTexts")),
        Row("3.2.18", "~Ayarlar / Yönetici hesapları", "E-49", L(PanelLayout, "@item.Path"), X(PanelBuilder, "PanelFramePaths.AdminAccounts")),
        Row("3.2.19", "~Üst satır — satış durumu göstergesi", "E-44", L(PanelLayout, "@frame.Sales.Path"), X(PanelBuilder, "PanelFramePaths.SalesStateSection")),
        Row("3.2.20", "~Üst satır", "E-01", L(PanelLayout, "@frame.SiteViewPath")),
        Row("3.2.21", "~Üst satır — yöneticinin adı", "E-50", L(PanelLayout, "@frame.Admin.AccountPath")),
        Row("3.2.22", "~Üst satır — yöneticinin adı", "E-29", X(PanelLayout, "?handler=SignOut"), R("Panel/SignOut.cshtml.cs", "SessionSchemes.AdminSignInPath")),

        // 04 3.3: the customer side.
        Row("3.3.1", "E-01", "E-04", L("Shared/_ProductCard.cshtml", "@Model.Path")),
        Row("3.3.2", "E-01", "E-02", L("Index.cshtml", "@link.Path")),
        Row("3.3.3", "E-01", "E-08", L("Index.cshtml", "@intro.Path")),
        Row("3.3.4", "E-01", "E-07,E-08", L("Shared/_HomeCardBlock.cshtml", "@Model.ListPath"), L("Shared/_ContentCard.cshtml", "@Model.Path")),
        Row("3.3.5", "E-02,E-03", "E-04", L("Shared/_ProductCard.cshtml", "@Model.Path")),
        Row("3.3.6", "E-02,E-04", "E-02", L("Category.cshtml", "@crumb.Path"), L("Product.cshtml", "@crumb.Path")),
        Row("3.3.7", "E-04", "E-04", R("Product.cshtml.cs", "VariantSelection.PathFor(productPath, variant)")),
        Row("3.3.8", "E-07", "E-08", L("Shared/_ContentCard.cshtml", "@Model.Path")),
        Row("3.3.9", "E-08", "E-10", L("Shared/_ContentRecordBody.cshtml", "@Model.ContactPath")),
        Row("3.3.10", "E-08", "E-04", X("Shared/_ContentRecordBody.cshtml", "related-products-heading")),
        Row("3.3.11", "E-10", "E-10", R("Storefront/Contact.cshtml.cs", "FramePaths.Contact"), L("Storefront/Contact.cshtml", "@FramePaths.Contact")),
        Row(
            "3.3.12",
            "E-02,E-03,E-05,E-06,E-07,E-09,E-12,E-27",
            "E-01,E-02",
            L("Shared/_NotFound.cshtml", "@Model.HomePath"),
            L("Shared/_NotFound.cshtml", "@category.Path"),
            E("Account/OrderHistory.cshtml.cs", "\"/\""),
            E("Storefront/Cart.cshtml", "\"/\""),
            E("Content/ContentListPages.cs", "\"/\""),
            E("Storefront/CategoryPage.cs", "\"/\""),
            E("Storefront/SearchPage.cs", "\"/\""),
            E("Storefront/FaqPage.cs", "FramePaths.Home"),
            L("Product.cshtml", "@FramePaths.Home"),
            L("Product.cshtml", "@category.Path"),
            L("Search.cshtml", "@category.Path")),
        Row("3.3.13", "E-12", "E-04", L("Storefront/Cart.cshtml", "/@AddressRules.ProductPrefix/@line.ProductSlug")),
        Row("3.3.14", "E-12", "E-13", L("Storefront/Cart.cshtml", "@FramePaths.Checkout")),
        Row("3.3.15", "E-13", "E-12", R("Storefront/Checkout.cshtml.cs", "FramePaths.Cart"), L(Header, "@Model.Cart.Path")),
        Row("3.3.16", "E-13", "E-13,E-22", X("Storefront/Checkout.cshtml", "CheckoutModel.LoginTarget"), R("Storefront/Checkout.cshtml.cs", "SignInPath")),
        Row("3.3.17", "E-13", "E-14", R("Storefront/Checkout.cshtml.cs", """result.IsGuestOrder ? ConfirmationKeys.AccessConfirmationPath(result.OrderAccessKey!) : ConfirmationKeys.MemberConfirmationPath(result.OrderNumber!)""")),
        Row("3.3.18", "E-14", "~Ürünün ödeme sayfası", X("Orders/ConfirmationScreenModels.cs", "PaymentFrameAddress")),
        Row("3.3.19", "E-14", "E-15,E-16", L("Orders/_ConfirmationScreen.cshtml", "@Model.MemberOrderPath"), L("Orders/_ConfirmationScreen.cshtml", "@ConfirmationPageModel.OrderTrackingPath")),
        Row("3.3.20", "E-27", "E-16", L("Account/OrderHistory.cshtml", "@row.Path")),
        Row("3.3.21", "E-15", "E-16", R("Orders/Tracking.cshtml.cs", "OrderRoutes.AccessPath(found.AccessKey)")),
        Row("3.3.22", "E-16", "E-17", L("Orders/_OrderPage.cshtml", "@link.Href"), X("Orders/OrderPageModels.cs", "OrderRoutes.ScreenPath(rootPath, \"iptal\")"), X("Orders/OrderPageModels.cs", "OrderRoutes.ScreenPath(rootPath, \"gecikme-feshi\")")),
        Row("3.3.23", "E-16", "E-18", L("Orders/_OrderPage.cshtml", "@link.Href"), X("Orders/OrderPageModels.cs", "OrderRoutes.ScreenPath(rootPath, \"cayma\")")),
        Row("3.3.24", "E-16", "E-19", L("Orders/_OrderPage.cshtml", "@link.Href"), X("Orders/OrderPageModels.cs", "OrderRoutes.ScreenPath(rootPath, \"ayip-talebi\")")),
        Row(
            "3.3.25",
            "E-17,E-18,E-19",
            "E-16",
            L("Orders/_DefectClaimScreen.cshtml", "@Model.RootPath"),
            L("Orders/_OperationScreen.cshtml", "@Model.RootPath"),
            L("Orders/_WithdrawalScreen.cshtml", "@Model.RootPath"),
            L("Shared/_ConfirmForm.cshtml", "@Model.CancelPath"),
            R("Orders/AfterSalesScreenModels.cs", "RootPath"),
            R("Orders/OrderScreenModels.cs", "RootPath"),
            R("!Infrastructure/Operations/OperationRefusals.cs", "orderPagePath")),
        Row("3.3.26", "E-22", "E-20,E-23", L("Storefront/SignIn.cshtml", "@FramePaths.Register"), L("Storefront/SignIn.cshtml", "@FramePaths.PasswordReset")),
        Row(
            "3.3.27",
            "E-20,E-22",
            "~Google'ın sayfası (ürünün dışında)",
            R("Storefront/SignIn.cshtml.cs", "_flow.Start(HttpContext, GoogleFlowKind.SignInOrRegistration, null, Input.ReturnUrl)"),
            R("Storefront/Register.cshtml.cs", "_flow.Start(HttpContext, GoogleFlowKind.SignInOrRegistration, null, null)")),
        Row(
            "3.3.28",
            "E-20",
            "E-20",
            R("Storefront/Register.cshtml.cs", """FramePaths.Register + "?" + PendingParameter + "=" + Uri.EscapeDataString(value)"""),
            R("Storefront/Register.cshtml.cs", """FramePaths.Register + "?" + PendingParameter + "=" + Uri.EscapeDataString(sent)"""),
            R("Storefront/SignIn.cshtml.cs", """FramePaths.SignIn + "?" + PendingRegistrations.Parameter + "=" + Uri.EscapeDataString(sent)""")),
        Row(
            "3.3.29",
            "E-21,E-23",
            "E-22",
            L("Storefront/PasswordReset.cshtml", "@FramePaths.SignIn"),
            L("Storefront/Verification.cshtml", "@landing.NextPath"),
            R("Storefront/PasswordReset.cshtml.cs", """FramePaths.PasswordReset + "/" + Uri.EscapeDataString(key)""")),
        Row(
            "3.3.30",
            "E-25",
            "E-24,E-25",
            L("Account/Profile.cshtml", "@ReauthenticationAddresses.Screen(ReauthenticationOperation.EmailChange)"),
            L("Account/Profile.cshtml", "@ReauthenticationAddresses.Screen(ReauthenticationOperation.PasswordChange)"),
            L("Account/Profile.cshtml", "@ReauthenticationAddresses.Screen(ReauthenticationOperation.AccountDeletion)"),
            L("Account/Reauthentication.cshtml", "@FramePaths.Account"),
            R("Account/Profile.cshtml.cs", "FramePaths.Account"),
            R("Account/Profile.cshtml.cs", "ReauthenticationAddresses.Screen(ReauthenticationOperation.EmailChange)"),
            R("Account/Profile.cshtml.cs", "ReauthenticationAddresses.Screen(ReauthenticationOperation.PasswordChange)"),
            R("Account/Profile.cshtml.cs", "ReauthenticationAddresses.Screen(ReauthenticationOperation.AccountDeletion)"),
            R("Account/Profile.cshtml.cs", "ReauthenticationAddresses.Screen(operation)"),
            R("Account/Reauthentication.cshtml.cs", "FramePaths.Account"),
            R("Account/Reauthentication.cshtml.cs", "ReauthenticationAddresses.Granted(operation, value)")),
        Row("3.3.31", "E-25", "E-25", R("Account/Profile.cshtml.cs", "FramePaths.AccountDeleted"), R("Account/Deleted.cshtml.cs", "FramePaths.Home")),
        Row("3.3.32", "E-25,E-26,E-27", "E-25,E-26,E-27", L("Shared/_AccountNav.cshtml", "@path")),
        Row("3.3.33", "E-28", "E-23,E-28", R("Storefront/EmailRevert.cshtml.cs", """FramePaths.EmailRevert + "/" + Uri.EscapeDataString(key)""")),
        Row("3.3.34", "E-21", "E-20", L("Storefront/Verification.cshtml", "@landing.NextPath")),
        Row("3.3.35", "E-21", "E-22,E-25", L("Storefront/Verification.cshtml", "@landing.NextPath"), R("Storefront/Verification.cshtml.cs", """FramePaths.Verification + "/" + Uri.EscapeDataString(key)""")),
        Row("3.3.36", "E-25", "E-23", L("Account/Profile.cshtml", "@FramePaths.PasswordReset"), L("Account/Reauthentication.cshtml", "@FramePaths.PasswordReset")),
        Row(
            "3.3.37",
            "E-24",
            "E-25",
            R("Account/Reauthentication.cshtml.cs", "_flow.Start(HttpContext, GoogleFlowKind.Reauthentication, operation.ToString(), null)"),
            R("Storefront/GoogleReturn.cshtml.cs", "ReauthenticationAddresses.Granted(operation, value)"),
            R("Storefront/GoogleReturn.cshtml.cs", "ReauthenticationAddresses.Screen(operation)"),
            R("Storefront/GoogleReturn.cshtml.cs", "FramePaths.Account")),
        Row("3.3.38", "E-14", "E-12", R("Orders/ConfirmationScreenModels.cs", "FramePaths.Cart"), L("Orders/_OrderPage.cshtml", "@FramePaths.Cart")),
        Row("3.3.39", "E-14", "E-15,E-16", R("Orders/ConfirmationScreenModels.cs", "NotAwaitingTarget()")),

        // The links the screen specifications of 04 5 and 9 write (K-1617).
        Row("3.3.40", "E-25", "E-01,E-15", L("Account/Deleted.cshtml", "@FramePaths.OrderTracking"), L("Account/Deleted.cshtml", "@FramePaths.Home")),
        Row("3.3.41", "E-15", "E-22", L("Orders/Tracking.cshtml", "@(HttpContext.GetStorefrontFrame()?.Account.SignInPath ?? FramePaths.SignIn)")),
        Row("3.3.42", "E-16", "~Kargo şirketinin takip sayfası (ürünün dışında)", L("Orders/_OrderPage.cshtml", "@tracking.Href")),
        Row("3.3.43", "E-20", "E-22", L("Storefront/Register.cshtml", "@FramePaths.SignIn")),
        Row(
            "3.3.44",
            "E-10,E-13,E-16,E-17,E-18,E-19,E-20,E-22",
            "E-11",
            L("Storefront/Checkout.cshtml", "@FramePaths.PrivacyNotice"),
            L("Orders/_DefectClaimScreen.cshtml", "@FramePaths.PrivacyNotice"),
            L("Storefront/Contact.cshtml", "@privacyNotice.Path"),
            L("Storefront/Register.cshtml", "@FramePaths.PrivacyNotice"),
            L("Storefront/SignIn.cshtml", "@FramePaths.PrivacyNotice"),
            L("Shared/_IbanField.cshtml", "@Model.NoticeHref")),
        Row("3.3.45", "E-10", "~Haritanın sayfası (ürünün dışında)", L("Storefront/Contact.cshtml", "@mapUrl")),

        // 04 3.4: the panel.
        Row("3.4.1", "E-29", "E-31", R("Panel/SignIn.cshtml.cs", "ReturnPath.Resolve(input.ReturnUrl, ReturnArea.Panel)")),
        Row("3.4.2", "E-31", "E-36,E-38", L("Shared/_PendingCounters.cshtml", "@counter.ListPath")),
        Row("3.4.3", "E-31", "E-36,E-44,E-45,E-46,E-47", L("Panel/Index.cshtml", "@warning.Href"), L("Shared/_PanelSalesStatus.cshtml", "@row.FixPath")),
        Row("3.4.4", "E-31", "E-44,E-45,E-46,E-47", L("Panel/Index.cshtml", "@item.Href")),
        Row("3.4.5", "E-31", "E-36,E-37", L("Shared/_AwaitingCustomerList.cshtml", "@row.OrderPath"), L("Shared/_AwaitingCustomerList.cshtml", "@AwaitingCustomerList.AllPath")),
        Row("3.4.6", "E-36", "E-37", L("Shared/_PanelTable.cshtml", "@row.RecordPath"), X("Panel/Orders.cshtml.cs", """ListReturn.LinkTo($"{PanelFramePaths.Orders}/{order.OrderId}", listAddress)""")),
        Row(
            "3.4.7",
            "E-37",
            "E-37",
            R("Panel/OrderDetail.cshtml.cs", "PathOf(id)"),
            R("Panel/OrderDetail.Corrections.cs", "PathOf(id)"),
            R("Panel/OrderDetail.Upkeep.cs", "PathOf(id)")),
        Row("3.4.8", "E-38", "E-37,E-39", L("Shared/_PanelTable.cshtml", "@row.RecordPath"), X("Panel/Requests.cshtml.cs", """ListReturn.LinkTo($"{PanelFramePaths.Requests}/{row.Id}", listAddress)"""), X("Panel/Requests.cshtml.cs", """ListReturn.LinkTo($"{PanelFramePaths.Orders}/{orderId}", listAddress)""")),
        Row("3.4.9", "E-37,E-39", "E-38", L("Panel/RequestDetail.cshtml", "@Model.BackPath"), L("Panel/OrderDetail.cshtml", "@Model.BackPath")),
        Row(
            "3.4.10",
            "E-32",
            "E-33",
            L("Panel/Products.cshtml", "@Model.NewProductPath"),
            E("Panel/Products.cshtml.cs", "NewProductPath"),
            L("Shared/_PanelTable.cshtml", "@row.RecordPath"),
            X("Panel/Products.cshtml.cs", """ListReturn.LinkTo($"{PanelFramePaths.Products}/{row.Id}", listAddress)"""),
            R("Panel/ProductForm.cshtml.cs", "FormAddress(savedId, returnUrl)"),
            R("Panel/ProductForm.cshtml.cs", "FormAddress(productId, returnUrl)")),
        Row("3.4.11", "E-33", "E-04,E-05", L("Panel/_ProductFormBody.cshtml", "/@AddressRules.ProductPrefix/@record.Slug")),
        Row(
            "3.4.12",
            "E-41",
            "E-41",
            L("Panel/ContentList.cshtml", "@ContentRoutes.FormPath(group.Type, row.Id)"),
            L("Panel/ContentList.cshtml", "@ContentRoutes.NewPath(group.Type)"),
            L("Panel/_ContentFormBody.cshtml", "@ContentFormModel.ListPath"),
            R("Panel/ContentForm.cshtml.cs", "ContentRoutes.FormPath(type, existing)"),
            R("Panel/ContentForm.cshtml.cs", "ContentRoutes.FormPath(type, key)"),
            R("Panel/ContentForm.cshtml.cs", "ContentRoutes.FormPath(type, savedId)"),
            R("Panel/ContentForm.cshtml.cs", "PanelFramePaths.CorporateContent"),
            R("Panel/ContentList.cshtml.cs", "PanelFramePaths.CorporateContent")),
        Row("3.4.13", "E-41", "E-08,E-09,E-10", L("Panel/_ContentFormBody.cshtml", "@record.PagePath")),
        Row("3.4.14", "E-40", "E-37", L("Panel/Members.cshtml", "@order.Path")),
        Row("3.4.15", "E-44", "E-44,E-45,E-46,E-47", L("Panel/FirmIdentity.cshtml", "@row.Href"), L("Shared/_PanelSalesStatus.cshtml", "@row.FixPath")),
        Row(
            "3.4.16",
            "E-50",
            "E-50,E-54",
            L("Panel/MyAccount.cshtml", "@reauthForEmail"),
            L("Panel/MyAccount.cshtml", "@reauthForPass"),
            R("Panel/MyAccount.cshtml.cs", "ReauthPath(ReauthenticationOperation.EmailChange)"),
            R("Panel/MyAccount.cshtml.cs", "ReauthPath(ReauthenticationOperation.PasswordChange)"),
            R("Panel/MyAccount.cshtml.cs", "PanelFramePaths.MyAccount"),
            R("Panel/Reauth.cshtml.cs", "PanelFramePaths.MyAccount"),
            R("Panel/Reauth.cshtml.cs", """PanelFramePaths.MyAccount + "?" + OperationParameter + "=" + Uri.EscapeDataString(operation.ToString()) + "&reauthenticationToken=" + Uri.EscapeDataString(value)""")),
        Row(
            "3.4.17",
            "E-49",
            "E-49",
            R("Panel/AdminAccounts.cshtml.cs", "ListRoot")),
        Row(
            "3.4.18",
            "E-30",
            "E-29",
            L("Panel/Invite.cshtml", "@PanelFramePaths.SignIn"),
            R("Panel/Invite.cshtml.cs", """PanelFramePaths.SignIn + "?" + SignInModel.AcceptedInvitationParameter + "=" + Uri.EscapeDataString(protectedAddress)""")),
        Row("3.4.19", "E-33", "E-37", L("Panel/_ProductFormBody.cshtml", "@ProductFormModel.OrderPathOf(holder.OrderId)")),
        Row(
            "3.4.20",
            "E-33,E-37",
            "E-32,E-36",
            L("Panel/_ProductFormBody.cshtml", "@Model.ReturnPath"),
            L("Panel/OrderDetail.cshtml", "@Model.BackPath"),
            R("Panel/ProductForm.cshtml.cs", "ListReturn.Target(returnUrl, PanelFramePaths.Products)")),
        Row("3.4.21", "E-39", "E-36,E-40", L("Panel/RequestDetail.cshtml", "@Model.SearchOrdersPath"), L("Panel/RequestDetail.cshtml", "@Model.SearchMembersPath")),
        Row("3.4.22", "E-47", "E-44", L("Shared/_LegalTextSection.cshtml", "@item.Href")),
        Row(
            "3.4.23",
            "E-54",
            "E-29,E-50",
            L("Panel/Revert.cshtml", "@Model.NextPath"),
            L("Panel/Verify.cshtml", "@Model.NextPath"),
            R("Panel/Revert.cshtml.cs", """PanelFramePaths.EmailRevert + "/" + Uri.EscapeDataString(key)"""),
            R("Panel/Verify.cshtml.cs", """PanelFramePaths.EmailVerification + "/" + Uri.EscapeDataString(key)""")),
        Row("3.4.24", "E-45", "E-46", L("Panel/PaymentMethods.cshtml", "@Shopfolio.Pages.Frame.PanelFramePaths.ShippingAndDefaults")),
        Row("3.4.25", "E-37", "~Kargo şirketinin takip sayfası (ürünün dışında)", L("Panel/OrderDetail.cshtml", "@link.Href")),
        Row(
            "3.4.26",
            "E-36,E-38,E-52",
            "E-31",
            E("Panel/Orders.cshtml.cs", "PanelFramePaths.Home"),
            E("Panel/Requests.cshtml.cs", "PanelFramePaths.Home"),
            E("Panel/AuditTrail.cshtml.cs", "PanelFramePaths.Home")),

        // 04 3.5: entries from outside the site.
        Row("3.5.1", "~Sipariş e-postası", "E-16", Page("/siparis/erisim/{accessKey*"), Page("/siparis/{orderNumber*")),
        Row("3.5.2", "~Sipariş e-postası", "E-16", Page("/siparis/erisim/{accessKey*"), Page("/siparis/{orderNumber*")),
        Row("3.5.3", "~IBAN gerekli", "E-16", Page("/siparis/erisim/{accessKey*")),
        Row("3.5.4", "~E-posta doğrulama bağlantısı (kayıt)", "E-21", Page("/dogrulama/{key}")),
        Row("3.5.5", "~Şifre sıfırlama bağlantısı", "E-23,E-29", Page("/sifre-sifirlama/{key?}"), Page("/panel/sifre-sifirlama/{key?}")),
        Row("3.5.6", "~Yeni e-posta adresinin doğrulama bağlantısı", "E-21,E-54", Page("/dogrulama/{key}"), Page("/panel/dogrulama/{key}")),
        Row("3.5.7", "~Bu değişikliği ben yapmadım", "E-28,E-54", Page("/geri-al/{key}"), Page("/panel/geri-al/{key}")),
        Row("3.5.8", "~Ödeme sağlayıcısının formu", "E-16", Page("/odeme/donus/{accessKey*"), L("Payment/Return.cshtml", "@Model.OrderPath")),
        Row(
            "3.5.9",
            "~Google'ın sayfası",
            "E-20,E-22",
            Page("/giris/google"),
            R("Storefront/GoogleReturn.cshtml.cs", "ReturnPath.AfterSignIn(flow.State.ReturnUrl)"),
            R("Storefront/GoogleReturn.cshtml.cs", "FramePaths.Register"),
            R("Storefront/GoogleReturn.cshtml.cs", "FramePaths.SignIn")),
        Row("3.5.10", "~Yönetici daveti e-postası", "E-30", Page("/panel/davet/{key}")),
        Row("3.5.11", "~Paylaşılmış ya da arama motorundan gelen ürün adresi", "E-04,E-05,E-06", Page("/urun/{slug}")),
        Row("3.5.12", "~Paylaşılmış içerik adresi", "E-06,E-08", Page("/hizmet/{slug}"), Page("/referans/{slug}"), Page("/sayfa/{slug}"), Page("/hakkimizda")),
        Row("3.5.13", "~Var olmayan adres", "E-06", X("Frame/FramedNotFoundPageRenderer.cs", "class FramedNotFoundPageRenderer")),
        Row(
            "3.5.14",
            "~Oturum gerektiren ekranın adresi",
            "E-22,E-29",
            R(SessionHandler, "StatusCodes.Status302Found, location"),
            R(SessionHandler, "StatusCodes.Status303SeeOther, NoticeParameter.Address(location, NoticeCode.SessionExpired)")),
        Row("3.5.15", "~Google girişinin teyit bağlantısı", "E-21", Page("/dogrulama/{key}")),

        // 04 3.7: between the panel and the storefront.
        Row("3.7.1", "~Panelin her ekranı — üst satır", "E-01", L(PanelLayout, "@frame.SiteViewPath")),
        Row("3.7.2", "E-33,E-41", "~Kaydın vitrindeki sayfası", L("Panel/_ContentFormBody.cshtml", "@record.PagePath"), L("Panel/_ProductFormBody.cshtml", "/@AddressRules.ProductPrefix/@record.Slug")),
        Row("3.7.3", "~Vitrinin her sayfası — yönetici şeridi", "E-31", L("Shared/_AdminStrip.cshtml", "@Model.PanelPath")),
        Row("3.7.4", "~Önizlenen taslak kaydın sayfası", "E-33,E-41", L("Shared/_DraftBand.cshtml", "@Model.EditPath"), X("Product.cshtml.cs", "HttpContext.SetDraftPreview(")),
        Row("3.7.5", "~Vitrin — yönetici oturumu kapanmışken taslak adresi", "E-06", X("Frame/FramedNotFoundPageRenderer.cs", "class FramedNotFoundPageRenderer"), X("!Modules/Catalog/ProductPageReader.cs", "ProductPageKind.DraftPreview")),

        // 04 3.6: the paragraphs; the pin of the paragraph is on the "to" side.
        Row(
            "3.6.1",
            string.Empty,
            "E-22,E-25,E-26,E-27,E-29,E-31",
            R(SessionHandler, "StatusCodes.Status302Found, location"),
            R("Panel/SignIn.cshtml.cs", "ReturnPath.Resolve(input.ReturnUrl, ReturnArea.Panel)"),
            R("Storefront/SignIn.cshtml.cs", "ReturnPath.AfterSignIn(input.ReturnUrl)")),
        Row(
            "3.6.2",
            string.Empty,
            "~girişten sonra ödeme adımına döner",
            X("Storefront/Checkout.cshtml.cs", """SignInPath = FramePaths.SignIn + "?returnUrl=" + FramePaths.Checkout"""),
            R("Storefront/Checkout.cshtml.cs", "SignInPath")),
        Row(
            "3.6.3",
            string.Empty,
            "~geldiği vitrin sayfasına döner",
            R("Storefront/SignIn.cshtml.cs", "ReturnPath.AfterSignIn(input.ReturnUrl)"),
            R("Storefront/GoogleReturn.cshtml.cs", "ReturnPath.AfterSignIn(flow.State.ReturnUrl)")),
        Row(
            "3.6.4",
            string.Empty,
            "~Çıkış yapan yönetici panel girişine iner",
            R("Storefront/SignOut.cshtml.cs", "ReturnPath.AfterSignOut(returnUrl)"),
            R("Panel/SignOut.cshtml.cs", "SessionSchemes.AdminSignInPath"),
            R(SessionHandler, "StatusCodes.Status303SeeOther, target")),
        Row(
            "3.6.5",
            string.Empty,
            "~dönüş 3.6.1'dir",
            R(SessionHandler, "StatusCodes.Status303SeeOther, NoticeParameter.Address(location, NoticeCode.SessionExpired)"),
            R("Panel/AdminAccounts.cshtml.cs", "PanelFramePaths.SignIn")),
    ];

    /// <summary>The sites that stay on their screen or are not a screen (04 3 introduction); no link that leads to another screen is here.</summary>
    public static IReadOnlyList<NavigationExemption> Exemptions { get; } =
    [
        // Sorting, paging, in-row editing and the confirmation window are no transitions (04 3 introduction); the lists' own addresses.
        Stay("Shared/_Pager.cshtml", "@Model.PrevPagePath", "paging stays on the list (04 2.13.3)"),
        Stay("Shared/_Pager.cshtml", "@item.Path", "paging stays on the list (04 2.13.3)"),
        Stay("Shared/_Pager.cshtml", "@Model.NextPath", "paging stays on the list (04 2.13.3)"),
        Stay("Category.cshtml", "@filter.ActionPath", "clearing the filter stays on the list (04 2.13.2)"),
        Stay("Panel/AuditTrail.cshtml", "@chip.RemovePath", "removing a filter chip stays on the list (04 2.13.2)"),
        Stay("Panel/Orders.cshtml", "@chip.RemovePath", "removing a filter chip stays on the list (04 2.13.2)"),
        Stay("Panel/Requests.cshtml", "@chip.RemovePath", "removing a filter chip stays on the list (04 2.13.2)"),
        Stay("Account/Addresses.cshtml", "@addPath", "E-26's own add form (04 5.26)"),
        Stay("Account/Addresses.cshtml", "@path", "E-26's own cancel (04 5.26)"),
        Stay("Account/Addresses.cshtml", "@path?addressBookId=@entry.Id.ToString(\"D\")", "E-26's own in-row edit (04 5.26)"),
        Stay("Panel/Coupons.cshtml", "@PanelFramePaths.Coupons", "E-35's list and form are one screen (04 3.2.6)"),
        Stay("Panel/Coupons.cshtml", """@(PanelFramePaths.Coupons + "/" + CouponsModel.NewKey)""", "E-35's list and form are one screen (04 3.2.6)"),
        Stay("Panel/Reset.cshtml", "@PanelFramePaths.PasswordReset", "E-29's own steps"),
        Stay("Panel/Reset.cshtml", "@PanelFramePaths.SignIn", "E-29's own steps"),
        Stay("Panel/SignIn.cshtml", "@PanelFramePaths.PasswordReset", "E-29's own steps"),
        Stay("Storefront/PasswordReset.cshtml", "@FramePaths.PasswordReset", "E-23's own steps"),
        Stay("Shared/_UnexpectedError.cshtml", "@Model.HomePath", "the error state's exits are no screen transition (04 2.7.3.2)"),
        Stay("Shared/_UnexpectedError.cshtml", "@Model.RetryPath", "the error state's exits are no screen transition (04 2.7.3.2)"),
        Stay("!Infrastructure/Errors/UnexpectedErrorSetup.cs", "resolution.Target ?? \"/\"", "the recovery of an unexpected error returns to the screen it came from (04 2.7.3)", NavigationSiteKind.Redirect),

        // The shared empty-state components draw the way out their callers give; each call is a site of its own (EmptyExit): the ones that go to another screen are rows (3.3.12, 3.4.10, 3.4.26), the
        // ones below stay on their list - removing the filter or the search, or opening the first record on the screen's own form (04 2.7.1.2, 2.13.2).
        Stay("Shared/_EmptyRow.cshtml", "@Model.ExitPath", "the component draws the way out each call gives it; the calls are the sites (EmptyExit)"),
        Stay("Shared/_EmptyState.cshtml", "@Model.ActionPath", "the component draws the way out each call gives it; the calls are the sites (EmptyExit)"),
        Stay("Storefront/CategoryPage.cs", "categoryPath", "removing the filter stays on the category list (04 2.7.1.2, 2.13.2)", NavigationSiteKind.EmptyExit),
        Stay("Storefront/SearchPage.cs", "searchAddress", "removing the filter stays on the search result (04 2.7.1.2, 2.13.2)", NavigationSiteKind.EmptyExit),
        Stay("Panel/Orders.cshtml.cs", "ListRoot", "removing the filter stays on the list (04 2.7.1.2, 2.13.2)", NavigationSiteKind.EmptyExit),
        Stay("Panel/Requests.cshtml.cs", "ListRoot", "removing the filter stays on the list (04 2.7.1.2, 2.13.2)", NavigationSiteKind.EmptyExit),
        Stay("Panel/AuditTrail.cshtml.cs", "ListRoot", "removing the filter stays on the list (04 2.7.1.2, 2.13.2)", NavigationSiteKind.EmptyExit),
        Stay("Panel/Products.cshtml.cs", "ListRoot", "removing the search stays on the list (04 2.7.1.2, 2.13.2)", NavigationSiteKind.EmptyExit),
        Stay("Account/Addresses.cshtml", "addPath", "E-26's own add form (04 5.26.9)", NavigationSiteKind.EmptyExit),
        Stay("Panel/Categories.cshtml.cs", """PanelFramePaths.Categories + "#" + NewCategoryFormId""", "E-34's own form (04 2.7.1.2)", NavigationSiteKind.EmptyExit),
        Stay("Panel/Coupons.cshtml.cs", """PanelFramePaths.Coupons + "/" + NewKey""", "E-35's list and form are one screen (04 3.2.6)", NavigationSiteKind.EmptyExit),

        // Post-redirect-get of a screen's own handler: the screen answers itself.
        Stay("Account/Addresses.cshtml.cs", "FramePaths.AccountAddresses", "E-26 answers itself", NavigationSiteKind.Redirect),
        Stay("Orders/ConfirmationScreenModels.cs", "ScreenPath", "E-14 answers itself", NavigationSiteKind.Redirect),
        Stay("Orders/OrderPageModels.cs", "RootPath", "E-16 answers itself", NavigationSiteKind.Redirect),
        Stay("Panel/Brand.cshtml.cs", "PanelFramePaths.Brand", "E-43 answers itself", NavigationSiteKind.Redirect),
        Stay("Panel/Categories.cshtml.cs", "PanelFramePaths.Categories", "E-34 answers itself", NavigationSiteKind.Redirect),
        Stay("Panel/Coupons.cshtml.cs", """PanelFramePaths.Coupons + "/" + written.ToString("D")""", "E-35 answers itself", NavigationSiteKind.Redirect),
        Stay("Panel/FirmIdentity.cshtml.cs", "PanelFramePaths.FirmIdentityAndSales", "E-44 answers itself", NavigationSiteKind.Redirect),
        Stay("Panel/HomeAndMenu.cshtml.cs", "PanelFramePaths.HomeAndMenu", "E-42 answers itself", NavigationSiteKind.Redirect),
        Stay("Panel/LegalTexts.cshtml.cs", "PanelFramePaths.LegalTexts", "E-47 answers itself", NavigationSiteKind.Redirect),
        Stay("Panel/Members.cshtml.cs", "PanelFramePaths.Members", "E-40 answers itself", NavigationSiteKind.Redirect),
        Stay("Panel/PaymentMethods.cshtml.cs", "PanelFramePaths.PaymentMethods", "E-45 answers itself", NavigationSiteKind.Redirect),
        Stay("Panel/RequestDetail.cshtml.cs", "path", "E-39 answers itself", NavigationSiteKind.Redirect),
        Stay("Panel/Reset.cshtml.cs", "PanelFramePaths.PasswordReset", "E-29's own steps", NavigationSiteKind.Redirect),
        Stay("Panel/Reset.cshtml.cs", """PanelFramePaths.PasswordReset + "/" + Uri.EscapeDataString(key)""", "E-29's own steps", NavigationSiteKind.Redirect),
        Stay("Panel/ShippingAndDefaults.cshtml.cs", "PanelFramePaths.ShippingAndDefaults", "E-46 answers itself", NavigationSiteKind.Redirect),
        Stay("Storefront/Cart.cshtml.cs", "FramePaths.Cart", "E-12 answers itself", NavigationSiteKind.Redirect),
        Stay("Storefront/PasswordReset.cshtml.cs", "FramePaths.PasswordReset", "E-23's own steps", NavigationSiteKind.Redirect),
        Stay("Storefront/Register.cshtml.cs", "FramePaths.Register", "E-20 answers itself", NavigationSiteKind.Redirect),
        Stay("Storefront/SignIn.cshtml.cs", "FramePaths.SignIn", "E-22 answers itself", NavigationSiteKind.Redirect),

        // A file is no screen (04 4 lists the 53 screens).
        Stay("Orders/OrderPageModels.cs", "Endpoints.Files.DownloadEndpoint.Prefix + \"/\" + value", "the download's answer is a file, no screen", NavigationSiteKind.Redirect),
    ];

    private static string Path(string file) => file.StartsWith('!') ? file[1..] : "Pages/" + file;

    private static NavigationEntry Row(string id, string from, string to, params NavigationEvidence[] evidence) => new(id, from, to, evidence);

    private static NavigationEvidence L(string file, string key) => new(NavigationEvidenceKind.Link, Path(file), key);

    private static NavigationEvidence R(string file, string key) => new(NavigationEvidenceKind.Redirect, Path(file), key);

    private static NavigationEvidence E(string file, string key) => new(NavigationEvidenceKind.EmptyExit, Path(file), key);

    private static NavigationEvidence X(string file, string text) => new(NavigationEvidenceKind.Text, Path(file), text);

    private static NavigationEvidence Page(string route) => new(NavigationEvidenceKind.Page, string.Empty, route);

    private static NavigationExemption Stay(string file, string key, string reason, NavigationSiteKind kind = NavigationSiteKind.Link) => new(kind, Path(file), key, reason);
}
