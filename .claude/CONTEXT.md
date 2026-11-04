# Proje Bağlamı ve Klasör Haritası

**Katman:** L3 | **Son güncelleme:** 2026-11-02

---

## 0. Bu dosyanın kuralı — önce oku

**Burada dosya listesi tutulmaz.** Yalnızca **klasör düzeyinde rol** yazılır: "bu klasör neye yarar, içine ne konur, kim günceller".

**Neden:** Dosya-bazlı envanter elle bakım gerektirir ve **kaçınılmaz olarak donar**. Bir referans projede bu dosya ~230 satırlık dosya envanteriyle başladı, ilk 25 task'lık dönemde donup kaldı; sonraki ~90 task'ın çıktıları hiç eklenmedi ve dosya yanıltıcı hâle geldi — hâlâ yer tutuyordu ama artık doğru değildi.

Bir dosyanın nerede olduğunu öğrenmek için dosya sistemine bak. Bir klasörün **ne için** olduğunu öğrenmek için buraya bak.

---

## 1. Proje kimliği

| Alan | Değer |
|---|---|
| Proje adı | Shopfolio |
| Tek cümlelik tanım | Herhangi bir firmanın kendi ürünlerini sergileyip çevrimiçi satabileceği ve kurumsal tanıtımını yapabileceği; üyelik (sipariş vermek için zorunlu değil — K-97), Google ile giriş (K-103), sipariş, ödeme ve sipariş takibi içeren web uygulaması. |
| Mevcut dönem | Implementation |
| Teknoloji yığını | C# / .NET 10 (LTS), ASP.NET Core Razor Pages (tek uygulama, sayfalar sunucuda üretilir), Entity Framework Core, PostgreSQL 18, düz JavaScript ve CSS (çatı ve npm yok); Linux konteynerleri, Compose, Caddy 2, pgBackRest, dosya yedeği aracı (aday restic, §2.2.9); GitHub Actions ve GitHub Container Registry → [`Docs/05_TECHNICAL_ARCHITECTURE.md`](../Docs/05_TECHNICAL_ARCHITECTURE.md) §2 |
| Dal koruma rejimi | **B — sistem-enforced** (ruleset aktif, `CI Gate` required; hook'lar da yerinde) |

---

## 2. Klasör haritası

### Kök

| Yol | Rol | Kim günceller |
|---|---|---|
| `CLAUDE.md` | AI giriş noktası — her oturumda okunacak dosyaların listesi | Onayla (GUARDRAILS §2) |
| `SETUP.md` | 0. gün kurulum checklist'i ve proje parametreleri | Bir kez, kurulumda |
| `README.md` | İnsan giriş noktası | Nadiren |
| `CHANGELOG.md` | Playbook sürüm geçmişi | Playbook sürümü değişince |

### `Docs/` — L2, source of truth

| Yol | Rol | Kim günceller |
|---|---|---|
| `Docs/00_PROJECT_METHODOLOGY.md` | L1 metodoloji — sürecin kendisi | Onayla; öğrenim terfilerinde (00 §K) |
| `Docs/01`–`Docs/12` | Ürün, tasarım, teknik ve plan dokümanları | İlgili aşamada; sonrasında yalnız onaylı düzeltmelerle |
| `Docs/PRODUCT_DISCOVERY_STATUS.md` | Doküman üretim döneminin karar kaydı | Her karar anında |
| `Docs/IMPLEMENTATION_STATUS.md` | **Task durumu için tek otoriter kaynak** | Her task doğrulaması sonrası (validator) |
| `Docs/STATUS_CHANGELOG.md` | Status dosyasının tarihsel arşivi | Status "son güncelleme" bloğu şiştiğinde |
| `Docs/DEFERRED_BACKLOG.md` | Ertelenen her işin tek listesi | Validator ve gate bulguları geldikçe |
| `Docs/PLAYBOOK_FEEDBACK.md` | project-playbook'a geri gidecek öğrenimlerin tek listesi — proje tamamlanınca proje sahibi gönderir (K-649) | Öğrenim doğduğunda; en geç aşama ve faz kapanışının öğrenim adımında |
| `Docs/DEBT_CLOSURE_PLAN.md` | MVP borç kapatma iş paketleri | Borç kapatma döneminde |
| `Docs/BYPASS_LOG.md` | Disiplin bypass kayıtları | **Otomatik** (git hook) |
| `Docs/CI_CD_SETUP.md` | Pipeline ve repo ayarları kılavuzu | CI task'ında + değiştikçe |
| `Docs/DEPLOY_RUNBOOK.md` | Prod ayar/secret listesi ve deploy adımları | Entegrasyon fazından itibaren; kapanışta finalize |

### `Docs/*_REPORTS/` ve runbook'lar — kanıt arşivi

| Yol | Rol | Kim günceller |
|---|---|---|
| `Docs/TASK_REPORTS/` | Task başına bir rapor (`TXX_REPORT.md`); şablonlar `_TEMPLATE*.md` | Yapım chat'i taslak, validator finalize |
| `Docs/CHECKPOINT_REPORTS/` | Aşama checkpoint'leri ve faz gate raporları | Checkpoint / gate chat'i |
| `Docs/AUDIT_REPORTS/` | Doküman audit raporları | `/audit` |
| `Docs/CROSS_REVIEW_REPORTS/` | İkinci-AI cross-review turları | `/cross-review` |
| `Docs/INTEGRATION_RUNBOOKS/` | Dış servis kurulum runbook'ları (servis başına bir dosya) | O servise ilk bağlanan task |

### `.claude/` — L3/L4 ajan katmanı

| Yol | Rol | Kim günceller |
|---|---|---|
| `.claude/INSTRUCTIONS.md` | Oturum davranışı | Öğrenim terfilerinde |
| `.claude/GUARDRAILS.md` | Sınırlar ve yasaklar | Öğrenim terfilerinde |
| `.claude/CONTEXT.md` | Bu dosya — klasör haritası | Yeni **klasör** açıldığında |
| `.claude/skills/` | İş akışı skill'leri — her skill kendi klasöründe `<ad>/SKILL.md` (frontmatter'sız düz `.md` kayıt olmaz) | Öğrenim terfilerinde |
| `.claude/checklists/` | Skill'e dönüşmemiş işletim checklist'leri | Aynı |
| `.claude/memory/` | Proje hafızası + kişisel çalışma tercihleri | Her task/faz kapanışında |
| `.claude/hooks/` | **Opsiyonel** oturum hook'ları (hatırlatıcı, bloklamaz — L5 değil) | Nadiren |
| `.claude/settings.json` | Paylaşılan ajan ayarları — **izlenir** (hook kayıtları + ekip izin listesi) | Hook eklendiğinde |

### `scripts/` ve `.github/` — L5 mekanik zorlama

| Yol | Rol | Kim günceller |
|---|---|---|
| `scripts/git-hooks/` | pre-commit (sır guard'ı) + commit-msg + pre-push (3 katman) + kurulum | Nadiren; değişimi ayrı `hooks:` PR'ı ister |
| `scripts/ci/` | CI adımlarının betikleri (`migration-dry-run.sh`, `patch-migration-check.sh`, `package-build.sh`, `publish-images.sh`); `ci-commands.env` çağırır | Adımın işini değiştiren task |
| `scripts/local-reference/` | Yerel referans kurulumun betikleri (`DEPLOY_RUNBOOK` §K; adlar `t09-*`) | Yerel kurulum reçetesine dokunan task |
| `scripts/doc-scans/` | Doküman dönemi tarama ve denetim betikleri (`11_matris_kapsama.py`, `12_matris_denetle.py` vb.) | Betiğin denetlediği dokümana dokunan PR |
| `scripts/metrics/` | Belirteç ve süreç ölçüm betikleri | Nadiren |
| `.github/workflows/` | CI pipeline + (varsa) yayınlama workflow'u | CI task'ında + pipeline genişledikçe |
| `.github/ci-commands.env` | Teknoloji-spesifik CI komutları (SETUP'ta doldurulur) | Kurulumda + yeni test katmanı eklendiğinde |
| `.github/pull_request_template.md` | PR şablonu | Nadiren |

---

## 3. Kod klasörleri

> Uygulama kodu klasörleri **projeye özeldir**. Kurulumda buraya klasör düzeyinde eklenir — dosya listesi **değil**, yalnız "bu klasör hangi katman/modül".

| Yol | Rol |
|---|---|
| `src/Shopfolio.Web/` | Uygulamanın tamamı — tek proje (.NET 10 Razor Pages); modüller bu projenin klasörüdür (`09 §4.1`) |
| `src/Shopfolio.Web/Modules/<Modül>/` | Her iş modülü için bir klasör ve ad alanı `Shopfolio.Modules.<Modül>` (dokuz klasör: `Catalog`, `Ordering`, `Payment`, `AfterSales`, `Membership`, `CorporateContent`, `Notifications`, `Administration`, `Settings`); türler `Contracts` dışında `internal`; `<Modül>Module.cs` modülün tek `Add<Modül>Module` kaydını taşır, `Program.cs` dokuzunu birer kez çağırır (`09 §6` madde 3) |
| `src/Shopfolio.Web/Modules/<Modül>/Contracts/` | Modülün dışa açık yüzeyi: arayüzü `I<Modül>Module` ve enum'ları (`06 §5`; kilit dosyası `tests/Shopfolio.UnitTests/Locks/`); modül dışı kalıcı enum'lar `Infrastructure/Values/`'ta |
| `src/Shopfolio.Web/Infrastructure/` | Ortak altyapı: zaman kaynağı (`TimeSource`, Türkiye saati `TurkeyTime`), iş günü takvimi (`BusinessCalendar`; resmî tatil listesi `PublicHolidays.txt` gömülü kaynak), kuruşa yuvarlama (`Money`; T227), dosya deposu, önbellek, hata yakalama, kayıt, bağlantı katmanı, tek veritabanı bağlamı ve göçler; veritabanı kurulum ayarları `DatabaseOptions` (`Database:ConnectionString` bağlantı dizisi — değeri tamamen sır dosyasından gelir (parola dahil; `FileSecrets/` dosyayı ayar kaynağı olarak okur) —, `Database:LivenessTimeoutSeconds`; `09 §3.5`, §10.7); oturum süreleri `DatabaseTimeouts` (altı sabit) ve tek kuruluş noktası `DatabaseConnection.Configure`/`Create` ile iki profil `DatabaseSessionProfile` (`Web`, `Maintenance`; T233, `09 §10.7`); `FileSecrets/` sır dosyalarını `/run/secrets`'ten okur (`05 §6.x.1`; beş sır ayarının adı `SecretNames.Settings`); `InstallationSettings/` kurulum ayarı sınıfları (`Site`, `Email`, `LinkKey`, koşullu `Payment` ve `Google`), varsayılanı kodda tasarım değerleri (`RateLimit`, `StatusThreshold`, `Logging:Level`) ve açılış denetimi (`SettingsAudit`, `StartupSettingsCheck`; T226, `05 §8.4.6`); `Jobs/` iş kuyruğunun satırı, nabız ve kesinti kaydı (`09 §13.1`), `Disposal/` imha kaydı, `Backup/` yedeğin durum kaydı (T118) |
| `src/Shopfolio.Web/Pages/` | Razor sayfaları — `Storefront/` (vitrin), `Account/` (müşteri hesabı), `Panel/` (yönetim paneli), `Shared/` (ortak parçalar); sayfa modül klasörüne girmez |
| `src/Shopfolio.Web/Endpoints/` | Makine uçları (sayfa olmayan GET ve POST uçları) |
| `src/Shopfolio.Web/wwwroot/` | Düz JavaScript ve CSS (`css/`, `js/`); derleme hattı ve çatı yok |
| `tests/Shopfolio.UnitTests/` | Birim testleri ve modül sınırlarının mimari testleri; veritabanı gerektirmez. Alt klasörler: `Fakes/` (elle yazılmış sahteler), `Architecture/` (mimari testler — zaman ve para kuralları `TimeAndMoneyRulesTests` dahil —, yasak test kalıbı taraması, göç dışı SQL saat taraması, oturum süresi `SET` biçimleri ve işlem gövdesinde dış çağrı taramaları (`DatabaseSessionSourceScan`, `TransactionBodySourceScan`; T233), ortam değişkeninden sır okuma taraması, deploy dosyalarının sır taraması — Compose `environment` liste ve eşleme biçimi, ayar şablonu —, kasıtlı ihlal örnekleri `ViolationSamples/`), `Consistency/` (doküman–kod eşitlik denetimleri; runbook §B ↔ Compose sır eşitliği dahil), `FileSecrets/` (sır–bileşen eşlemesinin Compose ile eşitliği, `.gitignore` denetimi), `Support/` |
| `tests/Shopfolio.IntegrationTests/` | Gerçek PostgreSQL 18'e karşı entegrasyon testleri. Alt klasörler: `Database/` (şablon ve koleksiyon veritabanı fikstürleri, göç demeti), `Web/` (`WebApplicationFactory` fabrikası), `FileSecrets/` (sır dosyası okuyucusu ve ayar kaynağı; dosya sistemine dokunan testler); `Fakes/` UnitTests'ten bağlantılı derlenir |
| `tests/scripts/` | Yedek betiklerinin betik testleri (`sh`; sahte `restic`/`pgbackrest` ile; ağ ve veritabanı gerekmez); CI Unit adımında `run-script-tests.sh` koşar (`09 §17` madde 7) |
| `.config/` | Yerel .NET araçları (`dotnet-tools.json`: `dotnet-ef`) |
| `deploy/` | İmaj, Compose (`compose.yaml`, `compose.dev.yaml` (e-posta yakalama aracı ve uygulamanın uydurma geliştirme bağlantı dizisi), `compose.install.yaml` — ilk kurulumun veritabanı sırları, `compose.local-reference.yaml` — yalnız yerel referans kurulumun bindirmesi, gerçek sunucuda kullanılmaz), ters vekil (`caddy/Caddyfile`), `release.env` (bir sürümün üç imaj başvurusu), `firewall/` (`DOCKER-USER` çıkış kuralı betiği ve kurulum denemeleri), `backup/` (kova yaşam döngüsü kuralı, anahtar politikası şablonu ve kurulumda koşan kova denemeleri; yedek betikleri imaj klasörlerindedir: `images/database/`, `images/backup/`, `images/shared/`) ve `.env.example` (kurulumun sır olmayan ayar şablonu); uygulama kodu değildir (`09 §4.3`) |

---

## 4. Bu dosya ne zaman güncellenir

- Yeni bir **klasör** açıldığında (yeni dosya eklendiğinde **değil**)
- Bir klasörün rolü veya sahibi değiştiğinde
- Dönem değiştiğinde (§1 tablosu)

`/handoff` skill'i bu üç koşulu kontrol eder.
