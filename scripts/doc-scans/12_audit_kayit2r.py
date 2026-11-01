"""Asama 10 -- 12 audit-2r Parca B kaydi: surum artislari, K-1555, S24.1, rapor, README, MEMORY, ust dokuman geri besleme.

Her degisiklik (dosya, ESKI, YENI): ESKI dosyada tam bir kez gecmeli; yoksa hicbir sey yazilmaz.
Kullanim: python -I scripts/doc-scans/12_audit_kayit2r.py [--kuru]
"""
import sys
from pathlib import Path

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")

R = Path(r"C:\projects\Shopfolio")
DOCS = R / "Docs"
V12 = DOCS / "12_VALIDATION_PROTOCOL.md"
V05 = DOCS / "05_TECHNICAL_ARCHITECTURE.md"
V10 = DOCS / "10_MVP_SCOPE.md"
ST = DOCS / "PRODUCT_DISCOVERY_STATUS.md"
RP = DOCS / "AUDIT_REPORTS" / "12_AUDIT.md"
RD = DOCS / "AUDIT_REPORTS" / "README.md"
MEM = R / ".claude" / "memory" / "MEMORY.md"
kuru = "--kuru" in sys.argv

C = []


def ekle(yol, eski, yeni):
    C.append((yol, eski, yeni))


GB05 = ("v0.47 Aşama 10 — Doğrulama Protokolü kalite döngüsünün audit-2r doğrulamasının geri beslemesi: K-1555 — §7.4.4 "
        "(ayıp talebinin on yılı durumunun son değiştiği andan işler; hiç değişmemişse açıldığı andan — `02 §4.2` Z-29, K-1092 (1)); "
        "✓ korunur, kalite döngüsü yeniden açılmaz · ")
GB10 = ("v0.62 Aşama 10 — Doğrulama Protokolü kalite döngüsünün audit-2r doğrulamasının geri beslemesi: K-1555 — KP-27 "
        "(Kaynak hücresine yeniden doğrulama kuralının evi `02 §8.1.5` eklendi); ✓ korunur, kalite döngüsü yeniden açılmaz · ")

# --- 05, 10 ---
ekle(V05, "**Versiyon: v0.46** |", "**Versiyon: v0.47** |")
ekle(V05, "*Shopfolio — Technical Architecture v0.46 — ✓ Tamamlandı (",
     "*Shopfolio — Technical Architecture v0.47 — ✓ Tamamlandı (" + GB05)
ekle(V05, "ayıp talebi — durumunun son değiştiği andan on yıl sonra, siparişin imha parçasından önce olmamak üzere (Z-29; K-1092)",
     "ayıp talebi — durumunun son değiştiği andan — hiç değişmemişse açıldığı andan — on yıl sonra, siparişin imha parçasından önce olmamak üzere (Z-29; K-1092)")
ekle(V10, "**Versiyon: v0.61** |", "**Versiyon: v0.62** |")
ekle(V10, "*Shopfolio — MVP Scope v0.61 (", "*Shopfolio — MVP Scope v0.62 (" + GB10)
ekle(V10, "§3.14, §8.2.6, §11 P-47", "§3.14, §8.1.5, §8.2.6, §11 P-47")

# --- 12 ---
ekle(V12, "**Versiyon: v0.19** |", "**Versiyon: v0.20** |")
ekle(V12, "*Shopfolio — Validation Protocol v0.19*", "*Shopfolio — Validation Protocol v0.20*")
ekle(V12, "sıradaki kalite döngüsünün audit-2r adımıdır.**",
     "audit-2r işlendi (v0.20; K-1555: 12 madde doğrulandı, 11 uygulandı, 0 yükseltildi); audit tamamlandı; sıradaki kalite döngüsünün adımı deep review'dur.**")

# --- tracker ---
ekle(ST, "**Versiyon: v0.314** |", "**Versiyon: v0.315** |")
ekle(ST, "| 12 | Validation Protocol | ⏳ | v0.19 (audit-2c işlendi:",
     "| 12 | Validation Protocol | ⏳ | v0.20 (audit-2r işlendi: yeniden doğrulama + 2a'nın yeni YAN notları, 12 madde (10 doğru · 1 kısmen · 1 çürük), "
     "11 uygulandı (7 ESKİ → YENİ çifti + 21 hücre + 3 yeni satır: VAL-A977 `[güvenlik]` üye bağlantısı, VAL-A978, VAL-A979 birleşmede stok/sınır), yükseltilen yok; "
     "VAL-A070 ve VAL-A736 ORTA → KRİTİK; matris 1.476 satır, KRİTİK 614 · ORTA 730 · DÜŞÜK 132; K-1555; audit tamamlandı, sıradaki deep review) · v0.19 (audit-2c işlendi:")

K1555 = (
    "| K-1555 | 2026-11-01 | Kalite döngüsü — audit yeniden doğrulama 2r, audit'in son adımı (`12` v0.19 → v0.20; `05` v0.46 → v0.47; "
    "`10` v0.61 → v0.62; rapor `Docs/AUDIT_REPORTS/12_AUDIT.md` \"Doğrulanmamış bulgular — KD-audit-2r\") **(öneriyle kaydedildi)** | "
    "**(1) Koşum:** 4 Opus/high doğrulayıcı (hukum_1…4; ön planda); 12 madde: yeniden doğrulanan 4 (Y21, #95, Y1, #88 — hepsi Orta kademesinde) "
    "ve 2a'nın yeni YAN notları 8 (D094, A070, C052, birleşmede stok/sınır, A278/A280 Kaynak, A600, bölüm girişi K-1319, A102/C028/A103 atfı); "
    "**10 doğru · 1 kısmen (Z8) · 1 çürük (Z3: VAL-A902 zaten karşılıyor)**. Hiçbir madde yeniden yükselmedi. "
    "**(2) Uygulama:** 11 madde `12`'ye uygulandı (`12_audit_uygula2.py`: 7 ESKİ → YENİ çifti, 21 hücre, 3 yeni satır). "
    "VAL-A070 ve VAL-A736 ORTA → KRİTİK (para/yasal süre ve ziyaretçinin kişisel verisi; `12 §4.2` (6)); VAL-A823 beklenen sonucu ve Kaynak'ı `05 §4.3.3`, `§4.6.1` ile hizalandı "
    "(yeniden gelen satır isteği yeniden göndermez, sonucu sorar); VAL-A081 kuralı ürünün davranışına çekildi (gönderim tarihi tutulmaz, bir yıl üst sınırı). "
    "**(3) Y1 kararı (VAL-B016):** iki yoldan **yol (2)** seçildi — üyenin sipariş e-postası bağlantısının girişsiz açılmaması ve hesap silindikten sonra eski hesap adresinin açılmaması "
    "yeni `[güvenlik]` KRİTİK **VAL-A977**'ye (§4.5, VAL-A054'ün ardında) taşındı; VAL-B016 misafir/silinmiş hesap anahtarlı akışı olarak ORTA kaldı (`12 §3.3`: `[güvenlik]` B satırına konmaz; `§4.2` (1): olumsuz hâl kendi satırıdır). "
    "Yol (1) B satırına etiket koyamadığı ve olumsuz parçayı olumlu akışın içinde bıraktığı için seçilmedi. Birleşmede ürün sınırı aşımı (`02 §3.16.10`) VAL-A978'e, sonradan aşılan stok/sınır (`02 §3.16.8`, `§3.16.10`) VAL-A979'a bağlandı. "
    "Matris: 1.473 → 1.476 satır; KRİTİK 611 → 614; `[güvenlik]` 38 → 39; `yok` 1.223 → 1.226. "
    "**(4) Üst doküman (K-652):** `05 §7.4.4` ayıp talebinin on yılı \"hiç değişmemişse açıldığı andan\" kolunu yazdı; `10` KP-27 Kaynak hücresine `02 §8.1.5` eklendi. "
    "**(5) Audit tamamlandı:** 105 maddenin tamamı hükme bağlandı; kalan yeni YAN notları (VAL-A080/C012/C015/A072 Kaynak `02 §7.4`; VAL-A068/A078 ORTA ↔ A238/A069 KRİTİK; birleşmede stok kolunun kaynakta belirsizliği) "
    "deep review'a girdidir. | **Gerekçe:** proje sahibinin kararı (2026-10-31): her madde doğrulanır, örnekleme ya da atlama yoktur. Kaynak ile `12` çeliştiğinde L1/L2 kazanır. ⚠ karar yok. "
    "| `05` v0.47 · `10` v0.62 · `12` v0.20 |\n"
)
ekle(ST, "| `05` v0.46 · `10` v0.61 · `12` v0.19 |\n", "| `05` v0.46 · `10` v0.61 · `12` v0.19 |\n" + K1555)

ekle(ST, "sıradaki adım 2r. Rapor: `Docs/AUDIT_REPORTS/12_AUDIT.md`. Açık soru yok; ⚠ karar yok.\n",
     "sıradaki adım 2r. Rapor: `Docs/AUDIT_REPORTS/12_AUDIT.md`. Açık soru yok; ⚠ karar yok.\n"
     "- [x] **Kalite döngüsü — audit yeniden doğrulama 2r — tamamlandı (2026-11-01, `12` v0.20, K-1555); audit tamamlandı.** 2r: **4 Opus/high doğrulayıcı, "
     "12 madde: 10 doğru · 1 kısmen · 1 çürük**; 11 madde uygulandı (7 şartname çifti + 21 hücre + 3 yeni satır); VAL-A070 ve VAL-A736 KRİTİK oldu; "
     "Y1 kararı yol (2): yeni `[güvenlik]` KRİTİK VAL-A977; yeniden yükselen yok. Sıradaki adım: `12` deep review. "
     "Rapor: `Docs/AUDIT_REPORTS/12_AUDIT.md`. Açık soru yok; ⚠ karar yok.\n")

# --- rapor ---
RAPOR = """## Doğrulanmamış bulgular — KD-audit-2r

**Tarih:** 2026-11-01 · **Girdi:** 2a ve 2b'de Düşük → Orta yükseltilen 4 madde (Y21, #95, Y1, #88) ve 2a hükümlerindeki doğrulanmamış yeni YAN notları (8 madde; 2b'nin açıkça 2r'ye bıraktığı üç not dahil). 2c'den yükselen yok.

### Doğrulayıcılar (4; hepsi `[dogrulama]` Opus / high, ön planda)

| k | Kademe | Maddeler |
|---|---|---|
| 1 | Orta + ona bağlı Düşük | #88, Z1 (VAL-D094 on yılı) |
| 2 | Orta (iki bölüm) | Y21, #95 |
| 3 | Orta + Orta aday | Y1 (VAL-B016), Z2 (VAL-A070 KRİTİK adayı) |
| 4 | Düşük (6 madde) | Z3 (VAL-C052), Z4 (birleşmede stok/sınır), Z5 (A278/A280 Kaynak), Z6 (VAL-A600), Z7 (bölüm girişi K-1319), Z8 (A102/C028/A103 atfı) |

Orta kademede Y21 ve #95 farklı bölümlerden olduğu için çift olarak gruplandı (4 doğrulayıcı sınırı). `12` v0.19'da zaten karşılananlar (VAL-A278/A280 paydası, VAL-A354…A357 atfı) betik ve `grep` ile ayıklandı; Z3 doğrulayıcıca da çürük çıktı.

**Toplam hüküm: 12 madde — 10 doğru · 1 kısmen (Z8) · 1 çürük (Z3).** Yeniden yükselen yok.

### Uygulama

11 madde uygulandı: **7 ESKİ → YENİ çifti**, **21 hücre düzeltmesi** ve **3 yeni satır** (`12_audit_uygula2.py`; uygulanan liste `a10-kd-audit-2/2r/uygulanan.md`). Z-kimlikleri betik başlığına eklendi.

- VAL-A070 ve VAL-A736 ORTA → KRİTİK (A736 kanıt türü "Uç yanıtı + Veri kaydı"; Kaynak ve ön koşulu talep ayrıntısını anar).
- VAL-A823 beklenen sonuç ve Kaynak `05 §4.3.3`, `§4.6.1`, `§4.6.2` ile hizalandı; VAL-A081 kural, ön koşul, beklenen ve Kaynak ürünün davranışına (gönderim tarihi tutulmaz, bir yıl üst sınırı, panel kaydı) çekildi.
- VAL-D094 ve VAL-D092: durumu hiç değişmemiş / hiç kapatılmamış talep eklendi (#88 ve Z1 tek çiftte).
- Y1 kararı (öneriyle): VAL-B016 ORTA kaldı; üye bağlantısı yeni `[güvenlik]` KRİTİK **VAL-A977**; birleşmede ürün sınırı aşımı **VAL-A978**, sonradan aşılan stok/sınır **VAL-A979** (ORTA).
- Z5: VAL-A277, A278, A280, A282, A283 Kaynak hücreleri `02 §10.6.3`/`§6.2.16` ile; Z6: VAL-A600 beklenen; Z7: bölüm girişi (`05 §9.1.3`, `07 §6.1.4`); Z8: VAL-A102, C028, A103 Kaynak.

**Matris sayımı (betik):** 1.476 satır (+3); Seviye A 979; Severity KRİTİK 614 · ORTA 730 · DÜŞÜK 132; `[güvenlik]` 39; Kanıt ortamı `yok` 1.226; §4.4 56, §4.5 19 satır.

### Üst dokümana geri besleme (K-652)

`05` v0.47 (§7.4.4: ayıp talebinin on yılı hiç değişmemişse açıldığı andan), `10` v0.62 (KP-27 Kaynak hücresine `02 §8.1.5`).

### Yükseltilenler

Yok.

### Toplam ilerleme — audit tamamlandı

**105 maddenin tamamı hükme bağlandı** (2a 31 · 2b 46 · 2c 28), yeniden doğrulama 2r ile kapandı (12 madde). **Audit tamamlandı; sıradaki adım deep review.** Kalan (doğrulanmamış, deep review girdisi) yeni YAN notları: VAL-A080, C012, C015, A072 Kaynak hücresi `02 §7.4` yerine `02 §7.3.1` (Düşük); VAL-A068/A078 (IBAN'ın geri ödemeye bağlanması, ORTA) ↔ A238/A069 (KRİTİK); birleşmede stok kolunun sonucu kaynakta "eklenmez mi, mesajla kalır mı" belirsiz (`02 §3.16.8`); 2b/2c hükümlerindeki öteki "ayrı madde" notları. Kapılar: `12_kapsam_hizasi.py` 0 · `12_matris_denetle.py --kapsam 1-77` hata 0 (1.476 satır, 77/77 KP) · `12_atif_denetle.py` hata 0.

"""
ekle(RP, "Prosedür: [`.claude/skills/audit/SKILL.md`]",
     RAPOR + "Prosedür: [`.claude/skills/audit/SKILL.md`]")

ekle(RD, "geri besleme; sıradaki 2r.\n",
     "geri besleme; sıradaki 2r. KD-audit-2r: `12` v0.20, 12 madde doğrulandı (10 doğru · 1 kısmen · 1 çürük), 11 uygulandı (3 yeni satır), yükselen yok; "
     "**audit tamamlandı; sıradaki deep review**; `05` v0.47, `10` v0.62 geri besleme.\n")

# --- MEMORY ---
ekle(MEM, "- **SIRADA — Aşama 10 kalite döngüsü, audit-2r (`12` v0.19, karar kaydı v0.314, K-1554; dal `docs/asama10-kd-audit-2c`): audit-2c tamam —",
     "- **SIRADA — Aşama 10 kalite döngüsü, deep review (`12` v0.20, karar kaydı v0.315, K-1555; dal `docs/asama10-kd-audit-2r`): audit tamamlandı — "
     "audit-2r: 4 Opus/high doğrulayıcı, 12 madde 10 doğru · 1 kısmen · 1 çürük, 11 uygulandı (3 yeni satır: VAL-A977 `[güvenlik]` üye bağlantısı, A978, A979); "
     "VAL-A070/A736 KRİTİK; Y1 kararı yol (2); `05` v0.47, `10` v0.62 hizalandı; matris 1.476 satır; ⚠ yok. "
     "Önceki: audit-2r (`12` v0.19, karar kaydı v0.314, K-1554; dal `docs/asama10-kd-audit-2c`): audit-2c tamam —")

for yol, e, y in C:
    t = yol.read_bytes().decode("utf-8")
    n = t.replace("\r\n", "\n").count(e)
    if n != 1:
        print("SORUN %s: %d kez: %.70s" % (yol.name, n, e))
        sys.exit(1)
if kuru:
    print("kuru: tamam")
    sys.exit(0)
for yol, e, y in C:
    raw = yol.read_bytes().decode("utf-8")
    crlf = "\r\n" in raw
    t = raw.replace("\r\n", "\n").replace(e, y, 1)
    if crlf:
        t = t.replace("\n", "\r\n")
    yol.write_bytes(t.encode("utf-8"))
print("yazildi", len(C))
