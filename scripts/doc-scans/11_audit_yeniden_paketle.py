"""Asama 9 -- KD-audit-2r: seviyesi yukselen 18 bulgunun yeniden dogrulama paketleri.

Kullanim: python -I scripts/doc-scans/11_audit_yeniden_paketle.py <kd-audit-2 dizini> <cikti dizini D>
Girdi: <kd-audit-2>/yeniden.md (17 bulgu), #336 (kd-audit/hukum_16.md), kd-audit/birlesik.json, plan.json, guncel 11.
Kademe (yeni seviye): Yuksek tek dogrulayici; Orta ayni moduldan 2-4'lu (tek kalan ayni fazin grubuna, olmazsa tek).
Cikti: <D>/dogrulayici_<k>.md + <D>/paketler.json
"""
import json
import re
import sys
from collections import defaultdict
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

K2 = Path(sys.argv[1])
D = Path(sys.argv[2])
D.mkdir(parents=True, exist_ok=True)
ROOT = Path(r"C:\projects\Shopfolio")
L11 = (ROOT / "Docs" / "11_IMPLEMENTATION_PLAN.md").read_text(encoding="utf-8").split("\n")
B = {b["no"]: b for b in json.loads((K2.parent / "kd-audit" / "birlesik.json").read_text(encoding="utf-8"))}
yol = str(K2).replace("\\", "/")
dyol = str(D).replace("\\", "/")

# --- bulgu listesi: no -> (eski, yeni, hukum dosyasi)
bul = {}
for l in (K2 / "yeniden.md").read_text(encoding="utf-8").split("\n"):
    m = re.match(r"^\| (\d+) \| (\w+) -> (\w+) \| \w+ \| (\S+) \|", l)
    if m:
        bul[int(m.group(1))] = (m.group(2), m.group(3), str(K2.parent / m.group(4)))
bul[336] = ("Orta", "Yuksek", str(K2.parent / "kd-audit" / "hukum_16.md"))
assert len(bul) == 18, len(bul)

mod = {}
for l in L11:
    m = re.match(r"^\| (T\d{2,3}[a-z]?) \| F\d \| .*? \| ([^|]+) \| ", l)
    if m:
        mod[m.group(1)] = m.group(2).strip()
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
    t = Path(bul[no][2]).read_text(encoding="utf-8")
    ms = list(re.finditer(r"^## #(\d+) ", t, re.M))
    for i, m in enumerate(ms):
        if int(m.group(1)) == no:
            return t[m.start(): ms[i + 1].start() if i + 1 < len(ms) else len(t)].rstrip()
    raise AssertionError("onceki hukum yok: %d" % no)


# --- gruplama
yuksek = sorted(n for n in bul if bul[n][1] == "Yuksek")
orta = sorted(n for n in bul if bul[n][1] == "Orta")
assert len(yuksek) == 11 and len(orta) == 7, (len(yuksek), len(orta))
paketler = [dict(seviye="Yuksek", modul=mod.get(B[n]["task"], "?"), nolar=[n]) for n in yuksek]
kova = defaultdict(list)
for n in orta:
    kova[mod.get(B[n]["task"], "?")].append(n)
tekler = []
for m, ns in sorted(kova.items()):
    if len(ns) == 1:
        tekler.append(ns[0])
    else:
        assert len(ns) <= 4
        paketler.append(dict(seviye="Orta", modul=m, nolar=ns))
for n in tekler:
    faz = B[n]["faz"]
    h = next((p for p in paketler if p["seviye"] == "Orta" and len(p["nolar"]) < 4
              and any(B[x]["faz"] == faz for x in p["nolar"])), None)
    if h:
        h["nolar"].append(n)
        h["modul"] += " + " + mod.get(B[n]["task"], "?")
    else:
        paketler.append(dict(seviye="Orta", modul=mod.get(B[n]["task"], "?"), nolar=[n]))

# --- sinama
tum = sorted(n for p in paketler for n in p["nolar"])
assert tum == sorted(bul) and len(tum) == 18, "18 no tam bir kez degil"
assert len(paketler) <= 20
for p in paketler:
    if p["seviye"] == "Yuksek":
        assert len(p["nolar"]) == 1
    else:
        assert len(p["nolar"]) <= 4

# --- yaz
for k, p in enumerate(paketler, 1):
    out = ["# Dogrulayici 2r/%d -- %s %s (%s)" % (k, p["seviye"], "tek" if len(p["nolar"]) == 1 else "%d'li" % len(p["nolar"]), p["modul"]), "",
           "Talimat: %s/talimat_dogrulayici.md (once oku)." % yol,
           "Hukum dosyasi: %s/hukum_%d.md" % (dyol, k),
           "Bulgu sayisi: %d (kimlikler: %s)" % (len(p["nolar"]), ", ".join("#%d" % n for n in p["nolar"])),
           "NOT: bulgudaki `11:<n>` numaralari v0.25'e aittir, metin degisti; yeri alintiyla grep -n ile bul.",
           "NOT: bu bulgular onceki adimda seviyesi YUKSELTILDI; yeni seviyede yeniden dogrulaniyor. Onceki hukumdeki sartname UYGULANMADI; "
           "dokuman hala eski metindedir (11'in guncel hali asagidaki bloktadir).", ""]
    for n in p["nolar"]:
        b = B[n]
        eski, yeni, _ = bul[n]
        out.append("## BULGU #%d | mercek seviyesi: %s (yeniden: %s->%s) | task: %s | yer: %s | mercek bulgulari: %s"
                   % (n, b["seviye"], eski, yeni, b["task"], b["yer"], ", ".join(b["paketler"])))
        if n == 134:
            out.append("NOT: ayni yer #12 paketinde (onun hukmu #12 sartnamesine birlestirilmisti); bu paket bagimsiz dogrulanir.")
        if n == 183:
            out.append("NOT (yeniden.md): 11:3870 k9 ayni ESKI satir ile #395 ile birlesti; ESKI artik #395'in uygulanmis hali (11'in guncel metni). "
                       "Sartnameyi guncel metne gore yaz; `--help` cumlesi (2r ekler) dahil.")
        out.append(b["govde"])
        out.append("")
        out.append("### Ilgili task blogu (%s) -- guncel metin" % b["task"])
        out.append(blok(b["task"]) if b["task"] else "(task yok)")
        out.append("")
        out.append("### ONCEKI DOGRULAYICININ HUKMU (guvenme; sifirdan kontrol et; sartnameyi guncel metne gore yeniden yaz)")
        out.append(onceki(n))
        out.append("")
    (D / ("dogrulayici_%d.md" % k)).write_text("\n".join(out), encoding="utf-8", newline="\n")
(D / "paketler.json").write_text(json.dumps(paketler, ensure_ascii=False, indent=1), encoding="utf-8", newline="\n")
print("paket: %d" % len(paketler))
for k, p in enumerate(paketler, 1):
    print(k, p["seviye"], p["modul"], p["nolar"])
