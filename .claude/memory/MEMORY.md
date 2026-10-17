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
- **Son adım — izlenebilirlik matrisinin 1d oturumu (karar kaydı v0.121, Veri Modeli v0.5, 2026-10-17; K-1003):** devirler satır olarak `06 §1.1.11`'e yazıldı — 172 satır: etki sütununda `06` taşıyan 126 karar satırı (Aşama 1 45, Aşama 2 23, Aşama 3 14, Aşama 4 44), park bloklarının 23 talimat maddesi, 23 gövde cümlesi — dizinlerin 21'i ve dizinlerden sonra Aşama 4'ün kapanışında eklenen iki (`02 §1.1` m4 — K-977; `05 §3.5.2` — K-982); eksik, fazla ve yinelenen sıfır. Birimin 1d netleşmesi K-1003 (K-997'ye geri işaret): `K-n`, `park An-m`, `0X §bölüm gN`; önceki satırla karşılanan devir ayrı satırla girer, "Alan" hücresi "→" ile karşılayan `02`/`03` satırını ve park satırını gösterir. `dev()`, `park()`, `gov()`; önceki oturumların kümeleri eski çıktıyla satır satır aynı (`cmp`); betik iki yönlü sınandı. Satırların tamamı okundu — etki sütununda yalnız `06` taşıyan 44'ü gerekçe hücresiyle; kesilmiş okuma yok. "Bu aşamanın kararıdır" diyen her madde §14.3'te — ★ yok. Aday 70 eşlendi; 71 (anahtar halkası) yalnız K-975'in (4). kolunda, park satırı yok (kaynağı `05 §7.6.3`). Yeni GAP adayı yok — VA-1'e K-617 ve K-645'in, VA-2'ye K-527'nin elediği seçenek ve K-585'in gerekçe cümlesi dayanak olarak eklendi (VA-2'nin eski adres parçası önceki kararın öncülünden ayrılır — ⚠ grubu). Öğrenim adayı 6 kayda geçti — devir dizini kapanışın anlık görüntüsüdür (§14.1). ⚠ karar yok; çok kritik soru çıkmadı.
- **SIRADA — izlenebilirlik matrisinin 2. oturumu** (`06 §1.2`, §1.3; K-998): geri izlenebilirlik — `Entity.alan` → kaynak; §1.1.2'nin 71 adayı ve §1.1.3–§1.1.11'in 1.550 satırı (1a 670, 1b 411, 1c 297, 1d 172) —, gerekçesiz ekleme listesi (71'in park satırsızlığı dahil), kesilmiş okumanın örneklemi (1a–1d'de kesilmiş okuma yok — checklist §2'nin örneklemi uygulanmaz, kayda yazılır), GAP kararları — §3'e GAP-12'den, `06 §1.3`'e (VA-1, VA-2; belirgin önerisi olan ve W konusuna bağlanmayan K-992 moduyla 2. oturumda, W'ye bağlanan o konuda); üst dokümana dönen karar aynı PR'da geri beslenir (K-652, K-994). Ardından workshop (VM2 → VM5 → VM4 → VM9 → VM8). Kapısı Aşama 5'in öğrenim terfisi olanlar: Ö-23 ve §14.1'in altı adayı. Desteklenen ödeme sağlayıcısının sayısı bu aşamanın sorusu değildir (`08`'in K-932 park satırı). **D-01** — kapısı ilk implementation task'ından önce; `CONTEXT.md` §1 ve `SETUP.md` onunla değişir.
- **Dokümanlar:** Proje Vizyonu (`01`) **v0.33** · Ürün Gereksinimleri (`02`) **v0.76** · Kullanıcı Akışları (`03`) **v0.28** · Arayüz Tanımları (`04`) **v0.28** · Teknik Mimari (`05`) **v0.17** · MVP Kapsamı (`10`) **v0.51** — altısı ✓ Tamamlandı. Veri Modeli (`06`) **⏳ v0.5** (izlenebilirlik matrisinin 1a, 1b, 1c ve 1d oturumları — `06 §1`; konu planı karar kaydı §14). Karar kaydı **v0.121**, K-01…K-1003 (Aşama 1–4'ün kayıtları salt okunur; Aşama 5 K-992'den, açık kalem A-19'dan, boşluklar GAP-12'den). Metodoloji (`00`) **v1.0.6** (Aşama 1–4'ün desenleri §N.1'de — K-853, K-991).
- **Oturum kapanışı:** kural `INSTRUCTIONS §7`'de (K-33, K-38) — her oturum ve kapanış adımı bu bloğu kendi `docs:` PR'ında günceller, alt ajan dahil.
- **Gate durumu:** Implementation başlamadı, faz yok. Doküman: **01, 02, 03, 04, 05, 10 ✓** · 06 ⏳ · 07–09, 11–12 ⬚. Aşama 1 blok planı: **0–9 ✓** (111 konu). Aşama 2 konu planı: **AK0–AK11 ✓** (62 konu; tracker §8.2–§8.3). Aşama 3 konu planı: **UI0–UI9 ✓** (57 konu; tracker §10.2–§10.3). Aşama 4 konu planı: **TM0–TM12 ✓** (54 konu — 33 W, 4 P, 17 T; yazım turu tamam; tracker §12.2–§12.3). Aşama 5 konu planı: **VM0–VM9 ⬚** (51 konu — 20 W, 5 P, 26 T; tracker §14.2–§14.3); VM1 ⏳ — matrisin 1a, 1b, 1c ve 1d oturumları ✓.
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
