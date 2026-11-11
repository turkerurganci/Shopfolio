using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.IntegrationTests.Pages;

/// <summary>The orders and the writes of the form-security sample pages, in memory; a singleton of the test host (T209).</summary>
public sealed class FormSecurityLog
{
    private readonly Dictionary<string, string> _ordersByKey = [];
    private readonly List<string> _writes = [];
    private readonly Lock _gate = new();

    public IReadOnlyList<string> Writes
    {
        get
        {
            lock (_gate)
            {
                return [.. _writes];
            }
        }
    }

    public int OrderCount
    {
        get
        {
            lock (_gate)
            {
                return _ordersByKey.Count;
            }
        }
    }

    public void Add(string write)
    {
        lock (_gate)
        {
            _writes.Add(write);
        }
    }

    /// <summary>The order of the confirmation key; opens one when the key is new. The second value is true when this call opened it.</summary>
    public (string AccessKey, bool Created) OrderOf(string confirmationKey)
    {
        lock (_gate)
        {
            if (_ordersByKey.TryGetValue(confirmationKey, out string? existing))
            {
                return (existing, false);
            }

            string addressPart = SecretKey.Create();
            _ordersByKey[confirmationKey] = addressPart;
            return (addressPart, true);
        }
    }
}

/// <summary>The sample payment step (07 2.5.1): the form carries a fresh confirmation key each time it is shown; the same key opens one order only.</summary>
public sealed class ConfirmationSampleModel(FormSecurityLog log) : PageModel
{
    public string Key { get; private set; } = string.Empty;

    public void OnGet() => Key = ConfirmationKeys.NewKey();

    public IActionResult OnPostPay(ConfirmationInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        (string accessKey, _) = log.OrderOf(input.ConfirmationKey);
        return FormResults.SeeOther(ConfirmationKeys.AccessConfirmationPath(accessKey));
    }
}

/// <summary>The input of the sample payment step: the hidden confirmation key.</summary>
public sealed class ConfirmationInput
{
    [Required]
    public string ConfirmationKey { get; set; } = string.Empty;
}

/// <summary>The sample operation that needs a reauthentication (07 2.5.2): the form carries the single-use value of the customer's session; a valid send consumes it, an invalid one does not.</summary>
public sealed class ReauthenticationSampleModel(FormSecurityLog log) : PageModel
{
    public string Value { get; private set; } = string.Empty;

    public async Task OnGetAsync(CancellationToken ct) => Value = await Values.GrantAsync(SessionId(), ReauthenticationOperation.PasswordChange, ct) ?? string.Empty;

    public async Task<IActionResult> OnPostApplyAsync(ReauthenticationInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (!ModelState.IsValid)
        {
            Value = input.ReauthenticationToken;
            return FormResults.Redisplay(this);
        }

        if (!await Values.TryRedeemAsync(SessionId(), ReauthenticationOperation.PasswordChange, input.ReauthenticationToken, ct))
        {
            return FormResults.SeeOther("/hesap/ornek-yeniden-dogrulama?asked=1");
        }

        log.Add($"applied:{input.Note}");
        return FormResults.SeeOther("/hesap");
    }

    private ReauthenticationValues Values => HttpContext.RequestServices.GetRequiredService<ReauthenticationValues>();

    private Guid SessionId() => Guid.Parse(User.FindFirst(SessionSchemes.SessionIdClaim)!.Value);
}

/// <summary>The input of the sample operation: a note and the single-use value.</summary>
public sealed class ReauthenticationInput
{
    [Required]
    [StringLength(10)]
    public string Note { get; set; } = string.Empty;

    public string ReauthenticationToken { get; set; } = string.Empty;
}

/// <summary>The sample account link page (09 15.1 item 4): the address holds a key, so no referrer is sent and the response is not stored.</summary>
[KeyedLinkPage]
[NoStore]
public sealed class KeyedLinkSampleModel : PageModel
{
    public IActionResult OnGet(string key) => Content($"link:{key}");
}
