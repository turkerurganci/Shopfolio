"""Asama 10 acilisi -- sablon taramasi, konu kimligi taramasi ve girdi sayimlari
(checklists/document-stage.md 1).

Kullanim (depo kokunden): python -I scripts/doc-scans/12_acilis_tarama.py . [cikti_dosyasi]
"""
import re
import subprocess
import sys
from pathlib import Path

sys.stdout.reconfigure(encoding="utf-8")
root = Path(sys.argv[1] if len(sys.argv) > 1 else ".")
out_path = sys.argv[2] if len(sys.argv) > 2 else None
lines_out = []


def p(s=""):
    lines_out.append(s)
    print(s)


path12 = root / "Docs/12_VALIDATION_PROTOCOL.md"
meth = (root / "Docs/00_PROJECT_METHODOLOGY.md").read_text(encoding="utf-8")
doc = path12.read_text(encoding="utf-8").split("\n")
full = "\n".join(doc)

# ---------------------------------------------------------------- 1. sablon
p("## 1. Sablon taramasi -- `12`")
heads = [(i, l) for i, l in enumerate(doc, 1) if l.startswith("## ")]
subs = [(i, l) for i, l in enumerate(doc, 1) if l.startswith("### ")]
p(f"`## ` basliklari: {len(heads)}; `### ` basliklari: {len(subs)}")
for i, l in heads:
    p(f"  {i}: {l}")
acik = [l for _, l in heads + subs if re.search(r"(?i)açık karar|open decision", l)]
p(f"Acik kararlar bolumu: {'var' if acik else 'yok'}")
size = path12.stat().st_size
p(f"`12` boyutu (bayt): {size}")

p("Sablonun gosterdigi yerler ve varligi:")
for i, l in enumerate(doc, 1):
    for m in re.finditer(r"00 §([A-Z])(?:\.(\d+))?|(\.claude/[\w/\.\-]+\.md)|`(DEFERRED_BACKLOG\.md)`", l):
        if m.group(3):
            p(f"  {i}: {m.group(3)} -> {'var' if (root / m.group(3)).exists() else 'YOK'}")
        elif m.group(4):
            p(f"  {i}: {m.group(4)} -> {'var' if (root / 'Docs' / m.group(4)).exists() else 'YOK'}")
        else:
            sec = m.group(1) + (("." + m.group(2)) if m.group(2) else "")
            pat = rf"^#{{2,4}} {re.escape(sec)}[\. ]"
            p(f"  {i}: 00 §{sec} -> {'baslik var' if re.search(pat, meth, re.M) else 'BASLIK YOK'}")

p("Sablonda 'ayri bolumde' tipi yonlendirmeler:")
for i, l in enumerate(doc, 1):
    if re.search(r"ayrı bölüm|ayrı bir bölüm|ayrıca yaz", l):
        p(f"  {i}: {l[:160]}")
p("  (varsa yukaridadir)")

# kimlik bicimi
p("Sablonun kendi kimlik bicimleri:")
for i, l in enumerate(doc, 1):
    for m in re.finditer(r"VAL-[A-Z0-9]+", l):
        p(f"  {i}: {m.group(0)}  <- {l.strip()[:110]}")

# sablonun terimleri ve L1-L5'te karsiliklari
terms = {
    "Defect kaydi": r"[Dd]efect",
    "Kanit arsivi": r"[Kk]anıt arşivi",
    "Risk envanteri / risk kaydi": r"risk envanteri|risk kaydı",
    "S1/S2/S3 hata sinifi (00 D.10)": r"\bS[123]\b",
    "Okuma duzeni / boyut (100 KB)": r"100 KB|okuma düzeni",
    "Kanit ortami (mock/sandbox/gercek)": r"mock|sandbox",
    "Yeniden dogrulama": r"[Yy]eniden doğrulama|[Rr]e-validation",
    "Acik kararlar": r"açık karar",
}
layers = [root / "Docs/00_PROJECT_METHODOLOGY.md", root / ".claude/INSTRUCTIONS.md", root / ".claude/GUARDRAILS.md",
          root / ".claude/CONTEXT.md"] + sorted((root / ".claude/skills").glob("*/SKILL.md")) \
         + sorted((root / ".claude/checklists").glob("*.md"))
p("Sablon terimleri: 12'de gecis sayisi / L1-L5 dosyalarinda gecis sayisi (dosya adi:adet):")
for k, rx in terms.items():
    in12 = len(re.findall(rx, full))
    other = []
    for f in layers:
        n = len(re.findall(rx, f.read_text(encoding="utf-8")))
        if n:
            other.append(f"{f.name if f.name != 'SKILL.md' else f.parent.name}:{n}")
    p(f"  {k}: 12'de {in12}; L1-L5: {', '.join(other) or '-'}")

# ---------------------------------------------------------------- 2. kimlik
p("")
p("## 2. Konu kimligi taramasi")
files = subprocess.run(["git", "-C", str(root), "ls-files", "--", "Docs", ".claude", "scripts", ".github"],
                       capture_output=True, text=True, encoding="utf-8").stdout.split("\n")
files = [f for f in files if f.strip() and not f.endswith((".png", ".jpg", ".pyc"))]
p(f"Taranan dosya sayisi: {len(files)}")
texts = {}
for f in files:
    try:
        texts[f] = (root / f).read_text(encoding="utf-8")
    except Exception:
        pass
fam = {}
for f, txt in texts.items():
    for m in re.finditer(r"\b([A-Z]{1,4})[0-9]*-[0-9]+", txt):
        fam[m.group(1)] = fam.get(m.group(1), 0) + 1
CAND = ["VP", "DP", "DK", "QA", "VPR"]
p("Aday onekler (kalip `\\bPFX[0-9]*-[0-9]+`, `\\bPFX[0-9]+\\b`, aile sayisi `PFX[0-9]*-n`):")
for c in CAND:
    h1 = []
    h2 = []
    for f, txt in texts.items():
        h1 += [(f, m.group(0)) for m in re.finditer(rf"\b{c}[0-9]*-[0-9]+", txt)]
        h2 += [(f, m.group(0)) for m in re.finditer(rf"\b{c}[0-9]+\b", txt)]
    p(f"  {c}: `{c}n-mm` {len(h1)} eslesme" + (" " + ", ".join(f"{a}:{b}" for a, b in h1[:6]) if h1 else "")
      + f"; `{c}n` {len(h2)} eslesme" + (" " + ", ".join(f"{a}:{b}" for a, b in h2[:6]) if h2 else "")
      + f"; aile '{c}': {fam.get(c, 0)}")
p("Tum `X[n]-m` aileleri (adet, azalan):")
p("  " + ", ".join(f"{k} {v}" for k, v in sorted(fam.items(), key=lambda kv: -kv[1])))
val = sum(len(re.findall(r"\bVAL-", t)) for t in texts.values())
p(f"Sablonun getirdigi `VAL-` ailesi: {val} eslesme (dosyalar: "
  + ", ".join(sorted(f for f, t in texts.items() if re.search(r"\bVAL-", t))) + ")")
sev = {}
for f, txt in texts.items():
    for m in re.finditer(r"\bS[123]\b", txt):
        sev[f] = sev.get(f, 0) + 1
p(f"`S1`/`S2`/`S3` (00 D.10 hata sinifi): {sum(sev.values())} eslesme, {len(sev)} dosya")
oneki = ["B", "AK", "UI", "TM", "VM", "API", "ENT", "KK", "IP"]
p("Onceki asama onekleri (aile sayisi): " + ", ".join(f"{k} {fam.get(k, 0)}" for k in oneki))
dfam = {k: sum(len(re.findall(rf"\b{k}-[0-9]+", t)) for t in texts.values()) for k in ("UC", "MK", "DV", "KP", "KD", "VAL")}
p(f"Dokuman kimlik aileleri: {dfam}")
tk = {"T<NN>[a|b]": sum(len(re.findall(r"\bT[0-9]{2}[ab]?\b", t)) for t in texts.values()),
      "F<n>": sum(len(re.findall(r"\bF[0-9]\b", t)) for t in texts.values())}
p(f"Task/faz kimlikleri (depo geneli): {tk}")

# ---------------------------------------------------------------- 3. girdiler
p("")
p("## 3. Girdilerin sayimi (`02`-`11`)")
tot = 0
for n in range(2, 12):
    cand = sorted((root / "Docs").glob(f"{n:02d}_*.md"))
    if not cand:
        p(f"  {n:02d}: YOK")
        continue
    f = cand[0]
    b = f.stat().st_size
    tot += b
    txt = f.read_text(encoding="utf-8")
    ver = re.search(r"\*\*Versiyon: (v[\d.]+)\*\*", txt)
    h2 = len(re.findall(r"^## ", txt, re.M))
    h3 = len(re.findall(r"^### ", txt, re.M))
    p(f"  {n:02d} {f.name}: {b} bayt ({b / 1048576:.2f} MB), {ver.group(1) if ver else '?'}, `## ` {h2}, `### ` {h3}")
p(f"  Toplam: {tot} bayt ({tot / 1048576:.2f} MB)")
p(f"  Okunacak sablon + park bloklari: `12` {size} bayt")

if out_path:
    Path(out_path).write_text("\n".join(lines_out) + "\n", encoding="utf-8")
