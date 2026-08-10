# <PROJE> — CI/CD Setup Kılavuzu

**Son güncelleme:** YYYY-AA-GG

> Workflow dosyaları sürüm kontrolündedir; **repo ayarları, secret'lar ve dal koruma kuralları platform arayüzünden/API'sinden manuel yapılır.** Bu doküman o manuel kısmın kaydıdır.

---

## 1. Workflow envanteri

| Dosya | Ne yapar | Tetikleyici |
|---|---|---|
| `.github/workflows/ci.yml` | 0. Guard → 1. Lint → 2. Build → 3. Unit → 4. Integration → 5. Contract → 6. Migration dry-run → 7. Paketleme → 8. E2E (advisory) → **CI Gate** | `pull_request` + `push` (main) |
| `.github/workflows/publish.yml` | Ana dala merge sonrası artifact yayınlama | `push` (main) + manuel |
| `.github/ci-commands.env` | Teknoloji-spesifik komutlar (SETUP.md §4) | — |
| `.github/ci-run.sh` | Adım çalıştırıcı + eksik komut kapısı | — |
| `.github/pull_request_template.md` | PR şablonu | PR açılınca |

### Job bağımlılıkları

```
lint → build → { unit, integration, contract, migration-dry-run, package, e2e }
guard (yalnız push) ─┐
                     ├─► CI Gate  (tek toplu kapı — dal korumasında required)
tüm zorunlu job'lar ─┘
```

**E2E advisory'dir:** `continue-on-error` + `ci-gate.needs` dışında. Buna karşılık E2E **harness kodu** lint/build adımlarında bloke-edicidir — harness çürümez, yalnız çalıştırma advisory'dir.

---

## 2. Eksik komut kapısı (önemli)

`.github/ci-commands.env` içindeki `SETUP_COMPLETE` bayrağı:

| Değer | Boş komut davranışı |
|---|---|
| `false` (kurulum dönemi) | ⚠ Uyarı + adım atlanır |
| `true` (kurulum bitti) | ✗ **FAIL** |

**Neden:** Hiçbir şey çalıştırmayan yeşil bir pipeline, bu playbook'un önlemek için var olduğu "yanıltıcı yeşil CI" defektinin ta kendisidir. Bir katman bu projede gerçekten yoksa komut yerine `NOT_APPLICABLE` yazılır — böylece "yok" kararı **açık** olur, sessiz kalmaz.

---

## 3. İlk kurulum adımları

### 3.1 Repo ayarları

- Actions izinleri: yayınlama workflow'u kullanılacaksa yazma izni açılır
- Fork PR'larının workflow çalıştırması: **kapalı** (secret sızıntısı önlemi)
- Merge stratejisi: **yalnız squash** · merge commit ve rebase kapalı
- PR başlığı/açıklaması varsayılan commit mesajı olarak kullanılır
- Merge sonrası dal otomatik silinir

### 3.2 Secret'lar

| Secret | Ne için | Hangi task'ta gerekli olur |
|---|---|---|

### 3.3 Dal koruma rejimi

> **SETUP.md §3'te seçilen rejim buraya yazılır.**

**Seçenek A — Discipline-only** (platform dal koruması yok/ücretli):
- Lokal `scripts/git-hooks/` (3 katman)
- CI `0. Guard` job'u (server-side görünür uyarı)
- Manuel disiplin: her task → dal → PR → CI yeşil → validator PASS → squash merge
- Bu rejimin sınırı: **hook kurulmamış bir klonda koruma yoktur.** `install.sh` onboarding'in ilk adımıdır.

**Seçenek B — Sistem-enforced** (platform dal koruması aktif):

| Ayar | Değer | Gerekçe |
|---|---|---|
| PR zorunlu | ✅ | Direct push yasağı |
| Gerekli onay sayısı | **0** | Tek geliştirici + **validator chat ikinci gözdür**; 1 yapılırsa akış tıkanır. İleride işbirlikçi gelirse tek komutla artırılır. |
| Status check zorunlu | ✅ `CI Gate` | Tek toplu kapı |
| Dal güncel olmalı (strict) | ✅ | Stale merge engeli |
| Doğrusal geçmiş | ✅ | Squash ile uyumlu |
| Force push / silme | ❌ | Geçmiş koruması |

> **Not:** Sistem-enforced rejimde bile hook'lar **kalır** — bundled-PR ve kırık-CI kontrolleri platform korumasında yoktur.

> **Dış varsayım uyarısı:** Dal koruma özelliğinin mevcut plan/görünürlük kombinasyonunda kullanılabilir olduğu **kurulumda kanıtla doğrulanır** (00 §F.3). Bir referans projede bu varsayım implementasyon ortasında 403 ile çöktü ve rejim değişikliğine yol açtı.

---

## 4. Operasyonel notlar

### 4.1 Çalışma süresi

| Job | Tahmini süre |
|---|---|

### 4.2 Test filtreleme

> **Ne yazılır:** Unit / integration / contract ayrımının nasıl yapıldığı (namespace, etiket, klasör).

### 4.3 Eşzamanlılık

Aynı ref'e hızlı art arda push atılırsa yeni run öncekini iptal eder. **İptal edilen run `failure` sayılmaz** — son tamamlanmış run'a bakılır (hook Layer 2 ve bitiş kapısı bu ayrımı yapar).

---

## 5. Bakım

- Workflow değişiklikleri normal PR akışıyla yapılır (`infra:` veya `chore:` prefix'i, ayrı PR).
- Action sürümleri major sürümde sabitlenir.
- Yeni bir test katmanı eklendiğinde: `ci.yml` job'u + `ci-commands.env` anahtarı + `ci-gate.needs` listesi **birlikte** güncellenir.

## 6. Follow-up

| # | Konu | Hangi task | Ne yapılacak |
|---|---|---|---|
