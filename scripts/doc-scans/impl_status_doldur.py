"""Implementation doneminin acilisi -- Docs/IMPLEMENTATION_STATUS.md'yi `11`'in faz ve task
listesiyle doldurur (checklist 7 adim 6, O-207a; K-1574).

Kaynak: `Docs/11_IMPLEMENTATION_PLAN.md`
  - 3. bolum faz tablosu: `| F0 | Ad | Kapsam | `T01`-`T10` (10) |`
  - `### Fn — Ad` basliklari altindaki `#### TXX: Ad` task bloklari.
Sablondaki faz gate tablosu satirlari (F0-F6) ve tek `## F0 — <Faz adi>` bolumu doldurulur;
F1-F6 bolumleri eklenir. Task satiri: `| T01 | Ad | ⬚ | — | — | — |`.
Sayim 11'in tablosundaki (N) ile task bloklarini eslestirir; fark varsa cikis 1.

Kullanim (depo kokunden): python -I scripts/doc-scans/impl_status_doldur.py [--yaz]
`--yaz` verilmezse yalniz sayar ve raporlar.
"""
import re
import sys
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

ROOT = Path(".")
PLAN = ROOT / "Docs" / "11_IMPLEMENTATION_PLAN.md"
STATUS = ROOT / "Docs" / "IMPLEMENTATION_STATUS.md"
YAZ = "--yaz" in sys.argv

plan = PLAN.read_text(encoding="utf-8")
lines = plan.split("\n")

# Faz tablosu
fazlar = {}
for ln in lines:
    m = re.match(r"^\| (F\d) \| ([^|]+?) \| .* \| `(T\d+[a-z]?)`–`(T\d+[a-z]?)` \((\d+)\) \|$", ln)
    if m:
        fazlar[m.group(1)] = dict(ad=m.group(2), bas=m.group(3), son=m.group(4), n=int(m.group(5)), tasks=[])

# Task bloklari
cur = None
for ln in lines:
    m = re.match(r"^### (F\d) — (.+)$", ln)
    if m:
        cur = m.group(1)
        continue
    m = re.match(r"^#### (T\d+[a-z]?): (.+)$", ln)
    if m and cur in fazlar and m.group(1) != "TXX":
        fazlar[cur]["tasks"].append((m.group(1), m.group(2).strip()))

hata = 0
toplam = 0
for f, d in sorted(fazlar.items()):
    sayi = len(d["tasks"])
    toplam += sayi
    ok = sayi == d["n"] and d["tasks"][0][0] == d["bas"] and d["tasks"][-1][0] == d["son"]
    print(f"{f} {d['ad']}: tablo {d['n']} / blok {sayi} / aralik {d['tasks'][0][0]}-{d['tasks'][-1][0]} {'OK' if ok else 'FARK'}")
    if not ok:
        hata += 1
print(f"TOPLAM task: {toplam}; faz: {len(fazlar)}")
if hata or len(fazlar) != 7:
    print("HATA: 11 ile eslesmiyor")
    sys.exit(1)

if not YAZ:
    sys.exit(0)

st = STATUS.read_text(encoding="utf-8")

# Faz gate tablosu
for f, d in sorted(fazlar.items()):
    eski = f"| {f} | | | ⬚ | — | |"
    yeni = f"| {f} | {d['ad']} | `{d['bas']}`–`{d['son']}` ({d['n']}) | ⬚ | — | |"
    if eski not in st:
        print(f"HATA: sablon satiri yok: {eski}")
        sys.exit(1)
    st = st.replace(eski, yeni, 1)

# Faz bolumleri
bolum_eski = re.search(r"## F0 — <Faz adı>\n\n.*?\n\n---\n", st, re.S)
if not bolum_eski:
    print("HATA: F0 sablon bolumu yok")
    sys.exit(1)
parcalar = []
for f, d in sorted(fazlar.items()):
    s = [f"## {f} — {d['ad']}", "", "| Task | Ad | Durum | Doğrulama | Commit | PR |", "|---|---|---|---|---|---|"]
    for t, ad in d["tasks"]:
        s.append(f"| {t} | {ad} | ⬚ | — | — | — |")
    parcalar.append("\n".join(s))
yeni_bolum = "\n\n---\n\n".join(parcalar) + "\n\n---\n"
st = st[: bolum_eski.start()] + yeni_bolum + st[bolum_eski.end():]

# Son guncelleme
st = re.sub(
    r"^\*\*Son güncelleme:\*\* .*$",
    f"**Son güncelleme:** 2026-11-02 — Implementation dönemi açıldı: tracker `11`'in faz ve task listesiyle dolduruldu ({len(fazlar)} faz, {toplam} task, hepsi ⬚); sıradaki iş `/task T02` (önce D-01 kapısı, `Docs/PRODUCT_DISCOVERY_STATUS.md` §25).",
    st, count=1, flags=re.M,
)
STATUS.write_text(st, encoding="utf-8", newline="\n")
print("yazildi")
