# T518 — Adres defteri ve sipariş geçmişi (E-26, E-27)

**Faz:** F5 | **Durum:** ✓ Tamamlandı (doğrulama bekliyor) | **Tarih:** 2026-11-13

---

## Yapılan İşler

- Adres defteri `Pages/Account/Addresses.cshtml(.cs)` (`/hesap/adresler`): `GET` üyenin girdilerini eskiden yeniye kart olarak verir; `addressBookId` düzenleme, `create=true` ekleme formunu ekranın içinde açar. `CreateAddressBook`, `UpdateAddressBook`, `DeleteAddressBook` modülün `IMembershipModule` yöntemlerini işlem sınırında çağırır; alan reddi süzgeç yolundan (`ModelState.AddFieldError`) 422 verir, yabancı ya da yok girdi `RejectionLog.Rejected(NotFound)` ile 404'tür; kayıt 303 → `/hesap/adresler` "kaydedildi" bildirimiyle.
- Sipariş geçmişi `Pages/Account/OrderHistory.cshtml(.cs)` (`/hesap/siparisler`): modülün yeni `IOrderingModule.ListMemberOrdersAsync` okuması (`CustomerOrderReader.ListShownAsync`): `OrderVisibility.ShownToCustomer`, `CreatedAt` ve `Id` azalan, sayfa başına 25 (`PageSizes.OrderHistory`), ilk kalem adı ve diğer kalem sayısı tek toplu okumayla; arama, süzgeç ve sıralama yoktur. Panelin `ListCustomerOrdersAsync`'ına dokunulmadı.
- Ortak: `_AccountNav` + `AccountTexts` (hesap iç gezinmesi), `AddressBookTexts` / `OrderHistoryTexts`, `FramePaths.AccountAddresses`, `storefront.css` hesap düzeni / adres kartı / sipariş tablosu stilleri.
- Test örnek sayfası `Pages/Account/Addresses.cshtml` (test derlemesi) gerçek sayfayla çakıştığı için `Sample.cshtml` (`/hesap/ornek`, `AccountSampleModel`) oldu; `SessionAccessTests` yolları buna çevrildi.

## Etkilenen Modüller / Dosyalar

`src/Shopfolio.Web/Modules/Ordering/{Contracts/CustomerOrderRows.cs, Contracts/IOrderingModule.cs, CustomerOrderReader.cs, OrderingModule.cs}`, `Pages/Account/{Addresses,OrderHistory}.cshtml(.cs)`, `Pages/Shared/_AccountNav.cshtml`, `Pages/Components/{AccountTexts,AddressBookTexts}.cs`, `Pages/Frame/FramePaths.cs`, `wwwroot/css/storefront.css`; testler `tests/Shopfolio.IntegrationTests/Web/AccountAddressAndOrderHistoryPageTests.cs`, `Pages/Account/Sample.cshtml`, `Web/SessionAccessTests.cs`, `Pages/AccessSamples.cs`, `tests/Shopfolio.UnitTests/Pages/AddressBookAndOrderHistoryTextsTests.cs`.

## Kabul Kriterleri Kontrolü

Test sınıfı `AccountAddressAndOrderHistoryPageTests` (kısaltma: S).

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | `GET /hesap/adresler` girdileri verir; boşsa boş hâl satırı | ✓ | `S.TheBook_ListsTheOwnEntries...KK1`, `S.AnEmptyBook_SaysThereIsNoSavedAddress...KK1` |
| 2 | `addressBookId` düzenleme, `create` ekleme formu ekran içinde; başkasının girdisi 404 | ✓ | `S.TheAddForm_OpensFromCreateTrue...KK2`, `S.AnotherAccountsEntry_AGoneOne...Are404...KK2` |
| 3 | `CreateAddressBook` yazar, 303 + bildirim; teslimat yapılmayan il tutulur | ✓ | `S.AnAdd_WritesTheEntry_Is303...AnUndeliveredProvinceIsKept_KK3` |
| 4 | UC-71/72 422: boş zorunlu alan, listede olmayan il, geçersiz telefon; değerlerle açık | ✓ | `S.TheCreate_RefusesAnEmptyRequiredField...KK4`, `S.TheUpdate_RefusesTheSameThreeReasons...KK4` |
| 5 | `UpdateAddressBook` yalnız sahibine yazar; yabancı/yok 404; sipariş adresine dokunmaz | ✓ | `S.TheUpdate_WritesToTheOwnersEntryOnly...KK5` |
| 6 | `DeleteAddressBook` onaysız siler; ikinci gönderim 404; sipariş adresine dokunmaz | ✓ | `S.TheDelete_RemovesTheEntry...TheSecondIs404...KK6` |
| 7 | Tekrar gönderim ikinci girdi; betikli yolda düğme kilidi | ✓ | `S.ARepeatOfTheCreate_IsASecondEntry_AndEveryFormLocksItsButton_KK7` (`data-js="submit-once"`, `data-busy-label`) |
| 8 | `GET /hesap/siparisler` en yeni önce, 25'lik sayfa; arama/süzgeç yok | ✓ | `S.TheHistory_IsNewestFirst_25APage...KK8` (eşit anda sayfa sınırı, `page=abc`, `page=99999999999999`), `S.TheHistory_SendsTheSameNumberOfCommands...KK8` |
| 9 | Misafir siparişi, müşteri/firma iptali ve ödemesi alınıp iptal edilen listede; hiç ödenmemiş kendiliğinden iptal yok | ✓ | `S.TheHistory_ListsTheGuestOrderThatFellToTheAccount...KK9` |
| 10 | Her satır üye köküne bağlanır; siparişsizde boş hâl | ✓ | `S.TheHistoryRow_NamesTheFirstItem...OpensTheMemberRoot_KK8_KK10`, `S.AMemberWithoutAnOrder_SeesTheEmptyLine...KK10` |

### Öz denetim (bitiş kapısı 2)

Önceki işçinin yaptıkları dahil kriter 1–10, KK1–KK10 ve `09 §25` maddeleri diff'te baştan denetlendi (yalnız kalanlar değil); `sik-fail-nedenleri.md` okundu. Bu turda bulunup giderilenler: (a) Integration sınıfı ilk kez koşuldu, 8 hata çıktı; sipariş kalemi tohumu `download_allowance` kısıtına takılıyordu (testte `DownloadAllowance` verildi), ekleme formunun "gizli girdi yok" denetimi listedeki silme formlarını da saydı (yalnız form bölümüne daraltıldı), geçmiş ekranının "arama/süzgeç yok" denetimi site başlığındaki arama kutusunu da saydı (yalnız ekran içeriğine daraltıldı); (b) `NoticeParameterSourceScanTests` `model="notice"` yerel adını yakaladı, adı `savedMessage` yapıldı; (c) D-437 ve D-693'ün T518 payları kapatıldı.

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kriter 1–10, KK1–KK10 | ✓ | yukarıdaki tablo; sınıf 17 test |
| (1) Bekçi/tarama istisnası geniş | ✓ | yeni istisna eklenmedi |
| (2) Kaynak taraması çağrı biçimi | ✓ | yeni tarama kuralı yok; bildirim taraması yakaladığı adı düzeltti, Architecture ad alanı yeşil |
| (3) 06 "Varsayılan" / FK dizini | ✓ | şema ve göç değişikliği yok |
| (4) Kural değişince aynı sayıyı anan yerler | ✓ | örnek sayfa yolu `/hesap/ornek`e taşındı: `SessionAccessTests`, `AccessSamples` güncel; `FramePaths.AccountAddresses` T517 ile aynı satır |
| (5) Belge kuralını değiştiren 09 metni | ✓ | `09` değişmedi |
| `09 §25` sayfalı liste: `ORDER BY` `Id` ile biter, `Paging.ToPageAsync`, `PageSizes`, sınır testi | ✓ | `CustomerOrderReader.ListShownAsync`; KK8 eşit anda sınır testi |
| `09 §25` kişisel veri taşıyan sayfa `[NoStore]` | ✓ | iki sayfada `[NoStore]`; `S.BothScreens_AreClosedAndNotStored...` |
| `09 §25` yetkilendirme sunucu tarafında | ✓ | `S.BothScreens_NeedAMembersSession...` |
| `09 §25` döngüde sorgu yok | ✓ | `S.TheHistory_SendsTheSameNumberOfCommands...` |
| `09 §25` modül sınırı (sayfada `DbContext` yok, arayüzde `SaveChanges` yok) | ✓ | sayfalar yalnız `Contracts`; Architecture ad alanı 1750/1750 |
| Kapalı ekran (`noindex`, sekme başlığı) | ✓ | `RecordPages.ClosedScreen`; `S.BothScreens_AreClosedAndNotStored...` |
| Hedefi T518 olan backlog (D-437, D-693) | ✓ | ikisinin T518 payı yerinde kapatıldı |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/... --no-build` | birleşik ağaçta 7557 test: 7556 yeşil; tek hata `OutageIndependenceSourceScanTests` yük altında Regex zaman aşımı, sınıf tek başına 12/12 yeşil (T518 dışı) |
| Integration | ✓ | `--filter-class` | `AccountAddressAndOrderHistoryPageTests` 17/17; birleşik ağaçta + `SessionAccessTests` + `StorefrontFrameTests` 80/80; **TAM: CI** |
| Derleme / biçim | ✓ | `dotnet build -warnaserror`, `dotnet format --verify-no-changes --include` | 0 uyarı, 0 hata; biçim temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-14) |
| Bulgu sayısı | 0 bloke eden; 2 K-notu (D-1471, D-1472) |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 2 |
| Kanıt | Kriter 1–10 ve KK1–KK10: testler doğrulayıcı tarafından head `00854fd6` (main `edf4eb2a`'yı içerir) üzerinde yeniden koşuldu — Integration `AccountAddressAndOrderHistoryPageTests` 17/17, `SessionAccessTests` 27/27; Unit TAM 7557/7557 (`OutageIndependenceSourceScanTests` tekrarlamadı); derleme `-warnaserror` 0 uyarı; `dotnet format --verify-no-changes` üç projede temiz; betik testleri 4/4; CI run 43505372903 (Integration TAM dahil) yeşil. Kod okuması: `OrderVisibility.ShownToCustomer` `02 §3.17.8`'in dört kendiliğinden iptal sebebini gizler, Ö1 kaydı olanı gösterir; liste `CreatedAt`, `Id` azalan, `PageSizes.OrderHistory` 25; satır `OrderRoutes.MemberPath`; adres POST'ları sahipliği modülde denetler, ret 404 yazmaz |
| `12` | `VAL-A101`, `VAL-A635` → PASS; paylı satırlar (`VAL-A059`, `VAL-A603`, `VAL-A554`) ve tarayıcı satırları (`VAL-B040`, `VAL-B041`, `VAL-B108`) BEKLEMEDE (D-1471) |
| Uyuşmazlık | Öz denetim tablosu `12` satırlarını eşlemiyor (doğrulamada eşlendi); başka uyuşmazlık yok |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T518-adres-siparis-gecmisi`
- **PR:** #566
- **CI:** yapım işçisi beklemez; yönetici izler
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok. Yeni paket, dış servis çağrısı ve ortam bağımlılığı yoktur.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `_AccountNav`, `AccountTexts`, `FramePaths.AccountAddresses` T517 dalıyla aynı içerikte (⚠ T517 henüz birleşmedi; add/add çakışırsa aynı içerik, main sürümü esas) | — | kayıt değil, birleşme notu |
| 2 | İç gezinmedeki "Hesabım" bağlantısı `/hesap` T517 birleşene kadar 404 (⚠) | — | T517'nin sayfası |
| 3 | Test derlemesinde örnek sayfa `/hesap/ornek` (⚠) | — | gerçek `/hesap/adresler` ile çakışmayı önler |
| 4 | Adres kartında teslimat yapılmayan il için uyarı çizilmedi (⚠) | — | `04 5.26.5`: kısıt ödeme adımında işler |
