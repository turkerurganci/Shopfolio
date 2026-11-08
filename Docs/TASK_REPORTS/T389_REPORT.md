# T389 — Hesap ve durum yönetim komutları: oturumları sonlandırma, şifreyi geçersiz kılma, durum

**Faz:** F3 | **Durum:** ⏳ Düzeltme 1 tamam — doğrulama tur 2 bekliyor | **Tarih:** 2026-11-08

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
| 6 | `status` koşulları, sürümü, açık ve kapalı hatları, "başarısız" iş satırlarını yazar | ✓ (⚠-3, ⚠-6) | Integration `Status_WritesTheVersion_EveryConditionByName_TheJobQueueAndTheUnmeasurableOnes`, `Status_AFailedJobRow_OpensTheJobQueueCondition_AndIsCounted`, `Status_WritesTheConditionalLines_*`, `Status_AConditionalLineWhoseSettingsArePresent_IsWrittenOpen_AndNoValueIsWritten`; Unit `StatusEvaluatorTests.Standings_*`, `AccountCommandsTests.DescribeLine_*` (düzeltme 1) |
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

### Düzeltme 1 (tur 1 FAIL sonrası, 2026-11-08)

Dal `origin/main` (0226a902) ile birleştirildi (çakışma yok); iki bulgu ve K1 kapatıldı.

- **Bulgu 1 (KRİTİK):** komut kipi, web'in modüllerinin dayandığı iki kaydı yapmıyordu: imaj işleyicisi (`ImageProcessor`; Program.cs'te çıplak satırdı) ve `IHttpContextAccessor` (Administration `RequestAuditActorSource`, `AddHttpContextAccessor` yalnız `LoggingSetup`'ta). Yeni `ImagingRegistration.AddImageProcessing` hem `Program.cs` hem `CommandHost` tarafından çağrılır (ikinci kopya yok); komut kipi `AddHttpContextAccessor()` ekler (istek yok, aktör yok; sunucu hizmeti değil, `CommandStructureTests` yeşil). Test `TheCommandHost_ResolvesEveryModule_SoACommandThatReachesAnyOfThemDoesNotBreak`: dokuz modülün arayüzü komut kipinde çözülür (benzer kırılmayı yakalar).
- **Bulgu 2 (ORTA):** `status` "Hatlar" bölümü yazar: `CardPayment`, `GoogleSignIn`. Durum web'in tek kural evinden (`ConditionalLines.Evaluate`; `AddConditionalLines` ortak kayıt) okunur, yalnız eksik anahtarın adı yazılır, değer asla. Ayar dosyası anahtarı eksikse KAPALI, yalnız sır dosyası anahtarı eksikse "ölçülemez" (komut süreci sırları almaz, `05 §6.x.19`), hepsi varsa AÇIK. **⚠-6:** "ölçülemez" gösterimi karardır, `D-633` (`DEFERRED_BACKLOG.d/T389.md`) ve `09 §13.2` madde 5 metni.
- **K1:** `Status_TheFirstNumberCounts*` testine `OutcomePending` kart iadesi eklendi (açık kart işi 4 = 1 + 3).

| Madde | Düzeltmeden sonra | Kanıt |
|---|---|---|
| Kriter 1–5 | ✓ | Değişmedi; `AccountAndStatusCommandTests` 29/29 (öncekiler dahil) |
| Kriter 6 | ✓ (⚠-3, ⚠-6) | `Status_WritesTheConditionalLines_*`, `…ArePresent_IsWrittenOpen_AndNoValueIsWritten`, `DescribeLine_*` (3 Unit) |
| Kriter 7 | ✓ (⚠-2) | `Status_TheFirstNumber…` (OutcomePending dahil), `Status_TheSecondNumber…`, `Status_WithNoCardOrderAtAll_WritesTwoZeros` birleşik ağaçta yeşil |
| Kriter 8 | ✓ | `Status_WritesTheRemainingRows…`, `Status_WithNoRegisteredConversion_SaysSo` birleşik ağaçta yeşil |
| Kriter 9 | ✓ | `TheCommand_WritesNothingToTheTrail…` yeşil |
| Sık FAIL: modül bağımlılığı / test düzeneği (T389 r1) | ✓ | Integration TAM küme birleşik ağaçta; modül çözüm testi |
| Sık FAIL: kısmen karşılanan kriter | ✓ | Kriter 6 hatları karşılandı; sır kaynaklı sınır kararla (D-633) |
| Sık FAIL: tek kural evi | ✓ | Hat durumu `ConditionalLines`; kayıt `AddConditionalLines`/`AddImageProcessing` ortak |
| Merge | ✓ | `git diff --name-only --diff-filter=U` boş; çakışma işareti yok; `git status` temiz |

### Düzeltme 2 (tur 2 FAIL sonrası, 2026-11-08)

Bulgu 1 (ORTA, `09 §20` m.2): kartla ödenmiş kalemler artık bütün olarak belleğe alınmaz. `IOrderingModule.ListPaidCardItemsAsync(afterOrderId, maxOrders, ct)` sipariş kimliğine göre sayfa sayfa okur (sayfa = sonraki 500 sipariş ve kalemleri, `Take` ile); `CountCardOrdersWithRefundExposureAsync` sayfaları gezer, her sayfada kapalı adedi sorgular ve siparişleri sayar — bellekte tek sayfa. Süre kuralı iş günü takvimine bağlı olduğundan (SQL'e çevrilemez) küme süzülmedi, sayfalandı. K1: `JobQueueStatusReader` `CountAsync` + `MinAsync` ile SQL'de. K2: `D-601` hedefi `T382`.
Sınama: `RefundExposureCount_WalkedInPagesOfAnySize_GivesTheSameNumber` (Integration; sayfa 1/2/3/500 aynı sayı), `PaidCardItemRead_InTheSource_TakesAPageOfOrders` + örnek taraması (Unit); `Status_TheSecondNumber…` aynen yeşil.

| Öz denetim (düzeltme 2) | Sonuç | Kanıt |
|---|---|---|
| Kabul kriterleri 1–9, KK1–KK9 yeniden | ✓ | Commands Unit/Integration kümeleri yeşil; değişiklik yalnız sayım yolu ve kuyruk okuyucu |
| `09 §25` Performans (büyüyen küme sayfalı) | ✓ | sayfalı okuma, `PaidCardItemRead_*` taraması |
| Biçim / derleme | ✓ | `dotnet build -warnaserror` 0 uyarı, `dotnet format --verify-no-changes` üç projede temiz |
| Birleştirme | ✓ | `origin/main` ile "Already up to date"; çakışma işareti yok |

## Test Sonuçları

Düzeltme 2: Unit 4671 + Commands/Architecture/Jobs 1120 geçti; Integration TAM küme parçalı yeşil (Commands 47, Jobs+Ordering+Payment 376 + Jobs 77 ayrıca, Catalog+Concurrency+CorporateContent+Database+Disposal 281, kalan 2175). Altyapı kaynaklı geçici zaman aşımları (veritabanı bırakma) yeniden koşuda geçti.

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Düzeltme 1 — Unit | ✓ 4926/4926 | `dotnet test --project tests/Shopfolio.UnitTests -c Release --no-build` | Birleşik ağaç |
| Düzeltme 1 — Integration TAM | ✓ 2861/2861 | `--filter-namespace` parçaları (Catalog+Concurrency+CorporateContent+Database 220; Disposal+FileSecrets+Files+Jobs+Logging+Outages+Security+SignedEndpoints+Commands 400; Ordering+Web 879; Model 1362); PostgreSQL 18 `shopfolio-t389-fix-pg` | Birleşik ağaç |
| Düzeltme 1 — Build/Format | ✓ | `dotnet build Shopfolio.slnx -c Release --no-restore -warnaserror`; `dotnet format <proje> --verify-no-changes --no-restore` (Web, UnitTests, IntegrationTests) | 0 uyarı, biçim farkı yok |
| Unit | ✓ 4820/4820 | `dotnet test --project tests/Shopfolio.UnitTests -c Release` | Commands, Architecture, Consistency dahil |
| Integration | ✓ 2790/2790 | `dotnet test --project tests/Shopfolio.IntegrationTests -c Release` (PostgreSQL 18.6 kapsayıcı `t389`) | `AccountAndStatusCommandTests` 26/26, `CreateAdminCommandTests` dahil |
| Build/Format | ✓ | `dotnet build Shopfolio.slnx -c Release -warnaserror`; `dotnet format <proje> --verify-no-changes` (Web, UnitTests, IntegrationTests) | 0 uyarı, 0 hata, biçim farkı yok |

## Doğrulama

### Tur 3 (2026-11-08)

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 3, 2026-11-08; dal ucu `2b73afed`) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 1 (`D-658`'e ileri alındı) |

Kabul kriterleri 1–9 ve KK1–KK9 validator kanıtıyla ✓: Integration `Shopfolio.IntegrationTests.Commands` 47/47 (PostgreSQL 18, `shopfolio-t389-val3-pg`), `Web` 594 (ilk tam koşuda adı alınamayan 1 geçici hata; iki parçalı yeniden koşuda 99 + 495 hepsi yeşil), `Security` 33/33; Unit 4742/4742 + Consistency 280/280; build `-warnaserror` 0 uyarı; format üç proje temiz; betik testleri 4/4; `--locked-mode` restore 0; PR CI run 42516494053 (dal ucu `2b73afed`) bütün adımlar yeşil; ana dal son üç run (42514888426, 42514888368, 42512134557) `success`. `VAL-A984` `PASS` (ikinci sayı: iki gün önce teslim + teslim edilmemiş ödenmiş kart siparişi sayılır, 90 gün önce teslim, havale, bekleyen ve tam iade sayılmaz; `OutcomePending` iade açık kart işine girer; panel sayaçları henüz yok — kaynak taraması ve `D-601`). Tur 2'nin bulgusu kapandı: `ListPaidCardItemsAsync` sipariş kimliği sırasıyla `Take` ile sayfalı okur, sayı sayfa boyundan bağımsız (`RefundExposureCount_WalkedInPagesOfAnySize_GivesTheSameNumber` 1/2/3/500); K1 (`JobQueueStatusReader` `CountAsync` + `MinAsync`) ve K2 (`D-601` hedefi `T382`) kapandı.

**K-notları:**

- K1 — Düzeltme 2'nin getirdiği `AfterSalesModule.ExposurePageOrders = 500` (okuma sayfası) `09 §3.5` madde 4 tablosunda yok; benzer sayfalar (dışa aktarma, imha) tabloda. Kılavuzun koyduğu bir değer olmadığı ve iç ayrıntı olduğu için bloke etmez → `D-658` (F3 faz kapısı).

**Birleştirme notu:** `origin/main` (`48dadb92`) ile birleştirmede `src/Shopfolio.Web/Modules/AfterSales/Contracts/IAfterSalesModule.cs` kod çakışması verdi; birleştirme geri alındı (`git merge --abort`), doğrulama dalın kendi ucunda yapıldı, bu kayıt commit'i birleştirmesiz. PR #472 şu an `CONFLICTING`; merge'den önce çakışma çözülmeli.

**Yapım raporu karşılaştırması:** öz denetim tablosu (kabul kriterleri ve KK1–KK9, düzeltme 2 tablosu) var, her maddede kanıt yazılı; sonuçlar validator ile uyumlu. Düzeltme 2'nin öz denetimi `09 §25`'in "§3.5 tablosu" sorusunu anmıyor (K1).

### Tur 2 (2026-11-08)

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 2, 2026-11-08; dal `4607ae6c` `origin/main` `4c049223` ile birleştirilerek) |
| Bulgu sayısı | 1 (1 ORTA S1) |
| Düzeltme gerekli mi | Evet |
| Bloke-etmeyen not (K) sayısı | 2 |

Kabul kriterleri 1–9 ve KK1–KK9 validator kanıtıyla ✓ (birleşik ağaçta Integration `Shopfolio.IntegrationTests.Commands` 43/43, `StatusEndpointTests` + `ProductGalleryTests` 59/59; Unit 4949/4949; build `-warnaserror` 0 uyarı; format üç proje temiz; betik testleri 4/4; PR CI run 42505970515 yeşil). Tur 1'in iki bulgusu ve K1 kapandı (komut kipi dokuz modülü çözüyor; "Hatlar" bölümü `ConditionalLines`'tan; `OutcomePending` örneği testte). `VAL-A984` satırının beklenen sonucu karşılandı (`PASS`).

**Bulgular:**

1. **ORTA · S1 · `09 §25` Performans satırı ("Büyüyen küme sayfalı mı") / `09 §20` madde 2:** `OrderingModule.ListPaidCardItemsAsync` kartla ödenmiş ve tam geri ödenmemiş **bütün** siparişlerin bütün kalemlerini `ToListAsync` ile belleğe alır; `AfterSalesModule.CountCardOrdersWithRefundExposureAsync` sonra bu kimliklerin hepsiyle `ClosedQuantitiesAsync`'i çağırır ve süzmeyi bellekte yapar. Ödendi siparişi süresiz Ödendi kalır, küme siparişle büyür ve hiç küçülmez; `§20` madde 2 "`ToList`/`ToArray` ile bütün kümeyi belleğe almak yasaktır" der (tek istisna dışa aktarma), kümenin kapalı olduğu gerekçesi de yazılı değil. `§25` "hayır" cevabı düzeltilmeden kabul edilmez. Düzeltme yönü: sayım SQL'de yapılır ya da sorgu yalnız açık / cayma süresi içindeki kalemleri getirecek biçimde süzülür (teslim edilmemiş fiziksel kalem, işaretsiz hizmet/dijital kalem, teslim tarihi son 14 günün içinde olan fiziksel kalem) — böylece okunan küme etkin siparişlerle sınırlanır; davranış testleri (`Status_TheSecondNumber…`) aynen yeşil kalmalı.

**K-notları:**

- K1 — `JobQueueStatusReader.ReadAsync` zamanı gelmiş bütün bekleyen satırların zamanını listeye alıp en eskisini bellekte bulur; sayı `CountAsync`, en eski an `MinAsync` ile SQL'de alınabilir (aynı düzeltme turunda kapatılır).
- K2 — `D-601`'in hedefi "Panel sayaçları task'ı" yazılı; task'ın kimliği `T382`'dir (Panel ana sayfası verisi: altı bekleyen iş sayacı) — hedef sütunu `T382` olarak düzeltilir (aynı düzeltme turunda kapatılır).

**Yapım raporu karşılaştırması:** öz denetim tablosu var ve her maddede kanıt yazılı; "`09 §25` ilgili satırlar ✓" satırı Performans bölümünü (büyüyen küme, `§20` madde 2) anmıyor — bulgu 1 bu satırla uyuşmuyor. Öteki kriter ve KK sonuçları validator ile uyumlu.

### Tur 1 (2026-11-08)

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 1, 2026-11-08; dal `origin/main` ile birleştirilerek) |
| Bulgu sayısı | 2 (1 KRİTİK S2, 1 ORTA S1) |
| Düzeltme gerekli mi | Evet |
| Bloke-etmeyen not (K) sayısı | 1 |

**Bulgular:**

1. **KRİTİK · S2 · VAL-A984, kriter 6–9 (`status` kısmı):** main ile birleşik hâlde `admin status` `Beklenmeyen hata: InvalidOperationException` ile 1 döner — `Unable to resolve service for type 'Shopfolio.Infrastructure.Imaging.ImageProcessor' while attempting to activate 'Shopfolio.Modules.Catalog.ImageUploads'` (`StatusCommand.WriteProviderKeysAsync`, `IAfterSalesModule` çözülürken; main'deki T320 ile `AfterSalesModule` kurucusuna `ISettingsModule` girdi ve zincir Catalog'un `ImageUploads`'ına iner, `CommandHost` imaj işleyicisini kaydetmiyor). Kanıt: PR CI run 42495682161 "4. Integration test" — `AccountAndStatusCommandTests`'in 9 `status` testi kırmızı (2847'de 9); yerelde `git merge origin/main` sonrası `Status_WithNoCardOrderAtAll_WritesTwoZeros` aynı hatayla kırmızı. Birleşmeden önceki dalda 26/26 yeşil. Düzeltme: komut kipinin hizmet koleksiyonu çözülebilir olmalı (eksik kayıt ya da bağımlılık yolu) ve komut kipinde her komutun gerektirdiği modüllerin çözüldüğünü sınayan bir test.
2. **ORTA · S1 · kriter 6 / KK6:** `05 §8.5.6` "açık ve kapalı hatları ayrıntısıyla yazar"; `05 §8.4.6` koşullu ayar eksikse "ilgili hat kapalıdır ve bu hâl ... durum komutuna yazılır"; `InstallationSettingsRegistration` yorumu "the status command reads it (T389)". `StatusCommand` `ConditionalLine` (kart ödemesi, Google ile giriş; eksik anahtar adlarıyla) okumuyor ve yazmıyor; bu boşluk için backlog kaydı yok (D-598 yalnız bellek koşullarını kapsar). Not: komut kipi `05 §6.x.19`'a göre sağlayıcı sırlarını almaz — hattın durumu komut sürecinde nasıl okunacağı da çözülmeli (ör. açılış denetiminin kaydı) ya da kararla `05`'e yazılmalı.

**K-notu:** K1 — `CountOpenCardRefundsAsync` "sonucu bilinmeyen" (`OutcomePending`) kart iadesini sayar ama entegrasyon testinde yalnız `Sending` ve kaydı olmayan iade örneklenmiş; `VAL-A984`'ün "sonucu bilinmeyen kart iadesi" örneği düzeltme turunda teste eklenir (bu PR'ın düzeltmesinde kapatılacak).

**Yapım raporu karşılaştırması:** kriter 6 "✓ (⚠-3)" yazılmış, ama "açık ve kapalı hatlar" `ConditionalLine` olarak karşılanmamış (bulgu 2); Integration "2790/2790" birleşmemiş dalda koşulmuş, main ile birleşik hâl kırmızı (bulgu 1); öz denetim tablosu var ve her maddede kanıt yazılı.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok (komut `StatusThresholdOptions` bölümünü web ile aynı kaynaktan bağlar)
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T389-hesap-durum-komutlari`
- **Commit:** son commit PR'ın head'idir
- **PR:** #472
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
