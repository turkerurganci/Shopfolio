"""Asama 10 -- `12` audit'i (KD-audit, Parca A): mercek bulgu dosyalarini birlestirir, yinelenenleri eler.

Girdi : <dizin>/bulgu_*.md  (blok: `## <MERCEK>-<n> | seviye: Yuksek|Orta|Dusuk`, ardindan Yer/Iddia/Kanit/Oneri)
Cikti : <dizin>/birlesik.md (numarali #N bulgular) ve <dizin>/birlesik_ozet.tsv (N, seviye, bolum, VAL kimlikleri, kaynak mercekler, iddia)
Yinelenen: ayni VAL kimliginden ortak en az biri ve iddia sozcuk benzerligi >= 0.45 (ya da kimlik kumeleri esit).

Kullanim: python -I scripts/doc-scans/12_audit_birlestir.py <dizin>
"""
import re
import sys
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

D = Path(sys.argv[1])
HEAD = re.compile(r"^## (\S+) \| seviye: (Yuksek|Orta|Dusuk)\s*$")
SEV_ORD = {"Yuksek": 0, "Orta": 1, "Dusuk": 2}
bulgular = []
for p in sorted(D.glob("bulgu_*.md")):
    cur = None
    for l in p.read_text(encoding="utf-8").split("\n"):
        m = HEAD.match(l)
        if m:
            cur = dict(src=m.group(1), sev=m.group(2), yer="", iddia="", kanit="", oneri="", dosya=p.name)
            bulgular.append(cur)
            continue
        if l.startswith("## "):
            cur = None
            continue
        if cur is None:
            continue
        for key, pre in (("yer", "Yer:"), ("iddia", "Iddia:"), ("kanit", "Kanit:"), ("oneri", "Oneri:")):
            if l.startswith(pre):
                cur[key] = l[len(pre):].strip()
                break
        else:
            if l.strip() and cur["oneri"] and not l.startswith(("Yer:", "Iddia:", "Kanit:", "Oneri:")):
                cur["oneri"] += " " + l.strip()


def vals(b):
    ids = set()
    for m in re.finditer(r"VAL-([A-F])(\d{3})", b["yer"]):
        ids.add("VAL-" + m.group(1) + m.group(2))
    # "A165, A166" kisaltmalari
    for m in re.finditer(r"\(([^)]*)\)", b["yer"]):
        for t in re.findall(r"\b([A-F])(\d{3})\b", m.group(1)):
            ids.add("VAL-%s%s" % t)
    return ids


def toks(s):
    return set(re.findall(r"[\wçğıöşüÇĞİÖŞÜ]{4,}", s.lower()))


for b in bulgular:
    b["ids"] = vals(b)
    b["t"] = toks(b["iddia"])
    b["lines"] = re.findall(r"12:(\d+)", b["yer"])

merged = []
for b in sorted(bulgular, key=lambda x: (SEV_ORD[x["sev"]], x["src"])):
    hit = None
    for g in merged:
        if g["ids"] and b["ids"] and g["ids"] & b["ids"]:
            j = len(g["t"] & b["t"]) / max(1, len(g["t"] | b["t"]))
            if g["ids"] == b["ids"] or j >= 0.45:
                hit = g
                break
    if hit:
        hit["srcs"].append(b["src"])
        hit["ids"] |= b["ids"]
        hit["alt"].append(b)
        if SEV_ORD[b["sev"]] < SEV_ORD[hit["sev"]]:
            hit["sev"] = b["sev"]
    else:
        merged.append(dict(sev=b["sev"], srcs=[b["src"]], ids=set(b["ids"]), t=set(b["t"]), main=b, alt=[]))


def key_of(g):
    m = g["main"]
    lines = [int(x) for x in m["lines"]]
    return (SEV_ORD[g["sev"]], min(lines) if lines else 10**6)


merged.sort(key=key_of)
out = ["# Birlesik bulgu listesi (betik: 12_audit_birlestir.py)\n\nHam bulgu: %d; birlesik: %d\n" % (len(bulgular), len(merged))]
tsv = []
for n, g in enumerate(merged, 1):
    m = g["main"]
    g["n"] = n
    first = min([int(x) for x in m["lines"]] or [0])
    out.append("## #%d | seviye: %s | kaynak: %s\nYer: %s\nIddia: %s\nKanit: %s\nOneri: %s\n" % (
        n, g["sev"], ",".join(g["srcs"]), m["yer"], m["iddia"], m["kanit"], m["oneri"]))
    for a in g["alt"]:
        out.append("  (yinelenen %s) Iddia: %s | Oneri: %s\n" % (a["src"], a["iddia"], a["oneri"]))
    tsv.append("%d\t%s\t12:%d\t%s\t%s\t%s" % (n, g["sev"], first, ",".join(sorted(g["ids"]))[:120], ",".join(g["srcs"]), m["iddia"][:160]))
(D / "birlesik.md").write_text("\n".join(out), encoding="utf-8", newline="\n")
(D / "birlesik_ozet.tsv").write_text("\n".join(tsv) + "\n", encoding="utf-8", newline="\n")
from collections import Counter
print("ham %d; birlesik %d; seviye %s" % (len(bulgular), len(merged), dict(Counter(g["sev"] for g in merged))))
