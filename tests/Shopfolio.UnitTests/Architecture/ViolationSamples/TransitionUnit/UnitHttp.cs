// Violation sample of the transition unit scan: the unit bound to an outside HTTP type. Excluded from compilation.
// A comment line that names HttpClient is not a finding.
using System.Net.Http;

internal sealed class OrderTransitions
{
    private readonly HttpClient _client;
    private readonly IHttpClientFactory _factory;
    private readonly AppDbContext _db;
}
