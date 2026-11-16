namespace Shopfolio.IntegrationTests.Browser;

/// <summary>The mask of the observation record (T551 criterion 3; 12 7.1: personal data masked, no access key or token in the evidence). No browser is needed, so the test runs with the plain Integration set.</summary>
public sealed class ObservationMaskTests
{
    private const string Key = "AbCdEf0123456789xyzKLMN";
    private const string Token = "CfDJ8N-pending_0123456789abcdef";

    [Fact]
    public void Mask_HidesTheEmail_TheAccessKey_AndThePendingRegistrationToken_InOneMixedText()
    {
        string text = $"adres /kayit/sonuc?pendingRegistration={Token}&x=1; e-posta b110-wen11@example.com; sayfa /siparis/erisim/{Key}/iptal ve /siparis/erisim/{Key}";

        string masked = DataRecord.Mask(text);

        Assert.DoesNotContain(Key, masked, StringComparison.Ordinal);
        Assert.DoesNotContain(Token, masked, StringComparison.Ordinal);
        Assert.DoesNotContain("example.com", masked, StringComparison.Ordinal);
        Assert.Contains("/siparis/erisim/***/iptal", masked, StringComparison.Ordinal);
        Assert.Contains("pendingRegistration=***&x=1", masked, StringComparison.Ordinal);
    }

    [Fact]
    public void Mask_KeepsTheTextAroundTheMaskedValues_SoTheEvidenceStaysReadable()
    {
        string masked = DataRecord.Mask("sonuç \"Doğrulama bağlantısı gönderildi\"; adres /giris");

        Assert.Equal("sonuç \"Doğrulama bağlantısı gönderildi\"; adres /giris", masked);
    }

    [Fact]
    public void TheRecord_KeepsOnlyTheMaskedObservedText()
    {
        string directory = Directory.CreateTempSubdirectory("t551-mask-").FullName;
        try
        {
            ObservationRecord record = new(directory);
            record.Add(new Observation("VAL-B110", "betikli", "chromium", 1280, "adım", "beklenen", DataRecord.Mask($"/siparis/erisim/{Key}; a@example.com"), true, null));

            string kept = Assert.Single(record.Snapshot()).Observed;

            Assert.DoesNotContain(Key, kept, StringComparison.Ordinal);
            Assert.DoesNotContain("@example.com", kept, StringComparison.Ordinal);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
