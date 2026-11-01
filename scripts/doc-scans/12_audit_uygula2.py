"""Asama 10 -- 12 kalite dongusu audit-2, Parca B: hukum_*.md'yi 12'ye uygular.

Okunan bicim (KD-audit-2): `## #N | hukum: x` ve `## Y<n> | hukum: x` basliklari; `ESKI:`/`YENI:` ciftleri;
`SATIR: <VAL> | <hucre> | <yeni>` (tekil olmayan hucre; `P4.n | Durum | ...` park tablosu satiri da olur);
`YENI_SATIR: <aciklama>: | VAL-... |` (anchor VAL'in hemen ardina yeni matris satiri).
Uygulanmaz: hukum `curuk`, baslikta YUKSELTILDI, `--haric` ile verilenler.
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
       "Severity": 8, "Seviye": 8, "Durum": 9}

# Ayni hucreyi birden fazla bulgu degistiriyorsa birlesik metin: (VAL, hucre) -> metin
YAMA = {
    ("VAL-D089", "Beklenen sonuç"):
        "Bir gün önce satırlar alıcısıyla ve olayın değerleriyle durur (üç yılda boşaltılmamıştır); sonra silinir; "
        "iptal ve geri ödeme kolu süreyi kısaltmaz ve geç gelip geri ödenen ödemenin siparişi Başarısız kalsa da "
        "ödemesi alınmış kolundadır — üç yılda alıcısı silinmemiştir",
}

raw = DOC.read_bytes().decode("utf-8")
crlf = "\r\n" in raw
lines = raw.replace("\r\n", "\n").split("\n")
TXT = "\n".join(lines)

hdr = re.compile(r"^## (#\d+|Y\d+) \| hukum: (\w+)(.*)$")
pairs, cells, newrows = [], [], []
for f in sorted(D.glob("hukum_*.md"), key=lambda p: int(re.search(r"(\d+)", p.stem).group(1))):
    cur, ok, eski = None, False, None
    for l in f.read_text(encoding="utf-8").split("\n"):
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
            cells.append((cur, p[0], p[1], p[2]))
        elif l.startswith("YENI_SATIR:"):
            body = l.split(":", 1)[1].strip()
            a = re.search(r"(VAL-[A-Z]\d+)['’]\w+ hemen ardına|(VAL-[A-Z]\d+)['’]\w+ hemen ardina", body)
            row = body[body.index("| VAL-"):]
            newrows.append((cur, a.group(1) or a.group(2), row))

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
for no, anchor, row in newrows:
    s = satir_bul(anchor)
    if len(s) != 1:
        sorun.append("%s: anchor %s %d satir" % (no, anchor, len(s)))
    m = re.match(r"\| (VAL-[A-Z]\d+) \|", row)
    if satir_bul(m.group(1)):
        sorun.append("%s: %s zaten var" % (no, m.group(1)))
    if row.count(" | ") != lines[s[0]].count(" | "):
        sorun.append("%s: hucre sayisi %d != %d" % (no, row.count(" | "), lines[s[0]].count(" | ")))

print("cift: %d; hucre: %d; yeni satir: %d; sorun: %d" % (len(pairs), len(yazilacak), len(newrows), len(sorun)))
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
