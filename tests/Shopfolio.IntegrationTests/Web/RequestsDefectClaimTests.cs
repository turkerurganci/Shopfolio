using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using NpgsqlTypes;

using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.AfterSales.Contracts;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The defect claim half of the requests list on a real database (T532 criteria 1-3, KK2, KK3; 07 UC-93): the raw statement that adds the claim's last opening to its own columns, the status filter and the
/// order composed over it, against a status history written the way the application writes it (a jsonb list of <c>At</c>/<c>Status</c> entries). The claims are inserted with the foreign keys switched off for
/// the seeding connection, since the list reads the claim row alone.
/// </summary>
[Collection(RequestsPageTestsDefinition.Name)]
public sealed class RequestsDefectClaimTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 10, 12, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _database;

    public RequestsDefectClaimTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new("TRUNCATE defect_claims CASCADE", connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task TheList_OrdersByTheLastOpening_SoAReopenedClaimComesWithItsReopening_AndFiltersByStatus_KK2()
    {
        Guid never = await InsertAsync("Open", Now.AddDays(-5), "[]");
        Guid reopened = await InsertAsync(
            "Open",
            Now.AddDays(-10),
            $"[{{\"At\":\"{Now.AddDays(-9):O}\",\"Status\":\"Resolved\",\"ActorKind\":\"Admin\",\"Quantity\":null}},{{\"At\":\"{Now.AddDays(-1):O}\",\"Status\":\"Open\",\"ActorKind\":\"Admin\",\"Quantity\":null}}]");
        Guid resolved = await InsertAsync("Resolved", Now.AddDays(-3), $"[{{\"At\":\"{Now.AddDays(-2):O}\",\"Status\":\"Resolved\",\"ActorKind\":\"Admin\",\"Quantity\":null}}]");
        await using Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> factory = new ShopfolioWebApplicationFactory(_database.Dsn);
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        IAfterSalesModule module = scope.ServiceProvider.GetRequiredService<IAfterSalesModule>();

        IReadOnlyList<PanelDefectClaimRow> all = await module.ListPanelDefectClaimsAsync([], 10, Ct);
        IReadOnlyList<PanelDefectClaimRow> open = await module.ListPanelDefectClaimsAsync([DefectClaimStatus.Open], 10, Ct);
        IReadOnlyList<PanelDefectClaimRow> firstOnly = await module.ListPanelDefectClaimsAsync([], 1, Ct);

        Assert.Equal([reopened, resolved, never], all.Select(r => r.ClaimId));
        Assert.Equal(Now.AddDays(-1), all[0].OpenedAt);
        Assert.Equal(Now.AddDays(-10), all[0].CreatedAt);
        Assert.Equal(Now.AddDays(-3), all[1].OpenedAt);
        Assert.Equal([reopened, never], open.Select(r => r.ClaimId));
        Assert.All(open, r => Assert.Equal(DefectClaimStatus.Open, r.Status));
        Assert.Equal([reopened], firstOnly.Select(r => r.ClaimId));
        Assert.Empty(await module.ListPanelDefectClaimsAsync([], 0, Ct));
    }

    [Fact]
    public async Task TheCount_FollowsTheSameStatusFilter_KK3()
    {
        await InsertAsync("Open", Now.AddDays(-5), "[]");
        await InsertAsync("Open", Now.AddDays(-4), "[]");
        await InsertAsync("Resolved", Now.AddDays(-3), "[]");
        await using Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program> factory = new ShopfolioWebApplicationFactory(_database.Dsn);
        await using AsyncServiceScope scope = factory.Services.CreateAsyncScope();
        IAfterSalesModule module = scope.ServiceProvider.GetRequiredService<IAfterSalesModule>();

        Assert.Equal(3, await module.CountPanelDefectClaimsAsync([], Ct));
        Assert.Equal(2, await module.CountPanelDefectClaimsAsync([DefectClaimStatus.Open], Ct));
        Assert.Equal(1, await module.CountPanelDefectClaimsAsync([DefectClaimStatus.Resolved], Ct));
        Assert.Equal(2, (await module.ListPanelDefectClaimsAsync([DefectClaimStatus.Open], 10, Ct)).Count);
    }

    private async Task<Guid> InsertAsync(string status, DateTimeOffset createdAt, string historyJson)
    {
        Guid id = Guid.NewGuid();
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using (NpgsqlCommand role = new("SET session_replication_role = replica", connection))
        {
            await role.ExecuteNonQueryAsync(Ct);
        }

        await using NpgsqlCommand command = new(
            "INSERT INTO defect_claims (id, created_at, order_item_id, quantity, description, defect_claim_status, status_history) VALUES (@id, @created, @item, 1, 'Kusurlu', @status, @history)",
            connection);
        command.Parameters.AddWithValue("id", id);
        command.Parameters.AddWithValue("created", createdAt);
        command.Parameters.AddWithValue("item", Guid.NewGuid());
        command.Parameters.AddWithValue("status", status);
        command.Parameters.Add(new NpgsqlParameter("history", NpgsqlDbType.Jsonb) { Value = historyJson });
        await command.ExecuteNonQueryAsync(Ct);
        return id;
    }
}
