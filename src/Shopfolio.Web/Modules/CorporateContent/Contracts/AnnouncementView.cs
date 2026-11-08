namespace Shopfolio.Modules.CorporateContent.Contracts;

/// <summary>
/// The announcement strip of every page (T360; 02 3.27.21, 3.27.25): the one-line plain text - the page prints it escaped, never as markup - and the optional link. A visitor gets a
/// strip only for a published announcement that is inside its dates; the administrator also gets a draft, marked by <see cref="IsDraft"/> with the "Taslak" band (04's work), whatever
/// its dates, and never gets a published one outside its dates (it is not on the site for anyone).
/// </summary>
public sealed record AnnouncementView(string Text, string? LinkUrl, bool IsDraft);
