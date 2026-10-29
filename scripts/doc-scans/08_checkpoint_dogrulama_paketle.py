"""Groups checkpoint lens findings (mercek_*.md) into verifier packages (dogrulama_<n>.md) by tier.

Usage: python -I 08_checkpoint_dogrulama_paketle.py <cp8_dir>

Tiers (INSTRUCTIONS section 9 (1)): high = one verifier each, medium = groups of 2-4, low = groups of 5-8.
Findings are grouped within the same lens file first, remaining ones are merged across lenses.
"""
import re
import sys
from pathlib import Path

sys.stdout.reconfigure(encoding="utf-8")
D = Path(sys.argv[1])

INSTR = """# Bulgu doğrulama — paket {n} ({tier})

Repo: C:\\projects\\Shopfolio (salt okunur; hiçbir dokümanı değiştirme). Aşağıdaki bulgular bir mercekten geldi ve **henüz doğrulanmadı**; mercek yanılmış olabilir.
Her bulgu için bağımsız karar ver. Kuralların: yalnız kendi okuduğunla hüküm ver; mercek alıntısına güvenme, alıntıları hedef dosyada `Grep` ile bul
(`Docs/09_CODING_GUIDELINES.md` ~470 KB, `05`/`06`/`07`/`02` büyük: bütün okunmaz; `Grep -n` ve aralıklı `Read` kullan; karar kaydı `Docs/PRODUCT_DISCOVERY_STATUS.md`
~3.4 MB: yalnız `Grep` ile ilgili `K-NNNN` satırı). Heredoc (`<<`) yok. Karar kaydı ve geçmiş kararlar bağlayıcıdır: bulgu bilinçli bir karara aykırı bir "düzeltme" öneriyorsa ÇÜRÜTÜLDÜ.

Her bulgu için `{out}` dosyasına şu biçimde ekle (bulduğun anda, sona bırakma):
`### <kimlik> — HÜKÜM: DOĞRU | KISMEN DOĞRU | ÇÜRÜTÜLDÜ` · `Seviye: yüksek|orta|düşük` (merceğin seviyesini sen belirle; orta = iki yer çelişiyor ya da kapalı liste/sayı/hedef yanlış
ya da kuralın evinde kural eksik; düşük = ifade/kapsam/atıf farkı) · `Kanıt:` hedefteki birebir alıntılar (dosya:satır) · `Gerekçe:` bir-iki cümle ·
DOĞRU/KISMEN ise `Hedef dosya:` · `ESKİ:` (hedef dosyada **tam bir kez** geçen birebir metin; `Grep -c` ile say, birden çok ise uzat) · `YENİ:` (birebir değiştirme metni, Türkçe, karar kaydı K numarası
eklenecekse "K-?" yaz) · `Yeni karar gerekir mi:` hayır | evet (neden) · `⚠:` para / yasal hak / güvenlik güvencesi / geri dönüşü zor ise "evet — bir sade cümle", değilse "hayır" ·
`Çok kritik:` proje sahibinin kararı olmadan yazılamıyorsa "evet — soru", değilse "hayır".
Düzeltme önerisi yeni bir kural uydurmamalı; yalnız var olan kararların yansıması olmalı. Mercek önerisini beğenmezsen daha doğrusunu yaz.
Sonda tek satır: doğru/kısmen/çürütülen sayısı.

---

"""


def parse(f):
    t = f.read_text(encoding="utf-8")
    parts = re.split(r"(?m)^### ", t)
    out = []
    for p in parts[1:]:
        head = p.split("\n", 1)[0]
        kid = head.split(" ")[0]
        body = "### " + p.rstrip() + "\n"
        m = re.search(r"\b(yüksek|orta|düşük)\b", p[:600], re.I)
        lv = m.group(1).lower() if m else "orta"
        out.append((kid, lv, body))
    return out


def chunk(items, lo, hi):
    """split items into chunks of size lo..hi, as even as possible."""
    n = len(items)
    if n == 0:
        return []
    k = -(-n // hi)
    size = -(-n // k)
    return [items[i:i + size] for i in range(0, n, size)]


def redo(ids, start):
    """Re-verification of risen findings at the medium tier: groups of 2-4 numbered from start."""
    allf = []
    for f in sorted(D.glob("mercek_*.md")):
        for kid, lv, body in parse(f):
            if kid in ids:
                allf.append((f.stem, kid, "orta", body))
    n = start - 1
    for c in chunk(allf, 2, 4):
        n += 1
        out = D / f"hukum_{n}.md"
        txt = INSTR.format(n=n, tier="orta, yeniden doğrulama", out=str(out))
        txt += "\n".join(b for _, _, _, b in c)
        (D / f"dogrulama_{n}.md").write_text(txt, encoding="utf-8")
        print(n, [k for _, k, _, _ in c])


def main():
    if len(sys.argv) > 3 and sys.argv[2] == "--ids":
        redo(set(sys.argv[3].split(",")), int(sys.argv[4]))
        return
    allf = []
    for f in sorted(D.glob("mercek_*.md")):
        for kid, lv, body in parse(f):
            allf.append((f.stem, kid, lv, body))
    high = [x for x in allf if x[2] == "yüksek"]
    med = [x for x in allf if x[2] == "orta"]
    low = [x for x in allf if x[2] == "düşük"]
    for old in D.glob("dogrulama_*.md"):
        old.unlink()
    n = 0
    groups = []
    for x in high:
        n += 1
        groups.append((n, "yüksek", [x]))
    for c in chunk(med, 2, 4):
        n += 1
        groups.append((n, "orta", c))
    for c in chunk(low, 5, 8):
        n += 1
        groups.append((n, "düşük", c))
    lines = []
    for n, tier, items in groups:
        out = D / f"hukum_{n}.md"
        txt = INSTR.format(n=n, tier=tier, out=str(out))
        txt += "\n".join(b for _, _, _, b in items)
        (D / f"dogrulama_{n}.md").write_text(txt, encoding="utf-8")
        lines.append(f"{n}\t{tier}\t{','.join(k for _, k, _, _ in items)}")
    print(f"toplam bulgu {len(allf)} (yüksek {len(high)}, orta {len(med)}, düşük {len(low)}); doğrulayıcı {len(groups)}")
    print("\n".join(lines))
    (D / "gruplar.txt").write_text("\n".join(lines), encoding="utf-8")


main()
