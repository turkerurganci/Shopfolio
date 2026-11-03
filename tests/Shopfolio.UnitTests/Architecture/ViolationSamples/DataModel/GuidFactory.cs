// Deliberate violation sample of the data model source scan (09 19.2 item 3): an id made outside EntityIds.cs.
// This file is excluded from compilation and from the real scan; the scan test reads it as text.
// The violation is on line 6.
public static class GuidFactorySample
{
    public static System.Guid Make() => System.Guid.NewGuid();
}
