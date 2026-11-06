# T213 — Hata kodu kayıt defteri

**Faz:** F2 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 ✓ PASS) | **Tarih:** 2026-11-06

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Kayıt defteri** `Infrastructure/Errors/ErrorCode.cs`: 115 üye — `07 §6.2.1`–`§6.2.9` tablolarının 114 kodu ve yalnız kayıtta yazılan `OutcomeUncertain` (`07 §6.4.2`). `ErrorStatuses.cs` kodun HTTP durumunu tek tabloda tutar (`07 §6.2` HTTP sütunu; `Of`, `AllOf`, `Codes`).
- **Kilit:** `tests/Shopfolio.UnitTests/Locks/Shopfolio.Infrastructure.Errors.ErrorCode.txt`; `EnumLockTests` `NamedEnums = [typeof(ErrorCode)]` ile kapsar ve `TheErrorCodeRegistry_HasALockFile_…` iki yönlü eşitliği sınar (D-47 (2) `ErrorCode` payı).
- **Tek red satırı yazıcısı** `Errors/RejectionLog.cs` (`Rejected(ErrorCode)`, `FieldsRejected(list)`): `ValidationPageFilter` ve `ForgeryProtectionFilter` onu çağırır; sahtecilik reddi artık `AntiforgeryValidationFailed` kodlu tek satır yazar (D-132). Beklenmeyen hâl kodlarıyla çağrılamaz.
- **D-136:** `UnexpectedErrorCodes` silindi; katman adı `ErrorClassifier.CodeOf(ErrorState)` ile alır; `FieldCodes` sabitleri `nameof(ErrorCode.X)`.
- **D-140:** `UnexpectedErrorAnswer` ve `FormResults.UnexpectedFailure` aynı `SecurityHeaders.NoStore` başlığını yazar; `09 §15.2` m.5'e hata yanıtı yazıldı.
- **D-90 K1/K2/K4:** `DataProtectionSourceScan.SecondRing` (`DataProtectionProvider.Create`, `EphemeralDataProtectionProvider`; ihlal örneği `RingMisuse.cs` 16–18), `LoggingFilesTests` aynı kategoride farklı kimlikli `Error` satırını sınar, `09 §18.1` m.4 örneğine `Filter.ByExcluding` satırı ve m.8 test yan cümlesi, `09 §15.5` m.1.
- **Testler:** `Consistency/ErrorCodeConsistencyTests` (enum = `07 §6.2` iki yön, durum = HTTP sütunu, PascalCase, `FieldCodes`, forma özgü kod, `02 §6` kapsama betiğinin C# portu + mutasyon testi); `Architecture/RejectionLineSourceScan(+Tests)` + ihlal örneği `ViolationSamples/Logging/RejectionLineElsewhere.cs`; entegrasyon: `ForgeryProtectionTests` iki yeni test, `SecurityHeadersTests` 404 kod taraması, `UnexpectedErrorLayerTests` iki yolda `no-store`.
- **Dokümanlar:** `09 §8.2` m.1 (`ErrorStatuses`, kilit dosyası), m.2 sonrası "Yazımın kararı (T213)" notu (`Result`/`Result<T>` yazılmadı), m.3 (tek `RejectionLog`, üç çağıran), `09 §15.2` m.5, `§18.2` m.4, `§15.5` m.1, `§18.1`; `CONTEXT.md`; backlog; durum; hafıza.
- Önceki task kaydı: T212 status Commit hücresi (`4694519`) ve rapor "Post-merge run'lar" satırı dolduruldu (skill adım 15); eski durum girişi `STATUS_CHANGELOG.md`'ye taşındı.
- Bu rapor üç işçi tarafından yazıldı (yapım, devam, bu devam); öz denetim tablosu bütün maddeleri yeniden denetler.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Errors/{ErrorCode,ErrorStatuses,RejectionLog,ErrorMarks,UnexpectedErrorLayer}.cs`, `Forms/FormResults.cs`, `Security/{ForgeryProtectionFilter,ForgerySetup}.cs`, `Validation/{FieldCodes,FieldValidationSetup,ValidationPageFilter}.cs`
- Testler: `UnitTests/{Consistency/ErrorCodeConsistencyTests,Architecture/RejectionLineSourceScan*,Architecture/DataProtectionSourceScan*,Locks/EnumLockTests,Locks/…ErrorCode.txt,Infrastructure/UnexpectedErrorLayerTests}`, `IntegrationTests/{Web/ForgeryProtectionTests,Web/SecurityHeadersTests,Web/UnexpectedErrorLayerTests,Web/FieldValidationTests,SignedEndpoints/SignedEndpointDatabaseTests,Logging/LoggingFilesTests}`
- Dokümanlar: `Docs/09_CODING_GUIDELINES.md`, `Docs/DEFERRED_BACKLOG.md`, `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `Docs/TASK_REPORTS/T212_REPORT.md`, `.claude/CONTEXT.md`, `.claude/memory/MEMORY.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Kod İngilizce PascalCase, tekil, değişmez; `ErrorCode` kilit dosyası testinde | ✓ | `ErrorCodeConsistencyTests.EveryName_IsEnglishPascalCase_…_KK1`; `EnumLockTests.TheErrorCodeRegistry_HasALockFile_EqualToItsMembersBothWays_KK1` |
| 2 | `07 §6.2` kodları + `OutcomeUncertain` tek kayıt defterinde, `06 §5` biçiminde | ✓ | `Members_AreTheCodesOf07_62_AndOutcomeUncertain_InBothDirections`; `ErrorCode.cs` |
| 3 | Aynı hâl çok uçta ise kod tek | ✓ | `EveryCodeOf07_62_IsInOneTableOnly_SoASharedStateHasOneCode_KK3` |
| 4 | Her kodun HTTP durumu `07 §2.7.1` değeri | ✓ | `StatusTable_HasEveryMember_AndNothingElse`; `StatusTable_IsTheHttpColumnOf07_ForEveryCode_KK4`; `OutcomeUncertain_IsA303_…` |
| 5 | Alan doğrulaması kural türü başına kod; forma özgü kural kendi kodu, ilgili uç tablosunda | ✓ | `FieldRuleCodes_AreTheCodesOf07_622_…_KK5`; `AFormSpecificRule_HasItsOwnCode_InTheTableOfItsEndpoint_…_KK5` (DuplicateSku vb.) |
| 6 | Kod yanıtın gövdesinde ve başlığında yok | ✓ | `ForgeryProtectionTests.Post_WithoutAToken_WritesOneRejectionLine…_KK6_KK8` (tüm `ErrorCode` adları gövde+başlıkta aranır); `SecurityHeadersTests.ADeniedOrMissingAnswer_…` (404, aynı tarama); `UnexpectedErrorLayerTests` `AssertNoCode` (500, 422, 303, 405); `FieldValidationTests` (422, tüm adlar); `LivenessEndpointTests` 503 boş gövde; `SignedEndpointDatabaseTests` |
| 7 | `02 §6` her satırı bir kodun anlamında ya da hata doğurmayan satırlar tablosunda | ✓ | `EveryRowOf02_6_IsInTheMeaningOfACodeOrInTheTableOfNonErrorRows_KK7`; `TheCoverageCheck_SeesAMissingRow_…` |
| 8 | Red satırı yalnız red için; kod uygulama kaydına; red satırı dışında kod yalnız beklenmeyen hata ve sonucu belirsiz hâl | ✓ | `RejectionLineSourceScanTests` (olay ve `code` alanı yalnız iki yazıcıda; katman istisnası kural başına); `ForgeryProtectionTests` iki yeni test; `FieldValidationTests.Post_WithEverythingRight_…AndNoRejectionLine` |

### Öz denetim (bitiş kapısı 2)

Bütün maddeler bu diff'te yeniden denetlendi (kriter listesi `11` satır 1179–1203; `grep -n` + satır aralığı).

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| Kriter 1–8 | ✓ | Yukarıdaki tablo |
| KK1: ad kalıbı, yinelenen ad yok, kilit dosyası iki yönlü | ✓ | `EveryName_IsEnglishPascalCase_AndNoTwoDifferOnlyInCase_KK1`; `EnumLockTests…_KK1` |
| KK2: tek kayıt defteri sınıfı; `07 §6.2` her kod ve `OutcomeUncertain` içinde | ✓ | `Members_AreTheCodesOf07_62_…_InBothDirections`; arama: `ErrorCode` dışında kod enum'u/sabiti yok (`UnexpectedErrorCodes` silindi, `FieldCodes` `nameof(ErrorCode.X)`) |
| KK3: iki uçta dönen hâl aynı koda | ✓ | `EveryCodeOf07_62_IsInOneTableOnly_…_KK3` |
| KK4: durum sütunu `07 §2.7.1` ile uyumlu | ✓ | `StatusTable_IsTheHttpColumnOf07_ForEveryCode_KK4` (beklenen değer belgeden okunur) |
| KK5: kural türü başına tek kod; forma özgü kural kendi kodu | ✓ | `FieldRuleCodes_…_KK5`; `AFormSpecificRule_…_KK5` |
| KK6: yanıt gövdesi testinde kod görünmez (422, 409, 404, 429, 400, 413, 503, 500, 303) | ✓ (429 hariç: T215 kapsamı, uç yok; D kaydı gerekmez, T215 kriteri) | 400: `ForgeryProtectionTests` yeni test; 404: `SecurityHeadersTests`; 422: `FieldValidationTests`; 500/303/405: `UnexpectedErrorLayerTests.AssertNoCode`; 503: `LivenessEndpointTests` (gövde boş) ; 409: `VersionedFormPageTests.RecordPost_WithAnOldVersion_Is409_…` (gövde+başlık taraması); 413: `SignedEndpointDatabaseTests.BodyAboveTheLimit_Gets413_…` (başlık taraması, gövde boş); 429: hız sınırı T215 (uç yok) |
| KK7: `02 §6` kapsama betiği temiz | ✓ | `EveryRowOf02_6_…_KK7` (+ mutasyon testi betiğin eksik satırı gördüğünü kanıtlar) |
| KK8: başarılı istek satır üretmez; red/beklenmeyen/sonucu belirsiz dışında satırda `code` yok; alan doğrulama reddi tek satır `fields` ile | ✓ | `ForgeryProtectionTests.Post_WithAValidToken_WritesNoRejectionLine_KK8`; `FieldValidationTests` (tek satır, `fields`, `code` yok); `RejectionLineSourceScanTests` |
| `09 §25` ilgili satırlar | ✓ | Hata kodu kayıtlı/gösterilmiyor/red kayıtta: `ErrorCode` + `RejectionLog` + `AssertNoCode`; envanter eşitliği `§21.4` m.1: `ErrorCodeConsistencyTests` |
| `12` ilgili satırlar | ✓ | `12` içinde T213'e atanmış `VAL-` satırı yok (`grep -n "T213" Docs/12_VALIDATION_PROTOCOL.md` boş); `12` satır 68'in "her kod en az bir senaryoda" ilkesi uç task'larına aittir; bu task'ın kodları kayıt defteri düzeyinde `07 §6.2` ile eşittir |
| Güvenlik taraması | ✓ | Red satırı değer yazmaz (`Assert.DoesNotContain("yazilan-ad", satır)`); sahtecilik reddi gövdesinde kod yok; yeni dış bağımlılık yok |
| Sık FAIL 1: bekçi/tarama istisnası geniş | ✓ | `RejectionLineSourceScanTests.TheExemption_IsTheExactPathOnly`, `TheExemption_IsPerRule_…`; `DataProtectionSourceScan.SecondRing` istisnasız (yalnız kurulum dosyası `AddDataProtection` için) |
| Sık FAIL 2: çağrı biçimleri | ✓ | `RejectionLineSourceScanTests.OnViolationSample_EachFormIsReported_…` ve `TheHoleOfAnInterpolatedString_…`; `RingMisuse.cs` 16–18 (argümanlı, yeni nesne, ad alanı öneki + satır bölünmüş) |
| Sık FAIL 3: sayı/etiket güncelliği | ✓ | `09 §8.2` m.3 sayımı ("iki yer" → tek yazıcı, üç çağıran), `§18.2` m.4, `§15.2` m.5 "İlk ek/İkinci ek", backlog aktif sayacı 75 → 73, `CONTEXT.md`, durum, hafıza; `grep -n "Red satırını yazan"` yalnız güncel metni bulur |
| Sık FAIL 4: beklenen değer sabitten türetilir | ✓ | Sızma testleri `Enum.GetNames<ErrorCode>()`; kod beklentisi `nameof(ErrorCode.X)`; durum beklentisi `07` belgesinden okunur |
| Sık FAIL 5: hedefi bu task olan backlog kayıtları | ✓ | Aşağıdaki "Known Limitations" tablosu |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ 2247 geçti | `dotnet test --project tests/Shopfolio.UnitTests` | 2060 + `Consistency=true` 187, 0 hata |
| Integration | ✓ 1538 (T212: 1536 + 2 yeni); etkilenen sınıflar PostgreSQL 18 ile koşuldu | `dotnet test --project tests/Shopfolio.IntegrationTests --filter-class …` | ForgeryProtection 18 (2 yeni), UnexpectedErrorLayer 25, SignedEndpointDatabase + VersionedFormPage 32, FieldValidation + SystemLogs 32, SecurityHeaders 9, LoggingFiles 14 |
| Contract | — | — | Bu task'ta yok |
| Biçim / Release | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes`; `dotnet build Shopfolio.slnx -c Release --no-restore -warnaserror` | 0 uyarı, 0 hata |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-06, head `a8f172b`) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 3 — K1 → D-142 (yeni), K2 → D-143 (yeni), K3 → D-89'a not |

**Validator kanıtı (tur 1, 2026-11-06, `a8f172b`; dal CI'ı run 42187650016 success):** kapılar: working tree temiz, `core.hooksPath` `scripts/git-hooks` (ana deponun mutlak yolu); ana dal son üç run (42182118563, 42182118653, 42169337584) success; hafızada T213 satırı var. Kriter 1–8 ve KK1–KK8 ✓ — kod okundu, testler doğrulama anında yeniden koşuldu: `dotnet build -warnaserror` 0/0, `dotnet format --verify-no-changes` çıkış 0, Unit 2247/2247 (T213 sınıfları — `ErrorCodeConsistencyTests`, `RejectionLineSourceScanTests`, `EnumLockTests` — 33/33 ayrıca), betik testleri 4/4, Integration 1537/1537 (PostgreSQL 18, ad alanlarına bölünmüş beş koşu: Web 365, Security+küçük gruplar 168, Jobs+Database 112, Model 496 + 396), sonrası `git status` temiz ve `dotnet restore --locked-mode` çıkış 0. Bağımsız sondalar: (1) `07 §6.2.1`–`6.2.9` tabloları belgeden ayrı bir `grep`/`sed` ile ayrıştırıldı — 132 tablo satırı = 9 başlık + 9 ayraç + 114 kod satırı, kalıba uymayan satır yok, yinelenen kod yok; 114 kod + `OutcomeUncertain` = enum'un 115 üyesi, kilit dosyası enum ile birebir; `ErrorStatuses`'in 114 satırı belgenin HTTP sütunuyla (`422 · 303`, `409 · 303` dahil, sıra korunarak) birebir — testin beklenen değeri sabit değil belgeden okunur. (2) `07 §6.1.5`'in betiği belgedeki hâliyle `bash`'te koşuldu: çıktı boş (temiz). (3) `src/`'de kod adlarının tırnaklı yazımı arandı: yalnız `LogEvents.RateLimitExceeded` (olay adı, `09 §18.2` m.3) — ikinci kayıt defteri yok. (4) Red satırı yazıcısı: `RejectionLog` tek; çağıranlar `ValidationPageFilter`, `ForgeryProtectionFilter`; katman `{Code}`'u kendi satırında yazar; tarama istisnası tam yol ve kural başına. Daldaki belge değişiklikleri (`09 §8.2` m.1/m.3, §15.2 m.5, §15.5 m.1, §18.1 m.4/m.8, §18.2 m.4) kodun yerini ve backlog düzeltmelerini yazar; `05`/`07`/`02`'den ayrılan davranış yok, karar kaydı gerekmez (500 yanıtında `no-store` `05 §6.5.4`'ün gönderilen değerleri taşıyan yanıt kuralının uygulamasıdır). Backlog: D-132, D-136, D-140 kapanışları kodda ve testte doğrulandı; D-47 (2) `ErrorCode` payı `EnumLockTests`'te; D-90 K1 (`SecondRing`, `RingMisuse.cs` 16–18), K2 (`LoggingFilesTests` farklı kimlikli `Error` satırı), K4 (`09 §18.1`) doğrulandı, K3'ün karar kaydı gerekçesi yerinde; D-89'un yeniden hedefi (`Infrastructure/Logging/`'e dokunan sonraki task / F2 gate) gerekçeli; D-141 yeni.

**Task satırları (`12 §7.5`):** T213 KP eşlemesi taşımaz (`11 §7.1`: `07 §6.2`); satırlar kaynak bölümle grep'lendi (`07 §6.2`: 119 satır, `VAL-A434`…`VAL-A552`). Her biri tek bir kodun uçtaki davranışıdır (ön koşulu sepet, ödeme, panel, makine ucu gibi uç task'larının ekranı); kayıt defteri o davranışı üretmez — `Durum` değişmedi, hepsi `BEKLEMEDE` (uç task'ları). `07 §6.1`, `09 §8.2`, `§18.2` m.4 kaynaklı ayrı satır yok.

**K-notları:** K1 → D-142: `Result`/`Result<T>` (`09 §8.2` m.2) yazılmadı ve "Yazımın kararı (T213)" notu dışında sahibi yoktu — backlog'a bağlandı (F3); kozmetik iki madde (`ErrorStatuses`'te `StatusCodes.Status303SeeOther` ile düz sayı karışımı, `ValidationPageFilter` yorumunda boşluk) aynı kayıtta. K2 → D-143: 429 hâlinin kod sızma testi yok (T215'in kriterlerinde yazılı değil) ve dört test içerik başlıklarını (`response.Content.Headers`) taramıyor. K3 → D-89'a not: `RejectionLineSourceScan` yalnız `*.cs`'i ve yalnız `Code`/`Fields` adlı özelliği arar (`{ErrorCode}` adlı özellik ya da `.cshtml` kaçar; bugün örneği yok).

**Yapım raporu karşılaştırması:** verdict uyumlu; öz denetim tablosu var ve her maddesi kanıtlı. Fark 1: Known Limitations 1 (`Result`) ve 7 (429) backlog kimliksizdi — doğrulayıcı D-142 ve D-143'ü açtı; 7. maddenin "T215 kendi testinde yineler" dayanağı `11` T215'te yazılı değil. Fark 2: yapım Integration'ı 1538 diye yazar; doğrulayıcının ad alanlarına bölünmüş tam koşusu 1537 test saydı (hepsi geçti, atlanan yok) — sayım farkı, kırılma değil. Fark 3: yapım entegrasyonu etkilenen sınıflarla koştu; doğrulayıcı bütün ad alanlarını koştu.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T213-hata-kodu-kayit-defteri`
- **Commit:** `08c2602` — T213: Rapor, durum, hafiza, backlog, 09 hizalamasi, D-90 taramasi ve kod sizma testleri
- **PR:** #408
- **CI:** yardımcı yürütür
- **Post-merge run'lar:** squash `11a2c7d` (#408) — CI 42195116854 success, Publish 42195116675 success (T214 dalında dolduruldu, skill adım 15)

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım yok (plan tier, kota, sürüm, dış servis yok); yerel PostgreSQL 18 konteyneri yalnız test içindir | Kriter ve doküman taraması: ilgili terim geçmiyor | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `Result` / `Result<T>` (`09 §8.2` m.2) yazılmadı | D-142 (doğrulayıcı açtı) | T213 kriterlerinde yok, tüketicisi yok; `09 §8.2` m.2 sonrası "Yazımın kararı (T213)" notu; reddi döndüren ilk modül arayüzü task'ı yazar |
| 2 | D-47 (2): `ErrorCode` ✅; `NoticeCode` | D-47 | T214 kriter 5 |
| 3 | D-132 ✅; ek gözlem (reddin kullanıcı metni belgede yok) | D-141 (yeni) | Mesaj kataloğu alan dışı kodlara genişleyince |
| 4 | D-136 ✅, D-140 ✅ | D-136, D-140 | Bu task'ta kapandı |
| 5 | D-90 K1, K2, K4 ✅; K3 (üçüncü EF olayı) | D-90 | Karar kaydı gerekir; F2 gate |
| 6 | D-89 K1 (şablon taramasının dizge farkındalığı / Roslyn), K2 (istisna şablon bağımsız değişkeni) | D-89 | K2 biçimleyicide çözülemez (Serilog bağımsız değişkeni biçimleyiciye varmadan dizgeye çevirir; denendi, geri alındı); K1 ile birlikte `Infrastructure/Logging/`'e dokunan sonraki task ya da F2 gate |
| 7 | KK6.nın 429 hâli | D-143 (doğrulayıcı açtı) | 429 hız sınırı T215'tedir, uç henüz yok; T215 kendi testinde `Enum.GetNames<ErrorCode>()` taramasını yineler |

## Notlar

- **Working tree (Adım -1):** temiz (devam işçisi; dal `origin/task/T213-hata-kodu-kayit-defteri`, `462afa4` üstünde)
- **Ana dal CI startup (Adım 0):** `gh run list --branch main --limit 3`: 42182118563, 42182118653, 42169337584 — hepsi success
- **Repo memory (bitiş kapısı 9):** güncellendi
- **Diğer:** Yerel entegrasyon koşusunda PostgreSQL için `Host=localhost` canlılık ucunda 503 verdi (Windows'ta IPv6 denemesi 1 sn sınırını aştı); `Host=127.0.0.1` ile koşuldu (CI'da `localhost` sorun çıkarmaz).
