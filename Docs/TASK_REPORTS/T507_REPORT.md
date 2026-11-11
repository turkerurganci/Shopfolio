# T507 — Yasal metin sayfaları ve panelde yönetimi (E-11, E-47)

**Faz:** F5 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS) | **Tarih:** 2026-11-11

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- E-11: `/aydinlatma-metni`, `/cerez-politikasi` (yayındaki son sürümün tam metni; yayın yoksa 404) ve `/islem-rehberi` (sabit metin, satış kapalıyken de 200, kapıya bağlı değil) sayfaları.
- E-47: `GET /panel/yasal-metinler` — dört metnin satırı, düzenlenen iki metnin bölümü (taslak, yayındaki sürüm numarası ve tarihi, taslağın yayından farkı bildirimi, tamamlanma satırı, kapı koşulu satırı), üretilen iki metnin güncel hâli; her form taslağın `Version`'ını taşır.
- `SaveLegalTextDraft` (süzme + yazma) ve `PublishLegalText` (yeni sürüm, iz, 422/409) işleyicileri; aydınlatma taslağı önizlemesi yer tutucuları kimlikteki güncel değerle doldurur (`GetLegalTextDraftPreviewAsync`).
- `NoticeTexts`: kısa ömürlü bildirim metinlerinin tek yeri (07 §6.3.1).
- CSS: `admin.css`, `storefront.css` (main birleşmesinde çakışma çözüldü).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Pages/Legal/*`, `Pages/Panel/LegalTexts.cshtml(.cs)`, `Pages/Shared/_LegalText*.cshtml`
- `src/Shopfolio.Web/Modules/Settings/*` (`ISettingsModule`, `LegalTextRecords`, `LegalTexts`, `SettingsModule`)
- `src/Shopfolio.Web/Infrastructure/Notices/NoticeTexts.cs`, `wwwroot/css/admin.css`, `storefront.css`
- Testler: `tests/Shopfolio.IntegrationTests/Web/LegalTextPageTests.cs`, `Model/LegalTextTests.cs`, `Model/AddressTests.cs`, `Model/MembershipTests.cs`; `tests/Shopfolio.UnitTests/Pages/LegalTextsModelTests.cs`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Güncel yayın sürümü; rehber sabit, satış kapalıyken de görünür | ✓ | `LegalTextPageTests.ThePublishedTexts_...KK1` |
| 2 | Yayınsız iki metin 404; rehber kapıya bağlı değil | ✓ | `AnUnpublishedNoticeAndPolicy_...KK2` |
| 3 | Dört satır, iki bölüm, `Version` taşıyan formlar | ✓ | `TheScreen_ShowsFourLines_...KK3` |
| 4 | Sürüm no/tarih, fark, tamamlanma, kapı satırı | ✓ | `TheSection_SaysTheVersion...KK4`, `TheCompletionLine_...KK4` |
| 5 | Yer tutucular güncel kimlik değeriyle | ✓ | `ThePreview_FillsThePlaceholders...KK5`; `LegalTextTests` önizleme |
| 6 | Taslak süzülerek yazılır; yayın ve iz değişmez, taslak sürümü artar | ✓ | `TheDraftSave_CleansTheText...KK6`; `LegalTextTests` SaveDraft temizleme |
| 7 | 422 (boş metin, boşaltılan barındırma konumu, tür dışı, üretilen metin); 409 iki düğme | ✓ | `AnEmptyText_Is422`, `EmptyingAFilledHostingLocation...`, `ATextTypeOutsideTheTwoEditedOnes` (Theory 4 değer), `AStaleVersion_Is409...KK7` |
| 8 | Aynı metinde yeni sürüm; onay penceresi/bildirim/yeniden onay yok; iz | ✓ | `APublication_OfTheSameText...KK8` |
| 9 | Eksik aydınlatma 422, taslak yazılmaz; eşzamanlı ikinci yayın 409 | ✓ | `AnIncompleteNotice_Is422...`, `ASecondPublication...409`, `TwoPublicationsAtOnce...KK9` |
| 10 | İlk iki yayın kapıların koşulu; çerez politikasından sonra vitrin çerez yazar | ✓ | `TheFirstPublicationsOfTheTwoTexts_Open...KK10` |

### Öz denetim (bitiş kapısı 2)

Önceki iki işçinin diff'i (`git diff origin/main...HEAD`, 20 dosya) dahil bütün maddeler yeniden denetlendi.

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kriter 1–10 ve KK1–KK10 | ✓ | Yukarıdaki tablo; testler yeşil |
| `09 §25` modül sınırı: sayfada `DbContext`/`SaveChanges` yok | ✓ | Sayfalar yalnız `ISettingsModule` (Contracts) çağırır; `LegalTextPages.cs`, `LegalTexts.cshtml.cs`; mimari testler Unit tam koşuda yeşil |
| `09 §15.3` m.1: metin yalnız görüntü yardımcısıyla; süzme yazımda | ✓ | `SaveDraft` süzmeyi üstlenir (aşağıda ⚠1); `_LegalText.cshtml` |
| Kural değişince öteki yerler (sik-fail 4): `NoticeTexts` ↔ 07 §6.3.1 tablosu | ✓ | Yalnız tabloda tırnaklı cümlesi olan üç kod (`Saved`, `CartItemAdded`, `InvitationSent`) var; iki iletişim kodu D-1232 |
| Kaynak taramaları çağrı biçimi (sik-fail 1/2) | ✓ | Yeni sayfalar mevcut taramalardan geçer (Unit tam yeşil); yeni tarama kuralı eklenmedi, istisna genişletilmedi |
| 06 "Varsayılan"/FK dizini (sik-fail 3) | ✓ (kapsam dışı) | Şema/migration değişikliği yok |
| Karar kaydı gerektiren yeni 09 metni (sik-fail 5) | ✓ (kapsam dışı) | 09 / belge kuralı değişmedi |
| Yeni backlog kalemleri parça dosyasında | ✓ | `Docs/DEFERRED_BACKLOG.d/T507.md` (D-1232, D-1233) |
| Kayıt: STATUS satırı + changelog parçası | ✓ | `Docs/IMPLEMENTATION_STATUS.md` T507 ⏳; `Docs/STATUS_CHANGELOG.d/T507.md` |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests` | Tam koşu 7139 yeşil (yeni testlerden önce); `LegalTextsModelTests` 8/8 |
| Integration | ✓ | `dotnet test tests/Shopfolio.IntegrationTests --no-build --filter-namespace/--filter-class ...` (ad alanı parçaları) | Web 775, Model A–G,I,L 746, M,N,P,R 370, S* 294, Administration 69, Catalog 82, Commands 70, Concurrency 30, CorporateContent 185, Database 36, Disposal 49, FileSecrets 24, Files 69, Jobs 85, Logging 30; Ordering 638, Outages 53, Payment 59, Security 34, SignedEndpoints 11, Pages 0 (yalnız örnek sayfalar); hepsi 0 kırmızı, tam Integration kapsandı |
| `dotnet format` | ✓ | src + iki test projesi | temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu (tur 1) | ✓ PASS — tur 1 (2026-11-11, head `7eadf545`; `origin/main` `a9270c02` (T511 #545) birleşik ağaçta, birleştirme çakışmasız). Kriter 1–10 ve KK1–KK10 ✓ (kanıtlar: `LegalTextPageTests` 20 durum, `LegalTextTests` önizleme ve temizleme, `LegalTextsModelTests`; kod okuması `LegalTexts.SaveDraftAsync`/`GetDraftPreviewAsync`/`PublishAsync`, `LegalTextsModel`, `PrivacyNoticeModel`, `CookiePolicyModel`, `ProcessGuideModel`, `_LegalTextSection`). Yayının ret hâlinde taslak yazımı `ITransactionRunner` ile geri alınır; eşzamanlı ikinci yayın taslak satırının sürümüne takılır (409). `09 §25` ilgili satırlar ✓ (sayfada `DbContext` yok, panel klasör yetkisi ve `no-store` kuralı, POST'lar sahtecilik süzgecinde, `RequestBodyClass.LongText` 1 MB, uzun metin yazımda `RichTextSanitizer`, gösterimde yalnız `RichText.Render`; `Html.Raw`/satır içi betik ve stil yok; yeni bağımlılık, şema, sır yok). Yerel: derleme `-warnaserror` 0 uyarı, `dotnet format` 3 proje temiz, Unit 7147/7147 (birleşik ağaçta 7158/7158), betik testleri 4/4, Integration `*LegalText*` + `Model.AddressTests` + `Model.MembershipTests` 190/190 (birleşik ağaçta `LegalTextPageTests` + `OrderPageTests` 30/30); CI run 43078021979 dokuz iş yeşil. `12`: VAL-A300 ve VAL-D034 → PASS; VAL-A299 tarayıcı gözlemi ister → BEKLEMEDE. |
| Bulgu sayısı (tur 1) | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (tur 1) | 2 — **K1** (bu PR'da kapatıldı): hedefi T507 olan D-219, D-269, D-730 işi yapılmıştı ama backlog'da açık duruyordu; üçü bu commit'te ✅ kapandı. **K2** (bu PR'da kapatıldı): D-222'nin "kayıt, onay adımı ve IBAN giriş ekranları T507 …" payı T507'nin kriterlerinde yok; hedef ilgili ekranların task'larına çevrildi. |
| Yapım raporu karşılaştırması (tur 1) | Kabul kriterlerinde tam uyum (1–10 ✓); öz denetim tablosu var, maddeler kanıtlı. Uyuşmazlık 1: rapor hedefi T507 olan backlog kalemlerini (D-219, D-269, D-730, D-222 payı) anmıyor (K1, K2). |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T507-yasal-metinler`
- **PR:** başlık "T507: Yasal metin sayfaları ve panelde yönetimi (E-11, E-47)"

## Dış Varsayımlar

Dış varsayım: yok.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Contact request closed/reopened bildirim metni | D-1232 | 07 §6.3.1 tırnaklı cümle vermiyor; ekranı kuran task yazar |
| 2 | Taslağın yayından farkının ayrıntılı gösterimi | D-1233 | 04 ayrıntıyı tanımlamıyor; yalnız bildirim |

## Notlar

- **⚠ (kurala en uygun seçenekle karar verildi):** (1) `SaveDraft` süzmeyi üstlendi (T366 notu 09 §15.3 m.1'e aykırıydı); (2) üretilen metinler `<pre>` içinde; (3) Yayına al düğmesi 409'da da durur; (4) barındırma alanı çok satırlı textarea; (5) yayın sırasında taslak yazımı ret'te geri alınır; (6) Yayına al düğmesinde onay penceresi yok (kabul k8).
- Birkaç koşuda başka makine koşularından kaynaklı Npgsql bağlantı zaman aşımı görüldü; parça küçültülüp yeniden koşuldu, geçti.
- **Working tree (Adım -1):** temiz. **Repo memory:** parça dosyaları dışında dokunulmadı.
