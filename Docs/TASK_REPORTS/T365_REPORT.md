# T365 — İletişim talebi: form, konu tipi, durumlar, saklama, istenmeyen ileti koruması

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS) | **Tarih:** 2026-11-09

---

## Yapılan İşler

- `ContactFormSending` (yeni): sıra kapı, tuzak alan, alan kuralları, L-4 IP sınırı, `ContactRequests.OpenAsync` (Açık talep + F-2 firma bildirimi). Göndericiye bildirim, numara ya da durum sorgusu yok.
- `ICorporateContentModule`: `GetContactFormAsync` ve `SendContactRequestAsync`; kayıt `AddCorporateContentModule` içinde. Sözleşme tipleri: `ContactFormInput`, `ContactFormFields`, `ContactFormFieldError`, `ContactFormResult`, `ContactFormView`, `ContactFormOutcome`.
- Devam turu: Integration `CorporateContent` alanındaki 35 kırmızı teşhis edildi. Neden: modül çözülürken `ContactRequests` `INotificationsModule` ister; modülü elle kuran beş düzenek bunu kaydetmiyordu (`AttemptCounter` ya da paylaşılan veritabanı değil). Düzeltme: test düzeneklerine `UnusedNotifications` kaydı (`tests/Shopfolio.IntegrationTests/CorporateContent/UnusedNotifications.cs`).
- Ekran ve POST yüzü T506'nın (D-811); üye adı/e-postası çağıran sayfadan gelir (CorporateContent Membership'e bağımlı değil).

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/CorporateContent/` (ContactFormSending, CorporateContentModule, Contracts/ContactFormRecords, Contracts/ICorporateContentModule, Contracts/Results/ContactFormOutcome)
- Testler: `UnitTests/CorporateContent/ContactFormTests.cs`, `IntegrationTests/CorporateContent/ContactFormSendingTests.cs`, `UnusedNotifications.cs`; beş düzenek (AnnouncementAndPreview, ContentRelations, EmptyInstallation, StorefrontReaders, Model/BrandingTests); `SitemapSourceTests.FakeContent`.
- Kayıt: `IMPLEMENTATION_STATUS.md` T365 satırı, `STATUS_CHANGELOG.d/T365.md`, `DEFERRED_BACKLOG.d/T365.md`, `DEFERRED_BACKLOG.md` D-199 (yerinde).

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Girişsiz form; üyede ön dolu; her gönderim bir talep | ✓ | `ASendWithoutAnySignIn_WritesOneRequest_BornOpen_LinkedToNoAccountOrOrder_KK1`, `TheForm_StartsWithTheMembersNameAndEmail_...KK1`, `ASecondSend_IsASecondRequest_NotCompared_KK1` |
| 2 | Dört zorunlu alan, telefon isteğe bağlı; ek ve sipariş numarası yok | ✓ | `EveryEmptyRequiredField_IsRefusedTogether_..._KK2`, `AnEmailAndAPhoneOfTheWrongForm_...KK2`, `TheRequestTable_HasNoFileColumnAndNoOrderNumberColumn_..._KK2`, Unit `TheForm_CarriesTheFiveFieldsAndTheTrap_..._KK2` |
| 3 | Kapalı beş konu; ayıp talebi yok | ✓ | `EachOfTheFiveTopics_IsAccepted_..._KK3`, `AValueOutsideTheList_IsRefusedAsNotInTheList_..._KK3`, Unit `TheTopics_AreTheClosedFive_..._KK3` |
| 4 | Açık doğar; kapatma ve yeniden açma; iki durum | ✓ | `ARequestFromTheForm_IsClosedAndReopenedByTheAdministrator_WithTheTwoStatuses_KK4` (+ `ContactRequestStatusTests`) |
| 5 | Saklama son kapatılıştan, kapanmamışta açılıştan; Sipariş hakkında on yıl | ✓ | `ARequestFromTheForm_CountsItsRetentionFromTheMomentItWasSent_UntilItIsClosed_KK5`, `RecordDisposalTests.AContactRequest_GoesAtTwoYearsFromItsLastClosing_AndAnAboutOrderRequestAtTenYears` |
| 6 | Kayıt + firmaya bildirim; yanıt ekranı/dizisi yok | ✓ | `ASend_QueuesTheNoticeF2ForTheFirmAndNothingForTheSender_KK6_KK7`, `NoTableForRepliesThreadsOrFollowingARequestExists_KK6_KK7` |
| 7 | Müşteriye numara, taleplerim, durum sorgusu, alındı e-postası yok | ✓ | aynı iki test; Unit `TheContactSurface_HasNoReplyThreadTrackingComplaintOrChatMember_KK6_KK7_KK10` |
| 8 | Tuzak sessiz düşer; IP limiti; captcha yok | ✓ | `ASendWithTheTrapFilled_ShowsTheSentState_..._KK8`, `TheTrapIsDroppedBeforeTheFieldsAreRead_...KK8`, `TheSendsPastTheLimit_AreRefusedWithTheNeutralCode_..._KK8`, `TheLimitCountsTheAddressAlone_..._KK8`, `ARefusedSend_DoesNotMoveTheCounter_..._KK8` |
| 9 | Aydınlatma ya da çerez politikası yayında değilse form kapalı; açıkta bağlantı görünür | ✓ | `AClosedGate_ClosesTheForm_BeforeAnyFieldIsReadOrAnyAttemptCounted_KK9`, `EachOfTheTwoConditions_ClosesTheForm_AndTheOpenFormShowsThePrivacyLink_KK9`, `ATrapFilledSend_OnAClosedGate_IsRefusedAsClosed_NotShownAsSent_KK9` |
| 10 | Şikâyet ve canlı destek yok; indirme yenileme "Sipariş hakkında" | ✓ | `TheTopicsHaveNoComplaintOrChatTopic_AndTheOrderTopicCarriesTheDownloadRenewal_KK10` |

### Öz denetim (bitiş kapısı 2)

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kabul kriterleri 1–10 | ✓ | yukarıdaki tablo |
| KK1…KK10 (12 §T365 kontrol listesi) | ✓ | test adlarının `_KKn` son eki; her KK en az bir testte |
| Sık FAIL 1: bekçi/tarama istisnası geniş | ✓ | yeni bekçi ya da tarama istisnası eklenmedi |
| Sık FAIL 2: kaynak taraması çağrı biçimi | ✓ | yeni kaynak taraması yok; mevcut 09 §19.2 mimari testleri Unit 5509/5509 geçti |
| Sık FAIL 3: 06 "Varsayılan" / FK dizini | ✓ | şema ya da migration değişmedi (`ContactRequests` T322'de) |
| Sık FAIL 4: aynı sayıyı anan öteki yerler | ✓ | konu sayısı enum'dan türetilir (`TheFormView_ListsTheFiveTopicsOfTheEnum_KK3`); 06 §5 tutarlılık testi `Results` ad alanındaki `ContactFormOutcome` ile geçti |
| Sık FAIL 5: 09 metni karar kaydısız | ✓ | 09 değişmedi |
| Modül yöntemi kendi işlemini açmaz, SaveChanges çağırmaz | ✓ | `ACallOutsideATransaction_IsAProgrammingError_BeforeAnythingIsRead_KK1` |
| Yeni modül kurucu bağımlılığı olan düzenekler | ✓ | Integration tamamı koşuldu (3162/3162) ve birleşme sonrası CorporateContent + Model |
| Backlog'da T365'e devredilen kalem | ✓ | D-199 iletişim formu yarısı yerinde kapatıldı; D-780 kapanmaz (D-810) |
| `dotnet format` | ✓ | Web, UnitTests, IntegrationTests ayrı ayrı temiz |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --no-build` | birleşme sonrası 5509/5509 |
| Integration (tamamı) | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests --no-build` | birleşme öncesi 3162/3162 |
| Integration CorporateContent | ✓ | `--filter-namespace Shopfolio.IntegrationTests.CorporateContent` | birleşme sonrası 185/185 (düzeltme öncesi 35 kırmızı) |
| Integration Model | ✓ | `--filter-namespace Shopfolio.IntegrationTests.Model` | birleşme sonrası 1393/1393 |
| Contract | — | — | bu task'ta yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-09) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 1 (K1 → D-824) |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T365-iletisim-talebi`
- **PR:** yönetici bilgisine dönüş mesajında
- **CI:** yönetici izler
- **Post-merge run'lar:** validator doldurur

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Dış varsayım: yok (paket, plan, API varsayımı yok); Postgres 18 konteyneri `track_commit_timestamp=on` ile | `docker run postgres:18 -c track_commit_timestamp=on` | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | L-4 sırası `ContactFormSending` içinde elle | D-810 | ortak çok eksenli yardımcı D-780'de |
| 2 | Form ekranı, POST, 303, 429 sayfası, ön dolu değer okuması | D-811 | sayfa T506'nın |
| 3 | `HasContactForm` sabit `true` | D-812 | iletişim sayfası T506'da |

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** main son run'lar success (CI 42596386862, Publish 42596386708, T367)
- **Repo memory (bitiş kapısı 9):** parça dosyaları yazıldı (`STATUS_CHANGELOG.d/T365.md`, `DEFERRED_BACKLOG.d/T365.md`)
- **Ayrıca:** ⚠ (kolay, karar verildi) ekran/POST yüzü T506'ya bırakıldı; sonuç nesnesinde talep numarası yok (`02 §3.32.6`).

## Doğrulama Sonucu — T365 İletişim talebi: form, konu tipi, durumlar, saklama, istenmeyen ileti koruması — tur 1
**Tarih:** 2026-11-09   **Branch:** task/T365-iletisim-talebi   **Commit:** `69144a13` (doğrulanan head)

### Verdict: ✓ PASS

Ayrıntılı tablolar (başlangıç kapıları, kabul kriterleri, task satırları, kontrol listesi, testler, güvenlik, K-notları) aşağıdadır.

### Başlangıç Kapıları
| Kapı | Sonuç | Kanıt |
|---|---|---|
| Working tree | temiz | `git status --short` boş |
| Hook kurulumu | ✓ | `core.hooksPath` → ana deponun `scripts/git-hooks` |
| Ana dal CI (son 3) | ✓ | 42600847174 · 42600846924 (#495) · 42596386708 (#494) — hepsi success |

### Kabul Kriterleri
| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Girişsiz; üyede ön dolu, değiştirilebilir; her gönderim bir talep | ✓ | `ReadAsync` üye değerini verir, ziyaretçide boş; `ASendWithoutAnySignIn_…_KK1`, `TheForm_StartsWithTheMembersNameAndEmail_…_KK1`, `ASecondSend_…_KK1`; sayfa D-811 |
| 2 | Dört zorunlu alan, telefon isteğe bağlı; ek/sipariş no yok | ✓ | hatalar birlikte, sayılmaz; tablo sütunları ve FK 0 (`TheRequestTable_…_KK2`); alanlar `07 §5.2.1` UC-15 ile birebir |
| 3 | Kapalı beş konu; ayıp yok | ✓ | `ContactTopic` sırası `02 §3.32.3`; liste dışı değer `InvalidListValue` |
| 4 | Açık doğar; kapat/yeniden aç; iki durum | ✓ | `ARequestFromTheForm_IsClosedAndReopened…_KK4` |
| 5 | Saklama başlangıcı; "Sipariş hakkında" 10 yıl, öteki P-40 (2 yıl) | ✓ | `…CountsItsRetention…_KK5`; `RecordDisposalTests.AContactRequest_GoesAtTwoYears…TenYears` yerelde yeşil |
| 6 | Bildirim gider; yanıt yüzü yok | ✓ | F-2 firma adresine; tablo taraması yalnız `contact_requests` |
| 7 | Müşteriye numara/takip/alındı yok | ✓ | sonuçta numara yok; müşteri bildirimi çağrılmaz; `src`'de takip yüzü yok |
| 8 | Tuzak sessiz; L-4; captcha yok | ✓ | tuzak sayılmaz (`07 §4.10.2`); 5/60 dk, altıncı `ContactRequestLimitExceeded`, satırlar kalır, başka IP serbest, pencere biter |
| 9 | Aydınlatma/çerez yayında değilse kapalı; bağlantı | ✓ | kapı yalnız `PrivacyNotice`, `CookiePolicy`; kapalıda alan okunmaz, sayılmaz; `LegalLinkScreen.ContactForm` |
| 10 | Şikâyet/canlı destek yok; yenileme "Sipariş hakkında" | ✓ | `src` taraması temiz; `…_KK10` |

### Task Satırları (`12 §7.5`, adım 2a ve 16)
| Satır (`VAL-…`) | Severity | Kanıt ortamı | Önceki `Durum` | Yeni `Durum` | Backlog kaydı (KABUL EDİLMİŞ RİSK ise) |
|---|---|---|---|---|---|
| VAL-A105 (KP-30) | KRİTİK | yok | BEKLEMEDE | BEKLEMEDE — veri kaydı kanıtlı; "panelde görünür" T532 | — |
| VAL-B022 (KP-34) | ORTA | yok | BEKLEMEDE | BEKLEMEDE — uç T506, panel T532 | — |
| VAL-A123 (KP-34) | KRİTİK | yok | BEKLEMEDE | BEKLEMEDE — kapı modülde kanıtlı; uç yanıtı T506 | — |
| VAL-A124 (KP-34) | ORTA | yok | BEKLEMEDE | BEKLEMEDE — panel gözlemi T532 | — |
| VAL-A256, VAL-C063, VAL-C064 (KP-49) | ORTA | yok | BEKLEMEDE | BEKLEMEDE — panel T532 | — |

`VAL-A106` T366'nın, `VAL-C061`/`C062` T374'ün. Emsal T341. `12_matris_denetle.py --kapsam 1-77` → Hata: 0.

### Doğrulama Kontrol Listesi
- [x] KK1 … KK10 (kanıtlar kabul kriterleri tablosunda)
- [x] `09 §25`: modül sınırı, işleme katılır ve açmaz, `SaveChanges` yok, `TimeProvider`, yeni paket/göç/ayar yok, tek sayaç `AttemptCounter`; ad sapması K1
- [x] Güvenlik taraması

### Test Sonuçları
| Tür | Sonuç | Komut | Çıktı |
|---|---|---|---|
| Build | ✓ | `dotnet build Shopfolio.slnx --no-restore -warnaserror` | 0 uyarı |
| Format | ✓ | `dotnet format <proje> --verify-no-changes` ×3 | temiz |
| Unit | ✓ | `dotnet test tests/Shopfolio.UnitTests --no-build` | 5509/5509 |
| Betik | ✓ | `bash tests/scripts/run-script-tests.sh` | 4/4 |
| Integration | ✓ | CorporateContent ad alanı; BrandingTests + RecordDisposalTests | 185/185; 42/42 |
| Dal CI | ✓ | `gh pr checks 496` | run 42600283047 tüm işler pass |

### Güvenlik Kontrolü
- Secret sızıntısı: Temiz
- Auth etkisi: Temiz (form tanım gereği açık; durum değişikliği yönetici ister, değişmedi)
- Input validation: Temiz (alanlar, konu listesi, tuzak, L-4 sunucuda; F-2 yalnız konuyu taşır)
- Yeni bağımlılık: Yok

### Bulgular (`12 §8.2` defect kaydı)
Bulgu yok.

### Bloke-etmeyen Notlar (K-notları)
| # | Not | Karar |
|---|---|---|
| K1 | Modül girdisi `ContactFormInput`; `07 §5.2.1` UC-15 modeli `SendContactRequestInput` (`09 §25` ad sorusu); alanlar birebir, sayfa modeli yok | DEFERRED_BACKLOG'a forward → D-824 (T506) |

### Yapım Raporu Karşılaştırması
- Uyum: Tam uyumlu; öz denetim tablosu kanıtlı. Yapım raporu K1'i anmıyor.

### Merge ve Post-merge
- Yardımcıda (adım 17–18).
