# T542 — Rozet ve işaretler, sayaç ve süre göstergeleri, eşzamanlı düzenleme uyarısı (OB-05, OB-06, OB-10)

**Faz:** F5 | **Durum:** ⏳ Yapım tamamlandı, doğrulama bekliyor | **Tarih:** 2026-11-11

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- `Pages/Components/StatusBadges.cs`: altı rozet ailesi (sipariş sevkiyat ve ödeme ekseni, yayın durumu, kurumsal içerik, kusur talebi, iletişim talebi); siparişin iki ekseni `OrderBadges` ile iki ayrı rozet. Hesap ve davet için rozet ailesi yok.
- `ItemRecordLabels.cs`: on kalem kaydı adı; kalem rozet taşımaz, kayıt satırında kendi adıyla görünür (`_ItemRecords`).
- `PanelMarks.cs`: yedi işaret, liste ve ayrıntı yeri (listede IBAN ve iade malı işareti yok); işaret her çizimde koşuldan kurulur, koşul kalkınca kalkar.
- `UndeliveredEmails.cs` + `_UndeliveredEmails`: "e-posta ulaşmadı" satırları; yeniden gönder yalnız `CanResend` satırlarında, davet satırı işareti taşır ama düğmesi yok; "Hepsini yeniden gönder" yalnız birden çok gönderilebilir satır varsa; düğme ikinci basışı engeller (`data-js="resend-once"`, `common.js`).
- `PendingCounters.cs` + `_PendingCounters`: altı sayaç (02 10.6.1 adları), sıfır yazılır, her biri süzülmüş listeye gider (07 5.3.2); `AwaitingCustomerList` ve `_AwaitingCustomerList`: "IBAN bekleniyor" sayaç değil satır listesi, ilk on, en yakın biten önce.
- `PeriodIndicator.cs` + `_PeriodIndicator`: gün ve son gün tarihi, iş günü birimi, aşımda "Süre aşıldı: N gün geçti", kanuni faiz uyarısı; canlı geri sayım ve saat yok.
- `ThresholdMessage.cs` + `_ThresholdMessage`: eşik mesajı `ShippingAssessment`'ın kendi cümlesi; koşul yoksa mesaj yok.
- `ConflictControls.SectionSaveControl`: bölümlü ekranda birleşme uyarısı yalnız kaydedilen bölümde; kayıt ve sipariş varyantları önceden vardı (`RecordSaveControl`, `OrderNotice`).
- CSS (`base.css` ton değişkenleri, `components.css`, `admin.css`) ve `common.js` `bindResendOnce`.
- Bu tur (devam): derleme hatası düzeltildi (`PeriodIndicator` using), görünümde `DateTimeOffset` adı geçmesin diye `ItemRecordEntry.MomentText` (OperationSourceScan), test düzeltmeleri (CA1309, test gövdesinde doğrudan assert), Integration örnek sayfaları ve testleri, kayıtlar.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Pages/Components/*`, `Pages/Shared/_Badge, _OrderBadges, _ItemRecords, _PanelMarks, _UndeliveredEmails, _PendingCounters, _AwaitingCustomerList, _PeriodIndicator, _ThresholdMessage.cshtml`, `Infrastructure/Forms/ConflictControls.cs`, `wwwroot/css/{base,components,admin}.css`, `wwwroot/js/common.js`.
- Testler: `tests/Shopfolio.UnitTests/Pages/StatusComponentTests.cs`, `IndicatorComponentTests.cs`; `tests/Shopfolio.IntegrationTests/Web/ComponentMarkupTests.cs` (StorefrontFrameTests'in parçası), örnek sayfalar `Pages/ComponentSamples.cs`, `Pages/Panel/ComponentSample.cshtml` (`/panel/siparisler/bilesen-ornegi`), `Pages/Storefront/ComponentThresholdSample.cshtml` (`/odeme/esik-ornegi`).
- Belgeler: `Docs/IMPLEMENTATION_STATUS.md` T542 satırı; `Docs/STATUS_CHANGELOG.d/T542.md`; `Docs/DEFERRED_BACKLOG.d/T542.md`. `12` Durum hücrelerine dokunulmadı.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Rozet her zaman metin taşır; iki eksen iki rozet | ✓ | `StatusComponentTests.EveryBadgeCarriesText_…_KK1`, `AnOrder_IsShownWithTwoSeparateBadges_…_KK1`; entegrasyon `TheOrderShowsTwoSeparateBadgesSideBySide_EachWithText_T542_KK1` |
| 2 | Rozet adları kaynakla birebir; hesap ve davet durum makinesi yok | ✓ | `…AreTheNamesOf_04_2_5_1_*` (adlar `04` belgesinden okunur, altı aile); `TheBadgeFamilies_HaveNoMemberForTheAccountOrTheInvitation_KK2` |
| 3 | Kalem kayıtları satırda, kaydın adıyla | ✓ | `TheItemRecordNames_AreTheOnesOf_04_2_5_2_…_KK3`, `AnItemRecord_HasNoBadge_…_KK3`; entegrasyon `AnItemRecord_ShowsUnderItsOwnName_OnTheItemsRow_WithoutABadge_T542_KK3` |
| 4 | Panel işaretleri koşul sürdükçe görünür, kalkınca kalkar; metinli | ✓ | `ThePanelMarkTexts_…_KK4`, `AMark_ShowsWhileItsConditionHolds_AndGoesWhenTheConditionGoes_KK4`, `TheOrderList_HasNoIban…_KK4`, `TwoMarksThatHoldTogether_…_KK4`; entegrasyon `ThePanelMarksOnThePage_…_T542_KK4` |
| 5 | Yeniden gönder; davet satırı işaretli, yeniden gönderilmez | ✓ | `TheUndeliveredEmailBlock_OffersResendAllOnlyWhenMoreThanOneRowCanBeResent_KK5`; entegrasyon `TheUndeliveredEmailBlock_HasAResendFormWithTheTokenAndHandlerPerResendableRow_AndNoneOnTheInvitation_T542_KK5` (antiforgery jetonu, işleyici adı, davet satırında form ve düğme yok) |
| 6 | Altı sayaç; sıfır görünür; süzülmüş listeye gider; IBAN satır listesi | ✓ | `TheCounterNames_AreTheSixOf_02_10_6_1_…`, `TheCounters_AreSix_AZeroCounterIsDrawn…`, `ASeventhCounter_OrAMissingOne_IsRefused_KK6`, `TheIbanListIsNotACounter_…`, `TheIbanList_PutsTheLineThatRunsOutSoonestFirst_…`; entegrasyon `TheHomeCounters_AreSix_…`, `TheAwaitingCustomerList_IsLines_…` |
| 7 | Gün ve son gün tarihi; canlı geri sayım yok; iş günü; aşım metinle | ✓ | `ARunningPeriod_…_NoClockTime_KK7`, `APeriodCountedInBusinessDays_WritesIsGunu_…`, `OnTheLastDay_…OverrunInWords_KK7`, `TheLegalInterestMessage_…`; entegrasyon `ThePeriodIndicators_SayDaysAndTheLastDay_…_T542_KK7` (saat/dakika/saniye yok, betik yok) |
| 8 | Eşik mesajı sepette ve özette yerinde kalıcı | ✓ | `TheFreeShippingLine_IsTheSentenceOfTheShippingAssessment_KK8`, `TheMinimumOrderLine_…_KK8`; entegrasyon `TheThresholdMessage_IsALastingLineInItsPlace_…` (3 satır) ve `…IsNotDrawnWhenTheBasketHasNoCondition_…` |
| 9 | Kayıt varyantı güncel hâli aç ve "Yine de kaydet"; bölümlü ekranda yalnız kaydedilen bölümde; sipariş varyantı üzerine yazma sunmaz | ✓ | `ASectionedScreen_ShowsTheConflictOnTheSavedSectionOnly_KK9`, `ASectionKeepsItsOwnSaveLabel_…`, `TheOrderVariant_OffersNoWayToOverwrite_KK9`; entegrasyon `TheConflictNotice_ShowsOnlyOnTheSectionThatWasSaved_…`, `WithoutAConflict_EverySectionKeepsItsOwnSaveButton_AndTheOrderVariantNeverOffersTheOverwrite_…` |

### Öz denetim (bitiş kapısı 2)

Önceki işçinin yaptıkları dahil kabul kriterlerinin ve `12` kontrol listesinin tamamı diff'te yeniden denetlendi; `sik-fail-nedenleri.md` okundu.

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 iki rozet | ✓ | yukarıda 1 |
| KK2 adlar kaynakla aynı | ✓ | yukarıda 2 |
| KK3 kalem kaydı satırda | ✓ | yukarıda 3 |
| KK4 işaret düşer | ✓ | yukarıda 4 |
| KK5 yeniden gönderme; davet istisnası | ✓ | yukarıda 5 |
| KK6 altı sayaç; sıfır; IBAN listesi | ✓ | yukarıda 6 |
| KK7 gün ve tarih; iş günü birimi; aşım metni | ✓ | yukarıda 7 |
| KK8 eşik mesajı | ✓ | yukarıda 8 |
| KK9 iki varyant; bölüm uyarısı | ✓ | yukarıda 9 |
| `09 §25`: `Html.Raw` yok; satır içi betik ve stil yok; forma `asp-antiforgery`; yeni bağımlılık yok | ✓ | yeni `_*.cshtml` dosyalarında `Html.Raw`, `<script`, `style=`, `on…=` yok (grep); resend formları `asp-antiforgery="true"` (entegrasyon testi jetonu sayar); csproj/props diff'i boş |
| Sık FAIL 1: bekçi/tarama istisnası geniş | ✓ | yeni istisna eklenmedi; `OperationSourceScan` yeşile görünümü düzelterek döndü (`MomentText`), tarama gevşetilmedi |
| Sık FAIL 2: tarama çağrı biçimi | ✓ | yeni tarama yazılmadı; var olan taramalar tam Unit'te yeşil |
| Sık FAIL 3: 06 "Varsayılan" / FK dizini | ✓ (uygulanmaz) | şema ve göç değişikliği yok |
| Sık FAIL 4: aynı sayıyı anan öteki yerler | ✓ | sayaç adları, rozet adları ve işaret metinleri tek tanımdan; testler `04` ve `02` belgesinden okur (elle ikinci kopya yok); `SessionAccessTests` kapalı listesi gerekmedi (`/panel/siparisler/bilesen-ornegi` panel kapsamında, tam Integration yeşil) |
| Sık FAIL 5: belge kuralını değiştiren yeni 09 metni | ✓ | `09`'a dokunulmadı; yeni metinler D-1151–D-1153'te `doc-drift` |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Derleme | ✓ | `dotnet build tests/Shopfolio.UnitTests -warnaserror`, `tests/Shopfolio.IntegrationTests -warnaserror` | 0 uyarı, 0 hata |
| Unit (tam, Mimari dahil) | ✓ | `dotnet test --project tests/Shopfolio.UnitTests` | 6809 geçti, 0 kaldı (yeni iki sınıf 32) |
| Integration (tam) | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests`, PostgreSQL 18 `track_commit_timestamp=on` | 3454 koşuldu; ilk koşuda 1 kararsız kırmızı (`CatalogCoreTablesTests.Migration_WhenPgTrgmIsNotInstalled_…`, yük altında), sınıf tek başına 60/60 yeşil; `StorefrontFrameTests` 36/36 (yeni 13) |
| Format | ✓ | `dotnet format <proje> --no-restore --verify-no-changes` | Web, UnitTests, IntegrationTests temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | *(doğrulayıcı doldurur)* |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T542-rozet-sayac-eszamanli-uyari`
- **PR:** *(PR açılınca doldurulur)*
- **CI:** *(yönetici izler)*
- **Post-merge run'lar:** *(validator doldurur)*

## Dış Varsayımlar

Dış varsayım: yok. Task yalnız uygulama içi ortak bileşen, stil ve örnek sayfa kurar; plan katmanı, kota, dış servis, paket sürümü ya da sır gerekmez.

## Devredilen backlog kalemleri

Hedefi T542 olan açık D kaydı yoktu (`DEFERRED_BACKLOG.md`'de T542 geçmiyor).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Bileşenleri çağıran ekranlar (sipariş ayrıntısı, panel ana sayfası, sepet, ödeme özeti) | D-1150 | Ekran task'ları T52x'te; T542 ortak bileşen ve örnek sayfa |
| 2 | Süre aşımı ve kanuni faiz metni ⚠ | D-1151 | `04` metni yazmıyor |
| 3 | Asgari sipariş mesajı metni ⚠ | D-1152 | `04` metni sabitlemiyor |
| 4 | Sayaçlar orta ve geniş sınıfta üçerli ⚠ | D-1153 | `04` yalnız dar sınıfı söylüyor |
| 5 | Tarayıcı gözlemi; `resend-once` ile T541 submit-once birleştirmesi | D-1154 | Gözlem doğrulayıcıda; birleştirme T541'de |

## Notlar

- **⚠ kolay kararlar:** asgari sipariş mesajı `ShippingAssessment` cümlesi; "Kanuni faiz yükümlülüğü doğar." ve "Süre aşıldı: N gün geçti · son gün …" yeni ürün metni; sayaçlar medium+ üçerli; yeniden gönder formunda ayrı `data-js="resend-once"` betiği; çağıran ekranlar T52x'te.
- `ItemRecordEntry.MomentText`: görünüm tarih türünü adıyla anmasın diye (`OperationSourceScan` kuralı); biçimleme `DisplayFormat.Date`.
- Önceki işçi bağlam sınırında bölündü; bu tur derleme, Unit, Integration, örnek sayfa ve testler, kayıtlar.
- **Repo memory:** dokunulmadı.
