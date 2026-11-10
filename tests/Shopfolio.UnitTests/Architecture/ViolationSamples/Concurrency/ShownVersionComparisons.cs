// Deliberate violations of the shown version scan (T364; 09 10.2 item 3). Read as text by ShownVersionSourceScanTests; never compiled.
internal sealed class ShownVersionComparisons
{
    public bool Samples(Order order, Request request, int shown)
    {
        bool a = order.Version != shown;
        bool b = shown == order.Version;
        bool c = request.ExpectedVersion != order!.Version;
        bool d = order.Version
            == shown;
        bool e = order.Version is not 3;
        bool f = order.Version.Equals(shown);
        bool g = Equals(order.Version, shown);
        bool h = shown.Equals(order.Version);
        // order.Version != shown in a comment is not code.
        bool i = ShownVersion.Matches(order, shown);
        bool j = template.TemplateVersion != row.TemplateVersion;
        int k = order.Version;
        bool l = (order.Version) != shown;
        bool m = order.Version < shown;
        bool n = shown > order.Version;
        bool o = order.Version.CompareTo(shown) == 0;
        bool p = EqualityComparer<int>.Default.Equals(order.Version, shown);
        bool q = object.Equals(Shown(request), order.Version);
        bool r = order is { Version: 3 };
        bool s = order is not { Status: OrderStatus.Placed, Version: 2 };
        int t = flag ? order.Version : 0;
        Expression<Func<Order, int>> u = o => o.Version;
        var v = new TransitionRequest(order.Id, expectedVersion: order.Version);
        bool w = request.ExpectedVersion is { } seen && !ShownVersion.Matches(order, seen);
        bool x = request.ExpectedVersion is null || ShownVersion.Matches(order, request.ExpectedVersion.Value);
        bool y = request.ExpectedVersion.HasValue
            && !ShownVersion.Matches(order, request.ExpectedVersion.Value);
        bool z = ShownVersion.Matches(order, request.ExpectedVersion ?? 0);
        bool aa = request.ExpectedVersion?.Let(seen => ShownVersion.Matches(order, seen)) ?? true;
        bool ab = request.ExpectedVersion is int seen ? !ShownVersion.Matches(order, seen) : false;
        bool ac = request.ExpectedVersion == null || ShownVersion.Matches(order, request.ExpectedVersion.Value);
        bool ad = !request.ExpectedVersion.HasValue || ShownVersion.Matches(order, request.ExpectedVersion.Value);
        bool ae = request.ExpectedVersion is not null && !ShownVersion.Matches(order, request.ExpectedVersion.Value);
        ShownVersionCheck af = ShownVersion.Check(order, request.ExpectedVersion, byAdministrator: true);
        bool ag = request.ExpectedVersion is null || request.Confirmed;
        bool ah = request.Admin?.AdminName is null;
        return a && b && c && d && e && f && g && h && i && j && k > 0 && l && m && n && o && p && q && r && s && t > 0;
    }
}
