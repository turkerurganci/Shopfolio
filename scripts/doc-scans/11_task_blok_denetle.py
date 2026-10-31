"""Asama 9 -- `11 5` task bloklarinin denetimi (yazim turu; karar kaydi 22.2 IP5-03;
K-1406 (5), K-1409, K-1443...K-1445).

`11 5` icindeki her `#### TXX: <ad>` blogunu denetler. Henuz task blogu yoksa bos gecer
(cikis 0). Denetlenenler:
  - blok basligi `#### TXX: <ad>`; kimlik `11 7` aday task tablosunda tanimli; blok, kimligin
    fazinin `### Fn — <ad>` basligi altinda; fazin icinde bloklar kimlik sirasiyla (artan);
    ayni kimlik iki kez yazilmaz; baslik `<` `>` yer tutucusu tasimaz;
  - blok boyutu en cok 2.560 bayt (2,5 KB; K-1406 (4)) -- baslik satirindan bir sonraki
    basliga kadar, UTF-8;
  - bes alan, bu sirayla ve bir kez: Bagimlilik, Dokumanlar, Kabul kriterleri,
    Test beklentisi, Dogrulama kontrol listesi; hicbiri bos degil;
  - Bagimlilik `—` ya da kimlik listesi; her kimlik aday tabloda tanimli, kendisi degil ve
    ayni fazdan (K-1408: fazlar arasi zincir gate ile saglanir, bagimlilik alani onu tekrar
    yazmaz);
  - Dokumanlar `çekirdek` sozcugunu tasir (K-1406 (5): `09` cekirdek yedi bolumu her
    satirda anilir);
  - Kabul kriterleri 1...10 numarali madde (`  1. ...`);
  - Test beklentisi degerleri Unit, Integration, Contract, E2E, Yok; `Yok` tek basina ve
    ` — gerekce` ile;
  - Dogrulama kontrol listesi en az kabul kriteri sayisi kadar madde (`  - ...`);
  - K atiflari: `11` govdesindeki her `K-<n>` karar kaydinda var;
  - Yurutme sirasi satiri (`**Yürütme sırası:** T301 → T302 → ...`, fazin basliginin altinda):
    varsa fazin bloklarinin permutasyonudur ve fazin icindeki her bagimlilik kendinden
    once gelir; faz tamamen yazilmissa (aday tablodaki her kimlik blok olmussa) satir
    zorunludur;
  - `--tam` verilirse yazilmamis aday task da sorundur (yazim turunun bitisi).

Kullanim (depo kokunden): python -I scripts/doc-scans/11_task_blok_denetle.py . [cikti] [--tam]
Cikis kodu: sorun sifirdan buyukse 1.
"""
import re
import sys
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

args = [a for a in sys.argv[1:] if not a.startswith("--")]
TAM = "--tam" in sys.argv
root = Path(args[0] if args else ".")
out_path = args[1] if len(args) > 1 else None

MAKS_BAYT = 2560
ALANLAR = ["Bağımlılık", "Dokümanlar", "Kabul kriterleri", "Test beklentisi",
           "Doğrulama kontrol listesi"]
TEST_DEGER = {"Unit", "Integration", "Contract", "E2E", "Yok"}
ID_RE = r"T\d+[a-z]?"

out = []
sorun = []


def p(s=""):
    out.append(s)
    print(s)


def bad(msg):
    sorun.append(msg)
    p("SORUN: " + msg)


def key(tid):
    m = re.match(r"T(\d+)([a-z]?)$", tid)
    return (int(m.group(1)), m.group(2))


doc = (root / "Docs" / "11_IMPLEMENTATION_PLAN.md").read_text(encoding="utf-8").split("\n")
kayit = (root / "Docs" / "PRODUCT_DISCOVERY_STATUS.md").read_text(encoding="utf-8")
kayit_k = {int(m) for m in re.findall(r"(?m)^\| K-(\d+) \|", kayit)}

# --- aday task tablosu (11 7) ---
aday = {}
for l in doc:
    m = re.match(r"\| (%s) \| (F[0-6]) \|" % ID_RE, l)
    if m:
        aday[m.group(1)] = m.group(2)
faz_aday = {}
for t, f in aday.items():
    faz_aday.setdefault(f, set()).add(t)

# --- 11 5 sinirlari ---
bas = bit = None
for i, l in enumerate(doc):
    if l.startswith("## 5. "):
        bas = i
    elif bas is not None and l.startswith("## ") and bit is None:
        bit = i
if bas is None:
    bad("`## 5.` basligi bulunamadi")
    bas, bit = 0, 0
bit = bit if bit is not None else len(doc)

# --- bloklari ayikla (kod citi disinda) ---
bloklar = []  # dict(id, ad, faz_baslik, satirlar, satir_no)
faz_baslik = None
faz_sira = {}  # faz -> yurutme sirasi satiri (liste) ya da None
cur = None
fence = False
for i in range(bas, bit):
    l = doc[i]
    if l.startswith("```"):
        fence = not fence
        if cur is not None:
            cur["satirlar"].append(l)
        continue
    if fence:
        if cur is not None:
            cur["satirlar"].append(l)
        continue
    m = re.match(r"### (F[0-6]) — ", l)
    if m:
        faz_baslik = m.group(1)
        cur = None
        continue
    if l.startswith("### ") or l.startswith("## ") or l.startswith("---"):
        cur = None
        if l.startswith("### "):
            faz_baslik = None
        continue
    m = re.match(r"#### (T\d+[a-z]?(?:\.\d+)?): (.*)$", l)
    if m:
        cur = {"id": m.group(1), "ad": m.group(2).strip(), "faz_baslik": faz_baslik,
               "satirlar": [l], "no": i + 1}
        bloklar.append(cur)
        continue
    if l.startswith("#"):
        cur = None
        continue
    ys = re.match(r"\*\*Yürütme sırası:\*\* (.*)$", l)
    if ys and faz_baslik and cur is None:
        faz_sira[faz_baslik] = re.findall(ID_RE, ys.group(1))
        continue
    if cur is not None:
        cur["satirlar"].append(l)

p("11 §5 task blok denetimi — aday tablosu: %d task; §5 blok: %d" % (len(aday), len(bloklar)))

gorulen = {}
for b in bloklar:
    tid = b["id"]
    et = "%s (satır %d)" % (tid, b["no"])
    if tid in gorulen:
        bad("%s: kimlik ikinci kez yazılmış (ilki satır %d)" % (et, gorulen[tid]))
    gorulen[tid] = b["no"]
    if tid not in aday:
        bad("%s: kimlik aday task tablosunda (§7) tanımlı değil" % et)
    elif b["faz_baslik"] != aday[tid]:
        bad("%s: blok %s başlığı altında ama aday tabloda fazı %s" % (et, b["faz_baslik"], aday[tid]))
    if "<" in b["ad"] or ">" in b["ad"] or not b["ad"]:
        bad("%s: başlıkta yer tutucu ya da boş ad" % et)
    govde = "\n".join(b["satirlar"]).rstrip("\n") + "\n"
    boyut = len(govde.encode("utf-8"))
    if boyut > MAKS_BAYT:
        bad("%s: blok %d bayt (en çok %d)" % (et, boyut, MAKS_BAYT))

    # alanlar
    alan_metin = {}
    sira = []
    cura = None
    for l in b["satirlar"][1:]:
        m = re.match(r"- \*\*([^*:]+):\*\*(.*)$", l)
        if m:
            cura = m.group(1)
            sira.append(cura)
            alan_metin[cura] = [m.group(2)]
        elif cura is not None:
            alan_metin[cura].append(l)
    if sira != ALANLAR:
        bad("%s: alanlar %s; beklenen sıra %s" % (et, sira, ALANLAR))
    for a in ALANLAR:
        if a in alan_metin and not "".join(alan_metin[a]).strip():
            bad("%s: '%s' alanı boş" % (et, a))

    # bagimlilik
    if "Bağımlılık" in alan_metin:
        bm = " ".join(alan_metin["Bağımlılık"]).strip()
        ids = re.findall(r"\b%s\b" % ID_RE, bm)
        if not ids and not bm.startswith("—"):
            bad("%s: Bağımlılık '—' ya da kimlik listesi olmalı" % et)
        for d in ids:
            if d == tid:
                bad("%s: kendine bağımlı" % et)
            elif d not in aday:
                bad("%s: bağımlılık %s aday tabloda tanımsız" % (et, d))
            elif tid in aday and aday[d] != aday[tid]:
                bad("%s: bağımlılık %s başka fazda (%s; yalnız aynı fazdaki task yazılır, %s)"
                    % (et, d, aday[d], "önceki fazlar gate ile karşılanır"))
        b["bag"] = ids
    # dokumanlar
    if "Dokümanlar" in alan_metin and "çekirdek" not in " ".join(alan_metin["Dokümanlar"]):
        bad("%s: Dokümanlar 'çekirdek' (09 çekirdek yedi bölüm) sözcüğünü taşımıyor" % et)
    # kabul kriterleri
    kn = 0
    if "Kabul kriterleri" in alan_metin:
        kn = sum(1 for l in alan_metin["Kabul kriterleri"] if re.match(r"\s{2,}\d+\. \S", l))
        if kn < 1 or kn > 10:
            bad("%s: kabul kriteri sayısı %d (1…10)" % (et, kn))
    # test beklentisi
    if "Test beklentisi" in alan_metin:
        tm = " ".join(alan_metin["Test beklentisi"]).strip()
        deger_kismi, _, gerekce = tm.partition(" — ")
        degerler = [x.strip() for x in deger_kismi.split(",") if x.strip()]
        if not degerler or any(x not in TEST_DEGER for x in degerler):
            bad("%s: Test beklentisi değerleri %s (kümesi: %s)" % (et, degerler, sorted(TEST_DEGER)))
        if "Yok" in degerler and (len(degerler) > 1 or not gerekce.strip()):
            bad("%s: 'Yok' tek başına ve ' — gerekçe' ile yazılır" % et)
    # kontrol listesi
    if "Doğrulama kontrol listesi" in alan_metin:
        vn = sum(1 for l in alan_metin["Doğrulama kontrol listesi"] if re.match(r"\s{2,}- \S", l))
        if vn < max(kn, 1):
            bad("%s: kontrol listesi %d madde, kabul kriteri %d (en az o kadar)" % (et, vn, kn))

# --- fazlar: sira, yurutme sirasi ---
for f in sorted(faz_aday):
    yazilan = [b for b in bloklar if b["faz_baslik"] == f and b["id"] in aday]
    ids = [b["id"] for b in yazilan]
    if ids != sorted(ids, key=key):
        bad("%s: bloklar kimlik sırasıyla (artan) değil" % f)
    tam_yazildi = faz_aday[f] <= set(ids)
    ys = faz_sira.get(f)
    if tam_yazildi and ys is None:
        bad("%s: faz tamamen yazılmış ama 'Yürütme sırası' satırı yok" % f)
    if ys is not None:
        if sorted(ys) != sorted(ids) or len(set(ys)) != len(ys):
            bad("%s: Yürütme sırası fazın bloklarının permütasyonu değil (sırada %d, blok %d)"
                % (f, len(ys), len(ids)))
        else:
            konum = {t: i for i, t in enumerate(ys)}
            for b in yazilan:
                for d in b.get("bag", []):
                    if d in konum and konum[d] > konum[b["id"]]:
                        bad("%s: Yürütme sırasında %s, bağımlı olduğu %s'den önce" % (f, b["id"], d))
    yz = len(ids)
    p("%s: %d / %d task yazıldı%s" % (f, yz, len(faz_aday[f]),
                                      "" if yz == len(faz_aday[f]) else " (yazılmamış: %d)" % (len(faz_aday[f]) - yz)))
    if TAM and yz != len(faz_aday[f]):
        bad("%s: --tam: yazılmamış aday task %d" % (f, len(faz_aday[f]) - yz))

# --- K atiflari ---
eksikk = set()
for l in doc:
    for n in re.findall(r"\bK-(\d+)\b", l):
        if int(n) not in kayit_k:
            eksikk.add(int(n))
for n in sorted(eksikk):
    bad("K-%d karar kaydında yok" % n)
p("K atıfları: kayıtta %d K; eksik %d" % (len(kayit_k), len(eksikk)))

p("")
p("TOPLAM SORUN: %d" % len(sorun))
if out_path:
    Path(out_path).write_text("\n".join(out) + "\n", encoding="utf-8", newline="\n")
sys.exit(1 if sorun else 0)
