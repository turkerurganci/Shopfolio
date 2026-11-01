"""Asama 10 -- `12` audit'i (KD-audit, Parca A): dogrulayici talimatlarini ve dogrulanmadi listesini uretir.

Girdi : <dizin>/birlesik.md (12_audit_birlestir.py), <dizin>/grup_plani.txt
        grup_plani.txt satiri: `<n>: #3` ya da `<n>: #30,#31,#32` (n = dogrulayici no; Yuksek tek, Orta 2-4'lu, Dusuk 5-8'li)
Cikti : <dizin>/dogrulama_<n>.md (talimat: bulgu bloklari + sabit hukum bicimi), <dizin>/dogrulanmadi.md
Sinir : plandaki satir sayisi (dogrulayici) en cok 20 (INSTRUCTIONS s9 (1): adimin toplami).

Kullanim: python -I scripts/doc-scans/12_audit_grupla.py <dizin>
"""
import re
import sys
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

D = Path(sys.argv[1])
MAX_VERIFIER = 20
txt = (D / "birlesik.md").read_text(encoding="utf-8")
blocks = {}
cur = None
for l in txt.split("\n"):
    m = re.match(r"^## #(\d+) \| seviye: (\w+)", l)
    if m:
        cur = int(m.group(1))
        blocks[cur] = dict(sev=m.group(2), lines=[l])
    elif cur is not None:
        blocks[cur]["lines"].append(l)

plan = []
for l in (D / "grup_plani.txt").read_text(encoding="utf-8").split("\n"):
    l = l.strip()
    if not l or l.startswith("#!"):
        continue
    m = re.match(r"^(\d+):\s*(.+)$", l)
    if not m:
        continue
    plan.append((int(m.group(1)), [int(x) for x in re.findall(r"#(\d+)", m.group(2))]))
if len(plan) > MAX_VERIFIER:
    raise SystemExit("plan %d dogrulayici: sinir %d asildi" % (len(plan), MAX_VERIFIER))
used = [i for _, ids in plan for i in ids]
if len(used) != len(set(used)):
    raise SystemExit("bir bulgu iki grupta")
missing = [i for i in used if i not in blocks]
if missing:
    raise SystemExit("birlesik.md'de olmayan bulgu: %s" % missing)

FORMAT = """# Dogrulayici talimati -- Shopfolio Asama 10, `12_VALIDATION_PROTOCOL.md` audit'i (dogrulayici {n})

Sen BAGIMSIZ bir dogrulayicisin. Asagidaki bulgular Sonnet mercekleri tarafindan uretildi; mercegin iddiasina GUVENME. Her bulguyu GUNCEL metne karsi yeniden oku
ve hukum ver. Dokumani DEGISTIRME, commit/PR yapma; yalniz hukum dosyasini yaz.

## Okuma kurali
- `12` dosyasi: `C:/projects/Shopfolio/Docs/12_VALIDATION_PROTOCOL.md` (740 KB: ASLA bastan sona okuma; `grep -n` ve `Read` offset/limit ile ilgili satirlari ac).
- Kaynak dokumanlar `C:/projects/Shopfolio/Docs/02_...`-`11_...md`; karar kaydi `C:/projects/Shopfolio/Docs/PRODUCT_DISCOVERY_STATUS.md` (5 MB: yalniz `grep -n "^| K-1234 |"`).
- Kaynak kazanir (K-652): `12` tuketicidir; kaynak ile 12 celisirse duzeltme 12'dedir. Kaynagin kendisi hatali/belirsizse hukum `kismen` ve `UST:` notu.
- Kabukta heredoc, here-string, `python -c`, `node -e` YOK. Betik gerekirse `Write` ile yaz.

## Hukum dosyasi
Yaz: `C:/Users/teknodeveloper/.claude/helper-state/shopfolio/a10-kd-audit/hukum_{n}.md`. Her bulgu icin SABIT bicim (betik okur; serbest yazma):

```
## #N | hukum: dogru|kismen|curuk
Gerekce: 1-3 cumle, kanit satiri (12:<n>| ... <-> NN:<n>| ...)
Seviye: Yuksek|Orta|Dusuk   (yukseltiyorsan `Seviye: ... YUKSELTILDI` yaz; yeniden dogrulama bu adimda yapilmaz)
ESKI: <12'deki metin PARCASI, TEK SATIR ICINDE, dosyada TAM BIR KEZ gecen, birebir>
YENI: <yerine gelecek metin; ayni satir icinde>
ESKI: ... (gerekirse ek cift; ayni bulgu icin birden cok cift olabilir; her ESKI dosyada tam bir kez gecmeli)
YENI: ...
UST: <varsa ust dokumanda (NN) gereken duzeltme: dosya ve yer, tek cumle>
YAN: <ayni kalibin baska yerleri: VAL kimlikleri/satir; ayri dogrulayiciya gidecek>
YINE_DE_SORULUR: <yalniz karar gerektiriyorsa: tam soru; yoksa "yok">
```
- `curuk` ise ESKI/YENI yazma, yalniz Gerekce.
- ESKI'yi satirdan `grep -c -F` ile sina: tam bir kez gecmeli (tablo hucresindeki tam ifadeyi sec; kisa ve benzersiz). `|` karakteri ESKI icinde olmasin; tablo satirinin bir hucresini degistiriyorsan hucre icindeki ifadeyi kullan.
- YENI Turkce, dokumanin diliyle ve satirin bicimiyle uyumlu; yeni sayi/sure UYDURMA: yalniz kaynaktan al, atif ver (`NN §x.y`). Sayim/ozet yerleri etkilenirse (12 s1 sayimlari, park tablosu) `YAN:` ile bildir.
- Karar gerektiren (ürün karari, KP degisimi, "Yine de sorulur" olcutu) madde uygulanmaz: `YINE_DE_SORULUR`.
- Donus metni EN FAZLA 5 satir: dogrulayici no, bulgu sayisi, hukum dagilimi (dogru/kismen/curuk), yukseltilen bulgu, hukum dosyasinin yolu.

## Bulgular ({k})
"""

for n, ids in plan:
    body = FORMAT.format(n=n, k=", ".join("#%d" % i for i in ids))
    for i in ids:
        body += "\n" + "\n".join(blocks[i]["lines"]).rstrip() + "\n"
    (D / ("dogrulama_%d.md" % n)).write_text(body, encoding="utf-8", newline="\n")

rest = sorted(set(blocks) - set(used), key=lambda i: (["Yuksek", "Orta", "Dusuk"].index(blocks[i]["sev"]), i))
out = ["# Dogrulanmadi -- sonraki adim (audit-2) girdisi\n",
       "Doğrulayıcı sınırı (adim toplami %d) nedeniyle Opus doğrulayıcıya gitmeyen bulgular: %d (Yuksek %d, Orta %d, Dusuk %d).\n"
       "Bunlar DOKUMANA UYGULANMAZ (doğrulanmamış bulgu uygulanmaz). Merceğin ham bulgusu aşağıdadır.\n" % (
           MAX_VERIFIER, len(rest), *[sum(1 for i in rest if blocks[i]["sev"] == s) for s in ("Yuksek", "Orta", "Dusuk")])]
for i in rest:
    out.append("\n".join(blocks[i]["lines"]).rstrip() + "\n")
(D / "dogrulanmadi.md").write_text("\n".join(out), encoding="utf-8", newline="\n")
print("dogrulayici: %d; dogrulanan bulgu: %d; dogrulanmadi: %d" % (len(plan), len(used), len(rest)))
