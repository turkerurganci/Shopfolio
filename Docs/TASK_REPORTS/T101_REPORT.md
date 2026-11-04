# T101 — Veri modeli temeli

**Faz:** F1 | **Durum:** ✓ PASS (doğrulama tur 1) — merge yardımcıda (#358) | **Tarih:** 2026-11-03

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.
> Validator bu raporu **Faz 3'e kadar açmaz** (izolasyon kuralı).

---

## Yapılan İşler

- `src/Shopfolio.Web/Infrastructure/`: `EntityIds` (UUID v7), `TextNormalizer.NormalizeIdentity`, `EmptyKeyGuard` (boş anahtar kesicisi), `EmptyTextGuard` (boş metin → `NULL`), `DataModel/ModelRulesConvention` (silme davranışı yazılmamışsa `NoAction`, Guid anahtar `ValueGeneratedNever`, `Version` int → concurrency token, `HasRule` → `ck_<tablo>_<kural>`, tipsiz `decimal` model kurmaz), `ModelBuilderExtensions` (`HasRule`, `AsMoney`, `AsPercent`, `AsNetQuantity`); `AppDbContext` kesicileri ve kuralları bağlar.
- `Microsoft.EntityFrameworkCore.Relational` 10.0.14 sabitlendi (`Directory.Packages.props`, `09` Kütüphaneler satırı, üç kilit dosyası).
- Testler: Unit (örnek model, kaynak taraması + ihlal örnekleri, temel testler), Consistency (`06 §2.5.1` tür tablosu eşitliği), Integration (gerçek PostgreSQL: UUID v7, tür/ad, sürüm, boş metin, `ck_` adı, `pg_constraint`'te `r` yok).
- `Docs/DEFERRED_BACKLOG.md`: D-37 (a) "T103'e" notu, yeni D-45.
- `origin/main` (EF Core 10.0.14, #357) dala birleştirildi.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Infrastructure/**`, `Directory.Packages.props`, `packages.lock.json` (3), `Docs/09_CODING_GUIDELINES.md`, `Docs/DEFERRED_BACKLOG.md`
- `tests/Shopfolio.UnitTests/{Samples,Architecture,Infrastructure,Consistency}`, `tests/Shopfolio.IntegrationTests/Model`

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Tek bağlam, `Infrastructure/` altında | ✓ | kaynak taraması testi (KK1) · validator: `DataModelFoundationTests.Solution_HasOneDbContextClass…` (yansıma, ad alanı `Shopfolio.Infrastructure`) ve `SourceFiles_DeclareNoSecondDbContext` geçti; `AppDbContext` dışında `: DbContext` yok |
| 2 | Anahtar `id`, UUID v7, uygulama üretir | ✓ | `SampleDatabaseTests` (`uuid_extract_version` = 7) (KK2) · validator: PG 18.6'da `uuid_extract_version(id)` = 7, `column_default` NULL; `ModelRulesConvention` Guid anahtarı `ValueGeneratedNever`; `EmptyKeyGuard` atanmamış anahtarı SQL'siz reddeder; tamsayı anahtar (T118) kurala girmez |
| 3 | Çoğul tablo, küçük harf sütun, `pk_/fk_/ix_/ck_` türer | ✓ | `SampleDatabaseTests` ad denetimi (KK3) · validator: katalogda `pk_sample_owners`, `ix_sample_owners_normalized_code`, `ck_sample_owners_price_not_negative`, `fk_sample_items_*`; her ad `^[a-z][a-z0-9_]*$` ve ≤ 63 bayt; `src`'de `ToTable`/`HasColumnName`/`HasDatabaseName`/`HasConstraintName` yok (kaynak taraması). Çoğulun `DbSet` adından gelişi ilk varlıkla sınanır (K2 → D-46 (b)) |
| 4 | Türler `06 §2.5.1` tablosuna uyar | ✓ | `ColumnTypeTable` eşitlik testi (KK4) · validator: katalogda `numeric(12,2)`, `numeric(5,2)`, `numeric(12,3)`, `timestamp with time zone`, `date`, `text`, `jsonb`, `inet`, `bigint`, `character varying(50)`; tipsiz `decimal` model kurmaz |
| 5 | `version` int, koşullu güncelleme | ✓ | eski sürümle güncelleme 0 satır (KK5) · validator: ikinci bağlamın eski sürümlü kaydı `DbUpdateConcurrencyException`; satır `First`/sürüm 2 kaldı |
| 6 | Tek normalleştirme işlevi | ✓ | `I İ ı i` aynı biçim, boşluk atılır (KK6) · validator: işlev `09 §9.4` m.1'in ifadesiyle birebir, kültürden bağımsız; tekil dizin `İSTANBUL-…`/`ISTANBUL-…` çiftini durdurur; ICU Türkçe sıralama `DatabaseUsersTests.Database_AfterSetup_IsUtf8WithIcuTurkishCollation…` (`UTF8\|i\|tr-TR`) |
| 7 | `created_at` ve boş değer yalnız `NULL` | ✓ | model testi + boş metin → `NULL` (KK7) · validator: isteğe bağlı sütunda `""` → `NULL`; zorunlu sütunda kayıt yazılmaz (ret veritabanının `NOT NULL`'undan, 23502 — K1 → D-46 (a)); `ModelRules` `created_at`'i zorunlu `timestamptz` bekler, ihlalli örnekte kırmızı |
| 8 | Silme davranışı açık, `RESTRICT` yok | ✓ | model testi + `pg_constraint` `r` yok (KK8) · validator: `confdeltype` `owner_id:c`, `emptied_owner_id:n`, `restrictive_owner_id:a`, `unwritten_owner_id:a`; ihlalli örnekte `Restrict` ve `ClientCascade` raporlanır; uygulamanın `Cascade` kümesi = kayıtlı liste (boş) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit + Consistency | ✓ | `dotnet test Shopfolio.slnx -c Release --no-restore` | UnitTests geçti |
| Integration | ✓ | aynı; `SHOPFOLIO_TEST_DATABASE` yerel PostgreSQL 18 | IntegrationTests geçti |
| Toplam | ✓ | aynı | 209 / 209, 0 başarısız |
| Contract | — | — | bu task'ta yok |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-03, head `f7b33c7`) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 3 (K1–K3 → D-46) |

### Doğrulama tur 1 — ✓ PASS (2026-11-03)

**Başlangıç kapıları:** working tree temiz; `core.hooksPath` = `C:\projects\Shopfolio\scripts\git-hooks` (ana checkout'un hook klasörü); ana dal son 3 run `41706376352`, `41706376093`, `41702892272` success; `MEMORY.md` satır 22'de T101.

**Kabul kriterleri (validator kanıtı):** 1–8 ✓ (tablo yukarıda). KK1–KK8 kanıtla geçti; `09 §25` ilgili satırlar evet ya da gerekçeli "uygulanamaz" (göç yok, tablo yok, enum T102).

**Testler:** `dotnet build Shopfolio.slnx -warnaserror` 0 uyarı/0 hata; Unit 128/128 (Consistency dahil); Integration 81/81 (yerel `postgres:18` 18.6, `Host=127.0.0.1` — `localhost` ile Windows'ta IPv6 denemesi canlılık testlerinin 2 sn sınırını aşar, T101 ile ilgisiz ortam etkisi); `dotnet restore Shopfolio.slnx --locked-mode` çıkış 0; dal CI `41706961892` (head `f7b33c7`) dokuz işin hepsi success (Guard skipped). `12`'de T101'in kaynak birimlerine (`06 §2.1`–§2.6, §2.8, §2.10; `02 §4.1.1`; `05` MK-03, MK-06, MK-19; `09 §5`, §10) bağlı ve bu task'ın kabul kriteriyle sınanan satır yok — yazılacak `Durum` hücresi yok; `12_matris_denetle.py --kapsam 1-77` hata 0.

**Güvenlik:** sır yok (örnek bağlantı dizileri parolasız yer tutucu); yetki ve girdi yüzeyi yok; yeni paket `Microsoft.EntityFrameworkCore.Relational` 10.0.14 zaten geçişli bağımlılıktı (aynı içerik özeti), Kütüphaneler tablosunda satırı var.

**K-notları:** K1 (D-46 (a)) — `EmptyTextGuard` yorumu reddi EF'e bağlar, gözlenen ret veritabanının `NOT NULL`'udur; test geniş yakalar. K2 (D-46 (b)) — tablo adının `DbSet` adından çoğul gelişi testte sınanmıyor. K3 (D-46 (c)) — `HasConversion<int>` taraması yok (T102).

**Yapım raporu karşılaştırması:** kriter 1–8 değerlendirmesi uyumlu. Uyuşmazlıklar (bloke etmeyen): (1) kriter 3'ün "çoğul tablo" kanıtı örnek tablolarda `ToTable` ile elle verilmiş addır, kural sınanmamıştır (K2); (2) yapım `Son güncelleme` bloğunda önceki girişi "Önceki:" diye tuttu — doğrulamada `STATUS_CHANGELOG.md`'ye taşındı; (3) backlog'un "Aktif kalem" sayısı D-45 ile artırılmamıştı — doğrulamada 24'e düzeldi.

## Altyapı Değişiklikleri

- **Şema/migration:** Yok (göç geçmişi tablosunu kuran ilk göç T103; migration-dry-run adım 1 yerelde geçti, adım 2+ CI'da)
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Var — `Microsoft.EntityFrameworkCore.Relational` 10.0.14 (MIT, EF Core ile aynı depo; sağlayıcı paketinin geçişli bağımlılığı, MSB3277 çakışmasını kapatmak için sabitlendi; `09` tablosunda satır)

## Commit & PR

- **Branch:** `task/T101-veri-modeli-temeli`
- **Commit:** `b7a332a`, `1098437`, birleştirme `e1287ee` — T101: Veri modeli temeli
- **PR:** #358
- **CI:** dal run `41706961892` (head `f7b33c7`) success
- **Post-merge run'lar:** squash `f2470d8` (#358) — CI `41710384453` ve Publish `41710384654` success (F1 gate'inde doğrulandı, 2026-11-04)

## Dış Varsayımlar

| # | Varsayım | Kanıt | Sonuç |
|---|---|---|---|
| 1 | `Microsoft.EntityFrameworkCore.Relational` 10.0.14 nuget'te var | `dotnet restore --force-evaluate` ve `--locked-mode` çıkış 0 | ✓ |
| 2 | PostgreSQL 18 (docker) ve `uuid_extract_version` (PG18) | Integration testleri yerel PG 18'de geçti | ✓ |

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | T101 sonrası açık kalem | D-45 | bkz. backlog |
| 2 | Yetki matrisi testinin göç geçmişi beklentisi | D-37 (a) | göç geçmişini T103 kurar |
| 3 | Doğrulama tur 1 K-notları (K1–K3) | D-46 | ilk varlık (T103) ve enum (T102) task'larında |

## Notlar

- **Working tree (Adım -1):** temiz
- **Ana dal CI startup (Adım 0):** run 41702892272, 41702891253, 41696545899 — hepsi success
- **Repo memory (bitiş kapısı 8):** güncellendi
- **Diğer:** ⚠ kararlar PR #358 açıklamasında numaralı.
