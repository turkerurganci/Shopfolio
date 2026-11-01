"""Asama 10 -- `12` deep review, A10-KD-deep-2: dogrulanmamis 127 bulgunun yeniden gruplanmasi + dogrulama paketleri.

Kipler
  plan            : python -I scripts/doc-scans/12_dr_grupla2.py plan [--girdi <a10-kd-deep>] [--cikti <a10-kd-deep-2>]
                    Girdi : <girdi>/dogrulanmadi.md (127 no)
                    Cikti : <cikti>/plan.json, <cikti>/plan.md
                    Kural : Bolum = `12` v0.20'nin `### ` alt bolumu. Orta 2-4'lu, Dusuk 5-8'li, ayni bolumden.
                            Artik (kuralin altinda kalan) once ayni `## ` ana bolumundeki en yakin gruba katilir (yeniden bolunur);
                            ana bolumde o seviyeden grup yoksa cerceve havuzuna (yer'i belirsiz / cerceve bolumleri) gider;
                            orada da olmazsa gerekceli kucuk grup. Adim <= 20 dogrulayici (adimin TOPLAMI).
                            Sira  : para/durum/guvenlik riskli bolumler once (RISK), bolum icinde Orta sonra Dusuk.
  --paket <adim>  : plan.json'daki adimin paketlerini O ANKI `12`'den uretir: <cikti>/<adim>/dogrulama_<k>.md
                    (talimat + bulgunun tam metni + ilgili 12 satirlarinin guncel hali + kaynak satirlari). Sablon <cikti>/talimat_dogrulayici.md.

Sinama (assert): 127 no tam bir kez, grup boyutu kurala uygun (istisna gerekceli), adim <= 20.
"""
import json
import math
import re
import subprocess
import sys
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

ROOT = Path(r"C:\projects\Shopfolio")
DOC = ROOT / "Docs" / "12_VALIDATION_PROTOCOL.md"
HS = Path(r"C:\Users\teknodeveloper\.claude\helper-state\shopfolio")
V020 = "c120777"  # bulgulardaki `12:<n>` numaralari bu commit'teki (v0.20) `12`'ye aittir
MAX_STEP = 20
SIZE = {"Orta": (2, 4), "Dusuk": (5, 8)}
RISK = ["12.3", "12.4", "12.6", "12.7", "12.9", "12.5", "4.4", "4.6", "4.10", "4.5", "12.8", "12.1", "4.12", "4.16", "4.20"]
POOL = "cerceve"  # yeri belirsiz / cerceve (1-3, 5-11) bolumleri havuzu


def sh(*a):
    return subprocess.run(a, cwd=ROOT, capture_output=True, check=True).stdout.decode("utf-8")


def v020_lines():
    return sh("git", "show", "%s:Docs/12_VALIDATION_PROTOCOL.md" % V020).replace("\r\n", "\n").split("\n")


def section_of(lines, n):
    for i in range(min(n, len(lines)) - 1, -1, -1):
        m = re.match(r"^###? (\d+(?:\.\d+)?)\.?\s", lines[i])
        if m:
            return m.group(1)
    return "0"


def sec_key(s):
    return [int(x) for x in s.split(".")]


def parse_findings(path):
    blocks, cur = {}, None
    for l in path.read_text(encoding="utf-8").split("\n"):
        m = re.match(r"^## #(\d+) \| seviye: (\w+)", l)
        if m:
            cur = int(m.group(1))
            blocks[cur] = dict(sev=m.group(2), lines=[l])
        elif cur is not None:
            blocks[cur]["lines"].append(l)
    for b in blocks.values():
        b["text"] = "\n".join(b["lines"]).rstrip()
        b["yer"] = next((x for x in b["lines"] if x.startswith("Yer:")), "")
    return blocks


def parent_of(sec):
    p = sec.split(".")[0]
    return p if p in ("4", "12") else POOL


def chunk(items, lo, hi):
    """items sirali liste -> (gruplar, artik). Her grup lo..hi."""
    n = len(items)
    if n < lo:
        return [], list(items)
    for g in range(math.ceil(n / hi), n // lo + 1):
        if lo * g <= n <= hi * g:
            base, extra = divmod(n, g)
            out, i = [], 0
            for k in range(g):
                sz = base + (1 if k < extra else 0)
                out.append(items[i:i + sz])
                i += sz
            return out, []
    g = n // hi
    return [items[i * hi:(i + 1) * hi] for i in range(g)], items[g * hi:]


def order_key(it):
    return (sec_key(it["sec"]), it["no"])


def build(girdi):
    L = v020_lines()
    fnd = parse_findings(girdi / "dogrulanmadi.md")
    items = []
    for no, b in sorted(fnd.items()):
        m = re.search(r"12:(\d+)", b["yer"])
        if m:
            sec = section_of(L, int(m.group(1)))
            kaynak = "satir"
        else:
            m2 = re.search(r"\u00a7(\d+(?:\.\d+)?)", b["yer"].replace("§", "\u00a7"))
            sec = m2.group(1) if m2 else "0"
            kaynak = "bolum-adi"
        if sec == "0":
            sec = "0"
        items.append(dict(id="#%d" % no, no=no, sev=b["sev"], sec=sec, par=parent_of(sec), kaynak=kaynak))
    units = []  # dict(sev, sec, par, ids, not)
    for sev in ("Orta", "Dusuk"):
        lo, hi = SIZE[sev]
        S = [i for i in items if i["sev"] == sev]
        groups = []  # list of list of items
        left_by_par = {}
        bysec = {}
        for it in S:
            bysec.setdefault(it["sec"], []).append(it)
        for sec in sorted(bysec, key=sec_key):
            its = sorted(bysec[sec], key=order_key)
            gs, art = chunk(its, lo, hi)
            groups += gs
            if art:
                left_by_par.setdefault(art[0]["par"], []).extend(art)
        # ayni ana bolumde artik: once artik kendi arasinda grupla, olmazsa en yakin gruba katil ve yeniden bol
        residual = []
        for par, art in left_by_par.items():
            art.sort(key=order_key)
            gs, art2 = chunk(art, lo, hi)
            groups += gs
            if art2:
                same = [g for g in groups if g[0]["par"] == par]
                if same:
                    key = sec_key(art2[0]["sec"])
                    near = min(same, key=lambda g: min(abs(sec_key(x["sec"])[-1] - key[-1]) for x in g))
                    groups.remove(near)
                    merged = sorted(near + art2, key=order_key)
                    gs2, art3 = chunk(merged, lo, hi)
                    groups += gs2
                    residual += art3
                else:
                    residual += art2
        # cerceve havuzu / ana bolumu olmayan artiklar
        if residual:
            residual.sort(key=lambda x: (x["par"], order_key(x)))
            gs, art = chunk(residual, lo, hi)
            groups += gs
            while art:
                pool = [g for g in groups if g[0]["par"] == art[0]["par"]]
                if not pool:
                    if sev == "Dusuk":
                        groups.append(art)  # gerekceli kucuk grup (ayni bolumde baska Dusuk yok)
                        break
                    pool = groups
                near = pool[-1]
                groups.remove(near)
                merged = sorted(near + art, key=order_key)
                gs2, art = chunk(merged, lo, hi)
                groups += gs2
        for g in groups:
            secs = sorted({x["sec"] for x in g}, key=sec_key)
            pars = {x["par"] for x in g}
            units.append(dict(sev=sev, sec="+".join(secs), par=sorted(pars)[0], coklu=len(pars) > 1, ids=[x["id"] for x in g]))

    def rank(u):
        first = u["sec"].split("+")[0]
        r = RISK.index(first) if first in RISK else len(RISK)
        return (r, sec_key(first), 0 if u["sev"] == "Orta" else 1, int(u["ids"][0][1:]))

    units.sort(key=rank)
    steps = []
    for u in units:
        if not steps or len(steps[-1]["gruplar"]) >= MAX_STEP:
            steps.append(dict(adim="2" + chr(ord("a") + len(steps)), gruplar=[]))
        u["k"] = len(steps[-1]["gruplar"]) + 1
        steps[-1]["gruplar"].append(u)
    return items, steps


def sinama(items, steps):
    ids = [i for s in steps for g in s["gruplar"] for i in g["ids"]]
    assert len(ids) == len(set(ids)), "bir madde iki grupta"
    assert set(ids) == {x["id"] for x in items}, "eksik/fazla madde"
    assert len(items) == 127, "madde sayisi %d" % len(items)
    by = {x["id"]: x for x in items}
    notlar = []
    for s in steps:
        assert len(s["gruplar"]) <= MAX_STEP, "adim %s > %d" % (s["adim"], MAX_STEP)
        for g in s["gruplar"]:
            lo, hi = SIZE[g["sev"]]
            n = len(g["ids"])
            assert n <= hi, "grup %s/%d boyutu %d > %d" % (s["adim"], g["k"], n, hi)
            assert all(by[i]["sev"] == g["sev"] for i in g["ids"]), "grup karisik seviye"
            if n < lo:
                same = [x for x in items if x["sev"] == g["sev"] and x["par"] == g["par"]]
                assert len(same) == n or g["coklu"], "kucuk grup gerekcesiz: %s/%d" % (s["adim"], g["k"])
                notlar.append("%s/%d: %s %d madde < %d (gerekce: ana bolum %s'te o seviyeden baska madde yok)" % (s["adim"], g["k"], g["sev"], n, lo, g["par"]))
            if g["coklu"]:
                notlar.append("%s/%d: birden cok ana bolum (%s)" % (s["adim"], g["k"], g["sec"]))
            elif g["par"] == POOL and len({x.split(".")[0] for x in g["sec"].split("+")}) > 1:
                notlar.append("%s/%d: cerceve havuzu, ana bolumler karisik (%s); yeri satir numarasiz ya da tek basina kalan cerceve bulgulari" % (s["adim"], g["k"], g["sec"]))
    return notlar


def write_plan(items, steps, notlar, cikti):
    cikti.mkdir(parents=True, exist_ok=True)
    (cikti / "plan.json").write_text(json.dumps(dict(items=items, steps=steps), ensure_ascii=False, indent=1), encoding="utf-8", newline="\n")
    o = ["# A10-KD-deep-2 plani (127 bulgu: Orta %d, Dusuk %d; adim basina <= %d dogrulayici)\n" % (
        sum(1 for x in items if x["sev"] == "Orta"), sum(1 for x in items if x["sev"] == "Dusuk"), MAX_STEP),
        "Bolum = `12` v0.20 alt bolumu (yeri satir numarasi vermeyen bulgularda Yer'deki ilk bolum adi). Orta 2-4, Dusuk 5-8.\n"]
    for s in steps:
        o.append("\n## Adim %s -- %d dogrulayici, %d madde\n" % (s["adim"], len(s["gruplar"]), sum(len(g["ids"]) for g in s["gruplar"])))
        o.append("| k | seviye | bolum | maddeler |\n|---|---|---|---|")
        for g in s["gruplar"]:
            o.append("| %d | %s | %s | %s |" % (g["k"], g["sev"], g["sec"], ", ".join(g["ids"])))
    if notlar:
        o.append("\n## Istisnalar ve notlar\n")
        o += ["- " + n for n in notlar]
    (cikti / "plan.md").write_text("\n".join(o) + "\n", encoding="utf-8", newline="\n")


def src_line(nn, n, cache={}):
    if nn not in cache:
        fs = sorted((ROOT / "Docs").glob(nn + "_*.md"))
        cache[nn] = fs[0].read_text(encoding="utf-8").replace("\r\n", "\n").split("\n") if fs else []
    L = cache[nn]
    return L[n - 1] if 0 < n <= len(L) else None


def paket(adim, cikti, girdi):
    plan = json.loads((cikti / "plan.json").read_text(encoding="utf-8"))
    step = next(s for s in plan["steps"] if s["adim"] == adim)
    fnd = parse_findings(girdi / "dogrulanmadi.md")
    old = v020_lines()
    cur = DOC.read_text(encoding="utf-8").replace("\r\n", "\n").split("\n")
    pos = {}
    for i, l in enumerate(cur):
        pos.setdefault(l, []).append(i + 1)
    tmpl = (cikti / "talimat_dogrulayici.md").read_text(encoding="utf-8")
    od = cikti / adim
    od.mkdir(parents=True, exist_ok=True)

    def guncel(n):
        if not 0 < n <= len(old):
            return "12:%d v0.20'de yok" % n
        l = old[n - 1]
        p = pos.get(l, [])
        if len(p) == 1:
            return "12:%d (v0.20) -> guncel 12:%d (birebir ayni satir)| %s" % (n, p[0], l[:600])
        if not p:
            return "12:%d (v0.20) -> guncelde BIREBIR YOK (degismis olabilir; `grep -n` ile alintidan bul)| %s" % (n, l[:600])
        return "12:%d (v0.20) -> guncelde %d yerde birebir (%s)| %s" % (n, len(p), ",".join(map(str, p[:6])), l[:300])

    for g in step["gruplar"]:
        body = (tmpl.replace("{ADIM}", adim).replace("{K}", str(g["k"])).replace("{IDS}", ", ".join(g["ids"]))
                .replace("{N}", str(len(g["ids"]))).replace("{HUKUM}", (od / ("hukum_%d.md" % g["k"])).as_posix()))
        for i in g["ids"]:
            b = fnd[int(i[1:])]
            text = b["text"]
            body += "\n" + text + "\n"
            refs = []
            for m in re.finditer(r"(?<![\w.])12:(\d+)(?:\.\.(\d+))?", text):
                a = int(m.group(1))
                b2 = int(m.group(2)) if m.group(2) else a
                for n in range(a, min(b2, a + 6) + 1):
                    if n not in refs:
                        refs.append(n)
            body += "\nGuncel 12 satirlari (%s):\n" % i
            for n in refs[:14]:
                body += "- " + guncel(n) + "\n"
            sr = []
            for m in re.finditer(r"(?<![\w:.])(0[2-9]|1[01]):(\d+)\b", text):
                k = (m.group(1), int(m.group(2)))
                if k not in sr:
                    sr.append(k)
            if sr:
                body += "Kaynak satirlari (%s):\n" % i
                for nn, n in sr[:12]:
                    l = src_line(nn, n)
                    body += "- %s:%d| %s\n" % (nn, n, (l or "(satir yok)")[:500])
        (od / ("dogrulama_%d.md" % g["k"])).write_text(body, encoding="utf-8", newline="\n")
    print("adim %s: %d paket -> %s" % (adim, len(step["gruplar"]), od))


def main():
    a = sys.argv[1:]
    girdi = HS / "a10-kd-deep"
    cikti = HS / "a10-kd-deep-2"
    if "--girdi" in a:
        i = a.index("--girdi"); girdi = Path(a[i + 1]); del a[i:i + 2]
    if "--cikti" in a:
        i = a.index("--cikti"); cikti = Path(a[i + 1]); del a[i:i + 2]
    if "--paket" in a:
        paket(a[a.index("--paket") + 1], cikti, girdi)
        return
    items, steps = build(girdi)
    notlar = sinama(items, steps)
    write_plan(items, steps, notlar, cikti)
    print("madde: %d; grup: %d; adim: %d -> %s" % (len(items), sum(len(s["gruplar"]) for s in steps), len(steps), cikti))
    for s in steps:
        print(" adim %s: %d dogrulayici" % (s["adim"], len(s["gruplar"])))
    for n in notlar:
        print(" not:", n)


if __name__ == "__main__":
    main()
