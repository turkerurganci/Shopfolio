// Violation sample of the event publisher scan: a domain event (line 3), an event bus (line 5) and a mediator (line 6). Excluded from compilation.
// A comment line that names IEventBus is not a finding.
public sealed class OrderCancelled : IDomainEvent
{
    public required IEventBus Bus { get; init; }
    public required IMediator Mediator { get; init; }
}
public sealed class OrderPlaced : INotification
{
    public List<object> DomainEvents { get; } = [];
    public void Raise(object e) => AddDomainEvent(e);
    public INotificationsModule Clean { get; init; } // a module interface named INotificationsModule is not a finding (line 12)
}
