# T115 — Şube, içerik–ürün bağları ve iletişim talebi

**Faz:** F1 | **Durum:** ⏳ Devam ediyor (yapım bitti, doğrulama bekliyor) | **Tarih:** 2026-11-04

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- `src/Shopfolio.Web/Modules/CorporateContent/` altında dört entity ve yapılandırması (`05 §3.3.11` "Kurumsal içerik" modülü): `Branch` (`06 §4.50`), `OfferingProduct` ve `PortfolioItemProduct` (§4.53), `ContactRequest` (§4.57). Özellik adları alan tablolarıyla birebir; `ContactTopic` ve `ContactRequestStatus` `CorporateContent/Contracts`'ta zaten vardı, `PublishStatus` `Catalog/Contracts`'tan, ortak kural `ContentRules` (T114).
- Veritabanı varsayılanları (`06` "Varsayılan" sütunu, `HasDefaultValue` + göçte `DEFAULT`): `branches.publish_status` = `Draft`, `branches.version` = 1, `contact_requests.contact_request_status` = `Open`.
- Kontrol kısıtları (`06 §7.3`): `branches.phone_ten_digits` ve `contact_requests.phone_ten_digits` (`^[0-9]{10}$`, §2.5.1); `branches.image_alt_text_only_with_image` (`image_id IS NOT NULL OR image_alt_text IS NULL`); `contact_requests.closed_has_last_closed_at` (`contact_request_status <> 'Closed' OR last_closed_at IS NOT NULL`); `branches.publish_status` iki değerli alt küme (`HasAllowedValues`, §2.9.2); `contact_request_status` ve `contact_topic` enum listeleri (`09 §10.1`).
- Dizinler (`06 §7.1`, §7.4, §7.5.2): `branches (PublishStatus, SortOrder)` (tekil değil), `branches.ProvinceId`, `branches.ImageId`; `offering_products` ve `portfolio_item_products` için (içerik, `ProductId`) tekil dizin (içeriğin bağlarını ilk sütunuyla karşılar) ve `ProductId` dizini (ürün silmede basamaklama); `contact_requests (ContactRequestStatus, CreatedAt)`; `contact_requests (ContactTopic, COALESCE(LastClosedAt, CreatedAt))` ifade dizini — EF modelleyemez, göçte elle SQL (`ix_contact_requests_contact_topic_retention_start`, T103/T114 kalıbı).
- Bağlar (`06 §7.2`): `Branch → Province` ve `Branch → Image` kısıtlayıcı (`NoAction`); `OfferingProduct → Offering/Product` ve `PortfolioItemProduct → PortfolioItem/Product` basamaklı; `ContactRequest` hiçbir yere bağlanmaz. Modüller arası bağ (`Province`, `Image`, `Product`) `const string` + `HasOne` kalıbıyla (`09 §10.1` m.1).
- Tek göç `20261104104643_CreateBranchContentLinkAndContactRequestTables` (`Down` yok; LF; BOM'lu): dört tablo + bir elle SQL dizini. `AppDbContext`'e dört `DbSet`.
- Test güncellemeleri: `EnumDatabaseTests` enum sütun sayısı 52 → 55; `ModelRuleTests` kayıtlı basamaklama listesine dört satır (`06 §2.2.5`); `ContentRecordAndBrandingTablesTests` içindeki `TRUNCATE {table}` artık `offerings`/`portfolio_items` için bağ tablosu yüzünden `CASCADE` ister.
- T114 raporunda "Post-merge run'lar" dolduruldu.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/CorporateContent/{Branch,OfferingProduct,PortfolioItemProduct,ContactRequest}.cs`, `Infrastructure/AppDbContext.cs`, `Infrastructure/Migrations/*` (göç, tasarım dosyası, anlık görüntü).
- `tests/Shopfolio.IntegrationTests/Model/{BranchContentLinkAndContactRequestTablesTests,EnumDatabaseTests,ModelRuleTests,ContentRecordAndBrandingTablesTests}.cs`.
- `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `.claude/memory/MEMORY.md`, `Docs/TASK_REPORTS/T114_REPORT.md` (post-merge satırı).

## Kabul Kriterleri Kontrolü

Öz denetim (PR öncesi): her kriter ve kontrol listesi maddesi diff'te göç satırıyla ve testle tek tek karşılaştırıldı; `06 §4.50`, §4.53, §4.57 ile §2.2.5, §2.5.1, §7.1 (5499–5500), §7.2 (5638–5643), §7.3 (5679, 5723, 5727), §7.4 (5813, 5816–5817) göçe karşı okundu. Yabancı anahtar sayısı 6 (hepsinin dizini var: `Branch` iki kendi dizini, bağ tablolarında çift dizininin ilk sütunu ve `ProductId` dizini); varsayılanlı alanlar hem varlıkta hem göçte `DEFAULT` (`Columns_HaveTheDatabaseDefaultsOfTheDefaultColumn`); varlıklar `CorporateContent` modülünde (`05 §3.3.11`); ON DELETE `06 §7.2` ile birebir (`ForeignKeys_AreTheRestrictiveBranchLinksAndTheCascadingContentProductLinks`); tekil dizinler ve kontrol kısıtları tam tanımlarıyla (`Indexes_AreTheDocumentedOnes`, `CheckConstraints_AreTheDocumentedOnes`); bekçi/tarama istisnası eklenmedi.

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Dört entity alan tablolarıyla tek göçle | ✓ | `Migration_MakesTheFourTables`, `Columns_HaveTheTypesAndNullabilityOfTheFieldTables`, `Columns_HaveTheDatabaseDefaultsOfTheDefaultColumn`, `Indexes_AreTheDocumentedOnes`, `ForeignKeys_…`, `CheckConstraints_AreTheDocumentedOnes` |
| 2 | `Branch` ve `ContactRequest` telefonu on rakam | ✓ | `BranchPhone_NineDigits_IsRefusedByTheDigitRule` (`ck_branches_phone_ten_digits`), `ContactRequestPhone_NineDigits_IsRefusedByTheDigitRule` (`ck_contact_requests_phone_ten_digits`), `BranchPhone_NotTenDigits_IsRefused` (7 değer), `Phone_TenDigitsOrEmpty_IsAccepted` |
| 3 | Görselsiz şubede alt metin boş; yayın durumu iki değer; elle sıra tekil değil; il ve görsel bağı kısıtlayıcı | ✓ | `Branch_AltTextWithoutAnImage_IsRefused`, `Branch_AltTextWithAnImage_AndAnImageWithoutAltText_AreAccepted`, `Branch_PublishStatusThirdValueOfTheEnum_IsRefused`, `Branch_PublishStatusUnknownText_IsRefused`, `Branch_PublishStatusDraftAndPublished_AreAccepted`, `Branch_SortOrder_SameValueInTwoRows_IsAccepted`, `DeletingTheImageOfABranch_IsRefused_AndAReplacedImageIsLeftUnlinkedAndDeletable`, `DeletingTheProvinceOfABranch_IsRefused`, `BranchDeleted_LeavesItsImageAndProvince` |
| 4 | İçerik–ürün bağı (içerik, `ProductId`) çiftinde tekil | ✓ | `OfferingProduct_SecondLinkOfTheSameProduct_IsRefused` (`ix_offering_products_offering_id_product_id`), `PortfolioItemProduct_SecondLinkOfTheSameProduct_IsRefused`, `ContentProductLinks_OneProductOnTwoRecords_AndTwoProductsOnOneRecord_AndTheSameOrder_AreAccepted` |
| 5 | Bağ ürün ve içerik kaydıyla basamaklanır | ✓ | `OfferingProduct_GoesWithTheProductAndWithTheOffering`, `PortfolioItemProduct_GoesWithTheProductAndWithThePortfolioItem`, `ForeignKeys_…` (`c`), `ModelRuleTests.ApplicationModel_FollowsTheRules_AndItsCascadeSetEqualsTheRegisteredList` |
| 6 | Kapalı talepte son kapanış anı dolu | ✓ | `ContactRequest_ClosedWithoutALastClosedMoment_IsRefused` (`ck_contact_requests_closed_has_last_closed_at`), `ContactRequest_OpenOrClosedWithTheMoment_AreAccepted_AndReopeningKeepsTheMoment`, `ContactRequest_StatusAndTopicOutsideTheClosedLists_AreRefused` |
| 7 | Göç tek konu; `Down` yok | ✓ | Mevcut `Migration_HasNoDownMethod` mimari taraması yeşil (Unit 245/245); `dotnet ef migrations has-pending-model-changes` → "No changes"; göçte `Down`/`GRANT`/`REVOKE`/`now()`/`suppressTransaction`/`CONCURRENTLY`/`UPDATE`/`DELETE` yok (grep) |

**Doğrulama kontrol listesi:**

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 göç boş veritabanına, dört tablo | ✓ | `Migration_MakesTheFourTables` (test veritabanı bütün göçlerle kurulur) |
| KK2 dokuz haneli telefon | ✓ | `BranchPhone_NineDigits_IsRefusedByTheDigitRule`, `ContactRequestPhone_NineDigits_…` |
| KK3 görselsiz şubede alt metin dolu satır; aynı sıralı iki şube; şubeye bağlı görselin silinmesi | ✓ | `Branch_AltTextWithoutAnImage_IsRefused`, `Branch_SortOrder_SameValueInTwoRows_IsAccepted`, `DeletingTheImageOfABranch_IsRefused_…` |
| KK4 aynı ürünü ikinci kez bağlamak | ✓ | `OfferingProduct_SecondLinkOfTheSameProduct_IsRefused`, `PortfolioItemProduct_SecondLinkOfTheSameProduct_IsRefused` |
| KK5 ürün silmede ve içerik kaydı silmede bağ gider | ✓ | `OfferingProduct_GoesWith…`, `PortfolioItemProduct_GoesWith…` |
| KK6 kapalı ve kapanış anı boş talep | ✓ | `ContactRequest_ClosedWithoutALastClosedMoment_IsRefused` |
| KK7 kaynak taraması | ✓ | Unit'teki mimari/kaynak taramaları yeşil (245/245); diff'te `Guid.NewGuid` (üretimde), `CreateVersion7`, `Restrict`/`Client`, `HasConversion<int>`, `DateTime.Now`, `UtcNow` yok (grep); `Cascade` yalnız `06 §2.2.5`'in basamaklı dört bağında (kayıtlı liste `ModelRuleTests`'te) |
| `09 §25` | ✓ | Enum metin + izinli değer kısıtı (`§10.1`); her FK'nin silme davranışı açık (`NoAction`/`Cascade`); anahtar `EntityIds.New()`; göç genişletme, `Down`/`now()`/`suppressTransaction`/`CONCURRENTLY`/`UPDATE`/`DELETE` yok (`§10.6`); yeni tablolar varsayılan yetkide, `Database` ad alanı testleri yeşil (`§10.8`); yeni kapalı değer ve paket yok; modül sınırı — `Province`, `Image`, `Product`'a yalnız tür adıyla FK |
| Güvenlik taraması | ✓ | Sır yok; yetki/kimlik doğrulama etkisi yok; giriş noktası yok (yalnız şema); form doğrulaması, gizli tuzak alan ve gönderim limiti "Uygulamada" kuralları akış task'larının işi; yeni dış bağımlılık yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests -c Release` | 245/245 (Consistency dahil) |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests -c Release` (PostgreSQL 18, `shopfolio-t115-pg`, `fsync=off`, sınıf grupları halinde) | T115 sınıfı 39/39; `ContentRecordAndBrandingTablesTests`, `ModelRuleTests`, `EnumDatabaseTests` yeşil; öteki sınıflar 551+125+… yeşil. Tek koşuda üç test (`Attempt_OffTheLimitsAxis_IsRefused`, `DisposalFunction_With…`, `PrivilegeMatrix_…`) yerel Npgsql zaman aşımına düştü (D-68 ile aynı altyapı sorunu); üç sınıf tek başına 129/129 |
| Build | ✓ | `dotnet build Shopfolio.slnx -c Release -warnaserror` | 0 uyarı |
| Biçim | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` | çıkış 0 |
| Göç | ✓ | `dotnet ef migrations has-pending-model-changes` | "No changes" |
| Kilit | ✓ | `dotnet restore Shopfolio.slnx --locked-mode` | çıkış 0 |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | Bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Var — dört tablo (genişletme göçü; bir ifade dizini elle SQL).
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T115-sube-icerik-urun-iletisim`
- **PR:** #373
- **CI:** yardımcı izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok (plan katmanı, kota, dış servis, paket sürümü yok; PostgreSQL 18 CI ve yerel konteynerle aynı).

## ⚠ Kararlar

1. **Alt metin "boş" = NULL.** `06 §4.50` "görsel boşken boştur"; kontrol `image_id IS NOT NULL OR image_alt_text IS NULL`. Boş metin (`''`) görselsiz satırda reddedilir; kayıtta boş metnin NULL'a çevrilmesi yazım katmanının işi (T114 `slug` kalıbı).
2. **İfade dizini adı** `ix_contact_requests_contact_topic_retention_start` (`06` ad vermez); EF modelleyemediği için göçte elle SQL, `public.` nitelikli.
3. **`ContactRequest` sürüm ve `Version` taşımaz** (`06 §4.57` "sürüm taşımaz"); `Name`, `Email`, `Message` `text` (uzunluk tavanı yok, `06 §4.57`).
4. **Telefon sütunu `varchar(10)`** (`Company.Phone` kalıbı) + on rakam kuralı; "başında sıfır" gibi biçimler 10 haneli rakam olarak geçer, normalleştirme yazım katmanının işi.
5. **Bağ tablolarında kendi `Id` ve `CreatedAt`** (`ProductCategory` kalıbı, `06 §2.x` ortak alanlar); bağ kümesinin "var olmayan ürünü atla" davranışı uygulama kuralıdır, bu task'ta değil.
6. **`ContactRequest` için ek bekçi yok.** `06 §4.57` "elle silme yoktur" der ama silme imha işiyle (T-sonrası) yapılır; bekçi yazılırsa imha işini kısıtlar, bu yüzden eklenmedi.
7. **`ContentRecordAndBrandingTablesTests`** `TRUNCATE {table}` → `TRUNCATE {table} CASCADE`: `offerings`/`portfolio_items` artık bağ tablosunca başvurulur; test başka bir şeyi gevşetmez.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Bağ kümesinin yazımı (var olmayan ürünün atlanması), sürüm kapsamı, ürün silmenin içerik sürümünü artırmaması | — | İlgili akış task'larının işi; bu task yalnız tabloları kurar |
| 2 | İletişim formunun "Uygulamada" kuralları (görünmez tuzak alan, IP başına limit, koşullu kapatma/yeniden açma, kapalıyken form) ve imha taraması | — | Akış ve imha task'ları |
| 3 | Şube yayın kapısı (ad ve adres), harita adresi biçimi, galeri bağları | — | Akış task'ları; galeri bağları T116 |

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `scripts/git-hooks`.
- **Ana dal CI startup (Adım 0):** `gh run list --branch main`: CI `41815985996` success (head `7fa45cf`); Publish `41815986016` failure — `docker push` sonunda `unknown blob` (kayıt defteri/altyapı, T114 kodundan bağımsız; bu task'ı bloke etmez, sahibe bildirilir); önceki CI `41809802367` ve Publish `41809802191` success.
- **Dış varsayım:** yok (yukarıda).
- **Bağlı backlog:** `grep -n "T115" Docs/DEFERRED_BACKLOG.md` boş — kapanan kalem yok, açılan yok.
- **Repo memory (bitiş kapısı 8):** güncellendi.
