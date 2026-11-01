"""Asama 10 -- `12` audit'i: hukum dosyalarini denetler (KD-audit, Parca A sonu).

Her `hukum_<n>.md` icin: hukum dagilimi, seviye degisimi, ESKI/YENI ciftleri; her ESKI `12` dosyasinda TAM BIR KEZ gecmeli;
iki cift ayni yeri degistirmemeli (ESKI parcalari ortusmemeli). Cikti: ozet satirlari + sorunlar.

Kullanim: python -I scripts/doc-scans/12_audit_hukum_denetle.py <dizin> [--doc yol]
"""
import re
import sys
from collections import Counter
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

args = [a for a in sys.argv[1:]]
DOC = Path(r"C:\projects\Shopfolio\Docs\12_VALIDATION_PROTOCOL.md")
if "--doc" in args:
    i = args.index("--doc")
    DOC = Path(args[i + 1])
    del args[i:i + 2]
D = Path(args[0])
text = DOC.read_text(encoding="utf-8")
HEAD = re.compile(r"^## (#\d+|Y\d+) \| hukum: (dogru|kismen|curuk)\s*$")
tot = Counter()
sorun = []
pairs = []   # (dosya, bulgu, eski, yeni)
yuks = []
satir = [0]
for p in sorted(D.glob("hukum_*.md"), key=lambda x: int(re.search(r"(\d+)", x.stem).group(1))):
    cur = None
    eski = None
    for l in p.read_text(encoding="utf-8").split("\n"):
        m = HEAD.match(l)
        if m:
            cur = m.group(1)
            tot[m.group(2)] += 1
            eski = None
            continue
        if l.startswith("## "):
            sorun.append("%s: bicimsiz baslik: %s" % (p.name, l[:80]))
            cur = None
            continue
        if cur is None:
            continue
        if l.startswith("Seviye:") and "YUKSELTILDI" in l:
            yuks.append((p.name, cur, l))
        if l.startswith("SATIR:") or l.startswith("YENI_SATIR:"):
            satir[0] += 1
        if l.startswith("ESKI:"):
            eski = l[5:].strip()
        elif l.startswith("YENI:") and eski is not None:
            pairs.append((p.name, cur, eski, l[5:].strip()))
            eski = None
cnt = 0
spans = []
for f, n, e, y in pairs:
    c = text.count(e) if e else 0
    if c != 1:
        sorun.append("%s %s: ESKI %d kez geciyor: %s" % (f, n, c, e[:90]))
    else:
        cnt += 1
        a = text.index(e)
        spans.append((a, a + len(e), f, n))
spans.sort()
for k in range(len(spans) - 1):
    if spans[k][1] > spans[k + 1][0]:
        sorun.append("ESKI ortusmesi: %s %s <-> %s %s" % (spans[k][2], spans[k][3], spans[k + 1][2], spans[k + 1][3]))
print("hukum: %s; cift: %d (uygulanabilir %d); yukseltilen: %d" % (dict(tot), len(pairs), cnt, len(yuks)))
for y in yuks:
    print("YUKSELTILDI", y)
print("satir kimlikli SATIR/YENI_SATIR: %d" % satir[0])
print("sorun: %d" % len(sorun))
for s in sorun:
    print("  " + s)
