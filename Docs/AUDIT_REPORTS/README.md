# Audit Raporları

`/audit XX` çıktıları: `XX_AUDIT.md` (tekrar turları `XX_AUDIT_R2.md`).

**Denetlenebilirlik kuralı:** Bir audit raporu, envanter toplamı ile eşleştirme sayısı **eşleşmiyorsa** geçersizdir — öğe atlanmış demektir. Rapor bu iki sayıyı açıkça gösterir.

Prosedür: [`.claude/skills/audit/SKILL.md`](../../.claude/skills/audit/SKILL.md)

- `09_AUDIT.md` — Kodlama Kılavuzu (`09` v0.16 → v0.17), 2026-10-30: 16 mercek, 16 doğrulayıcı, 48 bulgu uygulandı, 1 çürütüldü.
- `09_DEEP_REVIEW.md` — Kodlama Kılavuzu (`09` v0.17 → v0.18), 2026-10-30: 12 mercek, 25 doğrulayıcı (20 + 5 yeniden doğrulama), 81 bulgu uygulandı, 24 çürütüldü.
- `11_AUDIT.md` — Implementation Plan (`11` v0.25 → v0.26), 2026-10-31: 8 mercek (67 ajan), 20 doğrulayıcı, 485 ham bulgu, 44 hükme bağlandı, 41 uygulandı (101 şartname çifti), 2 çürütüldü, 435 doğrulanmadı (sonraki adımın girdisi).
- `11_DEEP_REVIEW.md` — Implementation Plan (`11` v0.33 → v0.34), 2026-10-31: 21 mercek, 20 doğrulayıcı, 191 ham bulgu, 48 hükme bağlandı (12 doğru · 22 kısmen · 14 çürük), 34 bulgu uygulandı (82 şartname çifti), 143 doğrulanmadı (KD-deep-2a, 2b'nin girdisi); `02` v0.106, `09` v0.26 geri besleme. KD-deep-2a (`11` v0.34 → v0.35): 19 doğrulayıcı, 65 bulgu hükme bağlandı (9 doğru · 27 kısmen · 29 çürük), 35 bulgu / 66 çift uygulandı, #117 yeniden doğrulamaya; `02` v0.107, `03` v0.45, `04` v0.48, `07` v0.36 geri besleme. KD-deep-2b (`11` v0.35 → v0.36): 20 doğrulayıcı, 78 bulgu hükme bağlandı (10 doğru · 41 kısmen · 27 çürük), 50 bulgu / 95 çift uygulandı, 143/143, #167 yeniden doğrulamaya; `04` v0.49, `05` v0.42, `08` v0.19, `09` v0.27 geri besleme. KD-deep-2r (`11` v0.36 → v0.37): 2 doğrulayıcı, #117 ve #167 doğru (Orta), 2 bulgu / 8 çift uygulandı, deep review tamam; `08` v0.20 geri besleme.
