using System.Text.RegularExpressions;

using ArchUnitNET.Domain;
using ArchUnitNET.Domain.Dependencies;
using ArchUnitNET.Loader;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The time and money rules as architecture tests over the compiled code (T227; 09 7.1 items 1, 3, 5; 14.1 items 1, 5; 05 7.2.4, 7.2.5,
/// 7.2.6). ArchUnitNET sees method calls in the IL (a property read is its getter call), so the rule holds whatever the source looks like:
/// <list type="bullet">
/// <item>The application reads no clock but <see cref="TimeProvider"/>: no <c>DateTime.Now</c>, <c>DateTime.UtcNow</c>, <c>DateTime.Today</c>,
/// <c>DateTimeOffset.Now</c>, <c>DateTimeOffset.UtcNow</c>, no <c>TimeZoneInfo</c> member (<c>Local</c>, the time zone database), and none
/// of the conversions that silently use the server's time zone (<c>ToLocalTime</c>, <c>DateTime.ToUniversalTime</c>,
/// <c>DateTimeOffset.LocalDateTime</c>, a <c>DateTimeOffset</c> made from a <c>DateTime</c>, <c>TimeProvider.GetLocalNow</c>,
/// <c>TimeProvider.LocalTimeZone</c>).</item>
/// <item><c>TimeProvider.System</c> is named only by the registration class <c>Infrastructure.TimeSource</c>.</item>
/// <item>No field, property, parameter or return is a <c>DateTime</c>: an instant is a <c>DateTimeOffset</c> (read by reflection).</item>
/// <item><c>Math</c>, <c>MathF</c>, <c>decimal</c>, <c>double</c> and <c>float</c> rounding (<c>Round</c>, <c>Floor</c>, <c>Ceiling</c>,
/// <c>Truncate</c>) and any call that takes a <c>MidpointRounding</c> is made only in <c>Infrastructure.Money</c>.</item>
/// <item>No module type and not <c>Money</c> depends on <c>double</c> or <c>float</c>.</item>
/// </list>
/// The exemptions are classes by their full name (and their compiler-made nested types), not namespaces. Each rule is shown to fail on the
/// deliberate violation sample compiled into its own assembly, <c>TimeAndMoneyViolationSample.dll</c> (09 19.2 item 3).
/// </summary>
public sealed partial class TimeAndMoneyRulesTests
{
    private const string TimeSourceClass = "Shopfolio.Infrastructure.TimeSource";

    private const string MoneyClass = "Shopfolio.Infrastructure.Money";

    private static readonly Lazy<ArchUnitNET.Domain.Architecture> _application =
        new(() => new ArchLoader().LoadAssemblies(typeof(Program).Assembly).Build());

    private static readonly Lazy<ArchUnitNET.Domain.Architecture> _sample =
        new(() => new ArchLoader().LoadAssemblies(
            System.Reflection.Assembly.LoadFrom(Path.Combine(AppContext.BaseDirectory, "TimeAndMoneyViolationSample.dll"))).Build());

    // --- One time source (09 7.1 items 1, 3, 5) ---

    [Fact]
    public void Application_ReadsTimeOnlyThroughTimeProvider()
    {
        Assert.Empty(ForbiddenTimeCalls(_application.Value));
    }

    [Theory]
    [InlineData("System.DateTime::get_Now()")]
    [InlineData("System.DateTime::get_UtcNow()")]
    [InlineData("System.DateTime::get_Today()")]
    [InlineData("System.DateTimeOffset::get_Now()")]
    [InlineData("System.DateTimeOffset::get_UtcNow()")]
    [InlineData("System.TimeZoneInfo::get_Local()")]
    [InlineData("System.TimeZoneInfo::FindSystemTimeZoneById(System.String)")]
    [InlineData("System.DateTime::ToLocalTime()")]
    [InlineData("System.DateTime::ToUniversalTime()")]
    [InlineData("System.DateTimeOffset::ToLocalTime()")]
    [InlineData("System.DateTimeOffset::get_LocalDateTime()")]
    [InlineData("System.DateTimeOffset::.ctor(System.DateTime)")]
    [InlineData("System.DateTimeOffset::op_Implicit(System.DateTime)")]
    [InlineData("System.TimeProvider::GetLocalNow()")]
    [InlineData("System.TimeProvider::get_LocalTimeZone()")]
    public void ForbiddenTimeCalls_OnViolationSample_CatchTheRead(string member)
    {
        Assert.Contains(ForbiddenTimeCalls(_sample.Value), f => f.Target.EndsWith(" " + member, StringComparison.Ordinal));
    }

    [Fact]
    public void ForbiddenTimeCalls_OnViolationSample_CatchReadsInLambdaAsyncMethodAndFieldInitializer()
    {
        IReadOnlyList<(string Caller, string Member, string Target)> findings = ForbiddenTimeCalls(_sample.Value);

        // ArchUnitNET gives the body of a lambda and of an async method's state machine to the method that wrote it.
        const string Sample = "Shopfolio.Modules.Sample.TimeViolations";
        Assert.Contains((Sample, "InLambda()", "System.DateTime System.DateTime::get_UtcNow()"), findings);
        Assert.Contains((Sample, "InAsyncMethod()", "System.DateTimeOffset System.DateTimeOffset::get_UtcNow()"), findings);
        Assert.Contains((Sample, ".cctor()", "System.DateTime System.DateTime::get_UtcNow()"), findings);
    }

    // --- An instant is a DateTimeOffset, never a DateTime (09 7.1 item 2; 05 7.2.1) ---

    [Fact]
    public void Application_DeclaresNoDateTimeFieldPropertyParameterOrReturn()
    {
        Assert.Empty(DateTimeSignatures(typeof(Program).Assembly));
    }

    [Fact]
    public void DateTimeSignatures_OnViolationSample_CatchEachPlace()
    {
        IReadOnlyList<string> findings = DateTimeSignatures(
            System.Reflection.Assembly.LoadFrom(Path.Combine(AppContext.BaseDirectory, "TimeAndMoneyViolationSample.dll")));

        const string Sample = "Shopfolio.Modules.Sample.LocalTimeRecord";
        Assert.Contains($"{Sample}.CreatedAt (property)", findings);
        Assert.Contains($"{Sample}._shippedAt (field)", findings);
        Assert.Contains($"{Sample}.Expire(dueAt) (parameter)", findings);
        Assert.Contains($"{Sample}.DueAt (return)", findings);
        Assert.Contains($"{Sample}..ctor(openedAt) (parameter)", findings);
        Assert.DoesNotContain(findings, f => f.StartsWith($"{Sample}.StoredAt", StringComparison.Ordinal));
    }

    [Fact]
    public void SystemClock_InApplication_IsNamedOnlyByTimeSource()
    {
        Assert.Empty(SystemClockOutsideTimeSource(_application.Value));
        Assert.Contains(_application.Value.Types, t => t.FullName == TimeSourceClass);
    }

    [Fact]
    public void SystemClock_OnViolationSample_IsCaughtOutsideTimeSourceOnly()
    {
        IReadOnlyList<(string Caller, string Member, string Target)> findings = SystemClockOutsideTimeSource(_sample.Value);

        Assert.Contains(findings, f => f.Caller == "Shopfolio.Infrastructure.TimeSourceCopy");
        Assert.Contains(findings, f => f.Caller == "Shopfolio.Modules.Sample.TimeViolations");
        Assert.DoesNotContain(findings, f => f.Caller == TimeSourceClass);
    }

    // --- Rounding only in Money (09 14.1 items 1, 5) ---

    [Fact]
    public void Rounding_InApplication_IsOnlyInMoney()
    {
        Assert.Empty(RoundingOutsideMoney(_application.Value));
    }

    [Theory]
    [InlineData("System.Math::Round(System.Decimal,System.Int32)")]
    [InlineData("System.Math::Round(System.Decimal,System.Int32,System.MidpointRounding)")]
    [InlineData("System.Decimal::Round(System.Decimal,System.Int32)")]
    [InlineData("System.Math::Floor(System.Decimal)")]
    [InlineData("System.Math::Ceiling(System.Decimal)")]
    [InlineData("System.Math::Truncate(System.Decimal)")]
    [InlineData("System.Decimal::Floor(System.Decimal)")]
    [InlineData("System.Double::Round(System.Double,System.Int32,System.MidpointRounding)")]
    public void Rounding_OnViolationSample_IsCaughtOutsideMoney(string member)
    {
        Assert.Contains(RoundingOutsideMoney(_sample.Value), f => f.Target.EndsWith(" " + member, StringComparison.Ordinal));
    }

    [Fact]
    public void Rounding_OnViolationSample_CatchesAnyCallThatTakesMidpointRounding()
    {
        // 09 14.1 item 5: MidpointRounding is not used outside Money - a generic math rounding call is caught by its argument type.
        Assert.Contains(
            RoundingOutsideMoney(_sample.Value),
            f => f.Member.StartsWith("GenericRound", StringComparison.Ordinal)
                && f.Target.Contains("System.MidpointRounding", StringComparison.Ordinal));
    }

    [Fact]
    public void Rounding_OnViolationSample_ExemptsMoneyByItsNameOnly()
    {
        IReadOnlyList<(string Caller, string Member, string Target)> findings = RoundingOutsideMoney(_sample.Value);

        Assert.Contains(findings, f => f.Caller == "Shopfolio.Infrastructure.MoneyHelper");
        Assert.DoesNotContain(findings, f => f.Caller == MoneyClass);
    }

    // --- No floating point money (09 14.1 item 5; 05 7.2.5) ---

    [Fact]
    public void ModulesAndMoney_InApplication_HaveNoFloatingPointType()
    {
        Assert.Empty(FloatingPointUsers(_application.Value));
        Assert.Contains(_application.Value.Types, t => t.FullName == MoneyClass);
    }

    [Fact]
    public void FloatingPointUsers_OnViolationSample_CatchTheModuleType()
    {
        IReadOnlyList<(string Type, string FloatingType)> findings = FloatingPointUsers(_sample.Value);

        Assert.Contains(("Shopfolio.Modules.Sample.FloatingPrice", "System.Double"), findings);
        Assert.Contains(("Shopfolio.Modules.Sample.FloatingPrice", "System.Single"), findings);
        Assert.DoesNotContain(findings, f => f.Type.StartsWith("Shopfolio.Infrastructure.", StringComparison.Ordinal) && f.Type != MoneyClass);
    }

    private static List<(string Caller, string Member, string Target)> ForbiddenTimeCalls(ArchUnitNET.Domain.Architecture architecture)
    {
        return Calls(architecture).Where(c => ForbiddenTimeMember().IsMatch(c.Target)).ToList();
    }

    /// <summary>
    /// Every field, property, method parameter and method return of <see cref="DateTime"/> (or <c>DateTime?</c>) declared in the assembly,
    /// compiler-made members included: a <c>DateTime</c> whose <c>Kind</c> is wrong or unspecified shifts the time zone silently, so an
    /// instant is a <see cref="DateTimeOffset"/> everywhere (09 7.1 item 2). A local inside a method body is not a declaration and is not
    /// listed - <c>TurkeyTime</c> builds its day boundary that way.
    /// </summary>
    private static List<string> DateTimeSignatures(System.Reflection.Assembly assembly)
    {
        const System.Reflection.BindingFlags Declared = System.Reflection.BindingFlags.DeclaredOnly | System.Reflection.BindingFlags.Instance
            | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
        static bool IsDateTime(System.Type type) => (Nullable.GetUnderlyingType(type) ?? type) == typeof(DateTime);

        List<string> findings = [];
        foreach (System.Type type in assembly.GetTypes())
        {
            findings.AddRange(type.GetFields(Declared).Where(f => IsDateTime(f.FieldType)).Select(f => $"{type.FullName}.{f.Name} (field)"));
            findings.AddRange(type.GetProperties(Declared).Where(p => IsDateTime(p.PropertyType))
                .Select(p => $"{type.FullName}.{p.Name} (property)"));

            // Accessors and operators are methods too; constructors take part through their parameters.
            IEnumerable<System.Reflection.MethodBase> methods =
                type.GetMethods(Declared).Concat<System.Reflection.MethodBase>(type.GetConstructors(Declared));
            foreach (System.Reflection.MethodBase method in methods)
            {
                if (method is System.Reflection.MethodInfo info && IsDateTime(info.ReturnType))
                {
                    findings.Add($"{type.FullName}.{method.Name} (return)");
                }

                findings.AddRange(method.GetParameters().Where(p => IsDateTime(p.ParameterType))
                    .Select(p => $"{type.FullName}.{method.Name}({p.Name}) (parameter)"));
            }
        }

        return findings;
    }

    private static List<(string Caller, string Member, string Target)> SystemClockOutsideTimeSource(ArchUnitNET.Domain.Architecture architecture)
    {
        return Calls(architecture).Where(c => SystemClockMember().IsMatch(c.Target) && !IsClassOrNested(c.Caller, TimeSourceClass)).ToList();
    }

    private static List<(string Caller, string Member, string Target)> RoundingOutsideMoney(ArchUnitNET.Domain.Architecture architecture)
    {
        return Calls(architecture).Where(c => RoundingMember().IsMatch(c.Target) && !IsClassOrNested(c.Caller, MoneyClass)).ToList();
    }

    private static List<(string Type, string FloatingType)> FloatingPointUsers(ArchUnitNET.Domain.Architecture architecture)
    {
        return architecture.Types
            .Where(t => ModuleType().IsMatch(t.FullName) || IsClassOrNested(t.FullName, MoneyClass))
            .SelectMany(t => t.Dependencies.Select(d => d.Target.FullName)
                .Where(name => name is "System.Double" or "System.Single")
                .Distinct()
                .Select(name => (t.FullName, name)))
            .ToList();
    }

    /// <summary>
    /// Every method call of every type with the calling member; ArchUnitNET gives a lambda's and an async state machine's calls to the method
    /// that wrote them, and a field initializer's to the constructor.
    /// </summary>
    private static IEnumerable<(string Caller, string Member, string Target)> Calls(ArchUnitNET.Domain.Architecture architecture)
    {
        foreach (IType type in architecture.Types)
        {
            foreach (IMember member in type.Members)
            {
                foreach (MethodCallDependency call in member.MemberDependencies.OfType<MethodCallDependency>())
                {
                    yield return (type.FullName, member.Name, call.TargetMember.FullName);
                }
            }
        }
    }

    private static bool IsClassOrNested(string typeName, string className)
    {
        return typeName == className || typeName.StartsWith(className + "+", StringComparison.Ordinal);
    }

    [GeneratedRegex(
        @" (System\.DateTime::get_(Now|UtcNow|Today)\(\)" +
        @"|System\.DateTimeOffset::get_(Now|UtcNow|LocalDateTime)\(\)" +
        @"|System\.TimeZoneInfo::" +
        @"|System\.DateTime::(ToLocalTime|ToUniversalTime)\(\)" +
        @"|System\.DateTimeOffset::ToLocalTime\(\)" +
        @"|System\.DateTimeOffset::(\.ctor|op_Implicit)\(System\.DateTime\)" +
        @"|System\.TimeProvider::(GetLocalNow|get_LocalTimeZone)\(\))")]
    private static partial Regex ForbiddenTimeMember();

    [GeneratedRegex(@" System\.TimeProvider::get_System\(\)$")]
    private static partial Regex SystemClockMember();

    [GeneratedRegex(@" System\.(Math|MathF|Decimal|Double|Single)::(Round|Floor|Ceiling|Truncate)\(|System\.MidpointRounding\b")]
    private static partial Regex RoundingMember();

    [GeneratedRegex(@"^Shopfolio\.Modules(\.|$)")]
    private static partial Regex ModuleType();
}
