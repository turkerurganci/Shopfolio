"""Asama 9 -- ozet <-> ayrinti taramasi: dogrulayici karar dosyalarini (karar_<n>.txt) birlestirir.

`B-<n> | <karar> | <gerekce> | <duzeltme>` bloklarini okur (bos satirla ayrilmis; bazi dosyalar
`**B-n**` ya da paket oneki kullanir). Cikti:
  <dizin>/karar_ozet.txt   -- her bulgunun karari (grup, id, karar, gerekce ilk 160 karakter)
  <dizin>/duzeltmeler.txt  -- yalniz DOGRULANDI olanlarin duzeltme metinleri (uygulama girdisi)
  <dizin>/hukme_kalan.txt  -- HUKME KALDI olanlar (gerekce tam)
stdout: karar sayilari.

Kullanim: python -I scripts/doc-scans/11_ozet_ayrinti_karar_derle.py <dizin>
"""
import re
import sys
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

D = Path(sys.argv[1])
START = re.compile(r"^\W*(?:bulgu_[AS]_\d+\W+|[AS]_\d+\W+)?(B-\d+)(?:\W*\([^)]*\))?\W*\|", re.I)
res = []
for f in sorted(D.glob("karar_[0-9]*.txt"), key=lambda p: int(re.search(r"_(\d+)\.txt", p.name).group(1))):
    g = int(re.search(r"_(\d+)\.txt", f.name).group(1))
    cur = None
    for ln in f.read_text(encoding="utf-8").split("\n"):
        if START.match(ln):
            if cur:
                res.append(cur)
            cur = [g, ln]
        elif cur is not None:
            cur[1] += "\n" + ln
    if cur:
        res.append(cur)
cnt = {"DOĞRULANDI": 0, "ÇÜRÜDÜ": 0, "HÜKME KALDI": 0, "?": 0}
ozet, duz, hk = [], [], []
for g, txt in res:
    flat = txt.replace("\n", " ")
    m = re.search(r"(DOĞRULANDI|ÇÜRÜDÜ|HÜKME KALDI)", flat[:80])
    k = m.group(1) if m else "?"
    cnt[k] += 1
    bid = START.match(txt.split("\n")[0]).group(1)
    ozet.append("g%d %s %s" % (g, bid, k))
    if k == "DOĞRULANDI":
        duz.append("### g%d %s\n%s" % (g, bid, txt))
    elif k == "HÜKME KALDI":
        hk.append("### g%d %s\n%s" % (g, bid, txt))
(D / "karar_ozet.txt").write_text("\n".join(ozet) + "\n", encoding="utf-8")
(D / "duzeltmeler.txt").write_text("\n\n".join(duz) + "\n", encoding="utf-8")
(D / "hukme_kalan.txt").write_text("\n\n".join(hk) + "\n", encoding="utf-8")
print(len(res), cnt)
