# T205 — Açılışta veritabanı fail-fast denetimi

**Faz:** F2 | **Durum:** ⏳ Devam ediyor | **Tarih:** 2026-11-05

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- `StartupDatabaseCheck` (ikinci hosted service; ayar denetiminden sonra kayıtlı): il listesi, ülke listesi ve altı tek satırlık kaydın (`Company`, `CompanySettings`, `Branding`, `ChannelStateRecord`, `HeartbeatRecord`, `BackupStatusRecord`) varlığını denetler; eksik varsa her eksik kaydı adıyla `Critical` satır (`StartupDatabaseChecked`) olarak yazar ve açılışı reddeder.
- `DatabaseAudit` / `DatabaseFailures`: saf işlev ve ileti metni; ileti yalnız kayıt adını taşır.
- `AppDbContext.ReadStartupRecordPresenceAsync`: yalnız `AnyAsync` (varlık); değer okunmaz. Okuma bağlamda durur, çünkü `Infrastructure/` modül türüne ulaşamaz (mimari test `Infrastructure_InApplicationAssembly_ReachesNoModuleTypeExceptInTheContext`). Tablosu hiç yapılmamış kayıt (`42P01`) yok sayılır; başka veritabanı hatası yukarı çıkar.
- Veritabanına bağlanılamıyorsa (ulaşılamayan sunucu, olmayan veritabanı, kimlik hatası) denetim çalışmaz, tek `Warning` satırı yazılır ve uygulama açılır — canlılık ucu 503 verir (`09 §10.7` madde 6; mevcut `LivenessEndpointTests` bu tasarımı sabitler). Bağlantı önce `NpgsqlDataSource` ile sınanır: bağlam kendi hata satırında sunucu adresini yazardı (`LivenessEndpointTests` "192.0.2.1 log'da yok" kuralı).
- `LogEvents.StartupDatabaseChecked` eklendi; `09 §18.1` madde 3 olay listesi ve `.claude/CONTEXT.md` klasör haritası güncellendi.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/StartupDatabase/` (`StartupRecordPresence.cs`, `DatabaseAudit.cs`, `StartupDatabaseCheck.cs`) — yeni
- `src/Shopfolio.Web/Infrastructure/AppDbContext.cs`, `Logging/LogEvents.cs`, `Program.cs`
- `tests/Shopfolio.IntegrationTests/Web/StartupDatabaseCheckTests.cs`, `tests/Shopfolio.UnitTests/InstallationSettings/DatabaseAuditTests.cs` — yeni
- `Docs/09_CODING_GUIDELINES.md` §18.1 madde 3, `.claude/CONTEXT.md`, `.claude/memory/MEMORY.md`, `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Açılışta il listesi dolu denetlenir | ✓ | `AppDbContext.cs` `ReadStartupRecordPresenceAsync` (`nameof(Province)`, liste); test `Start_WithAnEmptyList_RefusesAndNamesTheList(Province)` |
| 2 | Açılışta ülke listesi dolu denetlenir | ✓ | aynı yöntem (`nameof(Country)`); test `...(Country)` |
| 3 | Altı tek satırlık kaydın varlığı denetlenir | ✓ | aynı yöntem altı `nameof`; `Presence_OnTheStartData_ListsTheTwoListsAndTheSixRowsAllPresent`; `Start_WithASingleRowRecordMissing_...` altı satır (her kayıt için) |
| 4 | Eksikte açılmaz, eksik kayıt adıyla yazılır, `ILogger<T>` ile | ✓ | `StartupDatabaseCheck.StartAsync` (`LogFailure` → `ILogger<StartupDatabaseCheck>`, `InvalidOperationException`); `AssertRefusalLineNames` (`|Critical|`, `StartupDatabaseChecked`, ileti); dosyaya iniş eklenmedi (T230) |
| 5 | Denetlenen satırın varlığıdır, değeri değil | ✓ | yalnız `AnyAsync` (`ExistsAsync`); `Start_WithARecordWhoseValueIsEmptyButWhoseRowExists_Opens`; `Start_WithARecordMissingAndAnotherRecordHoldingAValue_NeverPrintsTheValue` |
| 6 | Hakkımızda/duyuru yokluğu geçerli; teslimat illeri ve taslaklar denetlenmez | ✓ | `Start_WithoutTheAboutPageAnnouncementDeliveryProvincesAndDrafts_Opens` |
| 7 | Ayar denetiminin kalıbını izler; ayar zorunluları dışarıda | ✓ | ikinci hosted service + `ILogger<T>` + saf işlev + `Critical` satır/atma (`StartupSettingsCheck` ile aynı); `Start_WithADatabaseFailureAndASettingMissing_RefusesOnTheSettingAndNeverAsksTheDatabase`; `AssertNoOtherRecordNamed` ayar anahtarı/değeri aramaz |

### Öz denetim (bitiş kapısı 2)

| Madde (kabul kriteri / KK) | Sonuç | Kanıt (dosya:satır, test adı) |
|---|---|---|
| Kriter 1 — il listesi | ✓ | `AppDbContext.cs:211`; `Start_WithAnEmptyList_RefusesAndNamesTheList(Province)` |
| Kriter 2 — ülke listesi | ✓ | `AppDbContext.cs:212`; `...(Country)` |
| Kriter 3 — altı kayıt | ✓ | `AppDbContext.cs:213-218`; `Presence_OnTheStartData_...` (ad listesi türden türer) |
| Kriter 4 — adıyla yazılır, `ILogger<T>` | ✓ | `StartupDatabaseCheck.cs:53-61`; `AssertRefusalLineNames` |
| Kriter 5 — varlık, değer değil | ✓ | `AppDbContext.cs` `ExistsAsync` (`AnyAsync`); `...NeverPrintsTheValue` |
| Kriter 6 — geçerli yokluklar | ✓ | `Start_WithoutTheAboutPage...Opens` (AboutPage, Announcement 0 satır doğrulanır; teslimat illeri ve taslaklar boşaltılır) |
| Kriter 7 — kalıp ve ayar dışı | ✓ | `Program.cs:48` (`AddInstallationSettings` 37'den sonra); `...NeverAsksTheDatabase` |
| KK1 boş il tablosu → düşer | ✓ | `Start_WithAnEmptyList_...(Province)` |
| KK2 boş ülke tablosu → düşer | ✓ | `Start_WithAnEmptyList_...(Country)` |
| KK3 altı kayıttan biri silinince düşer, ad yazılır | ✓ | `Start_WithASingleRowRecordMissing_...` (6 veri satırı); `Start_WithTheTableOfARecordMissing_...`; `Start_WithSeveralRecordsMissing_...` |
| KK4 iletide kayıt adı var, değer yok | ✓ | `DatabaseAuditTests.Failure_NamesTheRecordAndCarriesNoOtherRecordOrValue`; `...NeverPrintsTheValue` (Company.LegalName işaretçisi) |
| KK5 değeri boş ama satırı olan kayıt engellemez | ✓ | `Start_WithARecordWhoseValueIsEmptyButWhoseRowExists_Opens` |
| KK6 Hakkımızda satırsız açılış geçer | ✓ | `Start_WithoutTheAboutPage...Opens` |
| KK7 ayar zorunluları aranmaz, iletide geçmez | ✓ | `AssertNoOtherRecordNamed` (ayar anahtarları ve değerleri iletide yok); `...NeverAsksTheDatabase` |
| `09 §25` Test: beklenen değer sabitten türer | ✓ | testler ad ve ileti metnini `typeof(X).Name` ve `DatabaseFailures.*`'tan türetir; altı/iki kayıt listesi tek `Type[]` |
| `09 §25` DB: yeni tablo/sütun/göç | ✓ yok | şema değişikliği yok (FK dizini, varsayılan konuları uygulanmaz) |
| `09 §25` Mimari: yeni tarama/mimari kural | ✓ yok | yeni kural eklenmedi; mevcut `Infrastructure` → modül türü kuralı bağlam yöntemiyle korundu (Unit 548 yeşil) |
| `09 §25` Aynı sayıyı/adı anan öteki yerler | ✓ | `LogEvents` yorumu, `09 §18.1` madde 3 olay listesi, `.claude/CONTEXT.md` `Infrastructure/` satırı güncellendi; `grep StartupSettingsChecked` tek başka yer yok |
| `09 §25` Birim testi dosya sistemine yazmaz | ✓ | `DatabaseAuditTests` saf; dosya yok |
| Güvenlik taraması | ✓ | sır yok; ileti yalnız kayıt adı; bağlantı hatasında istisna yazılmaz, adres/ad log'da yok (`Start_WithADatabaseThatCannotBeReached_...`); yetki/girdi etkisi yok; yeni paket yok |
| Sık FAIL nedenleri 1–5 | ✓ | (1) beklenen değer türetilir; (2) öteki yerler güncel; (3) bekçi/tarama istisnası eklenmedi; (4) yeni kural yok, birim testi dosyaya yazmaz; (5) tablo yok |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ 548/548 | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj` | 0 fail (tutarlılık ve mimari testleri dahil) |
| Integration | ✓ 1004/1004 | `dotnet test --project tests/Shopfolio.IntegrationTests/...csproj --filter-namespace ...` üç parça (Web 52, Model 893, Database+FileSecrets 59) | 0 fail; yeni sınıf 17 test |
| Contract | — | — | bu task'ta yok |
| Biçim | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` | temiz |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | bekliyor |
| Bulgu sayısı | — |
| Düzeltme gerekli mi | — |
| Bloke-etmeyen not (K) sayısı | — |

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T205-acilista-veritabani-fail-fast`
- **Commit:** *(PR açıklamasında head SHA)*
- **PR:** #385
- **CI:** *(yardımcı izler)*
- **Post-merge run'lar:** *(validator doldurur — merge sonrası; bu satır ayrı bir `chore:` PR'ında veya sonraki task dalında güncellenir, doğrudan ana dala push edilmez)*

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | Bu task'ta dış varsayım yok | gerçek PostgreSQL 18 yerel Docker (`postgres:18`); yeni paket/tier/servis yok | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| — | Yok | — | — |

## Notlar

- **Working tree (Adım -1):** temiz; `core.hooksPath` = `scripts/git-hooks` (ana depo yolu)
- **Ana dal CI startup (Adım 0):** son üç tamamlanmış run success — Publish `41917927000`, CI `41917927066`, Publish `41903408101`
- **Önceki task kaydı (skill adım 15):** T233 status Commit hücresi `810fe27`; T233 raporu "Post-merge run'lar" satırı dolduruldu.
- **Repo memory (bitiş kapısı 9):** güncellendi
- **⚠ Kolay kararlar (öneriyle verildi):**
  1. Veritabanına ulaşılamazsa denetim atlanır (tek uyarı, açılış sürer; canlılık 503). Gerekçe: `LivenessEndpointTests` bu açılışı sabitler; bağlantı denetimi T226'nın değil canlılık ucunun işidir. Katı istenirse tek satırlık değişiklik (`CanConnectAsync` false iken atmak).
  2. Okuma `AppDbContext` yöntemi olarak yazıldı; çünkü `Infrastructure/` modül türüne ulaşamaz (mimari kural). Başka ev gerekirse mimari kuralın istisnası genişler.
  3. `09 §18.1` madde 3 olay listesine `StartupDatabaseChecked` eklendi (listenin kendi kuralı "yeni olay adı bu listeye eklenir").
  4. Tablosu hiç yapılmamış kayıt "eksik" sayılır ve adı yazılır (ileti "list is empty or its table is missing" / "row is missing").
- **Yerel ortam:** `shopfolio-t205-pg` konteyneri (127.0.0.1:57205) iş sonunda adıyla silindi.
