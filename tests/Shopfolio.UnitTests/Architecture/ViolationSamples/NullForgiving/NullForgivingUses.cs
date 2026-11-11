// Deliberate violation sample of the null-forgiving scan (09 3.1 item 5; T550): the scan reads this file as text; it is not compiled.
// Line numbers are part of the test. Lines 5-11 are findings; the lines after them are not.
internal static class NullForgivingUses
{
    public static string A(Product p) => p.Description!;
    public static int B(Item i) => i.VariantId!.Value;
    public static string C(Order? o) => Load(o)!.Name;
    public static string D(string?[] a) => a[0]!;
    public static string E() => $"{Find()!.Name} was found";
    public static Foo F(IServiceProvider s) => s.GetService<Foo>()!; // a comment without the marker
    public static string G = null!; // non-null: short
    public static int H(Item i) => i.VariantId!.Value; // non-null: the Where above keeps only rows with a variant
    public static bool I(int a, int b) => a != b && !Flag(a);
    public static string J() => "Hello! It works!=";
    // A comment that says x!.Value is not a finding.
    /* nor a block comment: y!.Value */
    public static string K() => @"verbatim ""quoted""! text";
    public static string L() => """raw "text"! here""";
    public static char M() => '!';
    public static Func<bool, bool> N() => x => !x;
    public static string O(Product p) => p.Description is { } d ? d : throw new InvalidOperationException("No description.");
}
