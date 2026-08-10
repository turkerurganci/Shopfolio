# Git Hooks — Mekanik Zorlama Katmanı (L5)

Bu hook'lar **iyi niyete bağlı olmayan** savunma katmanıdır. Disiplin kuralları
`.claude/INSTRUCTIONS.md` ve skill'lerde yazılıdır; burası onların `exit 1` veren karşılığıdır.

---

## Hook'lar

| Hook | Ne yapar |
|---|---|
| `pre-commit` | **Sır guard'ı.** Staged içerikte sır dosyası yolu, private key bloğu veya bilinen sır anahtarına gerçek değer ataması varsa **commit'i** bloklar |
| `commit-msg` | `task/TXX-*` dalında yapılan her commit'in subject'i kendi TXX'inden farklı bir TXX referansı içeriyorsa **commit'i** bloklar (bundled-PR'ın birincil savunması) |
| `pre-push` | Üç katman: **(1)** korunan dallara direct push · **(2)** push edilen dalın son CI run'ı başarısızsa · **(3)** dalda yabancı TXX commit'i varsa — push'u bloklar |

> **Sır guard'ı bilinçli olarak jeneriktir.** Domain'e veya sağlayıcıya özgü kalıp içermez — o kalıplar `hooks.config`'e yazılır, hook gövdesine değil. Aşırı hevesli bir tarayıcı yanlış pozitif üretir, yanlış pozitif bypass alışkanlığı doğurur ve guard fiilen ölür.

---

## Kurulum (klon başına bir kez)

```bash
bash scripts/git-hooks/install.sh
```

Bu komut `core.hooksPath`'i bu klasöre yönlendirir ve executable bit'lerini garanti eder.

> **Neden kopyalama değil `core.hooksPath`?** Hook dosyaları sürüm kontrollü kalır, edit'ler anında etkili olur, `.git/hooks/` ile stale kopya sorunu doğmaz.

**Doğrulama:**
```bash
git config core.hooksPath   # beklenen: scripts/git-hooks
```

---

## Konfigürasyon (opsiyonel)

`scripts/git-hooks/hooks.config` dosyası oluşturularak varsayılanlar değiştirilebilir:

```bash
PB_PROTECTED_BRANCHES="main develop"   # varsayılan: main
PB_TASK_BRANCH_PREFIX="task/"          # varsayılan: task/

# Sır guard'ı — projeye özgü ek desenler (varsayılanlara EKLENİR, değiştirmez)
PB_SECRET_PATHS='(^|/)certs/|\.jks$'   # ek sır dosyası yolları (regex)
PB_SECRET_KEYS='(WEBHOOK_SECRET|DSN)'  # ek sır anahtar adları (regex)
```

---

## Test matrisi

| Senaryo | Beklenen |
|---|---|
| `.env` dosyasını stage'leyip commit | ✗ BLOCKED (pre-commit, yol) |
| `.env.example` stage'leyip commit | ✓ PASS (şablon izlenir) |
| İçinde `BEGIN RSA PRIVATE KEY` olan dosya | ✗ BLOCKED (pre-commit, içerik) |
| `API_KEY=<YOUR_KEY_HERE>` satırı | ✓ PASS (placeholder) |
| `API_KEY=sk_live_9f3a...` satırı | ✗ BLOCKED (pre-commit, içerik) |
| `PB_ALLOW_SECRET=1 git commit` | ⚠ WARN + PASS, BYPASS_LOG `[secret]` kaydı |
| `git push origin main` | ✗ BLOCKED (Layer 1) |
| `PB_ALLOW_DIRECT_PUSH=1 git push origin main` | ⚠ WARN + PASS, BYPASS_LOG kaydı |
| `git push origin task/T21-x` (CI yok / son CI success, temiz commit'ler) | ✓ PASS |
| `git push origin task/T21-x` (son CI failure) | ✗ BLOCKED (Layer 2) |
| `git push origin task/T21-x` (dalda bir `T17:` commit'i var) | ✗ BLOCKED (Layer 3) |
| `git commit -m "T17: fix"` — `task/T21-x` üzerinde | ✗ BLOCKED (commit-msg) |
| `git commit -m "T21: add entity"` — `task/T21-x` üzerinde | ✓ PASS |
| `git commit -m "chore: typo"` — `task/T21-x` üzerinde | ✓ PASS (TXX iddiası yok) |
| `gh` yok veya auth yok | ⚠ WARN + PASS (yalnız Layer 2 atlanır; Layer 1 ve 3 aktif) |

---

## Bypass

İki ayrı değişken, iki ayrı katman:

```bash
# Layer 1 + 2 (direct push / kırık CI):
PB_ALLOW_DIRECT_PUSH=1 PB_BYPASS_REASON="aciklama" git push origin main

# Layer 3 + commit-msg (bundled-PR / yabancı TXX):
PB_ALLOW_BUNDLED=1 PB_BYPASS_REASON="aciklama" git commit -m "T17: quick fix"
PB_ALLOW_BUNDLED=1 PB_BYPASS_REASON="aciklama" git push origin task/T21-x

# pre-commit (sır guard'ı):
PB_ALLOW_SECRET=1 PB_BYPASS_REASON="aciklama" git commit -m "..."
```

> **Sır bypass'ı özel bir durumdur.** Diğer bypass'lar disiplin borcudur; bu, gerçek bir sır commit'lendiyse **geri alınamaz** bir olaydır. Yanlış pozitifse desenini daralt; gerçek sırsa **değeri döndür** — geçmişi temizlemek yeterli değildir.

**Kullanmadan iki kez düşün.** Her bypass `Docs/BYPASS_LOG.md`'ye otomatik satır ekler (tarih, kullanıcı, dal, commit, sebep, `[kind]` etiketi). Bypass commit'inden sonraki **ilk normal commit'te** log dosyasındaki değişikliği commit'le.

**Kind etiketleri:** `[direct-push]` (Layer 1) · `[ci-failure]` (Layer 2) · `[bundled-pr]` (Layer 3 veya commit-msg)

---

## Server-side tamamlayıcı

Hook lokal korumadır — kurulu değilse veya bypass edilirse iz bırakmaz. `.github/workflows/ci.yml` içindeki **`0. Guard`** job'u bu boşluğu kapatır:

- Ana dala gelen commit mesajında PR referansı `(#NN)` yoksa job FAIL eder
- Push zaten gerçekleşmiştir (engelleyemez) ama Actions sekmesinde **kırmızı uyarı** görünür
- Acil durum bypass'ı: commit mesajına `[skip-guard]`

---

## Devre dışı bırakma

```bash
git config --unset core.hooksPath
```

Yalnız lokal git config'i değiştirir; hook dosyaları repo'da kalır.

---

## Dal koruma rejimi

`SETUP.md`'de iki rejimden biri seçilir:

- **Discipline-only** — platform tarafında sistem-enforced koruma yok (ücretsiz plan + private repo kombinasyonunda dal koruma genelde ücretli bir özelliktir). Bu hook'lar + manuel disiplin + CI guard job tek koruma katmanıdır.
- **Sistem-enforced** — platform dal koruması aktif. Hook'lar **yine de kalır**: bundled-PR ve kırık-CI kontrolleri platform korumasında yoktur.

Yükseltme yolu ve hedef konfigürasyon: `Docs/CI_CD_SETUP.md`.
