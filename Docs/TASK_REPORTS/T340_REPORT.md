# T340 — Dijital ürün: dosya, indirme hakkı, hak yenileme, yeniden satın alma

**Faz:** F3 | **Durum:** ⏳ Doğrulama bekliyor | **Tarih:** 2026-11-08

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **İndirme sayacının tek yazıcısı — `DownloadCounters`:** `TakeAsync` tek koşullu güncelleme (dijital kalem, dondurulmuş hak dolu değil, siparişin ödemesi onaylı; okuyup yazma yok), reddin nedeni aynı işlemde okunur (`AllowanceUsed` ya da `NotAvailable`). `RenewAsync` sipariş satır kilidi altında, ekrandaki sürüme karşı sayacı sıfırlar ve siparişin sürümünü bir artırır; hakka dokunmaz, bildirim ve denetim izi yazmaz. İkisi de çağıranın işleminde çalışır ve işlem yoksa atar (`09 §9.1` madde 3).
- **`DownloadRules`:** ödeme onayı tek okuma (`Paid` ya da `PartiallyRefunded`; sorgu koşulu ve bellek karşılığı), dijital adet sabiti (`DigitalQuantity` = 1) ve `IsQuantityAllowed`.
- **Modül yüzü:** `IOrderingModule.TakeDownloadAsync`, `RenewDownloadAllowanceAsync`, `CheckDigitalAddAsync`; `FindDownloadableItemAsync` artık ödemesi onaylı siparişi şart koşar (indirme ucu onaysız kalemde 404). `Catalog` tarafında `IDigitalDelivery.IssueLinkAsync`: kalem ve dosya kontrolü, sonra tek koşullu güncellemeyle hak, sonra bir saatlik bağlantı; dosya yoksa hak harcanmaz (`NoFile`).
- **Tarama kuralları:** `ConcurrencySourceScan` indirme sayacını (`DownloadCounterProperties`, `DownloadCounters.cs` tam yolu) ayrı grup olarak kapsar; `VersionedSideBulkWriteScan` `DownloadCounters.cs`'i izinli yer yapar ve dosyanın yalnız iki `ExecuteUpdate`, yalnız `DownloadCount` yazdığını bir testle kilitler. D-94 (1) kapandı.
- **Test altyapısı:** `FileEndpointsHost.SeedDownloadAsync` siparişi varsayılan `Paid` yapar (ödeme durumu ve hak parametreli), `SeedPurchaseAsync`, `SetPaymentAsync`, `InTransactionAsync`, `InScopeAsync` eklendi; `OrderingModule`'ü elle kuran dört yardımcıya (`DisposalHarness`, `SchedulerHarness`, `AddressTests`, `MembershipTests`) `DownloadCounters` kaydı eklendi.
- **Backlog:** D-507 (sepete ekleme çağıranları T503/T508), D-508 (sipariş sayfası düğmesi T511, onay anı teslim T351, donma T355), D-509 (panel yenileme ucu T531), D-510 ve D-511 (belge boşlukları, karar kaydı bekleyen iki yorum). D-94 yerinde kapatıldı.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Ordering/DownloadCounters.cs`, `DownloadRules.cs`, `OrderingModule.cs`, `Contracts/DownloadContracts.cs`, `Contracts/IOrderingModule.cs`
- `src/Shopfolio.Web/Modules/Catalog/DigitalDelivery.cs`, `CatalogModule.cs`, `Contracts/IDigitalDelivery.cs`
- Testler: `tests/Shopfolio.UnitTests/Ordering/DownloadRulesTests.cs`, `Architecture/ConcurrencySourceScan(Tests).cs`, `Architecture/VersionedSideBulkWriteScan(Tests).cs`, `Architecture/ViolationSamples/Concurrency/LockAndCounterWrites.cs`; `tests/Shopfolio.IntegrationTests/Files/DigitalDeliveryTests.cs`, `Files/FileEndpointsHost.cs` ve dört kayıt yardımcısı
- Belgeler: `DEFERRED_BACKLOG.md` (D-94 yerinde), `DEFERRED_BACKLOG.d/T340.md`, `IMPLEMENTATION_STATUS.md` (T340 satırı), `STATUS_CHANGELOG.d/T340.md`, bu rapor

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | İndirme yalnız ödemesi onaylı kalemde açılır; onay anı teslimi T351, düğme T511 | ✓ | `DownloadRules.PaymentConfirmed`; `FindDownloadableItemAsync` ve `TakeAsync` bu koşulu taşır; Integration `Download_OfAnItemWhosePaymentIsNotConfirmed_IsRefusedEverywhere_AndTakesNothing_KK1` (Pending/Failed/Refunded), `Download_OfAnItemWhosePaymentIsConfirmed_Opens_KK1` (Paid/PartiallyRefunded), `Download_WhenTheTransferIsMarkedPaidLater_OpensFromThatMoment_KK1`; çağıranlar D-508 |
| 2 | Dosya ürün düzleminde; varyant kendi dosyasını taşır, taşımıyorsa ürününkini devralır | ✓ | `DigitalFileReader.FindAsync` (T312) `IssueLink` yolunda kullanılır; Integration `Download_OfAVariantWithoutAFile_GivesTheProductsFile_KK2` |
| 3 | Dosya boyut tavanı ürün sabitinden okunur (P-26) | ✓ | T312'den: `DigitalFileDelivery.CheckSize` + `ProductConstants.DigitalFileMaxBytes`; `UploadEndpointsTests.Start_WithASizeAboveTheCeiling_Answers422WithTheCeiling_AndOpensNoRow` (+1 bayt ret), `DigitalFileDeliveryTests` ve `DiskFileStoreTests` sınır testleri; bu task'ta değişmedi |
| 4 | Dijital kalemde adet her zaman 1 | ✓ | `DownloadRules.DigitalQuantity`/`IsQuantityAllowed`; `CheckDigitalAddAsync`; veritabanı kısıtı `quantity_above_zero_one_if_digital` (T-önceki); Unit `IsQuantityAllowed_ForADigitalVariant_IsOnlyTheDigitalQuantity`; Integration `CheckDigitalAdd_RefusesAQuantityAboveOne_AndTheSameVariantTwiceInACart_KK4`; sepet yazımı çağıranı D-507 |
| 5 | Adet sınırı kalem başına, süre sınırı yok; hak dolunca kapanır; donma T355, düşüş anı T511 | ✓ | `DownloadCounters.TakeAsync` (sayaç < dondurulmuş hak); Integration `Take_WhenTheAllowanceIsUsed_IsRefused_AndNoTimePassingChangesIt_KK5` (800 gün ilerletilir, sonuç aynı), `Take_TenRequestsAtOnceOnAnAllowanceOfThree_TakeExactlyThree_KK5`; hak yazımı ve düğme D-508 |
| 6 | Yönetici kalemin hakkını yeniler, sayaç sıfırlanır | ✓ | `DownloadCounters.RenewAsync`; Integration `Renew_PutsTheCounterBackToZero_RaisesTheOrdersVersionByOne_AndTheBuyerCanDownloadAgain_KK6`, `Renew_WithAStaleVersion_OnANonDigitalItem_OrAMissingOne_IsRefused_AndChangesNothing_KK6`; panel ucu D-509 |
| 7 | Sipariş sayfası varyantın o anki dosyasını verir; ürün silinse ya da dosya yenisiz kalksa son hâl | ✓ | `DigitalFileReader.FindAsync` (T312); Integration `ReplacedFile_IsWhatTheEarlierBuyerGets_...KK7_KK8`, `RemovedFileWithoutANewOne_AndADeletedProduct_GiveTheLastFile_KK7`; `DownloadEndpointTests` (T312) değişen dosya testleri |
| 8 | Dosya güncellemesi hakkı değiştirmez; yeni hâli indirmek haktan bir indirme harcar | ✓ | Integration `ReplacedFile_..._KK8`: değişimden sonra `download_count` ve `download_allowance` aynı, sonraki `IssueLink` sayacı 1 artırır |
| 9 | Üyeye uyarı, misafire yok, satın alma engellenmez | ✓ | `OrderingModule.CheckDigitalAddAsync`; Unit `DigitalAddCheck_BlocksOnAQuantityOrADuplicate_AndWarnsOnlyWhenNothingBlocks`; Integration `CheckDigitalAdd_WarnsAMemberWhoBoughtItBefore_AndNeverStopsTheAdd_KK9`, `..._GivesAGuestNoWarning_...`, `..._GivesNoWarning_WhenTheEarlierOrderWasNotConfirmed_OrBelongsToAnotherMember_KK9`; ölçüt yorumu D-511 |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| Kriter 1–9 (yukarıdaki tablo) | ✓ | tablodaki test adları; önceki işçinin kodu (sayaç yazıcısı, kurallar, modül yüzü) dahil diff'te tek tek denetlendi |
| KK1 onaysız reddedilir, onaylıda açılır | ✓ | `..._KK1` üç test; endpoint 404 gövdesiz (Pending/Failed/Refunded) |
| KK2 varyant dosyasızsa ürünün dosyası | ✓ | `Download_OfAVariantWithoutAFile_GivesTheProductsFile_KK2`; dosya hiç yoksa `IssueLink_WhenThereIsNoFileToGive_SpendsNoRight_KK2` |
| KK3 tavanı aşan dosya reddedilir | ✓ | `UploadEndpointsTests.Start_WithASizeAboveTheCeiling_...` (T312, bu task'ta yeniden koşuldu), `DigitalFileDeliveryTests` sınır testi |
| KK4 adet 1'den fazla yazılamaz | ✓ | `CheckDigitalAdd_RefusesAQuantityAboveOne_..._KK4` |
| KK5 hak dolunca ret, süre kontrolü yok | ✓ | `Take_WhenTheAllowanceIsUsed_..._KK5`, on eşzamanlı istekte tam üç |
| KK6 yenilemede sayaç sıfırlanır | ✓ | `Renew_PutsTheCounterBackToZero_..._KK6` |
| KK7 dosya değişince yeni dosya; ürün silinince son dosya | ✓ | `ReplacedFile_..._KK7_KK8`, `RemovedFileWithoutANewOne_AndADeletedProduct_GiveTheLastFile_KK7` |
| KK8 güncelleme hakkı artırmaz | ✓ | `ReplacedFile_..._KK8` sayaç ve hak değişmez |
| KK9 üyede uyarı, misafirde yok, engel yok | ✓ | üç KK9 testi |
| `09 §25` ilgili satırlar: tek yazıcı, tek koşullu güncelleme | ✓ | `ConcurrencySourceScanTests` (yeni üç test ve genişleyen örnek), `VersionedSideBulkWriteScanTests.TheDownloadCountersPlace_...`; sayaç yalnız `DownloadCounters.cs`'ten yazılır |
| Sık FAIL: geniş bekçi istisnası | ✓ | izinli yer tam yol; aynı adlı başka dosya bulgu (`Scan_AFileOfTheSameNameAsDownloadCountersElsewhere_...`); stok yazıcısında indirme yazımı, indirme yazıcısında stok yazımı hâlâ bulgu; `DownloadCounters.cs` yalnız iki `ExecuteUpdate` ve yalnız `DownloadCount` |
| Sık FAIL: kaçan çağrı biçimi | ✓ | ihlal örneği üç biçimi kapsar (`SetProperty`, ham `UPDATE ... download_count`, `EF.Property` adı); okuma ve öteki sütun örnekleri bulgu değil |
| Sık FAIL: DB default / FK dizini | ✓ | şema değişmedi (migration yok); `download_count` varsayılanı (0) ve `quantity_above_zero_one_if_digital` önceden vardı |
| Sık FAIL: aynı sayıyı anan öteki yerler | ✓ | `OrderingModule`'ü elle kuran dört yardımcıya kayıt eklendi (`DisposalHarness`, `SchedulerHarness`, `AddressTests`, `MembershipTests`); tarama sınıfı özeti ve izinli yer listeleri güncellendi |
| Sık FAIL: karar kaydısız 09 metni | ✓ | `09` değişmedi; belge boşluğu olan iki yorum D-510 ve D-511'de |
| Sık FAIL: modül yöntemi kendi işlemini açmıyor, `SaveChanges` çağırmıyor | ✓ | `RequireTransaction`; `DownloadCounters` `SaveChanges` içermez (testle kilitli) |
| Sık FAIL: eşzamanlılık / kilit sırası | ✓ | `RenewAsync` yalnız sipariş kilidini alır (tek kilit); `TakeAsync` kilit almaz, tek koşullu güncelleme |
| Doğrulama listesi `12`: VAL-A064, A065, A066, A201, A833, A491, A552, D010, D115 | ✓ (modül düzeyi) | uç düzeyi T511/T531; veri kaydı: yeni tablo yok (D115), hak sayıları testlerde |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --filter-not-trait "Consistency=true"` | 4369/4369 |
| Unit (Consistency) | ✓ | `--filter-trait "Consistency=true"` | 277/277 |
| Integration | ✓ | `SHOPFOLIO_TEST_DATABASE` (docker `t340-pg`, postgres 18.6) ad alanı ad alanı | `DigitalDeliveryTests` 21/21; Files 102, Ordering 185, Concurrency 26, Model 1291, Catalog 72, Disposal 61, Jobs 77, kalan 810; hepsi yeşil |
| Contract | — | — | bu task'ta sözleşme testi yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ⏳ bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T340-dijital-urun`
- **Commit:** `ce0707b` — T340: Dijital ürün: indirme hakkı, hak yenileme, yeniden satın alma kontrolü, testler ve kayıtlar
- **PR:** #462
- **CI:** yönetici izler
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; bu satır ayrı bir `chore:` PR'ında veya sonraki task dalında güncellenir, doğrudan ana dala push edilmez)*

## Dış Varsayımlar

Dış varsayım: yok. Veritabanı yerel docker (`t340-pg`, postgres 18.6); dış servis yok.

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Yok | Integration testleri yerel PostgreSQL'e karşı koştu | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Sepete ekleme çağıranları (adet, çift ekleme, yeniden alma uyarısı) | D-507 | Sepet sayfaları T503/T508 |
| 2 | Sipariş sayfası düğmesi, onay anı teslim, hak donması | D-508 | T511, T351, T355 |
| 3 | Panel hak yenileme ucu | D-509 | T531 |
| 4 | "Ödemesi onaylı" ve yenileme denetim izi metni | D-510 | belge geçişi, F3 gate |
| 5 | "Daha önce alınmış" ölçütü | D-511 | belge geçişi, F3 gate |

## Notlar

- **Working tree (Adım -1):** temiz; dal `origin/task/T340-dijital-urun` başından (`8a5fbc3`) sürdürüldü, `origin/main` birleştirildi (T338 sonrası).
- **Ana dal CI startup (Adım 0):** `gh run list --branch main` son üç: 42466502346 failure, 42466502303 failure, 42464077677 success. İlk ikisinin nedeni bu task'ın dışında (main'in kendi koşuları); teşhis edilmedi, yönetici bilgisine.
- **⚠ kararlar:** ödeme onayı = Paid | PartiallyRefunded (Refunded onaylı değil); yenileme denetim izine yazılmaz ve ödeme onayı aramaz; "daha önce alınmış" = aynı müşterinin ödemesi onaylı siparişinde o varyantlı kalem; dosya yoksa hak harcanmaz (`NoFile`); `IDigitalDelivery`, `ICatalogModule`'e eklenmedi (ICatalogModule'ü kullanan testlerde `IOrderingModule` yok), ayrı kayıtla çözülür.
- **Repo memory (bitiş kapısı 9):** parça dosyaları kuralıyla MEMORY "Güncel Durum" ve STATUS "Son güncelleme" satırına dokunulmadı.
