using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

using Shopfolio.Infrastructure;

namespace Shopfolio.UnitTests.Samples;

/// <summary>
/// Sample entities for the tests of the model rules (T101): the application has no entity of its own yet (T103 brings the first),
/// so the rules are shown on a small sample model that is added to the one application context through the framework's model customizer.
/// The test code defines no second context class (09 4.1 item 2). The sample files are shared as linked files with the integration tests.
/// </summary>
internal static class SampleModel
{
    public const string GoodNamespace = "Shopfolio.UnitTests.Samples.Good";

    public const string ViolatingNamespace = "Shopfolio.UnitTests.Samples.Violating";

    public const string UntypedDecimalNamespace = "Shopfolio.UnitTests.Samples.UntypedDecimal";

    /// <summary>
    /// The application context with the rule-following sample entities (<see cref="GoodNamespace"/>) added to its model. The connection
    /// string is a placeholder unless the test opens the connection; nothing connects by itself.
    /// </summary>
    public static AppDbContext CreateGoodContext(string connectionString = "Host=localhost;Database=shopfolio_sample_model")
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, connectionString);
        builder.ReplaceService<IModelCustomizer, GoodModelCustomizer>();
        return new AppDbContext(builder.Options);
    }

    /// <summary>The application context with the sample entities that break the model rules, for the negative tests of the rule checks.</summary>
    public static AppDbContext CreateViolatingContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, "Host=localhost;Database=shopfolio_sample_model");
        builder.ReplaceService<IModelCustomizer, ViolatingModelCustomizer>();
        return new AppDbContext(builder.Options);
    }

    /// <summary>The application context with a sample entity whose decimal property has no declared type; the model rule refuses it when the model is built.</summary>
    public static AppDbContext CreateUntypedDecimalContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, "Host=localhost;Database=shopfolio_sample_model");
        builder.ReplaceService<IModelCustomizer, UntypedDecimalModelCustomizer>();
        return new AppDbContext(builder.Options);
    }

    /// <summary>The application context with a sample entity whose enum property the context does not convert to text; the model rule refuses it.</summary>
    public static AppDbContext CreateUnconvertedEnumContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, "Host=localhost;Database=shopfolio_sample_model");
        builder.ReplaceService<IModelCustomizer, UnconvertedEnumModelCustomizer>();
        return new AppDbContext(builder.Options);
    }

    private sealed class UnconvertedEnumModelCustomizer(ModelCustomizerDependencies dependencies)
        : SampleModelCustomizer(dependencies, "Shopfolio.UnitTests.Samples.EnumWithoutText");

    private sealed class UntypedDecimalModelCustomizer(ModelCustomizerDependencies dependencies)
        : SampleModelCustomizer(dependencies, UntypedDecimalNamespace);

    private sealed class GoodModelCustomizer(ModelCustomizerDependencies dependencies)
        : SampleModelCustomizer(dependencies, GoodNamespace);

    private sealed class ViolatingModelCustomizer(ModelCustomizerDependencies dependencies)
        : SampleModelCustomizer(dependencies, ViolatingNamespace);

    private abstract class SampleModelCustomizer(ModelCustomizerDependencies dependencies, string sampleNamespace)
        : ModelCustomizer(dependencies)
    {
        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            base.Customize(modelBuilder, context);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SampleModel).Assembly, type => type.Namespace == sampleNamespace);
        }
    }
}
