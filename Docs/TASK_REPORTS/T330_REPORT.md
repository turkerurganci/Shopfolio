# T330 — Firma kimliği: tek firma, tip ve tipe göre zorunlu alanlar, yasal kimlik bilgilerinin erişilebilirliği, iade adresi

**Faz:** F3 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 ✓ PASS) | **Tarih:** 2026-11-07

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- `ISettingsModule` (Settings modülünün açık yüzü) genişledi: `GetCompanyIdentityAsync`, `GetCompanyIdentityCompletenessAsync` (T331'in birinci kapı koşulu; eksik alan adları `CompanyIdentityInput` üye adlarıdır), `GetLegalIdentityAsync` (oturumsuz; tipe göre alan), `SaveCompanyIdentityAsync(input, formVersion)`, `GetReturnAddressAsync`, `SaveReturnAddressAsync(input, formVersion)`. Yöntemler çağıranın işlemine katılır, kendi işlemini açmaz; ret bir sonuçtur (`SettingsSaveResult`: kaydedildi / sürüm çakışması / alan retleri).
- `CompanyIdentityRules` (saf kural): tipe göre zorunlu alanlar (Esnaf: unvan + vergi no; Gerçek kişi tacir ve Tüzel kişi: unvan + MERSİS + ticaret sicil no; üçünde ortak: il, ilçe, adres, telefon, e-posta, KEP, meslek odası, davranış kuralları), eksik liste, kayıt denetimi (boş zorunlu alan kaydı engellemez; dolu zorunlu alan ve seçili tip boşaltılamaz; tip değişimi reddedilmez; e-posta/KEP/telefon/bağlantı biçimi; il kapalı listede).
- `CompanyIdentities` (okuma ve kayıt: `RecordSave` kalıbı, satır kilidi önce) ve `CompanySettingsWriter` (ayar satırının tek yazıcısı: `FOR UPDATE company_settings` kilidi; iade adresi bölümü `ReturnAddressVersion` koşullu tek `ExecuteUpdate`; dört parça birlikte, dolduktan sonra boşaltılamaz, telefon tek başına kabul edilmez).
- `LockKind.CompanySettings` eklendi (`LockSequence.IsSeparate`: sipariş, varyant, kupon, sepet, kategori ağacı ya da yönetici kümesi kilidiyle birleşmez).
- Mimari taramalar: `ConcurrencySourceScan` düz `FOR UPDATE` metnine `CompanySettingsWriter.cs` yolunu ekledi; yeni kural: ayar satırının toplu yazımı (`ExecuteUpdate`/`ExecuteDelete`/`SaveChanges` ya da ham `UPDATE`/`DELETE`) yalnız o dosyadadır (09 §10.3 madde 4); ayrı ihlal örneği `ViolationSamples/Concurrency/CompanySettingsWrites.cs`.
- `09` §9.1 madde 5'in kilit sırası cümlesi ve §10.3 madde 6'nın tarama yolu cümlesi koda hizalandı (kural değişmedi; karar kaydı gerekmez).
- Backlog D-165 (iz ve yasal metin sürümü bağları sonraya).
- Önceki task'ların merge kaydı doldu: T378 ve T314 raporlarının "Post-merge run'lar" satırı, status Commit hücreleri.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Settings/`: `CompanyIdentities.cs`, `CompanyIdentityRules.cs`, `CompanySettingsWriter.cs`, `SettingsModule.cs`, `Contracts/ISettingsModule.cs`, `Contracts/CompanyIdentityRecords.cs`, `Contracts/SettingsSaveResult.cs`
- `src/Shopfolio.Web/Infrastructure/`: `LockKind.cs`, `LockSequence.cs`
- `tests/Shopfolio.UnitTests/`: `InstallationSettings/CompanyIdentityRulesTests.cs`, `Architecture/ConcurrencySourceScan.cs`, `Architecture/ConcurrencySourceScanTests.cs`, `Architecture/ViolationSamples/Concurrency/CompanySettingsWrites.cs`, `Concurrency/LockSequenceTests.cs`
- `tests/Shopfolio.IntegrationTests/Model/CompanyIdentityTests.cs`
- `Docs/09_CODING_GUIDELINES.md` (§9.1 madde 5, §10.3 madde 6 test cümlesi), `Docs/DEFERRED_BACKLOG.md` (D-165), `Docs/IMPLEMENTATION_STATUS.md`, `.claude/memory/MEMORY.md`, T378 ve T314 raporları

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Tek firma; ekleme, seçme, geçiş, silme yok; pazar/dil/para birimi seçilemez | ✓ | `SettingsModule_HasNoMemberThatAddsSelectsSwitchesOrDeletesACompany_OrChoosesALanguageOrCurrency`; `IdentityInput_HasNoSeparateNotificationAddress_NoLanguage_NoMarket_NoCurrency_NoCompanyChoice`; Integration `SecondCompanyRow_AndDeletingTheCompany_AreRefused` |
| 2 | Kimlik panelden, altyapı kurulumdan; alanlar ayrı | ✓ | `CompanyIdentityInput` üye listesi testle kilitli (kurulum alanı yok) |
| 3 | Üç kapalı tip; zorunlu alanlar tipe göre | ✓ | `CompanyType_HasExactlyTheThreeValues_AndNoOtherIsDefined`; `MissingFields_*` (üç tip); `Check_ATypeOutsideTheClosedList_IsRefused` |
| 4 | Ortak zorunlu alanlar; boş kayıt engellemez; dolu boşaltılamaz; tip değişimi reddedilmez; ayrı bildirim adresi yok | ✓ | `Check_AnEmptyRequiredFieldOfAnEmptyRow_IsNotARefusal`; `Check_EveryFilledRequiredFieldOfTheType_CannotBeEmptied`; `Check_AChosenTypeCannotBeCleared_ButChangingItIsNeverRefused`; Integration `Save_WithAnEmptyKepAddress_IsAccepted_AndTheGateWaitsForIt`, `Save_EmptyingAFilledKepAddress_IsRefused_AndNothingIsWritten`, `Save_ChangingTheType_IsNeverRefused_AndAChosenTypeCannotBeCleared` |
| 5 | Meslek odası ve davranış kuralları tipten bağımsız zorunlu; işletme adı isteğe bağlı, doluysa görünür | ✓ | `CommonFields` (üç tip); `MissingFields_OfACompleteRowOfAType_AreEmpty_WithoutTheOptionalFields`; Integration `LegalIdentity_OfAMerchant_ShowsTheBusinessNameWhenFilled_AndNotTheTaxNumber` |
| 6 | Yasal kimlik ana sayfadan İletişim başlığı altında sürekli erişilebilir | ~ | Modül düzeyi: oturumsuz `GetLegalIdentityAsync` (Integration `LegalIdentity_NeedsNoSession_ShowsTheFieldsOfTheType_AndTheEtbisLinkOnlyWhenFilled`). Sayfa/altbilgi bağı T537 (altbilgi, `02 §3.1.4`), T506 (iletişim sayfası), T361 (ana sayfa) — ⚠ aşağıda |
| 7 | ETBİS kaydı yapılmaz; bilgi isteğe bağlı, kapıyı tutmaz, doluysa bant | ~ | Modül düzeyi: alan isteğe bağlı ve eksik listesine girmez; dolu bağlantı `LegalIdentity.EtbisVerificationUrl` ile döner, boşken `null`. Bant ekranı T537 — ⚠ |
| 8 | İade adresi zorunlu ayrı ayar; beyan kaydına kopyalanır, sonra değişse beyanın adresi sabit | ✓ | Integration `ReturnAddress_*`; `Withdrawal_KeepsTheReturnAddressItCopied_WhenTheSettingChangesLater` |

### Öz denetim (bitiş kapısı 2)

Önceki işçinin yazdıkları dahil 11'deki k1–k8, KK1–KK8, `09 §25` ilgili satırları ve `sik-fail-nedenleri.md` bütün maddeleri diff'te denetlendi.

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya, test adı) |
|---|---|---|
| k1 tek firma, sabit pazar/dil/para birimi | ✓ | `CompanyIdentityRulesTests` yansıma testleri; `ProtectedRecordsGuard` + Integration silme/ikinci satır |
| k2 alanlar ayrı | ✓ | `IdentityInput_HasNoSeparateNotificationAddress_…` |
| k3 üç tip | ✓ | `CompanyType_HasExactlyTheThreeValues_…`, `OwnFieldsByType` teorileri |
| k4 boş kayıt engellemez / dolu boşaltılamaz / tip değişimi / bildirim adresi | ✓ | yukarıdaki satır 4 testleri |
| k5 meslek odası + kurallar + işletme adı | ✓ | `MissingFields_*`, `Check_AWellFormedValue_IsAccepted` (kural: bağlantı ya da metin) |
| k6 yasal kimlik erişilebilir | ~ | modül yüzü ✓, sayfa T537/T506/T361 — ⚠ 1 |
| k7 ETBİS | ~ | modül yüzü ✓, bant T537 — ⚠ 1 |
| k8 iade adresi + beyan sabit | ✓ | `ReturnAddress_*`, `Withdrawal_KeepsTheReturnAddressItCopied_…` |
| KK1 ikinci firma / silme / dil-para birimi yok | ✓ | `SecondCompanyRow_AndDeletingTheCompany_AreRefused`; `SettingsModule_HasNoMember…` |
| KK2 kurulum alanları panel formunda yok | ✓ (giriş kaydı düzeyinde) | `IdentityInput_…` üye listesi; panel formu T535 |
| KK3 tip değerleri üç ve kapalı | ✓ | `CompanyType_HasExactlyTheThreeValues_…` |
| KK4 KEP boşken kabul, dolu boşaltılamaz, ayrı bildirim adresi yok | ✓ | `Save_WithAnEmptyKepAddress_…`, `Save_EmptyingAFilledKepAddress_…`, `Check_KepAddressEmptyWhileItWasEmpty_IsAccepted_…` |
| KK5 isteğe bağlılar boş geçer, kapıyı kapatmaz | ✓ | `Save_OfACompleteIdentityOfAnyType_LeavesNothingWaiting_WithoutTheOptionalFields` (3 tip) |
| KK6 oturumsuz ziyaretçi İletişim ile yasal kimliğe ulaşır | ~ | modül: `LegalIdentity_NeedsNoSession_…`; sayfa T537/T506 |
| KK7 ETBİS doluysa bant, kayıt yapılmaz | ~ | modül: bağlantı dolu/boş testi; bant T537 |
| KK8 adres değişince eski beyan sabit | ✓ | `Withdrawal_KeepsTheReturnAddressItCopied_WhenTheSettingChangesLater` |
| `09 §25` Eşzamanlılık (bölüm sürümü, koşullu güncelleme, kilit, `LockSequence` bildirimi) | ✓ | `ReturnAddress_SavesBySectionVersion_…`, `SettingsRowLock_IsTakenByBothSaves_…`, `SettingsRowLock_OutsideATransaction_IsRefused`, `LockSequenceTests` (CompanySettings çiftleri), `Model_TheSevenSectionVersionsOfTheSettingsRow_AreNotConcurrencyTokens_…` |
| `09 §25` Modül sınırı (yalnız `Contracts`, arayüz yönteminde `SaveChanges` yok) | ✓ | mimari testler yeşil (Unit 2985); `CompanySettingsWriter` yalnız `ExecuteUpdate` |
| `09 §25` İşlem izi (`AuditOperation` + iz aynı işlemde) | ✗ → D-165 (a) | iz tablosu/yazıcısı T380'indir; plan T330'a bağımlılık olarak yazmamış |
| Sık FAIL 1: tarama istisnası geniş | ✓ | izin tam yoldur; `Scan_AFileOfTheSameNameAsTheSettingsWriterElsewhere_IsNotAnAllowedPlace`; yalnız düz `FOR UPDATE` serbest |
| Sık FAIL 2: tarama bir çağrı biçimini kaçırıyor | ✓ | örnek: değişken üzerinden çağrı (satır 11), çok satırlı ham SQL, tırnaklı/şema önekli ad, `SaveChanges`; `Scan_OnTheSettingsViolationSample_…` |
| Sık FAIL 3: "Varsayılan" DB default'u değil / FK dizini | ✓ (yeni sütun/tablo yok) | şema değişmedi; göç yok |
| Sık FAIL 4: kural değişince aynı sayıyı anan yerler | ✓ | `ConcurrencySourceScan` özet yorumu, `09` §9.1 m.5, §10.3 m.6, `LockSequenceTests` özet yorumu, `LockKind` dizisini anan tek yer `LockSequence.IsSeparate` |
| Sık FAIL 5: belge kuralını değiştiren yeni 09 metni karar kaydısız | ✓ | 09'a eklenen iki cümle mevcut §10.3 madde 2/4 kurallarını koda hizalar; yeni kural yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests` | 2985 / 2985 geçti (yeni: `CompanyIdentityRulesTests`, tarama ve kilit sırası testleri) |
| Integration | ✓ | `--filter-class` `CompanyIdentityTests`, `SettingsTablesTests`, `ConcurrencyTests`, `ModelRuleTests` | 92 / 92 geçti (`CompanyIdentityTests` 24) |
| Contract | — | — | task'ın test beklentisi Unit + Integration |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-07, head `04eafd6`) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 0 |

### Doğrulama tur 1 — ✓ PASS

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` ana deponun `scripts/git-hooks`'u; main son üç tamamlanmış run 42351368236, 42351368096, 42346300012 `success`; hafızada T330 satırı var (satır 22).

**Kapsam okuması:** T330 F3'tedir (`11` faz tablosu: iş mantığı, sayfasız). Sayfa ve panel bağları sonraki task'ların kendi kabul kriterleridir: İletişim sayfasının kimlik bloğu T506 k3, altbilgideki tam kimlik ve ETBİS bandı T537 k6, menünün İletişim öğesi T361 k4, kimlik formu ve `SaveCompany` T535 k6–k7, iade adresi formu T536 k10. Kriter 6, 7 ve KK6, KK7 bu yüzden servis düzeyinde değerlendirildi ve karşılandı (öncel: T313 k8 "sayfa testi T549").

| Madde | Sonuç | Kanıt (doğrulayıcı) |
|---|---|---|
| Kriter 1 / KK1 | ✓ | `ISettingsModule` ve `CompanyIdentityInput`'ta firma ekleme/seçme/silme, dil, pazar, para birimi üyesi yok; `CompanyIdentities` yalnız `SingleAsync` ile okur, satır eklemez; Integration `SecondCompanyRow_AndDeletingTheCompany_AreRefused` (tekil dizin `23505`, bağlam koruması silmeyi `InvalidOperationException` ile reddeder, satır sayısı 1) |
| Kriter 2 / KK2 | ✓ | Giriş kaydı yalnız `06 §4.1`'in 15 alanıdır; alan adı, gönderen, sağlayıcı anahtarı, Google kimliği yok (`02 §3.1.2`); logo/marka ayrı (§4.54) |
| Kriter 3 / KK3 | ✓ | `CompanyType` üç değer (`06` §5: `Tradesman`, `NaturalPersonMerchant`, `LegalEntity`); `Check` tanımsız değeri `InvalidListValue` ile reddeder; `TypeFields` esnafta unvan + VKN, iki tacir tipinde unvan + MERSİS + sicil (`02 §3.1.3`) |
| Kriter 4 / KK4 | ✓ | `CommonFields` sekiz alan (il, ilçe, adres, telefon, e-posta, KEP, oda, kurallar); boş zorunlu alan ret değildir, `MissingFields`e girer; dolu zorunlu alan boşalınca `FilledFieldCleared`; tip değişimi reddedilmez, seçili tip temizlenemez; `Save_WithAnEmptyKepAddress_IsAccepted_AndTheGateWaitsForIt`, `Save_EmptyingAFilledKepAddress_IsRefused_AndNothingIsWritten`, `Save_ChangingTheType_IsNeverRefused_…`. Ayrı bildirim adresi alanı yok; F-1…F-4'ün yönlendirmesi T308 k1'in, tek `Email` alanı onun kaynağı |
| Kriter 5 | ✓ | Oda ve kurallar `CommonFields`'te (tipten bağımsız); `LegalIdentity` ikisini ve doluysa `BusinessName`'i taşır; `LegalIdentity_OfAMerchant_ShowsTheBusinessNameWhenFilled_…`, boşken `null` |
| Kriter 6 / KK6 | ✓ (servis düzeyi) | `GetLegalIdentityAsync` oturum istemez, tipin alanlarını verir (başka tipin dolu değeri gösterilmez), ili adıyla verir; `LegalIdentity_NeedsNoSession_…` |
| Kriter 7 / KK7 | ✓ (servis düzeyi) | `EtbisVerificationUrl` isteğe bağlı, `MissingFields`e girmez; dolu bağlantı döner, boşken `null`; ürün ETBİS'e kayıt yapan kod taşımaz (dış çağrı yok) |
| Kriter 8 / KK8 | ✓ | `CompanySettingsWriter.SaveReturnAddressAsync`: kilit → denetim → `ReturnAddressVersion` koşullu tek `ExecuteUpdate`; dört parça birlikte, dolduktan sonra boşaltılamaz; `Withdrawal_KeepsTheReturnAddressItCopied_WhenTheSettingChangesLater` beyan satırının adresi ayar değişince aynı kalır |
| `09 §25` | ✓ | Eşzamanlılık `09 §10.3` m.1–5 ile uyumlu (izlemesiz okuma, tek yazıcı, `FOR UPDATE` kilidi, bölüm sürümü, sürümler belirteç değil — model testi); `LockKind.CompanySettings` `05 §7.3.5`'in "sipariş tarafının dışındaki kilit" cümlesiyle aynı (ayrı aile); kaynak taraması ihlal örneğiyle kırmızı; modül sınırı yalnız `Contracts`; göç, paket, ayar yok. İşlem izi maddesi (`AuditOperation`) karşılanmadı: iz yazıcısı T380'indir, D-165 (a) kayıtlı |
| Güvenlik | ✓ | Sır yok; yeni uç ve giriş yüzeyi yok; yasal kimlik bilerek açıktır (`02 §3.1.4`), başka tipin değeri sızmaz; e-posta, KEP, telefon, bağlantı, il sunucuda denetlenir; yeni bağımlılık yok |

**Testler (doğrulayıcı, yerel, head `04eafd6`):** `dotnet build Shopfolio.slnx -warnaserror` 0 uyarı 0 hata; `dotnet format --verify-no-changes` üç projede ayrı ayrı temiz; Unit Architecture 769, Consistency 207, kalan 2034 — hepsi geçti; betik testleri 4 dosya geçti; Integration (PostgreSQL 18, yerel konteyner) parçalı: `MigrationBundleTests` 2, Model 917 (`CompanyIdentityTests` dahil), Concurrency 26, Database 36, Security 32, Web üç parça 592, Jobs 77, Pages/Disposal/FileSecrets/Files 75, Logging/Outages/SignedEndpoints 49 — toplam 1806, hepsi geçti. Koşu sonrası `git status` temiz, `dotnet restore --locked-mode` 0. Dal CI 42348609633 (head `04eafd6`) `success`.

**Task satırları (`12 §7.5`):** `11 §7.1` birimleri KP-35 (T330, T356, T366, T537), KP-58 (T330, T535), KD-21 (satır yok). KP-35 satırları `VAL-A125`…`VAL-A130` ve KP-58 satırları `VAL-A284`, `A285`, `A992`, `A286`, `A287`, `VAL-D031` kanıtını tarayıcı gözlemi ve uç yanıtıyla ister; T330 sayfa ve uç kurmaz → hepsi `BEKLEMEDE` kalır (öncel T313, T314). Servis düzeyindeki karşılıkları yukarıdaki tabloda. Matris betiği (`--kapsam 1-77`) hata 0.

**Yapım raporu karşılaştırması:** bir uyuşmazlık — yapım raporu kriter 6, 7 ve KK6, KK7'yi `~` yazdı; doğrulayıcı bunları F3 kapsamında `✓` (servis düzeyi) sayar, çünkü sayfa kısımları T506, T537, T361'in kendi kriterleridir ve plandan düşmez. Öz denetim tablosu her kriter ve KK için kanıtlı; `09 §25` iz maddesinin `✗ → D-165` kaydı doğrulayıcının okumasıyla aynı. Raporda `12` task satırları bölümü yoktu (burada yazıldı).

**Merge notu:** PR #420 `main` (T304, #421) ile `MEMORY.md` ve `DEFERRED_BACKLOG.md`'de çakışıyordu; doğrulamada `origin/main` dala birleştirildi (iki satır da kaldı; backlog aktif 97 — sayıldı). Birleşik dalda build temiz, Unit 3050 geçti; kod dosyasında çakışma yoktu. Birleşik başın CI'ı yardımcının merge kapısıdır.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok (tablolar T103'te)
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T330-firma-kimligi`
- **Commit:** bkz. PR (son commit dal başı)
- **PR:** #420
- **CI:** dal CI 42348609633 (head `04eafd6`) ✓ yeşil
- **Post-merge run'lar:** squash `4924037` (#420) — CI 42358802050 success, Publish 42358802017 success (T308 yapımında dolduruldu)

## Dış Varsayımlar

Dış varsayım: yok (saf kod; paket, plan, servis, API ya da ortam varsayımı yok).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | İşlem izi (`UpdateCompany`, `UpdateSettings`) ve kimlik/iade adresi değişince yeni yasal metin sürümü | D-165 (a)(b) | iz T380'in, üretilen yasal metin T366'nın; ikisi T330'dan sonra |

## Notlar

- **Working tree (Adım -1):** temiz.
- **Ana dal CI startup (Adım 0):** yapım işçisi CI yoklamaz; `origin/main` (`335e9cb`) dala alındı.
- **Repo memory (bitiş kapısı 9):** güncellendi.
- **⚠ Kolay kararlar (kurala en uygun seçenekle verildi, sahibe soru yok):**
  1. Kapsam servis düzeyi: panel formu (T535), İletişim sayfası (T506), ana sayfa (T361), altbilgi ve ETBİS bandı (T537) bu task'ta yok; modül yüzü bunların okuyacağı modelleri verir. k6/k7 modül düzeyinde karşılanır (~).
  2. `ProfessionalConductRules`: "http(s)://" ile başlıyorsa tam bağlantı denetimi, değilse serbest kısa metin (`04` 7.2.19.6 limit "—" olduğundan uzunluk tavanı yok).
  3. Vergi, MERSİS ve ticaret sicil numaralarının biçimi denetlenmez (`02 §3.1.2`: doğruluğu denetlemez).
  4. Yeni enum eklenmedi (`06 §5` eşitlik denetimi); alan adları metin.
  5. Yeni `LockKind.CompanySettings` (ayrı aile).
  6. Telefon kural dışı yazım "olduğu gibi" kalır ve kural reddeder; geçerliyse on haneye sıkıştırılır (`06 §2.5.1`).
  7. `CompanyIdentityRules.Normalize` yerine `ToStoredForm`: `SearchEngineSourceScan` "Normalize" adını ad katlama sayıyor.

## Düzeltme 1

CI "1. Lint" (run 42346250564, head `2d53728`) `dotnet format --verify-no-changes` ile kırmızıydı: `CompanyIdentityRules.cs` (WHITESPACE), `ConcurrencySourceScan.cs` (WHITESPACE), `CompanySettingsWriter.cs` (IMPORTS).

- `origin/main` dala birleştirildi (merge). Çakışma yalnız kayıt belgelerinde: `IMPLEMENTATION_STATUS.md` (T313/T314 satırları main'inkiyle; "Son güncelleme" main'in girişi), `MEMORY.md` (iki satır da kaldı). Backlog "Aktif kalem" 94 → 95 olarak yeniden sayıldı (D-165 eklendi). Kod dosyasında çakışma yok.
- Biçim: `dotnet format` ile üç dosya düzeltildi (yalnız boşluk/using biçimi, davranış değişmedi); `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` temiz.
- Testler (birleşik dal): Unit Architecture+Concurrency+Consistency 1030, geri kalan 1980; hepsi geçti (toplam 3010, `CompanyIdentity*` 41 sınıf bunun içinde).
- Öz denetim: düzeltme yalnız biçim ve kayıt birleştirmesi; T330 kabul kriterleri tablosu yukarıdaki haliyle geçerli (6 ve 7 modül düzeyinde `~`, sayfa kısmı D-165 ve T535/T506/T537/T361). Çakışma işaretçisi kalmadı. Yeni backlog kaydı yok.
