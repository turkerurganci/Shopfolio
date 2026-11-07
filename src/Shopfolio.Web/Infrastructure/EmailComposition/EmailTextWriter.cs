using System.Text;

namespace Shopfolio.Infrastructure.EmailComposition;

/// <summary>
/// The plain text writer (09 11.4 item 1): the same <see cref="EmailFrameModel"/> the HTML is made from, so the content is equivalent and the addresses are
/// the same full addresses (08 2.2.5 item 1). A value is one line - its line breaks become blanks (08 2.2.4); a legal text keeps its lines, as it is frozen
/// (09 11.4 items 3, 4). Nothing here is markup.
/// </summary>
internal static class EmailTextWriter
{
    private const string Rule = "--";

    public static string Write(EmailFrameModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        StringBuilder text = new();
        text.Append(EmailText.OneLine(model.BrandText)).Append("\n\n");
        if (model.ResendSentence is not null)
        {
            text.Append(model.ResendSentence).Append("\n\n");
        }

        foreach (EmailBlock block in model.Blocks)
        {
            switch (block)
            {
                case EmailHeading heading:
                    text.Append(EmailText.OneLine(heading.Text)).Append("\n\n");
                    break;
                case EmailParagraph paragraph:
                    text.Append(EmailText.OneLine(paragraph.Text)).Append("\n\n");
                    break;
                case EmailKeyValue row:
                    text.Append(EmailText.OneLine(row.Label)).Append(": ").Append(EmailText.OneLine(row.Value)).Append('\n');
                    break;
                case EmailButton button:
                    text.Append('\n').Append(EmailText.OneLine(button.Label)).Append(":\n").Append(button.Link.AbsoluteUri).Append("\n\n");
                    break;
                case EmailLegalText legal:
                    text.Append(EmailText.OneLine(legal.Title)).Append('\n').Append(EmailText.NormalizeBreaks(legal.Text).Trim()).Append("\n\n");
                    break;
                default:
                    throw new InvalidOperationException($"No plain text form for the block {block.GetType().Name}.");
            }
        }

        text.Append(Rule).Append('\n')
            .Append(EmailText.OneLine(model.FirmName)).Append('\n')
            .Append(EmailText.OneLine(model.FirmEmail)).Append('\n')
            .Append(EmailFrameModel.ReplySentence).Append('\n');
        return text.ToString();
    }
}
