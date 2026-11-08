# T362 — Marka kimliği: logo, marka adı, site simgesi, marka rengi, platform imzası

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (doğrulama tur 1 ✗ FAIL — renk biçimi denetimi `InvalidColorAttribute`'a bağlanmayı bekliyor) | **Tarih:** 2026-11-08

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- **Sözleşme:** `Modules/CorporateContent/Contracts/BrandingContracts.cs` — `BrandingSection` (formun gösterdiği saklı değerler + sürüm), `BrandingInput` (ad, logo, simge, renk, imza anahtarı: betik, stil, izleme, ikinci renk, tema alanı yok), `BrandingRejection`, `BrandingSaveResult` (kaydedildi, sürüm çakışması, alan reddi), `BrandIdentity` (vitrinin ve e-postanın tek okuması; saklanmaz).
- **Kurallar:** `BrandingRules` (saf) — ad kırpma, renk biçimi (kırpılmış değere `InvalidColorAttribute`, büyük harfe), simge harfi (`RequestFormat.Culture` ile büyük harf; i → İ), iz metni sınırı.
- **Kayıt ve okuma:** `BrandingSettings` — `SaveAsync` kayıt değişkeni (`RecordSave`; sürüm karşılaştırması, işlem zorunlu, `SaveChanges` çağırmaz); ad bir kez girilince boşaltılamaz; renk biçimi; logo/simge yüklenmiş görsel olmalı; dört ayar `UpdateBranding`, imza anahtarı ayrı `UpdateSettings` satırı (hedef `Branding`); değişmeyen kayıt iz ve sürüm yazmaz; `CacheArea.Settings` düşer. `ReadIdentityAsync`: ad yoksa alan adı, renk yoksa ürünün varsayılanı, yazı rengi `EmailColors.Button` (tek kural evi).
- **Modül yüzü:** `ICorporateContentModule` + `CorporateContentModule`: `GetBrandingAsync`, `SaveBrandingAsync`, `GetBrandIdentityAsync`; `AddScoped<BrandingSettings>`.
- **Katalog:** `ICatalogModule.ImageExistsAsync` (başka modül `Image` türüne erişemez) + `CatalogModule` + `ImageUploads.ExistsAsync`.
- **İz kuralı:** `AuditEntryRules.TargetTypesOf(UpdateSettings)` + `Branding`; `09 §9.2` tablosu aynı yönde.
- **Bu turda (devam işçisi):** main birleştirildi (T341); ilk derleme temiz; `SaveAsync` işlem dışı çağrıda yazmadan önce `InvalidOperationException` verir (yazma otomatik işlemle işlem dışında kalıcı oluyordu); testler yazıldı.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/CorporateContent/`: `BrandingRules.cs`, `BrandingSettings.cs`, `Contracts/BrandingContracts.cs`, `Contracts/ICorporateContentModule.cs`, `CorporateContentModule.cs`
- `src/Shopfolio.Web/Modules/Catalog/`: `Contracts/ICatalogModule.cs`, `CatalogModule.cs`, `ImageUploads.cs`
- `src/Shopfolio.Web/Modules/Administration/AuditEntryRules.cs`; `Docs/09_CODING_GUIDELINES.md` (§9.2 tablosu)
- Testler: `tests/Shopfolio.UnitTests/CorporateContent/BrandingRulesTests.cs`, `Administration/AuditEntryRulesTests.cs`, `Infrastructure/SitemapSourceTests.cs` (sahte modüle üç üye); `tests/Shopfolio.IntegrationTests/Model/BrandingTests.cs`
- Kayıt: `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `.claude/memory/MEMORY.md`, `.claude/CONTEXT.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Dört ayar; vitrine ve e-postaya, panele değil; koyu tema yok; CSS/betik/izleme alanı yok | ✓ | Integration `TheBrandTable_HoldsTheFourSettingsAndTheSignatureSwitchOnly_…` (information_schema kapalı liste); Unit `TheBrandForm_HoldsTheFourSettingsAndTheSignatureSwitchOnly_…`, `TheBrandIdentity_HasNoMemberForAThemeOrAPanelColour_…` (kapalı üye listeleri). Vitrin ve e-posta okuması tek `GetBrandIdentityAsync`; sayfa ve e-posta bağlama T534/T537 (plan izlenebilirliği, `11` satır 6773–6781) |
| 2 | Logo zorunlu değil; yoksa ad yazı; tek logo; görsel dosya kuralları | ✓ | Integration `WithoutALogo_TheIdentityShowsTheNameAsText_AndNoLogoId`, `ALogoOrAnIconThatNamesNoUploadedImage_IsRefused_…`, `OnlyTheChangedFields_ReachTheTrail_AndALogoCanBeRemovedAgain`; biçim/boyut kuralı yükleme anında T223 (`ImageFilesTests`, 14 test geçti); tek `LogoId` alanı |
| 3 | Ad unvandan ayrı, zorunlu, boşaltılamaz; girilene kadar alan adı | ✓ | Integration `TheBrandName_CannotBeEmptied_OnceEntered_…`, `WhileTheNameIsEmpty_AnEmptyNameIsAccepted_AndTheDomainIsTheDisplayedName`; Unit `TheBrandName_IsTrimmed_AndBlankIsNone` |
| 4 | Simge yoksa baş harf marka rengi zeminde; Shopfolio simgesi yok | ✓ | Integration `WithoutAnIcon_TheIdentityCarriesTheFirstLetterOfTheName_InCapitals`, `WithoutAName_TheLetterIsTheFirstLetterOfTheDomain_…`; Unit `TheInitial_IsTheFirstLetterInCapitals_…`; `BrandIdentity`'de ürün simgesi yolu yok |
| 5 | Yazı rengi sistemde seçilir, ≥ 4,5:1, saklanmaz; varsayılan renk; anlam renkleri sabit | ✓ | Unit `TheChosenText_ReachesTheMinimumContrast_OnEveryColourOfAChannelGrid` (17³ renk, testin kendi WCAG formülü), `TheTextColour_IsBlackOrWhite_…`, `TheProductsDefaultColour_…`; Integration `TheIdentity_GivesTheBrandColour_AndTheTextColourTheSystemChose`, `WithoutAColour_TheIdentityTakesTheProductsDefault_AndNoTextColourIsStored` |
| 6 | İmza varsayılan açık, yalnız altbilgide; her değişiklik iz | ✓ | Integration `TheSignature_IsOnByDefault_AndEachSwitchIsItsOwnSettingsTrailRow…`, `ASaveThatChangesTheFieldsAndTheSignature_WritesTwoRows_…`; Unit `TheEmail_HasNoMemberForThePlatformSignature_KK6`, `TheSignatureSwitch_IsOnByDefault_…`; e-posta çatısında imza üyesi yok (`EmailCompositionTests` yasak kelime testi: "Shopfolio") |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| k1–k6 | ✓ | Üstteki tablo |
| KK1 dört ayar, panel sabit, kod ekleme alanı yok | ✓ | `BrandingTests.TheBrandTable_…` + `BrandingRulesTests.TheBrandForm_…`, `TheBrandIdentity_HasNoMemberForAThemeOrAPanelColour_…` |
| KK2 logosuz ad yazı; biçim/boyut dışı logo reddedilir | ✓ | `WithoutALogo_…`; boyut/biçim T223 `ImageFilesTests`; bilinmeyen kimlik `ALogoOrAnIconThatNamesNoUploadedImage_…` (`InvalidImage`, hiçbir şey yazılmaz) |
| KK3 ad boşaltılamaz | ✓ | `TheBrandName_CannotBeEmptied_OnceEntered_…` (`FilledFieldCleared`, iz yok) |
| KK4 simge yoksa baş harf; Shopfolio simgesi yok | ✓ | `WithoutAnIcon_…` (`i` → `İ`) |
| KK5 #FFFF00 siyah, #000080 beyaz, #777777 siyah; ≥ 4,5:1; renk yoksa varsayılan | ✓ | `TheIdentity_GivesTheBrandColour_…` (üç renk, üç satır), `TheLimitColour_777777_…` (beyaz < 4,5), ızgara taraması, `WithoutAColour_…` |
| KK6 imza anahtarı, açma/kapama iz, e-postada imza yok | ✓ | `TheSignature_IsOnByDefault_…` (iki satır: `true→false`, `false→true`), `TheEmail_HasNoMemberForThePlatformSignature_KK6` |
| `09 §25` ilgili satırlar | ✓ | Tek işlev: renk/yazı rengi yalnız `EmailColors.Button` (`BrandingSettings.cs:62`; ikinci kontrast hesabı yok); modül metodu işlemde zorunlu ve `SaveChanges` çağırmaz (`ASave_WithoutAnAdministrator_OrOutsideATransaction_IsRefused_…`, `TheSaveAndItsTrailRows_AreOneTransaction_…`); ref/değer: sürüm çakışması `AStaleVersion_IsAConflict_…`; `DbUpdateConcurrencyException` kayıt değişkeni dışında yakalanmaz (`RecordSave` içinde); ayrı kural yok → yeni tarama kuralı yok, yeni bekçi istisnası yok |
| Güvenlik taraması | ✓ | Renk stil niteliğine gider: yalnız `#RRGGBB` kabul (`AValueThatIsNotExactlyHashAndSixHexDigits_IsRefused`: `;`, `url(`, 3 hane, Unicode rakam; `AColourThatIsNotTheStoredShape_IsRefused_…` DB'ye inmez); yeni paket yok; sır yok; yetki: yazma iz için yönetici ister (`AuditTrail`); görsel kimliği başka kayda bağlanamaz (`ImageExistsAsync`, boş `Guid` ret) |
| Sık FAIL 1: bekçi/tarama istisnası geniş | ✓ | Bu task hiçbir bekçi/tarama istisnası eklemedi |
| Sık FAIL 2: tarama çağrı biçimi kaçırıyor | ✓ | Yeni tarama yok; kapalı üye listeleri yansıma ile tüm public özellikleri sayar |
| Sık FAIL 3: DB varsayılanı / FK dizini | ✓ | Şema değişikliği yok (`brandings`: `is_platform_signature_enabled` DEFAULT true, `version` DEFAULT 1 T119/T120'de; tek satırlı tabloda FK dizini yok, `06 §7.5.2`; `ContentRecordAndBrandingTablesTests` geçti) |
| Sık FAIL 4: kural değişince anan her yer | ✓ | `UpdateSettings` hedef listesi değişti: `grep "CompanySettings"` — `AuditEntryRules.cs:116`, `09 §9.2` satır 770; `05/06/07/08/12` bu hedef listesini anmıyor (grep `UpdateSettings` boş); `AuditEntryRulesTests` genişletildi; Consistency 241/241 |
| Sık FAIL 5: SaveChanges / kendi denetimi | ✓ | `BrandingSettings` `SaveChanges` çağırmaz (testte kaydı işlemin sahibi yapar); renk/okunurluk tek kural evi `EmailColors`; biçim denetimi `BrandingRules.TryNormalizeColor` içinde `new InvalidColorAttribute()` ile (`FieldRules.cs:292`, tek kural evi; `CompanyIdentityRules.AddFormRejection` kalıbı; kırpılmış değere uygulanır — `grep -n "InvalidColorAttribute" BrandingRules.cs`; kendi `#RRGGBB` kodu kalmadı: `grep IsAsciiHexDigit` boş; ayrışan örnek `"  #ffff00 "` ve ret örnekleri `AColourTheModuleStores_IsOneTheSingleRuleAccepts_…` teorisinde; E-posta tarafındaki `EmailColors.TryParse` gösterim güvenliği, saklama biçimi değil) — B1 düzeltmesi tur 1 |
| Birleşik ağaçta main'in zorunlu alanları | ✓ | `origin/main` (T341, `e2e9dbcc`) birleştirildi; testler `Image.Create`, `Admin.Create` güncel imzalarla; Unit 955 + 3315 + Consistency, Integration 29 + 157 geçti |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit (Architecture) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --no-build -- --filter-namespace "*Architecture*"` | 955/955 |
| Unit (öteki) | ✓ | `… --filter-not-namespace "*Architecture*"` | 3315/3315 (yeni `BrandingRulesTests` + genişleyen `AuditEntryRulesTests` dahil: 133/133) |
| Unit Consistency | ✓ | `… --filter-trait "Consistency=true"` | 241/241 |
| Integration `BrandingTests` | ✓ | `--filter-class "*BrandingTests"` | 29/29 |
| Integration komşu | ✓ | `--filter-class` `ContentRecordAndBrandingTablesTests`, `AuditTrailTests`, `ImageFilesTests`, `AdminAndAuditTablesTests` | 157/157 |
| Derleme | ✓ | `dotnet build -warnaserror` (Web, Unit, Integration) | 0 uyarı, 0 hata |
| Biçim | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` | temiz |
| Restore | ✓ | `dotnet restore --locked-mode` | temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL — tur 1 (2026-11-08, head `f0ef3ca` + `origin/main` `7097c01` birleştirmesi). Kriter 1–6 ve KK1–KK6 ✓, güvenlik taraması temiz, dal CI 42429549117 yeşil; yerelde derleme `-warnaserror` 0/0, biçim temiz (Integration projesi `whitespace` + `style`, analizörler derlemede), Unit Architecture 955 + Consistency 241 + öteki 3074, betik testleri 4/4, Integration `BrandingTests` 29/29, komşu (`ImageFilesTests`, `AuditTrailTests`, `ContentRecordAndBrandingTablesTests`, `AdminAndAuditTablesTests`, `FirmSettingsTests`, `CompanyIdentityTests`, `DisposalTests`) 219/219 (bunlar `f0ef3ca`'da); birleşik ağaçta derleme 0/0, Unit 955 + 3317, Integration `BrandingTests` + `AuditTrailTests` 51/51. Kontrol listesinin `09 §25` maddesi ✗ (B1) |
| Bulgu sayısı | 1 — B1 DÜŞÜK S1 (`09 §25` "aynı işi yapan tek işlev varsa o çağrılmış mı?"; `09 §3.4` m.1, §24 m.4; T341 tur 2 B1'in kalıbı): `BrandingRules.TryNormalizeColor` `InvalidColor` alan kodunun tek evi `InvalidColorAttribute`'u (`Infrastructure/Validation/FieldRules.cs:292`, `06 4.54`) çağırmadan kendi `#RRGGBB` kuralını kurar ve aynı kodu verir. Ölçülen ayrışma: `"  #ffff00 "` modülde kabul edilir (`BrandingRulesTests.AColour_IsStoredAsHashAndSixCapitalDigits` bunu sınar), nitelikte reddedilir (uzunluk 10 ≠ 7) — T534'ün formu niteliği taşıdığında aynı alan iki kuralla denetlenir. Modülün öteki denetimi zaten bu kalıptadır (`CompanyIdentityRules` niteliğin örneğini çağırır) |
| Düzeltme gerekli mi | Evet — `TryNormalizeColor` biçim denetimini `new InvalidColorAttribute()` ile yapar (`CompanyIdentityRules.AddFormRejection` kalıbı; kırpma tutulacaksa kırpılmış değere uygulanır ve bu bilinçli fark yorumda yazılır, ya da kırpma kaldırılır); büyük harfe çevirme kalır; ayrışan örnek ve niteliğin reddettiği bir örnek aynı teoride iki kuralın birlikte sınandığını gösterir. Rapor ve öz denetim tablosundaki "biçim denetimi `BrandingRules.TryNormalizeColor`" satırı güncellenir |
| Bloke-etmeyen not (K) sayısı | 1 — K1 → D-442 (e-posta göndereni markayı `GetBrandIdentityAsync`'ten kurar; bugün `11`'de bunu söyleyen task yok) |

**Task satırları (`12 §7.5`):** `grep -nE 'KP-56([^0-9]|$)'` → `VAL-A157`…`VAL-A160` (DÜŞÜK; kanıt türü uç yanıtı + tarayıcı gözlemi ya da iş kaydı). Dört satırın beklenen sonucu vitrinde/e-postada görünmeyi ister; sayfa (T534, T537) ve gönderen (D-442) henüz yok → `Durum` `BEKLEMEDE` kaldı. KD-46, KD-47 için matris satırı yok. `12_matris_denetle.py --kapsam 1-77` → Hata: 0.

**Doğrulayıcı uyuşmazlık notları (adım 13):** (1) Notlar'daki ana dal run'ı 42426248097 `failure` değil `cancelled`'dır (eşzamanlılık iptali, Integration işi `7097c01` itilince kesildi; `7097c01` CI 42430286061 yeşil). (2) "Unit (öteki) 3315" Consistency'yi de içerir; iki ad alanı birlikte dışlanınca 3074. (3) Öz denetimin "Sık FAIL 5" satırı biçim denetimini bilinçli olarak `BrandingRules`'a koyar ama `InvalidColorAttribute`'u anmaz — B1 bu boşluktur. (4) Biçim satırı `dotnet format Shopfolio.slnx` der; yerelde Integration projesinin tam biçim koşusu 150 sn'yi aştı, değişen dosya `whitespace` + `style` ile temiz, CI Lint yeşil.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T362-marka-kimligi`
- **Commit:** `task/T362-marka-kimligi` head (dönüş metninde)
- **PR:** #448
- **CI:** yardımcı izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Bu task'ta dış varsayım yok (yeni paket, plan katmanı, kota, dış servis yok) | 4a tetikleyici taraması: `11` T362 metninde tetikleyici terim yok; `git diff` `Directory.Packages.props` dokunmaz | ✓ |

**Başlangıç kapıları (devam işçisi):** Working tree: temiz (`git status --short` boş); `git config core.hooksPath` = `scripts/git-hooks`; ana dal son 3 run (`gh run list --branch main --limit 3`): 42426248059 (Publish) success, 42426248097 (CI) **failure**, 42423723835 (CI) success. Başarısız run `e2e9dbcc` (T341 birleşmesi) üzerinde `UploadEndpointsTests.Chunk_WithNoDeclaredLength_OnARealServer_…` "127.0.0.1:5000 address already in use" (CI koşucusunda port çakışması; T362 dışı, ⚠ 7).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Marka formu (`/panel/marka`), vitrin başlığı/altbilgi imzası/site simgesi etiketi ve e-posta çatısına `GetBrandIdentityAsync` bağlama | — (planda T534, T537 satırlarında) | Sayfa katmanı bu task'ın kapsamı değil; plan izlenebilirliği (`11` 6211, 6276, 6773–6781) onları T534/T537'ye bağlar |

## ⚠ Kararlar (belgeyle karşılaştırıldı)

1. **Varsayılan marka rengi:** `04` ve `02 §3.29.5` değer yazmaz ("ürünün varsayılan rengi"); ürünün tek değeri `EmailColors.DefaultButtonBackground` (`#1A56DB`, `08 §2.2.5`) kullanıldı.
2. **İmza anahtarı ayrı iz satırı:** `UpdateSettings`, hedef `Branding` (`02 §10.3.1` (6); `06` satır 2971 imzayı firma ayarı sayar); `09 §9.2` tablosu güncellendi. Dört ayar `UpdateBranding`.
3. **Marka adı iz değeri 2000 karakterde kesilir** (iz kuralı sınırı `AuditEntryRules.MaxValueLength`; `06 §4.54` ad için tavan koymaz, saklanan değer tam kalır; eşitlik Unit testle bağlı).
4. **Görsel varlığı:** başka modül `Image` türüne erişemediğinden `ICatalogModule.ImageExistsAsync` eklendi (salt okuma).
5. **Simge harfi:** ad yoksa alan adının baş harfi (`02 §3.29.3` alan adı görünür).
6. **Renk harf biçimi:** küçük harfle gelen değer büyüğe çevrilerek saklanır (`06 §4.54` büyük harf); boş = renk yok.
7. **Ana dal CI kırmızısı (T362 dışı):** yukarıdaki port çakışması akımı; yeniden koşuyla geçmesi beklenir.
