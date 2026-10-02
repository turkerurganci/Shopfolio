# Audit Raporu — 01 Project Vision

**Tarih:** 2026-10-02 · **Hedef:** `Docs/01_PROJECT_VISION.md` v0.8 (bulgular v0.9'da işlendi) · **Bağlam:** `PRODUCT_DISCOVERY_STATUS.md` §2 (karar kaydı), `01` şablonu (e74883f), `10_MVP_SCOPE.md` v0.2, `02_PRODUCT_REQUIREMENTS.md` v0.9 · **Odak:** Tam denetim

### Koşum biçimi (K-431)

Çok mercekli: dokuz paralel ajan, her biri ayrı bir boyutta — kapsama ve sayım (iki dilim) · atıf doğruluğu (iki dilim) · iç tutarlılık, terim ve şablon uyumu · çapraz referans · yasal uyum · ölçülebilirlik ve derinlik · deep review Katman 1 ve 4–8. Her bulgu iki bağımsız şüpheciye gitti — **kanıt** merceği (alıntı ve K satırı birebir mi) ve **bağlam** merceği (başka bölümde karşılanmış mı, kararla bilinçli mi, başka dokümanın işi mi, düzeltme doğru mu). İki şüpheci de çürüttüyse bulgu rapora işlenmedi; biri çürüttüyse "tartışmalı" sayıldı ve ana bağlam karar verdi. Karar gerektiren dört bulgu ⚠ grubunda proje sahibine soruldu (K-479…K-482); detay ve tutarlılık kararları öneriyle kaydedildi (K-483…K-487).

### Envanter Özeti

| Kaynak | Toplam | ✓ | ⚠ | ✗ |
|---|---|---|---|---|
| Karar kaydı — `01`'i gösteren kararlar, K-01…K-240 | 37 | 35 | 2 | 0 |
| Karar kaydı — `01`'i gösteren kararlar, K-241…K-478 | 39 | 34 | 5 | 0 |
| `01` iç envanteri — §1–§4'ün K atıfları | 73 | 66 | 7 | 0 |
| `01` iç envanteri — §5–§8'in K atıfları | 114 | 110 | 4 | 0 |
| Şablon beklentileri + `01`'in iç sayı, kimlik ve terim öğeleri | 62 | 49 | 11 | 2 |
| Çapraz referanslar — `01` ↔ `10`, `02` | 47 | 36 | 9 | 2 |
| Yasal iddialar | 22 | 15 | 6 | 1 |
| **Toplam** | **394** | **345** | **44** | **5** |

**Sayım tutuyor:** her kaynağın Faz 1 envanteri ile Faz 2 eşleştirmesi aynı öğe listesidir; toplam 394 = 345 + 44 + 5. Envanter yöntemi: kapsama dilimlerinde etki sütunu `01`'i gösteren kararlar (`awk` ile `$(NF-1)`) + `01` v0.6 notundaki anlamsal K-29 listesi; atıf dilimlerinde (bölüm, K) çiftleri — aynı K iki bölümde geçtiğinde iki öğedir.

### Bulgular (audit mercekleri)

Toplam 39 bulgu: 38 doğrulandı, 1 tartışmalı, 0 çürütüldü. Aynı sorunu birden çok merceğin bulduğu durumlar ayrı satırda kalır, "Karar / uygulama" sütunu ortak sonucu gösterir.

| # | Mercek | Yer | Tür | Seviye | Bulgu | Karşı-doğrulama | Sonuç | Karar / uygulama |
|---|---|---|---|---|---|---|---|---|
| KAP1-1 | kapsam-a | 01 §8 V-1 (satır 254) · §7 S-5 (satır 232) · §1 (satır 29) · §3.1 tablo (satır 78) | ÇELİŞKİ | High | K-03'ün 'tek depo' kabulü 01'de aynı anda iki rejimde duruyor: §7'de post-MVP'de de değişmeyen kalıcı kimlik sınırı (S-5), §8'de ise yanlış çıkarsa ürünün yeniden tasarlanmasını gerektiren bir varsayım (V-1). | kanit: CONFIRMED · baglam: CONFIRMED | Doğrulandı | K-479 (⚠, proje sahibi) — §1, §3.1, S-5, V-1 ayrıştı |
| KAP1-2 | kapsam-a | 01 §3.2 'Aktör olmayanlar' (satır 98) | ÇELİŞKİ | Medium | §3.2, tüketiciye sınırlı satışı 'MVP'de' kaydıyla yazıyor. Oysa aynı kural §7'de S-6 olarak kalıcı kimlik sınırı, `10 §4.2`'de de 'Kalkmaz'. Kayıt, okuyucuya bunun post-MVP'de kalkabilecek bir kapsam kısıtı olduğunu düşündürüyor. | kanit: CONFIRMED · baglam: CONFIRMED | Doğrulandı | Uygulandı — §3.2 S-6'ya bağlandı |
| KAP1-3 | kapsam-a | PRODUCT_DISCOVERY_STATUS.md §2 — K-98, K-108, K-111, K-122 etki sütunları · 01 satır 12 ve 19 (yazım notları) | DIŞ_TUTARSIZLIK | Medium | 01 §2 ve §3.2'ye Blok 4'ten (v0.3) beri içerik veren dört kararın etki sütununda `01` yok; K-29 işareti eksik. v0.6'daki anlamsal K-29 kaçağı taraması da (satır 19) bu dördünü yakalamamış. | kanit: CONFIRMED · baglam: CONFIRMED | Doğrulandı | Tracker — dört kararın etki sütununa `01` işareti |
| KAP1-4 | kapsam-a | 01 §7 S-3 Kaynak hücresi (satır 230) | YANLIŞ_ATIF | Low | S-3'ün kaynak hücresi K-81'i saymıyor. Oysa satırın 'hizmet sabit fiyatla ve randevusuz satılır' içeriği K-81'in kararıdır ve K-470 ile `10 §3` S-3'ü K-11, K-81, K-175'e bağlıyor. | kanit: CONFIRMED · baglam: CONFIRMED | Doğrulandı | Uygulandı — S-3 Kaynak |
| KAP241-1 | kapsam-b | 01 §1 satır 47 (Ölçülebilir karşılığı) | ÇELİŞKİ | High | §1, manuel adım bütçesini K-454'ün niteleyicisi olmadan 'sipariş başına' mutlak tavan olarak yazıyor. K-454'e göre karışık sipariş dört adım edebilir; bu yüzden cümle karışık siparişte yanlış oluyor. | kanit: CONFIRMED · baglam: CONFIRMED | Doğrulandı | Uygulandı — §1 hat başına |
| KAP241-2 | kapsam-b | 01 §6 satır 192 ve §7 S-9 satır 236 (M-2 satır 205 ile karşılaştırma) | ÇELİŞKİ | Medium | 01, mağaza düzeyi ölçülerin dördünün de ve satış özetinin tamamının 'sipariş verisinden' türediğini yazıyor. Oysa M-2 (iletişim talebi sayısı) iletişim talebi kaydından sayılıyor ve K-441 bu ölçüyü satış özetine ekledi. | kanit: CONFIRMED · baglam: CONFIRMED | Doğrulandı | K-483 — §6, S-9 |
| KAP241-3 | kapsam-b | 01 §6 Ü-1 satır 199 | DIŞ_TUTARSIZLIK | Medium | Ü-1 '`10 §4.1`'in ön koşulları tamamlanır —' diyerek bir liste sayıyor, ama liste 10 §4.1'in on bir satırının yalnız bir alt kümesi. 10 ise Ü-1'in listenin tamamını sınadığını yazıyor. | kanit: CONFIRMED · baglam: CONFIRMED | Doğrulandı | Uygulandı — Ü-1 `10 §4.1` ile birebir |
| KAP241-4 | kapsam-b | 01 §2 satır 59 | KISMİ | Medium | K-478'e göre Google girişi kuruluma bağlıdır: Google uygulaması tanımlı değilse düğme görünmez. §2 ise Google girişini her kurulumda varmış gibi koşulsuz yazıyor. | kanit: CONFIRMED · baglam: PARTIAL | Doğrulandı | Uygulandı — §2 Google koşulu (K-478) |
| KAP241-5 | kapsam-b | 01 §8 V-2 satır 255 ve liste maddesi satır 250 | BELİRSİZLİK | Medium | V-2'nin 'yanlışsa' hücresindeki tetikleyici ölçülebilir değil ve dilbilgisel olarak yanlış sayıya bağlanıyor. K-474 tetikleyicinin ölçülebilir hâlini yazdı; 01 bu hâli göstermiyor. | kanit: CONFIRMED · baglam: CONFIRMED | Doğrulandı | Uygulandı — V-2 (K-474) |
| ATF-14-1 | atif-1 | 01 §4 "Ölçüye bağlanma" notu, satır 130 | ÇELİŞKİ | Medium | "Ziyaretçi tarafının hunisi devralınmaz" cümlesi K-19'un devrini tersine çeviriyor. K-416'ya göre de huninin bir basamağı (M-1) devralınmış durumda. | kanit: CONFIRMED · baglam: CONFIRMED | Doğrulandı | Uygulandı — §4 huni cümlesi |
| ATF-14-2 | atif-1 | 01 v0.6 notu (satır 19) ve tracker §2 etki sütunları — K-203, K-305, K-108, K-111, K-415, K-418, K-433 | GAP | Medium | Taslak tarihinden sonra alınan yedi karar §2–§4'e işlendi, ama satırlarında `01` işareti yok. v0.6 notundaki "yirmi iki karar" listesi de bunları saymıyor. Bu bir K-29 izlenebilirlik açığı. | kanit: CONFIRMED · baglam: CONFIRMED | Doğrulandı | Tracker — etki işaretleri; K-305 §2'den çıkarıldı (ICT-11) |
| ATF-14-3 | atif-1 | 01 §3.2, satır 86 (ve satır 100'deki Kaynak) | KISMİ | Low | "Yönetici hesapları müşteri tarafıyla aynı rejime tabidir" iddiası yalnız K-122'ye dayanıyor. K-122 ise yalnız bir taban kuruyor ve sıkılaştırmaya kapı bırakıyor. MVP'nin son durumunu (sıkılaştırma yok) K-313 karara bağladı. | kanit: PARTIAL · baglam: CONFIRMED | Doğrulandı | Uygulandı — §3.2 (K-122, K-313) |
| ATF-14-4 | atif-1 | 01 §1 Kaynak (satır 53) ve §4 Kaynak (satır 132) | KALİTE | Low | Kaynak satırlarında beş K numarası gövdede hiç kullanılmıyor: §1'de K-01, K-02 ve K-04; §4'te K-04 ve K-05. Ters yönde bir eksik yok: gövdede kullanılan her K, Kaynak listesinde var. | kanit: CONFIRMED · baglam: CONFIRMED | Doğrulandı | Uygulandı — Kaynak atıfları gövdeye bağlandı |
| ATF-58-1 | atif-2 | 01 §6 satır 192 (mağaza düzeyi açıklaması) ve §7 S-9 satır 236 | ÇELİŞKİ | Medium | 01, dört mağaza düzeyi ölçünün dördünün de sipariş verisinden türediğini (K-400) ve panelde yalnız sipariş verisinden türeyen bir satış özeti olduğunu söylüyor. Oysa M-2 (iletişim talebi sayısı) sipariş verisinden değil, iletişim talebi kaydından sayılıyor ve K-441 onu satış özetine bu kayıttan ekledi. | kanit: CONFIRMED · baglam: CONFIRMED | Doğrulandı | K-483 |
| ATF-58-2 | atif-2 | 01 §6 Ü-1 satır 199 | DIŞ_TUTARSIZLIK | Medium | Ü-1, "10 §4.1'in ön koşulları" ifadesinin ardından yedi kalemi tam liste gibi sayıyor. 10 §4.1'de ise on bir kurulum ön koşulu var (ÖK-1…ÖK-11) ve 10, Ü-1'in bu listenin tamamını sınadığını söylüyor. İlk yönetici hesabı, firma kimliği, açık ödeme yöntemi ve iade adresi Ü-1'de yok. | kanit: CONFIRMED · baglam: CONFIRMED | Doğrulandı | Uygulandı — Ü-1 (karar gerekmedi: K-476, K-477) |
| ATF-58-3 | atif-2 | 01 §7 S-7 satır 234 ve §7 Kaynak satırı 239 | YANLIŞ_ATIF | Medium | S-7, içerik tiplerinin kapalı olduğunu K-244'e dayandırıyor; oysa K-244 genel sayfanın alan setini ve yasal metinlerden ayrımını karara bağlıyor. Tip envanterini kapatan karar K-238, ama ne S-7'de ne de §7'nin Kaynak satırında geçiyor. Gövde ayrıca genel sayfayı (K-237) hiç anmıyor. | kanit: CONFIRMED · baglam: PARTIAL | Doğrulandı | Uygulandı — S-7'ye K-238 |
| ATF-58-4 | atif-2 | 01 §7 S-3 satır 230 ve §7 Kaynak satırı 239 | YANLIŞ_ATIF | Low | S-3'ün "Hizmet sabit fiyatla ve randevusuz satılır" cümlesi K-81'in tanımıdır, ama K-81 ne S-3'ün kaynak hücresinde ne de §7'nin Kaynak satırında var. K-470 S-3'ün kaynakları arasında K-81'i sayıyor. | kanit: CONFIRMED · baglam: CONFIRMED | Doğrulandı | Uygulandı — KAP1-4 ile aynı |
| ATF-58-5 | atif-2 | 01 §7 S-5 satır 232 (Neden kalıcı sütunu) | BELİRSİZLİK | Low | "stok yalnız varyant başına bir adet olarak tutulur" ifadesi "her varyanttan bir parça" diye okunabiliyor. K-50'ye göre stok, varyant başına tutulan sayısal bir adettir. | kanit: CONFIRMED · baglam: PARTIAL | Doğrulandı | Uygulandı — S-5 "tek bir sayı" |
| ATF-58-6 | atif-2 | 01 §6 Ü-3 satır 201 (ayrıca §8 V-3 satır 256) | KISMİ | Low | Ü-3 kriteri K-404'ün "sipariş başına üçü geçmez" ifadesini aynen taşıyor ve hemen ardından karışık siparişte adımların toplandığını yazıyor. Havale ile ödenen fiziksel + hizmet siparişi dört adım ettiği için ilk cümle kendi hücresinde yanlışlanıyor. K-454 bu ifadenin "hat başına" okunmasını karara bağlamıştı. | kanit: CONFIRMED · baglam: CONFIRMED | Doğrulandı | Uygulandı — Ü-3 ve V-3 |
| ATF-58-7 | atif-2 | 01 §6 Kaynak satırı 214 | GAP | Low | K-20 Ü-1'in gövdesinde alan adının dayanağı olarak geçiyor, ama §6'nın Kaynak satırında yok. | kanit: CONFIRMED · baglam: CONFIRMED | Doğrulandı | Uygulandı — §6 Kaynak'a K-20 |
| ATF-58-8 | atif-2 | 01 §5 Kaynak satırı 181 | KALİTE | Low | K-15 §5'in Kaynak satırında listeleniyor, ama §5'in gövdesinde hiç kullanılmıyor. | kanit: CONFIRMED · baglam: CONFIRMED | Doğrulandı | Uygulandı — §5.2'ye (K-15) |
| ICT-1 | ic-tutarlilik | 01 §6 satır 192 (mağaza düzeyi giriş maddesi) ↔ satır 205 (M-2) ↔ §7 S-9 satır 236 | ÇELİŞKİ | High | §6 dört mağaza ölçüsünün dördünün de sipariş verisinden türediğini söylüyor, oysa aynı tablonun M-2 satırı iletişim talebi sayısını iletişim talepleri kaydından sayıyor. S-9 da satış özetini 'yalnız sipariş verisinden türeyen' diye tanımlıyor; K-441'e göre ise özet iletişim talebi sayısını da gösteriyor. | kanit: CONFIRMED · baglam: CONFIRMED | Doğrulandı | K-483 |
| ICT-2 | ic-tutarlilik | 01 §6 Ü-1 satır 199; §1 satır 47 | DIŞ_TUTARSIZLIK | Medium | Ü-1, '10 §4.1'in ön koşulları'nı yedi kalemlik bir listeyle sayıyor. 10 §4.1 on iki maddelik (ÖK-1…ÖK-12) ve 'Ü-1 bu listenin tamamlanmasını sınar' diyor. Ü-1'in listesinde ilk yönetici hesabı, firma kimliği, açık ödeme yöntemi ve iade adresi yok. Ayrıca §1, Ü-1'i 'dış ön koşullar' diye özetliyor, ama Ü-1'in saydığı 'dört yasal metin' panelde tamamlanan satış kapısı koşulu. | kanit: CONFIRMED · baglam: CONFIRMED | Doğrulandı | Uygulandı — Ü-1 |
| ICT-3 | ic-tutarlilik | 01 §6 M-1 satır 204 | BELİRSİZLİK | Medium | M-1'in ölçüm hücresi Başarısız'ı 'ödemesi alınmamış sipariş' diye tanımlıyor. 02 sözlüğünde Başarısız terminal bir durum, 'ödemesi alınmadan kapanan sipariş' demek. 'Ödemesi henüz gelmemiş' sipariş Bekliyor'dur. Bu okumayla dönem sonunda süresi dolmamış bir havale siparişi M-1'de kaybedilmiş satış sayılır. | kanit: CONFIRMED · baglam: PARTIAL | Doğrulandı | Uygulandı — M-1 Başarısız tanımı |
| ICT-4 | ic-tutarlilik | 01 §1 'Ölçülebilir karşılığı' satır 47 ↔ §6 Ü-2 satır 200 | KISMİ | Medium | §1, problemin ana sonucu olan çift giriş yükünün karşılığını ('tek panelde bir kez girilir') Ü-2'ye bağlıyor. Ü-2 ise yalnız işlerin geliştirici olmadan panelden yapılabildiğini sınıyor; aynı bilginin bir kez girilip iki yüzde (tanıtım ve mağaza) tutarlı göründüğünü ölçen bir senaryo yazmıyor. | kanit: CONFIRMED · baglam: PARTIAL | Doğrulandı | Uygulandı — §1 ifade (D-1'e bağlandı) |
| ICT-5 | ic-tutarlilik | 01 §6 Ü-3 satır 201 | KISMİ | Low | Ü-3'ün envanteri ödeme yöntemini yalnız fiziksel siparişte ayırıyor: 'hizmet 1 · dijital 0'. Havale ile ödenen hizmet ve dijital siparişe 'ödendi' işareti bir adım daha ekler; bu kural 01'de yazılı değil, okuyan sayıları yöntemden bağımsız sanabilir. | kanit: CONFIRMED · baglam: CONFIRMED | Doğrulandı | Uygulandı — Ü-3 havale notu |
| ICT-6 | ic-tutarlilik | 01 §7 giriş satır 224 | BELİRSİZLİK | Low | Paragraf 'üç madde kalıcı kimlik sınırı değildir ve `10`'da yaşar' diyor. Cümlede ayrı madde olarak okunabilenler yalnız K-09 ve K-84; K-10 K-09'un yan cümlesine gömülü. Ardından gelen K-12 '10'da yaşamaz', S-1'in parçasıdır. Dayanak K-440 aynı yerleşimi 'dört madde' diye sayıyor. | kanit: REFUTED · baglam: PARTIAL | Tartışmalı | Kısmi — sayı korundu, K-10 ayrı madde yapıldı |
| ICT-7 | ic-tutarlilik | 01 §2 satır 59; §3.2 tablo satır 91–92 | KALİTE | Low | 02 §1.2 sözlüğünden küçük terim sapmaları var. (a) §2 ürün tipini 'tür' diye anıyor; sözlükte kavramın adı 'Tip' (ProductType), 02 §2 de 'üç ürün tipi' diyor. (b) §3.2'nin 'Kim' hücreleri misafir alıcıyı ve üye müşteriyi 'tüketici' diye tanımlıyor; sözlük ikisini 'alıcı' diye tanımlıyor, 'Tüketici'yi ise alıcının hukuki sıfatı olarak ayrı bir terim tutuyor. | kanit: PARTIAL · baglam: PARTIAL | Doğrulandı | Kısmi — "üç tip" uygulandı; Kim hücreleri değişmedi (iyileştirme) |
| ICT-8 | ic-tutarlilik | 01 §6 Kaynak satırı 214 | YANLIŞ_ATIF | Low | §6 gövdesinde atıf yapılan K-20 (Ü-1'in alan adı ön koşulu), bölümün Kaynak satırında yok. Diğer yedi bölümde gövdedeki her K atfı Kaynak satırında da geçiyor. | kanit: CONFIRMED · baglam: CONFIRMED | Doğrulandı | Uygulandı — ATF-58-7 ile aynı |
| ICT-9 | ic-tutarlilik | 01 §5.1 satır 144 ↔ §5.3 satır 168 | KISMİ | Low | §5.1'in destekleyici ekseni 'Sahiplik ve maliyet' diye adlandırılmış, ama maddede maliyete dair bir öğe yok; yalnız sahiplik öğeleri sayılıyor. Maliyet karşılığı §5.3'te ve §5.2'nin ısmarlama yazılım satırında duruyor. | kanit: CONFIRMED · baglam: PARTIAL | Doğrulandı | Uygulandı — §5.1'e maliyet öğesi (K-05) |
| ICT-10 | ic-tutarlilik | 01 tarihçe notları satır 12–13 ↔ tracker §1 '01 v0.8 kapsamı' | KALİTE | Low | Tarihçe notlarında Blok 4 ve Blok 6 güncellemeleri sürüm etiketi taşımıyor; sonraki notlar taşıyor (v0.5, v0.6, v0.7, v0.8). Tracker bu iki güncellemeyi v0.3 ve v0.4 olarak kaydediyor. Doküman içi tarihçe ile tracker sürüm sırası birebir eşlenemiyor. | kanit: CONFIRMED · baglam: CONFIRMED | Doğrulandı | Uygulandı — tarihçe notlarına v0.3, v0.4 |
| ICT-11 | ic-tutarlilik | 01 §2 satır 59 | KALİTE | Low | Şablon §2 için 'tek paragrafta; nasıl yaptığı değil, ne yaptığı' bekliyor. Paragraf tek parça, ama birkaç mekanizma ayrıntısı (kayıt açma, cevabın kanalı) 02 düzeyinde bir 'nasıl' taşıyor ve paragrafı uzatıyor. | kanit: PARTIAL · baglam: PARTIAL | Doğrulandı | Kısmi — K-305 ayrıntısı çıkarıldı, K-300 cümlesi kaldı |
| XR-1 | capraz-ref | 01 §7 S-5 (satır 232) ↔ 01 §8 V-1 (satır 254) ↔ 01 §1 (satır 29) ↔ 10 §4.2 SK-10 (satır 272) ↔ 02 §3 ölçek notu (satır 247), 02 §11.3 H-1 | ÇELİŞKİ | High | 'Tek depo' üç dokümanda üç farklı statüde: 01 §7 ve 10 SK-10'da kalıcı kimlik sınırı ('Kalkmaz'), 01 §8 V-1'de yanlışlanırsa yeniden tasarım doğuracak varsayım, 01 §1 ile 02 §3'te ise 'kabul, kural değil'. | kanit: CONFIRMED · baglam: CONFIRMED | Doğrulandı | K-479; `02` ölçek notu ve H-1 etki yansıtmada |
| XR-2 | capraz-ref | 01 §6 Ü-1 (satır 199) ve §1 (satır 47) ↔ 10 §4.1 (satır 233, ÖK-1…ÖK-12) | KISMİ | Medium | Ü-1 '10 §4.1'in ön koşulları' diye yedi kalem sayıyor; 10 §4.1 ise Ü-1'in listenin tamamını (ÖK-1…ÖK-11) sınadığını söylüyor. İlk yönetici hesabı (ÖK-7), firma kimliği (ÖK-8) ve açık ödeme yöntemi (ÖK-9) Ü-1'de yok. §1 ayrıca listeye 'dış ön koşullar' diyor, oysa yasal metinler panel tarafındaki koşullardır. | kanit: PARTIAL · baglam: CONFIRMED | Doğrulandı | Uygulandı — Ü-1 |
| XR-3 | capraz-ref | 01 §8 V-2 (satır 255) ve 'Yanlışlanan varsayımın sonucu' maddesi (satır 250) ↔ 10 §5 YH-2/YH-3, 10 §4.2 SK-3 ↔ 02 §11.3 H-2 (satır 1788) | YANLIŞ_ATIF | Medium | 01 §8, 10 §5'e 'otomasyon' diye işaret ediyor ama 10 §5'te bu adla bir aday yok. K-473 adayları 'kargo şirketi entegrasyonu' ve 'havale eşleştirmesi' olarak adlandırdı, K-474 de 'sürekli aşarsa' ifadesini ölçülebilir kıldı. İki karar 01 §8 V-2'yi işaret ettiği hâlde bu bölüme işlenmedi (K-29 kaçağı). | kanit: CONFIRMED · baglam: CONFIRMED | Doğrulandı | Uygulandı — §8; `02` H-2 etki yansıtmada |
| XR-4 | capraz-ref | 02 §1.3 'Aktör olmayanlar' (satır 261) ↔ 01 §3.2 (satır 97), 01 §7 S-1 | DIŞ_TUTARSIZLIK | Medium | 02 §1.3 çok kiracılığı 'K-01 ile elenen' diye anıyor. 01 §3.2 ve S-1'e göre ise çok kiracılı SaaS yol haritası adayıdır; K-01 yalnız pazaryerini eledi (K-10). | kanit: CONFIRMED · baglam: CONFIRMED | Doğrulandı | Etki yansıtma — `02 §1.3` |
| XR-5 | capraz-ref | 01 §6 Ü-3 'Nasıl ölçülür' (satır 201) ↔ 02 §10.5.1 (satır 1669-1676) ↔ 10 KP-51 (satır 106) | KISMİ | Medium | Ü-3'ün adım envanteri hizmet ve dijital siparişi ödeme yolundan bağımsız olarak 1 ve 0 adım sayıyor. 02 §10.5.1'e göre havale ile ödenen her sipariş 'ödendi' işaretiyle bir adım daha alır; havale ile ödenen hizmet 2, dijital 1 adımdır. | kanit: CONFIRMED · baglam: PARTIAL | Doğrulandı | Uygulandı — Ü-3; `10` KP-51 etki yansıtmada |
| XR-6 | capraz-ref | 01 §6 mağaza düzeyi maddesi (satır 192), 01 §7 S-9 (satır 236) ↔ 01 §6 M-2 (satır 205) ↔ 02 §10.6.2-§10.6.3 | KISMİ | Low | 01 dört mağaza ölçüsünün dördünün de 'sipariş verisinden' türediğini söylüyor. Oysa M-2 (iletişim talebi sayısı) sipariş verisinden değil, iletişim talebi kaydından sayılır; bunu 01'in kendi M-2 satırı ve 02 §10.6.3 da yazıyor. | kanit: CONFIRMED · baglam: CONFIRMED | Doğrulandı | K-483; `02 §3.34.6`, `§10.6.2`, `10` KP-53 etki yansıtmada |
| XR-7 | capraz-ref | 01 §2 (satır 59) ↔ 02 §1.2 sözlük 'Tip' / 'Ürün tipi' (satır 74-81), 02 §3.2.2 | DIŞ_TUTARSIZLIK | Low | 01 §2 ürün tipine 'tür' diyor: "üç tür ürün". 02 sözlüğünde bu kavramın tek adı 'Tip'tir (`ProductType`); 01'in kendisi de Ü-3'te 'tek tipli' diyor, 10 KP-6 da 'üç tipte' yazıyor. | kanit: CONFIRMED · baglam: CONFIRMED | Doğrulandı | Uygulandı — §2 "üç tipte" |

### Aksiyon Planı

- **Critical:** yok.
- **High:** tek deponun iki statüsü (K-479) · mağaza ölçülerinin veri kaynağı (K-483) · §1'in koşulsuz adım bütçesi — hepsi `01` v0.9'da kapandı.
- **Medium → `01`:** v0.9'da kapandı. **→ `02`:** §1.3 kiracılık cümlesi (XR-4), §3.34.6 ve §10.6.2'nin "sipariş verisi" ifadesi (K-483), §11.3 H-1 ve ölçek notu (K-479), H-2 (K-473, K-474) — etki yansıtmasında. **→ `10`:** KP-51, KP-53 — etki yansıtmasında. **→ tracker:** etki sütunu işaretleri (KAP1-3, ATF-14-2) kondu.
- **Low:** v0.9'da kapandı; ICT-7'nin "Kim" hücreleri bilinçli olarak değişmedi.

### Envanter ve Eşleştirme Detayı

#### Karar kaydı — `01`'i gösteren kararlar, K-01…K-240 (mercek: `kapsam-a`)

Toplam: 37 öğe (35 ✓, 2 ⚠, 0 ✗)

| ID | Öğe | Durum | Not |
|---|---|---|---|
| K-01 | Shopfolio tek bir firmanın kendi sitesidir; pazaryeri ve çok satıcılı model yoktur. | ✓ | §3.2 satır 97, §7 S-1 satır 228. §1 gövdesinde K-no ile anılmıyor, kaynak listesinde var; problem sınırı (satır 49) üzerinden karşılanmış. |
| K-02 | Yeniden kurulabilir ürün, kurulum başına bir firma; kimlik, içerik ve parametreler koda gömülmez. | ✓ | §2 satır 59, §3.1 tablo satır 80, §5.3 satır 168, §6 satır 194. |
| K-03 | Sektör bağımsız KOBİ; ölçek varsayımı birkaç yüz ürün ve tek depo; dikey daraltma ve B2B elendi. | ⚠ | §1, §3.1, §5.3, §6 ve §8 karşılıyor. Ancak 'tek depo' hem V-1'de yanlışlanabilir varsayım hem S-5'te kalıcı sınır olarak duruyor (KAP1-1). |
| K-04 | Birincil farklılaşma: tanıtım ile mağazanın eşit ağırlıkta buluşması; sahiplik ve sadelik destekleyici eksenler. | ✓ | §5.1 satır 140-145, §1 satır 41, §6 M-2 satır 205, V-7 satır 260. |
| K-05 | Ticari ürün, ilk müşteri belirsiz; gerçek geri bildirim yok, bu yüzden §8 varsayım listesi ve doğrulama yöntemleri. | ✓ | §1 satır 51, §3.1 satır 71, §5.3 satır 168, §6 satır 194, §8 satır 245 ve V-7. §4 gövdesinde K-no ile anılmıyor, kaynak listesinde var. |
| K-06 | Yönetim tek rol, çoklu kullanıcı; platform operatörü aktör değil. | ✓ | §3.2 satır 86 ve 97. Aktör sayısı K-97 ile dörde çıktı ve doğru yansıtılmış. |
| K-07 | Alıcı yalnız tüketicidir; 6502 korumaları istisnasız uygulanır, ticari alıcı rejimi yoktur. | ⚠ | §3.2 satır 98'de 'MVP'de' kaydıyla yazılmış. K-419/K-470 ve 10 SK-9'a göre bu kalıcı sınır (S-6); §3.2 §7'ye işaret etmiyor (KAP1-2). |
| K-08 | Kimlik panelden yönetilir ve işlem izine yazılır; kurulumdan yalnız sır ve altyapı gelir. | ✓ | §4.1 D-3 satır 114, §5.3 satır 175 (K-478 ile dört kalem), §5.1 satır 144. |
| K-09 | Uygulamada firma tekildir; ikinci müşteri ikinci kurulumdur. | ✓ | §2 satır 59, §5.1 satır 145. §7 yerleşimi K-440'a göre kalıcı sınır değil (satır 224); K-10'un eski talimatı K-440 ile okunmuş. |
| K-10 | Pazaryeri kalıcı sınır; çok kiracılı SaaS yol haritası adayı; MVP'de hazırlık yok. | ✓ | §3.2 satır 97, S-1 satır 228, satır 224. K-10'un '01 §7'ye iki satır' talimatı K-440 ile değişti; S-1 ikisini ayrı kavram olarak adlandırıyor. |
| K-11 | Yalnız doğrudan satış; 'fiyat sorunuz' yok; iletişim formu satış hattı değil; randevu modeli yok. | ✓ | §2 satır 59, §5.1 satır 145, S-2 satır 229, S-3 satır 230. |
| K-12 | Satıcı firmadır, platform ticari zincirde yer almaz; tahsilat firmanın hesabına; POS sözleşmesi dış ön koşul. | ✓ | S-1 satır 228 (K-440'a göre S-1'in parçası), S-4 satır 231, Ü-1 satır 199. |
| K-14 | Firma tipi seçilir, zorunlu alan seti tipe göre değişir; ETBİS kaydı dış ön koşul. | ✓ | §3.1 satır 76, §5.3 satır 177, Ü-1 satır 199. |
| K-15 | Problem merkezi: iki ayrı sistemi yönetme yükü ve tutarsızlık; alternatifler mağaza paketi, ajans, ısmarlama. | ✓ | §1 satır 31-45 taslağı birebir genişletiyor; D-1 satır 112, V-7 satır 260. Destekleyici eksenler §4.2 ve §5.3'te. |
| K-16 | Tek pazar Türkiye, tek dil Türkçe, tek para birimi TRY; çoklu dil yol haritası adayı. | ✓ | §3.1 satır 79, S-10 satır 237 ('Çoklu dil bu sınırın parçası değildir'), K-439 ile uyumlu. |
| K-18 | Gelir modeli: kurulum bedeli ve yıllık bakım/destek aboneliği; ürüne faturalandırma modülü yok. | ✓ | D-5 satır 116, §6 satır 194, V-5 ve V-6 satır 258-259. |
| K-19 | §4 üç aktör: firma birincil, ziyaretçi ve üye türev; her türev satır gerekçe taşır; §6 hedefleri firma tarafında. | ✓ | §4 satır 106-130; §6 satır 194 'Ağırlık firma tarafındadır (K-19)'. |
| K-20 | Site firmanın kendi alan adında; platform imzası varsayılan açık ve panelden kapatılabilir. | ✓ | D-4 satır 115, §5.3 satır 178, Ü-1 satır 199. |
| K-21 | Beş alternatif tanıtım × satış matrisinde; hücreler ne veriyor / ne vermiyor / neden orada; ısmarlama ayrı satırda, ayıran eksen bütçe. | ✓ | §5.2 satır 149-159; beş satır ve üç alan tam. |
| K-22 | MVP çıtası geniş; en dar çıtada yönetim tarafı ölçüsüz ve abonelik karşılıksız kalırdı. | ✓ | §6 satır 187 (çıta 10 §1'de, 10 satır 25 doğrulandı), satır 194. |
| K-23 | Bitti çizgisi: referans kurulum, canlı ortam, gerçek kart, 3DS, panelden işletme ve iade; kanıt geliştiriciye bağlı. | ✓ | Ü-4 satır 202, §6 satır 192, Ü-1 ve Ü-2 'referans kurulumda'. |
| K-25 | 10 §3 cevap sözlüğü; 'Hayır' kalıcı sınırdır ve 01 §7'de yaşar. | ✓ | §7 satır 222 K-470 ile birlikte; K-470 'K-25'in sözlüğü değişmez' diyor. |
| K-26 | 10 §4 'Kalkmaz' satırı kalıcı sınıra ve 01 §7'ye bağlanır. | ✓ | §7 satır 222 (K-26, K-471); 10 SK-8, SK-9, SK-10 S-satırlarını gösteriyor. |
| K-27 | İki hazır ana sayfa düzeni; taban kural ikisi birlikte; serbest sayfa kurgusu yok. | ✓ | D-2 satır 113, §5.1 satır 145, §5.3 satır 174. |
| K-28 | Blok tetikli taslak; kalite döngüsü sonda, 01 → 02 → 10 sırasıyla, doküman başına bir kez. | ✓ | Satır 9; 'Etki' sütunu bölüm değil yazım sırası gösteriyor. |
| K-39 | Satılabilir birim varyanttır; beden ve renk satan KOBİ hedef kitlede. | ✓ | §3.1 satır 82; S-5 satır 232 'stok varyant başına'. |
| K-40 | Ürün başına en fazla iki seçenek boyutu. | ✓ | §3.1 satır 82. |
| K-81 | Üç ürün türü: fiziksel, dijital, sabit fiyatlı randevusuz hizmet; ölçek varsayımı fiziksel tipe ait. | ✓ | §1 satır 29, §2 satır 59, §3.1 satır 77, V-1. S-3 içeriğini taşıyor ama S-3 kaynağında K-81 yok (KAP1-4). |
| K-84 | Ek akış isteyen ürünler (alkol, tütün, ilaç/reçeteli, silah) MVP kapsamı dışında, yalnız 10 §3'te. | ✓ | §7 satır 224 K-440'a göre '10'da yaşar' diyor. Liste 'alkol, ilaç gibi' kısaltmasıyla örnekleniyor; tam liste 10'da. |
| K-89 | Sistem ölçek tavanı uygulamaz; 'birkaç yüz ürün' bir kabuldür. | ✓ | §3.1 satır 77, V-1 satır 254. |
| K-97 | Misafir alışverişi var; takip sipariş no ve e-postayla; misafir alıcı dördüncü aktör. | ✓ | §2 satır 59, §3.2 satır 86 ve 91. |
| K-100 | Misafir alıcı yalnız §3.2 tablosuna girer; §4 üç aktörde kalır; 'bir kez alır, gider'. | ✓ | §3.2 satır 91, §4 satır 106. |
| K-103 | Sosyal giriş yalnız Google; Google uygulaması dış ön koşul. | ✓ | §2 satır 59, Ü-1 satır 199. |
| K-123 | Üyenin sepeti hesapta durur ve her cihazda aynıdır. | ✓ | §2 satır 59 'her cihazda aynı kalan sepet'. |
| K-142 | Kargo listesi ve takip kalıpları ürünün varlığı; bakımı abonelikle finanse edilir, gecikirse bağlantı kırılır. | ✓ | D-5 satır 116, V-6 satır 259. |
| K-159 | MVP iki ödeme yöntemi taşır: kart ve havale/EFT. | ✓ | §2 satır 59, §3.1 satır 82, V-3 satır 256. |
| K-178 | İptal ve iade kalem düzeyinde; gerekçe tüketicinin hakkı. | ✓ | §3.1 satır 82 'cayma ve iade … yasal hakkı olarak MVP'dedir (K-07, K-178, K-203)'. |

#### Karar kaydı — `01`'i gösteren kararlar, K-241…K-478 (mercek: `kapsam-b`)

Toplam: 39 öğe (34 ✓, 5 ⚠, 0 ✗)

| ID | Öğe | Durum | Not |
|---|---|---|---|
| K-245 | Hizmet tanıtımı ve referans iş katalogdan ürün bağlar; içerik sayfasında yalnız yayındaki ürünler 'İlgili ürünler' olarak görünür. | ✓ | §3.2 satır 90, §4.1 D-2 satır 113 ve §5.3 satır 174'te karşılığı var. |
| K-249 | Panelde sosyal medya bağlantıları ve WhatsApp numarası girilir; sitede gömülü gönderi akışı yoktur. | ✓ | §5.3 satır 179'da var. 'İsteğe bağlı' niteliği yazılmamış ama özü karşılanıyor. |
| K-258 | Firma logo, marka adı, site simgesi ve tek bir marka rengini ayarlar; yazı tipi, düzen ve bileşenler üründür. | ✓ | §5.3 satır 176'da var. |
| K-300 | İletişim formu giriş istemez ve her gönderim bir iletişim talebi kaydı açar. | ✓ | §2 satır 59'da var. |
| K-310 | İşlem izi belirli alanları kaydeder, değiştirilemez ve silinemez. | ✓ | §3.2 satır 86'da var. D-3'te 'kim/ne zaman' yazıyor, 'ne değişti' eksik; önemsiz bir fark. |
| K-341 | Veri sorumlusu firmadır; VERBİS gibi yükümlülükler firmada kalır. | ✓ | §4.1 D-5 satır 116'da var. |
| K-364 | Serbest metinle ayar arasındaki çelişkinin sorumluluğu firmadadır; bağlayıcı olan ayardan üretilen ön bilgilendirme ve sözleşmedir. | ✓ | §4.1 D-1 satır 112'de var. |
| K-397 | Ziyaretçi analitiği yoktur; dönüşüm ürünün içinde ölçülmez. | ✓ | §4.2 satır 122, §4 not satır 130, §6 satır 209-210 ve S-9'da var. |
| K-400 | Başarı ölçüleri sipariş verisinden türetilebilenlerle kurulur; huninin üst basamakları ölçülemez. | ✓ | §4 satır 130 ve §6 satır 211'de var. 'Yalnız sipariş verisi' ifadesinin M-2 ile çelişkisi KAP241-2'de. |
| K-403 | Günde ortalama 10, tepe günde 50 sipariş; bu bir tasarım varsayımıdır, hedef değildir. | ✓ | §1 satır 29, §6 satır 212 ve V-2'de var. |
| K-410 | Veri firmanındır; abonelik sona erse de kurulum çalışmaya devam eder. | ✓ | V-5 satır 258'de var. |
| K-412 | Üç katman: işletme parametreleri panelden gelir; güvenliğe, yasal uyuma ya da müşteriye dokunan sayılar ürün sabitidir. | ✓ | §5.3 satır 168'de var. |
| K-415 | Ürün düzeyinde dört kriter vardır ve dördü de 12'de sınanır. | ⚠ | Ü-1…Ü-4 satır 199-202'de var. Ü-1'in ön koşul listesi 10 §4.1'in alt kümesi olduğu halde tamamı gibi sunuluyor (KAP241-3). |
| K-416 | Mağaza düzeyinde dört ölçü ve ölçülemeyenler listesi. | ✓ | M-1…M-4 ve ölçülemeyenler bölümü var. Kararın kendi 'dördü de sipariş verisinden' ifadesinin sonucu KAP241-2'de. |
| K-417 | Sipariş hacmi başarı kriteri değildir. | ✓ | §1 satır 47, §6 satır 194 ve 212'de var. |
| K-418 | Kabul kapısı ürün düzeyidir; mağaza ölçüleri ilk gerçek kurulumun ilk üç ayında toplanır. | ✓ | §6 satır 191-192 ve §8 satır 249'da var. |
| K-419 | On kalıcı sınır vardır; §1 problemin sınırı için bu listeye işaret eder. | ✓ | §7 S-1…S-10 ve §1 satır 49'da var. K-439'un adlandırma değişiklikleri de uygulanmış. |
| K-420 | 01 §7 kalıcı sınırdır, 10 §3 MVP dışıdır; yol haritası yalnız 10 §3'ten beslenir. | ✓ | §7 satır 220-222'de var. |
| K-421 | Yedi ürün varsayımı vardır ve her satır varsayım, yanlışsa sonucu ve doğrulama yöntemini taşır. | ✓ | §8 V-1…V-7'de var. V-3 ve V-7 K-445'in düzeltmeleriyle yazılmış. |
| K-428 | Aşama kapanışında 01'e ait açık satırlar 01'in açık kararlar bölümüne taşınır. | ✓ | 01'e ait A-02, A-03 ve A-04 kapalı; taşınacak satır yok. Bölümün yokluğu kayıtlı bilinen durum olduğu için bulgu sayılmadı. |
| K-429 | Aşama kapanışında açık kalem çakışma taraması yapılır. | ✓ | Doküman içinde karşılık gerektirmiyor; 01'de açık kalem yok. |
| K-433 | §6 tek tablodur: iki eksen, dört sütun ve ölçülemeyenler bölümü. | ✓ | §6 satır 189-212 ve §1 satır 47'de var. |
| K-434 | 10 §1 çıtayı yazar, 01 §6 ölçüleri yazar; ikisi aynı şeyi tekrarlamaz. | ✓ | §6 satır 187'de var. |
| K-436 | Tracker arşivlenince K numaraları çözülmeye devam eder. | ✓ | Satır 23'teki karar referansları notu bunu karşılıyor. |
| K-439 | Kalıcı sınır ile yol haritası adayı aynı alana düşebilir; S-4 adı 'muhasebe' oldu, S-10 tek pazar ve tek para birimine daraldı. | ✓ | §7 satır 222 ile S-4, S-7, S-8, S-9 ve S-10'da var. |
| K-440 | K-12 S-1'in parçasıdır; SaaS, tek firma ve K-84 kalıcı sınır değildir. | ✓ | §7 satır 224, S-1 ve §3.2 satır 97'de var. |
| K-441 | Satış özeti mağaza düzeyinin dört ölçüsünü de gösterir; iletişim talebi sayısı da eklenir. | ⚠ | §6 satır 192 kaynakta var, ama S-9 satır 236 ve satır 192 özeti 'yalnız sipariş verisinden' diye tanımlıyor (KAP241-2). |
| K-442 | Matrisin derece sözlüğü ve beş grubun dereceleri. | ✓ | §5.2 satır 151-164'te birebir var. |
| K-443 | Aktör tablosunun 'neden geri döner' hücreleri. | ✓ | §3.2 satır 90-93'te birebir var. |
| K-444 | Ü-1'e ETBİS kaydı, Ü-2'ye firma kimliğini güncelleme eklenir. | ✓ | Satır 199 ve 200'de var. |
| K-445 | Doğrulama ilk gerçek kurulumla başlar; an satıra göre değişir; K-04 V-7'nin hücresinde anılır. | ✓ | §8 satır 249 ve V-7 satır 260'ta var. |
| K-454 | Adım bütçesi tek tipli siparişin hattına uygulanır; karışık siparişte adımlar toplanır. | ⚠ | Ü-3 satır 201'de var, ama §1 satır 47 niteleyicisiz bir mutlak tavan yazıyor (KAP241-1). |
| K-457 | M-1'in birimi sepettir; M-2 KVKK ve sipariş başvurularını saymaz. | ✓ | M-1 satır 204 ve M-2 satır 205'te var. |
| K-470 | 10 §3 'Hayır' kullanmaz; kalıcı sınıra düşen kalem 01 §7'ye işaretle anılır. | ✓ | §7 satır 222'de var. |
| K-471 | 10 §4.2'nin 'Kalkmaz' satırları 01 §7'deki satırı gösterir. | ✓ | §7 satır 222'de var. |
| K-474 | Hacim tetikleyicisinin ölçülebilir hâli; ortalama 10, V-2'nin doğrulama ölçüsüdür. | ⚠ | Etki sütununda `01 §8` V-2'ye atıf var. V-2'deki tetikleyici ölçülebilir değil ve '150 panel işlemi'ne bağlanıyor (KAP241-5). |
| K-475 | 10 §4.2'nin tetikleyicileri V-1'in katalog ölçümüne bağlanır. | ✓ | Etki sütunu yalnız atıf; V-1'in doğrulama yöntemi ('İlk gerçek kurulumun katalog boyutu') bununla tutarlı. 01'de değişiklik gerekmiyor. |
| K-477 | Ü-1'in ön koşullarına alan adı eklenir. | ✓ | Satır 199'da 'alan adı (K-20)' var. |
| K-478 | Google uygulamasının kimlik bilgileri kurulum ayarıdır; kurulum ayarı listesi dörde çıkar; tanımlı değilse Google girişi görünmez. | ⚠ | §5.3 satır 175'te var, ama §2 satır 59 Google girişini koşulsuz yazıyor (KAP241-4). |

#### `01` iç envanteri — §1–§4'ün K atıfları (mercek: `atif-1`)

Toplam: 73 öğe (66 ✓, 7 ⚠, 0 ✗)

| ID | Öğe | Durum | Not |
|---|---|---|---|
| §1/K-03 | Sektör bağımsız KOBİ; birkaç yüz ürün, tek depo | ✓ | K-03 kararı birebir |
| §1/K-81 | Ölçek fiziksel ürüne ait | ✓ | K-81: 'artık fiziksel tipin varsayımıdır' |
| §1/K-403 | Günde ortalama 10, tepe 50 sipariş | ✓ | Sayılar birebir |
| §1/K-417 | Satış hacmi iddiası yapılmaz, ürün sorumlu tutulmaz | ✓ | Birebir |
| §1/K-433 | Ölçülebilir karşılık §6'nın ürün düzeyi eksenine bağlanır | ✓ | K-433 'A-04 için yön' cümlesi |
| §1/K-419 | Problemin sınırı §7'de | ✓ | K-419 etki: 01 §1 |
| §1/K-05 | Problem tanımı gerçek kullanıcı geri bildirimiyle doğrulanmadı | ✓ | K-05 'bedeli bilinçlidir' |
| §1/K-421 | İlk kurulumun firma sahibiyle görüşmede sınanır (V-7) | ✓ | K-421 madde (7) |
| §1/K-15 | Problem merkezi (yalnız Kaynak'ta etiketli) | ✓ | Gövde K-15'in onaylı taslağıyla örtüşüyor |
| §1/K-01 | Yalnız Kaynak'ta | ⚠ | Gövdede kullanılmıyor (ATF-14-4) |
| §1/K-02 | Yalnız Kaynak'ta | ⚠ | Gövdede kullanılmıyor (ATF-14-4) |
| §1/K-04 | Yalnız Kaynak'ta | ⚠ | Gövdede kullanılmıyor (ATF-14-4) |
| §2/K-159 | Kart ya da havale/EFT ile öder | ✓ | İki yöntem |
| §2/K-97 | Üyelik gerekmez; misafir sipariş no + e-posta ile izler | ✓ | Birebir |
| §2/K-103 | Yalnız Google ile giriş | ✓ | Birebir |
| §2/K-108 | Hatırlanan oturum | ✓ | 'Beni hatırla'; etki sütununda 01 işareti yok (ATF-14-2) |
| §2/K-111 | Adres defteri | ✓ | Etki sütununda 01 işareti yok (ATF-14-2) |
| §2/K-123 | Her cihazda aynı kalan sepet | ✓ | Birebir |
| §2/K-11 | Doğrudan satış; 'fiyat sorunuz', teklif ve talep hattı yok | ✓ | Birebir |
| §2/K-300 | Form bir iletişim talebi kaydı açar | ✓ | ContactRequest |
| §2/K-305 | Cevap sistem dışında e-postayla verilir | ✓ | İddia doğru; etki sütununda 01 işareti yok (ATF-14-2) |
| §2/K-81 | Üç ürün türü, aynı katalog/sepet/satın alma | ✓ | Birebir |
| §2/K-02 | Kurulum başına tek firma | ✓ | Birebir |
| §2/K-09 | İkinci müşteri ikinci kurulumdur | ✓ | Birebir |
| §3/K-05 | Ticari ürün; ilk müşteri belli değil | ✓ | Birebir |
| §3/K-03 | Segment, tek depo, dikey daraltma ve B2B'nin elenmesi | ✓ | Gerekçe cümleleri örtüşüyor |
| §3/K-14 | Şahıs/tüzel; zorunlu alanlar tipe göre değişir | ✓ | Birebir |
| §3/K-81 | Katalog ölçeği fiziksel üründe | ✓ | Birebir |
| §3/K-89 | Ürün adedine tavan yok; kabuldür | ✓ | Birebir |
| §3/K-16 | Türkiye, Türkçe, TRY | ✓ | Birebir |
| §3/K-02 | Kurulum başına bir firma | ✓ | Birebir |
| §3/K-08 | Firma kimliği koda gömülmez, panelden yönetilir | ✓ | Birebir |
| §3/K-40 | Dikey paket reddi; en fazla iki seçenek boyutu | ✓ | K-40'ın K-03 yorumuyla örtüşüyor |
| §3/K-39 | Satılabilir birim varyanttır | ✓ | Birebir |
| §3/K-07 | Alıcı yalnız tüketici; 6502 her siparişte geçerli | ✓ | Birebir |
| §3/K-178 | İade tüketicinin yasal hakkıdır | ✓ | 'Gerekçe tercih değil hak' |
| §3/K-203 | Cayma MVP'de | ✓ | İddia doğru; etki sütununda 01 işareti yok (ATF-14-2) |
| §3/K-159 | B2B cümlesinde ödeme yolu kart ya da havale/EFT | ✓ | K-03'ün 'kartla ödeme'si K-159 ile güncellendi |
| §3/K-97 | Dört aktör; misafir alıcı | ✓ | Birebir |
| §3/K-06 | Tek rol, çoklu kullanıcı; operatör aktör değil | ✓ | Birebir |
| §3/K-310 | İşlem izi belirli alanlarda tutulur, değiştirilemez | ✓ | Beş alan, değiştirilemez |
| §3/K-122 | Yönetici müşteriyle aynı rejime tabidir | ⚠ | K-122 yalnız taban; nihai durum K-313'te (ATF-14-3) |
| §3/K-15 | Ziyaretçi hücresi | ✓ | K-443'teki metinle birebir |
| §3/K-19 | Ziyaretçi, üye ve yönetici hücreleri | ✓ | K-443: 'üye hücresi K-19'dan gelir' |
| §3/K-245 | İçerik sayfaları ilgili ürünleri gösterir | ✓ | Birebir |
| §3/K-100 | Misafir: bir kez alır, gider | ✓ | Birebir |
| §3/K-98 | Doğrulanmış e-postayla geçmiş siparişler hesaba düşer | ✓ | Birebir |
| §3/K-405 | Bekleyen işler panel ana sayfasında sayılır | ✓ | Birebir |
| §3/K-392 | Telefondan da yapılabilir | ✓ | Panelde hiçbir işlev mobilde kapatılmaz |
| §3/K-01 | Tek firmanın kendi sitesi | ✓ | Birebir |
| §3/K-10 | SaaS yok, hazırlık yapılmaz, yol haritası adayı | ✓ | Birebir |
| §3/K-440 | SaaS kalıcı sınır değil | ✓ | Madde (2) |
| §3/K-443 | 'Neden geri döner' hücreleri (Kaynak'ta etiketli) | ✓ | Üç hücre birebir |
| §4/K-18 | Ürünü satın alan firmadır; abonelik uyumu finanse eder | ✓ | K-19 gerekçesi ve K-18'in mevzuat listesi |
| §4/K-19 | Firma birincil, diğer iki aktör türev; 'neden yazılır' gerekçesi | ✓ | Birebir |
| §4/K-97 | Misafire değer satırı yok (zorunsuz üyeliğin kendisi) | ✓ | K-100 ile birlikte |
| §4/K-100 | §4.4 açılmaz | ✓ | Birebir |
| §4/K-15 | D-1 problemi | ✓ | Birebir |
| §4/K-364 | Ayarlardan üretilen metin bağlayıcıdır, çelişkinin sorumluluğu firmada | ✓ | Birebir |
| §4/K-27 | İki ana sayfa düzeni; firma yalnız sırayı seçer | ✓ | Birebir |
| §4/K-245 | İlgili ürünler, yalnız yayındakiler | ✓ | Birebir |
| §4/K-08 | Kimlik panelden; değişiklik işlem izine yazılır | ✓ | Birebir |
| §4/K-20 | Kendi alan adı; imza kapatılabilir | ✓ | Birebir |
| §4/K-142 | Kargo takip sayfası değişimi bakım konusudur | ✓ | Bedel cümlesi |
| §4/K-341 | Veri sorumlusu firmadır; VERBİS firmada | ✓ | 'Gerekiyorsa' koşulu ile uyumlu |
| §4/K-397 | Dönüşüm ölçülmez; 'ziyaretçi tarafının hunisi devralınmaz' | ⚠ | §4.2 doğru; L130'daki 'devralınmaz' K-19 ve K-416 ile çelişiyor (ATF-14-1) |
| §4/K-416 | Dönüşüm ölçülemeyenler arasında; M-1 hunideki tek basamak | ✓ | Birebir; 01 §4 işareti var |
| §4/K-415 | Kabul kapısı ürün düzeyinde | ✓ | İddia doğru; etki sütununda §4 işareti yok (ATF-14-2) |
| §4/K-418 | Kabul kapısı B9-09 | ✓ | İddia doğru; etki sütununda §4 işareti yok (ATF-14-2) |
| §4/K-400 | Hunide yalnız sipariş verisi | ✓ | Birebir |
| §4/K-433 | Ölçülemeyenler §6 tablosunda | ✓ | İddia doğru; etki sütununda §4 işareti yok (ATF-14-2) |
| §4/K-04 | Yalnız Kaynak'ta | ⚠ | Gövdede kullanılmıyor (ATF-14-4) |
| §4/K-05 | Yalnız Kaynak'ta | ⚠ | Gövdede kullanılmıyor (ATF-14-4) |

#### `01` iç envanteri — §5–§8'in K atıfları (mercek: `atif-2`)

Toplam: 114 öğe (110 ✓, 4 ⚠, 0 ✗)

| ID | Öğe | Durum | Not |
|---|---|---|---|
| §5/K-02 | Firma kimliği, içerik ve işletme parametreleri koda gömülmez; ürün yeniden kurulabilir | ✓ | K-02 karar cümlesiyle birebir |
| §5/K-03 | Sektör bağımsız KOBİ kapsanır (firma tipi satırı) | ✓ | K-03 segment |
| §5/K-04 | Birincil eksen eşit ağırlıkta buluşma; destekleyici eksenler birincil değil; Shop+folio | ✓ | Karar ve gerekçe cümleleriyle birebir |
| §5/K-05 | Geliştirme maliyeti birden çok kuruluma yayılır | ✓ | K-05'in 'birden çok KOBİ'ye kurulacak ticari ürün' kararından türetilmiş |
| §5/K-08 | Kimlik panelden yönetilir; kurulumdan sır ve altyapı gelir | ✓ | K-08'in sınır ilkesi ve panel/kurulum listesi |
| §5/K-09 | Firma seçici yok | ✓ | K-09: firma seçme kavramı yok |
| §5/K-11 | Teklif hattı yok | ✓ | K-11 |
| §5/K-14 | Firma tipi seçilir; zorunlu alan seti tipe göre değişir; tek set şahıs işletmesini dışarıda bırakırdı | ✓ | K-14 karar ve gerekçe |
| §5/K-15 | Yalnız Kaynak satırında geçiyor | ✓ | Gövdede kullanılmıyor, bkz. ATF-58-8 |
| §5/K-20 | Kendi alan adı, alt alan adı yok, imza varsayılan açık ve kapatılabilir | ✓ | K-20 birebir |
| §5/K-21 | Beş alternatif, tanıtım × satış matrisi, ısmarlama yazılım ayrı satırda, ayıran eksen bütçe | ✓ | K-21 birebir |
| §5/K-27 | İki hazır düzen; ikisinde de tanıtım ve vitrin birlikte; serbest sayfa kurgusu yok | ✓ | K-27 taban kural |
| §5/K-245 | İçerik sayfası katalogdan ürün bağlar, 'İlgili ürünler' bloğunda yalnız yayındaki ürünleri gösterir | ✓ | K-245 |
| §5/K-249 | Sosyal medya bağlantıları ve WhatsApp numarası; gömülü akış yok | ✓ | K-249 |
| §5/K-258 | Logo, marka adı, site simgesi ve tek marka rengi; siteler yazı tipi ve düzen olarak benzer | ✓ | K-258 kararı ve 'bedeli' cümlesiyle birebir |
| §5/K-412 | İşletme parametreleri panelden; güvenliğe, yasal uyuma ya da müşteriye dokunan sayılar ürün sabiti | ✓ | K-412 atama ölçütü |
| §5/K-417 | Satış gücü alıcı trafiğini ölçmez | ✓ | K-442 üzerinden K-417 |
| §5/K-442 | Derece sözlüğü ve beş grubun dereceleri | ✓ | Tanımlar ve beş derece çifti birebir |
| §5/K-478 | Kurulumdan dört kalem gelir: alan adı, e-posta gönderim kimliği, ödeme anahtarları, Google kimlik bilgileri | ✓ | K-478: liste dörde çıkar |
| §6/K-02 | Ürün düzeyi kriterler yeniden kurulabilir ürün iddiasının sınavıdır | ✓ | Türetilmiş, kararla uyumlu |
| §6/K-03 | Ürünü satın alan sektör bağımsız KOBİ | ✓ | K-03 |
| §6/K-04 | M-2: tanıtım eşit ağırlıkta olduğu için ölçüsüz bırakılmaz | ✓ | K-416 gerekçesi K-04'e dayanıyor |
| §6/K-05 | Birden çok firmaya kurulacak ürün | ✓ | K-05 |
| §6/K-12 | Ü-1: firmanın ödeme sağlayıcı sözleşmesi bir ön koşul | ✓ | K-12: dış ön koşul |
| §6/K-14 | Ü-1: ETBİS kaydı bir ön koşul | ✓ | K-14 ve K-444 |
| §6/K-18 | En dar çıtada bakım aboneliği karşılıksız görünürdü | ✓ | K-22 gerekçesinde K-18 |
| §6/K-19 | Ağırlık firma tarafında | ✓ | K-19: §6 hedefleri firma tarafında toplanır |
| §6/K-20 | Ü-1: alan adı | ✓ | Doğru atıf ama Kaynak satırında yok, bkz. ATF-58-7 |
| §6/K-22 | En dar çıtada yönetim tarafı ölçüsüz kalırdı | ✓ | K-22 gerekçesi |
| §6/K-23 | Kabul, referans kurulumda verilen tek gerçek siparişe dayanır; Ü-4 bitti çizgisi ve kanıtın geliştiriciye bağlı olması | ✓ | K-23 birebir |
| §6/K-103 | Ü-1: Google uygulaması | ✓ | K-103: dış ön koşul (K-478'e göre koşullu, referans kurulumda tamamlanır) |
| §6/K-139 | M-3: vitrindeki kargoya verme sözüne uyum oranı | ✓ | K-139 sözü, K-416 ölçüyü tanımlıyor |
| §6/K-168 | M-3: süre ödemenin onaylandığı andan başlar | ✓ | K-168 |
| §6/K-180 | M-1: ödemesi alınmamış sipariş Başarısız kaydı taşır | ✓ | K-180 |
| §6/K-195 | M-4: iptal kayıtları | ✓ | K-195 ve K-398 |
| §6/K-203 | Cayma penceresi on dört gün | ✓ | K-203: on dört gün, sabit |
| §6/K-209 | Geri ödeme çatısı on dört gün | ✓ | K-209 |
| §6/K-296 | M-1: ödenmeden iptal edilen sipariş Başarısız olur | ✓ | K-296 |
| §6/K-300 | M-2: iletişim talebi kaydı | ✓ | K-300: her form bir ContactRequest kaydı açar |
| §6/K-303 | M-2: iletişim talebi kaydı ve durumu | ✓ | K-303 |
| §6/K-337 | M-3: süre iş günüyle sayılır | ✓ | K-337: kargoya verme süresi iş günüyle sayılır |
| §6/K-358 | Ü-1: barındırma konumu | ✓ | K-358: 10 §4'e dış ön koşul |
| §6/K-365 | Ü-1: dört yasal metin | ✓ | K-365: dört metnin dördü de tamamlanmadan sipariş alınamaz |
| §6/K-378 | Ü-1: e-posta yapılandırması | ✓ | K-378 |
| §6/K-397 | Ziyaretçi sayısı ve dönüşüm oranı ölçülmez | ✓ | K-397 |
| §6/K-398 | Ölçüler satış özetinden okunur; yeni veri tutulmaz | ✓ | K-398 ve K-441 |
| §6/K-400 | Dört ölçünün dördü de sipariş verisinden türer | ⚠ | M-2 iletişim talebi kaydından sayılıyor (K-441), bkz. ATF-58-1 |
| §6/K-401 | Ü-2 sipariş adımları; Ü-3 envanteri 2/3/1/0 | ✓ | Sayılar birebir |
| §6/K-402 | Fatura sipariş başına bir sistem dışı adım, envanterde açıkça yazılı | ✓ | K-402 |
| §6/K-403 | Günde ortalama 10, tepe 50 bir tasarım varsayımı, hedef değil | ✓ | Sayılar birebir |
| §6/K-404 | Sipariş başına üçü geçmez; kural ileriye dönük | ✓ | K-404 birebir; K-454 okunuşu için bkz. ATF-58-6 |
| §6/K-415 | Dört ürün düzeyi kriteri 12'de sınanır; Ü-1 ve Ü-2 listeleri | ✓ | K-415, K-444 ve K-477 ile uyumlu; 10 §4.1 ile fark var, bkz. ATF-58-2 |
| §6/K-416 | Dört mağaza ölçüsü, M-1 tek huni basamağı, ölçülemeyenler | ✓ | K-416 birebir |
| §6/K-417 | Sipariş hacmi kriter değil; ürünün sorumluluğu satışı kaybetmemek | ✓ | K-417 birebir |
| §6/K-418 | Kabul kapısı ürün düzeyi; ölçüm ilk gerçek kurulumun ilk üç ayı; bir ay iade oranını yarım gösterir | ✓ | K-418 birebir |
| §6/K-433 | Tek tablo, iki eksen, dört sütun, ölçülemeyenler bölümü | ✓ | K-433 |
| §6/K-434 | Çıta 10 §1'de; tablo değişirse 10 §1 değişmez | ✓ | K-434 birebir |
| §6/K-441 | Satış özetinin içeriği dört ölçüyü taşır | ✓ | K-441 |
| §6/K-444 | Ü-1'e ETBİS, Ü-2'ye firma kimliğini güncelleme eklendi | ✓ | K-444 |
| §6/K-454 | Bütçe tek tipli siparişin hattına uygulanır; karışık siparişte adımlar toplanır | ✓ | K-454; 'sipariş başına' ifadesi için bkz. ATF-58-6 |
| §6/K-457 | M-1'in birimi sepet; M-2 KVKK talebini ve sipariş hakkındaki başvuruyu saymaz | ✓ | K-457 birebir |
| §6/K-477 | Ü-1'e alan adı eklendi | ✓ | K-477 |
| §7/K-01 | Pazaryeri değil; satıcı onboarding'i, yetkilendirme, komisyon ve mutabakat | ✓ | K-01 gerekçesi birebir |
| §7/K-03 | S-5 tek depo; S-6 B2B açık fiyat + sepet + ödeme akışını değiştirirdi | ✓ | K-03 |
| §7/K-07 | S-6: tek hukuki rejim tüketici | ✓ | K-07 |
| §7/K-09 | Panelde tek firma bugünkü tanımın kabulü | ✓ | K-440 ile birlikte |
| §7/K-10 | Çok kiracılı SaaS yol haritası adayı, kalıcı sınır değil | ✓ | K-10 ve K-440 |
| §7/K-11 | S-2 teklif hattı yükleri; S-3 randevu modeli yok | ✓ | K-11 (dört yükten üçü sayılmış; liste kapalı iddia edilmiyor) |
| §7/K-12 | Platform ticari zincirde değil; tahsilatı platformda toplamak lisans ister | ✓ | K-12 birebir |
| §7/K-16 | S-10: tek pazar ve tek para birimi; hukuki katman Türkiye'ye çivili | ✓ | K-16 ve K-439 |
| §7/K-25 | 10 §3 'Hayır' cevabını kullanmaz | ✓ | K-470, K-25'in sözlüğünü koruyarak yerleşimi daralttı; iki atıf birlikte doğru |
| §7/K-26 | 10 §4.2'de 'Kalkmaz' diyen kısıt bu bölüme bağlanır | ✓ | K-26 ve K-471 |
| §7/K-50 | S-5: stok varyant başına 'bir adet' olarak tutulur | ⚠ | İfade belirsiz; K-50'ye göre sayısal adet, bkz. ATF-58-5 |
| §7/K-84 | Ek akış isteyen ürünler 10 §3'te yaşar | ✓ | K-84 ve K-440 |
| §7/K-175 | S-3: tek 'tamamlandı' işareti randevu yönetimi değildir | ✓ | K-175 birebir; K-81 atfı eksik, bkz. ATF-58-4 |
| §7/K-217 | S-4: fatura ürün dışında; mali sorumluluk firma tarafında | ✓ | K-217 |
| §7/K-237 | S-7: tipin kendi alan seti var; ana sayfa tiplerden beslenir | ✓ | K-237 |
| §7/K-244 | S-7: tipler kapalıdır | ⚠ | K-244 genel sayfa kararı; tip envanteri K-238'de, bkz. ATF-58-3 |
| §7/K-263 | S-9: ziyaretçi ölçümü üçüncü taraf kodu gerektirir | ✓ | K-263 |
| §7/K-267 | S-7: biçim seti kapalı | ✓ | K-267 |
| §7/K-323 | S-8: İYS'li pazarlama iletisi uyum yükü taşır | ✓ | K-323 |
| §7/K-324 | S-8: pazarlama onayı toplanmaz | ✓ | K-324 |
| §7/K-397 | S-9: ziyaretçi ayırt edici iz gerektirir ve çerez rejimini değiştirir | ✓ | K-397 gerekçesi |
| §7/K-398 | S-9: panelde yalnız sipariş verisinden türeyen satış özeti var | ⚠ | K-441 özete iletişim talebi sayısını ekledi, bkz. ATF-58-1 |
| §7/K-400 | S-9: ürünün ölçümü sipariş verisiyle sınırlı | ✓ | K-400 kararının metni |
| §7/K-417 | S-8: talep yaratmak firmanın işi | ✓ | K-417 |
| §7/K-419 | Sınır listesi ve kimliği tanımlayanlarla sınırlı oluşu | ✓ | K-419; S-4 ve S-10 K-439 ile daraltılmış hâliyle |
| §7/K-420 | Kalıcı sınır post-MVP'de de değişmez; yol haritası yalnız 10 §3'ten beslenir | ✓ | K-420 birebir |
| §7/K-426 | S-4: sistem fatura verisini dışa aktarır | ✓ | K-426 |
| §7/K-439 | Aynı alandaki aday sınırı bozmaz; 'muhasebe sistemi'; çoklu dil sınır dışı | ✓ | K-439'un üç maddesi birebir |
| §7/K-440 | Önceki kararların yönelttiği maddeler 10'da; K-12 S-1'in parçası | ✓ | K-440'ın dört maddesi: üçü 10'da, biri S-1'de |
| §7/K-470 | 10 §3 'Hayır'ı kullanmaz, işaretle anılır | ✓ | K-470 |
| §7/K-471 | 'Kalkmaz' satırı bu bölümdeki satırı gösterir | ✓ | K-471 |
| §8/K-03 | V-1: KOBİ ölçeği | ✓ | K-03 ve K-421 (1) |
| §8/K-04 | V-7: farklılaşma ekseni problem tanımıyla birlikte boşa düşer | ✓ | K-445 |
| §8/K-05 | Gerçek kullanıcı geri bildirimi yok; ilk müşteri belli değil | ✓ | K-05 |
| §8/K-15 | V-7: problem tanımı | ✓ | K-15 ve K-445 |
| §8/K-18 | V-5 abonelik sözleşmesi; V-6 abonelik modeli ve bakımsızlık sonucu | ✓ | K-18 gerekçesi |
| §8/K-23 | Doğrulama referans kurulumla değil, ilk gerçek kurulumla başlar | ✓ | K-445 |
| §8/K-81 | V-1: ölçek fiziksel ürüne ait | ✓ | K-81: K-03'ün varsayımı fiziksel tipe daraltıldı |
| §8/K-89 | V-1: kabuldür, ürün adedine tavan yok | ✓ | K-89 |
| §8/K-142 | V-6: 'Takip et' bağlantısı kırılır, numara görünür kalır | ✓ | K-142 bedeli birebir |
| §8/K-159 | V-3: havale hattı; eklediği adım 'ödendi' işareti | ✓ | K-159 |
| §8/K-398 | V-3: ödeme yöntemi dağılımı satış özetinde | ✓ | K-398 (K-445 kaynağı düzeltti) |
| §8/K-401 | V-3: üç elle adımlı hat | ✓ | K-401 |
| §8/K-403 | V-2: 10/50; tepe günde 150 panel işlemi, yaklaşık bir saat; otomasyon öne çekilir | ✓ | Sayılar birebir |
| §8/K-404 | V-3: tavan havale hattı yüzünden üç | ✓ | K-404: 'tavan ikiye çekilsin' seçeneği bu gerekçeyle elendi |
| §8/K-410 | V-5: veri firmanındır; abonelik çalışma hakkını kapsamaz | ✓ | K-410 birebir |
| §8/K-417 | V-2 hedef değil; V-4 firma kendi pazarlamasını yapar | ✓ | K-417 |
| §8/K-418 | Ölçüme dayanan satırlar ilk üç ayda okunur | ✓ | K-418 |
| §8/K-421 | Yedi varsayım, liste ölçütü, satır alanları | ✓ | K-421 (yazım K-445 ile düzeltildi) |
| §8/K-423 | Yanlışlanan varsayımın adayı öne çıkar | ✓ | K-423 ölçüt (2), örnekler birebir |
| §8/K-427 | V-5: çıkış hakkı 10 §1'de | ✓ | K-427: 10 §1'e varsayım olarak yazılır |
| §8/K-445 | Doğrulama anı satıra göre; V-6 ilk yıllık yenileme | ✓ | K-445 birebir |

#### Şablon beklentileri + `01`'in iç sayı, kimlik ve terim öğeleri (mercek: `ic-tutarlilik`)

Toplam: 62 öğe (49 ✓, 11 ⚠, 2 ✗)

| ID | Öğe | Durum | Not |
|---|---|---|---|
| ŞBL-§0-1 | Başlık: Versiyon · Bağımlılıklar · Son güncelleme + Aşama/Rol/Traceability/Tamamlandı bloğu | ✓ | s.3–7 şablonla birebir; v0.8, 2026-10-02 |
| ŞBL-§0-2 | Alt bilgi satırı (proje — Project Vision vX.Y) | ✓ | s.266 v0.8, başlıkla tutarlı |
| ŞBL-§1-1 | §1 Hangi problem | ✓ | s.31 iki ayrı sistem problemi |
| ŞBL-§1-2 | §1 Kimin problemi | ✓ | s.29 sektör bağımsız KOBİ (K-03) |
| ŞBL-§1-3 | §1 Bugün nasıl çözülüyor | ✓ | s.39–45; tam envanter §5.2'ye işaretli |
| ŞBL-§1-4 | §1 Mevcut çözüm neden yetersiz | ✓ | s.41–43 her alternatifin eksiği |
| ŞBL-§1-5 | §1 Somutluk ('X yapmak için Y adım, Z riski') | ✓ | s.35 'her değişiklik iki giriştir', s.36 tutarsızlık riski |
| ŞBL-§2-1 | §2 Ürünün ne yaptığı | ✓ | s.59 |
| ŞBL-§2-2 | §2 Tek paragraf biçimi | ✓ | s.59 tek paragraf |
| ŞBL-§2-3 | §2 'Nasıl değil, ne' beklentisi | ⚠ | Birkaç mekanizma ayrıntısı var — ICT-11 |
| ŞBL-§3-1 | §3 Aktörler ve motivasyonları | ✓ | §3.2 dört aktör tablosu; §3.1 alıcı segmenti ek |
| ŞBL-§3-2 | §3 'Neden gelir, neden geri döner' cevabı | ✓ | Ek sütun B1-09 (bilinen); misafir alıcı 'geri dönmesi beklenmez' gerekçeli |
| ŞBL-§3-3 | §3 Tablo: Aktör \| Kim \| Neden kullanır | ✓ | Üç şablon sütunu var + 'Neden geri döner' (bilinen, B1-09) |
| ŞBL-§4-1 | §4 Her aktör için 'bu ürün olmasaydı ne olurdu' | ⚠ | Firma için tablo sütunu var (D-1…D-5); ziyaretçi ve üye müşteri değer + 'neden yazılır' biçiminde, karşı-olgu örtük; misafir alıcı K-100 gerekçesiyle dışarıda. Kararla temellendiği için bulgu yazılmadı |
| ŞBL-§4-2 | §4 İş değeri buradan çıkar | ✓ | Ağırlık merkezi firma (K-18, K-19) |
| ŞBL-§5-1 | §5 Mevcut alternatifler | ✓ | §5.2 beş alternatifli matris + derece sözlüğü |
| ŞBL-§5-2 | §5 Bu ürünün farkı | ✓ | §5.1 eksen, §5.3 somut karşılık tablosu |
| ŞBL-§5-3 | §5 'Rakip yok' cevabı kabul edilmez | ✓ | s.149 açıkça yazılı |
| ŞBL-§6-1 | §6 Ölçülebilir hedefler | ✓ | Ü-1…Ü-4, M-1…M-4 |
| ŞBL-§6-2 | §6 Sayısal cevap | ⚠ | Ü-3 sayısal; M ölçülerinde hedef değer yok — K-417/K-433 bilinçli, bulgu sayılmadı |
| ŞBL-§6-3 | §6 Tablo # \| Kriter \| Ölçüm \| Hedef | ✓ | Sütunlar K-433 ile bilinçli farklı (kayıtlı durum) |
| ŞBL-§7-1 | §7 Ürünün ne olmadığı | ✓ | S-1…S-10 |
| ŞBL-§7-2 | §7 Her dışlama gerekçesiyle | ✓ | 'Neden kalıcı' sütunu her satırda dolu |
| ŞBL-§7-3 | §7 Detaylı kapsam 10'a işaret | ✓ | s.222 10 §3/§4.2/§5 bağı |
| ŞBL-§8-1 | §8 Ürünün dayandığı varsayımlar | ✓ | V-1…V-7 |
| ŞBL-§8-2 | §8 Yanlışsa ne olur | ✓ | Sütun her satırda dolu |
| ŞBL-§8-3 | §8 Teknik değil ürün varsayımı | ✓ | s.245 açıkça yazılı |
| ŞBL-§8-4 | §8 Tablo # \| Varsayım \| Yanlışsa \| Nasıl doğrulanacak | ✓ | Şablonla birebir |
| 01-§0-1 | Sürüm: başlık v0.8 = alt bilgi v0.8 = tracker §1 v0.8 | ✓ | Tarih 2026-10-02 de tutarlı; tracker durumu ⏳, döngü sütunları ⬚ |
| 01-§0-2 | Tarihçe notlarının sürüm etiketleri ↔ tracker | ⚠ | Blok 4/6 notlarında v0.3/v0.4 yok — ICT-10 |
| 01-§0-3 | s.19 'yirmi iki karar' sayımı | ✓ | 24 giriş, K-81 ve K-245 ikişer kez → 22 tekil |
| 01-§0-4 | s.17 'işaretli dört karar' (K-417, K-419, K-421, K-433) | ✓ | 4 |
| 01-§0-5 | s.21 'üç karar' (K-470, K-477, K-478) + K-471 | ✓ | Tracker ile aynı; §5.3 listesi dört, Ü-1'de alan adı var |
| 01-§0-6 | s.23 kimlik önekleri D-, Ü-, M-, S-, V- ↔ bölümler | ✓ | §4.1, §6, §7, §8 ile eşleşiyor |
| 01-§1-1 | §1 'üç madde' sayımı | ✓ | 1–3 listelenmiş |
| 01-§1-2 | §1 V-1, V-2, V-7 ve §7 atıfları | ✓ | Hepsi var |
| 01-§1-3 | §1 Ölçülebilir karşılığı ↔ §6 Ü-1/Ü-2/Ü-3 | ⚠ | 'Bir kez girilir' Ü-2'de ölçülmüyor (ICT-4); Ü-1 'dış ön koşullar' (ICT-2) |
| 01-§3-1 | §3.2 'dört aktör' ↔ tablo satırları ↔ 02 §1.3 | ✓ | 4 = 4 = 4; adlar sözlükle birebir |
| 01-§4-1 | §4 'dört aktörden üçü' ↔ §4.1–§4.3 | ✓ | Firma, ziyaretçi, üye müşteri |
| 01-§4-2 | D-1…D-5 kesintisizliği | ✓ | Boşluk yok |
| 01-§4-3 | §4 ölçüye bağlanma notu ↔ §6 (M-1, ölçülemeyenler) | ✓ | Tutarlı |
| 01-§5-1 | §5.2 matris dereceleri ↔ derece sözlüğü (K-442) | ✓ | Beş satırın her derecesi sözlük tanımına uyuyor |
| 01-§5-2 | §5.3 kurulumdan gelenler 'dört' (K-478) | ✓ | Alan adı, e-posta kimliği, ödeme anahtarları, Google kimlik bilgileri; 10 §4.1 ile aynı |
| 01-§5-3 | §5.1 ↔ §5.3 | ⚠ | 'Sahiplik ve maliyet' maddesinde maliyet öğesi yok — ICT-9 |
| 01-§6-1 | 'Dört kriterin dördü' ↔ Ü-1…Ü-4 | ✓ | 4 |
| 01-§6-2 | 'Dört ölçünün dördü sipariş verisinden' ↔ M-1…M-4 | ✗ | M-2 iletişim talebi kaydından — ICT-1 |
| 01-§6-3 | Ü-1 ön koşul listesi ↔ 10 §4.1 ÖK-1…ÖK-12 | ✗ | 7 ↔ 12; dört satış kapısı koşulu eksik — ICT-2 |
| 01-§6-4 | Ü-3 envanteri ↔ 02 §10.5.1 | ⚠ | Sayılar aynı; havale ek adım kuralı yok — ICT-5 |
| 01-§6-5 | Ölçülemeyenler 4 satırı ↔ K-433 | ✓ | Ziyaretçi, dönüşüm, sepet terk, sipariş hacmi |
| 01-§6-6 | 'On dört gün' ×2 (K-203, K-209) ve 'ilk üç ay' | ✓ | Kararlarla tutarlı |
| 01-§6-7 | §6 Kaynak satırı ↔ gövde atıfları | ⚠ | K-20 eksik — ICT-8; diğer bölümlerde fark yok |
| 01-§7-1 | S-1…S-10 kesintisiz; K-419 'on kalıcı sınır' | ✓ | 10 = 10; 10 §4.2 SK-8/9/10 → S-10/S-6/S-5 doğru |
| 01-§7-2 | §7 'üç madde' (K-440) | ⚠ | K-440 dört sayıyor, K-10 yan cümlede — ICT-6 |
| 01-§8-1 | 'Yedi ürün varsayımı' ↔ V-1…V-7 | ✓ | 7; K-421 ile birebir |
| 01-§8-2 | V-2 '150 panel işlemi' = 50 × 3 | ✓ | 02 §10.5.5 ile aynı |
| 01-§8-3 | Doğrulama anı maddesi ↔ tablo son sütunu | ✓ | V-5 sözleşme, V-6 yenileme, diğerleri ilk kurulum |
| 01-TRM-1 | Aktör adları (ziyaretçi, misafir alıcı, üye müşteri, firma yöneticisi) ↔ 02 §1.2 | ✓ | Adlar birebir; 'Kim' tanımlarında tüketici/alıcı sapması — ICT-7 |
| 01-TRM-2 | Başarısız / ödeme ekseni ↔ 02 sözlük | ⚠ | 'Ödeme ekseni' doğru; Başarısız tanımı gevşek — ICT-3 |
| 01-TRM-3 | Ürün tipi (fiziksel/dijital/hizmet) ↔ 'Tip' | ⚠ | Değer adları doğru; kavram adı 'tür' — ICT-7 |
| 01-TRM-4 | İletişim talebi, işlem izi, satış özeti, platform imzası, yasal metin (dört), hat, havale/EFT | ✓ | 02 ile uyumlu; işlem izinin 'belirli alanlar' ifadesi K-310'un beş alanını özetliyor |
| 01-GRD-1 | GUARDRAILS §5 belirsiz ifadeler | ✓ | Yasaklı ifade yok; s.159 'olabildiği' yetenek bildiriyor, belirsizlik değil |
| 01-GRD-2 | Ölü placeholder | ✓ | Yok |

#### Çapraz referanslar — `01` ↔ `10`, `02` (mercek: `capraz-ref`)

Toplam: 47 öğe (36 ✓, 9 ⚠, 2 ✗)

| ID | Öğe | Durum | Not |
|---|---|---|---|
| XR-1 | 01 s.7 → 00 §C.4, §C.5: Doküman Tamamlama Protokolü ve kalite döngüsü | ✓ | 00'da iki bölüm de var ve içerik uyuşuyor. |
| XR-2 | 01 s.23 → tracker §2 karar kaydı, §6.3 blok içerikleri | ✓ | Tracker'da §2 (s.38) ve §6.3 (s.680) var. |
| XR-3 | 01 §3.2 s.97 → Aşama 4 + DEPLOY_RUNBOOK.md: kurulum bir deploy işidir | ✓ | Dosya var; ifade K-06'nın gerekçesiyle birebir. Aşama 4 = 05 (00 §B). |
| XR-4 | 01 §6 s.187 → 10 §1: çıta K-22'nin cümlesiyle yazılır ve ölçüler için 01 §6'ya işaret eder | ✓ | 10 §1 çıtayı K-22'nin cümlesiyle veriyor ve 'Çıtanın ölçülebilir karşılığı `01 §6`'dadır ve burada tekrarlanmaz (K-434)' diyor. |
| XR-5 | 01 §6 s.191 → 12: dört ürün düzeyi kriteri 12'de sınanır | ✓ | 12 henüz şablon; ileri referans olarak geçerli. |
| XR-6 | 01 §6 s.192 → 10 §5: mağaza ölçüleri post-MVP sıralamasını besler | ✓ | 10 §5'in sıralama ölçütlerinden üçüncüsü bu (K-416, K-418). |
| XR-7 | 01 Ü-1 → 10 §4.1: kurulabilirliğin ön koşulları | ⚠ | Ü-1 yedi kalem sayıyor, 10 §4.1'de on bir satır var ve 10 Ü-1'in tamamını sınadığını söylüyor (bulgu XR-2). |
| XR-8 | 01 Ü-4 → 10 §4.1 ÖK-12 / 10 §1: referans kurulum, bitti çizgisi | ✓ | İçerik K-23 ile birebir; 10 §1'deki tekrar K-23'ün etki sütunundan (10 §1) geliyor. |
| XR-9 | 01 Ü-3 → 02 §10.5 (K-401 envanteri, K-454 okunuşu) | ⚠ | Havalede hizmet ve dijital hattına eklenen adım yok sayılmış (bulgu XR-5). |
| XR-10 | 01 §7 s.222 → 10 §3: kapsam dışı kalemler orada; 'Hayır' kullanılmaz, kalıcı sınıra düşen kalem işaretle anılır | ✓ | 10 §3 s.148 S-1, S-2, S-3, S-6 ve S-10'u işaretle anıyor; tabloda 'Hayır' yok. |
| XR-11 | 01 §7 s.222 → 10 §5: yol haritası yalnız 10 §3'ten beslenir | ✓ | 10 §5: 'Liste yalnız §3'ün "Evet" satırlarından beslenir'. |
| XR-12 | 01 §7 s.222 → 10 §4.2: 'Kalkmaz' satırları §7'deki satırı gösterir | ✓ | SK-8 → S-10, SK-9 → S-6, SK-10 → S-5. SK-10'un statüsü için bulgu XR-1'e bakın. |
| XR-13 | 01 §7 s.224 → 10: K-09 tek firma ve K-84 ek akışlı ürünler 10'da yaşar | ✓ | Biri 10 SK-1'de, diğeri KD-28'de. K-12'nin S-1'e alınması K-440 ile uyumlu. |
| XR-14 | 01 S-1 / §3.2 → 10 §5: çok kiracılı SaaS yol haritası adayıdır | ✓ | YH-21 (KD-21). |
| XR-15 | 01 S-4 → 10 §5: e-Arşiv/e-Fatura adaydır | ✓ | YH-1 (KD-1). |
| XR-16 | 01 S-7 → 10 §5: blog adaydır | ✓ | YH-14 (KD-14). |
| XR-17 | 01 S-8 → 10 §5: İYS'li pazarlama iletisi adaydır | ✓ | YH-17 (KD-17). |
| XR-18 | 01 S-9 → 10 §5: ziyaretçi analitiği adaydır | ✓ | YH-19 (KD-19). |
| XR-19 | 01 S-10 → 10 §5: çoklu dil adaydır | ✓ | YH-20 (KD-20). |
| XR-20 | 01 §8 s.250 → 10 §5: havale kullanılmıyorsa kapıda ödeme öne çıkar | ✓ | YH-4 (KD-4); notu `01 §8` V-3'ü gösteriyor. |
| XR-21 | 01 §8 s.250 ve V-2 → 10 §5 / 10 §4.2 SK-3: hacim aşılırsa 'otomasyon' | ⚠ | 10 §5'te 'otomasyon' adlı bir aday yok; K-473 ve K-474 işlenmemiş (bulgu XR-3). |
| XR-22 | 01 V-5 → 10 §1: çıkış hakkının nasıl karşılandığı | ✓ | 10 §1'in dördüncü kabulü bunu taşıyor (K-427'nin etki sütunu 10 §1). |
| XR-23 | 01 §8 s.248 → 02: kalan riskler kuralın 02'deki karşılığında yaşar | ✓ | 02 'Kalan risk bilinçlidir' kalıbını kullanıyor (ör. §3.1.2). |
| XR-24 | 01 §1 / S-5 / V-1 ↔ 10 SK-10 ↔ 02 ölçek notu: tek depo | ✗ | Aynı kalem bir yerde kalıcı sınır, bir yerde varsayım, bir yerde kabul olarak geçiyor (bulgu XR-1). |
| XR-25 | 10 §1 → 01 §6 Ü-1…Ü-4 ve dört mağaza ölçüsü | ✓ | Kimlikler ve kabul kapısı ayrımı 01 §6 ile uyumlu. |
| XR-26 | 10 §1 → 01 §8 V-2 (hacim) ve V-5 (çıkış hakkı) | ✓ | İki satır da 01'de var. |
| XR-27 | 10 §1 → 01 §1, §5, §7, §8 (yedi ürün varsayımı) | ✓ | 01'de V-1…V-7 var. |
| XR-28 | 10 §3 s.148 → 01 §7 S-1, S-2, S-3, S-6, S-10 ve S-4, S-7, S-8, S-9'un alanı | ✓ | Satır kimlikleri ve içerikleri eşleşiyor. |
| XR-29 | 10 §4.1 s.233 → 01 §6 Ü-1: Ü-1 bu listenin tamamlanmasını sınar | ⚠ | Ters yönden aynı uyumsuzluk (bulgu XR-2). |
| XR-30 | 10 SK-3, KP-51 → 01 §6 Ü-3 | ⚠ | Atıf doğru, ama KP-51 Ü-3'ün eksik sayımını tekrarlıyor (bulgu XR-5). |
| XR-31 | 10 SK-5, SK-6 → 01 §8 V-1 (katalog ölçümü) | ✓ | V-1'in doğrulama ölçüsü 'ilk gerçek kurulumun katalog boyutu'. |
| XR-32 | 10 YH-4 → 01 §8 V-3 | ✓ | V-3 havale hattının kullanımını varsayıyor. |
| XR-33 | 10 §5 → 01 §7 (kalıcı sınırlar yol haritası kalemi değildir) ve 01 §8 (ölçüt 2) | ✓ | 01 §7 s.222 ve §8 s.250 ile uyumlu. |
| XR-34 | 02 §1.3 → 01 §3.2: dört aktör; neden kullanır ve neden geri döner 01'dedir | ✓ | Aktör adları ve tanımları (02 §1.2) 01 §3.2 ile uyumlu. |
| XR-35 | 02 §1.3 s.261 → 01 §3.2: platform operatörü / 'K-01 ile elenen kiracılık' | ✗ | SaaS'ın aday statüsüyle çelişiyor (bulgu XR-4). |
| XR-36 | 02 §3 s.247 → 01 §3.1, 01 §8 V-1, 10 §4: ölçek bir kabuldür | ⚠ | Katalog için doğru; tek depo için 10 SK-10 ve 01 S-5 ile çelişiyor (bulgu XR-1). |
| XR-37 | 02 §3 s.325 → 01 §7, 01 §6, 10 §3, §5 | ✓ | Hedef bölümlerin hepsi var. |
| XR-38 | 02 §10.1.1 → 01 §7, 10 §5: tek firma; SaaS post-MVP adayı | ✓ | 01 ile uyumlu. |
| XR-39 | 02 §10.6.2-§10.6.3 → 01 §6 M-1…M-4: tanımlar | ✓ | M-1 (sepet birimi, K-457), M-2 (KVKK ve sipariş tipleri hariç), M-3 (söze uyum oranı) ve M-4 birebir. Satış özetinin içeriği K-441 ile uyumlu. |
| XR-40 | 01 §6 s.192 / S-9 ↔ 02 §10.6.3 M-2: 'dördü de sipariş verisinden türer' | ⚠ | M-2'nin kaynağı iletişim talebi kaydı (bulgu XR-6). |
| XR-41 | 01 §2 ↔ 02 §3.2.2: üç ürün tipi | ⚠ | İçerik doğru; terim 02'nin 'Tip' adından farklı (bulgu XR-7). |
| XR-42 | 01 §2 ↔ 02 §3.21, 10 KP-15: kart ya da havale/EFT | ✓ | Uyumlu; kapıda ödeme 10 KD-4'te. |
| XR-43 | 01 §2 ↔ 02 §1.2-§1.3, 10 KP-18: misafir alıcı sipariş numarası ve e-postayla izler | ✓ | Uyumlu. |
| XR-44 | 01 §3.2 ↔ 02 §1.3 (K-122): yönetici aynı kimlik doğrulama rejiminde | ✓ | Uyumlu. |
| XR-45 | 01 §5.3 ↔ 02 §3.1.2, 10 §4.1 (K-478): kurulumdan dört kalem | ✓ | Alan adı, e-posta gönderim kimliği, ödeme anahtarları ve Google kimlik bilgileri üç dokümanda da aynı. |
| XR-46 | 01 §1 / §6 'sipariş hacmi kriter değil' ↔ 10 §1, 02 §10.5.5 | ✓ | K-417 ve K-403 ile uyumlu. |
| XR-47 | 02 §11.3 H-2 → 10 §4: 'otomasyon post-MVP'den öne çekilir' | ⚠ | K-473'ün adlandırmasından önceki dil kalmış (bulgu XR-3'ün 02 tarafı). |

#### Yasal iddialar (mercek: `yasal`)

Toplam: 22 öğe (15 ✓, 6 ⚠, 1 ✗)

| ID | Öğe | Durum | Not |
|---|---|---|---|
| YS-1 | §2 s.59 — üç ürün türü "aynı şekilde satın alınır" (dijitalde cayma istisnası onayı) | ⚠ | K-81 ile birebir; K-204'ün yasal onay kutusu anılmıyor. Bulgu YS-6. |
| YS-2 | §3.1 s.76 ve §5.3 s.177 — firma tipi ve zorunlu kimlik alanları (6563 genel bilgi yükümlülüğü) | ⚠ | K-14 ile birebir; gerçek kişi tacirin MERSİS ve sicil numarası sette yok. Bulgu YS-3. |
| YS-3 | §3.1 s.79 — pazar Türkiye, tek dil Türkçe, tek para birimi TRY | ✓ | K-16 ile birebir; hukuki katmanın Türkiye'ye bağlı olması doğru. |
| YS-4 | §3.1 s.82 — "cayma ve iade her sektörde tüketicinin yasal hakkı" | ✗ | Yönetmelik'in istisnalarıyla ve K-204, K-205, K-206 ile aşırı genel. Bulgu YS-1. |
| YS-5 | §3.2 s.98 — "6502'nin tüketici korumaları istisnasız her siparişte uygulanır" | ⚠ | K-07 ile birebir; rejimin dallanmaması ile kanunun kendi istisnaları karışıyor. Bulgu YS-2. |
| YS-6 | §4.1 D-1 s.112 — bağlayıcı olan ön bilgilendirme ve sözleşmedir | ⚠ | K-364 ile uyumlu; K-189'un sürüm dondurması anılmıyor. Bulgu YS-5. |
| YS-7 | §4.1 D-5 s.116 — veri sorumlusu firmadır (KVKK 6698) | ✓ | K-341 ile birebir; hukuken doğru, çünkü işleme amacını ve vasıtasını firma belirler. |
| YS-8 | §4.1 D-5 s.116 — VERBİS kaydı firmanın yükümlülüğü | ⚠ | K-341'deki "gerekiyorsa" koşulu düşmüş; KVKK muafiyetleri var. Bulgu YS-4. |
| YS-9 | §6 s.192 — cayma penceresi on dört gün (K-203) | ✓ | Yönetmelik md. 9 ile uyumlu. Malda başlangıç teslim (K-203), hizmette sözleşme kurulması (K-289), teslimden önce de cayılabilir (K-340). K-177 parçalı gönderimi elediği için kalem bazlı pencere md. 9(3)(a) ile çelişmiyor. |
| YS-10 | §6 s.192 — geri ödeme çatısı on dört gün (K-209) | ✓ | Yönetmelik md. 13(1)'e göre süre, cayma bildiriminin satıcıya ulaşmasından itibaren on dört gündür; 01 süreyi ve adını doğru veriyor. K-209'daki tevkif hakkı için notes'a bakınız (doğrulanamadı). |
| YS-11 | §6 Ü-1 s.199 — firmanın ödeme sağlayıcı sözleşmesi (K-12) | ✓ | K-12 ile birebir. |
| YS-12 | §6 Ü-1 s.199 ve Ü-4 s.202 — ETBİS kaydı (6563, K-14, K-23) | ✓ | ETBİS kaydı satıcının yükümlülüğü, ürün kayıt bilgisini taşıyor; K-14, K-23 ve 10 ÖK-5 ile tutarlı. |
| YS-13 | §6 Ü-1 s.199 — barındırma konumu (K-358, KVKK yurt dışına aktarım) | ⚠ | E-posta altyapısının konumu ve aktarım şartları adlandırılmamış. Bulgu YS-7. |
| YS-14 | §6 Ü-1 s.199 — dört yasal metin (K-365) | ✓ | Aydınlatma, çerez, ön bilgilendirme ve mesafeli satış sözleşmesi; K-365 ile uyumlu. |
| YS-15 | §6 Ü-4 s.202 — gerçek kartla sipariş, 3D Secure geçilir | ✓ | K-23 ile birebir; 01 3D Secure'u yasal zorunluluk olarak değil kabul adımı olarak anıyor, 10 KP-15 ve ÖK-4 ile tutarlı. |
| YS-16 | §7 S-1 s.228 — tahsilatı platformda toplamak ödeme kuruluşu faaliyetidir ve lisans ister (6493) | ✓ | K-12 ile birebir; 6493 sayılı Kanun'a göre başkası adına fon toplamak lisanslı ödeme hizmetidir (TCMB). |
| YS-17 | §7 S-4 s.231 — fatura ürünün dışında kesilir; e-Arşiv/e-Fatura yol haritası adayı | ✓ | K-217 ile birebir; faturanın mali sorumluluğunun satıcıda olması doğru. |
| YS-18 | §7 S-6 s.233 — ürünün tek hukuki rejimi tüketici rejimidir | ✓ | K-07 ve K-112 ile tutarlı. |
| YS-19 | §7 S-8 s.235 — pazarlama iletisi için İYS kaydı, onayların yüklenmesi, ret senkronu | ✓ | K-323 ve K-324 ile birebir; 6563 ve Ticari İletişim Yönetmeliği'ne uygun. İşlemsel bildirimler ticari elektronik ileti sayılmaz. |
| YS-20 | §7 S-9 s.236 — ziyaretçi ölçümü çerez rejimini değiştirir | ✓ | K-263 ve K-397 ile uyumlu; KVKK'nın çerez yaklaşımında analitik çerez açık rıza ister. |
| YS-21 | §7 S-10 s.237 — hukuki katman Türkiye'ye bağlı; çoklu pazar yurt dışına aktarım getirir | ✓ | K-16 ile birebir. Aktarımın kurulumdan doğabilmesi (K-358) bu cümleyle çelişmiyor. |
| YS-22 | §5.3 s.168 — yasal uyuma dokunan sayılar ürün sabitidir | ✓ | K-412 ile birebir; cayma süresi sabit (K-203). |
