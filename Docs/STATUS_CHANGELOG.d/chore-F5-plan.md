## 2026-11-16 — chore: F5 plan düzeltmesi — T551 betiksiz yol tarayıcı gözlemi, T552 erişilebilirlik taraması

F5 kapısı tur 1 FAIL (rapor `CHECKPOINT_REPORTS/GATE_CHECK_F5.md` tur 2'de yazılır): kod sağlam (Unit 8087, Integration 4579, Release 0 uyarı), ama `12 §6.2` F5 ölçütü olan A seviyesi tarama ve betiksiz yol gözlemi için repoda araç yoktu; 27 `[erişilebilirlik]` ve 17 betiksiz yol satırı BEKLEMEDE. Yönetici yardımcısı sahip yerine karar verdi (K-1620): F5'e iki yeni task.

- **T551:** Tarayıcı aracı ve betiksiz yol gözlemi (Playwright, üç motor, iki bağlam, tohumlu veri, `VAL-B096`…`B112`).
- **T552:** A seviyesi erişilebilirlik taraması ve ihlal düzeltmesi (axe-core, elle denetim kaydı, 31 ölçüt, Kontrol Listesi eşlemesi); bağımlılık T551.
- **Belgeler:** `11` v0.66 (§3 `T501`–`T552` (52), T551 ve T552 blokları, yürütme sırası, §6.2 F5, aday tablosu, §7.1, §7.2); `12` Kaynak sütununa task atfı (27 + 17 satır, `Durum` değişmedi); K-1620; `IMPLEMENTATION_STATUS.md` F5 satırı ve iki `⬚` satır.
- **Betik:** `scripts/doc-scans/11_geri_yon.py` T551'in gerekçeli alt bölüm kaydı.
