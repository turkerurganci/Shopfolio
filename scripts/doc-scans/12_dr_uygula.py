"""Asama 10 -- 12 kalite dongusu audit-2, Parca B:(#d+|Yd+|Zd+) | hukum_*.md'yi 12'ye uygular.

Okunan bicim (KD-audit-2): `## #N |(#d+|Yd+|Zd+) | hukum: x` ve `## Y<n> | hukum: x` basliklari; `ESKI:`/`YENI:` ciftleri;
`SATIR: <VAL> | <hucre> | <yeni>` (tekil olmayan hucre; `P4.n | Durum | ...` park tablosu satiri da olur);
`YENI_SATIR: <aciklama>: | VAL-... |` (anchor VAL'in hemen ardina yeni matris satiri).
Uygulanmaz:(#d+|Yd+|Zd+) | hukum `curuk`, baslikta YUKSELTILDI, `--haric` ile verilenler.
Once hepsi sinanir (ESKI tam bir kez, hucre var ve tek satir, cakisma yok); sorun varsa hicbir sey yazilmaz.
`--yama VAL:hucre=metin` yok; birden fazla bulgunun ayni hucreye yazdigi durumlar `YAMA` sozlugunde elle birlestirilir.
Kullanim: python -I scripts/doc-scans/12_audit_uygula2.py <dizin> [--kuru] [--haric no,no]
"""
import re
import sys
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

args = sys.argv[1:]
kuru = "--kuru" in args
if kuru:
    args.remove("--kuru")
HARIC = set()
if "--haric" in args:
    i = args.index("--haric")
    HARIC = set(args[i + 1].split(","))
    del args[i:i + 2]
D = Path(args[0])
DOC = Path(r"C:\projects\Shopfolio\Docs\12_VALIDATION_PROTOCOL.md")
COL = {"Kural": 2, "Kaynak": 3, "Ön koşul": 4, "Beklenen": 5, "Beklenen sonuç": 5, "Kanıt türü": 6,
       "Kanıt ortamı": 7, "Severity": 8, "Seviye": 8, "Durum": 9}

# Ayni hucreyi birden fazla bulgu degistiriyorsa birlesik metin: (VAL, hucre) -> metin
YAMA = {
    # #86 SATIR + #85 ESKI/YENI ayni hucrede: birlesik metin
    ("VAL-D092", "Ön koşul"): 'Beş tipte birer kapatılmış talep; hiç kapatılmamış birer açık "Genel soru" ve "Sipariş hakkında" talebi (başlangıcı açılış — 02 §4.2 Z-33, §3.32.4); kapatılıp yeniden açılan ve ikinci kez kapatılan birer "Genel soru" ve "Sipariş hakkında" talebi (başlangıcı son kapatılış — 02 §4.2 Z-33, §3.32.4); test saati her talebin başlangıcından iki yıl ve on yıl dolmadan bir gün önce ve dolduktan sonraki ilk gece imhasından sonra (05 §7.4.1), yeniden açılan iki talepte ayrıca ilk kapatılışından iki yıl ve on yıl',
    ("VAL-D092", "Beklenen sonuç"): 'İki yıldan bir gün önce bütün talepler durur; iki yıl dolduktan sonraki ilk gece imhasından sonra dört tipin kapatılmış talebi ve açık "Genel soru" talebi silinir, kapatılmış ve açık "Sipariş hakkında" talepleri durur; on yıldan bir gün önce onlar da durur, dolduktan sonraki ilk gece imhasından sonra silinir; yeniden açılan "Genel soru" talebi ilk kapatılışından iki yıl, yeniden açılan "Sipariş hakkında" talebi ilk kapatılışından on yıl dolduğunda durur (süre ikinci kapatılıştan sayılır; ilk kapatılıştan sayılsaydı silinmiş olurdu) ve ikinci kapatılışından iki ve on yıl dolunca silinir',
}
SKIP_ESKI = {
    "test saati her talebin başlangıcından iki yıl ve on yıl",
    'İki yılda dört tip ve açık "Genel soru" talebi silinir, kapatılmış ve açık "Sipariş hakkında" talepleri durur; on yılda onlar da silinir',
}
PAIR_OVR = {}

# Yeni VAL kimligi cakismasi (D3, D4, D5, D9, D17 hepsi A980 onerdi): dosya -> [(eski, yeni)] metin degisimi;
# hukum_3 (Referrer) ile hukum_4 (A981 Referrer) ayni bosluk: hukum_3 kalir, hukum_4 A981 satiri dusulur.
KIMLIK = {
    "hukum_10.md": [("VAL-A<sıradaki>", "VAL-A986")],
    "hukum_3.md": [("VAL-A980", "VAL-A981")],
    "hukum_5.md": [("VAL-A980", "VAL-A983")],
    "hukum_9.md": [("VAL-A981", "VAL-A985"), ("VAL-A980", "VAL-A984")],
    "hukum_17.md": [("VAL-A981", "VAL-A988"), ("VAL-A980", "VAL-A987")],
}
DUSUR_SATIR = {"hukum_4.md": ["| VAL-A981 | A |"]}
# Yeni satir cipalari: yeni kimlik -> anchor kimligi
ANCHOR = {
    "F015": "F012", "F016": "A975", "D121": "D116", "D122": "D121",
    "A980": "A773", "A981": "A980", "A982": "A981", "A983": "A931",
    "A984": "A831", "A985": "A829", "A986": "A052", "A987": "A985", "A988": "A834",
}
ANCHOR = {k: "VAL-" + v for k, v in ANCHOR.items()}

raw = DOC.read_bytes().decode("utf-8")
crlf = "\r\n" in raw
lines = raw.replace("\r\n", "\n").split("\n")
TXT = "\n".join(lines)

hdr = re.compile(r"^## (#\d+|Y\d+|Z\d+) \| hukum: (\w+)(.*)$")
pairs, cells, newrows = [], [], []
for f in sorted(D.glob("hukum_*.md"), key=lambda p: int(re.search(r"(\d+)", p.stem).group(1))):
    cur, ok, eski = None, False, None
    ftxt = f.read_text(encoding="utf-8")
    for a_, b_ in KIMLIK.get(f.name, []):
        ftxt = ftxt.replace(a_, b_)
    flines = [x for x in ftxt.split("\n") if not any(x.startswith(d) for d in DUSUR_SATIR.get(f.name, []))]
    yrow = False
    for l in flines:
        if yrow and l.startswith("| VAL-"):
            for r in re.findall(r"\| VAL-[A-Z]\d+ \|.*?\| BEKLEMEDE \|", l):
                newrows.append((cur, ANCHOR[re.match(r"\| VAL-([A-Z]\d+)", r).group(1)], r))
            continue
        yrow = False
        m = hdr.match(l)
        if m:
            cur = m.group(1)
            ok = m.group(2).lower() in ("dogru", "kismen") and "YUKSELTILDI" not in m.group(3) \
                and cur.lstrip("#") not in HARIC
            eski = None
            continue
        if not ok:
            continue
        if l.startswith("ESKI:"):
            eski = l.split(":", 1)[1].strip()
        elif l.startswith("YENI:") and eski is not None:
            pairs.append((cur, eski, l.split(":", 1)[1].strip()))
            eski = None
        elif l.startswith("SATIR:") and l[6:].strip() != "yok":
            p = [x.strip() for x in l[6:].split(" | ", 2)]
            if len(p) < 3:
                continue
            cells.append((cur, p[0], p[1], p[2]))
        elif l.startswith("YENI_SATIR:"):
            yrow = True
            for r in re.findall(r"\| VAL-[A-Z]\d+ \|.*?\| BEKLEMEDE \|", l):
                newrows.append((cur, ANCHOR[re.match(r"\| VAL-([A-Z]\d+)", r).group(1)], r))
            if l[len("YENI_SATIR:"):].strip() == "yok":
                pass

seen = set()
pairs = [p for p in pairs if not ((p[1], p[2]) in seen or seen.add((p[1], p[2])))]
pairs = [(a, b_, PAIR_OVR.get(b_, c)) for a, b_, c in pairs if b_ not in SKIP_ESKI]
sorun, log = [], []
# 1) ESKI/YENI
araliklar = []
for no, eski, yeni in pairs:
    n = TXT.count(eski)
    if n != 1:
        sorun.append("%s: ESKI %d kez: %.70s" % (no, n, eski))
        continue
    if eski == yeni:
        sorun.append("%s: YENI == ESKI" % no)
    p = TXT.index(eski)
    araliklar.append((p, p + len(eski), no))
araliklar.sort()
for a, b in zip(araliklar, araliklar[1:]):
    if b[0] < a[1]:
        sorun.append("cakisan aralik: %s ve %s" % (a[2], b[2]))

# 2) hucre duzeltmeleri (ESKI/YENI sonrasi metin uzerinde degil, satir kimligiyle)
idx = {}


def satir_bul(kim):
    if kim not in idx:
        if kim.startswith("P4."):
            idx[kim] = [i for i, l in enumerate(lines) if l.startswith("| %s " % kim)]
        else:
            idx[kim] = [i for i, l in enumerate(lines) if l.startswith("| %s | " % kim)]
    return idx[kim]


gorulen = set()
yazilacak = []
for no, kim, hucre, yeni in cells:
    h = (kim, hucre)
    if h in gorulen:
        sorun.append("%s: %s %s birden fazla kez" % (no, kim, hucre))
    gorulen.add(h)
    s = satir_bul(kim)
    if len(s) != 1:
        sorun.append("%s: %s %d satir" % (no, kim, len(s)))
        continue
    if hucre not in COL:
        sorun.append("%s: bilinmeyen hucre %s" % (no, hucre))
        continue
    yazilacak.append((no, kim, hucre, YAMA.get(h, yeni), s[0]))
for (kim, hucre) in YAMA:
    if (kim, hucre) not in gorulen:
        sorun.append("YAMA kullanilmadi: %s %s" % (kim, hucre))

# ESKI/YENI araliginin SATIR ile yazilan hucreyle cakismasi (ESKI bir SATIR hucresinin icindeyse YAMA'da elle birlestirilmis olmali)
for a, b, no in araliklar:
    ln = TXT.count("\n", 0, a)
    for no2, kim, hucre, yeni, i in yazilacak:
        if i == ln and TXT[a:b] in lines[i].split(" | ")[COL[hucre] if not kim.startswith("P4.") else 3]:
            sorun.append("ESKI/YENI %s ile SATIR %s %s ayni satirda (elle birlestirilmeli)" % (no, kim, hucre))

# 3) yeni satirlar
newrows.sort(key=lambda x: (x[2].split(" ")[1][4], int(x[2].split(" ")[1][5:])))
yeni_idler = {r[2].split(" ")[1][4:] for r in newrows}
REF = [l for l in lines if l.startswith("| VAL-A052 | ")][0].count(" | ")
for no, anchor, row in newrows:
    if anchor[4:] not in yeni_idler:
        s = satir_bul(anchor)
        if len(s) != 1:
            sorun.append("%s: anchor %s %d satir" % (no, anchor, len(s)))
    yid = row.split(" ")[1]
    if satir_bul(yid):
        sorun.append("%s: %s zaten var" % (no, yid))
    if row.count(" | ") != REF:
        sorun.append("%s: hucre sayisi %d != %d" % (no, row.count(" | "), REF))

print("cift: %d; hucre: %d; yeni satir: %d; sorun: %d" % (len(pairs), len(yazilacak), len(newrows), len(sorun)))
print("yeni:", " ".join(r[2].split(" ")[1] + "<" + r[1][4:] + "(" + r[0] + ")" for r in newrows))
for s in sorun:
    print(" -", s)
if sorun or kuru:
    sys.exit(1 if sorun else 0)

# uygula: once hucreler (satir bazli), sonra ESKI/YENI (metin bazli), sonra yeni satirlar
for no, kim, hucre, yeni, i in yazilacak:
    c = lines[i].split(" | ")
    if kim.startswith("P4."):
        eski = c[3]
        c[3] = yeni + " |"
    else:
        eski = c[COL[hucre]]
        c[COL[hucre]] = yeni + (" |" if COL[hucre] == 9 else "")
    lines[i] = " | ".join(c)
    log.append("| %s | %s | %s | %s |" % (no, kim, hucre, eski[:50].replace("|", "/")))
out = "\n".join(lines)
for no, eski, yeni in pairs:
    assert out.count(eski) == 1, no
    out = out.replace(eski, yeni, 1)
    log.append("| %s | (ESKI/YENI) | | %s |" % (no, eski[:50].replace("|", "/")))
L2 = out.split("\n")
for no, anchor, row in newrows:
    j = [i for i, l in enumerate(L2) if l.startswith("| %s | " % anchor)][0]
    L2.insert(j + 1, row)
    log.append("| %s | YENI_SATIR | %s | %s sonrasi |" % (no, row.split(" | ")[0][2:], anchor))
out = "\n".join(L2)
if crlf:
    out = out.replace("\n", "\r\n")
DOC.write_bytes(out.encode("utf-8"))
(D / "uygulanan.md").write_text("# Uygulanan (KD-audit-2 Parca B)\n\n| Bulgu | Satir | Hucre | Eski (ilk 50) |\n|---|---|---|---|\n"
                                + "\n".join(log) + "\n", encoding="utf-8", newline="\n")
print("uygulandi")
