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
