using Shopfolio.Infrastructure;
using Shopfolio.Pages.Panel;

namespace Shopfolio.UnitTests.Pages;

/// <summary>The small rules of the panel access texts (T520 KK3, KK6, KK8; 04 9.1, 9.2; K-779, K-799): the one wrong-credentials sentence, the invalid-link sentences that tell no reason, the password hint.</summary>
public sealed class PanelAccessTextsTests
{
    [Fact]
    public void TheWrongCredentialsSentence_NamesNeitherTheFieldNorTheAccount_KK3()
    {
        string text = PanelAccessTexts.InvalidCredentials;

        Assert.DoesNotContain("kayıtlı değil", text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("bulunamadı", text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("E-posta adresi ya da şifre", text, StringComparison.Ordinal);
    }

    [Fact]
    public void TheInvalidLinkSentences_TellNoReason_KK6_KK8()
    {
        foreach (string text in new[] { PanelAccessTexts.ResetLinkInvalid, PanelAccessTexts.InvitationInvalid })
        {
            Assert.EndsWith(".", text, StringComparison.Ordinal);
            Assert.DoesNotContain("süresi doldu", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("kullanıldı", text, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("iptal", text, StringComparison.OrdinalIgnoreCase);
        }
    }

    [Fact]
    public void ThePasswordHint_TakesTheMinimumFromTheConstant_KK7_KK8()
    {
        string expected = $"En az {DisplayFormat.Number(ProductConstants.PasswordMinLength)} karakter olmalı.";

        Assert.Equal(expected, PanelAccessTexts.PasswordHint());
    }
}
