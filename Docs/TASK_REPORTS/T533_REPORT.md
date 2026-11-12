# T533 — Üye kaydı görünümü (E-40)

**Faz:** F5 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS) | **Tarih:** 2026-11-12

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- `GET /panel/uyeler`: aramasız yalnız arama alanı; müşteri hesaplarında e-postayla tam eşleşme (harf ayrımsız, yönetici aranmaz); üç sonuç hâli; biçimsiz e-posta alan mesajı (200, arama ve iz yok).
- Doğrulanmış hesapta giriş yolları, adres defteri ve hesaba bağlı siparişler (en yeni önce, sayfalı, satır bağlantısı UC-92'ye); geri alma bağlantısı açıkken "Hesabı sil" kapalı, sebep ve bitiş tarihi yerinde.
- `DeleteCustomer` (UC-157): onaysızda onay hâli 200 (metin `IrreversibleConfirmations.AccountDeletion()`'dan), onaylıda 303 → `/panel/uyeler` ve kalıcı mesaj; 422 (bağlantı açık), 404 (hesap yok, tekrar gönderim); sonuçsuz onayda 303 → aramanın adresi.
- `CustomerOrderReader` (`IOrderingModule.GetCustomerOrdersAsync`): sıralama ve sayfa `Order` varlığında, sonra `CustomerOrderRow`a eşleme.
- `_ListSearch` ortak parçası: `Message`/`FieldId` ve tag helper eklemesi.
- Bu turda: `DeleteCustomerInput` alanları `[SpecAssignedName]`; `admin.css` kesme noktası `64rem` yerine `1024px`.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Pages/Panel/Members.cshtml(.cs)`, `Pages/Shared/_ListSearch.cshtml`, `Pages/Lists/ListModels.cs`, `wwwroot/css/admin.css`
- `src/Shopfolio.Web/Modules/Ordering/*` (`CustomerOrderReader`, `OrderingModule`, `Contracts/IOrderingModule`, `Contracts/CustomerOrderRows`)
- Testler: `tests/Shopfolio.IntegrationTests/Web/MemberRecordPageTests.cs`, `tests/Shopfolio.UnitTests/Architecture/OutcomeUncertainConsistencyTests.cs`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Aramasız yalnız alan; tam eşleşme, harf ayrımsız, yönetici aranmaz | ✓ | `WithoutASearch_OnlyTheSearchFieldShows_KK1`, `TheSearch_FindsTheCustomerAccountByTheExactAddress_...KK1` |
| 2 | Üç sonuç hâli | ✓ | `TheResult_IsNoAccount_AnUnverifiedRegistration_OrAVerifiedAccount_KK2` |
| 3 | Giriş yolları, adres defteri, siparişler (sıra, sayfa, UC-92 bağlantısı) | ✓ | `TheAccount_ShowsTheLoginPaths_...KK2_KK3`, `TheBoundOrders_AreNewestFirst_...KK3` |
| 4 | Biçimsiz e-posta alan mesajı, 200, iz yok | ✓ | `AWrongEmail_IsAFieldMessage_With200_...KK4` |
| 5 | Bağlantı açıkken düğme yok; sebep ve bitiş tarihi | ✓ | `WhileARevertLinkIsOpen_TheDeleteButtonIsAbsent_...KK5` |
| 6 | Onay hâli 200; onaylı 303 + kalıcı mesaj, boş alan | ✓ | `WithoutTheConfirmationMark_...KK6`, `TheConfirmedDeletion_Answers303ToTheSearch_...KK6` |
| 7 | 422 (bağlantı açık, silinmez); 404 (hesap yok); yeniden doğrulama yok | ✓ | `ADeletionWhileARevertLinkIsOpen_Answers422_...KK7`, `ADeletionOfAnAccountThatIsNotThere_Answers404_...KK7` |
| 8 | Silinenler/kalanlar, imha kaydı, kilit, silme e-postası, iz hesabı adlandırmaz | ✓ | `TheDeletion_RemovesTheAccountAndWhatHangsOnIt_KeepsTheOrders_...KK8` |
| 9 | Tekrar gönderim 404; sonuçsuz onay 303 → arama | ✓ | `TheSecondSubmit_Answers404_AndSendsOneEmailOnly_KK9`, `AnUncertainOutcome_Answers303ToTheSearchOfTheEmail_...KK9` |

### Öz denetim (bitiş kapısı 2)

Önceki işçilerin diff'i (`git diff origin/main...HEAD`, 9 kaynak + test dosyası) dahil bütün maddeler yeniden denetlendi.

| Madde | Sonuç | Kanıt |
|---|---|---|
| Kriter 1–9 ve KK1–KK9 | ✓ | Yukarıdaki tablo; `MemberRecordPageTests` 14/14 |
| `09 §25` modül sınırı: sayfada `DbContext`/`SaveChanges` yok | ✓ | Sayfa yalnız `IMembershipModule`/`IOrderingModule` (Contracts) çağırır; mimari testler Unit tam koşuda yeşil |
| Onay metni tek ev (D-533, `09 §24` m.4) | ✓ | `Members.cshtml.cs` `IrreversibleConfirmations.AccountDeletion()`; sayfada metin yok |
| Bekçi/tarama istisnası geniş değil (sik-fail 1) | ✓ | Yeni istisna eklenmedi; yalnız `[SpecAssignedName]` işareti ve CSS 1024px'e çekildi |
| Kaynak taraması çağrı biçimi (sik-fail 2) | ✓ | Unit `FrameSourceScanTests`, `ModelBindingRuleTests`, `OutcomeUncertain*`, `PageHandlerRuleTests`, `ListSourceScan` tam koşuda yeşil |
| 06 "Varsayılan" / FK dizini (sik-fail 3) | ✓ (kapsam dışı) | Şema/migration değişikliği yok |
| Kural değişince aynı sayıyı anan yerler (sik-fail 4) | ✓ | UC-157 `NotMarkedYet` listesinden çıktı (`OutcomeUncertainConsistencyTests`); Unit tam yeşil, başka sayaç kırılmadı |
| Karar kaydı gerektiren yeni 09 metni (sik-fail 5) | ✓ (kapsam dışı) | 09 / belge kuralı değişmedi |
| Hedefi T533 olan backlog kalemleri (sik-fail: kapatılmadı/ileri atılmadı) | ✓ | D-533, D-605 ✅ kapandı; D-607, D-608, D-652, D-664 doküman düzeltmesi, karar kaydı ister → hedef F5 kapısına ileri atıldı (⚠1) |
| Kayıt: STATUS satırı + changelog parçası | ✓ | `Docs/IMPLEMENTATION_STATUS.md` T533 ⏳; `Docs/STATUS_CHANGELOG.d/T533.md` |

⚠1: Dört doküman düzeltmesi (07 yüzey metni, imha kaydı türleri, VAL-D024 hücresi, 05 §7.3.5 kilit sırası) belge kuralı değişikliği olduğundan karar kaydı ister; bu task'ta yapılmadı, F5 kapısına bırakıldı.

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests --max-threads 3` | Tam koşuda 7381 testten 2 kırmızı (CSS kesme noktası, alan adı işareti) düzeltildi; ilgili sınıflar 125/125, düzeltmeden sonra tam koşu kırmızıları dışında 7379 yeşil |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests --no-build --filter-namespace "*.X"` (ad alanı parçaları) | Yeşil: Web 1–2 turda sınıf parçalarıyla, Ordering 645, Administration 69, Catalog 82, Commands 70, Concurrency 30, CorporateContent 195, Database 32, Disposal 61, FileSecrets 24, Files 95, Jobs 85, Logging 30, Outages 53, Payment 59, Security 34, SignedEndpoints 11, Model 1410; Pages ad alanında test yok. Database (1) ve Model (3) ilk koşuda makine yükünden kırmızı, tek başına yeniden koşuda yeşil |
| `dotnet format` | ✓ | `--verify-no-changes` | Web, UnitTests ve IntegrationTests temiz; Unit TAM 7402 yeşil |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS (tur 1, 2026-11-12) — dal ucu `d1a1729a` (origin/main `29562ee9` ile birleşik, main ilerlemedi): build `-warnaserror` 0 uyarı, format temiz (3 proje), Unit 7402/7402, betik 4/4, Integration dokunulan sınıflar 59/59 (`MemberRecordPageTests` 14 + doğrulayıcının geçici VAL-A109/A110 kanıtı 2, `ListComponentPageTests` + `RequestsPageTests` + `MembershipAccountDeletionTests` 43), `--locked-mode` restore 0; dal CI'ı run 43183892942 yeşil. Kabul kriterleri 1–9 ve KK1–KK9 doğrulayıcı kanıtıyla ✓. `12`: VAL-A107, A108, A109, A110, D024, C031 → PASS |
| Bulgu sayısı | 0 (S1/S2/S3 yok) |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 2 — K1 → D-1373 (`04` 9.12.15 dar sınıf kartında toplam da görünür) · K2 → D-1374 (doğrulanmamış kaydın panelden silinmesi: `04` 6.3.12.2 ↔ `07 UC-157`, `02 §3.13.5`) |

Yapım raporu uyuşmazlıkları (bloke etmez, bu PR'da düzeltildi): okuyucu yöntemi `GetCustomerOrdersAsync` değil `IOrderingModule.ListCustomerOrdersAsync`; tutarlılık testi `tests/Shopfolio.UnitTests/Consistency/OutcomeUncertainConsistencyTests.cs`'tedir; PR #556.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T533-uye-kaydi`
- **PR:** #556
- **Post-merge run'lar:** *(validator doldurur)*; son ana dal koşuları: 43151185860, 43151185866 (`bc915800`, success)

## Dış Varsayımlar

Dış varsayım: yok.

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Yok | — | — |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | 07 yüzey metni, imha kaydı türleri, VAL-D024 hücresi, 05 §7.3.5 kilit sırası | D-607, D-608, D-652, D-664 | Belge kuralı değişikliği; karar kaydı ister, F5 kapısına ileri atıldı |
| 2 | Dar sınıf sipariş kartında toplam (doğrulama K1) | D-1373 | E-36 kart kalıbıyla birlikte karar (T527) |
| 3 | Doğrulanmamış kaydın panelden silinmesi belgeler arasında çelişik (doğrulama K2) | D-1374 | Doküman kararı; F5 kapısı |
