namespace Shopfolio.Infrastructure.Operations;

/// <summary>
/// Marks a POST handler that applies an operation the screen offers only when it is open (07 8.1.2; 09 8.3 item 2): the handler re-checks the operation's
/// rule - <see cref="OperationGate"/>, or <c>OperationRunner</c> that calls it - before it writes. The mark is what the source scan reads: a marked handler whose body does not
/// call the gate is refused, so a handler cannot be marked and skip the check. Every POST handler of an operation of 03 1.11 carries it.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
internal sealed class GatedOperationAttribute : Attribute
{
}
