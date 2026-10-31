"""Asama 9 -- 11 kalite dongusu audit'i: dogrulanmamis 435 bulgunun TAMAMI icin dogrulayici plani (KD-audit-2).

Girdi: <girdi>/birlesik.json + <girdi>/dogrulanmadi.md (KD-audit ciktisi; varsayilan <dizin>/../kd-audit).
Kural (INSTRUCTIONS 9 (1)): Orta ayni moduldan 2-4'lu, Dusuk ayni moduldan 5-8'li tek dogrulayiciya; Opus high.
Artik: modulun 2/5'ten az kalani once ayni fazin ayni kademeli grubuna (sinir asilmadan) katilir; olmazsa ayni fazin
artiklariyla yeni karma grup kurulur; hala az kalan kucuk grup olarak planda gerekcesiyle yazilir.
Gruplar adimlara (2a, 2b, ...) bolunur: adim basina en cok 20 dogrulayici (adimin TOPLAMI). Bir modulun gruplari
mumkunse ayni adimdadir. Sira: para/durum riski (Odeme, Iptal-cayma-iade, Sepet ve siparis once).

Kullanim:
  python -I scripts/doc-scans/11_audit_grupla2.py <dizin> [--girdi <kd-audit dizini>]            -> plan.json, plan.md
  python -I scripts/doc-scans/11_audit_grupla2.py <dizin> --paket <adim> [--girdi ...]           -> <dizin>/<adim>/dogrulayici_<k>.md
  ... --paket 2e --ek 2b:19,2b:20   (--ek: baska adimdaki grup, k sirasiyla adimin sonuna eklenir)
<dizin> = .../kd-audit-2
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
EK = []
if "--ek" in args:
    i = args.index("--ek")
    EK = [x for x in args[i + 1].split(",") if x]
    del args[i:i + 2]
D = Path(args[0])
if GIRDI is None:
    GIRDI = D.parent / "kd-audit"
ROOT = Path(r"C:\projects\Shopfolio")
L11 = (ROOT / "Docs" / "11_IMPLEMENTATION_PLAN.md").read_text(encoding="utf-8").split("\n")
AZAMI = 20
ARALIK = {"Orta": (2, 4), "Düşük": (5, 8)}

B = {b["no"]: b for b in json.loads((GIRDI / "birlesik.json").read_text(encoding="utf-8"))}
NOS = [int(m.group(1)) for m in re.finditer(r"^### #(\d+) \|", (GIRDI / "dogrulanmadi.md").read_text(encoding="utf-8"), re.M)]
assert len(NOS) == 435 and len(set(NOS)) == 435, len(NOS)


def blok_basla():
    d = {}
    for i, l in enumerate(L11):
        m = re.match(r"^#### (T\d{2,3}[a-z]?(?:\.\d+)?):", l)
        if m:
            d[m.group(1)] = i
    return d


def blok(t, bs):
    if t not in bs:
        return "(blok bulunamadi)"
    i = bs[t]
    j = i + 1
    while j < len(L11) and not L11[j].startswith("#"):
        j += 1
    return "\n".join("11:%d| %s" % (n + 1, L11[n]) for n in range(i, j))


# ------------------------------------------------------------------ paket kipi
if PAKET:
    plan = json.loads((D / "plan.json").read_text(encoding="utf-8"))
    adim = next((a for a in plan["adimlar"] if a["adim"] == PAKET), None)
    assert adim, "adim yok: " + PAKET
    out_dir = D / PAKET
    out_dir.mkdir(parents=True, exist_ok=True)
    bs = blok_basla()
    yol = str(D).replace("\\", "/")
    gl = list(adim["gruplar"])
    for e in EK:   # ek gruplar: baska adimda kosmamis gruplar bu adimin dizinine eklenir (plan.json degismez)
        ea, ek_k = e.split(":")
        gl.append(next(a for a in plan["adimlar"] if a["adim"] == ea)["gruplar"][int(ek_k) - 1])
    for k, g in enumerate(gl, 1):
        out =["# Dogrulayici %s/%d -- %s %d'li (%s)" % (PAKET, k, g["seviye"], len(g["nolar"]), g["modul"]), "",
               "Talimat: %s/talimat_dogrulayici.md (once oku)." % yol,
               "Hukum dosyasi: %s/%s/hukum_%d.md" % (yol, PAKET, k),
               "Bulgu sayisi: %d (kimlikler: %s)" % (len(g["nolar"]), ", ".join("#%d" % n for n in g["nolar"])),
               "NOT: bulgudaki `11:<n>` numaralari v0.25'e aittir, metin degisti; yeri alintiyla grep -n ile bul.", ""]
        for n in g["nolar"]:
            b = B[n]
            out.append("## BULGU #%d | mercek seviyesi: %s | task: %s | yer: %s | mercek bulgulari: %s"
                       % (n, b["seviye"], b["task"], b["yer"], ", ".join(b["paketler"])))
            out.append(b["govde"])
            out.append("")
            out.append("### Ilgili task blogu (%s) -- guncel metin" % b["task"])
            out.append(blok(b["task"], bs) if b["task"] else "(task yok)")
            out.append("")
        (out_dir / ("dogrulayici_%d.md" % k)).write_text("\n".join(out), encoding="utf-8", newline="\n")
    print("paket: %s -> %d dogrulayici paketi (%s)" % (PAKET, len(gl), out_dir))
    sys.exit(0)

# ------------------------------------------------------------------ modul eslemesi
mod = {}
for l in L11:
    m = re.match(r"^\| (T\d{2,3}[a-z]?) \| F\d \| .*? \| ([^|]+) \| ", l)
    if m:
        mod[m.group(1)] = m.group(2).strip()

ONCELIK = [r"[Öö]deme", r"[İi]ptal|cayma|[İi]ade", r"Sepet|[Ss]ipari[şs]", r"Stok", r"Kupon|[Ff]iyat|Vergi|KDV", r"Kargo|Teslim",
           r"Hesap|[Oo]turum|Kimlik|Yetki|Güvenlik|Parola", r"KVKK|Ki[şs]isel|Veri", r"Fatura|[Bb]ildirim|E-?posta", r"."]


def oncelik(m):
    for i, rx in enumerate(ONCELIK):
        if re.search(rx, m):
            return i
    return len(ONCELIK)


for n in NOS:
    b = B[n]
    b["modul"] = mod.get(b["task"], "?")
    assert b["seviye"] in ARALIK, (n, b["seviye"])

# ------------------------------------------------------------------ gruplama
gruplar = []   # dict(seviye, modul, faz, nolar, not)


def parcala(n, lo, hi):
    """n ogeyi [lo,hi] boyutlu gruplara boler; (boyutlar, artik)."""
    for g in range(max(1, math.ceil(n / hi)), n // lo + 1):
        if lo * g <= n <= hi * g:
            base, ex = divmod(n, g)
            return [base + 1] * ex + [base] * (g - ex), 0
    # tam bolunemiyor: hi'lik gruplar + artik
    g = n // hi
    return [hi] * g, n - g * hi


anahtar = lambda n: (B[n]["faz"], B[n]["task"] or "", n)
artik = defaultdict(list)   # (seviye, faz) -> nolar
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

# artiklar: once ayni faz+seviye grubuna katil (sinir asilmadan), sonra karma grup
for (sev, faz), nolar in sorted(artik.items(), key=lambda x: str(x[0])):
    lo, hi = ARALIK[sev]
    kalan = []
    for n in nolar:
        hedef = next((g for g in gruplar if g["seviye"] == sev and g["faz"] == faz and len(g["nolar"]) < hi
                      and g["modul"] != "karma"), None)
        if hedef is None:
            hedef = next((g for g in gruplar if g["seviye"] == sev and g["modul"] == "karma %s" % faz and len(g["nolar"]) < hi), None)
            if hedef is not None and len(hedef["nolar"]) >= hi:
                hedef = None
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
                gruplar.append(dict(seviye=sev, modul="karma %s" % faz, faz=faz, nolar=kalan[i:i + s],
                                    not_="modul artiklari, ayni faz"))
                i += s
            kalan = kalan[i:]
        if kalan:
            gruplar.append(dict(seviye=sev, modul="karma %s" % faz, faz=faz, nolar=kalan,
                                not_="KUCUK: ayni fazda ayni kademede %d'den az artik, katilacak grup yok (sinir)" % lo))

for g in gruplar:
    g["nolar"].sort(key=anahtar)
    g["oncelik"] = min(oncelik(B[n]["modul"]) for n in g["nolar"])

# ------------------------------------------------------------------ adimlara bolme
sirali = sorted(gruplar, key=lambda g: (g["oncelik"], g["modul"], g["faz"], g["seviye"] != "Orta", g["nolar"][0]))
bymod = defaultdict(list)
for g in sirali:
    bymod[g["modul"]].append(g)
modsira = []
for g in sirali:
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
assert sorted(tum) == sorted(NOS) and len(tum) == len(set(tum)) == 435, "435 no tam bir kez degil"
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

plan = dict(toplam_bulgu=435, toplam_grup=sum(len(a) for a in adimlar),
            adimlar=[dict(adim=ad[i], gruplar=[dict(seviye=g["seviye"], modul=g["modul"], faz=g["faz"], nolar=g["nolar"], not_=g["not_"])
                                              for g in a]) for i, a in enumerate(adimlar)])
(D / "plan.json").write_text(json.dumps(plan, ensure_ascii=False, indent=1), encoding="utf-8", newline="\n")

md = ["# KD-audit-2 plani (betikle: 11_audit_grupla2.py)", "",
      "Toplam: 435 bulgu (Orta %d, Dusuk %d), %d dogrulayici grubu, %d adim (adim basina <= %d). KD-audit-2r bunlara ek (#336 + yukselenler)." % (
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
