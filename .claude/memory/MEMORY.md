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

- **AŞAMA 3 AÇILDI — UI/UX tasarım → Arayüz Tanımları (`04`), 2026-10-04.** Rol Senior Product Designer / UX Architect; girdi `02`, `03`, `10` (üçü ✓); düzey wireframe; **izlenebilirlik matrisi zorunlu**. Aşama 1 2026-10-03'te, Aşama 2 2026-10-04'te kapandı (adımları `MEMORY_ARCHIVE.md`'de).
- **Son adım — izlenebilirlik matrisinin 2. oturumu (karar kaydı v0.76, `04` v0.4, 2026-10-04; K-732…K-739):** **matris tamamlandı.** Geri izlenebilirlik betikle kuruldu (`04 §1.2`): 54 adayın ve on beş ortak bileşen adının hepsi kaynaklı — gerekçesiz ekleme yok; aday liste 54 kimlik = 53 ekran + bir ayar (E-48 bir ayar ekranına iner; E-54 kalır) (K-738). 1a'nın satırları örneklemle doğrulandı: 82 satırda 9 ayrışma (%11) → 378 satırın tamamı anahtar sözcükle tarandı, toplam 111 satır tam okundu, **20 satır düzeltildi** — hepsi "eksik ikinci ekran". On iki GAP adayının onu doğrulandı, ikisi düştü (GA-2 kupon alanı ödeme adımındadır — `02 §3.10.1`; GA-7). **On bir GAP** (`04 §1.3`, karar kaydı §3): beşi üst dokümana geri beslendi — müşterinin geri alınamaz işlemlerinde onay (K-732 ⚠) · Google girişi kaldırılamaz (K-733 ⚠) · bekleyen e-posta değişikliği (K-734) · yönetici hesabının adı (K-736 ⚠) · ürün listesinde arama (K-737 ⚠); biri `08`'e park edildi (K-735, bildirim metinleri); beşi ekran kurgusudur ve W/T konularına bağlandı (K-739 — notlar §10.3'te). Matriste GAP adayı satırı kalmadı: 1.026 eşlendi · 167 ekran dışı.
- **SIRADA — workshop (tek oturum, yedi W konusu, karar kaydı §10.3'teki sırayla):** UI2-01 tasarım tabanı · UI2-02 mesajların biçimi (GAP-10 ve müşteri onayının kalıbı) · UI3-01 vitrin çerçevesi (GAP-6) · UI3-02 panelin gezinme iskeleti (GAP-6, GAP-7, GAP-11) · UI5-01 iki ana sayfa düzeni · UI5-02 ödeme adımının yapısı (GAP-4) · UI9-01 ⚠ yeniden gönderilebilen e-postalar. Sonra: tek yazım turu (§2 → §3 → §4 → §5 → §9 → §6 → §7 → §8) → kalite döngüsü → kapanış. Tahmin: workshop 1 + yazım 5 oturum.
- **Açık süreç maddeleri (karar kaydı §10.1):** ⚠ öneriyle kayıt listesi — **2. oturumun dört ⚠ kararı gösterilecek** (K-732, K-733, K-736, K-737; kapı: en geç Aşama 3'ün arşiv işaretinden önce) · örneklem doğrulamasının kalanı — 1a'nın tam okunmamış 267 satırı (kapı: `04 §5` ve §9'un yazımı) · Ö-23 — checklist'in skill'e dönüşmesi (kapı: Aşama 3'ün öğrenim terfisi) · öğrenim terfisi adayları (dört aday; kapı: kapanışın 5. adımı). Konu planı karar kaydının §10.2–§10.3'ünde: on blok (`UI0`…`UI9`), 57 konu — 7 W, 4 P, 46 T; çalışma modları K-723 (⚠ öneriyle kayıt, merge yetkisi, yönetici + alt ajan düzeni — Aşama 3 boyunca); plan kararları K-724…K-727; matris kararları K-728…K-739.
- **Dokümanlar:** Proje Vizyonu (`01`) **v0.32** · Ürün Gereksinimleri (`02`) **v0.56** · MVP Kapsamı (`10`) **v0.38** · Kullanıcı Akışları (`03`) **v0.14** — dördü ✓ Tamamlandı (son üç sürüm matrisin geri beslemesi) · Arayüz Tanımları (`04`) ⏳ **v0.4** (yalnız §1 — izlenebilirlik matrisi — yazılı; §2–§9 şablon). Karar kaydı **v0.76**, K-01…K-739 (Aşama 1 ve 2'nin kayıtları salt okunur; Aşama 3 K-723'ten, açık kalem A-19'dan). Metodoloji (`00`) **v1.0.4**.
- **Oturum kapanışı:** kural `INSTRUCTIONS §7`'de (K-33, K-38) — her oturum ve kapanış adımı bu bloğu kendi `docs:` PR'ında günceller, alt ajan dahil.
- **Gate durumu:** Implementation başlamadı, faz yok. Doküman: **01, 02, 03, 10 ✓** · **04 ⏳** · 05–09, 11–12 ⬚. Aşama 1 blok planı: **0–9 ✓** (111 konu). Aşama 2 konu planı: **AK0–AK11 ✓** (62 konu; tracker §8.2–§8.3). Aşama 3 konu planı: **UI1 ✓ (matris tamam)**, öteki bloklar ⬚ (57 konu; tracker §10.2–§10.3).
- **İleriye bağlayıcı kayıtlar:** **K-352** — `B9-05` ölçüm eklerse K-348 ve K-349 yeniden açılır (koşul gerçekleşmedi, K-397) · **K-400** — `B9-10` yalnız sipariş verisinden türetir · **K-420** — yol haritası yalnız `10 §3`'ten beslenir, `01 §7`'den asla · **K-424**'ün üç bağımlılık zinciri (öneri→analitik→çerez bandı · stokta haber ver ve terk edilmiş sepet→İYS onayı · SMS→yeni dış ön koşul; K-473 kargo ve havale otomasyonunu aynı kalıba koydu) · **K-439** — `01 §7`'nin sınırı ile aynı alandaki yol haritası adayı çelişmez: sınır kimliktir, aday özellik · **K-474** — tetikleyici gerçekleşince kargo entegrasyonu ve havale eşleştirmesi öne çekilir.
- **Aşağı dokümanlara devirler:** devir girdilerinin tek tablosu — Aşama 2'ye karar kaydının §7'sinde, **Aşama 3'e §9'unda**; Aşama 1 kararlarının tam dizini çakışma taramasının §6'sında (`Docs/CHECKPOINT_REPORTS/PHASE1_CONFLICT_SCAN.md`) — kalite döngüsünün 147 kararı `03`–`08`, `12` ve `DEPLOY_RUNBOOK`'a 292 devir; daha önceki devirler tracker'ın etki sütunlarında. **Aşama 2'nin dizini:** `Docs/CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md` §6 — 63 karar, 137 atıf, adsız 86 atfın işinin adı orada; hedef dokümanların park bloğu dizine işaret eder.
- **Son güncelleme:** 2026-10-04

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
