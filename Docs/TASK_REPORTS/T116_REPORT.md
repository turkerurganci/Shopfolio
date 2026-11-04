# T116 — Galeri bağları

**Faz:** F1 | **Durum:** ⏳ Yapım bitti, doğrulama bekliyor | **Tarih:** 2026-11-04

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- `src/Shopfolio.Web/Modules/CorporateContent/` altında beş entity ve yapılandırması (`05 §3.3.11` "Kurumsal içerik" modülü): `AboutPageImage`, `OfferingImage`, `PortfolioItemImage`, `FaqItemImage`, `CustomPageImage` (`06 §4.71`). Alanlar `ProductImage` (T104) ile birebir: `Id`, `CreatedAt`, sahibin bağı, `ImageId`, `SortOrder`, `IsMain`, `AltText`.
- `IsMain` varsayılanı `false`: varlıkta `HasDefaultValue(false)`, göçte `DEFAULT false` (`06` "Varsayılan" sütunu).
- Bağlar: sahip bağı `Cascade` (`06 §2.2.5`); `ImageId` bağı `NoAction` (kısıtlayıcı, `06 §4.71`, §7.2). `Image` `Catalog` modülündedir; `Image`'e bağ `const string` + `HasOne` kalıbıyla (`Branding`, `Branch` gibi; `09 §6` m.2).
- Dizinler (`06 §4.71`, §7.5.2): (sahip, `ImageId`) tekil; sahip bağında `is_main` filtreli tekil dizin (`ProductImage` ile aynı filtre); `ImageId` dizini. Sıra için tekil dizin yok.
- Tek göç `20261104112826_CreateGalleryLinkTables` (`Down` yok; LF; BOM'lu): beş tablo. `AppDbContext`'e beş `DbSet`.
- Test güncellemeleri: `ModelRuleTests` kayıtlı basamaklama listesine beş satır (`06 §2.2.5`). Önceki entity yorumlarındaki "sonraki task'ta gelir" notları güncellendi.
- T115 raporunda "Post-merge run'lar" dolduruldu; T115 durum girişi `STATUS_CHANGELOG.md`'ye taşındı.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/CorporateContent/{AboutPageImage,OfferingImage,PortfolioItemImage,FaqItemImage,CustomPageImage}.cs` (yeni); `AboutPage`, `Offering`, `PortfolioItem`, `FaqItem`, `CustomPage` (yalnız yorum); `Infrastructure/AppDbContext.cs`; `Infrastructure/Migrations/*` (göç, tasarım dosyası, anlık görüntü).
- `tests/Shopfolio.IntegrationTests/Model/{GalleryLinkTablesTests (yeni),ModelRuleTests}.cs`.
- `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `.claude/memory/MEMORY.md`, `Docs/TASK_REPORTS/T115_REPORT.md` (post-merge satırı).

## Kabul Kriterleri Kontrolü

Öz denetim (PR öncesi): her kriter ve kontrol listesi maddesi diff'te göç satırı ve testle `grep -n` ile tek tek karşılaştırıldı. Varsayılan sütunu DB default'u (`Columns_HaveTheDatabaseDefaultOfTheDefaultColumn`); beş tablonun her FK'sinin dizini var (sahip bağı çift dizininin ilk sütunu, `ImageId` dizini); varlıklar `CorporateContent` modülünde; ON DELETE `06 §4.71` ile birebir (`ForeignKeys_CascadeWithTheOwner_AndAreRestrictiveToTheImage`); filtreli tekil dizin filtresi `is_main`, `ProductImage` ile aynı (`Indexes_AreTheDocumentedOnes`); bekçi/tarama istisnası eklenmedi.

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Beş bağ `06 §4.71` alan tablosuyla tek göçle | ✓ | `Migration_MakesTheFiveTables`, `Columns_HaveTheTypesAndNullabilityOfTheFieldTable`, `Columns_HaveTheDatabaseDefaultOfTheDefaultColumn`, `Indexes_AreTheDocumentedOnes`, `ForeignKeys_…`, `CheckConstraints_AreNone`; göç `20261104112826_CreateGalleryLinkTables` |
| 2 | (sahip, `ImageId`) çiftinde tekil | ✓ | `SecondLinkOfTheSameImageToAGallery_IsRefused` (beş tablo, `ix_<tablo>_<sahip>_image_id`), `OneImage_OnTwoGalleries_AndTwoImagesOnOneGallery_AreAccepted` |
| 3 | Ana görsel sahip başına en çok bir; filtreli tekil dizin | ✓ | `SecondMainImageOfAnOwner_IsRefused` (`ix_<tablo>_<sahip>`), `MainImage_OfTwoOwners_AndManyNonMainImagesOfOneOwner_AreAccepted`, `Indexes_AreTheDocumentedOnes` (`WHERE is_main`) |
| 4 | Sahibiyle basamaklı; görsele kısıtlayıcı | ✓ | `DeletingTheOwner_RemovesItsLinks_AndLeavesTheImagesAndOtherGalleries`, `DeletingALinkedImage_IsRefused_AndAnUnlinkedImageIsDeletable` (`fk_<tablo>_images_image_id`), `Link_OfAnUnknownOwnerOrImage_IsRefused`, `ForeignKeys_…` (`c`/`a`), `ModelRuleTests.ApplicationModel_FollowsTheRules_AndItsCascadeSetEqualsTheRegisteredList` |
| 5 | Elle sıra tekil değil | ✓ | `SortOrder_SameValueInTwoRowsOfOneGallery_IsAccepted` (beş tablo); `Indexes_…` sıra üzerinde dizin yok |
| 6 | Göç tek konu; `Down` yok | ✓ | Mevcut `Migration_HasNoDownMethod` mimari taraması yeşil (Unit 245/245); `dotnet ef migrations has-pending-model-changes` → "No changes"; göçte `Down`/`now()`/`suppressTransaction`/`CONCURRENTLY`/`UPDATE`/`DELETE` yok (grep) |

**Doğrulama kontrol listesi:**

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 göç boş veritabanına, beş tablo | ✓ | `Migration_MakesTheFiveTables` (test veritabanı bütün göçlerle kurulur) |
| KK2 aynı görseli ikinci kez eklemek | ✓ | `SecondLinkOfTheSameImageToAGallery_IsRefused` |
| KK3 ikinci ana görsel | ✓ | `SecondMainImageOfAnOwner_IsRefused` |
| KK4 sahip silmede bağ gider; bağlı görsel silme reddedilir | ✓ | `DeletingTheOwner_RemovesItsLinks_…`, `DeletingALinkedImage_IsRefused_…` |
| KK5 aynı sıra değerli iki bağ | ✓ | `SortOrder_SameValueInTwoRowsOfOneGallery_IsAccepted` |
| KK6 kaynak taraması | ✓ | Unit'teki mimari/kaynak taramaları yeşil (245/245); üretim kodunda `Guid.NewGuid`, `CreateVersion7`, `Restrict`/`Client`, `HasConversion<int>`, `DateTime.Now`, `UtcNow` yok (grep; yalnız test yardımcılarında `Guid.NewGuid`); `Cascade` yalnız `06 §2.2.5`'in sahip bağlarında (kayıtlı liste `ModelRuleTests`'te) |
| `09 §25` | ✓ | Her FK'nin silme davranışı açık (`NoAction`/`Cascade`); anahtar `EntityIds.New()`; göç genişletme, `Down`/`now()`/`suppressTransaction`/`CONCURRENTLY`/`UPDATE`/`DELETE` yok (`§10.6`); yeni tablolar varsayılan yetkide; yeni kapalı değer ve paket yok; modül sınırı — `Image`'e yalnız tür adıyla FK |
| Güvenlik taraması | ✓ | Sır yok; yetki/kimlik doğrulama etkisi yok; giriş noktası yok (yalnız şema); yeni dış bağımlılık yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests` | 245/245 (Consistency dahil) |
| Integration | ✓ | `dotnet test tests/Shopfolio.IntegrationTests` (PostgreSQL 18, `t116-pg`) | 841/841; T116 sınıfı 47 test (beş tablo × teori) |
| Build | ✓ | `dotnet build Shopfolio.slnx` | 0 uyarı |
| Biçim | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` | çıkış 0 |
| Göç | ✓ | `dotnet ef migrations has-pending-model-changes` | "No changes" |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | Bekliyor |

## Altyapı Değişiklikleri

- **Şema/migration:** Var — beş tablo (genişletme göçü).
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T116-galeri-baglari`
- **PR:** *(açılınca)*
- **CI:** yardımcı izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok (plan katmanı, kota, dış servis, paket sürümü yok; PostgreSQL 18 CI ve yerel konteynerle aynı).

## ⚠ Kararlar

1. **Hakkımızda bağı da `Cascade`.** `06 §4.71` "sahibiyle basamaklanır" der; Hakkımızda hiç silinmediği için (§4.46) basamak yalnız şema düzeyindedir, galeriden kaldırma olayla siler. Basamak listesine `about_page_images -> about_pages` eklendi.
2. **Testte Hakkımızda tek satırdır** (`ix_about_pages_single_row`); çift sahipli senaryolar o tabloda tek sahiple koşar.
3. **Ürün bağı bu task'ın dışında:** `ProductImage` T104'tedir.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | "Galeri boş değilse tam bir ana görsel" (K-1069), kayıt başına adet sınırı (P-24, P-25), sahibin sürümüne girme | — | "Uygulamada" kuralları; akış task'larının işi |

## Notlar

- **Working tree (Adım -1):** temiz.
- **Ana dal CI startup (Adım 0):** `gh run list --branch main`: CI `41820820561` ve Publish `41820820591` (head `d2c7c36`) success; önceki CI `41815985996` ve Publish `41815986016` success.
- **Dış varsayım:** yok.
- **Bağlı backlog:** `grep -n "T116" Docs/DEFERRED_BACKLOG.md` boş — kapanan kalem yok, açılan yok.
- **Repo memory (bitiş kapısı 8):** güncellendi.
