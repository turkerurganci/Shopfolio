using System.Text.RegularExpressions;

using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Module boundary rule (09 6 item 2 and 4): a module reaches another module only through that module's
/// <c>Shopfolio.Modules.&lt;Module&gt;.Contracts</c> namespace. The rule is shown to fail on a deliberate violation
/// sample that lives in its own assembly (09 19.2 item 3).
/// </summary>
public sealed class ModuleBoundaryTests
{
    private const string ModulesNamespace = @"Shopfolio\.Modules";

    private static readonly Regex _moduleNamePattern = new(@"^Shopfolio\.Modules\.([^.]+)", RegexOptions.CultureInvariant);

    [Fact]
    public void ModuleReferences_InApplicationAssembly_OnlyGoThroughContracts()
    {
        ArchUnitNET.Domain.Architecture architecture = new ArchLoader().LoadAssemblies(typeof(Program).Assembly).Build();

        IReadOnlyList<string> violations = FindViolations(architecture);

        Assert.Empty(violations);
    }

    [Fact]
    public void ModuleReferences_InViolationSample_BreakTheRuleOnlyForTheInternalReference()
    {
        string samplePath = Path.Combine(AppContext.BaseDirectory, "ModuleBoundaryViolationSample.dll");
        System.Reflection.Assembly sample = System.Reflection.Assembly.LoadFrom(samplePath);
        ArchUnitNET.Domain.Architecture architecture = new ArchLoader().LoadAssemblies(sample).Build();

        IReadOnlyList<string> violations = FindViolations(architecture);

        string violation = Assert.Single(violations);
        Assert.Contains("ForbiddenCaller", violation, StringComparison.Ordinal);
        Assert.Contains("ProductEntity", violation, StringComparison.Ordinal);
    }

    [Fact]
    public void ModuleNames_InViolationSample_AreCatalogAndOrdering()
    {
        string samplePath = Path.Combine(AppContext.BaseDirectory, "ModuleBoundaryViolationSample.dll");
        System.Reflection.Assembly sample = System.Reflection.Assembly.LoadFrom(samplePath);
        ArchUnitNET.Domain.Architecture architecture = new ArchLoader().LoadAssemblies(sample).Build();

        IReadOnlyList<string> modules = ModuleNames(architecture);

        Assert.Equal(["Catalog", "Ordering"], modules);
    }

    private static List<string> ModuleNames(ArchUnitNET.Domain.Architecture architecture)
    {
        return architecture.Namespaces
            .Select(ns => _moduleNamePattern.Match(ns.FullName))
            .Where(match => match.Success)
            .Select(match => match.Groups[1].Value)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToList();
    }

    private static List<string> FindViolations(ArchUnitNET.Domain.Architecture architecture)
    {
        List<string> violations = [];
        foreach (string module in ModuleNames(architecture))
        {
            // Own module: any depth. Other module: everything except its Contracts namespace.
            string ownModule = $@"^{ModulesNamespace}\.{module}(\.|$)";
            string otherModuleInternals = $@"^{ModulesNamespace}\.(?!{module}(\.|$))[^.]+(\.(?!Contracts(\.|$)).*)?$";

            ArchRule<IType> rule = Types().That().ResideInNamespaceMatching(ownModule)
                .Should().NotDependOnAnyTypesThat().ResideInNamespaceMatching(otherModuleInternals)
                .WithoutRequiringPositiveResults();

            foreach (EvaluationResult result in rule.Evaluate(architecture))
            {
                if (!result.Passed)
                {
                    violations.Add(result.Description);
                }
            }
        }

        return violations;
    }
}
