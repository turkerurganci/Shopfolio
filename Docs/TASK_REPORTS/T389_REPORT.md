# T389 — Hesap ve durum yönetim komutları: oturumları sonlandırma, şifreyi geçersiz kılma, durum

**Faz:** F3 | **Durum:** ⏳ Yapım tamamlandı, doğrulama bekliyor | **Tarih:** 2026-11-08

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- `Commands/AccountTargetFlow` + `EndSessionsCommand` + `InvalidatePasswordCommand`: yönlendirilmiş girdide hiçbir şey okumadan 2; e-posta yalnız terminalden; sıra e-posta → `IAdministrationModule.FindAdminIdAsync` → kayıt satırı (`IAuditLineWriter`, işlemden önce) → `ITransactionRunner` içinde iş. Hesap yoksa satır `AuditLine.AdminCommandWithoutTarget` (boş kimlik) ve çıkış 3. Seçenek yok.
- Administration: `FindAdminIdAsync`, `EndAdminSessionsAsync`, `InvalidateAdminPasswordAsync` (`Contracts/AdminAccountWorkResult`); sıra hesap kilidi → özet boşaltma (`AdminPasswords.InvalidatePasswordAsync`, `PasswordHash = null`) → oturumlar; işlem dışında çağrılırsa `RequireTransaction`.
- `Commands/StatusCommand`: sürüm; `StatusEvaluator.StandingsAsync` ile bütün koşullar (açık / kapalı / kaynak okunamadı / ölçülemez); `JobQueueStatusReader` (başarısız ve zamanı gelmiş satırlar); sağlayıcı anahtarı iki sayısı; `IDataConversion` kalanları. Kendi satırı hedefsiz (boş kimlik). Hiçbir veri yazmaz.
- Sayım yöntemleri: `IOrderingModule.CountCardOrdersAwaitingPaymentAsync` + `ListPaidCardItemsAsync`, `IPaymentModule.CountOpenCardRefundsAsync`, `IAfterSalesModule.CountCardOrdersWithRefundExposureAsync` (Ordering AfterSales'i kurucuya alamaz: AfterSales → Ordering bağımlılığı, döngü).
- `Infrastructure/Jobs/IDataConversion.cs` (`09 §13.3` madde 2 arayüzü; işleyici sonra), `IStatusCondition.ReadsProcessMemory` (üç bellek koşulu `true`), `CommandHost` `AddStatus()` + eşik ayarları.
- Dokümanlar: `09 §13.2` madde 5 ("yok" = boş kimlik; `status` satırı ve "ölçülemez") ve tablo satırı, `§13.3` madde 5, `.claude/CONTEXT.md` Commands satırı.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Commands/{AccountTargetFlow,EndSessionsCommand,InvalidatePasswordCommand,StatusCommand,AdminCommandCatalog,CommandHost}.cs`
- `src/Shopfolio.Web/Infrastructure/{Logging/AuditLine.cs, Status/StatusEvaluator.cs, Status/StatusConditions.cs, Jobs/JobQueueStatusReader.cs, Jobs/IDataConversion.cs, Jobs/JobQueueRegistration.cs}`
- `src/Shopfolio.Web/Modules/{Administration,Ordering,Payment,AfterSales}/*` (arayüz ve uygulama yöntemleri)
- Testler: `tests/Shopfolio.UnitTests/Commands/AccountCommandsTests.cs` (yeni), `tests/Shopfolio.UnitTests/Infrastructure/StatusEvaluatorTests.cs`, `tests/Shopfolio.IntegrationTests/Commands/AccountAndStatusCommandTests.cs` (yeni)
- `Docs/09_CODING_GUIDELINES.md`, `.claude/CONTEXT.md`, `Docs/IMPLEMENTATION_STATUS.md` (T389 satırı), `Docs/STATUS_CHANGELOG.d/T389.md`, `Docs/DEFERRED_BACKLOG.d/T389.md`, `Docs/DEFERRED_BACKLOG.d/T303.md` (D-541 notu)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | `end-sessions` e-postayla gösterilen hesabın bütün oturumlarını siler; e-posta terminalden | ✓ | Integration `EndSessions_DeletesEverySessionOfTheNamedAccount_AndKeepsTheOtherAccountsAndTheAccountRow`, `EndSessions_TheEmailIsMatchedByTheNormalizedForm_*`; Unit `RedirectedInput_ExitsWithTwo_*` |
| 2 | `invalidate-password` şifreyi kullanılamaz kılar, aynı işlemde oturumları siler; yalnız sıfırlamayla açılır | ✓ | Integration `InvalidatePassword_EmptiesTheDigest_DeletesTheSessions_AndRefusesTheOldPassword_ButTheResetPathStaysOpen`, `InvalidatePassword_WhenTheSessionsCannotBeDeleted_TheDigestStays_BothAreOneTransaction` (oturum silmesini reddeden tetikleyiciyle geri alma) |
| 3 | Hesap yoksa satırda hedef "yok", çıkış 3 | ✓ | Integration `UnknownOrEmptyEmail_ExitsWithThree_ChangesNothing_AndTheLineHoldsTheEmptyId` (4 örnek: bilinmeyen, boş, boşluk); Unit `TheNoTargetLine_HoldsTheCommandAndTheEmptyId_*` |
| 4 | Sıra: e-posta → iç kimlik → satır → işlem; e-posta ve ad kayda girmez | ✓ | Integration `TheLine_IsWrittenBeforeTheWork_CarriesTheAccountId_AndHoldsNoEmailOrName` (satır yazılırken oturumlar ve özet duruyor), `TheCommandFile_HoldsOneLineWithTheCommandAndTheAccountId_AndNoEmailOrName` |
| 5 | Kurtarma sırası; arada açılan oturum da düşer | ✓ | Integration `Recovery_EndSessionsThenInvalidatePassword_ASessionOpenedBetweenTheTwoFallsToo` |
| 6 | `status` koşulları, sürümü, açık ve kapalı hatları, "başarısız" iş satırlarını yazar | ✓ (⚠-3) | Integration `Status_WritesTheVersion_EveryConditionByName_TheJobQueueAndTheUnmeasurableOnes`, `Status_AFailedJobRow_OpensTheJobQueueCondition_AndIsCounted`; Unit `StatusEvaluatorTests.Standings_*` |
| 7 | `status` iki sayıyı yazar; ikinci sayı panel sayaçlarına girmez | ✓ (⚠-2) | Integration `Status_TheFirstNumberCounts*`, `Status_TheSecondNumberCountsAWithinWithdrawalWindowCardOrder_*`, `Status_WithNoCardOrderAtAll_WritesTwoZeros`; Unit `RefundExposureCount_IsReadOnlyByTheStatusCommand_*` + `RefundExposureScan_OnSamples_*` |
| 8 | `status` kayıtlı her dönüşümün kalan satırını yazar | ✓ | Integration `Status_WritesTheRemainingRowsOfEveryRegisteredConversion`, `Status_WithNoRegisteredConversion_SaysSo` |
| 9 | Üç komut işlem izine yazmaz, bildirim üretmez | ✓ | Integration `TheCommand_WritesNothingToTheTrailTheSentRecordsOrTheJobTables` (3 komut; `audit_logs`, `dispatch_records`, `jobs`, `login_records` anlık görüntüsü `xmin` dahil değişmez, sayılar 0) |

### Öz denetim (bitiş kapısı 2)

Önceki işçinin yaptıkları dahil kabul kriterleri ve KK1–KK9 diff'te yeniden denetlendi; `impl/sik-fail-nedenleri.md` okundu.

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 hesabın bütün oturumları silinir | ✓ | `EndSessions_DeletesEverySessionOfTheNamedAccount_*` (A'nın iki oturumu gider, B'ninki ve hesap satırı kalır) |
| KK2 şifre kullanılamaz, oturumlar aynı işlemde | ✓ | `InvalidatePassword_*` iki test; eski şifreyle `CheckCredentialsAsync` reddi, sıfırlama isteği `IsIssued` |
| KK3 bilinmeyen e-postada 3 ve "yok" satırı | ✓ | `UnknownOrEmptyEmail_*` (komut dosyasında boş kimlik, gerçek hesabın kimliği yok) |
| KK4 kayıtta e-posta ve ad yok; `stdin` boruda iki komut 2 | ✓ | `TheCommandFile_*`; gerçek süreç `RealProcess_WithARedirectedInput_ExitsWithTwo_WritesNoLine_AndChangesNoRow` (iki komut); Unit `RedirectedInput_ExitsWithTwo_ReadsNothing_WritesNoLine_AndOpensNoDatabase` |
| KK5 kurtarma sırası uçtan uca | ✓ | `Recovery_*` |
| KK6 durum çıktısı koşulları ve iş satırlarını içerir | ✓ | `Status_WritesTheVersion_*` (kayıtlı her `IStatusCondition` adı çıktıda, üç "ölçülemez"), `Status_AFailedJobRow_*` (`JobQueue: AÇIK`, "Başarısız iş satırı: 1", "Durum ucu: 503") |
| KK7 iki sayı; cayma süresi dolmamış girer, dolmuş ve açık kalemsiz girmez; panel sayacı değişmez | ✓ (⚠-2) | `Status_TheSecondNumber*`: iki gün önce teslim ve teslim edilmemiş ödenmiş kart siparişi girer (2); 90 gün önce teslim, havale, bekleyen, tam iade edilmiş girmez; sipariş/kalem/iade tabloları değişmez. Panel sayaçları henüz yok: yöntemin yalnız `StatusCommand`'dan çağrıldığını kaynak tarama testi sınar (D-601) |
| KK8 dönüşüm için kalan sayı | ✓ | `Status_WritesTheRemainingRowsOfEveryRegisteredConversion` ("kalan 42 satır", "kalan 0 satır") |
| KK9 izde satır yok, bildirim yok | ✓ | `TheCommand_WritesNothingToTheTrailTheSentRecordsOrTheJobTables` |
| `09 §25` ilgili satırlar / güvenlik taraması | ✓ | `CommandStructureTests` (tablo eşitliği, `CommandsSource_*`), `SessionWriteSourceScan`, `JobSourceScan` (Jobs tablosuna yalnız `Infrastructure/Jobs/`: okuyucu orada), `LogTemplateSourceScanTests`, Unit tam yeşil; satır kişisel değer taşımaz |
| Sık FAIL (1) bekçi/tarama istisnası geniş | ✓ | Yeni tarama (`RefundExposureCount_*`) izinli listesi üç dosya, tam yol eşitliği; ihlal örnekleri ayrı (çağrı, yöntem grubu, `nameof`, satır bölünmüş çağrı); yorum satırı sayılmaz |
| Sık FAIL (2) tarama bir çağrı biçimini kaçırıyor | ✓ | `RefundExposureScan_OnSamples_*`: dört biçim kırmızı, üç izinli/yorum biçim yeşil; tarama çağrı değil yöntem adının kendisini arar |
| Sık FAIL (3) 06 "Varsayılan" / FK dizini | ✓ (uygulanmaz) | Şema değişikliği yok |
| Sık FAIL (4) aynı sayıyı anan öteki yerler | ✓ | "Dokuz komut" sayısı değişmedi (üç komut zaten `09 §13.2` tablosunda); `09 §13.2` tablosu ve madde 5, `§13.3` madde 5, `CONTEXT.md` güncellendi; `05 §8.5` tablosu zaten üç satırı taşıyor (`EveryListedCommand_IsARowOfThe09TableAndOfThe05Table` yeşil) |
| Sık FAIL (5) belge kuralını değiştiren yeni 09 metni karar kaydısız | ✓ (⚠ ile) | Eklenen metin kuralı değil gösterimi tanımlar ("yok" = boş kimlik) ve bilinen sınırı yazar ("ölçülemez", D-598); `05 §8.5.6` ile ayrışan tek nokta D-598 olarak kayıtlı |
| Sık FAIL (modül kurucusuna bağımlılık, test düzenekleri) | ✓ | Hiçbir modül kurucusu değişmedi; yalnız arayüz yöntemleri eklendi; Integration tam koşuldu |
| Sık FAIL (eşzamanlılık / kilit sırası) | ✓ | `EndAdminSessionsAsync` ve `InvalidateAdminPasswordAsync` hesap kilidini önce alır, sonra özet, sonra oturumlar (`09 §15.1` madde 3, sıfırlama ile aynı sıra) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ 4820/4820 | `dotnet test --project tests/Shopfolio.UnitTests -c Release` | Commands, Architecture, Consistency dahil |
| Integration | ✓ 2790/2790 | `dotnet test --project tests/Shopfolio.IntegrationTests -c Release` (PostgreSQL 18.6 kapsayıcı `t389`) | `AccountAndStatusCommandTests` 26/26, `CreateAdminCommandTests` dahil |
| Build/Format | ✓ | `dotnet build Shopfolio.slnx -c Release -warnaserror`; `dotnet format <proje> --verify-no-changes` (Web, UnitTests, IntegrationTests) | 0 uyarı, 0 hata, biçim farkı yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | Bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok (komut `StatusThresholdOptions` bölümünü web ile aynı kaynaktan bağlar)
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T389-hesap-durum-komutlari`
- **Commit:** son commit PR'ın head'idir
- **PR:** *(açılınca doldurulur)*
- **CI:** yapım işçisi izlemez (yönetici yürütür)
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım: yok | Plan metni tetikleyici terimler için tarandı; task yeni paket ya da dış servis kullanmıyor | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `status` web belleğindeki üç koşulu ölçemez | D-598 | Komut ayrı süreç; çözüm kalıcı durum ya da `05 §8.5.6` sınırı; T387 |
| 2 | "Açık kart işi" iki alt sayı | D-599 | `08 §2.1.2` yalnız anar; kurulumu yapan onaylar |
| 3 | `DataConversionJobHandler` ve ilk dönüşüm yok | D-600 | İlk dönüşüm task'ı |
| 4 | Geri ödeme sayısı yaklaşık; panel sayacı henüz yok | D-601 | Panel sayaçları task'ı |
| 5 | `invalidate-password` tanınan tarayıcı işaretlerini kapatmıyor | D-602 | `05 §6.9.2` istemiyor olabilir; T387 |

## Notlar

- **Devam:** önceki işçi bağlam sınırında WIP commit `3d0fa425` ile bölündü; bu işçi test, belge ve kayıtları tamamladı.
- **Repo memory:** `MEMORY.md` ve STATUS "Son güncelleme" satırına dokunulmadı (olay `STATUS_CHANGELOG.d/T389.md`).
- **⚠ listesi (kolay, kurala en uygun seçenekle karar):**
  1. Hesap yoksa satır = boş kimlik (`AuditLine.AdminCommandWithoutTarget`); `status` satırı da hedefsiz. D-541 açık kalır (yalnız `create-admin` reddi için), nota yazıldı.
  2. "Açık kart işi" = bekleyen kart siparişi + bitmemiş kart iadesi, iki alt sayı ayrıca yazılır. "Geri ödemesi doğabilecek kart siparişi": kartla ödenmiş (Ödendi / Kısmen geri ödendi), açık adedi olan ve teslim edilmemiş ya da fiziksel kalemde cayma süresi dolmamış (hizmet/dijital: teslim işareti yok) kalem; süresi dolmuş teslim edilmiş açık kalem (ayıp yolu) sayılmaz, yoksa sayı hiç sıfırlanmazdı.
  3. `status` bellek koşullarını (hata sayacı, ödeme başlatma dizisi, sertifika) "ölçülemez" yazar.
  4. Windows ana bilgisayarda `DiskUsage` koşulu `ArgumentException` verir (varsayılan dizinler konteynerin Linux yolları); testler `FileStoreDirectories`'i geçici dizine çevirir. Üretim (Linux konteyner) etkilenmez; `StatusEvaluator` bilinmeyen hatayı yine yukarı geçirir (`05 §9.4.10`).
  5. Yerel Docker: `t389` PostgreSQL 18.6 kapsayıcısı (55480); iş sonunda silindi.
