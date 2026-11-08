using System.Text;
using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the bulk writes that do not pass through a save (T208; D-95 (c); 09 10.2 items 2-4): the order side and the product form are guarded by
/// the two save interceptors (<c>OrderVersionGuard</c>, <c>ProductVersionGuard</c>), and a bulk statement - <c>ExecuteUpdate</c>, <c>ExecuteDelete</c>, a raw
/// <c>UPDATE</c> or <c>DELETE</c> - never passes through a save, so it would change a record the version covers without the raise being seen. Three rules,
/// over the whole text of every application <c>.cs</c> file (comments blanked, a statement split over lines is found):
/// <list type="number">
/// <item>A statement that names a set of the order side or of the product form (<c>db.Orders</c>, <c>db.Variants</c>, <c>Set&lt;Order&gt;()</c>) and ends in a bulk
/// write (<c>ExecuteUpdate</c>, <c>ExecuteDelete</c>) or hands the rows to <c>CurrentStateWrite</c>.</item>
/// <item>The text of a raw write - <c>UPDATE</c>, <c>DELETE FROM</c> or <c>INSERT INTO</c> of a table of those sides - wherever it is handed to the database (<c>ExecuteSql*</c>, <c>SqlQuery</c>, a command object's <c>CommandText</c>, a variable).</item>
/// <item>A bulk write of any kind inside the modules that own those sides (<c>Ordering</c>, <c>AfterSales</c>, <c>Payment</c>, <c>Catalog</c>) outside the closed list of
/// the places that have one (the counters' one writer, the download counter's one writer, the guest cart's disposal, the image derivative job, the disposal of unlinked images): a set reached through a variable or a helper is
/// not named in its statement, but the module is.</item>
/// </list>
/// A bulk write of a set outside the sides (sessions, links, heartbeats, the job queue) is not a finding. The sets and the tables are the ones the two
/// guards cover; a unit test derives them from the model and holds this list equal to them. The places are full paths. The test projects are not scanned.
/// </summary>
internal static partial class VersionedSideBulkWriteScan
{
    public const string NamedSetBulkWrite = "writes a record of the order side or of the product form by a bulk statement (09 10.2 items 2-4: it does not pass the version guard)";
    public const string RawStatementOnSide = "writes a table of the order side or of the product form by a raw statement (09 10.2 items 2-4: it does not pass the version guard)";
    public const string BulkWriteInSideModule = "a bulk write in a module that owns the order side or the product form, outside the closed list of places (09 10.2 items 2-4)";

    /// <summary>The sets of the order side: the order and the records <c>OrderVersionGuard</c> covers.</summary>
    public static readonly IReadOnlyList<string> OrderSideSets =
    [
        "Orders", "OrderItems", "Cancellations", "ItemRemovals", "LateDeliveryTerminations", "Withdrawals", "Returns", "ReturnRejections",
        "NotReturnedClosures", "DefectClaims", "RefundIbanRequests", "PaymentRecords", "Refunds", "RefundRecords", "RefundIbans",
        "OrderAddressHistories", "OrderEmailHistories", "StockReservations", "CheckboxAcceptances", "CheckboxAcceptanceOrderItems",
    ];

    /// <summary>The sets of the product form: the product and the records <c>ProductVersionGuard</c> covers.</summary>
    public static readonly IReadOnlyList<string> ProductFormSets =
    [
        "Products", "Variants", "Options", "OptionValues", "ProductCategories", "ProductImages", "Discounts",
    ];

    /// <summary>The tables of those sets.</summary>
    public static readonly IReadOnlyList<string> Tables =
    [
        "orders", "order_items", "cancellations", "item_removals", "late_delivery_terminations", "withdrawals", "returns", "return_rejections",
        "not_returned_closures", "defect_claims", "refund_iban_requests", "payment_records", "refunds", "refund_records", "refund_ibans",
        "order_address_histories", "order_email_histories", "stock_reservations", "checkbox_acceptances", "checkbox_acceptance_order_items",
        "products", "variants", "options", "option_values", "product_categories", "product_images", "discounts",
    ];

    /// <summary>The module folders that own the two sides.</summary>
    public static readonly IReadOnlyList<string> SideModules =
    [
        "src/Shopfolio.Web/Modules/Ordering/", "src/Shopfolio.Web/Modules/AfterSales/", "src/Shopfolio.Web/Modules/Payment/", "src/Shopfolio.Web/Modules/Catalog/",
    ];

    /// <summary>The places inside those modules that have a bulk write: the counters' one writer, the download counter's one writer (T340), the guest cart's disposal (a cart is no order), the image derivative job and the disposal of unlinked images (an image row with no link left is no product row), the disposal of unlinked and incomplete digital files (the same), the inbound notification record's disposal and its one insert (the record is no order-side row).</summary>
    public static readonly IReadOnlyList<string> AllowedPlaces =
    [
        "src/Shopfolio.Web/Modules/Catalog/StockCounters.cs",
        DownloadCountersPlace,
        "src/Shopfolio.Web/Modules/Ordering/GuestCartDisposalPart.cs",
        "src/Shopfolio.Web/Modules/Catalog/ImageDerivativeJobHandler.cs",
        "src/Shopfolio.Web/Modules/Catalog/ImageDisposalParts.cs",
        "src/Shopfolio.Web/Modules/Catalog/DigitalFileDisposalParts.cs",
        "src/Shopfolio.Web/Modules/Payment/InboundNotificationDisposalPart.cs",
        GalleryMainMarkPlace,
        InboundNotificationInsertPlace,
        CartStatementsPlace,
    ];

    /// <summary>The cart's conditional statements (T346): raw INSERT ... ON CONFLICT on <c>carts</c> and <c>cart_items</c> only - a cart is no order and no product form; T347 adds the one DELETE of the paid order's items from their own cart; a unit test holds the file to those two tables and five statements.</summary>
    public const string CartStatementsPlace = "src/Shopfolio.Web/Modules/Ordering/CartStatements.cs";

    /// <summary>The download counter's one writer (T340): the counter is the customer's, not the firm's write of the item, so it does not raise the order's version (06 2.6.4); its renewal under the order's lock raises it in the same file.</summary>
    public const string DownloadCountersPlace = "src/Shopfolio.Web/Modules/Ordering/DownloadCounters.cs";

    /// <summary>The notification endpoint's one raw statement (T311): the inbound record written with <c>ON CONFLICT DO NOTHING</c> inside the caller's transaction; a file of that one statement and nothing else.</summary>
    public const string InboundNotificationInsertPlace = "src/Shopfolio.Web/Modules/Payment/InboundNotificationInsert.cs";

    /// <summary>The gallery's one bulk statement (T339): the main mark taken off a product's current main image inside the caller's transaction; a file of that one statement and nothing else.</summary>
    public const string GalleryMainMarkPlace = "src/Shopfolio.Web/Modules/Catalog/GalleryMainMark.cs";

    public static bool Applies(string relativePath)
    {
        string path = relativePath.Replace('\\', '/');
        return path.EndsWith(".cs", StringComparison.Ordinal) && !path.Contains("/Migrations/", StringComparison.Ordinal);
    }

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        string path = relativePath.Replace('\\', '/');
        string clean = BlankCommentLines(text.ReplaceLineEndings("\n"));
        bool allowedPlace = AllowedPlaces.Contains(path);
        List<(string Rule, int Line)> findings = [];
        if (!allowedPlace)
        {
            foreach (Match statement in Statement().Matches(clean))
            {
                if (!BulkWrite().IsMatch(statement.Value))
                {
                    continue;
                }

                Match named = NamedSetPattern().Match(statement.Value);
                if (named.Success)
                {
                    AddAt(findings, NamedSetBulkWrite, clean, statement.Index + named.Index);
                }
            }

            // The text of the write is the finding, wherever it is handed to the database: ExecuteSql*, SqlQuery, a command object's CommandText, a variable.
            foreach (Match raw in RawOnSide().Matches(clean))
            {
                AddAt(findings, RawStatementOnSide, clean, raw.Index);
            }

            if (SideModules.Any(m => path.StartsWith(m, StringComparison.Ordinal)))
            {
                foreach (Match call in BulkWriteCall().Matches(clean))
                {
                    AddAt(findings, BulkWriteInSideModule, clean, call.Index);
                }
            }
        }

        findings.Sort((a, b) => a.Line != b.Line ? a.Line.CompareTo(b.Line) : string.CompareOrdinal(a.Rule, b.Rule));
        return findings;
    }

    private static void AddAt(List<(string Rule, int Line)> findings, string rule, string text, int index)
    {
        int line = text.AsSpan(0, index).Count('\n') + 1;
        if (!findings.Contains((rule, line)))
        {
            findings.Add((rule, line));
        }
    }

    private static string BlankCommentLines(string text)
    {
        StringBuilder builder = new();
        foreach (string line in text.Split('\n'))
        {
            builder.Append(line.TrimStart().StartsWith("//", StringComparison.Ordinal) ? string.Empty : line).Append('\n');
        }

        return builder.ToString(0, builder.Length - 1);
    }

    // One statement: up to the next semicolon (a lambda's inner semicolons end it early, which only narrows it to the part that holds the set).
    [GeneratedRegex(@"[^;{}]+;", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Statement();

    [GeneratedRegex(@"\bExecute(?:Update|Delete)(?:Async)?\s*\(|\bCurrentStateWrite\s*\.\s*ApplyAsync\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex BulkWrite();

    [GeneratedRegex(@"\bExecute(?:Update|Delete)(?:Async)?\s*\(|\bCurrentStateWrite\s*\.\s*ApplyAsync\b|\bExecuteSql\w*\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex BulkWriteCall();

    // db.Orders, _db.OrderItems, context.Set<Order>(): the set named by member access or by Set<T>().
    [GeneratedRegex(
        @"\.\s*(?:Orders|OrderItems|Cancellations|ItemRemovals|LateDeliveryTerminations|Withdrawals|Returns|ReturnRejections|NotReturnedClosures|DefectClaims|RefundIbanRequests|PaymentRecords|Refunds|RefundRecords|RefundIbans|OrderAddressHistories|OrderEmailHistories|StockReservations|CheckboxAcceptances|CheckboxAcceptanceOrderItems|Products|Variants|Options|OptionValues|ProductCategories|ProductImages|Discounts)\b(?!\s*\()"
        + @"|\bSet\s*<\s*(?:Order|OrderItem|Cancellation|ItemRemoval|LateDeliveryTermination|Withdrawal|Return|ReturnRejection|NotReturnedClosure|DefectClaim|RefundIbanRequest|PaymentRecord|Refund|RefundRecord|RefundIban|OrderAddressHistory|OrderEmailHistory|StockReservation|CheckboxAcceptance|CheckboxAcceptanceOrderItem|Product|Variant|Option|OptionValue|ProductCategory|ProductImage|Discount)\s*>",
        RegexOptions.CultureInvariant,
        matchTimeoutMilliseconds: 5000)]
    private static partial Regex NamedSetPattern();

    // UPDATE orders ..., DELETE FROM variants ..., INSERT INTO refunds ...: the text of a write to a table of the two sides, quoted or not, schema-qualified or not.
    [GeneratedRegex(
        @"\b(?:UPDATE|DELETE\s+FROM|INSERT\s+INTO)\s+(?:ONLY\s+)?(?:(?:\\?"")?public(?:\\?"")?\.)?(?:\\?"")?(?:orders|order_items|cancellations|item_removals|late_delivery_terminations|withdrawals|returns|return_rejections|not_returned_closures|defect_claims|refund_iban_requests|payment_records|refunds|refund_records|refund_ibans|order_address_histories|order_email_histories|stock_reservations|checkbox_acceptances|checkbox_acceptance_order_items|products|variants|options|option_values|product_categories|product_images|discounts)(?:\\?"")?(?![\w])",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase,
        matchTimeoutMilliseconds: 5000)]
    private static partial Regex RawOnSide();
}
