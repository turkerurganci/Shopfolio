# Shopfolio — CI/CD Setup Kılavuzu

**Son güncelleme:** 2026-08-11

> Workflow dosyaları sürüm kontrolündedir; **repo ayarları, secret'lar ve dal koruma kuralları platform arayüzünden/API'sinden manuel yapılır.** Bu doküman o manuel kısmın kaydıdır.

---

## 1. Workflow envanteri

| Dosya | Ne yapar | Tetikleyici |
|---|---|---|
| `.github/workflows/ci.yml` | 0. Guard → 1. Lint → 2. Build → 3. Unit → 4. Integration → 5. Contract → 6. Migration dry-run → 7. Paketleme → 8. E2E (advisory) → **CI Gate** | `pull_request` (**base filtresi yok**) + `push` (main) |
| `.github/workflows/publish.yml` | Ana dala merge sonrası ve sürüm etiketinde imaj yayınlama; yama etiketinde (`YAMA > 0`) imajdan önce `patch-migration-check` işi göç eklenmişse yayını düşürür (`migration-fix: K-<no>` istisnası; ilk etikette koşmaz) | `push` (main) + `push` etiketi `vX.Y.Z` + manuel |
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

**Uygulanan ayarlar (kurulum, 2026-08-11):**

```
$ gh repo edit turkerurganci/Shopfolio --enable-squash-merge \
    --enable-merge-commit=false --enable-rebase-merge=false --delete-branch-on-merge
$ gh repo view turkerurganci/Shopfolio --json squashMergeAllowed,mergeCommitAllowed,rebaseMergeAllowed,deleteBranchOnMerge
{"deleteBranchOnMerge":true,"mergeCommitAllowed":false,"rebaseMergeAllowed":false,"squashMergeAllowed":true}

$ gh api -X PUT repos/turkerurganci/Shopfolio/actions/permissions/fork-pr-contributor-approval \
    -f approval_policy=all_external_contributors
$ gh api repos/turkerurganci/Shopfolio/actions/permissions/fork-pr-contributor-approval
{"approval_policy":"all_external_contributors"}
```

> Fork PR onayı varsayılan olarak `first_time_contributors` geliyordu — repo public olduğu için bu, tanıdık olmayan her katkıcının workflow'u **otomatik** çalıştırabilmesi demekti. `all_external_contributors` ile dış katkıcıların tamamı manuel onaya bağlandı.
>
> Yayınlama yazma izni **açılmadı**: `publish.yml` henüz kullanılmıyor (`PUBLISH_CMD` boş). İzin, yayınlama gerçekten devreye alınırsa Aşama 4 sonrasında verilir.

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

#### Seçilen rejim: **B — Sistem-enforced** · karar 2026-08-11 (SETUP.md §3)

**Ön-uçuş kanıtı** — plan/görünürlük kombinasyonu ruleset'i destekliyor mu:

```
$ gh repo view turkerurganci/Shopfolio --json visibility,isPrivate,defaultBranchRef
{"defaultBranchRef":{"name":"main"},"isPrivate":false,"visibility":"PUBLIC"}

$ gh api repos/turkerurganci/Shopfolio/rulesets
[]                          <- HTTP 200. 403 YOK: endpoint bu repo icin erisilebilir.
```

**Uygulama kanıtı** — ruleset oluşturuldu ve geri okundu:

```
$ gh api -X POST repos/turkerurganci/Shopfolio/rulesets --input ruleset-main.json
id: 20666567 | name: "main protection (Rejim B)" | target: branch | enforcement: active

$ gh api repos/turkerurganci/Shopfolio/rulesets/20666567
enforcement    : active
bypass_actors  : 0 adet
rules          : deletion, non_fast_forward, required_linear_history,
                 pull_request, required_status_checks
required check : CI Gate   (strict: true)
onay sayisi    : 0
```

| Ayar | Beklenen (§3.3 tablosu) | Gerçekleşen |
|---|---|---|
| PR zorunlu | ✅ | `pull_request` kuralı aktif |
| Gerekli onay sayısı | 0 | 0 |
| Status check zorunlu | `CI Gate` | `CI Gate` |
| Dal güncel olmalı (strict) | ✅ | `strict: true` |
| Doğrusal geçmiş | ✅ | `required_linear_history` |
| Force push / silme | ❌ | `non_fast_forward` + `deletion` |

**Korunan dal:** `main` (`~DEFAULT_BRANCH` koşuluyla — dal yeniden adlandırılırsa koruma birlikte taşınır).

**`bypass_actors` bilinçli olarak boş:** repo sahibi dâhil kimse muaf değil. Admin muafiyeti bırakmak, Rejim B'yi kâğıt üstünde bırakır — hook'u bypass edebilen kişi sunucu tarafını da bypass edebilirdi.

**Hook'lar kaldırılmadı:** Layer 2 (kırık CI üstüne push) ve Layer 3 (bundled PR) kontrollerinin ruleset'te karşılığı yoktur; ikisi yalnız lokal hook'ta yaşar. Her yeni klonda `bash scripts/git-hooks/install.sh` çalıştırılmalıdır.

### 3.4 İmaj paketlerinin görünürlüğü (ilk yayından sonra, bir kez)

İmajlar GitHub Container Registry'ye `publish.yml` ile gider (`05 §8.2.5`, MK-11). Paketin görünürlüğü repodan geçmez: ilk yayın paketi **özel** doğurur, sunucuların kimlik bilgisi olmadan çekebilmesi için **her paket bir kez açık yapılır**. Açık paket yeniden özel yapılamaz (repo özel yapılırsa yeni paket adları ve salt okuma çekme belirteci gerekir — `05 §8.2.5`; K-981).

Adım (ilk ana dal yayını bittikten sonra, repo sahibi; Actions günlüğünde "Publish artifacts" işinin özeti üç paketin özetini yazar):

1. GitHub'da repo sahibinin hesabında **Packages** sekmesini aç; üç paket görünür: `shopfolio-app`, `shopfolio-database`, `shopfolio-backup`.
2. Her paket için **Package settings → Danger Zone → Change visibility → Public**.
3. Doğrula, kimlik bilgisi olmayan bir makineden: `docker logout ghcr.io; docker pull ghcr.io/<sahip>/shopfolio-app:<tam commit kimliği>` (üç paket için).
4. Sonucu `DEPLOY_RUNBOOK` ya da ilgili task raporuna işle. Açılış T09'da yapılır (`DEFERRED_BACKLOG.md` D-01); önce açılırsa CI'ın geri alma sınaması (`migration-dry-run.sh` 4. bölüm) ilk sürüm etiketinden sonra önceki imajı çekebilir.

Not: paket `org.opencontainers.image.source` etiketiyle repoya bağlıdır (`scripts/ci/publish-images.sh`); `GITHUB_TOKEN` yalnız bu repoya bağlı paketlere yazabilir.

---

## 4. Operasyonel notlar

### 4.1 Çalışma süresi

| Job | Tahmini süre |
|---|---|

### 4.2 Test filtreleme

Ayrım proje ve özellik (trait) iledir; komutlar `.github/ci-commands.env`'dedir:

- **Lint:** `UnitTests` projesinde `Consistency=true` özellikli testler (`--filter-trait "Consistency=true"`; eşitlik denetimleri, `UnitTests/Consistency/`).
- **Unit:** aynı proje, `Consistency` özellikli testler dışarıda (`--filter-not-trait "Consistency=true"`); eşitlik denetimleri iki kez koşmaz.
- **Integration:** `IntegrationTests` projesi, filtresiz; gerçek PostgreSQL 18 (`SHOPFOLIO_TEST_DATABASE`).
- **Contract:** `NOT_APPLICABLE` (`05 §8.3.8`).

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
