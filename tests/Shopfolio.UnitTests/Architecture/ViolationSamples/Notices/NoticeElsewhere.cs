// Deliberate violation sample of the notice parameter scan (07 2.2.3; 09 5.3 item 6): addresses and reads that build the parameter without the one writer.
// The scan reads this file as text; it is not compiled. Line numbers are part of the test.
internal sealed class NoticeElsewhere
{
    public string Build(HttpRequest request, string text)
    {
        string a = request.Query["notice"];
        string b = request.Query[@"Notice"];
        string c = request.Query["""notice"""];
        string d = "/sepet?notice=" + text;
        string e = $"/sepet?page=2&notice={text}";
        string f = "/sepet?page=2" + "&" + "notice=" + text;
        string g = "/sepet?x=1&amp;notice=Saved";
        string h = "/sepet%3Fnotice=Saved";
        return a + b + c + d + e + f + g + h;
    }

    [FromQuery(Name = "notice")]
    public string? Bound { get; set; }

    // Near names that are no finding: a CSS class, a type and a variable.
    public string Near(string notice) => "conflict-notice" + notice + nameof(PaymentNotice);
}
