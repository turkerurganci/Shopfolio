"""Asama 9 -- `11_IMPLEMENTATION_PLAN.md` ozet <-> ayrinti taramasi (yazim turu sonu, kalite
dongusunden once; checklists/document-stage.md 4 "ozet <-> ayrinti taramasi").

Iki is yapar:
  1. Betikle denetlenebilen esitlikler (sayim, kimlik kumesi, faz-blok uyumu, 7.2 durum
     sayilari, 7.1 aile sayilari, 6.2 / 8 / park tablosu atiflari, surum damgasi) -> stdout ve
     `script_bulgu.txt`; her satir bir bulgu adayidir (SB-n).
  2. Sonnet okuyucular icin paketler (`paket_S_<n>.md`: ozet cumleler + atifli bloklar;
     `paket_A_<n>.md`: aday tablo satiri + task blogu + 7.2 satiri + 7.1 kaynak satirlari).

Kullanim: python -I scripts/doc-scans/11_ozet_ayrinti_paketle.py [cikti_dizini]
"""
import re
import sys
from collections import Counter, defaultdict
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

ROOT = Path(r"C:\projects\Shopfolio")
DOC = ROOT / "Docs" / "11_IMPLEMENTATION_PLAN.md"
OUT = Path(sys.argv[1]) if len(sys.argv) > 1 else Path(__file__).resolve().parent
OUT.mkdir(parents=True, exist_ok=True)
CAP = 55_000

LINES = DOC.read_text(encoding="utf-8").split("\n")
N = len(LINES)
TID = re.compile(r"\bT\d{2,3}[a-z]?(?:\.\d+)?\b")
TID_FULL = re.compile(r"^T\d+[a-z]?(?:\.\d+)?$")
bulgular = []


def bulgu(msg):
    bulgular.append(msg)


def num(s):
    return int(s.replace(".", ""))


def cells(line):
    parts = line.strip().strip("|").split(" | ")
    return [c.strip() for c in parts]


def idx(pattern, start=0):
    rx = re.compile(pattern)
    for i in range(start, N):
        if rx.search(LINES[i]):
            return i
    raise SystemExit("bulunamadi: " + pattern)


# ---------------------------------------------------------------- bolum haritasi
H2 = [(i, l) for i, l in enumerate(LINES) if l.startswith("## ")]


def sec(prefix):
    for k, (i, l) in enumerate(H2):
        if l.startswith(prefix):
            e = H2[k + 1][0] if k + 1 < len(H2) else N
            return i, e
    raise SystemExit("bolum yok: " + prefix)


S1 = sec("## 1.")
S2 = sec("## 2.")
S3 = sec("## 3.")
S5 = sec("## 5.")
S6 = sec("## 6.")
S7 = sec("## 7.")
S8 = sec("## 8.")
S9 = sec("## 9.")
HEAD_END = S1[0]

# ---------------------------------------------------------------- aday tablo
a0 = idx(r"^\| Aday \| Faz \| Kısa ad")
aday = {}
aday_order = []
for i in range(a0 + 2, N):
    if not LINES[i].startswith("| T"):
        break
    c = cells(LINES[i])
    aday[c[0]] = {"faz": c[1], "ad": c[2], "modul": c[3], "kaynak": c[4], "line": i + 1}
    aday_order.append(c[0])

# ---------------------------------------------------------------- bloklar
blocks = {}
faz_of_block = {}
cur_faz = None
cur = None
for i in range(S5[0], S5[1]):
    l = LINES[i]
    m = re.match(r"^### (F\d) — ", l)
    if m:
        cur_faz = m.group(1)
    if l.startswith("#### T"):
        mt = re.match(r"^#### (T\d+[a-z]?(?:\.\d+)?): (.*)$", l)
        if mt:
            cur = mt.group(1)
            blocks[cur] = {"title": mt.group(2), "start": i, "lines": [l]}
            faz_of_block[cur] = cur_faz
            continue
    if cur and (l.startswith("### ") or l.startswith("## ")):
        cur = None
    if cur:
        blocks[cur]["lines"].append(l)
for t, b in blocks.items():
    b["text"] = "\n".join(b["lines"]).rstrip()


def field(t, name):
    txt = blocks[t]["text"]
    m = re.search(r"- \*\*" + re.escape(name) + r":\*\*(.*?)(?=\n- \*\*|\Z)", txt, re.S)
    return m.group(1).strip() if m else ""


def num_of(t):
    return int(re.match(r"T(\d+)", t).group(1))


def faz_by_id(t):
    n = num_of(t)
    return "F%d" % (0 if n < 100 else n // 100)


# ---------------------------------------------------------------- 7.1 ve 7.2
s71 = idx(r"^### 7\.1 ")
s72 = idx(r"^### 7\.2 ")
rows71 = []  # (src, tasks, line, subsection)
sub = ""
for i in range(s71, s72):
    l = LINES[i]
    m = re.match(r"^#### (7\.1\.\d)", l)
    if m:
        sub = m.group(1)
    if l.startswith("| ") and re.match(r"^\| (\d\d[: ]|devir:)", l):
        c = cells(l)
        tasks = [x.strip() for x in c[1].split(",") if x.strip() and x.strip() != "—"]
        rows71.append((c[0], tasks, i + 1, sub))
rows72 = {}
for i in range(s72, S8[0]):
    l = LINES[i]
    if re.match(r"^\| T\d", l):
        c = cells(l)
        rows72[c[0]] = {"src": c[1], "durum": c[2], "line": i + 1}

task_rows = defaultdict(list)
for src, tasks, ln, sb in rows71:
    for t in tasks:
        task_rows[t].append(src)

# ================================================================ BETIKLE DENETIMLER
# SB: kume esitlikleri
if not (set(aday) == set(blocks) == set(rows72)):
    bulgu("kume: aday %d, blok %d, 7.2 %d; fark aday-blok %s blok-aday %s 7.2-aday %s" % (
        len(aday), len(blocks), len(rows72), sorted(set(aday) - set(blocks)),
        sorted(set(blocks) - set(aday)), sorted(set(rows72) ^ set(aday))))
total = len(aday)

# faz uyumu
for t in aday_order:
    if aday[t]["faz"] != faz_by_id(t):
        bulgu("faz: %s aday tabloda %s, kimlik araligi %s" % (t, aday[t]["faz"], faz_by_id(t)))
    if t in faz_of_block and faz_of_block[t] != aday[t]["faz"]:
        bulgu("faz: %s blogu %s basligi altinda, aday tabloda %s" % (t, faz_of_block[t], aday[t]["faz"]))
    # baslik adi aday kisa adinin on kismiyla uyusuyor mu
    if t in blocks:
        ad = aday[t]["ad"].split(":")[0].strip().lower()
        bt = blocks[t]["title"].split(":")[0].strip().lower()
        if ad != bt and not ad.startswith(bt) and not bt.startswith(ad):
            blocks[t]["ad_farki"] = True

faz_count = Counter(aday[t]["faz"] for t in aday_order)
FAZ_IDS = defaultdict(list)
for t in aday_order:
    FAZ_IDS[aday[t]["faz"]].append(t)

# sayim iddialari
text_all = "\n".join(LINES)
exp_total = str(total)
for m in re.finditer(r"(\d+)\s+task\b", text_all):
    pass  # serbest cumleler okuyucuya birakilir; asagida bilinen kaliplar denetlenir

# 3 faz tablosu
for i in range(S3[0], S3[1]):
    m = re.match(r"^\| (F\d) \| (.*?) \| .*\| `(T\d+)`–`(T\d+)` \((\d+)\) \|$", LINES[i])
    if m:
        f, ad, lo, hi, n = m.group(1), m.group(2), m.group(3), m.group(4), int(m.group(5))
        ids = FAZ_IDS[f]
        nums = [num_of(x) for x in ids]
        if n != len(ids):
            bulgu("§3 satir %d: %s sayisi %d, aday tabloda %d" % (i + 1, f, n, len(ids)))
        if num_of(lo) != min(nums) or num_of(hi) != max(nums):
            bulgu("§3 satir %d: %s araligi %s-%s, aday tablo %s-%s" % (i + 1, f, lo, hi, "T%d" % min(nums), "T%d" % max(nums)))
        if f in faz_of_block.values():
            h = [l for l in LINES[S5[0]:S5[1]] if l.startswith("### " + f + " —")]
            if h and ad not in h[0]:
                bulgu("§3 satir %d: %s adi '%s', §5 basligi '%s'" % (i + 1, f, ad, h[0]))

# 2 satir: faz dagilimi
for i in range(S2[0], S2[1]):
    m = re.search(r"F0 (\d+) · F1 (\d+) · F2 (\d+) · F3 (\d+) · F4 (\d+) · F5 (\d+) · F6 (\d+)", LINES[i])
    if m:
        got = [int(x) for x in m.groups()]
        exp = [faz_count["F%d" % k] for k in range(7)]
        if got != exp:
            bulgu("§2 satir %d: faz dagilimi %s, aday tablo %s" % (i + 1, got, exp))

# yazim haritasi (baslik notu)
def count_range(lo, hi):
    return sum(1 for t in aday_order if num_of(lo) <= num_of(t) <= num_of(hi))


ym = 0
for i in range(0, HEAD_END):
    m = re.match(r"^\| (Y\d\w?) \| .*?\| (.*?) \| .*\| (✓|—|.*?) \|$", LINES[i])
    if m:
        ym += 1
        rng, n = m.group(2), None
        mm = re.search(r"\((\d+)\)\s*$", rng)
        if mm:
            n = int(mm.group(1))
            parts = re.findall(r"`(T\d+)`–`(T\d+)`", rng)
            tot = sum(count_range(a, b) for a, b in parts)
            if tot != n:
                bulgu("başlık notu yazım haritası %s: (%d) yazıyor, aday tabloda %d" % (m.group(1), n, tot))
            for a, b in parts:
                for t in aday_order:
                    if num_of(a) <= num_of(t) <= num_of(b) and t not in blocks:
                        bulgu("harita %s: %s blok olarak yazılı değil" % (m.group(1), t))
if ym != 13:
    bulgu("başlık notu: yazım haritasında %d oturum satırı, metin on üç diyor" % ym)
hm = re.search(r"Task toplamı ([\d + ]+) = (\d+)", "\n".join(LINES[:HEAD_END]))
if hm:
    parts = [int(x) for x in hm.group(1).split("+")]
    if sum(parts) != int(hm.group(2)) or int(hm.group(2)) != total:
        bulgu("başlık notu task toplamı %s = %s, aday tablo %d" % (hm.group(1).strip(), hm.group(2), total))
    exp = [faz_count["F0"] + faz_count["F1"], faz_count["F2"], faz_count["F3"], faz_count["F4"], faz_count["F5"], faz_count["F6"]]
    if parts != exp:
        bulgu("başlık notu task toplamı terimleri %s, fazlar %s" % (parts, exp))
for m in re.finditer(r"`(T\d+)`–`(T\d+)` \((\d+) \+ (\d+) \+ (\d+) \+ (\d+)\)|\((\d+) \+ (\d+) \+ (\d+) task\)", "\n".join(LINES[:HEAD_END])):
    pass
hn = "\n".join(LINES[:HEAD_END])
m = re.search(r"F3 dört parçadır — (.*?) \((\d+) \+ (\d+) \+ (\d+) \+ (\d+) task\)", hn)
if m:
    parts = re.findall(r"`(T\d+)`–`(T\d+)`", m.group(1))
    got = [count_range(a, b) for a, b in parts]
    exp = [int(x) for x in m.groups()[1:]]
    if got != exp:
        bulgu("başlık notu F3 parça sayıları %s, aday tablo %s" % (exp, got))
m = re.search(r"F5 üç parçadır — (.*?) \((\d+) \+ (\d+) \+ (\d+) task\)", hn)
if m:
    parts = re.findall(r"`(T\d+)`–`(T\d+)`", m.group(1))
    got = [count_range(a, b) for a, b in parts]
    exp = [int(x) for x in m.groups()[1:]]
    if got != exp:
        bulgu("başlık notu F5 parça sayıları %s, aday tablo %s" % (exp, got))

# 7.1 aile sayilari ve toplam
fam = Counter()
for src, tasks, ln, sb in rows71:
    fam[src[:2] if src[0].isdigit() else "devir"] += 1
fam["04E"] = sum(1 for s, _, _, _ in rows71 if s.startswith("04:E-"))
fam["04OB"] = sum(1 for s, _, _, _ in rows71 if s.startswith("04:OB-"))
fam["07UC"] = sum(1 for s, _, _, _ in rows71 if s.startswith("07 UC-"))
fam["05MK"] = sum(1 for s, _, _, _ in rows71 if s.startswith("05:MK-"))
fam["10KP"] = sum(1 for s, _, _, _ in rows71 if s.startswith("10:KP-"))
fam["10KD"] = sum(1 for s, _, _, _ in rows71 if s.startswith("10:KD-"))
fam["06ent"] = sum(1 for s, _, _, _ in rows71 if s.startswith("06:§4."))
fam["02id"] = sum(1 for s, _, _, _ in rows71 if re.match(r"02:[ZLBFPH]-\d+", s))
fam["devirK"] = sum(1 for s, _, _, _ in rows71 if s.startswith("devir:K-"))
fam["devirP"] = sum(1 for s, _, _, _ in rows71 if s.startswith("devir:P8."))
fam["devirD"] = sum(1 for s, _, _, _ in rows71 if s.startswith("devir:D-01"))
fam["devirG"] = fam["devir"] - fam["devirK"] - fam["devirP"] - fam["devirD"]
total_rows = len(rows71)
s2_claims = {}
for i in range(S2[0], S2[1]):
    m = re.match(r"^\| `(\d\d)` .*? \| (\d[\d.]*) \| (.*) \|$", LINES[i])
    if m:
        s2_claims[m.group(1)] = (num(m.group(2)), m.group(3), i + 1)
    m = re.match(r"^\| Devir satırları \| (\d+) \| (.*) \|$", LINES[i])
    if m:
        s2_claims["devir"] = (int(m.group(1)), m.group(2), i + 1)
    m = re.match(r"^\| \*\*Toplam\*\* \| \*\*([\d.]+)\*\*", LINES[i])
    if m:
        s2_claims["toplam"] = (num(m.group(1)), "", i + 1)
for k, (n, desc, ln) in s2_claims.items():
    if k == "toplam":
        if n != total_rows:
            bulgu("§2 satır %d: toplam %d, §7.1 satır sayısı %d" % (ln, n, total_rows))
    elif fam[k] != n:
        bulgu("§2 satır %d: `%s` %d, §7.1'de %d satır" % (ln, k, n, fam[k]))
    if k != "toplam":
        for lab, key in (("ekran", "04E"), ("ortak bileşen", "04OB"), ("uç", "07UC"), ("mimari karar (MK)", "05MK"),
                         ("kapsam satırı (KP)", "10KP"), ("kapsam dışı satır (KD)", "10KD"), ("entity", "06ent")):
            mm = re.search(lab + r" (\d+)", desc)
            if mm and int(mm.group(1)) != fam[key]:
                bulgu("§2 satır %d: %s %s, §7.1'de %d" % (ln, lab, mm.group(1), fam[key]))
        mm = re.search(r"kimlikli satır (\d+)", desc)
        if mm and int(mm.group(1)) != fam["02id"]:
            bulgu("§2 satır %d: kimlikli satır %s, §7.1'de %d" % (ln, mm.group(1), fam["02id"]))
        mm = re.search(r"iş kuralı (\d+)", desc)
        if k == "02" and mm:
            r = sum(1 for s, _, _, _ in rows71 if re.match(r"02:§\d+\.\d+\.\d+", s))
            if r != int(mm.group(1)):
                bulgu("§2 satır %d: `02` iş kuralı %s, §7.1'de %d" % (ln, mm.group(1), r))
        if k == "devir":
            for lab, key in (("karar", "devirK"), ("park maddesi", "devirP"), ("gövde anması", "devirG"), ("D-01", "devirD")):
                mm = re.search(lab + r" (\d+)", desc)
                if mm and int(mm.group(1)) != fam[key]:
                    bulgu("§2 satır %d: devir %s %s, §7.1'de %d" % (ln, lab, mm.group(1), fam[key]))

# 02 bolum gruplari (7.1.5 'Kaynak' cumleleri)
def r02(lo, hi):
    c = 0
    for s, _, _, _ in rows71:
        m = re.match(r"02:(?:§(\d+)|[ZLBFPH]-\d+)", s)
        if not m:
            continue
        if m.group(1):
            n = int(m.group(1))
        else:
            n = {"Z": 4, "L": 8, "B": 9, "F": 9, "P": 11, "H": 9}.get(s[3], 0)
        if lo <= n <= hi:
            c += 1
    return c


for i in range(s71, s72):
    m = re.match(r"^Kaynak(?: \(O4[ab](?: \d/\d)?\))?: `02 §(\d+)`–§(\d+) \((\d+):", LINES[i])
    if m:
        lo, hi, n = int(m.group(1)), int(m.group(2)), int(m.group(3))
        if r02(lo, hi) != n:
            bulgu("§7.1.5 satır %d: `02 §%d`–§%d %d diyor, satır sayısı %d (kimlikli satırlar bölüm 4/8/9/10/11 olarak sayıldı; yakınsa okuyucu karar versin)" % (i + 1, lo, hi, n, r02(lo, hi)))

# alt bolum haritasi: aile -> alt bolum
HARITA = {"06": "7.1.1", "07": "7.1.2", "08": "7.1.3", "04": "7.1.4", "03": "7.1.4",
          "02": "7.1.5", "05": "7.1.5", "09": "7.1.5", "10": "7.1.5", "devir": "7.1.5"}
for s, _, ln, sb in rows71:
    k = s[:2] if s[0].isdigit() else "devir"
    if HARITA[k] != sb:
        bulgu("§7.1 satır %d: %s kimliği %s altında, harita %s diyor" % (ln, s[:20], sb, HARITA[k]))

# 7.2 durum ile 7.1 turetimi
for t in aday_order:
    r = rows72.get(t)
    if not r:
        continue
    srcs = task_rows.get(t, [])
    m = re.match(r"✓ (\d+) kaynak \(özgün (\d+), alt bölüm (\d+), devir (\d+)\)", r["durum"])
    if m:
        n, a, b, c = [int(x) for x in m.groups()]
        dev = sum(1 for s in srcs if s.startswith("devir:"))
        if n != len(srcs) or a + b + c != n or c != dev:
            bulgu("7.2 satır %d: %s durumu %s, 7.1'den %d kaynak (devir %d)" % (r["line"], t, r["durum"], len(srcs), dev))
    elif "gerekçeli" in r["durum"]:
        if len(srcs) == 0:
            bulgu("7.2 satır %d: %s gerekçeli ama 7.1'de kaynağı yok" % (r["line"], t))
    else:
        bulgu("7.2 satır %d: %s durum biçimi tanınmadı: %s" % (r["line"], t, r["durum"][:60]))
    for ref in re.findall(r"K-\d+", r["durum"]):
        pass

# 6.2 / 8 / park / 3.1 atiflari: T kimligi var mi, fazi dogru mu
for name, (a, b) in (("§6", S6), ("§8", S8), ("§9", S9), ("§3", S3), ("başlık notu", (0, HEAD_END)), ("§1", S1), ("§2", S2)):
    for i in range(a, b):
        for t in set(TID.findall(LINES[i])):
            if t not in aday and t != "T123":
                bulgu("%s satır %d: %s aday tabloda yok" % (name, i + 1, t))
# 6.2 satir fazi: satirda anilan task o fazin mi (DV-5/T510 gibi bilincli istisnalar okuyucuda)
for i in range(S6[0], S6[1]):
    m = re.match(r"^\| (F\d) \| (.*) \|$", LINES[i])
    if m:
        for t in set(TID.findall(m.group(2))):
            if t in aday and aday[t]["faz"] != m.group(1):
                bulgu("§6.2 satır %d: %s satırı %s diyor, task fazı %s (bilinçli istisna olabilir; okuyucuya)" % (i + 1, m.group(1), t, aday[t]["faz"]))
# 8: kapatan task sutunu ile satir metni
for i in range(S8[0], S8[1]):
    m = re.match(r"^\| (GAP-\d+) \| (.*?) \| (.*?) \| (.*?) \|$", LINES[i])
    if m:
        kapatan = set(x.strip() for x in m.group(4).split(","))
        metin = set(TID.findall(m.group(3)))
        if not metin <= kapatan:
            bulgu("§8 %s: durum hücresi %s anıyor, 'Kapatan task' sütunu %s" % (m.group(1), sorted(metin), sorted(kapatan)))
        for t in kapatan:
            if t not in blocks:
                bulgu("§8 %s: kapatan %s blok olarak yok" % (m.group(1), t))
# K atiflari ilk sirada: surum damgasi
ver_head = re.search(r"\*\*Versiyon: (v[\d.]+)\*\*", LINES[2])
ver_foot = re.search(r"Implementation Plan (v[\d.]+)\*", "\n".join(LINES[-4:]))
if ver_head and ver_foot and ver_head.group(1) != ver_foot.group(1):
    bulgu("sürüm damgası: başlık %s, dipnot %s" % (ver_head.group(1), ver_foot.group(1)))

# aday tabloda modul degerleri ve faz basina modul tutarliligi (F0 Ortak altyapi vb. okuyucuya)

# ================================================================ PAKETLER
HDR_A = """# Paket {pkg} — aday tablo satırı ↔ task bloğu ↔ 7.2 satırı ↔ 7.1 kaynak satırları

Sen bir okuyucusun; yalnız karşılaştırma yaparsın, düzeltme yazmazsın. Aşağıda her task için dört özet yeri ve ayrıntısı var:
- **Aday satırı** (§7 aday task tablosu): kısa ad, modül, kaynak birimi.
- **Task bloğu** (§5): başlık, bağımlılık, dokümanlar, kabul kriterleri, test, doğrulama listesi.
- **7.2 satırı**: task'ın beslendiği kaynak öğeler.
- **7.1 kaynak satırları**: bu task'a bağlı matris satırları (kimlik + ad, 140 karaktere kesik).

Her task için şu soruları cevapla; yalnız **açık fark** varsa bulgu yaz:
1. Aday satırının kısa adı ve kaynak birimi, task bloğunun kapsamıyla (başlık + kabul kriterleri) çelişiyor mu, ya da blok kısa adın söylediği bir şeyi hiç kapsamıyor mu?
2. Bloğun `Dokümanlar` alanı, 7.2 satırının saydığı kaynak doküman ailelerini (`02`, `04`, `07`, `05` MK, `09`, `10`…) taşıyor mu? (`09` için çekirdek yedi bölüm — §1, §2, §5, §6, §23, §24, §25 — anılmaz, bu fark değildir; ayrıntı gövdesi olmayan alt bölüm satırlarını `Dokümanlar` saymayabilir.) Blokta anılan bir kaynak 7.2'de yoksa bulgu.
3. 7.1 kaynak satırlarından bir ya da birkaçı, blokta hiçbir kabul kriterinin konusu değilse (blok o kaynağın kuralını/uç/ekran/entity'sini hiç ele almıyorsa) bulgu yaz. Kaynak satırı bir alt bölüm başlığıysa ve blok o alt bölümün konusuna dokunuyorsa bulgu değildir.
4. Bloğun içinde kendi içinde tutarsızlık (kabul kriteri sayısı ile KK sayısı, bağımlılık alanı ile kabul kriteri metni) varsa bulgu yaz.

Bulgu biçimi (her bulgu bir satır, dosyanın sonuna ekle; dosya yolu: {out}):
`B-<n> | <task kimliği> | <özet yeri: aday/7.2/7.1 <kimlik>/blok> | <özet cümlenin kısa alıntısı> | <ayrıntı yerinin alıntısı ya da "yok"> | <Yüksek/Orta/Düşük>`
Önem: Yüksek = bir task'ın kapsamı yanlış ya da bir kaynak sahipsiz kalıyor; Orta = açık sayı/ad/atıf farkı; Düşük = anlamı bozmayan ifade farkı. Üslup, biçem ve "daha iyi yazılabilir" önerisi bulgu değildir. Emin değilsen bulgu yazma. Bulgu yoksa dosyaya yalnız `BULGU YOK` yaz. Bitince kaç task okuduğunu ve kaç bulgu yazdığını tek satırda dön.

---
"""

HDR_S = """# Paket {pkg} — özet cümleler ↔ atıf yapılan task blokları ve tablolar

Sen bir okuyucusun; yalnız karşılaştırma yaparsın, düzeltme yazmazsın. Aşağıda `11_IMPLEMENTATION_PLAN.md`'nin özet yerleri (başlık notu, §1, §2, §3, §6, §8, §9 cümleleri ve tablo satırları) ve her birinin atıf yaptığı task'ların aday satırı ve bloğu var.

Her özet yeri için: cümlenin task'lar, sayılar, kimlikler ve kapsam hakkında söylediği şey ayrıntıyla (task bloğunun başlığı, kabul kriterleri, bağımlılığı, aday satırı) uyuşuyor mu? Özellikle: atıf yapılan task gerçekten o işi yapıyor mu; sayılan şeyin sayısı doğru mu; "T357 ve T384 şunu korur" gibi cümlede anılan task'ın kabul kriteri o iddiayı taşıyor mu; faz çıkış ürünü ve §6.2 kontrolü, anılan task'ların bloğuyla örtüşüyor mu; park/boşluk tablosundaki "karşılandı" kaydının task'ı gerçekten o işi kapsıyor mu. Yalnız **açık fark** varsa bulgu yaz.

Bulgu biçimi (her bulgu bir satır, dosyanın sonuna ekle; dosya yolu: {out}):
`B-<n> | <özet yeri: bölüm + satır no> | <özet cümlenin kısa alıntısı> | <ayrıntının kısa alıntısı + yeri> | <Yüksek/Orta/Düşük>`
Önem: Yüksek = yanlış task ya da yanlış kapsam; Orta = açık sayı/ad/atıf farkı; Düşük = anlamı bozmayan fark. Üslup önerisi bulgu değildir. Emin değilsen yazma. Bulgu yoksa dosyaya yalnız `BULGU YOK` yaz. Bitince kaç özet yeri okuduğunu ve kaç bulgu yazdığını tek satırda dön.

---
"""


def emit(pkg, header, blocks_txt):
    files = []
    cur, size, n = [], 0, 0
    pieces = list(blocks_txt)
    chunks = []
    for p in pieces:
        if cur and size + len(p.encode("utf-8")) > CAP:
            chunks.append(cur)
            cur, size = [], 0
        cur.append(p)
        size += len(p.encode("utf-8"))
    if cur:
        chunks.append(cur)
    for k, ch in enumerate(chunks, 1):
        name = "paket_%s_%d.md" % (pkg, k)
        path = OUT / name
        outp = (OUT / ("bulgu_%s_%d.txt" % (pkg, k))).as_posix()
        path.write_text(header.format(pkg="%s_%d" % (pkg, k), out=outp) + "\n\n".join(ch) + "\n", encoding="utf-8")
        files.append(name)
    return files


# ---- A paketleri: task basina
A = []
for t in aday_order:
    a = aday[t]
    r = rows72.get(t, {})
    srcs = []
    for src, tasks, ln, sb in rows71:
        if t in tasks:
            c = None
            srcs.append(src[:140])
    piece = "### %s\n**Aday satırı (satır %d):** faz %s · modül %s · kısa ad: %s · kaynak birimi: %s\n\n**7.2 satırı (satır %s):** %s — %s\n\n**7.1 kaynak satırları (%d):**\n%s\n\n**Task bloğu (§5, satır %d):**\n%s" % (
        t, a["line"], a["faz"], a["modul"], a["ad"], a["kaynak"], r.get("line", "?"), r.get("src", "YOK"), r.get("durum", ""),
        len(srcs), "\n".join("- " + s for s in srcs) if srcs else "(7.1'de kaynak satırı yok)",
        blocks[t]["start"] + 1 if t in blocks else 0, blocks[t]["text"] if t in blocks else "(BLOK YOK)")
    A.append(piece)
fa = emit("A", HDR_A, A)

# ---- S paketleri: ozet cumleler
def region_places(a, b):
    out = []
    in_fence = False
    for i in range(a, b):
        l = LINES[i]
        if l.strip().startswith("```"):
            in_fence = not in_fence
            continue
        if in_fence or not l.strip() or l.strip() in ("---",) or re.match(r"^\|[-| ]+\|$", l):
            continue
        if l.startswith("#"):
            continue
        out.append((i + 1, l))
    return out


places = []
for name, (a, b) in (("başlık notu", (0, HEAD_END)), ("§1", S1), ("§2", S2), ("§3", S3), ("§6", S6), ("§8", S8), ("§9", S9)):
    places += [(name, ln, l) for ln, l in region_places(a, b)]
# §5 "Task yapisi" ve faz basliklari altindaki Yurutme sirasi satirlari 3.1 ile birlikte
for i in range(S5[0], S5[1]):
    if LINES[i].startswith("**Yürütme sırası:**") and not LINES[i].startswith("**Yürütme sırası:** T01") is None:
        places.append(("§5 yürütme sırası", i + 1, LINES[i]))
S = []
S_count = 0
for name, ln, l in places:
    ids = []
    for t in TID.findall(l):
        if t in aday and t not in ids:
            ids.append(t)
    # aralik ucu (T301–T322) icin ortadakiler okunmaz; yalniz uclar
    if not ids and not re.search(r"\d", l):
        continue
    S_count += 1
    ref = []
    for t in ids[:14]:
        a = aday[t]
        ref.append("- %s (aday, faz %s, satır %d): %s | kaynak: %s\n  blok başlığı: %s\n  kabul kriterleri ve bağımlılık:\n%s" % (
            t, a["faz"], a["line"], a["ad"], a["kaynak"], blocks[t]["title"] if t in blocks else "YOK",
            "    Bağımlılık: " + field(t, "Bağımlılık") + "\n    " + field(t, "Kabul kriterleri").replace("\n", "\n    ") if t in blocks else ""))
    if len(ids) > 14:
        ref.append("- (ayrıca anılan %d task daha: %s)" % (len(ids) - 14, ", ".join(ids[14:])))
    S.append("### Özet yeri — %s, satır %d\n%s\n\nATIF YAPILAN TASK'LAR:\n%s" % (name, ln, l, "\n".join(ref) if ref else "(task atfı yok; sayı ve kimlik iddiası betikle denetlendi — gerekirse yukarıdaki cümlenin kendi içi)"))
fs = emit("S", HDR_S, S)

# ---- ozet
(OUT / "script_bulgu.txt").write_text("\n".join("SB-%d | %s" % (i + 1, b) for i, b in enumerate(bulgular)) + ("\n" if bulgular else ""), encoding="utf-8")
print("aday %d, blok %d, 7.2 %d, 7.1 satır %d, 7.1 task bağı %d" % (len(aday), len(blocks), len(rows72), total_rows, sum(len(t) for _, t, _, _ in rows71)))
print("özet yeri: S (cümle/tablo satırı) %d · A aday satırı %d · A 7.2 satırı %d · A 7.1 matris satırı (task'a bağlı, tekil) %d" % (
    S_count, len(aday), len(rows72), len({s for s, t, _, _ in rows71 if t})))
print("toplam özet yeri:", S_count + len(aday) + len(rows72) + len({s for s, t, _, _ in rows71 if t}))
print("paket S:", len(fs), "dosya; paket A:", len(fa), "dosya")
print("betik bulgusu:", len(bulgular))
for i, b in enumerate(bulgular):
    print("SB-%d | %s" % (i + 1, b))
