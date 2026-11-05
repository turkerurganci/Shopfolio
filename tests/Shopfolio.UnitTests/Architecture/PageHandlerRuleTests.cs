using System.Reflection;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The page handler rule (T206; 07 2.2, 2.2.1; 09 5.3 item 2): every POST of a page is a named handler, so a page model has no unnamed
/// <c>OnPost</c> or <c>OnPostAsync</c> and no verb handler other than GET and POST. The rule runs over the application assembly and is shown to fail on a
/// deliberate sample class in the violation sample assembly (09 19.2 item 3). The application has no controller either (07 2.10.1).
/// </summary>
public sealed class PageHandlerRuleTests
{
    private static readonly Regex _unnamedPost = new(@"^OnPost(Async)?$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));

    private static readonly Regex _otherVerb = new(@"^On(Put|Patch|Delete|Head|Options|Trace|Connect)(Async)?($|(?=[A-Z]))", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));

    private static readonly Lazy<Assembly> _sample =
        new(() => Assembly.LoadFrom(Path.Combine(AppContext.BaseDirectory, "ModuleBoundaryViolationSample.dll")));

    [Fact]
    public void PageModels_InApplicationAssembly_HaveNoUnnamedPostAndNoVerbOtherThanGetAndPost()
    {
        Assert.Empty(Violations(typeof(Program).Assembly));
    }

    [Fact]
    public void PageModels_InViolationSample_ReportTheUnnamedPostsAndTheOtherVerbsButNotTheNamedHandlers()
    {
        List<string> violations = Violations(_sample.Value);

        Assert.Contains("UnnamedPost.OnPost", violations);
        Assert.Contains("UnnamedPostAsync.OnPostAsync", violations);
        Assert.Contains("OtherVerbs.OnPut", violations);
        Assert.Contains("OtherVerbs.OnDeleteAsync", violations);
        Assert.Contains("OtherVerbs.OnPatchRename", violations);
        Assert.DoesNotContain(violations, v => v.StartsWith("AllowedHandlers.", StringComparison.Ordinal));
        Assert.Equal(5, violations.Count);
    }

    [Fact]
    public void Application_HasNoControllerType()
    {
        Assert.Empty(typeof(Program).Assembly.GetTypes().Where(t => typeof(ControllerBase).IsAssignableFrom(t)).Select(t => t.FullName));
    }

    private static List<string> Violations(Assembly assembly)
    {
        List<string> violations = [];
        foreach (Type type in assembly.GetTypes().Where(t => typeof(PageModel).IsAssignableFrom(t)))
        {
            foreach (MethodInfo method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                if (_unnamedPost.IsMatch(method.Name) || _otherVerb.IsMatch(method.Name))
                {
                    violations.Add($"{type.Name}.{method.Name}");
                }
            }
        }

        return violations;
    }
}
