using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Shopfolio.Infrastructure.DataModel;

/// <summary>
/// The records one version value covers (06 2.6.1, 2.6.2, 2.6.4): a root record that carries the <c>Version</c> column and the records written
/// under it - named one by one, or every record linked to the root by a chain of foreign keys - less a closed list of records and fields whose
/// writing does not raise the version. A module's save guard declares its scope here and asks which tracked writes need the root's version
/// raised by one and whether it was. Types are named by their full name so that a module's guard can name another module's record without
/// reaching its type (09 6 item 2). The check reads only the change tracker; a bulk statement does not pass through a save.
/// </summary>
internal sealed class VersionScope
{
    private readonly ConditionalWeakTable<IModel, HashSet<IEntityType>> _members = [];

    public VersionScope(
        string rootType,
        IReadOnlyCollection<string>? memberTypes,
        IReadOnlyCollection<string> exemptTypes,
        IReadOnlyCollection<(string Type, string Field)> exemptFields,
        IReadOnlyCollection<(string Type, string Field)> emptiedFields)
    {
        RootType = rootType;
        MemberTypes = memberTypes;
        ExemptTypes = exemptTypes;
        ExemptFields = exemptFields;
        EmptiedFields = emptiedFields;
    }

    public string RootType { get; }

    /// <summary>The records under the root, by name; empty means every record linked to the root by a chain of foreign keys.</summary>
    public IReadOnlyCollection<string>? MemberTypes { get; }

    /// <summary>The records whose writing never raises the version; they are not members and do not carry the chain further.</summary>
    public IReadOnlyCollection<string> ExemptTypes { get; }

    /// <summary>Fields whose change alone does not raise the version.</summary>
    public IReadOnlyCollection<(string Type, string Field)> ExemptFields { get; }

    /// <summary>Fields whose emptying alone does not raise the version (a link the database empties when its target is deleted).</summary>
    public IReadOnlyCollection<(string Type, string Field)> EmptiedFields { get; }

    /// <summary>The member record types of the model; a named type that the model does not have is a configuration error.</summary>
    public IReadOnlySet<IEntityType> Members(IModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        return _members.GetValue(model, BuildMembers);
    }

    /// <summary>
    /// The tracked writes that need the root's version raised, each with the id of its root when the tracked records tell it: a changed root
    /// (other than its version and exempt fields) and every added, changed or deleted member. A new or deleted root needs no raise.
    /// </summary>
    public IReadOnlyList<(EntityEntry Entry, Guid? RootId)> Writes(DbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        IEntityType root = Root(context.Model);
        IReadOnlySet<IEntityType> members = Members(context.Model);
        List<EntityEntry> changed = [.. context.ChangeTracker.Entries().Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)];
        Dictionary<(IEntityType, Guid), EntityEntry> tracked = [];
        foreach (EntityEntry entry in context.ChangeTracker.Entries())
        {
            if (KeyOf(entry) is Guid id)
            {
                tracked[(entry.Metadata, id)] = entry;
            }
        }

        List<(EntityEntry, Guid?)> writes = [];
        foreach (EntityEntry entry in changed)
        {
            bool isRoot = entry.Metadata == root;
            if (!isRoot && !members.Contains(entry.Metadata))
            {
                continue;
            }

            if (isRoot && entry.State is EntityState.Added or EntityState.Deleted)
            {
                continue;
            }

            if (entry.State == EntityState.Modified && OnlyFreeFields(entry, isRoot))
            {
                continue;
            }

            writes.Add((entry, RootOf(entry, root, members, tracked, 0)));
        }

        return writes;
    }

    /// <summary>True when the root is new or deleted in this save, or its version was raised by exactly one.</summary>
    public bool IsRaised(DbContext context, Guid rootId)
    {
        ArgumentNullException.ThrowIfNull(context);
        EntityEntry? entry = Find(context, rootId);
        if (entry is null)
        {
            return false;
        }

        if (entry.State is EntityState.Added or EntityState.Deleted)
        {
            return true;
        }

        PropertyEntry version = entry.Property(ModelBuilderExtensions.VersionPropertyName);
        return version.IsModified && version.CurrentValue is int now && version.OriginalValue is int was && now == was + 1;
    }

    /// <summary>True when the root record with this id is tracked by the context.</summary>
    public bool IsTracked(DbContext context, Guid rootId)
    {
        return Find(context, rootId) is not null;
    }

    private static Guid? KeyOf(EntityEntry entry)
    {
        IKey? key = entry.Metadata.FindPrimaryKey();
        if (key is null || key.Properties.Count != 1 || key.Properties[0].ClrType != typeof(Guid))
        {
            return null;
        }

        return entry.Property(key.Properties[0].Name).CurrentValue as Guid?;
    }

    private static Guid? ValueOf(EntityEntry entry, IForeignKey foreignKey)
    {
        if (foreignKey.Properties.Count != 1)
        {
            return null;
        }

        PropertyEntry property = entry.Property(foreignKey.Properties[0].Name);
        return (entry.State == EntityState.Deleted ? property.OriginalValue : property.CurrentValue) as Guid?;
    }

    private EntityEntry? Find(DbContext context, Guid rootId)
    {
        IEntityType root = Root(context.Model);
        return context.ChangeTracker.Entries().FirstOrDefault(e => e.Metadata == root && KeyOf(e) == rootId);
    }

    private IEntityType Root(IModel model)
    {
        return model.FindEntityType(RootType) ?? throw new InvalidOperationException($"The version root '{RootType}' is not in the model.");
    }

    private bool OnlyFreeFields(EntityEntry entry, bool isRoot)
    {
        string type = entry.Metadata.ClrType.FullName!;
        foreach (PropertyEntry property in entry.Properties.Where(p => p.IsModified))
        {
            string name = property.Metadata.Name;
            bool free = (isRoot && name == ModelBuilderExtensions.VersionPropertyName)
                || ExemptFields.Contains((type, name))
                || (EmptiedFields.Contains((type, name)) && property.CurrentValue is null);
            if (!free)
            {
                return false;
            }
        }

        return true;
    }

    private static Guid? RootOf(EntityEntry entry, IEntityType root, IReadOnlySet<IEntityType> members, Dictionary<(IEntityType, Guid), EntityEntry> tracked, int depth)
    {
        if (entry.Metadata == root)
        {
            return KeyOf(entry);
        }

        if (depth > members.Count)
        {
            return null;
        }

        foreach (IForeignKey foreignKey in entry.Metadata.GetForeignKeys().Where(f => f.PrincipalEntityType == root))
        {
            if (ValueOf(entry, foreignKey) is Guid id)
            {
                return id;
            }
        }

        foreach (IForeignKey foreignKey in entry.Metadata.GetForeignKeys().Where(f => members.Contains(f.PrincipalEntityType)))
        {
            if (ValueOf(entry, foreignKey) is Guid id && tracked.TryGetValue((foreignKey.PrincipalEntityType, id), out EntityEntry? principal)
                && RootOf(principal, root, members, tracked, depth + 1) is Guid rootId)
            {
                return rootId;
            }
        }

        return null;
    }

    private HashSet<IEntityType> BuildMembers(IModel model)
    {
        IEntityType root = Root(model);
        HashSet<IEntityType> exempt = [.. ExemptTypes.Select(name => model.FindEntityType(name)).OfType<IEntityType>()];
        if (MemberTypes is not null)
        {
            return [.. MemberTypes.Select(name => model.FindEntityType(name) ?? throw new InvalidOperationException($"The version member '{name}' is not in the model."))];
        }

        HashSet<IEntityType> members = [];
        bool grew = true;
        while (grew)
        {
            grew = false;
            foreach (IEntityType type in model.GetEntityTypes())
            {
                if (type == root || exempt.Contains(type) || members.Contains(type))
                {
                    continue;
                }

                if (type.GetForeignKeys().Any(f => f.PrincipalEntityType == root || members.Contains(f.PrincipalEntityType)))
                {
                    members.Add(type);
                    grew = true;
                }
            }
        }

        return members;
    }
}
