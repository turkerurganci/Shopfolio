# Cross-Review Raporları

Bağımsız ikinci AI review turlarının çıktıları: `XX_CROSS_REVIEW.md`, `XX_CROSS_REVIEW_R2.md`, …

Her rapor şunları içerir:

1. **Ham bulgular** — ikinci modelin çıktısı, düzenlenmeden
2. **Bağımsız değerlendirme tablosu** — her bulgu için ✅ KABUL / ❌ RET / ⚠️ KISMİ + **gerekçe**
3. **Ek bulgular** — ikinci modelin kaçırdığı, okurken fark edilenler
4. **Kullanıcı onay checklist'i** — hangi düzeltmeler uygulanacak
5. **Etki yansıtma sonucu** (son turda) — downstream + upstream + yeni alan taraması

**Objektivite kuralı:** %100 KABUL de %100 RET de şüphelidir. RET gerekçesi somut referans olmalıdır ("ben böyle düşünüyorum" yetersiz).

Prosedür: [`.claude/skills/cross-review/SKILL.md`](../../.claude/skills/cross-review/SKILL.md)

- `09_CROSS_REVIEW.md` — Kodlama Kılavuzu (`09` v0.18 → v0.19), 1. tur, 2026-10-30: Codex iki koşum, 7 ayrı bulgu, 3 kabul, 4 ret; 6 doğrulayıcı; TEMİZ DEĞİL.
- `09_CROSS_REVIEW_R2.md` — Kodlama Kılavuzu (`09` v0.19 → v0.20), 2. tur, 2026-10-30: Codex iki koşum, 8 ayrı bulgu, 1 kabul + 3 kısmi, 4 ret (2 tekrar eden RET); 6 doğrulayıcı; TEMİZ DEĞİL.
- `09_CROSS_REVIEW_R3.md` — Kodlama Kılavuzu (`09` v0.20 → v0.21), 3. tur, 2026-10-30: Codex iki koşum, altı çağrının dördü TEMİZ, iki tekrar eden RET (F1, F5); doğrulayıcı yok; TEMİZ — ciddiyet ölçüsü ve tekrar eden RET kuralıyla. Etki yansıtma sonucu (Faz 5; `09` v0.22; K-1389, K-1390) rapor sonunda.
- `11_CROSS_REVIEW.md` — Implementation Planı (`11` v0.37 → v0.38), 1. tur, 2026-10-31: Codex iki koşum, 9 ayrı bulgu, 1 kabul + 3 kısmi (11 şartname çifti), 5 ret; 7 doğrulayıcı; TEMİZ DEĞİL.
- `11_CROSS_REVIEW_R2.md` — Implementation Planı (`11` v0.38 → v0.39), 2. tur, 2026-10-31: Codex iki koşum, 10 ayrı bulgu + 1. turun EK-1'i, 1 kabul + 2 kısmi + EK-1 kabul (16 şartname çifti), 7 ret; 9 doğrulayıcı; TEMİZ DEĞİL.
- `11_CROSS_REVIEW_R3.md` — Implementation Planı (`11` v0.39 → v0.40), 3. tur, 2026-10-31: Codex iki koşum, 13 ayrı bulgu, 2 kabul + 3 kısmi (7 şartname çifti), 8 ret (6 tekrar eden RET); 8 doğrulayıcı; TEMİZ DEĞİL.
