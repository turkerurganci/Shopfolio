using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// What an order item holds is the delivery mark, not a state (T351 criterion 4; KK4; 02 3.20.10, K-179, K-295): no status, state or transition of an item exists as a type, member, enum value,
/// parameter, table or column of the application, and the item's table carries the mark and its date and nothing that reads like a status. Like <see cref="SingleDeliveryWayTests"/>, the
/// scan reads names cut into words, so a property, a method, an enum member, a table and a column are all found in whatever spelling.
/// </summary>
public sealed class ItemHasNoStateMachineTests
{
    private static readonly string[][] ForbiddenPhrases =
    [
        ["item", "status"], ["item", "statuses"], ["item", "state"], ["item", "states"], ["item", "transition"], ["item", "transitions"], ["item", "stage"], ["item", "stages"],
        ["item", "lifecycle"], ["item", "state", "machine"], ["item", "delivery", "status"],
    ];

    [Fact]
    public void NoTypeMemberEnumValueOrParameterOfTheApplication_IsAnItemStatusAStateOrATransition_KK4()
    {
        List<string> findings = [.. SalesModelShape.NamesOf(typeof(OrderStatus).Assembly).SelectMany(n => FindingsIn(n.Name).Select(f => $"{n.Owner}: {f}"))];

        Assert.Empty(findings);
    }

    [Fact]
    public void TheModelHasNoTableOrColumnOfAnItemStatusAStateOrATransition_KK4()
    {
        IModel model = NewModel();
        List<string> findings = [];
        foreach (IEntityType entity in model.GetEntityTypes())
        {
            string table = entity.GetTableName() ?? entity.ShortName();
            findings.AddRange(FindingsIn(table).Select(f => $"{table}: {f}"));
            foreach (IProperty property in entity.GetProperties())
            {
                string column = property.GetColumnName();
                findings.AddRange(FindingsIn(column).Select(f => $"{table}.{column}: {f}"));
            }
        }

        Assert.Empty(findings);
    }

    [Fact]
    public void TheOrderItemsTable_HoldsTheMarkAndItsDate_AndNoColumnThatReadsLikeAStatus_KK4()
    {
        IEntityType item = NewModel().GetEntityTypes().Single(e => e.GetTableName() == "order_items");
        string[] columns = [.. item.GetProperties().Select(p => p.GetColumnName())];

        Assert.Contains("delivery_marked_at", columns);
        Assert.Contains("delivery_date", columns);
        Assert.DoesNotContain(columns, c => SalesModelShape.Words(c).Any(w => w is "status" or "state" or "stage" or "phase"));
    }

    [Fact]
    public void TheTransitionRecord_BelongsToTheOrder_AndNamesNoItem_KK4()
    {
        IEntityType record = NewModel().GetEntityTypes().Single(e => e.GetTableName() == "transition_records");

        Assert.DoesNotContain(record.GetProperties().Select(p => p.GetColumnName()), c => SalesModelShape.Words(c).Contains("item", StringComparer.Ordinal));
    }

    [Theory]
    [InlineData("OrderItemStatus", "item status")]
    [InlineData("item_state", "item state")]
    [InlineData("ItemTransition", "item transition")]
    [InlineData("order_item_transitions", "item transitions")]
    [InlineData("ItemDeliveryStatus", "item delivery status")]
    [InlineData("IsItemLifecycle", "item lifecycle")]
    public void TheScan_FindsEachForbiddenFormInItsSpelling(string name, string phrase)
    {
        Assert.Contains(phrase, FindingsIn(name));
    }

    [Theory]
    [InlineData("DeliveryMarkedAt")]
    [InlineData("DeliveryDate")]
    [InlineData("OrderItem")]
    [InlineData("ItemRecordRefusal")]
    [InlineData("order_status")]
    [InlineData("DefectClaimStatus")]
    [InlineData("MarkServiceCompletedAsync")]
    public void TheScan_DoesNotMistakeAnOrdinaryNameForAForbiddenOne(string name)
    {
        Assert.Empty(FindingsIn(name));
    }

    private static List<string> FindingsIn(string name)
    {
        List<string> words = SalesModelShape.Words(name);
        List<string> findings = [];
        foreach (string[] phrase in ForbiddenPhrases)
        {
            for (int start = 0; start + phrase.Length <= words.Count; start++)
            {
                if (words.Skip(start).Take(phrase.Length).SequenceEqual(phrase, StringComparer.Ordinal))
                {
                    findings.Add(string.Join(' ', phrase));
                    break;
                }
            }
        }

        return findings;
    }

    private static IModel NewModel()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, "Host=127.0.0.1;Database=never_opened");
        using AppDbContext context = new(builder.Options);
        return context.GetService<IDesignTimeModel>().Model;
    }
}
