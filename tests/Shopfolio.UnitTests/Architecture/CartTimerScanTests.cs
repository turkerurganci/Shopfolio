using System.Text.RegularExpressions;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Values;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The cart has no timer (T324 criterion 3, KK3; 03 4.1.6; Z-6): no job type names the cart, and no scheduled source, outage effect, nightly step or job
/// handler of the application touches the cart's tables. Every such type is found by reflection, so a new one is scanned without a list to keep; its
/// source file is found by its class declaration. The guest cart's 400-day disposal (06 9.4.3) is a part of the nightly disposal, judged by the
/// retention rules, not a timer of the cart, and is not one of these types.
/// </summary>
public sealed partial class CartTimerScanTests
{
    private static readonly Type[] TimerContracts = [typeof(IDueRecordSource), typeof(IOutageEffect), typeof(INightlyStep), typeof(IJobHandler)];

    [Fact]
    public void NoJobType_NamesTheCart()
    {
        Assert.DoesNotContain(Enum.GetNames<JobType>(), name => name.Contains("Cart", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void NoScheduledSource_OutageEffect_NightlyStep_OrJobHandler_TouchesTheCart_KK3()
    {
        List<Type> timers = typeof(ProductConstants).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && TimerContracts.Any(c => c.IsAssignableFrom(t)))
            .ToList();
        Assert.Contains(typeof(Shopfolio.Modules.Ordering.TransferPaymentExpiry), timers);
        Assert.Contains(typeof(Shopfolio.Modules.Ordering.CardPaymentExpiry), timers);

        string root = RepositoryRoot.Find();
        string[] sources = Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            .ToArray();
        Dictionary<string, string> texts = sources.ToDictionary(f => f, File.ReadAllText, StringComparer.Ordinal);

        List<string> findings = [];
        foreach (Type timer in timers.SelectMany(Hierarchy).Distinct())
        {
            Regex declaration = new($@"\bclass\s+{Regex.Escape(timer.Name.Split('`')[0])}\b", RegexOptions.CultureInvariant);
            string[] files = texts.Where(t => declaration.IsMatch(t.Value)).Select(t => t.Key).ToArray();
            Assert.True(files.Length > 0, $"The source of {timer.FullName} was not found.");
            foreach (string file in files)
            {
                if (CartTable().IsMatch(texts[file]))
                {
                    findings.Add($"{Path.GetRelativePath(root, file)}: {timer.Name}");
                }
            }
        }

        Assert.Empty(findings);
    }

    [Fact]
    public void CartTablePattern_SeesTheCartsSetsAndTypes_AndNotACartIdInAComment()
    {
        Assert.Matches(CartTable(), "db.Carts.Where(c => c.Id == id)");
        Assert.Matches(CartTable(), "db.CartItems.ExecuteDeleteAsync(ct)");
        Assert.Matches(CartTable(), "db.Set<CartItem>()");
        Assert.Matches(CartTable(), "Cart cart = await db.Set<Cart>().SingleAsync(ct);");
        Assert.DoesNotMatch(CartTable(), "order.CartId = null;");
    }

    private static IEnumerable<Type> Hierarchy(Type type)
    {
        for (Type? current = type; current is not null && current.Assembly == type.Assembly; current = current.BaseType)
        {
            yield return current;
        }
    }

    [GeneratedRegex(@"\bCarts\b|\bCartItems\b|\bCartItem\b|<Cart>|\bCart\s+\w+\s*=", RegexOptions.CultureInvariant)]
    private static partial Regex CartTable();
}
