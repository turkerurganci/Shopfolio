"""Asama 9 -- yazim turu kapanis taramalari (checklists/document-stage.md 4; karar kaydi 22.1, Y8).

Uc tarama:
  A  anlamsal tarama kaydi: karar kaydinin K-1401...son satirlarindan etki sutunu `11` tasiyanlar;
     her biri icin `11`'de K atfi var mi (kati desen: `11` kod aralikli; gevsek yanlis pozitifler
     11_devir_esle.py'de okunmustur)
  B  devir taramasi: park blogunun bes maddesi ve D-01 -- her birinin task'i `11 5`'te blok olarak
     yazili mi; park blogunda "karsilandi: TXX" isareti var mi
  C  belirsiz ifade ve olu placeholder: `11`'de yasakli kaliplar

Kullanim (depo kokunden): python -I scripts/doc-scans/11_yazim_kapanis.py .
Cikis kodu: sorun varsa 1.
"""
import re
import sys
from pathlib import Path

sys.stdout.reconfigure(encoding="utf-8")
root = Path(sys.argv[1] if len(sys.argv) > 1 else ".")
plan = (root / "Docs" / "11_IMPLEMENTATION_PLAN.md").read_text(encoding="utf-8")
kayit = (root / "Docs" / "PRODUCT_DISCOVERY_STATUS.md").read_text(encoding="utf-8")
sorun = 0

# ---- A ----
print("## A. Anlamsal tarama -- etki sutunu `11` olan K-1401...")
satirlar = [s for s in kayit.splitlines() if re.match(r"\| K-14\d\d \|", s)]
basliklar = set(re.findall(r"^#{2,4} (\d+(?:\.\d+)*)[ .]", plan, re.M))
fazlar = set(re.findall(r"^### (F\d) ", plan, re.M))
tasklar = set(re.findall(r"^#### (T\d+[a-z]?): ", plan, re.M))
aday = set(re.findall(r"^\| (T\d+[a-z]?) \|", plan, re.M))


def tid_ac(a, b):
    n1, n2 = int(a[1:]), int(b[1:])
    return {f"T{n}" for n in range(n1, n2 + 1)}


etkili = []
eksik = []
atifsiz = []
for s in satirlar:
    hucre = s.split("|")
    kid = hucre[1].strip()
    if int(kid[2:]) < 1401:
        continue
    etki = hucre[-2]
    if not re.search(r"`11`", etki):
        continue
    etkili.append(kid)
    # etki hucresinin `11` ile baslayan parcasi (" · " ile ayrilir): bolum, faz, task karsiliklari
    parca = [p for p in etki.split(" · ") if p.strip().startswith("`11`")]
    metin_parca = " ".join(parca)
    yok = []
    for n in re.findall(r"§(\d+(?:\.\d+)*)", metin_parca):
        if n not in basliklar:
            yok.append("§" + n)
    for f in re.findall(r"§5 (F\d)", metin_parca):
        if f not in fazlar:
            yok.append(f)
    for a, b in re.findall(r"(T\d+)[–-](T\d+)", metin_parca):
        yok += sorted(tid_ac(a, b) - aday)
    for t in re.findall(r"\b(T\d+[a-z]?)\b", metin_parca):
        if t not in aday:
            yok.append(t)
    if yok:
        eksik.append((kid, sorted(set(yok))))
    if kid not in plan:
        atifsiz.append(kid)
print(f"K-1401... satiri {len(satirlar)}; etki sutunu `11` olan {len(etkili)}; karsiligi olmayan bolum/faz/task: {len(eksik)}")
for kid, yok in eksik:
    print("  karsiliksiz:", kid, yok)
print(f"`11`'de K atfi bulunmayan {len(atifsiz)} satirin bolumleri ve task'lari yukarida denetlendi (atifsiz K, karsiliksiz sayilmaz): {', '.join(atifsiz)}")
sorun += len(eksik)

# ---- B ----
print("\n## B. Devir taramasi -- park blogu ve D-01")
beklenen = {
    "P8.1": [],
    "P8.2": ["T04"],
    "P8.3": ["T04", "T09"],
    "P8.4": ["T03"],
    "P8.5": ["T230", "T231", "T548"],
    "D-01": ["T02", "T03", "T04", "T05", "T06", "T07", "T09"],
}
for madde, tasklar in beklenen.items():
    yok = [t for t in tasklar if not re.search(rf"^#### {t}: ", plan, re.M)]
    print(f"  {madde}: task {', '.join(tasklar) or '(bicim; task yok)'}; blogu olmayan: {', '.join(yok) or 'yok'}")
    sorun += len(yok)
park = plan.split("**Aşama 8 park bloğunun durumu", 1)[1].split("**Yazım konvansiyonları", 1)[0]
for madde in ("P8.1", "P8.2", "P8.3", "P8.4", "P8.5"):
    satir = [s for s in park.splitlines() if s.startswith(f"| {madde} ")]
    ok = bool(satir) and "karşılandı" in satir[0]
    print(f"  park satiri {madde}: {'karsilandi isareti var' if ok else 'ISARET YOK'}")
    if not ok:
        sorun += 1

# ---- C ----
print("\n## C. Belirsiz ifade ve olu placeholder")
kaliplar = [
    r"muhtemelen", r"belirlenecek", r"sonra doldurulacak", r"\bTBD\b", r"\bTODO\b", r"\bXXX\b",
    r"yazılacak\b(?! kod)", r"doldurulacak", r"\(sonra\)", r"<[A-Za-zÇĞİÖŞÜçğıöşü][^>\n`]{0,30}>(?!\s*:)",
    r"⏳",
]
# kod blogu ve satir ici kod disinda ara; `<...>` komut sablonlari (grep ornekleri) satir ici kodda kalir
metin = re.sub(r"```.*?```", "", plan, flags=re.S)
metin = re.sub(r"`[^`\n]*`", "``", metin)
bulunan = 0
for no, satir in enumerate(metin.splitlines(), 1):
    for k in kaliplar:
        if re.search(k, satir, re.I if k not in (r"\bTBD\b", r"\bTODO\b", r"\bXXX\b") else 0):
            print(f"  satir {no}: [{k}] {satir[:150]}")
            bulunan += 1
            break
print(f"bulunan: {bulunan}")
sorun += bulunan

print(f"\nTOPLAM SORUN: {sorun}")
sys.exit(1 if sorun else 0)
