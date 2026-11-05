# T223 — Dosya deposu

**Faz:** F2 | **Durum:** ⏳ Yapım bitti, doğrulama bekliyor | **Tarih:** 2026-11-05

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Dosya deposu (`Infrastructure/Files/`):** tek erişim arayüzü `IFileStore` (yaz, ekle, oku, var mı, boyut, sil, yaşa göre listele); disk gerçeklemesi `DiskFileStore` (içerik aynı dizinde geçici dosyaya yazılır, bitince adlandırılır — yarım dosya görünmez; iki bölüm: görsel ve dijital dosya); anahtar biçimi `FileKeys` (görsel: içeriğin SHA-256'sı ya da `{özet}-{genişlik}`; dijital: 32 onaltılık hane; yol ayracı ve `..` anahtar olamaz, yol yalnız anahtardan kurulur). Sahte: `UnitTests/Fakes/InMemoryFileStore`.
- **Görüntü işleme (`Infrastructure/Imaging/`):** kütüphane **SkiaSharp 4.154.0** (MIT); `ImageProcessor` — tür ilk baytlardan, boyut başlıktan ve çözmeden önce, ilk kare sRGB'ye çözülür, önce küçültülür sonra EXIF yönü piksellere uygulanır, kodlayıcı yalnız pikselleri yazar (EXIF/XMP/IPTC/renk profili yok). Eşzamanlılık **1** (`ImagingLimits.MaxConcurrent`).
- **Catalog tarafı (`Modules/Catalog/`):** `ImageUploads` (tavan P-23 → işleme → kaynak kopya parmak iziyle dosyaya → tek işlemde `Image` satırı + `ImageDerivative` iş satırı), `ImageDerivativeJobHandler` (vitrin genişliklerinden kaynaktan küçük olanların WebP türevi; `DerivativesReadyAt` bir kez), `ImageServing` (türevsiz görsele kaynak kopya), `ImageDisposalPart` (bağsız ve bir günden eski görsel; bağlar modelden okunur) ve `OrphanImageFilePart` (satırı olmayan dosya; kayıt dosyadan önce).
- **Kütüphane kararı:** Magick.NET (gömülü kopyaleft yerel kütüphaneler) ve ImageSharp (3.x açıklı, 4.x koşullu lisans + anahtar) elendi; `09` Kütüphaneler tablosu 4 satır (SkiaSharp, Linux.NoDependencies, iki "Elendi"), §3.3 giriş cümlesi, §3.5 görüntü işleme satırı, §15.6 SkiaSharp'a göre yeniden yazıldı; karar kaydı **K-1575** (K-1316 (6)'nın "yedek geçiş karar istemez" kuralı kalktı).
- **Eşitlik/tarama:** `LibraryTableConsistency` "Elendi" satırlarını okumaz; `ImagingLibraryScan` (+ ayrı ihlal örneği `ViolationSamples/Imaging/LibraryLeak.cs`); `LinkKeySourceScan` istisnası dar (`ImageUploads.cs`) + istisna testi; `FileSizeCeilingConsistencyTests` (P-23/P-26 ↔ `ProductConstants`); `FileStorePathsConsistencyTests` (dizinler ↔ Compose birimleri ↔ app imajı).
- **Bu turda (devam 2):** `09` düzenlemesi, K-1575, eşzamanlılık 2 → 1, entegrasyon testleri (`IntegrationTests/Files/`), P-23/P-26 ve yol eşitlik testleri, `DiskFileStore.DeleteAsync` düzeltmesi (klasör henüz yokken silme hata vermez — arayüz sözleşmesi), `ConcurrencyTests` sağlayıcısına Catalog'un yeni bağımlılıkları, backlog (D-97, D-102, D-32), CONTEXT, status, MEMORY.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Files/`, `Infrastructure/Imaging/`, `Infrastructure/ProductConstants.cs`, `Program.cs` (`AddFileStore`, `ImageProcessor`), `Modules/Catalog/` (`Image*`, `CatalogModule.cs`, `Contracts/ICatalogModule.cs`, `Contracts/ImageUploadResult.cs`), `Shopfolio.Web.csproj`, `Directory.Packages.props`, kilit dosyaları.
- Testler: `UnitTests/Imaging/` (+ `fixtures/`), `UnitTests/Fakes/InMemoryFileStore.cs`, `UnitTests/Architecture/ImagingLibraryScan*`, `UnitTests/Consistency/` (`LibraryTableConsistency*`, `FileSizeCeilingConsistencyTests`, `FileStorePathsConsistencyTests`), `IntegrationTests/Files/`, `IntegrationTests/Concurrency/ConcurrencyTests.cs`, `IntegrationTests/*.csproj` (görsel yardımcılarının bağlı derlemesi).
- Doküman: `Docs/09_CODING_GUIDELINES.md` (§3.3, §3.5, Kütüphaneler tablosu, §15.6), `Docs/PRODUCT_DISCOVERY_STATUS.md` (K-1575), `Docs/DEFERRED_BACKLOG.md`, `Docs/IMPLEMENTATION_STATUS.md`, `.claude/CONTEXT.md`, `.claude/memory/MEMORY.md`.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Dosyalara arayüzden erişilir; disk gerçeklemesi; sahte `InMemoryFileStore` | ✓ | `IFileStore`, `DiskFileStore`, `InMemoryFileStore`; `DiskFileStoreTests` (9 test); modüller yalnız `IFileStore` alır (`ImageUploads`, `ImageDerivativeJobHandler`, `OrphanImageFilePart`) |
| 2 | Disk adı rastgele/anahtar; yükleyen adı yol olmaz | ✓ | `FileKeys` (parmak izi / 32 hane); `DiskFileStoreTests.AKeyThatDoesNotFitTheImageSection_IsRefused_…` (`../etc/passwd`, `a/b`, `..\secret`…); `FileKeysTests`; geçici ad `Path.GetRandomFileName` |
| 3 | Tür içerikten; yalnız P-22; boyut tavanı P-23 | ✓ | `ImageProcessorTests.AnSvgWithAnImageName_IsUnsupported_…`, `APngContent_IsReadAsPng_WhateverTheFileIsCalled`; `ImageUploads` `ProductConstants.ImageMaxBytes`; `FileSizeCeilingConsistencyTests.P23Row_EqualsTheImageCeiling` |
| 4 | Yeniden kodlama, 2560, yön, meta veri atılır, ilk kare, türev hareketsiz | ✓ | `ABigSource_IsReducedToTheLongestSideLimit_…`, `TheOrientationIsAppliedToThePixels_AndNoMetadataIsLeft`, `AMovingWebP_IsEncodedFromItsFirstFrame_AsOneFrame`, `ACmykJpeg_…`, `ADerivative_IsAStillWebPOfTheWantedWidth_…` |
| 5 | WebP türevleri kuyrukta; yükleme beklemez; türevsizde kaynak kopya | ✓ | `ImageFilesTests.AnUpload_WritesTheSourceCopyUnderItsFingerprint_AndTheRowAndTheJobInOneTransaction`, `TheDerivativeJob_WritesTheNarrowerWidths_AndASecondRunChangesNeitherFilesNorTheMoment`, `…_MakesNoDerivativeAsWideAsTheSource_…`; `ImageProcessorTests.AnImageWithoutDerivatives_IsServedFromTheSourceCopy_…` |
| 6 | 10.000 px / 40 MP aşan çözülmeden reddedilir | ✓ | `AHugeSmallFile_IsRefusedFromItsHeader_WithoutDecoding` (`DecodeCount` 0); çözülen arabellek bellek tavanı da başlık kapısında |
| 7 | Çözülemeyen dosya reddedilir | ✓ | `ABrokenFile_IsUndecodable_AndATruncatedOneToo`; `ImageFilesTests.ARejectedUpload_WritesNoFileNoRowAndNoJob` |
| 8 | Dijital dosyada tür sınırı yok; ek olarak sunulur; tavan P-26 | ~ | **Altyapı payı ✓:** dijital bölüm (tür denetimi yok, yalnız 32 hane anahtar), `AppendAsync` (parçalı yükleme), `ProductConstants.DigitalFileMaxBytes` + `FileSizeCeilingConsistencyTests.P26Row_…`; `DiskFileStoreTests.AnAppend_…`, `ADigitalFileKey_IsNotAnImageKey_…`. **İndirme (ek olarak) ve yükleme ucu T312/T340'tadır** (planda o task'ların kriteridir); T223'te uç yok |
| 9 | Bağsız ve bir günden eski görsel imhada silinir | ✓ | `ImageFilesTests.TheImageDisposal_DeletesTheUnlinkedOldImages_AndKeepsTheLinkedOnesAndTheYoungOnes`, `TheDeleteStatement_Names…`, `TheOrphanFileSweep_*` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1 modül dosyaya yalnız arayüzden | ✓ | Catalog'da `FileStream`/`System.IO` yok; `ImageUploads`/`ImageDerivativeJobHandler`/`OrphanImageFilePart` yalnız `IFileStore` ve `FileKeys` kullanır |
| KK2 disk adı rastgele, yükleyen adı yol değil | ✓ | Kriter 2 |
| KK3 SVG reddi, `.jpg` PNG içeriği PNG, P-23 aşımı | ✓ | Kriter 3; `ImageUploads` `ImageUploadResult.TooLarge` |
| KK4 2560, EXIF konumu yok, yön, hareketli → tek kare | ✓ | Kriter 4 (`ImageInspection.MarkersIn` çıktının baytlarını arar) |
| KK5 türevler kuyrukta, yükleme beklemez, türevsizde kaynak | ✓ | Kriter 5: yükleme yalnız satır + iş satırı yazar |
| KK6 devasa boyutlu küçük dosya | ✓ | Kriter 6 |
| KK7 bozuk dosya | ✓ | Kriter 7 |
| KK8 dijital ek olarak gider, tavan aşımı reddedilir | ~ | Kriter 8: altyapı payı burada, uç T312/T340'ta |
| KK9 bağsız eski görsel imhada silinir | ✓ | Kriter 9 |
| `09 §25` ilgili satırlar | ✓ | PR açıklamasında |
| Güvenlik taraması | ✓ | Yol anahtardan kurulur (`FileKeys.Require` her işlemde, yol birleştirmeden önce); kaynak taraması `ImagingLibraryScan`; kütüphane lisans/güvenlik açığı denetimi (NuGet denetimi temiz) |
| Sık FAIL: FK dizini / DB varsayılanı | ✓ | Tablo eklenmedi, şema/göç yok |
| Sık FAIL: varlık yanlış modülde | ✓ | `Image` Catalog'da (`11` aday tablosu); altyapı `ImageKind`'ı kendi taşır, modülün `ImageFormat`'ına başvurmaz |
| Sık FAIL: aynı sayıyı anan öteki yerler | ✓ | Eşzamanlılık 1: `ImagingLimits`, `09 §3.5` satırı, §15.6 m.3 ve m.4, K-1575; "2" arayan tarama kalan yer bulmadı; `09` kaynak satırına K-1575 eklendi |
| Sık FAIL: tarama istisnası geniş / ayrı ihlal örneği yok | ✓ | `LinkKeySourceScan` istisnası tek dosya (`ImageUploads.cs`) + dar istisna testi; `ImagingLibraryScan` + `ViolationSamples/Imaging/LibraryLeak.cs` |
| Sık FAIL: hedefi bu task olan backlog kapatılmadı | ✓ | D-97 (görsel türevi payı), D-102 (görsel parçaları), D-32 (a) dosya yolları kapandı; DigitalFile parçaları T302'de |
| Sık FAIL: belgede olmayan sabit / test sabiti kopyalıyor | ✓ | Sabitler `09 §3.5` satırında yazılı; testler genişlikleri `ImageServing`/`ImagingLimits`'ten türetir; P-23/P-26 eşitlik testi |
| Sık FAIL: test gerçek saate bağlı / koleksiyon yalıtımı | ✓ | `ManualTimeProvider`; kendi koleksiyonu `Files` ve `CollectionDatabaseFixture` |
| Düzeltme 1 sonrası: kriter kanıtları hâlâ geçerli | ✓ | Kriter 1–9'daki test adları ve `IFileStore` kullanımı (`ImageUploads`, `ImageDerivativeJobHandler`, `OrphanImageFilePart` → `ImageDisposalParts.cs`) `grep -n` ile yeniden bulundu; düzeltme yalnız `using` sırası ve `var` → açık tür |
| Sık FAIL benzeri: biçim denetimi yerelde koşulmadı | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes` PR öncesi kapıya eklendi (aşağıda) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj` | 1214 geçti, 0 kaldı |
| Integration | ✓ | aynı proje, `--filter-namespace` parçaları (Model `--max-threads 3`) | 1232 geçti (Model 893; Disposal+Jobs+Concurrency+Files 147; öteki 192); Jobs'ta bir koşuda tek bir geçici hata, yeniden koşuda yok |
| Contract | — | — | Bu task'ta sözleşme testi yok |

## Düzeltme 1 — CI lint

CI "1. Lint" (`dotnet format --verify-no-changes`) kırmızıydı: `Program.cs` ve `FilesHarness.cs` using sırası, `InMemoryFileStore.cs` iki yerde `var` (IDE0008). `dotnet format Shopfolio.slnx` ile düzeltildi; davranış değişikliği yok. Yerelde yakalanmadı çünkü yapımda yalnız build ve test koşulmuştu, `dotnet format --verify-no-changes` koşulmamıştı. Sonuç: `--verify-no-changes` çıkış 0, `-warnaserror` derleme temiz.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 1) |
| Bulgu sayısı | 3 (B1 kriter 8 kısmi, B2 modül arayüz metodu işlem açıyor, B3 gömülü yerel kütüphane lisans kanıtı) |
| Düzeltme gerekli mi | Evet — düzeltme turu |
| Bloke-etmeyen not (K) sayısı | 4 |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok (dizinler Compose'da zaten vardı; `FileStorePathsConsistencyTests` eşitliği sınar)
- **Paketleme / dağıtım:** Yok (yerel kütüphane `SkiaSharp.NativeAssets.Linux.NoDependencies` imajda sistem paketi istemez)
- **Yeni bağımlılık:** Var — `SkiaSharp` 4.154.0 (MIT, Microsoft; NuGet denetimi temiz) ve `SkiaSharp.NativeAssets.Linux.NoDependencies` 4.154.0. Gömülü yerel kütüphanelerin lisansları `09` Kütüphaneler tablosunda; **tek kopyaleft iz** gif çözücüsü (Mozilla üçlü lisans) — MPL 1.1 seçildi (⚠ 1)

## Commit & PR

- **Branch:** `task/T223-dosya-deposu`
- **Commit:** PR'ın başlık SHA'sı dönüşte
- **PR:** #399
- **CI:** yapım işçisi izlemez
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | SkiaSharp 4.154.0 lisansı MIT | paketin nuspec'i ve LICENSE.txt okundu (2026-11-05) | ✓ |
| 2 | Linux yerel kitaplığı `libmicrohttpd` (LGPL) ve Adobe DNG içermez | linux-x64 `libSkiaSharp.so`'da `MHD_`, `microhttpd`, `DNG` aramaları sıfır | ✓ |
| 3 | SkiaSharp'ın kendi bellek sınırı yoktur; tek işlem tepe ≈ 212 MB | ölçüldü (160 MB çözülen + iki ≈26 MB kopya) | ✓ → eşzamanlılık 1 (⚠ 2) |
| 4 | Profilsiz CMYK JPEG renkleri bozmaz | `ACmykJpeg_IsAccepted_AndComesOutAsRgb` (turuncu turuncu kalır) | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `DigitalFile` imha parçaları (tamamlanmamış yükleme, dijital `OrphanFile`) | D-102 | Dijital dosya yüklemesi T302/T312'dedir |
| 2 | Dijital dosya indirme ucu ve yükleme ucu | (T312, T340 kriteri) | Planda o task'ların kriteri |

## Notlar

- **⚠ 1:** Skia gif çözücüsünün üçlü lisansından (MPL 1.1 / GPL 2.0 / LGPL 2.1) **MPL 1.1** seçildi (dosya düzeyinde, değiştirilmeden; `09 §3.3` m.4 GPL ve LGPL'yi yasaklar). Yönetici kararı, sahip yetkisiyle.
- **⚠ 2:** SkiaSharp'ın bellek sınırı yok; görüntü işleme eşzamanlılığı 1'e indirildi (tepe ≈ 212 MB < 256 MB tavan). `09 §3.5` ve §15.6 buna göre.
- **⚠ 3:** K-1316 (6) ("yedek geçiş yeni karar gerektirmez") yerine karar kaydı **K-1575**.
- **Working tree (Adım -1):** temiz
- **Repo memory (bitiş kapısı 9):** güncellendi

## Doğrulama Sonucu — T223 Dosya deposu — tur 1
**Tarih:** 2026-11-05   **Branch:** task/T223-dosya-deposu   **Commit:** fe74fb2 (doğrulanan head)

### Verdict: ✗ FAIL

### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş (switch öncesi ve test koşuları sonrası) |
| Hook kurulumu | ✓ | `git config core.hooksPath` → `C:\projects\Shopfolio\scripts\git-hooks` |
| Ana dal CI (son 3) | ✓ | 42068910959 success · 42068911080 success · 42042657292 success |
| Memory drift | ✓ | `.claude/memory/MEMORY.md` satır 22 T223 |

### Kabul Kriterleri
| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Arayüz, disk gerçeklemesi, `InMemoryFileStore` | ✓ | `IFileStore`/`DiskFileStore`/`UnitTests/Fakes/InMemoryFileStore.cs`; `grep` Modules'ta `File.`/`Directory.`/`FileStream` yok; `DiskFileStoreTests` yeşil |
| 2 | Disk adı rastgele/parmak izi; yükleyen adı yol değil | ✓ | `FileKeys` (64/32 hane), `PathOf` her işlemde `Require`; `IFileStore` ad parametresi almaz; `AKeyThatDoesNotFitTheImageSection_IsRefused_ByEveryOperation_BeforeAPathIsMade` (`../etc/passwd`, `a/b`, `..\secret`, `.tmp-abc`) |
| 3 | Tür içerikten; yalnız P-22; tavan P-23 | ✓ | `ImageKinds.Sniff` kütüphaneden önce; `AnSvgWithAnImageName_IsUnsupported_AndNeverReachesTheLibrary` (DecodeCount 0), `APngContent_IsReadAsPng_…`; P-23: `ImageUploads.cs:40` (`> ImageMaxBytes` → `TooLarge`) kod okumasıyla, davranış testi yok (K2); `FileSizeCeilingConsistencyTests` |
| 4 | Yeniden kodlama, 2560, yön, meta veri, ilk kare | ✓ | `ABigSource_…`, `TheOrientationIsAppliedToThePixels_AndNoMetadataIsLeft` (Exif/XMP/IPTC fikstürde var, çıktıda yok; yön 6 renkle), `AMovingWebP_…AsOneFrame`, `ACmykJpeg_…`, `ADerivative_IsAStillWebP…`; yön matrisleri 8 değer için elle doğrulandı |
| 5 | Türevler kuyrukta, yükleme beklemez, türevsizde kaynak | ✓ | `AnUpload_…TheRowAndTheJobInOneTransaction` (iş satırı Pending), `TheDerivativeJob_WritesTheNarrowerWidths_AndASecondRunChanges…`, `ImageServing.KeyFor` + `AnImageWithoutDerivatives_IsServedFromTheSourceCopy_…` |
| 6 | 10.000 px / 40 MP çözülmeden ret | ✓ | `AHugeSmallFile_IsRefusedFromItsHeader_WithoutDecoding` (kenar ve alan, `DecodeCount` 0); `ImageProcessor.cs:182` başlık kapısı |
| 7 | Çözülemeyen dosya ret | ✓ | `ABrokenFile_IsUndecodable_AndATruncatedOneToo`; `ARejectedUpload_WritesNoFileNoRowAndNoJob` |
| 8 | Dijitalde tür sınırı yok; ek olarak sunulur; tavan P-26 | ~ | Tür sınırı yok ✓ (dijital bölüm yalnız anahtar biçimi), `DigitalFileMaxBytes` sabiti + eşitlik testi ✓; **"ek olarak sunulur" ve "tavan aşımı reddedilir" için kod ve test yok** (`grep attachment/octet-stream/DigitalFileMaxBytes` yalnız sabit) — B1 |
| 9 | Bağsız ve bir günden eski görsel imhada silinir | ✓ | `TheImageDisposal_DeletesTheUnlinkedOldImages_AndKeepsTheLinkedOnesAndTheYoungOnes`, `AnImageDeletedByTheDisposal_IsNotDeletedWhileALinkNamesIt_…`, `TheDeleteStatement_NamesEveryTableThatHoldsAForeignKeyToTheImageTable` (pg_constraint'ten bütün FK tabloları), `TheOrphanFileSweep_WritesTheRecordBeforeItDeletesAFile_…` (her silmede kayıt sayısı 1) |

### Task Satırları (`12 §7.5`, adım 2a ve 16)
T223'ün `10` birimi KP-42; satırları (`VAL-A186`…`VAL-A190`) panel/tarayıcı satırlarıdır (T339/T525). `VAL-A465` (`InvalidImage` 422 — uç, T339) ve `VAL-A834` (kayıtsız dosya süpürmesi: görsel bölümü burada ✓, dijital bölüm T302 — D-102) de T223'ün kriterlerine tam eşlenmez. Hiçbir satırın `Durum`'u değişmedi.

| Satır (`VAL-…`) | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` | Backlog kaydı |
|---|---|---|---|---|---|
| VAL-A186…A190 | ORTA/DÜŞÜK/KRİTİK | yok | BEKLEMEDE | BEKLEMEDE (panel uçları T339/T525) | — |
| VAL-A465 | — | yok | BEKLEMEDE | BEKLEMEDE (uç T339; işlemci payı kriter 3, 6, 7'de ✓) | — |
| VAL-A834 | ORTA | yok | BEKLEMEDE | BEKLEMEDE (görsel bölümü ✓; dijital bölüm T302) | — |

`python -I -X utf8 scripts/doc-scans/12_matris_denetle.py --kapsam 1-77`: Hata 0.

### Doğrulama Kontrol Listesi
- [x] KK1 modül dosyaya yalnız arayüzden erişir
- [x] KK2 disk adı rastgele, yükleyen adı yol değil
- [x] KK3 SVG reddi, `.jpg` adlı PNG → PNG; P-23 aşımı (kod okuması; test yok — K2)
- [x] KK4 2560, EXIF konumu yok, yön, tek kare
- [x] KK5 türev kuyrukta, yükleme beklemez, türevsizde kaynak kopya
- [x] KK6 devasa boyutlu küçük dosya
- [x] KK7 bozuk dosya
- [ ] KK8 dijital dosya ek olarak gider, tavan aşımı reddedilir — B1
- [x] KK9 bağsız eski görsel imhada silinir
- [ ] `09 §25` — "arayüz metodunda işlem yok" (§6 m.3, §9.1 m.3) tutmuyor — B2; "yeni paket kanıt ve lisans ölçütü" eksik — B3
- [x] Güvenlik taraması (aşağıda)

### Test Sonuçları
| Tür | Sonuç | Komut | Çıktı |
|---|---|---|---|
| Build | ✓ | `dotnet build Shopfolio.slnx -warnaserror` | 0 uyarı, 0 hata |
| Format | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes` | çıkış 0 |
| Unit (+Consistency) | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` | 1214/1214 |
| Integration (PostgreSQL 18, `shopfolio-t223-val-pg`, port 55631) | ✓ | `--filter-namespace`/`--filter-class`: Files 26, Jobs 77, Disposal+Logging+FileSecrets+SignedEndpoints 82, Web 75, Concurrency+Security+Database 79, Model 496 + 396 + 1 (`--max-threads 3`) | 1232/1232, 0 hata |
| Betik testleri | ✓ | `bash tests/scripts/run-script-tests.sh` | 4 dosya geçti |
| Restore | ✓ | `dotnet restore Shopfolio.slnx --locked-mode` | çıkış 0; `dotnet list package --vulnerable --include-transitive` açık yok |
| Dal CI | ✓ | `gh run list --branch task/T223-dosya-deposu` | 42079512012 success (head fe74fb2) |

### Güvenlik Kontrolü
- Secret sızıntısı: Temiz
- Auth etkisi: Temiz (uç yok)
- Input validation: Temiz — tür ilk baytlardan, piksel ve arabellek tavanı çözmeden önce, yol yalnız biçimi denetlenmiş anahtardan; yalnız K3 (regex `$`)
- Yeni bağımlılık: `SkiaSharp` 4.154.0, `SkiaSharp.NativeAssets.Linux.NoDependencies` 4.154.0 (+ geçişli Win32, macOS) — MIT (nuspec okundu), NuGet açığı yok; gömülü yerel kütüphane kanıtı B3

### Bulgular (`12 §8.2` defect kaydı)
| # | Madde | Severity | Hata sınıfı | Beklenen ↔ gerçekleşen | Etkilenen komşu maddeler | Karar | Etkilenen dosya |
|---|---|---|---|---|---|---|---|
| B1 | Kriter 8, KK8 | ORTA | S1 | **Beklenen:** dijital ürün dosyası indirilecek ek olarak (genel ikili tür, `nosniff`) sunulur, P-26'yı aşan dosya reddedilir (`05 §6.6.3`; KK8 "ek olarak gider, tavan aşımı reddedilir"). **Gerçekleşen:** yalnız `ProductConstants.DigitalFileMaxBytes` ve eşitlik testi; sunuş ve red kodu/testi yok; yapım raporu da `~` verir ("uç T312/T340"). `~` PASS sayılmaz (`12 §6.1`, K-1521) | T312 KK3, KK6; T340 kriter 3 | Düzeltme turu: ya altyapıda iki parça (ek olarak sunuşun başlıkları/sonucu ve P-26 kabul denetimi) testleriyle eklenir, ya `11` T223 kriter 8 ve KK8 planda T312/T340'a devredilir (plan düzeltmesi + karar satırı) | `Infrastructure/Files/`, `ProductConstants.cs` ya da `Docs/11` |
| B2 | `09 §25` (modül sınırları), `09 §6` m.3, `§9.1` m.3 | ORTA | S1 | **Beklenen:** "Modülün arayüz metodu `RunAsync` çağırmaz, çağıranın işlemine katılır; iç içe `RunAsync` programlama hatasıdır" (09 satır 717; §6 m.3 satır 551; `ITransactionRunner` yorumu da aynı). **Gerçekleşen:** `ICatalogModule.UploadImageAsync` (`Contracts/ICatalogModule.cs:45`) → `ImageUploads.UploadAsync` → `_runner.RunAsync(...)` (`ImageUploads.cs:69`) kendi işlemini açar; işlem içindeki bir sayfa ya da komuttan çağrılırsa `InvalidOperationException` verir. Mimari test bunu yakalamıyor | `09 §15.6` m.5, m.7 (yükleme işleyicisi, T339) | Düzeltme turu: dosya yazımı işlem dışında kalacak biçimde işlemi giriş noktası (yükleme işleyicisi/test) açar — ör. işleme+dosya yazımı ayrı arayüz metodu, satır+iş satırı çağıranın işlemine katılan metot; ya da istisna `09`'a karar kaydıyla yazılır | `Modules/Catalog/ImageUploads.cs`, `CatalogModule.cs`, `Contracts/ICatalogModule.cs` |
| B3 | `09 §3.3` m.2, m.4; `09 §25` (yeni paket kanıtı) | ORTA | S1 | **Beklenen:** paketin içinde gömülü gelen yerel kütüphanelerin lisansı doğru ve eksiksiz yazılır. **Gerçekleşen:** tablo satırı ve K-1575 (3) "gif çözücüsü (Mozilla kaynaklı dosyalar; MPL/GPL/LGPL) ikilidedir" deyip MPL 1.1 seçer; ama linux-x64 `libSkiaSharp.so` (nuget önbelleği 4.154.0) Wuffs GIF çözücüsünü taşır (`SkWuffsCodec`, `wuffs_gif__decoder__*` dizgeleri var; `GIFImageReader`/`GIFFrameContext`/`GIFLZWContext` sıfır — dalın libmicrohttpd/DNG için kullandığı yöntemle). Wuffs (Apache-2.0) ne bildirimde ne satırın listesinde; bildirimin "gif" bölümü eski görünüyor. Ayrıca aynı karar Magick.NET'te cairo'nun "MPL ve LGPL"sini kopyaleft sayıp SkiaSharp'ta MPL 1.1'i kabul eder (ölçüt tutarsız); MPL 1.1 seçilirse §3.6'nın yürütülebilir dağıtımdaki "kaynak erişilebilir" bildirimi hiçbir yerde karşılanmıyor | K-1575; `09 §15.6` girişi; MEMORY ⚠ | Düzeltme turu: ikilide Mozilla gif kodunun bulunup bulunmadığı kesinleştirilir (Skia sürümünün `skia_use_wuffs`/third_party/gif durumu); yoksa satır, K-1575 (3) ve ⚠ düzeltilir, Wuffs (Apache-2.0) ve bildirimdeki öteki gömülüler (piex, sfntly, SDL, SPIR-V, zlib) listelenir; varsa MPL bildirim yükümlülüğü runbook'a/imaja yazılır ve cairo ölçütü tutarlı hâle getirilir | `Docs/09_CODING_GUIDELINES.md` (Kütüphaneler), `Docs/PRODUCT_DISCOVERY_STATUS.md` K-1575 |

### Bloke-etmeyen Notlar (K-notları)
| # | Not | Karar |
|---|---|---|
| K1 | Eşzamanlılık 1'in testi yok: iki eşzamanlı `ProcessUploadAsync`/`RenderDerivativeAsync`'in sıraya girdiği sınanmıyor; kod okumasıyla ✓ (tek `SemaphoreSlim(1)`, `Program.cs`'te singleton, iki yol da kapıdan geçer) | bu PR'da kapatılır (düzeltme turu) |
| K2 | P-23 aşımının davranış testi yok (`ImageUploadResult.TooLarge`, sınırda 10 MB kabul / +1 bayt ret) | bu PR'da kapatılır (düzeltme turu) |
| K3 | `FileKeys` regex'leri `$` kullanır; .NET'te `$` sondaki `\n`'den önce de eşleşir — `"<64 hane>\n"` geçer. Yol geçişi değil (anahtarı uygulama üretir) ama biçim denetimi tam değil: `\z` | bu PR'da kapatılır (düzeltme turu) |
| K4 | `ImageDisposalPart.DeleteStatement` `id` ve `created_at` sütunlarını elle yazar (yorum "names come from the model" der); `VAL-A834`'ün ön koşulu 23 saatlik dosyadır, test 1 saatlik kullanır | bu PR'da kapatılır (düzeltme turu) |

### Yapım Raporu Karşılaştırması
- Uyum: 3 uyuşmazlık
- Kriter 8: rapor da `~` verir ve PASS'e engel saymaz; `12 §6.1`/K-1521'e göre FAIL'dir (B1).
- Rapor öz denetiminde modül sınırı/işlem sahibi maddesi yok; B2'yi anmaz.
- Rapor ⚠ 1 ve Dış Varsayım 2 gif çözücüsünü ikilide sayar; doğrulama bunu desteklemiyor (B3). Rapor "§3.3 giriş cümlesi değişti" der; değişen Kütüphaneler tablosunun giriş paragrafıdır (§3.3 değişmedi).
- Öz denetim tablosu var, maddeleri kanıtlı. Rapordaki "Jobs'ta tek geçici hata" bu koşuda görülmedi (77/77).
- Not: Faz 3'ten önce bir `grep` çıktısında raporun kriter 8 satırının ilk sözcükleri göründü; kriter 8 kararı koddaki `grep` kanıtına dayanır.

### Merge ve Post-merge
- FAIL: merge yok. Dal `task/T223-dosya-deposu`'da rapor, durum ve hafıza commit'i.
