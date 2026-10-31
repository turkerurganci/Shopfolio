"""Asama 9 acilisi -- sablon taramasi, konu kimligi taramasi ve girdi sayimlari
(checklists/document-stage.md 1).

Kullanim (depo kokunden): python -I scripts/doc-scans/11_acilis_tarama.py . [cikti_dosyasi]
"""
import re
import subprocess
import sys
from pathlib import Path

root = Path(sys.argv[1] if len(sys.argv) > 1 else ".")
out_path = sys.argv[2] if len(sys.argv) > 2 else None
lines_out = []


def p(s=""):
    lines_out.append(s)
    print(s)


# ---------------------------------------------------------------- 1. sablon
doc = (root / "Docs/11_IMPLEMENTATION_PLAN.md").read_text(encoding="utf-8").split("\n")
p("## 1. Sablon taramasi -- `11`")
heads = [(i, l) for i, l in enumerate(doc, 1) if l.startswith("## ")]
p(f"`## ` basliklari: {len(heads)}")
for i, l in heads:
    p(f"  {i}: {l}")
acik = [l for _, l in heads if re.search(r"(?i)açık karar|open decision", l)]
p(f"Acik kararlar bolumu: {'var' if acik else 'yok'}")
# sablonun kendi icinde istedigi yerler
ASK_RE = re.compile(r"ayrı bölümde|ayrı bir bölüm|00 §[A-Z]|\.claude/[\w/\.\-]+|`(\d\d)`")
p("Sablonun istedigi / gosterdigi yerler (satir: ifade) ve varligi:")
seen = set()
for i, l in enumerate(doc, 1):
    for m in re.finditer(r"00_PROJECT_METHODOLOGY\.md`? §([A-Z])|00 §([A-Z])|(\.claude/[\w/\.\-]+\.md)", l):
        key = m.group(0)
        sec = m.group(1) or m.group(2)
        path = m.group(3)
        if path:
            ok = (root / path).exists()
            p(f"  {i}: {key} -> {'var' if ok else 'YOK'}")
        elif sec:
            meth = (root / "Docs/00_PROJECT_METHODOLOGY.md").read_text(encoding="utf-8")
            ok = re.search(rf"^#{{2,3}} {sec}[\. ]", meth, re.M) is not None
            p(f"  {i}: 00 §{sec} -> {'baslik var' if ok else 'BASLIK YOK'}")
for i, l in enumerate(doc, 1):
    if re.search(r"ayrı bölümde|ayrı bir bölüm", l):
        p(f"  {i}: 'ayri bolumde' ifadesi: {l[:160]}")
# gozlenen eksikler: sablonun tasimadigi yerler
checks = {
    "okuma duzeni ve boyut sinirlari (100 KB kurali)": r"100 KB|okuma düzeni",
    "yazim konvansiyonlari / geri besleme yeri": r"geri besleme|yazım konvansiyon",
    "open decisions / acik kararlar": r"açık karar",
    "task boyutu / faz sinirlari olcutu": r"task boyutu|faz sınır",
    "kapi (gate) olcutu": r"gate|kapı",
    "test beklentisi alani": r"Test beklentisi",
    "forward pointer": r"[Ff]orward pointer",
    "D-01 / ertelenmis is yeri": r"D-01|DEFERRED",
}
full = "\n".join(doc)
p("Sablonda aranan ifadeler:")
for k, rx in checks.items():
    p(f"  {k}: {'var' if re.search(rx, full) else 'yok'}")
size = (root / "Docs/11_IMPLEMENTATION_PLAN.md").stat().st_size
p(f"`11` boyutu (bayt): {size}")

# ---------------------------------------------------------------- 2. kimlik
p("")
p("## 2. Konu kimligi taramasi (aday `IPn-mm`)")
files = subprocess.run(["git", "-C", str(root), "ls-files", "--", "Docs", ".claude", "scripts", ".github"],
                       capture_output=True, text=True, encoding="utf-8").stdout.split("\n")
files = [f for f in files if f.strip() and not f.endswith((".png", ".jpg", ".pyc"))]
p(f"Taranan dosya sayisi: {len(files)}")
fam = {}
ip_hits = []
ip_word = []
task_ids = {}
doc_fams = {"UC": 0, "MK": 0, "DV": 0}
for f in files:
    try:
        txt = (root / f).read_text(encoding="utf-8")
    except Exception:
        continue
    for m in re.finditer(r"\b([A-Z]{1,4})[0-9]+-[0-9]+", txt):
        fam[m.group(1)] = fam.get(m.group(1), 0) + 1
    for m in re.finditer(r"\bIP[0-9]*-[0-9]+", txt):
        ip_hits.append((f, m.group(0)))
    for m in re.finditer(r"\bIP[0-9]+\b", txt):
        ip_word.append((f, m.group(0)))
    for m in re.finditer(r"\b(T[0-9]{2}[ab]?|F[0-9])\b", txt):
        task_ids[m.group(1)[0] + ("XX" if m.group(1)[0] == "T" else "n")] = \
            task_ids.get(m.group(1)[0] + ("XX" if m.group(1)[0] == "T" else "n"), 0) + 1
    for k in doc_fams:
        doc_fams[k] += len(re.findall(rf"\b{k}-[0-9]+", txt))
p(f"`IPn-mm` kalibi (\\bIP[0-9]*-[0-9]+): {len(ip_hits)} eslesme" + (": " + ", ".join(f"{a}:{b}" for a, b in ip_hits[:10]) if ip_hits else ""))
p(f"`IPn` kalibi (\\bIP[0-9]+\\b): {len(ip_word)} eslesme" + (": " + ", ".join(f"{a}:{b}" for a, b in ip_word[:10]) if ip_word else ""))
p("`Xn-mm` aileleri (adet, azalan):")
for k, v in sorted(fam.items(), key=lambda kv: -kv[1]):
    p(f"  {k}: {v}")
p(f"Task kimlikleri: T<NN>[a|b] ve F<n> kullanimi (11'de plan konusu; aile adi): {task_ids}")
p(f"Dokuman kimlik aileleri (UC-, MK-, DV-) eslesme: {doc_fams}")
p("Onceki asama onekleri: B, AK, UI, TM, VM, API, ENT, KK -- aday IP bunlarla cakismaz: "
  + ("EVET (IP ailesi yok)" if "IP" not in fam else "HAYIR"))

# ---------------------------------------------------------------- 3. girdiler
p("")
p("## 3. Girdilerin sayimi (`02`-`10`)")
tot = 0
for n in range(2, 11):
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

if out_path:
    Path(out_path).write_text("\n".join(lines_out) + "\n", encoding="utf-8")
