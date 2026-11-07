using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

using Npgsql;

using Shopfolio.Infrastructure.Backup;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.StartupDatabase;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Settings;

namespace Shopfolio.Infrastructure;

/// <summary>
/// The single database context (09 4.1 item 2, 5.2). The entities arrive with the tasks that own them, each with its <c>DbSet</c> property
/// (09 5.2 item 2) and its entity configuration in the module's folder; the context applies those configurations and does no other work
/// except answering the start-up data check which records exist (<see cref="ReadStartupRecordPresenceAsync"/>).
/// A module never opens a context of its own. The context is also the data protection package's key context (06 4.68): the package's own
/// entity is its key ring table.
/// </summary>
internal sealed class AppDbContext : DbContext, IDataProtectionKeyContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Province> Provinces => Set<Province>();

    public DbSet<Country> Countries => Set<Country>();

    public DbSet<Company> Companies => Set<Company>();

    public DbSet<CompanySettings> CompanySettings => Set<CompanySettings>();

    public DbSet<CompanyDeliveryProvince> CompanyDeliveryProvinces => Set<CompanyDeliveryProvince>();

    public DbSet<LegalTextVersion> LegalTextVersions => Set<LegalTextVersion>();

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Variant> Variants => Set<Variant>();

    public DbSet<Option> Options => Set<Option>();

    public DbSet<OptionValue> OptionValues => Set<OptionValue>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();

    public DbSet<Image> Images => Set<Image>();

    public DbSet<ProductImage> ProductImages => Set<ProductImage>();

    public DbSet<PriceHistory> PriceHistories => Set<PriceHistory>();

    public DbSet<Discount> Discounts => Set<Discount>();

    public DbSet<Coupon> Coupons => Set<Coupon>();

    public DbSet<DigitalFile> DigitalFiles => Set<DigitalFile>();

    public DbSet<Admin> Admins => Set<Admin>();

    public DbSet<AdminInvitation> AdminInvitations => Set<AdminInvitation>();

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<PendingEmailChange> PendingEmailChanges => Set<PendingEmailChange>();

    public DbSet<AddressBook> AddressBooks => Set<AddressBook>();

    public DbSet<Session> Sessions => Set<Session>();

    public DbSet<RecognizedBrowserMark> RecognizedBrowserMarks => Set<RecognizedBrowserMark>();

    public DbSet<LinkRecord> LinkRecords => Set<LinkRecord>();

    public DbSet<AttemptRecord> AttemptRecords => Set<AttemptRecord>();

    public DbSet<LoginRecord> LoginRecords => Set<LoginRecord>();

    public DbSet<Cart> Carts => Set<Cart>();

    public DbSet<CartItem> CartItems => Set<CartItem>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<TransitionRecord> TransitionRecords => Set<TransitionRecord>();

    public DbSet<InternalNote> InternalNotes => Set<InternalNote>();

    public DbSet<CheckboxAcceptance> CheckboxAcceptances => Set<CheckboxAcceptance>();

    public DbSet<CheckboxAcceptanceOrderItem> CheckboxAcceptanceOrderItems => Set<CheckboxAcceptanceOrderItem>();

    public DbSet<StockReservation> StockReservations => Set<StockReservation>();

    public DbSet<OrderEmailHistory> OrderEmailHistories => Set<OrderEmailHistory>();

    public DbSet<OrderAddressHistory> OrderAddressHistories => Set<OrderAddressHistory>();

    public DbSet<RefundIban> RefundIbans => Set<RefundIban>();

    public DbSet<DefectClaim> DefectClaims => Set<DefectClaim>();

    public DbSet<Cancellation> Cancellations => Set<Cancellation>();

    public DbSet<ItemRemoval> ItemRemovals => Set<ItemRemoval>();

    public DbSet<LateDeliveryTermination> LateDeliveryTerminations => Set<LateDeliveryTermination>();

    public DbSet<Withdrawal> Withdrawals => Set<Withdrawal>();

    public DbSet<Return> Returns => Set<Return>();

    public DbSet<NotReturnedClosure> NotReturnedClosures => Set<NotReturnedClosure>();

    public DbSet<PaymentRecord> PaymentRecords => Set<PaymentRecord>();

    public DbSet<Refund> Refunds => Set<Refund>();

    public DbSet<RefundRecord> RefundRecords => Set<RefundRecord>();

    public DbSet<InboundNotificationRecord> InboundNotificationRecords => Set<InboundNotificationRecord>();

    public DbSet<ReturnRejection> ReturnRejections => Set<ReturnRejection>();

    public DbSet<RefundIbanRequest> RefundIbanRequests => Set<RefundIbanRequest>();

    public DbSet<AboutPage> AboutPages => Set<AboutPage>();

    public DbSet<Offering> Offerings => Set<Offering>();

    public DbSet<PortfolioItem> PortfolioItems => Set<PortfolioItem>();

    public DbSet<FaqItem> FaqItems => Set<FaqItem>();

    public DbSet<CustomPage> CustomPages => Set<CustomPage>();

    public DbSet<Announcement> Announcements => Set<Announcement>();

    public DbSet<SocialLink> SocialLinks => Set<SocialLink>();

    public DbSet<Branding> Brandings => Set<Branding>();

    public DbSet<Branch> Branches => Set<Branch>();

    public DbSet<OfferingProduct> OfferingProducts => Set<OfferingProduct>();

    public DbSet<PortfolioItemProduct> PortfolioItemProducts => Set<PortfolioItemProduct>();

    public DbSet<ContactRequest> ContactRequests => Set<ContactRequest>();

    public DbSet<AboutPageImage> AboutPageImages => Set<AboutPageImage>();

    public DbSet<OfferingImage> OfferingImages => Set<OfferingImage>();

    public DbSet<PortfolioItemImage> PortfolioItemImages => Set<PortfolioItemImage>();

    public DbSet<FaqItemImage> FaqItemImages => Set<FaqItemImage>();

    public DbSet<CustomPageImage> CustomPageImages => Set<CustomPageImage>();

    public DbSet<DispatchRecord> DispatchRecords => Set<DispatchRecord>();

    public DbSet<ChannelStateRecord> ChannelStateRecords => Set<ChannelStateRecord>();

    public DbSet<Job> Jobs => Set<Job>();

    public DbSet<DisposalRecord> DisposalRecords => Set<DisposalRecord>();

    public DbSet<HeartbeatRecord> HeartbeatRecords => Set<HeartbeatRecord>();

    public DbSet<OutageRecord> OutageRecords => Set<OutageRecord>();

    public DbSet<BackupStatusRecord> BackupStatusRecords => Set<BackupStatusRecord>();

    /// <summary>
    /// The key ring (06 4.68): the data protection package's entity, the one exception to the key and creation time rules. The application's
    /// code neither writes nor reads it; the framework does (09 5.2 item 4).
    /// </summary>
    public DbSet<DataProtectionKey> DataProtectionKeys => Set<DataProtectionKey>();

    /// <summary>
    /// Whether the records the start-up data check looks for exist (T205; 06 8.2.1): the province list and the country list are not empty and the
    /// six single-row records of the start data exist. It lives on the context because the check is in <c>Infrastructure/</c>, which reaches no
    /// module type (09 6 item 2). Only existence is asked, never a value (06 8.2.1). A table the half-done migration did not make counts as
    /// absent; any other database failure goes up.
    /// </summary>
    public async Task<IReadOnlyList<StartupRecordPresence>> ReadStartupRecordPresenceAsync(CancellationToken ct)
    {
        return
        [
            new(nameof(Province), true, await ExistsAsync(Provinces, ct)),
            new(nameof(Country), true, await ExistsAsync(Countries, ct)),
            new(nameof(Company), false, await ExistsAsync(Companies, ct)),
            new(nameof(CompanySettings), false, await ExistsAsync(this.CompanySettings, ct)),
            new(nameof(Branding), false, await ExistsAsync(Brandings, ct)),
            new(nameof(ChannelStateRecord), false, await ExistsAsync(ChannelStateRecords, ct)),
            new(nameof(HeartbeatRecord), false, await ExistsAsync(HeartbeatRecords, ct)),
            new(nameof(BackupStatusRecord), false, await ExistsAsync(BackupStatusRecords, ct)),
        ];
    }

    private static async Task<bool> ExistsAsync<T>(DbSet<T> set, CancellationToken ct)
        where T : class
    {
        try
        {
            return await set.AsNoTracking().AnyAsync(ct);
        }
        catch (PostgresException exception) when (exception.SqlState == PostgresErrorCodes.UndefinedTable)
        {
            return false;
        }
    }

    /// <summary>
    /// The one place where the provider, the naming convention and the save interceptors are set up (09 5.2 item 1; 10.1 item 2).
    /// </summary>
    public static DbContextOptionsBuilder Configure(DbContextOptionsBuilder builder, string connectionString)
    {
        // No retry strategy (09 10.7 item 4): the provider's automatic retry (EnableRetryOnFailure) stays off, so a lock timeout, a cancelled
        // query or a connection cut for idling goes up as it is. The connection string carries the session times (DatabaseConnection).
        return builder
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(
                EmptyKeyGuard.Instance,
                EmptyTextGuard.Instance,
                ProtectedRecordsGuard.Instance,
                CatalogRecordsGuard.Instance,
                ProductVersionGuard.Instance,
                ProductDefinitionGuard.Instance,
                PriceLabelGuard.Instance,
                LoginRecordsGuard.Instance,
                OrderingRecordsGuard.Instance,
                OrderVersionGuard.Instance,
                AfterSalesRecordsGuard.Instance,
                PaymentRecordsGuard.Instance,
                CorporateContentRecordsGuard.Instance);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        ArgumentNullException.ThrowIfNull(configurationBuilder);
        configurationBuilder.Conventions.Remove<ForeignKeyIndexConvention>();
        configurationBuilder.Conventions.Add(_ => new ModelRulesConvention());

        // Every enum of the application is stored as its code name (06 2.9.1; 09 10.1 item 3). The conversion is made here once;
        // an enum property never writes its own. The audit operation is the one exception: the trail stores its code as plain text
        // (AuditLog.Operation) so that a code of a later version is read as it is (09 9.2 item 2).
        foreach (Type enumType in typeof(EntityIds).Assembly.GetTypes().Where(t => t.IsEnum && t.FullName != ModelRulesConvention.AuditOperationTypeName))
        {
            configurationBuilder.Properties(enumType).HaveConversion(typeof(EnumTextConverter<>).MakeGenericType(enumType));
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
