# SETUP — 0. Gün Kurulum Checklist'i

**Playbook sürümü:** v1.1.0

> Bu template "Use this template" ile klonlandıktan sonra **bir kez** işletilir.
> Doldurulan her parametre kalıcıdır; değiştirilmesi rejim değişikliğidir ve proje sahibi onayı gerektirir (GUARDRAILS §2).
>
> **Sıra önemlidir.** §1 → §7 sırayla gidilir; §7 tamamlanmadan ilk doküman aşaması başlatılmaz.
>
> **§8 ne zaman çalışır.** §4 (CI komutları) teknoloji yığınını gerektirir, yığın ise Aşama 4'te belirlenir — bu yüzden §4 ertelenebilir (bkz. §2 uyarısı). §8 doğrulaması §4'ün kapanmasına bağlıdır: **`SETUP_COMPLETE=true` yapıldığı gün** işletilir ve **ilk implementation task'ından (Dönem 2) önce** tamamlanmış olmalıdır. Kurulum, §8 geçmeden bitmiş sayılmaz.

---

## 1. Proje kimliği

- [x] Repo adı ve görünürlüğü belirlendi — `turkerurganci/Shopfolio`, **public**
- [x] `<PROJE>` yer tutucusu tüm dokümanlarda gerçek proje adıyla değiştirildi — 20 dosyada 32 geçiş
      ```bash
      grep -rl '<PROJE>' Docs/ .claude/ | xargs sed -i 's/<PROJE>/GerçekAd/g'
      ```
- [x] `.claude/CONTEXT.md` §1 dolduruldu (ad, tek cümlelik tanım, dönem)
- [x] `.claude/memory/MEMORY.md` "Proje Özeti" dolduruldu

**Dil (değişmez, parametre değil):** Dokümanlar Türkçe · Kod ve kod yorumları İngilizce.

---

## 2. Teknoloji parametreleri

- [ ] Teknoloji yığını belirlendi ve `.claude/CONTEXT.md` §1'e yazıldı
- [ ] `.claude/CONTEXT.md` §3'e **kod klasörleri** klasör düzeyinde eklendi (dosya listesi **değil**)

**Repo konfigürasyon dosyaları yığına uyarlanır** (bunlar template'te bilinçli olarak eksik bırakıldı — yığına bağlıdır):

- [ ] `.gitignore`'a eklendi: bağımlılık klasörleri · derleme çıktısı · log / coverage / test sonucu · IDE artifact'ları
- [ ] `.gitignore`'a eklendi: **projeye özgü sır dosyası desenleri** (sertifika, anahtar dosyası, sağlayıcıya özgü kimlik dosyası)
- [ ] `.gitattributes`'a eklendi: projeye özgü **binary** dosya tipleri
- [ ] `.env.example` şablonu oluşturuldu — **gerçek değerler `.env`'de ve gitignored**; strateji `Docs/05_TECHNICAL_ARCHITECTURE.md §6`'ya yazılacak

> `.env` / `.env.*` / `!.env.example` varsayılanı template ile birlikte gelir; bu adım **yığına özgü** olanları ekler.

> **Uyarı:** Teknoloji seçimi bir *doküman aşaması* kararıdır (`05_TECHNICAL_ARCHITECTURE.md`, aşama 4). Burada yalnızca **CI'ın çalışması için** gereken minimum bilgi kaydedilir. Yığın henüz belli değilse §4'ü aşama 4'ten sonra tamamlayın; o zamana kadar `SETUP_COMPLETE=false` kalır.

---

## 3. Dal koruma rejimi (dış varsayım — KANIT GEREKLİ)

> ⚠️ Bu adım bir **ön-uçuş kontrolüdür** (00 §F.3). Bir referans projede "dal koruması aktifleştirilebilir" varsayımı implementasyonun ortasında 403 ile çöktü ve rejim değişikliğine, birden fazla düzeltme commit'ine ve yeniden doğrulamaya yol açtı.

- [x] **Plan / görünürlük kombinasyonu kontrol edildi:** GitHub, public repo — `gh api repos/turkerurganci/Shopfolio/rulesets` HTTP 200 döndü (403 yok)
- [x] **Kanıt kaydedildi** (komut çıktısı veya resmî doküman bağlantısı) → `Docs/CI_CD_SETUP.md` §3.3

**Rejim seçimi:**

- [ ] **A — Discipline-only** (dal koruması yok/erişilemez): hook'lar + CI guard job + manuel disiplin tek koruma katmanıdır
- [x] **B — Sistem-enforced** (dal koruması aktif): `CI Gate` required check olarak işaretlenir, hook'lar **yine de kalır**

- [x] Seçilen rejim `Docs/CI_CD_SETUP.md` §3.3 ve `.claude/CONTEXT.md` §1'e yazıldı

---

## 4. CI komutları

> Adım **isimleri** ve **kapılar** sabittir (playbook). Yalnız **komutlar** projeye özeldir.

- [ ] `.github/ci-commands.env` dolduruldu:
  - `LINT_CMD`
  - `BUILD_CMD`
  - `UNIT_TEST_CMD`
  - `INTEGRATION_TEST_CMD` *(yoksa `NOT_APPLICABLE`)*
  - `CONTRACT_TEST_CMD` *(yoksa `NOT_APPLICABLE`)*
  - `MIGRATION_DRYRUN_CMD` *(yoksa `NOT_APPLICABLE`)*
  - `PACKAGE_BUILD_CMD` *(yoksa `NOT_APPLICABLE`)*
  - `E2E_CMD` *(uçtan uca faza kadar boş kalabilir)*
- [ ] `.github/workflows/ci.yml` içindeki her job'a **runtime kurulum adımları** eklendi (dil/SDK kurulumu, bağımlılık cache'i)
- [ ] Yayınlanacak artifact **yoksa** `.github/workflows/publish.yml` **silindi**; varsa `PUBLISH_CMD` ve auth adımları dolduruldu
- [ ] Pipeline en az bir kez PR üzerinde çalıştırıldı ve yeşil

> ⚠️ **Bir katman bu projede yoksa boş bırakmayın — `NOT_APPLICABLE` yazın.** Boş bırakılan komut `SETUP_COMPLETE=true` yapıldığında CI'ı FAIL eder. Bu bilinçlidir: hiçbir şey çalıştırmayan yeşil bir pipeline, bu playbook'un önlemek için var olduğu defektin kendisidir.

- [ ] **Son adım (§4 bitince):** `.github/ci-commands.env` içinde `SETUP_COMPLETE=true` yapıldı ve CI hâlâ yeşil

---

## 5. Git hook'ları

- [x] Hook'lar kuruldu:
      ```bash
      bash scripts/git-hooks/install.sh
      git config core.hooksPath        # beklenen: scripts/git-hooks
      ```
- [x] Layer 1 test edildi: `git push origin main` → **bloklanmalı** — `HATA: 'main' dalina direct push engellendi (pre-push Layer 1)` + `error: failed to push some refs`
- [x] commit-msg test edildi: `task/T99-test` dalında `git commit -m "T01: x"` → **bloklanmalı** — `Yabanci TXX: T01`, commit oluşmadı
- [x] **Sır guard'ı test edildi:** aşağıdaki satırı içeren bir dosyayı stage'leyip commit → **bloklanmalı**
      > API_KEY=sk_live_abcdef123456

      > Sonuç: hook `sir anahtarina gercek deger atamasi` diyerek commit'i durdurdu; test dosyası unstage edilip silindi.

      > **Template düzeltmesi (2026-08-11).** Bu madde playbook v1.1.0'da sahte sırrı **checkbox satırının içinde** taşıyordu. Kutuyu işaretlemek satırı değiştirdiği için staged diff sırrı içeriyor ve pre-commit `chore: setup` commit'ini blokluyordu — template'ten doğan her proje §7'de bu kilide çarpar. Değer, hook'un kendi yorum-atlama kuralına (`^[[:space:]]*(#|//|\*|>|\|)` → `continue`, [`pre-commit`](scripts/git-hooks/pre-commit) satır 100) uyan bir blockquote'a taşındı: talimat somut değerini korur, guard zayıflamaz, `BYPASS_LOG` temiz kalır.
      >
      > `PB_SECRET_PATHS` bu iş için kullanılamaz — o bir **muafiyet** listesi değil, ek engel listesidir (satır 69).
- [x] Varsayılanlar değiştirilecekse `scripts/git-hooks/hooks.config` oluşturuldu
      (`PB_PROTECTED_BRANCHES`, `PB_TASK_BRANCH_PREFIX`, `PB_SECRET_PATHS`, `PB_SECRET_KEYS`)
      → **Gerekmedi:** varsayılanlar (`main`, `task/`) bu proje için doğru; dosya bilinçli olarak oluşturulmadı.
- [x] **Onboarding notu:** Her yeni klonda `install.sh` çalıştırılmalı — hook kurulmamış bir klonda bu koruma **yoktur** → `README.md` "Klonladıysanız: ilk iş" bölümüne yazıldı

> **Test artıkları temizlendi:** `task/T99-test` dalı silindi (uzağa hiç push edilmedi), sahte sır dosyası ve test dosyası kaldırıldı. `Docs/BYPASS_LOG.md` boş — üç testin hiçbirinde bypass kullanılmadı.
>
> **Sıralama notu:** Hook'lar bootstrap push'undan **sonra** kuruldu. Ters sırada Layer 1 ilk push'u bloklardı ve `PB_ALLOW_DIRECT_PUSH=1` gün 0'da `BYPASS_LOG`'a gereksiz bir kayıt düşerdi.

---

## 6. Repo ayarları

- [x] Merge stratejisi: **yalnız squash** (merge commit ve rebase kapalı) — `mergeCommitAllowed:false, rebaseMergeAllowed:false, squashMergeAllowed:true`
- [x] Merge sonrası dal otomatik silinir — `deleteBranchOnMerge:true`
- [x] Fork PR'larının workflow çalıştırması kapalı — `approval_policy: all_external_contributors` (varsayılan `first_time_contributors`'dan sıkılaştırıldı)
- [x] Yayınlama workflow'u varsa gerekli yazma izni verildi → **Gerekmedi:** `publish.yml` henüz kullanılmıyor (`PUBLISH_CMD` boş). İzin, yayınlama devreye alınırsa D-01 kapanışında verilir.
- [x] Gerekli gizli değerler tanımlandı ve `Docs/CI_CD_SETUP.md` §3.2'ye yazıldı
      > **Gerekmedi.** Bu aşamada hiçbir workflow secret kullanmıyor. §3.2 tablosu ilk dış servis entegrasyonunda (Aşama 8 / `INTEGRATION_RUNBOOKS`) dolar.
      >
      > **Template düzeltmesi (2026-08-11).** Bu maddenin özgün metni "Gerekli **secret**'lar tanımlandı…" idi ve pre-commit tarafından bloklanıyordu: `KEY_RE` çıplak kelime eşleştirir ([`pre-commit`](scripts/git-hooks/pre-commit) satır 95) ve satırda `=`/`:` yoksa `sed` hiçbir şey kırpmadığı için **tüm satır** "değer" sayılır (satır 97) → uzunluk ≥ 8 → bulgu. Sonuç: içinde `secret` geçen, `#`/`//`/`*`/`>`/`|` ile başlamayan **her** eklenen satır bloklanır. Checkbox satırındaki terim "gizli değerler" olarak değiştirildi; teknik terim bu blockquote'ta yaşamaya devam ediyor (blockquote satırları hook tarafından atlanır, satır 100).

> Ek olarak dal koruma ruleset'i kuruldu (id `20666567`, `enforcement: active`, `bypass_actors: []`) — kanıt `Docs/CI_CD_SETUP.md` §3.3. Ruleset PR'dan **önce** kuruldu; böylece bu kurulumun kendi PR'ı Rejim B'nin çalıştığının kanıtı oldu.

---

## 7. Süreç başlangıcı

- [x] `CLAUDE.md` okundu — ajan giriş noktası doğru dosyaları işaret ediyor (5 hedefin beşi de mevcut)
- [ ] `Docs/00_PROJECT_METHODOLOGY.md` proje sahibiyle birlikte gözden geçirildi
      → **AÇIK.** Proje sahibinin katılımını gerektirir; tek başına işaretlenemez. Aşama 1 oturumunun ilk işi (`PRODUCT_DISCOVERY_STATUS.md` §6).
- [x] **İkinci AI erişim yöntemi belirlendi** ve aşağıdaki kayıt tablosuna yazıldı — script / web arayüzü / API. Kalite döngüsünün üç ayağından biri cross-review'dur (`00 §C.5`, *"üçü birbirinin yerine geçmez"*); yöntem atanmazsa ilk dokümanın kalite döngüsü **yarım** kalır.
- [x] Paylaşılan ajan ayarları gözden geçirildi (`.claude/settings.json` izlenir; `settings.local.json` kişiseldir) — `permissions.allow/deny` ve `hooks` boş; playbook varsayılanı korundu
- [x] `Docs/PRODUCT_DISCOVERY_STATUS.md` açıldı, doküman durumu tablosu hazır
- [ ] İlk oturum `.claude/checklists/document-stage.md` ile başlatıldı (Aşama 1 — Product Discovery)
      → **AÇIK.** Bilinçli: Aşama 1 **ayrı bir chat'te** başlatılır (00 — her iş kendi oturumunda).
- [x] Bu dosyadaki tamamlanan maddeler commit'lendi (`chore: setup`)

---

## 8. Kurulum sonrası doğrulama

| # | Kontrol | Nasıl |
|---|---|---|
| 1 | CI yeşil ve **gerçekten bir şey çalıştırıyor** | PR açıp job loglarına bak — "adım atlandı" uyarısı kalmamalı |
| 2 | `SETUP_COMPLETE=true` | `.github/ci-commands.env` |
| 3 | Hook'lar aktif | `git config core.hooksPath` |
| 4 | Direct push bloklanıyor | `git push origin main` |
| 5 | Guard job çalışıyor | Ana dala bir PR merge edip Actions'ta `0. Guard` sonucuna bak |
| 6 | Sır guard'ı çalışıyor | Sahte bir `API_KEY=...` satırını stage'leyip commit dene → bloklanmalı |
| 7 | **Kurulum yer tutucusu kalmadı** | Aşağıdaki komut boş dönmeli — muafiyet tablosu altta |
| 8 | Tarih yer tutucusu kalmadı | `grep -rn 'YYYY-AA-GG' Docs/ .claude/` — yalnız **şablon** dosyalarında (`_TEMPLATE*.md`) kalmalı |
| 9 | İkinci AI atandı | Aşağıdaki kayıt tablosunda satır dolu |

**Kontrol 7 komutu:**

```bash
grep -rn '`<[^>]*>`' Docs/ .claude/ \
  | grep -vE '_TEMPLATE|IMPLEMENTATION_STATUS|STATUS_CHANGELOG|DEBT_CLOSURE_PLAN|memory/MEMORY|/skills/'
```

> **Desen neden jenerik.** İleride eklenecek her yer tutucuyu otomatik kapsar. Belirli bir dizeyi (`<PROJE>`) aramak, o dizeyi bilmeyen yeni bir yer tutucuyu ağdan kaçırır — O-1 bulgusunun kaynağı buydu.
>
> **Muafiyet neden dosya bazlı.** Muaf dosyaların yer tutucuları kuruluma değil, aşağıdaki kapıya aittir — sahipsiz yer tutucu yasaktır (GUARDRAILS §5). Muafiyet **dosya** bazlıdır, dize bazlı değil: muaf olmayan bir dosyaya eklenen **yeni** bir yer tutucu ağa takılmaya devam eder.
>
> | Muaf | Yer tutucuyu dolduran kapı |
> |---|---|
> | `_TEMPLATE*.md` | Şablon — hiç doldurulmaz |
> | `IMPLEMENTATION_STATUS.md` | İlk task doğrulaması |
> | `STATUS_CHANGELOG.md` | İlk status taşıması (biçim örneği) |
> | `DEBT_CLOSURE_PLAN.md` | Dönem 3 — borç kapatma (00 §I) |
> | `.claude/memory/MEMORY.md` | Her task kapanışı. "Proje Özeti" bloğu bunun istisnası — onu §1'in checkbox'ı kurulumda kapatır |
> | `.claude/skills/*.md` | Çıktı biçimi örnekleri — doldurulmaz |
>
> `SETUP.md` taramaya **dahil değildir**: §1 ve §8'deki `<PROJE>` geçişleri yer tutucu değil, yer tutucuyu *tarif eden* talimattır. Kurulumda temizlenmesi gereken asıl hedef `.claude/CONTEXT.md` §1/§3'tür.

---

## Doldurulan parametreler (kayıt)

| Parametre | Değer | Tarih |
|---|---|---|
| Proje adı | **Shopfolio** — `github.com/turkerurganci/Shopfolio` (public) | 2026-08-11 |
| Teknoloji yığını | *Belirlenmedi* — Aşama 4 kararı (`05_TECHNICAL_ARCHITECTURE.md`); bkz. `DEFERRED_BACKLOG.md` D-01 | — |
| Dal koruma rejimi | **B — sistem-enforced** | 2026-08-11 |
| Dal koruma kanıtı | `gh api repos/turkerurganci/Shopfolio/rulesets` → HTTP 200 (ön-uçuş) · ruleset id `20666567`, `enforcement: active`, `bypass_actors: []` (uygulama) → `Docs/CI_CD_SETUP.md` §3.3 | 2026-08-11 |
| Korunan dallar | `main` (`~DEFAULT_BRANCH`) — sunucu tarafı ruleset + lokal `PB_PROTECTED_BRANCHES` varsayılanı | 2026-08-11 |
| İkinci AI (cross-review) | **`cursor-agent`** (CLI, hesap tabanlı auth — API key kullanılmıyor). Raporlar `Docs/CROSS_REVIEW_REPORTS/` altına yazılır. | 2026-08-11 |
| `SETUP_COMPLETE=true` tarihi | *Yapılmadı* — §4 ertelendi (`DEFERRED_BACKLOG.md` D-01). Aşama 4 kapanışından sonra, ilk implementation task'ından önce. | — |

---

## Kurulum durumu (2026-08-11)

**Tamamlanan:** §1 · §3 · §5 · §6 · §7 (2 madde hariç)
**Ertelenen:** §2 · §4 → `DEFERRED_BACKLOG.md` D-01 (yığın Aşama 4'ün kararı — SETUP §2 uyarısı)
**§7'de açık:** metodolojinin proje sahibiyle gözden geçirilmesi · Aşama 1 oturumunun başlatılması (ayrı chat)
**§8 durumu:** kontrol 3–8 geçti · kontrol 1 ve 2 D-01'e bağlı, henüz geçmedi

Kurulum **§8'in tamamı geçene kadar bitmiş sayılmaz** (bu dosyanın girişi). D-01 kapanmadan ilk implementation task'ı başlatılamaz.
