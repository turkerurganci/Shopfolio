# Audit Raporu — 07 API Design

**Tarih:** 2026-10-19 · **Hedef:** `Docs/07_API_DESIGN.md` v0.15 (bulgular v0.15'te üretildi, v0.16'da işlendi; taban `main` = d04d066) · **Bağlam:** `PRODUCT_DISCOVERY_STATUS.md` §2 (Aşama 6 kararları K-1144…K-1198), §16; `02_PRODUCT_REQUIREMENTS.md`, `03_USER_FLOWS.md`, `04_UI_SPECS.md`, `05_TECHNICAL_ARCHITECTURE.md`, `06_DATA_MODEL.md`, `08`, `10_MVP_SCOPE.md`; `09`, `12`, `DEPLOY_RUNBOOK`'un Aşama 6 park blokları · **Odak:** Tam denetim — audit mercekleri ve güvence mercekleri bu raporda (ayrı dosya yok)

### Koşum biçimi (K-431, K-1145; `audit` skill'i, "Koşum biçimi")

Dokümanı yazan bağlamda koşmadı. Adım ajanı (Opus / medium) mercekleri, doğrulayıcıları ve uygulayıcıları **ön planda** başlattı. **Maliyet düzeni (K-1145)** skill'in koşumunu şöyle değiştirdi:

- **On altı mercek dört dalgada, hepsi Opus / medium.** Dalga 1 — K1a, K1b, K2, K3, K4 · dalga 2 — K5, K6, AT, IT, CR · dalga 3 — YU, TE, G1, G2, G3 · dalga 4 — M. Audit mercekleri: **K1a** başlık notu, park blokları, §1'in girişi, §1.2, §1.3 · **K1b** §2, §3, §7, §8 ↔ workshop ve yazım 1 kararları · **K2** §4.1–§4.16 (vitrin) · **K3** §4.17–§4.34 (işlem ekranları, hesap, ödeme dönüşü, makine uçları) · **K4** §9 girişi ve §9.1–§9.12 · **K5** §9.13–§9.26 · **K6** §5, §6, §10 · **AT** atıflar · **IT** iç tutarlılık · **CR** çapraz referans · **YU** yasal uyum · **TE** terim ve enum · **M** matris hücreleri. Güvence mercekleri: **G1** §2, §3, §5–§8, §10 · **G2** §4 · **G3** §9.
- **Doğrulama.** Her bulguyu **tek bir Sonnet / medium** doğruladı (skill'in iki şüphecisi yerine; sekiz parti B1–B8). **Opus / medium ikinci doğrulayıcı** yalnız yüksek ya da orta önemli bulgulara ve Sonnet'in çürüttüklerine açıldı (üç parti O1–O3, 25 bulgu). Yasal dayanaklı üç bulgunun (K3-2, YU-1, YU-2) hükmü **Opus / high** (H1) — mevzuat resmî metne yeniden okundu. İki doğrulayıcının ayrıştığı yedi bulgunun son hükmü adım ajanınındır (maliyet düzeni madde 8; aşağıda "Yürütücünün hükümleri"): K1b-1, IT-1, K2-1, K2-3, K3-1, K5-3, G3-3.
- **Uygulama.** Altı Sonnet / medium uygulayıcı sırayla aynı çalışma ağacında: **A1** başlık notu, §2, §3, §7, §8 · **A2** §4 · **A3** §5, §6, §9 · **A4** matris (§1.1) ve §1.2 · **A5** üst dokümanlar, park satırları ve karar kaydı satırları · **A6** §1.3'ün geri besleme tablosu ve iki matris hücresi. Her uygulayıcı sonucunu `apply\A<n>.md` günlüğüne bulduğu anda yazdı.
- **Mekanik denetimler (betikle; mercekler başlamadan doğrulandı).** `extract.py` (dilimler) · `atif.py` (K atfının kayıtta varlığı, K kolu, UC ve E kimlikleri, `§` atıfları — yazım konvansiyonu 5'in dizi kuralıyla; bozulmuş bir kopyada yedi enjeksiyonun hepsini yakaladı: K-1162 (77), K-1170 (88), §2.99.1, `04 §5.99`, `02 §3.99.9`, UC-200, E-77; 108 SEC eşleşmesinin 100'ü yanlış pozitif, 8'i gerçek → AT-1, AT-2; uygulamadan sonra yeniden koşuldu: gerçek sekiz kalktı, SEC 108 → 102, uygulamanın açtığı bir dizi hatası — `05 §4.2.2` (2), §4.2.3 — yürütücü tarafından düzeltildi) · `defcheck.py` (180 uç tanımının alan kalıbı: 10 blok bilinçli okunuş, belirsiz sözcük sıfır) · `gscan.py` (güvence envanteri, 1.261 birim; ilk sürümü "tek yoldan", "her liste", "yoktur" gibi kalıpları kaçırdı, numaralı maddelerle karşılaştırma buldu ve desen genişletildi: 791 → 1.261; birim üretmeyen 35 numaralı madde merceğe ayrıca verildi) · matrisin §1.1.1 betiği (önce/sonra; değişmemesi beklenen kümeler `cmp` ile aynı — yalnız `geri07`, `tab07` ve özet değişti; çiftler 5.056 → 5.071).
- **Ölçüm.** Mercekler 16 · Opus / medium · ~4,14 M token; Sonnet doğrulayıcılar 8 · ~1,00 M; Opus ikinci doğrulayıcılar 3 · ~0,45 M; Opus / high hüküm 1 · ~0,13 M; uygulayıcılar 6 · Sonnet / medium · ~0,95 M; rapor taslağı 1 · Sonnet / medium · ~0,18 M.

**Yürütücünün hükümleri (kısa):** çürütüldü, uygulanmaz — **K1a-3**, **G3-2** (Sonnet ve Opus birlikte). **Tek düzeltmede birleşenler** — K1b-1 + IT-1 · K3-3 + CR-2 · K4-4 + K5-6. **Uygulanmaz, deep review'a devredildi** — **IT-6**. Seviye değişimleri (Opus, yürütücü onayıyla): K2-1 High, K1b-1 / IT-1 / K2-3 / G3-3 Medium, K5-3 Low, IT-2 ve K5-2 Low. **K5-3**'te karar gerekmedi: bekleyen kayıt dışa aktarma dosyasına girer (K-823; `02 §3.13.4`). **YU-2**'de Sonnet'in "UC-174'e matris eki" yan yüzü alınmadı (H1). **TE-2**'de Sonnet'in (B) yolu seçildi.

**Sonuç:** 89 ham bulgu → Sonnet: **64 doğrulandı · 23 kısmen doğrulandı · 2 çürütüldü** → doğrulanmış seviye **High 4 · Medium 17 · Low 66** (87 bulgu; çürütülen iki bulgu sayılmaz) → **86 uygulandı** (üç çift tek düzeltmede: K1b-1 + IT-1, K3-3 + CR-2, K4-4 + K5-6; G2-3 iki aşamada — A1 kısmen, A2 tamamladı) · **1 uygulanmadı** (IT-6 — deep review'a devredildi) · **2 çürütüldü** (K1a-3, G3-2). Audit mercekleri 77 ham bulgu (76 doğrulanmış: 3/15/58), güvence mercekleri 12 (11 doğrulanmış: 1/2/8). **Çok kritik soru çıkmadı.** **Sekiz karar** (K-1199…K-1206; ikisi ⚠: K-1199, K-1202). API Tasarımı v0.16; Ürün Gereksinimleri v0.91, Arayüz Tanımları v0.38, Teknik Mimari v0.27, Veri Modeli v0.29'a geri beslendi; `09`, `12`, `DEPLOY_RUNBOOK` park satırları güncellendi. Yeni numaralı madde açılmadı.

> **Sayımın denetimi.** Bulgu ve hüküm sayıları betikle toplandı (merceklerin dosyaları ↔ sekiz doğrulama partisi: 89 ↔ 9 + 7 + 8 + 13 + 10 + 13 + 17 + 12 = 89). Toplama betiğinin ilk sürümü kimlik deseninde `K1b`'yi tanımıyordu ve beş bulguyu düşürdü (84); rapor taslağı hüküm dosyalarıyla karşılaştırırken buldu, sayılar düzeltilmiş hâlleridir.

### Envanter Özeti

Her satır merceğin dosyasındaki envanter tablosunun toplamıdır (Faz 1 = Faz 2; ✓ / ⚠ / ✗ eşleştirmesi; "kapsam dışı" başka merceğin kapsadığı öğedir). "Doğrulanan" çürütülmeyen bulguları, **seviye yürütücünün hükmüyle** sayar. IT merceğinin envanteri §2'nin 81 numaralı maddesi, 12 ölçülebilirlik öğesi ve 3 bölümler arası karşılaştırmadır (96).

| Mercek | Envanter | Eşleştirme | Ham bulgu | Doğrulanan (H/M/L) | Çürütülen |
|---|---|---|---|---|---|
| K1a kapsama — başlık notu, park blokları, §1 giriş, §1.2, §1.3 | 201 | 176 ✓ · 3 ⚠ · 1 ✗ · 21 kapsam dışı | 4 | 3 (0/1/2) | 1 (K1a-3) |
| K1b kapsama — §2, §3, §7, §8 ↔ workshop ve yazım 1 | 208 | 194 ✓ · 6 ⚠ · 0 ✗ · 8 kapsam dışı | 5 | 5 (0/2/3) | 0 |
| K2 kapsama — §4.1–§4.16 (vitrin) | 281 | 266 ✓ · 8 ⚠ · 1 ✗ · 6 kapsam dışı | 7 | 7 (1/2/4) | 0 |
| K3 kapsama — §4.17–§4.34 | 259 | 246 ✓ · 13 ⚠ · 0 ✗ | 8 | 8 (0/3/5) | 0 |
| K4 kapsama — §9 girişi, §9.1–§9.12 | 419 | 381 ✓ · 11 ⚠ · 0 ✗ · 27 kapsam dışı | 5 | 5 (1/1/3) | 0 |
| K5 kapsama — §9.13–§9.26 | 290 | 279 ✓ · 11 ⚠ · 0 ✗ | 8 | 8 (0/0/8) | 0 |
| K6 kapsama — §5, §6, §10 | 404 | 388 ✓ · 12 ⚠ · 0 ✗ · 4 kapsam dışı | 6 | 6 (0/2/4) | 0 |
| AT atıflar | 392 | 370 ✓ · 22 ⚠ · 0 ✗ | 4 | 4 (0/0/4) | 0 |
| IT iç tutarlılık | 96 | 86 ✓ · 10 ⚠ · 0 ✗ | 8 | 8 (0/2/6) | 0 |
| CR çapraz referans | 423 | 406 ✓ · 5 ⚠ · 0 ✗ · 12 kapsam dışı | 5 | 5 (1/2/2) | 0 |
| YU yasal uyum | 496 | 486 ✓ · 7 ⚠ · 0 ✗ · 3 kapsam dışı | 2 | 2 (0/0/2) | 0 |
| TE terim ve enum | 576 | 546 ✓ · 30 ⚠ · 0 ✗ | 8 | 8 (0/0/8) | 0 |
| M matris hücreleri | 156 | 149 ✓ · 7 ⚠ · 0 ✗ | 7 | 7 (0/0/7) | 0 |
| **Audit toplamı** | | | **77** | **76 (3/15/58)** | **1** |
| G1 güvence — §2, §3, §5–§8, §10 | 403 (368 birim + 35 satır) | 154 ✓ · 9 ⚠ · 0 ✗ · 240 güvence değil | 4 | 4 (1/0/3) | 0 |
| G2 güvence — §4 | 396 birim | 388 ✓ · 8 ⚠ · 0 ✗ (161 güvence birimi, 235 güvence değil) | 3 | 3 (0/1/2) | 0 |
| G3 güvence — §9 | 497 birim | 490 ✓ · 7 ⚠ · 0 ✗ (145 güvence birimi, 352 güvence değil) | 5 | 4 (0/1/3) | 1 (G3-2) |
| **Güvence toplamı** | 1.261 birim + 35 satır | | **12** | **11 (1/2/8)** | **1** |
| **Genel** | | | **89** | **87 (4/17/66)** | **2** |

Kapsama sonucunun özü: Aşama 6'nın 55 kararının (K-1144…K-1198) kolları büyük çoğunlukla gövdede bölüm numarasıyla kapanmış; §1 matrisinin bütün kümeleri baz sayımla aynı (yalnız bildirilen beş satır eksik aday aldı); 180 uç tanımının hepsi alan kalıbına uyar (10 blok bilinçli okunuş). Bulgular yazımın kenarlarındadır: genel kuralların (§2) uç tanımlarındaki kenar durumlarla uyuşmaması (kayıt varyantının çakışması, 303/422 ayrımı, çift gönderim listesi), özet yerlerinin (§1.2, §1.3, harita, Kaynak satırları) eksik sayımı, üst dokümanlarla iki ya da üç yerde kopan mekanizma (ödeme çerezi, geri ödeme ekseni, E-mail değişikliği limiti) ve güvencelerin yalnız veri katmanında tutup yanıt zamanında ya da eşzamanlı ikinci istekte tutmayan beş dar kenarı.

### Bulgular (audit ve güvence mercekleri)

"Seviye" mercek → Sonnet → Opus (Opus yalnız ikinci doğrulayıcı açılan bulgularda; son seviye yürütücünün hükmüyle kalın). "Doğrulayıcılar" Sonnet / Opus hükmü. "Sonuç" uygulayıcı günlüğündendir; "+ üst doküman" — düzeltmenin bir yüzü geri beslendi. Tam metin, gerekçe ve loglar adım ajanının çalışma dizinindeydi; bu tablo onların özetidir.

| Bulgu | Seviye | Doğrulayıcılar | Bulgu (özet) | Sonuç | Karar · yer |
|---|---|---|---|---|---|
| K1a-1 | Medium → Medium → Medium | doğrulandı / doğrulandı | §6.4.3'ün üç kalan riski "Doğrulama Protokolü senaryo olarak sınar" diyor, `12`'nin park bloğunda yok | uygulandı + üst doküman | `12` K-1196 · K-1198 satırı; K-1198 etki sütunu |
| K1a-2 | Low → Low | doğrulandı | Alt bölüm haritasının §1.3 satırı geri besleme satırı sayısını eski veriyor | uygulandı | başlık notu, harita §1.3 satırı |
| K1a-3 | Low → bulgu değil → bulgu değil | çürütüldü / çürütüldü | Yazım durumu "yedi oturumda (K-1150…K-1161)" der; K-1150 oturum değil plan önerisi | çürütüldü | — (K-1150 matrisin oturumlarını kuran karar) |
| K1a-4 | Low → Low | doğrulandı | §3.1 "Okunuş" tek alt bölüm olarak Kaynak satırı taşımıyor (konvansiyon 8) | uygulandı | §3.1 sonu (AT-4 ile tek) |
| K1b-1 | Medium → Low → **Medium** | kısmen / doğrulandı | §2.4.5 "gösterilen değere karşı tutmama aynı mesajdır" der, dört `Original` alanının üçü farklı davranır | uygulandı | §2.4.1, §2.4.3, §2.4.5, Kaynak (IT-1 ile tek düzeltme, O1'in 5 adımı) |
| K1b-2 | Medium → Medium → Medium | doğrulandı / doğrulandı | §2.7.1 422 listesi "gösterilen değerin reddi"ni sayar; o hâl 409'dur | uygulandı | §2.7.1 (422 / 409 ayrımı); K-1168 geri işareti |
| K1b-3 | Low → Low | kısmen | UC-80 Sınır hücresi hız sınırı ile 64 KB tavanını tek ifadede birleştiriyor | uygulandı | §3.2 UC-80 |
| K1b-4 | Low → Low | doğrulandı | §2 tablo 1. satır istisnayı §2.1.2'ye bağlar, istisna §2.1.7 / §2.1.9'dadır | uygulandı | §2 tablo satır 1 |
| K1b-5 | Low → Low | doğrulandı | §2.2.5 GET istisna listesinde iki madde arasında ayırıcı yok | uygulandı | §2.2.5 |
| K2-1 | **High** → Medium → High | doğrulandı / doğrulandı | Yayındaki ürünün taslak varyantı yönetici önizlemesinde seçilebilir; §4.4.1'in istek ve yanıtı bunu yazmıyor | uygulandı | §4.4.1; K-1178 (2) netleştirmesi |
| K2-2 | Medium → Medium → Medium | doğrulandı / doğrulandı | Aynı onay anahtarıyla ikinci gönderim sipariş sayfasına gider; kart siparişinde teyit ekranı görünmez | uygulandı + üst doküman | **K-1199 ⚠** · UC-27, §2.5.1, K-893 satırı; `05 §4.5.1`, `06`, `12` |
| K2-3 | Medium → Low → **Medium** | kısmen / doğrulandı | UC-29'un bağlantı hatası sepete döner ama bir kez söylenen mesajın taşıyıcısı yok | uygulandı + üst doküman | **K-1200** · UC-19, UC-29, §2.2.3; `04` 5.12.15, 6.2.12.6; `06 §4.15` |
| K2-4 | Low → Low | kısmen | Girişten ödeme adımına dönüşte birleşme mesajı ve §4.34.3 işaret silmesi çakışıyor | uygulandı | UC-19, UC-22 |
| K2-5 | Low → Low | doğrulandı | Ödeme adımının oturum kapanmış üye kuralı (K-1181 (4)) yalnız UC-27'de yazılı | uygulandı | §4.13 girişi |
| K2-6 | Low → Low | doğrulandı | UC-29 hatası havale siparişi için UC-28'e yönlendirir; UC-28 yalnız kart siparişi yönlendirir | uygulandı | UC-29 hata 303 |
| K2-7 | Low → Low | doğrulandı | UC-27'nin özet farkı 422'si `05 §4.2.3`'e bağlı, ama §4.2.3 yalnız ayırmayı yazar | uygulandı | UC-27 atfı (`05 §4.2.2` (2), §4.2.3) |
| K3-1 | Medium → Medium → Medium | doğrulandı / doğrulandı | Yeniden doğrulamada Google'a ulaşılamaması ve vazgeçme girişin hedefine (`/giris`) gidiyor, kod yok | uygulandı | UC-51, UC-60, §6.3 (iki yeni kod: `SocialReauthenticationUnavailable`, `SocialReauthenticationFailed` anlamı) |
| K3-2 | Medium → Medium → Medium | doğrulandı / doğrulandı | UC-51 başarı: doğrulanmamış bekleyen kayda eşleşmede kapı koşulu yok, kayıt koşulsuz silinir (aydınlatma kapısı) | uygulandı | UC-51 yanıtı ve hata maddesi, §6.3 `SocialLoginDataCollectionClosed`, Kaynak |
| K3-3 | Medium → Medium → Medium | doğrulandı / doğrulandı | UC-66 "429 — L-9" hangi gönderimin sayıldığını ve deneme kaydını yazmıyor | uygulandı | UC-66, UC-179, §3.2 (CR-2 ile tek düzeltme) |
| K3-4 | Low → Low | doğrulandı | UC-74 sipariş geçmişini "hesap silmeden dönüşün ekranı" sayıyor; UC-68 `/hesap`'a yönlendirir | uygulandı | UC-74 (§4.27) |
| K3-5 | Low → Low | doğrulandı | Kalemi bu siparişin olup talebi olmayan `orderItemId`: GET 303, POST 404 | uygulandı | UC-44 GET 404 |
| K3-6 | Low → Low | kısmen | UC-78 sonucu belirsiz işlemleri `05 §10.3.4`'e bağlar; liste kayıt ve yeni adres doğrulamasını saymaz | uygulandı | UC-78 atfı `04` 2.7.3.3; `05 §10.3.4` park adayı açık |
| K3-7 | Low → Low | kısmen | Bağlantı iniş uçları anahtarın türünü denetlediğini söylemiyor | uygulandı | §4.21, §4.23, §4.28 girişleri |
| K3-8 | Low → Low | kısmen | §4.32.2 "noindex diye yazılan sayfalar" der; hesap ekranları sözcüğü taşımaz | uygulandı | UC-79, §4 girişi |
| K4-1 | Low → Low | doğrulandı | §9 "Sonuç" kuralı iletişim talebi kapat / yeniden aç için yanlış (kısa süreli bildirim vardır) | uygulandı | §9 girişi |
| K4-2 | **High** → High → High | doğrulandı / doğrulandı | UC-131 / UC-133 kart hattındaki geri ödemeyi ödeme ekseninin geçişi gibi yazıyor; `05 §4.6.2` farklı | uygulandı | UC-131, UC-133 yan etkileri (havale / kart ayrımı; O3 metni) |
| K4-3 | Low → Low | doğrulandı | UC-130'un 422 sebebi `03 §1.11.8`'in üçüncü koşulunu (fiziksel kalem yok) saymıyor | uygulandı | UC-130 hata, §9.9 Kaynak (K-1140) |
| K4-4 | Low → Low | doğrulandı | §9 girişi çerçevesiz sayfaları üç ekranla sınırlıyor; `04` 2.2.9 iki iniş ekranını daha sayar | uygulandı | §9 girişi (K5-6 ile tek düzeltme) |
| K4-5 | Medium → Medium → Medium | doğrulandı / doğrulandı | UC-134 isteğin sebebini formdan alıyor; `06 §4.29` `Reason` isteği doğuran olayın değeri | uygulandı + üst doküman | **K-1201** · UC-134, `RequestRefundIbanInput`, §3.2; `04` 9.9.23, 7.2.13.3 |
| K5-1 | Low → Low | doğrulandı | UC-158 422 listesi şube telefonunun biçim denetimini saymıyor | uygulandı | §9.13 kayıt formu 422 |
| K5-2 | Medium → Low → Low | kısmen / kısmen | UC-173 yönetici kaldırma: kilit altı denetim tutmazsa yanıt yazılı değil | uygulandı | UC-173 hata (O3 metni) |
| K5-3 | Medium → Medium → **Low** | doğrulandı / kısmen | UC-183 "bütün üye hesapları" der; doğrulanmamış bekleyen kaydın dosyaya girip girmediği açık değil | uygulandı | UC-183 isteği (bekleyen kayıt dahil; K-823; yeni K yok) |
| K5-4 | Low → Low | doğrulandı | UC-105 "dört oran" der; dördüncü ölçü oran değil sayıdır | uygulandı | UC-169 yanıtı |
| K5-5 | Low → Low | doğrulandı | `period=MeasurementWindow` istenirse (eski yer imi) sonuç yazılı değil | uygulandı | §9.22 yanıtı |
| K5-6 | Low → Low | doğrulandı | §9 girişi ↔ §9.25 / §9.26 çerçevesiz sayfa sayımı | uygulandı | §9 girişi (K4-4 ile tek) |
| K5-7 | Low → Low | doğrulandı | KDV oranı reddi "0 ile 100 arasında değil" — uçlar belirsiz | uygulandı | UC-171 hata (0 ≤ oran < 100) |
| K5-8 | Low → Low | doğrulandı | CSV iç biçim listesinden "ondalık ve tarih biçimi" düşmüş (K-1195 (6)) | uygulandı | UC-182 (§9.24.2) |
| K6-1 | Medium → Medium → Medium | doğrulandı / doğrulandı | `FilledFieldCleared` anlamı `02 §6.9.1`'e bağlı; o satış kapısı hâlidir | uygulandı | §6.2.2, §6.2 `SalesClosed` |
| K6-2 | Medium → Medium → Medium | doğrulandı / doğrulandı | "Adı konmuş her hâl bir koddur" (K-1198 (1)) ama üç 303 / 409 hâli kodsuz | uygulandı | §6.2.5 üç kod, §6.2.6 `SingletonContentExists`; §6 sayımları (yeni karar yok) |
| K6-3 | Low → Low | doğrulandı | §6.2.10 ilk satırı `02 §6.7.2`'yi "uç yanıtı yoktur" sayar; satır yanıt doğurur | uygulandı | §6.2.10, §6.2.1 |
| K6-4 | Low → Low | doğrulandı | `LinkNoLongerValid` ↔ `InvitationNoLongerActive` atfı `02 §6.9.12` ile karışık | uygulandı | §6.2.5 atfı |
| K6-5 | Low → Low | doğrulandı | §6.1.5 betiğinin ikinci süzgeci tablodaki kendi `§6.x` atıflarını `02 §6` sanıyor | uygulandı | §6.1.5 (iki yönlü sınandı) |
| K6-6 | Low → Low | doğrulandı | §5.1.2 "ad kalın yazılır": `Items` ve `OriginalAdjacentSortOrder` kalın değil | uygulandı | §5.2.1, §5.3.2 |
| AT-1 | Low → Low | doğrulandı | Üç gövde cümlesinde önsüz `§` atfı konvansiyon 5'e göre yanlış dokümanı gösteriyor | uygulandı | §2.4.5, §5.2, §8.1.5 |
| AT-2 | Low → Low | doğrulandı | Matris ve GAP tablosunun dört atfı konvansiyon 5'e aykırı (dizi virgülle bitiyor, önsüz `§16.3`) | uygulandı | §1.1.1, §1.1.9 satırı, §1.3 GAP-14 |
| AT-3 | Low → Low | doğrulandı | On iki alt bölümün Kaynak satırı Aşama 6'nın kolunu saymıyor | uygulandı | Kaynak satırları (§2.1, §2.2, §2.7, §2.9, §3.3, §4.10…§4.31, §8.2, §9.5) |
| AT-4 | Low → Low | doğrulandı | §3.1 Kaynak satırı yok | uygulandı | §3.1 (K1a-4 ile tek) |
| IT-1 | Medium → Low → **Medium** | doğrulandı / doğrulandı | K1b-1 ile aynı yer (§2.4.5 ↔ §2.4.3, §2.4.1 "formu") | uygulandı | K1b-1 ile |
| IT-2 | Medium → Low → Low | doğrulandı / doğrulandı | §2.5.1'in kapalı listesi dört işlemi sayar, tablo ve `12` üç diyor | uygulandı + üst doküman | §2.5.1, §8.2.2 satırı; `12` K-1178 · K-1179 · K-1181 |
| IT-3 | Medium → Medium → Medium | doğrulandı / doğrulandı | §2.2.2 "yazmayan POST yönlendirmez" mutlak yazılmış, uç tanımları istisna taşır | uygulandı | §2.2.2, §2.7.1 |
| IT-4 | Low → Low | kısmen | §2.7.1 durum kodları listesinde 416 yok (indirme ucu ve §6 kullanır) | uygulandı | §2.7.1 |
| IT-5 | Low → Low | doğrulandı | GET'in yazdığı Google dönüşü sayımı UC-51'in yan etkileriyle tutmuyor | uygulandı | §2.2.5, UC-51 yan etkileri |
| IT-6 | Low → Low | kısmen | §2.6.8 "listeden kayda giden bağlantı `returnUrl` taşır"; üyenin sipariş geçmişi ekranında dönüş öğesi yok | **uygulanmadı** | E-16'ya dönüş öğesi kararı gerektirir — deep review'a devredildi |
| IT-7 | Low → Low | doğrulandı | §2.3.2 "oturumu kapanmış POST girişe yönlendirir"; müşteri çıkışının tanımı bunu yinelemez | uygulandı | §2.3.2 |
| IT-8 | Low → Low | doğrulandı | UC-127 teslim tarihi biçim atfı §2.9.1; biçim §2.9.3'te | uygulandı | §9.9 teslim tarihi isteği |
| CR-1 | Medium → Medium → Medium | doğrulandı / doğrulandı | `05 §6.7.7` ödeme çerezini sepet boşalınca da siler; `07` yalnız onay kolunu yazar | uygulandı + matris | UC-19, UC-21; `05 §6.7.7` matris hücresi |
| CR-2 | Medium → Medium → Medium | doğrulandı / doğrulandı | `02 §8.2` L-9 yerine geçen e-posta değişikliği isteğini sayar, UC-66 yazmaz | uygulandı | UC-66, UC-179 (K3-3 ile tek) |
| CR-3 | **High** → High → High | doğrulandı / doğrulandı | `DEPLOY_RUNBOOK` maskeleme talimatı `/siparis/erisim/` gibi anahtar taşıyan yolların son bölümünü kapsıyor, park satırı daha dar | uygulandı + üst doküman | `DEPLOY_RUNBOOK` K-1162 · K-1169 satırı |
| CR-4 | Low → Low | doğrulandı | `12` park satırı sepetin GET'inin yazdıklarına K-1183'ün ödeme yöntemi işaretini eklemiyor | uygulandı + üst doküman | `12` aynı satır |
| CR-5 | Low → Low | kısmen | `07` dışa aktarma dosyası "geçici olarak üretilir ve silinir"; `05 §6.8.4` "saklanmaz" | uygulandı + üst doküman | UC-182 yan etkileri; `09` K-1193 · K-1195 satırı |
| M-1 | Low → Low | doğrulandı | §1.1.11 park A4-2 ayarlar satırı UC-169…172 ve UC-174'ü saymıyor | uygulandı | §1.1.11 Uç hücresi (+5 çift, devir) |
| M-2 | Low → Low | doğrulandı | `03 §1.7.2` m3 hizmet kalemi yolu firma iptalini (UC-129) taşımıyor | uygulandı | matris hücresi (+1, 03) |
| M-3 | Low → Low | doğrulandı | `03 §9` m2 yöneticinin e-posta değişikliği yeniden isteme ucunu (UC-180) saymıyor | uygulandı | matris hücresi (+1, 03) |
| M-4 | Low → Low | doğrulandı | `03 §3.2.1.10` yalnız iki POST'u eşliyor; ödeme adımının gösterimi (UC-22) yok | uygulandı | matris hücresi (+1, 03) |
| M-5 | Low → Low | kısmen | `04` 9.17.13'ün dördüncü işi (şifreyi sıfırlama) eşlenmemiş | uygulandı | matris hücresi UC-181 (+1, 04) |
| M-6 | Low → Low | kısmen | §1.1.13 başlığı parantezde satır sayısı yerine "(2)" taşıyor | uygulandı | §1.1.13 başlığı ve harita satırı |
| M-7 | Low → Low | kısmen | Google'ın sayfasına geçen iki gezinti satırı "uç doğurmaz — dış sayfa" değerini taşımıyor | uygulandı | `04` 3.3.27, 3.3.37 hücreleri |
| TE-1 | Low → Low | kısmen | Konvansiyon 3 "fiil ve nesne" der; altı işleyici nesnesizdir (`Recalculate`, `Leave`…) | uygulandı | **K-1205** · yazım konvansiyonu 3 |
| TE-2 | Low → Low | kısmen | `Save` "yeni ya da var olan kaydı yazan form" der; adres defteri `Create…` / `Update…` kullanır | uygulandı | **K-1205** · yazım konvansiyonu 3 (B yolu) |
| TE-3 | Low → Low | kısmen | `ProcessPartialRefund` ↔ `06 §5.6.7` `AmountBased`; "kısmi geri ödeme" sözlükte yok | uygulandı | UC-133 adresi, §5.2, §6.2, §9.9.12 başlığı (4 geçiş) |
| TE-4 | Low → Low | doğrulandı | Konvansiyon 2 "dört tür ad" der; §2.3.1 ve §5 altı tür veriyor | uygulandı | başlık notu madde 2 (altı tür) |
| TE-5 | Low → Low | doğrulandı | K-1196 (4): `search`, `Up`, `Down` ad satırında kalın değil | uygulandı | §5.3.2 |
| TE-6 | Low → Low | doğrulandı | "Arşivlendi" sözlükte yok (değer "Arşiv") | uygulandı | §4.5.1 |
| TE-7 | Low → Low | doğrulandı | §2.3.4 sipariş adresinin iki kökünü yer tutucusuz yazıyor | uygulandı | §2.3.4 |
| TE-8 | Low → Low | doğrulandı | UC-78 / UC-06 aktör etiketleri konvansiyon 7'nin kapalı listesinde yok | uygulandı | §3.2, §1.2 aday tablosu |
| YU-1 | Low → Low → Low | kısmen / kısmen | Onay bölümü sayımı `04` 5.13.9 (e)'yi (sabit sipariş metni, aydınlatma bağlantısı; 6563 m.3/1-b, c, ç) saymıyor | uygulandı | UC-22 onay bölümü sayımı (a)–(h) |
| YU-2 | Low → Low → Low | kısmen / kısmen | "Çerez politikası yayına alınana kadar çerez yazılmaz" matriste `API2-03`'e bağlı, §2.3'te yok | uygulandı | §2.3.1 yeni cümle, §4 girişi atfı, UC-174 yan etkisi; matris eki alınmadı (H1) |
| G1-1 | **High** → High → High | doğrulandı / doğrulandı | Kısmi adetli iptal ve caymada ikinci gönderim güncel hâlden farklı adedi sessizce işleyebilir (iki sekme) | uygulandı + üst doküman | **K-1202 ⚠** · §2.4, UC-40, UC-43, §5.2.1, §8.2.2; `05 §7.3.2`, `12` |
| G1-2 | Low → Low | doğrulandı | "Çift tıklamada ikinci gönderim güncel hâli gösterir" kayıt varyantında 409 verir | uygulandı | §2.4.8 |
| G1-3 | Low → Low | doğrulandı | "GET uçları koruma taşımaz — kayıt değiştirmezler" gerekçesi Google dönüşüyle tutmuyor | uygulandı | §2.5.5 |
| G1-4 | Low → Low | doğrulandı | Sahtecilik doğrulaması başarısız formun yeniden gösterimi: kalan risk adıyla yazılı değil | uygulandı | **K-1206** · §2.5.8, Kaynak |
| G2-1 | Medium → Medium → Medium | doğrulandı / doğrulandı | Parçalı yüklemenin sırası (kilit, alınan bayt, son parçadan sonra çökme) yazılı değil | uygulandı + üst doküman | **K-1203** · UC-83; `05 §7.7.3` |
| G2-2 | Low → Low | doğrulandı | "Ömrü dolmuş kayıt silinmiştir" — silmeyi zamanlayıcı yapar, istek yalnız süresi dolmuşu görür | uygulandı | UC-47, UC-55, `05` 5.2.7 devir notu |
| G2-3 | Low → Low | kısmen | Üç "Idempotency" maddesi eşzamanlı ikinci isteği tekil dizine bırakır, yazılı değil | uygulandı | **K-1206** · §2.5.1, UC-05, UC-48, UC-55 |
| G3-1 | Low → Low | kısmen | §9.24 "dosya saklanmaz" der, UC-182 "geçici üretilir ve silinir" der | uygulandı | §9.24 girişi, UC-182 |
| G3-2 | Low → yok → yok | çürütüldü / çürütüldü | §9.13.5 sıra kaydırmada iki satırın kilit sırası yazılı değil (ölü kilit) | çürütüldü | — (`05 §7.3.5` ölü kilidi adlandırır; veritabanı bir işlemi geri alır) |
| G3-3 | Medium → Medium → **Medium** | doğrulandı / doğrulandı | Panel girişinde hesap yoksa özet hesaplanmaz; yanıt süresi hesabın varlığını ele verir | uygulandı + üst doküman | **K-1204** · UC-110, UC-111; `05 §6.1.4`, `02 §8.1.4` |
| G3-4 | Low → Low | kısmen | İki yöneticinin aynı adrese eşzamanlı daveti tekil dizine takılır, yazılı değil | uygulandı | **K-1206** · §2.5.1 kalan risk |
| G3-5 | Low → Low | kısmen | Kategori silme ağacın danışma kilidini almaz; eşzamanlı ekleme yarışı | uygulandı | **K-1206** · §2.5.1 kalan risk |

### Güvence mercekleri (G1–G3)

Güvence merceği, dokümanın bir **güvence** verdiği her cümleyi ("tek yoldan", "yoktur", "her liste", "çift gönderim engellenir") bulur ve onu sağlayan mekanizmayı (`05`, `06`, `07 §2`) arar: güvence **tutar**, **kısmen tutar** ya da **tutmaz**. Envanter betikle çıkarıldı (`gscan.py`, 1.261 birim; ilk desen 791 üretip numaralı maddelerle karşılaştırmada eksik bulundu ve genişletildi); birim üretmeyen 35 numaralı madde G1'e ayrıca verildi. Sınıf sayımları merceklerin dosyalarındadır ve toplam tutar.

| Mercek | Birim | Güvence değil | Tutar | Kısmen | Tutmaz | Bulgu |
|---|---|---|---|---|---|---|
| G1 §2, §3, §5–§8, §10 | 368 (+ 35 satır) | 208 | 151 | 9 | 0 | G1-1…G1-4 (4) |
| G2 §4 | 396 | 235 | 153 | 8 | 0 | G2-1…G2-3 (3) |
| G3 §9 | 497 | 352 | 138 | 7 | 0 | G3-1…G3-5 (5; G3-2 çürütüldü) |
| **Toplam** | **1.261** | **795** | **442** | **24** | **0** | **12** |

Özet: güvence ürünün veri tarafında tutar — tekil dizinler, koşullu güncellemeler, sürüm sütunları ve danışma kilitleri 442 birimin hepsini karşılar; hiçbir güvence tamamen tutmaz çıkmadı. 24 kısmen tutan birim beş kümeye iner: (1) **formun gösterdiği değerle karşılaştırılmayan işlemler** (G1-1: kısmi adetli iptal ve cayma — K-1202) · (2) **zamanlama ve kilit** (G2-1 parçalı yükleme; G3-3 panel girişinin yanıt süresi — K-1203, K-1204) · (3) **eşzamanlı ikinci istek tekil dizine takılır** (G2-3, G3-4, G3-5 — K-1206) · (4) **"yedeğe girmeyen" ve "saklanmaz" sözcüklerinin dosya için yinelenmesi** (G3-1) · (5) **adı konmamış kalan riskler** (G1-4, G1-2, G1-3, G2-2). Adıyla daraltılan iki kalan risk satırı `07 §2.5.1`'de toplandı (K-1206).

### Karar listesi (K-1199…K-1206)

Kayıt modu K-992 (⚠ öneriyle kayıt). Tam metinleri ve elenen seçenekleri karar kaydının §2'sinde (K-1198'in ardından) durur. Hepsi bu denetimin doğrulanmış bulgularından çıktı.

| K | Karar (tek cümle) | Bulgular | İşaret |
|---|---|---|---|
| K-1199 | Aynı onay anahtarıyla ikinci gönderimin hedefi teyit ekranıdır (`{kök}/teyit`); K-893 (1)'i netleştirir | K2-2 | ⚠ öneriyle kaydedildi |
| K-1200 | Teyit ekranından sağlayıcıya bağlanılamayınca sepetin bir kez söylenen mesajı — K-1183'ün işareti (`IsPaymentUnavailableNoticePending`) bu dönüşü de kapsar | K2-3 | öneriyle kaydedildi |
| K-1201 | UC-134'te isteğin sebebi formdan alınmaz, sunucu `AdminRequest` yazar | K4-5 | öneriyle kaydedildi |
| K-1202 | Kısmi adetli iptal ve caymada formun gösterilen açık adedi taşıması; değiştiyse istek güncel hâlle döner (K-1170 (4)'ün iki sekme kenar durumu daralır) | G1-1 | ⚠ öneriyle kaydedildi |
| K-1203 | Parçalı yüklemede parçanın eklenmesi kilit altında; alınan baytın evi; son parçadan sonra çökmenin çıkışı (K-1171 (1)'i netleştirir) | G2-1 | öneriyle kaydedildi |
| K-1204 | Panel girişinde hesap yoksa da sahte özetle doğrulama; şifre sıfırlamada yanıt süresi kalan risk | G3-3 | öneriyle kaydedildi |
| K-1205 | Yazım konvansiyonu 3'ün iki okunuşu — nesnesiz işleyiciler; ayrı adresli ekleme ve düzenlemede `Create…` / `Update…` (K-1173'ü netleştirir) | TE-1, TE-2 | öneriyle kaydedildi |
| K-1206 | Adıyla daraltılan kalan riskler — sahtecilik doğrulaması başarısız formun yeniden gösterimi; tekil dizine takılan eşzamanlı ikinci istek | G1-4, G2-3 (+ G3-4, G3-5) | öneriyle kaydedildi |

**⚠ olanlar (2):** K-1199 ve K-1202. İkisi de çok kritik olmadığından Türker'e sorulmadı; yürütücü öneriyle karar verip sonda listeledi (ayrıntı karar kaydında). Gövdede karar numarası doğrudan yazıldı; `*Kaynak:*` satırları yeni K'yı aldı. Geri işaretler (karar kaydı etki sütunları): K-893 ← K-1199 · K-1170 ← K-1202 · K-1183 ← K-1200 · K-1173 ← K-1205 · K-1168 ← K1b-2 · K-1198 `12` kolu ← K1a-1 · K-1166, K-1171, K-1179 ← K-1206 / K-1203 (A5'in kendi geri işaretleri) · K-1160 ← matris düzeltmeleri (A4).

### Yasal dayanak (mevzuat doğrulama günlüğü)

`07` hiçbir mevzuatı adıyla ya da maddesiyle anmaz (betik: `6698`, `6502`, `6563`, `5651`, `md.`, `Yönetmeli`, `açık rıza` için sıfır eşleşme); yasal dayanak `02` / `04` / `05` evindedir ve bu denetimde cümlelerin o eve uyumu okundu. Okuyan: **YU** (günlüğü 2026-10-19'da yazdı) ve **H1** (Opus / high; üç yasal dayanaklı bulgunun mevzuatını resmî metne yeniden okudu).

| Mevzuat | Madde | Okuyan | Sonuç | `07`'deki karşılık |
|---|---|---|---|---|
| Mesafeli Sözleşmeler Yönetmeliği (RG 27/11/2014-29188; son değ. RG 24/5/2025-32909) | m.5/1-g, -ğ, -h, -k; m.5/2; m.6/1, /2-a, /2-b; m.7/1; m.8/1; m.9/2, /5; m.10/1; m.11/1, /2, /4; m.12/1, /4, /5 | YU (PDF, `pdftotext`) | Tutarlı; m.11/2 (teyit bilgisi derhal iletilir) B-9'u, m.8/1 UC-27 düğme metnini, m.6/2-a "cayma özeti"ni karşılar | UC-22, UC-27, UC-42 / 43, UC-130 |
| 6698 sayılı KVKK | m.5/2 (a–f), m.10/1 (a–d), m.11/1, m.13/1–3 | YU; H1 (m.10) | Tutarlı; m.10/1 aydınlatma "verilerin elde edilmesi sırasında" — K3-2 ve YU-2'nin yasal dayanağı (K-850'nin kapısı) | UC-94, UC-157, UC-51, kapı uçları |
| Veri Sorumlusuna Başvuru Usul ve Esasları Hakkında Tebliğ (RG 10/3/2018-30356) | m.5/1, /4, /5; m.6/3, /5 | YU | Tutarlı; m.5/5 "başvurunun veri sorumlusuna ulaştığı tarih" = UC-94 "firmaya ulaştığı an". Not: özgün RG metni okundu, sonraki değişiklik denetlenemedi | UC-94 (§9.11.1) |
| 6563 sayılı Elektronik Ticaretin Düzenlenmesi Hakkında Kanun | m.3/1 (a–d), m.3/4; m.4/1 (a–c), m.4/2 | YU; H1 | Tutarlı (m.3 ve m.4'te değişiklik notu yok); m.3/1-b, -c, -ç → YU-1; aydınlatma bağlantısı m.3/1-d'ye de düşer (günlük anmamıştı, sonucu değiştirmez) | UC-22 (YU-1), UC-27 |
| 6502 sayılı Tüketicinin Korunması Hakkında Kanun | m.12/1, /3; m.16/1, /2; m.48/2, /3, /4 | YU | Tutarlı — iki yıl teslim / ifadan (Z-18); otuz gün (Z-11; 7392 ile değişik m.48/3); bir yıllık uzama (m.48/4) | UC-44–46, UC-41, UC-130 |
| Elektronik Ticaret Aracı Hizmet Sağlayıcı ve Elektronik Ticaret Hizmet Sağlayıcılar Hakkında Yönetmelik (RG 29/12/2022-32058) | m.5, m.7/1 (a–ç) | YU (RG'den) | Tutarlı; m.7/1 işlem rehberinin ana sayfadan erişimi → UC-18. 06_AUDIT'te mevzuat.gov.tr'den "doğrulanamadı" idi, bu kez RG'den okundu. Sonraki değişiklik denetlenemedi | UC-18, UC-14 |
| Kişisel Verilerin Silinmesi, Yok Edilmesi veya Anonim Hale Getirilmesi Hakkında Yönetmelik | m.7/3 (imha kaydı üç yıl) | — | Yeniden okunmadı (06_AUDIT'te doğrulandı); `07`'nin cümleleri `02 §12.2.7` ile tutarlı | UC-51, UC-68, UC-157 |
| KVKK Çerez Uygulamaları Hakkında Rehber (Temmuz 2025) | — | — | Kapsam dışı; yeniden okunmadı — YU-2'nin hükmü ona dayanmaz (yasal ev `02 §12.2.5`; K-834, K-850, K-908) | — |
| Ticari İletişim ve Ticari Elektronik İletiler Hakkında Yönetmelik · 5651 sayılı Kanun | — | — | Kapsam dışı — `07` ticari ileti uçları açmaz, 5651'i anmaz | — |

**Yasal dayanaklı üç bulgu (H1):** **K3-2** (Medium, doğrulandı) — kapı koşulu üçüncü dalda yok; yanlış okunuşun sonucu yasal (aydınlatma metni yayında değilken hesap açılır) ve geri dönüşsüz (bekleyen kayıt silinir), ama `§6.3`'ün kodu karşı sinyal verdiği için High değil. **YU-1** (Low, kısmen) — özet cümlesinin eksik sayımı, yasal içerik doğru (6563 m.3/1 (b), (c), (ç), resmî metne karşı doğrulandı). **YU-2** (Low, kısmen) — kural `07`'de vardır (§4 girişi); ev uyumsuzluğu ve UC-174'ün tamlığı düzeltildi. Yasal bakımdan **eksik, yanlış ya da süresi geçmiş** hüküm çıkmadı.

### Geri besleme — üst dokümanlara ve park satırlarına

Güncellenen yerler `07 §1.3`'ün geri besleme tablosunda (6 satır eklendi: K-1199…K-1204; harita satırı "geri besleme satırı 3 → 9") ve karar kaydının etki sütunlarında K numarasıyla sayılır.

| Doküman | Sürüm | Değişen bölümler (K) |
|---|---|---|
| `02_PRODUCT_REQUIREMENTS.md` | v0.90 → **v0.91** | §8.1.4 kalan risk cümlesi + Kaynak (K-1204) |
| `04_UI_SPECS.md` | v0.37 → **v0.38** | 5.12.15, 6.2.12.6 hâl ve kaynak hücresi, 5.12 Kaynak (K-1200) · 9.9.23, 7.2.13.3, 9.9 Kaynak (K-1201) |
| `05_TECHNICAL_ARCHITECTURE.md` | v0.26 → **v0.27** | §4.5.1 + Kaynak (K-1199) · §7.3.2 müşteri işlemi maddesi + Kaynak (K-1202) · §7.7.3 + Kaynak (K-1203) · §6.1.4 + Kaynak (K-1204) |
| `06_DATA_MODEL.md` | v0.28 → **v0.29** | §4.17 `ConfirmationKey` açıklaması ve kaynak hücresi (K-1199) · §4.15 `IsPaymentUnavailableNoticePending`, §3 Cart satırı, §1.1 `03 §10.1` satırı, Kaynak (K-1200); başlık bloğuna Aşama 6 notu eklendi |
| `09_CODING_GUIDELINES.md` | park satırı | K-1193 · K-1195: dışa aktarma dosyası "yedeğe girmeyen geçici bir dizinde" (CR-5) |
| `12_VALIDATION_PROTOCOL.md` | park satırları (şablon durumunda, sürüm değişmedi) | K-1196 · K-1198: §6.4.3'ün üç kalan riski (K1a-1) · K-1178 · K-1179 · K-1181: dört işlem, sepet GET'i ödeme yöntemi işareti (IT-2, CR-4) · K-890 · K-893: teyit ekranı (K-1199) · K-1163…K-1172: gösterilenden farklı açık adet, eşzamanlı ikinci parça ve son parçadan sonra çökme (K-1202, K-1203) |
| `DEPLOY_RUNBOOK.md` | park satırı (şablon durumunda) | K-1162 · K-1169 (1): anahtar taşıyan bölüm, alt ekran adresleri (CR-3) |
| `PRODUCT_DISCOVERY_STATUS.md` | karar kaydı | §2'ye K-1199…K-1206 satırları; K-1144…K-1206 her biri tam bir kez |

`07`'nin kendi geri besleme çıktıları: matrisin çiftleri 5.056 → **5.071** (`04` 2.956 → 2.961 · `03` 1.576 → 1.579 · devir 280 → 285 · `05` 244 → 246); satır sayısı (2.318), aday sayısı (187) ve durum dağılımı (eşlendi 2.318 · konvansiyon 111 · uç doğurmaz 219) aynı; §1.2'nin 13 aday satırı güncellendi. `05`'te yeni numaralı madde açılmadı. Etki yansıtma `02` v0.91, `04` v0.38, `05` v0.27, `06` v0.29'u yeniden tarar.

### Aksiyon Planı

- **Critical:** yok.
- **High (4; dördü de uygulandı):** yayındaki ürünün taslak varyantının yönetici önizlemesindeki davranışı (K2-1) · kart hattındaki geri ödemede ödeme ekseninin geçişi (K4-2) · `DEPLOY_RUNBOOK` maskeleme talimatının anahtar taşıyan yolları kapsaması (CR-3) · kısmi adetli iptal ve caymada iki sekme kenarı (G1-1 → **K-1202** ⚠).
- **Medium (17; on yedisi de uygulandı):** K1a-1, K1b-1 (+ IT-1), K1b-2, K2-2 (K-1199), K2-3 (K-1200), K3-1, K3-2, K3-3 (+ CR-2), K4-5 (K-1201), K6-1, K6-2, IT-3, CR-1, G2-1 (K-1203), G3-3 (K-1204); IT-1 ve CR-2 kendi çiftlerinin parçası olarak Medium sayılır. Sonraki dokümanın işi olanlar park satırlarına talimat olarak girdi (`09`, `12`, `DEPLOY_RUNBOOK`).
- **Low (66):** 64'ü uygulandı (G2-3 iki aşamada); 1'i uygulanmadı (IT-6); G3-2 ve K1a-3 çürütüldü ve sayıma dahil değildir.
- **Sıradaki adım:** `deep-review` (IT-6 ve aşağıdaki açık gözlemler girdisi); ardından `cross-review`.

### Uygulanmayanlar

| Bulgu | Durum | Gerekçe |
|---|---|---|
| IT-6 | uygulanmadı — deep review'a devredildi | Sonnet'in iki seçeneği var: (a) §2.6.8 genel kalır, UC-74 / UC-33'e `returnUrl` yazılır — bu `04`'te E-16'ya "Siparişlerime dön" öğesi eklemeyi gerektirir (Arayüz Tanımları'nın kararı, yeni K gerekebilir); (b) §2.6.8 "panelin listeleri" diye daraltılır ve `04` 5.27.7'nin "bıraktığı sayfayı bulur" cümlesi tarayıcı geçmişine bağlanır — oysa §2.6.9 geçmiş yolunu eler. İki yön de `04`'ün okunuşuna bağlı; uygulayıcıya bırakılmadı |
| K1a-3 | çürütüldü (Sonnet ve Opus) | K-1150 (`API1-01`) matrisin kaynak birimi ve oturumlara bölünmesini kuran karardır; "yedi oturumda (K-1150…K-1161)" aralığı matrisin konusunun kararlarını sayar ve doğrudur. Karar kaydı 2708'in "(K-1151…K-1161)" yazımı oturumların kararlarını anar — iki kolun ölçüsü aynı |
| G3-2 | çürütüldü (Sonnet ve Opus) | UC-158'in iki koşullu güncellemesi aynı işlemde yazılı ve veri güvencesi tutar; ters yönde eşzamanlı iki kaydırmada ölü kilit olasılığı `05 §7.3.5`'te adlandırılmış yedek yolla karşılanır ("veritabanı ölü kilidi bulur ve işlemlerden birini geri alır — kayıp doğmaz"). Senaryo çok dar. İsteğe bağlı bedelsiz iyileştirme (§9.13.5'e "iki satır iç anahtar sırasıyla güncellenir (`05 §7.3.5`)") alınmadı |

**Kısmen uygulananlar:** yok (G2-3 A1'de kısmen, A2'de tamamlandı; CR-5 A2'de A3'e bırakıldı, A3 uyguladı).

### Cross-review ve deep review için notlar

- **Boyut:** `07` ~1,16 MB (1.157.812 bayt, ~6.400 satır) — `cross-review` skill'inin 400 KB eşiğinin çok üstünde; 06'daki gibi her turda iki koşum (tek parça çağrı + parçalı koşum; K-953) beklenir.
- **Bu adımda en çok değişen yerler** (cross-review'ın öncelikli okuması): §2.4–§2.5 (K-1199, K-1202, K-1206; kapalı liste, kalan riskler), UC-27 / UC-29 / UC-19 (K-1199, K-1200), UC-40 / UC-43 (K-1202), UC-51 / UC-60 ve §6.3 (K3-1, K3-2), UC-83 (K-1203), UC-110 / UC-111 (K-1204), UC-134 (K-1201), §6.2.5 üç yeni kod (K6-2), §1.2 ve §1.1.1.
- **Açık gözlemler (doğrulayıcı ve uygulayıcı loglarından):**
  - **IT-6** deep review'ın girdisidir (yukarıda). Sonnet'in yan yüzü: UC-90'da kupon formundan sonra `page` kaybı aynı kalıp; `04` 9.7.11'in "listeye dönüş"ü ile birlikte aynı karar çerçevesinde bakılır.
  - **K3-6**: `05 §10.3.4`'ün sonucu belirsiz işlem sayımı kayıt ve yeni adres doğrulamasını anmıyor (`04` 2.7.3.3 anıyor; K-904). `07` atfı düzeltildi, `05` tarafı park adayı olarak (Low) açık.
  - **G3-2**'nin bedelsiz iyileştirmesi (iç anahtar sırası) alınmadı; deep review isterse ekler.
  - **K2-2**: Opus, Sonnet'in bir yan iddiasına katılmadı (O2); düzeltme K-1199'un kapsadığı ikinci gönderim hedefiyle sınırlı.
  - **K3-1**: Opus iki kod istedi (Sonnet'in önerisinden küçük bir ayrışma); A2 §6.3'e `SocialReauthenticationUnavailable`'ı ekledi ve `SocialReauthenticationFailed`'ın anlamını genişletti; §6.3'ün sayısı buna göre güncellendi.
  - **A1**: AT-3'te Kaynak eksiği yalnız mercek yan yüzlerinden doğrulanan on iki yere eklendi; mercek yan yüzlerinden doğrulanmayan sekiz yer eklenmedi ve §6.1'e K-1198 (3) kapsadığı için dokunulmadı. A6 sonunda K-1199'un Kaynak satırı (§9 onay ucu) eksik bulunup eklendi.
  - **A2**: UC-19 / UC-21 / UC-29 / UC-51 için matris uç hücreleri A4'e bırakıldı; A4 UC-29 için `03 §10.1.1.2` ve `04` 5.14.10'un zaten UC-29'u taşıdığını doğruladı (K2-3 matris uygulaması "kısmen" olarak loglandı, eksik yoktur).
  - **A3**: UC-183 / UC-184 "9.24.2'ninkiler" atfı olduğu için CR-5 düzeltmesinde ayrıca dokunulmadı; `04` 9.9.23 ve 7.2.13.3 matris hücreleri K-1201'e A6'da hizalandı (matris `cmp`: fark yok).
  - **AT mekanik sınaması**: uygulamadan sonra `05 §4.2.2` (2), §4.2.3 dizi hatası (virgül) uygulamanın açtığı tek yeni SEC kalıbıydı; yürütücü düzeltti. Kalan 102 SEC eşleşmesi yanlış pozitiftir (karar kaydı § dizisi).
  - **Mekanik betik ölçütü**: `defcheck` 10 bloğu bilinçli okunuş saydı (UC-06, UC-27, UC-51, UC-81, UC-82, UC-78, UC-125, UC-127, UC-128, UC-154); bu bloklar her sürümde yeniden bakılacak yerlerdir.
  - **Sayım betiği**: bulgu kimliği deseni mercek adlarının bütün biçimlerini (`K1a-`, `K1b-`) almalı — ilk sürüm `K1b`'yi düşürdü (Sonuç'un altındaki not).
- **Etki yansıtma** bu adımın geri beslediği `02` v0.91, `04` v0.38, `05` v0.27 ve `06` v0.29'u yeniden tarar; park satırlarının (`09`, `12`, `DEPLOY_RUNBOOK`) etiketleri ile karar kayıtlarının etki sütunları iki yönde eşleşir.
