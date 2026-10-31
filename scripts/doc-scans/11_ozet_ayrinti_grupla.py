"""Asama 9 -- ozet <-> ayrinti taramasi: okuyucu bulgularini birlestirir ve kademeli dogrulama
grup dosyalari uretir (Yuksek tek, Orta 4'lu, Dusuk 8'li; INSTRUCTIONS 9 (9)).

Girdi: <dizin>/bulgu_*.txt (okuyucu ciktilari). Cikti: <dizin>/hepsi.txt, <dizin>/elenen.txt
(betikle elenen: `09` bolum anmasi Dusuk bulgular -- Dokumanlar alani okuma tablosundan
bolum alir, 7.2 matris kaynagidir; Task yapisi konvansiyon 4), <dizin>/dogrulama_<n>.md.

Kullanim: python -I scripts/doc-scans/11_ozet_ayrinti_grupla.py <dizin>
"""
import re
import sys
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

D = Path(sys.argv[1])
rows = []
for f in sorted(D.glob("bulgu_*.txt"), key=lambda p: (p.name[6], int(re.search(r"_(\d+)\.txt", p.name).group(1)))):
    for ln in f.read_text(encoding="utf-8").split("\n"):
        if ln.startswith("B-"):
            rows.append((f.stem[6:], ln.strip()))
kept, dropped = [], []
for pk, ln in rows:
    sev = ln.rsplit("|", 1)[-1].strip()
    if sev == "Düşük" and "| blok |" in ln and "`09`" in ln:
        dropped.append((pk, ln))
    else:
        kept.append((pk, ln, sev))
(D / "hepsi.txt").write_text("\n".join("%s|%s" % (p, l) for p, l in rows) + "\n", encoding="utf-8")
(D / "elenen.txt").write_text("\n".join("%s|%s" % (p, l) for p, l in dropped) + "\n", encoding="utf-8")

HDR = """# Doğrulama grubu {n} ({sev}, {k} bulgu)

Sen bağımsız bir doğrulayıcısın. Depo: C:/projects/Shopfolio. Aşağıdaki bulgular `Docs/11_IMPLEMENTATION_PLAN.md`'nin özet yerleri ile ayrıntı yerleri arasında bir okuyucunun bulduğu iddia edilen farklardır. Okuyucu hatalı olabilir; **her bulguyu kendin sına**. Dosya 687 KB'tır — **bütün okuma**; yalnız ilgili satırları oku:
- task bloğu: `grep -n -A 30 '^#### T123: ' Docs/11_IMPLEMENTATION_PLAN.md`
- aday tablo ve 7.2 satırı: `grep -n '^| T123 |' Docs/11_IMPLEMENTATION_PLAN.md`
- 7.1 matris satırı (kaynak kimliği ilk hücre): `grep -n '^| 02:§3.8.3' ...` (kaynak kimliği biçimi: `06:§4.17 Order`, `07 UC-12`, `04:E-05`, `02:Z-24`, `05:MK-12`, `09:§10`, `10:KP-5`, `devir:K-1356`)
- kaynak doküman: `Docs/0N_*.md` (başlığı grep'le, sonra `sed -n 'a,bp'`); kimlik ya da kural metni orada **gerçek** ne diyor?
Ölçüt: kaynak doküman doğrudur; 7.1 ve task bloğu onun özetleridir. 7.2 tablosu 7.1'den betikle türetilir (`11_geri_yon.py --yaz`), elle düzeltilmez: 7.2'deki bir eksik gerçekse düzeltme 7.1 satırının "Task" hücresindedir. Bir task bloğu en çok 2.560 bayt ve 10 kabul kriteri taşır.

Her bulgu için şu üç karardan birini ver:
- **DOĞRULANDI** — fark gerçek ve **metin düzeyinde** düzeltilir (yanlış sayı/ad/atıf/kimlik, eksik atıf, 7.1'e eksik task bağı, KK sayısı uyumsuzluğu). Tam düzeltmeyi yaz: dosya, satır, eski metin → yeni metin (blok içi düzeltme 2.560 bayt sınırını aşmamalı; sığmıyorsa HÜKME KALDI de).
- **ÇÜRÜDÜ** — kaynak ve blok incelenince fark yok ya da fark anlamlı değil (neden: bir cümle).
- **HÜKME KALDI** — düzeltme kapsam kararı gerektirir (yeni kabul kriteri, task bölme, matris eşlemesini yeniden kurma, kaynağın kendisi belirsiz). Tercih ettiğin çözümü bir cümleyle öner.

Çıktıyı Write ile `{out}` dosyasına yaz; her bulgu için bir blok: `B-<id> | <karar> | <gerekçe> | <DOĞRULANDI ise: düzeltme (dosya:satır, eski → yeni)>`. Kaynak dosyalara dokunma. Kabukta heredoc, here-string, python -c, node -e kullanma. Dönüşte yalnız karar sayılarını yaz.

## Bulgular
"""


def write(n, sev, items):
    out = (D / ("karar_%d.txt" % n)).as_posix()
    body = "\n".join("%s %s" % (p, l) for p, l, _ in items)
    (D / ("dogrulama_%d.md" % n)).write_text(HDR.format(n=n, sev=sev, k=len(items), out=out) + body + "\n", encoding="utf-8")


n = 0
groups = []
for sev, size in (("Yüksek", 1), ("Orta", 4), ("Düşük", 8)):
    items = [k for k in kept if k[2] == sev]
    for i in range(0, len(items), size):
        n += 1
        write(n, sev, items[i:i + size])
        groups.append((n, sev, len(items[i:i + size])))
print("okuyucu bulgusu:", len(rows), "elenen (09 Düşük):", len(dropped), "doğrulamaya:", len(kept))
print("grup:", len(groups), {s: sum(1 for g in groups if g[1] == s) for s in ("Yüksek", "Orta", "Düşük")})
