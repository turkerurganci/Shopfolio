"""Asama 9 -- 11 kalite dongusu audit'i: kademeli eleme ve dogrulayici gruplari (KD-audit, Parca A).

Girdi: <dizin>/birlesik.json (11_audit_birlestir.py ciktisi). Cikti (hepsi <dizin>/'de):
  dogrulama_plani.md   -- hangi dogrulayici hangi bulgulari alir; puan tablosu
  dogrulayici_<k>.md   -- dogrulayicinin gorev paketi (bulgunun tam metni + task blogu)
  dogrulanmadi.md      -- sigmayanlar (sonraki adimin girdisi; dokumana uygulanmaz)

Kademeli eleme (INSTRUCTIONS 9 (1): Yuksek tek; Orta 2-4'lu; Dusuk 5-8'li; adimda en cok 20 Opus dogrulayici):
  1. Betikle kanit eslesmesi: bulgunun `11:<n>| "alinti"` ve `NN:<n>| "alinti"` parcalari gosterdigi satirda geciyor mu.
  2. Risk puani (para / durum makinesi / guvenlik / kisisel veri / yetki / eszamanlilik anahtar sozcukleri, kategori, kanit eslesmesi,
     birden cok mercekte bulunma, okunmamis bolge).
  3. Tum Yuksek bulgular tek tek; kalan kontenjan Orta bulgularin en yuksek puanlilarina, ayni modulden <=4'lu gruplarla.
     Dusuk bulgular bu adimda Opus'a girmez.
Kullanim: python -I scripts/doc-scans/11_audit_grupla.py <dizin> [--azami 20]
"""
import json
import re
import sys
from collections import defaultdict
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

args = [a for a in sys.argv[1:]]
AZAMI = 20
if "--azami" in args:
    i = args.index("--azami")
    AZAMI = int(args[i + 1])
    del args[i:i + 2]
D = Path(args[0])
ROOT = Path(r"C:\projects\Shopfolio")
DOCS = {}
for p in sorted((ROOT / "Docs").glob("[0-1][0-9]_*.md")):
    DOCS[p.name[:2]] = p.read_text(encoding="utf-8").split("\n")
L11 = DOCS["11"]
B = json.loads((D / "birlesik.json").read_text(encoding="utf-8"))


def norm(s):
    s = s.replace("`", "").replace("*", "").replace("\u2026", "...")
    return re.sub(r"\s+", " ", s).strip().lower()


QRX = re.compile(r"\b(\d\d):(\d+)(?:-\d+)?\|\s*\"([^\"]+)\"")


def kanit_say(g):
    ok11 = tot11 = oks = tots = 0
    for fam, n, q in QRX.findall(g):
        n = int(n)
        ls = DOCS.get(fam)
        if not ls or n < 1 or n > len(ls):
            tots += 1 if fam != "11" else 0
            tot11 += 1 if fam == "11" else 0
            continue
        line = norm(ls[n - 1])
        parcalar = [p.strip() for p in re.split(r"\.\.\.", norm(q)) if len(p.strip()) >= 6]
        hit = bool(parcalar) and all(p in line for p in parcalar)
        if fam == "11":
            tot11 += 1
            ok11 += hit
        else:
            tots += 1
            oks += hit
    return ok11, tot11, oks, tots


RISK = re.compile(r"ödeme|odeme|iade|geri öde|tutar|para\b|kupon|stok|kdv|vergi|imza|parola|oturum|çerez|cerez|yetki|kvkk|kişisel|"
                  r"kisisel|imha|saklama|durum|geçiş|gecis|iptal|cayma|kilit|eşzamanlı|eszamanli|atomik|idempot|sızma|sizma|"
                  r"güvenlik|guvenlik|süre|sure\b|sınır|esik|eşik", re.I)
KAT = re.compile(r"kapsam|eksik|çelişki|celiski|yanlış|yanlis|uyumsuz|bağımlılık|bagimlilik|sıra|sira", re.I)
ONCELIK = re.compile(r"\b(02:Z-|02:L-|02 §[4-9]|02 §1[0-3]|07 §6\.2|05 MK|09 §|10:KP|04 E-)", re.I)

mod = {}
for l in L11:
    m = re.match(r"^\| (T\d{2,3}[a-z]?) \| F\d \| .*? \| ([^|]+) \| ", l)
    if m:
        mod[m.group(1)] = m.group(2).strip()

for b in B:
    g = b["govde"]
    b["ok11"], b["tot11"], b["oks"], b["tots"] = kanit_say(g)
    s = 0.0
    if b["tot11"]:
        s += 3 * (b["ok11"] / b["tot11"]) - 1.5 * (1 - b["ok11"] / b["tot11"])
    if b["tots"]:
        s += 2 * (b["oks"] / b["tots"])
    s += min(6, 1.5 * len(set(w.lower() for w in RISK.findall(g))))
    s += 2 if KAT.search(b["kategori"] + " " + g[:300]) else 0
    s += 1.5 * (len(set(b["paketler"])) - 1)
    s += 1 if ONCELIK.search(g) else 0
    if "Yüksek" == b["seviye"]:
        s += 100
    elif b["seviye"] == "Düşük":
        s -= 5
    b["puan"] = round(s, 2)
    b["modul"] = mod.get(b["task"], "?")

yuksek = [b for b in B if b["seviye"] == "Yüksek"]
orta = sorted([b for b in B if b["seviye"] == "Orta"], key=lambda b: -b["puan"])
dusuk = [b for b in B if b["seviye"] == "Düşük"]

kalan = AZAMI - len(yuksek)
if kalan < 0:
    raise SystemExit("Yuksek bulgu sayisi dogrulayici sinirini asiyor: %d > %d" % (len(yuksek), AZAMI))
# Orta: modul kovalarina, 4'luk gruplar; en cok `kalan` grup
gruplar = []   # her grup: dict(modul, uyeler)
secilen = []
for b in orta:
    if len(gruplar) >= kalan and not any(g["modul"] == b["modul"] and len(g["uyeler"]) < 4 for g in gruplar):
        continue
    hedef = None
    for g in gruplar:
        if g["modul"] == b["modul"] and len(g["uyeler"]) < 4:
            hedef = g
            break
    if hedef is None:
        if len(gruplar) >= kalan:
            continue
        hedef = dict(modul=b["modul"], uyeler=[])
        gruplar.append(hedef)
    hedef["uyeler"].append(b)
    secilen.append(b)
# 1 uyeli gruplari, kontenjan varsa baska ayni-faz grupla birlestir (aynı bolum: faz)
faz_of = lambda b: b["faz"]
for g in list(gruplar):
    if len(g["uyeler"]) == 1:
        for h in gruplar:
            if h is not g and len(h["uyeler"]) < 4 and faz_of(h["uyeler"][0]) == faz_of(g["uyeler"][0]):
                h["uyeler"].extend(g["uyeler"])
                gruplar.remove(g)
                break
# kontenjan hala varsa kalan Orta'dan yeni gruplar
secilen_idler = {b["no"] for b in secilen}
while len(yuksek) + len(gruplar) < AZAMI:
    rest = [b for b in orta if b["no"] not in secilen_idler]
    if not rest:
        break
    ilk = rest[0]
    uy = [b for b in rest if b["faz"] == ilk["faz"]][:4]
    gruplar.append(dict(modul=ilk["modul"], uyeler=uy))
    secilen_idler |= {b["no"] for b in uy}
secilen_idler = {b["no"] for g in gruplar for b in g["uyeler"]}

dog = []   # (k, tur, uyeler)
k = 0
for b in sorted(yuksek, key=lambda b: (b["faz"], b["task"] or "")):
    k += 1
    dog.append((k, "Yüksek (tek)", [b]))
for g in gruplar:
    k += 1
    dog.append((k, "Orta (%d'lu, modul/bolum: %s)" % (len(g["uyeler"]), g["modul"]), g["uyeler"]))
assert len(dog) <= AZAMI, (len(dog), AZAMI)

# ---------------------------------------------------------------- task bloklari
blk_start = {}
for i, l in enumerate(L11):
    m = re.match(r"^#### (T\d{2,3}[a-z]?(?:\.\d+)?):", l)
    if m:
        blk_start[m.group(1)] = i


def blok(t):
    if t not in blk_start:
        return "(blok bulunamadi)"
    i = blk_start[t]
    j = i + 1
    while j < len(L11) and not L11[j].startswith("#"):
        j += 1
    return "\n".join("11:%d| %s" % (n + 1, L11[n]) for n in range(i, j))


for k, tur, uy in dog:
    out = ["# Dogrulayici %d -- %s" % (k, tur), "",
           "Talimat: C:/Users/teknodeveloper/.claude/helper-state/shopfolio/kd-audit/talimat_dogrulayici.md (once oku).",
           "Hukum dosyasi: C:/Users/teknodeveloper/.claude/helper-state/shopfolio/kd-audit/hukum_%d.md" % k,
           "Bulgu sayisi: %d (kimlikler: %s)" % (len(uy), ", ".join("#%d" % b["no"] for b in uy)), ""]
    for b in uy:
        out.append("## BULGU #%d | mercek seviyesi: %s | task: %s | yer: %s | mercek bulgulari: %s"
                   % (b["no"], b["seviye"], b["task"], b["yer"], ", ".join(b["paketler"])))
        out.append(b["govde"])
        out.append("")
        out.append("### Ilgili task blogu (%s) -- guncel metin" % b["task"])
        out.append(blok(b["task"]) if b["task"] else "(task yok)")
        out.append("")
    (D / ("dogrulayici_%d.md" % k)).write_text("\n".join(out), encoding="utf-8", newline="\n")

# ---------------------------------------------------------------- plan ve dogrulanmadi
plan = ["# Dogrulama plani (KD-audit Parca A)", "",
        "Toplam dogrulayici: %d (azami %d). Yuksek tek: %d; Orta grup: %d; Dusuk: 0." % (len(dog), AZAMI, len(yuksek), len(gruplar)),
        "Dogrulanan bulgu: %d (Yuksek %d, Orta %d). Dogrulanmayan: %d." % (
            len(secilen_idler) + len(yuksek), len(yuksek), len(secilen_idler), len(B) - len(secilen_idler) - len(yuksek)),
        "", "| Dogrulayici | Tur | Bulgular |", "|---|---|---|"]
for k, tur, uy in dog:
    plan.append("| %d | %s | %s |" % (k, tur, ", ".join("#%d (%s %s, puan %s)" % (b["no"], b["task"], b["seviye"][0], b["puan"]) for b in uy)))
(D / "dogrulama_plani.md").write_text("\n".join(plan) + "\n", encoding="utf-8", newline="\n")

dd = ["# dogrulanmadi -- sonraki adimin girdisi (dokumana uygulanmaz)", "",
      "Neden: adimda en cok %d Opus dogrulayici (INSTRUCTIONS 9 (1); merge denetimi). Kademeli eleme: kanit eslesmesi + risk puani;" % AZAMI,
      "Yuksek hepsi dogrulandi; Orta'nin en yuksek puanlilari dogrulandi; geri kalan Orta ve butun Dusuk bulgular buradadir.", ""]
for b in sorted(B, key=lambda b: (b["seviye"] != "Orta", -b["puan"])):
    if b["seviye"] == "Yüksek" or b["no"] in secilen_idler:
        continue
    dd.append("### #%d | %s | puan %s | %s | %s | %s" % (b["no"], b["seviye"], b["puan"], b["task"], b["yer"], ", ".join(b["paketler"])))
    dd.append(b["govde"])
    dd.append("")
(D / "dogrulanmadi.md").write_text("\n".join(dd), encoding="utf-8", newline="\n")

print("dogrulayici: %d (Yuksek %d + Orta grup %d)" % (len(dog), len(yuksek), len(gruplar)))
print("dogrulanan Orta:", len(secilen_idler), "| dogrulanmayan:", len(B) - len(secilen_idler) - len(yuksek),
      "(Orta %d, Dusuk %d)" % (len(orta) - len(secilen_idler), len(dusuk)))
ok = sum(1 for b in B if b["tot11"] and b["ok11"] == b["tot11"])
print("11 alintisi tamamen eslesen bulgu: %d / %d; hic eslesmeyen: %d" % (
    ok, len(B), sum(1 for b in B if b["tot11"] and b["ok11"] == 0)))
