"""Compiles scratchpad cp8/sonuc.md from the verifier verdict files (hukum_<n>.md).

Usage: python -I 08_checkpoint_sonuc_derle.py <cp8_dir>

Governing verdict per finding = the one from the highest-numbered verdict file that mentions it (re-verification files
12, 13, 15 supersede the first-pass files). Also checks that each ESKİ text occurs exactly once in its target file.
"""
import re
import sys
from pathlib import Path

sys.stdout.reconfigure(encoding="utf-8")
D = Path(sys.argv[1])
ROOT = Path(r"C:\projects\Shopfolio")

LENSES = [
    ("M1", "K-1302…K-1392 etki sütunu ↔ hedef dokümanda karşılık (betik adayı: 101 A, 5 B)", 1),
    ("M2", "09'un adları ↔ 06 §4, §5, 07 ve üst dokümanlar; yeni alan/kural taraması (586 ad, 379 aday)", 1),
    ("M3", "09 §3.5 değer tablosu ↔ 05 §8.4 ve 02 §11", 4),
    ("M4", "09 Kütüphaneler tablosu ↔ gövde, 05, DEPLOY_RUNBOOK", 3),
    ("M5", "09 §2 geri besleme tablosu ↔ hedeflerin gerçek metni; tabloda olmayan geri besleme", 5),
    ("S1", "09 satır 1-638 (başlık, §1-§8): özet ↔ ayrıntı", 6),
    ("S2", "09 §8-§10.2: özet ↔ ayrıntı", 6),
    ("S3", "09 §10.3-§12: özet ↔ ayrıntı", 10),
    ("S4", "09 §13-§15.5: özet ↔ ayrıntı", 12),
    ("S5", "09 §15.6-§19: özet ↔ ayrıntı", 4),
    ("S6", "09 §23-§25: özet ↔ ayrıntı", 3),
]


def parse_hukum():
    gov = {}
    srcs = {}
    files = sorted(D.glob("hukum_*.md"), key=lambda p: int(re.search(r"(\d+)", p.stem).group(1)))
    for f in files:
        n = int(re.search(r"(\d+)", f.stem).group(1))
        if n == 14:
            continue  # superseded by 15 (same finding, re-verified at the right tier)
        t = f.read_text(encoding="utf-8")
        for p in re.split(r"(?m)^### ", t)[1:]:
            head = p.split("\n", 1)[0]
            m = re.match(r"(\S+) — HÜKÜM: (.+)", head)
            if not m:
                continue
            kid, verdict = m.group(1), m.group(2).strip()
            gov[kid] = (n, verdict, "### " + p.rstrip() + "\n")
            srcs.setdefault(kid, []).append(n)
    return gov, srcs


def field(block, name):
    m = re.search(rf"(?m)^[-\s]*`?{name}`?\s*(.*)$", block)
    return m.group(1).strip() if m else ""


_docs = {}


def docs_text():
    if not _docs:
        for p in sorted((ROOT / "Docs").glob("*.md")):
            _docs[p.name] = p.read_text(encoding="utf-8")
    return _docs


def eski_check(block):
    """Every ESKİ text (lines from 'ESKİ:' up to the next 'YENİ:') must occur exactly once in exactly one Docs file."""
    if "s105_pairs" in block:
        return []
    res = []
    parts = re.split(r"(?m)^[-\s]*`?ESKİ`?:", block)[1:]
    for part in parts:
        s = re.split(r"(?m)^[-\s]*`?YENİ`?:", part)[0].strip()
        s = re.sub(r"(?m)^```\w*\s*$", "", s).strip()
        for _ in range(3):
            s = re.sub(r"[\s`]*\((?:dosyada|grep|Grep)[^)]*\)[\s`]*$", "", s).strip()
            if s.startswith("``") and s.endswith("``") and len(s) > 4:
                s = s[2:-2].strip()
            elif s.startswith("`") and s.endswith("`") and len(s) > 2:
                s = s[1:-1].strip()
        s = re.sub(r"[\s]*\((?:dosyada|grep|Grep)[^)]*\)\s*$", "", s.replace("`", "")).strip()
        if not s:
            res.append("ESKİ boş")
            continue
        hits = {n: t.replace("`", "").count(s) for n, t in docs_text().items() if s in t.replace("`", "")}
        tgt = field(block, "Hedef dosya:")
        pref = set(re.findall(r"Docs/(\w+?)_", tgt)) | set(re.findall(r"\b(\d\d)\b", tgt))
        sel = {n: c for n, c in hits.items() if any(n.startswith(p) for p in pref)} if pref else hits
        if "PRODUCT_DISCOVERY" in tgt or "tracker" in tgt.lower() or "karar kaydı" in tgt.lower():
            sel = {n: c for n, c in hits.items() if n.startswith("PRODUCT")}
        if sum((sel or hits).values()) != 1:
            res.append(f"ESKİ {hits or 0}: {s[:70]}")
    return res


def main():
    gov, srcs = parse_hukum()
    ver = {"DOĞRU": [], "KISMEN": [], "ÇÜRÜTÜLDÜ": []}
    for kid, (n, v, b) in gov.items():
        key = "ÇÜRÜTÜLDÜ" if v.startswith("ÇÜRÜTÜLDÜ") else ("KISMEN" if v.startswith("KISMEN") else "DOĞRU")
        ver[key].append(kid)
    changed = []
    for kid, (n, v, b) in gov.items():
        if len(srcs[kid]) > 1:
            changed.append(kid)
    out = ["# Checkpoint sonucu (C-a) — Aşama 8 Kodlama Kılavuzu, K-437'nin 4. adımı\n",
           "Tarih: 2026-10-30 · taban: `main` 43f9d1a (çakışma taraması PR #224) · girdi sürümleri: karar kaydı v0.221 · `09` v0.23 · `02` v0.103 · `05` v0.40 · `06` v0.42 · `07` v0.32 · `08` v0.18 · `10` v0.58 · `11` ve `12` şablon v0.1 (park blokları) · `DEPLOY_RUNBOOK`.\n",
           "Bu dosya C-b'nin girdisidir: her doğrulanmış bulgu hüküm dosyasındaki ESKİ → YENİ şartnamesiyle aşağıda durur; ESKİ metin hedef dosyada tam bir kez geçer (betikle sayıldı, bkz. \"ESKİ denetimi\"). Hiçbir doküman bu turda değiştirilmedi.\n",
           "\n## 1. Mercek tablosu\n",
           "| Mercek | Kapsam | Ham bulgu |\n|---|---|---|\n"]
    tot = 0
    for lid, desc, n in LENSES:
        out.append(f"| {lid} | {desc} | {n} |\n")
        tot += n
    out.append(f"| (betik) | sürüm kontrolü, K tekilliği, etki sütunu ↔ hedef, ad adayları (`scripts/doc-scans/08_checkpoint_paketle.py`) | — |\n")
    out.append(f"\nSonnet / low, ön planda, beşli dalga (5 + 5 + 1); ham bulgu toplamı {tot}; ayrıca bir doğrulayıcının gözlemi (V-1) → **{tot + 1} ayrı bulgu**.\n")
    out.append("\n## 2. Doğrulayıcı tablosu (Opus / high; hükümler `hukum_<n>.md`)\n")
    out.append("| # | Kademe | Bulgular |\n|---|---|---|\n")
    for ln in (D / "gruplar.txt").read_text(encoding="utf-8").split("\n"):
        p = ln.split("\t")
        if len(p) == 3:
            out.append(f"| {p[0]} | {p[1]} | {p[2].replace('Kontrol', '(ayıklandı: bulgu değil)')} |\n")
    out.append("| 12 | orta (yeniden) | M5-3, S2-6, S3-10 — kademe düşükten orta'ya yükselen bulgular |\n")
    out.append("| 13 | orta (yeniden) | S5-04, S6-2 — kademe düşükten orta'ya yükselen bulgular |\n")
    out.append("| 14 → 15 | düşük → orta (yeniden) | V-1 (`09 §9.4` madde 5 ↔ `§7.2`; bir doğrulayıcının gözlemi, 14 düşük kademede, 15 orta kademede; 15 geçerli) |\n")
    out.append("\n**Toplam 15 doğrulayıcı (sınır 20).** Yeniden doğrulama borcu yok: kademesi yükselen beş bulgu ve V-1 yeni kademede yeniden doğrulandı; ikinci geçişte düşen ya da sabit kalanların hükmü geçerlidir.\n")
    out.append(f"\n## 3. Sayım\n\n- Ayrı bulgu: {len(gov)}; **doğru {len(ver['DOĞRU'])}**, **kısmen doğru {len(ver['KISMEN'])}**, **çürütülen {len(ver['ÇÜRÜTÜLDÜ'])}**.\n")
    out.append(f"- Doğrulanan (doğru + kısmen): {len(ver['DOĞRU']) + len(ver['KISMEN'])}.\n")
    out.append("- Doğru: " + ", ".join(sorted(ver['DOĞRU'])) + "\n- Kısmen: " + ", ".join(sorted(ver['KISMEN'])) + "\n- Çürütülen: " + ", ".join(sorted(ver['ÇÜRÜTÜLDÜ'])) + "\n")
    out.append("\n## 4. Doğrulanmış bulgular ve şartnameleri (geçerli hüküm: son geçiş)\n")
    warn, crit, newk, lev = [], [], [], []
    problems = []
    for kid in sorted(gov, key=lambda k: (k[0], int(re.sub(r"\D", "", k) or 0), k)):
        n, v, b = gov[kid]
        if v.startswith("ÇÜRÜTÜLDÜ"):
            continue
        out.append(f"\n<!-- hüküm: hukum_{n}.md -->\n" + b)
        for pr in eski_check(b):
            problems.append(f"{kid}: {pr}")
        w = field(b, r"⚠:")
        if w and not w.lower().startswith("hayır"):
            warn.append((kid, w))
        c = field(b, r"Çok kritik:")
        if c and not c.lower().startswith("hayır"):
            crit.append((kid, c))
        k = field(b, r"Yeni karar gerekir mi:")
        if k and not k.lower().startswith("hayır"):
            newk.append((kid, k))
        lv = field(b, r"Seviye:")
        lev.append((kid, lv, srcs[kid]))
    out.append("\n## 5. Çürütülenler\n")
    for kid in sorted(ver["ÇÜRÜTÜLDÜ"]):
        n, v, b = gov[kid]
        g = re.search(r"(?s)Gerekçe:(.*?)(?:\n(?:Yeni karar|⚠|Çok kritik|Hedef)|\Z)", b)
        out.append(f"- **{kid}** (hukum_{n}.md): {(g.group(1).strip() if g else '')[:600]}\n")
    out.append("\n## 6. Seviye değişenler\n\nİlk geçişte düşük kademede doğrulanıp ikinci geçişte orta'ya yükselen bulgular (kademe: orta'da yeniden doğrulandı, hüküm 12/13/15): M5-3, S3-10, S2-6 (ilk geçişte orta, ikinci geçişte düşük), S5-04 ve S6-2 (ikinci geçişte düşük), V-1 (orta). Mercek seviyesinden değişenler geçerli hükümlerin `Seviye:` satırındadır (M4-1 orta → düşük, S1-05 orta → düşük, S5-02 orta → düşük, S2-6 düşük → orta → düşük, M5-3 düşük → orta, S3-10 düşük → orta).\n")
    out.append("\n**Yeniden doğrulama borcu: yok.**\n")
    out.append("\n## 7. ⚠ adayları (para, yasal hak, güvenlik güvencesi, geri dönüşü zor)\n")
    for kid, w in warn:
        out.append(f"- **{kid}**: {w}\n")
    if not warn:
        out.append("(yok)\n")
    out.append("\n## 8. Çok kritik (proje sahibinin kararı olmadan yazılamaz)\n")
    for kid, c in crit:
        out.append(f"- **{kid}**: {c}\n")
    if not crit:
        out.append("**Yok.** Hiçbir doğrulayıcı proje sahibinin kararı olmadan yazılamayacak bir konu bildirmedi.\n")
    out.append("\n## 9. Yeni karar satırı gerektirenler\n")
    for kid, k in newk:
        out.append(f"- **{kid}**: {k}\n")
    out.append("\n## 10. ESKİ denetimi (betik: her ESKİ hedef dosyada tam bir kez)\n")
    if problems:
        for pr in problems:
            out.append(f"- {pr}\n")
    else:
        out.append("Sorun yok.\n")
    out.append("\n## 11. Seviye ve geçerli hüküm kaynağı\n\n| Bulgu | Seviye | Hüküm dosyaları (sonuncusu geçerli) |\n|---|---|---|\n")
    for kid, lv, s in lev:
        out.append(f"| {kid} | {lv} | {', '.join('hukum_' + str(x) for x in s)} |\n")
    (D / "sonuc.md").write_text("".join(out), encoding="utf-8")
    print(f"gov={len(gov)} dogru={len(ver['DOĞRU'])} kismen={len(ver['KISMEN'])} curutulen={len(ver['ÇÜRÜTÜLDÜ'])}")
    print("warn:", [k for k, _ in warn])
    print("crit:", [k for k, _ in crit])
    print("newk:", [k for k, _ in newk])
    print("problems:", problems)


main()
