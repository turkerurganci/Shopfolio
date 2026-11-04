// Deliberate violation sample of the setting layer scan (09 19.2 item 3): a settings class that carries the name of a product constant.
// This file is excluded from compilation and from the real scan; the scan test reads it as text.
// The violation is on line 6.
public sealed class MixedOptions
{
    public int CategoryLevelMax { get; set; } = 3;
}
