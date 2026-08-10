# Proje Bağlamı ve Klasör Haritası

**Katman:** L3 | **Son güncelleme:** 2026-08-01

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
| Tek cümlelik tanım | Herhangi bir firmanın kendi ürünlerini sergileyip çevrimiçi satabileceği ve kurumsal tanıtımını yapabileceği; üyelik, Google/Facebook ile giriş, sipariş, ödeme ve sipariş takibi içeren web uygulaması. |
| Mevcut dönem | Doküman üretimi |
| Teknoloji yığını | Aşama 4'te belirlenir → [`Docs/05_TECHNICAL_ARCHITECTURE.md`](../Docs/05_TECHNICAL_ARCHITECTURE.md) |
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
| `.github/workflows/` | CI pipeline + (varsa) yayınlama workflow'u | CI task'ında + pipeline genişledikçe |
| `.github/ci-commands.env` | Teknoloji-spesifik CI komutları (SETUP'ta doldurulur) | Kurulumda + yeni test katmanı eklendiğinde |
| `.github/pull_request_template.md` | PR şablonu | Nadiren |

---

## 3. Kod klasörleri

> Uygulama kodu klasörleri **projeye özeldir**. Kurulumda buraya klasör düzeyinde eklenir — dosya listesi **değil**, yalnız "bu klasör hangi katman/modül".

| Yol | Rol |
|---|---|
| — | Kod klasörü henüz yok. Yığın Aşama 4'te seçilir; ilk klasörler Dönem 2'nin (implementation) ilk task'ında bu tabloya eklenir. |

---

## 4. Bu dosya ne zaman güncellenir

- Yeni bir **klasör** açıldığında (yeni dosya eklendiğinde **değil**)
- Bir klasörün rolü veya sahibi değiştiğinde
- Dönem değiştiğinde (§1 tablosu)

`/handoff` skill'i bu üç koşulu kontrol eder.
