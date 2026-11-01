"""Asama 10 -- 12 audit-2c Parca B kaydi: surum artislari, K-1554, S24.1, rapor, README, MEMORY, ust dokuman geri besleme.

Her degisiklik (dosya, ESKI, YENI): ESKI dosyada tam bir kez gecmeli; yoksa hicbir sey yazilmaz.
Kullanim: python -I scripts/doc-scans/12_audit_kayit2c.py [--kuru]
"""
import sys
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

R = Path(r"C:\projects\Shopfolio")
DOCS = R / "Docs"
V12 = DOCS / "12_VALIDATION_PROTOCOL.md"
V05 = DOCS / "05_TECHNICAL_ARCHITECTURE.md"
V10 = DOCS / "10_MVP_SCOPE.md"
ST = DOCS / "PRODUCT_DISCOVERY_STATUS.md"
RP = DOCS / "AUDIT_REPORTS" / "12_AUDIT.md"
RD = DOCS / "AUDIT_REPORTS" / "README.md"
MEM = R / ".claude" / "memory" / "MEMORY.md"
kuru = "--kuru" in sys.argv

C = []


def ekle(yol, eski, yeni):
    C.append((yol, eski, yeni))


GB05 = ("v0.46 Aşama 10 — Doğrulama Protokolü kalite döngüsünün audit-2c doğrulamasının geri beslemesi: K-1554 — §10.1.8 "
        "(yükleme diski tam eşiğe — %85 — getirirse de kabulde reddedilir; durum ucunun \"%85'e ulaşınca\" ölçütüyle aynı, §9.4.3); "
        "✓ korunur, kalite döngüsü yeniden açılmaz · ")
GB10 = ("v0.61 Aşama 10 — Doğrulama Protokolü kalite döngüsünün audit-2c doğrulamasının geri beslemesi: K-1554 — KD-59 "
        "(sepet kalem sayısı, ekleme hızı ve stok yoklamada ürün kuralıyla limit yoktur; ürün başına \"bir siparişte en fazla\" adet P-9 ayrıdır; "
        "`02 §8.2.5`, `02 §3.16.9`); ✓ korunur, kalite döngüsü yeniden açılmaz · ")

# --- 05, 10 ---
ekle(V05, "**Versiyon: v0.45** |", "**Versiyon: v0.46** |")
ekle(V05, "*Shopfolio — Technical Architecture v0.45 — ✓ Tamamlandı (",
     "*Shopfolio — Technical Architecture v0.46 — ✓ Tamamlandı (" + GB05)
ekle(V10, "**Versiyon: v0.60** |", "**Versiyon: v0.61** |")
ekle(V10, "*Shopfolio — MVP Scope v0.60 (", "*Shopfolio — MVP Scope v0.61 (" + GB10)

# --- 12 ---
ekle(V12, "**Versiyon: v0.18** |", "**Versiyon: v0.19** |")
ekle(V12, "*Shopfolio — Validation Protocol v0.18*", "*Shopfolio — Validation Protocol v0.19*")
ekle(V12, "sıradaki kalite döngüsünün audit-2c adımıdır.**",
     "audit-2c işlendi (v0.19; K-1554: 28 madde doğrulandı, 24 uygulandı, 0 yükseltildi); sıradaki kalite döngüsünün audit-2r adımıdır.**")

# --- tracker ---
ekle(ST, "**Versiyon: v0.313** |", "**Versiyon: v0.314** |")
ekle(ST, "| 12 | Validation Protocol | ⏳ | v0.18 (audit-2b işlendi:",
     "| 12 | Validation Protocol | ⏳ | v0.19 (audit-2c işlendi: 28 madde doğrulandı (13 doğru · 11 kısmen · 4 çürük), 24 uygulandı "
     "(19 ESKİ → YENİ çifti + 13 hücre), yükseltilen yok; #56 ve #87 Orta → Düşük; matris 1.473 satır, KRİTİK 611 · ORTA 730 · DÜŞÜK 132; "
     "K-1554; 105 maddenin tamamı hükme bağlandı; sıradaki audit-2r) · v0.18 (audit-2b işlendi:")

K1554 = (
    "| K-1554 | 2026-11-01 | Kalite döngüsü — audit doğrulama 2c, KD-audit-2'nin son doğrulama adımı (`12` v0.18 → v0.19; `05` v0.45 → v0.46; "
    "`10` v0.60 → v0.61; `.claude/skills/validate/SKILL.md`; rapor `Docs/AUDIT_REPORTS/12_AUDIT.md` \"Doğrulanmamış bulgular — KD-audit-2c\") "
    "**(öneriyle kaydedildi)** | "
    "**(1) Koşum:** 2c'de 18 Opus/high doğrulayıcı koştu (hukum_1…18; iki dalga: 10 + 8); 28 madde hükme bağlandı: **13 doğru · 11 kısmen · 4 çürük** "
    "(çürük: Y17, Y4, #114, Y12). **105 maddenin tamamı hükme bağlandı** (2a 31 · 2b 46 · 2c 28). Doğrulayıcı toplamı adım başına 20'nin altında. "
    "**(2) Uygulama:** 24 madde `12`'ye uygulandı (`12_audit_uygula2.py`: 19 ESKİ → YENİ çifti + 13 hücre düzeltmesi, yeni satır yok). "
    "Seviye değişimi: #56 ve #87 Orta → Düşük (uygulandı); matris severity sayımı değişmedi (KRİTİK 611 · ORTA 730 · DÜŞÜK 132). "
    "**(3) Yükselen:** yok. **(4) Üst doküman (K-652):** `10` §3 KD-59, `02 §8.2.5` ve P-9'a hizalandı; `05` §10.1.8 yükleme tam %85'e ulaşırsa da "
    "reddedilir (durum ucunun \"%85'e ulaşınca\" ölçütüyle aynı); `.claude/skills/validate/SKILL.md` adım 11 (PASS: `~ Kısmi` PASS sayılmaz, `12 §6.1`, K-1521) "
    "ve adım 12 (S1/S2/S3 tanımı `00 §D.10` ve `12 §8.1`'e hizalı; L4 değişikliği, `12` dışı); `PRODUCT_DISCOVERY_STATUS.md` §24.1'e K-1538 (4)'ün andığı açık nokta eklendi. "
    "**(5) Kalan:** 2r (Y21, #95, Y1, #88 + 2a'nın yeni YAN notları). "
    "| **Gerekçe:** proje sahibinin kararı (2026-10-31): her madde doğrulanır, örnekleme ya da atlama yoktur. Kaynak ile `12` çeliştiğinde L1/L2 kazanır, L4 skill hizalanır; "
    "skill değişikliği L4 olduğu için karar satırına yazıldı. ⚠ karar yok. | `05` v0.46 · `10` v0.61 · `12` v0.19 |\n"
)
ekle(ST, "| `12` v0.18 |\n", "| `12` v0.18 |\n" + K1554)

ekle(ST, "**Kalan 28 madde (2c) + 2r (Y21, #95, Y1, #88); sıradaki adım 2c.** Rapor: `Docs/AUDIT_REPORTS/12_AUDIT.md`. Açık soru yok; ⚠ karar yok.\n",
     "**Kalan 28 madde (2c) + 2r (Y21, #95, Y1, #88); sıradaki adım 2c.** Rapor: `Docs/AUDIT_REPORTS/12_AUDIT.md`. Açık soru yok; ⚠ karar yok.\n"
     "- [x] **Kalite döngüsü — audit doğrulama 2c (KD-audit-2) — tamamlandı (2026-11-01, `12` v0.19, K-1554).** 2c: **18 Opus/high doğrulayıcı, "
     "28 madde: 13 doğru · 11 kısmen · 4 çürük**; 24 madde uygulandı (19 şartname çifti + 13 hücre); #56 ve #87 Orta → Düşük; yükselen yok. "
     "**105 maddenin tamamı hükme bağlandı.** Kalan: 2r (Y21, #95, Y1, #88 + 2a'nın yeni YAN notları); sıradaki adım 2r. "
     "Rapor: `Docs/AUDIT_REPORTS/12_AUDIT.md`. Açık soru yok; ⚠ karar yok.\n"
     "- [ ] **Açık nokta (K-1538 (4)):** Erişilebilirlik Kontrol Listesi A Seviyesi madde numaraları okunmadı; eşleme F5 tarama kaydında yapılır (K-1538).\n")

# --- rapor ---
RAPOR = """## Doğrulanmamış bulgular — KD-audit-2c

**Tarih:** 2026-11-01 · **Girdi:** KD-audit-2b'den kalan 28 madde (2b'den devreden #97, #98, Y10, Y12, Y2 dahil); 2c: 28 madde hükme bağlandı (hukum_1…18).

### Doğrulayıcılar (18; hepsi `[dogrulama]` Opus / high; iki dalga: 10 + 8)

Kademe: Orta 8 madde (#50, #56, #66, #67, #68, #69, #71, #87), Düşük 20 madde (14 bulgu + Y17, Y4, Y23, Y10, Y12, Y2). Paket 1–16 `--paket 2c`; 17 = #97, 18 = #98, Y10, Y12, Y2 (2b'den devreden).

**Toplam hüküm: 28 madde — 13 doğru · 11 kısmen · 4 çürük** (çürük: Y17, Y4, #114, Y12). Seviye: #56 ve #87 Orta → Düşük (**düşürüldü, uygulandı**); yükselen yok.

### Uygulama

24 madde uygulandı: **19 ESKİ → YENİ çifti** ve **13 hücre düzeltmesi** (`12_audit_uygula2.py`; yeni satır yok). Y23'ün aynı satırdaki çoklu SATIR'ı (VAL-A944, A826, A834) ve VAL-A195'in 4 SATIR'ı betikçe okundu; VAL-A013/A015 düzeltmesi Y10'da tek kez. Uygulanan liste: `a10-kd-audit-2/2c/uygulanan.md`.

**Matris sayımı (betik):** 1.473 satır (değişmedi); Severity KRİTİK 611 · ORTA 730 · DÜŞÜK 132 (değişmedi).

### Üst dokümana geri besleme (K-652)

`10` v0.61 (KD-59 ifadesi `02 §8.2.5` ve P-9'a hizalandı), `05` v0.46 (§10.1.8: yükleme diski tam %85'e getirirse de reddedilir). L4: `.claude/skills/validate/SKILL.md` adım 11 ve 12, `12` ve `00 §D.10` ile hizalandı. `PRODUCT_DISCOVERY_STATUS.md` §24.1'e K-1538 (4) açık noktası eklendi (K-1554).

### Yükseltilenler

Yok.

### Toplam ilerleme

**105 maddenin tamamı hükme bağlandı** (2a 31 · 2b 46 · 2c 28). Kalan: 2r — Y21, #95, Y1, #88 ve 2a'nın yeni YAN notları. Kapılar: `12_kapsam_hizasi.py` 0 · `12_matris_denetle.py --kapsam 1-77` hata 0 (1.473 satır, 77/77 KP) · `12_atif_denetle.py` hata 0.

"""
ekle(RP, "Prosedür: [`.claude/skills/audit/SKILL.md`]",
     RAPOR + "Prosedür: [`.claude/skills/audit/SKILL.md`]")

ekle(RD, "2 yükseltildi; kalan 28 madde.\n",
     "2 yükseltildi; kalan 28 madde. KD-audit-2c: `12` v0.19, 28 madde doğrulandı (13 doğru · 11 kısmen · 4 çürük), 24 uygulandı, yükselen yok; "
     "105 maddenin tamamı hükme bağlandı; `10` v0.61, `05` v0.46 geri besleme; sıradaki 2r.\n")

# --- MEMORY ---
ekle(MEM, "- **SIRADA — Aşama 10 kalite döngüsü, audit-2c (`12` v0.18, karar kaydı v0.313, K-1553; dal `docs/asama10-kd-audit-2b`):",
     "- **SIRADA — Aşama 10 kalite döngüsü, audit-2r (`12` v0.19, karar kaydı v0.314, K-1554; dal `docs/asama10-kd-audit-2c`): audit-2c tamam — "
     "18 Opus/high doğrulayıcı, 28 madde 13 doğru · 11 kısmen · 4 çürük, 24 uygulandı; 105 maddenin tamamı hükme bağlandı; #56/#87 Orta → Düşük; "
     "`10` v0.61, `05` v0.46 ve validate skill'i hizalandı; kalan 2r (Y21, #95, Y1, #88 + 2a'nın yeni YAN notları); ⚠ yok. "
     "Önceki: audit-2c öncesi (`12` v0.18, karar kaydı v0.313, K-1553; dal `docs/asama10-kd-audit-2b`):")

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
