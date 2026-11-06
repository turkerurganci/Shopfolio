using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;

namespace Shopfolio.Infrastructure.Validation;

/// <summary>One rejected field of a request: the field's name and the code of 07 6.2.2 - the two things the rejection line carries (09 18.2 item 4).</summary>
internal sealed record FieldRejection(string Field, string Code);

/// <summary>
/// The one page filter of the field validation (09 3.4 item 3; 07 2.7.1, 6.1.3). After the model is bound and validated and before the handler runs,
/// when the model state holds errors it (a) writes one application log line with every rejected field's name and code (<see cref="RejectionLog"/>),(b) replaces each rule's code
/// with the message of the catalog, made from the code and the field's label, and (c) for a request that writes, answers 422 and shows the form again
/// without running the handler. Every error of the request is handled together (04 7.3.5). A rule the handler adds after the database answered goes
/// through the same translation after the handler returns (09 3.4 item 2). The code is never written to the page or the response (07 2.7.3): the model
/// state holds the catalog text from here on.
/// </summary>
internal sealed class ValidationPageFilter : IAsyncPageFilter
{
    private readonly IModelMetadataProvider _metadata;
    private readonly RejectionLog _rejections;

    public ValidationPageFilter(IModelMetadataProvider metadata, RejectionLog rejections)
    {
        _metadata = metadata;
        _rejections = rejections;
    }

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        bool logged = false;
        if (!context.ModelState.IsValid)
        {
            logged = Translate(context, logRejection: true);
            if (HandlerRequiredFilter.IsWritingMethod(context.HttpContext.Request.Method) && context.HandlerInstance is PageModel page)
            {
                context.Result = FormResults.Redisplay(page);
                return;
            }
        }

        await next();

        // The rules a handler decided itself (the database answered): the same path, one log line for the request.
        if (!context.ModelState.IsValid)
        {
            Translate(context, logRejection: !logged);
        }
    }

    /// <summary>Turns the rule codes of the model state into catalog texts; returns whether any code was found (and so whether a line was due).</summary>
    private bool Translate(PageHandlerExecutingContext context, bool logRejection)
    {
        FieldTargets targets = new(_metadata, context);
        List<FieldRejection> rejected = [];

        foreach (KeyValuePair<string, ModelStateEntry> pair in context.ModelState)
        {
            ModelStateEntry? entry = pair.Value;
            if (entry is null)
            {
                continue;
            }

            for (int i = 0; i < entry.Errors.Count; i++)
            {
                FieldTarget target = targets.Resolve(pair.Key);
                string message = entry.Errors[i].ErrorMessage;
                string code;
                string? parameter;
                if (message == FieldError.ConversionFailed)
                {
                    (code, parameter) = target.ConversionRule is { } rule ? (rule.Code, rule.ParameterText) : (FieldCodes.InvalidListValue, null);
                }
                else if (!FieldError.TryParse(message, out code, out parameter))
                {
                    continue;
                }

                entry.Errors[i] = new ModelError(FieldMessageCatalog.Build(code, target.Label, parameter));
                rejected.Add(new FieldRejection(pair.Key, code));
            }
        }

        if (rejected.Count > 0 && logRejection)
        {
            _rejections.FieldsRejected(rejected);
        }

        return rejected.Count > 0;
    }
}
