# Aşama 7 — Öğrenim Terfisi

**Tarih:** 2026-10-29 | **Aşama:** 7 — Entegrasyon spesifikasyonları | **Kapanış adımı:** K-437'nin 5. adımı (`00 §K`; `checklists/document-stage.md` §7 madde 5)
**Girdi:** karar kaydı v0.191 — §18.1'in yedi öğrenim adayı, bekletilen Ö-23, Aşama 7'nin model ve belirteç tablosu; Aşama 7'nin süreç kararları (K-1233…K-1296 içinden); [çakışma taraması](PHASE7_CONFLICT_SCAN.md) §7, [checkpoint](CP07_PHASE7_CHECKPOINT.md) §6; `Docs/PLAYBOOK_FEEDBACK.md` (PF-117…PF-121); kullanıcı hafızasının on altı notu — `origin/main` @ `ccfe77d`
**Çıktı:** karar kaydı v0.192 · `.claude/checklists/document-stage.md` (altı madde genişledi, madde sayısı 67 yerinde), `.claude/INSTRUCTIONS.md` §2, §3.2, §9 · `.claude/memory/MEMORY.md`, `MEMORY_ARCHIVE.md` · `Docs/PLAYBOOK_FEEDBACK.md` 127 satır · karar K-1297, K-1298 — `00`, `CLAUDE.md`, `SETUP.md`, API Tasarımı ve öteki dokümanlar, `GUARDRAILS.md`, `CONTEXT.md` ve skill'ler değişmedi

> **Ne işe yarar:** `00 §K` gereği aşama, öğrenimi yazılmadan kapanmaz. Bu rapor Aşama 7 boyunca kayda, raporlara ve hafızaya düşen her öğrenim adayının sonucunu yazar. Yedi aday L3–L4'e terfi etti ve bu PR'da uygulandı. Aşama 7'nin maliyet ölçümü §3'te; checklist'in skill'e dönüşmesi §4'te; proje sahibine gidecek dört soru §5'te — her biri ayrı numaralı, sade dille, K numarasız. `00` hedefli öneri, bulgu doğrulamasının toplu yapılması ve hook önerisi uygulanmadı (GUARDRAILS §2, §3; hook proje dışıdır); cevap gelince ayrı `docs:` PR'ında uygulanır.

---

## 1. Sonuç

| Sonuç | Sayı | Adaylar |
|---|---|---|
| **Terfi etti — bu PR'da uygulandı** (L3, L4) | 8 | Ö-126, Ö-127, Ö-128, Ö-130, Ö-131, Ö-132, Ö-133, Ö-135 |
| **Raporda kalır** — tek seferlik gözlem | 4 | Ö-134, Ö-136, Ö-137, Ö-138 |
| **Proje sahibine sorulur** | 4 soru | Ö-129 (Soru 1) · Ö-23 (Soru 2) · Ö-133'ün hook yanı (Soru 3) · `00 §N.1`'e Aşama 7'nin desenleri (Soru 4) |
| **Önceden terfi etmişti ya da dokümanda yaşar** — süreç kararları | 7 grup | S-1…S-7 (§2.2) |
| **Önceden terfi etmişti / hafızada kalır / sorulur** — kullanıcı hafızası | 16 not | H-1…H-16 (§2.4) |

Toplam otuz yedi kalem: tracker §18.1'in yedi adayı, bu adımda bulunan altı gözlem (Ö-133…Ö-138), Ö-23, Aşama 7'nin süreç kararlarının yedi grubu ve kullanıcı hafızasının on altı notu. `CLAUDE.md` ve `SETUP.md` için öneri yok — oturum başlangıcı ve katman kuralı aşamanın hiçbir öğrenimiyle çelişmiyor.

**Aşamanın asıl öğrenimi.** Entegrasyon Spesifikasyonları, izlenebilirlik matrisi zorunlu olmayan ilk aşamaydı (devirlerin eşlemesi yerini aldı) ve on sekiz adımda yürüdü. Üç şey belirgin: (1) **Süre ve adım ajanı belirteci Aşama 6'ya göre yarıya indi** — 5 sa 34 dk net süre (Aşama 6: 13 sa 23 dk) ve adım ajanı belirteci ~5,6 M (Aşama 6: ~10,7 M); ama toplam kayıtlı belirteç neredeyse aynı kaldı (~33,0 M, Aşama 6: ~33,1 M) çünkü bulgu doğrulayıcıları payı büyüttü: 232 Opus / `high` doğrulayıcı ~19,5 M, yani toplamın yüzde 59'u (§3). (2) **Aşamanın en tekrarlanan sapması bir kural değil, bir uygulama kusuruydu:** alt ajanlar kabukta heredoc yasağını yedi adımda ihlal etti; kural yalnız yöneticinin talimatında yaşıyordu (Ö-133). (3) **Mekanik taramalar yine kayda yazılmamış dokunuşu görmedi:** kurulum işleri (Ö-132), kıyas tablosunun sonuç cümlesi (Ö-131) ve ⚠ listesi (Ö-130) betik ya da kalite döngüsü tarafından değil, çakışma taramasının ve checkpoint'in farklı okuma biçimleriyle bulundu.

---

## 2. Adaylar ve sonuçları

**Kimlikler:** Ö-126…Ö-132 tracker §18.1'in yedi adayıdır (sırasıyla aday 1…7; Ö numaralandırması Aşama 6'nın Ö-125'inden sürer). Ö-133…Ö-138 bu adımda bulunan gözlemlerdir. Ö-23 Aşama 1'den gelir. S-1…S-7 Aşama 7'nin süreç kararlarının gruplarıdır, H-1…H-16 kullanıcı hafızasının on altı notudur — ikisi de bu rapora özgü numaralardır.

### 2.1 Karar kaydındaki adaylar (§18.1 ve çakışma taraması §7, checkpoint §6)

| # | §18.1 | Aday | Sınıf (`00 §K`) | Sonuç | Hedef katman ve dosya | Uygulanan metin (özet) | Playbook |
|---|---|---|---|---|---|---|---|
| Ö-126 | 1 | Şablon taraması sekizinci şablonda aynı eksiği buldu — açık kararlar bölümü yok; her açılışta aynı çözüm yeniden gerekçelendi | İhlali önleyen kural | **Terfi etti** | L4 checklist §1 (şablon taraması maddesi) | Şablonda açık kararlar bölümü yoksa çözüm hazırdır: açık kalem tracker'da, talimat sonraki dokümanın park bloğunda; açılış tek cümleyle uygular, eksik playbook satırına yazılır | PF-122 |
| Ö-127 | 2 | Devir dizinleri Aşama 1'in workshop kararlarını `08` tablosunda taşımıyor — etki sütununda `08` taşıyan 82 kararın 14'ü dizinlerde yok | İhlali önleyen kural | **Terfi etti** | L4 checklist §1 (girdi maddesi, devirlerin eşlenmesi) | Eşleme kaynağı etki sütununun betikle taranmasıdır; dizin ikinci kaynaktır ve fark girdiler notuna yazılır | PF-122 |
| Ö-128 | 3 | Gövde anmalarının sayımı matrisin hücre değerini düzyazıdan ayırmalı — 353 anmanın 208'i tek bir değer | İhlali önleyen kural | **Terfi etti** | L4 checklist §1 (girdi maddesi, gövde cümlesi sayımı) | Matrisin hücre değeri ayrı kalem olarak sayılır | PF-122 |
| Ö-129 | 4 | Bulgu doğrulamasının maliyeti — seviyeye göre toplu doğrulama | Proje sahibinin kararı | **Sorulur (Soru 1)** | L3 `INSTRUCTIONS §9` (1), `skills/audit` "Koşum biçimi" 2. madde | Uygulanmadı | PF-127 |
| Ö-130 | 5 | ⚠ listesinin elle tutulması kayıt satırlarının gerisinde kaldı — Aşama 6'da bir, Aşama 7'de 30 kararın 11'i | İhlali önleyen kural | **Terfi etti** | L3 `INSTRUCTIONS §2` ("Ortak kural"), L4 checklist §7 (6. adım) | Liste konu hücresi ⚠ taşıyan karar satırlarıyla her adımın kapanışında betikle eşitlenir; eksik ve fazla yazılır | PF-123 |
| Ö-131 | 6 | Kıyas tablosunun sonuç cümlesi aynı dokümanın uygulama bölümüyle çelişebilir; geri işaret okuması buldu | Tek seferlik gözlem (aday) → **tekrarlandı**, terfi | **Terfi etti** — adayın "raporda kalır" önerisinden ayrılır (aşağıdaki not) | L4 checklist §4 (yazım turunun özet ↔ ayrıntı taraması) | Tarama ölçüt ve kıyas tablolarının sonuç ve özet hücrelerini de uygulama bölümüne karşı okur | PF-124 |
| Ö-132 | 7 | Etki sütunu `DEPLOY_RUNBOOK`'u gösteren kararın kurulum işleri bloktaki işin metniyle okunmuyor | İhlali önleyen kural | **Terfi etti** | L4 checklist §6 (etki yansıtma maddesi) | Kararın her kurulum işi (izin, yetki, ayar, saklanan değer) için runbook bloğunda bir iş aranır; betik etikete değil işin metnine bakar | PF-125 |

**Ö-131'in sonucu adayın önerisinden ayrılır.** Aday, tek seferlik gözlem olarak raporda kalmayı öneriyordu. Checkpoint aynı sınıfı `08 §3.1` tablosunun özet hücrelerinde (C-8, C-9, C-10) yeniden buldu: sınıf aynı aşamada iki ayrı yerde çıktı ve ikisini de kalite döngüsünün altı adımı görmedi. İki gelişle "tek seferlik" ölçütü düştü. Terfi yalnız var olan §4 maddesini genişletir — yeni madde yok, `audit` skill'ine dokunulmadı (skill değişikliği proje sahibinin onayına bağlıdır).

### 2.2 Karar kaydında yaşayan süreç kararları

| # | Kararlar | Süreç kararı | Sonuç |
|---|---|---|---|
| S-1 | K-1233 | Üç çalışma düzeni (⚠ öneriyle kayıt, merge yetkisi, yönetici + alt ajan düzeni) Aşama 7 boyunca açık; arşiv işaretiyle sona erer, sonraki aşamanın açılışında yeniden sorulur | **Önceden terfi etmişti** — biçimi `INSTRUCTIONS §2`, §9; yetki süreli (H-1, H-14) |
| S-2 | K-1234 | Açılış: iskelet ve sıra, şablonun taşımadığı üç yer, kimlikler (`ENTn-mm`), girdilerin okunma derinliği, adlar, çıktının kapsamı | **Dokümanda yaşar** (`08` başlık notu); şablon kusurları playbook'ta (PF-117…PF-121); açık kararlar bölümü çözümü Ö-126 |
| S-3 | K-1235, K-1254 | Tek ödeme sağlayıcısı; sağlayıcının adının seçimi proje sahibinden yöneticiye bırakıldı, PayTR | **Dokümanda yaşar** — ürün kararı |
| S-4 | K-1236…K-1243, K-1245…K-1258 | Workshop ve yazım turu: e-posta altyapısı, kart iadesi, anahtar değişimi, bildirim şablonları, Google akışı | **Dokümanda yaşar** (`08` §2, §3, §4, §5); geri besleme tablosu `08 §5` |
| S-5 | K-1259…K-1274 | Audit ve deep review'un kararları | **Dokümanda yaşar** — ürün ve kalan risk kararları |
| S-6 | K-1275…K-1290 | Cross-review: ciddiyet ölçüsü 1. turdan, iki koşum biçimi (tek parça + altı parça), tekrar eden RET maddesi (H1 ve H4, 4. tur), dört turda TEMİZ | **Önceden terfi etmişti** — `cross-review` skill'i (ciddiyet ölçüsü; çıkış kuralı; parçalı koşum) |
| S-7 | K-1291…K-1296 | Etki yansıtma, çakışma taraması, checkpoint: kararlar özeti, park satırları, ikinci ödeme sağlayıcısının yol haritası adayı oluşu (K-1293), kıyasın düzeltilmesi (K-1294), kapı | **Dokümanda yaşar**; yöntem dersleri Ö-130…Ö-132 |

### 2.3 Bu adımda bulunan adaylar

| # | Aday | Sonuç |
|---|---|---|
| Ö-133 | **Heredoc ihlalleri.** Alt ajanlar kabukta heredoc yasağını yedi adımda ihlal etti (workshop, yazım 5, audit, deep review, cross-review 1. tur, etki yansıtma, çakışma taraması): boş heredoc, `python -I -`, `python - <<'EOF'` ve commit mesajı; yönetici bir kez. `INSTRUCTIONS §7` yalnız doküman düzenleyen betiği anıyordu; yasak yöneticinin geçici talimatında yaşıyordu | **Terfi etti** — L3 `INSTRUCTIONS §9` "Alt ajanların koşumu" (6): kabukta heredoc kullanılmaz, betik `Write` ile dosyaya yazılır, commit mesajı `git commit -F` ile verilir. **Genel hook'a engel eklenmesi proje sahibine sorulur (Soru 3)** — hook `~/.claude` altında, proje dışıdır. PF-126 |
| Ö-134 | Pre-commit sır guard'ı belirteç anlamındaki İngilizce sözcükte yanlış pozitif verdi (commit ve PR metinleri); alt ajanlar "belirteç" yazarak geçti | **Tek seferlik — raporda kalır.** Guard sır sızıntısı için bilinçli geniş tutulmuştur (`scripts/git-hooks/README.md` — atama biçimi); sözcük değiştirmek yasak olan hook dolanması değil, guard'ın metin eşiğine uymaktır. Guard gevşetmesi gerekirse L5 işi olarak sonraki fazda düşünülür |
| Ö-135 | Yöneticinin CI yoklama deseni boş `conclusion`'ı bitmiş saydı (çakışma taraması PR'ı #193; merge öncesinde yakalandı) | **Terfi etti** — L3 `INSTRUCTIONS §3.2` ("Sessizlik yeşil değildir"): boş `conclusion` bitmiş sayılmaz; yalnız `success` yeşildir. PF-126 |
| Ö-136 | Tek mesajda 20–21 doğrulayıcı başlatıldı; araç reddetti, yeniden başlatıldı (iş tekrarlandı) | **Raporda kalır** — kural var (`INSTRUCTIONS §9` (5), Ö-121); bu bir kural boşluğu değil uygulama kusuru. Kuralın metni aynen duruyor; tekrar ederse hook sorusu açılır |
| Ö-137 | Cross-review 2. tur Codex kullanım sınırı yüzünden düştü; devam eden ajan aynı turu yeniden koştu (bildirilmeyen ilk deneme) | **Tek seferlik — raporda kalır.** `cross-review` skill'inin "tur aynı girdilerle yeniden koşulur" kuralı (Faz 1 madde 1) ve kullanıcı hafızası H-8 kapsıyor (Ö-124'ün tekrarı) |
| Ö-138 | Checkpoint'te çürütülen sekiz bulgunun beşi kapanmış bir kararın bilinçli sonucuydu; cross-review 4. turda H1 ve H4 tekrar eden RET oldu | **Raporda kalır** — doğrulayıcılar karar kaydını ilk iş okuyor (CP07 §6); çıkış kuralı işledi, yeni kural gerekmedi |

### 2.4 Hafızadaki adaylar (kullanıcı hafızası, salt okunur)

| # | Not | Sonuç |
|---|---|---|
| H-1 | `asama1-kapanis-otomasyonu-yetkisi` | Önceden terfi etmişti (biçim — `INSTRUCTIONS §9`); yetki süreli — arşiv işaretinde "Aşama 7 kapandı, Kodlama Kılavuzu açılışında yeniden sorulur" satırı (6. adım) |
| H-2 | `basit-isleri-ucuz-modele-ver` | Önceden terfi etmişti (K-1141; K-1207, K-1232) |
| H-3 | `ci-beklerken-izleyiciye-guvenme` | Önceden terfi etmişti (`INSTRUCTIONS §3.2`); Ö-135 boş `conclusion` ayrıntısını ekledi |
| H-4 | `cursor-agent-cross-review-kosumu` | Önceden terfi etmişti (`cross-review` skill'i) |
| H-5 | `dokumanlari-isimleriyle-kisa-acik-yaz` | Önceden terfi etmişti (`INSTRUCTIONS §2`) — bu rapor ve §5 buna uyar |
| H-6 | `kisa-diyalog-tarzi-workshop-sorusu` | Önceden terfi etmişti (kısmen — `INSTRUCTIONS §2`) |
| H-7 | `limit-dolarsa-kaldigin-yerden-devam` | Hafızada kalır — kişisel çalışma tercihi (Ö-137 örneği) |
| H-8 | `sonraki-chat-mesaji` | Hafızada kalır — kişisel çalışma tercihi |
| H-9 | `toplu-dogrulama-sorusu-asama7` | **Soru 1**; cevap işlenince not silinir |
| H-10 | `toplu-onay-verdiginde-isaretle` | Önceden terfi etmişti (`INSTRUCTIONS §2`, `GUARDRAILS §6`) |
| H-11 | `toplu-oneride-evet-ret-sayilmaz` | Önceden terfi etmişti — §5'in dört sorusu ayrı numaralıdır |
| H-12 | `uzun-adimda-ara-durum-ver` | Önceden terfi etmişti (`INSTRUCTIONS §9`, K-1141) |
| H-13 | `workshop-sorusu-oncesi-template-taramasi` | Önceden terfi etmişti (`INSTRUCTIONS §2`, checklist §1) |
| H-14 | `yalniz-cok-kritik-konulari-sor` | Önceden terfi etmişti; mod süreli — arşiv işaretinde yeniden sorulur (6. adım) |
| H-15 | `yalniz-sonnet-haiku-low` | Önceden terfi etmişti (K-1232, `INSTRUCTIONS §9` (1)); Soru 1 istisnayı (bulgu doğrulaması) değiştirir |
| H-16 | `yazim-oturumunda-tek-baglam` | Önceden terfi etmişti (kısmen — checklist §4, `audit` skill'i); oturum ayarı notta kalır |

---

## 3. Aşama 7'nin maliyet ölçümü (K-1298)

> **Düzeltme (2026-10-29, K-1301).** Bu bölümün belirteç sayıları ajanların bitiş bildirimlerinden toplanmıştır; o alan ajan bağlamının son büyüklüğüdür ve her adımda bağlamın yeniden okunmasını saymaz. Kayıt dosyalarından ölçülen gerçek tüketim (`scripts/metrics/token-usage.js`): **işlenen toplam 692,3 M** (yönetici 64,3 M, alt ajanlar 628,0 M; ~660 M önbellekten yeniden okuma, ~32,2 M yeni metin). Aşağıdaki "~33 M" yeni metne denk düşer, toplam tüketim değildir. Karşılaştırma tablosunun Aşama 6 sütunu da aynı yöntemle toplandığından aynı sınırı taşır.

Kaynak: adım ajanlarının bitiş bildirimlerindeki yürütücü belirteç alanı (yürütücünün kendi belirteci) ve yürütücülerin raporlarında verdikleri alt ajan toplamları — **ölçüm değil, ajan çıktılarından toplanmış tahmin; değerler yaklaşıktır.** Yöneticinin (ana oturum, Opus) belirteci ölçülmedi. Hook günlüğü çağrıları ve modelleri sayar, belirteç saymaz.

### 3.1 Tablo

| Adım | PR | Yürütücü (Sonnet / low) | Alt ajanlar | Süre |
|---|---|---|---|---|
| Açılış | #177 | ~448k | — | ~21 dk |
| Workshop | #178 | ~251k | — | ~10 dk |
| Yazım 1 | #179 | ~297k | — | ~9 dk |
| Yazım 3 | #180 | ~293k | — | ~8 dk |
| Yazım 4 | #181 | ~329k | — | ~11 dk |
| Yazım 2 (Sipay) + düzeltme (PayTR) | #182 | ~315k + ~188k | — | ~14 + ~8 dk |
| Yazım 5 | #183 | ~353k | — | ~16 dk |
| Özet ↔ ayrıntı | #184 | ~302k | 1 Haiku `[arama]` ~23k (yönetici) | ~7 dk |
| Audit | #185 | ~274k | 9 Sonnet mercek ~1,65 M · 50 Opus/high doğrulayıcı ~4,17 M · 4 Sonnet uygulayıcı ~0,43 M | ~43 dk |
| Deep review | #186 | ~385k | 10 Sonnet mercek ~1,5 M · 71 Opus/high ~6,3 M · 1 Sonnet uygulayıcı ~0,1 M | ~40 dk |
| Cross-review 1. tur | #187 | ~220k | 6 Opus/high ~0,78 M · Codex 7 çağrı ~0,52 M | ~23 dk |
| Google teyidi | #188 | ~301k | — | ~13 dk |
| Cross-review 2. tur (limitle düşen ilk deneme + devam) | #189 | bildirilmedi + ~169k | 9 Opus/high ~0,77 M · Codex 7 çağrı | ~9 dk (devam) |
| Cross-review 3. tur | #190 | ~150k | 7 Opus/high ~0,6 M · Codex 7 çağrı | ~13 dk |
| Cross-review 4. tur | #191 | ~125k | 6 Opus/high ~0,5 M · Codex 7 çağrı | ~11 dk |
| Etki yansıtma | #192 | ~420k | — | ~25 dk |
| Çakışma taraması | #193 | ~436k | 18 Sonnet ~1,6 M · 44 Opus/high ~2,8 M | ~29 dk |
| Checkpoint | #194 | ~287k | 16 Sonnet ~2,5 M · 39 Opus/high ~2,6 M | ~24 dk |

**Toplamlar (öğrenim terfisi hariç):** Sonnet yürütücüler ~5,6 M; Sonnet mercek, okuyucu ve uygulayıcı ~7,9 M; Opus/high doğrulayıcı 232 ajan ~19,5 M; Haiku ~0,02 M; Codex 28 çağrı (ChatGPT hesabı, ayrı). Net süre toplamı ~334 dk (5 sa 34 dk), 19 yürütücü koşumu. Hook: bütün çağrılar etikete uygun modelde; 1 engel ("hedefli" sözcüğü, cross-review 1. tur).

**Sapmalar:** alt ajanlar heredoc yasağını tekrar tekrar ihlal etti (Ö-133); yönetici bir kez. Tek mesajda 20–21 doğrulayıcı (araç reddetti, yeniden başlatıldı; Ö-136). Pre-commit sır guard'ı belirteç anlamındaki İngilizce sözcükte yanlış pozitif verdi (Ö-134). Yöneticinin CI yoklama deseni boş `conclusion`'ı bitmiş saydı (#193; merge öncesi yakalandı; Ö-135).

### 3.2 Aşama 6'nın ölçümüyle karşılaştırma

Aşama 6 sütunu `PHASE6_LEARNING_PROMOTION.md` §3.1'dendir (toplam, öğrenim terfisi ve arşiv işareti hariç).

| Ölçüt | Aşama 6 (API tasarımı) | Aşama 7 (Entegrasyon) | Fark |
|---|---|---|---|
| Adım (yürütücü koşumu) | 27 | 19 | -8 |
| Net süre | 803 dk | ~334 dk | yüzde 58 düşük |
| Adım başına net süre | 29,7 dk | ~17,6 dk | yüzde 41 düşük |
| Adım ajanı (yürütücü) belirteci | ~10,7 M | ~5,6 M | yüzde 48 düşük |
| Yardımcı ajan belirteci (kayıtlı) | ~22,4 M | ~27,4 M | yüzde 22 yüksek |
| Toplam kayıtlı belirteç | ~33,1 M | ~33,0 M | aynı |
| Opus/high doğrulayıcı sayısı | 56 (yalnız son dönem) | 232 | — (karşılaştırılamaz) |
| Hedef çürütme oranı (çakışma taraması + checkpoint) | — | 16 / 83 bulgu (yüzde 19) | — |

**Okuma.** (1) Adım ajanı yükü ve süre belirgin düştü; yazım turu beş oturuma indi ve cross-review dört turda TEMİZ döndü (Aşama 6: beş tur). (2) **Toplam kayıtlı belirteç aynı kaldı; kaymanın nedeni doğrulayıcılar:** 232 Opus/high doğrulayıcı ~19,5 M, yani toplamın yüzde 59'u. Aşama 6'da Opus/high doğrulama yalnız son dönemde başladı (56 bulgu, bunların 47'si son üç adımda); Aşama 7'de aşamanın tamamında. (3) **Doğrulayıcı sayısı bulgu sayısına eşit** (bulgu başına bir; `INSTRUCTIONS §9` (1)); bir doğrulayıcının belirteçlerinin çoğu bulguya değil dokümanın ve karar kaydının okunmasına gidiyor — bu, Soru 1'in dayanağıdır. (4) Çakışma taraması ve checkpoint'te 83 bulgunun 16'sı (yüzde 19) çürütüldü: doğrulayıcı gerçekten ayıklıyor, ama Aşama 6'nın Opus/high verisine (56 bulgudan 16, yüzde 29) göre daha az; farkı bu iki aşamanın doküman büyüklüğü ve bulgu türü karıştırır, tek sayıya bağlanmaz.

**Karşılaştırmanın sınırı.** İki aşama farklı dokümanları (07 ve 08), farklı boyutlarda yazdı; yazım ve kalite döngüsü adımlarının sayısı farklıdır; Aşama 7'de matris yoktur (Aşama 6'da yedi matris oturumu vardı, 146 dk). Yardımcı ajan belirteçleri iki aşamada da ajanların kendi raporlarındaki tahmindir. Bu nedenle tablo "düzenin işe yaradığını" değil, "iki aşamanın ölçülebilen büyüklüklerini" gösterir.

---

## 4. Ö-23 — checklist skill'e dönüşür mü

**Sonuç: iki koşul karşılanıyor — dönüşüm proje sahibine yeniden sunulur (Soru 2); bu PR'da uygulanmadı.** Ölçüt iki koşuldur ve ikisi birlikte aranır (checklist başlık notu, L4; K-722, K-853). Değerlendirme betikle yapıldı:

| Koşul | Sonuç | Kanıt |
|---|---|---|
| **1. Checklist izlenebilirlik matrisi zorunlu bir aşamada da işletilmiş olmalı** | ✓ **Karşılandı** — Aşama 3, 5 ve 6'da işletildi. Aşama 7'de matris zorunlu değildi (`00 §C.1`); koşul geçmişte karşılanmıştır, Aşama 7 onu değiştirmez | `07 §1` (üçüncü işletim) |
| **2. Bir aşama kapanışında yapısal değişiklik — yeni adım ya da yeni madde — almamış olmalı** | ✓ **Karşılandı** — Aşama 7 boyunca ve bu terfide | `grep -c '^- \[ \]'`: Aşama 6'nın arşiv işaretinden sonra **67**; Aşama 7 boyunca değişmedi (`git log 9cb05e5..HEAD -- .claude/checklists/` boş); **bu terfiden sonra 67**. Değişen: **altı madde genişletmesi** — §1'de iki (girdi maddesi, şablon taraması), §4'te bir (özet ↔ ayrıntı taraması), §6'da bir (etki yansıtma), §7'de bir (6. adım). Adım sayısı aynı |

**Aşama 6'nın "ikinci koşul sağlanmaz" uyarısı geçersiz kaldı.** Aşama 6'nın kapanışında özet ↔ ayrıntı taramasının checklist §4'e yeni bir madde olarak girmesi (66 → 67) ikinci koşulu Aşama 6 için bozmuştu. Aşama 7 boyunca ve kapanışında yeni madde girmedi; **koşul yeniden sağlanıyor.** Soru 1 ya da Soru 3 onaylansa bile checklist'e yeni madde gerekmez (cevaplar `INSTRUCTIONS §9` ve `audit` skill'ine yazılır), bu yüzden Soru 2 diğer sorulardan bağımsızdır.

**Bu kez neyin farklı olduğu.** Proje sahibi üç kez dönmemesini seçti; son seçim *"Liste kalsın, her aşama sonunda yine sorulsun"* (K-1232). Aşama 7'nin yeni bilgisi: iki koşul matrissiz bir aşamada da bozulmadı; dönüşüm gerçek bir sorunu çözmüyor (liste yedi aşamadır çalışıyor) ve her aşama sonunda aynı soruyu sormanın kendisi bir maliyet. Önerim bu yüzden **seçenek 3**: liste kalır ve bu soru artık her aşama sonunda sorulmaz; checklist'in ölçütü başlık notunda durur, proje sahibi isterse dönüşümü o an açar. Seçenek 2 (bugünkü düzen) de geçerlidir ve proje sahibi önceki talimatını (*"her aşama sonunda yine sorulsun"*) sürdürmek isterse bu seçenektir. Üç seçenek Soru 2'dedir.

**Elenen — dönüşümü bu PR'da uygulamak:** yeni skill yaratmak yapısal değişikliktir ve onay ister (GUARDRAILS §3); `00 §C.7`'nin metni değişir (GUARDRAILS §2). **Elenen — soruyu sormadan kapatmak:** kapatma kararı proje sahibinindir.

---

## 5. Proje sahibinin cevabını bekleyen dört soru

> **Sonuç (2026-10-29, K-1300):** dört sorunun hepsi cevaplandı — Soru 1 seçenek 2 (düşük ve orta bulgular toplu, yüksek tek tek) · Soru 2 seçenek 2 (liste kalsın, her aşama sonunda yine sorulsun) · Soru 3 seçenek 2 (yalnız betik çalıştıran heredoc engellensin; yönetici genel hook'ta uyguladı) · Soru 4 seçenek 1 (üç ders `00 §N.1`'e yazılsın; `00` v1.0.9). "Uygulanacak metin" bölümleri olduğu gibi uygulandı (`INSTRUCTIONS §9`, `skills/audit` / `deep-review` / `cross-review` / `checkpoint`, `checklists/document-stage.md` başlık notu, `00 §N.1`).

> **Hepsi ayrı numaralı sorudur** (`INSTRUCTIONS §2`; K-722 → K-853: tek bir "evet" birden çok öneriye okunmaz). Uygulanacak metinler cevaptan sonra ayrı bir `docs:` PR'ında yazılır; hiçbiri aşamanın kapanışını engellemez, kurallar L3–L4'te yürürlükte. **Kapı:** arşiv işaretinden önce (K-437'nin 6. adımı), ⚠ listesiyle aynı mesajda. Her sorunun altında "Uygulanacak metin" çıplak biçimdedir; yönetici sade metni iletir.

### Soru 1 — Bulguların doğrulanması: küçük bulgular tek ajanda toplansın mı (Ö-129)

> **Sorun:** Bir denetimin bulduğu her sorun için, bulgunun gerçekten sorun olup olmadığına bakan ayrı bir güçlü ajan çalışıyor. Bu aşamada 119 bulgu böyle kontrol edildi ve yaklaşık 10 milyon belirteç tuttu; her ajanın belirteçlerinin büyük kısmı bulgunun kendisine değil, dokümanı ve kararları okumaya gidiyor. **Ne değişir:** Önerim, ağır bulgular tek tek kontrol edilmeye devam eder, orta ve küçük bulgular aynı bölümden gruplanıp birkaç bulgu tek ajana verilir (güçlü model aynı kalır). Bu aşamanın verisiyle 119 ajan yerine yaklaşık 40 ajan, yaklaşık 3,5 milyon belirteç. Risk: toplu okumada bir bulgu diğerinin gölgesinde kalabilir; bu yüzden kontrol eden ajan bir bulgunun ağırlığını yükseltirse bulgu yeni gruptaki yerinde yeniden kontrol edilir. Ucuz modele devretmek önerilmiyor: önceki aşamada ucuz model 186 bulgudan yalnız 4'ünü çürütmüştü.
> **1 ·** yalnız küçük (düşük) bulgular toplu kontrol edilsin · **2 ·** küçük ve orta bulgular toplu kontrol edilsin (öneri) · **3 ·** bugünkü kural: her bulgu için ayrı ajan

**Uygulanacak metin (1 ya da 2 ise):** `INSTRUCTIONS §9` "Alt ajanların koşumu"nun (1)'inde *"bulgu başına bir doğrulayıcıdır, ikinci doğrulayıcı açılmaz (K-1224)"* cümlesinin yerine: *"Yüksek seviyeli bulgu başına bir doğrulayıcı açılır; [seçenek 2 ise: orta seviyeli bulgular aynı bölümden 2–4'lü, ] düşük seviyeli bulgular aynı bölümden 5–8'li tek doğrulayıcıya verilir; model ve efor değişmez (Opus, `high`). Seviye merceğin işaretidir; doğrulayıcı bir bulgunun seviyesini yükseltirse bulgu yeni kademede yeniden doğrulanır, ikinci doğrulayıcı açılmaz (K-1224). **Neden:** Aşama 7'de 119 hüküm birimi (17 yüksek, 39 orta, 63 düşük) birer doğrulayıcıyla doğrulandı, ~10 milyon belirteç; kademeli düzen ~40 doğrulayıcı ve ~3,5 milyon belirteç tutardı (Aşama 7'nin öğrenim terfisi)."* `skills/audit` "Koşum biçimi" 2. madde aynı cümleyle; `deep-review` madde 7, `cross-review` Faz 2 ve `checkpoint` koşum paragrafı aynı atıfla. Checklist'e yeni madde girmez.

### Soru 2 — Kontrol listesi beceriye dönüşsün mü (Ö-23)

> **Sorun:** Proje boyunca izlediğimiz adımların listesi (kontrol listesi) şu an düz bir liste; aynı işi yapan bir "beceri" (skill) olarak da yazılabilir. Önceki üç aşamada bunu önerdim ve her seferinde listenin kalmasını seçtiniz; son seçiminiz her aşama sonunda soruyu yeniden sormamdı. Bu aşamada liste yine yeni adım almadı (67 madde yerinde kaldı) ve yine iyi çalıştı; dönüştürmeyi gerektiren bir sorun görmedim. **Ne değişir:** 1 derseniz liste beceriye çevrilir (metodoloji dokümanının bir cümlesi de değişir). 2 derseniz bugünkü düzen sürer ve soru bir sonraki aşama sonunda yine sorulur. 3 derseniz liste kalır ve soru artık her aşama sonunda sorulmaz; dönüşümü istediğiniz an siz açarsınız.
> **1 ·** liste beceriye dönüşsün · **2 ·** liste kalsın, her aşama sonunda yine sorulsun (bugünkü düzen) · **3 ·** liste kalsın, artık sorulmasın (öneri)

**Uygulanacak metin (1 ise):** ayrı `docs:` PR'ında yeni bir skill ve `00 §C.7`'nin cümlesi (GUARDRAILS §2, §3). **(3 ise):** checklist başlık notuna *"Aşama 7'nin öğrenim terfisinde (K-1297) proje sahibi sorunun artık her aşama sonunda sorulmamasına karar verdi; ölçüt bu notta kalır, dönüşümü proje sahibi isterse açar"* cümlesi ve `PF-32`'nin durumu. **(2 ise):** başlık notuna yalnız Aşama 7'nin sonucu eklenir.

### Soru 3 — Alt ajanların kabukta heredoc kullanmasını araçla engelleyelim mi (Ö-133)

> **Sorun:** Bir komutu dosya yerine doğrudan kabuğa metin vererek çalıştırma yöntemi (heredoc) bu projede yasak, çünkü metne sessizce bozuk karakter yazabiliyor. Buna rağmen alt ajanlar yedi adımda bunu yaptı (betik çalıştırırken ve commit mesajı yazarken); yasak şimdiye kadar yalnız benim her ajana verdiğim talimatta yaşıyordu. Bu PR'da yasağı projenin kural dosyasına (`INSTRUCTIONS`) yazdım, ama kural yazılı olduğu hâlde ajan unutabiliyor. **Ne değişir:** Araç düzeyinde engel, ajan yasağı unutsa bile komutu çalışmadan durdurur. Bu engel sizin kullanıcı ayarlarınızdaki genel komut koruyucusunda (proje dışında, bütün projelerinizi etkiler) olur; ben o dosyaya dokunmam, değişikliği siz ya da sizin onayınızla yapılır. Bütün heredoc'ları engellemek commit mesajı verme yöntemini de etkiler (`-F dosya` kullanılır). Yalnız betik çalıştırmayı engellemek commit'i etkilemez.
> **1 ·** tüm heredoc'lar engellensin (commit mesajı dahil) · **2 ·** yalnız betik çalıştırma (python, node, sh) engellensin (öneri) · **3 ·** engel eklenmesin, yasak kural dosyasında kalsın

**Uygulanacak metin:** proje tarafında bir şey gerekmez (kural `INSTRUCTIONS §9` (6)'da yazılıdır). Hook değişikliği `~/.claude/hooks/staging/` içinde yapılır, `deploy.js` ile yayına alınır ve testleri geçmelidir; proje dışıdır. Playbook'a PF-126 olarak girer.

### Soru 4 — Yöntem dokümanına Aşama 7'nin dersleri (`00 §N.1`)

> **Sorun:** Yöntem dokümanı (`00`) her aşamanın tekrarlanan derslerini kısa bir paragrafla tutuyor; Aşama 7 için henüz yok. **Ne değişir:** Evet derseniz aşağıdaki üç ders yöntem dokümanına yazılır (bir sonraki aşamaların kurallara bakmadan aynı hataya düşmemesi için); hayır derseniz dersler yalnız bu raporda ve kontrol listesinde kalır. Üç ders: (1) bir karşılaştırma tablosunun sonuç cümlesi, aynı dokümanın uygulama bölümüyle ayrışabiliyor ve altı denetim turu görmedi; (2) kararda yazılı bir kurulum işi, kurulum kılavuzunda eksik kalabiliyor — etiketi arayan tarama görmüyor, işin metnine bakan görüyor; (3) bir devir dizini kendi anındaki görüntüdür, bir aşamanın kararlarının hepsini taşımayabilir — kayıp çıkmadı çünkü eşleme dizinden değil kararlardan yapıldı.
> **1 ·** üç ders yöntem dokümanına yazılsın (öneri) · **2 ·** yazılmasın

**Uygulanacak metin (1 ise):** `00 §N.1`'e, Aşama 6 paragrafının altına: *"**Aşama 7 — Entegrasyon spesifikasyonları (2026-10-29).** Kurallar `.claude/checklists/document-stage.md` §1, §4, §6, §7, `.claude/INSTRUCTIONS.md` §2, §3.2, §9 ve rapor `Docs/CHECKPOINT_REPORTS/PHASE7_LEARNING_PROMOTION.md`. Tekrarlanacak desenler: 1. **Kıyas ve ölçüt tablosunun sonuç cümlesi uygulama bölümüyle ayrışır.** Bir sağlayıcı kıyasının sonuç cümlesi aynı dokümanın uygulama bölümüyle çelişti; audit, deep review ve beş cross-review turu görmedi, çakışma taramasının geri işaret okuması buldu; checkpoint aynı sınıfı bir özet tablosunda yeniden buldu. 2. **Kurulum işi, kararın anıldığı yerde değil kurulum kaydının metninde aranır.** Dört kurulum işi (izin, yetki, ayar) kararda yazılıydı ve kurulum kılavuzunda yoktu; etiketi arayan betik görmedi, işin metnine bakan buldu. 3. **Devir dizini anlık görüntüdür.** Etki sütununda dokümanı taşıyan 82 kararın 14'ü dizinlerde yoktu; eşleme etki sütunundan çıkarıldığı için kayıp çıkmadı."* `00` sürümü +1. Seçenek 2 de geçerlidir: üç dersin üçü kontrol listesinde zaten uygulanır; `00 §N.1` dönemin özetidir, kuralın evi değil.

---

## 6. Uygulanan terfiler — dosya ve bölüm

| Dosya | Bölüm | Değişiklik | Aday |
|---|---|---|---|
| `.claude/checklists/document-stage.md` | §1 — girdi dokümanları maddesi | Matrisin hücre değeri düzyazıdan ayrı sayılır; devirlerin eşleme kaynağı etki sütununun betikle taranması, dizin ikinci kaynak | Ö-127, Ö-128 |
| | §1 — şablonu tara maddesi | Şablonda açık kararlar bölümü yoksa çözüm hazırdır; sekiz şablon, yedi satır | Ö-126 |
| | §4 — yazım turunun son oturumu bitince özet ↔ ayrıntı taraması | Tarama ölçüt ve kıyas tablolarının sonuç ve özet hücrelerini de uygulama bölümüne karşı okur | Ö-131 |
| | §6 — etki yansıtma maddesi | Kararın her kurulum işi için runbook bloğunda iş aranır; betik etikete değil işin metnine bakar | Ö-132 |
| | §7 — 6. adım | ⚠ listesi konu hücresi ⚠ taşıyan satırlarla betikle eşitlenir | Ö-130 |
| `.claude/INSTRUCTIONS.md` | §2 — "Ortak kural" | Liste betikle eşitlenir; eksik ve fazla yazılır | Ö-130 |
| | §3.2 — "Sessizlik yeşil değildir" | Boş `conclusion` bitmiş sayılmaz | Ö-135 |
| | §9 — "Alt ajanların koşumu" (6) | Kabukta heredoc kullanılmaz; betik `Write` ile dosyaya yazılır, commit mesajı dosyadan verilir | Ö-133 |

Madde sayısı yerinde: `grep -c '^- \[ \]' .claude/checklists/document-stage.md` → 67 (başlangıç 67). Bölüm bazında değişen yok.

**L5 (hook, CI) elendi:** Ö-133'ün hook yanı proje dışıdır ve Soru 3'te sorulur; Ö-130 (liste eşitleme) betikle denetlenebilir ama doküman döneminin bir CI görevi olarak taşınması bugün gerekmiyor (liste her kapanışta elle sayılıyor ve çakışma taraması betikle eşitliyor). **Skill'ler değişmedi:** `audit` "Koşum biçimi" 2. madde yalnız Soru 1'in cevabına bağlı.

---

## 7. Playbook'a geri akış (K-649)

Liste 121 satırdan **127**'ye çıktı. Var olan satırlardan PF-32 güncellendi (Ö-23'ün Aşama 7 sonucu — `Onay bekliyor`). PF-117…PF-121 tekrar açılmadı (Ö-126 PF-122'de anılıyor).

| Satır | Öğrenim | Hedef |
|---|---|---|
| **PF-122** (yeni) | Eşleme kaynağı etki sütunu, matris hücre değeri ayrı sayım, şablonda açık kararlar bölümü çözümü (Ö-126, Ö-127, Ö-128) | checklist §1 |
| **PF-123** (yeni) | ⚠ listesinin betikle eşitlenmesi (Ö-130) | checklist §7, `INSTRUCTIONS §2` |
| **PF-124** (yeni) | Özet ↔ ayrıntı taramasının ölçüt tablolarını da kapsaması (Ö-131) | checklist §4 |
| **PF-125** (yeni) | Kurulum işi ile runbook işinin metin eşlemesi (Ö-132) | checklist §6 |
| **PF-126** (yeni) | Kabukta heredoc yasağı; boş `conclusion` (Ö-133, Ö-135) | `INSTRUCTIONS §9` (6), §3.2 |
| **PF-127** (yeni) | Toplu bulgu doğrulaması (Soru 1) ve Aşama 7'nin desenleri (Soru 4) | `INSTRUCTIONS §9` (1), `audit` skill'i, `00 §N.1` — `Onay bekliyor` |

---

## 8. Kayda ve hafızaya yansıma

- **Karar kaydı v0.192:** K-1297 (bu adımın sonucu: 40 kalemin dökümü, sekiz terfi, dört soru) ve K-1298 (Aşama 7'nin maliyet ölçümü; §3'ün tablosu). Her ikisi süreç kararıdır, ürün kuralına dokunmaz — ⚠ grubuna girmez. §18.1: öğrenim terfisi adayları maddesi [x]; aşama kapanışının adımları maddesi (5. adım koşuldu); Ö-23 maddesi değerlendirmeyle güncellendi ([ ] kalır, kapısı Soru 2'nin cevabı); **yeni açık madde** — proje sahibinin cevabını bekleyen dört soru (kapı: arşiv işaretinden önce).
- **Repo hafızası:** `MEMORY.md` Güncel Durum bu adımın hâline getirildi — önceki adımın (checkpoint) paragrafı `MEMORY_ARCHIVE.md`'ye taşındı (`INSTRUCTIONS §7`).
- **Kullanıcı hafızası — bu adımda yazılmadı; yönetici günceller:**
  - `toplu-dogrulama-sorusu-asama7.md` (H-9) — soru §5'te Soru 1 olarak hazırlandı; cevap işlenince not silinir.
  - `asama1-kapanis-otomasyonu-yetkisi.md` (H-1) ve `yalniz-cok-kritik-konulari-sor.md` (H-14) — arşiv işaretinde "Aşama 7 kapandı — Kodlama Kılavuzu açılışında yeniden sorulur" satırları (6. adımın işi).
- **Mekanik:** değişen dosyalarda CR 0 (`tr -cd '\r'`), kontrol karakteri 0; eklenen satırlarda geri başvuru artığı yok; düzenlemeler birebir değiştiren, her değiştirmede tam bir eşleşme isteyen betikle yapıldı ve betikler dosya yazma aracıyla yazıldı.
- **Model dağılımı:** bu adımı yürüten ajan Sonnet / `low`; alt ajan açılmadı, bulgu doğrulaması gerektiren iddia çıkmadı.

**Sırada:** K-437'nin 6. adımı — proje sahibine tek mesaj: çakışma taramasının otuz ⚠ kararı (hazır metin `PHASE7_CONFLICT_SCAN.md` §4.1) ve bu raporun §5'indeki dört soru, **her biri ayrı numaralı**; cevapların kayda işlenmesi (onaylananlar ayrı `docs:` PR'larında); ardından arşiv işareti (⚠ işaretlerinin dönüşümü; Aşama 7'nin kayıtları salt okunur; `08` ✓) ve Kodlama Kılavuzu aşamasına (Aşama 8) devir.

---

*Kaynak: K-437 (kapanış sırası, 5. adım) · `00 §K`, §C.7, §N · GUARDRAILS §2, §3 · karar kaydı §18.1 (aday listesi, Ö-23) · çakışma taraması §7 · CP07 §6 · K-1233…K-1296 (süreç kararları) · K-1224, K-1232 (modeller) · K-722, K-852, K-853, K-991, K-1143, K-1232 (Ö-23'ün ölçütü ve önceki kararları) · K-649 (playbook'a geri akış) · K-1297, K-1298 (bu adımın kararları) · `Docs/CHECKPOINT_REPORTS/PHASE6_LEARNING_PROMOTION.md` (biçim ve karşılaştırma tabanı).*
