"""Asama 9 -- 11 kalite dongusu deep review: dogrulanmamis 143 bulgunun TAMAMI icin dogrulayici plani (KD-deep-2).

Girdi: <girdi>/dogrulanmadi.md + <girdi>/bulgular.md (KD-deep ciktisi; varsayilan <dizin>/../kd-deep).
  bulgular.md: `<!-- #<no> kaynak=.. modul=<modul> ana=<task> -->` (modul ve ana task buradan).
Kural (INSTRUCTIONS 9 (1)): Orta ayni moduldan 2-4'lu, Dusuk ayni moduldan 5-8'li tek dogrulayiciya; Opus high.
Artik: modulun alt sinirdan az kalani once ayni fazin ayni kademeli grubuna (sinir asilmadan) katilir; olmazsa ayni fazin
artiklariyla karma grup; hala az kalan kucuk grup olarak gerekcesiyle yazilir.
Gruplar adimlara (2a, 2b, ...) bolunur: adim basina en cok 20 dogrulayici. Sira: para/durum riski.

Kullanim:
  python -I scripts/doc-scans/11_dr_grupla2.py <dizin> [--girdi <kd-deep dizini>]          -> plan.json, plan.md
  python -I scripts/doc-scans/11_dr_grupla2.py <dizin> --paket <adim> [--girdi ...]         -> <dizin>/<adim>/dogrulayici_<k>.md
<dizin> = .../kd-deep-2
"""
import json
import math
import re
import sys
from collections import defaultdict
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

args = list(sys.argv[1:])
GIRDI = None
PAKET = None
if "--girdi" in args:
    i = args.index("--girdi")
    GIRDI = Path(args[i + 1])
    del args[i:i + 2]
if "--paket" in args:
    i = args.index("--paket")
    PAKET = args[i + 1]
    del args[i:i + 2]
D = Path(args[0])
if GIRDI is None:
    GIRDI = D.parent / "kd-deep"
ROOT = Path(r"C:\projects\Shopfolio")
L11 = (ROOT / "Docs" / "11_IMPLEMENTATION_PLAN.md").read_text(encoding="utf-8").split("\n")
AZAMI = 20
ARALIK = {"Orta": (2, 4), "Düşük": (5, 8)}
TIDRX = r"T\d{2,3}[a-z]?(?:\.\d+)?"

# ------------------------------------------------------------------ girdi
B = {}
for p in re.split(r"(?m)^(?=## #\d+ )", (GIRDI / "dogrulanmadi.md").read_text(encoding="utf-8")):
    m = re.match(r"## #(\d+) \(([^,]+), kademe (\w+)\) \| seviye: (\S+) \| yer: (.*)", p)
    if not m:
        continue
    n = int(m.group(1))
    B[n] = dict(no=n, mercek=m.group(2), seviye={"Dusuk": "Düşük"}.get(m.group(4), m.group(4)), yer=m.group(5).strip(), govde=p.rstrip() + "\n")
NOS = sorted(B)
assert len(NOS) == 143 and len(set(NOS)) == 143, len(NOS)
for m in re.finditer(r"<!-- #(\d+) kaynak=(\S+) modul=(.*?) ana=(\S+) -->", (GIRDI / "bulgular.md").read_text(encoding="utf-8")):
    n = int(m.group(1))
    if n in B:
        B[n]["modul_eski"] = m.group(3)
        B[n]["ana"] = m.group(4)
for n in NOS:
    assert "ana" in B[n], n
    assert B[n]["seviye"] in ARALIK, (n, B[n]["seviye"])


def blok_araligi():
    starts = [(i, re.match(r"^#### (" + TIDRX + r"):", l).group(1)) for i, l in enumerate(L11)
              if re.match(r"^#### " + TIDRX + r":", l)]
    assert starts, "task blogu yok"
    d = {}
    for k, (i, t) in enumerate(starts):
        end = starts[k + 1][0] if k + 1 < len(starts) else len(L11)
        for j in range(i + 1, end):
            if L11[j].startswith("## ") or L11[j].startswith("### "):
                end = j
                break
        d[t] = (i, end)
    return d


BEND = blok_araligi()

mod = {}
faz = {}
for l in L11:
    m = re.match(r"^\| (" + TIDRX + r") \| (F\d) \| (.*?) \| (.*?) \| ", l)
    if m:
        faz[m.group(1)] = m.group(2)
        mod[m.group(1)] = m.group(4).strip()


def tasklar(b):
    """Bulgunun task'lari: ana ilk, metindeki (yer + kanit) task kimlikleri, en cok 4."""
    out = [b["ana"]]
    for t in re.findall(r"\b" + TIDRX + r"\b", b["govde"]):
        if t in BEND and t not in out:
            out.append(t)
    return [t for t in out if t in BEND][:4]


# ------------------------------------------------------------------ paket kipi
if PAKET:
    plan = json.loads((D / "plan.json").read_text(encoding="utf-8"))
    adim = next((a for a in plan["adimlar"] if a["adim"] == PAKET), None)
    assert adim, "adim yok: " + PAKET
    out_dir = D / PAKET
    out_dir.mkdir(parents=True, exist_ok=True)
    yol = str(D).replace("\\", "/")
    for k, g in enumerate(adim["gruplar"], 1):
        out = ["# Dogrulayici %s/%d -- %s %d'li (%s)" % (PAKET, k, g["seviye"], len(g["nolar"]), g["modul"]), "",
               "Talimat: %s/talimat_dogrulayici.md (once oku)." % yol,
               "Hukum dosyasi: %s/%s/hukum_%d.md" % (yol, PAKET, k),
               "Bulgu sayisi: %d (kimlikler: %s)" % (len(g["nolar"]), ", ".join("#%d" % n for n in g["nolar"])),
               "NOT: bulgudaki `11:<n>` numaralari yazildigi surume aittir (11 v0.33), metin degisti; yeri alintiyla grep -n ile bul.", "",
               "## BULGULAR (mercek metni; mercek Sonnet/low, hatali olabilir)", ""]
        tl = []
        for n in g["nolar"]:
            b = B[n]
            out.append("<!-- ana task: %s | mercek: %s -->" % (b["ana"], b["mercek"]))
            out.append(b["govde"])
            for t in tasklar(b):
                if t not in tl:
                    tl.append(t)
        out.append("## ILGILI TASK BLOKLARI (guncel, `11:<satir>|` onekli)")
        for t in tl:
            a, e = BEND[t]
            out.append("### BLOK %s\n%s" % (t, "\n".join("11:%d| %s" % (x + 1, L11[x]) for x in range(a, e))))
            out.append("")
        (out_dir / ("dogrulayici_%d.md" % k)).write_text("\n".join(out), encoding="utf-8", newline="\n")
    print("paket: %s -> %d dogrulayici paketi (%s)" % (PAKET, len(adim["gruplar"]), out_dir))
    sys.exit(0)

# ------------------------------------------------------------------ modul eslemesi
ONCELIK = [r"[Öö]deme", r"[İi]ptal|cayma|[İi]ade", r"Sepet|[Ss]ipari[şs]", r"Stok", r"Kupon|[Ff]iyat|Vergi|KDV", r"Kargo|Teslim",
           r"Hesap|[Oo]turum|Kimlik|Yetki|Güvenlik|Parola|Üyelik", r"KVKK|Ki[şs]isel|Veri", r"Fatura|[Bb]ildirim|E-?posta", r"."]


def oncelik(m):
    for i, rx in enumerate(ONCELIK):
        if re.search(rx, m):
            return i
    return len(ONCELIK)


for n in NOS:
    b = B[n]
    b["modul"] = mod.get(b["ana"], b.get("modul_eski", "?"))
    b["faz"] = faz.get(b["ana"], "?")

# ------------------------------------------------------------------ gruplama
gruplar = []


def parcala(n, lo, hi):
    for g in range(max(1, math.ceil(n / hi)), n // lo + 1):
        if lo * g <= n <= hi * g:
            base, ex = divmod(n, g)
            return [base + 1] * ex + [base] * (g - ex), 0
    g = n // hi
    return [hi] * g, n - g * hi


def tkey(t):
    m = re.match(r"T(\d+)([a-z]?)", t)
    return (int(m.group(1)), m.group(2), t)


anahtar = lambda n: (B[n]["faz"], tkey(B[n]["ana"]), n)
artik = defaultdict(list)
for sev, (lo, hi) in ARALIK.items():
    kova = defaultdict(list)
    for n in NOS:
        if B[n]["seviye"] == sev:
            kova[B[n]["modul"]].append(n)
    for m, nolar in sorted(kova.items()):
        nolar.sort(key=anahtar)
        if len(nolar) < lo:
            for n in nolar:
                artik[(sev, B[n]["faz"])].append(n)
            continue
        boy, art = parcala(len(nolar), lo, hi)
        i = 0
        for s in boy:
            gruplar.append(dict(seviye=sev, modul=m, faz=B[nolar[i]]["faz"], nolar=nolar[i:i + s], not_=""))
            i += s
        for n in nolar[i:]:
            artik[(sev, B[n]["faz"])].append(n)

for (sev, fz), nolar in sorted(artik.items(), key=lambda x: str(x[0])):
    lo, hi = ARALIK[sev]
    kalan = []
    for n in nolar:
        hedef = next((g for g in gruplar if g["seviye"] == sev and g["faz"] == fz and len(g["nolar"]) < hi
                      and not g["modul"].startswith("karma")), None)
        if hedef is None:
            hedef = next((g for g in gruplar if g["seviye"] == sev and g["modul"] == "karma %s" % fz and len(g["nolar"]) < hi), None)
        if hedef is not None:
            hedef["nolar"].append(n)
            hedef["not_"] = (hedef["not_"] + " artik katildi: #%d" % n).strip()
        else:
            kalan.append(n)
    if kalan:
        if len(kalan) >= lo:
            boy, art = parcala(len(kalan), lo, hi)
            i = 0
            for s in boy:
                gruplar.append(dict(seviye=sev, modul="karma %s" % fz, faz=fz, nolar=kalan[i:i + s], not_="modul artiklari, ayni faz"))
                i += s
            kalan = kalan[i:]
        if kalan:
            gruplar.append(dict(seviye=sev, modul="karma %s" % fz, faz=fz, nolar=kalan,
                                not_="KUCUK: ayni fazda ayni kademede %d'den az artik, katilacak grup yok (sinir)" % lo))

for g in gruplar:
    g["nolar"].sort(key=anahtar)
    g["oncelik"] = min(oncelik(B[n]["modul"]) for n in g["nolar"])

# ------------------------------------------------------------------ adimlara bolme
sirali = sorted(gruplar, key=lambda g: (g["oncelik"], g["modul"], g["faz"], g["seviye"] != "Orta", g["nolar"][0]))
bymod = defaultdict(list)
modsira = []
for g in sirali:
    bymod[g["modul"]].append(g)
    if g["modul"] not in modsira:
        modsira.append(g["modul"])
adimlar = [[]]
for m in modsira:
    gs = bymod[m]
    bos = AZAMI - len(adimlar[-1])
    if len(gs) > bos and len(gs) <= AZAMI and adimlar[-1]:
        adimlar.append([])
    for g in gs:
        if len(adimlar[-1]) >= AZAMI:
            adimlar.append([])
        adimlar[-1].append(g)

# ------------------------------------------------------------------ sinama
tum = [n for a in adimlar for g in a for n in g["nolar"]]
assert sorted(tum) == NOS and len(tum) == len(set(tum)) == 143, "143 no tam bir kez degil"
kucuk = []
for a in adimlar:
    assert 1 <= len(a) <= AZAMI, len(a)
    for g in a:
        lo, hi = ARALIK[g["seviye"]]
        assert len(g["nolar"]) <= hi, g
        assert all(B[n]["seviye"] == g["seviye"] for n in g["nolar"]), g
        if len(g["nolar"]) < lo:
            assert g["not_"].startswith("KUCUK"), g
            kucuk.append(g)
        if not g["modul"].startswith("karma"):
            assert all(B[n]["modul"] == g["modul"] or "artik katildi" in g["not_"] for n in g["nolar"]), g
ad = ["2" + "abcdefghij"[i] for i in range(len(adimlar))]

plan = dict(toplam_bulgu=143, toplam_grup=sum(len(a) for a in adimlar),
            adimlar=[dict(adim=ad[i], gruplar=[dict(seviye=g["seviye"], modul=g["modul"], faz=g["faz"], nolar=g["nolar"], not_=g["not_"])
                                              for g in a]) for i, a in enumerate(adimlar)])
(D / "plan.json").write_text(json.dumps(plan, ensure_ascii=False, indent=1), encoding="utf-8", newline="\n")

md = ["# KD-deep-2 plani (betikle: 11_dr_grupla2.py)", "",
      "Toplam: 143 bulgu (Orta %d, Dusuk %d), %d dogrulayici grubu, %d adim (adim basina <= %d)." % (
          sum(1 for n in NOS if B[n]["seviye"] == "Orta"), sum(1 for n in NOS if B[n]["seviye"] == "Düşük"),
          plan["toplam_grup"], len(adimlar), AZAMI),
      "Kucuk grup (alt sinirin altinda, gerekceli): %d" % len(kucuk), ""]
for i, a in enumerate(adimlar):
    md.append("## Adim %s -- %d dogrulayici, %d bulgu" % (ad[i], len(a), sum(len(g["nolar"]) for g in a)))
    md.append("")
    md.append("| k | Kademe | Modul | Faz | Bulgular | Not |")
    md.append("|---|---|---|---|---|---|")
    for k, g in enumerate(a, 1):
        md.append("| %d | %s %d'li | %s | %s | %s | %s |" % (k, g["seviye"], len(g["nolar"]), g["modul"], g["faz"],
                                                           ", ".join("#%d" % n for n in g["nolar"]), g["not_"]))
    md.append("")
(D / "plan.md").write_text("\n".join(md), encoding="utf-8", newline="\n")
print("grup %d, adim %d: %s" % (plan["toplam_grup"], len(adimlar), ", ".join("%s=%d" % (ad[i], len(a)) for i, a in enumerate(adimlar))))
print("kucuk grup:", len(kucuk), "| modul '?':", sum(1 for n in NOS if B[n]["modul"] == "?"))
