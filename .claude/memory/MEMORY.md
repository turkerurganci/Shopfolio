# Proje Hafızası

> **Bu dosya indeks + güncel durum snapshot'ıdır.** İçerik gömülmez — her hafıza ayrı bir dosyadır.
> Ne buraya girer / ne L1–L5'e terfi eder: [`README.md`](README.md).
> **Otoriter kaynak değildir** — task durumu için [`../../Docs/IMPLEMENTATION_STATUS.md`](../../Docs/IMPLEMENTATION_STATUS.md).

---

## Proje Özeti

- **Ad:** Shopfolio
- **Tanım:** Herhangi bir firmanın kendi ürünlerini sergileyip çevrimiçi satabileceği ve kurumsal tanıtımını yapabileceği; üyelik (sipariş için zorunlu değil), Google ile giriş, sipariş, ödeme ve sipariş takibi içeren web uygulaması.
- **Dönem:** Implementation
- **Doküman dili:** Türkçe · **Kod dili:** İngilizce

---

## Güncel Durum

> Kısa tut. Tarihsel detay `MEMORY_ARCHIVE.md`'ye taşınır (00 §G.2).

- **2026-11-10 — F3 (İş mantığı) faz kapısı: ✓ PASS** (`Docs/CHECKPOINT_REPORTS/GATE_CHECK_F3.md`; gate PR'ı #513, squash ve `phase/F3-pass` etiketi merge'de yardımcıda; taban `origin/main` `ad7b10e`). Metrikler: 90/90 task (97 merge); Release `-warnaserror` 0 uyarı, format üç proje temiz; Unit 5791, Integration 3487 (parçalı), betik 4 dosya; 19 göç (F3'te yeni 1: yalnız indeks), ikinci uygulama no-op, F2 şemasından özdeş; paket denetimi 7/7 (app imajı 792 → 315 MB, D-160); `12 §6.2` F3 B (T329 `MainFlowTests` + `BranchFlowTests`) ve C (`12 §12.3` 16 geçiş = `TransitionTable`) ✓; `11 §7.1` F3 883 satır dolu, `GATE_DOLU` += F3. Gate'te düzeltilen: `StatusEndpointTests` paylaşılan veritabanını bozuk bırakıyordu, `package-build.sh` Git Bash sır klasörü, rapor/status kayıt satırları, para dışı backlog düzeltmeleri. Backlog: F3 kapısına bırakılmış 105 kalem — 73 ✅, 32 yeniden hedef; yeni F4 task'ları T411 (kayıt maskeleme taraması) ve T412 (para; sipariş bekçisi, satış sonrası kapanış, IBAN kullanımı), T601b; K-1579…K-1604; D-159 doküman geçişi; yeni D-980…D-982; aktif 285. Kabul edilen riskler: `12 §12.3`'ün ~50 VAL-C satırının yalnız 2'si PASS (D-981), 568 zayıf matris kanıtı (D-982).
- **SIRADA — F4 (Entegrasyonlar): gate PR'ının merge'ü ve `phase/F3-pass` etiketinden (yardımcı) sonra ilk task `/task T412` (F3 kapanış düzeltmeleri, para); yürütme sırası T412 → T411 → T401 … T410 (`11` F4). En geç F4 gate'e bağlanan kalemler: D-778, D-919, D-961, D-981, D-982.**
- **Dokümanlar:** `01` v0.34 · `02` v0.115 · `03` v0.49 · `04` v0.54 · `05` v0.52 · `06` v0.48 · `07` v0.42 · `08` v0.23 · `09` v0.32 · `10` v0.63 · `11` v0.64 · `12` v0.35 — hepsi ✓ Tamamlandı (implementation döneminde yalnız geri besleme sürümü artar). Doküman dönemi sürüm geçmişi `MEMORY_ARCHIVE.md`'de.
- **Oturum kapanışı:** kural `INSTRUCTIONS §7`'de (K-33, K-38) — her oturum ve kapanış adımı bu bloğu kendi `docs:` PR'ında günceller, alt ajan dahil.
- **Gate durumu:** F0 ✓ (2026-11-03), F1 ✓ (2026-11-04), F2 ✓ (2026-11-07), **F3 ✓ PASS (2026-11-10)**; F4 ⬚ (12 task, T401–T412), F5 ⬚ (49), F6 ⬚ (4, T601b dahil). Otoriter kaynak `Docs/IMPLEMENTATION_STATUS.md`.
- **İleriye bağlayıcı kayıtlar:** **K-352** — `B9-05` ölçüm eklerse K-348 ve K-349 yeniden açılır (koşul gerçekleşmedi, K-397) · **K-400** — `B9-10` yalnız sipariş verisinden türetir · **K-420** — yol haritası yalnız `10 §3`'ten beslenir, `01 §7`'den asla · **K-424**'ün üç bağımlılık zinciri (öneri→analitik→çerez bandı · stokta haber ver ve terk edilmiş sepet→İYS onayı · SMS→yeni dış ön koşul; K-473 kargo ve havale otomasyonunu aynı kalıba koydu) · **K-439** — `01 §7`'nin sınırı ile aynı alandaki yol haritası adayı çelişmez: sınır kimliktir, aday özellik · **K-474** — tetikleyici gerçekleşince kargo entegrasyonu ve havale eşleştirmesi öne çekilir · **K-10** — MVP'de çok kiracılık için hazırlık yapılmaz (`05` park satırı).
- **Son güncelleme:** 2026-11-10 (F3 gate)

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
