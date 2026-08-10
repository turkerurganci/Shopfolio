# <PROJE> — Ertelenmiş İşler Backlog'u

**Son güncelleme:** YYYY-AA-GG | **Aktif kalem:** 0 | **Gate'i bloklayan:** 0

> **Amaç:** Bilinçli olarak ertelenen **her** somut işin tek izlenebilir listesi.
>
> **Neden gerekli:** Validator ve gate bulgularının bir kısmı "gerçek ama şimdi değil" kararıyla kapanır. Bu karar yalnızca task raporunun içinde kalırsa **buharlaşır** — yüz rapor sonra kimse o satırı bulamaz.
>
> **Kritik ayrım:**
> - **Ertelenen iş** → buraya. MVP kapsamında ama sonraya bırakıldı.
> - **Kapsam dışı** → [`10_MVP_SCOPE.md`](10_MVP_SCOPE.md) §3. Erteleme değil, MVP tanımı gereği hariç.
> - **Tamamlanan iş** → [`IMPLEMENTATION_STATUS.md`](IMPLEMENTATION_STATUS.md).
>
> Bu ayrım borç kapatma aşamasında (00 §I) hayati önem kazanır: neyin kapatılacağı buradan çıkar.

---

## Lejant

- **Öncelik:** 🔴 high · 🟡 medium · ⚪ low · ✅ çözüldü
- **Tip:** `task` (planlı görev) · `backend-gap` (kod boşluğu) · `k-note` (validator notu) · `code-todo` · `doc-drift` (doküman ↔ kod uyumsuzluğu) · `test-gap` · `bypass`
- **Bloklar mı:** kalemin engellediği şey; "—" = hiçbir şeyi bloklamıyor

---

## Kayıt kuralları

1. **Kanıt zorunlu.** Her kalem bir kod referansı (dosya:satır) veya rapor bağlantısı taşır. Kanıtsız kalem bir süre sonra doğrulanamaz hâle gelir.
2. **Satır silinmez.** Çözülen kalem `✅ Çözüldü → <nerede>` olarak işaretlenir. İzlenebilirlik korunur.
3. **Backlog forward zorunlu.** Validator ve gate'te çıkan bloke-etmeyen her bulgu ya o PR'da kapatılır ya buraya yazılır. **Üçüncü seçenek yoktur.**
4. **Hijyen taraması.** Her faz gate'inde açık kalemler kod düzeyinde yeniden doğrulanır — bir kısmı araya giren işlerle çözülmüş, bir kısmı stale olmuş olur (açıklama artık gerçeği yansıtmıyor).

---

## 🔝 Öne çıkanlar (dikkat gerektiren)

| Önc. | ID | Özet | Bloklar mı |
|---|---|---|---|

---

## 1. <Grup adı — ör. faz, modül veya konu>

| Önc. | ID | Açıklama | Tip | Hedef | Kanıt / kaynak |
|---|---|---|---|---|---|

---

## Post-MVP parkı

> MVP kapanışında (00 §L adım 2) buraya taşınan, bilinçli olarak sonraki sürüme bırakılan kalemler.

| Önc. | ID | Açıklama | Tip | Kanıt / kaynak |
|---|---|---|---|---|
