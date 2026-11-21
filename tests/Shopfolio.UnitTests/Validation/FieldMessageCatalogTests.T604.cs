using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Validation;

/// <summary>
/// The action-level messages of the catalog (T604 D-141; 09 3.4 item 4, 8.2 item 4; K-1627 (1)): a code that belongs to no field keeps its sentence in the catalog, the sentence is the one
/// 04 2.7.3.1 writes, and the forgery refusal shows the catalog's sentence - no copy of its own.
/// </summary>
public sealed partial class FieldMessageCatalogTests
{
    [Fact]
    public void TheForgeryRefusalsSentence_IsTheCatalogsMessageOfItsCode_AndThe04Sentence_D141()
    {
        string ui = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "04_UI_SPECS.md"));
        string clause = ui[ui.IndexOf("- **2.7.3.1 İşlem düzeyi.**", StringComparison.Ordinal)..];
        clause = clause[..clause.IndexOf('\n', StringComparison.Ordinal)];

        string message = FieldMessageCatalog.ActionMessage(ErrorCode.AntiforgeryValidationFailed);

        Assert.Contains("\"" + message + "\"", clause, StringComparison.Ordinal);
        Assert.Contains("`AntiforgeryValidationFailed`", clause, StringComparison.Ordinal);
        Assert.Equal(message, ForgeryProtectionFilter.Message);
    }

    [Fact]
    public void AnActionMessage_BelongsToACodeOfNoField_AndNamesNoCode_D141()
    {
        Assert.NotEmpty(FieldMessageCatalog.ActionMessages);
        foreach ((ErrorCode code, string message) in FieldMessageCatalog.ActionMessages)
        {
            Assert.DoesNotContain(code.ToString(), FieldCodes.All);
            Assert.DoesNotContain(code.ToString(), message, StringComparison.Ordinal);
            Assert.EndsWith(".", message, StringComparison.Ordinal);
        }

        Assert.Throws<ArgumentException>(() => FieldMessageCatalog.ActionMessage(ErrorCode.NotFound));
    }
}
