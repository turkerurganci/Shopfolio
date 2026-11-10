using System.Text.Json;
using System.Text.RegularExpressions;

using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Integrations;

/// <summary>
/// The IAM policy of the runbook (T407 criterion 8, KK8; 08 2.2 "Kimlik doğrulama"): three permissions; the send permission is tied to the sender domain identity's
/// ARN and to <c>ses:FromAddress</c>; the list and single-address query permissions are account level, unconditional and in a separate statement; there is no delete
/// or put permission. The runbook is the installation step, so the test reads the policy block of the file itself.
/// </summary>
public sealed class SesPolicyRunbookTests
{
    private const string SendAction = "ses:SendEmail";
    private const string ListAction = "ses:ListSuppressedDestinations";
    private const string GetAction = "ses:GetSuppressedDestination";

    private static JsonElement[] Statements()
    {
        string path = Path.Combine(RepositoryRoot.Find(), "Docs", "INTEGRATION_RUNBOOKS", "AMAZON_SES.md");
        string markdown = File.ReadAllText(path);
        Match block = Regex.Match(markdown, "```json\\s*(?<json>.*?)```", RegexOptions.Singleline | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));
        Assert.True(block.Success, "The runbook has no JSON policy block.");
        Assert.Single(Regex.Matches(markdown, "```json", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)));

        using JsonDocument document = JsonDocument.Parse(block.Groups["json"].Value);
        return [.. document.RootElement.GetProperty("Statement").EnumerateArray().Select(s => s.Clone())];
    }

    private static string[] ActionsOf(JsonElement statement)
    {
        JsonElement action = statement.GetProperty("Action");
        return action.ValueKind == JsonValueKind.String
            ? [action.GetString()!]
            : [.. action.EnumerateArray().Select(a => a.GetString()!)];
    }

    [Fact]
    public void Policy_HasExactlyTheThreePermissions_AndNoOther()
    {
        JsonElement[] statements = Statements();

        string[] actions = [.. statements.SelectMany(ActionsOf).Order(StringComparer.Ordinal)];

        Assert.Equal([GetAction, ListAction, SendAction], actions);
        Assert.All(statements, s => Assert.Equal("Allow", s.GetProperty("Effect").GetString()));
        Assert.DoesNotContain(actions, a => a.Contains('*', StringComparison.Ordinal));
        Assert.DoesNotContain(actions, a => a.Contains("Delete", StringComparison.Ordinal) || a.Contains("Put", StringComparison.Ordinal));
    }

    [Fact]
    public void SendStatement_IsTiedToTheSenderDomainIdentity_AndToTheFromAddress()
    {
        JsonElement send = Assert.Single(Statements(), s => ActionsOf(s).Contains(SendAction));

        Assert.Equal([SendAction], ActionsOf(send));
        Assert.Equal(JsonValueKind.String, send.GetProperty("Resource").ValueKind);
        Assert.Equal("arn:aws:ses:{bölge}:{hesap}:identity/{alan adı}", send.GetProperty("Resource").GetString());

        JsonProperty conditionOperator = Assert.Single(send.GetProperty("Condition").EnumerateObject());
        Assert.Equal("StringEquals", conditionOperator.Name);
        JsonProperty key = Assert.Single(conditionOperator.Value.EnumerateObject());
        Assert.Equal("ses:FromAddress", key.Name);
        Assert.Equal("{gönderen adresi}", key.Value.GetString());
    }

    [Fact]
    public void ListAndGetPermissions_AreOneSeparateUnconditionalStatement_OnAllResources()
    {
        JsonElement[] statements = Statements();
        JsonElement account = Assert.Single(statements, s => ActionsOf(s).Contains(ListAction));

        Assert.Equal([GetAction, ListAction], ActionsOf(account).Order(StringComparer.Ordinal));
        Assert.Equal("*", account.GetProperty("Resource").GetString());
        Assert.False(account.TryGetProperty("Condition", out _), "The account level statement must have no condition: the send condition would deny these requests.");
        Assert.NotEqual(Assert.Single(statements, s => ActionsOf(s).Contains(SendAction)).GetRawText(), account.GetRawText());
    }
}
