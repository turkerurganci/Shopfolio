using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.CorporateContent.Contracts;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>
/// A contact request (06 4.57): the record of every application sent from the contact form. It links to no account and no order. The moment
/// the request reached the firm is the row's creation moment. It carries no version. It is deleted by time, not by hand: the retention
/// starts at the last closing moment, or at the creation moment when it was never closed.
/// </summary>
internal sealed class ContactRequest
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    /// <summary>Ten digits, without the leading zero or "+90" (06 2.5.1).</summary>
    public string? Phone { get; set; }

    public ContactTopic ContactTopic { get; set; }

    public string Message { get; set; } = string.Empty;

    public ContactRequestStatus ContactRequestStatus { get; set; } = ContactRequestStatus.Open;

    /// <summary>The moment of the last closing; every closing writes it again and reopening does not empty it (06 4.57).</summary>
    public DateTimeOffset? LastClosedAt { get; set; }

    public static ContactRequest Create(string name, string email, ContactTopic contactTopic, string message, DateTimeOffset now)
    {
        return new ContactRequest { Id = EntityIds.New(), CreatedAt = now, Name = name, Email = email, ContactTopic = contactTopic, Message = message };
    }
}

internal sealed class ContactRequestConfiguration : IEntityTypeConfiguration<ContactRequest>
{
    public void Configure(EntityTypeBuilder<ContactRequest> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasColumnType("text");
        builder.Property(c => c.Email).HasColumnType("text");
        builder.Property(c => c.Phone).HasMaxLength(10);
        builder.Property(c => c.Message).HasColumnType("text");
        builder.Property(c => c.ContactRequestStatus).HasDefaultValue(ContactRequestStatus.Open);

        builder.HasIndex(c => new { c.ContactRequestStatus, c.CreatedAt });

        // The disposal's scan index (contact topic, COALESCE(last_closed_at, created_at)) is an expression index; EF cannot model it, so
        // the migration writes it.

        builder.HasRule("phone_ten_digits", "phone ~ '^[0-9]{10}$'");
        builder.HasRule("closed_has_last_closed_at", "contact_request_status <> 'Closed' OR last_closed_at IS NOT NULL");
    }
}
