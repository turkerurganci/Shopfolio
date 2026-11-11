# T550 — F4 kapanış düzeltmeleri: null-forgiving temizliği ve sır bekçisi önek soyma

**Faz:** F5 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-11

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Null-forgiving temizliği (KK1, KK3; D-778 karar (c)):** `src` altında `!` taşıyan 199 satırın (102'si Ordering — para yolu) her biri ya kaldırıldı ya aynı satırda `// non-null: <gerekçe>` yorumu aldı.
  - Kaldırılanlar: Ordering'de `[NotNullWhen]` kalkanları (`TryAdminActor`, `TryCancellationActor`, `IsAdmin`, `TryNormalizeShipment`, `TryPlan`) ve açık istisnalar (`?? throw`: FirmCancellation `request.Admin`, Carts `GuestCartKey`, UnpaidCancellationLimit L7 penceresi, DefectRemedies `check.Amount`/`ClaimId`, `TransitionTable.ActorsOf`); Payment yolunda `PayTrPaymentGateway` token yanıtının `failure`'ı (`?? throw`), `PayTrStartForm.TryClientAddress` (`text is null ||`); `PriceLabelRules` (`netQuantity is not { } quantity`), `BrandingSettings` (`BrandName ?? _domain`), `JobRunnerService` (filtre `current is not null` zaten akış analizine giriyor).
  - Kalanlar: hemen önceki `Where(... != null)`, doğrulama reddi ya da çağıranın sözleşmesi gerekçesiyle yorum.
- **Mimari tarama (KK2):** `tests/Shopfolio.UnitTests/Architecture/NullForgivingSourceScan.cs` — sözcük çözücü yorum, metin, verbatim, raw ve char literal içeriğini boşaltır, interpolasyon deliğini kod bırakır; literalin kapanış tırnağı işaret olarak kalır (`"x"!` yakalanır); boşluklu biçim (`x !.`, `x !;`) yakalanır, `return !flag` yakalanmaz. `.cshtml`/`.razor` dar desenle. `NullForgivingSourceScanTests`: `src` 0, ihlal örneği 9 satır kırmızı, yorumlu/kaldırılmış/benzer biçimler yeşil, 23 tek satırlık teori.
- **Belge (K-1614):** `09 §3.1` madde 5'e gerekçe biçimi ve tarama yazıldı (v0.34); karar kaydı `PRODUCT_DISCOVERY_STATUS.md` K-1614.
- **Sır bekçisi (KK4, KK5; D-1036):** `scripts/git-hooks/pre-commit` anahtar denetimi `is_secret_assignment` işlevine taşındı; satırdaki her `-e`/`--env` önekinin tek kabuk sözcüğü ayrı aday; satır başı adayı eskisi gibi; yorum/düzyazı satırı bütünüyle atlanır. `test-pre-commit.sh` e7–e13.
- **Paket betiği (KK6):** `scripts/ci/package-build.sh` taraması satır başına bağlı değildir (her atama ayrı eşleşme) — soyacak önek yok; gerekçe betikte yorum. `grep -o` ile `_FILE` istisnası eşleşme başına uygulanır (eskiden `_FILE`'lı satırın tamamı düşüyor, aynı satırdaki gerçek değer kaçıyordu).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/` — Ordering, AfterSales, CorporateContent, Settings, Catalog, Notifications, Membership, Administration, Payment (yorum ya da kaldırış; davranış aynı)
- `src/Shopfolio.Web/Infrastructure/` — Integrations (PayTr, Google), Jobs, LogRead, Logging, Security, SignedEndpoints, Validation, DataModel, AdvisoryLocks; `Commands/ReadLogsCommand.cs`, `Endpoints/Files/UploadEndpoints.cs`
- `tests/Shopfolio.UnitTests/Architecture/NullForgivingSourceScan*.cs`, `ViolationSamples/NullForgiving/NullForgivingUses.cs`
- `scripts/git-hooks/pre-commit`, `scripts/git-hooks/test-pre-commit.sh`, `scripts/ci/package-build.sh`
- `Docs/09_CODING_GUIDELINES.md` (§3.1 m.5, v0.34), `Docs/PRODUCT_DISCOVERY_STATUS.md` (K-1614), `Docs/DEFERRED_BACKLOG.md` (D-778, D-1036 ✅), `Docs/DEFERRED_BACKLOG.d/T550.md` (D-1120), `Docs/STATUS_CHANGELOG.d/T550.md`, `Docs/IMPLEMENTATION_STATUS.md` (T550 satırı)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | `src` altındaki her `!` kalkar ya da aynı satırda gerekçe yorumu alır | ✓ | `git grep -nP '[A-Za-z0-9_)\]>]!(?!=)' -- 'src/**.cs' \| grep -v "// non-null:"` → 0 satır; tarama testi 0 |
| 2 | Mimari test gerekçesiz kullanımı sayar, 0; ihlal örneği kırmızı, yorumlu ve kaldırılmış yeşil | ✓ | `NullForgivingSourceScanTests.ApplicationSource_HasNoNullForgivingWithoutReason`, `Scan_OnViolationSample_…` (satır 5–13 bulgu; 14–23 değil), `Scan_OnOneLine_…` (23 satır), `ScanRazor_…` |
| 3 | Para yolunda davranış aynı; var olan testler değişmeden geçer; `null` açık istisna | ✓ | Mevcut test dosyalarına dokunulmadı (diff yalnız yeni tarama dosyaları); Unit 6628/6628; Integration Ordering 637/637, Payment 59/59, Concurrency 30/30; konan denetimler `?? throw new InvalidOperationException(…)` ya da `[NotNullWhen]`; sessiz varsayılan yok |
| 4 | Satırdaki her `-e`/`--env` öneki ayrı aday; ikinci önek ve tek satırlık komut yakalanır | ✓ | `pre-commit` `is_secret_assignment` + `options` döngüsü; `test-pre-commit.sh` e7, e8, e9 |
| 5 | Kanca testine iki biçimin `check 1` satırları; D-80 devam satırı ve eski örnekler aynı | ✓ | e7–e10 `check 1`, e11–e13 `check 0`; e1–e6 değişmedi; `bash scripts/git-hooks/test-pre-commit.sh` → 223 passed, 0 failed |
| 6 | Paket betiği eşitlenir ya da gerekçe yazılır | ✓ | `scripts/ci/package-build.sh:67–70` yorum (önek soyma yok, desen satır başına bağlı değil); `grep -rnoE` ile `_FILE` istisnası eşleşme başına |

### Öz denetim (bitiş kapısı 2)

> Devam işçisi önceki işçinin yaptıkları dahil bütün maddeleri yeniden denetledi.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1 `!` envanteri; her kullanım kaldırılmış ya da yorumlu | ✓ | `git grep` envanteri 0; 92 yorum + 5 kaldırış (bu tur), Ordering 102 (önceki tur, `001debe2`); gerekçeler kod okunarak doğrulandı (devir notunun "doğrulanmamış" listesi: Coupons → `CouponRules.CheckDefinition`/`CheckFixedAmount`; DigitalFileUploads → `IsValidName`; PriceLabelRules → kaldırıldı; FirmSettings → `CheckRequiredAmount`/`CheckFenced`; LegalTexts:173 → `PrivacyNotice` için `HostingLocation` FieldRequired; RefundAmounts:125 → `receipts` sorgusu `WithdrawalId != null`; LegalTexts:202/212, LegalTextGenerator:60 → `CreatePublished` iki alanı da yazar) |
| KK2 mimari test 0 ve ihlal örnekleri | ✓ | `NullForgivingSourceScanTests` 30/30; ihlal örneği ayrı dosyada, derlenmez |
| KK3 para yolunda davranış aynı; `null` açık istisna | ✓ | Ordering/Payment testleri değişmeden yeşil (üstte); `PaymentModule.cs:386` yorum (bellek içi `!.Value` boşta zaten istisna atar) |
| KK4 iki yeni biçim yakalanır | ✓ | `test-pre-commit.sh` e7, e8 (+ e9, e10) |
| KK5 kanca testinde iki `check 1`; eski satırlar aynı | ✓ | 223/223 |
| KK6 paket betiği eşit ya da gerekçeli | ✓ | `package-build.sh:67–72` (yorum 67–70, tarama 72) |
| `09 §25` "C# standartları (`!` …) uygulanmış mı" | ✓ | KK1 + tarama |
| `09 §25` "Diff yalnız görevin istediğini mi içeriyor" | ✓ | Kaldırışlar davranış değiştirmiyor; ek ayar yok |
| `09 §25` "İstisna yalnız beklenmeyen hâlde mi" | ✓ | Konan `?? throw`'lar yalnız sözleşme bozulursa çalışır |
| Güvenlik taraması (mini) | ✓ | Sır yok (bekçi geçti); auth etkisi yok; girdi doğrulaması değişmedi; yeni bağımlılık yok |
| Sık FAIL (1) istisna geniş | ✓ | Gerekçe yalnız aynı satırda, işaretli ve ≥10 karakter; işaretsiz/blok/kısa yorum bulgu (teori satırları); bekçinin istisnası genişletilmedi (D-1120) |
| Sık FAIL (2) taramanın kaçırdığı biçim | ✓ | Literal ardından `!`, boşluklu `!`, interpolasyon deliği, `]!`, `>!`, Razor biçimi örnekli |
| Sık FAIL (4) aynı sayıyı anan öteki yerler | ✓ | `09` §23 tablosu "gerekçe yorumlu tek satır istisna" — kural değişmedi, biçim eklendi; `09` sürüm başlığı ve dipnotu v0.34 |
| Sık FAIL (5) yeni 09 metni karar kaydısız | ✓ | K-1614 |
| `dotnet format` | ✓ | `dotnet format src/Shopfolio.Web/Shopfolio.Web.csproj --verify-no-changes` → 0; UnitTests değişen dosyalar → 0 |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj` | 6628/6628 |
| Integration | ✓ (parça) | `dotnet test --project tests/Shopfolio.IntegrationTests/… --filter-namespace "…Ordering*"` / `Payment*` / `Concurrency*` (Postgres 18, `track_commit_timestamp=on`) | 637/637, 59/59, 30/30 |
| Kanca | ✓ | `bash scripts/git-hooks/test-pre-commit.sh` | 223 passed, 0 failed |
| Contract | — | — | task kapsamında yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Var — `scripts/ci/package-build.sh` sır taramasında `_FILE` istisnası eşleşme başına (daha sıkı)
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T550-f4-kapanis-duzeltmeleri`
- **Commit:** `5a9572af` (sır bekçisi), `001debe2` (Ordering + tarama), `39c269af` (kalan satırlar, tarama biçimleri, 09/K-1614), kayıt commit'i
- **PR:** #(açılınca yazılır)
- **CI:** yönetici izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok (kod içi null sözleşmeleri kod okunarak doğrulandı; dış servis davranışına dayanan kaldırış yok).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Sır bekçisi kabuk genişlemesini (`"$(cat …)"`, `$VAR`) ve `_FILE` yolunu gerçek değer sayar; artık satır ortasındaki önekte de (yanlış pozitif) | D-1120 | İstisnayı genişletmek D-21 ilkesine aykırı; dar istisna ayrı karar ister |

## Notlar

- Kolay ⚠ kararlar (kurala en uygun seçenekle): (1) gerekçe biçimi `// non-null:` ve en az on karakter — K-1614; (2) K numarası K-1614 paralel task'larla çakışabilir, birleştirmede yeniden numaralanabilir; (3) D-1120 yeni kalem, istisna genişletilmedi.
- Working tree başlangıçta temiz; önceki işçinin dalı (`001debe2`) üzerinde devam edildi.
