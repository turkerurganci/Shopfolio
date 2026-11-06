using System.Reflection;

using Shopfolio.Infrastructure.Binding;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The model binding rules (T211 criteria 1, 2, 4, 6; KK1, KK4, KK6; 07 5.1.1-5.1.5; 09 5.3 items 1-2): the three model kinds carry their name pattern,
/// a form is named after its main handler, no handler or model touches an entity of 06, a field has the name 06 gives or the one 07 gives, and a model
/// property carries none of the framework's built-in validation attributes (09 3.4 item 1, the <c>BuiltInRule</c>). The value formats of 07 2.9 (T216):
/// a form, a query and a page bind no time, no floating point number and no decimal without its amount or percentage rule, and a machine body carries no
/// date and its byte counts are integers.
/// The rules run over the application assembly and fail on the deliberate samples (09 19.2 item 3) - every rule has its own violation and the quiet
/// neighbours stay quiet.
/// </summary>
public sealed class ModelBindingRuleTests
{
    private static readonly Lazy<Assembly> _sample =
        new(() => Assembly.LoadFrom(Path.Combine(AppContext.BaseDirectory, "ModelBindingViolationSample.dll")));

    [Fact]
    public void ApplicationAssembly_BreaksNoModelBindingRule()
    {
        Assert.Empty(ModelBindingRules.Violations(typeof(Program).Assembly, ModelBindingRules.EntityTypesOfContext()));
    }

    [Fact]
    public void EntitySet_IsTheSetsOfTheContext_AndHoldsTheEntitiesOf06()
    {
        IReadOnlySet<Type> entities = ModelBindingRules.EntityTypesOfContext();

        Assert.True(entities.Count > 50);
        Assert.Contains(entities, t => t.Name == "Variant");
        Assert.Contains(entities, t => t.Name == "Product");
    }

    [Fact]
    public void Sample_ReportsEveryRuleOnItsOwnViolation()
    {
        Assembly sample = _sample.Value;
        List<string> violations = ModelBindingRules.Violations(sample, SampleEntities(sample));

        string[] expected =
        [
            // Names and markers (KK1).
            "KindName:BadKindName",
            "KindName:WrongKindInput",
            "MissingMarker:UnmarkedInput",
            "InputName:MisnamedInputPage:SaveAnythingInput",

            // No entity in a model or in a handler's signature (KK4).
            "EntityIsModel:ExposedEntityInput",
            "EntityInModel:WrapsEntityInput.Item:SampleOrderItem",
            "EntityInModel:WrapsEntityInput.Rows.File:SampleFile",
            "EntityBound:BindsEntityPage.Item:SampleOrderItem",
            "EntityBound:BindsEntityPage.OnPostChangeAsync(item):SampleOrderItem",
            "EntityBound:BindsEntityPage.OnPostChangeAllAsync(items):SampleOrderItem",
            "UnmarkedBound:BindsPlainClassPage.OnPostSaveAsync(dto):PlainDto",

            // The framework's validation attributes on input and query models, at any depth (09 3.4 item 1); a machine request may carry one.
            "BuiltInRule:BuiltInRuleInput.Name:RequiredAttribute",
            "BuiltInRule:BuiltInRuleInput.Name:StringLengthAttribute",
            "BuiltInRule:BuiltInRuleQuery.Search:EmailAddressAttribute",
            "BuiltInRule:BuiltInRuleRowsInput.Items.Quantity:RangeAttribute",
            "BuiltInRule:DataTypePasswordInput.Password:DataTypeAttribute",

            // Field names (KK6).
            "FieldName:WrongFieldNamesInput.Cost",
            "SpecNameShadows06:WrongFieldNamesInput.Price",

            // The value formats of 07 2.9 (T216; 09 7.2 items 2-3): no time, no floating point number, no decimal without its rule - in a model, a
            // row and a page's signature.
            "TimeBound:TimedInput.StartsAt:DateTimeOffset",
            "TimeBound:TimedInput.OpensAt:TimeOnly",
            "TimeBound:TimedInput.Rows.Days:DateTime",
            "FloatBound:FloatQuery.Rate:Double",
            "FloatBound:FloatQuery.Weight:Single",
            "DecimalWithoutRule:UnruledAmountInput.Deposit:Decimal",
            "DecimalWithoutRule:UnruledAmountInput.Portion:Decimal",
            "DecimalWithoutRule:UnruledAmountInput.Rows.Fee:Decimal",
            "TimeBound:TimedPage.Stamp:DateTimeOffset",
            "TimeBound:TimedPage.OnGet(day):DateTime",
            "FloatBound:TimedPage.OnGetRate(rate):Double",
            "DecimalWithoutRule:TimedPage.OnPostSaveAsync(price):Decimal",

            // A machine body carries no date and its byte counts are integers (07 2.9.6; T216).
            "MachineDate:StampedResponse.CreatedAt:DateTimeOffset",
            "MachineDate:DatedUploadRequest.ExpiresOn:DateOnly",
            "ByteCountNotNumber:TextBytesResponse.ReceivedBytes:String",
            "ByteCountNotNumber:TextBytesResponse.TotalBytes:Double",
        ];

        Assert.Equal(expected.Order(StringComparer.Ordinal), violations.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void Sample_QuietNeighbours_ProduceNothing()
    {
        Assembly sample = _sample.Value;
        List<string> violations = ModelBindingRules.Violations(sample, SampleEntities(sample));

        string[] quiet =
        [
            "AddNoteInput", "AdminAddNoteInput", "SignInInput", "ReturnItemsInput", "ReturnRow", "NoteQuery", "StartUploadRequest", "UploadStatusResponse",
            "CleanNotesPage", "CleanAdminNotesPage",
        ];
        foreach (string name in quiet)
        {
            Assert.DoesNotContain(violations, v => v.Contains(name, StringComparison.Ordinal));
        }
    }

    [Theory]
    [InlineData("AddCartItemInput", typeof(IFormInput))]
    [InlineData("AdminReopenDefectClaimInput", typeof(IFormInput))]
    [InlineData("ProductQuery", typeof(IQueryModel))]
    [InlineData("StartUploadRequest", typeof(IMachineRequest))]
    [InlineData("UploadStatusResponse", typeof(IMachineResponse))]
    [InlineData("UploadRejectedResponse", typeof(IMachineResponse))]
    public void ModelNaming_ReadsTheKindFromTheSuffix(string name, Type marker)
    {
        Assert.Equal(marker, ModelNaming.MarkerOfName(name));
    }

    [Theory]
    [InlineData("CartItemModel")]
    [InlineData("InputBox")]
    [InlineData("Product")]
    public void ModelNaming_NameWithoutAKindSuffix_HasNoKind(string name)
    {
        Assert.Null(ModelNaming.MarkerOfName(name));
    }

    [Theory]
    [InlineData("OnPostAddAsync", "Add")]
    [InlineData("OnPostAddCartItemAsync", "AddCartItem")]
    [InlineData("OnPostRemoveCartItem", "RemoveCartItem")]
    [InlineData("OnGetExportAsync", "Export")]
    [InlineData("OnPost", null)]
    [InlineData("OnPostAsync", null)]
    [InlineData("OnGet", null)]
    [InlineData("OnGetAsync", null)]
    [InlineData("Add", null)]
    public void ModelNaming_ReadsTheHandlerName(string method, string? expected)
    {
        Assert.Equal(expected, ModelNaming.HandlerNameOf(method));
    }

    [Theory]
    [InlineData("AddCartItem", "AddCartItemInput", true)]
    [InlineData("ReopenDefectClaim", "AdminReopenDefectClaimInput", true)]
    [InlineData("AddCartItem", "AddCartItemsInput", false)]
    [InlineData("AddCartItem", "AddCartItemQuery", false)]
    [InlineData("AddCartItem", "AdminAdminAddCartItemInput", false)]
    public void ModelNaming_FormInputIsNamedAfterTheHandler(string handler, string input, bool expected)
    {
        Assert.Equal(expected, ModelNaming.IsInputNameOf(handler, input));
    }

    private static readonly string[] _entityNames = ["SampleOrderItem", "SampleFile", "ExposedEntityInput"];

    private static HashSet<Type> SampleEntities(Assembly sample) =>
        [.. _entityNames.Select(n => sample.GetTypes().Single(t => t.Name == n))];
}
