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
}
