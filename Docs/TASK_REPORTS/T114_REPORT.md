# T114 — İçerik kayıtları ve marka

**Faz:** F1 | **Durum:** ✓ Doğrulama tur 1 PASS | **Tarih:** 2026-11-04

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- `src/Shopfolio.Web/Modules/CorporateContent/` altında sekiz entity ve yapılandırması (`05 §3.3.11` "Kurumsal içerik" modülü; `PublishStatus` `Catalog/Contracts`'tan, `SocialPlatform` `CorporateContent/Contracts`'ta): `AboutPage` (`06 §4.46`), `Offering` (§4.47), `PortfolioItem` (§4.48), `FaqItem` (§4.49), `CustomPage` (§4.51), `Announcement` (§4.52), `Branding` (§4.54), `SocialLink` (§4.55). Özellik adları alan tablolarıyla birebir; ortak kural `ContentRules` (alan uzunlukları P-27…P-30, sayfa adresi biçimi, iki değerli yayın durumu).
- Veritabanı varsayılanları (`06` "Varsayılan" sütunu, `HasDefaultValue` + göçte `DEFAULT`): `publish_status` = `Draft` (altı tablo), `version` = 1 (yedi tablo), `is_shown_on_homepage` / `is_shown_in_menu` = false, `is_platform_signature_enabled` = true.
- Kontrol kısıtları (`06 §7.3`): yayın durumu `('Draft', 'Published')` alt kümesi (`HasAllowedValues`, `§2.9.2`; altı tablo); `announcements.end_not_before_start`; `brandings.brand_color_format` (`^#[0-9A-F]{6}$`); `slug_format` (`^[a-z0-9]+(-[a-z0-9]+)*$`; üç tablo, `§4.72.4`); `social_links.social_platform` altı değer.
- Dizinler (`06 §7.1`, §7.4): üç tek satırlık tabloda sabit ifadenin tekil dizini `((true))` (göçte elle SQL, `T103` kalıbı); `Slug` filtresiz tekil (üç tablo); (`PublishStatus`, `SortOrder`) — `offerings`, `portfolio_items`, `faq_items`, `custom_pages`; filtreli `(IsShownOnHomepage, SortOrder)` (`offerings`, `portfolio_items`) ve `(IsShownInMenu, SortOrder)` (`custom_pages`) — filtre "işaretli ve yayındaki": `… AND publish_status = 'Published'`; `social_links.social_platform` tekil. Elle sıra tekil değil.
- Bağlar (`06 §7.2`): yalnız `Branding.LogoId` ve `FaviconId` → `Image`, kısıtlayıcı (`NoAction`); tek satırlık tablo olduğu için yabancı anahtar dizini yok (`§7.5.2`, `AsSingleRow`). Modüller arası bağ `const string` + `HasOne` kalıbıyla (`09 §10.1` m.1). İçerik kaydının bağ tablolarına basamaklanması T115/T116'da.
- `CorporateContentRecordsGuard` (yeni, `AppDbContext`'e kayıtlı): `AboutPage` ve `Branding` silinmez (`06 §9.1.4`, §4.46, §4.54); öteki altı kayıt kendi olayıyla silinir.
- Tek göç `20261104095121_CreateContentRecordAndBrandingTables` (`Down` yok; LF; BOM'lu): sekiz tablo. `AppDbContext`'e sekiz `DbSet`.
- `EnumDatabaseTests` enum sütun sayısı 45 → 52; `ModelRuleTests` yabancı anahtar dizin kuralı testinin tek satırlık tablo listesine `brandings` eklendi (D-50 c; Branding'in iki yabancı anahtarı vardır ve dizinsizdir).
- T113 raporunda "Post-merge run'lar" dolduruldu.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/CorporateContent/{AboutPage,Offering,PortfolioItem,FaqItem,CustomPage,Announcement,SocialLink,Branding,ContentRules,CorporateContentRecordsGuard}.cs`, `Infrastructure/AppDbContext.cs`, `Infrastructure/Migrations/*` (göç, tasarım dosyası, anlık görüntü).
- `tests/Shopfolio.IntegrationTests/Model/{ContentRecordAndBrandingTablesTests,EnumDatabaseTests,ModelRuleTests}.cs`.
- `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `.claude/memory/MEMORY.md`, `Docs/TASK_REPORTS/T113_REPORT.md` (post-merge satırı).

## Kabul Kriterleri Kontrolü

Öz denetim (PR öncesi): her kriter ve kontrol listesi maddesi diff'te göç satırıyla ve testle tek tek karşılaştırıldı; `06 §4.46`…§4.49, §4.51, §4.52, §4.54, §4.55 ile §4.72, §7.1 (5495–5502), §7.2 (5644–5645), §7.3 (5724–5725, 5738–5739 ve slug satırı), §7.4 (5812–5815) göçe karşı okundu. Yabancı anahtar: 2 (ikisi `brandings`, tek satır, dizinsiz — `ModelRuleTests`); varsayılanlı alanlar hem varlıkta hem göçte `DEFAULT` (`Columns_HaveTheDatabaseDefaultsOfTheDefaultColumn`); varlıklar `CorporateContent` modülünde (`05 §3.3.11`); ON DELETE yalnız `Branding`'in iki kısıtlayıcı bağı (`ForeignKeys_AreTheTwoRestrictiveImageLinksOfTheBrand`); tekil dizinler ve kontrol kısıtları `06 §7.1`/§7.3 ile birebir (tam tanımlarıyla `Indexes_AreTheDocumentedOnes`, `CheckConstraints_AreTheDocumentedOnes`); bekçi yalnız iki kalıcı kaydı kapsar.

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Sekiz entity alan tablolarıyla tek göçle | ✓ | `Migration_MakesTheEightTables`, `Columns_HaveTheTypesAndNullabilityOfTheFieldTables`, `Columns_HaveTheDatabaseDefaultsOfTheDefaultColumn`, `Indexes_AreTheDocumentedOnes`, `ForeignKeys_AreTheTwoRestrictiveImageLinksOfTheBrand`, `CheckConstraints_AreTheDocumentedOnes` |
| 2 | `AboutPage`, `Announcement`, `Branding` en çok bir satır; sabit ifadenin tekil dizini | ✓ | `SecondAboutPageRow_IsRefusedByTheSingleRowIndex` (`ix_about_pages_single_row`), `SecondAnnouncementRow_…` (`ix_announcements_single_row`), `SecondBrandingRow_…` (`ix_brandings_single_row`), `SingleRowTables_CarryOnlyTheSingleRowIndexAndTheKey`, `AnnouncementDeleted_LetsANewRowBeOpened`; göçte `CREATE UNIQUE INDEX … ((true))` |
| 3 | Yayın durumu yalnız taslak ya da yayımlanmış | ✓ | `PublishStatus_ThirdValueOfTheEnum_IsRefused` (6 tablo, `Archived`), `PublishStatus_UnknownText_IsRefused` (6), `PublishStatus_DraftAndPublished_AreAccepted` (6); enum kısıtı `HasAllowedValues(Draft, Published)`; `EnumDatabaseTests` (52 sütun) |
| 4 | `Announcement` bitişi başlangıçtan önce olamaz | ✓ | `Announcement_EndBeforeStart_IsRefused` (`ck_announcements_end_not_before_start`), `Announcement_EqualOrLaterEndOrAnOpenSide_IsAccepted` (5 durum) |
| 5 | `SocialLink` platform başına tekil; `Offering`/`PortfolioItem`/`CustomPage` `Slug` filtresiz tekil ve biçim kısıtı | ✓ | `SocialLink_SecondLinkOfThePlatform_IsRefused` (`ix_social_links_social_platform`), `SocialLink_EveryPlatformOnce_IsAccepted`, `Offering_/PortfolioItem_/CustomPage_SecondXWithTheSameSlug_IsRefused`, `Slug_OfADraftRow_IsStillTaken`, `Slug_SameValueInTwoTables_IsAccepted`, `Slug_Uppercase_IsRefused` (3 tablo), `Slug_OtherMalformedValues_AreRefused` (3 × 10 değer), `Slug_WellFormedValues_AreAccepted` (3) |
| 6 | `Branding` marka rengi `#` + altı büyük onaltılık hane | ✓ | `BrandColor_OtherThanHashAndSixCapitalHexDigits_IsRefused` (9 değer, `ck_brandings_brand_color_format`), `BrandColor_HashAndSixCapitalHexDigitsOrEmpty_IsAccepted` (5) |
| 7 | Elle sıra tekil değil | ✓ | `SortOrder_SameValueInTwoRows_IsAccepted` (4 tabloda ikişer satır aynı değer); `Indexes_AreTheDocumentedOnes` (sıra dizinleri tekil değil) |
| 8 | Uzun metin `text`; logo ve simge bağı `Image`'e kısıtlayıcı; basamaklama T115/T116'da | ✓ | `LongTextColumns_AreTextAndTakeALongValue` (5 sütun `text`, 100 000 karakter yazılıp okunur), `ShortTextColumns_RefuseAValueOverTheirLength` / `…TakeAValueOfExactlyTheirLength`, `DeletingTheImageOfTheLogo_IsRefused`, `DeletingTheImageOfTheFavicon_IsRefused`, `ReplacedLogo_LeavesTheOldImageUnlinkedAndDeletable`, `BrandingWithAnUnknownImage_IsRefused`; içerik–ürün ve galeri bağ tabloları bu task'ta yok |
| 9 | Göç tek konu; `Down` yok | ✓ | Mevcut `Migration_HasNoDownMethod` mimari taraması yeşil (Unit 245/245); `dotnet ef migrations has-pending-model-changes` → "No changes"; göçte `Down`/`GRANT`/`REVOKE`/`now()`/`suppressTransaction`/`CONCURRENTLY`/`UPDATE`/`DELETE` yok (grep çıkış 1) |

**Doğrulama kontrol listesi:**

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 göç boş veritabanına, sekiz tablo | ✓ | `Migration_MakesTheEightTables` (test veritabanı bütün göçlerle kurulur) |
| KK2 ikinci `Announcement` satırı | ✓ | `SecondAnnouncementRow_IsRefusedByTheSingleRowIndex` |
| KK3 üçüncü yayın durumu değeri | ✓ | `PublishStatus_ThirdValueOfTheEnum_IsRefused`, `PublishStatus_UnknownText_IsRefused` |
| KK4 bitişi başlangıçtan önce duyuru | ✓ | `Announcement_EndBeforeStart_IsRefused` |
| KK5 aynı platformda ikinci bağlantı; aynı `Slug`'lı ikinci hizmet; büyük harfli `Slug` | ✓ | `SocialLink_SecondLinkOfThePlatform_IsRefused`, `Offering_SecondOfferingWithTheSameSlug_IsRefused`, `Slug_Uppercase_IsRefused` |
| KK6 kısa renk kodu | ✓ | `BrandColor_OtherThanHashAndSixCapitalHexDigits_IsRefused` (`#FFF`, `#1A6B3`) |
| KK7 aynı sıra değerli iki satır kaydedilir | ✓ | `SortOrder_SameValueInTwoRows_IsAccepted` |
| KK8 uzun metin sütunu `text`; logoya bağlı görselin silinmesi | ✓ | `LongTextColumns_AreTextAndTakeALongValue`, `DeletingTheImageOfTheLogo_IsRefused` |
| KK9 kaynak taraması | ✓ | Unit'teki mimari/kaynak taramaları yeşil (245/245); diff'te `Guid.NewGuid`, `CreateVersion7`, `Restrict`/`Client`/`Cascade`, `HasConversion<int>`, `DateTime.Now`, `UtcNow` yok (grep çıkış 1) |
| `09 §25` | ✓ | Enum metin + izinli değer kısıtı (`§10.1`); her FK'nin silme davranışı açık (`NoAction`); anahtar `EntityIds.New()`; göç genişletme, `Down`/`now()`/`suppressTransaction`/`CONCURRENTLY`/`UPDATE`/`DELETE` yok (`§10.6`); yeni tablolar varsayılan yetkide, `Database` ad alanı testleri yeşil (`§10.8`); yeni kapalı değer yok; yeni paket yok; modül sınırı — `Image`'e yalnız tür adıyla FK, kod başvurusu yok, `PublishStatus` Contracts'tan |
| Güvenlik taraması | ✓ | Sır yok; yetki/kimlik doğrulama etkisi yok; giriş noktası yok (yalnız şema); uzun metnin temizlenmesi ve adres biçimi denetimi "Uygulamada" kuralları ilgili akış task'larının işi; yeni dış bağımlılık yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests -c Release` | 245/245 (Consistency dahil) |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests -c Release` (PostgreSQL 18, `shopfolio-t114-pg`, `fsync=off`) | T114 sınıfı 79/79; tam koşu 754/755 — kalan test (`AdminAndAuditTablesTests.Migration_WithoutTheApplicationUserRoleSetting_…`) yerel Npgsql zaman aşımına düştü, tek başına sınıf 42/42. Önceki bir tam koşuda sınıf temizliği (`SampleDatabaseFixture.DisposeAsync`) zaman aşımı verdi — D-68 ile aynı yerel altyapı sorunu |
| Build | ✓ | `dotnet build Shopfolio.slnx -c Release -warnaserror` | 0 uyarı |
| Biçim | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` | çıkış 0 |
| Göç | ✓ | `dotnet ef migrations has-pending-model-changes` | "No changes" |
| Kilit | ✓ | `dotnet restore Shopfolio.slnx --locked-mode` | çıkış 0 |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-04, head `c976977`, PR #372) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 0 |

### Doğrulama tur 1 — ✓ PASS (2026-11-04)

**Kapılar:** working tree temiz; `core.hooksPath` mutlak yolla `scripts/git-hooks` (ana checkout'un hook'ları; eşdeğer); ana dal son 3 run `41809802191` (Publish), `41809802367` (CI), `41805273609` success; `MEMORY.md`'de T114 satırı var.

**Kabul kriterleri:** kapsam tam (9 kriter, KK1–KK9, `09 §25`, güvenlik); hepsi ✓. Validator göçü ve yapılandırmaları `06 §4.46`–§4.49, §4.51, §4.52, §4.54, §4.55, §4.72, §2.3.2, §2.9.2, §2.10.1, §5.3.11, §7.1 (5494–5502, 5525), §7.2 (5644–5645), §7.3 (5724–5725, 5736, 5738), §7.4 (5810–5815) satırlarına karşı elle okudu:
- Kriter 1, 9: sekiz tablo tek göçte (`20261104095121_CreateContentRecordAndBrandingTables`), `Down` yok; sütun türleri, uzunluklar (P-27…P-30: 150/200/400/120), boşluk ve varsayılanlar alan tablolarıyla birebir; göçte `Down`/`suppressTransaction`/`CONCURRENTLY`/`now()`/`UPDATE`/`DELETE` yok; elle SQL `public.` nitelikli; dal CI'ının göç adımı `has-pending-model-changes` yeşil; anlık görüntü diff'i yalnız ekleme.
- Kriter 2: `ix_about_pages_single_row`, `ix_announcements_single_row`, `ix_brandings_single_row` — `((true))` üzerinde tekil dizin (`Company`/`CompanySettings` kalıbı); silinen duyurunun yerine yeni satır açılabiliyor.
- Kriter 3: altı tabloda `publish_status IN ('Draft', 'Published')`; `Archived` ve bilinmeyen metin reddediliyor.
- Kriter 4: `start_date IS NULL OR end_date IS NULL OR end_date >= start_date` (`06 §4.52` "ikisi de doluysa").
- Kriter 5: `ix_social_links_social_platform` tekil; platform kısıtının altı değeri `06 §5.3.11` ile birebir; üç `slug` dizini filtresiz tekil, biçim `^[a-z0-9]+(-[a-z0-9]+)*$` (`§4.72.4`); taslak satırın adresi dolu sayılıyor, iki türde aynı değer serbest.
- Kriter 6: `brand_color ~ '^#[0-9A-F]{6}$'`; `#FFF`, küçük harf, `#`siz, boşluklu değerler reddediliyor, NULL serbest.
- Kriter 7: sıra dizinleri tekil değil; dört tabloda aynı sıralı iki satır kaydediliyor.
- Kriter 8: beş uzun metin sütunu `text`; `Branding` → `Image` iki bağ `NO ACTION` (proje kalıbı), tek satır olduğu için FK dizini yok (`06 §7.2` "gerekmez"; `ModelRuleTests` listesi buna göre genişledi); içerik kayıtlarında başka FK yok — basamaklama T115/T116'da.

**`09 §25`:** göç genişletmedir; enum metin + izinli değer kısıtı; FK silme davranışı açık; anahtar `EntityIds.New()`; yeni kapalı değer ve paket yok (`SocialPlatform` ve `PublishStatus` kilit dosyaları zaten var); yeni tablolar varsayılan yetkide (`Database` ad alanı testleri yeşil); modül sınırı — `Image`'e yalnız tür adıyla bağ, `PublishStatus` `Catalog.Contracts`'tan. `CorporateContentRecordsGuard` `06 §9.1.4`'ün kalıcı saydığı iki kaydı korur; kapsam dışı ek davranış değil.

**Testler (validator, yerel PostgreSQL 18 `shopfolio-t114-val-pg`):** build `-warnaserror` 0 uyarı; `dotnet format --verify-no-changes` çıkış 0; Unit 245/245 (Consistency dahil); Integration 882 — T114 sınıfı 79/79, Model 820 (tam koşuda `EnumDatabaseTests` ve `SettingsTablesTests` sınıf temizliği yerel Npgsql zaman aşımına düştü — D-68; ikisi ve `ModelRuleTests` tek başına 13/13, 28/28, 14/14), Database+FileSecrets+Web 62/62; `dotnet restore --locked-mode` çıkış 0; koşu sonrası `git status` temiz. Dal CI `41812838046` (head `c976977`) yeşil — 9 iş success, guard skip.

**Güvenlik:** sır yok; yalnız şema, auth etkisi yok; girdi doğrulama uygulanamaz (uç yok — metin temizleme ve adres denetimi akış task'larında); yeni bağımlılık yok.

**`12` satırları:** kaynak birimleri `06` entity'leri (KP yok); `06 §4.46`…§4.55, §4.72, §2.10'u anan matris satırı yok (`VAL-A526` `AboutPageNotDeletable` uç task'ının — BEKLEMEDE kalır); `Durum` değişmedi; `12_matris_denetle.py --kapsam 1-77` hata 0.

**K-notları:** yok (yerel sınıf temizliği zaman aşımı D-68'de açık).

**Yapım raporu karşılaştırması:** kriter tablosu, KK listesi ve ⚠ kararlar (filtreli dizin süzgeci, bekçi, `brandings` adı, uzunluklar, boş `slug`'ın reddi) validator'ın bağımsız okumasıyla uyumlu. Tek fark sayım: yapımın yerel tam koşu toplamı (755) validator'ın namespace'lere bölünmüş toplamından (882) farklı; kriteri etkilemez.

## Altyapı Değişiklikleri

- **Şema/migration:** Var — sekiz tablo (genişletme göçü; üç tek satırlık dizin elle SQL).
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T114-icerik-kayitlari-marka`
- **PR:** #372
- **CI:** yardımcı izler
- **Post-merge run'lar:** squash `7fa45cf` — CI `41815985996` success; Publish `41815986016` failure (kayıt defterine itme: `unknown blob` — altyapı, kod sorunu değil)

## Dış Varsayımlar

Dış varsayım: yok (plan katmanı, kota, dış servis, paket sürümü yok; PostgreSQL 18 CI ve yerel konteynerle aynı).

## ⚠ Kararlar

1. **Filtreli dizin süzgeci.** `06 §4.47`/§4.48/§4.51 "işaretli ve yayındaki satırlarda filtreli" der; filtre `is_shown_on_homepage AND publish_status = 'Published'` (menü için `is_shown_in_menu AND …`) yazıldı. `06 §7.1`'de bu dizinlerin süzgeç sütunu ayrıca kısıtlanmamış; okuma doküman cümlesinin birebiridir.
2. **`CorporateContentRecordsGuard` eklendi** (kabul kriterlerinde adı geçmez): `06 §9.1.4` `AboutPage` ve `Branding`'i kalıcı sayar (§4.46 "silinmez", §4.54 "kalıcı"); `Company`/`CompanySettings` için `ProtectedRecordsGuard` aynı kalıpla yazılmıştır. Öteki altı kayıt olayla silinir, bekçi dışı.
3. **Tablo adı `brandings`** (`06 §2.4.1`: çoğul; `Branding` sayılamaz ad olsa da kural kod adının çoğuludur); `about_pages`, `faq_items` aynı kuralla.
4. **`FaqItem.Question` uzunluğu 200** (`P-28`, `06 §4.49`); `Offering.Name`, `PortfolioItem.Title`, `CustomPage.Title` 150 (`P-27`); `ShortDescription` 200 (`P-28`); `ShortIntroduction` 400 (`P-29`); `Announcement.Text` 120 (`P-30`).
5. **`IsPlatformSignatureEnabled` varsayılanı `true`** DB'de `DEFAULT true`; açık `false` değerinin veritabanı varsayılanına yutulmadığı `Entities_RoundTripTheirStatusAndSwitches` ile tutulur (EF'in bool sentinel'i).
6. **`ModelRuleTests`** D-50 c testi `brandings`'i tek satırlık listeye aldı (tablonun iki yabancı anahtarı vardır ve `06 §7.2` "gerekmez — tek satır" der); test başka bir şeyi gevşetmez.
7. **Slug biçimi boş metni de reddeder** (`""` desene uymaz); kayıtta boş metin zaten NULL'a çevrilir ve `slug` zorunludur.
8. Kontrol kısıtı adları `06` ad vermediği için seçildi (`end_not_before_start`, `brand_color_format`, `slug_format` — `Product`'ın `slug_format` adıyla aynı).
9. Fabrika yöntemleri asgari: `Create(…)` ad/başlık, adres ve elle sırayı alır; yayın durumu ve sürüm varsayılanlarıyla doğar.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | İçerik kayıtlarının yazım yolları ve "Uygulamada" kuralları (yayın kapısı, adres üretimi ve sabitlenmesi, adres biçimi/bağlantı denetimi, metin temizleme, sürümün kapsamı, işlem izi, marka adının boşaltılmaması) | — | İlgili akış task'larının işi; bu task yalnız tabloları kurar |
| 2 | İçerik–ürün ve galeri bağ tabloları, silmede basamaklama | — | T115 ve T116 (kabul kriteri 8) |
| 3 | `Branding` satırının başlangıç verisi | — | T119 |

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `scripts/git-hooks`.
- **Ana dal CI startup (Adım 0):** son üç tamamlanmış run success — `41809802191` (Publish), `41809802367` (CI), `41805273609` (Publish).
- **Dış varsayım:** yok (yukarıda).
- **Bağlı backlog:** `grep -n "T114" Docs/DEFERRED_BACKLOG.md` boş — kapanan kalem yok, açılan yok.
- **Repo memory (bitiş kapısı 8):** güncellendi.
