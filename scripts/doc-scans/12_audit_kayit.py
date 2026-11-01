"""Asama 10 -- 12 audit Parca B kaydi: surum artislari, durum tablosu, K-1549/K-1550, S24.1, README, MEMORY.

Her degisiklik (dosya, ESKI, YENI): ESKI dosyada tam bir kez gecmeli; yoksa hicbir sey yazilmaz.
Kullanim: python -I scripts/doc-scans/12_audit_kayit.py [--kuru]
"""
import sys
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

R = Path(r"C:\projects\Shopfolio")
DOCS = R / "Docs"
ST = DOCS / "PRODUCT_DISCOVERY_STATUS.md"
MEM = R / ".claude" / "memory" / "MEMORY.md"

NOT = "Aşama 10 — Doğrulama Protokolü kalite döngüsünün audit'inin geri beslemesi: K-1550"

C = []  # (yol, eski, yeni)


def ekle(yol, eski, yeni):
    C.append((yol, eski, yeni))


# --- 02
d02 = DOCS / "02_PRODUCT_REQUIREMENTS.md"
ekle(d02, "**Versiyon: v0.108**", "**Versiyon: v0.109**")
ekle(d02, "*Shopfolio — Product Requirements v0.108 (v0.108 Aşama 9",
     "*Shopfolio — Product Requirements v0.109 (v0.109 " + NOT + " — §2.1 Sipariş takibi satırı (erişim anahtarlı bağlantı misafir alıcıda ve hesabı silinmiş müşteride; üyenin bağlantısı girişsiz açılmaz — §3.22.5'e hizalandı, yeni kural yok); ✓ korunur, kalite döngüsü yeniden açılmaz · v0.108 Aşama 9")
ekle(ST, "| 02 | Product Requirements | ✓ | v0.108 (Aşama 9",
     "| 02 | Product Requirements | ✓ | v0.109 (" + NOT + " — §2.1 Sipariş takibi satırı) · v0.108 (Aşama 9")
# --- 05
d05 = DOCS / "05_TECHNICAL_ARCHITECTURE.md"
ekle(d05, "**Versiyon: v0.43**", "**Versiyon: v0.44**")
ekle(d05, "*Shopfolio — Technical Architecture v0.43 — ✓ Tamamlandı (v0.43 Aşama 9",
     "*Shopfolio — Technical Architecture v0.44 — ✓ Tamamlandı (v0.44 " + NOT + " — §10.1.6 Veritabanı satırı (canlılık ucu 503 verir, durum ucu okunamayan koşul kaynağında 503 döner; `07 §4.33.1`, K-1328 (5)); ✓ korunur, kalite döngüsü yeniden açılmaz · v0.43 Aşama 9")
ekle(ST, "| 05 | Technical Architecture | ✓ | v0.43 (Aşama 9",
     "| 05 | Technical Architecture | ✓ | v0.44 (" + NOT + " — §10.1.6) · v0.43 (Aşama 9")
# --- 07
d07 = DOCS / "07_API_DESIGN.md"
ekle(d07, "**Versiyon: v0.38**", "**Versiyon: v0.39**")
ekle(d07, "*Shopfolio — API Design v0.38 — ✓ Tamamlandı (v0.38 Aşama 9",
     "*Shopfolio — API Design v0.39 — ✓ Tamamlandı (v0.39 " + NOT + " — §6.2.3 `OrderSummaryChanged` satırının fark listesine \"KDV oranı\" (§4.13.6'nın listesine hizalandı); ✓ korunur, kalite döngüsü yeniden açılmaz · v0.38 Aşama 9")
ekle(ST, "| 07 | API Design | ✓ | v0.38 (Aşama 9",
     "| 07 | API Design | ✓ | v0.39 (" + NOT + " — §6.2.3) · v0.38 (Aşama 9")
# --- 10
d10 = DOCS / "10_MVP_SCOPE.md"
ekle(d10, "**Versiyon: v0.59**", "**Versiyon: v0.60**")
ekle(d10, "*Shopfolio — MVP Scope v0.59 (v0.59 Aşama 9",
     "*Shopfolio — MVP Scope v0.60 (v0.60 " + NOT + " — KP-18 (erişim anahtarlı bağlantı misafir alıcıda ve hesabı silinmiş müşteride; üyenin bağlantısı girişsiz açılmaz — `02 §3.22.5`'e hizalandı); ✓ korunur, kalite döngüsü yeniden açılmaz · v0.59 Aşama 9")
ekle(ST, "| 10 | MVP Scope | ✓ | v0.59 (Aşama 9",
     "| 10 | MVP Scope | ✓ | v0.60 (" + NOT + " — KP-18) · v0.59 (Aşama 9")
# --- 12
d12 = DOCS / "12_VALIDATION_PROTOCOL.md"
ekle(d12, "**Versiyon: v0.15**", "**Versiyon: v0.16**")
ekle(d12, "*Shopfolio — Validation Protocol v0.14*", "*Shopfolio — Validation Protocol v0.16*")
ekle(d12, "yazım turu kapandı (v0.15); sıradaki kalite döngüsünün audit adımıdır.",
     "yazım turu kapandı (v0.15); kalite döngüsünün audit'i işlendi (v0.16; K-1549, K-1550; rapor `AUDIT_REPORTS/12_AUDIT.md`: 38 bulgu uygulandı, 82 doğrulanmadı); sıradaki kalite döngüsünün audit-2 adımıdır.")
ekle(ST, "| 12 | Validation Protocol | ⏳ | v0.15 (yazım turu kapandı",
     "| 12 | Validation Protocol | ⏳ | v0.16 (kalite döngüsü audit'i işlendi: 126 ham → 120 tekil bulgu, 20 doğrulayıcı, 38 hüküm (31 doğru · 7 kısmen), 94 şartname çifti + 41 hücre uygulandı, 82 bulgu doğrulanmadı; K-1549, K-1550; sıradaki audit-2) · v0.15 (yazım turu kapandı")

# --- karar kaydi
ekle(ST, "**Versiyon: v0.310**", "**Versiyon: v0.311**")

K1549 = ("| K-1549 | 2026-11-01 | `12` kalite döngüsü — audit (Aşama 10; rapor `Docs/AUDIT_REPORTS/12_AUDIT.md`; 35 paket, 22 KA + ÇR + SV + KD + TE + ÖL + İT mercekleri; tarama: L1–L5'te yeni kural gerekmedi, `skills/audit` ve Karar B uygulandı) **(öneriyle kaydedildi)** | "
         "**(1) Bulgular:** 126 ham bulgu → 120 tekil (Yüksek 24 · Orta 63 · Düşük 33); 20 Opus/high doğrulayıcı (14 Yüksek tek + 6 Orta 4'lü) 38 bulguya hüküm verdi: **31 doğru · 7 kısmen · 0 çürük**; yükselen yok; 6 bulgunun seviyesi düştü (#13, #16, #17 Yüksek → Orta; #24 Yüksek → Düşük; #32, #75 → Düşük). "
         "**(2) Uygulama:** 38 bulgunun hepsi `12`'ye işlendi: 94 ESKİ → YENİ çifti (`12_audit_uygula.py`; her ESKİ tam bir kez, sorun 0) ve Kaynak/Ön koşul/Beklenen hücresi tekrarlandığı için benzersiz ESKİ kurulamayan 41 hücre düzeltmesi (`12_audit_satir_uygula.py`; satır kimliği ve sütun hedefli); satır eklenmedi ya da silinmedi, §1 sayımları, park tablosu ve severity dağılımı (601 · 738 · 132) değişmedi. "
         "**(3) Doğrulanmayan 82 bulgu** (Yüksek 10 · Orta 39 · Düşük 33) ile hükümlerin 23 `YAN:` notu dokümana uygulanmadı; sonraki adım audit-2'nin girdisi (`dogrulanmadi.md`). Öncelik: #1 ve #2 (park P4.2 ve P4.6, yeni VAL satırı gerekir), #8…#10, #15, #4 ve #20 (severity yükseltme), #23, #56 (`10` KD-59 ↔ P-9 kaynak çelişkisi). "
         "**(4) Karar B kapandı:** `12_audit_paketle.py` yazıldı; ek betikler `12_audit_birlestir.py`, `12_audit_grupla.py`, `12_audit_hukum_denetle.py`, `12_audit_uygula.py`. "
         "**(5) Kapılar:** `12_kapsam_hizasi.py` hata 0 · `12_matris_denetle.py --kapsam 1-77` hata 0 (77/77) · `12_atif_denetle.py` hedefi bulunamayan 0. "
         "| **Gerekçe:** doğrulayıcı sınırı adım başına 20'dir (`INSTRUCTIONS §9` (8)); Yüksek tek, Orta 4'lü doğrulandı, yalnız doğrulanmış bulgu uygulandı. **Elenen:** (a) doğrulanmayan Yüksek 10 bulgunun doğrulamasız uygulanması — #1 ve #2 yeni satır açar, satır sayımı ve matris hizasını etkiler, doğrulanmadan yazılmaz; (b) hükümlerin `YAN:` notlarının (aynı bölüm kayması kalıbının başka yerleri) aynı PR'da uygulanması — yan yerler doğrulanmadı, audit-2'ye gitti. Yeni karar yoktur: ⚠ karar yok, \"yine de sorulur\" madde yok. "
         "| `12` v0.15 → v0.16 · `Docs/AUDIT_REPORTS/12_AUDIT.md`, `README.md` · `scripts/doc-scans/12_audit_*.py` · §24.1 · `.claude/memory/MEMORY.md` Güncel Durum |")
K1550 = ("| K-1550 | 2026-11-01 | `12` audit bulgularının üst dokümanlara geri beslemesi (K-652; bulgular #3, #24, #58; #16 ve #77 elendi; tarama: `02`–`12` genelinde \"herkes … bağlantı\", \"canlılık ve durum uçları\", `OrderSummaryChanged` ve \"özet farkı\" eşleşmeleri okundu) **(öneriyle kaydedildi)** | "
         "**Kaynak gerçekten yanlış ya da eksik olduğu için üç yerde düzeltildi; `12` tüketicidir, kaynak kazanır.** (1) `10` KP-18 ve `02` §2.1 Sipariş takibi satırı \"herkes sipariş e-postasındaki bağlantıyla\" diyordu; `02 §3.22.5` (K-1256) erişim anahtarlı bağlantıyı yalnız misafir alıcıya ve hesabı silinmiş müşteriye verir, üyenin bağlantısı girişsiz açılmaz → iki cümle `02 §3.22.5`'e hizalandı (`10` v0.59 → v0.60, `02` v0.108 → v0.109); yeni kural yok. "
         "(2) `05 §10.1.6` Veritabanı satırı \"canlılık ve durum uçları yanıt vermez\" diyordu; `07 §4.33.1` ve K-1328 (5) canlılığın 503 verdiğini, `05 §9.4.10` okunamayan koşul kaynağında durum ucunun 503 döndüğünü yazar → satır bunlara hizalandı (`05` v0.43 → v0.44). "
         "(3) `07 §6.2.3` `OrderSummaryChanged` satırının fark listesinde `07 §4.13.6`'nın saydığı \"KDV oranı\" yoktu → eklendi (`07` v0.38 → v0.39). "
         "**Elenen:** #16 — `07 §2.8.1` ve `§2.8.7` ters vekilin kendi 413'ünün görsel yükleme sayfasında nasıl görüneceğini yazmıyor; bu yazım hatası değil tasarım ve işletim boşluğudur (`DEPLOY_RUNBOOK` ile birlikte karar ister), yeni kural icat edilmedi, madde audit-2'nin girdisinde açık kaldı. #77 — K-946 (1)'in \"denenir\" cümlesi tarihsel kayıttır; `05 §4.3.4` ve `06` zaten \"yeniden denenir\" der, düzeltme gerekmez. "
         "| **Gerekçe:** K-652 ve checklist §6: üst dokümana dokunan bulgu aynı PR'da sürüm artışıyla geri beslenir, ✓ durumu korunur, kalite döngüsü yeniden açılmaz; kaynak kazanır. **Elenen:** #3'ü yalnız `12`'de düzeltmek — kaynak çelişkili kalır ve sonraki aşamalar yanlış cümleyi tüketirdi. "
         "| `10` v0.60 · `02` v0.109 · `05` v0.44 · `07` v0.39 · `12` v0.16 · `12_AUDIT.md` \"Üst dokümana geri besleme\" |")
ekle(ST, "\n| K-1548 | 2026-11-01 | `VP1-01`", "\n| K-1548 | 2026-11-01 | `VP1-01`")  # varlik sinamasi (degismez)

S241 = ("- [x] **Kalite döngüsü — audit — tamamlandı (2026-11-01, `12` v0.16, K-1549, K-1550).** Rapor: `Docs/AUDIT_REPORTS/12_AUDIT.md`. Betikler: `scripts/doc-scans/12_audit_paketle.py`, `12_audit_birlestir.py`, `12_audit_grupla.py`, `12_audit_hukum_denetle.py`, `12_audit_uygula.py`, `12_audit_satir_uygula.py`. 35 paket; **126 ham bulgu** (120 tekil: Yüksek 24 · Orta 63 · Düşük 33); **20 Opus/high doğrulayıcı** (14 Yüksek tek + 6 Orta 4'lü; sınır aşılmadı); **38 hüküm: 31 doğru · 7 kısmen · 0 çürük**; yükselen yok, 6 indirilen; **38 bulgu uygulandı** (94 şartname çifti + 41 hücre düzeltmesi; satır sayısı ve §1 sayımları değişmedi). Üst dokümana geri besleme (K-1550): `10` v0.60, `02` v0.109, `05` v0.44, `07` v0.39 — metin hizalaması, yeni kural yok. Kapılar: `12_kapsam_hizasi.py` 0 · `12_matris_denetle.py --kapsam 1-77` 0 · `12_atif_denetle.py` 0. **82 bulgu doğrulanmadı** (Yüksek 10 · Orta 39 · Düşük 33) ve hükümlerin 23 `YAN:` notu dokümana uygulanmadı — audit-2'nin girdisi (`dogrulanmadi.md`, yardımcı durum dizini); açık madde: `07 §2.8` ters vekil 413 (#16) ve `10` KD-59 ↔ P-9 (#56). **Sıradaki adım: audit-2 (doğrulanmamış bulguların doğrulanması), sonra özet ↔ ayrıntı taraması ve deep review.** Açık soru yok; ⚠ karar yok.")

# --- MEMORY
ekle(MEM, "- **SIRADA — Aşama 10 kalite döngüsü, audit adımı (`12`; yazım turu kapandı, 1.471 satır):** Y5 tamam",
     "- **SIRADA — Aşama 10 kalite döngüsü, audit-2 (`12` v0.16; dal `docs/asama10-kd-audit`):** audit tamam (karar kaydı v0.311, K-1549, K-1550; rapor `Docs/AUDIT_REPORTS/12_AUDIT.md`): 126 ham → 120 tekil bulgu, 20 Opus/high doğrulayıcı, 38 hüküm (31 doğru · 7 kısmen · 0 çürük), 94 şartname çifti + 41 hücre uygulandı; üst dokümana geri besleme `10` v0.60, `02` v0.109, `05` v0.44, `07` v0.39; **82 bulgu doğrulanmadı** (+23 YAN notu) → audit-2'nin girdisi (`~/.claude/helper-state/shopfolio/a10-kd-audit/dogrulanmadi.md`; öncelik #1, #2, #8…#10, #15, #4, #20, #23, #56); Karar B kapandı; kapılar hata 0; ⚠ yok. Önceki: Y5 tamam")
ekle(MEM, "Ürün Gereksinimleri (`02`) **v0.108**", "Ürün Gereksinimleri (`02`) **v0.109**")
ekle(MEM, "Teknik Mimari (`05`) **v0.43**", "Teknik Mimari (`05`) **v0.44**")
ekle(MEM, "MVP Kapsamı (`10`) **v0.59**", "MVP Kapsamı (`10`) **v0.60**")

# --- README
RD = DOCS / "AUDIT_REPORTS" / "README.md"

kuru = "--kuru" in sys.argv
metin = {}
sorun = []
for yol, eski, yeni in C:
    if yol not in metin:
        metin[yol] = yol.read_bytes().decode("utf-8")
    if eski == yeni:
        if metin[yol].count(eski) != 1:
            sorun.append("%s: varlik sinamasi %r" % (yol.name, eski[:40]))
        continue
    n = metin[yol].count(eski)
    if n != 1:
        sorun.append("%s: %d kez: %.70s" % (yol.name, n, eski))
        continue
    metin[yol] = metin[yol].replace(eski, yeni, 1)

# K satirlari: K-1548 satirindan sonra
st = metin[ST]
ls = st.split("\n")
ix = [i for i, l in enumerate(ls) if l.startswith("| K-1548 | ")]
if len(ix) != 1:
    sorun.append("K-1548 satiri %d" % len(ix))
elif any(l.startswith("| K-1549 | ") or l.startswith("| K-1550 | ") for l in ls):
    sorun.append("K-1549/K-1550 zaten var")
else:
    ls[ix[0] + 1:ix[0] + 1] = [K1549, K1550]
# S24.1: Y5 maddesinden sonra
iy = [i for i, l in enumerate(ls) if l.startswith("- [x] **Yazım Y5 — yazım turu kapandı")]
if len(iy) != 1:
    sorun.append("Y5 maddesi %d" % len(iy))
else:
    ls.insert(iy[0] + 1, S241)
# Karar B
ib = [i for i, l in enumerate(ls) if l.startswith("- [ ] **Karar B — kalite döngüsünün paketleyicisi**")]
if len(ib) != 1:
    sorun.append("Karar B %d" % len(ib))
else:
    ls[ib[0]] = ls[ib[0]].replace("- [ ] **Karar B", "- [x] **Karar B", 1).replace(
        "**Kapı:** kalite döngüsünün audit'i başlamadan paketleyici yazılmış olur.",
        "**Kapı:** kalite döngüsünün audit'i başlamadan paketleyici yazılmış olur. **Kapandı (2026-11-01, K-1549):** `scripts/doc-scans/12_audit_paketle.py` yazıldı ve audit'te koştu.")
metin[ST] = "\n".join(ls)

rd = RD.read_bytes().decode("utf-8")
SATIR = "- `12_AUDIT.md` — Doğrulama Protokolü (`12` v0.15 → v0.16), 2026-11-01: 38 mercek koşumu (35 paket), 20 doğrulayıcı, 126 ham bulgu, 38 hükme bağlandı (31 doğru · 7 kısmen · 0 çürük), 38 bulgu uygulandı (94 şartname çifti + 41 hücre), 82 doğrulanmadı (audit-2'nin girdisi); `10` v0.60, `02` v0.109, `05` v0.44, `07` v0.39 geri besleme."
if "12_AUDIT.md" in rd:
    sorun.append("README zaten 12_AUDIT.md anıyor")
else:
    lines = rd.split("\n")
    idx = [i for i, l in enumerate(lines) if l.startswith("- `11_DEEP_REVIEW.md`")]
    if len(idx) != 1:
        sorun.append("README 11_DEEP_REVIEW satiri %d" % len(idx))
    else:
        lines.insert(idx[0] + 1, SATIR)
        metin[RD] = "\n".join(lines)

print("degisiklik: %d; sorun: %d" % (len(C), len(sorun)))
for s in sorun:
    print(" -", s)
if sorun:
    sys.exit(1)
if kuru:
    sys.exit(0)
for yol, t in metin.items():
    yol.write_bytes(t.encode("utf-8"))
print("yazildi: %d dosya" % len(metin))
