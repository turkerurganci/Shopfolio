namespace Shopfolio.Modules.Settings.Contracts;

/// <summary>One province of the closed list of 81 (06 4.3; 02 3.14.3): its id, its plate code ("06") and its Turkish name.</summary>
public sealed record ProvinceInfo(Guid Id, string Code, string Name);
