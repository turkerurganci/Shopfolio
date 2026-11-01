"""Asama 10 -- 12 audit Parca B: tekil olmayan hucreler icin VAL kimligi + sutun hedefli duzeltmeler.

Her duzeltme (VAL kimligi, sutun, ESKI hucre, YENI hucre): satir `| VAL-<id> | ` ile baslayan tek satir olmali; sutun
(0=kimlik, 1=seviye, 2=Kural, 3=Kaynak, 4=On kosul, 5=Beklenen) hucresi ESKI'ye tam esit olmali. Hepsi sinanir, sonra yazilir.
Kullanim: python -I scripts/doc-scans/12_audit_satir_uygula.py [--kuru]
"""
import sys
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

DOC = Path(r"C:\projects\Shopfolio\Docs\12_VALIDATION_PROTOCOL.md")
KAYNAK, ONKOSUL, BEKLENEN = 3, 4, 5

# (bulgu, VAL, sutun, ESKI, YENI)
D = [
    (6, "A166", KAYNAK, "10 KP-39 · 02 §3.6", "10 KP-39 · 02 §3.8.2"),
    (35, "A165", KAYNAK, "10 KP-39 · 02 §3.6", "10 KP-39 · 02 §3.8.4"),
    (35, "A169", KAYNAK, "10 KP-39 · 02 §3.6.7", "10 KP-39 · 02 §3.8.5"),
    (35, "A170", KAYNAK, "10 KP-39 · 02 §3.6.7", "10 KP-39 · 02 §3.8.5"),
    (35, "A172", KAYNAK, "10 KP-39 · 02 §3.8, §10.1.2", "10 KP-39 · 02 §3.6.7"),
    (35, "A173", KAYNAK, "10 KP-39 · 02 §3.8, §10.1.2", "10 KP-39 · 02 §3.6.7"),
    (35, "A175", KAYNAK, "10 KP-39 · 02 §3.2.2", "10 KP-39 · 02 §10.7.2"),
    (30, "A045", KAYNAK, "10 KP-13 · 02 §3.20", "10 KP-13 · 02 §3.18"),
    (36, "D023", KAYNAK, "10 KP-29 · 02 §3.15.6", "10 KP-29 · 02 §3.15.1, §3.15.2"),
    (36, "A120", KAYNAK, "10 KP-33 · 02 §3.27.7", "10 KP-33 · 02 §3.27.8"),
    (39, "A218", KAYNAK, "10 KP-47 · 02 §7.4.9", "10 KP-47 · 02 §7.4.7, §7.5.3"),
    (39, "A219", KAYNAK, "10 KP-47 · 02 §7.4.9", "10 KP-47 · 02 §7.4.7"),
    (39, "A220", KAYNAK, "10 KP-47 · 02 §7.4.9", "10 KP-47 · 02 §7.4.7"),
    (39, "A221", KAYNAK, "10 KP-47 · 02 §7.4.9", "10 KP-47 · 02 §5.5, §7.4.5"),
    (39, "A222", KAYNAK, "10 KP-47 · 02 §7.4.9", "10 KP-47 · 02 §7.4.5"),
    (39, "A223", KAYNAK, "10 KP-47 · 02 §7.4.9", "10 KP-47 · 02 §5.5"),
    (39, "A225", KAYNAK, "10 KP-47 · 02 §7.4.9", "10 KP-47 · 02 §7.4.10"),
    (39, "A226", KAYNAK, "10 KP-47 · 02 §7.4.9", "10 KP-47 · 02 §7.4.10"),
    (39, "A229", KAYNAK, "10 KP-47 · 02 §7.4.10", "10 KP-47 · 02 §7.2.3, §12.5"),
    (39, "A230", KAYNAK, "10 KP-47 · 02 §7.2", "10 KP-47 · 02 §7.4.5"),
    (39, "A232", KAYNAK, "10 KP-47 · 02 §7.4.9", "10 KP-47 · 02 §10.4.10"),
    (39, "A233", KAYNAK, "10 KP-47 · 02 §7.4.9", "10 KP-47 · 02 §10.4.10"),
    (39, "A234", KAYNAK, "10 KP-47 · 02 §7.4.9", "10 KP-47 · 02 §10.4.10"),
    (39, "A235", KAYNAK, "10 KP-47 · 02 §7.4.9", "10 KP-47 · 02 §10.4.10"),
    (39, "A236", KAYNAK, "10 KP-47 · 02 §7.4.9", "10 KP-47 · 02 §10.4.10"),
    (39, "A237", KAYNAK, "10 KP-47 · 02 §7.4.9", "10 KP-47 · 02 §7.4.1, §10.4.10"),
    (39, "A238", KAYNAK, "10 KP-47 · 02 §7.4.9", "10 KP-47 · 02 §7.4.5, §10.4.10"),
    (39, "C054", KAYNAK, "10 KP-47 · 02 §7.4.9, §10.4", "10 KP-47 · 02 §7.3.7, §10.4.10"),
    (39, "C060", KAYNAK, "10 KP-48 · 02 §7.4.9, §10.4.9", "10 KP-48 · 02 §7.4.4, §10.4.9"),
    (39, "A242", KAYNAK, "10 KP-48 · 02 §7.4.9, §10.4.9", "10 KP-48 · 02 §10.4.9"),
    (40, "A248", KAYNAK, "10 KP-48 · 02 §10.4, §7.2", "10 KP-48 · 02 §10.4.3, §5.5"),
    (40, "A249", KAYNAK, "10 KP-48 · 02 §10.4, §7.2", "10 KP-48 · 02 §5.5, §7.5.3, §10.4.9"),
    (40, "A250", KAYNAK, "10 KP-48 · 02 §10.4, §7.2", "10 KP-48 · 02 §5.5"),
    (42, "A269", ONKOSUL, "Panel oturumu; kayıtlı üyeler", "Panel oturumu; biri hiç sipariş vermemiş kayıtlı üyeler"),
    (43, "A281", KAYNAK, "10 KP-53 · 02 §10.6.2", "10 KP-53 · 02 §10.6.2, §10.6.3"),
    (44, "C073", KAYNAK, "10 KP-62 · 02 §10.2.5, §9.3.4", "10 KP-62 · 02 §3.13.14, §10.2.4"),
    (44, "C074", KAYNAK, "10 KP-62 · 02 §10.2.5, §9.3.4", "10 KP-62 · 02 §3.13.14, §10.2.4"),
    (44, "A309", KAYNAK, "10 KP-62 · 02 §10.2.5", "10 KP-62 · 02 §10.2.4"),
    (45, "A329", KAYNAK, "10 KP-67 · 02 §9.3.4", "10 KP-67 · 02 §9.3.3"),
    (49, "C077", BEKLENEN,
     "Önce tamamlanan uygulanır; yönetici işlemi sonra kalmışsa uygulanmaz ve güncel hâl gösterilir",
     "Önce tamamlanan uygulanır; yönetici işlemi sonra kalmışsa ve sipariş değişmişse uygulanmaz ve güncel hâl gösterilir; müşteri işlemi sonra kalmışsa güncel hâle göre ya uygulanır ya reddedilir ve sebebi söylenir"),
]

kuru = "--kuru" in sys.argv
raw = DOC.read_bytes().decode("utf-8")
crlf = "\r\n" in raw
lines = raw.replace("\r\n", "\n").split("\n")
sorun = []
yaz = []
for no, vid, col, eski, yeni in D:
    idx = [i for i, l in enumerate(lines) if l.startswith("| VAL-%s | " % vid)]
    if len(idx) != 1:
        sorun.append("#%d VAL-%s: %d satir" % (no, vid, len(idx)))
        continue
    cells = lines[idx[0]].split(" | ")
    if cells[col] != eski:
        sorun.append("#%d VAL-%s: sutun %d = %r" % (no, vid, col, cells[col][:100]))
        continue
    yaz.append((idx[0], col, yeni, no, vid, eski))
print("duzeltme: %d; sorun: %d" % (len(D), len(sorun)))
for s in sorun:
    print(" -", s)
if sorun:
    sys.exit(1)
# C078 Beklenen: C077'nin Beklenen'inden turetilir (sistem islemi)
i78 = [i for i, l in enumerate(lines) if l.startswith("| VAL-C078 | ")]
c78 = lines[i78[0]].split(" | ")
e78 = "Önce tamamlanan uygulanır; yönetici işlemi sonra kalmışsa uygulanmaz ve güncel hâl gösterilir"
if c78[BEKLENEN] != e78:
    print("C078 Beklenen beklenmedik:", c78[BEKLENEN][:120])
    sys.exit(1)
yaz.append((i78[0], BEKLENEN,
            "Önce tamamlanan uygulanır; yönetici işlemi sonra kalmışsa ve sipariş değişmişse uygulanmaz ve güncel hâl gösterilir; sistem işlemi sonra kalmışsa güncel hâle göre ya uygulanır ya reddedilir ve sebebi söylenir",
            49, "C078", e78))
if kuru:
    for i, col, yeni, no, vid, eski in yaz:
        print("#%d %s: %s -> %s" % (no, vid, eski[:60], yeni[:70]))
    sys.exit(0)
for i, col, yeni, no, vid, eski in yaz:
    cells = lines[i].split(" | ")
    cells[col] = yeni
    lines[i] = " | ".join(cells)
out = "\n".join(lines)
if crlf:
    out = out.replace("\n", "\r\n")
DOC.write_bytes(out.encode("utf-8"))
print("uygulandi: %d hucre" % len(yaz))
