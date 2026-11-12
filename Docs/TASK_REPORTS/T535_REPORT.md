# T535 — Ana sayfa ve menü, firma kimliği ve satış (E-42, E-44)

**Faz:** F5 | **Durum:** ⏳ Devam ediyor (doğrulama bekliyor) | **Tarih:** 2026-11-12

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- Panel "Ana sayfa ve menü" (UC-97, UC-162) ve "Firma kimliği ve satış" (UC-99, UC-164, UC-165, UC-166) ekranları ve yazma yolları: düzen, altı menü adı, WhatsApp numarası, sosyal bağlantılar (boş bağlantı satırı siler), kimlik kaydı, satışı kapatma ve yeniden açma.
- Değişen ayar eski ve yeni değerle ize girer; kimlik değişince iki yasal metnin yeni sürümü aynı işlemde, ayar satırı kilitliyken yazılır ve `PublishLegalText` izine girer.
- Firma bildirimi son ulaşmayan e-postaya gittiyse e-posta alanı uyarır; sonraki gönderilen bildirimle uyarı kalkar.
- Testler: `HomepageAndIdentityTrailTests` (Model, 26 test), `HomeAndFirmPageTests` (Web, 9 test; bu turda e-posta uyarısı testi eklendi).
- Bu turda: `origin/main` (T532 #552) birleştirildi; `admin.css` iki tarafın eklemesi korunarak çözüldü.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web` (CorporateContent, Settings, Panel sayfaları, `wwwroot/css/admin.css`), CorporateContent ve Settings modülleri, `tests/Shopfolio.IntegrationTests/Model/HomepageAndIdentityTrailTests.cs`, `tests/Shopfolio.IntegrationTests/Web/HomeAndFirmPageTests.cs`.
- Kayıtlar: `Docs/STATUS_CHANGELOG.d/T535.md`, `Docs/DEFERRED_BACKLOG.d/T535.md` (D-1328…D-1332), `Docs/DEFERRED_BACKLOG.md` (D-501 ✅; D-165, D-220, D-199, D-502 yerinde), `Docs/IMPLEMENTATION_STATUS.md` T535 satırı.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Ana sayfa ekranı düzen, ad, bağlantı, görünmeme sebebi, varsayılanlar | ✓ | `HomeAndFirmPageTests.TheHomeScreen_ShowsLayoutNamesDefaultsAndTheReasonsItemsAreHidden_…_KK1` |
| 2 | SaveHomepage yazar; boş bağlantı siler; iz | ✓ | `TheSave_WritesLayoutNamesNumberAndLinks_…_KK2`; `HomepageAndIdentityTrailTests.SaveHomepage_WritesOneTrailRow_…`, `SaveSocialLinks_AddsChangesAndDeletes_…` |
| 3 | 422 (ad, bağlantı, numara); 409 | ✓ | `TheSave_RefusesAnEmptyName…_KK3`; `SaveHomepage_WithAnEmptyMenuName_…`, `…WhatsApp…`, `…WithAStaleVersion_IsAConflict_…` |
| 4 | Firma ekranı koşullar, bağlantı, e-posta uyarısı | ✓ | `TheFirmScreen_ReadsTheSalesState…_KK4_KK5`; `TheEmailField_Warns…_KK4` (tek başına yeşil) |
| 5 | Anahtar etiketi değere göre; kimlik formundan ayrı | ✓ | `TheFirmScreen_ReadsTheSalesState…_KK4_KK5` |
| 6 | SaveCompany 422 sebepleri; tip değişimi serbest; iz | ✓ | `TheIdentitySave_Refuses…_KK6_KK7`; `SaveCompanyIdentity_WritesOneRow…` |
| 7 | İki yasal metin yeni sürüm, kilit, iz, 409 | ✓ | `TheIdentitySave_…_KK6_KK7`; `SaveCompanyIdentity_ThatCausesGeneratedVersions_…`; `CompanyIdentityTests` (`SettingsRowLock_*`) |
| 8 | SuspendSales onaysız 200, onaylı yazar; iz | ✓ | `Closing_AsksFirst_…_KK8_KK9`; `SalesGateTests` |
| 9 | Değişmiş/zaten istenen hâl 303, yazı yok, 409 yok | ✓ | `Closing_AsksFirst_…_KK8_KK9` |
| 10 | ResumeSales koşulsuz açmaz, koşulu söyler, iz | ✓ | `Reopening_ClosesTheSwitchButLeavesSalesClosed…_KK10` |

### Öz denetim (bitiş kapısı 2)

Önceki işçilerin yaptıkları dahil bütün kriterler ve KK1–KK10 diff'te yeniden denetlendi (yukarıdaki tablo her madde için testi adlandırır; KK sayısı kriter sayısıyla birebir).

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kriter 1–10 / KK1–KK10 | ✓ | Yukarıdaki tablo; Web 9 + Model 26 test |
| `09 §25` ilgili satırlar, mimari kurallar | ✓ | Unit TAM 7402/7402 (mimari testler dahil) |
| Güvenlik taraması | ✓ | Unit TAM içindeki kaynak taramaları; yeni yazma yolları modül arayüzünden, işlemi sahibi açıyor |
| FAIL nedeni 1: bekçi/tarama istisnası geniş | ✓ | Yeni istisna eklenmedi; `VersionedSideBulkWriteScan` için yerel değişken geçici çözümü (D-1329), istisna genişletilmedi |
| FAIL nedeni 2: tarama bir çağrı biçimini kaçırıyor | ✓ | Yeni tarama yazılmadı |
| FAIL nedeni 3: 06 "Varsayılan" / FK dizini | ✓ | Şema/migration değişikliği yok |
| FAIL nedeni 4: kural değişince aynı sayıyı anan yerler | ✓ | Kural/sayı değişmedi; kayıt parçaları ve D kalemleri tutarlı |
| FAIL nedeni 5: belge kuralını değiştiren 09 metni | ✓ | 09/05/07 metnine dokunulmadı |
| `dotnet format` test dosyalarında | ✓ | Web ve IntegrationTests (Model, Administration, HomeAndFirmPageTests) temiz |
| Main birleşince yeni zorunlu alan / kurucu bağımlılığı | ✓ | Birleşik ağaçta derleme yeşil |

12'deki Durum hücreleri BEKLEMEDE kalır (doğrulayıcı PASS yapar); ilgili satırlar VAL-A449, VAL-A541, VAL-A747.

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` | 7402/7402 (main birleşik) |
| Integration (yeni) | ✓ | `--filter-class HomepageAndIdentityTrailTests` ; `--filter-method *TheEmailField_Warns*` | 26/26 ; 1/1 |
| Integration TAM | ✓ | `--filter-namespace` parçalarıyla | Hepsi yeşil: Administration 69, Catalog 82, Commands 70, Concurrency 30, CorporateContent 195, Database 36, Disposal 61, FileSecrets 24, Files 112, Jobs 85, Logging 30, Ordering 645, Outages 53, Payment 59, Security 34, SignedEndpoints 11, Web 914, Model 1436 (Pages ad alanında test yok, yalnız örnek sınıflar) |
| Contract | — | — | Bu task'ta yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | Bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T535-ana-sayfa-firma`
- **PR:** STATUS satırında
- **CI:** merge öncesi doğrulayıcı/yardımcı izler

## Dış Varsayımlar

Dış varsayım: yok.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `UpdateSettings` izi sosyal bağlantıyı `CompanySettings` hedefli tek satırda taşıyor | D-1328 | Ayrı hedef satırı ayrı iz modeli gerektirir |
| 2 | `VersionedSideBulkWriteScan` `names.Products` yanlış pozitifi (yerel değişken geçici çözümü) | D-1329 | Tarama inceltmesi ayrı iş |
| 3 | Tip değişimi alan işaretleri betiksiz yolda sunucuda | D-1330 | Betikli arayüz sonrası |
| 4 | ResumeSales sonrası ileti yok, koşul bölümde | D-1331 | Kriter 10 koşulu adıyla söylüyor, karşılandı |
| 5 | WhatsApp kuralı Settings ve CorporateContent'te ayrı satırlar | D-1332 | Tek kural evine toplama ayrı iş |

## Notlar

- **⚠ (f):** kimlik kaydı sürümü değişmeyen kayıtta da artıyor (RecordSave her zaman BumpVersion; T330 davranışı, değişmedi).
- **Yük:** Makine yüklü; `HomeAndFirmPageTests` sınıfı tek çağrıda 330 sn zaman aşımına girdi, testler tek başına koşuldu.
- **Integration TAM:** bütün ad alanları yeşil; Database, Files ve Ordering ilk koşuda kendi `timeout` kesintisiyle (yük) hata verdi, 590 sn ile tek başına yeniden koşuldu ve yeşil.
- **Repo memory (bitiş kapısı 9):** güncellendi (parça dosyaları).
