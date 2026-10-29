"""Builds deep-review lens packages for Docs/09_CODING_GUIDELINES.md (Stage 8 quality loop, deep review D-a).

Usage: python -I 09_dr_paketle.py [output_dir]   builds dpaket_L12, dpaket_L3_<i>, dpaket_L4..L8 (each <= 90 KB).
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



# ================================================================ deep review packages (Stage 8, D-a)
import subprocess

DR_CAP = 88_000


def sec_range(key):
    a, end, _ = KEYS[key]
    return (a, end)


def merge_ranges(rs):
    rs = sorted(rs)
    out = []
    for a, b in rs:
        if out and a <= out[-1][1] + 1:
            out[-1] = (out[-1][0], max(out[-1][1], b))
        else:
            out.append((a, b))
    return out


def body_of(ranges):
    return "\n\n".join(f"[satır {a}-{b}]\n{numbered(a, b)}" for a, b in merge_ranges(ranges))


def ks_of(ranges):
    ks = set()
    for a, b in merge_ranges(ranges):
        for i in range(a, b + 1):
            if LINES[i - 1].startswith("*Kaynak"):
                ks |= kaynak_ks(LINES[i - 1])
    return ks


def up_refs_of(ranges):
    seen, out = set(), []
    for a, b in merge_ranges(ranges):
        for d, n in collect_up_refs(a, b):
            if (d, n) not in seen:
                seen.add((d, n))
                out.append((d, n))
    return out


def build_dr_pack(pid, title, question, ranges, use_k=True, use_up=True, extra=""):
    body = body_of(ranges)
    ks = sorted(k for k in ks_of(ranges) if k in REG) if use_k else []
    parts_up, unresolved, used = [], [], 0
    base = nbytes(body) + nbytes(extra) + 4_000
    up_budget = min(UP_CAP, max(0, DR_CAP - base - 40_000)) if use_up else 0
    if use_up:
        for d, n in up_refs_of(ranges):
            ex = upstream_excerpt(d, n)
            if ex is None:
                continue
            ea, eb, txt = ex
            blk = f"### Üst doküman {d} §{n} (satır {ea}-{eb})\n{txt}"
            if used + nbytes(blk) > up_budget:
                continue
            used += nbytes(blk)
            parts_up.append(blk)
    room = DR_CAP - base - used
    per = 0
    if ks:
        per = max(100, min(2500, room // len(ks) - 120))
    ktxt = []
    for k in ks:
        konu, karar, _ = REG[k]
        kt = karar if len(karar) <= per else karar[:per] + " [...kesildi]"
        ktxt.append(f"### K-{k} — {konu[:140]}\n{kt}")
    txt = (f"# Paket {pid} — {title}\n\nYalnız bu dosyayı oku. A: ilgili 09 bölümleri (satır numaralı). "
           f"B: bu bölümlerin Kaynak satırlarının andığı karar kaydı satırları (karar hücresi {per} karakterde kesik olabilir). "
           "C: atıf yapılan üst doküman bölümleri (kesik). Bulguyu bulduğun anda dosyaya yaz.\n\n"
           f"**Merceğin sorusu:** {question}\n\n## A. 09 metni\n\n{body}\n\n"
           + (extra + "\n\n" if extra else "")
           + f"## B. Karar kaydı satırları ({len(ks)} adet)\n\n" + "\n\n".join(ktxt)
           + f"\n\n## C. Üst doküman bölümleri ({len(parts_up)} adet)\n\n" + "\n\n".join(parts_up) + "\n")
    while nbytes(txt) > DR_CAP and per > 60 and ks:
        per = max(60, int(per * 0.85))
        ktxt = [f"### K-{k} — {REG[k][0][:100]}\n" + (REG[k][1] if len(REG[k][1]) <= per else REG[k][1][:per] + " [...kesildi]") for k in ks]
        head = txt.split("## B. Karar kaydı satırları")[0]
        tail = "## C. Üst doküman" + txt.split("## C. Üst doküman", 1)[1]
        txt = head + f"## B. Karar kaydı satırları ({len(ks)} adet; hücre {per} karakterde kesik)\n\n" + "\n\n".join(ktxt) + "\n\n" + tail
    while nbytes(txt) > DR_CAP and parts_up:
        parts_up.pop()
        txt = txt.rsplit("### Üst doküman", 1)[0] + "\n"
    return txt, len(ks), len(parts_up)


def git_out(args):
    return subprocess.run(["git", "-C", str(ROOT)] + args, capture_output=True, text=True, encoding="utf-8").stdout


def diff_pack():
    base, head = "5eacc19", "c2e44e6"
    hunks = []
    for line in git_out(["diff", "-U0", base, head, "--", "Docs/09_CODING_GUIDELINES.md"]).split("\n"):
        m = re.match(r"@@ -\d+(?:,\d+)? \+(\d+)(?:,(\d+))? @@", line)
        if m:
            s, c = int(m.group(1)), int(m.group(2) or 1)
            hunks.append((max(1, s), min(N, s + max(c, 1) - 1)))
    rs = merge_ranges(hunks)
    extra = ""
    d05 = git_out(["diff", "-U3", base, head, "--", "Docs/05_TECHNICAL_ARCHITECTURE.md"])
    if d05.strip():
        extra = "## A2. 05 Teknik Mimari değişikliği (audit PR'ı; git diff)\n\n```diff\n" + d05[:20_000] + "\n```"
    regk = []
    for k in range(1360, 1372):
        if k in REG:
            konu, karar, _ = REG[k]
            regk.append(f"### K-{k} — {konu[:160]}\n{karar[:1500]}")
    txt = ("# Paket L12 — audit düzeltmelerinin (K-1360…K-1371) doğurduğu yeni tutarsızlıklar\n\nYalnız bu dosyayı oku. "
           "A: audit PR'ında (#218) değişen 09 yerleri (bağlamsız, satır numaralı). A2: 05'in değişikliği. B: K-1360…K-1371 kararları. "
           "Soru: bu düzeltmeler başka yerleri eskitti mi, yeni çelişki ya da erişilemez ölçüt doğurdu mu? Bulguyu bulduğun anda dosyaya yaz.\n\n"
           f"## A. 09 değişen yerler ({len(rs)} aralık)\n\n{body_of(rs)}\n\n" + (extra + "\n\n" if extra else "")
           + "## B. Karar kaydı\n\n" + "\n\n".join(regk) + "\n")
    return txt, len(rs)


def depth_slices():
    us = [u for u in units() if u[0] != "baş"]
    us = [u for u in us if u[1] >= KEYS["3"][0]]
    out, cur = [], None
    for lab, a, b in us:
        sz = nbytes(L(a, b)) + 6 * (b - a + 1)
        if cur and cur["size"] + sz > 80_000:
            out.append(cur)
            cur = None
        if cur is None:
            cur = {"labels": [], "ranges": [], "size": 0}
        cur["labels"].append(lab)
        cur["ranges"].append((a, b))
        cur["size"] += sz
    out.append(cur)
    return out


def sr(*keys):
    return [sec_range(k) for k in keys]


def dr_main():
    OUT.mkdir(parents=True, exist_ok=True)
    for old in OUT.glob("dpaket_*.md"):
        old.unlink()
    table = []
    txt, nr = diff_pack()
    (OUT / "dpaket_L12.md").write_text(txt, encoding="utf-8")
    table.append(("L12", nbytes(txt), f"{nr} aralık"))
    qd = ("Kodlama ajanı bu kuralla TEK anlamlı kod yazabilir mi, yoksa 'ama nasıl?' der mi? Belirsiz ifade, eksik parametre/ad/sıra, "
          "çatı API adı yanlış ya da derlenmeyecek kod örneği, iki okumaya açık kural. Uygulamayı sonraki task'a bırakılabilir ise Orta yaz.")
    for i, sl in enumerate(depth_slices(), 1):
        txt, nk, nu = build_dr_pack(f"L3-{i}", "teknik derinlik: " + ", ".join(sl["labels"]), qd, sl["ranges"], use_k=False, use_up=False)
        (OUT / f"dpaket_L3_{i}.md").write_text(txt, encoding="utf-8")
        table.append((f"L3-{i}", nbytes(txt), ", ".join(sl["labels"])))
    specs = [
        ("L4a", "güvenlik (OWASP Top 10) — §15", sr("15"),
         "OWASP Top 10 (A01 erişim, A02 kriptografi, A03 enjeksiyon, A04 tasarım, A05 yapılandırma, A06 bileşen, A07 kimlik/oturum, A08 bütünlük, A09 kayıt, A10 SSRF) ile: kimlik, oturum, çerez, CSP, dosya yükleme, sır dağıtımı, hız sınırı, imza doğrulama kurallarında açık var mı?"),
        ("L4b", "güvenlik (OWASP Top 10) — §11.3, §11.4, §16, §10.8", sr("11.3", "11.4", "16", "10.8"),
         "OWASP Top 10 ile: kimlik doğrulama (Google belirteci), e-posta/belge üretimi, istemci tarafı (CSP, çerez, betiksiz yol, yükleme betiği), veritabanı kullanıcı yetkileri; sır dağıtımı ve en az yetki."),
        ("L5", "hata modu", sr("8", "13", "11", "10.2"),
         "Her bileşen/işlem çökerse ya da 10 dk erişilemezse ne olur? Kilit, yeniden deneme, sonucu belirsiz işlem, SPOF, geri dönüş, veri kaybı, çift işlem, zaman aşımı sırası."),
        ("L6", "veri akışı", sr("18", "15.5", "10.10", "11", "9.2"),
         "Kişisel verinin kayda, işleme, dış servise ve dışa aktarmaya giden yolu: nerede oluşur, doğrulanır, maskelenir, silinir/imha edilir; kopukluk, sızıntı yolu, snapshot-referans belirsizliği."),
        ("L7", "ölçeklenebilirlik", sr("20", "10.4", "10.5", "13.1"),
         "Yük artarsa kural dayanır mı? Önbellek, sayfalama, arama, kuyruk; büyüme stratejisi; 'büyüyünce X'e geçilir' ifadeleri gerçekçi mi; MVP kararı ileriyi engelliyor mu?"),
        ("L8", "bağımlılık riski", sr("3.2", "3.3", "3"),
         "Kütüphaneler tablosu, koşullu lisanslı yedek aday, sürüm sabitleme: lisans/kapanma/CVE/bakım riski, soyutlama katmanı, alternatif, sürüm sabitleme kuralının uygulanabilirliği ve tablo-gövde tutarlılığı."),
    ]
    for pid, title, rngs, q in specs:
        if pid == "L8":
            rngs = sr("3.2", "3.3") + [(KEYS["3"][0], KEYS["3"][0] + 1)]
            # library table block
            lines = [i for i, l in enumerate(LINES, 1) if l.strip() == "### Kütüphaneler"]
            if lines:
                a = lines[0]
                end = next(i for i in range(a + 1, N) if LINES[i - 1].startswith("## 4."))
                rngs = sr("3.2", "3.3") + [(a, end - 1)]
        txt, nk, nu = build_dr_pack(pid, title, q, rngs)
        (OUT / f"dpaket_{pid}.md").write_text(txt, encoding="utf-8")
        table.append((pid, nbytes(txt), f"K {nk}, üst {nu}"))
    for name, sz, note in table:
        print(f"{name}\t{sz / 1024:.1f} KB{'  !! >90KB' if sz > 90_000 else ''}\t{note}")


dr_main()
