# T310 — Kargo şirketi listesi ve takip bağlantısı kalıpları

**Faz:** F3 | **Durum:** ✓ Tamamlandı | **Tarih:** 2026-11-07

---

## Yapılan İşler

- `CarrierCatalog` (kodda sabit liste: `YurticiKargo`, `ArasKargo`, `SuratKargo`, `Ups`), `CarrierEntry`, `RetiredCarrierEntry`, `TrackingLink`, `ShippingDisplay` — kod adı, kalıp ve çıkmış şirket kuralları kurucuda denetlenir; bağlantı kurma ve sipariş teslimat alanı gösterimi (`Describe`).
- 31 birim testi (`CarrierCatalogTests`): `08 §2.4` tablosuyla iki yönlü eşitlik, kural ihlalleri, yüzde kodlama, üç durumda bağlantı yok, `noreferrer`/yeni sekme, kaynak taramasıyla giden istek yok.
- **Plan düzeltmesi (K-1576, yönetici kararı, sahip yetkisiyle):** gerçek takip numarası yalnız sahibin verebileceği girdi olduğundan DV-32'nin tarayıcı denemesi T601'e (canlı kanıt) taşındı. `11` T310 kriter 3 ve KK3 güncellendi; T601'e kriter 12 ve KK12 eklendi; `08 §2.4` ve §4.1 DV-32 satırı T601'i gösterir; `12` `VAL-E033` K-1576 ile T601'e atıflandı.
- **DV-33 yapıldı (2026-11-07, sayfa okuma):** MNG Kargo adresi DHL eCommerce ana sayfasına düşüyor, numara adreste değil sayfa betiğiyle sorgulanıyor (kalıp yok); PTT Kargo `gonderitakip.ptt.gov.tr` adresi `ptt.gov.tr` ana sayfasına yönleniyor (302); HepsiJet 403. Üçü de "Diğer" kalır; sonuç `08 §4.1` DV-33 satırında tarihli.
- Karar kaydı: `PRODUCT_DISCOVERY_STATUS.md` K-1576.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Ordering/Contracts/CarrierCatalog.cs`
- `tests/Shopfolio.UnitTests/Ordering/CarrierCatalogTests.cs`
- `Docs/08_INTEGRATION_SPEC.md` (§2.4, §4.1 DV-32/DV-33), `Docs/11_IMPLEMENTATION_PLAN.md` (T310, T601), `Docs/12_VALIDATION_PROTOCOL.md` (`VAL-E033`), `Docs/PRODUCT_DISCOVERY_STATUS.md` (K-1576)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Liste kodda, panelden düzenlenmez | ✓ | `CarrierCatalog.Default`; `Default_ListIsInCodeOnly_NoSettingOrTableNamesIt` |
| 2 | Kod adı ASCII, tek kelime, büyük harf, kalıcı, tekil | ✓ | `CheckCode`; `Default_HoldsEveryCodeOnce…`, `Constructor_RejectsACode…` (retired dahil tekillik) |
| 3 | `https` + tek yer tutucu; MNG/PTT/HepsiJet okunabilirse girer, okunamazsa "Diğer"; DV-33 tarihli sonuç; DV-32 T601'de (K-1576) | ✓ | `CheckPattern`; `Constructor_RejectsAPatternThat…`; `08 §4.1` DV-33 satırı (2026-11-07); DV-32 satırı "Bekliyor — T601" |
| 4 | Numara yüzde kodlanır, başka parça türetilmez | ✓ | `BuildLink` (`Uri.EscapeDataString`); `BuildLink_EncodesTheNumberAndDerivesNothingElse` |
| 5 | "Takip et" yalnız listedeki şirket + numara varsa | ✓ | `BuildLink_WithoutANumber_IsNull`, `Describe_ListedCompanyWithoutNumber_HasNoLink` |
| 6 | Diğer / kendi aracımız / çıkmış şirkette bağlantı yok; ad+numara ya da yalnız beyan | ✓ | `Describe_Other…`, `Describe_RetiredCompany…`, `Describe_OwnVehicle…` |
| 7 | Çıkmış şirket seçilemez, eski siparişte adıyla çözülür | ✓ | `IsSelectable_OnlyForAListedCompany`, `FindName`, `Describe_RetiredCompany…` |
| 8 | Yeni sekme + `noreferrer` | ✓ | `TrackingLink.Target/Rel`; `Link_OpensInANewTabAndCarriesNoreferrer` |
| 9 | Sunucudan kargo şirketine istek yok | ✓ | `Source_MakesNoOutgoingRequest` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| Kriter 1–9 (yukarıdaki tablo) | ✓ | Tablodaki test adları; 31/31 geçti |
| KK1 liste kodda, ayar/tablo yok | ✓ | `Default_ListIsInCodeOnly_NoSettingOrTableNamesIt` |
| KK2 kod adları | ✓ | `Default_HoldsEveryCodeOnce…` |
| KK3 tek yer tutucu, `https`; DV-33 tarihli ve sonuçlu; DV-32 T601'i gösterir | ✓ | `08 §4.1` iki satır; `11` T310 KK3 düzeltilmiş |
| KK4 boşluklu numara kodlanır | ✓ | `BuildLink_EncodesTheNumberAndDerivesNothingElse` |
| KK5 numarasız şirkette bağlantı yok | ✓ | `BuildLink_WithoutANumber_IsNull` |
| KK6 üç durumda bağlantı yok | ✓ | `Describe_Other…`, `Describe_RetiredCompany…`, `Describe_OwnVehicle…` |
| KK7 çıkmış şirket | ✓ | `IsSelectable_OnlyForAListedCompany`, `Describe_RetiredCompany…` |
| KK8 `noreferrer` | ✓ | `Link_OpensInANewTabAndCarriesNoreferrer` |
| KK9 giden istek yok | ✓ | `Source_MakesNoOutgoingRequest` |
| Aynı sayıyı/kararı anan yerler | ✓ | grep: DV-32/DV-33 `08` (§2.4 iki cümle, §4.1 iki satır), `11` (T310 başlık satırı, kriter 3, KK3; T601 kriter 12, KK12), `12` `VAL-E033`/`E034`, kod yorumu; hepsi K-1576'ya hizalı |
| Karar kaydı | ✓ | K-1576; `11_task_blok_denetle.py`: "K atıfları: eksik 0" |
| Kısmen (~) kriter yok | ✓ | tabloda ~ yok |
| `09 §25` ilgili satırlar; güvenlik taraması | ✓ | Yeni dış çağrı, yeni paket, girdi yolu yok; adres yalnız yüzde kodlanmış numara alır |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests` | 3024 / 3024 geçti (Consistency testleri dahil); `CarrierCatalogTests` 31/31 |
| Integration | — | — | Çalıştırılmadı: veritabanına, uca, göçe dokunulmadı |
| Contract | — | — | Yok |
| Belge taraması | ⚠ | `python scripts/doc-scans/11_task_blok_denetle.py` | 3 SORUN, üçü de main'de önceden var (T330 blok bayt, T601 blok bayt, T601 kriter sayısı 11); T601'e eklenen kriter 12 bu sayıları büyüttü (T601 blok 2876 → 3271 bayt, kriter 11 → 12) |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-07, head `c3e1670`) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 1 (K1 → D-196) |

### Doğrulama tur 1 — ✓ PASS

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` ana deponun `scripts/git-hooks`'u; main son üç tamamlanmış run 42356915467, 42356915419, 42351368236 `success`; hafızada T310 satırı var.

**Kriterler 1–9 ✓, KK1–KK9 ✓, `09 §25` ✓.** Kanıtlar (doğrulayıcı):

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kriter 1 / KK1 | ✓ | `CarrierCatalog.Default` kodda sabit (dört şirket); `src` içinde `Carrier` yalnız `Order` alanı, göç ve bu dosyada — tablo, `DbSet`, ayar anahtarı, panel ekranı yok (grep); `Default_ListIsInCodeOnly_NoSettingOrTableNamesIt` |
| Kriter 2 / KK2 | ✓ | `CheckCode`: ilk harf ASCII büyük, tümü ASCII harf/rakam, etkin + çıkmış listede tekil; `Constructor_RejectsACode…` (Türkçe harf, boşluk, tire, rakamla başlama), `…GivenTwice_AlsoAcrossListedAndRetired`; kalıcılık: kod adı çıkmış listeye taşınır, silinmez (`RetiredCarrierEntry`) ve liste `08 §2.4` tablosuyla iki yönlü eşit (`Default_EqualsTheTableOfTheIntegrationSpecInBothDirections`) |
| Kriter 3 / KK3 | ✓ | Dört kalıp `08 §2.4` ile birebir, `https://` + tek `{takipNumarasi}` (`CheckPattern`, `Default_Holds…`, `Constructor_RejectsAPattern…` `http`, `HTTPS`, iki yer tutucu); `08 §4.1` DV-33 satırı "Tamamlandı (2026-11-07, T310; K-1576)" ve üç sonuç; DV-32 satırı "Bekliyor — T601". Doğrulayıcı DV-33'ü HTTP ile yineledi: `mngkargo.com.tr/gonderi-takip/` 301 → `dhlecommerce.com.tr/gonderi-takip/` 302 → ana sayfa; `gonderitakip.ptt.gov.tr/Track/Verify?q=…` 302 → `ptt.gov.tr`; `hepsijet.com/gonderi-takip` 403 — üçü "Diğer" |
| Kriter 4 / KK4 | ✓ | `BuildLink` yalnız `Uri.EscapeDataString(numara)`'yı yer tutucuya koyar; `"AB 12&x=1/é#"` → `AB%2012%26x%3D1%2F%C3%A9%23` |
| Kriter 5 / KK5 | ✓ | Numara boş/boşluk ya da şirket listede değilse bağlantı `null`; `BuildLink_WithoutANumber_IsNull`, `Describe_ListedCompanyWithoutNumber_HasNoLink` |
| Kriter 6 / KK6 | ✓ | `Describe`: kendi aracımızla teslimde yalnız beyan (başıboş değerler yok sayılır); "Diğer" (`Carrier` boş, `06 §4.17`) ad + numara; çıkmış şirket adıyla + numara; üçünde bağlantı yok — `08 §2.2.6` B-5 kargo bloğuyla aynı |
| Kriter 7 / KK7 | ✓ | `IsSelectable` çıkmış ve bilinmeyen kodda false; `FindName` çıkmış şirketi adıyla çözer; `Describe_RetiredCompany…` |
| Kriter 8 / KK8 | ✓ | `TrackingLink.Target = "_blank"`, `Rel = "noreferrer"`; sayfadaki çizim T350'nindir (`VAL-A062`) |
| Kriter 9 / KK9 | ✓ | Kaynakta `HttpClient`/`System.Net`/`WebRequest` yok; `Source_MakesNoOutgoingRequest` |
| Plan düzeltmesi (K-1576) | ✓ | Karar satırı (gerekçe, elenenler, etki) var; `11` T310 kriter 3 / KK3, T601 kriter 12 / KK12, `08` §2.4 ve DV-32/33, `12` `VAL-E033` atıfları birbiriyle tutarlı; `08 §4.1` sayım paragrafı etkilenmez; `11_task_blok_denetle.py` "K atıfları: eksik 0" (T601 boyutu K1) |
| `09 §25` | ✓ | Ad alanı ve klasör `Modules/Ordering/Contracts` (B-5 için `Notifications` de okuyacak; `AuditOperationText` emsali); `record`'lar değişmez, sınıf `sealed`; `!`, `dynamic`, uyarı bastırma yok; yeni paket, ayar, göç, uç yok; Architecture testleri Unit içinde yeşil |
| Güvenlik | ✓ | sır yok; adres yalnız sabit `https` kalıbı + yüzde kodlu numara (şema/konak enjeksiyonu yok); yetki etkisi yok; yeni bağımlılık yok; gösterilen ad ve numaranın HTML kaçışı sayfanın (T350) işi |

**Testler (doğrulayıcı, yerel):** `dotnet build Shopfolio.slnx -warnaserror` 0 uyarı 0 hata; `dotnet format --verify-no-changes` Web ve UnitTests ayrı ayrı çıkış 0 (IntegrationTests değişmedi); Consistency 209/209, Unit 2815/2815 (toplam 3024; `CarrierCatalogTests` 31/31); betik testleri 4 dosya geçti. Integration koşulmadı — dal veritabanına, uca ve göçe dokunmuyor; dal CI 42353336593 `success` (head `c3e1670`).

**Task satırları (`12 §7.5`):** `11 §7.1` T310 kaynakları KP-19, KD-2. KP-19 grep'i `VAL-A060`…`VAL-A063` getirir (KP-19 → T310, T350, T511): beklenenleri vitrin ve sipariş sayfasıdır (uç yanıtı, tarayıcı gözlemi), T310 sayfa kurmaz → `BEKLEMEDE`. KD-2 satırı yok. DV satırları: `VAL-E034` (DV-33, DÜŞÜK, gerçek) kriter 3'le karşılandı → `BEKLEMEDE` → `PASS`; `VAL-E033` (DV-32) K-1576 ile T601'de → `BEKLEMEDE`.

**K1 → D-196:** T601 bloğu `11 §5` madde 8'in sınırını aşıyor (main'de 2876 B / 11 kriter; bu PR ile 3271 B / 12 kriter). T310'un kriterlerini etkilemez; bölme ya da sınır kararı F6'dan önce.

**Yapım raporu karşılaştırması:** uyumlu; öz denetim tablosu her kriter ve KK için kanıtlı. Rapor T601 aşımını ⚠ olarak yazıp backlog kaydı açmamıştı (D-196 açıldı); raporda `12` task satırları bölümü yoktu ve `VAL-E034` `Durum`u yazılmamıştı (bu doğrulamada yazıldı).

**Merge notu:** dal `origin/main` (T380, #419) ile birleştirildi; çakışma yalnız `MEMORY.md` ve `IMPLEMENTATION_STATUS.md`'de, kayıtlar korunarak çözüldü (T380 girişi `STATUS_CHANGELOG.md`'ye taşındı); kodda çakışma yok.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T310-kargo-sirketleri`
- **Commit:** `5e0c1d0` (kod), devam commit'i (plan düzeltmesi, rapor)
- **PR:** #423
- **CI:** dal CI 42353336593 `success` (head `c3e1670`; doğrulayıcı)
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dört şirketin adresi yönlenmeden açılır | `08 §4.1` DV-32 "Mevcut" sütunu (HTTP 200, önceki oturum) | Bu task'ta yeniden denenmedi; numaranın gönderiyi açması T601'de denenir |
| 2 | MNG, PTT, HepsiJet kalıbı okunabilir | WebFetch 2026-11-07: DHL eCommerce ana sayfası (betikle sorgu), PTT 302 → ana sayfa, HepsiJet 403 | Okunamadı; üçü "Diğer" |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | DV-32 tarayıcı denemesi (gerçek numara) | — (T601 kriter 12) | Gerçek numara yalnız sahipte; K-1576 |
| 2 | T601 blok boyutu ve kriter sayısı taraması zaten aşıyordu, şimdi biraz daha | D-196 | Doğrulama tur 1 K1 olarak backlog'a yazıldı |
