# T312 — Dosya uçları: imzalı indirme ucu ve parçalı yükleme ucu

**Faz:** F3 | **Durum:** ⏳ Doğrulama bekliyor | **Tarih:** 2026-11-07

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **İndirme ucu** (`Endpoints/Files/DownloadEndpoint`, `/indir/{değer}`): imzalı değer (`Security/DownloadLinks`) oturum ve sipariş kökü aranmadan açılır; imza, ömür ve "kalem indirilemez" üç ayrı nedenle gövdesiz 404; 200/206 `Results.File` ile (ek olarak, genel ikili tür, `nosniff`, `no-store`, `X-Robots-Tag: noindex`); ETag `Security/DownloadEntityTag`; ömür yalnız kabulde denetlenir, indirme sayaç değiştirmez.
- **Yükleme uçları** (`Endpoints/Files/UploadEndpoints`, `/panel/yukleme`): başlat (201; ad/boy 400; P-26 ya da disk eşiği 422 ve sınır gövdede), parça (`Content-Range`, `ChunkRange`; 5 MB tavanı 413; eksik/uyumsuz başlık 400; alınan baytın ardında değilse 409 ve alınan bayt; son parça aynı işlemde tamamlar), durum (alınan bayt ve tamamlandı; bilinmeyen 404). Denetim sırası: oturum, 404, 413, 400, 409.
- **Sahtecilik değeri başlıkta** (`Security/HeaderForgeryToken`, `AntiforgeryExemptions.HeaderTokenPaths`; muafiyet değil, kontrol açık); makine ucu işareti; gövde `MachineJson.Options`.
- **Kilit ve okuyucu:** `LockKind.DigitalFile` + `Modules/Catalog/DigitalFileLocks`, `DigitalFileReader`, `DigitalFileUploads`.
- **Belgeler:** 09 §9.1 m.5, §10.2 m.5-6, §15.2 m.1, §15.7 m.6(b); DEFERRED_BACKLOG D-131, D-139, D-148, D-149 ✅, D-111 K4, yeni D-191 ✅ (09 değişiklikleri karar kaydı); D-125/126/127/144 yeniden hedeflendi; CONTEXT, MEMORY.

## Kabul Kriterleri Kontrolü / Öz denetim

| # | Kriter / KK | Sonuç | Kanıt |
|---|---|---|---|
| 1 / KK1 | İmzalı değer oturumsuz açılır | ✓ | `DownloadEndpointTests.Download_OfASignedValue_OpensWithNoSession_*` |
| 2 / KK2 | Üç 404 nedeni ayrı | ✓ | `*_WithABadSignature_*`, `*_WithASpentLife_*`, `*_OfAnItemThatCannotBeDownloaded_*` (+ yarım yükleme) |
| 3 / KK3 | Güncel dosya, başlıklar, 206 | ✓ | `*_AfterTheFileIsReplaced_*`, `*_WithARange_Is206_*`, `*_WithARangeBeyondTheFile_Is416` |
| 4 / KK4 | Sayaç değişmez; ömür aktarım sürerken dolarsa tamamlanır | ✓ | `*_SecondDownloadAndRange_ChangeNoCounter_*`, `*_WhoseLifeEndsWhileTheFileIsSent_*` |
| 5 / KK5 | Başlatma satır açar; 400'de satır yok | ✓ | `UploadEndpointsTests.Start_*` |
| 6 / KK6 | 422 gövdesinde sınır | ✓ | `UploadEndpointsTests.Start_*` (P-26, disk eşiği) |
| 7 / KK7 | Eşzamanlı iki parçadan biri 409 | ✓ | `Chunk_TwoEqualChunksAtOnce_*`, `Chunk_ThatDoesNotStart…` |
| 8 / KK8 | 413 ve iki 400 gövdesiz; denetim sırası | ✓ | `Chunk_AboveFiveMegabytes_*` (gerçek Kestrel chunked dahil), `Chunk_Order_*`, `Chunk_WithAMissingOrMalformed…` |
| 9 / KK9 | Çökme sonrası son parça; durum; bilinmeyen 404 | ✓ | `Chunk_AfterACrash…`, `Status_*` |
| 10 / KK10 | İzde satır yok; başlık var | ✓ | `Upload_WritesNothingToTheTrace_AndEveryAnswerIsNoStoreAndNoIndex`, indirmede X-Robots-Tag başlık testi |
| `09 §25` | İlgili satırlar | ✓ | Taramalar: `ForgeryProtectionSourceScanTests`, `FormVersionSourceScanTests`, `ConcurrencySourceScanTests`; birim: `DownloadLinksTests`, `DiskUsageGuardTests`, `ChunkRangeTests`, `RequestBodyLimitMiddlewareTests` |

**sık-FAIL nedenleri kontrolü:**

1. Tarama istisnası geniş: açılmadı. `FormVersionSourceScan`'e tek dar istisna (indirme dosyasında iki ad, başka dosyada bulgu — iki ihlal testi); `LinkKeySourceScan` listesi açılmadı (SHA256 `DownloadEntityTag.cs`); `ThirdPartyContentScan` gevşetilmedi (takma ad).
2. Çağrı biçimi kaçırma: `FormVersionSourceScanTests` `Headers.ETag`/`If-Match` biçimlerini ayrıca sınar; başlık-jeton yolu kaynak taramada yalnız `UploadEndpoints.cs` tam yolunda kabul.
3. 06 "Varsayılan"/FK dizini: şema ve göç değişmedi (T312 göç eklemedi).
4. Sayı anan öteki yerler: backlog "Aktif kalem" 101 → 97 (94 ⚪ + 3 🟡 sayıldı); 09 §15.2 m.1 ve §15.7 m.6(b) atıfları hizalı; STATUS satırı ⏳.
5. Karar kaydı gerektiren 09 metni: D-191 (09 değişiklikleri).

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Build | ✓ | `dotnet build Shopfolio.slnx -warnaserror` | 0 uyarı, 0 hata |
| Unit | ✓ 3323/3323 | `Shopfolio.UnitTests.exe` (tam; Consistency 209 dahil) | 0 hata |
| Integration | ✓ parçalı | Files 81, Database 36, Jobs 77, Logging 30, Disposal 18, FileSecrets 24, Outages 19, Model 939, Concurrency 26, Security 32, SignedEndpoints 11, `Web.ForgeryProtectionTests` 20 | 0 hata (PostgreSQL 18, `t312`, silindi). `Web` ad alanının tamamı 150 sn'de bitmedi (önceki turda 594 yeşildi); tam Web koşusu dal CI'ında |
| Format | ✓ | `dotnet format <Web|Unit|Integration> --no-restore --verify-no-changes` | değişiklik yok |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok (üretim `ForwardedHeaders` yapılandırması değişmedi; yalnız test host'unda loopback güvenildi)
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T312-dosya-uclari`
- **PR:** açıldığında PR gövdesinde
- **CI:** yapım işçisi izlemez

## Dış Varsayımlar

Dış varsayım: yok.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Hak/ödeme denetimi ("kalem indirilemez" yalnız kalem yok / Digital değil) | T511 (UC-36) | Hak sorusu sipariş tarafındadır |
| 2 | D-125, D-126, D-127, D-144 yeniden hedeflendi | D-125 → T525; D-126 → F3 gate; D-127 → T339/T330; D-144 → T601 | T312'nin konusu değil |

## Notlar

- **⚠ Kolay kararlar:** (1) `Content-Range` aralık uzunluğu gövde boyuna eşit değilse 400, toplam alanı yok sayılır (D-191, 09 §15.7 m.6(b)). (2) `LockKind.DigitalFile` ayrı aile; `DigitalFileLocks.cs` taramada yalnız düz FOR UPDATE yeri. (3) Chunk gövde tavanı ara katmandan uca taşındı (sıra: oturum, 404, 413, 400, 409); uzunluksuz gövde oturum+404 sonrası en çok 5 MB + 1 okunur (D-148). (4) Dosya satırı tamam ama depoda yok → gövdesiz 500. (5) ETag SHA256'sı `Security/DownloadEntityTag.cs`'te. (6) Başlık-jeton uçları `HeaderTokenPaths`'te; kontrol açık. (7) Gerçek Kestrel testinde ForwardedHeaders'a yalnız test host'unda loopback güvenildi. (8) `ThirdPartyContentScan`: `object` anahtar sözcüğü için takma ad (tarama gevşetilmedi).
- **Repo memory (bitiş kapısı 9):** güncellendi.
- **Devir:** önceki üç işçi bağlam sınırında bölündü; bu işçi main'i birleştirdi (backlog sayacı çakışması çözüldü), CONTEXT/MEMORY/rapor kayıtlarını tamamladı.
