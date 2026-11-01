"""Asama 10 -- `12` audit'i, KD-audit-2: dogrulanmamis bulgularin (82) ve YAN notlarinin (23) dogrulama plani + paketleri.

Kipler
  plan   : python -I scripts/doc-scans/12_audit_grupla2.py plan [--dizin <a10-kd-audit>] [--cikti <a10-kd-audit-2>]
           Girdi  : <dizin>/dogrulanmadi.md (82 no), <dizin>/hukum_*.md icindeki `YAN:` satirlari (yok olmayanlar -> Y<n>), <dizin>/birlesik.md (ana bulgu yeri)
           Cikti  : <cikti>/plan.json, <cikti>/plan.md
           Kural  : Bolum = v0.15 `12`'nin `### ` alt bolumu (yoksa `## `). Yuksek tek; Orta 2-4'lu; Dusuk 5-8'li (bolumde yeterince bulgu yoksa
                    kalan tek grup, asgariden kucuk olabilir). Adim basina en cok 20 dogrulayici (INSTRUCTIONS s9 (1): adimin TOPLAMI).
           Sira   : Yuksek once (#1, #2, #8, #9, #10, sonra kalan Yuksek), sonra para/durum riskli bolumler, sonra kalan bolumler; bolum icinde Orta sonra Dusuk.
  --paket <adim> : plan.json'daki adimin paketlerini O ANKI `12`'den uretir: <cikti>/<adim>/dogrulama_<k>.md
           (talimat + bulgunun tam metni + ilgili 12 satirlarinin guncel hali + kaynak satirlari). Talimat sablonu <cikti>/talimat_dogrulayici.md.

Sinama (assert): her no/Y tam bir kez, grup boyutu kurala uygun, adim <= 20.
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
V015 = "6ac4c72"  # bulgulardaki `12:<n>` numaralari bu surume aittir
MAX_STEP = 20
SIZE = {"Yuksek": (1, 1), "Orta": (2, 4), "Dusuk": (5, 8)}
FIRST = [1, 2, 8, 9, 10]
# para / durum riskli bolumler (oncelik sirasi)
RISK = ["12.3", "12.4", "12.6", "12.7", "12.9", "12.5", "4.4", "4.6", "4.10", "4.5", "12.8", "12.1", "4.12", "4.16", "4.20"]


def sh(*a):
    return subprocess.run(a, cwd=ROOT, capture_output=True, check=True).stdout.decode("utf-8")


def v015_lines():
    return sh("git", "show", "%s:Docs/12_VALIDATION_PROTOCOL.md" % V015).replace("\r\n", "\n").split("\n")


def section_of(lines, n):
    """n (1 tabanli, v0.15) satirinin bolum kodu: `### 12.6 ...` -> '12.6', `## 5. ...` -> '5'."""
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
        y = next((x for x in b["lines"] if x.startswith("Yer:")), "")
        b["yer"] = y
    return blocks


def parse_yan(dizin):
    out, n = [], 0
    files = sorted(dizin.glob("hukum_*.md"), key=lambda p: int(re.search(r"(\d+)", p.stem).group(1)))
    for f in files:
        cur = None
        for l in f.read_text(encoding="utf-8").split("\n"):
            m = re.match(r"^## #(\d+) \|", l)
            if m:
                cur = int(m.group(1))
            elif l.startswith("YAN:"):
                body = l[4:].strip()
                if body.startswith("yok"):
                    continue
                n += 1
                out.append(dict(id="Y%d" % n, parent=cur, file=f.name, text=body))
    return out


def sev_of_yan(t):
    return "Dusuk"  # YAN notlari varsayilan Dusuk; ust seviye hukmu dogrulayicinindir (yukseltilirse yeniden.md)


def build_plan(dizin, cikti):
    L = v015_lines()
    fnd = parse_findings(dizin / "dogrulanmadi.md")
    birl = parse_findings(dizin / "birlesik.md")
    items = []
    for no, b in fnd.items():
        m = re.search(r"12:(\d+)", b["yer"])
        sec = section_of(L, int(m.group(1))) if m else "0"
        items.append(dict(id="#%d" % no, no=no, sev=b["sev"], sec=sec))
    for y in parse_yan(dizin):
        m = re.search(r"12:(\d+)", y["text"])
        if m:
            sec = section_of(L, int(m.group(1)))
        else:
            pm = re.search(r"12:(\d+)", birl.get(y["parent"], {}).get("yer", ""))
            sec = section_of(L, int(pm.group(1))) if pm else "0"
        items.append(dict(id=y["id"], no=None, sev=sev_of_yan(y["text"]), sec=sec, parent=y["parent"], file=y["file"]))
    # birimler
    units = []
    from itertools import groupby
    for sev in ("Yuksek", "Orta", "Dusuk"):
        lo, hi = SIZE[sev]
        bysec = {}
        for it in items:
            if it["sev"] == sev:
                bysec.setdefault(it["sec"], []).append(it)
        for sec, its in bysec.items():
            its.sort(key=lambda x: (x["no"] is None, x["no"] or 0, x["id"]))
            g = max(1, math.ceil(len(its) / hi))
            base, extra = divmod(len(its), g)
            i = 0
            for k in range(g):
                sz = base + (1 if k < extra else 0)
                units.append(dict(sev=sev, sec=sec, ids=[x["id"] for x in its[i:i + sz]]))
                i += sz

    def rank(u):
        if u["sev"] == "Yuksek":
            nums = [int(x[1:]) for x in u["ids"] if x[0] == "#"]
            f = FIRST.index(nums[0]) if nums and nums[0] in FIRST else len(FIRST)
            return (0, f, nums[0] if nums else 0)
        r = RISK.index(u["sec"]) if u["sec"] in RISK else len(RISK)
        return (1, r, sec_key(u["sec"]), 0 if u["sev"] == "Orta" else 1)

    units.sort(key=rank)
    steps = []
    adim = 0
    for u in units:
        if not steps or len(steps[-1]["gruplar"]) >= MAX_STEP:
            steps.append(dict(adim="2" + chr(ord("a") + len(steps)), gruplar=[]))
        s = steps[-1]
        u["k"] = len(s["gruplar"]) + 1
        s["gruplar"].append(u)
    return items, steps


def sinama(items, steps):
    ids = [i for s in steps for g in s["gruplar"] for i in g["ids"]]
    assert len(ids) == len(set(ids)), "bir madde iki grupta"
    assert set(ids) == {x["id"] for x in items}, "eksik/fazla madde"
    nn = sum(1 for x in items if x["id"][0] == "#")
    yy = sum(1 for x in items if x["id"][0] == "Y")
    assert nn == 82, "no sayisi %d" % nn
    assert yy == 23, "YAN sayisi %d" % yy
    by = {x["id"]: x for x in items}
    for s in steps:
        assert len(s["gruplar"]) <= MAX_STEP, "adim %s > %d" % (s["adim"], MAX_STEP)
        for g in s["gruplar"]:
            lo, hi = SIZE[g["sev"]]
            n = len(g["ids"])
            assert n <= hi, "grup %s/%d boyutu %d > %d" % (s["adim"], g["k"], n, hi)
            assert len({by[i]["sec"] for i in g["ids"]}) == 1, "grup birden cok bolumde"
            assert all(by[i]["sev"] == g["sev"] for i in g["ids"]), "grup karisik seviye"
            if n < lo:  # yalniz bolumde o seviyeden yeterince madde yoksa
                same = [x for x in items if x["sev"] == g["sev"] and x["sec"] == g["sec"]]
                assert len(same) == n, "kucuk grup bolumun tamami degil: %s/%d" % (s["adim"], g["k"])
    return nn, yy


def write_plan(items, steps, cikti):
    cikti.mkdir(parents=True, exist_ok=True)
    (cikti / "plan.json").write_text(json.dumps(dict(items=items, steps=steps), ensure_ascii=False, indent=1), encoding="utf-8", newline="\n")
    o = ["# KD-audit-2 plani (82 bulgu + 23 YAN notu)\n",
         "Bolum = `12` v0.15 alt bolumu. Dogrulayici sayisi adim basina <= %d.\n" % MAX_STEP]
    for s in steps:
        o.append("\n## Adim %s -- %d dogrulayici, %d madde\n" % (s["adim"], len(s["gruplar"]), sum(len(g["ids"]) for g in s["gruplar"])))
        o.append("| k | seviye | bolum | maddeler |\n|---|---|---|---|")
        for g in s["gruplar"]:
            o.append("| %d | %s | %s | %s |" % (g["k"], g["sev"], g["sec"], ", ".join(g["ids"])))
    (cikti / "plan.md").write_text("\n".join(o) + "\n", encoding="utf-8", newline="\n")


SRC = {"02": "02_", "03": "03_", "04": "04_", "05": "05_", "06": "06_", "07": "07_", "08": "08_", "09": "09_", "10": "10_", "11": "11_"}


def src_line(nn, n, cache={}):
    if nn not in cache:
        fs = sorted((ROOT / "Docs").glob(nn + "_*.md"))
        cache[nn] = fs[0].read_text(encoding="utf-8").replace("\r\n", "\n").split("\n") if fs else []
    L = cache[nn]
    return L[n - 1] if 0 < n <= len(L) else None


def paket(adim, cikti, dizin):
    plan = json.loads((cikti / "plan.json").read_text(encoding="utf-8"))
    step = next(s for s in plan["steps"] if s["adim"] == adim)
    fnd = parse_findings(dizin / "dogrulanmadi.md")
    yan = {y["id"]: y for y in parse_yan(dizin)}
    birl = parse_findings(dizin / "birlesik.md")
    old = v015_lines()
    cur = DOC.read_text(encoding="utf-8").replace("\r\n", "\n").split("\n")
    pos = {}
    for i, l in enumerate(cur):
        pos.setdefault(l, []).append(i + 1)
    tmpl = (cikti / "talimat_dogrulayici.md").read_text(encoding="utf-8")
    od = cikti / adim
    od.mkdir(parents=True, exist_ok=True)

    def guncel(n):
        if not 0 < n <= len(old):
            return "12:%d v0.15'te yok" % n
        l = old[n - 1]
        p = pos.get(l, [])
        if len(p) == 1:
            return "12:%d (v0.15) -> guncel 12:%d (birebir ayni satir)| %s" % (n, p[0], l[:600])
        if not p:
            return "12:%d (v0.15) -> guncelde BIREBIR YOK (degismis olabilir; `grep -n` ile alintidan bul)| %s" % (n, l[:600])
        return "12:%d (v0.15) -> guncelde %d yerde birebir (%s)| %s" % (n, len(p), ",".join(map(str, p[:6])), l[:300])

    for g in step["gruplar"]:
        body = tmpl.replace("{ADIM}", adim).replace("{K}", str(g["k"])).replace("{IDS}", ", ".join(g["ids"])).replace("{N}", str(len(g["ids"])))
        for i in g["ids"]:
            if i[0] == "#":
                b = fnd[int(i[1:])]
                text = b["text"]
                yer = b["yer"]
                extra = text
            else:
                y = yan[i]
                text = "## %s | seviye: Dusuk | YAN notu (hukum dosyasi %s, ana bulgu #%s)\n%s" % (i, y["file"], y["parent"], y["text"])
                par = birl.get(y["parent"], {})
                text += "\n\nAna bulgunun yeri: %s" % par.get("yer", "?")
                yer = y["text"] + " " + par.get("yer", "")
                extra = text
            body += "\n" + text + "\n"
            refs = []
            for m in re.finditer(r"(?<![\w.])12:(\d+)(?:\.\.(\d+))?", yer + " " + extra):
                a = int(m.group(1))
                b2 = int(m.group(2)) if m.group(2) else a
                for n in range(a, min(b2, a + 6) + 1):
                    if n not in refs:
                        refs.append(n)
            body += "\nGuncel 12 satirlari (%s):\n" % i
            for n in refs[:14]:
                body += "- " + guncel(n) + "\n"
            sr = []
            for m in re.finditer(r"(?<![\w:.])(0[2-9]|1[01]):(\d+)\b", extra):
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
    dizin = HS / "a10-kd-audit"
    cikti = HS / "a10-kd-audit-2"
    if "--dizin" in a:
        i = a.index("--dizin"); dizin = Path(a[i + 1]); del a[i:i + 2]
    if "--cikti" in a:
        i = a.index("--cikti"); cikti = Path(a[i + 1]); del a[i:i + 2]
    if "--paket" in a:
        paket(a[a.index("--paket") + 1], cikti, dizin)
        return
    items, steps = build_plan(dizin, cikti)
    nn, yy = sinama(items, steps)
    write_plan(items, steps, cikti)
    print("madde: %d (#%d + Y%d); grup: %d; adim: %d -> %s" % (len(items), nn, yy, sum(len(s["gruplar"]) for s in steps), len(steps), cikti))
    for s in steps:
        print(" adim %s: %d dogrulayici" % (s["adim"], len(s["gruplar"])))


if __name__ == "__main__":
    main()
