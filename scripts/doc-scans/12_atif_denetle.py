#!/usr/bin/env python3
"""12_VALIDATION_PROTOCOL.md icindeki `0N §x.y` bolum atiflarinin hedef dokumanda var olusunu sinar.

Kullanim: python -I scripts/doc-scans/12_atif_denetle.py <depo-koku> [--ayrinti]

Atif bicimi: `05 §4.5.3`, `04 §9.9.20`; ayni dokumana ardisik atif (`04 §2`, §3) onceki
dokumanu devralir (aradaki ayirac yalniz virgul, noktali virgul, ve, ..., bosluk ise).
Doku numarasi bulunamayan ciplak § atiflari (kendi dokumanina ya da belirsiz) atlanir ve sayilir.

Hedefte bolum numarasi su yerlerden biri olarak aranir (satir basinda, onekler soyulduktan sonra):
  - baslik:       `## 4.5`, `#### 4.5.3 Baslik`
  - govde basligi: `**4.5.3 Baslik.**`, `- **9.9.20 Baslik**`, `- 5.1.13 Baslik`, `| 6.3.9.8 |`
Numara yalnizca onu izleyen karakter bosluk, nokta, iki nokta, yildiz ya da `|` ise sayilir.
Cikis kodu: eksik hedef varsa 1, yoksa 0.
"""
import glob
import os
import re
import sys

ONEK = re.compile(r"^\s*(?:#+\s+|>\s*|[-*]\s+|\|\s*)*(?:\*\*)?")
NUM = re.compile(r"^(\d+(?:\.\d+)*)(?=[\s.:*|)]|$)")
REF = re.compile(r"§\s*(\d+(?:\.\d+)*)")
DOC_BEFORE = re.compile(r"(?<![\d\-A-Za-z.])(0[0-9]|1[01])`?(?:'[^\s`]{0,6})?\s*`?\s*$")
AYIRAC = re.compile(r"^[\s,`()….]*(?:ve|ile|ya da)?[\s,`()….]*$")


def hedef_numaralari(yol):
    kume = set()
    baslik = set()
    with open(yol, encoding="utf-8") as f:
        for satir in f:
            ham = satir.rstrip("\n")
            m = ONEK.match(ham)
            kalan = ham[m.end():]
            n = NUM.match(kalan)
            if n:
                kume.add(n.group(1))
                if ham.startswith("#"):
                    baslik.add(n.group(1))
    return kume, baslik


def main():
    if hasattr(sys.stdout, "reconfigure"):
        sys.stdout.reconfigure(encoding="utf-8")
    if len(sys.argv) < 2:
        print(__doc__)
        return 2
    kok = sys.argv[1]
    ayrinti = "--ayrinti" in sys.argv
    docs = {}
    for yol in glob.glob(os.path.join(kok, "Docs", "[0-1][0-9]_*.md")):
        no = os.path.basename(yol)[:2]
        docs[no] = hedef_numaralari(yol)
    kaynak = os.path.join(kok, "Docs", "12_VALIDATION_PROTOCOL.md")
    toplam = 0
    atlanan = 0
    eksik = []
    yalniz_govde = 0
    with open(kaynak, encoding="utf-8") as f:
        for no_satir, satir in enumerate(f, 1):
            onceki_doc = None
            onceki_son = 0
            for m in REF.finditer(satir):
                oncesi = satir[max(0, m.start() - 24):m.start()]
                d = DOC_BEFORE.search(oncesi)
                doc = d.group(1) if d else None
                if doc is None and onceki_doc is not None and AYIRAC.match(satir[onceki_son:m.start()]):
                    doc = onceki_doc
                onceki_doc = doc
                onceki_son = m.end()
                if doc is None or doc == "12" or doc not in docs:
                    atlanan += 1
                    continue
                num = m.group(1)
                toplam += 1
                kume, baslik = docs[doc]
                if num not in kume:
                    eksik.append((no_satir, doc, num))
                elif num not in baslik:
                    yalniz_govde += 1
    print("Atif (belge numarali) sayisi: %d; belge numarasi cozulemeyen/kendi belgesi atlanan: %d" % (toplam, atlanan))
    print("Hedefi yalniz govde basligi (kalin/madde) olarak bulunan: %d" % yalniz_govde)
    print("Hedefi bulunamayan atif: %d" % len(eksik))
    gor = set()
    for no_satir, doc, num in eksik:
        anahtar = (doc, num)
        if ayrinti or anahtar not in gor:
            print("  12 satir %d -> %s §%s yok" % (no_satir, doc, num))
        gor.add(anahtar)
    print("Hata: %d" % len(gor))
    return 1 if eksik else 0


if __name__ == "__main__":
    sys.exit(main())
