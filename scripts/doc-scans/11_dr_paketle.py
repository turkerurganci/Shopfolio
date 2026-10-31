"""Asama 9 -- `11_IMPLEMENTATION_PLAN.md` kalite dongusu deep review'u: mercek paketleyicisi (KD-deep, Parca A).

Mercekler (her paket en cok 85 KB; satir numarali; `11:<n>|` onek):
  AY   audit'in kararlarinin dogurdugu yeni tutarsizliklar: audit oncesi (--taban) ile simdiki blok farki + soz kayiplari
       (karar kaydi K satirlarindan `Kayip:` cumleleri) + F0 T02/T03/T01 zinciri
  TD   teknik derinlik: faz dilimleri, TAM task bloklari (gelistirici baslayabilir mi, belirsiz ifade, test <-> kabul)
  GV   guvenlik   (OWASP cercevesi)      \\  anahtar sozcuk puanina gore secilen,
  HM   hata modu                          |  sikistirilmis bloklar (secim ilkesi: puan, sonra task sirasi);
  VA   veri akisi                         |  secilen / secilmeyen task sayisi tanilarda yazilir
  OB   olceklenebilirlik + bagimlilik    /

Kullanim: python -I scripts/doc-scans/11_dr_paketle.py <cikti_dizini> [--doc yol] [--taban git-surum] [--sinama]
  --doc     denetlenecek 11 dosyasi (varsayilan: Docs/11_IMPLEMENTATION_PLAN.md)
  --taban   AY farki icin audit oncesi commit (varsayilan 4805b2b = 11 v0.25)
  --sinama  yalniz tanilari yazdirir, paket yazmaz
"""
import difflib
import re
import subprocess
import sys
from collections import defaultdict
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

ROOT = Path(r"C:\projects\Shopfolio")
ARGS = list(sys.argv[1:])
SINAMA = "--sinama" in ARGS


def opt(name, default):
    if name in ARGS:
        i = ARGS.index(name)
        v = ARGS[i + 1]
        del ARGS[i:i + 2]
        return v
    return default


DOC = Path(opt("--doc", str(ROOT / "Docs" / "11_IMPLEMENTATION_PLAN.md")))
TABAN = opt("--taban", "4805b2b")
ARGS = [a for a in ARGS if not a.startswith("--")]
OUT = Path(ARGS[0]) if ARGS else Path("kd-deep-paket")
CAP = 85_000

TIDRX = r"T\d{2,3}[a-z]?(?:\.\d+)?"


def clip(s, n):
    b = s.encode("utf-8")
    return s if len(b) <= n else b[:n].decode("utf-8", "ignore") + " [...kesildi]"


def tid_key(t):
    m = re.match(r"T(\d+)([a-z]?)", t)
    return (int(m.group(1)), m.group(2), t)


def parse_blocks(lines):
    h5 = h6 = None
    for i, l in enumerate(lines):
        if h5 is None and re.match(r"^## 5\. Task listesi", l):
            h5 = i
        if h6 is None and re.match(r"^## 6\. Faz ge", l):
            h6 = i
    if h5 is None or h6 is None:
        raise SystemExit("11 bolum basliklari bulunamadi (bozuk girdi?)")
    starts, faz = [], None
    for i in range(h5, h6):
        m = re.match(r"^### (F\d) ", lines[i])
        if m:
            faz = m.group(1)
        m = re.match(r"^#### (" + TIDRX + r"): ?(.*)$", lines[i])
        if m:
            starts.append((i, m.group(1), m.group(2), faz))
    blocks = {}
    for k, (i, tid, ad, fz) in enumerate(starts):
        end = starts[k + 1][0] if k + 1 < len(starts) else h6
        for j in range(i + 1, end):
            if lines[j].startswith("### ") or lines[j].startswith("## "):
                end = j
                break
        blocks[tid] = dict(start=i, end=end, faz=fz, ad=ad)
    return blocks


LINES = DOC.read_text(encoding="utf-8").split("\n")
BLOCKS = parse_blocks(LINES)
TASKS = sorted(BLOCKS, key=tid_key)
FAZSIRA = sorted({b["faz"] for b in BLOCKS.values() if b["faz"]})


def full(tid):
    b = BLOCKS[tid]
    return "\n".join("11:%d| %s" % (n + 1, LINES[n]) for n in range(b["start"], b["end"]))


def parse_fields(tid):
    b = BLOCKS[tid]
    d = dict(dep="", doc="", crit=[], test="")
    mode = None
    for l in LINES[b["start"] + 1:b["end"]]:
        if l.startswith("- **Bağımlılık:**"):
            d["dep"] = l.split(":**", 1)[1].strip(); mode = None
        elif l.startswith("- **Dokümanlar:**"):
            d["doc"] = l.split(":**", 1)[1].strip(); mode = None
        elif l.startswith("- **Kabul kriterleri:**"):
            mode = "crit"
        elif l.startswith("- **Test beklentisi:**"):
            d["test"] = l.split(":**", 1)[1].strip(); mode = None
        elif l.startswith("- **Doğrulama kontrol listesi:**"):
            mode = None
        elif mode == "crit" and l.strip():
            d["crit"].append(l.strip())
    return d


def condensed(tid, cap=420):
    d = parse_fields(tid)
    b = BLOCKS[tid]
    o = ["11:%d| #### %s: %s  [%s]" % (b["start"] + 1, tid, b["ad"], b["faz"]),
         "   Bağımlılık: %s" % d["dep"], "   Dokümanlar: %s" % clip(d["doc"], 300)]
    o += ["   " + clip(c, cap) for c in d["crit"]]
    o.append("   Test: %s" % clip(d["test"], 300))
    return "\n".join(o)


# ------------------------------------------------------------------ karar kaydi
KAYIT = {}
kp = ROOT / "Docs" / "PRODUCT_DISCOVERY_STATUS.md"
if kp.exists():
    for n, l in enumerate(kp.read_text(encoding="utf-8").split("\n")):
        m = re.match(r"^\| (K-\d+) \|", l)
        if m and m.group(1) not in KAYIT:
            KAYIT[m.group(1)] = (n + 1, l)
SOZ_K = ["K-1459", "K-1462", "K-1463", "K-1464", "K-1465", "K-1467", "K-1468", "K-1469", "K-1470"]


def kayip_cumleleri(k):
    n, l = KAYIT[k]
    if k == "K-1459":
        return ["K:%d| %s" % (n, clip(l, 2400))]
    out = []
    for m in re.finditer(r"Kayıp:.*?(?=Kural kaybı yok|\*\*\(\d\)|\| [A-Z]|$)", l):
        out.append("K:%d| %s" % (n, clip(m.group(0), 4200)))
    if k == "K-1462":
        m = re.search(r"\*\*\(3\) Blok bayt.*?(?=\*\*\(4\))", l)
        if m:
            out.append("K:%d| %s" % (n, clip(m.group(0), 1800)))
    if k == "K-1463":
        out.append("K:%d| %s" % (n, clip(l, 1800)))
    return out


# ------------------------------------------------------------------ taban surum
def taban_satirlari():
    try:
        r = subprocess.run(["git", "-C", str(ROOT), "show", "%s:Docs/11_IMPLEMENTATION_PLAN.md" % TABAN],
                           capture_output=True, text=True, encoding="utf-8", check=True)
        return r.stdout.split("\n")
    except Exception as e:
        print("UYARI: taban okunamadi:", e)
        return None


TL = taban_satirlari()
TB = parse_blocks(TL) if TL else {}
degisen, yeni_blok, silinen = [], [], sorted(set(TB) - set(BLOCKS), key=tid_key)
for t in TASKS:
    if t not in TB:
        yeni_blok.append(t)
        continue
    a = TL[TB[t]["start"]:TB[t]["end"]]
    b = LINES[BLOCKS[t]["start"]:BLOCKS[t]["end"]]
    if a != b:
        degisen.append(t)


def blok_farki(t):
    a = TL[TB[t]["start"]:TB[t]["end"]]
    bs = BLOCKS[t]["start"]
    b = LINES[bs:BLOCKS[t]["end"]]
    o = ["### FARK %s (11:%d)" % (t, bs + 1)]
    sm = difflib.SequenceMatcher(None, a, b, autojunk=False)
    for op, i1, i2, j1, j2 in sm.get_opcodes():
        if op == "equal":
            continue
        for x in a[i1:i2]:
            o.append("  ESKI| " + clip(x, 700))
        for k, x in enumerate(b[j1:j2]):
            o.append("  YENI 11:%d| %s" % (bs + j1 + k + 1, clip(x, 700)))
    return "\n".join(o)


# ------------------------------------------------------------------ yazim
paketler = []
tani = []


def yaz(ad, mercek, govde, ac):
    if not SINAMA:
        OUT.mkdir(parents=True, exist_ok=True)
        (OUT / ("paket_%s.md" % ad)).write_text(govde, encoding="utf-8", newline="\n")
    paketler.append((ad, mercek, len(govde.encode("utf-8")), ac))


def pack(items, header, prefix, mercek, cap=CAP):
    cur, size, n = [], len(header.encode("utf-8")), 1
    hs = len(header.encode("utf-8"))
    for et, tx in items:
        sz = len(tx.encode("utf-8")) + 2
        if cur and size + sz > cap:
            yaz("%s_%02d" % (prefix, n), mercek, header + "\n\n".join(t for _, t in cur) + "\n",
                "%s..%s (%d)" % (cur[0][0], cur[-1][0], len(cur)))
            cur, size, n = [], hs, n + 1
        cur.append((et, tx)); size += sz
    if cur:
        yaz("%s_%02d" % (prefix, n), mercek, header + "\n\n".join(t for _, t in cur) + "\n",
            "%s..%s (%d)" % (cur[0][0], cur[-1][0], len(cur)))


tani.append("11 satir: %d; blok: %d; faz: %s; taban blok: %d; degisen: %d; yeni blok: %d; silinen: %s"
            % (len(LINES), len(BLOCKS), FAZSIRA, len(TB), len(degisen), len(yeni_blok), silinen))
eksik_k = [k for k in SOZ_K if k not in KAYIT]
tani.append("soz kaybi K satirlari bulunamayan: %s" % (eksik_k or "yok"))

# ------------------------------------------------------------------ AY
KAYIP_BLOKLARI = ["T373", "T527", "T372", "T356", "T380", "T374", "T330", "T337", "T338", "T339", "T366", "T507", "T536",
                  "T601", "T602", "T379", "T385", "T521", "T341", "T342", "T306", "T307", "T305", "T319", "T350"]
F0 = ["T02", "T03", "T01", "T04", "T06", "T05", "T08", "T07", "T10", "T09"]
ay_head = ("# AY paketi -- audit'in kararlarinin dogurdugu yeni tutarsizliklar\n"
           "Bak: (a) audit sartnamelerinin eklediklerinin bagli task'larla celiskisi (ayni kavrama farkli kural/sayi/ad; kabul <-> KK <-> test), "
           "(b) SOZ KAYIPLARI: kisaltmada dusen sozun kural/anlam kaybi olup olmadigi (kayip cumlesi + guncel blok yan yana), "
           "(c) F0 yurutme sirasi T02 -> T03 -> T01 -> T04 -> T06 -> T05 -> T08 -> T07 -> T10 -> T09 (K-1470) ve bagimliliklarin buna uyumu. "
           "`ESKI` audit oncesi (11 v0.25), `YENI 11:<n>` guncel satirdir.\n\n")
items = []
items.append(("KAYIP", "## SOZ KAYIPLARI (karar kaydi K satirlari)\n" + "\n".join(
    c for k in SOZ_K if k in KAYIT for c in kayip_cumleleri(k))))
items.append(("KAYIPBLOK", "## KAYIP BLOKLARININ GUNCEL HALI\n" + "\n\n".join(
    "### BLOK %s\n%s" % (t, full(t)) for t in KAYIP_BLOKLARI if t in BLOCKS)))
items.append(("F0", "## F0 YURUTME ZINCIRI (guncel)\n" + "\n".join(condensed(t, 300) for t in F0 if t in BLOCKS)))
kayip_set = set(KAYIP_BLOKLARI) | set(F0)
for t in degisen:
    if t not in kayip_set:
        items.append((t, blok_farki(t)))
for t in yeni_blok:
    items.append((t, "### YENI BLOK %s\n%s" % (t, full(t))))
pack(items, ay_head, "AY", "AY")

# ------------------------------------------------------------------ TD
td_head = ("# TD paketi -- teknik derinlik (faz dilimi, TAM task bloklari)\n"
           "Bak: her task'i okuyan gelistirici baslayabilir mi (belirsiz/olculemez ifade, eksik girdi/cikti, adi gecen ama tanimlanmamis kavram), "
           "test beklentisi kabul kriterini gercekten sinar mi (kriter <-> KK <-> Test beklentisi eslesmesi), task buyuklugu (bir task'ta birden cok is).\n\n")
for grup in (["F0", "F1"], ["F2"], ["F3"], ["F4", "F6"], ["F5"]):
    its = [(t, "### %s\n%s" % (t, full(t))) for t in TASKS if BLOCKS[t]["faz"] in grup]
    if its:
        pack(its, td_head + "FAZ %s\n\n" % "+".join(grup), "TD_" + "".join(grup), "TD")

# ------------------------------------------------------------------ GV / HM / VA / OB
KONU = {
    "GV": ("guvenlik (OWASP Top 10 cercevesi)",
           "Bak: kimlik dogrulama, yetki/rol kontrolu, girdi dogrulama, sir dagitimi, webhook imza dogrulamasi, hiz siniri, oturum/cerez, "
           "enjeksiyon/XSS/CSRF; her biri icin hangi task kuruyor, hangisi sinayan kabul kriteri tasiyor; kuran var sinayan yok ya da tersi.",
           r"parola|şifre|oturum|yetki|JWT|webhook|imza|hız sınır|rate|CSRF|XSS|\bsır|secret|\brol\b|erişim|kimlik doğ|token|HMAC|enjeksiyon|CORS|çerez|cookie|kilit|brute|403|401|izin|güvenlik"),
    "HM": ("hata modu",
           "Bak: dis servis kesintisi (odeme, e-posta, kargo, depolama), yeniden deneme ve ust sinir, idempotency, zaman asimi, yedekten donus, "
           "kismi basarisizlik; task ve kabul kriterlerinde karsiligi var mi, hata davranisi iki task'ta celisiyor mu.",
           r"yeniden dene|retry|idempot|zaman aşım|kesinti|yedek|geri al|geri dön|başarısız|hata|kuyruk|çift|tekrar|fallback|dış servis|sağlayıcı|kurtar|geri yükle|devre"),
    "VA": ("veri akisi",
           "Bak: para tutari, siparis durumu, kisisel veri: olusur -> dogrulanir -> guncellenir -> silinir/arsivlenir; hangi task hangi asamayi kuruyor, "
           "task sirasi akisi bozuyor mu (tuketen, ureticiden once mi), durum/tutar/saklama kurali iki yerde farkli mi.",
           r"tutar|kuruş|KDV|toplam|ödeme|iade|sipariş durum|durum geçiş|silin|silme|arşiv|anonim|kişisel|KVKK|saklama|snapshot|e-posta|adres|fiyat|stok|indirim|kupon"),
    "OB": ("olceklenebilirlik ve bagimlilik riski",
           "Bak: YALNIZ MVP'yi engelleyen ya da ileriyi kapatan kararlar: is kuyrugu, buyuyen tablolar (indeks, sayfalama, temizlik), dis saglayici soyutlamasi "
           "(sozlesme/degistirilebilirlik), tek hata noktasi, onbellek tutarliligi. Farkindalik-duzeyi kaygi bulgu degildir.",
           r"kuyruk|iş kuyruğu|arka plan|indeks|sayfala|önbellek|cache|soyutlama|arayüz|sağlayıcı|adapter|saklama süresi|temizlik|bakım|toplu|batch|zamanlanmış|büyü|limit|sınır"),
}
SKOR = {}
for kod, (ad, bak, rx) in KONU.items():
    r = re.compile(rx, re.I)
    skor = {}
    for t in TASKS:
        b = BLOCKS[t]
        txt = "\n".join(LINES[b["start"]:b["end"]])
        skor[t] = len(r.findall(txt))
    SKOR[kod] = skor
    secil, size = [], 0
    for t in sorted(TASKS, key=lambda x: (-skor[x], tid_key(x))):
        if skor[t] == 0:
            break
        c = condensed(t)
        sz = len(c.encode("utf-8")) + 2
        if size + sz > CAP * (2 if kod in ("GV", "VA") else 1) - 6000:
            break
        secil.append(t); size += sz
    secil.sort(key=tid_key)
    its = [(t, condensed(t)) for t in secil]
    head = ("# %s paketi -- %s\n%s\nSecim: anahtar sozcuk puani > 0 olan en yuksek puanli %d / %d task (sikistirilmis); "
            "secilmeyen task'in tam blogu `11:<n>` ile acilabilir.\n\n" % (kod, ad, bak, len(secil), len(TASKS)))
    pack(its, head, kod, kod)
    tani.append("%s: puan>0 task %d; secilen %d" % (kod, sum(1 for v in skor.values() if v), len(secil)))

liste = ["| Paket | Mercek | Boyut (bayt) | Kapsam |", "|---|---|---|---|"]
for ad, m, sz, ac in paketler:
    liste.append("| %s | %s | %d | %s |" % (ad, m, sz, ac))
if SINAMA:
    print("\n".join(tani))
    print("paket sayisi:", len(paketler), "toplam bayt:", sum(p[2] for p in paketler))
    sys.exit(0)
(OUT / "paket_listesi.md").write_text("\n".join(liste) + "\n\nTANILAR\n" + "\n".join(tani) + "\n", encoding="utf-8", newline="\n")
print("\n".join(liste))
print()
print("\n".join(tani))
agir = [(ad, sz) for ad, m, sz, ac in paketler if sz > CAP]
if agir:
    print("UYARI: sinir asan paket:", agir)
    sys.exit(1)
