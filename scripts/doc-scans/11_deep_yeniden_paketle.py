"""Asama 9 -- KD-deep-2r: seviyesi yukselen 2 bulgunun yeniden dogrulama paketleri.

Kullanim: python -I scripts/doc-scans/11_deep_yeniden_paketle.py <kd-deep-2 dizini> <cikti dizini D>
Girdi: <kd-deep-2>/yeniden.md, <kd-deep>/bulgular.md, onceki hukum dosyalari, guncel 11.
Kademe (yeni seviye Orta): ayni moduldan 2'li tek dogrulayici, degilse her bulgu ayri.
Cikti: <D>/dogrulayici_<k>.md + <D>/paketler.json
"""
import json
import re
import sys
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

K2 = Path(sys.argv[1])
D = Path(sys.argv[2])
D.mkdir(parents=True, exist_ok=True)
ROOT = Path(r"C:\projects\Shopfolio")
L11 = (ROOT / "Docs" / "11_IMPLEMENTATION_PLAN.md").read_text(encoding="utf-8").split("\n")
yol = str(K2).replace("\\", "/")
dyol = str(D).replace("\\", "/")

# --- bulgu listesi: no -> (eski, yeni, hukum dosyasi, adim)
bul = {}
for l in (K2 / "yeniden.md").read_text(encoding="utf-8").split("\n"):
    m = re.match(r"^\| #(\d+) \| (\w+) -> (\w+) \| (\w+) \(D\d+\) \| (\S+) \|", l)
    if m:
        bul[int(m.group(1))] = (m.group(2), m.group(3), K2 / m.group(5))
assert sorted(bul) == [117, 167], sorted(bul)

# --- bulgular.md: no -> (modul, ana, govde)
BT = (K2.parent / "kd-deep" / "bulgular.md").read_text(encoding="utf-8")
ms = list(re.finditer(r"^<!-- #(\d+) kaynak=(\S+) modul=(.*?) ana=(\S*) -->$", BT, re.M))
B = {}
for i, m in enumerate(ms):
    son = ms[i + 1].start() if i + 1 < len(ms) else len(BT)
    B[int(m.group(1))] = dict(modul=m.group(3), ana=m.group(4), govde=BT[m.end():son].strip())
for n in bul:
    assert n in B, n

bs = {}
for i, l in enumerate(L11):
    m = re.match(r"^#### (T\d{2,3}[a-z]?(?:\.\d+)?):", l)
    if m:
        bs[m.group(1)] = i


def blok(t):
    if t not in bs:
        return "(blok bulunamadi)"
    i = bs[t]
    j = i + 1
    while j < len(L11) and not L11[j].startswith("#"):
        j += 1
    return "\n".join("11:%d| %s" % (n + 1, L11[n]) for n in range(i, j))


def onceki(no):
    t = bul[no][2].read_text(encoding="utf-8")
    hs = list(re.finditer(r"^## #(\d+) ", t, re.M))
    for i, m in enumerate(hs):
        if int(m.group(1)) == no:
            return t[m.start(): hs[i + 1].start() if i + 1 < len(hs) else len(t)].rstrip()
    raise AssertionError("onceki hukum yok: %d" % no)


YAN = {117: ["T324", "T327"]}

# --- gruplama: ayni modul -> 2'li, degilse ayri
nolar = sorted(bul)
if B[nolar[0]]["modul"] == B[nolar[1]]["modul"]:
    paketler = [dict(seviye="Orta", modul=B[nolar[0]]["modul"], nolar=nolar)]
else:
    paketler = [dict(seviye="Orta", modul=B[n]["modul"], nolar=[n]) for n in nolar]
assert sorted(n for p in paketler for n in p["nolar"]) == nolar

for k, p in enumerate(paketler, 1):
    out = ["# Dogrulayici 2r/%d -- Orta %s (%s)" % (k, "tek" if len(p["nolar"]) == 1 else "%d'li" % len(p["nolar"]), p["modul"]), "",
           "Talimat: %s/talimat_dogrulayici.md (once oku)." % yol,
           "Hukum dosyasi: %s/hukum_%d.md" % (dyol, k),
           "Bulgu sayisi: %d (kimlikler: %s)" % (len(p["nolar"]), ", ".join("#%d" % n for n in p["nolar"])),
           "NOT: bulgudaki `11:<n>` numaralari v0.25'e aittir, metin degisti; yeri alintiyla grep -n ile bul.",
           "NOT: bu bulgular onceki adimda seviyesi YUKSELTILDI (Dusuk->Orta); yeni seviyede yeniden dogrulaniyor. "
           "Onceki hukumdeki sartname UYGULANMADI; dokuman hala eski metindedir (11'in guncel hali asagidaki bloktadir).", ""]
    for n in p["nolar"]:
        b = B[n]
        eski, yeni, _ = bul[n]
        out.append("## BULGU #%d | yeniden: %s->%s | task: %s | modul: %s" % (n, eski, yeni, b["ana"], b["modul"]))
        out.append(b["govde"])
        out.append("")
        out.append("### Ilgili task blogu (%s) -- guncel metin" % b["ana"])
        out.append(blok(b["ana"]))
        out.append("")
        for t in YAN.get(n, []):
            out.append("### Yan yuz task blogu (%s) -- guncel metin" % t)
            out.append(blok(t))
            out.append("")
        out.append("### ONCEKI DOGRULAYICININ HUKMU (guvenme; sifirdan kontrol et; sartnameyi guncel metne gore yeniden yaz)")
        out.append(onceki(n))
        out.append("")
    (D / ("dogrulayici_%d.md" % k)).write_text("\n".join(out), encoding="utf-8", newline="\n")
(D / "paketler.json").write_text(json.dumps(paketler, ensure_ascii=False, indent=1), encoding="utf-8", newline="\n")
print("paket: %d" % len(paketler))
for k, p in enumerate(paketler, 1):
    print(k, p["seviye"], p["modul"], p["nolar"])
