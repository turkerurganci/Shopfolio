"""Asama 9 acilisi -- devirlerin eslemesi (checklists/document-stage.md 1).

Kullanim (depo kokunden): python -I scripts/doc-scans/11_devir_esle.py . [cikti_dosyasi]

Dort tarama:
  A  karar kaydinin etki sutununda `11` / Implementation Plani tasiyan K satirlari (asama dagilimi)
  B  11'in park blogundaki maddeler ve etiketlerindeki K numaralari
  C  PHASE*_CONFLICT_SCAN.md 6'nin `11` bolumundeki K numaralari (dizin) ile A'nin farki
  D  hedefin adinin (11 / Implementation Plani) bitun dokumanlarin park bloklarinda aranmasi
  E  gövde anmalari (02..10, 12, DEPLOY_RUNBOOK, DEFERRED_BACKLOG) -- bugunku metinden
"""
import re
import sys
from pathlib import Path

root = Path(sys.argv[1] if len(sys.argv) > 1 else ".")
out_path = sys.argv[2] if len(sys.argv) > 2 else None
lines_out = []


def p(s=""):
    lines_out.append(s)
    print(s)


tracker = (root / "Docs/PRODUCT_DISCOVERY_STATUS.md").read_text(encoding="utf-8").split("\n")

LOOSE_RE = re.compile(r"(^|[ ·(,`])11([ ,(·)`;:]|$)|Implementation Plan|Uygulama Plan")
ETKI_RE = re.compile(r"`11`|11_IMPLEMENTATION|Implementation Plan|Uygulama Plan")
loose_only = []
CELL_SPLIT = re.compile(r"(?<!\\)\|")
BOUNDS = [(1, 478, "Asama 1 workshop"), (479, 646, "Asama 1 kalite dongusu"), (647, 722, "Asama 2"),
          (723, 854, "Asama 3"), (855, 991, "Asama 4"), (992, 1143, "Asama 5"),
          (1144, 1232, "Asama 6"), (1233, 1300, "Asama 7"), (1301, 1400, "Asama 8"),
          (1401, 99999, "Asama 9")]


def asama(n):
    for lo, hi, name in BOUNDS:
        if lo <= n <= hi:
            return name
    return "?"


# A -- etki sutunu
a_rows = []
total_rows = 0
for ln in tracker:
    if not re.match(r"^\| K-\d+ ", ln):
        continue
    total_rows += 1
    cells = CELL_SPLIT.split(ln)
    if len(cells) < 7:
        continue
    n = int(re.match(r"\| K-(\d+)", ln).group(1))
    if ETKI_RE.search(cells[6]):
        a_rows.append((n, cells[6].strip()))
    elif LOOSE_RE.search(cells[6]):
        loose_only.append(n)
p("## A. Etki sutununda `11` / Implementation Plani tasiyan kararlar")
p(f"K satiri toplam: {total_rows}; eslesen (kati desen: `11` ya da Implementation/Uygulama Plani): {len(a_rows)}")
p("Gevsek desende yalniz eslesen, okunup yanlis pozitif sayilanlar (sayi 11 baska bir listenin ogesi): "
  + (", ".join(f"K-{x}" for x in loose_only) or "-"))
by = {}
for n, _ in a_rows:
    by.setdefault(asama(n), []).append(n)
for name in [b[2] for b in BOUNDS]:
    if name in by:
        p(f"  {name}: {len(by[name])} -> " + ", ".join(f"K-{x}" for x in by[name]))
for n, e in a_rows:
    # eslesen parcayi goster
    parts = [x for x in re.split(r" · ", e) if ETKI_RE.search(x)]
    p(f"  K-{n}: " + " || ".join(s[:260] for s in parts))
a_set = {n for n, _ in a_rows}

# B -- 11'in park blogu
doc11 = (root / "Docs/11_IMPLEMENTATION_PLAN.md").read_text(encoding="utf-8").split("\n")
park = []
for ln in doc11:
    if ln.startswith("## "):
        break
    if ln.startswith("> - **"):
        label = ln[len("> - **"):].split(":**")[0]
        park.append(label)
p("")
p("## B. `11` park blogu")
p(f"Madde sayisi: {len(park)}")
b_set = set()
for i, lab in enumerate(park, 1):
    ks = [int(x) for x in re.findall(r"K-(\d+)", lab)]
    b_set.update(ks)
    p(f"  P8.{i}: {lab}")
p("  Etiketlerdeki K'lar: " + ", ".join(f"K-{x}" for x in sorted(b_set)))

# C -- dizin
p("")
p("## C. Devir dizinleri (PHASE*_CONFLICT_SCAN.md 6) -- `11` bolumu")
c_set = set()
for f in sorted((root / "Docs/CHECKPOINT_REPORTS").glob("PHASE*_CONFLICT_SCAN.md")):
    txt = f.read_text(encoding="utf-8").split("\n")
    in6 = False
    in11 = False
    ks = set()
    found = False
    for ln in txt:
        if ln.startswith("## 6"):
            in6 = True
            continue
        if in6 and ln.startswith("## "):
            break
        if not in6:
            continue
        if ln.startswith("#### ") or ln.startswith("### "):
            in11 = ("`11`" in ln)
            found = found or in11
            continue
        if in11:
            ks.update(int(x) for x in re.findall(r"K-(\d+)", ln))
    p(f"  {f.name}: `11` alt bolumu {'var' if found else 'yok'}; K: " + (", ".join(f"K-{x}" for x in sorted(ks)) or "-"))
    c_set |= ks
p(f"  Etki sutunu K'lari ({len(a_set)}) ile dizin K'lari ({len(c_set)}):")
p("   etki ve dizinde olmayan: " + (", ".join(f"K-{x}" for x in sorted(a_set - c_set)) or "-"))
p("   dizinde olup etki sutununda olmayan: " + (", ".join(f"K-{x}" for x in sorted(c_set - a_set)) or "-"))
p("  Etki sutunu K'lari ile 11 park etiketleri:")
p("   etkide olup parkta anilmayan: " + (", ".join(f"K-{x}" for x in sorted(a_set - b_set)) or "-"))
p("   parkta anilip etkide olmayan: " + (", ".join(f"K-{x}" for x in sorted(b_set - a_set)) or "-"))

# D -- hedefin adi tum park bloklarinda
NAME_RE = re.compile(r"`11`|11_IMPLEMENTATION|Implementation Plan|Uygulama Plan")
p("")
p("## D. Hedefin adi bitun dokumanlarin park bloklarinda ve baslik notlarinda")
d_total = 0
for f in sorted((root / "Docs").glob("*.md")):
    if f.name == "11_IMPLEMENTATION_PLAN.md" or f.name == "PRODUCT_DISCOVERY_STATUS.md":
        continue
    txt = f.read_text(encoding="utf-8").split("\n")
    hits = []
    for i, ln in enumerate(txt, 1):
        if ln.startswith("## "):
            break
        if ln.startswith(">") and NAME_RE.search(ln):
            hits.append((i, ln))
    if hits:
        p(f"  {f.name}: {len(hits)} satir")
        for i, ln in hits:
            p(f"    {i}: {ln[:230]}")
        d_total += len(hits)
p(f"  Toplam: {d_total}")

# E -- govde anmalari
BODY_RE = re.compile(r"`11`|11_IMPLEMENTATION|(?<![\w§.\-/])11 §|Implementation Plan|Uygulama Plan")
SENT_SPLIT = re.compile(r"(?<=[\.;:])\s+(?=[A-ZÇĞİÖŞÜ(*`\[])")
p("")
p("## E. Govde anmalari (> ve *Kaynak satirlari disinda; cumle ya da tablo hucresi)")
targets = ["02", "03", "04", "05", "06", "07", "08", "09", "10", "12", "DEPLOY_RUNBOOK", "DEFERRED_BACKLOG"]
e_total = 0
for t in targets:
    cands = sorted((root / "Docs").glob(f"{t}*.md"))
    for f in cands:
        n = 0
        for i, ln in enumerate(f.read_text(encoding="utf-8").split("\n"), 1):
            if ln.startswith(">") or ln.startswith("*Kaynak") or not BODY_RE.search(ln):
                continue
            if ln.startswith("|"):
                pieces = [c for c in CELL_SPLIT.split(ln) if BODY_RE.search(c)]
            else:
                pieces = [s for s in SENT_SPLIT.split(ln) if BODY_RE.search(s)]
            for s in pieces:
                n += 1
                p(f"    {f.name}:{i} [{BODY_RE.search(s).group(0)}]: {s.strip()[:300]}")
        p(f"  {f.name}: {n}")
        e_total += n
p(f"  Toplam govde anmasi: {e_total}")

if out_path:
    Path(out_path).write_text("\n".join(lines_out) + "\n", encoding="utf-8")
