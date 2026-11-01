"""Asama 10 acilisi -- devirlerin eslemesi (checklists/document-stage.md 1).

Kullanim (depo kokunden): python -I scripts/doc-scans/12_devir_esle.py . [cikti_dosyasi]

Alti tarama:
  A  karar kaydinin etki sutununda `12` / Dogrulama Protokolu tasiyan K satirlari (asama dagilimi,
     cipak numara adaylari)
  B  12'nin park bloklari (asama basina) -- maddeler ve etiketlerindeki K numaralari
  C  PHASE*_CONFLICT_SCAN.md 6'nin `12` bolumundeki K numaralari (dizin) ile A'nin farki
  D  etki sutunu K'lari ile park bloklarinin iki yonlu karsilastirmasi (etiket ve madde metni)
  E  hedefin adinin (12 / Dogrulama Protokolu) 12 disindaki butun dokumanlarin park bloklarinda aranmasi
  F  govde anmalari (02..11, DEPLOY_RUNBOOK, DEFERRED_BACKLOG) -- bugunku metinden

Ciktinin tamami dosyaya yazilir; ekrana yalniz sayim satirlari cikar.
"""
import re
import sys
from pathlib import Path

sys.stdout.reconfigure(encoding="utf-8")
root = Path(sys.argv[1] if len(sys.argv) > 1 else ".")
out_path = sys.argv[2] if len(sys.argv) > 2 else None
buf = []


def p(s=""):
    buf.append(s)


def ps(s):
    buf.append(s)
    print(s)


tracker = (root / "Docs/PRODUCT_DISCOVERY_STATUS.md").read_text(encoding="utf-8").split("\n")

CELL_SPLIT = re.compile(r"(?<!\\)\|")
STRICT_RE = re.compile(r"`12`|12_VALIDATION|Validation Protocol|Doğrulama Protokol")
LOOSE_RE = re.compile(r"(^|[ ·(,`])12([ ,(·)`;:§]|$)")
BOUNDS = [(1, 478, "Asama 1 workshop"), (479, 646, "Asama 1 kalite dongusu"), (647, 722, "Asama 2"),
          (723, 854, "Asama 3"), (855, 991, "Asama 4"), (992, 1143, "Asama 5"),
          (1144, 1232, "Asama 6"), (1233, 1300, "Asama 7"), (1301, 1400, "Asama 8"),
          (1401, 1503, "Asama 9"), (1504, 99999, "Asama 10")]


def asama(n):
    for lo, hi, name in BOUNDS:
        if lo <= n <= hi:
            return name
    return "?"


def expand(text):
    """Metindeki K numaralari; K-a...K-b (ya da K-a..K-b) araligi acilir."""
    ks = set()
    for m in re.finditer(r"K-(\d+)\s*(?:…|\.\.\.|\.\.)\s*K?-?(\d+)", text):
        a, b = int(m.group(1)), int(m.group(2))
        if a < b and b - a < 400:
            ks.update(range(a, b + 1))
    for m in re.finditer(r"K-(\d+)", text):
        ks.add(int(m.group(1)))
    return ks


# ------------------------------------------------------------------ A
a_rows = []
bare_rows = []
loose_only = []
# cipak doküman numarasi: " · " ile ayrilan parcada virgullu listenin bir ogesi tam "12" (ya da "12 (...)", "12 §...")
class _Bare:
    ITEM_RE = re.compile(r"^\*{0,2}12(\s*\(.*|\s*§.*)?\*{0,2}$")

    def search(self, cell):
        for part in re.split(r" · ", cell):
            for tok in re.split(r",\s+(?![^(]*\))", part):
                if self.ITEM_RE.match(tok.strip()):
                    return True
        return False


BARE_RE = _Bare()
total_rows = 0
for ln in tracker:
    if not re.match(r"^\| K-\d+ ", ln):
        continue
    total_rows += 1
    cells = CELL_SPLIT.split(ln)
    if len(cells) < 7:
        continue
    n = int(re.match(r"\| K-(\d+)", ln).group(1))
    if STRICT_RE.search(cells[6]):
        a_rows.append((n, cells[6].strip()))
    elif BARE_RE.search(cells[6]):
        bare_rows.append((n, cells[6].strip()))
    elif LOOSE_RE.search(cells[6]):
        loose_only.append((n, cells[6].strip()))
p("## A. Etki sutununda `12` / Dogrulama Protokolu tasiyan kararlar")
ps(f"A: K satiri toplam {total_rows}; adli (`12` / Dogrulama Protokolu) {len(a_rows)}; cipak numara ('12' ogesi) {len(bare_rows)}; "
   f"yalniz gevsek desen (yanlis pozitif adayi) {len(loose_only)}")
p("Gevsek desende yalniz eslesenler (yanlis pozitif adayi; sayi baska bir listenin ogesi mi -- her biri okunur):")
for n, e in loose_only:
    m = LOOSE_RE.search(e)
    s = max(0, m.start() - 60)
    p(f"  K-{n}: ...{e[s:m.end() + 80]}")
p("Cipak numara satirlari (parca '12' ya da '12 (is)'):")
for n, e in bare_rows:
    parts = [x for x in re.split(r" · ", e) if BARE_RE.search(x)]
    p(f"  K-{n}: " + " || ".join(s[:200] for s in parts))
p("Adli eslesen parcalar:")
for n, e in a_rows:
    parts = [x for x in re.split(r" · ", e) if STRICT_RE.search(x)]
    p(f"  K-{n}: " + " || ".join(s[:240] for s in parts))
# Gevsek desenle eslesen 11 satir okundu: yedisi gercek atif ("12 park satirlari", "08 / 12 (park)"),
# dordu yanlis pozitif (sayi baska bir listenin ogesi: madde numaralari, satir numarasi, "(12)" madde isareti)
LOOSE_REAL = {1161, 1178, 1179, 1181, 1216, 1217, 1226}
LOOSE_FALSE = {1145, 1207, 1333, 1479}
assert {n for n, _ in loose_only} == LOOSE_REAL | LOOSE_FALSE, "gevsek kume degisti -- yeniden oku"
a_rows_all = sorted(a_rows + bare_rows + [(n, e) for n, e in loose_only if n in LOOSE_REAL])
p(f"Gevsek desenden gercek atif sayilan: {len(LOOSE_REAL)}; yanlis pozitif elenen: {len(LOOSE_FALSE)}")
a_set = {n for n, _ in a_rows_all}
ps(f"A: etki sutununda `12` tasiyan toplam {len(a_set)} karar (adli {len(a_rows)} + cipak {len(bare_rows)} + gevsek-gercek {len(LOOSE_REAL)})")
by = {}
for n in sorted(a_set):
    by.setdefault(asama(n), []).append(n)
p("Asama dagilimi (etki sutunu):")
for name in [b[2] for b in BOUNDS]:
    if name in by:
        ps(f"  A/{name}: {len(by[name])}")


def dist(label, nums):
    d = {}
    for n in nums:
        d.setdefault(asama(n), []).append(n)
    p(f"  {label}: toplam {len(nums)}")
    for name in [b[2] for b in BOUNDS]:
        if name in d:
            p(f"    {name}: {len(d[name])} -> " + ", ".join(f"K-{x}" for x in d[name]))

# ------------------------------------------------------------------ B
doc = (root / "Docs/12_VALIDATION_PROTOCOL.md").read_text(encoding="utf-8").split("\n")
blocks = []  # (baslik, [(etiket, tam metin)])
cur = None
last_item = None
for ln in doc:
    if ln.startswith("## "):
        break
    m = re.match(r"> \*\*(Aşama \d+)'(?:den|dan|ten|tan)\s+park edilen girdiler\*\*", ln)
    if m:
        cur = (m.group(1), [])
        blocks.append(cur)
        last_item = None
        continue
    if cur is None:
        continue
    if ln.startswith("> - **"):
        label = ln[len("> - **"):].split(":**")[0]
        cur[1].append([label, ln])
        last_item = cur[1][-1]
    elif ln.startswith(">") and last_item is not None and ln.strip() not in (">",):
        last_item[1] += "\n" + ln
p("")
p("## B. `12` park bloklari")
ps(f"B: park blogu {len(blocks)}; madde {sum(len(b[1]) for b in blocks)}")
b_label = set()
b_text = set()
for title, items in blocks:
    p(f"  {title}: {len(items)} madde")
    for i, (lab, txt) in enumerate(items, 1):
        p(f"    {i}. {lab[:200]}")
        b_label |= expand(lab)
        b_text |= expand(txt)
p("  Etiketlerdeki K'lar (aralik acik): " + str(len(b_label)))

# ------------------------------------------------------------------ C
p("")
p("## C. Devir dizinleri (PHASE*_CONFLICT_SCAN.md 6) -- `12` bolumu")
c_set = set()
for f in sorted((root / "Docs/CHECKPOINT_REPORTS").glob("PHASE*_CONFLICT_SCAN.md")):
    txt = f.read_text(encoding="utf-8").split("\n")
    in6 = False
    in12 = False
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
            in12 = ("`12`" in ln)
            found = found or in12
            continue
        if in12:
            ks |= expand(ln)
    p(f"  {f.name}: `12` alt bolumu {'var' if found else 'yok'}; K: {len(ks)}")
    c_set |= ks
ps(f"C: dizin K sayisi {len(c_set)}")
p(f"  Etki sutunu K'lari ({len(a_set)}) ile dizin K'lari ({len(c_set)}):")
dz1 = sorted(a_set - c_set)
dz2 = sorted(c_set - a_set)
dist("etkide olup dizinde olmayan", dz1)
dist("dizinde olup etkide olmayan", dz2)
ps(f"C: etkide olup dizinde olmayan {len(dz1)}; dizinde olup etkide olmayan {len(dz2)}")

# ------------------------------------------------------------------ D
p("")
p("## D. Etki sutunu ile park bloklari (iki yonlu)")
d1 = sorted(a_set - b_label)
d2 = sorted(b_label - a_set)
d3 = sorted(a_set - b_text)
dist("etkide olup park ETIKETINDE anilmayan", d1)
dist("etkide olup park bloklarinin hicbir yerinde (etiket ya da metin) anilmayan", d3)
p("  parkta anilip etkide olmayan (etiket): " + (", ".join(f"K-{x}" for x in d2) or "-"))
ps(f"D: etkide olup etikette yok {len(d1)}; etkide olup blok metninde de yok {len(d3)}; etikette olup etkide yok {len(d2)}")
d4 = sorted(set(d3) - c_set)
dist("etkide olup park bloklarinda da, dizinde de (dosya olarak) anilmayan", d4)
ps(f"D: etkide olup ne park bloklarinda ne dizinde olan {len(d4)}")

# G -- d4'teki K'larin 02..11 dokumanlarinda gecip gecmedigi (kurali ust dokumana yazilmis devir; Kaynak satiri atfi)
p("")
p("## G. Dizinde ve parkta olmayan etki satirlarinin K numarasi 02..11 dokumanlarinda geciyor mu")
doc_ks = set()
for t in range(2, 12):
    for f in sorted((root / "Docs").glob(f"{t:02d}_*.md")):
        doc_ks |= expand(f.read_text(encoding="utf-8"))
g_gec = [n for n in d4 if n in doc_ks]
g_yok = [n for n in d4 if n not in doc_ks]
ps(f"G: d4 {len(d4)} karardan 02..11 dokumanlarinda K numarasiyla gecen {len(g_gec)}; gecmeyen {len(g_yok)}")
dist("02..11'de K numarasi gecmeyen (tek tek okunur)", g_yok)
for n in g_yok:
    row = next(e for k, e in a_rows_all if k == n)
    parts = [x for x in re.split(r" · ", row) if STRICT_RE.search(x) or BARE_RE.search(x)]
    p(f"    K-{n} [{asama(n)}]: " + " || ".join(s[:200] for s in parts))

for n in d3:
    row = next(e for k, e in a_rows_all if k == n)
    parts = [x for x in re.split(r" · ", row) if STRICT_RE.search(x) or BARE_RE.search(x)]
    p(f"    K-{n} [{asama(n)}]: " + " || ".join(s[:200] for s in parts))

# ------------------------------------------------------------------ E
p("")
p("## E. Hedefin adi 12 disindaki dokumanlarin park bloklarinda ve baslik notlarinda")
NAME_RE = re.compile(r"`12`|12_VALIDATION|Validation Protocol|Doğrulama Protokol")
e_total = 0
for f in sorted((root / "Docs").glob("*.md")):
    if f.name in ("12_VALIDATION_PROTOCOL.md", "PRODUCT_DISCOVERY_STATUS.md"):
        continue
    hits = []
    for i, ln in enumerate(f.read_text(encoding="utf-8").split("\n"), 1):
        if ln.startswith("## "):
            break
        if ln.startswith(">") and NAME_RE.search(ln):
            hits.append((i, ln))
    if hits:
        p(f"  {f.name}: {len(hits)} satir")
        for i, ln in hits:
            p(f"    {i}: {ln[:230]}")
        e_total += len(hits)
ps(f"E: toplam {e_total}")

# ------------------------------------------------------------------ F
BODY_RE = re.compile(r"`12`|12_VALIDATION|(?<![\w§.\-/])12 §|Validation Protocol|Doğrulama Protokol")
SENT_SPLIT = re.compile(r"(?<=[\.;:])\s+(?=[A-ZÇĞİÖŞÜ(*`\[])")
p("")
p("## F. Govde anmalari (> ve *Kaynak satirlari disinda; cumle ya da tablo hucresi)")
targets = ["02", "03", "04", "05", "06", "07", "08", "09", "10", "11", "DEPLOY_RUNBOOK", "DEFERRED_BACKLOG"]
f_total = 0
per = []
kinds = {}
MATRIS_RE = re.compile(r"ekran dışı — doğrulama \(`12`\)|veri doğurmaz — doğrulama \(`12`\)")
KAYIT_RE = re.compile(r"park (satır|blo)|sürüm alanı değişmez|sürümsüz|şablonu|Açık kalem ve devir|"
                      r"talimat bırakan|ileri işaret|Düzey \(`GUARDRAILS|Kimlikler\*\*|Kalite döngüsü")


def classify(fname, ln, s):
    """matris: matrisin hucre degeri (ayri sayilir); kayit: park/surum/geri besleme/kural notu (is tasimaz);
    aday: is birakabilecek cumle -- elle okunur."""
    if ln.startswith("|") and MATRIS_RE.search(s):
        return "matris"
    if KAYIT_RE.search(s):
        return "kayit"
    return "aday"


for t in targets:
    for f in sorted((root / "Docs").glob(f"{t}*.md")):
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
                kind = classify(f.name, ln, s)
                kinds[kind] = kinds.get(kind, 0) + 1
                p(f"    {f.name}:{i} <{kind}> [{BODY_RE.search(s).group(0)}]: {s.strip()[:300]}")
        p(f"  {f.name}: {n}")
        per.append(f"{f.name[:2] if f.name[:2].isdigit() else f.name.split('.')[0]} {n}")
        f_total += n
ps(f"F: toplam govde anmasi {f_total}; " + " | ".join(per))
ps("F: siniflar (matris hucre degeri / kayit notu / aday): " + ", ".join(f"{k} {v}" for k, v in sorted(kinds.items())))
sys.stdout.flush()

if out_path:
    Path(out_path).write_text("\n".join(buf) + "\n", encoding="utf-8")
