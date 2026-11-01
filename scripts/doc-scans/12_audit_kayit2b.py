"""Asama 10 -- 12 audit-2b Parca B kaydi: surum artislari, K-1553, S24.1, rapor, README, MEMORY.

Her degisiklik (dosya, ESKI, YENI): ESKI dosyada tam bir kez gecmeli; yoksa hicbir sey yazilmaz.
Kullanim: python -I scripts/doc-scans/12_audit_kayit2b.py [--kuru]
"""
import sys
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

R = Path(r"C:\projects\Shopfolio")
DOCS = R / "Docs"
V12 = DOCS / "12_VALIDATION_PROTOCOL.md"
ST = DOCS / "PRODUCT_DISCOVERY_STATUS.md"
RP = DOCS / "AUDIT_REPORTS" / "12_AUDIT.md"
RD = DOCS / "AUDIT_REPORTS" / "README.md"
MEM = R / ".claude" / "memory" / "MEMORY.md"
kuru = "--kuru" in sys.argv

C = []


def ekle(yol, eski, yeni):
    C.append((yol, eski, yeni))


OZET = ("31 madde"  # 2a
        )
SATIR_2B = ("audit-2b işlendi (v0.18; K-1553: 46 madde doğrulandı, 39 uygulandı, 2 yükseltildi); "
            "sıradaki kalite döngüsünün audit-2c adımıdır.")

# --- 12 ---
ekle(V12, "**Versiyon: v0.17** |", "**Versiyon: v0.18** |")
ekle(V12, "*Shopfolio — Validation Protocol v0.17*", "*Shopfolio — Validation Protocol v0.18*")
ekle(V12, "28 uygulandı, 2 yükseltildi); sıradaki kalite döngüsünün audit-2b adımıdır.**",
     "28 uygulandı, 2 yükseltildi); " + SATIR_2B + "**")

# --- tracker ---
ekle(ST, "**Versiyon: v0.312** |", "**Versiyon: v0.313** |")
ekle(ST, "| 12 | Validation Protocol | ⏳ | v0.17 (audit-2a işlendi:",
     "| 12 | Validation Protocol | ⏳ | v0.18 (audit-2b işlendi: 46 madde doğrulandı (38 doğru · 3 kısmen · 5 çürük), 39 uygulandı "
     "(43 ESKİ → YENİ çifti + 45 hücre), 2 yükseltildi (2r); matris 1.473 satır, KRİTİK 611 · ORTA 730; K-1553; sıradaki audit-2c) · "
     "v0.17 (audit-2a işlendi:")

K1553 = (
    "| K-1553 | 2026-11-01 | Kalite döngüsü — audit doğrulama 2b, KD-audit-2 (`12` v0.17 → v0.18; rapor "
    "`Docs/AUDIT_REPORTS/12_AUDIT.md` \"Doğrulanmamış bulgular — KD-audit-2b\") **(öneriyle kaydedildi)** | "
    "**(1) Koşum:** 2b'de 18 Opus/high doğrulayıcı koştu (hukum_1…18); 46 madde hükme bağlandı: **38 doğru · 3 kısmen · 5 çürük** "
    "(çürük: Y3, #65, #90, Y11, Y19). Planlanan 20 doğrulayıcıdan ikisi (19: #97; 20: #98, Y10, Y12, Y2) eşzamanlı 20 sınırına takılıp "
    "yazılmadı; bu 5 madde **2c'ye** alındı. **(2) Uygulama:** 39 madde `12`'ye uygulandı (`12_audit_uygula2.py`: 43 ESKİ → YENİ çifti "
    "+ 45 hücre düzeltmesi, yeni satır yok). İki çakışma tek değişikliğe birleştirildi: hukum_1 Y8 ↔ hukum_17 #91 (aynı KP-6 atıf "
    "çifti) ve hukum_5 Y5 ↔ hukum_4 #70 (ödeme tutarı = iade tutarı cümlesi; taksit farkı sözü iki şartnamenin birleşimidir). "
    "Matris 1.473 satır; Severity KRİTİK 610 → 611 · ORTA 731 → 730 (VAL-D051 #48); Kanıt ortamı `gerçek` 37 → 38 · `sandbox` 24 → 23 "
    "(VAL-E029 #104). #51 Orta → Düşük (uygulandı). **(3) Yükselenler (uygulanmaz):** Y1 (VAL-B016; KARAR iki yol, 2r) ve #88, Düşük → Orta; "
    "KD-audit-2r'de yeniden doğrulanır. **(4) KARAR (öneriyle):** Google ile giriş hacmi (VAL-E031; `08` DV-30 / `02` §11.3) için tepe gün "
    "taban sayısı uydurulmaz; \"Google'ın kotası okununca bilgim tarihli yazılır\" kapısı korunur, eşik o zaman konur; \"Yine de sorulur\" "
    "ölçütünü karşılamaz. **(5) Üst doküman (K-652):** hiçbir kaynak yanlış bulunmadı, geri besleme yok (`08` DV-30 notu koşulludur, "
    "yukarıdaki kapıyla karşılanır). **(6) Kalan:** 28 madde (5'i 2b'den devreden: #97, #98, Y10, Y12, Y2; 2c) + 2r (Y21, #95, Y1, #88). "
    "| **Gerekçe:** proje sahibinin kararı (2026-10-31): her madde doğrulanır, örnekleme ya da atlama yoktur; yükselen madde kendi "
    "doğrulayıcısınca yükseltildiği için bu adımda uygulanmaz, hüküm 2r'de verilir. Çakışan şartnamelerde aynı hücreye iki ayrı "
    "yazım yapılmaz; birleşik metin her iki bulguyu da karşılar. **Elenen:** B016 için iki yoldan birini bu adımda seçmek — 2r'nin işi. "
    "| `12` v0.18 |\n"
)
ekle(ST, "| `05` v0.45 · `12` v0.17 |\n", "| `05` v0.45 · `12` v0.17 |\n" + K1553)

ekle(ST, "**Kalan 74 madde (2b, 2c) + 2r; sıradaki adım 2b.** Rapor: `Docs/AUDIT_REPORTS/12_AUDIT.md`. Açık soru yok; ⚠ karar yok.\n",
     "**Kalan 74 madde (2b, 2c) + 2r; sıradaki adım 2b.** Rapor: `Docs/AUDIT_REPORTS/12_AUDIT.md`. Açık soru yok; ⚠ karar yok.\n"
     "- [x] **Kalite döngüsü — audit doğrulama 2b (KD-audit-2) — tamamlandı (2026-11-01, `12` v0.18, K-1553).** 2b: **18 Opus/high doğrulayıcı, "
     "46 madde: 38 doğru · 3 kısmen · 5 çürük**; 39 madde uygulandı (43 şartname çifti + 45 hücre); matris 1.473 satır, KRİTİK 611 · ORTA 730; "
     "Y1 ve #88 (Düşük → Orta) 2r'ye kaldı; #97, #98, Y10, Y12, Y2 doğrulayıcı sınırına takıldığı için 2c'ye taşındı. **Kalan 28 madde (2c) + 2r "
     "(Y21, #95, Y1, #88); sıradaki adım 2c.** Rapor: `Docs/AUDIT_REPORTS/12_AUDIT.md`. Açık soru yok; ⚠ karar yok.\n")

# --- rapor ---
RAPOR = """## Doğrulanmamış bulgular — KD-audit-2b

**Tarih:** 2026-11-01 · **Girdi:** KD-audit-2a'dan kalan 74 madde; 2b: 46 madde hükme bağlandı (hukum_1…18).

### Doğrulayıcılar (18; hepsi `[dogrulama]` Opus / high)

| Doğrulayıcı | Kapsam | Hüküm |
|---|---|---|
| 1 | Y13, Y14, Y15, Y16, Y3, Y8 | doğru 3 · kısmen 2 (Y16, Y8) · çürük 1 (Y3) |
| 2 · 3 · 4 · 5 | #83 · Y7 · #70 · Y5 | her biri doğru 1 |
| 6 | #46, #47, #48 | doğru 3 |
| 7 | #100, Y18, Y19 | doğru 2 · çürük 1 (Y19) |
| 8 | #51, #52, #53 | doğru 2 · kısmen 1 (#51; Orta → Düşük) |
| 9 | #54, #55 | doğru 2 |
| 10 | #103, #104, #105, Y1 | doğru 4 (Y1 yükseltildi) |
| 11 | #61, #62, #63 | doğru 3 |
| 12 | #64, #65 | doğru 1 · çürük 1 (#65) |
| 13 | #109…#112, Y20 | doğru 5 |
| 14 · 15 | #25, #26, #27 · #28, #29 | doğru 3 · doğru 2 |
| 16 | #88, #89, #90 | doğru 2 (#88 yükseltildi) · çürük 1 (#90) |
| 17 · 18 | #91 · #96, Y11 | doğru 1 · doğru 1 · çürük 1 (Y11) |

Planlanan iki doğrulayıcı (19: #97; 20: #98, Y10, Y12, Y2) eşzamanlı 20 sınırı nedeniyle yazılamadı; paketleri (`dogrulama_19.md`, `dogrulama_20.md`) duruyor, 5 madde **2c'ye** taşındı. Bekçi olayı: eşzamanlı sınır hatası 19 ve 20'yi saydı, yeniden denemeler engellendi.

**Toplam hüküm: 46 madde — 38 doğru · 3 kısmen · 5 çürük.** Seviye: #51 Orta → Düşük (uygulandı); Y1 ve #88 Düşük → Orta **yükseltildi**.

### Uygulama

39 madde uygulandı: **43 ESKİ → YENİ çifti** ve **45 hücre düzeltmesi** (`12_audit_uygula2.py`; yeni satır yok). Çakışmalar: hukum_1 Y8 ↔ hukum_17 #91 (aynı KP-6 atıf çifti; tek değişiklik) ve hukum_5 Y5 ↔ hukum_4 #70 (ödeme tutarı = iade tutarı cümlesi; iki şartname tek birleşik metinde). Uygulanan liste: `a10-kd-audit-2/2b/uygulanan.md`.

**Matris sayımı (betik):** 1.473 satır (değişmedi); Severity KRİTİK 610 → **611** · ORTA 731 → **730** · DÜŞÜK 132 (VAL-D051, #48); Kanıt ortamı `gerçek` 37 → **38** · `sandbox` 24 → **23** (VAL-E029, #104).

### Üst dokümana geri besleme (K-652)

Yok. `08` DV-30 için hükmün üst doküman notu koşulludur (tepe gün giriş sayısı kaynaktan konunca VAL-E031 yazılır); öneriyle kapı korunur (K-1553).

### Yükseltilenler (uygulanmadı → KD-audit-2r)

Y1 (VAL-B016: güvenlik kuralını tek başına sınar ama ORTA; iki yoldan biri) ve #88 (açık iletişim talebi ile durumu hiç değişmemiş ayıp talebinin "açılıştan" kolunu hiçbir satır sınamaz). Dosya: `a10-kd-audit-2/yeniden.md`.

### Kalan

105 maddenin 77'si hükme bağlandı (2a 31 · 2b 46); **kalan 28 madde** (2c; #97, #98, Y10, Y12, Y2 dahil) ve 2r (Y21, #95, Y1, #88). Kapılar: `12_kapsam_hizasi.py` 0 · `12_matris_denetle.py --kapsam 1-77` hata 0 (1.473 satır, 77/77 KP) · `12_atif_denetle.py` hata 0.

"""
ekle(RP, "Prosedür: [`.claude/skills/audit/SKILL.md`]",
     RAPOR + "Prosedür: [`.claude/skills/audit/SKILL.md`]")

ekle(RD, "kalan 74 madde.\n",
     "kalan 74 madde. KD-audit-2b: `12` v0.18, 46 madde doğrulandı (38 doğru · 3 kısmen · 5 çürük), 39 uygulandı, 2 yükseltildi; kalan 28 madde.\n")

# --- MEMORY ---
ekle(MEM, "- **SIRADA — Aşama 10 kalite döngüsü, audit-2b (`12` v0.17, karar kaydı v0.312, K-1551, K-1552; dal `docs/asama10-kd-audit-2a`):",
     "- **SIRADA — Aşama 10 kalite döngüsü, audit-2c (`12` v0.18, karar kaydı v0.313, K-1553; dal `docs/asama10-kd-audit-2b`): audit-2b tamam — "
     "18 Opus/high doğrulayıcı, 46 madde 38 doğru · 3 kısmen · 5 çürük, 39 uygulandı (43 çift + 45 hücre; matris 1.473 satır, KRİTİK 611 · ORTA 730), "
     "Y1 ve #88 2r'ye kaldı, #97/#98/Y10/Y12/Y2 doğrulayıcı sınırı nedeniyle 2c'ye taşındı; kalan 28 madde (2c) + 2r; ⚠ yok. "
     "Önceki: audit-2a (`12` v0.17, karar kaydı v0.312, K-1551, K-1552; dal `docs/asama10-kd-audit-2a`):")

for yol, e, y in C:
    t = yol.read_bytes().decode("utf-8")
    n = t.replace("\r\n", "\n").count(e)
    if n != 1:
        print("SORUN %s: %d kez: %.70s" % (yol.name, n, e))
        sys.exit(1)
if kuru:
    print("kuru: tamam")
    sys.exit(0)
for yol, e, y in C:
    raw = yol.read_bytes().decode("utf-8")
    crlf = "\r\n" in raw
    t = raw.replace("\r\n", "\n").replace(e, y, 1)
    if crlf:
        t = t.replace("\n", "\r\n")
    yol.write_bytes(t.encode("utf-8"))
print("yazildi", len(C))
