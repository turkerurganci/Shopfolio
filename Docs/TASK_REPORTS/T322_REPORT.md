# T322 — İçerik yayın durumları ve iletişim talebi durumları

**Faz:** F3 | **Durum:** ⏳ Yapım tamamlandı, doğrulama bekliyor | **Tarih:** 2026-11-07

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- `ContentRecords.cs`: yedi içerik türü (`ContentRecordKind`), `IContentRecord` (yedi varlık uygular), kapı alanı tablosu `ContentGate` (tür başına zorunlu alanlar, `02 §5.2`); yalnız boşluk içeren değer boş sayılır.
- `ContentPublishing.cs`: `PublishAsync` (zorunlu alan eksikse eksik alanlarla ret, ikinci onay yok), `UnpublishAsync` (Taslak'a döner, arşiv yok), `SaveEditAsync` (yayındaki kaydın kapıyı bozan düzenlemesi kaydedilmez; taslak her hâliyle kaydedilir; sürüm uyuşmazlığı reddedilir), `DeleteAsync` (Hakkımızda hiçbir koşulda silinmez; öteki kayıtlar yalnız "geri alınamaz" uyarısı onaylanırsa silinir). Önbellek alanları (menü, anasayfa blokları, site haritası) işlem sonrası düşer.
- `ContentVisibility.cs`: Taslak sitede yok, yönetici "Taslak" bandıyla önizler; yayındaki duyuru tarihleri girilmişse yalnız aralıkta görünür (iki uç dahil, boş tarih sınır yok).
- `ContactRequests.cs`: talep `Açık` doğar ve F-2 bildirimi aynı işlemde yazılır (yalnız konu; ad, e-posta, telefon, ileti yok); `CloseAsync` / `ReopenAsync` yalnız yönetici kimliğiyle, bildirim üretmez, yinelenen çağrı değişiklik yazmaz; `ContactRequest.RetentionStart` son kapatılıştan, hiç kapatılmamışsa açılıştan sayar (`CurrentStateWrite` ile durum yazımı).
- `CorporateContentModule.cs` kaydı; `CorporateContentRecordsGuard` (SaveChanges kesicisi) mevcut.
- Main birleşmesinden sonra (T332, T381, T315 vb.) derleme ve testler yeniden koşuldu.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/CorporateContent/`: `ContentRecords.cs`, `ContentVisibility.cs`, `ContentPublishing.cs`, `ContactRequests.cs`, `CorporateContentModule.cs`, `ContactRequest.cs` (`RetentionStart`)
- Testler: `tests/Shopfolio.UnitTests/CorporateContent/` (57), `tests/Shopfolio.IntegrationTests/CorporateContent/` (`ContentStatusTests`, `ContactRequestStatusTests`; 45)
- Kayıt: `Docs/DEFERRED_BACKLOG.md` (D-315…D-317), `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `.claude/memory/MEMORY.md`, `.claude/CONTEXT.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Taslak ve Yayında; arşiv, ileri tarihli yayın, sürüm geçmişi yok | ✓ | `TheArchive_IsNotAContentStatus_KK1`, `EverySevenContentTables_AcceptOnlyDraftAndPublished_AndNoVersionTableExists_KK1`, `TheContentTables_HaveNoVersionHistoryRevisionOrArchiveTable_KK1` |
| 2 | Yayına geçiş yalnız zorunlu alanlar doluysa; düzenleme anında yayında | ✓ | `Publish_ARecordWithAnEmptyGateField_IsRefusedWithTheMissingFields_AndWritesNothing_KK2`, `Edit_APublishedRecord_ThatKeepsTheGate_IsSaved_AndIsOnTheSiteAtOnce_KK3` |
| 3 | Kapıyı bozan düzenleme kaydedilmez | ✓ | `Edit_APublishedRecord_ThatEmptiesAGateField_IsNotSaved_AndTheRecordStaysAsItWas_KK3` |
| 4 | Taslak sitede yok, yönetici bant görür | ✓ | `ADraft_IsNotOnTheSite_AndTheAdministratorPreviewsItWithTheBand_KK4`, `ADraft_IsNotOnTheSiteMap_APublishedRecordIs_AndTheWayBackTakesItOff_KK4` |
| 5 | Duyuru tarihleri girilmişse yalnız aralıkta görünür | ✓ | `TheRange_HoldsBothEndDays_AndAnEmptyDateIsNoLimit_KK5`, `APublishedAnnouncementOutsideItsDates_IsNotShownToAnyone_EvenThoughItIsPublished_KK5` |
| 6 | Hakkımızda silinmez, taslağa alınır; öteki silme uyarılı | ✓ | `TheAboutPage_IsNeverDeleted_EvenWhenConfirmed_AndCanBePutBackToDraft_KK6`, `ADeletion_WithoutTheIrreversibleWarningConfirmed_IsRefused_AndTheRecordStays_KK6` |
| 7 | Talep Açık ve Kapatıldı; ayrı durum makinesi, süre sayacı yok | ✓ | `TheStatus_HasTwoValues_AndNoOther_KK7`, `TheStatusColumn_AcceptsOnlyOpenAndClosed_AndNoOtherStateOrCounterColumnExists_KK7` |
| 8 | Açık doğar, F-2 firmaya; yalnız yönetici kapatır/açar, bildirim yok | ✓ | `ARequest_IsBornOpen_AndTheNoticeF2GoesToTheFirmInTheSameTransaction_KK8`, `Close_AndReopen_ProduceNoNotice_AndTheFirmHearsOnlyOfTheBirth_KK8`, `ACallWithoutAnAdministratorsIdentity_IsRefused_BeforeAnythingIsWritten_KK8` |
| 9 | Saklama son kapatılıştan, kapatılmamışta açılıştan | ✓ | `TheRetention_RunsFromTheLastClosing_AReopenedAndClosedRequestFromTheNewOne_AnOpenReopenedFromTheOldOne_KK9` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| k1 iki durum, arşiv/sürüm yok | ✓ | yukarıdaki kriter 1 testleri (`UnitTests/CorporateContent`, `IntegrationTests/CorporateContent/ContentStatusTests`) |
| k2 zorunlu alan kapısı | ✓ | `TheGate_IsTheTableOfTheRequirementsDocument_PerKind_KK2`, `EmptyingEachRequiredField_ReportsExactlyThatField_AndNoOther_KK2`, `Publish_ACompleteRecordOfEveryKind_NeedsNoSecondApproval_KK2` |
| k3 bozucu düzenleme | ✓ | `Edit_*_KK3` (üç test) |
| k4 taslak/bant | ✓ | `ADraftAnnouncement_IsPreviewedWhateverItsDates_AndNeverShownToAVisitor_KK4`, `APublishedRecord_IsOnTheSite_ForEveryone_WithoutTheBand_KK4` |
| k5 duyuru aralığı | ✓ | `TheRange_HoldsBothEndDays_...` (iki uç dahil, sınır değerle) |
| k6 silme | ✓ | `ADeletion_AfterTheWarningIsConfirmed_RemovesTheRecord_OfEveryKindButTheAboutPage_KK6` |
| k7 talep iki durum | ✓ | KK7 testleri |
| k8 doğuş F-2, bildirimsiz kapatma | ✓ | `TheRequestAndItsNotice_GoTogether_WhenTheTransactionIsRolledBack_KK8`, `TwoAdministratorsClosingTogether_WriteOnce_KK8` |
| k9 saklama başlangıcı | ✓ | KK9 üç birim testi + entegrasyon testi |
| KK1 üçüncü durum ve sürüm tablosu yok | ✓ | k1 ile aynı |
| KK2 eksik zorunlu alanda yayın reddi | ✓ | k2 ile aynı |
| KK3 yayındaki kaydın bozucu düzenlemesi | ✓ | k3 ile aynı |
| KK4 taslak sitede yok, yöneticide bant | ✓ | k4 ile aynı |
| KK5 süresi dolmuş duyuru görünmez | ✓ | `APublishedAnnouncementOutsideItsDates_...KK5` |
| KK6 Hakkımızda'da silme yok, taslağa alma var | ✓ | k6 ile aynı |
| KK7 iki durum dışında değer yok | ✓ | veritabanı kısıtı testi KK7 |
| KK8 doğuşta F-2, kapatmada bildirim yok | ✓ | k8 ile aynı |
| KK9 yeniden açılıp kapatılan / açık duran talebin süresi | ✓ | `AReopenedRequest_KeepsCountingFromTheOldClosing_UntilItIsClosedAgain_KK9`, `AClosedRequest_CountsItsRetentionFromTheLastClosing_KK9` |
| `09 §25` satırları (işlem kuralı satırı dahil) | ✓ | `IOperationRule` / `[GatedOperation]` satırı bu işlemlere uygulanmadı: `09 §25` satırı ve `09 §8.1` madde 1 bu kalıbı `03 §1.11`'in 41 sipariş işlemine bağlar (T323); içerik yayını ve talep kapatma o listede değil, `Orders` kilidi/sürümü kapısı yok — ⚠ 1. Arayüz yönteminde ayrı işlem açılmıyor (`DeleteAsync`'in `SaveChangesAsync` çağrısı modül içi, çağıranın işlemine katılır) |
| `12` ilgili kontrol listesi, mini güvenlik | ✓ | yeni paket yok; ham SQL yok; talep kapat/aç yönetici kimliği ister; F-2 kişisel veri taşımaz (`TheNoticeF2_CarriesTheTopicOnly_...KK8`) |
| Sık FAIL (1) bekçi/tarama istisnası geniş | ✓ | yeni bekçi/tarama istisnası yok |
| Sık FAIL (2) eşitlik denetimi tek yönlü | ✓ | `TheKinds_AreExactlyTheRecordsThatImplementTheContentRecordInterface_KK1` (iki yönlü), `TheGate_IsTheTableOfTheRequirementsDocument_PerKind_KK2` |
| Sık FAIL (3) DB varsayılanı / FK dizini | ✓ | şema ve göç değişmedi |
| Sık FAIL (4) aynı sayıyı anan yerler | ✓ | backlog aktif kalem 125 → 128 (D-315…D-317); STATUS satırı ⏳, changelog, MEMORY, CONTEXT güncellendi |
| Sık FAIL (5) test beklenen değeri sabitten türetmiyor | ✓ | kapı alanları tablodan (`ContentGate`), tür listesi arayüzü uygulayan türlerden türetilir; teorilerde tür adı `ContentRecordKind`'tan gelir |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit (Consistency) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/... --no-build --filter-namespace Shopfolio.UnitTests.Consistency` | 230 geçti, 0 kaldı (main birleşmesi sonrası) |
| Unit (Architecture) | ✓ | aynı, `...Architecture` | 906 geçti, 0 kaldı |
| Unit (CorporateContent) | ✓ | aynı, `...CorporateContent` | 57 geçti |
| Unit (tamamı, önceki işçi) | ✓ | — | 3956 geçti |
| Integration | ✓ | `--filter-namespace Shopfolio.IntegrationTests.CorporateContent` (yerel PostgreSQL) | 45/45 |
| Lint (yerel) | ✓ | `dotnet format --verify-no-changes` (üç proje, önceki işçi); `dotnet build Shopfolio.slnx --no-restore -warnaserror` (birleşme sonrası) | temiz, 0 uyarı |
| Contract | — | — | task kapsamında yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | Bekliyor |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T322-icerik-iletisim-durumlari`
- **Commit:** son baş SHA PR açıklamasında
- **PR:** PR açıklamasında
- **CI:** doğrulayıcı izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | Dış varsayım: yok | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Denetim izi (`ChangePublishStatus`) ve ilk yayında adres sabitleme çağıran panelde; `ICorporateContentModule`'e yöntem eklenmedi | D-315 | çağıranlar T360/T363'tedir |
| 2 | Duyuru aralığı gün düzeyinde; saat/gün sonu sınırları | D-316 | T378 k7 |
| 3 | `PublishStatus` ayarlayıcısı public, kayıt bekçisi yok | D-317 | mevcut tohum ve testler doğrudan yazıyor |

⚠ (doğrulayıcıya, kolay kararlar):
1. `IOperationRule` / `[GatedOperation]` bu işlemlere uygulanmadı (gerekçe yukarıda); çağıran paneller (T360/T363/T532) yazım yöntemlerini doğrudan çağırır.
2. Duyuru "Taslak"ı yönetici için tarihe bakmadan bantla görünür; yayındaki duyuru tarih dışında herkese gizli.
3. Başlık/ad alanları veritabanında NOT NULL ve boş metin NULL'a çevrilir; "boş kapı" yalnız boşluk içeren değerle ya da bellekte oluşur, testler buna göre.
4. İletişim talebi kapat/yeniden aç yalnız `adminId != Guid.Empty` ister (kimlik doğrulaması çağıran panelde).
5. Önceki task'ların (T315 vb.) Commit hücresi ve Post-merge satırları bu dalda doldurulmadı.
