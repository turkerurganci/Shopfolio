# T226 — Ayar katmanları, tasarım değerleri ve açılışta ayar denetimi

**Faz:** F2 | **Durum:** ⏳ Devam ediyor (yapım bitti, doğrulama bekliyor) | **Tarih:** 2026-11-04

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- **Ayar sınıfları** (`src/Shopfolio.Web/Infrastructure/InstallationSettings/`): zorunlu `SiteOptions` (`Site:Domain`), `EmailOptions` (`Email:SenderAddress`, `Email:Region`, `Email:Credential`), `LinkKeyOptions` (`LinkKey:DerivationSecret`, Base64, en az 32 bayt), veritabanı bağlantısı (`DatabaseOptions`, T08); koşullu `PaymentOptions` (`Payment:ProviderKeys`) ve `GoogleOptions` (`Google:ClientId` + `Google:ClientSecret`). Her sınıfın doğrulayıcısı eksik ayarı adıyla, biçimsiz ayarı adı ve beklenen biçimle bildirir; değeri hiç yazmaz (`SettingFailures`).
- **Tasarım değerleri** (`TunableDesignValues.cs`): `RateLimit:PageRequestsPerMinute` 300, `RateLimit:FormSubmissionsPerMinute` 30, 11 sayısal durum eşiği (`StatusThreshold:*`, `05 §9.4.2`; disk eşiği `DiskUsageFraction` 0,85) ve `Logging:Level` (varsayılan Information). Varsayılanlar kodda; ayar dosyasında değişir; sıfır, negatif, `None` ve bilinmeyen düzey açılışı reddeder.
- **Ürün sabiti adı koruması** (`ProductConstantNameGuard`): bütün yapılandırma anahtarlarının her bölümü `ProductConstants` sabitlerinin adlarıyla karşılaştırılır (harf ve rakam, büyük-küçük harf ve `_` `-` yok sayılır); eşleşen anahtar açılışı reddeder. Sabitler sınıfın kendisinden okunur, sonradan eklenen sabit kendiliğinden kapsanır.
- **Açılış denetimi** (`SettingsAudit` saf fonksiyon + `StartupSettingsCheck` ilk hosted service): başarısızlıkta Critical satır (`LogEvents.StartupSettingsChecked`, ayar adıyla) ve `InvalidOperationException`; koşullu hat kapalıysa Warning satırı eksik anahtarlarla, açıksa Information satırı. Kayıt `ILogger<T>` iledir (dosyaya inişi T230).
- **Sır dosyaları** (`FileSecrets/SecretNames.Settings`, `SecretFileConfigurationSource`): beş sır ayarı (`Database:ConnectionString`, `Email:Credential`, `LinkKey:DerivationSecret`, `Payment:ProviderKeys`, `Google:ClientSecret`) yalnız kendi sır dosyasından gelir; boş ya da yalnız boşluktan oluşan dosya "yok" sayılır.
- **Dağıtım yüzeyi:** `deploy/compose.yaml` (`app` servisi `Site__Domain=${SHOPFOLIO_DOMAIN}` — D-32 (a) tek kaynak), `deploy/.env.example` (tasarım değerleri, `Email__SenderAddress`, `Email__Region`, `Google__ClientId=`), `deploy/compose.dev.yaml` (sır olmayan uydurma zorunlu ayarlar; iki sır ayarı sır dosyasından), `scripts/ci/package-build.sh` (CI paket adımı için uydurma zorunlu ayarlar), `scripts/local-reference/host-secrets.sh` (Email ayarları), `Docs/DEPLOY_RUNBOOK.md` §A (altı zorunlu ayar) ve §C (18 satır: koşullu hatlar ve tasarım değerleri, anahtar + varsayılan).
- **Testler:** `SettingsAuditTests` (birim), `SettingLayerScan` + ihlal örnekleri, `SettingLayersConsistencyTests`, `StartupSettingsCheckTests` ve `SecretFilesTests` eklemeleri (entegrasyon); fabrika (`ShopfolioWebApplicationFactory`) uydurma zorunlu ayarları verir.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/InstallationSettings/*` (yeni), `Infrastructure/FileSecrets/SecretNames.cs`, `SecretFileConfigurationSource.cs`, `Infrastructure/Logging/LogEvents.cs`, `Program.cs` (`AddInstallationSettings()`; `DatabaseOptions` için `ValidateOnStart` kalktı — tek denetim yolu).
- `deploy/compose.yaml`, `deploy/compose.dev.yaml`, `deploy/.env.example`, `scripts/ci/package-build.sh`, `scripts/local-reference/host-secrets.sh`, `Docs/DEPLOY_RUNBOOK.md`, `.claude/CONTEXT.md`.
- `tests/Shopfolio.UnitTests/InstallationSettings/SettingsAuditTests.cs`, `Architecture/SettingLayerScan*.cs`, `Architecture/ViolationSamples/SettingLayers/*`, `Consistency/SettingLayersConsistencyTests.cs`; `tests/Shopfolio.IntegrationTests/Web/StartupSettingsCheckTests.cs`, `Web/ShopfolioWebApplicationFactory.cs`, `FileSecrets/SecretFilesTests.cs`.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Her değerin tek evi; üç katman karışmaz | ✓ | `SettingLayerScanTests.SettingsClasses_CarryNoMemberNamedLikeAProductConstantOrAFirmSetting`; `SettingLayersConsistencyTests` |
| 2 | Tasarım değerlerinden yalnız hız sınırı, durum eşikleri, kayıt düzeyi ayar dosyasında; ayar dosyasında ürün sabiti adı → açılış reddi | ✓ | `SettingLayersConsistencyTests.TunableDesignValues_AreOnlyTheRateLimitTheStatusThresholdsAndTheLogLevel`; `StartupSettingsCheckTests.Start_WithASettingThatNamesAProductConstant_...` |
| 3 | Sır olmayan ayar tek dosyada, sırlar ayrı yerde | ✓ | `SecretFilesTests.ConfigurationSource_WithASecretFile_...`; `SettingLayersConsistencyTests.SecretSettings_AreInNoSettingsTemplate...` |
| 4 | Açılışta bütün ayarlar doğrulanır; zorunlu eksik/biçimsiz → açılmaz ve adıyla kaydeder; koşullu eksik → açılır, hat kapalı, açılış kaydı yazar | ✓ | `StartupSettingsCheckTests.Start_WithARequiredSettingMissingOrMalformed_...`, `Start_WithOnlyTheRequiredSettings_OpensAndTheLogSaysWhichLinesAreClosed` |
| 5 | Zorunlu ayarlar: alan adı, gönderen adresi, e-posta gönderim kimliği, veritabanı bağlantısı, türetme sırrı | ✓ | `SettingsAuditTests.Run_WithARequiredSettingMissing_...` (6 anahtar); `DEPLOY_RUNBOOK` §A |
| 6 | Yanıt süresi ve eşzamanlı ziyaretçi tasarım değerleri sistemde uygulanmaz | ✓ | Ayar sınıfı yok; `SettingLayersConsistencyTests.TunableDesignValues_...` (anahtar kümesi dört bölümle sınırlı, Response/Concurrent/Visitor/Volume adı yok) |

### Öz denetim (bitiş kapısı 2)

Bütün maddeler bu devam turunda yeniden, diff üzerinde denetlendi.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| Kriter 1 — katmanlar karışmaz | ✓ | `SettingLayerScan.ScanOptionsSource`; ihlal örneği `ViolationSamples/SettingLayers/MixedOptions.cs` (kırmızı, satır 6), temiz örnek `CleanOptions.cs` |
| Kriter 2 — yalnız üç tasarım değeri ayarda; ürün sabiti adı reddi | ✓ | `ProductConstantNameGuard`; `SettingsAuditTests.Run_WithAProductConstantNameInAnyWriting_Refuses`, `...ProductConstantNames` (her sabit adı); `StartupSettingsCheckTests` (gerçek uygulama açılmaz) |
| Kriter 3 — sırlar ayrı yerde | ✓ | `SecretNames.Settings` (5 sır); `SecretFilesTests`; `.env.example` ve compose dosyalarında sır ayarı adı yok (`SettingLayersConsistencyTests.SecretSettings_...`) |
| Kriter 4 — açılış denetimi ve koşullu hat | ✓ | `StartupSettingsCheck`; `SettingsAuditTests` (koşullu hat açık/kapalı, yalnız `Google:ClientId` → kapalı + `Google:ClientSecret` eksik); `StartupSettingsCheckTests` (log satırları) |
| Kriter 5 — beş zorunlu ayar | ✓ | `SettingsAuditTests.RequiredKeys`; `SettingLayersConsistencyTests.RunbookSectionA_ListsTheSixRequiredSettings` (veritabanı bağlantısı dahil altı anahtar) |
| Kriter 6 — süre/ziyaretçi/hacim kabulü uygulanmaz | ✓ | Bu değerler için kod yok; anahtar kümesi testi (yukarıda); aşım çalışmayı durduracak mekanizma eklenmedi |
| KK1 — aynı değer iki yerde değil | ✓ | `SettingLayersConsistencyTests.RunbookSectionC_...` (runbook = kod varsayılanı), `SettingsTemplate_TunableLines_...` (şablon = kod varsayılanı), `Compose_GivesTheApplicationItsDomain...` (alan adı bir kez: `SHOPFOLIO_DOMAIN`) |
| KK2 — örnek değer beklenen yerde; hız sınırı ayardan değişir; ürün sabiti adlı ayarla açılmaz | ✓ | `StartupSettingsCheckTests.RateLimit_FromTheSettings_IsTheValueTheSettingsGiveAndTheCodeDefaultWithoutOne` (120 / 300); ürün sabiti adı testleri yukarıda |
| KK3 — sır ayar dosyasında yok | ✓ | `SettingLayerScanTests.SettingsTemplate_NamesNoProductConstantAndListsNoSecretSetting` (yeni kural, ihlal örneği `SettingsSecretSetting.env` satır 6); `SecretSettings_AreInNoSettingsTemplateAndNoComposeEnvironment...`; `DeploySecretScanTests` (mevcut) istisna listesi genişlemedi (tek istisna `Database__ConnectionString`) |
| KK4 — zorunlu eksik/biçimsiz açılmaz; koşullu eksik açılır, hat kapalı, kayıt yazar | ✓ | `StartupSettingsCheckTests` (9 zorunlu eksik/biçimsiz durum; 3 koşullu durum) |
| KK5 — ileti ayar adını taşır, değerini taşımaz | ✓ | `SettingsAuditTests.Run_With...NeverPrintsTheValue` (biçimsiz zorunlu, aralık dışı tasarım değeri, tip hatası); `StartupSettingsCheckTests` (özel değer işareti hiçbir kayıt satırında ve istisnada yok) |
| KK6 — süre/ziyaretçi/hacim aşımı çalışmayı durdurmaz | ✓ | Bu değerleri uygulayan kod yok (Kriter 6) |
| `09 §25` — "Yeni ayar ... §3.5 tablosunda mı; `.env.example` aynı PR'da güncel mi?" | ✓ | Hız sınırı, disk eşiği, canlılık süresi `09 §3.5` tablosunda; yeni ayarlar `05 §8.4.6` ve `DEPLOY_RUNBOOK` §A/§C'de; `.env.example` güncel (`SettingLayersConsistencyTests`) |
| `09 §25` — "Yeni yasak ya da mimari kural için kaynak taraması + kasıtlı ihlalli örnek" | ✓ | `SettingLayerScan` iki kural; ihlal örnekleri `MixedOptions.cs`, `SettingsProductConstant.env`, `SettingsSecretSetting.env` (depoda, salt okunur; birim testleri dosya sistemine yazmaz) |
| Aynı adı anan öteki yerler (sık FAIL) | ✓ | runbook §A/§C/§B satır 13, `.env.example`, `compose.yaml`, `compose.dev.yaml`, `package-build.sh`, `host-secrets.sh`, `.claude/CONTEXT.md`, D-32 satırı hepsi güncellendi; `Email__Credential` / `LinkKey__DerivationSecret` yalnız CI'ın `docker run -e` satırında (aşağıda ⚠) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Build | ✓ | `dotnet build Shopfolio.slnx -warnaserror` | 0 uyarı, 0 hata |
| Format | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes` | değişiklik yok |
| Unit (Consistency dahil) | ✓ | `dotnet test tests/Shopfolio.UnitTests` | 520/520 |
| Integration | ✓ | `dotnet test tests/Shopfolio.IntegrationTests` (PostgreSQL 18, konteyner `shopfolio-t226-pg`) | 978/980 ilk koşuda; iki başarısız (`GalleryLinkTablesTests`, `SchedulerBackupDisposalAndKeyTablesTests` — zaman aşımı, yük altında) tek başlarına 102/102 geçti; T226 sınıfları (`StartupSettingsCheckTests`, `SecretFilesTests`) 41/41 |
| Betik testleri | ✓ | `bash tests/scripts/run-script-tests.sh` | 4 dosya geçti |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | Bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Var — `Site__Domain` (Compose `app`), `Email__SenderAddress`, `Email__Region`, `Google__ClientId`, tasarım değerleri (`.env.example`); sır dosyaları `email_credential` ve `link_derivation_secret` artık uygulamanın açılması için dolu olmalıdır
- **Paketleme / dağıtım:** Var — `compose.yaml`, `compose.dev.yaml`, `package-build.sh`, `host-secrets.sh`
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T226-ayar-katmanlari`
- **Commit:** PR'da
- **PR:** PR'da
- **CI:** yapım işçisi izlemez
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım: yok. Ayar adları ve biçimleri `05 §8.4.6`, `09 §3.5` ve `08 §2.2`'den; e-posta kimliğinin içerik biçimi doğrulanmaz | — | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `command` / `read-command` konteynerleri açılışta tam ayar kümesini ister; grup bazlı bölme | D-78 | Komut giriş noktası T389'da |
| 2 | `Email:Credential` içerik biçimi doğrulanmıyor (yalnız boş değil) | D-79 | Biçim `08 §2.2`'de yok; e-posta task'ında |
| 3 | `Logging:Directory` ayarı | — | T230 (planda) |
| 4 | D-32 (a) çözüldü | D-32 | `Site__Domain=${SHOPFOLIO_DOMAIN}` |

## Notlar

- **Working tree (Adım -1):** temiz.
- **Ana dal CI startup (Adım 0):** önceki işçide; bu devam turunda değişmedi.
- **Repo memory (bitiş kapısı 9):** güncellendi (`.claude/memory/MEMORY.md`).
- **Kolay ⚠ kararlar:** (a) e-posta gönderim kimliği = `Email:Region` (ayar dosyası) + `Email:Credential` (sır dosyası, boş değil); içerik biçimi doğrulanmaz. (b) Google hattı için `ClientId` ve `ClientSecret` ikisi de dolu olmalı. (c) Durum eşiği olarak `05 §9.4.2`'nin 11 sayısal eşiği tunable; 9.4.7 sayısal değil. (d) Boş sır dosyası "yok" sayılır (veritabanı dahil). (e) Açılış denetimi ilk hosted service; `DatabaseOptions.ValidateOnStart` kaldırıldı. (f) Ürün sabiti adı koruması bütün yapılandırma anahtarlarını tarar. (g) Sır kararı: sır değeri compose `environment`'ına ve ayar dosyasına girmez; dev'de `email_credential` ve `link_derivation_secret` dosyaları geliştirme dizininde uydurma içerikle durur (önceki "boş dosya" yazımı kalktı, çünkü e-posta kimliği zorunludur); `DeploySecretScan.AllowedEnvironmentSettings` genişletilmedi.
- **⚠ Bilinçli kalan istisna:** `scripts/ci/package-build.sh` CI paket adımında `docker run -e` ile uydurma `Email__Credential` ve `LinkKey__DerivationSecret` verir (geçici CI konteyneri, hiçbir kurulumun değeri değil, compose/ayar dosyası değil); sır taraması bu betiği kapsamaz. Sır dosyası bağlamak istenirse betik geçici dizin yazmak zorunda kalır; kolay karar olarak bırakıldı.
- **Yarım kalan tutarsızlık notu:** `DEPLOY_RUNBOOK` §B satır 13 geliştirme makinesi cümlesi "boş" yerine "uydurma içerikli" yazıyor; `05 §8.1.2` bu ayrıntıyı taşımaz, değişiklik gerekmedi.
