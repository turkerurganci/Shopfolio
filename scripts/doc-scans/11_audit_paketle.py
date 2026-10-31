"""Asama 9 -- `11_IMPLEMENTATION_PLAN.md` kalite dongusu audit'i: mercek paketleyicisi (KD-audit, Parca A).

Mercekler (her biri dar, satir numarali paket; en cok 85 KB):
  KA  kapsama/dogruluk : task blogu + 7.1'de bagli kaynak birimlerin KAYNAK METNI (+ aday tablo satiri, K satirlari)
  SB  sira ve bagimlilik : faz dilimi, sikistirilmis task gorunumu + Yurutme sirasi
  KD  kapsam disi (negatif denetim) : sikistirilmis task gorunumu + `10` KD/KP satirlari
  FK  faz kapilari : 3 faz tablosu, 6.1/6.2, `00` kapi tanimi, aday tablo
  IT  ic tutarlilik + olculebilirlik : modul gruplu TAM task bloklari (task <-> task celiskisi, sinanabilirlik)
  IP  ust bolumler/prose : 1-4, 3.1, 6, 7 giris notlari, 8, 9 (OA'nin dilimlemedigi yerler icin)
  TE  terim/ad : betik adayi (blok ve aday tablodaki PascalCase / uc / ekran adlari ve kaynakta varligi)
  AT  atif : betik (her `NN §x.y` hedefi var mi) -> script_AT.txt

Kullanim: python -I scripts/doc-scans/11_audit_paketle.py <cikti_dizini> [--doc yol] [--sinama]
  --doc     denetlenecek 11 dosyasi (varsayilan: Docs/11_IMPLEMENTATION_PLAN.md)
  --sinama  yalniz tanilari yazdirir, paket yazmaz (iki yonlu sinama icin)
"""
import re
import sys
from collections import Counter, defaultdict
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

ROOT = Path(r"C:\projects\Shopfolio")
ARGS = [a for a in sys.argv[1:]]
SINAMA = "--sinama" in ARGS
if "--doc" in ARGS:
    i = ARGS.index("--doc")
    DOC = Path(ARGS[i + 1])
    del ARGS[i:i + 2]
else:
    DOC = ROOT / "Docs" / "11_IMPLEMENTATION_PLAN.md"
ARGS = [a for a in ARGS if not a.startswith("--")]
OUT = Path(ARGS[0]) if ARGS else Path("kd-audit-paket")
CAP_PACKET = 85_000
CAP_UNIT = 1_100       # kaynak birimi basina bayt (KA)
CAP_K = 800            # K satiri basina bayt

LINES = DOC.read_text(encoding="utf-8").split("\n")
N = len(LINES)
TIDRX = r"T\d{2,3}[a-z]?(?:\.\d+)?"
TID = re.compile(r"\b" + TIDRX + r"\b")


def clip(s, n):
    b = s.encode("utf-8")
    if len(b) <= n:
        return s
    return b[:n].decode("utf-8", "ignore") + " [...kesildi]"


def cells(line):
    return [c.strip() for c in line.strip().strip("|").split(" | ")]


# ------------------------------------------------------------------ 11 yapisi
def find_line(rx, start=0):
    r = re.compile(rx)
    for i in range(start, N):
        if r.search(LINES[i]):
            return i
    return None


H5 = find_line(r"^## 5\. Task listesi")
H6 = find_line(r"^## 6\. Faz ge")
H7 = find_line(r"^## 7\. Traceability")
H71 = find_line(r"^### 7\.1 ")
H72 = find_line(r"^### 7\.2 ")
H8 = find_line(r"^## 8\. ")
H3 = find_line(r"^## 3\. Faz tan")
if None in (H5, H6, H7, H71, H72, H8, H3):
    raise SystemExit("11 bolum basliklari bulunamadi (bozuk girdi?)")

# faz -> task bloklari
BLOCKS = {}   # tid -> dict(start,end,faz,text_lines)
FAZ_BASLIK = []
cur_faz = None
starts = []
for i in range(H5, H6):
    l = LINES[i]
    m = re.match(r"^### (F\d) ", l)
    if m:
        cur_faz = m.group(1)
        FAZ_BASLIK.append((cur_faz, i))
    m = re.match(r"^#### (" + TIDRX + r"): ?(.*)$", l)
    if m:
        starts.append((i, m.group(1), m.group(2), cur_faz))
for k, (i, tid, ad, faz) in enumerate(starts):
    end = starts[k + 1][0] if k + 1 < len(starts) else H6
    # blok bir sonraki ### baslikta biter
    for j in range(i + 1, end):
        if LINES[j].startswith("### ") or LINES[j].startswith("## "):
            end = j
            break
    if tid in BLOCKS:
        print("UYARI: yinelenen blok", tid)
    BLOCKS[tid] = dict(start=i, end=end, faz=faz, ad=ad)


def block_lines(tid):
    b = BLOCKS[tid]
    return [(n + 1, LINES[n]) for n in range(b["start"], b["end"]) if LINES[n].strip() != "" or True]


def fmt_lines(pairs, prefix="11"):
    return "\n".join("%s:%d| %s" % (prefix, n, t) for n, t in pairs)


def block_parse(tid):
    b = BLOCKS[tid]
    seg = LINES[b["start"]:b["end"]]
    d = dict(dep="", doc="", crit=[], test="", kk=[])
    mode = None
    for l in seg[1:]:
        if l.startswith("- **Bağımlılık:**"):
            d["dep"] = l.split(":**", 1)[1].strip(); mode = None
        elif l.startswith("- **Dokümanlar:**"):
            d["doc"] = l.split(":**", 1)[1].strip(); mode = None
        elif l.startswith("- **Kabul kriterleri:**"):
            mode = "crit"
        elif l.startswith("- **Test beklentisi:**"):
            d["test"] = l.split(":**", 1)[1].strip(); mode = None
        elif l.startswith("- **Doğrulama kontrol listesi:**"):
            mode = "kk"
        elif mode == "crit" and l.strip():
            d["crit"].append(l.strip())
        elif mode == "kk" and l.strip():
            d["kk"].append(l.strip())
    return d


def condensed(tid, crit_cap=380):
    d = block_parse(tid)
    b = BLOCKS[tid]
    o = ["11:%d| #### %s: %s  [%s]" % (b["start"] + 1, tid, b["ad"], b["faz"])]
    o.append("   Bağımlılık: %s" % d["dep"])
    o.append("   Dokümanlar: %s" % clip(d["doc"], 400))
    for c in d["crit"]:
        o.append("   " + clip(c, crit_cap))
    o.append("   Test: %s" % d["test"])
    return "\n".join(o)


# aday tablo
ADAY = {}
for i in range(H7, H72):
    m = re.match(r"^\| (" + TIDRX + r") \| (F\d) \| ", LINES[i])
    if m:
        c = cells(LINES[i])
        ADAY[c[0]] = dict(line=i + 1, faz=c[1], ad=c[2] if len(c) > 2 else "", mod=c[3] if len(c) > 3 else "",
                          kaynak=c[4] if len(c) > 4 else "", raw=LINES[i])

# 7.1 satirlari: task -> kaynak birimleri
SRC_OF = defaultdict(list)       # tid -> [(id, baslik, satirno)]
UNITS = []                       # (id, baslik, tasks, satirno)
IDRX = re.compile(r"^(\w+):(\S+)")
for i in range(H71, H72):
    l = LINES[i]
    if not l.startswith("| ") or l.startswith("| Kaynak öğe") or l.startswith("|---"):
        continue
    c = cells(l)
    if len(c) < 2:
        continue
    if not re.match(r"^(—|" + TIDRX + r"(, " + TIDRX + r")*)$", c[1]):
        continue
    parts = c[0].split(" ", 1)
    uid = parts[0]
    title = parts[1] if len(parts) > 1 else ""
    if uid == "07" and re.match(r"^UC-\d+", title):      # `07 UC-01 GET ...` biçimi
        uid = "07:" + title.split(" ", 1)[0]
    if ":" not in uid:                                     # faz ozet tablolari vb.
        continue
    tasks = [] if c[1] == "—" else c[1].split(", ")
    UNITS.append((uid, title, tasks, i + 1))
    for t in tasks:
        SRC_OF[t].append((uid, title, i + 1))

# ------------------------------------------------------------------ kaynak dokumanlari
DOCS = {}
for p in sorted((ROOT / "Docs").glob("[0-1][0-9]_*.md")):
    DOCS[p.name[:2]] = p.read_text(encoding="utf-8").split("\n")

HEAD_RX = re.compile(r"^(#{1,6}) (.*)$")
IDX = {}   # doc -> key -> [(kind, line_idx)]
for dn, ls in DOCS.items():
    ix = defaultdict(list)
    for i, l in enumerate(ls):
        m = HEAD_RX.match(l)
        if m:
            txt = m.group(2)
            mn = re.match(r"^(?:§)?(\d+(?:\.\d+)*)\.?\s", txt + " ")
            if mn:
                ix[mn.group(1)].append(("h", i))
            for t in re.findall(r"\b(E-\d+|OB-\d+|UC-\d+|MK-\d+|KP-\d+|KD-\d+|TM\d+-\d+)\b", txt):
                ix[t].append(("h", i))
            continue
        mb = re.match(r"^\*\*(\d+(?:\.\d+)+)[ .]", l)
        if mb:
            ix[mb.group(1)].append(("b", i))
            continue
        mr = re.match(r"^\|\s*`?([A-Z]{1,3}-\d+|\d+(?:\.\d+)+)`?\s*\|", l)
        if mr:
            ix[mr.group(1)].append(("r", i))
    IDX[dn] = ix

# karar kaydi
KAYIT = {}
kp = ROOT / "Docs" / "PRODUCT_DISCOVERY_STATUS.md"
if kp.exists():
    rx = re.compile(r"^\| (K-\d+) \|")
    for n, l in enumerate(kp.read_text(encoding="utf-8").split("\n")):
        m = rx.match(l)
        if m and m.group(1) not in KAYIT:
            KAYIT[m.group(1)] = (n + 1, l)


def sect_text(dn, i, cap):
    ls = DOCS[dn]
    out = [(i + 1, ls[i])]
    for j in range(i + 1, min(len(ls), i + 400)):
        if HEAD_RX.match(ls[j]):
            break
        out.append((j + 1, ls[j]))
    txt = "\n".join("%s:%d| %s" % (dn, n, t) for n, t in out if t.strip() or True)
    return clip(txt, cap)


def para_text(dn, i, cap):
    ls = DOCS[dn]
    out = []
    for j in range(i, min(len(ls), i + 40)):
        if j > i and (ls[j].strip() == "" or HEAD_RX.match(ls[j])):
            break
        out.append("%s:%d| %s" % (dn, j + 1, ls[j]))
    return clip("\n".join(out), cap)


def resolve(uid, cap=CAP_UNIT):
    """Kaynak birimin metni. Bulunamazsa None."""
    m = IDRX.match(uid)
    if not m:
        return None
    fam, rest = m.group(1), m.group(2)
    if fam == "devir":
        if rest.startswith("K-") and rest in KAYIT:
            return "K:%d| %s" % (KAYIT[rest][0], clip(KAYIT[rest][1], cap))
        return ""   # devir satiri: baslik yeterli (gerek duyulmaz)
    key = rest.lstrip("§").rstrip(".,;")
    if fam not in IDX:
        return None
    hits = IDX[fam].get(key)
    if not hits:
        return None
    outs = []
    for kind, i in hits[:3]:
        if kind == "h":
            outs.append(sect_text(fam, i, cap))
        elif kind == "b":
            outs.append(para_text(fam, i, cap))
        else:
            outs.append(clip("%s:%d| %s" % (fam, i + 1, DOCS[fam][i]), cap))
    return clip("\n".join(outs), cap)


def kler(tid, n=2):
    ks = []
    a = ADAY.get(tid)
    if not a:
        return ""
    for k in re.findall(r"K-\d+", a["kaynak"] + " " + a["ad"]):
        if k not in ks and k in KAYIT:
            ks.append(k)
    return "\n".join("K:%d| %s" % (KAYIT[k][0], clip(KAYIT[k][1], CAP_K)) for k in ks[:n])


# ------------------------------------------------------------------ tanilar
tani = []
tani.append("11 satir: %d; blok: %d; aday: %d; 7.1 birim: %d; 7.1'de task bagi olan task: %d; K kaydi: %d"
            % (N, len(BLOCKS), len(ADAY), len(UNITS), len(SRC_OF), len(KAYIT)))
only_blk = sorted(set(BLOCKS) - set(ADAY))
only_aday = sorted(set(ADAY) - set(BLOCKS))
no_src = sorted(t for t in BLOCKS if t not in SRC_OF)
tani.append("blokta olup aday tabloda olmayan: %s" % only_blk)
tani.append("aday tabloda olup blokta olmayan: %s" % only_aday)
tani.append("7.1'de kaynagi olmayan blok: %s" % no_src)
tani.append("7.1'de gecen ama aday tabloda olmayan task: %s" % sorted(set(SRC_OF) - set(ADAY)))
tani.append("aday faz != blok faz: %s" % [(t, ADAY[t]["faz"], BLOCKS[t]["faz"]) for t in ADAY
                                          if t in BLOCKS and ADAY[t]["faz"] != BLOCKS[t]["faz"]])
cozulmez = Counter()
cozulen = Counter()
cozulmez_ornek = defaultdict(list)
for uid, title, tasks, ln in UNITS:
    r = resolve(uid, 200)
    fam = uid.split(":")[0]
    if r is None:
        cozulmez[fam] += 1
        if len(cozulmez_ornek[fam]) < 6:
            cozulmez_ornek[fam].append(uid)
    else:
        cozulen[fam] += 1
tani.append("kaynak cozumleme (aile: cozulen/cozulmeyen): " +
            "; ".join("%s %d/%d" % (f, cozulen[f], cozulmez[f]) for f in sorted(set(cozulen) | set(cozulmez))))
for f, o in cozulmez_ornek.items():
    tani.append("  cozulmeyen ornek %s: %s" % (f, o))

if SINAMA:
    print("\n".join(tani))
    sys.exit(0)

OUT.mkdir(parents=True, exist_ok=True)
for old in OUT.glob("paket_*.md"):
    old.unlink()
paketler = []   # (ad, mercek, boyut, aciklama)


def yaz(ad, mercek, govde, aciklama):
    p = OUT / ("paket_%s.md" % ad)
    p.write_text(govde, encoding="utf-8", newline="\n")
    paketler.append((ad, mercek, len(govde.encode("utf-8")), aciklama))


def pack(items, header, ad_prefix, mercek, cap=CAP_PACKET, aciklama=lambda its: ""):
    """items: [(etiket, metin)] -> paketlere bol."""
    cur, size, n = [], len(header.encode("utf-8")), 1
    for et, tx in items:
        sz = len(tx.encode("utf-8")) + 2
        if cur and size + sz > cap:
            yaz("%s_%02d" % (ad_prefix, n), mercek, header + "\n\n".join(t for _, t in cur) + "\n",
                aciklama([e for e, _ in cur]))
            cur, size, n = [], len(header.encode("utf-8")), n + 1
        cur.append((et, tx)); size += sz
    if cur:
        yaz("%s_%02d" % (ad_prefix, n), mercek, header + "\n\n".join(t for _, t in cur) + "\n",
            aciklama([e for e, _ in cur]))


def aralik(etiketler):
    return "%s..%s (%d)" % (etiketler[0], etiketler[-1], len(etiketler)) if etiketler else ""


def tid_key(t):
    m = re.match(r"T(\d+)([a-z]?)", t)
    return (int(m.group(1)), m.group(2), t)


TASKS = sorted(BLOCKS, key=tid_key)
FAZSIRA = ["F0", "F1", "F2", "F3", "F4", "F5", "F6"]

# ------------------------------------------------------------------ KA
bas = ("# KA paketi -- task blogu + 7.1'de bagli kaynak birimlerin kaynak metni\n"
       "Satir onekleri: `11:<n>|` Implementation Plani, `NN:<n>|` kaynak dokuman, `K:<n>|` karar kaydi. "
       "Kaynak metin birim basina kesilmistir ([...kesildi]); kesilen yerde hukum vermek icin kaynak dosyayi grep -n / aralik ile ac.\n\n")
items = []
seen_global = {}
for t in TASKS:
    blk = fmt_lines(block_lines(t))
    parts = ["### BLOK %s\n%s" % (t, blk)]
    a = ADAY.get(t)
    if a:
        parts.append("ADAY SATIRI 11:%d| %s" % (a["line"], clip(a["raw"], 700)))
    kk = kler(t)
    if kk:
        parts.append("ILGILI K SATIRLARI\n" + kk)
    su = ["7.1 BAGLI KAYNAK BIRIMLERI (%s)" % t]
    for uid, title, ln in SRC_OF.get(t, []):
        r = resolve(uid)
        if r is None:
            su.append("- %s %s  (11:%d)  [KAYNAK METIN BULUNAMADI]" % (uid, title, ln))
        elif r == "":
            su.append("- %s %s  (11:%d)  [devir satiri: ayri metni yok]" % (uid, title, ln))
        else:
            su.append("- %s %s  (11:%d)\n%s" % (uid, title, ln, r))
    parts.append("\n".join(su))
    items.append((t, "\n".join(parts)))
# oncelik bolgeleri: okunmadan yazilan yerler
ONCELIK = re.compile(r"^(02:(§[4-9]|§1[0-3]|Z-|L-|B-|F-|P-|H-)|03:|04:(E-|§)|05:|07:|09:|10:)")


def oncelik_skor(t):
    return sum(1 for uid, _, _ in SRC_OF.get(t, []) if ONCELIK.match(uid))


# faz sirasi koru; paketler sonra puana gore siralanir
ka_ilk = len(paketler)
pack(items, bas, "KA", "KA", aciklama=lambda e: aralik(e))
ka_pk = paketler[ka_ilk:]

# ------------------------------------------------------------------ SB / KD
fazlar = defaultdict(list)
for t in TASKS:
    fazlar[BLOCKS[t]["faz"]].append(t)


def yurutme(faz):
    for fz, i in FAZ_BASLIK:
        if fz == faz:
            for j in range(i + 1, min(i + 6, N)):
                if LINES[j].startswith("**Yürütme sırası:**"):
                    return "11:%d| %s" % (j + 1, LINES[j])
    return "(Yürütme sırası satırı bulunamadı)"


sb_head = ("# SB paketi -- sira ve bagimlilik (task'in kullandigi entity / uc / ekran / ayar / kayit daha SONRAKI bir task'ta mi dogar? "
           "faz ici Baglimlilik ve Yurutme sirasi tutarli mi?)\n"
           "Task'lar sikistirilmistir: ad, Bagimlilik, Dokumanlar, kabul kriterleri (kesik), test. Tam blok: 11 dosyasinda `11:<n>`.\n\n")
sb_head = sb_head.replace("Baglimlilik", "Bagimlilik")
for faz in FAZSIRA:
    if faz not in fazlar:
        continue
    its = [(t, condensed(t, 330)) for t in fazlar[faz]]
    # her paketin basina o fazin yurutme sirasi ve faz tanimi
    pack(its, sb_head + "FAZ %s -- %s\n\n" % (faz, yurutme(faz)), "SB_" + faz, "SB", aciklama=aralik)

# 10'un KD / KP satirlari
kd_rows = []
if "10" in DOCS:
    for i, l in enumerate(DOCS["10"]):
        if re.match(r"^\|\s*(KD|KP)-\d+\s*\|", l):
            kd_rows.append("10:%d| %s" % (i + 1, clip(l, 600)))
kd_only = [r for r in kd_rows if "| KD-" in r[:20]]
kd_head = ("# KD paketi -- negatif denetim: hicbir task `10`'un kapsam disi (KD) satirindaki isi yapmamali; KP satirlari kapsam ici referanstir.\n"
           "`10` KD satirlari asagidadir (her paketin basinda). Task'lar sikistirilmistir.\n\n"
           "## 10 KD satirlari\n" + "\n".join(kd_only) + "\n\n## Task'lar\n\n")
for faz in FAZSIRA:
    if faz not in fazlar:
        continue
    its = [(t, condensed(t, 330)) for t in fazlar[faz]]
    pack(its, kd_head, "KD_" + faz, "KD", aciklama=aralik)

# ------------------------------------------------------------------ IT (modul gruplu tam bloklar)
bymod = defaultdict(list)
for t in TASKS:
    bymod[ADAY.get(t, {}).get("mod", "?")].append(t)
it_head = ("# IT/OL paketi -- ic tutarlilik + olculebilirlik; ayni modulun TAM task bloklari yan yana\n"
           "Bak: (a) task <-> task celiskisi (ayni kavrama farkli kural, farkli sayi/sure/durum adi, ayni isi iki task'in sahiplenmesi), "
           "(b) blok ici celiski (kabul kriteri <-> KK <-> Test beklentisi), (c) kabul kriteri sinanabilir mi (olculebilir esik, gozlenebilir sonuc), "
           "(d) numaralama/alan eksigi. OA'nin ozet<->ayrinti taramasi ozet yerlerini kapsadi; burada BLOKLARIN kendisi okunur.\n\n")
items = []
for mod in sorted(bymod):
    for t in bymod[mod]:
        items.append(("%s/%s" % (mod, t), "### [%s] %s\n%s" % (mod, t, fmt_lines(block_lines(t)))))
pack(items, it_head, "IT", "IT", aciklama=lambda e: "%s ... %s (%d)" % (e[0], e[-1], len(e)))

# ------------------------------------------------------------------ IP (ust bolumler / prose)
ip_items = []
ranges = [("Bas ve 1-2 (satir 1-96)", 0, H3), ("3 Faz tanimlari, 3.1, 4, 5 giris (97-174)", H3, find_line(r"^### F0 ")),
          ("6 Faz gecis kapilari", H6, H7), ("7 giris ve 7.1 oncesi notlar", H7, H71),
          ("7.2 oncesi 7.1 aciklayici notlar (tablo disi satirlar)", None, None),
          ("8 Bosluk raporu ve 9 Post-MVP", H8, N)]
for ad, a, b in ranges:
    if a is None:
        pairs = [(i + 1, LINES[i]) for i in range(H71, H72) if LINES[i].strip() and not LINES[i].startswith("|")]
        pairs = [(n, clip(t, 1800)) for n, t in pairs]
    else:
        pairs = [(i + 1, clip(LINES[i], 2500)) for i in range(a, b) if LINES[i].strip()
                 and not (a == H7 and LINES[i].startswith("| T"))]
    ip_items.append((ad, "## %s\n%s" % (ad, fmt_lines(pairs))))
ip_head = ("# IP paketi -- ust bolumlerin ic tutarliligi (sayilar, faz tablosu, konvansiyonlar, kapi cumleleri, 8 ve 9)\n"
           "Bak: ayni olgunun iki yerde farkli yazilmasi (task sayilari, faz araliklari, K atiflari, K-1409 sinirlari, surum/tarih), "
           "konvansiyonun kendi iclinde celiski, 3.1 kurallari <-> 5 konvansiyonlari <-> 6 kapilar.\n\n")
pack(ip_items, ip_head, "IP", "IP", aciklama=lambda e: "; ".join(e))

# ------------------------------------------------------------------ FK
fk = ["# FK paketi -- faz gecis kapilari <-> task'lar <-> `00`'in kapi tanimi\n"
      "Bak: 6.2'deki her faz-spesifik kontrolun (a) faz ciktisini/`3` cikis urunu ile uyumu, (b) kontrolu somut yapan bir task'in varligi "
      "(ornek: 'ilk uctan uca akis testi (T329)' dogru task mi), (c) `00 E.1` ve gate-check beklentisiyle celiski, (d) kapida adi gecen "
      "OK / DV / T atiflarinin varligi ve dogrulugu, (e) faz kapisini kapatan bir task'in yanlis fazda olmasi.\n"]
fk.append("## 3 Faz tablosu\n" + fmt_lines([(i + 1, clip(LINES[i], 1500)) for i in range(H3, find_line(r"^### 3\.1 "))]))
fk.append("## 3.1 Faz bagimliliklari\n" + fmt_lines([(i + 1, clip(LINES[i], 1800)) for i in range(find_line(r"^### 3\.1 "), find_line(r"^## 4\. "))
                                                   if LINES[i].strip()]))
fk.append("## 6 Faz gecis kapilari\n" + fmt_lines([(i + 1, clip(LINES[i], 1500)) for i in range(H6, H7) if LINES[i].strip()]))
if "00" in DOCS:
    ls0 = DOCS["00"]
    sel = []
    for i, l in enumerate(ls0):
        if re.search(r"gate|kapı|Gate|PASS|faz kapan", l) and (re.match(r"^#{1,4} ", l) or l.startswith("|") or l.startswith("- ") or l.startswith("**")):
            sel.append("00:%d| %s" % (i + 1, clip(l, 500)))
    fk.append("## 00 icinde kapi/gate satirlari (secim)\n" + "\n".join(sel[:120]))
fk.append("## Aday task tablosu (kimlik, faz, ad)\n" + "\n".join("11:%d| %s | %s | %s" % (a["line"], t, a["faz"], clip(a["ad"], 160))
                                                              for t, a in sorted(ADAY.items(), key=lambda kv: tid_key(kv[0]))))
body = "\n\n".join(fk) + "\n"
if len(body.encode("utf-8")) > CAP_PACKET:
    body = clip(body, CAP_PACKET)
yaz("FK_01", "FK", body, "faz tablosu, 3.1, 6, 00 kapi satirlari, aday tablo")

# ------------------------------------------------------------------ TE / AT betikleri
# TE: bloklarda gecen CamelCase / UC-n / E-nn / OB-nn adlarinin kaynak dokumanlarda varligi
alltext = {dn: "\n".join(ls) for dn, ls in DOCS.items() if dn in ("02", "03", "04", "05", "06", "07", "08", "09", "10")}
pascal = re.compile(r"`([A-Z][A-Za-z0-9]+(?:\.[A-Za-z0-9]+)*)`")
te = []
mis = Counter()
yer = defaultdict(list)
for t in TASKS:
    b = BLOCKS[t]
    for n in range(b["start"], b["end"]):
        for nm in set(pascal.findall(LINES[n])):
            if len(nm) < 4:
                continue
            ok = any(nm in tx for tx in alltext.values())
            if not ok:
                mis[nm] += 1
                if len(yer[nm]) < 4:
                    yer[nm].append("%s 11:%d" % (t, n + 1))
te.append("# TE paketi -- terim/ad: task bloklarinda `kod adi` olarak gecen ama `02`-`10` metinlerinde HIC gecmeyen adlar (betik adayi)\n"
          "Her ad icin karar ver: gercek ad farki/yazim hatasi mi, yoksa task'in dogru bir turetmesi (sinif/yontem adi) mi? "
          "Yakin bir kaynak adi varsa (ornek: Order <-> Orders) onu bul ve kanit olarak iki satiri yaz.\n")
for nm, c in mis.most_common():
    te.append("- `%s` (%d yer): %s" % (nm, c, "; ".join(yer[nm])))
# ek: entity adlari (06 §3.1 / 4.n basliklari) ve task bloklarinda gecisi
ent = []
if "06" in DOCS:
    for l in DOCS["06"]:
        m = re.match(r"^### 4\.\d+ ([A-Z][A-Za-z]+)", l)
        if m:
            ent.append(m.group(1))
te.append("\n## 06 entity adlari (%d)\n%s" % (len(ent), ", ".join(ent)))
ucs = sorted(set(re.findall(r"\bUC-\d+\b", "\n".join(DOCS.get("07", [])))), key=lambda s: int(s[3:]))
te.append("## Task bloklarinda gecen UC / E / OB kimlikleri kaynakta var mi")
for pat, dn, ad in (("UC-\\d+", "07", "UC"), ("E-\\d+", "04", "E"), ("OB-\\d+", "04", "OB"), ("MK-\\d+", "05", "MK"),
                    ("KP-\\d+|KD-\\d+", "10", "KP/KD"), ("[ZLBFPH]-\\d+", "02", "02 satir")):
    rx = re.compile(r"\b(" + pat + r")\b")
    eksik = []
    for t in TASKS:
        b = BLOCKS[t]
        for n in range(b["start"], b["end"]):
            for idv in set(rx.findall(LINES[n])):
                if dn in alltext and re.search(r"(^|\W)" + re.escape(idv) + r"(\W|$)", alltext[dn]) is None:
                    eksik.append("%s %s 11:%d" % (idv, t, n + 1))
    te.append("- %s -> %s icinde bulunmayan: %s" % (ad, dn, eksik[:40] if eksik else "yok"))
yaz("TE_01", "TE", "\n".join(te) + "\n", "betik adaylari: %d ad kaynakta yok" % len(mis))

# AT: `NN §x.y` atiflari
at = []
refrx = re.compile(r"`?(0[0-9]|1[0-2]) ?§(\d+(?:\.\d+)*)")
miss = []
sayi = 0
for t in TASKS:
    b = BLOCKS[t]
    for n in range(b["start"], b["end"]):
        for dn, sec in refrx.findall(LINES[n]):
            sayi += 1
            if dn not in IDX:
                if dn in ("12",):
                    continue
                miss.append((t, n + 1, dn, sec, "dokuman yok"))
                continue
            if sec not in IDX[dn] and not any(k.startswith(sec + ".") for k in IDX[dn]):
                miss.append((t, n + 1, dn, sec, "baslik yok"))
at.append("AT: task bloklarinda %d `NN §x.y` atfi; hedefi olmayan: %d" % (sayi, len(miss)))
for t, n, dn, sec, why in miss:
    at.append("%s | 11:%d | %s §%s | %s" % (t, n, dn, sec, why))
(OUT / "script_AT.txt").write_text("\n".join(at) + "\n", encoding="utf-8", newline="\n")
tani.append(at[0])

# ------------------------------------------------------------------ liste
liste = ["| Paket | Mercek | Boyut (bayt) | Kapsam |", "|---|---|---|---|"]
for ad, m, sz, ac in paketler:
    liste.append("| %s | %s | %d | %s |" % (ad, m, sz, ac))
ka_skor = {}
for ad, m, sz, ac in ka_pk:
    pass
(OUT / "paket_listesi.md").write_text("\n".join(liste) + "\n\nTANILAR\n" + "\n".join(tani) + "\n", encoding="utf-8", newline="\n")
print("\n".join(liste))
print()
print("\n".join(tani))
agir = [(ad, sz) for ad, m, sz, ac in paketler if sz > CAP_PACKET]
if agir:
    print("UYARI: sinir asan paket:", agir)
    sys.exit(1)
