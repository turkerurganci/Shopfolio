using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Panel;

namespace Shopfolio.UnitTests.Administration;

/// <summary>
/// The pure parts of the sales summary and the data export pages (T524; 04 E-51, E-53; 07 UC-105, UC-182; 11 T524 criteria 1, 3, 4, 7, 10): the ready period a parameter names, the ratio
/// card with its numerator and denominator or "Değerlendirilemez", the field rule of a refused export range and the streamed CSV answer. The pages' requests are in the integration tests.
/// </summary>
public sealed class SalesSummaryAndExportPageTests
{
    [Theory]
    [InlineData("Last7Days", SalesSummaryPreset.Last7Days)]
    [InlineData("Last30Days", SalesSummaryPreset.Last30Days)]
    [InlineData("LastMonth", SalesSummaryPreset.LastMonth)]
    public void ReadPreset_NamesTheOfferedPeriod(string value, SalesSummaryPreset expected)
    {
        Assert.Equal(expected, SalesSummaryModel.ReadPreset(value, windowOffered: false));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("last30days")]
    [InlineData("Yesterday2")]
    [InlineData("99")]
    public void ReadPreset_OnAValueOutsideTheList_IsNone(string? value)
    {
        Assert.Null(SalesSummaryModel.ReadPreset(value, windowOffered: true));
    }

    [Fact]
    public void ReadPreset_TheMeasurementWindow_OnlyWhenSalesEverOpened()
    {
        Assert.Equal(SalesSummaryPreset.MeasurementWindow, SalesSummaryModel.ReadPreset(nameof(SalesSummaryPreset.MeasurementWindow), windowOffered: true));
        Assert.Null(SalesSummaryModel.ReadPreset(nameof(SalesSummaryPreset.MeasurementWindow), windowOffered: false));
    }

    [Fact]
    public void Ratio_WithADenominator_ShowsTheShareAndBothCounts()
    {
        SummaryMeasureView card = SalesSummaryModel.Ratio("odeme", "Ödeme tamamlama oranı", new SummaryRatio(1, 3), "ödeme denemesi", "Tanım");

        Assert.Equal(DisplayFormat.Percent(33.3m), card.Value);
        Assert.Equal("1 / 3", card.Detail);
    }

    [Fact]
    public void Ratio_WithAZeroDenominator_IsNotAssessableWithTheReason()
    {
        SummaryMeasureView card = SalesSummaryModel.Ratio("odeme", "Ödeme tamamlama oranı", new SummaryRatio(0, 0), "ödeme denemesi", "Tanım");

        Assert.Equal("Değerlendirilemez", card.Value);
        Assert.Equal("Değerlendirilemez — bu dönemde ödeme denemesi yok.", card.Detail);
    }

    [Theory]
    [InlineData(ExportRangeError.StartMissing, DateRangeOutcome.StartMissing)]
    [InlineData(ExportRangeError.EndMissing, DateRangeOutcome.EndMissing)]
    [InlineData(ExportRangeError.StartInFuture, DateRangeOutcome.StartInTheFuture)]
    [InlineData(ExportRangeError.EndInFuture, DateRangeOutcome.EndInTheFuture)]
    [InlineData(ExportRangeError.EndBeforeStart, DateRangeOutcome.EndBeforeStart)]
    public void OutcomeOf_GivesTheFieldRuleOfEachRefusal(ExportRangeError error, DateRangeOutcome expected)
    {
        Assert.Equal(expected, DataExportModel.OutcomeOf(error));
    }

    [Fact]
    public async Task CsvFileResult_FromAStream_SendsTheRestOfTheStreamAsAnAttachment()
    {
        DefaultHttpContext http = new();
        http.Response.Body = new MemoryStream();
        ActionContext context = new() { HttpContext = http };
        byte[] bytes = "xxa;b\n1;2\n"u8.ToArray();
        MemoryStream file = new(bytes) { Position = 2 };

        await new CsvFileResult("uyeler.csv", file).ExecuteResultAsync(context);

        Assert.Equal(StatusCodes.Status200OK, http.Response.StatusCode);
        Assert.Equal(bytes.Length - 2, http.Response.ContentLength);
        Assert.Equal("noindex", http.Response.Headers["X-Robots-Tag"].ToString());
        Assert.Equal("no-store", http.Response.Headers.CacheControl.ToString());
        http.Response.Body.Position = 0;
        using MemoryStream copy = new();
        await http.Response.Body.CopyToAsync(copy, TestContext.Current.CancellationToken);
        Assert.Equal(bytes[2..], copy.ToArray());
    }
}
