# <PROJE> — Implementation Status

**Son güncelleme:** YYYY-AA-GG — `<yalnızca EN SON güncellemenin özeti; birkaç cümle>`

> ⚠️ **Bu dosya task durumu için TEK OTORİTER KAYNAKTIR.** Hafıza snapshot'ı bilgilendiricidir, otoriter değildir.
> ⚠️ **"Son güncelleme" bloğu prepend-only changelog'a DÖNÜŞTÜRÜLMEZ.** Yalnız en son giriş burada durur; tarihsel geçmiş [`STATUS_CHANGELOG.md`](STATUS_CHANGELOG.md)'de.
> **Neden:** Bir referans projede bu alan 63 KB'lık tek satıra büyüdü ve okuma araçları tarafından kırpıldı — "nerede kaldık" sorusu cevaplanamaz hâle geldi.
>
> **Okuma tavsiyesi:** Tek bir task'ın durumu için tüm dosyayı açma; ilgili satırı `grep`'le (`grep -n "^| T42 " Docs/IMPLEMENTATION_STATUS.md`).

---

## Durum lejandı

| Simge | Durum | Açıklama |
|---|---|---|
| ⬚ | Bekliyor | Henüz başlanmadı |
| ⏳ | Devam ediyor | Yapım chat'inde aktif |
| ✓ | Tamamlandı | Doğrulama PASS, merge edildi |
| ✗ | FAIL | Doğrulama başarısız |
| ⛔ | BLOCKED | İlerleyemiyor (alt tür belirtilir) |

**Doğrulama durumları:** ✓ PASS / ✗ FAIL / ⛔ BLOCKED
**Detaylı raporlar:** `Docs/TASK_REPORTS/TXX_REPORT.md`

---

## Faz gate durumu

| Faz | Ad | Task aralığı | Gate | Tag | Rapor |
|---|---|---|---|---|---|
| F0 | | | ⬚ | — | |
| F1 | | | ⬚ | — | |
| F2 | | | ⬚ | — | |
| F3 | | | ⬚ | — | |
| F4 | | | ⬚ | — | |
| F5 | | | ⬚ | — | |
| F6 | | | ⬚ | — | |

---

## Açık bulgular (cross-task)

> Bir task'ta ortaya çıkıp başka bir task/faz tarafından kapatılacak bulgular.
> **Not:** "Şimdi değil" kararıyla ertelenen işler buraya değil, [`DEFERRED_BACKLOG.md`](DEFERRED_BACKLOG.md)'ye yazılır.

| # | Kaynak | Bulgu | Çözüm yetkisi / tetikleyici |
|---|---|---|---|

**Kapatılanlar:**
- *(henüz yok)*

---

## F0 — <Faz adı>

| Task | Ad | Durum | Doğrulama | Commit | PR |
|---|---|---|---|---|---|
| T01 | | ⬚ | — | — | — |

---

## Borç kapatma paketleri (WP)

> Ayrıntı: [`DEBT_CLOSURE_PLAN.md`](DEBT_CLOSURE_PLAN.md)

| WP | Başlık | Durum | Doğrulama | Commit | PR |
|---|---|---|---|---|---|
