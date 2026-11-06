using System.Text.RegularExpressions;

using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;

using Shopfolio.UnitTests.Support;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The architecture rules of 09 6 (T03, T218; 05 3.3, 3.4.1, 3.4.5): a module reaches another module only through that module's
/// <c>Shopfolio.Modules.&lt;Module&gt;.Contracts</c> namespace (item 2), everything of a module outside <c>Contracts</c> is <c>internal</c>
/// (item 1), a page or an endpoint touches neither a module's internals nor the database context (item 2), and the common infrastructure
/// reaches no module type - the one exception is the context class <c>AppDbContext</c>, left out by its name, not by its namespace (item 4).
/// Each rule is shown to fail on a deliberate violation sample that lives in its own assembly (09 19.2 item 3).
/// </summary>
public sealed class ModuleBoundaryTests
{
    private const string ModulesNamespace = @"Shopfolio\.Modules";

    /// <summary>The module's own namespace tree is allowed to the module; for another module only <c>Contracts</c> is open.</summary>
    private const string AnyModuleInternals = @"^Shopfolio\.Modules\.[^.]+(\.(?!Contracts(\.|$)).*)?$";

    private const string AnyModuleContracts = @"^Shopfolio\.Modules\.[^.]+\.Contracts(\.|$)";

    private const string AnyModuleType = @"^Shopfolio\.Modules(\.|$)";

    /// <summary>The page and endpoint namespaces, and the namespace the Razor compiler puts the class of a <c>.cshtml</c> into (D-73).</summary>
    private const string PageAndEndpointTypes = @"^(Shopfolio\.(Pages|Endpoints)|AspNetCoreGeneratedDocument)(\.|$)";

    private const string InfrastructureTypes = @"^Shopfolio\.Infrastructure(\.|$)";

    /// <summary>The one infrastructure class that may reach module types; its nested compiler types go with it.</summary>
    private const string TheContextClass = @"^Shopfolio\.Infrastructure\.AppDbContext(\+.*)?$";

    private const string EntityFramework = @"^Microsoft\.EntityFrameworkCore(\.|$)";

    private static readonly string[] _theNineModules =
    [
        "Administration", "AfterSales", "Catalog", "CorporateContent", "Membership", "Notifications", "Ordering", "Payment", "Settings",
    ];

    private static readonly Regex _moduleNamePattern = new(@"^Shopfolio\.Modules\.([^.]+)", RegexOptions.CultureInvariant);

    private static readonly Lazy<ArchUnitNET.Domain.Architecture> _application =
        new(() => new ArchLoader().LoadAssemblies(typeof(Program).Assembly).Build());

    private static readonly Lazy<System.Reflection.Assembly> _sampleAssembly =
        new(() => System.Reflection.Assembly.LoadFrom(Path.Combine(AppContext.BaseDirectory, "ModuleBoundaryViolationSample.dll")));

    private static readonly Lazy<ArchUnitNET.Domain.Architecture> _sample =
        new(() => new ArchLoader().LoadAssemblies(_sampleAssembly.Value).Build());

    // --- Module boundary: another module only through its Contracts (09 6 item 2) ---

    [Fact]
    public void ModuleReferences_InApplicationAssembly_OnlyGoThroughContracts()
    {
        Assert.Empty(FindModuleReferenceViolations(_application.Value));
    }

    [Fact]
    public void ModuleReferences_InViolationSample_BreakTheRuleOnlyForTheInternalReference()
    {
        IReadOnlyList<string> violations = FindModuleReferenceViolations(_sample.Value);

        Assert.Contains(violations, v => v.Contains("ForbiddenCaller", StringComparison.Ordinal) && v.Contains("ProductEntity", StringComparison.Ordinal));
        Assert.DoesNotContain(violations, v => v.Contains("AllowedCaller", StringComparison.Ordinal));
    }

    [Fact]
    public void ModuleReferences_InViolationSample_BreakTheRuleThroughTheContextToo()
    {
        IReadOnlyList<string> violations = FindModuleReferenceViolations(_sample.Value);

        Assert.Contains(violations, v => v.Contains("ForbiddenContextReader", StringComparison.Ordinal));
    }

    [Fact]
    public void ModuleNames_InViolationSample_AreCatalogAndOrdering()
    {
        Assert.Equal(["Catalog", "Ordering"], ModuleNames(_sample.Value));
    }

    [Fact]
    public void ModuleNames_InApplicationAssembly_AreTheNineModulesOnly()
    {
        Assert.Equal(_theNineModules, ModuleNames(_application.Value));
    }

    // --- Public surface: everything outside Contracts is internal (09 6 item 1) ---

    [Fact]
    public void ModuleTypes_OutsideContracts_AreNotPublic()
    {
        Assert.Empty(Evaluate(_application.Value, NotPublicOutsideContractsRule()));
    }

    [Fact]
    public void ModuleTypes_InViolationSample_ABreakOfTheSurfaceIsReportedForThePublicLeakOnly()
    {
        IReadOnlyList<string> violations = Evaluate(_sample.Value, NotPublicOutsideContractsRule());

        string violation = Assert.Single(violations);
        Assert.Contains("PublicLeak", violation, StringComparison.Ordinal);
    }

    [Fact]
    public void ContractsTypes_InApplicationAssembly_DoNotReachModuleInternals()
    {
        Assert.Empty(Evaluate(_application.Value, ContractsReachNothingRule()));
    }

    [Fact]
    public void ContractsTypes_InViolationSample_ReportTheLeakyContract()
    {
        IReadOnlyList<string> violations = Evaluate(_sample.Value, ContractsReachNothingRule());

        string violation = Assert.Single(violations);
        Assert.Contains("LeakyContract", violation, StringComparison.Ordinal);
    }

    [Fact]
    public void EveryModule_InApplicationAssembly_HasAContractsNamespaceWithAnOpenInterface()
    {
        Assert.Empty(ModulesWithoutOpenInterface(_application.Value, _theNineModules));
    }

    [Fact]
    public void EveryModule_InViolationSample_OnlyOrderingHasNoOpenInterface()
    {
        Assert.Equal(["Ordering"], ModulesWithoutOpenInterface(_sample.Value, ["Catalog", "Ordering"]));
    }

    private static List<string> ModulesWithoutOpenInterface(ArchUnitNET.Domain.Architecture architecture, IEnumerable<string> modules)
    {
        return modules
            .Where(module => !architecture.Interfaces.Any(
                i => i.FullName == $"Shopfolio.Modules.{module}.Contracts.I{module}Module" && i.Visibility == Visibility.Public))
            .ToList();
    }

    // --- Pages and endpoints: no module internals, no database context (09 6 item 2; 05 3.3.12) ---

    [Fact]
    public void PagesAndEndpoints_InApplicationAssembly_TouchNeitherModuleInternalsNorTheContext()
    {
        Assert.Empty(Evaluate(_application.Value, PageRule()));
    }

    [Fact]
    public void PagesAndEndpoints_InViolationSample_ReportTheContextAndTheInternalTypeButNotTheContractsCall()
    {
        IReadOnlyList<string> violations = Evaluate(_sample.Value, PageRule());

        Assert.Contains(violations, v => v.Contains("ForbiddenPage ", StringComparison.Ordinal) && v.Contains("AppDbContext", StringComparison.Ordinal));
        Assert.Contains(violations, v => v.Contains("ForbiddenPageEntity", StringComparison.Ordinal) && v.Contains("ProductEntity", StringComparison.Ordinal));
        Assert.DoesNotContain(violations, v => v.Contains("AllowedPage", StringComparison.Ordinal));
    }

    [Fact]
    public void CompiledViews_InViolationSample_ReportTheContextButNotTheContractsCall()
    {
        IReadOnlyList<string> violations = Evaluate(_sample.Value, PageRule());

        Assert.Contains(violations, v => v.Contains("Pages_Forbidden_View", StringComparison.Ordinal) && v.Contains("AppDbContext", StringComparison.Ordinal));
        Assert.DoesNotContain(violations, v => v.Contains("Pages_Allowed_View", StringComparison.Ordinal));
    }

    // The available-operations pattern as a page of the application's namespace uses it (T217 r1 B1, r2 B1-r2; 09 8.3 items 1 and 3): OperationPatternPage
    // (OperationPagePattern.cs) lives in Shopfolio.Pages.Panel, so the page rule reads it - the runner is common infrastructure, the rule is the module's and the page reaches
    // it only through the module's Contracts interface (IPatternOrderOperations: the read for the GET, the write for the POST).
    private static readonly Lazy<ArchUnitNET.Domain.Architecture> _withPatternPage =
        new(() => new ArchLoader().LoadAssemblies(typeof(Program).Assembly, typeof(Shopfolio.Modules.Ordering.Contracts.IPatternOrderOperations).Assembly).Build());

    [Fact]
    public void OperationPatternPage_UnderThePageRule_IsCleanAndReachesTheRuleOnlyThroughTheContractsInterface()
    {
        Assert.Empty(Evaluate(_withPatternPage.Value, PageRule()));
        Assert.Empty(Evaluate(_withPatternPage.Value, ContractsReachNothingRule()));

        IType page = _withPatternPage.Value.Types.Single(t => t.FullName == "Shopfolio.Pages.Panel.OperationPatternPage");
        List<string> used = page.Dependencies.Select(d => d.Target.FullName).ToList();
        Assert.Contains("Shopfolio.Infrastructure.Operations.OperationRunner", used);
        Assert.Contains("Shopfolio.Modules.Ordering.Contracts.IPatternOrderOperations", used);

        // The page writes and applies no rule: it names no rule type and no gate, and no module type except the Contracts namespace (the module is the rule's one place).
        Assert.DoesNotContain("Shopfolio.Infrastructure.Operations.IOperationRule`1", used);
        Assert.DoesNotContain("Shopfolio.Infrastructure.Operations.OperationGate", used);
        Assert.DoesNotContain(used, name => name.StartsWith("Shopfolio.Modules.", StringComparison.Ordinal)
            && !name.StartsWith("Shopfolio.Modules.Ordering.Contracts.", StringComparison.Ordinal));
    }

    // The search engine data pattern as a page of the application's namespace uses it (T219; 09 6 items 2 and 7): SearchDataPatternPage (SearchDataPagePattern.cs) lives in
    // Shopfolio.Pages.Storefront, so the page rule reads it - the pattern is common infrastructure and the record's values come through the module's Contracts interface.
    [Fact]
    public void SearchDataPatternPage_UnderThePageRule_IsCleanAndTakesTheRecordsValuesOnlyThroughTheContractsInterface()
    {
        Assert.Empty(Evaluate(_withPatternPage.Value, PageRule()));
        Assert.Empty(Evaluate(_withPatternPage.Value, ContractsReachNothingRule()));

        IType page = _withPatternPage.Value.Types.Single(t => t.FullName == "Shopfolio.Pages.Storefront.SearchDataPatternPage");
        List<string> used = page.Dependencies.Select(d => d.Target.FullName).ToList();
        Assert.Contains("Shopfolio.Infrastructure.SearchEngine.SearchEngineData", used);
        Assert.Contains("Shopfolio.Infrastructure.SearchEngine.PageSearchData", used);
        Assert.Contains("Shopfolio.Modules.Catalog.Contracts.IPatternProductSearchSource", used);
        Assert.DoesNotContain(used, name => name.StartsWith("Shopfolio.Modules.", StringComparison.Ordinal)
            && !name.StartsWith("Shopfolio.Modules.Catalog.Contracts.", StringComparison.Ordinal));
        Assert.DoesNotContain(used, name => name.Contains("AppDbContext", StringComparison.Ordinal));
    }

    // The in-memory cache pattern as the application's modules and a page of the application's namespace use it (T229; 09 10.4 items 1 to 5, 6 items 2 and 7):
    // CachePatternPage (CachePattern.cs) lives in Shopfolio.Pages.Storefront and the module reads in Shopfolio.Modules.<Module>, so the page rule, the module rule and
    // the internal-only rule read them. The cache is reached inside the module; the page never names it.
    [Fact]
    public void CachePattern_UnderTheModuleRules_IsClean_AndOnlyTheModuleReachesTheCache()
    {
        Assert.Empty(Evaluate(_withPatternPage.Value, PageRule()));
        Assert.Empty(Evaluate(_withPatternPage.Value, ContractsReachNothingRule()));
        Assert.Empty(Evaluate(_withPatternPage.Value, NotPublicOutsideContractsRule()));
        Assert.Empty(FindModuleReferenceViolations(_withPatternPage.Value));

        IType page = _withPatternPage.Value.Types.Single(t => t.FullName == "Shopfolio.Pages.Storefront.CachePatternPage");
        List<string> pageUses = page.Dependencies.Select(d => d.Target.FullName).ToList();
        Assert.Contains("Shopfolio.Modules.Settings.Contracts.IPatternShopSettings", pageUses);
        Assert.Contains("Shopfolio.Modules.Catalog.Contracts.IPatternPriceSource", pageUses);
        Assert.Contains("Shopfolio.Modules.CorporateContent.Contracts.IPatternAnnouncementSource", pageUses);
        Assert.DoesNotContain(pageUses, name => name.StartsWith("Shopfolio.Infrastructure.", StringComparison.Ordinal));
        Assert.DoesNotContain(pageUses, name => name.StartsWith("Shopfolio.Modules.", StringComparison.Ordinal) && !name.Contains(".Contracts.", StringComparison.Ordinal));

        IType settings = _withPatternPage.Value.Types.Single(t => t.FullName == "Shopfolio.Modules.Settings.PatternShopSettings");
        List<string> settingsUses = settings.Dependencies.Select(d => d.Target.FullName).ToList();
        Assert.Contains("Shopfolio.Infrastructure.AppCache", settingsUses);
        Assert.Contains("Shopfolio.Infrastructure.CacheInvalidations", settingsUses);

        // The price path has no cache (05 7.10.1): the Catalog read names neither the cache nor an area.
        IType price = _withPatternPage.Value.Types.Single(t => t.FullName == "Shopfolio.Modules.Catalog.PatternPriceSource");
        Assert.DoesNotContain(price.Dependencies, d => d.Target.FullName is "Shopfolio.Infrastructure.AppCache" or "Shopfolio.Infrastructure.CacheArea");
    }

    [Fact]
    public void TheCache_InApplicationAssembly_IsCommonInfrastructureThatReachesNoModuleType()
    {
        IType[] cache = [.. _application.Value.Types.Where(t => t.FullName is "Shopfolio.Infrastructure.AppCache" or "Shopfolio.Infrastructure.CacheArea" or "Shopfolio.Infrastructure.CacheInvalidations")];

        Assert.Equal(3, cache.Length);
        Assert.All(cache, t => Assert.DoesNotContain(t.Dependencies, d => d.Target.FullName.StartsWith("Shopfolio.Modules.", StringComparison.Ordinal)));
    }

    [Fact]
    public void SearchEngineDataPattern_InApplicationAssembly_IsCommonInfrastructureThatReachesNoModuleType()
    {
        IType[] pattern = [.. _application.Value.Types.Where(t => (t.FullName ?? string.Empty).StartsWith("Shopfolio.Infrastructure.SearchEngine.", StringComparison.Ordinal))];

        Assert.Contains(pattern, t => t.FullName == "Shopfolio.Infrastructure.SearchEngine.SearchEngineData");
        Assert.Contains(pattern, t => t.FullName == "Shopfolio.Infrastructure.SearchEngine.SearchEngineClosureMiddleware");
        Assert.All(pattern, t => Assert.DoesNotContain(t.Dependencies, d => d.Target.FullName.StartsWith("Shopfolio.Modules.", StringComparison.Ordinal)));
    }

    [Fact]
    public void OperationRunnerAndTheGate_InApplicationAssembly_AreCommonInfrastructureThatReachNoModuleType()
    {
        IType runner = _application.Value.Types.Single(t => t.FullName == "Shopfolio.Infrastructure.Operations.OperationRunner");

        Assert.DoesNotContain(runner.Dependencies, d => d.Target.FullName.StartsWith("Shopfolio.Modules.", StringComparison.Ordinal));
        Assert.Contains(_application.Value.Types, t => t.FullName == "Shopfolio.Infrastructure.Operations.OperationGate");
        Assert.DoesNotContain(_application.Value.Types, t => t.FullName == "Shopfolio.Modules.Ordering.GatedOrderOperation");
    }

    // --- Common infrastructure: no module type, the one exception is the context class (09 6 item 2, 4) ---

    [Fact]
    public void Infrastructure_InApplicationAssembly_ReachesNoModuleTypeExceptInTheContext()
    {
        Assert.Empty(Evaluate(_application.Value, InfrastructureRule()));
    }

    [Fact]
    public void Infrastructure_InViolationSample_ReportsTheForbiddenTypeAndSparesTheContextClass()
    {
        IReadOnlyList<string> violations = Evaluate(_sample.Value, InfrastructureRule());

        string violation = Assert.Single(violations);
        Assert.Contains("ForbiddenInfrastructure", violation, StringComparison.Ordinal);
        Assert.DoesNotContain("AllowedInfrastructure", violation, StringComparison.Ordinal);
    }

    // --- Skeleton: nine module folders, no tenant, no operator panel (05 3.3.1; 10 KD-21) ---

    [Fact]
    public void ModuleFolders_InSource_AreTheNineModulesEachWithContracts()
    {
        string modules = Path.Combine(RepositoryRoot.Find(), "src", "Shopfolio.Web", "Modules");

        Assert.Empty(FindModuleFolderProblems(modules));
    }

    [Fact]
    public void ModuleFolders_InViolationSample_ReportTheMissingTheExtraTheEmptyAndTheContractsLessFolder()
    {
        string root = Path.Combine(RepositoryRoot.Find(), "tests", "Shopfolio.UnitTests", "Architecture", "ViolationSamples", "ModuleFolders");

        List<string> problems = FindModuleFolderProblems(root);

        Assert.Contains(problems, p => p.Contains("missing Settings", StringComparison.Ordinal));
        Assert.Contains(problems, p => p.Contains("unexpected Extra", StringComparison.Ordinal));
        Assert.Contains(problems, p => p.Contains("Catalog has no Contracts source", StringComparison.Ordinal));
        Assert.Contains(problems, p => p.Contains("Payment has no Contracts folder", StringComparison.Ordinal));
        Assert.DoesNotContain(problems, p => p.Contains("Ordering", StringComparison.Ordinal));
        Assert.Equal(4, problems.Count);
    }

    [Fact]
    public void Application_HasNoTenantAndNoOperatorPanel()
    {
        Assert.Empty(FindTenantAndOperatorNames(typeof(Program).Assembly));
    }

    [Fact]
    public void TenantAndOperatorScan_InViolationSample_ReportsTheNamespaceTheTypeAndTheProperty()
    {
        List<string> found = FindTenantAndOperatorNames(_sampleAssembly.Value);

        Assert.Equal(
            [
                "Shopfolio.Accounts.Account.TenantId",
                "Shopfolio.Accounts.TenantResolver",
                "Shopfolio.OperatorPanel.Dashboard",
            ],
            found.Order(StringComparer.Ordinal).ToList());
    }

    private static List<string> FindModuleFolderProblems(string modules)
    {
        List<string> problems = [];
        string[] folders = Directory.GetDirectories(modules).Select(d => Path.GetFileName(d)).Order(StringComparer.Ordinal).ToArray();

        problems.AddRange(_theNineModules.Except(folders, StringComparer.Ordinal).Select(m => $"missing {m}"));
        problems.AddRange(folders.Except(_theNineModules, StringComparer.Ordinal).Select(f => $"unexpected {f}"));
        foreach (string folder in folders.Intersect(_theNineModules, StringComparer.Ordinal))
        {
            string contracts = Path.Combine(modules, folder, "Contracts");
            if (!Directory.Exists(contracts))
            {
                problems.Add($"{folder} has no Contracts folder");
            }
            else if (Directory.GetFiles(contracts, "*.cs").Length == 0)
            {
                problems.Add($"{folder} has no Contracts source");
            }
        }

        return problems;
    }

    private static List<string> FindTenantAndOperatorNames(System.Reflection.Assembly assembly)
    {
        Regex forbidden = new("Tenant|Operator", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        List<string> found = [];
        foreach (System.Type type in assembly.GetTypes())
        {
            if (type.Namespace is not null && forbidden.IsMatch(type.Namespace) || forbidden.IsMatch(type.Name))
            {
                found.Add(type.FullName ?? type.Name);
            }

            foreach (System.Reflection.PropertyInfo property in type.GetProperties(
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static
                | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.DeclaredOnly))
            {
                if (forbidden.IsMatch(property.Name))
                {
                    found.Add($"{type.FullName}.{property.Name}");
                }
            }
        }

        return found;
    }

    private static ArchRule<IType> NotPublicOutsideContractsRule()
    {
        return Types().That().ResideInNamespaceMatching(AnyModuleInternals)
            .Should().NotBePublic()
            .WithoutRequiringPositiveResults();
    }

    private static ArchRule<IType> ContractsReachNothingRule()
    {
        return Types().That().ResideInNamespaceMatching(AnyModuleContracts)
            .Should().NotDependOnAnyTypesThat().ResideInNamespaceMatching(AnyModuleInternals)
            .WithoutRequiringPositiveResults();
    }

    private static ArchRule<IType> PageRule()
    {
        return Types().That().ResideInNamespaceMatching(PageAndEndpointTypes)
            .Should().NotDependOnAnyTypesThat().ResideInNamespaceMatching(AnyModuleInternals)
            .AndShould().NotDependOnAnyTypesThat().ResideInNamespaceMatching(EntityFramework)
            .AndShould().NotDependOnAnyTypesThat().HaveFullNameMatching(TheContextClass)
            .WithoutRequiringPositiveResults();
    }

    private static ArchRule<IType> InfrastructureRule()
    {
        return Types().That().ResideInNamespaceMatching(InfrastructureTypes).And().DoNotHaveFullNameMatching(TheContextClass)
            .Should().NotDependOnAnyTypesThat().ResideInNamespaceMatching(AnyModuleType)
            .WithoutRequiringPositiveResults();
    }

    private static List<string> Evaluate(ArchUnitNET.Domain.Architecture architecture, ArchRule<IType> rule)
    {
        return rule.Evaluate(architecture).Where(result => !result.Passed).Select(result => result.Description).ToList();
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

    private static List<string> FindModuleReferenceViolations(ArchUnitNET.Domain.Architecture architecture)
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

            violations.AddRange(Evaluate(architecture, rule));
        }

        return violations;
    }
}
