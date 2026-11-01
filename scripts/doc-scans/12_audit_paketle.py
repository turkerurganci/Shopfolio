"""Asama 10 -- `12_VALIDATION_PROTOCOL.md` kalite dongusu audit'i: mercek paketleyicisi (KD-audit, Parca A; Karar B).

`11_audit_paketle.py` `11`'e ozeldir (task blogu); bu betik `12`'ye ozeldir: matris satiri (VAL-*) + Kaynak hucresinin
birimlerinin KAYNAK METNI + ilgili K satirlari. Ayni yeri-esleme kurali: bir Kaynak birimi once basliktan, sonra kalin
numaradan, sonra tablo satiri kimliginden cozulur.

Mercekler (her biri dar, satir numarali paket; en cok 85 KB):
  KA  kapsama/dogruluk : 12 dilimi (satir + duzyazi) + Kaynak birimlerinin kaynak metni (10 KP, 02-09 bolum, K satiri)
  CR  cerceve          : 12 s3, s5-s11 <-> 00 sD/E/F <-> 11 s6 <-> skills/validate
  SV  seviye/severity/kanit/etiket : betik anomalileri + kural metni (s3, s4.2, s7, s8.1, K-1512/1518/1519)
  KD  negatif          : kompakt satirlar + 10 s3 KD/YH satirlari
  TE  terim/ad         : betik adayi (satirda gecen ve 02-09'da bulunamayan simge + yakin eslesme)
  OL  olculebilirlik   : belirsiz ifade / kisa beklenen sonuc adaylari
  IT  ic tutarlilik    : s1/s2 sayimlari <-> matris, park tablosu, yinelenen/celisen aday cifti

Kullanim: python -I scripts/doc-scans/12_audit_paketle.py <cikti_dizini> [--doc yol] [--sinama]
  --sinama  yalniz tanilari yazdirir, paket yazmaz (iki yonlu sinama)
"""
import difflib
import re
import sys
from collections import Counter, defaultdict
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

ROOT = Path(r"C:\projects\Shopfolio")
ARGS = list(sys.argv[1:])
SINAMA = "--sinama" in ARGS
if "--doc" in ARGS:
    i = ARGS.index("--doc")
    DOC = Path(ARGS[i + 1])
    del ARGS[i:i + 2]
else:
    DOC = ROOT / "Docs" / "12_VALIDATION_PROTOCOL.md"
ARGS = [a for a in ARGS if not a.startswith("--")]
OUT = Path(ARGS[0]) if ARGS else Path("kd-audit-paket")
CAP_PACKET = 85_000
CAP_SLICE = 32_000
CAP_K = 700

LINES = DOC.read_text(encoding="utf-8").split("\n")
N = len(LINES)


def clip(s, n):
    b = s.encode("utf-8")
    if len(b) <= n:
        return s
    return b[:n].decode("utf-8", "ignore") + " [...kesildi]"


def bsz(s):
    return len(s.encode("utf-8"))


def cells(line):
    return [c.strip() for c in line.strip().strip("|").split(" | ")]


def find_line(rx, start=0, end=None):
    r = re.compile(rx)
    for i in range(start, end or N):
        if r.search(LINES[i]):
            return i
    return None


# ------------------------------------------------------------------ 12 yapisi
H1 = find_line(r"^## 1\. ")
H3 = find_line(r"^## 3\. ")
H4 = find_line(r"^## 4\. ")
H5 = find_line(r"^## 5\. ")
H8 = find_line(r"^## 8\. ")
H12 = find_line(r"^## 12\. ")
if None in (H1, H3, H4, H5, H8, H12):
    raise SystemExit("12 bolum basliklari bulunamadi (bozuk girdi?)")
H2 = find_line(r"^## 2\. ")
H9 = find_line(r"^## 9\. ")
H7 = find_line(r"^## 7\. ")
H11 = find_line(r"^## 11\. ")

ROWS = []          # dict(line, id, lvl, kural, kaynak, onkosul, beklenen, kanit, ortam, sev, durum, raw, sec)
SEC_OF = {}
cur = ""
for i, l in enumerate(LINES):
    m = re.match(r"^#{2,3} (\d+(?:\.\d+)*)\.? ", l)
    if m:
        cur = m.group(1)
    if l.startswith("| VAL-"):
        c = cells(l)
        if len(c) != 10:
            print("UYARI: sutun sayisi %d: 12:%d" % (len(c), i + 1))
            continue
        ROWS.append(dict(line=i + 1, id=c[0], lvl=c[1], kural=c[2], kaynak=c[3], onkosul=c[4], beklenen=c[5],
                         kanit=c[6], ortam=c[7], sev=c[8], durum=c[9], raw=l, sec=cur))
ROW_AT = {r["line"]: r for r in ROWS}


def fmt(a, b):
    return "\n".join("12:%d| %s" % (n + 1, LINES[n]) for n in range(a, b))


# ------------------------------------------------------------------ kaynak dokumanlari
DOCS = {}
for p in sorted((ROOT / "Docs").glob("[0-1][0-9]_*.md")):
    if p.name[:2] != "12":
        DOCS[p.name[:2]] = p.read_text(encoding="utf-8").split("\n")

HEAD_RX = re.compile(r"^(#{1,6}) (.*)$")
IDX = {}
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
        mb = re.match(r"^(?:- )?\*\*(\d+(?:\.\d+)+)[ .]", l)
        if mb:
            ix[mb.group(1)].append(("b", i))
            continue
        mr = re.match(r"^\|\s*`?([A-Z]{1,3}-\d+|\d+(?:\.\d+)+)`?\s*\|", l)
        if mr:
            ix[mr.group(1)].append(("r", i))
    IDX[dn] = ix

KAYIT = {}
kp = ROOT / "Docs" / "PRODUCT_DISCOVERY_STATUS.md"
rxk = re.compile(r"^\| (K-\d+) \|")
for n, l in enumerate(kp.read_text(encoding="utf-8").split("\n")):
    m = rxk.match(l)
    if m and m.group(1) not in KAYIT:
        KAYIT[m.group(1)] = (n + 1, l)


def sect_text(dn, i, cap):
    ls = DOCS[dn]
    out = [(i + 1, ls[i])]
    for j in range(i + 1, min(len(ls), i + 400)):
        if HEAD_RX.match(ls[j]):
            break
        out.append((j + 1, ls[j]))
    return clip("\n".join("%s:%d| %s" % (dn, n, t) for n, t in out), cap)


def para_text(dn, i, cap):
    ls = DOCS[dn]
    out = []
    for j in range(i, min(len(ls), i + 40)):
        if j > i and (ls[j].strip() == "" or HEAD_RX.match(ls[j])):
            break
        out.append("%s:%d| %s" % (dn, j + 1, ls[j]))
    return clip("\n".join(out), cap)


def resolve(fam, key, cap):
    """Kaynak birimin metni; bulunamazsa None."""
    if fam == "K":
        if key in KAYIT:
            return "K:%d| %s" % (KAYIT[key][0], clip(KAYIT[key][1], max(cap, CAP_K)))
        return None
    if fam not in IDX:
        return None
    hits = IDX[fam].get(key.rstrip(".,;"))
    if not hits:
        return None
    outs = []
    for kind, i in hits[:2]:
        if kind == "h":
            outs.append(sect_text(fam, i, cap))
        elif kind == "b":
            outs.append(para_text(fam, i, cap))
        else:
            outs.append(clip("%s:%d| %s" % (fam, i + 1, DOCS[fam][i]), cap))
    return clip("\n".join(outs), cap)


REF_DOC = re.compile(r"\b(0[1-9]|1[01]) ((?:§\d+(?:\.\d+)*(?:, ?)?)+)")
REF_SEC = re.compile(r"§(\d+(?:\.\d+)*)")
REF_K = re.compile(r"\bK-\d+\b")
REF_KP = re.compile(r"\b10 (KP-\d+|ÖK-\d+|SK-\d+)\b")


def refs_of(text):
    """text -> sirali, tekil (fam, key) listesi."""
    out = []
    for m in REF_KP.finditer(text):
        out.append(("10", m.group(1)))
    for m in REF_DOC.finditer(text):
        for s in REF_SEC.findall(m.group(2)):
            out.append((m.group(1), s))
    for k in REF_K.findall(text):
        out.append(("K", k))
    seen, res = set(), []
    for r in out:
        if r not in seen:
            seen.add(r)
            res.append(r)
    return res


# ------------------------------------------------------------------ dilimleme
def slice_ranges():
    """12 satirlarini (s4 ve s12 govdesi) CAP_SLICE bayt dilimlerine bol; satir sinirinda."""
    spans = [(H4, H5), (H12, N)]
    out = []
    for a, b in spans:
        s, size = a, 0
        for i in range(a, b):
            sz = bsz(LINES[i]) + 12
            if size and size + sz > CAP_SLICE:
                out.append((s, i))
                s, size = i, 0
            size += sz
        out.append((s, b))
    return out


KA_SLICES = slice_ranges()


def slice_refs(a, b):
    refs = []
    seen = set()
    for i in range(a, b):
        l = LINES[i]
        if l.startswith("| VAL-"):
            c = cells(l)
            txt = c[3] if len(c) == 10 else l
        elif l.startswith("Kaynak:") or l.startswith("**Kaynak"):
            txt = l
        else:
            continue
        for r in refs_of(txt):
            if r not in seen:
                seen.add(r)
                refs.append(r)
    return refs


# ------------------------------------------------------------------ tanilar
tani = []
tani.append("12 satir: %d; VAL satiri: %d; benzersiz kimlik: %d; kayit K: %d" % (N, len(ROWS), len({r["id"] for r in ROWS}), len(KAYIT)))
tani.append("KA dilimleri: %d; aralik: %s" % (len(KA_SLICES), KA_SLICES))
cnt_tot = sum(1 for a, b in KA_SLICES for i in range(a, b) if LINES[i].startswith("| VAL-"))
cnt_all_rows_in_scope = sum(1 for r in ROWS if (H4 + 1 <= r["line"] - 1 < H5) or r["line"] - 1 >= H12)
tani.append("dilimlerdeki VAL satiri: %d; kapsamdaki (s4+s12) VAL satiri: %d; kapsam disi VAL satiri: %d"
            % (cnt_tot, cnt_all_rows_in_scope, len(ROWS) - cnt_all_rows_in_scope))
unres = Counter(); res_ok = Counter(); unres_ex = defaultdict(list)
for a, b in KA_SLICES:
    for fam, key in slice_refs(a, b):
        r = resolve(fam, key, 200)
        if r is None:
            unres[fam] += 1
            if len(unres_ex[fam]) < 8:
                unres_ex[fam].append(key)
        else:
            res_ok[fam] += 1
tani.append("kaynak cozumleme (aile: cozulen/cozulmeyen): " + "; ".join(
    "%s %d/%d" % (f, res_ok[f], unres[f]) for f in sorted(set(res_ok) | set(unres))))
for f, o in unres_ex.items():
    tani.append("  cozulmeyen ornek %s: %s" % (f, o))

if SINAMA:
    print("\n".join(tani))
    sys.exit(0)

# ------------------------------------------------------------------ yazim
OUT.mkdir(parents=True, exist_ok=True)
for old in list(OUT.glob("paket_*.md")) + list(OUT.glob("script_*.txt")):
    old.unlink()
paketler = []


def yaz(ad, mercek, govde, aciklama):
    p = OUT / ("paket_%s.md" % ad)
    p.write_text(govde, encoding="utf-8", newline="\n")
    paketler.append((ad, mercek, bsz(govde), aciklama))


def pack(items, header, prefix, mercek, cap=CAP_PACKET, aciklama=lambda its: ""):
    cur, size, n = [], bsz(header), 1
    for et, tx in items:
        sz = bsz(tx) + 2
        if cur and size + sz > cap:
            yaz("%s_%02d" % (prefix, n), mercek, header + "\n\n".join(t for _, t in cur) + "\n", aciklama([e for e, _ in cur]))
            cur, size, n = [], bsz(header), n + 1
        cur.append((et, tx))
        size += sz
    if cur:
        yaz("%s_%02d" % (prefix, n), mercek, header + "\n\n".join(t for _, t in cur) + "\n", aciklama([e for e, _ in cur]))


def aralik(e):
    return "%s..%s (%d)" % (e[0], e[-1], len(e)) if e else ""


# ---- KA
KA_HEAD = ("# KA paketi -- 12 dilimi + Kaynak birimlerinin kaynak metni\n"
           "Satir onekleri: `12:<n>|` Dogrulama Protokolu, `NN:<n>|` kaynak dokuman, `K:<n>|` karar kaydi. Kaynak metin birim basina "
           "kesilmistir ([...kesildi]); kesilen yerde hukum icin kaynak dosyayi `grep -n` / aralikli okumayla ac (dosyanin tamamini degil).\n"
           "Is: her VAL satirinin `Beklenen sonuc` + `On kosul` + `Kural` hucreleri `Kaynak` hucresindeki kaynak kuraldan DOGRU ve EKSIKSIZ "
           "turetilmis mi (uydurulmus sayi/sure/ad yok mu; kaynak kuralin bir parcasi dusmus mu; kaynak ile celisen yer var mi)?\n\n")
ka_yaz = []
for k, (a, b) in enumerate(KA_SLICES, 1):
    sl = fmt(a, b)
    refs = slice_refs(a, b)
    body = None
    for cap in (1100, 800, 560, 380, 260):
        parts = []
        for fam, key in refs:
            r = resolve(fam, key, cap)
            if r is None:
                lab = "KP" if fam == "10" else fam
                parts.append("- [%s %s] KAYNAK METIN BULUNAMADI" % (fam, key))
            else:
                parts.append("- [%s %s]\n%s" % (fam, key, r))
        src = "\n".join(parts)
        body = "## DILIM 12:%d-%d\n%s\n\n## KAYNAK BIRIMLERI (%d)\n%s\n" % (a + 1, b, sl, len(refs), src)
        if bsz(KA_HEAD) + bsz(body) <= CAP_PACKET:
            break
    ka_yaz.append(("KA_%02d" % k, body))
    yaz("KA_%02d" % k, "KA", KA_HEAD + body, "12:%d-%d (%d satir)" % (a + 1, b, sum(1 for i in range(a, b) if LINES[i].startswith("| VAL-"))))

# ---- CR
def text_of(dn, a, b):
    ls = DOCS[dn]
    return "\n".join("%s:%d| %s" % (dn, i + 1, ls[i]) for i in range(a, b))


def doc_range(dn, rx_start, rx_end):
    ls = DOCS[dn]
    s = next(i for i, l in enumerate(ls) if re.match(rx_start, l))
    e = next((i for i in range(s + 1, len(ls)) if re.match(rx_end, ls[i])), len(ls))
    return s, e


m00 = {}
for key, (rs, re_) in dict(D=(r"^## D\. ", r"^## E\. "), E=(r"^## E\. ", r"^## F\. "), F=(r"^## F\. ", r"^## G\. ")).items():
    s, e = doc_range("00", rs, re_)
    m00[key] = text_of("00", s, e)
s11, e11 = doc_range("11", r"^## 6\. Faz ge", r"^## 7\. ")
m11 = text_of("11", s11, e11)
skill = (ROOT / ".claude" / "skills" / "validate" / "SKILL.md").read_text(encoding="utf-8").split("\n")
msk = "\n".join("skills/validate:%d| %s" % (i + 1, l) for i, l in enumerate(skill))
CR_HEAD = ("# CR paketi -- cerceve: 12 s3, s5-s11 <-> 00 sD/E/F (Dönem 2 modeli, faz yapisi, savunma katmanlari) <-> 11 s6 (faz kapilari) <-> skills/validate\n"
           "Is: 12'nin cerceve kurallari (seviye, kapi, giris/cikis kriteri, kanit standardi, severity, FAIL akisi, ajan modeli, roller, yeniden dogrulama) "
           "ust dokuman ve skill ile CELISIYOR mu, EKSIK mi, FAZLA mi? Kaynak kazanir (K-652): 00/11/skill kural koyar, 12 tuketicidir.\n\n")
cr_items = [
    ("12s3", "## 12 s3 (Dogrulama seviyeleri)\n" + fmt(H3, H4 - 0 if False else find_line(r"^## 4\. ", H3))),
    ("12s5-7", "## 12 s5-s7\n" + fmt(H5, H8)),
    ("12s8-11", "## 12 s8-s11\n" + fmt(H8, H12)),
    ("00D", "## 00 sD\n" + m00["D"]),
    ("00E", "## 00 sE\n" + m00["E"]),
    ("00F", "## 00 sF\n" + m00["F"]),
    ("11s6", "## 11 s6\n" + m11),
    ("skill", "## skills/validate\n" + msk),
]
pack(cr_items, CR_HEAD, "CR", "CR", aciklama=lambda e: ",".join(e))

# ---- SV
def read_sect(name_rx, nxt_rx):
    s = find_line(name_rx)
    e = find_line(nxt_rx, s + 1)
    return fmt(s, e)


sv_rules = "\n\n".join([
    "## 12 s3.1-3.2 (seviye)\n" + read_sect(r"^### 3\.1 ", r"^### 3\.3 "),
    "## 12 s4.2 (satir yazim kurallari)\n" + read_sect(r"^### 4\.2 ", r"^### 4\.3 "),
    "## 12 s7.1-7.2 (kanit)\n" + read_sect(r"^### 7\.1 ", r"^### 7\.3 "),
    "## 12 s7.4 (kanit ortami)\n" + read_sect(r"^### 7\.4 ", r"^### 7\.5 "),
    "## 12 s8.1 (severity)\n" + read_sect(r"^### 8\.1 ", r"^### 8\.2 "),
    "\n".join("K:%d| %s" % (KAYIT[k][0], clip(KAYIT[k][1], 1500)) for k in ("K-1512", "K-1518", "K-1519") if k in KAYIT),
])
KANIT_TURLERI = set()
s71 = find_line(r"^### 7\.1 ")
for i in range(s71, find_line(r"^### 7\.2 ", s71)):
    m = re.match(r"^\| ([^|]+?) \|", LINES[i])
    if m and m.group(1) not in ("Tür", "---"):
        KANIT_TURLERI.add(m.group(1).strip())
ORTAMLAR = {"mock", "sandbox", "gerçek", "yok"}
SEVS = {"KRİTİK", "ORTA", "DÜŞÜK"}
DURUMLAR = {"BEKLEMEDE", "PASS", "FAIL", "KABUL EDİLMİŞ RİSK"}
KRIT_KW = re.compile(r"tutar|KDV|ödeme|para|iade|cayma|fesih|yetki|başka (müşteri|üye)|silin|imha|kişisel veri|yasak|geri ödeme|yasal|fatura|sözleşme", re.I)
DIS_KW = re.compile(r"ödeme sağlayıcı|iyzico|PayTR|e-posta (servis|altyap)|SMTP|Google|3D ?Secure|3DS|webhook", re.I)
anom = []
sev_lvl = Counter(); ort_lvl = Counter()
for r in ROWS:
    sev_lvl[(r["lvl"], r["sev"])] += 1
    ort_lvl[(r["lvl"], r["ortam"])] += 1
    why = []
    mid = re.match(r"VAL-([A-Z])(\d{3})$", r["id"])
    if not mid:
        why.append("kimlik bicimi")
    elif mid.group(1) != r["lvl"]:
        why.append("kimlik harfi %s != Seviye %s" % (mid.group(1), r["lvl"]))
    if r["sev"] not in SEVS:
        why.append("severity degeri '%s'" % r["sev"])
    if r["ortam"] not in ORTAMLAR:
        why.append("kanit ortami '%s'" % r["ortam"])
    if r["durum"] not in DURUMLAR:
        why.append("durum '%s'" % r["durum"])
    turler = [t.strip() for t in r["kanit"].split("+")]
    bad = [t for t in turler if t not in KANIT_TURLERI]
    if bad:
        why.append("kanit turu 7.1'de yok: %s" % bad)
    if r["sev"] == "KRİTİK" and len(set(turler)) < 2:
        why.append("KRITIK ama tek kanit turu")
    if r["ortam"] == "mock" and not DIS_KW.search(r["kural"] + " " + r["onkosul"] + " " + r["beklenen"] + " " + r["kaynak"]):
        why.append("mock ama dis servis ifadesi yok")
    if r["ortam"] == "yok" and DIS_KW.search(r["kural"] + " " + r["beklenen"]):
        why.append("ortam yok ama dis servis ifadesi var")
    if r["lvl"] in ("A", "B", "C", "D") and r["ortam"] in ("sandbox", "gerçek"):
        why.append("seviye %s ve ortam %s" % (r["lvl"], r["ortam"]))
    if r["sev"] in ("ORTA", "DÜŞÜK") and KRIT_KW.search(r["kural"]):
        why.append("%s ama kural KRITIK anahtar sozcuk iceriyor: %s" % (r["sev"], KRIT_KW.search(r["kural"]).group(0)))
    if why:
        anom.append((r, why))
sv_head = ("# SV paketi -- seviye / severity / kanit turu / kanit ortami / etiket tutarliligi\n"
           "Betik anomalileri onceden hesaplanmistir (anahtar sozcuk esleme KESIN DEGILDIR; her aday satiri oku ve karar ver: gercek tutarsizlik mi, "
           "yanlis pozitif mi). Kural metni asagidadir (12 s3, s4.2, s7, s8.1, K-1512/1518/1519).\n\n"
           "KANIT TURLERI (7.1): %s\n\n%s\n\n" % (sorted(KANIT_TURLERI), sv_rules))
dist = ["## DAGILIM (betik)", "seviye x severity: " + "; ".join("%s/%s=%d" % (k[0], k[1], v) for k, v in sorted(sev_lvl.items())),
        "seviye x ortam: " + "; ".join("%s/%s=%d" % (k[0], k[1], v) for k, v in sorted(ort_lvl.items())), ""]
sv_items = [("dagilim", "\n".join(dist))]
for r, why in anom:
    sv_items.append((r["id"], "12:%d| %s\n   ADAY: %s" % (r["line"], r["raw"], "; ".join(why))))
(OUT / "script_SV.txt").write_text("\n".join("%s\t%s" % (r["id"], "; ".join(w)) for r, w in anom), encoding="utf-8", newline="\n")
pack(sv_items, sv_head, "SV", "SV", aciklama=lambda e: aralik(e[1:] if e and e[0] == "dagilim" else e))

# ---- KD
kd_lines = []
m10 = DOCS["10"]
s10 = next(i for i, l in enumerate(m10) if re.match(r"^## 3\. ", l))
e10 = next(i for i in range(s10 + 1, len(m10)) if re.match(r"^## 4\. ", m10[i]))
for i in range(s10, e10):
    l = m10[i]
    if re.match(r"^\|\s*(KD|YH|SK)-\d+\s*\|", l):
        c = cells(l)
        kd_lines.append("10:%d| %s | %s" % (i + 1, c[0], clip(c[1] if len(c) > 1 else "", 260)))
    elif l.startswith("#"):
        kd_lines.append("10:%d| %s" % (i + 1, l))
kd_head = ("# KD paketi -- negatif denetim: MVP DISI (KD / YH / SK) ozelligine dogrulama kriteri yazilmis mi?\n"
           "Asagida `10 s3` kapsam disi satirlari (kisaltilmis), sonra kompakt VAL satirlari (kimlik | seviye | kural | beklenen sonuc | kaynak). "
           "Bir satir KD/YH maddesinin ozelligini MVP'de varmis gibi sinaniyorsa (ya da sinanmasi gereken 'olmama' yerine ozelligin kendisi "
           "kriter yapilmissa) bulgudur. Kapsam hizasi betigi yalniz KD/YH KIMLIGININ gecmesini sinar, ozelligi sinamaz.\n\n"
           "## 10 s3 KAPSAM DISI\n" + "\n".join(kd_lines) + "\n\n## VAL SATIRLARI (kompakt)\n")
kd_items = []
for r in ROWS:
    kd_items.append((r["id"], "12:%d| %s | %s | %s => %s | %s" % (
        r["line"], r["id"], r["lvl"], clip(r["kural"], 170), clip(r["beklenen"], 150), clip(r["kaynak"], 70))))
cap_kd = CAP_PACKET
pack(kd_items, kd_head, "KD", "KD", aciklama=aralik)
# kompakt satirlar tek satir: ayirici '\n\n' yerine tek satir sonu icin dosyalari yeniden yaz
for ad, mercek, sz, ac in list(paketler):
    if mercek == "KD":
        p = OUT / ("paket_%s.md" % ad)
        t = p.read_text(encoding="utf-8").replace("\n\n12:", "\n12:")
        p.write_text(t, encoding="utf-8", newline="\n")

# ---- TE
tok_rx = re.compile(r"`([^`\n]{1,80})`")
pas_rx = re.compile(r"\b([A-Z][a-z]+(?:[A-Z][a-z0-9]+)+)\b")
route_rx = re.compile(r"\b(?:GET|POST|PUT|PATCH|DELETE) /[A-Za-z0-9_\-/{}:.?=&]+")
code_rx = re.compile(r"\b[A-Z][A-Z0-9]+(?:_[A-Z0-9]+){1,}\b")
corpus = "\n".join("\n".join(ls) for ls in DOCS.values() if ls is not DOCS.get("10")) if False else "\n".join("\n".join(DOCS[k]) for k in sorted(DOCS) if k in ("02", "03", "04", "05", "06", "07", "08", "09", "10", "11"))
vocab = set(tok_rx.findall(corpus)) | set(pas_rx.findall(corpus)) | set(code_rx.findall(corpus))
vocab_l = [v for v in vocab if 4 <= len(v) <= 60]
te_cand = []
seen_t = {}
IDENT = re.compile(r"^[\w./:{}?=&\-]+$")
freq_cache = {}
for r in ROWS:
    toks = set()
    for cell in (r["kural"], r["onkosul"], r["beklenen"]):
        toks |= set(tok_rx.findall(cell)) | set(pas_rx.findall(cell)) | set(route_rx.findall(cell)) | set(code_rx.findall(cell))
    for t in toks:
        if len(t) < 3 or not re.search(r"[A-Za-zÇĞİÖŞÜçğıöşü]", t):
            continue
        if re.match(r"^VAL-|^VP\d|^K-\d|^KP-\d|^\d+( |$)|^§", t):
            continue
        if t not in freq_cache:
            freq_cache[t] = corpus.count(t)
        f = freq_cache[t]
        if f == 0 or (f <= 2 and IDENT.match(t)):
            seen_t.setdefault(t, []).append(r["id"])
for t, ids in sorted(seen_t.items()):
    near = difflib.get_close_matches(t, vocab_l, n=3, cutoff=0.78) if (freq_cache[t] == 0 and len(t) >= 4) else []
    te_cand.append((t, ids, near))
te_head = ("# TE paketi -- terim / ad: 12'de gecen ve 02-11 metninde HIC (gecis 0) ya da en cok 2 kez (zayif eslesme) gecen simgeler\n"
           "Betik: satirdaki `...` simgeleri, PascalCase adlar, `GET /...` uclari ve BUYUK_HARF kodlar; 02-11 metninde birebir gecmeyenler listelenir "
           "(yakin eslesme betikle onerilir). Yanlis pozitif cok olabilir (12'nin kendi kavramlari, dizge ornekleri): her adayi kaynakla (`grep -n`) "
           "sinayip gercek ad/uc/enum/hata kodu sapmasini bulgu yap. Ek: asagida ornek olarak tum ekran/uc/enum adlarini el ile de tara.\n\n")
te_items = []
for t, ids, near in te_cand:
    ex = ids[0]
    row = next(r for r in ROWS if r["id"] == ex)
    te_items.append((t, "- `%s`  02-11'de gecis: %d  (12'de %d satir: %s)  yakin: %s\n   ornek 12:%d| %s" % (
        t, freq_cache[t], len(ids), ", ".join(ids[:6]), near, row["line"], clip(row["raw"], 420))))
pack(te_items, te_head, "TE", "TE", aciklama=lambda e: "%d aday" % len(e))
(OUT / "script_TE.txt").write_text("\n".join("%s\t%s\t%s" % (t, ",".join(i), n) for t, i, n in te_cand), encoding="utf-8", newline="\n")

# ---- OL
VAG = re.compile(r"\b(uygun|gerekirse|gerektiğinde|makul|hızlı|yeterli|benzeri|vb\.|vs\.|gibi|ilgili|düzgün|doğru (bir )?(biçimde|şekilde)|kısa sürede|"
                 r"bazı|birkaç|çok|az|sorunsuz|beklendiği gibi|normal|olağan|mümkün|genellikle|çoğunlukla|yaklaşık|en kısa|derhal|anında|uygun biçimde)\b", re.I)
ol_items = []
for r in ROWS:
    why = []
    mv = VAG.search(r["beklenen"])
    if mv:
        why.append("belirsiz: %s" % mv.group(0))
    mk = VAG.search(r["kural"])
    if mk and not mv:
        why.append("kuralda belirsiz: %s" % mk.group(0))
    if len(r["beklenen"]) < 45:
        why.append("kisa beklenen sonuc (%d)" % len(r["beklenen"]))
    if r["onkosul"] in ("", "—", "-", "yok"):
        why.append("on kosul bos/yok")
    if why:
        ol_items.append((r["id"], "12:%d| %s\n   ADAY: %s" % (r["line"], r["raw"], "; ".join(why))))
ol_head = ("# OL paketi -- olculebilirlik: satir sinanabilir mi?\n"
           "Betik adaylari: belirsiz sozcuk, cok kisa beklenen sonuc, bos on kosul. Her aday satiri oku: PASS/FAIL'i iki farkli test eden ayni mi karar verir? "
           "Sayi/sure/esik/durum adi gereken yerde var mi? (Sozcuk eslemesi kesin degildir; yanlis pozitifi ele.) 12 s4.2 satir yazim kurallari:\n"
           + read_sect(r"^### 4\.2 ", r"^### 4\.3 ") + "\n\n")
pack(ol_items, ol_head, "OL", "OL", aciklama=aralik)

# ---- IT
def toks_of(s):
    return set(w for w in re.findall(r"[\wçğıöşüÇĞİÖŞÜ]{4,}", s.lower()))


by_kp = defaultdict(list)
for r in ROWS:
    mk = re.search(r"10 (KP-\d+)", r["kaynak"])
    by_kp[mk.group(1) if mk else r["kaynak"][:40]].append(r)
dups = []
for key, rs in by_kp.items():
    for i in range(len(rs)):
        a = toks_of(rs[i]["kural"] + " " + rs[i]["beklenen"])
        for j in range(i + 1, len(rs)):
            b = toks_of(rs[j]["kural"] + " " + rs[j]["beklenen"])
            if not a or not b:
                continue
            jac = len(a & b) / len(a | b)
            if jac >= 0.62:
                dups.append((jac, rs[i], rs[j]))
# KP disindaki satirlar icin: K-1512 sinifi -- ayni kural iki seviyede
by_kural = defaultdict(list)
for r in ROWS:
    by_kural[re.sub(r"\W+", " ", r["kural"].lower()).strip()[:90]].append(r)
for k, rs in by_kural.items():
    if len(rs) > 1:
        for j in range(1, len(rs)):
            dups.append((1.0, rs[0], rs[j]))
dups.sort(key=lambda x: -x[0])
count_tab = Counter((r["sec"]) for r in ROWS)
lv_tab = Counter(r["lvl"] for r in ROWS)
sv_tab = Counter(r["sev"] for r in ROWS)
sec1 = fmt(H1, H3) if False else fmt(H1, find_line(r"^## 3\. ", H1))
park_s = find_line(r"^\*\*Park bloklarının durumu")
park_e = find_line(r"^\*\*Y5 kapan", park_s)
it_head = ("# IT paketi -- ic tutarlilik\n"
           "(1) 12 s1 ve s2'deki SAYIM / HARITA iddialari <-> matrisin gercek sayilari (asagida betik cikti) ; (2) park tablosu <-> gövde ; "
           "(3) cift / celisen / ayni kuralin iki seviyedeki satiri (K-1512) adaylari. Adaylar betiktir, kesin degildir.\n\n"
           "## BETIK SAYIMLARI\nVAL satiri: %d\nbolum basina: %s\nseviye basina: %s\nseverity basina: %s\n\n" % (
               len(ROWS), dict(sorted(count_tab.items(), key=lambda x: [int(p) if p.isdigit() else 0 for p in x[0].split(".")])),
               dict(sorted(lv_tab.items())), dict(sv_tab)))
it_items = [("s1s2", "## 12 s1 ve s2\n" + sec1 + "\n" + fmt(H2, H3)),
            ("park", "## PARK TABLOSU (12 s0)\n" + fmt(park_s, park_e + 1))]
for jac, a, b in dups:
    it_items.append((a["id"] + "~" + b["id"], "### ADAY CIFT (benzerlik %.2f)\n12:%d| %s\n12:%d| %s" % (jac, a["line"], a["raw"], b["line"], b["raw"])))
pack(it_items, it_head, "IT", "IT", aciklama=lambda e: "%d oge" % len(e))

# ------------------------------------------------------------------ dizin
(OUT / "paket_dizin.txt").write_text(
    "\n".join("%s\t%s\t%d\t%s" % p for p in paketler) + "\n\n" + "\n".join(tani) + "\n", encoding="utf-8", newline="\n")
print("\n".join(tani))
print("paket sayisi: %d" % len(paketler))
for ad, mercek, sz, ac in paketler:
    print("%-8s %-3s %7d  %s" % (ad, mercek, sz, ac))
