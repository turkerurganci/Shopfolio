"""Asama 9 -- izlenebilirlik matrisinin kaynak envanteri ve kapsama denetimi
(checklists/document-stage.md 2; karar kaydi 22.2, K-1404).

Kaynak birimleri (aile basina): 02 kural/kimlikli satir/alt bolum; 03 alt bolum;
04 ekran/ortak bilesen/alt bolum; 05 MK karari/alt bolum; 06 entity/enum/alt bolum;
07 uc (UC)/alt bolum; 08 entegrasyon/alt bolum; 09 bolum/park maddesi; 10 KP satiri/alt bolum;
devir satirlari (11_devir_esle.py ciktisi). Her aile iki bagimsiz yoldan sayilir
(envanter tablosu <-> tanim basligi, ya da kimlik dizisinin sureklilik denetimi);
kapsama denetimi beklenen kume ile bulunan kume arasinda eksik / fazla / yinelenen sayar.

Kullanim (depo kokunden): python -I scripts/doc-scans/11_matris_envanter.py . [cikti_dosyasi]
Cikis kodu: eksik, fazla ya da yinelenen sifirdan buyukse 1.
"""
import re
import sys
from collections import Counter
from pathlib import Path

root = Path(sys.argv[1] if len(sys.argv) > 1 else ".")
out_path = sys.argv[2] if len(sys.argv) > 2 else None
out = []


def p(s=""):
    out.append(s)
    print(s)


def load(prefix):
    f = sorted((root / "Docs").glob(prefix + "_*.md"))[0]
    return f.read_text(encoding="utf-8").split("\n")


def bolum(lines, n):
    """`## n.` basligindan sonraki `## ` basligina kadar (satir_no, metin) listesi."""
    res, inside = [], False
    for i, l in enumerate(lines, 1):
        if l.startswith("## "):
            inside = re.match(r"## %s\. " % re.escape(str(n)), l) is not None
            continue
        if inside:
            res.append((i, l))
    return res


problems = 0


def denetle(ad, beklenen, bulunan_liste):
    """beklenen: kume; bulunan_liste: liste (yinelenen icin)."""
    global problems
    c = Counter(bulunan_liste)
    bulunan = set(c)
    eksik = sorted(beklenen - bulunan, key=str)
    fazla = sorted(bulunan - beklenen, key=str)
    yinelenen = sorted([k for k, v in c.items() if v > 1], key=str)
    problems += len(eksik) + len(fazla) + len(yinelenen)
    p(f"| {ad} | {len(beklenen)} | {len(bulunan_liste)} | {len(eksik)} | {len(fazla)} | {len(yinelenen)} |")
    for lbl, v in (("eksik", eksik), ("fazla", fazla), ("yinelenen", yinelenen)):
        if v:
            p(f"    {lbl}: {v[:20]}")


def numsira(ids):
    """['3.1.1','3.1.2'...] -> beklenen ardisik kume (her n.m icin 1..max)."""
    mx = {}
    for s in ids:
        a, b, k = s.split(".")
        mx[(a, b)] = max(mx.get((a, b), 0), int(k))
    return {f"{a}.{b}.{k}" for (a, b), m in mx.items() for k in range(1, m + 1)}


def alt_bolumler(lines, nums, seviye="###"):
    res = []
    for n in nums:
        for i, l in bolum(lines, n):
            if l.startswith(seviye + " "):
                m = re.match(seviye + r" (\d+(?:\.\d+)*)\b", l)
                res.append(m.group(1) if m else l[:40])
    return res


sayim = {}  # aile -> birim sayisi (ozet tablosu)

p("## Matrisin kaynak envanteri ve kapsama denetimi")
p()
p("| Aile | Beklenen | Bulunan | Eksik | Fazla | Yinelenen |")
p("|---|---|---|---|---|---|")

# ---------------------------------------------------------------- 02
d02 = load("02")
rules, idrows = [], []
for n in range(3, 14):
    for i, l in bolum(d02, n):
        m = re.match(r"\*\*(\d+\.\d+\.\d+)[ . ]", l) or re.match(r"\*\*(\d+\.\d+\.\d+)\*\*", l)
        if m:
            rules.append(m.group(1))
        m2 = re.match(r"\| ((?:Z|L|P|B|F|H)-\d+) \|", l)
        if m2:
            idrows.append(m2.group(1))
sayim["02 kural (kalin numarali, §3-§13)"] = len(rules)
denetle("02 kural (n.m.k; her alt bolumde 1..max ardisik)", numsira(rules), rules)
fam = Counter(re.match(r"[A-Z]", x).group(0) for x in idrows)
sayim["02 kimlikli satir (Z, L, P, B, F, H)"] = len(idrows)
exp_id = set()
for x in idrows:
    pass
mx = {}
for x in idrows:
    a, b = x.split("-")
    mx[a] = max(mx.get(a, 0), int(b))
# kimlikli satirlar bazi numaralari bilerek birakmis olabilir; beklenen = gorulen en buyuk numaraya kadar
exp_id = {f"{a}-{k}" for a, m in mx.items() for k in range(1, m + 1)}
denetle("02 kimlikli satir (ardisik 1..max)", exp_id, idrows)
sub02 = alt_bolumler(d02, range(0, 14))
sayim["02 alt bolum (###)"] = len(sub02)
denetle("02 alt bolum (### basligi; yineleme denetimi)", set(sub02), sub02)

# ---------------------------------------------------------------- 03
d03 = load("03")
sub03 = alt_bolumler(d03, range(1, 11))
sayim["03 alt bolum (###, §1-§10)"] = len(sub03)
denetle("03 alt bolum (###, §1-§10; yineleme)", set(sub03), sub03)

# ---------------------------------------------------------------- 04
d04 = load("04")
e_tablo = []
for i, l in bolum(d04, 4):
    m = re.match(r"\| (E-\d+) \|", l)
    if m:
        e_tablo.append(m.group(1))
e_tanim = []
for n in (5, 9):
    for i, l in bolum(d04, n):
        m = re.match(r"### %d\.\d+ (E-\d+)\b" % n, l)
        if m:
            e_tanim.append(m.group(1))
sayim["04 ekran (E-nn)"] = len(e_tablo)
denetle("04 ekran: §4 envanter satiri <-> §5 ve §9 tanim basligi", set(e_tablo), e_tanim)
denetle("04 ekran: §4 envanter satiri yineleme", set(e_tablo), e_tablo)
ob = []
for i, l in bolum(d04, 2):
    m = re.match(r"#{3,4} \d+(?:\.\d+)* .*\((OB-\d+)\)", l)
    if m:
        ob.append(m.group(1))
mxo = max(int(x[3:]) for x in ob)
sayim["04 ortak bilesen (OB-nn)"] = len(ob)
denetle("04 ortak bilesen (OB-nn ardisik 1..max)", {f"OB-{k:02d}" for k in range(1, mxo + 1)}, ob)
sub04 = alt_bolumler(d04, (2, 3, 6, 7, 8))
sayim["04 alt bolum (§2, §3, §6, §7, §8; ###)"] = len(sub04)
denetle("04 alt bolum (### §2/§3/§6/§7/§8; yineleme)", set(sub04), sub04)

# ---------------------------------------------------------------- 05
d05 = load("05")
mk = []
for i, l in bolum(d05, 12):
    m = re.match(r"\| (MK-\d+) \|", l)
    if m:
        mk.append(m.group(1))
mxm = max(int(x[3:]) for x in mk)
sayim["05 mimari karar (MK-nn)"] = len(mk)
denetle("05 MK karari (§12 tablosu; ardisik 1..max)", {f"MK-{k:02d}" for k in range(1, mxm + 1)}, mk)
sub05 = alt_bolumler(d05, range(3, 12))
sayim["05 alt bolum (###, §3-§11)"] = len(sub05)
denetle("05 alt bolum (### §3-§11; yineleme)", set(sub05), sub05)

# ---------------------------------------------------------------- 06
d06 = load("06")
ent_tablo = []
for i, l in bolum(d06, 3):
    m = re.match(r"\| (\d+) \| ([A-Z][A-Za-z0-9]+) \|", l)
    if m:
        ent_tablo.append(m.group(2))
ent_tanim = []
for i, l in bolum(d06, 4):
    m = re.match(r"### 4\.\d+ (.*)$", l)
    if m:
        # baslik tek entity adi ya da gruplu baslik (parantez icinde adlar) olabilir
        ent_tanim += [t for t in re.findall(r"[A-Z][A-Za-z0-9]+", m.group(1)) if t in set(ent_tablo)]
    # alt entity: ust entity'nin bolumunde kalin adla (`**Ad** — ...`) tanimlanir
    m2 = re.match(r"\*\*([A-Z][A-Za-z0-9]+)\*\* — ", l)
    if m2 and m2.group(1) in set(ent_tablo) and m2.group(1) not in ent_tanim:
        ent_tanim.append(m2.group(1))
sayim["06 entity"] = len(ent_tablo)
denetle("06 entity: §3.1 envanter satiri <-> §4 tanim basligi", set(ent_tablo), ent_tanim)
denetle("06 entity: §3.1 envanter satiri yineleme", set(ent_tablo), ent_tablo)
enum_b = []
for i, l in bolum(d06, 5):
    m = re.match(r"### (5\.\d+(?:\.\d+)*)\b", l)
    if m:
        enum_b.append(m.group(1))
sayim["06 enum alt bolumu (§5 ###)"] = len(enum_b)
denetle("06 enum alt bolumu (§5 ###; yineleme)", set(enum_b), enum_b)
sub06 = alt_bolumler(d06, (2, 6, 7, 8, 9))
sayim["06 alt bolum (§2, §6, §7, §8, §9; ###)"] = len(sub06)
denetle("06 alt bolum (### §2/§6/§7/§8/§9; yineleme)", set(sub06), sub06)

# ---------------------------------------------------------------- 07
d07 = load("07")
uc_tablo = []
for i, l in bolum(d07, 3):
    m = re.match(r"\| (UC-\d+) \|", l)
    if m:
        uc_tablo.append(m.group(1))
uc_tanim = []
for n in (4, 9):
    for i, l in bolum(d07, n):
        if re.match(r"\*\*%d\.\d+\.\d+ `UC-\d+`" % n, l):
            # bir tanim basligi birden cok ucu tasiyabilir ("`UC-07` ... ve `UC-08` ... — ad")
            uc_tanim += re.findall(r"`(UC-\d+)`", l.split(" — ")[0])
sayim["07 uc (UC-nn)"] = len(uc_tablo)
denetle("07 uc: §3.2 envanter satiri <-> §4 ve §9 tanimi", set(uc_tablo), uc_tanim)
denetle("07 uc: §3.2 envanter satiri yineleme", set(uc_tablo), uc_tablo)
sub07 = alt_bolumler(d07, (2, 5, 6, 7, 8))
sayim["07 alt bolum (§2, §5, §6, §7, §8; ###)"] = len(sub07)
denetle("07 alt bolum (### §2/§5/§6/§7/§8; yineleme)", set(sub07), sub07)

# ---------------------------------------------------------------- 08
d08 = load("08")
ent_t = []
for i, l in bolum(d08, 1):
    m = re.match(r"\| (2\.\d+) \|", l)
    if m:
        ent_t.append(m.group(1))
ent_d = []
for i, l in bolum(d08, 2):
    m = re.match(r"### (2\.\d+) (.*)$", l)
    if m:
        if m.group(2).startswith("Çağrısı olmayan"):
            cagrisiz = m.group(1)  # entegrasyon degil; ayri birim
        else:
            ent_d.append(m.group(1))
sayim["08 entegrasyon (2.x)"] = len(ent_t)
sayim["08 cagrisi olmayan dis baglanti (§2.4)"] = 1
denetle("08 entegrasyon: §1 envanter satiri <-> §2 tanim basligi", set(ent_t), ent_d)
sub08 = alt_bolumler(d08, (2,), "####") + alt_bolumler(d08, (3, 4, 5))
sayim["08 alt bolum (§2 ####, §3-§5 ###)"] = len(sub08)
denetle("08 alt bolum (§2 #### + §3-§5 ###; yineleme)", set(sub08), sub08)

# ---------------------------------------------------------------- 09
d09 = load("09")
b09 = [m.group(1) for l in d09 for m in [re.match(r"## (\d+)\. ", l)] if m]
sayim["09 bolum (§1-§25)"] = len(b09)
denetle("09 bolum (§1-§25 ardisik)", {str(k) for k in range(1, 26)}, b09)
park09 = [l for l in d09[:140] if l.startswith("> - **K-")]
sayim["09 park maddesi (Aşama 1-8 bloklari)"] = len(park09)

# ---------------------------------------------------------------- 10
d10 = load("10")
kp = []
for i, l in bolum(d10, 2):
    m = re.match(r"\| (KP-\d+) \|", l)
    if m:
        kp.append(m.group(1))
mxk = max(int(x[3:]) for x in kp)
sayim["10 kapsam dahili satir (KP-nn)"] = len(kp)
denetle("10 KP satiri (§2; ardisik 1..max)", {f"KP-{k}" for k in range(1, mxk + 1)}, kp)
dis = [re.match(r"\| (KD-\d+) \|", l).group(1) for i, l in bolum(d10, 3) if re.match(r"\| KD-\d+ \|", l)]
sayim["10 kapsam disi satir (KD-nn)"] = len(dis)
denetle("10 KD satiri (§3; ardisik 1..max)", {f"KD-{k}" for k in range(1, max(int(x[3:]) for x in dis) + 1)}, dis)
sub10 = alt_bolumler(d10, (1, 3, 4))
sayim["10 alt bolum (§1, §3, §4; ###)"] = len(sub10)

# ---------------------------------------------------------------- kalip disi icerik (bilgi)
p()
p("### Kalip disi icerik (birim degil; alt bolum satiriyla karsilanir, sayim bilgi)")
p()
p("| Dokuman | Liste maddesi (`- `) | Kimliksiz tablo satiri |")
p("|---|---|---|")
for ad, ln, secs in (("02", d02, range(3, 14)), ("03", d03, range(1, 11))):
    lm = tb = 0
    for n in secs:
        for i, l in bolum(ln, n):
            if re.match(r"- ", l):
                lm += 1
            elif l.startswith("| ") and not re.match(r"\| (?:[A-Z]{1,3}-\d+|\d+|#|---|:?-)", l) and not l.startswith("|---"):
                tb += 1
    p(f"| {ad} | {lm} | {tb} |")

# ---------------------------------------------------------------- kapali liste
p()
p("### Gövdedeki \"kapali liste\" ifadeleri (kaynagin kendi tamlik kurali; checklist 2)")
p()
kl = {}
for ad, ln in (("02", d02), ("03", d03), ("04", d04), ("05", d05), ("06", d06), ("07", d07), ("08", d08), ("10", d10)):
    kl[ad] = sum(1 for l in ln if re.search(r"(?i)kapalı liste", l) and not l.startswith(">"))
p("| " + " | ".join(kl) + " |")
p("|" + "---|" * len(kl))
p("| " + " | ".join(str(v) for v in kl.values()) + " |")

# ---------------------------------------------------------------- 02 bolum bazinda (oturum bolumu icin)
p()
p("### 02 birimleri bolum bazinda (kural + kimlikli satir + alt bolum)")
p()
p("| `02` bolumu | Kural | Kimlikli satir | Alt bolum | Toplam |")
p("|---|---|---|---|---|")
tot02 = {}
for n in range(0, 14):
    ks = ik = 0
    for i, l in bolum(d02, n):
        if re.match(r"\*\*\d+\.\d+\.\d+[ . *]", l):
            ks += 1
        if re.match(r"\| (?:Z|L|P|B|F|H)-\d+ \|", l):
            ik += 1
    ab = len(alt_bolumler(d02, (n,)))
    tot02[n] = ks + ik + ab
    p(f"| §{n} | {ks} | {ik} | {ab} | {ks + ik + ab} |")

# 09'un kendi park maddeleri matris birimi degildir (09'a girmis devirlerdir): bilgi
sayim.pop("09 park maddesi (Aşama 1-8 bloklari)", None)

# ---------------------------------------------------------------- ozet
p()
p("### Birim sayilari (ozet)")
p()
p("| Aile | Birim |")
p("|---|---|")
for k, v in sayim.items():
    p(f"| {k} | {v} |")
p()
p(f"Birim toplami (devir satirlari haric; devir 19 satir = 9 K + 5 park + 4 govde + D-01, ayrica `11_devir_esle.py` ile): {sum(sayim.values())}")
p(f"Toplam sorun (eksik + fazla + yinelenen): {problems}")
if out_path:
    Path(out_path).write_text("\n".join(out) + "\n", encoding="utf-8")
sys.exit(1 if problems else 0)
