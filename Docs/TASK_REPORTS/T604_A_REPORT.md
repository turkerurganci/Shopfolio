# T604 (A) — F5 kapısından kalan test aracı kalemleri

**Faz:** F5 | **Durum:** ✅ A parçası tamam (dilimler A1–A7; son dilim A6 devamıyla kapandı; ürün kodu parçası U ayrı dalda) | **Tarih:** 2026-11-21

---

## Dilim A1 — iade, ödeme, sipariş testleri

Yalnız test ve belge dosyaları değişti; `src` değişikliği yok (`src` isteyen iki kalem U parçasına daraltıldı: IBAN alanı ve iade ekranı kaynağı U'nun).

### Yapılan işler

İlk işçi (A1 ilk yarı):

- D-1676: `PayTrPaymentGatewayTests.Query.cs` yasak değer ve yanıttaki net tutar "9,7631".
- D-1725: `ScriptlessFlowTests.FillCheckoutAsync` yanıltıcı fatura değerlerini betikli yolda da gönderir.
- D-1368: `OrderOperationScreenTests` üye kökü: başka hesap 404, oturumsuz 302/303 giriş, kapalı işlem 303.
- D-1284 ve D-1283 (1): `CustomerOperationsTests` itemless alan süzgeci ve CardRefundNotRealized olgusu.
- D-1424: `DataExportPageTests` erişim, `SalesSummaryPageTests` sayfa düzeyi ciro/sayım.
- D-697: `SchedulerScanTests` VAL-C109 ve VAL-A827 payları.
- D-981 (C090, C091, C080, C002): `CardRefundJobTests`, `PaymentNotificationTests`.

Bu devam:

- D-981 `VAL-C088`: `PaymentWindowTests.UnansweredFinalQuery_KeepsTheReservations_ShowsTheMarkOnThePanelRows_AndTheMarkGoesWithEitherAnswer_VALC088_D981` — yanıtsız sorguda Alındı + Bekliyor, ayırma sürer, iki panel liste satırında işaret; başarı yanıtında Ö1, `004` yanıtında S3 + Ö2, iki satırda işaret kalkar. D-981 kapandı.
- D-1167 (KP-76 beş KRİTİK satır): `VAL-A371`, `A372` → `ProductFormPageTests.TwoAdministratorsOnTheSameProduct_...` (E-33) ve `ContentPanelPageTests.TwoAdministratorsOnTheSameContentRecord_...` (E-41); `VAL-C076`, `C077`, `C078` → yeni `Files/ConcurrentOrderScreenTests` (E-37; 6 test: iki yönetici; yönetici önce kargo/müşteri iptali geçersiz; müşteri önce iptal/yönetici işareti geçersiz; yönetici önce ödendi/müşterinin eski iptal formu sebepli reddedilir; sistem önce süre iptali/yönetici işareti 409; yönetici önce ödendi/sistem süre iptali uygulanmaz). Geçersizleştirmeyen sistem çifti `PaymentNotificationTests...LateSuccess_OnACancelledOrder_..._VALC002`. D-1167 kapandı; `VAL-A371`'in tarayıcı gözlemi payı D-1753 (yeni).
- D-1494 (`VAL-B009`…`B011`): `ScriptlessFlowTests.ThePaymentScreens_ShowTheIban_TheReceivedSentenceAndTheAwaitedCard_InBothWays_VALB009_VALB010_VALB011` (`BrowserSeed.NewOrderAsync` kart seçeneği aldı). Üç motor, iki yol yeşil. D-1494 kapandı.
- D-1725 KK3: `TheSameAsDeliveryBox_DecidesTheBillingAddress_InBothWays_VALB107` firefox ve webkit'te de iki yolda yeşil (chromium önceki dilimde).
- D-1283 (2) ve D-1599: kod değiştirdikleri için T604 U'ya daraltıldı (aşağıda ⚠).

### Notlar

- Working tree: temiz. `core.hooksPath` = `scripts/git-hooks`.
- Dış varsayım: yok (yalnız test kodu; PostgreSQL 18, `track_commit_timestamp=on`).
- Ana dal CI başlangıç denetimi yardımcının işidir (yönetici düzeni); işçi dal CI'ını yoklamaz.
- Integration: **TAM: CI**. Koşulan sınıflar: `ProductFormPageTests`, `ContentPanelPageTests`, `PaymentWindowTests`, `ConcurrentOrderScreenTests` (tek çağrı 92/92); önceki dilimden OrderOperationScreenTests, CustomerOperationsTests, DataExportPageTests, SalesSummaryPageTests, SchedulerScanTests, CardRefundJobTests, PaymentNotificationTests (184/184). Tarayıcı: `VALB009` ve `VALB107` chromium, firefox, webkit (motor başına ayrı çağrı).
- Unit TAM: 8088/8088. `dotnet format` değişen `.cs` dosyalarında temiz.

## Dilim A2 — kimlik, oturum, davet testleri

Yalnız test ve belge dosyaları değişti; `src` değişikliği yok (değişinim denemeleri için geçici bozuldu, `git checkout -- src` ile geri alındı). Kalemler: D-464, D-572, D-801, D-1289, D-1301, D-1374, D-1389, D-1394, D-1405, D-1723.

### Yapılan işler (A2)

- D-1394 ve D-464 (`AdminAccountsPageTests`): davet sayfasında (`/panel/davet/{anahtar}`) dört geçersizleşme anı — geri çekilmiş, aynı adrese yenisiyle geçersizleşmiş, göndereni kaldırılmış, adresi yönetici olmuş (K-1085); her birinde sayfa geçersiz cümlesini formsuz söyler, kabul POST'u 200 geçersiz sayfa verir ve hesap açmaz. KK8 satır denetimi: kaldırılan yöneticinin bağlantı satırları silinir, yalnız onun daveti geçersizleşir, başkasının daveti açık kalır.
- D-572, D-801, D-1405 (`CustomerSignInPageTests`, `RegistrationPageTests`): L-1 429'da `Retry-After` yok; tanınan tarayıcı e-posta ekseni engelliyken girer, işaretsiz tarayıcı engelli kalır; deneme satırları pencere dışına alınınca doğru şifre 303 alır ve `customers` tablosunda kilit/engel sütunu yoktur; kayıtta aynı pencere dolma sınaması; gerçek misafir sepetiyle `/giris` birleşmesi (çerez silinir, eklenen satır adı bir kez söylenir); `/kayit` Google yüzü (tanımsızken düğme yok ve 422, tanımlı ve kapı açıkken düğme ve 303 → Google, kapı kapalıyken düğme yok) ve `SocialLoginEmailUnverified` cümlesi.
- D-1289 (`PanelAccessPagesTests`): yaygın şifre sıfırlamada ve davet kabulünde 422 (bağlantı tüketilmez); sıfırlama tanınan tarayıcı çerezini yazar ve oturum açmaz; L-1'de engellenen deneme giriş kaydına da yazılır, hesap kilitlenmez, pencere dolunca girer; yönetici çıkışı vitrin oturumuna ve çerezine dokunmaz (K-899); `VAL-A306` (müşteri şifresi panele girmez, vitrin oturumu paneli açmaz) ve `VAL-A308` (müşteri adresine giden davet ayrı yönetici hesabı açar, müşteri satırı değişmez).
- D-1301 (`PanelHomeAndVerificationPagesTests`): iki eşzamanlı tüketim — iki basış hesap satır kilidinde birlikte bekletilip sırayla bırakılır; yeni adres doğrulaması ve geri alma için ikişer test; bir 303, bir geçersiz sayfa, tek tüketim, tek iz satırı.
- D-1723 (`Browser/ScriptlessFlowTests.TheLinkScreens_SendTheirButtons_…_VALB110_D1723`): E-21 yeni e-posta ve Google teyidi, E-28 geri alma, E-54 doğrulama ve geri alma düğmeleri gönderilir; yeniden açılışta düğme yok. chromium, firefox, webkit ayrı çağrılarla, her biri betikli ve betiksiz: 2/2 yeşil (üç çağrı). `BrowserSeed.LinkIdOfAsync` eklendi.
- D-1374, D-1389: `src` gerektirdiği için T604 U'ya daraltıldı (karar önerisi satırda; ⚠ aşağıda).

### Notlar (A2)

- Integration: **TAM: CI**. Koşulan sınıflar: `AdminAccountsPageTests`, `CustomerSignInPageTests`, `RegistrationPageTests`, `PanelAccessPagesTests`, `PanelHomeAndVerificationPagesTests` (iki çağrıda 90/90 ve 50/50); tarayıcı `…_VALB110_D1723` üç motorda 2/2.
- Unit TAM: 8088/8088. `dotnet format` değişen yedi `.cs` dosyasında temiz.
- Docker: Postgres 18 `track_commit_timestamp=on`, konteyner `shopfolio-t604a-pg`, port 56604 (iş bitince silindi).

## Dilim A3 — sır, kayıt maskeleme

Kalemler: D-1005, D-1020, D-1120, D-1130. Yalnız test, betik ve belge dosyası değişti (`src` değişikliği yok).

### Yapılan işler (A3)

- D-1005 ve D-1020 (`LogSourceText`, `LogTemplateSourceScan`, `RejectionLineSourceScan`): dört tür-gerektiren biçim metin taramasına girdi — `…Exception`/`…Ex` adlı alan ve `var` ile istisna ifadesinden alınan değişken; `ErrorCode` türlü değişken (bildirimli, `var` ile `ErrorCode` anan ifadeden, adı `ErrorCode`/`RejectionCode` içeren); anonim nesne üye adları (`BeginScope(new { Email = x })`, `new { order.Phone }`, iç içe, `Log` çağrısına verilen anonim nesne); değişkende tutulan şablon (`var t = "{Event} {Email}"`, `string T => …`). D-1020: şablon sabit adla ya da değişkenle verilince ilk istisna-dışı bağımsız değişken şablon sayılır; şablon yerindeki istisna ve `.Message`/`.ToString()` bulgu. Örnek dosyalar `ViolationSamples/Logging/ExceptionArgumentsUntyped.cs`, `AnonymousScopes.cs`, `ErrorCodeVariables.cs`; yeni testler `LogTemplateSourceScanTests` (üç sabit-satır testi + dört veri kümesi), `RejectionLineSourceScanTests` (bir sabit-satır testi + üç veri kümesi). Gerçek `src/` taraması temiz kaldı.
- D-1130: `test-pre-commit.sh` içindeki `-e`/`--env` örnekleri parçalardan kurulur; tek başına denetimde bekçi dosyada yedi satırda takılıyordu, şimdi sıfır.
- D-1120: bekçiye dördüncü dar istisna (yalnız `.sh`, tam sağ taraf `$(cat <yol>)`/`$AD`/`${AD}` ya da `_FILE` + mutlak yol); `README.md` istisnayı yazar. Sekiz satırdan yedisi bekçiden geçer; `storage-setup.sh:66` (`AWS_ACCESS_KEY_ID=t09admin` değeri) ile `package-build.sh:167,172` takılır kalır, D-1770 (`storage-setup.sh:66` bu dalda düzenlenemedi: satır `$ADMIN_CREDENTIAL` seçeneği yüzünden main'deki eski bekçiyi takar; bekçi atlanmaz).
- Yeni backlog: D-1768 (`09 §18.2` madde 2a yan cümlesi + K-1626; belge sözü tarama koduna yetişir), D-1769 (tür çözümlemesiz kalan son artık), D-1770 (`package-build.sh:167,172`).

### Notlar (A3)

- Unit: `Shopfolio.UnitTests.Architecture` ad alanı 1861/1861. Unit TAM ve Integration: bkz. A7 notları (süre sınırında bölündü; TAM Unit ve format devam işçisinde yeniden koşulur).
- `test-pre-commit.sh` 240/240 (eski bekçiyle 232/240: sekiz yeni `check 0` kırmızı).

## Dilim A7 — belge ve yorum

Kalemler: D-1546, D-1547, D-1617.

- D-1546: `ConcurrencySourceScan.ShareLockText` iletisi `ProductLocks.cs`'in yalnız `FOR KEY SHARE` alabildiğini söyler; iletiyi `ConcurrencySourceScanTests.ShareLockText_…_D1546` sınar. Belge payı F5 kapısında kapanmıştı.
- D-1617: iki yorum `src/…/OrderDetail.Refunds.cs` içinde; A parçası `src` değiştirmez, U parçası aynı dosyayı değiştiriyor → T604 U'ya daraltıldı (satırda gerekçe).
- D-1547: yedi davranışın hepsi kalıcı testte (`ContentPanelPageTests` 12 yeni test, `PanelContentEditingTests.ABranchImage_…_D1547`); önceki işçinin derlenmemiş WIP işi derlendi, `InitializeAsync` `images CASCADE` markalama satırını da siliyordu (`brandings` kayıtsız kaldı, açılış düştü) — `UPDATE brandings` + `DELETE FROM images` ile düzeltildi. Ayırt edicilik: Hakkımızda silme kuralı ve marka görsel metni bozulunca iki test KIRMIZI. D-1768: `09` v0.37 §18.2 madde 2 (a) yan cümlesi + K-1626 (`PRODUCT_DISCOVERY_STATUS.md`).

## Dilim A5 — kaynak taraması ve mimari testler

Kalemler: D-146, D-1226, D-1285, D-1329 (daraltıldı), D-1614, D-1626, D-1627, D-1636, D-1656, D-1657, D-1661, D-1671. `src` değişmedi.

- D-146: `NoticeParameterSourceScan` üç tırnaksız okuma yolunu bulur (işleyici parametresi `notice`, `SupportsGet`/`FromQuery` ile bağlı `Notice` özelliği, yazıcı dışında `NoticeParameter.ParameterName` ile değer okuma; `ContainsKey` varlık sorusu muaf — `Product.cshtml.cs` gerçek kullanımı). Örnek `ViolationSamples/Notices/NoticeUnquotedReads.cs`.
- D-1226: `ControlSourceScanTests.TypedLimits` `min`/`max` sayı niteliklerini de bulur; yalnız `min="1"` ve `<progress max="100">` tam biçimiyle muaf. Gezinme (2) maddesi koşullu ileri nottur (başka olay tarihi yok).
- D-1285: (a) görünüm taraması durum üyelerini tam dizge olarak bulur; (b) `OperationSourceScan.Gate()` runner'ın kendi değişkeninin `RunAsync`'ini ister (üç biçim); başka alıcı kapı değildir.
- D-1329: tarama kümeyi zincirin kökü sayar (ExecuteUpdate/ExecuteDelete çağrısından önce ya da `CurrentStateWrite.ApplyAsync(` içinde); `names.Products` değeri küme değildir. `CompanySettingsWriter` yerel değişken geçici çözümünün sadeleşmesi `src` ister → T604 U'ya daraltıldı.
- D-1614: `DirectRequestReadScan` + `DirectRequestReadScanTests`: sayfa modelindeki her `Request.Form`/`Request.Query` okuması gerekçeli, adla ve sayıyla kapalı listeye bağlı (11 kayıt); iki yönlü (liste dışı okuma ve eskimiş kayıt kırar).
- D-1627: `LinkKeySourceScan.MethodHeader` atomik gruplarla geri izlemesiz; uzun metin testi.
- D-1657: `StateRoleCheck.Gates` ters yön (`GateReadMissing`); E-45 ve E-46 sayfaları kapıyı okumuyor → bilinen boşluk `PanelGateReadGaps` (iki yönlü) ve yeni D-1778 (`src`, T604 U).
- D-1661: müşteri durum x rol testi işlem başına "hiç açılmadı" sayar; bilinen üç işlem `NeverOpenForTheCustomer` listesinde (iki yönlü): `CancelDigitalItem` (D-1197) ve yeni D-1779 (tohum eksiği: `WithdrawDigitalItem`, `ReopenDefectClaimByCustomer`).
- D-1626: `NavigationSites` form hedeflerini (`<form action>`, `formaction`, `asp-page`) ve `FormResults.WriteSeeOther`'ın adresini (ikinci bağımsız değişken) site sayar (yeni `FormAction` site ve kanıt türü). Kendi işleyicisine gidenler (`?handler=`, ya da aynı dosyadaki yerelin tanımı taşıyorsa) site değil; düz yol öneki (`/baska?handler=`) site kalır. Listelerin GET formları ve yükleme düğmeleri muafiyette (adres bağlarıyla); başlık araması 3.1.3'ün `F` kanıtı. Kalan iki yazım (C# dizgesindeki `<a href>`, `wwwroot/js` `location.assign`) kalemin "Yapılacak" maddesinde yoktu → D-1780.
- D-1636: muafiyet gerekçesi `04` 2.7.3.3'e dayandı (hata kurtarma yolu, harita dışı) ve `NavigationInventory.OutcomeScreens` her `OutcomeScreen()` gövdesini (4) sabitler; kod ile envanter iki yönlü eşleşir, hedefi veren alan satırları metin kanıtı ve `BoundTo` ile iki yönlendirmeye bağlıdır.
- D-1656: `FormFieldCodeCalls` alan adı ile hata kodunu aynı ifadede taşıyan çağrıları kaynaktan tarar; tabloda tutulmayan çağrı `CodeCheckNotInTable`. Başlangıçta 38 çağrı tabloda yoktu: 16 çağrı için 17 satır `CodeCheckLines`'a eklendi, 22 çağrı `FormFieldCodeInventory`'de gerekçeli muaf (komut satırı yöneticisi 6, modülün retini gösteren sayfa 8, isteğe bağlı satırın koşullu zorunluluğu 4, sözcükle kapalı liste 2, okuyucu 1, `04` 7.2.21.5'in 7.3.2.9 adı geçmeyen yüzde satırı 1).
- D-1671: işlem satırı parçaları tarihi okuyan koşulu da tutar (7.2.8.1 iki yer, 7.2.8.3 düzeltme yolu, 7.2.12.1); okuma sabit tarihe bağlanınca yer raporlanır.

### Notlar (A5)

- Unit TAM (devam işçisi, düzeltmelerden sonra): Consistency dışı 7518/7518 geçti, `Consistency=true` (Lint adımı) 696/696 geçti; betik testleri (`run-script-tests.sh`) 4 dosya geçti. İlk koşuda `ForbiddenTestPatternScan` `InlineData` dizgesindeki `) =>` yüzünden kırmızıydı (yeni `OutcomeScreen_…` Theory); dizgeler gövdeye indirildi, yeşil.
- Integration (yalnız ilgili sınıflar): `ContentPanelPageTests` + `PanelContentEditingTests` 43/43, `StateRolePageTests` 2/2. **TAM: CI.** Bu devam: Integration sınıfı koşulmadı (yalnız test araçları ve belge; `src` değişmedi). **TAM: CI.**
- `dotnet format Shopfolio.slnx --no-restore --include <dalın 56 .cs dosyası>`: iki biçim düzeltmesi uygulandı (boşluk), yeniden koşuda değişiklik yok.
- Ayırt edicilik (D-1547): `ContentForm` Hakkımızda silme koşulu ve `Brand` görsel kodu geçici bozuldu → iki test KIRMIZI; `ProductConstants` tavanı bozulması modeli değiştirdiği için geçersiz deney (başlatma hatası), geri alındı. A5 taramaları için kırmızı örnekler `ViolationSamples` ve Theory satırlarıdır (taramanın eski hâli yeni sabitleri taşımadığından derlenmez).

## Dilim A4 — tarayıcı aracı (tamam; ilk işçi BÖLÜNDÜ, devam işçisi bitirdi)

Kalemler: D-1228, D-1418, D-1433, D-1455, D-1456, D-1678, D-1704, D-1709, D-1710, D-1732, D-1736, D-1746. `src` değişmedi.

- D-1678 kapandı: `MigrationBundle.ResolveBuildTimeout`, `SHOPFOLIO_TEST_BUNDLE_BUILD_TIMEOUT_SECONDS` (pozitif tam sayı saniye; geçersizde 5 dk); `MigrationBundleTests` (varsayılan, 9 geçersiz değer, 3 geçerli değer).
- D-1704 kapandı: `BrowserWaitScan` + `BrowserWaitScanTests` (Unit; 21 test): `Browser` klasöründe istisnasız `Task.Delay`/`Delay(`/`Thread.Sleep`/`Sleep(`/`WaitForTimeout`/`setTimeout`/`setInterval`; yorum ve dizge muaf. Ayırt edicilik: `BrowserTestBase.cs`'e geçici `Task.Delay(10)` konuldu, tarama `BrowserTestBase.cs:85` ile kırmızı oldu, geri alındı.
- D-1710 kapandı: `BrowserSeed` marka adı kurar (`Örnek Mağaza`); yürüyüş başlığın "<ad> - <marka>"/"<ad> – <marka>" (vitrin) ya da "… - Shopfolio Panel" (panel) ile bitmesini ölçer (`TitleMeasure`, 16 test). Ölçü FAQ (`/sss`) ve iletişim (`/iletisim`) başlıklarının markasız olduğunu buldu: `src` hatası → D-1788 (T604 U); iki yönlü bilinen liste `TitleMeasure.KnownWithoutBrand`.
- D-1746 kapandı: (a) `ProbeAsync` okumadan önce `WaitForStylesAsync` (bağlı stil dosyalarının `sheet`i), (b) `AuditScripts.Names` boyutsuz adsız denetimi sayar (aria-hidden, inert, hidden, display none, kapalı details dışarıda; `AuditScriptsTests` 4 sınıf × motor), (c) kesilen betik istisnası adresle eşler, adres yoksa sayıya düşer (`OwnErrorsOf`, `WatchedPage.UrlOfLocation`).
- D-1736: (a) `ScanStep` ekran × ölçüt hücresine girdi (kırmızı tarama = ✗, taranmamış = "–"); (b) `ObservationRecord.AddCompletenessCheck` + `BrowserHost.DisposeAsync`: kayıt koşu sonunda tüm koşudan yeniden yazılır, "–" kalan hücre koşuyu kırar (Audit tek başına artık eksik kayıtla geçmez); (c) WebKit notu kayıtta (`EngineNotes`). Açık: (d) Firefox ileti metni, Firefox sınıf süresi (590 sn) ölçümü.
- D-1732: (c) kayıt notu, (d) `Facts.skip` sonuca bağlandı (atlanacak `nav` bölgesi olan sayfada blokları atla bağlantısı; kopuk sayfa düzeni E-29/E-30/E-54'te gezinme yok); `sameNameElsewhere` kayıtta bilgi (D-1789, bağlam ölçüsü yok).
- D-1709: araç kısmı: `ExecutableOverrides` + `SHOPFOLIO_BROWSER_EXECUTABLES` (`motor=yol|motor=yol`), kayıt motor sürümünü yazar. `12` §4.17 karar cümlesi ve K-1630 AÇIK.
- Yapılmadı (ilk işçide; devam işçisi yaptı, aşağıda): D-1228, D-1418, D-1456, D-1433, D-1455, D-1709 belge kararı.

### A4 devamı (devam işçisi)

`src` değişmedi (geçici değişinimler `git checkout src` ile geri alındı). Yeni dosya `Browser/NarrowClassTests.cs`; tohum (`BrowserSeed`) iki yayında soru ve bir şube (haritada aç bağlantılı) ekledi.

- **D-1228, D-1418, D-1456 (tarayıcı, KK3: üç motor × iki yol; yerelde chromium ve firefox betikli ve betiksiz, webkit CI E2E).** `NarrowClassTests` üç sınıfta (320, 768, 1280) elementlerin gerçek kutularını okur: iletişim sayfası (VAL-B037 geniş sınıfta solda kimlik bloğu ve şubeler, sağda form; öteki sınıflarda tek sütun ve sıra; kimlik bloğu tam, kısaltılmamış, bir bağlantı ya da katlanmanın arkasında değil), VAL-A853 (altbilgideki "İletişim" başlığı altında blok ile sayfadaki blok aynı alanlar, ikisi tam; başlık `/iletisim`'e gider), VAL-A120 (telefon, e-posta, adres), VAL-A121 (her şubede "Haritada aç", adres şubenin harita bağlantısı, yeni sekme), VAL-A122 (dört alan doldurulur ve değer durur, konu seçilir, Gönder görünür; gönderilmez), VAL-B067 (SSS tek sütun, her soru ve cevap tam), VAL-B078 (yeniden doğrulama ekranı tek sütun, bütün blok ve denetim görünür ve sınır içinde, yatay taşma yok), E-25 (dar ve orta sınıfta yatay sekme ve bölümlerden önce; geniş sınıfta dikey liste; "Hesabı sil" her sınıfta son bölüm; klavyeyle belge sırasıyla ve görünür alanda gezinme: chromium ve firefox Tab tuşuyla, webkit odakla). Yatay taşma okuması 04 8.3.2 satır 1'in tek izinli kesmesini (dar sınıfta üst bilgideki uzun marka adı) hariç tutar.
- **Gözlemin bulduğu sapma → D-1790:** geniş sınıfta hesap ekranının iç gezinmesi solda değil, bölümlerin üstünde tam genişlikte (`Profile.cshtml` `account-layout` kullanmıyor); `04` 5.25.18'e aykırı, `src` → T604 U. Testte iki yönlü bilinen sapma (`KnownNavigationNotAtTheLeft`): sayfa düzelince gözlem kırılır.
- **D-1433:** `TheRequestAnswered429_WritesNoLinkAndNoDispatchRow_VALA498_D1433`: limit aşılana dek yazılan bağlantı ve kuyruk satırları ölçülür, bir 429 daha gelir, iki sayı değişmez ve e-posta ekseninde bir deneme daha sayılır. Ayırt edicilik: işlem geri alma kuralı (`TransactionRunner`, reddedilen sonuç kaydedilmez) ve sınırda bağlantı yazan geçici bir değişiklik birlikte uygulanınca kırmızı (bağlantı sayısı 4 → 5), geri alındı; yalnız tek başına sınırda bağlantı yazan değişiklik testi kırmadı, çünkü ret sonucunu işlem sarmalayıcısı geri alır (iddiayı tutan o kural).
- **D-1455, D-1433 paylı satırlar:** her payın kanıtı aşağıdaki "Paylı satır kanıtları" tablosunda; PASS okuması T604 doğrulayıcısının, `12` `Durum` hücreleri yazılmadı.
- **D-1709:** `12` v0.37 §4.17 "Tarayıcı kümesi" (3) iki sürümün koşusunu yazar; K-1630 (`PRODUCT_DISCOVERY_STATUS.md`).
- **D-1732 (a), (b):** kodda zaten kapalı çıktı (T552 düzeltme 2/2b; `WcagCriteria.SourceScan` var olan `AccessibilityRecordTests.TheSourceScan...` testlerini anar, `ResultOf` sonucu matristen türetir); ilk A4 işçisi satırı yanlış okumuştu, satır kapatıldı.
- **D-1736 (d):** `AccessibilityCriteriaTests.ReachAsync`: odak sayfanın son denetiminde kalırsa "Tab sırasında değil", başka denetimde kalırsa "keyboard trap" der; `AuditScriptsTests.Reach_OfAControlOutOfTheTabOrder_...` ve `Reach_OfAFocusThatDoesNotLeaveAControl_...` (chromium, firefox; webkit odağı betikle verir, durum orada doğmaz). Firefox sınıf süresinin 590 sn altı ölçümü ve CI E2E doğrulaması F6 gate'e ileri atıldı (süre bütçesi; yerelde firefox betikli ve betiksiz sınıf koşuları ayrı ayrı geçti).
- **`TitleMeasure.KnownWithoutBrand`:** tohum artık SSS içeriği ve "Sıkça Sorulanlar" menü adı taşıdığından `/sss` başlığı bu ad olarak güncellendi (D-1788 aynen açık).
- **`BrowserWaitScanTests`:** `InlineData` dizgesindeki `() =>` `ForbiddenTestPatternScan`'i kırıyordu (önceki işçiden kalma, yalnız Unit TAM'da görüldü); ok işlevli örnek gövdeye taşındı.

**Koşular (devam).** Unit TAM: Consistency dışı 7540/7540, `Consistency=true` 696/696. Integration (ilgili sınıflar): `NarrowClassTests` 4 test × (chromium, firefox) × (betikli, betiksiz) yeşil; `AuditScriptsTests` (iki yeni Reach testi dahil) chromium ve firefox yeşil; `CustomerPasswordResetAndReauthenticationPageTests` yeşil; chromium betikli: `AccessibilityAuditTests` + `AccessibilityScanTests` + `AccessibilityCriteriaTests` + `AccessibilityRecordTests` + `ScreenInventoryTests` tek çağrıda 27/27 (6 dk 40 sn; tohum değişikliğinden sonra bir kez `ScreenInventoryTests` E-09 başlığı kırmızı çıktı, `TitleMeasure` düzeltildi, `ScreenInventoryTests` + `TitleMeasureTests` 20/20). Webkit ve tam Integration: **TAM: CI E2E**, **TAM: CI**. `dotnet format` değişen yedi `.cs` dosyasında değişiklik üretmedi.

**Paylı satır kanıtları (D-1433, D-1455; `Durum` yazılmadı).**

| `12` satırı | Payı bu task'ın | Kanıt (test) | Öteki pay |
|---|---|---|---|
| VAL-A352 | T516: sıfırlama istekleri 429 (KK4) | `TooManyRequestsOfOneAddress_Give429_AndTheRequestsAreCounted_T516_KK4`, `TheBlockOfTheResetRequest_LiftsWhenTheAttemptsLeaveTheWindow_D572_T516_KK4` | kayıt ve doğrulama bağlantısının yeniden istenmesi: T514 |
| VAL-A362 | T516: yeniden doğrulamada L-1, şifre değiştirme işlemi (KK9) | `AWrongPassword_Gives422WithTheFieldMessage_CountsForL1_AndGivesNoPermission_T516_KK9`, `TooManyWrongPasswords_Give429_AndTheRightOneIsThenNotAccepted_T516_KK9`; T517: e-posta ve silme işlemleri `TheEmailChange_...T517_KK7`, `TheDeletion_NeedsTheValue_...T517_KK10` | sayfa ucunun L-1 sayacı hesabı silme ve e-posta için ayrıca yok |
| VAL-A451 | T516: bağlantı tüketilmez (KK7) | `AWeakOrCommonPassword_Gives422_AndTheLinkIsNotConsumed_T516_KK7` | değer tüketilmez payı T517: `AWeakOrCommonPassword_Gives422_AndTheValueStaysUnspent_T517_KK6` |
| VAL-A498 | T516: 429 | `TooManyRequestsOfOneAddress_...T516_KK4` ve yeni `TheRequestAnswered429_WritesNoLinkAndNoDispatchRow_VALA498_D1433` (ek bağlantı ve kuyruk satırı yok) | UC-111 panel ucu (başka task) |
| VAL-A499, VAL-A501 | T516 müşteri yüzü (UC-61, UC-60/62) | `AWrongPassword_Gives422WithTheFieldMessage_...T516_KK9`, `AnOperationOutsideTheClosedList_IsA303ToTheAccountPage_T516_KK8`, `AnOperationParameterSentTwice_..._T538` | UC-185, UC-108 panel |
| VAL-A506 | T516: bağlantı eski hâli (UC-59) | `AValidLink_OpensTheSettingStep_AndEveryOtherStateOpensTheInvalidOne_T516_KK5`, `ALinkSpentBetweenThePageAndTheButton_Gives200WithTheInvalidState_AndChangesNothing_T516_KK7` | UC-53 T514, UC-112 panel |
| VAL-A561, VAL-A562 | T516: UC-57, UC-59 | `TheRequest_GivesTheSameAnswerForARegisteredAndAnUnregisteredAddress_T516_KK2`, `TheRequest_QueuesTheEmailOnlyForAVerifiedAccount_...T516_KK3`; `TheNewPassword_ClosesEverySession_OpensNone_AndTheOldPasswordStopsWorking_T516_KK6` | UC-111, UC-112 panel |
| VAL-A675 | T516: E-23 payı (KK6) | `TheNewPassword_ClosesEverySession_...T516_KK6` | E-21, E-28 |
| VAL-A100 | T517: e-posta değişikliği başlar (KK7) | `TheEmailChange_WritesThePendingChange_SendsTheLinkToTheNewAddress_AndTheAccountKeepsTheOldOne_T517_KK7` | doğrulanınca hesabın e-postası olur: T343 |
| VAL-A103 | T517: üç işlem değer ister (KK2, KK10) | `ThePosts_WithoutTheValue_Go303ToTheReauthenticationAndWriteNothing_T517_KK2`, `TheDeletion_NeedsTheValue_AndAMissingOneGoesToTheReauthentication_T517_KK10` | yeniden doğrulama ekranı: T516 |
| VAL-A494 | T517: UC-66, UC-67 (KK9) | `TheLinkAskedForAgain_WithNothingPendingIs422_AndTooManyAskingsGive429_T517_KK9`, `TheEmailChange_Gives429OnlyWhenItReplacesAPendingChange_...T517_KK8` | UC-49, UC-179, UC-180 |
| VAL-A504 | T517: UC-66 (KK8) | `TheEmailChange_RefusesABrokenAddress_AVerifiedOthersAddress_ButNotAnUnverifiedOnes_AndSpendsNoValue_T517_KK8` | UC-179 |
| VAL-A554 | T517: ad kaydı "Kaydedildi" (KK4) | `TheName_IsWritten_With303AndTheSavedNotice_AndAnEmptyNameIs422_T517_KK4` | adres, ürün, kategori, kupon payları |
| VAL-A560 | T517: UC-67 (KK9) | `TheLinkAskedForAgain_IsNew_TheEarlierOnesAreSuperseded_AndTheLifeIsNotLengthened_T517_KK9` | UC-49, UC-180 |
| VAL-A566 | T517: UC-65 (KK5) | `ThePasswordChange_TakesTheValueOnce_EndsTheOtherSessions_AndKeepsThisOne_T517_KK5` | UC-181 |

**Koşular.** Unit: `BrowserWaitScanTests` 21/21 (tam Unit koşulmadı: **TAM: CI**). Integration (ilgili sınıflar): `TitleMeasureTests`, `ExecutableOverridesTests`, `AuditScriptsTests`, `MigrationBundleTests` (çözümleme testleri), kesilen betik testleri geçti; chromium betikli: `AccessibilityAuditTests` + `AccessibilityScanTests` + `AccessibilityCriteriaTests` + `AccessibilityRecordTests` + `ScreenInventoryTests` tek çağrıda 27/27 (8 dk 45 sn). Firefox, webkit ve betiksiz yol: **TAM: CI E2E**. Integration TAM: CI. `dotnet format` değişen dosyalarda değişiklik üretmedi.

**⚠ A4.** (1) Aynı adlı bağlantı okuması sonuca bağlanmadı (D-1789). (2) Ölçüt kaydı koşu sonunda kırar: CI E2E'nin Audit, Scan, Criteria sınıflarını tek çağrıda koşması şart (D-1736 b kabulü). (3) Ayırt edicilik: D-1704 değişinimle gösterildi; D-1710 `localhost` başlığı `TitleMeasure` testleriyle kırmızı; D-1746 (b) `AuditScriptsTests` ile.

## Dilim A6 — kararsız test, 12 satırı, birim test

Kalemler: D-137, D-894, D-1140, D-1244, D-1311, D-1348, D-1363, D-1366, D-1397, D-1452, D-1471, D-1677; A5'ten kalan D-1770, D-1779, D-1780. `src` değişmedi. Karar kaydı K-1632 (dört belge hizası ve tarama kapsamı); `04` v0.57, `07` v0.46, `09` v0.38, `11` v0.68, `12` v0.38.

### Yapılan işler (A6)

- **D-1363:** `FirmSettingsPureRulesTests` (27 sınama): `FirmSettings.RequiredName`, `TrackText` ve `FirmIdentityModel.ConditionsOf` saf parçaları. Parçalar özel statik olduğu için `src` değişmeden ada göre (yansıma) ulaşılır; ad kalkarsa arama kırılır.
- **D-1677:** `04` altı hücre tür kimliğini anar; `FormFieldRuleClauseTable` kimliksiz üç anahtarı bıraktı (`Gün (7.3.2.11)` eklendi); `FormFieldInventory`, `FormFieldEnforcementTable`, `FormFieldPlaceEnforcement` kanıt satırlarıyla eşitlendi. Kırmızı: yalnız hücreler değişince `FormInventoryConsistencyTests.FieldInventory_EqualsTheDocumentsAndTheCode` ve `FormFieldRuleClausesTests.EveryClauseKindAndEveryReference_OfTheTable_HasALineOfItsRow` üç hata verdi; tablolar eşitlenince Consistency 696/696.
- **D-1397, D-1452, D-1244, D-1348 (belge hizaları, K-1632):** net miktar üçüncü decimal kuralı (`09 §7.2` m.3, `07 §6.2.2`, `04` 7.3.2.8); `04` 5.25.19 hesap ekranı cümleleri (+ `07` 4.25 atfı); `12` VAL-A024 "seçilemez" okuması; `11 §7.1` KP-50 satırına T527.
- **D-1366, D-1140, D-1311:** `Model` ad alanı beş kez koşuldu (taze Postgres 18, `track_commit_timestamp=on`; dört tam koşu 1457/1457 yeşil, biri benim süzgeç zaman aşımımla yarım): kararsız test yakalanmadı. D-1366 testinin zaman bağımlılığı göçün komut zaman aşımıdır (sürücü varsayılanı 30 sn; kayıtlı kırmızı 47 sn); `MigrateAsync` 300 sn komut zaman aşımı alır. ⚠ kök neden yeniden üretilemedi.
- **D-1770 (yama hazır, commit EDİLEMEDİ):** `storage-setup.sh` `ADMIN_USER`, `package-build.sh` `APP_DSN` ve `CI_LINK_BYTES` değişkenleri ve `test-pre-commit.sh` örnekleri yazıldı ve bu daldaki bekçiyle doğrulandı (254/254); ama makinenin `core.hooksPath`'i main'deki eski bekçiyi çalıştırır ve o iki satırı sır sayıp commit'i durdurdu. Bekçi atlanmadı ve dolanılmadı: yama `helper-state/shopfolio/impl/T604/t604a-d1770.patch`, çalışma ağacından geri alındı; backlog satırı ⚪ kaldı (hedef T604).
- **D-1779:** `Records.ResolvedClaim` tohumu `ReopenDefectClaimByCustomer`'ı açar (listeden çıktı); `WithdrawDigitalItem` katalogda sabit `Closed` olduğundan tohumla açılmaz, neden metni düzeltildi. `StateRolePageTests` 2/2.
- **D-1780:** `04` §3 "Taramanın kapsamı" ve `NavigationOutsideTheMapTests` (4 sınama; C# dizgesi `<a href>` üç dosya dört bağlantı, `location.assign` iki dosya).
- **D-137, D-894 daraltıldı → T604 U** (işaretler ve D-1525 kod seçimi `src`). **D-1471:** payları kanıtlandı (aşağıda); `VAL-B040`/`VAL-B041` tarayıcı gözlemi A6 devamında yapıldı (aşağıda).

**A6 devamı (2026-11-21).**

- **D-1471 (kapandı):** `NarrowClassTests.TheAddressBook_LaysOutItsCardsInTheClassOfEachWidth_VALB040_D1471` ve `TheOrderHistory_LaysOutItsRowsInTheClassOfEachWidth_VALB041_D1471`; 320, 768 ve 1280'de, chromium, firefox, webkit × betikli, betiksiz (12 koşu yeşil). Tohum: yeni `ListedCustomer` oyuncusu (üç adres, durumları farklı üç sipariş; kimse onun listesini değiştirmez). E-26: geniş sınıfta iki sütunlu kart ızgarası (ilk iki kart yan yana, üçüncüsü ilkinin altında aynı hizada), dar sınıfta kartlar alt alta; iç gezinme E-25 düzeninde (dar ve orta sınıfta başta yatay sekmeler, geniş sınıfta içeriğin solunda dikey liste); her kartın Düzenle ve Sil denetimi tam. E-27: geniş sınıfta tablo (beş hücre aynı satırda, sütunlar aynı hizada, başlık görünür), dar ve orta sınıfta kart (numara ve durum rozetleri üstte, tarih, ilk kalem ve toplam altta; başlık yok); her satırda iki ayrı rozet kutusu; ilk kalem adı görünür; hiçbir sınıfta yatay taşma yok. Kırmızı: kart ızgarasının CSS'i geçici tek sütuna çevrilince VAL-B040 kırıldı (geri alındı).
- **D-1798 (kapandı; hedefi F6 gate'ten T604'e alındı):** `PendingCountersAndListsTests.EachOfTheSixCounters_EqualsTheRowsOfTheListItsLinkOpens_AndTheRowsAreTheSeededRecordsOfThatWork_D1798_VALA258` — taze veritabanı; altı sayaç 1, 2, 3, 2, 2, 1 kayıtla tohumlanır (ödeme onayı, kargoya verilecek + teslim edilemedi, teslim işareti, hizmet, iki iletişim talebi, doğmuş havale iadesi); ana sayfadaki sayı, sayacın bağlantısı ve açtığı listenin satırları sayı ve küme olarak eşit. Kırmızı: hizmet sayacının bağlantısı geçici olarak teslim işareti listesine çevrilince kırıldı (geri alındı). Açık talep sayacının kusur talebi payı `PanelHomeTests`te.
- **D-1753 (kapandı; A1'den kalan):** `ConcurrentEditTests` (yeni `SecondAdmin` oyuncusu): iki tarayıcı bağlamı, ilk yönetici kaydeder, ikincisi eski sürümle farklı ad kaydeder → sayfada kaydın o arada değiştiği uyarısı, "Güncel hâli aç" bağlantısı ve tek "Yine de kaydet" düğmesi; ikincinin adı formda durur, veritabanında ilkinin adı kalır; "Yine de kaydet" ikincinin adını yazar. Üç motor × iki yol yeşil.
- **D-1778 (satır yazıldı):** A5'in anıp yazmadığı satır `T604-A.md`ye eklendi (E-45, E-46 sayfaları satış kapısını okumuyor; `src`, T604 U).
- **D-1770:** satıra not zaten yazılı (⚪, hedef T604); yama `t604a-d1770.patch`, bu devamda dokunulmadı (D-1120 bekçisi main'e inince ayrı işle).

**Paylı satır kanıtları (A6; D-1348, D-1471; `Durum` yazılmadı).**

| `12` satırı | Payı bu task'ın | Kanıt (test) | Öteki pay |
|---|---|---|---|
| VAL-A258 | T521/T542 sayaç bağlantıları; T527 liste süzgeçleri | `TheHomeCounters_AreSix_…_AndEachLeadsToItsList_T542_KK6`, `ACounter_ChangesWhenTheRecordsChange_AndIsNotStored_T521_KK2`, `ThePaymentDeliveryIbanAndEmailFilters_EachKeepOnlyTheirOwnOrder_KK1`, `ATimedPendingWorkFilter_…_KK1_KK2`, `RequestsPageTests` (`requestStatus=Open`) | sayı = liste satırı eşitliği tek testte: `PendingCountersAndListsTests…_D1798_VALA258` (D-1798 ✅) |
| VAL-A264 | T527: `EmailUndeliveredMark` süzgeci yalnız işaretli siparişi gösterir | `ThePaymentDeliveryIbanAndEmailFilters_EachKeepOnlyTheirOwnOrder_KK1` | ana sayfa satırının bağlantısı: T521 |
| VAL-A059 | T518 müşteri listesi: hiç ödenmemiş sistem iptali yok | `TheHistory_ListsTheGuestOrderThatFellToTheAccount_…_NotTheNeverPaidSystemCancelledOnes_KK9` | T527 panel listesi: `TheList_IsNewestFirst_ShowsTheSystemCancelledUnpaidOrder_AndPagesWithoutLossOnEqualMoments_KK2` |
| VAL-A603 | T518 adres defteri: düzenleme ve silme reddi (404, kayıt değişmez) | `TheUpdate_WritesToTheOwnersEntryOnly_AForeignOrGoneOrNoIdEntryIs404_AndNoOrderIsTouched_KK5`, `TheDelete_RemovesTheEntry_Is303WithNoNotice_TheSecondIs404_AForeignOneIs404_AndNoOrderIsTouched_KK6` | sepet kaleminin adet reddi: `CartPageTests.ALineOfAnotherCart_OrOneThatNeverWas_Is404_AndNothingIsWritten_T508_KK8` |
| VAL-A554 | T518 adres kaydı `Saved` | `AnAdd_WritesTheEntry_Is303ToTheBookWithTheSavedNotice_AndAnUndeliveredProvinceIsKept_KK3` | ürün: `ProductFormPageTests.SaveProduct_Saved_Is303ToTheProductsForm_AndWritesTheEntries_KK3`; kategori: `CategoriesPageTests.ACategory_GetsItsAddressFromItsName_…` (`notice=Saved`); kupon: `CouponsPageTests.ANewCoupon_IsWritten_And303GoesToItsOwnAddressWithTheSavedNotice_T526_KK9`; tarayıcı gözlemi |
| VAL-B040 | E-26 üç genişlikte tarayıcı gözlemi | `NarrowClassTests.TheAddressBook_LaysOutItsCardsInTheClassOfEachWidth_VALB040_D1471` (üç motor × iki yol) | yok |
| VAL-B041 | E-27 üç genişlikte tarayıcı gözlemi | `NarrowClassTests.TheOrderHistory_LaysOutItsRowsInTheClassOfEachWidth_VALB041_D1471` (üç motor × iki yol) | yok |

**Koşular (A6).** Unit TAM: 8267 sınama, 8266 yeşil; tek kırmızı `TestIsolationScanTests.ConsistencyChecks_Types_CarryTheConsistencyTrait_AndNoOtherTypeDoes` (yeni sınıfta `Consistency` özelliği yoktu) düzeltildi ve yeniden koşuldu. Integration (ilgili): `Model` ad alanı beş kez (1457/1457), `StateRolePageTests` 2/2; **TAM: CI**. **A6 devamı:** Unit TAM yeniden 8267/8267 yeşil (iki kez: tarayıcı ve sayaç testleri eklenince ve biçimden sonra); Integration (ilgili, taze Postgres 18, `track_commit_timestamp=on`): `NarrowClassTests` `VALB040`/`VALB041` 12/12 (üç motor × iki yol), `ConcurrentEditTests` 6/6 ve `ConfirmWindowTests`, `ScreenInventoryTests`, `EmptyStateTests`, `ObservationMaskTests` (tohum değişti) toplu 217/217, `NarrowClassTests` ve öteki ilgili tarayıcı sınıfları chromium betikli 77/77 (`PendingCountersAndListsTests` dahil); **TAM: CI**. `dotnet format` beş değişen `.cs` dosyasında temiz. `test-pre-commit.sh` yamayla 254/254 (dalda yok). `dotnet format` yalnız değişen sekiz `.cs` dosyasında temiz.

**⚠ A6.** (0) **D-1770 commit edilemedi:** `core.hooksPath` main'deki eski sır bekçisini çalıştırır (D-1120 istisnasız); bekçi atlanmadı. Yama `helper-state/shopfolio/impl/T604/t604a-d1770.patch`; D-1120 bekçisi main'e inince uygulanır. (1) D-1366 kök nedeni yeniden üretilemedi; komut zaman aşımı yükseltmesi gözlemden çıkarımdır. (2) `WithdrawDigitalItem` katalogda koşulsuz kapalı: dijital kalemde müşteri caymasının açılması sahibin kararıdır, bu dilim açmadı. (3) VAL-A024'te "seçilemez" okuması seçildi (`02 §3.3.3` ile uyumlu; "görünmez" okuması `ReadOptionsAsync` süzgeci ister → T604 U). (4) D-1780: dizge ve betik hedefleri tarama türü yapılmadı; bilinen liste testi kuruldu.

## Para ve güvenlik kalemleri (KK2)

**A6 D-1770 (sır bekçisi betikleri; commit edilmedi).** Kırmızı: bu daldaki bekçi `HEAD` sürümündeki `scripts/local-reference/storage-setup.sh` ve `scripts/ci/package-build.sh` üzerinde çıkış 1 verir; yeşil: yama uygulanmış sürümlerde çıkış 0 (kanıt yamadaki `test-pre-commit.sh` örnekleri: iki gerçek betik bütün olarak çıkış 0, `h1`–`h5` eski satır biçimleri ve değişken + metin biçimleri çıkış 1, `h6`–`h9` kabul biçimleri çıkış 0). Yama main'deki eski bekçiye takıldığı için dalda yok; dalda D-1770 için değişiklik yoktur. Çıkış yolu ve geri alma: yama uygulanmazsa üç betik aynı kalır.

Bu dilimin kalemleri yalnız test ekler; ürün kodu değişmediği için "değişiklikten önce kırmızı" yerine her testin **ayırt ediciliği** değişinimle gösterilir (src geçici bozuldu, test kırıldı, geri alındı; `git checkout src`) ve var olan testler değişmeden geçer. Çıkış yolu ve geri alma: test dosyalarının PR'ı geri alınır.

| Kalem | Test | Değişinim → sonuç |
|---|---|---|
| D-1167 `VAL-A371/A372` (E-33) | `ProductFormPageTests.TwoAdministratorsOn...` | `RecordSave` sürüm karşılaştırması kapalı → KIRMIZI |
| D-1167 `VAL-A371/A372` (E-41) | `ContentPanelPageTests.TwoAdministratorsOn...` | `ContentPublishing.SaveEditAsync` ve `RecordSave` karşılaştırması kapalı → KIRMIZI |
| D-1167 `VAL-C076/C077/C078` | `ConcurrentOrderScreenTests` | `ShownVersion.Matches` hep doğru → üç test KIRMIZI (C076, C077 müşteri önce, C078 sistem önce) |
| D-981 `VAL-C088` | `...VALC088_D981` | `IsPaymentOutcomeUnknown` hep yanlış → KIRMIZI |
| D-981 `VAL-C090/C091` | `CardRefundJobTests...O4/O5` | `sent >= order.Total` → `>` → iki test KIRMIZI |
| D-697 | `SchedulerScanTests` | `OutageRecovery.IsFresh` hep yanlış → VALC109 ve VALA827 satırları KIRMIZI |
| D-1725 | `...VALB107` | önceki dilim: kutu işaretliyken fatura adı değişinimi → iki yol KIRMIZI |
| D-1368 | `...UnderTheMembersRoot_...` | önceki dilim: ilk koşuda beklenen 303 gerçek davranışla (GET 302, POST 303) uyuşmadı |
| D-1284 | `...TheItemlessIbanFields_...` | önceki dilim: CHECK kısıtına takıldı, tohum düzeltildi |
| D-981 `C080`, `C002`, D-1424, D-1494 | ilgili testler | değişinim yapılmadı (⚠ aşağıda) |
| A2: D-801/D-572 (L-1 429 `Retry-After` yok) | `CustomerSignInPageTests.TheLimit429OfTheSignIn_HasNoRetryAfter_…` | `FormResults.AttemptLimitExceeded` 429'a `Retry-After` yazar → KIRMIZI (kayıt için `PastTheLimitL3_…NoRetryAfter` da) |
| A2: D-572/D-1405 pencere dolunca engel kalkar | `…TheBlock_LiftsWhenTheAttemptsLeaveTheWindow_…`, `RegistrationPageTests.TheBlockOfTheRegistration_…`, `PanelAccessPagesTests.TheBlockedAttempt_…` | `AttemptCounter.IsExceededAsync` pencereyi yok sayar → üçü KIRMIZI |
| A2: D-1405 tanınan tarayıcı | `ARecognizedBrowser_SignsIn_WhileTheEmailAxisIsBlocked_…` | `PasswordRegime` işareti hiç sunmaz → KIRMIZI |
| A2: D-1405 misafir sepeti birleşmesi | `ASignIn_WithARealGuestCart_MergesItIntoTheMembersCart_…` | `GuestCartMerge` birleştirmeyi atlar → KIRMIZI |
| A2: D-1405 `/kayit` Google yüzü | `TheRegistrationPage_HasNoGoogleButtonWhileGoogleIsUndefined_…` | `GoogleOpen` Google tanımını yok sayar → KIRMIZI |
| A2: D-1394/D-464 davet sayfası | `AdminAccountsPageTests.…_D1394_D464` dört test, `Removing_Deletes…_KK8` | `AdminAccounts` davet okuması bayrakları ve adres çakışmasını yok sayar, bağlantı sonlandırma ve geçersizleştirme etkisiz → altısı KIRMIZI |
| A2: D-1289 yaygın şifre | `ACommonPassword_Is422_…` | `PasswordPolicy` yaygın listeyi atlar → KIRMIZI |
| A2: D-1289 tanınan tarayıcı çerezi | `TheSetting_WritesTheKnownBrowserMarkCookie_…` | `Reset` çerezi yazmaz → KIRMIZI |
| A2: D-1289 K-899 çıkış | `TheAdministratorsSignOut_EndsOnlyThePanelSession_…` | çıkış vitrin çerezini de siler → KIRMIZI |
| A2: D-1301 eşzamanlı tüketim | `TwoVerificationPressesAtOnce_…`, `TwoRevertPressesAtOnce_…` | hesap kilidi atlanır ve `TryConsumeAsync` kullanılmışlık koşulunu bırakır → iki test (ve KK8 iki var olan test) KIRMIZI |
| A2: D-1723 | `…_VALB110_D1723` | `TryConsumeAsync` kullanıldı işaretini yazmaz → iki yol KIRMIZI (chromium) |
| A2: D-1289 `VAL-A306`, `VAL-A308` | ilgili testler | değişinim yapılmadı (⚠ aşağıda) |
| A3: D-1005 (istisna, `ErrorCode`, anonim üye, değişkende şablon) | `LogTemplateSourceScanTests` ve `RejectionLineSourceScanTests` yeni 27 örnek/sabit-satır testi | tarama değişikliği geri alındı (`git checkout` üç tarama dosyası) → 27 test KIRMIZI (yalnız yeni olumlu örnekler; olumsuz örnekler yeşil kaldı), değişiklik geri uygulandı → 153/153 YEŞİL; çıkış yolu: tarama dosyalarının önceki hâli, geri alma: PR geri alınır |
| A3: D-1020 (sabit adlı şablon, şablon yerinde istisna) | `…_OnTheUntypedExceptionSample_…`, `ScanTemplates_AnExceptionNotTypedInTheFile_IsReported` | aynı geri alma → KIRMIZI; var olan `ExceptionArguments` örnek testi (5 satır) değişmeden geçer |
| A3: D-1120 (`.sh` dar istisna) | `test-pre-commit.sh` `check 0` sekiz örnek (g10–g17), `check 1` on iki karşı örnek (g1–g9, g18–g20; her kabul biçiminin yanında gerçek sır biçimi: `$(cat "$D/x")` ↔ `$(cat "$D/x")` + metin (g2), `$AD` ↔ `$AD:` + değer (g18), `${AD}` ↔ `${AD}` + değer (g19), `_FILE` + mutlak yol ↔ `_FILE` olmayan anahtar + mutlak yol (g20) ve göreli yol/dizge (g4, g5), `$(cat x)` ↔ `$(echo değer)` (g1, g6), `.sh` dışı dosyada aynı açılım (g7–g9)) | eski bekçiyle sekiz `check 0` KIRMIZI (232/240); istisnanın sağ-taraf bağı (`$` çapası) kaldırılınca `sh expansion: cat + text` KIRMIZI (239/240); yeni bekçi 240/240 |
| A5: D-1626 | gerçek dosyada: `Orders.cshtml` `action` başka yola, `Brand.cshtml` `formaction` başka yola, `Profile.cshtml` `?handler=` önüne düz yol, `UnexpectedErrorLayer` `WriteSeeOther` adresi değişti | her biri `UndocumentedTransition` (kopya kaynakla; dosyalar değişmedi) |
| A5: D-1636 | `Checkout`, `Profile`, `Members`, `ContentForm` `OutcomeScreen()` hedefi değişti; yeni `OutcomeScreen()`; `_outcomeScreen`/`_outcomeAddress`/`own` satırı değişti | `OutcomeScreenMissing` + `OutcomeScreenUndocumented`, `OutcomeScreenCountDiffers`, `EvidenceMissing:Exemption`/`:OutcomeScreen` (T546 doğrulamasının elle deneyi: eskiden hepsi geçiyordu) |
| A5: D-1656 | `FirmSettings`'e tabloda olmayan alan+kod, `ProductSaves`'e dizge alanlı kod, kupon bitiş tarihi kodu değişti, tablo satırı çıkarıldı | `CodeCheckNotInTable` her birinde; muafiyet ölü/tabloda tutulu → `DeadExemption`/`ExemptAndHeld` |
| A5: D-1671 | üç tarih okuması (`MarkDelivered`, iki düzeltme) ve alınan iadenin okuması sabit tarihe bağlandı | yalnız kendi yeri raporlandı (üç kırılma Theory'si tam bir yer bekler; iade okuması yer + 7.2.12.1) |
| A3: D-1130 (bekçinin kendi test dosyası) | her anahtar sözcüklü satırın tek başına bekçiden geçişi (geçici betik) | eski dosya: yedi satır takıldı (65, 67, 72, 73, 74, 75, 78) → yeni dosya: sıfır |

## Öz denetim tablosu (bitiş kapısı 2)

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kabul 1: hedefi T604 olan kalemler kapanır ya da gerekçeyle daraltılır (A1 payı) | ✓ | Kapandı: D-697, D-981, D-1167, D-1284, D-1368, D-1424, D-1494, D-1676, D-1725. Daraltıldı: D-1283 (2) ve D-1599 → T604 U (kod gerekçesi satırda); yeni D-1753 hedef T604. Kalan T604 kalemleri A2…A4 dilimlerinin (bu dilimin kalemi değil) |
| Kabul 2 / KK2: para-güvenlik kalemleri ayrı başlıkta, kırmızı–yeşil | ~ | Yukarıdaki tablo; C080, C002, D-1424 değişinimsiz ⚠ |
| Kabul 3 / KK3 (D-1725, D-1494): üç motor, iki yol | ✓ | `VALB107` ve `VALB009` chromium, firefox, webkit × betikli/betiksiz yeşil |
| Kabul 4 / KK4 (D-1678) | — | Bu dilimin değil |
| Kabul 5 / KK5: kapanan satır `✅ Çözüldü → T604` | ✓ | `Docs/DEFERRED_BACKLOG.md` dokuz satır; daraltılan iki satır yeni hedefi (T604 U) ve gerekçeyi taşır |
| KK1: hedefi T604 olan kalem envanteri (`grep T604`) | ✓ | Her A1 kalemi kapalı ya da gerekçeli; satır listesi yukarıda |
| D-697 | ✓ | `SchedulerScanTests...VALC109`, `...VALA827` |
| D-981 | ✓ | C090, C091, C080, C002, C088 testli |
| D-1167 | ✓ | `ProductFormPageTests`, `ContentPanelPageTests`, `ConcurrentOrderScreenTests`; A371 tarayıcı payı D-1753 |
| D-1283 | ~ | (1) testli; (2) U'ya (`06 §4.31`: IBAN beyan başına, kod kayıt başına) |
| D-1284, D-1368, D-1424, D-1676, D-1725 | ✓ | önceki dilim; D-1725 üç motorda yeniden koşuldu |
| D-1494 | ✓ | `VALB009` testi, üç motor |
| D-1599 | ~ | U'ya (belge çatışması kodu da değiştirir) |
| `sik-fail-nedenleri`: tarama istisnası geniş / çağrı biçimi kaçırma | ✓ | Tarama ya da bekçi değişmedi |
| Hedefi bu task olan kayıt ne kapandı ne ileri atıldı | ✓ | Dokuz kapandı, iki yeniden ileri (T604 U), bir yeni |
| Kısmen karşılanan kabul (~) | ⚠ | A1 dilimi bütün T604 değildir; kabul 1'in kalan payı A2…A4 ve U parçasında, PR bu dilimin PR'ıdır |
| 06 default/FK, kural sayısı, 09 karar kaydı | ✓ | Şema, kural sayısı ve 09 metni değişmedi; karar kaydı yok (D-1599 kararı U'da) |
| Gerçek saate bağlı test yok | ✓ | Yeni testler test saati ve sürümle çalışır; bekleme yok |
| **A2** Kabul 1: A2 kalemleri kapanır ya da gerekçeyle daraltılır | ✓ | Kapandı: D-464, D-572, D-801, D-1289, D-1301, D-1394, D-1405, D-1723. Daraltıldı → T604 U: D-1374, D-1389 (satırlarda gerekçe). `grep -nE "T604" Docs/DEFERRED_BACKLOG.md` ile on kalem de tarandı |
| A2 D-464, D-1394 | ✓ | `AdminAccountsPageTests` beş yeni test (dört geçersizleşme anı sayfada, KK8 satırı); değişinimle altısı kırmızı |
| A2 D-572, D-801, D-1405 | ✓ | `CustomerSignInPageTests` sekiz yeni test, `RegistrationPageTests` bir; üç parçanın (1) `/kayit` yüzü, (2) 429/tanınan tarayıcı/pencere, (3) misafir sepeti hepsi testli |
| A2 D-1289 | ✓ | `PanelAccessPagesTests` altı yeni test: dört davranış + `VAL-A306` + `VAL-A308` |
| A2 D-1301 | ✓ | `PanelHomeAndVerificationPagesTests` iki yeni test, kilit altında deterministik eşzamanlılık |
| A2 D-1723 | ✓ | `…_VALB110_D1723` üç motor × iki yol yeşil; gözlem kaydında e-posta maskeli (`***@***`), anahtar yok; kayıt satırı yalnız boolean sütunlar |
| A2 D-1374, D-1389 | ~ | U'ya daraltıldı: `src` gerektiriyor (⚠ aşağıda); karar kaydı K-1625 U'da |
| A2 KK2 (para/güvenlik) | ✓ | Yukarıdaki KK2 tablosuna A2 satırları eklendi; çıkış yolu ve geri alma aynı: test dosyalarının PR'ı geri alınır |
| A2 KK3 (D-1723) | ✓ | chromium, firefox, webkit ayrı çağrılar, betikli ve betiksiz |
| A2 `sik-fail-nedenleri` (1) bekçi/tarama istisnası | ✓ | Tarama ya da bekçi değişmedi |
| A2 `sik-fail-nedenleri` (2) çağrı biçimi | ✓ | Yeni tarama yok |
| A2 `sik-fail-nedenleri` (3) 06 default/FK | ✓ | Şema değişmedi |
| A2 `sik-fail-nedenleri` (4) aynı sayıyı anan yerler | ✓ | Kural, sayaç ve şablon değişmedi; backlog sayacına dokunulmadı |
| A2 `sik-fail-nedenleri` (5) belge kuralı / karar kaydı | ✓ | 05/07/11/09 metni değişmedi; yeni karar yok (D-1374/D-1389 kararları U'da, kayıtla) |
| **A3** Kabul 1: A3 kalemleri kapanır ya da gerekçeyle daraltılır | ✓ | Kapandı: D-1005, D-1020, D-1120, D-1130 (`✅ Çözüldü → T604`, kanıt satırda). Yeni: D-1768 (belge yan cümlesi), D-1769 (tür çözümlemesiz son artık), D-1770 (`package-build.sh:167,172`). `grep -nE "T604" Docs/DEFERRED_BACKLOG.md Docs/DEFERRED_BACKLOG.d/*.md` ile yedi kalem tarandı |
| A3 D-1005 dört biçim | ✓ | `ExceptionArgumentsUntyped.cs` (satır 14–19), `AnonymousScopes.cs` (8, 9, 10, 12, 16), `ErrorCodeVariables.cs` (10, 11, 13, 14) ve satır içi örnekler |
| A3 D-1020 | ✓ | `ScanTemplates_AnExceptionNotTypedInTheFile_IsReported` (sekiz örnek: sabit adlı şablon, `var` şablon, `ex.Message` şablon yerinde, `ex.ToString()`, diğer-dosya üyesi, `var` istisna ifadesinden, `…Ex`), olumsuz dokuz örnek |
| A3 D-1120 | ✓ | `test-pre-commit.sh` g1–g17; sekiz satırdan yedisi bekçiden geçer; tek tek denetim: `host-up.sh` 0 (önce 1), `storage-setup.sh` 1 (önce 3; satır 66), `package-build.sh` 2 (önce 7; 167, 172) — kalan üç satır D-1770 |
| A3 D-1130 | ✓ | `test-pre-commit.sh` 240/240; tek satır denetimi sıfır takılma |
| A3 `sik-fail-nedenleri` (1) istisna geniş | ✓ | İstisna yalnız `.sh`, tam sağ taraf, karşı örnek dokuz; `nameof`/`typeof` ve `ExceptionCount` gibi adlar bulgu değil (olumsuz örnek) |
| A3 `sik-fail-nedenleri` (2) çağrı biçimi kaçırma | ✓ | Adlı bağımsız değişken (`exception:`, `message:`), sabit adlı şablon, `var` şablon, iç içe anonim nesne, `Log` ve `Scope` çağrısı örnekli |
| A3 `sik-fail-nedenleri` (3) 06 default/FK | ✓ | Şema değişmedi |
| A3 `sik-fail-nedenleri` (4) aynı sayıyı anan yerler | ✓ | `git-hooks/README.md` dördüncü istisnayı ve `pre-commit` başlık yorumu üçüncü satırı yazar; sayaçlara dokunulmadı |
| A3 `sik-fail-nedenleri` (5) belge kuralı / karar kaydı | ✓ (K-1626 yazıldı) | `09 §18.2` madde 2a'nın yan cümlesi ve K-1626 D-1768'e bırakıldı (belge değişmedi; tarama kodu sözü genişletiyor) |
| **A7** D-1546 | ✓ | `ShareLockText` iletisi + `ShareLockText_NamesTheProductLocksExceptionAndTheOneFormItMayTake_D1546` |
| A7 D-1617 | ~ | T604 U'ya daraltıldı (`src` yorumu; satırda gerekçe) |
| A7 D-1547 | ✓ | `ContentPanelPageTests` (yedi davranış: `TheNewAddress_OfASingleRecord…`, `ASecondAnnouncement…409`, `TheNameCeiling_P27…`, `TheAnnouncementTextCeiling…`, `TheBranchMapLink…`, `TheAboutPageDeletion…422`, `TheBrandForm_WithADeletedLogoOrIcon…`, `TheBrandForm_ShowsTheFallbackTexts…`, `TheBranchImage_IsWritten…`, `TheOpenForm_AfterAPublishMove…409`) + `PanelContentEditingTests.ABranchImage…D1547`; iki sınıf 43/43; değişinim iki testi kırdı |
| A7 D-1768 | ✓ | `09` §18.2 madde 2 (a) yan cümlesi, v0.37 başlık ve altbilgi, `PRODUCT_DISCOVERY_STATUS` 09 satırı ve K-1626 satırı |
| **A5** D-146 | ✓ | `NoticeParameterSourceScanTests.OnTheUnquotedReadSample…_D146`, `EachUnquotedReadForm_IsOneFinding_D146` (8 biçim), `TheNearForms_AreNoFinding_D146` (7 yakın biçim); gerçek kaynak taraması temiz |
| A5 D-1226 | ✓ | `ControlSourceScanTests.ATypedLimit_IsFound_KK10` (6 yeni min/max satırı), `ALimitFromTheConstants_IsQuiet_KK10` (3 yeni) |
| A5 D-1285 | ✓ | `OperationSourceScanTests`: 3 ihlal + 3 temiz yeni runner satırı, 3 durum üyesi dizgesi ihlal, 2 düz yazı temiz; `ApplicationSource_EveryGatedHandlerReachesTheGate`, `TheSamplePage_…` gerçek dosyada yeşil |
| A5 D-1329 | ~ | Tarama bitti (`Scan_OfASetThatRootsTheWrite_IsAFinding_…_D1329` 4 satır, 2 sessiz satır); `CompanySettingsWriter` sadeleşmesi T604 U'ya daraltıldı |
| A5 D-1614 | ✓ | `DirectRequestReadScanTests` (gerçek dosya, eskimiş kayıt, örnek 6 okuma, başka dosya/sayı, kapsam) |
| A5 D-1627 | ✓ | `Scan_OfALongTextOfOpenParenthesesAndWhereWords_FinishesWithoutATimeout_D1627`; sınıf 16/16 + yeni |
| A5 D-1657 | ✓ | `PanelScreenThatStopsReadingTheSalesGate_IsReported_D1657`, `AKnownPanelGateGap_ThatThePageHasClosed_IsReportedAsStale_D1657`; D-1778 |
| A5 D-1661 | ✓ | `TheCustomersPage_OffersExactlyTheOpenOperations_InEveryPairOfTheRows_KK2` (2/2); D-1779 |
| A5 D-1626 | ✓ | `NavigationConsistencyTests`: `FormActionChanged_ToAnotherScreen_IsReported`, `ButtonFormAction_…`, `FormPageTagHelper_…`, `HandlerOfAnotherScreen_ByALiteralPath_…`, `…_ThroughALocal_…`, `ResponseRedirect_WrittenStraightOnTheResponse_…` (adres ikinci bağımsız değişken), `FormsPostingToTheirOwnHandler_AreNoSite_…`; `AnExemptSite_WhoseAddressIsAVariable_…` yeni `FormAction` muafiyetlerinin bağlarını da ister; kalan iki yazım D-1780 |
| A5 D-1636 | ✓ | `OutcomeScreens_OfTheCode_AreTheOnesTheInventoryPins`, `OutcomeScreen_ChangedToAnotherScreen_IsReported` ×3 + `…OfTheConfirmation_…`, `OutcomeScreen_OfANewOperation_WithoutAPin_…`, `TheLineThatGivesAnOutcomeScreenItsValue_Changed_…` ×3; muafiyet gerekçesi `04` 2.7.3.3 |
| A5 D-1656 | ✓ | `FormFieldEnforcementTests.EveryCodeCheck_OfAField_IsHeldByALineOfTheTable` (gerçek kod), `CodeCheck_AddedWithoutALineOfTheTable_…` (nameof, dizge, sabit, kod önde: dört yazım), `…_ChangedToAnotherCode_OrAnotherField_…`, `LineOfTheTable_Removed_…`, `CodeCheckExemption_ThatIsNotInTheCode_OrIsHeldByALine_…`; satır → kod yönü mevcut `EnforcementNotInCode` |
| A5 D-1671 | ✓ | `FormFieldPlaceEnforcementTests.DateRead_ChangedToAConstant_ReportsThePlaceThatReadsIt` ×3, `ArrivalDateRead_OfTheReceivedReturn_…`, `LinesOfTheDateRows_HoldTheConditionThatReadsTheField` |
| A5 `04` 2.7.3.3 / karar kaydı | ✓ | Belge kuralı değişmedi (D-1636 yalnız muafiyet gerekçesini belgedeki 2.7.3.3 metnine bağladı); `09` metni değişmedi → karar kaydı gerekmedi |
| A5 `sik-fail-nedenleri` (1)(2) istisna geniş / çağrı biçimi | ✓ | Muafiyetler tam biçimli ve karşı örnekli (`min="1"`, `progress max="100"`, `ContainsKey`, 11 kayıtlı okuma listesi ad+sayı ile); yeni tarama biçimleri 3–8'er örnek |
| A5 (4) aynı sayıyı anan yerler | ✓ | `09` §8.3 görünüm cümlesi zaten "türleri ile üyeleri" der (kod söze yetişti); başka belge sayısı değişmedi |
| A5 (9) eşitlik denetimi iki yönlü | ✓ | D-1614 listesi ve `PanelGateReadGaps`, `NeverOpenForTheCustomer` ikisi de iki yönlü kırılır; ters yön testleri var |
| A2 12 satırı durumları | ⚠ | `VAL-A351`, `VAL-C075`, `VAL-A306`, `VAL-A308` BEKLEMEDE bırakıldı: kanıt testleri var, PASS yazımı ve uygulama kaydı payı faz kapısı/`12` sahibinde |
| **A4** Kabul 1: A4 kalemleri kapanır ya da gerekçeyle daraltılır (önceki işçinin yaptıkları dahil) | ✓ | Kapandı: D-1678, D-1704, D-1710, D-1746, D-1228, D-1418, D-1433, D-1455, D-1456, D-1709, D-1732 (hepsi `✅ Çözüldü → T604`, kanıt satırda). Daraltıldı: D-1736 (d) Firefox sınıf süresi ve CI E2E → F6 gate (gerekçe satırda); D-1789 → F6 gate (yeni ölçüt tasarımı); yeni D-1788, D-1790 → T604 U. `grep -nE "T604" Docs/DEFERRED_BACKLOG.md Docs/DEFERRED_BACKLOG.d/*.md` ile on iki kalemin her biri tarandı: ⚪ ve hedefi T604 kalan A4 kalemi yok |
| A4 D-1228 | ✓ | `NarrowClassTests.TheContactPage_...D1228` (VAL-B037, A853, A120, A121, A122; 320/768/1280), `TheFaqPage_...VALB067`; 12 satırı `Durum` yazılmadı (gözlem kaydı ekranlı: `screenshots/VAL-*`) |
| A4 D-1418 | ✓ | `NarrowClassTests.TheReauthenticationScreen_...D1418_VALB078` |
| A4 D-1456 | ✓ | `NarrowClassTests.TheAccountScreen_...D1456`; sapma D-1790 (src) |
| A4 D-1433 | ✓ | `TheRequestAnswered429_WritesNoLinkAndNoDispatchRow_VALA498_D1433`; paylı satır tablosu |
| A4 D-1455 | ✓ | Paylı satır kanıtları tablosu (T517 payları) |
| A4 D-1709 | ✓ | `ExecutableOverridesTests` + `12` v0.37 §4.17 (3) + K-1630 |
| A4 D-1732 | ✓ | (a), (b) kodda kapalı (`WcagCriteria.SourceScan`, `ResultOf`), (c), (d) `EngineNotes`, `Facts.skip` |
| A4 D-1736 | ~ | (a), (b), (c) yapıldı; (d) ileti metni yapıldı (`AuditScriptsTests.Reach_...D1736`), Firefox sınıf süresi ve CI E2E F6 gate'e (kısmen karşılanan kabul: PASS sayılmaz, satır açık ve yeni hedefli) |
| A4 Kabul 3 / KK3: üç motor, iki yol; ölçüt kaydı tek çağrıda eksiksiz | ~ | chromium ve firefox betikli ve betiksiz yeşil (`NarrowClassTests`, `AuditScriptsTests`); ölçüt kaydı (Audit + Scan + Criteria + Record + ScreenInventory) chromium betikli tek çağrıda 27/27; webkit ve firefox'ta ölçüt kaydı sınıf grubu ve 590 sn ölçümü CI E2E'ye (TAM: CI E2E) |
| A4 Kabul 4 / KK4: D-1678 | ✓ | `MigrationBundleTests` (varsayılan 5 dk, 9 geçersiz değer, 3 geçerli değer) |
| A4 Kabul 5 / KK5: satırlar | ✓ | sayaca dokunulmadı; kapanan satırlar `✅ Çözüldü → T604`, daraltılanlar yeni hedef ve gerekçeyle |
| A4 KK2: para-güvenlik | ✓ | D-1433 `VAL-A498` (kimlik/oturum bağlantısı): ayırt edicilik değişinimle gösterildi (yukarıda); eski testler değişmedi, Unit TAM ve ilgili Integration sınıfları yeşil; geri alma: test dosyalarının PR'ı |
| A4 `sik-fail-nedenleri` (1) bekçi/tarama istisnası geniş | ✓ | Tarama ya da bekçi değişmedi; `BrowserWaitScanTests` yalnız örnek taşıdı |
| A4 (2) çağrı biçimi kaçırma | ✓ | Yeni tarama yok |
| A4 (3) 06 default/FK | ✓ | Şema değişmedi |
| A4 (4) aynı sayıyı anan yerler | ✓ | `12` v0.37: üst bilgi, altbilgi ve `PRODUCT_DISCOVERY_STATUS` 12 satırı birlikte; MEMORY'ye dokunulmadı (kural); karar kaydı K-1630 |
| A4 (5) belge kuralını değiştiren 09 metni karar kaydısız | ✓ | `12` §4.17 (3) kural değil koşu biçimi; yine de K-1630 yazıldı; `09` değişmedi |
| A4 gerçek saate bağlı test yok | ✓ | Yeni testler bekleme içermez (`BrowserWaitScan` temiz); odak okuması Tab sonrası anlık |

### A6 öz denetim tablosu

| Madde | ✓ | Kanıt |
|---|---|---|
| Kabul 1: hedefi T604 olan A6 kalemleri kapanır ya da gerekçeyle daraltılır | ✓ | `grep -nE "T604" Docs/DEFERRED_BACKLOG.md Docs/DEFERRED_BACKLOG.d/*.md` ile tarandı: kapandı D-1140, D-1244, D-1311, D-1348, D-1363, D-1366, D-1397, D-1452, D-1677, D-1779, D-1780; D-1770 yaması hazır ama commit bekçiye takıldı (satır ⚪); daraltıldı D-137 (→ T604 U), D-894 (→ T604 U, D-1525 ile); D-1471 kapandı (A6 devamı: VAL-B040/B041 tarayıcı gözlemi); D-1798 ve D-1753 kapandı (A6 devamı); D-1778 satırı yazıldı |
| Kabul 2 / KK2: para-güvenlik | ~ | D-1770: kırmızı (HEAD betikleri çıkış 1) ve yeşil (yamalı çıkış 0) gösterildi ama yama dalda değil (bekçi engeli); öteki A6 kalemleri para-güvenlik değil |
| Kabul 5 / KK5: satırlar | ✓ | kapanan satırlar `✅ Çözüldü → T604 (2026-11-21; A6)`, daraltılanlar yeni hedef ve gerekçeyle; sayaca dokunulmadı |
| `sik-fail-nedenleri` (1) bekçi/tarama istisnası geniş | ✓ | Bekçi değişmedi; `NavigationOutsideTheMapTests` yalnız dosya ve sayıyla sabitler, istisna açmaz |
| (2) kaynak taraması bir çağrı biçimini kaçırıyor | ✓ | Yeni tarama kaçırma örneği: `ANewStringLinkOrScriptExit_IsFoundByThePatterns_WhateverItsQuoting` (kaçışlı, ham, birebir dizge; `assign`, `replace`, `href =`; `==` karşılaştırması sayılmaz) |
| (3) 06 default/FK | ✓ | Şema değişmedi |
| (4) aynı sayıyı anan öteki yerler | ✓ | `04`, `07`, `09`, `11`, `12` üst bilgi, altbilgi ve `PRODUCT_DISCOVERY_STATUS` satırları birlikte; K-1632 satırı; MEMORY'ye dokunulmadı; Consistency 696/696 |
| (5) belge kuralını değiştiren 09 metni karar kaydısız | ✓ | `09 §7.2` m.3 K-1632 ile; kodda var olan kuralı yazar, yeni ürün kuralı yok |
| Eşitlik denetimleri iki yönlü (sık FAIL 9) | ✓ | `FormField*` tabloları iki yönlü (hücre değişince kırmızı, tablo eşitlenince yeşil); `StateRolePageTests.NeverOpenForTheCustomer` iki yönlü kaldı |
| Gerçek saate bağlı test yok | ✓ | Yeni testlerde bekleme yok; D-1366 düzeltmesi zaman sınırını yükseltir, hızı sınamaz |

## A parçasının kapanışı (A1–A7, bütün kalemler)

A parçası yalnız test, tarayıcı aracı, betik ve belge dosyası değiştirdi; `src` değişmedi (A6 devamındaki iki geçici CSS ve bağlantı bozması değişinim içindi, `git checkout` ile geri alındı; son diff'te `src` yok). Daraltılan kalemler `T604 U` + gerekçe ile ürün kodu parçasına (dal `task/T604-urun-kodu`) gider. Aşağıdaki tablo A1–A7'nin bütün kalemlerini taşır; sonuç sütunu backlog satırından okunmuştur (`grep -nE "T604" Docs/DEFERRED_BACKLOG.md Docs/DEFERRED_BACKLOG.d/*.md`).

| Dilim | Kalem | Sonuç | Gerekçe / kanıt (satırın başı) |
|---|---|---|---|
| A1 | D-697 | Kapandı | VAL-C109 (1 dakikalık boşluk kesinti sayılmaz, boşlukta dolan iptal aynı uyanışta işlenir) `SchedulerScanTests.Scan_AHeartbeatOneMinuteOld_IsNoOutage_ |
| A1 | D-981 | Kapandı | `VAL-C090`, `C091`, `C080`, `C002` (A1 ilk dilim: `CardRefundJobTests`, `PaymentNotificationTests`) ve `VAL-C088` `PaymentWindowTests.UnansweredFinalQ |
| A1 | D-1167 | Kapandı (VAL-A371 tarayıcı payı D-1753 ile kapandı) | KP-76'nın beş KRİTİK satırı gerçek ekranlarda iki oturumla sınandı — `VAL-A371`, `A372`: `ProductFormPageTests.TwoAdministratorsOnTheSameProduct_..._V |
| A1 | D-1283 | Daraltıldı → T604 U | T604 A1 (2026-11-18), daraltıldı → T604 U: (1) olay → alan açılışı `CustomerOperationsTests.TheFailedCardRefund_SetsTheCardRefundNotRealizedFact_AndTh |
| A1 | D-1284 | Kapandı | `CustomerOperationsTests.TheItemlessIbanFields_KeepOnlyTheOrdersOwnRefunds_...D1284`: iki kalemsiz alanlı sipariş, başka siparişin geri ödemesine bağl |
| A1 | D-1368 | Kapandı | `OrderOperationScreenTests.UnderTheMembersRoot_AnotherAccountsOrderIs404_AndAnAnonymousRequestGoesToTheSignIn_...D1368_VALA693` (başka hesap 404, otur |
| A1 | D-1424 | Kapandı | (a) `DataExportTests.TheScreenAndTheThreeDownloads_GiveNoFileAndWriteNoRow_ToAnAnonymousRequest_OrToACustomerSession_D1424_VALA271` (GET ve üç POST, o |
| A1 | D-1494 | Kapandı | `ScriptlessFlowTests.ThePaymentScreens_ShowTheIban_TheReceivedSentenceAndTheAwaitedCard_InBothWays_VALB009_VALB010_VALB011` — havale teyit ve sipariş  |
| A1 | D-1599 | Daraltıldı → T604 U | T604 A1 (2026-11-18), daraltıldı → T604 U: çatışmada üst belge kazanır: `02 §10.4.10` ("taşıdığı IBAN denetimden geçmiyorsa kayıt IBAN'sız yapılır"),  |
| A1 | D-1676 | Kapandı | `PayTrPaymentGatewayTests.Query` yasak değeri ve yanıttaki net tutar "9,7631" yapıldı (virgül JSON zaman damgasında bulunamaz); Unit TAM 8088/8088 / — |
| A1 | D-1725 | Kapandı | `ScriptlessFlowTests.FillCheckoutAsync` yanıltıcı fatura değerlerini betikli yolda da gönderir (gizli alanlara doğrudan değer yazılır); değişinim (`Ad |
| A1 | D-1753 | Kapandı | `ConcurrentEditTests.TwoAdministratorsOnTheSameProduct_TheSecondOneSeesTheWarningInThePage_AndNothingIsOverwrittenBeforeTheConfirmation_VALA371_D1753` |
| A2 | D-464 | Kapandı | davet bağlantısının dört geçersizleşme anı (geri çekme, aynı adrese yeni davet, gönderenin kaldırılması, kabulde adresin yönetici olması — K-1085) dav |
| A2 | D-572 | Kapandı | engelin pencere dolunca sayfa düzeyinde kalkması üç sayfada sınandı — giriş `CustomerSignInPageTests.TheBlock_LiftsWhenTheAttemptsLeaveTheWindow_TheRi |
| A2 | D-801 | Kapandı | kalan 429 uç yanıtı payı kapandı — `/giris` L-1 429'unda `Retry-After` yok ve yalnız nötr cümle (`CustomerSignInPageTests.TheLimit429OfTheSignIn_HasNo |
| A2 | D-1289 | Kapandı | dört davranış ve iki satır sayfa düzeyinde kalıcı testte — (1) yaygın şifre sıfırlamada ve davet kabulünde 422, bağlantı tüketilmez (`PanelAccessPages |
| A2 | D-1301 | Kapandı | `AdminEmailChanges` iki eşzamanlı tüketim testi — iki basış hesap satır kilidinde birlikte bekletilip sırayla bırakılır; yeni adres doğrulaması (`Pane |
| A2 | D-1374 | Daraltıldı → T604 U | T604 A2 (2026-11-18), daraltıldı → T604 U: karar (kurala uygun, ⚠ raporda): `04` 6.3.12.2 ve K-811'in gerekçesi doğrulanmamış kaydın talep üzerine pan |
| A2 | D-1389 | Daraltıldı → T604 U | T604 A2 (2026-11-18), daraltıldı → T604 U: karar (kurala uygun, ⚠ raporda): `05` 6.1.5 (K-971) izni ilk sayfa isteğinde düşürür; izin düşürme tek bir  |
| A2 | D-1394 | Kapandı | üç bağlantı durumu davet sayfasında sınandı (geri çekilmiş, aynı adrese yenisiyle geçersizleşmiş, göndereni kaldırılmış; dördüncü olarak K-1085 adres  |
| A2 | D-1405 | Kapandı | (1) `/kayit` yüzü — Google tanımsızken düğme yok, `StartSocialLogin` 422, akış çerezi yok; tanımlı ve kapı açıkken düğme var, 303 → Google (`code_chal |
| A2 | D-1723 | Kapandı | E-21 yeni e-posta ve Google teyidi, E-28 geri alma, E-54 doğrulama ve geri alma düğmeleri tarayıcıda gönderilir; değişiklik yazılır, bağlantı kullanıl |
| A3 | D-1005 | Kapandı | dört biçim metin taramasına eklendi — (1) istisna: adı `…Exception` ya da `…Ex` ile biten alan/üye ve istisna ifadesinden `var` ile alınan değişken (` |
| A3 | D-1020 | Kapandı | `LogTemplateSourceScan.ExceptionArguments` sabit dizgeli bağımsız değişken yoksa şablonu istisna yuvası olmayan ilk bağımsız değişken sayar (sabit adı |
| A3 | D-1120 | Kapandı | `scripts/git-hooks/pre-commit` `is_secret_assignment` dördüncü dar istisna — yalnız `.sh` dosyasında sağ taraf tamamıyla `$(cat <yol>)`, `$AD` ya da ` |
| A3 | D-1130 | Kapandı | seçenek (a): `scripts/git-hooks/test-pre-commit.sh` `-e`/`--env` örneklerinin anahtar adı ve değeri parçalardan kurulur (`K_API`, `K_DBP`, `K_DER`, `V |
| A3 | D-1768 | Kapandı | `09` v0.37 §18.2 madde 2 (a)'ya yan cümle yazıldı (şablon sabit adla ya da değişkenle verilse de şablon yerindeki istisna ve anonim üye adları aynı ku |
| A3 | D-1769 | Açık (hedef sahip) | T604 A3 (2026-11-18): kayıt maskeleme taramasının tür çözümlemesi gerektiren son artığı (DÜŞÜK; bugün `src/`'de örneği yok). D-1005'in dört biçimi met |
| A4 | D-1228 | Kapandı | `NarrowClassTests.TheContactPage_ShowsItsContentInTheClassOfEachWidth_D1228` (VAL-B037 üç sınıfta yerleşim, VAL-A853 kimlik bloğu altbilgide ve sayfad |
| A4 | D-1418 | Kapandı | `NarrowClassTests.TheReauthenticationScreen_IsOneColumnWithNothingHidden_D1418_VALB078` (320, 768 ve 1280'de tek sütun, bütün içerik görünür, yatay ta |
| A4 | D-1433 | Kapandı | `VAL-A498` 429 iddiası `TheRequestAnswered429_WritesNoLinkAndNoDispatchRow_VALA498_D1433` ile testli (429'da bağlantı ve kuyruk satırı artmaz, yalnız  |
| A4 | D-1455 | Kapandı | paylı `12` satırlarının T517 payı kanıtları `T604_A_REPORT.md` 'Paylı satır kanıtları' tablosunda (her paya test adı); PASS okuması T604 doğrulayıcısı |
| A4 | D-1456 | Kapandı | `NarrowClassTests.TheAccountScreen_KeepsItsNavigationAndItsLastSectionInEveryClass_D1456` (dar ve orta sınıfta yatay sekme, geniş sınıfta dikey liste, |
| A4 | D-1678 | Kapandı | ✅ Çözüldü → T604 A4 (2026-11-21): süre sınırı `SHOPFOLIO_TEST_BUNDLE_BUILD_TIMEOUT_SECONDS` ile (pozitif tam sayı saniye) yükseltilir; geçersiz değerd |
| A4 | D-1704 | Kapandı | ✅ Çözüldü → T604 A4 (2026-11-21): `BrowserWaitScan` + `BrowserWaitScanTests` (Unit) `Browser` klasöründe istisnasız sabit süreli beklemeyi (Task.Delay |
| A4 | D-1709 | Kapandı | araç: `SHOPFOLIO_BROWSER_EXECUTABLES` ile motorun önceki büyük sürümü aynı araçla koşar (`ExecutableOverrides`), kayıt sürümü yazar; belge: `12` v0.37 |
| A4 | D-1710 | Kapandı | ✅ Çözüldü → T604 A4 (2026-11-21): tohum marka adı kurar (`BrowserSeed.BrandName`), yürüyüş başlığın ürün adıyla bittiğini ölçer (`TitleMeasure`); FAQ  |
| A4 | D-1732 | Kapandı | (a) kanıt adı `WcagCriteria.SourceScan` gerçek sınıfı (`AccessibilityRecordTests.TheSourceScan...`) anar ve (b) ölçüt sonucu matristen türer (`ResultO |
| A4 | D-1736 | Açık (hedef F6 gate) | T604 A4 (2026-11-21) KISMEN: (a) ScanStep ekran × ölçüt hücresine girdi (kırmızı tarama hücreyi kırmızı yapar, taranmamış hücre "–"), (b) koşu sonunda |
| A4 | D-1746 | Kapandı | ✅ Çözüldü → T604 A4 (2026-11-21): (a) `ProbeAsync` cevabı okumadan önce stil dosyalarının yüklenmesini bekler (`WaitForStylesAsync`); (b) `AuditScript |
| A4 | D-1788 | Daraltıldı → T604 U | T604 A4 (2026-11-21): FAQ (E-09) ve iletişim (E-10) sayfalarının sekme başlığı marka adını taşımıyor. `StorefrontHeads.Faq` ve `.Contact` belgede "say |
| A4 | D-1789 | Açık (hedef F6 gate) | T604 A4 (2026-11-21): elle denetimin "aynı adlı bağlantı başka adrese gidiyor" okuması bağlam ölçüsü olmadan sonuca bağlanamadı. `AuditScripts.Names`  |
| A4 | D-1790 | Daraltıldı → T604 U | T604 A4 (2026-11-21): hesap ekranında (E-25) geniş sınıfta iç gezinme solda değil, bölümlerin üstünde tam genişlikte. `04` 5.25.18: "geniş sınıfta iç  |
| A5 | D-146 | Kapandı | `NoticeParameterSourceScan` üç tırnaksız okuma yolunu bulgu sayar — işleyici parametresi `notice` (`NoticeHandlerParameterOutsideTheWriter`), `Support |
| A5 | D-1226 | Kapandı | (1) `ControlSourceScanTests.TypedLimits` `min`/`max` sayı niteliklerini de bulur; yalnız `min="1"` (sıra alanı) ve `<progress max="100">` (yüzde) tam  |
| A5 | D-1285 | Kapandı | (a) görünüm taraması durum üyelerini tam dizge olarak da bulur (`"Shipped"`, `case "Paid":`; düz yazıdaki sözcük bulgu değil); (b) `OperationSourceSca |
| A5 | D-1329 | Daraltıldı → T604 U | A5 (2026-11-20), daraltıldı → T604 U: tarama tarafı bitti — `VersionedSideBulkWriteScan` kümeyi zincirin kökü sayar (çağrıdan önce ya da `CurrentState |
| A5 | D-1614 | Kapandı | `DirectRequestReadScan` sayfa modellerindeki her `Request.Form`/`Request.Query` okumasını (indeksleyici, `ContainsKey`, `TryGetValue`, `ReadFormAsync` |
| A5 | D-1626 | Kapandı | `NavigationSites` form hedeflerini (`<form action>`, `formaction`, `asp-page`) ve `FormResults.WriteSeeOther`'ın adresini (ikinci bağımsız değişken) s |
| A5 | D-1627 | Kapandı | `LinkKeySourceScan.MethodHeader` geri izlemesiz (atomik gruplar; anlam aynı); `Scan_OfALongTextOfOpenParenthesesAndWhereWords_FinishesWithoutATimeout_ |
| A5 | D-1636 | Kapandı | muafiyetin gerekçesi `04` 2.7.3.3'e dayandı (sonucu belirsiz işlemde hata kurtarma yolu; harita satırı değil) ve her `OutcomeScreen()` gövdesi `Naviga |
| A5 | D-1656 | Kapandı | `FormFieldCodeCalls` kaynaktan alan adını (`nameof`, `...Fields.X` sabiti, `...FieldName` yerel, `FieldError(`/`Rejection(` ilk dizgesi) ve tür deneti |
| A5 | D-1657 | Kapandı | `StateRoleCheck.Gates` ters yönü: `04` 6.1.5.2'nin saydığı panel ekranı kapıyı okumazsa `GateReadMissing`; gerçek kodda E-45 ve E-46 sayfaları okumuyo |
| A5 | D-1661 | Kapandı | müşteri durum x rol testi işlem başına "hiç açılmadı" sayar (`openedForCustomer`); hiçbir çiftte açılmayan işlem kırar, bilinen üç işlem açık listede  |
| A5 | D-1671 | Kapandı | işlem satırı parçalarına tarihi okuyan koşul katıldı — 7.2.8.1 (iki yer), 7.2.8.3 (düzeltme yolu) `if (!DateOnly.TryParseExact(input.<Alan>, …)) { ret |
| A5 | D-1778 | Daraltıldı → T604 U | T604 A5 (2026-11-20; satır A6 devamında yazıldı): E-45 (ödeme yöntemleri) ve E-46 (kargo ve varsayılanlar) sayfaları satış kapısını okumuyor. `04` 6.1 |
| A6 | D-137 | Daraltıldı → T604 U | T604 A6 (2026-11-21), daraltıldı → T604 U: `NotMarkedYet`'te kalan dokuz kullanım durumunun işaretlenmesi (dokuz işleyicide `[OutcomeUncertain("UC-nn" |
| A6 | D-894 | Daraltıldı → T604 U | T604 A6 (2026-11-21), daraltıldı → T604 U (D-1525 ile): `12` VAL-A239/VAL-A244 hizası D-1525'in kod seçimine bağlıdır (formda kapalı liste ya da yazıc |
| A6 | D-1140 | Kapandı | `Model` ad alanı yeniden koşuldu: dört tam koşu (1457/1457 yeşil; taze Postgres 18, `track_commit_timestamp=on`) ve bir yarım (benim süzgeç zaman aşım |
| A6 | D-1244 | Kapandı | K-1632: okuma 'seçilemez' (yalnız taslak varyantın kullandığı değer açılmamış kombinasyon gibi devre dışı görünür; `02 §3.3.3`); `12` VAL-A024 hücrele |
| A6 | D-1311 | Kapandı | tekrarlı koşu D-1140 ile aynı: `Model` ad alanı dört tam koşuda 1457/1457 yeşil, kararsız test yakalanmadı; testin adı yok. Bilinen tek kırmızı D-1366 |
| A6 | D-1348 | Kapandı | `11 §7.1` eşlemesi: KP-50 satırına T527 payı (`OrderListPageTests`, #565) eklendi. `VAL-A258`/`VAL-A264` payları: sayaç bağlantıları `TheHomeCounters_ |
| A6 | D-1363 | Kapandı | saf kural parçalarının birim testi `FirmSettingsPureRulesTests` (27 sınama): `FirmSettings.RequiredName` (kırpma, boş ad `FieldRequired` reddi, ret bi |
| A6 | D-1366 | Kapandı | inceleme: testte gerçek bekleme ya da saat yok; zaman bağımlılığı göçlerin komut zaman aşımıdır (sürücü varsayılanı 30 sn; yükte tek kırmızı 47 sn sür |
| A6 | D-1397 | Kapandı | K-1632: `09 §7.2` madde 3 net miktarın üçüncü kuralını yazar; `07 §6.2.2` `InvalidAmount` satırı net miktarı anar (kod tutarınki, mesajı kendi kalıbı) |
| A6 | D-1452 | Kapandı | K-1632: `04` 5.25.19 `AccountTexts`'in kaynağı sözünü vermeyen cümlelerini sabitler (form düğmeleri, şifre değişti ve şifre yok, bekleyen satır, geri  |
| A6 | D-1471 | Kapandı | paylı satırların payları kanıtlandı (`T604_A_REPORT.md` 'Paylı satır kanıtları (A6)'): `VAL-A059`, `VAL-A603`, `VAL-A554`; `Durum` yazılmadı. `VAL-B04 |
| A6 | D-1677 | Kapandı | `04` 7.2.4.15, 7.2.8.3, 7.2.10.3, 7.2.11.1, 7.2.11.2, 7.2.12.2 hücreleri tür kimliğini anar (K-1632, `04` v0.57); `FormFieldRuleClauseTable` kimliksiz |
| A6 | D-1770 | Açık (hedef T604; yama hazır, commit D-1120 bekçisi main'e inince) | T604 A6 (2026-11-21), yama hazır, commit edilemedi: seçenek (a) uygulandı ve doğrulandı (`storage-setup.sh` `ADMIN_USER`, `package-build.sh` `APP_DSN` |
| A6 | D-1779 | Kapandı | `ReopenDefectClaimByCustomer`: yeni `Records.ResolvedClaim` tohumu (on gün önce teslim edilmiş kalem, beş gün önce çözülmüş ayıp talebi) işlemi açar,  |
| A6 | D-1780 | Kapandı | ikinci seçenek: `04` §3 girişi 'Taramanın kapsamı' sınırını yazar (K-1632); `NavigationOutsideTheMapTests` iki yazımı dosya ve sayıyla sabitler (C# di |
| A6 | D-1798 | Kapandı | `PendingCountersAndListsTests.EachOfTheSixCounters_EqualsTheRowsOfTheListItsLinkOpens_AndTheRowsAreTheSeededRecordsOfThatWork_D1798_VALA258` (taze ver |
| A7 | D-1546 | Kapandı | belge payı F5 kapısında kapanmıştı (`09 §10.2` m.5–m.6, K-1621); `ConcurrencySourceScan.ShareLockText` iletisi artık `ProductLocks.cs`'in yalnız `FOR  |
| A7 | D-1547 | Kapandı | yedi davranışın hepsi kalıcı testte — `ContentPanelPageTests` (tekil kaydın adresi 303, ikinci duyuru 409 ve forma bağlanma, P-27 ve 120 tavanları, ha |
| A7 | D-1617 | Daraltıldı → T604 U | T604 A7 (2026-11-18), daraltıldı → T604 U: iki yorum `src/Shopfolio.Web/Pages/Panel/OrderDetail.Refunds.cs` içinde; A parçası yalnız test, betik ve be |

**A'ya düşen ve açık kalan satırlar (`grep T604` denetimi).** Hedefi T604 olan öteki ⚪/🟡 satırların (D-138, D-1134, D-1135… toplam yetmiş civarı) hepsi ürün kodu, belge kararı ya da karar kaydı ister ve ürün kodu parçasının (U) kapsamıdır; A dilimlerinde kalem olarak anılmadılar. A'nın anıp açık bıraktığı satırlar gerekçeyle:

- **D-1770** (hedef T604): betik yaması hazır, makinenin `core.hooksPath`'i main'deki eski sır bekçisini çalıştırdığı için commit edilemedi; yama `helper-state/shopfolio/impl/T604/t604a-d1770.patch`; D-1120 bekçisi main'e inince ayrı işle uygulanır (satırda yazılı).
- **D-1736** (hedef F6 gate): Firefox sınıf süresinin 590 sn altı ölçümü ve CI E2E doğrulaması CI'a bağlı (`TAM: CI E2E`); (a), (b), (c), (d) yapıldı; K-1621 gerekçeli yeniden ileri atma satırda yazılı.
- **D-1789** (F6 gate), **D-1769** (sahip): A4 ve A3'te çıkan yeni gözlemler; kendi satırlarında gerekçeli.
- **D-1788, D-1790, D-1778** (T604 U): ürün kodu (`src`).

**KK payları (A parçası).**

| KK | A parçasının payı | Kanıt |
|---|---|---|
| KK1 (envanter) | A1–A7 kalemleri yukarıdaki tabloda; her biri kapalı ya da gerekçeli (U'ya ya da F6 gate'e daraltılmış) | tablo; `grep T604` denetimi |
| KK2 (para ve güvenlik) | A1, A2, A3 kalemleri 'Para ve güvenlik kalemleri (KK2)' bölümünde; değişinimle kırmızı, eski testler aynı | yukarıdaki bölüm; D-981, D-1424, D-1005, D-1020, D-1120, D-1130, D-1289, D-1301 |
| KK3 (tarayıcı kalemleri, üç motor × iki yol) | D-1723, D-1725, D-1732, D-1746 ✅; D-1736 (c)–(d) yapıldı, Firefox süre ölçümü CI E2E'ye (F6 gate); D-1471 (VAL-B040, VAL-B041) ve D-1753 (VAL-A371) üç motor × iki yolda yeşil | `NarrowClassTests`, `ConcurrentEditTests`, `AccessibilityRecordTests`, `ScriptlessFlowTests` |
| KK4 (D-1678) | `MigrationBundle` derleme süre sınırı `SHOPFOLIO_TEST_BUNDLE_BUILD_TIMEOUT_SECONDS` ile yapılandırılır; varsayılan 5 dk değişmez; geçersiz değerde varsayılan | A4 bölümü; `MigrationBundleTests` |
| KK5 (satırlar ve sayaç) | kapanan satırlar `✅ Çözüldü → T604`, daraltılanlar yeni hedef ve gerekçeyle; yeni satırlar `DEFERRED_BACKLOG.d/T604-A.md`'de (D-1753 ✅, D-1768 ✅, D-1769, D-1770, D-1778, D-1779 ✅, D-1780 ✅, D-1788, D-1789, D-1790, D-1798 ✅); sayaca dokunulmadı (faz kapısı) | `Consistency` testleri yeşil |

**Öz denetim (A1–A7'nin bütün maddeleri; PR öncesi; `12 §6.1` ve `11` T604 kabul kriterleri).**

| Madde | ✓ | Kanıt |
|---|---|---|
| Kabul 1: hedefi T604 olan her A kalemi kod ve test kanıtıyla kapanır ya da gerekçeyle daraltılır | ✓ | yukarıdaki tablo (75 kalem: kapanan ✅, daraltılan T604 U / F6 gate satırda gerekçeli, açık kalan D-1769, D-1770 gerekçeli) |
| Kabul 2: para ve güvenlik kalemleri ayrı başlıkta, kırmızı–yeşil, çıkış yolu ve geri alma, eski testler aynı | ✓ | 'Para ve güvenlik kalemleri (KK2)' bölümü; D-1770 kırmızı (HEAD betikleri çıkış 1) ve yeşil (yamalı çıkış 0) gösterildi ama yama dalda değil (bekçi engeli, ⚠) |
| Kabul 3: tarayıcı kalemleri üç motor × iki yol, 590 sn altı, ölçüt kaydı tek çağrıda eksiksiz | ~ | D-1723, D-1725, D-1732, D-1746 üç motor × iki yol yeşil, kayıt tamlık denetimi (`AddCompletenessCheck`); Firefox sınıf süresi ölçümü ve CI E2E yalnız CI'da (D-1736, F6 gate): kısmi, satırda gerekçeli |
| Kabul 4: D-1678 yapılandırılabilir sınır, varsayılan 5 dk | ✓ | `SHOPFOLIO_TEST_BUNDLE_BUILD_TIMEOUT_SECONDS`; `MigrationBundleTests` (varsayılan, geçersiz ve geçerli değerler) |
| Kabul 5: kapanan satır `✅ Çözüldü → T604`; daraltılan satır yeni hedef ve gerekçe | ✓ | `Docs/DEFERRED_BACKLOG.md` ve `DEFERRED_BACKLOG.d/T604-A.md` |
| KK1–KK5 | ✓ (KK3 ~) | yukarıdaki KK tablosu |
| Mini güvenlik kontrolü | ✓ | yeni sır, kimlik ya da dış bağımlılık yok; tarayıcı testlerinin kaydı maskeli (`DataRecord.Mask`); `src` değişmedi |
| Sık FAIL (1) bekçi/tarama istisnası geniş | ✓ | yeni istisna yok; A6 devamının testleri bekçi değil |
| Sık FAIL (2) kaynak taraması çağrı biçimi kaçırır | ✓ | yeni tarama yok; A5 ve A6 taramalarının kaçırma örnekleri satırlarında |
| Sık FAIL (3) 06 default/FK | ✓ | şema değişmedi |
| Sık FAIL (4) aynı sayıyı anan öteki yerler | ✓ | kayıt sayacı `customers = 3`, `admins = 2` (`ForRecord`) ve yeni oyuncu belgesi aynı değişiklikte; belge sürümleri A6'daki gibi; MEMORY ve sayaç el değmedi |
| Sık FAIL (5) belge kuralını değiştiren 09 metni karar kaydısız | ✓ | A6 devamında belge kuralı değişmedi |
| Sık FAIL: hedefi bu task olan kalem ne kapandı ne ileri atıldı | ✓ | `grep T604` denetimi; D-1471, D-1798, D-1753 kapandı; D-1778 satırı yazıldı |
| Bitiş kapısı: Unit TAM, ilgili Integration, biçim | ✓ | Unit TAM 8267/8267; Integration ilgili sınıflar yeşil (yukarıdaki 'Koşular'); `dotnet format` değişen `.cs`te temiz; **TAM: CI** |

## ⚠ Maddeler

- D-1283 (2): `06 §4.31` IBAN'ı beyan (istek) başına tek satır sayar ("aynı beyanla girilen IBAN'ı birden çok kalem kullanır"); kod kalem kaydı başına yazar. `06` izlenir (kurala uygun seçenek); hizalama `src` (para) değiştirir → T604 U.
- D-1599: üst belge (`02 §10.4.10`, `03 §8.4.8`, `12 VAL-A238`) denetimden geçmeyen IBAN'da IBAN'sız kayıt der; `07` UC-130, `04` 7.2.10.4 ve T528 kodu 422 der. Öneri: `07`/`04` `02`'ye hizalanır, kod değişir; karar kaydı, belge hizası, kod ve `VAL-A238` koşumu T604 U'da (K-1623 numarası kullanılmadı).
- `VAL-A371` tarayıcı gözlemi yapılmadı (D-1753); uç yanıtı ve veri kaydı sınandı.
- D-981 C080/C002, D-1424 testleri var olan davranışın ilk testleridir; değişinimle kırmızı gösterilmedi.
- Üye kökü oturumsuz yönlendirme: backlog "302" diyordu; gerçekte GET 302, POST 303; testler gerçeği sabitler.
- C077 "yönetici önce ödendi" çiftinde müşterinin eski tam-iptal formu `OperationClosed` ile reddedilir (ödenmiş siparişin iptali kalem iptalidir); `02 §3.31.2`'nin "ya uygulanır ya reddedilir ve sebebi söylenir" cümlesine uyar.

- A2 D-1374: karar (kurala uygun seçenek): belge yönü korunur (`04` 6.3.12.2, K-811 gerekçesi), UC-157 doğrulanmamış kaydı da siler; `AccountDeletions.DeleteByAdministratorAsync` ve T533 sayfası değişir → T604 U, karar kaydı K-1625 U'da.
- A2 D-1389: karar: izin düşürme tek panel ara katmanına alınır (D-1454 ile aynı iş), kayıt kapatılmaz → T604 U.
- A2 `VAL-A351`, `VAL-C075`, `VAL-A306`, `VAL-A308` 12 satırları BEKLEMEDE bırakıldı (PASS yazımı ve uygulama kaydı payı faz kapısında); kanıt testleri backlog satırlarında.
- A2 D-1289 `VAL-A306`/`VAL-A308` testleri ve K-899 testi var olan davranışın ilk testleridir; `VAL-A306`/`VAL-A308` için değişinim yapılmadı (ikisi seed ve ret durumlarını doğrudan sınar), K-899 ve öteki testler değişinimle kırmızı gösterildi.
- A2 `/kayit` testleri `CustomerSignInPageTests` içinde (Google taklidi ve `Open(google, gateOpen)` orada); backlog "`CustomerSignInPageTests` ve `RegistrationPageTests`" dediği için yeri iki dosyaya bölündü.
- A2 D-1301 testleri bekletmeyi `pg_stat_activity` ile bekler (var olan KK8 deseni); gerçek saate bağlı bekleme yok.
- A5 D-1657: `04` 6.1.5.2 "Panelde hâl E-31, E-44…E-47'de görünür" der; E-45 (ödeme yöntemleri) ve E-46 (kargo ve varsayılanlar) sayfaları kapıyı okumuyor. Karar (kurala uygun, `src` yasak): boşluk açık listeye ve D-1778'e yazıldı (T604 U); belge ya da kod hangisine hizalanacağı U işçisinin kararı.
- A5 D-1661: müşteri testinin iki işlemi (`WithdrawDigitalItem`, `ReopenDefectClaimByCustomer`) hiçbir çiftte açılmıyor; D-1779 (tohum eksiği), bilinen liste iki yönlü.
- A5 D-1329: tarama konumsal ("küme zincirin kökü"), tür çözümlemesi değil; metin taramasında tür bilgisi olmadığından bu seçildi. Geçici çözümün sadeleşmesi `src` → T604 U.
- A3 D-1768: karar kaydı yeri `PRODUCT_DISCOVERY_STATUS.md` karar tablosu (K-1626) + `09` altbilgisi; U işçisi K-1625'i aynı tabloya ekleyecekse birleştirmede aynı satır bölgesinde çakışma çıkabilir.
- A6 devamı: E-26 için belge orta sınıfı söylemez (`04` 5.26.10 yalnız geniş ve dar); CSS 768'den iki sütundur; VAL-B040 orta sınıfta yerleşimi kaydeder, yalnız kartların tam olmasını ve yatay taşma olmamasını ister (kurala uygun seçenek; belge ya da CSS hizası U işçisinin kararı).
- Önceki işçinin `ContentPanelPageTests.InitializeAsync` değişikliği `images CASCADE` ile `brandings` satırını siliyordu (29/43 kırmızı); düzeltildi.
- A5 D-1626/D-1636: gezinme taraması `<form action>` hedeflerini görür; `?handler=` ile kendi işleyicisine gidenler site değildir (düz yol öneki site kalır). Sınır: `@(FramePaths.Account)?handler=X` gibi bir ifadenin gerçekten kendi ekranı olduğu metinle doğrulanamaz (yalnız düz yol öneki ayrılır); kalan iki yazım D-1780.
- A5 D-1656: karşılanmayan 22 çağrı gerekçeli muafiyette (⚠ listesi): isteğe bağlı satırın koşullu zorunluluğu (7.2.4.13, 7.2.4.6) ve sözcükle kapalı liste (7.2.4.12, 7.2.4.17) satırlara eklenemedi çünkü `04` hücreleri `RuleKindUnmapped`/`RuleMissesAttribute` karşılaştırmasını kırar; bu `04` hücresini düzeltmek ayrı bir belge işidir (karar yardımcıya, kurala uygun seçenek: muafiyet).
- A5 kontrol: `NavigationConsistencyTests` 261/261, `FormField*` sınıfları 117/117.

## Commit & PR

PR: #594 — A1, A2, A3, A4, A5, A6, A7 tamam (A parçasının son dilimi A6 devamı); A6 devamının kod head'i bu rapor satırından sonraki commit
