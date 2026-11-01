"""Asama 10 -- `12_VALIDATION_PROTOCOL.md` kalite dongusu deep review'u: mercek paketleyicisi (A10-KD-deep, Parca A).

`12_audit_paketle.py` kapsam/dogruluk/cerceve/seviye/negatif/terim/olculebilirlik/ic tutarlilik mercekleriydi (skill deep-review
Kurallar 7: audit'ten sonra katman 1-2 yeniden tam kosmaz). Bu betik KALITE katmanlarini (3-8) ve audit hukumlerinin dokundugu yerleri paketler:

  AY   audit kararlari : K-1549..K-1555'in dokundugu 12 satirlari (v0.15 -> guncel fark) + devreden YAN notlari + kaynak
  TD   teknik derinlik : 12 s4 + s12 dilimleri (satir + duzyazi) ve s5-s11 + 11 s6 + skills/validate (kapi isler mi)
  GV   guvenlik        : OWASP Top 10 cercevesi; anahtar sozcukle secilen satirlar + kategori envanteri
  HM   hata modu       : kesinti/yeniden deneme/idempotency/zaman asimi/yedekten donus; 12 s4.16, s12.6, s12.11
  VA   veri akisi      : para tutari, durum, kisisel veri; 12 s12.3, s12.4, s12.7
  OB   olceklenebilirlik + bagimlilik riski : 12 s12.12, s4.16, 08 s3

Kullanim: python -I scripts/doc-scans/12_dr_paketle.py <cikti_dizini> [--sinama] [--doc yol]
  --sinama  yalniz tanilari yazdirir, paket yazmaz (iki yonlu sinama: bilinen bag ornegi + bozulmus girdi bayragi)
"""
import re
import subprocess
import sys
from collections import Counter, defaultdict
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

ROOT = Path(r"C:\projects\Shopfolio")
HS = Path(r"C:\Users\teknodeveloper\.claude\helper-state\shopfolio")
V015 = "6ac4c72"
ARGS = list(sys.argv[1:])
SINAMA = "--sinama" in ARGS
if "--doc" in ARGS:
    i = ARGS.index("--doc")
    DOC = Path(ARGS[i + 1])
    del ARGS[i:i + 2]
else:
    DOC = ROOT / "Docs" / "12_VALIDATION_PROTOCOL.md"
ARGS = [a for a in ARGS if not a.startswith("--")]
OUT = Path(ARGS[0]) if ARGS else Path("kd-deep-paket")
CAP = 85_000
CAP_K = 700

LINES = DOC.read_text(encoding="utf-8").split("\n")
N = len(LINES)


def clip(s, n):
    b = s.encode("utf-8")
    if len(b) <= n:
        return s
    return b[:n].decode("utf-8", "ignore") + " [...kesildi]"


def bsz(s):
    return len(s.encode("utf-8"))


def cells(line):
    return [c.strip() for c in line.strip().strip("|").split(" | ")]


def find_line(rx, start=0, end=None):
    r = re.compile(rx)
    for i in range(start, end or N):
        if r.search(LINES[i]):
            return i
    return None


H = {k: find_line(r"^## %d\. " % k) for k in range(1, 13)}
if None in (H[1], H[3], H[4], H[5], H[8], H[12]):
    raise SystemExit("12 bolum basliklari bulunamadi (bozuk girdi?)")

ROWS = []
cur = ""
for i, l in enumerate(LINES):
    m = re.match(r"^#{2,3} (\d+(?:\.\d+)*)\.? ", l)
    if m:
        cur = m.group(1)
    if l.startswith("| VAL-"):
        c = cells(l)
        if len(c) != 10:
            print("UYARI: sutun sayisi %d: 12:%d" % (len(c), i + 1))
            continue
        ROWS.append(dict(line=i + 1, id=c[0], lvl=c[1], kural=c[2], kaynak=c[3], onkosul=c[4], beklenen=c[5],
                         kanit=c[6], ortam=c[7], sev=c[8], durum=c[9], raw=l, sec=cur))
BYID = {r["id"]: r for r in ROWS}
BYLINE = {r["line"]: r for r in ROWS}


def fmt(a, b):
    return "\n".join("12:%d| %s" % (n + 1, LINES[n]) for n in range(a, b))


def sec_range(code):
    """`### 12.6 ...` / `## 5. ...` bolum kodu -> (baslangic idx, bitis idx)."""
    s = find_line(r"^###? %s\.?\s" % re.escape(code))
    if s is None:
        raise SystemExit("bolum yok: " + code)
    lvl = len(re.match(r"^(#+)", LINES[s]).group(1))
    e = next((i for i in range(s + 1, N) if re.match(r"^#{1,%d} " % lvl, LINES[i])), N)
    return s, e


def crow(r, kc=300, bc=300, oc=200):
    return "12:%d| %s|%s|K:%s|S:%s|Ort:%s|Kanıt:%s|Ön:%s|Bek:%s" % (
        r["line"], r["id"], r["sev"], clip(r["kural"], kc), clip(r["kaynak"], 80), r["ortam"], clip(r["kanit"], 50),
        clip(r["onkosul"], oc), clip(r["beklenen"], bc))


# ------------------------------------------------------------------ kaynak dokumanlari
DOCS = {}
for p in sorted((ROOT / "Docs").glob("[0-1][0-9]_*.md")):
    if p.name[:2] != "12":
        DOCS[p.name[:2]] = p.read_text(encoding="utf-8").split("\n")
HEAD_RX = re.compile(r"^(#{1,6}) (.*)$")
IDX = {}
for dn, ls in DOCS.items():
    ix = defaultdict(list)
    for i, l in enumerate(ls):
        m = HEAD_RX.match(l)
        if m:
            txt = m.group(2)
            mn = re.match(r"^(?:§)?(\d+(?:\.\d+)*)\.?\s", txt + " ")
            if mn:
                ix[mn.group(1)].append(("h", i))
            for t in re.findall(r"\b(E-\d+|OB-\d+|UC-\d+|MK-\d+|KP-\d+|KD-\d+|TM\d+-\d+)\b", txt):
                ix[t].append(("h", i))
            continue
        mb = re.match(r"^(?:- )?\*\*(\d+(?:\.\d+)+)[ .]", l)
        if mb:
            ix[mb.group(1)].append(("b", i))
            continue
        mr = re.match(r"^\|\s*`?([A-Z]{1,3}-\d+|\d+(?:\.\d+)+)`?\s*\|", l)
        if mr:
            ix[mr.group(1)].append(("r", i))
    IDX[dn] = ix
KAYIT = {}
rxk = re.compile(r"^\| (K-\d+) \|")
for n, l in enumerate((ROOT / "Docs" / "PRODUCT_DISCOVERY_STATUS.md").read_text(encoding="utf-8").split("\n")):
    m = rxk.match(l)
    if m and m.group(1) not in KAYIT:
        KAYIT[m.group(1)] = (n + 1, l)


def sect_text(dn, i, cap):
    ls = DOCS[dn]
    out = [(i + 1, ls[i])]
    for j in range(i + 1, min(len(ls), i + 400)):
        if HEAD_RX.match(ls[j]):
            break
        out.append((j + 1, ls[j]))
    return clip("\n".join("%s:%d| %s" % (dn, n, t) for n, t in out), cap)


def para_text(dn, i, cap):
    ls = DOCS[dn]
    out = []
    for j in range(i, min(len(ls), i + 40)):
        if j > i and (ls[j].strip() == "" or HEAD_RX.match(ls[j])):
            break
        out.append("%s:%d| %s" % (dn, j + 1, ls[j]))
    return clip("\n".join(out), cap)


def resolve(fam, key, cap):
    if fam == "K":
        if key in KAYIT:
            return "K:%d| %s" % (KAYIT[key][0], clip(KAYIT[key][1], max(cap, CAP_K)))
        return None
    if fam not in IDX:
        return None
    hits = IDX[fam].get(key.rstrip(".,;"))
    if not hits:
        return None
    outs = []
    for kind, i in hits[:2]:
        if kind == "h":
            outs.append(sect_text(fam, i, cap))
        elif kind == "b":
            outs.append(para_text(fam, i, cap))
        else:
            outs.append(clip("%s:%d| %s" % (fam, i + 1, DOCS[fam][i]), cap))
    return clip("\n".join(outs), cap)


REF_DOC = re.compile(r"\b(0[1-9]|1[01]) ((?:§\d+(?:\.\d+)*(?:, ?)?)+)")
REF_SEC = re.compile(r"§(\d+(?:\.\d+)*)")
REF_K = re.compile(r"\bK-\d+\b")
REF_KP = re.compile(r"\b10 (KP-\d+|ÖK-\d+|SK-\d+)\b")


def refs_of(text):
    out = []
    for m in REF_KP.finditer(text):
        out.append(("10", m.group(1)))
    for m in REF_DOC.finditer(text):
        for s in REF_SEC.findall(m.group(2)):
            out.append((m.group(1), s))
    for k in REF_K.findall(text):
        out.append(("K", k))
    seen, res = set(), []
    for r in out:
        if r not in seen:
            seen.add(r)
            res.append(r)
    return res


def kaynak_blok(rows, cap_each=420, max_refs=60):
    """rows -> '## KAYNAK BIRIMLERI' metni (tekil, kesilmis)."""
    refs, seen = [], set()
    for r in rows:
        for x in refs_of(r["kaynak"]):
            if x not in seen:
                seen.add(x)
                refs.append(x)
    parts = []
    for fam, key in refs[:max_refs]:
        t = resolve(fam, key, cap_each)
        parts.append("- [%s %s] %s" % (fam, key, ("\n" + t) if t else "KAYNAK METIN BULUNAMADI"))
    return "\n".join(parts), len(refs)


# ------------------------------------------------------------------ yazim
paketler = []


def yaz(ad, mercek, govde, aciklama):
    (OUT / ("paket_%s.md" % ad)).write_text(govde, encoding="utf-8", newline="\n")
    paketler.append((ad, mercek, bsz(govde), aciklama))


def pack(items, header, prefix, mercek, cap=CAP, aciklama=lambda its: ""):
    cur, size, n = [], bsz(header), 1
    for et, tx in items:
        sz = bsz(tx) + 2
        if cur and size + sz > cap:
            yaz("%s_%02d" % (prefix, n), mercek, header + "\n\n".join(t for _, t in cur) + "\n", aciklama([e for e, _ in cur]))
            cur, size, n = [], bsz(header), n + 1
        cur.append((et, tx))
        size += sz
    if cur:
        yaz("%s_%02d" % (prefix, n), mercek, header + "\n\n".join(t for _, t in cur) + "\n", aciklama([e for e, _ in cur]))


def aralik(e):
    return "%s..%s (%d)" % (e[0], e[-1], len(e)) if e else ""


COMMON = (
    "Sen bir Sonnet mercegisin. Dokumani DEGISTIRME; commit/PR yapma; yalniz bulgu dosyasini yaz. Kabukta heredoc/here-string/`python -c`/`node -e` YOK.\n"
    "`12` = `C:/projects/Shopfolio/Docs/12_VALIDATION_PROTOCOL.md` (740 KB: ASLA bastan sona okuma; `grep -n -F` ve Read offset/limit). Kaynaklar `Docs/02_…`-`11_…md`; "
    "karar kaydi `Docs/PRODUCT_DISCOVERY_STATUS.md` (5 MB: yalniz `grep -n \"^| K-1234 |\"`). Kaynak kazanir (K-652): 12 tuketicidir.\n"
    "Kapsam: skill deep-review katman 3-8 (kapsam ve tutarlilik katmanlarini audit 38 mercek + 105 madde ile genis kostu; bunlari yeniden TAM tarama, "
    "yalniz kalite bulgusu yaz). AUDIT'TE ZATEN KARARA BAGLANANLARI (sartname uygulanmis) tekrar raporlama: satir guncelse ve metin kaynakla uyumluysa bulgu degildir.\n"
    "Bulgu yaz: sadece GERCEK, kanitli ve eyleme donusen; dolgu bulgu YOK. Seviye: Yuksek = guvenlik / veri kaybi / para / yanlis PASS'a yol acan eksik; "
    "Orta = uygulamacinin (dogrulayici ajan) satiri belirsizlikle birakan eksik/yetersiz sinama; Dusuk = farkindalik, kucuk netlik.\n"
    "Bulguyu BULDUGUN ANDA dosyaya ekle (Write/Edit). SABIT bicim (betik okur):\n"
    "```\n## <MERCEK>-<n> | seviye: Yuksek|Orta|Dusuk\nKatman: <3|4|5|6|7|8|AY>\nYer: VAL-xxxx (12:<n>); baska yerler\nIddia: tek-iki cumle\n"
    "Kanit: iki tarafin satiri (12:<n>| ... <-> NN:<n>| ...)\nOneri: somut duzeltme (ya da eklenecek satir: bolum, kural, beklenen, seviye, kaynak)\n```\n"
    "Pozitif gozlem gerekiyorsa tek satir `POZITIF: ...` yaz (bulgu sayilmaz). Sonda dosya yoksa bile `bulgu yok` yaz. "
    "Donus tek satir: `<paket> | <n> bulgu (Y/O/D)`.\n\n")


# ------------------------------------------------------------------ AY
def diff_hunks():
    out = subprocess.run(["git", "diff", "-U0", V015, "HEAD", "--", "Docs/12_VALIDATION_PROTOCOL.md"], cwd=ROOT,
                         capture_output=True).stdout.decode("utf-8")
    spans = []
    for m in re.finditer(r"(?m)^@@ -\d+(?:,\d+)? \+(\d+)(?:,(\d+))? @@", out):
        a = int(m.group(1))
        c = int(m.group(2)) if m.group(2) is not None else 1
        if c > 0:
            spans.append((a, a + c - 1))
    return spans


AY_SPANS = diff_hunks()
AY_LINES = sorted({n for a, b in AY_SPANS for n in range(a, b + 1) if n <= N})
AY_ROWS = [BYLINE[n] for n in AY_LINES if n in BYLINE]
AY_PROSE = [n for n in AY_LINES if n not in BYLINE]

YAN = []
for adim in ("2a", "2b", "2c", "2r"):
    for f in sorted((HS / "a10-kd-audit-2" / adim).glob("hukum_*.md"), key=lambda p: int(re.search(r"(\d+)", p.stem).group(1))):
        cur_h = None
        for l in f.read_text(encoding="utf-8").split("\n"):
            m = re.match(r"^## (#\d+|Y\d+) \|", l)
            if m:
                cur_h = m.group(1)
            elif l.startswith("YAN:") and not l[4:].strip().startswith("yok"):
                YAN.append(("%s/%s %s" % (adim, f.stem, cur_h), l[4:].strip()))
for f in sorted((HS / "a10-kd-audit").glob("hukum_*.md"), key=lambda p: int(re.search(r"(\d+)", p.stem).group(1))):
    cur_h = None
    for l in f.read_text(encoding="utf-8").split("\n"):
        m = re.match(r"^## (#\d+|Y\d+) \|", l)
        if m:
            cur_h = m.group(1)
        elif l.startswith("YAN:") and not l[4:].strip().startswith("yok"):
            YAN.append(("1/%s %s" % (f.stem, cur_h), l[4:].strip()))

# ------------------------------------------------------------------ anahtar sozcuk kumeleri
OWASP = [
    ("A01 yetki / IDOR / erisim denetimi", r"yetki|başka (müşteri|üye|firma)|başkasının|sahip|erişim (denetim|izni|yok)|IDOR|403|401|oturumsuz|rol\b|izinsiz|yetkisiz|panel.{0,20}(giriş|erişim)"),
    ("A02 kriptografik / sir / veri koruma", r"şifre|parola|hash|sır\b|anahtar|belirteç|token|HTTPS|TLS|şifrele|maskele|saklan(maz|ır)|IBAN|kart numarası|açık metin"),
    ("A03 enjeksiyon / girdi dogrulama", r"enjeksiyon|XSS|SQL|kaçış|HTML|script|doğrulama hata|geçersiz|422|400|girdi|sınır değer|uzunluk|biçim|karakter|dosya (türü|boyut)|yükleme"),
    ("A04 guvensiz tasarim / is mantigi", r"yarış|eşzamanlı|aynı anda|çift (işlem|ödeme|gönder)|tek kullan|bir kez|kötüye|suistimal|atlat|manipül|değiştirilemez|istemciden|fiyat.{0,25}(istemci|tarayıcı)"),
    ("A05 yanlis yapilandirma / baslik / cerez", r"çerez|cookie|SameSite|HttpOnly|Secure|başlık|CSP|CORS|X-Frame|çerçeve|robots|hata (sayfası|ayrıntı)|yığın izi|debug|sürüm bilgisi|404 sayfası"),
    ("A06 bilesen / bagimlilik guvenligi", r"bağımlılık|paket (güvenlik|güncelle)|zafiyet|CVE|lisans|üçüncü taraf|sürüm"),
    ("A07 kimlik dogrulama / oturum", r"giriş|oturum|şifre sıfırla|parola|kilit|deneme (sayısı|sınır)|brute|hız sınırı|tek kullanımlık|doğrulama (kodu|bağlantı)|e-posta doğrula|çıkış|zaman aşımı"),
    ("A08 butunluk / imza / webhook", r"imza|webhook|bildirim (URL|adres)|hash doğrula|kimlik belirteci|nonce|aud\b|iss\b|HMAC|geri çağrı|callback|bütünlük|sağlama"),
    ("A09 kayit / izleme / denetim izi", r"denetim izi|günlük|log\b|erişim kaydı|iz kaydı|olay kaydı|kayda yaz|izlenir|uyarı e-posta|sır.{0,15}(günlük|log)"),
    ("A10 SSRF / dis cagri", r"SSRF|dış (adres|çağrı|bağlantı)|yönlendir|redirect|URL (doğrula|izin)|bağlantı adresi"),
]
HM_RX = re.compile(r"kesinti|erişilemez|yeniden dene|idempot|zaman aşımı|çöker|çökerse|düşer|yanıt vermez|yanıt gelmez|tekrar (gelen|gönder|işle)|çift (işlem|bildirim|gönder|ödeme|kayıt)|"
                   r"yedek|geri yükle|geri dön|telafi|kuyruk|bekleyen (iş|bildirim)|sağlayıcı (kesinti|yanıt|hata)|başarısız|hata hâli|hata durum|bağlantı kes|tutarsız|yarım kal|kısmen|"
                   r"devam eder|kalır\b.*(yeniden|tekrar)|sonradan|ertele|nabız|denetçi|zamanlanmış", re.I)
VA_RX = re.compile(r"tutar|KDV|toplam|indirim|kupon|kargo ücret|iade (tutar|edilen)|geri ödeme|komisyon|fiyat|kuruş|yuvarla|snapshot|dondur|kopyala|anlık görüntü|"
                   r"durum (değiş|geç)|silin|imha|arşiv|saklan|saklama|anonim|kişisel veri|KVKK|aydınlatma|sayaç|stok|adet|bakiye|kısmi|e-posta adresi|telefon|adres", re.I)
OB_RX = re.compile(r"yük\b|eşzamanlı|aynı anda|paralel|sayfalama|büyük (liste|veri|dosya)|binlerce|yüz(ler)? (sipariş|ürün)|kota|rate|hız sınırı|ölçek|kapasite|"
                   r"sağlayıcı (değiş|geç)|bağımlı|alternatif|yedek sağlayıcı|soyutla|sürüm (değişikliği|uyum)|belge(si)? (değiş|güncel)|uç (sürüm|değişik)|aylık|günlük (sınır|limit)|"
                   r"süre aşımı|performans|yanıt süresi|saniye içinde", re.I)

# ------------------------------------------------------------------ tanilar
tani = []
tani.append("12 satir: %d; VAL satiri: %d; benzersiz kimlik: %d; kayit K: %d" % (N, len(ROWS), len({r['id'] for r in ROWS}), len(KAYIT)))
tani.append("AY: degisen satir (v0.15 -> guncel): %d (hunk %d); bunlardan VAL satiri %d, duzyazi %d" % (len(AY_LINES), len(AY_SPANS), len(AY_ROWS), len(AY_PROSE)))
tani.append("YAN notu (non-yok): %d" % len(YAN))
for k in ("K-1549", "K-1550", "K-1551", "K-1552", "K-1553", "K-1554", "K-1555"):
    tani.append("%s %s" % (k, "var" if k in KAYIT else "YOK (bozuk girdi?)"))
cnt = {name: sum(1 for r in ROWS if re.search(rx, r["kural"] + " " + r["beklenen"] + " " + r["onkosul"], re.I)) for name, rx in OWASP}
tani.append("OWASP esleme: " + "; ".join("%s=%d" % (k.split()[0], v) for k, v in cnt.items()))
tani.append("HM esleme: %d; VA esleme: %d; OB esleme: %d" % tuple(
    sum(1 for r in ROWS if rx.search(r["kural"] + " " + r["beklenen"] + " " + r["onkosul"])) for rx in (HM_RX, VA_RX, OB_RX)))
# iki yonlu sinama: bilinen bag (VAL-A238 satirinin Kaynak'i cozulur) ve bozulmus girdi bayragi
ornek = BYID.get("VAL-A238")
if ornek:
    cz = [resolve(f, k, 100) is not None for f, k in refs_of(ornek["kaynak"])]
    tani.append("SINAMA bilinen bag VAL-A238 Kaynak '%s' cozulen: %s" % (ornek["kaynak"], cz))
bozuk = refs_of("10 KP-9999 · 02 §99.99.99 · K-9999999")
tani.append("SINAMA bozulmus girdi (olmayan kaynak) cozulmeli=False: %s" % [resolve(f, k, 100) is not None for f, k in bozuk])
if ornek and not all(cz):
    tani.append("UYARI: bilinen bag cozulmedi")
if SINAMA:
    print("\n".join(tani))
    sys.exit(0)

OUT.mkdir(parents=True, exist_ok=True)
for old in list(OUT.glob("paket_*.md")):
    old.unlink()

# ---- AY
ay_head = ("# AY paketi -- audit kararlarinin dokundugu yerler (K-1549…K-1555) + devreden YAN notlari\n" + COMMON +
           "Is: audit uygulamalari (v0.15 -> guncel) yeni tutarsizlik, yeni ic celiski, kaynakla sapma, bozulmus sayim/atif yaratti mi? YAN notlarindan hangisi hala acik "
           "(guncel metinde karsilanmamis)? Mercek adi `AY`; Katman `AY`.\n\n")
ay_items = []
named = []
for vid in ("VAL-A080", "VAL-C012", "VAL-C015", "VAL-A072"):
    if vid in BYID:
        named.append("12:%d| %s" % (BYID[vid]["line"], BYID[vid]["raw"]))
ay_named = ("## DEVREDEN YAN (1): Kaynak hucresi `02 §7.4` yerine `02 §7.3.1` olmali mi? (VAL-A080, C012, C015, A072)\n" + "\n".join(named) +
            "\n" + (kaynak_blok([BYID[v] for v in ("VAL-A080", "VAL-C012", "VAL-C015", "VAL-A072") if v in BYID], 700)[0]))
sev_pair = [BYID[v] for v in ("VAL-A068", "VAL-A078", "VAL-A238", "VAL-A069") if v in BYID]
ay_sev = ("## DEVREDEN YAN (2): IBAN'in geri odemeye baglanmasi (A068/A078 ORTA) <-> es degeri A238/A069 (KRITIK) -- severity tutarliligi\n" +
          "\n".join("12:%d| %s" % (r["line"], r["raw"]) for r in sev_pair) + "\n" + kaynak_blok(sev_pair, 600)[0])
bi = [r for r in ROWS if re.search(r"birleş", r["kural"] + " " + r["beklenen"], re.I) and re.search(r"stok", r["kural"] + " " + r["beklenen"] + " " + r["kaynak"], re.I)][:8]
bi_src = resolve("02", "3.16.8", 1400) or "(02 §3.16.8 bulunamadi)"
ay_bir = ("## DEVREDEN YAN (3): birlesmede stok kolunun sonucu kaynakta belirsiz (`02 §3.16.8`) -- kaynak belirsizse K-652 geri besleme ya da KARAR\n" +
          bi_src + "\n\nILGILI 12 SATIRLARI (betik adayi):\n" + "\n".join("12:%d| %s" % (r["line"], r["raw"]) for r in bi))
yan4 = [BYID[v] for v in ("VAL-A147", "VAL-A177", "VAL-A178", "VAL-A730") if v in BYID]
ay_yan4 = ("## DEVREDEN YAN (4): oturum/yetki davranisi ORTA (A147, A177, A178, A730) -- degerlendirildi, sartname yazilmadi; yeniden BAKILMAZ, yalniz AY'de celiski varsa\n" +
           "\n".join("12:%d| %s" % (r["line"], r["raw"]) for r in yan4))
ay_yan5 = ("## DEVREDEN YAN (5): 2a/2b/2c/2r ve KD-audit hukumlerindeki `YAN:` notlari (%d); uygulanan listeler: a10-kd-audit-2/*/uygulanan.md. Her not icin guncel 12'de "
           "karsilanip karsilanmadigini `grep -n -F` ile sina; karsilanmamis VE gercek olanlari bulgu yap (ayni kalipli baska satirlari da tara)\n" % len(YAN) +
           "\n".join("- [%s] %s" % (a, clip(b, 700)) for a, b in YAN))
ay_k = "## KARAR KAYITLARI K-1549…K-1555 (kisaltilmis)\n" + "\n".join(
    "K:%d| %s" % (KAYIT[k][0], clip(KAYIT[k][1], 900)) for k in ("K-1549", "K-1550", "K-1551", "K-1552", "K-1553", "K-1554", "K-1555") if k in KAYIT)
ay_items += [("YAN1", ay_named), ("YAN2", ay_sev), ("YAN3", ay_bir), ("YAN4", ay_yan4), ("K", ay_k)]
# YAN notlari uzun: uc parcaya
ya = [("- [%s] %s" % (a, clip(b, 700))) for a, b in YAN]
chunk, sz, k = [], 0, 1
for t in ya:
    if chunk and sz + bsz(t) > 36_000:
        ay_items.append(("YAN5_%d" % k, "## YAN NOTLARI (5) parca %d\n" % k + "\n".join(chunk)))
        chunk, sz, k = [], 0, k + 1
    chunk.append(t)
    sz += bsz(t) + 1
if chunk:
    ay_items.append(("YAN5_%d" % k, "## YAN NOTLARI (5) parca %d\n" % k + "\n".join(chunk)))
for i in range(0, len(AY_ROWS), 12):
    grp = AY_ROWS[i:i + 12]
    kb, _ = kaynak_blok(grp, 380, 30)
    ay_items.append(("DEG_%d" % i, "## DEGISEN SATIRLAR %s..%s\n%s\n### Bu satirlarin kaynak birimleri\n%s" % (
        grp[0]["id"], grp[-1]["id"], "\n".join("12:%d| %s" % (r["line"], r["raw"]) for r in grp), kb)))
ay_pr = "\n".join("12:%d| %s" % (n, LINES[n - 1]) for n in AY_PROSE)
ay_items.append(("DEG_DUZYAZI", "## DEGISEN DUZYAZI / SAYIM SATIRLARI\n" + ay_pr))
pack(ay_items, ay_head, "AY", "AY", aciklama=lambda e: "%d oge" % len(e))

# ---- TD
td_head = ("# TD paketi -- teknik derinlik: dogrulayici ajan satiri YALNIZ satirla kosabilir mi?\n" + COMMON +
           "Is (katman 3, sec 7.2): her satir icin ön koşul KURULABILIR mi (veri/hesap/durum nasil hazirlanir belli mi), adim TEKIL mi, beklenen sonuc OLCULEBILIR mi "
           "(sayi/ad/durum/kod), kanit turu severity'ye uyuyor mu, ayni satirdan iki farkli dogrulayici ayni PASS/FAIL'e varir mi? Mercek adi `TD`. "
           "Yalniz GERCEK belirsizlik; kisa ama yeterli satir bulgu degil.\n\n")
spans = [(H[4], H[5]), (H[12], N)]
td_items = []
for a, b in spans:
    s, size = a, 0
    for i in range(a, b):
        sz = bsz(LINES[i]) + 12
        if size and size + sz > 38_000:
            td_items.append(("12:%d-%d" % (s + 1, i), "## 12:%d-%d\n%s" % (s + 1, i, fmt(s, i))))
            s, size = i, 0
        size += sz
    td_items.append(("12:%d-%d" % (s + 1, b), "## 12:%d-%d\n%s" % (s + 1, b, fmt(s, b))))
# 38 KB'lik dilimleri ikiser birlestir (pack zaten esik uygular)
pack(td_items, td_head, "TD", "TD", aciklama=lambda e: ",".join(e))
# kapilar
s11a = find_line(r"^## 6\. Faz ge", 0, None)
d11 = DOCS["11"]
a11 = next(i for i, l in enumerate(d11) if re.match(r"^## 6\. Faz ge", l))
b11 = next((i for i in range(a11 + 1, len(d11)) if re.match(r"^## 7\. ", d11[i])), len(d11))
sk = (ROOT / ".claude" / "skills" / "validate" / "SKILL.md").read_text(encoding="utf-8").split("\n")
td_k = ("# TD_K paketi -- kapilar: 12 s5-s11 <-> 11 s6 faz kapilari <-> skills/validate\n" + COMMON +
        "Is (katman 3, mercek adi `TD`): giris/cikis/kanit/severity/FAIL/ajan modeli/roller/yeniden dogrulama kurallari (12 s5-s11) bir dogrulayici ajan tarafindan "
        "ISLETILEBILIR mi (her kapi icin olculebilir kriter, sorumlu, kanit yeri), `11` §6 faz kapilari ve `skills/validate` ile calisir mi (adim eksik, sirasiz, belirsiz)?\n\n"
        "## 12 s5-s11\n" + fmt(H[5], H[12]) + "\n\n## 11 s6 Faz kapilari\n" +
        "\n".join("11:%d| %s" % (i + 1, d11[i]) for i in range(a11, b11)) + "\n\n## skills/validate\n" +
        "\n".join("skills/validate:%d| %s" % (i + 1, l) for i, l in enumerate(sk)) + "\n")
if bsz(td_k) > CAP:
    td_k = clip(td_k, CAP)
yaz("TD_K", "TD", td_k, "12 s5-s11 + 11 s6 + skill")


# ---- GV
def inventory(rx_list):
    o = ["## OWASP KATEGORI ENVANTERI (betik: anahtar sozcuk esleme; KESIN DEGILDIR -- bosluk analizi icin)"]
    for name, rx in rx_list:
        ids = [r["id"] for r in ROWS if re.search(rx, r["kural"] + " " + r["beklenen"] + " " + r["onkosul"], re.I)]
        gv = [r["id"] for r in ROWS if r["id"] in ids and "[güvenlik]" in r["kural"]]
        o.append("- %s: %d satir; `[güvenlik]` etiketli %d; ilk 18: %s" % (name, len(ids), len(gv), ", ".join(ids[:18])))
    return "\n".join(o)


gv_tag = [r for r in ROWS if "[güvenlik]" in r["kural"]]
gv_head = ("# GV paketi -- guvenlik (OWASP Top 10 cercevesi, skill deep-review katman 4)\n" + COMMON +
           "Is (mercek adi `GV`, Katman 4): her guven-kritik yuzey icin (kimlik, oturum, yetki/IDOR, girdi dogrulama, sir, webhook imzasi, hiz siniri, kisisel veri, cerez/baslik, "
           "kayit) bir `[güvenlik]` etiketli satir VAR MI, KRITIK mi (sec 4.2 madde 6, sec 8.1), kanit turu yeterli mi (en az iki tur), sinama yontemi gercekten acigi yakalar mi "
           "(ornek: IDOR icin ikinci kullanicinin kaynagi)? Etiketli satir sayisi toplam: %d. Bulgu = eksik yuzey ya da zayif/yanlis seviyeli satir.\n\n%s\n\n" % (len(gv_tag), inventory(OWASP)))
seen_gv, gv_items = set(), []
for r in ROWS:
    txt = r["kural"] + " " + r["beklenen"] + " " + r["onkosul"]
    if "[güvenlik]" in r["kural"] or any(re.search(rx, txt, re.I) for name, rx in OWASP[:2] + OWASP[6:8]):
        gv_items.append((r["id"], crow(r)))
gv_head += "## ADAY SATIRLAR (compact; K=kural, S=kaynak, Ort=kanit ortami, Ön=on kosul, Bek=beklenen)\n"
pack(gv_items, gv_head, "GV", "GV", aciklama=aralik)

# ---- HM
hm_secs = ["4.16", "12.6", "12.11"]
hm_head = ("# HM paketi -- hata modu (skill deep-review katman 5)\n" + COMMON +
           "Is (mercek adi `HM`, Katman 5): dis servis kesintisi (odeme, e-posta, Google), yeniden deneme, idempotency (cift webhook/cift tiklama), zaman asimi, yarim kalan islem, "
           "yedekten donus -- kaynakta (08 §3, 05 §10, 02/07) karsiligi olan her hata modu icin 12'de sinanan satir var mi; seviye E sozlesmeleri (s4.16), esik/sure sayilari kaynakla ayni mi; "
           "kesinti sirasinda para/stok/durum tutarsiz kalabilecek bir yol sinanmamis mi?\n\n")
hm_items = []
for c in hm_secs:
    a, b = sec_range(c)
    hm_items.append(("S" + c, "## 12 §%s (tum bolum)\n%s" % (c, fmt(a, b))))
hm_secnums = [sec_range(c) for c in hm_secs]
hm_items.append(("08s3", "## 08 §3 (ortak hata yonetimi) kaynak\n" + (resolve("08", "3", 7000) or "")))
hm_items.append(("05s10", "## 05 §10.1 kaynak\n" + (resolve("05", "10.1", 4000) or "")))
for r in ROWS:
    if any(a < r["line"] <= b for a, b in hm_secnums):
        continue
    if HM_RX.search(r["kural"] + " " + r["beklenen"] + " " + r["onkosul"]):
        hm_items.append((r["id"], crow(r)))
pack(hm_items, hm_head, "HM", "HM", aciklama=lambda e: "%d oge" % len(e))

# ---- VA
va_secs = ["12.3", "12.4", "12.7"]
va_head = ("# VA paketi -- veri akisi izleme (skill deep-review katman 6)\n" + COMMON +
           "Is (mercek adi `VA`, Katman 6): para tutari (sepet → KDV → kargo → kupon → siparis → iade), sipariş/kalem/talep durumu, kisisel veri (olusur → dogrulanir → guncellenir → "
           "silinir/arsivlenir): 12 s12.3 (durum gecis), s12.4 (hesaplama), s12.7 (saklama/imha) UCTAN UCA zinciri kapatiyor mu; snapshot-mi-referans-mi karari her yerde ayni mi; "
           "bir veri kendi yolculugunda sinanmadan atlanmis mi (olusur ama silinmez, hesaplanir ama yuvarlanmaz)?\n\n")
va_items = []
for c in va_secs:
    a, b = sec_range(c)
    va_items.append(("S" + c, "## 12 §%s (tum bolum)\n%s" % (c, fmt(a, b))))
va_secnums = [sec_range(c) for c in va_secs]
for r in ROWS:
    if any(a < r["line"] <= b for a, b in va_secnums):
        continue
    if VA_RX.search(r["kural"]) and r["lvl"] in ("B", "C", "D"):
        va_items.append((r["id"], crow(r, 260, 260, 160)))
pack(va_items, va_head, "VA", "VA", aciklama=lambda e: "%d oge" % len(e))

# ---- OB
ob_head = ("# OB paketi -- olceklenebilirlik + bagimlilik riski (skill deep-review katman 7-8)\n" + COMMON +
           "Is (mercek adi `OB`, Katman 7-8): YALNIZ MVP'yi engelleyen ya da ileriyi kapatan. 12 s12.12 yuk senaryosu gercekci mi (sayi/sure kaynakta), seviye E sozlesmeleri (s4.16) "
           "saglayici degisiminde protokolun ayakta kalmasini sagliyor mu (sozlesme–urun tepkisi ayrimi), dis servis kisitlari (hiz sinirlari, kota, sandbox farki) satirlara yansimis mi?\n\n")
ob_items = []
for c in ("12.12", "4.16"):
    a, b = sec_range(c)
    ob_items.append(("S" + c, "## 12 §%s (tum bolum)\n%s" % (c, fmt(a, b))))
ob_items.append(("08s1", "## 08 dis bagimlilik ozeti (s1-s2 basliklari)\n" + "\n".join(
    "08:%d| %s" % (i + 1, l) for i, l in enumerate(DOCS["08"]) if re.match(r"^#{1,3} ", l))[:6000]))
ob_secnums = [sec_range(c) for c in ("12.12", "4.16")]
for r in ROWS:
    if any(a < r["line"] <= b for a, b in ob_secnums):
        continue
    if OB_RX.search(r["kural"] + " " + r["beklenen"]):
        ob_items.append((r["id"], crow(r, 240, 240, 140)))
pack(ob_items, ob_head, "OB", "OB", aciklama=lambda e: "%d oge" % len(e))

# ------------------------------------------------------------------ dizin
(OUT / "paket_dizin.txt").write_text(
    "\n".join("%s\t%s\t%d\t%s" % p for p in paketler) + "\n\n" + "\n".join(tani) + "\n", encoding="utf-8", newline="\n")
print("\n".join(tani))
print("paket sayisi: %d; toplam bayt: %d" % (len(paketler), sum(p[2] for p in paketler)))
for ad, mercek, sz, ac in paketler:
    print("%-8s %-3s %7d  %s" % (ad, mercek, sz, ac[:90]))
