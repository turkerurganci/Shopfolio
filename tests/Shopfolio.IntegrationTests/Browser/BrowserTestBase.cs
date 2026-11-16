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

    protected void AssertObserved() => Assert.True(Failures.Count == 0, string.Join(Environment.NewLine, Failures));

    protected static CancellationToken Ct => TestContext.Current.CancellationToken;

    /// <summary>The engine and way pairs every row is observed in: the three engines, the script on and off (T551 criterion 1).</summary>
    public static TheoryData<string, ScriptPath> Ways()
    {
        TheoryData<string, ScriptPath> data = [];
        foreach (string engine in BrowserHost.Engines)
        {
            foreach (ScriptPath path in Enum.GetValues<ScriptPath>())
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
            }
        };
        page.PageError += (_, message) => watched.Errors.Add("page: " + message);
        return watched;
    }

    protected async Task<IResponse?> GoAsync(WatchedPage page, string path)
    {
        IResponse? response = await page.Page.GotoAsync(Host.BaseUrl + path, new PageGotoOptions { WaitUntil = WaitUntilState.Load, Timeout = 30000 });
        return response;
    }

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

        Host.Record.Add(new Observation(row, path == ScriptPath.Scripted ? "betikli" : "betiksiz", engine, width, step, expected, observed, ok, shot, data));
        if (!ok)
        {
            Failures.Add($"{row} {engine} {path} - {step}: expected [{expected}], observed [{observed}]");
        }
    }
}

/// <summary>The data record of a row (12 4.17 "Veri kaydı"; T551 criterion 3): the table row read back, with the personal data masked (12 7.1).</summary>
public static class DataRecord
{
    private static readonly System.Text.RegularExpressions.Regex Email = new(@"[^\s;=,]+@[^\s;=,]+", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(2));
    private static readonly System.Text.RegularExpressions.Regex Digits = new(@"\d{7,}", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(2));
    private static readonly System.Text.RegularExpressions.Regex Iban = new(@"TR\d{2}[\d ]{10,}", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(2));

    /// <summary>Masks an e-mail address, an IBAN and a long digit run (a phone, a number) in a value the record keeps.</summary>
    public static string Mask(string text) => Digits.Replace(Iban.Replace(Email.Replace(text, "***@***"), "TR**IBAN**"), "***");

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
}
