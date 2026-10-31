# Audit Raporu — 11 Implementation Plan

**Tarih:** 2026-10-31 · **Hedef:** `Docs/11_IMPLEMENTATION_PLAN.md` v0.25 (bulgular v0.25'te üretildi, v0.26'da işlendi; taban `main` = 4805b2b) · **Bağlam:** `PRODUCT_DISCOVERY_STATUS.md` K-1401…K-1455; üst dokümanlar `02`–`10` · **Odak:** Tam denetim, betikle çıkarılmış dar kapsamlı paketlerle (`11` 693 KB, 214 task bloğu, 1.501 kaynak birimi; bütün okunmaz)

### Koşum biçimi (K-431, K-1232; `audit` skill'i, "Koşum biçimi")

Dokümanı yazan bağlamda koşmadı; adım baştan iki parçaya bölündü (`.claude/INSTRUCTIONS.md` §9 (8)). **Parça A (denetim):** paketleyici betik (`scripts/doc-scans/11_audit_paketle.py`) merceklere paket üretti, mercekler paketleri okudu, doğrulayıcılar bulguları hükme bağladı; her bulgunun ESKİ → YENİ şartnamesi dosyada tutuldu, dokümana yazılmadı. **Parça B (uygulama ve kayıt):** şartnameler tek betikle (`scripts/doc-scans/11_audit_uygula.py`) uygulandı, K-1455'in hükme kalan 41 maddesi karara bağlandı, üst dokümana dönen iki not geri beslendi, rapor ve karar kaydı yazıldı. Parça B bir işçide bağlam sınırına geldi ve devredildi; uygulama ikinci işçide tamamlandı.

- **Paketleyici betik.** Her mercek için satır numaralı dar paket (en çok 85.000 bayt; en büyüğü 84.993): kapsamdaki task blokları, `7.1`'de onlara bağlı kaynak birimlerin kaynak metni (birim başına 1.100 bayt kesik; kesilende mercek kaynağı açtı) ve ilgili K satırları. **İki yönlü sınama:** gerçek `11` → 214 blok, 1.501 kaynak birimi 1.501 çözüldü, 0 eksik; bilerek bozulmuş kopya (T05 başlığı, `02 §3.6.7` ve UC-81 kimliği, aday faz, `7.1`'de T999) → beş bozulmanın beşi de tanıda görüldü.
- **Mercekler ve paketler:** tabloda. Mercekler `[ozet]` Sonnet / low, ön planda, beşli dalga; toplam 67 mercek ajanı. Öneri mercek **OL** (ölçülebilirlik) ayrı koşmadı, **İT** ile birleştirildi (modül gruplu tam bloklar).
- **Kademeli doğrulama — 20 doğrulayıcı, hepsi `[dogrulama]` Opus / high, ön planda.** 12 Yüksek bulgu tek tek (D1…D12), 8 Orta grup dört bulgulu (D13…D20; modül/bölüm gruplu). Düşük bulgular Opus'a girmedi. **Adım başına 20 doğrulayıcı sınırı aşılmadı.** Sığmayan 435 bulgu doğrulanmadı ve uygulanmadı.
- **Uygulama.** Her ESKİ metnin `11`'de tam bir kez geçtiği betikle sınandı (kuru koşum, sonra yazım); 101 şartname çifti uygulandı, sorun 0. Çok yerli olan ve K-1455'ten gelen maddeler elle işlendi (aşağıda).
- **Ölçüm.** Belirteç tüketimi bu raporda ölçülmedi; sayılar mercek ve doğrulayıcı sayılarıdır.

**Sonuç:** 485 ham bulgu (Yüksek 13 · Orta 283 · Düşük 189) → birleştirme sonrası **479 tekil** (Yüksek 12 · Orta 281 · Düşük 186). Mercek dağılımı: KA 365 · İT 87 · SB 22 · İP 6 · FK 5 · KD 0 · TE 0 · AT 0. **44 bulgu hükme bağlandı → 20 doğru · 22 kısmen · 2 çürük** (Yüksek 6/6/0; Orta 14/16/2). **41 bulgu uygulandı** (101 şartname çifti); 2 çürütüldü (#360, #290); #345 ile #357 aynı IBAN silme düzeltmesidir (#357'nin şartnamesiyle uygulandı). **435 bulgu (Orta 249 + Düşük 186) doğrulanmadı** — dokümana uygulanmaz; sonraki doğrulama adımlarının girdisidir. **Seviyesi doğrulayıcıca değişen:** #336 Orta → **Yüksek** (yeniden doğrulaması bu adımda yapılmaz, `INSTRUCTIONS` §9 (1); sonraki adıma kalır); düşen: #225 ve #351 Yüksek → Orta; #285, #288, #70, #207, #360, #440, #373, #240, #241, #106, #332 Orta → Düşük. **Çok kritik soru çıkmadı; sorulacak madde kalmadı.** Karar satırları: **K-1456…K-1461**.

> **Sayımın denetimi.** Mercek tablosundaki ham bulgu toplamı 365 + 87 + 22 + 6 + 5 = **485**; birleştirme 6 yinelenen çıkardı (479 tekil); hüküm toplamı 20 + 22 + 2 = **44** = doğrulayıcı tablosundaki bulgu sayıları (12 + 32); uygulanan 41 + çürük 2 + birleşen 1 (#345) = 44; doğrulanmayan 479 − 44 = **435**. Şartname çifti toplamı 101 (bulgu tablosunun son sütunu).

### Envanter Özeti

| Mercek | Paket | Toplam | Ham bulgu | Not |
|---|---|---|---|---|
| KA kapsama ve doğruluk | 44 | 3,29 MB | 365 | faz dilimleri; her task bloğunun kabul kriterleri ve `Dokümanlar` alanı ↔ `7.1`'de bağlı kaynak birimin kuralı; yazım turu kalite notlarının okunmayan bölgeleri önce |
| SB sıra ve bağımlılık | 9 | 0,32 MB | 22 | faz dilimi, sıkıştırılmış task |
| KD kapsam dışı | 10 | 0,51 MB | 0 | `10 §3` KD satırları ↔ sıkıştırılmış task; negatif denetim: hiçbir task kapsam dışı işi yapmıyor |
| İT/OL iç tutarlılık ve ölçülebilirlik | 6 | 0,49 MB | 87 | modül gruplu tam bloklar; K-1455'in kapsamadığı yerler |
| İP üst bölümler | 1 | 59 KB | 6 | `11` §1–§4, §3.1, §6, §7 notları, §8, §9 |
| FK faz kapıları | 1 | 37 KB | 5 | §3, §3.1, §6, `00` kapı satırları, aday tablosu |
| TE terim ve ad | 1 + örnekleme | 1,7 KB | 0 | betik adayı 4 ad, hepsi doğru türetme; 10 task elle örneklendi (T03, T104, T110, T213, T303, T330, T401, T502, T520, T536) |
| AT atıf | betik | — | 0 | 2.141 `NN §x.y` atfı, hedefi olmayan 0 |

### Doğrulayıcılar

Hepsi `[dogrulama]` Opus / high (toplam 20). Kapsam tablosu aşağıdadır.

| Doğrulayıcı | Kademe | Kapsam (bulgu) | Hüküm |
|---|---|---|---|
| D1 | yüksek (tek) | #25 | doğru 1 · kısmen 0 · çürük 0 |
| D2 | yüksek (tek) | #103 | doğru 0 · kısmen 1 · çürük 0 |
| D3 | yüksek (tek) | #117 | doğru 1 · kısmen 0 · çürük 0 |
| D4 | yüksek (tek) | #180 | doğru 0 · kısmen 1 · çürük 0 |
| D5 | yüksek (tek) | #225 | doğru 1 · kısmen 0 · çürük 0 |
| D6 | yüksek (tek) | #226 | doğru 0 · kısmen 1 · çürük 0 |
| D7 | yüksek (tek) | #250 | doğru 1 · kısmen 0 · çürük 0 |
| D8 | yüksek (tek) | #308 | doğru 1 · kısmen 0 · çürük 0 |
| D9 | yüksek (tek) | #351 | doğru 0 · kısmen 1 · çürük 0 |
| D10 | yüksek (tek) | #356 | doğru 1 · kısmen 0 · çürük 0 |
| D11 | yüksek (tek) | #390 | doğru 0 · kısmen 1 · çürük 0 |
| D12 | yüksek (tek) | #431 | doğru 0 · kısmen 1 · çürük 0 |
| D13 | orta (4'lü; Ortak altyapı) | #143, #160, #170, #221 | doğru 2 · kısmen 2 · çürük 0 |
| D14 | orta (4'lü; Sepet ve sipariş) | #285, #123, #284, #288 | doğru 2 · kısmen 2 · çürük 0 |
| D15 | orta (4'lü; Ödeme) | #346, #345, #70, #207 | doğru 1 · kısmen 3 · çürük 0 |
| D16 | orta (4'lü; İptal, cayma ve iade) | #357, #360, #336, #130 | doğru 2 · kısmen 1 · çürük 1 |
| D17 | orta (4'lü; Üyelik) | #281, #440, #273, #276 | doğru 2 · kısmen 2 · çürük 0 |
| D18 | orta (4'lü; Sepet ve sipariş) | #290, #302, #309, #216 | doğru 3 · kısmen 0 · çürük 1 |
| D19 | orta (4'lü; Yönetim ve işlem izi) | #386, #384, #381, #373 | doğru 1 · kısmen 3 · çürük 0 |
| D20 | orta (4'lü; Ayarlar) | #240, #241, #106, #332 | doğru 1 · kısmen 3 · çürük 0 |


### Bulgu → hüküm → uygulama

Seviye, doğrulayıcının hükmüdür ("değişti": ham seviyeden farklı; yeniden doğrulaması sonraki adıma kalır). Uygulama: hükmü doğru ya da kısmen olan bulgunun şartname çiftleri `11`'e işlendi (**K-1456**); ESKİ → YENİ çiftleri betikle uygulandı. Çürük (2) ve şartnamesi olmayan (#345 → #357) satırlar 0 çift taşır.

| Bulgu | Task | Seviye | Hüküm | Şartname çifti |
|---|---|---|---|---|
| #25 | T10 | yüksek | doğru | 7 |
| #103 | T101 | yüksek | kısmen | 2 |
| #117 | T106 | yüksek | doğru | 2 |
| #180 | T301 | yüksek | kısmen | 3 |
| #225 | T328 | Orta (değişti) | doğru | 3 |
| #226 | T330 | yüksek | kısmen | 2 |
| #250 | T337 | yüksek | doğru | 3 |
| #308 | T356 | yüksek | doğru | 2 |
| #351 | T372 | Orta (değişti) | kısmen | 2 |
| #356 | T373 | yüksek | doğru | 3 |
| #390 | T386 | yüksek | kısmen | 2 |
| #431 | T527 | yüksek | kısmen | 2 |
| #143 | T201 | Orta | doğru | 2 |
| #160 | T221 | Orta | doğru | 6 |
| #170 | T228 | Orta | kısmen | 2 |
| #221 | T325 | Orta | kısmen | 2 |
| #285 | T346 | düşük (değişti) | kısmen | 2 |
| #123 | T109 | Orta | doğru | 4 |
| #284 | T346 | Orta | doğru | 3 |
| #288 | T347 | düşük (değişti) | kısmen | 2 |
| #346 | T370 | Orta | doğru | 3 |
| #345 | T370 | Orta | kısmen | 0 |
| #70 | T311 | düşük (değişti) | kısmen | 1 |
| #207 | T311 | düşük (değişti) | kısmen | 3 |
| #357 | T373 | Orta | kısmen | 7 |
| #360 | T374 | düşük (değişti) | çürük | 0 |
| #336 | T369 | yüksek (değişti) | doğru | 2 |
| #130 | T113 | Orta | doğru | 1 |
| #281 | T345 | Orta | doğru | 2 |
| #440 | T533 | düşük (değişti) | kısmen | 2 |
| #273 | T342 | Orta | doğru | 2 |
| #276 | T343 | Orta | kısmen | 5 |
| #290 | T348 | Orta | çürük | 0 |
| #302 | T351 | Orta | doğru | 2 |
| #309 | T356 | Orta | doğru | 2 |
| #216 | T319 | Orta | doğru | 2 |
| #386 | T382 | Orta | doğru | 1 |
| #384 | T383 | Orta | kısmen | 1 |
| #381 | T383 | Orta | kısmen | 2 |
| #373 | T379 | düşük (değişti) | kısmen | 2 |
| #240 | T331 | düşük (değişti) | kısmen | 1 |
| #241 | T331 | düşük (değişti) | kısmen | 1 |
| #106 | T103 | düşük (değişti) | kısmen | 1 |
| #332 | T366 | düşük (değişti) | doğru | 2 |

### Kritik bulguların seyri

- **#25** (T10): uygulama, salt okuma ve yedek durum kullanıcılarının yetkileri — T10 kriterleri ve KK3…KK6 `05`'e göre düzeltildi (7 çift). **#103** (T101): silme davranışı `NO ACTION` (2 çift). **#117** (T106): `audit_logs` göçü yetkisi (2 çift). **#180** (T301): iki sürenin ayrı anda tetiklenmesi ve gönderim kaydının Z-34/Z-35 ayrımı (3 çift). **#226** (T330): #225 ile aynı satır; `(F-1…F-4)` eki #226'nın şartnamesine işlendi. **#250**, **#308**, **#356**, **#390**, **#431**: ilgili task kriter ve KK'leri kaynakla hizalandı; ayrıntı şartname dosyalarındadır.
- **#225** ve **#226** aynı `11` satırını (2431) düzeltir: #225 şartnamesinin 4. çifti kaldırıldı. **#345** ve **#357** aynı IBAN silme düzeltmesidir; #357 üst kümedir.
- **Üst doküman notları** (şartname çifti değil): #351 (hukum 9) → `02 §7.3.6` ve `03` §2.8 (satır 599, "Cayma beyanı hiçbir ekseni değiştirmez"); #356 (hukum 10) → `07 §9.9.28` UC-149 422 satırı ve `04` 7.2.8.3. Aşağıda "Üst dokümana geri besleme".

### K-1455'in hükme kalan 41 maddesi

K-1455 (4): 41 madde hükme kalmıştı; hepsi daha önce Opus ile doğrulanmıştı, bu adımda yeniden doğrulanmadı. **19 madde (A)** K-1455'teki öneriyle uygulandı; **22 madde (T)** da karara bağlandı: hiçbiri `INSTRUCTIONS` §2 "Yine de sorulur" ölçütünü karşılamıyor (bağ eşlemesi, task bölme, KK ekleme, blok bayt sınırı) → öneriyle karar. **Sorulacak madde yok.**

| Madde | Kime | Karar |
|---|---|---|
| (1), (3), (6), (7), (10), (11), (13), (18), (19), (20), (22), (25), (27), (28), (30), (33), (34), (35), (36) | A | K-1455'teki öneri uygulandı: T540 KK2/KK5, T05 AC8/AC9 + KK8/KK9, T209 AC9/KK9, T201 `Dokümanlar` (KP-74, KD-56), T202 AC2/KK2 + KD-64, T224 AC9/KK9, T330 AC1/KK1, T332 AC6, T335 AC10/KK10, T346 KK2, T352 AC8/KK8, T343 AC4/KK4, T366 AC10/KK10, T515 AC3/AC4 (AC7 + AC8 birleşti, KK yeniden numaralandı); (19) ve (20): T334 AC6 ve T338 AC8 zaten taşıyordu, değişiklik yok |
| (2), (39) | T → öneri | KP-70 (tarayıcı tabanı) T540'ta kalır, T539'dan çıkar |
| (4) | T → öneri | T539'a KK10 (KD-22: mobil uygulama ve PWA yok); T540 satırdan çıkar; `§9` "T539 korur" |
| (5) | T → öneri | T03'ten `09 §20` bağı çıkar (genel kural `09 §25` yoluyla denetlenir) |
| (8), (31) | T → öneri | `06 §2.9` ve §5.1 yalnız T102 |
| (9) | T → öneri | T101'den MK-74 bağı çıkar |
| (32) | T → öneri | T101'den `09 §9` ve §12 bağı çıkar |
| (12) | T → öneri | `7.1` KD-57 yalnız T365 |
| (14), (24) | T → öneri | KD-58 yalnız T380; T380 AC10/KK10; T225'ten çıkar |
| (15) | T → öneri | T302'den Z-35, Z-45, Z-46, P-47, P-48 bağları çıkar |
| (16) | T → öneri | T305'ten Z-26, P-38, P-41 çıkar |
| (17) | T → öneri | T332 AC10/KK10 (ifa süresi) |
| (21) | T → öneri | T342'den P-48 ve `02 §8.2.6` çıkar |
| (23) | T → öneri | T375'ten B-3, B-11, B-12, B-13 çıkar; T386 B-11, B-12 `Dokümanlar`a |
| (26) | T → öneri | T509 AC1/KK1 (misafir e-postası) ve `Dokümanlar` |
| (29) | T → öneri | T516 `Dokümanlar` (§11.2, P-13, KP-27) |
| (37), (38), (40) | T → öneri | T503 AC5 (ifa süresi), AC6 (Paylaş), KK5/KK6 |
| (41) | T → öneri | T525 `Dokümanlar` KP-44 |

**Blok bayt sınırı (2.560).** Sığmayan bloklar kısaltmayla çözüldü (bölme yok; anlam korunarak): T373 (2.608 → 2.555), T527 (2.714 → 2.555), T372, T356, T380 (2.578 → 2.547; K-1455 (14) ve (24) eklemeleri sonrası). `11_task_blok_denetle.py --tam` sorun 0.

### Üst dokümana geri besleme (K-652)

Ayrıntı kaynağa göre doğrudur; `11` tüketicidir (K-1406 (2)(d)). İki doğrulayıcı notu üst dokümanda gerçek bir tutarsızlık buldu; ikisi de **kural değişikliği değil, metin hizalamasıdır** (yeni kural numarası yok, ✓ korunur, kalite döngüsü yeniden açılmaz):

| Not | Doküman | Düzeltme | Sürüm |
|---|---|---|---|
| hukum 9 (#351, T372) | `02 §7.3.6` — "sevkiyat durumu değişmez" `02 §5.6.6` ile gerilimdeydi (beyan "kendi başına" ekseni değiştirmez; teslim işaretsiz kalemde kapanış ve S2/S10/S11/S3/S4 geçişi §5.6.6'dadır) | "beyan sevkiyat durumunu kendi başına değiştirmez (teslim işaretsiz kalemde kapanışın etkisi §5.6.6'dadır)" | `02` v0.104 |
| hukum 9 | `03` §2.8 (satır 599) — "Cayma beyanı hiçbir ekseni değiştirmez" | "kendi başına hiçbir ekseni değiştirmez" | `03` v0.44 |
| hukum 10 (#356, T373) | `07 §9.9.28` UC-149 422 satırı — alt sınırın istisnası düşmüştü (`02 §7.4.7` koşullu yazar; `04` 7.2.8.3 istisnayı zaten taşır) | sonuna "; kargodan dönen malda alt sınır yoktur (`04` 7.2.8.3)" | `07` v0.33 |

`04` 7.2.8.3 zaten doğrudur, değişmedi. Geri besleme tablosu satırı `07 §1.3`'tedir (K-1460).

### Doğrulanmayan bulgular (sonraki adımın girdisi)

**435 bulgu** (Orta 249 + Düşük 186) bu adımda **doğrulanmadı ve dokümana uygulanmadı** — adım başına 20 Opus doğrulayıcı sınırı. Liste puana göre sıralı: `C:/Users/teknodeveloper/.claude/helper-state/shopfolio/kd-audit/dogrulanmadi.md` (300 KB; yardımcı durum dizini, depoda değil). Öneri: sonraki adım "audit 2" olarak Orta'yı dört bulgulu gruplarla (~62 doğrulayıcı) ya da ayrı adımlara bölerek doğrulasın; **#336'nın (Orta → Yüksek) yeniden doğrulaması** aynı adıma kalır. Sıra: doğrulanmamış bulguların doğrulanması, sonra deep review.

### Doğrulanmamış bulgular — KD-audit-2a

Plan (`scripts/doc-scans/11_audit_grupla2.py`): 435 bulgu → **95 doğrulayıcı grubu**, **6 adım** (2a 12 · 2b 20 · 2c 20 · 2d 20 · 2e 17 · 2f 6; adım başına en çok 20); sıra para ve durum riskine göre (Ödeme, İptal-cayma-iade, Sepet ve sipariş önce). Doğrulamasız uygulama, örnekleme ya da atlama yoktur (proje sahibinin kararı, 2026-10-31).

| Doğrulayıcı | Model | Kademe | Modül | Sayı |
|---|---|---|---|---|
| 1–2, 5 | Opus / high | Orta 4'lü (1–2), 3'lü (5) | Ödeme | 3 |
| 3–4 | Opus / high | Düşük 6'lı | Ödeme | 2 |
| 6, 8–10 | Opus / high | Orta 4'lü | İptal, cayma ve iade | 4 |
| 7 | Opus / high | Düşük 7'li | İptal, cayma ve iade | 1 |
| 11 | Opus / high | Düşük 6'lı | İptal, cayma ve iade | 1 |
| 12 | Opus / high | Orta 3'lü | İptal, cayma ve iade | 1 |
| **Toplam** | | | | **12** |

**Hükümler (55 bulgu):** 25 doğru · 21 kısmen · 9 çürük. **Uygulanan:** 44 doğru ya da kısmen bulgudan 37'si, **71 şartname çifti** (7 bulgu ek şartname istemedi: önceki turda karşılanmış ya da başka bulguyla birleşti; üç çakışma Parça A'da elle birleştirildi). Liste: `C:/Users/teknodeveloper/.claude/helper-state/shopfolio/kd-audit-2/2a/uygulanan.md` (yardımcı durum dizini). **Seviyesi düşen** 13 bulgu (Orta/Düşük → Düşük) uygulandı. **Yükselen:** #437 ve #350 (Düşük → Orta) uygulanmadı; KD-audit-2r'de yeniden doğrulanır. **Blok bayt sınırı:** T368, T369, T372, T374, T528 kısaltıldı (bölme yok; K-1462 (3)).

**İlerleme:** 435 bulgudan **55'i hükme bağlandı**; **kalan 380** (2b…2f) + #336, #437, #350 (2r). Kapılar: `11_matris_kapsama.py` 1.501/1.501 sorun 0 · `11_task_blok_denetle.py --tam` 0 · `11_yazim_kapanis.py` 0 · `11_geri_yon.py . --denetle` 214 = 214.

### Doğrulanmamış bulgular — KD-audit-2b

Plan 20 doğrulayıcı (97 bulgu) kurmuştu; **18'i koştu**, 19. ve 20. doğrulayıcılar eşzamanlı ajan sınırı hatasıyla açılamadı ve adım başına 20 sayacına yine de sayıldı. Koşmayan **10 bulgu (#391, #392, #396, #182, #197, #198, #206, #234, #235, #399; Bildirim)** 2e'ye taşındı; plan bir adım uzar. Doğrulamasız uygulama yoktur.

| Doğrulayıcı | Model | Kademe | Modül | Sayı |
|---|---|---|---|---|
| 2–8, 14, 16, 17 | Opus / high | Orta 4'lü | Sepet ve sipariş (2–8), Bildirim (14, 16, 17) | 10 |
| 9, 13, 18 | Opus / high | Orta 3'lü | Sepet ve sipariş (9, 13), Bildirim (18) | 3 |
| 1, 11, 12 | Opus / high | Düşük 8'li | Sepet ve sipariş | 3 |
| 10, 15 | Opus / high | Düşük 7'li | Sepet ve sipariş (10), Bildirim (15) | 2 |
| **Toplam** | | | | **18** |

**Hükümler (87 bulgu):** 33 doğru · 33 kısmen · 21 çürük. **Uygulanan:** 59 bulgu, **126 şartname çifti** (138 çiftten 12'si yükselen bulguların). Liste: `C:/Users/teknodeveloper/.claude/helper-state/shopfolio/kd-audit-2/2b/uygulanan.md` (yardımcı durum dizini). **Seviyesi düşen** 15 bulgu uygulandı. **Yükselen:** #210 (Düşük → Orta), #12, #134, #186, #467 (Orta → Yüksek) uygulanmadı; KD-audit-2r'de yeniden doğrulanır. **Blok bayt sınırı:** T305, T306, T307, T319, T350, T356, T366 kısaltıldı (bölme yok; K-1464 (3)'te kayıp listesi). KARAR ve ÜST DOKÜMAN satırı çıkmadı.

**İlerleme:** 435 bulgudan **142'si hükme bağlandı** (2a 55 + 2b 87); **kalan 293** (2b'den 10, 2c…2f) + #336, #437, #350 (2r) ve 2b'nin beş yükselen bulgusu. Kapılar: `11_matris_kapsama.py` 1.501/1.501 sorun 0 · `11_task_blok_denetle.py --tam` 0 · `11_yazim_kapanis.py` 0 · `11_geri_yon.py . --denetle` 214 = 214.

### Doğrulanmamış bulgular — KD-audit-2c

Plan 20 doğrulayıcı (94 bulgu) kurdu; **20'si koştu** (iki dalga, 1–10 ve 11–20), bekçi olayı yok. Doğrulamasız uygulama yoktur.

| Doğrulayıcı | Model | Kademe | Modül | Sayı |
|---|---|---|---|---|
| 2, 3, 5, 6, 7, 8, 9, 10, 14, 17 | Opus / high | Orta 4'lü | Ayarlar (2, 3), Katalog (5–10), Kurumsal içerik (14, 17) | 10 |
| 4, 16, 18, 20 | Opus / high | Orta 3'lü | Ayarlar (4), Kurumsal içerik (16, 18, 20) | 4 |
| 1 | Opus / high | Düşük 8'li | Ayarlar | 1 |
| 11, 12, 13, 15 | Opus / high | Düşük 7'li | Katalog (11–13), Kurumsal içerik (15) | 4 |
| 19 | Opus / high | Düşük 6'lı | Kurumsal içerik | 1 |
| **Toplam** | | | | **20** |

**Hükümler (94 bulgu):** 34 doğru · 49 kısmen · 11 çürük. **Uygulanan:** 74 bulgu, **171 şartname çifti** (179 çiftten 8'i yükselen bulguların; 4 ESKI çakışması hükümde birleştirildi). Liste: `C:/Users/teknodeveloper/.claude/helper-state/shopfolio/kd-audit-2/2c/uygulanan.md` (yardımcı durum dizini). **Seviyesi düşen** 25 bulgu uygulandı. **Yükselen:** #469 (Orta → Yüksek), #322 ve #261 (Düşük → Orta) uygulanmadı; KD-audit-2r'de yeniden doğrulanır. **Blok bayt sınırı:** T330, T337, T338, T339, T366, T507, T536 kısaltıldı (bölme yok; K-1465 (3)'te kayıp listesi). **Üst doküman:** `07 §4.10.1` isteğinin "kısa süreli bildirimin koduyla" ifadesi "sonuç hâlinin koduyla" oldu (`07` v0.34; K-652). KARAR satırı çıkmadı.

**İlerleme:** 435 bulgudan **236'sı hükme bağlandı** (2a 55 + 2b 87 + 2c 94); **kalan 199** (2b'den 10 dahil; 2d, 2e, 2f) + #336, #437, #350 (2r) ve 2b ile 2c'nin sekiz yükselen bulgusu. Kapılar: `11_matris_kapsama.py` 1.501/1.501 sorun 0 · `11_task_blok_denetle.py --tam` 0 · `11_yazim_kapanis.py` 0 · `11_geri_yon.py . --denetle` 214 = 214.

### Sınırlar

- **Uygulanmayan 435 bulgu** gövdede hâlâ bulunabilir; bu raporun "temiz" okunmaması gerekir: audit yalnız 44 bulguyu hükme bağladı.
- **KD, TE, AT mercekleri 0 bulgu** verdi; TE'nin örneklemesi 10 task'ladır, tam sayım değildir.
- **Mercekler Sonnet / low**; doğrulayıcı hükmü yalnız Yüksek ve Orta'nın 44'ünü kapsar. Düşük bulguların (186) hiçbiri Opus'a girmedi.
- **Seviye değişimleri** (1 yükselen, 13 düşen) yeniden doğrulanmadı (`INSTRUCTIONS` §9 (1)).
- **Belirteç tüketimi ölçülmedi.**
- Parça B bir kez bağlam sınırında bölündü (WIP commit `97f3fed`); uygulama yeniden yapılmadı.
- Kapılar son durumda: `11_matris_kapsama.py` 1.501/1.501 sorun 0 · `11_task_blok_denetle.py --tam` 0 · `11_yazim_kapanis.py` 0 · `11_geri_yon.py . --denetle` 214 = 214.
