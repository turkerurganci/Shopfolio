using Shopfolio.UnitTests.Architecture;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the model tables of 07 5.2, 5.3.2 and 5.4.2 equal to the model types of the code, and the use cases of the inventory (07 3.2) equal to what their
/// handlers bind, in both directions (T538 criteria 1-5, 7; KK1-KK5, KK7; 09 21.4 item 1; 07 5.1.5). The form inputs, the query models and the machine bodies are
/// compared by name and by field (the query parameters in camelCase); a model of a row's "Uçlar" column is bound by a handler of that use case, a handler that
/// reads input binds a model of its row or takes a route value, the handlers of 07 5.2.4 bind no model, and no handler binds an entity of 06. Reads
/// <c>Docs/07_API_DESIGN.md</c> and the application assembly only - no database, no network, no file written (09 19.2 item 2). Runs in the Lint step.
/// </summary>
[Trait("Consistency", "true")]
public sealed class ModelInventoryConsistencyTests
{
    [Fact]
    public void FormInputs_AreTheRowsOf07_5_2_ByNameAndField()
    {
        List<string> problems = ModelInventoryCheck.Problems(
            ApiInventoryDocument.FormRows(ApiInventoryDocument.Text()),
            ModelInventoryCheck.ModelsOf(typeof(Program).Assembly, ModelInventoryCheck.Kind.Form),
            ModelInventoryCheck.Kind.Form);

        Assert.Empty(problems);
    }

    [Fact]
    public void QueryModels_AreTheRowsOf07_5_3_2_ByNameAndParameter()
    {
        List<string> problems = ModelInventoryCheck.Problems(
            ApiInventoryDocument.QueryRows(ApiInventoryDocument.Text()),
            ModelInventoryCheck.ModelsOf(typeof(Program).Assembly, ModelInventoryCheck.Kind.Query),
            ModelInventoryCheck.Kind.Query);

        Assert.Empty(problems);
    }

    [Fact]
    public void MachineBodies_AreTheRowsOf07_5_4_2_ByNameAndField()
    {
        List<string> problems = ModelInventoryCheck.Problems(
            ApiInventoryDocument.MachineRows(ApiInventoryDocument.Text()),
            ModelInventoryCheck.ModelsOf(typeof(Program).Assembly, ModelInventoryCheck.Kind.Machine),
            ModelInventoryCheck.Kind.Machine);

        Assert.Empty(problems);
    }

    [Fact]
    public void Handlers_BindTheModelsOfTheirRows_AndNoEntity()
    {
        string text = ApiInventoryDocument.Text();
        List<string> problems = EndpointBindingCheck.Problems(
            ApiInventoryDocument.FormRows(text),
            ApiInventoryDocument.QueryRows(text),
            ApiInventoryDocument.ModellessUseCases(text),
            ApiInventoryDocument.Endpoints(text),
            EndpointBindingCheck.PagesOf(typeof(Program).Assembly),
            ModelBindingRules.EntityTypesOfContext());

        Assert.Empty(problems);
    }
}
