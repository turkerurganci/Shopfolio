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

Prosedür: [`.claude/skills/audit/SKILL.md`](../../.claude/skills/audit/SKILL.md)
