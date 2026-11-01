"""Asama 10 -- 12 audit Parca B: INSTRUCTIONS §7 uc tarama + K tekilligi (degisen dosyalarda)."""
import re
import subprocess
import sys
from pathlib import Path
from collections import Counter

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")
R = Path(r"C:\projects\Shopfolio")
out = subprocess.run(["git", "status", "--short"], cwd=R, capture_output=True, text=True, encoding="utf-8").stdout
files = [R / l[3:].strip() for l in out.split("\n") if l.strip()]
kontrol = re.compile(r"[\x00-\x08\x0b\x0c\x0e-\x1f]")
sorun = 0
for f in files:
    b = f.read_bytes()
    t = b.decode("utf-8")
    cr = b.count(b"\r")
    kk = len(kontrol.findall(t))
    if cr or kk:
        sorun += 1
        print("SORUN", f.name, "CR", cr, "kontrol", kk)
d = subprocess.run(["git", "diff", "-U0"], cwd=R, capture_output=True, text=True, encoding="utf-8").stdout
art = [l for l in d.split("\n") if l.startswith("+") and re.search(r"\$[0-9]", l)]
print("dosya:", len(files), "sorun:", sorun, "$N artigi:", len(art))
st = (R / "Docs" / "PRODUCT_DISCOVERY_STATUS.md").read_text(encoding="utf-8")
ids = re.findall(r"^\| (K-\d+) \|", st, re.M)
c = Counter(ids)
print("K satiri:", len(ids), "tekrar:", [k for k, n in c.items() if n > 1], "son:", ids[-3:])
