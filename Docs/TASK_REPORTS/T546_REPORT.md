# T546 — Gezinme ve geçiş eşitlik denetimleri: iskeletler, geçişler, dışarıdan girişler, dönüşler

**Faz:** F5 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-15

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- `04 §3.1`–§3.5 ve §3.7'nin 125 tablo satırı ile §3.6'nın beş paragrafı, kodun bağlantı, yönlendirme ve sayfa yollarıyla iki yönlü karşılaştırılır (`UnitTests/Consistency/`, `Consistency` özellikli, veritabanına ve ağa dokunmaz, dosya yazmaz).
- **Belge tarafı** (`NavigationDocument`): tabloları ve §3.6 paragraflarını okur; her satırın nereden, tetikleyici ve nereye hücresi boş olamaz (`EmptyCell`).
- **Kod tarafı** (`NavigationSites`): `src/Shopfolio.Web` altındaki her `<a href>` (Razor açıklamaları ve yalnız `#` ile başlayanlar hariç) ve her yönlendirme çağrısı (`FormResults.SeeOther`, `SeeOtherToGoogle`, `Redirect`, `RedirectToPage`, `LocalRedirect`, ...) adres ifadesiyle taranır.
- **Envanter** (`NavigationInventory`): her satır için kimlik, kaynak ve hedef ekran kümesi (E-nn) ya da metin işareti, ve kanıt: bağlantı, yönlendirme, metin ya da sayfa yolu (derlenmiş `@page`'lerden). Kodda olup hiçbir satırın ve hiçbir muafiyetin sahiplenmediği bağlantı ve yönlendirme `UndocumentedTransition` olarak bildirilir.
- **Muafiyetler** kapalı ve tek tek adlandırılmıştır (dosya + tür + ifade + gerekçe) ve yalnız ekranda kalan ya da ekran olmayan yerlerdir: sayfalama, süzgeç kaldırma, ekran içi PRG, hata hâlinin çıkışları (`04 §3` girişinin kendi kuralı) ve dosya indirme. Başka ekrana götüren her bağlantı satırdır. Koddan kalkan muafiyet `DeadExemption`; hem satır kanıtı hem muafiyet olan site `ExemptAndClaimed` bildirir. **Düzeltme 1:** alıntılı `Spec` muafiyetleri ve açık kalem (`OpenGap`) kanıtı kaldırıldı; bilinen fark listesi yoktur.
- **Menü** (`§3.2`): panel menüsü (`PanelFrameBuilder.MenuOf`) menü tablosuyla giriş giriş, sıra dahil karşılaştırılır (ekran → `PanelFramePaths` adresi).
- **Dönüşler** (`§3.6`): paragraflar envanterde sabitlenir; dönüş kuralları kararı veren işlevlerden (`ReturnPath.AfterSignIn`, `Resolve`, `AfterSignOut`, iki kapının giriş adresleri, ödeme adımının giriş adresi) sınanır.
- **Düzeltme 1 (K-1617, doğrulamadan önce, yönetici kararı):** (a) ekran tanımlarının yazdığı yedi bağlantı grubu `04 §3.3`'e (3.3.40–3.3.45) ve `§3.4`'e (3.4.24, 3.4.25) satır olarak eklendi, `04` §5.25.13'e sonuç hâlinin iki bağlantısı yazıldı, `04` v0.55; (b) `3.3.13`: sepet satırında ürün adı `/urun/{slug}` bağlantısı yapıldı (D-1189'un bağlantı payı); (c) kırılma örnekleri: tabloya kodda olmayan satır, koda tabloda olmayan bağlantı, yeni satırların silinmesi ve bağlantılarının kalkması, eksik hücreli satır.
- **Düzeltme 2 (doğrulama tur 1 B1, K-1617 ek madde):** (a) `Pages/Orders/_DefectClaimScreen.cshtml` ayıp talebi ekranında aydınlatma bağlantısını öteki ekranlarla aynı yoldan çizer (`FramePaths.PrivacyNotice` / `PrivacyNoticeTitle`; `04` 5.19.3 (4), 6.2.19.1, KP-74, `02 §3.33.5`); (b) `AfterSalesScreenTests` E-19 sayfasında bağlantıyı ve hedefini sınar; (c) `04` 3.3.44 "Nereden" sütununa E-19 ve kaynak olarak §5.19.3 (4) eklendi, `04` sürüm satırı ve `PRODUCT_DISCOVERY_STATUS` K-1617 satırı güncellendi; (d) envantere E-19 bağlantısı kanıt olarak girdi, `TheLinksOfTheScreenSpecificationRows_LeavingTheCode_AreReported` teorisine E-19 bağlantısı silinince kıran satır, `PrivacyNoticeRow_WithoutTheDefectClaimScreenInItsSources_IsReported` ile belge tarafında E-19 düşürülünce `FromDiffers` kırılması eklendi. 3.3.40–3.3.45 ve 3.4.24–3.4.25 satırlarının her biri kaynak paragraflarıyla yeniden karşılaştırıldı (5.25.13, 5.15.3, 5.16.8, 5.20.9, 2.12.1.6 + `02 §3.33.5` + 5.19.3, 5.10.4, 9.17.4, 9.9.9): başka eksik bağlantı çıkmadı. K1 (D-1626): form `action`/`formaction` taraması ve `WriteSeeOther` bu dala katılmadı — yirmiden fazla yerde (süzgeç ve arama GET formları, `formaction` düğmeleri, `Coupons` formu, `UnexpectedErrorLayer` yönlendirmesi) her birinin muafiyet ya da `04 §3` satırı kararı gerekir, ~100 satırı aşar; D-1626 F5 kapısında kalır. K2 (D-1627) dokunulmadı.
- Üretim kodu değişikliği yalnız `3.3.13` payıdır (`CartVariantFact.ProductSlug`, `CartLine.ProductSlug`, `Cart.cshtml`'de ad bağlantısı); öteki değişiklik test ve kayıt dosyalarıdır.

## Etkilenen Modüller / Dosyalar

- `tests/Shopfolio.UnitTests/Consistency/NavigationDocument.cs`, `NavigationSites.cs`, `NavigationInventory.cs`, `NavigationCheck.cs`, `NavigationConsistencyTests.cs` (yeni; düzeltme 1'de Inventory, Check, Document, ConsistencyTests değişti)
- Düzeltme 1: `src/Shopfolio.Web/Modules/Catalog/Contracts/CartVariantFact.cs`, `Modules/Catalog/CartVariantReader.cs`, `Modules/Ordering/Contracts/CartContracts.cs`, `Modules/Ordering/Carts.cs`, `Pages/Storefront/Cart.cshtml`; testler `CartRulesTests.cs`, `CartPageModelTests.cs`, `IntegrationTests/Web/CartPageTests.cs`
- Düzeltme 1: `Docs/04_UI_SPECS.md` (v0.55), `Docs/PRODUCT_DISCOVERY_STATUS.md` (K-1617, `04` sürüm satırı), `Docs/DEFERRED_BACKLOG.d/T508.md` (D-1189 payı)
- `Docs/TASK_REPORTS/T546_REPORT.md`, `Docs/STATUS_CHANGELOG.d/T546.md`, `Docs/IMPLEMENTATION_STATUS.md` (T546 satırı)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Müşteri geçişleri (`§3.3`) kodla eşit | ✓ ⚠ | `NavigationMap_EqualsTheCode_InBothDirections`: 45 satır (3.3.1–3.3.45), her biri kaynak ve hedef ekranıyla ve en az bir kanıtla; 3.3.13 artık bağlantıyla kanıtlı (sepet satırında ad bağlantısı, `TheLinesProductName_LinksToTheProductPage_T546_3_3_13`); ⚠ 1: satırın "görseli" yarısı D-1189'da (aşağıda) |
| 2 | Panel geçişleri (`§3.4`) eşit | ✓ | aynı test: 26 satır (3.4.1–3.4.26; 3.4.26 panel boş liste çıkışı, düzeltme 3) |
| 3 | Dış girişler (`§3.5`) eşit | ✓ | aynı test: 15 satır; kanıt derlenmiş `@page` yolları (`Page` kanıtı) ve oturum işleyicisinin yönlendirmeleri |
| 4 | Dönüşler (`§3.6`) eşit | ✓ | aynı test: 3.6.1–3.6.5 paragrafları; `SignInReturn_GoesToTheAskedPage_ExceptTheEntryScreens`, `PanelSignInReturn_...`, `CustomerSignOut_...`, `TheTwoGatesAreSeparate_...`, `CheckoutSignIn_ReturnsToThePaymentStep` |
| 5 | Panel ↔ vitrin (`§3.7`) eşit | ✓ | aynı test: 3.7.1–3.7.5 (Düzenlemeye dön = 3.7.4, oturumsuz taslak adresi = 3.7.5) |
| 6 | İskelet (`§3.1`, `§3.2`) çerçevede ve denetimde | ✓ | 21 + 22 satır; `PanelMenu_EqualsTheMenuTableOfTheDocument_EntryByEntry` |
| 7 | Tabloda olmayan geçiş koda sızmaz; `Consistency` özellikli birim testi | ✓ | `UndocumentedTransition` denetimi; test sınıfı `[Trait("Consistency", "true")]`, `UnitTests/Consistency/` altında; ekran tanımlarının yazdığı bağlantılar artık muafiyet değil satırdır |

### Düzeltme 3 öz denetimi — tur 2 B1 (panel boş liste çıkışı, paylaşılan bileşen kanıtı) kapatıldı; bütün kriterler ve KK'ler yeniden denetlendi

**Düzeltme 3 notu (K-1617 ek madde 4, sahip yerine karar):** panel boş liste çıkışı "Ana sayfaya dön" (E-36, E-38, E-52 → E-31) kodda kalır, belgeye yazıldı: `04 §3.4`'e 3.4.26 satırı, §9.8.13, §9.10.12, §9.23.11 ve §2.7.1.2'ye çıkış cümlesi (satır ekran paragraflarından yazıldı, koddan değil), `04` v0.55 sürüm satırları. Denetim: paylaşılan boş hâl bileşenlerinin (`_EmptyRow`, `_EmptyState`) bağlantısı artık hiçbir satırın kanıtı değildir (`TheSharedEmptyComponents_AreNoEvidenceOfAnyRow`); çıkışı bileşene veren her çağrı (`EmptyStates.WithExit`, `WithFirstRecordButton`, `new EmptyState`; bileşenin üç kurucusu) yeni `EmptyExit` türüyle taranan bir yerdir ve ya bir satırın kanıtıdır — 3.3.12 (altı müşteri çağrısı: E-27, E-12, E-07, E-02, E-03, E-09), 3.4.10 (E-32 "Yeni ürün"), 3.4.26 (üç panel çağrısı) — ya listede kalan çıkıştır (süzgeç/arama kaldırma, ekranın kendi formu: dar muafiyet, dosya + tür + tam ifade). Tüm çağrılar tarandı (`grep` `EmptyStates.With*`, `new EmptyState(`): tabloda karşılığı olmayan başka çıkış çıkmadı; `_NotFound` tek kullanıcılı (E-06; `FramedNotFoundPageRenderer`), paylaşılan değil.

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kriter 1 (`§3.3`) | ✓ ⚠ | `NavigationMap_EqualsTheCode_InBothDirections` 45 satır; 3.3.12'nin kanıtı artık altı müşteri çağrısıdır (bileşen bağlantısı değil); 3.3.13 koşul sapması D-1631 |
| Kriter 2 (`§3.4`) | ✓ | 3.4.1–3.4.26; 3.4.26 ↔ 9.8.13, 9.10.12, 9.23.11 (çıkış cümlesi bu turda eklendi); panel çağrıları `Orders/Requests/AuditTrail.cshtml.cs` `PanelFramePaths.Home` |
| Kriter 3–6 | ✓ | değişmedi; `OutsideEntry_*`, `ReturnTarget_*`, `ReturnRedirect_*`, `PanelStorefrontRow_*`, `BackToEditingLink_*`, `StorefrontSkeletonLink_*`, `PanelSkeletonLink_*`, `PanelMenu_*` geçer |
| Kriter 7 | ✓ | `UndocumentedTransition` `EmptyExit` çağrılarını da kapsar; `[Trait("Consistency","true")]`; dosya yazmaz, DB/ağ yok |
| KK1 | ✓ | `CustomerTransition_*`, `TheRowsOfTheScreenSpecificationLinks_*`, `TheLinksOfTheScreenSpecificationRows_*`; yeni: `PanelEmptyListExitRow_DeletedFromTheTable_OrChangedInIt_IsReported` (3.4.26 silinince/Nereden değişince) |
| KK2 | ✓ | `PanelTransition_*`; yeni: `PanelEmptyListExit_ChangedToAnotherScreen_IsReported` (üç panel sayfası) |
| KK3–KK6 | ✓ | değişmedi |
| KK7 paylaşılan bileşenle eklenen/değişen geçiş kırılıyor mu | ✓ | sentetik: `PanelEmptyListExit_ChangedToAnotherScreen_IsReported` (3), `EmptyListExit_ChangedToAnotherScreen_IsReported_OnTheScreenThatChangedIt` (7: OrderHistory, Cart, FaqPage, CategoryPage, SearchPage, ContentListPages, Products), `EmptyListExit_AddedToAScreen_WithoutARow_IsReported`, `EmptyListExit_OfAnotherKindOfCall_IsAlsoASite` (`WithFirstRecordButton`, `FaqPage`). **Gerçek dosyada deney (geri alındı):** `Panel/Orders.cshtml.cs` çıkışı `PanelFramePaths.Home` → `PanelFramePaths.Members`: `EvidenceMissing:3.4.26` + `UndocumentedTransition ... PanelFramePaths.Members`; `Storefront/Cart.cshtml` `"/"` → `"/hesabim"`: `EvidenceMissing:3.3.12` + `UndocumentedTransition ... "/hesabim"`; `git checkout` ile geri alındı, çalışma ağacında yalnız bu turun dosyaları kaldı |
| Sessiz atlama yok | ✓ | yeni tarama yalnız yorum satırını ve bileşenin tanım dosyasını (`StateModels.cs`) atlar; ikisi de gerekçeli |
| Sık FAIL (eşitlik denetimi paylaşılan bileşen anahtarını tek satıra kanıt sayıyor; belge–kod: satır ekran paragrafından) | ✓ | yukarıdaki not; satır §9.8.13/§9.10.12/§9.23.11/§2.7.1.2 paragraflarından yazıldı |
| Sık FAIL (aynı sayıyı anan öteki yerler) | ✓ | `04` sürüm satırları (başlık sürümü v0.55 korundu, dipnot, `PRODUCT_DISCOVERY_STATUS`), K-1617 satırı (madde 4), 3.4 Kaynak dipnotu; `07`'nin 9.8.13/9.10.12/9.23.11 özet satırları çıkış cümlesini anmıyor, hedef çelişkisi yok |
| Güvenlik taraması | ✓ | yalnız test ve belge; kaynak koda dokunulmadı |
| `09` biçim | ✓ | build `-warnaserror` 0 uyarı; `dotnet format --verify-no-changes --include` 4 `.cs` temiz |
| Test | ✓ | Consistency 397/397 (383 + 14 yeni); Consistency dışı Unit 7420/7420; Integration: sayfa/kaynak değişmedi, TAM: CI |

### Düzeltme 2 öz denetimi — bütün kabul kriterleri ve KK'ler yeniden denetlendi

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kriter 1 (`§3.3`, 3.3.44 dahil) | ✓ ⚠ | `NavigationMap_EqualsTheCode_InBothDirections` 45 satır; 3.3.44 "Nereden" `02 §3.33.5` sayımıyla birebir (E-10, E-13, E-16, E-17, E-18, E-19, E-20, E-22); E-19 kanıtı `_DefectClaimScreen.cshtml` `@FramePaths.PrivacyNotice`; `AfterSalesScreenTests` E-19 sayfasında bağlantı ve hedef (17/17); ⚠ görsel yarısı D-1189 |
| Kriter 2 (`§3.4`) | ✓ | aynı test: 3.4.1–3.4.25; 3.4.24 ↔ 9.17.4, 3.4.25 ↔ 9.9.9 yeniden karşılaştırıldı |
| Kriter 3 (`§3.5`) | ✓ | aynı test: 15 satır, `OutsideEntry_*` |
| Kriter 4 (`§3.6`) | ✓ | 3.6.1–3.6.5, `ReturnTarget_ChangedInTheDocument_IsReported`, `ReturnRedirect_LeavingTheCode_IsReported` |
| Kriter 5 (`§3.7`) | ✓ | 3.7.1–3.7.5, `BackToEditingLink_LeavingTheCode_IsReported` |
| Kriter 6 (`§3.1`, §3.2) | ✓ | 21 + 22 satır; `PanelMenu_EqualsTheMenuTableOfTheDocument_EntryByEntry` |
| Kriter 7 (tabloda olmayan geçiş sızmaz; `Consistency`, DB/ağ yok) | ✓ | `UndocumentedTransition`; `[Trait("Consistency","true")]`; dosya yazmaz |
| KK1 satır silinince / bağlantı kalkınca kırılır | ✓ | `TheRowsOfTheScreenSpecificationLinks_DeletedFromTheTable_AreReported` (3.3.44 dahil), `TheLinksOfTheScreenSpecificationRows_LeavingTheCode_AreReported` (3.3.44 için iki kanıt: `_IbanField` ve `_DefectClaimScreen`), `PrivacyNoticeRow_WithoutTheDefectClaimScreenInItsSources_IsReported` |
| KK2–KK6 | ✓ | düzeltme 1 tablosundaki testler değişmeden geçer (Consistency 383/383) |
| KK7 tabloda olmayan geçiş koda eklenince kırılır | ✓ | `LinkAddedToTheCode_WithoutARow_IsReported`, `RedirectAddedToTheCode_WithoutARow_IsReported`, `PanelLinkAddedToTheCode_WithoutARow_IsReported`; K1 kapsam notu D-1626 |
| Sessiz atlama yok | ✓ | `grep -n "continue\|Skip\|Except\|ignore"` Navigation* dosyalarında: `SkippedFolders` (bin, obj, Migrations), `NavigationSites` satır yorumu ve `void` tanım satırı (çağrı değil), tablonun başlık/ayırıcı satırı; başka atlama yok |
| Sık FAIL (belge–kod eşitliği: belgeye koddan yazılan satır) | ✓ | 3.3.40–3.3.45 ve 3.4.24–3.4.25 kaynak paragraflarıyla satır satır yeniden karşılaştırıldı; E-19 dışında fark yok |
| Sık FAIL (4) aynı sayıyı anan öteki yerler | ✓ | `04` sürüm satırları (başlık, dipnot, `PRODUCT_DISCOVERY_STATUS`), K-1617 satırı; KP-74 ve 6.2.19.1 zaten E-19'u sayıyordu |
| Sık FAIL (5) hedefi bu task olan backlog kalemi | ✓ | D-1626 hedefi "düzeltme 2 ya da F5 kapısı": kapsam ~100 satırı aştığı için F5 kapısında kalır (yukarıdaki dayanak); D-1627 F5 kapısı |
| Güvenlik taraması | ✓ | sabit yol bağlantısı; sır/yetki/girdi etkisi yok; yeni bağımlılık yok |
| `09` biçim | ✓ | `dotnet format --verify-no-changes --include` 13 `.cs` temiz; build `-warnaserror` 0 uyarı |

### Öz denetim (bitiş kapısı 2) — Düzeltme 1'de bütün kriterler ve KK'ler yeniden denetlendi

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| Kriter 1 (`§3.3`) | ✓ ⚠ | `NavigationMap_EqualsTheCode_InBothDirections` 45 satır; 3.3.13 ad bağlantısıyla; ⚠ 1: görsel yarısı D-1189 |
| Kriter 2–7 | ✓ | yukarıdaki tablo; `NavigationConsistencyTests.cs`; yeniden koşuldu: Consistency 381 geçti |
| KK1 tablodan satır silinince ya da bağlantı kalkınca kırılır | ✓ | `CustomerTransition_DeletedFromTheTable_IsReported`, `CustomerTransition_WhoseLinkLeavesTheCode_IsReported`, `CustomerTransition_WhoseTargetChangesInTheTable_IsReported`; düzeltme 1: `TheRowsOfTheScreenSpecificationLinks_DeletedFromTheTable_AreReported` (9 satır), `TheLinksOfTheScreenSpecificationRows_LeavingTheCode_AreReported` (9 bağlantı) |
| KK2 panel geçişi eksilince kırılır | ✓ | `PanelTransition_DeletedFromTheTable_IsReported`, `PanelTransition_WhoseLinkLeavesTheCode_IsReported` |
| KK3 dış giriş eksilince kırılır | ✓ | `OutsideEntry_DeletedFromTheTable_IsReported`, `OutsideEntry_WhosePageLeavesTheCode_IsReported` |
| KK4 dönüş hedefi değişince kırılır | ✓ | `ReturnTarget_ChangedInTheDocument_IsReported`, `ReturnRedirect_LeavingTheCode_IsReported` |
| KK5 `§3.7`'nin beş satırından biri silinince ya da bağlantı kalkınca kırılır | ✓ | `PanelStorefrontRow_DeletedFromTheTable_IsReported` (3.7.3, 3.7.5), `BackToEditingLink_LeavingTheCode_IsReported` (3.7.4), `BackToThePanelLink_LeavingTheCode_IsReported` (3.7.3) |
| KK6 iskelet bağlantısı eksilince kırılır | ✓ | `StorefrontSkeletonLink_LeavingTheCode_IsReported`, `PanelSkeletonLink_LeavingTheCode_IsReported`, `PanelMenu_WithARenamedEntryInTheDocument_IsReported`, `PanelMenu_WithAnEntryPointingToAnotherScreen_IsReported` |
| KK7 koda tabloda olmayan geçiş eklenince kırılır; testler `UnitTests/Consistency/` altında | ✓ | `LinkAddedToTheCode_WithoutARow_IsReported`, `RedirectAddedToTheCode_WithoutARow_IsReported`, `PanelLinkAddedToTheCode_WithoutARow_IsReported`, `LinkAddedToAScreenThatHadExemptLinks_WithoutARow_IsReported`, `ExemptSite_LeavingTheCode_IsReported` |
| Düzeltme 1 (d): tabloya kodda olmayan satır eklenince kırılır (`09 §19.2` m.3) | ✓ | `RowAddedToTheTable_WithoutCode_IsReported` (`RowOnlyInDocument`), `RowWithTooFewCells_IsNotPassedOver_ItBreaksTheReader` |
| Düzeltme 1: muafiyet ve açık kalem mekanizması yok | ✓ | `TheInventory_HasNoOpenItemAndNoCitedExemption`; `OpenGap`, `Spec`, `CiteId`, `GapNotOpen`, `ExemptionNotCited` koddan kalktı; kalan muafiyetler yalnız `Stay` (ekranda kalan işlem, ekran olmayan dosya indirme) |
| `09 §19.2` m.2: eşitlik denetimi veritabanına, ağa dokunmaz, dosya yazmaz | ✓ | yalnız `File.ReadAllText` ve `Directory.Enumerate*`; değişen metin bellekteki kopyadır; denetim backlog dosyalarını artık okumaz |
| `09 §19.2` m.3: kırıldığı gösterilir | ✓ | 21 + düzeltme 1'in 21 olumsuz testi (iki teori dahil), değiştirilmiş belge ya da kaynak kopyasıyla |
| `09 §19.2` m.7: adlandırma | ✓ | `<Sınanan>Tests`, `Yöntem_Durum_Beklenen`; dosya sınıf adını taşır (yardımcı sınıflar kendi dosyasında) |
| `09 §25`: diff yalnız görevi içerir | ✓ | üretim kodunda yalnız 3.3.13 payı (ürün adresi sepet satırında); öteki değişiklik test ve kayıt |
| Güvenlik taraması | ✓ | sır, yetki, girdi doğrulama etkisi yok; yeni bağımlılık yok; bağlantı `/urun/{slug}` kayıtlı slug'dan kurulur (Razor kaçışlı) |
| Sık FAIL (1) istisna geniş | ✓ | muafiyetler dosya + tür + tam ifade; joker yok; ölü ve çift sahipli muafiyet testle yakalanır; bilinen fark listesi yok |
| Sık FAIL (2) çağrı biçimi kaçıyor | ✓ | tarama: Razor `<a ... href="...">` (ek nitelik, `@(...)` ifadesi, içinde tırnak), `FormResults.SeeOther` (adlı ya da çok satırlı argüman, satır yorumu), `SeeOtherToGoogle`, `Redirect(`, `RedirectToPage`, `LocalRedirect`, `RedirectPermanent`, `RedirectToRoute`, `RedirectToAction`; Razor açıklamaları ve `///` satırları atlanır |
| Sık FAIL (3) 06 varsayılan/FK | — | veri modeline dokunulmadı |
| Sık FAIL (4) aynı sayıyı anan öteki yerler | ✓ | satır sayısı artık testte yazılı değil (satırlar tablodan okunur, her tablonun numarası boşluksuz sınanır); `04` ekran metni (5.25.13) ve sürüm satırları (başlık, dipnot, `PRODUCT_DISCOVERY_STATUS` tablosu) güncellendi |
| Sık FAIL (5) belge metni karar kaydısız | ✓ | `04 §3` satırları K-1617 ile kayıtlı (`PRODUCT_DISCOVERY_STATUS.md`) |
| Backlog: hedefi T546 olan kalem | ✓ | `grep -nE "T546" Docs/DEFERRED_BACKLOG.md Docs/DEFERRED_BACKLOG.d/*.md` yalnız D-1189'daki "bağlantı payı T546'da yapıldı" notunu verir; yeni kalem açılmadı |
| T538 emsali: sessiz atlama yok | ✓ | `grep -n "continue\|Skip\|Except\|ignore"` Navigation* dosyalarında: `SkippedFolders` (bin, obj, Migrations — görünüm yok), yorum ve `void` tanım satırları (çağrı değil), tablonun başlık ve ayırıcı satırı; eksik hücreli satır artık atlanmaz, istisna fırlatır; öteki çıkış yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit (Consistency süzgeci) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj --filter-trait "Consistency=true"` | 381 geçti, 0 kaldı (düzeltme 1: 361 → 381) |
| Unit (Consistency dışı) | ✓ | `... --filter-not-trait "Consistency=true"` | 7420 geçti, 0 kaldı |
| Integration (sepet sayfası değişti) | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests/... --filter-class` (`CartPageTests`, `CartTests`, `CartQuantityTests`, `MainFlowTests`, `PageFormPatternTests`, `SecurityHeadersTests`) | 106 geçti, 0 kaldı (yeni `TheLinesProductName_LinksToTheProductPage_T546_3_3_13` dahil); TAM: CI |
| Biçim | ✓ | `dotnet format Shopfolio.slnx --no-restore --include <değişen .cs>` | değişiklik yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 3, 2026-11-15, dal ucu `c0d65410`; CI run `43708482759` Integration dışı işler yeşil, Integration koşuyordu) — eskalasyon (`12 §8.5`) |
| Bulgu sayısı | 1 (S1, ORTA) |
| Düzeltme gerekli mi | Evet — B1 (tur 3): bağlantısı bir modelin kurduğu listeden gelen satırlarda kanıt yalnız görünümdeki ortak ifadedir (`@item.Href`, `@warning.Href`, `@path` …); listedeki hedef karşılaştırılmaz. Gerçek dosyada deney (geri alındı): `Panel/Index.cshtml.cs` kurulum listesinin "iade adresi" maddesi `ShippingAndDefaults` → `Brand` (3.4.4'ün E-46 hedefi gitti) ve "e-posta ulaşmadı" uyarısı süzülmüş E-36 → `Members` (3.4.3) — `NavigationConsistencyTests` 84/84 geçti; `_AccountNav.cshtml`'den Adres defteri girdisi silindi (3.3.32'nin E-26 hedefi gitti) — 84/84 geçti. KK1 ve KK2'nin "geçiş eksilince test kırılır" maddesi bu satırlarda karşılanmıyor; 3.3.22–3.3.24 ve menü (3.2, 3.1.12–3.1.16) için yazılan ayırt edici kanıt 3.3.32, 3.4.2, 3.4.3, 3.4.4, 3.4.15, 3.4.22'de yok. Düzeltme: liste kuran her satırda hedefleri kanıtlamak (menüdeki gibi listeyi belgeyle karşılaştırmak ya da her hedefe metin kanıtı) ve kırılma testi. Tur 2 B1 (3.4.26, `EmptyExit`) kapandı — gerçek dosyada hedef değişimi denetimi kırıyor. |
| Bloke-etmeyen not (K) sayısı — tur 3 | 1 — K1 → D-1636 (`UnexpectedErrorSetup` muafiyetinin gerekçesi yanlış: 2.7.3.3'ün yönlendirmesi başka ekrana gider) |

**Doğrulayıcı kabul kriterleri (tur 3):** 1 ✗ (B1: 3.3.32) · 2 ✗ (B1: 3.4.3, 3.4.4) · 3 ✓ · 4 ✓ · 5 ✓ · 6 ✓ · 7 ✓ (eklemelerin kapsam sınırı D-1626'da). Yerel: build `-warnaserror` 0 uyarı; `dotnet format --include` 13 `.cs` temiz; Consistency 397/397; Unit (Consistency dışı) 7420/7420; betik 4/4; Integration `CartPageTests`, `AfterSalesScreenTests`, `CartTests`, `CartQuantityTests`, `MainFlowTests`, `StorefrontFrameTests` 124/124, TAM: CI. `12`'de T546'ya düşen satır yok.

Tur 2 kaydı:

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 2, 2026-11-15, dal ucu `07682894`; CI run `43703091010` yeşil) — ikinci iterasyon FAIL'i: eskalasyon (`12 §8.5`) |
| Bulgu sayısı | 1 (S1, ORTA) |
| Düzeltme gerekli mi | Evet — B1 (tur 2): panelin boş liste satırının "Ana sayfaya dön" çıkışı E-36, E-38, E-52 → E-31 (`Panel/Orders.cshtml.cs`, `Requests.cshtml.cs`, `AuditTrail.cshtml.cs`) kodda var, `04 §3.4`'te satırı yok; denetim `_EmptyRow.cshtml` `@Model.ExitPath`'i müşteri satırı 3.3.12'nin kanıtı saydığı için yakalamaz (E-36 çıkışı E-40'a çevrildiğinde `NavigationConsistencyTests` 70/70 geçti). Düzeltme: çıkış ya `04 §3.4` satırı olur (karar kaydıyla) ya kalkar; `EmptyStates.WithExit`/`WithFirstRecordButton` çağrıları çağrı yerinden kendi satırına/muafiyetine bağlanır; kırılma testi. Tur 1 B1 (3.3.44 / E-19) kapandı. |
| Bloke-etmeyen not (K) sayısı — tur 2 | 2 — K1 → D-1631 (3.3.13 "Ürün Yayında" koşulu: vitrinden kalkmış kalemde de bağlantı), K2 → D-1632 (x.1 "Aktör · giriş · çıkış" paragrafları K-1617 satırlarını anmıyor) |

**Doğrulayıcı kabul kriterleri (tur 2):** 1 ✓ (3.3.13 ad bağlantısıyla karşılanmış; görsel D-1189) · 2 ✗ (B1) · 3 ✓ · 4 ✓ · 5 ✓ · 6 ✓ · 7 ✗ (B1: tabloda olmayan geçiş paylaşılan bileşen üzerinden koda girmiş). Yerel: build `-warnaserror` 0 uyarı; `dotnet format --include` 13 `.cs` temiz; Consistency 383/383; Unit (Consistency dışı) 7420/7420; betik 4/4; Integration `CartPageTests`, `AfterSalesScreenTests`, `CartTests`, `CartQuantityTests` 86/86, TAM: CI. `12`'de T546'ya düşen satır yok.

Tur 1 kaydı:

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 1, 2026-11-15, dal ucu `a8cb7217`; CI run `43695678189` yeşil) |
| Bulgu sayısı | 1 (S1, ORTA) |
| Düzeltme gerekli mi | Evet — B1: `04` 3.3.44 satırı E-19'u (ayıp talebi ekranı) atlıyor; `02 §3.33.5`, `04` 5.19.3 (4), 6.2.19.1 ve KP-74 eşlemesi E-19'da aydınlatma bağlantısını sayar; kodda `_DefectClaimScreen.cshtml` bu bağlantıyı çizmez. Satır koda göre daraltıldığı için denetim farkı gizliyor. Düzeltme: 3.3.44'ün "Nereden"ine E-19 eklenir, E-19'a aydınlatma bağlantısı çizilir (sayfa testiyle), envanter kanıtı ve kırılma testi eklenir. |
| Bloke-etmeyen not (K) sayısı | 2 — K1 → D-1626 (taramanın görmediği form `action`/`formaction`, `WriteSeeOther`, paylaşılan anahtarlar), K2 → D-1627 (`LinkKeySourceScanTests` yük altında regex zaman aşımı) |

**Doğrulayıcı kabul kriterleri:** 1 ✗ (3.3.44 / E-19, B1) · 2 ✓ · 3 ✓ · 4 ✓ · 5 ✓ · 6 ✓ · 7 ✓ (kapsam notu K1). Yerel: build `-warnaserror` 0 uyarı; `dotnet format --include` değişen 12 `.cs` temiz; Consistency 381/381; Unit (Consistency dışı) 7419/7420 — tek kalan `LinkKeySourceScanTests` regex zaman aşımı, sınıf tek başına 16/16 (K2); betik testleri 4/4; Integration `CartPageTests`, `CartTests`, `CartQuantityTests`, `MainFlowTests` 70/70, TAM: CI. `12`'de T546'ya düşen satır yok.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T546-gezinme-gecis-esitlik`
- **Commit:** `67237e82` — T546: Gezinme ve geçiş eşitlik denetimleri: iskeletler, geçişler, dışarıdan girişler, dönüşler
- **PR:** #580
- **Düzeltme 2:** `T546-fix2` dalından aynı dala itildi (doğrulama tur 1 B1; Unit Consistency 383/383, Unit diğer 7420/7420, `AfterSalesScreenTests` 17/17; Integration TAM: CI)
- **Düzeltme 1:** `T546-fix1` dalından aynı dala itildi (düzeltme commit'i dal ucudur; K-1617)
- **CI:** dal CI'ını yönetici izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok. Denetim yalnız repo dosyalarını (`Docs/04_UI_SPECS.md`, `Docs/DEFERRED_BACKLOG*`, `src/Shopfolio.Web`) ve derlenmiş Razor sayfalarını okur; plan katmanı, kota, dış servis ya da paket sürümü gerektirmez.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `04 §3.3.13`: E-12 sepet kaleminin **görseli** E-04'e bağlanmıyor (satır "adı ya da görseli" der; ad bağlantısı bu dalda yapıldı) | D-1189 (açık, hedef F5 kapısı; kalemi "bağlantı payı T546'da yapıldı" diye güncellendi) | Görsel, Catalog'un sepet okumasından görsel kimliği ve çizim ister (yönetici kararı b: yalnız ürün sayfası bağlantısı payı bu dalda); denetimde açık kalem mekanizması yoktur, satır adın bağlantısıyla kanıtlıdır |

## Notlar

- **Working tree (Adım -1):** temiz (`git status --short` boş); `core.hooksPath` = `scripts/git-hooks`.
- **Ana dal CI startup (Adım 0):** `43691824226` success, `43691824223` success, `43666144462` success.
- **Repo memory (bitiş kapısı 9):** dokunulmadı.
- **⚠ 1 — 3.3.13 görsel yarısı:** sepet satırında ürün adı E-04'e bağlandı (bağlantı payı ~20 satır, sözleşme değişikliği küçük: `CartVariantFact.ProductSlug`, `CartLine.ProductSlug`); satırın "görseli" yarısı D-1189'da kalır (karar b). Denetimde açık kalem kalmadı.
- **⚠ 2 — kapandı (düzeltme 1, K-1617):** ekran tanımlarının yazdığı yedi bağlantı grubu `04 §3`'e satır oldu (3.3.40–3.3.45, 3.4.24, 3.4.25); muafiyet listesi kaldırıldı, denetim bu satırları da tablodan okur.
- **⚠ 3 — Kapsam sınırı (karar c):** tarama `src/Shopfolio.Web` içindeki bağlantı ve yönlendirme çağrılarını görür; C# dizgesi olarak yazılmış HTML (hata ve oran sınırı sayfalarının sabit yanıtları) ekran değildir, çerçevesiz sabit yanıtlardır (`04 §2.7.3.2`): denetime girmez, dokunulmadı.
- **⚠ 4 — Satır kanıtı paylaşımı:** bazı satırlar aynı bağlantıyı paylaşır (ör. 3.3.22–3.3.24 `_OrderPage` `@link.Href`; menü satırları 3.1.12–3.1.16 `@item.Path`; panel liste satırları 3.4.6, 3.4.8, 3.4.10 `_PanelTable` `@row.RecordPath`). Paylaşılan bağlantıya ek olarak her biri için ayırt edici metin kanıtı yazıldı (sipariş sayfasındaki adres son ekleri; menü adı sabitleri); panel liste satırlarında (3.4.6, 3.4.8, 3.4.10) listenin kendi satır adresini kuran satır da metin kanıtı olarak yazılıdır.
