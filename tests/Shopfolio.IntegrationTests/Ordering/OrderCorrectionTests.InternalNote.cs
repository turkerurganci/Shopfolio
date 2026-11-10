using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>
/// The internal note's lock and conditional write on a real PostgreSQL with two real transactions (09 10.9 item 5 (a)-(e); 06 4.20; K-1051, K-1095, K-1014 (3)):
/// the note reads the order's row under a shared lock - it waits for a disposal that locked first and sees the emptied order, a disposal waits for a note that
/// locked first, two note writes do not wait for each other - and it is written only while the note still holds the text the form showed; otherwise the
/// administrator gets <c>DisplayedValueConflict</c> with the current note and two administrators never silently overwrite each other. (f)-(h) are in
/// <c>TheInternalNote_IsWrittenChangedAndEmptied_…_KK10</c>.
/// </summary>
public sealed partial class OrderCorrectionTests
{
    // The disposal's emptying of the order's personal data (06 9.2.1), as ADisposedOrder_TakesNoInternalNote_KK10 writes it.
    private static string DisposalEmptying(Guid orderId) =>
        "UPDATE orders SET contact_email = NULL, normalized_contact_email = NULL, order_access_key = NULL, ip_address = NULL, cart_id = NULL, customer_id = NULL, "
        + "billing_recipient_name = NULL, billing_province_id = NULL, billing_district = NULL, billing_address_line = NULL, shipping_recipient_name = NULL, shipping_phone = NULL, "
        + $"shipping_province_id = NULL, shipping_district = NULL, shipping_address_line = NULL WHERE id = '{orderId}'";

    [Fact]
    public async Task ADisposalHoldingTheOrdersRowLock_MakesTheNoteWait_AndTheNoteThenSeesTheDisposedOrder_AndWritesNothing_10_9a()
    {
        Order order = await SeedAsync(OrderStatus.Delivered, PaymentStatus.Paid, ProductType.Digital);
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope disposalScope = provider.CreateAsyncScope();
        await using AsyncServiceScope noteScope = provider.CreateAsyncScope();
        AppDbContext disposalDb = disposalScope.ServiceProvider.GetRequiredService<AppDbContext>();

        // The disposal locks the order (FOR UPDATE, as OrderDisposalPart does) and empties it, not yet committed.
        await using IDbContextTransaction disposal = await disposalDb.Database.BeginTransactionAsync(Ct);
        await disposalScope.ServiceProvider.GetRequiredService<OrderLocks>().LockAsync([order.Id], Ct);
        await disposalDb.Database.ExecuteSqlRawAsync(DisposalEmptying(order.Id), Ct);

        int pid = await BackendPidAsync(noteScope);
        Task<SaveInternalNoteResult> waiting = StartNote(noteScope, new SaveInternalNoteRequest(order.Id, AdminCaller, "Müşteri aradı", null));
        await WaitUntilNoteBlockedAsync(pid);
        Assert.False(waiting.IsCompleted);
        await disposal.CommitAsync(Ct);

        SaveInternalNoteResult result = await waiting.WaitAsync(TimeSpan.FromSeconds(30), Ct);
        Assert.Equal(ErrorCode.OperationNotAvailable, result.Code);
        Assert.Equal(0, await CountAsync(c => c.InternalNotes.Where(n => n.OrderId == order.Id)));
        Assert.Equal(0, await CountAsync(c => c.AuditLogs.Where(a => a.TargetRecordId == order.Id)));
    }

    [Fact]
    public async Task ANoteHoldingItsSharedLock_MakesTheDisposalWait_UntilTheNoteEnds_10_9b()
    {
        Order order = await SeedAsync(OrderStatus.Delivered, PaymentStatus.Paid, ProductType.Digital);
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope noteScope = provider.CreateAsyncScope();
        await using AsyncServiceScope disposalScope = provider.CreateAsyncScope();
        AppDbContext noteDb = noteScope.ServiceProvider.GetRequiredService<AppDbContext>();
        AppDbContext disposalDb = disposalScope.ServiceProvider.GetRequiredService<AppDbContext>();

        // The note's write runs to its end inside its transaction and does not commit yet: its shared row lock is held.
        await using IDbContextTransaction note = await noteDb.Database.BeginTransactionAsync(Ct);
        SaveInternalNoteResult written = await noteScope.ServiceProvider.GetRequiredService<IOrderCorrections>()
            .SaveInternalNoteAsync(new SaveInternalNoteRequest(order.Id, AdminCaller, "Müşteri aradı", null), Ct);
        Assert.True(written.IsSaved);
        await noteDb.SaveChangesAsync(Ct);

        int pid = await BackendPidAsync(disposalScope);
        OrderLocks disposalLocks = disposalScope.ServiceProvider.GetRequiredService<OrderLocks>();
        Task waiting = EmptyUnderTheRowLockAsync(disposalDb, disposalLocks, order.Id);
        await WaitUntilNoteBlockedAsync(pid);
        Assert.False(waiting.IsCompleted);
        await note.CommitAsync(Ct);

        await waiting.WaitAsync(TimeSpan.FromSeconds(30), Ct);
        Assert.Null((await ReloadAsync(order.Id)).ContactEmail);
        Assert.Equal(1, await CountAsync(c => c.InternalNotes.Where(n => n.OrderId == order.Id)));
    }

    [Fact]
    public async Task TwoNoteWrites_DoNotBlockEachOther_TheSharedLocksAgree_10_9c()
    {
        Order order = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical);
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope holderScope = provider.CreateAsyncScope();
        AppDbContext holderDb = holderScope.ServiceProvider.GetRequiredService<AppDbContext>();

        // One note write holds the order's shared lock (its first statement) and does not end; the other runs to its end meanwhile.
        await using IDbContextTransaction holder = await holderDb.Database.BeginTransactionAsync(Ct);
        Assert.Equal("Ali@Example.com", await holderScope.ServiceProvider.GetRequiredService<OrderLocks>().LockSharedAsync(order.Id, Ct));

        SaveInternalNoteResult other = await NoteAsync(order.Id, "Kargo gecikti", null).WaitAsync(TimeSpan.FromSeconds(30), Ct);

        Assert.True(other.IsSaved);
        await holder.RollbackAsync(Ct);
        Assert.Equal("Kargo gecikti", await NoteTextAsync(order.Id));
    }

    [Fact]
    public async Task TwoAdministratorsFirstWrites_OneIsSaved_TheOtherGetsTheConflictWithTheCurrentNote_10_9d()
    {
        Order order = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical);
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope firstScope = provider.CreateAsyncScope();
        await using AsyncServiceScope secondScope = provider.CreateAsyncScope();
        AppDbContext firstDb = firstScope.ServiceProvider.GetRequiredService<AppDbContext>();

        // The runner's retry list is the note's unique index on its order, as the model names it.
        Assert.Equal(
            [firstDb.Model.FindEntityType(typeof(InternalNote))!.GetIndexes().Single(i => i.IsUnique).GetDatabaseName()!],
            SaveInternalNoteRequest.RetryOnUniqueIndexes);

        // Both forms showed no note. The first inserts and does not commit yet; the second's insert waits on the unique index.
        await using IDbContextTransaction first = await firstDb.Database.BeginTransactionAsync(Ct);
        SaveInternalNoteResult firstResult = await firstScope.ServiceProvider.GetRequiredService<IOrderCorrections>()
            .SaveInternalNoteAsync(new SaveInternalNoteRequest(order.Id, AdminCaller, "Birinci yöneticinin notu", null), Ct);
        Assert.True(firstResult.IsSaved);
        await firstDb.SaveChangesAsync(Ct);

        int pid = await BackendPidAsync(secondScope);
        Task<SaveInternalNoteResult> waiting = StartNote(secondScope, new SaveInternalNoteRequest(order.Id, AdminCaller, "İkinci yöneticinin notu", string.Empty));
        await WaitUntilNoteBlockedAsync(pid);
        Assert.False(waiting.IsCompleted);
        await first.CommitAsync(Ct);

        // 23505 runs the second again: now the note exists and the shown text is still empty.
        SaveInternalNoteResult second = await waiting.WaitAsync(TimeSpan.FromSeconds(30), Ct);
        Assert.Equal(ErrorCode.DisplayedValueConflict, second.Code);
        Assert.Equal("Birinci yöneticinin notu", second.CurrentText);
        Assert.Equal("Birinci yöneticinin notu", await NoteTextAsync(order.Id));
        Assert.Equal(1, await CountAsync(c => c.AuditLogs.Where(a => a.TargetRecordId == order.Id && a.Operation == nameof(AuditOperation.SaveInternalNote))));
    }

    [Fact]
    public async Task AStaleShownText_IsTheConflict_TheCurrentNoteComesBack_AndNothingIsOverwritten_10_9e()
    {
        Order order = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical);
        Assert.True((await NoteAsync(order.Id, "İlk not", null)).IsSaved);

        // Two administrators opened the form on "İlk not"; the first saves, the second's form is now stale for a change, an emptying and a resend alike.
        SaveInternalNoteResult first = await NoteAsync(order.Id, "Birinci düzeltti", "İlk not");
        SaveInternalNoteResult changed = await NoteAsync(order.Id, "İkinci düzeltti", "İlk not");
        SaveInternalNoteResult emptied = await NoteAsync(order.Id, string.Empty, "İlk not");
        SaveInternalNoteResult resent = await NoteAsync(order.Id, "İlk not", "İlk not");
        SaveInternalNoteResult shownEmpty = await NoteAsync(order.Id, "Boş sandım", null);

        Assert.True(first.IsSaved);
        Assert.All(new[] { changed, emptied, resent, shownEmpty }, r => Assert.Equal(ErrorCode.DisplayedValueConflict, r.Code));
        Assert.All(new[] { changed, emptied, resent, shownEmpty }, r => Assert.Equal("Birinci düzeltti", r.CurrentText));
        Assert.Equal("Birinci düzeltti", await NoteTextAsync(order.Id));
        Assert.Equal(2, await CountAsync(c => c.AuditLogs.Where(a => a.TargetRecordId == order.Id)));

        // A browser sends the shown text's line breaks as CR LF: the note is stored with LF and the shown text is compared in that form (ShownValue).
        Assert.True((await NoteAsync(order.Id, "Satır bir\r\nSatır iki", "Birinci düzeltti")).IsSaved);
        Assert.Equal("Satır bir\nSatır iki", await NoteTextAsync(order.Id));
        Assert.True((await NoteAsync(order.Id, "Son not", "Satır bir\r\nSatır iki")).IsSaved);

        // The note's deletion by a stale form is the conflict; the current form's deletion goes through. The order's version never moved (09 10.9 (g)).
        Assert.Equal("Son not", (await NoteAsync(order.Id, string.Empty, "Satır bir\nSatır iki")).CurrentText);
        Assert.True((await NoteAsync(order.Id, string.Empty, "Son not")).IsSaved);
        Assert.Equal(0, await CountAsync(c => c.InternalNotes.Where(n => n.OrderId == order.Id)));
        Assert.Equal(order.Version, (await ReloadAsync(order.Id)).Version);
    }

    // ---- helpers ----

    /// <summary>The note's write as its caller runs it: in the runner, again on the note's unique index (09 10.9 item 3).</summary>
    private async Task<SaveInternalNoteResult> NoteAsync(Guid orderId, string text, string? shown)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await StartNote(scope, new SaveInternalNoteRequest(orderId, AdminCaller, text, shown));
    }

    private static Task<SaveInternalNoteResult> StartNote(AsyncServiceScope scope, SaveInternalNoteRequest request)
    {
        IOrderCorrections unit = scope.ServiceProvider.GetRequiredService<IOrderCorrections>();
        return scope.ServiceProvider.GetRequiredService<ITransactionRunner>()
            .RunAsync(token => unit.SaveInternalNoteAsync(request, token), SaveInternalNoteRequest.RetryOnUniqueIndexes, Ct);
    }

    private static async Task EmptyUnderTheRowLockAsync(AppDbContext db, OrderLocks locks, Guid orderId)
    {
        await using IDbContextTransaction transaction = await db.Database.BeginTransactionAsync(Ct);
        await locks.LockAsync([orderId], Ct);
        await db.Database.ExecuteSqlRawAsync(DisposalEmptying(orderId), Ct);
        await transaction.CommitAsync(Ct);
    }

    /// <summary>Opens the scope's connection and keeps it open, so the transaction that follows runs on the backend whose wait is watched.</summary>
    private static async Task<int> BackendPidAsync(AsyncServiceScope scope)
    {
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.OpenConnectionAsync(Ct);
        return (await db.Database.SqlQuery<int>($"SELECT pg_backend_pid() AS \"Value\"").ToListAsync(Ct)).Single();
    }

    /// <summary>Waits until that one backend waits on a lock (pg_stat_activity by its pid), at most about ten seconds.</summary>
    private async Task WaitUntilNoteBlockedAsync(int pid)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        for (int attempt = 0; attempt < 400; attempt++)
        {
            await using NpgsqlCommand command = new("SELECT wait_event_type FROM pg_stat_activity WHERE pid = @pid", connection);
            command.Parameters.AddWithValue("pid", pid);
            if (await command.ExecuteScalarAsync(Ct) is string type && type == "Lock")
            {
                return;
            }

            await Task.Delay(25, Ct);
        }

        throw new Xunit.Sdk.XunitException($"Backend {pid} never waited on a lock.");
    }

    private async Task<string?> NoteTextAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.InternalNotes.AsNoTracking().Where(n => n.OrderId == orderId).Select(n => (string?)n.Text).SingleOrDefaultAsync(Ct);
    }
}
