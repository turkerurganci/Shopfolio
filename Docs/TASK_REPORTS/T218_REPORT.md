# T218 — Dokuz modülün iskeleti ve Contracts

**Faz:** F2 | **Durum:** ⏳ Devam ediyor (yapım bitti, doğrulama bekliyor) | **Tarih:** 2026-11-04

> Taslağı yapım chat'i doldurur; doğrulama chat'i finalize eder.

---

## Yapılan İşler

- F1'de varlıklar dokuz modül klasörüne yerleşmişti ve `Contracts` dışındaki türler zaten `internal`'dı; T218 iskeleti tamamladı: sekiz modülde `Contracts` vardı, `Membership`'te yoktu. Dokuz modülün hepsine açık arayüz `I<Modül>Module` (`Contracts`'ta, boş: üyeleri kuralların sahibi task'lar ekler) ve iç gerçeklemesi `<Modül>Module` + tek `Add<Modül>Module` kaydı eklendi; `Program.cs` dokuzunu birer kez çağırır.
- Mimari test (`ModuleBoundaryTests`) genişledi: modül sınırı (bağlam üzerinden dokunuş dahil), `Contracts` dışı türlerin `public` olmaması, `Contracts`'ın iç türlere uzanmaması, sayfa ve uçların modül iç türüne, `Microsoft.EntityFrameworkCore`'a ve `AppDbContext`'e dokunmaması, altyapının modül türüne başvurmaması (tek istisna `AppDbContext`, adıyla), dokuz modül adı, dokuz klasör + `Contracts`, kiracı/operatör adı yokluğu. Her kural için ihlal örneği `ViolationSamples/ModuleBoundary/Sample.cs`'te (ayrı derleme).
- Kaynak taraması `ModuleSourceScan`: `Program.cs` modül başına tek düz çağrı (eksik, çift, zincirli/argümanlı çağrı kırar) ve uygulamada olay yayıncısı adı yok; ihlal örnekleri `ViolationSamples/ModuleSource/`.
- Integration: `ModuleCallTransactionTests` — çağıran işleminde çağrılan modülün yazımı ortak işlemdedir (kesinleşir ya da birlikte geri alınır).
- Backlog: D-13 kapandı; D-21 ve D-70'e yardımcının F1 gate kararları işlendi; D-21 için `09` satırı ⚠ olarak önerildi.
- `CONTEXT.md` klasör haritası güncellendi.

## Etkilenen Modüller / Dosyalar

- `src/Shopfolio.Web/Modules/<dokuz modül>/<Modül>Module.cs` ve `.../Contracts/I<Modül>Module.cs` (18 yeni dosya), `src/Shopfolio.Web/Program.cs`
- `tests/Shopfolio.UnitTests/Architecture/ModuleBoundaryTests.cs`, `ModuleSourceScan.cs`, `ModuleSourceScanTests.cs`, `ViolationSamples/ModuleBoundary/Sample.cs`, `ViolationSamples/ModuleSource/*`
- `tests/Shopfolio.IntegrationTests/Model/ModuleCallTransactionTests.cs`
- `Docs/DEFERRED_BACKLOG.md`, `.claude/CONTEXT.md`, `.claude/memory/MEMORY.md`, `Docs/IMPLEMENTATION_STATUS.md`
- Varlık, yapılandırma, göç ve `InternalsVisibleTo` değişmedi.

## Kabul Kriterleri Kontrolü

| # | Kriter | Sonuç | Kanıt |
|---|---|---|---|
| 1 | Dokuz modül kendi klasöründe; kiracı ve operatör paneli yok | ✓ | `ModuleBoundaryTests.ModuleFolders_InSource_AreTheNineModulesEachWithContracts`, `ModuleNames_InApplicationAssembly_AreTheNineModulesOnly`, `Application_HasNoTenantAndNoOperatorPanel` |
| 2 | Ortak altyapı modül değil, iş kuralı taşımaz | ✓ | `ModuleNames_InApplicationAssembly_AreTheNineModulesOnly` (`Infrastructure` modül adı değil); `Infrastructure_InApplicationAssembly_ReachesNoModuleTypeExceptInTheContext` |
| 3 | Yalnız `Contracts` açık, iç türler `internal` | ✓ | `ModuleTypes_OutsideContracts_AreNotPublic`, `ContractsTypes_InApplicationAssembly_DoNotReachModuleInternals`, `EveryModule_..._HasAContractsNamespaceWithAnOpenInterface` |
| 4 | Başka modül tablosuna doğrudan yazma/okuma yok | ✓ | `ModuleReferences_InApplicationAssembly_OnlyGoThroughContracts`; ihlal: `ModuleReferences_InViolationSample_BreakTheRuleThroughTheContextToo` (`ForbiddenContextReader`) |
| 5 | Modüller arası çağrı eşzamanlı, çağıranın işleminde | ✓ | `ModuleCallTransactionTests` (3 test: kesinleşme, birlikte geri alma, çağrılan tek başına yazmaz) |
| 6 | Dış yan etki kuyruk satırı; olay yayını yok | ✓ (olay yayını) / T220 (kuyruk satırı) | `ModuleSourceScanTests.Application_HasNoEventPublisher`; kuyruk satırı yazan kod T220'nin işidir (`Jobs/` yalnız satır türü) |
| 7 | Mimari test iç türe ve altyapıdan modül türüne başvuruda kırılır (tek istisna `AppDbContext`) | ✓ | `ModuleReferences_InViolationSample_BreakTheRuleOnlyForTheInternalReference`, `Infrastructure_InViolationSample_ReportsTheForbiddenTypeAndSparesTheContextClass` (istisna sınıf adıyla: `^Shopfolio\.Infrastructure\.AppDbContext(\+.*)?$`) |
| 8 | Vitrin ve panel modül değil; sayfa bağlama dokunmaz | ✓ | `PagesAndEndpoints_InApplicationAssembly_TouchNeitherModuleInternalsNorTheContext`, ihlal `PagesAndEndpoints_InViolationSample_...`; vitrin/panel modül adı değil: dokuz modül testi |
| 9 | `Program.cs` modül başına tek kayıt uzantısı | ✓ | `ModuleSourceScanTests.Program_RegistersEachModuleWithOnePlainCall`; ihlal `ScanProgram_OnViolationSample_ReportsTheRuleForTheModule` (zincirli, argümanlı, çift, eksik) |

## Öz Denetim (PR öncesi; her madde kendi diff'inde denetlendi)

| Madde | Sonuç | Kanıt |
|---|---|---|
| KK1: dokuz klasör, her birinde `Contracts`; kiracı yok | ✓ | `ModuleFolders_InSource_AreTheNineModulesEachWithContracts` (klasör kümesi birebir dokuz, her `Contracts`'ta `.cs`), `Application_HasNoTenantAndNoOperatorPanel` (tür, ad alanı, özellik adlarında `Tenant`/`Operator` yok); ad eşlemesi 05 §3.3.1: Katalog `Catalog`, Sepet ve sipariş `Ordering`, Ödeme `Payment`, İptal–cayma–iade `AfterSales`, Üyelik `Membership`, Kurumsal içerik `CorporateContent`, Bildirim `Notifications`, Yönetim ve işlem izi `Administration`, Ayarlar `Settings` (klasör adları F1'den, değişmedi) |
| KK2: ortak altyapı modül sayılmaz | ✓ | `ModuleNames_InApplicationAssembly_AreTheNineModulesOnly`; altyapı `Shopfolio.Infrastructure.*` ad alanında, `Shopfolio.Modules.*` dışında |
| KK3: `Contracts` dışı `internal` | ✓ | `ModuleTypes_OutsideContracts_AreNotPublic`; ihlal `ModuleTypes_InViolationSample_ABreakOfTheSurfaceIsReportedForThePublicLeakOnly` (`PublicLeak`; `AllowedCaller` internal olarak sessiz) |
| KK4: başka modül tablosuna erişim kırmızı | ✓ | ihlaller `ForbiddenCaller` (iç tür) ve `ForbiddenContextReader` (`AppDbContext.Products` → `DbSet<ProductEntity>`); `AllowedCaller` (Contracts çağrısı) sessiz |
| KK5: örnek çağrı aynı işlemde | ✓ | `ModuleCallTransactionTests.CallerCommit_...`, `CallerRollBack_UndoesTheCallersAndTheCalleesWriteTogether` (gerçek PostgreSQL) |
| KK6: olay yayıncısı yok; e-posta dışı yan etki kuyruk satırı | ✓ / T220 | `Application_HasNoEventPublisher` + ihlal `ScanEventPublisher_OnViolationSample_ReportsTheLine` (3 satır); kuyruk satırı yazımı T220 (bu task'ta dış yan etki kodu yok) |
| KK7: ihlal örneği testi düşürür; `AppDbContext` dışı altyapı türü de düşürür | ✓ | `ModuleReferences_InViolationSample_...`, `Infrastructure_InViolationSample_ReportsTheForbiddenTypeAndSparesTheContextClass` (`ForbiddenInfrastructure` kırar, sample'daki `AppDbContext` kırmaz) |
| KK8: sayfa `DbContext`'e başvurmaz | ✓ | `PagesAndEndpoints_InViolationSample_...` (`ForbiddenPage` → `AppDbContext`/EF, `ForbiddenPageEntity` → iç tür; `AllowedPage` sessiz); gerçek derlemede 0 ihlal (`Pages` ve `Endpoints` ad alanları) |
| KK9: `Program.cs` modül başına tek çağrı | ✓ | `Program_RegistersEachModuleWithOnePlainCall`; `grep -n "Add.*Module()" src/Shopfolio.Web/Program.cs` 9 satır |
| 09 §25 — doküman uyumu ve kapsam | ✓ | 09 §1, §2, §4.1, §5, §6, §9, §23, §24, §25; 05 §3.3, §3.4; 10 KD-21; yeni adlar (`I<Modül>Module`) 09 §6 madde 3'ün `Add<Modül>Module` kalıbı ve madde 1'in `Contracts` düzenindendir, arayüz üyesi uydurulmadı; diff yalnız iskelet, mimari test ve ilgili belgeler |
| 09 §25 — mimari ve kod | ✓ | `CONTEXT.md` harita satırları güncel; ArchUnitNET testleri yeşil; arayüz metodu yok, `SaveChanges` yok; `InternalsVisibleTo` değişmedi |
| 09 §25 — test (§19.2 m.3, §21.4) | ✓ | her kural için ayrı ihlal örneği (`Sample.cs`, `ViolationSamples/ModuleSource/`); kaynak taraması `UnitTests/Architecture/`'da; birim testi dosya yazmaz (yalnız okur); eşitlik denetimi eklenmedi |
| 09 §25 — veri ve göç, performans, para | ✓ | göç, model, ayar, paket yok (kilit dosyası değişmedi) |
| Güvenlik taraması | ✓ | diff'te sır, parola, token, bağlantı dizesi yok; auth ve girdi doğrulama etkisi yok; yeni bağımlılık yok (`Microsoft.Extensions.DependencyInjection` çatıdan) |

## Test Sonuçları

| Tür | Sonuç | Komut | Çıktı özeti |
|---|---|---|---|
| Unit | ✓ | `dotnet test --project tests/Shopfolio.UnitTests/Shopfolio.UnitTests.csproj` | 366 geçti, 0 kaldı (Consistency dahil) |
| Integration | ✓ | `dotnet test --project tests/Shopfolio.IntegrationTests/Shopfolio.IntegrationTests.csproj` (yerel `postgres:18`) | 955 geçti, 0 kaldı |
| Biçim ve derleme | ✓ | `dotnet format Shopfolio.slnx --verify-no-changes`; `dotnet build -c Release -warnaserror` | temiz; 0 uyarı |
| Contract | — | yok | |

## Doğrulama

| Alan | Sonuç |
|---|---|
| Doğrulama durumu | ✗ FAIL — tur 1 (2026-11-04, bağımsız doğrulayıcı; dal head `33d7551`) |
| Bulgu sayısı | 1 (DÜŞÜK, S1) |
| Düzeltme gerekli mi | Evet — B1 |
| Bloke-etmeyen not (K) sayısı | 2 — D-72, D-73'e forward |

### Doğrulama tur 1 — validator kanıtları

| # | Kriter | Sonuç | Validator kanıtı |
|---|---|---|---|
| 1 | Dokuz modül klasörü; kiracı/operatör yok | ✓ | 9 klasör (`Administration`…`Settings`), her birinde `Contracts/I<Modül>Module.cs`; kod adları `09 §5.1` madde 5 tablosuyla birebir; `src/` taramasında `tenant` yok, `operator` yalnız göçlerin `gin_trgm_ops` dizin işleci |
| 2 | Ortak altyapı modül değil | ✓ | `Shopfolio.Infrastructure.*` ayrı ad alanı; `ModuleNames_InApplicationAssembly_AreTheNineModulesOnly`, altyapı kuralı yeşil |
| 3 | Yalnız `Contracts` açık | ✓ | `Modules/` altında `public` tür yalnız `Contracts/` dizinlerinde (42 tür, grep); `ModuleTypes_OutsideContracts_AreNotPublic` + ihlal `PublicLeak` |
| 4 | Başka modül tablosuna erişim yok | ✓ | `ForbiddenCaller` (iç tür) ve `ForbiddenContextReader` (`AppDbContext.Products`) örnekte kırar, `AllowedCaller` sessiz; uygulamada 0 ihlal |
| 5 | Çağrı eşzamanlı, aynı işlemde | ✓ | `ModuleCallTransactionTests` 3/3 gerçek PostgreSQL 18'de (yerel konteyner) |
| 6 | Olay yayını yok; dış yan etki kuyruk satırı | ✓ | `Application_HasNoEventPublisher` + ihlal örneği; `src/`'deki `SaveChangesInterceptor`'lar yalnız kayıt bekçisi (dağıtım yok); kuyruk satırını yazan ilk kod T220 (K1 → D-72) |
| 7 | Mimari test iç türe ve altyapıdan modül türüne başvuruda kırılır | ✓ | `ForbiddenInfrastructure` kırar, örnekteki `AppDbContext` adla dışarıda; göç anlık görüntüsü varlığa ad dizgisiyle başvurur (`modelBuilder.Entity("Shopfolio.Modules…")`), `Migrations` istisnası gerekmedi — `09 §6` madde 2'nin koşullu düzeltmesi tetiklenmez |
| 8 | Sayfa bağlama dokunmaz | ✓ | `PageRule` + ihlal `ForbiddenPage`/`ForbiddenPageEntity`; bugün `Pages/` boş (K2 → D-73) |
| 9 | `Program.cs` modül başına tek kayıt | ✓ | `Program.cs:35-43` dokuz düz çağrı; tarama + ihlal örneği (zincirli, argümanlı, çift, eksik) |
| §25 | Test: yeni yasak kural kasıtlı ihlalle kırmızı | ✗ | **B1:** `Application_HasNoTenantAndNoOperatorPanel` bir yasak ad taramasıdır (`Tenant`/`Operator`), kasıtlı ihlal örneğine karşı koşmaz (`09 §19.2` madde 3, `§6` madde 4) |

Testler (validator, yerel): build `-warnaserror` 0 uyarı · `dotnet format --verify-no-changes` 0 · Unit 366/366 (Consistency dahil) · betik testleri 4/4 · Integration 955/955 (sınıf gruplarıyla bölünmüş koşu) · `restore --locked-mode` 0. Dal CI `41871877761` success (head `33d7551`).

## Altyapı Değişiklikleri

- **Şema/migration:** Yok
- **Config / ortam değişkeni:** Yok
- **Paketleme / dağıtım:** Yok
- **Yeni bağımlılık:** Yok

## Commit & PR

- **Branch:** `task/T218-modul-iskeleti-contracts`
- **Commit:** `T218: Dokuz modülün iskeleti, Contracts ve mimari testler` (head SHA PR açıklamasında ve yardımcıya dönüşte)
- **PR:** #380
- **CI:** yardımcı izler
- **Post-merge run'lar:** *(validator doldurur — merge sonrası)*

## Dış Varsayımlar

Dış varsayım: yok (plan tier, kota, paket sürümü, dış servis yok). Tek ön-uçuş: ArchUnitNET'in bağlam üzerinden (`DbSet<Varlık>` getirici) bağımlılığı yakaladığı sınandı — `ForbiddenContextReader` ihlal örneği kırdı. Başlangıç kapıları (working tree, hook yolu, ana dal CI) yardımcı tarafından F1 gate'inde işletildi; bu işçi temiz `origin/main`'den (`8700785`) başladı.

## Known Limitations / Follow-up

| # | Kalem | Backlog ID | Neden ertelendi |
|---|---|---|---|
| 1 | `09 §6` madde 2'nin "bilgim tarihli" göç/anlık görüntü notu sınandı: anlık görüntü ve tasarım dosyaları varlığa tür olarak değil ad dizgisiyle başvurur; altyapı kuralı `Migrations`'a istisna gerektirmedi. Cümle "T218'de sınandı" diye düzeltilebilir (`09` değişikliği, sahibin onayıyla) | — | 09 L2 değişikliği bu task'ın kapsamında değil |
| 2 | `I<Modül>Module` arayüzleri boştur; üyeler kuralların sahibi task'larla gelir | — | plan böyle |
| 3 | D-21: takma ad kuralını `09 §5.1`'e yazma önerisi | D-21 | sahibin onayı |
