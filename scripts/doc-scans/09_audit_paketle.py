"""Builds audit lens packages for Docs/09_CODING_GUIDELINES.md (Stage 8 quality loop, audit).

Usage:
  python -I 09_audit_paketle.py [output_dir]     build packages (paket_K<i>, paket_TE_1, paket_AT_1, paket_G<i>, paket_DO_1)
  python -I 09_audit_paketle.py --selftest       two-way check of the reference checker and the K pack resolver

Every package is a single file (<= 90 KB). K packages: a slice of 09 + the decision-register rows named by the
slice's "Kaynak" lines + the upper-document sections those lines (and the slice body) cite.
"""
import difflib
import re
import sys
from pathlib import Path

sys.stdout.reconfigure(encoding="utf-8")
ROOT = Path(r"C:\projects\Shopfolio")
DOCS = ROOT / "Docs"
CAP = 88_000          # hard cap per package (bytes)
SLICE_CAP = 46_000    # 09 text per K slice (bytes)
UP_CAP = 24_000       # upper-document excerpts per K slice (bytes)
UP_EXCERPT = 3_000    # chars per upper-document excerpt

OUT = Path(__file__).resolve().parent
SELFTEST = False
for a in sys.argv[1:]:
    if a == "--selftest":
        SELFTEST = True
    else:
        OUT = Path(a)

DOCFILES = {"02": "02_PRODUCT_REQUIREMENTS.md", "03": "03_USER_FLOWS.md", "04": "04_UI_SPECS.md",
            "05": "05_TECHNICAL_ARCHITECTURE.md", "06": "06_DATA_MODEL.md", "07": "07_API_DESIGN.md",
            "08": "08_INTEGRATION_SPEC.md"}
_cache = {}


def read_lines(path):
    if path not in _cache:
        _cache[path] = path.read_text(encoding="utf-8").split("\n")
    return _cache[path]


def nbytes(s):
    return len(s.encode("utf-8"))


# ---------------------------------------------------------------- headings
HEAD = re.compile(r"^(#{1,6}) (\d+(?:\.\d+)*)\.? ")


def build_heads(lines, maxlvl=6, track_fence=True):
    heads, fence = [], False
    for i, ln in enumerate(lines, 1):
        if track_fence and ln.strip().startswith("```"):
            fence = not fence
            continue
        if fence:
            continue
        m = HEAD.match(ln)
        if m and len(m.group(1)) <= maxlvl:
            heads.append((i, len(m.group(1)), m.group(2)))
    keys = {}
    for idx, (ln, lvl, key) in enumerate(heads):
        end = len(lines)
        for ln2, lvl2, _ in heads[idx + 1:]:
            if lvl2 <= lvl:
                end = ln2 - 1
                break
        if key not in keys:
            keys[key] = (ln, end, lvl)
    return heads, keys


PARA = re.compile(r"^\s*(?:[-*>|] ?)*\*{0,2}(\d+(?:\.\d+)+)(?=[\s.:*|])")


def upstream_keys(doc):
    """Heading keys plus bold paragraph numbers ('**8.8.2 Title**' at line start) of an upper document."""
    k = ("keys", doc)
    if k not in _cache:
        lines = read_lines(DOCS / DOCFILES[doc])
        # upper documents may carry an unbalanced code fence; headings are taken without fence tracking
        keys = dict(build_heads(lines, 6, False)[1])
        for i, ln in enumerate(lines, 1):
            m = PARA.match(ln)
            if m and m.group(1) not in keys:
                end = i
                if not lines[i - 1].lstrip().startswith("|"):
                    while end < len(lines) and end - i < 40 and lines[end].strip() != "":
                        end += 1
                keys[m.group(1)] = (i, end, 7)
        _cache[k] = keys
    return _cache[k]


# ---------------------------------------------------------------- decision register
def load_register():
    rows = {}
    for ln in read_lines(DOCS / "PRODUCT_DISCOVERY_STATUS.md"):
        if ln.startswith("| K-"):
            m = re.match(r"\| K-(\d+) \| ", ln)
            if m and int(m.group(1)) not in rows:
                cells = ln.split(" | ")
                rows[int(m.group(1))] = (cells[2] if len(cells) > 2 else "", cells[3] if len(cells) > 3 else "", ln)
    return rows


REG = load_register()


# ---------------------------------------------------------------- reference parsing
TOK = re.compile(r"`(\d\d)\s*§\s*([^`]*)`|`([^`]*)`|§\s*(\d+(?:\.\d+)*)")
FOREIGN_PREV = re.compile(
    r"(kaydı|kayıt|INSTRUCTIONS|GUARDRAILS|CONTEXT\.md|\.md|rapor|raporu|tracker|tracker'ın|checklist|checklists|skill|`\d\d|\b\d\d|\]\([^)]*\)"
    r"|(?:Mimari|Modeli|Tanımları|Spesifikasyonları|Gereksinimleri|Akışları|Planı|Protokolü|Kılavuzu|Kapsamı|Rehberi|Metodoloji|Vizyonu)(?:'\w+)?)\s*[`*)]*\s*$")
GAP_OK = re.compile(r"[\s,\u2013\u2026-]*(?:ve\s*)?")


def expand_range(prev, cur):
    pp, cp = prev.split("."), cur.split(".")
    if len(pp) == len(cp) and pp[:-1] == cp[:-1] and pp[-1].isdigit() and cp[-1].isdigit():
        a, b = int(pp[-1]), int(cp[-1])
        if a < b <= a + 30:
            return [".".join(pp[:-1] + [str(x)]) for x in range(a + 1, b)]
    return []


def parse_refs(line):
    """Return [(doc, num, item_or_None)]; doc is '02'..'08', '00' (other numbered), '?' (foreign file) or 'local'."""
    refs, cur, last_end, last = [], None, -1, None
    for m in TOK.finditer(line):
        if m.group(1):
            cur, last_end = m.group(1), m.end()
            last = None
            for n in re.findall(r"\d+(?:\.\d+)*", m.group(2)):
                refs.append((cur, n, None))
                last = (cur, n)
        elif m.group(3) is not None:
            cur, last_end, last = "?", m.end(), None
        else:
            gap = line[last_end:m.start()] if last_end >= 0 else None
            before = line[:m.start()]
            if cur and gap is not None and GAP_OK.fullmatch(gap):
                doc = cur
            elif FOREIGN_PREV.search(before):
                doc = "?"
            else:
                doc = "local"
            n = m.group(4)
            im = re.match(r"\s*madde\s+(\d+)", line[m.end():m.end() + 20])
            item = im.group(1) if im else None
            if gap and re.search(r"[\u2013\u2026]", gap) and last and last[0] == doc:
                for x in expand_range(last[1], n):
                    refs.append((doc, x, None))
            refs.append((doc, n, item))
            last = (doc, n)
            cur = doc if doc != "local" else None
            last_end = m.end()
    return refs


def kaynak_ks(text):
    out = set()
    for m in re.finditer(r"K-(\d+)\s*[\u2026\u2013]\s*K-(\d+)", text):
        lo, hi = int(m.group(1)), int(m.group(2))
        if 0 < hi - lo <= 60:
            out.update(range(lo, hi + 1))
    for m in re.finditer(r"K-(\d+)", text):
        out.add(int(m.group(1)))
    return out


# ---------------------------------------------------------------- doc model (09)
def load_09():
    return read_lines(DOCS / "09_CODING_GUIDELINES.md")


LINES = load_09()
N = len(LINES)
HEADS, KEYS = build_heads(LINES, 4)


def section_of_line(n, heads=None):
    best = "baş"
    for ln, lvl, key in (heads or HEADS):
        if ln <= n:
            best = key
        else:
            break
    return best


def L(a, b):
    return "\n".join(LINES[a - 1:b])


def numbered(a, b):
    return "\n".join(f"{i:4d}| {LINES[i - 1]}" for i in range(a, b + 1))


def units():
    first = HEADS[0][0]
    us = [("baş", 1, first - 1)]
    l2 = [h for h in HEADS if h[1] == 2]
    for ln, lvl, key in l2:
        a, end, _ = KEYS[key]
        kids = [h for h in HEADS if h[1] == 3 and a < h[0] <= end]
        if kids:
            us.append((f"§{key} giriş", a, kids[0][0] - 1))
            for kl, _, kk in kids:
                us.append((f"§{kk}", kl, KEYS[kk][1]))
        else:
            us.append((f"§{key}", a, end))
    return us


def slices():
    out, cur = [], None
    for lab, a, b in units():
        sz = nbytes(L(a, b)) + 6 * (b - a + 1)
        if cur and cur["size"] + sz > SLICE_CAP:
            out.append(cur)
            cur = None
        if cur is None:
            cur = {"labels": [], "a": a, "b": b, "size": 0}
        cur["labels"].append(lab)
        cur["b"] = b
        cur["size"] += sz
    out.append(cur)
    return out


# ---------------------------------------------------------------- K pack
def upstream_excerpt(doc, num):
    keys = upstream_keys(doc)
    if num not in keys:
        return None
    a, end, lvl = keys[num]
    lines = read_lines(DOCS / DOCFILES[doc])
    txt = "\n".join(lines[a - 1:end])
    cut = ""
    if len(txt) > UP_EXCERPT:
        cut = f"\n[...kesildi: bölüm {end - a + 1} satır]"
        txt = txt[:UP_EXCERPT]
    return a, end, txt + cut


def collect_up_refs(a, b):
    """Kaynak-line refs first, then body refs by frequency."""
    prio, body, unresolved = [], {}, []
    for i in range(a, b + 1):
        ln = LINES[i - 1]
        refs = parse_refs(ln)
        if ln.startswith("*Kaynak"):
            for d, n, _ in refs:
                if d in DOCFILES and (d, n) not in prio:
                    prio.append((d, n))
        else:
            for d, n, _ in refs:
                if d in DOCFILES:
                    body[(d, n)] = body.get((d, n), 0) + 1
    rest = sorted((k for k in body if k not in prio), key=lambda k: -body[k])
    return prio + rest


def build_k_pack(idx, sl):
    a, b = sl["a"], sl["b"]
    ks = set()
    for i in range(a, b + 1):
        if LINES[i - 1].startswith("*Kaynak"):
            ks |= kaynak_ks(LINES[i - 1])
    ups, used, parts_up, unresolved = collect_up_refs(a, b), 0, [], []
    for d, n in ups:
        ex = upstream_excerpt(d, n)
        if ex is None:
            unresolved.append(f"{d} §{n}")
            continue
        ea, eb, txt = ex
        blk = f"### Üst doküman {d} §{n} (satır {ea}-{eb})\n{txt}"
        if used + nbytes(blk) > UP_CAP:
            unresolved.append(f"{d} §{n} (bütçe doldu; paket dışı)")
            continue
        used += nbytes(blk)
        parts_up.append(blk)
    body = numbered(a, b)
    reg_ids = sorted(k for k in ks if k in REG)
    missing = sorted(k for k in ks if k not in REG)
    # shrink the decision cells (then the upper excerpts) until the package fits
    while True:
        fixed = nbytes(body) + sum(nbytes(p) for p in parts_up) + 3_000
        per = 3000
        if reg_ids:
            per = max(150, min(3000, (CAP - fixed) // len(reg_ids) - 120))
        if fixed + (per + 120) * len(reg_ids) <= CAP or not parts_up:
            break
        parts_up.pop()
        unresolved.append("(bütçe için paket dışı bırakıldı: bir üst bölüm)")
    while True:
        txt = assemble_k(idx, sl, body, reg_ids, missing, parts_up, unresolved, per)
        if nbytes(txt) <= CAP - 1_000 or (per <= 60 and not parts_up):
            break
        if per > 60:
            per = max(60, int(per * 0.85))
        else:
            parts_up.pop()
    return txt, len(reg_ids), len(parts_up)


def assemble_k(idx, sl, body, reg_ids, missing, parts_up, unresolved, per):
    a, b = sl["a"], sl["b"]
    kparts = []
    for k in reg_ids:
        konu, karar, _ = REG[k]
        kt = karar if len(karar) <= per else karar[:per] + " [...kesildi]"
        kparts.append(f"### K-{k} — {konu[:160]}\n{kt}")
    txt = (
        f"# Paket K{idx} — 09 dilimi: {', '.join(sl['labels'])} (satır {a}-{b})\n\n"
        "Yalnız bu dosyayı oku. A: 09 metni (satır numaralı). B: dilimin Kaynak satırlarının andığı karar kaydı satırları "
        f"(karar hücresi {per} karakterde kesilmiş olabilir). C: atıf yapılan üst doküman bölümleri (her biri {UP_EXCERPT} karakterde kesik). "
        "Bulguyu bulduğun anda dosyaya yaz.\n\n"
        f"## A. 09 metni\n\n{body}\n\n## B. Karar kaydı satırları ({len(reg_ids)} adet)\n\n" + "\n\n".join(kparts)
        + (f"\n\nKarar kaydında bulunamayan K: {', '.join('K-' + str(k) for k in missing)}" if missing else "")
        + f"\n\n## C. Üst doküman bölümleri ({len(parts_up)} adet)\n\n" + "\n\n".join(parts_up)
        + ("\n\nÇözülemeyen / pakete girmeyen üst atıflar: " + "; ".join(unresolved) if unresolved else "") + "\n"
    )
    return txt


# ---------------------------------------------------------------- AT (references)
def at_check(lines):
    heads, keys = build_heads(lines, 4)
    results, counts = [], {"local": 0, "up": 0, "K": 0, "UC": 0}
    status_text = "\n".join(read_lines(DOCS / "PRODUCT_DISCOVERY_STATUS.md"))
    uc_all = set(re.findall(r"UC-\d+\b", "\n".join("\n".join(read_lines(DOCS / f)) for f in DOCFILES.values())))
    uc_all |= set(re.findall(r"UC-\d+\b", status_text[:0]))
    fence = False
    for i, ln in enumerate(lines, 1):
        if ln.strip().startswith("```"):
            fence = not fence
        for d, n, item in parse_refs(ln):
            if d == "local":
                counts["local"] += 1
                if n not in keys:
                    results.append((i, f"ÖLÜ yerel atıf §{n}", ln))
                elif item:
                    a, end, _ = keys[n]
                    if not any(re.match(rf"^{item}\. ", lines[j - 1]) for j in range(a, end + 1)):
                        results.append((i, f"ÖLÜ madde atfı §{n} madde {item}", ln))
            elif d in DOCFILES:
                counts["up"] += 1
                if n not in upstream_keys(d):
                    results.append((i, f"ŞÜPHELİ üst atıf {d} §{n} (başlık yok)", ln))
        for m in re.finditer(r"K-(\d+)(?:\s*\((\d+)\))?", ln):
            counts["K"] += 1
            k = int(m.group(1))
            if k not in REG:
                results.append((i, f"ÖLÜ karar K-{k}", ln))
            elif m.group(2):
                cell = REG[k][1]
                if not re.search(rf"\({m.group(2)}\)", cell):
                    results.append((i, f"ŞÜPHELİ K-{k} ({m.group(2)}) — karar hücresinde ({m.group(2)}) numarası yok", ln))
        for m in re.finditer(r"UC-(\d+)\b", ln):
            counts["UC"] += 1
            if m.group(0) not in uc_all and f"UC-{m.group(1)}" not in status_text:
                results.append((i, f"ÖLÜ kullanım senaryosu {m.group(0)}", ln))
    return results, counts


def build_at_pack():
    res, counts = at_check(LINES)
    out = [f"# Paket AT — atıf denetimi\n\nBetik {sum(counts.values())} atfı kontrol etti: yerel § {counts['local']}, üst doküman § {counts['up']}, K {counts['K']}, UC {counts['UC']}. "
           f"Aşağıda yalnız ölü ya da şüpheli {len(res)} atıf var; her birini satırın bağlamında oku ve doğru hedefi bul (gerekirse yalnız o hedef bölümü aç). Yanlış pozitifleri ele.\n"]
    for i, why, ln in res:
        out.append(f"## satır {i} (§{section_of_line(i)}) — {why}\n{ln[:900]}")
    return "\n\n".join(out) + "\n", len(res), counts


# ---------------------------------------------------------------- TE (terms / enums / names)
IDENT = re.compile(r"^[A-Za-z_][A-Za-z0-9_]{2,}$")
EXT = re.compile(r"\.(cs|md|json|yml|yaml|sql|csproj|sln|props|razor|js|css|html|txt|sh|py|ps1|slnx)$", re.I)


def is_code_name(t):
    if not IDENT.match(t):
        return False
    return bool(re.search(r"[a-z][A-Z]|[A-Z][a-z]|_", t) and (re.search(r"[A-Z]", t) or "_" in t))


def build_te_pack():
    text_by_doc = {d: "\n".join(read_lines(DOCS / f)) for d, f in DOCFILES.items()}
    names_by_doc = {d: set(re.findall(r"[A-Za-z_][A-Za-z0-9_]{2,}", t)) for d, t in text_by_doc.items()}
    all_names = set().union(*names_by_doc.values())
    d06 = text_by_doc["06"]
    norm06 = {}
    for t in names_by_doc["06"]:
        norm06.setdefault(re.sub(r"[_]", "", t).lower(), t)
    occ = {}
    fence = False
    for i in range(1, N + 1):
        ln = LINES[i - 1]
        if ln.strip().startswith("```"):
            fence = not fence
        if i < HEADS[0][0] + 0 and False:
            continue
        for m in re.finditer(r"`([^`\n]+)`", ln):
            t = m.group(1).strip()
            t = re.sub(r"\(\)$", "", t)
            if "." in t and not EXT.search(t) and all(IDENT.match(p) or p == "" for p in t.split(".")):
                parts = t.split(".")
                for p in parts:
                    if is_code_name(p):
                        occ.setdefault(p, []).append(i)
                continue
            if is_code_name(t) and not EXT.search(t):
                occ.setdefault(t, []).append(i)
    nomatch, near, ok = [], [], 0
    pool = list(norm06.keys())
    for t, lns in sorted(occ.items()):
        if t in names_by_doc["06"]:
            ok += 1
            continue
        nk = re.sub(r"_", "", t).lower()
        hit = norm06.get(nk)
        if hit and hit != t:
            near.append((t, lns, hit, "yalnız harf/altçizgi farkı"))
            continue
        if t in all_names:
            ok += 1
            continue
        cm = difflib.get_close_matches(nk, pool, n=2, cutoff=0.86) if len(nk) >= 6 else []
        if cm:
            near.append((t, lns, norm06[cm[0]], "yakın yazım"))
        else:
            nomatch.append((t, lns))

    def ctx(i):
        return f"s{i} §{section_of_line(i)}: " + re.sub(r"\s+", " ", LINES[i - 1]).strip()[:230]

    def rline06(tok):
        for j, ln in enumerate(read_lines(DOCS / DOCFILES["06"]), 1):
            if tok in ln:
                return f"06 s{j}: " + re.sub(r"\s+", " ", ln).strip()[:200]
        return ""

    out = [f"# Paket TE — terim, enum ve ad adayları\n\n09'daki {len(occ)} farklı kod adı (backtick içinde, PascalCase/camelCase/snake) betikle çıkarıldı. "
           f"{ok} adı 06'da ya da 02-08'de aynen geçiyor (liste dışı). Aşağıda: (1) 06'daki bir adla yalnız yazım farkı olanlar ({len(near)}), (2) 02-08'in hiçbirinde geçmeyenler ({len(nomatch)}). "
           "09'un kendi tanıttığı sınıf/yöntem adları (örn. işleyici, yardımcı) meşrudur; yalnız 06 §4/§5 ya da sözlükle (02 §1) çelişen, yanlış yazılan ya da başka kavram için kullanılan adları bulgu yap.\n"]
    out.append("## 1. 06'daki adla yazım farkı olanlar\n")
    for t, lns, hit, why in near:
        out.append(f"- `{t}` ({len(lns)} yer; {why}) ↔ 06'da `{hit}`\n  - {ctx(lns[0])}\n  - {rline06(hit)}")
    out.append("\n## 2. 02-08'de hiç geçmeyen adlar\n")
    budget = CAP - nbytes("\n".join(out)) - 2000
    lines2 = []
    for t, lns in nomatch:
        lines2.append(f"- `{t}` ({len(lns)} yer): {ctx(lns[0])}")
    cur = 0
    kept = []
    for x in lines2:
        if cur + nbytes(x) > budget:
            kept.append("[...bütçe doldu; kalan adlar paket dışı]")
            break
        kept.append(x)
        cur += nbytes(x)
    out += kept
    return "\n".join(out) + "\n", len(near), len(nomatch), len(occ)


# ---------------------------------------------------------------- G (guarantee sentences)
GKEY = re.compile(r"\b(asla|her zaman|hiçbir\w*|tek\b|yalnız\w*|kaybolmaz|olamaz|edilmez|yapılmaz|yazılmaz|okunmaz|aşılmaz|değişmez|silinmez|çıkmaz|girmez|anında|en geç|kesin\w*|mutlaka|hiç\b)", re.I)
SENT = re.compile(r"(?<=[.;!?])\s+(?=[A-ZÇĞİÖŞÜ0-9*`(\[])")


def g_inventory():
    items, fence = [], False
    for i in range(HEADS[0][0], N + 1):
        ln = LINES[i - 1]
        if ln.strip().startswith("```"):
            fence = not fence
            continue
        if fence or ln.startswith("*Kaynak") or not GKEY.search(ln):
            continue
        sents = SENT.split(ln)
        for si, s in enumerate(sents):
            if GKEY.search(s):
                prev = sents[si - 1][-160:] if si > 0 else ""
                nxt = sents[si + 1][:160] if si + 1 < len(sents) else ""
                items.append((i, s.strip()[:800], prev, nxt, [m.group(0).lower() for m in GKEY.finditer(s)]))
    return items


def build_g_packs():
    items = g_inventory()
    packs, cur, size, hdr = [], [], 0, ""
    for it in items:
        i, s, prev, nxt, kws = it
        blk = f"### G-{len(packs) + 1}.{len(cur) + 1} satır {i} (§{section_of_line(i)}) [{', '.join(sorted(set(kws)))}]\n{s}"
        if prev or nxt:
            blk += f"\n(önceki: …{prev} | sonraki: {nxt}…)"
        if size + nbytes(blk) > 80_000:
            packs.append(cur)
            cur, size = [], 0
        cur.append(blk)
        size += nbytes(blk) + 2
    packs.append(cur)
    outs = []
    for pi, p in enumerate(packs, 1):
        head = (f"# Paket G{pi} — güvence cümleleri ({len(p)} cümle; toplam envanter {len(items)})\n\n"
                "Her cümle 09'dan betikle çıkarıldı (asla, her zaman, hiçbir, tek, yalnız, kaybolmaz, olamaz, edilmez, anında, en geç ...). "
                "Her biri için sor: bu kesin cümleyi hangi mekanizma (bileşen, ayar, sıra, kısıt) sağlıyor ve eşzamanlı istek, çökme, yeniden deneme, saat kayması, "
                "kesinti ve dönüş, saldırgan, yedekten dönüş durumlarında tutuyor mu? Yalnız cümlenin kendi bağlamından (ve satır başındaki §'den) "
                "sonucuna varabildiklerini bulgu yap: cümlenin kendi içinde ya da önceki/sonraki cümleyle çelişen, mekanizması hiç anılmayan veya kapsamı fazla geniş olanlar. "
                "Dilsel kalıp olan ('tek bir', 'yalnız şu') ve zararsız olanları ele.\n\n")
        outs.append(head + "\n\n".join(p) + "\n")
    return outs, len(items)


# ---------------------------------------------------------------- DO (external facts)
def build_do_pack():
    a = next(i for i in range(1, N + 1) if LINES[i - 1].startswith("### Kütüphaneler"))
    b = next(i for i in range(a, N + 1) if LINES[i - 1].startswith("*Kaynak"))
    rows = []
    for i in range(a, b + 1):
        ln = LINES[i - 1]
        if ln.startswith("| `") or (ln.startswith("| ") and not ln.startswith("| Kütüphane") and not ln.startswith("|---")):
            cols = [c.strip() for c in ln.strip().strip("|").split("|")]
            rows.append((i, cols))
    out = [f"# Paket DO — Kütüphaneler tablosundaki dış olgular\n\nTablo satır {a}-{b}; {len(rows)} satır. Erişim tarihi tabloda 2026-10-29. "
           "Her satırdaki lisans, sürüm, CVE ve destek olgularını çıkar; 'bilgim tarihli', 'ikincil kaynak', 'doğrulanır' işaretli olanlar önce; en çok 15 olgu seç. "
           "Her olgu için WebSearch (mode standard) ile resmî kaynağı (NuGet/GitHub/proje sayfası) bul; kaynak URL'si ve okuma tarihi ile yaz. Doğrulayamadığını 'doğrulanamadı' diye bırak, varsayma.\n",
           "## Tablo giriş metni\n" + L(a + 2, a + 2), "## Satırlar"]
    for i, cols in rows:
        flags = [f for f in ("bilgim tarihli", "ikincil kaynak", "doğrulanır", "okunamadı", "bulunamadı") if f in " ".join(cols)]
        out.append(f"### satır {i}: {cols[0]}  [{', '.join(flags) or 'işaretsiz'}]\n- Görev: {cols[1] if len(cols) > 1 else ''}\n- Lisans: {cols[2] if len(cols) > 2 else ''}\n"
                   f"- Lisans koşulu: {cols[3] if len(cols) > 3 else ''}\n- Bakım kanıtı: {cols[4] if len(cols) > 4 else ''}\n- Karar: {cols[5] if len(cols) > 5 else ''}")
    return "\n\n".join(out) + "\n", len(rows)


# ---------------------------------------------------------------- main
def selftest():
    ok = True
    mod = list(LINES)
    broken = {}

    def inject(pat, repl, label):
        for j, ln in enumerate(mod):
            if re.search(pat, ln):
                mod[j] = re.sub(pat, repl, ln, count=1)
                broken[label] = j + 1
                return
        print("SELFTEST: pattern not found", label)

    inject(r"§10\.1 madde 4", "§10.1 madde 99", "madde")
    inject(r"K-1349", "K-99991", "K")
    inject(r"`05 §2\.3\.5`", "`05 §2.99.5`", "up")
    inject(r"UC-114", "UC-99992", "UC")
    inject(r"§8\.2\b", "§8.77", "local")
    res, _ = at_check(mod)
    got = {(i, why.split()[1]) for i, why, _ in res}
    for label, line in broken.items():
        hit = any(i == line for i, _, _ in res)
        print(f"SELFTEST bozuk {label} satır {line}: {'yakalandı' if hit else 'KAÇTI'}")
        ok = ok and hit
    base, _ = at_check(LINES)
    print(f"SELFTEST sağlam metinde ölü/şüpheli: {len(base)}")
    # known citation must be inside the K pack
    for sl_i, sl in enumerate(slices(), 1):
        for i in range(sl["a"], sl["b"] + 1):
            if LINES[i - 1].startswith("*Kaynak") and "K-1349" in LINES[i - 1]:
                txt, nk, nu = build_k_pack(sl_i, sl)
                hit = "### K-1349 " in txt and "### K-889 " in txt and "### Üst doküman 05 §" in txt
                print(f"SELFTEST K paketi {sl_i}: K-1349/K-889/üst atıf pakete girdi: {hit}")
                ok = ok and hit
    # ranges: K-1322…K-1328 must expand
    ks = kaynak_ks("K-1322…K-1328")
    print("SELFTEST aralık K-1322…K-1328:", sorted(ks) == list(range(1322, 1329)))
    ok = ok and sorted(ks) == list(range(1322, 1329))
    r = parse_refs("`05 §3.3.11`–§3.3.13, §8.4 · §1, §2")
    print("SELFTEST parse:", r)
    ok = ok and ("05", "3.3.12", None) in r and ("local", "1", None) in r
    print("SELFTEST", "GEÇTİ" if ok else "KALDI")
    return ok


def main():
    if SELFTEST:
        sys.exit(0 if selftest() else 1)
    OUT.mkdir(parents=True, exist_ok=True)
    for old in OUT.glob("paket_*.md"):
        old.unlink()
    table = []
    for i, sl in enumerate(slices(), 1):
        txt, nk, nu = build_k_pack(i, sl)
        (OUT / f"paket_K{i}.md").write_text(txt, encoding="utf-8")
        table.append((f"K{i}", nbytes(txt), f"{', '.join(sl['labels'])} | satır {sl['a']}-{sl['b']} | K {nk}, üst {nu}"))
    txt, nn, nm, no = build_te_pack()
    (OUT / "paket_TE_1.md").write_text(txt, encoding="utf-8")
    table.append(("TE", nbytes(txt), f"{no} ad; yazım farkı {nn}; hiçbirinde yok {nm}"))
    txt, nd, counts = build_at_pack()
    (OUT / "paket_AT_1.md").write_text(txt, encoding="utf-8")
    table.append(("AT", nbytes(txt), f"ölü/şüpheli {nd}; {counts}"))
    outs, tot = build_g_packs()
    for gi, t in enumerate(outs, 1):
        (OUT / f"paket_G{gi}.md").write_text(t, encoding="utf-8")
        table.append((f"G{gi}", nbytes(t), f"envanter toplam {tot}"))
    txt, nr = build_do_pack()
    (OUT / "paket_DO_1.md").write_text(txt, encoding="utf-8")
    table.append(("DO", nbytes(txt), f"{nr} satır"))
    for name, sz, note in table:
        flag = "  !! >90KB" if sz > 90_000 else ""
        print(f"{name}\t{sz / 1024:.1f} KB{flag}\t{note}")


main()
