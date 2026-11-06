# T216 — Tarih, sayı ve para biçimi: istek ve gösterim

**Faz:** F2 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-06

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **İstekteki biçim (`Infrastructure/RequestFormat.cs`):** tutar ve ondalık sayı tr-TR kültürle, yalnız `AllowDecimalPoint` stiliyle okunur (rakam ve en çok bir virgül); `decimal`'in tutabileceğinden çok haneli metin okunmaz — çatının ayrıştırıcısı onu sessizce yuvarlıyordu (`1,999…9` → 2, `12,000…01` → 12; bu turda ölçülüp düzeltildi). Tarih yalnız `yyyy-AA-GG`. Tam sayı yalnız ASCII rakam (`NumberStyles.None`, değişmez kültür; `0x10`, `#10`, `+5`, boşluk okunmaz). Form değeri ayrıştırmanın tersiyle yazılır (`FormatDecimal`, `FormatDate`). `CompactPhone`, `CompactIban` (boşluk yok sayılır).
- **Tek model bağlayıcısı (`Validation/RequestValueBinder.cs`):** `decimal`, `DateOnly`, `int`, `long` (boşalabilenler dahil) formdan, sorgudan ve yoldan; listenin başında (`FieldValidationSetup`). Kırpar; okunamayan değerde alanın dönüşüm hatasını üstlenen kuralın kodu ve sınırı (`InvalidAmount`, `InvalidPercentage`, `InvalidQuantity`, `InvalidDate`), kuralsız tarihte `InvalidDate`; boş değer boşalabilende `null`, değilse `FieldRequired`.
- **Gösterim (`Infrastructure/DisplayFormat.cs`):** `04 §8.2`'nin tarih, saat, tarih aralığı, sayı, yüzde, tutar, gün, iş günü, telefon ve IBAN biçimi; kültür her çağrıda açık (iş parçacığının kültürü ve `Accept-Language` metni değiştirmez); an Türkiye saatinde, saat dilimi yazılmaz; tutar ikiden çok ondalıkta yuvarlanmaz, hata verir.
- **Para sınırları (`Infrastructure/Money.cs`):** `MaxAmount` (para türünden, 9.999.999.999,99) ve `MaxPercentage` (99,99).
- **Kurallar ve mesajlar (`FieldRules.cs`, `FieldMessageCatalog.cs`, `FieldError.cs`):** `InvalidAmount` / `InvalidPercentage` mesajı sınırı anar (D-124); `InvalidAmountAttribute.Limit`; yeni `InvalidDateAttribute` (D-127 T216 payı); `InvalidQuantity`/`TextTooLong` parametresi `DisplayFormat.Number`; `InvalidPhone`/`InvalidIban` `Compact*` kullanır.
- **Makine gövdesi (`Binding/MachineJson.cs`):** web varsayılanları + `JsonNumberHandling.Strict`, salt okunur (KK8; ilk kullanıcısı T312 → D-149).
- **Mimari kurallar (`UnitTests/Architecture/ModelBindingRules.cs`):** `TimeBound` (DateTime/DateTimeOffset/TimeOnly bağlanmaz), `FloatBound` (float/double/Half bağlanmaz), `DecimalWithoutRule` (form/sorgu/sayfa parametresi `decimal`'i `InvalidAmount` ya da `InvalidPercentage` taşır), `MachineDate`, `ByteCountNotNumber`; `IsSimple`'dan `DateTimeOffset`/`TimeOnly` çıktı (D-125 T216 payı). İhlal örnekleri `ViolationSamples/ModelBinding/Models.cs`, sessiz komşular `AddNoteInput`, `CleanNotesPage`.
- **Örnek sayfalar (yalnız test sunucusu):** `/odeme/bicim-ornegi` (vitrin), `/panel/bicim-ornegi`, `/panel/bicim-formu`; `Pages/FormatSamples.cs`. Örnek sayfa iş parçacığının kültürünü de basar (testin kırılabildiğinin kanıtı).
- **Örnek sayfa sınırı sabitten (D-123 (a)):** `ContactInput.Amount` `Max = BindingLimits.AmountMax`; `FieldValidationTests` tutar ve adet mesajlarını katalog + nitelikten türetir.
- **Doküman:** `09 §7.2` m.4 (bağlayıcı tam sayıları da kapsar), m.6 (`12,` → 12, `,5` → 0,5 ölçüldü) ve "Yazımın kararı (T216)" notu; `CONTEXT.md` klasör haritasına T216 satırı; backlog.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/`: `RequestFormat.cs`, `DisplayFormat.cs` (yeni), `Money.cs`, `Binding/MachineJson.cs` (yeni), `Validation/{RequestValueBinder.cs (yeni), FieldRules.cs, FieldMessageCatalog.cs, FieldError.cs, FieldValidationSetup.cs}`
- `tests/Shopfolio.UnitTests/`: `Infrastructure/{RequestFormatTests, DisplayFormatTests, MachineJsonTests, MoneyTests, DataModelFoundationTests}.cs`, `Validation/{FieldRuleTests, FieldMessageCatalogTests}.cs`, `Architecture/{ModelBindingRules, ModelBindingRuleTests}.cs`, `Architecture/ViolationSamples/ModelBinding/Models.cs`
- `tests/Shopfolio.IntegrationTests/`: `Pages/FormatSamples.cs`, `Pages/Storefront/FormatSample.cshtml`, `Pages/Panel/{FormatSample, FormatForm}.cshtml`, `Pages/Panel/BindingForm.cshtml.cs`, `Web/{FormatPageTests, FieldValidationTests}.cs`
- `Docs/09_CODING_GUIDELINES.md` §7.2, `Docs/DEFERRED_BACKLOG.md`, `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `Docs/TASK_REPORTS/T215_REPORT.md` (post-merge satırı), `.claude/CONTEXT.md`, `.claude/memory/MEMORY.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Gösterim vitrinde ve panelde aynı Türkçe biçim; dil seçimi yok; `Accept-Language` biçimi değiştirmez | ✓ | `FormatPageTests.TheStorefrontAndThePanel_ShowTheSameValueAsTheSameTurkishText_AndAcceptLanguageEnChangesNothing` (TestServer + Kestrel; en isteğinde iş parçacığı `en-US` olduğu da doğrulanır), `ThePage_WritesTheFormsOf04…`, `DisplayFormatTests.EveryFormat_IsTheSame_WhateverTheThreadCulture`. Tarih alanının tarayıcı denetimindeki görünümü kalan risktir (kriterin kendisi; ekran task'ları) |
| 2 | İstek saat taşımaz; tarih Türkiye günü; saat dilimi yazılmaz | ✓ | `ModelBindingRules` `TimeBound` + `ModelBindingRuleTests`; `RequestFormatTests.TryParseDate_ReadsTheOneFormat_AndNoTime`; `DisplayFormatTests.Time_IsTurkeyTime_TwentyFourHours_WithoutSecondsOrZone`, `Date_OfAnInstantNearMidnight_FallsOnItsTurkeyDay` |
| 3 | Tarih `yyyy-AA-GG` | ✓ | `RequestFormat.TryParseDate`; `RequestFormatTests.TryParseDate_*`; `FormatPageTests.Post_WithADateInAnotherFormat_Is422WithTheDateMessage` |
| 4 | Tutar metin; virgül; tr-TR; yalnız rakam + en çok bir virgül; `12.50` 1250 olmaz | ✓ | `RequestFormat.TryParseDecimal`; `RequestFormatTests.TryParseDecimal_TheTableOf09…`, `…ATextBeyondThePrecisionOfADecimal_IsNotRead_NothingIsRounded`, `…ZerosThatChangeNothing…`; `FormatPageTests.Post_WithAnAmountInAnotherFormat_Is422…` (Kestrel dahil) |
| 5 | Telefon ve IBAN boşluk yok sayılır | ✓ | `RequestFormatTests.CompactPhone_*`, `CompactIban_AnIbanWithBlanks_IsEqualToTheOneWithout`; `FormatPageTests.Post_WithTheOneFormat_IsSaved…` (IBAN boşluklu → boşluksuz) |
| 6 | Baştaki/sondaki boşluk yok sayılır; yalnız boşluk boştur | ✓ | `RequestValueBinder` kırpar (T211 `TrimmingBinder` metinde); `FormatPageTests.Post_AnEmptyValue_IsNullForANullableField_AndBlankRequiredFieldsAreEmpty`, `Post_WithTheOneFormat_IsSaved…` (`" 7 "` → 7) |
| 7 | E-posta, kupon, stok kodu `I/İ/ı/i` aynı harf | ✓ | `DataModelFoundationTests.NormalizeIdentity_EmailCouponAndStockCodes_WithAnyOfTheFourIs_AreTheSameIdentity`, `…TheFourIs_ReachTheSameForm`, `…DoesNotDependOnTheCurrentCulture` |
| 8 | Parçalı yükleme JSON'u bayt sayısı JSON sayısı, tarih yok | ✓ | `MachineJson`; `MachineJsonTests` (3 test); `ModelBindingRules` `MachineDate`, `ByteCountNotNumber` + ihlal örnekleri `StampedResponse`, `DatedUploadRequest`, `TextBytesResponse` |

> `? Doğrulanamadı` **FAIL değildir** — kanıt eksikliğidir. FAIL'den ayrı tutulur.

### Öz denetim (bitiş kapısı 2)

> Devam işçisi 2 bütün maddeleri yeniden denetledi (yalnız kalanları değil).

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1 aynı değer vitrinde ve panelde aynı metin; `Accept-Language: en` aynı Türkçe | ✓ | `FormatPageTests.TheStorefrontAndThePanel_…` (iki sunucu; beklenen `FormatSampleValues.Texts()`'ten türetilir; `thread-culture` = `en-US`) |
| KK2 gece yarısına yakın an doğru gün | ✓ | `FormatPageTests.ThePage_WritesTheFormsOf04_AndTheInstantNearMidnightOnItsTurkeyDay` (20:59:59Z → 4 Ekim 23:59, 21:00Z → 5 Ekim), `DisplayFormatTests.Date_OfAnInstantNearMidnight_FallsOnItsTurkeyDay` |
| KK3 `2026-10-04` geçer, başka biçim red | ✓ | `RequestFormatTests.TryParseDate_ReadsTheOneFormat_AndNoTime` (13 satır), `FormatPageTests.Post_WithADateInAnotherFormat_Is422WithTheDateMessage` |
| KK4 `1234,50` geçer; `12.50`, `1.250,50`, `12,5,1`, `-5`, `12 TL`, `1e3` geçmez; sessizce büyütülmez | ✓ | `RequestFormatTests.TryParseDecimal_TheTableOf09…` (22 satır), hassasiyeti aşan 6 metin, `FormatPageTests.Post_WithAnAmountInAnotherFormat_Is422…` (12 satır; `1,999…9` Kestrel'de) |
| KK5 boşluklu IBAN boşluksuzla eşit | ✓ | `RequestFormatTests.CompactIban_…`, `FormatPageTests.Post_WithTheOneFormat_IsSaved…` |
| KK6 yalnız boşluk zorunlu alanda "boş olamaz" | ✓ | `FormatPageTests.Post_AnEmptyValue_…` (beklenen `FieldMessageCatalog.Build(FieldRequired, …)`) |
| KK7 `I`, `İ`, `ı`, `i` aynı harf | ✓ | `DataModelFoundationTests` T216 KK7 teorisi |
| KK8 JSON'da bayt sayısı sayı, tarih yok | ✓ | `MachineJsonTests.Write_ByteCountsAreJsonNumbers…`, `Read_ANumber_IsRead_AndANumberWrittenAsAText_IsNot`; `ModelBindingRuleTests` beklenen listesi |
| `09 §25` §7 satırı (zaman `TimeProvider`'dan, tarih ve tutar biçimi kurala uygun) | ✓ | `DisplayFormat`/`RequestFormat` saat okumaz; `09 §7.2` m.1–6 + T216 notu |
| `09 §25` mimari kural için kasıtlı ihlal örneği | ✓ | `ViolationSamples/ModelBinding/Models.cs` (`TimedInput`, `FloatQuery`, `UnruledAmountInput`, `TimedPage`, `StampedResponse`, `DatedUploadRequest`, `TextBytesResponse`) + sessiz komşular `AddNoteInput`, `CleanNotesPage`; `ModelBindingRuleTests` |
| `09 §25` yeni kuralın Test maddesi (§7.2 m.6) | ✓ | ayrıştırıcı tablosu, tarih tablosu, bağlayıcı testi (form 422 + kod, sorguda 200 ve süzgeç yok: `FormatPageTests.Get_AQueryInTheOneFormat…`) |
| `09 §25` §3.5 tablosu / `.env.example` | ✓ | yeni ayar ya da ürün sabiti yok (`Money.MaxAmount` sütun türünden türetilir) |
| `09 §25` `CONTEXT.md` klasör haritası | ✓ | `.claude/CONTEXT.md` T216 satırı |
| `09 §25` gerçek sunucu (FAIL nedeni 5) | ✓ | `FormatPageTests` Kestrel varyantları (KK1, kayıt, tutar reddi, sorgu) |
| `09 §25` testler silinmemiş/zayıflatılmamış | ✓ | `FieldValidationTests` beklentileri sabitten türetildi (elle yazılmış metin kaldırıldı), test silinmedi |
| `09 §25` güvenlik (sır, kişisel veri, yeni uç) | ✓ | sır yok; yeni üretim ucu yok (örnek sayfalar yalnız test sunucusunda); girdi doğrulaması daraldı |
| `12` ilgili maddeler | ✓ (uygulanamaz) | `07 §2.9`/`04 §8.2` anan satırlar `VAL-B069`, `VAL-B087` ekran (E-14, E-45) task'larınındır; `Durum` değişmedi |
| Sık FAIL 1–5 | ✓ | (1) tarama istisnası yok — kurallar yansımayla, `DecimalWithoutRule` yalnız iki kural türünü geçirir; (2) yansıma tabanlı kural nitelik yazımına bağlı değil (Attribute soneki/adlı argüman sorunu yok); (3) mesaj değişince `FieldValidationTests`, `FieldRuleTests`, `FieldMessageCatalogTests` güncellendi, `09` m.4/m.6, backlog sayacı (satır sayımı 89) güncellendi; (4) beklenenler `FieldMessageCatalog`/`DisplayFormat`/sabitten; (5) Kestrel |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/… -c Release --filter-not-trait "Consistency=true"` + `--filter-trait "Consistency=true"` | 2362 + 207 = 2569 geçti, 0 kaldı |
| Integration | ✓ | `dotnet test tests/Shopfolio.IntegrationTests` ad alanı/sınıf parçalarıyla (Web 3 parça, Concurrency+Database+Disposal+Files+FileSecrets, Jobs, Logging+Outages+Security+SignedEndpoints, Model); Postgres 18 konteyneri | 1680 geçti (bütün sınıflar); hassasiyet düzeltmesinden sonra etkilenen sınıflar yeniden: 97 + 80 geçti; toplam 1682 |
| Contract | — | `NOT_APPLICABLE` | — |
| Build | ✓ | `dotnet build Shopfolio.slnx -c Release -warnaserror` | 0 uyarı, 0 hata |
| Lint | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes` | çıkış 0 |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | *(validator doldurur)* |
| Bulgu sayısı | |
| Düzeltme gerekli mi | |
| Bloke-etmeyen not (K) sayısı | |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok (ICU imajda var; doğrulandı)
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T216-tarih-sayi-para-bicimi`
- **Commit:** *(PR açıldıktan sonra son commit)*
- **PR:** *(açılıyor)*
- **CI:** *(yardımcı izler)*
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; sonraki task dalında güncellenir)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Uygulama imajında ICU var, değişmez küreselleşme kipi kapalı (`09 §7.2` m.1) | `deploy/images/app/Dockerfile` `aspnet:10.0.14-noble`; imajda `libicu*.so.74`, `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT` yok (1. işçi); `Shopfolio.Web.csproj`'da `InvariantGlobalization` yok | ✓ |
| 2 | `tr-TR` ondalık ayırıcısı virgül, binlik nokta | `RequestFormatTests.Culture_IsTurkish_TheCommaIsTheDecimalSeparator_AndThePointTheGroupSeparator` | ✓ |
| 3 | Çatının `decimal` ayrıştırıcısı `12,` ve `,5`'i nasıl okur (`09` m.6 "bilgim tarihli") | ölçüldü: `12,` → 12, `,5` → 0,5; `09` m.6'ya yazıldı | ✓ |
| 4 | Çatının `decimal` ayrıştırıcısı fazla haneyi reddeder | ✗ — sessizce yuvarlıyor (ölçüldü); `TryParseDecimal` geri yazım eşitliğiyle reddeder | düzeltildi |
| 5 | Plan tier / kota / dış servis | yok | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `MachineJson.Options`'u kullanan parçalı yükleme uçları | D-149 | uçlar T312'nin |
| 2 | `IFormFile` dar istisnası | D-125 (T312 payı) | yükleme işleyicileri T312'nin |
| 3 | `FieldRuleSourceScan` `data-val` deseninin `data-value` yanlış pozitifi | D-123 (b) | T216 kapsamı dışı; en geç F2 gate |
| 4 | Tarih alanının tarayıcı denetimindeki görünümü | — | kabul kriterinde kalan risk olarak yazılı (`07 §2.9.9`; K-824) |

## Notlar

- **Working tree (Adım -1):** temiz (her üç işçide).
- **Hook yolu:** `core.hooksPath` = `scripts/git-hooks`.
- **Ana dal CI startup (Adım 0):** 42217406499 (CI, T215) success, 42217406517 (Publish, T215) success, 42207595893 (CI, T214) success, 42207595875 (Publish, T214) success.
- **Repo memory (bitiş kapısı 9):** güncellendi.
- **⚠ Kararlar:** (1) tam sayılar (`int`, `long`) da projenin bağlayıcısındadır — çatının dönüştürücüsü `0x10`'u 16, `+5`'i 5 okurdu (`09 §7.2` m.3'ün kuralı); eksi işaretli tam sayı alanı bugün yok, gerektiren task alanın kuralını yazar. (2) Hassasiyeti aşan tutar metni (28 haneden çok) okunmaz, alanın tutar kodu döner — sessiz yuvarlama yerine red. (3) `DecimalWithoutRule` yalnız `InvalidAmount`/`InvalidPercentage`'i geçirir; net miktar gibi yeni ondalık türünü kuran task kuralını ekler (`09` T216 notu (4)). (4) Backlog "Aktif kalem" sayacı paralel task birleştirmelerinde kaymıştı (76); satır sayımıyla 89'a düzeltildi (⚪ 87 + 🟡 2).
- **Önceki task kaydı:** T215 status Commit hücresi (`5ebe163`) ve `T215_REPORT` post-merge satırı dolduruldu (skill adım 15); eski durum girişi `STATUS_CHANGELOG.md`'ye taşındı.
- **Bölünme:** yapım üç işçide yürüdü (bağlam sınırı); devir notları yardımcı durum dizininde.
