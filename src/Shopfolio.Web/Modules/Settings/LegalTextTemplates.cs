using System.Security.Cryptography;
using System.Text;

using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Settings;

/// <summary>One shipped template of a generated legal text: its text and its version id (09 11.4 item 5).</summary>
internal sealed record LegalTextTemplate(LegalTextType Type, string Text, string Version);

/// <summary>
/// The templates that come with the product (02 3.33.1; 09 11.4 items 3 and 5): constant text files embedded in the assembly. The version id of a template is the first
/// twelve hexadecimal digits of the SHA-256 of its content with line endings normalized to <c>\n</c>, so it changes exactly when the text changes and never because of the
/// checkout's line endings; there is no hand-raised version constant. Only the two generated texts have a template. This file is the one place of the Settings module that names
/// a digest function (the content digest of 09 11.4 item 5); the link key scan lists it as such.
/// </summary>
internal static class LegalTextTemplates
{
    /// <summary>The two generated types, in the order their versions are written.</summary>
    public static IReadOnlyList<LegalTextType> GeneratedTypes { get; } = [LegalTextType.PreInformationForm, LegalTextType.DistanceSalesContract];

    /// <summary>The tokens of a generated text that the version fills from the settings: the company identity and the return address. Every other token is an order field that waits (09 9.3 item 2).</summary>
    public static IReadOnlyList<string> CompanyTokens { get; } =
    [
        "CompanyLegalName", "CompanyTaxIdNumber", "CompanyMersisNumber", "CompanyTradeRegistryNumber", "CompanyProvince", "CompanyDistrict", "CompanyAddressLine",
        "CompanyPhone", "CompanyEmail", "CompanyKepAddress", "ReturnAddress",
    ];

    public static LegalTextTemplate Get(LegalTextType type)
    {
        string name = type switch
        {
            LegalTextType.PreInformationForm => "PreInformationForm.tr.txt",
            LegalTextType.DistanceSalesContract => "DistanceSalesContract.tr.txt",
            _ => throw new ArgumentException($"{type} is not a generated legal text and has no template.", nameof(type)),
        };
        string text = Read($"Shopfolio.Modules.Settings.Templates.{name}");
        return new LegalTextTemplate(type, text, VersionOf(text));
    }

    public static string VersionOf(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(text.ReplaceLineEndings("\n")));
        return Convert.ToHexStringLower(hash)[..12];
    }

    private static string Read(string resourceName)
    {
        using Stream stream = typeof(LegalTextTemplates).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"The template resource {resourceName} is not in the assembly.");
        using StreamReader reader = new(stream, Encoding.UTF8);
        return reader.ReadToEnd().ReplaceLineEndings("\n");
    }
}
