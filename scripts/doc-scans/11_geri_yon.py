"""Asama 9 -- izlenebilirlik matrisinin geri yonu (11 7.2) ve task boyutu denetimi
(checklists/document-stage.md 2; karar kaydi 22.2, K-1404 O6; boyut K-1409).

7.1'deki her satirin "Task" hucresinden task -> kaynak ogeleri turetir:
  --yaz       11 7.2 tablosunu (Task | Beslendigi kaynak oge(ler) | Durum) yeniden yazar
  --denetle   7.2'nin 7.1'den turetilenle birebir esit oldugunu denetler (yalniz okur)
  --rapor     gerekcesiz ek adaylarini, faz basina task sayisini ve K-1409 asimlarini yazar
Kapsama betigi (11_matris_kapsama.py) bu dosyadaki `turet`/`tablo_satirlari`/`oku_7_2` islevlerini
kullanir.

Kullanim (depo kokunden): python -I scripts/doc-scans/11_geri_yon.py . --rapor|--yaz|--denetle
"""
import re
import sys
from collections import Counter, OrderedDict
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

TASK_RE = re.compile(r"^T\d+[a-z]?(?:\.\d+)?$")
FAZ = [(1, 99, "F0"), (101, 199, "F1"), (201, 299, "F2"), (301, 399, "F3"),
       (401, 499, "F4"), (501, 599, "F5"), (601, 699, "F6")]

# K-1409 (1): birlikte saglanan ust sinirlar.
SINIR = {"F1": [("entity", 8)], "F3": [("uc", 8)], "F4": [("uc", 8)],
         "F5": [("ekran", 2), ("ob", 4), ("uc", 8)]}

# O6-1 incelemesinin sonucu: task -> (durum, gerekce). Rapor modunda aday cikan her task buraya yazilir.
INCELENEN = {
    "T03": ("✓ gerekçeli (alt bölüm + devir) — K-1432, K-1434", ""),
    "T102": ("✓ gerekçeli (alt bölüm) — K-1413, K-1414", ""),
    "T119": ("✓ gerekçeli (alt bölüm) — K-1413, K-1414", ""),
    "T120": ("✓ gerekçeli (alt bölüm) — K-1414, K-1416", ""),
    "T204": ("✓ gerekçeli (alt bölüm) — K-1414, K-1419", ""),
    "T205": ("✓ gerekçeli (alt bölüm) — K-1414", ""),
    "T211": ("✓ gerekçeli (alt bölüm) — K-1416", ""),
    "T213": ("✓ gerekçeli (alt bölüm) — K-1416", ""),
    "T214": ("✓ gerekçeli (alt bölüm) — K-1416", ""),
    "T217": ("✓ gerekçeli (alt bölüm) — K-1416, K-1419", ""),
    "T230": ("✓ gerekçeli (alt bölüm + devir) — K-1435", ""),
    "T231": ("✓ gerekçeli (alt bölüm + devir) — K-1435", ""),
    "T402": ("✓ gerekçeli (alt bölüm) — K-1415", ""),
    "T408": ("✓ gerekçeli (alt bölüm) — K-1414", ""),
    "T410": ("✓ gerekçeli (alt bölüm) — K-1413, K-1414", ""),
    "T538": ("✓ gerekçeli (alt bölüm) — K-1416, K-1418", ""),
    "T546": ("✓ gerekçeli (alt bölüm) — K-1418", ""),
    "T548": ("✓ gerekçeli (alt bölüm + devir) — K-1435", ""),
    "T10": ("✓ gerekçeli (alt bölüm) — K-1441", ""),
    "T232": ("✓ gerekçeli (alt bölüm) — K-1441", ""),
    "T233": ("✓ gerekçeli (alt bölüm) — K-1441", ""),
    "T389": ("✓ gerekçeli (alt bölüm) — K-1441", ""),
    "T390": ("✓ gerekçeli (alt bölüm) — K-1441", ""),
}


def faz_of(t):
    n = int(re.match(r"T(\d+)", t).group(1))
    for a, b, f in FAZ:
        if a <= n <= b:
            return f
    return None


def tur(kimlik):
    """Kaynak kimliginin turu: guclu (ozgun) ya da zayif (alt_bolum, devir) -- ve ince tur."""
    fam, _, rest = kimlik.partition(":")
    if fam == "devir":
        return "devir", "devir"
    if fam == "06":
        return ("ozgun", "entity") if re.match(r"§4\.\d+ ", rest) else ("alt_bolum", "06 alt bolum")
    if fam == "07":
        return ("ozgun", "uc") if rest.startswith("UC-") else ("alt_bolum", "07 alt bolum")
    if fam == "08":
        return ("ozgun", "entegrasyon") if re.match(r"§2\.\d+$", rest) else ("alt_bolum", "08 alt bolum")
    if fam == "04":
        if rest.startswith("E-"):
            return "ozgun", "ekran"
        if rest.startswith("OB-"):
            return "ozgun", "ob"
        return "alt_bolum", "04 alt bolum"
    if fam == "03":
        return "alt_bolum", "03 alt bolum"
    if fam == "02":
        if re.match(r"§\d+\.\d+\.\d+$", rest) or re.match(r"[A-Z]-\d+$", rest):
            return "ozgun", "02 kural"
        return "alt_bolum", "02 alt bolum"
    if fam == "05":
        return ("ozgun", "mk") if rest.startswith("MK-") else ("alt_bolum", "05 alt bolum")
    if fam == "09":
        return "alt_bolum", "09 bolum"
    if fam == "10":
        return ("ozgun", "kp/kd") if re.match(r"K[PD]-\d+", rest) else ("alt_bolum", "10 alt bolum")
    return "ozgun", fam


def oku_plan(root):
    f = sorted((Path(root) / "Docs").glob("11_*.md"))[0]
    return f, f.read_text(encoding="utf-8").split("\n")


def kimlik_kisa(h):
    """Hucre -> `fam:id` (06 entity icin adli); 11_matris_kapsama.kimlik ile ayni kurallar."""
    h = h.strip().strip("`")
    for pat in (r"^(04):((?:E|OB)-\d+)\b", r"^(02):((?:Z|L|P|B|F|H)-\d+)\b", r"^(05):(MK-\d+)\b",
                r"^(10):((?:KP|KD)-\d+)\b", r"^(devir):(K-\d+|P8\.\d|D-01|\d\d-\d+)\b"):
        m = re.match(pat, h)
        if m:
            return "%s:%s" % (m.group(1), m.group(2))
    mu = re.match(r"^(07) (UC-\d+)\b", h)
    if mu:
        return "%s:%s" % (mu.group(1), mu.group(2))
    m = re.match(r"^(\d\d):(§\d+(?:\.\d+)*)(?: (.*))?$", h)
    if not m:
        return None
    fam, sec, rest = m.group(1), m.group(2), m.group(3)
    if fam == "06" and sec.startswith("§4.") and rest:
        ad = re.match(r"[A-Z][A-Za-z0-9]+", rest)
        if ad:
            return "%s:%s %s" % (fam, sec, ad.group(0))
    return "%s:%s" % (fam, sec)


def matris_satirlari(plan):
    """7.1 satirlari: (alt_bolum, kimlik, [task...])."""
    res, cur = [], None
    for l in plan:
        m = re.match(r"#### (7\.1\.\d)\b", l)
        if m:
            cur = m.group(1)
            continue
        if l.startswith("## ") or (l.startswith("### ") and not l.startswith("#### ")):
            cur = None
        if cur and l.startswith("|"):
            c = [x.strip() for x in l.strip().strip("|").split("|")]
            if len(c) != 4:
                continue
            k = kimlik_kisa(c[0])
            if k:
                t = c[1]
                tasks = [] if t.startswith(("—", "-")) else [x.strip() for x in t.split(",")]
                res.append((cur, k, tasks))
    return res


def aday_tablosu(plan):
    """7 icindeki `| T.. | Fxx | kisa ad | modul | kaynak |` satirlari: id -> (faz, ad, modul, kaynak)."""
    res = OrderedDict()
    in7 = False
    for l in plan:
        if l.startswith("## "):
            in7 = l.startswith("## 7.")
        if in7:
            c = [x.strip() for x in l.strip().strip("|").split("|")]
            if len(c) == 5 and re.match(r"^T\d+[a-z]?$", c[0]) and re.match(r"^F\d$", c[1]):
                res[c[0]] = (c[1], c[2], c[3], c[4])
    return res


def turet(plan):
    """task -> kaynak kimlikleri (7.1 sirasiyla)."""
    ters = OrderedDict()
    for _, k, tasks in matris_satirlari(plan):
        for t in tasks:
            ters.setdefault(t, []).append(k)
    return ters


def anahtar(t):
    m = re.match(r"T(\d+)([a-z]?)", t)
    return (int(m.group(1)), m.group(2))


def kaynak_hucre(kaynaklar):
    gr = OrderedDict()
    for k in kaynaklar:
        fam, _, rest = k.partition(":")
        gr.setdefault(fam, []).append(rest)
    return "; ".join("`%s` %s" % (f, ", ".join(v)) for f, v in gr.items())


def durum(t, kaynaklar):
    if t in INCELENEN:
        return INCELENEN[t][0]
    c = Counter(tur(k)[0] for k in kaynaklar)
    if not kaynaklar:
        return "KAYNAKSIZ"
    return "✓ %d kaynak (özgün %d, alt bölüm %d, devir %d)" % (len(kaynaklar), c["ozgun"], c["alt_bolum"], c["devir"])


def tablo_satirlari(plan):
    ters = turet(plan)
    aday = aday_tablosu(plan)
    satir = []
    for t in sorted(aday, key=anahtar):
        ks = ters.get(t, [])
        satir.append("| %s | %s | %s |" % (t, kaynak_hucre(ks) or "—", durum(t, ks)))
    return satir


def oku_7_2(plan):
    """Mevcut 7.2 tablosunun govde satirlari."""
    ic, res = False, []
    for l in plan:
        if l.startswith("### 7.2"):
            ic = True
            continue
        if ic and l.startswith("## "):
            break
        if ic and re.match(r"\| T\d+[a-z]? \|", l):
            res.append(l.rstrip())
    return res


def zayif_adaylar(plan):
    ters, aday = turet(plan), aday_tablosu(plan)
    res = []
    for t in sorted(aday, key=anahtar):
        ks = ters.get(t, [])
        tr = Counter(tur(k)[0] for k in ks)
        if ks and tr["ozgun"] == 0:
            res.append((t, "yalniz devir" if tr["alt_bolum"] == 0 else "yalniz alt bolum" if tr["devir"] == 0
                        else "alt bolum + devir", ks))
        elif not ks:
            res.append((t, "kaynaksiz", ks))
    return res


def boyut_asimlari(plan):
    ters, aday = turet(plan), aday_tablosu(plan)
    asim, bos = [], []
    for t in sorted(aday, key=anahtar):
        faz = aday[t][0]
        c = Counter(tur(k)[1] for k in ters.get(t, []))
        for ad, ust in SINIR.get(faz, []):
            if c[ad] > ust:
                asim.append((t, faz, ad, c[ad], ust))
    return asim


if __name__ == "__main__":
    root = sys.argv[1] if len(sys.argv) > 1 else "."
    mod = sys.argv[2] if len(sys.argv) > 2 else "--rapor"
    f, plan = oku_plan(root)
    if mod == "--rapor":
        ters, aday = turet(plan), aday_tablosu(plan)
        print("aday %d, kaynakli task %d" % (len(aday), len(ters)))
        fz = Counter(v[0] for v in aday.values())
        print("faz basina task: " + ", ".join("%s %d" % (k, fz[k]) for k in sorted(fz)))
        print("asim:", boyut_asimlari(plan))
        for t, ne, ks in zayif_adaylar(plan):
            print("ZAYIF %s [%s] %d kaynak: %s | %s | %s" % (t, ne, len(ks), ", ".join(ks)[:150], aday[t][1][:90], aday[t][3][:80]))
    elif mod == "--denetle":
        yeni, eski = tablo_satirlari(plan), oku_7_2(plan)
        print("turetilen %d, yazili %d, esit: %s" % (len(yeni), len(eski), yeni == eski))
        sys.exit(0 if yeni == eski else 1)
    elif mod == "--yaz":
        yeni = tablo_satirlari(plan)
        out, i = [], 0
        while i < len(plan):
            out.append(plan[i])
            if plan[i].startswith("|---|---|---|") and any(x.startswith("### 7.2") for x in plan[:i]) \
                    and not any(x.startswith("## 8") for x in plan[:i]):
                out.extend(yeni)
                i += 1
                while i < len(plan) and plan[i].startswith("| T"):
                    i += 1
                continue
            i += 1
        f.write_text("\n".join(out), encoding="utf-8")
        print("yazildi: %d satir" % len(yeni))
