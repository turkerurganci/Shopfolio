# Audit Raporları

`/audit XX` çıktıları: `XX_AUDIT.md` (tekrar turları `XX_AUDIT_R2.md`).

**Denetlenebilirlik kuralı:** Bir audit raporu, envanter toplamı ile eşleştirme sayısı **eşleşmiyorsa** geçersizdir — öğe atlanmış demektir. Rapor bu iki sayıyı açıkça gösterir.

Prosedür: [`.claude/skills/audit/SKILL.md`](../../.claude/skills/audit/SKILL.md)

- `09_AUDIT.md` — Kodlama Kılavuzu (`09` v0.16 → v0.17), 2026-10-30: 16 mercek, 16 doğrulayıcı, 48 bulgu uygulandı, 1 çürütüldü.
- `09_DEEP_REVIEW.md` — Kodlama Kılavuzu (`09` v0.17 → v0.18), 2026-10-30: 12 mercek, 25 doğrulayıcı (20 + 5 yeniden doğrulama), 81 bulgu uygulandı, 24 çürütüldü.
- `11_AUDIT.md` — Implementation Plan (`11` v0.25 → v0.26), 2026-10-31: 8 mercek (67 ajan), 20 doğrulayıcı, 485 ham bulgu, 44 hükme bağlandı, 41 uygulandı (101 şartname çifti), 2 çürütüldü, 435 doğrulanmadı (sonraki adımın girdisi).
- `11_DEEP_REVIEW.md` — Implementation Plan (`11` v0.33 → v0.34), 2026-10-31: 21 mercek, 20 doğrulayıcı, 191 ham bulgu, 48 hükme bağlandı (12 doğru · 22 kısmen · 14 çürük), 34 bulgu uygulandı (82 şartname çifti), 143 doğrulanmadı (KD-deep-2a, 2b'nin girdisi); `02` v0.106, `09` v0.26 geri besleme.
