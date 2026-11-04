using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Settings;

namespace Shopfolio.Infrastructure;

/// <summary>
/// The single database context (09 4.1 item 2, 5.2). The entities arrive with the tasks that own them, each with its <c>DbSet</c> property
/// (09 5.2 item 2) and its entity configuration in the module's folder; the context applies those configurations and does no other work.
/// A module never opens a context of its own.
/// </summary>
internal sealed class AppDbContext : DbContext
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

    /// <summary>
    /// The one place where the provider, the naming convention and the save interceptors are set up (09 5.2 item 1; 10.1 item 2).
    /// </summary>
    public static DbContextOptionsBuilder Configure(DbContextOptionsBuilder builder, string connectionString)
    {
        return builder
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(
                EmptyKeyGuard.Instance,
                EmptyTextGuard.Instance,
                ProtectedRecordsGuard.Instance,
                CatalogRecordsGuard.Instance,
                LoginRecordsGuard.Instance,
                OrderingRecordsGuard.Instance,
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
