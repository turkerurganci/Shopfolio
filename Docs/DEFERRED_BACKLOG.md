# Shopfolio — Ertelenmiş İşler Backlog'u

**Son güncelleme:** 2026-11-03 | **Aktif kalem:** 10 | **Gate'i bloklayan:** 1

> **Amaç:** Bilinçli olarak ertelenen **her** somut işin tek izlenebilir listesi.
>
> **Neden gerekli:** Validator ve gate bulgularının bir kısmı "gerçek ama şimdi değil" kararıyla kapanır. Bu karar yalnızca task raporunun içinde kalırsa **buharlaşır** — yüz rapor sonra kimse o satırı bulamaz.
>
> **Kritik ayrım:**
> - **Ertelenen iş** → buraya. MVP kapsamında ama sonraya bırakıldı.
> - **Kapsam dışı** → [`10_MVP_SCOPE.md`](10_MVP_SCOPE.md) §3. Erteleme değil, MVP tanımı gereği hariç.
> - **Tamamlanan iş** → [`IMPLEMENTATION_STATUS.md`](IMPLEMENTATION_STATUS.md).
>
> Bu ayrım borç kapatma aşamasında (00 §I) hayati önem kazanır: neyin kapatılacağı buradan çıkar.

---

## Lejant

- **Öncelik:** 🔴 high · 🟡 medium · ⚪ low · ✅ çözüldü
- **Tip:** `task` (planlı görev) · `backend-gap` (kod boşluğu) · `k-note` (validator notu) · `code-todo` · `doc-drift` (doküman ↔ kod uyumsuzluğu) · `test-gap` · `bypass`
- **Bloklar mı:** kalemin engellediği şey; "—" = hiçbir şeyi bloklamıyor

---

## Kayıt kuralları

1. **Kanıt zorunlu.** Her kalem bir kod referansı (dosya:satır) veya rapor bağlantısı taşır. Kanıtsız kalem bir süre sonra doğrulanamaz hâle gelir.
2. **Satır silinmez.** Çözülen kalem `✅ Çözüldü → <nerede>` olarak işaretlenir. İzlenebilirlik korunur.
3. **Backlog forward zorunlu.** Validator ve gate'te çıkan bloke-etmeyen her bulgu ya o PR'da kapatılır ya buraya yazılır. **Üçüncü seçenek yoktur.**
4. **Hijyen taraması.** Her faz gate'inde açık kalemler kod düzeyinde yeniden doğrulanır — bir kısmı araya giren işlerle çözülmüş, bir kısmı stale olmuş olur (açıklama artık gerçeği yansıtmıyor).

---

## 🔝 Öne çıkanlar (dikkat gerektiren)

| Önc. | ID | Özet | Bloklar mı |
|---|---|---|---|
| 🔴 | D-01 | SETUP §2 + §4 kurulumda tamamlanmadı — `SETUP_COMPLETE=false` | Dönem 2 girişi (ilk implementation task'ı) |
| ✅ | D-02 | ✅ Çözüldü → K-1505 (`INSTRUCTIONS.md` §3.3) — yardımcı izole doğrulama ajanını açar, PASS + CI yeşilse merge eder | — |

---

## 1. Kurulum (SETUP.md)

| Önc. | ID | Açıklama | Tip | Hedef | Kanıt / kaynak |
|---|---|---|---|---|---|
| 🔴 | D-01 | **SETUP §2 + §4 ertelendi.** Doldurulmayanlar: (a) §2 — teknoloji yığını, `.gitignore`/`.gitattributes` yığın uyarlaması, projeye özgü sır dosyası desenleri, `.env.example`; (b) §4 — `LINT_CMD` (`Consistency` özelliğiyle işaretli beş eşitlik denetimi Lint'te koşar, Unit'te dışarıda kalır — `09 §21.4` madde 1; K-1356), `BUILD_CMD`, `UNIT_TEST_CMD`, `INTEGRATION_TEST_CMD`, `CONTRACT_TEST_CMD`, `MIGRATION_DRYRUN_CMD`, `PACKAGE_BUILD_CMD`, `E2E_CMD`, `ci.yml` runtime kurulum adımları, `publish.yml` silinsin mi kararı (silinmezse: etiket imaja basılmadan önce "yama sürümü göç taşımaz" denetimi — `09 §21.4` madde 2; K-1356), imaj paketlerinin ilk yayından sonra bir kez açık yapılması (Teknik Mimari `05 §8.2.5`; K-981); (c) doğrulanacaklar (bilgim tarihli; `05 §2.3.8`) — yedek araçlarının güncel nesneyi silme yetkisi (`05 §7.6.10`; K-961), pgBackRest'in resmî konteyner imajı (`05 §8.2.2`; K-937), veritabanının işlemlerin kesinleşme anını tutması (`05 §8.5.11`; K-962), paketlerin görünürlüğü (`05 §8.2.5`; K-981), konteyner trafiğinin güvenlik duvarındaki zinciri ve pgBackRest'in yerel soket bağlantısı (`05 §3.2.5`; K-987); ardından `SETUP_COMPLETE=true`. **Gerekçe:** yığın Aşama 4'ün kararıdır (SETUP §2 uyarısı) — kurulumda seçmek, teknik kararı doküman aşamasından kaçırmak olurdu. **Sonuç:** CI iskeleti şu an yeşil ama hiçbir şey çalıştırmıyor; §8 kontrol 1 ve 2 bu kalem kapanana kadar geçmez. | `task` | Aşama 4 kapanışından sonra, **ilk implementation task'ından önce** | [`.github/ci-commands.env:19`](../.github/ci-commands.env#L19) (`SETUP_COMPLETE=false`) · [`SETUP.md`](../SETUP.md) §2 uyarısı, §4, §8 · **Aşama 4'ün kararları** (karar kaydı §2; Teknik Mimari `05 §2.3` — D-01'in her parçasına verilenler, yazım turunun 1. oturumu, v0.2 · §8.3 — yazım turunun 2. oturumu): yığın K-866, K-867 · paketleme ve kayıt defteri K-920 · CI adımlarının işi, `publish.yml`'in kalması ve sürüm etiketi K-921 · `.env.example` K-910 · `CONTEXT.md` §3'ün kod klasörleri K-932 (Kodlama Kılavuzu ile) · veritabanı imajı ve yedek aracı imajı K-937 · CI'ın göç adımı K-965 — Teknik mimari aşamasının kapanışının çakışma taramasında tamamlandı (`CHECKPOINT_REPORTS/PHASE4_CONFLICT_SCAN.md` §6.4) · paketlerin görünürlüğü ve CI'ın geri alma sınaması K-981 · doğrulanacaklar K-961, K-962, K-987 — Teknik mimari aşamasının kapanışının checkpoint'inde eklendi (`CHECKPOINT_REPORTS/CP04_PHASE4_CHECKPOINT.md`) |
| ✅ | D-02 | **✅ Çözüldü → K-1505 (2026-11-01; `INSTRUCTIONS.md` §3.1, §3.2, §3.3; `skills/validate`, `skills/task`; genel `SubAgentIndependentValidation` skill'i §0).** Validator yardımcının açtığı izole doğrulama ajanı olarak koşar (talimat gösterilmez, "başlat" beklenmez), 16. adımda verdict'i rapor dosyasına yazıp biter; yardımcı rapor dosyasında PASS okur, dal CI'ı yeşilse merge eder. Bekçi değişmedi. **Önceki kayıt:** **Yönetici yardımcı düzeninin implementation akışıyla uyumu.** Düzen implementation döneminde de açıktır (K-1504). Düğme açıkken yalnız yardımcı ve ana oturum `gh pr merge` çalıştırabilir; task PR'ını ise validator merge eder (`INSTRUCTIONS.md` §3.2, `skills/validate`). Kurulacak: validator'ın hangi bağlamda koştuğu (ayrı ana oturum ya da yardımcının işçisi) ve buna göre bekçinin merge kuralı — validator işçi olarak koşacaksa PASS sonrası merge'ü yardımcı yapar ya da bekçiye validator istisnası eklenir; yapanın denetlememesi kuralı (`skills/validate`) korunur. Bekçi proje dışıdır (`~/.claude/hooks/`, `~/.claude/rules/agent-rules.json` > `helperMode`); değişiklik staging + `deploy.js` ile yayına alınır. **Gerekçe:** doküman döneminde merge'ü yönetici yapıyordu; implementation'da merge eden rol değişiyor ve bekçi bunu bilmiyor. | `task` | Doküman dönemi kapandıktan sonra, **ilk task'ın doğrulamasından önce** | Karar kaydı K-1504 · `INSTRUCTIONS.md` §9 ("Yönetici yardımcı düzeni") · `~/.claude/rules/agent-rules.json` > `helperMode` |

---

## 2. Validator notları

| Önc. | ID | Açıklama | Tip | Hedef | Kanıt / kaynak |
|---|---|---|---|---|---|
| ✅ | D-03 | **✅ Çözüldü → T02 düzeltme turu 1: `Shopfolio.Web.csproj`'a `<RootNamespace>Shopfolio</RootNamespace>` yazıldı; sondaj sayfası `Shopfolio.Pages.Storefront` ad alanıyla derlendi.** Önceki kayıt: `Shopfolio.Web.csproj`'da `<RootNamespace>Shopfolio</RootNamespace>` yok; şu an kök ad alanı `Shopfolio.Web` → ilk Razor sayfası `Shopfolio.Web.Pages.…` alır, `09 §5.1` madde 7 `Shopfolio.Pages.…` ister ("iskeleti kuran task'ta derlenerek sınanır"). `11`'de bu satırı taşıyan başka task yok. | `k-note` | T02 düzeltme turu ya da ilk sayfa/modül klasörünü açan task | `src/Shopfolio.Web/Shopfolio.Web.csproj` · `09 §5.1` madde 7 · `TASK_REPORTS/T02_REPORT.md` (tur 1) |
| ✅ | D-04 | **✅ Çözüldü → T02 düzeltme turu 1: `09 §3.1` madde 4 notuna IDE1006 eklendi (sondajla doğrulandı: yalnız `dotnet format`'ta hata).** Önceki kayıt: IDE1006 (`_camelCase` adlandırma) IDE0008 gibi yalnız `dotnet format`'ta yakalanıyor, `dotnet build`'de değil (sondaj); `.editorconfig`'e `dotnet_diagnostic.IDE1006.severity = warning` eklenince build hata veriyor (sondaj). `09 §3.1` madde 4 notu yalnız IDE0008'i anıyor. | `doc-drift` | T04 (Lint komutu) ya da T02 düzeltme turu | `.editorconfig` · `09 §3.1` madde 4 · `TASK_REPORTS/T02_REPORT.md` (tur 1) |
| ✅ | D-05 | **✅ Çözüldü → T02 düzeltme turu 1: özel statik alan kuralı `_camelCase` yapıldı, `s_camelcase` stili silindi.** Önceki kayıt: `.editorconfig` özel statik alanlara `s_camelCase` öneriyor (şablon artığı; severity `suggestion`) — `09 §3.1` madde 4 "özel alanlar `_camelCase`" ile çelişir. | `k-note` | T02 düzeltme turu ya da T04 | `.editorconfig:251-253`, `:388-391` · `09 §3.1` madde 4 |
| ✅ | D-06 | **✅ Çözüldü → T02 düzeltme turu 1: var olmayan sayfaya işaret eden `UseExceptionHandler("/Error")` bloğu kaldırıldı; hata katmanı `09 §8.1` task'ında yazılır.** Önceki kayıt: `Program.cs` `UseExceptionHandler("/Error")` var olmayan sayfaya işaret ediyor (şablon artığı); hata katmanı `09 §8.1`'in task'ında yeniden yazılır. | `code-todo` | `09 §8.1`'in hata katmanı task'ı | `src/Shopfolio.Web/Program.cs:9` · `09 §8.1` |
| ⚪ | D-07 | **Dependabot PR'ının kilit dosyasını güncellemesi sınanmadı.** `09 §3.2` "ilk task'ta sınanır" der; T02'de depoda `dependabot.yml` yoktur (Dependabot T04'te açılır — `11` T04 KK9). İlk Dependabot PR'ında `packages.lock.json`'un güncellenip güncellenmediği gözlenir; güncellemiyorsa `09 §3.2`'nin yolu (kilit aynı PR'da yeniden üretilir) uygulanır ve sonuç `09 §3.2`'ye yazılır. | `k-note` | T04 ya da ilk Dependabot PR'ı | `09 §3.2` · `TASK_REPORTS/T02_REPORT.md` (Dış Varsayımlar 7; tur 2) |
| ⚪ | D-08 | **`global.json` `rollForward: latestPatch`.** Makinede kurulu en yeni `10.0.4xx` yamasını seçer; `09 §3.2` ".NET SDK sürümü `global.json` ile sabitlenir" der — yerelde ve CI'da farklı yama derleyebilir. CI'ın runtime adımı (`05 §2.3.6`) kurulurken karar: `latestPatch` kalır ve `09 §3.2`'ye yazılır ya da tam sürüm (`disable`/`patch`) seçilir. | `k-note` | T04 (`ci.yml` runtime adımı) | `global.json:3` · `09 §3.2` · `05 §2.3.6` |
| ⚪ | D-09 | **Ad alanı–klasör kuralı Lint'te zorlanmıyor.** `09 §5.1` madde 7 "ad alanı klasör yolunu izler" der; `.editorconfig` `dotnet_style_namespace_match_folder = true:suggestion` — IDE0130 ne `dotnet build`'de ne `dotnet format --verify-no-changes`'te hata verir (sondaj, tur 2: `Infrastructure/` altında `namespace Shopfolio.Web.Infrastructure;` yalnız `--severity info` ile `IDE0130 … expected "Shopfolio.Infrastructure"`). `RootNamespace` doğru çalışıyor; eksik olan zorlama düzeyi. Not: Razor'ın ürettiği sayfa sınıfı `AspNetCoreGeneratedDocument` ad alanındadır, `@model`'deki kısa ad `RootNamespace`'ten çözülmez — `09 §4.1` madde 3 örneği tam adı yazar (uyumlu). | `k-note` | T04 (Lint komutu) | `.editorconfig:68` · `09 §5.1` madde 7 · `09 §4.1` madde 3 |
| ⚪ | D-10 | **`deploy/.env.example` başlığı "Only keys named in the documents are listed" der, ama dokümanda adı geçen kurulum ayarı `Export:TempDirectory` (`09 §10.10` madde 6) dosyada yok.** Başlığın öteki cümlesi ("the task that introduces a setting adds its line") ile tutarlı yol: ayarı getiren dışa aktarma task'ı satırı ekler; başlık cümlesi "yalnız dokümanda adı geçen anahtarlar yazılır; her anahtarı onu getiren task ekler" diye daraltılır. | `doc-drift` | T384 (dışa aktarma) ya da `.env.example`'a dokunan ilk task | `deploy/.env.example:11-12` · `09 §10.10` madde 6 · `09 §4.3` madde 3 |
| ⚪ | D-11 | **T02'de sınanıp farksız çıkan "bilgim tarihli" işaretler `09`'da kaldırılmadı.** `09 §3.2` "T02'de sınandı" diye güncellendi; ama §3.1 madde 2 ("ayar adları ve düzey değerleri bilgim tarihlidir — ilk task'ta … doğrulanır"), §4.1 madde 1 (`.slnx` bilgim tarihli) ve §5.1 madde 7 (`RootNamespace` "iskeleti kuran task'ta derlenerek sınanır") aynı turda sınandı (fark yok; T02 PR #335 "Dış Varsayımlar") ve işaret duruyor — kılavuzda işaretler tutarsız. KK8 yalnız farkın yazılmasını ister; bu kayıt hizalama içindir. | `doc-drift` | `09`'a dokunan sonraki PR ya da F0 gate | `09 §3.1` madde 2, §4.1 madde 1, §5.1 madde 7 · PR #335 |
| ⚪ | D-12 | **Uygulamanın veritabanı bağlantı ayarının adı dokümanda yok.** T03'ün `WebApplicationFactory` fabrikası uygulamanın bağlantı ayarını okumaz; DI'daki `AppDbContext` kaydını koleksiyon veritabanına çevirir (T03'te uygulamada `AppDbContext` kaydı yoktur, kayıt fabrikada eklenir). Bağlantı ayarının adı ve uygulamanın `AppDbContext` kaydı ilk okuyan task'ta (T101 ya da T01) yazılır; o zaman fabrika ayarı değiştirmeye döner ya da kayıt değiştirmesi kalır ve `09 §19.1` madde 5 buna göre güncellenir. | `doc-drift` | T101 / T01 | `tests/Shopfolio.IntegrationTests/Web/ShopfolioWebApplicationFactory.cs` · `09 §19.1` madde 5 · `TASK_REPORTS/T03_REPORT.md` |
| ⚪ | D-13 | **Mimari testin diğer iki kuralı (sayfa ve altyapı) T03'te yazılmadı.** T03 yalnız modül sınırı kuralını (`Contracts` dışına başvuru yok) ve kasıtlı ihlalli derlemeyi kurar; "sayfa modül türlerine/bağlama dokunmaz" ve "ortak altyapı modül türlerine başvurmaz" kuralları T218'dedir (`09 §6` madde 2, `11` T218). | `test-gap` | T218 | `tests/Shopfolio.UnitTests/Architecture/ModuleBoundaryTests.cs` · `09 §6` madde 2 |
| ⚪ | D-14 | **Kütüphane tablosu eşitliği iki yönlü katı değil.** Tabloda henüz eklenmemiş gelecek paketler (Serilog…, HtmlSanitizer, Magick.NET, AWSSDK, Microsoft.IdentityModel*, DataProtection.EFCore) satır olarak durur; katı eşitlik T03'te kırmızı olurdu. Denetim: props ⊆ tablo ve tablo − props == koddaki açık "henüz eklenmemiş" listesi (`LibraryTableConsistency.NotYetAdded`); paketi ekleyen task listeden çıkarır. `09 §3.2` cümlesi "tablo, henüz eklenmemiş paketleri içerebilir; denetim bunları kodda açık bir listeyle izler" diye netleştirilmeli (doküman boşluğu). | `doc-drift` | `09` bir sonraki düzeltme PR'ı ya da T04 (Lint adımı) | `tests/Shopfolio.UnitTests/Consistency/LibraryTableConsistency.cs` · `09 §3.2` · `TASK_REPORTS/T03_REPORT.md` |
| ⚪ | D-15 | **Test koşucusunun telemetrisi açık.** `xunit.v3` geçişli olarak `Microsoft.Testing.Extensions.Telemetry` 1.9.1 ve `Microsoft.ApplicationInsights` 2.23.0 getirir (iki test projesinin kilit dosyası); Microsoft.Testing.Platform telemetrisi varsayılan açıktır (bilgim tarihli). `09 §19.1` madde 6 "dış ağ yoktur" test kodunu sınırlar; koşucunun kendi gönderimi kapsamda yazılı değil. CI komutları bağlanırken `TESTINGPLATFORM_TELEMETRY_OPTOUT=1` (adı bilgim tarihli — sınanır) ortamına konur ya da açık kalması gerekçesiyle `09 §19.1`'e yazılır. | `k-note` | T04 (CI test komutları) | `tests/Shopfolio.UnitTests/packages.lock.json` · `tests/Shopfolio.IntegrationTests/packages.lock.json` · `09 §19.1` madde 6 · T03 doğrulama tur 1 (K1) |

---

## Post-MVP parkı

> MVP kapanışında (00 §L adım 2) buraya taşınan, bilinçli olarak sonraki sürüme bırakılan kalemler.

| Önc. | ID | Açıklama | Tip | Kanıt / kaynak |
|---|---|---|---|---|
