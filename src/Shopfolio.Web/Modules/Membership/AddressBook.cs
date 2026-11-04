using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Modules.Membership;

/// <summary>
/// An entry of a member's address book (06 4.45): a named address the member saved. Orders freeze their addresses in their own columns and
/// carry no link to an entry. The entry goes with its account (06 2.2.5); the province link is restrictive. The book carries no version
/// (06 2.6.5).
/// </summary>
internal sealed class AddressBook
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid CustomerId { get; set; }

    /// <summary>The name of the address ("Home", "Work").</summary>
    public string Name { get; set; } = string.Empty;

    public string RecipientName { get; set; } = string.Empty;

    public Guid ProvinceId { get; set; }

    public string District { get; set; } = string.Empty;

    public string AddressLine { get; set; } = string.Empty;

    /// <summary>Ten digits, without the leading zero or "+90" (06 2.5.1); optional in the book.</summary>
    public string? Phone { get; set; }

    public static AddressBook Create(
        Guid customerId,
        string name,
        string recipientName,
        Guid provinceId,
        string district,
        string addressLine,
        string? phone,
        DateTimeOffset now)
    {
        return new AddressBook
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            CustomerId = customerId,
            Name = name,
            RecipientName = recipientName,
            ProvinceId = provinceId,
            District = district,
            AddressLine = addressLine,
            Phone = phone,
        };
    }
}

internal sealed class AddressBookConfiguration : IEntityTypeConfiguration<AddressBook>
{
    // The province is another module's entity (09 6 item 2): the link is a database foreign key, so the principal is named, not referenced.
    private const string SettingsProvinceType = "Shopfolio.Modules.Settings.Province";

    public void Configure(EntityTypeBuilder<AddressBook> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Name).HasColumnType("text");
        builder.Property(a => a.RecipientName).HasColumnType("text");
        builder.Property(a => a.District).HasColumnType("text");
        builder.Property(a => a.AddressLine).HasColumnType("text");
        builder.Property(a => a.Phone).HasMaxLength(10);

        // The account is deleted: the entries go with it. The province link is restrictive (06 7.2; 2.2.5).
        builder.HasOne<Customer>().WithMany().HasForeignKey(a => a.CustomerId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(SettingsProvinceType, navigationName: null).WithMany().HasForeignKey(nameof(AddressBook.ProvinceId)).OnDelete(DeleteBehavior.NoAction);

        builder.HasRule("phone_ten_digits", "phone ~ '^[0-9]{10}$'");
    }
}
