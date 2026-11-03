# T102 — Enum tanımları ve değer dönüşümü

**Faz:** F1 | **Durum:** ✓ PASS (doğrulama tur 1) — merge yardımcıda (#359) | **Tarih:** 2026-11-03

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- 42 enum (`06 §5.2`–§5.6 + `AuditOperation` 43 kod): sahip modülün `Modules/<Modül>/Contracts/` ad alanında; modül dışı kalıcı altyapı enumları `Infrastructure/Values/`; `LinkState` (sütun değil) `Infrastructure/`. Çıplak kod adı; sayı/yorum kuralı yok.
- `Infrastructure/DataModel/EnumText.cs` (`EnumText<T>`, `EnumTextConverter<T>`, katı okuma), `ModelBuilderExtensions.HasAllowedValues`, `ModelRulesConvention.ApplyEnumRules` (metin dönüşümü zorunlu, `ck_<tablo>_<sütun>` izinli değer kısıtı; `AuditOperation` adla istisna), `AppDbContext.ConfigureConventions` (tüm enumlara dönüşüm; `AuditOperation` hariç).
- `Modules/Administration/Contracts/AuditOperationText.cs`: `ToCode`/`TryParse`/`Display` (tanınmayan kodu aynen gösterir), `AreaOf` (43 kod → alan).
- Testler: `Locks` (41 kilit dosyası + `EnumLockTests`), `Consistency/EnumTable`, `EnumSourceScan` (+ ihlal örnekleri), `DataModelSourceScan`'a `HasConversion<int>` kuralı (D-46 (c)), `EnumConversionTests`, örnekler, `IntegrationTests/Model/EnumDatabaseTests`.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/*/Contracts/**`, `src/Shopfolio.Web/Infrastructure/{Values,DataModel}/**`, `AppDbContext`
- `tests/Shopfolio.UnitTests/{Locks,Consistency,Architecture,Infrastructure,Samples}`, `tests/Shopfolio.IntegrationTests/Model`
- `Docs/DEFERRED_BACKLOG.md`, `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `.claude/CONTEXT.md`, `.claude/memory/MEMORY.md`

## Kabul Kriterleri Kontrolü

Kriterler `Docs/11_IMPLEMENTATION_PLAN.md` `#### T102` bloğundadır. Tablo doğrulamada `11`'in altı kriterinin numarasına göre yeniden yazıldı (yapım taslağı beş satırdı, numaralar `11` ile örtüşmüyordu ve kriter 6 yoktu).

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Her enum `06 §5.2`…`§5.6` tablosunun kod adlarıyla birebir (KK1) | ✓ | `EnumTableConsistencyTests` (`06 §5`'in 41 enumu ↔ kod; `09 §9.2`'nin 43 kodu ↔ `AuditOperation`) · validator: `06 §5.2`–§5.6 elle okundu (6 + 11 + 10 + 3 + 11 = 41 enum; `LinkState` sütun değil, kilitsiz); mutasyon — `OrderStatus`'a `Returned` eklenince `DataModelDocument_AndTheEnums_NameTheSameMembers` ve kilit testi kırmızı, geri alındı |
| 2 | Değer metin olarak kod adıyla; sayısal değer anlamsız (KK2) | ✓ | `EnumDatabaseTests.EnumRecord_AfterSave_ShowsTheCodeNamesAsTextInTheDatabase` (PG 18: `Archived`, `BankTransfer`, `S10`, `B16`; `data_type` = `text`) · `EveryMemberOfAnEnum_IsWrittenAndReadBack…` (`TransitionCode`'un 17 üyesi) · birim: 41 kalıcı enumun her üyesi dönüştürücüde gidiş-dönüş · `DataModelSourceScan` `HasConversion<int>`/`EnumToNumberConverter` kuralı ihlal örneğiyle kırmızı, kaynakta bulgu yok |
| 3 | İzinli değer kısıtı; alt küme; `audit_logs.operation` istisnası (KK3) | ✓ | PG 18: `Bogus`/`archived`/`1`/`Cash`/`S12` → 23514 ve doğru `ck_` adı; alt küme `Archived`'ı reddeder (ham SQL ve EF yazımı), `Published`'ı kabul eder; `pg_constraint` listesi ↔ üyeler iki yönlü (5 sütun) · `audit_logs` T106'da doğar: istisna `ModelRulesConvention` satır 93'te tür adıyla; validator geçici örnekle gösterdi — `AuditOperation` + metin dönüşümü `ck_` almaz, aynı varlıkta `AuditArea` `ck_…_area` alır; kalıcı test T106'ya (K1 → D-48 (a)) |
| 4 | Dönüşüm tek yerde, bilinmeyen metinde hata; `AuditOperation` okuyucusu tanınmayanı kodla gösterir (KK4) | ✓ | `EnumText<T>`/`EnumTextConverter<T>` tek dönüşüm, `AppDbContext.ConfigureConventions` bütün enumlara verir; PG'de kısıtı kaldırılıp yazılan `Bogus` okunurken `InvalidOperationException`; birim: `Bogus`, `published`, `1`, `""`, `Draft,Published` reddedilir, tanımsız değer yazılmaz · `AuditOperationText.TryParse("DoSomethingNew")` false, `Display` kodu aynen döndürür |
| 5 | Kalıcı her enum için `UnitTests/Locks/<EnumTamAdı>.txt`, iki yönlü (KK5) | ✓ | 41 kilit dosyası (40 kalıcı `06` enumu + `AuditOperation`); `EveryStoredEnum_HasALockFile…` ve `EveryLockFile_BelongsToAStoredEnum` yeşil; mutasyon (kriter 1) kilit testini kırdı · model yürüyüşündeki ad alanı süzgeci K2 → D-48 (b) |
| 6 | Davranış kuralın evinde, enum yinelemez (KK6) | ✓ | `EnumSourceScan` `Contracts`/`Values` altındaki 41 enum dosyasında yalnız ad alanı/baş/parantez/çıplak üye; ihlal örnekleri (yöntem, sayı, öznitelik, dosya adı) kırmızı · validator: enum dosyaları okundu, yorumlarda kural yok; `AreaOf` eşlemesi enum dosyasında değil `AuditOperationText`'te (`09 §9.2` m.1) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit + Consistency | ✓ | `dotnet test Shopfolio.slnx -c Release --no-restore` | 199 / 199 |
| Integration | ✓ | aynı; `SHOPFOLIO_TEST_DATABASE` yerel PostgreSQL 18 | 94 / 94 |
| Biçim / restore | ✓ | `dotnet format --verify-no-changes`; `dotnet restore --locked-mode` | temiz, çıkış 0 |
| Contract | — | — | bu task'ta yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-03, head `8277427`) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 2 (K1–K2 → D-48); D-47'nin hedef sütunu düzeltildi |

### Doğrulama tur 1 — ✓ PASS (2026-11-03)

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` (ana checkout'un hook klasörü; içerik daldakiyle aynı); ana dal son 3 run `41710384453`, `41710384654`, `41706376352` success; `MEMORY.md` satır 22'de T102.

**Kabul kriterleri (validator kanıtı):** 1–6 ✓ (tablo yukarıda), KK1–KK6 kanıtla geçti. `12`'de T102'nin kaynak birimlerine (`06 §2.9`, §5.1–§5.6; `09 §12`) bağlı satır yok — yazılacak `Durum` hücresi yok; `12_matris_denetle.py --kapsam 1-77` hata 0. `09 §25`: ad uyumu, kapsam, `CONTEXT.md` (`Infrastructure/Values/` satırı), modül sınırı (altyapı modül türüne adla bakar, başvurmaz), bilgim tarihli olgular (D-45 notu), kilit dosyası ↔ değer, test katmanı evet; göç, yük biçimi, sayfa, POST, para "uygulanamaz" (dokunulmadı).

**Testler:** `dotnet build Shopfolio.slnx -c Release -warnaserror` 0 uyarı/0 hata; Unit 199/199 (Consistency dahil) + betik testleri 4 dosya; Integration 94/94 (yerel `postgres:18`, `EnumDatabaseTests` 13/13); `dotnet format --verify-no-changes` ve `dotnet restore Shopfolio.slnx --locked-mode` çıkış 0; dal CI `41712894839` (head `8277427`) dokuz işin hepsi success (Guard skipped).

**Güvenlik:** sır yok; yetki ve girdi yüzeyi yok; yeni paket ve göç yok.

**K-notları:** K1 (D-48 (a)) — `audit_logs.operation`'ın eşlemesi T106'ya kalıyor ve kısıtsızlığı testle kilitli değil; iki dönüştürücü de tanınmayan kodda hata attığından alan metin özelliği olmalı. K2 (D-48 (b)) — kilit testinin model yürüyüşü ad alanıyla süzülüyor; enum taraması taban türlü bildirimi tanımıyor.

**Yapım raporu karşılaştırması:** sonuç uyumlu (PASS). Uyuşmazlıklar (bloke etmeyen): (1) yapımın kriter tablosu `11`'in numaralarını izlemiyordu, kriter 6 yoktu — yeniden yazıldı; (2) D-47'nin hedefleri ters yazılmıştı (`ErrorCode` T213'ün, `NoticeCode` T214'ün kriteri; etiket testi T380) — düzeltildi; (3) PR kararı 2 `audit_logs` varlığını T380'e bağlar, varlık T106'nındır (K1).

## Altyapı Değişiklikleri

- **Şema/migration:** Yok (enum sütunları ilk varlıkla T103+; kısıt mekanizması hazır)
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T102-enum-tanimlari`
- **Commit:** `84f729c` — T102: Enum tanımları, metin dönüşümü, izinli değer kısıtı, kilit dosyaları ve testler
- **PR:** #359 — "T102: Enum tanımları ve değer dönüşümü"
- **CI:** dal run `41712894839` (head `8277427`) success

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | EF Core API'leri: `ConfigureConventions` + `HaveConversion`, finalizing kuralında `GetColumnName(StoreObjectIdentifier.Table)`, `AddCheckConstraint` sütun kısıtı, `ValueConverterType` ek açıklaması | Integration testleri yerel PG 18'de geçti | ✓ (D-45 notu) |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `AuditOperation` etiket testi; `ErrorCode`/`NoticeCode` kilit testine eklenmesi | D-47 | mesaj kataloğu ve kodlar sonraki task'larda |
| 2 | D-46 (a), (b) | D-46 | T103 (ilk varlık); (c) bu task'ta ✅ |
| 3 | Doğrulama K1–K2: `audit_logs.operation` eşlemesi ve kısıtsızlık testi; kilit testinin ad alanı süzgeci | D-48 | tablo T106'da doğar; ilk varlık T103 |

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** run 41710384453, 41710384654, 41706376352 — hepsi success
- **Hook:** `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` (kurulu)
- **Repo memory (bitiş kapısı 8):** güncellendi
- **Diğer:** ⚠ kararlar PR açıklamasında numaralı.
