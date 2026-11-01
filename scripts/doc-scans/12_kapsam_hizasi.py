"""Asama 10 acilisi -- kapsam hizasi (A2 madde 1).

Kullanim (depo kokunden): python -I scripts/doc-scans/12_kapsam_hizasi.py . [cikti_dosyasi]

12 henuz sablondur (yazim turu yok); hiza iki yonlu sinanir:
  1  Envanter: 10 2 (KP-n), 10 3 (KD-n), 10 4.1 (OK-n: ust satirlar ile "OK-n" ), SK-n (10 4.2), YH-n (10 5)
     satirlari, grup basliklari ve satir basina K numaralari.
  2  12'nin bugunku metni: bu kimliklerden herhangi birini tasiyor mu (sablon bos; MVP'de olmayan ozellik kriter olamaz).
  3  Konu plani hizasi: her KP satiri tracker 24.2'nin bir konusuna (GROUP_TOPIC) baglanir; baglanmayan KP, KD, YH
     adayi kriter konusuna girmez. OK-n ve SK-n konulari ayridir.
  4  Etki sutunu eslesmesi: KP satirinin K numaralarindan biri etki sutununda `12` tasiyan kararlardan (devir eslemesi)
     biriyse "etkide"; degilse yalniz KP-37'nin satiri 03 0.1.4 ile isaretlidir.
Ciktinin tamami dosyaya yazilir; ekrana yalniz sayim satirlari cikar.
"""
import re
import sys
from pathlib import Path

sys.stdout.reconfigure(encoding="utf-8")
root = Path(sys.argv[1] if len(sys.argv) > 1 else ".")
out_path = sys.argv[2] if len(sys.argv) > 2 else None
buf = []


def p(s=""):
    buf.append(s)


def ps(s):
    buf.append(s)
    print(s)


def expand(text):
    ks = set()
    for m in re.finditer(r"K-(\d+)\s*(?:…|\.\.\.|\.\.)\s*K?-?(\d+)", text):
        a, b = int(m.group(1)), int(m.group(2))
        if a < b and b - a < 400:
            ks.update(range(a, b + 1))
    for m in re.finditer(r"K-(\d+)", text):
        ks.add(int(m.group(1)))
    return ks


mvp = (root / "Docs/10_MVP_SCOPE.md").read_text(encoding="utf-8").split("\n")
doc12 = (root / "Docs/12_VALIDATION_PROTOCOL.md").read_text(encoding="utf-8")
tracker = (root / "Docs/PRODUCT_DISCOVERY_STATUS.md").read_text(encoding="utf-8").split("\n")

# ---- etki sutunu kumesi (12_devir_esle.py ile ayni kural)
CELL_SPLIT = re.compile(r"(?<!\\)\|")
STRICT_RE = re.compile(r"`12`|12_VALIDATION|Validation Protocol|Doğrulama Protokol")
LOOSE_REAL = {1161, 1178, 1179, 1181, 1216, 1217, 1226}
ITEM_RE = re.compile(r"^\*{0,2}12(\s*\(.*|\s*§.*)?\*{0,2}$")


def bare(cell):
    for part in re.split(r" · ", cell):
        for tok in re.split(r",\s+(?![^(]*\))", part):
            if ITEM_RE.match(tok.strip()):
                return True
    return False


a_set = set()
for ln in tracker:
    if not re.match(r"^\| K-\d+ ", ln):
        continue
    cells = CELL_SPLIT.split(ln)
    if len(cells) < 7:
        continue
    n = int(re.match(r"\| K-(\d+)", ln).group(1))
    if STRICT_RE.search(cells[6]) or bare(cells[6]) or n in LOOSE_REAL:
        a_set.add(n)
ps(f"Etki sutununda `12` tasiyan karar: {len(a_set)}")

# ---- 10'un satirlari
ROW = re.compile(r"^\| ((?:KP|KD|YH|ÖK|SK)-\d+) \|")
GROUP = re.compile(r"^\| \| \*\*(.+?)\*\*")
rows = []  # (kimlik, grup, satir_no, k kumesi, metin)
group = None
for i, ln in enumerate(mvp, 1):
    g = GROUP.match(ln)
    if g:
        group = g.group(1)
        continue
    m = ROW.match(ln)
    if m:
        rows.append((m.group(1), group, i, expand(ln), ln))

fam = {}
for r in rows:
    fam.setdefault(r[0].split("-")[0], []).append(r)
for f in ["KP", "KD", "ÖK", "SK", "YH"]:
    ids = sorted(int(r[0].split("-")[1]) for r in fam.get(f, []))
    gaps = [k for k in range(1, max(ids) + 1) if k not in ids] if ids else []
    ps(f"10: {f} satiri {len(ids)} (en buyuk {max(ids) if ids else 0}; bosluk {gaps if gaps else 0})")

# 10'un kendi sayim cumleleri (basliktaki "yetmis alti", "altmis" vb.) ile gercek sayim
for i, ln in enumerate(mvp[:40], 1):
    if "KP-1…KP-76" in ln or "KP-1...KP-76" in ln:
        ps(f"10 satir {i}: ust nottaki cumle KP-1…KP-76 der; gercek KP sayisi {len(fam['KP'])} (KP-77 var)")
        break

# ---- grup -> konu (tracker 24.2)
GROUP_TOPIC = [
    ("Vitrin ve katalog", "VP4-02"),
    ("Sepet, ödeme ve sipariş", "VP4-03"),
    ("Sipariş takibi ve teslim", "VP4-04"),
    ("İptal, cayma, iade ve ayıp talebi", "VP4-05"),
    ("Üyelik ve hesap", "VP4-06"),
    ("Kurumsal içerik ve site", "VP4-07"),
    ("Katalog yönetimi", "VP4-08"),
    ("Sipariş yürütümü", "VP4-09"),
    ("Kurumsal içerik, marka ve site yönetimi", "VP4-07"),
    ("Mağaza ayarları ve yönetim", "VP4-10"),
    ("Bildirimler", "VP4-11"),
    ("Site geneli, güvenlik ve yasal uyum", "VP4-11"),
]
OK_TOPIC = "VP1-04"
SK_TOPIC = "VP1-03"
KD_TOPIC = "VP1-03"
YH_TOPIC = "VP1-03"
KP37_TOPIC = "VP4-12"

topic_count = {}
kp_etkisiz = []
kp_unmapped = []
for kid, grp, ln_no, ks, _ in rows:
    f = kid.split("-")[0]
    if f == "KP":
        t = None
        for key, tid in GROUP_TOPIC:
            if grp and grp.startswith(key):
                t = tid
        if kid == "KP-37":
            t = t  # grup konusu + ★ konu
        if t is None:
            kp_unmapped.append(kid)
            continue
        topic_count.setdefault(t, []).append(kid)
        if not (ks & a_set):
            kp_etkisiz.append((kid, sorted(ks)))
    elif f == "ÖK":
        topic_count.setdefault(OK_TOPIC, []).append(kid)
    elif f == "SK":
        topic_count.setdefault(SK_TOPIC, []).append(kid)
    elif f == "KD":
        topic_count.setdefault(KD_TOPIC, []).append(kid)
    elif f == "YH":
        topic_count.setdefault(YH_TOPIC, []).append(kid)

p("")
p("## Grup -> konu (KP)")
kp_groups = []
for kid, grp, ln_no, ks, _ in rows:
    if kid.startswith("KP-") and grp not in kp_groups:
        kp_groups.append(grp)
for g in kp_groups:
    ids = [r[0] for r in rows if r[0].startswith("KP-") and r[1] == g]
    tid = next((t for key, t in GROUP_TOPIC if g and g.startswith(key)), "?")
    p(f"  {tid}: {g} -- {len(ids)} satir ({ids[0]}..{ids[-1]})")
ps(f"KP gruplari {len(kp_groups)}; konuya baglanmayan KP {len(kp_unmapped)} {kp_unmapped if kp_unmapped else ''}")
ps("KP konu dagilimi: " + " ".join(f"{t}={len([x for x in v if x.startswith('KP-')])}" for t, v in sorted(topic_count.items()) if any(x.startswith('KP-') for x in v)))
ps(f"Diger aileler: VP1-03 (KD {len(fam['KD'])} + SK {len(fam['SK'])} + YH {len(fam['YH'])}); VP1-04 (OK {len(fam['ÖK'])})")

p("")
p("## KP satirlari: K numaralari etki sutunu kumesinde olmayanlar")
ps(f"KP satiri, K numaralarindan hicbiri etki sutunu kumesinde olmayan: {len(kp_etkisiz)}")
for kid, ks in kp_etkisiz:
    p(f"  {kid}: K {ks}")

# ---- 12'nin bugunku metni
p("")
p("## 12'nin metni")
pat = re.compile(r"\b(KP|KD|YH|ÖK|SK)-\d+")
hits = pat.findall(doc12)
ps(f"12: KP/KD/YH/OK/SK kimligi tasiyan anma {len(hits)} (sablon bos; kriter satiri henuz yok)")
ps(f"12: 'birebir' hiza cumlesi {len(re.findall('birebir', doc12))}; P7 kapsam hizalamasi satiri {len(re.findall('Kapsam hizalaması', doc12))}")

# ---- KD/YH kriter yasagi: 12'nin park bloklarinda YH/KD gecen satir
ps("YH adaylari: 10 5 yol haritasi adayi satiri " + str(len(fam["YH"])) + "; 12'de kriter yok (0 anma)")

# ---- K-24: 10 2 ve 3 satirlari cumle olarak tam mi (nokta ile biter)
bad = []
for kid, grp, ln_no, ks, ln in rows:
    if kid.startswith(("KP-", "SK-")):
        cells = CELL_SPLIT.split(ln)
        if len(cells) > 2 and not cells[2].strip().rstrip("*").endswith((".", "…", "”", ")", "\"")):
            bad.append(kid)
ps(f"K-24 (tam cumle): KP/SK satirinda ozellik hucresi noktayla bitmeyen {len(bad)} {bad[:8]}")

# ---- Aciklamalar
p("")
p("## ★ KP-37")
for kid, grp, ln_no, ks, ln in rows:
    if kid == "KP-37":
        p(f"  satir {ln_no}; grup {grp}; K {sorted(ks)}; etkide {bool(ks & a_set)}")
if out_path:
    Path(out_path).write_text("\n".join(buf) + "\n", encoding="utf-8", newline="\n")
