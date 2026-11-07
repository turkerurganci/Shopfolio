# T310 — Kargo şirketi listesi ve takip bağlantısı kalıpları

**Faz:** F3 | **Durum:** ⏳ Devam ediyor (doğrulama bekliyor) | **Tarih:** 2026-11-07

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

- **Branch:** `task/T310-kargo-sirketleri`
- **Commit:** `5e0c1d0` (kod), devam commit'i (plan düzeltmesi, rapor)
- **PR:** #423
- **CI:** yoklanmadı
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
| 2 | T601 blok boyutu ve kriter sayısı taraması zaten aşıyordu, şimdi biraz daha | — | ⚠ Yönetici/gate'e bildirilir; yeni backlog kaydı açılmadı |
