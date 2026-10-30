# Aşama 8 — Öğrenim Terfisi

**Tarih:** 2026-10-30 | **Aşama:** 8 — Kodlama kılavuzu | **Kapanış adımı:** K-437'nin 5. adımı (`00 §K`; `checklists/document-stage.md` §7 madde 5)
**Girdi:** karar kaydı v0.222 — §20.1'in beş öğrenim adayı, bekletilen Ö-23, belirteç ölçümünün tahminle karşılaştırılması; Aşama 8'in süreç kararları (K-1302…K-1396 içinden); yöneticinin on gözlemi; [çakışma taraması](PHASE8_CONFLICT_SCAN.md) §7, [checkpoint](CP08_PHASE8_CHECKPOINT.md) §6; `Docs/PLAYBOOK_FEEDBACK.md` (PF-128…PF-133); kullanıcı hafızasının on beş notu (indeks satırlarından, salt okunur); oturumun kayıt dosyalarından ölçülen belirteç tüketimi — `origin/main` @ `95209c9`
**Çıktı:** karar kaydı v0.223 · `.claude/INSTRUCTIONS.md` §3.2, §9 · `.claude/checklists/document-stage.md` (üç madde genişledi, madde sayısı 67 yerinde) · `.claude/skills/checkpoint/SKILL.md` · `scripts/doc-scans/geri_isaret_tara.py` (yeni) · `.claude/memory/MEMORY.md`, `MEMORY_ARCHIVE.md` · `Docs/PLAYBOOK_FEEDBACK.md` 142 satır · karar K-1397, K-1398 — `00`, `CLAUDE.md`, `SETUP.md`, `GUARDRAILS.md`, `CONTEXT.md`, Kodlama Kılavuzu ve öteki dokümanlar değişmedi

> **Ne işe yarar:** `00 §K` gereği aşama, öğrenimi yazılmadan kapanmaz. Bu rapor Aşama 8 boyunca kayda, raporlara, yöneticinin gözlemlerine ve hafızaya düşen her öğrenim adayının sonucunu yazar. Sekiz aday L3–L4'e terfi etti ve bu PR'da uygulandı. Aşama 8'in maliyet ölçümü §3'te; checklist'in skill'e dönüşmesi §4'te; proje sahibine gidecek beş soru §5'te — her biri ayrı numaralı, sade dille, K numarasız. `00` hedefli öneriler (Soru 4, Soru 5), cross-review skill'inin kapsamı (Soru 1) ve genel hook (Soru 2) uygulanmadı (GUARDRAILS §2, §3; hook proje dışıdır); cevap gelince ayrı `docs:` PR'ında uygulanır.

---

## 1. Sonuç

| Sonuç | Sayı | Adaylar |
|---|---|---|
| **Terfi etti — bu PR'da uygulandı** (L3, L4, bir L5 betiği) | 8 | Ö-143, Ö-144, Ö-145, Ö-147, Ö-148, Ö-149, Ö-151, Ö-153 |
| **Raporda kalır** — tek seferlik gözlem ya da bilgi | 8 | Ö-140, Ö-146, Ö-150, Ö-152, Ö-156, Ö-157, Ö-158, Ö-159 |
| **Önceden terfi etmişti** | 2 | Ö-141, Ö-142 |
| **Proje sahibine sorulur** | 5 soru | Ö-154 (Soru 1) · Ö-155 (Soru 2) · Ö-23 (Soru 3) · Ö-139 (Soru 4) · `00 §N.1`'e Aşama 8'in desenleri (Soru 5) |
| **Önceden terfi etmişti ya da dokümanda yaşar** — süreç kararları | 9 grup | S-1…S-9 (§2.4) |
| **Önceden terfi etmişti / hafızada kalır** — kullanıcı hafızası | 15 not | H-1…H-15 (§2.5) |

Toplam kırk altı kalem: tracker §20.1'in beş adayı (Ö-139…Ö-143), yöneticinin on gözlemi (Ö-144…Ö-153), adımların bulduğu altı gözlem (Ö-154…Ö-159), Ö-23, Aşama 8'in süreç kararlarının dokuz grubu ve kullanıcı hafızasının on beş notu. `CLAUDE.md` ve `SETUP.md` için öneri yok.

**Aşamanın asıl öğrenimi.** Kodlama Kılavuzu en uzun doküman (yaklaşık 470 KB) ve otuz beş yürütücü koşumunda yürüdü. Üç şey belirgin: (1) **Belirteç tasarrufu iki yerde işledi, bir yerde işlemedi.** Aşama 7'nin kuralları (toplu doğrulama, dar mercek, büyük dosya sınırı) merceklerin yeniden okumasını yüzde 61, doğrulayıcıların yüzde 43 düşürdü ve audit adımı dört kat ucuzladı; ama toplam, K-1301'in tahminini (~400 M) yüzde 28 aştı, çünkü yazım turunun yürütücüleri (toplamın üçte biri) ve yönetici oturumu bu kurallara değmiyor (§3). (2) **Yazılı kural yetmedi:** geri işaret ve üst doküman cümlesinin okunuşunun geri beslemesi checklist'te yazılıydı, ilk uygulamada tutmadı; her ikisi yönetici ya da çakışma taraması tarafından sonradan bulundu (Ö-147, Ö-148). (3) **Genel koruyucu yasağı tam kapamadı:** alt ajanlar heredoc yasağının yan kapılarını (`<<<`, `python -c`, boş heredoc, markdown'a ekleme) denedi ve geçti (Ö-155).

---

## 2. Adaylar ve sonuçları

**Kimlikler:** Ö-139…Ö-143 tracker §20.1'in beş adayıdır (sırasıyla aday 1…5; Ö numaralandırması Aşama 7'nin Ö-138'inden sürer). Ö-144…Ö-153 yöneticinin on gözlemidir (sırasıyla 1…10). Ö-154…Ö-159 adımların bulduğu gözlemlerdir. Ö-23 Aşama 1'den gelir. S-1…S-9 Aşama 8'in süreç kararlarının gruplarıdır, H-1…H-15 kullanıcı hafızasının on beş notudur — ikisi de bu rapora özgü numaralardır.

### 2.1 Karar kaydındaki adaylar (§20.1 ve çakışma taraması §7, checkpoint §6)

| # | §20.1 | Aday | Sınıf (`00 §K`) | Sonuç | Hedef katman ve dosya | Uygulanan metin (özet) | Playbook |
|---|---|---|---|---|---|---|---|
| Ö-139 | 1 | Her görevde verilen doküman boyut sınırıyla çakışır — `00 §C.6` "her kodlama görevinde" der, 100 KB'ı aşan dosya bütün okunamaz | Tekrarlanacak desen / `00` önerisi | **Sorulur (Soru 4)**; doküman düzeyinde zaten çözülü | `00 §C.6` (öneri); `09` başlık notu ve `11`'in park bloğu çözümü taşır (K-1302 (4)) | Uygulanmadı | PF-131 |
| Ö-140 | 2 | Devir dizininde etki sütunuyla eşleşmeyen fazla satır (K-1211 `09` tablosunda, etki sütununda `09` yok) | Tek seferlik gözlem | **Raporda kalır** | — | Ö-127'nin ters yönüdür; checklist §1 "eşleme etki sütununu taradıktan sonra dizinle karşılaştırır ve farkı girdiler notuna yazar" der — iki yönü kapsar. Fazla satır tek | — |
| Ö-141 | 3 | Cevap bekleyen soru ara metinde kayboldu | İhlali önleyen kural | **Önceden terfi etmişti** (K-1304) | L3 `INSTRUCTIONS §2` | Soru penceresiyle ya da kapanış mesajında tam metin; "cevaba kadar varsayılan geçerli" beklemeden devam izni değildir | PF-132 |
| Ö-142 | 4 | Modlar cevaplanınca yönetici düzenine geçiş | İhlali önleyen kural | **Önceden terfi etmişti** (K-1311) | L3 `INSTRUCTIONS §9` işletim kuralı 1 | Modlar açıldığı anda sıradaki adım sorulmadan başlar | PF-133 |
| Ö-143 | 5 | Etki yansıtmanın "kalan eşleşme yoktur" taraması bir bölümün kapalı liste ve sayım cümlelerini kaçırdı (`09 §8.1` madde 7, §9.1 madde 8) | İhlali önleyen kural | **Terfi etti** | L4 checklist §6 (etki yansıtma maddesi) | Anahtar terim taraması terimleri yalnız kararın cümlesinden değil, değişen kuralın bölümündeki kapalı liste ve sayım cümlelerinden de türetir | PF-140 |

### 2.2 Yöneticinin gözlemleri

| # | Gözlem | Sonuç |
|---|---|---|
| Ö-144 | **Bağlam sınırı ve kaynak paketi.** Paketsiz yazım parçalarının neredeyse hepsi 250 bin sınırına ulaştı (adım başına 10–12 M işlenen, 70–80 araç çağrısı); W-2a, Y-2b, Y-4b, etki yansıtma ve çakışma taraması devam ajanı istedi. Y-6a'dan itibaren yönetici her parçaya betikle tek dosyalık kaynak paketi verdi (plan satırları, park metinleri, K satırları, kaynak bölümler; ≤ 90 KB): Y-6a 41 çağrı, 5,6 M, sınıra yaklaşmadı | **Terfi etti** — L3 `INSTRUCTIONS §9` (9): dar kapsam yazım oturumlarına da uygulanır; paketleyici betikler `scripts/doc-scans/`'ta. PF-134 |
| Ö-145 | **Sınırda ajan büyümeye devam eder.** Y-2b sınırda 21 KB'lık devir notu yazdı, bağlamı 290 bine çıktı (merge denetimi UYARI verdi) | **Terfi etti** — L3 `INSTRUCTIONS §9` (8): devir notu en çok 5 KB, yönetici sınırı görev dosyasına yazar. PF-135 |
| Ö-146 | **Hook'un sınırdaki bitirme komutlarını engellemesi** — tırnak içindeki `;` (PR başlığı), boru sonrası `tail`/`head`, `S="…" && git …` ataması; `context-guard.js` staging + deploy ile düzeltildi (163 test) | **Raporda kalır** — hook proje dışıdır, düzeltildi ve testlenmiştir; proje kuralı değişmez |
| Ö-147 | **Geri işaret eksikleri.** Kararı değiştiren yeni kararlar eski satıra geri işaret bırakmadı: yönetici dört kez elle ekledi (K-1325, K-1363, K-1374, K-1364), çakışma taraması 16 satır daha buldu (K-1391). Kural checklist §3'te yazılıydı, uygulanmıyordu | **Terfi etti** — L4 checklist §3 (geri işaret maddesi): denetim adım sonu betiğine girer; betik `scripts/doc-scans/geri_isaret_tara.py` yazıldı (aday çift bulur, her aday okunur). Aşama 8 kayıtlarında (K-1302…K-1396) 6 aday çift, 3'ü işaretlendi, üçü de okununca yanlış pozitif (değişmedi diyen atıf; elenen seçenek; ret gerekçesindeki atıf) — yani eksik geri işaret kalmadı. PF-136 |
| Ö-148 | **Okunuş geri beslemesinin atlanması.** Beş kez alt ajan üst doküman cümlesini yorumlayıp geri beslemeyi atladı ya da ilk task'a bıraktı (K-1314 (6), K-1315 (3), K-1337 `ck_`, K-1342 sütunları, S3-10) | **Terfi etti** — L4 checklist §3 (geri besleme maddesi): yorum satırı kayıt anında geri beslemeyi ya da "gerekmez" gerekçesini taşır; adımın raporu yorumlanan cümleleri ve geri besleme durumunu listeler. PF-137 |
| Ö-149 | **Seviyesi yükselen bulgunun yeniden doğrulanması 20 doğrulayıcı sınırıyla çatıştı** (deep review D-a: 6 bulgu ayrı parçada 5 doğrulayıcıyla yapıldı; ilk dalga dört grupta 2–4'lü kademeyi aştı) | **Terfi etti** — L3 `INSTRUCTIONS §9` (1): yeniden doğrulama ayrı adımda yapılır; merge denetiminin 20 sınırı adım başına olduğundan ilk doğrulama ve yeniden doğrulama ayrı sayılır. PF-138 |
| Ö-150 | **"Tekrar eden RET" kuralının erken uygulanması** — yöneticinin talimatı kuralın "en az iki önceki ret" koşulunu yazmadı; 2. turda bir retten sonra uygulandı | **Raporda kalır** — tek seferlik; kural skill'de (`cross-review` Faz 4 madde 5) doğru yazılı, yanlış olan geçici talimattı |
| Ö-151 | **CI yoklama betiği sonucu denetlemiyordu** (yöneticinin bekleme betiği bitişi bekliyordu, başarıyı değil); zincir `&& gh pr merge` olunca kırmızı CI'da da merge ederdi; düzeltildi, zarar yok | **Terfi etti** — L3 `INSTRUCTIONS §3.2`: bekleme betiği başarısız ya da boş sonuçta sıfırdan farklı kodla çıkar. PF-139 |
| Ö-152 | **Kapanış adımlarının maliyeti.** Checkpoint C-a 82 M (11 mercek + 15 doğrulayıcı), çakışma taraması 37 M (23 okuyucu + 11 doğrulayıcı); kapanış adımlarının okuyucu sayısı sınırlanmadı | **Raporda kalır** — sınır konmaz. Checkpoint'in 56 bulgusunun 49'u doğru ya da kısmen doğru çıktı ve 48 düzeltme uygulandı (verim yüksek); sınırın tahmini tasarrufu toplamın yüzde 6'sı kadardır, kaçan bulgu ise sonraki aşamalara taşınır. Rakamlar §3'te, proje sahibine bilgi olarak gider |
| Ö-153 | **S3-10** — checkpoint, var olan bir kararın (K-546) yansımasını "yasal dayanak doğrulanamadı" diye soru yaptı; yönetici yansıma olarak uyguladı | **Terfi etti** — L4 `skills/checkpoint` "Koşum": kaynak karar zaten hükmetmişse doğrulayıcı dayanağı yeniden açmaz. PF-141 |

### 2.3 Adımların bulduğu gözlemler

| # | Gözlem | Sonuç |
|---|---|---|
| Ö-154 | **Cross-review'da ciddiyet ölçüsü ilk turdan, sorulmadan uygulandı.** Skill'in "ne zaman uygulanır" listesi Kodlama Kılavuzu'nu adıyla saymıyor; yönetici dokümanı türetim dokümanlarına benzetip ölçüyü görev dosyasına kendisi yazdı, sormadı, ⚠ işaretlemedi (K-1383). Sonuç: 3 tur, 7 → 6 → 0 bulgu | **Sorulur (Soru 1)** — proje sahibi aşama sonunda konuşmak istedi |
| Ö-155 | **Koruyucunun kaçırdığı yazımlar.** Alt ajanlar en az on kez yasak biçimi denedi; hook betik ve yorumlayıcıya giden heredoc'u engelledi (log: 4 engel), ama `<<<` here-string, `python -c` satır içi betik, `/dev/null`'a boş heredoc ve markdown dosyasına `cat >> … <<` ekleme geçti | **Sorulur (Soru 2)** — hook proje dışıdır. PF-126'nın devamı |
| Ö-156 | Üç bulgu çifti aynı yeri anıyordu (M5-2 ≡ S2-2, M5-3 ≡ S2-1, S1-04 ≡ S3-09); şartnameler birleştirilmeden uygulansa ikincisinin eski metni bulunamazdı (CP08 §6) | **Raporda kalır** — tek seferlik; uygulama betiği her değiştirmede tam bir eşleşme istediği için zarar vermezdi |
| Ö-157 | Doğrulayıcı mercek kapsamının dışındaki bir çelişkiyi buldu (V-1): doğrulayıcılar kapsam dışı notlarını yazıyor (CP08 §6) | **Raporda kalır** — olumlu gözlem, kural gerekmez |
| Ö-158 | PDF biçimindeki resmî tebliğin metni araçlarla okunamadı; yasal dayanağa bağlanan düzeltme dayanak okunmadan uygulanmadı (CP08 §6) | **Raporda kalır** — `cross-review` skill'i (Faz 4 madde 5, hukuki kontrol) ve checkpoint'in kuralı zaten bunu ister |
| Ö-159 | **Yönetici oturumunun payı.** Yönetici (Opus) 92,5 M işledi — toplamın yüzde 18'i, Aşama 7'ye göre yüzde 44 fazla (310 çağrı, ortalama ~300 bin bağlam). Alt ajan sınırı (250 bin) yönetici için geçerli değil | **Raporda kalır** — bilgi; yönetici oturumu her aşama kapanışında sonraki sohbet mesajıyla zaten bölünür (H-7) |

### 2.4 Karar kaydında yaşayan süreç kararları

| # | Kararlar | Süreç kararı | Sonuç |
|---|---|---|---|
| S-1 | K-1302 | Açılış: altı plan önerisi, şablon taraması (PF-128…PF-131), konu kimliği taraması, devirlerin eşlemesi | **Dokümanda yaşar** (`09` başlık notu, §20.2); şablon kusurları playbook'ta; okuma düzeni Ö-139 |
| S-2 | K-1303 | Üç çalışma düzeni (⚠ öneriyle kayıt, merge yetkisi, yönetici + alt ajan düzeni) Aşama 8 boyunca açık; arşiv işaretiyle sona erer, sonraki aşamanın açılışında yeniden sorulur | **Önceden terfi etmişti** — biçimi `INSTRUCTIONS §2`, §9; yetki süreli (H-10, H-12) |
| S-3 | K-1304, K-1311 | Cevap bekleyen sorunun yolu; modlar cevaplanınca yönetici düzenine geçiş | **Önceden terfi etmişti** (Ö-141, Ö-142) |
| S-4 | K-1305…K-1328, K-1329…K-1358 | Workshop (on sekiz W konusu) ve yazım turu (altı oturum, dokuz parça) | **Dokümanda yaşar** (`09`); okunuş geri beslemeleri `02`, `05`, `06`'da; yöntem dersleri Ö-144, Ö-148 |
| S-5 | K-1310, K-1312 | Koşullu lisanslı kütüphanenin kabulü ve işletimi | **Dokümanda yaşar** (`09` Kütüphaneler tablosu; `DEPLOY_RUNBOOK` bloğu) — ürün kararı |
| S-6 | K-1359…K-1382 | Özet ↔ ayrıntı taraması, audit, deep review | **Dokümanda yaşar**; yöntem: betikli dar mercek paketleri (`scripts/doc-scans/09_*_paketle.py`; Ö-144) |
| S-7 | K-1383…K-1388 | Cross-review: ciddiyet ölçüsü 1. turdan, iki koşum (tek parça + beş parça), tekrar eden RET, 3 turda TEMİZ | **Önceden terfi etmişti** — `cross-review` skill'i Faz 1 madde 9, Faz 4 madde 5; ölçünün sorulmadan açılması Soru 1 |
| S-8 | K-1389, K-1390 | Etki yansıtma: downstream, upstream, yeni alan ve kural, Kaynak satırları | **Dokümanda yaşar**; yöntem dersi Ö-143 |
| S-9 | K-1391…K-1396 | Çakışma taraması, checkpoint | **Dokümanda yaşar**; yöntem dersleri Ö-147, Ö-148, Ö-153 |

### 2.5 Hafızadaki adaylar (kullanıcı hafızası, salt okunur — indeks satırlarından)

| # | Not | Sonuç |
|---|---|---|
| H-1 | `workshop-sorusu-oncesi-template-taramasi` | Önceden terfi etmişti (`INSTRUCTIONS §2`, checklist §1) |
| H-2 | `kisa-diyalog-tarzi-workshop-sorusu` | Önceden terfi etmişti (kısmen — `INSTRUCTIONS §2`) |
| H-3 | `ci-beklerken-izleyiciye-guvenme` | Önceden terfi etmişti (`INSTRUCTIONS §3.2`); Ö-151 bekleme betiğinin sonucu denetlemesini ekledi |
| H-4 | `toplu-onay-verdiginde-isaretle` | Önceden terfi etmişti (`INSTRUCTIONS §2`, `GUARDRAILS §6`) |
| H-5 | `limit-dolarsa-kaldigin-yerden-devam` | Hafızada kalır — kişisel çalışma tercihi |
| H-6 | `yazim-oturumunda-tek-baglam` | Önceden terfi etmişti (kısmen — checklist §4); Ö-144 kaynak paketini ekledi |
| H-7 | `sonraki-chat-mesaji` | Hafızada kalır — kişisel çalışma tercihi |
| H-8 | `cursor-agent-cross-review-kosumu` | Önceden terfi etmişti (`cross-review` skill'i; Codex CLI asıl yol) |
| H-9 | `dokumanlari-isimleriyle-kisa-acik-yaz` | Önceden terfi etmişti (`INSTRUCTIONS §2`) — bu rapor ve §5 buna uyar |
| H-10 | `asama1-kapanis-otomasyonu-yetkisi` | Önceden terfi etmişti (biçim — `INSTRUCTIONS §9`); yetki süreli — arşiv işaretinde "Aşama 8 kapandı — Aşama 9'un açılışında yeniden sorulur" satırı (6. adım) |
| H-11 | `toplu-oneride-evet-ret-sayilmaz` | Önceden terfi etmişti — §5'in beş sorusu ayrı numaralıdır |
| H-12 | `yalniz-cok-kritik-konulari-sor` | Önceden terfi etmişti; mod süreli — arşiv işaretinde yeniden sorulur (6. adım) |
| H-13 | `uzun-adimda-ara-durum-ver` | Önceden terfi etmişti (`INSTRUCTIONS §9`) |
| H-14 | `basit-isleri-ucuz-modele-ver` | Önceden terfi etmişti (`INSTRUCTIONS §9` (1)) |
| H-15 | `yalniz-sonnet-haiku-low` | Önceden terfi etmişti (`INSTRUCTIONS §9` (1)); değişmedi |

---

## 3. Aşama 8'in maliyet ölçümü (K-1398)

Kaynak: yönetici oturumunun kayıt dosyaları — `node scripts/metrics/token-usage.js <oturum>.jsonl` (`INSTRUCTIONS §9` (10)). Oturum 2026-10-29'da Aşama 8'in açılışıyla başladı; bu öğrenim terfisi adımı ölçüm sırasında koşuyordu (0,42 M, tabloda eksik kalır). Codex çağrıları (ChatGPT hesabı) ayrı hesaptır, dahil değildir. Başlangıç saatleri UTC.

### 3.1 Adım tablosu

| Adım | Başlangıç | Alt ajan | Yeni metin (M) | İşlenen (M) |
|---|---|---|---|---|
| Workshop W-1a | 15:40 | 0 | 0,24 | 4,07 |
| Workshop W-1b | 17:06 | 0 | 0,27 | 4,92 |
| Workshop W-2a | 17:16 | 0 | 0,30 | 10,43 |
| W-2a devamı | 17:30 | 0 | 0,10 | 0,97 |
| Workshop W-2b | 17:32 | 0 | 0,20 | 6,27 |
| Yazım Y-1a | 17:42 | 0 | 0,30 | 9,87 |
| Yazım Y-1b | 17:54 | 0 | 0,23 | 7,80 |
| Yazım Y-2a | 18:03 | 0 | 0,31 | 11,30 |
| Yazım Y-2b | 18:18 | 0 | 0,36 | 10,45 |
| Y-2b devamı | 18:33 | 0 | 0,14 | 2,92 |
| Yazım Y-3a | 18:40 | 0 | 0,30 | 10,51 |
| Yazım Y-3b | 18:53 | 0 | 0,32 | 12,16 |
| Yazım Y-3c | 19:09 | 0 | 0,30 | 11,51 |
| Yazım Y-4a | 19:23 | 0 | 0,23 | 6,52 |
| Yazım Y-4b | 19:31 | 0 | 0,31 | 9,54 |
| Y-4b devamı | 19:44 | 0 | 0,11 | 1,87 |
| Yazım Y-5a1 | 19:47 | 0 | 0,30 | 10,68 |
| Yazım Y-5a2 | 19:59 | 0 | 0,30 | 11,78 |
| Yazım Y-5b | 20:11 | 0 | 0,28 | 10,75 |
| Yazım Y-6a (kaynak paketli) | 20:23 | 0 | 0,26 | 5,55 |
| Yazım Y-6b | 20:32 | 0 | 0,31 | 8,64 |
| Özet ↔ ayrıntı taraması | 20:44 | 6 | 0,78 | 10,80 |
| Audit A-a | 20:55 | 33 | 2,51 | 19,57 |
| Audit A-b | 21:18 | 0 | 0,24 | 7,16 |
| Deep review D-a | 21:27 | 32 | 3,28 | 37,13 |
| Deep review D-b (yeniden doğrulama) | 21:48 | 5 | 0,69 | 13,98 |
| Cross-review 1. tur | 22:02 | 6 | 0,61 | 9,56 |
| Cross-review 2. tur | 22:14 | 6 | 0,56 | 8,32 |
| Cross-review 3. tur | 22:25 | 0 | 0,11 | 2,09 |
| Etki yansıtma | 22:34 | 4 | 0,67 | 16,25 |
| Çakışma taraması | 22:50 | 34 | 2,72 | 36,68 |
| Çakışma taraması devamı | 23:16 | 0 | 0,12 | 0,90 |
| Checkpoint C-a | 23:19 | 26 | 3,49 | 81,93 |
| Checkpoint C-b | 23:50 | 0 | 0,28 | 7,01 |
| Öğrenim terfisi (bu adım, eksik) | 00:01 | 0 | 0,06 | 0,42 |

**Toplam (yönetici + alt ajanlar):** işlenen **512,84 M**; yeni metin (yazılan + çıktı + girdi) **22,81 M**; 3 962 model çağrısı. İşlenenin yüzde 96'sı önbellekten yeniden okumadır (490,0 M).

| Rol | Ajan | Çağrı | İşlenen (M) | Pay |
|---|---|---|---|---|
| Yönetici (Opus) | 1 oturum | 310 | 92,5 | yüzde 18 |
| Yürütücü adım ajanları (Sonnet / low) | 35 koşum | — | 246,4 | yüzde 48 |
| Bulgu doğrulayıcıları (Opus / high) | 79 | 1 391 | 113,2 | yüzde 22 |
| Mercek, okuyucu, uygulayıcı (Sonnet ve Haiku / low) | 73 | — | 60,7 | yüzde 12 |

Doğrulayıcı başına ortalama 1,43 M (17,6 çağrı). Hook: 19 engel — 15'i alt ajan bağlam sınırı (250 bin) sonrası, 4'ü heredoc; merge denetimi 1 UYARI verdi (Y-2b, bağlam sınırı aşımı, Ö-145); 20'den fazla doğrulayıcı UYARI'sı yok (en çok 20, deep review D-a).

### 3.2 Aşama 7'nin ölçümüyle ve K-1301'in tahminiyle karşılaştırma

Aşama 7 sütunu K-1301'in ölçümünden alındı (692,3 M işlenen; ~32,2 M yeni metin; ~660 M yeniden okuma). Rol satırları **yeniden okuma** (önbellek okuması) temelinde, aynı yöntemle yazıldı.

| Ölçüt | Aşama 7 | K-1301 tahmini | Aşama 8 | Fark (Aşama 7'ye göre) |
|---|---|---|---|---|
| İşlenen toplam | 692,3 M | ~400 M (toplu doğrulama, dar mercek, büyük dosya) · ~350 M (uzun adımların bölünmesiyle) | **512,8 M** | yüzde 26 düşük; tahminin yüzde 28 üstünde |
| Yeni metin | ~32,2 M | — | 22,8 M | yüzde 29 düşük |
| Yeniden okuma toplamı | ~660 M | — | 490,0 M | yüzde 26 düşük |
| Yönetici (yeniden okuma) | ~62 M | — | 91,3 M | yüzde 47 yüksek |
| Yürütücü | 22 koşum, 271 M | bölmeyle azalır | 35 koşum, 237,6 M | yüzde 12 düşük (koşum başına 12,3 → 6,8 M) |
| Opus doğrulayıcı | 241 ajan, 188 M | toplu doğrulamayla azalır | 79 ajan, 106,6 M | adet yüzde 67, tüketim yüzde 43 düşük |
| Mercek ve uygulayıcı | 58 ajan, 139 M | dar mercekle azalır | 73 ajan, 54,5 M | yüzde 61 düşük |

**Okuma.** (1) **Toplu doğrulama ve dar mercek işledi:** doğrulayıcı sayısı üçte bire indi, merceklerin yeniden okuması yüzde 61 düştü; audit adımı 111,7 M'den 26,7 M'ye indi (dört kat), deep review 108,8 M'den 51,1 M'ye. Etki yansıtma 39,9 M'den 16,3 M'ye, çakışma taraması 50,9 M'den 37,6 M'ye indi; checkpoint 111,5 M'den yalnız 88,9 M'ye indi. (2) **Tahmin tutmadı, çünkü tahmin yürütücüleri küçültmeye güveniyordu:** yürütücüler ve yönetici toplamın yüzde 66'sı (338,9 M) ve K-1301'in üç kuralı onlara değmiyor. Yürütücü koşumu başına tüketim yarıya indi (bölme ve kaynak paketi işledi: Y-6a 5,6 M), ama koşum sayısı 22'den 35'e çıktı; yazım turunun on altı parçası (141,9 M, toplamın yüzde 28'i) ve workshop'un beş parçası (26,7 M) birlikte 168,5 M tutar. (3) **Doğrulayıcı başına tüketim arttı** (0,78 M → 1,43 M): doküman 302 KB'tan 470 KB'a çıktı ve doğrulayıcılar bulgunun geçtiği bölümleri, ilgili karar satırlarını ve üst doküman bölümlerini birlikte okuyor. (4) **Yönetici payı büyüdü:** otuz beş adım döngüsünde yönetici bağlamı ortalama ~300 bin belirteçti (Ö-159).

**Karşılaştırmanın sınırı.** İki aşama farklı dokümanları (`08` ve `09`), farklı boyutlarda yazdı; Aşama 8 workshop'unda on sekiz W konusu vardı. Aşama 7'nin yürütücü satırı 22 koşumdur, bu aşamanın 35'i "devamı" koşumlarını içerir. Ölçüm kayıt dosyalarındandır (ajan bildirimi değil), ama Codex çağrıları ve bu adımın kendisi dışarıdadır.

### 3.3 En pahalı üç adımın nedeni

1. **Checkpoint — C-a ve C-b, 88,9 M (toplamın yüzde 17'si).** C-a'nın 81,9 M'si: 15 Opus / high doğrulayıcı 43,3 M (doğrulayıcı başına 2,9 M, 521 çağrı), 11 Sonnet mercek 33,8 M (mercek başına 3,1 M, 284 çağrı), yürütücünün kendisi 4,9 M. Neden: checkpoint'in iç tutarlılık mercekleri 470 KB'lık dokümanın tamamını ve üst dokümanların ilgili bölümlerini tarar; 56 bulgunun hepsi doğrulayıcıya gitti ve her doğrulayıcı dokümanın birden çok bölümünü okudu. Kapanış adımlarında okuyucu sayısına sınır yok (Ö-152). Verim yüksek: 48 düzeltme.
2. **Deep review — D-a ve D-b, 51,1 M.** D-a'nın 37,1 M'sinin 28,8 M'si 20 doğrulayıcıdadır (doğrulayıcı başına 1,44 M, 337 çağrı); 12 mercek yalnız 4,5 M. Neden: 105 ham bulgudan 81'i doğrulandı; doğrulayıcılar toplamın yüzde 78'ini harcadı. Seviyesi yükselen altı bulgunun yeniden doğrulaması ayrı adımda (D-b) 14,0 M tuttu (Ö-149).
3. **Çakışma taraması — 37,6 M.** 11 Opus doğrulayıcı 22,3 M (doğrulayıcı başına 2,0 M), 23 Sonnet okuyucu 6,0 M, yürütücü 8,4 M (55 çağrı). Neden: 35 bulgunun kademeli dağıtımı 11 doğrulayıcıya indi, ama her doğrulayıcı karar kaydının ilgili satırlarını ve `09` bölümlerini yeniden okur; yürütücü K, park ve devir taramalarını betikle sürdürürken uzun bağlam taşıdı ve bir devam koşumu gerekti.

Ortak neden: kapanış adımlarının doğrulayıcıları ve okuyucuları, Aşama 7'de tasarruf getiren sınırlardan (dar mercek, toplu doğrulama) yalnız kısmen yararlandı; çünkü kapsamları doğası gereği dokümanın tamamıdır. Bu bir kural boşluğu değil ödünleşimdir (Ö-152).

---

## 4. Ö-23 — checklist skill'e dönüşür mü

**Sonuç: iki koşul karşılanıyor — dönüşüm proje sahibine yeniden sunulur (Soru 3); bu PR'da uygulanmadı.** Ölçüt iki koşuldur ve ikisi birlikte aranır (checklist başlık notu, L4; K-722, K-853). Değerlendirme betikle yapıldı:

| Koşul | Sonuç | Kanıt |
|---|---|---|
| **1. Checklist izlenebilirlik matrisi zorunlu bir aşamada da işletilmiş olmalı** | ✓ **Karşılandı** — Aşama 3, 5 ve 6'da işletildi. Aşama 7 ve 8'de matris zorunlu değildi (`00 §C.1`); koşul geçmişte karşılanmıştır | `07 §1` (üçüncü işletim) |
| **2. Bir aşama kapanışında yapısal değişiklik — yeni adım ya da yeni madde — almamış olmalı** | ✓ **Karşılandı** — Aşama 8 boyunca ve bu terfide | `grep -c '^- \[ \]'`: **67**; `git log 96e7b53..HEAD -- .claude/checklists/` boş (Aşama 8 boyunca dokunulmadı); **bu terfiden sonra 67**. Değişen: **üç madde genişletmesi** — §3'te iki (geri işaret, geri besleme), §6'da bir (etki yansıtma). Adım sayısı aynı |

**Bu kez neyin farklı olduğu.** Proje sahibi dört kez dönmemesini seçti; son seçim *"Liste kalsın, her aşama sonunda yine sorulsun"* (K-1300). Aşama 8'in yeni bilgisi: liste matrissiz ikinci aşamada da sorun çıkarmadı ve yaşadığı boşluklar (geri işaret, okunuş geri beslemesi) listenin biçiminden değil uygulanmasından geldi; skill'e dönüşmek bunu çözmezdi. Önerim bu yüzden Aşama 7'dekiyle aynı: **seçenek 3** — liste kalır ve soru artık her aşama sonunda sorulmaz; ölçüt başlık notunda durur, proje sahibi isterse dönüşümü o an açar. Seçenek 2 (bugünkü düzen) de geçerlidir.

**Elenen — dönüşümü bu PR'da uygulamak:** yeni skill yaratmak yapısal değişikliktir ve onay ister (GUARDRAILS §3); `00 §C.7`'nin metni değişir (GUARDRAILS §2). **Elenen — soruyu sormadan kapatmak:** kapatma kararı proje sahibinindir.

---

## 5. Proje sahibinin cevabını bekleyen beş soru

> **Hepsi ayrı numaralı sorudur** (`INSTRUCTIONS §2`; K-722 → K-853: tek bir "evet" birden çok öneriye okunmaz). Soru 1'in seçenekleri birbirinden bağımsızdır; birden fazlasını seçebilirsiniz. Uygulanacak metinler cevaptan sonra ayrı bir `docs:` PR'ında yazılır; hiçbiri aşamanın kapanışını engellemez, kurallar L3–L4'te yürürlükte. **Kapı:** arşiv işaretinden önce (K-437'nin 6. adımı), ⚠ listesiyle aynı mesajda. Her sorunun altında "Uygulanacak metin" çıplak biçimdedir; yönetici sade metni iletir.

### Soru 1 — Bağımsız okuma turlarında "ağır sayma" ölçüsü kimin kararıyla açılır (Ö-154)

> **Sorun:** Her doküman bittikten sonra başka bir yapay zekâya baştan sona okutup bulgu istiyoruz (cross-review turları). Her turda yeni bir kenar durum bulunur ve doküman büyür, tur bitmez; bunu önlemek için bir "ağır sayma" ölçüsü var: kenar durum bulgusu ağır sayılmaz, yalnız gerçek kural ihlali sayılır. Yöntem dosyası bu ölçünün hangi dokümanlarda ilk turdan açılacağını adlarıyla sayıyor; Kodlama Kılavuzu o listede yok. Yönetici Kodlama Kılavuzu'nu listedekilere benzetti, ölçüyü ilk turdan görev dosyasına kendisi yazdı; size sormadı ve kararı ⚠ ile işaretlemedi. Sonuç: üç turda temiz (7 → 6 → 0 bulgu). Ölçü yerinde görünüyor, ama sizin haberiniz olmadan açıldı; aşama sonunda bunu konuşmak istemiştiniz. **Ne değişir:** aşağıdaki seçenekler birbirinden bağımsızdır.
> **1 ·** yöntem dosyasına Kodlama Kılavuzu'nu, Implementation Planı'nı ve Doğrulama Protokolü'nü adıyla ekleyelim; bunlarda ölçü ilk turdan, sorulmadan açılsın (öneri) · **2 ·** listede adı olmayan bir dokümanda yönetici ölçüyü kendisi açmasın, önce size sorsun · **3 ·** böyle süreç kararları (ölçü, tur sayısı, koşum biçimi) da aşama sonunda size gösterilen ⚠ listesine girsin · **4 ·** bu aşama için ölçüsüz bir doğrulama turu daha koşulsun (yaklaşık 9–10 milyon belirteç ek maliyet; önerilmez — üç tur temiz bitti)
> Kaynak: K-1383, K-1385, K-1387 · `cross-review` skill'i "Ne zaman uygulanır" · `INSTRUCTIONS §2` ("Ortak kural", ⚠ listesi)

**Uygulanacak metin.** (1 ise) `cross-review` skill'i "Ne zaman uygulanır" ikinci maddenin parantez listesine: *"Kodlama Kılavuzu, K-1383 — 470 KB'lık doküman üç turda TEMİZ döndü, 7 → 6 → 0 bulgu; Implementation Planı ve Doğrulama Protokolü de aynı sınıftır (kuralı başka dokümanlardan devralıp kendi mekanizma kararlarını koyar)"*. (2 ise) aynı maddenin sonuna: *"Listede adı geçmeyen bir dokümanda ölçüyü yönetici kendisi açmaz; 1. turdan önce proje sahibine tek soruyla sorulur."* (3 ise) `INSTRUCTIONS §2` "Ortak kural"a: *"Ürün kuralı olmayan ama aşamanın sonucunu etkileyen süreç kararları (cross-review ölçüsü, tur sayısı, koşum biçimi) de konu hücresinde ⚠ taşır ve listede 'süreç' başlığıyla ayrı gösterilir."* (4 ise) metin yok; Kodlama Kılavuzu'na ölçüsüz bir cross-review turu ayrı adım olarak koşar.

**Cevap (2026-10-30, proje sahibi; oturumda Soru 6):** seçenek 1, 2 ve 3; 4 değil. Proje sahibi ilk anlatımı anlamadı ("daha anlaşılır dilde yaz"); sade metinle yeniden soruldu. Uygulandı: `cross-review` skill'i, `INSTRUCTIONS §2`; K-1383, K-1385, K-1387 ⚠ listesine 'süreç' başlığıyla girdi (K-1399).

### Soru 2 — Komut koruyucusu heredoc'un yan kapılarını da kapatsın mı (Ö-155)

> **Sorun:** Bir metni dosya yerine doğrudan kabuğa yazdırarak komut çalıştırmak (heredoc ve benzerleri) bu projede yasak; Türkçe harfler ve tırnaklar sessizce bozulabiliyor. Genel komut koruyucusu (projenin dışında, kullanıcı ayarlarınızda; bütün projelerinizi etkiler) şu an yalnız betik çalıştıran heredoc'u engelliyor; here-string'i önceki kararınızda serbest bırakmıştık. Bu aşamada alt ajanlar yasak biçimi en az on kez denedi ve koruyucu dört biçimi geçirdi: metni `<<<` ile vermek, tek satırlık betiği `python -c` ile çalıştırmak, `/dev/null`'a boş heredoc ve markdown dosyasına `cat >> dosya <<` ile satır eklemek. Yasak yazılıydı ama araç durdurmadı; kural yalnız ajanın hatırlamasına kaldı. **Ne değişir:** 1 derseniz koruyucu bu dört biçimi de engeller (commit ve PR metni serbest kalır); ajan betiği dosyaya yazıp dosyadan çalıştırmak zorunda olur. Değişiklik koruyucunun deneme klasöründe yapılır, testleri geçmeden yayına girmez. 2 derseniz koruyucu aynı kalır, yasak yalnız talimatta durur.
> **1 ·** koruyucu bu dört biçimi de engellesin (öneri) · **2 ·** engel eklenmesin, yasak yalnız talimatta kalsın
> Kaynak: Ö-155; K-1300 (Soru 3, seçenek 2) · `INSTRUCTIONS §9` (6) · PF-126

**Uygulanacak metin (1 ise).** Proje tarafında `INSTRUCTIONS §9` (6)'daki *"commit ve PR metni ile here-string serbesttir"* cümlesi *"commit ve PR metni serbesttir; yorumlayıcıya giden here-string, `-c` ile satır içi betik, boş heredoc ve dosyaya `<<` ile ekleme engellenir"* olur. Hook değişikliği `~/.claude/hooks/staging/` içinde `command-guard.js`'e yapılır, `run-tests.js`'e her biçim için engelli ve serbest örnekler eklenir, `node ~/.claude/hooks/deploy.js` ile yayına alınır; proje dışıdır. PF-126'ya devam notu. **(2 ise):** metin yok; `INSTRUCTIONS §9` (6)'ya "Aşama 8'de dört yan biçim geçti, koruyucu genişletilmedi" notu.

**Cevap (2026-10-30, proje sahibi; oturumda Soru 7):** seçenek 1, engellensin. Hook yönetici tarafından değiştirildi ve yayına alındı (`~/.claude/hooks/command-guard.js`, 173 test; proje dışı); `INSTRUCTIONS §9` (6) ve PF-126 güncellendi (K-1399).

### Soru 3 — Kontrol listesi beceriye dönüşsün mü (Ö-23)

> **Sorun:** Proje boyunca izlediğimiz adımların listesi (kontrol listesi) şu an düz bir liste; aynı işi yapan bir "beceri" (skill) olarak da yazılabilir. Önceki dört aşamada bunu önerdim ve her seferinde listenin kalmasını seçtiniz; son seçiminiz soruyu her aşama sonunda yeniden sormamdı. Bu aşamada liste yine yeni adım ya da madde almadı (67 madde yerinde; Aşama 8 boyunca listeye hiç dokunulmadı) ve çalıştı; yaşadığımız boşluklar listenin biçiminden değil uygulanmasından geldi. **Ne değişir:** 1 derseniz liste beceriye çevrilir (yöntem dosyasının bir cümlesi de değişir). 2 derseniz bugünkü düzen sürer ve soru bir sonraki aşama sonunda yine sorulur. 3 derseniz liste kalır ve soru artık her aşama sonunda sorulmaz; dönüşümü istediğiniz an siz açarsınız.
> **1 ·** liste beceriye dönüşsün · **2 ·** liste kalsın, her aşama sonunda yine sorulsun (bugünkü düzen) · **3 ·** liste kalsın, artık sorulmasın (öneri)
> Kaynak: Ö-23 · K-722, K-853, K-991, K-1143, K-1232, K-1300 · PF-32

**Uygulanacak metin.** (1 ise) ayrı `docs:` PR'ında yeni bir skill ve `00 §C.7`'nin cümlesi (GUARDRAILS §2, §3). (3 ise) checklist başlık notuna *"Aşama 8'in öğrenim terfisinde (K-1397) proje sahibi sorunun artık her aşama sonunda sorulmamasına karar verdi; ölçüt bu notta kalır, dönüşümü proje sahibi isterse açar"* cümlesi ve PF-32'nin durumu. (2 ise) başlık notuna yalnız Aşama 8'in sonucu eklenir (iki koşul karşılandı, 67 madde yerinde).

**Cevap (2026-10-30, proje sahibi; oturumda Soru 8):** seçenek 3, liste kalsın, artık sorulmasın. Checklist başlık notuna cümle eklendi; PF-32 `Dönüşmedi`; tracker §20.1'in Ö-23 maddesi [x] (K-1399).

### Soru 4 — Yöntem dosyasına büyük standart dokümanın okunma biçimi yazılsın mı (Ö-139)

> **Sorun:** Yöntem dosyası "Kodlama Kılavuzu her kodlama görevinde ajana verilir" diyor. Kılavuz yaklaşık 470 KB; ajanlar 100 KB'tan büyük dosyayı bütün okuyamıyor (maliyeti sınırlamak için konan kural). Yani yazılı kural ile uygulama ayrışıyor: bugün kılavuzun başlık notu ve Implementation Planı bölüm bölüm okumayı söylüyor, yöntem dosyası söylemiyor. **Ne değişir:** 1 derseniz yöntem dosyasının ilgili tablosunun altına bir paragraf girer (aşağıda); sonraki projeler aynı sorunu baştan yaşamaz. 2 derseniz yöntem dosyası aynı kalır, kural yalnız Kodlama Kılavuzu'nun başlık notunda ve Implementation Planı'nda yaşar.
> **1 ·** paragraf yöntem dosyasına yazılsın (öneri) · **2 ·** yazılmasın
> Kaynak: Ö-139; K-1302 (4) · `00 §C.6` · `INSTRUCTIONS §9` (7) · PF-131

**Uygulanacak metin (1 ise).** `00 §C.6`'nın katman tablosunun altına: *"**Büyük doküman:** katmandaki bir doküman 100 KB'ı aştığında (ör. Kodlama Kılavuzu) 'her görevde' dokümanın bütününün verilmesi anlamına gelmez; ajan dokümanın çekirdek bölümlerini ve görevin konu bölümlerini okur, hangi bölümlerin okunacağı Implementation Planı'nın task satırında yazılır (`INSTRUCTIONS.md` §9 (7))."* `00` sürümü +1 (v1.0.10). PF-131 `Uygulandı` olur.

**Cevap (2026-10-30, proje sahibi; oturumda Soru 9):** seçenek 1, yazılsın. Paragraf `00 §C.6`'nın tablosunun altına yazıldı; `00` v1.0.10; PF-131 `Uygulandı` (K-1399).

### Soru 5 — Yöntem dosyasına Aşama 8'in dersleri (`00 §N.1`)

> **Sorun:** Yöntem dosyası her aşamanın tekrarlanan derslerini kısa bir paragrafla tutuyor; Aşama 8 için henüz yok. **Ne değişir:** Evet derseniz aşağıdaki üç ders yöntem dosyasına yazılır (sonraki projelerin aynı hataya düşmemesi için); hayır derseniz dersler yalnız bu raporda ve kontrol listesinde kalır. Üç ders: (1) çok uzun bir dokümanı yazarken her parçaya önceden hazırlanmış tek dosyalık kaynak paketi verilince ajan sınıra yaklaşmadı, verilmeyince hemen hepsi sınıra dayandı; (2) yazılı bir kural ilk uygulamada tutmayabiliyor — "eski karara geri işaret bırak" ve "üst doküman cümlesini yorumladıysan geri besle" kurallarını yönetici elle düzeltti ve betik denetimi gerekti; (3) bir tarama "kalan eşleşme yok" derken yalnız aradığı terimler kadar doğrudur — terimler kararın cümlesinden türetilince bölümün öteki cümleleri kaçtı.
> **1 ·** üç ders yöntem dosyasına yazılsın (öneri) · **2 ·** yazılmasın
> Kaynak: Ö-143, Ö-144, Ö-147, Ö-148 · `00 §N.1` · PF-142

**Uygulanacak metin (1 ise).** `00 §N.1`'e, Aşama 7 paragrafının altına: *"**Aşama 8 — Kodlama kılavuzu (2026-10-30).** Kurallar `.claude/INSTRUCTIONS.md` §3.2, §9, `.claude/checklists/document-stage.md` §3, §6 ve rapor `Docs/CHECKPOINT_REPORTS/PHASE8_LEARNING_PROMOTION.md`. Tekrarlanacak desenler: 1. **Uzun bir dokümanın yazımında her parçaya kaynak paketi verilir.** Paketsiz yazım parçaları adım başına 10–12 M belirteç ve 70–80 araç çağrısıyla bağlam sınırına ulaştı; betikle hazırlanmış tek dosyalık paket (plan satırları, park metinleri, ilgili kararlar, kaynak bölümler) verilen parça 41 çağrı ve 5,6 M ile bitti. 2. **Yalnız yazılı bir kural uygulanmıyorsa betik denetler.** Geri işaret (yönetici dört kez elle ekledi, çakışma taraması 16 satır daha buldu) ve üst doküman cümlesinin okunuşunun geri beslemesi (beş kez atlandı) checklist'te yazılıydı ve ilk uygulamada tutmadı. 3. **'Kalan eşleşme yoktur' tarama terimleri kadar doğrudur.** Etki yansıtmanın terimleri yalnız kararın cümlesinden türetildi, bölümün kapalı liste ve sayım cümleleri taranmadı; checkpoint dört yerde buldu."* `00` sürümü +1. Seçenek 2 de geçerlidir: üç dersin üçü kontrol listesinde ve `INSTRUCTIONS`'ta zaten uygulanır.

**Cevap (2026-10-30, proje sahibi; oturumda Soru 10):** seçenek 1, yazılsın. Paragraf `00 §N.1`'e, Aşama 7 paragrafının altına yazıldı; `00` v1.0.10 (iki değişiklik, tek sürüm artışı); PF-142 `Uygulandı` (K-1399).

---

## 6. Uygulanan terfiler — dosya ve bölüm

| Dosya | Bölüm | Değişiklik | Aday |
|---|---|---|---|
| `.claude/INSTRUCTIONS.md` | §9 — "Alt ajanların koşumu" (1) | Yeniden doğrulama ayrı adımda yapılır; 20 sınırı adım başınadır | Ö-149 |
| | §9 — "Yeniden okumayı sınırlayan kurallar" (8) | Devir notu en çok 5 KB | Ö-145 |
| | §9 — (9) | Kaynak paketi yazım oturumlarına da verilir | Ö-144 |
| | §3.2 — "Sessizlik yeşil değildir" | CI bekleme betiği sonucu da denetler (çıkış kodu) | Ö-151 |
| `.claude/checklists/document-stage.md` | §3 — geri işaret maddesi | Denetim adım sonu betiğine girer; betik `scripts/doc-scans/geri_isaret_tara.py` | Ö-147 |
| | §3 — geri besleme maddesi | Yorum satırı kayıt anında geri beslemeyi ya da "gerekmez" gerekçesini taşır | Ö-148 |
| | §6 — etki yansıtma maddesi | Tarama terimleri bölümün kapalı liste ve sayım cümlelerinden de türetilir | Ö-143 |
| `.claude/skills/checkpoint/SKILL.md` | "Koşum" | Kaynak karar zaten hükmetmişse doğrulayıcı dayanağı yeniden açmaz | Ö-153 |
| `scripts/doc-scans/geri_isaret_tara.py` | yeni (L5 niteliğinde) | Karar aralığında geri işaret adaylarını bulur | Ö-147 |

Madde sayısı yerinde: `grep -c '^- \[ \]' .claude/checklists/document-stage.md` → 67 (başlangıç 67). Checklist'in "Son güncelleme" tarihi 2026-10-30.

**L5 (CI) elendi:** Ö-147'nin betiği adım sonu betiğine elle zincirlenir; çıktısı aday listesidir ve her aday okunur, bu yüzden CI görevi olarak taşınması doküman dönemi için gereksizdir. **Hook (genel) değişmedi:** Ö-155 Soru 2'dedir.

---

## 7. Playbook'a geri akış (K-649)

Liste 133 satırdan **142**'ye çıktı. Var olan satırlardan PF-32 (Ö-23'ün Aşama 8 sonucu) ve PF-131 (Soru 4) güncellendi. PF-132 ve PF-133 tekrar açılmadı.

| Satır | Öğrenim | Hedef |
|---|---|---|
| **PF-134** (yeni) | Kaynak paketi yazım oturumlarına (Ö-144) | `INSTRUCTIONS §9` (9) |
| **PF-135** (yeni) | Devir notu en çok 5 KB (Ö-145) | `INSTRUCTIONS §9` (8) |
| **PF-136** (yeni) | Geri işaret denetiminin adım sonu betiğine alınması (Ö-147) | checklist §3 · `scripts/doc-scans/` |
| **PF-137** (yeni) | Okunuş geri beslemesinin kayıt anında yazılması (Ö-148) | checklist §3 |
| **PF-138** (yeni) | Yeniden doğrulamanın ayrı adımda yapılması (Ö-149) | `INSTRUCTIONS §9` (1) |
| **PF-139** (yeni) | CI bekleme betiğinin sonucu denetlemesi (Ö-151) | `INSTRUCTIONS §3.2` |
| **PF-140** (yeni) | Etki yansıtma taramasının terimleri bölümün kapalı liste ve sayım cümlelerinden de türetmesi (Ö-143) | checklist §6 |
| **PF-141** (yeni) | Doğrulayıcı kaynak kararın hükmünü yeniden açmaz (Ö-153) | `skills/checkpoint` |
| **PF-142** (yeni) | Soru 1, 2, 5: cross-review ölçüsünün kapsamı, hook'un yan kapıları, Aşama 8'in desenleri | `skills/cross-review` · hook · `00 §N.1` — `Onay bekliyor` |

---

## 8. Kayda ve hafızaya yansıma

- **Karar kaydı v0.223:** K-1397 (bu adımın sonucu: 46 kalemin dökümü, sekiz terfi, beş soru) ve K-1398 (Aşama 8'in maliyet ölçümü; §3). Her ikisi süreç kararıdır, ürün kuralına dokunmaz — ⚠ grubuna girmez. §20.1: öğrenim terfisi adayları maddesi [x]; aşama kapanışının adımları maddesi (5. adım koşuldu); Ö-23 maddesi değerlendirmeyle güncellendi ([ ] kalır, kapısı Soru 3'ün cevabı); belirteç ölçümü maddesi [x]; **yeni açık madde** — proje sahibinin cevabını bekleyen beş soru (kapı: arşiv işaretinden önce).
- **Repo hafızası:** `MEMORY.md` Güncel Durum bu adımın hâline getirildi — önceki adımın (checkpoint) paragrafı `MEMORY_ARCHIVE.md`'ye taşındı (`INSTRUCTIONS §7`).
- **Kullanıcı hafızası — bu adımda yazılmadı; yönetici günceller:** `asama1-kapanis-otomasyonu-yetkisi.md` (H-10) ve `yalniz-cok-kritik-konulari-sor.md` (H-12) — arşiv işaretinde "Aşama 8 kapandı — Aşama 9'un açılışında yeniden sorulur" satırları (6. adımın işi).
- **Mekanik:** değişen dosyalarda CR 0 (`tr -cd '\r'`), kontrol karakteri 0; eklenen satırlarda geri başvuru artığı yok; K numarası çifti yok; karar kaydı satırlarının sütun sayısı 7 ayraç; düzenlemeler birebir değiştiren, her değiştirmede tam bir eşleşme isteyen betikle yapıldı ve betikler dosya yazma aracıyla yazıldı. Bu adım kabukta heredoc denemedi.
- **Model dağılımı:** bu adımı yürüten ajan Sonnet / `low`; alt ajan açılmadı, bulgu doğrulaması gerektiren iddia çıkmadı.

**Sırada:** K-437'nin 6. adımı — proje sahibine tek mesaj: çakışma taramasının ve checkpoint'in ⚠ kararları (22 karar; hazır metin `PHASE8_CONFLICT_SCAN.md` §4.1) ve bu raporun §5'indeki beş soru, **her biri ayrı numaralı**; cevapların kayda işlenmesi (onaylananlar ayrı `docs:` PR'larında); ardından arşiv işareti (⚠ işaretlerinin dönüşümü; Aşama 8'in kayıtları salt okunur; `09` ✓) ve Aşama 9'a (Implementation Planı) devir.

---

*Kaynak: K-437 (kapanış sırası, 5. adım) · `00 §K`, §C.6, §C.7, §N · GUARDRAILS §2, §3 · karar kaydı §20.1 (aday listesi, Ö-23, belirteç ölçümü) · çakışma taraması §7 · CP08 §6 · K-1302…K-1396 (süreç kararları) · K-1301 (ölçüm yöntemi ve tahmin) · K-1383 (ciddiyet ölçüsü) · K-722, K-852, K-853, K-991, K-1143, K-1232, K-1300 (Ö-23'ün ölçütü ve önceki kararları) · K-649 (playbook'a geri akış) · K-1397, K-1398 (bu adımın kararları) · `Docs/CHECKPOINT_REPORTS/PHASE7_LEARNING_PROMOTION.md` (biçim ve karşılaştırma tabanı).*
