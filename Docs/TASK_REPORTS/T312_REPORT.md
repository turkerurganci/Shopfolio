# T312 — Dosya uçları: imzalı indirme ucu ve parçalı yükleme ucu

**Faz:** F3 | **Durum:** ✓ Tamamlandı | **Tarih:** 2026-11-07

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

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-07, head `93e5beb` + `origin/main` birleştirmesi) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 3 (K1 → D-217, K2 → D-218, K3 kod kanıtıyla kapandı) |

### Doğrulama tur 1 — ✓ PASS

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` ana deponun `scripts/git-hooks`'u; main son üç tamamlanmış run 42362112644, 42362112617, 42360805990 `success`; hafızada T312 satırı var.

**Kriterler 1–10 ✓, KK1–KK10 ✓, `09 §25` ✓, güvenlik taraması temiz.** Kanıtlar (doğrulayıcı):

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kriter 1 / KK1 | ✓ | `DownloadEndpoint` yalnız `DownloadLinks.Resolve` → `FindDownloadableItemAsync` → `FindDownloadFileAsync` okur; oturum şeması, sipariş kökü ya da erişim anahtarı yok; `Download_OfASignedValue_OpensWithNoSession_*` |
| Kriter 2 / KK2 | ✓ | `Resolve`: imza (amaç `Shopfolio.DownloadLink.v1`), yük biçimi, `now > expiresAt` → `Invalid`; kalem yok ya da dijital değil → `null`; üçü aynı gövdesiz 404 + `DownloadLinkInvalid` red satırı; `*_WithABadSignature_*`, `*_WithASpentLife_*` (son saniye açılır), `*_OfAnItemThatCannotBeDownloaded_*`, `*_OfAFileWhoseUploadDidNotComplete_*`; `DownloadLinksTests` |
| Kriter 3 / KK3 | ✓ | `Results.File(…, "application/octet-stream", fileDownloadName, lastModified: UploadCompletedAt, entityTag: güçlü, enableRangeProcessing: true)`, `no-store`; ETag = SHA-256(`StorageKey:SizeBytes`) ilk 16 bayt, anahtar başlıkta yok; `DigitalFileReader` varyantın bağlı ve kaldırılmamış dosyası → ürününki → kalemin son dosyası; `*_AfterTheFileIsReplaced_GivesTheNewFileUnderANewTag`, `*_WithARange_Is206_*` (`If-Range` eski etiket → 200), `*_WithARangeBeyondTheFile_Is416` |
| Kriter 4 / KK4 | ✓ | Ömür yalnız isteğin başında okunur; uç yazı yapmaz; `*_SecondDownloadAndRange_ChangeNoCounter_AndWriteNothingToTheTrace`, `*_WhoseLifeEndsWhileTheFileIsSent_IsCompleted_AndTheNextRangeRequestIs404` (3 MB, aktarım ortasında saat +2 saat) |
| Kriter 5 / KK5 | ✓ | Ad boş/boşluk, `/`, `\`, denetim karakteri ya da boy ≤ 0 → 400; geçerliyse `UploadCompletedAt` boş satır, 201; `Start_WithAValidNameAndSize_*`, `Start_WithAnInvalidNameOrSizeOrBody_Answers400_AndOpensNoRow` (9 durum, satır sayısı değişmez) |
| Kriter 6 / KK6 | ✓ | P-26 ya da `DiskUsageGuard.WouldExceed` (eşiğe ulaşma dahil; `05 §7.7.1`) → 422 `maxSizeBytes` = 524288000; `Start_WithASizeAboveTheCeiling_*`, `Start_WithASizeThatBringsTheDiskToItsThreshold_*`; `DiskUsageGuardTests` |
| Kriter 7 / KK7 | ✓ | `DigitalFileLocks` (`FOR UPDATE`, `LockKind.DigitalFile` ayrı aile); alınan bayt kilit altında depodan; uymayan başlangıç → 409 + alınan bayt; `Chunk_TwoEqualChunksAtOnce_GiveOne200AndOne409_AndTheBytesAreWrittenOnce`, `Chunk_ThatDoesNotStartAtTheReceivedBytes_*`, `Chunk_AfterTheUploadIsComplete_*` |
| Kriter 8 / KK8 | ✓ | Sıra: oturum → 404 (gövde okunmadan) → 413 (bildirilen uzunluk okumadan; uzunluksuz gövde en çok 5 MB + 1) → `Content-Range` 400 (`ChunkRange`; uzunluk gövdeyle eşit, toplam yok sayılır) → 409 → taşma 400; `Chunk_AboveFiveMegabytes_*`, `Chunk_ExactlyFiveMegabytes_IsAccepted`, `Chunk_WithAMissingOrMalformedOrWrongLengthContentRange_*` (6 durum), `Chunk_ThatGoesPastTheDeclaredSize_*`, üç `Chunk_Order_*`, gerçek Kestrel'de chunked gövde; 400/404/413 gövdesiz |
| Kriter 9 / KK9 | ✓ | Son parça tamamlanmayı aynı işlemde yazar; alınan bayt = boy ve tamamlanma boşsa bayt eklemeden tamamlar (K-1203); durum 200 `receivedBytes`/`isCompleted`, bilinmeyen 404; `Chunk_AfterACrashThatLeftEveryByteButNoCompletion_*`, `Status_*`, `Chunk_ForAnUnknownOrMalformedIdentity_*` |
| Kriter 10 / KK10 | ✓ | Uçlarda işlem izi çağrısı yok; `Upload_WritesNothingToTheTrace_AndEveryAnswerIsNoStoreAndNoIndex`; indirmede `X-Robots-Tag: noindex` |
| `09 §25` | ✓ | Uçlar `Endpoints/Files/` (CONTEXT güncel); mantık modül sözleşmelerinde; işlem `RunAsync` lambdası; kilit `LockSequence`'e bildirilir; zaman `TimeProvider`; JSON yalnız `MachineJson.Options`; yeni paket, ayar, göç yok; Architecture 818/818; `09` değişiklikleri D-191'de |
| Güvenlik | ✓ | Sır yok; indirme değeri veri koruma halkasıyla imzalı, ömür yükte, adres kayıtlarda maskeli (`PathMasker`); diskteki ad hiçbir başlıkta yok; yükleme uçları yönetici oturumu + başlıktaki sahtecilik değeri (oturumdan sonra); ad yol olarak kullanılmaz (depo yolu `StorageKey`); parça tavanı oturum ve 404'ten sonra, bellek istek başına ≤ 5 MB + 1; yeni bağımlılık yok |

**Testler (doğrulayıcı, yerel):** `dotnet build Shopfolio.slnx -warnaserror` 0 uyarı 0 hata (birleştirmeden önce ve sonra); `dotnet format --verify-no-changes` Web, UnitTests, IntegrationTests ayrı ayrı çıkış 0; Consistency 209/209, Unit (Consistency dışı) 3114/3114; betik testleri 4 dosya geçti; Integration (PostgreSQL 18): `Files.*` 81/81, `Web.ForgeryProtectionTests` + `SessionAccessTests` + `StatusEndpointTests` 97/97, `ConcurrencyTests` 26/26. Dal CI 42362008048 `success` (head `93e5beb`).

**Task satırları (`12 §7.5`):** T312 kaynakları KP-20, KP-44. KP-20 grep'i `VAL-B018`, `VAL-A064`…`VAL-A066`, KP-44 grep'i `VAL-A199`…`VAL-A201` getirir: beklenenleri sipariş sayfasının düğmesi (UC-36, T511), formun kaydı (UC-114) ve hak yenilemesidir — T312 yalnız iki ucu kurar → altısı `BEKLEMEDE`. `VAL-A705` ve `VAL-A552` KP ile eşlenmez; kriter 2'yle örtüşür ama "hak düşmez" ve E-06 beklentisi T511 ve T504'ü ister → `BEKLEMEDE`.

**K1 → D-217:** indirme ucunun 404'ü bugün gövdesiz (makine ucu işareti); `07 §4.31.1` ve `09 §15.7` m.1 bu 404'ün sayfasını E-06 diye yazar — hata sayfası T504'ün.

**K2 → D-218:** "kalemi indirilemez" bugün "kalem yok ya da dijital değil"dir (yapım raporu Known Limitations 1 bunu T511'e bırakıyordu ama backlog kaydı yoktu).

**K3 (kod kanıtıyla kapandı):** denetim karakterli adın 400'ü ayrı test satırı değil; `DigitalFileUploads.IsValidName` `char.IsControl` ile reddeder, aynı yol yol ayırıcılı adla sınanmıştır.

**Yapım raporu karşılaştırması:** uyumlu; öz denetim tablosu her kriter ve KK için kanıtlı. Uyuşmazlık: Known Limitations 1 backlog kimliği taşımıyordu (D-218 açıldı).

**Merge notu:** dal `origin/main` (T310, #423) ile birleştirildi; çakışma yalnız `MEMORY.md` ve `DEFERRED_BACKLOG.md`'de, kayıtlar korunarak çözüldü ("Aktif kalem" main 102 − 4 kapanış + 2 yeni = 100; T310 girişi `STATUS_CHANGELOG.md`'ye taşındı); kodda çakışma yok, birleştirme sonrası derleme temiz.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok (üretim `ForwardedHeaders` yapılandırması değişmedi; yalnız test host'unda loopback güvenildi)
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T312-dosya-uclari`
- **PR:** #426
- **Commit:** `1112ca6` (squash)
- **Post-merge run'lar:** squash `1112ca6` (#426) — CI 42366197638 success, Publish 42366197659 success (T381 yapımında dolduruldu)
- **CI:** yapım işçisi izlemez
- **Post-merge run'lar:** squash `1112ca6` (#426) — CI 42366197638 success, Publish 42366197659 success (T309 yapımında dolduruldu)

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
