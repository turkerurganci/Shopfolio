# T521 — Panel ana sayfası ve yönetici hesabının doğrulama ekranları (E-31, E-54)

**Faz:** F5 | **Durum:** Yapım tamam, doğrulama bekliyor | **Tarih:** 2026-11-12

---

## Yapılan İşler

- Panel ana sayfası (`Pages/Panel/Index.cshtml(.cs)`, `PanelHomeModel`, `PanelHomeTexts`): uyarılar, kurulum listesi, altı sayaç, "Müşteriden beklenenler" (IBAN bekleniyor, ilk on satır); boş bölüm çizilmez; sayfada form ve yazma yok.
- Ana sayfanın kendi okuyucuları: `IPanelHomeDetails` / `PanelHomeDetailsService` (Administration), `IAfterSalesModule.ListAwaitingIbanLinesAsync`, `IPaymentModule.CountCardRefundProblemsAsync`, `AfterSalesPeriodRules.RefundLastDay`.
- E-54 sayfaları: `Verify` (`/panel/dogrulama/{anahtar}`), `Revert` (`/panel/geri-al/{anahtar}`), `Reauth` (`/panel/yeniden-dogrulama`), `LandingNextStep`; metinler `PanelAccessTexts`'te, adresler `PanelFramePaths`'te. Modül tarafı: `AdminEmailChanges.VerifyNewEmailAsync` / `RestorePreviousEmailAsync`, `IAdministrationModule`, `ReauthenticationValues`, `SessionStore`.
- Bu turda (devam 2): eksik testler (ana sayfa uyarıları, IBAN satırları, iki yeni modül okuyucusu), mimari tarama düzeltmeleri (tüketici yöntem listesi, `ReauthenticateInput` alan işareti, `!` kullanımı), kayıtlar.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Pages/Panel/{Index,Verify,Revert,Reauth}.cshtml(.cs)`, `LandingNextStep.cs`, `PanelAccessTexts.cs`, `Pages/Frame/PanelFrame.cs`
- `src/Shopfolio.Web/Modules/Administration/*` (AdminEmailChanges, PanelHomeDetailsService, sözleşmeler), `Modules/AfterSales/*`, `Modules/Payment/*`, `Infrastructure/Security/{ReauthenticationValues,SessionStore}.cs`
- Testler: `tests/Shopfolio.IntegrationTests/Web/PanelHomeAndVerificationPagesTests.cs` (15), `Administration/PanelHomeTests.cs` (+2), `tests/Shopfolio.UnitTests/Administration/PanelHomeDetailsTests.cs` (6), `Fakes/StubModule.cs`, `Architecture/LinkKeySourceScan.cs`
- Kayıt: `Docs/STATUS_CHANGELOG.d/T521.md`, `Docs/DEFERRED_BACKLOG.d/T521.md`, `IMPLEMENTATION_STATUS.md` tablo satırı, `DEFERRED_BACKLOG.md` (D-968, D-975 kapandı)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | `GET /panel` uyarı, kurulum listesi, altı sayaç, beklenenler; boş bölüm görünmez | ✓ (düzeltme 1: satır içeriği, kanal uyarısı geçişsiz, aydınlatma alt satırı) | `TheHome_DrawsTheSixCounters_ZeroIncluded_AndLeavesAnEmptySectionOut_T521_KK1`, `AnIbanLine_CarriesTheNumberTheItemNameTheReasonAndTheDate_AndTheRefundOfALatePaymentHasNoItemName_T521_KK3`, `TheChannelWarning_LeadsNowhere_…_T521_KK1`, `TheNoticeItem_CarriesTheLineAboutTheClosedEntrances_…_T521_KK1`, `ACountOfZero_OpensNoWarning_AndACountAboveZero_OpensItsOwnLine_T521_KK1` |
| 2 | İstek anında hesap, sayaç saklanmaz; eksik madde satış kapısından, kendi ayar ekranına bağlı | ✓ | `ACounter_ChangesWhenTheRecordsChange_AndIsNotStored_T521_KK2`; `PanelHomeDetails` tabloya yazmaz; `PanelHomeModel.WarningPathOf`, `PanelHomeTexts.ChecklistPath` |
| 3 | Sayaçlar süzgeçle listeye bağlı; IBAN bekleniyor ilk on, kalan süreye göre | ✓ | `TheIbanLines_ShowTheTenWithTheLeastTimeLeftFirst_AndTheRestAreOnTheLink_T521_KK3`, `ALineWithoutALastDay_HasNoPeriod_…_KK3`, `TheAwaitingIbanLines_AreTheOpenRequests_…_KK3` |
| 4 | Yeniden doğrulama: işlem adı, şifre alanı, Google yok; yok/dışı 303 `/panel/hesabim`; açılış önceki izni düşürür | ✓ | `TheReauthenticationPage_Redirects_…_KK4`, `TheReauthenticationPage_NamesTheOperation_AsksThePassword_HasNoGoogle_AndDropsTheEarlierPermission_T521_KK4` |
| 5 | `Reauthenticate` doğruda tek kullanımlık değer; liste dışı 303; yanlış 422 + L-1; 429 yapmaz | ✓ | `TheRightPassword_GrantsOneSingleUseValue_…_KK5`, `AWrongOrEmptyPassword_Is422_CountsForTheLimit_AndAtTheLimit429DoesNothing_T521_KK5` |
| 6 | İniş ekranları çerçevesiz, oturumdan bağımsız; hâl kayıttan, kod yalnız kullanılmış bağlantıdan | ✓ | `TheVerificationLanding_HasThreeStates_…_KK6`, `TheRevertLanding_HasThreeStates_…_KK6` |
| 7 | `VerifyEmail` tek koşullu tüketir, e-posta yeni adres, eskiye bildirim + geri alma bağlantısı, iz, oturum açmaz | ✓ | `TheVerificationButton_ConsumesTheLinkOnce_ChangesTheAddress_NotifiesTheOldOne_AndWritesTheTrail_T521_KK7` |
| 8 | Arada geçersizleşmişse 200 yazmaz; yeni adres başkasının olduysa 422, tüketilmez | ✓ (düzeltme 2: kilit altında yeniden okuma, tüketim yazımdan önce) | `TheVerificationButton_Is422_WhenTheNewAddressBecameAnotherAdministrators_AndTheLinkIsNotConsumed_T521_KK8`; 200 hâli: `TheVerificationButton_WritesNothing_WhenTheLinkIsSupersededWhileItWaitsForTheAccountLock_T521_KK8`, `…WhenThePendingChangeGoesWhileItWaitsForTheAccountLock_T521_KK8` |
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
| KK8 | Yarışta 200; başkasına geçen adres | ✓ | `…_KK8` (üç test; ikisi kilit beklerken bağlantı/bekleyen değişiklik değişir) |
| KK9 | Oturumlar, şifre, e-posta, iz | ✓ | `…_KK9` |
| KK10 | Çakışmada geri dönüş; yarışta 200 | ✓ | `…_KK10` |
| 09 §25 | İlgili satırlar | ✓ | Tüketici yöntem listesi (`LinkKeySourceScan.ConsumingMethods`/`WrapperMethods`) ve `ModelBindingRuleTests` yeşil; modül yöntemleri kendi işlemini açmaz; sayfa `[NoStore]` |
| Güvenlik taraması | Geri al/doğrula yalnız `OnPost…` işleyicisinden tüketir; GET hiçbir şey yazmaz | ✓ | `LinkKeySourceScanTests` (Architecture 1696/1696) |

### Sık FAIL nedenleri

| # | Neden | Sonuç |
|---|---|---|
| 1 | Bekçi/tarama istisnası geniş | ✓ Tüketici listesine yalnız `AdminEmailChanges.cs` içindeki `RestorePreviousEmailAsync` eklendi (tek dosya, tek yöntem) |
| 2 | Kaynak taraması çağrı biçimi kaçırıyor | ✓ Yeni tarama eklenmedi; var olan tarama yeni tüketiciyi yakaladı (önce kırmızı, sonra listeyle yeşil) |
| 3 | 06 "Varsayılan" / FK dizini | ✓ Şema değişmedi |
| 4 | Aynı sayıyı anan öteki yerler | ✓ Kural değişmedi; yalnız tüketici listesi genişledi, açıklaması güncellendi |
| 5 | 09 metni karar kaydısız | ✓ 09 değişmedi |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj --no-build` | Son tam koşu 7224 testten 4 kırmızı (üç mimari tarama + alan işareti) düzeltildi; Architecture ad alanı 1696/1696 yeşil; yeni `PanelHomeDetailsTests` 6/6 |
| Unit (main birleşiminden sonra) | ✓ | `dotnet test tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj` | 7373/7373 |
| Integration TAM | ✓ | `dotnet test tests/Shopfolio.IntegrationTests/… --no-build --filter-namespace "*<Ad alanı>"` (Web: `--filter-class` parçaları) | 0 kırmızı: Administration 71, Catalog 82, Commands 70, Concurrency 30, CorporateContent 194, Database 36, Disposal 61, FileSecrets 24, Files 112, Jobs 85, Logging 30, Model 1410, Outages 53, Ordering 645, Payment 59, Security 34, SignedEndpoints 11, Web 884 (216+383+285); Pages ad alanında test yok (yalnız örnek sınıflar). Web'de `SessionAccessTests.EveryEndpoint…` açık sayfa listesine `/panel/dogrulama/{}` ve `/panel/geri-al/{}` eklenerek düzeltildi |
| Format | ✓ | `dotnet format <proje> --no-restore --verify-no-changes` | Web, UnitTests, IntegrationTests temiz |

Öz denetim tablosu bu turda diff'te yeniden geçildi (10 kabul kriteri + KK1–KK10: 28 `_T521_KK*` test adı mevcut); eksik bulunmadı.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T521-panel-ana-sayfa`
- **PR:** PR açıklamasında

## ⚠ Maddeler

- IBAN satırı süresi yalnız `CancellationOrRemoval`, `NoticeWithoutIban`, `ReturnAfterIbanDeleted` nedenlerinde; başlangıç talep `CreatedAt` günüdür (beyan/iptal gününün yaklaşığı); öteki nedenler süresiz (K-716) (D-1298).
- "Kart iadesinin sonucu bekleniyor" cümlesi belgede yok; ürünün kendi cümlesi yazıldı (K-940) (D-1299).
- Sipariş satırı bağlantısı `/panel/siparisler/{orderId}`; sayfa sonraki task'ta (D-1300).
- Yeniden doğrulamada yanlış şifre: alan mesajı "Şifre hatalı."; 429 nötr limit iletisi.
- `AdminEmailChanges` modül düzeyi ayrı test sınıfı yok; davranış sayfa testleriyle (KK7-KK10) gerçek satırlarda sınanıyor (D-1301).

## Dış Varsayımlar

Dış varsayım: yok.

## Doğrulama

| Alan | Değer |
|---|---|
| Doğrulama durumu (tur 1) | ✗ FAIL (2026-11-12; dal ucu `cafe1466`, origin/main `bc915800` ile birleşik ağaçta) |
| Bulgu sayısı (tur 1) | 4 (1 ORTA, 3 DÜŞÜK; hepsi S1) + 3 K-notu |
| Düzeltme gerekli mi | Evet — B1 (ORTA, `VAL-A261`): "Müşteriden beklenenler" satırı `04` 9.3.6'nın içeriğini taşımıyor — kalemin adı (geç gelen ödemenin geri ödemesinde "geç gelen ödemenin geri ödemesi"), isteğin sebebi ve tarihi yok, `Text` her satırda sabit "IBAN bekleniyor"; süresiz satırlar istek tarihine göre değil sipariş numarasına göre sıralı (`AwaitingCustomerList.Of`). D-1297 kapsam içi işi ertelemiş; kabul edilmiş risk onayı yok. B2 (DÜŞÜK): kanal uyarısı "Ekrana git" ile E-44'e bağlanıyor; `04` 9.3.3 ve 3.4.3 "geçiş taşımaz" der. B3 (DÜŞÜK): geri alma inişinin "bağlantı geçersiz" ve "geri alma uygulanmadı" hâllerinde sonraki adımın bağlantısı yok, "geri alındı" hâli oturuma bakmadan girişe gider (`04` 9.25.2, 3.4.23); ayrıca başarıda tarayıcıdaki panel çerezi kimin olursa olsun siliniyor (K3). B4 (DÜŞÜK): aydınlatma metni maddesinin altında iletişim formu, hesap kaydı ve yeni hesap açan Google girişinin de kapalı olduğunu söyleyen satır yok (`04` 9.3.4; K-850) |
| Yapım raporu karşılaştırması (tur 1) | KK2, KK4–KK10 kanıtları uyumlu. Uyuşmazlıklar: (1) kriter 1/KK1 ✓ yazılmış ama `04` 9.3.6'nın satır içeriği eksik ve D-1297 ile ertelenmiş (B1); (2) öz denetim `04` 3.4.3 / 9.25.2 / 9.3.4 ayrıntılarını kapsamıyor (B2–B4); (3) "Etkilenen modüller" eski adları (`RevertPendingEmailChangeAsync`) yazıyor, koddaki ad `RestorePreviousEmailAsync`; (4) `12` Durum hücreleri yazılmamış |
| Doğrulayıcı kanıtları (tur 1) | Derleme `-warnaserror` 0/0; `dotnet format --verify-no-changes` üç proje temiz; Unit 7092/7092 + Consistency 295/295; betik 4/4; Integration TAM 3909/3909 (Web 901 = 229+387+285; Ordering 645; Model 1410; Administration+Catalog+Commands+Concurrency+CorporateContent+Database+Disposal+FileSecrets+Files+Jobs+Logging+Outages+Payment+Security+SignedEndpoints 953) — postgres:18, `track_commit_timestamp=on`; `restore --locked-mode` 0; CI run 43150388572 yeşil; undo bekçisi (`IrreversibleConfirmationTests.UndoWord`) değişmemiş, istisna yok; `12` Durum: A257, A259, A260, A262, A263, A316, D043 PASS · A261 FAIL · A258, A264 BEKLEMEDE (E-36 yok; D-1348) |
| Birleştirme notu (tur 1) | origin/main `29562ee9` (T532) ile `src/Shopfolio.Web/Modules/Administration/AdministrationModule.cs` (DI kayıt satırı: `IPanelHomeDetails` ↔ `IPanelRequestList`) kod çakışması — birleştirme geri alındı; kayıt commit'i birleştirmesiz |

## Düzeltme 1 (2026-11-12; tur 1 FAIL sonrası)

Yapım raporundaki eski ad (`RevertPendingEmailChangeAsync`) `RestorePreviousEmailAsync` olarak düzeltildi. D-1297 kapandı.

| Bulgu | Düzeltme | Kanıt |
|---|---|---|
| B1 (ORTA, `VAL-A261`) | "Müşteriden beklenenler" satırı sipariş numarası, kalemin adı (kalemsiz geç gelen ödeme geri ödemesinde "geç gelen ödemenin geri ödemesi"), isteğin sebebi ve tarihi ile varsa kalan süreyi taşır; süresiz satırlar istek tarihiyle sıralı. `AwaitingIbanLine` sebep + `CreatedAt`, `AwaitingIbanRow` kalem adı + sebep + tarih alır; kalem adı `IOrderingModule.FindItemLabelsAsync` (T532) ve `PanelRequestListService.ItemNameOf` ile okunur (ikinci ad yazıcısı yok); refund'ın kalemi `OpenRefundWork.OrderItemId`'den çözülür | `AnIbanLine_CarriesTheNumberTheItemNameTheReasonAndTheDate_…_T521_KK3`, `TheLinesWithoutAPeriod_AreOrderedByTheDateOfTheirRequest_…_T521_KK3`, `TheAwaitingIbanLines_AreTheOpenRequests_…_KK3` (sebep + tarih) |
| B2 (DÜŞÜK) | Kanal uyarısı "Ekrana git" taşımaz (`HomeWarningView.Href` null) | `TheChannelWarning_LeadsNowhere_AndTheOthersLeadToTheirScreen_T521_KK1` |
| B3 (DÜŞÜK) | Geri alma inişinin "bağlantı geçersiz", "uygulanmadı" ve "geri alındı" hâllerinde sonraki adım bağlantısı `LandingNextStep` ile (oturum açıksa E-50, değilse E-29) | `TheRevertLanding_ShowsTheNextStepInEveryState_…_T521_KK6` |
| K3 | Geri alma başarısında çerez yalnız geri alınan hesabın kendi oturumuysa silinir (POST'ta işlemden önce oturumdaki hesap okunur) | `TheRevertButton_DeletesTheBrowsersCookieOnlyWhenItIsTheRevertedAccountsOwnSession_T521_KK9` |
| B4 (DÜŞÜK) | Aydınlatma metni maddesinin altına (aydınlatma tamamsa çerez politikasının altına) "iki metin de yayına alınana kadar iletişim formu, hesap kaydı ve yeni hesap açan Google girişi kapalıdır" satırı | `TheNoticeItem_CarriesTheLineAboutTheClosedEntrances_…_T521_KK1`, `TheHome_DrawsTheSixCounters_…_KK1` (satır bir kez) |

Öz denetim: 10 kabul kriteri ve KK1–KK10 yeniden geçildi; KK1 artık ✓ (satır içeriği, kanal uyarısı, aydınlatma alt satırı); öteki kriterlerin kanıt testleri değişmedi. `sik-fail-nedenleri.md` okundu: "satır içeriği belgedeki alanları taşımıyor" ve "iniş/uyarı hâllerinde belgenin istediği cümle/bağlantı" satırları bu turda karşılandı; "Integration TAM koşulmadan PR" nedeni için aşağıdaki not.

Sınama (düzeltme 1; dal + origin/main `29562ee9` birleşik ağaç): restore ✓; build `-warnaserror` 0 uyarı/0 hata ✓; `dotnet format --verify-no-changes` Web, UnitTests, IntegrationTests ✓; Unit 7116/7116 + Consistency 295/295 ✓; Integration (postgres:18, `track_commit_timestamp=on`): `PanelHomeAndVerificationPagesTests` + `PanelHomeTests` 42/42 ✓. **Integration TAM bu turda koşulamadı** (45 dk bütçe; makinede başka koşular var, 42 test 6 dk sürdü) — doğrulayıcı TAM koşmalı.

## Doğrulama — tur 2 (2026-11-12)

| Alan | Değer |
|---|---|
| Doğrulama durumu (tur 2) | ✗ FAIL (dal ucu `8c9a0754`; origin/main `29562ee9` dalın atası — birleşik ağaç, birleştirme gerekmedi) |
| Bulgu sayısı (tur 2) | 2 (1 ORTA, 1 DÜŞÜK; ikisi de S1) + 2 K-notu (D-1365, D-1366) |
| Düzeltme gerekli mi | Evet. **B1 (ORTA, kriter 8 / KK8):** `AdminEmailChanges.VerifyNewEmailAsync` bağlantıyı ve bekleyen değişikliği hesap kilidinden önce okur, e-postayı **önce** günceller, bağlantıyı **sonra** tüketir; tüketim tutmazsa `LinkInvalid` döner ve `IsRejected` false olduğu için işlem kesinleşir. Bağlantı okumayla kilit arasında yenisiyle değiştirilirse (`ResendAsync`/`RequestAsync` kilit altında `SupersedeEarlierAsync`) hesabın e-postası yeni adres olur ama bağlantı tüketilmez, bekleyen değişiklik silinmez, eski adrese bildirim ve geri alma bağlantısı gitmez, iz yazılmaz — `07` 9.25.3'ün "bağlantı o arada geçersizleşti: tüketim tutmadı, hiçbir şey yazılmaz" cümlesine aykırı; sonraki doğrulamada bildirim yeni adrese gider, eski adres geri alma yolunu kaybeder (`02 §10.2.4`). Düzeltme: tüketimi güncellemeden önceye al (geri alma yolunun sırası) ya da tüketim tutmazsa işlemi geri aldır; bekleyen değişikliği kilit altında oku; yarışı (bağlantı okunduktan sonra yenisiyle değiştirilmiş) sınayan test. **B2 (DÜŞÜK, kriter 1 / KK1):** "kart iadesinin sonucu bekleniyor" uyarısı yalnız "{n} siparişte kart iadesinin sonucu bekleniyor." der; `04` 9.3.3 ve `03 §3.3.24` (K-940) uyarının iadenin sağlayıcıda gerçekleşmiş olabileceğini ve önce sağlayıcının panelinde bakılacağını söylemesini ister (`PanelHomeTexts.CardRefundOutcomePending`). D-1299 bunu belge boşluğu sayar; içerik `04` 9.3.3'te yazılıdır — düzeltmeyle D-1299 kapanır. |
| `12` satırları (tur 2) | `VAL-A261` FAIL → PASS (satır içeriği ve sayaç dışı kalma kanıtlandı); `VAL-A257`, A259, A260, A262, A263, A316, D043 PASS kalır; `VAL-A258`, A264 BEKLEMEDE kalır (E-36 yok; D-1348). Matris betiği hata 0. |
| Yapım raporu karşılaştırması (tur 2) | Öz denetim tablosu var. Uyuşmazlıklar: (1) kriter 8 / KK8 "200 hâli: KK6 testleri" kanıtıyla ✓ yazılmış — KK6 GET inişini sınar, yarışı (okuma ile tüketim arası geçersizleşme) sınayan test yok (D-1301 de bunu söyler); B1. (2) kriter 1 ✓ yazılmış, kart iadesi sonucu uyarısının içeriği D-1299 ile belgeye atılmış; B2. |
| Doğrulayıcı kanıtları (tur 2) | Ana dal CI son 3: 43153986199, 43153986204, 43151185866 success. PR #554 CI run 43162590489 (head `8c9a0754`) tüm işler yeşil. restore ✓; build `-warnaserror` 0/0; `dotnet format --verify-no-changes` üç proje temiz; Unit 7411/7411 (Consistency, Architecture dahil); betik 4/4; Integration TAM 3930 test: Administration 71, Catalog…Logging 725, Model 1409/1410 (1 yük kaynaklı hata — sınıf tek başına 55/55, D-1366), Ordering 645, kalan ad alanları 157, Web 231+389+302 — hepsi yeşil. |

## Düzeltme 2 (2026-11-12; tur 2 FAIL sonrası)

| Bulgu | Düzeltme | Kanıt |
|---|---|---|
| B1 (ORTA, kriter 8 / KK8) | `VerifyNewEmailAsync`: hesabın satır kilidi önce; bağlantı ve bekleyen değişiklik kilit altında yeniden okunur; bağlantı koşullu güncellemeyle e-posta yazımından önce tüketilir (geri almanın sırası); benzersizlik ihlali `Taken()` ile işlemi geri alır. | `TheVerificationButton_WritesNothing_WhenTheLinkIsSupersededWhileItWaitsForTheAccountLock_T521_KK8`, `TheVerificationButton_WritesNothing_WhenThePendingChangeGoesWhileItWaitsForTheAccountLock_T521_KK8` (gerçek satır kilidi tutulur, istek kilitte bekler, kayıt değişir, kilit bırakılır; e-posta, bildirim, geri alma bağlantısı, iz yazılmaz) |
| B2 (DÜŞÜK, kriter 1 / KK1) | `PanelHomeTexts.CardRefundOutcomePending` iadenin sağlayıcıda gerçekleşmiş olabileceğini ve önce sağlayıcının panelinde bakılacağını söyler; D-1299 ✅. | `PanelHomeDetailsTests` (uyarı metni sınanır) |

Öz denetim: 10 kabul kriteri ve KK1–KK10 diff'te yeniden geçildi; kriter 8 / KK8 ve kriter 1 / KK1 dışındakilere dokunulmadı. Kök neden: kilit-sırası ve tüketim-sırası kuralı (`07` 9.25.3) geri alma yoluna uygulanmış, doğrulama yoluna uygulanmamıştı; yarış testi yoktu.

| Tür | Sonuç | Çıktı |
|---|---|---|
| Restore, build `-warnaserror`, format ×3 | ✓ | 0 uyarı, 0 hata |
| Unit TAM | ✓ | 7411/7411 |
| Integration | KISMİ | `PanelHomeAndVerificationPagesTests` 19/19; Integration TAM süre bütçesi nedeniyle bu turda koşulmadı (devir notunda) |

## Doğrulama — tur 3 (2026-11-12)

| Alan | Değer |
|---|---|
| Doğrulama durumu (tur 3) | ✓ PASS (dal ucu `3cec5c9c`; origin/main `059a8e62` (T533) ve `64002df5` (T512) ile birleşik ağaçta — iki birleştirme de çakışmasız) |
| Bulgu sayısı (tur 3) | 0 bulgu + 1 yeni K-notu (D-1375); önceki K-notları D-1298, D-1300, D-1301, D-1347, D-1348, D-1365, D-1366 açık kalır |
| Düzeltme gerekli mi | Hayır. Tur 2'nin B1'i kapalı: `VerifyNewEmailAsync` önce satır kilidini alır, bağlantıyı ve bekleyen değişikliği kilit altında yeniden okur, bağlantıyı e-posta yazımından önce tüketir; iki yarış testi eski kodda kırmızıdır (bağlantı kilit beklerken geçersizleşirse eski kod e-postayı yazıp tüketimde düşer ve işlemi kapatırdı; bekleyen değişiklik kilit beklerken silinirse eski kod kilitten önce okuduğu değişikliği uygulayıp 303 verirdi). B2 kapalı: `CardRefundOutcomePending` `04` 9.3.3'ün içeriğini söyler. |
| `12` satırları (tur 3) | Değişiklik yok: `VAL-A257`, A259–A263, A316, D043 PASS; `VAL-A258`, A264 BEKLEMEDE (E-36 yok; D-1348). Matris betiği hata 0. |
| Yapım raporu karşılaştırması (tur 3) | Öz denetim tablosu var, kriterlerin verdict'i uyumlu. Uyuşmazlıklar: (1) KK10 "yarışta 200" kanıtı yalnız `…_KK10` (422 testi) gösterilmiş; geri almanın 200 hâli yalnız `…_KK9` testinin ikinci gönderiminde (kullanılmış bağlantı) sınanır, kilit beklerken geçersizleşme yarışı test edilmez — kod okumasıyla doğru (tüketim ilk yazım, koşullu), D-1301/D-1375'e not edildi. (2) Düzeltme 2'nin Integration satırı KISMİ'ydi; tur 3'te TAM koşuldu. |
| Doğrulayıcı kanıtları (tur 3) | Ana dal CI son 3: 43153986199, 43153986204, 43151185866 success. PR #554 CI run 43191483868 (head `3cec5c9c`) dokuz işin hepsi yeşil. restore ✓; build `-warnaserror` 0/0; `dotnet format --verify-no-changes` üç proje temiz; Unit 7411/7411 (Consistency, Architecture dahil; T512 birleşiminden sonra 7425/7425); betik 4/4; Integration TAM 3945/3945 (Administration+AutoGenerated+Catalog+Commands+Concurrency+CorporateContent+Disposal+FileSecrets+Files 645; kök+Database+Jobs+Logging+Outages+Payment+Security+SignedEndpoints+Modules.Ordering 308; Ordering 645; Model 515+894; Web 253+400+285); T512 birleşiminden sonra T521 sınıfları 80/80; `restore --locked-mode` 0; `git status` temiz. |
| Birleştirme notu (tur 3) | Çakışma yok. |
