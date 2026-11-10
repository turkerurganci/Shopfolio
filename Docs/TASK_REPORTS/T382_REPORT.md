# T382 — Panel ana sayfası verisi: altı bekleyen iş sayacı, kanal ve firma bildirimi uyarıları, kurulum kontrol listesi

**Faz:** F3 | **Durum:** ⏳ düzeltme 2 tamam (tur 2 FAIL: B3), doğrulama tur 3 bekliyor | **Tarih:** 2026-11-10

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- Administration: `IPanelHome` / `PanelHomeService` (tablo, bağlam ve istek yok; altı sayaç, iki uyarı ve kurulum kontrol listesi her çağrıda yeniden hesaplanır, hiçbir şey saklanmaz), `PanelHomeRules` (saf kurallar: açık adet sayımı, sayaçların sabit biçimi, kontrol listesi), `PanelCounterKind`, `PanelListTarget`, `SetupChecklistItemKind` (her biri ayrı dosya).
- Ordering: `PendingWorkReader` (ödeme onayı bekleyen havale, Teslim edilemedi sayısı; Hazırlanıyor / Kargoda / işaretsiz hizmet kalem kümeleri sipariş sayfa sayfa okunur) ve `IOrderingModule` yüzeyi.
- AfterSales: `CountOpenDefectClaimsAsync`, `ListOrdersAwaitingReturnGoodsAsync` (bildirilen − teslim alınan − reddedilen − "mal dönmedi" kapanışı; yeniden açılan geri gelir; devam 3'te kayıt sayfa sayfa okunur), `ListOrdersAwaitingRefundProcessingAsync` (imleçli sayfa).
- Payment: `ListOpenRefundWorkAsync` (düzeltme 1; eski `ListOrdersWithOpenRefundWorkAsync`): sağlayıcıda başarısız ya da sonucu beklenen kart geri ödemesi ve satırı doğmuş, gönderilmemiş havale geri ödemesi; iade bazında (`OpenRefundWork`).
- AfterSales (düzeltme 1): `FindRefundsAwaitingIbanAsync` — açık IBAN isteği (`RefundId`) olan iadeler; panel bunları sayaçtan düşer (kural IBAN isteği okumasının tek evinde).

### Düzeltme 1 (tur 1 FAIL: B1, B2)

- **B1:** kart iadesi gerçekleşmeyip havale yolu açılan iade ("IBAN bekleniyor") sayaçta kalıyordu; artık açık IBAN isteği olan iade düşer, IBAN girilince (iade havaleye döner, gönderilmemiş satır) geri gelir.
- **B2:** gönderilmemiş havale iade satırı hiçbir kümede yoktu; artık işlenecek geri ödeme sayılır, gönderilince (`SentAt`) düşer. İki yol da kapsanır: karttan havaleye dönüş ve ayıp çözümünün havale iadesi. D-927 kapandı.
- Eski testler: `RefundAndReturn_CountsTheReturnedGoodsToReceive_...KK4` ve `AGoodsReceipt_EndsTheWaitForTheGoods_...KK4` gönderilmemiş havale satırını "sayılmaz" diye sınıyordu; düzeltildi (sayılır, gönderilince düşer).
- Öteki dışlama ve geçişler (mal dönmedi, ret, yeniden açılma, iki tür iş tek sipariş, başarısız/sonucu beklenen kart) aynı gözle tarandı; davranış değişmedi, testleri yeşil.
### Düzeltme 2 (tur 2 FAIL: B3; ikinci iterasyon, eskalasyon kararı: sahip yerine yardımcı)

- **B3:** "IBAN bekleniyor" = iadenin kendisine (`RefundId`) ya da iadenin kalemine (`RefundId == null && OrderItemId == refund.OrderItemId`) bağlı açık IBAN isteği (`02 §10.6.1`; K-686). Kural tek yerde: `AfterSalesModule.FindRefundsAwaitingIbanAsync` artık `RefundOfItem(RefundId, OrderItemId)` çiftleri alır; `OpenRefundWork` kalemi (`OrderItemId`) taşır, `PanelHomeService` çiftleri verir. Böyle iade sayaca girmez; istek karşılanınca (IBAN girilince) sayılır, gönderilince düşer. Kart yolu (VAL-A260: yol açılmadıkça sayılır) değişmedi.
- Main birleştirmesi (T386 #509, T390 #508): `IAfterSalesModule.cs` çakışmasında iki tarafın üyeleri korundu; kayıt belgelerinde main sürümü, T382 girişleri parça dosyalarında.
- Yeni testler (Integration, `PanelHomeTests`): `RefundAndReturn_DefectRemedyTransferRefundWithNoIbanOnFile_WaitsForTheCustomer_UntilTheIbanIsEnteredAndTheRefundSent_KK4` (a), `RefundAndReturn_FailedCardRefundMovedToTheTransferPathWhileTheItemsRequestWasAlreadyOpen_IsNotCounted_KK4` (b), `RefundAndReturn_ARefundVoidedWithTheCorrectionOfTheCancellation_IsNotCounted_KK4` (iptal edildi geçişi; tabloda eksikti).

**Sayacın dışlama ve geçiş durumları (iade ve geri ödeme bekleyen):**

| Durum | Sayaçta | Test |
|---|---|---|
| IBAN bekleniyor: iadeye bağlı açık istek (kart iadesi, havale yolu açıldı) | hayır; istek karşılanınca evet | `..._AndAFailedCardRefundWithItsTransferPathOpenOnlyOnceTheIbanIsEntered_KK4` |
| IBAN bekleniyor: kaleme bağlı açık istek, iade satırı doğmuş (ayıp çözümü, dosyada IBAN yok) | hayır; IBAN girilince evet; gönderilince hayır | `..._DefectRemedyTransferRefundWithNoIbanOnFile_...KK4` (B3 a) |
| IBAN bekleniyor: kalemin isteği iade yolu açılmadan önce açık | hayır; IBAN girilince evet | `..._FailedCardRefundMovedToTheTransferPathWhileTheItemsRequestWasAlreadyOpen_...KK4` (B3 b) |
| IBAN bekleniyor: iade satırı henüz yok, kalemin isteği açık | hayır; IBAN girilince evet | `RefundAndReturn_DoesNotCountAnItemThatWaitsForTheCustomersIban_KK4` |
| Yol değişimi: kart iadesi başarısız, yol açılmadı | evet | `..._CountsTheReturnedGoodsToReceive_TheRefundToProcess_AndTheCardRefundThatFailedOrWaits_KK4`; B1 testinin ilk adımı |
| Yol değişimi: kart sonucu bekleniyor | evet | aynı test (OutcomePending) |
| Mal dönmedi kapanışı | hayır | `ANotReturnedClosure_DropsTheItem_AndItsReopeningBringsItBack_KK5` |
| İade reddi | hayır | `AReturnRejection_DropsTheItem_KK5` |
| Yeniden açılma (mal geldi) | evet | `ANotReturnedClosure_...` ikinci adım |
| Gönderildi (havale, kart) | hayır | `..._CountsABornTransferRefundUntilItIsSent_...`, `..._DefectRemedy...`, `AGoodsReceipt_...KK4`, ilk testte başarılı kart ve gönderilmiş havale |
| İptal edildi (iade satırı geçersiz kılındı) | hayır | `RefundAndReturn_ARefundVoidedWithTheCorrectionOfTheCancellation_IsNotCounted_KK4` |
| İki tür iş, tek sipariş | bir kez | `AnOrderWithTwoKindsOfWork_IsCountedOnce_KK4` |

- CorporateContent: `CountOpenContactRequestsAsync`.
- Notifications: `IPanelWarnings` / `PanelWarningReader` (kanal uyarısı `ChannelWarningCondition.IsOpen`; firma uyarısı F1–F4 kesinleşmiş son satır "ulaşmadı" ve alıcı güncel iletişim e-postası).
- Devam 3: KK8/KK9 testlerinin sıra bağımlılığı giderildi (ödeme alanları başta sıfırlanır); `ListOrdersAwaitingReturnGoodsAsync` büyüyen kümeyi belleğe almasın diye 500'lük kayıt sayfalarıyla okunur; kayıtlar (rapor, değişiklik günlüğü parçası, STATUS satırı, backlog parçası).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Administration/{PanelHomeService,PanelHomeRules,AdministrationModule}.cs`, `Administration/Contracts/{PanelHomeContracts,PanelCounterKind,PanelListTarget,SetupChecklistItemKind}.cs`
- `src/Shopfolio.Web/Modules/Ordering/{PendingWorkReader,OrderingModule}.cs`, `Ordering/Contracts/{IOrderingModule,PendingWorkFacts,PendingWorkSet}.cs`
- `src/Shopfolio.Web/Modules/AfterSales/AfterSalesModule.PendingWork.cs`, `AfterSales/Contracts/{IAfterSalesModule,PendingWorkContracts}.cs`
- `src/Shopfolio.Web/Modules/Payment/{PaymentModule.cs,Contracts/IPaymentModule.cs}`
- `src/Shopfolio.Web/Modules/CorporateContent/{ContactRequests,CorporateContentModule,Contracts/ICorporateContentModule}.cs`
- `src/Shopfolio.Web/Modules/Notifications/{PanelWarningReader,NotificationsModule,Contracts/PanelWarnings}.cs`
- Testler: `tests/Shopfolio.UnitTests/Administration/PanelHomeRulesTests.cs` (yeni), `tests/Shopfolio.IntegrationTests/Administration/PanelHomeTests.cs` (yeni, 18 test), `SalesSummaryTests.cs` ve `SitemapSourceTests.cs` (fake'ler yeni yüzeye uyduruldu), `Support/ApplicationEnums.cs` (4 enum "saklanmaz" listesine)
- `Docs/IMPLEMENTATION_STATUS.md` (T382 satırı), `Docs/STATUS_CHANGELOG.d/T382.md`, `Docs/DEFERRED_BACKLOG.d/T382.md`, `Docs/DEFERRED_BACKLOG.d/T389.md` (D-601 yerinde kapatıldı)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Altı sayaç, yedincisi yok | ✓ | `PanelHomeRules.CounterKinds` (6 eleman); `PanelHomeRulesTests.TheHome_HasSixCounters_AndNoSeventh_KK1`; `PanelHomeTests.AnEmptyShop_ShowsSixCountersAtZero_KK1_KK2_KK6` |
| 2 | Altı sayacın adı ve kaynağı | ✓ | `PanelHomeRulesTests.TheCounters_AreNamedAsTheDocumentNamesThem_...KK2`; `PanelHomeTests` KK2 testleri (ödeme onayı, kargoya verilecek, teslim işareti, hizmet, açık talep, iade ve geri ödeme) |
| 3 | Kargoya verilecek: Hazırlanıyor'daki açık fiziksel kalemli + Teslim edilemedi | ✓ | `PanelHomeTests.ToShip_CountsPreparingOrdersWithAnOpenPhysicalItem_AndTheDeliveryFailedOnes_KK3`, `ToShip_AnOrderThatCameBackAsDeliveryFailed_IsCountedWhateverItsItemsHold_KK3` |
| 4 | İade ve geri ödeme: üç tür, IBAN bekleniyor yok | ✓ | Düzeltme 2 (B3): `RefundAndReturn_DefectRemedyTransferRefundWithNoIbanOnFile_...KK4`, `RefundAndReturn_FailedCardRefundMovedToTheTransferPathWhileTheItemsRequestWasAlreadyOpen_...KK4`, `RefundAndReturn_ARefundVoidedWith..._KK4`; ayrıca: `PanelHomeTests.RefundAndReturn_CountsABornTransferRefundUntilItIsSent_AndAFailedCardRefundWithItsTransferPathOpenOnlyOnceTheIbanIsEntered_KK4` (B1: yol açılmadan 1, açılınca 0, IBAN girilince 1, gönderilince 0; B2 kart→havale), `RefundAndReturn_CountsTheDefectRemedyTransferRefundUntilItIsSent_KK4` (B2 ayıp havale iadesi), `RefundAndReturn_CountsTheReturnedGoodsToReceive_...KK4`, `RefundAndReturn_DoesNotCountAnItemThatWaitsForTheCustomersIban_KK4`, `AnOrderWithTwoKindsOfWork_IsCountedOnce_KK4`, `AGoodsReceipt_EndsTheWaitForTheGoods_...KK4` |
| 5 | "Mal dönmedi" ve iade reddi düşürür, yeniden açılan geri gelir | ✓ | `PanelHomeTests.ANotReturnedClosure_DropsTheItem_AndItsReopeningBringsItBack_KK5`, `AReturnRejection_DropsTheItem_KK5`; `PanelHomeRulesTests.AnOrder_WhoseEveryItemIsClosed_...KK5` |
| 6 | Her sayaç kendi listesine gider | ✓ | `PanelHomeRules.ListOf`; `PanelHomeRulesTests.EveryCounter_LeadsToItsOwnList_KK6` |
| 7 | Kanal uyarısı (5 hata + 15 dk, ilk başarıda kalkar), firma uyarısı, ikisi okunan hâl | ✓ | `PanelHomeTests.TheChannelWarning_OpensAtFiveErrorsAndFifteenMinutes_...KK7`, `TheFirmNoticesWarning_IsOpenWhen...KK7`, `TheFirmNoticesWarning_LiftsWhenTheContactEmailChanges_AndTheTwoWarningsShowTogether_KK7`; `PanelHomeRulesTests.TheChannelWarning_OpensAtFiveErrorsAndFifteenMinutes_KK7` |
| 8 | Kurulum kontrol listesi eksiği adıyla sayar | ✓ | `PanelHomeRules.SetupChecklist`; `PanelHomeTests.TheChecklist_NamesTheMissingIdentityFields_...KK8`; `PanelHomeRulesTests.TheChecklist_NamesEachMissingCondition_...KK8` |
| 9 | Sihirbaz ve sıra yok; tamamlanan işaretlenir, hepsi bitince liste kalkar | ✓ | `PanelHomeTests.TheChecklist_MarksTheItemsAsTheyComplete_KK9`; `PanelHomeRulesTests.TheChecklist_MarksTheCompletedItem_...KK9`, `TheChecklist_Disappears_WhenEveryItemIsDone_...KK9` (null döner) |

### Öz denetim (bitiş kapısı 2)

**Düzeltme 2 (birleşik ağaç, main birleştirmesinden sonra):** kabul kriterleri 1–9 ve KK1–KK9 birleşik ağaçta yeniden denetlendi; tablolar geçerli, fark yalnız KK4 (B3 testleri, `FindRefundsAwaitingIbanAsync` imzası). `git grep -nE "^(<{7}|>{7}) "` boş. Yüzey değişikliği (`FindRefundsAwaitingIbanAsync` imzası, `OpenRefundWork.OrderItemId`) için `git grep` ile kalan eski çağrı yok. Sık-fail dosyası okundu: ilgili satırlar (kural değişince aynı yerler; kural tek evde; modül kurucusu/fake; format; Integration tam) karşılandı.

Devam 3 işçisi önceki işçilerin yaptıkları dahil bütün maddeleri diff'te yeniden denetledi. Düzeltme 1 işçisi push'tan önce KK1–KK9 ve kabul kriterlerinin hepsini, sık-fail listesini yeniden denetledi (tablo geçerli; fark yalnız KK4 ve yeni testler). Sık-fail "kural değişince aynı yerler": yüzey adı değişti (`ListOrdersWithOpenRefundWorkAsync` → `ListOpenRefundWorkAsync`), `git grep` ile kalan anış yok (yalnız bu raporun tarihçe satırları), D-927 kapatıldı.

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kabul kriterleri 1–9 | ✓ | yukarıdaki tablo |
| KK1 altı sayaç, yedincisi yok | ✓ | `TheHome_HasSixCounters_AndNoSeventh_KK1` |
| KK2 altı sayacın adı ve kaynağı | ✓ | `TheCounters_AreNamedAsTheDocumentNamesThem_...KK2`; `PanelHomeTests` KK2 |
| KK3 geri dönen sipariş kargoya verilecekte | ✓ | `ToShip_AnOrderThatCameBackAsDeliveryFailed_...KK3` |
| KK4 üç tür sayılır, IBAN bekleniyor sayılmaz | ✓ | `RefundAndReturn_*_KK4` (düzeltme 1: B1, B2 testleri; `FindRefundsAwaitingIbanAsync`, `ListOpenRefundWorkAsync`) |
| KK5 kapatma ve ret düşürür, yeniden açılan geri gelir | ✓ | `ANotReturnedClosure_*_KK5`, `AReturnRejection_*_KK5` |
| KK6 sayaçtan listeye gidilir | ✓ | `EveryCounter_LeadsToItsOwnList_KK6` |
| KK7 uyarılar okunan hâlden, eşikler doğru | ✓ | `TheChannelWarning_*_KK7` (5 hata/15 dk sınırları Theory'de; eşik değerleri tasarım değerleri testi) |
| KK8 eksik koşul adıyla görünür | ✓ | `TheChecklist_NamesTheMissingIdentityFields_*_KK8` |
| KK9 sihirbaz yok, liste kendiliğinden kalkar | ✓ | `TheChecklist_Disappears_WhenEveryItemIsDone_*_KK9` |
| `09 §25` ilgili satırlar | ✓ | modül sınırı: Administration yalnız modül yüzeylerini okur (`TheHome_ReadsOnlyTheModulesSurfaces_AndKeepsNothing`); kaydı işlemin sahibi yapar kuralı: panel okuma-yalnız, `SaveChanges` yok; yeni enum dosyaları "bir dosya = bir enum" |
| Güvenlik taraması | ✓ | gizli bilgi yok; yeni uç/sayfa yok (veri katmanı), yetki denetimi sayfa task'larının; girdi alınmıyor (yalnız iletişim e-postası karşılaştırması OrdinalIgnoreCase); yeni dış bağımlılık yok |
| Sık-fail 1: bekçi/tarama istisnası geniş | ✓ | yeni tarama istisnası eklenmedi; D-601 taraması (`AccountCommandsTests.RefundExposureCount_IsReadOnlyByTheStatusCommand_NotByAnyCounterOfThePanel`) bütün `src` dosyalarını tarar, panel dosyaları dahil, istisna yalnız `StatusCommand` ve tanımın kendisi |
| Sık-fail 2: kaynak taraması biçim kaçırıyor | ✓ | yeni kaynak taraması yok; mevcut D-601 taraması çok biçimli (çağrı, yöntem grubu, nameof, satır bölünmüş) örnek testiyle sınanıyor |
| Sık-fail 3: 06 "Varsayılan" / FK dizini | ✓ | şema değişikliği yok (tablo, sütun, göç yok) |
| Sık-fail 4: kural değişince aynı sayıyı anan yerler | ✓ | `ApplicationEnums` NotStored listesi, SalesSummaryTests ve SitemapSourceTests fake'leri yeni yüzeye uyduruldu; belge kuralı değişmedi |
| Sık-fail 5: belge kuralını değiştiren 09 metni karar kaydısız | ✓ | 09 / 02 / 05 metnine dokunulmadı |
| Sık-fail "modül kurucusuna bağımlılık" (#51) | ✓ | `NotificationsModule` kurucusu değişmedi (ayrı `IPanelWarnings`); `CorporateContent` ve `Ordering` arayüz yöntemi eklenen fake'ler güncel; Integration Administration 62/62 |
| Sık-fail "büyüyen küme sayfasız belleğe" (#53) | ✓ | `PanelHomeService.OrdersPerPage` 200'lük sipariş sayfaları; `ListOrdersAwaitingReturnGoodsAsync` bu turda 500'lük kayıt sayfalarına çevrildi; `ListOrdersAwaitingRefundProcessingAsync` imleçli |
| Sık-fail "dotnet format test dosyasında koşulmadı" | ✓ | Web, UnitTests ve IntegrationTests projeleri `dotnet format --verify-no-changes` temiz (çıkış 0) |
| Sık-fail "kabul kriteri kısmen karşılandı" | ✓ | hiçbir kriter `~`; backlog kalemleri ek iyileştirme, kriter parçası değil |
| Branch izolasyonu | ✓ | dalda yalnız T382 commit başlıkları |
| D-601 (hedef T382) | ✓ | kapatıldı: sayı panele girmez, tarama testi bütün `src` altında panel dosyalarını da kapsar; panelin sayaçlarında `CountCardOrdersWithRefundExposureAsync` yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit (düzeltme 2, birleşik ağaç) | ✓ 5770/5770 (Architecture, Consistency dahil) | `dotnet test --project tests/Shopfolio.UnitTests --no-build` | 0 kırık |
| Integration (düzeltme 2, birleşik ağaç) | ✓ PanelHomeTests 23/23; Administration 69/69; Payment 14/14; Ordering 599/599 | `--filter-namespace` parçalı | 0 kırık |
| Build / Format (düzeltme 2) | ✓ `-warnaserror` 0 uyarı; `dotnet format --verify-no-changes` 3 proje çıkış 0 | | |
| Unit (önceki) | ✓ 5755 geçti (devam 2; devam 3'te birim kodu değişmedi) | `dotnet test --project tests/Shopfolio.UnitTests/...` | `PanelHomeRulesTests` dahil |
| Integration (PanelHomeTests) | ✓ 18/18 (devam 3, sayfalama değişikliğinden sonra yeniden koşuldu) | `dotnet test --project tests/Shopfolio.IntegrationTests/... --filter-class "*PanelHomeTests"` | 18 geçti, 0 kırık |
| Integration (tam, devam 3 öncesi kod) | ⚠ 3399 testten 232 kırık, hepsi `Web.*` (222) ve `Logging.*` (10): "HeartbeatRecord row is missing" | tam koşu | `origin/main`'de aynı Web ad alanı 222 kırık (594 testten 372 geçti): dal kaynaklı değil, bu ortamda önceden var; CI yeşil. Kovalanmadı. Administration 62/62 |
| Format | ✓ | `dotnet format <proje> --verify-no-changes` (üç proje) | çıkış 0 |

## Doğrulama

### Tur 2 (2026-11-10; head `be29b223`)

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 2; head `be29b223`; dal CI 42733212125 doğrulama anında koşuyordu — Lint, Build, Unit, Contract, Migration, Paketleme geçti, Integration sürüyordu) |
| Bulgu sayısı | 1 — B3 (kriter 4 / KK4, `VAL-A261`, S1, ORTA). B1 ve B2 düzeldi (testler ve kod okuması) |
| Düzeltme gerekli mi | Evet (ikinci iterasyon FAIL: `12 §8.5` sınırı — eskalasyon yardımcının kararı) |
| Bloke-etmeyen not (K) sayısı | 0 |

**B3:** ayıp çözümünün havale iadesi (havale siparişi, dosyada IBAN yok) "IBAN bekleniyor"dadır (`02 §10.6.1`: "firma IBAN'ı olmayan bir geri ödeme için IBAN istemiş (K-686)", "ayıp talebinin çözümünde … süre sayacı yoktur"), ama sayaçta sayılır. `OrderTransitions.StartRemedyRefundAsync` iade satırını doğurur ve `RequestDefectRemedyIbanAsync` kalemin IBAN isteğini `RefundId = null` ile açar (`OpenRefundIbanRequestAsync(new RefundIbanRequestEntry(subject.OrderItemId, null, AdminRequest, …))`); `FindRefundsAwaitingIbanAsync` yalnız `RefundId`'ye bakar, kalemin (`OrderItemId`) açık isteğini görmez; `ListOpenRefundWorkAsync` gönderilmemiş havale satırını sayar. Sondaj (geçici test, geri alındı): havale iadesi `OrderItemId` dolu + kalemin açık `AdminRequest` isteği (`RefundId` null) → beklenen 0, gelen 1. Aynı kök: başarısız kart iadesinin havale yolu açılırken kalemin açık isteği zaten varsa (ör. `NoticeWithoutIban`) istek yeniden kullanılır, `RefundId` yazılmaz, iade sayılmaya devam eder. Düzeltme yönü: açık istek iade kimliğiyle ya da iade kalemiyle (`RefundId == null && OrderItemId == refund.OrderItemId`) eşlensin; test bu iki yolu sınasın. Tur 1 düzeltmesi (B2) bu durumu doğurdu: tur 1'de havale satırı hiç sayılmıyordu.

**Validator kabul kriterleri tablosu (tur 2):** 1, 2, 3, 5, 6, 7, 8, 9 ✓ (düzeltme bu kriterlerin koduna dokunmadı; Integration `PanelHomeTests` + `SalesSummaryTests` 28/28 yeşil); 4 ✗ (B3). Task satırları değişmedi: hepsi `BEKLEMEDE` (uç ve ekran T521). `12_matris_denetle.py --kapsam 1-77` hata 0.

**Birleştirme notu:** `git merge origin/main` (`0aa3716e`, T386 #509) `src/Shopfolio.Web/Modules/AfterSales/Contracts/IAfterSalesModule.cs`'te kod çakışması verdi; `git merge --abort`, doğrulama dalın ucunda yapıldı, kayıt commit'i birleştirmesiz.

**Yapım raporu karşılaştırması (tur 2):** kriter 4 / KK4'te yapım ✓, validator ✗ (B3). Yapım testi `RefundAndReturn_CountsTheDefectRemedyTransferRefundUntilItIsSent_KK4` ayıp havale iadesini IBAN isteği olmadan sınıyor; dosyada IBAN yokken açılan kalem isteği kapsanmıyor. Öz denetim tablosu var, kanıtlı.

### Tur 1

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL (tur 1, 2026-11-10; head `595d5d5f`; dal CI 42728221524 yeşil) |
| Bulgu sayısı | 2 — B1, B2 (ikisi de kriter 4 / KK4, S1, ORTA) |
| Düzeltme gerekli mi | Evet |
| Bloke-etmeyen not (K) sayısı | 0 |

**Validator kabul kriterleri tablosu (tur 1):**

| # | Sonuç | Validator kanıtı |
|---|---|---|
| 1 | ✓ | `PanelHomeRules.CounterKinds` altı eleman; `Counters` her türü bir kez verir; birim ve Integration KK1 yeşil |
| 2 | ✓ | ödeme onayı `PaymentWindowRules.Awaiting(BankTransfer)`; teslim işareti Kargoda + açık fiziksel adet; hizmet: iptal edilmemiş, ödemesi Pending/Failed olmayan, işaretsiz hizmet kalemi; açık talep = açık iletişim + açık ayıp; Integration KK2 testleri yeşil |
| 3 | ✓ | Teslim edilemedi koşulsuz + Hazırlanıyor'da açık adedi olan fiziksel kalem (`ClosedQuantitiesAsync`); Integration KK3 yeşil |
| 4 | ✗ | **B1:** kart iadesi gerçekleşmeyip firma havale yolunu açtığında ("IBAN bekleniyor") sipariş sayaçta kalır (`PaymentModule.ListOrdersWithOpenRefundWorkAsync` IBAN isteğine bakmaz). **B2:** satırı doğmuş ama gönderilmemiş havale iadesi (karttan havaleye dönmüş iade, ayıp çözümünün havale iadesi) IBAN girildikten sonra sayılmaz (`ListOrdersAwaitingRefundProcessingAsync` satırı olanı "işlenmiş" sayar, Payment okuması yalnız kartı okur). Validator sondajı (geçici Integration testi, geri alındı): B1 beklenen 0 gelen 1; B2 beklenen 1 gelen 0 |
| 5 | ✓ | `ListOrdersAwaitingReturnGoodsAsync`: beyan − teslim − ret − açık "mal dönmedi" kapanışı; yeniden açılan geri gelir; Integration KK5 yeşil |
| 6 | ✓ | `PanelHomeRules.ListOf` her sayaca ayrı hedef; birim KK6 yeşil |
| 7 | ✓ | `ChannelWarningCondition` 5 hata / 15 dk; firma uyarısı F1–F4'ün kesinleşmiş son satırı + güncel e-posta; ikisi de her çağrıda okunur; Integration KK7 yeşil |
| 8 | ✓ | kontrol listesi satış kapısının (`GetSalesGateAsync`) karşılanmayan koşullarından, kimlikte eksik alan adlarıyla; Integration KK8 yeşil |
| 9 | ✓ | sıra yok, `IsDone` işareti, hepsi tamamsa `null`; birim ve Integration KK9 yeşil |

**Task satırları (tur 1):** `VAL-A257`…`A264`, `VAL-A316`, `VAL-D043` kanıt olarak uç yanıtı / tarayıcı gözlemi ister — uç ve ekran T521'de (`07 UC-86`) → `BEKLEMEDE` (T383, T384 emsali). `VAL-A261`'in beklenen sonucunu B1 ve B2 bozar; düzeltme T382'de. `12_matris_denetle.py --kapsam 1-77` hata 0.

**Yapım raporu karşılaştırması:** kriter 4 / KK4'te yapım ✓, validator ✗ (B1, B2). D-927 "satırı doğmuş havale iadesi AfterSales okumasında zaten görünür" der; görünmez — o okuma satırı olan kaydı "işlenmiş" sayıp düşürür (sondaj B2). Öz denetim tablosu var, her maddede kanıt yazılı.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T382-panel-ana-sayfa`
- **Commit:** `c7359e7c` — T382: Panel ana sayfası verisi (rapor, kayıtlar, mal bekleniyor kümesi sayfalı okunur)
- **PR:** #510
- **CI:** yapım işçisi izlemez (yönetici izler)
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım yok (yalnız içeride modül okumaları) | yeni paket, servis, API yok | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Sipariş satırında "e-posta ulaşmadı" işareti | D-926 | Sayfa task'ı; panel yalnız uyarıyı verir |
| 2 | Açık havale iade satırı Payment okumasında yok | D-927 | ✅ düzeltme 1 ile kapandı |
| 3 | Liste sayfaları sayaç hedeflerini tüketir | D-928 | Sayfa task'ları (T52x) |
| 4 | Sayaç birimi sipariş (talep kaydı sayısı değil) kararının teyidi | D-929 | Ürün kararı |
| 5 | "Açık talep" tek liste mi iki liste mi (04 E-38 GA-9) | D-930 | Belge boşluğu |

## Notlar

- **⚠ Kolay kararlar (kurala en uygun seçenekle verildi):** sayaç birimi sipariştir; açık talep = tüm açık iletişim talepleri (her tip) + açık ayıp talepleri, liste hedefi tek (`OpenRequests`); Teslim edilemedi siparişleri kalemlerine bakılmadan "kargoya verilecek"te; açık adet = adet − kapanan adet; terminasyon malı yalnız gönderilmiş fiziksel kalemde "teslim alınacak", "IBAN bekleniyor" sayılmaz; `ListOrdersAwaitingRefundProcessingAsync` sahne filtresi ödeme yöntemine bakmaz (kart iptali satırı refund row yoksa sayılır; sistem kart iadesi her zaman satır yazar).
- **⚠ Yerel ortam:** Integration tam koşuda Web/Logging "HeartbeatRecord row is missing" kırıkları `origin/main`'de de var; CI yeşil; kovalanmadı.
- **Working tree (Adım -1):** temiz
- **Repo memory (bitiş kapısı 9):** MEMORY.md'ye dokunulmadı
