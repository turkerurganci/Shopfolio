// Deliberate violation sample of the transaction body scan (09 10.7 item 5): work handed to ITransactionRunner.RunAsync that calls an outside service.
// The scan reads this file as text; it is not compiled. Line numbers are part of the test.
internal sealed class OutsideCallInTransaction(ITransactionRunner runner, IPaymentGateway gateway, HttpClient http)
{
    public Task UsesTheHttpClient(CancellationToken ct) => runner.RunAsync(async token => await http.GetAsync("https://example.test", token), ct);

    public Task UsesTheGatewayField(CancellationToken ct) => runner.RunAsync(
        async token =>
        {
            await gateway.StartAsync(token);
            return true;
        },
        ct);

    public Task NamesTheTypeInTheBody(CancellationToken ct) => runner.RunAsync(async token => await new HttpClient().GetAsync("https://example.test", token), ct);

    public async Task CallsOutsideTheTransaction(CancellationToken ct)
    {
        // A comment that says runner.RunAsync(token => gateway.StartAsync(token)) is not a finding.
        await gateway.StartAsync(ct);
        await runner.RunAsync(async token => await Task.FromResult(1), ct);
    }

    // D-82: the work is a lambda written at the call; a method group or a delegate variable hides its body from the scan.
    public Task HandsAMethodGroup(CancellationToken ct) => runner.RunAsync(PayInside, ct);

    public Task HandsADelegateVariable(Func<CancellationToken, Task<bool>> work, CancellationToken ct) => runner.RunAsync<bool>(
        work,
        ct);

    public Task HandsAStaticLambda(CancellationToken ct) => runner.RunAsync(static (token) => Task.FromResult(true), ct);

    private Task<bool> PayInside(CancellationToken token) => gateway.StartAsync(token);

    // T219: the retry overload (09 9.4 item 3) is a RunAsync call too: its work is scanned in the same way.
    public Task RetryOverloadUsesTheGateway(CancellationToken ct) => runner.RunAsync(async step => await gateway.StartAsync(step), ["ix_products_slug"], ct);

    public Task RetryOverloadHandsAMethodGroup(CancellationToken ct) => runner.RunAsync(PayInside, ["ix_products_slug"], ct);

    public Task RetryOverloadWithTheOutsideCallOutside(CancellationToken ct) => runner.RunAsync(async token => await Task.FromResult(1), ["ix_products_slug"], ct);
}
