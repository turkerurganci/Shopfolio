# Shopfolio — Product Requirements

**Versiyon: v0.25** | **Bağımlılıklar:** `01_PROJECT_VISION.md`, `PRODUCT_DISCOVERY_STATUS.md` | **Son güncelleme:** 2026-10-03

> **Aşama:** 1 — Product Discovery · **Rol:** Product Manager
> **Traceability zorunlu:** Hayır (kaynak doküman — sonraki aşamalar buraya izlenir)
> **Bu doküman tüm iş kurallarının tek kaynağıdır.** Kod bu dokümanla çelişemez.

> **Taslak durumu (K-28, K-432):** §1–§13 **taslak** olarak yazılmıştır; besleyen blokların tamamı kapalıdır — Aşama 1'in workshop turu 2026-09-30'da bitti. §4 ve §8–§13 2026-10-02'de, Blok 9'dan sonraki **yazım turunda** ilk kez yazıldı; §1–§3 ve §5–§7 aynı turda Blok 8 ve 9'un kararlarıyla güncellendi (aşağıdaki v0.8 notu). Kalite döngüsü (audit → deep review → cross-review → etki yansıtma → checkpoint) yazım turu tamamlandıktan sonra, `01` → `02` → `10` sırasıyla ve doküman başına **bir kez** koşar; ardından çakışma taraması, checkpoint, öğrenim terfisi ve arşiv işaretiyle Aşama 1 kapanır (K-28, K-430, K-437).
> Taslak yazıldıktan sonra alınan bir karar bu bölümlere dokunursa, karar satırı `(taslak güncellenecek)` işaretini taşır (K-29). Blok 8 ve 9'un işaretli kararları kendi blok PR'larında değil, yazım turunda işlendi; işaret orada `(taslak güncellendi — v0.8)` oldu (K-432).
>
> **Önceki taslak notu (v0.7, 2026-09-19):** §1, §2, §3, §5, §6 ve §7 **taslak** olarak yazılmıştır. §1 2026-09-15'te yazıldı; o gün §6.2'nin "Hedef doküman" sütunu bu bölümü Blok 1 ve Blok 3'e bağlıyordu ve ikisi de kapalıydı. §2, §3, §5, §6 ve §7 2026-09-19'da, besleyen blokları hem §6.2 sütununa hem kararların etki sütunlarına göre kapalıyken yazıldı (§2: 1, 6, 7 · §3: 1–7 · §5: 1, 3–7 · §6: 1, 3, 5, 6 · §7: 1, 6). Aynı gün sütun düzeltildi ve Blok 8'i bu beş bölüme, Blok 9'u §3'e de bağladı; taslaklar geçerliliğini korur, o blokların bu bölümlere dokunan kararları K-29 işaretiyle işlenir (2026-09-15'teki §1 kalıbı). §4 ve §8–§13 henüz yazılmamıştır — her bölümün yazım kapısı kendi başlığının altındadır; kapılardaki besleyen blok listeleri 2026-09-19'da §6.2 sütunu ile etki sütunlarının birleşimine hizalandı. Kalite döngüsü (audit → deep review → cross-review → etki yansıtma → checkpoint) bu dokümanda **tüm bloklar kapandıktan sonra** ve **bir kez** koşar; sıra `01` → `02` → `10`'dur (00 §C.5).
> Taslak yazıldıktan sonra alınan bir karar taslak bir bölüme dokunursa, karar satırı `(taslak güncellenecek)` işaretini taşır ve güncelleme aynı bloğun `docs:` PR'ında yapılır (K-29).

> **§1 bloklar arası bir bölümdü ve Blok 8 kapanmadan tamamlanamazdı** — Blok 8'in terimleri yazım turunda eklendi (v0.8 notu). Sözlük ve aktör envanteri, diğer bölümlerin aksine **tek bir blokta bitmez** — terim üreten her blok ona satır ekler. §6.2'nin sütunu bunu görmüyordu; **2026-09-15'te düzeltildi** ve `02 §1` Blok 4–8'e de bağlandı (gerekçe: `PRODUCT_DISCOVERY_STATUS.md` §6.2, tablo altındaki not). 2026-09-15'te bilinen eklemeler: K-17 sipariş durum adlarının sözlüğe ilk satırlarını `B6-03`'e bırakmıştı; **Sepet** ve **Sipariş kalemi** terimleri `B5-01`/`B5-03` ve `B6-04` ile tanımlanacaktı — üçü de Blok 5 (v0.4) ve Blok 6'da (v0.5) kapandı.
> **Kapalı Blok 3'ten gelen eksik kapatıldı (2026-09-15):** İndirim, Kupon ve Referans fiyat tam karara bağlı olduğu hâlde sözlükte satır taşımıyordu — kararları `02 §3`/`§5`'e yönlendirilmiş, `§1`'e yönlendirilmemişti. Üçü de eklendi; K-63 ve K-70'in etki sütunları düzeltildi. **Sepet** ve **Sipariş kalemi** hâlâ eksiktir ve bilinçli olarak bekletilmektedir — tanımları `B5-01`/`B5-03` ve `B6-04`'e bağlı, bugün yazılamaz.
> **Blok 4 eklemeleri (2026-09-16):** K-06'nın `B4-01`'e bıraktığı soru kapandı — **misafir alıcı dördüncü aktördür** (K-97) ve §1.3 üçten dörde çıktı, listedeki "aktör değildir" maddesi kalktı. Sözlüğe altı satır girdi: **Misafir alıcı · Oturum · Sosyal giriş · Adres defteri · Teslimat adresi · Fatura adresi** (K-97, K-98, K-103, K-104, K-106, K-108, K-111, K-112, K-113). Altısı da türetilmiştir (**†**) ve **A-05**'e eklenmiştir; sözlük 25 → **31 terim**. §1.3'e ayrıca üçüncü bir aktör kuralı ve yönetici tarafının kimlik doğrulama tabanı yazıldı (K-122).
> **Blok 5 eklemeleri (2026-09-17, v0.4):** sözlüğe üç satır girdi — **Sepet · Sepet kalemi · Stok ayırma** (K-123, K-128, K-131). Üçünün de İngilizce karşılığı karar kaydında yazılı olduğu için † almadılar; sözlük 31 → **34 terim**. Yukarıdaki iki notun beklettiği **Sepet** terimi böylece kapandı.
> **Blok 6 eklemeleri (2026-09-18, v0.5):** sözlüğe beş grup altında **26 satır** girdi: **Sipariş** (Sipariş · Sipariş kalemi · Sipariş numarası), **sevkiyat ekseni** (Sipariş durumu ve altı durum), **Ödeme** (Ödeme yöntemi · Havale/EFT · Ödeme süresi · Ödeme durumu ve beş durum), **İptal, cayma ve iade** (İptal · Cayma · İade · Geri ödeme · Ayıp talebi), **Yasal metinler** (Ön Bilgilendirme Formu · Mesafeli Satış Sözleşmesi) — K-159…K-236. Sözlük 34 → **60 terim**. Durum adlarının on üçünün İngilizce karşılığı karar kaydında yazılıdır ve † almadı; diğer on üç satır türetilmiştir (**†**) ve **A-05**'e eklenmiştir (20 → **33**). K-17'nin sipariş durum adlarını `B6-03`'e bırakan devri ve **Sipariş kalemi**'nin `B6-04`'e bağlı beklemesi kapandı. **Kapanış taramasının iki bulgusu:** (1) en temel terim olan **Sipariş** sözlükte hiç yoktu — Blok 3'ten beri kayıtta kullanılıyordu ve hiçbir blok onu adıyla üstlenmemişti; (2) **"iade" iki kavramı taşıyordu** — malın firmaya dönmesi ve paranın müşteriye dönmesi. K-17'nin tek ad kuralı gereği ayrıldı: **İade** (`Return`) yalnız malın dönüşüdür, **Geri ödeme** (`Refund`) paranın dönüşüdür; ödeme eksenindeki iki durumun adı buna hizalandı (**Geri ödendi**, **Kısmen geri ödendi** — K-172, K-222).
> **Blok 7 eklemeleri (2026-09-19, v0.6):** sözlüğe iki yeni grup altında **15 satır** girdi: **Kurumsal içerik** (Kurumsal içerik · Hakkımızda · Hizmet tanıtımı · Referans iş · Sık sorulan soru · Şube · Genel sayfa · Duyuru · Sosyal medya bağlantısı) ve **Marka kimliği** (Marka kimliği · Marka adı · Unvan · Logo · Site simgesi · Marka rengi) — K-237…K-286. On beşinin de İngilizce karşılığı karar kaydında yazılıdır ve † almadı; sözlük 60 → **75 terim**, A-05'in türetilmiş sayısı **33'te kaldı**. **Yedi tanım düzeltildi:** *Ana kategori* — ürünün adresi artık kategoriden bağımsızdır, ana kategori yalnız kırıntı yolunu üretir (K-279); *Yayın durumu*, *Taslak*, *Yayında*, *Arşiv* — kurumsal içerik yayın durumunun yalnız iki değerini kullanır, arşiv yoktur (K-273, K-274, K-275); *Hizmet* — fiyatsız **Hizmet tanıtımı**ndan ayrılır (K-240); *Firma* — yasal adı **unvan**, vitrinde görünen adı **marka adı**dır (K-260). **Ad çakışmaları kayda geçti:** "Referans" tek başına hem *Referans fiyat* ile hem dokümanların atıf anlamıyla çakıştığı için terim **Referans iş** oldu (K-241); "Kampanya" K-63'ün indirimini anlattığı için duyurunun adı **Duyuru** oldu (K-272).
> **§2, §3, §5, §6, §7 taslağı (2026-09-19, v0.7):** K-28'in üçüncü uygulaması; K-30 gereği workshop'tan ayrı bir oturumda yazıldı. Girdi: karar kaydı (`PRODUCT_DISCOVERY_STATUS.md` §2 ve §4), bu dokümanın şablonu ve üst doküman `01` — workshop sohbet geçmişi kullanılmadı. Etki sütununda bu beş bölümden birini taşıyan her karar satırı kendi bölümünde karşılık bulur. **Yazım, kaydın yeterlilik testiydi (K-30) ve beş boşluk buldu;** bölümlerde "açık" diye adıyla işaretlidir ve `PRODUCT_DISCOVERY_STATUS.md` §4'te **A-06…A-10** olarak izlenir. **§1'e üç satır girdi:** §5.10'un kullandığı Ayıp talebi durumu · Açık · Çözüldü — kodları kayıtta yazılı olmadığı için † ile türetildi; sözlük 75 → **78 terim**, A-05 33 → **36**. **Blok 8 ve Blok 9'un bu bölümlere dokunuşu:** konu başlıkları adıyla dokunuyor — `B8-01` talep durumları (§5), `B8-13` şikâyet kanalı (§7), `B8-14` elle durum geçişi ve elle sipariş (§2, §5, §7), `B9-14` toplu veri işlemleri (§3). §6.2 sütunu bunu göstermiyordu; aynı gün düzeltildi (`PRODUCT_DISCOVERY_STATUS.md` §6.2, tablo altındaki not). Bu konuların kararları K-29'un `(taslak güncellenecek)` işaretini taşır. Düzeltmenin tek kapı etkisi: §10'un kapısı Blok 9'a geçti.
> **Yazım turu (2026-10-02, v0.8 — K-432):** K-30 gereği workshop'tan ayrı bir oturumda yazıldı; girdi karar kaydı (`PRODUCT_DISCOVERY_STATUS.md` §2 ve §4), bu dokümanın şablonu ve üst doküman `01` v0.6 — workshop sohbet geçmişi kullanılmadı.
> - **İlk kez yazılanlar:** §4 sayım kuralları ve otuz yedi süre (Z-1…Z-37) · §8 yedi deneme limiti ve ürün düzeyi güvenlik kuralları · §9 bildirim matrisi — müşteriye on üç, firmaya dört bildirim · §10 yönetim kuralları — panelin kapsamı, yönetici hesapları, işlem izi, altı müdahale, manuel adım bütçesi, satış özeti, dışa aktarma, ilk kurulum kontrol listesi · §11 kırk iki parametre ve dört hacim kabulü (K-411) · §12 ticari çerçeve, KVKK, veri sahipliği ve firmanın yükümlülükleri · §13 açık kararların kuralı (K-428, K-429).
> - **K-29 güncellemeleri:** işaretli 150 satır — Blok 8'in 97, Blok 9'un 52 ve yazım turunun `01` oturumunun 1 satırı — §1–§3 ve §5–§7'ye işlendi. §3'e üç grup girdi: §3.32 iletişim talebi, §3.33 yasal metinler, §3.34 site geneli; satış kapısı dört koşula çıktı (§3.1.5). A-06…A-10'un "açık" işaretleri kapanış kararlarıyla (K-288…K-299) değişti.
> - **Sözlük 78 → 86 terim:** Blok 8'in altı terimi — İletişim talebi · İletişim talebi durumu · Kapatıldı · Yönetici daveti · İşlem izi · Bildirim — ve iki yasal metin — Aydınlatma metni · Çerez politikası (K-458). **A-05 kapandı** (K-455): türetilmiş otuz beş karşılık onaylandı, Açık → `Open` K-303 ile kayda geçmişti; † işareti kalktı. **A-12 kapandı:** K-08'in kalan riski §3.1.2'de (K-445), K-147'nin elle adımı §10.5.2'de (K-456), mağaza düzeyi ölçülerin tanımı §10.6.3'te (K-457).
> - **İşaret ve anlamsal K-29 kaçakları:** dokuz Blok 8–9 satırı taslağı yazılmış bir bölümü — §6 ya da §7 — işaretsiz taşıyordu (K-328, K-334, K-356, K-365…K-368, K-389, K-395); elli satır etki sütununda göstermediği taslak bölümlerine dokunuyordu. En ağırları: K-340 caymanın beyanını sipariş onayına açtı ve sözlükteki *"teslimattan sonra"* tanımını yanlışladı; K-347 siparişe üçüncü bir donan metin sürümü ekledi; K-296 Başarısız'ın tanımını genişletti; K-365 satış kapısına dördüncü bir koşul ekledi; K-310 içeriğin metin düzenlemelerini işlem izinin dışında bıraktı. Satırlar `02 §X (taslak güncellendi — v0.8)` işaretini taşır.
> - **Yazımın bulduğu boşluklar:** yirmi üç madde karara bağlandı (K-447…K-469) — sekizi ⚠ ile proje sahibine gösterildi, on beşi öneriyle kaydedildi; ikisi `01 §6`'ya dokundu ve `01` v0.7 oldu (K-454, K-457). Ayrıntı: `PRODUCT_DISCOVERY_STATUS.md` §6.1.
> **Yazım turunun `10` oturumu (2026-10-02, v0.9 — K-29):** `10`'un yazımında alınan iki karar bu dokümana dokundu ve aynı PR'da işlendi. K-478 Google uygulamasının kimlik bilgilerini kurulum ayarı saydı — kurulumdan gelenler listesi dörde çıktı (§3.1.2, §10.1.3, §11) ve uygulama tanımlı değilse "Google ile giriş" düğmesi görünmez (§3.13.6). K-477 alan adını kurulumun dış ön koşullarına ekledi (§10.8.2). `10 §4` iki parçaya ayrıldı (K-476); bu dokümandaki "`10 §4`" işaretleri bölümün tamamını, kurulum kontrol listesine yapılanlar `10 §4.1`'i gösterir. A-11'in kapanışıyla (K-470…K-472) §13.4'ün son cümlesi güncellendi: tracker §4'te açık satır kalmadı.
> **`01`'in kalite döngüsünün etki yansıtması (2026-10-02, v0.10 — K-29):** `01`'in audit ve deep review turunda alınan kararlar bu dokümana dokundu ve aynı PR'da işlendi. Firma tipi üçe çıktı — esnaf, gerçek kişi tacir, tüzel kişi (K-480; §1.2 Unvan, §3.1.3) · tek depo kabul değil kalıcı sınırdır (K-479; §1 ölçek notu, §11.3 H-1) · satış özetinin ve mağaza ölçülerinin kaynağı "ürünün kendi kayıtları"dır (K-483; §3.34.6, §10.6.2) · kargoya verme sözüne uyum oranının paydası (K-484), iptal ve iade oranının payı ve paydası (K-485) ve ölçüm penceresinin başlangıcı ve iptal ve iade oranının okunma anı (K-486, K-488) §10.6.3'te · H-2 hacim tetikleyicisinin ölçülebilir hâlini ve öne çekilecek iki adayı adıyla taşır (K-473, K-474) · §10.5.4 bütçe hat başınadır (K-454) · §1.3 çok kiracılı SaaS'ı yol haritası adayı olarak anar (K-10) · §12.2.1 bakım ve destekte geliştirici veri işleyendir (K-482) · §12.3.1 aboneliği biten kurulumun uyumu firmadadır (K-481) · §3.13.2'ye misafir siparişlerinin e-postayla bağlanmasının kalan riski yazıldı (K-489) · §10.6.3'te iptal ve iade oranının okunma anı (K-488) ve sıfır payda kuralı (K-490) · §1.3, §7.1.2 ve §12.1.2 ürünü tüketiciye satış için kurulmuş tek akış olarak anlatır, alıcının hukuki sıfatını mevzuata bırakır (`01` cross-review'ının sonucu — K-07, K-112) · §3.25.1 ve §12.1.10 tüketici faturasıyla sınırlandı (K-112). Tracker §4'te bir açık satır doğdu — A-13, `10`'a aittir; §13.4'ün "açık satır kalmadı" cümlesi bu yüzden güncellendi. `02`'nin kendi kalite döngüsü bu düzeltmelerle başlar (K-430).
> **A-14'ün kapanışı (2026-10-03, v0.11 — K-29):** K-209'un geri ödeme kuralı Mesafeli Sözleşmeler Yönetmeliği'nin güncel metnine karşı doğrulandı ve proje sahibinin kararıyla değişti (K-491): geri ödemenin on dört günü teslimden önceki caymada ve hizmette cayma beyanından, teslimden sonraki caymada iade edilen malın firmaya ulaştığı tarihten işler; "tevkif hakkı" ifadesi kalktı. İşlenen yerler: §1.2 (İade, Geri ödeme) · §4.2 Z-16 · §5.5 · §6.4.8 ve iki yeni satır (6.4.16, 6.4.17) · §7.3.5 · §7.4.1 · §7.4.7 · §10.1.2 · §10.4.5 · §10.5.2 · §10.6.1 · §12.1.9. Aynı araştırmanın üç komşu bulgusu öneriyle kaydedildi: iade kargo bedelinin firmada olması artık yasal zorunluluktur (K-492; §7.4.2, §12.1.9) · Ön Bilgilendirme Formu iade için taşıyıcı belirlenmediğini yazar (K-493; §3.24.3, §7.4.2, §7.4.4, §12.1.9) · müşteri malı cayma beyanından itibaren on dört gün içinde gönderir (K-494; §1.2, §3.24.3, §4.2 Z-16, §6.4.16, §7.3.5, §7.4.4, §12.1.9). İki konu açık kaldı ve bu dokümanın kalite döngüsünde, audit'ten önce kapanır: malı hiç dönmeyen caymada §10.6.3'teki iptal ve iade oranının okunma anı ve bekleyen işler sayacı (tracker A-15) · kısmi caymada gidiş kargo bedeli kuralının (K-292) güncel metne uyumu (tracker A-16). §13.4 buna göre güncellendi.
> **A-15'in kapanışı (2026-10-03, v0.12 — K-29):** malı hiç dönmeyen caymanın iki ucu proje sahibinin kararıyla kapandı. **Ölçü (K-495 — K-488'in okunma anını değiştirir):** cayma, beyan edildiği anda iade sayılır; iptal ve iade oranı cayma pencereleri kapanınca okunur ve geri ödeme süresinin kapanması beklenmez (§10.6.3). **Bekleyen iş (K-496):** müşterinin malı gönderme süresi geçtikten sonra firma, malı dönmemiş cayma kalemini "mal dönmedi" gerekçesiyle kapatabilir; kapatma işlem izine yazılır, müşteriye bildirilmez ve geri ödeme yapılmaz; mal sonradan ulaşırsa kalem yeniden açılır ve süre ulaşma tarihinden işler. Kapatma §10.4'ün yedinci müdahalesidir ve zorunlu bir elle adım değildir — bütçe değişmez. İşlenen yerler: §4.2 Z-16 · §5.8 · §6.4.16 ve yeni satır 6.4.18 · §7.4.1 · §7.4.4 · §9.2 · §10.1.2 · §10.3.1 · §10.4.1 ve yeni §10.4.9 · §10.5.2 · §10.6.1 · §10.6.3. Kapanış bir açık doğurdu: malı dönmeyen caymada havale hattı IBAN'ının ne zaman silineceği (tracker A-17); §13.4 buna göre güncellendi.
> **A-17'nin kapanışı (2026-10-03, v0.13 — K-29):** malı dönmeyen caymada havale hattı IBAN'ının saklanma süresi proje sahibinin kararıyla kapandı (K-497). IBAN geri ödeme tamamlanınca silinir — bu kural değişmedi (K-356); yanına iki silme anı eklendi: firma kalemi "mal dönmedi" gerekçesiyle kapattığında, ve firma kapatmazsa müşterinin malı gönderme süresinin bitiminden otuz gün sonra sistem kendiliğinden siler. Mal sonradan ulaşırsa geri ödeme için IBAN müşteriden yeniden istenir. Otuz gün yeni bir ürün sabitidir (P-43) ve §4.2'ye yeni bir süre girdi (Z-38). İşlenen yerler: §4.2 Z-35 ve yeni Z-38 · §4.3 · §5.8 · §6.4.16 ve yeni satır 6.4.19 · §7.4.4 · §7.4.5 · §8.4.2 · §9.2 · §10.4.9 · §11.2 yeni P-43 · §12.2.7 · §13.4. Kapanış bir açık doğurdu: IBAN'ın müşteriden yeniden isteniş biçimi — kanal, bildirim ve IBAN beklenirken geri ödeme süresinin işleyişi (tracker A-18); §13.4 buna göre güncellendi.
> **A-18'in kapanışı (2026-10-03, v0.14 — K-29):** IBAN silindikten sonra ulaşan malda IBAN'ın müşteriden yeniden isteniş biçimi öneriyle kaydedildi ve proje sahibinin gözden geçirmesi için ⚠ ile işaretlendi (K-498). **Kanal:** firma teslim alma adımını işlediğinde müşterinin sipariş sayfasında o kalem için IBAN alanı yeniden açılır ve müşteri IBAN'ı oradan girer; firma IBAN'ı panelden girmez. **Bildirim:** müşteriye yeni bir işlem bildirimi gider (§9.2 B-14) — müşteriye giden bildirimler on üçten on dörde çıktı; e-posta ulaşmasa da sipariş sayfası alanı gösterir. **Süre:** geri ödemenin on dört günü malın ulaştığı tarihten işler ve IBAN beklenirken durmaz; panel kalemi "IBAN bekleniyor" olarak ve kalan süreyle gösterir, IBAN isteğinin tarihi işlem izine yazılır. Müşteri IBAN'ı girdiğinde firma geri ödemeyi mevcut elle adımla işler; bütçe değişmez. İşlenen yerler: §4.2 Z-16 ve Z-35 · §5.8 · §6.4.19 ve yeni satır 6.4.20 · §7.4.5 · §8.4.2 · §9.2 yeni B-14 · §10.1.2 · §10.3.1 · §10.6.1 · §13.4.
> **Kalite döngüsü — audit ve deep review (2026-10-03, v0.15 — K-431):** on beş mercekle koştu; 190 bulgu iki şüpheciyle karşı-doğrulandı — 185'i doğrulandı, 5'i tartışmalıydı, çürütülen yok. 138'i bu sürümde işlendi, 48'i aynı döngüde başka bir bulguyla zaten karşılanmıştı, tartışmalı 4'ü yönetici kararıyla işlenmedi; resmî metne karşı doğrulamada çürüyen karar çıkmadı. Altmış bir karar kaydedildi (K-499…K-559): yirmi yedisi proje sahibinin talimatıyla öneriyle kaydedildi ve gözden geçirmesi için ⚠ ile işaretli (K-499…K-525), otuz dördü öneriyle (K-526…K-559). **A-16 kapandı** (K-501): kargo ücretinin geri ödenme tetiği siparişin iptal edilmemiş fiziksel kalemlerinin tamamıdır (§7.2.9, §7.4.6). Sayımlar: sözlük 86 → **95 terim** · süreler **Z-1…Z-44** (yeni Z-39…Z-44; müşterinin gönderme süresi Z-42'dir) · parametreler **P-1…P-46** — P-2 ve P-11 kaldırıldı, kimlikleri yeniden kullanılmaz; on firma ayarı ve otuz dört ürün sabiti, toplam **kırk dört** (§11) · deneme limitleri **L-1…L-8** · müdahaleler **sekiz** (§10.4) · bildirimler değişmedi (B-1…B-14, F-1…F-4) · ödeme geçişleri P1…P5 → **Ö1…Ö5** (K-532). Raporlar `Docs/AUDIT_REPORTS/02_AUDIT.md` ve `02_DEEP_REVIEW.md`. Etki yansıtması: `01` v0.28, `10` v0.8. Sırada cross-review var.
> **Kalite döngüsü — cross-review 1. tur (2026-10-03, v0.16 — K-431):** Codex CLI (`gpt-5.6-terra`) dört bulgu döndürdü; değerlendirme `Docs/CROSS_REVIEW_REPORTS/02_CROSS_REVIEW.md`'dedir. İşlenenler: adresin alanları yazıldı — ad-soyad, il, ilçe ve açık adres zorunlu; teslimat adresinde telefon zorunlu; posta kodu ve T.C. kimlik numarası istenmez (§3.14.6, §1.2, §12.2.3; K-561) · sipariş e-postasındaki bağlantı numaradan ayrı bir erişim anahtarı taşır, numara tek başına sayfayı açmaz; bağlantının ömrü ve kalan riski yazıldı (§3.22.5, §8.3.5; K-562) · geçmişi kısa varyantta referans fiyatın başlangıcı varyantın satışa sunuluşudur (§3.9.4, §1.2, Z-21; K-563) · boş alternatif metnin geri düşüşü görselleri birbirinden ayırır (§3.11.4, §3.27.15; K-564). K-561 ve K-562 kişisel veriye dokunduğu için proje sahibinin gözden geçirmesine ⚠ ile işaretli.
> **Kalite döngüsü — cross-review 2. tur (2026-10-03, v0.17 — K-431):** Codex CLI (`gpt-5.6-terra`) altı bulgu döndürdü; değerlendirme `Docs/CROSS_REVIEW_REPORTS/02_CROSS_REVIEW_R2.md`'dedir. İşlenenler: kupon siparişi sıfır tutara indiremez — sabit tutarlı kuponun asgari sepet tutarı kupon tutarından büyüktür, yüzdesel kupon %100'e ulaşamaz (§3.10.4, §1.2, §6.2.12, §6.6.8; K-565) · yanlış iptalin düzeltilmesi yalnız ödeme Ödendi'de kalmış ve para geri gönderilmemişse yapılır ve iptalin serbest bıraktığı ayırmaları yeniden ister (§10.4.6, §5.4, §7.6.3, §6.7.5; K-566) · sipariş numarası bütün siparişler arasında tekildir (§3.22.2; K-567) · kalıcı silinen kaydın adresi serbest kalır (§3.30.2, §3.30.4; K-568) · cayma beyanıyla kapanan kalem ayrı bir kalem durumu değildir — "Cayıldı" etiketi kalktı (§5.8, §6.4.21) · iptal ve iade oranının adındaki "iade"nin sözlükteki İade'den geniş olduğu yazıldı (§10.6.3). K-566 önceki bir kararın (K-504) yan etki kuralından ayrıldığı için proje sahibinin gözden geçirmesine ⚠ ile işaretli.
> **Kalite döngüsü — cross-review 3. tur (2026-10-03, v0.18 — K-431):** Codex CLI (`gpt-5.6-terra`) altı bulgu döndürdü; değerlendirme `Docs/CROSS_REVIEW_REPORTS/02_CROSS_REVIEW_R3.md`'dedir. İşlenenler: sağlayıcıda başarısız kart iadesinde havale yolu müşterinin seçimidir, IBAN girilene kadar kart iadesi yeniden denenebilir (§7.2.8, §5.5, §6.4.24, §7.4.5, §8.4.2, §9.2 B-14, §10.1; K-569) · para gelmeden konmuş havale "ödendi" işareti ödeme ve sevkiyat birlikte geri alınarak düzeltilir, ödeme süresi düzeltmeden yeniden işler (§10.4.6, §5.5, §5.6.1, §4.2 Z-8, §6.7.5, §9.2 B-13; K-570) · gecikme feshi gecikmenin dokunduğu fiziksel kalemlerle sınırlıdır, geri ödenen tutar onların bedeli ve kargo ücretidir (§7.2.3, §1.2, §4.2 Z-11, §12.5; K-571) · KVKK talebinin hukuki sebebi m.5/2-ç'dir (§12.2.3; K-572) · teslim tarihi olmayan kalemde iade teslim almanın alt sınırı yazıldı (§7.4.7) · geç teslim işaretinde pencerenin yeniden başlatılmamasının gerekçesi yazıldı (§7.3.1). Dört karar kişisel veriye ya da paraya dokunduğu için proje sahibinin gözden geçirmesine ⚠ ile işaretli.
> **Kalite döngüsü — cross-review 4. tur (2026-10-03, v0.19 — K-431):** Codex CLI (`gpt-5.6-terra`) dört bulgu döndürdü; değerlendirme `Docs/CROSS_REVIEW_REPORTS/02_CROSS_REVIEW_R4.md`'dedir. İşlenenler: ölçü birimi alanının isteğe bağlı olması muafiyet değildir, ölçüyle satan firma alanı doldurur ve panel bunu hatırlatır (§3.8.5, §12.1.5, §12.5; K-573) · fiziksel kalemlerin teslim tarihi sipariş düzeyinde bir kez girilir ve hepsine aynı yazılır, düzeltmesi de birliktedir (§1.2, §3.20.11, §5.8, §10.4.5) · ETBİS doğrulama bilgisi firma kimliğinin isteğe bağlı bir alanıdır, doluysa band görünür (§3.1.2, §3.1.4, §10.1.2, §12.1.4, §12.5; K-574) · müşteri IBAN'ını geri ödeme işlenene kadar düzeltebilir, bankada geri dönen havalede IBAN yeniden istenir (§7.4.5, §6.4.27, §8.4.2, §9.2 B-14, §10.1.2, §10.3.1, §10.5.2, §10.6.1; K-575). Üç karar yasal yükümlülüğe, paraya ya da kişisel veriye dokunduğu için proje sahibinin gözden geçirmesine ⚠ ile işaretli.
> **Kalite döngüsü — cross-review 5. tur (2026-10-03, v0.20 — K-431):** Codex CLI (`gpt-5.6-terra`) dört bulgu döndürdü; değerlendirme `Docs/CROSS_REVIEW_REPORTS/02_CROSS_REVIEW_R5.md`'dedir. İşlenenler: satış kapısının yasal metin koşulu veri toplayan girişlerin kapısıyla aynı tanımı kullanır — aydınlatma metni tamamlanmış ve yayına alınmış, çerez politikası yayına alınmış olmalıdır (§3.1.5, §6.9.10, §10.8.2; K-576) · fiziksel kalem sipariş kargoya verildikten sonra çıkarılamaz (§5.4, §10.4.3; K-577) · ürün hizmet ifasının başladığı anı izlemez; kısmen ifa edilmiş hizmetten caymada kalemin bedelinin tamamı geri ödenir (§7.3.3, yeni §6.4.28, §12.1.9; K-578). Geri ödeme süresinin teslimden sonraki caymada malın ulaşmasıyla başlaması reddedilen bulgudur: Yönetmelik m.12/1'in 2022 değişikliğinden sonraki metnidir (§7.4.1). İki karar yasal yükümlülüğe ya da paraya dokunduğu için proje sahibinin gözden geçirmesine ⚠ ile işaretli.
> **Kalite döngüsü — cross-review 6. tur (2026-10-03, v0.21 — K-431):** Codex CLI (`gpt-5.6-terra`) dört bulgu döndürdü; değerlendirme `Docs/CROSS_REVIEW_REPORTS/02_CROSS_REVIEW_R6.md`'dedir. İşlenenler: L-7 ve L-8'de e-posta ekseni yalnız giriş yapmış üyenin siparişlerini sayar, misafir siparişi yalnız IP ekseninde sayılır — başkasının e-postasını yazan biri onu engelleyemez (§3.17.8, §3.21.5, §6.2.19, §6.2.21, §8.2, §11.2 P-37, P-46; K-579) · gecikme feshi iptal ve iade oranına ve iptal ve iade sayısına girer (§10.6.3; K-580) · sepetten düşen kalemde siparişin kalan kalemlerle sürmesi §3.17.2'nin yeniden onayına bağlandı (§3.16.6, §3.16.7, §6.2.4) · yönetmeliğin adı "Fiyat Etiketi Yönetmeliği" olarak düzeltildi ve değişikliğin tarihi yazıldı (§3.9.3, §12.1.6). Referans fiyat penceresinin otuz gün olması reddedilen bulgudur: m.11/1 RG 11/10/2025-33044'ten beri on gündür. K-579 önceki bir kararın eksenini daralttığı için proje sahibinin gözden geçirmesine ⚠ ile işaretli.
> **Kalite döngüsü — cross-review 7. tur (2026-10-03, v0.22 — K-431):** Codex CLI (`gpt-5.6-terra`) dört bulgu döndürdü; değerlendirme `Docs/CROSS_REVIEW_REPORTS/02_CROSS_REVIEW_R7.md`'dedir. İşlenenler: başka kanaldan gelen cayma bildirimi IBAN taşımıyorsa kayıt IBAN'sız yapılır, IBAN sipariş sayfasından B-14 ile istenir ve süre durmaz (§5.8, §6.4.25, §7.4.5, §8.4.2, §9.2 B-14, §10.4.10, §10.6.1; K-581) · süresi dolan siparişin hesapla bağı da kesilir ve kalan ticari kaydın alanları sayıldı (§4.2 Z-28, Z-41, §12.2.7; K-582) · T.C. kimlik numarası cümlesine Gelir İdaresi'nin 11111111111 uygulaması yazıldı (§3.14.6) · öneriyle kaydedilen ⚠ kararın kapalı olduğu §13.4'e yazıldı.
> **Kalite döngüsü — cross-review 8. tur (2026-10-03, v0.23 — K-431):** Codex CLI (`gpt-5.6-terra`) üç bulgu döndürdü; değerlendirme `Docs/CROSS_REVIEW_REPORTS/02_CROSS_REVIEW_R8.md`'dedir. İşlenenler: e-postaya bağlı olmama ilkesi sipariş ve talep akışlarıyla sınırlandı, hesap ve yönetim bağlantıları istisna olarak yazıldı (§3.22.4, §6.1.1, §9.1.2) · süresi dolan siparişin kalan kaydı, firmanın elinde eşleşen fatura ya da yazışma durdukça anonim sayılmaz; o kayıtların imhası firmanın yükümlülüğü olarak yazıldı (§4.2 Z-28, §12.2.7, §12.5; K-583) · §8.1.2'nin başlığı hesaba bağlamayı sipariş sayfasına erişimden ayıracak biçimde netleştirildi.
> **Kalite döngüsü — cross-review 9. tur (2026-10-03, v0.24 — K-431):** Codex CLI (`gpt-5.6-terra`) dört bulgu döndürdü; değerlendirme `Docs/CROSS_REVIEW_REPORTS/02_CROSS_REVIEW_R9.md`'dedir. İşlenenler: sözlükteki "Yayında" tanımı ürünün yayın kapısına hizalandı — en az bir Yayında varyant (§1.2; K-538) · kurumsal içeriğin zorunlu alanları tipe göre yazıldı; ad ya da başlık taşımayan Hakkımızda'nın kapısı kısa tanıtım, duyurunun kapısı metindir (§1.2, §3.27.13, §3.27.15, §3.27.24, §5.2; K-584) · manuel adım bütçesinin başlığı hat başına okunuşa hizalandı (§10.5.4) · 8. turun sürüm notundaki bölüm listesi düzeltildi. Misafirin onaydan sonra fark edilen yanlış e-postası (BULGU-2) proje sahibinin kararına bırakıldı; bu sürümde işlenmedi.
> **Kalite döngüsü — cross-review 9. turun açık bulgusu (2026-10-03, v0.25 — K-431):** 9. turun BULGU-2'si — misafir alıcının onaydan sonra fark ettiği yanlış e-posta — proje sahibinin talimatıyla yönetici kararıyla kapandı (K-585, seçenek A). Firma misafir siparişinin iletişim e-postasını müşterinin talebi üzerine ve kimliği siparişteki bilgilerle teyit ederek panelden düzeltir; yeni adrese yeni erişim anahtarıyla sipariş e-postası gider, eski anahtar geçersizleşir, eski adrese "siparişinizin e-posta adresi değiştirildi" bildirimi gider ve değişiklik eski ve yeni değerle işlem izine yazılır. Sayımlar: müdahaleler **dokuz** (§10.4.1, yeni §10.4.11) · müşteriye giden bildirimler **on beş** (yeni B-15). İşlenen yerler: §1.2 (Misafir alıcı) · §3.13.3 · §3.22.5 · §3.23.3 · §4.2 Z-43 · §6.1.1 · §6.2.13 · §6.3.1 ve yeni satır 6.3.5 · §8.1.2 ve yeni §8.3.6 · §9.1.6 · §9.2 (B-1, yeni B-15) · §10.1.2 · §10.3.1 · §10.4.1 ve yeni §10.4.11 · §10.5.2 · §12.5. Kişisel veriye, paraya ve önceki iki karara (K-113'ün donması, K-527'nin iz kuralı) dokunduğu için proje sahibinin gözden geçirmesine ⚠ ile işaretli.


> **Karar referansları:** Metindeki `K-xx` işaretleri `PRODUCT_DISCOVERY_STATUS.md` §2 karar kaydına, `Bx-yy` işaretleri aynı dosyanın §6.3 blok içeriklerine gider. Kayıt aşama kapanışında arşiv işareti alır ve yerinde kalır; K numaraları çözülmeye devam eder (K-436). `Z-`, `L-`, `B-`, `F-`, `P-`, `H-` ve sevkiyat ile ödeme geçişlerinin `S`/`Ö` kimlikleri bu dokümanın kendi satır kimlikleridir (§4, §8, §9, §11, §5). Aileler önekle ayrılır: `P-1` bir parametredir (§11), ödeme geçişleri `Ö1…Ö5`'tir (§5.5) — parametreyle karışmasın diye `P` önekini taşımaz; sevkiyat geçişleri `S1…S11`'dir, `01 §7`'nin sınırlarına (`S-5`) atıf her yerde `01 §7` diye nitelenir (K-532).

---

## 0. Nasıl kullanılır

- Her iş kuralı **numaralandırılır** ve bölüm referansıyla anılır (`§4.7`). Sonraki dokümanlar ve task'lar bu numaraya atıf yapar.
- Sayısal parametreler (süre, oran, limit, eşik) **tablo hâlinde** toplanır — dağınık yazılırsa tutarsızlık kaçınılmaz olur.
- **Admin esnekliği prensibi:** Rakamsal parametreleri yönetici tarafından değiştirilebilir yapmak, "doğru rakam ne?" tartışmasını ürün aşamasından çıkarır. Hangi parametrenin panelden ayarlanabildiği §11'de katmanıyla yazılıdır: yanlış değerin zararı yalnız firmaya dokunan işletme kararları firma ayarıdır; zararı güvenliğe, yasal uyuma ya da müşteriye dokunan sayılar ürün sabitidir (K-412).
- "Muhtemelen", "belki", "sonra karar veririz" yasak. Detay ileriye bırakılabilir; **varlık kararı** burada alınır.

---

## 1. Terimler ve aktörler

> **Ne yazılır:** Projede kullanılan her terimin tek tanımı. Aynı kavram iki farklı isimle anılmamalı.

### 1.1 Sözlük kuralı

Terim sözlüğü **bu bölümde yaşar** ve üç sütunludur: Türkçe terim · İngilizce kod karşılığı · tanım. Kurallar (K-17):

- **İngilizce karşılık Aşama 1'de yazılır** — terim sözlüğe girdiği anda. Sonraya bırakılmaz.
- **Bir kavramın tek adı vardır.** Eş anlamlı kullanım yasaktır; aynı şeyi anlatan iki terim birlikte kullanılamaz.
- **`06`, `07` ve `09` bu sözlüğü birebir devralır**, isim icat etmez.
- **Kapalı listeler sözlüğe liste adı ve İngilizce karşılığıyla girer; liste değerlerinin kod adları `06`'da verilir.** Bu, ilk kuralın tek istisnasıdır: değerler gövdede kapalı listeyle tek yerde yazılıdır ve `06` onları birebir adlandırır (K-531).

Kuralın gerekçesi `03` ve `04`'ün `06`'dan **önce** yazılmasıdır: İngilizce karşılık veri modeline bırakılsaydı o aralıkta durum ve enum adları başıboş kalır, `06` geldiğinde geriye dönük hizalama gerekirdi (K-17).

**Bedeli kayıtlıdır:** henüz varlığa dönüşmemiş kavramlara erken isim verilir ve `06` yazılırken birkaç satırın revize edilmesi beklenir — bu bir sapma değil, normal akıştır (K-17).

### 1.2 Terim sözlüğü

> **İngilizce karşılıkların kaynağı:** Her satırın kod karşılığı karar kaydında yazılıdır. Taslak boyunca karşılığı kayıtta olmayan otuz altı satır sözlüğün kendi konvansiyonundan türetilmiş ve † ile işaretlenmişti (**A-05**). Biri — Açık → `Open` — K-303 ile kayda geçti; kalan otuz beşi yazım turunda olduğu gibi onaylandı ve A-05 kapandı (K-455). `02`'nin kalite döngüsünde giren satırların — Müşteri (K-526), Gecikme feshi (K-499) ile yedi kapalı liste ve kayıt adı: Firma tipi · Konu tipi · İptal sebebi · Cayma istisnası sebebi · Düzeltme sebebi · Ana sayfa düzeni · Teslim tarihi (K-531) — karşılığı aynı konvansiyonla türetildi ve † taşır.

**Katalog düzlemleri**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Ürün | `Product` | Vitrin ve katalog birimi. Ad, açıklama, kategori, tip, KDV oranı, indirim, üretim yeri ve — ölçüyle satılan üründe — ölçü birimi ile net miktar bu düzlemde yaşar; görsel de üründedir ama varyant kendi görselini taşıyarak ezebilir. Ürün doğrudan satılmaz — satılan birim varyanttır (§3.8.5; K-39, K-59, K-64, K-82, K-90, K-96, K-511). |
| Varyant | `Variant` | Satılan birim. Stok ve fiyat bu düzlemde yaşar. Seçeneği olmayan ürünün de tek bir varyantı vardır ve ziyaretçi bunu fark etmez (K-39). |
| Seçenek | `Option` | Varyantları ayıran boyutun adı — "Renk", "Beden". Bir ürün **en fazla iki** seçenek boyutu taşır (K-40, K-41). |
| Seçenek değeri | `OptionValue` | Bir seçeneğin aldığı değer — "Kırmızı", "M" (K-41). |
| Stok kodu | `SKU` | Her varyantın taşıdığı **zorunlu ve tekil** kod. Tip ayrımı yoktur; dijital ve hizmet varyantı da kod taşır (K-88). |

**Ürün tipi**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Ürün tipi | `ProductType` | Ürünün üç değerli **kapalı** alanı. Ürün düzleminde yaşar; ürünün tüm varyantları aynı tiptedir, varyant tipi devralır ve kendi tipini taşımaz (K-82). Niteleyicisiz "tip" firma tipi, konu tipi ve içerik tipiyle karışmasın diye terim **Ürün tipi**dir (K-531). |
| Fiziksel ürün | `Physical` | Tip değeri. Kargoyla teslim edilen mal; sayısal stok takibi **zorunludur** (K-81, K-83). |
| Dijital ürün | `Digital` | Tip değeri. İndirilebilir dosya; stok alanı **yoktur**, sınırsız satılır (K-81, K-83). |
| Hizmet | `Service` | Tip değeri. Sabit fiyatlı, randevusuz hizmet. Firma isterse toplam adet **kontenjanı** girer; girmezse sınırsızdır (K-81, K-83). Zorunlu bir **ifa süresi** taşır — ödeme onayından itibaren en geç kaç gün içinde ifa edileceği (§3.2.2; K-503). Satın alınmayan, fiyatsız **Hizmet tanıtımı**ndan ayrıdır (K-240). |

**Sınıflandırma**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Kategori | `Category` | Ürünün kataloğdaki yeri. Ağaç **en fazla üç seviyedir**; ürün ağacın herhangi bir düğümüne asılabilir, yaprak zorunluluğu yoktur (K-42, K-43). |
| Ana kategori | `PrimaryCategory` | Ürünün asıldığı kategorilerden biri. Kırıntı yolu bundan üretilir; yayına çıkacak ürün için **zorunludur** (K-45). Ürünün **adresi kategoriden bağımsızdır** — ürünün adından üretilir (K-279). |

**Yayın durumu**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Yayın durumu | `PublishStatus` | Ürün ve varyantta üç değerli alan. **Her iki düzlemde de yaşar:** ürünün kendi durumu, varyantın kendi durumu vardır (K-54, K-55). **Kurumsal içerik yalnız iki değerini kullanır** — Taslak ve Yayında (K-273). |
| Taslak | `Draft` | Yayında olmayan ve üzerinde çalışılan kayıt — hiç yayınlanmamış ya da yayından taslağa çekilmiş (K-273, K-298, K-531). Adresi ziyaretçiye 404 döner; giriş yapmış firma yöneticisine sayfayı "Taslak" bandıyla gösterir (K-54, K-57, K-274). |
| Yayında | `Published` | Vitrinde görünen kayıt. Bir ürünün "Yayında" olabilmesi için **en az bir Yayında varyantı** olmalıdır — taslak varyant ziyaretçiye görünmediği için koşulu karşılamaz (K-54, K-55, K-538); bir kurumsal içerik kaydının zorunlu alanları dolu olmalıdır (§3.27.13; K-275, K-584). |
| Arşiv | `Archived` | Vitrinden çıkmış ama kaydı korunan ürün ya da varyant (§3.7.1, §5.1). **Silme değildir** — adresi çalışmaya devam eder ve "Bu ürün artık satılmıyor" sayfası döner (K-54, K-56). Kalıcı silme ayrı bir yoldur; silinen ürünün adresi 404 döner (K-85). **Kurumsal içerikte arşiv yoktur** (K-273). |

**Stok ve fiyat**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Stok | `StockQuantity` | Varyantın sayısal adedi. Ürün düzleminde ayrı bir stok alanı **yoktur**; ürünün stok durumu varyantlarından türetilir (K-50). |
| Stok ayırma | `StockReservation` | Sipariş onaylandığı anda siparişe giren adetlerin stoktan ayrılması. Ödeme başarılı olursa adet **kesin düşer**; ödeme süresi dolar ya da ödeme başarısız olursa stoğa geri döner. Sepete eklemek stok ayırmaz. Aynı rejim hizmet kontenjanına ve kupon kullanım hakkına uygulanır. **"Rezervasyon" kelimesi bilerek kullanılmadı** — K-11 o kelimeyi randevu anlamında kapsam dışı bıraktı (K-11, K-128). |
| KDV oranı | `VatRate` | Ürün düzleminde tutulan oran; varyantlar devralır. Varsayılanı ayardan gelir (K-58, K-59). |

**İndirim ve kupon**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| İndirim | `Discount` | **Ürün düzleminde** tanımlanan ve tüm varyantlara inen **yüzdesel** indirim; her varyant kendi fiyatı üzerinden indirilir. **Tarihlidir** — başlangıç ve bitiş girilir, sistem indirimi kendisi başlatır ve bitirir (K-63, K-64, K-65, K-66). |
| Referans fiyat | `ReferencePrice` | İndirim beyanının dayandığı fiyat. Fiziksel ve dijital üründe indirimin başladığı andan önceki **10 gün** içinde o varyanta uygulanmış **en düşük** fiyat; hizmette ve cayma istisnası sebebi "çabuk bozulan" olan üründe indirimli fiyattan **hemen önceki** fiyat. **Sistem hesaplar**, yönetici giremez. 10 günlük geçmişi olmayan malda referans, o varyantın satışa sunulduğu andan beri uygulanmış en düşük fiyattır (§3.9.3, §3.9.4; K-63, K-67, K-508, K-563). |
| Kupon | `Coupon` | Ödeme adımında girilen kod; **sepet toplamına** iner ve indirimli fiyatın üzerine uygulanır. Yüzde veya sabit tutar olabilir; siparişi sıfır tutara indiremez ve sabit tutarlı kupon, kupon tutarından büyük zorunlu bir asgari sepet tutarı taşır. Sınırı **tarih + toplam kullanım adedidir** (K-70, K-71, K-73, K-74, K-75, K-565). |

**Aktörler ve taraflar**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Firma | `Company` | Kurulumun sahibi ve **satıcı**. Uygulamada tekildir: firma ekleme, firma seçme ve firmalar arası geçiş kavramı yoktur (K-01, K-09). Tahsilat firmanın kendi ödeme sağlayıcı hesabına geçer; platform ticari zincirde yer almaz (K-12). Yasal adı **unvan**, vitrinde görünen adı **marka adı**dır (K-260). Kişisel verilerin **veri sorumlusudur** (K-341). |
| Firma tipi | `CompanyType` † | Firmanın hukuki biçimini gösteren üç değerli **kapalı** liste — esnaf, gerçek kişi tacir, tüzel kişi. Zorunlu kimlik alanları ve unvanın biçimi tipe göre değişir; değerler §3.1.3'tedir (K-480, K-531). |
| Ziyaretçi | `Visitor` | Siteye üye olmadan gelen kişi (K-06). |
| Misafir alıcı | `GuestBuyer` | Hesap açmadan sipariş veren alıcı. Siparişini **sipariş numarası + e-posta** ile takip eder; doğrulanmış e-postayla hesap açtığında o e-postaya ait geçmiş siparişleri hesabına düşer. E-postasını yanlış yazdığını onaydan sonra fark ederse firma, kimliğini teyit ederek siparişin e-postasını panelden düzeltir (§10.4.11) (K-97, K-98, K-585). |
| Üye müşteri | `Customer` | Hesabı olan alıcı (K-06). `Customer` yalnız üye müşteriyi gösterir. Tek başına "üye", üye müşterinin kısaltması olarak kullanılabilir; tek başına "müşteri" ise iki alıcıyı birlikte anar (K-17, K-526). |
| Müşteri | `Buyer` † | Misafir alıcı ya da üye müşteri olarak sipariş veren kişi. **Aktör değildir** — iki aktörün ortak adıdır; metinde tek başına "müşteri" bu anlamdadır (§1.3; K-526). |
| Firma yöneticisi | `Admin` | Firmanın panel kullanıcısı. Yönetim tarafı **tek roldür ve çoklu kullanıcıya açıktır** (K-06). İlk hesap kurulumda doğar, sonrakiler davetle açılır (K-308). Yönetici hesabının sepeti ve siparişi olmaz (K-312). |
| Tüketici | `Consumer` | Alıcının **hukuki sıfatı**: kişisel ihtiyacı için alan gerçek kişi. Ayrı bir aktör değildir — ürün tüketiciye satış için kurulmuştur ve 6502 sayılı Kanun'un tüketici akışını her siparişte işletir; alıcının hukuken tüketici sayılıp sayılmadığını mevzuat belirler (K-07, K-112). |

**Hesap ve oturum**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Oturum | `Session` | Kullanıcının giriş yapmış hâli. Giriş ekranındaki "beni hatırla" seçimine göre **kısa** veya **uzun ömürlü** açılır; şifre sıfırlandığında, hesap silindiğinde ya da yönetici kaldırıldığında hesabın tüm oturumları düşer (K-106, K-108, K-115, K-314). |
| Sosyal giriş | `SocialLogin` | Kullanıcının bir kimlik sağlayıcısı üzerinden giriş yapması. MVP'de tek sağlayıcı **Google**'dır; Facebook kapsam dışındadır (K-103). Sağlayıcının **doğrulanmış** verdiği e-posta mevcut bir hesabınkiyle eşleşirse aynı hesaba bağlanır (K-104). |
| Adres defteri | `AddressBook` | Üye müşterinin kaydettiği ve adlandırdığı adreslerin listesi (ör. Ev, İş). **Misafir alıcının adres defteri yoktur** — her siparişte adresini yazar (K-97, K-111). |
| Teslimat adresi | `ShippingAddress` | Siparişin gönderileceği adres: alıcının adı ve soyadı, telefonu, il, ilçe ve açık adres (§3.14.6). Sipariş anında **siparişin içine donar**; defterdeki sonraki değişiklik veya silme geçmiş siparişe dokunmaz (K-112, K-113, K-561). |
| Fatura adresi | `BillingAddress` | Faturanın kesileceği adres: adı ve soyadı, il, ilçe ve açık adres — telefon taşımaz (§3.14.6). Varsayılan olarak teslimat adresiyle aynıdır; kullanıcı farklı bir adres seçebilir ve o da sipariş anında donar (K-112, K-113, K-561). |

**Sepet**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Sepet | `Cart` | Müşterinin satın almaya aday kalemlerini tuttuğu liste. **Üyenin sepeti hesabında yaşar** ve her cihazda aynıdır; misafirin sepeti bulunduğu tarayıcıya bağlıdır. Kendiliğinden boşalmaz — misafirinki tarayıcının verileri silinene, üyeninki hesap durdukça yaşar. Ödeme başarılı olduğunda siparişe giren kalemler sepetten çıkar (K-123, K-126, K-127). |
| Sepet kalemi | `CartItem` | Sepetteki bir varyant ve adedi. **Canlıdır:** fiyatı ve satın alınabilirliği güncel üründen okunur — sipariş kalemi ise sipariş anında donar (K-80). Varyantın sepete **ilk eklendiği** andaki birim fiyatını referans olarak hatırlar; güncel fiyat bundan farklıysa satırda *"Sepete eklediğinden beri fiyatı değişti"* yazar, eski fiyat ve değişimin yönü gösterilmez (K-131). |

**Sipariş**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Sipariş | `Order` | Müşterinin onayladığı ve ödeme sürecine giren alışveriş. Onaylandığı anda oluşur, bir sipariş numarası alır ve **iki eksende** durum taşır: sevkiyat ve ödeme. Kalemleri, adresleri, firmanın o günkü kimliği ve o gün yürürlükteki üç yasal metnin sürümü — Ön Bilgilendirme Formu, Mesafeli Satış Sözleşmesi, aydınlatma metni — oluştuğu anda içine donar (K-77, K-80, K-113, K-170, K-186, K-189, K-190, K-347). |
| Sipariş kalemi | `OrderItem` | Siparişteki bir varyant, adedi ve sipariş anında donan değerleri — birim fiyat, KDV, indirim oranı ve beyan edilen referans fiyat, kuponun payı; ürün adı, seçenek değerleri ve görsel. Sepet kaleminin aksine **sipariş anında donar** (K-77, K-78, K-79, K-80). İptal ve iade kalem düzeyinde işler (K-178); her kalem kendi teslim işaretini ve tarihini taşır — fiziksel kalemde firmanın girdiği teslim tarihi, dijitalde ödeme onayı, hizmette tamamlama işareti (K-179, K-288, K-290). |
| Teslim tarihi | `DeliveryDate` † | Kalemin teslim işaretinin taşıdığı tarih: fiziksel kalemde firmanın teslim işaretiyle girdiği tarih — geçmişe dönük olabilir, ileri tarihli olamaz —, dijital kalemde ödeme onayı, hizmet kaleminde tamamlama işareti. Fiziksel kalemlerin tarihi sipariş düzeyinde bir kez girilir ve siparişin fiziksel kalemlerinin tamamında aynıdır (§3.20.11; K-177). Fiziksel kalemin cayma penceresi ve üç tipte ayıp talebinin iki yılı bu tarihten işler (§5.8; K-179, K-288, K-290, K-531). |
| Sipariş numarası | `OrderNumber` | Siparişin **tahmin edilemez** kimliği: yıl + rastgele blok (ör. `2026-7K4M9P`). Sipariş oluştuğunda verilir ve iptal edilse bile tekrar kullanılmaz; **fatura numarası değildir** (K-185, K-186). |

**Sipariş durumu — sevkiyat ekseni**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Sipariş durumu | `OrderStatus` | Siparişin sevkiyat ekseni. Ödeme ekseninden ayrıdır ve ona iki biçimde bağlanır (§5.6): **tek kapı koşulu** — "Hazırlanıyor"a ancak ödeme "Ödendi" olduğunda geçilir (K-173); **eşzamanlı kendiliğinden geçişler** — fiziksel kalemsiz siparişte ödeme onayı dijital kalemleri teslim eder ve yalnız dijital siparişi "Teslim edildi"ye alır (K-174, K-294), ödenmeden iptal ödemeyi "Başarısız" yapar (K-296). Geçişler beyaz listeyle tanımlıdır, listede olmayan her geçiş yasaktır (K-170, K-224, K-531). |
| Alındı | `Placed` | Siparişin oluştuğu andaki durum. Ödeme onaylanana kadar sürer; fiziksel kalemi olmayan siparişte bütün kalemler teslim işaretini alana kadar (K-171, K-175, K-294). |
| Hazırlanıyor | `Preparing` | Ödemesi onaylanmış fiziksel siparişin firma tarafından hazırlandığı durum. Kargoya verme süresi bu anda başlar (K-168, K-171, K-173). |
| Kargoya verildi | `Shipped` | Takip numarasıyla ya da "kendi aracımızla teslim" beyanıyla yola çıkmış sipariş. Fiziksel kalemin iptal yolu bu anda kapanır ve cayma düğmesi açılır (K-141, K-171, K-196, K-452). |
| Teslim edildi | `Delivered` | **Terminal.** Fiziksel siparişte firmanın panelden koyduğu teslim işareti ve girdiği teslim tarihi; fiziksel kalemi olmayan siparişte bütün kalemlerin teslim işareti — dijital kalemde ödeme onayı, hizmet kaleminde firmanın tamamlama işareti (K-171, K-174, K-175, K-223, K-288, K-294). |
| Teslim edilemedi | `DeliveryFailed` | Kargonun ulaştıramayıp firmaya geri döndürdüğü sipariş. **Terminal değildir** — yeniden gönderilebilir ya da iptal edilebilir (K-176). |
| İptal edildi | `Cancelled` | **Terminal.** Teslimattan önce müşterinin, firmanın, ödeme süresinin dolmasının, kart ödemesinin başarısızlığının ya da aynı sepetten verilen yeni siparişin kapattığı sipariş (K-171, K-180, K-182, K-196, K-199). |

**Ödeme**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Ödeme yöntemi | `PaymentMethod` | Müşterinin ödeme adımında seçtiği yol. MVP'de **iki** yöntem vardır: kart ve havale/EFT. Kapıda ödeme yoktur; taksiti ürün bilmez, sağlayıcının kart ekranında kalır (K-12, K-13, K-159, K-160, K-164). |
| Havale/EFT | `BankTransfer` | Müşterinin firmanın IBAN'ına elle gönderdiği ödeme. Firma panelden açar veya kapatır, açıkken IBAN zorunludur; ödemeyi firma "ödendi" işaretleyerek onaylar ve sistem gelen tutarı sormaz (K-159, K-161, K-169). |
| Ödeme süresi | `PaymentWindow` | Onaylanan siparişin ödenmesi için tanınan süre; stok ayırma bu süre boyunca sürer. Kartta dakikalarla ölçülen bir ürün sabitidir, havalede iş günüyle ölçülen ve alt ve üst çiti olan bir firma ayarıdır (K-128, K-165, K-166, K-337, K-339). |
| Ödeme durumu | `PaymentStatus` | Siparişin ödeme ekseni. Geri ödeme bu eksende yaşar, sevkiyat eksenini değiştirmez (K-170, K-223). |
| Bekliyor | `Pending` | Ödemesi henüz gelmemiş sipariş (K-172). |
| Ödendi | `Paid` | Ödemesi onaylanmış sipariş: kartta sağlayıcının başarı bildirimi, havalede firmanın işareti (K-172, K-173). |
| Başarısız | `Failed` | **Terminal.** Ödemesi alınmadan kapanan sipariş: ödeme süresi dolmuş, kart ödemesi başarısız olmuş ya da sipariş hangi yoldan olursa olsun ödenmeden iptal edilmiş. Ad *"para gelmedi"* demektir, müşterinin hatası demek değildir — sebep siparişin iptal kaydındadır. Başarısız bir siparişe sonradan gelen ödeme işlenmez (K-172, K-180, K-183, K-296). |
| Kısmen geri ödendi | `PartiallyRefunded` | Parasının bir kısmı — kalemlerinin bir kısmının parası ya da tutar bazlı bir kısmi geri ödeme — müşteriye dönmüş sipariş. Terminal değildir (K-222, K-223, K-539). |
| Geri ödendi | `Refunded` | **Terminal.** Parasının tamamı müşteriye dönmüş sipariş (K-172, K-222, K-223). |

**İptal, cayma ve iade**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| İptal | `Cancellation` | Teslimattan **önce** siparişin ya da bir kalemin kapatılması. Müşteri fiziksel kalemi sipariş "Kargoya verildi"ye geçene kadar, hizmet kalemini tamamlanana kadar kendi iptal eder; dijital kalem ödeme onayından sonra iptal edilemez. Firma kapalı bir listeden sebep seçerek iptal eder (K-195, K-196, K-197, K-199, K-200, K-373, K-452). |
| İptal sebebi | `CancellationReason` † | Firmanın iptalde seçmek zorunda olduğu sebebin **kapalı** listesi; ürünle gelir, firma düzenlemez, "diğer"de açıklama zorunludur. Sebep müşteriye bildirilir; değerler §7.2.4'tedir (K-199, K-373, K-451, K-531). |
| Cayma | `Withdrawal` | Tüketicinin sebep göstermeden sözleşmeden dönmesi. Fiziksel kalemde cayma düğmesi sipariş kargoya verildiğinde açılır — ondan önce aynı sonucu veren yol iptaldir — ve pencere teslim tarihinden itibaren 14 gün sonra kapanır; tüketici malı teslim almadan da cayabilir. Dijital üründe ön onayla, hizmette müşterinin onayıyla başlayan ifa tamamlanınca ya da pencere — sipariş tarihinden, fiziksel kalem içeren siparişte teslim tarihinden — 14 gün sonra kapanınca düşer. İstisna işaretli üründe mutlak sebepte hiç yoktur; koşullu sebepte koruyucu ambalajı açılmamış malda sürer (§7.3.4). Sipariş sayfasından ya da başka bir kanaldan — firmanın panelden kaydıyla — bildirilir (K-195, K-203…K-206, K-289, K-340, K-452, K-503, K-509, K-510). |
| Cayma istisnası sebebi | `WithdrawalExemptionReason` † | Fiziksel üründe cayma hakkı istisnası işaretlendiğinde seçilen sebebin **kapalı** listesi; ürün düzleminde yaşar ve ön bilgilendirmede gösterilir. Değerler Mesafeli Sözleşmeler Yönetmeliği m.15/1 bentlerinin metnidir ve mutlak ya da koşulludur; §7.3.4'tedir (K-206, K-509, K-531). |
| Gecikme feshi | `LateDeliveryTermination` † | Otuz günlük yasal teslim sınırı aşıldığında müşterinin sipariş sayfasından sözleşmeyi feshetmesi — sipariş kargoya verilmiş olsa bile. İptalden ve caymadan ayrı bir kayıt tipidir; teslim edilmemiş fiziksel kalemlerin tamamına uygulanır ve cayma istisnası işaretli ürünü de kapsar. Feshedilen kalemlerin bedeli kargo ücreti dahil geri ödenir (§7.2.3; K-499, K-571). |
| İade | `Return` | Cayma ya da ayıp nedeniyle **malın** firmaya geri gönderilmesi: müşteri malı istediği taşıyıcıyla firmanın iade adresine karşı ödemeli gönderir; caymada bunu beyandan itibaren on dört gün içinde yapar. Kargo bedeli firmaya aittir; firma malın ulaştığı tarihi panelde girer ve mal stoğa firmanın kontrolünden sonra döner (K-210, K-211, K-215, K-293, K-491, K-494). |
| Geri ödeme | `Refund` | **Paranın** müşteriye dönmesi — ödemenin geldiği yoldan: kartta karta, havalede müşterinin IBAN'ına. İptalde kart hattının geri ödemesini sistem kendiliğinden başlatır; havale hattını firma panelden işler (K-291). Caymada on dört günlük süre, teslimden önceki caymada ve hizmette beyandan, teslimden sonraki caymada iade edilen malın firmaya ulaştığı tarihten işler (K-491). Kayıttaki "para iadesi" ifadesi bu terimi anlatır; "iade" tek başına yalnız malın dönüşüdür (K-209, K-214, K-491). |
| Ayıp talebi | `DefectClaim` | Müşterinin sipariş sayfasından bildirdiği **sebepli** sorun. Caymadan ayrı bir yoldur; sipariş sayfasındaki kanalı iki yıllık zamanaşımı boyunca açıktır ve süre kalemin teslim işaretinin tarihinden başlar — süre dolunca kanal kapanır, hile ya da ağır kusur hâli iletişim formundan yürür (§7.5.1). İki durumu vardır: Açık, Çözüldü (K-226, K-227, K-228, K-290, K-551). |
| Ayıp talebi durumu | `DefectClaimStatus` | Ayıp talebinin iki değerli durumu. Talebi Çözüldü'ye firma işaretler; çözüm tutmazsa firma ya da müşteri talebi yeniden Açık'a döndürür. Seçimlik hakların yürütümü sistem dışındadır (K-228, K-297). |
| Açık | `Open` | Henüz çözülmemiş ayıp talebi ya da henüz kapatılmamış iletişim talebi; iki talep de bu durumda doğar. Değer iki durum alanında ortaktır (K-228, K-303). |
| Çözüldü | `Resolved` | Firmanın çözüldü olarak işaretlediği ayıp talebi (K-228). |

**Yasal metinler**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Ön Bilgilendirme Formu | `PreInformationForm` | Sipariş onayından önce gösterilen ve **ayrı bir kutuyla** teyit edilen yasal form; içeriği onay özetini birebir taşır ve üzerine yasal bilgileri ekler — teslimat il kısıtı, kargoya verme süresi, cayma koşulları, firma kimliği ve iade bilgileri (§3.24.3); ayarlardan üretilir. Onaylanan sürüm siparişe donar ve sipariş onay e-postasının gövdesinde tam metin olarak gönderilir (K-188, K-191, K-316, K-361, K-493, K-494). |
| Mesafeli Satış Sözleşmesi | `DistanceSalesContract` | Sipariş onayında ayrı bir kutuyla kabul edilen sözleşme; ayarlardan üretilir. Onaylanan sürüm ve firmanın o günkü kimliği siparişe donar; sürüm sipariş onay e-postasının gövdesinde tam metin olarak gönderilir (K-188, K-189, K-190, K-316, K-361). |
| Aydınlatma metni | `PrivacyNotice` | KVKK aydınlatma metni: veri sorumlusu olarak firmanın kimliğini ve kişisel verinin hangi amaçla işlendiğini anlatır. Ürün taslağıyla gelir, firma panelden düzenler; bir **bilgilendirmedir**, onay kutusu yoktur. Siparişe o günkü sürümü donar (K-341, K-342, K-343, K-346, K-347, K-458). |
| Çerez politikası | `CookiePolicy` | Sitenin kullandığı çerezleri ve amaçlarını anlatan metin; altbilgide durur. Ürün taslağıyla gelir, firma panelden düzenler (K-350, K-458). |

**Kurumsal içerik**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Kurumsal içerik | `Content` | Firmanın tanıtım tarafını oluşturan kayıtların ortak adı: Hakkımızda, hizmet tanıtımı, referans iş, sık sorulan soru, şube, genel sayfa ve duyuru. Her tipin düzeni sabittir — sayfa kurucu yoktur, firma yalnız içerik girer. Her kayıt kendi yayın durumunu taşır: **Taslak** ya da **Yayında**; içerikte arşiv ve sürüm geçmişi yoktur (K-237, K-238, K-251, K-253, K-273, K-276). |
| Hakkımızda | `AboutPage` | **Tekil** kayıt: kısa tanıtım, uzun metin ve görseller; ad ya da başlık alanı yoktur. Ana sayfanın kurumsal bloğu kısa tanıtımı ve ana görseli gösterir; Hakkımızda boşken ya da yayında değilken blok marka adını ve logoyu gösterir. Yayına alınması kısa tanıtımı ister. Silinmez, yalnız taslağa alınır (K-239, K-250, K-277, K-584). |
| Hizmet tanıtımı | `Offering` | Firmanın yaptığı bir işin **fiyatsız** tanıtımı: ad, kısa açıklama, metin, görseller ve kendi sayfası. Satın alınmaz; sayfasındaki "Bize ulaşın" düğmesi iletişim formuna götürür. Satılan **Hizmet** ürün tipinden ayrıdır; ikisi içerik–ürün bağıyla birbirine bağlanabilir (K-240, K-245). |
| Referans iş | `PortfolioItem` | Firmanın yaptığı bir işi anlatan kayıt: başlık, kısa açıklama, metin, görseller ve kendi sayfası. Katalogdan ürün bağlayabilir. **Referans fiyat** ile ilgisi yoktur (K-241, K-245). |
| Sık sorulan soru | `FaqItem` | Soru + cevap. Tümü tek bir SSS sayfasında, firmanın verdiği sırayla listelenir; başlıklara bölünmez (K-242, K-247). |
| Şube | `Branch` | Firmanın ziyaret edilebilir bir yeri: ad ve adres zorunlu (il kapalı listeden); telefon, çalışma saatleri, görsel ve harita bağlantısı isteğe bağlı. İletişim sayfasında listelenir. **Teslim noktası değildir** (K-134, K-243). |
| Genel sayfa | `CustomPage` | Hiçbir hazır tipe uymayan içerik için başlık, metin ve görsellerden oluşan, düzeni sabit sayfa; altbilgide listelenir, istenirse menüde gösterilir. **Yasal metinler genel sayfa değildir** (K-237, K-244, K-248). |
| Duyuru | `Announcement` | Sitenin her sayfasının en üstünde görünen tek satırlık metin ve isteğe bağlı bağlantı; aynı anda tek duyuru vardır. Başlangıç ve bitiş tarihi girilirse yalnız o aralıkta görünür (K-269). Kurumsal içerik kaydıdır: Taslak ya da Yayında durumunu taşır (K-273); ad ya da başlık alanı yoktur, yayına alınması metni ister (K-584). Adı, indirimi anlatan "kampanya"dan ayrılmak için Duyuru'dur (K-272). |
| Sosyal medya bağlantısı | `SocialLink` | Firmanın kapalı bir platform listesindeki hesabına giden bağlantı; WhatsApp numarası da bu gruptadır. Firma kimliğinin parçası değildir: zorunlu değildir, siparişe donmaz. Sitede gömülü gönderi akışı yoktur (K-249). |

**Marka kimliği**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Marka kimliği | `Branding` | Firmanın vitrinde ve e-postalarda görünen dört ayarı: marka adı, logo, site simgesi, marka rengi. Yazı tipi, düzen ve diğer renkler üründedir; firmanın siteye kod eklemesi yoktur. Değişiklikleri işlem izine yazılır ama siparişe donmaz (K-258, K-263, K-265, K-266). Altbilgideki "Shopfolio ile kuruldu" platform imzası bu dört ayardan biri değildir, ayrı bir anahtardır (§3.29.6; K-541). |
| Marka adı | `BrandName` | Sitenin üstünde (logo yoksa), tarayıcı sekmesinde ve e-postalarda görünen ad — ör. "Yılmaz Mobilya". Zorunludur; girilene kadar site alan adını gösterir (K-260). |
| Unvan | `LegalName` | Firmanın yasal adı — tüzel kişide ticaret unvanı (ör. "Yılmaz Mobilya San. ve Tic. Ltd. Şti."), gerçek kişi tacirde adını ve soyadını taşıyan ticaret unvanı, esnafta işletme sahibinin adı-soyadı. Yasal bilgilerde ve sözleşmede görünür; sipariş anında siparişe donar (K-14, K-190, K-260, K-480). |
| Logo | `Logo` | Firmanın tek logosu; sitenin her yerinde ve e-postalarda aynısı kullanılır. **Zorunlu değildir** — yoksa marka adı yazıyla gösterilir (K-259). |
| Site simgesi | `Favicon` | Tarayıcı sekmesindeki ikon. İsteğe bağlıdır; yüklenmezse marka adının baş harfi marka rengi üzerinde gösterilir — firmanın sitesinde Shopfolio simgesi çıkmaz (K-261). |
| Marka rengi | `BrandColor` | Firmanın seçtiği tek renk; düğme, bağlantı ve vurgulara uygulanır. Üstündeki yazının rengini sistem seçer; hata, uyarı, "Tükendi" ve indirim gibi anlam taşıyan renkler ondan bağımsızdır (K-258, K-262). |
| Ana sayfa düzeni | `HomepageLayout` † | Ana sayfanın iki hazır düzeninin **kapalı** listesi — tanıtım öncelikli ya da mağaza öncelikli; firma panelden birini seçer. Marka kimliğinin dört ayarından biri değildir (§3.28.1; K-27, K-531). |

**İletişim talebi**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| İletişim talebi | `ContactRequest` | İletişim formundan gönderilen her başvurunun kaydı. Form girişsiz açıktır; talep panele düşer, cevabı firma sistemin dışında e-postayla verir (K-300, K-305). |
| İletişim talebi durumu | `ContactRequestStatus` | İletişim talebinin iki değerli durumu: Açık ve Kapatıldı. KVKK talebi de aynı iki durumu kullanır; ayrı durum makinesi yoktur (K-118, K-303). |
| Kapatıldı | `Closed` | Firmanın panelden kapattığı iletişim talebi. Kapatılan talep yeniden açılabilir (K-303). |
| Konu tipi | `ContactTopic` † | İletişim formunda seçilen başvuru konusunun beş değerli **kapalı** listesi; ürünle gelir, firma düzenlemez. Değerler §3.32.3'tedir (K-302, K-531). |

**Yönetim**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Yönetici daveti | `AdminInvitation` | Mevcut bir yöneticinin panelden bir e-posta adresine gönderdiği, tek kullanımlık ve süreli bağlantı. Davetli bağlantıdan girip şifresini kendisi kurar ve yönetici hesabı o anda doğar (K-308). |
| Düzeltme sebebi | `CorrectionReason` † | Yöneticinin yanlış yapılmış bir durum geçişini düzeltirken seçtiği sebebin **kapalı** listesi; "diğer"de açıklama zorunludur. Değerler §10.4.6'dadır (K-373, K-531). |
| İşlem izi | `AuditLog` | Kimliğe, paraya ve yetkiye dokunan yönetici işlemlerinin kaydı: her satır kimin, ne zaman, neyi değiştirdiğini taşır. **Değiştirilemez ve silinemez**; yanlış bir işlemin düzeltilmesi yeni bir satır üretir. Kapsam §10.3.1'dedir (K-08, K-310, K-384, K-414, K-426, K-527, K-523). |

**Bildirim**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Bildirim | `Notification` | Bir olay üzerine müşteriye ya da firmaya giden e-posta. Tek kanal e-postadır; işlem bildirimleri ticari elektronik ileti değildir ve kapatılamaz (K-315, K-317, K-326, K-377). |

> **Kapsam notu (K-84):** "Ürün" terimi, firmanın kataloğa koyduğu her şeyi kapsar; **sistem ne satıldığını denetlemez.** Ek akış gerektiren ürün türleri — alkol, tütün, ilaç ve reçeteli ürünler, silah — MVP kapsamı dışındadır (`10 §3`). Engelleme mekanizması, yasaklı kategori listesi ve ürün başına mevzuat belgesi alanı **yoktur**; mevzuata uygunluk firmanın yükümlülüğüdür (K-09, K-12, K-14).

> **Ölçek notu (K-89):** `01 §3.1`'deki "fiziksel üründe birkaç yüz ürüne kadar katalog" ifadesi bir **kabuldür** — kural değildir; `01 §8`'de varsayım (V-1), `10 §4`'te kabul olarak yaşar ve hacim kabulleriyle birlikte §11.3'te toplanır (K-03, K-81, K-409). **Tek depo** bu kabulün parçası değildir: ürünün kalıcı sınırıdır (`01 §7` S-5 — K-479). Sistem hiçbir yerde ölçek sınırı uygulamaz: ürün adedi, boyut başına seçenek değeri sayısı ve ürün başına varyant adedi tavansızdır. K-40'ın **iki seçenek boyutu** sınırı bundan ayrıdır ve geçerliliğini korur.

### 1.3 Aktörler

Uygulamada **dört aktör** vardır: ziyaretçi · misafir alıcı · üye müşteri · firma yöneticisi (K-06, K-97, K-100). Adları, kod karşılıkları ve tanımları §1.2'dedir; her aktörün **neden kullandığı ve neden geri döndüğü** `01 §3.2`'dedir ve burada tekrarlanmaz (K-443). Metinde tek başına **müşteri**, sipariş veren alıcıyı — misafir alıcı ya da üye müşteri — birlikte anlatır; bir aktör değil, iki aktörün ortak adıdır (`Buyer`). Tek başına "üye", üye müşterinin kısaltmasıdır (K-526).

Bu dokümanın iş kurallarının dayandığı üç aktör kuralı:

1. **Yönetim tarafı tek roldür, çoklu kullanıcıya açıktır.** Firma birden fazla yönetici hesabı açabilir; hepsi aynı yetkiye sahiptir. Yetki matrisi MVP'de **yoktur**. Ayrı hesaplar korunduğu için "kim ne değiştirdi" izlenebilirliği kaybolmaz (K-06). **Kimlik doğrulama rejimi müşteri tarafıyla aynıdır:** yönetici hesapları da doğrulama kapısına, oturum kurallarına, hassas işlemde yeniden doğrulamaya, e-posta değiştirme kuralına ve şifre politikasına **aynı değerlerle** tabidir; K-122'nin tanıdığı sıkılaştırma yetkisi MVP'de kullanılmaz (K-122, K-313, K-384). Yönetici hesapları davetle açılır ve yönetici hesabıyla vitrinde alışveriş yapılmaz — kendi mağazasından alacak yönetici ayrı bir müşteri hesabı açar; iki hesap birbirine bağlanmaz (K-308, K-312). Tek rolün veri tarafındaki sonucu: her yönetici bütün müşteri verisini görür (K-383). Yönetim tarafının kuralları §10'dadır.
2. **Ürün tüketiciye satış için kurulmuştur.** Ticari/kurumsal alıcıya özel akış yoktur; cayma hakkı, ön bilgilendirme ve mesafeli satış sözleşmesi her siparişte dallanmadan işler (K-07). Alıcının hukuken tüketici sayılıp sayılmadığını mevzuat belirler; ürün alıcıdan ticari bilgi istemez ve ticari amaçla alan biri de aynı akıştan geçer (`01 §3.2` — K-07, K-112).
3. **Sipariş vermek için üyelik zorunlu değildir.** Ziyaretçi hesap açmadan sipariş verebilir ve siparişini **sipariş numarası + e-posta** ile takip eder; üyelik kaldırılmamıştır, yalnız zorunluluğu kalkmıştır (K-97). Üyeliğin taşıdığı değer süreklilikte toplanır — adres defteri (K-111), sipariş geçmişi ve "beni hatırla" (K-108). Doğrulanmış e-postayla hesap açıldığında o e-postaya ait geçmiş misafir siparişleri hesaba düşer (K-98); silinmiş bir hesabın siparişleri **düşmez** (K-117).

**Aktör olmayanlar — bilinçli kararlar:**

- **Platform operatörü uygulama içi aktör değildir.** Kurulum bir deploy işidir; uygulamaya operatör paneli koymak çok kiracılığı arka kapıdan geri getirirdi (K-06). Çok kiracılı SaaS mevcut ürün tanımında yoktur ve yol haritası adayıdır (K-10, K-440). Ürünün sağlayıcısı veri sorumlusu da değildir; kurulum ve içindeki veri firmanındır (K-341, K-410).

*Kaynak: K-17 (sözlük kuralı ve adlandırma konvansiyonu) · K-06 (aktör envanteri) · K-07 (alıcının hukuki sıfatı) · K-455 (A-05 — türetilmiş karşılıkların onayı) · K-458 (iki yasal metin terimi) · K-01 · K-03 · K-09 · K-12 · K-14 · K-39 · K-40 · K-41 · K-42 · K-43 · K-45 · K-50 · K-54 · K-55 · K-56 · K-57 · K-58 · K-59 · K-63 · K-64 · K-65 · K-66 · K-67 · K-70 · K-71 · K-73 · K-74 · K-75 · K-81 · K-82 · K-83 · K-84 · K-85 · K-88 · K-89 · K-90 · K-96 · K-97 · K-98 · K-100 · K-103 · K-104 · K-106 · K-108 · K-111 · K-112 · K-113 · K-115 · K-118 · K-122 · K-182 · K-237 · K-238 · K-239 · K-240 · K-241 · K-242 · K-243 · K-244 · K-245 · K-247 · K-248 · K-249 · K-250 · K-251 · K-253 · K-258 · K-259 · K-260 · K-261 · K-262 · K-263 · K-265 · K-266 · K-269 · K-272 · K-273 · K-274 · K-275 · K-276 · K-277 · K-279 · K-288 · K-289 · K-290 · K-291 · K-293 · K-294 · K-296 · K-297 · K-300 · K-303 · K-305 · K-308 · K-310 · K-312 · K-313 · K-314 · K-315 · K-316 · K-317 · K-326 · K-337 · K-339 · K-340 · K-341 · K-342 · K-343 · K-346 · K-347 · K-350 · K-361 · K-373 · K-377 · K-383 · K-384 · K-410 · K-414 · K-08 · K-11 · K-13 · K-77 · K-78 · K-80 · K-117 · K-123 · K-126 · K-127 · K-128 · K-131 · K-134 · K-141 · K-159 · K-160 · K-161 · K-164 · K-165 · K-166 · K-168 · K-169 · K-170 · K-171 · K-172 · K-173 · K-174 · K-175 · K-176 · K-178 · K-179 · K-180 · K-183 · K-185 · K-186 · K-188 · K-189 · K-190 · K-191 · K-195 · K-196 · K-197 · K-199 · K-200 · K-203 · K-204 · K-205 · K-206 · K-209 · K-210 · K-211 · K-214 · K-215 · K-222 · K-223 · K-224 · K-226 · K-227 · K-228 · K-409 · K-443 · K-452 · K-491 · K-494 · K-79 · K-499 · K-526 · K-527 · K-426 · K-10 · K-440 · K-479 · K-480 · K-27 · K-298 · K-302 · K-451 · K-493 · K-531 · K-541 · K-503 · K-539 · K-508 · K-509 · K-510 · K-511 · K-523 · K-551 · K-561 · K-563 · K-565 · K-538 · K-584.*

---

## 2. Temel akış

> **Ne yazılır:** Ürünün ana iş akışı, uçtan uca. Adım adım, dallanmasız. Detaylı akışlar `03_USER_FLOWS.md`'de.

Ürünün **sekiz akışı** vardır ve dört aktörün üzerine kuruludur: dördü müşteri tarafında, dördü firma tarafında (K-221, K-06, K-97). Satış **doğrudandır** — yayındaki her ürünün fiyatı vardır ve satın alınabilir; teklif ya da talep hattı yoktur (K-11). Üç ürün tipi — fiziksel ürün, dijital ürün, hizmet — aynı kataloğa girer, aynı sepetten geçer ve aynı şekilde satın alınır (K-81). Bu bölüm akışların listesini ve omurgasını taşır; adımların ayrıntısı ve dalları `03`'ün işidir (K-221).

### 2.1 Akış envanteri

| # | Akış | Taraf | Omurga | Kaynak |
|---|---|---|---|---|
| 1 | Satın alma | Müşteri | Vitrin → sepet → ödeme adımı → ödeme → sipariş sayfası | K-221 |
| 2 | Sipariş takibi | Müşteri | Sipariş sayfasına üç yoldan biriyle girilir: üye sipariş geçmişinden, misafir alıcı sipariş numarası + e-postayla, herkes sipariş e-postasındaki bağlantıyla | K-187, K-221 |
| 3 | İptal, cayma ve iade | Müşteri | Fiziksel kalemde iptal kargoya verilene kadar, cayma teslimden on dört gün sonrasına kadar açıktır; kalem tipine göre sınırlar §7'dedir. Sebepli sorun için üçüncü yol ayıp talebidir (§7) | K-195, K-226, K-340, K-452, K-221 |
| 4 | Üyelik | Müşteri | Kayıt → e-posta doğrulama → giriş → hesap yönetimi → hesap silme | K-101, K-221 |
| 5 | Katalog yönetimi | Firma | Ürün, varyant, stok, fiyat, indirim, kupon | K-221 |
| 6 | Sipariş yürütümü | Firma | Havale onayı → hazırlık → kargoya verme → teslim işareti ve tarihi; hizmet tamamlama; iptal; iade teslim alma → geri ödeme; yönetici müdahaleleri (§10) | K-221, K-288, K-369 |
| 7 | Kurumsal içerik | Firma | Tip seçimi → taslak → önizleme → yayın → ana sayfada gösterme → düzenleme → taslağa alma ya da silme; üç kolu marka ayarları, duyuru ve iletişim talebidir (§2.3) | K-285, K-468 |
| 8 | Mağaza ayarları | Firma | Kimlik, ödeme yöntemleri, kargo, eşikler, iade adresi, yasal metinler, yönetici hesapları | K-221, K-293, K-343, K-308, K-468 |

### 2.2 Uçtan uca ana akış

Ana akış, fiziksel bir ürünün vitrinden teslimata kadar yolculuğudur — müşterinin 1. akışı ile firmanın 6. akışının birleşimi. Dijital ürün ve hizmet aynı akıştan geçer, yalnız teslim adımında ayrılır: dijital kalem ödeme onaylandığı anda indirilebilir olur, hizmet kalemi firma "tamamlandı" işaretlediğinde teslim edilmiş olur; fiziksel kalemi olmayan sipariş — fiziksel kalemlerinin tamamı iptal edilmiş karışık sipariş de — kalemlerinin tamamı teslim işaretini aldığında kapanır (K-174, K-175, K-179, K-294, K-295).

1. **Ziyaretçi ürünü bulur ve varyantını seçer** — kategori gezinmesiyle ya da ürün adında aramayla (§3.3–§3.5).
2. **Varyantı adediyle sepete ekler.** Sepet canlıdır; fiyatı ve satın alınabilirliği güncel üründen okur (§3.16).
3. **Ödeme adımına geçer.** Üye adres defterinden seçer, misafir adresini ve e-postasını yazar; teslimat ve fatura adresi, kupon ve ödeme yöntemi burada girilir (§3.10, §3.14, §3.21).
4. **Onay özetini ve Ön Bilgilendirme Formu'nu görür, iki onay kutusunu işaretler ve siparişi onaylar** — onay düğmesinin yanında siparişin ödeme yükümlülüğü doğurduğu yazar (§3.24; K-544).
5. **Sipariş oluşur.** Sepet onay anında yeniden değerlendirilir; özet değişmemişse sipariş numarası verilir, kalemler, adresler ve yasal metin sürümleri donar, stok ayrılır. Sipariş **Alındı**, ödeme **Bekliyor** durumundadır. Müşteriye iki yasal metni gövdesinde taşıyan sipariş e-postası gider (§3.17, §3.23, §5.3, §9; K-316).
6. **Müşteri öder ve ödeme onaylanır** — kartta 3D Secure'dan geçen ödemenin sağlayıcıdan gelen başarı bildirimiyle, havale/EFT'de firmanın panelden "ödendi" işaretiyle. Ödeme **Ödendi**, sipariş **Hazırlanıyor** olur; ayrılan stok kesin düşer, siparişe giren kalemler sepetten çıkar ve kargoya verme süresi başlar (§3.16.12, §3.17, §3.21, §5.6; K-159, K-173).
7. **Firma siparişi hazırlar ve kargoya verir** — kargo şirketi ile takip numarası ya da "kendi aracımızla teslim" beyanı girilir; sipariş **Kargoya verildi** olur ve müşteriye takip bilgisiyle e-posta gider (§3.20, §9).
8. **Mal müşteriye ulaşır; firma teslimi panelden işaretler ve teslim tarihini girer** — tarih geçmişe dönük olabilir, ileri tarihli olamaz. Sipariş **Teslim edildi** olur; bu tarih cayma penceresinin ve ayıp talebi süresinin başlangıcıdır. Otomatik geçiş yoktur ve teslim işareti bildirim üretmez (§3.20.11, §5.4; K-288, K-317).

Müşteri her adımı sipariş sayfasından izler (akış 2); hiçbir adım bir e-postanın ulaşmasına bağlı değildir (§6.1.1).

### 2.3 Kurumsal içerik akışı

Yedinci akış şu adımlarla yürür (K-285):

1. Firma içerik tipini seçer.
2. Kayıt **taslak** olarak açılır; yalnız adla ya da başlıkla kaydedilebilir.
3. Yönetici sayfayı "Taslak" bandıyla önizler.
4. Zorunlu alanlar dolunca kayıt yayına alınır.
5. Kayıt isteğe bağlı olarak ana sayfada gösterilir; genel sayfa menüde gösterilebilir.
6. Yayındaki kaydın düzenlemesi, kaydedildiği anda yayındadır.
7. Kayıt taslağa alınır ya da silinir.

Akışın üç kolu vardır: **marka ayarları** (§3.29), **duyuru** (§3.27.21) ve **iletişim talebi** (§3.32) — ziyaretçi İletişim sayfasından ya da bir hizmet tanıtımının "Bize ulaşın" düğmesinden formu gönderir (K-240, K-243, K-300), talep panele düşer ve firma cevabı e-postayla verip talebi kapatır (K-305, K-468). Kuralları §3.27–§3.29 ve §3.32'dedir.

*Kaynak: K-221 (temel akış omurgası) · K-285 (yedinci akışın adımları) · K-468 (Blok 8'in işlerinin akışlardaki yeri) · K-11 (doğrudan satış) · K-159 (iki ödeme yolu) · K-81 · K-06 · K-97 · K-101 · K-173 · K-174 · K-175 · K-179 · K-187 · K-195 · K-226 · K-240 · K-243 · K-288 · K-293 · K-294 · K-300 · K-305 · K-308 · K-316 · K-317 · K-340 · K-343 · K-369 · K-295 · K-452 · K-544.*

## 3. İş kuralları

> **Ne yazılır:** Her kural ayrı madde. Kuralın **koşulu**, **sonucu** ve **istisnası**. Her kural bir sonraki dokümanda karşılığını bulacak şekilde somut olmalı.

**Kuralların biçimi.** Her kural bir numara (`§3.6.2`) ve kalın bir başlık cümlesi taşır; başlığı izleyen cümleler kuralın koşulunu, sonucunu ve — varsa — **istisnasını** yazar. Kaynak karar satırları parantez içindedir.

- **Ayarlanabilir parametrelerin değerleri burada yazılmaz.** "Değeri §11'dedir" diyen kuralın değeri, katmanı ve — varsa — çiti §11'in parametre envanterindedir (K-411, K-412). Ürünün yapısını tanımlayan sınırlar (P-20, P-21) ve yasal süreler kuralın kendisidir; burada yazılır, değerlerini §11 ve §4 de taşır (K-533).
- **Başka bölümde yaşayan kurallar tekrarlanmaz:** süreler ve sayım kuralları §4'te, durum makineleri §5'te, hata ve istisna senaryolarının envanteri §6'da, iptal, cayma, iade ve ayıp talebi §7'de, kötüye kullanım limitleri ve güvenlik kuralları §8'de, bildirim matrisi §9'da, yönetim tarafının kuralları — yönetici hesapları, işlem izi, sipariş müdahaleleri, manuel adım bütçesi, satış özeti ve dışa aktarma — §10'da, parametreler §11'de, kişisel veri, saklama ve diğer yasal yükümlülükler §12'dedir. Bu bölümde yalnız işaretleri kalır.
- **Kapsamın sınırları başka dokümanlardadır:** ürünün kalıcı sınırları `01 §7`'de, MVP başarı kriterleri `01 §6`'da, MVP kapsamı dışında kalan kalemler ve post-MVP aday listesi `10 §3` ve `§5`'tedir. Bu bölüm kapsam dışı bir özelliği yalnız bir kuralın sınırını çizdiği yerde anar. Başarı kriterleri, ürün varsayımları ve post-MVP sıralaması bu bölüme kural eklemez; o dokümanların işidir (K-415, K-418…K-424, K-433…K-435).
- Bir kuralın biçimini, metnini ya da ekrandaki yerini `04`'e bırakan ifade, **arayüz** kararının o dokümanda alınacağını söyler; kuralın kendisi burada tamdır.

| Gruplar | Alan |
|---|---|
| §3.1–§3.2 | Firma, satış kapısı ve satış modeli |
| §3.3–§3.12 | Katalog |
| §3.13–§3.15 | Hesap |
| §3.16–§3.26 | Sepet, sipariş, ödeme ve teslim |
| §3.27–§3.30 | Kurumsal içerik, marka ve site |
| §3.31 | Panel |
| §3.32–§3.34 | İletişim talebi, yasal metinler, site geneli |

### 3.1 Firma, kurulum ve satış kapısı

**3.1.1 Uygulama kurulum başına tek bir firmaya hizmet eder.** Firma ekleme, firma seçme ve firmalar arası geçiş yoktur; ikinci firma ikinci kurulumdur. Firmanın kimlik kaydı silinemez, yalnız düzenlenir (K-01, K-02, K-09).

**3.1.2 Firmanın kimliği panelden yönetilir; altyapısı kurulumdan gelir.** Panelden: firma tipi ve tipin zorunlu kimlik alanları (§3.1.3), adres, iletişim bilgileri, ETBİS doğrulama bilgisi (§3.1.4), logo ve marka ayarları. Kurulumdan: alan adı, e-posta gönderim kimliği, ödeme sağlayıcı anahtarları ve Google uygulamasının kimlik bilgileri — yani sır ve altyapı (K-478). Kimlik alanları zorunludur ve boşaltılamaz; her değişiklik kimin ve ne zaman yaptığıyla işlem izine yazılır (K-08, K-310). **Kalan risk bilinçlidir:** zorunluluk alanın boş bırakılmasını engeller, yanlış yazılmasını engellemez; kimliğin doğruluğu firmanın sorumluluğudur (K-08, K-445). **İstisna:** logo zorunlu değildir (§3.29.2, K-259).

**3.1.3 Firma tipi seçilir ve zorunlu kimlik alanları tipe göre değişir.** Üç tip vardır. **Esnaf** (esnaf ve sanatkâr siciline kayıtlı gerçek kişi): ad-soyad ve vergi kimlik numarası. **Gerçek kişi tacir** (ticaret siciline kayıtlı gerçek kişi): adını ve soyadını taşıyan ticaret unvanı, MERSİS numarası ve ticaret sicil numarası. **Tüzel kişi:** ticaret unvanı, MERSİS numarası ve ticaret sicil numarası. Üç tipte de adres, telefon ve e-posta zorunludur. Üç tipte de isteğe bağlı bir **meslek odası** alanı vardır (esnaf odası ya da ticaret odası); doluysa yasal bilgilerde ve Ön Bilgilendirme Formu'nda görünür, satış kapısını tutmaz (K-547). Tip sonradan değiştirilebilir; değiştiğinde satış kapısı (§3.1.5) yeni tipin setini denetler — kapı yalnız kurulum anında değil **sürekli** işler (K-14, K-480).

**3.1.4 Yasal kimlik bilgileri sitede sürekli erişilebilir durur;** yerleşimi `04`'ün işidir. ETBİS kaydı firmanın yükümlülüğüdür: ürün kayıt yapmaz, kayıt bilgisini taşır ve doğrulama bandını gösterir (K-14). **Kayıt bilgisi firma kimliğinin isteğe bağlı "ETBİS doğrulama bilgisi" alanıdır:** firma kaydı yaptırdıktan sonra ETBİS'in verdiği doğrulama bağlantısını panelden girer. Alan doluysa doğrulama bandı yasal kimlik bilgilerinin yanında sitenin her sayfasında görünür ve ziyaretçiyi doğrulama sayfasına götürür; boşsa band hiç görünmez — ürün kaydı olmayan firmayı kayıtlı göstermez. Alan satış kapısını (§3.1.5) ve kurulum kontrol listesini (§10.8.2) tutmaz: kayıt yükümlülüğü her firmada doğmaz ve ürün kaydın varlığını denetleyemez; panel alanın yanında yükümlülüğü bir cümleyle hatırlatır. Alanın değişikliği firma kimliğinin değişikliği gibi işlem izine yazılır (§10.3.1; K-574).

**3.1.5 Satış yalnız dört koşul birlikte sağlandığında açıktır:** (1) seçilen firma tipinin zorunlu kimlik alanları doludur (K-09, K-14); (2) en az bir ödeme yöntemi açıktır (K-163); (3) "satışı geçici olarak kapat" anahtarı kapalıdır (K-235); (4) dört yasal metin tamamlanmıştır — firmanın düzenlediği aydınlatma metni tamamlanmış ve yayına alınmıştır (aşağıdaki veri toplayan girişlerin kapısıyla aynı tanım), çerez politikası yayına alınmıştır; ayarlardan üretilen Ön Bilgilendirme Formu ile Mesafeli Satış Sözleşmesi'nin besleyen ayarlarından firmanın girmesi gereken iade adresi girilmiştir (K-365, K-360, K-293, K-465, K-576). Koşullardan biri sağlanmıyorsa sepete ekleme ve ödeme kapalıdır; vitrin ve kurumsal içerik yayında kalır, ürünler görünür ve ziyaretçi satışın kapalı olduğunu görür. Dört koşul aynı kapı mekanizmasıdır ve panel karşılanmayan koşulu adıyla gösterir (K-09, K-235, K-365, K-465). Kurumsal taraf kapıya bağlı değildir — kimlik eksikken de yayınlanabilir (K-09). **Veri toplayan girişlerin ayrı bir kapısı vardır:** aydınlatma metni tamamlanmış ve yayına alınmış değilse — barındırma konumu bölümü ve firma kimliği alanları dolu değilse — iletişim formu, hesap kaydı ve Google ile ilk giriş kapalıdır (§3.13.19, §3.32.8; K-517). Bu koşul `10 §4.1` ÖK-10 ile tek koşuldur. **Satış kapısı bu kapıdan gevşek olamaz:** sipariş onay adımı da — misafir siparişi dahil — kişisel veri toplar ve aydınlatma bağlantısını gösterir (§3.33.5); bu yüzden satışın dördüncü koşulu aydınlatma için aynı tanımı kullanır (K-576). Alıcı gruplarının adıyla doldurulması kapıya girmez: taslağın alıcı grupları bölümü grupları kategori olarak zaten anar ve KVKK m.10/1-c ile Aydınlatma Tebliği alıcı grubunu kategori olarak ister; grupların adıyla doldurulması firmanın sorumluluğudur (§12.5; K-553, K-576).

**3.1.6 Firma satışı panelden geçici olarak kapatabilir.** Anahtar açıkken açık siparişler etkilenmez — firma onları yürütmeye devam eder, havale onayı dahil. Müşterinin sepeti korunur ve içeriği görünür; yalnız ödeme adımına geçilemez. **Bakım modu — sitenin tamamen kapanması — yoktur** (K-235). İlan edilen planlı bakım penceresi de yoktur; satışı durdurmak isteyen firma bu anahtarı kullanır (K-408; §3.34.7).

**3.1.7 Firmanın iade adresi zorunlu bir firma ayarıdır.** Cayma ve ayıp talebinde müşterinin malı göndereceği adrestir; beyan ekranında müşteriye gösterilir ve üretilen yasal metinleri besler (K-293, K-362, K-413). Firma kimliğindeki adresten ayrı bir alandır.

### 3.2 Satış modeli ve ürün tipleri

**3.2.1 Satış yalnız doğrudandır.** Yayındaki her ürünün fiyatı vardır ve satın alınabilir; "fiyat sorunuz" diye bir ürün durumu, teklif ya da talep hattı yoktur. Fiyat sorusu olan ziyaretçi iletişim formunu (§3.32) kullanır; bu ticari bir hat değildir. Randevu ve rezervasyon modeli yoktur (K-11).

**3.2.2 Katalog üç tipte ürün taşır: fiziksel ürün, dijital ürün ve hizmet.** Fiziksel ürün kargoyla teslim edilen maldır; dijital ürün indirilebilir dosyadır; hizmet sabit fiyatlı ve randevusuzdur. Üçü de aynı kataloğa girer, aynı sepetten geçer ve aynı şekilde satın alınır (K-81). Hizmet ürünü zorunlu bir **ifa süresi** taşır: ödeme onayından itibaren hizmetin en geç kaç gün içinde ifa edileceği. Süre ürün formunda gün olarak girilir, vitrinde ve Ön Bilgilendirme Formu'nda yazar; girilmeden hizmet ürünü yayına alınamaz (§3.7.4). Çiti §11'de, süre kuralı §4'tedir (P-44, Z-39; K-503).

**3.2.3 Tip ürün düzleminde yaşar ve üç değerli kapalı bir alandır.** Ürünün bütün varyantları aynı tiptedir; varyant tipi devralır, kendi tipini taşımaz (K-82).

Tipe göre ayrılan kurallar kendi gruplarındadır: stok (§3.6.2), dijital ürün (§3.12), teslimat adresi (§3.14.4), asgari sipariş tutarı (§3.18.3), kargo ücreti (§3.19.1), teslim yolu (§3.20.9), iptal ve cayma (§7).

### 3.3 Ürün, varyant ve seçenek

**3.3.1 Satılan birim varyanttır; ürün vitrin ve katalog birimidir.** Stok ve fiyat varyantta yaşar. Seçeneği olmayan ürünün de tek bir varyantı vardır ve ziyaretçi bunu fark etmez. Ziyaretçi vitrinde ürün başına **tek kart** görür ve varyant seçimini kartın içinde yapar (K-39).

**3.3.2 Bir ürün en fazla iki seçenek boyutu taşır** (ör. renk × beden); üç ve üzeri boyut yoktur (K-40).

**3.3.3 İki boyutun birleşim matrisinin tam olması gerekmez.** Firma yalnız gerçekten var olan kombinasyonları varyant olarak açar; var olmayan kombinasyon vitrinde görünür ama seçilemez — tükenmiş varyantın kalıbıyla aynı, sebebi farklıdır (K-87, K-51).

**3.3.4 Her varyant zorunlu ve tekil bir stok kodu (`SKU`) taşır;** tip ayrımı yoktur — dijital ve hizmet varyantı da kod taşır. Kodun panelde önceden doldurulması ve düzenlenebilirliği `04`'ün işidir (K-88).

**3.3.5 Ürün başına asgari adet yoktur.** Asgari adetle satmak isteyen firma ürünü paket olarak tanımlar ("10'lu paket" varyantı); paket kendi fiyatını ve kendi stoğunu taşır (K-158).

**3.3.6 Ürün sayfasında yorum, puan ve öneri bloğu yoktur.** Müşteri yorum yazamaz; puan ve yıldız gösterilmez (K-385). "Bunu alanlar şunu da aldı", "benzer ürünler" ve kişiselleştirilmiş öneri blokları yoktur; içerik sayfasındaki "İlgili ürünler" bloğu firmanın elle kurduğu bağdır, öneri motoru değildir (§3.27.10; K-387).

### 3.4 Kategori

**3.4.1 Kategori ağacı en fazla üç seviyedir** (ör. `Elektrik → Kablo → NYA Kablo`). Kategori üründe yaşar, varyantta değil (K-42).

**3.4.2 Ürün ağacın herhangi bir düğümüne asılabilir;** yaprak zorunluluğu yoktur. Bir kategori sayfası hem alt kategorilerini hem kendisine doğrudan asılı ürünleri taşıyabilir (K-43).

**3.4.3 Kategori sayfası alt dallarındaki ürünleri de gösterir.** Kendisine doğrudan asılı ürünler ile bütün alt dallarındakiler tek listede toplanır; ürün sayacı, sayfalama ve süzgeçler aynı kapsam üzerinden çalışır (K-44).

**3.4.4 Bir ürün birden fazla kategoriye asılabilir; asıldığı kategorilerden biri ana kategoridir.** Ürün asıldığı her kategorinin sayfasında görünür (§3.4.3). Kırıntı yolu ana kategoriden üretilir. Ana kategori, yayına çıkacak ürün için zorunludur (§3.7.3). Ürünün adresi kategoriden bağımsızdır (§3.30.1) (K-45, K-279).

**3.4.5 İçinde ürün ya da alt kategori bulunan kategori silinemez.** Panel silmeyi engeller ve neyin engellediğini sayısıyla gösterir; firma önce ürünleri ve alt dalları taşır (K-46).

**3.4.6 Kategori taşımak serbesttir, iki sınırla:** taşıma sonunda üç seviyeyi aşan bir dal oluşamaz ve bir kategori kendi alt ağacına taşınamaz (K-46, K-42).

Boş kategorinin menüdeki görünürlüğü §3.28.6'dadır.

### 3.5 Arama, süzgeç ve liste düzeni

**3.5.1 Site içi arama yalnız ürün adında çalışır.** Açıklama, kategori adı ve seçenek değerleri aranmaz. Eşleşme büyük-küçük harf ve Türkçe karakter duyarsızdır (`AMPUL` = `ampul`, `isıtıcı` = `ısıtıcı`) (K-47).

**3.5.2 Ürün listeleri iki sabit eksende süzülür: fiyat aralığı ve stok durumu.** Seçenek değerine (renk, beden) göre süzme yoktur. Ürün düzleminde "stokta var", en az bir varyantın satın alınabilir olması demektir (K-48, K-50). Stok süzgecinin **varsayılanı tükenmiş ürünleri de gösterir** (K-52).

**3.5.3 Ziyaretçiye sıralama seçeneği sunulmaz.** Kategori sayfası ve arama sonucu tek bir sabit düzende gelir: **en yeni önce**. Firmanın ürünleri elle sıralaması yoktur (K-49). Kurumsal içeriğin elle sıralanması (§3.27.11) bu kuralı değiştirmez.

### 3.6 Stok ve tükenme

**3.6.1 Stok, varyantın sayısal adedidir.** Firma her varyant için adet girer. Ürün düzleminde stok alanı yoktur; ürünün stok durumu varyantlarından türetilir (K-50). Adedin ayrıldığı ve düştüğü anlar §3.17.3'tedir.

**3.6.2 Stok takibi tipe göre değişir.** Fiziksel üründe sayısal stok **zorunludur**. Dijital üründe stok alanı **yoktur** ve sınırsız satılır. Hizmette firma isterse toplam adet **kontenjanı** girer; kontenjan dolunca ürün "Tükendi" davranışını gösterir (§3.6.3, §3.6.4), girilmezse sınırsızdır (K-83). Hizmet kontenjanı ayırma, adet ve sepet kurallarında fiziksel stokla birebir aynı işler (K-128, K-150).

**3.6.3 Tükenmiş varyant seçenek listesinde "Tükendi" işaretiyle görünür ve seçilemez;** gizlenmez (K-51). Satın alınabilirlik ayrılmış adetler düşülerek hesaplanır: başka bir müşterinin ödemesi beklenirken — kartta dakikalar, havalede iş günleri — son parça "Tükendi" görünebilir ve ödeme gerçekleşmezse geri gelir (K-128, K-165).

**3.6.4 Bütün varyantları tükenmiş ürün vitrinde kalır,** "Tükendi" olarak işaretlenir ve sepete eklenemez (K-52).

**3.6.5 Stok adedi ziyaretçiye hiçbir yerde gösterilmez.** Ziyaretçi yalnız iki durum görür: satın alınabilir ya da "Tükendi". "Son 2 adet" gibi eşik uyarısı yoktur (K-53). Sepete stoktan fazla adet eklenmek istendiğinde de adet söylenmez (§3.16.8).

**3.6.6 İptal edilen kalemin adedi stoğa kendiliğinden döner; iade edilen fiziksel kalemi firma kontrol edip panelden ekler** (§7.2.6, §7.4.7; K-201, K-215).

### 3.7 Ürünün yayın durumu, arşiv ve silme

**3.7.1 Ürün ve varyant yayın durumunu ayrı ayrı taşır: Taslak, Yayında, Arşiv.** Üç durum arasındaki her geçiş serbesttir ve firmanın panelden yaptığı bir işlemdir; tanımlar ve geçişler §5.1'dedir. Zamanlanmış (ileri tarihli) yayın yoktur. Yayın durumu değişiklikleri işlem izine yazılır (K-54, K-55, K-298, K-310).

**3.7.2 Arşiv silme değildir.** Arşivlenen ürünün kaydı korunur, yalnız vitrinden çıkar (K-54).

**3.7.3 Bir ürün yayına ancak adı, ana kategorisi ve tipi girilmişse ve en az bir Yayında varyantı varsa alınır.** K-55'in "arşivlenmemiş en az bir varyant" koşulu gerekçesine göre okunur: taslak varyant ziyaretçiye görünmez (§3.7.5) ve koşulu karşılamaz — yoksa vitrinde satılacak hiçbir şeyi olmayan boş bir kart oluşurdu (K-538). Açıklama ve görsel zorunlu değildir; KDV oranının varsayılanı ayardan geldiği için kapıyı ayrıca tutmaz (K-86, K-45, K-55, K-58, K-538). Yayındaki her varyantın fiyatı vardır (K-11, K-39).

**3.7.4 Dijital ürünün yayın kapısı bir koşul daha taşır:** yayındaki her varyantı bir dosyaya — kendi dosyasına ya da ürününkine — bağlı olmalıdır; panel dosyası eksik varyantı gösterir. Yayındaki bir dijital ürünün herhangi bir varyantını dosyasız bırakacak dosya silme işlemi **engellenir**; firma önce yeni dosyayı yükler ya da o varyantı yayından çeker. Hizmet ürününün kapısı ifa süresini de ister (§3.2.2; K-503); fiziksel ürünün kapısı değişmez (K-145, K-144).

**3.7.5 Taslak ürünün adresi ziyaretçiye "sayfa bulunamadı" döner; giriş yapmış firma yöneticisine ürün sayfasını "Taslak" bandıyla gösterir.** Taslak varyant, yayındaki bir ürünün seçenek listesinde ziyaretçiye görünmez, yalnız yönetici önizlemesinde görünür — tükenmiş varyantın görünür kalma kuralı (§3.6.3) yalnız stok durumu içindir (K-57, K-281).

**3.7.6 Arşivlenmiş ürünün adresi çalışmaya devam eder ve "Bu ürün artık satılmıyor" sayfası döner:** ürün adı, görseli ve durum bilgisi görünür; **fiyat ve sepete ekleme yoktur**. Ürün listelerde, kategori sayfalarında ve aramada görünmez; yalnız doğrudan adresle açılır (K-56, K-69). Arama motorlarına kapalıdır (§3.30.6).

**3.7.7 Yönetici ürünü kalıcı olarak silebilir.** Silinen ürün katalogdan tamamen kalkar ve adresi "sayfa bulunamadı" döner. Arşiv ile silme iki ayrı yoldur. Geçmiş siparişler silmeden etkilenmez — kalem kendi donmuş bilgisini taşır (K-85, K-77); dijital üründe dosyanın son hâli bu siparişler için saklanır (§3.12.7).

**3.7.8 Yayındaki bir ürünün son Yayında varyantı arşivlenemez ve taslağa çekilemez.** Panel işlemi durdurur ve firmayı önce ürünü taslağa ya da arşive almaya yönlendirir; yayın kapısının (§3.7.3) varyant koşulu böylece hiçbir anda bozulmaz (K-299, K-538). Engelleme kalıbı dolu kategori (§3.4.5) ve son dosyayla (§3.7.4) aynıdır.

### 3.8 Fiyat ve KDV

**3.8.1 Fiyat varyantta yaşar ve KDV dahil girilir.** Yöneticinin yazdığı sayı ile ziyaretçinin gördüğü sayı birebir aynıdır; matrah ve KDV tutarı bu sayıdan geriye hesaplanır. Tüketiciye gösterilen fiyatın KDV dahil olması bir seçenek değil yasal kısıttır (K-39, K-58, K-60).

**3.8.2 KDV oranı ürün düzleminde tutulur ve varyantlar devralır;** varsayılanı ayardan gelir. Matrah ve KDV tutarı sipariş kaydında ayrışır (K-58, K-59). Kargo ücretinin oranı ayrı bir ayar değildir; siparişteki fiziksel kalemlerin oranını izler (§3.19.2; K-500).

**3.8.3 KDV kalem bazında hesaplanır ve kuruşa orada yuvarlanır.** Siparişin toplamı yuvarlanmış kalemlerin toplamıdır; müşterinin ödediği toplam KDV dahil kalem tutarlarının toplamıdır ve matrah + KDV'den yeniden türetilmez (K-61).

**3.8.4 Fiyatın hassasiyeti kuruştur — iki ondalık;** panel daha fazlasını kabul etmez (K-62).

**3.8.5 Ürün sayfası fiyat etiketinin zorunlu bilgilerini taşır.** Ürün bir **"üretim yeri (ülke)"** alanı taşır; fiziksel üründe zorunludur, dijital üründe isteğe bağlıdır ve ürün sayfasında görünür. Ölçüyle satılan ürün için isteğe bağlı **"ölçü birimi + net miktar"** alanı vardır; doluysa sistem birim fiyatı (KDV dahil, ölçü biriminin net miktarına göre) türetir ve satış fiyatının yanında gösterir — birim fiyat satış fiyatıyla aynıysa gösterilmez. **Alanın isteğe bağlı olması bir muafiyet değildir:** hangi ürünün kilo, litre ya da metreyle satıldığını sistem ürün kaydından bilemez; ölçüyle satılan üründe alanı doldurmak firmanın yükümlülüğüdür. Panel alanın yanında bu yükümlülüğü bir cümleyle hatırlatır; alan boşken ürün yayına alınabilir ve sonucu firmadadır (K-573). **Fiyatın uygulanmaya başlandığı tarih** varyantın fiyat geçmişinden (§3.9.3) okunur ve ürün sayfasında gösterilir. Yerli üretim logosu firmanın görsel içeriğidir: logo kullanılacaksa firma onu ürün görseline koyar, ayrı bir alan yoktur. Hizmette ayırıcı özellik, fiyat ve fiyatın başlangıç tarihi yeterlidir. **Kalan risk:** Fiyat Etiketi Yönetmeliği m.5'in çevrim içi ürün sayfasına uygulanışı Bakanlık pratiğidir; ürün tüketici lehine okumayı seçer (K-511).

### 3.9 İndirim ve referans fiyat

**3.9.1 İndirim ürün düzleminde yüzde olarak tanımlanır ve bütün varyantlara iner;** her varyant kendi fiyatı üzerinden indirilir (K-64, K-65).

**3.9.2 İndirim tarihlidir.** Başlangıç ve bitiş girilir; sistem indirimi kendisi başlatır ve bitirir (K-66). İndirim beyanının göründüğü her yerde — ürün sayfası ve vitrin kartı — indirimin başlangıç ve bitiş tarihi de gösterilir; biçimi `04`'ün işidir (K-545).

**3.9.3 İndirim beyanı referans fiyata dayanır ve referansı sistem hesaplar;** yönetici giremez. Sistem her varyantın fiyat geçmişini tutar. Fiyat Etiketi Yönetmeliği m.11/1'in güncel metnini — RG 11/10/2025-33044 ile değişik; değişiklikten önceki metnin otuz günü bu tarihte on güne indi — izler: **fiziksel ve dijital üründe** indirimin başladığı andan önceki **10 gün** içinde o varyanta uygulanmış **en düşük** fiyattır; **hizmette ve cayma istisnası sebebi "çabuk bozulan" olan üründe** (§7.3.4) indirimli fiyattan **hemen önceki** fiyattır. İndirimli fiyat da uygulanmış bir fiyattır ve geçmişe yazılır — art arda kampanyalarda referans kademeli düşer. Çabuk bozulan ürünü istisna sebebiyle işaretlemeyen firmanın ürünü mal kuralını izler; işaretleme firmanın sorumluluğudur (K-63, K-64, K-508).

**3.9.4 10 günlük geçmişi olmayan malda referans,** o varyantın satışa sunulduğu andan — kendisinin ve ürününün birlikte Yayında olduğu ilk andan — beri uygulanmış en düşük fiyattır. Ürün yayındayken sonradan eklenen varyantın geçmişi kendi satışa sunuluşuyla başlar; ürünün diğer varyantlarının fiyatı ona referans olmaz (K-67, K-508, K-563).

**3.9.5 İndirimli fiyat referansın altına inmiyorsa sistem indirimi kaydetmez** ve yöneticiyi sebebiyle birlikte uyarır; referans §3.9.3'ün ürün tipine göre kuralıdır (K-68, K-508).

**3.9.6 Beyan edilen referans fiyat sipariş kalemine donar** (§3.23.1, K-79).

### 3.10 Kupon

**3.10.1 Kupon, ödeme adımında girilen ve sepet toplamına inen bir koddur.** Kupon indirimli fiyattan iner ve indirimli ya da indirimsiz her kaleme aynı biçimde uygulanır — kalem ayrımı yoktur; payı kalemlere dağıtılır (§3.10.3). Örnek: %20 indirimli 960 TL'ye %10 kupon 864 TL verir (K-70, K-74, K-77).

**3.10.2 Kupon yüzde ya da sabit tutar olarak tanımlanır** (K-71).

**3.10.3 Sabit tutarlı kupon kalemlere tutarlarıyla orantılı dağıtılır;** bölünmeden kalan kuruş en büyük kaleme yazılır. Her kalemin kupon payı kaleme donar (K-72, K-77).

**3.10.4 Kupon siparişi sıfır tutara indiremez: sepet toplamına eşit ya da onu aşan kupon reddedilir.** Kuponlu kalem toplamı her zaman sıfırın üstünde kalır; böylece her sipariş bir ödemeden geçer ve 0 TL'lik sipariş doğmaz. Sabit tutarlı kupon zorunlu bir **asgari sepet tutarı** taşır ve bu tutar kupon tutarından büyük olmalıdır — eşit ya da altında girilemez. Yüzdesel kupon %100'e ulaşamaz ve asgari tutarı isteğe bağlıdır; yuvarlama kuponlu toplamı sıfıra indirirse kupon yine reddedilir (K-73, K-565). Asgari sepet tutarı yalnız siparişe giren kalemlere bakar (K-130).

**3.10.5 Kuponun sınırı tarih aralığı ve toplam kullanım adedidir.** Tarih aralığının dışında ya da kullanım hakkı dolmuş kupon kabul edilmez. Kişiye özel kod, toplam kullanım adedi **1** olan koddur (K-75). Art arda geçersiz kod denemesi limitlidir; eşik aşıldığında o sepetin kupon alanı geçici olarak kapanır (§8; K-334).

**3.10.6 Kuponun kullanım hakkı stok gibi ayrılır:** sipariş onaylandığında ayrılır, ödeme başarılı olduğunda **kullanılmış** sayılır, ödeme süresi dolar ya da ödeme başarısız olursa geri döner — son kullanım hakkını iki müşterinin birden alması böylece kapanır (K-76, K-128). İptal ve iadede hakkın dönüşü §7'dedir (K-201, K-202, K-216).

**3.10.7 Kupon, ücretsiz kargo eşiğinin ve asgari sipariş tutarının tabanını düşürmez;** kupon ile ücretsiz kargo aynı siparişte birlikte geçerlidir (§3.19.4, §3.18.2; K-136, K-156).

### 3.11 Ürün görseli ve metni

**3.11.1 Görsel ürün düzleminde yaşar;** varyant isterse kendi görselini taşır, taşımıyorsa ürününkini devralır (K-90).

**3.11.2 Ayrı bir "ana görsel" işareti vardır ve galeri sırasından bağımsızdır;** vitrin kartında ve listede işaretli görsel çıkar (K-91). Galeri sırasını firma panelde elle belirler (K-94).

**3.11.3 Görsele üç kısıt uygulanır:** kabul edilen format listesi, tek dosya için boyut tavanı ve ürün başına görsel adedi tavanı; değerleri §11'dedir (K-92).

**3.11.4 Görselin alternatif metni isteğe bağlıdır ve geri düşüşlüdür:** firma yazarsa onun metni kullanılır; yazmazsa sistem bir metin üretir ve çıktı hiçbir zaman boş kalmaz. **Üretilen metin aynı ürünün görsellerini birbirinden ayırır:** varyanta bağlı görselde ürün adı ve varyantın seçenek değerleri ("Pamuk Tişört — Kırmızı / M"), ürünün galerisindeki görselde ürün adı ve görselin galerideki sırası ("Pamuk Tişört — görsel 2 / 5"); vitrin kartında ve listede tek görsel çıktığı için yalnız ürün adı kullanılır. Alan bilinçli olarak zorunlu değildir: zorunlu alan çok görselli üründe "resim1, resim2" gibi ürün adından daha kötü metinler üretir (K-93, K-564).

**3.11.5 Ürün adına karakter tavanı konur; açıklama serbesttir.** Tavanın değeri §11'dedir (K-95).

**3.11.6 Ürün açıklaması ve kurumsal içeriğin uzun metinleri tek bir kapalı metin biçimi setini kullanır:** kalın, italik, madde listesi, bağlantı ve tek seviyeli ara başlık. Serbest editör ve ham HTML yoktur; metnin içine görsel eklenmez — görseller kaydın görsel alanındadır; setin dışındaki biçim metne yapıştırıldığında düşer. Setin geçerli olduğu alanlar: ürün açıklaması, Hakkımızda'nın uzun metni, hizmet tanıtımının ve referans işin metni, SSS cevabı, genel sayfanın metni (K-96, K-267).

**3.11.7 Garanti bilgisinin ayrı alanı yoktur.** Garanti süresi ve koşulları firmanın ürün açıklamasına yazdığı metindir. Garanti belgesi gerektiren üründe belge malla birlikte fiziksel olarak gönderilir; sistem garanti belgesi üretmez ve saklamaz (K-230).

### 3.12 Dijital ürün

**3.12.1 Dijital ürünü sistem teslim eder.** Firma dosyayı ürüne bir kez yükler. Ödeme başarılı olduğunda sipariş sayfasında dosyanın indirme düğmesi açılır ve müşteriye sipariş sayfasına giden bir e-posta gönderilir; müşteri dosyayı yeniden indirmek için sipariş sayfasına döner (K-143). Havale ile alınan dijital üründe teslim firmanın "ödendi" işaretine bağlıdır (K-174).

**3.12.2 Dosya ürün düzleminde yaşar; varyant isterse kendi dosyasını taşır, taşımıyorsa ürününkini devralır.** Her iki düzlemde de **tek dosya** vardır — birden çok dosya satan firma onları tek bir arşivde (ör. zip) toplar. Örnek: PDF ve EPUB varyantlı e-kitapta her varyant kendi dosyasını taşır; yalnız lisansla ayrılan varyantlar ürünün tek dosyasını devralır (K-144).

**3.12.3 Tek dosyanın boyut tavanı vardır;** değeri §11'dedir. Kabul edilen dosya türleri ve yüklenen dosyanın güvenliği `05`'in işidir (K-143).

**3.12.4 Dijital üründe adet her zaman 1'dir.** Adet seçici yoktur; bir sipariş kalemi dijital ürünün tek bir satışıdır. Aynı dijital varyantı sepete ikinci kez eklemeye çalışan müşteri "Bu ürün zaten sepetinde" mesajını görür. Aynı ürünün farklı dijital varyantları ayrı kalemlerdir — PDF ve EPUB aynı siparişte alınabilir (K-153).

**3.12.5 İndirmenin adet sınırı vardır, süre sınırı yoktur.** Müşteri dosyayı sipariş sayfasından belli sayıda indirebilir; hak **sipariş kalemi başına** sayılır ve değeri §11'dedir. Hakkı dolmayan müşteri sipariş sayfasına girebildiği sürece indirir (K-146). İndirmenin sayıldığı an `05`'in işidir (K-146).

**3.12.6 Hakkı dolan müşteri firmaya başvurur; firma paneldeki sipariş sayfasından o kalemin indirme hakkını yeniler ve sayaç sıfırlanır.** Müşteri dosyayı yine sipariş sayfasından indirir; misafir alıcıda da yol aynıdır (K-147). Müşteri başvurusunu iletişim formundan "Sipariş hakkında" tipiyle yapar; sipariş sayfasında ayrı bir "hakkımı yenile" düğmesi yoktur (§3.32.10; K-306). Yenileme, firmanın sipariş başına koşullu bir elle adımıdır (§10.5; K-456).

**3.12.7 Sipariş sayfası, kalemin bağlı olduğu varyantın o anki dosyasını verir.** Firma dosyayı değiştirdiğinde geçmiş alıcılar da yeni hâli indirir — düzeltme eski alıcılara kendiliğinden ulaşır. **Bu, kalemin dondurma kuralının (§3.23.1) tek istisnasıdır:** kalem ürünün tanımını ve tutarı dondurmaya devam eder, teslim edilen dosyanın kendisini dondurmaz. Ürün kalıcı silinirse ya da dosyası kaldırılıp yerine yenisi konmazsa geçmiş kalemler dosyanın son hâlini indirmeye devam eder (K-148).

**3.12.8 Dosya güncellemesi indirme hakkını değiştirmez;** yeni hâli indirmek de haktan bir indirme harcar. Hakkı dolan müşteri yeni hâle §3.12.6'nın yoluyla ulaşır (K-149).

**3.12.9 Daha önce alınmış dijital varyant yeniden satın alınabilir.** Üye müşteri onu sepete eklediğinde "Bu ürünü daha önce aldınız — sipariş sayfanızdan indirebilirsiniz" uyarısını görür ve isterse yine de satın alır; misafir alıcıda geçmiş bilinmediği için uyarı yoktur (K-236).

Dijital kalemin teslim hattı §5.7'de, iptal edilemezliği ve cayma istisnası §7'dedir (K-174, K-200, K-204).

### 3.13 Üyelik, e-posta doğrulama ve oturum

**3.13.1 Sipariş vermek için üyelik zorunlu değildir.** Ziyaretçi hesap açmadan sipariş verebilir ve siparişini sipariş numarası + e-posta ile takip eder; üyelik kaldırılmamıştır, yalnız zorunluluğu kalkmıştır (K-97).

**3.13.2 Hesap açıldığında, o e-postaya ait geçmiş misafir siparişleri hesaba düşer** ve "Siparişlerim"de görünür. Bağlanmanın tek dayanağı **doğrulanmış** e-postadır (K-98). **İstisna:** silinmiş bir hesabın siparişleri bağlanmaz (§3.15.3). **Kalan risk bilinçlidir:** e-posta adresi el değiştirirse — ör. bir şirket adresi başka bir çalışana verilirse — yeni sahip, o adresle verilmiş eski misafir siparişlerini ve teslimat adreslerini görür. Doğrulama adresin bugünkü sahibini kanıtlar, geçmiştekini değil; bağlamaya ek doğrulama konmaz (K-489).

**3.13.3 Hesabı olan kişi giriş yapmadan misafir olarak sipariş verirse sipariş kabul edilir ve hesaba anında düşer;** giriş duvarı yoktur. Bağlanan sipariş tam bağlanır — yetki bakımından ikinci sınıf bir sipariş türü yoktur. Ödeme adımında "bu e-posta kayıtlı, giriş yaparsanız adresleriniz dolu gelir" hatırlatması gösterilir; biçimi `04`'ün işidir (K-99). **Kalan risk bilinçlidir:** e-postasını yanlış yazan alıcının siparişi o adresin hesabına düşer ve hesap sahibi o siparişi iptal edebilir ya da iade talebi açabilir. Riski daraltan bir çit konmaz (K-99). Firma siparişin e-postasını düzelttiğinde (§10.4.11) bu bağ kesilir (K-585).

**3.13.4 E-posta doğrulaması sert bir kapıdır:** doğrulanmayan hesapla giriş yapılamaz ve sipariş verilemez. Tek tip hesap vardır — doğrulanmamış hesap bir kullanım durumu değil, bir bekleme aşamasıdır. Misafir siparişi bu kapının dışındadır (K-101).

**3.13.5 Doğrulanmamış hesap geçicidir.** Doğrulama bağlantısının bir ömrü vardır; süre dolduğunda kayıt silinir ve o e-posta yeniden kayda açılır. Süresi geçmiş bağlantıya tıklayan kullanıcı "bağlantı geçersiz, yeniden kayıt olun" mesajını görür. Sürenin değeri §11'dedir (K-102).

**3.13.6 Sosyal giriş yalnız Google ile yapılır;** Facebook ile giriş yoktur (K-103). Google uygulamasının kimlik bilgileri kurulum ayarıdır; kurulumda tanımlı değilse "Google ile giriş" düğmesi görünmez ve üyelik e-posta ile şifreyle yürür. Panelde Google girişi için açık/kapalı anahtarı yoktur — kart yönteminin kalıbı (§3.21.4; K-478, K-378, K-162).

**3.13.7 Aynı e-posta iki giriş yolundan gelirse tek hesaba bağlanır.** Sağlayıcının **doğrulanmış** olarak verdiği e-posta mevcut bir hesabın e-postasıyla eşleşiyorsa kullanıcı doğrudan o hesaba girer ve hesap bundan sonra iki giriş yolunu da taşır. Sağlayıcı e-postayı doğrulanmamış verirse bağlama yapılmaz; kural sağlayıcıdan bağımsızdır (K-104).

**3.13.8 Google ile açılmış hesap şifresiz başlar ve sonradan şifre konabilir;** kullanıcı o günden sonra iki giriş yolunu da kullanır. Ayrı bir akış yoktur: şifresi olmayan hesap "şifremi unuttum" dediğinde sıfırlama akışı "şifre belirle" işlevi görür (K-121).

**3.13.9 Şifre sıfırlama bağlantısının bir ömrü vardır ve bağlantı tek kullanımlıktır.** Kullanılmış ya da süresi dolmuş bağlantı geçersizdir; kullanıcı yeni bir talep açar. Sürenin değeri §11'dedir (K-105).

**3.13.10 Şifre sıfırlandığında hesabın bütün cihazlardaki oturumları kapanır;** kullanıcı kendi cihazında da yeni şifreyle yeniden girer (K-106).

**3.13.11 Ekranlar hesabın varlığını iki farklı biçimde ele verir:** şifre sıfırlama ekranı **nötr** konuşur ("bu adres kayıtlıysa sıfırlama bağlantısını gönderdik"), kayıt ekranı **dürüst** konuşur ("bu e-posta zaten kayıtlı") (K-107).

**3.13.12 Oturum ömrünü kullanıcı seçer.** Giriş ekranında "beni hatırla" işaretlenirse uzun ömürlü, işaretlenmezse kısa ömürlü oturum açılır; iki sürenin değeri §11'dedir (K-108).

**3.13.13 Üç işlem, oturum açık olsa bile yeniden doğrulama ister:** şifre değiştirme, e-posta adresi değiştirme ve hesap silme. Başka hiçbir işlem istemez (K-109). Şifresi olmayan, Google ile açılmış hesapta yeniden doğrulama Google ile yapılır; hesaba şifre konduktan sonra iki yol da geçerlidir (K-109, K-121).

**3.13.14 Hesabın e-posta adresi, yeni adres doğrulanmadan değişmez.** Doğrulandıktan sonra §3.13.2'nin bağlama kuralı yeni adres için yeniden işler; eski adrese bağlı siparişler hesapta kalır, bağlanmış bir sipariş çözülmez (K-110).

**3.13.15 Şifre politikası uzunluk tabanlıdır.** Asgari bir uzunluk aranır (değeri §11'de); büyük harf, rakam ya da sembol dayatılmaz. Çok yaygın kullanılan şifreler reddedilir; bu liste ürünle birlikte gelir ve dış servise bağlanmaz (K-120).

**3.13.16 Bu grubun kimlik doğrulama kuralları firma yöneticisi hesaplarına da aynen ve aynı değerlerle uygulanır** — doğrulama kapısı (§3.13.4), oturum ömrü (§3.13.12), yeniden doğrulama (§3.13.13), e-posta adresinin değiştirilmesi (§3.13.14) ve şifre politikası (§3.13.15). MVP'de yönetici tarafına sıkılaştırma uygulanmaz; iki adımlı doğrulama yoktur (K-122, K-313, K-384). Yöneticinin e-posta değişikliği firma kimliğindeki iletişim e-postasını değiştirmez — ikisi ayrı alanlardır (K-384).

**3.13.17 Giriş, şifre sıfırlama ve hesap kaydı denemeleri limitlidir.** Eşik aşıldığında o işlem geçici olarak engellenir ve pencere içindeki deneme sayısı eşiğin altına inince kendiliğinden açılır; hesap kilitlenmez. Kullanıcı nötr bir mesaj görür ve mesaj ne kalan deneme sayısını ne engelin süresini söyler. Limitler yönetici hesaplarına da aynen uygulanır (§8; K-328, K-329, K-330, K-333, K-460).

**3.13.18 Kayıtta ve hesapta pazarlama onayı yoktur.** Kayıt formunda "kampanyalardan haberdar olmak istiyorum" kutusu bulunmaz ve müşteri kaydında böyle bir alan tutulmaz; ürün pazarlama iletisi göndermez (K-323, K-324; §12).

**3.13.19 Hesap kaydı ve Google ile ilk giriş aydınlatma kapısına bağlıdır.** Aydınlatma metni tamamlanmış ve yayına alınmış değilse (§3.1.5) kayıt ekranı ve yeni hesap açacak Google ile giriş kapalıdır; mevcut hesapların girişi etkilenmez. Panel karşılanmayan koşulu adıyla gösterir (K-517, K-365).

**3.13.20 Her giriş denemesi giriş kaydına yazılır.** Başarılı ve başarısız girişler — müşteri ve yönetici — için zaman, IP, hesap (e-posta) ve sonuç tutulur; kayıt altı ay saklanır ve periyodik imhayla silinir (§4.2 Z-40, §11.2 P-45). Kayıt veri ihlalinde kapsamın kaynağıdır (§8.5.1; K-515).

### 3.14 Adres

**3.14.1 Üye müşterinin adres defteri vardır.** Üye birden çok adres kaydeder ve adlandırır (ör. Ev, İş); siparişte defterinden seçer. **Misafir alıcının adres defteri yoktur** — her siparişte adresini yazar (K-111).

**3.14.2 Fatura adresi teslimat adresinden ayrı seçilebilir.** Varsayılan olarak aynıdır; kullanıcı "fatura adresim farklı" diyerek ikinci bir adres seçer (K-112).

**3.14.3 Adreste il serbest metin değildir;** 81 ilden oluşan kapalı listeden seçilir (K-133).

**3.14.4 Siparişin istediği adresler kalemlerin tipine bağlıdır.** Siparişte en az bir fiziksel kalem varsa teslimat ve fatura adresi istenir ve §3.14.2 işler. Yalnız dijital ve/veya hizmet kalemi taşıyan siparişte teslimat adresi alanları hiç gösterilmez; fatura adresi yasal fatura zorunluluğu nedeniyle istenir (K-194, K-143).

**3.14.5 Sipariş, adreslerini kendi içine dondurur;** adres defterindeki sonraki değişiklik ya da silme geçmiş siparişe dokunmaz (§3.23.3; K-113).

**3.14.6 Adresin alanları:** alıcının adı ve soyadı, il (kapalı liste — §3.14.3), ilçe ve açık adres — mahalle, cadde ya da sokak, bina ve daire numarası tek bir metin alanında; dördü de zorunludur. İlçe serbest metindir: hiçbir kural ilçeye dayanmaz (teslimat kısıtı il düzeyindedir — §3.20.1) ve ilçe listesi ürünün bakımına ikinci bir resmî liste eklerdi. **Teslimat adresi ayrıca telefon taşır ve telefon zorunludur** — kargo şirketi ya da firmanın kendi aracı teslimde alıcıya ulaşır. Telefon bir bildirim kanalı değildir; bildirimler yalnız e-postayla gider (§9.1; K-315). Fatura adresi telefon taşımaz. **Posta kodu ve T.C. kimlik numarası istenmez:** teslim posta koduna dayanmaz; vergi mükellefi olmayan nihai tüketiciye kesilen faturada T.C. kimlik numarası zorunlu değildir — Gelir İdaresi, numarasını paylaşmayan tüketicinin e-Arşiv faturasında alıcı alanına 11111111111 yazılabileceğini duyurmuştur; firma bu alanı muhasebe aracında böyle tamamlar ve ürün numarayı toplamaz (§12.1.10). Adres defterindeki kayıt aynı alanları taşır; telefon kayıtta isteğe bağlıdır ve telefonu olmayan kayıt teslimat adresi olarak seçildiğinde telefon ödeme adımında istenir (K-561).

### 3.15 Hesabın silinmesi ve kişisel veri başvurusu

**3.15.1 Hesap silindiğinde hesap kapanır, sipariş kaydı yasal saklama süresi boyunca durur.** Giriş bilgileri, şifre, açık oturumlar, adres defteri ve sepet **hemen** silinir; siparişlerin içindeki kişisel veriler yasal saklama süresi dolduğunda imha edilir — tutar ve tarih gibi kişisel olmayan alanlar firmanın ticari kaydı olarak kalır (K-115, K-123, K-357). Süreler §4'te, imhanın biçimi §12'dedir (K-353).

**3.15.2 Yürüyen sipariş hesap silmeyi engellemez.** Sipariş kendi kaydıyla yoluna devam eder; kullanıcı takibi, cayma ve iade taleplerini misafir yolundan — sipariş numarası + e-posta — sürdürür. Bunu mümkün kılan, siparişin kendi iletişim e-postasını dondurmasıdır (§3.23.3; K-116).

**3.15.3 Silinmiş bir hesabın siparişleri hiçbir hesaba bağlanmaz;** §3.13.2'nin bağlama kuralı onlara uygulanmaz. Kayıt yalnız firmanın ticari kaydı olarak yasal saklama süresi boyunca yaşar (K-117).

**3.15.4 Ürünün sunduğu başvuru kanalı iletişim formudur.** Formun "KVKK talebi" konu tipi (§3.32.3) bu kanaldır; ayrı bir KVKK modülü, ayrı durum makinesi ve süre sayacı yoktur — talep iletişim talebinin iki durumunu kullanır (K-118, K-302, K-303). Tebliğ'in diğer başvuru yolları açık kalır ve aydınlatma metninde sayılır; formun alanları değişmez. Başvurunun zorunlu unsurlarının tamamlatılması ve cevaptan önce başvuranın kimliğinin doğrulanması firmanın yükümlülüğüdür (§12.2.6, §12.5; K-553). 30 günlük cevap süresi bir ürün kararı değil, KVKK md. 13'ün gereğidir ve sistemde sayaç olarak tutulmaz (K-118, K-305; §12).

**3.15.5 Uygulama içi veri indirme yoktur.** Kullanıcı verisini §3.15.4'ün kanalından ister, firma cevabı hazırlar (K-119). Cevabın dayanağı paneldedir: siparişler sipariş listesinden, hesap verisi **üye kaydı görünümünden** okunur — e-postayla aranır; hesabın varlığı, adres defteri, giriş yolları (şifre, Google) ve bağlı siparişler görünür; görünümde düzenleme yoktur. Firma, ilgili kişinin talebi üzerine hesabı panelden silebilir; silme §3.15.1'in rejimiyle yapılır ve işlem izine yazılır (§10.1.2; K-512).

### 3.16 Sepet

**3.16.1 Üyenin sepeti hesabında durur ve her cihazda aynıdır; misafirin sepeti bulunduğu tarayıcıya bağlıdır.** Üye çıkış yaptığında o tarayıcıda sepet boş görünür — sepet hesapla birlikte gider (K-123).

**3.16.2 Sepet kendiliğinden boşalmaz.** Misafirin sepeti o tarayıcının verileri silinene kadar, üyeninki hesap durdukça yaşar; süre parametresi yoktur (K-126).

**3.16.3 Giriş anında iki sepet birleşir; aynı varyant toplanmaz.** Misafirken dolan sepet, girişte hesaptaki sepetle tek sepete iner. Aynı varyant iki tarafta da varsa adetler toplanmaz, **büyük olan kalır**; farklı varyantlar ayrı kalemler olarak yan yana durur. Kural giriş yolundan bağımsızdır (K-124). Birleşen sepet de adet ve stok kurallarından geçer (§3.16.8–§3.16.10).

**3.16.4 Birleştirme, müşterinin o ziyarette görmediği bir ürünü ya da adedi sepete eklediyse müşteriye bir kez söylenir** ve eklenen ürünler adıyla listelenir ("Önceki ziyaretinden sepetine 1 ürün eklendi: Kupa"). Birleştirme müşterinin gördüğü sepete bir şey eklemediyse mesaj yoktur. Mesajın metni ve yeri `04`'ün işidir (K-125).

**3.16.5 Sepet kalemi canlıdır:** fiyatını ve satın alınabilirliğini güncel üründen okur (K-80). Kalem, varyantın sepete **ilk** eklendiği andaki birim fiyatı hatırlar; güncel fiyat bundan farklıysa satırda "Sepete eklediğinden beri fiyatı değişti" yazar. Artışta da düşüşte de aynı metin yazar; eski fiyat ve değişimin yönü gösterilmez. İşaret fiyat referansa dönerse kalkar; sonradan adet artırmak referansı değiştirmez; birleştirmede daha önce eklenmiş olanın fiyatı referans kalır (K-131).

**3.16.6 Vitrinde görünen ama satın alınamayan kalem sepette "Tükendi" işaretiyle kalır ve siparişe girmez** — tükenmiş varyant ve kontenjanı dolmuş hizmet. Stok geri geldiğinde kalem yeniden satın alınabilir olur (K-130). Kalem müşteri onay özetini görmeden önce satın alınamaz olduysa özet onsuz hesaplanır; özet gösterildikten sonra olduysa onay anındaki fark §3.17.2'ye göre siparişin oluşmasını durdurur ve müşteri güncel özeti yeniden onaylar — kalemi düşmüş sipariş müşterinin onaylamadığı bir özetle oluşmaz (K-129).

**3.16.7 Vitrinden kalkan kalem sepetten çıkar ve müşteriye bir kez söylenir** — ürünün ya da varyantın arşive ya da taslağa alınması, kalıcı silinmesi. Sipariş kalan kalemlerle devam eder; bu fark güncel özette söylenir ve özet gösterildikten sonra oluştuysa müşteri yeniden onaylar (§3.17.2; K-129). **Siparişe girmeyen kalem tutarlara da girmez:** kuponun asgari sepet tutarı, kargo hesabı ve eşikler yalnız siparişe giren kalemleri görür (K-130).

**3.16.8 Stoktan fazla adet sepete eklenmez ve stok adedi söylenmez.** Müşteri "Bu adette stok yok" mesajını görür; mesaj kaç adet alınabileceğini söylemez. Stok ayrılmış adetler düşülerek okunur (K-128). Sepette adet artırma ve giriş anındaki birleştirme aynı kuraldan geçer. Kalem sepete eklendikten sonra stok sepetteki adedin altına düşerse kalem aynı mesajla sepette kalır ve adet düşürülene kadar siparişe girmez — adet kendiliğinden stoğa indirilmez, çünkü indirilen adet stoğu söylerdi. Hizmet kontenjanı aynı kuralı izler (K-150).

**3.16.9 Firma bir üründen bir siparişte alınabilecek adedi sınırlayabilir.** Ürün formunda isteğe bağlı bir "bir siparişte en fazla" alanı vardır; sınır ürünün **bütün varyantlarının toplam adedine** uygulanır — sınır 2 ise sepette 1 kırmızı + 1 mavi lamba bulunabilir, 2 kırmızı + 1 mavi bulunamaz. Sınırı aşan adet sepete eklenmez ve müşteri sınırı sayısıyla söyleyen mesajı görür ("Bu üründen bir siparişte en fazla 2 adet alınabilir"). Seçilen adet hem sınırı hem stoğu aşıyorsa firmanın sınırı söylenir, stok söylenmez. Alan boşsa tek sınır stoktur (K-151, K-152). Alan doluysa değeri en az 1'dir; panel 0'ı sebebini söyleyerek reddeder — sıfır, ürünü yayında görünür ama satın alınamaz kılardı (§4.1.5'in kalıbı; K-533).

**3.16.10 Sonradan aşılan sınırda kalemler sepette mesajıyla kalır.** Firma sınırı düşürdüğünde ya da girişte birleşen sepette farklı varyantların toplamı sınırı aştığında, ürünün kalemleri toplam sınıra inene kadar siparişe girmez. Siparişe girmeyen kalem toplama sayılmaz (K-151, K-152).

**3.16.11 Sepetteki kalem sayısına tavan yoktur** (K-154). Elli kaleme kadar sepet bir performans kabulüdür, sınır değildir: ödeme öncesi yeniden değerlendirme (§3.17.2) bu boyutta kabul edilebilir sürede yanıt verir; yanıt süresinin değeri `05`'in tasarım girdisidir (§11.3; K-409, K-533).

**3.16.12 Sepet, ödeme başarılı olduğunda boşalır** — ve yalnız siparişe giren kalemler çıkar; ödeme sürerken sepete eklenen başka bir ürün sepette kalır. Ödeme yarıda kalırsa (yanlış doğrulama kodu, kart limiti, banka ekranının kapanması) sepet olduğu gibi durur ve müşteri aynı sepetten yeniden dener (K-127).

**3.16.13 İstek listesi ve sepet hatırlatması yoktur.** "Favorilerime ekle" düğmesi ve istek listesi sayfası bulunmaz; süresiz sepet "sonra alırım" ihtiyacını karşılar (K-386, K-126). Sepetinde ürün bırakıp ayrılan müşteriye hatırlatma e-postası gönderilmez — böyle bir e-posta ticari elektronik iletidir ve ürün pazarlama iletisi göndermez (K-325, K-323).

Dijital üründe adedin 1 olması ve daha önce alınmış dijital varyantın uyarısı §3.12.4 ve §3.12.9'dadır. Misafir sepetini tarayıcıya bağlayan çerez zorunlu çerezdir (§12; K-351).

### 3.17 Sipariş onayı ve stok ayırma

**3.17.1 Sipariş, müşterinin ödeme adımında siparişi onayladığı anda oluşur.** Aynı anda sipariş numarası verilir (§3.22.2), kalemler, adresler ve sözleşme sürümleri donar (§3.23) ve stok ayrılır (§3.17.3) (K-80, K-128, K-186).

**3.17.2 Onaylanan özet ile oluşan sipariş birebir aynıdır.** Onay anında sepet yeniden değerlendirilir. Ödenecek tutarı, tutarın dökümünü (KDV, indirim, kuponun payı, kargo) ya da siparişe girecek kalemleri değiştiren **her fark** — fiyatın artması ya da düşmesi, indirimin başlaması ya da bitmesi, kuponun geçersizleşmesi, bir kalemin ayrılamaması, kargo ücretinin, ücretsiz kargo eşiğinin ya da teslimat illerinin değişmesi — siparişin oluşmasını durdurur. Müşteri güncel özeti görür ve isterse yeniden onaylar; güncel özet neyin değiştiğini açıkça söyler ("Lambanın fiyatı 1.000 TL'den 1.200 TL'ye değişti"). Değişikliğin yönü fark etmez. Ayrılamayan kalem için sayı söylenmez (K-129, K-132, K-133, K-135, K-150). Ön Bilgilendirme Formu özetle birebir aynı olduğu için aynı fark formu da değiştirir (§3.24.3).

**3.17.3 Sipariş onaylandığında kalemlerin adedi stoktan ayrılır ve müşteriye bir ödeme süresi tanınır.** Ödeme süre içinde başarılı olursa ayrılan adet stoktan **kesin düşer**; süre dolar ya da ödeme başarısız olursa — hangisi önce gelirse — adet **stoğa geri döner**. Sepete eklemek stok ayırmaz. Aynı rejim hizmet kontenjanına ve kupon kullanım hakkına (§3.10.6) uygulanır; dijital ürünün ayrılacak bir şeyi yoktur (K-128).

**3.17.4 Ödeme süresi yönteme göre ayrıdır.** Kart ödemesinin süresi dakikalarla ölçülür ve ürün sabitidir; havalenin süresi iş günüyle ölçülür, firma panelden girer ve panel alt ve üst çitin dışındaki değeri kabul etmez. İki değer de §11'dedir, havalenin alt ve üst çiti de oradadır; iş gününün tanımı ve sayım kuralı §4'tedir (K-166, K-336, K-337, K-338, K-339). Havale beklenirken ayırma süre boyunca sürer (K-165).

**3.17.5 Ödeme süresi dolan ya da kart ödemesi başarısız olan sipariş kendiliğinden iptal olur** ve ayrılan stok, hizmet kontenjanı ve kupon hakkı aynı anda serbest kalır; firmanın elle iptal etmesi beklenmez (K-180). Kart ödemesinde süre dolduğunda, iptalden önce sağlayıcıya ödemenin sonucu son bir kez sorulur (§6.1.2, K-232). Ödemesi alınmadan iptal edilen her siparişin — hangi yoldan iptal edilirse edilsin — ödemesi Başarısız olur (K-296). Geçişler §5.6'dadır.

**3.17.6 Başarısız ödemenin tekrarı yeni bir siparişle olur.** Müşteri sepetten yeniden onaylar ve yeni bir sipariş numarası alır; başarısız sipariş İptal edildi olarak kayıtta kalır. Aynı siparişi sürdürme yolu yoktur (K-181).

**3.17.7 Müşteri aynı sepetten yeni bir siparişi onayladığında, o sepete bağlı ödenmemiş önceki siparişi kendiliğinden iptal edilir** ve ayırması serbest kalır; yeni siparişin ayırması bundan sonra yapılır. Müşteri kendi ayırdığı stoğa takılmaz (K-182).

**3.17.8 Ödemesi hiç başarılı olmamış ve kendiliğinden iptal edilmiş sipariş — ödeme süresi dolmuş, kart ödemesi başarısız olmuş ya da aynı sepetten yeni sipariş onaylanmış — müşterinin sipariş listesinde görünmez; firmanın panelinde görünür.** Müşterinin ya da firmanın iptal ettiği ödenmemiş sipariş ve ödemesi başarılı olmuş ve sonradan iptal edilen sipariş müşteriye görünmeye devam eder (K-184, K-528). Ödeme denemesi limiti yalnız ödeme süresinin dolmasıyla ya da kart başarısızlığıyla kendiliğinden iptal edilmiş ödenmemiş siparişleri sayar; müşterinin ya da firmanın iptali ve aynı sepetten yeni siparişin tetiklediği iptal sayılmaz. Sayım aynı e-posta adresi ya da aynı IP üzerinden — iki eksen ayrı ayrı — yapılır; eşiği aşan kullanıcı bir süre yeni sipariş onaylayamaz (§8; K-332, K-528). **E-posta ekseni yalnız giriş yapmış üyenin verdiği siparişleri, hesabın e-postasıyla sayar;** misafir siparişi — hesaba anında ya da sonradan düşse de (§3.13.2, §3.13.3) — yalnız IP ekseninde sayılır, çünkü doğrulanmamış bir e-posta başkasının adresi olabilir (§8.2.2; K-579). **Aynı anda açık ödenmemiş sipariş de tavanlıdır:** aynı e-posta adresi ya da aynı IP için — iki eksen ayrı ayrı, e-posta ekseni yalnız üyenin siparişlerinde — tavan doluyken yeni **havale** siparişi açılamaz, kart yolu açık kalır; tavan §8.2'de L-8, değeri §11'dedir (K-524, K-579). Ödemesi hiç alınmamış siparişin kişisel verisi üç yıl saklanır; on yıllık süre yalnız ödemesi alınmış siparişe uygulanır (§4.2 Z-41; K-519).

### 3.18 Asgari sipariş tutarı

**3.18.1 Firma isteğe bağlı tek bir asgari sipariş tutarı girebilir;** girmezse özellik kapalıdır. Sepetin tutarı bunun altındaysa sipariş verilemez ve müşteri eksik tutarı görür ("Sipariş verebilmek için 130 TL'lik daha ürün ekleyin"). Kontrol sepette ve onay anında yapılır; onay anında bir fiyat değişikliği ya da siparişe girmeyen bir kalem yüzünden toplam altına düşerse sipariş durur ve müşteri güncel özeti görür (K-155).

**3.18.2 Karşılaştırma tabanı ücretsiz kargo eşiğininkiyle aynıdır:** siparişe giren bütün kalemlerin kupondan önceki, indirimli fiyatlarla toplamı. Kupon girmek bir siparişi asgari tutarın altına düşürüp engellemez (K-156).

**3.18.3 Asgari tutar yalnız en az bir fiziksel kalem içeren siparişe uygulanır.** Uygulandığında taban yine bütün kalemlerin toplamıdır — karışık siparişte dijital ürün ve hizmet de tabana girer. Koşul **siparişe giren** kalemlere bakar: sepette yalnız "Tükendi" işaretli bir fiziksel kalem varsa kural uygulanmaz; müşteri fiziksel kalemi çıkarırsa kural kalkar (K-157).

### 3.19 Kargo ücreti ve ücretsiz kargo eşiği

**3.19.1 Kargo ücreti sipariş başına sabittir.** Firma panelde tek bir tutar girer; siparişte **en az bir fiziksel kalem** varsa sipariş bu ücreti bir kez öder — adet, kalem sayısı, ağırlık ve teslimat ili ücreti değiştirmez. Yalnız dijital ürün ve hizmetten oluşan siparişte kargo ücreti yoktur. Firma 0 TL girerse her sipariş ücretsiz kargoludur (K-132). Kargo şirketiyle entegrasyon yoktur (`08`, K-132).

**3.19.2 Kargo ücreti KDV dahil tek tutar olarak girilir; KDV'si siparişteki fiziksel kalemlerin oranını izler.** Firmanın alıcının adresine kadar yaptırdığı taşımanın bedeli ayrı bir hizmet değil, malın tesliminin matrahıdır (KDV Kanunu m.24/a). Fiziksel kalemler farklı oranlardaysa kargo ücreti bu kalemlerin tutarına orantılı bölünür ve her parça kendi kaleminin oranıyla KDV dökümüne girer; bölüşüm sipariş anında kargo ücretiyle birlikte donar (§3.19.6). Kargo KDV oranı bir firma ayarı değildir (§11 P-2 kaldırıldı). **Kalan risk:** farklı oranlı kalemler arasında orantılı bölüşüm kanun metninde adıyla yazılı değildir; kurulumda firmanın mali müşaviri teyit eder (§12.5) (K-132, K-219, K-500).

**3.19.3 Firma isteğe bağlı bir ücretsiz kargo eşiği girebilir.** Karşılaştırma tabanı eşiğe eşit ya da üstündeyse siparişten kargo ücreti alınmaz ("500 TL ve üzeri siparişlerde kargo ücretsiz"); eşik girilmezse özellik kapalıdır (K-135).

**3.19.4 Eşiğin tabanı kupondan önceki tutardır:** siparişe giren bütün kalemlerin — fiziksel, dijital ve hizmet — indirimli fiyatlarla toplamı. İndirim tabanı düşürür, kupon düşürmez; kupon girmek bir siparişi hiçbir zaman pahalılaştırmaz (K-136, K-137).

**3.19.5 Eşik tanımlıysa ve siparişte fiziksel kalem varsa, sepette ve ödeme özetinde eşiğe kalan tutar yazar** ("Ücretsiz kargoya 80 TL kaldı"); taban eşiği karşıladığında "Kargo ücretsiz" yazar. Eşik yoksa ya da fiziksel kalem yoksa mesaj da yoktur. Metnin biçimi ve yeri `04`'ün işidir (K-138).

**3.19.6 Kargo ücreti ve ücretsiz kargo kararı sipariş düzleminde donar;** sonraki kısmi iptal ya da iade onu yeniden hesaplatmaz (§3.23.2, §7.4.6; K-132, K-213). Ödenmiş kargo ücreti iki hâlde geri ödenir: siparişin iptal edilmemiş fiziksel kalemlerinin tamamı kargoya verilmeden iptal edildiğinde ve kargoyla gönderilen fiziksel kalemlerin tamamından cayıldığında — ikisinde de tek seferde ya da ardışık işlemlerle (§7.2.9, §7.4.6; K-501).

### 3.20 Teslim: bölge, yol, kargoya verme ve takip

**3.20.1 Firma teslimat yaptığı illeri seçer; varsayılan seçim tüm Türkiye'dir.** Fiziksel kalem içeren siparişin teslimat adresi seçili illerden birinde değilse sipariş verilemez ve müşteriye sebebi söylenir ("Bu firma Ankara'ya teslimat yapmıyor"). Kısıt il düzeyindedir; ilçe düzeyinde sınır yoktur. Kısıt varsa site bunu müşteri adres adımına gelmeden görünür kılar; biçimi `04`'ün işidir. Üyenin defterindeki teslimat yapılmayan ildeki adres ödeme adımında seçilemez. Yalnız dijital ürün ve hizmetten oluşan sipariş il kontrolüne girmez (K-133).

**3.20.2 Tek teslimat yolu kargodur.** Ödeme adımında teslimat seçeneği yoktur; mağazadan teslim alma ve adlandırılmış teslimat seçenekleri (standart kargo, hızlı kargo) yoktur. Sistem için "kargo", fiziksel kalemin müşterinin adresine gönderilmesidir — firmanın bunu bir kargo şirketiyle ya da kendi aracıyla yapması bir ayrım değildir (K-134). Şube teslim noktası değildir (K-243).

**3.20.3 Sipariş tek parça gider ve tek takip numarası taşır;** parçalı gönderim yoktur. Farklı kargoya verme süresi taşıyan kalemler birlikte bekler; acelesi olan müşteri ayrı sipariş verir (K-177).

**3.20.4 Firmanın teslimat sözü kargoya verme süresidir** ve vitrinde "2 iş günü içinde kargoya verilir" biçiminde gösterilir. **Yasal teslim üst sınırı** — satıcı malı taahhüt ettiği sürede ve, kişiye özel üretilen mallar dışında, en geç 30 gün içinde teslim eder — sözleşme metninde yazar; aşılırsa müşteri, sipariş kargoya verilmiş olsa bile gecikme nedeniyle fesheder (§7.2.3; K-499, K-549). Panel kargoya verme süresine bir üst çit koyar ve çitin üstündeki değeri sebebini söyleyerek reddeder; çitin değeri §11'dedir (K-339). Çit kişiye özel üretim istisnası işaretli üründe de bilinçli olarak korunur. **Kalan risk:** siparişe özel uzun üretim yapan firma çitten uzun bir süreyi sistemde söz olarak veremez; bu ihtiyaç ilk gerçek kurulumda doğarsa çit yeniden açılır (K-549). Süre iş günüyle sayılır; iş gününün tanımı ve sayım kuralı §4'tedir (K-336, K-337, K-338). Söz yalnız fiziksel kalemi kapsar (K-139).

**3.20.5 Kargoya verme süresinin firma varsayılanı vardır; ürün kendi süresini taşıyabilir** ve taşıyorsa varsayılanı ezer; varyant ürününkini devralır. Siparişin sözü, siparişe giren fiziksel kalemlerin sürelerinin **en uzunudur** ve sipariş onaylandığı anda donar. Üst çit hem varsayılana hem ürünün süresine uygulanır (K-140).

**3.20.6 Kargoya verme süresi ödemenin onaylandığı anda başlar** — kartta sağlayıcının başarı bildiriminde, havalede firmanın "ödendi" işaretinde (K-168, K-173).

**3.20.7 Sipariş "kargoya verildi" olarak işaretlenirken ya kargo şirketi + takip numarası girilir ya da "kendi aracımızla teslim" seçeneği işaretlenir;** ikisinden biri olmadan işaret konamaz. Müşteri bu bilgiyi sipariş sayfasında görür (K-141).

**3.20.8 Kargo şirketi ürünle gelen bir listeden seçilir;** listede olmayan şirket "Diğer" seçilip adıyla yazılır. Listedeki şirketlerde müşteri sipariş sayfasında tek tıkla açılan bir "Takip et" bağlantısı görür; "Diğer"de yalnız şirket adı ve takip numarası görünür. Liste ve takip bağlantısı kalıpları ürünün kendi varlığıdır: firma panelden düzenlemez ve §11'e girmez (K-142).

**3.20.9 Kalemin teslimi tipine göre işler.** Fiziksel kalem kargo hattından geçer (§3.20.7) ve firmanın teslim işaretiyle teslim edilmiş olur (§3.20.11). Dijital kalem ödeme onaylandığında sistem tarafından teslim edilir (§3.12.1, K-174). Hizmet kalemi firmanın panelden "tamamlandı" işaretiyle teslim edilmiş olur; tek bir işlemdir — randevu, takvim ya da planlama ekranı yoktur (K-175). İşaret ancak ödeme Ödendi iken konur; panel ödenmemiş siparişte işareti sunmaz (§5.6.1; K-536). Hizmette firmanın sözü ifa süresidir (§3.2.2; Z-39): süre aşıldığında kendiliğinden bir işlem başlamaz, müşterinin iptal düğmesi kalem tamamlanana kadar açık kalır ve süre aşıldıktan sonraki iptalde panel firmaya kanuni faiz uyarısını gösterir (§7.2.3; K-503, K-499).

**3.20.10 Karışık siparişte sevkiyat tek eksende kalır ve iptal edilmemiş fiziksel kalem kaldığı sürece fiziksel hattı izler;** dijital kalem bunu beklemeden ödeme onayında indirilebilir olur, hizmet kalemi ayrıca "tamamlandı" işaretlenir. Fiziksel kalemlerin tamamı iptal edilirse sipariş fiziksel kalemi olmayan siparişin hattına düşer (§3.20.12). Kalem düzeyinde tutulan şey **teslim işaretidir**, ayrı bir durum makinesi değildir (K-179, K-295). Hatlar §5.7'dedir.

**3.20.11 Fiziksel teslimi firma işaretler ve teslim tarihini kendisi girer.** "Kargoya verildi" → "Teslim edildi" geçişi firmanın panelden koyduğu elle bir işarettir. Firma teslimin gerçekleştiği tarihi girer: tarih **geçmişe dönük olabilir** — kargo salı teslim ettiyse firma perşembe işaretlerken salıyı yazar — ama **ileri tarihli olamaz** ve siparişin kargoya verildiği günden önce de olamaz; panel kuralın dışındaki tarihi sebebini söyleyerek reddeder (K-554). Otomatik geçiş yoktur: işaretlenmeyen sipariş "Kargoya verildi"de kalır ve cayma penceresi başlamaz — yükü firma taşır. Teslim işareti sipariş düzeyinde bir kez konur ve girilen tarih siparişin iptal edilmemiş ve çıkarılmamış fiziksel kalemlerinin tamamına aynı tarih olarak yazılır — parçalı gönderim olmadığı için (§3.20.3) kalem başına ayrı tarih girilmez (K-177, K-288). Tarih fiziksel kalemlerin teslim işaretinde tutulur; cayma penceresi (§7.3.1) ve ayıp talebinin süresi (§7.5.1) bu tarihten başlar (K-288, K-179). Yanlış girilen tarihi firma panelden düzeltir (§10.4; K-369).

**3.20.12 Fiziksel kalemi olmayan sipariş kargo adımlarından geçmez.** Yalnız dijital, yalnız hizmet ya da ikisini birlikte taşıyan sipariş "Hazırlanıyor"u ve "Kargoya verildi"yi kullanmaz; Alındı'da bekler ve kalemlerinin tamamı teslim işaretini aldığında Teslim edildi'ye geçer (K-294; §5.7).

### 3.21 Ödeme

**3.21.1 Satıcı firmadır; platform ticari zincirde yer almaz.** Tahsilat doğrudan firmanın kendi sanal POS / ödeme sağlayıcı hesabına geçer. Kart bilgisi sağlayıcının barındırdığı sayfada ya da çerçevede girilir; ürün kart verisini **görmez, saklamaz, loglamaz** (K-12). Çerçeve kullanılıyorsa sağlayıcının zorunlu çerezleri ödeme adımında bulunur ve çerez politikasında anılır (§12.2.5; K-518). Ödemenin başlatılması ve geri ödeme için alıcının kimlik ve iletişim verisi sağlayıcıya aktarılır (§12.2.8; K-513). Sağlayıcıdan beklenen yetenekler — 3D Secure, barındırılan sayfa, ödeme sonucunu sorgulama ve kısmi iade dahil iade — `10 §4.1` ÖK-4'tedir (K-556).

**3.21.2 İki ödeme yöntemi vardır: kart ve havale/EFT.** Kapıda ödeme yoktur (K-159, K-160).

**3.21.3 Her kart ödemesi istisnasız 3D Secure doğrulamasından geçer;** doğrulama başarısızsa ödeme gerçekleşmez ve sipariş ödenmiş sayılmaz. Tutar eşiği ya da sağlayıcı takdiri yoktur (K-13).

**3.21.4 Kart yönteminin panelde açık/kapalı anahtarı yoktur.** Ödeme sağlayıcı anahtarları kurulumdan geldiği için kart, anahtarlar tanımlıysa açıktır; tanımlı değilse kart yöntemi görünmez ve havale açıksa satış yalnız havaleyle açılır (K-162, K-163).

**3.21.5 Havale/EFT panelden açılıp kapatılır; açıkken IBAN zorunludur.** Havale kapalıyken yöntem ödeme adımında hiç görünmez (K-161). Aynı e-posta adresi — yalnız giriş yapmış üyede — ya da aynı IP için açık ödenmemiş sipariş tavanı (L-8) doluyken havale yöntemi o müşteriye seçilemez, kart yolu açık kalır (§3.17.8; K-524, K-579).

**3.21.6 Havale ile siparişi onaylayan müşteri ekranda firmanın IBAN'ını ve sipariş numarasını görür;** sipariş ödeme bekler. Firma parayı hesabında görüp panelden "ödendi" işaretlediğinde ödeme onaylanır (K-159). Ödeme süresi dolmadan müşteriye **bir kez**, havale ödeme süresinin son iş gününün başında — o günün ilk gönderim çalışmasında — bir hatırlatma gider: varsayılan 2 iş gününde ikinci günün, 1 iş gününde o günün, 3 iş gününde üçüncü günün başında. Hatırlatma bir işlem bildirimidir, ticari elektronik ileti değildir; anı ayrı bir parametre değildir, süreden türer (K-167, K-320, K-529; §9).

**3.21.7 Firma havalede gelen tutarı sisteme girmez.** Sistemde gelen tutar alanı, sipariş tutarıyla karşılaştırma ve "eksik ödeme" diye bir durum yoktur; eksik ya da fazla gelen havalede farkı isteme ya da kabul etme kararı firmanındır ve sistem dışında yürür (K-169).

**3.21.8 Süresi dolup iptal edilmiş bir sipariş sonradan "ödendi" işaretlenemez.** Firma gelen parayı müşteriye geri öder ya da müşteriden yeni sipariş vermesini ister. Bu sınır yöneticinin durum düzeltme yetkisinin de dışındadır (§10.4; K-183, K-372). Kart hattındaki karşılığı §6.2.16'dadır.

**3.21.9 Ürün taksiti bilmez.** Taksit yalnız sağlayıcının kart ekranında sunulur; vitrinde, ürün sayfasında, sepette ve ödeme özetinde taksit bilgisi gösterilmez ve sipariş **tek tutar** taşır (K-164).

**3.21.10 Sipariş hazırlığa ancak ödeme onaylandığında geçer** (§5.6; K-173).

### 3.22 Sipariş numarası ve takip erişimi

**3.22.1 Sipariş numarası tahmin edilemez bir koddur:** yıl + rastgele karakter bloğu (ör. `2026-7K4M9P`). Karışabilen karakterler (`0`/`O`, `1`/`I`/`l`) kullanılmaz; rastgele bloğun uzunluğu §11'dedir (K-185).

**3.22.2 Numara sipariş oluştuğu anda verilir ve sipariş iptal edilse bile tekrar kullanılmaz.** Numara bütün siparişler arasında tekildir: üretilen numara var olan bir siparişinkiyle çakışırsa sipariş oluşmadan yeni bir numara üretilir. Sipariş numarası **fatura numarası değildir** (K-186, K-567).

**3.22.3 Sipariş sayfasına üç yoldan girilir:** üye giriş yapıp sipariş geçmişinden açar; misafir alıcı sipariş numarası + e-posta ile açar; sipariş e-postasındaki bağlantı sipariş sayfasını doğrudan açar ve ayrıca e-posta sormaz. Üçüncü yol dijital ürünün indirme sayfasına giden bağlantıyla aynı kapıdır (K-187, K-97). Sipariş numarası + e-postayla yapılan sorgular IP başına limitlidir (§8; K-328, K-330).

**3.22.4 Sipariş sayfası her bilgiyi taşır** — durum, takip bağlantısı, indirme düğmesi, IBAN; hiçbir sipariş akışı bir e-postanın ulaşmasına bağlı değildir (§6.1.1; K-234).

**3.22.5 Sipariş e-postasındaki bağlantı, sipariş numarasından ayrı, siparişe özgü ve tahmin edilemez bir erişim anahtarı taşır; sipariş numarası tek başına sipariş sayfasını açmaz.** Numara faturada, iletişim talebinde (§3.32.2) ve firmayla yazışmada dolaşır; bu yüzden numarayla açılan her yol e-postayı da sorar. Bağlantının kendi ömrü yoktur: siparişin kişisel verileri imha edilene kadar (§4.2 Z-28, Z-41) çalışır ve hesabın silinmesinden ya da hesabın e-posta adresinin değişmesinden etkilenmez — siparişin iletişim e-postası donmuştur (§3.23.3). Anahtar yalnız firma misafir siparişinin e-postasını düzelttiğinde yenilenir; eski anahtar o anda geçersizleşir (§10.4.11; K-585). **Kalan risk bilinçlidir:** bağlantıyı başkasına ileten müşteri sipariş sayfasını da paylaşmış olur; dijital ürünün indirme bağlantısı aynı riski taşır (K-187, K-143, K-562).
### 3.23 Sipariş anında donan değerler

**3.23.1 Sipariş kalemi, ödenen tutarı üreten her değeri ve ürünün o anki tanımını dondurur:** birim fiyat, KDV oranı, kalem KDV'si, indirim oranı, beyan edilen referans fiyat, kuponun kaleme düşen payı; ürün adı, seçenek değerleri ("Kırmızı / M") ve görsel. Kalem canlı üründen hiçbir şey okumadan kendini anlatır (K-77, K-79). Tek istisna dijital ürünün dosyasıdır (§3.12.7).

**3.23.2 Donan her değer sipariş kaleminde düz olarak yaşar.** Ürün düzlemli alanlar da (KDV oranı, indirim oranı) kaleme kopyalanır; sipariş düzleminde yalnız sepet düzeyindeki değerler durur — kupon kodu, kargo ücreti ve toplamlar. Ara bir gruplama katmanı yoktur (K-78, K-132).

**3.23.3 Sipariş, teslimat ve fatura adreslerini, iletişim e-postasını, firmanın o günkü yasal kimliğini (§3.1.3'ün tipe göre kimlik alanları, adres, iletişim), onaylanan Ön Bilgilendirme Formu ile Mesafeli Satış Sözleşmesi sürümlerini ve o gün yürürlükte olan aydınlatma metninin sürümünü kendi içine dondurur.** Adres defterindeki, hesaptaki ve firma kimliğindeki sonraki değişiklik ya da silme geçmiş siparişe dokunmaz; geçmiş siparişin sayfası onaylanan sürümü ve o günkü unvanı göstermeye devam eder (K-113, K-116, K-189, K-190, K-347, K-480). **Tek istisna:** misafir siparişinin iletişim e-postası firmanın düzeltmesiyle değişir (§10.4.11; K-585). Donmuş sürümler silinmez ve siparişin saklama süresini izler (§3.33.3; K-362).

**3.23.4 Donma anı siparişin oluştuğu andır.** Müşterinin onayladığı tutar bağlayıcıdır ve ödeme sonucundan bağımsızdır (K-80). Siparişin kargoya verme sözü de bu anda donar (K-140).

**3.23.5 Marka ayarları siparişe donmaz:** geçmiş siparişin sayfası güncel logo, marka adı ve renkle görünür; donan yalnız yasal kimliktir (K-265, K-190).

### 3.24 Onay adımı ve yasal metinler

**3.24.1 Sipariş iki ayrı onay kutusuyla onaylanır:** Ön Bilgilendirme Formu'nun okunduğunun teyidi ve Mesafeli Satış Sözleşmesi'nin onayı. İkisi de işaretlenmeden sipariş onaylanamaz (K-188). İki kutu **sözleşmeye** ilişkindir, KVKK rızası değildir: onay adımında "kişisel verilerimin işlenmesine izin veriyorum" diyen bir kutu yoktur ve aydınlatma metni yalnız bağlantısıyla gösterilir (K-346; §3.33.5). **Onay düğmesinin hemen yanında ya da üstünde siparişin ödeme yükümlülüğü doğurduğu açıkça yazar** (ör. düğme metni "Siparişi onayla — ödeme yükümlülüğü doğar"); havale hattında da aynıdır. Bilgi verilmezse tüketici siparişiyle bağlı değildir (Mesafeli Sözleşmeler Yönetmeliği m.8/1); metin ve biçim `04`'ün işidir (K-544).

**3.24.2 Sepette dijital kalem varsa üçüncü bir kutu çıkar:** "İndirme, ödemem onaylandığı anda açılacak; bu üründe cayma hakkımın düşeceğini kabul ediyorum" — havalede indirme firmanın "ödendi" işaretiyle açıldığı için metin "hemen" demez; nihai metin `04`'ün işidir (K-550). Kutu işaretlenmeden dijital kalem siparişe giremez; kutu cayma istisnasının kurucu koşuludur (K-204, K-346; §7.3.2). **Sepette hizmet kalemi varsa aynı kalıpla bir kutu daha çıkar:** "Hizmetin cayma süresi dolmadan ifasına başlanmasını onaylıyorum; ifa tamamlandığında cayma hakkım düşer." Kutu zorunludur — işaretlenmeden sipariş verilemez; kutu hizmetteki cayma istisnasının kurucu koşuludur (§7.3.3; K-503).

**3.24.3 Ön Bilgilendirme Formu onay özetiyle birebir aynıdır** ve üzerine dört bilgi ekler: teslimat il kısıtı, kargoya verme süresi, cayma hakkının kullanım koşulları ve süresi, firmanın kimlik bilgileri. Özeti değiştiren her fark formu da değiştirir ve siparişi durdurur (K-191, K-129). İstisna işaretli üründe istisna ve sebebi (K-206) — koşullu sebeplerde (§7.3.4) *"koruyucu ambalajı açılmamışsa cayabilirsiniz"* koşuluyla (K-509) —, iade kargo bedelinin firmaya ait olduğu (K-210) ve firmanın iade adresi (K-293, K-362) de formda yazar. Form ayrıca iade için taşıyıcı belirlenmediğini — müşterinin malı istediği taşıyıcıyla iade adresine karşı ödemeli gönderdiğini — (K-493) ve müşterinin malı cayma beyanından itibaren on dört gün içinde göndermesi gerektiğini (K-494) yazar; ikisi de sabit metindir ve panelde yeni bir alan doğurmaz. Sepette hizmet kalemi varsa form hizmetin ifa süresini (§3.2.2) ve cayma hakkının hangi koşulda düşeceğini — müşterinin onayıyla başlayan ifanın tamamlanması — yazar (K-503). Form ve sözleşme, tüketicinin uyuşmazlık başvurularını tüketici hakem heyetine ya da — dava açılmadan önce arabulucuya başvurma şartıyla — tüketici mahkemesine yapabileceğini de yazar (§3.33.7; K-366, K-543). **Onay adımında görünen sabit bir "sipariş nasıl kurulur" metni** — Ön Bilgilendirme Formu'nun da parçasıdır — dört bilgiyi verir: sözleşmenin kurulması için izlenecek adımlar; sözleşme metninin sipariş sayfasında saklama süresi (Z-28) boyunca erişilebilir olduğu; veri giriş hatalarının onaydan önce özetten düzeltilebileceği; gizlilik kuralları için aydınlatma metninin bağlantısı ve uyuşmazlık yolları. Firma kimliğinin meslek odası alanı doluysa form onu da yazar (§3.1.3; K-547).

**3.24.4 Yaş için ayrı beyan istenmez.** Yaş kuralı Mesafeli Satış Sözleşmesi'nde yazılıdır; müşteri metni onaylayarak beyan etmiş olur. Onay adımında "18 yaşından büyüğüm" kutusu ve doğum tarihi alanı yoktur (K-114, K-192). Ayrı bir üyelik sözleşmesi yoktur — yasal metin envanteri dört metinden ibarettir (§3.33.1; K-361, K-453).

**3.24.5 Misafir alıcının e-postası doğrulama koduyla sınanmaz.** Onay adımında özetin içinde açıkça gösterilir ("Siparişiniz şu adrese gönderilecek: x@y.com") ve müşteri onaylamadan önce düzeltebilir; adres ikinci kez yazdırılmaz (K-193).

**3.24.6 Onaylanan metinlerin sürümü ve firmanın kimliği siparişe donar** (§3.23.3). Metinlerin sürümlenmesi §3.33'tedir (K-189, K-361, K-362).

**3.24.7 Ödeme adımında müşteri notu alanı yoktur.** "Sipariş notu" ya da "teslimat talimatı" için serbest metin alanı bulunmaz; özel talebi olan müşteri iletişim formunun "Sipariş hakkında" tipiyle yazar (K-389; §3.32.3).

### 3.25 Fatura

**3.25.1 Ürün fatura kesmez; e-Arşiv ve e-Fatura entegrasyonu yoktur.** Sistem ürünün tuttuğu fatura verisini taşır — kalem dökümü, KDV oranları ve tutarları, indirim ve kupon payı, kargo ücreti ve fiziksel kalemlerin oranlarına bölünmüş KDV'si (§3.19.2; K-500), fatura adresi, firmanın donmuş kimliği, ödeme yöntemi ve ödeme onay tarihi, kargoya verme tarihi (hizmette tamamlama, dijitalde ödeme onayı), kargo şirketinin adı ve sitenin alan adı — ve sipariş dışa aktarmasıyla (§10.7; K-426) dışarı verir. e-Arşiv faturanın ürün dışındaki alanlarını (ör. taşıyıcının vergi kimlik numarası) firma muhasebe aracında tamamlar (K-546). Firma faturayı kendi muhasebe programından ya da entegratöründen keser; bu, sipariş başına bir **sistem dışı** adımdır (K-217, K-402). Kurumsal fatura alanı yoktur: ticari amaçla alan birinin vergi bilgisi ürünün dışında alınır (K-112).

**3.25.2 Fatura sistemde saklanmaz ve gösterilmez.** Sipariş sayfasına fatura yükleme, faturayı sistem içinden iletme ve fatura arşivi yoktur; firma faturayı müşteriye kendi kanalından iletir (K-218).

**3.25.3 Dışa aktarmada siparişin donmuş firma kimliği kullanılır,** panelin o anki kimliği değil (K-220).

### 3.26 İptal, cayma, iade ve ayıp talebi

Bu kuralların tamamı §7'dedir ve burada tekrarlanmaz (K-178, K-195…K-216 — K-209'u K-491 değiştirir —, K-491…K-494, K-226…K-229, K-288…K-293, K-297, K-340, K-356, K-366, K-367, K-373).

### 3.27 Kurumsal içerik

**3.27.1 Kurumsal içerik hazır tiplerle ve düzeni sabit bir genel sayfayla girilir;** sayfa kurucu yoktur — firma düzen kurmaz, yalnız içerik girer. Ana sayfa bu tiplerden beslenir (K-237).

**3.27.2 Beş hazır tip vardır: Hakkımızda, hizmet tanıtımı, referans iş, sık sorulan soru ve şube;** bunlara genel sayfa ve duyuru eklenir. **Ayrı bir galeri tipi yoktur** — fotoğraf ait olduğu içerikte durur. Kariyer, belgeler, ekip, tarihçe gibi ihtiyaçlar genel sayfayla karşılanır (K-238, K-269).

**3.27.3 Hakkımızda tek kayıttır:** kısa tanıtım (düz metin, karakter tavanlı), uzun metin ve görseller. Firma ikinci bir Hakkımızda açamaz; Hakkımızda **silinmez**, yalnız taslağa alınır (K-239, K-277).

**3.27.4 Hizmet tanıtımı fiyatsızdır ve satın alınmaz.** Ad, kısa açıklama (listedeki kartta), metin, görseller ve kendi sayfası vardır; sepete ekleme yoktur, sayfadaki "Bize ulaşın" düğmesi iletişim formuna (§3.32) götürür. Satılan hizmet ürün tipinden ayrıdır (K-240).

**3.27.5 Referans iş, firmanın yaptığı bir işi anlatır:** başlık, kısa açıklama, metin, görseller ve kendi sayfası. Ayrı müşteri adı, yıl ve kategori alanı yoktur; müşteri adı başlıkta yazılır (K-241).

**3.27.6 Sık sorulan soru, soru ve cevaptan oluşur;** hepsi tek bir SSS sayfasında listelenir ve başlıklara bölünmez. Soru düz metin ve karakter tavanlıdır. **Kayda geçen risk:** cevap, sistemin ayarlarda tuttuğu bir kuralı (kargo ücreti, iade süresi) tekrarlarsa ayar değiştiğinde çelişkili kalır. Ürün çelişkiyi tespit etmez; bağlayıcı olan ayarların güncel hâlinden üretilen form ve sözleşmedir ve sorumluluk firmadadır (§3.33.8; K-242, K-364).

**3.27.7 Şubeler İletişim sayfasında listelenir;** ayrı sayfaları ve menü öğeleri yoktur. Ad ve adres zorunludur — il kapalı listeden seçilir; telefon, çalışma saatleri (kısa düz metin — karakter tavanı §3.27.15'tedir), görsel ve harita bağlantısı isteğe bağlıdır. **Gömülü harita yoktur;** şube "Haritada aç" bağlantısı taşır. Şube teslim noktası değildir (K-243, K-533).

**3.27.8 İletişim sayfası ayrı bir içerik tipi değildir:** firma kimliğinin iletişim bilgileri, şubeler ve iletişim formu (§3.32) bir araya gelir; firma ayrıca metin girmez (K-243).

**3.27.9 Genel sayfa başlık, metin ve görsellerden oluşur, düzeni sabittir ve adet sınırı yoktur.** Yasal metinler genel sayfa olarak girilmez — sürümlü ayrı kayıtlardır (§3.33; K-244, K-361).

**3.27.10 Hizmet tanıtımı ve referans iş katalogdan ürün bağlayabilir.** Bağlı ürünler içerik sayfasında "İlgili ürünler" başlığıyla vitrin kartı olarak görünür. Yalnız yayındaki ürünler gösterilir — taslağa ya da arşive alınan ürün bağda kalır ama görünmez, yeniden yayına alınınca geri gelir; silinen ürün bağdan kalkar. Bağ ürün düzlemindedir, varyant seçilmez; ürün sayfasında ters yön ("bu ürünün geçtiği referanslar") yoktur (K-245). Bağ firmanın elle kurduğu bir ilişkidir; davranışa dayalı öneri yoktur (§3.3.6; K-387).

**3.27.11 Hizmet tanıtımı, referans iş, SSS, şube ve genel sayfa listelerini firma panelde elle sıralar;** vitrindeki liste sayfaları, ana sayfa blokları ve menü bu sırayı izler (K-247). Ürün listelerinin sabit düzeni (§3.5.3) bundan etkilenmez.

**3.27.12 Firma isteğe bağlı sosyal medya hesap bağlantıları ve bir WhatsApp numarası girer.** Platform listesi kapalıdır: Instagram, Facebook, X, LinkedIn, YouTube, TikTok. Bağlantılar sitenin üst ve alt bölümünde görünür; sitede gömülü gönderi akışı yoktur. Bağlantılar firma kimliğinin parçası değildir: zorunlu değildir ve siparişe donmaz (K-249). WhatsApp bağlantısı canlı destek değildir; sitede sohbet penceresi yoktur (§3.32.9; K-388).

**3.27.13 Hiçbir kurumsal içerik tipi zorunlu değildir;** site kurumsal içerik girilmeden de yayındadır. Kayıt düzeyinde zorunlu alanlar — yayın kapısı — tipe göredir: hizmet tanıtımında ad; referans işte ve genel sayfada başlık; SSS'de soru ve cevap; şubede ad ve adres. **Ad ya da başlık alanı taşımayan iki tekil kayıtta** kapı içeriğin kendisidir: Hakkımızda'da kısa tanıtım — ana sayfanın kurumsal bloğu onu gösterir (§3.27.14) —, duyuruda metin (§3.27.21). Görsel hiçbir tipte zorunlu değildir (K-250, K-584).

**3.27.14 Hakkımızda boşken ya da yayında değilken ana sayfanın kurumsal bloğu marka adını ve logoyu gösterir;** boş kutu görünmez (K-250, K-259).

**3.27.15 Kurumsal içeriğin görselleri ve kısa alanları ürünün kurallarına tabidir:** ana görsel işareti sıradan bağımsızdır, galeri elle sıralanır, alternatif metin isteğe bağlıdır ve boşsa kaydın adı ya da başlığı — SSS'de soru, Hakkımızda'da tipin adı — görselin galerideki sırasıyla birlikte kullanılır (§3.11.4; K-564, K-584); format listesi ve tek dosya boyutu tavanı aynı parametrelerdir, kayıt başına görsel adedi tavanı vardır. Kısa alanlara — ad, başlık, soru, kısa tanıtım, kısa açıklama, şubenin çalışma saatleri — karakter tavanı konur; uzun metin serbesttir. Değerler §11'dedir (K-252, K-533).

**3.27.16 Uzun metinler §3.11.6'nın metin biçimi setini kullanır** (K-267).

**3.27.17 Kurumsal içerikte gömülü video oynatıcı yoktur.** Firma videoya bağlantı verir; ziyaretçi tıkladığında video kendi platformunda açılır (K-254).

**3.27.18 Kurumsal içeriğe dosya eki (PDF vb.) eklenmez.** Kalite belgesi, sertifika gibi belgeler görsel olarak yüklenir (K-255).

**3.27.19 Müşteri görüşleri ayrı bir içerik tipi değildir.** Firma bir müşterisinin sözünü referans işin metnine yazabilir; ana sayfada dönen bir alıntı bloğu yoktur (K-256).

**3.27.20 Blog yoktur.** Tarihli yazılar, yazı arşivi, kategori ya da etiket ve yazı akışı kapsam dışıdır; arada bir yazı yayımlamak isteyen firma genel sayfa açar (K-268).

**3.27.21 Duyuru şeridi vardır: tek, kısa, tarihli.** Sitenin her sayfasının en üstünde bir satırlık **metin** ve isteğe bağlı bir **bağlantı**. Duyuru tekil bir kayıttır: aynı anda tek duyuru vardır ve sıraya dizilmiş birden çok duyuru yoktur (K-269). Başlangıç ve bitiş tarihi isteğe bağlıdır; girilirse duyuru yalnız o aralıkta görünür — firmanın elle kaldırmasına gerek kalmaz. Metin düz metin ve karakter tavanlıdır (değeri §11'de). Kayan afiş (slider) ve ana sayfa kampanya görseli yoktur (K-269). **Duyuru bir kurumsal içerik kaydıdır, firma ayarı değildir:** yayın durumunu taşır ve önizlenir (§3.27.23); metin düzenlemesi işlem izine yazılmaz, yayın durumu değişikliği yazılır (K-502, K-273, K-276, K-310). Kurulumda duyuru kaydı yoktur (§3.27.22).

**3.27.22 Site boş kurulur.** Kurumsal içerik, katalog ve duyuru örnek kayıt taşımaz — örnek metin, ürün ya da görsel yoktur. Boş sitenin görünüşü kurallarla tanımlıdır: marka adı girilene kadar alan adı (§3.29.3), logo yoksa marka adı (§3.29.2), Hakkımızda boşken marka adı ve logo (§3.27.14), kaydı olmayan tip menüde ve ana sayfada görünmez (§3.28.3, §3.28.4). İlk kurulumda firmaya yol gösteren kontrol listesi §10.8'dedir (K-271, K-469).

**3.27.23 Her içerik kaydı kendi yayın durumunu taşır: Taslak ya da Yayında;** arşiv yoktur ve yayından kaldırılan kayıt taslağa döner. Yayına almak için ikinci bir onay yoktur — her yönetici yayımlar (K-251, K-273). Durum tanımı §5.2'dedir.

**3.27.24 Taslak kayıt yalnız adla ya da başlıkla — SSS'de soruyla — kaydedilebilir; yayına almak §3.27.13'ün zorunlu alanlarını ister.** Ad ya da başlık taşımayan Hakkımızda ve duyuru tekil olduğu için panel listesinde ayırt edilmeye ihtiyaç duymaz ve boş da taslak kaydedilir (K-584). Zorunlu alanı eksik kayıt yayına alınamaz ve panel eksik alanı gösterir (K-275). Yayındaki kaydın düzenlemesi kaydedildiği anda yayına girdiği için (K-276) yayın kapısını bozan düzenleme — ör. SSS cevabının, şubenin adresinin ya da Hakkımızda'nın kısa tanıtımının silinmesi — kaydedilmez; panel eksik alanı gösterir ve firmayı önce kaydı taslağa almaya yönlendirir (§5.2; K-299'un kalıbı, K-538).

**3.27.25 Taslak içeriğin sayfası ziyaretçiye "sayfa bulunamadı" döner; giriş yapmış yöneticiye sayfayı "Taslak" bandıyla gösterir** — önizleme budur. Kendi sayfası olmayan kayıtlar — SSS sorusu, şube, duyuru — yöneticiye göründükleri yerde aynı işaretle önizlenir; biçimi `04`'ün işidir. Taslak kayıt menüde, ana sayfada ve listelerde ziyaretçiye görünmez (K-274).

**3.27.26 İçerikte sürüm geçmişi yoktur.** Yayındaki bir kayıt düzenlenip kaydedildiğinde değişiklik hemen yayına girer; eski hâle dönme işlemi yoktur. Büyük bir yeniden yazımda firma yeni bir taslak kayıt açar, hazır olduğunda onu yayına alır ve eskisini taslağa alır ya da siler. Kaydın yayına alınmasını ya da yayından kaldırılmasını kimin yaptığı işlem izinde görünür; içeriğin metin düzenlemeleri ize yazılmaz — izin kapsamı §10.3'tedir (K-276, K-310). Yasal metinlerin sürümlenmesi ayrı bir konudur (§3.33; K-189, K-361).

**3.27.27 Kurumsal içerik kalıcı olarak silinebilir; silme geri alınamaz işlem uyarısıyla yapılır.** Silinen kaydın sayfası "sayfa bulunamadı" döner; ana sayfa işareti, menüdeki yeri ve ürün bağları kendiliğinden kalkar. **İstisna:** Hakkımızda silinmez (K-277).

**3.27.28 Kurumsal içerikte ileri tarihli yayın yoktur;** tek istisna duyurunun tarih aralığıdır (K-278).

### 3.28 Ana sayfa ve menü

**3.28.1 İki hazır ana sayfa düzeni vardır ve firma panelden birini seçer: tanıtım öncelikli ya da mağaza öncelikli.** İki düzende de kurumsal tanıtım ve ürün vitrini ana sayfada **birlikte** bulunur — değişen yalnız ağırlık ve sıradır. Serbest sayfa kurgusu yoktur (K-27).

**3.28.2 Ana sayfanın kurumsal bloğu her zaman Hakkımızda'dır:** kısa tanıtımı ve ana görseli gösterir; uzun metin Hakkımızda sayfasında durur (K-239, K-246). Boş hâli §3.27.14'tedir.

**3.28.3 Hizmet tanıtımı ve referans iş bir "ana sayfada göster" işareti taşır;** işaretli kayıtlar ana sayfadaki kendi bloklarında elle sıralarıyla görünür. **İşaretli kayıt yoksa o blok ana sayfada hiç görünmez.** İşaret sıradan bağımsızdır. SSS, şube ve genel sayfa ana sayfaya çıkmaz. Blokların düzendeki yeri ve blok başına kaç kayıt görüneceği `04`'ün işidir (K-246).

**3.28.4 Menü düzenleyici yoktur.** Üst menü sabit bir iskeletten kurulur: ürünler (kategoriler), içerik tipleri (Hizmetlerimiz, Referanslarımız, Hakkımızda, SSS) ve İletişim. **Yayında kaydı olmayan tip menüde görünmez** — yayında olmayan Hakkımızda'nın menü öğesi de gizlenir (K-251). Firma menüdeki adları değiştirebilir (ör. "Referanslarımız" → "Projelerimiz"); varsayılan adlar ürünle gelir. İskeletin sırası ve görünümü `04`'ün işidir (K-248).

**3.28.5 Genel sayfalar altbilgide listelenir;** firma bir genel sayfayı "menüde göster" işaretiyle üst menüye de çıkarabilir (K-248).

**3.28.6 Yayında ürünü olmayan kategori menüde görünmez** — alt dallarında da yayında ürün yoksa. Katalogda hiç yayında ürün yoksa menünün ürünler kısmı görünmez. Boş kategorinin adresine gelen ziyaretçi boş kategori sayfası görür; kategori var olduğu için "sayfa bulunamadı" dönmez (K-286).

### 3.29 Marka kimliği

**3.29.1 Firma görünümde dört şeyi ayarlar: logo, marka adı, site simgesi ve tek bir marka rengi.** Ana sayfa düzeni §3.28.1'in iki seçeneğinden seçilir. Yazı tipi, sayfa düzeni, bileşenler ve marka rengi dışındaki bütün renkler üründedir; firma değiştiremez. Marka ayarları vitrine ve e-postalara uygulanır; yönetim paneli ürünün kendi görünümündedir. Koyu tema yoktur (K-258). Firma ayrıca altbilgideki platform imzasını kapatabilir (§3.29.6).

**3.29.2 Logo zorunlu değildir.** Logo yoksa marka adı yazıyla gösterilir — sitenin üst bölümünde, ana sayfanın kurumsal bloğunda ve e-postalarda. Tek bir logo yüklenir ve her yerde aynısı kullanılır; logoya görsel dosya kuralları uygulanır (§3.11.3, §3.27.15). Yasal kimlik alanları zorunlu kalır (§3.1.2) (K-259).

**3.29.3 Marka adı unvandan ayrı ve zorunlu bir alandır;** bir kez girildikten sonra boşaltılamaz. Sitenin üst bölümünde (logo yoksa), tarayıcı sekmesinde ve e-postalarda görünen addır. Unvan altbilgideki yasal bilgilerde ve sözleşmede durur. **Marka adı girilene kadar site alan adını gösterir** (K-260).

**3.29.4 Site simgesi isteğe bağlıdır;** yüklenmezse marka adının baş harfi marka rengi zemin üzerinde gösterilir. Firmanın sitesinde Shopfolio simgesi çıkmaz (K-261).

**3.29.5 Marka rengi okunurluğu bozamaz.** Marka renginin üstündeki yazının siyah mı beyaz mı olacağını sistem, yeterli kontrastı veren tarafı seçerek belirler. Anlam taşıyan renkler — hata, uyarı, başarı, "Tükendi", indirim — marka renginden bağımsızdır ve ürünle sabit gelir. Renk seçilmezse ürünün varsayılan rengi kullanılır. Rengin uygulandığı yerler `04`'ün tasarım sisteminin işidir (K-262).

**3.29.6 Sitenin altbilgisinde "Shopfolio ile kuruldu" platform imzası varsayılan olarak açıktır; firma panelden kapatabilir.** İmza yalnız sitenin altbilgisinde durur; e-postalarda ve site simgesinde yer almaz (§3.29.4, §9.1.4; K-20, K-261, K-264). İmza §3.29.1'in dört ayarından biri değildir, ayrı bir anahtardır; sayı olmadığı için §11'e girmez ve değişikliği işlem izine yazılır (K-541).

### 3.30 Sayfa adresleri, arama motorları ve paylaşım

**3.30.1 Ürünün adresi kategoriye bağlı değildir;** ürünün adından üretilir (ör. `/urun/ahsap-masa`; önekin kendisi `07`'nin işidir). Kategori sayfasının adresi de ağaçtaki yerinden bağımsızdır, kategorinin adından üretilir. Kırıntı yolu ana kategoriden üretilmeye devam eder (K-279).

**3.30.2 Her sayfanın adresi kaydın adından üretilir ve ilk yayında sabitlenir.** Türkçe harfler sadeleşir ("Çelik Kapı" → `celik-kapi`); aynı adres zaten varsa sonuna sayı eklenir; adreslerde dil öneki yoktur. Kayıt ilk kez yayına alınana kadar adres adla birlikte değişir; ilk yayında sabitlenir ve kaydın adı ya da menüdeki adı değişse de bir daha değişmez. Taslak ve arşivdeki kaydın adresi dolu sayılır; **kalıcı silinen kaydın adresi serbest kalır** ve aynı adla sonradan oluşturulan kayıt onu alabilir — silme kaydın izini bırakmaz (K-85). Kalan risk bilinçlidir: silinen kaydın paylaşılmış eski bağlantısı, adresi alan yeni kaydı açar (K-568). Liste sayfalarının adresleri (hizmetler, referanslar, SSS, Hakkımızda, İletişim) sabittir; kategori adresi kategori oluşturulduğunda sabitlenir. Firma adresi elle düzenleyemez (K-280).

**3.30.3 Yönlendirme sistemi yoktur.** Adres değişmediği için eski adresten yenisine yönlendirme gerekmez; eski siteden gelen adreslerin taşınması kapsam dışıdır (K-280).

**3.30.4 Var olmayan, silinmiş ve yeni bir kayda verilmemiş ya da ziyaretçiye kapalı (taslak) her adres "sayfa bulunamadı" sayfasına düşer.** Sayfa sitenin görünümündedir — marka, menü, altbilgi — ve ana sayfaya ve ürünlere dönüş yolu taşır; içeriğini firma düzenlemez. Arşivlenmiş ürünün "artık satılmıyor" sayfası (§3.7.6) bundan ayrıdır (K-281).

**3.30.5 Arama motoru bilgileri otomatik üretilir;** firmanın elle doldurduğu SEO alanı yoktur. (1) **Sayfa başlığı:** kaydın adı ve marka adı ("Ahşap Masa – Yılmaz Mobilya"). (2) **Açıklama:** kaydın kısa açıklamasından ya da kısa tanıtımından; kısa alanı olmayan sayfada metnin başından; kategori sayfasında kategori adından ve marka adından. (3) **Site haritası** yayındaki ürünlerden, kategorilerden ve içerik sayfalarından kendiliğinden güncellenir. (4) **Yapılandırılmış veri:** ürün (fiyat, stok durumu), firma, kırıntı yolu ve SSS, arama motorlarının okuyacağı biçimde sayfaya eklenir. (5) **Paylaşım önizlemesi** aynı başlık ve açıklamayla, sayfanın ana görseliyle — görsel yoksa logoyla — çıkar (K-282).

**3.30.6 Arama motorlarına kapalı sayfalar:** sepet, ödeme adımı, hesap sayfaları, sipariş takip sayfası, yönetim paneli, site içi arama ve süzgeç sonuçları, taslaklar ve arşivlenmiş ürün sayfası. Arşiv sayfası doğrudan bağlantıyla açılmaya devam eder; yalnız arama sonuçlarına ve site haritasına girmez (K-283).

**3.30.7 Ürün ve kurumsal içerik sayfalarında tek bir "Paylaş" düğmesi vardır.** Telefonda cihazın kendi paylaşım menüsünü açar; paylaşım menüsü olmayan tarayıcıda sayfanın bağlantısını kopyalar. Platformların paylaşım kodları (Facebook, X vb.) siteye eklenmez. Paylaşılan bağlantının önizlemesi §3.30.5'in başlığı, açıklaması ve görseliyle çıkar (K-287).

### 3.31 Panelde eşzamanlı düzenleme

**3.31.1 Sessiz üzerine yazma yoktur.** İki yönetici aynı kaydı aynı anda düzenlerse ikinci kaydeden, kaydın o arada değiştiğini görür ve üzerine yazmadan önce uyarılır. Kural kurumsal içerik, ürün ve ayarlar için aynıdır; mekanizması `05`/`06`'nın eşzamanlılık kararıdır (K-270).

Yönetim tarafının diğer kuralları — yönetici hesapları, işlem izi, sipariş müdahaleleri, manuel adımlar, satış özeti, dışa aktarma ve ilk kurulum kontrol listesi — §10'dadır.

### 3.32 İletişim talebi

**3.32.1 İletişim formu girişsiz açıktır.** Form İletişim sayfasında (§3.27.8) ve hizmet tanıtımının "Bize ulaşın" düğmesinin götürdüğü yerde durur; ziyaretçiye, misafir alıcıya ve üye müşteriye açıktır. Üye girişliyken ad ve e-posta ön dolu gelir ve değiştirilebilir — üye farklı bir adresten cevap isteyebilir. Gönderilen her form bir **iletişim talebi** kaydı oluşturur (K-300, K-240, K-243).

**3.32.2 Formun alanları:** ad, e-posta, konu tipi ve mesaj zorunludur; telefon isteğe bağlıdır. Dosya eki yoktur. Sipariş numarası ayrı bir alan değildir — gerekiyorsa mesajın içine yazılır (K-301).

**3.32.3 Konu tipi kapalı beş değerden seçilir:** Genel soru · Sipariş hakkında · Ürün hakkında · KVKK talebi · Diğer. Liste ürünle gelir; firma panelden düzenlemez. **Ayıp talebi bu listede yoktur** — sipariş sayfasından, kalem bağlamıyla gider (§7.5.2) (K-302, K-226, K-227).

**3.32.4 Talebin iki durumu vardır: Açık ve Kapatıldı.** Talep Açık doğar; firma panelden kapatır ve kapatılan talep yeniden açılabilir. KVKK talebi ayrı bir durum makinesi taşımaz, aynı iki durumu kullanır (K-303, K-118). Durumlar §5.13'tedir. Saklama süresi talebin son kapatıldığı andan işler; hiç kapatılmayan açık talep, açıldığı andan itibaren aynı sürenin sonunda imha edilir (§4 Z-33; K-507).

**3.32.5 Sistem talebi kaydeder ve iletir, yazışmayı yürütmez.** Talep panele düşer ve firmaya bildirim e-postası gider (§9). Cevap sistemden yazılmaz: firma müşteriye e-postayla cevap verir; panelde yanıt yazma ekranı, yazışma dizisi ya da hazır şablon yoktur (K-305).

**3.32.6 Müşteri talebini takip etmez.** Talebin müşteriye gösterilen bir numarası, "taleplerim" sayfası ya da durum sorgusu yoktur; müşteri cevabı e-postayla alır ve talebin durumu yalnız firmanın panelinde yaşar (K-307).

**3.32.7 Form spam'e karşı iki mekanizmayla korunur:** insanın görmediği **görünmez tuzak alan** — doldurulduğunda gönderim hata mesajı vermeden **sessizce** düşer — ve **IP başına gönderim limiti** (§8). Üçüncü taraf captcha yoktur (K-304, K-328).

**3.32.8 Aydınlatma metni tamamlanmış ve yayına alınmış değilse iletişim formu kapalıdır.** "Tamamlanmış", barındırma konumu bölümünün ve firma kimliği alanlarının dolu olmasıdır — metin ürünle bir taslakla geldiği ve boşaltılamadığı için "boş" hâli hiç gerçekleşmez. Form kişisel veri topladığı için aydınlatma metninin bağlantısını göstermek zorundadır; koşul karşılanana kadar form çalışmaz, kurumsal sayfalar yayında kalır. Aynı kapı hesap kaydını ve Google ile ilk girişi de kapatır (§3.1.5, §3.13.19; K-342, K-365, K-517).

**3.32.9 Ayrı bir şikâyet kanalı ve canlı destek yoktur.** Müşterinin sipariş sayfasındaki üç yolu — iptal, cayma, ayıp talebi — dışındaki her konu bu formdan gider; panelde ayrı bir "şikâyetler" listesi açılmaz (K-367). Sitede sohbet penceresi, çevrimiçi göstergesi ya da mesajlaşma aracı yoktur (K-388).

**3.32.10 İndirme hakkını yenileme başvurusu da bu formdan, "Sipariş hakkında" tipiyle gelir** (K-306; §3.12.6). Ödeme adımında not alanı olmadığı için siparişe dair özel talepler de aynı tiple gelir (K-389; §3.24.7).

Kapatılan talebin saklama süresi §4'tedir (K-353).

### 3.33 Yasal metinler

**3.33.1 Dört yasal metin vardır:** Ön Bilgilendirme Formu · Mesafeli Satış Sözleşmesi · aydınlatma metni · çerez politikası. Dördü de **sürümlüdür** ve genel sayfa olarak girilmez (§3.27.9). İlk ikisi ayarlardan ve sipariş içeriğinden **üretilir**; son ikisini ürün bir taslak metinle getirir, firma panelden düzenler ve içeriğin sorumluluğunu üstlenir (K-361, K-191, K-343, K-350, K-244). Envanterin dışında yasal metin yoktur; ayrı bir üyelik sözleşmesi bulunmaz (K-453).

**3.33.2 Firmanın düzenlediği iki metin zorunludur ve boşaltılamaz.** Aydınlatma taslağı firma kimliği alanlarından beslenen yer tutucular taşır ve barındırma ile e-posta altyapısının konumu için firmanın dolduracağı bir bölüm içerir; o bölüm doldurulmadan metin tamamlanmış sayılmaz. Ödeme sağlayıcısı ve Google girişi de taslakta yer tutucuyla anılır; ödeme sağlayıcısı alıcı grubu olarak anılır — ödemenin başlatılması ve geri ödeme için alıcının kimlik ve iletişim verisi ona aktarılır (§12.2.8; K-343, K-359, K-360, K-513). **Taslak bir alıcı grupları bölümü taşır:** kargo ve teslimat şirketleri · fatura ve muhasebe hizmeti sağlayıcıları · ödeme kuruluşu · barındırma ve e-posta altyapısı (veri işleyen) · bakım ve destek sağlayıcısı (veri işleyen); firma bunları adıyla doldurur (K-553). Taslak her işleme amacının hukuki sebebini §12.2.3'ün sayımıyla yazar ve Tebliğ'in başvuru yollarını sayar (K-516, K-553).

**3.33.3 Sürüm kendiliğinden artar.** Firmanın düzenlediği metinde sürüm, firma metni yayına aldığında; üretilen metinde, metni besleyen bir ayar değiştiğinde artar — kargo ücreti, kargoya verme süresi, iade adresi gibi. Sürüm elle girilmez. **Eski sürümler silinmez:** siparişe donmuş her sürüm okunabilir kalır ve bağlı olduğu siparişin saklama süresini izler. Sürüm değişikliği işlem izine yazılır (K-362, K-310, K-353).

**3.33.4 Metin değiştiğinde yeniden onay alınmaz.** Her sipariş onaylandığı gün yürürlükte olan sürüme tabidir; yürüyen siparişler etkilenmez ve yeni sürüm yalnız yeni siparişlere uygulanır. Üyeye "metinler değişti, kabul et" penceresi gösterilmez ve aydınlatma metni değiştiğinde bildirim gönderilmez (K-363).

**3.33.5 Aydınlatma metninin bağlantısı yeni bir kişisel veri alanı açan her ekranda görünür:** hesap kaydı ekranı · Google ile giriş düğmesinin bulunduğu giriş ekranı · sipariş onay adımı · IBAN'ın girildiği ekranlar (iptal ve cayma beyanı, sipariş sayfasındaki IBAN alanı) · ayıp talebi ekranı · iletişim formu · her sayfanın altbilgisi (K-553). Çerez politikasının bağlantısı da altbilgidedir; çerez onay bandı yoktur (K-342, K-349, K-350). Aydınlatmanın onay kutusu yoktur ve kullanıcı bazında "gördü" kaydı tutulmaz; ispat metnin sürümüyle ve gösterim noktalarıyla sağlanır, sipariş o günkü sürümü dondurur (K-346, K-347; §3.23.3).

**3.33.6 Ön Bilgilendirme Formu ve Mesafeli Satış Sözleşmesi müşteriye e-postayla da gönderilir.** Siparişe donan sürümleriyle sipariş onay e-postasının **gövdesinde tam metin** olarak yer alırlar; ayrı dosya eki üretilmez ve sipariş sayfası aynı sürümleri göstermeye devam eder (K-316; §9). E-posta ulaşmazsa firma onu panelden donmuş sürümüyle yeniden gönderebilir (§6.1.1; K-522).

**3.33.7 Uyuşmazlık bilgisi iki metnin zorunlu parçasıdır.** Form ve sözleşme, tüketicinin uyuşmazlık başvurularını tüketici hakem heyetine ya da — dava açılmadan önce arabulucuya başvurma şartıyla — tüketici mahkemesine yapabileceğini yazar (Mesafeli Sözleşmeler Yönetmeliği m.5/1-k; K-543). Merciler arasındaki parasal görev sınırı **rakamla yazılmaz** — sınır her yıl değişir ve siparişe donan sürümde eskirdi. Bilgi ayrıca altbilgide durmaz (K-366, K-368).

**3.33.8 Ayarlarla çelişen serbest metin firmanın sorumluluğundadır.** SSS cevabı, genel sayfa ya da aydınlatma metni, ayarlarda tutulan bir kuralı — kargo ücreti, iade süresi, kargoya verme süresi — tekrarlarsa ve ayar sonradan değişirse metin çelişkili kalır. Ürün çelişkiyi tespit etmez ve ayarlardan üretilen bir bilgi sayfası kurmaz; bağlayıcı olan, ayarların güncel hâlinden üretilen form ve sözleşmedir. Panelde ilgili ayarın yanında bir hatırlatma satırı durur (K-364, K-242).

**3.33.9 Dört metin tamamlanmadan satış açılmaz;** kapı §3.1.5'tedir ve panel eksik metni adıyla gösterir (K-365, K-465).

### 3.34 Site geneli: cihaz, tarayıcı, erişilebilirlik ve süreklilik

**3.34.1 Tek site, duyarlı tasarım.** Ayrı bir mobil site (`m.` alt alan adı) ya da hafifletilmiş mobil sürüm yoktur; her cihaz aynı adresi açar ve aynı içeriği görür. Kırılma noktalarının sayısı ve genişlikleri `04`'ün işidir ve §11'e girmez (K-391, K-463).

**3.34.2 Vitrin mobil önceliklidir, panel masaüstü önceliklidir; panelde hiçbir işlev mobilde kapatılmaz.** Firma kargoya verme, havale onayı, hizmet tamamlama, iptal ve teslim işareti gibi zamana duyarlı işleri telefondan yapabilir; öncelik ayrımı yalnız hangisinin önce tasarlanacağını söyler (K-392).

**3.34.3 Tarayıcı tabanı son iki büyük sürümdür:** masaüstünde Chrome, Safari, Firefox ve Edge; mobilde iOS Safari ve Android Chrome. Internet Explorer ve eski (EdgeHTML) Edge desteklenmez. Kural bir tarih değil, kendini güncel tutan bir ölçüttür ve §11'e sayı olarak girmez (K-393).

**3.34.4 En dar desteklenen ekran genişliği bir sayıdır;** değeri §11'dedir. Bu genişliğin altında düzenin bozulması kabul edilir, üstünde bozulması bir hatadır (K-394).

**3.34.5 Erişilebilirlik hedefi WCAG 2.1 AA'dır; uyumluluk beyanı verilmez.** Yedi kural zorunlu ve test edilebilirdir: (1) metin ile arka plan arasında yeterli kontrast (§3.29.5) · (2) her görselin alternatif metni (§3.11.4) · (3) klavyeyle tam gezinme — ödeme akışı fare olmadan tamamlanabilir · (4) görünür odak göstergesi · (5) etiketli form alanları · (6) anlamın yalnız renkle taşınmaması — hata, "Tükendi" ve indirim işareti metin de taşır · (7) %200 yakınlaştırmada içerik kaybı ve yatay kaydırma olmaması (K-395, K-396).

**3.34.6 Sitede ziyaretçi ölçümü yoktur.** Ne üçüncü taraf analitik aracı ne ürünün kendi sayacı vardır: ziyaretçi sayısı, sayfa görüntüleme, oturum takibi ve huni ölçümü tutulmaz. Firma panelden analitik kodu, reklam dönüşüm etiketi ya da sosyal medya pikseli ekleyemez. Panelde yalnız ürünün kendi kayıtlarından — sipariş ve iletişim talebi — türeyen satış özeti vardır (§10.6) (K-397, K-399, K-263, K-398, K-483).

**3.34.7 Ürün çalışma süresi taahhüdü vermez.** Kesinti olağan kabul edilir; erişilebilirlik barındırmaya bağlıdır ve barındırma kurulum tarafındadır. İlan edilen, sitenin kapatıldığı planlı bakım penceresi yoktur; güncelleme sırasındaki kısa kesinti bu kabulün içindedir (K-406, K-408, K-08).

**3.34.8 Sipariş tarafında veri kaybı toleransı sıfırdır.** Onaylanmış sipariş, ödemesi, durum geçişleri, kalem düzeyindeki kayıtlar — cayma beyanı ve tarih damgası, iade malının ulaşma tarihi, ayıp talebi —, iletişim talepleri (KVKK talebi dahil), donmuş yasal metin sürümleri ve işlem izi kaybolamaz — para, yasal kayıt ya da yasal süre taşırlar (K-557). Sepet ve oturum kaybı kabul edilir. Yedeklemenin sıklığı, yöntemi ve kurtarma süresi `05`'in ve `DEPLOY_RUNBOOK`'un işidir (K-407).

*Kaynak: K-01 · K-02 · K-08 · K-09 · K-11…K-14 · K-27 · K-39 · K-40 · K-42…K-83 · K-85…K-88 · K-90…K-99 · K-101…K-169 · K-173…K-175 · K-177…K-220 · K-226…K-230 · K-232 · K-234…K-252 · K-254…K-256 · K-258…K-263 · K-265 · K-267…K-271 · K-273…K-283 · K-286…K-307 · K-310 · K-313 · K-316 · K-320 · K-323…K-325 · K-328…K-330 · K-332…K-334 · K-336…K-340 · K-342 · K-343 · K-346 · K-347 · K-349…K-351 · K-353 · K-356 · K-357 · K-359…K-369 · K-372 · K-373 · K-384…K-389 · K-391…K-399 · K-402 · K-406…K-409 · K-411…K-413 · K-415 · K-418…K-424 · K-426 · K-433…K-435 · K-445 · K-465 · K-463 · K-456 · K-469 · K-453 · K-478 · K-493 · K-494. **Kapsama (2026-10-02, v0.8):** etki sütununda `02 §3` taşıyan **376 satırın** tamamı bu bölümde ya da kuralı başka bölümde yaşıyorsa o bölümde — süreler §4, durumlar §5, hata senaryoları §6, iptal, cayma, iade ve ayıp talebi §7, kötüye kullanım §8, bildirimler §9, yönetim §10, parametreler §11, yasal yükümlülükler §12, açık kararlar §13 — karşılık bulur; bu bölümde onlara işaret kalır. Aşama sürecine ilişkin Blok 9 satırlarından K-428, K-429 ve K-436 §13'te, K-430, K-432 ve K-437 dokümanın başlık notunda, K-438 §10.5.4'te karşılık bulur. Sayım v0.8'in anlık görüntüsüdür; o tarihten sonra etki sütununa `02 §3` ile giren satırlar Kaynak'ta adıyla anılır · K-20 · K-264 · K-480 · K-499 · K-528 · K-529 · K-378 · K-483 · K-489 · K-533 · K-541 · K-502 · K-460 · K-491 · K-492 · K-500 · K-501 · K-503 · K-507 · K-536 · K-538 · K-539 · K-554 · K-508 · K-509 · K-511 · K-512 · K-513 · K-516 · K-517 · K-518 · K-519 · K-524 · K-543 · K-544 · K-545 · K-546 · K-547 · K-549 · K-550 · K-553 · K-556 · K-557 · K-515 · K-522 · K-561 · K-562 · K-563 · K-564 · K-565 · K-567 · K-568 · K-576 · K-584 · K-585.*

## 4. Zaman kuralları

> **Ne yazılır:** Tüm süre/timeout kuralları **tek bölümde**. Aktör akışlarına gömülmez — dağıtılırsa tutarsızlık doğar.

Bu bölüm ürünün bütün sürelerini ve kendiliğinden işleyen anlarını tek yerde toplar. Sürenin **değeri** bir parametreyse §11'de yaşar ve burada "§11" yazar; yasal süreler §11'e girmez, değerleri burada yazılıdır (K-411, K-353). Sürelerin ürün kuralı olarak yeri §3, §5 ve §7'dir; burada tekrarlanan yalnız süre, başlangıç ve sonuçtur.

### 4.1 Sayım kuralları

**4.1.1 Tek saat dilimi Türkiye saatidir.** Bütün tarihler, süreler ve zaman damgaları bu saate göre tutulur ve gösterilir; ürün ikinci bir saat dilimi desteklemez ve kullanıcıya saat dilimi seçtirmez. Yaz saati geçişi yoktur. Depolamanın biçimi `05` ve `06`'nın işidir (K-335).

**4.1.2 İş günü pazartesi–cumadır, resmî tatiller hariç.** Resmî tatil listesi ürünle gelir; firma panelden düzenlemez ve liste §11'e girmez. Arife gibi yarım günler tam iş günü sayılır. Firmanın kendi kapalı günleri iş günü hesabına girmez — firma bunu kargoya verme süresini uzatarak karşılar (K-336). **Kalan risk:** liste bakım güncellemesiyle yenilenir — Ramazan ve Kurban bayramlarının tarihleri her yıl kayar; güncelleme gelmezse iş günü hesabı (Z-8, Z-10) yanlışlaşabilir ve sonucu firmadadır (§12.3.1; K-481). Listenin kapsamadığı yılda iş günü hesabı yalnız hafta sonunu dışlar (K-558).

**4.1.3 İki süre iş günüyle, gerisi takvim günüyle sayılır.** İş günüyle sayılanlar: kargoya verme süresi ve havale ödeme süresi. Yasal süreler takvim günüdür — cayma penceresi, müşterinin iade malını gönderme süresi, geri ödeme süreleri, ayıp talebi süresi, KVKK cevap süresi ve yasal teslim üst sınırı; firmanın tatil takvimi tüketicinin hakkını kısaltamaz (K-337).

**4.1.4 Gün olarak belirlenen süre, başlangıç gününün ertesi günü başlar ve son günün 23:59'unda dolar.** Başlangıç günü sayıma girmez ve saat bazlı sayım yoktur — "14 gün" 14 × 24 saat değil, 14 takvim günüdür. Aynı kural iş günüyle sayılan sürelere de uygulanır (K-338). Dakika ve saatle ölçülen süreler — bağlantı ömürleri, kart ödeme süresi — ve son işlemden işleyen oturum süreleri — gün olarak belirlenmiş uzun oturum dahil — gün sayımına girmez, başladıkları andan itibaren işler (K-459).

**4.1.5 Üç süre çitlidir:** kargoya verme süresinin üst çiti vardır — kargonun yolda geçen günleriyle birlikte yasal 30 günü aşmasın diye; havale ödeme süresinin alt ve üst çiti vardır; hizmetin ifa süresinin alt çiti vardır. Panel çitin dışındaki değeri kabul etmez ve sebebini söyler. Çitlerin değerleri §11'dedir (K-339, K-448, K-503).

### 4.2 Süreler

| # | Kural | Başlangıç | Süre | Süre dolunca ne olur | Ayarlanabilir mi | Kaynak |
|---|---|---|---|---|---|---|
| | **Hesap ve oturum** | | | | | |
| Z-1 | E-posta doğrulama bağlantısının ömrü | Bağlantının gönderildiği an | §11 | Bağlantı geçersizleşir; doğrulanmamış kayıt silinir ve e-posta yeniden kayda açılır | Hayır — ürün sabiti | K-102 |
| Z-2 | Şifre sıfırlama bağlantısının ömrü | Talep anı | §11 | Bağlantı geçersizleşir; kullanıcı yeni talep açar. Bağlantı ayrıca tek kullanımlıktır | Hayır — ürün sabiti | K-105 |
| Z-3 | Yönetici daveti bağlantısının ömrü | Davetin gönderildiği an | §11 | Bağlantı geçersizleşir ve hesap açılmaz; yeni bir davet gerekir. Bağlantı ayrıca tek kullanımlıktır | Hayır — ürün sabiti | K-308 |
| Z-4 | Kısa oturum — "beni hatırla" işaretsiz | Son işlem | §11 | Oturum kapanır, kullanıcı yeniden giriş yapar | Hayır — ürün sabiti | K-108, K-459 |
| Z-5 | Uzun oturum — "beni hatırla" işaretli | Son işlem | §11 | Oturum kapanır, kullanıcı yeniden giriş yapar | Hayır — ürün sabiti | K-108, K-459 |
| | **Sepet ve ödeme** | | | | | |
| Z-6 | Sepetin ömrü | — | Süresiz | Kendiliğinden boşalmaz: misafirin sepeti tarayıcının verileri silinene, üyeninki hesap durdukça yaşar | Parametre yoktur | K-126 |
| Z-7 | Kart ödeme süresi — stok ayırma bu süre boyunca sürer | Sipariş onayı | §11 | Sipariş kendiliğinden iptal olur, ödemesi Başarısız olur; ayrılan stok, hizmet kontenjanı ve kupon hakkı aynı anda serbest kalır. İptalden önce sağlayıcıya ödemenin sonucu son bir kez sorulur. **Son sorgu yanıtsız kalırsa sipariş iptal edilmez:** ödeme Bekliyor'da kalır, ayrılanlar tutulur ve sorgu sağlayıcı yanıt verene kadar yinelenir; sonuç gelince olağan geçiş işler, sipariş panelde "ödeme sonucu alınamadı" işaretiyle görünür (§6.1.2) | Hayır — ürün sabiti | K-128, K-166, K-180, K-232, K-296, K-462, K-556 |
| Z-8 | Havale ödeme süresi — stok ayırma bu süre boyunca sürer | Sipariş onayı; yanlış "ödendi" işaretinin düzeltilmesinde düzeltme anı (§10.4.6) | §11 (iş günü; alt ve üst çitli) | Sipariş kendiliğinden iptal olur, ödemesi Başarısız olur; ayrılanlar serbest kalır. Süre dolup iptal edilmiş sipariş sonradan "ödendi" işaretlenemez | Evet — firma ayarı | K-165, K-166, K-180, K-183, K-337, K-339, K-570 |
| Z-9 | Havale ödeme hatırlatması | Sipariş onayı | Havale ödeme süresinin son iş gününün başı — o günün ilk gönderim çalışması (§3.21.6) | Müşteriye **bir kez** hatırlatma gider | Ayrı parametre değildir — süreden türer | K-167, K-320, K-529 |
| | **Teslim** | | | | | |
| Z-10 | Kargoya verme süresi — firmanın vitrindeki sözü | Ödemenin onaylandığı an | §11 (iş günü; üst çitli). Siparişin sözü, fiziksel kalemlerin sürelerinin en uzunudur ve sipariş onaylandığında donar | Kendiliğinden bir işlem başlamaz; süre firmanın sözüdür. Sipariş kargoya verilmediği sürece müşterinin iptal düğmesi açıktır ve gecikmede fesih bu düğmeyle kullanılır; süre aşıldıktan sonraki iptalde panel firmaya kanuni faiz uyarısını gösterir (§7.2.3) | Evet — firma ayarı; ürün kendi süresini taşıyabilir | K-139, K-140, K-168, K-198, K-337, K-339, K-499 |
| Z-11 | Yasal teslim üst sınırı — kişiye özel üretilen mallar dışında | Sipariş onayı — siparişin firmaya ulaştığı an | 30 takvim günü | Sözleşme metninde yazar; aşılırsa teslim tarihi girilmemiş fiziksel kalemi bulunan siparişte müşteri, sipariş kargoya verilmiş olsa bile sipariş sayfasından "gecikme nedeniyle fesih" yapar; feshedilen kalemlerin bedeli kargo ücreti dahil fesih bildiriminden itibaren on dört gün içinde geri ödenir (§7.2.3) | Hayır — yasal | K-139, K-198, K-337, K-448, K-499, K-549 |
| Z-12 | Teslim tarihi | — | Firmanın panelden girdiği tarih: geçmişe dönük olabilir ama siparişin kargoya verildiği günden önce ve ileri tarihli olamaz | Teslim işaretlenmeyen sipariş "Kargoya verildi"de kalır ve cayma penceresi başlamaz | — | K-288, K-554 |
| Z-39 | İfa süresi — hizmet kalemi; firmanın hizmetteki sözü | Ödemenin onaylandığı an | Ürünün ifa süresi — takvim günü (§11 P-44) | Kendiliğinden bir işlem başlamaz; süre firmanın sözüdür, vitrinde ve Ön Bilgilendirme Formu'nda yazar. Kalem "tamamlandı" işaretlenene kadar müşterinin iptal düğmesi açıktır; süre aşıldıktan sonraki iptalde panel firmaya kanuni faiz uyarısını gösterir (§7.2.3) | Evet — firma ayarı (ürün formunda; alt çitli) | K-503, K-499 |
| | **Cayma, geri ödeme ve ayıp talebi** | | | | | |
| Z-13 | Cayma penceresi — fiziksel kalem | Pencere kalemin teslim tarihinden işler | 14 takvim günü | Cayma düğmesi kapanır; daha önce yapılmış beyan geçerli kalır. Geç işaretlenen teslimin riski firmadadır | Hayır — yasal | K-203, K-288, K-337, K-338, K-340 |
| Z-14 | Cayma penceresi — hizmet kalemi | Sipariş tarihi; fiziksel kalem içeren siparişte siparişin teslim tarihi — teslim tarihi girilmemişse pencere başlamaz | 14 takvim günü ya da müşterinin onayıyla başlayan ifanın "tamamlandı" işareti — hangisi önce gelirse | Cayma hakkı düşer | Hayır — yasal | K-205, K-289, K-503 |
| Z-15 | Cayma hakkı — dijital kalem | — | Ödemenin başarılı olduğu anda, üçüncü onay kutusuyla düşer | — | Hayır — yasal | K-204 |
| Z-16 | Geri ödeme — cayma | Teslimden önceki caymada ve hizmette cayma beyanının tarih damgası; teslimden sonraki caymada iade edilen malın firmaya ulaştığı tarih — firmanın teslim alma adımında girdiği tarih, geçmişe dönük olabilir ama kalemin teslim tarihinden önce ve ileri tarihli olamaz; mal beyandan önce ulaştıysa beyanın tarih damgası — ikisinden geç olanı | 14 takvim günü. Teslimden sonraki caymada mal ulaşmadan süre başlamaz — bu bir bekletme hakkı değil, başlangıç kuralıdır | Panel kalan süreyi gösterir; teslimden sonraki caymada mal ulaşana kadar "iade malı bekleniyor" görünür — beyandan geçen gün ve müşterinin on dört günlük gönderme süresi (Z-42; K-494). Süre dolunca kendiliğinden bir işlem başlamaz; gönderme süresi geçtikten sonra firma malı dönmemiş kalemi "mal dönmedi" gerekçesiyle kapatabilir (§10.4.9). Havale hattında IBAN silinmişse süre IBAN beklenirken de işler; panel kalemi "IBAN bekleniyor" olarak ve kalan süreyle gösterir (§7.4.5) | Hayır — yasal | K-491 (K-209'u değiştirir), K-337, K-494, K-496, K-498, K-554 |
| Z-42 | Müşterinin iade malını gönderme süresi — teslimden sonraki cayma | Cayma beyanının tarih damgası | 14 takvim günü | Sistem gönderimi denetlemez ve kendiliğinden bir işlem başlatmaz; mal ulaşana kadar kalem "iade malı bekleniyor" görünür ve geri ödeme süresi malın ulaşmasıyla başlar (Z-16). Süre geçtikten sonra firma malı dönmemiş kalemi "mal dönmedi" gerekçesiyle kapatabilir (§10.4.9; K-496); havale hattında Z-38 bu günden işler (K-497) | Hayır — yasal | K-494, K-496, K-497, K-530 |
| Z-17 | Geri ödeme — iptal edilen ödenmiş sipariş ya da kalem | İptal anı | 14 takvim günü. Kart hattında geri ödemeyi sistem kendiliğinden başlatır; havale hattında firma panelden işler | Havale hattında panel kalan süreyi gösterir | Hayır — yasal | K-291, K-337, K-533 |
| Z-18 | Ayıp talebinin iki yıllık zamanaşımı | Kalemin teslim işaretinin tarihi — fiziksel kalemde teslim tarihi, hizmette "tamamlandı" işareti, dijitalde ödeme onayı. Fiziksel kalemde "sorun bildir" kanalı sipariş "Kargoya verildi"ye geçtiği andan açıktır; teslim tarihi girilmemişse süre işlemez ve riski firmadadır (§7.5.2) | 2 yıl | Sipariş sayfasındaki kanal kapanır: kalem için yeni talep açılamaz ve çözülmüş talep yeniden açılamaz. Müşteri iletişim formundan başvurabilir; ayıp ağır kusur ya da hileyle gizlenmişse zamanaşımı işlemez; firma daha uzun bir sorumluluk üstlendiyse o süre geçerlidir — kanal yine iki yılda kapanır, sonrası iletişim formundan yürür | Hayır — yasal | K-226, K-290, K-297, K-506, K-551 |
| Z-19 | İndirme hakkı | — | Süre sınırı yoktur; adet sınırı vardır (§11) | — | — | K-146 |
| | **Katalog, kupon ve içerik** | | | | | |
| Z-20 | İndirimin tarih aralığı | Firmanın girdiği başlangıç | Firmanın girdiği bitiş | Sistem indirimi kendisi başlatır ve bitirir | Evet — kayıt başına firma girer | K-66 |
| Z-21 | Referans fiyatın geriye bakış penceresi | İndirimin başladığı an | Fiziksel ve dijital üründe geriye doğru 10 gün; geçmişi 10 günden kısa malda varyantın satışa sunulduğu andan beri (§3.9.4). Hizmette ve cayma istisnası sebebi "çabuk bozulan" olan üründe pencere yoktur — referans indirimli fiyattan hemen önceki fiyattır (§3.9.3) | — | Hayır — yasal | K-63, K-67, K-508, K-563 |
| Z-22 | Kuponun tarih aralığı | Firmanın girdiği başlangıç | Firmanın girdiği bitiş | Kupon kabul edilmez | Evet — kayıt başına firma girer | K-75 |
| Z-23 | Duyurunun tarih aralığı | İsteğe bağlı başlangıç | İsteğe bağlı bitiş | Duyuru yalnız aralıkta görünür; firmanın elle kaldırmasına gerek kalmaz | Evet — firma girer | K-269 |
| Z-24 | İleri tarihli yayın | — | Yoktur: ürün ve kurumsal içerik yayına anında girer; tek istisna duyurunun tarih aralığıdır | — | — | K-54, K-278 |
| | **İletişim, bildirim ve kötüye kullanım** | | | | | |
| Z-25 | KVKK başvurusuna cevap | Başvurunun firmaya ulaştığı an | 30 takvim günü | Sistemde sayaç tutulmaz; yükümlülük firmanındır (§12) | Hayır — yasal | K-118, K-305, K-337 |
| Z-26 | Gönderilemeyen e-postanın yeniden denenmesi | İlk gönderimin başarısız olduğu an | Üç yeniden deneme, artan aralıkla (§11) | Siparişin ya da talebin panel satırına "e-posta ulaşmadı" işareti düşer; akış etkilenmez | Hayır — ürün sabiti | K-321, K-462 |
| Z-27 | Deneme limitlerinin penceresi | Kayan pencere — her denemede o andan geriye doğru sayılır (§8.2.1) | §11 | O işlem için geçici engel; pencere içindeki deneme sayısı eşiğin altına inince kendiliğinden açılır. Hesap kilitlenmez | Hayır — ürün sabiti | K-328, K-329, K-332, K-334, K-460 |
| | **Saklama ve imha** | | | | | |
| Z-28 | Sipariş, ödeme ve fatura verisi; sözleşme ve ön bilgilendirme kayıtları; siparişe donmuş yasal metin sürümleri | Siparişin oluştuğu takvim yılının sonu | 10 yıl | Siparişin içindeki kişisel veriler imha edilir ve hesapla bağı kesilir; kalan alanlar ticari kayıt olarak kalır — firmanın elinde eşleşen fatura ya da yazışma durdukça kayıt anonim sayılmaz (§12.2.7). Yalnız ödemesi alınmış — muhasebe kaydına dönüşen — siparişe uygulanır; ödemesi hiç alınmamış sipariş Z-41'i izler | Hayır — yasal | K-353, K-357, K-362, K-449, K-519, K-582, K-583 |
| Z-41 | Ödemesi hiç alınmamış (Başarısız) siparişin kişisel verisi — ön bilgilendirme ve onay kayıtları dahil | Siparişin Başarısız olduğu an | 3 yıl | Kişisel veri periyodik imhayla silinir ve hesapla bağı kesilir; kayıt ölçüler için (ödeme tamamlama oranı) kişisel verisiz kalır (§12.2.7) | Hayır — yasal | K-519, K-353, K-457 |
| Z-29 | Ayıp talebi kaydı | — | Bağlı olduğu siparişin süresini izler | Siparişle birlikte imha edilir | Hayır | K-353 |
| Z-30 | İşlem izi | Satırın yazıldığı takvim yılının sonu | 10 yıl | İz satırları imha edilir; süre boyunca değiştirilemez ve silinemez | Hayır — ticari kayda bağlı | K-355, K-449, K-533 |
| Z-31 | Hesap verisi — giriş bilgileri, şifre, açık oturumlar, adres defteri | — | Hesap durduğu sürece | Hesap silindiğinde **hemen** silinir | Hayır | K-115, K-353 |
| Z-32 | Sepet | — | Süresiz | Hesapla birlikte silinir | Hayır | K-123, K-126, K-353 |
| Z-33 | İletişim talebi — KVKK talebi dahil | Talebin son kapatıldığı an — yeniden açılıp kapatılan talepte son kapatılış; hiç kapatılmayan açık talepte açıldığı an | §11 | Talep silinir — kapatılmamış açık talep de | Hayır — ürün sabiti | K-353, K-357, K-507 |
| Z-34 | Bildirim gönderim kaydı — hesap ve yönetim e-postaları ile firmaya giden bildirimler; yeniden denemeler dahil | Gönderim anı | §11 | Kayıt silinir | Hayır — ürün sabiti | K-353, K-548 |
| Z-43 | Bildirim gönderim kaydı — siparişe bağlı müşteri bildirimleri (B-1…B-15); yeniden denemeler ve panelden yeniden gönderimler dahil | Gönderim anı | 3 yıl | Kayıt silinir | Hayır — yasal | K-548, K-353, K-316, K-522 |
| Z-35 | Havale hattında müşterinin geri ödeme için girdiği IBAN | Beyan anı | Geri ödeme tamamlanana kadar. Malı dönmeyen caymada daha erken silinir: firma kalemi "mal dönmedi" gerekçesiyle kapattığında ya da Z-38 dolduğunda | IBAN silinir; ödemenin yapıldığı kayıt — tarih, tutar, sipariş — ticari kayıt olarak kalır. Mal silmeden sonra ulaşırsa geri ödeme için IBAN müşteriden yeniden istenir: müşteri sipariş sayfasından girer ve yeniden girilen IBAN da geri ödeme tamamlanınca silinir (§7.4.5) | Hayır | K-356, K-497, K-498 |
| Z-36 | Periyodik imha | — | §11 | Süresi dolan veri kendiliğinden çalışan bir işle imha edilir; panelde düğme yoktur | Hayır — ürün sabiti | K-354 |
| Z-40 | Giriş kaydı — başarılı ve başarısız girişler, müşteri ve yönetici: zaman, IP, hesap (e-posta), sonuç | Kaydın oluştuğu an | §11 | Kayıt periyodik imhayla silinir | Hayır — ürün sabiti | K-515, K-353 |
| Z-44 | İmha kaydı — her periyodik imha çalışmasının tarihi, kayıt türü ve imha edilen kayıt sayısı; kişisel veri içermez | Kaydın yazıldığı an | En az 3 yıl | Kayıt silinir | Hayır — yasal | K-552, K-354 |
| Z-37 | Veri ihlalinde Kurul'a bildirim | İhlalin öğrenildiği an | 72 saat | Firmanın yasal yükümlülüğüdür; sistemde sayaç tutulmaz (§12) | Hayır — yasal | K-380 |
| Z-38 | Malı dönmeyen caymada havale hattı IBAN'ının kendiliğinden silinmesi | Müşterinin malı gönderme süresinin dolduğu gün — beyandan on dört gün (§7.4.4) | §11 | Mal hâlâ firmaya ulaşmamışsa IBAN silinir; kalem kapanmaz, cayma geçerliliğini korur ve müşteriye bildirim gitmez. Mal bu süre içinde ulaşırsa silme olmaz ve IBAN geri ödeme tamamlanınca silinir (Z-35). Aynı beyanla girilen IBAN'ı geri ödemesi işlenmeyi bekleyen başka bir kalem kullanıyorsa IBAN o geri ödeme tamamlanana kadar kalır | Hayır — ürün sabiti | K-497, K-494, K-496 |

### 4.3 Kendiliğinden işleyen anlar

Süresi olmayan ama zamanı kural olan anlar; kuralların tam metni gösterilen bölümlerdedir.

- **Sipariş onayı:** sipariş numarası verilir; kalemler, adresler, firma kimliği, yasal metin sürümleri ve kargoya verme sözü donar; stok, hizmet kontenjanı ve kupon hakkı ayrılır (§3.17, §3.23; K-80, K-128, K-140, K-186). Aynı sepete bağlı ödenmemiş önceki sipariş bu anda kendiliğinden iptal edilir (K-182).
- **Ödeme onayı:** ayrılan stok kesin düşer, kupon hakkı kullanılmış sayılır, siparişe giren kalemler sepetten çıkar, kargoya verme süresi başlar ve dijital kalem teslim edilir (§3.12.1, §3.16.12, §3.17.3; K-127, K-128, K-168, K-174).
- **İptal:** iptal edilen kalemin stoğu ve hizmet kontenjanı anında döner; kupon hakkı yalnız siparişin tamamı iptal edildiğinde döner (§7.2.6; K-201, K-202). Müşterinin fiziksel kalemdeki iptal yolu sipariş "Kargoya verildi"ye geçtiği anda kapanır; dijital ve hizmet kaleminin sınırı §7.2.5'tedir (§7.2.1; K-196, K-452).
- **İade:** iade edilen fiziksel kalemin stoğu kendiliğinden dönmez — firma malı teslim alıp kontrol ettikten sonra panelden ekler; caymada hizmet kontenjanı döner, kupon hakkı yalnız siparişin tamamı iade edildiğinde döner (§7.4.7, §7.4.8; K-215, K-216).
- **Kısmi iptal ve iade:** kupon ve kargo ücreti yeniden hesaplanmaz (§7.2.7, §7.4.6; K-202, K-213). Siparişin iptal edilmemiş fiziksel kalemlerinin tamamı kargoya verilmeden iptal edildiğinde ya da kargoyla gönderilen fiziksel kalemlerin tamamından cayıldığında ödenmiş kargo ücreti son işlemin geri ödemesine eklenir (§7.2.9, §7.4.6; K-501).
- **Malı dönmeyen caymada IBAN'ın silinmesi:** havale hattında müşterinin geri ödeme için girdiği IBAN, firma kalemi "mal dönmedi" gerekçesiyle kapattığı anda silinir; firma kapatmazsa ve mal hâlâ ulaşmamışsa, müşterinin gönderme süresinin bitiminden otuz gün sonra kendiliğinden silinir. Silme kalemi kapatmaz ve caymayı etkilemez (§7.4.5, §10.4.9; Z-35, Z-38; K-497).

*Kaynak: K-335…K-339 (sayım kuralları ve çitler) · K-411 (yasal süreler envantere girmez) · K-63 · K-66 · K-67 · K-75 · K-80 · K-102 · K-105 · K-108 · K-115 · K-118 · K-123 · K-126 · K-127 · K-128 · K-139 · K-140 · K-146 · K-165 · K-166 · K-167 · K-168 · K-174 · K-180 · K-182 · K-183 · K-186 · K-196 · K-198 · K-201 · K-202 · K-203 · K-204 · K-205 · K-209 · K-213 · K-215 · K-216 · K-226 · K-232 · K-269 · K-278 · K-288 · K-289 · K-290 · K-291 · K-296 · K-297 · K-305 · K-308 · K-320 · K-321 · K-328 · K-329 · K-332 · K-334 · K-340 · K-353…K-357 · K-362 · K-380 · K-459 · K-449 · K-54 · K-448 · K-460 · K-462 · K-491 · K-494 · K-496 · K-497 · K-498 · K-452 · K-499 · K-529 · K-533 · K-501 · K-503 · K-506 · K-507 · K-530 · K-554 · K-508 · K-515 · K-519 · K-548 · K-549 · K-551 · K-552 · K-556 · K-558 · K-481 · K-522 · K-563.*

## 5. Durum tanımları

> **Ne yazılır:** Ana varlıkların (işlem, hesap, talep vb.) alabileceği durumlar ve geçiş koşulları. Durum makinesi mantığı burada başlar, `03`'te akışa, `06`'da şemaya döner. **Bu isimler tüm dokümanlarda birebir aynı kalır.**

Durum adları ve kod karşılıkları §1.2 sözlüğündedir; `06`, `07` ve `09` onları birebir devralır (K-17). Mağazanın satış açıklığı bir varlık durumu değildir — dört koşula bağlı bir kapıdır ve kuralı §3.1.5'tedir (K-09, K-163, K-235, K-365, K-465).

### 5.1 Ürün ve varyantın yayın durumu

Ürün ve varyant aynı üç değerli seti **ayrı ayrı** taşır: ürünün kendi durumu, varyantın kendi durumu vardır (K-54, K-55).

| Durum | Kod | Ziyaretçi ne görür | Kaynak |
|---|---|---|---|
| Taslak | `Draft` | Ürünün adresi "sayfa bulunamadı" döner; giriş yapmış yönetici sayfayı "Taslak" bandıyla görür. Yayındaki bir ürünün taslak varyantı seçenek listesinde görünmez | K-54, K-57 |
| Yayında | `Published` | Vitrinde, kategori sayfalarında ve aramada görünür | K-54 |
| Arşiv | `Archived` | Listelerde ve aramada görünmez; ürünün adresi "Bu ürün artık satılmıyor" sayfası döner — ad, görsel, durum; fiyat ve sepete ekleme yok. Yayındaki bir ürünün arşivlenmiş varyantı vitrinden kalkar. **Silme değildir**, kayıt korunur | K-54, K-56, K-69, K-130 |

- **Yayında'ya geçişin koşulu (yayın kapısı):** ürünün adı, ana kategorisi ve tipi girilmiş, en az bir Yayında varyantı vardır — taslak varyant ziyaretçiye görünmediği için koşulu karşılamaz (K-86, K-45, K-55, K-538). Dijital üründe ayrıca yayındaki her varyant bir dosyaya bağlıdır (K-145); hizmet ürününde ifa süresi girilmiştir (§3.2.2; K-503).
- **Geçişler serbesttir:** üç durum arasındaki altı geçişin hepsi — Taslak ↔ Yayında, Yayında ↔ Arşiv, Arşiv ↔ Taslak — firmanın panelden yaptığı işlemlerdir; yasak geçiş yoktur ve kural ürün ile varyant düzleminde aynıdır. Tek koşul yayın kapısıdır (K-298, K-245, K-130). Sipariş durumundan bilinçli ayrışmadır: yayın durumu yalnız vitrindeki görünürlüğü değiştirir ve geri alınabilir.
- **Yayın kapısını bozan işlem engellenir:** yayındaki bir ürünün son Yayında varyantı arşivlenemez ve taslağa çekilemez; panel firmayı önce ürünü taslağa ya da arşive almaya yönlendirir (K-299, K-538).
- **Kalıcı silme bir durum değildir:** kayıt katalogdan kalkar, adresi "sayfa bulunamadı" döner (K-85).
- **Olmayan durumlar:** zamanlanmış yayın yoktur (K-54); "fiyat sorunuz" gibi bir ürün durumu yoktur (K-11). Satışın geçici olarak kapatılması yayın durumunu değiştirmez (K-235).
- Tükenmişlik yayın durumu değildir: tükenmiş ürün ve varyant **Yayında** kalır ve "Tükendi" işaretiyle görünür (§3.6.3, §3.6.4).

### 5.2 Kurumsal içeriğin yayın durumu

Kurumsal içerik yayın durumunun **iki değerini** kullanır; her kayıt — Hakkımızda, her hizmet tanıtımı, referans iş, SSS sorusu, şube, genel sayfa ve duyuru — kendi durumunu taşır (K-251, K-273).

| Durum | Kod | Ziyaretçi ne görür | Kaynak |
|---|---|---|---|
| Taslak | `Draft` | Sayfa "sayfa bulunamadı" döner; kayıt menüde, ana sayfada ve listelerde görünmez. Yönetici sayfayı "Taslak" bandıyla önizler; kendi sayfası olmayan kayıtlar göründükleri yerde aynı işaretle önizlenir | K-274 |
| Yayında | `Published` | Kayıt sitede görünür | K-273 |

- **Taslak → Yayında:** zorunlu alanlar doludur — hizmet tanıtımında ad, referans işte ve genel sayfada başlık, SSS'de soru ve cevap, şubede ad ve adres, Hakkımızda'da kısa tanıtım, duyuruda metin (§3.27.13); ikinci bir onay yoktur, her yönetici yayımlar (K-275, K-273, K-584).
- **Yayında → Taslak:** yayından kaldırılan kayıt taslağa döner (K-273).
- **Arşiv yoktur**; ileri tarihli yayın yoktur; sürüm geçmişi yoktur — yayındaki kaydın düzenlemesi kaydedildiği anda yayındadır (K-273, K-276, K-278).
- **Yayın kapısını bozan düzenleme kaydedilmez:** yayındaki kaydın zorunlu alanı boşaltılamaz; panel eksik alanı gösterir ve firmayı önce kaydı taslağa almaya yönlendirir (§3.27.24; K-299'un kalıbı, K-538).
- **Duyuru** tekil bir kayıttır — aynı anda tek duyuru vardır (§3.27.21; K-269). Yayında olduğu hâlde, tarihleri girilmişse yalnız o aralıkta görünür (K-269, K-273).
- Kalıcı silme bir durum değildir; Hakkımızda silinmez, yalnız taslağa alınır (K-277).

### 5.3 Sipariş: iki eksen ve oluşma anı

**5.3.1 Sipariş iki bağımsız eksende durum taşır:** sevkiyat ekseni — Sipariş durumu (`OrderStatus`) — ve ödeme ekseni — Ödeme durumu (`PaymentStatus`). Her siparişin her an iki durumu vardır (K-170). Durum sipariş düzeyindedir; parçalı gönderim olmadığı için sevkiyat tek eksende kalır (K-177). Kalem düzeyinde durum makinesi yoktur, kayıtlar vardır (§5.8).

**5.3.2 Sipariş, müşteri onayladığı anda Alındı + Bekliyor olarak doğar.** Aynı anda sipariş numarası verilir (K-186); kalemler kalem bazında yuvarlanmış tutarlarıyla, adresler, iletişim e-postası, firma kimliği, sözleşme sürümleri ve kargoya verme sözü donar (K-61, K-77, K-78, K-79, K-80, K-113, K-116, K-140, K-189, K-190; §3.23); stok, hizmet kontenjanı ve kupon hakkı ayrılır (K-128). Dijital ürünün dosyası donmaz (K-148).

**5.3.3 Onay anında özet değişmişse sipariş hiç doğmaz** — durum da oluşmaz; müşteri güncel özeti görür (K-129).

**5.3.4 Her ödeme denemesi ayrı bir sipariştir.** Başarısız ödemenin tekrarı yeni bir sipariş açar; eski sipariş İptal edildi olarak kayıtta kalır (K-181). Ödemesi hiç başarılı olmamış ve kendiliğinden iptal edilmiş sipariş (K-180, K-182) müşterinin listesinde görünmez, firmanın panelinde görünür; müşterinin ya da firmanın iptal ettiği ödenmemiş sipariş listede görünmeye devam eder (§3.17.8; K-184, K-528). Ödeme denemesi limitine yalnız ödeme süresinin dolmasıyla ya da kart başarısızlığıyla iptal edilenler sayılır (§8.2; K-332, K-528).

**5.3.5 Sipariş hesabın kaderinden bağımsızdır:** hesap silinse de kendi kaydıyla yoluna devam eder (K-116).

### 5.4 Sevkiyat ekseni — Sipariş durumu

| Durum | Kod | Anlamı | Terminal |
|---|---|---|---|
| Alındı | `Placed` | Siparişin oluştuğu andaki durum. Ödeme onaylanana kadar sürer — kartta dakikalar, havalede günler (K-166); fiziksel kalemi olmayan siparişte bütün kalemler teslim işaretini alana kadar (K-294) | Hayır |
| Hazırlanıyor | `Preparing` | Ödemesi onaylanmış, fiziksel kalem içeren siparişin firma tarafından hazırlandığı durum; kargoya verme süresi bu anda başlar | Hayır |
| Kargoya verildi | `Shipped` | Kargo şirketi + takip numarasıyla ya da "kendi aracımızla teslim" beyanıyla yola çıkmış sipariş; fiziksel kalemin iptal yolu kapanır, cayma düğmesi açılır | Hayır |
| Teslim edildi | `Delivered` | Fiziksel siparişte firmanın teslim işareti ve girdiği teslim tarihi (K-288); fiziksel kalemi olmayan siparişte — fiziksel kalemleri iptal edilmiş karışık sipariş dahil — bütün kalemlerin teslim işareti: dijital kalemde ödeme onayı, hizmet kaleminde firmanın tamamlama işareti; iptal edilmiş ve cayma beyanıyla kapanmış kalem beklenmez (K-294, K-295, K-503) | **Evet** |
| Teslim edilemedi | `DeliveryFailed` | Kargonun ulaştıramayıp firmaya geri döndürdüğü sipariş | Hayır |
| İptal edildi | `Cancelled` | Teslimattan önce müşterinin ya da firmanın iptaliyle, ödeme süresinin dolmasıyla, kart ödemesinin başarısızlığıyla ya da aynı sepetten verilen yeni siparişin onaylanmasıyla kapanan sipariş (K-180, K-182) | **Evet** |

*Kaynak: K-171 (set), K-176 (Teslim edilemedi), K-223 (terminal durumlar), K-288, K-294, K-295 (teslim işareti), K-180, K-182 (kendiliğinden iptal) · K-503 · K-504 · K-505 · K-535 · K-536 · K-537 · K-554 · K-566.*

**İzin verilen geçişler — beyaz liste.** Listede olmayan her geçiş yasaktır (K-224).

| # | Geçiş | Koşul ya da tetikleyici | Kaynak |
|---|---|---|---|
| S1 | Alındı → Hazırlanıyor | Ödeme **Ödendi** olur; sipariş en az bir fiziksel kalem taşır | K-173, K-179 |
| S2 | Alındı → Teslim edildi | Ödeme **Ödendi**'dir ve fiziksel kalemi olmayan siparişin kalemlerinin tamamı teslim işaretini alır — dijital kalem ödeme onayında, hizmet kalemi firmanın "tamamlandı" işaretiyle; işaret ancak ödeme Ödendi iken konur. İptal edilmiş ve cayma beyanıyla kapanmış kalem beklenmez | K-174, K-175, K-294, K-503, K-536 |
| S3 | Alındı → İptal edildi | Müşteri iptal eder; firma sebep seçerek iptal eder; ödeme süresi dolar, kart ödemesi başarısız olur ya da aynı sepetten yeni sipariş onaylanır | K-180, K-182, K-196, K-199 |
| S4 | Hazırlanıyor → İptal edildi | Müşteri iptal eder; firma sebep seçerek iptal eder | K-196, K-199 |
| S5 | Hazırlanıyor → Kargoya verildi | Sipariş iptal edilmemiş fiziksel kalem taşır; firma kargo şirketi + takip numarası ya da "kendi aracımızla teslim" girer | K-141, K-295, K-535 |
| S6 | Kargoya verildi → Teslim edildi | Firma panelden teslim işaretini koyar ve teslim tarihini girer — tarih geçmişe dönük olabilir ama kargoya verildiği günden önce ve ileri tarihli olamaz; otomatik geçiş yoktur | K-224, K-288, K-554 |
| S7 | Kargoya verildi → Teslim edilemedi | Kargo malı ulaştıramayıp firmaya geri döndürür; firma panelden işaretler | K-176 |
| S8 | Teslim edilemedi → Kargoya verildi | Firma yeniden gönderir | K-176 |
| S9 | Teslim edilemedi → İptal edildi | Firma sebep seçerek iptal eder. Firma iptali cayma beyanlı kaleme uygulanmaz: geri dönen cayma kalemini firma teslim alma adımıyla işler ve siparişin kapanışında kalem iptal edilmiş kalem gibi sayılır (§5.8) | K-176, K-199, K-503 |
| S10 | Hazırlanıyor → Teslim edildi | Fiziksel kalemlerin tamamı iptal edilmiş ya da çıkarılmıştır ve kalan dijital ve hizmet kalemlerinin tamamı teslim işaretini almıştır; cayma beyanıyla kapanmış kalem beklenmez | K-295, K-464, K-503, K-537 |
| S11 | Teslim edilemedi → Teslim edildi | Geri dönen gönderinin fiziksel kalemleri iptal edilmiş ya da cayma beyanıyla kapanmıştır ve kalan dijital ve hizmet kalemlerinin tamamı teslim işaretini almıştır; cayma beyanıyla kapanmış kalem beklenmez | K-295, K-464, K-503 |

- **Yasak geçişler özellikle:** geri yönlü her geçiş — Kargoya verildi → Hazırlanıyor, Teslim edildi → herhangi biri (K-224). İptal edilmiş bir sipariş yeniden açılmaz (K-223). Yasak müşteri akışı içindir; yöneticinin yanlış işareti düzeltmesi aşağıdaki maddenin ve §10.4.6'nın sınırlarıyla yapılır (K-504).
- **Bildirimler:** hangi geçişin müşteriye e-posta ürettiği §9.2'dedir; "Hazırlanıyor"a geçiş ve teslim işareti bildirim üretmez (K-317).
- **Yönetici müdahaleleri** geçiş değildir ve tabloya girmez: adres düzeltmesinin ve fiziksel kalem çıkarmanın sınırı "Kargoya verildi"dir — dijital ve hizmet kalemi teslim işaretine kadar çıkarılabilir (K-577); çıkarılan kalem sevkiyat hattında iptal edilmiş kalem gibi sayılır (S3, S4, S10 ve İptal edildi'ye geçiş) ve teslim işaretini almış kalem çıkarılamaz; düzeltmeler §10.4'tedir (K-369, K-374, K-537).
- **Kalem düzeyinde iptal:** siparişin tamamı ancak bütün kalemleri iptal edildiğinde İptal edildi'ye geçer (K-197). Kalem düzeyi iptal ödeme onayından sonra açılır; ödenmemiş sipariş yalnız bütünüyle iptal edilir (§7.2.2; K-505). Fiziksel kalemlerin tamamı iptal edilen karışık sipariş fiziksel kalemi olmayan siparişin hattına düşer; teslim edilmiş dijital kalem iptal edilemediği için sipariş İptal edildi'ye değil, kalan kalemler teslim işaretini aldığında Teslim edildi'ye geçer (S10, S11; K-295, K-200).
- **Yanlış yapılmış geçişin düzeltilmesi** bu tablonun dışındadır: beyaz liste müşteri akışı içindir. Düzeltme yalnız yöneticinin panelinden, sebep seçilerek yapılan **yeni bir geçiştir**; "geri alma" değildir ve izi temizlemez — hem yanlış geçiş hem düzeltme işlem izinde kalır (§10.3.1; K-527). Terminal durumdan — İptal edildi, Teslim edildi — çıkış yalnız bir önceki duruma ve yalnız yanlış işaretin düzeltilmesi için yapılır; düzeltme §5.6.1'in eksen bağını bozamaz ve ödemeyi kendiliğinden değiştirmez. İptal yalnız ödeme Ödendi'de kalmışsa düzeltilir ve düzeltme iptalin serbest bıraktığı ayırmaları yeniden ister; ödemesi Başarısız olmuş ya da parası geri gönderilmiş iptal düzeltilmez (§10.4.6; K-504, K-566). Ayrıca iki sınırı vardır: iptal edilmiş sipariş "ödendi" yapılamaz; dış dünyaya çıkmış sonuç — gönderilen e-posta, kargoya verilen paket, karta gönderilen para — düzeltmeyle geri alınmaz. Düzeltme müşteriye bildirilir (§10.4; K-225, K-372, K-183, K-375).

### 5.5 Ödeme ekseni — Ödeme durumu

| Durum | Kod | Anlamı | Terminal |
|---|---|---|---|
| Bekliyor | `Pending` | Ödemesi henüz gelmemiş sipariş | Hayır |
| Ödendi | `Paid` | Ödemesi onaylanmış sipariş: kartta 3D Secure'dan geçen ödemenin sağlayıcıdan gelen başarı bildirimi, havalede firmanın "ödendi" işareti | Hayır |
| Başarısız | `Failed` | Ödemesi alınmadan kapanan sipariş: ödeme süresi dolmuş, kart ödemesi başarısız olmuş ya da sipariş ödenmeden iptal edilmiş; sonradan gelen ödeme işlenmez | **Evet** |
| Kısmen geri ödendi | `PartiallyRefunded` | Parasının bir kısmı — kalemlerinin bir kısmının parası ya da tutar bazlı bir kısmi geri ödeme — müşteriye geri ödenmiş sipariş | Hayır |
| Geri ödendi | `Refunded` | Parasının tamamı müşteriye geri ödenmiş sipariş | **Evet** |

*Kaynak: K-172 (set), K-222 (Kısmen geri ödendi), K-539 (tutar bazlı kısmi geri ödeme), K-223 (terminal durumlar), K-13 (3D Secure), K-183, K-296 (Başarısız'ın kapsamı) · K-532 (ödeme geçişlerinin `Ö` kimlikleri).*

| # | Geçiş | Koşul ya da tetikleyici | Kaynak |
|---|---|---|---|
| Ö1 | Bekliyor → Ödendi | Kartta sağlayıcının başarı bildirimi; havalede firmanın panelden "ödendi" işareti (geri alınamaz onayıyla — §5.9) | K-13, K-159, K-173, K-225 |
| Ö2 | Bekliyor → Başarısız | Ödeme süresi dolar ya da kart ödemesi başarısız olur — kartta süre dolduğunda iptalden önce sağlayıcıya son sorgu yapılır; ya da ödemesi alınmamış sipariş hangi yoldan olursa olsun iptal edilir: müşteri, firma, aynı sepetten yeni sipariş | K-180, K-232, K-296 |
| Ö3 | Ödendi → Kısmen geri ödendi | Paranın bir kısmı geri ödenir — kalemlerin bir kısmının parası ya da tutar bazlı kısmi geri ödeme | K-222, K-539 |
| Ö4 | Ödendi → Geri ödendi | Siparişin parasının tamamı geri ödenir | K-172, K-222 |
| Ö5 | Kısmen geri ödendi → Geri ödendi | Kalan paranın tamamı geri ödenir | K-222, K-539 |

- **Geri ödeme geçişlerinin tetikleyicisi iki türlüdür:** iptal edilen ödenmiş kart siparişinde ya da kalemde geri ödemeyi sistem kendiliğinden başlatır ve firmaya bildirir; havale hattında, cayma ve iade sonrasında ve ayıp talebinin para gerektiren çözümünde geri ödemeyi firma panelden işler ve bu işlem geri alınamaz onayı ister (§5.9). Ödeme ekseni paranın fiilen gönderildiği anı anlatır (K-208, K-225, K-291). **Tutar bazlı kısmi geri ödeme** dördüncü tetikleyicidir: kalemden bağımsızdır ya da bir kalemin bedelinin bir kısmı kadardır — ayıp talebinde bedel indirimi (§7.5.3) ve yöneticinin indirim için yaptığı kısmi geri ödeme (§10.4.3). Firma panelden işler ve geri alınamaz onayı ister (§5.9); tutar siparişte ödenmiş ve henüz geri ödenmemiş tutarı aşamaz (K-539). Caymada iade malının teslim alma işareti geri ödemeyi kendiliğinden başlatmaz; teslimden sonraki caymada on dört günlük süreyi başlatır (§7.4.1; K-491).
- **Yasak geçişler özellikle:** Başarısız → Ödendi — süresi dolmuş siparişe sonradan gelen ödeme işlenmez (K-183, K-224). İptal edilmiş siparişe gelen kart ödemesi sistem tarafından kendiliğinden geri ödenir; siparişin durumları değişmez (K-233).
- **Kart iadesi sağlayıcıda başarısız olursa ödeme ekseni değişmez:** geçiş yalnız paranın fiilen gönderildiği anda olur. Siparişin panel satırına "geri ödeme gerçekleşmedi" işareti düşer ve firma yeniden dener ya da müşteriye havale yolunu açar — havale müşterinin seçimidir (§6.4.24, §7.2.8; K-521, K-569). Ekseni yeni bir "iade başarısız" durumu yoktur.
- **Ödeme ekseninde tek düzeltme yanlış konmuş havale "ödendi" işaretidir:** yöneticinin düzeltmesiyle Ödendi'den Bekliyor'a, sevkiyatla birlikte döner; beyaz listenin dışındadır ve koşulları §10.4.6'dadır (K-570).
- **Ödeme eksenine "İptal edildi" durumu eklenmez:** iptalin sebebi siparişin iptal kaydından okunur (K-296).
- Sistemde "eksik ödeme" diye bir durum yoktur (K-169).

### 5.6 İki eksenin bağı ve kendiliğinden geçişler

**5.6.1 İki eksen tek bir kapı koşuluyla bağlıdır:** sipariş Hazırlanıyor'a ancak ödeme Ödendi olduğunda geçer. Bu an kargoya verme süresinin başladığı andır — kartta ödeme başarısı, havalede firmanın "ödendi" işareti (K-173, K-168). Kapının yanında iki ekseni birlikte değiştiren **eşzamanlı kendiliğinden geçişler** vardır: fiziksel kalemsiz siparişte ödeme onayı dijital kalemleri teslim eder ve yalnız dijital siparişi Teslim edildi'ye alır; ödenmeden iptal ödemeyi Başarısız yapar (§5.6.2–§5.6.5; K-174, K-294, K-296, K-531). Bunların dışında eksenler bağımsızdır: geri ödeme, sipariş hangi sevkiyat durumunda olursa olsun ödeme ekseninde yaşanır ve sevkiyat eksenini değiştirmez (K-173, K-223). Fiziksel kalemsiz siparişte teslim işaretleri ödemeden sonra gelir; bu yüzden **Teslim edildi + Bekliyor** ve **Teslim edildi + Başarısız** yasak birleşimlerdir — hizmet kalemi ancak ödeme Ödendi iken "tamamlandı" işaretlenir (§3.20.9; K-536). Yöneticinin durum düzeltmesi bu bağı bozamaz: ödeme Ödendi'den Bekliyor'a yalnız yanlış konmuş havale "ödendi" işaretinin düzeltilmesinde ve sevkiyatla birlikte döner — Hazırlanıyor'daki sipariş aynı adımda Alındı'ya geçer (§10.4.6; K-504, K-570).

**5.6.2 Yalnız dijital siparişte ödemenin onayı sevkiyatı doğrudan Teslim edildi'ye alır** (K-174).

**5.6.3 Ödeme süresi dolduğunda ya da kart ödemesi başarısız olduğunda — hangisi önce gelirse — sipariş kendiliğinden İptal edildi, ödemesi Başarısız olur;** ayrılan stok, hizmet kontenjanı ve kupon hakkı aynı anda serbest kalır. Firmanın elle iptal etmesi beklenmez (K-180). Kart ödemesinde süre dolduğunda, bu iptalden önce sağlayıcıya ödemenin sonucu son bir kez sorulur; ödeme alınmışsa sipariş iptal edilmez ve ödeme Ödendi'ye geçer (K-232). Sorgu yanıtsız kalırsa — sağlayıcı hâlâ erişilemezse — sipariş de iptal edilmez: ödeme Bekliyor'da, ayrılanlar tutulu kalır ve sorgu sağlayıcı yanıt verene kadar yinelenir (§6.1.2, Z-7; K-556).

**5.6.4 Müşteri aynı sepetten yeni bir siparişi onayladığında o sepete bağlı ödenmemiş önceki sipariş kendiliğinden iptal edilir** ve ayırması serbest kalır (K-182).

**5.6.5 Ödenmeden iptal edilen siparişin ödemesi Başarısız olur** — müşterinin ya da firmanın iptalinde ve §5.6.4'ün kendiliğinden iptalinde de. "Başarısız" burada *"para gelmedi"* demektir; sebep siparişin iptal kaydındadır (K-296).

**5.6.6 Cayma beyanı kendi başına hiçbir ekseni değiştirmez:** hakkın kullanıldığını kayda geçirir ve iade sürecini başlatır; ödeme ekseni paranın geri ödendiği anda değişir (K-208). Kargoya verilmeden yapılan beyanın etkisi §7.3.7'dedir (K-452). Teslim işaretini almamış kalemde — tamamlanmamış hizmet kalemi, kargodayken cayılan fiziksel kalem — beyan kalemi kapatır (§5.8); siparişin kalan kalemleri teslim işaretini almışsa sevkiyat ekseni bu kapanışla S2, S10 ya da S11'den geçer (K-503).

### 5.7 Tipe göre sevkiyat hattı

Üç ürün tipi aynı sepetten geçer ve aynı şekilde satın alınır (K-81); sevkiyat ekseninde ayrıldıkları yer teslimdir. Tipler için ayrı bir durum seti yoktur — hatların farkı **kullanılmayan durumlarla** anlatılır (K-174).

| Siparişin kalemleri | Sevkiyat hattı | Kalem düzeyi | Kaynak |
|---|---|---|---|
| En az bir fiziksel kalem | Alındı → Hazırlanıyor → Kargoya verildi → Teslim edildi | Dijital kalem ödeme onayında indirilebilir olur; hizmet kalemi firma "tamamlandı" işaretlediğinde teslim edilmiş olur — ikisi de siparişin hattını beklemez | K-171, K-179 |
| Yalnız dijital | Alındı → Teslim edildi, ödeme onayında; Hazırlanıyor ve Kargoya verildi kullanılmaz. Havalede teslim firmanın "ödendi" işaretine bağlıdır | — | K-174 |
| Yalnız hizmet | Alındı → Teslim edildi, firma "tamamlandı" işaretlediğinde; ödeme onayı ile tamamlama arasında sipariş Alındı'da bekler | — | K-175 |
| Dijital + hizmet, fiziksel kalem yok | Hazırlanıyor ve Kargoya verildi kullanılmaz; sipariş Alındı'da bekler ve kalemlerin tamamı teslim işaretini aldığında Teslim edildi'ye geçer | Dijital kalem ödeme onayında, hizmet kalemi "tamamlandı" işaretinde teslim işaretini alır | K-294 |
| Fiziksel kalemlerinin tamamı iptal edilmiş karışık sipariş | Bulunduğu durumdan — Alındı, Hazırlanıyor ya da Teslim edilemedi — kalan kalemlerin tamamı teslim işaretini aldığında Teslim edildi'ye geçer (S2, S10, S11) | "En az bir fiziksel kalem" koşulu "iptal edilmemiş fiziksel kalem" diye okunur | K-295, K-464 |

### 5.8 Sipariş kalemi düzeyindeki kayıtlar

Kalem ayrı bir durum makinesi taşımaz (K-179); aşağıdaki kayıtları taşır:

- **Teslim işareti ve teslim tarihi** — fiziksel kalemde firmanın teslim işaretiyle girdiği tarih; sipariş düzeyinde bir kez girilir ve fiziksel kalemlerin tamamına aynı yazılır (§3.20.11; K-288, K-177), dijital kalemde ödeme onayı, hizmet kaleminde tamamlama işareti. Cayma penceresi fiziksel kalemin teslim tarihinden işler (K-203); ayıp talebinin iki yılı üç tipte de bu işaretin tarihinden başlar (K-179, K-290).
- **İptal kaydı** — kalem tek tek iptal edilir; firma iptalinde sebep kayda geçer (K-178, K-197, K-199).
- **Çıkarma kaydı** — yöneticinin kalemi çıkarması ya da adedini azaltması; işlem izine yazılır. Çıkarılan kalem sevkiyat hattında iptal edilmiş kalem gibi sayılır; teslim işaretini almış kalem çıkarılamaz (§10.4.3; K-537).
- **Gecikme feshi** — otuz günlük yasal teslim sınırı aşıldıktan sonra müşterinin sipariş sayfasından yaptığı fesih; tarih damgasıyla, teslim edilmemiş fiziksel kalemlerin tamamına düşer. İptal kaydı ve cayma beyanından ayrıdır; cayma beyanı gibi sevkiyat eksenini değiştirmez, geri ödeme ödeme ekseninde yaşar (§7.2.3; K-499).
- **Cayma beyanı** — tarih damgasıyla; pencerenin içinde yapılıp yapılmadığı bu damgadan okunur (K-207). Beyan müşterinin sipariş sayfasından ya da — başka bir kanaldan gelen bildirimde — firmanın panelden kaydıyla doğar; ikincisinde damga bildirimin firmaya ulaştığı tarihtir (§10.4.10; K-510). Pencere fiziksel kalemde teslim tarihinden, hizmet kaleminde sipariş tarihinden — fiziksel kalem içeren siparişte siparişin teslim tarihinden — işler; beyanın açık olduğu aralık §7.3.1, §7.3.3 ve §7.3.7'dedir (K-203, K-289, K-340, K-452, K-503). Teslim işaretini almamış kalemde beyan kalemi kapatır — bu ayrı bir kalem durumu değildir, cayma beyanı kaydının sonucudur: tamamlanmamış hizmet kalemi tamamlanmayı beklemez; kargodayken cayılıp geri dönen fiziksel kalemi firma iptal etmez, teslim alma adımıyla işler. S2, S10 ve S11 bu kalemi beklemez; geri ödeme tek hattan, caymanın hattı ve süresiyle yürür (§4 Z-16; K-503).
- **İade teslim alma** — fiziksel kalemde firmanın iade malını teslim alma adımında girdiği ulaşma tarihi — geçmişe dönük olabilir, ileri tarihli olamaz — ve stoğa ekleme; teslimden sonraki caymada geri ödeme süresinin başlangıcıdır (§7.4.1, §7.4.7; K-491, K-215).
- **"Mal dönmedi" kapanışı** — teslimden sonra yapılan caymada, müşterinin gönderme süresi geçtikten sonra firmanın malı dönmemiş kaleme koyduğu kapanış; işlem izine yazılır. Kalem bekleyen işlerden düşer; mal sonradan ulaşırsa teslim alma adımıyla yeniden açılır (§10.4.9; K-496). Havale hattında kapanış, müşterinin geri ödeme için girdiği IBAN'ı siler (§7.4.5; K-497).
- **IBAN isteği** — havale hattında IBAN silindikten sonra mal firmaya ulaştığında, firmanın teslim alma adımıyla kaleme düşen istek ve tarihi — firma başka kanaldan gelen, IBAN taşımayan cayma bildirimini kaydettiğinde de kayıtla birlikte düşer (§10.4.10; K-581); müşteri IBAN'ı sipariş sayfasından girene kadar kalem "IBAN bekleniyor" görünür ve istek işlem izine yazılır (§7.4.5, §10.3.1; K-498).
- **Ayıp talebi** — kalem ve sipariş bağlamıyla; çözülse de yeniden açılabilir (K-227, K-297; §5.10).
- **İndirme sayacı** — dijital kalemde (K-146; §3.12.5).

İptal ve iade kalem düzeyinde işler; siparişin kalan kalemleri yoluna devam eder ve para kısmen döner (K-178).

### 5.9 Geri alınamaz geçişler

Dört geçiş panelde "geri alınamaz" onayı ister: **havale siparişinde "ödendi" işareti** (Ö1) · **kargoya verme** (S5) · **firma kaynaklı iptal** (S3, S4, S9) · **geri ödemenin firma tarafından işlenmesi** (Ö3–Ö5). Firma işlemi onaylamadan önce sonucunu tek cümleyle görür ("Müşteriye takip numarası gönderilecek. Bu işlem geri alınamaz."). Kart hattında karta para gönderen kalem çıkarması (§10.4.3) firma iptalinin kalıbıyla aynı onayı ister (K-537). Sistemin iptalde kendiliğinden başlattığı karta geri ödeme bir onay penceresi değil, firmaya bildirim üretir (K-291, K-233). **Panel "geri al" düğmesi sunmaz;** yanlış yapılmış bir geçiş, sebep seçilerek yapılan yeni bir geçişle düzeltilir ve iz temizlenmez (§5.4, §10.4; K-225, K-372).

### 5.10 Ayıp talebinin durumu

| Durum | Kod | Anlamı |
|---|---|---|
| Açık | `Open` | Müşterinin bildirdiği, firmanın henüz çözmediği talep; talep bu durumda doğar |
| Çözüldü | `Resolved` | Firmanın çözüldü olarak işaretlediği talep |

- **Açık → Çözüldü:** firma talebi çözüldü olarak işaretler; seçimlik hakların — onarım, değişim, bedel indirimi, sözleşmeden dönme — yürütümü sistem dışındadır (K-228).
- **Çözüldü → Açık:** çözüm tutmadığında — onarılan mal yine bozulduğunda, değişimi de ayıplı çıktığında — firma ya da müşteri talebi yeniden açar; yeni bir talep açılmaz ve ayıbın geçmişi tek kayıtta kalır. Geçiş kalemin iki yıllık süresi içinde serbesttir (K-297, K-290). Sipariş durumundaki geri yön yasağından bilinçli ayrışmadır: talebin durumu yalnız firmanın takip listesini değiştirir.

Kodlar K-455 (`DefectClaimStatus`, `Resolved`) ve K-303 (`Open`) ile kayda geçmiştir.

### 5.11 İndirim ve kuponun zamana ve kullanıma bağlı geçerliliği

**5.11.1 İndirim tarih aralığında kendiliğinden işler:** sistem indirimi başlangıçta başlatır, bitişte bitirir; aralık içinde uygulanan indirimli fiyat fiyat geçmişine yazılır ve referans fiyatın hesabına girer (K-63, K-66). Onay anında indirimin başlaması ya da bitmesi fark sayılır (K-129).

**5.11.2 Kupon, tarih aralığının içinde ve kullanım hakkı kaldıkça geçerlidir** (K-70, K-75).

**5.11.3 Kuponun kullanım hakkının ömrü:** sipariş onaylandığında **ayrılır** → ödeme başarılı olduğunda **kullanılmış** sayılır → ödeme süresi dolar, ödeme başarısız olur, siparişin tamamı iptal ya da iade edilirse **geri döner**. Kısmi iptalde ve kısmi iadede hak geri dönmez (K-76, K-128, K-201, K-202, K-216).

**5.11.4 Kuponun kalemlere dağıtılan payı sipariş anında donar** ve geri ödeme tutarının hesabında kullanılır (K-72, K-77, K-202).

### 5.12 Hesap ve oturum

**5.12.1 Hesabın durum makinesi yoktur.** Tek tip hesap vardır; doğrulanmamış hesap bir kullanım durumu değil bir bekleme aşamasıdır — e-posta doğrulanana kadar giriş ve sipariş yoktur; doğrulama bağlantısının ömrü dolarsa kayıt silinir (K-101, K-102). Hesap silindiğinde kapanır (§3.15.1).

**5.12.2 Oturum "beni hatırla" seçimine göre kısa ya da uzun ömürlü açılır** (K-108). Şifre sıfırlandığında hesabın bütün oturumları kapanır (K-106); hesap silindiğinde açık oturumlar hemen silinir (K-115); kaldırılan yöneticinin açık oturumları anında sonlanır (K-314).

**5.12.3 Deneme limiti hesabın durumunu değiştirmez.** Limit aşıldığında yalnız o işlem geçici olarak engellenir; hesap kilitlenmez ve kilidi açacak bir yönetici işlemi yoktur (§8; K-329).

### 5.13 İletişim talebinin durumu

| Durum | Kod | Anlamı |
|---|---|---|
| Açık | `Open` | Formdan gelen, firmanın henüz kapatmadığı talep; talep bu durumda doğar |
| Kapatıldı | `Closed` | Firmanın panelden kapattığı talep |

- **Açık → Kapatıldı:** firma talebi panelden kapatır. Cevap sistemin dışında e-postayla verildiği için "Cevaplandı" diye bir ara durum yoktur (K-303, K-305).
- **Kapatıldı → Açık:** firma kapatılmış talebi yeniden açar — ayıp talebinin kalıbı. Müşterinin talebi görebileceği bir yer olmadığı için yeniden açma firmanın işlemidir (K-303, K-307).
- **KVKK talebi aynı iki durumu kullanır;** ayrı durum makinesi ve süre sayacı yoktur (K-118, K-303).
- **Saklama süresi talebin son kapatıldığı andan işler** — yeniden açılıp kapatılan talepte son kapatılıştan; hiç kapatılmayan açık talep, açıldığı andan itibaren aynı sürenin sonunda imha edilir. KVKK talebi dahil; firmanın kapatmayı unutması veriyi süresiz bırakmaz (§4, Z-33; K-353, K-507).

*Kaynak: K-170…K-176 · K-222…K-225 (sipariş durum makinesi) · K-531, K-532 (eksen bağı ve geçiş kimlikleri) · K-54 · K-55 · K-251 · K-273…K-275 (yayın durumları) · K-228 (ayıp talebi) · K-09 · K-11 · K-13 · K-17 · K-45 · K-56 · K-57 · K-61 · K-63 · K-66 · K-69 · K-70 · K-72 · K-75 · K-76 · K-77 · K-78 · K-79 · K-80 · K-81 · K-85 · K-86 · K-101 · K-102 · K-106 · K-108 · K-113 · K-115 · K-116 · K-128 · K-129 · K-130 · K-145 · K-146 · K-148 · K-159 · K-163 · K-166 · K-168 · K-169 · K-177 · K-178 · K-179 · K-180 · K-181 · K-182 · K-183 · K-184 · K-186 · K-189 · K-190 · K-196 · K-197 · K-199 · K-201 · K-202 · K-203 · K-207 · K-208 · K-216 · K-227 · K-232 · K-233 · K-235 · K-245 · K-269 · K-276 · K-277 · K-278 · K-118 · K-141 · K-200 · K-288 · K-289 · K-290 · K-291 · K-294 · K-295 · K-296 · K-297 · K-298 · K-299 · K-303 · K-305 · K-307 · K-314 · K-317 · K-329 · K-332 · K-340 · K-353 · K-369 · K-372 · K-374 · K-375 · K-455 · K-464 · K-452 · K-491 · K-496 · K-497 · K-498 · K-365 · K-465 · K-499 · K-528 · K-527 · K-140 · K-215 · K-503 · K-504 · K-505 · K-507 · K-535 · K-536 · K-537 · K-538 · K-539 · K-510 · K-521 · K-556 · K-566 · K-570 · K-584.*

## 6. Hata ve istisna senaryoları

> **Ne yazılır:** Her ana akış için "ne ters gidebilir" ve sistemin cevabı. Happy path kadar detaylı.

Envanter §2.1'in sekiz akışına göre dizilir; önce bütün akışlara uygulanan ilkeler gelir. Kuralı §3'te ya da §7'de yaşayan senaryonun cevabı burada bir cümleyle yazılır ve kurala bağlanır; kuralın tam metni orada kalır. Kullanıcıya gösterilen metinlerin biçimi ve yeri `04`'ün işidir.

### 6.1 Bütün akışlara uygulanan ilkeler

**6.1.1 Hiçbir sipariş ya da talep akışı bir e-postanın ulaşmasına bağlı değildir.** E-posta gönderilemediğinde sipariş, ödeme ve durum geçişleri etkilenmez; müşteri sipariş sayfasında her bilgiye — durum, takip bağlantısı, indirme düğmesi, IBAN — ulaşır. Misafir alıcı sipariş sayfasına sipariş numarası + e-postayla girer; e-postayı yanlış yazdığını onaydan sonra fark ederse — sayfaya giden iki yolu da, numara + e-posta ve e-postadaki bağlantı, o adresten geçer — firmaya başka bir kanaldan ulaşır ve firma siparişin e-postasını düzeltir (§10.4.11; K-585). Gönderilemeyen e-posta üç kez, artan aralıkla yeniden denenir; üçü de başarısız olursa siparişin ya da talebin panel satırına "e-posta ulaşmadı" işareti düşer — işaret işlem izine yazılmaz (K-234, K-187, K-321). İşaretli satırda firma e-postayı panelden **yeniden gönderebilir** — en az sipariş e-postası (B-1) için; gönderilen metin siparişe donmuş sürümdür ve yeniden gönderim gönderim kaydına girer (Z-43). Art arda başarısız gönderimde panelin ana sayfasında kanal düzeyinde bir uyarı çıkar (*"e-posta gönderilemiyor — kurulum ayarını kontrol edin"*); eşik `05`'in tasarım girdisidir ve uyarı satış kapısına bağlanmaz — firma isterse satışı kendisi geçici kapatır (§3.1.6, §9.1.6, §10.6.1; K-522). **Hesap ve yönetim bağlantıları kuralın dışındadır:** e-posta doğrulaması, şifre sıfırlama, yeni e-posta adresinin doğrulanması ve yönetici daveti, adresin sahibini kanıtlamak için bağlantının ulaşmasına bilerek bağlıdır. Bağlantı ulaşmazsa kullanıcı onu ekrandan yeniden ister; davette mevcut bir yönetici yeni davet gönderir (§9.4; K-101, K-105, K-110, K-308).

**6.1.2 Sistem, müşterinin ödediği bir siparişi kendi bilgisizliği yüzünden iptal etmez.** Kart ödemesinde ödeme süresi dolduğunda kendiliğinden iptal işlemeden önce sağlayıcıya ödemenin sonucu son bir kez sorulur; ödeme alınmışsa sipariş iptal edilmez ve Ödendi'ye geçer. **Son sorgu yanıtsız kalırsa** — sağlayıcı hâlâ erişilemezse — sipariş iptal edilmez: ödeme Bekliyor'da kalır, ayrılanlar tutulur ve sorgu sağlayıcı yanıt verene kadar yinelenir; sonuç gelince olağan geçiş işler ve sipariş panelde "ödeme sonucu alınamadı" işaretiyle görünür. Sonradan gelen başarı bildirimi iptal edilmiş bir siparişe düşerse §6.2.16'nın kendiliğinden geri ödemesi uygulanır. **Kalan risk:** uzun bir sağlayıcı kesintisinde stok ve kupon hakları kesinti boyunca kilitli kalır. Sorgunun teknik biçimi `08`'in işidir (Z-7; K-232, K-556).

**6.1.3 Onaylanan özet ile oluşan sipariş birebir aynıdır.** Özeti değiştiren her fark siparişin oluşmasını durdurur ve müşteriye neyin değiştiği söylenir (§3.17.2; K-129).

**6.1.4 Sepet hatalarda korunur.** Ödeme yarıda kaldığında, ödeme sağlayıcısına erişilemediğinde ve satış kapalıyken sepet olduğu gibi durur (K-127, K-231, K-235).

**6.1.5 Limit aşımı nötr konuşur.** Bir deneme limiti aşıldığında kullanıcı *"Çok fazla deneme yapıldı, birazdan tekrar deneyin"* mesajını görür; mesaj ne kalan deneme sayısını ne engelin süresini ne de hangi eksende engellendiğini söyler. Hesap kilitlenmez (§8; K-329).

**6.1.6 Hata ve uyarının anlamı yalnız renkle taşınmaz;** her hata, uyarı ve "Tükendi" işareti metin de taşır (§3.34.5; K-395).

### 6.2 Satın alma (akış 1)

| # | Ne ters gider | Sistemin cevabı | Kaynak |
|---|---|---|---|
| 6.2.1 | Kart ödemesinde 3D Secure doğrulaması başarısız olur ya da kart ödemesi reddedilir | Ödeme gerçekleşmez ve sipariş ödenmiş sayılmaz. Sipariş kendiliğinden İptal edildi, ödemesi Başarısız olur; ayrılanlar aynı anda serbest kalır. Sepet olduğu gibi durur; müşteri sepetten yeni bir siparişle yeniden dener | K-13, K-127, K-180, K-181 |
| 6.2.2 | Müşteri banka ekranını kapatır ya da ödeme yarıda kalır | Sepet olduğu gibi durur. Ödeme süresi dolunca kartta sonuç son kez sorulur (§6.1.2); ödeme yoksa sipariş kendiliğinden iptal olur ve ayrılanlar serbest kalır. Müşteri aynı sepetten yeniden onaylarsa önceki ödenmemiş sipariş hemen iptal edilir; müşteri kendi ayırdığı stoğa takılmaz | K-127, K-180, K-182, K-232 |
| 6.2.3 | Onay anında fiyat, indirim, kupon, kargo ücreti, ücretsiz kargo eşiği ya da teslimat illeri değişmiştir veya bir kalem ayrılamaz | Sipariş oluşmaz; müşteri neyin değiştiğini söyleyen güncel özeti görür ve isterse yeniden onaylar. Ayrılamayan kalem için sayı söylenmez (§3.17.2) | K-129, K-133, K-150 |
| 6.2.4 | Sepetteki kalem tükenmiştir ya da hizmetin kontenjanı dolmuştur | Kalem "Tükendi" işaretiyle sepette kalır ve siparişe girmez; sipariş kalan kalemlerle devam eder ve fark güncel özette söylenir — özet gösterildikten sonra oluşan fark siparişi durdurur ve müşteri yeniden onaylar (§3.16.6, §3.17.2) | K-130, K-129 |
| 6.2.5 | Sepetteki ürün ya da varyant arşive ya da taslağa alınmış veya silinmiştir | Kalem sepetten çıkar ve müşteriye bir kez söylenir (§3.16.7) | K-130 |
| 6.2.6 | Müşteri stoktan fazla adet seçer ya da stok sepetteki adedin altına düşer | "Bu adette stok yok" — adet söylenmez; kalem adet düşürülene kadar siparişe girmez (§3.16.8) | K-150 |
| 6.2.7 | Müşteri bir siparişte alınabilecek adedi aşar | Ürün sepete eklenmez ve sınır sayısıyla söylenir; sonradan aşılan sınırda kalemler mesajla sepette kalır (§3.16.9, §3.16.10) | K-151, K-152 |
| 6.2.8 | Aynı dijital varyant sepete ikinci kez eklenir | "Bu ürün zaten sepetinde" (§3.12.4) | K-153 |
| 6.2.9 | Üye, daha önce aldığı dijital varyantı yeniden sepete ekler | Engellenmez; "Bu ürünü daha önce aldınız" uyarısı gösterilir (§3.12.9) | K-236 |
| 6.2.10 | Teslimat adresi firmanın teslimat yapmadığı bir ildedir | Sipariş verilemez ve sebebi söylenir; defterdeki o adres ödeme adımında seçilemez (§3.20.1) | K-133 |
| 6.2.11 | Fiziksel kalemli sepetin tutarı asgari sipariş tutarının altındadır | Sipariş verilemez; eksik tutar gösterilir (§3.18.1) | K-155 |
| 6.2.12 | Kupon sepet toplamına eşittir ya da onu aşar, tarih aralığının dışındadır ya da kullanım hakkı dolmuştur | Kupon kabul edilmez; kupon siparişi sıfır tutara indiremez (§3.10.4, §3.10.5) | K-73, K-75, K-565 |
| 6.2.13 | Misafir alıcı e-posta adresini yanlış yazar | Onay özetinde adres açıkça gösterilir ve onaydan önce düzeltilebilir; doğrulama kodu istenmez (§3.24.5). Hata onaydan sonra fark edilirse müşteri sipariş sayfasına giremez — numara + e-posta sorgusu da e-postadaki bağlantı da o adrese bağlıdır. Müşteri firmaya telefonla, iletişim formuyla ya da e-postayla ulaşır; firma kimliği siparişteki bilgilerle teyit eder ve siparişin e-postasını panelden düzeltir: yeni adrese yeni bağlantıyla sipariş e-postası, eski adrese B-15 gider (§10.4.11). Sipariş yanlış adrese ait bir hesaba düşmüşse bağ kesilir (§3.13.3) | K-193, K-234, K-585 |
| 6.2.14 | Hesabı olan kişi giriş yapmadan misafir olarak sipariş verir | Sipariş kabul edilir ve hesaba anında düşer; ödeme adımında giriş hatırlatması gösterilir (§3.13.3) | K-99 |
| 6.2.15 | Ödeme sağlayıcısına erişilemez | Kart yöntemi "şu an kullanılamıyor" olarak görünür ve seçilemez. Havale açıksa müşteri ona yönlendirilir; değilse sipariş verilemez ve müşteri daha sonra denemesi söylenerek sepetine döner — sepet korunur. Ödeme sayfasına hiç gidilmemiş siparişte ayrılmış bir şey varsa hemen serbest kalır; ödeme sayfasına gidilmiş kart siparişinde ayrılanlar süre dolana ve son sorgu yanıt alana kadar tutulur (§6.1.2). Erişilemezliğin tespiti `08`'in işidir | K-231, K-556 |
| 6.2.16 | İptal edilmiş bir siparişe sağlayıcıdan başarılı ödeme bildirimi gelir — iki sekmeden aynı sepetle ödeme, kendiliğinden iptalden sonra tamamlanan eski deneme | Sistem ödemeyi sağlayıcı üzerinden kendiliğinden geri öder ve firmaya panelde bildirir; siparişin durumları değişmez. Havale hattında aynı durum firmanın elle yaptığı geri ödemeyle çözülür (§3.21.8). Sağlayıcıya geri ödeme çağrısının biçimi `08`'in işidir | K-233, K-183 |
| 6.2.17 | Satış kapalıdır — kimlik eksik, açık ödeme yöntemi yok, yasal metinler tamamlanmamış ya da firma satışı geçici olarak kapatmış | Ürünler görünür; sepete ekleme ve ödeme kapalıdır ve ziyaretçi satışın kapalı olduğunu görür; sepet korunur (§3.1.5) | K-09, K-163, K-235, K-365, K-465 |
| 6.2.18 | Sepette art arda geçersiz kupon kodu denenir | Eşik aşıldığında o sepetin kupon alanı geçici olarak kapanır; mesaj nötrdür (§6.1.5, §8) | K-334, K-329 |
| 6.2.19 | Aynı e-posta adresi (yalnız giriş yapmış üyenin siparişleri) ya da aynı IP üzerinden — iki eksen ayrı ayrı — ödeme süresi dolarak ya da kart başarısızlığıyla kendiliğinden iptal edilen ödenmemiş siparişler eşiği aşar | Kullanıcı bir süre yeni sipariş onaylayamaz; firma o siparişleri panelde görmeye devam eder. Misafir siparişi e-posta ekseninde sayılmaz — başkasının e-postasını yazan biri onu engelleyemez (§8.2.2) | K-332, K-184, K-528, K-579 |
| 6.2.20 | Müşteri siparişe not ya da teslimat talimatı eklemek ister | Ödeme adımında not alanı yoktur; müşteri iletişim formunun "Sipariş hakkında" tipiyle yazar (§3.24.7) | K-389 |
| 6.2.21 | Aynı e-posta adresi (yalnız giriş yapmış üyenin siparişleri) ya da aynı IP üzerinden — iki eksen ayrı ayrı — açık ödenmemiş siparişler tavana ulaşmışken yeni havale siparişi verilmek istenir | Havale yöntemi seçilemez ve mesaj nötrdür; kart yolu açık kalır. Açık siparişlerden biri ödenince ya da iptal edilince tavan boşalır (§3.17.8, §8 L-8) | K-524, K-332, K-579 |

### 6.3 Sipariş takibi (akış 2)

| # | Ne ters gider | Sistemin cevabı | Kaynak |
|---|---|---|---|
| 6.3.1 | Sipariş e-postası müşteriye ulaşmaz | Müşteri her bilgiye sipariş sayfasından ulaşır; misafir alıcı sayfaya sipariş numarası + e-postayla girer (§6.1.1). E-posta yanlış yazıldığı için ulaşmıyorsa bu yol da kapalıdır; firma siparişin e-postasını düzeltir (§6.2.13, §10.4.11) | K-234, K-187, K-585 |
| 6.3.2 | Misafir alıcı sipariş numarasını ya da e-postayı yanlış girer | Sipariş sayfası açılmaz. Art arda başarısız sorgular IP başına limitlidir; eşik aşılınca sorgu geçici olarak engellenir (§3.22.3, §8) | K-187, K-328, K-330 |
| 6.3.3 | Hesap silinmiştir ama sipariş yürümektedir | Takip, cayma ve iade misafir yolundan — sipariş numarası + siparişe donmuş e-posta — sürer (§3.15.2) | K-116 |
| 6.3.4 | Müşterinin ödemesi hiç başarılı olmamış ve kendiliğinden iptal edilmiş siparişleri vardır | Müşterinin sipariş listesinde görünmezler; firmanın panelinde görünürler. Müşterinin ya da firmanın iptal ettiği ödenmemiş sipariş listede görünmeye devam eder (§3.17.8) | K-184, K-528 |
| 6.3.5 | Misafir siparişinin e-postası, gerçek sahibi istemeden düzeltilir — firmayı kandıran biri e-postayı kendi adresine çevirtir | Eski adrese B-15 gider: siparişin e-postasının değiştirildiğini söyler, yeni adresi ve bağlantıyı taşımaz. Gerçek sahibi firmaya ulaşır, firma e-postayı yeniden düzeltir; yeni bir anahtar üretilir ve öncekiler geçersizleşir. Arada sayfaya erişen kişinin yaptığı işlemler — iptal, cayma, IBAN girişi ya da düzeltmesi — geri alınmaz ve işlem izinden okunur; kandırılmanın sonucu firmadadır (§10.4.11, §8.3.6) | K-585 |

### 6.4 İptal, cayma ve iade (akış 3)

| # | Ne ters gider | Sistemin cevabı | Kaynak |
|---|---|---|---|
| 6.4.1 | Müşteri, sipariş kargoya verildikten sonra iptal etmek ister | İptal yolu kapanmıştır; müşteri cayma yolunu kullanır — cayma düğmesi sipariş kargoya verildiği anda açılır (§7.2.1, §7.3.7) | K-196, K-340, K-452 |
| 6.4.2 | Firma kargoya verme süresini ya da yasal 30 günlük üst sınırı aşar | Kargoya verme süresi aşıldığında sipariş henüz kargoya verilmemiştir; müşterinin iptal düğmesi açıktır ve fesih hakkı o düğmeyle kullanılır. Otuz günlük sınır aşıldığında sipariş kargoya verilmiş olsa bile müşteri "gecikme nedeniyle fesih" yapar; kargo ücreti dahil geri ödenir, kargodaki mal firmaya döner ve panel firmaya kanuni faiz uyarısını gösterir (§7.2.3) | K-198, K-499 |
| 6.4.3 | Müşteri ödemesi onaylanmış dijital kalemi iptal etmek ya da ondan caymak ister | İptal edilemez — kalem teslim edilmiştir; ön onayla cayma hakkı da düşmüştür (§7.2.5, §7.3.2) | K-200, K-204 |
| 6.4.4 | Müşteri cayma istisnası işaretli bir üründen cayar | Mutlak sebeplerde — kişiye özel üretim, çabuk bozulan ya da son kullanma tarihi geçebilecek mal — cayma düğmesi yoktur. Koşullu sebeplerde — koruyucu ambalajı açılınca iadesi sağlık ve hijyen açısından uygun olmayan mal; maddi ortamdaki kitap, dijital içerik ve bilgisayar sarf malzemesi — düğme açıktır ve beyan kayda geçer; koruyucu ambalajı açılmış dönen malda ret firmanın kararıdır. İstisna, sebebi ve koşulu ön bilgilendirmede gösterilmiştir (§7.3.4) | K-206, K-509 |
| 6.4.5 | Müşteri tamamlanmış bir hizmetten cayar | Hak, müşterinin onay adımında onayladığı ifa tamamlanıp firma hizmeti "tamamlandı" işaretlediğinde düşmüştür (§3.24.2, §7.3.3) | K-205, K-503 |
| 6.4.6 | On dört günlük cayma penceresi geçmiştir | Cayma hakkı yoktur; sebepli bir sorun ayıp talebi yolundan yürür (§7.3.1, §7.5.1) | K-203, K-226 |
| 6.4.7 | Havale ile ödenmiş siparişin parası geri ödenecektir | Müşteri IBAN'ını iptal ya da cayma beyanı sırasında girer; kart hattında IBAN istenmez (§7.4.5) | K-214 |
| 6.4.8 | Firma, teslimden sonra yapılan caymada geri ödemeyi iade malı ulaşana kadar yapmaz | Süre henüz başlamamıştır: teslimden sonraki caymada geri ödemenin on dört günü malın firmaya ulaştığı tarihten işler. Teslimden önceki caymada ve hizmette süre beyandan işler ve mal beklenmez; panel kalan süreyi gösterir (§7.4.1) | K-491 |
| 6.4.9 | Gönderi, müşterinin adresi yanlış vermesi ya da teslimatta bulunmaması yüzünden geri döner ve sipariş iptal edilir | Firma "gönderi teslim edilemedi — müşteri kaynaklı" sebebini seçer: ürün bedeli geri ödenir, gidiş kargo bedeli ödenmez. Diğer sebeplerde gidiş kargosu da geri ödenir (§7.2.4, §7.2.9) | K-176, K-212, K-451 |
| 6.4.10 | Kısmi iptal ya da iadeden sonra kalan tutar kuponun asgari tutarının ya da ücretsiz kargo eşiğinin altına düşer | Kupon ve kargo yeniden hesaplanmaz; müşteriden bir şey geri istenmez. Kargo ücreti yalnız fiziksel kalemlerin tamamı kargodan önce iptal edildiğinde ya da gönderilenlerin tamamından cayıldığında geri ödenir (§7.2.7, §7.2.9, §7.4.6) | K-202, K-213, K-501 |
| 6.4.11 | Müşteri, sipariş kargoya verildikten sonra ve mal eline ulaşmadan vazgeçer | İptal yolu kapanmıştır ama cayma beyanı açıktır; malı teslim almadan cayabilir (§7.3.1) | K-340, K-196 |
| 6.4.12 | Firma teslimi geç işaretler ve girdiği teslim tarihine göre pencere işaret anında dolmuştur | Cayma düğmesi kapanır; daha önce yapılmış beyan geçerlidir ve gecikmenin riski firmadadır (§7.3.1) | K-340, K-288 |
| 6.4.13 | Müşteri siparişin bir kaleminden cayar | Gönderilen fiziksel kalemlerden biri müşteride kalıyorsa gidiş kargo ücreti geri ödeme tutarına girmez; gönderilen fiziksel kalemlerin tamamından cayıldığında — ardışık beyanlarla da — son caymanın geri ödemesine girer (§7.4.6) | K-292, K-501 |
| 6.4.14 | Müşteri malı geri göndermek için kargo ücretini önden ödemek istemez | Malı firmanın iade adresine karşı ödemeli gönderir; bedeli teslim alırken firma öder (§7.4.4) | K-293 |
| 6.4.15 | Çözülmüş ayıp tekrarlar — onarılan mal yine bozulur | Müşteri ya da firma talebi yeniden açar; yeni talep açılmaz (§5.10) | K-297 |
| 6.4.16 | Müşteri teslimden sonra cayar ama malı hiç göndermez | Geri ödeme süresi başlamaz; panel kalemi "iade malı bekleniyor" olarak, beyandan geçen gün ve müşterinin on dört günlük gönderme süresiyle gösterir. Sistem caymaya ve kaleme dokunan kendiliğinden bir işlem başlatmaz. Gönderme süresi geçtikten sonra firma kalemi "mal dönmedi" gerekçesiyle kapatabilir: kapatma işlem izine yazılır, müşteriye bildirim gitmez ve geri ödeme yapılmaz. Havale hattında müşterinin IBAN'ı kapatmayla, firma kapatmazsa gönderme süresinin bitiminden otuz gün sonra kendiliğinden silinir (§7.4.1, §7.4.4, §7.4.5, §10.4.9) | K-491, K-494, K-496, K-497 |
| 6.4.17 | Firma iade malının teslim alma işaretini geciktirir | Süre işaretin konduğu andan değil, girilen ulaşma tarihinden işler; tarih geçmişe dönük girilebilir. Geç ya da uydurulmuş tarihin riski firmadadır: karşı ödemeli teslimin kargo kaydı malın ulaştığı günü gösterir ve ispat yükü firmadadır (§7.4.1) | K-491, K-288, K-07 |
| 6.4.18 | Firma kalemi "mal dönmedi" gerekçesiyle kapattıktan sonra mal firmaya ulaşır | Kalem yeniden açılır: firma teslim alma adımında ulaşma tarihini girer ve geri ödemenin on dört günü o tarihten işler. Kapatma cayma beyanını geçersiz kılmamıştır (§7.4.1, §10.4.9) | K-496, K-491 |
| 6.4.19 | Havale hattında IBAN silindikten sonra mal firmaya ulaşır — kalem kapatılmış ya da gönderme süresinin bitiminden otuz gün geçmiştir | Firma teslim alma adımında ulaşma tarihini girer; kalem kapatılmışsa yeniden açılır (§10.4.9). Adımla birlikte müşterinin sipariş sayfasında o kalem için IBAN alanı yeniden açılır ve müşteriye IBAN'ını oradan girmesini isteyen bir e-posta gider (§9.2 B-14); firma IBAN'ı panelden girmez. Geri ödemenin on dört günü ulaşma tarihinden işler ve IBAN beklenirken durmaz; panel kalemi "IBAN bekleniyor" olarak ve kalan süreyle gösterir (§7.4.5) | K-498, K-497, K-496, K-491 |
| 6.4.20 | Müşteri yeniden istenen IBAN'ı geç girer ya da e-posta ona ulaşmaz | Süre durmaz; sipariş sayfası IBAN alanını e-postadan bağımsız gösterir (§6.1.1). IBAN isteğinin tarihi işlem izinde durur — gecikmenin müşteriden kaynaklandığını gösteren kanıt firmanın elinde kalır. Müşteri IBAN'ı girdiğinde firma geri ödemeyi mevcut elle adımla işler (§7.4.5, §10.3.1) | K-498, K-491, K-234 |
| 6.4.21 | Müşteri tamamlanmamış bir hizmet kaleminden cayar ve hizmet hiç ifa edilmez | Beyan kalemi kapatır — ayrı bir kalem durumu doğmaz; kalem tamamlanmayı beklemez ve geri ödemenin on dört günü beyandan işler. Siparişin kalan kalemleri teslim işaretini almışsa sipariş Teslim edildi'ye geçer (§5.6.6, §5.8, §7.3.3) | K-503, K-491 |
| 6.4.22 | Kargodayken cayılan gönderi alıcıya teslim edilemeyip firmaya döner | Firma cayma beyanlı kalemi iptal etmez; geri dönen malı teslim alma adımıyla işler. Geri ödeme tek hattan — caymanın hattı ve süresiyle — yürür; gönderilen fiziksel kalemlerin tamamından cayıldıysa kargo ücreti eklenir. Firma iptalinin ikinci bir geri ödemesi açılmaz (§5.4 S9, S11; §7.4.6) | K-503, K-501, K-215 |
| 6.4.23 | Havale bekleyen müşteri siparişin yalnız bir kalemini iptal etmek ister | Ödeme onayından önce kalem düzeyi iptal yoktur; müşteri siparişi bütünüyle iptal eder ve yeni sipariş verir (§7.2.2) | K-505 |
| 6.4.24 | Kart hattındaki geri ödeme sağlayıcıda başarısız olur — sağlayıcı iadeyi reddeder ya da sağlayıcıya ulaşılamaz | Ödeme ekseni değişmez. Siparişin panel satırına "geri ödeme gerçekleşmedi" işareti düşer, kalem bekleyen işler sayacının "iade ve geri ödeme bekleyen" satırına girer ve panelde firmaya uyarı çıkar. Firma panelden yeniden dener ya da müşteriye havale yolunu açar: havale müşterinin seçimidir — müşteri IBAN'ı sipariş sayfasından girerse geri ödeme havaleyle yapılır (§9.2 B-14) ve firma ödemeyi işaretler; IBAN girilene kadar kart iadesi yeniden denenebilir, girildikten sonra denenmez. Sistem kendiliğinden yeniden denemeyi sürdürmez; yasal süre (Z-16, Z-17) işlemeye devam eder (§7.2.8) | K-521, K-498, K-569 |
| 6.4.25 | Müşteri caymayı sipariş sayfası yerine e-postayla, mektupla ya da örnek cayma formuyla bildirir — düğmesi kapanmış olsa da süresi içinde | Firma bildirimi panelden kaydeder: kalemi seçer, bildirimin firmaya ulaştığı tarihi girer — geçmişe dönük olabilir, ileri tarihli olamaz — ve havale hattında IBAN'ı müşterinin bildiriminden girer; bildirim IBAN taşımıyorsa kayıt IBAN'sız yapılır, sipariş sayfasında o kalem için IBAN alanı açılır ve müşteriye B-14 gider. Kayıt bir cayma beyanıdır; geri ödeme süresi bu tarihle işler, müşteriye cayma bildirimi (B-9) gider ve kayıt işlem izine yazılır (§10.4.10) | K-510, K-207, K-491, K-581 |
| 6.4.26 | Havale hattında IBAN yeniden istenmiş ama müşteri IBAN'ı hiç girmez | Kalem kapanmaz ve borç sürer. Kalem panelde "IBAN bekleniyor" listesinde görünür ama firmanın yapabileceği bir iş olmadığı için bekleyen işler sayacında sayılmaz; firma müşteriye ulaşmak için siparişin iletişim bilgisini görür. Geri ödeme süresi işlemeye devam eder (§7.4.5, §10.6.1) | K-525, K-498 |
| 6.4.27 | Müşteri geri ödeme için IBAN'ı yanlış girer ya da banka geri ödeme havalesini reddeder | Geri ödeme işlenmeden önce müşteri IBAN'ı sipariş sayfasından düzeltir. Havale bankada gerçekleşmezse firma geri ödemeyi işlemez, panelden "havale gerçekleşmedi" der: IBAN silinir, sipariş sayfasında IBAN alanı yeniden açılır ve müşteriye B-14 gider; kalem "IBAN bekleniyor" listesine döner ve süre durmaz. İşlenmiş geri ödemeden sonra dönen havalede ödeme ekseni düzeltilmez; firma müşteriye ulaşır ve parayı yeniden gönderir (§7.4.5, §9.2) | K-575, K-498, K-525 |
| 6.4.28 | Müşteri kısmen ifa edilmiş bir hizmet kaleminden cayar | Kalem "tamamlandı" işaretini almadığı için hak sürer; beyan kalemi kapatır ve kalemin siparişte ödenmiş bedelinin tamamı geri ödenir — yapılmış kısım için bedel kesilmez; geri ödemenin on dört günü beyandan işler (§7.3.3, §7.4.1) | K-578, K-205, K-503 |

### 6.5 Üyelik (akış 4)

| # | Ne ters gider | Sistemin cevabı | Kaynak |
|---|---|---|---|
| 6.5.1 | Doğrulama bağlantısının süresi dolmuştur | Kayıt silinir; bağlantıya tıklayan "bağlantı geçersiz, yeniden kayıt olun" mesajını görür; e-posta yeniden kayda açıktır (§3.13.5) | K-102 |
| 6.5.2 | Kullanıcı e-postasını doğrulamadan giriş yapmaya çalışır | Giriş yapılamaz (§3.13.4) | K-101 |
| 6.5.3 | Kayıtta girilen e-posta zaten kayıtlıdır | "Bu e-posta zaten kayıtlı" (§3.13.11) | K-107 |
| 6.5.4 | Şifre sıfırlamada girilen e-posta kayıtlı değildir | Nötr mesaj: "bu adres kayıtlıysa sıfırlama bağlantısını gönderdik" (§3.13.11) | K-107 |
| 6.5.5 | Şifre sıfırlama bağlantısı kullanılmış ya da süresi dolmuştur | Bağlantı geçersizdir; kullanıcı yeni talep açar (§3.13.9) | K-105 |
| 6.5.6 | Kullanıcı asgari uzunluğun altında ya da çok yaygın bir şifre seçer | Şifre reddedilir (§3.13.15) | K-120 |
| 6.5.7 | Kimlik sağlayıcısı e-postayı doğrulanmamış verir | Mevcut bir hesapla bağlama yapılmaz (§3.13.7) | K-104 |
| 6.5.8 | Google ile açılmış, şifresi olmayan hesap "şifremi unuttum" der | Sıfırlama akışı "şifre belirle" işlevi görür (§3.13.8) | K-121 |
| 6.5.9 | Kullanıcı yeni e-posta adresini doğrulamaz | Değişiklik geçerli olmaz; hesap eski adresle çalışmaya devam eder (§3.13.14) | K-110 |
| 6.5.10 | Yürüyen siparişi olan kullanıcı hesabını siler | Engellenmez; sipariş yoluna devam eder (§3.15.2) | K-116 |
| 6.5.11 | Giriş, şifre sıfırlama ya da hesap kaydı denemeleri eşiği aşar — aynı IP'den ya da aynı e-posta adresine | O işlem geçici olarak engellenir ve pencere içindeki deneme sayısı eşiğin altına inince kendiliğinden açılır; hesap kilitlenmez; mesaj nötrdür (§3.13.17, §6.1.5) | K-328, K-329, K-330, K-460 |
| 6.5.12 | Aydınlatma metni tamamlanmamış ya da yayına alınmamışken ziyaretçi hesap açmak ya da ilk kez Google ile girmek ister | Kayıt ekranı ve yeni hesap açacak Google ile giriş kapalıdır; mevcut hesapların girişi etkilenmez (§3.13.19) | K-517 |

### 6.6 Katalog yönetimi (akış 5)

| # | Ne ters gider | Sistemin cevabı | Kaynak |
|---|---|---|---|
| 6.6.1 | İndirimli fiyat referans fiyatın altına inmez | Sistem indirimi kaydetmez ve yöneticiyi sebebiyle uyarır (§3.9.5) | K-68 |
| 6.6.2 | Firma içinde ürün ya da alt kategori bulunan bir kategoriyi siler | Silme engellenir; engelleyenler sayısıyla gösterilir (§3.4.5) | K-46 |
| 6.6.3 | Taşıma, üç seviyeyi aşan bir dal üretir ya da kategori kendi alt ağacına taşınır | Taşıma yapılamaz (§3.4.6) | K-46 |
| 6.6.4 | Yayın kapısının koşullarından biri eksiktir — ad, ana kategori, tip, Yayında varyant ya da hizmet ürününde ifa süresi | Ürün yayına alınamaz (§3.7.3, §3.7.4) | K-86, K-55, K-538, K-503 |
| 6.6.5 | Dijital ürünün yayındaki bir varyantı dosyasızdır | Ürün yayına alınamaz; panel dosyası eksik varyantı gösterir (§3.7.4) | K-145 |
| 6.6.6 | Yayındaki dijital ürünün dosyasını silmek bir varyantı dosyasız bırakır | Silme engellenir; firma önce yeni dosyayı yükler ya da varyantı yayından çeker (§3.7.4) | K-145 |
| 6.6.7 | Fiyata ikiden fazla ondalık girilir | Panel kabul etmez (§3.8.4) | K-62 |
| 6.6.8 | Sabit tutarlı kuponun asgari sepet tutarı kupon tutarına eşit ya da altında girilir; yüzdesel kupon %100 girilir | Girilemez (§3.10.4) | K-73, K-565 |
| 6.6.9 | Ürünün kargoya verme süresi üst çiti aşar | Panel çitin üstünde değer kabul etmez (§3.20.5) | K-139, K-140 |
| 6.6.10 | İki yönetici aynı kaydı aynı anda düzenler | İkinci kaydeden kaydın değiştiğini görür ve üzerine yazmadan önce uyarılır (§3.31.1) | K-270 |
| 6.6.11 | Yayındaki bir ürünün son Yayında varyantı arşivlenmek ya da taslağa çekilmek istenir | İşlem engellenir; panel firmayı önce ürünü taslağa ya da arşive almaya yönlendirir (§3.7.8) | K-299, K-538 |

### 6.7 Sipariş yürütümü (akış 6)

| # | Ne ters gider | Sistemin cevabı | Kaynak |
|---|---|---|---|
| 6.7.1 | Havalede gelen tutar sipariş tutarından farklıdır | Sistem tutar sormaz; firma "ödendi" işaretler ya da farkı müşteriyle sistem dışında çözer (§3.21.7) | K-169 |
| 6.7.2 | Havale, ödeme süresi dolup sipariş iptal edildikten sonra gelir | İptal edilmiş sipariş "ödendi" işaretlenemez; firma parayı geri öder ya da müşteriden yeni sipariş ister (§3.21.8) | K-183 |
| 6.7.3 | Kargo gönderiyi ulaştıramayıp firmaya geri döndürür | Firma siparişi Teslim edilemedi olarak işaretler; buradan yeniden gönderir ya da iptal eder (§5.4 — S7, S8, S9). Cayma beyanlı kalem iptal edilmez, teslim alma adımıyla işlenir (§6.4.22). Sipariş panelde "kargoya verilecek" sayacında görünür (§10.6.1) | K-176, K-503, K-535 |
| 6.7.4 | Firma kargoya verirken takip numarası girmez | Kargo şirketi + takip numarası ya da "kendi aracımızla teslim" beyanı olmadan işaret konamaz (§3.20.7) | K-141 |
| 6.7.5 | Firma bir geçişi yanlışlıkla yapar | Panel "geri al" sunmaz; dört geri alınamaz geçiş önceden onay ister. Firma geçişi sebep seçerek yeni bir geçişle düzeltir; iz temizlenmez ve müşteriye bildirim gider. Dış dünyaya çıkmış sonuç geri alınmaz. Yanlış iptal yalnız ödeme Ödendi'de kalmışsa ve iptalin serbest bıraktığı stok ve kontenjan hâlâ yetiyorsa düzeltilir; ödemesi Başarısız olmuş ya da parası geri gönderilmiş siparişte müşteri yeni sipariş verir. Para gelmeden konmuş havale "ödendi" işareti, sipariş kargoya verilmemiş ve hiçbir kalem teslim işareti almamışsa ödeme ve sevkiyat birlikte geri alınarak düzeltilir; ödeme süresi düzeltmeden yeniden işler (§5.4, §5.9, §10.4.6) | K-225, K-372, K-375, K-504, K-566, K-570 |
| 6.7.6 | Firma siparişi karşılayamaz — stok hatası, ürünün bulunamaması, teslimatın mümkün olmaması | Firma sebep seçerek iptal eder; sebep müşteriye bildirilir ve işlem izine yazılır (§7.2.4) | K-199 |
| 6.7.7 | Dijital üründe müşterinin indirme hakkı dolar | Müşteri firmaya başvurur; firma o kalemin hakkını panelden yeniler (§3.12.6) | K-147 |
| 6.7.8 | Satılan dijital dosya ayıplıdır | Firma dosyayı düzeltip yeniden yükler; düzeltme eski alıcılara da ulaşır (§7.5.4) | K-229, K-148 |
| 6.7.9 | İptal edilmiş siparişe kart ödemesi gelir | Sistem kendiliğinden geri öder ve firmaya panelde bildirir (§6.2.16) | K-233 |
| 6.7.10 | Firma kargoya verilmiş siparişin teslimini hiç işaretlemez | Otomatik geçiş yoktur; sipariş "Kargoya verildi"de kalır ve cayma penceresi başlamaz; "sorun bildir" kanalı açıktır ama iki yıllık süre işlemez — yükü firma taşır (§3.20.11, §7.5.2) | K-288, K-506 |
| 6.7.11 | Firma teslim tarihini ya da kargo şirketi ve takip numarasını yanlış girer | Firma panelden düzeltir; düzeltme işlem izine yazılır. Takip bilgisinin düzeltilmesi müşteriye bildirilir, teslim tarihinin düzeltilmesi bildirilmez (§10.4) | K-369, K-375 |
| 6.7.12 | Siparişin teslimat ya da fatura adresi yanlıştır | Firma "Kargoya verildi"ye kadar düzeltir ve müşteriye bildirilir; sonrasında düzeltme yapılmaz (§10.4) | K-374, K-375, K-450 |
| 6.7.13 | Bir e-posta üç yeniden denemeden sonra da müşteriye ulaşmaz | Siparişin panel satırına "e-posta ulaşmadı" işareti düşer; akış etkilenmez (§6.1.1) | K-321 |
| 6.7.14 | Firma ödenmemiş siparişte hizmet kalemini "tamamlandı" işaretlemek ister | Panel işareti sunmaz; işaret ödeme Ödendi olduktan sonra konur (§3.20.9, §5.6.1) | K-536 |

### 6.8 Kurumsal içerik (akış 7)

| # | Ne ters gider | Sistemin cevabı | Kaynak |
|---|---|---|---|
| 6.8.1 | Zorunlu alanı eksik kayıt yayına alınmak istenir | Yayına alınamaz; panel eksik alanı gösterir (§3.27.24) | K-275 |
| 6.8.11 | Yayındaki bir kaydın zorunlu alanı düzenlemede boşaltılır — SSS cevabı, şubenin adresi | Düzenleme kaydedilmez; panel eksik alanı gösterir ve firmayı önce kaydı taslağa almaya yönlendirir (§3.27.24, §5.2) | K-538, K-299 |
| 6.8.2 | Firma Hakkımızda'yı silmek ister | Hakkımızda silinmez, yalnız taslağa alınır (§3.27.3) | K-277 |
| 6.8.3 | Hakkımızda boştur ya da yayında değildir | Ana sayfanın kurumsal bloğu marka adını ve logoyu gösterir; boş kutu görünmez (§3.27.14) | K-250 |
| 6.8.4 | İçeriğe bağlı ürün taslağa ya da arşive alınır veya silinir | Taslakta ve arşivde ürün bağda kalır ama görünmez; silinen ürün bağdan kalkar (§3.27.10) | K-245 |
| 6.8.5 | Ziyaretçi silinmiş ya da taslaktaki bir içeriğin adresine gelir | "Sayfa bulunamadı" (§3.30.4) | K-274, K-277, K-281 |
| 6.8.6 | İki yönetici aynı kaydı aynı anda düzenler | İkinci kaydeden uyarılır (§3.31.1) | K-270 |
| 6.8.7 | Bir bot iletişim formunu doldurur ve görünmez tuzak alanı da doldurur | Gönderim hata mesajı vermeden sessizce düşer; talep kaydı oluşmaz (§3.32.7) | K-304 |
| 6.8.8 | Aynı IP'den çok sayıda form gönderilir | Eşik aşıldığında gönderim geçici olarak engellenir; mesaj nötrdür (§8) | K-304, K-328 |
| 6.8.9 | Aydınlatma metni tamamlanmamış ya da yayına alınmamışken ziyaretçi formu açar | Form çalışmaz; kurumsal sayfalar yayında kalır (§3.32.8) | K-365, K-342, K-517 |
| 6.8.10 | İletişim talebine e-postayla verilen cevabı müşteri yanıtlar | Yanıt firma kimliğindeki iletişim e-postasına düşer; sisteme girmez ve yeni talep açmaz (§9) | K-379, K-305 |

### 6.9 Mağaza ayarları (akış 8)

| # | Ne ters gider | Sistemin cevabı | Kaynak |
|---|---|---|---|
| 6.9.1 | Firma tipinin zorunlu kimlik alanları eksiktir ya da firma tipi değişmiş ve yeni setin alanları boştur | Satış açılmaz; kurumsal taraf yayında kalır (§3.1.3, §3.1.5) | K-09, K-14 |
| 6.9.2 | Havale açılmak istenir ama IBAN boştur | Havale açılamaz (§3.21.5) | K-161 |
| 6.9.3 | Ödeme sağlayıcı anahtarları kurulumda tanımlı değildir | Kart yöntemi görünmez; havale açıksa satış yalnız havaleyle açıktır (§3.21.4) | K-162, K-163 |
| 6.9.4 | Hiçbir ödeme yöntemi açık değildir | Satış açılmaz (§3.1.5) | K-163 |
| 6.9.5 | Havale ödeme süresi alt ya da üst çitin dışındadır ya da varsayılan kargoya verme süresi üst çiti aşar | Panel çit dışındaki değeri kabul etmez ve sebebini söyler (§3.17.4, §3.20.4, §4.1.5) | K-139, K-166, K-339, K-448 |
| 6.9.6 | Marka adı girilmemiştir | Site alan adını gösterir (§3.29.3) | K-260 |
| 6.9.7 | Logo yüklenmemiştir | Marka adı yazıyla gösterilir (§3.29.2) | K-259 |
| 6.9.8 | Site simgesi yüklenmemiştir | Marka adının baş harfi marka rengi zemin üzerinde gösterilir (§3.29.4) | K-261 |
| 6.9.9 | Marka rengi seçilmemiştir ya da üstündeki yazıyı okunmaz kılacak bir renk seçilmiştir | Renk seçilmemişse ürünün varsayılan rengi kullanılır; üstündeki yazının rengini sistem kontrasta göre seçer (§3.29.5) | K-262 |
| 6.9.10 | Aydınlatma metni tamamlanmamış ya da yayına alınmamıştır — barındırma konumu bölümü ya da firma kimliği eksiktir —, çerez politikası yayına alınmamıştır ya da iade adresi girilmemiştir | Satış açılmaz ve panel eksik olanı adıyla gösterir; aydınlatma metni tamamlanmamışken — barındırma bölümü ya da firma kimliği eksikken — iletişim formu, hesap kaydı ve Google ile ilk giriş de kapalıdır (§3.1.5, §3.13.19, §3.32.8) | K-365, K-360, K-293, K-465, K-517, K-576 |
| 6.9.11 | Son yönetici kaldırılmak istenir ya da bir yönetici kendi hesabını kaldırmak ister | İşlem engellenir ve panel sebebini söyler (§10.2) | K-309 |
| 6.9.12 | Yönetici davetinin bağlantısı kullanılmış ya da süresi dolmuştur | Bağlantı geçersizdir ve hesap açılmaz; mevcut bir yönetici yeni davet gönderir (§10.2) | K-308 |
| 6.9.13 | Firmanın SSS ya da sayfa metni, sonradan değişen bir ayarla çelişir | Ürün çelişkiyi tespit etmez; bağlayıcı olan ayarlardan üretilen form ve sözleşmedir. Panelde ilgili ayarın yanında bir hatırlatma satırı durur (§3.33.8) | K-364 |

*Kaynak: K-231 · K-232 · K-233 · K-234 (`B6-15` — hata ve istisna envanteri) · K-13 · K-68 · K-129 · K-130 · K-133 · K-150 · K-151 · K-169 · K-193 · K-127 · K-187 · K-235 · K-321 · K-329 · K-395 · K-497 · K-498 — ve tabloların kaynak sütunundaki satırlar · K-339 · K-448 · K-499 · K-528 · K-460 · K-501 · K-503 · K-505 · K-506 · K-535 · K-536 · K-538 · K-509 · K-510 · K-517 · K-521 · K-522 · K-524 · K-525 · K-556 · K-565 · K-566 · K-569 · K-570 · K-571 · K-585.*

## 7. İptal, itiraz ve geri alma

> **Ne yazılır:** Kullanıcının bir işlemi geri alma yolları, koşulları, sınırları.

### 7.1 Üç yol ve ortak ilkeler

**7.1.1 Müşterinin satın almadan sonra başvurabileceği üç ayrı yol vardır.** **İptal** kalem yola çıkmadan ya da teslim edilmeden önce işler ve sınırı kalem tipine göre kapanır — fiziksel kalemde sipariş kargoya verilene kadar (§7.2.1, §7.2.5). **Cayma** sebepsizdir; yasal penceresi on dört gündür — fiziksel kalemde teslimden, hizmette sipariş tarihinden ya da fiziksel kalem içeren siparişte teslimden işler (§7.3.1, §7.3.3) —, tüketici malı teslim almadan da cayabilir ve tipe özel istisnaları vardır. **Ayıp talebi** sebeplidir ve teslimden itibaren yasal süre boyunca açıktır. Üçü arayüzde aynı düğme değildir ve kayıtta aynı sebep kodunu paylaşmaz; iptal ile caymanın sınırı §7.3.7'dedir (K-195, K-226, K-289, K-340, K-452, K-503).

**7.1.2 Tek akış tüketici akışıdır.** Ürün 6502 sayılı Kanun'un tüketiciye tanıdığı korumaları her siparişte dallanmadan işletir (K-07). Alıcının hukuken tüketici sayılıp sayılmadığını mevzuat belirler; ürün alıcıdan ticari bilgi istemez ve ticari amaçla alan biri de aynı akıştan geçer (`01 §3.2` — K-07, K-112).

**7.1.3 İptal ve iade kalem düzeyinde işler.** Sipariş kalemi tek tek iptal ve iade edilebilir; siparişin kalan kalemleri yoluna devam eder ve para kısmen döner. Gerekçe tercih değil **hak**tır: üç üründen birini beğenmeyen tüketici üçünü birden iade etmeye zorlanamaz (K-178).

**7.1.4 Üç yol da sipariş sayfasından kullanılır;** misafir alıcı sayfaya §3.22.3'ün yollarından girer (K-207, K-227, K-187). Müşteri kendi yaptığı işlemin de e-postasını alır: iptal, cayma beyanı ve ayıp talebi, işlemin tarihini taşıyan bir kayıt kopyası olarak e-postayla gider (§9; K-317, K-319).

**7.1.5 İtirazın sistemdeki yeri.** Ayrı bir şikâyet kanalı yoktur: müşterinin sipariş sayfasındaki üç yolu ve bunların dışındaki her konu için iletişim formu vardır (§3.32.9; K-367). Uyuşmazlıkta tüketicinin tüketici hakem heyetine ya da — dava açılmadan önce arabulucuya başvurma şartıyla — tüketici mahkemesine başvurabileceği Ön Bilgilendirme Formu'nda ve Mesafeli Satış Sözleşmesi'nde yazar; merciler arasındaki parasal görev sınırı rakamla yazılmaz (§3.33.7; K-366, K-368, K-543).

### 7.2 İptal

Ödenmemiş siparişin kendiliğinden iptali §3.17.5 ve §3.17.7'dedir.

**7.2.1 Müşteri fiziksel kalemi, sipariş "Kargoya verildi"ye geçene kadar kendi iptal eder** — sipariş sayfasından, firma onayı beklemeden; iptal kendiliğinden işler. Kargoya verildikten sonra fiziksel kalemin iptal yolu kapanır ve müşterinin yolu caymadır (§7.3). Bedeli bilinçlidir: firma, hazırlamaya başladığı bir siparişin iptalini engelleyemez (K-196, K-340). İptal yolu kalem tipine göre kapanır: dijital ve hizmet kaleminin sınırı §7.2.5'tedir (K-197, K-200, K-452).

**7.2.2 İptal kalem düzeyindedir.** Müşteri siparişin tek bir kalemini iptal eder, kalan kalemler yoluna devam eder; siparişin tamamı ancak bütün kalemleri iptal edildiğinde İptal edildi'ye geçer (K-197). Fiziksel kalemlerinin tamamı iptal edilen karışık sipariş İptal edildi'ye geçmez; kalan dijital ve hizmet kalemlerinin hattına düşer (§5.7; K-295). **Kalem düzeyi iptal ödeme onayından sonra açılır;** ödeme onayından önce iptal yalnız sipariş bütünüyle yapılır — müşterinin ve firmanın iptalinde ve yöneticinin kalem çıkarmasında (§10.4.3). Havale bekleyen müşteri bir kalemden vazgeçerse siparişi iptal edip yeni sipariş verir: ödenmemiş siparişte kalem iptali ödenecek tutarı ve havale bilgisini değiştirir ve eski tutarın gelmesi bir fark iadesi doğururdu (K-505).

**7.2.3 Teslimat gecikmesinde fesih iki yoldan işler.** **Kargoya verme süresi** aşıldığında sipariş tanımı gereği hâlâ kargoya verilmemiştir; müşterinin iptal düğmesi açıktır ve fesih hakkı o düğmeyle kullanılır (K-198). **Otuz günlük yasal teslim sınırı** (§4.2 Z-11) aşıldığında ise sipariş kargoya verilmiş olsa bile müşteri sipariş sayfasından **"gecikme nedeniyle fesih"** yapar; bu iptalden ayrı bir kayıt tipidir (§5.8). Düğme, siparişin firmaya ulaştığı andan otuz gün geçmiş ve teslim tarihi girilmemiş fiziksel kalemi bulunan siparişte görünür ve cayma istisnası işaretli ürünü de kapsar — fesih cayma değildir. Fesih teslim edilmemiş fiziksel kalemlerin tamamına uygulanır; teslim işaretini almış dijital ve hizmet kalemleri ile henüz tamamlanmamış hizmet kalemi feshin dışındadır ve yollarına devam eder (K-571). Feshedilen kalemlerin siparişte ödenmiş bedelleri — kupon payı §7.2.7'nin kalıbıyla — **kargo ücreti dahil**, fesih bildiriminin tarih damgasından itibaren on dört gün içinde ödemenin geldiği yoldan geri ödenir: kartta sistem kendiliğinden başlatır, havalede firma panelden işler ve IBAN fesih anında müşteriden alınır — iptalin kalıbı (§7.2.8, §7.4.5; K-214). Kargodaki mal firmanın iade adresine döner, masrafı firmadadır. Teslim tarihinin girilmemiş olmasının riski firmadadır (K-340'ın kalıbı). **Kanuni faiz:** sistem faizi hesaplamaz; panel fesih satırında firmaya *"gecikme feshi — kanuni faiz yükümlülüğü"* uyarısını gösterir ve faiz firmanın yükümlülüğüdür (§12.5). Aynı uyarı, taahhüt edilen süre — kargoya verme süresi ya da hizmetin ifa süresi (K-503) — aşıldıktan sonra yapılan iptalde de gösterilir (K-499, K-198, K-448). **Kalan risk:** Yönetmelik m.16/3 *"tahsil edilen tüm ödemeleri"* der; karışık siparişte feshin sözleşmenin bütününe yayıldığı okuması tüketici lehine olabilir — ürün feshi, kalem düzeyindeki iptalin kalıbıyla (K-197), gecikmenin dokunduğu kalemlerle sınırlar (K-571).

**7.2.4 Firma da iptal edebilir** — stok hatası, ürünün bulunamaması, teslimatın mümkün olmaması gibi durumlarda. **Sebep seçmek zorunludur:** firma kapalı bir listeden seçer; sebep müşteriye bildirilir ve kim/ne zaman bilgisiyle işlem izine yazılır (K-199). Liste ürünle gelir, firma düzenlemez: **stokta bulunamadı · ürün hasarlı ya da satılamaz durumda · teslimat adresine gönderim yapılamıyor · gönderi teslim edilemedi — müşteri kaynaklı (adres hatalı ya da alıcı bulunamadı) · müşteriyle anlaşıldı (müşteri talebi) · diğer** — "diğer"de açıklama zorunludur (K-373, K-451). Firma iptali geri alınamaz onayı ister (§5.9). Durum düzeltmesinin sebep listesi bundan ayrıdır (§10.4; K-373).

**7.2.5 Dijital kalem ödeme onayından sonra iptal edilemez:** ödeme onaylandığında kalem teslim edilmiş sayılır ve bundan sonrası cayma konusudur (§7.3.2). Ödeme onayından **önce** — havale beklerken — sipariş yalnız bütünüyle iptal edilebilir (§7.2.2; K-505) ve indirme hiç açılmamış olur. **Hizmet kalemi**, firma "tamamlandı" işaretleyene kadar iptal edilebilir (K-200); ifa süresi (§3.2.2) aşıldıktan sonraki iptalde panel firmaya kanuni faiz uyarısını gösterir (§7.2.3; K-503).

**7.2.6 İptalde sayaçlar kendiliğinden döner.** İptal edilen kalemin adedi stoğa, hizmet kontenjanı havuza döner; kuponun kullanım hakkı yalnız siparişin **tamamı** iptal edildiğinde sayaca döner (K-201, K-202). Dijital ürünün dönecek sayacı yoktur (K-83).

**7.2.7 Kısmi iptalde kupon yeniden hesaplanmaz.** İptal edilen kalemin kupon payı geri ödeme tutarına dahil edilir; kalan kalemler kendi indirimlerini korur ve kuponun kullanım hakkı geri dönmez — sipariş hâlâ yaşıyordur. Kalan tutarın kuponun asgari sepet tutarını karşılayıp karşılamadığına bakılmaz (K-202).

**7.2.8 İptal edilen ödenmiş kalemin parası, ödemenin geldiği yoldan geri ödenir** (§7.4.5; K-214). **Kart hattında** geri ödemeyi sistem kendiliğinden başlatır ve firmaya panelde bildirir — onay penceresi yoktur. **Havale hattında** geri ödeme firmanın panelden işlediği elle bir adımdır ve müşterinin iptal sırasında girdiği IBAN'ı kullanır. Süre her iki hatta da iptalden itibaren **on dört gündür**; havale hattında panel kalan süreyi gösterir (K-291). Kart hattında müşteri iptalinin parası firmanın dikkatine bağlanmaz — firma onayı beklemeyen iptal parayı da beklemez; havale hattında geri ödeme firmanın elle adımıdır ve panel kalan süreyi gösterir (K-196, K-233, K-291). **Kart iadesi sağlayıcıda başarısız olursa** — sağlayıcı reddeder ya da ulaşılamaz — ödeme ekseni değişmez; siparişin panel satırına "geri ödeme gerçekleşmedi" işareti düşer, kalem bekleyen işler sayacına girer ve panelde firmaya uyarı çıkar. Firma panelden yeniden dener ya da müşteriye havale yolunu açar. **Havale yolu müşterinin seçimidir** (K-569): sipariş sayfası kart iadesinin gerçekleşmediğini söyler ve o kalem için bir IBAN alanı açar (B-14); müşteri IBAN'ı girerse geri ödeme havaleyle yapılır ve firma ödemeyi işaretler — firma IBAN'ı panelden girmez (§8.4.2). Müşteri IBAN girene kadar firma kart iadesini yeniden denemeye devam edebilir; kart iadesi gerçekleşirse IBAN alanı kapanır, IBAN girildikten sonra kart iadesi yeniden denenmez — aynı para iki yoldan gönderilmez. Dayanak: Mesafeli Sözleşmeler Yönetmeliği m.12/4 geri ödemeyi tüketicinin satın alırken kullandığı ödeme aracına uygun biçimde yapmayı ister; başka bir yol ancak tüketicinin kendi tercihiyle açılır. Yasal süre işlemeye devam eder (§6.4.24; K-521).

**7.2.9 İptalde kargo ücreti sebebe ve kapsama göre ayrılır.** Tetik siparişin tamamı değil, **iptal edilmemiş fiziksel kalemlerinin tamamıdır:** kargoya verilmeden bütün fiziksel kalemler iptal edilirse — müşteri ya da firma, tek seferde ya da ardışık; dijital ve hizmet kalemleri kalsa bile — ödenmiş kargo ücreti son iptalin geri ödemesine eklenir, çünkü taşıma yapılmamıştır. Bir fiziksel kalem kargoyla gidecekse kargo ücreti alıkonur: bu kısmi iptalde kargo ücreti yeniden hesaplanmaz ve geri ödenmez. Yöneticinin kalem çıkarması (§10.4.3) aynı kurala bağlıdır (K-501, K-451, K-213). Geri dönen gönderide (§5.4, S9) sipariş iptal edilirse ürün bedeli geri ödenir; gidiş kargo bedeli yalnız sebep "gönderi teslim edilemedi — müşteri kaynaklı" ise geri ödenmez — taşıma fiilen yapılmıştır. Listedeki diğer sebeplerin tamamında — yanlış adrese gönderim ve hasarlı paket gibi firma kaynaklı durumlar dahil — gidiş kargosu da geri ödenir; ayrım firma iptalinin sebep kaydından okunur (K-212, K-176, K-199, K-451). Gecikme feshinde kargo ücreti her durumda geri ödenir (§7.2.3; K-499).

### 7.3 Cayma

**7.3.1 Cayma penceresi on dört gündür ve sabittir;** firma uzatamaz ve süre §11'e girmez. Pencere kalemin teslim tarihinden işler — fiziksel kalemde firmanın teslim işaretiyle girdiği tarihten (§3.20.11). Parçalı gönderim olmadığı için (§3.20.3) siparişin fiziksel kalemleri tek bir teslim tarihi taşır ve pencere bu tarihten işler; cayma kalem düzeyinde kullanılır (Mesafeli Sözleşmeler Yönetmeliği m.9/3-a; K-203, K-288, K-177). **Tüketici malı teslim almadan da cayabilir:** sipariş kargoya verildikten sonra mal yoldayken müşterinin yolu caymadır (K-340). Pencere kapandığında cayma düğmesi kapanır. Firma teslimi geç işaretlediği için pencere işaret anında çoktan dolmuşsa da düğme kapanır; ama daha önce yapılmış beyan geçerli kalır ve gecikmenin riski firmadadır (K-340, K-07). Pencere işaret anından yeniden başlatılmaz — bilinçli bir seçimdir: yasal pencere fiilî teslimden işler ve doğru girilmiş tarihte süre gerçekten dolmuştur; yanlış girilmiş tarihin riski ve ispat yükü firmadadır ve müşteri caymayı her durumda başka bir kanaldan da bildirebilir (§7.3.5; K-340, K-510).

**7.3.2 Dijital üründe cayma hakkı yoktur; koşulu ayrı bir onaydır.** Mesafeli Sözleşmeler Yönetmeliği m.15/1-ğ'nin *"Elektronik ortamda anında ifa edilen hizmetler veya tüketiciye anında teslim edilen gayrimaddi mallara ilişkin sözleşmeler"* istisnası uygulanır; dijital ürün ikinci kategoridir (K-550). İstisna kendiliğinden işlemez: sepette dijital kalem varsa onay adımında üçüncü bir kutu çıkar ve kutu işaretlenmeden dijital kalem siparişe giremez (§3.24.2). Hak ödeme başarısında — indirmenin açıldığı anda — düşer (K-204).

**7.3.3 Hizmette cayma hakkı, müşterinin onayıyla başlayan ifa tamamlanınca düşer.** Yönetmelik'in istisnası "cayma hakkı süresi sona ermeden önce, tüketicinin onayı ile ifasına başlanan hizmet"e uygulanır ve onay kurucu unsurdur: sepette hizmet kalemi varsa onay adımında zorunlu bir kutu çıkar ve müşteri ifanın cayma süresi dolmadan başlamasını onaylar; kutu işaretlenmeden sipariş verilemez (§3.24.2). Firma hizmet kalemini "tamamlandı" işaretleyene kadar müşteri cayabilir; işaretlendikten sonra hak düşer. **Ürün ifanın başladığı anı izlemez ve kaydetmez:** Yönetmelik'in istisnası ifa onayla başladığında uygulanabilirdi; ürün hakkı tüketici lehine daha geç bir ana — tamamlanma işaretine — bağlar ve ifanın başlaması hakkı düşürmez (K-205). **Kısmen ifa edilmiş hizmetten cayma da tam caymadır:** kalemin siparişte ödenmiş bedelinin tamamı geri ödenir (§7.4); yapılmış kısım için bedel kesilmez — Yönetmelik orantılı bir bedel hükmü tanımaz ve ürün yasal sonuç uyduramaz. Kısmi ifanın maliyeti firmadadır; bu bilinçli bir seçimdir: bir "ifaya başlandı" işareti hizmete ikinci bir elle adım ve başlangıç anı üzerine bir ispat tartışması getirirdi (K-578). Tamamlanmamış hizmette pencere on dört gündür ve **sipariş tarihinden** — sözleşmenin kurulduğu günden — işler; firma kalemi "tamamlandı" işaretlerse hak, pencere dolmamış olsa bile o anda düşer: iki sınırdan hangisi önce gelirse (K-205, K-289). **Fiziksel kalem içeren siparişte** hizmet kaleminin penceresi siparişin teslim tarihinden işler — mal teslimi ile hizmet ifasının birlikte yapıldığı sözleşmede mal hükümleri uygulanır (Mesafeli Sözleşmeler Yönetmeliği m.9/5); teslim tarihi girilmemişse pencere açık kalır ve hak yalnız onaylı ifa tamamlanınca düşer. **Cayılan hizmet kalemi beyanla kapanır:** kalem tamamlanmayı beklemez, siparişin sevkiyat hattı onu beklemez (§5.6.6, §5.8) ve geri ödemenin on dört günü beyandan işler (§7.4.1). **Kalan risk:** m.9/5'in aynı sepetteki bağımsız mal ve hizmet kalemini kapsaması bir yorumdur; ürün tüketici lehine okumayı seçer (K-503).

**7.3.4 Fiziksel üründe cayma istisnası ürün düzleminde isteğe bağlı bir işarettir.** Ürün formunda bir "cayma hakkı istisnası" kutusu ve Mesafeli Sözleşmeler Yönetmeliği m.15/1 bentlerinin metnine çekilmiş kapalı bir sebep listesi bulunur (K-206, K-509):

- **(b)** tüketicinin istekleri veya kişisel ihtiyaçları doğrultusunda hazırlanan mallar — **mutlak**;
- **(c)** çabuk bozulabilen veya son kullanma tarihi geçebilecek mallar — **mutlak**;
- **(ç)** tesliminden sonra ambalaj, bant, mühür, paket gibi koruyucu unsurları açılmış olan mallardan iadesi sağlık ve hijyen açısından uygun olmayanlar — **koşullu**;
- **(e)** malın tesliminden sonra koruyucu unsurları açılmış olması hâlinde maddi ortamda sunulan kitap, dijital içerik ve bilgisayar sarf malzemeleri — **koşullu**.

**Mutlak sebepte** cayma düğmesi gösterilmez. **Koşullu sebepte** düğme açık kalır; beyan ekranı ve Ön Bilgilendirme Formu *"koruyucu ambalajı açılmamışsa cayabilirsiniz"* koşulunu yazar ve koruyucu ambalajı açılmış dönen malda ret firmanın kararıdır. İşaretli ürün istisnayı, sebebini ve — koşullu sebepte — koşulu ön bilgilendirmede gösterir. İşaret varyanta inmez, ürünün bütün varyantlarını kapsar; yayın kapısına girmez (K-206, K-509). Listede olmayan bentler — karışan mallar, süreli yayın, belirli tarihli konaklama ve eğlence hizmetleri — hedef kitlede olağan değildir; ihtiyaç doğarsa sebep bir karar satırıyla eklenir (K-509).

**7.3.5 Cayma sipariş sayfasından bildirilir.** Müşteri kalemi seçip cayma beyanında bulunur; bildirim **tarih damgasıyla** kayda geçer ve pencerenin içinde yapılıp yapılmadığı bu damgadan okunur. Ayrı bir form doldurma, e-posta yazma ya da noter kanalı zorunluluğu yoktur (K-207). Sipariş sayfası birincil kanaldır, tek yasal kanal değildir: müşteri caymayı e-postayla, mektupla ya da örnek cayma formuyla da bildirebilir; firma bu bildirimi panelden kaydeder ve beyanın tarihi bildirimin firmaya ulaştığı tarih olur (§10.4.10; K-510). Koşullu istisna sebebi taşıyan üründe beyan ekranı koruyucu ambalaj koşulunu gösterir (§7.3.4; K-509). Fiziksel kalemin beyan ekranı firmanın iade adresini ve müşterinin malı on dört gün içinde gönderme yükümlülüğünü gösterir (§7.4.4); hizmet kaleminin beyan ekranı ikisini de göstermez — geri gönderilecek mal yoktur (K-530). Müşteri beyanın tarihini taşıyan bir e-posta alır (K-293, K-319, K-494). Havale ile ödenmiş siparişte müşteri geri ödeme için IBAN'ını beyan sırasında girer (§7.4.5); firmanın kaydettiği beyanda IBAN müşterinin bildiriminden gelir (§8.4.2; K-510).

**7.3.6 Cayma beyanı siparişi kapatmaz.** Beyan hakkın kullanıldığını kayda geçirir ve iade sürecini **başlatır**; sevkiyat durumu değişmez, ödeme durumu paranın geri ödendiği anda değişir (K-208; §5.6.6).

**7.3.7 Kargoya verilmemiş fiziksel kalemde tüketicinin yolu iptaldir.** Fiziksel kalemin cayma düğmesi sipariş "Kargoya verildi"ye geçtiği andan itibaren görünür; ondan önce tüketici sebep göstermeden iptal düğmesiyle döner (§7.2.1). Bu aralıkta iptal, cayma hakkının sonucunu aynen verir: firma onayı beklenmez, para on dört gün içinde geri ödenir (§7.2.8) ve geri gönderilecek bir mal yoktur. İptal yolu kalem tipine göre kapanır — fiziksel kalem kargoya verilince, dijital kalem ödeme onayında, hizmet kalemi tamamlama işaretinde; K-196'nın sipariş düzeyindeki cümlesi fiziksel kalemin kuralıdır (K-340, K-196, K-200, K-452).

### 7.4 İade ve geri ödeme

**7.4.1 Geri ödemenin on dört günü, cayma beyanı kalemin teslim tarihinden önce yapıldıysa ya da kalem hizmetse beyandan, teslimden sonra yapıldıysa iade edilen malın firmaya ulaştığı tarihten işler.** Teslimden önceki caymada ve hizmette sistem sayacı beyanın tarih damgasından başlatır (§7.3.5). Teslimden sonraki caymada sayaç, firmanın iade malını teslim alma adımında girdiği ulaşma tarihinden başlar; tarih geçmişe dönük olabilir ama kalemin teslim tarihinden önce ve ileri tarihli olamaz (§7.4.7). Ulaşma tarihi beyandan önce olabilir — müşteri malı gönderip beyanı sonra yapmış olabilir; o durumda sayaç beyandan başlar: başlangıç ikisinden geç olanıdır, çünkü geri ödeme yükümlülüğü beyandan önce doğmaz (K-554). Ayrımı sistem, kalemin teslim tarihini (§3.20.11) beyanın damgasıyla karşılaştırarak yapar: teslim tarihi girilmemiş kalemde beyan teslimden önce sayılır; beyan teslim tarihiyle aynı gün yapılmışsa teslimden sonra sayılır — mal o gün müşteridedir ve geri gönderilmesi gerekir (K-520); teslim tarihi sonradan girilir ya da düzeltilirse (§10.4.5) ayrım o tarihe göre yapılır. Kısmi iadede her kalem kendi ulaşma tarihini taşır (K-178). **Mal ulaşmadan teslim sonrası caymanın süresi başlamaz; bu bir bekletme hakkı değil, sürenin başlangıç kuralıdır** — Yönetmelik satıcıya geri ödemeyi bekletme hakkı tanımaz. Panel iki modda gösterir: süre işliyorsa kalan günü; teslimden sonraki caymada mal ulaşana kadar "iade malı bekleniyor" — beyandan geçen gün ve müşterinin on dört günlük gönderme süresi (§7.4.4). Müşteri malı hiç göndermezse süre başlamaz; gönderme süresi geçtikten sonra firma kalemi "mal dönmedi" gerekçesiyle kapatabilir ve mal sonradan ulaşırsa kalem yeniden açılır (§10.4.9; K-496). **Dayanak:** Mesafeli Sözleşmeler Yönetmeliği m.12/1–3 (23/8/2022 tarihli değişiklik, 1/1/2026'dan beri uygulanır). Firma iade taşıyıcısı belirlemediği için (§7.4.4) teslimden sonraki her iade "iade için öngörülenin haricinde bir taşıyıcı" ile yapılır ve süre malın satıcıya ulaşmasıyla başlar; teslimden önceki caymada ve hizmette süre bildirimin satıcıya ulaşmasıyla başlar — beyan siteden yapıldığı için bu an beyanın damgasıdır. **Bedeli bilinçlidir:** işaretin anı firmanın elindedir; ispat yükü firmadadır ve uydurulmuş ya da geciktirilmiş tarih firmanın aleyhine işler (K-07, K-288). **Kalan risk:** taşıyıcı belirtilmediğinde sürenin malın ulaşmasıyla başlaması metnin lafzından çıkar, açık bir hükmü yoktur (K-491).

**7.4.2 Cayma hâlinde iade kargo bedeli firmaya aittir;** panelde bunu değiştiren bir ayar yoktur ve ön bilgilendirmede böyle yazar (K-210). Bu bir ürün tercihi değil yasal zorunluluktur: satıcı ön bilgilendirmede iade için taşıyıcı belirtmediğinde Yönetmelik tüketiciden iade masrafı için bedel istenmesini yasaklar (m.12/5) ve firma taşıyıcı belirtmez (K-492, K-493).

**7.4.3 Ayıplı malda iade kargo bedeli her hâlükârda firmaya aittir.** Ayıplı mal ve garanti kapsamındaki geri gönderimlerde masraf cayma rejiminden bağımsız olarak firmanındır; dayanağı ayıplı mal hükümleridir (K-211).

**7.4.4 Müşteri malı firmanın iade adresine karşı ödemeli gönderir.** Cayma ve ayıp talebinde geri gönderim, bedeli teslim alırken firma tarafından ödenmek üzere yapılır; müşteri masrafı önden ödemez. İade adresi zorunlu bir firma ayarıdır (§3.1.7) ve cayma ya da ayıp beyanı ekranında müşteriye gösterilir. Taşıyıcıyı müşteri seçer — firma taşıyıcı belirlemez, ürün iade etiketi üretmez ve Ön Bilgilendirme Formu taşıyıcı belirlenmediğini yazar (K-293, K-210, K-132, K-493). Firma malı kendisi geri almayı teklif etmez; caymada müşteri malı beyandan itibaren on dört gün içinde gönderir. Yükümlülük formda ve beyan ekranında yazar; süre dolunca sistem caymaya ve kaleme dokunan kendiliğinden bir işlem başlatmaz (K-494) — havale hattında yalnız müşterinin IBAN'ı, sürenin bitiminden otuz gün sonra silinir (§7.4.5; K-497). Süre geçtikten sonra malı dönmemiş kalemi firma panelden kapatabilir (§10.4.9; K-496). Taşıyıcı belirlenmediği için teslimden sonraki caymada geri ödeme süresi malın firmaya ulaştığı tarihten işler (§7.4.1). Bedeli bilinçlidir: firma iade kargo tutarını önceden bilmez; karşılığında kart hattında ikinci bir para hattı açılmaz ve müşteriden IBAN istenmez (K-293, K-214).

**7.4.5 Geri ödeme, ödemenin geldiği yoldan gider.** Kartla ödenen sipariş **karta** geri ödenir (sağlayıcı üzerinden); havale ile ödenen sipariş müşterinin **IBAN'ına** gönderilir. Havale hattında müşteri IBAN'ını iptal, gecikme feshi ya da cayma beyanı sırasında girer (K-499); kart hattında IBAN alanı yoktur ve istenmez (K-214). Sipariş sayfası IBAN'ı girişte biçim ve sağlama basamağıyla denetler. **Müşteri girdiği IBAN'ı geri ödeme işlenene kadar sipariş sayfasından düzeltebilir;** firma geri ödemeyi işlediği anda alanda duran IBAN'ı kullanır. **Firma geri ödemeyi havale bankasında gerçekleştikten sonra işler.** Banka havaleyi reddederse ya da para firmaya geri dönerse firma geri ödemeyi işlemez, panelden "havale gerçekleşmedi" der: kalemin IBAN'ı silinir, sipariş sayfasında o kalem için IBAN alanı yeniden açılır ve müşteriye bildirim gider (§9.2 B-14); kalem "IBAN bekleniyor" listesine döner ve geri ödemenin süresi durmaz. Adım işlem izine yazılır (§10.3.1). **Kalan risk:** firma geri ödemeyi işledikten sonra geri dönen havalede ödeme ekseninin düzeltmesi yoktur (§10.4.6); firma müşteriye siparişin iletişim bilgisinden ulaşır ve parayı yeniden gönderir (K-575). IBAN yalnız o geri ödemenin yürütülmesi için tutulur: para gönderildiğinde silinir, müşterinin hesabına kaydedilmez ve sonraki iadede yeniden istenir; ödemenin yapıldığı kayıt ticari kayıt olarak kalır (K-356). **Malı dönmeyen caymada IBAN daha erken silinir:** firma kalemi "mal dönmedi" gerekçesiyle kapattığında kapatmayla birlikte (§10.4.9); firma kapatmazsa ve mal hâlâ ulaşmamışsa, müşterinin gönderme süresinin (§7.4.4) bitiminden otuz gün sonra kendiliğinden (§4 Z-38, §11 P-43). Kendiliğinden silme kalemi kapatmaz, caymayı etkilemez ve müşteriye bildirilmez. Aynı beyanla girilen IBAN'ı geri ödemesi işlenmeyi bekleyen başka bir kalem kullanıyorsa IBAN o geri ödeme tamamlanana kadar kalır. Mal IBAN silindikten sonra ulaşırsa geri ödeme için IBAN müşteriden yeniden istenir (K-497): firma teslim alma adımını işlediğinde müşterinin sipariş sayfasında o kalem için IBAN alanı yeniden açılır ve müşteri IBAN'ı oradan girer; firma IBAN'ı panelden girmez — IBAN'ın tek giriş yolu müşterinin kendi beyanıdır (§8.4.2). Aynı anda müşteriye bir işlem bildirimi gider (§9.2 B-14); e-posta ulaşmasa da sipariş sayfası alanı gösterir (§6.1.1). Geri ödemenin on dört günü malın ulaştığı tarihten işler (§7.4.1) ve IBAN beklenirken durmaz — Yönetmelik bir durma hâli tanımaz. Panel kalemi "IBAN bekleniyor" olarak ve kalan süreyle gösterir; IBAN isteğinin tarihi işlem izine yazılır (§10.3.1) ve gecikmenin müşteriden kaynaklandığını gösteren kanıt firmanın elinde kalır. Müşteri IBAN'ı girdiğinde firma geri ödemeyi mevcut elle adımla işler (§10.5.2) ve yeniden girilen IBAN da geri ödeme tamamlanınca silinir. Aynı beyanın IBAN'ı başka bir kalem için hâlâ duruyorsa yeniden istenmez (K-498). **Müşteri IBAN'ı hiç girmezse** kalem kapanmaz ve borç sürer: kalem panelde "IBAN bekleniyor" listesinde görünür ama firmanın yapabileceği bir iş olmadığı için bekleyen işler sayacında (§10.6.1) sayılmaz; firma müşteriye ulaşmak için siparişin iletişim bilgisini görür. Belirli bir süre sonra kendiliğinden kapanma yoktur — firmanın borcu müşteri sessiz kaldı diye düşmez (K-525). Aynı istek yolu, kart iadesi sağlayıcıda başarısız olduğunda firmanın müşteriye havale yolunu açtığı kalemde de kullanılır; orada IBAN girmek müşterinin seçimidir (§7.2.8; K-521, K-569). Başka bir kanaldan gelen ve IBAN taşımayan cayma bildiriminin kaydında da aynı yol kullanılır (§10.4.10; K-581). Firmanın elle işlediği geri ödeme geri alınamaz onayı ister (§5.9).

**7.4.6 Kısmi iadede kargo ücreti ve ücretsiz kargo kararı yeniden hesaplanmaz.** Sipariş anında hesaplanan kargo ücreti ya da ücretsiz kargo donmuştur; bir kalem iade edildiğinde kalan tutar eşiğin altına düşse bile kargo ücreti müşteriden geri istenmez ve geri ödeme tutarından düşülmez (K-213). Caymada tetik **kargoyla gönderilen fiziksel kalemlerin tamamıdır:** bunların tamamından cayıldığında — tek seferde ya da ardışık beyanlarla; kargodan önce iptal edilmiş kalemler hesaba katılmaz — teslimat masrafı da tüketiciye geri ödenir ve kargo ücreti son caymanın geri ödemesine eklenir (K-501, K-212). Gönderilen fiziksel kalemlerden biri müşteride kalırsa siparişin tek seferlik kargo ücreti geri ödeme tutarına girmez — taşıma o kalem için de fiilen yapılmıştır; tek ücret kalem sayısından bağımsızdır ve bölünecek bir pay yoktur (K-292). K-213'ün "hiçbir hâlde yeniden hesaplanmaz" kuralı bu iki istisnayla — §7.2.9'un iptal tetiği ve bu tetik — okunur (K-213, K-501). **Kalan risk:** kısmi cayma Yönetmelik'te açıkça düzenlenmemiştir; tüketici lehine bir yorum her kısmi caymada kargo iadesi isteyebilir (K-501).

**7.4.7 İade edilen fiziksel kalemin adedi stoğa kendiliğinden eklenmez.** Firma malı teslim alır ve panelin teslim alma adımında malın ulaştığı tarihi girer — tarih geçmişe dönük olabilir ama ileri tarihli olamaz ve kalemin teslim tarihi girilmişse ondan önce olamaz; panel kuralın dışındaki tarihi sebebini söyleyerek reddeder. Teslimden önce cayılıp firmaya dönen gönderide (§6.4.22) teslim tarihi yoktur: bu alt sınır uygulanmaz ve adım aynı biçimde işler — geri ödemenin süresi zaten beyandan işlemektedir (§7.4.1). Bu tarih teslimden sonraki caymada geri ödemenin on dört gününü başlatır; mal beyandan önce ulaştıysa süre beyandan işler (§7.4.1; K-491, K-554). Tarih alanı mevcut adımın parçasıdır, yeni bir elle adım değildir (§10.5.2). Firma ardından malın durumunu kontrol eder ve panelden stoğa ekler. İptalden bilinçli ayrışmadır: iptalde mal hiç yola çıkmadığı için stok kendiliğinden döner, iadede mal gitmiş ve geri gelmiştir (K-215).

**7.4.8 Caymada hizmet kontenjanı havuza döner; kuponun kullanım hakkı yalnız siparişin tamamı iade edildiğinde sayaca döner.** Kısmi iadede hak dönmez (K-216, K-202). Dijital ürünün dönecek sayacı yoktur ve zaten cayma kapsamı dışındadır (K-204).

### 7.5 Ayıp talebi

**7.5.1 Ayıp talebi üçüncü yoldur.** Cayma sebepsizdir ve on dört günle sınırlıdır; ayıp talebi **sebeplidir** ve sipariş sayfasındaki kanalı teslimden itibaren **iki yıllık zamanaşımı** boyunca açıktır. Süre yasal bir sabittir ve §11'e girmez (K-226). Süre dolunca kanal kapanır, hak ortadan kalkmaz: müşteri iletişim formundan başvurabilir; ayıp ağır kusur ya da hileyle gizlenmişse zamanaşımı işlemez; firma daha uzun bir sorumluluk üstlendiyse o süre geçerlidir ve sonrası iletişim formundan yürür (6502 sayılı Kanun m.12, m.16; Z-18; K-551). Başlangıç kalemin teslim işaretinin tarihidir: fiziksel kalemde teslim tarihi, hizmet kaleminde "tamamlandı" işareti, dijital kalemde ödeme onayı (K-290).

**7.5.2 Kanal sipariş sayfasındaki "sorun bildir"dir.** Müşteri kalemi seçer ve açıklama yazar; talep kalem ve sipariş bağlamıyla birlikte firmanın paneline düşer. Fotoğraf ya da dosya yükleme yoktur — firma kanıt gerekiyorsa müşteriden e-postayla ister (K-227). Ayıp talebi iletişim formunun konu tiplerinde yoktur; kendi kanalından gider (K-302). **Kanalın açıldığı an:** fiziksel kalemde "sorun bildir" sipariş "Kargoya verildi"ye geçtiği andan açıktır; dijitalde ödeme onayından, hizmette "tamamlandı" işaretinden açılır. İki yıllık süre teslim tarihinden işler; teslim tarihi girilmemişse süre işlemez ve bunun riski firmadadır — firmanın işaretlemeyi unutması müşterinin yasal ayıp hakkını kapatmaz (§7.3.1'in kalıbı; K-506, K-340).

**7.5.3 Sistem talebi kaydeder ve iletir, çözümü yürütmez.** Talebin iki durumu vardır: Açık ve Çözüldü (§5.10); firma talebi çözüldü olarak işaretler. Çözüm tutmazsa firma ya da müşteri talebi yeniden açar ve ayıbın geçmişi tek kayıtta kalır (K-297). Kanunun tüketiciye tanıdığı seçimlik haklar — onarım, değişim, bedel indirimi, sözleşmeden dönme — firmanın müşteriyle sistem dışında yürüttüğü süreçlerdir. Çözüm geri ödeme gerektiriyorsa sözleşmeden dönmede §7.4'ün kalem düzeyindeki iade hattı, bedel indiriminde ise tutar bazlı kısmi geri ödeme kullanılır — bedel indiriminde kalem iade edilmez (§5.5; K-228, K-539).

**7.5.4 Ayıplı dijital üründe doğal çözüm dosya güncellemesidir.** Talep aynı kanaldan gelir; firma dosyayı düzeltip ürüne yeniden yükler ve düzeltilmiş dosya eski alıcılara da ulaşır (§3.12.7). Geri ödeme gerekirse §7.5.3'ün hattı işler (K-229, K-148).

Garanti bilgisinin yeri §3.11.7'dedir (K-230).

### 7.6 Geri alma sınırları

**7.6.1 Panel "geri al" düğmesi sunmaz.** Dört geçiş — havalede "ödendi" işareti, kargoya verme, firma kaynaklı iptal, geri ödemenin firma tarafından işlenmesi — önceden geri alınamaz onayı ister. Yanlış yapılmış bir geçiş, yöneticinin sebep seçerek yaptığı **yeni bir geçişle** düzeltilir; düzeltme izi temizlemez ve dış dünyaya çıkmış sonucu — gönderilen e-posta, kargoya verilen paket, karta gönderilen para — geri almaz (§5.4, §5.9, §10.4; K-225, K-372).

**7.6.2 Süresi dolup iptal edilmiş siparişe gelen ödeme işlenemez;** firma parayı geri öder ya da müşteriden yeni sipariş ister (§3.21.8; K-183).

**7.6.3 Müşteri akışında iptal edilmiş ya da teslim edilmiş sipariş yeniden açılmaz;** sevkiyat ekseninde müşteri akışının geri yönlü geçişi yoktur (§5.4; K-223, K-224). Yöneticinin yanlış işareti düzeltmesi bu kuralın dışındadır ve sınırları — terminal durumdan yalnız bir önceki duruma çıkış, iptalin düzeltilmesinin koşulu, yan etkiler ve eksen bağı — §10.4.6'dadır (K-504, K-566).

**7.6.4 Kurumsal içerikte eski hâle dönülmez ve silme geri alınmaz:** sürüm geçmişi yoktur, silme geri alınamaz işlem uyarısıyla yapılır (§3.27.26, §3.27.27; K-276, K-277).

**7.6.5 Silinmiş bir hesabın siparişleri hiçbir hesaba geri bağlanmaz** (§3.15.3; K-117).

**7.6.6 Yöneticinin sipariş düzeltmeleri ileri yönlü kayıtlardır ve tutarı değiştirmez.** Adres, kalem adedi ya da kalemin çıkarılması, takip bilgisi, teslim tarihi ve durum düzeltilebilir; kalem eklenemez ve fiyat değiştirilemez — onaylanan tutar bağlayıcıdır. Çıkarılan kalemin parası §7.2.8'in hattından geri ödenir (§10.4; K-369, K-374, K-77, K-80, K-291).

*Kaynak: K-195 · K-226 (üç yol) · K-07 (tek rejim) · K-178 (kalem düzeyi) · K-196…K-202 (iptal) · K-203…K-208 (cayma) · K-209…K-216 (iade ve geri ödeme) · K-226…K-230 (ayıp talebi) · K-491 (K-209'u değiştirir — geri ödeme süresinin başlangıcı) · K-492 · K-493 · K-494 · K-496 · K-83 · K-117 · K-148 · K-176 · K-183 · K-187 · K-223 · K-224 · K-225 · K-276 · K-277 · K-77 · K-80 · K-132 · K-233 · K-288 · K-289 · K-290 · K-291 · K-292 · K-293 · K-295 · K-297 · K-302 · K-317 · K-319 · K-340 · K-356 · K-366 · K-367 · K-368 · K-369 · K-372 · K-373 · K-374 · K-451 · K-452 · K-497 · K-498 · K-499 · K-503 · K-448 · K-112 · K-501 · K-504 · K-505 · K-506 · K-530 · K-539 · K-520 · K-554 · K-509 · K-510 · K-521 · K-525 · K-543 · K-550 · K-551 · K-177 · K-566 · K-569 · K-571.*

## 8. Kötüye kullanım ve güvenlik kuralları

> **Ne yazılır:** Kötüye kullanım senaryoları ve karşı önlemler. Teknik güvenlik değil, **ürün düzeyinde** kurallar (limit, cooldown, doğrulama zorunluluğu).

Bu bölüm ürün düzeyindeki önlemleri toplar. Teknik güvenlik — genel istek hızı sınırı, yüklenen dosyanın güvenliği, depolama ve şifreleme — `05`'in işidir (K-331, K-143). Sayısal eşikler §11'de, süreler §4'tedir.

### 8.1 Kimlik doğrulama tabanı

Kuralların tam metni §3.13'tedir; burada güvenlik açısından toplanır.

**8.1.1 Doğrulanmamış hesapla giriş ve sipariş yoktur.** E-posta doğrulaması sert bir kapıdır; doğrulanmamış kayıt, bağlantının ömrü dolunca silinir (§3.13.4, §3.13.5; K-101, K-102).

**8.1.2 Bir siparişin ya da bir giriş yolunun hesaba bağlanmasının tek dayanağı doğrulanmış e-postadır.** Sipariş sayfasına erişim bu kurala değil, §8.3.5'in anahtarlarına dayanır. Geçmiş misafir siparişleri hesaba yalnız doğrulanmış e-postayla düşer; Google ile girişte mevcut hesaba bağlama yalnız sağlayıcının doğrulanmış verdiği e-postayla yapılır (§3.13.2, §3.13.7; K-98, K-104). Hesabı olan kişinin giriş yapmadan verdiği misafir siparişi hesaba anında düşer. **Kalan risk bilinçlidir:** e-postasını yanlış yazan alıcının siparişi bir yabancının hesabına düşer ve hesap sahibi o siparişi iptal edebilir ya da iade talebi açabilir; riski daraltan çit konmaz (§3.13.3; K-99). Firma siparişin e-postasını düzelttiğinde bağ kesilir (§10.4.11; K-585).

**8.1.3 Bağlantılar süreli ve tek kullanımlıktır.** Şifre sıfırlama ve yönetici daveti bağlantıları bir kez kullanılır ve ömürleri dolunca geçersizleşir; şifre sıfırlandığında hesabın bütün oturumları kapanır (§3.13.9, §3.13.10, §10.2; K-105, K-106, K-308).

**8.1.4 Ekranlar hesabın varlığını kayıtta ve ödeme adımının hatırlatmasında ele verir.** Şifre sıfırlama ekranı nötr konuşur; kayıt ekranı "bu e-posta zaten kayıtlı" der (§3.13.11; K-107), ödeme adımı kayıtlı e-postayı hatırlatır (§3.13.3; K-99). **Kalan risk bilinçlidir:** kayıt ekranından bir adresin üye olup olmadığı sorgulanabilir; hesap kaydı limiti (L-3) bunu daraltır, ortadan kaldırmaz (K-107).

**8.1.5 Üç işlem oturum açıkken de yeniden doğrulama ister:** şifre değiştirme, e-posta adresi değiştirme ve hesap silme. Şifresi olmayan hesapta yeniden doğrulama Google ile yapılır (K-121). Yeni e-posta adresi doğrulanmadan değişiklik geçerli olmaz (§3.13.13, §3.13.14; K-109, K-110).

**8.1.6 Şifre politikası uzunluk tabanlıdır** ve çok yaygın şifreleri reddeder; liste ürünle gelir (§3.13.15; K-120). Google ile açılmış hesaba şifre "şifremi unuttum" akışıyla konur (§3.13.8; K-121).

**8.1.7 Yönetici tarafı aynı rejimdedir.** Yönetici hesapları aynı doğrulama, oturum, yeniden doğrulama, e-posta değiştirme ve şifre kurallarına aynı değerlerle tabidir; MVP'de iki adımlı doğrulama yoktur. Kaldırılan yöneticinin açık oturumları anında sonlanır (§10.2; K-122, K-313, K-314, K-384).

### 8.2 Deneme limitleri

Sekiz limit vardır; her birinin eşiği ve — sayma limitlerinde — zaman penceresi §11'dedir (K-328, K-460, K-524). Pencere kayan penceredir; L-8 pencere değil, aynı anda açık kayıt sayar.

| # | Limit | Ölçüm ekseni | Aşıldığında | Kaynak |
|---|---|---|---|---|
| L-1 | Başarısız giriş denemesi | Aynı IP ve aynı e-posta adresi — ayrı ayrı | Giriş geçici olarak engellenir | K-328, K-330 |
| L-2 | Şifre sıfırlama talebi | Aynı IP ve aynı e-posta adresi — ayrı ayrı | Talep geçici olarak engellenir | K-328, K-330 |
| L-3 | Hesap kaydı | Aynı IP ve aynı e-posta adresi — ayrı ayrı | Kayıt geçici olarak engellenir | K-328, K-330 |
| L-4 | İletişim formu gönderimi | Aynı IP | Gönderim geçici olarak engellenir | K-304, K-328, K-330 |
| L-5 | Başarısız misafir sipariş takibi sorgusu | Aynı IP | Sorgu geçici olarak engellenir | K-185, K-187, K-328, K-330 |
| L-6 | Geçersiz kupon kodu denemesi | Aynı sepet — misafirde tarayıcının sepeti, üyede hesabın sepeti; girişte birleşen sepet iki sayacın büyüğünü taşır | O sepetin kupon alanı geçici olarak kapanır | K-334, K-330, K-555 |
| L-7 | Kendiliğinden iptal edilen ödenmemiş sipariş — ödeme süresi dolmuş ya da kart ödemesi başarısız olmuş | Aynı IP ve aynı e-posta adresi — ayrı ayrı; e-posta ekseni yalnız giriş yapmış üyenin siparişlerini sayar, misafir siparişi yalnız IP ekseninde | Kullanıcı bir süre yeni sipariş onaylayamaz | K-332, K-184, K-528, K-579 |
| L-8 | Aynı anda açık ödenmemiş sipariş — ödeme Bekliyor'daki siparişler | Aynı IP ve aynı e-posta adresi — ayrı ayrı; e-posta ekseni yalnız giriş yapmış üyenin siparişlerini sayar, misafir siparişi yalnız IP ekseninde | Tavan doluyken yeni havale siparişi açılamaz; kart yolu açık kalır. Açık siparişlerden biri ödenince ya da iptal edilince tavan boşalır | K-524, K-332, K-128, K-579 |

**8.2.1 Limit aşıldığında yalnız o işlem geçici olarak engellenir.** Engel, pencere içindeki deneme sayısı eşiğin altına indiğinde kendiliğinden açılır. **Hesap kilitlenmez** — kilitlenen hesap, e-postayı bilen birinin başkasının hesabını kilitlemesine araç olurdu ve her kilit panele elle açılacak bir iş düşürürdü. Kullanıcı nötr bir mesaj görür; mesaj ne kalan deneme sayısını ne engelin süresini söyler (§6.1.5; K-329).

**8.2.2 İki eksen ayrı sayılır.** Giriş, şifre sıfırlama, hesap kaydı, kendiliğinden iptal edilen ödenmemiş sipariş (L-7) ve açık ödenmemiş sipariş tavanında (L-8) aynı IP'den gelen denemeler ile aynı e-posta adresine yönelen denemeler kendi eşiğini taşır; hangisi aşılırsa o eksen engellenir. Böylece farklı IP'lerden tek bir hesaba yönelen dağıtık deneme de, tek IP'den çok sayıda adrese yönelen deneme de görünür. Kupon kodu misafirce de denendiği için sepet ekseninde sayılır — misafirde tarayıcının sepeti, üyede hesabın sepeti; girişte birleşen sepet iki sayacın büyüğünü taşır, mekanizma `05`'tedir (K-330, K-332, K-528, K-555). **L-7 ve L-8'de e-posta ekseni yalnız giriş yapmış üyenin verdiği siparişleri, hesabın e-postasıyla sayar;** misafir siparişi — hesaba anında ya da sonradan düşse de — yalnız IP ekseninde sayılır. Misafirin yazdığı e-posta doğrulanmamıştır: başkasının adresiyle art arda ödenmemiş sipariş açan biri, o kişinin bir gün boyunca sipariş vermesini ya da havaleyle ödemesini engelleyebilirdi — hesap kilidini eleyen gerekçenin aynısı (§8.2.1). E-postasını serbestçe değiştirebilen misafir saldırganı e-posta ekseni zaten durdurmuyordu; onu IP ekseni sayar. Engele takılan kullanıcıya ayrı bir kurtarma ya da muafiyet yolu açılmaz — engel kendiliğinden açılır ve L-8'de kart yolu açıktır. **Kalan risk:** paylaşılan bir IP'nin (ofis, mobil operatör) arkasındaki gerçek müşteri başkasının denemeleriyle IP ekseninde geçici olarak engellenebilir (K-579, K-329).

**8.2.3 L-7 tekrarı ölçer, tek denemeyi değil.** Kart ödemesi olağan biçimde de başarısız olur; eşik, ödemeden sipariş açarak stoğu ve kupon hakkını tekrar tekrar ayıranı durdurur. Firma bu siparişleri panelde görmeye devam eder (§3.17.8; K-332, K-184, K-128). **L-8 eşzamanlı kilidi durdurur:** havale siparişi ancak ödeme süresi dolunca (iş günleri) iptal olduğu için L-7 o süre boyunca devreye girmez; farklı sepet ya da tarayıcıyla art arda havale siparişi açan biri bir ürünün bütün stokunu, hizmet kontenjanını ya da kuponun son haklarını kilitleyebilirdi. Tavan yalnız havale yolunu kapatır — kart siparişinin kilidi dakikalar sürer (K-524, K-128). L-7 ile L-8 ayrı sayaçlardır: biri iptal sıklığını, öteki eşzamanlı kilidi ölçer.

**8.2.4 Yönetici de aynı limitlere aynı değerlerle tabidir;** panel girişine muafiyet tanınmaz (K-333).

**8.2.5 Sepet ve stok tarafında ürün düzeyinde limit yoktur.** Sepetteki kalem sayısına sınır konmaz ve stok yoklamaya ayrı bir limit eklenmez — stok adedi hiçbir yerde söylenmediği için öğrenilmesinin değeri düşüktür. Genel istek hızı sınırı bir altyapı kararıdır ve `05`'e aittir (K-331, K-154, K-150, K-53).

### 8.3 Form ve site güvenliği

**8.3.1 İletişim formu üçüncü taraf captcha kullanmaz.** Form görünmez tuzak alanla — doldurulduğunda gönderim sessizce düşer — ve IP başına gönderim limitiyle (L-4) korunur (§3.32.7; K-304).

**8.3.2 Siteye kod eklenmez.** Firma panelden analitik kodu, reklam dönüşüm etiketi, sosyal medya pikseli ya da başka bir betik giremez; eklenen kod ödeme ve üyelik sayfalarında da çalışırdı (K-263, K-399).

**8.3.3 Sitede üçüncü taraf gömme içerik yoktur:** gömülü harita, gömülü video, sohbet aracı ve captcha bulunmaz — sayfayı açan her ziyaretçinin verisi üçüncü tarafa giderdi (K-243, K-254, K-304, K-388). Ürünün çerez envanteri bu yüzden yalnız zorunlu, birinci taraf çerezlerden oluşur; tek istisna ödeme sağlayıcısının çerçevesidir — çerçeve kullanılıyorsa sağlayıcının zorunlu çerezleri ödeme adımında bulunur (§12.2.5; K-348, K-518).

**8.3.4 Ürün kart verisini görmez, saklamaz, loglamaz.** Kart bilgisi ödeme sağlayıcısının barındırdığı sayfada ya da çerçevede girilir ve her kart ödemesi 3D Secure'dan geçer (§3.21.1, §3.21.3; K-12, K-13). Çerçeve elenmez: bazı sağlayıcılar yalnız çerçeve sunar; çerçevenin zorunlu çerezleri çerez politikasında anılır (§12.2.5; K-518).

**8.3.5 Sipariş erişimi tahmin edilemez anahtarlara dayanır.** Sipariş numarası tahmin edilemez bir koddur; misafir alıcı sayfaya numara + e-postayla ya da sipariş e-postasındaki bağlantıyla girer. Bağlantı numaradan ayrı, siparişe özgü bir erişim anahtarı taşır; numara tek başına sayfayı açmaz (§3.22; K-185, K-187, K-562).

**8.3.6 Misafir siparişinin e-postası yalnız firmanın teyidiyle değişir.** Müşteri siparişin e-postasını kendisi değiştiremez; sipariş sayfasında böyle bir alan yoktur. Firma, müşterinin talebi üzerine kimliği siparişteki bilgilerle — ad, fiziksel kalemli siparişte teslimat telefonu, kalemler ve tutar — teyit ederek düzeltir; sipariş numarası tek başına teyit değildir, çünkü numara dolaşır (§3.22.5). Düzeltme eski erişim anahtarını geçersiz kılar, eski adrese B-15'i gönderir ve işlem izine eski ve yeni değerle yazılır (§10.4.11). **Kalan risk:** firmayı kandıran biri siparişin e-postasını kendi adresine çevirtip sipariş sayfasına ve havale hattında IBAN alanına erişebilir. Ürün teyidi denetlemez; teyidin ispatı ve kandırılmanın sonucu firmadadır. B-15 gerçek sahibini uyarır (§6.3.5; K-585).

### 8.4 Veri asgariliği

**8.4.1 Ürün yalnız işlediği kişisel veriyi toplar.** Pazarlama onayı toplanmaz ve pazarlama listesi tutulmaz (K-324); açık rıza kaydı yoktur (K-345); doğum tarihi alanı yoktur (K-114); ziyaretçi ölçümü yoktur (K-397); formlarda ve ayıp talebinde dosya eki yoktur (K-301, K-227).

**8.4.2 IBAN yalnız gerektiğinde istenir ve iş bitince silinir.** Yalnız havale hattındaki geri ödemede istenir; geri ödeme tamamlanınca silinir ve hesaba kaydedilmez. Malı dönmeyen caymada geri ödeme hiç yapılmayabilir; IBAN bu hâlde de süresiz kalmaz — kalem "mal dönmedi" gerekçesiyle kapatıldığında ya da müşterinin gönderme süresinin bitiminden otuz gün sonra silinir (§7.4.5; K-214, K-356, K-497). Mal sonradan ulaşırsa IBAN'ı müşteri sipariş sayfasından yeniden girer; firma IBAN'ı panelden girmez — IBAN'ın tek giriş yolu müşterinin kendi beyanıdır (§7.4.5; K-498). **Tek istisna** müşterinin başka bir kanaldan — e-posta, mektup, örnek form — yaptığı cayma bildirimidir: firma bildirimi panelden kaydederken IBAN'ı bildirimin içinden aktarır; IBAN yine müşterinin kendi beyanından gelir (§10.4.10; K-510). Bildirim IBAN taşımıyorsa firma IBAN'ı başka bir yoldan isteyip girmez; IBAN müşteriden sipariş sayfasından istenir (§10.4.10; K-581). Kart iadesi başarısız olup firma müşteriye havale yolunu açtığında IBAN'ı müşteri kendi seçimiyle sipariş sayfasından girer (§7.2.8; K-521, K-569). Müşteri IBAN'ını geri ödeme işlenene kadar sipariş sayfasından düzeltebilir; bankada geri dönen havalede IBAN silinir ve müşteriden aynı yolla yeniden istenir (§7.4.5; K-575).

**8.4.3 Serbest metin alanları kayda geçen bir risk taşır.** Yöneticinin iç notuna kişisel veri yazılabilir ve ürün bunu denetleyemez; not siparişle birlikte imha edilir. Ödeme adımında müşteri notu alanı aynı sebeple de yoktur (§3.24.7, §10.4; K-371, K-389).

**8.4.4 Çerezler yalnız zorunludur ve — ödeme sağlayıcısının çerçevesi dışında — birinci taraftır; onay bandı yoktur.** Misafir sepetini tarayıcıya bağlayan çerez de zorunlu çerezdir; çerçevenin zorunlu çerezleri onay gerektirmez, çerez politikasında anılır (§12.2.5; K-518). Bir izleme aracı eklenirse çerez envanteri değişir ve bant zorunlu hâle gelir — bu koşul MVP'de gerçekleşmedi (§12; K-348, K-349, K-351, K-352, K-397).

**8.4.5 Saklama süreleri §4'te, imhanın biçimi ve kişisel veri yükümlülükleri §12'dedir** (K-353…K-357).

### 8.5 Kayıtlar ve veri ihlali

**8.5.1 Ürün ayrı bir ihlal kaydı tutmaz.** İhlalin kapsamı üç kayıttan okunur: işlem izi (hangi yönetici neye dokundu), giriş kaydı (§3.13.20 — başarılı ve başarısız girişlerin zamanı, IP'si, hesabı ve sonucu; altı ay saklanır, Z-40) ve sistem kayıtları (`05`). Limit sayaçları kaynak sayılmaz — pencereleri en çok bir gündür ve bildirim süresi işlerken çoktan sıfırlanmış olurlar (K-515). Panelde ihlal bildirimi ekranı, formu ya da durum makinesi yoktur; bildirim yükümlülüğü firmanındır (§12; K-380, K-381).

**8.5.2 İşlem izi değiştirilemez ve silinemez;** kaldırılan yöneticinin satırları adıyla kalır (§10.3; K-310, K-314, K-355).

*Kaynak: K-328…K-334 (deneme limitleri) · K-304 (form koruması) · K-380 (ihlal kayıtları) · K-12 · K-13 · K-53 · K-98 · K-99 · K-101 · K-102 · K-104 · K-105 · K-106 · K-107 · K-109 · K-110 · K-114 · K-120 · K-121 · K-122 · K-128 · K-143 · K-150 · K-154 · K-184 · K-185 · K-187 · K-214 · K-227 · K-243 · K-254 · K-263 · K-301 · K-308 · K-310 · K-313 · K-314 · K-324 · K-345 · K-348 · K-349 · K-351 · K-352 · K-353…K-357 · K-371 · K-381 · K-384 · K-388 · K-389 · K-397 · K-399 · K-460 · K-497 · K-498 · K-528 · K-555 · K-510 · K-515 · K-518 · K-521 · K-524 · K-562 · K-569 · K-585.*

## 9. Bildirimler

> **Ne yazılır:** Hangi olayda kime bildirim gider. Kanal detayı `08`'de; burada **tetikleyici ve alıcı**.

### 9.1 Kanal ve ilkeler

**9.1.1 Tek kanal e-postadır.** SMS, anlık bildirim ve uygulama içi bildirim merkezi yoktur; bütün bildirimler e-postayla gider (K-315).

**9.1.2 Hiçbir sipariş ya da talep akışı bir e-postanın ulaşmasına bağlı değildir.** Sipariş sayfası her bilgiyi taşır; e-posta ulaşmadığında müşteri sipariş sayfasına girer (§6.1.1; K-234, K-187). Hesap ve yönetim bağlantıları bu kuralın dışındadır (§9.4).

**9.1.3 İşlem bildirimleri ticari elektronik ileti değildir ve kapatılamaz.** Bu bölümdeki bildirimler mevcut bir sözleşme ilişkisinin yürütülmesine ilişkindir: ayrı onay istemez ve İYS'ye kaydedilmez. Ne müşteri ne firma onları kapatabilir; panelde ya da hesap ayarlarında bildirim tercihi ekranı yoktur (K-326, K-377).

**9.1.4 Gönderen ve yanıt.** Gönderen adı firmanın marka adıdır — marka adı girilmemişse alan adı; platform adı hiçbir yerde geçmez. Gönderen adresi kurulumdan gelir ve panelden değiştirilemez. E-postalar yanıtlanabilir: "yanıtlamayın" adresi kullanılmaz ve müşterinin yanıtı firma kimliğindeki iletişim e-postasına düşer; yanıt sisteme girmez ve iletişim talebi açmaz (K-376, K-379, K-260, K-264, K-08). E-postalar firmanın logosunu — logo yoksa marka adını — ve marka rengini taşır (K-264, K-259).

**9.1.5 Çalışan bir e-posta gönderim yapılandırması her kurulumun dış ön koşuludur;** yapılandırma olmadan ürün canlıya alınamaz (`10 §4.1` ÖK-3; K-378).

**9.1.6 Gönderilemeyen e-posta üç kez, artan aralıkla yeniden denenir.** Üçü de başarısız olursa ilgili siparişin ya da talebin panel satırına "e-posta ulaşmadı" işareti düşer; işaret işlem izine yazılmaz ve akış etkilenmez. İşaretli satırda firma e-postayı panelden yeniden gönderebilir — en az B-1 için; gönderilen metin siparişe donmuş sürümdür. Art arda başarısız gönderimde panelin ana sayfasında kanal düzeyinde uyarı çıkar (§6.1.1; K-522). Aralıklar §11'de, gönderim kaydının saklama süresi §4'tedir: siparişe bağlı müşteri bildirimlerinin (B-1…B-15) kaydı üç yıl (Z-43), diğerlerininki Z-34'ün süresi kadar saklanır (K-321, K-353, K-548).

### 9.2 Müşteriye giden bildirimler

| # | Olay | E-posta neyi taşır | Kaynak |
|---|---|---|---|
| B-1 | Sipariş alındı — firma misafir siparişinin e-postasını düzelttiğinde yeni adrese yeniden gider (§10.4.11) | Sipariş numarası, sipariş sayfasının bağlantısı ve siparişe donan sürümleriyle Ön Bilgilendirme Formu ile Mesafeli Satış Sözleşmesi — gövdede tam metin; düzeltmede bağlantı yeni erişim anahtarını taşır | K-316, K-317, K-187, K-585 |
| B-2 | Havale/EFT seçildi | Firmanın IBAN'ı ve sipariş numarası | K-159, K-317 |
| B-3 | Havale ödeme hatırlatması | Bir kez, havale ödeme süresinin son iş gününün başında | K-167, K-320, K-529 |
| B-4 | Ödeme onaylandı | Siparişte dijital kalem varsa indirmenin hazır olduğu, aynı e-postada | K-143, K-174, K-317 |
| B-5 | Kargoya verildi | Kargo şirketi, takip numarası ve listedeki şirketlerde "Takip et" bağlantısı; "kendi aracımızla teslim"de bu beyan | K-141, K-142, K-317 |
| B-6 | Teslim edilemedi | Gönderinin firmaya geri döndüğü | K-176, K-317 |
| B-7 | İptal edildi | İptalin sebebi — firma iptalinde seçilen sebep; müşterinin kendi iptalinde de gider | K-180, K-196, K-199, K-317, K-319 |
| B-8 | Geri ödeme işlendi | Geri ödemenin işlendiği | K-291, K-317 |
| B-9 | Cayma beyanı, gecikme feshi ya da ayıp talebi alındı — firmanın başka kanaldan gelen cayma bildirimini panelden kaydetmesi dahil (§10.4.10) | Beyanın, feshin ya da talebin tarih damgası — müşterinin kendi işleminin kayıt kopyası | K-207, K-227, K-317, K-319, K-499, K-510 |
| B-10 | Teslimat ya da fatura adresi düzeltildi | Düzeltilen adres | K-369, K-375 |
| B-11 | Kalem çıkarıldı ya da adedi azaltıldı | Değişen kalem | K-369, K-374, K-375 |
| B-12 | Kargo şirketi ya da takip numarası düzeltildi | Düzeltilmiş takip bilgisi | K-369, K-375 |
| B-13 | Yanlış durum geçişi düzeltildi | Siparişin düzeltilmiş durumu — havale "ödendi" işaretinin düzeltilmesinde ödemenin alınmadığı, havale bilgisi ve yeni son ödeme günü | K-372, K-375, K-570 |
| B-14 | Havale hattında IBAN silindikten sonra iade edilen mal firmaya ulaştı — firma teslim alma adımını işledi; ya da kart iadesi sağlayıcıda başarısız oldu ve firma müşteriye havale yolunu açtı (§7.2.8); ya da geri ödeme havalesi bankada gerçekleşmedi ve firma bunu panelden bildirdi (§7.4.5); ya da firma başka bir kanaldan gelen, IBAN taşımayan cayma bildirimini havale hattında kaydetti (§10.4.10) | Geri ödeme için IBAN'ın sipariş sayfasından girilmesi gerektiği — iade malında ürünün ulaştığı da, kart hattında kart iadesinin gerçekleşmediği ve IBAN girmenin müşterinin seçimi olduğu da, dönen havalede havalenin girilen IBAN'a gerçekleşmediği de, başka kanaldan bildirilen caymada bildirimin IBAN taşımadığı da; sipariş sayfasının bağlantısı | K-498, K-497, K-317, K-521, K-569, K-575, K-581 |
| B-15 | Firma misafir siparişinin e-postasını düzeltti (§10.4.11) — **eski adrese** gider | Siparişin e-posta adresinin değiştirildiği, sipariş numarası ve değişikliğin tarihi; değişikliği kendisi istemediyse firmaya ulaşması gerektiği. Yeni adresi ve sipariş sayfasının bağlantısını taşımaz | K-585, K-375 |

**Bildirim üretmeyen olaylar:** teslim işareti — tarihi firma çoğu zaman geriye dönük girer ve e-posta teslimden günler sonra ulaşırdı (K-288, K-317) · "Hazırlanıyor"a geçiş — ödeme onayı e-postası zaten gitmiştir (K-317) · teslim tarihinin düzeltilmesi ve iç not (K-375, K-371) · iade malının teslim alınması — IBAN'ın yeniden istendiği hâl (B-14) dışında (K-215, K-498) · malı dönmemiş cayma kaleminin "mal dönmedi" gerekçesiyle kapatılması — kapatma müşterinin hakkını değiştirmez (K-496) · havale hattında IBAN'ın silinmesi — kapatmayla ya da gönderme süresinin bitiminden otuz gün sonra kendiliğinden (K-497) · ayıp talebinin "Çözüldü" işareti ve firmanın talebi yeniden açması — çözümü firma müşteriye zaten kendi cevabıyla yazmıştır (K-540).

### 9.3 Firmaya giden bildirimler

| # | Olay | Kaynak |
|---|---|---|
| F-1 | Yeni sipariş | K-318 |
| F-2 | Yeni iletişim talebi | K-305, K-318 |
| F-3 | Yeni cayma beyanı, gecikme feshi ya da ayıp talebi — müşterinin çözülmüş ayıp talebini yeniden açması dahil; yeniden açmada yeni talep açılmadığı için firma bunu yeni talep gibi öğrenir | K-207, K-227, K-297, K-318, K-499, K-540 |
| F-4 | Sistemin kendiliğinden yaptığı karta geri ödeme — iptal edilmiş siparişe gelen ödemenin geri ödenmesi ve iptal edilen ödenmiş kart siparişinin geri ödemesi | K-233, K-291, K-318 |

**9.3.1 Firmaya giden bildirimler firma kimliğindeki iletişim e-postasına gider.** Yönetici hesaplarının kendi adreslerine işlem bildirimi gönderilmez ve panelde ayrı bir "bildirim adresi" alanı yoktur (K-322, K-08).

**9.3.2 Firmanın kendi yaptığı işlem kendisine bildirilmez;** kargoya verme, havale onayı, hizmet tamamlama ve firma iptali panelde görünür (K-318). Bekleyen işler e-postayla değil, panelin ana sayfasındaki sayaçlarla gösterilir (§10.6; K-405).

### 9.4 Hesap ve yönetim e-postaları

Bildirim matrisinin dışında, akışın parçası olan bağlantılı e-postalar: **e-posta doğrulama bağlantısı** (K-101, K-102) · **şifre sıfırlama bağlantısı** (K-105) · **yeni e-posta adresinin doğrulanması** (K-110) · **yönetici daveti** — davetlinin kendi adresine gider, firmanın iletişim e-postasına değil (K-308, K-322). Gönderen kimliği (§9.1.4) bunlara da uygulanır. Yeniden deneme kuralının üç denemesi de uygulanır (§9.1.6), ama "e-posta ulaşmadı" işareti düşmez — bu e-postaların bağlanacağı bir sipariş ya da talep satırı yoktur: kullanıcı bağlantıyı ekrandan yeniden ister. Yönetici davetinde işaret panelin yönetici listesindeki davet satırına düşer (K-321, K-376, K-540).

### 9.5 Olmayan bildirimler

- **Pazarlama iletisi yoktur:** bülten, kampanya duyurusu ve benzeri e-posta gönderilmez; ürün İYS'ye bağlanmaz ve pazarlama onayı toplamaz (K-323, K-324).
- **Terk edilmiş sepet hatırlatması ve "stokta haber ver" bildirimi yoktur** — ikisi de satın almaya teşvik eden ticari iletidir (K-325, K-327).
- **Yasal metin değiştiğinde bildirim gönderilmez;** güncel metin her zaman altbilgidedir (K-363).
- **Veri ihlalinde ürün toplu bildirim göndermez;** firma etkilenen kişilere kendi e-posta aracıyla ulaşır (§12; K-382).
- **Duyuru şeridi gönderilmez;** sitede durur, kimseye iletilmez (K-326, K-269).
- **Müşterinin kendi iptali firmaya e-postayla bildirilmez:** havale hattında doğan geri ödeme "iade ve geri ödeme bekleyen" sayacına düşer ve iptal edilmiş kalem kargoya verme ekranında görünür; kart hattında sistemin kendiliğinden yaptığı iade F-4 ile zaten bildirilir. Bekleyen işler e-postayla değil sayaçla gösterilir (§10.6.1; K-540, K-405).

*Kaynak: K-315…K-322 (bildirim matrisi) · K-323…K-327 (ticari ileti rejimi) · K-375 (müdahale bildirimleri) · K-376…K-379 (e-posta kimliği ve kurulumu) · K-08 · K-101 · K-102 · K-105 · K-110 · K-141 · K-142 · K-143 · K-159 · K-167 · K-174 · K-176 · K-180 · K-187 · K-196 · K-199 · K-207 · K-215 · K-227 · K-233 · K-234 · K-260 · K-264 · K-269 · K-288 · K-291 · K-305 · K-308 · K-353 · K-363 · K-369 · K-371 · K-372 · K-374 · K-382 · K-405 · K-496 · K-497 · K-498 · K-499 · K-529 · K-297 · K-540 · K-259 · K-510 · K-521 · K-522 · K-548 · K-569 · K-570 · K-585.*

## 10. Yönetim (admin) kuralları

> **Ne yazılır:** Yöneticinin yapabildikleri ve yapamadıkları. **"Sonra ekleriz" yaklaşımı riskli** — yönetim akışları kullanıcı akışları kadar karmaşıktır, aynı derinlikte ele alınır.

Yönetim tarafı tek roldür ve çoklu kullanıcıya açıktır (§1.3; K-06). Bu bölüm panelin kapsamını, yönetici hesaplarını, işlem izini, sipariş müdahalelerini, manuel adımları, satış özetini, toplu veri işlemlerini ve ilk kurulum kontrol listesini toplar. Katalog, içerik ve sipariş işlemlerinin kuralları kendi bölümlerindedir ve aşağıdaki tabloda işaretlenir.

### 10.1 Panelin kapsamı

**10.1.1 Panel tek firmayı yönetir; firma seçimi yoktur.** Firma ekleme, firma seçme ve firmalar arası geçiş kavramı yoktur; ikinci firma ikinci kurulumdur. Çok kiracılı kullanım — tek kurulumda çok firma — mevcut ürün tanımında yoktur ve MVP'de onun için hazırlık yapılmaz; post-MVP aday listesindedir (`10 §5`) (K-01, K-02, K-09, K-10). Uygulamada platform operatörü ve operatör paneli yoktur (K-06). Ürünün kalıcı sınırları `01 §7`'dedir (K-419).

**10.1.2 Yöneticinin yapabildikleri ve yapamadıkları:**

| Alan | Yönetici ne yapar | Yapamadıkları | Kural |
|---|---|---|---|
| Firma kimliği ve satış | Kimlik alanlarını, firma tipini, isteğe bağlı meslek odasını ve ETBİS doğrulama bilgisini düzenler; satışı geçici olarak kapatır | Kimlik kaydını silemez ve zorunlu alanları boşaltamaz; bakım modu yoktur | §3.1 (K-08, K-09, K-14, K-235, K-547, K-574) |
| Ödeme yöntemleri | Havale/EFT'yi açar ve kapatır, IBAN girer | Kart yöntemini panelden açıp kapatamaz — sağlayıcı anahtarları kurulumdan gelir | §3.21 (K-161, K-162) |
| Kargo, teslimat ve ödeme süreleri | Kargo ücreti, ücretsiz kargo eşiği, asgari sipariş tutarı, teslimat illeri, kargoya verme süresi, havale ödeme süresi ve iade adresini girer | Çit dışındaki süreyi giremez; kargonun KDV oranını girmez — oran fiziksel kalemlerin oranını izler (§3.19.2) | §3.1.7, §3.17–§3.20, §11 (K-132, K-133, K-135, K-139, K-155, K-166, K-293, K-339, K-500) |
| Katalog | Ürün, varyant, stok, fiyat, KDV oranı, indirim, kupon, kategori, görsel ve dijital dosya yönetir; hizmet ürününe ifa süresi girer (§3.2.2; K-503); üretim yerini ve — ölçüyle satılan üründe — ölçü birimi ile net miktarı girer (§3.8.5; K-511); ürünü kalıcı olarak siler; iade malını stoğa ekler | Ürünleri elle sıralayamaz; toplu içe aktarma ve toplu güncelleme yoktur | §3.3–§3.12, §7.4.7, §10.7 (K-49, K-85, K-215, K-425) |
| Kurumsal içerik ve site | İçerik kayıtlarını, ana sayfa düzenini, menü adlarını, duyuruyu ve sosyal medya bağlantılarını yönetir | Sayfa kurucu, menü düzenleyici ve siteye kod ekleme yoktur; Hakkımızda silinmez | §3.27, §3.28 (K-27, K-237, K-238, K-248, K-249, K-263, K-277) |
| Marka | Logo, marka adı, site simgesi ve marka rengini ayarlar; "Shopfolio ile kuruldu" platform imzasını kapatır — imza varsayılan olarak açıktır | Yazı tipini, düzeni ve marka rengi dışındaki renkleri değiştiremez | §3.29, §3.29.6 (K-20, K-258, K-259, K-260, K-265) |
| Yasal metinler | Aydınlatma metnini ve çerez politikasını düzenler ve yayına alır | Ön Bilgilendirme Formu ile Mesafeli Satış Sözleşmesi'ni elle düzenlemez — ayarlardan üretilir | §3.33 (K-343, K-350, K-361) |
| Siparişler | Havale onayı, kargoya verme, teslim işareti, "teslim edilemedi" işareti ve yeniden gönderim, hizmet tamamlama — ödeme onaylandıktan sonra, iptal, iade teslim alma ve malın ulaştığı tarih, geri ödeme — gecikme feshinin geri ödemesi ve tutar bazlı kısmi geri ödeme dahil (§5.5; K-539); gecikme feshi satırında ve taahhüt edilen süre aşıldıktan sonraki iptalde kanuni faiz uyarısını görür (§7.2.3); sağlayıcıda başarısız olan kart iadesini yeniden dener ya da müşteriye havale yolunu açar (§7.2.8; K-521, K-569); bankada gerçekleşmeyen geri ödeme havalesini "havale gerçekleşmedi" diye bildirir (§7.4.5; K-575); "e-posta ulaşmadı" işaretli e-postayı yeniden gönderir (§6.1.1; K-522); müdahaleler — malı dönmemiş cayma kaleminin kapatılması, başka kanaldan gelen cayma bildiriminin kaydı ve misafir siparişinin e-postasının müşterinin talebi ve kimlik teyidiyle düzeltilmesi dahil (§10.4; K-510, K-585) | Müşteri adına sipariş oluşturamaz; tutarı ve fiyatı değiştiremez; kalem ekleyemez; teslim işaretini almış kalemi çıkaramaz ve ödenmemiş siparişte kalem düzeyinde iptal ya da çıkarma yapamaz (K-537, K-505); cayma beyanlı kalemi iptal edemez (K-503); iade malının ulaşma tarihini ileri tarihli giremez; malı dönmemiş cayma kalemini müşterinin gönderme süresi geçmeden kapatamaz; müşterinin geri ödeme IBAN'ını panelden giremez — IBAN'ı müşteri sipariş sayfasından girer; tek istisna başka kanaldan gelen cayma bildiriminin kaydında bildirimdeki IBAN'ın aktarılmasıdır (K-510); cayma bildirimini ileri tarihli kaydedemez; üyenin giriş yaparak verdiği siparişin e-postasını düzeltemez | §3.17–§3.22, §7, §10.4 (K-176, K-370, K-374, K-491, K-496, K-498, K-499, K-503, K-505, K-510, K-521, K-522, K-535, K-536, K-537, K-539, K-585) |
| Talepler | İletişim taleplerini listeler, kapatır ve yeniden açar; ayıp talebini çözüldü işaretler ve yeniden açar | Panelden yanıt yazmaz — cevap e-postayla verilir | §3.32, §5.10, §7.5 (K-228, K-297, K-303, K-305, K-540) |
| Üye hesapları | Üye kaydı görünümünde e-postayla arar; hesabın varlığını, adres defterini, giriş yollarını (şifre, Google) ve bağlı siparişleri görür; ilgili kişinin talebi üzerine hesabı siler — silme §3.15.1'in rejimiyle | Hesabı düzenleyemez; görünüm salt okunurdur | §3.15.5 (K-512) |
| Yönetici hesapları | Davet eder, başka bir yöneticiyi kaldırır | Son yöneticiyi ve kendi hesabını kaldıramaz | §10.2 (K-308, K-309) |
| İz, özet ve dışa aktarma | İşlem izini ve satış özetini görür; siparişleri, üye listesini ve iletişim taleplerini dışa aktarır | İzi düzeltemez ve silemez; izi ve özeti dışa aktaramaz | §10.3, §10.6, §10.7 (K-310, K-311, K-398, K-426, K-514) |

**10.1.3 Parametreler üç katmandadır.** **Firma ayarı** panelden değiştirilir; **kurulum ayarı** — alan adı, e-posta gönderim kimliği, ödeme sağlayıcı anahtarları, Google uygulamasının kimlik bilgileri — kurulumdan gelir ve panelden değişmez; **ürün sabiti** değiştirilemez. **Atama ölçütü:** yanlış değerin zararı yalnız firmaya dokunuyorsa ve karar bir işletme kararıysa firma ayarıdır; zarar güvenliğe, yasal uyuma ya da müşteriye dokunuyorsa ürün sabitidir. Yeni bir parametrenin katmanı bu ölçütten okunur (K-412, K-08, K-478). Firma ayarları kurulumda dolu gelir ve firma hiçbir sayıyı doldurmadan sistem uçtan uca çalışır; tam liste §11'dedir (K-413).

**10.1.4 Panelde hiçbir işlev mobilde kapatılmaz;** panel masaüstü öncelikli tasarlanır (§3.34.2; K-392). Aynı kaydı iki yöneticinin aynı anda düzenlemesinde sessiz üzerine yazma yoktur (§3.31.1; K-270).

**10.1.5 Firmaya giden bildirimler** ve gittikleri adres §9.3'tedir (K-318, K-322).

### 10.2 Yönetici hesapları

**10.2.1 İlk yönetici kurulumda doğar, sonrakiler davetle açılır.** Mevcut bir yönetici panelden bir e-posta adresi girerek davet gönderir; davet bağlantısı tek kullanımlık ve sürelidir (ömrü §11'de). Davetli bağlantıdan girip **şifresini kendisi kurar** ve hesap o anda doğar. Kendi kendine yönetici kaydı yolu yoktur. Davet e-postası davetlinin kendi adresine gider (K-308, K-322).

**10.2.2 Bir yönetici başka bir yöneticiyi kaldırabilir; son yönetici kaldırılamaz ve hiç kimse kendi hesabını kaldıramaz.** Panel iki durumda da işlemi engeller ve sebebini söyler; kaldırma geri alınamaz onayı ister. Kaldırılan yöneticinin açık oturumları anında sonlanır; işlem izindeki satırları silinmez ve adıyla ve e-posta adresiyle okunur kalır. Pasifleştirme yoktur (K-309, K-314).

**10.2.3 Bütün yöneticiler aynı yetkiye sahiptir.** Yetki matrisi ve rol bazlı veri kısıtı yoktur: her yönetici bütün müşteri verisini — sipariş, adres, e-posta, telefon, IBAN, iletişim talebi ve üye kaydı görünümü (§3.15.5; K-512) — görür. Firmanın kime yönetici hesabı açtığı bu yüzden kişisel veri sorumluluğunun parçasıdır (§12; K-06, K-383).

**10.2.4 Kimlik doğrulama müşteri tarafıyla aynıdır.** Yönetici hesapları aynı doğrulama, oturum, yeniden doğrulama ve şifre kurallarına ve aynı deneme limitlerine aynı değerlerle tabidir; MVP'de sıkılaştırma ve iki adımlı doğrulama yoktur (K-122, K-313, K-333). Yönetici kendi e-posta adresini müşteriyle aynı rejimle değiştirebilir; değişiklik işlem izine yazılır ve firma kimliğindeki iletişim e-postasını değiştirmez (K-384, K-110).

**10.2.5 Yönetici hesabının sepeti ve siparişi olmaz.** Kendi mağazasından alışveriş yapacak yönetici ayrı bir müşteri hesabı açar ya da misafir alıcı olarak sipariş verir; iki hesap birbirine bağlanmaz (K-312, K-97).

### 10.3 İşlem izi

**10.3.1 İz sekiz alanda tutulur:** (1) firma kimliği ve marka ayarları (K-08, K-265) · (2) ürünün ve kurumsal içeriğin yayın durumu değişiklikleri (K-276) · (3) siparişe yönelik yönetici işlemleri — firmanın elle yaptığı sipariş geçişleri ve kayıtları: havale "ödendi" işareti, kargoya verme, teslim işareti ve teslim tarihi, "teslim edilemedi" işareti, hizmet tamamlama, firma iptali, elle işlenen geri ödeme, iade malının teslim alınması ve ulaşma tarihi; §10.4'ün dokuz müdahalesi, kart iadesinin yeniden denenmesi ya da havaleyle ödenmesi (K-521), geri ödeme havalesinin "havale gerçekleşmedi" bildirimi (K-575), "e-posta ulaşmadı" işaretli e-postanın yeniden gönderilmesi (K-522) ve havale hattında IBAN'ın müşteriden yeniden istendiği teslim alma adımı — bu satır adımı yapan yöneticiyi ve IBAN isteğinin tarihini taşır (§7.4.5). Sipariş satırları müşterinin kişisel verisini değer olarak taşımaz: adres düzeltmesinde eski ve yeni adres değil, sipariş, alan ve işlem yazılır; iç notun metni de ize girmez (§10.4.7). **Tek istisna** misafir siparişinin e-postasının düzeltilmesidir: satır eski ve yeni adresi taşır, çünkü sahtekârlık iddiasında hangi adresin erişim aldığını gösteren tek kayıt odur (§10.4.11) (K-199, K-225, K-369, K-496, K-498, K-527, K-585) · (4) yönetici hesabının açılması, kaldırılması ve e-posta adresinin değiştirilmesi (K-308, K-309, K-384) · (5) yasal metin sürümü (K-362) · (6) panelden değişen her ayar — sayısal olsun olmasın: firma ayarı katmanının parametreleri, ödeme yöntemlerinin açılıp kapanması ve IBAN, iade adresi, teslimat illeri, satışı geçici kapatma anahtarı ve platform imzası dahil — eski ve yeni değerle (K-414, K-523) · (7) dışa aktarma — sipariş, üye listesi ve iletişim talepleri (K-426, K-514) · (8) üye hesabının ilgili kişinin talebi üzerine firma tarafından silinmesi — satır hesabı değil, işlemi ve tarihini adlandırır (K-512). Her satır **kim, ne zaman, ne değişti** üçlüsünü taşır (K-310). Her işlem ize düşmez: fiyat düzenlemesi, stok girişi ve içeriğin metin düzenlemesi ize yazılmaz — aranan kayıt gürültüde kaybolurdu (K-310).

**10.3.2 İz değiştirilemez ve silinemez.** Yanlış bir işlemin düzeltilmesi izi temizlemez, yeni bir satır üretir; yanlış geçişin kendi satırı da izde kalır (K-310, K-372, K-527).

**10.3.3 Sistem olayları ize yazılmaz.** "E-posta ulaşmadı" işareti ilgili satırda, periyodik imhanın kaydı sistem kaydında durur — her imha çalışmasının tarihi, kayıt türü ve imha edilen kayıt sayısı; kişisel veri içermez ve en az üç yıl saklanır (Z-44; K-552) —, giriş denemeleri giriş kaydında durur (§3.13.20; K-515); iz yönetici işlemleri içindir (K-321, K-354).

**10.3.4 İz panelde düz bir liste olarak görünür;** tarih aralığı ve yönetici süzgeci vardır, arama, gruplama ve dışa aktarma yoktur (K-311).

**10.3.5 İz on yıl saklanır;** kaldırılan yöneticinin satırları bu süre boyunca adıyla durur (§4, Z-30; K-355, K-314).

### 10.4 Sipariş müdahaleleri

**10.4.1 Dokuz müdahale vardır:** (1) teslimat ve fatura adresinin düzeltilmesi · (2) kalem adedinin azaltılması ya da kalemin çıkarılması · (3) kargo şirketi ve takip numarasının düzeltilmesi · (4) teslim tarihinin düzeltilmesi · (5) yanlış yapılmış durum geçişinin düzeltilmesi · (6) iç not · (7) malı dönmemiş cayma kaleminin kapatılması (§10.4.9) · (8) başka bir kanaldan gelen cayma bildiriminin kaydı (§10.4.10; K-510) · (9) misafir siparişinin iletişim e-postasının düzeltilmesi (§10.4.11; K-585). Her müdahale işlem izine yazılır. **Elle sipariş oluşturma yoktur:** telefonla ya da yüz yüze sipariş almak isteyen firma müşteriyi vitrine yönlendirir — elle sipariş mesafeli satışın onay, ön bilgilendirme ve 3D Secure adımlarının hepsini atlardı (K-369, K-370, K-310, K-496).

**10.4.2 Adres yalnız "Kargoya verildi"ye kadar düzeltilir.** Sınır teslimat ve fatura adresi için aynıdır: paket yola çıktıktan sonra düzeltme gerçek dünyada bir şey değiştirmez. Ürün faturanın kesilip kesilmediğini bilmez; fatura kesildikten sonra fatura adresini düzelten firma faturayı kendi aracında düzeltir (K-374, K-217, K-450). Fiziksel kalemi olmayan siparişte "Kargoya verildi" kullanılmadığı için fatura adresi Teslim edildi'ye kadar düzeltilir (K-537). Düzeltme müşteriye bildirilir (K-375).

**10.4.3 Kalem adedi azaltılabilir ya da kalem çıkarılabilir; kalem eklenemez ve fiyat değiştirilemez.** Onaylanan tutar bağlayıcıdır: eklenen kalem yeni bir satış olurdu ve indirim yapmak isteyen firma tutar bazlı kısmi geri ödeme yolunu kullanır (§5.5; K-539). Çıkarılan kalemin parası §7.2.8'in hattından geri ödenir ve stok, hizmet kontenjanı ve kupon hakkı iptaldeki rejimle döner; kargo ücreti §7.2.9'un tetiğiyle geri ödenir (K-501). Çıkarma bir düzeltmedir — iptal gibi sebep listesi istemez ama iz bırakır ve müşteriye bildirilir (K-374, K-369, K-77, K-80, K-291, K-201, K-375). **Durum makinesindeki karşılığı (K-537):** (1) sevkiyat hattında çıkarılan kalem iptal edilmiş kalem gibi sayılır — S3, S4, S10 ve İptal edildi'ye geçiş (§5.4); (2) teslim işaretini almış kalem — teslim edilmiş dijital kalem, tamamlanmış hizmet kalemi — çıkarılamaz; para iadesi gerekiyorsa tutar bazlı kısmi geri ödeme ya da §7.4'ün hattı kullanılır; (3) kart hattında karta para gönderen çıkarma §5.9'un geri alınamaz onayını ister — firma iptalinin kalıbı (K-225); (4) çıkarma kalem kaydına düşer (§5.8); (5) **fiziksel kalem sipariş "Kargoya verildi"ye geçtikten sonra çıkarılamaz** — paket yola çıkmıştır ve çıkarma gerçek dünyada bir şey değiştirmez (§10.4.2'nin gerekçesi); sevkiyat ekseninde de "Kargoya verildi"den fiziksel kalemsiz hatta bir geçiş yoktur. Gönderilmiş malda müşterinin yolu caymadır (§7.3.7), geri dönen gönderide firmanın yolu Teslim edilemedi hattıdır (S7, S9); para iadesi gerekiyorsa tutar bazlı kısmi geri ödeme kullanılır. Dijital ve hizmet kalemi teslim işaretini alana kadar siparişin durumundan bağımsız olarak çıkarılabilir (K-577). Çıkarma iptalin rejimini izlediği için ödeme onayından önce kalem düzeyinde yapılamaz (§7.2.2; K-505).

**10.4.4 Kargo şirketi ve takip numarası düzeltilebilir;** düzeltilmiş takip bilgisi müşteriye bildirilir (K-369, K-141, K-375).

**10.4.5 Teslim tarihi düzeltilebilir;** cayma penceresi ve ayıp talebi süresi düzeltilmiş tarihten işler ve caymanın teslimden önce mi sonra mı yapıldığı bu tarihe göre ayrılır — beyan düzeltilmiş tarihle aynı gün yapılmışsa teslimden sonra sayılır (§7.4.1) —, daha önce yapılmış cayma beyanı geçerli kalır. Düzeltilmiş tarih de siparişin kargoya verildiği günden önce ve ileri tarihli olamaz (§3.20.11). Düzeltme de sipariş düzeyindedir: fiziksel kalemlerin tamamının tarihini birlikte değiştirir, tek bir kalemin tarihi ayrıca düzeltilmez (§3.20.11). Teslim tarihinin düzeltilmesi bildirim üretmez (K-369, K-288, K-340, K-375, K-491, K-520, K-554, K-177).

**10.4.6 Yanlış yapılmış durum geçişi yeni bir geçişle düzeltilir.** Düzeltme sevkiyat ekseninin beyaz listesinin dışındadır ve yalnız yöneticinin panelinden, kapalı bir listeden sebep seçilerek yapılır: **yanlış siparişte işlem yapıldı · işlem gerçekleşmeden işaretlendi · diğer** — "diğer"de açıklama zorunludur. Hem yanlış geçiş hem düzeltme izde kalır (§10.3.1) ve düzeltme müşteriye bildirilir. **İki sınır:** iptal edilmiş sipariş "ödendi" yapılamaz; dış dünyaya çıkmış sonuç — gönderilen e-posta, kargoya verilen paket, karta gönderilen para — düzeltmeyle geri alınmaz (K-372, K-373, K-466, K-183, K-225, K-375, K-527). **Gidilebilecek durumlar ve yan etkiler (K-504):** terminal durumlardan — İptal edildi, Teslim edildi — çıkış **yalnız bir önceki duruma** ve **yalnız yanlış işaretin düzeltilmesi için** yapılır: erken konmuş teslim işareti Kargoya verildi'ye, yanlış siparişte yapılmış iptal iptalden önceki duruma döner. **İptalin düzeltilmesinin koşulu (K-566):** iptal yalnız ödeme Ödendi'de kalmışsa — iptal için müşteriye henüz para gönderilmemişse: havale hattında firma geri ödemeyi işlemeden önce, kart hattında sistemin başlattığı geri ödeme sağlayıcıda gerçekleşmemişse — düzeltilir. Ödenmeden iptal edilen siparişin ödemesi Başarısız'dır ve Başarısız terminaldir (§5.5); parası geri gönderilmiş siparişi yeniden açmak firmaya bedeli alınmamış bir gönderim yükler. İkisinde de düzeltme yapılamaz ve müşteri yeni sipariş verir (K-183'ün kalıbı). Düzeltme, iptalin serbest bıraktığı stoğu ve hizmet kontenjanını yeniden ayırır; yetmiyorsa düzeltme yapılamaz ve panel hangi kalemin yetmediğini söyler. Kuponun kullanım hakkı da yeniden kullanılmış sayılır; hak bu arada dolmuşsa sipariş donmuş kupon payını yine korur. Kart hattındaki "geri ödeme gerçekleşmedi" işareti (§5.5) düzeltmeyle kalkar. Cayma penceresi ve ayıp talebinin süresi düzeltilmiş teslim tarihinden yeniden hesaplanır — teslim işareti kalkarsa yeni teslim tarihi girilene kadar işlemez (§7.3.1, §7.5.2). İptalin düzeltilmesi dışında stok, hizmet kontenjanı ve kupon hakkı kendiliğinden değişmez; ödeme, aşağıdaki havale işaretinin düzeltilmesi dışında hiçbir düzeltmede değişmez — gerekirse ilgili müdahale (§10.4) ayrıca ve görünür yapılır. **Havale "ödendi" işaretinin düzeltilmesi (K-570):** ödeme ekseninde düzeltilebilen tek geçiş, para o siparişe gelmemişken konmuş havale "ödendi" işaretidir. Düzeltme iki ekseni birlikte geri alır — ödeme Ödendi'den Bekliyor'a, sevkiyat Hazırlanıyor'dan Alındı'ya döner (fiziksel kalemsiz sipariş zaten Alındı'dadır) — ve §5.6.1'in eksen bağı bu yüzden bozulmaz. Yalnız sipariş kargoya verilmemişken, hiçbir geri ödeme yapılmamışken ve hiçbir kalem teslim işareti almamışken yapılır. Dijital kalem taşıyan siparişte ödeme onayı indirmeyi açmıştır ve dış dünyaya çıkmış bu sonuç geri alınmaz; o siparişte düzeltme yapılamaz ve sonucu firmadadır — §5.9'un geri alınamaz onayı bunu önceden söyler. Ayrılan stok, hizmet kontenjanı ve kupon hakkı tutulu kalır. Havale ödeme süresi (Z-8) düzeltme anından yeniden başlar ve düzeltme bildirimi (B-13) müşteriye ödemenin alınmadığını, havale bilgisini ve yeni son ödeme gününü söyler; süre içinde ödeme gelmezse sipariş §5.6.3'e göre kendiliğinden iptal edilir. Kargoya verme süresi yeni ödeme onayından işler. Kart hattında ödeme ekseninin düzeltmesi yoktur: başarı bildirimini sağlayıcı verir. Bunun dışında düzeltme §5.6.1'in eksen bağını bozamaz: ödeme Ödendi'den Bekliyor'a yalnız bu düzeltmeyle ve sevkiyatla birlikte döner. Her düzeltme işlem izine yazılır (K-372).

**10.4.7 İç not müşteriye hiçbir yerde görünmez.** Yönetici siparişe serbest metin bir not yazar; not sipariş sayfasına, e-postalara ve sözleşmeye girmez. Notun yazılması ve değiştirilmesi işlem izine yazılır, notun kendisi siparişin saklama süresini izler. **Kayda geçen risk:** serbest metne kişisel veri yazılabilir ve ürün bunu denetleyemez (K-371).

**10.4.8 Firma iptalinin sebep listesi §7.2.4'te,** geri alınamaz onay isteyen geçişler §5.9'dadır (K-373, K-451, K-225).

**10.4.9 Malı dönmemiş cayma kalemi "mal dönmedi" gerekçesiyle kapatılabilir.** Teslimden sonra yapılan caymada müşterinin malı gönderme süresi — beyandan on dört gün (§7.4.4) — geçmiş ve mal firmaya ulaşmamışsa firma kalemi panelden kapatır; süre geçmeden kapatma yapılamaz. Kapatma işlem izine yazılır, müşteriye bildirim gitmez ve geri ödeme yapılmaz; havale hattında müşterinin IBAN'ı kapatmayla birlikte silinir (§7.4.5; K-497); kalem "iade ve geri ödeme bekleyen" sayacından düşer (§10.6.1). Kapatma cayma beyanını geçersiz kılmaz: mal sonradan ulaşırsa firma kapalı kalemde teslim alma adımıyla ulaşma tarihini girer, kalem yeniden açılır ve geri ödemenin on dört günü o tarihten işler (§7.4.1); havale hattında geri ödeme için IBAN müşteriden yeniden istenir (K-497). Kalemi sistem değil firma kapatır — süre dolunca kalemi kapatan kendiliğinden bir işlem başlamaz (K-494); firma kapatmazsa sistem yalnız IBAN'ı, gönderme süresinin bitiminden otuz gün sonra siler (K-497). Kapatma isteğe bağlıdır, zorunlu elle adım değildir ve bütçeye girmez (§10.5.2); iptal ve iade oranı caymayı beyan anında saydığı için oranı değiştirmez (§10.6.3). **Kalan risk:** mal hiç gönderilmezse geri ödeme süresinin başlamaması Yönetmelik'in lafzından çıkan bir sonuçtur (§7.4.1); bu yorum tüketici lehine değişirse geri ödemesiz kapatmanın sonucunu firma taşır (K-496, K-491, K-494, K-495, K-07).

**10.4.10 Başka bir kanaldan gelen cayma bildirimi panelden kaydedilir.** Müşteri caymayı e-postayla, mektupla ya da örnek cayma formuyla bildirdiğinde — yazılı ya da kalıcı veri saklayıcısıyla yapılan bildirim yeterlidir (Mesafeli Sözleşmeler Yönetmeliği m.11) — firma kalemi seçer ve bildirimin firmaya ulaştığı tarihi girer: tarih geçmişe dönük olabilir, ileri tarihli olamaz. Havale hattında bildirim IBAN taşıyorsa firma onu aktarır; IBAN müşterinin bildiriminden gelir ve sipariş sayfasındakiyle aynı biçim ve sağlama denetiminden geçer (§8.4.2). **Bildirim IBAN taşımıyorsa ya da taşıdığı IBAN denetimden geçmiyorsa kayıt IBAN'sız yapılır;** firma IBAN'ı telefonla ya da e-postayla isteyip panelden girmez. Kayıtla birlikte müşterinin sipariş sayfasında o kalem için IBAN alanı açılır ve müşteriye B-14 gider (§9.2); kalem IBAN girilene kadar "IBAN bekleniyor" listesinde görünür ve geri ödemenin süresi IBAN beklenirken durmaz (§7.4.5; K-581). Kayıt bir cayma beyanıdır: beyanın tarihi girilen tarihtir, pencerenin içinde yapılıp yapılmadığı bu tarihten okunur ve geri ödeme süresi (Z-16) ile teslimden önce/sonra ayrımı (§7.4.1) bu tarihle işler. Kayıt işlem izine yazılır ve müşteriye cayma bildirimi (B-9) gider. Olağan dışı bir müdahaledir: zorunlu elle adım değildir ve bütçeye girmez (§10.5.2). **Elenen yollar:** firmanın kalem çıkarmayla para iadesi yapması caymanın kaydını, tarihini ve yasal süresini taşımaz; müşteriyi sipariş sayfasına yönlendirmek süresi içinde yapılmış yasal bir bildirimi yok sayar; IBAN'sız bildirimi IBAN gelene kadar kaydetmemek beyanın tarihini kaydırır (K-510, K-207, K-491, K-581).

**10.4.11 Misafir siparişinin iletişim e-postası müşterinin talebi üzerine düzeltilir.** E-postasını yanlış yazdığını onaydan sonra fark eden misafir alıcı sipariş sayfasına giremez: numara + e-posta sorgusu da e-postadaki bağlantı da o adrese bağlıdır (§3.22.3). Havale hattında geri ödemenin IBAN'ı yalnız sipariş sayfasından girildiği için (§7.4.5) bu kapı paraya da dokunur.
- **Kapsam:** yalnız giriş yapılmadan verilmiş sipariş. Giriş yapmış üyenin siparişinin e-postası hesabın doğrulanmış e-postasıdır ve düzeltilmez. Düzeltme siparişin kişisel verileri imha edilene kadar (Z-28, Z-41) yapılabilir.
- **Teyit:** müşteri firmaya telefonla, iletişim formuyla (§3.32) ya da e-postayla ulaşır. Firma kimliği siparişteki bilgilerle — ad, fiziksel kalemli siparişte teslimat telefonu, kalemler ve tutar — kendisi teyit eder; sipariş numarası tek başına teyit değildir (§3.22.5). Ürün teyidi denetlemez; **ispat ve teyit yükümlülüğü firmadadır** (§12.5).
- **Sonuçlar:** (1) siparişin iletişim e-postası yeni değer olur — §3.23.3'ün donmasına tek istisna; (2) yeni bir erişim anahtarı üretilir, eski anahtar geçersizleşir — eski e-postadaki sipariş ve indirme bağlantıları açılmaz, numara ile eski e-postayla sorgu da sayfayı açmaz (§3.22.5); (3) yeni adrese sipariş e-postası (B-1) siparişe donmuş sürümüyle ve yeni anahtarlı bağlantıyla gider; (4) eski adrese B-15 gider — değişikliği söyler, yeni adresi ve bağlantıyı taşımaz (§9.2); (5) sipariş e-posta eşleşmesiyle bir hesaba düşmüşse (§3.13.3) bağ kesilir; yeni adres doğrulanmış bir hesabın e-postasıysa sipariş §3.13.3'ün kuralıyla o hesaba düşer; (6) düzeltme işlem izine **eski ve yeni değerle** yazılır — §10.3.1'in kişisel veri kuralına tek istisna — ve panel siparişin satırında "e-posta düzeltildi" işaretini gösterir.
- **Ne değişmez:** müşterinin girdiği IBAN, siparişin durumları, süreler ve kalemler düzeltmeden etkilenmez. Olağan dışı bir müdahaledir: zorunlu elle adım değildir ve bütçeye girmez (§10.5.2).
- **Kalan risk:** firmayı kandıran biri siparişin e-postasını kendi adresine çevirtip sipariş sayfasına ve havale hattında IBAN alanına erişebilir — iptal, cayma, IBAN girişi ya da düzeltmesi (§7.4.5) dahil. Kandırılmanın sonucu firmadadır. B-15 gerçek sahibini uyarır; firma e-postayı yeniden düzeltebilir, ama arada yapılan işlemler geri alınmaz ve izden okunur (§6.3.5, §8.3.6). İz satırındaki iki adres izin süresi (Z-30) boyunca kalır.
- **Elenen yollar:** onay ekranında gösterilen tek kullanımlık kurtarma koduyla müşterinin e-postayı kendisi düzeltmesi ödeme adımına yeni bir öğe ekler ve kodu saklamayan müşteriyi yine kapıda bırakır; kurtarma yolu açmayıp firmanın IBAN'ı telefonla alması IBAN'ın tek giriş yolunu (§8.4.2) deler ve müşteri sipariş sayfasını yine göremez; müşterinin numara ve telefonla sayfayı kendisi açması doğrulanmamış bir veriye yeni bir kapı açar (K-585, K-193, K-187, K-99, K-498, K-527, K-562, K-575, K-581).

### 10.5 Manuel adımlar ve bütçe

**10.5.1 Sipariş başına sistem içi zorunlu elle adımlar:**

| Sipariş | Adım | Adımlar |
|---|---|---|
| Kart ile ödenen fiziksel sipariş | **2** | Kargoya verme ve takip bilgisi (K-141) · teslim işareti ve tarihi (K-288) |
| Havale ile ödenen fiziksel sipariş | **3** | Kart hattının iki adımı + "ödendi" işareti (K-159) |
| Hizmet siparişi | **1** | "Tamamlandı" işareti (K-175) |
| Dijital sipariş | **0** | Sistem teslim eder (K-174) |

Havale ile ödenen her siparişe "ödendi" işareti bir adım ekler (K-159); işaret sipariş düzeyinde bir kez yapılır ve karışık siparişte bir kez sayılır. Karışık siparişte her hat kendi adımını taşır ve adımlar toplanır: havale ile ödenen fiziksel + hizmet siparişi dört adımdır — "ödendi", kargoya verme, teslim işareti, hizmetin tamamlanması. Bütçe (§10.5.4) hat başına okunur (K-401, K-454).

**10.5.2 Koşullu adımlar — her biri bir adım:** firma kaynaklı iptal ve sebep seçimi (K-199, K-373) · geri dönen gönderiyi "teslim edilemedi" işaretleme ve yeniden gönderme (K-176, K-535) · iade malını teslim alıp ulaşma tarihini girme ve stoğa ekleme (K-215, K-491 — tarih alanı yeni bir adım değildir) · havale hattında geri ödemeyi işleme (K-291) · ayıp talebini çözüldü işaretleme (K-228) · dijital kalemin indirme hakkını yenileme (K-147, K-456) (K-401). Malı dönmemiş cayma kaleminin "mal dönmedi" gerekçesiyle kapatılması bir müdahaledir (§10.4.9): isteğe bağlıdır, zorunlu adım değildir ve bütçeyi değiştirmez (K-496). Aynısı olağan dışı işlere de uygulanır — başka kanaldan gelen cayma bildiriminin kaydı (§10.4.10; K-510), sağlayıcıda başarısız olan kart iadesinin yeniden denenmesi (K-521), bankada gerçekleşmeyen geri ödeme havalesinin bildirilmesi (K-575), ulaşmayan e-postanın yeniden gönderilmesi (K-522), misafir siparişinin e-postasının düzeltilmesi (§10.4.11; K-585) ve üye hesabının talep üzerine silinmesi (K-512): zorunlu adım değildirler ve bütçeye girmezler.

**10.5.3 Fatura sipariş başına bir sistem dışı adımdır.** Firma faturayı kendi aracıyla keser; ürün bunu göremez, izleyemez ve hatırlatamaz. Adım bütçenin konusu değildir ama envanterde açıkça yazılıdır (K-402, K-217).

**10.5.4 Bütçe hat başınadır: tek tipli bir siparişin hattında sistem içi zorunlu elle adım üçü geçmez.** Bütçe bir ürün kuralıdır; süreç tarafındaki karşılığı aşama kapanışının terfi adayıdır (K-438). Bütçe tek tipli bir siparişin hattına uygulanır: en ağır hat — havale ile ödenen fiziksel sipariş — bugün üç adımdır. Kural ileriye dönüktür: `03`–`12` yazılırken ve implementation sırasında doğan her yeni elle adım önerisi bu bütçeyle birlikte okunur ve sayıyı geçirecek her öneri karar kaydına geri döner (K-404, K-454).

**10.5.5 Hacim varsayımı:** ortalama günde 10, tepe günde 50 sipariş; en ağır hatta tepe gün 150 panel işlemi eder, bu tek kişinin yaklaşık bir saatlik işidir. Sayı bir hedef değil, tasarım varsayımıdır (§11.3; K-403, K-417).

### 10.6 Bekleyen işler ve satış özeti

**10.6.1 Panel ana sayfası bekleyen işleri sayar.** Sayaçlı blok altı sayaç taşır: **ödeme onayı bekleyen** (havale) · **kargoya verilecek** — Hazırlanıyor'da olup iptal edilmemiş fiziksel kalem taşıyan siparişler ve geri dönüp yeniden gönderilecek ya da iptal edilecek Teslim edilemedi siparişleri; yedinci sayaç açılmaz (K-535, K-467) · **teslim işareti bekleyen** · **tamamlanmayı bekleyen hizmet** · **açık talep** — iletişim ve ayıp talepleri · **iade ve geri ödeme bekleyen** — teslim alınacak iade malı, havale hattında işlenecek geri ödeme ve kart hattında sağlayıcıda başarısız olmuş geri ödeme ("geri ödeme gerçekleşmedi" işaretli; §7.2.8, K-521). Teslimden sonraki caymada geri ödemenin on dört günü, iade malının teslim alma adımında girilen ulaşma tarihinden — mal beyandan önce ulaştıysa beyandan — işler (§7.4.1; K-491, K-554). Firmanın "mal dönmedi" gerekçesiyle kapattığı kalem sayaçtan düşer; mal ulaşıp kalem yeniden açılırsa geri gelir (§10.4.9; K-496). Havale hattında IBAN'ı müşteriden istenen kalem — iade malı IBAN silindikten sonra ulaşmış ya da kart iadesi başarısız olup firma müşteriye havale yolunu açmış ya da geri ödeme havalesi bankada gerçekleşmemiş ya da başka kanaldan gelen cayma bildirimi IBAN taşımamış — panelde "IBAN bekleniyor" listesinde, geri ödemenin kalan süresiyle görünür (§7.4.5; K-498, K-521, K-581); ama firmanın yapabileceği bir iş olmadığı için bekleyen işler sayacında sayılmaz — iş müşteridedir ve sayaç süresiz şişerdi. Müşteri IBAN'ı girdiğinde kalem işlenecek geri ödeme olarak sayaca girer (K-525, K-405). Ana sayfada sayaçlardan ayrı olarak, art arda başarısız e-posta gönderiminde kanal düzeyinde uyarı çıkar (§6.1.1; K-522). Her sayaç kendi süzülmüş listesine götürür. Yeni bir ekran ve yeni veri yoktur; sayaçlar mevcut durumlardan hesaplanır, saklanmaz (K-405, K-467). Bekleyen işler e-postayla hatırlatılmaz (K-405).

**10.6.2 Panelde bir satış özeti vardır ve analitik değildir.** Ürünün kendi kayıtlarından — sipariş, ödeme ve iletişim talebi kaydı — türeyen bir rapordur; ziyaretçi verisi kullanmaz (K-483); seçilen dönem için şunları gösterir: sipariş sayısı ve cirosu · en çok satan ürünler · ödeme yöntemi dağılımı · iptal ve iade sayısı · ödeme tamamlama oranı · iletişim talebi sayısı · kargoya verme sözüne uyum oranı · iptal ve iade oranı (§10.6.3). Özet çerez kullanmaz, ziyaretçi izlemez ve yeni kişisel veri biriktirmez; tamamı mevcut kayıttan hesaplanır, saklanmaz ve dışa aktarılmaz. Dönem seçimi ve ekranın düzeni `04`'ün işidir (K-398, K-441, K-400, K-534). Özetin dört ölçüsü `01 §6`'nın mağaza düzeyi ölçüleridir (M-1…M-4; K-416).

**10.6.3 Ölçülerin tanımı:**

- **Ödeme tamamlama oranı:** dönem içinde sipariş onayına gelen sepetlerden, ödemesi tamamlanmış bir siparişle sonuçlananların oranı. Aynı sepetten art arda verilen siparişler — başarısız ödemenin yeni siparişle tekrarı ve aynı sepetten verilen yeni siparişin iptal ettirdiği önceki sipariş — tek deneme sayılır; ödemesi alınmamış sipariş Başarısız kaydı taşır (K-181, K-182, K-296, K-416, K-457). Sepet, ilk siparişinin onaylandığı döneme yazılır; oran okunduğu anda ödeme süresi dolmamış denemesi olan sepet paydaya girmez, süre dolunca girer (K-534).
- **İletişim talebi sayısı:** dönem içinde açılan iletişim taleplerinden "KVKK talebi" ve "Sipariş hakkında" tipi dışındakiler — Genel soru, Ürün hakkında ve Diğer. Ölçü folio tarafının çıktısını sayar; satın almadan sonraki başvurular ve kişisel veri başvuruları ona girmez (K-302, K-416, K-457).
- **Kargoya verme sözüne uyum oranı:** kargoya verme süresi dönem içinde dolan ve iptal edilmemiş fiziksel kalem taşıyan siparişlerden (K-535), ödemenin onaylandığı andan kargoya verilene geçen sürenin siparişin donmuş kargoya verme sözünü iş günüyle aşmadığı siparişlerin oranı. Süre dolduğunda henüz kargoya verilmemiş sipariş — sonradan kargoya verilse de, gecikme yüzünden iptal düğmesiyle ya da gecikme feshiyle (§7.2.3) feshedilse de — "uyulmadı" sayılır; süre dolmadan iptal edilen sipariş paydaya girmez (K-139, K-140, K-168, K-337, K-441, K-484, K-499).
- **İptal ve iade oranı:** dönem içinde ödemesi tamamlanmış siparişlerden en az bir kalemi iptal edilen, gecikme nedeniyle feshedilen ya da iade alanların oranı; iptal kayıtlarından, gecikme feshi kayıtlarından ve cayma beyanlarından hesaplanır. Gecikme feshi iptalden ayrı bir kayıt tipidir (§5.8, §7.2.3) ama ölçüde iptal gibi sayılır — fesih bildiriminin tarih damgasında, feshedilen kalemleriyle (K-580). Cayma, beyan edildiği anda iade sayılır — malın firmaya dönmesi ve geri ödemenin yapılması beklenmez; ölçünün adındaki "iade" bu yüzden sözlükteki İade'den (§1.2) geniştir ve malın geri gönderilmesini değil cayma beyanını sayar, malı hiç dönmeyen cayma da girer. Satış özetinin iptal ve iade sayısı gecikme feshini de sayar ve caymayı beyan anında sayar ve kalemin sonradan "mal dönmedi" gerekçesiyle kapatılması (§10.4.9) sayıyı değiştirmez. Ödemesi alınmadan kapanan (Başarısız) sipariş bu orana girmez — ödeme tamamlama oranının konusudur (K-195, K-207, K-296, K-416, K-485, K-495, K-496).
- **Sıfır payda:** bir oranın paydası seçilen dönemde sıfırsa — ör. hiç sipariş yoksa — oran "değerlendirilemez" sayılır, sıfır yüzde gösterilmez; gösterimi `04`'ün işidir (K-490).
- **Ölçüm penceresi:** `01 §6`'nın "ilk gerçek kurulumun ilk üç ayı", satışın o kurulumda ilk açıldığı gün başlar — §3.1.5'in dört koşulunun ilk kez birlikte sağlandığı gün (K-418, K-486). İptal ve iade oranı bu pencere için, pencerede ödemesi tamamlanıp teslim edilen ya da ifası tamamlanan siparişlerin hepsinde cayma penceresi — fiziksel üründe teslimden, hizmette sipariş tarihinden, fiziksel kalem içeren siparişteki hizmette teslimden başlar (K-503) — kapandığı gün okunur; geri ödeme süresinin kapanması beklenmez, çünkü cayma beyan anında sayılmıştır. O güne kadar teslim edilmemiş ya da ifası tamamlanmamış sipariş beklenmez; o ana kadarki iptal ve iadesiyle sayılır (K-203, K-488, K-495).

### 10.7 Toplu veri işlemleri

**10.7.1 Siparişler dışa aktarılır.** Firma panelden tarih aralığı seçip siparişleri CSV olarak indirir; dosya ürünün tuttuğu fatura verisini taşır — tutar, KDV oranı ve tutarı, kalem dökümü, indirim ve kupon payı, kargo ücreti ve fiziksel kalemlerin oranlarına bölünmüş KDV'si (§3.19.2; K-500), donmuş fatura adresi, siparişin donmuş firma kimliği, ödeme yöntemi ve ödeme onay tarihi, kargoya verme tarihi (hizmette tamamlama, dijitalde ödeme onayı), kargo şirketinin adı ve sitenin alan adı (§3.25.1; K-546) — ve ihlalde ulaşma için siparişin donmuş iletişim e-postası ile alıcı adını (K-514). Dışa aktarma bir yönetici işlemidir ve işlem izine yazılır; indirilen dosyanın sorumluluğu o andan itibaren firmanındır. Aynı yol, veri ihlalinde firmanın etkilenen kişilere ulaşma ihtiyacını da karşılar (K-426, K-217, K-219, K-220, K-382, K-341, K-514).

**10.7.4 Sipariş dışındaki kişilere ulaşma listesi de dışa aktarılır.** Hiç sipariş vermemiş üyeler ve iletişim formuyla başvuranlar için firma üye kaydı görünümünden (§3.15.5) ve iletişim talepleri listesinden ad ve e-postayı CSV olarak indirir. Düğme olağandır — sistem ihlali bilemez; her dışa aktarma işlem izine yazılır ve kötüye kullanımı görünür kılar. Yöneticilerin iletişim bilgisi panelin yönetici listesinden okunur (§12.2.9; K-514, K-512, K-426).

**10.7.2 Ürün içe aktarma ve toplu güncelleme yoktur.** CSV ile ürün içe aktarma ya da liste ekranından toplu fiyat ve stok güncelleme bulunmaz; firma ürünleri tek tek ekler ve günceller (K-425).

**10.7.3 Katalog ve içerik için ürün içinde dışa aktarma yoktur;** firmanın çıkış hakkı barındırma düzleminde karşılanır (§12; K-427, K-410). İşlem izi ve satış özeti dışa aktarılmaz (K-311, K-398).

### 10.8 İlk kurulum kontrol listesi

**10.8.1 Site boş kurulur;** örnek içerik, ürün ya da görsel yoktur (§3.27.22; K-271).

**10.8.2 Panel ana sayfasında bir kurulum kontrol listesi durur.** Liste satışın açılması için karşılanmamış koşulları ve zorunlu doldurulacakları adıyla sayar: seçilen firma tipinin zorunlu kimlik alanları · en az bir açık ödeme yöntemi — havale açıksa IBAN · aydınlatma metni ve çerez politikası — ikisi de yayına alınmış, aydınlatmanın barındırma konumu bölümü dahil (K-576); aydınlatma tamamlanıp yayına alınana kadar iletişim formu, hesap kaydı ve Google ile ilk giriş de kapalıdır (K-517) · iade adresi (§3.1.5). Sihirbaz ya da zorunlu adım sırası yoktur; madde tamamlandıkça işaretlenir ve hepsi tamamlanınca liste kaybolur. Kurulumun dış ön koşulları — alan adı, barındırma, e-posta gönderim yapılandırması, ödeme sağlayıcı sözleşmesi, ETBİS kaydı (kayıt yükümlülüğü olan firmada), Google uygulaması, ilk yönetici hesabı — panelin değil `10 §4.1`'in kurulum kontrol listesinin konusudur (K-271, K-469, K-365, K-465, K-413, K-476, K-477, K-308, K-444).

*Kaynak: K-401…K-405 (manuel adımlar, bütçe, bekleyen işler) · K-476, K-477, K-478 (kurulum ayarları ve dış ön koşullar) · K-308…K-314, K-383, K-384 (yönetici hesapları) · K-310, K-311, K-414 (işlem izi) · K-369…K-375 (müdahaleler) · K-398, K-441, K-457 (satış özeti) · K-412, K-413 (katmanlar) · K-425…K-427 (toplu veri) · K-01 · K-02 · K-06 · K-08 · K-09 · K-10 · K-14 · K-20 · K-27 · K-49 · K-77 · K-80 · K-85 · K-97 · K-110 · K-122 · K-132 · K-133 · K-135 · K-139 · K-140 · K-141 · K-147 · K-155 · K-159 · K-161 · K-162 · K-166 · K-168 · K-174 · K-175 · K-181 · K-182 · K-183 · K-195 · K-199 · K-201 · K-207 · K-215 · K-217 · K-219 · K-220 · K-225 · K-228 · K-235 · K-237 · K-238 · K-248 · K-249 · K-258 · K-259 · K-260 · K-263 · K-265 · K-270 · K-271 · K-276 · K-277 · K-288 · K-291 · K-293 · K-296 · K-302 · K-303 · K-305 · K-318 · K-321 · K-322 · K-333 · K-337 · K-339 · K-340 · K-341 · K-343 · K-350 · K-354 · K-355 · K-361 · K-362 · K-365 · K-382 · K-392 · K-400 · K-410 · K-416 · K-417 · K-466 · K-450 · K-451 · K-456 · K-469 · K-454 · K-467 · K-419 · K-438 · K-465 · K-491 · K-495 · K-496 · K-497 · K-498 · K-499 · K-527 · K-534 · K-297 · K-500 · K-501 · K-503 · K-504 · K-505 · K-535 · K-536 · K-537 · K-539 · K-540 · K-07 · K-176 · K-203 · K-418 · K-444 · K-483 · K-484 · K-485 · K-486 · K-488 · K-490 · K-494 · K-520 · K-554 · K-510 · K-511 · K-512 · K-514 · K-515 · K-517 · K-521 · K-522 · K-523 · K-525 · K-546 · K-547 · K-552 · K-566 · K-569 · K-570 · K-585.*

## 11. Sayısal parametreler (özet)

> **Ne yazılır:** Dokümanın her yerine dağılmış sayıların tek tablosu. Diğer dokümanlar bu tabloyu referans alır.

Sayısal parametreler koda gömülmez (K-02). Envanter bu bölümde yaşar ve her satır altı alan taşır: parametre · katman · varsayılan değer · çit · nerede kullanılır · kaynak karar. `06`, `07` ve `09` değeri buradan okur; `10 §2` yalnız özet satırını taşır (K-411). Katmanlar ve atama ölçütü §10.1.3'tedir: **firma ayarı** panelden değiştirilir, **ürün sabiti** değiştirilemez (K-412). Firma ayarları kurulumda dolu gelir; firma hiçbir sayıyı doldurmadan sistem uçtan uca çalışır (K-413). Tek istisna hizmet ürününün ifa süresidir (P-44): ürün tanımının zorunlu alanıdır ve firma hizmet ürününü yayına alırken girer (K-503). Firma ayarındaki her değişiklik eski ve yeni değerle işlem izine yazılır — panelden değişen sayı olmayan ayarlar da (§10.3.1; K-414, K-523). P-11 (duyuru metni) envanterden çıkmıştır — duyuru kurumsal içerik kaydıdır (K-502); P-2 (kargo KDV oranı) çıkmıştır — kargonun KDV'si fiziksel kalemlerin oranını izler (K-500); P-44 (hizmetin ifa süresi) girmiştir (K-503); P-45 (giriş kaydının saklama süresi — K-515) ve P-46 (aynı anda açık ödenmemiş sipariş tavanı — K-524) girmiştir. Kimlikler yeniden sıralanmaz; envanterde on firma ayarı ile otuz dört ürün sabiti, toplam kırk dört parametre vardır.

**Envantere girmeyenler:** yasal süreler — cayma penceresi, müşterinin iade malını gönderme süresi, geri ödeme süreleri, ayıp talebi süresi, yasal teslim üst sınırı, KVKK ve ihlal bildirim süreleri, referans fiyatın penceresi (K-508), ticari kayıtların, ödemesi alınmamış siparişin kişisel verisinin (Z-41), siparişe bağlı bildirim gönderim kaydının (Z-43) ve imha kaydının (Z-44) saklama süreleri — §4'te yazılıdır ve değiştirilemez; süreler §4'ün sayım kuralıyla işler ve iki süre — kargoya verme, havale — iş günüyle sayılır (K-63, K-337, K-338); işlem izinin on yıllık saklama süresi yasal değildir ama ticari kaydın süresine bağlıdır ve §4'te kalır (Z-30; K-355, K-533); ürünün kendi listeleri — kargo şirketi listesi, resmî tatil listesi, yaygın şifre listesi, konu tipleri ve sebep listeleri — ürünle gelir; tarayıcı tabanı bir kuraldır, sayı değildir; kırılma noktaları `04`'ün işidir; iade kargo bedelinin kime ait olduğu bir ayar değildir (K-411, K-142, K-336, K-393, K-463, K-210). Mağaza düzeyi ölçüler parametre değildir; tanımları §10.6.3'tedir (K-416, K-441). **Kurulum ayarları** — alan adı, e-posta gönderim kimliği, ödeme sağlayıcı anahtarları, Google uygulamasının kimlik bilgileri — sayı değildir, kurulumda girilir ve panelden değişmez (K-412, K-08, K-478).

### 11.1 Firma ayarları

| # | Parametre | Katman | Varsayılan değer | Çit | Nerede kullanılır | Kaynak |
|---|---|---|---|---|---|---|
| P-1 | Kargo ücreti — sipariş başına, KDV dahil | Firma ayarı | **0 TL** — firma girene kadar her sipariş ücretsiz kargolu | — | §3.19.1 | K-132, K-447 |
| P-2 | *Kaldırıldı* — kargo ücretinin KDV'si fiziksel kalemlerin oranını izler, firma ayarı değildir (§3.19.2); kimlik yeniden kullanılmaz | — | — | — | — | K-219, K-447, K-500 |
| P-3 | Ücretsiz kargo eşiği | Firma ayarı | **Boş** — özellik kapalı | — | §3.19.3–§3.19.5 | K-135, K-447 |
| P-4 | Asgari sipariş tutarı | Firma ayarı | **Boş** — özellik kapalı | — | §3.18 | K-155, K-447 |
| P-5 | Kargoya verme süresi — firma varsayılanı | Firma ayarı | **2 iş günü** | Üst çit **10 iş günü** | §3.20.4, §3.20.5; §4 Z-10 | K-139, K-339, K-447, K-448 |
| P-6 | Kargoya verme süresi — ürün düzeyi | Firma ayarı (ürün formunda) | **Boş** — firma varsayılanını devralır | P-5'in üst çiti | §3.20.5 | K-140, K-339, K-447, K-448 |
| P-7 | Havale ödeme süresi | Firma ayarı | **2 iş günü** | Alt çit **1**, üst çit **3 iş günü** | §3.17.4; §4 Z-8 | K-166, K-339, K-447, K-448 |
| P-8 | İndirme hakkı — sipariş kalemi başına | Firma ayarı | **5 indirme** | En az **1** | §3.12.5 | K-146, K-447 |
| P-9 | Bir siparişte en fazla adet — ürün başına | Firma ayarı (ürün formunda) | **Boş** — tek sınır stoktur | En az **1** | §3.16.9 | K-151, K-447, K-533 |
| P-10 | KDV oranı varsayılanı | Firma ayarı | **%20** — genel oran; ürün kendi oranını taşır | — | §3.8.2 | K-58, K-59, K-447 |
| P-11 | *Kaldırıldı* — duyuru metni firma ayarı değil, kurumsal içerik kaydıdır (§3.27.21); kimlik yeniden kullanılmaz | — | — | — | — | K-269, K-447, K-502 |
| P-44 | İfa süresi — hizmet ürünü | Firma ayarı (ürün formunda) | **Varsayılan yoktur** — hizmet ürününün zorunlu alanıdır; girilmeden ürün yayına alınamaz | Alt çit **1 gün** | §3.2.2, §3.7.4; §4 Z-39 | K-503 |

### 11.2 Ürün sabitleri

| # | Parametre | Katman | Değer | Çit | Nerede kullanılır | Kaynak |
|---|---|---|---|---|---|---|
| | **Hesap ve oturum** | | | | | |
| P-12 | E-posta doğrulama bağlantısının ömrü | Ürün sabiti | **24 saat** | — | §3.13.5; §4 Z-1 | K-102, K-459 |
| P-13 | Şifre sıfırlama bağlantısının ömrü | Ürün sabiti | **60 dakika** | — | §3.13.9; §4 Z-2 | K-105, K-459 |
| P-14 | Yönetici daveti bağlantısının ömrü | Ürün sabiti | **72 saat** | — | §10.2.1; §4 Z-3 | K-308, K-459 |
| P-15 | Kısa oturum — "beni hatırla" işaretsiz | Ürün sabiti | **2 saat**, son işlemden | — | §3.13.12; §4 Z-4 | K-108, K-459 |
| P-16 | Uzun oturum — "beni hatırla" işaretli | Ürün sabiti | **30 gün**, son işlemden | — | §3.13.12; §4 Z-5 | K-108, K-459 |
| P-17 | Asgari şifre uzunluğu | Ürün sabiti | **10 karakter** | — | §3.13.15 | K-120, K-459 |
| | **Ödeme, sipariş ve katalog yapısı** | | | | | |
| P-18 | Kart ödeme süresi | Ürün sabiti | **30 dakika** | — | §3.17.4; §4 Z-7 | K-128, K-166, K-462 |
| P-19 | Sipariş numarasının rastgele blok uzunluğu | Ürün sabiti | **6 karakter** | — | §3.22.1 | K-185, K-462 |
| P-20 | Kategori ağacının derinliği | Ürün sabiti | **3 seviye** | — | §3.4.1 | K-42 |
| P-21 | Ürün başına seçenek boyutu | Ürün sabiti | **2** | — | §3.3.2 | K-40 |
| | **Görsel, dosya ve metin** | | | | | |
| P-22 | Kabul edilen görsel biçimleri | Ürün sabiti | **JPEG, PNG, WebP** | — | §3.11.3, §3.27.15, §3.29.2 | K-92, K-252, K-461 |
| P-23 | Tek görsel dosyasının boyut tavanı | Ürün sabiti | **10 MB** | — | §3.11.3, §3.27.15, §3.29.2 | K-92, K-252, K-461 |
| P-24 | Ürün başına görsel adedi — ürünün ve varyantlarının görselleri birlikte | Ürün sabiti | **20** | — | §3.11.3 | K-92, K-461 |
| P-25 | Kurumsal içerik kaydı başına görsel adedi | Ürün sabiti | **20** | — | §3.27.15 | K-252, K-461 |
| P-26 | Dijital ürün dosyasının boyut tavanı | Ürün sabiti | **500 MB** | — | §3.12.3 | K-143, K-461 |
| P-27 | Ürün adının ve kurumsal içerikte ad ya da başlığın karakter tavanı | Ürün sabiti | **150 karakter** | — | §3.11.5, §3.27.15 | K-95, K-252, K-461 |
| P-28 | SSS sorusunun, kısa açıklamanın ve şubenin çalışma saatlerinin karakter tavanı | Ürün sabiti | **200 karakter** | — | §3.27.6, §3.27.7, §3.27.15 | K-242, K-252, K-461, K-533 |
| P-29 | Hakkımızda kısa tanıtımının karakter tavanı | Ürün sabiti | **400 karakter** | — | §3.27.3, §3.27.15 | K-239, K-252, K-461 |
| P-30 | Duyuru metninin karakter tavanı | Ürün sabiti | **120 karakter** | — | §3.27.21 | K-269, K-461 |
| | **Deneme limitleri** — eşik / pencere (§8.2) | | | | | |
| P-31 | L-1 Başarısız giriş denemesi | Ürün sabiti | E-posta başına **5 / 15 dakika**; IP başına **20 / 15 dakika** | — | §3.13.17, §8.2 | K-328, K-330, K-460 |
| P-32 | L-2 Şifre sıfırlama talebi | Ürün sabiti | E-posta başına **3 / 1 saat**; IP başına **10 / 1 saat** | — | §3.13.17, §8.2 | K-328, K-330, K-460 |
| P-33 | L-3 Hesap kaydı | Ürün sabiti | E-posta başına **3 / 1 saat**; IP başına **5 / 1 saat** | — | §3.13.17, §8.2 | K-328, K-330, K-460 |
| P-34 | L-4 İletişim formu gönderimi | Ürün sabiti | IP başına **5 / 1 saat** | — | §3.32.7, §8.2 | K-304, K-328, K-460 |
| P-35 | L-5 Başarısız misafir sipariş takibi sorgusu | Ürün sabiti | IP başına **10 / 15 dakika** | — | §3.22.3, §8.2 | K-328, K-330, K-460 |
| P-36 | L-6 Geçersiz kupon kodu denemesi | Ürün sabiti | Sepet başına **5 / 15 dakika** — girişte birleşen sepet iki sayacın büyüğünü taşır | — | §3.10.5, §8.2 | K-334, K-460, K-555 |
| P-37 | L-7 Kendiliğinden iptal edilen ödenmemiş sipariş | Ürün sabiti | Aynı e-posta adresi (yalnız giriş yapmış üyenin siparişleri) ya da aynı IP başına — ayrı ayrı — **5 / 24 saat** | — | §3.17.8, §8.2 | K-332, K-460, K-528, K-579 |
| P-46 | L-8 Aynı anda açık ödenmemiş sipariş | Ürün sabiti | Aynı e-posta adresi (yalnız giriş yapmış üyenin siparişleri) ya da aynı IP başına — ayrı ayrı — **3** açık sipariş; pencere yoktur | — | §3.17.8, §3.21.5, §8.2 | K-524, K-332, K-579 |
| | **E-posta, saklama ve imha** | | | | | |
| P-38 | Gönderilemeyen e-postanın yeniden deneme aralıkları | Ürün sabiti | İlk denemeden **5 dakika**, **30 dakika** ve **2 saat** sonra — üç yeniden deneme | — | §9.1.6; §4 Z-26 | K-321, K-462 |
| P-39 | Periyodik imha aralığı | Ürün sabiti | **Günde bir** | — | §4 Z-36; §12 | K-354, K-449 |
| P-40 | İletişim talebinin saklama süresi | Ürün sabiti | Son kapatılıştan sonra **2 yıl**; hiç kapatılmayan açık talepte açılıştan **2 yıl** | — | §4 Z-33 | K-353, K-507 |
| P-41 | Bildirim gönderim kaydının saklama süresi — hesap ve yönetim e-postaları ile firmaya giden bildirimler; siparişe bağlı müşteri bildirimlerinin kaydı yasal süreyle üç yıldır ve envantere girmez (Z-43) | Ürün sabiti | **1 yıl** | — | §4 Z-34 | K-353, K-548 |
| P-43 | Malı dönmeyen caymada havale hattı IBAN'ının saklama süresi | Ürün sabiti | Müşterinin malı gönderme süresinin bitiminden **30 gün** | — | §4 Z-38; §7.4.5 | K-497 |
| P-45 | Giriş kaydının saklama süresi | Ürün sabiti | Kaydın oluştuğu andan **6 ay** | — | §3.13.20, §8.5.1; §4 Z-40 | K-515 |
| | **Site** | | | | | |
| P-42 | En dar desteklenen ekran genişliği | Ürün sabiti | **320 CSS pikseli** | — | §3.34.4 | K-394, K-462 |

### 11.3 Hacim kabulleri

Aşağıdakiler parametre değildir ve sistem hiçbirini uygulamaz: tasarımın dayandığı **tabanlardır**, tavan değildir. Aşıldığında ürün çalışmayı sürdürür ama performans garanti edilmez; aşımın ölçülebilir tetikleyicisi `10 §4`'tedir (K-409, K-26).

| # | Kabul | Değer | Kaynak |
|---|---|---|---|
| H-1 | Katalog | Fiziksel üründe birkaç yüz ürüne kadar; sistem ürün adedine tavan koymaz. Tek depo bir kabul değil, kalıcı sınırdır (`01 §7` S-5) | K-03, K-89, K-409, K-479 |
| H-2 | Sipariş hacmi | Ortalama günde 10, tepe günde 50 — tasarım varsayımıdır, hedef değildir; üç aylık bir dönemin en az iki ayında en yoğun gün 50 siparişi aşarsa kargo şirketi entegrasyonu ve havale eşleştirmesi post-MVP'den öne çekilir (`10 §4.2` SK-3). Tetikleyici ödemesi tamamlanmış siparişleri sayar; Başarısız ödemeli sipariş sayılmaz (K-181, K-457) | K-473, K-474, K-403, K-417, K-409, K-534 |
| H-3 | Sepet | Elli kaleme kadar sepette ödeme öncesi yeniden değerlendirme kabul edilebilir sürede yanıt verir — yanıt süresinin değeri `05`'in tasarım girdisidir; kalem sayısına tavan yoktur | K-409, K-154, K-129, K-533 |
| H-4 | Eşzamanlı ziyaretçi | `05`'in tasarım girdisidir | K-409 |

*Kaynak: K-411 (envanterin evi ve biçimi) · K-412, K-478 (katmanlar ve kurulum ayarları) · K-413 (varsayılanlar) · K-414 (iz) · K-447, K-448, K-449, K-459…K-462 (değerler) · K-463 (kırılma noktaları) · K-409 (hacim kabulleri) · K-03 · K-08 · K-26 · K-40 · K-42 · K-58 · K-59 · K-89 · K-92 · K-95 · K-102 · K-105 · K-108 · K-120 · K-128 · K-129 · K-132 · K-135 · K-139 · K-140 · K-142 · K-143 · K-146 · K-151 · K-154 · K-155 · K-166 · K-185 · K-219 · K-239 · K-242 · K-252 · K-269 · K-304 · K-308 · K-321 · K-328 · K-330 · K-332 · K-334 · K-336 · K-339 · K-353 · K-354 · K-393 · K-394 · K-403 · K-417 · K-463 · K-02 · K-63 · K-210 · K-337 · K-338 · K-416 · K-441 · K-497 · K-528 · K-534 · K-181 · K-457 · K-473 · K-474 · K-479 · K-502 · K-500 · K-503 · K-507 · K-355 · K-533 · K-555 · K-508 · K-515 · K-519 · K-523 · K-524 · K-548 · K-552.*

## 12. Uyumluluk ve yasal

> **Ne yazılır:** Yasal yükümlülükler, veri saklama/silme hakları, erişim kısıtları.

Ürün yükümlülüğü yerine getirmeyi mümkün kılan mekanizmayı sağlar; yükümlülüğün kendisi — kayıtlar, beyanlar, bildirimler, metinlerin içeriği — firmadadır. Satıcı ve veri sorumlusu firmadır ve sonuç tüketici rejimi gereği firmanın üstünde kalır (K-07, K-12, K-341). Firmanın yükümlülükleri §12.5'te tek tabloda toplanır.

### 12.1 Ticari çerçeve ve tüketici rejimi

**12.1.1 Tek pazar Türkiye'dir;** tek dil Türkçe, tek para birimi TRY'dir (K-16).

**12.1.2 Ürün tüketiciye satış için kurulmuştur.** 6502 sayılı Kanun'un korumaları her siparişte dallanmadan işler; ticari alıcıya özel akış yoktur (K-07). Alıcının hukuken tüketici sayılıp sayılmadığını mevzuat belirler; ürün alıcıdan ticari bilgi istemez ve ticari amaçla alan biri de aynı akıştan geçer (`01 §3.2` — K-07, K-112).

**12.1.3 Satıcı firmadır.** Tahsilat firmanın kendi ödeme sağlayıcı hesabına geçer; platform ticari zincirde yer almaz. Ürün kart verisini görmez, saklamaz, loglamaz ve her kart ödemesi 3D Secure'dan geçer (§3.21; K-12, K-13).

**12.1.4 ETBİS kaydı ve yasal kimlik.** ETBİS kaydı firmanın yükümlülüğüdür; ürün kayıt yapmaz, kayıt bilgisini isteğe bağlı "ETBİS doğrulama bilgisi" alanında taşır ve alan doluysa doğrulama bandını gösterir (K-574). Firmanın yasal kimlik bilgileri sitede sürekli erişilebilir durur (§3.1.4; K-14). Kimliğin doğruluğu firmanın sorumluluğudur (§3.1.2; K-08, K-445).

**12.1.5 Fiyat tüketiciye KDV dahil gösterilir** — bir seçenek değil, yasal kısıttır (§3.8.1; K-58, K-60). Ürün sayfası Fiyat Etiketi Yönetmeliği m.5'in zorunlu bilgilerini taşır: fiziksel üründe üretim yeri, ölçüyle satılan üründe birim fiyat — ölçü birimi ile net miktarı girmek firmanın yükümlülüğüdür (K-573) — ve fiyatın uygulanmaya başlandığı tarih; çevrim içi uygulanışı Bakanlık pratiğidir ve ürün tüketici lehine okur (§3.8.5; K-511).

**12.1.6 İndirim beyanı referans fiyata dayanır.** Fiyat Etiketi Yönetmeliği m.11/1'in RG 11/10/2025-33044 ile değişik metnini izler: fiziksel ve dijital üründe indirimin başladığı andan önceki on gün içinde uygulanmış en düşük fiyat, hizmette ve çabuk bozulan üründe indirimli fiyattan hemen önceki fiyattır; sistem hesaplar. Beyan edilen referans sipariş kalemine donar ve beyanın kanıtı olarak kalır. İndirim beyanının göründüğü her yerde indirimin başlangıç ve bitiş tarihi de gösterilir (Ticari Reklam ve Haksız Ticari Uygulamalar Yönetmeliği m.14/1) (§3.9; K-63, K-67, K-79, K-508, K-545).

**12.1.7 Mesafeli satış.** Sipariş, Ön Bilgilendirme Formu'nun teyidi ve Mesafeli Satış Sözleşmesi'nin onayı için iki ayrı kutuyla onaylanır; onaylanan sürümler ve firmanın o günkü kimliği siparişe donar. İki metin, siparişe donan sürümleriyle sipariş onay e-postasının gövdesinde tam metin olarak gönderilir — tüketicinin elinde kendi örneği kalır. Metinler cayma koşullarını, istisnaları, iade kargo bedelinin firmaya ait olduğunu ve tüketici hakem heyetine ya da — arabulucuya başvurma şartıyla — tüketici mahkemesine başvuru bilgisini (K-543) taşır; onay düğmesi siparişin ödeme yükümlülüğü doğurduğunu söyler (K-544) ve onay adımı sözleşmenin nasıl kurulduğunu, metnin sipariş sayfasında saklandığını ve veri giriş hatalarının özetten düzeltilebileceğini anlatır (6563 sayılı Kanun m.3; K-547); yaş kuralı Mesafeli Satış Sözleşmesi'ndedir ve ayrı bir üyelik sözleşmesi yoktur (K-453). Dört yasal metin sürümlüdür, metin değiştiğinde geçmiş siparişler için yeniden onay alınmaz ve dördü tamamlanmadan satış açılmaz (§3.24, §3.33; K-188…K-192, K-194, K-316, K-361…K-363, K-365, K-366, K-368, K-114).

**12.1.8 Teslim:** firma taahhüt ettiği sürede ve, kişiye özel üretilen mallar dışında, en geç otuz gün içinde teslim eder (K-549); süre sözleşme metninde yazar. Kargoya verme süresinin aşımında fesih müşterinin iptal düğmesiyle kullanılır. Otuz günlük sınır aşıldığında sipariş kargoya verilmiş olsa bile müşteri "gecikme nedeniyle fesih" yapar; satıcı teslimat masrafları dahil tahsil edilen tüm ödemeleri fesih bildiriminden itibaren on dört gün içinde kanuni faiziyle geri öder — tutarı sistem geri öder, kanuni faiz firmanın yükümlülüğüdür (§12.5) (Mesafeli Sözleşmeler Yönetmeliği m.16; §3.20.4, §7.2.3; K-139, K-168, K-198, K-499).

**12.1.9 Cayma, iade ve ayıp:** cayma penceresi on dört gündür ve fiziksel üründe teslimden, hizmette sipariş tarihinden işler — fiziksel kalem içeren siparişte hizmetin penceresi de teslimden işler (Yönetmelik m.9/5) —; tüketici malı teslim almadan da cayabilir; hak dijital üründe ön onayla, hizmette müşterinin onayıyla başlayan ifa tamamlanınca düşer — ifanın başlaması hakkı düşürmez ve kısmen ifa edilmiş hizmetten caymada kalemin bedelinin tamamı geri ödenir (K-578) —; istisna sebepleri Yönetmelik bentlerinin metnidir — mutlak sebepte hak yoktur, koşullu sebepte (koruyucu ambalaj) koruyucu ambalajı açılmamış malda hak sürer (K-509); beyan sipariş sayfasından ya da başka bir kanaldan gelen bildirimin firma tarafından kaydıyla yapılır (K-510); beyan tarih damgasıyla kayda geçer; geri ödeme on dört gün içinde yapılır — teslimden önceki caymada ve hizmette beyandan, teslimden sonraki caymada iade edilen malın firmaya ulaştığı tarihten — mal beyandan önce ulaştıysa beyandan; teslim günü yapılan beyan teslimden sonra sayılır —; müşteri malı beyandan itibaren on dört gün içinde gönderir ve ön bilgilendirme iade için taşıyıcı belirlenmediğini yazar; iade kargo bedeli cayma ve ayıpta firmaya aittir — taşıyıcı belirlenmediği için caymada yasal zorunluluktur; ayıp talebinin iki yılı bir zamanaşımıdır — sipariş sayfasındaki kanal kapanır, hile ve ağır kusur hâli iletişim formundan yürür (K-551) — ve seçimlik haklar sistem dışında yürür. Yasal süreler takvim günüyle sayılır (§4.1.3). Kuralların tamamı §7'dedir (K-195, K-203…K-207, K-209…K-211, K-226, K-289, K-290, K-337, K-340, K-491…K-494, K-503, K-520, K-554, K-509, K-510, K-551).

**12.1.10 Fatura ürünün dışındadır.** Ürün fatura kesmez ve e-Arşiv/e-Fatura entegrasyonu yoktur; ürünün tuttuğu fatura verisini taşır ve dışa aktarır — e-Arşiv faturanın ürün dışındaki alanlarını (ör. taşıyıcının vergi kimlik numarası) firma muhasebe aracında tamamlar. Fatura sistemde saklanmaz ve faturanın mali sorumluluğu firmadadır (§3.25; K-217, K-218, K-220, K-546).

**12.1.11 Ticari elektronik ileti gönderilmez.** Pazarlama iletisi yoktur, ürün İYS'ye bağlanmaz ve pazarlama onayı toplamaz. İşlem bildirimleri mevcut sözleşmenin yürütülmesine ilişkindir, ticari ileti değildir ve kapatılamaz (§9; K-323, K-324, K-326, K-377).

**12.1.12 Ürün ne satıldığını denetlemez.** İzin ya da ek akış gerektiren ürünler — alkol, tütün, ilaç, silah — MVP kapsamı dışındadır; mevzuata uygunluk firmanın yükümlülüğüdür (§1.2 kapsam notu; K-84).

### 12.2 Kişisel verilerin korunması

**12.2.1 Veri sorumlusu firmadır.** Ürünü kuran geliştirici ya da ürünün sağlayıcısı veri sorumlusu değildir. Aydınlatma metninde firmanın kimlik bilgileri görünür ve panelden gelir. VERBİS kaydı gerekiyorsa firmanın yükümlülüğüdür; ürün bunu üstlenmez ve denetlemez (K-341). **Bakım ve destek sırasında geliştirici veri işleyendir:** kuruluma bakım ya da destek için eriştiğinde firma adına veri işler; firma ile geliştirici arasında veri işleme sözleşmesi yapılır ve erişim bakım ile destekle sınırlıdır. Güncellemenin kuruluma hangi yolla ve kimin yetkisiyle ulaştığı `05`'in ve `DEPLOY_RUNBOOK`'un işidir (K-482).

**12.2.2 Aydınlatma yükümlülüğü.** Aydınlatma metni sürümlü ayrı bir kayıttır; ürün taslak metinle gelir, firma düzenler ve içeriğin sorumluluğunu üstlenir. Metin zorunludur ve boşaltılamaz; tamamlanıp yayına alınmadan iletişim formu, hesap kaydı ve Google ile ilk giriş kapalıdır (K-517). Bağlantısı yeni bir kişisel veri alanı açan her ekranda görünür (§3.33.5; K-553). Taslak alıcı gruplarını — kargo, fatura ve muhasebe, ödeme kuruluşu, barındırma ve e-posta altyapısı, bakım ve destek — ve her amacın hukuki sebebini anar (§3.33.2; K-553, K-516). Aydınlatma bir onay değildir: onay kutusu ve kullanıcı bazında "gördü" kaydı yoktur, ispat metnin sürümüyle sağlanır ve sipariş o günkü sürümü dondurur (§3.33; K-342, K-343, K-346, K-347).

**12.2.3 Hukuki sebepler KVKK m.5/2'nin kapalı sayımıyla yazılır; açık rıza alınmaz.** **Sipariş ve hesap verisi → m.5/2-c** (sözleşmenin kurulması ve ifası): misafir ve üye siparişinde ad, e-posta, teslimat ve fatura adresi — fiziksel kalemli siparişte alıcının telefonu dahil (§3.14.6) — siparişin ifası için zorunludur; hesap ilişkisi ayrı bir metin gerektirmeyen sözleşmesel bir ilişkidir. **Saklama, fatura ve KVKK başvurusu → m.5/2-ç** (hukuki yükümlülük) — iletişim formunun KVKK talebi konu tipiyle gelen başvurunun alınması, kaydı ve cevaplanması dahil (KVKK m.13; K-572). **Deneme limitleri, oturum ve giriş kayıtları, işlem izi ve KVKK talebi dışındaki iletişim talepleri → m.5/2-f** (meşru menfaat; güvenlik için m.5/2-ç ve m.12). Aynı sebepler aydınlatma taslağında yer alır (§3.33.2). Gereksiz rıza almak rızayı sakatlardı ve geri alınabilir rıza yürüyen siparişi ifa edilemez kılardı (K-344, K-345, K-516, K-515).

**12.2.4 Ürün hiçbir noktada açık rıza toplamaz.** Rıza yönetimi ekranı, rıza kaydı ve rızayı geri alma yolu yoktur; onay adımındaki kutular sözleşmeye ilişkindir, KVKK rızası değildir (K-345, K-346).

**12.2.5 Çerezler.** Yalnız zorunlu çerezler vardır ve ödeme sağlayıcısının çerçevesi dışında hepsi birinci taraftır: oturum, "beni hatırla", misafir sepetinin tarayıcıya bağlanması ve form güvenliği. **Ödeme sağlayıcısının çerçevesi kullanılıyorsa** sağlayıcının zorunlu çerezleri ödeme adımında bulunur: onay gerektirmezler ama çerez politikasında anılırlar — taslak bunun için bir yer tutucu taşır ve firma sağlayıcısının çerezlerini yazar (K-518). Ödeme çerçevesi dışında üçüncü taraf çerezi, izleme pikseli ve reklam etiketi yoktur; çerez onay bandı ve seçim kaydı yoktur. Çerez politikası sürümlü ayrı bir metindir ve altbilgide durur. Bir izleme aracı eklenirse çerez envanteri değişir ve bant zorunlu hâle gelir; MVP'de ziyaretçi ölçümü olmadığı için bu koşul gerçekleşmedi. Firma panelden ölçüm kodu ya da piksel ekleyemez; çerez envanteri bu yüzden — ödeme sağlayıcısının çerçevesi dışında — firmadan firmaya değişmez ve ürün kendi tabanını denetleyebilir (K-348…K-352, K-397, K-399, K-518).

**12.2.6 İletişim formunun "KVKK talebi" tipi ürünün sunduğu başvuru kanalıdır.** Tebliğ'in diğer başvuru yolları açık kalır ve aydınlatmada sayılır; formun alanları değişmez. Başvurunun zorunlu unsurlarının tamamlatılması ve cevaptan önce başvuranın kimliğinin doğrulanması firmanın yükümlülüğüdür — oturum açmadan doldurulan forma kimliği doğrulanmamış biri için verilen cevap verinin başkasına açıklanması olurdu (K-553). Başvuruya otuz gün içinde cevap vermek yasal yükümlülüktür ve firmanındır; sistemde sayaç tutulmaz. Uygulama içi veri indirme yoktur — kullanıcı verisini ister, firma cevabı hazırlar: siparişler sipariş listesinden, hesap verisi üye kaydı görünümünden okunur ve firma talep üzerine hesabı panelden silebilir (§3.15.4, §3.15.5; K-118, K-119, K-305, K-512).

**12.2.7 Saklama ve imha.** Süreler ve başlangıçları §4'tedir (Z-28…Z-44). Süresi dolan veri her gün kendiliğinden çalışan bir işle imha edilir; panelde "imha et" düğmesi yoktur ve imha işlem izine yazılmaz (K-449). Her imha çalışması — tarih, kayıt türü ve imha edilen kayıt sayısı — imha kaydına yazılır; kayıt kişisel veri içermez, sistem kaydında durur ve en az üç yıl saklanır (Kişisel Verilerin Silinmesi, Yok Edilmesi veya Anonim Hale Getirilmesi Hakkında Yönetmelik m.7/3; Z-44; K-552, K-354). On yıllık süre yalnız ödemesi alınmış siparişe uygulanır; ödemesi hiç alınmamış siparişin kişisel verisi üç yıl sonra silinir (Z-41; K-519). Siparişe bağlı müşteri bildirimlerinin gönderim kaydı üç yıl (Z-43; K-548), giriş kaydı altı ay (Z-40; K-515) saklanır. **İmhanın biçimi:** hesap ve talep verisi silinir — hiç kapatılmayan iletişim talebi de açılışından itibaren aynı sürenin sonunda (Z-33; K-507); sipariş kaydının süresi dolduğunda kaydın tamamı silinmez — içindeki kişisel veriler (ad, adres, e-posta, telefon) imha edilir ve **siparişin hesapla bağı kesilir:** kayıt hesabın sipariş geçmişinden düşer ve sipariş sayfasına hiçbir yoldan girilemez. Kalan ticari kayıt yalnız sipariş numarasını, kalemleri, tutarları, KDV'yi, indirim ve kargo payını, ödeme yöntemini, tarihleri, durumları ve firmanın donmuş kimliğini taşır; ürünün içinde kişiye götüren bir alan kalmaz (K-357, K-582). **Kayıt yine de firmanın elinde aynı siparişi anan bir kayıt durdukça anonim sayılmaz:** muhasebe aracındaki fatura ya da müşteriyle yazışma sipariş numarasını, tarihi ve tutarı taşır ve veri sorumlusu olan firma iki kaydı eşleştirebilir. Bu dönemde kalan kayıt kişisel veridir. Firma eşleşen kayıtları kendi saklama süresinin sonunda imha ettiğinde kayıt anonim hâle gelir (KVKK m.3/1-b; Kişisel Verilerin Silinmesi, Yok Edilmesi veya Anonim Hale Getirilmesi Hakkında Yönetmelik m.10/2; K-583). O kayıtların saklanması ve imhası ürünün dışındadır ve firmanın yükümlülüğüdür (§12.1.10, §12.5). Hesap silindiğinde giriş bilgileri, şifre, açık oturumlar, adres defteri ve sepet hemen silinir; silinen hesabın siparişleri hiçbir hesaba bağlanmaz ve yalnız ticari kayıt olarak yaşar. Havale hattındaki geri ödeme için alınan IBAN geri ödeme tamamlanınca silinir; malı dönmeyen caymada kalem kapatıldığında ya da müşterinin gönderme süresinin bitiminden otuz gün sonra silinir (§7.4.5; K-497). İşlem izi on yıl saklanır; sipariş satırları müşterinin kişisel verisini değer olarak taşımadığı için siparişin kişisel verisi imha edildiğinde izde kalmaz (§10.3.1; K-115, K-116, K-117, K-353…K-357, K-527).

**12.2.8 Kişisel verinin yurt dışına aktarımı.** Ürünün kendisi kişisel veriyi yurt dışına aktarmaz: gömülü harita, gömülü video, üçüncü taraf kodu, captcha ve analitik yoktur (K-243, K-254, K-263, K-304, K-397). Aktarım yalnız kurulumdan doğabilir — barındırma ve e-posta gönderim altyapısı ile ödeme sağlayıcısı; konumları ve yurt dışı aktarım şartlarının sağlanması firmanın yükümlülüğüdür ve `10 §4.1`'de dış ön koşuldur (ÖK-2). Aydınlatma taslağı barındırma konumu için firmanın dolduracağı bir bölüm taşır. **Ödeme sağlayıcısına aktarım vardır:** kart verisi ürüne hiç gelmez, ama ödemenin başlatılması ve geri ödeme için alıcının kimlik ve iletişim verisi — ad, e-posta, adres, IP — sağlayıcıya aktarılır; sağlayıcı ayrı veri sorumlusudur, hukuki sebep sözleşmenin ifasıdır (m.5/2-c) ve rıza gerekmez. Sağlayıcı yurt dışındaysa yurt dışı aktarım şartları firmanın yükümlülüğüdür; aydınlatmada alıcı grubu olarak anılır (K-513). Google ile girişte ürün yalnız dönen ad ve e-postayı alır — bu ilişki bir aktarım değildir. İkisinin aydınlatmada adıyla anılması firmanın yükümlülüğüdür (K-358, K-359, K-360, K-513).

**12.2.9 Veri ihlali.** Ürün ayrı bir ihlal kaydı tutmaz; ihlalin kapsamı işlem izi, giriş kaydı (§3.13.20, Z-40) ve sistem kayıtlarından okunur — limit sayaçları kaynak sayılmaz (§8.5.1; K-515). Kurul'a yetmiş iki saat içinde ve ilgili kişilere bildirim firmanın yükümlülüğüdür; ürün ihlali tespit etmeyi taahhüt etmez, firmaya uyarı üretmez ve bildirimi kendisi yapmaz. Ürünün toplu e-posta hattı yoktur: firma etkilenen kişilere kendi e-posta aracıyla ulaşır ve iletişim bilgilerini sipariş dışa aktarmasından — siparişin donmuş iletişim e-postası ve alıcı adı —, sipariş dışındaki kişiler için üye listesinin ve iletişim taleplerinin dışa aktarmasından alır; yöneticilerin iletişim bilgisi yönetici listesindedir (§8.5, §10.7.1, §10.7.4; K-380, K-381, K-382, K-426, K-514, K-512).

**12.2.10 Yönetim tarafında veri erişimi ayrımı yoktur.** Tek rolün sonucu olarak her yönetici bütün müşteri verisini görür; ürün rol bazlı veri kısıtı sunmaz. Firma, kime yönetici hesabı açtığına bu bilgiyle dikkat eder (§10.2.3; K-383).

**12.2.11 Kurumsal içerikte başkasına ait kişisel veriyi yayımlamanın sorumluluğu firmadadır.** Referans işte müşteri adı ve fotoğrafı, genel sayfada çalışan adı ve fotoğrafı gibi verilerin yayımı için gereken izni firma alır; sistem denetlemez (K-257).

**12.2.12 Veri asgariliği** — toplanmayan veriler, IBAN'ın ömrü ve serbest metin riski — §8.4'tedir.

### 12.3 Veri sahipliği ve süreklilik

**12.3.1 Veri firmanındır.** Kurulum ve içindeki bütün veri — katalog, içerik, siparişler, müşteri kayıtları, işlem izi — firmaya aittir; ürünün sağlayıcısı üzerinde hak iddia etmez, veriyi kilitlemez ve erişimi kesmez. Bakım aboneliği sona erse bile firma verisiyle ve çalışan kurulumuyla kalır: abonelik güncelleme ve desteği kapsar, çalışma hakkını değil (K-410). **Çalışma hakkı korunur, uyum korunmaz:** güncelleme almayan kurulumun yasal uyumu ve dış servis değişikliklerinin sonucu — duran ödeme yöntemi ya da giriş dahil — firmadadır; abonelik sözleşmesi bunu yazar (K-481).

**12.3.2 Çıkış hakkı barındırma düzleminde karşılanır.** Kurulum firmanın kendi barındırmasında çalışır ve veritabanı ile yedekler firmanın elindedir; katalog ve içerik için ürün içinde bir dışa aktarma yolu yoktur. Sipariş tarafı panelden dışa aktarılır (§10.7). Yedek erişiminin tarifi `DEPLOY_RUNBOOK`'tadır (K-427, K-426, K-08, K-358).

**12.3.3 Ürün çalışma süresi taahhüdü vermez ve sipariş tarafında veri kaybına tolerans sıfırdır;** kurallar §3.34.7 ve §3.34.8'dedir (K-406, K-407).

### 12.4 Erişilebilirlik

**12.4.1 Hedef WCAG 2.1 AA'dır; uyumluluk beyanı verilmez.** Beyan her ekranda her ölçütün denetimini ister ve tutulamayan beyan firmanın üstünde kalırdı; bunun yerine yedi kural zorunlu ve test edilebilirdir (§3.34.5; K-395, K-396).

### 12.5 Firmanın yükümlülükleri

| Yükümlülük | Ürünün sağladığı | Firmanın yaptığı | Kaynak |
|---|---|---|---|
| ETBİS kaydı | Kayıt bilgisini isteğe bağlı bir alanda taşır, alan doluysa doğrulama bandını gösterir | Kaydı yaptırır ve doğrulama bilgisini panelden girer | K-14, K-574 |
| Yasal kimliğin doğruluğu | Zorunlu alan ve işlem izi | Bilgileri doğru girer | K-08, K-445 |
| Fiyat etiketinin birim fiyatı | Ölçü birimi ve net miktar alanı; doluysa birim fiyatı türetip gösterir, panelde yükümlülüğü hatırlatır | Ölçüyle satılan üründe alanı doldurur | K-511, K-573 |
| VERBİS | — | Gerekiyorsa kaydı yaptırır | K-341 |
| Aydınlatma metni ve çerez politikası | Taslak metin ve yer tutucular — alıcı grupları bölümü ve ödeme çerçevesinin çerezleri dahil; sürüm ve gösterim | İçeriği kendi veri pratiğine göre düzenler; barındırma konumunu, alıcı gruplarını adıyla ve — çerçeve kullanılıyorsa — sağlayıcının çerezlerini yazar | K-343, K-350, K-360, K-518, K-553 |
| Barındırma, e-posta altyapısı ve ödeme sağlayıcısının konumu; yurt dışı aktarım şartları | — | Konumu belirler, aktarım şartlarını sağlar; aktarım standart sözleşmeye dayanıyorsa sözleşmeyi imzalanmasından itibaren beş iş günü içinde Kurum'a bildirir | K-358, K-513, K-542 |
| Ödeme sağlayıcısı ve Google'ın aydınlatmada anılması | Taslakta yer tutucu; ödeme sağlayıcısı alıcı grubu olarak | Metne yazar — ödeme sağlayıcısına alıcının kimlik ve iletişim verisinin aktarıldığını dahil | K-359, K-513 |
| KVKK başvurusuna cevap | Başvuru kanalı ve talep kaydı; üye kaydı görünümü ve hesabın talep üzerine silinmesi | Başvurunun zorunlu unsurlarını tamamlatır, cevaptan önce başvuranın kimliğini doğrular, diğer başvuru yollarından gelenleri de kabul eder ve otuz gün içinde cevap verir | K-118, K-305, K-512, K-553 |
| Veri ihlali bildirimi | İz, giriş kaydı ve dışa aktarmalar — sipariş, üye listesi, iletişim talepleri | Kurul'a ve ilgili kişilere bildirir | K-380…K-382, K-514, K-515 |
| İhlal kaydı ve müdahale planı | İşlem izi ve giriş kaydı | İhlali, etkilerini ve alınan önlemleri kayda geçirir; bir ihlal müdahale planı tutar | K-380, K-542 |
| Kişisel veri saklama ve imha politikası | Süre tablosu (§4) ve periyodik imha; imha kaydı (Z-44) | Veri Sorumluları Siciline kayıt yükümlüsüyse politikayı hazırlar; ürünün dışındaki faturayı ve müşteriyle yazışmayı kendi saklama süresinin sonunda imha eder — süresi dolan siparişin kalan kaydı ancak bundan sonra anonimdir (§12.2.7) | K-353, K-542, K-552, K-583 |
| İçerikte üçüncü kişi verisi | — | Yayım iznini alır | K-257 |
| Yönetici hesabı açılan kişiler | Tek rol, işlem izi | Kime hesap açtığına dikkat eder | K-383 |
| Faturanın kesilmesi | Ürünün tuttuğu fatura verisi ve dışa aktarma | Faturayı keser, e-Arşiv faturanın ürün dışındaki alanlarını muhasebe aracında tamamlar ve faturayı müşteriye iletir | K-217, K-218, K-546 |
| Satılan ürünün mevzuata uygunluğu | — | Uygunluğu sağlar | K-84 |
| Serbest metnin ayarlarla tutarlılığı | Ayarın yanında hatırlatma; bağlayıcı metin ayarlardan üretilir | Metni güncel tutar | K-364 |
| Kargo ücretinin KDV'sinin bölüşümü | Kargonun KDV'sini fiziksel kalemlerin oranına — farklı oranlarda tutara orantılı — bölen hesap ve döküm; dışa aktarmada bölünmüş tutarlar (§3.19.2) | Kurulumda bölüşüm yöntemini mali müşavirine teyit ettirir ve faturayı buna göre keser | K-500 |
| Gecikme feshinde kanuni faiz | Feshedilen kalemlerin bedelinin kargo ücreti dahil geri ödenmesi; fesih satırında ve taahhüt edilen süre aşıldıktan sonraki iptalde "gecikme feshi — kanuni faiz yükümlülüğü" uyarısı — sistem faizi hesaplamaz | Kanuni faizi hesaplar ve müşteriye öder | K-499 |
| Güncelleme almayan kurulumun yasal uyumu | Abonelik sürerken güncellemeler | Abonelik bittikten sonra mevzuat ve dış servis değişikliklerinin sonucunu — duran ödeme yöntemi ya da giriş dahil — üstlenir (§12.3.1) | K-481, K-542 |
| Bakım, destek ve altyapı sağlayıcılarıyla veri işleme sözleşmesi | Geliştiricinin erişimi bakım ve destekle sınırlıdır | Geliştiriciyle, barındırma ve e-posta sağlayıcılarıyla veri işleme sözleşmesi yapar (§12.2.1) | K-482, K-358, K-542 |
| Teslim tarihinin ve iade malının ulaşma tarihinin doğruluğu | Tarih alanları — geçmişe dönük olabilir, ileri tarihli olamaz — ve işlem izi | Tarihleri doğru ve zamanında girer; ispat yükü kendisindedir (§3.20.11, §7.4.1, §6.4.17) | K-288, K-491, K-542 |
| Geri ödemenin yasal süre içinde yapılması | Kart hattında iptalde kendiliğinden başlayan geri ödeme; panelde kalan süre (Z-16, Z-17) | Havale hattındaki ve caymadaki geri ödemeyi on dört gün içinde işler — süre dolunca sistem kendiliğinden işlem başlatmaz | K-209, K-291, K-491, K-542 |
| Dışa aktarılan dosya | Dışa aktarmanın işlem izi | Dosyayı korur | K-426 |
| Misafir siparişinin e-postasını düzeltirken kimlik teyidi | Düzeltme ekranı; eski anahtarın geçersizleşmesi, eski adrese B-15 ve eski ve yeni değerle iz satırı | Müşterinin kimliğini siparişteki bilgilerle teyit eder; teyidin ispatı ve kandırılmanın sonucu kendisindedir (§10.4.11) | K-585 |

*Kaynak: K-341…K-347 (veri sorumlusu, aydınlatma, hukuki sebep, rıza) · K-348…K-352 (çerez) · K-353…K-357 (saklama ve imha) · K-358…K-360 (yurt dışı) · K-380…K-383 (ihlal ve erişim) · K-406, K-407, K-410, K-427 (süreklilik ve sahiplik) · K-07 · K-08 · K-12 · K-13 · K-14 · K-16 · K-58 · K-60 · K-63 · K-67 · K-79 · K-84 · K-114 · K-115 · K-116 · K-117 · K-118 · K-119 · K-139 · K-168 · K-188 · K-189 · K-190 · K-191 · K-192 · K-194 · K-195 · K-198 · K-203 · K-204 · K-205 · K-206 · K-207 · K-209 · K-210 · K-211 · K-217 · K-218 · K-220 · K-226 · K-257 · K-289 · K-290 · K-305 · K-316 · K-323 · K-324 · K-326 · K-340 · K-364 · K-366 · K-368 · K-377 · K-395 · K-396 · K-397 · K-426 · K-445 · K-243 · K-254 · K-263 · K-304 · K-337 · K-361 · K-362 · K-363 · K-365 · K-399 · K-449 · K-453 · K-491…K-494 · K-497 · K-499 · K-527 · K-112 · K-288 · K-291 · K-481 · K-482 · K-542 · K-500 · K-503 · K-507 · K-520 · K-554 · K-508 · K-509 · K-510 · K-511 · K-512 · K-513 · K-514 · K-515 · K-516 · K-517 · K-518 · K-519 · K-543 · K-544 · K-545 · K-546 · K-547 · K-548 · K-549 · K-551 · K-552 · K-553 · K-561 · K-572 · K-582 · K-583 · K-585.*

## 13. Açık kararlar

> **Ne yazılır:** Bilinçli olarak ileriye bırakılan **detaylar** ve ne zaman karara bağlanacakları. Varlık kararı burada asla açık kalmaz.

**13.1 Bu tablo `02`'nin kendi açık kararlarını taşır ve doküman yaşadığı sürece yaşar.** Doküman üretim döneminin açık kalem kaydı `PRODUCT_DISCOVERY_STATUS.md` §4'tür; o dosya aşama kapanışında arşiv işareti alır ve yerinde kalır (K-428, K-436).

**13.2 Aşama kapanışında tracker §4'ün açık her satırı bir dokümana atanır.** `02`'nin konusuna girenler bu tabloya taşınır; atanamayan satır kapatılır ya da gerekçesiyle tracker'da kalır ve kapanışın devir notuna yazılır (K-428).

**13.3 Kapanıştan önce bir çakışma taraması koşar.** Tracker §4 bu tabloyla ve `01` ile `10`'un açık kalem listeleriyle karşılaştırılır: aynı açık iki yerde durmaz; her satır gözlemlenebilir bir kapı taşır — taşımayan satır kapatılır ya da kapısı yazılır; vadesi geçmiş ve hâlâ açık satır kapanış raporunda adıyla listelenir (K-429, K-37).

**13.4 Yazım turu sonunda `02`'ye ait açık karar yoktur.** A-05 — sözlükteki İngilizce karşılıklar — K-455 ile; A-12 — yazıma kalan üç parça — K-445, K-456 ve K-457 ile kapandı. Tracker §4'ün son açık satırı A-11 `10`'a aitti ve `10`'un yazımında kapandı (K-470, K-471, K-472); yazım turu sonunda tracker §4'te açık satır kalmadı. `01`'in kalite döngüsünde açılan A-13 `10 §5`'e aittir ve bu tabloya girmez (K-428). Aynı döngüde açılan A-14 — K-209'un geri ödeme kuralının güncel mevzuata uyumu — K-491 ile kapandı; kapanışı iki açık doğurdu ve ikisi de `02`'ye aittir: A-15 (malı hiç dönmeyen caymada iptal ve iade oranının okunma anı ve bekleyen işler sayacı — §10.6) ve A-16 (K-292'nin güncel metne uyumu — §7.4.6). A-15 K-495 ve K-496 ile kapandı ve A-17'yi doğurdu (malı dönmeyen caymada havale hattı IBAN'ının saklanma süresi — §4.2 Z-35). A-17 K-497 ile kapandı ve A-18'i doğurdu (IBAN silindikten sonra ulaşan malda IBAN'ın müşteriden yeniden isteniş biçimi — §7.4.5); A-18 K-498 ile kapandı. A-16 bu dokümanın kalite döngüsünde K-501 ile kapandı: kargo ücretinin geri ödenme tetiği siparişin iptal edilmemiş fiziksel kalemlerinin tamamıdır (§7.2.9, §7.4.6). **Öneriyle kaydedilen karar kapalıdır:** `(öneriyle kaydedildi)` işaretli bir karar — ⚠ taşısa da, K-498 dahil — geçerli bir kayıttır. ⚠ kararın proje sahibinin gözden geçirme listesine girdiğini gösterir; itiraz gelirse karar yeni bir karar satırıyla değişir, açık karar olarak bu tabloya dönmez. Bu yüzden tablo boştur; satırını implementation döneminde doğan bir açık açar.

| # | Konu | Ne belirsiz | Ne zaman karara bağlanır |
|---|---|---|---|

*Kaynak: K-428 (§13 ile tracker §4'ün ayrımı) · K-429 (çakışma taraması) · K-436 (arşiv işareti) · K-37 (vade kuralı) · K-445 · K-455 · K-456 · K-457 · K-470…K-472 (A-11) · K-491 (A-14) · K-495, K-496 (A-15) · K-497 (A-17) · K-498 (A-18) · K-501 · K-209 · K-292.*

---

*Shopfolio — Product Requirements v0.25 (§1–§13 taslak; kalite döngüsü: audit ✓, deep review ✓, cross-review sürüyor — 9. tur işlendi, BULGU-2 K-585 ile kapandı — K-430, K-431)*
