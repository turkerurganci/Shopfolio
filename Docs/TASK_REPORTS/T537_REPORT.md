# T537 — Vitrin çerçevesinin modeli: marka, menü, duyuru şeridi, hesap alanı, sepet sayısı

**Faz:** F5 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-11

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- `StorefrontFrame` görünüm modeli (`07 §5.5.1`) ve parçaları: `FrameBrand`, `FrameMenu`, `FrameSearch`, `FrameAccount`, `FrameSignOutForm`, `FrameCart`, `FrameContact`, `FrameFooter`, `FrameLegalLink`, `FrameSales`, `FrameAdminStrip`.
- `StorefrontFrameBuilder`: çerçeveyi isteğin bağlamından üretir (marka, menü, duyuru, hesap, sepet sayısı, bağlantılar, altbilgi, kapılar, yönetici şeridi, birleşme mesajı); ödeme adımında (`/odeme`) menü, arama ve duyuru yoktur.
- `StorefrontFrameFilter` (`IAsyncPageFilter`, sıra 100): yalnız sayfa çizen yanıtta (`PageResult`/sonuçsuz) ve `/Panel` dışında çerçeveyi isteğe koyar; yönlendirme yanıtı çerçeve kurmaz ve birleşme işaretini okumaz. Sayfa modeli çerçeveyi eklemez (`StorefrontFrameAccess.GetStorefrontFrame(HttpContext)`).
- Modüllerin yeni okuma uçları: `ICorporateContentModule.ListSocialLinksAsync`, `ISettingsModule.GetWhatsAppNumberAsync`, `IMembershipModule.GetMemberNameAsync`, `IOrderingModule.ReadCartBadgeAsync` ve `TakeMergeNoticeAsync` (modül işlemi çağıranın işlemine katılır, kaydetmez).
- Satış kapısı ve veri toplayan giriş kapısı her istekte `ISettingsModule.GetSalesGateAsync`'ten okunur; çerçevede saklanmaz, önbelleğe alınmaz.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Pages/Frame/*` (yeni), `Program.cs` (`AddStorefrontFrame`).
- CorporateContent, Settings, Membership, Ordering modüllerinin arayüz ve uygulama dosyaları (yalnız okuma uçları).
- Testler: `tests/Shopfolio.IntegrationTests/Web/StorefrontFrameTests.cs`, `Pages/FrameSamples.cs`, `Pages/Storefront/FrameSample.cshtml`, `FrameCheckoutSample.cshtml`; `tests/Shopfolio.UnitTests/Pages/StorefrontFrameModelTests.cs`; üç yerde `ISettingsModule` sahtesine yeni yöntem.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Çerçeve her sayfa yanıtına girer, sayfa eklemez | ✓ | `StorefrontFrameFilter`; `APageThatDrawsItself_GetsTheFrame_…_KK1_KK2_KK6` (`FrameSampleModel` çerçeve taşımaz) |
| 2 | Marka ve menü; logo, ad, alan adı geri düşüşü | ✓ | `StorefrontFrameBuilder` (`GetBrandIdentityAsync`, `GetMenuAsync`); aynı test (logo yok, ad yok → alan adı, metin) |
| 3 | Duyuru şeridi; taslak yalnız yönetici tarayıcısında "Taslak" etiketiyle | ✓ | `ADraftAnnouncement_ComesWithItsMarkToTheAdministratorOnly_…_KK3` |
| 4 | Hesap alanı üç hâl; yönetici oturumu oturum sayılmaz | ✓ | `TheAccountArea_HasThreeStates_AndAnAdministratorIsNoMember_KK4`, `AnExpiredSession_IsAVisitor_KK4` |
| 5 | Sepet kalem sayısı o tarayıcının sepeti; sosyal bağlantılar, WhatsApp | ✓ | `TheCartCount_IsThisBrowsersCart_…_KK5`; `TheCartBadge_CountsLiveLinesOnly_…_KK5` |
| 6 | Altbilgi: kimlik seti, ETBİS, genel sayfalar, yayındaki yasal metinler, platform imzası | ✓ | `TheFooter_CarriesTheEtbisBand…_KK6`; `ThePlatformSignature_FollowsTheFirmsSwitch_…_KK6` |
| 7 | Satış kapalı hâli ve giriş kapısı her istekte hesaplanır, saklanmaz | ✓ | `TheGates_AreComputedInEveryRequest_…_KK7` (firmanın anahtarı iki kez çevrilir, aynı uygulamada sonraki istek yeni değeri okur) |
| 8 | Ödeme adımında menü, arama, duyuru yok; altbilgi eksiksiz | ✓ | `ThePaymentStep_HasNoMenuNoSearchAndNoAnnouncement_…_KK8`; `StorefrontFrameModelTests.IsCheckoutStep_…` |
| 9 | Birleşme mesajı bir kez; çıkış, giriş, arama ayrı uçlar, model form ve bağlantı taşır | ✓ | `TheMergeMessage_IsToldOnce…_KK9`; `FrameSignOutForm`, `FrameSearch`, `FrameAccount.SignInPath` |

### Öz denetim (bitiş kapısı 2)

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 sayfa modeli çerçeveyi eklemez | ✓ | `…_KK1_KK2_KK6`; `FrameSampleModel` |
| KK2 geri düşüş zinciri | ✓ | aynı test; `FrameBrand.ShowsLogo` birim testi |
| KK3 taslak duyuru yalnız yöneticide | ✓ | `…_KK3` |
| KK4 üç hesap hâli | ✓ | `…_KK4` |
| KK5 sepet sayısı; bağlantılar | ✓ | `…_KK5` iki test |
| KK6 altbilgi alanları | ✓ | `…_KK6` iki test |
| KK7 kapı saklanmaz | ✓ | `…_KK7` |
| KK8 sadeleşmiş model | ✓ | `…_KK8` |
| KK9 bir kez mesaj | ✓ | `…_KK9` (yönlendirme işareti silmez; ikinci istek boş) |
| `07 §5.5.1` alan adları model ile birebir | ✓ | marka (4 alan + yedekler), menü, duyuru, hesap, sepet, bağlantılar, altbilgi, kapılar, yönetici şeridi, birleşme mesajı; `PanelFrame` kapsam dışı (T540) |
| Kaynak taraması / mimari kuralları (çerez, ad alanı, sayfa-bağlam) | ✓ | UnitTests tam koşu 6598 + yeni testler yeşil (Mimari, Consistency dahil) |
| Veri şeması değişikliği (06 varsayılan, FK dizini) | ✓ yok | göç yok |
| Kural değişince aynı sayıyı anan yerler | ✓ | belgede modül yöntem listesi/sayacı yok (grep); 09 değişmedi |
| Devredilen backlog kalemleri | ✓ | aşağıda tablo |
| `dotnet format --verify-no-changes` | ✓ | Web, UnitTests, IntegrationTests temiz |
| Güvenlik taraması | ✓ | aşağıda |

**Güvenlik taraması.** Çerçeve yalnız okur; yazma yolu yok (birleşme işareti silmesi tek işlemde, çağıranın işleminde). Hesap alanı yalnız müşteri şemasını sayar (yönetici oturumu üye gibi görünmez). Yönetici şeridi ve taslak duyuru yalnız yönetici oturumunda. Sepet çerezi uzunluk sınırıyla okunur, ham değer kaydedilmez. Altbilgideki kimlik seti firmanın kamusal bilgisidir; kişisel veri yok. Yeni dış bağımlılık yok.

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit (tam, Consistency dahil) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests` | 6598 geçti, 0 kaldı; sonra `StorefrontFrameModelTests` 9 geçti |
| Integration (tam, parçalı) | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests` (ad alanı parçaları) | Web+Pages+Security 645, Model 1407, Catalog 82, kalan 1452; hepsi geçti (+1 yeni test sonradan Web'de 11/11) |
| Format | ✓ | `dotnet format <proje> --no-restore --verify-no-changes` | 3 proje temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T537-vitrin-cercevesi`
- **Commit:** `3b1ba2ad` — T537: Vitrin çerçevesinin modeli (PR head son commit)
- **PR:** #532
- **CI:** yardımcı yürütür
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok. Task yalnız uygulama içi modeli ve okuma uçlarını kurar; plan katmanı, kota, dış servis, paket sürümü ya da ortam sırrı gerekmez (4a taraması: plan metninde tetikleyici terim yok). Test Postgres'i CI ile aynı (`postgres:18`, `track_commit_timestamp=on`).

## Devredilen backlog kalemleri

| Kalem | Karar |
|---|---|
| D-529 (kategori ağacı önbelleği) | Menü modeli ağacı `GetMenuAsync`'ten her istekte okur; önbellek eklenmedi, bu yüzden üç yola `CacheArea.CategoryTree` bildirimi gerekmez. Ölçüm T601 KK11'e bırakıldı; satır yerinde güncellendi. |
| D-678 (duyuru önbelleği) | Aynı: duyuru her istekte okunur, önbellek yok; ölçüm T601 KK11. Satır yerinde güncellendi. |
| D-116 (`@Assets` adresi, çerçeve sayfası) | Çerçeve sayfasını (`_Layout`) T540 kurar; hedef T540'a alındı. |
| D-138 (çerez süzgeci hata yolu) | Çerçeve modelinin işi değil; hedef "T540 ya da en geç F5 gate" yapıldı. |
| D-135 (çerçeveli hata sayfası) | Hedefte zaten T540; T537 yalnız model, değişmedi. |
| D-315, D-500, D-502, D-671, D-672, D-674, D-676, D-677 | Vitrin sayfaları (T505) ve panel/taslak bandı (T540) işi; T537 yalnız model. D-676'daki "Taslak" bandı biçimi T540. D-677: T537'nin eklediği okuma uçları çağıranla birlikte eklendi (çağıranı olmayan yöntem yok). Satırlar değişmedi. |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Yönetici şeridi adı taşımaz (yalnız panel yolu) | D-1125 | `07 §5.5.1` yönetici adını saymaz |
| 2 | WhatsApp yalnız numara; bağlantı türetmesi çizimde | D-1126 | Çizim T540 |
| 3 | Ödeme adımı tespiti yalnız tam yol `/odeme` | D-1127 | Alt yollar (örn. dönüş) tam çerçeve alır; sipariş sayfaları kendi task'ında |
| 4 | `PanelFrame` modeli | D-1128 | Bu task yalnız vitrin; T540 |

## Notlar

- **Working tree (Adım -1):** temiz; hook yolu `scripts/git-hooks`.
- **Ana dal CI startup (Adım 0):** `main` son üç run `success` (42935348461, 42935348492, 42924454187).
- **Repo memory:** dokunulmadı.
- **⚠ kolay kararlar:** yönetici şeridi modeli yalnız panel yolu taşır; WhatsApp yalnız numara; `GetLegalTextsAsync` `LastVersionNumber != null` yayında sayıldı; KK7 testi kapıyı firmanın geçici kapatma anahtarıyla sınar (yasal metin sürümleri kalıcı olduğundan test onları geri alamaz; identity eksikken veri toplama kapısı zaten kapalıdır).
- Önceki işçi bağlam sınırında bölündü; bu tur: Integration derlemesi, test düzeltmeleri, modül ve birim testleri, tam koşular, format, kayıtlar.
