// Deliberate violation sample of the panel form scan (09 19.2 item 3): a bound page model member that carries the name of a product constant.
// This file is excluded from compilation and from the real scan; the scan test reads it as text.
// The violation is on line 7.
public sealed class BoundConstantPageModel
{
    [BindProperty]
    public int ShortSessionHours { get; set; }
}
