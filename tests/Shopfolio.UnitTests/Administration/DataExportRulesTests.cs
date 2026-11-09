using Shopfolio.Infrastructure;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Administration;

/// <summary>
/// The pure rules of the exports (T384 KK1, KK8; 04 9.24.8; 07 9.24.2; 09 3.5 item 4): the order range's faults come in the order the interface spec gives, a one-day range is allowed, the
/// reverse proxy's wait on the export paths is the documented 150 seconds and above the production ceiling, and the trail's three export operations carry no target record. The file, the trail row
/// and the directory on a real database are the integration tests' (DataExportTests).
/// </summary>
public sealed class DataExportRulesTests
{
    private static readonly DateOnly Today = new(2026, 11, 20);

    [Theory]
    [InlineData("2026-11-01", "2026-11-05", ExportRangeError.None)]
    [InlineData("2026-11-20", "2026-11-20", ExportRangeError.None)]
    [InlineData("2026-11-05", "2026-11-05", ExportRangeError.None)]
    [InlineData(null, "2026-11-05", ExportRangeError.StartMissing)]
    [InlineData("2026-11-01", null, ExportRangeError.EndMissing)]
    [InlineData(null, null, ExportRangeError.StartMissing)]
    [InlineData("2026-11-21", "2026-11-25", ExportRangeError.StartInFuture)]
    [InlineData("2026-11-21", "2026-11-19", ExportRangeError.StartInFuture)]
    [InlineData("2026-11-01", "2026-11-21", ExportRangeError.EndInFuture)]
    [InlineData("2026-11-05", "2026-11-04", ExportRangeError.EndBeforeStart)]
    public void TheRange_IsCheckedInTheOrderOfTheSpec_MissingThenFutureThenReversed(string? start, string? end, ExportRangeError expected)
    {
        Assert.Equal(expected, DataExport.CheckRange(Day(start), Day(end), Today));
    }

    [Fact]
    public void TheProxysWait_OnTheExportPaths_IsTheValueInTheCaddyfile_AndTheNoteBeforeItNamesTheCeiling()
    {
        string caddyfile = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "deploy", "caddy", "Caddyfile"));
        int block = caddyfile.IndexOf("handle /panel/disa-aktarma*", StringComparison.Ordinal);

        Assert.True(block >= 0, "the export paths have a reverse proxy block of their own");
        string text = caddyfile[block..];
        int closing = text.IndexOf("\n\thandle", 1, StringComparison.Ordinal);
        string exportBlock = closing > 0 ? text[..closing] : text;
        Assert.Contains($"response_header_timeout {(int)ExportLimits.ReverseProxyHeaderWait.TotalSeconds}s", exportBlock, StringComparison.Ordinal);
        Assert.Equal(1, caddyfile.Split("response_header_timeout", StringSplitOptions.None).Length - 1);
    }

    [Fact]
    public void TheExportOperations_AreInTheExportArea_AndTakeNoTargetRecord()
    {
        AuditOperation[] exports = [AuditOperation.ExportOrders, AuditOperation.ExportCustomers, AuditOperation.ExportContactRequests];

        Assert.All(exports, o => Assert.Equal(AuditArea.Export, AuditOperationText.AreaOf(o)));
        Assert.All(exports, o => Assert.Empty(AuditEntryRules.TargetTypesOf(o)!));
        Assert.Equal(exports.Order(), Enum.GetValues<AuditOperation>().Where(o => AuditOperationText.AreaOf(o) == AuditArea.Export).Order());
    }

    private static DateOnly? Day(string? text) => text is null ? null : DateOnly.ParseExact(text, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
}
