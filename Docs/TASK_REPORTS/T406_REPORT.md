# T406 — PayTR: 3D Secure kurulum şartı, test kanıtı ve canlı kanıtın satırları

**Faz:** F4 | **Durum:** ⏳ Devam ediyor (yapım bitti, doğrulama bekliyor) | **Tarih:** 2026-11-11

- **Dal:** `task/T406-paytr-3d-secure` (taban `main` `cda5032e`)
- **Bağımlılık:** T402 ✓, T405 ✓
- **Yapım:** iki işçi (ilki testleri ve anahtar taramasını yazdı, bağlam sınırında bölündü — ara commit `37a9b731`; ikincisi kaynak taramasını ve çok satırlı değer biçimini ekledi, belgeleri, backlog'u, kayıtları ve raporu yazdı, Unit ve Integration'ı tam koştu)

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- **Uygulama kodu değişmedi.** Kriter 1, 2 ve 5'in davranışı T402/T403'ün bildirim ve sorgu kuralında zaten vardır; T406 onu 3D Secure açısından testlerle kanıtlar ve kurulum şartını belgeye bağlar.
- **Integration `PaymentNotificationTests.PayTrThreeDSecure` (12 test):** `failed` + kod `1`, `2`, `10` → sipariş İptal + Başarısız, sebep `CardPaymentFailed`, `PaymentRecord` yok; 3D'ye benzeyen alan (`non_3d`, `3d_secure`, `threeds_status`) okunmaz, başarı 7. sıra gibi uygulanır; `payment_type` `eft`/`CARD`/`Card`/boş/yok → işaretli kayıt, sipariş Bekliyor; canlıda `test_mode` = `1` `failed` → işaretli, sipariş iptal edilmez.
- **Unit `PayTrPaymentGatewayTests.ThreeDSecure`:** başlatma formunda 3D alanı yok; `PayTr*` sabitlerinde 3D alan adı yok (yansıma); **uygulamanın bütün `.cs` kaynağında 3D alanı adlandıran dizge yok** (indeksleyici, verbatim, ham ve aradeğerli dizge biçimleri; ihlal ve temiz örnekleri satır içi); 3D'ye benzeyen alan imzayı ve notice'i değiştirmez; kod `1`/`2`/`10` `PaymentFailed`; yalnız `card` (ordinal) kart.
- **Unit `PaymentKeysScan` + `PaymentKeysScanTests`:** `appsettings*.json`, `deploy/**`, `.github/**`, `scripts/ci`, `scripts/local-reference` anahtara değer vermez — tarama bütün metin üzerinde, değerin sonraki satırda yazıldığı JSON ve YAML biçimleri de yakalanır; CI dosyaları PayTR'yi ya da anahtar adını hiç anmaz; yerel referansın uydurma `payment_provider_keys` içeriği `PaymentProviderKeys.TryParse` ile null (kart hattı kapalı). İhlal ve temiz örnekleri satır içi.
- **Belgeler:** `DEPLOY_RUNBOOK` yeni §N (N.1 kurulum şartı kayıt tablosu, N.2 test modu kanıtı, N.3 `VAL-F001`…`VAL-F012` canlı kanıt satırları), §0 özet satırı, §B satır 12'ye "§N.1 ✓ olmadan yazılmaz"; `PAYTR.md` başlık, kapsam notu, §6 komut listesi, §6.1 DV-3 satırı, yeni §6.2 sandbox tablosu, §9 satırı; `08 §4.1` DV-3 son hücresi.
- **Backlog:** D-1089, D-1090 (`DEFERRED_BACKLOG.d/T406.md`); hedefi "en geç T406" olan D-1034, D-1038, D-1075, D-1076, D-1082 yerinde D-1090'ın sandbox koşusuna ileri atıldı.

## Etkilenen Modüller / Dosyalar

- Testler: `tests/Shopfolio.IntegrationTests/Payment/PaymentNotificationTests.PayTrThreeDSecure.cs` (yeni), `tests/Shopfolio.UnitTests/Integrations/PayTrPaymentGatewayTests.ThreeDSecure.cs` (yeni), `tests/Shopfolio.UnitTests/Architecture/PaymentKeysScan.cs` (yeni), `PaymentKeysScanTests.cs` (yeni)
- Belgeler: `Docs/DEPLOY_RUNBOOK.md` (§0, §B, §N), `Docs/INTEGRATION_RUNBOOKS/PAYTR.md`, `Docs/08_INTEGRATION_SPEC.md` §4.1 DV-3, `Docs/DEFERRED_BACKLOG.d/T406.md` (yeni), `Docs/DEFERRED_BACKLOG.d/T401.md`, `T402.md`, `T404.md`, `T405.md` (hedef hücreleri), `Docs/STATUS_CHANGELOG.d/T406.md` (yeni), `Docs/IMPLEMENTATION_STATUS.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Her kart ödemesi 3D'den geçer; bildirimde 3D işareti yok, güvence kurulum şartı ve canlı kanıt; doğrulama başarısızsa ödeme yok, sipariş ödenmiş değil | ✓ (kod ve belge payı) | Integration `PayTr_3DVerificationThatFailed_CancelsTheOrder_TheCardFailureReason_AndNoPayment_T406_KK1` (1/2/10), `PayTr_AFieldThatLooksLikeA3DMark_IsNotRead_*`; Unit `TheApplicationsSource_HasNoStringNamingA3DField_T406_KK1`, `TheLayersFieldNames_NameNo3DField_T406_KK1`, `Start_SendsNo3DField_*`; `DEPLOY_RUNBOOK §N` girişi, N.1, N.3 `VAL-F003` |
| 2 | Başarı `success` + `card`; kod 1, 2, 10 ödeme gerçekleşmedi | ✓ | Unit `Notification_OnlyThePaymentTypeCard_IsACardPayment_AsItCame_T406_KK2`, `Notification_AFailed3DVerification_ReadsAsAFailedPayment_*`; Integration `PayTr_SuccessWhosePaymentTypeIsNotCard_IsMarkedUnrecognized_*` |
| 3 | Kurulum şartı: yazılı teyit (3D + Non3D kapalı) ve test modunda 3D'siz işlem tamamlanmaz; teyit yoksa anahtar yok | ✓ (şartın kaydı ve kod payı) · teyidin kendisi OWNER_INPUT (D-1089) | `DEPLOY_RUNBOOK §N.1` (üç satır, ✓ olmadan `payment_provider_keys` yazılmaz), §B satır 12; `PAYTR.md` §6.1 DV-3, §6.2 satır 3, §9; Unit `LocalReferenceInstallation_SecretFileDoesNotDefineTheKeys_TheCardLineStaysClosed_T406_KK3` |
| 4 | `test_mode` ile test; canlı anahtarla imza; CI PayTR'ye bağlanmaz, firma anahtarı geliştirme ve CI'a geçmez | ✓ | Unit `TrackedSettingsDeployAndScriptFile_GivesThePaymentKeysNoValue_T406_KK4` (her dosya ayrı), `CiFile_NamesNeitherTheProviderNorItsKeys_T406_KK4`, `TheScannedSets_*`, ihlal/temiz örnekleri; bağlantı katmanı testleri sahte işleyiciyle (`CapturingHandler`) |
| 5 | Canlıda `test_mode` 1 bildirimi işaretli, uygulanmaz; sorgu sonucu da uygulanmaz | ✓ | Integration `PayTr_TestModeFailureOnTheLiveInstallation_IsMarkedUnrecognized_AndDoesNotCancelTheOrder_T406_KK5`; T402 `PayTr_Row4_TestModeOnTheLiveInstallation_*`; sorgu: T403 `PaymentWindowTests.PayTr` (`FinalPaymentQueryRule.TestModeReason` satırı), Unit `FinalPaymentQueryRuleTests` |
| 6 | Sandbox kanıtı: başlatma, 3D, bildirim, sorgu, iade test modunda uçtan uca; sonuç runbook'ta | ✓ (tablo ve yeri) · koşunun kendisi OWNER_INPUT (D-1090) | `PAYTR.md` §6.2 (altı satır, "Bekliyor — OWNER_INPUT", tarihli); `DEPLOY_RUNBOOK §N.2` |
| 7 | Canlı kanıtın satırları `DEPLOY_RUNBOOK`'ta; taksit varsa taksitli; yürütme F6 | ✓ | `DEPLOY_RUNBOOK §N.3` (`VAL-F001`…`VAL-F012`, sipariş no/tutar/taksit/an/kanıt türü/sonuç sütunları, "Bekliyor — F6, T601", kart verisi ve sır yazılmaz, taksit kuralı) |
| 8 | DV-3 kanıtla kapanır; test kanıtı canlı kanıtın yerini tutmaz | ✓ (tarihli bekliyor kaydı) · kapanış OWNER_INPUT (D-1089) | `08 §4.1` DV-3 son hücresi (2026-11-11, kayıt yerleri, D-1089); `DEPLOY_RUNBOOK §N.2`, §N.3; `PAYTR.md` §6.2 girişi |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1: `failed` + 1/2/10 ödeme sayılmaz, sipariş ödenmiş olmaz; kodda 3D alanı aranmaz | ✓ | Integration `PayTr_3DVerificationThatFailed_*` (3 kol); Unit kaynak taraması `TheApplicationsSource_HasNoStringNamingA3DField_T406_KK1` + `SourceScan_OnViolationSample_FindsTheString` (6 biçim) + `SourceScan_OnCleanSample_FindsNothing` (4 örnek); yansıma `TheLayersFieldNames_NameNo3DField_T406_KK1` |
| KK2: `payment_type` farklıysa uygulanmaz | ✓ | Integration `PayTr_SuccessWhosePaymentTypeIsNotCard_*` (5 kol); Unit `Notification_OnlyThePaymentTypeCard_*` (7 kol, baştaki boşluk dahil) |
| KK3: teyitsiz kurulumda anahtar yok | ✓ | `DEPLOY_RUNBOOK §N.1`, §B satır 12; Unit `LocalReferenceInstallation_SecretFileDoesNotDefineTheKeys_*`; anahtarsız kart hattı kapalı (`§C` satır 1, T401) |
| KK4: testte ağ çağrısı yok; geliştirme ve CI ayarında firma anahtarı yok | ✓ | `PaymentKeysScanTests` (dosya başına teori, 11 ihlal + 7 temiz değer örneği, 5 ihlal + 1 temiz CI örneği; çok satırlı JSON/YAML) |
| KK5: canlıda test bildirimi işaretli, uygulanmaz; `test_mode` 1 sorgu sonucu uygulanmaz | ✓ | `PayTr_TestModeFailureOnTheLiveInstallation_*`; `PaymentWindowTests.PayTr` `TestModeReason` satırı (T403) |
| KK6: sandbox tablosu doludur | ✓ (satırlar tarihli "Bekliyor — OWNER_INPUT"; koşu D-1090) | `PAYTR.md` §6.2 |
| KK7: runbook canlı kanıt satırları | ✓ | `DEPLOY_RUNBOOK §N.3` |
| KK8: DV-3 tarihli | ✓ | `08 §4.1` DV-3 son hücresi (2026-11-11); `PAYTR.md` §6.1 DV-3; `DEPLOY_RUNBOOK §N.1` |
| `09 §25` — doküman uyumu, kapsam (yalnız test ve belge; ek ayar yok) | ✓ | Uygulama kodu değişmedi; yeni ayar/sabit yok (`09 §3.5` dokunulmadı) |
| `09 §25` — tarama kuralının ayrı ihlal örneği; birim testi dosya sistemine yazmıyor | ✓ | İki taramanın ihlal/temiz örnekleri satır içi; testler yalnız okur |
| `09 §25` — dış servis çağrısı arayüzde; imza doğrulaması işleyiciden önce | ✓ | Değişmedi (T401/T402); testler sahte işleyiciyle |
| Kod biçimi | ✓ | `dotnet format --verify-no-changes` Unit ve Integration projelerinde değişen dört dosyayla çıkış 0; `-warnaserror` derleme temiz |
| Güvenlik taraması: sır sızıntısı, auth etkisi, girdi doğrulaması, yeni bağımlılık | ✓ | Belgelerde ve testlerde gerçek anahtar yok (örnekler uydurma); auth ve girdi kodu değişmedi; yeni paket yok |
| Sık FAIL nedenleri (geniş istisna, kaçan çağrı biçimi, sayıyı anan öteki yerler, karar kaydısız 09 metni, devredilen backlog) | ✓ | Tarama istisnası yok, temiz örnekler dar; kaynak taraması bütün dizge biçimlerinde; `§0` özet ve `§B` satır 12 güncel; 09 değişmedi; "en geç T406" hedefli beş kalem yerinde ileri atıldı |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit (TAM) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj --max-threads 3` | 6598 / 6598 geçti |
| Unit (T406 + PayTR) | ✓ | `--filter-class "*PaymentKeysScanTests" --filter-class "*PayTrPaymentGatewayTests"` | 270 / 270 |
| Integration (TAM, beş parça) | ✓ | `--filter-namespace` Payment (59), Ordering (637), Administration…Database (472), Disposal…Pages (355), kalanı `--filter-not-namespace` ile (2052) | 3575 / 3575 geçti (Postgres 18, `track_commit_timestamp=on`) |
| Contract | — | — | Yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | *(doğrulayıcı doldurur)* |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok (yalnız `DEPLOY_RUNBOOK §N` kurulum adımı)
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T406-paytr-3d-secure`
- **Commit:** ara `37a9b731`; `49d17155` — T406: PayTR: 3D Secure kurulum şartı, test kanıtı ve canlı kanıtın satırları
- **PR:** #530
- **CI:** *(doğrulayıcı)*
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | DV-3: PayTR her kart ödemesini 3D'den geçirir, Non3D yetkisi kapalı | PayTR yazılı teyidi yok, test mağazası yok | Bekliyor — OWNER_INPUT (D-1089); teyit gelmeden kart anahtarları tanımlanmaz |
| 2 | Test modunda başlatma/3D/bildirim/sorgu/iade uçtan uca | Test mağazası yok | Bekliyor — OWNER_INPUT (D-1090) |
| 3 | Bildirimde 3D işareti yoktur (`08 §2.1.3`, PayTR belgesi 2026-10-29) | Kod 3D alanı okumaz; kaynak taraması | Belgeye uygun |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | DV-3 yazılı teyit ve test modunda 3D'siz deneme | D-1089 | PayTR sözleşmesi ve test mağazası yok (OWNER_INPUT) |
| 2 | Sandbox uçtan uca koşusu | D-1090 | Test mağazası yok (OWNER_INPUT) |
| 3 | `12` `VAL-E003` BEKLEMEDE kalır | D-1089 | Kanıt ortamı `sandbox`; koşu yapılmadı |

## Notlar

- **⚠ Kararlar (kolay, kurala en uygun seçenekle):** (1) Yeni "3D teyidi" ayarı eklenmedi — `08 §2.1.2`'nin "üç değer tek ayar" kuralını değiştirirdi; kapı kurulum adımıdır, kod payı anahtarsız kart hattının kapalı oluşudur (`DEPLOY_RUNBOOK §N.1`, §B satır 12). (2) Kriter 3, 6 ve 8'in dış payı (teyit, sandbox koşusu, DV-3 kapanışı) OWNER_INPUT'tur; satırlar tarihli "Bekliyor" kaydıyla yazıldı, koşu D-1089/D-1090'da. Bu üç kriterin ✓'si kayıt ve kod payınındır. (3) Sandbox tablosunda test bildirimi referans kurulumda işaretli kaydedilir; geliştirme ortamındaki sipariş son sorguyla çözülür (canlı kurulum olmadığından `test_mode` 1 sorgu sonucu orada uygulanır) — `08 §2.1.10`'un cümlesine uygun. (4) Hedefi "en geç T406" olan beş backlog kaleminin hedef hücresi yerinde D-1090'a ileri atıldı.
- **Working tree (Adım -1):** temiz (devam işçisi kendi worktree'sinde, `origin/task/T406-paytr-3d-secure` `37a9b731`'den)
- **Ana dal CI startup (Adım 0):** ilk işçide 42894790643, 42894790413, 42889878953 success; `origin/main` dal tabanından ilerlemedi (`cda5032e`)
- **Repo memory (bitiş kapısı 9):** parça dosyası kuralıyla `STATUS_CHANGELOG.d/T406.md` ve STATUS tablo satırı; MEMORY "Güncel Durum"a dokunulmadı
