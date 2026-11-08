using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;

namespace Shopfolio.UnitTests.CorporateContent;

/// <summary>
/// The publish gate of the corporate content (T322 criteria 1-2; KK1, KK2; 02 3.27.13, 5.2; 03 1.8.4): the seven record kinds, the required fields of
/// each, and the two statuses without a third one or a version table.
/// </summary>
public sealed class ContentGateTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 7, 9, 0, 0, TimeSpan.Zero);

    private static readonly Guid SomeProvince = Guid.Parse("018f0000-0000-7000-8000-0000000000c3");

    public static TheoryData<string> Kinds => [.. Enum.GetNames<ContentRecordKind>()];

    [Fact]
    public void TheKinds_AreExactlyTheRecordsThatImplementTheContentRecordInterface_KK1()
    {
        Type[] implementers =
        [
            .. typeof(ContentRecordKind).Assembly.GetTypes()
                .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IContentRecord).IsAssignableFrom(t)),
        ];

        Assert.Equal(Enum.GetValues<ContentRecordKind>().Length, implementers.Length);
        foreach (ContentRecordKind kind in Enum.GetValues<ContentRecordKind>())
        {
            Assert.Single(implementers, t => Complete(kind).GetType() == t);
        }

        Assert.Equal(implementers.Length, Enum.GetValues<ContentRecordKind>().Select(k => Complete(k).GetType()).Distinct().Count());
    }

    [Fact]
    public void TheGate_IsTheTableOfTheRequirementsDocument_PerKind_KK2()
    {
        // 02 3.27.13: service introduction - name; reference work and general page - title; question - question and answer; branch - name and address
        // (province, district, open address); about page - short introduction; announcement - text. The picture is required nowhere.
        Assert.Equal([ContentGateField.ShortIntroduction], ContentGate.RequiredFields(ContentRecordKind.AboutPage));
        Assert.Equal([ContentGateField.Name], ContentGate.RequiredFields(ContentRecordKind.Offering));
        Assert.Equal([ContentGateField.Title], ContentGate.RequiredFields(ContentRecordKind.PortfolioItem));
        Assert.Equal([ContentGateField.Question, ContentGateField.Answer], ContentGate.RequiredFields(ContentRecordKind.FaqItem));
        Assert.Equal(
            [ContentGateField.Name, ContentGateField.Province, ContentGateField.District, ContentGateField.AddressLine],
            ContentGate.RequiredFields(ContentRecordKind.Branch));
        Assert.Equal([ContentGateField.Title], ContentGate.RequiredFields(ContentRecordKind.CustomPage));
        Assert.Equal([ContentGateField.Text], ContentGate.RequiredFields(ContentRecordKind.Announcement));
    }

    [Theory]
    [MemberData(nameof(Kinds))]
    public void ACompleteRecord_HasNoMissingField_KK2(string kindName)
    {
        ContentRecordKind kind = Enum.Parse<ContentRecordKind>(kindName);
        IContentRecord record = Complete(kind);

        Assert.Empty(record.MissingGateFields());
        Assert.Equal(kind, record.Kind);
    }

    [Theory]
    [MemberData(nameof(Kinds))]
    public void EmptyingEachRequiredField_ReportsExactlyThatField_AndNoOther_KK2(string kindName)
    {
        ContentRecordKind kind = Enum.Parse<ContentRecordKind>(kindName);
        foreach (ContentGateField field in ContentGate.RequiredFields(kind))
        {
            IContentRecord record = Complete(kind);

            Empty(record, field);

            Assert.Equal([field], record.MissingGateFields());
        }
    }

    [Theory]
    [MemberData(nameof(Kinds))]
    public void ABlankRecord_ReportsEveryRequiredField_InTheOrderOfTheTable_KK2(string kindName)
    {
        ContentRecordKind kind = Enum.Parse<ContentRecordKind>(kindName);
        IContentRecord record = Complete(kind);
        foreach (ContentGateField field in ContentGate.RequiredFields(kind))
        {
            Empty(record, field);
        }

        Assert.Equal(ContentGate.RequiredFields(kind), record.MissingGateFields());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t\n")]
    public void WhiteSpaceOnly_IsEmpty_KK2(string? text)
    {
        Assert.False(ContentGate.IsFilled(text));
        Assert.True(ContentGate.IsFilled(" a "));
    }

    [Theory]
    [MemberData(nameof(Kinds))]
    public void ARecordIsBornADraft_AndTheContentOnlyTakesDraftOrPublished_KK1(string kindName)
    {
        IContentRecord record = Created(Enum.Parse<ContentRecordKind>(kindName));

        Assert.Equal(PublishStatus.Draft, record.PublishStatus);
        Assert.Equal(1, record.Version);
    }

    [Fact]
    public void TheContentTables_HaveNoVersionHistoryRevisionOrArchiveTable_KK1()
    {
        string[] names =
        [
            .. typeof(AppDbContext).GetProperties()
                .Where(p => p.PropertyType.IsGenericType && p.PropertyType.Name.StartsWith("DbSet", StringComparison.Ordinal))
                .Select(p => p.PropertyType.GetGenericArguments()[0])
                .Where(t => t.Namespace == typeof(ContentRecordKind).Namespace)
                .Select(t => t.Name),
        ];

        Assert.NotEmpty(names);
        Assert.DoesNotContain(names, n => n.Contains("History", StringComparison.Ordinal) || n.Contains("Revision", StringComparison.Ordinal)
            || n.Contains("Archive", StringComparison.Ordinal) || n.Contains("Snapshot", StringComparison.Ordinal) || n.EndsWith("Version", StringComparison.Ordinal));
    }

    [Fact]
    public void AGateFieldTheKindDoesNotHave_IsRefused_NotSilentlyPassed()
    {
        Assert.Throws<InvalidOperationException>(() => ContentGate.Missing(ContentRecordKind.Offering, field => ContentGate.Unreadable(ContentRecordKind.Offering, field)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ContentGate.RequiredFields((ContentRecordKind)999));
    }

    private static IContentRecord Created(ContentRecordKind kind)
    {
        return kind switch
        {
            ContentRecordKind.AboutPage => AboutPage.Create(Now),
            ContentRecordKind.Offering => Offering.Create("Bakım", "bakim", 1, Now),
            ContentRecordKind.PortfolioItem => PortfolioItem.Create("Proje", "proje", 1, Now),
            ContentRecordKind.FaqItem => FaqItem.Create("Soru?", 1, Now),
            ContentRecordKind.Branch => Branch.Create("Merkez", 1, Now),
            ContentRecordKind.CustomPage => CustomPage.Create("Sayfa", "sayfa", 1, Now),
            ContentRecordKind.Announcement => Announcement.Create(Now),
            _ => throw new ArgumentOutOfRangeException(nameof(kind)),
        };
    }

    private static IContentRecord Complete(ContentRecordKind kind)
    {
        IContentRecord record = Created(kind);
        switch (record)
        {
            case AboutPage about:
                about.ShortIntroduction = "Kısa tanıtım";
                break;
            case FaqItem faq:
                faq.SetAnswer("<p>Cevap</p>");
                break;
            case Branch branch:
                branch.ProvinceId = SomeProvince;
                branch.District = "Çankaya";
                branch.AddressLine = "Atatürk Bulvarı 1";
                break;
            case Announcement announcement:
                announcement.Text = "Bayram tatili";
                break;
        }

        return record;
    }

    private static void Empty(IContentRecord record, ContentGateField field)
    {
        switch (record, field)
        {
            case (AboutPage about, ContentGateField.ShortIntroduction):
                about.ShortIntroduction = "  ";
                break;
            case (Offering offering, ContentGateField.Name):
                offering.Name = string.Empty;
                break;
            case (PortfolioItem work, ContentGateField.Title):
                work.Title = string.Empty;
                break;
            case (FaqItem faq, ContentGateField.Question):
                faq.Question = " ";
                break;
            case (FaqItem faq, ContentGateField.Answer):
                faq.SetAnswer(null);
                break;
            case (Branch branch, ContentGateField.Name):
                branch.Name = string.Empty;
                break;
            case (Branch branch, ContentGateField.Province):
                branch.ProvinceId = null;
                break;
            case (Branch branch, ContentGateField.District):
                branch.District = string.Empty;
                break;
            case (Branch branch, ContentGateField.AddressLine):
                branch.AddressLine = null;
                break;
            case (CustomPage page, ContentGateField.Title):
                page.Title = string.Empty;
                break;
            case (Announcement announcement, ContentGateField.Text):
                announcement.Text = null;
                break;
            default:
                throw new InvalidOperationException($"{record.Kind} has no gate field {field}.");
        }
    }
}
