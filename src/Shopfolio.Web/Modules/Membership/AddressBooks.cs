using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Membership;

/// <summary>
/// The member's address book and the address part of an order (T344; 02 3.14.1-3.14.6; 07 UC-70..UC-73, UC-27). Every write joins the caller's transaction and none of them saves
/// (09 6 item 3, item 7): the entry is added or changed on the context and the transaction's owner saves. The rules are <see cref="AddressFieldRules"/>'s; the province list is the
/// Settings module's. An order's addresses are copied out as values (<see cref="ShippingAddressSnapshot"/>, <see cref="BillingAddressSnapshot"/>) - the order keeps no link to a book
/// entry (06 4.45), so a later edit or deletion of the entry cannot reach it.
/// </summary>
internal sealed class AddressBooks
{
    private readonly AppDbContext _db;
    private readonly ISettingsModule _settings;
    private readonly TimeProvider _time;

    public AddressBooks(AppDbContext db, ISettingsModule settings, TimeProvider time)
    {
        _db = db;
        _settings = settings;
        _time = time;
    }

    /// <summary>The member's entries, oldest first (the id breaks a tie); the book is the member's own and has no page.</summary>
    public async Task<IReadOnlyList<AddressBookEntry>> ListAsync(Guid customerId, CancellationToken ct)
    {
        List<AddressBook> rows = await _db.AddressBooks.AsNoTracking()
            .Where(a => a.CustomerId == customerId)
            .OrderBy(a => a.CreatedAt).ThenBy(a => a.Id)
            .ToListAsync(ct);
        Dictionary<Guid, string> names = await ProvinceNamesAsync(ct);
        return rows.Select(row => EntryOf(row, names)).ToList();
    }

    /// <summary>One entry of the member's, or nothing when it is not theirs or is gone (the answer is the same: 07 2.3.3).</summary>
    public async Task<AddressBookEntry?> GetAsync(Guid customerId, Guid entryId, CancellationToken ct)
    {
        AddressBook? row = await _db.AddressBooks.AsNoTracking().SingleOrDefaultAsync(a => a.Id == entryId && a.CustomerId == customerId, ct);
        return row is null ? null : EntryOf(row, await ProvinceNamesAsync(ct));
    }

    public async Task<AddressBookSaveResult> CreateAsync(Guid customerId, AddressBookInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        RequireTransaction("The address book entry is written only inside a transaction (09 6 item 3).");

        if (!await _db.Customers.AsNoTracking().AnyAsync(c => c.Id == customerId, ct))
        {
            return AddressBookSaveResult.NotFound();
        }

        BookCheck check = await CheckBookInputAsync(input, ct);
        if (check.Values is not { } values)
        {
            return AddressBookSaveResult.FieldsRefused(check.Errors);
        }

        AddressBook entry = AddressBook.Create(
            customerId, values.Name, values.Address.RecipientName, values.Address.ProvinceId, values.Address.District, values.Address.AddressLine, values.Phone, _time.GetUtcNow());
        _db.AddressBooks.Add(entry);
        return AddressBookSaveResult.Saved(entry.Id);
    }

    public async Task<AddressBookSaveResult> UpdateAsync(Guid customerId, Guid entryId, AddressBookInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        RequireTransaction("The address book entry is written only inside a transaction (09 6 item 3).");

        // Ownership first: an entry that is not the member's and one that is gone are the same answer, whatever the fields are (07 2.3.3).
        AddressBook? entry = await _db.AddressBooks.SingleOrDefaultAsync(a => a.Id == entryId && a.CustomerId == customerId, ct);
        if (entry is null)
        {
            return AddressBookSaveResult.NotFound();
        }

        BookCheck check = await CheckBookInputAsync(input, ct);
        if (check.Values is not { } values)
        {
            return AddressBookSaveResult.FieldsRefused(check.Errors);
        }

        entry.Rewrite(values.Name, values.Address.RecipientName, values.Address.ProvinceId, values.Address.District, values.Address.AddressLine, values.Phone);
        return AddressBookSaveResult.Saved(entry.Id);
    }

    /// <summary>The member deletes an entry (07 UC-73): deleted when it was theirs, not found when it is not theirs or was gone. No order is touched (02 3.14.5).</summary>
    public async Task<AddressBookDeleteResult> DeleteAsync(Guid customerId, Guid entryId, CancellationToken ct)
    {
        RequireTransaction("The address book entry is deleted only inside a transaction (09 6 item 3).");
        int deleted = await _db.AddressBooks.Where(a => a.Id == entryId && a.CustomerId == customerId).ExecuteDeleteAsync(ct);
        return deleted == 1 ? AddressBookDeleteResult.Deleted() : AddressBookDeleteResult.NotFound();
    }

    /// <summary>
    /// The address part of an order (02 3.14.1-3.14.6; 07 UC-27): checks the addresses the basket asks for, with every refused field together, and gives the values the order freezes.
    /// A member's shipping address is a book entry or four typed fields; a typed one is written to the book (in the caller's transaction) only when the member chose to keep it. A
    /// guest has no book. The billing address is the shipping one when the box says so and the basket has a physical item; otherwise it is its own four fields.
    /// </summary>
    public async Task<OrderAddressResolution> ResolveAsync(OrderAddressRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        RequireTransaction("The order's addresses are resolved only inside a transaction (09 6 item 3).");

        AddressRequirement need = AddressFieldRules.RequirementOf(request.ItemTypes);
        IReadOnlySet<Guid> provinceIds = (await _settings.GetProvincesAsync(ct)).Select(p => p.Id).ToHashSet();

        // A guest, or a member whose account is gone, has no book.
        Guid? member = null;
        if (request.CustomerId is { } signedIn && await _db.Customers.AsNoTracking().AnyAsync(c => c.Id == signedIn, ct))
        {
            member = signedIn;
        }

        List<MembershipFieldError> errors = [];
        AddressValues? shippingFour = null;
        ShippingAddressSnapshot? shipping = null;
        bool typedShipping = false;
        if (need.AsksShipping)
        {
            string? phoneText = AddressFieldRules.Text(request.ShippingPhone);
            if (request.AddressBookId is { } entryId)
            {
                if (member is not { } owner)
                {
                    return OrderAddressResolution.BookEntryNotFound();
                }

                AddressBook? entry = await _db.AddressBooks.AsNoTracking().SingleOrDefaultAsync(a => a.Id == entryId && a.CustomerId == owner, ct);
                if (entry is null)
                {
                    return OrderAddressResolution.BookEntryNotFound();
                }

                shippingFour = new AddressValues(entry.RecipientName, entry.ProvinceId, entry.District, entry.AddressLine);

                // The entry's own phone is optional in the book; the payment step asks for it when the entry has none (02 3.14.6). A phone typed on the step wins.
                phoneText ??= entry.Phone;
            }
            else
            {
                typedShipping = true;
                shippingFour = AddressFieldRules.CheckAddress(
                    AddressFieldRules.ShippingPrefix,
                    request.ShippingRecipientName,
                    request.ShippingProvinceId,
                    request.ShippingDistrict,
                    request.ShippingAddressLine,
                    provinceIds,
                    errors);
            }

            bool phoneOk = AddressFieldRules.TryStorePhone(nameof(OrderAddressRequest.ShippingPhone), phoneText, true, errors, out string? phone);
            if (shippingFour is not null && phoneOk)
            {
                shipping = new ShippingAddressSnapshot(shippingFour.RecipientName, phone!, shippingFour.ProvinceId, shippingFour.District, shippingFour.AddressLine);
            }
        }

        AddressValues? billingFour;
        if (need.OffersBillingSameAsShipping && request.IsBillingSameAsShipping)
        {
            billingFour = shippingFour;
        }
        else
        {
            billingFour = AddressFieldRules.CheckAddress(
                AddressFieldRules.BillingPrefix,
                request.BillingRecipientName,
                request.BillingProvinceId,
                request.BillingDistrict,
                request.BillingAddressLine,
                provinceIds,
                errors);
        }

        if (errors.Count > 0 || billingFour is null || (need.AsksShipping && shipping is null))
        {
            return OrderAddressResolution.FieldsRefused(errors);
        }

        Guid? savedEntryId = null;
        if (need.AsksShipping && typedShipping && request.SaveToAddressBook && member is { } keeper)
        {
            // Never on its own (K-680): only the member's choice writes the typed address to the book, under the name given, or the recipient's name when none is given.
            string name = AddressFieldRules.Text(request.SavedAddressName) ?? shipping!.RecipientName;
            AddressBook saved = AddressBook.Create(
                keeper, name, shipping!.RecipientName, shipping.ProvinceId, shipping.District, shipping.AddressLine, shipping.Phone, _time.GetUtcNow());
            _db.AddressBooks.Add(saved);
            savedEntryId = saved.Id;
        }

        return OrderAddressResolution.Resolved(
            shipping, new BillingAddressSnapshot(billingFour.RecipientName, billingFour.ProvinceId, billingFour.District, billingFour.AddressLine), savedEntryId);
    }

    private sealed record BookValues(string Name, AddressValues Address, string? Phone);

    private sealed record BookCheck(BookValues? Values, IReadOnlyList<MembershipFieldError> Errors);

    private async Task<BookCheck> CheckBookInputAsync(AddressBookInput input, CancellationToken ct)
    {
        IReadOnlySet<Guid> provinceIds = (await _settings.GetProvincesAsync(ct)).Select(p => p.Id).ToHashSet();
        List<MembershipFieldError> errors = [];
        string? name = AddressFieldRules.Text(input.Name);
        if (name is null)
        {
            errors.Add(new MembershipFieldError(nameof(AddressBookInput.Name), ErrorCode.FieldRequired));
        }

        AddressValues? address = AddressFieldRules.CheckAddress(
            AddressFieldRules.BookPrefix, input.RecipientName, input.ProvinceId, input.District, input.AddressLine, provinceIds, errors);
        AddressFieldRules.TryStorePhone(nameof(AddressBookInput.Phone), input.Phone, false, errors, out string? phone);
        return errors.Count > 0 || name is null || address is null
            ? new BookCheck(null, errors)
            : new BookCheck(new BookValues(name, address, phone), errors);
    }

    private async Task<Dictionary<Guid, string>> ProvinceNamesAsync(CancellationToken ct)
    {
        return (await _settings.GetProvincesAsync(ct)).ToDictionary(p => p.Id, p => p.Name);
    }

    private static AddressBookEntry EntryOf(AddressBook row, IReadOnlyDictionary<Guid, string> provinceNames)
    {
        return new AddressBookEntry(
            row.Id, row.Name, row.RecipientName, row.ProvinceId, provinceNames.GetValueOrDefault(row.ProvinceId, string.Empty), row.District, row.AddressLine, row.Phone, row.CreatedAt);
    }

    private void RequireTransaction(string message)
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(message);
        }
    }
}
