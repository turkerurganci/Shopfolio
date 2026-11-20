using Microsoft.Playwright;

namespace Shopfolio.IntegrationTests.Browser;

/// <summary>
/// The base of the browser tests (T551): the opened tool, the page opening with the browser's own messages collected, and the observation of a row - the
/// way, the engine, the width, the step, what was expected and what was seen, and the screenshot (12 7.1 "Tarayıcı gözlemi"; T546 and T547 precedent: every line
/// carries its own concrete evidence, a shared sentence is no evidence).
/// </summary>
[Collection(BrowserTestsDefinition.Name)]
[Trait("Browser", "true")]
public abstract class BrowserTestBase
{
    private static int _shots;

    protected BrowserTestBase(BrowserHost host)
    {
        Host = host;
    }

    protected BrowserHost Host { get; }

    /// <summary>The observations that did not match; the test asserts the list is empty at its end, so one failing line does not hide the others.</summary>
    protected List<string> Failures { get; } = [];

    protected static CancellationToken Ct => TestContext.Current.CancellationToken;

    /// <summary>The engine and way pairs every row is observed in: the three engines, the script on and off (T551 criterion 1).</summary>
    public static TheoryData<string, ScriptPath> Ways()
    {
        TheoryData<string, ScriptPath> data = [];
        foreach (string engine in BrowserHost.Engines)
        {
            foreach (ScriptPath path in BrowserHost.Paths)
            {
                data.Add(engine, path);
            }
        }

        return data;
    }

    /// <summary>A page of the context with the browser's own error messages collected (console errors, page errors, content security policy reports).</summary>
    protected static async Task<WatchedPage> NewPageAsync(IBrowserContext context)
    {
        IPage page = await context.NewPageAsync();
        WatchedPage watched = new(page);
        page.Console += (_, message) =>
        {
            if (message.Type == "error")
            {
                watched.Errors.Add("console: " + message.Text);
                watched.ErrorUrls.Add(WatchedPage.UrlOfLocation(message.Location));
            }
        };
        page.PageError += (_, message) =>
        {
            watched.Errors.Add("page: " + message);
            watched.ErrorUrls.Add(string.Empty);
        };
        page.RequestFailed += (_, request) =>
        {
            if (request.ResourceType == "script")
            {
                watched.CutScriptUrls.Add(request.Url);
            }
        };
        return watched;
    }

    protected async Task<IResponse?> GoAsync(WatchedPage page, string path)
    {
        IResponse? response = await page.Page.GotoAsync(Host.BaseUrl + path, new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 30000 });
        return response;
    }

    /// <summary>
    /// Runs the action that starts a navigation (a click, a send) and waits until the page it leads to has loaded. WebKit returns from a click before the navigation has started,
    /// so a load-state wait right after it would see the old, already loaded page; the navigation wait is armed before the action, so no pause is needed.
    /// </summary>
#pragma warning disable CS0612 // The race-free form (the wait is armed before the action) is the one this wait needs; WaitForURLAsync cannot tell a same-address send from no send.
    protected static async Task NavigateAsync(WatchedPage page, Func<Task> act, int timeout = 30000, WaitUntilState until = WaitUntilState.Load)
    {
        await page.Page.RunAndWaitForNavigationAsync(act, new PageRunAndWaitForNavigationOptions { WaitUntil = until, Timeout = timeout });
        if (until == WaitUntilState.Commit)
        {
            // The answer's document has been taken over (the wait was armed before the action, so this is the new document, not the old loaded one); its own markup is what is read, not its pictures and scripts.
            await page.Page.WaitForLoadStateAsync(LoadState.DOMContentLoaded, new PageWaitForLoadStateOptions { Timeout = timeout });
        }
    }
#pragma warning restore CS0612

    /// <summary>
    /// Waits until every style sheet the page links has been loaded and applied (D-1746 a). A wait for the document alone (<see cref="WaitUntilState.Commit"/>, then DOMContentLoaded) can end before the
    /// style sheets have arrived, and a reading that depends on the page's own style (the contrast of a scan, the visibility of a control) would read the unstyled page; this wait names what is needed
    /// instead of a length of time.
    /// </summary>
    protected static async Task WaitForStylesAsync(WatchedPage page, int timeout = 30000)
    {
        await page.Page.WaitForFunctionAsync(StylesLoaded, null, new PageWaitForFunctionOptions { Timeout = timeout });
    }

    /// <summary>True when every linked style sheet has a loaded sheet (a link that failed to load never gets one, so the wait ends at its time limit and the test fails on the stylesheet, as it should).</summary>
    internal const string StylesLoaded = "() => [...document.querySelectorAll('link[rel~=stylesheet]')].every((l) => l.sheet !== null)";

    /// <summary>Records one observation of a row and keeps the page's screenshot beside the record.</summary>
    protected async Task ObserveAsync(string row, ScriptPath path, string engine, WatchedPage page, string step, string expected, string observed, bool ok, string? data = null)
    {
        int width = page.Page.ViewportSize?.Width ?? 0;
        string name = $"{row}-{engine}-{(path == ScriptPath.Scripted ? "betikli" : "betiksiz")}-{Interlocked.Increment(ref _shots):D4}.png";
        string directory = System.IO.Path.Combine(Host.Record.Directory, "screenshots");
        Directory.CreateDirectory(directory);
        string? shot = null;
        try
        {
            await page.Page.ScreenshotAsync(new PageScreenshotOptions { Path = System.IO.Path.Combine(directory, name), FullPage = false });
            shot = "screenshots/" + name;
        }
        catch (PlaywrightException)
        {
            // A page that cannot be drawn (a download, an aborted load) has no image; the line still stands with its text.
        }

        // The free "observed" text carries addresses and page texts, so it passes the same mask as the data record (12 7.1: personal data masked, no access key in the evidence).
        string maskedObserved = DataRecord.Mask(observed);
        Host.Record.Add(new Observation(row, path == ScriptPath.Scripted ? "betikli" : "betiksiz", engine, width, step, expected, maskedObserved, ok, shot, data));
        if (!ok)
        {
            Failures.Add($"{row} {engine} {path} - {step}: expected [{expected}], observed [{maskedObserved}]");
        }
    }
}

/// <summary>The data record of a row (12 4.17 "Veri kaydı"; T551 criterion 3): the table row read back, with the personal data masked (12 7.1).</summary>
public static class DataRecord
{
    private static readonly System.Text.RegularExpressions.Regex Email = new(@"[^\s;=,]+@[^\s;=,]+", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(2));
    private static readonly System.Text.RegularExpressions.Regex Digits = new(@"\d{7,}", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(2));
    private static readonly System.Text.RegularExpressions.Regex Iban = new(@"TR\d{2}[\d ]{10,}", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(2));

    // The guest order's access key and the key of an e-mailed link (invitation, verification, reset, revert; the panel's addresses end in the same segments) are the credential in the address; the pending-registration token travels as a query value.
    private static readonly System.Text.RegularExpressions.Regex AccessKey = new(@"(?<=/siparis/erisim/|/panel/davet/|/dogrulama/|/sifre-sifirlama/|/geri-al/)[^/?#\s""]+", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(2));
    private static readonly System.Text.RegularExpressions.Regex QueryToken = new(@"(?<=[?&](?:pendingRegistration|reauthenticationToken)=)[^&#\s""]+", System.Text.RegularExpressions.RegexOptions.IgnoreCase, TimeSpan.FromSeconds(2));

    /// <summary>Masks an e-mail address, an IBAN, a long digit run (a phone, a number), an order access key and a pending-registration token in a value the record keeps.</summary>
    public static string Mask(string text) => Digits.Replace(Iban.Replace(Email.Replace(QueryToken.Replace(AccessKey.Replace(text, "***"), "***"), "***@***"), "TR**IBAN**"), "***");

    /// <summary>Reads the first row of the query as <c>column=value; ...</c>, masked; "satır yok" when the query gives none.</summary>
    public static async Task<string> RowAsync(string dsn, string sql, params (string Name, object Value)[] parameters)
    {
        await using Npgsql.NpgsqlConnection connection = new(dsn);
        await connection.OpenAsync(CancellationToken.None);
        await using Npgsql.NpgsqlCommand command = new(sql, connection);
        foreach ((string name, object value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        await using Npgsql.NpgsqlDataReader reader = await command.ExecuteReaderAsync(CancellationToken.None);
        if (!await reader.ReadAsync(CancellationToken.None))
        {
            return "satır yok";
        }

        List<string> cells = [];
        for (int i = 0; i < reader.FieldCount; i++)
        {
            cells.Add($"{reader.GetName(i)}={(reader.IsDBNull(i) ? "NULL" : Mask(Convert.ToString(reader.GetValue(i), System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty))}");
        }

        return string.Join("; ", cells);
    }

    /// <summary>
    /// The first cell of the query as it is stored, not masked: a value a test compares or derives a key from (a link's id, a password digest). It never reaches the record; a caller that writes it into an observation passes it through <see cref="Mask"/> by the
    /// observation itself.
    /// </summary>
    public static async Task<string> ScalarAsync(string dsn, string sql, params (string Name, object Value)[] parameters)
    {
        await using Npgsql.NpgsqlConnection connection = new(dsn);
        await connection.OpenAsync(CancellationToken.None);
        await using Npgsql.NpgsqlCommand command = new(sql, connection);
        foreach ((string name, object value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        return Convert.ToString(await command.ExecuteScalarAsync(CancellationToken.None), System.Globalization.CultureInfo.InvariantCulture) ?? string.Empty;
    }

    /// <summary>One number of the database (a count).</summary>
    public static async Task<long> CountAsync(string dsn, string sql, params (string Name, object Value)[] parameters)
    {
        await using Npgsql.NpgsqlConnection connection = new(dsn);
        await connection.OpenAsync(CancellationToken.None);
        await using Npgsql.NpgsqlCommand command = new(sql, connection);
        foreach ((string name, object value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        return Convert.ToInt64(await command.ExecuteScalarAsync(CancellationToken.None), System.Globalization.CultureInfo.InvariantCulture);
    }
}

/// <summary>A page and the error messages the browser gave while it was open.</summary>
public sealed class WatchedPage(IPage page)
{
    public IPage Page { get; } = page;

    public List<string> Errors { get; } = [];

    /// <summary>How many script requests the page made that failed at the network (the accessibility pass cuts them on the scriptless way, AccessibilityScanTests.AuditContextAsync).</summary>
    public int CutScripts => CutScriptUrls.Count;

    /// <summary>The addresses of the script requests that failed at the network, one per request (D-1746 c: the cut-script exemption matches the console line by the address, not by the count alone).</summary>
    public List<string> CutScriptUrls { get; } = [];

    /// <summary>The address each line of <see cref="Errors"/> came from (the console message's location), at the same index; empty when the engine gave none or the line is a page error.</summary>
    public List<string> ErrorUrls { get; } = [];

    /// <summary>The address part of a console message's location ("address:line:column"; the line and column are dropped).</summary>
    internal static string UrlOfLocation(string? location)
    {
        if (string.IsNullOrWhiteSpace(location))
        {
            return string.Empty;
        }

        string[] parts = location.Split(':');
        int drop = parts.Length >= 3 && parts[^1].All(char.IsAsciiDigit) && parts[^2].All(char.IsAsciiDigit) ? 2 : 0;
        return string.Join(':', parts.Take(parts.Length - drop));
    }
}
