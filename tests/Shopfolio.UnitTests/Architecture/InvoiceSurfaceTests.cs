using System.Reflection;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The product issues no invoice and keeps none (T357 KK1, KK3; 02 3.25.1, 3.25.2, 12.1.10; 10 KD-1): no type, member, enum value, parameter, page or column of the application names an
/// invoice or an e-Arşiv / e-Fatura integration, except the one read that hands the order's frozen values to the export (<see cref="IOrderingModule.ListInvoiceDataAsync"/> and its
/// records). The scan reads every name of the application assembly cut into words (<see cref="SalesModelShape.NamesOf"/>; Razor pages and entity properties are names of the assembly too),
/// so an issuing endpoint, an upload or archive field, a sending method or an integration client is found in whatever spelling; it is shown to find each form in a sample (09 19.2 item 3).
/// </summary>
public sealed class InvoiceSurfaceTests
{
    private static readonly Assembly Application = typeof(AppDbContext).Assembly;

    /// <summary>The names of the one invoice read, exactly: the data record, its item, the reader, the open method and the module's field.</summary>
    private static readonly HashSet<string> InvoiceDataRead =
    [
        nameof(InvoiceData), nameof(InvoiceItem), nameof(InvoiceDataReader), nameof(IOrderingModule.ListInvoiceDataAsync), "_invoiceData",
    ];

    [Fact]
    public void NoNameOfTheApplication_IssuesStoresOrIntegratesAnInvoice_BesideTheOneInvoiceDataRead_KK1_KK3()
    {
        List<string> findings = [.. SalesModelShape.NamesOf(Application).Where(n => IsInvoiceSurface(n.Name)).Select(n => n.Owner)];

        Assert.Empty(findings);
    }

    [Fact]
    public void TheScanReadsTheWholeApplication_AndSeesTheInvoiceDataRead()
    {
        List<string> names = [.. SalesModelShape.NamesOf(Application).Select(n => n.Name)];

        Assert.Contains(nameof(IOrderingModule.ListInvoiceDataAsync), names);
        Assert.Contains(nameof(InvoiceData), names);
        Assert.True(names.Count > 5000, "the application surface has thousands of names; a scan that reads few is not reading the application");
    }

    [Theory]
    [InlineData("IssueInvoiceAsync")]
    [InlineData("InvoiceFileId")]
    [InlineData("invoice_file_id")]
    [InlineData("UploadInvoice")]
    [InlineData("SendInvoiceToCustomer")]
    [InlineData("InvoiceArchive")]
    [InlineData("Invoices")]
    [InlineData("EArchiveClient")]
    [InlineData("EArsivEntegrasyonu")]
    [InlineData("EFaturaSender")]
    [InlineData("EInvoiceProvider")]
    [InlineData("FaturaYukle")]
    [InlineData("Pages_Admin_Orders_Invoice")]
    [InlineData("InvoiceDataWriter")]
    public void TheScan_FindsEachFormOfAnInvoiceSurface(string name)
    {
        Assert.True(IsInvoiceSurface(name), name);
    }

    [Theory]
    [InlineData("InvoiceData")]
    [InlineData("ListInvoiceDataAsync")]
    [InlineData("BillingAddress")]
    [InlineData("Archived")]
    [InlineData("ShippingFeeVatBreakdown")]
    public void TheScan_LeavesTheInvoiceDataReadAndUnrelatedNamesAlone(string name)
    {
        Assert.False(IsInvoiceSurface(name), name);
    }

    // A name is an invoice surface when one of its words is an invoice word or it holds the "e archive" / "e invoice" run, unless it is exactly one of the read's names.
    private static bool IsInvoiceSurface(string name)
    {
        if (InvoiceDataRead.Contains(name))
        {
            return false;
        }

        List<string> words = SalesModelShape.Words(name);
        string[] invoiceWords = ["invoice", "invoices", "invoicing", "fatura", "faturas", "efatura", "earsiv", "arsiv"];
        bool run = Enumerable.Range(0, Math.Max(0, words.Count - 1)).Any(i => words[i] == "e" && words[i + 1] is "archive" or "invoice" or "fatura" or "arsiv");
        return run || words.Any(w => invoiceWords.Contains(w));
    }
}
