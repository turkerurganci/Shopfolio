"""Asama 9 -- KD-deep Parca A: mercek bulgularini birlestirir, kademeli dogrulayici gruplari kurar.

Girdi: <dizin>/bulgu_*.md (`## B<n> | seviye: ... | yer: 11:<satir> ...` bicimi)
Cikti: <dizin>/bulgular.md (hepsi, genel numarali), gorev_D<k>.md (dogrulayici gorev paketi),
       plan.md (grup tablosu), dogrulanmadi.md (butceye sigmayanlar)
Kademe: Yuksek tek; Orta ayni bolumden (modul) <= 4'lu; Dusuk <= 8'li. Toplam grup <= BUTCE (varsayilan 19).
Kullanim: python -I scripts/doc-scans/11_dr_birlestir.py <dizin> [--butce N] [--doc yol] [--ozel no,no,...]
  --ozel  sona eklenen tek ek grup (ornegin soz kaybi bulgulari); ayni modul kosulu aranmaz
"""
import re
import sys
from collections import defaultdict
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

ROOT = Path(r"C:\projects\Shopfolio")
ARGS = list(sys.argv[1:])
BUTCE = 19
DOC = ROOT / "Docs" / "11_IMPLEMENTATION_PLAN.md"
OZEL = set()
for name in ("--butce", "--doc", "--ozel"):
    if name in ARGS:
        i = ARGS.index(name)
        v = ARGS[i + 1]
        del ARGS[i:i + 2]
        if name == "--ozel":
            OZEL = {int(x) for x in v.split(",")}
        elif name == "--butce":
            BUTCE = int(v)
        else:
            DOC = Path(v)
if not ARGS:
    raise SystemExit("kullanim: 11_dr_birlestir.py <dizin>")
D = Path(ARGS[0])
LINES = DOC.read_text(encoding="utf-8").split("\n")
TIDRX = r"T\d{2,3}[a-z]?(?:\.\d+)?"

# blok araligi ve aday tablo modulu
starts = [(i, re.match(r"^#### (" + TIDRX + r"):", l).group(1)) for i, l in enumerate(LINES)
          if re.match(r"^#### " + TIDRX + r":", l)]
if not starts:
    raise SystemExit("task blogu yok (bozuk girdi?)")
BEND = {}
for k, (i, t) in enumerate(starts):
    end = starts[k + 1][0] if k + 1 < len(starts) else len(LINES)
    for j in range(i + 1, end):
        if LINES[j].startswith("## ") or LINES[j].startswith("### "):
            end = j
            break
    BEND[t] = (i, end)
MOD = {}
for l in LINES:
    m = re.match(r"^\| (" + TIDRX + r") \| (F\d) \| (.*?) \| (.*?) \| ", l)
    if m:
        MOD[m.group(1)] = (m.group(2), m.group(4))


def task_of_line(n):
    for t, (a, b) in BEND.items():
        if a <= n - 1 < b:
            return t
    return None


def tid_key(t):
    m = re.match(r"T(\d+)([a-z]?)", t)
    return (int(m.group(1)), m.group(2), t)


SEV = {"yuksek": "Yuksek", "yüksek": "Yuksek", "orta": "Orta", "dusuk": "Dusuk", "düşük": "Dusuk"}
bulgular = []
for f in sorted(D.glob("bulgu_*.md")):
    txt = f.read_text(encoding="utf-8")
    parts = re.split(r"(?m)^(?=## B\d+ )", txt)
    for p in parts:
        m = re.match(r"## B(\d+)\s*\|\s*seviye:\s*(\w+)\s*\|\s*yer:\s*(.*)", p)
        if not m:
            continue
        sev = SEV.get(m.group(2).lower())
        if sev is None:
            print("UYARI: seviye taninmadi", f.name, m.group(1), m.group(2))
            continue
        yerler = [int(x) for x in re.findall(r"11:(\d+)", m.group(3))]
        tasks = []
        for n in yerler:
            t = task_of_line(n)
            if t and t not in tasks:
                tasks.append(t)
        for t in re.findall(r"\b" + TIDRX + r"\b", m.group(3) + " " + p[:600]):
            if t in BEND and t not in tasks:
                tasks.append(t)
        tasks = tasks[:4]
        ana = tasks[0] if tasks else None
        bulgular.append(dict(kaynak=f.stem[6:], yerel=m.group(1), sev=sev, tasks=tasks, ana=ana,
                             modul=MOD.get(ana, ("?", "?"))[1] if ana else "?",
                             faz=MOD.get(ana, ("?", "?"))[0] if ana else "?", metin=p.rstrip() + "\n"))
for i, b in enumerate(bulgular, 1):
    b["no"] = i

yuk = [b for b in bulgular if b["sev"] == "Yuksek"]
ort = sorted([b for b in bulgular if b["sev"] == "Orta"], key=lambda b: (b["modul"], tid_key(b["ana"] or "T999")))
dus = sorted([b for b in bulgular if b["sev"] == "Dusuk"], key=lambda b: (b["modul"], tid_key(b["ana"] or "T999")))


def chunk(items, n):
    """Ayni modul oncelikli: modul gruplarini n'lik parcalara boler; artan kucuk parcalar komsu parcayla birlesir."""
    bym = defaultdict(list)
    for b in items:
        bym[b["modul"]].append(b)
    out, rest = [], []
    for m in sorted(bym):
        l = bym[m]
        while len(l) >= n:
            out.append(l[:n]); l = l[n:]
        if l:
            rest.extend(l)
    # artanlar (farkli modul) n'lik parcalara
    while rest:
        out.append(rest[:n]); rest = rest[n:]
    return out


gruplar = []   # (kademe, [bulgu])
for b in yuk:
    gruplar.append(("Yuksek", [b]))
for g in chunk(ort, 4):
    gruplar.append(("Orta", g))
for g in chunk(dus, 8):
    gruplar.append(("Dusuk", g))
secilen = gruplar[:BUTCE]
kalan = gruplar[BUTCE:]
if OZEL:
    secilen_nolar = {b["no"] for _, g in secilen for b in g}
    ozel = [b for b in bulgular if b["no"] in OZEL and b["no"] not in secilen_nolar]
    secilen.append(("Orta", ozel))
    kalan = [(kad, [b for b in g if b["no"] not in OZEL]) for kad, g in kalan]
    kalan = [(kad, g) for kad, g in kalan if g]

(D / "bulgular.md").write_text("# KD-deep ham bulgular (%d)\n\n" % len(bulgular) + "\n".join(
    "<!-- #%d kaynak=%s modul=%s ana=%s -->\n%s" % (b["no"], b["kaynak"], b["modul"], b["ana"],
                                                  re.sub(r"^## B\d+", "## #%d" % b["no"], b["metin"], count=1)) for b in bulgular),
    encoding="utf-8", newline="\n")

plan = ["| D | Kademe | Bulgu | Moduller | Gorev paketi |", "|---|---|---|---|---|"]
for k, (kad, g) in enumerate(secilen, 1):
    nos = [b["no"] for b in g]
    tasks = []
    for b in g:
        for t in b["tasks"]:
            if t not in tasks:
                tasks.append(t)
    tasks = sorted(tasks, key=tid_key)[:10]
    govde = ["# Dogrulayici gorev paketi D%d (KD-deep, Parca A; kademe %s; %d bulgu)" % (k, kad, len(g)),
             "Talimat: C:/Users/teknodeveloper/.claude/helper-state/shopfolio/kd-deep/talimat_dogrulayici.md",
             "Hukum dosyasi (yaz): C:/Users/teknodeveloper/.claude/helper-state/shopfolio/kd-deep/hukum_D%d.md" % k,
             "Bulgu numaralari: " + ", ".join("#%d" % n for n in nos), "",
             "## BULGULAR (mercek metni; mercek Sonnet/low, hatali olabilir)", ""]
    for b in g:
        govde.append(re.sub(r"^## B\d+", "## #%d" % b["no"], b["metin"], count=1))
    govde.append("## ILGILI TASK BLOKLARI (guncel, `11:<satir>|` onekli)")
    for t in tasks:
        a, e = BEND[t]
        govde.append("### BLOK %s\n%s" % (t, "\n".join("11:%d| %s" % (n + 1, LINES[n]) for n in range(a, e))))
    (D / ("gorev_D%d.md" % k)).write_text("\n".join(govde) + "\n", encoding="utf-8", newline="\n")
    plan.append("| D%d | %s | %s | %s | %d bayt |" % (k, kad, ", ".join("#%d" % n for n in nos),
                                                      "; ".join(sorted({b["modul"] for b in g})), len("\n".join(govde).encode("utf-8"))))
(D / "plan.md").write_text("\n".join(plan) + "\n", encoding="utf-8", newline="\n")

dn = ["# Dogrulanmadi (butceye sigmayan; dokumana UYGULANMAZ; KD-deep-2 adimlarinda dogrulanir)", ""]
for kad, g in kalan:
    for b in g:
        dn.append(re.sub(r"^## B\d+", "## #%d (%s, kademe %s)" % (b["no"], b["kaynak"], kad), b["metin"], count=1))
(D / "dogrulanmadi.md").write_text("\n".join(dn) + "\n", encoding="utf-8", newline="\n")

print("ham bulgu: %d (Yuksek %d, Orta %d, Dusuk %d)" % (len(bulgular), len(yuk), len(ort), len(dus)))
print("grup toplami: %d; secilen: %d; dogrulanmayan bulgu: %d" % (len(gruplar), len(secilen), sum(len(g) for _, g in kalan)))
print("\n".join(plan))
