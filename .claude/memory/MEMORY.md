# Proje Hafızası

> **Bu dosya indeks + güncel durum snapshot'ıdır.** İçerik gömülmez — her hafıza ayrı bir dosyadır.
> Ne buraya girer / ne L1–L5'e terfi eder: [`README.md`](README.md).
> **Otoriter kaynak değildir** — task durumu için [`../../Docs/IMPLEMENTATION_STATUS.md`](../../Docs/IMPLEMENTATION_STATUS.md).

---

## Proje Özeti

- **Ad:** Shopfolio
- **Tanım:** Herhangi bir firmanın kendi ürünlerini sergileyip çevrimiçi satabileceği ve kurumsal tanıtımını yapabileceği; üyelik (sipariş için zorunlu değil), Google ile giriş, sipariş, ödeme ve sipariş takibi içeren web uygulaması.
- **Dönem:** Doküman üretimi
- **Doküman dili:** Türkçe · **Kod dili:** İngilizce

---

## Güncel Durum

> Kısa tut. Tarihsel detay `MEMORY_ARCHIVE.md`'ye taşınır (00 §G.2).

- **AŞAMA 4 AÇILDI — 2026-10-16: Teknik mimari → Teknik Mimari (`05`)** (`00 §C.1`; rol Senior Software Architect; girdi `01`–`04`, `10`; matris zorunlu değil). Aşama 3 2026-10-05'te kapandı — adımları ve arşiv işareti (#97, K-854) `MEMORY_ARCHIVE.md`'de.
- **Son adım — kalite döngüsü, cross-review 6. tur (karar kaydı v0.109, 2026-10-17; K-961):** Teknik Mimari (`05`) **v0.11**. Aynı istem, şablon ve iki koşum (K-952, K-953). Altı çağrının ikisi (parça 1 ve 3) TEMİZ; altı ham bulgu, beş ayrı bulgu — biri KABUL, ikisi KISMİ, ikisi RET: sunucudaki yedek anahtarı güncel nesneyi silebildiği için silme işareti bırakır ve eski sürüm otuz günde düşer — "yedek oradan silinemez" güvencesi otuz günle sınırlı yazıldı, fark edilmeyen silme kalıcı olur (kalan risk) (§7.6.6, 1.2.30, §7.6.10, §9.2.3, MK-26; **K-961 ⚠**, orta şiddet; `DEPLOY_RUNBOOK` park satırı) · sağlayıcı bildiriminin kaydı iş satırıyla aynı işlemde, "alındı" işlemden sonra (§4.5.2; KISMİ, düşük) · 9.2.3 kayıtların yedekteki kopyaları için 7.4.5'e işaret eder (KISMİ, düşük; 4. turun dönen iddiası) · **RET:** arşiv zaman aşımının bir dakikası (5. turun dönen iddiası; K-888) · iki dakikadan kısa kesintide havale ertelemesi (tasarım değeri, nadir). Geri besleme yok. Çok kritik soru çıkmadı. **⚠ listesi 34.** Yakınsama: ayrı bulgu 7 → 5 → 6 → 6 → 5 → 5, kabul 7 → 3 → 3 → 3 → 4 → 3, yeni karar 2 → 0 → 1 → 2 → 2 → 1; 3.–6. turların beş orta bulgusunun dördü §7.6 ve §8.7'de — güvence cümlesinin mekanizmaya karşı okunması. **Çıkış kuralı uygulanmadı** — K-961'in kabulü orta şiddette. **TEMİZ DEĞİL;** Faz 5 koşmadı. Rapor `Docs/CROSS_REVIEW_REPORTS/05_CROSS_REVIEW_R6.md`.
- **SIRADA — `05`'in cross-review'ının 7. turu (K-437'nin 2. adımının devamı; checklist §6):** aynı istem (1. turun raporunun §1'i, değiştirilmeden), aynı şablon (`58cf0b9`'daki v0.1, park blokları — 9–32. satırlar — çıkarılmış) ve iki koşum (K-953): tek parça + beş parça (başlık notu ile §0–§3 · §4–§5 · §6 · §7 · §8–§12); ölçü sürer (K-952). **Tekrar eden RET** (aynı yer ve iddiayla dönerse turun sonucunu belirlemez): Google'ın kalıcı kimliği (§6.1.4), ödemesi alınmamış siparişin üç yılı (§7.4.3 (4), §4.4.4), yedeğe ulaşmamış penceredeki silmeler / ayrı silme deposu (§7.4.5; K-888, K-939, K-955). Dönen iki iddia — arşiv zaman aşımının bir dakikası (§7.6.1), kayıtların yedekteki süresi (§9.2.3) — henüz iki kez reddedilmedi, kendi gerekçesiyle okunur. **Çıkış kuralı uygulanabilir** (skill Faz 4 madde 5): kabullerin hepsi düşük şiddette iki yerin metin farkıysa bulgular uygulanır, kalıp taranır, döngü "TEMİZ — çıkış kuralıyla, 7 tur" diye kapanır, kural bir karar satırıyla kaydedilir (K-849 kalıbı) ve sınıfın kalan riski §12.1'de checkpoint'e devredilir; kabullerden biri orta ya da yüksek şiddetteyse döngü sürer. **6. turun önerisi:** 7. tur da §7.6 ya da §8.7'de orta bir bulgu getirirse 8. turdan önce §7.4.5–§7.6 ve §8.6–§8.7'nin güvence cümlelerini mekanizmaya karşı okuyan hedefli bir mercek (6. turun raporu §6). TEMİZ olunca etki yansıtma (`02` v0.73, `03` v0.26, `04` v0.24, `10` v0.48, `01` v0.33) ve `05` ✓ (K-432). Sonra kapanış: çakışma taraması, checkpoint, öğrenim terfisi (Ö-23, öğrenim adayı 13 ve §12.1'in altı adayı), ⚠ listesinin (34) gösterilmesi ve arşiv işareti. D-01 (yığın → `CONTEXT.md` §1, `SETUP.md` §2, §4; verilenler `05 §2.3`) Aşama 4 kapanışından sonra, ilk implementation task'ından önce kapanır.
- **Dokümanlar:** Proje Vizyonu (`01`) **v0.33** · Ürün Gereksinimleri (`02`) **v0.73** · Kullanıcı Akışları (`03`) **v0.26** · Arayüz Tanımları (`04`) **v0.24** · MVP Kapsamı (`10`) **v0.48** — beşi ✓ Tamamlandı. Teknik Mimari (`05`) **⏳ v0.11** (audit ve deep review tamam — K-937…K-950; 6563 m.11/3 — K-951; cross-review 1. tur — K-952…K-955, TEMİZ DEĞİL; 2. tur — yeni K yok, TEMİZ DEĞİL; 3. tur — K-956, TEMİZ DEĞİL; 4. tur — K-957, K-958, TEMİZ DEĞİL; 5. tur — K-959, K-960, TEMİZ DEĞİL; 6. tur — K-961, TEMİZ DEĞİL; 7. tur sırada). Karar kaydı **v0.109**, K-01…K-961 (Aşama 1, 2 ve 3'ün kayıtları salt okunur; Aşama 4 K-855'ten, açık kalem A-19'dan). Metodoloji (`00`) **v1.0.5** (Aşama 1, 2 ve 3'ün desenleri §N.1'de — K-853).
- **Oturum kapanışı:** kural `INSTRUCTIONS §7`'de (K-33, K-38) — her oturum ve kapanış adımı bu bloğu kendi `docs:` PR'ında günceller, alt ajan dahil.
- **Gate durumu:** Implementation başlamadı, faz yok. Doküman: **01, 02, 03, 04, 10 ✓** · 05 ⏳ · 06–09, 11–12 ⬚. Aşama 1 blok planı: **0–9 ✓** (111 konu). Aşama 2 konu planı: **AK0–AK11 ✓** (62 konu; tracker §8.2–§8.3). Aşama 3 konu planı: **UI0–UI9 ✓** (57 konu; tracker §10.2–§10.3). Aşama 4 konu planı: **TM0–TM12 ✓** (54 konu — 33 W, 4 P, 17 T; yazım turu tamam; tracker §12.2–§12.3).
- **İleriye bağlayıcı kayıtlar:** **K-352** — `B9-05` ölçüm eklerse K-348 ve K-349 yeniden açılır (koşul gerçekleşmedi, K-397) · **K-400** — `B9-10` yalnız sipariş verisinden türetir · **K-420** — yol haritası yalnız `10 §3`'ten beslenir, `01 §7`'den asla · **K-424**'ün üç bağımlılık zinciri (öneri→analitik→çerez bandı · stokta haber ver ve terk edilmiş sepet→İYS onayı · SMS→yeni dış ön koşul; K-473 kargo ve havale otomasyonunu aynı kalıba koydu) · **K-439** — `01 §7`'nin sınırı ile aynı alandaki yol haritası adayı çelişmez: sınır kimliktir, aday özellik · **K-474** — tetikleyici gerçekleşince kargo entegrasyonu ve havale eşleştirmesi öne çekilir · **K-10** — MVP'de çok kiracılık için hazırlık yapılmaz (`05` park satırı).
- **Aşağı dokümanlara devirler:** devir girdilerinin tek tablosu — Aşama 2'ye karar kaydının §7'sinde, Aşama 3'e §9'unda, **Aşama 4'e §11'inde** — açılışta konu planına betikle eşlendi (§12.2, tamlık taraması), workshop'un kapanışında her birinin kapanışı betikle doğrulandı (§12.2, kapanış taraması). Dizinler: Aşama 1 `Docs/CHECKPOINT_REPORTS/PHASE1_CONFLICT_SCAN.md` §6 (kalite döngüsünün 147 kararı, 292 devir; `05`'e 25 karar) · Aşama 2 `PHASE2_CONFLICT_SCAN.md` §6 (63 karar, 137 atıf; `05`'e 7) · Aşama 3 `PHASE3_CONFLICT_SCAN.md` §6 (40 karar, 53 atıf; `05`'e 9, `04`'ün gövdesinde dört cümle — biri karar satırsız: genişlik sınıflarının uygulanması, `04 §2.1.1`). `05`'in başında üç park bloğu (Aşama 1, 2, 3); hedef dokümanların park bloğu dizine işaret eder.
- **Son güncelleme:** 2026-10-17

---

## Task Changelog (son N task)

| Task | Durum | Özet | Commit | PR |
|---|---|---|---|---|
| | | | | |

> Bu tablo şişmeye başladığında (yaklaşık 20 satırı geçince) eski satırlar `MEMORY_ARCHIVE.md`'ye taşınır.

---

## Proje

- `<project_*.md dosyaları buraya tek satır olarak listelenir>`

## Kullanıcı

- `<user_*.md>`

## Çalışma Tercihleri (feedback)

- `<feedback_*.md>`

## Referanslar

- `<reference_*.md>`

---

## Terfi Edenler

> Bir zamanlar hafızada yaşayıp artık L1–L5'te olan kurallar. Buraya yalnız işaretçi yazılır.

| Kural | Terfi ettiği yer | Tarih |
|---|---|---|
| Bir konunun alt parçaları da tek tek sorulur | `INSTRUCTIONS.md` §2 | 2026-08-18 |
| Seçenekler sade dille, somut sonuç üzerinden yazılır | `INSTRUCTIONS.md` §2 | 2026-08-18 |
| Sorular ve seçenekler numaralandırılır | `INSTRUCTIONS.md` §2 | 2026-08-30 |
| Konu kapanışında tamlık taraması | `INSTRUCTIONS.md` §2 | 2026-09-15 |
| Kritik olmayan workshop soruları öneriyle kaydedilir | `00` §C.2 · `INSTRUCTIONS.md` §2 · `GUARDRAILS.md` §6 · `checklists/document-stage.md` §3 | 2026-09-17 |
| PR base'i her zaman `main`'dir | `INSTRUCTIONS.md` §3.2 · `.github/workflows/ci.yml` (base filtresi kaldırıldı) | 2026-09-17 |
| Workshop konusu tek mesajda, numaralı öneri listesi olarak sunulur; kritik maddeler ⚠ ile işaretlenir | `INSTRUCTIONS.md` §2 · `checklists/document-stage.md` §3 | 2026-09-18 |
| Tamlık taraması liste hazırlanırken koşulur; blok sonunda bloğun tamamına genişler | `INSTRUCTIONS.md` §2 · `checklists/document-stage.md` §3 | 2026-09-19 |
| Taslağı yazılmış bölüme dokunan karar `(taslak güncellenecek)` işaretini alır; bloğun son konusunda işaretsiz eşleşme taranır (K-29) | `checklists/document-stage.md` §3 | 2026-09-19 |
| Dokümanlar adıyla anılır; özetler kısa ve sade | `INSTRUCTIONS.md` §2 (PR #37) | 2026-10-02 |
| Toplu onay ve ⚠ öneriyle kayıt — iki mod, işaretleri ve gösterilmeyen kararların listesinin kapısı | `INSTRUCTIONS.md` §2 · `GUARDRAILS.md` §6 · `checklists/document-stage.md` §1, §7 | 2026-10-03 |
| Soru açmadan önce kural katmanları taranır; öncül önce anlatılır | `INSTRUCTIONS.md` §2 | 2026-10-03 |
| CI'da sessizlik yeşil değildir — check-run sayısı doğrulanır | `INSTRUCTIONS.md` §3.2 | 2026-10-03 |
| Her oturum ve kapanış adımı PR'ı Güncel Durum'u taşır (K-38); K numarası tekildir | `INSTRUCTIONS.md` §7 · `checklists/document-stage.md` §3 | 2026-10-03 |
| Yazım tek bağlamda; çok mercekli denetim kalite döngüsünde | `checklists/document-stage.md` §4 · `skills/audit` | 2026-10-03 |
| Aşama 1'in dokuz deseni; döngü yakınsamıyorsa ciddiyet ölçüsü | `00` §N.1, §C.5 | 2026-10-03 |
| Aşama 1'in süreç kararları (K-28, K-30, K-33…K-38, K-429…K-432, K-437) ve Aşama 1 öğrenimleri | `checklists/document-stage.md` §3, §4, §6, §7 · `skills/cross-review`, `audit`, `deep-review` · `scripts/git-hooks/pre-commit` | 2026-10-03 |
| Devir notunun sorusu da katmanlara karşı taranır; açık maddeleri yeni aşamanın listesine geçer | `INSTRUCTIONS.md` §2 · `checklists/document-stage.md` §1 | 2026-10-04 |
| Aşamalar da adıyla anılır | `INSTRUCTIONS.md` §2 | 2026-10-04 |
| Merge yetkisi ve yönetici düzeni — biçimi (yetkinin kendisi aşamayla sınırlı, K-648) | `INSTRUCTIONS.md` §9 · `GUARDRAILS.md` §3, §6 | 2026-10-04 |
| Güncel Durum değiştirilir, eklenmez; önceki adım arşive | `INSTRUCTIONS.md` §7 · `memory/MEMORY_ARCHIVE.md` | 2026-10-04 |
| Aşama 2'nin süreç kararları (K-652, K-670, K-682, K-718) ve öğrenimleri | `checklists/document-stage.md` §1, §3, §4, §7 · `skills/audit`, `checkpoint`, `cross-review` | 2026-10-04 |
| Proje sahibine giden plan bilgidir; cevap beklenen üç şey; `00`/`CLAUDE.md`/`SETUP.md` önerisi sade dille — "sorun ne / ne değişir" (kullanıcı hafızası: "yalnız çok kritik konuları sor" notunun 2026-10-04 eki) | `INSTRUCTIONS.md` §9 (işletim kuralı 1) · `checklists/document-stage.md` §7 (5. adım) | 2026-10-05 |
| Aşama 3'ün süreç kararları (K-727, K-738, K-739, K-778, K-796, K-847…K-849) ve öğrenimleri — matrisin okuma derinliği, büyük dokümanda cross-review, geri besleme yüzleri, yönetici düzeninin işletim kuralları, CR ve `$1` taraması | `checklists/document-stage.md` §1–§4, §7 · `INSTRUCTIONS.md` §7, §9 · `skills/cross-review`, `audit`, `checkpoint` | 2026-10-05 |
