using System.Text.RegularExpressions;

using Shopfolio.Infrastructure.Integrations;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the job-to-limit pairing of the link layer equal to the "Zaman aşımı" column of the 08 3.1 table (T202 criterion 3, KK3; 09 21.4
/// item 1): every job of the table that has a limit is an <see cref="IntegrationOperation"/> with that limit, and every operation is a row of
/// the table. The test names each row by its job's words only; the seconds come from the document.
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class IntegrationTimeoutsConsistencyTests
{
    private const string TableLead = "Katmanın işleri, her işin sonuç sınıfları ve deneme sahibi:";

    // The opening words of each job's cell in the 08 3.1 table.
    private static readonly Dictionary<string, IntegrationOperation> Jobs = new(StringComparer.Ordinal)
    {
        ["ödemeyi başlat"] = IntegrationOperation.PaymentStart,
        ["ödemenin sonucunu sor"] = IntegrationOperation.PaymentQuery,
        ["kart iadesi iste"] = IntegrationOperation.RefundRequest,
        ["kart iadesinin sonucunu sor"] = IntegrationOperation.RefundQuery,
        ["iletiyi gönder"] = IntegrationOperation.EmailSend,
        ["geri dönen adresleri sor"] = IntegrationOperation.BounceQuery,
        ["kimliği doğrula"] = IntegrationOperation.IdentityVerify,
    };

    [Fact]
    public void EveryJobWithALimitInThe08Table_IsAnOperationWithThatLimit_AndEveryOperationIsARow()
    {
        Dictionary<IntegrationOperation, TimeSpan> documented = [];
        foreach ((string job, string limit) in TableRows())
        {
            Match seconds = Seconds().Match(limit);
            string? key = KeyOf(job);
            if (!seconds.Success)
            {
                // A local job (the notice's signature check) has no outside call and no limit.
                Assert.Null(key);
                continue;
            }

            Assert.True(key is not null, "a job of the 08 3.1 table with a limit but no operation: " + job);
            documented.Add(Jobs[key], TimeSpan.FromSeconds(int.Parse(seconds.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture)));
        }

        Assert.Equal(Enum.GetValues<IntegrationOperation>().Order(), documented.Keys.Order());
        Assert.All(documented, row => Assert.Equal(row.Value, IntegrationOperations.LimitOf(row.Key)));
    }

    private static IEnumerable<(string Job, string Limit)> TableRows()
    {
        string[] lines = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "08_INTEGRATION_SPEC.md")).ReplaceLineEndings("\n").Split('\n');
        int lead = Array.IndexOf(lines, TableLead);
        Assert.True(lead >= 0, "the 08 3.1 table's lead line");
        string[] header = Cells(lines[lead + 2]);
        int jobColumn = Array.IndexOf(header, "Katmanın işi");
        int limitColumn = Array.IndexOf(header, "Zaman aşımı");
        Assert.True(jobColumn >= 0 && limitColumn >= 0, "the job and limit columns");

        foreach (string line in lines.Skip(lead + 4).TakeWhile(l => l.StartsWith('|')))
        {
            string[] cells = Cells(line);
            yield return (cells[jobColumn], cells[limitColumn]);
        }
    }

    // The longest job words that begin the cell.
    private static string? KeyOf(string job) =>
        Jobs.Keys.Where(k => job.StartsWith(k, StringComparison.Ordinal)).OrderByDescending(k => k.Length).FirstOrDefault();

    private static string[] Cells(string line) => [.. line.Trim().Trim('|').Split('|').Select(c => c.Trim())];

    [GeneratedRegex(@"^(\d+) sn", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex Seconds();
}
