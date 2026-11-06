namespace Shopfolio.Infrastructure.Binding;

/// <summary>
/// Marks a model field whose name 06 4 does not carry: the name is the one 07 gives once, in bold, in its model tables (07 5.1.2). A field without
/// the mark takes the name of a property of a 06 entity (07 5.1.2: the name of 06 4 when it has a counterpart); the architecture rule reads both.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class SpecAssignedNameAttribute : Attribute
{
}
