using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure.Binding;
using Shopfolio.UnitTests.Architecture;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the model tables of 07 5.2, 5.3.2 and 5.4.2 equal to the model types of the code, and the use cases of the inventory (07 3.2) equal to what their
/// handlers bind, in both directions (T538 criteria 1-5, 7; KK1-KK5, KK7; 09 21.4 item 1; 07 5.1.5). The form inputs, the query models and the machine bodies are
/// compared by name and by field (the query parameters in camelCase); a model of a row's "Uçlar" column is bound by a handler of that use case, a handler that
/// reads input binds a model of its row or takes a route value, the handlers of 07 5.2.4 bind no model, and no handler binds an entity of 06. There is no list of
/// known differences: a difference is closed by the document or the code in the same change (K-1616). Reads <c>Docs/07_API_DESIGN.md</c> and the application assembly only - no
/// database, no network, no file written (09 19.2 item 2). Runs in the Lint step. The checks are shown to fail on a deliberately wrong text and on sample models and
/// handlers (09 19.2 item 3); each failure names what differs.
/// </summary>
[Trait("Consistency", "true")]
public sealed class ModelInventoryConsistencyTests
{
    private static readonly System.Reflection.Assembly Application = typeof(Program).Assembly;

    [Fact]
    public void FormInputs_AreTheRowsOf07_5_2_ByNameAndField()
    {
        List<string> problems = FormProblems(ApiInventoryDocument.Text());

        Assert.True(problems.Count == 0, ModelInventoryCheck.Report(problems));
    }

    [Fact]
    public void QueryModels_AreTheRowsOf07_5_3_2_ByNameAndParameter()
    {
        List<string> problems = QueryProblems(ApiInventoryDocument.Text());

        Assert.True(problems.Count == 0, ModelInventoryCheck.Report(problems));
    }

    [Fact]
    public void MachineBodies_AreTheRowsOf07_5_4_2_ByNameAndField()
    {
        List<string> problems = MachineProblems(ApiInventoryDocument.Text());

        Assert.True(problems.Count == 0, ModelInventoryCheck.Report(problems));
    }

    [Fact]
    public void Handlers_BindTheModelsOfTheirRows_AndNoEntity()
    {
        List<string> problems = HandlerProblems(ApiInventoryDocument.Text());

        Assert.True(problems.Count == 0, ModelInventoryCheck.Report(problems));
    }

    [Fact]
    public void FormCheck_ADocumentRowDeleted_NamesTheModelOnlyInTheCode()
    {
        string text = DropRow(ApiInventoryDocument.Text(), "| `CancelOrderInput` |");

        Assert.Contains("Model: only in the model: CancelOrderInput", FormProblems(text));
    }

    [Fact]
    public void FormCheck_AModelAddedToTheCode_NamesTheModelOnlyInTheCode()
    {
        List<string> problems = ModelInventoryCheck.Problems(
            ApiInventoryDocument.FormRows(ApiInventoryDocument.Text()),
            [.. ModelInventoryCheck.ModelsOf(Application, ModelInventoryCheck.Kind.Form), typeof(SampleExtraInput)],
            ModelInventoryCheck.Kind.Form);

        Assert.Contains("Model: only in the model: SampleExtraInput", problems);
    }

    [Fact]
    public void FormCheck_AFieldRenamedInTheDocument_NamesBothSides()
    {
        string text = ApiInventoryDocument.Text().Replace("`CartItemId` (`06 §4.16`) · `Quantity` (`04` 7.1.4.1)", "`LineId` (`06 §4.16`) · `Quantity` (`04` 7.1.4.1)", StringComparison.Ordinal);
        List<string> problems = FormProblems(text);

        Assert.Contains("Field:UpdateCartItemInput: only in the document: LineId", problems);
        Assert.Contains("Field:UpdateCartItemInput: only in the model: CartItemId", problems);
    }

    [Fact]
    public void FormCheck_AListFieldRenamedInTheDocument_NamesTheNestedField()
    {
        string text = ApiInventoryDocument.Text().Replace("kalem başına `OrderItemId`, `Quantity` ve `OriginalOpenQuantity`", "kalem başına `LineKey`, `Quantity` ve `OriginalOpenQuantity`", StringComparison.Ordinal);
        List<string> problems = FormProblems(text);

        Assert.Contains("Field:CancelOrderItemsInput.Items: only in the document: LineKey", problems);
        Assert.Contains("Field:CancelOrderItemsInput.Items: only in the model: OrderItemId", problems);
    }

    [Fact]
    public void QueryCheck_AParameterRenamedInTheDocument_NamesBothSides()
    {
        string text = ApiInventoryDocument.Text().Replace("**`minPrice`**", "**`minimumPrice`**", StringComparison.Ordinal);
        List<string> problems = QueryProblems(text);

        Assert.Contains("Field:ProductQuery: only in the document: minimumPrice", problems);
        Assert.Contains("Field:ProductQuery: only in the model: minPrice", problems);
    }

    [Fact]
    public void MachineCheck_AFieldRenamedInTheDocument_NamesBothSides()
    {
        string text = ApiInventoryDocument.Text().Replace("**`receivedBytes`**", "**`gotBytes`**", StringComparison.Ordinal);
        List<string> problems = MachineProblems(text);

        Assert.Contains("Field:UploadStatusResponse: only in the document: gotBytes", problems);
        Assert.Contains("Field:UploadStatusResponse: only in the model: receivedBytes", problems);
    }

    [Fact]
    public void HandlerCheck_AHandlerThatReadsInputWithoutAModel_IsNamed()
    {
        List<string> problems = EndpointBindingCheck.Problems(
            [],
            [],
            [],
            [new EndpointRef("UC-900", "POST", "/sample", "Save")],
            [new PageRoute("/sample", typeof(SamplePage))],
            new HashSet<Type>(),
            []);

        Assert.Contains("InputWithoutModel:UC-900:SamplePage.OnPostSaveAsync(note)", problems);
    }

    [Fact]
    public void HandlerCheck_AModellessHandlerThatBindsAModel_IsNamed()
    {
        List<string> problems = EndpointBindingCheck.Problems(
            [],
            [],
            ["UC-901"],
            [new EndpointRef("UC-901", "POST", "/sample", "SaveWithModel")],
            [new PageRoute("/sample", typeof(SamplePage))],
            new HashSet<Type>(),
            []);

        Assert.Contains("ModelOnModelless:UC-901", problems);
    }

    [Fact]
    public void HandlerCheck_AHandlerThatBindsAnEntity_IsNamed()
    {
        List<string> problems = EndpointBindingCheck.Problems(
            [],
            [],
            [],
            [new EndpointRef("UC-902", "POST", "/sample", "SaveEntity")],
            [new PageRoute("/sample", typeof(SamplePage))],
            new HashSet<Type> { typeof(SampleEntity) },
            []);

        Assert.Contains("EntityBound:UC-902:SamplePage.OnPostSaveEntityAsync(entity)", problems);
    }

    [Fact]
    public void HandlerCheck_AModelNoHandlerBinds_IsNamed()
    {
        List<string> problems = EndpointBindingCheck.Problems([], [], [], [], [new PageRoute("/sample", typeof(SamplePage))], new HashSet<Type>(), [typeof(SampleExtraInput)]);

        Assert.Contains("ModelWithoutEndpoint:SampleExtraInput", problems);
    }

    [Fact]
    public void Checks_TouchNeitherTheDatabaseNorTheNetworkNorAFile()
    {
        string[] sources =
        [
            "ApiInventoryDocument.cs",
            "EndpointBindingCheck.cs",
            "ModelInventoryCheck.cs",
        ];
        string[] forbidden = ["Connection" + "String", "Npg" + "sql", "Http" + "Client", "File.Write", "File.Append", "File.Delete", "Directory.Create"];
        string folder = Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Consistency");

        foreach (string source in sources)
        {
            string text = File.ReadAllText(Path.Combine(folder, source));
            Assert.DoesNotContain(forbidden, word => text.Contains(word, StringComparison.Ordinal));
        }
    }

    private static string DropRow(string text, string rowStart) =>
        string.Join('\n', text.Split('\n').Where(l => !l.StartsWith(rowStart, StringComparison.Ordinal)));

    private static List<string> FormProblems(string text) =>
        ModelInventoryCheck.Problems(ApiInventoryDocument.FormRows(text), ModelInventoryCheck.ModelsOf(Application, ModelInventoryCheck.Kind.Form), ModelInventoryCheck.Kind.Form);

    private static List<string> QueryProblems(string text) =>
        ModelInventoryCheck.Problems(ApiInventoryDocument.QueryRows(text), ModelInventoryCheck.ModelsOf(Application, ModelInventoryCheck.Kind.Query), ModelInventoryCheck.Kind.Query);

    private static List<string> MachineProblems(string text) =>
        ModelInventoryCheck.Problems(ApiInventoryDocument.MachineRows(text), ModelInventoryCheck.ModelsOf(Application, ModelInventoryCheck.Kind.Machine), ModelInventoryCheck.Kind.Machine);

    private static List<string> HandlerProblems(string text) =>
        EndpointBindingCheck.Problems(
            ApiInventoryDocument.FormRows(text),
            ApiInventoryDocument.QueryRows(text),
            ApiInventoryDocument.ModellessUseCases(text),
            ApiInventoryDocument.Endpoints(text),
            EndpointBindingCheck.PagesOf(Application),
            ModelBindingRules.EntityTypesOfContext(),
            [
                .. ModelInventoryCheck.ModelsOf(Application, ModelInventoryCheck.Kind.Form),
                .. ModelInventoryCheck.ModelsOf(Application, ModelInventoryCheck.Kind.Query),
            ]);

    /// <summary>A sample form model that 07 does not list.</summary>
    internal sealed class SampleExtraInput : IFormInput
    {
        public string? Name { get; set; }
    }

    internal sealed class SampleEntity
    {
        public Guid Id { get; set; }
    }

    /// <summary>A sample page whose handlers read input without a model, bind a model, and bind an entity.</summary>
    internal sealed class SamplePage : PageModel
    {
        public object? Last { get; private set; }

        public void OnPostSaveAsync(string note) => Last = note;

        public void OnPostSaveWithModelAsync(SampleExtraInput input) => Last = input;

        public void OnPostSaveEntityAsync(SampleEntity entity) => Last = entity;
    }
}
