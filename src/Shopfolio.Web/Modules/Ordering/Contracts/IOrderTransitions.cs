namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The open face of the transition unit (09 9.1 item 1; 05 4.1.1, MK-35): what the panel, the storefront and the background jobs call to move an order
/// between states. Every transition is a method whose name is the operation's name (a verb and a dictionary object, 09 5.3) and whose
/// <c>TransitionCode</c> constant stands in one place inside it; the correction is a method of its own that only the panel's administrator calls.
/// The core of the unit - the white list, the order of work, the record and the closure rule - is the task that wrote this interface (T315); the
/// methods arrive with the tasks that own each transition's condition (T316 the shipment axis, T317 the payment axis, T319 the correction), as methods
/// that take and give immutable records and join the caller's database transaction (09 6 item 3). No method is open before its condition is
/// written: a call that named a code and skipped the condition would open the white list's rows without their rules.
/// </summary>
public interface IOrderTransitions
{
}
