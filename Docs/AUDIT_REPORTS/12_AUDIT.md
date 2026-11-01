# Audit Raporu — 12 Validation Protocol

**Tarih:** 2026-11-01 · **Hedef:** `Docs/12_VALIDATION_PROTOCOL.md` v0.15 (bulgular v0.15'te üretildi, v0.16'da işlendi; taban `main` = 6ac4c72) · **Bağlam:** `PRODUCT_DISCOVERY_STATUS.md` K-1516…K-1548; üst dokümanlar `02`–`11` · **Odak:** Tam denetim — 1.471 satırlık matris (§4), proje-spesifik ek (§12), çerçeve bölümleri (§3, §5…§11) ve yazım turu kalite notlarının okunmadan yazılan bölgeleri önce.

### Koşum biçimi (K-431, K-1232; `audit` skill'i, "Koşum biçimi")

Dokümanı yazan bağlamda koşmadı; adım baştan iki parçaya bölündü (`.claude/INSTRUCTIONS.md` §9 (8)). **Parça A (denetim):** paketleyici betik (`scripts/doc-scans/12_audit_paketle.py`; Karar B, K-1399 — `11_*` paketleyicisi `11`'e özel olduğundan yenisi yazıldı) merceklere satır numaralı dar paket üretti, mercekler paketleri okudu, doğrulayıcılar hüküm ve şartname çiftlerini bıraktı. **Parça B (uygulama, kayıt, PR)** ayrı işçidir.

- **Paketleyici betik.** 35 paket (her biri en çok 85.000 bayt): mercek kapsamındaki `12` satırları + VAL satırının Kaynak hücresindeki birimlerin kaynak metni (`10` KP satırı, `02`–`11` bölüm satırları) + ilgili K satırları. İki yönlü sınama yapıldı: KA merceklerinin dilimleri 1.471 VAL satırının tamamını kapsıyor (1.471/1.471). Park tablosunun 42 maddesi için ek, el yapımı paketler (IT_02, IT_03) açıldı; IT_02 bağlam sınırı nedeniyle üç maddeyi bıraktı, IT_03 tamamladı.
- **Mercekler** `[ozet]` Sonnet / low, ön planda, beşli dalga; toplam 38 koşum + 2 yeniden koşum (KD_01, KD_02 tam okuma için yeniden koşuldu).
- **Kademeli doğrulama — 20 doğrulayıcı, hepsi `[dogrulama]` Opus / high, ön planda.** 14 Yüksek bulgu tek tek (D1…D14), 24 Orta bulgu 4'lü gruplarla (D15…D20; bölüm gruplu). Düşük bulgular Opus'a girmedi. **Adım başına 20 doğrulayıcı sınırı aşılmadı; bu adımda yeni doğrulayıcı açılmadı.**
- **Uygulama.** Her ESKİ metnin `12`'de tam bir kez geçtiği, hiçbir ESKİ'nin iki çiftte bulunmadığı ve aralıkların çakışmadığı betikle sınandı (`12_audit_hukum_denetle.py`, `12_audit_uygula.py` kuru koşum: sorun 0); 94 şartname çifti uygulandı. Kaynak hücresi başka satırlarda birebir tekrarlandığı için benzersiz ESKİ kurulamayan düzeltmeler satır kimliği ve sütun hedefli ikinci betikle (`12_audit_satir_uygula.py`; her satır tek, her hücre beklenen değerde) işlendi: 41 hücre.
- **Ölçüm.** Belirteç tüketimi bu raporda ölçülmedi; sayılar mercek ve doğrulayıcı sayılarıdır.

**Sonuç:** 126 ham bulgu (KA 92 · İT 14 · ÇR 7 · SV 7 · ÖL 5 · KD 1 · TE 0) → birleştirme sonrası **120 tekil** (Yüksek 24 · Orta 63 · Düşük 33). **38 bulgu hükme bağlandı:** doğru 31 · kısmen 7 · çürük 0; yükselen bulgu yok; **6 bulgunun seviyesi düştü** (#13, #16, #17 Yüksek → Orta; #24 Yüksek → Düşük; #32, #75 → Düşük). **38 bulgunun hepsi uygulandı** (94 çift + 41 hücre; `12` v0.15 → v0.16, satır eklenmedi ya da silinmedi, §1 sayımları ve park tablosu değişmedi). **82 bulgu doğrulanmadı** (Yüksek 10 · Orta 39 · Düşük 33) ve dokümana uygulanmadı — sonraki adımın (audit-2) girdisi.

> **Sayımın denetimi.** Mercek tablosundaki ham bulgu toplamı 92 + 14 + 7 + 7 + 5 + 1 + 0 = **126**; birleştirme 6 yinelenen çıkardı (120 tekil); tekil bulgu 38 hükümlü + 82 doğrulanmayan = **120**; hüküm toplamı 31 + 7 + 0 = **38** = doğrulayıcı tablosundaki bulgu sayıları (14 + 24); uygulanan 38 = doğru + kısmen; çürük 0.

### Envanter Özeti

| Mercek | Paket | Ham bulgu | Not |
|---|---|---|---|
| KA kapsama ve doğruluk (KA_01…KA_22) | 22 | 92 | §4 ve §12 dilimleri (12:329-1639 ve 1912-2609); VAL satırının beklenen sonucu ↔ Kaynak hücresindeki kural; yazım turu kalite notlarının okunmayan bölgeleri önce |
| ÇR çerçeve (CR_01) | 1 | 7 | §3, §5…§11 ↔ `00` §D, §E, §F ↔ `11` §6 faz kapıları ↔ `skills/validate` |
| SV seviye ve etiket (SV_01, SV_02) | 2 | 7 | betikle anomali listesi + kural okuması (K-1512, K-1518, K-1519); anahtar sözcük eşlemesi kesin değildir |
| KD kapsam dışı (KD_01…KD_06) | 6 | 1 | negatif denetim: MVP dışı (KD/YH/SK) özelliğe kriter yazılmış mı; tek bulgu (#56) `10` KD-59 ile P-9 arasındaki kaynak çelişkisi |
| TE terim ve ad (TE_01) | 1 | 0 | 130 aday; yalnız `02`–`11`'de hiç ya da en çok iki kez geçen simgeler |
| ÖL ölçülebilirlik (OL_01, OL_02) | 2 | 5 | yalnız betik adayları okundu |
| İT iç tutarlılık (IT_01 + IT_02, IT_03) | 1 + 2 ek | 14 | §1/§2 sayımları, aday çiftler; park tablosu 42 madde ↔ gövde (IT_02, IT_03) |

### Doğrulayıcılar

Hepsi `[dogrulama]` Opus / high (toplam 20).

| Doğrulayıcı | Kademe | Kapsam (bulgu) | Hüküm |
|---|---|---|---|
| D1 | Yüksek (tek) | #3 | doğru 0 · kısmen 1 · çürük 0 |
| D2 | Yüksek (tek) | #5 | doğru 1 · kısmen 0 · çürük 0 |
| D3 | Yüksek (tek) | #6 | doğru 1 · kısmen 0 · çürük 0 |
| D4 | Yüksek (tek) | #7 | doğru 1 · kısmen 0 · çürük 0 |
| D5 | Yüksek (tek) | #12 | doğru 1 · kısmen 0 · çürük 0 |
| D6 | Yüksek (tek) | #13 | doğru 1 · kısmen 0 · çürük 0 |
| D7 | Yüksek (tek) | #14 | doğru 1 · kısmen 0 · çürük 0 |
| D8 | Yüksek (tek) | #16 | doğru 1 · kısmen 0 · çürük 0 |
| D9 | Yüksek (tek) | #17 | doğru 0 · kısmen 1 · çürük 0 |
| D10 | Yüksek (tek) | #18 | doğru 1 · kısmen 0 · çürük 0 |
| D11 | Yüksek (tek) | #19 | doğru 1 · kısmen 0 · çürük 0 |
| D12 | Yüksek (tek) | #21 | doğru 1 · kısmen 0 · çürük 0 |
| D13 | Yüksek (tek) | #22 | doğru 1 · kısmen 0 · çürük 0 |
| D14 | Yüksek (tek) | #24 | doğru 0 · kısmen 1 · çürük 0 |
| D15 | Orta (4'lü) | #30, #31, #32, #33 | doğru 3 · kısmen 1 · çürük 0 |
| D16 | Orta (4'lü) | #35, #36, #37, #38 | doğru 4 · kısmen 0 · çürük 0 |
| D17 | Orta (4'lü) | #39, #40, #41, #42 | doğru 2 · kısmen 2 · çürük 0 |
| D18 | Orta (4'lü) | #43, #44, #45, #49 | doğru 4 · kısmen 0 · çürük 0 |
| D19 | Orta (4'lü) | #57, #58, #59, #60 | doğru 3 · kısmen 1 · çürük 0 |
| D20 | Orta (4'lü) | #75, #77, #78, #79 | doğru 4 · kısmen 0 · çürük 0 |

### Bulgu → hüküm → uygulama

Seviye, doğrulayıcının hükmüdür ("değişti": ham seviyeden farklı; yeniden doğrulaması sonraki adıma kalır — yükselen yok, düşenlerin yeniden doğrulaması gerekmez). Uygulama: hükmü doğru ya da kısmen olan bulgunun şartname çiftleri betikle, satır kimlikli hücre düzeltmeleri ikinci betikle `12`'ye işlendi (**K-1549**); üst dokümana dokunan üç bulgu geri beslendi (**K-1550**).

| Bulgu | 12 yeri | VAL | Seviye | Hüküm | Uygulama |
|---|---|---|---|---|---|
| #3 | 484 | B016 | Yüksek | kısmen | 3 çift; üst: `10` KP-18, `02` §2.1 (K-1550) |
| #5 | 533 | A080 | Yüksek | doğru | 1 çift |
| #6 | 673 | A166 | Yüksek | doğru | 2 çift + 1 hücre |
| #7 | 765 | A233 | Yüksek | doğru | 1 çift |
| #12 | 1200 | E073 | Yüksek | doğru | 2 çift |
| #13 | 1214 | E087 | Yüksek → Orta (değişti) | doğru | 1 çift |
| #14 | 1239 | B105 | Yüksek | doğru | 1 çift |
| #16 | 1490 | A629 | Yüksek → Orta (değişti) | doğru | 2 çift |
| #17 | 1937 | F010 | Yüksek → Orta (değişti) | kısmen | 1 çift |
| #18 | 2105 | D073 | Yüksek | doğru | 2 çift |
| #19 | 2174 | A824 | Yüksek | doğru | 4 çift |
| #21 | 2224 | D094 | Yüksek | doğru | 1 çift |
| #22 | 2297 | A877 | Yüksek | doğru | 2 çift |
| #24 | 2491 | A974 | Yüksek → Düşük (değişti) | kısmen | 1 çift; üst: `05` §10.1.6 (K-1550) |
| #30 | 442 | A043, A045 | Orta | doğru | 1 çift + 1 hücre |
| #31 | 442 | A043, A045, A046 | Orta | doğru | 6 çift |
| #32 | 469 | D011 | Orta → Düşük (değişti) | kısmen | 1 çift |
| #33 | 528 | A075 | Orta | doğru | 2 çift |
| #35 | 576 | A164, A165, A166 … | Orta | doğru | 6 çift + 6 hücre |
| #36 | 576 | A120, D023 | Orta | doğru | 0 çift + 2 hücre |
| #37 | 659 | A160 | Orta | doğru | 2 çift |
| #38 | 670 | A163 | Orta | doğru | 4 çift |
| #39 | 748 | A218, A223, A224 … | Orta | doğru | 3 çift + 20 hücre |
| #40 | 789 | A248, A249, A250 | Orta | doğru | 3 çift + 3 hücre |
| #41 | 796 | A254 | Orta | kısmen | 3 çift |
| #42 | 819 | A269, A270 | Orta | kısmen | 2 çift + 1 hücre |
| #43 | 828 | A277, A281 | Orta | doğru | 4 çift + 1 hücre |
| #44 | 877 | A306, A307, A308 … | Orta | doğru | 4 çift + 3 hücre |
| #45 | 919 | A329, A330 | Orta | doğru | 2 çift + 1 hücre |
| #49 | 979 | C077, C078 | Orta | doğru | 4 çift + 2 hücre |
| #57 | 1328 | A474 | Orta | doğru | 3 çift |
| #58 | 1335 | A481 | Orta | kısmen | 2 çift; üst: `07` §6.2.3 (K-1550) |
| #59 | 1338 | A484 | Orta | doğru | 3 çift |
| #60 | 1402 | A548 | Orta | doğru | 2 çift |
| #75 | 2170 | A822 | Orta → Düşük (değişti) | doğru | 2 çift |
| #77 | 2180 | A828 | Orta | doğru | 3 çift |
| #78 | 2181 | A829 | Orta | doğru | 4 çift |
| #79 | 2184 | A831 | Orta | doğru | 4 çift |

### Kritik bulguların seyri

Yüksek 14 bulgunun hükmü: 10 Yüksek olarak doğrulandı (#3, #5, #6, #7, #12, #14, #18, #19, #21, #22); #13, #16, #17 Orta'ya, #24 Düşük'e indi. En ağır bulgular: #3 (VAL-B016 üyenin sipariş bağlantısını girişsiz açılır diyordu — kaynağın kendisi de çelişkiliydi), #6 (KDV oranını varyanta bağlıyordu; kaynak ürün düzleminde tutar), #7 (teyit süresini uzatıyordu; kaynak süreden düşer), #5 (cayma kaydının tarihini gönderim tarihi saydı; kaynakta ulaşma tarihidir). Kaynak hücrelerinde bölüm kayması kalıbı (`02` §3.6/§3.8, §7.4.9, §10.4) en çok tekrarlayan bulgu sınıfıdır; hükümlü yerleri düzeltildi, kalan yerler audit-2'ye yazıldı.

### Üst dokümana geri besleme (K-652)

Üç bulgunun kaynağı gerçekten yanlıştı ya da eksikti; `12` tüketicidir, kaynak kazandı (aynı PR, sürüm artışı, karar satırı K-1550):

| Bulgu | Doküman | Düzeltme |
|---|---|---|
| #3 | `10` v0.59 → v0.60 (KP-18); `02` v0.108 → v0.109 (§2.1 Sipariş takibi satırı) | "herkes sipariş e-postasındaki bağlantıyla" cümlesi `02 §3.22.5`'e (K-1256) hizalandı: erişim anahtarlı bağlantı misafir alıcıda ve hesabı silinmiş müşteride; üyenin bağlantısı girişsiz açılmaz |
| #24 | `05` v0.43 → v0.44 (§10.1.6 Veritabanı satırı) | "Canlılık ve durum uçları yanıt vermez" → canlılık ucu 503 verir (`07 §4.33.1`, K-1328 (5)); durum ucu okunamayan koşul kaynağında 503 döner (`05 §9.4.10`) |
| #58 | `07` v0.38 → v0.39 (§6.2.3 `OrderSummaryChanged` satırı) | fark listesine "KDV oranı" eklendi (`07 §4.13.6`'nın listesine hizalandı) |

**Elenenler.** #16 — `07 §2.8.1`/`§2.8.7` ters vekilin kendi 413'ünün görsel yükleme sayfasında nasıl görüneceğini yazmıyor; yazım hatası değil tasarım ve işletim boşluğu (`DEPLOY_RUNBOOK` ile birlikte karar ister), `12`'deki "ters vekil ve çatı aynı sonucu verir" cümlesi korundu; açık madde olarak audit-2'ye yazıldı. #77 — K-946 (1)'in "denenir" cümlesi tarihsel, `05 §4.3.4` ve `06` düzeltilmiş; düzeltme gerekmez. Okunuş tarama paragrafı (checklist §6): terimler "herkes … bağlantı", "canlılık ve durum uçları … yanıt", `OrderSummaryChanged` + "özet farkı"; `02`–`12` genelinde eşleşmeler okundu; eski hâlde kalan yer yalnız yukarıdaki satırlardı.

### "Yine de sorulur" ölçütü

Hükümlerin hiçbirinde "Yine de sorulur" işaretli madde yok. Geri dönüşü zor, para ya da hukuk riski taşıyan karar çıkmadı; ⚠ karar yok.

### Doğrulanmayan bulgular (sonraki adımın girdisi)

**82 bulgu** (Yüksek 10 · Orta 39 · Düşük 33) bu adımda **doğrulanmadı ve dokümana uygulanmadı** — adım başına 20 Opus doğrulayıcı sınırı. Liste: `C:/Users/teknodeveloper/.claude/helper-state/shopfolio/a10-kd-audit/dogrulanmadi.md` (yardımcı durum dizini, depoda değil). Ek olarak hüküm dosyalarının **23 `YAN:` notu** (aynı bölüm kayması kalıbının başka yerleri; ilgili bulgu hükümlü ama yan yer doğrulanmadı) aynı dosyanın sonuna yazıldı ve uygulanmadı. Öncelik: #1 ve #2 (park P4.2 ve P4.6: sınanmayan iki kural, yeni VAL satırı gerekir — satır sayımı ve matris hizası etkilenir), #8…#10 (KP metrik paydaları A278…A280), #15 (A532), #4 ve #20 (severity yükseltme: C008, A825), #23, #56 (`10` KD-59 ↔ P-9 kaynak çelişkisi; `10` tarafı düzelecekse K-652 geri besleme). Önerilen sonraki adım: **audit-2** — Yüksek tek, Orta 4'lü, Düşük 5…8'li doğrulayıcı grupları, adım başına en çok 20.

### Kapılar

`12_kapsam_hizasi.py` hata 0 · `12_matris_denetle.py --kapsam 1-77`: 77/77 KP kapsandı, eksik 0, hata 0; severity KRİTİK 601 · ORTA 738 · DÜŞÜK 132 (toplam 1.471; değişmedi) · `12_atif_denetle.py`: 2.601 atıf, hedefi bulunamayan 0, hata 0.

### Sınırlar

KD mercekleri tam okuma için yeniden koşuldu. SV merceğinin anahtar sözcük eşlemesi kesin değildir. ÖL merceği yalnız betikle seçilen adayları okudu. TE merceği yalnız `02`–`11`'de hiç ya da en çok iki kez geçen simgeleri denetledi. İT_01 park–gövde sınamasını yapmadı (IT_02, IT_03 yaptı). KA mercekleri satır dilimi bazındadır; kaynak kesilmişse grep ile tamamlandı. Doğrulayıcılar yalnız kendilerine verilen bulgu ve çevresini okudu; tüm matris yeniden okunmadı. Belirteç tüketimi ölçülmedi.

**Denetlenebilirlik kuralı:** Bir audit raporu, envanter toplamı ile eşleştirme sayısı **eşleşmiyorsa** geçersizdir — öğe atlanmış demektir. Rapor bu iki sayıyı açıkça gösterir (126 ham = 92 + 14 + 7 + 7 + 5 + 1; 120 tekil = 38 hükümlü + 82 doğrulanmayan).

## Doğrulanmamış bulgular — KD-audit-2a

**Tarih:** 2026-11-01 · **Girdi:** 82 doğrulanmayan bulgu + hüküm dosyalarındaki 23 `YAN:` notu = 105 madde; `scripts/doc-scans/12_audit_grupla2.py` onları 56 doğrulayıcıya ve 3 adıma (2a 20 · 2b 20 · 2c 16) böldü. Her madde kademeli düzenle doğrulanır (Yüksek tek · Orta 1–3'lü · Düşük 3–4'lü); örnekleme ya da atlama yoktur. 2a: 31 madde (27 bulgu: Yüksek 10 · Orta 11 · Düşük 6; 4 `YAN`: Y6, Y9, Y21, Y22).

### Doğrulayıcılar (20; hepsi `[dogrulama]` Opus / high)

| Doğrulayıcı | Kademe | Kapsam | Hüküm |
|---|---|---|---|
| D1…D10 | Yüksek (tek) | #1, #2, #8, #9, #10, #4, #11, #15, #20, #23 | her biri doğru 1 (#9 Orta'ya indi) |
| D11 | Orta | #72, #73 | doğru 2 |
| D12 · D13 | Orta (tek) | #74 · #76 | doğru 1 · doğru 1 |
| D14 | Düşük | #116, Y21, Y22, Y6 | doğru 2 · kısmen 1 (#116) · çürük 1 (Y6) |
| D15 | Orta | #80, #81, #82 | doğru 1 · kısmen 2 (#80, #81) |
| D16 | Orta | #84, #85, #86 | doğru 3 |
| D17 | Düşük (tek) | #117 | doğru 1 |
| D18 | Düşük | #92, #93, Y9 | doğru 3 |
| D19 | Orta (tek) | #34 | doğru 1 |
| D20 | Düşük | #94, #95 | doğru 2 |

**Toplam hüküm: 31 madde — 27 doğru · 3 kısmen · 1 çürük.** Çürük: Y6 (VAL-A824, "doğrulanamayan bildirim kayda yazılır" — `12`'de zaten karşılanmış, 12:2174). Seviye değişimi: #9 Yüksek → Orta (uygulandı); Y21 ve #95 Düşük → Orta **yükseltildi**.

### Uygulama

28 madde uygulandı: **20 ESKİ → YENİ çifti** (`12_audit_uygula2.py`; #80'in çifti VAL-D089 Beklenen hücresinde #2'nin hücre düzeltmesiyle birleştirildi), **33 hücre düzeltmesi** (32 matris hücresi + park tablosu P4.6 Durum hücresi), **2 yeni matris satırı**: VAL-A976 (§12.6; ödeme süresi dolumu ile yeniden onayın kilit sırası; park P4.2 (5)) ve VAL-D120 (§12.7; iptalden sonra gelip sistemce geri ödenen ödemenin saklama kolu; park P4.6 (4)). Severity ORTA → KRİTİK: VAL-C008 (#4), VAL-A825 (#20), VAL-D082 (#76), VAL-D093, VAL-D095, VAL-D096 (#81), VAL-A079 (#34) — para, silme ve iade/cayma/fesih süresini doğrudan etkileyen kural (`12` §4.2 kural 6). Park tablosu P4.2 ve P4.6 satırları yeni satırlara bağlandı. Uygulanan liste: yardımcı durum dizini `a10-kd-audit-2/2a/uygulanan.md` (depoda değil).

**Matris sayımı (betik):** 1.471 → **1.473** satır; Seviye A 976 · D 120; Severity KRİTİK 601 → **610** · ORTA 738 → **731** · DÜŞÜK 132; Kanıt ortamı `yok` 1.223; `BEKLEMEDE` 1.473; §12.1…§12.13 343 → 345.

### Üst dokümana geri besleme (K-652)

| Bulgu | Doküman | Düzeltme |
|---|---|---|
| #116 | `05` v0.44 → v0.45 (§4.2.2 adım (0)) | "müşteri o siparişin sayfasına gider" → "teyit ekranına gider" (§4.5.1 ve K-1199 ile aynı; K-1552) |

### Yükseltilenler (uygulanmadı → KD-audit-2r)

Y21 (VAL-A823: Beklenen "aynı anahtarla gider" diyor; `05 §4.3.3`/`§4.6.1`'e göre istek yeniden gönderilmez, sonuç sağlayıcıya sorulur) ve #95 (VAL-A081: ürünün yapmadığı gönderim tarihi denetimini bekliyor; bir yıllık sınırı VAL-A236 sınar). Dosya: `a10-kd-audit-2/yeniden.md`.

### 2c'ye ek: hükümlerin yeni `YAN:` notları (doğrulanmadı, uygulanmadı)

- VAL-A070 (12:518): baska kanaldan gelen gecikme feshinin kaydı ORTA; geri ödemenin on dört günü bildirimin firmaya ulaştığı tarihten işler (`02 §7.2.3`, K-706) — KRİTİK adayı (#4'ün yanı).
- VAL-D094 (12:2224): sipariş imhasından önce on yılı dolan talebi sınamıyor; ikinci bir talep eklenmeli (#82'nin yanı).
- VAL-C052 (12:757): reddin "mal stoğa eklenmez" sonucunu doğrulamıyor; `02 §7.4.9`, K-656 (#85'in yanı).
- `02 §3.16.8`, `§3.16.10`: girişte sepet birleşmesinin stok ve ürün sınırı kolu 12'de sınanmıyor (VAL-A030, VAL-A031 yalnız sepete eklemeyi sınar; #92'nin yanı).
- VAL-A278 ve VAL-A280 paydası: bu adımda #8 ve #10 ile karşılandı (ayrı madde gerekmez).
- Kaynak hücresi atıf kayması: VAL-A278 ve VAL-A280 `02 §10.6.2` yazıyor, oranı tanımlayan `§10.6.3`'tür (A280'de atıf Beklenen içinde verildi; A278 için #8 aynısını yaptı).

### Kalan

105 maddenin 31'i hükme bağlandı (27 bulgu · 4 `YAN`); **kalan 74 madde** (55 bulgu + 19 `YAN`; 2b: 20 · 2c: 16 doğrulayıcı) ve 2r (Y21, #95). Açık madde: `07 §2.8` ters vekil 413 (#16) ve `10` KD-59 ↔ P-9 (#56) bu adımda düşmedi. Kapılar: `12_kapsam_hizasi.py` 0 · `12_matris_denetle.py --kapsam 1-77` hata 0 (1.473 satır, 77/77 KP) · `12_atif_denetle.py` 0. Bekçi olayı 1 (Parça A; betik yanlışlıkla heredoc ile yazılmak istendi, engellendi).

## Doğrulanmamış bulgular — KD-audit-2b

**Tarih:** 2026-11-01 · **Girdi:** KD-audit-2a'dan kalan 74 madde; 2b: 46 madde hükme bağlandı (hukum_1…18).

### Doğrulayıcılar (18; hepsi `[dogrulama]` Opus / high)

| Doğrulayıcı | Kapsam | Hüküm |
|---|---|---|
| 1 | Y13, Y14, Y15, Y16, Y3, Y8 | doğru 3 · kısmen 2 (Y16, Y8) · çürük 1 (Y3) |
| 2 · 3 · 4 · 5 | #83 · Y7 · #70 · Y5 | her biri doğru 1 |
| 6 | #46, #47, #48 | doğru 3 |
| 7 | #100, Y18, Y19 | doğru 2 · çürük 1 (Y19) |
| 8 | #51, #52, #53 | doğru 2 · kısmen 1 (#51; Orta → Düşük) |
| 9 | #54, #55 | doğru 2 |
| 10 | #103, #104, #105, Y1 | doğru 4 (Y1 yükseltildi) |
| 11 | #61, #62, #63 | doğru 3 |
| 12 | #64, #65 | doğru 1 · çürük 1 (#65) |
| 13 | #109…#112, Y20 | doğru 5 |
| 14 · 15 | #25, #26, #27 · #28, #29 | doğru 3 · doğru 2 |
| 16 | #88, #89, #90 | doğru 2 (#88 yükseltildi) · çürük 1 (#90) |
| 17 · 18 | #91 · #96, Y11 | doğru 1 · doğru 1 · çürük 1 (Y11) |

Planlanan iki doğrulayıcı (19: #97; 20: #98, Y10, Y12, Y2) eşzamanlı 20 sınırı nedeniyle yazılamadı; paketleri (`dogrulama_19.md`, `dogrulama_20.md`) duruyor, 5 madde **2c'ye** taşındı. Bekçi olayı: eşzamanlı sınır hatası 19 ve 20'yi saydı, yeniden denemeler engellendi.

**Toplam hüküm: 46 madde — 38 doğru · 3 kısmen · 5 çürük.** Seviye: #51 Orta → Düşük (uygulandı); Y1 ve #88 Düşük → Orta **yükseltildi**.

### Uygulama

39 madde uygulandı: **43 ESKİ → YENİ çifti** ve **45 hücre düzeltmesi** (`12_audit_uygula2.py`; yeni satır yok). Çakışmalar: hukum_1 Y8 ↔ hukum_17 #91 (aynı KP-6 atıf çifti; tek değişiklik) ve hukum_5 Y5 ↔ hukum_4 #70 (ödeme tutarı = iade tutarı cümlesi; iki şartname tek birleşik metinde). Uygulanan liste: `a10-kd-audit-2/2b/uygulanan.md`.

**Matris sayımı (betik):** 1.473 satır (değişmedi); Severity KRİTİK 610 → **611** · ORTA 731 → **730** · DÜŞÜK 132 (VAL-D051, #48); Kanıt ortamı `gerçek` 37 → **38** · `sandbox` 24 → **23** (VAL-E029, #104).

### Üst dokümana geri besleme (K-652)

Yok. `08` DV-30 için hükmün üst doküman notu koşulludur (tepe gün giriş sayısı kaynaktan konunca VAL-E031 yazılır); öneriyle kapı korunur (K-1553).

### Yükseltilenler (uygulanmadı → KD-audit-2r)

Y1 (VAL-B016: güvenlik kuralını tek başına sınar ama ORTA; iki yoldan biri) ve #88 (açık iletişim talebi ile durumu hiç değişmemiş ayıp talebinin "açılıştan" kolunu hiçbir satır sınamaz). Dosya: `a10-kd-audit-2/yeniden.md`.

### Kalan

105 maddenin 77'si hükme bağlandı (2a 31 · 2b 46); **kalan 28 madde** (2c; #97, #98, Y10, Y12, Y2 dahil) ve 2r (Y21, #95, Y1, #88). Kapılar: `12_kapsam_hizasi.py` 0 · `12_matris_denetle.py --kapsam 1-77` hata 0 (1.473 satır, 77/77 KP) · `12_atif_denetle.py` hata 0.

## Doğrulanmamış bulgular — KD-audit-2c

**Tarih:** 2026-11-01 · **Girdi:** KD-audit-2b'den kalan 28 madde (2b'den devreden #97, #98, Y10, Y12, Y2 dahil); 2c: 28 madde hükme bağlandı (hukum_1…18).

### Doğrulayıcılar (18; hepsi `[dogrulama]` Opus / high; iki dalga: 10 + 8)

Kademe: Orta 8 madde (#50, #56, #66, #67, #68, #69, #71, #87), Düşük 20 madde (14 bulgu + Y17, Y4, Y23, Y10, Y12, Y2). Paket 1–16 `--paket 2c`; 17 = #97, 18 = #98, Y10, Y12, Y2 (2b'den devreden).

**Toplam hüküm: 28 madde — 13 doğru · 11 kısmen · 4 çürük** (çürük: Y17, Y4, #114, Y12). Seviye: #56 ve #87 Orta → Düşük (**düşürüldü, uygulandı**); yükselen yok.

### Uygulama

24 madde uygulandı: **19 ESKİ → YENİ çifti** ve **13 hücre düzeltmesi** (`12_audit_uygula2.py`; yeni satır yok). Y23'ün aynı satırdaki çoklu SATIR'ı (VAL-A944, A826, A834) ve VAL-A195'in 4 SATIR'ı betikçe okundu; VAL-A013/A015 düzeltmesi Y10'da tek kez. Uygulanan liste: `a10-kd-audit-2/2c/uygulanan.md`.

**Matris sayımı (betik):** 1.473 satır (değişmedi); Severity KRİTİK 611 · ORTA 730 · DÜŞÜK 132 (değişmedi).

### Üst dokümana geri besleme (K-652)

`10` v0.61 (KD-59 ifadesi `02 §8.2.5` ve P-9'a hizalandı), `05` v0.46 (§10.1.8: yükleme diski tam %85'e getirirse de reddedilir). L4: `.claude/skills/validate/SKILL.md` adım 11 ve 12, `12` ve `00 §D.10` ile hizalandı. `PRODUCT_DISCOVERY_STATUS.md` §24.1'e K-1538 (4) açık noktası eklendi (K-1554).

### Yükseltilenler

Yok.

### Toplam ilerleme

**105 maddenin tamamı hükme bağlandı** (2a 31 · 2b 46 · 2c 28). Kalan: 2r — Y21, #95, Y1, #88 ve 2a'nın yeni YAN notları. Kapılar: `12_kapsam_hizasi.py` 0 · `12_matris_denetle.py --kapsam 1-77` hata 0 (1.473 satır, 77/77 KP) · `12_atif_denetle.py` hata 0.

Prosedür: [`.claude/skills/audit/SKILL.md`](../../.claude/skills/audit/SKILL.md)
