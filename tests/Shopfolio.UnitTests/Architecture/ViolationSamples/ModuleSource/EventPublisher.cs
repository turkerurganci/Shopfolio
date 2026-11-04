// Violation sample of the event publisher scan: a domain event (line 3), an event bus (line 5) and a mediator (line 6). Excluded from compilation.
// A comment line that names IEventBus is not a finding.
public sealed class OrderCancelled : IDomainEvent
{
    public required IEventBus Bus { get; init; }
    public required IMediator Mediator { get; init; }
}
