# T214 — Kısa süreli bildirim kodları

**Faz:** F2 | **Durum:** ⏳ Yapım bitti, doğrulama bekliyor | **Tarih:** 2026-11-06

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **Kapalı liste** `Infrastructure/Notices/NoticeCode.cs`: 25 üye — `07 §6.3.1`–`§6.3.3` tablolarının kodları. `NoticeKind` + `NoticeKinds` üç ayrı listeyi tutar (kısa süreli bildirim, sonuç hâli, sebep); `NoticeRoutes` `07 §6.3.1` uç → hedef tablosudur.
- **Tek yazıcı** `NoticeParameter` (internal): `ParameterName`, `Address(path, code)`, `Read(request, kind)`, `TryParse` (tam ad; listede olmayan, tekrarlı ya da başka listenin kodu yok sayılır). `FormResults.SeeOther(path, NoticeCode)` 303 verir; yanıtta yalnız `Location` başlığı vardır, çerez ve sunucuda kayıt yoktur.
- **D-121:** `SessionAuthenticationHandler.HandleChallengeAsync` oturumu kapanmış POST'un 303'üne `&notice=SessionExpired` ekler (GET 302 ve çıkış değişmez).
- **D-109 payı:** `GoogleIdentityFailureRule` artık `NoticeCode?` döner; geçici `GoogleIdentityFailure` enum'u silindi.
- **Kilit:** `Locks/Shopfolio.Infrastructure.Notices.NoticeCode.txt`; `EnumLockTests.NamedEnums` ve `TheNoticeCodeList_HasALockFile_…KK5` (D-47 (2) kapandı).
- **Kaynak taraması** `Architecture/NoticeParameterSourceScan(+Tests)`: parametre adı (her tırnak biçimi, büyük/küçük harf duyarsız), adres literali (`?notice=`, `&notice=`, `&amp;`, `%3F`/`%26`, bölünmüş literal) ve görünümde `asp-route-notice`/`href` biçimleri tek yazıcı dosyasının dışında bulgudur; istisna tam yoldur. İhlal örnekleri `ViolationSamples/Notices/*`.
- **Dokümanlar:** `09 §5.3` m.6 ve `§8.2` m.1 (yer, tek yazıcı, tarama, kilit), `CONTEXT.md` `Infrastructure/Notices/` satırı, backlog (D-47, D-109, D-121), durum, hafıza.
- Önceki task kaydı: T213 status Commit hücresi (`11a2c7d`) ve rapor "Post-merge run'lar" satırı dolduruldu (skill adım 15); eski durum girişi `STATUS_CHANGELOG.md`'ye taşındı.
- Bu rapor iki işçi tarafından yazıldı (yapım, devam); öz denetim bütün maddeleri yeniden denetler. Testlerdeki elle yazılı `"notice"` adları `NoticeParameter.ParameterName`'e, `QuantityExceedsOpen` ve `Saved` gibi değerler `NoticeCode` üyesine bağlandı (tek istisna `TheParameterName_IsNotice_AsThe07Says`: sabitin kendisini belgeye karşı sabitler).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/Notices/{NoticeCode,NoticeKind,NoticeParameter,NoticeRoutes}.cs`, `Forms/FormResults.cs`, `Security/SessionAuthenticationHandler.cs`, `Modules/Membership/GoogleIdentityFailureRule.cs`
- Testler: `UnitTests/{Consistency/NoticeCodeConsistencyTests,Notices/NoticeParameterTests,Architecture/NoticeParameterSourceScan*,Architecture/ViolationSamples/Notices,Locks/EnumLockTests,Locks/…NoticeCode.txt,Outages/OutageRulesTests}`, `IntegrationTests/{Web/NoticeParameterTests,Web/SessionAccessTests,Web/VersionedFormPageTests,Pages/NoticeSamples,Pages/Storefront/NoticeSample.cshtml,Pages/Panel/VersionedOrder.cshtml.cs}`
- Dokümanlar: `Docs/09_CODING_GUIDELINES.md`, `Docs/DEFERRED_BACKLOG.md`, `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `Docs/TASK_REPORTS/T213_REPORT.md`, `.claude/CONTEXT.md`, `.claude/memory/MEMORY.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | `notice` kapalı listesi var; liste dışı değer yok sayılır | ✓ | `NoticeCode` (25 üye); Unit `AValueOutsideTheClosedList_IsIgnored_ForEveryList_KK1_KK2`; Integration `AValueOutsideTheList_IsIgnored_200_NoNotice_AndNotReflected_KK1_KK2` |
| 2 | Kod adreste taşınır; serbest metin ve kişisel veri yok | ✓ | `NoticeParameter.Address` yalnız `NoticeCode` alır; Unit `TheAddress_*` (4 test); `NoticeParameterSourceScanTests` |
| 3 | Yeni çerez ve geçici kayıt yok | ✓ | Unit `TheSeeOtherResult_IsA303…SetsNoCookie_KK3` (yalnız `Location`); Integration `ASuccessfulPost_Is303…SetsNoCookie_KK3_KK6` |
| 4 | Kısa süreli bildirim, sonuç hâli, sebep ayrı listeler; sonuç hâli kalıcı | ✓ | `NoticeKinds`; Consistency `EveryCode_IsInOneTableOnly…KK4`, `TheKindOfAMember_IsTheTableItsCodeStandsIn…KK4`; Integration `TheNoticeRepeatsWhenThePageIsReloaded…KK4` |
| 5 | Biçim `07 §6.1.1`; `NoticeCode` kilit testinde | ✓ | `EveryName_IsEnglishPascalCase…KK5`; `EnumLockTests.TheNoticeCodeList_HasALockFile…KK5`; `Locks/Shopfolio.Infrastructure.Notices.NoticeCode.txt` |
| 6 | Beş kod uç → hedef | ✓ | `TheRouteTable_IsTheEndpointColumnOf07_631…KK6`, `TheFiveCodesOf07_631_RedirectFromTheirEndpointToTheirTarget_KK6`; Integration `ASuccessfulPost_Is303…KK6` (25 kod) |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| K1–K6 | ✓ | yukarıdaki tablo |
| KK1 listede olmayan kod ekranda görünmez | ✓ | Integration `TheGet_ShowsTheTextOfACode_InItsOwnListOnly_AndNeverTheName_KK1`, `AValueOutsideTheList_IsIgnored…` |
| KK2 adreste serbest metin kabul edilmez | ✓ | Unit `AValueOutsideTheClosedList…KK2`, `TheAddress_RefusesAPathThatIsNotTheSitesOwn…`; kaynak taraması |
| KK3 yanıtta ek çerez yok | ✓ | Unit `…SetsNoCookie_KK3`; Integration `…SetsNoCookie_KK3_KK6` |
| KK4 sonuç hâli yenilenince kalır; sebep kodları listede | ✓ | `TheNoticeRepeatsWhenThePageIsReloaded…KK4`; `Members_AreTheCodesOfTheTablesOf07_63_InBothDirections` |
| KK5 kalıp + kilit iki yönlü | ✓ | `EveryName_IsEnglishPascalCase…KK5`; `TheNoticeCodeList_HasALockFile…KK5` |
| KK6 beş kod uçtan hedefe | ✓ | `TheFiveCodesOf07_631_RedirectFromTheirEndpointToTheirTarget_KK6` |
| `09 §25` yeni kural için kaynak taraması + ihlalli örnek (§19.2 m.3) | ✓ | `NoticeParameterSourceScan` + `ViolationSamples/Notices/*`; `OnTheCodeSample_EveryFormIsReported…` |
| `09 §25` değer ekleyen PR: kilit dosyası + önce `07 §6`'ya yazılmış | ✓ | kilit dosyası; `Members_AreTheCodesOfTheTablesOf07_63_InBothDirections` |
| Güvenlik taraması | ✓ | adreste yalnız enum adı; yerel olmayan yol `ARedirectWithANotice_StillRefusesANonLocalPath`; yansıma yok (`…NotReflected…`) |

Sık FAIL nedenleri (`sik-fail-nedenleri.md`) denetimi:

| # | Neden | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Tarama istisnası geniş | ✓ | `TheExemption_IsTheExactPathOnly`; istisna tek tam yol sabiti |
| 2 | Çağrı biçimi kaçıyor | ✓ | `OnTheCodeSample_EveryFormIsReported_AndTheNearNamesAreNot` (düz, verbatim, ham, karakter, bölünmüş literal, `&amp;`, `%3F`); `OnTheViewSample_TheTagHelperAndTheHrefFormsAreReported` |
| 3 | Öteki yerler güncellenmedi | ✓ | `09 §5.3` m.6, `§8.2` m.1, `CONTEXT.md`, durum, hafıza, backlog; `09 §12` m.3 ve `§25` zaten `NoticeCode`'u anıyordu |
| 4 | Beklenen değer sabitten türetilmiyor | ✓ | testlerde ad `NoticeParameter.ParameterName`, değer `NoticeCode` üyesi; kümeler `Enum.GetValues<NoticeCode>()` |
| 5 | Hedefi T214 olan kayıt | ✓ | D-47 (2) ✅, D-121 ✅, D-109 T214 payı ✅ (kalan T343/T409'a yeniden hedeflendi); başka kayıt T214 hedefli değil |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests -c Release --no-build` | 2307 geçti, 0 başarısız |
| Integration | ✓ | `--filter-namespace` parçalarıyla (Release) | Database 35 · Model 893 · Jobs 77 · Concurrency+Disposal+FileSecrets+Files+Logging+Outages+Security+SignedEndpoints 168 · NoticeParameter+SessionAccess+VersionedForm 118 · Web (öteki) 309 — toplam 1600, hepsi geçti |
| Contract | — | — | Bu task'ta yok |
| Derleme/biçim | ✓ | `dotnet build Shopfolio.slnx -c Release -warnaserror`; `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` | 0 uyarı, 0 hata; biçim temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | — (doğrulama bekliyor) |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T214-bildirim-kodlari`
- **Commit:** SON_COMMIT
- **PR:** PR_NO
- **CI:** — (yapım işçisi izlemez)
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; sonraki task dalında güncellenir)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| — | Dış varsayım: yok | Yeni paket, servis ya da çerçeve davranışı varsayılmadı; sorgu anahtarının harf duyarsız okunması çerçeve davranışıdır ve Unit testte sabitlendi (`ARepeatedOrAbsentParameter_IsIgnored`) | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Sayfaların `GoogleIdentityFailureRule` kodunu kısa süreli bildirime yazması | D-109 (T343, T409) | Sayfalar o task'larda doğar |
| 2 | Giriş ekranının `SessionExpired` mesajı; `12` VAL-A607 | D-121 notu (T515, panel girişi) | Giriş ekranları o task'larda |
| 3 | `AuditOperation` etiket testi | D-47 (1) (T380) | Etiket kataloğu T380'de |
| 4 | `QuantityExceedsOpen` hem hata hem bildirim kodu adı (`07 §6.3.3` "aynı hâl") | — | Belgede tek ortak ad; testte sabitlendi, yeni kayıt gerekmedi |

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** 42195116854 CI, 42195116675 Publish, 42182118563 Publish — hepsi success
- **Repo memory (bitiş kapısı 9):** güncellendi
- Bilinen not: sorgu anahtarı okuması harf duyarsızdır (çerçeve); `?Notice=Saved` okunur.
