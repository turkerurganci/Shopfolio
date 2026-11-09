namespace Shopfolio.Modules.Ordering;

internal sealed class KeyWriters
{
    // The comment names OrderAccessKey = "x" and RenewAccessKey() and is no finding.
    public void Write(Order order, AppDbContext db, string typed)
    {
        order.OrderAccessKey = typed;
        bool same = order.OrderAccessKey == typed;
        order.RenewAccessKey();
        db.Orders.ExecuteUpdate(s => s.SetProperty(o => o.OrderAccessKey, typed));
        string shown = order.OrderAccessKey;
    }
}
