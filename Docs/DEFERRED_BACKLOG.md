# Shopfolio — Ertelenmiş İşler Backlog'u

**Son güncelleme:** 2026-08-11 | **Aktif kalem:** 1 | **Gate'i bloklayan:** 1

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
| 🔴 | D-01 | SETUP §2 + §4 kurulumda tamamlanmadı — `SETUP_COMPLETE=false` | Dönem 2 girişi (ilk implementation task'ı) |

---

## 1. Kurulum (SETUP.md)

| Önc. | ID | Açıklama | Tip | Hedef | Kanıt / kaynak |
|---|---|---|---|---|---|
| 🔴 | D-01 | **SETUP §2 + §4 ertelendi.** Doldurulmayanlar: (a) §2 — teknoloji yığını, `.gitignore`/`.gitattributes` yığın uyarlaması, projeye özgü sır dosyası desenleri, `.env.example`; (b) §4 — `LINT_CMD`, `BUILD_CMD`, `UNIT_TEST_CMD`, `INTEGRATION_TEST_CMD`, `CONTRACT_TEST_CMD`, `MIGRATION_DRYRUN_CMD`, `PACKAGE_BUILD_CMD`, `E2E_CMD`, `ci.yml` runtime kurulum adımları, `publish.yml` silinsin mi kararı, ardından `SETUP_COMPLETE=true`. **Gerekçe:** yığın Aşama 4'ün kararıdır (SETUP §2 uyarısı) — kurulumda seçmek, teknik kararı doküman aşamasından kaçırmak olurdu. **Sonuç:** CI iskeleti şu an yeşil ama hiçbir şey çalıştırmıyor; §8 kontrol 1 ve 2 bu kalem kapanana kadar geçmez. | `task` | Aşama 4 kapanışından sonra, **ilk implementation task'ından önce** | [`.github/ci-commands.env:19`](../.github/ci-commands.env#L19) (`SETUP_COMPLETE=false`) · [`SETUP.md`](../SETUP.md) §2 uyarısı, §4, §8 |

---

## Post-MVP parkı

> MVP kapanışında (00 §L adım 2) buraya taşınan, bilinçli olarak sonraki sürüme bırakılan kalemler.

| Önc. | ID | Açıklama | Tip | Kanıt / kaynak |
|---|---|---|---|---|
