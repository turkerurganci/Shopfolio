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
- `11_CROSS_REVIEW_R3.md` (sonu, "Hedefli mercek") — Implementation Planı (`11` v0.40 → v0.41), Faz 4 madde 6, 2026-10-31: 10 faz diliminde Sonnet mercek, 109 tekil bulgu; 17 doğrulayıcı: 39 doğru · 20 kısmen · 50 çürük; 59 task bloğunda 78 şartname çifti + 1 elle.
- `11_CROSS_REVIEW_R4.md` — Implementation Planı (`11` v0.41 → v0.42), 4. tur, 2026-10-31: Codex iki koşum, 8 ayrı bulgu, kabul yok, 8 ret (4 tekrar eden RET); 8 doğrulayıcı; şartname çifti 0, içerik değişmedi.
- `11_CROSS_REVIEW_R5.md` — Implementation Planı (`11` v0.42 → v0.43), 5. tur, 2026-10-31: Codex iki koşum, 13 ayrı bulgu, 2 kabul + 2 kısmi (6 şartname çifti), 9 ret (5 tekrar eden RET); 9 doğrulayıcı; TEMİZ DEĞİL, F2 için üst doküman KARAR'ı (⚠).
- `11_CROSS_REVIEW_R6.md` — Implementation Planı (`11` v0.43 → v0.44), 6. tur, 2026-10-31: Codex iki koşum (ilk kez tek betikle, K-1484), 9 ayrı bulgu, 1 kısmen (8 şartname çifti: T04, T06 Paketleme ve yayın komutları), 8 ret (altısı tekrar eden RET); 8 doğrulayıcı; TEMİZ DEĞİL (kabul Yüksek), ⚠ yok.
- `11_CROSS_REVIEW_R7.md` — Implementation Planı (`11` v0.44 → v0.45), 7. tur, 2026-10-31: Codex iki koşum (tek betikle, K-1484), 12 ayrı bulgu, 2 kısmen (7 şartname çifti: T119, T201, T302, T366), 10 ret (yedisi tekrar eden RET); 10 doğrulayıcı; TEMİZ DEĞİL (kabul Orta), ⚠ yok.
- `11_CROSS_REVIEW_R8.md` — Implementation Planı (`11` v0.45 → v0.46), 8. tur, 2026-10-31: Codex iki koşum (tek betikle, K-1484), 11 ayrı bulgu, 3 kısmen (6 şartname çifti: T118, T224, T370, T390), 8 ret (beşi tekrar eden RET); 10 doğrulayıcı; TEMİZ DEĞİL (kabuller Orta), ⚠ yok.
- `11_CROSS_REVIEW_R9.md` — Implementation Planı (`11` v0.46 → v0.47), 9. tur, 2026-10-31: Codex iki koşum (tek betikle, K-1484), 15 ayrı bulgu, 1 doğru, 1 kısmen (2 şartname çifti: T07 KK6, T336 KK8), 13 ret (dokuzu tekrar eden RET); 14 doğrulayıcı; TEMİZ DEĞİL (kabuller Orta, 8. turla aynı sınıf: sıradaki hedefli mercek), ⚠ yok.
- `11_CROSS_REVIEW_R10.md` — Implementation Planı (`11` v0.49 → v0.50), 10. tur, 2026-11-01: Codex iki koşum (tek betikle, K-1484; ilk deneme kullanım sınırına takıldı, tur baştan koşuldu), 8 ayrı bulgu, 3 kısmen (5 şartname çifti: T103 KK6, T514 k10/KK10, T504 k4/KK4), 5 ret (üçü tekrar eden RET); 8 doğrulayıcı; TEMİZ DEĞİL (F7 Orta kabul), ayrıca T370 KK7 ek düzeltmesi; ⚠ yok.
- `11_CROSS_REVIEW_R11.md` — Implementation Planı (`11` v0.50 → v0.51), 11. tur, 2026-11-01: Codex iki koşum (tek betikle, K-1484), 14 ayrı bulgu, 1 kısmen (1 şartname çifti: T355 k4), 13 ret (on tanesi tekrar eden RET); 12 doğrulayıcı; **TEMİZ — çıkış kuralıyla, 11 tur** (tek kabul Düşük ifade farkı), ⚠ yok.
- `11_CROSS_REVIEW_R11.md` bölümü "Etki yansıtma" (2026-11-01, `11` v0.52, K-1493): cross-review Faz 5; downstream temiz, K-1483 `ExpiryDated` bölmesi, K-1491 çerçevesiz panel hata sayfası, K-1481/K-1490 notları uygulandı; iki doğrulayıcı; kalite döngüsü tamam.
