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

- **AŞAMA 5 AÇILDI — 2026-10-17: Veri modeli → Veri Modeli (`06`)** (`00 §C.1`; rol Data / Domain Modeler; girdi `02`, `03`, `05`, `10` — dördü ✓; **izlenebilirlik matrisi zorunlu** — checklist §2). Aşama 4 2026-10-17'de kapandı — adımları ve arşiv işareti (#118, K-990, K-991) `MEMORY_ARCHIVE.md`'de.
- **Son adım — yazım turunun 3c oturumu, yazım turunun son oturumu: Veri Modeli (`06`) §9, §8, §7, §6 (karar kaydı v0.132, Veri Modeli v0.16, 2026-10-17; K-1073, K-1074):** **§9 Saklama ve arşivleme** — yirmi iki süreli kaydın başlangıç değerinin alanı ve dizini; siparişin imha parçasının alan listesi (boşaltılan alanlar, silinen altı bağlı kayıt, dokunulmayan ticari kayıtlar); öteki kayıtların süreyle imhası; imha kaydı yazan beş anlık silme ve gecelik temizliğin sekiz parçası; arşiv tablosu yoktur, yedekler ve sıfır kayıp. **Z-41'in başlangıç değeri Ö2 geçişinin geçiş kaydından okunur — ayrı alan yok (K-1074).** **§8 Seed** — on kaydın başlangıç verisi, fail-fast'ın üç denetimi (il ve ülke listesi, başlangıç verisinden doğan altı tek satır), ilk yönetici yönetim komutuyla, operatör hesabı yok, veritabanında olmayanlar, `DEPLOY_RUNBOOK` §A ve §H'nin dört satırı. **§7** — 72 tekil dizin, 129 bağ (29 basamaklı, 26 boşaltılır, 74 kısıtlayıcı), kontrol kısıtları ve yetkiler, 21 koşullu güncelleme, 49 sorgu dizini, hacim kabulleri; **§6** — Mermaid, altı diyagram, 129 çizgi, on üç bağsız tablo (biçim K-1073). §7 ve §6 §4'ten betikle toplandı ve iki yönlü karşılaştırıldı — eksik ve fazla sıfır. **Yazım turu tamam** — yedi oturum; elli bir konunun hepsi ✓; blok kapanış taramaları (VM3, VM5…VM9) ve Doküman Tamamlama Protokolü'nün üç kontrolü kayda geçti (karar kaydı §14.3). Kalite döngüsüne beş açık madde — karar kaydı §14.1 (yeniden açılan talebin silinme anı, veri koruma anahtarlarının şifrelenmesi, geç ödemenin havale yolu, `05 §5.2.9`'un görsel yüklemesi, sepet kaleminin filtreli dizini). Anlamsal tarama kırk altı Aşama 5 satırına `06 §x (taslak güncellendi — v0.16)` ekledi; geçici hedefler sıfır; matrisin kümeleri `origin/main`'le satır satır aynı; geri besleme yok; ⚠ karar yok.
- **SIRADA — Veri Modeli (`06`) kalite döngüsü** (`00 §C.5`; checklist §6): audit ve deep review yazan bağlamda koşmaz — mercekler paralel, bulgular karşı-doğrulamalı, **güvence merceği** dokümanın tamamında bir kez; cross-review'ın girdisi yalnız doküman ve şablonudur — dokümanın boyutu (~1,1 MB) için Teknik Mimari'nin kalıbı: tek parça ve parçalı iki koşum (K-953); ardından etki yansıtma. Açık maddeleri karar kaydı §14.1'de. Kapısı Aşama 5'in öğrenim terfisi olanlar: Ö-23 ve §14.1'in on üç adayı. **D-01** — kapısı ilk implementation task'ından önce.
- **Dokümanlar:** Proje Vizyonu (`01`) **v0.33** · Ürün Gereksinimleri (`02`) **v0.83** · Kullanıcı Akışları (`03`) **v0.32** · Arayüz Tanımları (`04`) **v0.31** · Teknik Mimari (`05`) **v0.20** · MVP Kapsamı (`10`) **v0.54** — altısı ✓ Tamamlandı. Veri Modeli (`06`) **⏳ v0.16** (izlenebilirlik matrisi tamam — `06 §1`; workshop tamam — iki oturum; yazım turunun 1. oturumu — başlık notu, §2, §3, §5; 2a oturumu — §4'ün girişi, §4.1…§4.16; 2b1 oturumu — §4.17…§4.21; 2b2 oturumu — §4.22…§4.35; 3a oturumu — §4.36…§4.53; 3b oturumu — §4.54…§4.72; 3c oturumu — §9, §8, §7, §6 — **yazım turu tamam**; konu planı karar kaydı §14). Karar kaydı **v0.132**, K-01…K-1074 (Aşama 1–4'ün kayıtları salt okunur; Aşama 5 K-992'den, açık kalem A-19'dan; boşluklar GAP-12, GAP-13 — sıradaki GAP-14). Metodoloji (`00`) **v1.0.6** (Aşama 1–4'ün desenleri §N.1'de — K-853, K-991).
- **Oturum kapanışı:** kural `INSTRUCTIONS §7`'de (K-33, K-38) — her oturum ve kapanış adımı bu bloğu kendi `docs:` PR'ında günceller, alt ajan dahil.
- **Gate durumu:** Implementation başlamadı, faz yok. Doküman: **01, 02, 03, 04, 05, 10 ✓** · 06 ⏳ · 07–09, 11–12 ⬚. Aşama 1 blok planı: **0–9 ✓** (111 konu). Aşama 2 konu planı: **AK0–AK11 ✓** (62 konu; tracker §8.2–§8.3). Aşama 3 konu planı: **UI0–UI9 ✓** (57 konu; tracker §10.2–§10.3). Aşama 4 konu planı: **TM0–TM12 ✓** (54 konu — 33 W, 4 P, 17 T; yazım turu tamam; tracker §12.2–§12.3). Aşama 5 konu planı: **VM0–VM9 ✓** (51 konu — 20 W, 5 P, 26 T; tracker §14.2–§14.3); **VM1 ✓** — matrisin altı oturumu (2026-10-17); **VM2 ✓**, VM5'in W'leri ✓ — workshop 1 (2026-10-17, K-1010…K-1018); VM4, VM9, VM8'in W'leri ✓ — workshop 2 (2026-10-17, K-1019…K-1031) — **workshop tamam (20 W)**; **VM0, VM3, VM5 ✓** — yazım turunun 1. oturumu (2026-10-17, K-1032…K-1036; 9 T); yazım 2a — §4.1…§4.16 (2026-10-17, K-1037…K-1044; 2 T — VM4-04, VM4-05); yazım 2b1 — §4.17…§4.21 (2026-10-17, K-1045…K-1052; 1 T — VM4-06; K-1052 ⚠); yazım 2b2 — §4.22…§4.35 (2026-10-17, K-1053…K-1059; 1 T — VM4-08; K-1056…K-1058 ⚠); yazım 3a — §4.36…§4.53 (2026-10-17, K-1060…K-1065; T yok — VM4-09 ve VM4-10'un alanları; K-1064, K-1065 ⚠); yazım 3b — §4.54…§4.72 (2026-10-17, K-1066…K-1072; 3 T — VM4-12, VM4-14, ★VM4-16; **VM4 ✓**; ⚠ yok); yazım 3c — §9, §8, §7, §6 (2026-10-17, K-1073, K-1074; 5 T — VM9-01, VM9-03, VM8-02, VM7-01, VM6-01; **VM3, VM5…VM9 ✓**; ⚠ yok) — **yazım turu tamam, 51 konunun hepsi ✓**; sıradaki kalite döngüsü.
- **İleriye bağlayıcı kayıtlar:** **K-352** — `B9-05` ölçüm eklerse K-348 ve K-349 yeniden açılır (koşul gerçekleşmedi, K-397) · **K-400** — `B9-10` yalnız sipariş verisinden türetir · **K-420** — yol haritası yalnız `10 §3`'ten beslenir, `01 §7`'den asla · **K-424**'ün üç bağımlılık zinciri (öneri→analitik→çerez bandı · stokta haber ver ve terk edilmiş sepet→İYS onayı · SMS→yeni dış ön koşul; K-473 kargo ve havale otomasyonunu aynı kalıba koydu) · **K-439** — `01 §7`'nin sınırı ile aynı alandaki yol haritası adayı çelişmez: sınır kimliktir, aday özellik · **K-474** — tetikleyici gerçekleşince kargo entegrasyonu ve havale eşleştirmesi öne çekilir · **K-10** — MVP'de çok kiracılık için hazırlık yapılmaz (`05` park satırı).
- **Aşağı dokümanlara devirler:** devir girdilerinin tek tablosu — Aşama 2'ye karar kaydının §7'sinde, Aşama 3'e §9'unda, Aşama 4'e §11'inde, **Aşama 5'e §13'ünde** — açılışta konulara betikle eşlendi (§14.2); satır düzeyindeki eşleme matrisin 1d oturumunda yapıldı (`06 §1.1.11`, 172 satır; K-1003). Dizinler: Aşama 1 `Docs/CHECKPOINT_REPORTS/PHASE1_CONFLICT_SCAN.md` §6 (kalite döngüsünün 147 kararı, 292 devir; `05`'e 25 karar) · Aşama 2 `PHASE2_CONFLICT_SCAN.md` §6 (63 karar, 137 atıf; `05`'e 7) · Aşama 3 `PHASE3_CONFLICT_SCAN.md` §6 (40 karar, 53 atıf; `05`'e 9, `04`'ün gövdesinde dört cümle — biri karar satırsız: genişlik sınıflarının uygulanması, `04 §2.1.1`) · **Aşama 4 `PHASE4_CONFLICT_SCAN.md` §6** (111 karar, 204 atıf — Veri Modeli 39, API Tasarımı 20, Entegrasyon Spesifikasyonu 20, Kodlama Kılavuzu 44, Doğrulama Protokolü 20, `DEPLOY_RUNBOOK` 54 karar; D-01'e `05 §2.3`). `06`'nın başında dört park bloğu (Aşama 1–4); hedef dokümanların park bloğu dizine işaret eder.
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
| Birden çok öneri, birden çok numaralı soru — tek "evet" birden çok öneriye okunmaz (kullanıcı hafızası: "toplu öneride evet ret sayılmaz" notu; Aşama 3'ün öğrenim adayı 13) | `INSTRUCTIONS.md` §2 · `checklists/document-stage.md` §7 (5. adım) | 2026-10-17 |
| Aşama 4'ün süreç kararları (K-860, K-952, K-953, K-976) ve öğrenimleri — matrissiz aşamada devirlerin açılışta eşlenmesi, sorunun öncülü, kısıt soruları açılışta, güvence merceği, geri beslemenin dört yeni yüzü, park satırının kolları, eşiğin altında 1. turun ölçümü, hukuki kontrol ve web araması, betiğin iki yönlü sınanması | `checklists/document-stage.md` §1, §3, §4, §7, Teknik mimari hatırlatması · `INSTRUCTIONS.md` §2 · `skills/audit`, `cross-review` | 2026-10-17 |
