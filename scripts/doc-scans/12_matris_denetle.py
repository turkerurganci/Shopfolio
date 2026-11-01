"""Asama 10 yazim -- 12 dogrulama matrisi denetimi (VP4-01; K-1521).

Kullanim (depo kokunden):
  python -I scripts/doc-scans/12_matris_denetle.py . --kapsam 1-30,77 [--cikti dosya]

--kapsam: bu oturumda kapsamasi zorunlu KP numaralari (aralik ve tek numara, virgulle).
  Kapsam verilmezse yalniz satir ve kimlik denetimleri calisir (KP kapsamasi sinanmaz).
  Matrisin tamami bitince kapsam 1-77 olur (Y3b'nin sonu).

Sinananlar (12'nin `| VAL-` ile baslayan satirlari):
  1  Sutun sayisi 10 ve sablon (bos Kural) satiri yok.
  2  Kimlik `VAL-<A-F><NNN>` ve tekil; Seviye sutunu kimlikteki harfle ayni.
  3  Kanit ortami {mock, sandbox, gercek, yok}; Severity {KRITIK, ORTA, DUSUK}; Durum 4 deger.
  4  Kural hucresi bos degil; bir tur etiketi varsa dortten biri ve hucrenin basinda.
  5  Kaynak hucresi KP, OK ya da K kimligi tasir; KD, SK ve YH kimligi tasimaz; KP kimlikleri 10'da var.
  6  Kanit turu 7.1'in tur adlarindan; KRITIK satirda en az iki farkli tur.
  7  On kosul bos degil.
  8  Kapsam: verilen KP kumesinin her biri en az bir satirin Kaynak hucresinde.
Ekrana sayim ve hatalar yazilir; --cikti verilirse ayrintili dokum dosyaya.
"""
import re
import sys
from pathlib import Path

sys.stdout.reconfigure(encoding="utf-8")

args = sys.argv[1:]
root = Path(args[0]) if args and not args[0].startswith("--") else Path(".")
kapsam_arg = None
cikti = None
for i, a in enumerate(args):
    if a == "--kapsam" and i + 1 < len(args):
        kapsam_arg = args[i + 1]
    if a == "--cikti" and i + 1 < len(args):
        cikti = args[i + 1]

CELL_SPLIT = re.compile(r"(?<!\\)\|")
DEGER_ORTAM = {"mock", "sandbox", "gerçek", "yok"}
DEGER_SEV = {"KRİTİK", "ORTA", "DÜŞÜK"}
DEGER_DURUM = {"BEKLEMEDE", "PASS", "FAIL", "KABUL EDİLMİŞ RİSK"}
ETIKETLER = {"[tarayıcı]", "[erişilebilirlik]", "[güvenlik]", "[yük]"}
KANIT_TURLERI = {
    "Uç yanıtı", "Tarayıcı gözlemi", "Veri kaydı", "Uygulama kaydı", "İş kaydı",
    "Test raporu", "Dış sistem kaydı", "İzleme/uyarı", "Runbook kaydı",
}


def parse_kapsam(s):
    kume = set()
    for part in s.split(","):
        part = part.strip()
        if not part:
            continue
        if "-" in part:
            a, b = part.split("-", 1)
            kume.update(range(int(a), int(b) + 1))
        else:
            kume.add(int(part))
    return kume


doc12 = (root / "Docs/12_VALIDATION_PROTOCOL.md").read_text(encoding="utf-8").split("\n")
mvp = (root / "Docs/10_MVP_SCOPE.md").read_text(encoding="utf-8").split("\n")

kp_var = set()
for ln in mvp:
    m = re.match(r"^\| KP-(\d+) \|", ln)
    if m:
        kp_var.add(int(m.group(1)))

hatalar = []
rows = []
for no, ln in enumerate(doc12, 1):
    if ln.startswith("| VAL-"):
        cells = [c.strip() for c in CELL_SPLIT.split(ln)[1:-1]]
        rows.append((no, cells))


def hata(no, kimlik, mesaj):
    hatalar.append(f"satir {no} {kimlik}: {mesaj}")


gorulen = {}
kapsanan = set()
etiket_say = {}
seviye_say = {}
seviye_etiket = {}
sev_say = {}
for no, c in rows:
    if len(c) != 10:
        hata(no, c[0] if c else "?", f"sutun sayisi {len(c)} (10 olmali)")
        continue
    kimlik, seviye, kural, kaynak, onkosul, beklenen, kanit, ortam, sev, durum = c
    m = re.fullmatch(r"VAL-([A-F])(\d{3})", kimlik)
    if not m:
        hata(no, kimlik, "kimlik VAL-<A-F><NNN> bicimine uymuyor")
        continue
    if kimlik in gorulen:
        hata(no, kimlik, f"kimlik tekil degil (ilk: satir {gorulen[kimlik]})")
    gorulen[kimlik] = no
    if seviye != m.group(1):
        hata(no, kimlik, f"Seviye sutunu '{seviye}' kimlikteki harfle ({m.group(1)}) ayni degil")
    if ortam not in DEGER_ORTAM:
        hata(no, kimlik, f"Kanit ortami '{ortam}' gecersiz")
    if sev not in DEGER_SEV:
        hata(no, kimlik, f"Severity '{sev}' gecersiz")
    if durum not in DEGER_DURUM:
        hata(no, kimlik, f"Durum '{durum}' gecersiz")
    if not kural:
        hata(no, kimlik, "Kural hucresi bos (sablon satiri)")
    if not onkosul:
        hata(no, kimlik, "On kosul bos")
    if not beklenen:
        hata(no, kimlik, "Beklenen sonuc bos")
    # etiket
    tag = None
    if kural.startswith("["):
        em = re.match(r"^(\[[^\]]+\])", kural)
        tag = em.group(1) if em else None
        if tag not in ETIKETLER:
            hata(no, kimlik, f"tur etiketi '{tag}' gecersiz")
    if tag:
        etiket_say[tag] = etiket_say.get(tag, 0) + 1
        seviye_etiket[(tag, m.group(1))] = seviye_etiket.get((tag, m.group(1)), 0) + 1
    seviye_say[m.group(1)] = seviye_say.get(m.group(1), 0) + 1
    sev_say[sev] = sev_say.get(sev, 0) + 1
    # kaynak
    if re.search(r"\b(KD|SK|YH)-\d+", kaynak):
        hata(no, kimlik, "Kaynak KD/SK/YH kimligi tasiyor (kriter yazilmaz)")
    kps = {int(x) for x in re.findall(r"KP-(\d+)(?!\d)", kaynak)}
    if not (kps or re.search(r"ÖK-\d+|K-\d+|Ö\d+|\b\d{2} §\d", kaynak)):
        hata(no, kimlik, "Kaynak KP, OK ya da K kimligi tasimiyor")
    for k in kps:
        if k not in kp_var:
            hata(no, kimlik, f"Kaynak'taki KP-{k} 10'da yok")
    kapsanan |= kps
    # kanit
    turler = [t.strip() for t in kanit.split("+") if t.strip()]
    for t in turler:
        if t not in KANIT_TURLERI:
            hata(no, kimlik, f"kanit turu '{t}' 7.1'de yok")
    if sev == "KRİTİK" and len(set(turler)) < 2:
        hata(no, kimlik, "KRITIK satirda en az iki farkli kanit turu gerekir (7.2)")

print(f"Matris satiri: {len(rows)}; kimlik tekil: {len(gorulen)}")
print("Seviye dagilimi: " + " ".join(f"{k}={v}" for k, v in sorted(seviye_say.items())))
print("Severity dagilimi: " + " ".join(f"{k}={v}" for k, v in sorted(sev_say.items())))
print("Tur etiketi: " + (" ".join(f"{k}={v}" for k, v in sorted(etiket_say.items())) or "yok"))

eksik = []
if kapsam_arg:
    kume = parse_kapsam(kapsam_arg)
    for k in sorted(kume):
        if k not in kp_var:
            hatalar.append(f"kapsam: KP-{k} 10'da yok")
        elif k not in kapsanan:
            eksik.append(k)
    print(f"Kapsam: {len(kume)} KP istendi; kapsanan {len(kume) - len(eksik)}; eksik {len(eksik)} {['KP-%d' % k for k in eksik] if eksik else ''}")
    for k in eksik:
        hatalar.append(f"kapsam: KP-{k} hicbir satirin Kaynak hucresinde degil")
else:
    print("Kapsam argumani verilmedi; KP kapsamasi sinanmadi")

print(f"Hata: {len(hatalar)}")
for h in hatalar[:60]:
    print("  " + h)
if cikti:
    buf = [f"Matris satiri {len(rows)}", f"Seviye {seviye_say}", f"Etiket-seviye {seviye_etiket}", f"Hata {len(hatalar)}"] + hatalar
    Path(cikti).write_text("\n".join(buf) + "\n", encoding="utf-8", newline="\n")
sys.exit(1 if hatalar else 0)
