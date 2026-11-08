using System.Text.RegularExpressions;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Values;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The catalog, coupon, content, communication and notification periods start no work of their own (T326 criteria 2, 4-8, KK2, KK4-KK8; 03 4.1.24-4.1.30; Z-21,
/// Z-23-Z-27): the reference price window is a calculation, a coupon and an announcement are judged where they are read, nothing is published on a date, the KVKK
/// reply and the breach notice have no counter, the e-mail retry is the one dispatch schedule, and the attempt limits' windows are only read - no job, source,
/// outage effect, nightly step or job handler of the application names them, and no account row has a lock to open. Every timer type is found by reflection, so a new
/// one is scanned without a list to keep; its source file is found by its class declaration. Z-20 is the one period here that has a timer (the discount bounds,
/// T337), and it is not scanned for.
/// </summary>
public sealed partial class ExpiryTimerScanTests
{
    private static readonly Type[] TimerContracts =
        [typeof(IDueRecordSource), typeof(IOutageEffect), typeof(INightlyStep), typeof(IJobHandler), typeof(ITransactionalJobHandler)];

    private static readonly string[] ExpiryWords =
        ["Coupon", "Announcement", "Reference", "Publish", "Schedule", "Kvkk", "Breach", "Unlock", "Lock", "Retry", "Resend", "Attempt", "Window"];

    [Fact]
    public void NoJobType_NamesACouponAnnouncementReferenceWindowPublishingBreachRetryOrUnlock_KK2_KK4_KK5_KK6_KK7_KK8()
    {
        Assert.DoesNotContain(
            Enum.GetNames<JobType>(),
            name => ExpiryWords.Any(word => name.Contains(word, StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public void NoScheduledSource_OutageEffect_NightlyStep_OrJobHandler_ReadsTheseRulesOrTables_KK2_KK3_KK4_KK5_KK6_KK7_KK8()
    {
        List<Type> timers = typeof(ProductConstants).Assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && TimerContracts.Any(c => c.IsAssignableFrom(t)))
            .ToList();
        Assert.Contains(typeof(Shopfolio.Modules.Catalog.DiscountBoundarySource), timers);
        Assert.Contains(typeof(Shopfolio.Modules.Ordering.TransferPaymentExpiry), timers);
        Assert.Contains(typeof(Shopfolio.Infrastructure.Disposal.DisposalStep), timers);

        string root = RepositoryRoot.Find();
        Dictionary<string, string> texts = SourceFiles(root).ToDictionary(f => f, File.ReadAllText, StringComparer.Ordinal);

        List<string> findings = [];
        foreach (Type timer in timers.SelectMany(Hierarchy).Distinct())
        {
            Regex declaration = new($@"\bclass\s+{Regex.Escape(timer.Name.Split('`')[0])}\b", RegexOptions.CultureInvariant);
            string[] files = texts.Where(t => declaration.IsMatch(t.Value)).Select(t => t.Key).ToArray();
            Assert.True(files.Length > 0, $"The source of {timer.FullName} was not found.");
            foreach (string file in files)
            {
                if (ExpiryUse().IsMatch(texts[file]))
                {
                    findings.Add($"{Path.GetRelativePath(root, file)}: {timer.Name}");
                }
            }
        }

        Assert.Empty(findings);
    }

    [Fact]
    public void NothingIsPublishedOnADate_ThereIsNoPublishMomentOnAProductVariantOrContentRecord_KK5()
    {
        string root = RepositoryRoot.Find();
        string[] publishing = SourceFiles(root)
            .Where(f => IsUnder(root, f, "Modules", "Catalog") || IsUnder(root, f, "Modules", "CorporateContent"))
            .Where(f => FuturePublish().IsMatch(File.ReadAllText(f)))
            .Select(f => Path.GetRelativePath(root, f))
            .ToArray();

        Assert.Empty(publishing);
    }

    [Fact]
    public void TheKvkkReplyAndTheBreachNotice_HaveNoRecordCounterOrDeadline_KK6()
    {
        string root = RepositoryRoot.Find();
        string[] counters = SourceFiles(root)
            .Where(f => LegalObligationCounter().IsMatch(File.ReadAllText(f)))
            .Select(f => Path.GetRelativePath(root, f))
            .ToArray();

        Assert.Empty(counters);
    }

    [Fact]
    public void TheEmailRetryTimes_AreReadOnlyByTheDispatchSchedule_AndTheProductConstantsTable_KK7()
    {
        string root = RepositoryRoot.Find();
        string[] allowed =
        [
            Path.Combine(root, "src", "Shopfolio.Web", "Infrastructure", "ParameterInventory.cs"),
            Path.Combine(root, "src", "Shopfolio.Web", "Infrastructure", "ProductConstants.cs"),
            Path.Combine(root, "src", "Shopfolio.Web", "Modules", "Notifications", "DispatchRetrySchedule.cs"),
        ];

        string[] users = SourceFiles(root).Where(f => RetryTime().IsMatch(File.ReadAllText(f))).ToArray();

        Assert.Equal(allowed.Order(StringComparer.Ordinal), users.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void TheAttemptRecords_AreWrittenAndReadOnlyByTheCounter_AndDeletedOnlyByTheRetentionPart_KK8()
    {
        string root = RepositoryRoot.Find();
        string[] allowed =
        [
            Path.Combine(root, "src", "Shopfolio.Web", "Infrastructure", "AppDbContext.cs"),
            Path.Combine(root, "src", "Shopfolio.Web", "Infrastructure", "Disposal", "TemporaryRecordParts.cs"),
            Path.Combine(root, "src", "Shopfolio.Web", "Infrastructure", "Security", "AttemptCounter.cs"),
        ];

        string[] users = SourceFiles(root).Where(f => AttemptTable().IsMatch(File.ReadAllText(f))).ToArray();

        Assert.Equal(allowed.Order(StringComparer.Ordinal), users.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void NoAccountRowCarriesALock_ABlockOnAnAttemptLimitIsNotStateOnTheAccount_KK8()
    {
        string root = RepositoryRoot.Find();
        string[] accounts =
        [
            Path.Combine(root, "src", "Shopfolio.Web", "Modules", "Membership", "Customer.cs"),
            Path.Combine(root, "src", "Shopfolio.Web", "Modules", "Administration", "Admin.cs"),
        ];

        foreach (string file in accounts)
        {
            Assert.True(File.Exists(file), $"{file} was not found.");
            Assert.DoesNotMatch(AccountLockMember(), File.ReadAllText(file));
        }
    }

    [Fact]
    public void ExpiryPattern_SeesTheRulesTheTablesAndTheRetryTimes_AndNotAPlainWord()
    {
        Assert.Matches(ExpiryUse(), "CouponApplication application = CouponRules.Evaluate(terms, lines, now);");
        Assert.Matches(ExpiryUse(), "var rows = await db.Coupons.Where(c => c.EndDate < today).ToListAsync(ct);");
        Assert.Matches(ExpiryUse(), "ContentVisibility.Decide(announcement, audience, today)");
        Assert.Matches(ExpiryUse(), "Announcement announcement = await db.Set<Announcement>().SingleAsync(ct);");
        Assert.Matches(ExpiryUse(), "decimal? reference = ReferencePriceRules.Reference(history, startedAt);");
        Assert.Matches(ExpiryUse(), "await db.AttemptRecords.Where(a => a.CreatedAt < from).ExecuteDeleteAsync(ct);");
        Assert.Matches(ExpiryUse(), "TimeSpan.FromMinutes(ProductConstants.DispatchFirstRetryMinutes)");
        Assert.DoesNotMatch(ExpiryUse(), "DiscountRules.OwedBounds(terms, since, variant.Price, variant.LastAt, now)");
        Assert.DoesNotMatch(ExpiryUse(), "// a coupon and an announcement are not timers");

        Assert.Matches(FuturePublish(), "public DateTimeOffset? PublishAt { get; set; }");
        Assert.Matches(FuturePublish(), "public DateOnly ScheduledPublishDate { get; set; }");
        Assert.DoesNotMatch(FuturePublish(), "public PublishStatus PublishStatus { get; set; }");
        Assert.DoesNotMatch(FuturePublish(), "// the product publishes at once");

        Assert.Matches(LegalObligationCounter(), "class KvkkRequest { public DateTimeOffset DueAt { get; set; } }");
        Assert.Matches(LegalObligationCounter(), "public int BreachNoticeHoursLeft { get; set; }");
        Assert.DoesNotMatch(LegalObligationCounter(), "string kvkkText = LegalTexts.Kvkk;");

        Assert.Matches(AccountLockMember(), "public DateTimeOffset? LockedUntil { get; set; }");
        Assert.Matches(AccountLockMember(), "public bool IsLockedOut { get; set; }");
        Assert.DoesNotMatch(AccountLockMember(), "public string PasswordDigest { get; set; }");
    }

    private static bool IsUnder(string root, string file, params string[] folders)
    {
        string prefix = Path.Combine([root, "src", "Shopfolio.Web", .. folders]) + Path.DirectorySeparatorChar;
        return file.StartsWith(prefix, StringComparison.Ordinal);
    }

    private static IEnumerable<string> SourceFiles(string root)
    {
        return Directory.EnumerateFiles(Path.Combine(root, "src"), "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !f.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
                && !f.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}", StringComparison.Ordinal));
    }

    private static IEnumerable<Type> Hierarchy(Type type)
    {
        for (Type? current = type; current is not null && current.Assembly == type.Assembly; current = current.BaseType)
        {
            yield return current;
        }
    }

    // What a timer must not read: the coupon rules and tables, the announcement and its visibility, the reference price rules, the attempt tables and counter, and the retry times.
    [GeneratedRegex(
        @"\b(?:CouponRules|Coupons|Coupon|CouponTerms|CouponApplication|ContentVisibility|Announcements|Announcement|ReferencePriceRules|AttemptRecords|AttemptRecord|AttemptCounter|AttemptLimits|DispatchRetrySchedule)\b"
            + @"|\bDispatch(?:First|Second|Third)RetryMinutes\b",
        RegexOptions.CultureInvariant)]
    private static partial Regex ExpiryUse();

    // A moment a product, a variant or a content record is published at: there is none (Z-24).
    [GeneratedRegex(@"\b\w*(?:PublishAt|PublishOn|PublishDate|PublishFrom|PublishAfter|PublishStart|ScheduledPublish)\w*\b", RegexOptions.CultureInvariant)]
    private static partial Regex FuturePublish();

    // A record, a counter or a deadline of the KVKK reply (Z-25) or of the breach notice to the Board (Z-37).
    [GeneratedRegex(
        @"\b(?:class|record|enum)\s+\w*(?:KvkkRequest|KvkkApplication|DataSubjectRequest|BreachNotice|BreachReport|BreachCase)\w*"
            + @"|\b(?:Kvkk|Breach)\w*(?:Deadline|DueAt|Counter|DaysLeft|HoursLeft)\b",
        RegexOptions.CultureInvariant)]
    private static partial Regex LegalObligationCounter();

    [GeneratedRegex(@"\bDispatch(?:First|Second|Third)RetryMinutes\b|\bRetriesFromFirstAttempt\b", RegexOptions.CultureInvariant)]
    private static partial Regex RetryTime();

    [GeneratedRegex(@"\bAttemptRecords\b", RegexOptions.CultureInvariant)]
    private static partial Regex AttemptTable();

    // A state on an account row that says it is closed because of attempts: a lock, a lockout, a block, a suspension.
    [GeneratedRegex(@"\b\w*(?:Lock|Locked|LockedOut|Lockout|Blocked|BlockedUntil|Suspended)\w*\s*\{\s*get", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex AccountLockMember();
}
