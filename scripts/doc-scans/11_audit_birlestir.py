"""Asama 9 -- 11 kalite dongusu audit'i: mercek bulgularini birlestirir (KD-audit, Parca A).

Girdi: <dizin>/bulgu_<paket>.md (her bulgu `### B-<paket>-<n> | Seviye | yer | kategori` basligiyla baslar).
Cikti: <dizin>/birlesik.md (yineleme elenmis, task'a ve bolume gore gruplu), <dizin>/birlesik.json (makine okunur),
       stdout: sayilar.
Yineleme: ayni task + ayni 11 satiri/yeri + Oneri'nin ayni ESKI parcasi -> tek bulgu (yerler birlestirilir).
Kullanim: python -I scripts/doc-scans/11_audit_birlestir.py <dizin>
"""
import json
import re
import sys
from collections import Counter, defaultdict
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

D = Path(sys.argv[1])
DOC = Path(r"C:\projects\Shopfolio\Docs\11_IMPLEMENTATION_PLAN.md")
LINES = DOC.read_text(encoding="utf-8").split("\n")

# satir -> task / bolum
blk = []
for i, l in enumerate(LINES):
    m = re.match(r"^#### (T\d{2,3}[a-z]?(?:\.\d+)?):", l)
    if m:
        blk.append((i + 1, m.group(1)))
heads = [(i + 1, l) for i, l in enumerate(LINES) if l.startswith("## ") or l.startswith("### ")]


def task_of(n):
    cur = None
    for s, t in blk:
        if s <= n:
            cur = t
        else:
            break
    # blok sonrasi ust bolum satiri ise task degil
    for s, h in heads:
        if cur and s > 0:
            pass
    return cur


def bolum_of(n):
    cur = ""
    for s, h in heads:
        if s <= n:
            cur = h
        else:
            break
    return cur


def seviye(s):
    s = s.lower()
    if s.startswith("y"):
        return "Yüksek"
    if s.startswith("o"):
        return "Orta"
    return "Düşük"


HDR = re.compile(r"^### (B-\S+)\s*\|\s*([^|]+?)\s*\|\s*([^|]*?)\s*\|\s*(.*)$")
bulgular = []
for f in sorted(D.glob("bulgu_*.md")):
    paket = f.stem[len("bulgu_"):]
    txt = f.read_text(encoding="utf-8").split("\n")
    cur = None
    for l in txt:
        m = HDR.match(l)
        if m:
            if cur:
                bulgular.append(cur)
            yer = m.group(3)
            ln = re.search(r"11:(\d+)", yer)
            n = int(ln.group(1)) if ln else None
            tm = re.search(r"\b(T\d{2,3}[a-z]?(?:\.\d+)?)\b", yer)
            task = tm.group(1) if tm else (task_of(n) if n else None)
            cur = dict(id=m.group(1), paket=paket, seviye=seviye(m.group(2)), yer=yer, satir=n, task=task,
                       kategori=m.group(4).strip(), govde=[])
        elif cur is not None:
            cur["govde"].append(l)
    if cur:
        bulgular.append(cur)

for b in bulgular:
    g = "\n".join(b["govde"]).strip()
    b["govde"] = g
    m = re.search(r"Öneri:\s*(.*)|Oneri:\s*(.*)", g, re.S)
    onr = (m.group(1) or m.group(2)) if m else ""
    b["oneri"] = onr.strip()
    b["bolum"] = bolum_of(b["satir"]) if b["satir"] else ""
    # faz
    t = b["task"] or ""
    mm = re.match(r"T(\d)", t)
    b["faz"] = ("F" + mm.group(1)) if mm else "ust"
    if t.startswith("T0") or re.match(r"T\d\d$", t):
        b["faz"] = "F0"


def anahtar(b):
    eski = b["oneri"].split("->")[0].strip()[:80] if "->" in b["oneri"] else b["oneri"][:60]
    return (b["task"], b["satir"], re.sub(r"\W+", "", eski.lower())[:50])


def kelimeler(b):
    s = re.sub(r"[`\"'()\[\].,;:→<>-]", " ", (b["govde"] + " " + b["oneri"]).lower())
    return {w for w in s.split() if len(w) > 4}


def benzer(a, b):
    if a["task"] != b["task"] or a["task"] is None:
        return False
    if anahtar(a) == anahtar(b):
        return True
    wa, wb = kelimeler(a), kelimeler(b)
    if not wa or not wb:
        return False
    return len(wa & wb) / len(wa | wb) >= 0.29


SEV = ["Yüksek", "Orta", "Düşük"]
uniq = []
yinelenen = 0
for b in bulgular:
    for g in uniq:
        if benzer(g, b):
            g["paketler"].append(b["id"])
            if SEV.index(b["seviye"]) < SEV.index(g["seviye"]):
                g["seviye"] = b["seviye"]
            yinelenen += 1
            break
    else:
        b2 = dict(b)
        b2["paketler"] = [b["id"]]
        uniq.append(b2)
for i, b in enumerate(uniq, 1):
    b["no"] = i

sira = {"Yüksek": 0, "Orta": 1, "Düşük": 2}
uniq.sort(key=lambda b: (sira[b["seviye"]], b["faz"], b["task"] or "", b["satir"] or 0))
(D / "birlesik.json").write_text(json.dumps(uniq, ensure_ascii=False, indent=1), encoding="utf-8", newline="\n")
out = ["# Birlesik bulgu listesi", "",
       "ham: %d; yinelenen elenen: %d; tekil: %d" % (len(bulgular), yinelenen, len(uniq)), ""]
for b in uniq:
    out.append("### #%d | %s | %s | %s | %s" % (b["no"], b["seviye"], b["task"], b["yer"], ", ".join(b["paketler"])))
    out.append(b["govde"])
    out.append("")
(D / "birlesik.md").write_text("\n".join(out), encoding="utf-8", newline="\n")

print("ham bulgu:", len(bulgular), "| yinelenen:", yinelenen, "| tekil:", len(uniq))
print("ham seviye:", dict(Counter(b["seviye"] for b in bulgular)))
print("tekil seviye:", dict(Counter(b["seviye"] for b in uniq)))
print("paket basina ham:", dict(sorted(Counter(b["paket"].split("_")[0] for b in bulgular).items())))
print("tekil faz x seviye:")
tab = defaultdict(Counter)
for b in uniq:
    tab[b["faz"]][b["seviye"]] += 1
for f in sorted(tab):
    print(" ", f, dict(tab[f]))
sizsiz = [b["id"] for b in bulgular if not b["task"]]
print("task'i belirlenemeyen:", len(sizsiz), sizsiz[:10])
