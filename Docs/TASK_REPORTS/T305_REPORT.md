# T305 — E-posta ortak kalıbı ve gövde üretimi

**Faz:** F3 | **Durum:** ✓ Tamamlandı | **Tarih:** 2026-11-07

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- **İçerik modeli** (`Infrastructure/EmailComposition/EmailContent.cs`, `09 §11.4` m.1): tipli blok dizisi — `EmailHeading`, `EmailParagraph`, `EmailKeyValue` (tutar, tarih, sayı fabrikaları `DisplayFormat`'tan), `EmailButton` (yalnız tam `https` adresi; dış adres `IsExternal` → `noreferrer`), `EmailLegalText`; `EmailContent` (başlık, sipariş numarası, bloklar, yeniden gönderim notu, logo genişliği), `EmailResend` (tek cümle), `EmailBranding`, `EmailFirm`. İki yazıcı aynı modeli okur; "düz metin eşdeğerdir" yapısaldır.
- **Çerçeve** `EmailFrame.razor` (`HtmlRenderer`): tek sütun, `max-width:600px`, tablo yerleşimi, satır içi biçim, sistem yazı tipi; betik, dış stil, form, izleme pikseli yok; logo (kaynak kopya, mutlak adres, alt metin marka adı) ya da marka adı metni; ana düğme + altında aynı adres düz metin; yasal metin 12 punto, boş satırla paragraf; altbilgi (unvan, iletişim e-postası, yanıt cümlesi). Her değer Razor kodlamasından geçer.
- **Düz metin yazıcısı** `EmailTextWriter`: aynı `EmailFrameModel`'den; değerlerde satır sonu boşluk (`EmailText.OneLine`); yasal metin satırlarını korur.
- **Birleştirici** `EmailComposer` → `EmailMessage`: konu `"{marka adı}: {başlık}"` (+ `" (Sipariş {no})"`), marka adı satır sonu / denetim karakterinden arınır, adsızda alan adı; gönderen adı aynı arındırmayla; yanıt adresi firma e-postası.
- **Adresler** `EmailLinks`: kök yalnız kurulum ayarındaki alan adı (`SiteAddress`); `MemberOrder` `/siparis/{no}`, `GuestOrder` `/siparis/erisim/{anahtar}` (yüzde kodlu), `Logo` `/gorsel/` altı.
- **Renk** `EmailColors`: marka rengi yalnız düğme zemini; yazı rengi WCAG kontrastıyla siyah/beyaz; `#RRGGBB` değilse (style özniteliğine girdiği için) sabit ürün rengi.
- **Mimari tarama** `EmailRenderingSourceScan` (`09 §11.4` m.2, `§25`): e-posta ad alanında ham işaretleme (`MarkupString`, `HtmlString`, `Html*Content*`, `Html.Raw`, `Raw(`, `AddMarkupContent`, `WriteLiteral`) ve istek / `Host` başlığı okuması yasak; `.cs` + `.razor`; ihlalli örnek `ViolationSamples/EmailRendering/Misuse.cs`.
- **Kayıt:** `EmailLinks` ve `EmailComposer` `AddNotificationsModule`'da singleton.
- **Belgeler:** `CONTEXT.md` Infrastructure satırı; backlog D-167; T313 / T314 / T378 raporlarının "Post-merge run'lar" satırı ve status Commit hücreleri dolduruldu (`f94b4d8`, `335e9cb`, `552a13d`).

## Etkilenen Modüller / Dosyalar

- Yeni: `src/Shopfolio.Web/Infrastructure/EmailComposition/` (`EmailContent.cs`, `EmailFrameModel.cs`, `EmailFrame.razor`, `EmailTextWriter.cs`, `EmailText.cs`, `EmailLinks.cs`, `EmailColors.cs`, `EmailComposer.cs`).
- Değişen: `Modules/Notifications/NotificationsModule.cs` (kayıt), `src/Shopfolio.Web/packages.lock.json` (Razor SDK'nın örtük `Microsoft.AspNetCore.App.Internal.Assets` 10.0.11 girdisi), `.claude/CONTEXT.md`, `Docs/DEFERRED_BACKLOG.md`, `Docs/IMPLEMENTATION_STATUS.md`, önceki üç task raporu.
- Test: `tests/Shopfolio.UnitTests/Infrastructure/EmailCompositionTests.cs`, `Architecture/EmailRenderingSourceScan.cs`, `EmailRenderingSourceScanTests.cs`, `ViolationSamples/EmailRendering/Misuse.cs`.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | HTML + düz metin; düz metinde tam adres | ✓ | `TheMessage_HasAnHtmlPartAndAPlainTextPart_…`, `TheLink_IsTheSameFullAddress_…`, `TheTwoParts_CarryTheSameValues…` |
| 2 | Tek sütun, ≤ 600 px; betik, dış stil, form, piksel yok | ✓ | `TheHtml_IsOneColumnOfAtMost600Pixels_…`, `TheHtml_HasNoScript_NoExternalStyle_NoForm_NoTrackingPixel_…` |
| 3 | Logo mutlak adres / yoksa ad; alt metin marka adı; renk yalnız düğme zemini; yazı kontrastlı | ✓ | `TheLogo_IsTheSourceCopy…`, `WithoutALogo_…`, `TheTextOnTheButton_IsBlackOrWhiteByContrast`, `TheBrandColour_IsOnlyTheMainButtonsBackground` |
| 4 | Konu biçimi, sipariş eki, adsızda alan adı, satır sonu arınma | ✓ | `TheSubject_IsBrandColonTitle_…`, `WithoutABrandName_TheDomainStandsInItsPlace_…`, `ABrandNameWithLineBreaksAndControlCharacters_IsOneLine_…` |
| 5 | Değişken değer HTML'de kaçışlı; düz metinde satır sonu boşluk | ✓ | `AValueWithMarkup_IsTextInTheHtml_…`, `TheFirmsValues_AreEncodedToo_…`, `EmailRenderingSourceScanTests` (ham işaretleme yolu yok) |
| 6 | Tam `https`; düğme + altında düz adres; misafir anahtarlı, üye hesap adresi; B-15 / silinmede bağlantısız | ✓ | `TheMainAction_IsAButton_AndTheSameAddressIsPlainTextUnderIt`, `TheGuestOrderLink_…`, `AButtonAddress_IsAFullHttpsAddress`, `AnEmailWithoutAButton_HasNoLinkAtAll_…` |
| 7 | Tarih, tutar, sayı `04 §8.2` biçiminde, olay kaydından | ✓ | `AmountsDatesAndNumbers_AreInTheDisplayFormat` ("1.200 TL", "266,67 TL", "4 Ekim 2026") |
| 8 | Yeniden gönderimde aynı konu + tarihli tek cümle; yeni olayda cümle yok | ✓ | `AResend_KeepsTheSubject_…`, `ANewEventOfTheSameType_CarriesNoResendSentence`, `TheResendDate_IsTheTurkeyDayOfTheFirstEmail` |
| 9 | Altbilgi: unvan, e-posta, yanıt cümlesi; abonelik / pazarlama yok | ✓ | `TheFooter_HasTheLegalName…` |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| KK1 iki parça; düz metinde tam adres | ✓ | `EmailCompositionTests` — `TheMessage_…`, `TheLink_IsTheSameFullAddress_…` |
| KK2 betik, dış stil, form, izleme pikseli yok | ✓ | `…HasNoScript_NoExternalStyle_NoForm_NoTrackingPixel_AndNoImageWithoutALogo` (`<script`, `<link`, `<style`, `@import`, `<form`, `<input`, `<iframe`, `<img`, `onload`, `background-image` …) |
| KK3 logo mutlak, alt metin marka adı; logosuzda ad metni; renk yalnız düğme; yazı kontrastlı | ✓ | `TheLogo_…`, `WithoutALogo_…`, `ALogoPathOutsideTheImageAddress_IsRefused`, `ABrandColorThatIsNotSixHexDigits_…` (style enjeksiyonu), `TheBrandColour_IsOnlyTheMainButtonsBackground` |
| KK4 konu biçimi, sipariş eki, adsızda alan adı, satır sonlu ad tek satır | ✓ | `TheSubject_…`, `WithoutABrandName_…` (null / boş / boşluk / denetim karakteri), `ABrandNameWithLineBreaks…` (CR LF, NEL, U+2028, BEL) |
| KK5 `<b>`'li ad HTML'de metin; düz metinde satır sonu boşluk | ✓ | `AValueWithMarkup_…`, `AValueThatLooksLikeALinkOrAnAttribute_StaysText`, `ALegalText_KeepsItsLines…` |
| KK6 misafir / üye ayrı; düğme altında düz adres; B-15 ve silinmede bağlantı yok | ✓ | `TheGuestOrderLink_…`, `AValueInThePath_IsPercentEncoded_…`, `AnExternalAddress_CarriesNoReferrer_…`, `AnEmailWithoutAButton_HasNoLinkAtAll_…`, `TheAddressRoot_IsTheDomainSetting_Always` |
| KK7 "1.200 TL" ve "4 Ekim 2026" | ✓ | `AmountsDatesAndNumbers_…` (21:00 UTC → "5 Ekim 2026"; "-50 TL"; "₺" / "TRY" yok) |
| KK8 yeniden gönderim cümlesinde ilk satırın tarihi; ikinci hatırlatmada yok | ✓ | `AResend_…`, `ANewEventOfTheSameType_…`, `TheResendDate_…` |
| KK9 altbilgi doğru, abonelik yok | ✓ | `TheFooter_HasTheLegalNameTheContactEmailAndTheReplySentence_AndNoSubscriptionOrMarketing` |
| `09 §11.4` m.7: HTML özel karakterli ad kaçışlı, iki parça aynı modelden, sahte `Host` | ✓ | yukarıdakiler; sahte `Host` kapısı: `EmailRenderingSourceScan` (istek / `Host` okuması yok) + `EmailLinks` yalnız `SiteAddress` (alan adı ayarı) |
| `09 §25` yeni yasak için mimari test + ihlalli örnek | ✓ | `EmailRenderingSourceScanTests.Scan_OnTheViolationSample_ReportsEveryShapeOfEveryRule` (15 satır, her çağrı biçimi; çok satırlı `Html` / `.Raw`, cast, `Html*Content*` ailesi, `Host` başlık anahtarları), `…ApplicationSource_OfTheEmailNamespace_…` (tarama gerçekten çerçeve, birleştirici ve yazıcıya erişti) |
| `09 §25` klasör düzeni → `CONTEXT.md` | ✓ | `.claude/CONTEXT.md` Infrastructure satırı |
| Güvenlik taraması | ✓ | Aşağıda "Notlar" |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj --no-build --filter-not-trait "Consistency=true"` ve `--filter-trait "Consistency=true"` | 2802 geçti + 207 Consistency geçti, 0 başarısız (yeni: 56 — 51 e-posta, 5 tarama) |
| Integration | — | koşulmadı (task beklentisi Unit; Integration projesi derlendi, `packages.lock.json` değişmedi) | dal CI'ı koşar |
| Contract | — | — | — |

`dotnet format --verify-no-changes` (Web ve UnitTests, değişen yollar) temiz.

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-07, head `2ee4f63`) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 3 (K1 bu PR'da kapatıldı; K2 → D-189; K3 → D-190) |

### Doğrulama tur 1 — ✓ PASS

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` ana deponun `scripts/git-hooks`'u; main son üç tamamlanmış run 42351368236, 42351368096, 42346300012 `success`; hafızada T305 satırı var (satır 22).

**Kriterler 1–9 ✓, KK1–KK9 ✓, `09 §25` ✓.** Kanıtlar (doğrulayıcı, kod okuması + testler):

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kriter 1 / KK1 | ✓ | `EmailComposer.ComposeAsync` aynı `EmailFrameModel`'den `HtmlBody` (Razor `EmailFrame` + `HtmlRenderer`) ve `TextBody` (`EmailTextWriter`) üretir; düğme adresi düz metinde `Link.AbsoluteUri`; `TheLink_IsTheSameFullAddress_…`, `TheTwoParts_CarryTheSameValuesAndTheSameAddresses_FromOneModel` |
| Kriter 2 / KK2 | ✓ | `EmailFrame.razor`: dış tablo `width="600"` + `max-width:600px`, tek sütun, satır içi biçim, sistem yazı tipi; `<script>`, `<link>`, `<style>`, `<form>` yok, tek `<img>` yalnız logo varken; `TheHtml_HasNoScript_NoExternalStyle_NoForm_NoTrackingPixel_AndNoImageWithoutALogo` |
| Kriter 3 / KK3 | ✓ | Logo `EmailLinks.Logo` (yalnız `/gorsel/` altı, kök `SiteAddress` = `https://` + alan adı ayarı), `alt` = marka adı (adsızda alan adı); logosuzda başlık metni; `EmailColors.Button`: marka rengi yalnız düğme `bgcolor`/`background-color`, yazı WCAG oranı yüksek olan siyah/beyaz (`02 §3.29.5`), `#RRGGBB` değilse sabit renk; `TheBrandColour_IsOnlyTheMainButtonsBackground` (renk iki kez, ikisi de düğme zemini), `TheTextOnTheButton_IsBlackOrWhiteByContrast` |
| Kriter 4 / KK4 | ✓ | `EmailComposer.Subject`: `OneLine(brand) + ": " + OneLine(title)` + `" (Sipariş {no})"`; başlık şablonun sabiti, müşteri metni modele girmez; `EmailText.BrandOrDomain`; `OneLine` denetim karakterlerini (CR, LF, NEL, BEL) ve U+2028/2029'u boşluğa çevirir; `ABrandNameWithLineBreaksAndControlCharacters_IsOneLine_…`, `WithoutABrandName_TheDomainStandsInItsPlace_…` |
| Kriter 5 / KK5 | ✓ | Çerçevede her değer `@…` ile Razor kodlamasından geçer (öznitelik dahil); `EmailRenderingSourceScan` ad alanında `MarkupString`/`HtmlString`/`Html.Raw`/`AddMarkupContent` yok; düz metin yazıcısı her değeri `OneLine`'dan geçirir; `AValueWithMarkup_IsTextInTheHtml_AndTheLineBreaksAreBlanksInThePlainText`, `TheFirmsValues_AreEncodedToo_…`, `AValueThatLooksLikeALinkOrAnAttribute_StaysText` |
| Kriter 6 / KK6 | ✓ | `EmailButton` yalnız mutlak `https` kabul eder; çerçevede düğmenin hemen altında aynı adres `<p>` metni; `EmailLinks.GuestOrder` `/siparis/erisim/{anahtar}`, `MemberOrder` `/siparis/{no}` (`07 §2.1.3`, UC-33/UC-34), değer `Uri.EscapeDataString`; dış adreste `rel="noreferrer"`; düğmesiz içerik bağlantısız (B-15, silinme); `TheMainAction_IsAButton_AndTheSameAddressIsPlainTextUnderIt`, `AnEmailWithoutAButton_HasNoLinkAtAll_…` |
| Kriter 7 / KK7 | ✓ | `EmailKeyValue.Amount/Date/Number` ve `EmailResend.Sentence` yalnız `DisplayFormat`'ı çağırır (`04 §8.2`; tarih Türkiye günü); değer çağırandan gelir, bileşen saat okumaz; `AmountsDatesAndNumbers_AreInTheDisplayFormat` ("1.200 TL", "266,67 TL", "-50 TL", "4 Ekim 2026", 21:00 UTC → "5 Ekim 2026", "1.250") |
| Kriter 8 / KK8 | ✓ | `EmailResend` yalnız şablon verirse cümle gövdenin en üstünde (HTML ve düz metin); konu `Resend`'den bağımsız; `AResend_KeepsTheSubject_AndPutsTheDatedSentenceOnTop_InBothParts`, `ANewEventOfTheSameType_CarriesNoResendSentence`, `TheResendDate_IsTheTurkeyDayOfTheFirstEmail` |
| Kriter 9 / KK9 | ✓ | Altbilgi `FirmName`, `FirmEmail`, `ReplySentence` ("Bu e-postayı yanıtlayarak firmaya ulaşabilirsiniz."); abonelik/pazarlama/platform imzası yok; yanıt adresi firma e-postası; `TheFooter_HasTheLegalName…AndNoSubscriptionOrMarketing` |
| `09 §25` | ✓ | `HtmlRenderer` çatıyla (yeni paket yok, kilit dosyaları main ile aynı; `RequiresAspNetWebAssets=false` örtük Blazor varlık paketini kapatır); yeni ayar/göç yok; kaynak taraması + ihlalli örnek (`Scan_OnTheViolationSample_ReportsEveryShapeOfEveryRule`); `CONTEXT.md` Infrastructure satırı güncel; zaman ve tutar biçimi `DisplayFormat`'tan; gövde saklanmaz (`05 §4.4.2`) |
| Güvenlik | ✓ | sır yok; uç ve yetki etkisi yok; girdi: HTML kodlaması, konu/gönderen adı tek satır, yol değeri yüzde kodlu, renk `#RRGGBB` doğrulamalı (style özniteliği), logo yolu `/gorsel/` önekli, düğme yalnız `https`; adres kökü istek başlığından okunmaz (tarama); yeni dış bağımlılık yok |

**Testler (doğrulayıcı, yerel):** `dotnet build Shopfolio.slnx -warnaserror` 0 uyarı 0 hata; `dotnet format --verify-no-changes` Web, UnitTests, IntegrationTests ayrı ayrı temiz; Unit 3049/3049 (Architecture + Consistency + Locks 987, kalan 2062; T305 sınıfları 56/56); betik testleri 4 dosya geçti; Integration gerçek PostgreSQL 18 (yerel konteyner) parçalı, hepsi geçti: `MigrationBundleTests` 2, Web 592 (beş parça), Security + Pages 32, Concurrency + Jobs 103, Database 36, Model 893 (üç parça), Disposal/FileSecrets/Files/Logging/Outages/SignedEndpoints 135 — toplam 1793; koşu sonrası `git status` temiz, `dotnet restore Shopfolio.slnx --locked-mode` çıkış 0. Dal CI 42352393327 `success` (head `2ee4f63`); önceki iki run (Lint, Paketleme) Düzeltme 1–2 ile kapandı.

**Task satırları (`12 §7.5`):** T305'in KP birimleri KP-66 ve KP-68 (`11 §7.1`). Satırları `VAL-A317`–`A325` ve `VAL-A332`–`A336` (14 satır) bildirimin gerçekten gönderilmesini (sahte e-posta altyapısı, olayın tetiği, yeniden deneme, işaret, panelden yeniden gönderim) ister; T305 gönderen yazmaz (D-97, D-166), bu yüzden hiçbiri T305'in kriterine eşlenmez ve `Durum`'ları `BEKLEMEDE` kalır. `08 §2.2.5`'i anan `VAL-A159` (KP-56) da gönderilmiş e-posta ister; T305'in kalıbı onun beklenen sonucunu (logo mutlak adres, alt metin, düğme zemini, imza yok) birim düzeyde karşılar, satır sahip task'ında (gönderen) okunur.

**K1 (bu PR'da kapatıldı):** D-97 ve D-166 göndericiyi ve sınırın çağrı noktasını "T305/T385/T407"ye bırakıyordu; T305 kriterleri göndericiyi kapsamaz ve T305 yazmadı. İki kaydın sahibi T385/T407 olarak düzeltildi, `EmailComposer`'ın hazır olduğu not edildi.

**K2 → D-189:** Düğmenin varsayılan rengi (`EmailColors.DefaultButtonBackground = "#1A56DB"`) "ürünün varsayılan rengi"nin (`02 §3.29.5`) tek kaynağı değil; vitrinin tasarım sistemi rengi tanımlayınca e-posta aynı kaynaktan okumalı.

**K3 → D-190:** `09 §11.4` madde 7'nin "düz metin parçasında … etiket yoktur" cümlesi iki biçimde okunabilir; kural (madde 2), `08 §2.2.4` ve kriter 5 / KK5 yalnız satır sonunu ister ve eşdeğerlik (`08 §2.2.5` madde 1) değerin olduğu gibi kalmasını gerektirir. T305 bu okumayla doğru (yapım raporu ⚠ 2 ile aynı); cümle doküman geçişinde netleşmeli.

**Yapım raporu karşılaştırması:** uyumlu; öz denetim tablosu her kriter, KK, `09 §11.4` m.7 ve `09 §25` için kanıtlı. Raporun Integration'ı "koşulmadı" diyor; doğrulayıcı yerelde parçalı koştu (hepsi geçti). Raporun ⚠ 2'si K3 olarak backlog'a taşındı; D-97/D-166 sahiplik düzeltmesi (K1) raporda yoktu. Raporda `12` task satırları bölümü yok (bu doğrulamada yazıldı).

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok (yeni ayar, süre, tavan yok; `.env.example` değişmedi)
- **Paketleme / dağıtım:** `packages.lock.json`'a Razor SDK'nın örtük `Microsoft.AspNetCore.App.Internal.Assets` 10.0.11 girdisi eklenmişti; Düzeltme 2'de kaldırıldı (`packages.lock.json` değişmez; ⚠ 3)
- **Yeni bağımlılık:** Yok (`HtmlRenderer` çatıyla gelir; `09 §11.4`)

## Commit & PR

- **Branch:** `task/T305-eposta-kalibi`
- **Commit:** `8943077` — T305: E-posta ortak kalıbı ve gövde üretimi (üstüne PR numarası commit'i)
- **PR:** #422
- **CI:** *(yardımcı izler)*
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; ayrı `chore:` PR'ında veya sonraki task dalında)*

## Dış Varsayımlar

Tetikleyici tarama (`plan tier`, kota, sürüm, registry, …): iki varsayım bulundu.

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | `HtmlRenderer` (`Microsoft.AspNetCore.Components.Web`) ek paketsiz çatıyla gelir ve `net10.0` Web SDK'sında .razor derlenir | `dotnet build src/Shopfolio.Web` 0 hata, 0 uyarı; Directory.Packages.props'a satır eklenmedi; render testleri geçti | ✓ |
| 2 | Razor bileşeni her zaman `public`tir; modül içinde `public` tür yasak (`ModuleBoundaryTests.ModuleTypes_OutsideContracts_AreNotPublic`) | önce `internal` model + bileşen denendi: CS0053 / CS0262 (Probe derlemesi); çözüm: bileşen ve model `Infrastructure/`'a (orada `DisplayFormat` gibi `public` tür vardır) | ✓ (⚠ 1) |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Kaçış taraması belge (yasal metin) ad alanlarını da okuyacak | D-167 | Belge metnini üreten kod henüz yok (T366) |
| 2 | Gönderen adındaki `"` ve `\` ters bölüyle kaçışı (`08 §2.2.2`) | — | `FromEmailAddress`'i kuran SES uygulamasının işi: T407 kriter 4; T305 yalnız satır sonu / denetim arındırmasını yapar |

## Notlar

- **Working tree (Adım -1):** temiz (yeni worktree); `core.hooksPath` bekçi kapsamında.
- **Ana dal CI startup (Adım 0):** `gh run list --branch main`: 42346300012 (CI) success, 42346300006 (Publish) success, 42344353123 (CI) success — üç tamamlanmış run başarılı.
- **Repo memory (bitiş kapısı 9):** güncellendi.
- **Önceki task kayıtları (adım 15):** T313 (`f94b4d8`, CI 42346300012, Publish 42346300006), T314 (`335e9cb`, 42344353123 / 42344353073), T378 (`552a13d`, 42343289320 / 42343289321) için rapor satırı ve status Commit hücresi dolduruldu. "Son güncelleme" bloğuna dokunulmadı (doğrulamanın).
- **Mini güvenlik kontrolü:** secret yok; yetki / kimlik doğrulama etkisi yok (yalnız metin üretimi, uç yok); girdi denetimi: her değer HTML'de Razor kodlamasıyla, konu / gönderen adı `OneLine` ile, yol değeri `Uri.EscapeDataString` ile, renk `#RRGGBB` doğrulamasıyla (style özniteliği), logo yolu `/gorsel/` önekiyle sınırlı, düğme adresi yalnız `https`; adres kökü istek başlığından hiçbir yerde okunmaz (tarama); yeni dış bağımlılık yok.

### ⚠ Kolay kararlar (kurala en uygun seçenekle verildi)

1. **Bileşen ve model yeri:** `Infrastructure/EmailComposition/` (modül dışı), model türleri `public`. Sebep: Razor bileşeni her zaman `public`tir, parametre türü de `public` olmak zorundadır; modül içinde `public` tür `09 §6` m.1'e ve mimari teste aykırıdır. `09 §11.4` yer söylemez; `CONTEXT.md` güncellendi. Birleştirici, adresler ve yazıcılar `internal`dır.
2. **Düz metin parçasında HTML benzeri değer olduğu gibi gider** (yalnız satır sonu boşluk olur): `08 §2.2.4` düz metin için başka bir şey istemez; `09 §11.4` m.7'nin "düz metin parçasında etiket yoktur" cümlesi şablonun kendi işaretlemesi olarak okundu (düz metin işaretleme yorumlamaz). Aksi okuma doğrulamada çıkarsa etiket temizleme eklenir.
3. **`packages.lock.json` değişmez (Düzeltme 2 ile düzeltildi):** ilk turda .razor dosyasının getirdiği örtük `Microsoft.AspNetCore.App.Internal.Assets` girdisi kilide yazılmıştı; kapsayıcı geri yüklemesi yalnız csproj'u gördüğü için `NU1004` verdi. Örtük ekleme `RequiresAspNetWebAssets=false` ile kapatıldı, kilit `main`'deki hâline döndü.
4. **HTML'de Türkçe karakterler sayısal başvuru** (`&#xDC;` gibi) çıkar: `HtmlRenderer`'ın varsayılan kodlayıcısı; geçerli HTML, istemcide aynı görünür, testler çözerek karşılaştırır.
5. **Yeniden gönderim kararını şablon verir:** `EmailContent.Resend` null ise cümle yoktur; "ilk e-postanın tarihi" ve "yeni olay mı" ayrımı `OriginalDispatchId`'yi okuyan T306+ şablonlarının işidir (`08 §2.2.5` m.6); T305 yalnız cümleyi ve yerini üretir.
6. **Logo yolu `/gorsel/` altında** zorunlu (`07 §2.1.2`); başka yol `ArgumentException`. Logonun kaynak kopya adresini seçmek (WebP türevi değil) Branding'i okuyan şablon / T306 işidir.

## Düzeltme 1

**Neden:** PR #422 CI'ı (head `8aa0b2c`, run 42347826185) "1. Lint"te kırmızıydı: `EmailComposer.cs(79,13): error IDE0008: Use explicit type instead of 'var'`. Önceki tur `dotnet format` denetimini yalnız değişen yollarda koşmuştu; CI bütün çözümü denetler.

**Düzeltme:** `RenderAsync` içindeki `var root` açık türe çevrildi (`HtmlRootComponent root`, `using Microsoft.AspNetCore.Components.Web.HtmlRendering;` eklendi). Davranış değişmedi. `origin/main` dalda zaten vardı; birleştirme gerekmedi. Yeni backlog kaydı yok.

| Denetim | Sonuç | Komut / Çıktı |
|---|---|---|
| Biçim, bütün çözüm | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` rc=0; Web, UnitTests, IntegrationTests projeleri ayrı ayrı da temiz (Razor dahil) |
| Consistency | ✓ | `--filter-trait "Consistency=true"`: 207 geçti, 0 başarısız |
| Unit | ✓ | `-c Release --filter-not-trait "Consistency=true"`: 2802 geçti, 0 başarısız |
| Web derlemesi | ✓ | 0 uyarı, 0 hata |

**Öz denetim:** Diff tek dosyada, bir tür adı ve bir `using` satırı (rapor hariç); kabul kriterleri 1–9 ve yukarıdaki "Öz denetim" tablosunun bütün maddeleri değişmeden ✓ (kanıt testleri yeşil: 2802 + 207). `sik-fail-nedenleri.md` taraması: bu düzeltme tarama istisnası, DB, kural sayısı ya da belge kuralı değiştirmiyor; ilgili sık neden yok.

## Düzeltme 2

**Neden:** PR #422 CI'ı (head `1dbd6b0`, run 42350175720) "7. Paketleme doğrulaması"nda kırmızıydı: kapsayıcıda `dotnet restore src/Shopfolio.Web/Shopfolio.Web.csproj --locked-mode` `NU1004: The package references have changed`. Kök neden: Web SDK, projede `.razor` dosyası görünce `RequiresAspNetWebAssets=true` yapar ve `Microsoft.AspNetCore.App.Internal.Assets` (Blazor betik varlıkları) paketini restore'a örtük ekler. İlk tur bu girdiyi `packages.lock.json`'a yazmıştı; `deploy/images/app/Dockerfile` geri yüklemeyi yalnız csproj + kilit ile (kaynaksız, `.razor` yok) koşar, orada örtük ekleme olmaz, kilit "fazla" kalır.

**Düzeltme:** `src/Shopfolio.Web/Shopfolio.Web.csproj`'a `<RequiresAspNetWebAssets>false</RequiresAspNetWebAssets>` (gerekçe yorumuyla); uygulama Blazor betiği sunmaz, `HtmlRenderer` bu pakete bağlı değildir. Üç `packages.lock.json` (Web, UnitTests, IntegrationTests) `origin/main` ile birebir; yeni paket ve `Directory.Packages.props` satırı yok (üçüncü bir seçenek olan açık `PackageReference` 09 Kütüphaneler tablosu satırı ve tutarlılık testi gerektirirdi, seçilmedi). `origin/main` (T304 #421) dala birleştirildi (rebase değil): STATUS "Son güncelleme" main'in girişi, MEMORY satırları hepsi, backlog'da main kayıtları (D-166, D-172) + D-167, "Aktif kalem" 97 (93 ⚪ + 3 🟡 main'de 96, artı D-167); T305 STATUS satırı ⏳ + #422; `NotificationsModule` kayıtları birlikte. Yeni backlog kaydı yok.

| Denetim | Sonuç | Komut / Çıktı |
|---|---|---|
| Kilitli geri yükleme, çözüm | ✓ | `dotnet restore Shopfolio.slnx --locked-mode --force`: üç proje temiz |
| Kapsayıcı görünümü (csproj + kilit, `.razor` yok) | ✓ | kopya dizinde `--locked-mode` temiz (önceki tur: NU1004) |
| Uygulama imajı | ✓ | `docker build -f deploy/images/app/Dockerfile -t t305-app .`: geri yükleme ve `publish` adımları geçti (migration paketi adımı ön plan sınırında kesildi; adımdan öncesi kanıt); imaj/kaynak silindi |
| Web derlemesi | ✓ | 0 uyarı, 0 hata |
| Biçim | ✓ | `dotnet format --verify-no-changes`: Web, UnitTests, IntegrationTests temiz |
| Unit | ✓ | `--filter-not-trait "Consistency=true"`: 2840 geçti, 0 başarısız (T305: 56); Architecture 769, Consistency 209 geçti |

**Öz denetim:** kabul kriterleri 1–9 ve "Öz denetim" tablosu diff'te yeniden denetlendi; Düzeltme 2 yalnız csproj özelliği + birleştirme + rapor; ürün kodu, test, belge kuralı değişmedi, tüm kanıt testleri yeşil (T305 56, Architecture 769). `sik-fail-nedenleri.md`: "aynı sayıyı anan öteki yerler" — backlog sayacı (97), STATUS satırı, MEMORY ve rapor ⚠ 3 / Altyapı satırı güncellendi; tarama istisnası, DB, karar kaydısız 09 metni yok.
