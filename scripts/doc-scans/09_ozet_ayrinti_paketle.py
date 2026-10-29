"""Builds comparison packages (summary place + detail text) for 09_CODING_GUIDELINES.md.

Usage: python -I 09_ozet_ayrinti_paketle.py [output_dir]
Outputs paket_G<n>_<i>.md files next to this script. Prints summary-place count per group.
"""
import re
import os
from pathlib import Path

ROOT = Path(r"C:\projects\Shopfolio")
DOC = ROOT / "Docs" / "09_CODING_GUIDELINES.md"
import sys
OUT = Path(sys.argv[1]) if len(sys.argv) > 1 else Path(__file__).resolve().parent
CAP = 75_000

LINES = DOC.read_text(encoding="utf-8").split("\n")
N = len(LINES)


def L(a, b):
    """1-based inclusive line range text."""
    return "\n".join(LINES[a - 1:b])


# ---------- section map ----------
HEAD = re.compile(r"^(#{2,4}) (\d+(?:\.\d+)*)\.? ")
heads = []  # (line, level, key, title)
in_fence = False
for i, ln in enumerate(LINES, 1):
    if ln.strip().startswith("```"):
        in_fence = not in_fence
        continue
    if in_fence:
        continue
    m = HEAD.match(ln)
    if m:
        heads.append((i, len(m.group(1)), m.group(2), ln))
KEYS = {}
for idx, (ln, lvl, key, title) in enumerate(heads):
    end = N
    for ln2, lvl2, _, _ in heads[idx + 1:]:
        if lvl2 <= lvl:
            end = ln2 - 1
            break
    intro_end = end
    for ln2, lvl2, _, _ in heads[idx + 1:]:
        if lvl2 > lvl:
            intro_end = ln2 - 1
        break
    KEYS[key] = (ln, end, intro_end, title)
# the numberless "### Kütüphaneler" heading is separate; handled by explicit ranges below


def line_of(pattern, start=1):
    rx = re.compile(pattern)
    for i in range(start, N + 1):
        if rx.search(LINES[i - 1]):
            return i
    raise SystemExit("pattern not found: " + pattern)


def section_of_line(n):
    best = "baş"
    for ln, lvl, key, _ in heads:
        if ln <= n:
            best = key
        else:
            break
    return best


# ---------- refs ----------
REF = re.compile(r"§\s*(\d+(?:\.\d+){0,4})")
FOREIGN_PREV = re.compile(r"(kaydı|INSTRUCTIONS|GUARDRAILS|CONTEXT\.md|\.md|rapor|tracker)\s*$")


def local_refs(text):
    """Return list of (key, item_or_None, raw) for local (unprefixed) section refs."""
    out = []
    for line in text.split("\n"):
        foreign_until = -1
        for m in REF.finditer(line):
            s = m.start()
            before = line[:s]
            foreign = False
            if before.count("`") % 2 == 1:
                span = before[before.rfind("`") + 1:]
                if re.match(r"^\d\d\b", span) or re.match(r"^[A-Z]{2,}", span):
                    foreign = True
            if re.search(r"`\d\d\s*$", before) or re.search(r"\b\d\d\s*$", before):
                foreign = True
            if FOREIGN_PREV.search(before):
                foreign = True
            if foreign_until >= 0:
                between = line[foreign_until:s]
                if re.fullmatch(r"[\s,`–…]*(ve\s*)?", between):
                    foreign = True
            if foreign:
                foreign_until = m.end()
                continue
            foreign_until = -1
            parts = m.group(1).split(".")
            key = ".".join(parts[:2]) if len(parts) > 1 else parts[0]
            after = line[m.end():m.end() + 40]
            im = re.match(r"\s*madde\s+(\d+)", after)
            item = im.group(1) if im else None
            out.append((key, item, m.group(0) + (after[:im.end()] if im else "")))
    return out


def words(text):
    ws = re.findall(r"[A-Za-zÇĞİÖŞÜçğıöşü0-9_]{5,}", text.replace("`", " "))
    return {w.lower()[:6] for w in ws}


def paragraphs(a, b):
    blocks, cur = [], []
    for ln in LINES[a - 1:b]:
        if ln.strip() == "":
            if cur:
                blocks.append("\n".join(cur))
                cur = []
        else:
            cur.append(ln)
    if cur:
        blocks.append("\n".join(cur))
    return blocks


def evidence(key, item, query, limit=1800):
    if key not in KEYS:
        return f"[ÖLÜ ATIF? §{key} başlığı yok]"
    a, end, intro_end, title = KEYS[key]
    # for a level-2 section with children and no item, use intro + children paragraphs scoring
    blocks = paragraphs(a, end)
    if item:
        pat = re.compile(rf"^{item}\. ")
        for li in range(a, end + 1):
            if pat.match(LINES[li - 1]):
                got = [LINES[li - 1]]
                for lj in range(li + 1, end + 1):
                    nx = LINES[lj - 1]
                    if re.match(r"^\d+\. ", nx) or nx.startswith("*Kaynak") or nx.startswith("## ") or nx.startswith("### "):
                        break
                    if nx.strip() == "" or nx.startswith("   ") or nx.startswith("|"):
                        got.append(nx)
                    else:
                        break
                txt = "\n".join(got).rstrip()
                return f"[§{key} madde {item}] " + txt[:limit * 3]
        return f"[ÖLÜ ATIF? §{key} içinde madde {item} bulunamadı] " + "\n".join(LINES[a - 1:a])
    qw = words(query)
    scored = []
    for bi, blk in enumerate(blocks):
        sc = len(qw & words(blk))
        scored.append((sc, bi))
    scored.sort(key=lambda t: (-t[0], t[1]))
    chosen = sorted(bi for sc, bi in scored[:3] if sc > 0)
    out, used = [], 0
    for bi in chosen:
        blk = blocks[bi]
        if used + len(blk) > limit:
            blk = blk[: max(200, limit - used)] + " […kesildi]"
        out.append(blk)
        used += len(blk)
        if used >= limit:
            break
    head = LINES[a - 1]
    return f"[§{key}: {head.lstrip('# ')[:80]}] " + ("\n\n".join(out) if out else "(eşleşen paragraf yok; bölüm girişi: " + L(a, min(intro_end, a + 6))[:600] + ")")


def table_rows(a, b):
    rows, sub = [], ""
    for i in range(a, b + 1):
        ln = LINES[i - 1]
        if re.match(r"^\*\*[^*|]+\*\*\s*$", ln.strip()):
            sub = ln.strip().strip("*")
        if ln.lstrip().startswith("|"):
            cols = [c.strip() for c in ln.strip().strip("|").split("|")]
            if all(re.fullmatch(r":?-{3,}:?", c) for c in cols):
                continue
            rows.append((i, sub, cols))
    # drop header rows (first row after each separator is header: detect by previous line)
    clean = []
    for i, sub, cols in rows:
        prev_is_blank = i - 1 < 1 or not LINES[i - 2].lstrip().startswith("|")
        if prev_is_blank:
            continue
        clean.append((i, sub, cols))
    return clean


def emit(group, blocks, header):
    """blocks: list of strings. Writes chunked files."""
    files, cur, size = [], [header], len(header)
    for blk in blocks:
        if len(blk) > CAP:
            blk = blk[:CAP] + "\n[…blok kesildi]"
        if size + len(blk) > CAP and len(cur) > 1:
            files.append("\n\n".join(cur))
            cur, size = [header], len(header)
        cur.append(blk)
        size += len(blk) + 2
    files.append("\n\n".join(cur))
    for old in OUT.glob(f"paket_{group}_*.md"):
        old.unlink()
    for i, txt in enumerate(files, 1):
        (OUT / f"paket_{group}_{i}.md").write_text(txt, encoding="utf-8")
    return len(files)


def kb(n):
    return f"{n / 1024:.1f} KB"


HDR = "# Paket {g} — {t}\n\nHer blok: ÖZET YERİ (satır no ile) + AYRINTI (kaynak bölümden seçilmiş paragraflar). 'ÖLÜ ATIF?' işaretleri betiğin ön bulgusudur; doğrula.\n"
counts = {}

# =================== G1 header note ===================
a = line_of(r"^> \*\*Yazım durumu")
b = line_of(r"^\*Kaynak: K-1302 · K-1305")
blocks = []
yd = line_of(r"^> \*\*Yazım durumu")
pm = line_of(r"^> \*\*Park maddelerinin durumu")
kv = line_of(r"^\*\*Yazım konvansiyonları")
hm = line_of(r"^\*\*Alt bölüm haritası")
ok = line_of(r"^\*\*Okuma tablosu")
blocks.append(f"### ÖZET YERİ G1-a: Yazım durumu (satır {yd})\n" + L(yd, yd))
blocks.append(f"### ÖZET YERİ G1-b: Park maddelerinin durumu (satır {pm})\n" + L(pm, pm))
# park bullets
park_blocks = []
cur = None
bullets = []
for i in range(1, kv):
    ln = LINES[i - 1]
    if ln.startswith("> **Aşama") and "park edilen" in ln:
        cur = re.match(r"> \*\*(Aşama \d+)", ln).group(1)
    elif cur and ln.startswith("> - **"):
        bullets.append((cur, i, ln[2:][:230].replace("\n", " ")))
dizin = [x for x in bullets if "Dizin" in x[2][:80]]
real = [x for x in bullets if x not in dizin]
bl = [f"Park blokları: {len(real)} madde (Dizin satırları hariç; Dizin satırı {len(dizin)})."]
for cur_, ln, txt in bullets:
    bl.append(f"- satır {ln} [{cur_}] {txt}")
blocks.append("### AYRINTI G1-b: gerçek park maddeleri (başlık notunun park blokları)\n" + "\n".join(bl))
# conventions
conv = []
for i in range(kv, hm):
    if re.match(r"^\d+\. \*\*", LINES[i - 1]):
        conv.append(i)
for n_, i in enumerate(conv, 1):
    nxt = conv[n_] if n_ < len(conv) else hm
    blocks.append(f"### ÖZET YERİ G1-c{n_}: Yazım konvansiyonu {n_} (satır {i})\n" + L(i, nxt - 1))
# convention usage in body
use = []
for n_ in range(1, 11):
    hits = [(i, section_of_line(i), LINES[i - 1]) for i in range(hm, N + 1) if re.search(rf"yazım konvansiyonu(?:\s+\d+,)*\s*(?:{n_}\b)|konvansiyon {n_}\b", LINES[i - 1])]
    use.append(f"- Konvansiyon {n_}: gövdede {len(hits)} satır anıyor; ilk 4: " + " | ".join(f"s{h[0]} §{h[1]}: " + (re.search(rf".{{0,90}}konvansiyon[^|]{{0,5}}{n_}.{{0,60}}", h[2]) or re.search(r".{0,90}", h[2])).group(0) for h in hits[:4]))
blocks.append("### AYRINTI G1-c: konvansiyonların gövdedeki kullanımı\n" + "\n".join(use))
# map
hm_rows = table_rows(hm, ok - 1)
blocks.append(f"### ÖZET YERİ G1-d: Alt bölüm haritası (satır {hm}-{ok-1}); {len(hm_rows)} satır\n" + L(hm, ok - 1))
idx = ["Gerçek başlıklar (satır, düzey, boyut):"]
for ln, lvl, key, title in heads:
    a_, end, _, _ = KEYS[key]
    size = sum(len(x.encode()) + 1 for x in LINES[a_ - 1:end])
    idx.append(f"- satır {ln} {'#' * lvl} {title.lstrip('# ')[:90]} [{kb(size)}]")
numless = [(i, LINES[i - 1]) for i in range(1, N + 1) if re.match(r"^#{2,4} [^0-9]", LINES[i - 1]) and not LINES[i - 1].startswith("# ")]
idx.append("Numarasız başlıklar:")
for i, t in numless:
    idx.append(f"- satır {i} {t[:80]}")
blocks.append("### AYRINTI G1-d/e: gerçek başlık dizini ve boyları\n" + "\n".join(idx))
# okuma tablosu
ok_end = line_of(r"^\*Kaynak: K-1302 · K-1305", ok)
ok_rows = table_rows(ok, ok_end)
blocks.append(f"### ÖZET YERİ G1-e: Okuma tablosu (satır {ok}-{ok_end - 1}); {len(ok_rows)} satır\n" + L(ok, ok_end))
core = ["1", "2", "5", "6", "23", "24", "25"]
sizes = []
tot = 0
for k in core:
    a_, end, _, t = KEYS[k]
    s = sum(len(x.encode()) + 1 for x in LINES[a_ - 1:end])
    tot += s
    sizes.append(f"- §{k} {t.lstrip('# ')[:50]}: {kb(s)}")
sizes.append(f"Çekirdek toplamı: {kb(tot)}; dosya: {kb(sum(len(x.encode()) + 1 for x in LINES))}")
blocks.append("### AYRINTI G1-e: çekirdek bölümlerin boyları (okuma tablosu için)\n" + "\n".join(sizes))
blocks.append("### AYRINTI G1-e2: okuma tablosundaki her § atfının hedef başlığı\n" + "\n".join(
    f"- satır {i}: " + "; ".join(f"§{k}" + ("" if k in KEYS else " (ÖLÜ?)") for k, _, _ in local_refs(" | ".join(c[1:]))) for i, _, c in ok_rows if len(c) > 1))
counts["G1"] = (len(blocks), emit("G1", blocks, HDR.format(g="G1", t="Başlık notu")))
g1_places = 2 + len(conv) + len(hm_rows) + len(ok_rows)

# =================== G2 ===================
s35 = KEYS["3.5"][0]
lib_h = line_of(r"^### Kütüphaneler")
lib_end = line_of(r"^\*Kaynak: K-1305", lib_h)
s35_end = lib_h - 1
blocks = []
rows35 = table_rows(s35, s35_end)
blocks.append(f"### ÖZET YERİ G2-a: §3.5 metni (satır {s35}-{s35_end}); {len(rows35)} tablo satırı\n" + L(s35, s35_end))
UNIT = r"(?:saniye|dakika|saat|gün|MB|KB|GB|piksel|megapiksel|kez|adet|karakter|satır|bayt|ay|hafta|milisaniye|%)"
NUM = re.compile(rf"(\d[\d.,]*)\s*{UNIT}\b")
txt35 = L(s35, lib_end)
tok35 = sorted(set(m.group(0) for m in NUM.finditer(L(s35, s35_end))))
occ = []
for tk in tok35:
    hits = [i for i in range(1, N + 1) if tk in LINES[i - 1] and not (s35 <= i <= lib_end)]
    if hits:
        occ.append(f"- `{tk}` §3.5 dışında {len(hits)} satırda: " + " || ".join(f"s{i} §{section_of_line(i)}: {LINES[i-1].strip()[:160]}" for i in hits[:5]))
    else:
        occ.append(f"- `{tk}` §3.5 dışında geçmiyor")
blocks.append("### AYRINTI G2-a: §3.5'teki sayı+birim belirteçlerinin gövdedeki karşılıkları\n" + "\n".join(occ))
# numbers outside 3.5 not in 3.5
body_nums = []
t35 = L(s35, s35_end)
for i in range(1, N + 1):
    if s35 <= i <= lib_end or i < kv:
        continue
    ln = LINES[i - 1]
    ms = [m.group(0) for m in NUM.finditer(ln)]
    new = [m for m in ms if m not in t35]
    if new:
        body_nums.append(f"s{i} §{section_of_line(i)} [{', '.join(sorted(set(new))[:5])}]: {ln.strip()[:170]}")
chunk = "\n".join(body_nums)
blocks.append(f"### AYRINTI G2-b: gövdede olup §3.5'te geçmeyen sayı+birim satırları ({len(body_nums)} satır; tabloya girmesi gereken sabit/süre/tavan var mı?)\n" + chunk)
# libraries
lib_rows = table_rows(lib_h, lib_end)
blocks.append(f"### ÖZET YERİ G2-c: Kütüphaneler tablosu (satır {lib_h}-{lib_end}); {len(lib_rows)} satır\n" + L(lib_h, lib_end))
lib_names = []
for i, sub, cols in lib_rows:
    c = cols[0]
    m = re.search(r"`([^`]+)`", c)
    nm = m.group(1) if m else c.split()[0]
    lib_names.append((i, nm))
lo = []
for i, nm in lib_names:
    base = nm.split(" ")[0]
    hits = [j for j in range(1, N + 1) if base in LINES[j - 1] and not (lib_h <= j <= lib_end) and j >= kv]
    lo.append(f"- `{nm}` (tablo s{i}): tablo dışı {len(hits)} satır: " + " || ".join(f"s{j} §{section_of_line(j)}: {LINES[j-1].strip()[:170]}" for j in hits[:5]))
blocks.append("### AYRINTI G2-c: her kütüphanenin gövdedeki kullanım yerleri\n" + "\n".join(lo))
PKG = re.compile(r"`([A-Z][A-Za-z0-9]+(?:\.[A-Za-z0-9]+){1,}(?:-[A-Za-z0-9]+)?)`")
known = " ".join(n for _, n in lib_names)
cand = {}
for j in range(kv, N + 1):
    if lib_h <= j <= lib_end:
        continue
    for m in PKG.finditer(LINES[j - 1]):
        t = m.group(1)
        if t.split(".")[0] in ("Shopfolio", "System") or t in known:
            continue
        cand.setdefault(t, []).append(j)
cl = [f"- `{t}`: {len(v)} satır; ilk: s{v[0]} §{section_of_line(v[0])}: {LINES[v[0]-1].strip()[:150]}" for t, v in sorted(cand.items())]
blocks.append("### AYRINTI G2-c2: tabloda olmayan, paket/ad alanı biçiminde geçen adlar (aday; çoğu ad alanı/tip olabilir, paket olanı ayır)\n" + "\n".join(cl))
counts["G2"] = (len(rows35) + len(lib_rows), emit("G2", blocks, HDR.format(g="G2", t="§3 Kütüphaneler ve §3.5 değer tablosu")))

# =================== G3 §23 ===================
a23, e23 = KEYS["23"][0], KEYS["23"][1]
rows = table_rows(a23, e23)
blocks = [f"### BAĞLAM G3: §23 giriş\n" + L(a23, a23 + 8)]
for i, sub, cols in rows:
    q = cols[0]
    refs = local_refs(cols[1]) if len(cols) > 1 else []
    ev = []
    seen = set()
    for key, item, raw in refs:
        if (key, item) in seen:
            continue
        seen.add((key, item))
        ev.append(evidence(key, item, q, 1500))
    if not refs:
        ev.append("[ev sütununda yerel § atfı yok: " + (cols[1] if len(cols) > 1 else "") + "]")
    blocks.append(f"### ÖZET YERİ G3 s{i} [{sub}]\nYasak: {cols[0]}\nKuralın evi: {cols[1] if len(cols)>1 else ''}\n\nAYRINTI:\n" + "\n\n".join(ev))
counts["G3"] = (len(rows), emit("G3", blocks, HDR.format(g="G3", t="§23 Yasaklar")))

# =================== G4 §24 tables ===================
a24, e24 = KEYS["24"][0], KEYS["24"][1]
rows = table_rows(a24, e24)
blocks = []
for i, sub, cols in rows:
    q = " ".join(cols)
    refs = local_refs(cols[-1])
    ev, seen = [], set()
    for key, item, raw in refs:
        if (key, item) in seen:
            continue
        seen.add((key, item))
        ev.append(evidence(key, item, q, 1500))
    sym = re.findall(r"`([^`]+)`", cols[1]) if len(cols) == 3 else []
    symloc = []
    for sname in sym[:2]:
        base = sname.split("/")[-1].split(".")[0]
        hits = [j for j in range(kv, N + 1) if base in LINES[j - 1] and not (a24 <= j <= e24)]
        symloc.append(f"`{sname}` gövdede {len(hits)} satır; bölümler: " + ", ".join(sorted({section_of_line(j) for j in hits})[:12]))
    blocks.append(f"### ÖZET YERİ G4 s{i}\n" + " | ".join(cols) + "\n\nİşaret edilen sembolün yerleri: " + ("; ".join(symloc) or "-") + "\n\nAYRINTI:\n" + "\n\n".join(ev))
counts["G4"] = (len(rows), emit("G4", blocks, HDR.format(g="G4", t="§24 tek işlev tablosu ve kritik akış tablosu")))

# =================== G5 §25 ===================
a25, e25 = KEYS["25"][0], KEYS["25"][1]
rows = table_rows(a25, e25)
blocks = [f"### BAĞLAM G5: §25 giriş\n" + L(a25, a25 + 6)]
for i, sub, cols in rows:
    q = cols[0]
    refs = local_refs(cols[-1])
    ev, seen = [], set()
    for key, item, raw in refs:
        if (key, item) in seen:
            continue
        seen.add((key, item))
        ev.append(evidence(key, item, q, 1200))
    if not refs:
        ev.append("[bölüm sütununda yerel § atfı yok: " + cols[-1] + "]")
    blocks.append(f"### ÖZET YERİ G5 s{i} [{sub}]\nSoru: {cols[0]}\nBölüm: {cols[-1]}\n\nAYRINTI:\n" + "\n\n".join(ev))
counts["G5"] = (len(rows), emit("G5", blocks, HDR.format(g="G5", t="§25 inceleme listesi")))

# =================== G6 ===================
blocks = []
places = 0
# feedback table
fb_h = line_of(r"^#### Üst dokümanlara geri besleme")
fb_end = line_of(r"^\*\*Anahtar terim taraması", fb_h)
fb_rows = table_rows(fb_h, fb_end)
docs = {p.name[:2]: p for p in (ROOT / "Docs").glob("[0-9][0-9]_*.md")}
for i, sub, cols in fb_rows:
    if len(cols) < 3:
        continue
    places += 1
    m = re.match(r"`(\d\d) (§[\d.]+)`", cols[0]) or re.match(r"`(\d\d) (§[\d.]+)", cols[0])
    ev = ""
    if m and m.group(1) in docs:
        p = docs[m.group(1)]
        T = p.read_text(encoding="utf-8").split("\n")
        ver = next((x for x in T[:12] if "Versiyon" in x), "")
        num = m.group(2)[1:]
        found = None
        for j, x in enumerate(T):
            if re.match(rf"^#*\s*\**{re.escape(num)}[\s.\*]", x):
                found = j
                break
        sec = "\n".join(T[found:found + 14])[:2500] if found is not None else "(başlık bulunamadı: " + num + ")"
        kid = cols[2]
        kh = [f"s{j+1}: {x.strip()[:260]}" for j, x in enumerate(T) if kid in x][:4]
        ev = f"Hedef doküman `{p.name}`: {ver.strip()[:120]}\nBölüm {num} metni (ilk satırlar):\n{sec}\nHedefte {kid} geçen satırlar ({len(kh)}):\n" + "\n".join(kh)
    else:
        ev = "(hedef doküman çözülemedi)"
    blocks.append(f"### ÖZET YERİ G6-a s{i}\n" + " | ".join(cols) + "\n\nAYRINTI:\n" + ev)
# folder tree
t41 = KEYS["4.1"]
tree_txt = L(t41[0], t41[1])
blocks.append(f"### ÖZET YERİ G6-b: §4.1 klasör ağacı ve düzen (satır {t41[0]}-{t41[1]})\n" + tree_txt)
places += 1
PATH = re.compile(r"`((?:src|tests|scripts|\.github|Docs|wwwroot|Modules|Infrastructure|Pages|Endpoints|Migrations|Fakes|Consistency|Shopfolio\.[A-Za-z]+)/[A-Za-z0-9_./<>\-*]*)`")
pc = {}
for j in range(kv, N + 1):
    if t41[0] <= j <= t41[1]:
        continue
    for m in PATH.finditer(LINES[j - 1]):
        pc.setdefault(m.group(1), []).append(j)
pl = [f"- `{p}`: {len(v)} satır; ilk: s{v[0]} §{section_of_line(v[0])}: {LINES[v[0]-1].strip()[:140]}" for p, v in sorted(pc.items())]
blocks.append("### AYRINTI G6-b: gövdede anılan klasör/yol adları (§4.1 ağacındaki yerlerle uyuşuyor mu?)\n" + "\n".join(pl))
# 11 and DEPLOY_RUNBOOK blocks
for fname, label in (("11_IMPLEMENTATION_PLAN.md", "11"), ("DEPLOY_RUNBOOK.md", "DEPLOY_RUNBOOK")):
    T = (ROOT / "Docs" / fname).read_text(encoding="utf-8").split("\n")
    s = next(j for j, x in enumerate(T) if "Aşama 8'den park edilen girdiler" in x)
    e = s
    while e < len(T) and T[e].startswith(">"):
        e += 1
    for j in range(s, e):
        if T[j].startswith("> - **"):
            places += 1
            ks = re.findall(r"K-\d{3,4}", T[j][:200])
            refs09 = re.findall(r"`09 (§[\d.]+(?: madde \d+)?)`", T[j])
            ev = []
            for r_ in refs09:
                for key, item, raw in local_refs(r_):
                    ev.append(evidence(key, item, T[j], 1000))
            blocks.append(f"### ÖZET YERİ G6-c [{label}] s{j+1}\n{T[j][:1400]}\n\nKaynak K ids: {ks}\n`09` atıfları: {refs09}\n\nAYRINTI (`09` gövdesi):\n" + ("\n\n".join(ev[:4]) if ev else "(atıf yok)"))
    # K rows from decision log
KLOG = ROOT / "Docs" / "PRODUCT_DISCOVERY_STATUS.md"
want = set()
for fname in ("11_IMPLEMENTATION_PLAN.md", "DEPLOY_RUNBOOK.md"):
    T = (ROOT / "Docs" / fname).read_text(encoding="utf-8").split("\n")
    s = next(j for j, x in enumerate(T) if "Aşama 8'den park edilen girdiler" in x)
    e = s
    while e < len(T) and T[e].startswith(">"):
        e += 1
    for j in range(s, e):
        if T[j].startswith("> - **"):
            want.update(re.findall(r"K-\d{3,4}", T[j][:160]))
klines = []
with KLOG.open(encoding="utf-8") as f:
    for ln in f:
        m = re.match(r"^\| (K-\d+) \|", ln)
        if m and m.group(1) in want:
            klines.append(f"{m.group(1)}: " + ln[:700].replace("\n", " "))
blocks.append("### AYRINTI G6-c: karar kaydında bu K'ların satırları (ilk 700 karakter)\n" + "\n".join(klines))
counts["G6"] = (places, emit("G6", blocks, HDR.format(g="G6", t="geri besleme tablosu, klasör ağacı, 11 ve DEPLOY_RUNBOOK blokları")))

places_by = {"G1": g1_places, "G2": counts["G2"][0], "G3": counts["G3"][0], "G4": counts["G4"][0], "G5": counts["G5"][0], "G6": counts["G6"][0]}
for g in ("G1", "G2", "G3", "G4", "G5", "G6"):
    fs = sorted(OUT.glob(f"paket_{g}_*.md"))
    print(g, "özet yeri:", places_by[g], "dosya:", len(fs), "boyut:", [kb(f.stat().st_size) for f in fs])
print("toplam özet yeri:", sum(places_by.values()))
