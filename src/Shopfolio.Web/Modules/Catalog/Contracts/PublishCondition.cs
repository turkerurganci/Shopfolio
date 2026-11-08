namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>
/// The closed list of the publish gate's conditions by name (02 3.7.3, 3.7.4; 07 6.2 <c>PublishGateIncomplete</c>): what a refused move to Published names, so the panel says
/// which condition is not met. The list is what the gate asks and nothing more: the gate does not look at what the product is (10 KD-28), so no condition names an age, a
/// permission, a blocked category or a legal document.
/// </summary>
public static class PublishCondition
{
    /// <summary>The product's name is entered.</summary>
    public const string Name = "Name";

    /// <summary>The product hangs on a category and one of them is its main category (02 3.4.4).</summary>
    public const string MainCategory = "MainCategory";

    /// <summary>The product's type is one of the three (02 3.2.2).</summary>
    public const string ProductType = "ProductType";

    /// <summary>The product has at least one Published variant (02 3.7.3).</summary>
    public const string PublishedVariant = "PublishedVariant";

    /// <summary>Every Published variant of a digital product is bound to a file - its own or its product's (02 3.7.4).</summary>
    public const string DigitalFile = "DigitalFile";

    /// <summary>A service product carries its fulfilment period (02 3.2.2, 3.7.4; P-44).</summary>
    public const string FulfilmentPeriod = "FulfilmentPeriod";

    /// <summary>Every condition the gate can name, in the order the gate asks them.</summary>
    public static IReadOnlyList<string> All { get; } = [Name, MainCategory, ProductType, PublishedVariant, DigitalFile, FulfilmentPeriod];
}
