"""Asama 9 -- 11 kalite dongusu audit'i, Parca B: hukum_*.md'deki ESKI -> YENI sartname ciftlerini 11'e uygular.

Girdi: <dizin>/hukum_<k>.md. Yalniz `SARTNAME n` bloklari (DOSYA/SATIR/ESKI/YENI); `SARTNAME YOK` ve `UST DOKUMAN` satirlari girmez.
Once hepsi sinanir (her ESKI dosyada tam bir kez, ESKI iki ciftte yok, ESKI != YENI, cakisan aralik yok); sorun varsa
hicbir sey yazilmaz. Sonra uygulanir; her YENI dosyada artik ESKI'yi icermez. Bir uygulanan cift listesi
`<dizin>/uygulanan.md`'ye yazilir (bulgu, satir, ESKI ilk 60 karakter).
Kullanim: python -I scripts/doc-scans/11_audit_uygula.py <dizin> [--doc yol] [--kuru]
"""
import re
import sys
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

args = sys.argv[1:]
kuru = "--kuru" in args
if kuru:
    args.remove("--kuru")
DOC = Path(r"C:\projects\Shopfolio\Docs\11_IMPLEMENTATION_PLAN.md")
if "--doc" in args:
    i = args.index("--doc")
    DOC = Path(args[i + 1])
    del args[i:i + 2]
D = Path(args[0])
raw = DOC.read_bytes().decode("utf-8")
crlf = "\r\n" in raw
TXT = raw.replace("\r\n", "\n")

hdr = re.compile(r"^## #(\d+) \| hukum: (\w+)", re.I)
ciftler = []
for f in sorted(D.glob("hukum_*.md"), key=lambda p: int(re.search(r"(\d+)", p.stem).group(1))):
    cur = None
    eski = None
    for l in f.read_text(encoding="utf-8").split("\n"):
        m = hdr.match(l)
        if m:
            cur = (int(m.group(1)), m.group(2).lower())
            continue
        if cur is None or cur[1] not in ("dogru", "kismen"):
            continue
        if l.startswith("ESKI:"):
            eski = l.split(":", 1)[1].strip()
        elif l.startswith("YENI:") and eski is not None:
            ciftler.append((cur[0], f.name, eski, l.split(":", 1)[1].strip()))
            eski = None

sorun = []
aralik = []
gorulen = set()
for no, fn, eski, yeni in ciftler:
    n = TXT.count(eski)
    if n != 1:
        sorun.append("#%d %s: ESKI %d kez: %.80s" % (no, fn, n, eski))
        continue
    if eski == yeni:
        sorun.append("#%d %s: YENI == ESKI" % (no, fn))
    if eski in gorulen:
        sorun.append("#%d %s: ayni ESKI iki ciftte" % (no, fn))
    gorulen.add(eski)
    p = TXT.index(eski)
    aralik.append((p, p + len(eski), no, fn))
aralik.sort()
for a, b in zip(aralik, aralik[1:]):
    if b[0] < a[1]:
        sorun.append("cakisan aralik: #%d ve #%d" % (a[2], b[2]))
print("cift: %d; sorun: %d" % (len(ciftler), len(sorun)))
for s in sorun:
    print(" -", s)
if sorun or kuru:
    sys.exit(1 if sorun else 0)

out = TXT
log = ["# Uygulanan sartname ciftleri", "", "| Bulgu | Dosya | ESKI (ilk 60) |", "|---|---|---|"]
for no, fn, eski, yeni in ciftler:
    out = out.replace(eski, yeni, 1)
    log.append("| #%d | %s | %s |" % (no, fn, eski[:60].replace("|", "/")))
if crlf:
    out = out.replace("\n", "\r\n")
DOC.write_bytes(out.encode("utf-8"))
(D / "uygulanan.md").write_text("\n".join(log) + "\n", encoding="utf-8", newline="\n")
print("uygulandi: %d cift" % len(ciftler))
