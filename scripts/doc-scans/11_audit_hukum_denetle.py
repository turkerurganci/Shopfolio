"""Asama 9 -- 11 kalite dongusu audit'i: dogrulayici hukum dosyalarini okur ve sartname ciftlerini sinar (KD-audit).

Girdi: <dizin>/hukum_<k>.md (talimat_dogrulayici.md biciminde). 11 dosyasi: Docs/11_IMPLEMENTATION_PLAN.md (varsayilan).
Her `ESKI:` degeri 11 dosyasinda TAM BIR KEZ gecmeli; ayni ESKI iki sartnamede olmamali; ESKI != YENI.
Cikis: stdout ozet; sorun varsa cikis kodu 1. `<dizin>/sartname_ozet.md` yazar (bulgu -> hukum -> cift sayisi; seviyesi yukselenler).
Kullanim: python -I scripts/doc-scans/11_audit_hukum_denetle.py <dizin> [--doc yol]
"""
import re
import sys
from collections import Counter
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

args = sys.argv[1:]
DOC = Path(r"C:\projects\Shopfolio\Docs\11_IMPLEMENTATION_PLAN.md")
if "--doc" in args:
    i = args.index("--doc")
    DOC = Path(args[i + 1])
    del args[i:i + 2]
D = Path(args[0])
TXT = DOC.read_text(encoding="utf-8")

hdr = re.compile(r"^## #(\d+) \| hukum: (\w+) \| seviye: (\w+)( \(degisti\))?", re.I)
ev = Counter()
seviyeler = []
cift = 0
sorun = []
satirlar = []
eskiler = Counter()
dosyalar = sorted(D.glob("hukum_*.md"), key=lambda p: int(re.search(r"(\d+)", p.stem).group(1)))
for f in dosyalar:
    cur = None
    parts = f.read_text(encoding="utf-8").split("\n")
    for l in parts:
        m = hdr.match(l)
        if m:
            cur = dict(no=int(m.group(1)), hukum=m.group(2).lower(), sev=m.group(3), deg=bool(m.group(4)), cift=0, dosya=f.name)
            satirlar.append(cur)
            ev[cur["hukum"]] += 1
            if cur["deg"]:
                seviyeler.append((cur["no"], cur["sev"], f.name))
            continue
        if cur is None:
            continue
        if l.startswith("ESKI:") or l.startswith("ESKİ:"):
            eski = l.split(":", 1)[1].strip().replace("\\n", "\n")  # "\n" yazimi: iki satirlik ESKI (baslik + satir)
            n = TXT.count(eski)
            eskiler[eski] += 1
            cur["cift"] += 1
            cift += 1
            if n != 1:
                sorun.append("%s #%d: ESKI dosyada %d kez geciyor: %.90s" % (f.name, cur["no"], n, eski))
        if l.startswith("YENI:") or l.startswith("YENİ:"):
            yeni = l.split(":", 1)[1].strip()
            if yeni == eski:
                sorun.append("%s #%d: YENI == ESKI" % (f.name, cur["no"]))
for e, c in eskiler.items():
    if c > 1:
        sorun.append("ayni ESKI %d sartnamede: %.90s" % (c, e))

no_set = [s["no"] for s in satirlar]
yinelen = [n for n, c in Counter(no_set).items() if c > 1]
cikti = ["# Sartname ozeti (hukum dosyalarindan, betikle)", "",
         "hukum dosyasi: %d; hukum satiri: %d; dogru %d, kismen %d, curuk %d; sartname cifti: %d" % (
             len(dosyalar), len(satirlar), ev["dogru"], ev["kismen"], ev["curuk"], cift),
         "seviyesi degisen (sonraki adimda yeniden dogrulanir, bu adimda degil): %s" % (seviyeler or "yok"),
         "yinelenen bulgu hukmu: %s" % (yinelen or "yok"),
         "sorun: %d" % len(sorun)] + ["- " + s for s in sorun] + ["", "| Bulgu | Hukum | Seviye | Cift | Dosya |", "|---|---|---|---|---|"]
for s in satirlar:
    cikti.append("| #%d | %s | %s%s | %d | %s |" % (s["no"], s["hukum"], s["sev"], " (degisti)" if s["deg"] else "", s["cift"], s["dosya"]))
(D / "sartname_ozet.md").write_text("\n".join(cikti) + "\n", encoding="utf-8", newline="\n")
print("\n".join(cikti[:6 + len(sorun)]))
sys.exit(1 if sorun else 0)
