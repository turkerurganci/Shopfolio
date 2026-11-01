"""Asama 10 -- 12 kalite dongusu deep review dogrulama 2a, Parca B: hukum_*.md'yi 12'ye uygular.

Bicim: `## #N | hukum: x` basliklari; ESKI:/YENI: ciftleri; `SATIR: <VAL> | <hucre> | <yeni>`;
`YENI_SATIR: <konum aciklamasi>: | VAL-<L>-YENI | ... | BEKLEMEDE |` (anchor = aciklamadaki ilk VAL kimligi;
yeni kimlik o seviyenin son numarasindan sirayla verilir).
Uygulanmaz: hukum `curuk`. `[guvenlik]` sayim cifti (`[guvenlik]` <n>) atlanir; 12:225 elle yeniden sayilir.
Kullanim: python -I scripts/doc-scans/12_dr_uygula2.py <dizin> [--kuru]
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
D = Path(args[0])
DOC = Path(r"C:\projects\Shopfolio\Docs\12_VALIDATION_PROTOCOL.md")
COL = {"Kural": 2, "Kaynak": 3, "Ön koşul": 4, "Beklenen": 5, "Beklenen sonuç": 5, "Kanıt türü": 6,
       "Kanıt ortamı": 7, "Severity": 8, "Seviye": 8, "Durum": 9}

raw = DOC.read_bytes().decode("utf-8")
crlf = "\r\n" in raw
lines = raw.replace("\r\n", "\n").split("\n")
TXT = "\n".join(lines)

hdr = re.compile(r"^## (#\d+) \| hukum: (\w+)(.*)$")
pairs, cells, newrows = [], [], []
for f in sorted(D.glob("hukum_*.md"), key=lambda p: int(re.search(r"(\d+)", p.stem).group(1))):
    cur, ok, eski = None, False, None
    for l in f.read_text(encoding="utf-8").split("\n"):
        m = hdr.match(l)
        if m:
            cur = m.group(1)
            ok = m.group(2).lower() in ("dogru", "kismen")
            eski = None
            continue
        if not ok:
            continue
        if l.startswith("ESKI:"):
            eski = l.split(":", 1)[1].strip()
        elif l.startswith("YENI:") and eski is not None:
            if not eski.startswith("`[güvenlik]` ") and eski != "Bu işlemlerin ekranının ve gönderiminin sahipliği sipariş kökündendir":
                pairs.append((cur, eski, l.split(":", 1)[1].strip()))
            eski = None
        elif l.startswith("SATIR:") and l[6:].strip() not in ("", "yok"):
            p = [x.strip() for x in l[6:].split(" | ", 2)]
            if len(p) == 3:
                cells.append((cur, p[0], p[1], p[2]))
        elif l.startswith("YENI_SATIR:") and l[11:].strip() not in ("", "yok"):
            body = l[11:].strip()
            k = body.index("| VAL-")
            a = re.search(r"VAL-[A-F]\d+", body[:k]).group(0)
            row = body[k:]
            lv = re.match(r"\| VAL-([A-F])-YENI \|", row).group(1)
            newrows.append((cur, a, lv, row))

seen = set()
pairs = [p for p in pairs if not ((p[1], p[2]) in seen or seen.add((p[1], p[2])))]
sorun, log = [], []
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

idx = {}


def satir_bul(kim):
    if kim not in idx:
        idx[kim] = [i for i, l in enumerate(lines) if l.startswith("| %s | " % kim)]
    return idx[kim]


gorulen, yazilacak = set(), []
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
    yazilacak.append((no, kim, hucre, yeni, s[0]))

for a, b, no in araliklar:
    ln = TXT.count("\n", 0, a)
    for no2, kim, hucre, yeni, i in yazilacak:
        if i == ln and TXT[a:b] in lines[i].split(" | ")[COL[hucre]]:
            sorun.append("ESKI/YENI %s ile SATIR %s %s ayni satirda (elle birlestirilmeli)" % (no, kim, hucre))

son = {}
for lv in "ABCDEF":
    son[lv] = max(int(re.match(r"\| VAL-%s(\d+) " % lv, l).group(1)) for l in lines if re.match(r"\| VAL-%s\d+ " % lv, l))
REF = lines[satir_bul("VAL-A052")[0]].count(" | ")
atanan = []
for no, anchor, lv, row in newrows:
    son[lv] += 1
    yid = "VAL-%s%03d" % (lv, son[lv])
    row = row.replace("VAL-%s-YENI" % lv, yid, 1)
    if len(satir_bul(anchor)) != 1:
        sorun.append("%s: anchor %s %d satir" % (no, anchor, len(satir_bul(anchor))))
    if row.count(" | ") != REF:
        sorun.append("%s: hucre sayisi %d != %d" % (no, row.count(" | "), REF))
    atanan.append((no, anchor, yid, row))

print("cift: %d; hucre: %d; yeni satir: %d; sorun: %d" % (len(pairs), len(yazilacak), len(atanan), len(sorun)))
print("yeni:", " ".join("%s<%s(%s)" % (y, a, n) for n, a, y, r in atanan))
for s in sorun:
    print(" -", s)
if sorun or kuru:
    sys.exit(1 if sorun else 0)

for no, kim, hucre, yeni, i in yazilacak:
    c = lines[i].split(" | ")
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
eklenen = {}
for no, anchor, yid, row in atanan:
    j = [i for i, l in enumerate(L2) if l.startswith("| %s | " % anchor)][0] + eklenen.get(anchor, 0)
    L2.insert(j + 1, row)
    eklenen[anchor] = eklenen.get(anchor, 0) + 1
    log.append("| %s | YENI_SATIR | %s | %s sonrasi |" % (no, yid, anchor))
out = "\n".join(L2)
if crlf:
    out = out.replace("\n", "\r\n")
DOC.write_bytes(out.encode("utf-8"))
(D / "uygulanan.md").write_text("# Uygulanan (deep-2a Parca B)\n\n| Bulgu | Satir | Hucre | Eski (ilk 50) |\n|---|---|---|---|\n"
                                + "\n".join(log) + "\n", encoding="utf-8", newline="\n")
print("uygulandi")
