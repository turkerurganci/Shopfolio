"""Asama 10 -- `12` deep review (A10-KD-deep, Parca A): mercek bulgularini birlestirir, audit hukumleriyle ortusmeyi isaretler,
kademeli dogrulayici gruplarini kurar ve dogrulama paketlerini yazar.

Kipler
  birlestir : python -I 12_dr_birlestir.py birlestir <dizin>
      Girdi  : <dizin>/bulgu_*.md  (`## <MERCEK>-<n> | seviye: Yuksek|Orta|Dusuk`; Katman/Yer/Iddia/Kanit/Oneri)
      Cikti  : birlesik.md (numarali #N), birlesik_ozet.tsv, audit_ortusme.tsv (audit hukumleriyle ortusme adaylari: elle bak)
      Yinelenen: ayni VAL kimliginden ortak en az biri ve iddia sozcuk benzerligi >= 0.45 (ya da kimlik kumeleri esit).
  grupla    : python -I 12_dr_birlestir.py grupla <dizin> [--butce 20] [--ele no,no,...]
      Kademe : Yuksek tek; Orta ayni bolumden 2-4'lu; Dusuk ayni bolumden 5-8'li. Toplam grup <= butce (adimin TOPLAMI).
      Cikti  : plan.md, plan.json, dogrulama_<k>.md, dogrulanmadi.md (butceye sigmayanlar)
      --once : bu no'lari tasiyan gruplar Yuksek'ten hemen sonra (devreden YAN notlari)
      --ele  : audit'te zaten karara baglanmis diye elenen bulgu no'lari (elemeler plan.md'de yazilir)
Sinama (assert): her no tam bir kez (grup ya da dogrulanmadi ya da elenen); grup boyutu kurala uygun; grup sayisi <= butce.
"""
import json
import math
import re
import sys
from collections import Counter
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

ROOT = Path(r"C:\projects\Shopfolio")
DOC = ROOT / "Docs" / "12_VALIDATION_PROTOCOL.md"
HS = Path(r"C:\Users\teknodeveloper\.claude\helper-state\shopfolio")
SEV_ORD = {"Yuksek": 0, "Orta": 1, "Dusuk": 2}
SIZE = {"Yuksek": (1, 1), "Orta": (2, 4), "Dusuk": (5, 8)}
RISK = ["12.3", "12.4", "12.6", "12.7", "12.9", "12.5", "4.4", "4.6", "4.10", "4.5", "12.8", "12.1", "4.12", "4.16", "4.20"]
HEAD = re.compile(r"^## (\S+) \| seviye: (Yuksek|Orta|Dusuk)\s*$")

LINES = DOC.read_text(encoding="utf-8").replace("\r\n", "\n").split("\n")


def section_of(n):
    for i in range(min(n, len(LINES)) - 1, -1, -1):
        m = re.match(r"^###? (\d+(?:\.\d+)?)\.?\s", LINES[i])
        if m:
            return m.group(1)
    return "0"


def sec_key(s):
    return [int(x) for x in s.split(".")]


def toks(s):
    return set(re.findall(r"[\wçğıöşüÇĞİÖŞÜ]{4,}", s.lower()))


def vals(text):
    ids = set(re.findall(r"VAL-[A-F]\d{3}", text))
    for m in re.finditer(r"\(([^)]*)\)", text):
        for t in re.findall(r"\b([A-F])(\d{3})\b", m.group(1)):
            ids.add("VAL-%s%s" % t)
    return ids


def parse_bulgu(D):
    out = []
    for p in sorted(D.glob("bulgu_*.md")):
        cur = None
        for l in p.read_text(encoding="utf-8").split("\n"):
            m = HEAD.match(l)
            if m:
                cur = dict(src=m.group(1), sev=m.group(2), kat="", yer="", iddia="", kanit="", oneri="", dosya=p.name)
                out.append(cur)
                continue
            if l.startswith("## "):
                cur = None
                continue
            if cur is None:
                continue
            for key, pre in (("kat", "Katman:"), ("yer", "Yer:"), ("iddia", "Iddia:"), ("kanit", "Kanit:"), ("oneri", "Oneri:")):
                if l.startswith(pre):
                    cur[key] = l[len(pre):].strip()
                    cur["_last"] = key
                    break
            else:
                if l.strip() and cur.get("_last"):
                    cur[cur["_last"]] += " " + l.strip()
    return out


def birlestir(D):
    ham = parse_bulgu(D)
    for b in ham:
        b["ids"] = vals(b["yer"] + " " + b["iddia"])
        b["t"] = toks(b["iddia"])
        b["lines"] = [int(x) for x in re.findall(r"12:(\d+)", b["yer"])]
    merged = []
    for b in sorted(ham, key=lambda x: (SEV_ORD[x["sev"]], x["src"])):
        hit = None
        for g in merged:
            if g["ids"] and b["ids"] and g["ids"] & b["ids"]:
                j = len(g["t"] & b["t"]) / max(1, len(g["t"] | b["t"]))
                if g["ids"] == b["ids"] or j >= 0.45:
                    hit = g
                    break
        if hit:
            hit["srcs"].append(b["src"])
            hit["ids"] |= b["ids"]
            hit["alt"].append(b)
            if SEV_ORD[b["sev"]] < SEV_ORD[hit["sev"]]:
                hit["sev"] = b["sev"]
        else:
            merged.append(dict(sev=b["sev"], srcs=[b["src"]], ids=set(b["ids"]), t=set(b["t"]), main=b, alt=[]))
    merged.sort(key=lambda g: (SEV_ORD[g["sev"]], min(g["main"]["lines"] or [10 ** 6])))
    out = ["# Birlesik bulgu listesi (betik: 12_dr_birlestir.py)\n\nHam bulgu: %d; birlesik: %d\n" % (len(ham), len(merged))]
    tsv = []
    for n, g in enumerate(merged, 1):
        m = g["main"]
        g["n"] = n
        first = min(m["lines"] or [0])
        out.append("## #%d | seviye: %s | kaynak: %s | katman: %s\nYer: %s\nIddia: %s\nKanit: %s\nOneri: %s\n" % (
            n, g["sev"], ",".join(g["srcs"]), m["kat"], m["yer"], m["iddia"], m["kanit"], m["oneri"]))
        for a in g["alt"]:
            out.append("  (yinelenen %s) Iddia: %s | Oneri: %s\n" % (a["src"], a["iddia"], a["oneri"]))
        tsv.append("%d\t%s\t%s\t12:%d\t%s\t%s\t%s" % (n, g["sev"], m["kat"], first, ",".join(sorted(g["ids"]))[:120], ",".join(g["srcs"]), m["iddia"][:160]))
    (D / "birlesik.md").write_text("\n".join(out), encoding="utf-8", newline="\n")
    (D / "birlesik_ozet.tsv").write_text("\n".join(tsv) + "\n", encoding="utf-8", newline="\n")
    # audit hukumleriyle ortusme
    hk = []
    for base in [HS / "a10-kd-audit"] + [HS / "a10-kd-audit-2" / s for s in ("2a", "2b", "2c", "2r")]:
        for f in sorted(base.glob("hukum_*.md")):
            cur = None
            for l in f.read_text(encoding="utf-8").split("\n"):
                mm = re.match(r"^## (#\d+|Y\d+) \| hukum: (\w+)", l)
                if mm:
                    cur = dict(ref="%s/%s %s" % (base.name if base.name.startswith("a10") else base.parent.name + "/" + base.name, f.stem, mm.group(1)),
                               hk=mm.group(2), txt=[])
                    hk.append(cur)
                elif cur is not None:
                    cur["txt"].append(l)
    for h in hk:
        h["text"] = " ".join(h["txt"])
        h["ids"] = vals(h["text"])
        h["t"] = toks(h["text"])
    ort = []
    for g in merged:
        m = g["main"]
        cands = []
        for h in hk:
            if h["hk"] == "curuk":
                continue
            sh = g["ids"] & h["ids"]
            if sh:
                j = len(g["t"] & h["t"]) / max(1, len(g["t"]))
                if j >= 0.22:
                    cands.append((j, h["ref"], ",".join(sorted(sh))[:60]))
        cands.sort(reverse=True)
        if cands:
            ort.append("%d\t%s\t%s\t%s" % (g["n"], g["sev"], " | ".join("%s (%.2f; %s)" % (r, j, s) for j, r, s in cands[:3]), m["iddia"][:200]))
    (D / "audit_ortusme.tsv").write_text("\n".join(ort) + "\n", encoding="utf-8", newline="\n")
    print("ham %d; birlesik %d; seviye %s; audit ortusme adayi %d" % (len(ham), len(merged), dict(Counter(g["sev"] for g in merged)), len(ort)))


def parse_birlesik(D):
    blocks, cur = {}, None
    for l in (D / "birlesik.md").read_text(encoding="utf-8").split("\n"):
        m = re.match(r"^## #(\d+) \| seviye: (\w+) \| kaynak: ([^|]*)\| katman: (.*)$", l)
        if m:
            cur = int(m.group(1))
            blocks[cur] = dict(sev=m.group(2), kat=m.group(4).strip(), lines=[l])
        elif cur is not None:
            blocks[cur]["lines"].append(l)
    for b in blocks.values():
        b["text"] = "\n".join(b["lines"]).rstrip()
        y = next((x for x in b["lines"] if x.startswith("Yer:")), "")
        b["yer"] = y
    return blocks


def src_line(nn, n, cache={}):
    if nn not in cache:
        fs = sorted((ROOT / "Docs").glob(nn + "_*.md"))
        cache[nn] = fs[0].read_text(encoding="utf-8").replace("\r\n", "\n").split("\n") if fs else []
    L = cache[nn]
    return L[n - 1] if 0 < n <= len(L) else None


def grupla(D, butce, ele, once=frozenset()):
    fnd = parse_birlesik(D)
    items = []
    for no, b in fnd.items():
        m = re.search(r"12:(\d+)", b["yer"])
        sec = section_of(int(m.group(1))) if m else "0"
        items.append(dict(id="#%d" % no, no=no, sev=b["sev"], sec=sec, elendi=no in ele))
    live = [i for i in items if not i["elendi"]]
    units = []
    for sev in ("Yuksek", "Orta", "Dusuk"):
        lo, hi = SIZE[sev]
        bysec = {}
        for it in live:
            if it["sev"] == sev:
                bysec.setdefault(it["sec"], []).append(it)
        for sec, its in bysec.items():
            its.sort(key=lambda x: x["no"])
            g = max(1, math.ceil(len(its) / hi))
            base, extra = divmod(len(its), g)
            i = 0
            for k in range(g):
                sz = base + (1 if k < extra else 0)
                units.append(dict(sev=sev, sec=sec, ids=[x["id"] for x in its[i:i + sz]]))
                i += sz

    def rank(u):
        if u["sev"] == "Yuksek":
            return (0, 0, int(u["ids"][0][1:]))
        if once & {int(x[1:]) for x in u["ids"]}:
            return (0, 1, min(int(x[1:]) for x in u["ids"]))
        r = RISK.index(u["sec"]) if u["sec"] in RISK else len(RISK)
        return (1, 0 if u["sev"] == "Orta" else 1, r, sec_key(u["sec"]))

    units.sort(key=rank)
    kabul = units[:butce]
    ertele = units[butce:]
    for k, u in enumerate(kabul, 1):
        u["k"] = k
    # sinama
    ids = [i for u in kabul + ertele for i in u["ids"]] + [i["id"] for i in items if i["elendi"]]
    assert len(ids) == len(set(ids)), "bir madde iki yerde"
    assert set(ids) == {i["id"] for i in items}, "eksik/fazla madde"
    assert len(kabul) <= butce
    by = {i["id"]: i for i in items}
    for u in kabul + ertele:
        lo, hi = SIZE[u["sev"]]
        assert len(u["ids"]) <= hi, "grup boyutu"
        assert len({by[i]["sec"] for i in u["ids"]}) == 1
        if len(u["ids"]) < lo:
            same = [x for x in live if x["sev"] == u["sev"] and x["sec"] == u["sec"]]
            assert len(same) == len(u["ids"]), "kucuk grup bolumun tamami degil"
    tmpl = (D / "talimat_dogrulayici.md").read_text(encoding="utf-8")
    cur_lines = LINES
    for old in D.glob("dogrulama_*.md"):
        old.unlink()
    for u in kabul:
        body = tmpl.replace("{K}", str(u["k"])).replace("{IDS}", ", ".join(u["ids"])).replace("{N}", str(len(u["ids"])))
        for i in u["ids"]:
            b = fnd[int(i[1:])]
            body += "\n" + b["text"] + "\n"
            refs = []
            for m in re.finditer(r"(?<![\w.])12:(\d+)(?:\.\.(\d+))?", b["text"]):
                a = int(m.group(1))
                e = int(m.group(2)) if m.group(2) else a
                for n in range(a, min(e, a + 6) + 1):
                    if n not in refs:
                        refs.append(n)
            body += "\nGuncel 12 satirlari (%s):\n" % i
            for n in refs[:14]:
                if 0 < n <= len(cur_lines):
                    body += "- 12:%d| %s\n" % (n, cur_lines[n - 1][:700])
            sr = []
            for m in re.finditer(r"(?<![\w:.])(0[2-9]|1[01]):(\d+)\b", b["text"]):
                kk = (m.group(1), int(m.group(2)))
                if kk not in sr:
                    sr.append(kk)
            if sr:
                body += "Kaynak satirlari (%s):\n" % i
                for nn, n in sr[:12]:
                    l = src_line(nn, n)
                    body += "- %s:%d| %s\n" % (nn, n, (l or "(satir yok)")[:500])
        (D / ("dogrulama_%d.md" % u["k"])).write_text(body, encoding="utf-8", newline="\n")
    o = ["# A10-KD-deep plani (%d bulgu; butce %d)\n" % (len(items), butce),
         "| k | seviye | bolum | maddeler |\n|---|---|---|---|"]
    for u in kabul:
        o.append("| %d | %s | %s | %s |" % (u["k"], u["sev"], u["sec"], ", ".join(u["ids"])))
    o.append("\nDogrulanmadi (butceye sigmayan): %d grup\n" % len(ertele))
    for u in ertele:
        o.append("- %s %s: %s" % (u["sev"], u["sec"], ", ".join(u["ids"])))
    if ele:
        o.append("\nElenen (audit'te karara baglanmis): %s" % ", ".join("#%d" % n for n in sorted(ele)))
    (D / "plan.md").write_text("\n".join(o) + "\n", encoding="utf-8", newline="\n")
    (D / "plan.json").write_text(json.dumps(dict(kabul=kabul, ertele=ertele, ele=sorted(ele)), ensure_ascii=False, indent=1), encoding="utf-8", newline="\n")
    dn = ["# Dogrulanmadi -- A10-KD-deep'te butceye (adim basina 20 Opus dogrulayici) sigmayan bulgular\n",
          "Dokumana UYGULANMAZ; yardimci `A10-KD-deep-2a…` alt adimlari acar.\n"]
    for u in ertele:
        for i in u["ids"]:
            dn.append(fnd[int(i[1:])]["text"] + "\n")
    (D / "dogrulanmadi.md").write_text("\n".join(dn), encoding="utf-8", newline="\n")
    print("bulgu %d; elenen %d; kabul grup %d (%d bulgu); dogrulanmadi grup %d (%d bulgu)" % (
        len(items), len(ele), len(kabul), sum(len(u["ids"]) for u in kabul), len(ertele), sum(len(u["ids"]) for u in ertele)))
    print("dogrulanmadi seviye:", dict(Counter(by[i]["sev"] for u in ertele for i in u["ids"])))


def main():
    a = sys.argv[1:]
    if len(a) < 2:
        raise SystemExit("kullanim: 12_dr_birlestir.py birlestir|grupla <dizin> [--butce N] [--ele no,no]")
    mode, D = a[0], Path(a[1])
    butce, ele = 20, set()
    if "--butce" in a:
        butce = int(a[a.index("--butce") + 1])
    if "--ele" in a:
        v = a[a.index("--ele") + 1]
        ele = {int(x) for x in v.split(",") if x}
    once = set()
    if "--once" in a:
        once = {int(x) for x in a[a.index("--once") + 1].split(",") if x}
    if mode == "birlestir":
        birlestir(D)
    elif mode == "grupla":
        grupla(D, butce, ele, once)
    else:
        raise SystemExit("kip: birlestir | grupla")


if __name__ == "__main__":
    main()
