using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.UnitTests.Samples;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The enum value conversion and the audit operation reader without a database (T102 criteria 1, 2, 4; KK2, KK4): an enum is stored
/// as its code name, an undefined value or an unknown text fails, and the audit operation reader shows an unrecognized code as it is.
/// </summary>
public sealed class EnumConversionTests
{
    public static TheoryData<string> StoredEnumNames()
    {
        TheoryData<string> data = [];
        foreach (Type type in ApplicationEnums.Persistent())
        {
            data.Add(type.FullName!);
        }

        return data;
    }

    [Theory]
    [MemberData(nameof(StoredEnumNames))]
    public void Converter_ForEveryMember_WritesTheCodeNameAndReadsItBack(string enumFullName)
    {
        Type type = ApplicationEnums.Persistent().Single(t => t.FullName == enumFullName);
        ValueConverter converter = ConverterOf(type);

        foreach (object member in Enum.GetValues(type))
        {
            object? text = converter.ConvertToProvider(member);

            Assert.Equal(member.ToString(), text);
            Assert.Equal(member, converter.ConvertFromProvider(text));
        }
    }

    [Fact]
    public void Converter_ForAnUndefinedValue_RefusesToWrite()
    {
        ValueConverter converter = ConverterOf(typeof(PublishStatus));

        Assert.Throws<ArgumentOutOfRangeException>(() => converter.ConvertToProvider((PublishStatus)99));
    }

    [Theory]
    [InlineData("Bogus")]
    [InlineData("published")]
    [InlineData("1")]
    [InlineData("")]
    [InlineData("Draft,Published")]
    public void Converter_ForATextThatIsNotACodeName_FailsOnRead(string text)
    {
        ValueConverter converter = ConverterOf(typeof(PublishStatus));

        Assert.Throws<InvalidOperationException>(() => converter.ConvertFromProvider(text));
    }

    [Fact]
    public void AuditOperationText_ForAnUnrecognizedCode_ShowsTheCodeItselfAndDoesNotFail()
    {
        Assert.False(AuditOperationText.TryParse("DoSomethingNew", out _));
        Assert.Equal("DoSomethingNew", AuditOperationText.Display("DoSomethingNew"));
        Assert.True(AuditOperationText.TryParse("UpdateSettings", out AuditOperation known));
        Assert.Equal(AuditOperation.UpdateSettings, known);
        Assert.Equal("UpdateSettings", AuditOperationText.Display("UpdateSettings"));
    }

    [Fact]
    public void AuditOperationText_ToCode_WritesTheMemberNameAndRefusesAnUndefinedValue()
    {
        Assert.Equal("ExportOrders", AuditOperationText.ToCode(AuditOperation.ExportOrders));
        Assert.Throws<ArgumentOutOfRangeException>(() => AuditOperationText.ToCode((AuditOperation)999));
    }

    [Fact]
    public void AuditOperation_HasFortyThreeCodes_AndEveryCodeHasAnArea()
    {
        AuditOperation[] all = Enum.GetValues<AuditOperation>();

        Assert.Equal(43, all.Length);
        Assert.All(all, operation => Assert.True(Enum.IsDefined(AuditOperationText.AreaOf(operation))));
        Assert.Equal(29, all.Count(o => AuditOperationText.AreaOf(o) == AuditArea.Order));
        Assert.Equal(3, all.Count(o => AuditOperationText.AreaOf(o) == AuditArea.Export));
        Assert.Equal(Enum.GetValues<AuditArea>().Order(), all.Select(AuditOperationText.AreaOf).Distinct().Order());
    }

    [Fact]
    public void AuditOperationTypeName_InTheModelRule_IsTheNameOfTheType()
    {
        Assert.Equal(ModelRulesConvention.AuditOperationTypeName, typeof(AuditOperation).FullName);
    }

    [Fact]
    public void Model_OfAnEnumProperty_StoresTextAndCarriesTheCheckOfTheAllowedNames()
    {
        using AppDbContext context = SampleModel.CreateGoodContext();
        IEntityType record = context.GetService<IDesignTimeModel>().Model.GetEntityTypes().Single(e => e.GetTableName() == "sample_enum_records");

        IProperty status = record.FindProperty("Status")!;
        IProperty method = record.FindProperty("Method")!;
        Dictionary<string, string> checks = record.GetCheckConstraints().ToDictionary(c => c.Name!, c => c.Sql!, StringComparer.Ordinal);

        Assert.Equal(typeof(string), status.GetTypeMapping().Converter!.ProviderClrType);
        Assert.Equal(typeof(string), method.GetTypeMapping().Converter!.ProviderClrType);
        Assert.True(method.IsNullable);
        Assert.Equal("\"status\" IN ('Draft', 'Published', 'Archived')", checks["ck_sample_enum_records_status"]);
        Assert.Equal("\"subset_status\" IN ('Draft', 'Published')", checks["ck_sample_enum_records_subset_status"]);
        Assert.Equal("\"method\" IN ('Card', 'BankTransfer')", checks["ck_sample_enum_records_method"]);
    }

    [Fact]
    public void Model_OfAnEnumThatTheContextDoesNotConvert_IsRefused()
    {
        using AppDbContext context = SampleModel.CreateUnconvertedEnumContext();

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(() => context.GetService<IDesignTimeModel>().Model);

        Assert.Contains("not stored as text", error.ToString(), StringComparison.Ordinal);
    }

    private static ValueConverter ConverterOf(Type enumType)
    {
        return (ValueConverter)Activator.CreateInstance(typeof(EnumTextConverter<>).MakeGenericType(enumType))!;
    }
}
