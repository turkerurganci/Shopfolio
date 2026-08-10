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

- [ ] Repo adı ve görünürlüğü belirlendi
- [ ] `<PROJE>` yer tutucusu tüm dokümanlarda gerçek proje adıyla değiştirildi
      ```bash
      grep -rl '<PROJE>' Docs/ .claude/ | xargs sed -i 's/<PROJE>/GerçekAd/g'
      ```
- [ ] `.claude/CONTEXT.md` §1 dolduruldu (ad, tek cümlelik tanım, dönem)
- [ ] `.claude/memory/MEMORY.md` "Proje Özeti" dolduruldu

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

- [ ] **Plan / görünürlük kombinasyonu kontrol edildi:** Kullanılan git platformunda, bu repo'nun planı ve görünürlüğüyle dal koruma özelliği kullanılabilir mi?
- [ ] **Kanıt kaydedildi** (komut çıktısı veya resmî doküman bağlantısı) → `Docs/CI_CD_SETUP.md` §3.3

**Rejim seçimi:**

- [ ] **A — Discipline-only** (dal koruması yok/erişilemez): hook'lar + CI guard job + manuel disiplin tek koruma katmanıdır
- [ ] **B — Sistem-enforced** (dal koruması aktif): `CI Gate` required check olarak işaretlenir, hook'lar **yine de kalır**

- [ ] Seçilen rejim `Docs/CI_CD_SETUP.md` §3.3 ve `.claude/CONTEXT.md` §1'e yazıldı

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

- [ ] Hook'lar kuruldu:
      ```bash
      bash scripts/git-hooks/install.sh
      git config core.hooksPath        # beklenen: scripts/git-hooks
      ```
- [ ] Layer 1 test edildi: `git push origin main` → **bloklanmalı**
- [ ] commit-msg test edildi: `task/T99-test` dalında `git commit -m "T01: x"` → **bloklanmalı**
- [ ] **Sır guard'ı test edildi:** içinde `API_KEY=sk_live_abcdef123456` olan bir dosyayı stage'leyip commit → **bloklanmalı**
- [ ] Varsayılanlar değiştirilecekse `scripts/git-hooks/hooks.config` oluşturuldu
      (`PB_PROTECTED_BRANCHES`, `PB_TASK_BRANCH_PREFIX`, `PB_SECRET_PATHS`, `PB_SECRET_KEYS`)
- [ ] **Onboarding notu:** Her yeni klonda `install.sh` çalıştırılmalı — hook kurulmamış bir klonda bu koruma **yoktur**

---

## 6. Repo ayarları

- [ ] Merge stratejisi: **yalnız squash** (merge commit ve rebase kapalı)
- [ ] Merge sonrası dal otomatik silinir
- [ ] Fork PR'larının workflow çalıştırması kapalı
- [ ] Yayınlama workflow'u varsa gerekli yazma izni verildi
- [ ] Gerekli secret'lar tanımlandı ve `Docs/CI_CD_SETUP.md` §3.2'ye yazıldı

---

## 7. Süreç başlangıcı

- [ ] `CLAUDE.md` okundu — ajan giriş noktası doğru dosyaları işaret ediyor
- [ ] `Docs/00_PROJECT_METHODOLOGY.md` proje sahibiyle birlikte gözden geçirildi
- [ ] **İkinci AI erişim yöntemi belirlendi** ve aşağıdaki kayıt tablosuna yazıldı — script / web arayüzü / API. Kalite döngüsünün üç ayağından biri cross-review'dur (`00 §C.5`, *"üçü birbirinin yerine geçmez"*); yöntem atanmazsa ilk dokümanın kalite döngüsü **yarım** kalır.
- [ ] Paylaşılan ajan ayarları gözden geçirildi (`.claude/settings.json` izlenir; `settings.local.json` kişiseldir)
- [ ] `Docs/PRODUCT_DISCOVERY_STATUS.md` açıldı, doküman durumu tablosu hazır
- [ ] İlk oturum `.claude/checklists/document-stage.md` ile başlatıldı (Aşama 1 — Product Discovery)
- [ ] Bu dosyadaki tamamlanan maddeler commit'lendi (`chore: setup`)

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
| Proje adı | | |
| Teknoloji yığını | | |
| Dal koruma rejimi | A / B | |
| Dal koruma kanıtı | | |
| Korunan dallar | | |
| İkinci AI (cross-review) | | |
| `SETUP_COMPLETE=true` tarihi | | |
