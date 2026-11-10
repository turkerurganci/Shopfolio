# T382 — Panel ana sayfası verisi: altı bekleyen iş sayacı, kanal ve firma bildirimi uyarıları, kurulum kontrol listesi

**Faz:** F3 | **Durum:** ⏳ Yapım tamam — doğrulama bekliyor | **Tarih:** 2026-11-10

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- Administration: `IPanelHome` / `PanelHomeService` (tablo, bağlam ve istek yok; altı sayaç, iki uyarı ve kurulum kontrol listesi her çağrıda yeniden hesaplanır, hiçbir şey saklanmaz), `PanelHomeRules` (saf kurallar: açık adet sayımı, sayaçların sabit biçimi, kontrol listesi), `PanelCounterKind`, `PanelListTarget`, `SetupChecklistItemKind` (her biri ayrı dosya).
- Ordering: `PendingWorkReader` (ödeme onayı bekleyen havale, Teslim edilemedi sayısı; Hazırlanıyor / Kargoda / işaretsiz hizmet kalem kümeleri sipariş sayfa sayfa okunur) ve `IOrderingModule` yüzeyi.
- AfterSales: `CountOpenDefectClaimsAsync`, `ListOrdersAwaitingReturnGoodsAsync` (bildirilen − teslim alınan − reddedilen − "mal dönmedi" kapanışı; yeniden açılan geri gelir; devam 3'te kayıt sayfa sayfa okunur), `ListOrdersAwaitingRefundProcessingAsync` (imleçli sayfa).
- Payment: `ListOrdersWithOpenRefundWorkAsync` (sağlayıcıda başarısız ya da sonucu beklenen kart geri ödemesi).
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
| 4 | İade ve geri ödeme: üç tür, IBAN bekleniyor yok | ✓ | `PanelHomeTests.RefundAndReturn_CountsTheReturnedGoodsToReceive_...KK4`, `RefundAndReturn_DoesNotCountAnItemThatWaitsForTheCustomersIban_KK4`, `AnOrderWithTwoKindsOfWork_IsCountedOnce_KK4`, `AGoodsReceipt_EndsTheWaitForTheGoods_...KK4` |
| 5 | "Mal dönmedi" ve iade reddi düşürür, yeniden açılan geri gelir | ✓ | `PanelHomeTests.ANotReturnedClosure_DropsTheItem_AndItsReopeningBringsItBack_KK5`, `AReturnRejection_DropsTheItem_KK5`; `PanelHomeRulesTests.AnOrder_WhoseEveryItemIsClosed_...KK5` |
| 6 | Her sayaç kendi listesine gider | ✓ | `PanelHomeRules.ListOf`; `PanelHomeRulesTests.EveryCounter_LeadsToItsOwnList_KK6` |
| 7 | Kanal uyarısı (5 hata + 15 dk, ilk başarıda kalkar), firma uyarısı, ikisi okunan hâl | ✓ | `PanelHomeTests.TheChannelWarning_OpensAtFiveErrorsAndFifteenMinutes_...KK7`, `TheFirmNoticesWarning_IsOpenWhen...KK7`, `TheFirmNoticesWarning_LiftsWhenTheContactEmailChanges_AndTheTwoWarningsShowTogether_KK7`; `PanelHomeRulesTests.TheChannelWarning_OpensAtFiveErrorsAndFifteenMinutes_KK7` |
| 8 | Kurulum kontrol listesi eksiği adıyla sayar | ✓ | `PanelHomeRules.SetupChecklist`; `PanelHomeTests.TheChecklist_NamesTheMissingIdentityFields_...KK8`; `PanelHomeRulesTests.TheChecklist_NamesEachMissingCondition_...KK8` |
| 9 | Sihirbaz ve sıra yok; tamamlanan işaretlenir, hepsi bitince liste kalkar | ✓ | `PanelHomeTests.TheChecklist_MarksTheItemsAsTheyComplete_KK9`; `PanelHomeRulesTests.TheChecklist_MarksTheCompletedItem_...KK9`, `TheChecklist_Disappears_WhenEveryItemIsDone_...KK9` (null döner) |

### Öz denetim (bitiş kapısı 2)

Devam 3 işçisi önceki işçilerin yaptıkları dahil bütün maddeleri diff'te yeniden denetledi.

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kabul kriterleri 1–9 | ✓ | yukarıdaki tablo |
| KK1 altı sayaç, yedincisi yok | ✓ | `TheHome_HasSixCounters_AndNoSeventh_KK1` |
| KK2 altı sayacın adı ve kaynağı | ✓ | `TheCounters_AreNamedAsTheDocumentNamesThem_...KK2`; `PanelHomeTests` KK2 |
| KK3 geri dönen sipariş kargoya verilecekte | ✓ | `ToShip_AnOrderThatCameBackAsDeliveryFailed_...KK3` |
| KK4 üç tür sayılır, IBAN bekleniyor sayılmaz | ✓ | `RefundAndReturn_*_KK4` |
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
| Unit | ✓ 5755 geçti (devam 2; devam 3'te birim kodu değişmedi) | `dotnet test --project tests/Shopfolio.UnitTests/...` | `PanelHomeRulesTests` dahil |
| Integration (PanelHomeTests) | ✓ 18/18 (devam 3, sayfalama değişikliğinden sonra yeniden koşuldu) | `dotnet test --project tests/Shopfolio.IntegrationTests/... --filter-class "*PanelHomeTests"` | 18 geçti, 0 kırık |
| Integration (tam, devam 3 öncesi kod) | ⚠ 3399 testten 232 kırık, hepsi `Web.*` (222) ve `Logging.*` (10): "HeartbeatRecord row is missing" | tam koşu | `origin/main`'de aynı Web ad alanı 222 kırık (594 testten 372 geçti): dal kaynaklı değil, bu ortamda önceden var; CI yeşil. Kovalanmadı. Administration 62/62 |
| Format | ✓ | `dotnet format <proje> --verify-no-changes` (üç proje) | çıkış 0 |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ⏳ bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T382-panel-ana-sayfa`
- **Commit:** son commit PR gövdesinde
- **PR:** PR_NUMARASI
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
| 2 | Açık havale iade satırı (satır var, gönderilmemiş) Payment okumasında yok | D-927 | IBAN durumuyla birlikte ele alınmalı |
| 3 | Liste sayfaları sayaç hedeflerini tüketir | D-928 | Sayfa task'ları (T52x) |
| 4 | Sayaç birimi sipariş (talep kaydı sayısı değil) kararının teyidi | D-929 | Ürün kararı |
| 5 | "Açık talep" tek liste mi iki liste mi (04 E-38 GA-9) | D-930 | Belge boşluğu |

## Notlar

- **⚠ Kolay kararlar (kurala en uygun seçenekle verildi):** sayaç birimi sipariştir; açık talep = tüm açık iletişim talepleri (her tip) + açık ayıp talepleri, liste hedefi tek (`OpenRequests`); Teslim edilemedi siparişleri kalemlerine bakılmadan "kargoya verilecek"te; açık adet = adet − kapanan adet; terminasyon malı yalnız gönderilmiş fiziksel kalemde "teslim alınacak", "IBAN bekleniyor" sayılmaz; `ListOrdersAwaitingRefundProcessingAsync` sahne filtresi ödeme yöntemine bakmaz (kart iptali satırı refund row yoksa sayılır; sistem kart iadesi her zaman satır yazar).
- **⚠ Yerel ortam:** Integration tam koşuda Web/Logging "HeartbeatRecord row is missing" kırıkları `origin/main`'de de var; CI yeşil; kovalanmadı.
- **Working tree (Adım -1):** temiz
- **Repo memory (bitiş kapısı 9):** MEMORY.md'ye dokunulmadı
