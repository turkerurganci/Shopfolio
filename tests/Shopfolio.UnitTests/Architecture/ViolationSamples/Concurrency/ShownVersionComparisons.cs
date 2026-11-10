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
        return a && b && c && d && e && f && g && h && i && j && k > 0;
    }
}
