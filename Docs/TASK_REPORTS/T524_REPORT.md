# T524 — Satış özeti ve dışa aktarma (E-51, E-53)

**Faz:** F5 | **Durum:** ✓ PASS (doğrulama tur 1) | **Tarih:** 2026-11-12

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- `GET /panel/satis-ozeti` (E-51): parametresizken "Son 30 gün"; hazır dönemler, "Ölçüm penceresi" (yalnız satış en az bir kez açıldıysa) ve özel aralık; sınırlar Türkiye günüyle. Değerler istek anında `ISalesSummary`'nin hesabıdır (tek kural evi `SalesSummaryService`/`SalesSummaryRules`); sayfa tutar ya da oran yeniden hesaplamaz, yalnız biçimler (`DisplayFormat`). Oran payı ve paydasıyla; payda sıfırsa "Değerlendirilemez".
- Satışın ilk açılış anı (`ISettingsModule` → `SalesGates`): ölçüm penceresinin başlangıcı ayar değişikliklerinin izinden türetilir (iki yasal metnin yayını, ödeme hattı, iade adresi birlikte sağlandığı ilk an).
- `GET /panel/disa-aktarma` (E-53): üç bölüm + sorumluluk satırı; `ExportOrders`, `ExportCustomers`, `ExportContactRequests` işleyicileri `IDataExport`'u çağırır, onay istemez. 422: aralık hatası (alan mesajı) ve boş küme (bölümde kalıcı mesaj); 500'de bölüm içi işlem mesajı, iz yok.
- `CsvFileResult` açık akışla çalışır: akış `Response.RegisterForDisposeAsync` ile isteğe bağlanır, `Content-Length` akışın kalanıdır (D-849).
- `PanelFrameFilter` artık `IAsyncAlwaysRunResultFilter` da: işleyici istisnasında hata katmanı sayfayı 500 ile yeniden çizdiğinde çerçeve sonuç aşamasında kurulur (KK10 500 yolu; önce `_PanelLayout` çerçevesiz kalıp middleware'e düşüyordu).
- Gerçek Kestrel testi (D-850): gövde modülün dosyasıyla bayt bayt aynı, `Content-Length` doğru, dosya istek bitince silinmiş.

## Etkilenen Modüller / Dosyalar

- Sayfalar: `Pages/Panel/SalesSummary.cshtml(.cs)`, `Pages/Panel/DataExport.cshtml(.cs)`, `Pages/Frame/PanelFrameFilter.cs`; `wwwroot/css/admin.css`, `wwwroot/js/common.js`.
- Altyapı: `Infrastructure/Forms/CsvFileResult.cs`, `Infrastructure/DisplayFormat.cs`.
- Modüller: `Administration` (`SalesSummaryService`, `SalesSummaryContracts`, `AuditTrail`/`IAuditTrail`), `Settings` (`SalesGates`, `ISettingsModule`, `SettingsModule`).
- Testler: Integration `SalesSummaryPageTests`, `SalesSummaryTests`, `DataExportPageTests`, `DataExportTests`, `SalesGateTests`, `AddressTests`, `MembershipTests` (düzenek); Unit `SalesSummaryAndExportPageTests`, `SalesOpeningRulesTests`, `SalesSummaryRulesTests`, `AuditTrailSurfaceTests`, `ExportSurfaceTests`.

## Kabul Kriterleri Kontrolü (öz denetim)

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Parametresiz "Son 30 gün"; hazır dönemler, özel aralık; Türkiye günü | ✓ | `SalesSummaryPageTests.WithNoParameter_ThePeriodIsTheLast30Days_AndEveryReadyPeriodHasItsDays_KK1`, `APeriodOutsideTheList_…_KK1_KK3`; Unit `ReadPreset_*` |
| 2 | İstek anında hesap, saklanmaz; sayı ve ciro oluştuğu güne, iptal/iade olay gününe | ✓ | `SalesSummaryTests.TheCountAndTheRevenue_AreTheApprovedOrdersBornInThePeriod_…`, `TheCancellationAndReturnCount_IsByEventDay_…_KK8`; `SalesSummaryPageTests.TheNumbers_AreTheApprovedOrdersOfTheirBirthDay_…_KK2_KK3` |
| 3 | Üç oran pay/payda; sıfır paydada "Değerlendirilemez"; pencere satış açılmadıysa yok | ✓ | `TheNumbers_…_AndAZeroDenominatorIsNotAssessable_KK2_KK3`, `OnceSalesOpened_TheWindowIsOffered_…_KK3`, `APeriodOutsideTheList_AndTheWindowWhenSalesNeverOpened_…`; Unit `Ratio_WithAZeroDenominator_IsNotAssessableWithTheReason`, `SalesOpeningRulesTests` (8) |
| 4 | Özel aralık eksik/ileri/ters → alan mesajı, özet yok, 200 | ✓ | `SalesSummaryPageTests.AWrongCustomRange_IsAFieldMessageWithA200_AndNoSummary_KK4` (Theory) |
| 5 | `/panel/disa-aktarma` üç bölüm + sorumluluk satırı | ✓ | `DataExportTests.TheScreen_HasTheResponsibilityLine_AndTheThreeSections_EachWithItsDownload_KK5` |
| 6 | `ExportOrders` 200, ad, `attachment`, `noindex`; satır kalem; yalnız ödenmiş; imhada kişisel sütunlar boş | ✓ | `TheOrderFile_Is200WithTheHeaders_AndTheName_…_KK6_KK7_KK9`, `TheOrderFile_CarriesTheOrdersCreatedInTheRange_ThatWerePaid_…`, `ALine_IsAnOrderItem_…`, `ADisposedOrder_StaysInTheFile_WithTheAddressEmailAndRecipientEmpty_…_KK6` |
| 7 | `=`, `+`, `-`, `@`, sekme, CR öneki etkisiz; dosya sunucuda kalmaz | ✓ | Unit `CsvExportWriterTests.ATextCell_StartingWithAFormulaCharacter_GetsASingleQuote_…` (altı önek); `TheMemberAndContactFiles_…` (`'+Ayşe`); dosya yokluğu `TheOrderFile_Is200…` ve `TheOrderFile_OnARealServer_…_D850` |
| 8 | `ExportCustomers` bekleyen dahil bütün hesaplar; `ExportContactRequests` saklanan bütün talepler | ✓ | `TheMemberAndContactFiles_CarryEveryAccountAndEveryKeptRequest_…_KK8_KK9`, `TheMemberList_HoldsEveryAccount_…`, `TheContactRequestList_HoldsEveryKeptRequest_…` |
| 9 | Dosya üretildikten sonra iz; her indirme ayrı satır; onay yok | ✓ | `TheOrderFile_Is200…` (iki indirme → 2 `ExportOrders` satırı, 200 doğrudan dosya), `EveryDownload_IsInTheTrail_…_KK8`, `TheTrailRow_IsWrittenInItsOwnTransaction_AfterTheFileIsMade_…` |
| 10 | 422 tarih eksik/ileri/ters ve boş küme: dosya yok, iz yok, bölümde kalıcı mesaj; 500'de iz yok | ✓ | `AWrongRange_Is422WithAFieldMessage_NoFileAndNoRow_KK10` (6 durum), `ARangeWithNoPaidOrder_Is422…_KK10`, `AnEmptyMemberOrContactList_Is422…_KK10`, `AFileThatCannotBeMade_Is500WithTheOperationMessageInTheSection_AndWritesNoRow_KK10` |

**Doğrulama kontrol listesi KK1–KK10:** yukarıdaki satırlarla birebir (KK1→1, KK2→2, KK3→3, KK4→4, KK5→5, KK6→6, KK7→7, KK8→8, KK9→9, KK10→10). **`09 §25` ilgili satırlar:** tek kural evi — oranlar ve tutarlar `SalesSummaryService`'te, sayfa yalnız biçimler; tarih aralığı `DateFields.CheckRange` (ortak kural); CSV kaçışı tek yer `CsvExportWriter`; tek hata katmanı — 500 yolu `FormResults.UnexpectedFailure`, işleyici kendi 500'ünü vermez; kaynak akışı istekle kapanır. **Güvenlik:** dosya `no-store` + `noindex`, formül öneki etkisiz, geçici dosya istek sonunda silinir (Kestrel'de de), yalnız yönetici oturumu (panel kapısı), POST'lar sahtecilik jetonuyla.

**Hedefi T524 olan backlog kalemleri:** D-849 ✅ kapatıldı; D-850 Kestrel kısmı ✅, Excel'de açılış D-1379'a (F5 kapısı, elle) devredildi.

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Derleme | ✓ | `dotnet build -warnaserror` | 0 uyarı, 0 hata |
| Unit TAM | ✓ | `dotnet test --project tests/Shopfolio.UnitTests` | 7456/7456 |
| Integration (ilgili sınıflar) — **TAM: CI** | ✓ | `--filter-class "*DataExportTests"`; `"*SalesSummaryTests"`, `"*FrameLayoutTests"`, `"*UnexpectedErrorLayerTests"`, `"*NotFoundPageTests"`, `"*StorefrontFrameTests"`; `"*SalesGateTests"`, `"*AddressTests"`, `"*AdminAccountsPageTests"`, `"*AuditTrail*"`; `"*.MembershipTests"` | 51/51; 91/91; 94/94; 136/136. Tam koşu CI'ın Integration işinde |
| `dotnet format` | ✓ (Web, Unit, Integration) | `dotnet format <proje> --no-restore` (`--verify-no-changes`) | değişiklik yok |

## ⚠ Kolay kararlar (öneriyle verildi)

- Ölçüm penceresinin açılış anı ayrı bir sütunda saklanmaz; ayar izinden türetilir (`SalesGates`, `SalesOpeningRulesTests`). İz imha edilirse geçmiş açılış görünmez → pencere sunulmaz (güvenli taraf).
- Kart hattı tek başına ödeme koşulunu sağlar sayıldı (`TheCardLine_OpensWithoutTheTransfer`).
- Dosya bir kez indirilir; aynı aralığın ikinci indirmesi yeni dosya ve yeni iz satırıdır.
- `PanelFrameFilter`'a sonuç aşaması eklendi (hata katmanının 500 yeniden çizimi için); aynı boşluk vitrin filtresinde → D-1378.
- `SalesSummaryTests`'teki bölüm sürüm numarası beklentisi main'deki sürüme göre düzeltildi.
- İade adresi kaydının iz yazmaması (kapsam dışı) → D-1377.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu (tur 1) | ✓ PASS — tur 1 (2026-11-12, head `c5add538`; `origin/main` `73e36672` dalda, birleştirme gerekmedi). Kriter 1–10 ve KK1–KK10 ✓ (kanıtlar: `SalesSummaryPageTests`, `DataExportPageTests`, `SalesSummaryTests`, `DataExportTests`, `SalesGateTests` 2 yeni, Unit `SalesOpeningRulesTests` 8 ve `CsvExportWriterTests`; doğrulayıcının 3 geçici sınaması — oturumsuz ve müşteri oturumlu export erişimi dosya vermez ve iz 0; aralık dışı ödenmiş sipariş dosyada yok, başlıkta ödeme/kargo/iletişim sütunları; zengin günde sayfa kartları servis değerleriyle birebir, geç ödemesi sistemce iade edilmiş Başarısız sipariş sayı/ciro/dağılımda yok). Yerel: derleme `-warnaserror` 0 uyarı, format temiz (Web, Unit, Integration), Unit 7493/7493, betik testleri 4/4, Integration ilgili sınıflar 374/374 + geçici 3/3; **Integration TAM: CI**. Dal CI'ı yeşil (`43282828056`; Lint, Build, Unit, Integration, Contract, Migration, Paketleme, CI Gate). Ana dal son üç run `success` (`43276977661`, `43276977628`, `43243831320`). |
| `12` satırları (tur 1) | PASS: `VAL-A266`, A267, A268, A269, A270, A271, A272, A273, A274, A275, A276, A277, A278, A279, A280, A281, A282, A283, D030; `VAL-D029` zaten PASS. Matris betiği hata 0. |
| Bulgu sayısı (tur 1) | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (tur 1) | 3 — **K1** (D-1377 hedefi → T536): iade adresi kaydı ize yazılmazsa ölçüm penceresinin başı erken türetilir (`06 §10.6` m6 izi varsayar). **K2** (forward D-1423): kart hattı kurulum değeri izde yok; anahtar sonradan tanımlanırsa pencere başı erken çıkar. **K3** (forward D-1424): A271 (yetkisiz istek), A273/A274/A283 sayfa düzeyi sınaması kalıcı teste girmedi (geçici sınamada yeşil). |
| Yapım raporu karşılaştırması (tur 1) | Kabul kriterlerinde tam uyum (1–10 ✓); öz denetim tablosu var. Uyuşmazlık 1: rapor "iz imha edilirse pencere sunulmaz" diyor; kod eksik izde güncel hâli geçmişe yayar (pencere sunulabilir, başı kayar) — 10 yıllık saklama nedeniyle pratik etkisi yok. |

## Known Limitations / Follow-up

- D-1377: iade adresi kaydı işlem izine yazılmıyor (kapsam dışı; kural denetimi gerekli).
- D-1378: vitrin çerçevesi hata katmanının 500 yeniden çiziminde kurulmuyor.
- D-1379: CSV'nin Excel'de açılış denemesi (elle, F5 kapısı).
