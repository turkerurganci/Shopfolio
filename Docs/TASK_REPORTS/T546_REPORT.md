# T546 — Gezinme ve geçiş eşitlik denetimleri: iskeletler, geçişler, dışarıdan girişler, dönüşler

**Faz:** F5 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-15

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- `04 §3.1`–§3.5 ve §3.7'nin 125 tablo satırı ile §3.6'nın beş paragrafı, kodun bağlantı, yönlendirme ve sayfa yollarıyla iki yönlü karşılaştırılır (`UnitTests/Consistency/`, `Consistency` özellikli, veritabanına ve ağa dokunmaz, dosya yazmaz).
- **Belge tarafı** (`NavigationDocument`): tabloları ve §3.6 paragraflarını okur; her satırın nereden, tetikleyici ve nereye hücresi boş olamaz (`EmptyCell`).
- **Kod tarafı** (`NavigationSites`): `src/Shopfolio.Web` altındaki her `<a href>` (Razor açıklamaları ve yalnız `#` ile başlayanlar hariç) ve her yönlendirme çağrısı (`FormResults.SeeOther`, `SeeOtherToGoogle`, `Redirect`, `RedirectToPage`, `LocalRedirect`, ...) adres ifadesiyle taranır.
- **Envanter** (`NavigationInventory`): her satır için kimlik, kaynak ve hedef ekran kümesi (E-nn) ya da metin işareti, ve kanıt: bağlantı, yönlendirme, metin, sayfa yolu (derlenmiş `@page`'lerden) ya da açık backlog kalemi. Kodda olup hiçbir satırın ve hiçbir muafiyetin sahiplenmediği bağlantı ve yönlendirme `UndocumentedTransition` olarak bildirilir.
- **Muafiyetler** kapalı ve tek tek adlandırılmıştır (dosya + tür + ifade + gerekçe): ekranın kendi içinde kalan işlemler (sayfalama, süzgeç kaldırma, ekran içi PRG, `04 §3` girişi), dosya indirme (ekran değil) ve ekran tanımlarının yazdığı bağlantılar. Alıntılı muafiyetin alıntı yaptığı `04` paragrafı kendi sözcüğünü taşımıyorsa `ExemptionNotCited`; koddan kalkan muafiyet `DeadExemption`; hem satır kanıtı hem muafiyet olan site `ExemptAndClaimed` bildirir.
- **Menü** (`§3.2`): panel menüsü (`PanelFrameBuilder.MenuOf`) menü tablosuyla giriş giriş, sıra dahil karşılaştırılır (ekran → `PanelFramePaths` adresi).
- **Dönüşler** (`§3.6`): paragraflar envanterde sabitlenir; dönüş kuralları kararı veren işlevlerden (`ReturnPath.AfterSignIn`, `Resolve`, `AfterSignOut`, iki kapının giriş adresleri, ödeme adımının giriş adresi) sınanır.
- Kodda değişiklik yok; yalnız test ve kayıt dosyaları.

## Etkilenen Modüller / Dosyalar

- `tests/Shopfolio.UnitTests/Consistency/NavigationDocument.cs`, `NavigationSites.cs`, `NavigationInventory.cs`, `NavigationCheck.cs`, `NavigationConsistencyTests.cs` (yeni)
- `Docs/TASK_REPORTS/T546_REPORT.md`, `Docs/STATUS_CHANGELOG.d/T546.md`, `Docs/IMPLEMENTATION_STATUS.md` (T546 satırı)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Müşteri geçişleri (`§3.3`) kodla eşit | ✓ | `NavigationMap_EqualsTheCode_InBothDirections`: 39 satır (3.3.1–3.3.39), her biri kaynak ve hedef ekranıyla ve en az bir kanıtla; tek istisna 3.3.13 (aşağıda ⚠) |
| 2 | Panel geçişleri (`§3.4`) eşit | ✓ | aynı test: 23 satır (3.4.1–3.4.23) |
| 3 | Dış girişler (`§3.5`) eşit | ✓ | aynı test: 15 satır; kanıt derlenmiş `@page` yolları (`Page` kanıtı) ve oturum işleyicisinin yönlendirmeleri |
| 4 | Dönüşler (`§3.6`) eşit | ✓ | aynı test: 3.6.1–3.6.5 paragrafları; `SignInReturn_GoesToTheAskedPage_ExceptTheEntryScreens`, `PanelSignInReturn_...`, `CustomerSignOut_...`, `TheTwoGatesAreSeparate_...`, `CheckoutSignIn_ReturnsToThePaymentStep` |
| 5 | Panel ↔ vitrin (`§3.7`) eşit | ✓ | aynı test: 3.7.1–3.7.5 (Düzenlemeye dön = 3.7.4, oturumsuz taslak adresi = 3.7.5) |
| 6 | İskelet (`§3.1`, `§3.2`) çerçevede ve denetimde | ✓ | 21 + 22 satır; `PanelMenu_EqualsTheMenuTableOfTheDocument_EntryByEntry` |
| 7 | Tabloda olmayan geçiş koda sızmaz; `Consistency` özellikli birim testi | ✓ | `UndocumentedTransition` denetimi; test sınıfı `[Trait("Consistency", "true")]`, `UnitTests/Consistency/` altında |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| Kriter 1–7 | ✓ | yukarıdaki tablo; `NavigationConsistencyTests.cs` |
| KK1 tablodan satır silinince ya da bağlantı kalkınca kırılır | ✓ | `CustomerTransition_DeletedFromTheTable_IsReported`, `CustomerTransition_WhoseLinkLeavesTheCode_IsReported`, `CustomerTransition_WhoseTargetChangesInTheTable_IsReported` |
| KK2 panel geçişi eksilince kırılır | ✓ | `PanelTransition_DeletedFromTheTable_IsReported`, `PanelTransition_WhoseLinkLeavesTheCode_IsReported` |
| KK3 dış giriş eksilince kırılır | ✓ | `OutsideEntry_DeletedFromTheTable_IsReported`, `OutsideEntry_WhosePageLeavesTheCode_IsReported` |
| KK4 dönüş hedefi değişince kırılır | ✓ | `ReturnTarget_ChangedInTheDocument_IsReported`, `ReturnRedirect_LeavingTheCode_IsReported` |
| KK5 `§3.7`'nin beş satırından biri silinince ya da bağlantı kalkınca kırılır | ✓ | `PanelStorefrontRow_DeletedFromTheTable_IsReported` (3.7.3, 3.7.5), `BackToEditingLink_LeavingTheCode_IsReported` (3.7.4), `BackToThePanelLink_LeavingTheCode_IsReported` (3.7.3) |
| KK6 iskelet bağlantısı eksilince kırılır | ✓ | `StorefrontSkeletonLink_LeavingTheCode_IsReported`, `PanelSkeletonLink_LeavingTheCode_IsReported`, `PanelMenu_WithARenamedEntryInTheDocument_IsReported`, `PanelMenu_WithAnEntryPointingToAnotherScreen_IsReported` |
| KK7 koda tabloda olmayan geçiş eklenince kırılır; testler `UnitTests/Consistency/` altında | ✓ | `LinkAddedToTheCode_WithoutARow_IsReported`, `RedirectAddedToTheCode_WithoutARow_IsReported`, `PanelLinkAddedToTheCode_WithoutARow_IsReported`, `ExemptSite_LeavingTheCode_IsReported`, `CitedExemption_WhoseParagraphLosesTheWord_IsReported` |
| `09 §19.2` m.2: eşitlik denetimi veritabanına, ağa dokunmaz, dosya yazmaz | ✓ | yalnız `File.ReadAllText` ve `Directory.Enumerate*`; değişen metin bellekteki kopyadır |
| `09 §19.2` m.3: kırıldığı gösterilir | ✓ | yukarıdaki 21 olumsuz test, değiştirilmiş belge ya da kaynak kopyasıyla |
| `09 §19.2` m.7: adlandırma | ✓ | `<Sınanan>Tests`, `Yöntem_Durum_Beklenen`; dosya sınıf adını taşır (yardımcı sınıflar kendi dosyasında) |
| `09 §25`: diff yalnız görevi içerir | ✓ | kodda değişiklik yok |
| Güvenlik taraması | ✓ | sır, yetki, girdi doğrulama etkisi yok; yeni bağımlılık yok; yalnız repo dosyaları okunur |
| Sık FAIL (1) istisna geniş | ✓ | muafiyetler dosya + tür + tam ifade; joker yok; ölü ve çift sahipli muafiyet testle yakalanır |
| Sık FAIL (2) çağrı biçimi kaçıyor | ✓ | tarama: Razor `<a ... href="...">` (ek nitelik, `@(...)` ifadesi, içinde tırnak), `FormResults.SeeOther` (adlı ya da çok satırlı argüman, satır yorumu), `SeeOtherToGoogle`, `Redirect(`, `RedirectToPage`, `LocalRedirect`, `RedirectPermanent`, `RedirectToRoute`, `RedirectToAction`; Razor açıklamaları ve `///` satırları atlanır |
| Sık FAIL (3) 06 varsayılan/FK | — | veri modeline dokunulmadı |
| Sık FAIL (4) aynı sayıyı anan öteki yerler | ✓ | `09 §21.4` m.1 T546'yı zaten sayar; sayı sabitleri (125 satır, 5 paragraf, 18 menü satırı) testte tablodan okunur ya da `Assert.Equal` ile kilitlidir ve belge değişirse kırılır |
| Sık FAIL (5) 09 metni karar kaydısız | ✓ | belgeye dokunulmadı |
| Backlog: hedefi T546 olan kalem | ✓ | `grep -nE "T546" Docs/DEFERRED_BACKLOG.md Docs/DEFERRED_BACKLOG.d/*.md` boş döndü |
| T538 emsali: sessiz atlama yok | ⚠ | tek açık fark 3.3.13 (aşağıda); öteki her fark muafiyet olarak kapalı listede ve gerekçeli |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit (Consistency süzgeci) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj --filter-trait "Consistency=true"` | 361 geçti, 0 kaldı (yeni sınıf 48) |
| Unit (Consistency dışı) | ✓ | `... --filter-not-trait "Consistency=true"` | 7420 geçti, 0 kaldı |
| Integration | — | koşulmadı | TAM: CI (talimat 2026-11-12); dokunulan Integration sınıfı yok, kodda değişiklik yok |
| Biçim | ✓ | `dotnet format Shopfolio.slnx --no-restore --include <değişen .cs>` | değişiklik yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | — |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T546-gezinme-gecis-esitlik`
- **Commit:** `67237e82` — T546: Gezinme ve geçiş eşitlik denetimleri: iskeletler, geçişler, dışarıdan girişler, dönüşler
- **PR:** #580
- **CI:** dal CI'ını yönetici izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok. Denetim yalnız repo dosyalarını (`Docs/04_UI_SPECS.md`, `Docs/DEFERRED_BACKLOG*`, `src/Shopfolio.Web`) ve derlenmiş Razor sayfalarını okur; plan katmanı, kota, dış servis ya da paket sürümü gerektirmez.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `04 §3.3.13`: E-12 sepet kaleminin adı ve görseli E-04'e bağlanmıyor (kodda bağlantı yok; `CartLine` ürün adresini ve görseli taşımıyor) | D-1189 (açık, hedef F5 kapısı) | Düzeltme Catalog'un sepet okumasına ve `CartLine` sözleşmesine dokunur; T508/T509'da kayıtlı. Envanterde satır `OpenGap` kanıtı taşır: D-1189 açık olmalı ve satır başka kanıt taşımamalıdır; kalem kapanıp bağlantı kodda belirince test satırın kanıtını bağlantı olarak istemeye zorlar |

## Notlar

- **Working tree (Adım -1):** temiz (`git status --short` boş); `core.hooksPath` = `scripts/git-hooks`.
- **Ana dal CI startup (Adım 0):** `43691824226` success, `43691824223` success, `43666144462` success.
- **Repo memory (bitiş kapısı 9):** dokunulmadı.
- **⚠ 1 — 3.3.13 açık fark:** bkz. yukarıdaki tablo; karar: mevcut D-1189 kaydı gerekçeli ve hedefi faz kapısı olduğundan T546 kodda sözleşme değiştirmedi.
- **⚠ 2 — 04 §3 tablosunda satırı olmayan, ekran tanımlarının yazdığı bağlantılar** (`Spec` muafiyetleri, her biri `04` paragrafıyla alıntılı ve testle doğrulanır): hesap silme sonuç hâlinin iki bağlantısı (5.25.13), sipariş takibinde üyeye giriş hatırlatması (5.15.3), sipariş sayfasında "Takip et" (5.16.8, 9.9.9), ödeme yöntemleri ekranından E-46'ya not bağlantısı (9.17.4), aydınlatma metni bağlantıları (2.12.1.6), şubenin "Haritada aç" bağlantısı (5.10.4), kayıt ekranında "e-posta kayıtlı" iken giriş bağlantısı (5.20.9). Bunları `§3` tablosuna satır olarak eklemek belge değişikliğidir ve karar kaydı ister; eklenmedi.
- **⚠ 3 — Kapsam sınırı:** tarama `src/Shopfolio.Web` içindeki bağlantı ve yönlendirme çağrılarını görür; C# dizgesi olarak yazılmış HTML (hata ve oran sınırı sayfalarının sabit yanıtları) bağlantı sayılmaz — bunlar ekran değil, çerçevesiz sabit yanıtlardır (`04 §2.7.3.2`).
- **⚠ 4 — Satır kanıtı paylaşımı:** bazı satırlar aynı bağlantıyı paylaşır (ör. 3.3.22–3.3.24 `_OrderPage` `@link.Href`; menü satırları 3.1.12–3.1.16 `@item.Path`; panel liste satırları 3.4.6, 3.4.8, 3.4.10 `_PanelTable` `@row.RecordPath`). Paylaşılan bağlantıya ek olarak her biri için ayırt edici metin kanıtı yazıldı (sipariş sayfasındaki adres son ekleri; menü adı sabitleri); panel liste satırlarında (3.4.6, 3.4.8, 3.4.10) listenin kendi satır adresini kuran satır da metin kanıtı olarak yazılıdır.
