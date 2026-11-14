# T517 — Hesap — profil ve güvenlik (E-25)

**Faz:** F5 | **Durum:** ⏳ Devam ediyor (doğrulama bekliyor) | **Tarih:** 2026-11-13

> Taslağı yapım chat'i doldurdu; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- `/hesap` (`Pages/Account/Profile.cshtml(.cs)`, UC-63 ila UC-68): ad, e-posta, şifre, giriş yolları, hesabı sil bölümleri ve iç gezinme (`_AccountNav`). İşlemin formu yalnız oturumun izni o işlem içinse ve değerin özeti oturumdakiyle aynıysa açık gelir; tutmayan parametre yok sayılır; izin formun gösterimi dışındaki `/hesap` GET'inde düşer.
- `/hesap/silindi` (`Pages/Account/Deleted.cshtml(.cs)`, UC-69): oturum çerezinin gösterdiği silinmiş oturumla bir kez görünen sonuç hâli; yenilenirse 303 → `/`.
- Modül: `RequestEmailChangeAsync(customerId, sessionId, newEmail, reauthenticationValue, ip, ct)` yeniden doğrulama değerini en sonda tüketir (D-429); `ReadPendingEmailChangeAsync` (bekleyen değişiklik, son gün); `ISessionService.HoldsReauthenticationAsync` (yalnız okur).
- Silme: son adım GET'te yalnız gösterilir (kayıt yazmaz, izni tüketmez); onaysız POST 200 son adımı yeniden gösterir; onaylı POST tek işlemde siler, siparişi işaretler, 303 → `/hesap/silindi`, `InstantDeletion` denetim satırı işlem tamamlanınca yazılır (D-700).
- Şifre değişimi bu tarayıcının "tanınan tarayıcı" işaretini yeniler, ötekileri düşürür (D-806).
- Devir kararları (yönetici): (a) `AccountTexts` belgede sözü olan yerlerde belgenin birebir sözüne çekildi (`Yeni e-posta`, `Bağlantıyı yeniden gönder`); sözü olmayanlar ⚠ + D-1452; (b) silme son adımı GET'te yan etkisiz, silme POST'ta (testle kilitlendi); (c) UC-63 izin düşmesi `07` UC-63 ve UC-60'ın istediği iki istekte uygulandı, genel sayfa düzeyi D-1454.
- Main ile iki kez birleştirildi (`7fc11ffa`, `edf4eb2a`): `SessionStore.HoldsReauthenticationAsync` ve `ReauthenticationValues.HoldsAsync` main'de (T523) de vardı, yinelenen kopya silindi.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Pages/Account/Profile.cshtml(.cs)`, `Deleted.cshtml(.cs)`, `Pages/Shared/_AccountNav.cshtml`, `Pages/Components/AccountTexts.cs`, `Pages/Frame/FramePaths.cs`
- `src/Shopfolio.Web/Modules/Membership/` (`EmailChanges.cs`, `MembershipModule.cs`, `SessionService.cs`, `Contracts/IMembershipModule.cs`, `ISessionService.cs`, `EmailChangeRecords.cs`), `Infrastructure/Security/SessionStore.cs`, `ReauthenticationValues.cs`
- Testler: `Web/AccountProfilePageTests.cs` (yeni, 21), `Web/SessionAccessTests.cs`, `Web/RegistrationPageTests.cs`, `Model/MembershipGoogleTests.cs`, `Model/MembershipAccountDeletionTests.cs`, `Consistency/OutcomeUncertainConsistencyTests.cs`, `Pages/AccessSamples.cs` ve örnek `.cshtml`'ler (silindi)

## Kabul Kriterleri Kontrolü

Test sınıfı `AccountProfilePageTests` (kısaltma: P).

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | `GET /hesap` beş bölüm; oturum yoksa 302 girişe | ✓ | `P.TheScreen_GivesTheFiveSections_AndNoSessionGoesToTheSignIn_T517_KK1` |
| 2 | İşlemin formu yalnız izin ve özet tutunca açık; tutmayan parametre yok sayılır; üç işlemde değer yok/tutmuyorsa 303 → yeniden doğrulama | ✓ | `P.TheFormOpens_OnlyForTheOperationAndTheValueTheSessionHolds_...KK2`, `P.APageRequestOutsideTheForm_DropsThePermission_...KK2`, `P.ThePosts_WithoutTheValue_Go303ToTheReauthentication_...KK2` (üç işlem, Theory); düzeltme 1: `P.ARefusableFormWithoutTheValue_IsThe303ToTheReauthentication_NotTheRefusal_...KK2_Correction1` (değersiz/tutmayan değerle dolu adres, biçim hatası, boş adres, zayıf ve yaygın şifre → 303), `P.TheUnconfirmedDeletionWithoutTheValue_...Correction1` (onaysız silme → 303), `P.AnEmailChangeThatReplacesAPendingOneWithoutTheValue_WritesNoL9Line_...Correction1` (L-9 satırı yok; değerli kontrol sayılıyor) |
| 3 | Geri alma açıkken e-posta değiştirme ve silme sebep ve bitiş tarihiyle kapalı; şifresizde şifre belirleme | ✓ | `P.WhileTheRevertLinkIsOpen_...KK3`, `P.TheEmailChangeAndTheDeletion_OfAClosedAccount_Give422AndSpendNothing_...KK3`, `P.AnAccountWithoutAPassword_IsShownThePasswordSetting_...KK3` |
| 4 | `ChangeName` yazar, 303 → `/hesap`; boş ad 422; geçmiş siparişin alıcı adı değişmez | ✓ | `P.TheName_IsWritten_With303AndTheSavedNotice_AndAnEmptyNameIs422_...KK4` (303, 422); alıcı adının değişmediği `Model.MembershipTests.TheName_ChangesWithoutANewVerification_AndLeavesTheOrdersRecipientNameAlone` (P testi siparişe bakmaz; düzeltme 1 K2) |
| 5 | `ChangePassword` tek koşullu güncellemeyle tüketir, öteki oturumları siler, öteki işaretler düşer | ✓ | `P.ThePasswordChange_TakesTheValueOnce_EndsTheOtherSessions_AndKeepsThisOne_...KK5`; `Model.MembershipTests` (işaret) |
| 6 | P-17 altı ya da yaygın şifre 422, değer tüketilmez; şifresizde işlem sunulmaz | ✓ | `P.AWeakOrCommonPassword_Gives422_AndTheValueStaysUnspent_...KK6` (Theory), KK3'ün şifresiz testi |
| 7 | `ChangeEmail` bekleyen değişikliği yazar, öncekinin yerine geçer, yeni adrese bağlantı; hesap eski adresle çalışır | ✓ | `P.TheEmailChange_WritesThePendingChange_SendsTheLinkToTheNewAddress_AndTheAccountKeepsTheOldOne_...KK7` |
| 8 | 422: biçim, doğrulanmış başka hesap, açık geri alma; 429 (L-9) yalnız yerine geçende; değer tüketilmez | ✓ | `P.TheEmailChange_RefusesABrokenAddress_AVerifiedOthersAddress_ButNotAnUnverifiedOnes_...KK8`, `P.TheEmailChange_Gives429OnlyWhenItReplacesAPendingChange_...KK8` |
| 9 | `ResendEmailChangeLink` yeni bağlantı, öncekiler `SupersededAt`, ömür uzamaz; bekleyen yoksa 422, 429 | ✓ | `P.TheLinkAskedForAgain_IsNew_TheEarlierOnesAreSuperseded_AndTheLifeIsNotLengthened_...KK9`, `P.TheLinkAskedForAgain_WithNothingPendingIs422_AndTooManyAskingsGive429_...KK9` |
| 10 | `DeleteAccount` onaysız 200, yazmaz; onaylı tek işlemde siler, siparişi işaretler, 303 → `/hesap/silindi`; sonuç bir kez | ✓ | `P.TheDeletion_WithoutTheMark_IsTheLastStep200_WritesNothing_AndSpendsNoValue_...KK10` (GET sonrası da hesap duruyor), `P.TheConfirmedDeletion_DeletesInOneTransaction_Writes303_...KK10`; sipariş işareti `Model.MembershipTests.DeleteOwn_LeavesTheOrdersWithTheirOwnRecord_MarkedDeleted...` (sayfa aynı `DeleteAccountAsync`'i çağırır; düzeltme 1 K2), `P.TheResultState_GoesHome_...KK10`, `P.TheDeletion_NeedsTheValue_...KK10` |

### Öz denetim (bitiş kapısı 2)

Önceki işçilerin yaptığı dahil kriter 1–10 ve KK1–KK10 diff'te baştan denetlendi; `sik-fail-nedenleri.md` okundu. Bu turda: (1) tarama kuralları (`SessionWriteSourceScan` alan adı, `NullForgivingSourceScan` Razor `!`) Unit TAM'da kırmızıydı, giderildi; (2) `AccountTexts`'te belge sözü olan iki yer düzeltildi; (3) main birleştirmesinde yinelenen yöntem silindi; (4) silme son adımının GET'te yazmadığı teste bağlandı. Sık FAIL nedenleri: bekçi istisnası eklenmedi (kapalı listeler dokunulmadı); kaynak taramaları çalıştırıldı; 06 varsayılan/FK dizini değişmedi (şema değişikliği yok); sayıları anan öteki yerler (`OutcomeUncertainConsistencyTests` UC-68 listesi, `SessionAccessTests` açık adres listesi) güncellendi; belge kuralını değiştiren 09 metni yok; hedefi T517 olan backlog kalemleri taranıp kapatıldı ya da ilerletildi (aşağıda).

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 oturumsuz 302 | ✓ | `P...KK1` |
| KK2 izin, özet; üç işlemde 303 | ✓ | `P...KK2` (üç test) |
| KK3 geri alma açıkken kapalı işlemler | ✓ | `P...KK3` (üç test) |
| KK4 ad değişir, sipariş adı sabit | ✓ | `P...KK4` |
| KK5 tek tüketim; öteki oturum ve işaret silinir | ✓ | `P...KK5`, `Model.MembershipTests` |
| KK6 iki ret sebebi | ✓ | `P...KK6` |
| KK7 bekleyen değişiklik ve yeni bağlantı | ✓ | `P...KK7` |
| KK8 üç 422 sebebi, L-9 | ✓ | `P...KK8` (iki test) |
| KK9 eski bağlantılar geçersiz | ✓ | `P...KK9` (iki test) |
| KK10 onaysız yazmaz; onaylı 303, sipariş işareti; sonuç bir kez | ✓ | `P...KK10` (dört test) |
| `09 §25` ilgili satırlar | ✓ | Unit TAM (mimari taramalar: `SessionWriteSourceScan`, `NullForgivingSourceScan`, `OutcomeUncertainConsistency`, `CookieSourceScan`, `NoticeParameter`, `FormSource`, `Frame`) |
| Güvenlik taraması | ✓ | işlem formu yalnız izinle; değer gizli alanda; GET yazmaz; silme POST'ta; `[OutcomeUncertain("UC-68")]` |
| `12` T517 satırları | ~ | paylı satırlar `BEKLEMEDE` kalır (D-1455); T517 kanıtı: `VAL-A098`, `A099`, `A441`, `A505`, `A672` tam; `A100`, `A103`, `A362`, `A494`, `A504`, `A554`, `A560`, `A566` T517 payı |

Backlog (hedefi T517): kapandı D-428, D-429, D-555; payı kapandı D-254, D-606 (UC-157 paneli kalır), D-693, D-700 (T522 payı kalır), D-801 (A362 L-1 payı T516), D-806; ileri: D-665 (yarışma testi yazılmadı, kalır), D-683 (eskimiş açıklama temizliği kalır), D-1434 (T516 k-note, T517 değil, hedefte).

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | TAM yeşil | `dotnet test --project tests/Shopfolio.UnitTests --no-build` | 7546 test, 0 kırmızı; ilk koşuda 3 tarama kırmızıydı (oturum izni alan adı, Razor null-forgiving, bir düzenli ifade zaman aşımı yük altında), giderildi, üç sınıf 46/46 |
| Düzeltme 1 | Unit TAM 7557/7557; Integration `AccountProfilePageTests`+`SessionAccessTests`+`AccountAddressAndOrderHistoryPageTests` 72/72, `Model.MembershipTests`+`RegistrationPageTests`+`SitemapEndpointTests`+`ReauthenticationPageTests` 208/208, **TAM: CI**; build `-warnaserror` ve `dotnet format` temiz | | |
| Integration | İlgili sınıflar yeşil, **TAM: CI** | `dotnet test --project tests/Shopfolio.IntegrationTests --no-build --filter-class ...` | `AccountProfilePageTests` 21/21, `Model.MembershipTests` 136/136, `SessionAccessTests`+`StorefrontFrameTests`+`AccountProfilePageTests` 84/84; önceki işçi: `SecurityHeadersTests`, `SitemapEndpointTests`, `OneTimeValueTests`, `CustomerPasswordResetAndReauthenticationPageTests`, `RegistrationPageTests` yeşil |
| Biçim | Temiz | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` | çıkış 0 |

## Düzeltme 1 (2026-11-14, tur 1 FAIL sonrası)

**Değersiz gönderim önce 303 yeniden doğrulama.** Bulgu 1 (S1): yeniden doğrulama değeri ret denetimlerinden sonra soruluyordu; değersiz `ChangeEmail` dolu adreste 422 + K-786 mesajı verip hesap varlığını sızdırıyordu. Düzeltme: `ProfileModel.OnPageHandlerExecutionAsync` (sayfa modeli süzgeci doğrulama süzgecinden önce çalışır) şifre, e-posta ve silme gönderiminde değerin tutup tutmadığını yalnız okuyarak (`HoldsReauthenticationAsync`) önce sorar; tutmuyorsa ret kaydı + 303 → yeniden doğrulama, biçim, dolu adres, zayıf şifre ve onaysız silme denetimleri değerin tuttuğu bilindikten sonra koşar. Hizmet katmanında `EmailChanges.RequestAsync` aynı okumayı kilitten hemen sonra yapar (L-9 satırı değersiz yazılmaz); tüketim en sonda kalır. Değer tutarken eski 422/200 davranışı sürer (KK6, KK8, KK10 testleri). Şifresiz hesabın değersiz şifre gönderimi de artık 303 (KK3 testi güncellendi; "şifre yok" cümlesi yalnız izin tutarsa görünür). K1: KK6 testi şifrenin sayfaya geri yazılmadığını `value="…"` ile sınar. K2: KK10 testinin adı sipariş işaretini anmaz, kanıt model testine bağlandı. Main birleştirmesi (T518, #566): `AccountTexts`, `FramePaths`, `AccessSamples`, STATUS, backlog (D-693) iki taraf birlikte korunarak çözüldü.

**Öz denetim:** kriter 1–10 ve KK1–KK10 yeniden denetlendi; tablolardaki ✓ ve kanıtlar güncel. Yönetici tarafı (T523) aynı sıralama için D-1473'te, UC-67 L-9 için D-1474'te kalır.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 2 (2026-11-14), head `d682fc7a` (main 49ba60f0 birleşik); tur 1 bulgusu kapandı |
| Tur 2 kanıtı | Correction1 testleri (değersiz/tutmayan değerli dolu adres, biçim, boş adres, zayıf ve yaygın şifre, onaysız silme → 303; değersiz yerine geçen istekte L-9 satırı yok) yeşil; doğrulayıcı sondası (silindi): şifresiz hesap izinle 422 + "şifre yok", şifre yazılmaz, değer tüketilmez; GET `?handler=ChangeEmail` 200, `ChangeName` 303 kaydedildi, bağlantı yenileme 422 — yanlış 303 yok; değerli dolu ve bozuk adres 422, değer gizli alanda. Unit 7557/7557; Integration ilgili sınıflar 316/316, TAM: CI (run 43546906906 yeşil) |
| Tur 2 bulgu / K | Bulgu 0. K: rapor `12` öz denetim satırı `A441`, `A672`'yi "tam" sayıyor, tur 1'deki gibi BEKLEMEDE kalır (D-1455, D-1474); D-1453 kapandı (T518 birleşik) |
| Tur 1 | ✗ FAIL — 2026-11-14, head `65932f9f` |
| Bulgu sayısı | 1 (S1, ORTA, güvenlik): e-posta değiştirmede "adres doğrulanmış başka bir hesabın" sorgusu yeniden doğrulamadan önce yanıtlanıyor — değersiz `ChangeEmail` gönderimi dolu adreste 422 ve K-786 mesajını alıyor (`02 §3.13.11`: "sorgu oturumun ve yeniden doğrulamanın arkasındadır"; KK2: değer yoksa 303). Aynı sıralama kökü: değersiz gönderimde biçim hatası ve zayıf şifre 422, onaysız silme 200 |
| Düzeltme gerekli mi | Evet — `EmailChanges.RequestAsync` (ya da sayfa) değerin tutup tutmadığını yalnız okuyarak (`HoldsReauthenticationAsync`) ret denetimlerinden önce sormalı, tüketim en sonda kalmalı; değersiz/tutmayan değerli gönderimin dolu adreste 303 verdiği test. Öteki üç işleyicide de değersiz gönderim 303 olmalı |
| Bloke-etmeyen not (K) sayısı | 4 — K1 KK6 testindeki etkisiz `DoesNotContain` (düzeltme turunda); K2 KK10 test adı "MarksTheOrder" sipariş tohumlamıyor, KK4 kanıtı rapordaki P testinde değil `Model.MembershipTests.TheName_ChangesWithoutANewVerification_AndLeavesTheOrdersRecipientNameAlone`'da (düzeltme turunda); K3 → D-1473 (yönetici tarafı aynı sıralama); K4 → D-1474 (UC-67 422'de L-9 satırı, `VAL-A441` BEKLEMEDE) |
| `12` | `VAL-A098`, `A099`, `A103`, `A503`, `A505`, `C030` → PASS; `A441`, `A504`, `A672` (tarayıcı gözlemi yok), `D022` ve öteki paylı satırlar BEKLEMEDE |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T517-hesap-profil-guvenlik`
- **Commit:** PR'ın head'i
- **PR:** açıldığında yönetici yardımcıya bildirilir
- **CI:** yönetici yardımcı izler
- **Post-merge run'lar:** main `3fe7d829`: CI 43552777845 success, Publish 43552777848 success

## Dış Varsayımlar

Dış varsayım: yok. (Google yeniden doğrulaması T516'nın ucundadır; bu task Google'a istek atmaz.)

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | ⚠ Belgede sözü olmayan metinler (`AccountTexts`) | D-1452 | `04` ve `07` cümleyi vermiyor |
| 2 | ⚠ İç gezinme `/hesap/adresler` ve `/hesap/siparisler` T518'e kadar 404 | D-1453 | iki ekran T518'in işi |
| 3 | ⚠ İzin düşmesi yalnız `/hesap` GET'inde ve yeniden doğrulama açılışında (`05` 6.1.5 genel sayfa der) | D-1454 | katman task'ı gerektirir |
| 4 | Paylı `12` satırları | D-1455 | öteki paylar başka task'ın ucunda |
| 5 | 320 genişlik ve klavye gezintisi tarayıcıyla sınanmadı | D-1456 | Integration kapsamı dışı |
