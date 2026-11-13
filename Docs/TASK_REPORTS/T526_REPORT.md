# T526 — Kategori ağacı ve kuponlar (E-34, E-35)

**Faz:** F5 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS) | **Tarih:** 2026-11-12

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **E-34 kategori ağacı** (`Pages/Panel/Categories.cshtml(.cs)`): en çok üç seviyeli ağaç, aynı seviyede alfabetik; düğümde doğrudan ürün ve alt kategori sayısı, alt dalıyla yayında ürünü yoksa "vitrin menüsünde görünmüyor" satırı (sunucunun menüsünden, `GetMenuAsync`); üçüncü seviyede "alt kategori ekle" yok; boş ağaç boş hâl satırı ve "Yeni kategori" formu. UC-118…UC-121 işleyicileri T333'ün modül yöntemlerini (`CreateCategoryAsync`, `RenameCategoryAsync`, `MoveCategoryAsync`, `DeleteCategoryAsync`; ağacın danışma kilidi) çağırır; 422 sebebi düğümün yerinde K-799 metniyle ve ret kaydıyla (`CategoryDepthExceeded`, `CategoryNotEmpty`, `FieldRequired`), yok 404, başarı 303 + kısa bildirim.
- **E-35 kuponlar** (`Pages/Panel/Coupons.cshtml(.cs)`): en yeni önce sayfalı liste (`ToPageAsync`, `PageSizes.Panel`; son sayfadan öte istek son sayfayı verir), satırın hâli (`CouponValidity`: Başlamadı / Geçerli / Süresi doldu / Hakkı doldu) tarihten ve kalan haktan `CouponRules.ValidityOf`'ta okunur; form, kullanılmış ve ayrılmış hakları salt okunur ve sürümü taşır; yabancı ya da bozuk anahtar 404.
- **UC-122 `SaveCoupon`** (`Catalog/Coupons.cs`): yeni kupon yazar ya da var olanı değiştirir; değişiklikte önce kuponun satır kilidi (`Catalog/CouponLocks.cs`, `LockKind.Coupon`), sonra sürüm (409, alan kurallarından önce), sonra kurallar (`CouponRules`: değer, %100, asgari tutar, ters tarih, kod tekilliği harf ayrımsız, kullanılmış + ayrılmış hak tabanı). İz yazılmaz (`07` UC-122; K-310). Değişiklik yalnız yeni sepetlere işler; siparişin dondurduğu pay kalır.
- Tek kural evi: yüzde sınırı `Money.MaxPercentage`, tutar/yüzde biçimi `InvalidAmount` / `InvalidPercentage` öznitelikleri; sayfa tutar hesaplamaz (yalnız iletiyi seçer).
- `CouponValidity` kendi dosyasında; `ConcurrencySourceScan` yalnız `Catalog/CouponLocks.cs`'e düz `FOR UPDATE` izni verir.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Pages/Panel/{Categories,Coupons}.cshtml(.cs)`, `wwwroot/css/admin.css`
- `src/Shopfolio.Web/Modules/Catalog/{Coupons,CouponRules,CouponLocks,CatalogModule}.cs`, `Contracts/ICatalogModule.cs`, `Contracts/Coupons/{CouponPanelRecords,CouponRecords,CouponValidity}.cs`
- Testler: `tests/Shopfolio.IntegrationTests/Web/CategoriesPageTests.cs`, `Web/CouponsPageTests.cs`, `Model/CouponTests.cs` (+12), `tests/Shopfolio.UnitTests/Catalog/CouponRulesTests.cs` (+3), `Architecture/ConcurrencySourceScan(Tests).cs`, `Infrastructure/SitemapSourceTests.cs` (sahte modüle yeni yöntemler)
- Kayıt: `Docs/STATUS_CHANGELOG.d/T526.md`, `Docs/DEFERRED_BACKLOG.d/T526.md` (D-1407, D-1408), `IMPLEMENTATION_STATUS.md` tablo satırı, `DEFERRED_BACKLOG.md` (D-333 kapandı)

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Üç seviye, aynı seviyede alfabetik; üçüncü seviyede "alt kategori ekle" yok; boş ağaç boş hâl | ✓ | `TheTree_IsAlphabeticalOnEachLevel_TheThirdLevelHasNoAddChild_AndTheCountsAndMenuLineAreTheServers_T526_KK1_KK2`, `AnEmptyTree_IsTheEmptyRow_WithTheNewCategoryForm_T526_KK1` |
| 2 | Ürün ve alt kategori sayısı, menü satırı sunucunun hesabı | ✓ | `…_T526_KK1_KK2`; `CategoriesModel.CountsTextOf`, menü satırı `GetMenuAsync` üyeliğinden |
| 3 | Ad ve üst kategoriyle açar; adres addan, çakışırsa sayı; danışma kilidi | ✓ | `ACategory_GetsItsAddressFromItsName_ASecondSendOpensASecondOne_WithANumberedAddress_T526_KK3_KK4`; kilit: `CategoryTreeAndListsTests.TwoOpposingMoves_AreInOrderedByTheTreeLock_SoOnlyOneHoldsAndNoCycleForms` (T333, aynı modül yolu) |
| 4 | 422 boş ad / üçüncü seviye; 404 üst yok; tekrar gönderim ikinci kategori | ✓ | `ABlankName_Is422InPlace_AndWritesNothing_T526_KK4`, `AChildUnderTheThirdLevel_Is422_AndAMissingParentIs404_T526_KK4`, `…ASecondSendOpensASecondOne…_KK3_KK4` |
| 5 | Ad değişir, adres değişmez; boş 422, yok 404 | ✓ | `ARename_ChangesTheName_TheAddressStays_ABlankNameIs422_AndAMissingNodeIs404_T526_KK5` |
| 6 | Taşıma: üç seviye / kendi alt dalı 422, yok 404 | ✓ | `AMovePastThreeLevels_AndAMoveIntoItsOwnSubtree_Are422_AMissingNodeIs404_AndAValidMoveIs303_T526_KK6` |
| 7 | Onaysız silme; dolu 422 sayılarla + taşıma önerisi; yok 404 | ✓ | `AFullCategory_IsNotDeleted_And422NamesTheBlockingCounts_AnEmptyOneIsDeleted_AMissingOneIs404_T526_KK7` (`CategoriesModel.NotEmptyText`, K-799 birebir) |
| 8 | En yeni önce; hâl tarihten ve kalan haktan; haklar salt okunur; yabancı 404 | ✓ | `TheList_IsNewestFirst_EachRowWithTheValidityLineReadFromItsDatesAndRights_T526_KK8`, `WithoutCoupons_TheListIsTheEmptyRow_AndTheNewCouponLinkStands_T526_KK8`, `ACouponsForm_ShowsItsRightsReadOnly_AndItsVersion_T526_KK8`, `AForeignOrMalformedKey_Is404_OnTheFormAndOnTheSave_T526_KK8`; `CouponTests.TheList_IsNewestFirst_ThenByInnerKey_AndAPagePastTheLastShowsTheLast_T526_KK8`, `EachRowsValidity_IsReadFromItsDatesAndItsRightsLeft_AndAForeignKeyReadsNothing_T526_KK8`; Unit `TheValidityLine_…_T526_KK8` (2) |
| 9 | Yazar; yalnız yeni siparişe işler; bitiş öne çekilince yeni siparişte kabul edilmez, ayrılmış haklar kalır; iz yok | ✓ | `CouponTests.EveryFieldIsWritten_TheVersionRises_TheRightsStay_AndNoTrailRowIsWritten_T526_KK9`, `MovingTheEndDayEarlier_RefusesANewOrder_AndTheReservedRightsStay_T526_KK9`, `AChangedValue_ReachesOnlyANewBasket_TheOrderItemKeepsItsShare_T526_KK9`; `CouponsPageTests.ANewCoupon_IsWritten_And303GoesToItsOwnAddressWithTheSavedNotice_T526_KK9`, `AChange_IsWritten_TheRightsStay_AndNoTrailRowIsWritten_T526_KK9` |
| 10 | 422: kod boş/çakışan, sıfır, %100, asgari, ters tarih, hak tabanı; 409 422'den önce; yok 404 | ✓ | Sayfa: `APercentageCouponsFieldProblem_Is422WithItsOwnMessage_AndWritesNothing_T526_KK10` (Theory: boş ve harf ayrımsız çakışan kod `yaz10`/`YAZ10`, sıfır yüzde, %100, ters tarih), `AFixedAmountCouponsValueAndMinimumRules_Are422WithTheirOwnMessage_T526_KK10`, `AUsageCountUnderTheUsedAndReservedRights_Is422WithTheirNumber_AndTheFloorItselfIsWritten_T526_KK10`, `AStaleVersion_Is409EvenWithAFieldProblem_AndOverwritingWithTheCurrentVersionIs303_T526_KK10`; modül: `ChangingAMissingCoupon_IsNotFound_T526_KK10`, `AStaleVersion_IsAConflict_BeforeTheFieldRules_AndWritesNothing_T526_KK10`, `TheUsageCount_MayNotGoUnderTheUsedAndReservedRights_ButMayEqualThem_T526_KK10`, `TheCode_IsUniqueAgainstOtherCoupons_ButTheCouponItselfIsNotAnother_T526_KK10`, yarış `AChangeThatRacesAReservation_WaitsForIt_AndMeasuresTheFloorAfterIt_T526_KK10`; Unit `TheUsageCount_MayEqualTheUsedAndReservedRights_ButNotGoUnderThem_T526_KK10` |

### Öz denetim (bitiş kapısı 2)

> Bu turda (devam 5) on kabul kriteri, KK1–KK10, `09 §25`, güvenlik ve `12`'nin E-34/E-35 satırları önceki işçilerin yaptıkları dahil diff'te yeniden geçildi; 31 `_T526_KK*` test adı mevcut. Eksik bulunmadı; yalnız kayıt işleri yapıldı.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1 sıra, seviye sınırı, boş ağaç | ✓ | `CategoriesPageTests` `…_KK1_KK2`, `AnEmptyTree_…_KK1` |
| KK2 sayılar ve menü satırı | ✓ | `…_KK1_KK2`; `Categories.cshtml.cs` `MenuHiddenText`, `GetMenuAsync` |
| KK3 adres, çakışma, kilit | ✓ | `…_KK3_KK4`; T333 kilit testi `TwoOpposingMoves_AreInOrderedByTheTreeLock_…` |
| KK4 iki ret, 404 | ✓ | `ABlankName_…_KK4`, `AChildUnderTheThirdLevel_…_KK4` |
| KK5 ad değişir, adres sabit | ✓ | `ARename_…_KK5` |
| KK6 iki taşıma reddi | ✓ | `AMovePastThreeLevels_AndAMoveIntoItsOwnSubtree_…_KK6` (ret kodu `CategoryDepthExceeded`, `07` §6.2.6 tablosu iki sebebi bu koda yazar) |
| KK7 dolu kategori silinmez | ✓ | `AFullCategory_IsNotDeleted_…_KK7` (`CategoryNotEmpty`) |
| KK8 dört hâl; yabancı 404 | ✓ | `CouponsPageTests` dört `_KK8`, `CouponTests` iki `_KK8`, Unit iki `_KK8` |
| KK9 yeni siparişe işler; bitiş çekme | ✓ | `CouponTests` üç `_KK9`, `CouponsPageTests` iki `_KK9` |
| KK10 422 sebepleri ayrı testte; 409 | ✓ | Sayfa 422 Theory'leri her sebebi ayrı satırda; 409 `AStaleVersion_Is409EvenWithAFieldProblem_…`; modül 409 önce, 404, taban, tekillik, yarış |
| `09 §25` tek kural evi | ✓ | Yüzde sınırı `Money.MaxPercentage` (sayfanın kendi `Hundred` sabiti silindi); tutar/yüzde biçimi `InvalidAmount`/`InvalidPercentage`; kurallar `CouponRules`; hâl `CouponRules.ValidityOf`; adet `InvalidQuantity` |
| `09 §25` kilit tek yerde | ✓ | `CouponLocks.LockForUpdateAsync`; tarama `Scan_InTheCouponLocksFile_AllowsOnlyThePlainRowLockText`, `Scan_AFileOfTheSameNameAsCouponLocksElsewhere_OrTheCouponModule_IsNotAnAllowedPlace` (`Coupons.cs` dahil), tam taramada `Assert.Contains(CouponLocksFile, scanned)` |
| `09 §25` sayfalı liste sınırı | ✓ | `TheList_IsNewestFirst_ThenByInnerKey_AndAPagePastTheLastShowsTheLast_T526_KK8` (aynı `CreatedAt` iç anahtarla sıralı; `int.MaxValue` sayfa → son sayfa) |
| `09 §25` döngüde sorgu yok | ✓ | Liste tek sayfalı sorgu; ağaç tek ağaç okuması + önbellekli menü |
| `09 §8.2` m.3 ret kaydı | ✓ | Her 422/409 dalında `RejectionLog.Rejected` (`Categories.cshtml.cs` `AnswerAsync`, `Coupons.cshtml.cs` `AddProblem`, 409 dalları) |
| `09 §3.4` alan hatası ortak kalıp | ✓ | `ModelState.AddFieldError` + `FieldCodes`; K-799 metinleri sabit (`DuplicateCodeText`, `MinimumNotAboveAmountText`, `PercentageReachesHundredText`, hak tabanı metni) birebir |
| `12` VAL-A520, A521 | ✓ | `…_KK7`, `…_KK4`, `…_KK6` (422; ağaç değişmez) |
| `12` VAL-A522, A727 | ✓ | `APercentageCouponsFieldProblem_…_KK10` (`yaz10`/`YAZ10` → `DuplicateCodeText`); modül `ACodeThatDiffersOnlyInLetterCase_IsNotWritten` |
| `12` VAL-A554 "Kaydedildi" | ✓ | `…ASecondSendOpensASecondOne…_KK3_KK4` ve `ANewCoupon_…_KK9`: 303 `notice=Saved` |
| `12` VAL-A726 tekrar gönderim | ✓ | `…ASecondSendOpensASecondOne…_KK3_KK4` |
| `12` VAL-B045, B082 [tarayıcı] | — | Tarayıcı satırları; yapımda tarayıcı koşulmadı, stiller `admin.css`'te (doğrulamada tarayıcıyla) |
| Güvenlik | ✓ | Sayfalar panel çerçevesinde (yönetici panel oturumu); POST'lar çatının antiforgery'siyle; yabancı/bozuk anahtar 404 (`AForeignOrMalformedKey_Is404_…_KK8`); 409 ve 404 alan hatalarından önce; ham SQL yalnız `CouponLocks` (parametreli) |
| Backlog: hedefi T526 olan kalemler | ✓ | Yalnız D-333 → yerinde kapatıldı (panel ekranı; iz satırı `07` UC-118…121'de yok) |

### Sık FAIL nedenleri

| # | Neden | Sonuç |
|---|---|---|
| 1 | Bekçi/tarama istisnası geniş | ✓ İzin yalnız `Catalog/CouponLocks.cs` tam yoluna; aynı adlı başka dosya ve `Coupons.cs` izinli değil (ayrı ihlal testi) |
| 2 | Kaynak taraması çağrı biçimi kaçırıyor | ✓ Yeni tarama biçimi yok; var olan `FOR UPDATE` taraması genişletilmedi, yalnız yer eklendi |
| 3 | 06 "Varsayılan" / FK dizini | ✓ Şema değişmedi |
| 4 | Aynı sayıyı anan öteki yerler | ⚠ `09 §10.2` m.5–m.6 izinli kilit yerlerini tek tek sayar; `CouponLocks.cs` eklenmedi → D-1407 (karar kaydıyla belge turunda) |
| 5 | 09 metni karar kaydısız | ✓ 09 değişmedi |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit TAM | ✓ (⚠ 1 kararsız) | `dotnet test tests/Shopfolio.UnitTests/…` | 7476 test; 1 kırmızı `PageSourceScanTests` `RegexMatchTimeoutException` (makine yükü, tam koşu 264 sn); sınıf tek başına 27/27 yeşil |
| Integration | ✓ hedefli — TAM: CI | `Shopfolio.IntegrationTests.exe -class <ad>` | CouponTests, CouponsPageTests, CategoriesPageTests 58/58 (c43507f5); son kod değişikliğinden (086759f5) sonra CouponsPageTests 19/19. Tam koşuyu CI'ın Integration işi yapar |
| Format | ✓ | `dotnet format <proje> --verify-no-changes --include <değişen dosyalar>` | Web, UnitTests, IntegrationTests temiz; `Coupons.cshtml.cs` 086759f5 sonrası ayrıca temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-13; bağımsız doğrulama ajanı) — `901e0cdc` |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 1 — K1: `06 §4.13` "Kısıtlar" toplam adedin panel yazımındaki tabanı "§2.6.5'in kalıbı" (koşullu güncelleme) diye yazar; kod tabanı `CouponLocks` satır kilidi altında okuyup `RecordSave` ile yazar. Sonuç aynı (yarış testi), yöntem farklı → D-1407'ye eklendi (bu PR'da, `DEFERRED_BACKLOG.d/T526.md`) |
| Başlangıç kapıları | Working tree temiz; hook yolu `scripts/git-hooks`; main CI son 3: 43313032090 ✓, 43313031981 ✓, 43276977661 ✓ |
| Kabul kriterleri | 1–10 ✓ — kanıtlar yapım tablosundaki testler, doğrulayıcı tarafından yeniden koşuldu ve kodda okundu (sıra Türkçe harmanlamayla veritabanında; menü satırı `GetMenuAsync`; kategori yazımları ağacın danışma kilidiyle; kupon: 404 → kilit → 409 → alan kuralları, `07 §2.4.8`; hak tabanı kilit altında; değişiklik yalnız yeni sepete; iz yok; K-799 metinleri birebir; sayfa tutar hesaplamaz) |
| KK1–KK10, `09 §25` | ✓ — sayfalı liste `Id` ile biter + sınır testi; kilit `LockSequence`'e bildirilir, gövde `RunAsync` lambda'sı; sayfada `DbContext` yok |
| Bekçi istisnası | Dar: izin yalnız `src/Shopfolio.Web/Modules/Catalog/CouponLocks.cs` tam yoluna ve yalnız düz `FOR UPDATE`'e; aynı dosyada danışma, `NO KEY`, `SHARE` ve sayaç yazımı yine bulgu (`Scan_InTheCouponLocksFile_…`); aynı adlı başka yer ve `Coupons.cs` izinsiz |
| Testler (doğrulayıcı) | build `-warnaserror` 0/0; format Web/Unit/Integration temiz; Unit 7476/7476; betik 4/4; Integration CouponTests + CouponsPageTests + CategoriesPageTests 58/58, CategoryTreeAndListsTests + ConcurrencyTests + PageFormPatternTests 90/90 — TAM: CI |
| Güvenlik | Temiz — `/Panel` klasörü yönetici politikasıyla kapalı; formlar antiforgery'li; ham SQL parametreli ve tek yerde; yabancı/bozuk anahtar 404; yeni bağımlılık yok |
| `12` satırları | PASS: VAL-A184, A185, A196, A197, A198, A520, A521, A522, A523, A524, A726, A727. BEKLEMEDE kalır: A181, A182 (vitrin gözlemi), A183 (ürün formu, T525), A191–A195 (indirim), A456, A508, A554 (öteki ekranlarla ortak), B045, B082 (`[tarayıcı]`). `12_matris_denetle.py --kapsam 1-77` hata 0 |
| Yapım raporu karşılaştırması | Uyumlu; tek fark: öz denetim VAL-A554'ü ✓ saydı — satır ürün ve adres kaydını da kapsadığı için doğrulamada BEKLEMEDE kaldı |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T526-kategori-kupon`
- **PR:** PR açıklamasında
- **CI:** dal run 43303039201 ✓ (Lint, Build, Unit, Integration, Contract, Migration, Paketleme, CI Gate) — head `901e0cdc`
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok.

## ⚠ Maddeler

- Kupon satır kilidi (`CouponLocks.cs`) `09 §10.2` m.5–m.6 listelerine yazılmadı; belge turunda karar kaydıyla (D-1407).
- "Tutar sıfırdan büyük olmalı." ve "Üçüncü seviyedeki kategorinin altına kategori açılamaz." K-799 kataloğunda yok (D-1408).
- Unit tam koşuda `PageSourceScanTests` regex zaman aşımı (yük; kod değişikliği yok, sınıf tek başına yeşil).
- Toplam kullanım adedi 0 reddedilir: alan `04` 7.3.2.10 adet kuralıyla (`InvalidQuantity`, 1…); `06 §4.13` veritabanı kısıtı ≥ 0 daha geniştir.
- Menü satırı kategorinin sunucu menüsündeki (`GetMenuAsync`) üyeliğinden okunur; menü önbelleği yazma yollarında geçersiz kılınır (T333).
- Eşzamanlı aynı kodla iki yeni kupon tekil dizine çarpar (`TwoSavesThatRaceForOneCode_MeetTheUniqueIndex`, main davranışı).
- Kupon 409'unda alan hataları atılır, 409 işleyicide döner (`07` §2.4.8: 409 422'den önce).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `09 §10.2` m.5–m.6'ya `CouponLocks.cs` | D-1407 | Belge kuralı karar kaydısız değiştirilmez |
| 2 | İki mesaj K-799'a | D-1408 | Karar kaydı metni belge turunda eklenir |

## Notlar

- **Working tree (Adım -1):** temiz
- **Yapım dört turda bölündü (süre); devam 5 yalnız format doğrulaması, kayıtlar ve PR.** main `73e36672`, dal ilerisinde; birleştirme gerekmedi.
- **Repo memory (bitiş kapısı 9):** MEMORY "Güncel Durum"a dokunulmadı (faz kapısı kayıt PR'ı).
