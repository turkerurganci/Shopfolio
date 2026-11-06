# T214 — Kısa süreli bildirim kodları

**Faz:** F2 | **Durum:** ✓ Doğrulama tur 1 PASS | **Tarih:** 2026-11-06

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
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-06, head `479c891`) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 1 — K1 → D-146 (yeni) |

**Validator kanıtı (tur 1, 2026-11-06, `479c891`; dal CI'ı run 42201202210 success):** kapılar: working tree temiz, `core.hooksPath` `scripts/git-hooks` (ana deponun mutlak yolu); ana dal son üç run (42195116854, 42195116675, 42182118563) success; hafızada T214 satırı var. Kriter 1–6 ve KK1–KK6 ✓ — kod okundu, testler doğrulama anında yeniden koşuldu: `dotnet build -warnaserror` 0/0, `dotnet format --verify-no-changes` çıkış 0, Unit 2307/2307, betik testleri 4/4, Integration 1599/1599 (PostgreSQL 18; Web 427, Security+Concurrency+Pages+küçük gruplar 168, Jobs+Database+kök 112, Model 482 + 410), sonrası `git status` temiz ve `dotnet restore --locked-mode` çıkış 0. Bağımsız sondalar: (1) `07 §6.3`'ün üç tablosu testten ayrı `awk`/`grep` ile ayrıştırıldı — 25 kod, yinelenen yok; kilit dosyasıyla `diff` boş. (2) `QuantityExceedsOpen`'ın iki enumda bulunması belgeyle uyumlu: `07 §6.2.7` satırı "müşterinin işlemlerinde 303 ve aynı kod `notice`'te (§6.3.3)" der. (3) Okuma `ListFilters` ile aynı ölçüdedir (tam ad, sayı ve harf farkı yok sayılır); `07 §2.6.6` geçersiz değeri yok sayar. (4) D-121'in 303'ü `07 §2.3.2` ve §6.3.2 (`SessionExpired` → UC-54, UC-84) ile uyumlu; GET 302 ve çıkış değişmez. (5) Daldaki `09 §5.3` m.6 / §8.2 m.1 eklemeleri yalnız kodun yerini ve tek yazıcıyı adlandırır, `07`'nin davranışını değiştirmez — karar kaydı gerekmez. (6) Backlog: D-121 kapanışı testle kanıtlı; D-47 (2) kilit testiyle kanıtlı, (1) açık kalır; D-109'un T214 payı kanıtlı (`GoogleIdentityFailure` kodda yok), kalan T343/T409'a yeniden hedeflendi — gerekçe: sayfalar o task'larda doğar.

**Task satırları (`12 §7.5`):** T214 KP eşlemesi taşımaz (`11 §7.1`: `07 §6.3`); satırlar kaynak bölümle grep'lendi (`VAL-A553`…`VAL-A577`, `VAL-A669`, `VAL-A670`, `VAL-A675`, `VAL-A679`, D-121'in `VAL-A607`'si). Her biri bir kodun ya da kuralın uçtaki ve ekrandaki davranışıdır (sepet, kayıt, giriş ekranı gibi uç task'larının ekranı); kod listesi ve tek yazıcı o davranışı tek başına üretmez — `Durum` değişmedi, hepsi `BEKLEMEDE`.

**K-notları:** K1 → D-146: `NoticeParameterSourceScan` yalnız tırnaklı adı ve adres literallerini arar; sayfa işleyicisinin `notice` adlı parametresi, `SupportsGet` özelliği ve yazıcı dışında `NoticeParameter.ParameterName` ile okuma bulgu olmaz (bugün örneği yok).

**Yapım raporu karşılaştırması:** verdict uyumlu; öz denetim tablosu var ve her maddesi kanıtlı. Fark 1: yapım Integration'ı 1600 diye yazar; doğrulayıcının bölünmüş tam koşusu 1599 test saydı (hepsi geçti, atlanan yok) — sayım farkı, kırılma değil. Fark 2: yapımın "kaynak taraması" kanıtı tırnaksız bağlamayı kapsamaz (K1). Fark 3: Known Limitations 2'nin (giriş ekranının mesajı) izi kapanmış D-121'in notundadır; açık iz `12` VAL-A607 (KRİTİK, `BEKLEMEDE`) ve `11` T546 kriter 4'tür.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T214-bildirim-kodlari`
- **Commit:** 6c761d5
- **PR:** #410
- **CI:** — (yapım işçisi izlemez)
- **Post-merge run'lar:** squash `9a56f83` (#410) — CI 42207595893 success, Publish 42207595875 success (T215 dalında dolduruldu, skill adım 15)

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
