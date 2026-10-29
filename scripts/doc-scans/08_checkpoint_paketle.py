"""Builds checkpoint packages and mechanical-check output for Stage 8 (Coding Guidelines) closing step 4.

Usage:
  python -I 08_checkpoint_paketle.py [output_dir]

Output (output_dir): mek.txt (mechanical results), paket_M1..M5 (transition lenses), paket_S1..Sn (09 slices for the
summary-vs-detail class). Every package is a single file (<= 90 KB). Reuses helpers of 09_audit_paketle.py.
"""
import importlib.util
import re
import sys
from pathlib import Path

sys.stdout.reconfigure(encoding="utf-8")
HERE = Path(__file__).resolve().parent
spec = importlib.util.spec_from_file_location("ap", HERE / "09_audit_paketle.py")
sys.argv = [sys.argv[0]] + [a for a in sys.argv[1:] if a != "--selftest"]
OUT = Path(sys.argv[1]) if len(sys.argv) > 1 else HERE
ap = importlib.util.module_from_spec(spec)
# the audit script runs main() at import; neutralise by feeding it a scratch output dir
_real_argv = sys.argv
sys.argv = [sys.argv[0], str(OUT / "_ap_tmp")]
spec.loader.exec_module(ap)
sys.argv = _real_argv
OUT.mkdir(parents=True, exist_ok=True)

DOCS = ap.DOCS
CAP = 88_000
FILES = dict(ap.DOCFILES)
FILES.update({"09": "09_CODING_GUIDELINES.md", "10": "10_MVP_SCOPE.md", "11": "11_IMPLEMENTATION_PLAN.md",
              "12": "12_VALIDATION_PROTOCOL.md", "DEPLOY_RUNBOOK": "DEPLOY_RUNBOOK.md"})
REG = ap.REG
K_LO, K_HI = 1302, 1392
nb = ap.nbytes
L09 = ap.read_lines(DOCS / FILES["09"])

HEADER = """# Checkpoint merceği — paket {pid}

> **Görev ({title}).** Bu dosya tek pakettir; bütün kanıt burada. Aşama 8 (Kodlama Kılavuzu) kapanışının checkpoint'idir:
> `09` ile bu aşamada geri beslenen dokümanlar arasında tutarsızlık aranır. Karar kaydının tamamı okunmaz; gerekirse
> `grep -n` ve aralıklı okuma kullan, **100 KB'tan büyük dosya bütün okunmaz**. Heredoc yok.
> Her bulguyu bulduğun anda `{outfile}` dosyasına ekle (tek tek, sona bırakma). Bulgu biçimi:
> `### <kimlik> — <yer1> <-> <yer2>` · seviye (yüksek/orta/düşük; orta = iki yer çelişiyor ya da kapalı liste/sayı/hedef yanlış
> ya da kuralın evinde kural eksik; düşük = ifade/kapsam/atıf farkı) · iki yerin birebir alıntısı · ne çelişiyor · önerilen
> düzeltme ESKİ → YENİ (ESKİ hedef dosyada tam bir kez geçen birebir metin; hedef dosya adı).
> Uygulanmış ve kayda geçmiş bulgu yeniden açılmaz. Emin olmadığın şeyi "belirsiz" diye yaz, uydurma. Sonda tek satır: bulgu sayısı.
> Rapor Türkçe.

---

"""


def reg_cells(k):
    r = REG.get(k)
    return r


def etki_targets(cell):
    out = set()
    for m in re.finditer(r"`(0[1-9]|1[0-2]|DEPLOY_RUNBOOK|CI_CD_SETUP)(?:\s*§[^`]*)?`", cell):
        out.add(m.group(1))
    for m in re.finditer(r"(?:^| · |, |\()(0[1-9]|1[0-2]) (?=§|[A-Za-zÇĞİÖŞÜçğıöşü(])", cell):
        out.add(m.group(1))
    return out


_text = {}


def doc_text(d):
    if d not in _text:
        _text[d] = (DOCS / FILES[d]).read_text(encoding="utf-8")
    return _text[d]


def ks_in(text):
    return ap.kaynak_ks(text)


# ------------------------------------------------------------- mechanical: versions
def tracker_versions():
    res = {}
    for ln in ap.read_lines(DOCS / "PRODUCT_DISCOVERY_STATUS.md")[28:60]:
        m = re.match(r"\| (\d\d) \| [^|]+\| (\S+) \| (v[\d.]+)", ln)
        if m:
            res[m.group(1)] = (m.group(3), m.group(2))
    return res


def version_check():
    tv = tracker_versions()
    out = ["## Sürüm kontrolü (başlık / alt bilgi / tracker §1)\n"]
    for d in ("02", "05", "06", "07", "08", "09", "10", "11", "12"):
        t = doc_text(d)
        h = re.search(r"\*\*Versiyon: (v[\d.]+)", t)
        tail = t.strip().split("\n")[-6:]
        foot = re.findall(r"v\d+\.\d+", " ".join(tail))
        out.append(f"- {d}: başlık {h.group(1) if h else '?'} · alt bilgi {foot[:3]} · tracker {tv.get(d)}")
    t = doc_text("DEPLOY_RUNBOOK")
    h = re.search(r"[Vv]ersiyon[: *]+(v[\d.]+)", t)
    out.append(f"- DEPLOY_RUNBOOK: başlık {h.group(1) if h else '?'}")
    return "\n".join(out)


# ------------------------------------------------------------- mechanical: etki
def etki_check():
    miss, extra = [], []
    kfiles = {d: ks_in(doc_text(d)) for d in FILES if d != "09"}
    per_k = {}
    for k in range(K_LO, K_HI + 1):
        r = REG.get(k)
        if not r:
            continue
        cells = r[2].split(" | ") if False else None
        row = r[2]
        parts = row.split(" | ")
        etki = parts[5] if len(parts) > 5 else ""
        tg = etki_targets(etki)
        per_k[k] = (tg, etki, parts[2] if len(parts) > 2 else "", parts[3] if len(parts) > 3 else "")
        for d in sorted(tg):
            if d in ("09", "01", "03", "04", "10", "CI_CD_SETUP") and d != "10":
                if d not in kfiles:
                    continue
            if d in kfiles and k not in kfiles[d]:
                miss.append((k, d))
    # reverse: target doc cites K in range, etki doesn't list the doc
    for d, ks in kfiles.items():
        for k in sorted(x for x in ks if K_LO <= x <= K_HI):
            if k in per_k and d not in per_k[k][0]:
                extra.append((k, d))
    return per_k, miss, extra


def pkg_m1(per_k, miss, extra):
    body = [HEADER.format(pid="M1", title="K-1302…K-1392 etki sütunu ↔ hedef dokümanda karşılık",
                          outfile=str(OUT / "mercek_M1.md"))]
    body.append("## Betik adayları\n\n"
                "A) Karar etki sütununda bir doküman gösteriyor ama o dokümanın hiçbir yerinde `K-<n>` anılmıyor (K aralıkları açılmış). "
                "Her aday için: kararın dokümanda yansıması gerekir mi (etki sütunu hangi bölümü/işi söylüyor), yansıması varsa K anılmasa da "
                "metinde duruyor mu (terim için `grep -n`), yoksa eksik mi? Park blokları (`11`, `12`, `DEPLOY_RUNBOOK`) ve `02`/`05`/`06` sürüm notları öncelikli.\n"
                "B) Hedef doküman bir K'yı anıyor ama K'nın etki sütunu o dokümanı göstermiyor.\n")
    body.append(f"\nA adayları: {len(miss)} · B adayları: {len(extra)}\n")
    body.append("\n### A) Etki sütunu gösteriyor, hedefte K yok\n")
    byk = {}
    for k, d in miss:
        byk.setdefault(k, []).append(d)
    for k, ds in byk.items():
        tg, etki, konu, karar = per_k[k]
        body.append(f"\n**K-{k}** hedefler yok: {', '.join(ds)} | konu: {konu[:220]}\n> etki: {etki[:900]}\n")
    body.append("\n### B) Hedef anıyor, etki sütunu göstermiyor\n")
    for k, d in extra:
        tg, etki, konu, karar = per_k[k]
        body.append(f"- K-{k} anılan doküman: {d} | etki sütunundakiler: {sorted(tg)} | konu: {konu[:150]}\n")
    body.append("\n### Çözülmüş hedef listesi (doküman → etki sütununda gösteren K sayısı)\n")
    cnt = {}
    for k, v in per_k.items():
        for d in v[0]:
            cnt[d] = cnt.get(d, 0) + 1
    body.append(", ".join(f"{d}: {n}" for d, n in sorted(cnt.items())) + "\n")
    return "".join(body)


# ------------------------------------------------------------- names (M2)
NAME = re.compile(r"`([A-Za-z_][A-Za-z0-9_]*)`")


def is_name(t):
    if len(t) < 5:
        return False
    if re.fullmatch(r"[a-z][a-z0-9]*(?:_[a-z0-9]+)+", t):
        return True
    if re.fullmatch(r"[A-Z][A-Z0-9]*(?:_[A-Z0-9]+)+", t):
        return True
    if re.fullmatch(r"[A-Z][a-z0-9]+(?:[A-Z][a-z0-9]+)+", t):
        return True
    return False


def names_candidates():
    corpus = {d: doc_text(d) for d in ("02", "05", "06", "07", "08", "10")}
    allt = "\n".join(corpus.values())
    seen = {}
    for i, ln in enumerate(L09, 1):
        for m in NAME.finditer(ln):
            t = m.group(1)
            if is_name(t):
                seen.setdefault(t, []).append(i)
    absent = {t: ls for t, ls in seen.items() if t not in allt}
    in06 = {t for t in seen if t in corpus["06"]}
    in07 = {t for t in seen if t in corpus["07"]}
    return seen, absent, in06, in07


def pkg_m2():
    seen, absent, in06, in07 = names_candidates()
    body = [HEADER.format(pid="M2", title="09'un adları ↔ 06 §4, §5 ve 07; yeni alan/kural taraması",
                          outfile=str(OUT / "mercek_M2.md"))]
    body.append("## Görev\n\n`09`'da ters tırnaklı ad (eşleme anahtarı, enum değeri, hata kodu, tablo/sütun/işlem adı, uç adı) `06`/`07`/`02`/`05`/`08`'de "
                "tanımlı olmalıdır (`09 §1`: ad uydurulmaz). Aşağıda betik adayları var: **A)** hiçbir üst dokümanda (02,05,06,07,08,10) geçmeyen adlar; "
                "**B)** `09`'da `06`/`07` adı olarak anılan ama yazımı kaynaktakinden farklı görünenler (yalnız kaynakta benzeri bulunanlar). "
                "Her aday için: bu ad `09`'un kendi kalıp adı mı (sınıf/yardımcı/betik/iş adı — bulgu değil) yoksa bir kaynak dokümanın sahip olması gereken "
                "adı mı (varlık/sütun/enum/hata kodu/ayar adı — bulgu)? Karar için satır bağlamını oku (`sed -n` ile `Docs/09_CODING_GUIDELINES.md` aralık); "
                "kaynak dokümanlarda `grep -n -i` ile benzer yazım ara (büyük dosyalar bütün okunmaz).\n")
    body.append(f"\nToplam farklı ad: {len(seen)} · `06`'da geçen: {len(in06)} · `07`'de geçen: {len(in07)} · hiçbirinde geçmeyen (A): {len(absent)}\n\n### A) Üst dokümanlarda geçmeyen adlar\n")
    used = nb("".join(body))
    rows = []
    for t, ls in sorted(absent.items()):
        ln = L09[ls[0] - 1].strip()
        rows.append(f"- `{t}` ×{len(ls)} satır {','.join(map(str, ls[:4]))} | {ln[:260]}\n")
    # B: near-miss (case-insensitive / underscore-insensitive equivalent present in 06/07)
    corp = (doc_text("06") + "\n" + doc_text("07")).lower().replace("_", "")
    near = []
    for t, ls in sorted(absent.items()):
        k = t.lower().replace("_", "")
        if k in corp:
            near.append(f"- `{t}` (satır {ls[0]}) — `06`/`07`'de büyük-küçük harf ya da alt çizgi farkıyla geçiyor\n")
    out = "".join(body)
    out += "".join(rows)[: CAP - used - 6000]
    out += "\n### B) Yazım farkıyla geçenler\n" + ("".join(near) or "(yok)\n")
    return out, len(absent)


# ------------------------------------------------------------- 3.5 values (M3)
def section_lines(doc, num, cap=14_000):
    keys = ap.upstream_keys(doc)
    if num not in keys:
        return f"[{doc} §{num} bulunamadı]\n"
    a, end, _ = keys[num]
    lines = ap.read_lines(DOCS / FILES[doc])
    t = "\n".join(lines[a - 1:end])
    return t[:cap] + ("\n[...kesildi]" if len(t) > cap else "") + "\n"


def pkg_m3():
    s = HEADER.format(pid="M3", title="09 §3.5 değer tablosu ↔ 05 ve 02 §11",
                      outfile=str(OUT / "mercek_M3.md"))
    s += "## Görev\n\n`09 §3.5` (ayarlar, ürün sabitleri, süre ve tavan değerleri) satır satır `02 §11` (parametre envanteri), `05 §8.4` (ayarlar) ve `05`'in " \
         "değer verdiği yerlerle karşılaştır: aynı değer, aynı birim, aynı ad, aynı kaynak/katman (firma ayarı · kurulum ayarı · ürün sabiti). " \
         "Ayrıca `09`'un başka bölümlerinde (`§10.7`, `§13.1`, `§15.4`, `§15.7` ...) §3.5'teki bir değeri yineleyen cümleleri `grep -n` ile bul ve eşleştir " \
         "(devredilen sınıf: iki yerin metin farkı; örnek: §3.5 ile §15.7'nin 'sabit' ibaresi).\n\n"
    s += "## 09 §3.5\n\n" + section_lines("09", "3.5", 30_000) if False else ""
    keys = ap.build_heads(L09, 6)[1]
    a, end, _ = keys["3.5"]
    s += "## 09 §3.5 (satır %d-%d)\n\n%s\n" % (a, end, "\n".join(L09[a - 1:end]))
    for d, n, cap in (("02", "11", 60_000), ("05", "8.4", 24_000)):
        s += f"\n## {d} §{n}\n\n" + section_lines(d, n, cap)
    # 09 mentions of ProductConstants / lines with units, for the lens to cross-check
    hits = []
    for i, ln in enumerate(L09, 1):
        if a <= i <= end:
            continue
        if re.search(r"§\s*3\.5|ProductConstants|ürün sabit", ln):
            hits.append(f"- 09:{i} | {ln.strip()[:300]}\n")
    s += "\n## 09'da §3.5'e ya da ürün sabitlerine değinen satırlar (kendi bölümü dışında)\n\n" + "".join(hits[:120])
    return s[:CAP * 1]


# ------------------------------------------------------------- libraries (M4)
def pkg_m4():
    s = HEADER.format(pid="M4", title="09 Kütüphaneler tablosu ↔ gövde",
                      outfile=str(OUT / "mercek_M4.md"))
    s += "## Görev\n\n`09`'un Kütüphaneler tablosundaki her satır (kütüphane, görev, lisans, lisans koşulu, bakım kanıtı, karar) ile `09` gövdesinde o kütüphaneyi anan " \
         "yerler (kural, sürüm, ayar, kalıp), `05` ve `DEPLOY_RUNBOOK`'taki anışlar tutarlı mı? Tabloda olmayıp gövdede kullanılan kütüphane/paket var mı? " \
         "Gövdede 'seçilmedi/elendi' denen kütüphane tabloda ya da başka yerde kullanılıyor mu (Kütüphanesiz kararlar listesi)? Lisans koşulu hücresi ile §3.3 işletimi " \
         "ve ilgili K kararı uyumlu mu? Sürüm numarası iki yerde farklı mı?\n\n"
    heads = ap.build_heads(L09, 6)[0]
    ia = next(i for i, ln in enumerate(L09, 1) if ln.strip() == "### Kütüphaneler")
    ib = next(i for i, ln in enumerate(L09, 1) if i > ia and ln.startswith("## "))
    s += "## 09 — Kütüphaneler bölümü (satır %d-%d)\n\n%s\n" % (ia, ib - 1, "\n".join(L09[ia - 1:ib - 1]))
    s += "\n## 09 §3.2, §3.3\n\n" + "\n".join(L09[244:276]) + "\n"
    names = []
    for ln in L09[ia - 1:ib - 1]:
        if ln.startswith("| ") and not ln.startswith("| ---") and not ln.startswith("| Kütüphane"):
            c = ln.split(" | ")[0].strip("| ").strip("`")
            names.append(c.split(" ")[0].strip("`"))
    s += "\n## Her kütüphane için gövde ve komşu doküman anışları\n"
    t05, tdr, t08 = doc_text("05"), doc_text("DEPLOY_RUNBOOK"), doc_text("08")
    for n in names:
        key = n.split("(")[0]
        if len(key) < 4:
            continue
        s += f"\n### {n}\n"
        c = 0
        for i, ln in enumerate(L09, 1):
            if ia <= i < ib:
                continue
            if key in ln:
                s += f"- 09:{i} | {ln.strip()[:240]}\n"
                c += 1
                if c >= 10:
                    break
        for nm, t in (("05", t05), ("DEPLOY_RUNBOOK", tdr), ("08", t08)):
            cc = 0
            for j, ln in enumerate(t.split("\n"), 1):
                if key in ln:
                    s += f"- {nm}:{j} | {ln.strip()[:200]}\n"
                    cc += 1
                    if cc >= 4:
                        break
    # dotted package-like tokens in 09 not in table
    tbl = "\n".join(L09[ia - 1:ib - 1])
    extra = {}
    for i, ln in enumerate(L09, 1):
        if ia <= i < ib:
            continue
        for m in re.finditer(r"`((?:Microsoft|System|Serilog|AWSSDK|Npgsql|Magick|SixLabors|Ganss|TngTech|xunit|Testcontainers|Polly|Dapper|Hangfire|Quartz|FluentValidation|MediatR|AutoMapper)[A-Za-z0-9_.\-]*)`", ln):
            if m.group(1) not in tbl:
                extra.setdefault(m.group(1), []).append(i)
    s += "\n## Gövdede anılan, tabloda geçmeyen paket benzeri adlar (betik adayı)\n"
    for k, ls in sorted(extra.items()):
        s += f"- `{k}` satır {','.join(map(str, ls[:5]))} | {L09[ls[0] - 1].strip()[:220]}\n"
    return s


# ------------------------------------------------------------- feedback table (M5)
def pkg_m5():
    s = HEADER.format(pid="M5", title="09 §2 'Üst dokümanlara geri besleme' tablosu ↔ hedeflerin gerçek metni",
                      outfile=str(OUT / "mercek_M5.md"))
    s += "## Görev\n\nTablonun her satırı bir üst doküman bölümünde bir cümlenin düzeldiğini söyler (sürüm ve karar ile). Her satır için hedef bölümün gerçek metnini oku: " \
         "söylenen düzeltme orada duruyor mu, sürüm notu (dokümanın durum satırı: `Docs/PRODUCT_DISCOVERY_STATUS.md` §1) o sürümü ve K'yı anıyor mu, " \
         "`09`'un ilgili gövde cümlesi hedefin metniyle çelişiyor mu? Tabloda olması gerekip olmayan geri besleme var mı (K-1302…K-1392 etki sütununda `02`/`05`/`06`/`07` gösterip " \
         "tabloda satırı olmayan; aşağıdaki B listesi)?\n\n"
    ia = next(i for i, ln in enumerate(L09, 1) if ln.startswith("#### Üst dokümanlara geri besleme"))
    ib = next(i for i, ln in enumerate(L09, 1) if i > ia and ln.startswith("*Kaynak"))
    s += "## 09 — geri besleme bölümü (satır %d-%d)\n\n%s\n" % (ia, ib, "\n".join(L09[ia - 1:ib]))
    rows = [ln for ln in L09[ia - 1:ib] if ln.startswith("| `")]
    s += "\n## Satır satır hedef metinler\n"
    tv = tracker_versions()
    for ln in rows:
        c = [x.strip() for x in ln.strip("|").split("|")]
        if len(c) < 3:
            continue
        refs = re.findall(r"`(0\d) §([^`]*)`", c[0])
        s += f"\n### {c[0][:200]} | {c[1]} | {c[2]}\n"
        for d, nums in refs:
            for n in re.findall(r"\d+(?:\.\d+)*", nums):
                s += f"\n**{d} §{n}** (tracker sürümü {tv.get(d, ('?',))[0]}):\n" + section_lines(d, n, 2600)
        km = re.findall(r"K-(\d+)", c[2])
        for k in km[:1]:
            r = REG.get(int(k))
            if r:
                parts = r[2].split(" | ")
                s += f"\n*K-{k} karar (kısaltılmış):* {parts[3][:900] if len(parts) > 3 else ''}\n"
    # B list
    inrows = set()
    for ln in rows:
        for d, nums in re.findall(r"`(0\d) §([^`]*)`", ln):
            inrows.add(d)
    s += "\n## B) K-1302…K-1392 etki sütunu 02/05/06/07/08 gösteren kararlar (tabloda satırı olmayanlar; doküman → K)\n"
    tabk = set(int(x) for ln in rows for x in re.findall(r"K-(\d+)", ln))
    for k in range(K_LO, K_HI + 1):
        r = REG.get(k)
        if not r:
            continue
        parts = r[2].split(" | ")
        etki = parts[5] if len(parts) > 5 else ""
        tg = sorted(x for x in etki_targets(etki) if x in ("02", "05", "06", "07", "08"))
        if tg and k not in tabk:
            s += f"- K-{k}: {tg} | {etki[:240]}\n"
    return s


# ------------------------------------------------------------- 09 slices (S)
def pkg_slices():
    heads = [(i, ln) for i, ln in enumerate(L09, 1) if ln.startswith("## ") or ln.startswith("### ")]
    cuts = [h[0] for h in heads]
    packs, cur_a, cur_b, size = [], 1, 0, 0
    bounds = cuts + [len(L09) + 1]
    for j, a in enumerate(bounds[:-1]):
        b = bounds[j + 1] - 1
        sz = nb("\n".join(L09[a - 1:b]))
        if cur_b == 0:
            cur_a, cur_b, size = (1 if j == 0 else a), b, sz
            continue
        if size + sz > 80_000:
            packs.append((cur_a, cur_b))
            cur_a, cur_b, size = a, b, sz
        else:
            cur_b, size = b, size + sz
    packs.append((cur_a, cur_b))
    out = []
    for n, (a, b) in enumerate(packs, 1):
        s = HEADER.format(pid=f"S{n}", title=f"09 satır {a}-{b}: özet cümle ↔ ayrıntı ayrışması (devredilen sınıf)",
                          outfile=str(OUT / f"mercek_S{n}.md"))
        s += ("## Görev\n\nDevredilen sınıf: **özet hücre / genel cümle ile uygulama bölümü / ayrıntının metin farkı** (kaynak: `PHASE8_CONFLICT_SCAN.md` §8; "
              "bu aşamada `09 §3.5` ile `§15.7`'nin 'sabit' ibaresi). Bu dilimdeki her tabloyu, listeyi ve genel kuralı, aynı konuyu başka yerde "
              "(bu dilimde ya da `grep -n` ile `09`'un diğer bölümlerinde) anlatan ayrıntıyla karşılaştır: sayı, süre, ad, kapalı liste öğeleri, "
              "koşul (`yalnız`/`her`/`hiç`), kapsam ve istisna sayısı, katman (ayar/sabit), bölüm atıfı (`§N` gerçekten o konuyu anlatıyor mu). "
              "Okurken 'Kaynak:' satırı atıflarının ve iç `§` atıflarının hedefte var olduğuna da bak. Dilimin dışına çıkan kanıtı birebir alıntıla.\n\n"
              f"## 09 satır {a}-{b}\n\n")
        s += "\n".join(L09[a - 1:b]) + "\n"
        out.append((n, a, b, s))
    return out


def main():
    mek = [version_check()]
    per_k, miss, extra = etki_check()
    mek.append(f"\n## Etki sütunu ↔ hedef (K-{K_LO}…K-{K_HI})\n\n- karar: {len(per_k)} · A adayı (etki var, hedefte K yok): {len(miss)} · B adayı: {len(extra)}")
    seen, absent, in06, in07 = names_candidates()
    mek.append(f"\n## Adlar\n\n- farklı ad {len(seen)}, 06'da {len(in06)}, 07'de {len(in07)}, üst dokümanlarda hiç geçmeyen {len(absent)}")
    (OUT / "mek.txt").write_text("\n".join(mek), encoding="utf-8")
    table = []
    pk = {"M1": pkg_m1(per_k, miss, extra), "M2": pkg_m2()[0], "M3": pkg_m3(), "M4": pkg_m4(), "M5": pkg_m5()}
    for n, a, b, t in pkg_slices():
        pk[f"S{n}"] = t
    for pid, t in pk.items():
        if nb(t) > 90_000:
            t = t.encode("utf-8")[:89_000].decode("utf-8", "ignore") + "\n[...paket sınırında kesildi]\n"
        (OUT / f"paket_{pid}.md").write_text(t, encoding="utf-8")
        table.append(f"{pid}\t{nb(t) / 1024:.1f} KB")
    print("\n".join(table))
    print("\n".join(mek))


main()
