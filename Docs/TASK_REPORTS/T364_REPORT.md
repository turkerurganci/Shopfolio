# T364 — Panelde eşzamanlı düzenleme ve siparişte çakışan işlemler

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (yapım tamam, doğrulama bekliyor) | **Tarih:** 2026-11-10

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

Eşzamanlılığın mekanizması (`RecordSave`, `OrderLocks`, `ShownVersion`, `VersionFields`, `FormResults.Conflict`) T208, T316, T317, T358, T370 ve T386'da kurulmuştu. T364 boşlukları kapattı ve kuralı uçtan uca kanıtladı:

- **Sistemin kendiliğinden geçişinin reddi uygulama kaydında (K-1559; `02 §3.31.2`, `05 §9.1.3`).** Yeni olay adı `LogEvents.AutomaticTransitionRefused` ve tek yazıcısı `Modules/Ordering/AutomaticTransitionLog` (LoggerMessage, Information; alanlar: olay, geçiş biriminin red kodu, siparişin iç kimliği; kişisel veri yok, bildirim yok). Yazıldığı yerler:
  - geçiş biriminin sistem aktörlü reddi (`OrderTransitions.ApplyChosenAsync` sarmalayıcısı; gövde `ApplyChosenCoreAsync`): ödeme süresi dolumunun iptali, sağlayıcının kart ödemesinin Ö1/S1'i;
  - sağlayıcının kart ödemesinin artık kart hattında olmayan siparişe gelmesi (`ApplyCardPaymentAsync`) ve kart iadesi sonucunun artık açık olmayan kayda gelmesi (`ApplyCardRefundOutcomeAsync`);
  - zamanlayıcının durum geçişi kaynağının (`ScheduledOrderTransition.IsStatusTransition`; `TransferPaymentExpiry`) kilit altında vadesi artık gelmemiş bulduğu sipariş (`ConditionNotMet`). Hatırlatma bildirimi geçiş değildir, satır yazmaz.
  - Müşterinin reddini sipariş sayfası, yöneticininkini panel söyler; ikisi bu satırı yazmaz.
  - `09 §18.2 m.3` kapalı olay listesine eklendi.
- **Yöneticinin sürüm karşılaştırması tek yerde (`09 §10.2 m.3`).** Sipariş tarafındaki bütün elle karşılaştırmalar `ShownVersion.Matches`'e çevrildi (OrderTransitions ×2, ItemRecords, LateDeliveryTermination, Payment ×2, RefundLine, OrderCorrections, DownloadCounters). Yeni kaynak taraması `ShownVersionSourceScan` (Ordering/AfterSales/Payment): eşitlik ve sıra işleçleri, `is`, `{ Version: … }` örüntüsü, `Equals`/`CompareTo`/`Compare` çağrıları, satır kırılımı ve parantez biçimleri; kasıtlı ihlal örneği `ViolationSamples/Concurrency/ShownVersionComparisons.cs` (16 bulgu satırı + 5 bulgu olmayan biçim). `09 §10.2 m.3`'e tarama cümlesi eklendi.
- **Çakışma testleri (gerçek PostgreSQL):** iki yönetici aynı anda havale "ödendi" işareti, aynı havale geri ödemesi ve aynı kalem çıkarma → yalnız biri uygulanır, öteki `VersionConflict`; tek geçiş/ödeme/geri ödeme/kalem çıkarma satırı, tek bildirim. Yöneticinin işaretinden sonra sistem ve müşteri iptali reddedilir, yalnız sistemin reddi kayda yazılır, bildirim yok, sipariş kilitli kalmaz (`FOR UPDATE NOWAIT`). Süre dolumu ödenmiş siparişte tek satır yazar.
- Modül kaydı: `AddOrderingModule` `services.AddLogging()` (TryAdd; host'un kaydını bozmaz) + `AddSingleton<AutomaticTransitionLog>()`.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Logging/LogEvents.cs`
- `src/Shopfolio.Web/Modules/Ordering/AutomaticTransitionLog.cs` (yeni), `OrderTransitions.cs`, `OrderTransitions.Payment.cs`, `OrderTransitions.ItemRecords.cs`, `OrderTransitions.LateDeliveryTermination.cs`, `OrderTransitions.RefundLine.cs`, `OrderCorrections.cs`, `DownloadCounters.cs`, `OrderingModule.cs`, `ScheduledOrderTransition.cs`, `PaymentWindowSources.cs`
- `tests/Shopfolio.UnitTests/Architecture/ShownVersionSourceScan.cs`, `ShownVersionSourceScanTests.cs`, `ViolationSamples/Concurrency/ShownVersionComparisons.cs` (yeni)
- `tests/Shopfolio.IntegrationTests/Ordering/PaymentTransitionsTests.Concurrency.cs` (yeni), `PaymentTransitionsTests.cs` (partial), `PaymentWindowTests.cs`, `OrderCorrectionTests.Interventions.cs`, `OrderingTestServices.cs`
- `Docs/09_CODING_GUIDELINES.md` (§10.2 m.3, §18.2 m.3)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Sessiz üzerine yazma yok; ikinci kaydeden uyarılır | ✓ | `ConcurrencyTests.RecordSave_IsOneConditionalUpdate_AndOfTwoSavesOfTheSameVersionOneFails_KK1`, `RecordUpdate_WithAStaleVersion_AffectsNoRow_KK1`; sayfa: `VersionedFormPageTests.RecordPost_WithAnOldVersion_Is409_WritesNothing_ShowsTheSentValuesTheCurrentVersionAndOneButton_KK4`, `TwoSavesOfTheSameVersion_ThatArriveTogether_OneSavesAndTheOtherIsRefused_KK4` |
| 2 | Kural içerik, ürün ve ayarda aynı | ✓ | İçerik `ContentAuthoringTests.TheAboutPageEdit_RaisesTheVersion_AndAStaleFormIsAConflict_KK3`; ürün `ProductTransitionsTests.AScreenOfAnOlderVersion_IsRefused_AndNothingIsWritten`, `CompanyIdentityTests.Save_WithAnOldVersion_IsAVersionConflict_AndWritesNothing`; ayar `FirmSettingsTests.AStaleVersion_IsAConflict_AndWritesNeitherTheValuesNorATrailRow`, `BrandingTests.AStaleVersion_IsAConflict_AndWritesNeitherTheValuesNorATrailRow` — hepsi tek `RecordSave.SaveAsync` yolu |
| 3 | Siparişte işlem güncel hâle karşı; önce tamamlanan uygulanır | ✓ | `PaymentTransitionsTests.TwoAdministrators_MarkTheSameTransferPaidAtOnce_…_T364_KK3_KK5`, `TwoAdministrators_ProcessTheSameTransferRefundAtOnce_…_T364_KK3_KK5`, `OrderCorrectionTests.TwoAdministrators_RemoveTheSameItemAtOnce_…_T364_KK3_KK5`; `ConcurrencyTests.AdministratorOperation_ThatSawAnOlderVersion_IsNotApplied_EvenWhenItWaitedForTheLock_KK4` |
| 4 | Gördüğü hâlden sonra değiştiyse uygulanmaz; panel söyler, güncel hâli gösterir | ✓ | `VersionedFormPageTests.OrderPost_ByTheAdministrator_WithAnOldVersion_IsNotApplied_Is409_ShowsTheCurrentOrder_AndHasNoWayToOverwrite_KK6`, `OrderPost_ByTheAdministrator_SentTwice_TheSecondIs409_KK8`; karşılaştırma tek yerde: `ShownVersionSourceScanTests` |
| 5 | Aynı geçiş, geri ödeme kaydı, kalem çıkarma iki kez işlenmez | ✓ | KK3 satırındaki üç eşzamanlı test (tek O1 + tek ödeme kaydı; tek geri ödeme; tek `ItemRemoval` + tek geri ödeme + tek iz + tek B-11) |
| 6 | Müşteri ve sistem çakışmasında sonraki uygulanır ya da sebebi söylenerek reddedilir; kilit yok | ✓ | `PaymentTransitionsTests.SystemAndCustomerCancellation_AfterTheAdministratorsPaidMark_AreRefused_OnlyTheSystemsIsLogged_NoNotice_NoLockRemains_T364_KK6`, `ProviderResults_ThatNoLongerFitTheOrder_AreRefused_WithOneLogLineEach_AndNoNotice_T364_KK6`; `PaymentWindowTests` (ödenmiş siparişte süre dolumu tek satır); müşteri: `VersionedFormPageTests.OrderPost_ByTheCustomer_WithAChangedOpenQuantity_IsNotProcessed_AndWithTheShownOneIsProcessed_KK2` |

### Öz denetim (bitiş kapısı 2)

> Önceki işçinin yaptıkları dahil bütün maddeler bu turda yeniden denetlendi.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1: eski sürümle kayıt uyarı verir, sessiz yazmaz | ✓ | `ConcurrencyTests` KK1 iki test; `VersionedFormPageTests` 409 + "Yine de kaydet" testleri |
| KK2: içerik, ürün ve ayarda aynı davranış | ✓ | Kriter 2 satırındaki beş test; hepsi `RecordSave.SaveAsync` |
| KK3: iki yönetici aynı işlemde yalnız biri uygulanır | ✓ | Üç `Task.WhenAll` testi (ödendi işareti, havale geri ödemesi, kalem çıkarma) |
| KK4: değişmiş siparişte ret ve güncel hâl | ✓ | `VersionedFormPageTests.OrderPost_…_KK6`, `…SentTwice_…_KK8`; `ShownVersion.Matches` her yerde + tarama |
| KK5: çift geçiş, çift geri ödeme kaydı, çift kalem çıkarma yok | ✓ | Üç eşzamanlı test; satır sayımları (TransitionRecords, PaymentRecords, Refunds, ItemRemovals, AuditLogs, DispatchRecords) |
| KK6: müşteri ve sistem çakışmasında sebep söylenir, kilit yok | ✓ | İki T364_KK6 testi; `PaymentWindowTests`; `FOR UPDATE NOWAIT` kanıtı |
| `09 §25` doküman uyumu: yeni ad belgeden | ✓ | `AutomaticTransitionRefused` `09 §18.2 m.3` listesinde; davranış `02 §3.31.2` (K-1559) ve `05 §9.1.3` ile aynı — belge kuralı değişmedi, karar kaydı gerekmedi |
| `09 §25` tek işlev | ✓ | Karşılaştırma yalnız `ShownVersion.Matches`; kayıt satırı yalnız `AutomaticTransitionLog` |
| `09 §25` §3.5 yeni sabit | ✓ uygulanamaz | Yeni ayar/süre/tavan yok |
| `09 §25` modül sınırı, `SaveChanges` | ✓ | Arayüz yöntemleri değişmedi; mimari testler yeşil (Unit 5762/5762) |
| `09 §25` kayıtta kişisel veri yok | ✓ | Satır yalnız olay, kod, iç kimlik; test satırı biçimini birebir sınar |
| `09 §25` eşzamanlılık | ✓ | Kilit → yükleme → `ShownVersion.Matches` sırası korunur; yeni kilit yok |
| `09 §25` yeni kural için tarama + ihlal örneği | ✓ | `ShownVersionSourceScanTests.Scan_OnTheSample_…` (16 satır kırmızı, 5 biçim temiz), modül kapsamı ve dışı testleri |
| `09 §25` test katmanı | ✓ | Eşzamanlılık gerçek PostgreSQL'de; gerçek saat yok (`TimeProvider`), birim testi dosyaya yazmaz |
| `09 §25` göç / şema | ✓ uygulanamaz | Göç yok |
| Güvenlik taraması | ✓ | Yeni uç, POST, sayfa, sır yok; kayıt satırında kişisel veri yok |
| sik-fail (1)(2) tarama istisnası/çağrı biçimi | ✓ | İzinli dosya yok; çok satırlı, parantezli, `CompareTo`, `EqualityComparer`, özellik örüntüsü biçimleri örnekte |
| sik-fail (4) aynı şeyi anan öteki yerler | ✓ | Olay listesi tek yerde (`09 §18.2 m.3`) ve `LogEvents`; tarama cümlesi `09 §10.2 m.3` |
| sik-fail "Integration TAM" (modül kaydına bağımlılık) | ✓ | `AddLogging` + yeni kurucu bağımlılığı: Integration ad alanı ad alanı TAM koşuldu (aşağıda) |
| sik-fail "format koşulmadan temiz" | ✓ | `dotnet format --verify-no-changes` değişen bütün dosyalarda çıkış 0 |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests -c Release -- --max-threads 3` (+ Debug, son değişiklik sonrası) | 5762/5762 |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests --filter-namespace …` (ad alanı ad alanı, Postgres 18, `track_commit_timestamp=on`) | 3444/3444 — Ordering 605, Model 1404, Web 594, CorporateContent 185, Files 126, Catalog 82, Jobs 77, Commands 70, Disposal 61, Administration 46, Database 36, Security 34, Logging 30, Concurrency 26, FileSecrets 24, Outages 19, Payment 14, SignedEndpoints 11 |
| Derleme | ✓ | `dotnet build Shopfolio.slnx -c Release -warnaserror` | 0 uyarı, 0 hata |
| Biçim | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes --include <değişen dosyalar>` | çıkış 0 |
| Contract | uygulanamaz | — | Sözleşme değişmedi |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | *(doğrulayıcı doldurur)* |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok (`Microsoft.Extensions.Logging` çatının parçası)

## Commit & PR

- **Branch:** `task/T364-eszamanli-islemler`
- **Commit:** son commit PR'da
- **PR:** *(açılınca)*
- **CI:** *(doğrulayıcı)*
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok.

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | — | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| — | Ertelenen kalem yok | — | — |

## Notlar

- **⚠ Kararlar (kurala en uygun seçenekle verildi):**
  1. `AddLogging()` modül kaydında (TryAdd): komut ve test düzenekleri host'suz servis kümesi kurar; host'un kaydını değiştirmez. Etkisi Integration TAM koşusuyla sınandı.
  2. Satır düzeyi Information: `Shopfolio.*` kategorisinin varsayılanı bilgidir (`09 §18.1 m.4`); red olağan bir olaydır, hata değildir.
  3. Zamanlayıcının vadesi geçmemiş bulduğu sipariş ve sağlayıcı sonucunun artık uymadığı kayıt `ConditionNotMet` koduyla yazılır (geçiş biriminin var olan kodu; yeni kod açılmadı).
  4. Sağlayıcının aynı ödemeyi yeniden bildirmesi (`isAlreadyApplied`) ret değil, tekrar korumasıdır (K-970); satır yazmaz.
  5. Tarama yerel değişkene kopyalanıp karşılaştırılan sürümü göremez (metin taramasının sınırı); sürümü okumak serbesttir, kopya tek başına bulgu sayılmadı.
- **Working tree (Adım -1):** temiz.
- **Ana dal CI startup (Adım 0):** önceki işçi: 42712574041, 42712573916 success.
- **Main birleştirmesi:** `origin/main` `206f918c` (T390) dala birleştirildi (merge, rebase değil); çakışma yok.
- **Repo memory (bitiş kapısı 9):** parça dosyası kuralı gereği MEMORY'ye dokunulmadı.
- **Devam:** yapım iki işçide yapıldı (ilki bağlam sınırında bölündü, devir notuyla sürdü).
