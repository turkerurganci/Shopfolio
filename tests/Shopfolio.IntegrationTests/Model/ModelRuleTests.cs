using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

using Shopfolio.Infrastructure;
using Shopfolio.UnitTests.Consistency;
using Shopfolio.UnitTests.Samples;

namespace Shopfolio.IntegrationTests.Model;

/// <summary>
/// Model tests of the data model rules (T101; 09 5.2 item 5, 10.1 item 5): the finished model of the one context is read, no database is
/// opened. The application has no entity yet, so the rules run over the sample entities that follow them and, as negative tests, over the
/// sample that breaks them.
/// </summary>
public sealed class ModelRuleTests
{
    /// <summary>The relationships of the application's own entities that cascade; equal to the cascading line of 06 2.2.5. The task that adds an entity adds its lines.</summary>
    private static readonly string[] RegisteredCascades = [];

    private static bool IsApplicationEntity(IEntityType entityType) => entityType.ClrType.Assembly == typeof(AppDbContext).Assembly;

    [Fact]
    public void ApplicationModel_FollowsTheRules_AndItsCascadeSetEqualsTheRegisteredList()
    {
        using AppDbContext context = SampleModel.CreateGoodContext();
        IModel model = ModelRules.ModelOf(context);

        Assert.Empty(ModelRules.Check(model, IsApplicationEntity));
        Assert.Equal(RegisteredCascades, ModelRules.CascadeRelations(model, IsApplicationEntity));
    }

    [Fact]
    public void SampleModel_FollowsTheRules()
    {
        using AppDbContext context = SampleModel.CreateGoodContext();

        IReadOnlyList<string> findings = ModelRules.Check(ModelRules.ModelOf(context));

        Assert.Empty(findings);
    }

    [Fact]
    public void SampleModel_NamesAreDerivedByTheRule()
    {
        using AppDbContext context = SampleModel.CreateGoodContext();
        IEntityType owner = ModelRules.ModelOf(context).GetEntityTypes().Single(e => e.GetTableName() == "sample_owners");
        IEntityType item = ModelRules.ModelOf(context).GetEntityTypes().Single(e => e.GetTableName() == "sample_items");

        Assert.Equal("pk_sample_owners", owner.FindPrimaryKey()!.GetName());
        Assert.Equal("ix_sample_owners_normalized_code", Assert.Single(owner.GetIndexes()).GetDatabaseName());
        Assert.Equal("ck_sample_owners_price_not_negative", Assert.Single(owner.GetCheckConstraints()).Name);
        Assert.Equal("price >= 0", Assert.Single(owner.GetCheckConstraints()).Sql);
        Assert.All(item.GetForeignKeys(), f => Assert.StartsWith("fk_sample_items_", f.GetConstraintName(), StringComparison.Ordinal));
        Assert.DoesNotContain(owner.GetAnnotations(), a => a.Name.StartsWith("Shopfolio:Rule:", StringComparison.Ordinal));
    }

    [Fact]
    public void SampleModel_CascadeSetIsTheOneWrittenRelationship()
    {
        using AppDbContext context = SampleModel.CreateGoodContext();

        IReadOnlyList<string> cascades = ModelRules.CascadeRelations(ModelRules.ModelOf(context));

        Assert.Equal(["sample_items -> sample_owners (OwnerId)"], cascades);
    }

    [Fact]
    public void SampleModel_DeleteBehaviorsAreTheThreeAndAnUnwrittenRelationshipIsNoAction()
    {
        using AppDbContext context = SampleModel.CreateGoodContext();
        IEntityType item = ModelRules.ModelOf(context).GetEntityTypes().Single(e => e.GetTableName() == "sample_items");

        Dictionary<string, DeleteBehavior> behaviors = item.GetForeignKeys().ToDictionary(f => f.Properties.Single().Name, f => f.DeleteBehavior);

        Assert.Equal(DeleteBehavior.Cascade, behaviors["OwnerId"]);
        Assert.Equal(DeleteBehavior.SetNull, behaviors["EmptiedOwnerId"]);
        Assert.Equal(DeleteBehavior.NoAction, behaviors["RestrictiveOwnerId"]);
        Assert.Equal(DeleteBehavior.NoAction, behaviors["UnwrittenOwnerId"]);
    }

    [Fact]
    public void SampleModel_GuidKeysAreNeverGeneratedByEf_AndVersionIsAConcurrencyToken()
    {
        using AppDbContext context = SampleModel.CreateGoodContext();
        IModel model = ModelRules.ModelOf(context);

        Assert.All(model.GetEntityTypes(), e => Assert.Equal(ValueGenerated.Never, e.FindPrimaryKey()!.Properties.Single().ValueGenerated));
        IEntityType owner = model.GetEntityTypes().Single(e => e.GetTableName() == "sample_owners");
        Assert.True(owner.FindProperty("Version")!.IsConcurrencyToken);
        Assert.False(owner.FindProperty("Count")!.IsConcurrencyToken);
    }

    [Fact]
    public void SampleModel_ColumnTypesEqualTheTypeTable()
    {
        using AppDbContext context = SampleModel.CreateGoodContext();
        IEntityType owner = ModelRules.ModelOf(context).GetEntityTypes().Single(e => e.GetTableName() == "sample_owners");
        Dictionary<string, string> expected = new()
        {
            ["Id"] = ColumnTypeTable.StoreTypeOf("uuid"),
            ["CreatedAt"] = ColumnTypeTable.StoreTypeOf("timestamptz"),
            ["StartDate"] = ColumnTypeTable.StoreTypeOf("date"),
            ["Price"] = ColumnTypeTable.StoreTypeOf("numeric(12,2)"),
            ["VatRate"] = ColumnTypeTable.StoreTypeOf("numeric(5,2)"),
            ["NetQuantity"] = ColumnTypeTable.StoreTypeOf("numeric(12,3)"),
            ["Count"] = ColumnTypeTable.StoreTypeOf("integer"),
            ["Version"] = ColumnTypeTable.StoreTypeOf("integer"),
            ["ByteSize"] = ColumnTypeTable.StoreTypeOf("bigint"),
            ["IsActive"] = ColumnTypeTable.StoreTypeOf("boolean"),
            ["ClientAddress"] = ColumnTypeTable.StoreTypeOf("inet"),
            ["Payload"] = ColumnTypeTable.StoreTypeOf("jsonb"),
            ["LongText"] = ColumnTypeTable.StoreTypeOf("text"),
            ["Name"] = ColumnTypeTable.StoreTypeOf("text"),
            ["Code"] = ColumnTypeTable.StoreTypeOf("varchar(n)", 50),
        };

        Dictionary<string, string> actual = owner.GetProperties().ToDictionary(p => p.Name, p => p.GetColumnType());

        Assert.Equal(expected.OrderBy(p => p.Key, StringComparer.Ordinal), actual.Where(p => expected.ContainsKey(p.Key)).OrderBy(p => p.Key, StringComparer.Ordinal));
    }

    [Fact]
    public void ViolatingModel_IsReportedPlaceByPlace()
    {
        using AppDbContext context = SampleModel.CreateViolatingContext();

        IReadOnlyList<string> findings = ModelRules.Check(ModelRules.ModelOf(context));

        Assert.Contains(findings, f => f.Contains("table name 'Bad_Parents'", StringComparison.Ordinal));
        Assert.Contains(findings, f => f.StartsWith("Bad_Parents: there is no created_at column", StringComparison.Ordinal));
        Assert.Contains(findings, f => f.Contains("delete behavior of (RestrictParentId) is Restrict", StringComparison.Ordinal));
        Assert.Contains(findings, f => f.Contains("delete behavior of (ClientCascadeParentId) is ClientCascade", StringComparison.Ordinal));
    }

    [Fact]
    public void DecimalWithoutADeclaredType_RefusesToBuildTheModel()
    {
        using AppDbContext context = SampleModel.CreateUntypedDecimalContext();

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => ModelRules.ModelOf(context));

        Assert.Contains("UntypedPrice.Amount", error.ToString(), StringComparison.Ordinal);
    }
}
