# Deep Review Raporu — 04 UI Specs

**Tarih:** 2026-10-04 · **Hedef:** `Docs/04_UI_SPECS.md` v0.12 (bulgular v0.12'de üretildi, v0.13'te işlendi; taban `main` = e585e6a) · **Bağlam:** `PRODUCT_DISCOVERY_STATUS.md` §2 (K-01…K-829), §10; `02_PRODUCT_REQUIREMENTS.md` v0.62; `03_USER_FLOWS.md` v0.18; `10_MVP_SCOPE.md` v0.41; `01_PROJECT_VISION.md` v0.33; `04` şablonu; resmî mevzuat metinleri (mevzuat.gov.tr, resmigazete.gov.tr, kvkk.gov.tr — 2026-10-04'te okundu) · **Odak:** Tam analiz (8 katman; wireframe düzeyinde — mekanizma Teknik Mimari'nin, veri Veri Modeli'nin, e-posta metni Entegrasyon Spesifikasyonu'nun işidir, GUARDRAILS §1)

### Koşum biçimi (K-431)

Audit raporuyla aynı koşumdur ([`04_AUDIT.md`](04_AUDIT.md), "Koşum biçimi"): dokuz mercek iki dalgada (beş + dört), salt okuma, bulgular bulundukça dosyaya; iki şüpheci (kanıt, bağlam); yönetici hükmü; uygulama temiz bağlamlı ayrı alt ajanda. Deep review'ın katmanları üç mercekte koştu: **durum × rol matrisi** (`DURUM` — Katman 2), **yasal uyum** (`YASAL` — Katman 4) ve **deep review** (`DR` — Katman 1, 3–8: tasarım kalitesi ve uygulanabilirlik, edge case, erişilebilirlik, MVP sınırı, manuel adım bütçesi, doğrulanabilirlik). Yasal dayanaklı her bulgu resmî metnin güncel hâline karşı doğrulandı — mercek, kanıt şüphecisi ve uygulayan ajan ayrı ayrı (günlük aşağıda).

**Karşı-doğrulamanın sonucu (bu raporun 37 bulgusu):** kanıt şüphecisi 29 doğru · 6 kısmi · 2 çürük; bağlam şüphecisi 25 kabul (3'ü tekrar) · 11 kısmi · 1 ret. Şiddet (çürük ve ret hariç): yüksek 2 · orta 10 · düşük 22. **Sonuç: 23 uygulandı · 11 kısmen uygulandı · 3 uygulanmadı.** On iki karar (K-830…K-835, K-839…K-844); yedisi ⚠.

**Karar modu (K-723):** ⚠ öneriyle kayıt — ⚠ grubundaki kararlar `(öneriyle kaydedildi — ⚠)` işaretiyle karar kaydı §10.1'in listesine girdi ve proje sahibine aşamanın arşiv işaretinden önce gösterilir. Çok kritik soru çıkmadı: bağlam şüphecisinin sınırda saydığı tek konu (YASAL-1 — cayma özeti) yönetici hükmüyle öneriyle kaydedildi; seçeneklerden biri yönetmeliğe aykırılık riski taşıdığı için öneri belirgindir ve bedeli küçüktür.

### Özet Skor Tablosu

| # | Katman | Skor | Kritik bulgu |
|---|---|---|---|
| 1 | Kapsam | ⚠ | Kaynağın bir alanı düşmüş (dijital üründe üretim yeri — YASAL-5, FORM-7); kaynaksız ekleme (sayaç = satır eşitliği — EKRAN-P-5); matriste eksik varyantlar (DURUM-8, DURUM-10) |
| 2 | Tutarlılık | ⚠ | Ayıp talebinin adet sınırı `02 §7.5.2`'yle çelişiyordu (DURUM-1, YASAL-3 → K-830 ⚠); ekran tanımının kapalı işlem listesi matrisle çelişiyordu (DURUM-2); rol adları konvansiyon 7'den ayrışıyordu (DURUM-12) |
| 3 | Teknik derinlik | ⚠ | Tanımsız ekran hâlleri: teyit ekranının yeniden açılması (DR-1 → K-840), az ürünlü vitrin (DR-5 → K-841), kurumsal içerik listesinin sayfalanması (DR-8 → K-843), kaydetme anında kapanmış oturum (DR-10 → K-844); belirsiz ifadeler (DR-9, DR-13) |
| 4 | Güvenlik ve yasal uyum | ⚠ | Mesafeli Sözleşmeler Yönetmeliği m.6/2-a (YASAL-1 → K-831 ⚠), m.6/1 (YASAL-2 → K-832 ⚠), m.7/1 teyidi (YASAL-10 → K-833 ⚠); ilk kurulumda çerez (YASAL-7 → K-834 ⚠); koşullu satış reklamı (YASAL-9 → K-835 ⚠); durum değiştiren bağlantı — OWASP A04 güvensiz tasarım (DR-3 → K-839 ⚠); WCAG 2.2 — 3.2.6 ödeme adımında (DR-6 → K-842) |
| 5 | Hata modu | ⚠ | E-posta güvenlik tarayıcısının geri alma bağlantısını açması meşru değişikliği geri alıyordu (DR-3); oturum düşmesinde yazılanların akıbeti (DR-10) |
| 6 | Veri akışı | ⚠ | Teslimat illeri değişikliği üretilen metinlerin sürümünü artırmıyordu (YASAL-4); özet farkında teyit eski içerikte kalıyordu (YASAL-10) |
| 7 | Ölçeklenebilirlik | ✓ | Liste büyümesi tek açıkta: elle sıralı kurumsal içerik grubu (DR-8 → K-843) |
| 8 | Bağımlılık riski | ✓ | Ödeme sağlayıcısının ve e-postanın kesintileri ekranlarda karşılıklı; ödeme yöntemi kalmadığında tetik netleşti (EKRAN-M-7, audit raporu) |

**Genel (v0.12 için):** ⚠ İyileştirme gerekli — Critical yok; iki yüksek bulgu (DURUM-1, YASAL-3) aynı kökün iki yüzüdür ve K-830 ile kapandı. **v0.13 sonrası:** 37 bulgunun 34'ü uygulandı ya da kısmen uygulandı, 3'ü uygulanmadı (iki çürük, bir ret). Sırada `04`'ün cross-review'ı var.

**Güçlü yönler (deep review kural 4):** şablonun sekiz alanı 53 ekranın hepsinde var (424/424); §6'nın kırk bir işlem kapsaması `03 §1.11` ile birebir (eksik 0, fazla 0); hata hâlleri üç düzeyli kalıpla sistematik, sonucu belirsiz işlem sonucu görmeye götürür; panel mobilde gerçekçi — sayaç → liste → sipariş en çok üç adım; manuel adım bütçesi sipariş ayrıntısında yeniden sayılmış ve yeni zorunlu adım yok; MVP sınırı korunmuş — kapsam dışı öğeler "yoktur" diye anılmış; ağır yasal yükümlülükler (ödeme yükümlülüğü düğmesi, işaretsiz kutular, cayma istisnaları, geri ödemenin başlangıcı, kanuni faiz uyarısı, indirimde referans fiyat) güncel resmî metinle uyumlu (YASAL envanteri: 73 ✓).

### Mercek skorları

| Katman | Skor | Mercek | Not |
|---|---|---|---|
| Katman 2 — Durum × rol | ⚠ | durum | Envanter 323 öğe (282 ✓, 34 ⚠, 7 ✗). Durum adları kaynakla birebir; kırk bir işlemin hepsi bir satırın Kaynak sütununda. Zayıf yan: kalem düzeyi işlemlerin satırlara dağılımı ve sipariş sayfasının izinli çiftleri |
| Katman 4 — Yasal uyum | ⚠ | yasal | Envanter 89 öğe (73 ✓, 7 ⚠, 9 ✗). Açıklar onay bölümünün biçim şartlarında ve kenar hâllerde |
| Katman 1, 3–8 — Deep review | ⚠ | dr | Envanter 458 öğe (427 ✓, 30 ⚠, 1 ✗ — Tutarlı Yardım ödeme adımında). Zayıf nokta adı konmamış edge case'ler |

### Bulgular (deep review mercekleri)

| # | Yer | Şiddet | Bulgu | Kanıt · Bağlam | Sonuç | Uygulama / karar |
|---|---|---|---|---|---|---|
| DURUM-1 | 5.19.3, 6.2.16.6/7/24, 6.2.19.1 (G-C) | yüksek | Ayıplı adedin üst sınırı "teslim edilmiş" adet — teslim işaretsiz kalemde talep açılamıyor | DOĞRU · KABUL | Uygulandı | Yönetici hükmü 6 — **K-830 (⚠)**: sınır talebe açık adettir, teslim işaretine bağlı değil; 5.19.3, 7.1.10.2; K-793'e geri işaret |
| DURUM-2 | 9.9.38 ↔ 6.3.9.8 (G-D) | orta | Ekran tanımı Teslim edildi'de açık işlemleri dar sayıyor | DOĞRU · KABUL | Uygulandı | 9.9.38 `03 §1.11`'e ve 6.3.9.8, 6.3.9.10'a bağlandı |
| DURUM-3 | 6.3.9.5, 9.9.5 | düşük | Kargodayken bütün fiziksel kalemleri kapanmış siparişin birincil düğmesi | KISMİ · KISMİ | Kısmen uygulandı | Ayrışma: kanıt "eksik yalnız matris satırı, teslim düğmesi yanlış değil" dedi; bağlam K-803'ün uygulanmasını önerdi. İkisinin kesişimi uygulandı: birincil düğme yalnız açık fiziksel kalem varken; yoksa birincil düğme yok, yol teslim edilemedi + 6.3.9.7 (K-803) — teslim işareti işlem olarak kaldı, yeni satır açılmadı |
| DURUM-4 | 6.2.16.4–6.2.16.8 | düşük | Kalem düzeyi işlemler (yeniden açma, IBAN) satırlarda tutarsız | DOĞRU · KABUL | Uygulandı | Bağlamın küçük seçeneği: blok girişine tek cümle, 6.2.16.8'den iki madde çıktı |
| DURUM-5 | 6.3.9.6 | düşük | Hizmet kalemine dokunan üç işlem yok | DOĞRU · KABUL | Uygulandı | Aksiyon ve Kaynak |
| DURUM-6 | 6.2.16.8 | düşük | "Açık fiziksel kalem yoktur" terimi kaynakla çelişiyor | DOĞRU · KABUL | Uygulandı | Gerekçe `03 §1.11.2`, §1.11.8 |
| DURUM-7 | 6.2.16.12, 6.2.16.14, 6.3.9.12 | düşük | "Kalemde işlem yok" IBAN alanıyla çelişiyor | DOĞRU · KABUL | Uygulandı | "IBAN alanı ve bekleyen geri ödeme hariç" |
| DURUM-8 | 6.2.4.2, 6.2.4.9, 5.4.6 | orta | Taslak varyant önizlemesi ve Arşiv varyant matriste yok | DOĞRU · KISMİ | Kısmen uygulandı | Var olan iki satır genişledi; 5.4.6'ya arşivlenmiş varyant (`02 §5.1`) — yeni satır yok |
| DURUM-9 | 6.1.5.1 | düşük | Duyuru şeridinin taslak hâli matriste yok | DOĞRU · KISMİ | Kısmen uygulandı | 6.1.5.1'e tek cümle; yeni madde açılmadı |
| DURUM-10 | 6.2.16.9, 6.2.16.10 | düşük | İptal edildi + Kısmen geri ödendi'nin kalıcı son hâli yok | DOĞRU · KISMİ | Kısmen uygulandı | 6.2.16.10'un Durum hücresi genişledi; "ne gösterilir"e yeni içerik eklenmedi |
| DURUM-11 | altı satırın Kaynak sütunu | düşük | Aksiyondaki işlemlerin kimliği Kaynak'ta yok | KISMİ · KABUL | Kısmen uygulandı | Beş satır tamamlandı; 6.2.16.7 kanıtın okumasıyla değişmedi (üç işlemi adıyla sınırlar) |
| DURUM-12 | 6.3.1.1–6.3.1.3, 6.2.16.27 | düşük | Rol adları konvansiyon 7'den ayrışıyor | DOĞRU · KABUL | Uygulandı | "Kullanıcı"; 6.2.16.27 "Ziyaretçi" |
| DURUM-13 | 6.3.9.7, işlem ekranlarının satırları | düşük | İki eksen biçimi eksik | DOĞRU · KABUL | Uygulandı | 6.3.9.7'ye ödeme ekseni; 6.1.2'ye işlem ekranı istisnası |
| DURUM-14 | 5.16.17 | düşük | Çift listesi üç çifti atlıyor | DOĞRU · KISMİ | Kısmen uygulandı | Liste kaldırıldı, matrise işaret (konvansiyon 4) |
| YASAL-1 | 5.13.9 | orta | m.6/2-a — (g), (h) bilgileri ödeme yükümlülüğünden hemen önce ayrıca gösterilmiyor | DOĞRU · KABUL | Uygulandı | Yönetici hükmü 3 — **K-831 (⚠)**: kutuların üstünde cayma özeti; `02 §3.24.3` (K-652); K-756'ya geri işaret |
| YASAL-2 | 5.13.32 | orta | m.6/1 — en az on iki punto şartı yok | DOĞRU · KABUL | Uygulandı | Yönetici hükmü 4 — **K-832 (⚠)**: değer `02 §3.24.3`'te, `04` işaret eder; e-posta yanı `08`'in park bloğuna |
| YASAL-3 | 5.19.3 (G-C) | yüksek | DURUM-1'in yasal yanı (6502 m.12/1) | DOĞRU · KABUL (tekrar) | Uygulandı | K-830; adedi bir olan kalemin açıkça yazılması dahil |
| YASAL-4 | 9.18.4, 9.18.9, 9.19.4 | orta | Teslimat illeri üretilen metinlerin sürümünü artırmıyor | DOĞRU · KABUL | Uygulandı | Üç yer; `03 §8.7.2.3`, §8.7.4.2 `02 §3.24.3` ve §3.33.3'e hizalandı (v0.19) — kural `02`'de olduğundan K satırı açılmadı |
| YASAL-5 | 5.4.4, 9.5.6, 9.5.22, 6.3.5.6 (G-E) | orta | Dijital üründe üretim yeri düşmüş (Fiyat Etiketi Yönetmeliği m.5/2-a) | DOĞRU · KABUL (tekrar) | Uygulandı | FORM-7 ile birlikte (audit raporu) |
| YASAL-6 | 2.12.1.6, 5.25, 5.26 | — | Adres defteri ve e-posta değişikliği formunda aydınlatma bağlantısı yok | KISMİ · RET | Uygulanmadı | Yönetici hükmü 1: `02 §3.33.5`'in listesi K-342 ve K-553'ün bilinçli kararıdır — "yeni kişisel veri kategorisi" okuması; adres ve e-posta kayıt ve onay adımında zaten toplanan kategorilerdir, altbilgi bağlantısı her sayfada |
| YASAL-7 | 5.11.9 | düşük | İlk kurulumda çerez politikası yayında değilken çerez yazılıp yazılmadığı yok | KISMİ · KISMİ | Kısmen uygulandı | **K-834 (⚠)**: ilk yayına kadar çerez yazılmaz — K-770'in netleştirmesi; çerez politikasının yayını vitrine koşul yapılmadı; doğrulama `05` ve `12`'nin park bloklarına |
| YASAL-8 | 5.25.8 | düşük | "bölüm forma giden yolu söyler" belirsiz | DOĞRU · KISMİ | Kısmen uygulandı | Yan cümle silindi; E-25'e yeni satır tanımlanmadı |
| YASAL-9 | 5.12.6, 5.13.10 | düşük | Ticari Reklam Yönetmeliği m.14/7 — koşullu satış reklamı değerlendirilmemiş | DOĞRU · KISMİ | Kısmen uygulandı | **K-835 (⚠)** değerlendirme kaydı; `02 §3.19.5`'e bir cümle; `04` değişmedi |
| YASAL-10 | 5.13.20, 2.12.7.3 | orta | Özet farkında kutuların işareti yalnız sürüm değişiminde kalkıyor (m.7/1) | KISMİ · KABUL | Uygulandı | **K-833 (⚠)**: K-756'nın netleştirmesi — `03 §2.4.8` ve `10` KP-14 zaten böyle diyordu; `02`/`03` değişmedi |
| DR-1 | 5.14.10, 6.2.14 | orta | Teyit ekranının yeniden açılması tanımsız | DOĞRU · KABUL | Uygulandı | **K-840**: sağlayıcıya geçiş sunulmaz; 5.14.1, 5.14.10, 3.3.39, 6.2.14.5 |
| DR-2 | 5.16.4 | — | Yavaş ödeme dönüşünde güncellemenin biçimi belirsiz | ÇÜRÜK · KISMİ | Uygulanmadı | Kanıt: davranış 5.16.4 ve 2.7.2'de yazılı ("sonuç gelince güncellenir"), beklemenin sonu `03 §2.5.1.4` ve 6.2.16.2'de; mekanizma Teknik Mimari'nin (yönetici hükmü 1) |
| DR-3 | 5.28.5, 9.25 | düşük | Geri alma bağlantısının açılması geri alıyor — e-posta tarayıcısı meşru değişikliği geri alabilir | KISMİ · KABUL | Uygulandı | Yönetici hükmü 5 — **K-839 (⚠)**: tek düğme; E-28 ve E-54; `02 §3.13.14`, `03 §9.3.6` (K-652); `05` park satırı (GET durum değiştirmez); K-782 ve K-819'a geri işaret |
| DR-4 | 2.2.2 | — | Duyuru şeridi "tek satırlık" — kesilir mi kırar mı | ÇÜRÜK · KABUL | Uygulanmadı | Kanıt: "bir satırlık" `02 §3.27.21`'in kendi ifadesidir ve içeriğin türünü anlatır; 8.3.2'nin genel kuralı ("kesilmez, satır kırar") soruyu cevaplar (yönetici hükmü 1) |
| DR-5 | 5.1.13 | orta | Eksik satır kuralı az ürünlü vitrinde bütün kartları gizliyor | DOĞRU · KABUL | Uygulandı | **K-841**: kural en az bir tam satırda işler |
| DR-6 | 2.1.4, 2.2.5, 3.1.21 | düşük | Ödeme adımında iletişim formuna bağlantı yok (WCAG 2.2 — 3.2.6) | DOĞRU · KABUL | Uygulandı | **K-842**: altbilginin "İletişim" başlığı E-10'a; 5.10.1 |
| DR-7 | 5.4.11, 5.14.7, 5.16.14 | düşük | Kopyalamanın sonucu kısa süreli bildirimle — K-743'ün sınırını aşıyor | DOĞRU · KISMİ | Kısmen uygulandı | Sonuç düğmenin yanında yerinde (2.3.1.2); §2.3.1.4'e istisna eklenmedi, yeni tırnaklı metin yazılmadı |
| DR-8 | 9.13.3 | düşük | Elle sıralı kurumsal içerik listesinin sayfalanması tanımsız | DOĞRU · KABUL | Uygulandı | **K-843**: gruplar sayfalanmaz |
| DR-9 | 5.17.15 | düşük | "kalem listesi görsel ve adla kısalır" belirsiz | DOĞRU · KABUL | Uygulandı | 5.16.21'in kalıbı |
| DR-10 | 3.6.5 | orta | Kaydetme anında kapanan oturumda yazılanların akıbeti yok | DOĞRU · KISMİ | Kısmen uygulandı | **K-844**: form gönderilmez, korunmayacağı söylenir; taslak saklama yeteneği açılmadı |
| DR-11 | 2.7.1.2, 5.12.16, 5.27.10 (G-K) | düşük | "Ürünlere dönüş"ün hedefi tanımsız | DOĞRU · KABUL | Uygulandı | 2.7.1.2'de tek tanım (K-754) |
| DR-12 | 9.18.13 (G-B) | düşük | "0 TL" | DOĞRU · KABUL (tekrar) | Uygulandı | Audit raporu, KAPSAM-K-3 |
| DR-13 | 5.14.2 | düşük | İlk görülen alanında birincil eylem yok | DOĞRU · KABUL | Uygulandı | Kartta sağlayıcıya geçiş; havalede birincil düğme yok |

### Aksiyon Planı

- **Critical:** yok. **High:** DURUM-1 ve YASAL-3 — v0.13'te K-830 ile kapandı.
- **Medium → `04`:** v0.13'te kapandı — K-831…K-833 (onay bölümü), YASAL-4, YASAL-5, DURUM-2, DURUM-8, DR-1 (K-840), DR-5 (K-841), DR-10 (K-844).
- **Medium → `02`:** v0.63 — §3.13.14 (K-839), §3.19.5 (K-835), §3.24.3 (K-831, K-832), §7.3.5 (K-838). **→ `03`:** v0.19 — 9.3.6 (K-839), 8.7.2.3 ve 8.7.4.2 (YASAL-4 hizalaması). **→ sonraki dokümanlar:** `05` — K-834 (ilk kurulumda çerez), K-839 (geri alma bağlantısının açılması durum değiştirmez) · `08` — K-832 (yasal metinlerin e-postadaki okunabilirliği) · `12` — K-834, K-798 (uyarıların göründüğü hâller), K-808, K-823; K-756'nın park satırı K-831 ve K-833'le güncellendi.
- **Low:** v0.13'te kapandı ya da kısmen uygulandı; DR-2, DR-4 (çürük) ve YASAL-6 (ret) işlenmedi.

### Cross-reference Haritası

| `04` bölümü | `02` | `03` | `10` | Durum |
|---|---|---|---|---|
| Başlık notu ve konvansiyonlar | §3 girişi | §0.3 | — | ✓ (v0.13 — konvansiyon 1, 9, 11; ATIF-5…ATIF-8) |
| §1 İzlenebilirlik matrisi | §1–§12 | 793 satır | KP-1…KP-77 | ✓ (1.193 satır; iki satırdan E-16 çıktı — EKRAN-M-4) |
| §2 Ortak bileşenler | §3.11.6, §3.24, §3.34.5 | §1.11, §2.4.8 | — | ✓ (v0.13 — K-831, K-833, K-842, K-845) |
| §3 Navigasyon | §3.30.4, §3.34.5 | §10.1.1.2 | — | ✓ (3.3.38, 3.3.39 girdi; 3.1.21, 3.3.12, 3.3.25, 3.4.18) |
| §5 Müşteri ekranları | §3.8.5, §3.13.14, §3.24.3, §7.3.5, §7.5.2 | §2.4.9, §2.8.1.2, §2.10.3, §9.3.6 | KP-14 | ✓ (v0.13 — K-830…K-834, K-837…K-842, K-845) |
| §6 Durum × rol | §5 | §1.4.1, §1.4.3, §1.11 | — | ✓ (283 satır; 6.2.14.5 girdi) |
| §7 Form ve validasyon | §3.8.5, §3.9.1, §3.10.1, §8.2 L-9 | §3.2.1.12 | — | ✓ (165 alan satırı; 7.1.12.4 girdi) |
| §8 Lokalizasyon | §6.2.8, §6.2.9 | — | — | ✓ (8.2.9 — hitap) |
| §9 Panel ekranları | §3.24.3, §3.33.3, §10.7.4 | §8.5.6, §8.8.6 | — | ✓ (v0.13 — K-836, K-839, K-843, K-846) |

### Mevzuat doğrulama günlüğü

Her satır: mevzuat · madde · kaynak · okuyan · tarih · sonuç.

| Mevzuat | Madde | Kaynak | Okuyan · tarih | Sonuç |
|---|---|---|---|---|
| Mesafeli Sözleşmeler Yönetmeliği (RG 27/11/2014-29188; değişik RG 23/8/2022-31932, RG 24/5/2025-32909; Danıştay 10. D. 6/5/2026 iptali işlenmiş konsolide metin) | m.5/1 (a, d, g, h), m.6/1, m.6/2-a, m.7/1, m.8/1, m.13/1 | mevzuat.gov.tr GeneratePdf?mevzuatNo=20237 | YASAL merceği, kanıt şüphecisi ve uygulayan ajan ayrı ayrı · 2026-10-04 | Doğrulandı — K-831 (m.6/2-a; m.5/1-g "satıcının iade için öngördüğü taşıyıcıya ilişkin bilgiler", -h), K-832 (m.6/1 "en az on iki punto"), K-833 (m.7/1 "sözleşme kurulmamış sayılır"), K-838 (m.13/1 — süre bildirimden işler) |
| Aynı yönetmelik | m.12/5, m.13/3 (mülga), m.15/1-ı, j, k (Danıştay iptali) | aynı | YASAL merceği · 2026-10-04 | Etki yok — `02 §7.3.4`'ün sebep listesi iptal edilen bentleri kullanmıyor; taşıyıcısız iade kuralı `04` 5.18.5 ile uyumlu |
| Elektronik Ticaret Aracı Hizmet Sağlayıcı ve Elektronik Ticaret Hizmet Sağlayıcılar Hakkında Yönetmelik (RG 29/12/2022-32058; değişik RG 8/3/2025) | m.5/1, m.7/1, m.8/1-b ve d, m.9/1 | mevzuat.gov.tr MevzuatMetin/yonetmelik/7.5.39927.pdf | YASAL merceği · 2026-10-04 | Uyumlu — kimlik seti, işlem rehberi, sipariş teyidi |
| Elektronik Ticaret Bilgi Sistemi ve Bildirim Yükümlülükleri Hakkında Tebliğ (RG 11/8/2017-30151) | m.5, m.6/1 | mevzuat.gov.tr GeneratePdf?mevzuatNo=23842 | YASAL merceği · 2026-10-04 | Bandın sitede gösterimini zorunlu kılan hüküm yok — `02 §3.1.4`'ün kuralı ürün kararıdır |
| 6502 sayılı Tüketicinin Korunması Hakkında Kanun (AYM 12/2/2026 iptali işlenmiş) | m.3/1-h, m.12/1, m.48/2–4 | mevzuat.gov.tr MevzuatMetin/1.5.6502.pdf | YASAL merceği, kanıt şüphecisi · 2026-10-04 | Doğrulandı — K-830 (m.12/1 — iki yıllık süre teslimden işler); YASAL-5 (m.3/1-h — gayri maddi mal) |
| Fiyat Etiketi Yönetmeliği (değişik RG 11/10/2025-33044) | m.5/1–8, m.11/1–2 | mevzuat.gov.tr GeneratePdf?mevzuatNo=19819 | YASAL merceği, kanıt şüphecisi · 2026-10-04 | Doğrulandı — m.5/2-a "malın üretim yeri" (YASAL-5); m.11/1 `02 §3.9.3` ile birebir |
| 6698 sayılı KVKK · Aydınlatma Tebliği (RG 10/3/2018-30356) | m.10/1 · Tebliğ m.4/1, m.5/1 | mevzuat.gov.tr MevzuatMetin/1.5.6698.pdf, GeneratePdf?mevzuatNo=24454 | YASAL merceği, kanıt şüphecisi · 2026-10-04 | Uyumlu — YASAL-6 reddedildi (liste bilinçli karar) |
| KVKK Çerez Uygulamaları Hakkında Rehber (Kurum, 2022) | zorunlu çerez (Kriter A/B); "en geç internet sitesine girildiği anda aydınlatma" | kvkk.gov.tr (PDF) | YASAL merceği okudu · 2026-10-04; kanıt şüphecisi yalnız ikincil kaynaklarla teyit etti | K-834'ün dayanağı; altbilgi bağlantısının yeterliliği Rehber'de açıkça yazılı değil — doğrulanamadı (yorum), ✓ sayıldı |
| Ticari Reklam ve Haksız Ticari Uygulamalar Yönetmeliği (değişik RG 1/2/2022-31737, RG 1/7/2026-33297) | m.14/1, m.14/3, m.14/5, m.14/7 | mevzuat.gov.tr GeneratePdf?mevzuatNo=20435; resmigazete.gov.tr 20260701-9 | YASAL merceği, kanıt şüphecisi ve uygulayan ajan · 2026-10-04 | Doğrulandı — m.14/7 koşullu satış reklamına m.14'ü uygular; eşik satırının durumu yorum gerektirir → K-835 (⚠) |
| 6563 sayılı Elektronik Ticaretin Düzenlenmesi Hakkında Kanun | m.3/1–4, m.4/1–2 | mevzuat.gov.tr MevzuatMetin/1.5.6563.pdf | YASAL merceği · 2026-10-04 | Uyumlu |
| Veri Sorumlusuna Başvuru Usul ve Esasları Hakkında Tebliğ | başvuru yolları | kvkk.gov.tr duyurusu | YASAL merceği · 2026-10-04 | Kısmen doğrulandı — konsolide metin açılmadı |

### Okunmayan ya da kesilerek okunan kısımlar (merceklerin beyanı)

- **DURUM:** §6 tam; §5.16, §9.9, §5.19 tam; öteki ekranların aksiyon maddeleri yalnız §6 satırları üzerinden; `02 §5`'in iki uzun satırının sonu kesik; karar kaydından yalnız K-793.
- **YASAL:** tam okunan — §2.1, §2.2, §2.11, §2.12, §5.4, §5.5, §5.10–§5.28, §8.2, §9.9, §9.12, §9.16–§9.19, §9.23–§9.24; grep ve dilimle — §2.5.6, §6.2.19, §6.2.25, §6.3.5.6, §7.1.10, §9.5. **Okunmayan:** §1, §3, §6–§7'nin kapsam dışı satırları, panelin öteki ekranları (E-29…E-36, E-38, E-39, E-41…E-43, E-45, E-49…E-51, E-54 — yalnız göz atıldı).
- **DR:** başlık notu, §2, §3, §4, §5, §8, §9 tam; §6'dan yalnız 6.2.14; §1 ve §7 okunmadı (öteki merceklerin işi).

### Karar listesi

| Karar | Bulgu | Özet | ⚠ |
|---|---|---|---|
| K-830 | DURUM-1, YASAL-3 | Ayıplı adedin üst sınırı talebe açık adettir; teslim işaretine bağlı değil (K-793'ün netleştirmesi) | ⚠ |
| K-831 | YASAL-1 | Onay kutularının hemen üstünde formdan ayrı kısa bir cayma özeti (`02 §3.24.3`) | ⚠ |
| K-832 | YASAL-2 | İki yasal metin ekranda ve e-postada en az on iki punto karşılığı, küçültülmeden (`02 §3.24.3`) | ⚠ |
| K-833 | YASAL-10 | Özet farkı formun içeriğini değiştirince iki yasal metnin kutuları yeniden işaretlenir (K-756'nın netleştirmesi) | ⚠ |
| K-834 | YASAL-7 | Çerez politikasının ilk yayınına kadar vitrin çerez yazmaz (K-770'in netleştirmesi) | ⚠ |
| K-835 | YASAL-9 | Eşik satırı süreli indirim duyurusu değildir; tarih alanı açılmaz (`02 §3.19.5`) | ⚠ |
| K-839 | DR-3 | Geri alma bağlantısı açılınca geri almaz; ekrandaki tek düğmeyle (K-782'den ayrılır) | ⚠ |
| K-840 | DR-1 | Yeniden açılan teyit ekranı ödeme beklemeyen siparişte sağlayıcıya geçişi sunmaz | — |
| K-841 | DR-5 | Eksik satır kuralı yalnız en az bir tam satırda işler (K-755'in netleştirmesi) | — |
| K-842 | DR-6 | Altbilginin "İletişim" başlığı her sayfada E-10'a götürür | — |
| K-843 | DR-8 | Kurumsal içerik listesinin grupları sayfalanmaz | — |
| K-844 | DR-10 | Kaydetme anında kapanmış oturumda form gönderilmez, korunmayacağı söylenir | — |

Audit merceklerinden doğan beş karar (K-836, K-837, K-838 ⚠; K-845, K-846) [`04_AUDIT.md`](04_AUDIT.md)'dedir. Adımın toplamı: on yedi karar, onu ⚠; karar kaydının ⚠ listesi otuz dokuza çıktı.
