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

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj` | 1214 geçti, 0 kaldı |
| Integration | ✓ | aynı proje, `--filter-namespace` parçaları (Model `--max-threads 3`) | 1232 geçti (Model 893; Disposal+Jobs+Concurrency+Files 147; öteki 192); Jobs'ta bir koşuda tek bir geçici hata, yeniden koşuda yok |
| Contract | — | — | Bu task'ta sözleşme testi yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

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
