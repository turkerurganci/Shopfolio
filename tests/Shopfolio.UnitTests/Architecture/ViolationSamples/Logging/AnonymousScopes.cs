// Deliberate violation sample of the template scan for the forms a string scan cannot see (T604 A3, D-1005 forms 3 and 4; 09 18.2 item 2a): the member
// names of an anonymous object handed to a scope or destructured into a line, and a template kept in a variable. The scan reads this file as text; it is
// not compiled. Line numbers are part of the test.
internal sealed class AnonymousScopes
{
    public void Run(Order order, string email)
    {
        using (_logger.BeginScope(new { Email = email, OrderId = order.Id })) { }
        using (_logger.BeginScope(new { order.Phone })) { }
        _logger.LogInformation("{Event} {@Data}", LogEvents.JobFinished, new { OrderId = order.Id, Path = "x" });
        using (_logger.BeginScope(new { OrderId = order.Id, Total = order.Total })) { }
        var template = "{Event} {Email}";
        string kept = "{Event} {Count}";
        var route = "/x/{key}";
        _logger.LogInformation(template, LogEvents.JobFinished, email);
        using (_logger.BeginScope(new { Items = new { Address = 1 } })) { }
        var holder = new { Email = email };
    }
}
