using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Validation;

namespace Shopfolio.IntegrationTests.Pages;

/// <summary>
/// The test-only page of the unexpected error tests (T212; 09 8.1 items 3, 4, 9). The handlers fail on purpose, each in a place the layer must
/// catch: in the GET, in the unmarked POST, inside a marked operation, after the marked operation's commit, and while the answer is drawn.
/// Its database side effect is a table created inside the operation's transaction, so the test sees whether the operation was rolled back.
/// </summary>
public class ErrorSampleModel : PageModel, IOutcomeTarget
{
    public const string ResultScreen = "/odeme/hata-ornegi?sonuc=1";

    public const string RolledBackTable = "t212_rolled_back";

    public const string CommittedTable = "t212_committed";

    public ErrorSampleInput Input { get; set; } = new();

    public bool RenderFails { get; private set; }

    public string OutcomeScreen() => ResultScreen;

    public async Task<IActionResult> OnGetAsync(string? fail, CancellationToken ct)
    {
        AppDbContext db = HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        switch (fail)
        {
            case "handler":
                throw new InvalidOperationException("t212 failure in the handler");
            case "render":
                RenderFails = true;
                break;
            case "timeout":
                // The session time out of the database (09 10.7): a statement that runs past its time is cancelled by the server with 57014.
                await using (var transaction = await db.Database.BeginTransactionAsync(ct))
                {
                    await db.Database.ExecuteSqlRawAsync("SET LOCAL statement_timeout = '200ms'", ct);
                    await db.Database.ExecuteSqlRawAsync("SELECT pg_sleep(5)", ct);
                    await transaction.CommitAsync(ct);
                }

                break;
            default:
                break;
        }

        return Page();
    }

    // The unmarked operation: the table is created in the open transaction and the error leaves it, so the transaction is rolled back (09 8.1 item 5).
    public async Task<IActionResult> OnPostSaveAsync(ErrorSampleInput input, CancellationToken ct)
    {
        Input = input;
        await CreateTableAsync(RolledBackTable, commit: false, ct);
        throw new InvalidOperationException("t212 failure in the unmarked operation");
    }

    public IActionResult OnPostPlain(ErrorSampleInput input)
    {
        Input = input;
        return FormResults.SeeOther("/odeme/hata-ornegi");
    }

    // A named business-rule refusal (07 2.7.1): 422, the page again with the sent values, nothing written.
    public IActionResult OnPostRule(ErrorSampleInput input)
    {
        Input = input;
        ModelState.AddFieldError("Input.Name", FieldCodes.InvalidListValue);
        return FormResults.Redisplay(this);
    }

    // A form saved with an upload that the nightly disposal removed (07 6.4.1): the field of the upload is marked, 422, the other values stay.
    public IActionResult OnPostUpload(ErrorSampleInput input)
    {
        Input = input;
        ModelState.AddFieldError("Input.Upload", FieldCodes.UploadMissing);
        return FormResults.Redisplay(this);
    }

    [OutcomeUncertain("SampleConfirm")]
    public async Task<IActionResult> OnPostConfirmAsync(ErrorSampleInput input, CancellationToken ct)
    {
        Input = input;
        switch (input.Mode)
        {
            case "inside":
                await CreateTableAsync(RolledBackTable, commit: false, ct);
                throw new InvalidOperationException("t212 failure inside the marked operation");
            case "after":
                await CreateTableAsync(CommittedTable, commit: true, ct);
                throw new InvalidOperationException("t212 failure after the commit of the marked operation");
            case "render":
                await CreateTableAsync(CommittedTable, commit: true, ct);
                RenderFails = true;
                return Page();
            case "reject":
                // A named refusal of a marked operation stays what it is: 422, not the uncertain outcome's 303 (07 6.4.2).
                ModelState.AddFieldError("Input.Name", FieldCodes.InvalidListValue);
                return FormResults.Redisplay(this);
            default:
                return FormResults.SeeOther("/odeme/hata-ornegi");
        }
    }

    private async Task CreateTableAsync(string table, bool commit, CancellationToken ct)
    {
        AppDbContext db = HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        ITransactionRunner runner = HttpContext.RequestServices.GetRequiredService<ITransactionRunner>();
        await runner.RunAsync(
            async token =>
            {
#pragma warning disable EF1002 // The table name is one of the two constants of this sample, never a request value.
                await db.Database.ExecuteSqlRawAsync($"CREATE TABLE {table} (id int)", token);
#pragma warning restore EF1002
                if (!commit)
                {
                    throw new InvalidOperationException("t212 failure before the commit");
                }

                return ErrorSampleOutcome.Done;
            },
            ct);
    }
}

/// <summary>The panel twin of the sample page: the same handlers on the panel's address, for the panel's message.</summary>
public sealed class PanelErrorSampleModel : ErrorSampleModel
{
}

/// <summary>The form of the sample page.</summary>
public sealed class ErrorSampleInput : IFormInput
{
    public string Name { get; set; } = string.Empty;

    public string Mode { get; set; } = string.Empty;

    public string? Upload { get; set; }
}

/// <summary>The result of the sample operation.</summary>
internal sealed class ErrorSampleOutcome : IOperationResult
{
    public static ErrorSampleOutcome Done { get; } = new();

    public bool IsRejected => false;
}
