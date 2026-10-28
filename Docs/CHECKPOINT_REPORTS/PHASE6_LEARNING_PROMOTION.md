# Aşama 6 — Öğrenim Terfisi

**Tarih:** 2026-10-28 | **Aşama:** 6 — API tasarımı | **Kapanış adımı:** K-437'nin 5. adımı (`00 §K`; `checklists/document-stage.md` §7 madde 5)
**Girdi:** karar kaydı v0.171 — §16.1'in on iki öğrenim adayı, bekletilen Ö-23, maliyet düzeni denemesi (K-1145; on dokuz madde ve ölçüm tablosu); Aşama 6'nın süreç kararları (K-1144…K-1229 içinden); [çakışma taraması](PHASE6_CONFLICT_SCAN.md) §7, [checkpoint](CP06_PHASE6_CHECKPOINT.md) §6; kalite döngüsünün raporları (`AUDIT_REPORTS/07_AUDIT.md`, `07_DEEP_REVIEW.md`, `CROSS_REVIEW_REPORTS/07_CROSS_REVIEW.md`); `Docs/PLAYBOOK_FEEDBACK.md` (PF-105…PF-108); kullanıcı hafızasının on altı notu — `origin/main` @ `2ad1658`
**Çıktı:** karar kaydı v0.172 · `.claude/checklists/document-stage.md` (altı madde genişledi, madde sayısı 66 yerinde), `.claude/INSTRUCTIONS.md` §9 · `.claude/memory/MEMORY.md`, `MEMORY_ARCHIVE.md` · `Docs/PLAYBOOK_FEEDBACK.md` 116 satır · karar K-1230 — `00`, `CLAUDE.md`, `SETUP.md`, API Tasarımı ve öteki dokümanlar, `GUARDRAILS.md`, `CONTEXT.md` ve skill'ler değişmedi

> **Ne işe yarar:** `00 §K` gereği aşama, öğrenimi yazılmadan kapanmaz. Bu rapor Aşama 6 boyunca kayda, raporlara ve hafızaya düşen her öğrenim adayının sonucunu yazar. Sekiz aday L3–L4'e terfi etti ve bu PR'da uygulandı. Maliyet düzeni denemesinin (K-1145) değerlendirmesi §3'te; checklist'in skill'e dönüşmesi §4'te; proje sahibine gidecek dokuz soru §5'te — her biri ayrı numaralı, sade dille, K numarasız. `00`, `CLAUDE.md` ve `SETUP.md` hedefli öneriler ve maliyet düzeninin terfi önerileri uygulanmadı (GUARDRAILS §2, §3; K-1145'in kapısı); cevap gelince ayrı `docs:` PR'ında uygulanır.

---

## 1. Sonuç

| Sonuç | Sayı | Adaylar |
|---|---|---|
| **Terfi etti — bu PR'da uygulandı** (L3, L4) | 8 | Ö-112, Ö-114, Ö-115, Ö-117, Ö-118, Ö-120, Ö-121, Ö-122 |
| **Playbook'ta ya da raporda yaşar** — tek seferlik gözlem ya da şablon düzeltmesi | 3 | Ö-113 (şablon — PF-108), Ö-119 (tek seferlik), Ö-123 (tek seferlik; PF-114) |
| **Proje sahibine sorulur** | 4 aday · 9 soru | Ö-116 (Soru 3) · Ö-23 (Soru 8) · maliyet düzeninin on dokuz maddesi (Soru 1–7) · `00 §N.1`'e Aşama 6'nın desenleri (Soru 9) |
| **Önceden terfi etmişti ya da dokümanda yaşar** — süreç kararları | 13 grup | S-1…S-13 (§2.2) |
| **Önceden terfi etmişti / hafızada kalır / sorulur** — kullanıcı hafızası | 16 not | H-1…H-16 (§2.4) |

Toplam altmış üç kalem: tracker §16.1'in on iki adayı, Ö-23, maliyet düzeninin on dokuz maddesi (M-1…M-19; §3), süreç kararlarının on üç grubu, bu adımda bulunan iki gözlem (Ö-124, Ö-125) ve kullanıcı hafızasından on altı not. `CLAUDE.md` ve `SETUP.md` için öneri yok — oturum başlangıcı ve katman kuralı aşamanın hiçbir öğrenimiyle çelişmiyor.

**Aşamanın asıl öğrenimi.** API Tasarımı, izlenebilirlik matrisi zorunlu üçüncü aşamaydı ve aşama bu kez maliyet düzeni denemesiyle yürüdü. Üç şey belirgin: (1) **Süre ve adım ajanının token'ı düştü** — açılıştan checkpoint'e 27 adım 13 sa 23 dk net sürdü (Veri modeli aşamasında aynı adımlar 22 sa 49 dk, §3); ama yardımcı ajanlar kayda geçen token'ın yaklaşık üçte ikisini taşıdı ve tabanda ölçülmediği için toplam token karşılaştırılamıyor. (2) **Bulguyu bulmak ucuz modelde yetti, bulguyu çürütmek yetmedi:** Sonnet doğrulayıcılar 186 bulgudan dördünü çürüttü (yüzde 2); Opus / `high` doğrulayıcılar 56 bulgudan 16'sını (yüzde 29). Aynı kaynaktan çıkmayan bulgularla yapılan bir karşılaştırma olduğu için kanıt değil işarettir (§3.3). (3) **Hook ile proje kuralı çatıştı ve çatışma ilk kez dolanılarak çözüldü** — ders `INSTRUCTIONS §9`'a yazıldı (Ö-117).

---

## 2. Adaylar ve sonuçları

**Kimlikler:** Ö-112…Ö-123 tracker §16.1'in on iki adayıdır (sırasıyla aday 1…12; Ö numaralandırması Aşama 5'in Ö-111'inden sürer). Ö-124, Ö-125 bu adımda bulunan gözlemlerdir. Ö-23 Aşama 1'den gelir. M-1…M-19 maliyet düzeninin on dokuz maddesidir (§3). S-1…S-13 Aşama 6'nın süreç kararlarının gruplarıdır, H-1…H-16 kullanıcı hafızasının on altı notudur — ikisi de bu rapora özgü numaralardır.

### 2.1 Karar kaydındaki adaylar (§16.1 ve çakışma taraması §7, checkpoint §6)

| # | §16.1 | Aday | Sınıf (`00 §K`) | Sonuç | Hedef katman ve dosya | Uygulanan metin (özet) | Playbook |
|---|---|---|---|---|---|---|---|
| Ö-112 | 1 | Bir dokümana bırakılan karar başka bir dokümanın park satırında durabilir — `08`'in K-906 · K-973 satırı ödeme sağlayıcısından dönüşü API Tasarımı'na bıraktı, `07`'nin blokları işi saymıyordu | İhlali önleyen kural | **Terfi etti** | L4 checklist §1 (girdi maddesi) ve §4 (devir taraması) | Eşleme hedefin adını bütün dokümanların park bloklarında arar; devir taraması da öyle | PF-109 |
| Ö-113 | 2 | API Tasarımı şablonu ayrı bir JSON API'yi varsayıyor | Tekrarlanacak desen değil — şablon kusuru | **Playbook'ta yaşar** | `07` şablonu (playbook) | Bu repoda uygulanışı K-1149 yazdı; şablonun kendisi değişmedi | PF-108 (teyit) |
| Ö-114 | 3 | Açılışın sayımı gövde cümlelerini yine dizinden aldı (19 yerine 20) | İhlali önleyen kural | **Terfi etti** | L4 checklist §1 | Açılışın "iş bırakan gövde cümlesi" sayısı bugünkü metinden betikle alınır | PF-109 |
| Ö-115 | 4 | Konvansiyon tablosu karar satırının genel cümlesini kopyalıyor; istisnalar sonraki kararlarla geliyor | İhlali önleyen kural | **Terfi etti** | L4 checklist §4 (özet tabloları maddesi) | Kapalı liste ve sayı taşıyan cümle her oturumun bitişinde betikle yeniden sayılır | PF-110 |
| Ö-116 | 5 | Alt ajanlar yalnız Sonnet / Haiku ve `low` (K-1207); `INSTRUCTIONS §9`'un koşum paragrafı Aşama 6'da uygulanmıyor ve deneme notu yok | Proje sahibinin kararı | **Kısmen uygulandı, kalıcılığı sorulur** | L3 `INSTRUCTIONS §9` | Paragrafa olgusal not girdi (K-1207 ve K-1224 dağılımı değiştirdi); kalıcı mı geçici mi sorusu Soru 3 | PF-115 |
| Ö-117 | 6 | Hook ile proje kuralı çatışınca dolanma yerine yöneticiye bildirim | İhlali önleyen kural | **Terfi etti** | L3 `INSTRUCTIONS §9` — "Alt ajanların koşumu" (4) | Alt ajan çatışmayı dolanmaz, kurala uygun yeniden çağırır ve bildirir | PF-113 |
| Ö-118 | 7 | Deep review'un düzeltmesi gövdeye karar satırı olmadan kural ve devir yazabilir | İhlali önleyen kural | **Terfi etti** | L4 checklist §6 (etki yansıtma maddesi) | Gövde cümleleri K numarasından bağımsız taranır; her biri için K ve park satırı aranır | PF-111 |
| Ö-119 | 8 | Geri işaret taramasının çiftlerinin çoğu kuralı değiştirmeyen atıf | Tek seferlik gözlem | **Raporda kalır** | — | 402 çiftten 332'si eski satırda yeni numarayı taşımıyordu; 41 aday satırdan 18 kümede 9 gerçek eksik geri işaret ve 1 eksik hâl çıktı, 8 küme çürüdü | — |
| Ö-120 | 9 | Park etiketi ↔ etki sütunu betiği etiketi okur, metni okumaz | İhlali önleyen kural | **Terfi etti** | L4 checklist §6 (etki yansıtma maddesi) | Betik iki yönlüdür, doküman numarasını parça başı kalıbıyla arar, madde listelerini dışlar | PF-111 |
| Ö-121 | 10 | Bir mesajda yirmiden fazla alt ajan başlatılamaz | İhlali önleyen kural | **Terfi etti** | L3 `INSTRUCTIONS §9` — "Alt ajanların koşumu" (5) | Tek mesajda en çok yirmi çağrı; doğrulayıcılar yirmişerli dalgalarla | PF-113 |
| Ö-122 | 11 | Karar kaydının tablo hücresindeki kod aralığında kaçışsız çubuk satırı böler | İhlali önleyen kural | **Terfi etti** | L4 checklist §3 (satır biçimi maddesi) | Tabloda çubuk her zaman kaçışlı yazılır; betikler kaçışlıları sayıdan çıkarır | PF-112 |
| Ö-123 | 12 | Matris aday listesinin devir hâli olduğu `07 §1.1.2`'de yazılı değildi | Tek seferlik; şablon notu | **Raporda kalır; playbook'a satır** | `07` şablonu §1 notu (playbook) | Altı mercek bulgusu bundan çıktı, dördü çürüdü; şablona tek cümle önerilir | PF-114 |

### 2.2 Karar kaydında yaşayan süreç kararları

| # | Kararlar | Süreç kararı | Sonuç |
|---|---|---|---|
| S-1 | K-1144 | Üç çalışma düzeni (⚠ öneriyle kayıt, merge yetkisi, yönetici + alt ajan düzeni) Aşama 6 boyunca açık; arşiv işaretiyle sona erer, Entegrasyon spesifikasyonları aşamasının açılışında yeniden sorulur | **Önceden terfi etmişti** — biçimi `INSTRUCTIONS §2`, §9 (K-648, K-992); yetki süreli (H-1) |
| S-2 | K-1145 | Maliyet düzeni denemesi (19 madde) | **§3** |
| S-3 | K-1146…K-1150 | Açılış: iskelet, yazım konvansiyonlarının ve geri besleme tablosunun yeri, kimlikler, şablonun uç/DTO/hata bölümlerinin uyarlanışı, matrisin kaynak birimi | **Dokümanda yaşar** (`07` başlık notu); şablon kusurları playbook'ta (PF-105…PF-108) |
| S-4 | K-1151…K-1161 | Matrisin birimi, alt bölüm haritası, eşleme okunuşları, geri izlenebilirlik, GAP-14 | **Önceden terfi etmişti** — checklist §2 (birim kalıp dışı içeriği sayar, harita sabitlenir); GAP-14 dokümanda |
| S-5 | K-1162…K-1172 | Workshop: konvansiyonlar, kimlik ve erişim, listeler, uçlar, DTO envanteri | **Dokümanda yaşar** (`07 §2`, §4, §5) — ürün ve biçim kararları |
| S-6 | K-1173…K-1198 | Yazım turu: konvansiyonlar, oturum bölünmesi (K-1177, K-1187), devir betiğinin anlık görüntü okuması (K-1197) | **Önceden terfi etmişti** — checklist §4 (oturum tahmini gözlenen yükten; dizin anlık görüntüdür) |
| S-7 | K-1199…K-1206 | Audit'in kararları | **Dokümanda yaşar** — ürün ve kalan risk kararları |
| S-8 | K-1207 | Alt ajanlar yalnız Sonnet / Haiku, `low` | **Ö-116 — Soru 3** |
| S-9 | K-1208…K-1217 | Deep review'un kararları | **Dokümanda yaşar** |
| S-10 | K-1218…K-1223 | Cross-review: ciddiyet ölçüsü ilk turdan, iki koşum biçimi, tek parça çağrının yerine iki birleşim, çıkış kuralıyla kapanış (beş tur) | **Önceden terfi etmişti** — `cross-review` skill'i (ciddiyet ölçüsü; Faz 1 madde 9; Faz 4 madde 5). 5. turun kullanım sınırı nedeniyle dokuz gün sonra yeniden koşulması ve betiklerin 1. turun raporundaki tarifle yeniden kurulması: Ö-124 |
| S-11 | K-1224 | Bulgu doğrulaması Opus / `high`; hook'un dolanılması | **Ö-117 uygulandı; kalıcılık Soru 2** |
| S-12 | K-1225, K-1226 | Etki yansıtma: atlanmış yüzler, kararlar özeti, park satırlarının iki yönü | **Önceden terfi etmişti** — Aşama 5'in 6. deseni; checklist §3, §6 |
| S-13 | K-1227…K-1229 | Çakışma taraması ve checkpoint'in kararları | **Dokümanda yaşar**; yöntem dersleri Ö-118…Ö-122 |

### 2.3 Bu adımda bulunan adaylar

| # | Aday | Sonuç |
|---|---|---|
| Ö-124 | Cross-review'ın 5. turu Codex kullanım sınırı yüzünden dokuz gün sonra koştu; önceki betikler silinmişti ve 1. turun raporundaki tarifle yeniden kuruldu (istem, parça notu ve şablonun SHA-256'ları aynı çıktı) | **Tek seferlik — raporda kalır.** Tarifin raporda durması yetti; `cross-review` skill'inin "tur aynı girdilerle yeniden koşulur" kuralı (Faz 1 madde 1) doğrulandı |
| Ö-125 | Ölçüm tablosunun elle taşınması: her adımın satırı sonraki adımın görev metnine verildi, 27 PR'a yayıldı; üç adımın alanı "yönetici ölçer" kaldı ve cross-review turlarının Sonnet alt ajan token'ları hiç kaydedilmedi | **Soru 7'nin girdisi** — ölçümün kalıcı olup olmayacağı |

### 2.4 Hafızadaki adaylar (kullanıcı hafızası, salt okunur)

| # | Not | Sonuç |
|---|---|---|
| H-1 | `asama1-kapanis-otomasyonu-yetkisi` | Önceden terfi etmişti (biçim — `INSTRUCTIONS §9`); yetki süreli — arşiv işaretinde "Aşama 6 kapandı, Entegrasyon açılışında yeniden sorulur" satırı (6. adım) |
| H-2 | `asama6-maliyet-duzeni-denemesi` | §3; terfiden sonra silinir (cevaplar gelince) |
| H-3 | `basit-isleri-ucuz-modele-ver` | Önceden terfi etmişti (K-1141); Aşama 6'da K-1207 dağılımı değiştirdi — Soru 3 |
| H-4 | `ci-beklerken-izleyiciye-guvenme` | Önceden terfi etmişti (`INSTRUCTIONS §3.2`) |
| H-5 | `cursor-agent-cross-review-kosumu` | Önceden terfi etmişti (`cross-review` skill'i); Ö-124 not edilebilir |
| H-6 | `dokumanlari-isimleriyle-kisa-acik-yaz` | Önceden terfi etmişti (`INSTRUCTIONS §2`) — bu rapor ve §5 buna uyar |
| H-7 | `kisa-diyalog-tarzi-workshop-sorusu` | Önceden terfi etmişti (kısmen — `INSTRUCTIONS §2`) |
| H-8 | `limit-dolarsa-kaldigin-yerden-devam` | Hafızada kalır — kişisel çalışma tercihi |
| H-9 | `sonraki-chat-mesaji` | Hafızada kalır — kişisel çalışma tercihi |
| H-10 | `toplu-onay-verdiginde-isaretle` | Önceden terfi etmişti (`INSTRUCTIONS §2`, `GUARDRAILS §6`) |
| H-11 | `toplu-oneride-evet-ret-sayilmaz` | Önceden terfi etmişti — §5'in dokuz sorusu ayrı numaralıdır |
| H-12 | `uzun-adimda-ara-durum-ver` | Önceden terfi etmişti (`INSTRUCTIONS §9`, K-1141) |
| H-13 | `workshop-sorusu-oncesi-template-taramasi` | Önceden terfi etmişti (`INSTRUCTIONS §2`, checklist §1) |
| H-14 | `yalniz-cok-kritik-konulari-sor` | Önceden terfi etmişti; mod süreli — arşiv işaretinde yeniden sorulur (6. adım) |
| H-15 | `yalniz-sonnet-haiku-low` | Ö-116 — Soru 3 (ve K-1224'ün istisnası Soru 2) |
| H-16 | `yazim-oturumunda-tek-baglam` | Önceden terfi etmişti (kısmen — checklist §4, `audit` skill'i); oturum ayarı notta kalır |

---

## 3. Maliyet düzeni denemesinin değerlendirmesi (K-1145, madde 19)

### 3.1 Ölçüm — Veri modeli aşamasının tabanıyla

Taban (hafıza notu, K-1145'in karar hücresi): Veri modeli aşaması **23 sa 34 dk net, 28 adım**; adım başına net dakika ve **yönetici seviyesinde görülen** ajan token'ı. API Tasarımı sütunu tracker §16.1'in ölçüm tablosundandır (hesap: `olcum.py`, tablodan alınan 27 satırın toplamı; checkpoint satırı yönetici ölçümüyle doldu — 34 dk, 393 869 token).

| Adım | Taban: net dk (adım) | Taban: token | API Tasarımı: net dk (adım) | Adım ajanı token | Yardımcı ajan token (kayıtlı) |
|---|---|---|---|---|---|
| Açılış | 22 (1) | 449 k | 26 (1) | 422 640 | yok |
| Matris | 160 (6) | ~3,3 M | 146 (7) | 2 869 122 | 103 686 |
| Workshop | 77 (2) | ~1,24 M | 31 (1) | 555 294 | yok |
| Yazım | 281 (7) | ~5,1 M | 181 (6) | 3 373 541 | yok |
| Özet ↔ ayrıntı taraması ve hükmü (yeni) | — | — | 48 (2) | 703 402 | 1 724 500 |
| Audit (+ güvence merceği) | 310 (1) | 539 k | 121 (1) | 354 344 | 6 850 000 |
| Deep review | 109 (1) | 411 k | 82 (1) | 392 495 | 5 350 000 |
| Cross-review (5 tur) | ~96 (5) | ~1,74 M | 83 (5) | 969 209 | yalnız sayı (Sonnet token kaydı yok); Codex ayrı ~7,2 M |
| Etki yansıtma | 54 (1) | 616 k | 25 (1) | 330 513 | 295 000 |
| Çakışma taraması | 81 (1) | 574 k | 26 (1) | 358 245 | 3 050 000 |
| Checkpoint | 179 (1) | 642 k | 34 (1) | 393 869 | 5 010 000 |
| **Toplam — öğrenim terfisi ve arşiv işareti hariç** | **1 369 (26)** | **~14,6 M** | **803 (27)** | **10 722 674** | **22 383 186** |
| Öğrenim terfisi | 27 | 466 k | bu adım — sonraki adımın görev metninde ölçülür | | |
| Arşiv işareti ve devir | 18 | 371 k | sonraki adım | | |

**Okuma.** Net süre yüzde 41 düştü (1 369 → 803 dk; adım başına 52,7 → 29,7 dk). Adım ajanı token'ı yüzde 27 düştü (14,6 M → 10,7 M). **Toplam token karşılaştırılamaz:** Aşama 6'da kayıtlı toplam 33,1 M'dir ve yüzde 68'i yardımcı ajanlardadır; tabanda yardımcı ajanlar sayılmadı (aşağıda 3.4). Yönetici hazırlık satırı (~11 dk) API Tasarımı toplamında yok. Net süre Codex kullanım sınırı bekleyişini (5. tur, 2026-10-19 → 2026-10-28) içermez.

### 3.2 İki ayrı dönem

K-1207 audit'ten sonra geldi, K-1224 beş gün sonra: ölçüm üç dönemi ayrı gösterir.

| Dönem | Adımlar | Net dk | Adım ajanı | Adım ajanı token | Yardımcı ajan token (kayıtlı) |
|---|---|---|---|---|---|
| **1 — K-1145 dağılımı** (2026-10-18 açılış → audit, 2026-10-19) | Açılış, matris 7, workshop, yazım 6, özet taraması ve hükmü, audit — 18 satır | 553 | Opus / medium (özet taraması: Sonnet / medium) | 8 278 343 | 8 678 186 — audit: 16 Opus / medium mercek ~4,14 M, 8 Sonnet partisi ~1,00 M, 3 Opus ikinci doğrulayıcı partisi ~0,45 M, 1 Opus / `high` ~0,13 M, 6 Sonnet uygulayıcı ~0,95 M; özet taraması: 19 Sonnet / medium okuyucu ~1,72 M |
| **2 — K-1207** (deep review → cross-review 5. tur, 2026-10-19 → 2026-10-28) | Deep review, cross-review 5 tur — 6 satır | 165 | Sonnet / low | 1 361 704 | deep review 5 350 000 (15 mercek ~2,60 M, 12 doğrulayıcı partisi ~1,41 M, 9 uygulayıcı ~1,14 M); cross-review: 54 Sonnet / low alt ajan, token kaydı yok; Codex ayrı |
| **3 — K-1224** (etki yansıtma, çakışma taraması, checkpoint, 2026-10-28) | 3 satır | 85 | Sonnet / low | 1 082 627 | 8 355 000 — 33 Sonnet / low mercek ve okuyucu ~4,4 M, **47 Opus / `high` bulgu doğrulayıcı ~3,97 M** (çakışma taraması 20, checkpoint 27) |

Dönem 1'in adım ajanları Opus idi ve 553 dakikanın 522'si Opus / medium adımlardı (özet ↔ ayrıntı taraması Sonnet / medium); dönem 2 ve 3'te adım ajanı Sonnet / low, doğrulama yalnız dönem 3'te Opus / `high`. Adım türlerini birebir eşleyen bir çift yok (aynı iş iki modelde koşmadı); tek doğrudan karşılaştırma, tabanda Opus ile koşan adımların API Tasarımı'nda Sonnet / low ile koşan karşılığıdır: deep review 109 → 82 dk, etki yansıtma 54 → 25, çakışma taraması 81 → 26, checkpoint 179 → 34.

### 3.3 Kalite göstergeleri — ve neden kanıt sayılmazlar

| Adım | Bulgu arayan | Doğrulayıcı | Sonuç |
|---|---|---|---|
| Audit (dönem 1) | 16 Opus / medium mercek | Sonnet / medium; 25 bulguya Opus / medium ikinci | 89 bulgu: 64 doğru, 23 kısmen, **2 çürüdü**; doğrulanmış seviye High 4 · Medium 17 · Low 66 |
| Deep review (dönem 2) | 15 Sonnet / low mercek | Sonnet / low; 38 bulguya Sonnet / low ikinci | 97 ham bulgu: 47 doğru, 48 kısmen, **2 çürüdü**; doğrulanmış (95) High 7 · Medium 26 · Low 62 |
| Çakışma taraması (dönem 3) | 14 + 1 Sonnet / low okuyucu | **Opus / `high`**, bulgu başına bir | 20 aday: 4 doğru, 8 kısmen, **8 çürüdü** |
| Checkpoint (dönem 3) | 15 Sonnet / low mercek | **Opus / `high`**, bulgu başına bir | 36 ayrı bulgu: 20 doğru, 8 kısmen, **8 çürüdü** |

**İşaret:** Sonnet doğrulayıcılar 186 bulgudan 4'ünü çürüttü (yüzde 2); Opus / `high` doğrulayıcılar 56 bulgudan 16'sını (yüzde 29). **Kanıt değil:** bulgu kaynakları farklıydı (dönem 3'ün okuyucuları dar dilimlerden "aday" çıkarıyordu ve çürüme oranının yüksek olması beklenir), bulgular aynı metne karşı ikinci kez doğrulanmadı ve doğrulayıcıların hangi bulguyu yanlış onayladığını gösteren bir kontrol yok. Elde olan şu: Sonnet / low merceği Opus / medium merceğinden **daha az** bulgu getirmedi (97 ham bulguya karşı 89) ve daha fazla High buldu (7'ye karşı 4; katmanlar farklı: audit kapsama ve tutarlılık, deep review teknik derinlik ve güvenlik). Merceğin modelini düşürmek bulgu sayısını azaltmadı; doğrulayıcıyı düşürmek çürütme gücünü azalttığına dair işaret verdi.

### 3.4 Tabanın ölçüm boşluğu

Taban, Aşama 5'te **yardımcı ajanları saymadı**: audit satırı "310 dk / 539 k (+19 mercek, 20 şüpheci, hepsi Opus)" der ve mercekler ile şüphecilerin token'ı yoktur; deep review, cross-review ve checkpoint için de yalnız yönetici seviyesindeki ajan sayıldı. Aşama 6'da audit'in yardımcıları ~6,85 M, deep review'unki ~5,35 M, checkpoint'inki ~5,01 M token'dır — adım ajanının kendi token'ının 12 ila 19 katı. Bu yüzden **toplam token karşılaştırması yapılamaz** ve rapor yapmıyor: Aşama 5'in gerçek toplamı bilinmiyor, tabanın gösterdiği ~14,6 M alt sınırdır. Aşama 6'nın kendi eksiği de var: cross-review turlarında 54 Sonnet alt ajan ve üç adımın ölçüm alanı ("yönetici ölçer") başta boştu; checkpoint satırı bu adımda doldu, cross-review'ın alt ajan token'ları kayıtsız kaldı. Karşılaştırma bu yüzden **süre** ve **adım ajanı token'ı** üstünden kuruldu.

**Başka kısıtlar.** (a) Token fiyatı modele göre değişir; bu rapor para ölçmez. (b) Aşama 6'nın süresindeki düşüş yalnız maliyet düzeninden gelmiyor: ikinci kez işletilen yönetici düzeni, cross-review'ın çıkış kuralı (beş tur, Aşama 5'te de beş), matrisin ikinci kez işletilmesi ve doküman büyüklüğü de etkili — hiçbirinin payı ayrıştırılamıyor. (c) Tek aşamadır.

### 3.5 On dokuz maddenin hükmü

Kolon "Terfi önerisi": hedef dosya ve sorunun numarası (§5). "Düşer": gerekçeyle bırakılır.

| # | Madde | İşe yaradı mı — kanıt | Hüküm |
|---|---|---|---|
| M-1 | Yazım turunun son oturumundan sonra, kalite döngüsünden önce özet ↔ ayrıntı taraması | **Kısmen.** 56 bulgu buldu: 38 düzeltildi, 6 çürüdü, 12 hükme kaldı (hükümden 4'ü konvansiyon-tablosu sınıfı — Ö-115). Maliyeti 48 dk, ~2,4 M token. Sınıfı kapatmadı: K-1129'dan devredilen sınıf (iki yerin metin farkı) checkpoint'te `07`'de ve geri beslenen dokümanlarda yine bulundu (20 doğru, 8 kısmen doğru bulgu; K-1229) | **Terfi önerisi — Soru 1** (checklist §4 yeni madde) |
| M-2 | Matris / kaynak eşleme tablosu yazım bittikten sonra çıktıya karşı yeniden okunur | **Evet** — M-1'in kapsamındaydı: matris örneklemi (84 satır, sabit tohum) ve §1.1/§1.2 hücreleri okundu, "Uç" hücreleri ve 2.648 ileri satır betikle ayrıntıya eşlendi. Aşama 5'in Ö-103'ünün (matris çıktıdan sonra okunur) bu aşamada işletilmiş hâli | **Terfi önerisi — Soru 1** (M-1 ile aynı madde) |
| M-3 | Audit, M-1'in PR'ı merge edilmeden başlamaz | **Evet, maliyetsiz** — PR #163 ve #164 merge edildi, audit #165'te başladı; sıra gecikme yaratmadı. Checklist §6 zaten "bir doküman TEMİZ olmadan sonrakine geçilmez" der; ek yük sıfır | **Terfi önerisi — Soru 1** (M-1 ile aynı soru) |
| M-4 | Bulgu arayan mercek Opus / `medium` | **Düşer.** Dönem 2'nin Sonnet / low mercekleri daha az değil daha çok ham bulgu getirdi (§3.3); K-1207 yerine geçti | Düşer — Soru 3 K-1207'nin kalıcılığını sorar |
| M-5 | Her bulguyu önce tek bir Sonnet doğrular | **Hayır.** 186 bulgudan 4'ünü çürüttü (yüzde 2); yüksek ve orta önemli ya da çürütülen 63 bulgu (25 + 38) ikinci doğrulayıcıya da gitti — iş ikiye katlandı. K-1224 yerine geçti | Düşer — Soru 2 |
| M-6 | İkinci doğrulayıcı Opus, yalnız orta/yüksek ya da şüpheli bulguya | **Düşer.** Audit'te 25 bulguya üç parti (~0,45 M) açıldı ve 7 bulguda iki doğrulayıcı ayrıştı; K-1207 ikinci doğrulayıcıyı Sonnet yaptı, K-1224 kaldırdı. Bulgu başına tek Opus / `high` doğrulayıcı daha basit ve hook'la uyumlu | Düşer — Soru 2 |
| M-7 | Sonnet'in doğruladığı düşük bulgular dosya başına gruplanıp tek Sonnet'le toplu uygulanır | **Evet, daha iyi biçimiyle.** Audit'te 6, deep review'da 9 uygulayıcı (86/89 ve 91/95 uygulandı). Checkpoint'te uygulayıcı ajan açılmadı: doğrulayıcının yazdığı birebir şartname **betikle** uygulandı — 61 şartname parçasının hiçbiri çakışmadı (CP06 Notlar) | **Terfi önerisi — Soru 4** (`audit` skill'i "Koşum biçimi" 2. madde) |
| M-8 | Doğrulayıcılar ayrışırsa son hüküm adım ajanında | **Düşer.** K-1224 ayrışmayı kaldırdı. Audit'te 7 bulguda uygulandı; `audit` skill'inin 2. maddesi ("ayrışırsa yöneticinin seçimi kayda yazılır") zaten bunu yazar | Düşer |
| M-9 | Mekanik iş betikle; betik yazılamıyorsa Haiku / `low` | **Evet.** Açılış, matris, workshop ve yazım adımlarının 15'inden 14'ü alt ajansız koştu, sayımlar betikle; çakışma taraması ve checkpoint'in sürüm, K tekilliği, K atıfı, UC kapsaması, hata kodu, CR / kontrol karakteri kontrolleri betikle temiz geçti; Haiku hiç gerekmedi. `INSTRUCTIONS §9`'un (1)'i yalnız "ucuz model" der, "önce betik" demez | **Terfi önerisi — Soru 5** (`INSTRUCTIONS §9` (1)) |
| M-10 | Düzeltmenin işlenmesi, rapor ve karar satırı taslağı Sonnet / `medium` | **Düşer** (efor `low` oldu, K-1207). Sonnet / low uygulayıcılar yetti: deep review'da 95 doğrulanmış bulgunun 91'i uygulandı, 2'si kısmen, 2'si karşılandı | Düşer — Soru 3 kapsar |
| M-11 | Adım ajanı: yönetici Sonnet, açılış / workshop / yazım / audit / çakışma / checkpoint / öğrenim Opus / `medium` | **Düşer.** Dönem 2–3'te aynı iş türleri Sonnet / low ile koştu: süre düştü (§3.2) ve kalite göstergesi kötüleşmedi (§3.3); ancak aynı iş iki modelde koşmadığından bu kanıt değil işarettir | Düşer — Soru 3 |
| M-12 | `high` efor yalnız üç iş (kritik soru, yasal dayanaklı bulgu, ayrışan bulgu) | **Düşer.** Audit'te Opus / `high` yalnız üç yasal dayanaklı bulgunun hükmünde koştu (mevzuat resmî metinden yeniden okundu — ki bu kural `audit` skill'inin 3. maddesinde zaten yazılı); sonraki dönemde `high` doğrulayıcıya verildi (K-1224) | Düşer — Soru 2 ve 3 kapsar |
| M-13 | `xhigh` ve `max` kullanılmaz | **Uygulandı, hiç kullanılmadı** — efor `low` / `high` ile sınırlıydı | Terfi gerekmez — Soru 3'ün cevabı eforu belirler |
| M-14 | Her alt ajan raporunun son satırı model ve eforu yazar | **Evet** — her adımın raporunda vardı ve ölçüm tablosunu mümkün kıldı. `INSTRUCTIONS §9` (1) "adımın raporu hangi işin hangi modelle koştuğunu bir satırla yazar" zaten der (K-1141) | **Önceden terfi etmişti** |
| M-15 | Her adımdan önce ihtiyaç duyulan metin ayrı dosyaya çıkarılır | **Evet.** Audit'in 16 merceği ve checkpoint'in 15 merceği önce kendi dilim dosyasını (satır numaralı) okudu ve bulgusunu bulduğu anda dosyaya yazdı; çakışma taramasının 15 okuyucusu da dilimlerle koştu | **Terfi önerisi — Soru 6** |
| M-16 | Mercek ve doğrulayıcı önce bu dosyayı okur; tam doküman yalnız dayanak dışarıdaysa açılır | **Evet** — M-15 ile aynı düzen; checkpoint'te 27 doğrulayıcı güncel metne karşı yeniden okudu | **Terfi önerisi — Soru 6** |
| M-17 | Karar kaydı hiçbir ajana bütünüyle okutulmaz | **Evet.** Dosya 3,96 MB'tır; hiçbir ajan bütününü okumadı, `grep` / `sed` / betikle bölüm bölüm okundu. Bu kural `GUARDRAILS`'ta ya da `INSTRUCTIONS`'ta yazılı değil; yalnız ortak talimatta yaşıyor | **Terfi önerisi — Soru 6** |
| M-18 | Her adımın sonunda net süre, token, alt ajan sayısı ve modelleri ilerleme notuna yazılır | **Evet, ama pahalı.** Bu karşılaştırmayı mümkün kıldı; bedeli: 27 PR'a yayılan elle taşıma, üç adımın boş kalan alanı ve cross-review alt ajanlarının kayıtsız kalması (Ö-125) | **Terfi önerisi — Soru 7** (önerim: kalıcı yapma) |
| M-19 | Aşama kapanışında tablo tabanla karşılaştırılarak proje sahibine verilir | **Bu rapor.** Tek seferlik; tablo ve hükümler §3 ve §5'tedir | Tamam — kalıcı kural değil |

---

## 4. Ö-23 — checklist skill'e dönüşür mü

**Sonuç: iki koşul üçüncü kez karşılandı — dönüşüm proje sahibine yeniden sunulur (Soru 8); bu PR'da uygulanmadı.** Ölçüt iki koşuldur ve ikisi birlikte aranır (checklist başlık notu, L4; K-722, K-853). Değerlendirme betikle yapıldı:

| Koşul | Sonuç | Kanıt |
|---|---|---|
| **1. Checklist izlenebilirlik matrisi zorunlu bir aşamada da işletilmiş olmalı** | ✓ **Karşılandı** — Aşama 3'te ilk, Aşama 5'te ikinci kez; şimdi üçüncü | `07 §1`: yedi oturum (K-1150…K-1160), kaynak birimi envanterden önce kayda geçti, envanter betikle çıkarıldı, GAP-14 karara bağlandı |
| **2. Bir aşama kapanışında yapısal değişiklik — yeni adım ya da yeni madde — almamış olmalı** | ✓ **Karşılandı** — üçüncü kez | `grep -c '^- \[ \]'`: Aşama 5'in kapanışından sonra **66**; Aşama 6 boyunca değişmedi (`git log e4b2300..HEAD -- .claude/checklists/` boş); **bu terfiden sonra 66**. Bölüm bazında aynı: §1 8 · §2 8 · §3 19 · §4 12 · §5 3 · §6 9 · §7 7. Adım sayısı (§7'nin altı adımı) aynı. Değişen: **altı madde genişletmesi** — §1'de bir (girdi maddesi: hedefin adı bütün park bloklarında; gövde cümlelerinin bugünkü metinden sayımı), §3'te bir (satır biçimi: kaçışlı çubuk), §4'te iki (devir taraması; özet tabloları maddesi), §6'da bir (etki yansıtma) ve başlık notu |

**Dürüstlük notu — sınır vakası ve bu terfinin bir sonraki değerlendirmeyi bozabilmesi.** Sınır vakası yok: altı genişleme var olan yükümlülüklerin hedeflerini ve tarama biçimini genişletiyor, yeni bir an ya da adım eklemiyor. **Ama Soru 1 onaylanırsa** M-1'in "yazım turunun son oturumundan sonra özet ↔ ayrıntı taraması" yeni bir yükümlülüktür — checklist §4'e yeni madde girer ve ikinci koşulu Aşama 7'nin kapanışında bozar. Bu bilgi Soru 8'in cevabına etki etmez ama Soru 1 ile Soru 8'in birlikte okunması gerekir; öyle sunulur.

**Bu kez neyin farklı olduğu.** Proje sahibi iki kez dönmemesini seçti; son seçim *"Liste kalsın, her aşama sonunda yine sorulsun"* (K-1143). Bu aşamanın yeni bilgisi: matris üçüncü kez işletildi ve madde sayısı üçüncü aşama kapanışında da değişmedi (66). `00 §C.7`'nin kendi cümlesi dönüşümü *"checklist bir kez gerçek bir projede işletildikten sonra"*ya bağlar: bu proje doküman döneminin altıncı aşamasında, dört aşama daha var. Önerim bu yüzden **seçenek 2'de kalmak**: aynı cevabı üçüncü kez istemek proje sahibinin zamanını yer, ama cevap onundur ve "her aşama sonunda yine sorulsun" talimatı verdi. Üç seçenek Soru 8'dedir.

**Elenen — dönüşümü bu PR'da uygulamak:** yeni skill yaratmak yapısal değişikliktir ve onay ister (GUARDRAILS §3); `00 §C.7`'nin metni değişir (GUARDRAILS §2). **Elenen — soruyu sormadan kapatmak:** kapatma kararı proje sahibinindir.

---

## 5. Proje sahibinin cevabını bekleyen dokuz soru

> **Hepsi ayrı numaralı sorudur** (`INSTRUCTIONS §2`; K-722 → K-853: tek bir "evet" birden çok öneriye okunmaz). Uygulanan metinler cevaptan sonra ayrı bir `docs:` PR'ında yazılır; hiçbiri aşamanın kapanışını engellemez, kurallar L3–L4'te yürürlükte. **Kapı:** arşiv işaretinden önce (K-437'nin 6. adımı), ⚠ listesiyle aynı mesajda. Her sorunun altında "Uygulanacak metin" çıplak biçimdedir; yönetici sade metni iletir.

### Soru 1 — Yazım bittikten sonra "özet ↔ ayrıntı" taraması (maliyet düzeninin 1–3. maddeleri)

> **Sorun:** Bir dokümanda özet yerleri (tablo hücreleri, "on üç yer" gibi sayılar) ayrıntıyla zamanla ayrışıyor; bu küçük farklar denetimlere kadar sürünüyor ve pahalı denetimlerin bulgularının büyük kısmını oluşturuyor. **Ne değişir:** Yazım bitince, denetimden önce ucuz bir tarama koşar; bulduğu farklar aynı PR'da düzeltilir ve denetim o PR birleşmeden başlamaz. API Tasarımı'nda bu tarama denetimden önce 56 fark buldu, 38'i düzeltildi; ama checkpoint'te aynı türden farklar yine çıktı — tarama sorunu bitirmiyor, azaltıyor. Bedeli yaklaşık yarım gün (48 dk). Kontrol listesine yeni bir madde girer; bu, "liste beceriye dönüşsün mü" sorusunun ikinci şartını (listeye yeni madde girmemiş olması) bir sonraki aşamadan itibaren bozar.
> **1 ·** ekle (öneri) · **2 ·** ekleme

**Uygulanacak metin (1 ise):** `checklists/document-stage.md` §4'e yeni madde: *"- [ ] **Yazım turunun son oturumu bitince, kalite döngüsünden önce özet ↔ ayrıntı taraması koşar** — betik dokümandaki her özet yerini bulur (matris hücreleri, envanter tabloları, kapalı listeler, başlık notu); bir Sonnet her birini ayrıntısına karşı okur; izlenebilirlik matrisi ya da başka bir kaynak eşleme tablosu varsa yazım bittikten sonra çıktıya karşı bir kez yeniden okunur; farklar aynı PR'da düzeltilir ve kalite döngüsünün audit'i bu PR merge edilmeden başlamaz. **Neden:** API Tasarımı'nın taraması denetimden önce 56 bulgu buldu — 38'i düzeltildi, 6'sı çürüdü, 12'si hükme kaldı (Aşama 6'nın öğrenim terfisi)."* `audit` skill'i "Koşum biçimi" 1. madde: iç tutarlılık merceği cümlesine *"— bu tarama koşmuşsa aynı yerler yeniden dilimlenmez, mercek taramanın kapsamadığı yerlere (hüküm bekleyen maddeler dahil) bakar"*.

### Soru 2 — Bulguyu doğrulayan ajanın modeli (K-1224)

> **Sorun:** Bir denetimin bulduğu sorunu "gerçekten sorun mu" diye kontrol eden ajan, bu aşamanın sonunda Opus / `high` ile çalışıyor (proje sahibi 2026-10-28'de seçti); önce Sonnet ile çalışıyordu. Sonnet ile 186 bulgudan 4'ü çürütüldü (yüzde 2), Opus / `high` ile 56 bulgudan 16'sı (yüzde 29) — iki grup aynı bulgulardan oluşmadığı için bu kesin kanıt değil, işaret. Ayrıca sizin genel kuralınızın hook'u bulgu kontrolünü zaten Opus / `high` istiyor; Sonnet'e dönmek her turda yeniden çatışma demek. **Ne değişir:** Evet derseniz bulgu başına bir Opus / `high` doğrulayıcı kalıcı kural olur (ikinci doğrulayıcı açılmaz); hayır derseniz bu projede doğrulama Sonnet ile yapılır ve hook çatışması yönetici tarafından her seferinde size sorulur. Bedeli: bulgu başına yaklaşık 8 kat token (Opus fiyatı ayrı).
> **1 ·** Opus / `high` kalıcı (öneri) · **2 ·** Sonnet'e dön

**Uygulanacak metin (1 ise):** `INSTRUCTIONS §9` "Alt ajanların koşumu"na (1)'in sonuna: *"Bir merceğin ya da ikinci modelin bulgusunu güncel metne karşı kontrol eden doğrulayıcı `[dogrulama]` etiketiyle, Opus ve `high` eforla açılır; bulgu başına bir doğrulayıcıdır, ikinci doğrulayıcı açılmaz (K-1224). **Neden:** Sonnet doğrulayıcılar 186 bulgudan 4'ünü, Opus / `high` doğrulayıcılar 56 bulgudan 16'sını çürüttü (Aşama 6'nın öğrenim terfisi)."* `audit` skill'i "Koşum biçimi" 2. madde: "iki şüpheci" → "bulgu başına bir doğrulayıcı (Opus / `high`)"; `deep-review` madde 7 ve `cross-review` Faz 2 aynı atıfla; `checkpoint` skill'inin koşum paragrafı aynı atıfla.

### Soru 3 — Alt ajanların modeli: yalnız Sonnet / Haiku ve düşük efor (K-1207)

> **Sorun:** Denetimin ortasında "bu projede alt ajanlar yalnız Sonnet ve Haiku, efor düşük, Opus hiç kullanılmasın" dediniz. Kural şu an yalnız kayıtta yaşıyor ve aşama kapanınca unutulur; yönetici talimatındaki eski dağılım (zor işte güçlü model) hâlâ yazılı. **Ne değişir:** Evet derseniz bu kalıcı kural olur (bulgu doğrulaması Soru 2'nin cevabına göre istisna kalır); hayır derseniz bu yalnız API tasarımı aşamasına özgüydü ve sonraki aşamalarda işe göre model seçimi geri gelir. Bu aşamada Sonnet / low merceği Opus / medium merceğinden az bulgu getirmedi ve adım süreleri düştü (§3.1–§3.2); ama aynı iş iki modelde koşmadı ve doküman türü sonraki aşamada değişiyor (Entegrasyon spesifikasyonu dış sağlayıcılara bağlı).
> **1 ·** kalıcı (öneri) · **2 ·** yalnız bu aşamaya özgüydü, sonraki aşamada işe göre seçilsin

**Uygulanacak metin (1 ise):** `INSTRUCTIONS §9` "Alt ajanların koşumu" (1)'in yerine: *"**(1) Bu projede alt ajanlar yalnız Sonnet ve Haiku ile, `low` eforla koşar; Opus ve Fable kullanılmaz** (proje sahibinin kararı, K-1207); mekanik iş önce betikle, betik yazılamıyorsa Haiku, bulgu bulan ve metin yazan iş Sonnet. Tek istisna Soru 2'nin cevabıdır. Agent aracının `model` ve `effort` parametreleri her çağrıda birlikte verilir ve adımın raporu hangi işin hangi modelle koştuğunu bir satırla yazar."* 2 ise: `INSTRUCTIONS §9` paragrafındaki olgusal not "yalnız Aşama 6'ya özgü" diye kapatılır, (1) yürürlükte kalır.

### Soru 4 — Doğrulanan bulguların düzeltmesi: birebir şartname, betikle uygulama (madde 7)

> **Sorun:** Doğrulanan bulguların dokümana işlenmesi için her seferinde ayrı ajanlar açılıyor (audit'te 6, deep review'da 9). **Ne değişir:** Checkpoint'te doğrulayıcı, düzeltmeyi birebir "şu metin şuna dönüşür" şartnamesi olarak yazdı ve bir betik uyguladı — ajan açılmadı, 61 şartname parçasının hiçbiri birbiriyle çakışmadı. Evet derseniz bu biçim denetimlerin kuralı olur; betikle uygulanamayan karmaşık düzeltme yine Sonnet uygulayıcıya gider. Bedel: şartnameyi yazan doğrulayıcının işi biraz uzar.
> **1 ·** ekle (öneri) · **2 ·** ekleme

**Uygulanacak metin (1 ise):** `audit` skill'i "Koşum biçimi" 2. maddenin sonuna: *"**Düzeltme önce birebir şartname olarak yazılır:** doğrulayıcı düzeltmeyi 'eski metin → yeni metin' çiftleri olarak bulguyla birlikte bırakır; çiftler betikle uygulanır (her çiftin eski metni dosyada tam bir kez geçmelidir), uygulanamayanlar ve birden çok yeri etkileyenler dosya ya da bölüm başına gruplanıp tek bir uygulayıcıya verilir. **Neden:** Aşama 6'nın checkpoint'inde 27 doğrulayıcının 61 şartname parçası uygulayıcı ajan açılmadan betikle uygulandı ve hiçbiri çakışmadı; audit ve deep review 15 uygulayıcı ajan kullanmıştı."*

### Soru 5 — Mekanik işte önce betik (madde 9)

> **Sorun:** Sayma, listeleme, atıf ve tekillik kontrolü gibi mekanik işler bu aşamada hep betikle yapıldı ve hiçbirinde ajan gerekmedi; ama kural bunu yazmıyor, yalnız "mekanik iş ucuz modele" diyor. **Ne değişir:** Evet derseniz kuralın ilk tercihi betik olur, betik yazılamıyorsa en ucuz model. Bedel yok; var olan uygulama yazılı hâle gelir.
> **1 ·** ekle (öneri) · **2 ·** ekleme

**Uygulanacak metin (1 ise):** `INSTRUCTIONS §9` (1)'in mekanik iş cümlesi: *"mekanik iş (sayma, listeleme, bir atfın ya da kimliğin varlığına bakma, CR ve kontrol karakteri taraması, sürüm ve tarih alanları) **önce betikle** yapılır (betik `Write` aracıyla dosyaya yazılır, `INSTRUCTIONS §7`); betik yazılamıyorsa en ucuz modelde (Haiku) koşar"*. (Soru 3'ün cevabı (1)'in geri kalanını belirler.)

### Soru 6 — Ajanlara daha az metin okutmak (madde 15–17)

> **Sorun:** Denetim ajanlarına tüm doküman ya da tüm karar kaydı (3,96 MB) okutmak hem pahalı hem hata kaynağı. Bu aşamada audit ve checkpoint'in mercekleri önce kendi küçük dilim dosyasını okudu, bulgularını bulduğu anda dosyaya yazdı ve karar kaydının bütününü hiçbir ajan okumadı. Bu düzen yalnız yönetici talimatında yaşıyor. **Ne değişir:** Evet derseniz denetim skill'lerinin "Koşum biçimi"ne ve `INSTRUCTIONS §9`'a yazılır. Etkisi ölçülemedi (tabanda mercek token'ı yok) ama kalite kaybı olmadı.
> **1 ·** ekle (öneri) · **2 ·** ekleme

**Uygulanacak metin (1 ise):** `audit` skill'i "Koşum biçimi" 1. maddenin sonuna (`deep-review` madde 7 ve `checkpoint` skill'i koşum paragrafı aynı atıfla): *"**Her mercek ve doğrulayıcı önce kendi dilim dosyasını okur:** adımdan önce betik, adımın ihtiyacı olan metni (dokümanın ilgili bölümleri, karar kaydının ilgili K satırları, aşamanın plan bölümü) satır numaralı ayrı dosyalara çıkarır; tam doküman ya da tam karar kaydı yalnız bulgunun dayandığı yer dosyada yoksa ya da bir atfın hedefi doğrulanacaksa açılır. **Karar kaydı hiçbir ajana bütünüyle okutulmaz** — `grep` / `sed` / betikle bölüm bölüm okunur. **Neden:** Aşama 6'nın audit ve checkpoint'inde 31 mercek bu düzenle koştu; karar kaydı 3,96 MB'tır."*

### Soru 7 — Ölçüm tablosu kalıcı mı (madde 18–19)

> **Sorun:** Bu aşamada her adımın süresi ve token'ı bir tabloya yazıldı; bu rapor o tabloyla karşılaştırma yaptı. Ama tabloyu yönetici elle taşıdı (27 PR'a yayıldı), üç adımın alanı boş kaldı ve ayrıca cross-review'ın yardımcı ajanları hiç ölçülmedi. **Ne değişir:** Hayır derseniz ölçüm bu karşılaştırmayla biter; maliyet yeniden sorun olursa aynı yöntem istenir. Evet derseniz her aşamada kapanış kontrol listesine ölçüm maddesi girer (yeni madde — Soru 1'deki gibi ikinci şartı etkiler). Önerim hayır: bedeli ölçmenin faydasından büyük.
> **1 ·** kalıcı yap (checklist §7'ye madde) · **2 ·** bırak (öneri)

**Uygulanacak metin (1 ise):** `checklists/document-stage.md` §7'ye 5. adımın altına madde: *"Maliyet ölçümü — her adımın net süresi, adım ajanının token'ı, açtığı alt ajanların sayısı ve modeli ilerleme notuna yazılır; kapanışta tablo bir önceki aşamanın tablosuyla karşılaştırılır. **Yardımcı ajanların token'ı da kayda geçer** (taban Aşama 5'te bunu saymadı)."* 2 ise: metin uygulanmaz, `PLAYBOOK_FEEDBACK` PF-115 `Reddedildi`.

### Soru 8 — Kontrol listesi beceriye dönüşsün mü (Ö-23)

> **Sorun:** Aşama kontrol listesini beceriye (otomatik çalışan bir tarife) dönüştürmek için koyduğumuz iki şart bu aşamada da üçüncü kez sağlandı: matris yine işletildi ve kapanışta listeye yeni madde girmedi (altmış altı madde yerinde). Kural, soruyu her aşama sonunda yeniden sormamı istiyor. **Ne değişir:** Hangisini seçerseniz o uygulanır; dikkat: Soru 1'e "ekle" derseniz listeye yeni madde girer ve ikinci şart bir sonraki aşamadan itibaren sağlanmaz.
> **1 ·** dönüştür · **2 ·** liste kalsın, her aşama sonunda yine sorulsun (öneri — bugünkü karar) · **3 ·** liste kalsın, bu proje boyunca bir daha sorulmasın (karar sonraki projeye)

**Uygulanacak metin:** 1 ise `.claude/skills/<ad>/SKILL.md` yeni skill (GUARDRAILS §3) ve `00 §C.7`'ye onay istenir; 2 ise checklist başlık notuna bu kapanışın sonucu eklenir (cümle hazır); 3 ise başlık notunun son cümleleri *"Proje sahibi <onay tarihi>'nde ölçütün bu proje için kapanmasına karar verdi (K-…): checklist bu projede checklist olarak kalır, skill'e dönüşme kararı proje tamamlandıktan sonra playbook'a gider (`Docs/PLAYBOOK_FEEDBACK.md` PF-32; `00 §C.7`)."*, §7'nin 5. adımına *"— ölçüt bu projede kapandıysa atlanır (başlık notu)"* eki.

### Soru 9 — Yöntem dokümanına Aşama 6'nın dersleri (`00 §N.1`)

> **Sorun:** Yöntem dokümanının her aşamanın derslerini yazdığı bölümünde API tasarımı aşamasının dersleri henüz yok. **Ne değişir:** Bu aşamanın beş dersi — örneğin "bir doküman başka bir dokümana iş bırakmışsa bunu yalnız bırakanın kayıtlarında değil, bütün dokümanların kayıtlarında aramak gerekir" — önceki aşamalardaki gibi o bölüme eklenir; reddederseniz dersler yalnız raporda kalır.
> **1 ·** ekle (öneri) · **2 ·** ekleme

**Uygulanacak metin (1 ise) — `00 §N.1`, "Aşama 5 — Veri modeli" paragrafının ardına yeni paragraf; `00` sürümü +1:**

> **Aşama 6 — API tasarımı (2026-10-18 → 2026-10-28).** Kurallar `.claude/checklists/document-stage.md` §1, §3, §4, §6 ve `.claude/INSTRUCTIONS.md` §9'a terfi etti; rapor `Docs/CHECKPOINT_REPORTS/PHASE6_LEARNING_PROMOTION.md`. Tekrarlanacak desenler:
>
> 1. **Bir dokümana bırakılan iş başka bir dokümanın kayıtlarında yaşayabilir; hedefin adı bütün dokümanların kayıtlarında aranır.** Entegrasyon Spesifikasyonu'nun park satırı ödeme sağlayıcısından dönüşün yolunu API Tasarımı'na bıraktı; API Tasarımı'nın kendi kayıtları ve dizinleri işi saymıyordu ve açılış onu hedefin adını bütün dokümanların park bloklarında tarayarak buldu. Aşama 4'ün 1. ve Aşama 5'in 6. deseninin devamıdır.
> 2. **Özet, yazıldığı günün cümlesini kopyalar; istisna sonraki kararlarla gelir.** Konvansiyon tabloları ve kapalı listeler yazıldıkları oturumun karar cümlesini aldı; sonraki oturumların kararları listeye yer ekledi ya da istisna açtı ve listeyi güncelleyen mekanik bir adım yoktu. Yazım turundan sonraki özet ↔ ayrıntı taraması 56 fark buldu. Aşama 5'in 1. ve 2. desenlerinin devamıdır: kaydın özeti de sınanır.
> 3. **Kalite döngüsünün düzeltmesi gövdeye karar satırı olmadan kural ve devir yazabilir.** İki cümle deep review'un düzeltmesiyle girdi, K numarası ve park satırı yoktu; K'ya dayanan etki yansıtma betikleri görmedi, çakışma taraması buldu. Aşama 5'in 6. deseninin devamıdır.
> 4. **Proje kuralı genel kural ya da hook'la çatışırsa çatışma dolanılmaz, bildirilir.** Cross-review'ın bir turunda hook iki kontrolcüyü engelledi, adım ajanı açıklamadaki sözcüğü değiştirerek dolandı; proje sahibi sorulunca bulgu doğrulamasını Opus / `high` yaptı. Aşama 5'in 7. deseninin (yönetici düzeni) devamıdır.
> 5. **Yardımcı ajanlar ölçülmezse maliyet karşılaştırılamaz.** Aşama 5'in tabanı yalnız yönetici seviyesindeki ajanı saydı; Aşama 6'da yardımcı ajanlar kayıtlı token'ın yaklaşık üçte ikisini taşıdı ve adım ajanının kendi token'ının 12 ila 19 katıydı. Süre ve adım ajanı token'ı düştü; toplam token karşılaştırılamadı. Aşama 5'in 7. deseninin (maliyet ve görünürlük) devamıdır.

---

## 6. Uygulanan terfiler — dosya ve bölüm

| Dosya | Bölüm | Değişiklik | Aday |
|---|---|---|---|
| `.claude/checklists/document-stage.md` | Başlık notu, son güncelleme | Ö-23'ün Aşama 6 sonucu (iki koşul üçüncü kez karşılandı, cevap arşiv işaretinden önce notaya yazılır) | Ö-23 |
| | §1 — girdi maddesi | Eşleme hedefin adını bütün dokümanların park bloklarında arar; gövde cümlelerinin sayımı bugünkü metinden | Ö-112, Ö-114 |
| | §3 — satır biçimi maddesi | Tablo hücresinde çubuk her zaman `\|`; betikler kaçışlıları sayıdan çıkarır | Ö-122 |
| | §4 — devir taraması | Hedefin adı bütün dokümanların park bloklarında aranır | Ö-112 |
| | §4 — özet tabloları maddesi | Kapalı liste ve sayı taşıyan cümle her oturumun bitişinde yeniden sayılır | Ö-115 |
| | §6 — etki yansıtma | Gövde cümleleri K'dan bağımsız taranır; park etiketi ↔ etki sütunu betiği iki yönlü, metni okur | Ö-118, Ö-120 |
| `.claude/INSTRUCTIONS.md` | Son güncelleme; §9 — "Alt ajanların koşumu" | Aşama 6'da dağılımın K-1207 ve K-1224 ile değiştiğine olgusal not; (4) çatışma dolanılmaz, bildirilir; (5) eşzamanlı sınır yirmi | Ö-116, Ö-117, Ö-121 |

**L5 (hook, CI) elendi:** bu aşamanın derslerinin hiçbiri git ya da CI düzeyinde mekanik bir ihlal değildir — park satırı eşlemesi, gövde cümlesi taraması ve özet farkları anlamsal okumadır. Çubuk kaçışı (Ö-122) betikle denetlenebilir; ama CI'ya alınması doküman döneminin sonunda yeniden düşünülebilir (bugün CR ve `$1` taramaları da elle koşuyor ve hiçbir PR'da kaçmadı).

**Skill'ler değişmedi.** Ö-116…Ö-122 hiçbiri `audit`, `deep-review`, `cross-review` ya da `checkpoint` skill'inin metnini gerektirmedi; skill'e dokunan öneriler (Soru 1, 2, 4, 6) cevaba bağlı.

---

## 7. Playbook'a geri akış (K-649)

Liste 108 satırdan **116**'ya çıktı. Var olan satırlar tekrar açılmadı, güncellendi: PF-32 (Ö-23'ün Aşama 6 sonucu — `Onay bekliyor`), PF-108 (şablon düzeltmesinin playbook'a gideceği teyit edildi).

| Satır | Öğrenim | Hedef |
|---|---|---|
| **PF-109** (yeni) | Hedefin adı bütün park bloklarında; gövde cümleleri bugünkü metinden (Ö-112, Ö-114) | checklist §1, §4 |
| **PF-110** (yeni) | Kapalı liste ve sayı cümlesi her oturumda yeniden sayılır (Ö-115) | checklist §4 |
| **PF-111** (yeni) | Etki yansıtma K'dan bağımsız gövde taraması; iki yönlü betik (Ö-118, Ö-120) | checklist §6 |
| **PF-112** (yeni) | Tablo hücresinde çubuk kaçışı (Ö-122) | checklist §3 |
| **PF-113** (yeni) | Çatışma dolanılmaz; eşzamanlı sınır yirmi (Ö-117, Ö-121) | `INSTRUCTIONS §9` |
| **PF-114** (yeni) | `07` şablonunun §1 notu: aday listesi devir hâlidir (Ö-123) | `Docs/07_API_DESIGN.md` şablonu |
| **PF-115** (yeni) | Maliyet düzeninin terfi önerileri (Soru 1–7) | `INSTRUCTIONS §9`, checklist, skill'ler — `Onay bekliyor` |
| **PF-116** (yeni) | Aşama 6'nın desenleri — `00 §N.1` (Soru 9) | `00 §N.1` — `Onay bekliyor` |

---

## 8. Kayda ve hafızaya yansıma

- **Karar kaydı v0.172:** K-1230 (bu adımın kararları, öneriyle kaydedildi — ⚠ değildir: süreç kararıdır, ürün kuralına dokunmaz). §16.1: öğrenim terfisinin adım maddesi [x]; öğrenim adayları maddesi [x]; Ö-23 ve maliyet düzeni maddeleri değerlendirmeyle güncellendi (kapıları dokuz sorunun cevabına bağlandı, [ ] kalır); **yeni açık madde** — proje sahibinin cevabını bekleyen dokuz soru (kapı: arşiv işaretinden önce); ölçüm tablosunda checkpoint satırı tamamlandı (34 dk, 393 869 token, #174) ve bu adımın satırı eklendi. K-1224'ün ve K-1207'nin etki sütununa K-1230'a geri işaret eklenmedi — K-1230 kuralı değiştirmez, soruyu açar.
- **Repo hafızası:** `MEMORY.md` Güncel Durum bu adımın hâline getirildi — önceki adımın (checkpoint) paragrafı `MEMORY_ARCHIVE.md`'ye taşındı (`INSTRUCTIONS §7`).
- **Kullanıcı hafızası — bu adımda yazılmadı; yönetici günceller:**
  - `asama6-maliyet-duzeni-denemesi.md` (H-2) — "Uygulandı" satırına: değerlendirme bu raporun §3'ünde, dokuz soru (kapı: arşiv işareti); cevaplar işlendikten sonra not silinir.
  - `yalniz-sonnet-haiku-low.md` (H-15) — K-1207'nin kalıcılığı Soru 3, istisnası Soru 2'dir; cevap gelince `Terfi` satırı eklenir.
  - `asama1-kapanis-otomasyonu-yetkisi.md` (H-1) ve `yalniz-cok-kritik-konulari-sor.md` (H-14) — arşiv işaretinde "Aşama 6 kapandı — Entegrasyon spesifikasyonları aşamasının açılışında yeniden sorulur" satırları (6. adımın işi).
- **Mekanik:** değişen dosyalarda CR 0 (`tr -cd '\r'`), kontrol karakteri 0; eklenen satırlarda geri başvuru artığı yok; düzenlemeler birebir değiştiren, her değiştirmede tam bir eşleşme isteyen betikle yapıldı ve şartnamesi dosya yazma aracıyla yazıldı.
- **Model dağılımı:** bu adımı yürüten ajan Sonnet / `low`; ölçüm hesabı betikle (`olcum.py`); alt ajan açılmadı, bulgu doğrulaması gerektiren iddia çıkmadı.

**Sırada:** K-437'nin 6. adımı — proje sahibine tek mesaj: çakışma taramasının dokuz ⚠ kararı ve checkpoint'in K-1229'u (hazır metin `PHASE6_CONFLICT_SCAN.md` §4.1, `CP06_PHASE6_CHECKPOINT.md` §5.2) ve bu raporun §5'indeki dokuz soru, **her biri ayrı numaralı**; cevapların kayda işlenmesi (onaylananlar ayrı `docs:` PR'larında); ardından arşiv işareti (⚠ işaretlerinin dönüşümü; Aşama 6'nın kayıtları salt okunur; `07` ✓) ve Entegrasyon spesifikasyonları aşamasına devir. Devir notu çalışma modlarının yeniden sorulacağını ve Ö-23'ün sonucunu taşır.

---

*Kaynak: K-437 (kapanış sırası, 5. adım) · `00 §K`, §C.7, §N · GUARDRAILS §2, §3 · karar kaydı §16.1 (aday listesi, maliyet düzeni maddesi ve ölçüm tablosu), §16.2 · çakışma taraması §7 · CP06 §3, §6 · K-1144…K-1229 (süreç kararları) · K-1145, K-1207, K-1224 (maliyet düzeni ve modeller) · K-722, K-852, K-853, K-991, K-1143 (Ö-23'ün ölçütü ve önceki kararları) · K-649 (playbook'a geri akış) · K-1230 (bu adımın kararı) · `Docs/CHECKPOINT_REPORTS/PHASE5_LEARNING_PROMOTION.md` (yapı örneği).*
