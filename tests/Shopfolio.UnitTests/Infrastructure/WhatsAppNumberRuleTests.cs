using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.CorporateContent;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>The WhatsApp number's one rule (D-1332; 02 3.27.12; 06 2.5.1): the shared rule, and the social links' own check that asks it.</summary>
public sealed class WhatsAppNumberRuleTests
{
    [Theory]
    [InlineData("0 (532) 111 22 33", "5321112233", null)]
    [InlineData("+90 532 111 22 33", "5321112233", null)]
    [InlineData("5321112233", "5321112233", null)]
    [InlineData("", null, null)]
    [InlineData("   ", null, null)]
    [InlineData(null, null, null)]
    [InlineData("12345", "12345", FieldCodes.InvalidPhone)]
    [InlineData("whatsapp numaram", "whatsapp numaram", FieldCodes.InvalidPhone)]
    public void TheRule_StoresTenDigits_AcceptsEmpty_AndRefusesAnythingElse_D1332(string? typed, string? stored, string? code)
    {
        Assert.Equal((stored, code), WhatsAppNumberRule.Check(typed));
    }

    [Theory]
    [InlineData("0 (532) 111 22 33")]
    [InlineData("")]
    [InlineData("12345")]
    [InlineData("whatsapp numaram")]
    public void TheSocialLinksCheck_AnswersWhatTheSharedRuleAnswers_D1332(string typed)
    {
        (string? stored, string? code) = WhatsAppNumberRule.Check(typed);

        (string? linkStored, FieldRejection? rejection) = SocialLinks.CheckWhatsAppNumber("WhatsAppNumber", typed);

        Assert.Equal(stored, linkStored);
        Assert.Equal(code, rejection?.Code);
        Assert.Equal(code is null ? null : "WhatsAppNumber", rejection?.Field);
    }
}
