# T331 — Satış kapısı: dört koşul, veri toplayan girişlerin kapısı, satışın geçici kapatılması

**Faz:** F3 | **Durum:** ✓ Tamamlandı | **Tarih:** 2026-11-07

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Kapı değeri ve kuralı:** `SalesGate` (sınıf; kurucusu `internal`, `with` ile kopyalanamaz) `Settings/Contracts/SalesGate.cs`; üç enum `Contracts/Gates/` altında (`SalesGateCondition`: kimlik, ödeme yöntemi, geçici kapatma, aydınlatma metni, çerez politikası, iade adresi; `SalesGateOperation`; `DataCollectionEntry`). `SalesGateRules` kapı kararının tek yeridir (saf kural); `SalesGates` kapıyı her çağrıda veritabanından okur (önbellek yok; `09 §10` madde 5, K-980).
- **Dört koşul:** kimlik (T330 eksik alan listesi), ödeme yöntemi (`ICardPaymentCondition` T352, `IBankTransferCondition` T353), geçici kapatma anahtarı, dört yasal metin (aydınlatma `IPrivacyNoticeCondition` T366; çerez politikası gerçek yayın verisinden `ICookiePolicyPublication`; iade adresi dolu). Bağlanmamış koşul karşılanmamış sayılır (`UnboundSalesGateConditions`, `TryAdd`).
- **Kapalı kapıda:** sepete ekleme, ödeme adımına geçiş, yeni sipariş onayı `ErrorCode.SalesClosed` ile reddedilir; eksik koşullar adıyla gelir; vitrin, var olan sepet ve açık sipariş kapıdan etkilenmez. Veri toplayan girişler (hesap kaydı, yeni hesap açacak Google girişi, iletişim formu) aydınlatma metni ve çerez politikasına bağlıdır, `ErrorCode.DataCollectionClosed`; mevcut hesabın girişi hiç kapanmaz.
- **Anahtar:** `ISettingsModule.CloseSalesTemporarilyAsync(shown)` / `ReopenSalesAsync(shown)`; ekranın gösterdiği değere karşı koşullu tek `UPDATE` (kilitsiz, sürüm artırmaz); yazılırsa aynı işlemde `UpdateSettings` iz satırı (`IAuditTrail`); bayat ya da zaten istenen durumda hiçbir şey yazmaz, iz satırı bırakmaz. Yönetici oturumsuz ya da işlem dışı çağrı `InvalidOperationException`.
- **Kurulum kontrol listesi:** `SalesGate.SetupChecklist` (anahtar kurulum işi değildir) aynı değeri okur.
- **Mimari tarama:** `SalesGateSourceScan` — anahtar adı (`IsSalesTemporarilyClosed`, `is_sales_temporarily_closed`) yalnız varlık, bölüm yazıcısı, kapı okuyucusu ve anahtar yazıcısı dosyalarında geçer (göç dosyaları dışında); `new SalesGate(` yalnız `SalesGateRules.cs`'te. Tüm metin okunur (çok satırlı çağrı bulunur), yorumlar boşaltılır; ayrı ihlal örneği `ViolationSamples/SalesGate`.
- Önceki task'ların merge kaydı: T304, T330, T380, T305 raporlarının "Post-merge run'lar" satırı ve status Commit hücreleri.
- Backlog: D-198 (üç dış koşulun bağları), D-199 (veri toplayan girişlerin ve anahtarın ekran bağı).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Settings/`: `SalesGateRules.cs`, `SalesGates.cs`, `SalesSwitches.cs`, `UnboundSalesGateConditions.cs`, `CompanySettingsWriter.cs` (`WriteSalesSwitchAsync`), `SettingsModule.cs`; `Contracts/SalesGate.cs`, `Contracts/SalesSwitchResult.cs`, `Contracts/ISalesGateConditions.cs`, `Contracts/ISettingsModule.cs`, `Contracts/Gates/{SalesGateCondition,SalesGateOperation,DataCollectionEntry}.cs`
- `tests/Shopfolio.UnitTests/`: `InstallationSettings/SalesGateRulesTests.cs`, `Architecture/SalesGateSourceScan.cs`, `Architecture/SalesGateSourceScanTests.cs`, `Architecture/ViolationSamples/SalesGate/SwitchReadAndGateConstruction.cs`
- `tests/Shopfolio.IntegrationTests/Model/SalesGateTests.cs`, `Model/CompanyIdentityTests.cs` (modül artık `IAuditTrail` ister)
- `Docs/DEFERRED_BACKLOG.md` (D-198, D-199), `Docs/IMPLEMENTATION_STATUS.md`, `.claude/memory/MEMORY.md`, T304/T330/T380/T305 raporları

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Satış yalnız dört koşul birlikte sağlanınca açık (iade adresi dahil) | ✓ | Unit `WhenAllFourConditionsHold_SalesAreOpen_AndEverythingIsPermitted`, `EachConditionBroken_ClosesSales_AndIsNamed`; Integration `Gate_IsOpen_WhenTheFourConditionsHoldTogether`, `Gate_IsClosedWithEachConditionBroken_AndNamesIt` |
| 2 | Bozulunca ekleme, ödeme adımı, onay kapanır; sebep ve eksik koşul söylenir; vitrin ve sepet sürer | ✓ | Unit `AClosedGate_RefusesTheAddingTheCheckoutAndTheConfirmation_AndNeverTheShowcaseTheCartOrAnOpenOrder`; Integration `ClosedGate_RefusesTheAddingTheCheckoutAndTheConfirmation_NamingTheMissingCondition_AndKeepsTheShowcaseTheCartAndTheOpenOrders` |
| 3 | Veri toplayan girişler aydınlatma metni ve çerez politikasına bağlı; mevcut hesap girişi etkilenmez | ✓ | Unit `ThePrivacyNoticeOrTheCookiePolicyMissing_ClosesTheDataCollectingEntries_ButNeverTheSignInOfAnExistingAccount`; Integration `DataCollectingEntries_AreRefusedUntilThePrivacyNoticeAndTheCookiePolicyArePublished_AndTheSignInOfAnExistingAccountIsNot` |
| 4 | Anahtar açılıp kapanır ve işlem izine yazılır | ✓ (modül yüzü; panel ekranı T535 — D-199) | Integration `CloseSales_WritesTheSwitchAndOneTrailRow_AndReopeningWritesAnother`, `TheSwitch_RolledBackWithItsTransaction_LeavesNeitherTheSwitchNorTheTrailRow` |
| 5 | Anahtar açıkken açık siparişler etkilenmez; havale onayı sürer | ✓ | Integration `OpenOrders_AreNotTouched_WhenTheSwitchOrAConditionClosesTheGate_AndTheirIbanAndTextsStayAsFrozen`; `ContinueOpenOrder` her durumda izinli (Unit `AnOpenGate_RefusesNothing`) |
| 6 | Kimlik alanı boşalması ya da havale kapanması açık siparişi etkilemez; yayındaki yasal metin çekilmez | ✓ | aynı Integration test; `APublishedLegalText_IsNeverWithdrawn_ByTheGateClosing_AndTheStoreRefusesItsUnpublishing` |
| 7 | Havale siparişi donan IBAN'ıyla sürer | ✓ | `OpenOrders_AreNotTouched_…` sipariş durumunu (IBAN dahil) önce ve sonra karşılaştırır |
| 8 | Kapı kararı tek yerde; karşılanmayan koşullar adıyla; dört yüzey aynı sonucu okur; T352/T353/T366 bağlar; bağlanmamış karşılanmamış | ✓ | `SalesGateSourceScanTests.ApplicationSource_NamesTheSwitchOnlyInTheGateFiles_AndBuildsAGateOnlyInTheRule`; Unit `TheFourSurfaces_ReadOneResult_ForEveryCombinationOfTheFacts`, `AnUnboundCondition_IsNotMet_SoTheGateStaysClosedUntilItsTaskBindsIt`; Integration `UnboundConditions_KeepTheGateClosed_UntilTheirTasksBindThem`, `Gate_IsReadFromTheDatabaseOnEveryCall_NotStoredNorCached` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| k1–k8 | ✓ | yukarıdaki tablo |
| KK1 dört koşuldan biri eksikken satış kapalı | ✓ | `Gate_IsClosedWithEachConditionBroken_AndNamesIt` (her koşul için) |
| KK2 kapalı kapıda ekleme, ödeme, onay reddedilir; vitrin ve sepet sürer | ✓ | `ClosedGate_RefusesTheAddingTheCheckoutAndTheConfirmation_…` |
| KK3 kayıt, Google yeni hesap, form reddedilir; mevcut giriş sürer | ✓ | `DataCollectingEntries_AreRefusedUntil…` (üç giriş + `ExistingAccountSignIn` izinli) |
| KK4 anahtar değişimi iz satırı bırakır | ✓ | `CloseSales_WritesTheSwitchAndOneTrailRow_…` (işlem `UpdateSettings`, alan `IsSalesTemporarilyClosed`, yönetici, hedef kayıt); bayat çağrı satır bırakmaz: `TheSwitch_WithAStaleShownValue_…` |
| KK5 açık sipariş anahtardan etkilenmez | ✓ | `OpenOrders_AreNotTouched_…` |
| KK6 kimlik boşalınca açık sipariş sürer, yasal metin çekilmez | ✓ | `OpenOrders_AreNotTouched_…` (tip değişimi ve zorunlu alan boşalması), `APublishedLegalText_IsNeverWithdrawn_…` |
| KK7 havale siparişi donmuş IBAN'la sürer | ✓ | `OpenOrders_AreNotTouched_…` (havale kapanıp IBAN değişir, siparişin IBAN'ı aynı) |
| KK8 dört yüzey aynı sonucu okur | ✓ | `TheFourSurfaces_ReadOneResult_ForEveryCombinationOfTheFacts` (Unit); `TheModuleSurface_GivesTheGateInOnePlace_AndTheSwitchChangesInTwoMethods` |
| `09 §25` / `§10` madde 5 (K-980): onay işlemi ayarı önbellekten okumaz | ✓ | `SalesGates` önbellek kullanmaz; `Gate_IsReadFromTheDatabaseOnEveryCall_NotStoredNorCached` |
| `09 §10.3` ayar satırı tek yazıcı | ✓ | anahtar yazımı `CompanySettingsWriter.WriteSalesSwitchAsync`; `ConcurrencySourceScan` yeşil |
| Sık FAIL (1) bekçi/tarama istisnası geniş | ✓ | kapalı dosya listesi (4 dosya + göç klasörü), `TheSwitchFilesReallyNameTheSwitch_…` ölü dosya denetimi |
| Sık FAIL (2) tarama çağrı biçimini kaçırıyor | ✓ | özellik her biçimde (üye erişimi, `nameof`, dize, lambda, camelCase), sütun adı, `new SalesGate(`, nitelikli ad, satır sonu, hedef türlü `SalesGate x = new(` (`ACallSplitOverLines_IsFound`, ihlal örneği). Sınır: dönüş türünden hedef türlü `=> new(` biçimi metinle ayırt edilemez; kurucu `internal` ve `ASalesGate_HasNoPublicConstructorAndNoSetter_…` ile kilitli |
| Sık FAIL (3) 06 "Varsayılan" / FK dizini | ✓ (uygulanmaz) | şema değişmedi; yeni sütun ya da FK yok |
| Sık FAIL (4) kural değişince aynı sayıyı anan yerler | ✓ | enum'lar `Contracts` ad alanı dışında (`Contracts.Gates`) — saklanan enum kilit ve eşitlik testleri (`EnumLockTests`, `EnumTableConsistencyTests`) bu saklanmayan değerleri saymaz; 09/06 sayısı değişmedi |
| Sık FAIL (5) belge kuralını değiştiren yeni 09 metni | ✓ | `09` değişmedi; karar kaydı gerekmez |
| Ayrı ihlal örneği (tarama kuralı) | ✓ | `ViolationSamples/SalesGate/SwitchReadAndGateConstruction.cs` |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit (Mimari ad alanı) | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build -- --filter-namespace "Shopfolio.UnitTests.Architecture"` | 809 / 0 fail |
| Unit (öteki) | ✓ | `… -- --filter-not-namespace "Shopfolio.UnitTests.Architecture"` | 2473 / 0 fail |
| Integration | ✓ | `--filter-class "*.SalesGateTests"` (22), `"*.CompanyIdentityTests"` (24), `SettingsTablesTests` + `ConcurrencyTests` + `ModelRuleTests` (68), yerel PostgreSQL 18 | 0 fail |
| Biçim | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes --no-restore` | temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-07; head `461c66b` + `origin/main` birleştirmesi). Kriter 1–8 ve KK1–KK8 validator kanıtıyla ✓ (kanıt testleri yukarıdaki tabloyla aynı; validator yerelde build `-warnaserror`, format, Unit 3282, betik testleri, Integration SalesGate 22 + CompanyIdentity/AuditTrail 46 koştu; dal CI 42361992653 yeşil). Kriter 2–7'nin ekran ve POST yüzleri (sepet, ödeme, kayıt, form, panel) modül düzeyinde doğrulandı; bağları D-198, D-199. `12` satırları (KP-25, KP-38, KP-59, KP-60) `BEKLEMEDE` kaldı: kanıt türleri uç yanıtı / tarayıcı gözlemi, uçlar sonraki task'larda |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 0 |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok (`is_sales_temporarily_closed` T-önceki göçte var)
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T331-satis-kapisi`
- **Commit:** bkz. PR (son commit dal başı)
- **PR:** #425
- **CI:** dal CI 42361992653 success (head `461c66b`)
- **Post-merge run'lar:** squash `eb91975` (#425) — CI 42364842660 success, Publish 42364842661 success (T301 yapımında dolduruldu)

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | Dış varsayım: yok | Yeni paket, harici servis, yapılandırma yok; T352/T353/T366 bağları arayüzle ayrıldı (D-198) | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Ödeme yöntemi (T352, T353) ve aydınlatma metni (T366) koşullarının bağı; kapıyı okuyan yüzler (vitrin, sepet, ödeme, panel kurulum listesi) | D-198 | Sahipleri sonraki task'lar; bağlanmamış koşul karşılanmamış sayıldığından kapı o zamana dek kapalı (kriter 8) |
| 2 | Veri toplayan girişlerin ve "satışı kapat / aç" işleminin ekran ve POST bağı | D-199 | Ekran task'ları (T341, T365, T535); T331 modül yüzüdür |

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** main CI son koşular success (`7cc0db5`: CI 42360805990, Publish 42360805989)
- **Repo memory (bitiş kapısı 9):** güncellendi
- **⚠ Kolay kararlar (öneriyle verildi):** (1) çerez politikası koşulu gerçek yayın verisinden okunur, T366 yalnız aydınlatma metnini bağlar; (2) kapı `record` değil sınıftır (`with` ile kopyalanamaz) ve kaynak taramasıyla tek yerde kurulur; (3) ekran ve POST yüzleri (UC-165/166, kayıt, iletişim, sepet, ödeme) sonraki task'larda, bu task modül yüzüdür; (4) anahtar yazımı yönetici oturumsuz `InvalidOperationException` verir (iz kuralı); (5) üç enum `Contracts.Gates` alt ad alanındadır: `Contracts` ad alanındaki her enum saklanan sayıldığı için (06 enum tabloları ve kilit dosyaları) saklanmayan değerler dışarıda tutuldu, modül sınır kuralı alt ad alanını `Contracts` sayar.
