namespace Shopfolio.Infrastructure.EmailComposition;

/// <summary>
/// Everything the frame component shows, made by <see cref="EmailComposer"/> from the template's content, the brand and the firm (08 2.2.5): the subject
/// (also the document title), the brand text (the logo's alternative text, or the header text when there is no logo), the logo's absolute address, the
/// main button's two colours, the re-send sentence, the blocks and the footer. Public because the component is (see <see cref="EmailBlock"/>).
/// </summary>
public sealed record EmailFrameModel(
    string Subject,
    string BrandText,
    string? LogoUrl,
    int LogoWidth,
    string ButtonBackground,
    string ButtonText,
    string? ResendSentence,
    IReadOnlyList<EmailBlock> Blocks,
    string FirmName,
    string FirmEmail)
{
    /// <summary>The footer's reply sentence (08 2.2.5 item 8).</summary>
    public const string ReplySentence = "Bu e-postayı yanıtlayarak firmaya ulaşabilirsiniz.";
}
