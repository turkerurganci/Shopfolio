using Shopfolio.Infrastructure.Notices;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Pages.Panel;

namespace Shopfolio.UnitTests.Pages;

/// <summary>
/// The legal texts screen's fixed tables (T507; 04 9.19.3; 07 6.3.1): the completion line names every identity field the identity can lack, and the short-lived notice texts are the sentences
/// of the table and only for the codes that have one.
/// </summary>
public sealed class LegalTextsModelTests
{
    public static TheoryData<CompanyType> Types => new() { CompanyType.Tradesman, CompanyType.NaturalPersonMerchant, CompanyType.LegalEntity };

    [Theory]
    [MemberData(nameof(Types))]
    public void EveryRequiredIdentityField_HasATurkishLabel_SoTheCompletionLineNeverPrintsACodeName(CompanyType type)
    {
        IEnumerable<string> unlabelled = CompanyIdentityRules.RequiredFields(type).Except(LegalTextsModel.LabelledIdentityFields, StringComparer.Ordinal);

        Assert.Empty(unlabelled);
    }

    [Fact]
    public void TheLabelledFields_AreTheFieldsSomeTypeRequires_AndTheTypeItself()
    {
        string[] required = [.. new[] { CompanyType.Tradesman, CompanyType.NaturalPersonMerchant, CompanyType.LegalEntity }.SelectMany(type => CompanyIdentityRules.RequiredFields(type)).Distinct(StringComparer.Ordinal)];

        // The type itself is not a required field but is the first thing the completion line lists when it is not chosen (CompanyIdentityRules.MissingFields).
        Assert.Empty(LegalTextsModel.LabelledIdentityFields.Except([.. required, "CompanyType"], StringComparer.Ordinal));
    }

    [Theory]
    [InlineData(NoticeCode.Saved, "Kaydedildi")]
    [InlineData(NoticeCode.CartItemAdded, "Sepete eklendi")]
    [InlineData(NoticeCode.InvitationSent, "Davet gönderildi")]
    public void AShortLivedNoticeWithASentenceInTheTable_HasThatText(NoticeCode code, string text)
    {
        Assert.True(NoticeTexts.TryShortLived(code, out string? found));
        Assert.Equal(text, found);
    }

    [Fact]
    public void ACodeWithoutASentence_ShowsNothing_AndEveryCodeWithOneIsShortLived()
    {
        foreach (NoticeCode code in Enum.GetValues<NoticeCode>())
        {
            bool hasText = NoticeTexts.TryShortLived(code, out string? text);

            Assert.Equal(hasText, text is not null);
            if (hasText)
            {
                Assert.Equal(NoticeKind.ShortLived, NoticeKinds.Of(code));
            }
        }

        Assert.False(NoticeTexts.TryShortLived(NoticeCode.ContactRequestClosed, out _));
        Assert.False(NoticeTexts.TryShortLived(NoticeCode.ContactRequestReopened, out _));
    }
}
