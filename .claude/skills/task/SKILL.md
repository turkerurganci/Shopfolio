---
name: task
description: "Implementation task'ının yapım chat'ini işletir — başlangıç kapıları, referans dokümanlar, kod, test, PR ve 8 maddelik bitiş kapısı. Kullan: 'TXX yap', 'task TXX', /task TXX. Doğrulama ve merge bu skill'in işi DEĞİLDİR."
user-invocable: true
---

# Task — Implementation Yapım Chat'i

> **Ne zaman:** Bir implementation task'ına başlanacağında.
> **Tetikleme:** "TXX yap", "task TXX" veya `/task TXX`.
> **Parametre:** `hedef` — task numarası (örn. `T01`, `T14`, `T63a`, `WP3`)
> **Kural:** Bu skill **yalnız yapım** yapar. Doğrulama ayrı chat'te (`/validate TXX`), merge validator'ın işidir.

---

## Başlangıç kapıları

### Adım -1 — Working tree hygiene check (HARD STOP)

**Neden:** Oturum öncesi uncommitted değişiklikler task dalı açıldığında task PR'ına bundle olur. commit-msg hook bunu **yakalayamaz** — dosya değişiklikleri commit subject'inde TXX referansı oluşturmaz.

```bash
git status --short
```

- **Boş** → Adım 0.
- **Değişiklik var** → **HARD STOP.**
  - Değişiklikleri listele (`git diff --stat` yeterli).
  - Sor: *"Bu uncommitted değişiklikler task'tan önce nasıl ele alınmalı?"*
  - Üç seçenek sun:
    1. **Commit + PR** — ayrı `chore:`/`docs:`/`infra:` dalı, PR, CI izle, sonra task'a dön
    2. **Stash** — `git stash push -m "pre-TXX uncommitted"`, task bitince pop
    3. **Discard** — `git restore <dosya>`; **açık onay zorunlu**, geri dönüşsüz
  - **Kullanıcı kararı olmadan task'a başlama.** *"Sonra hallederiz"*, *"önemsiz"*, *"benim değil"* **yasak**.
- **Untracked dosyalar** da raporlanır (stage / ignore / sil kararını kullanıcı verir).

**Kanıt:** Sonuç rapora yazılır — "Working tree: temiz" veya "Working tree: 3 dosya → chore PR #NN".

**Aynı adımda — hook kurulum kontrolü (HARD STOP):**

```bash
git config core.hooksPath
```

- Çıktı `scripts/git-hooks` ise → devam.
- **Boş veya farklı** ise → **HARD STOP.** `bash scripts/git-hooks/install.sh` çalıştır, tekrar doğrula, sonra devam et.

**Neden her oturumda:** Hook'lar kurulu değilse pre-commit + commit-msg + pre-push'un üç katmanı **sessizce yoktur** — ve bu gözlemlenebilir değildir. Yeni bir klon, bulut oturumu, geçici konteyner veya `git config --unset` sonrası koruma sıfırdır ama hiçbir şey uyarı vermez. Bu, playbook'un doğuş dersinin (kırık CI'nin 5 task boyunca sessizce sürmesi) birebir aynı şeklidir. Kurulum tek seferlik bir SETUP adımı **değildir**, oturum kapısıdır.

### Adım 0 — Ana dal CI startup check (HARD STOP)

**Neden:** Bir referans projede ana dalın CI'ı 5 task üst üste kırık kaldı; 7'den fazla merge kırmızı CI üzerine indi ve "CI PASS zorunlu" disiplini fiilen düştü.

```bash
gh run list --branch main --limit 3 --json databaseId,conclusion,status,displayTitle,createdAt
```

- Üç **tamamlanmış** run'ın hepsi `success` → devam.
- Biri bile `failure` / `cancelled` / `timed_out` / `action_required` → **HARD STOP.** Sebebini sor; root cause çözülmeden (ayrı fix PR veya BLOCKED kaydı) başlama.
- `conclusion` boş olanlar (`in_progress`/`queued`) sayılmaz; gerekirse `--limit` artır.
- `gh` yok veya auth başarısız → kullanıcıya bildir, manuel doğrulama iste. **Varsayımla ilerleme.**

**Yasak rasyonelizasyonlar:** *"lokal temiz"*, *"benim task'ımla ilgisiz"*, *"zaten biliyordum"*, *"bu sefer küçük değişiklik"*.

**Kanıt:** 3 run ID + conclusion rapora yazılır. Retrospektif denetim için zorunlu audit trail.

---

## Hazırlık

1. **Task tanımını oku** — `Docs/11_IMPLEMENTATION_PLAN.md`'den `hedef` task'ın: adı, bağımlılıkları, doküman referansları, kabul kriterleri, test beklentisi, doğrulama kontrol listesi.

2. **Bağımlılık kontrolü** — `Docs/IMPLEMENTATION_STATUS.md`'de bağımlı task'lar `✓ Tamamlandı` mı? Değilse proje sahibine bildir, task başlatılmaz.

3. **Doküman referanslarını oku** — yalnız belirtilen bölümleri. Tüm dokümanı yükleme.

4. **Dış varsayım ön-uçuş kontrolü** (kanıt zorunlu)

   Kod yazmadan ve scope'u kesinleştirmeden önce, task'ın dayandığı dış varsayımları listele ve **her birini somut kanıtla doğrula**.

   **4a. Tetikleyici tarama (varsayımı fark etme adımı).** Kabul kriterlerini ve plan metnini şu terimler için tara — biri geçiyorsa orada bir dış varsayım vardır:

   > plan tier · ücretsiz / paid / trial / subscription / Pro / Team / Enterprise · kota · rate limit · API key · onay süreci · dal koruması · zorunlu review · runner / dakika / depolama limiti · paket sürümü · registry · sandbox / test ortamı · bölge kısıtı · SLA

   Kuralın doğduğu vakada sorun kanıt üretememek değil, **varsayımın var olduğunun fark edilmemesiydi**.

   **4b. Doğrula.** Tipik kategoriler:
   - **Plan tier / feature availability** — ücretli mi, hangi plan gerekli
   - **Paket sürüm uyumu** — planda/dokümanda geçen sürüm registry'de gerçekten var mı
   - **Platform/OS farkı** — path, tip eşlemesi, base image, geliştirme ≠ CI ortamı
   - **API/sözleşme varsayımı** — dış servis belgelendiği gibi cevap veriyor mu, auth modeli, şema sürümü
   - **Repo/ortam state'i** — runner kapasitesi, secret'lar tanımlı mı, ortam değişkenleri

   **Her varsayım için bir satır kanıt zorunlu:** komut çıktısı, resmî doküman bağlantısı, manuel test sonucu. *"Sanırım"*, *"muhtemelen"*, *"genelde böyle"* yetersizdir.

   - Hepsi doğrulandı → Adım 5.
   - Biri kırık → **DUR.** Bu scope'u etkileyen bir karardır. Proje sahibine sun: *"Task X, Y varsayımına dayanıyor; Y kırık; seçenekler A/B/C."* Gerekirse BLOCKED (`EXTERNAL_BLOCKER` / `PLAN_CORRECTION_REQUIRED`).
   - *"Bu task'ta dış varsayım yok"* geçerli bir sonuçtur — ama rapora **açıkça** yaz. Audit trail'in eksikliği ≠ varsayımın yokluğu.

   **4c. Zaman kutusu.** Bir varsayımın doğrulanması **~5 dakikalık** bir iştir. Daha uzun sürecekse (derin doküman okuma, ücretli kayıt, onay bekleyen erişim) **dur ve sor:** *"Bu varsayımı doğrulamam ~X dakika alacak, devam edeyim mi?"* Kaçış yolu olmayan kapı atlanır — üçüncü yol tanımlı olmazsa ajanın önünde yalnız tavşan deliği veya rasyonelizasyon kalır.

5. **Scope netleştirme** — proje sahibine sun:
   - Etkilenen modüller / dosyalar
   - Beklenen çıktı / artifact listesi
   - "Bu task bittiğinde sistemde tam olarak ne değişmiş olacak"

6. **Ana dalı güncelle** — `git checkout main && git pull origin main`. Eski ana dal üzerinden açılan branch merge conflict riski doğurur.

7. **Branch aç** — `task/TXX-kisa-aciklama`.

---

## Yapım

8. **Kodu yaz** — kabul kriterlerini tek tek karşılayacak şekilde.
   - `Docs/09_CODING_GUIDELINES.md` standartlarına uy.
   - Dokümanlarla çelişki fark edersen → **DURMA**, bildir (BLOCKED akışı).
   - Varsayımla ilerleme, doğaçlama yapma.
   - Küçük diff üret; istenmeyen refactor ekleme.

9. **Testleri yaz ve çalıştır** — task tanımındaki test beklentisine göre. Hepsinin geçtiğini doğrula.

10. **Build kontrolü** — tüm bileşenler temiz build vermeli (`.github/ci-commands.env`'deki komutların lokal karşılıkları).

11. **Mini güvenlik kontrolü** — secret sızıntısı · auth/authorization etkisi · input validation etkisi · yeni dış bağımlılık.

12. **Kabul kriterleri self-check** — her kriteri tek tek gözden geçir; karşılanmayan varsa tamamla.

---

## Tamamlama

13. **Commit ve push** — commit subject'i `TXX: ...` ile başlar (commit-msg hook kontrol eder).

14. **Rapor taslağı** — `Docs/TASK_REPORTS/TXX_REPORT.md`, şablon: [`_TEMPLATE.md`](../../../Docs/TASK_REPORTS/_TEMPLATE.md).
    Startup check sonuçları, dış varsayım listesi ve working tree kararı "Notlar" bölümüne girer.

15. **Status güncelle** — `Docs/IMPLEMENTATION_STATUS.md`'de task `⏳ Devam ediyor`.
    **`✓ Tamamlandı` yapma** — bu doğrulama chat'inin işidir.

16. **Repo memory güncelle** — `.claude/memory/MEMORY.md` "Güncel Durum" bloğuna TXX için 1–2 satır (commit hash + PR no + tek cümle çıktı).

17. **Rapor + status + memory commit ve push.**
    **Neden:** Bulut oturumlarında commit edilmeyen dosyalar oturum kapanınca kaybolur; ayrıca validator'ın dalda görmesi gerekir.

18. **PR aç** — `gh pr create --base main --title "TXX: ..." --body "..."`. PR açıklaması `.github/pull_request_template.md`'yi doldurur.

19. **CI'yi izle** — `gh run watch <ID> --exit-status` (arka planda başlatılabilir). **Sen izlersin**; kullanıcıya sorma. Aynı ref'e bağlı **tüm** workflow'ların sonucu beklenir.

20. **Proje sahibine bildir** — *"TXX yapım bitti, CI yeşil, PR #NN açık. Doğrulama için yeni chat açılabilir."*
    **PR'ı merge etme** — merge validator'ın işidir.

---

## Bitiş kapısı

Aşağıdaki **sekiz maddenin hepsi ✓** olmadan task "yapım bitti" sayılmaz ve doğrulama chat'ine geçilmez:

- [ ] **Branch push edildi** — `git push -u origin task/TXX-*` başarılı
- [ ] **PR açıldı** — PR numarası geri geldi
- [ ] **PR numarası rapora yazıldı** — "Commit & PR" bölümünde `PR: #NN`
- [ ] **Rapor + status + memory push edildi**
- [ ] **CI run tamamlandı** — `concluded`. *"Başladı, sonucu bekleniyor"* **yeterli değildir**
- [ ] **CI run sonucu `success`** — aksi hâlde root cause çözülür, yeniden push, CI tekrar beklenir. Validator'a kırık CI ile geçmek yasaktır
- [ ] **Branch izolasyon check temiz:**
  ```bash
  git log main..HEAD --format='%s' | grep -oE '^T[0-9]+(\.[0-9]+)?[a-z]?' | sort -u
  ```
  Çıktıda **yalnız kendi TXX'in** görünmeli. Başka TXX varsa **bundled-PR ihlali** — `git rebase -i main` ile kaldır veya ayrı dala `cherry-pick` et. (commit-msg ve pre-push Layer 3 hook'ları aynı kontrolü otomatik yapar; bu manuel check hook yoksa son savunmadır.)
- [ ] **Repo memory'de TXX satırı var** — yoksa validator Adım 0b'de BLOCKED verir

**Otomatik BLOCKED trigger:** Raporda "PR: Henüz oluşturulmadı", "PR: TBD", "PR: —" veya boş PR alanı görülürse **otomatik BLOCKED** (`DEPENDENCY_MISMATCH`) — yapım chat'i açık kalır, sonraki task'a geçilmez.

**Concurrency notu:** Hızlı art arda push'ta yeni run öncekini iptal edebilir; bu `failure` sayılmaz — `gh run list --branch task/TXX-* --limit 5` ile **son tamamlanmış** run'a bak.

**Bundled PR yasağı:** Başka bir task'ın PR'ına "tek commit daha ne olur" diyerek gömmek yasaktır. Küçük görünen düzeltmeler bile ayrı PR ister. Tek istisna: aynı TXX'in düzeltmeleri aynı dala.

---

## BLOCKED durumunda

1. Çalışmayı durdur.
2. Alt türü belirle: `SPEC_GAP` / `DEPENDENCY_MISMATCH` / `PLAN_CORRECTION_REQUIRED` / `EXTERNAL_BLOCKER`.
3. `Docs/TASK_REPORTS/TXX_REPORT.md`'yi [`_TEMPLATE_BLOCKED.md`](../../../Docs/TASK_REPORTS/_TEMPLATE_BLOCKED.md) şablonuyla oluştur.
4. `Docs/IMPLEMENTATION_STATUS.md`'de durumu `⛔ BLOCKED` yap.
5. Proje sahibine sun: neden, etki analizi, çözüm önerileri.
