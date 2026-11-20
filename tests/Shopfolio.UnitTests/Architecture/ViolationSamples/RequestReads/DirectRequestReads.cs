// Deliberate violation sample of the direct request read scan (D-1614; 07 5.3.1): reads a page model makes on the request itself.
// The scan reads this file as text; it is not compiled. Line numbers are part of the test.
internal sealed class DirectRequestReads
{
    public void Read(HttpContext context)
    {
        string a = Request.Form["Input.Name"];
        string b = HttpContext.Request.Query["page"];
        bool c = Request.Form.ContainsKey("Input.Name");
        bool d = Request.Query.TryGetValue("sort", out StringValues sort);
        IFormCollection e = await Request.ReadFormAsync(cancel);
        string f = Request . Form [ nameof(Input.Name) ];
    }

    // Near forms that are no read of the request: a comment, a string, a model's own property and the content-type check.
    public void Near()
    {
        // Request.Form["Input.Name"] is read here by the binder
        string s = "Request.Form is not read";
        bool g = Request.HasFormContentType;
        string h = Input.Form["x"];
    }
}
