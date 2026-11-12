# T521 — Panel ana sayfası ve yönetici hesabının doğrulama ekranları (E-31, E-54)

**Faz:** F5 | **Durum:** Yapım tamam, doğrulama bekliyor | **Tarih:** 2026-11-12

---

## Yapılan İşler

- Panel ana sayfası (`Pages/Panel/Index.cshtml(.cs)`, `PanelHomeModel`, `PanelHomeTexts`): uyarılar, kurulum listesi, altı sayaç, "Müşteriden beklenenler" (IBAN bekleniyor, ilk on satır); boş bölüm çizilmez; sayfada form ve yazma yok.
- Ana sayfanın kendi okuyucuları: `IPanelHomeDetails` / `PanelHomeDetailsService` (Administration), `IAfterSalesModule.ListAwaitingIbanLinesAsync`, `IPaymentModule.CountCardRefundProblemsAsync`, `AfterSalesPeriodRules.RefundLastDay`.
- E-54 sayfaları: `Verify` (`/panel/dogrulama/{anahtar}`), `Revert` (`/panel/geri-al/{anahtar}`), `Reauth` (`/panel/yeniden-dogrulama`), `LandingNextStep`; metinler `PanelAccessTexts`'te, adresler `PanelFramePaths`'te. Modül tarafı: `AdminEmailChanges.VerifyNewEmailAsync` / `RevertPendingEmailChangeAsync`, `IAdministrationModule`, `ReauthenticationValues`, `SessionStore`.
- Bu turda (devam 2): eksik testler (ana sayfa uyarıları, IBAN satırları, iki yeni modül okuyucusu), mimari tarama düzeltmeleri (tüketici yöntem listesi, `ReauthenticateInput` alan işareti, `!` kullanımı), kayıtlar.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Pages/Panel/{Index,Verify,Revert,Reauth}.cshtml(.cs)`, `LandingNextStep.cs`, `PanelAccessTexts.cs`, `Pages/Frame/PanelFrame.cs`
- `src/Shopfolio.Web/Modules/Administration/*` (AdminEmailChanges, PanelHomeDetailsService, sözleşmeler), `Modules/AfterSales/*`, `Modules/Payment/*`, `Infrastructure/Security/{ReauthenticationValues,SessionStore}.cs`
- Testler: `tests/Shopfolio.IntegrationTests/Web/PanelHomeAndVerificationPagesTests.cs` (15), `Administration/PanelHomeTests.cs` (+2), `tests/Shopfolio.UnitTests/Administration/PanelHomeDetailsTests.cs` (6), `Fakes/StubModule.cs`, `Architecture/LinkKeySourceScan.cs`
- Kayıt: `Docs/STATUS_CHANGELOG.d/T521.md`, `Docs/DEFERRED_BACKLOG.d/T521.md`, `IMPLEMENTATION_STATUS.md` tablo satırı, `DEFERRED_BACKLOG.md` (D-968, D-975 kapandı)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | `GET /panel` uyarı, kurulum listesi, altı sayaç, beklenenler; boş bölüm görünmez | ✓ | `TheHome_DrawsTheSixCounters_ZeroIncluded_AndLeavesAnEmptySectionOut_T521_KK1`, `ACountOfZero_OpensNoWarning_AndACountAboveZero_OpensItsOwnLine_T521_KK1` |
| 2 | İstek anında hesap, sayaç saklanmaz; eksik madde satış kapısından, kendi ayar ekranına bağlı | ✓ | `ACounter_ChangesWhenTheRecordsChange_AndIsNotStored_T521_KK2`; `PanelHomeDetails` tabloya yazmaz; `PanelHomeModel.WarningPathOf`, `PanelHomeTexts.ChecklistPath` |
| 3 | Sayaçlar süzgeçle listeye bağlı; IBAN bekleniyor ilk on, kalan süreye göre | ✓ | `TheIbanLines_ShowTheTenWithTheLeastTimeLeftFirst_AndTheRestAreOnTheLink_T521_KK3`, `ALineWithoutALastDay_HasNoPeriod_…_KK3`, `TheAwaitingIbanLines_AreTheOpenRequests_…_KK3` |
| 4 | Yeniden doğrulama: işlem adı, şifre alanı, Google yok; yok/dışı 303 `/panel/hesabim`; açılış önceki izni düşürür | ✓ | `TheReauthenticationPage_Redirects_…_KK4`, `TheReauthenticationPage_NamesTheOperation_AsksThePassword_HasNoGoogle_AndDropsTheEarlierPermission_T521_KK4` |
| 5 | `Reauthenticate` doğruda tek kullanımlık değer; liste dışı 303; yanlış 422 + L-1; 429 yapmaz | ✓ | `TheRightPassword_GrantsOneSingleUseValue_…_KK5`, `AWrongOrEmptyPassword_Is422_CountsForTheLimit_AndAtTheLimit429DoesNothing_T521_KK5` |
| 6 | İniş ekranları çerçevesiz, oturumdan bağımsız; hâl kayıttan, kod yalnız kullanılmış bağlantıdan | ✓ | `TheVerificationLanding_HasThreeStates_…_KK6`, `TheRevertLanding_HasThreeStates_…_KK6` |
| 7 | `VerifyEmail` tek koşullu tüketir, e-posta yeni adres, eskiye bildirim + geri alma bağlantısı, iz, oturum açmaz | ✓ | `TheVerificationButton_ConsumesTheLinkOnce_ChangesTheAddress_NotifiesTheOldOne_AndWritesTheTrail_T521_KK7` |
| 8 | Arada geçersizleşmişse 200 yazmaz; yeni adres başkasının olduysa 422, tüketilmez | ✓ | `TheVerificationButton_Is422_WhenTheNewAddressBecameAnotherAdministrators_AndTheLinkIsNotConsumed_T521_KK8`; 200 hâli: `TheVerificationLanding_…_KK6` |
| 9 | `RevertPendingEmailChange` e-postayı döndürür, oturumları siler, şifre ve işaretleri geçersiz kılar, sıfırlama bağlantısı, iz | ✓ | `TheRevertButton_ReturnsTheAddress_EndsEverySession_InvalidatesThePassword_SendsTheResetLink_AndWritesTheTrail_T521_KK9` |
| 10 | Eski adres başkasının olduysa 422, tüketim geri döner; arada geçersizleşmişse 200 | ✓ | `TheRevertButton_Is422_WhenTheEarlierAddressBecameAnotherAdministrators_AndTheConsumptionRollsBack_T521_KK10` |

## Doğrulama Kontrol Listesi (KK) — Öz Denetim

| KK | Madde | Sonuç | Kanıt |
|---|---|---|---|
| KK1 | Boş bölüm gizli | ✓ | `…_AndLeavesAnEmptySectionOut_T521_KK1` (beklenenler, e-posta/kart uyarıları yok), `ACountOfZero_OpensNoWarning_…_KK1` |
| KK2 | Durum değişince sayaç değişir | ✓ | `ACounter_ChangesWhenTheRecordsChange_…_KK2`, `TheCardRefundProblems_CountOrdersWith…_T521_KK2`, `TheMarkedOrders_AreCountedPageByPage_…_KK2` |
| KK3 | Sayaç süzülmüş listeye, "açık talep" E-38'e; ilk on satır | ✓ | `href="/panel/siparisler?pendingWork=AwaitingPayment"` (KK2 testi), `PanelHomeRulesTests` (liste hedefleri, T382); ilk on: `…ShowTheTenWithTheLeastTimeLeftFirst…_KK3` |
| KK4 | İki işlem; yok/dışı 303 | ✓ | `…_KK4` iki test |
| KK5 | Yanlış şifre L-1; tek izin | ✓ | `…_KK5` iki test |
| KK6 | Üç iniş hâli, kod koşulu | ✓ | `…_KK6` iki test |
| KK7 | Tek tüketim; bildirim, bağlantı, iz | ✓ | `…_KK7` |
| KK8 | Yarışta 200; başkasına geçen adres | ✓ | `…_KK8`, KK6 testleri |
| KK9 | Oturumlar, şifre, e-posta, iz | ✓ | `…_KK9` |
| KK10 | Çakışmada geri dönüş; yarışta 200 | ✓ | `…_KK10` |
| 09 §25 | İlgili satırlar | ✓ | Tüketici yöntem listesi (`LinkKeySourceScan.ConsumingMethods`/`WrapperMethods`) ve `ModelBindingRuleTests` yeşil; modül yöntemleri kendi işlemini açmaz; sayfa `[NoStore]` |
| Güvenlik taraması | Geri al/doğrula yalnız `OnPost…` işleyicisinden tüketir; GET hiçbir şey yazmaz | ✓ | `LinkKeySourceScanTests` (Architecture 1696/1696) |

### Sık FAIL nedenleri

| # | Neden | Sonuç |
|---|---|---|
| 1 | Bekçi/tarama istisnası geniş | ✓ Tüketici listesine yalnız `AdminEmailChanges.cs` içindeki `RevertPendingEmailChangeAsync` eklendi (tek dosya, tek yöntem) |
| 2 | Kaynak taraması çağrı biçimi kaçırıyor | ✓ Yeni tarama eklenmedi; var olan tarama yeni tüketiciyi yakaladı (önce kırmızı, sonra listeyle yeşil) |
| 3 | 06 "Varsayılan" / FK dizini | ✓ Şema değişmedi |
| 4 | Aynı sayıyı anan öteki yerler | ✓ Kural değişmedi; yalnız tüketici listesi genişledi, açıklaması güncellendi |
| 5 | 09 metni karar kaydısız | ✓ 09 değişmedi |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj --no-build` | Son tam koşu 7224 testten 4 kırmızı (üç mimari tarama + alan işareti) düzeltildi; Architecture ad alanı 1696/1696 yeşil; yeni `PanelHomeDetailsTests` 6/6 |
| Integration | bkz. aşağı | `dotnet test --project tests/Shopfolio.IntegrationTests/… --no-build --filter-class …` | `PanelHomeAndVerificationPagesTests` 15/15, `Administration.PanelHomeTests` 25/25 |
| Format | ✓ | `dotnet format <proje> --no-restore --verify-no-changes` | Web, UnitTests, IntegrationTests temiz |

Integration TAM sonucu dönüşte ve PR açıklamasında yazılıdır.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T521-panel-ana-sayfa`
- **PR:** PR açıklamasında

## ⚠ Maddeler

- Kalem adı okuyucusu yok: IBAN bekleniyor satırı "IBAN bekleniyor" + sipariş no gösterir, kalem adı yok (D-1297).
- IBAN satırı süresi yalnız `CancellationOrRemoval`, `NoticeWithoutIban`, `ReturnAfterIbanDeleted` nedenlerinde; başlangıç talep `CreatedAt` günüdür (beyan/iptal gününün yaklaşığı); öteki nedenler süresiz (K-716) (D-1298).
- "Kart iadesinin sonucu bekleniyor" cümlesi belgede yok; ürünün kendi cümlesi yazıldı (K-940) (D-1299).
- Sipariş satırı bağlantısı `/panel/siparisler/{orderId}`; sayfa sonraki task'ta (D-1300).
- Yeniden doğrulamada yanlış şifre: alan mesajı "Şifre hatalı."; 429 nötr limit iletisi.
- `AdminEmailChanges` modül düzeyi ayrı test sınıfı yok; davranış sayfa testleriyle (KK7-KK10) gerçek satırlarda sınanıyor (D-1301).

## Dış Varsayımlar

Dış varsayım: yok.
