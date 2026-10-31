"""Asama 9 -- izlenebilirlik matrisinin kapsama denetimi
(checklists/document-stage.md 2; karar kaydi 22.2, K-1404; alt bolum haritasi K-1412).

`11 7.1.n` satirlarinin kaynak kimliklerini kaynak dokumanlardan cikarilan beklenen
kumeyle karsilastirir; eksik, fazla ve yinelenen sayar. Envanter betigi
(11_matris_envanter.py) kaynak ailelerin kendi sayimini yapar; bu betik matrisin
o sayimi eksiksiz tasidigini denetler.

Denetlenen aileler DOLU listesindedir; matris oturumu kendi ailesini listeye ekler:
  O1: 06, 08 (7.1.1, 7.1.3) ... O2: 07 ... O3: 04, 03 (7.1.4) ... O4a: 02 §1-§3 ... O4b 1/2: 02 §4-§8 (KAPSAM_02) ... O4b 2/2: 02 §9-§13 ... O5: 05, 09, 10, devir.

Ek denetimler:
  - her satirin Task hucresi aday kimlik listesi ("T107" ya da "T402, T311") ya da "-" olur;
  - kullanilan her aday kimlik 11 7'deki aday task tablosunda tanimlidir (K-1407 faz araliklari);
  - geri yon (O6 1/2): her aday task >=1 kaynak; 11 7.1 ile 7.2 iki yonlu esit (11_geri_yon.py);
    K-1409 boyut sinirlari asilmaz;
  - "Implement edildi" ve "Kanit" hucreleri bu asamada bostur (gate'te dolar).

Kullanim (depo kokunden): python -I scripts/doc-scans/11_matris_kapsama.py . [cikti_dosyasi]
Cikis kodu: eksik, fazla, yinelenen ya da bicim sorunu sifirdan buyukse 1.
"""
import re
import sys
from collections import Counter
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

root = Path(sys.argv[1] if len(sys.argv) > 1 else ".")
out_path = sys.argv[2] if len(sys.argv) > 2 else None

# Matris oturumu kendi ailesini ekler (alt bolum haritasi: 11 7).
DOLU = ["06", "07", "08", "04", "03", "02", "05", "09", "10", "devir"]
# Ailenin yerlestigi alt bolum (K-1404, K-1412).
ALT_BOLUM = {"06": "7.1.1", "07": "7.1.2", "08": "7.1.3", "04": "7.1.4", "03": "7.1.4",
             "02": "7.1.5", "10": "7.1.5", "05": "7.1.5", "09": "7.1.5", "devir": "7.1.5"}

out = []


def p(s=""):
    out.append(s)
    print(s)


def load(prefix):
    f = sorted((root / "Docs").glob(prefix + "_*.md"))[0]
    return f.read_text(encoding="utf-8").split("\n")


def bolum(lines, n):
    """`## n.` basligindan sonraki `## ` basligina kadar (satir_no, metin)."""
    res, inside = [], False
    for i, l in enumerate(lines, 1):
        if l.startswith("## "):
            inside = re.match(r"## %s\. " % re.escape(str(n)), l) is not None
            continue
        if inside:
            res.append((i, l))
    return res


def alt_bolumler(lines, nums, seviye="###"):
    res = []
    for n in nums:
        for _, l in bolum(lines, n):
            if l.startswith(seviye + " "):
                m = re.match(seviye + r" (\d+(?:\.\d+)*)\b", l)
                if m:
                    res.append(m.group(1))
    return res


# ------------------------------------------------------------ beklenen kumeler
def beklenen_06():
    d = load("06")
    exp = []
    for _, l in bolum(d, 3):
        m = re.match(r"\| (\d+) \| ([A-Z][A-Za-z0-9]+) \|.*\| (§4\.\d+) \|\s*$", l)
        if m:
            exp.append("06:%s %s" % (m.group(3), m.group(2)))
    n_ent = len(exp)
    enum_b = []
    for _, l in bolum(d, 5):
        m = re.match(r"### (5\.\d+(?:\.\d+)*)\b", l)
        if m:
            enum_b.append("06:§" + m.group(1))
    sub = ["06:§" + x for x in alt_bolumler(d, (2, 6, 7, 8, 9))]
    return exp + enum_b + sub, {"entity": n_ent, "enum": len(enum_b), "alt bolum": len(sub)}


def beklenen_08():
    d = load("08")
    exp = []
    ent = 0
    for _, l in bolum(d, 2):
        m = re.match(r"### (2\.\d+) (.*)$", l)
        if m:
            exp.append("08:§" + m.group(1))
            ent += 1
    sub = alt_bolumler(d, (2,), "####") + alt_bolumler(d, (3, 4, 5))
    exp += ["08:§" + x for x in sub]
    return exp, {"entegrasyon (2.x, cagrisiz dahil)": ent, "alt bolum": len(sub)}


def beklenen_07():
    d = load("07")
    ucs = []
    for _, l in bolum(d, 3):
        m = re.match(r"\| (UC-\d+) \|", l)
        if m:
            ucs.append("07:" + m.group(1))
    sub = ["07:§" + x for x in alt_bolumler(d, (2, 5, 6, 7, 8))]
    return ucs + sub, {"uc": len(ucs), "alt bolum": len(sub)}


def beklenen_04():
    d = load("04")
    ekran = []
    for _, l in bolum(d, 4):
        m = re.match(r"\| (E-\d+) \|", l)
        if m:
            ekran.append("04:" + m.group(1))
    ob = []
    for _, l in bolum(d, 2):
        m = re.match(r"#{3,4} \d+(?:\.\d+)* .*\((OB-\d+)\)", l)
        if m:
            ob.append("04:" + m.group(1))
    sub = ["04:§" + x for x in alt_bolumler(d, (2, 3, 6, 7, 8))]
    return ekran + ob + sub, {"ekran": len(ekran), "ortak bilesen": len(ob), "alt bolum": len(sub)}


def beklenen_03():
    d = load("03")
    sub = ["03:§" + x for x in alt_bolumler(d, range(1, 11))]
    return sub, {"alt bolum": len(sub)}


# O4a: 02 §1-§3 (kural 257 + alt bolum 40); O4b 1/2: §4-§8 (kural 101, kimlikli satir 56, alt bolum 36); O4b 2/2 §9-§13'u ekler:
# ekler: KAPSAM_02'ye bolum numaralarini ve kimlikli satirlari katar.
KAPSAM_02 = tuple(range(1, 14))  # O4b 2/2: §9-§13 eklendi (§1-§13)


def beklenen_02():
    d = load("02")
    kural, kimlikli = [], []
    for n in KAPSAM_02:
        if n < 3:
            continue
        for _, l in bolum(d, n):
            m = re.match(r"\*\*(\d+\.\d+\.\d+)[\s .*]", l)
            if m:
                kural.append("02:§" + m.group(1))
            m2 = re.match(r"\| ((?:Z|L|P|B|F|H)-\d+) \|", l)
            if m2:
                kimlikli.append("02:" + m2.group(1))
    sub = ["02:§" + x for x in alt_bolumler(d, KAPSAM_02)]
    return kural + kimlikli + sub, {"kural": len(kural), "kimlikli satir": len(kimlikli), "alt bolum": len(sub)}


def beklenen_05():
    """O5 1/2: 05 -- MK-nn karari (§12 tablosu) ve ### alt bolum (§3-§11; `### 6.x` kimligi `05:§6`)."""
    d = load("05")
    mk = []
    for _, l in bolum(d, 12):
        m = re.match(r"\| (MK-\d+) \|", l)
        if m:
            mk.append("05:" + m.group(1))
    sub = ["05:§" + x for x in alt_bolumler(d, range(3, 12))]
    return mk + sub, {"MK karari": len(mk), "alt bolum": len(sub)}


def beklenen_09():
    """O5 2/2: 09 -- `## n.` bolumu (n = 1..25); kimlik `09:§n`."""
    d = load("09")
    b = []
    for l in d:
        m = re.match(r"## (\d+)\. ", l)
        if m:
            b.append("09:§" + m.group(1))
    return b, {"bolum": len(b)}


def beklenen_10():
    """O5 2/2: 10 -- KP satiri (§2), KD satiri (§3), alt bolum (§1, §3, §4 `###`; yalniz §4.1 ve §4.2)."""
    d = load("10")
    kp = ["10:" + m.group(1) for _, l in bolum(d, 2) for m in [re.match(r"\| (KP-\d+) \|", l)] if m]
    kd = ["10:" + m.group(1) for _, l in bolum(d, 3) for m in [re.match(r"\| (KD-\d+) \|", l)] if m]
    sub = ["10:§" + x for x in alt_bolumler(d, (1, 3, 4))]
    return kp + kd + sub, {"KP": len(kp), "KD (negatif denetim)": len(kd), "alt bolum": len(sub)}


# 11_devir_esle.py A-F tablosu (11 22.1): 9 karar + 5 park maddesi + 4 govde anmasi + D-01 = 19.
DEVIR = (["devir:K-%d" % n for n in (423, 424, 1302, 1354, 1355, 1356, 1357, 1358, 1391)]
         + ["devir:P8.%d" % n for n in range(1, 6)]
         + ["devir:04-1948", "devir:09-117", "devir:09-2249", "devir:12-177", "devir:D-01"])


def beklenen_devir():
    return list(DEVIR), {"karar (A)": 9, "park maddesi (B)": 5, "govde anmasi (F)": 4, "D-01": 1}


BEKLENEN = {"06": beklenen_06, "07": beklenen_07, "08": beklenen_08, "04": beklenen_04, "03": beklenen_03,
            "02": beklenen_02, "05": beklenen_05, "09": beklenen_09, "10": beklenen_10, "devir": beklenen_devir}

# ------------------------------------------------------------ matris satirlari
plan_f = sorted((root / "Docs").glob("11_*.md"))[0]
plan = plan_f.read_text(encoding="utf-8").split("\n")



def kimlik(hucre):
    """`06:§4.17 Order` -> '06:§4.17 Order'; `06:§2.1 Baslik` -> '06:§2.1';
    `07 UC-12 ...` -> '07:UC-12'."""
    h = hucre.strip().strip("`")
    me = re.match(r"^(04):((?:E|OB)-\d+)\b", h)
    if me:
        return "%s:%s" % (me.group(1), me.group(2))
    mk = re.match(r"^(02):((?:Z|L|P|B|F|H)-\d+)\b", h)
    if mk:
        return "%s:%s" % (mk.group(1), mk.group(2))
    mm = re.match(r"^(05):(MK-\d+)\b", h)
    if mm:
        return "%s:%s" % (mm.group(1), mm.group(2))
    mp = re.match(r"^(10):((?:KP|KD)-\d+)\b", h)
    if mp:
        return "%s:%s" % (mp.group(1), mp.group(2))
    mv = re.match(r"^(devir):(K-\d+|P8\.\d|D-01|\d\d-\d+)\b", h)
    if mv:
        return "%s:%s" % (mv.group(1), mv.group(2))
    mu = re.match(r"^(07) (UC-\d+)\b", h)
    if mu:
        return "%s:%s" % (mu.group(1), mu.group(2))
    m = re.match(r"^(\d\d):(§\d+(?:\.\d+)*)(?: (.*))?$", h)
    if not m:
        return None
    fam, sec, rest = m.group(1), m.group(2), m.group(3)
    if fam == "06" and sec.startswith("§4.") and rest:
        ad = re.match(r"[A-Z][A-Za-z0-9]+", rest)
        if ad:
            return "%s:%s %s" % (fam, sec, ad.group(0))
    return "%s:%s" % (fam, sec)


satirlar = []  # (alt_bolum, kimlik, task, impl, kanit, ham)
cur = None
for l in plan:
    m = re.match(r"#### (7\.1\.\d)\b", l)
    if m:
        cur = m.group(1)
        continue
    if l.startswith("## ") or (l.startswith("### ") and not l.startswith("#### ")):
        cur = None
    if cur and l.startswith("|"):
        c = [x.strip() for x in l.strip().strip("|").split("|")]
        if len(c) != 4:
            continue
        k = kimlik(c[0])
        if k:
            satirlar.append((cur, k, c[1], c[2], c[3], l))

# aday task tablosu: "| T107 | ad | ..." satirlari 11 7 icinde
aday = set()
in7 = False
for l in plan:
    if l.startswith("## "):
        in7 = l.startswith("## 7.")
    if in7:
        m = re.match(r"\| (T\d+[a-z]?) \|", l)
        if m:
            aday.add(m.group(1))

TASK_RE = re.compile(r"^T\d+[a-z]?(?:\.\d+)?$")
FAZ = [(1, 99, "F0"), (101, 199, "F1"), (201, 299, "F2"), (301, 399, "F3"),
       (401, 499, "F4"), (501, 599, "F5"), (601, 699, "F6")]


def faz_of(t):
    n = int(re.match(r"T(\d+)", t).group(1))
    for a, b, f in FAZ:
        if a <= n <= b:
            return f
    return None


problems = 0
p("## Matris kapsama denetimi (11 7.1)")
p()
p("| Aile | Alt bolum | Beklenen | Satir | Eksik | Fazla | Yinelenen |")
p("|---|---|---|---|---|---|---|")

kullanilan = Counter()
faz_say = Counter()
for fam in DOLU:
    exp, kirilim = BEKLENEN[fam]()
    exp_set = set(exp)
    exp_dup = [k for k, v in Counter(exp).items() if v > 1]
    problems += len(exp_dup)
    rows = [s for s in satirlar if s[1].startswith(fam + ":")]
    yanlis_yer = [s[1] for s in rows if s[0] != ALT_BOLUM[fam]]
    c = Counter(s[1] for s in rows)
    eksik = sorted(exp_set - set(c))
    fazla = sorted(set(c) - exp_set)
    yin = sorted(k for k, v in c.items() if v > 1)
    problems += len(eksik) + len(fazla) + len(yin) + len(yanlis_yer)
    p("| %s | %s | %d | %d | %d | %d | %d |" % (fam, ALT_BOLUM[fam], len(exp_set), len(rows),
                                                  len(eksik), len(fazla), len(yin)))
    p("    beklenen kirilimi: " + ", ".join("%s %d" % kv for kv in kirilim.items()))
    for ad, v in (("eksik", eksik), ("fazla", fazla), ("yinelenen", yin),
                  ("yanlis alt bolumde", yanlis_yer), ("kaynakta yinelenen", exp_dup)):
        if v:
            p("    %s: %s" % (ad, v[:20]))
    for s in rows:
        task = s[2]
        if task.startswith("—") or task.startswith("-"):
            continue
        ids = [x.strip() for x in task.split(",")]
        for t in ids:
            if not TASK_RE.match(t):
                problems += 1
                p("    task bicimi bozuk: %s -> %r" % (s[1], task))
                continue
            kullanilan[t] += 1
            faz_say[faz_of(t) or "?"] += 1
            if faz_of(t) is None:
                problems += 1
                p("    task faz araligi disinda: %s -> %s" % (s[1], t))
            if t not in aday:
                problems += 1
                p("    aday listesinde tanimsiz task: %s -> %s" % (s[1], t))
        if s[3] or s[4]:
            problems += 1
            p("    implement/kanit bos olmali: %s" % s[1])

# doldurulmamis aileler: satiri varsa uyar (DOLU listesine eklenmemis)
diger = sorted({s[1].split(":")[0] for s in satirlar} - set(DOLU))
if diger:
    p()
    p("Uyari: DOLU listesinde olmayan ailelerin satiri var: %s (betige ekle)" % diger)

# ------------------------------------------------------------ geri yon (O6 1/2): 11 7.2
import importlib.util
_spec = importlib.util.spec_from_file_location("geri_yon", str(Path(__file__).resolve().parent / "11_geri_yon.py"))
gy = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(gy)

ileri_cift = {(k, t) for _, k, tasks in gy.matris_satirlari(plan) for t in tasks}
geri_cift, geri_task, geri_bicim = set(), [], 0
for l in gy.oku_7_2(plan):
    c = [x.strip() for x in l.strip().strip("|").split("|")]
    if len(c) != 3:
        geri_bicim += 1
        continue
    geri_task.append(c[0])
    for gr in c[1].split("; "):
        m = re.match(r"^`(\w+)` (.*)$", gr)
        if not m:
            geri_bicim += 1
            continue
        for kid in m.group(2).split(", "):
            geri_cift.add(("%s:%s" % (m.group(1), kid), c[0]))
aday_tum = gy.aday_tablosu(plan)
kaynaksiz = sorted(t for t in aday_tum if not any(t == x[1] for x in ileri_cift))
geri_yin = sorted(t for t, v in Counter(geri_task).items() if v > 1)
p()
p("## Geri yon denetimi (11 7.2, O6 1/2)")
p("7.1 cift (kaynak, task): %d; 7.2 cift: %d; yalniz 7.1'de: %d; yalniz 7.2'de: %d" % (
    len(ileri_cift), len(geri_cift), len(ileri_cift - geri_cift), len(geri_cift - ileri_cift)))
p("7.2 satiri %d, aday %d; 7.2'de olmayan aday: %s; 7.2'de aday olmayan: %s; yinelenen: %s; bicim sorunu: %d" % (
    len(geri_task), len(aday_tum), sorted(set(aday_tum) - set(geri_task))[:10] or "yok",
    sorted(set(geri_task) - set(aday_tum))[:10] or "yok", geri_yin or "yok", geri_bicim))
p("Kaynagi olmayan aday task: %s" % (kaynaksiz or "yok"))
tablo_esit = gy.tablo_satirlari(plan) == gy.oku_7_2(plan)
p("7.2, 7.1'den betikle turetilenle birebir esit: %s" % tablo_esit)
asim = gy.boyut_asimlari(plan)
p("K-1409 boyut asimi (entity<=8, uc<=8, ekran<=2, OB<=4): %s" % (asim or "yok"))
fz = Counter(v[0] for v in aday_tum.values())
p("Faz basina task: " + ", ".join("%s %d" % (f, fz[f]) for f in sorted(fz)))
problems += (len(ileri_cift ^ geri_cift) + len(kaynaksiz) + len(geri_yin) + geri_bicim + (0 if tablo_esit else 1)
             + len(set(aday_tum) ^ set(geri_task)) + len(asim))

kullanilmayan = sorted(aday - set(kullanilan))
p()
p("Aday task: %d tanimli, %d kullanilan; kullanilmayan: %s" % (len(aday), len(kullanilan), kullanilmayan or "yok"))
p("Faz dagilimi (satir x task): " + ", ".join("%s %d" % (f, faz_say[f]) for f in sorted(faz_say)))
p("Toplam sorun (eksik + fazla + yinelenen + bicim): %d" % problems)

if out_path:
    Path(out_path).write_text("\n".join(out) + "\n", encoding="utf-8")
sys.exit(1 if problems else 0)
