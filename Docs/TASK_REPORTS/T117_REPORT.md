# T117 — Gönderim kaydı ve kanal hâli

**Faz:** F1 | **Durum:** ✓ Tamamlandı (doğrulama tur 1 PASS) | **Tarih:** 2026-11-04

> Taslağı **yapım chat'i** doldurur; **doğrulama chat'i** finalize eder.

---

## Yapılan İşler

- `src/Shopfolio.Web/Modules/Notifications/` altında iki entity ve yapılandırması (`05 §3.3.8` "Bildirim" modülü): `DispatchRecord` (`06 §4.58`) ve `ChannelStateRecord` (`06 §4.60`). Alanlar `06` alan tablolarıyla birebir; `DispatchType`, `DispatchState`, `DispatchOutcome`, `DispatchRelatedRecordType` T102'nin `Notifications/Contracts` enum'larıdır.
- Varsayılanlar veritabanı default'udur: `DispatchState` `'Queued'`, `AttemptCount` ve `ConsecutiveChannelErrorCount` `0` (varlıkta `HasDefaultValue`, göçte `defaultValue`).
- `DispatchRecord` bağı: yalnız `OrderId` → `Order`, `NoAction` (kısıtlayıcı, `06 §2.2.5`, §7.2); `Order` `Ordering` modülündedir, bağ `const string` + `HasOne` kalıbıyla (`PaymentRecord` gibi; `09 §6` m.2). `RelatedRecordId` ve `OriginalDispatchId` yabancı anahtarsız değerdir (`06 §2.2.6`, §7.2.3).
- Kontrol kısıtları (`06 §4.58` Kısıtlar, §7.3): türe göre sipariş ve ilgili kayıt türü (`CASE dispatch_type …`, tablonun her satırı; her liste `IS NOT NULL` ile başlar — boş değer `IN` listesinden geçmez); tür ile kimlik birlikte dolu ya da boş; `SentAt` `Sent`'te dolu, `Queued`'da boş, `Undelivered`'da ikisi de kabul (K-1120); `Queued` dışında `NextAttemptAt` boş; `AttemptCount >= 0`. Enum sütunlarının izinli değer kısıtları kural gereği otomatik.
- Dizinler (`06 §4.58`, §7.5): (`DispatchState`, `NextAttemptAt`) `dispatch_state = 'Queued'` filtreli; (`OrderId`, `CreatedAt`) (ilk sütunu sipariş bağının dizini, §7.5.2); (`RelatedRecordType`, `RelatedRecordId`, `CreatedAt`); (`DispatchType`, `CreatedAt`); `CreatedAt`. Hiçbiri tekil değil.
- `ChannelStateRecord`: tek satır — `AsSingleRow()` + göçte elle `ix_channel_state_records_single_row ON (true)` (`Branding` kalıbı); bağı yok, dizini yalnız tek satır dizini; `ConsecutiveChannelErrorCount >= 0` ve `(sayı = 0) = (FirstChannelErrorAt IS NULL)`.
- Tek göç `20261104120550_CreateDispatchAndChannelStateTables` (`Down` yok; LF; BOM'lu): iki tablo. `AppDbContext`'e iki `DbSet`.
- Test güncellemesi: `EnumDatabaseTests` enum sütun sayısı 55 → 59 (dört yeni enum sütunu). `ModelRuleTests` değişmedi: yeni basamaklı bağ yok, `ChannelStateRecord` bağsızdır.
- T116 raporunda "Post-merge run'lar" dolduruldu; T116 durum girişi `STATUS_CHANGELOG.md`'ye taşındı.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/Notifications/{DispatchRecord,ChannelStateRecord}.cs` (yeni); `Infrastructure/AppDbContext.cs`; `Infrastructure/Migrations/*` (göç, tasarım dosyası, anlık görüntü).
- `tests/Shopfolio.IntegrationTests/Model/{DispatchAndChannelStateTablesTests (yeni),EnumDatabaseTests}.cs`.
- `Docs/IMPLEMENTATION_STATUS.md`, `Docs/STATUS_CHANGELOG.md`, `.claude/memory/MEMORY.md`, `Docs/TASK_REPORTS/T116_REPORT.md` (post-merge satırı).

## Kabul Kriterleri Kontrolü

Öz denetim (PR öncesi): her kriter ve kontrol listesi maddesi diff'te göç satırı ve testle `grep -n` ile tek tek karşılaştırıldı. Varsayılan sütunu DB default'u (`Columns_HaveTheDatabaseDefaultsOfTheDefaultColumns`); `OrderId` FK'sinin dizini var (`ix_dispatch_records_order_id_created_at`'ın ilk sütunu), `ChannelStateRecord`'un bağı yok; varlıklar `Notifications` modülünde; ON DELETE `06 §4.58` ile birebir (`ForeignKeys_AreTheRestrictiveOrderLinkOnly`: `a`; basamak yok, `ModelRuleTests` kayıtlı listesi değişmedi); kontrol kısıtları `06 §4.58` "Kısıtlar" ve K-1120 ile birebir (`Undelivered` satırında gönderim anı dolu da boş da kabul — `SentAt_IsFilledOnSent_EmptyOnQueued_AndEitherOnUndelivered`); bekçi/tarama istisnası eklenmedi. Öz denetimde bulunup düzeltilen: ilk yazımda `related_record_type IN (…)` boş değerde `NULL` döndüğü için, ilgili kayıt türü gereken türlerde (B-9, F-1…F-4, F-6, hesap e-postaları) boş tür kabul ediliyordu; her liste `IS NOT NULL` ile başlatıldı ve test her tür × her kombinasyonu denetliyor.

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | İki entity `06 §4.58` ve `§4.60` alan tablolarıyla tek göçle | ✓ | `Migration_MakesTheTwoTables`, `Columns_HaveTheTypesAndNullabilityOfTheFieldTables`, `Columns_HaveTheDatabaseDefaultsOfTheDefaultColumns`, `Indexes_AreTheDocumentedOnes`, `ForeignKeys_…`, `CheckConstraints_AreTheDocumentedOnes`; göç `20261104120550_CreateDispatchAndChannelStateTables` |
| 2 | Sipariş bağı ve ilgili kayıt türü gönderim türüne göre dolu/boş; tür ile kimlik birlikte | ✓ | `EveryTypeAgainstEveryCombination_IsAcceptedExactlyWhereTheTypeTableAllows` (29 tür × sipariş dolu/boş × 10 tür değeri; test kendi tablo kopyasını taşır), `TypeTable_NamesEveryDispatchType`, `RelatedRecordType_WithoutId_AndId_WithoutType_AreRefused` |
| 3 | `SentAt`/`NextAttemptAt`/`AttemptCount` kuralları, `Undelivered` iki yönlü | ✓ | `SentAt_IsFilledOnSent_EmptyOnQueued_AndEitherOnUndelivered`, `NextAttemptAt_IsAllowedOnlyWhileQueued`, `AttemptCount_BelowZero_IsRefused`, `OnlyTheTypeAndTheCreationAreRequired_AndTheDatabaseFillsTheStateAndTheCount` |
| 4 | İlgili kayıt kimliği değer; siparişe bağlı kayıt siparişi silinemez kılar | ✓ | `DeletingTheRelatedRecord_LeavesTheDispatchRow_AndTheFirstEmailLikewise`, `OrderWithADispatchRecord_CannotBeDeleted_AndAnUnknownOrderIsRefused` (`fk_dispatch_records_orders_order_id`), `ForeignKeys_AreTheRestrictiveOrderLinkOnly` |
| 5 | `ChannelStateRecord` tek satır; sayı ≥ 0; ilk hata anı sayı sıfırsa ve yalnız o zaman boş | ✓ | `SecondChannelStateRow_IsRefused` (`ix_channel_state_records_single_row`), `FirstErrorMoment_IsEmptyExactlyWhenTheCountIsZero` (7 durum), `ChannelState_StartsAtZeroWithEmptyMoments_…`, `ChannelState_CountIncrementIsOneUpdate…` |
| 6 | Göç tek konu; `Down` yok | ✓ | `Migration_HasNoDownMethod` (+ mevcut mimari tarama yeşil, Unit 245/245); `dotnet ef migrations has-pending-model-changes` → "No changes"; göçte `Down`/`now()`/`suppressTransaction`/`CONCURRENTLY`/`UPDATE`/`DELETE`/`GRANT` yok (grep) |

**Doğrulama kontrol listesi:**

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1 göç boş veritabanına, iki tablo | ✓ | `Migration_MakesTheTwoTables` (test veritabanı bütün göçlerle kurulur) |
| KK2 türe aykırı sipariş bağı reddedilir | ✓ | `EveryTypeAgainstEveryCombination_…` (ör. B-1 siparişsiz, F-1 siparişli, F-5 `Customer` türlü; hata `ck_dispatch_records_order_and_related_record_by_type`) |
| KK3 kuyruk/gönderilmiş satırda an; `Undelivered` iki yönlü; sıra dışında sonraki deneme anı; negatif sayı | ✓ | `SentAt_…`, `NextAttemptAt_…`, `AttemptCount_BelowZero_IsRefused` |
| KK4 ilgili kayıt silinince gönderim kaydı kalır | ✓ | `DeletingTheRelatedRecord_LeavesTheDispatchRow_…` |
| KK5 ikinci `ChannelStateRecord`; sayı/ilk hata anı uyumsuzluğu | ✓ | `SecondChannelStateRow_IsRefused`, `FirstErrorMoment_IsEmptyExactlyWhenTheCountIsZero` |
| KK6 kaynak taraması | ✓ | Unit'teki mimari/kaynak taramaları yeşil (245/245); üretim kodunda `Guid.NewGuid`, `CreateVersion7`, `Restrict`/`Client`/`Cascade`, `HasConversion<int>`, `DateTime.Now`, `UtcNow` yok (grep); anahtar `EntityIds.New()` |
| `09 §25` | ✓ | FK silme davranışı açık (`NoAction`); anahtar `EntityIds.New()`; göç genişletme, `Down`/`now()`/`suppressTransaction`/`CONCURRENTLY`/`UPDATE`/`DELETE` yok (`§10.6`); yeni tablolar varsayılan yetkide; yeni kapalı değer ve paket yok (enum'lar T102'nin); modül sınırı — `Order`'a yalnız tür adıyla FK |
| Güvenlik taraması | ✓ | Sır yok; yetki/kimlik doğrulama etkisi yok; giriş noktası yok (yalnız şema); yeni dış bağımlılık yok. `RecipientEmail` ve `EventValues` kişisel veri taşıyabilir: boşaltma ve imha `06 §4.58` "Uygulamada"dır (imha task'ları), şema bunu engellemez |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj -c Release` | 245/245 (203 + Consistency 42) |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests/Shopfolio.IntegrationTests.csproj -c Release` (PostgreSQL 18, `t117-pg`) | 871/871; T117 sınıfı 30 test |
| Build | ✓ | `dotnet build Shopfolio.slnx -c Release --no-restore -warnaserror` | 0 uyarı, 0 hata |
| Biçim | ✓ | `dotnet format Shopfolio.slnx --no-restore --verify-no-changes` | çıkış 0 |
| Göç | ✓ | `dotnet ef migrations has-pending-model-changes` | "No changes" |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✓ PASS — tur 1 (2026-11-04, head `ba81b23`, PR #375) |
| Bulgu sayısı | 0 |
| Düzeltme gerekli mi | Hayır |
| Bloke-etmeyen not (K) sayısı | 0 |

### Doğrulama tur 1 — ✓ PASS (2026-11-04)

**Kapılar:** working tree temiz; `core.hooksPath` mutlak yolla `scripts/git-hooks` (ana checkout'un hook'ları; eşdeğer); ana dal son 3 run `41825046782` (Publish), `41825046994` (CI), `41820820561` (Publish) success; `MEMORY.md`'de T117 satırı var.

**Kabul kriterleri:** kapsam tam (6 kriter, KK1–KK6, `09 §25`, güvenlik); hepsi ✓. Validator göçü ve iki yapılandırmayı `06 §4.58`, §4.60, §4'ün girişi, §2.2.5, §2.2.6, §2.3.2, §2.4.3, §7.1–§7.5 satırlarına ve önceki tek satır kalıbına (`Company`, `Branding`) karşı elle okudu:
- Kriter 1, 6: iki tablo tek göçte (`20261104120550_CreateDispatchAndChannelStateTables`), `Down` yok; alan, tip ve boşluk `06` alan tablolarıyla birebir (`id`, `created_at` + 12 ve + 3 alan); varsayılanlar DB default'u (`'Queued'`, `0`, `0`); beş dizin `06 §7.5` satırlarıyla eşit, kuyruk dizini `dispatch_state = 'Queued'` filtreli; göçte `suppressTransaction`/`CONCURRENTLY`/`now()`/`UPDATE`/`DELETE` yok (grep eşleşmeleri yalnız enum adlarındaki `Deleted`); model ve anlık görüntü uyumlu (validator `dotnet ef migrations has-pending-model-changes` → "No changes").
- Kriter 2: `CASE dispatch_type` kısıtı `06 §4.58` tür tablosunun 11 satırıyla validator karşılaştırmasında birebir; her `IN` listesi `IS NOT NULL` ile başlıyor (boş türün `NULL` sonucu kısıtı geçmez), `ELSE false`; tür ile kimlik birlikte kısıtı var. Test tablonun bağımsız kopyasıyla 29 tür × sipariş dolu/boş × 10 tür değerini dener.
- Kriter 3: `sent_at_with_state` `Sent`'te dolu, `Queued`'da boş ister, `Undelivered`'ı serbest bırakır (K-1120); `next_attempt_only_when_queued`; `attempt_count >= 0`.
- Kriter 4: tek FK `fk_dispatch_records_orders_order_id`, `NO ACTION` (`06 §2.2.5`, §4'ün girişi — `RESTRICT` değil); `RelatedRecordId` ve `OriginalDispatchId` FK'siz (`06 §7.2.3`); sipariş silme `23503` ile reddediliyor, ilgili kayıt ve ilk satır silinince gönderim satırı kalıyor.
- Kriter 5: `AsSingleRow()` + göçte `ix_channel_state_records_single_row ON public.channel_state_records ((true))` (`Company`, `Branding` kalıbı); iki kontrol kısıtı `06 §4.60` ile birebir; sürüm alanı yok (`06 §4.60` "sürüm taşımaz").

**`09 §25`:** göç genişletmedir; FK silme davranışı açık; anahtar `EntityIds.New()`; enum sütunları kod adıyla metin ve izinli değer kısıtlı (enum'lar T102'nin, yeni kapalı değer yok); yeni tablolar varsayılan yetkide (`Database` testleri yeşil); modül sınırı — `Order`'a yalnız tür adıyla bağ; varlıklar `05 §3.3.8` "Bildirim" modülünde; diff yalnız görevin istediği (T116 post-merge satırı dışında).

**Testler (validator, yerel PostgreSQL 18 `shopfolio-t117-val-pg`):** build `-warnaserror` 0 uyarı; `dotnet format --verify-no-changes` çıkış 0; Unit 245/245 (Consistency dahil); Integration 871 — T117 sınıfı 30/30, Model sınıfları iki grupta 452/452 ve 327/327, Database+FileSecrets+Web 62/62; yerel zaman aşımı yok; `dotnet restore --locked-mode` çıkış 0; koşu sonrası `git status` temiz. Dal CI `41827953732` (head `ba81b23`) success (önceki `41827881422` head `68df0dc` iptal — yeni push).

**Güvenlik:** sır yok; yalnız şema, auth etkisi yok; girdi doğrulama uygulanamaz (uç yok); yeni bağımlılık yok (csproj ve kilit dosyası değişmedi).

**`12` satırları:** kaynak birimleri `06` entity'leri ve `05 MK-38`, §4.4 (KP yok); `VAL-A951` `06 §4.58`'i anar ama geri dönme bildiriminin işlenişi ve uyarının satırıdır (gönderim akışı task'ının), T117'nin değil; `Durum` değişmedi; `12_matris_denetle.py --kapsam 1-77` hata 0.

**K-notları:** yok.

**Yapım raporu karşılaştırması:** kriter tablosu, KK listesi ve ⚠ kararlar validator'ın bağımsız okumasıyla tam uyumlu. ⚠ 2 (kanal hâli için silme bekçisi yok) kriter dışıdır; kalıcı kayıtların imhada silinmemesi imha task'ının kriteridir (`11` "Kalıcı kayıtlar … süreyle … silinmez", `06 §9.1.4`).

## Altyapı Değişiklikleri

- **Şema/migration:** Var — iki tablo (genişletme göçü).
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T117-gonderim-kaydi-kanal-hali`
- **PR:** #375
- **CI:** yardımcı izler
- **Post-merge run'lar:** squash `b7bd9b2` — CI `41830794252` ve Publish `41830794202` success

## Dış Varsayımlar

Dış varsayım: yok (plan katmanı, kota, dış servis, paket sürümü yok; PostgreSQL 18 CI ve yerel konteynerle aynı).

## ⚠ Kararlar

1. **Başlangıç satırı bu task'ta yok.** `ChannelStateRecord`'un tek satırı başlangıç verisinden doğar (`06 §4.60`, §8.1); göçü T119'dadır. Bu task yalnız şemayı ve tek satır kısıtını kurar.
2. **`ChannelStateRecord` için silme bekçisi eklenmedi.** `06 §4.60` "kalıcı" der; kriterler bekçi istemez ve `Branding`/`AboutPage` bekçisi başka bir kaydın bekçisidir. Kalıcılığı kullanan akış task'ı (gönderici) satırı hiç silmez; gerekirse bekçi ayrı bir kararla eklenir.
3. **Ham SQL kısıtı `CASE` olarak yazıldı.** Tür tablosu tek `CASE dispatch_type` ifadesidir; `ELSE false` bilinmeyen türü de reddeder (enum kısıtı zaten reddeder, iki katman).

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | Alıcı ve olay değerinin boşaltılması, yeniden gönderim koşulları, e-posta düzeltmesinin `Queued` satırlara yazması, kanal hâlinin aynı işlemde güncellenmesi | — | "Uygulamada" kuralları; gönderim ve imha task'larının işi |
| 2 | Başlangıç `ChannelStateRecord` satırı | — | T119 (başlangıç verisi göçü) |

## Notlar

- **Working tree (Adım -1):** temiz.
- **Ana dal CI startup (Adım 0):** `gh run list --branch main`: CI `41825046994` ve Publish `41825046782` (head `7afeaff`) success; önceki CI `41820820591` ve Publish `41820820561` success.
- **Dış varsayım:** yok.
- **Bağlı backlog:** `grep -n "T117" Docs/DEFERRED_BACKLOG.md` boş — kapanan kalem yok, açılan yok.
- **Repo memory (bitiş kapısı 8):** güncellendi.
