namespace Shopfolio.Modules.CorporateContent.Contracts;

/// <summary>One social media link as the storefront frame shows it (T537; 06 4.55): the platform of the closed list and the address the firm typed.</summary>
public sealed record SocialLinkItem(SocialPlatform Platform, string Url);
