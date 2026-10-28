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

- **AŞAMA 7 AÇILDI — 2026-10-29: Entegrasyon spesifikasyonları → Entegrasyon Spesifikasyonları (`08`)** (`00 §C.1`; rol Integration Engineer; girdi `02`, `03`, `05`, `06`, `07` — beşi ✓; izlenebilirlik matrisi zorunlu değil — checklist §2 işletilmez, GAP beklenmez). Aşama 6 2026-10-29'da kapandı — adımları ve arşiv işareti (#176, K-1231, K-1232) `MEMORY_ARCHIVE.md`'de.
- **Son adım — Aşama 7 açılışı (karar kaydı v0.174, 2026-10-29; K-1233, K-1234):** **K-1233** (proje sahibi, üç ayrı soruyla) — ⚠ öneriyle kayıt, `docs:` PR'larında CI yeşilse merge yetkisi ve yönetici + alt ajan düzeni Aşama 7 boyunca açık; Aşama 7'nin arşiv işaretiyle sona erer, sonraki aşamanın açılışında yeniden sorulur; alt ajan modeli kalıcı kuraldır ve bu kararın parçası değildir. Karar kaydına **§18** açıldı: açık süreç maddeleri (§18.1 — Ö-23 ve `07 §10` geri yazım beklentisi dahil), konu planı (§18.2 — **43 konu**: 9 W, 6 P, 28 T; 6 ★; konu kimliği `ENTn-mm`, blok numarası = `08`'in bölüm numarası), blok içerikleri (§18.3). Altı plan önerisi öneriyle kaydedildi (K-1234): iskelet ve sıra, şablonun taşımadığı üç yer, kimlikler, e-posta şablonlarının yeri, servis tanımı alanlarının üç servise uygulanışı, runbook bağlantıları. PF-117…PF-121. Tamlık taraması betikle yapıldı: etki sütununda `08` taşıyan 82 karar satırı, 23 park maddesi, 4 karar satırsız dizin işi, 353 gövde anması konulara eşlendi; eşlenmeyen yok. **Çok kritik soru açık:** desteklenen ödeme sağlayıcısının sayısı — L1–L5'te cevap yok, taslak §18.1'de, proje sahibine yönetici iletir; kapısı `ENT2-02`, `ENT2-05`, `ENT2-08`, `ENT2-13` ve `08 §2.1`. "Ücretsiz seçenek yeter mi?" soru olmaktan çıktı (workshop'un ilk konusu, `ENT2-15`). ⚠ listesi boş. `08`'e dokunulmadı.
- **SIRADA — çok kritik sorunun iletilmesi ve workshop** (`checklists/document-stage.md` §3; K-1234 (1)): yönetici §18.1'deki soruyu proje sahibine iletir; ardından tek workshop oturumu — `ENT2-15` (ücretsiz seçenek yeter mi) önce, cevapla `ENT2-02` → `ENT2-09` → `ENT2-13`, sonra `ENT2-16` → `ENT2-18` → `ENT2-19` → `ENT2-26` (9 W). Ardından beş oturumluk tek yazım turu (başlık notu → §1 → §2 → §3 → §4 → §5; `07`'ye geri yazılacak uç çıkarsa `07 §10` aynı PR'da), özet ↔ ayrıntı taraması ve kalite döngüsü. Ö-23 ve `07 §10` beklentisi §18.1'de açık. **D-01** — kapısı ilk implementation task'ından önce.
- **Dokümanlar:** Proje Vizyonu (`01`) **v0.33** · Ürün Gereksinimleri (`02`) **v0.94** · Kullanıcı Akışları (`03`) **v0.40** · Arayüz Tanımları (`04`) **v0.42** · Teknik Mimari (`05`) **v0.30** · Veri Modeli (`06`) **v0.33** · MVP Kapsamı (`10`) **v0.55** — sekizi ✓ Tamamlandı (API Tasarımı dahil). Entegrasyon Spesifikasyonları (`08`) **⏳** (şablon v0.1; konu planı karar kaydı §18). API Tasarımı (`07`) **✓ Tamamlandı** (v0.26 — kapanış sürümü; v0.25'e kadar: izlenebilirlik matrisi tamamlandı — `07 §1`, GAP-14; workshop tamamlandı — K-1162…K-1172; yazım turu tamamlandı — 1. oturum, 2a, 2b, 3a, 3b ve 4 ✓, `07` başlık notu, §2…§10; özet ↔ ayrıntı taraması ve hükmü ✓; audit ✓ (K-1199…K-1206); deep review ✓ (K-1207…K-1217); cross-review 1. tur ✓ (K-1218, K-1219; TEMİZ değil); cross-review 2. tur ✓ (K-1220; TEMİZ değil); cross-review 3. tur ✓ (K-1221; TEMİZ değil); cross-review 4. tur ✓ (K-1222; TEMİZ değil); cross-review 5. tur ✓ (K-1223; TEMİZ — çıkış kuralıyla, 5 tur); etki yansıtma ✓ (K-1224…K-1226); kalite döngüsü tamam; çakışma taraması ✓ (K-1227, K-1228); checkpoint ✓ (K-1229); öğrenim terfisi ✓ (K-1230); arşiv işareti ve devir ✓ (K-1231, K-1232); konu planı karar kaydı §16). Karar kaydı **v0.174**, K-01…K-1234 (Aşama 1–6'nın kayıtları salt okunur; Aşama 7 K-1233'ten, açık kalem A-19'dan, boşluklar GAP-15'ten). Metodoloji (`00`) **v1.0.8** (Aşama 1–6'nın desenleri §N.1'de — K-853, K-991, K-1143, K-1232).
- **Oturum kapanışı:** kural `INSTRUCTIONS §7`'de (K-33, K-38) — her oturum ve kapanış adımı bu bloğu kendi `docs:` PR'ında günceller, alt ajan dahil.
- **Gate durumu:** Implementation başlamadı, faz yok. Doküman: **01, 02, 03, 04, 05, 06, 07, 10 ✓** · 08 ⏳ · 09, 11–12 ⬚. Aşama 1 blok planı: **0–9 ✓** (111 konu). Aşama 2 konu planı: **AK0–AK11 ✓** (62 konu; tracker §8.2–§8.3). Aşama 3 konu planı: **UI0–UI9 ✓** (57 konu; tracker §10.2–§10.3). Aşama 4 konu planı: **TM0–TM12 ✓** (54 konu — 33 W, 4 P, 17 T; yazım turu tamam; tracker §12.2–§12.3). Aşama 5 konu planı: **VM0–VM9 ✓** (51 konu — 20 W, 5 P, 26 T; tracker §14.2–§14.3); **VM1 ✓** — matrisin altı oturumu (2026-10-17); **VM2 ✓**, VM5'in W'leri ✓ — workshop 1 (2026-10-17, K-1010…K-1018); VM4, VM9, VM8'in W'leri ✓ — workshop 2 (2026-10-17, K-1019…K-1031) — **workshop tamam (20 W)**; **VM0, VM3, VM5 ✓** — yazım turunun 1. oturumu (2026-10-17, K-1032…K-1036; 9 T); yazım 2a — §4.1…§4.16 (2026-10-17, K-1037…K-1044; 2 T — VM4-04, VM4-05); yazım 2b1 — §4.17…§4.21 (2026-10-17, K-1045…K-1052; 1 T — VM4-06; K-1052 ⚠); yazım 2b2 — §4.22…§4.35 (2026-10-17, K-1053…K-1059; 1 T — VM4-08; K-1056…K-1058 ⚠); yazım 3a — §4.36…§4.53 (2026-10-17, K-1060…K-1065; T yok — VM4-09 ve VM4-10'un alanları; K-1064, K-1065 ⚠); yazım 3b — §4.54…§4.72 (2026-10-17, K-1066…K-1072; 3 T — VM4-12, VM4-14, ★VM4-16; **VM4 ✓**; ⚠ yok); yazım 3c — §9, §8, §7, §6 (2026-10-17, K-1073, K-1074; 5 T — VM9-01, VM9-03, VM8-02, VM7-01, VM6-01; **VM3, VM5…VM9 ✓**; ⚠ yok) — **yazım turu tamam, 51 konunun hepsi ✓**; kalite döngüsü: **audit ✓** (2026-10-18, K-1075…K-1103; 11 ⚠); **deep review ✓** (2026-10-18, K-1104…K-1124; 7 ⚠); cross-review 1. tur (2026-10-18, K-1125, K-1126; TEMİZ değil); cross-review 2. tur (2026-10-18, K-1127 ⚠; TEMİZ değil); cross-review 3. tur (2026-10-18, K-1128; TEMİZ değil — tek bulgu reddedildi); cross-review 4. tur (2026-10-18, karar yok; TEMİZ değil — 1 kabul, 1 kısmi, 3 ret); **cross-review ✓** — 5. tur (2026-10-18, K-1129; TEMİZ — çıkış kuralıyla, 5 tur); **etki yansıtma ✓ — kalite döngüsü ✓** (2026-10-18, K-1130, K-1131; ⚠ yok); **kapanış: çakışma taraması ✓** (2026-10-18, K-1132…K-1136; 4 ⚠); **checkpoint ✓** (2026-10-18, K-1137…K-1140; 3 ⚠); **öğrenim terfisi ✓** (2026-10-18, K-1141; ⚠ yok); **arşiv işareti ve devir ✓** (2026-10-18, K-1142, K-1143) — **Aşama 5 kapandı**. Aşama 6 konu planı: **API0–API10 ✓ (aşama 2026-10-29'da kapandı)** (50 konu — 11 W, 5 P, 34 T; tracker §16.2–§16.3); 5 P ✓ — açılış (2026-10-18, K-1146…K-1150); **API1** — matris 1a ✓ (2026-10-18, K-1151…K-1153), 1b ✓ (2026-10-18, K-1154, K-1155), 1c ✓ (2026-10-18, K-1156), 1d ✓ (2026-10-18, K-1157), 1e ✓ (2026-10-18, K-1158), 1f ✓ (2026-10-18, K-1159), 2 ✓ (2026-10-18, K-1160, K-1161 — GAP-14; K-1161 ⚠) — **matris tamamlandı**; **workshop ✓** (2026-10-18, K-1162…K-1172 — 11 W; API2-03 ⚠); yazım 1 ✓ (2026-10-18, K-1173…K-1176 — 10 T; API0, API2, API3, API7, API8 ✓); yazım 2a ✓ (2026-10-18, K-1177…K-1182 — `07 §4.1`…§4.19; 3 T — API4-01, API4-03, API4-06; ⚠ yok); yazım 2b ✓ (2026-10-18, K-1183…K-1186 — `07 §4.20`…§4.34; 5 T — API4-02, API4-05, API4-08, API4-09, API4-10; K-1185 ⚠); yazım 3a ✓ (2026-10-18, K-1187…K-1192 — `07 §9.1`…§9.12; 1 T — API9-03; ⚠ yok); yazım 3b ✓ (2026-10-18, K-1193…K-1195 — `07 §9.13`…§9.26; 5 T — API9-01, API9-02, API9-04, API9-05, API9-06; **API9 ✓**; ⚠ yok); yazım 4 ✓ (2026-10-19, K-1196…K-1198 — API5, API6, API10; **yazım turu tamam**; ⚠ yok); özet ↔ ayrıntı taraması ✓ (2026-10-19; yeni K yok); taramanın hükmü ✓ (2026-10-19; yeni K yok). Aşama 7 konu planı: **ENT0–ENT5** (43 konu — 9 W, 6 P, 28 T; tracker §18.2–§18.3); açılış ✓ (2026-10-29, K-1233, K-1234); çok kritik soru (ödeme sağlayıcısının sayısı) açık.
- **İleriye bağlayıcı kayıtlar:** **K-352** — `B9-05` ölçüm eklerse K-348 ve K-349 yeniden açılır (koşul gerçekleşmedi, K-397) · **K-400** — `B9-10` yalnız sipariş verisinden türetir · **K-420** — yol haritası yalnız `10 §3`'ten beslenir, `01 §7`'den asla · **K-424**'ün üç bağımlılık zinciri (öneri→analitik→çerez bandı · stokta haber ver ve terk edilmiş sepet→İYS onayı · SMS→yeni dış ön koşul; K-473 kargo ve havale otomasyonunu aynı kalıba koydu) · **K-439** — `01 §7`'nin sınırı ile aynı alandaki yol haritası adayı çelişmez: sınır kimliktir, aday özellik · **K-474** — tetikleyici gerçekleşince kargo entegrasyonu ve havale eşleştirmesi öne çekilir · **K-10** — MVP'de çok kiracılık için hazırlık yapılmaz (`05` park satırı).
- **Aşağı dokümanlara devirler:** devir girdilerinin tek tablosu — Aşama 2'ye karar kaydının §7'sinde, Aşama 3'e §9'unda, Aşama 4'e §11'inde, Aşama 5'e §13'ünde, **Aşama 6'ya §15'inde**, **Aşama 7'ye §17'sinde** — Aşama 6'nın açılışında konulara betikle eşlendi (§16.2 — 51 karar satırı, 14 park maddesi, 4 dizin satırı, 19 gövde cümlesi ve `08`'in K-906 · K-973 satırı); satır düzeyindeki eşleme matrisin 1f oturumunda yapıldı (`07 §1.1.11` — 90 satır; API1-06; K-1159). Aşama 5'in eşlemesi `06 §1.1.11`'de (172 satır; K-1003). Dizinler: Aşama 1 `Docs/CHECKPOINT_REPORTS/PHASE1_CONFLICT_SCAN.md` §6 (kalite döngüsünün 147 kararı, 292 devir; `05`'e 25 karar) · Aşama 2 `PHASE2_CONFLICT_SCAN.md` §6 (63 karar, 137 atıf; `05`'e 7) · Aşama 3 `PHASE3_CONFLICT_SCAN.md` §6 (40 karar, 53 atıf; `05`'e 9, `04`'ün gövdesinde dört cümle — biri karar satırsız: genişlik sınıflarının uygulanması, `04 §2.1.1`) · Aşama 4 `PHASE4_CONFLICT_SCAN.md` §6 (111 karar, 204 atıf — Veri Modeli 39, API Tasarımı 20, Entegrasyon Spesifikasyonu 20, Kodlama Kılavuzu 44, Doğrulama Protokolü 20, `DEPLOY_RUNBOOK` 54 karar; D-01'e `05 §2.3`). `06`'nın başında dört park bloğu (Aşama 1–4); hedef dokümanların park bloğu dizine işaret eder. **Aşama 5'in dizini — API tasarımı aşamasının girdisi: `PHASE5_CONFLICT_SCAN.md` §6** (70 karar, 93 atıf — API Tasarımı 16, Entegrasyon Spesifikasyonu 10, Kodlama Kılavuzu 34, Doğrulama Protokolü 22, `DEPLOY_RUNBOOK` 11; Uygulama Planı 0); `07`, `08`, `09`, `12` ve `DEPLOY_RUNBOOK`'un Aşama 5 bloğu dizine işaret eder. **Aşama 6'nın dizini — Entegrasyon spesifikasyonları aşamasının girdisi: `PHASE6_CONFLICT_SCAN.md` §6** (44 karar, 76 atıf; `08`'e 8 karar); Aşama 7'nin açılışında devirler konulara betikle eşlendi (§18.2 — etki sütununda `08` taşıyan 82 karar satırı, 23 park maddesi ve 4 karar satırsız dizin işi, 353 gövde anması; dizinlerde bulunmayan 14 karar dahil).
- **Son güncelleme:** 2026-10-29

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
| Birden çok öneri, birden çok numaralı soru — tek "evet" birden çok öneriye okunmaz (kullanıcı hafızası: "toplu öneride evet ret sayılmaz" notu; Aşama 3'ün öğrenim adayı 13) | `INSTRUCTIONS.md` §2 · `checklists/document-stage.md` §7 (5. adım) | 2026-10-17 |
| Aşama 4'ün süreç kararları (K-860, K-952, K-953, K-976) ve öğrenimleri — matrissiz aşamada devirlerin açılışta eşlenmesi, sorunun öncülü, kısıt soruları açılışta, güvence merceği, geri beslemenin dört yeni yüzü, park satırının kolları, eşiğin altında 1. turun ölçümü, hukuki kontrol ve web araması, betiğin iki yönlü sınanması | `checklists/document-stage.md` §1, §3, §4, §7, Teknik mimari hatırlatması · `INSTRUCTIONS.md` §2 · `skills/audit`, `cross-review` | 2026-10-17 |
| Model işe göre seçilir — mekanik iş ucuz modelde, taslak ve uygulama orta modelde, bulgu ve karar en güçlü modelde (kullanıcı hafızası: "basit işleri ucuz modele ver", 2026-10-18) | `INSTRUCTIONS.md` §9 ("Alt ajanların koşumu") | 2026-10-18 |
| Uzun adımda ara durum (kullanıcı hafızası; Teknik mimari aşamasında tek vaka, Veri modeli aşamasında ikinci vaka) | `INSTRUCTIONS.md` §9 ("Alt ajanların koşumu") | 2026-10-18 |
| Aşama 5'in süreç kararları (K-997…K-1005, K-1004, K-1037, K-1045, K-1060, K-1125, K-1126, K-1129, K-1130) ve öğrenimleri — matrisin birimi ve çıktıya karşı okunması, kararın özeti, öncülün tarihi, park satırları bütün aşamalarda, oturum tahmini ve önceden bölünme, alt ajanların ön planda başlatılması, kontrol karakteri taraması (yöneticinin ortak talimatından), ikinci modelin girdi ve kullanım sınırı | `checklists/document-stage.md` §1–§4 · `INSTRUCTIONS.md` §7, §9 · `skills/audit`, `deep-review`, `cross-review` | 2026-10-18 |
| Aşama 6'nın süreç kararları (K-1146…K-1229) ve öğrenimleri — devrin hedefinin adı bütün dokümanların park bloklarında aranması, açılışın gövde cümlesi sayımının bugünkü metinden alınması, kapalı liste ve sayı cümlesinin her oturumda yeniden sayılması, etki yansıtmanın gövde cümlelerini K'dan bağımsız taraması, park etiketi ↔ etki sütunu betiğinin iki yönlü olması, tabloda kaçışlı çubuk, hook çatışmasının dolanılmaması, eşzamanlı alt ajan sınırı | `checklists/document-stage.md` §1, §3, §4, §6 · `INSTRUCTIONS.md` §9 | 2026-10-28 |
