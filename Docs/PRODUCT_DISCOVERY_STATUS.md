# Shopfolio — Product Discovery Status

**Versiyon: v0.1** | **Son güncelleme:** 2026-08-11

> **Doküman üretim döneminin karar kaydıdır.** Her karar **alındığı anda** buraya yazılır — hiçbir karar kaybolmaz.
> Bu dosya sürecin sonunda arşivlenir; alınan kararların nihai hâli `01`, `02` ve `10`'da yaşar.

---

## 1. Doküman durumu

| No | Doküman | Durum | Versiyon | Audit | Deep review | Cross-review | Checkpoint |
|---|---|---|---|---|---|---|---|
| 01 | Project Vision | ⬚ | — | ⬚ | ⬚ | ⬚ | ⬚ |
| 02 | Product Requirements | ⬚ | — | ⬚ | ⬚ | ⬚ | ⬚ |
| 03 | User Flows | ⬚ | — | ⬚ | ⬚ | ⬚ | ⬚ |
| 04 | UI Specs | ⬚ | — | ⬚ | ⬚ | ⬚ | ⬚ |
| 05 | Technical Architecture | ⬚ | — | ⬚ | ⬚ | ⬚ | ⬚ |
| 06 | Data Model | ⬚ | — | ⬚ | ⬚ | ⬚ | ⬚ |
| 07 | API Design | ⬚ | — | ⬚ | ⬚ | ⬚ | ⬚ |
| 08 | Integration Spec | ⬚ | — | ⬚ | ⬚ | ⬚ | ⬚ |
| 09 | Coding Guidelines | ⬚ | — | ⬚ | ⬚ | ⬚ | ⬚ |
| 10 | MVP Scope | ⬚ | — | ⬚ | ⬚ | ⬚ | ⬚ |
| 11 | Implementation Plan | ⬚ | — | ⬚ | ⬚ | ⬚ | ⬚ |
| 12 | Validation Protocol | ⬚ | — | ⬚ | ⬚ | ⬚ | ⬚ |

**Lejant:** ⬚ Başlanmadı · ⏳ Devam ediyor · ✓ Tamamlandı

---

## 2. Karar kaydı

> Her karar: konu · seçenekler · **alınan karar** · gerekçe · tarih · etkilediği dokümanlar.

| # | Tarih | Konu | Karar | Gerekçe | Etkilediği dokümanlar |
|---|---|---|---|---|---|

---

## 3. Traceability boşlukları (GAP) ve kararları

> `04`, `06`, `07`, `11` matrislerinden çıkan boşluklar ve verilen kararlar.

| # | Kaynak matris | Boşluk | Karar | Nereye yansıdı |
|---|---|---|---|---|

---

## 4. Açık kararlar (detaylandırılacak konular)

> **Kural:** Burada yalnız **detay** açık kalabilir. "Olacak mı olmayacak mı" kararı asla açık bırakılmaz.

| # | Konu | Ne belirsiz | Ne zaman karara bağlanacak | Durum |
|---|---|---|---|---|

---

## 5. Checkpoint log

| # | Tarih | Aşama | Genel durum | Aksiyon maddeleri | Rapor |
|---|---|---|---|---|---|

---

## 6. Sonraki adımlar

- [ ] **Aşama 1 — Product Discovery** oturumunu **ayrı bir chat'te** `.claude/checklists/document-stage.md` ile başlat → `01_PROJECT_VISION`, `02_PRODUCT_REQUIREMENTS`, `10_MVP_SCOPE`
- [ ] `Docs/00_PROJECT_METHODOLOGY.md`'yi proje sahibiyle birlikte gözden geçir (SETUP §7'nin açık kalan maddesi — Aşama 1 oturumunun ilk işi)
- [ ] Aşama 1'in cross-review turunu `cursor-agent` ile koş, çıktıyı `Docs/CROSS_REVIEW_REPORTS/` altına yaz

> **Kurulum notu (2026-08-11).** Bu dosya kurulumda açıldı; §1 tablosu hazır, §2–§5 bilinçli olarak **boş**. Kurulum kararları (proje adı, dal koruma rejimi, ikinci AI, §2/§4 ertelemesi) buraya kopyalanmadı — evleri `SETUP.md` parametre tablosu, `CI_CD_SETUP.md §3.3` ve `DEFERRED_BACKLOG.md` D-01'dir. Bu dosyanın §2'si **ürün** kararlarının kaydıdır ve ilk satırını Aşama 1'de alır.
