// Deliberate violation sample of the notice parameter scan (07 2.2.3; 09 5.3 item 6): reads of the parameter that carry no quoted name.
// The scan reads this file as text; it is not compiled. Line numbers are part of the test.
internal sealed class NoticeUnquotedReads
{
    [BindProperty(SupportsGet = true)]
    public string? Notice { get; set; }

    [FromQuery]
    public string? Notice2 { get; set; }

    public void OnGet(string? notice) => Keep(notice);

    public Task<IActionResult> OnPostSaveAsync(
        int id,
        [FromQuery(Name = Other.Name)] string? Notice,
        CancellationToken cancel) => Run(id, Notice, cancel);

    public string Read(HttpRequest request) => request.Query[NoticeParameter.ParameterName];

    // Near forms that are no finding: a presence check, a handler without the parameter, a longer name and a result property.
    public bool Present(HttpRequest request) => request.Query.ContainsKey(NoticeParameter.ParameterName);

    public void OnGetAsync(int id, string? noticeText, CancellationToken cancel) => Keep(noticeText);

    public MessageView? ShownNotice { get; private set; }

    [BindProperty(SupportsGet = true)]
    public string? Sort { get; set; }
}
