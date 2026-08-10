# Playbook Changelog

Bu template kendi sürümünü taşır. Yeni projelerdeki öğrenimler buraya geri akar (00 §K).

**Sürümleme:**
- **Major** — süreç şeklinin değişmesi (aşama ekleme/çıkarma, kapı kaldırma)
- **Minor** — yeni kural, yeni savunma katmanı, yeni skill
- **Patch** — düzeltme, netleştirme, şablon iyileştirmesi

---

## v1.1.0 — 2026-08-01

**Skill'ler gerçekten çağrılabilir hâle geldi.** README ve `INSTRUCTIONS §10` `/task`, `/validate`, `/gate-check` gibi slash command'ler vaat ediyordu; oysa `.claude/skills/*.md` düz markdown dosyalarıydı — frontmatter yok, `<ad>/SKILL.md` yapısı yok, `.claude/commands/` yok. Ajan bunları yalnızca `CLAUDE.md`'yi okuyup dosyaya gittiği için işletiyordu: **konvansiyon olarak** çalışıyor, **mekanizma olarak** çalışmıyordu. Vaat edilen kapı, ajan `CLAUDE.md`'yi atladığı anda yok oluyordu.

Sekiz skill `<ad>/SKILL.md` yapısına taşındı (`git mv` ile geçmiş korunarak) ve `name` / `description` / `user-invocable: true` frontmatter'ı eklendi. Açıklamalar çift tırnaklı — tırnaksız hâli de ayrıştırılıyor, ama bu ayrıştırıcı hoşgörüsüne bağımlılıktır ve açıklamalar `:` içeriyor.

Minor sürüm: yeni kural veya skill eklenmedi, ama L4 katmanının çalışma mekanizması değişti.

**Ayrıca:**
- README ilk cümlesi "13 doküman aşaması" diyordu; `00 §C.1` tablosunda **10** aşama var, 13 ise *doküman* sayısı (00–12). Yeni gelenin okuduğu ilk esaslı cümleydi.
- `SETUP §8`'in (kurulum sonrası doğrulama) ne zaman işletileceği hiç yazmıyordu. §4 Aşama 4'e ertelenebildiği için §8 de otomatik erteleniyordu, ama bu çıkarım okuyucuya bırakılmıştı — şablonun kendi kuralına göre (GUARDRAILS §5) her adım bir kapıya bağlı olmalı. Tetikleyici tanımlandı: `SETUP_COMPLETE=true` günü, ilk implementation task'ından önce.
- Skill dosya yoluna verilen beş referans yeni yapıya güncellendi.

---

## v1.0.3 — 2026-08-01

**Bypass kaydı yanlış commit'i yazıyordu.** `pre-push` `log_bypass()` fonksiyonu `git rev-parse --short HEAD` kullanıyordu; oysa kaydedilmesi gereken **push edilen ref'in tepesi**. Üzerinde durmadığınız bir dalı push etmek olağandır — topic dalındayken `git push origin main`, ya da yeni bir remote'a bootstrap push'u. O durumda kayıt, kapıyı hiç atlamamış bir commit'i işaret ediyordu.

Bir bypass kaydının tek işi **hangi commit'in kapıyı atladığını** söylemektir; yanlış hash o kaydı işlevsiz bırakır. Döngü zaten stdin'den push edilen OID'i okuyordu (`_local_oid`), yalnız kullanılmıyordu — kullanılır hâle getirildi.

Fonksiyonel olarak doğrulandı (izole bare remote, üç senaryo): topic dalındayken `main` push'u artık `main`'in tepesini kaydediyor · üzerinde durduğunuz dalı push etmek regresyona uğramadı · Layer 3 `bundled-pr` yolunda hem hash doğru hem yabancı TXX notu korunuyor.

Bu defektin kendisi v1.0.2'nin yayın push'unda canlı olarak gözlendi.

---

## v1.0.2 — 2026-08-01

Şablonun kendi üzerinde çalıştırılan doğrulama turu sonrası düzeltme.

**Ulaşılamaz kapı kapatıldı:**
- `SETUP §8` Kontrol 7'nin PASS koşulu (`grep` boş dönmeli) yapısal olarak sağlanamıyordu: taranan 36 satırın 27'si tasarım gereği kalıcıydı — desenin *kendisini tarif eden* SETUP satırları, Dönem 3'te dolan `DEBT_CLOSURE_PLAN`, her task'ta dolan `MEMORY.md`, biçim örnekleri. 0. günde boş dönemeyen bir kapı, esnetilmekten başka bir şey yapamaz (00 §A.2). Muafiyet **dosya bazlı** eklendi ve her muaf dosya bir kapıya bağlandı (GUARDRAILS §5); desen jenerik kaldı — muaf olmayan bir dosyaya eklenen yeni yer tutucu ağa takılmaya devam eder.
- `09_CODING_GUIDELINES` §1'deki düz-yazı örneği yer tutucu biçimindeydi ve taramada yanlış pozitif üretiyordu; kaynağında düzeltildi — dosyayı muaf listesine almak, o dosyanın aşama 8'de dolan **gerçek** yer tutucularını da kör ederdi.

**Damga sürüklenmesi:**
- v1.0.1'de içeriği değişen üç dosyanın sürüm/tarih damgası hareket etmemişti: `00` (+51 satır, hâlâ `v1.0` / `2026-07-27`, footer dahil), `SETUP.md` (`v1.0`, README `v1.0.1` derken), `.claude/CONTEXT.md` (`2026-07-27`). Damganın yalan söylemesi INSTRUCTIONS §7'nin yasakladığı şeyin aynası ve 00 §K'nın öğrenim izini dayandırdığı veri.

**Mekanik katman:**
- `.gitattributes` "executable surface" bloğu `pre-commit`'i kapsamıyordu — hook v1.0.1'de eklendi, `.gitattributes` aynı commit'te düzenlendi ama yalnız bir yorum satırı değişti. `git check-attr` kanıtı: `text: auto` (diğer iki hook `text: set`). Pratik risk dardı (`* text=auto eol=lf` yine normalize eder), beyan edilen savunma üç hook'un birinde uygulanmıyordu.

---

## v1.0.1 — 2026-07-30

Bağımsız doğrulama + ikinci tur adversarial inceleme sonrası düzeltme turu.

**Kapatılan kayıp kurallar:**
- Doğrulama matrisine **Durum** sütunu (`BEKLEMEDE / PASS / FAIL / KABUL EDİLMİŞ RİSK`) ve *"ön koşulu sağlanmayan madde BEKLEMEDE kalır"* giriş kuralı — `BEKLEMEDE` bir sonuç olarak kullanılıyor ama tanımlı değildi, dolayısıyla final gate'in *"hiçbir madde beklemede olamaz"* şartı mekanik olarak cevaplanamıyordu
- Matrise **Kanıt ortamı** (mock / sandbox / gerçek) alanı — mock'la ilgili iki sonuç kuralı vardı, ikisinin de dayandığı veriyi üreten kural yoktu
- `.gitignore` sır varsayılanları (`.env` / `.env.*` / `!.env.example`) ve ajan runtime artifact'ı
- Implementation planı traceability matrisine **geri izlenebilirlik** (task → kaynak öğe): kaynağı olmayan task = gerekçesiz iş

**Yeni savunma katmanları:**
- **`pre-commit` sır guard'ı** — staged içerikte sır dosyası yolu, private key bloğu veya bilinen anahtara gerçek değer ataması. Bilinçli olarak jenerik; projeye özgü desenler `hooks.config`'te
- **`core.hooksPath` oturum kapısı** — hook'lar kurulu değilse mekanik katmanın tamamı sessizce yoktur; artık her yapım ve doğrulama oturumunun Adım -1'inde kontrol edilir
- **Gerçek konfigürasyonla boot provası** — faz gate'i "servisleri ayağa kaldır" der ama test konfigürasyonuyla tatmin edilebilir; kapanış ritüelinde gerçek dağıtım konfigürasyonu zorunlu (`00 §L.1`)

**Güçlendirmeler:** ön-uçuş kontrolüne tetikleyici tarama listesi + 5 dk zaman kutusu · secret dağıtım yarıçapı (blast radius) + rotation · süreç baseline valfi · seed edilemeyen kayıt sınıfı ve bootstrap script'i · deep-review güvenlik katmanına OWASP çapası · re-validation sonuç kuralları · `.claude/settings.json` + opsiyonel oturum hook'u katmanı.

**Hijyen:** 7 hedefsiz bölüm/yol referansı kapatıldı · `SETUP` doğrulaması jenerik yer-tutucu desenine çevrildi · `SETUP`'a repo-config ve ikinci-AI atama adımları eklendi.

---

## v1.0 — 2026-07-27

İlk sürüm. Gerçek bir projede fikirden MVP'ye kadar fiilen uygulanan yöntemin konsolide edilmiş hâli.

Lisans: MIT.

**İçerik:**
- **L1** Konsolide metodoloji: doküman üretimi (10 aşama) + implementation + **MVP borç kapatma aşaması** + **ertelenmiş işler disiplini** + kapanış ritüeli + numaralandırma konvansiyonları + **öğrenim terfisi**
- **L2** 12 doküman iskeleti + 8 süreç dosyası + rapor şablonları
- **L3** Çalışma talimatları, guardrail'ler, klasör düzeyi bağlam haritası
- **L4** 8 skill (task · validate · gate-check · audit · deep-review · cross-review · checkpoint · handoff) + doküman aşaması checklist'i + hafıza katmanı
- **L5** Üç katmanlı git hook'ları + CI iskeleti + eksik komut kapısı + PR şablonu

**Kaynak yöntemden düzeltilerek taşınanlar:**
- Ölü prompt kütüphanesi placeholder'ı **taşınmadı**
- Dosya-bazlı bağlam envanteri → **klasör düzeyi harita** (dosya envanteri kaçınılmaz olarak donar)
- Doküman üretim aşamaları için yeni skill icat edilmedi → **checklist** (skill'e dönüştürme ilk gerçek kullanımdan sonra)
- Öğrenim → kural terfisi elle ve tesadüfi olmaktan çıkarıldı → **gate maddesi**

**Kaynak yöntemde geç doğup burada gün 0'da bulunanlar:**
Başlangıç kapıları (working tree / ana dal CI / hafıza drift) · üç katmanlı bundled-PR önlemesi · pre-push CI guard · bypass log · server-side guard job · dış varsayım ön-uçuş kontrolü · 8 maddelik bitiş kapısı · post-merge çok-workflow izleme · CI izleme sorumluluğunun ajanda olması · status/changelog ayrımı · ertelenmiş işler backlog'u · MVP borç kapatma aşaması · runbook katmanı · validator merge yetkisi.
