using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.HtmlRendering;
using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;

namespace Shopfolio.Infrastructure.EmailComposition;

/// <summary>
/// The common pattern of every e-mail (08 2.2.5; 09 11.4): from a template's <see cref="EmailContent"/>, the brand and the firm it makes the
/// <see cref="EmailMessage"/> of one recipient - the subject "{brand}: {title}" (with " (Sipariş {number})" on an order-bound e-mail), the sender's name, the reply
/// address, the HTML part (the Razor frame through <see cref="HtmlRenderer"/>) and the plain text part (<see cref="EmailTextWriter"/>) from one model. Where the
/// brand name is not entered yet the installation's domain stands in its place (02 3.29.3). The composer reads nothing from a request and no table: every value
/// is handed in, the addresses are made by <see cref="EmailLinks"/> from the domain setting. One instance serves the application.
/// </summary>
internal sealed class EmailComposer
{
    private readonly EmailLinks _links;
    private readonly string _domain;
    private readonly IServiceProvider _services;
    private readonly ILoggerFactory _loggers;

    public EmailComposer(EmailLinks links, IOptions<SiteOptions> site, IServiceProvider services, ILoggerFactory loggers)
    {
        ArgumentNullException.ThrowIfNull(links);
        ArgumentNullException.ThrowIfNull(site);
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(loggers);
        _links = links;
        _domain = site.Value.Domain;
        _services = services;
        _loggers = loggers;
    }

    public async Task<EmailMessage> ComposeAsync(string recipientEmail, EmailBranding branding, EmailFirm firm, EmailContent content, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientEmail);
        ArgumentNullException.ThrowIfNull(branding);
        ArgumentNullException.ThrowIfNull(firm);
        ArgumentNullException.ThrowIfNull(content);
        ct.ThrowIfCancellationRequested();

        string brand = EmailText.BrandOrDomain(branding.BrandName, _domain);
        string subject = Subject(brand, content);
        (string background, string text) = EmailColors.Button(branding.BrandColor);
        string? logo = string.IsNullOrWhiteSpace(branding.LogoPath) ? null : _links.Logo(branding.LogoPath).AbsoluteUri;
        EmailFrameModel model = new(
            subject,
            brand,
            logo,
            content.LogoWidth,
            background,
            text,
            content.Resend?.Sentence,
            content.Blocks,
            firm.LegalName,
            firm.ContactEmail);

        string html = await RenderAsync(model);
        string plain = EmailTextWriter.Write(model);
        string? replyTo = string.IsNullOrWhiteSpace(firm.ContactEmail) ? null : EmailText.OneLine(firm.ContactEmail);
        return new EmailMessage(recipientEmail, brand, replyTo, subject, html, plain);
    }

    /// <summary>The subject (08 2.2.5 item 3): "{brand}: {title}" and, on an order-bound e-mail, " (Sipariş {number})"; no customer text, one line.</summary>
    public static string Subject(string brand, EmailContent content)
    {
        ArgumentNullException.ThrowIfNull(content);
        string subject = EmailText.OneLine(brand) + ": " + EmailText.OneLine(content.Title);
        return content.OrderNumber is null ? subject : subject + " (Sipariş " + EmailText.OneLine(content.OrderNumber) + ")";
    }

    private async Task<string> RenderAsync(EmailFrameModel model)
    {
        await using HtmlRenderer renderer = new(_services, _loggers);
        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            HtmlRootComponent root = await renderer.RenderComponentAsync<EmailFrame>(
                ParameterView.FromDictionary(new Dictionary<string, object?> { [nameof(EmailFrame.Model)] = model }));
            return root.ToHtmlString();
        });
    }
}
