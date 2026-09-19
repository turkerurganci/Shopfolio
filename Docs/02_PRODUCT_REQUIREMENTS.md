# Shopfolio — Product Requirements

**Versiyon: v0.7** | **Bağımlılıklar:** `01_PROJECT_VISION.md`, `PRODUCT_DISCOVERY_STATUS.md` | **Son güncelleme:** 2026-09-19

> **Aşama:** 1 — Product Discovery · **Rol:** Product Manager
> **Traceability zorunlu:** Hayır (kaynak doküman — sonraki aşamalar buraya izlenir)
> **Bu doküman tüm iş kurallarının tek kaynağıdır.** Kod bu dokümanla çelişemez.

> **Taslak durumu (K-28):** §1, §2, §3, §5, §6 ve §7 **taslak** olarak yazılmıştır. §1 2026-09-15'te yazıldı; o gün §6.2'nin "Hedef doküman" sütunu bu bölümü Blok 1 ve Blok 3'e bağlıyordu ve ikisi de kapalıydı. §2, §3, §5, §6 ve §7 2026-09-19'da, besleyen blokları hem §6.2 sütununa hem kararların etki sütunlarına göre kapalıyken yazıldı (§2: 1, 6, 7 · §3: 1–7 · §5: 1, 3–7 · §6: 1, 3, 5, 6 · §7: 1, 6). Aynı gün sütun düzeltildi ve Blok 8'i bu beş bölüme, Blok 9'u §3'e de bağladı; taslaklar geçerliliğini korur, o blokların bu bölümlere dokunan kararları K-29 işaretiyle işlenir (2026-09-15'teki §1 kalıbı). §4 ve §8–§13 henüz yazılmamıştır — her bölümün yazım kapısı kendi başlığının altındadır; kapılardaki besleyen blok listeleri 2026-09-19'da §6.2 sütunu ile etki sütunlarının birleşimine hizalandı. Kalite döngüsü (audit → deep review → cross-review → etki yansıtma → checkpoint) bu dokümanda **tüm bloklar kapandıktan sonra** ve **bir kez** koşar; sıra `01` → `02` → `10`'dur (00 §C.5).
> Taslak yazıldıktan sonra alınan bir karar taslak bir bölüme dokunursa, karar satırı `(taslak güncellenecek)` işaretini taşır ve güncelleme aynı bloğun `docs:` PR'ında yapılır (K-29).

> **§1 kapanmamıştır ve Blok 8'den önce kapanmaz.** Sözlük ve aktör envanteri, diğer bölümlerin aksine **tek bir blokta bitmez** — terim üreten her blok ona satır ekler. §6.2'nin sütunu bunu görmüyordu; **2026-09-15'te düzeltildi** ve `02 §1` Blok 4–8'e de bağlandı (gerekçe: `PRODUCT_DISCOVERY_STATUS.md` §6.2, tablo altındaki not). Bugün bilinen eklemeler: K-17 sipariş durum adlarının sözlüğe ilk satırlarını `B6-03`'e bırakmıştır; **Sepet** ve **Sipariş kalemi** terimleri `B5-01`/`B5-03` ve `B6-04` ile tanımlanacaktır.
> **Kapalı Blok 3'ten gelen eksik kapatıldı (2026-09-15):** İndirim, Kupon ve Referans fiyat tam karara bağlı olduğu hâlde sözlükte satır taşımıyordu — kararları `02 §3`/`§5`'e yönlendirilmiş, `§1`'e yönlendirilmemişti. Üçü de eklendi; K-63 ve K-70'in etki sütunları düzeltildi. **Sepet** ve **Sipariş kalemi** hâlâ eksiktir ve bilinçli olarak bekletilmektedir — tanımları `B5-01`/`B5-03` ve `B6-04`'e bağlı, bugün yazılamaz.
> **Blok 4 eklemeleri (2026-09-16):** K-06'nın `B4-01`'e bıraktığı soru kapandı — **misafir alıcı dördüncü aktördür** (K-97) ve §1.3 üçten dörde çıktı, listedeki "aktör değildir" maddesi kalktı. Sözlüğe altı satır girdi: **Misafir alıcı · Oturum · Sosyal giriş · Adres defteri · Teslimat adresi · Fatura adresi** (K-97, K-98, K-103, K-104, K-106, K-108, K-111, K-112, K-113). Altısı da türetilmiştir (**†**) ve **A-05**'e eklenmiştir; sözlük 25 → **31 terim**. §1.3'e ayrıca üçüncü bir aktör kuralı ve yönetici tarafının kimlik doğrulama tabanı yazıldı (K-122).
> **Blok 5 eklemeleri (2026-09-17, v0.4):** sözlüğe üç satır girdi — **Sepet · Sepet kalemi · Stok ayırma** (K-123, K-128, K-131). Üçünün de İngilizce karşılığı karar kaydında yazılı olduğu için † almadılar; sözlük 31 → **34 terim**. Yukarıdaki iki notun beklettiği **Sepet** terimi böylece kapandı.
> **Blok 6 eklemeleri (2026-09-18, v0.5):** sözlüğe beş grup altında **26 satır** girdi: **Sipariş** (Sipariş · Sipariş kalemi · Sipariş numarası), **sevkiyat ekseni** (Sipariş durumu ve altı durum), **Ödeme** (Ödeme yöntemi · Havale/EFT · Ödeme süresi · Ödeme durumu ve beş durum), **İptal, cayma ve iade** (İptal · Cayma · İade · Geri ödeme · Ayıp talebi), **Yasal metinler** (Ön Bilgilendirme Formu · Mesafeli Satış Sözleşmesi) — K-159…K-236. Sözlük 34 → **60 terim**. Durum adlarının on üçünün İngilizce karşılığı karar kaydında yazılıdır ve † almadı; diğer on üç satır türetilmiştir (**†**) ve **A-05**'e eklenmiştir (20 → **33**). K-17'nin sipariş durum adlarını `B6-03`'e bırakan devri ve **Sipariş kalemi**'nin `B6-04`'e bağlı beklemesi kapandı. **Kapanış taramasının iki bulgusu:** (1) en temel terim olan **Sipariş** sözlükte hiç yoktu — Blok 3'ten beri kayıtta kullanılıyordu ve hiçbir blok onu adıyla üstlenmemişti; (2) **"iade" iki kavramı taşıyordu** — malın firmaya dönmesi ve paranın müşteriye dönmesi. K-17'nin tek ad kuralı gereği ayrıldı: **İade** (`Return`) yalnız malın dönüşüdür, **Geri ödeme** (`Refund`) paranın dönüşüdür; ödeme eksenindeki iki durumun adı buna hizalandı (**Geri ödendi**, **Kısmen geri ödendi** — K-172, K-222).
> **Blok 7 eklemeleri (2026-09-19, v0.6):** sözlüğe iki yeni grup altında **15 satır** girdi: **Kurumsal içerik** (Kurumsal içerik · Hakkımızda · Hizmet tanıtımı · Referans iş · Sık sorulan soru · Şube · Genel sayfa · Duyuru · Sosyal medya bağlantısı) ve **Marka kimliği** (Marka kimliği · Marka adı · Unvan · Logo · Site simgesi · Marka rengi) — K-237…K-286. On beşinin de İngilizce karşılığı karar kaydında yazılıdır ve † almadı; sözlük 60 → **75 terim**, A-05'in türetilmiş sayısı **33'te kaldı**. **Yedi tanım düzeltildi:** *Ana kategori* — ürünün adresi artık kategoriden bağımsızdır, ana kategori yalnız kırıntı yolunu üretir (K-279); *Yayın durumu*, *Taslak*, *Yayında*, *Arşiv* — kurumsal içerik yayın durumunun yalnız iki değerini kullanır, arşiv yoktur (K-273, K-274, K-275); *Hizmet* — fiyatsız **Hizmet tanıtımı**ndan ayrılır (K-240); *Firma* — yasal adı **unvan**, vitrinde görünen adı **marka adı**dır (K-260). **Ad çakışmaları kayda geçti:** "Referans" tek başına hem *Referans fiyat* ile hem dokümanların atıf anlamıyla çakıştığı için terim **Referans iş** oldu (K-241); "Kampanya" K-63'ün indirimini anlattığı için duyurunun adı **Duyuru** oldu (K-272).
> **§2, §3, §5, §6, §7 taslağı (2026-09-19, v0.7):** K-28'in üçüncü uygulaması; K-30 gereği workshop'tan ayrı bir oturumda yazıldı. Girdi: karar kaydı (`PRODUCT_DISCOVERY_STATUS.md` §2 ve §4), bu dokümanın şablonu ve üst doküman `01` — workshop sohbet geçmişi kullanılmadı. Etki sütununda bu beş bölümden birini taşıyan her karar satırı kendi bölümünde karşılık bulur. **Yazım, kaydın yeterlilik testiydi (K-30) ve beş boşluk buldu;** bölümlerde "açık" diye adıyla işaretlidir ve `PRODUCT_DISCOVERY_STATUS.md` §4'te **A-06…A-10** olarak izlenir. **§1'e üç satır girdi:** §5.10'un kullandığı Ayıp talebi durumu · Açık · Çözüldü — kodları kayıtta yazılı olmadığı için † ile türetildi; sözlük 75 → **78 terim**, A-05 33 → **36**. **Blok 8 ve Blok 9'un bu bölümlere dokunuşu:** konu başlıkları adıyla dokunuyor — `B8-01` talep durumları (§5), `B8-13` şikâyet kanalı (§7), `B8-14` elle durum geçişi ve elle sipariş (§2, §5, §7), `B9-14` toplu veri işlemleri (§3). §6.2 sütunu bunu göstermiyordu; aynı gün düzeltildi (`PRODUCT_DISCOVERY_STATUS.md` §6.2, tablo altındaki not). Bu konuların kararları K-29'un `(taslak güncellenecek)` işaretini taşır. Düzeltmenin tek kapı etkisi: §10'un kapısı Blok 9'a geçti.

> **Karar referansları:** Metindeki `K-xx` işaretleri `PRODUCT_DISCOVERY_STATUS.md` §2 karar kaydına, `Bx-yy` işaretleri aynı dosyanın §6.3 blok içeriklerine gider.

---

## 0. Nasıl kullanılır

- Her iş kuralı **numaralandırılır** ve bölüm referansıyla anılır (`§4.7`). Sonraki dokümanlar ve task'lar bu numaraya atıf yapar.
- Sayısal parametreler (süre, oran, limit, eşik) **tablo hâlinde** toplanır — dağınık yazılırsa tutarsızlık kaçınılmaz olur.
- **Admin esnekliği prensibi:** Rakamsal parametreleri yönetici tarafından değiştirilebilir yapmak, "doğru rakam ne?" tartışmasını ürün aşamasından çıkarır. Hangi parametrenin runtime ayarlanabilir olduğu burada belirtilir.
- "Muhtemelen", "belki", "sonra karar veririz" yasak. Detay ileriye bırakılabilir; **varlık kararı** burada alınır.

---

## 1. Terimler ve aktörler

> **Ne yazılır:** Projede kullanılan her terimin tek tanımı. Aynı kavram iki farklı isimle anılmamalı.

### 1.1 Sözlük kuralı

Terim sözlüğü **bu bölümde yaşar** ve üç sütunludur: Türkçe terim · İngilizce kod karşılığı · tanım. Kurallar (K-17):

- **İngilizce karşılık Aşama 1'de yazılır** — terim sözlüğe girdiği anda. Sonraya bırakılmaz.
- **Bir kavramın tek adı vardır.** Eş anlamlı kullanım yasaktır; aynı şeyi anlatan iki terim birlikte kullanılamaz.
- **`06`, `07` ve `09` bu sözlüğü birebir devralır**, isim icat etmez.

Kuralın gerekçesi `03` ve `04`'ün `06`'dan **önce** yazılmasıdır: İngilizce karşılık veri modeline bırakılsaydı o aralıkta durum ve enum adları başıboş kalır, `06` geldiğinde geriye dönük hizalama gerekirdi (K-17).

**Bedeli kayıtlıdır:** henüz varlığa dönüşmemiş kavramlara erken isim verilir ve `06` yazılırken birkaç satırın revize edilmesi beklenir — bu bir sapma değil, normal akıştır (K-17).

### 1.2 Terim sözlüğü

> **† işareti:** İngilizce karşılığı karar kaydında **yazılı olmayan**, bu taslakta sözlüğün kendi adlandırma konvansiyonundan türetilmiş satırları gösterir. Varlık kararı açık değildir — sütun dolacaktır; açık olan yalnız dizenin kendisidir. İzleme: `PRODUCT_DISCOVERY_STATUS.md` §4, **A-05**.

**Katalog düzlemleri**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Ürün | `Product` | Vitrin ve katalog birimi. Ad, açıklama, kategori, tip, KDV oranı ve indirim bu düzlemde yaşar; görsel de üründedir ama varyant kendi görselini taşıyarak ezebilir. Ürün doğrudan satılmaz — satılan birim varyanttır (K-39, K-59, K-64, K-82, K-90, K-96). |
| Varyant | `Variant` | Satılan birim. Stok ve fiyat bu düzlemde yaşar. Seçeneği olmayan ürünün de tek bir varyantı vardır ve ziyaretçi bunu fark etmez (K-39). |
| Seçenek | `Option` | Varyantları ayıran boyutun adı — "Renk", "Beden". Bir ürün **en fazla iki** seçenek boyutu taşır (K-40, K-41). |
| Seçenek değeri | `OptionValue` | Bir seçeneğin aldığı değer — "Kırmızı", "M" (K-41). |
| Stok kodu | `SKU` | Her varyantın taşıdığı **zorunlu ve tekil** kod. Tip ayrımı yoktur; dijital ve hizmet varyantı da kod taşır (K-88). |

**Ürün tipi**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Tip | `ProductType` | Ürünün üç değerli **kapalı** alanı. Ürün düzleminde yaşar; ürünün tüm varyantları aynı tiptedir, varyant tipi devralır ve kendi tipini taşımaz (K-82). |
| Fiziksel ürün | `Physical` † | Tip değeri. Kargoyla teslim edilen mal; sayısal stok takibi **zorunludur** (K-81, K-83). |
| Dijital ürün | `Digital` † | Tip değeri. İndirilebilir dosya; stok alanı **yoktur**, sınırsız satılır (K-81, K-83). |
| Hizmet | `Service` † | Tip değeri. Sabit fiyatlı, randevusuz hizmet. Firma isterse toplam adet **kontenjanı** girer; girmezse sınırsızdır (K-81, K-83). Satın alınmayan, fiyatsız **Hizmet tanıtımı**ndan ayrıdır (K-240). |

**Sınıflandırma**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Kategori | `Category` | Ürünün kataloğdaki yeri. Ağaç **en fazla üç seviyedir**; ürün ağacın herhangi bir düğümüne asılabilir, yaprak zorunluluğu yoktur (K-42, K-43). |
| Ana kategori | `PrimaryCategory` | Ürünün asıldığı kategorilerden biri. Kırıntı yolu bundan üretilir; yayına çıkacak ürün için **zorunludur** (K-45). Ürünün **adresi kategoriden bağımsızdır** — ürünün adından üretilir (K-279). |

**Yayın durumu**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Yayın durumu | `PublishStatus` † | Ürün ve varyantta üç değerli alan. **Her iki düzlemde de yaşar:** ürünün kendi durumu, varyantın kendi durumu vardır (K-54, K-55). **Kurumsal içerik yalnız iki değerini kullanır** — Taslak ve Yayında (K-273). |
| Taslak | `Draft` | Henüz yayınlanmamış kayıt. Adresi ziyaretçiye 404 döner; giriş yapmış firma yöneticisine sayfayı "Taslak" bandıyla gösterir (K-54, K-57, K-274). |
| Yayında | `Published` | Vitrinde görünen kayıt. Bir ürünün "Yayında" olabilmesi için **arşivlenmemiş en az bir varyantı** olmalıdır (K-54, K-55); bir kurumsal içerik kaydının zorunlu alanları dolu olmalıdır (K-275). |
| Arşiv | `Archived` | Vitrinden çıkmış ama kaydı korunan ürün. **Silme değildir** — adresi çalışmaya devam eder ve "Bu ürün artık satılmıyor" sayfası döner (K-54, K-56). Kalıcı silme ayrı bir yoldur; silinen ürünün adresi 404 döner (K-85). **Kurumsal içerikte arşiv yoktur** (K-273). |

**Stok ve fiyat**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Stok | `StockQuantity` † | Varyantın sayısal adedi. Ürün düzleminde ayrı bir stok alanı **yoktur**; ürünün stok durumu varyantlarından türetilir (K-50). |
| Stok ayırma | `StockReservation` | Sipariş onaylandığı anda siparişe giren adetlerin stoktan ayrılması. Ödeme başarılı olursa adet **kesin düşer**; ödeme süresi dolar ya da ödeme başarısız olursa stoğa geri döner. Sepete eklemek stok ayırmaz. Aynı rejim hizmet kontenjanına ve kupon kullanım hakkına uygulanır. **"Rezervasyon" kelimesi bilerek kullanılmadı** — K-11 o kelimeyi randevu anlamında kapsam dışı bıraktı (K-11, K-128). |
| KDV oranı | `VatRate` † | Ürün düzleminde tutulan oran; varyantlar devralır. Varsayılanı ayardan gelir (K-58, K-59). |

**İndirim ve kupon**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| İndirim | `Discount` † | **Ürün düzleminde** tanımlanan ve tüm varyantlara inen **yüzdesel** indirim; her varyant kendi fiyatı üzerinden indirilir. **Tarihlidir** — başlangıç ve bitiş girilir, sistem indirimi kendisi başlatır ve bitirir (K-63, K-64, K-65, K-66). |
| Referans fiyat | `ReferencePrice` † | İndirim beyanının dayandığı fiyat: indirimin başladığı ana kadarki **30 gün** içinde o varyanta uygulanmış **en düşük** fiyat. **Sistem hesaplar**, yönetici giremez. 30 günlük geçmişi olmayan üründe referans, ürünün yayına girdiğinden beri uygulanmış en düşük fiyattır (K-63, K-67). |
| Kupon | `Coupon` † | Ödeme adımında girilen kod; **sepet toplamına** iner ve indirimli fiyatın üzerine uygulanır. Yüzde veya sabit tutar olabilir; sabit tutarlı kupon zorunlu bir asgari sepet tutarı taşır. Sınırı **tarih + toplam kullanım adedidir** (K-70, K-71, K-73, K-74, K-75). |

**Aktörler ve taraflar**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Firma | `Company` † | Kurulumun sahibi ve **satıcı**. Uygulamada tekildir: firma ekleme, firma seçme ve firmalar arası geçiş kavramı yoktur (K-01, K-09). Tahsilat firmanın kendi ödeme sağlayıcı hesabına geçer; platform ticari zincirde yer almaz (K-12). Yasal adı **unvan**, vitrinde görünen adı **marka adı**dır (K-260). |
| Ziyaretçi | `Visitor` † | Siteye üye olmadan gelen kişi (K-06). |
| Misafir alıcı | `GuestBuyer` † | Hesap açmadan sipariş veren alıcı. Siparişini **sipariş numarası + e-posta** ile takip eder; doğrulanmış e-postayla hesap açtığında o e-postaya ait geçmiş siparişleri hesabına düşer (K-97, K-98). |
| Üye müşteri | `Customer` † | Hesabı olan alıcı (K-06). "Üye" ve "müşteri" **ayrı terimler olarak kullanılmaz** — tek terim budur (K-17). |
| Firma yöneticisi | `Admin` † | Firmanın panel kullanıcısı. Yönetim tarafı **tek roldür ve çoklu kullanıcıya açıktır** (K-06). |
| Tüketici | `Consumer` † | Alıcının **hukuki sıfatı**: kişisel ihtiyacı için alan gerçek kişi. Ayrı bir aktör değildir — MVP'de alıcı istisnasız tüketicidir ve 6502 sayılı Kanun'un korumaları her siparişte uygulanır (K-07). |

**Hesap ve oturum**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Oturum | `Session` † | Kullanıcının giriş yapmış hâli. Giriş ekranındaki "beni hatırla" seçimine göre **kısa** veya **uzun ömürlü** açılır; şifre sıfırlandığında hesabın tüm oturumları düşer (K-106, K-108). |
| Sosyal giriş | `SocialLogin` † | Kullanıcının bir kimlik sağlayıcısı üzerinden giriş yapması. MVP'de tek sağlayıcı **Google**'dır; Facebook kapsam dışındadır (K-103). Sağlayıcının **doğrulanmış** verdiği e-posta mevcut bir hesabınkiyle eşleşirse aynı hesaba bağlanır (K-104). |
| Adres defteri | `AddressBook` † | Üye müşterinin kaydettiği ve adlandırdığı adreslerin listesi (ör. Ev, İş). **Misafir alıcının adres defteri yoktur** — her siparişte adresini yazar (K-97, K-111). |
| Teslimat adresi | `ShippingAddress` † | Siparişin gönderileceği adres. Sipariş anında **siparişin içine donar**; defterdeki sonraki değişiklik veya silme geçmiş siparişe dokunmaz (K-112, K-113). |
| Fatura adresi | `BillingAddress` † | Faturanın kesileceği adres. Varsayılan olarak teslimat adresiyle aynıdır; kullanıcı farklı bir adres seçebilir ve o da sipariş anında donar (K-112, K-113). |

**Sepet**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Sepet | `Cart` | Müşterinin satın almaya aday kalemlerini tuttuğu liste. **Üyenin sepeti hesabında yaşar** ve her cihazda aynıdır; misafirin sepeti bulunduğu tarayıcıya bağlıdır. Kendiliğinden boşalmaz — misafirinki tarayıcının verileri silinene, üyeninki hesap durdukça yaşar. Ödeme başarılı olduğunda siparişe giren kalemler sepetten çıkar (K-123, K-126, K-127). |
| Sepet kalemi | `CartItem` | Sepetteki bir varyant ve adedi. **Canlıdır:** fiyatı ve satın alınabilirliği güncel üründen okunur — sipariş kalemi ise sipariş anında donar (K-80). Varyantın sepete **ilk eklendiği** andaki birim fiyatını referans olarak hatırlar; güncel fiyat bundan farklıysa satırda *"Sepete eklediğinden beri fiyatı değişti"* yazar, eski fiyat ve değişimin yönü gösterilmez (K-131). |

**Sipariş**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Sipariş | `Order` † | Müşterinin onayladığı ve ödeme sürecine giren alışveriş. Onaylandığı anda oluşur, bir sipariş numarası alır ve **iki eksende** durum taşır: sevkiyat ve ödeme. Kalemleri, adresleri, firmanın o günkü kimliği ve onaylanan sözleşme sürümleri oluştuğu anda içine donar (K-77, K-80, K-113, K-170, K-186, K-189, K-190). |
| Sipariş kalemi | `OrderItem` † | Siparişteki bir varyant, adedi ve donmuş fiyat bilgisi — birim fiyat, KDV, indirim, kuponun payı. Sepet kaleminin aksine **sipariş anında donar** (K-77, K-78, K-80). İptal ve iade kalem düzeyinde işler (K-178); karışık siparişte dijital ve hizmet kalemi kendi teslim işaretini taşır (K-179). |
| Sipariş numarası | `OrderNumber` † | Siparişin **tahmin edilemez** kimliği: yıl + rastgele blok (ör. `2026-7K4M9P`). Sipariş oluştuğunda verilir ve iptal edilse bile tekrar kullanılmaz; **fatura numarası değildir** (K-185, K-186). |

**Sipariş durumu — sevkiyat ekseni**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Sipariş durumu | `OrderStatus` | Siparişin sevkiyat ekseni. Ödeme ekseninden bağımsızdır; tek bağ, "Hazırlanıyor"a ancak ödeme "Ödendi" olduğunda geçilmesidir. Geçişler beyaz listeyle tanımlıdır, listede olmayan her geçiş yasaktır (K-170, K-173, K-224). |
| Alındı | `Placed` | Siparişin oluştuğu andaki durum. Ödeme onaylanana kadar sürer; hizmet siparişinde firmanın tamamlama işaretine kadar (K-171, K-175). |
| Hazırlanıyor | `Preparing` | Ödemesi onaylanmış fiziksel siparişin firma tarafından hazırlandığı durum. Kargoya verme süresi bu anda başlar (K-168, K-171, K-173). |
| Kargoya verildi | `Shipped` | Takip numarasıyla kargoya teslim edilmiş sipariş. Müşterinin iptal yolu bu anda kapanır (K-141, K-171, K-196). |
| Teslim edildi | `Delivered` | **Terminal.** Fiziksel siparişte kargonun teslimi; yalnız dijital siparişte ödeme onayı; yalnız hizmet siparişinde firmanın tamamlama işareti (K-171, K-174, K-175, K-223). |
| Teslim edilemedi | `DeliveryFailed` | Kargonun ulaştıramayıp firmaya geri döndürdüğü sipariş. **Terminal değildir** — yeniden gönderilebilir ya da iptal edilebilir (K-176). |
| İptal edildi | `Cancelled` | **Terminal.** Teslimattan önce müşterinin, firmanın ya da ödeme süresinin dolmasının kapattığı sipariş (K-171, K-180, K-196, K-199). |

**Ödeme**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Ödeme yöntemi | `PaymentMethod` † | Müşterinin ödeme adımında seçtiği yol. MVP'de **iki** yöntem vardır: kart ve havale/EFT. Kapıda ödeme yoktur; taksiti ürün bilmez, sağlayıcının kart ekranında kalır (K-12, K-13, K-159, K-160, K-164). |
| Havale/EFT | `BankTransfer` † | Müşterinin firmanın IBAN'ına elle gönderdiği ödeme. Firma panelden açar veya kapatır, açıkken IBAN zorunludur; ödemeyi firma "ödendi" işaretleyerek onaylar ve sistem gelen tutarı sormaz (K-159, K-161, K-169). |
| Ödeme süresi | `PaymentWindow` † | Onaylanan siparişin ödenmesi için tanınan süre; stok ayırma bu süre boyunca sürer. Kartta dakikalarla ölçülen bir sistem sabitidir, havalede günlerle ölçülen ve üst çiti olan bir firma ayarıdır (K-128, K-165, K-166). |
| Ödeme durumu | `PaymentStatus` | Siparişin ödeme ekseni. Geri ödeme bu eksende yaşar, sevkiyat eksenini değiştirmez (K-170, K-223). |
| Bekliyor | `Pending` | Ödemesi henüz gelmemiş sipariş (K-172). |
| Ödendi | `Paid` | Ödemesi onaylanmış sipariş: kartta sağlayıcının başarı bildirimi, havalede firmanın işareti (K-172, K-173). |
| Başarısız | `Failed` | **Terminal.** Ödeme süresi dolmuş ya da kart ödemesi başarısız olmuş sipariş. Başarısız bir siparişe sonradan gelen ödeme işlenmez (K-172, K-180, K-183). |
| Kısmen geri ödendi | `PartiallyRefunded` | Kalemlerinin bir kısmının parası müşteriye dönmüş sipariş. Terminal değildir (K-222, K-223). |
| Geri ödendi | `Refunded` | **Terminal.** Parasının tamamı müşteriye dönmüş sipariş (K-172, K-222, K-223). |

**İptal, cayma ve iade**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| İptal | `Cancellation` † | Teslimattan **önce** siparişin ya da bir kalemin kapatılması. Müşteri "Kargoya verildi"ye kadar kendi iptal eder; firma sebep seçerek iptal eder (K-195, K-196, K-197, K-199). |
| Cayma | `Withdrawal` † | Tüketicinin teslimattan **sonra**, sebep göstermeden sözleşmeden dönmesi. Pencere teslimden itibaren 14 gündür; dijital üründe ön onayla, hizmette ifa tamamlanınca, istisna işaretli üründe hiç yoktur (K-195, K-203…K-206). |
| İade | `Return` † | Cayma ya da ayıp nedeniyle **malın** firmaya geri gönderilmesi. Kargo bedeli firmaya aittir; mal stoğa firmanın kontrolünden sonra döner (K-210, K-211, K-215). |
| Geri ödeme | `Refund` † | **Paranın** müşteriye dönmesi — ödemenin geldiği yoldan: kartta karta, havalede müşterinin IBAN'ına. Kayıttaki "para iadesi" ifadesi bu terimi anlatır; "iade" tek başına yalnız malın dönüşüdür (K-209, K-214). |
| Ayıp talebi | `DefectClaim` † | Müşterinin sipariş sayfasından bildirdiği **sebepli** sorun. Caymadan ayrı bir yoldur ve yasal süre boyunca açıktır; iki durumu vardır: Açık, Çözüldü (K-226, K-227, K-228). |
| Ayıp talebi durumu | `DefectClaimStatus` † | Ayıp talebinin iki değerli durumu. Talebi Çözüldü'ye firma işaretler; seçimlik hakların yürütümü sistem dışındadır (K-228). |
| Açık | `Open` † | Firmanın henüz çözmediği ayıp talebi; talep bu durumda doğar (K-228). |
| Çözüldü | `Resolved` † | Firmanın çözüldü olarak işaretlediği ayıp talebi (K-228). |

**Yasal metinler**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Ön Bilgilendirme Formu | `PreInformationForm` † | Sipariş onayından önce gösterilen ve **ayrı bir kutuyla** teyit edilen yasal form; içeriği onay özetiyle birebir aynıdır (K-188, K-191). |
| Mesafeli Satış Sözleşmesi | `DistanceSalesContract` † | Sipariş onayında ayrı bir kutuyla kabul edilen sözleşme. Onaylanan sürüm ve firmanın o günkü kimliği siparişe donar (K-188, K-189, K-190). |

**Kurumsal içerik**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Kurumsal içerik | `Content` | Firmanın tanıtım tarafını oluşturan kayıtların ortak adı: Hakkımızda, hizmet tanıtımı, referans iş, sık sorulan soru, şube, genel sayfa ve duyuru. Her tipin düzeni sabittir — sayfa kurucu yoktur, firma yalnız içerik girer. Her kayıt kendi yayın durumunu taşır: **Taslak** ya da **Yayında**; içerikte arşiv ve sürüm geçmişi yoktur (K-237, K-238, K-251, K-273, K-276). |
| Hakkımızda | `AboutPage` | **Tekil** kayıt: kısa tanıtım, uzun metin ve görseller. Ana sayfanın kurumsal bloğu kısa tanıtımı ve ana görseli gösterir; Hakkımızda boşken ya da yayında değilken blok marka adını ve logoyu gösterir. Silinmez, yalnız taslağa alınır (K-239, K-250, K-277). |
| Hizmet tanıtımı | `Offering` | Firmanın yaptığı bir işin **fiyatsız** tanıtımı: ad, kısa açıklama, metin, görseller ve kendi sayfası. Satın alınmaz; sayfasındaki "Bize ulaşın" düğmesi iletişim formuna götürür. Satılan **Hizmet** ürün tipinden ayrıdır; ikisi içerik–ürün bağıyla birbirine bağlanabilir (K-240, K-245). |
| Referans iş | `PortfolioItem` | Firmanın yaptığı bir işi anlatan kayıt: başlık, kısa açıklama, metin, görseller ve kendi sayfası. Katalogdan ürün bağlayabilir. **Referans fiyat** ile ilgisi yoktur (K-241, K-245). |
| Sık sorulan soru | `FaqItem` | Soru + cevap. Tümü tek bir SSS sayfasında, firmanın verdiği sırayla listelenir; başlıklara bölünmez (K-242, K-247). |
| Şube | `Branch` | Firmanın ziyaret edilebilir bir yeri: ad ve adres zorunlu (il kapalı listeden); telefon, çalışma saatleri, görsel ve harita bağlantısı isteğe bağlı. İletişim sayfasında listelenir. **Teslim noktası değildir** (K-134, K-243). |
| Genel sayfa | `CustomPage` | Hiçbir hazır tipe uymayan içerik için başlık, metin ve görsellerden oluşan, düzeni sabit sayfa; altbilgide listelenir, istenirse menüde gösterilir. **Yasal metinler genel sayfa değildir** (K-237, K-244, K-248). |
| Duyuru | `Announcement` | Sitenin her sayfasının en üstünde görünen tek satırlık metin ve isteğe bağlı bağlantı; aynı anda tek duyuru vardır. Başlangıç ve bitiş tarihi girilirse yalnız o aralıkta görünür (K-269). |
| Sosyal medya bağlantısı | `SocialLink` | Firmanın kapalı bir platform listesindeki hesabına giden bağlantı; WhatsApp numarası da bu gruptadır. Firma kimliğinin parçası değildir: zorunlu değildir, siparişe donmaz. Sitede gömülü gönderi akışı yoktur (K-249). |

**Marka kimliği**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Marka kimliği | `Branding` | Firmanın vitrinde ve e-postalarda görünen dört ayarı: marka adı, logo, site simgesi, marka rengi. Yazı tipi, düzen ve diğer renkler üründedir; firmanın siteye kod eklemesi yoktur. Değişiklikleri işlem izine yazılır ama siparişe donmaz (K-258, K-263, K-265). |
| Marka adı | `BrandName` | Sitenin üstünde (logo yoksa), tarayıcı sekmesinde ve e-postalarda görünen ad — ör. "Yılmaz Mobilya". Zorunludur; girilene kadar site alan adını gösterir (K-260). |
| Unvan | `LegalName` | Firmanın yasal adı — tüzel kişide ticaret unvanı (ör. "Yılmaz Mobilya San. ve Tic. Ltd. Şti."), şahıs işletmesinde işletme sahibinin adı-soyadı. Yasal bilgilerde ve sözleşmede görünür; sipariş anında siparişe donar (K-14, K-190, K-260). |
| Logo | `Logo` | Firmanın tek logosu; sitenin her yerinde ve e-postalarda aynısı kullanılır. **Zorunlu değildir** — yoksa marka adı yazıyla gösterilir (K-259). |
| Site simgesi | `Favicon` | Tarayıcı sekmesindeki ikon. İsteğe bağlıdır; yüklenmezse marka adının baş harfi marka rengi üzerinde gösterilir — firmanın sitesinde Shopfolio simgesi çıkmaz (K-261). |
| Marka rengi | `BrandColor` | Firmanın seçtiği tek renk; düğme, bağlantı ve vurgulara uygulanır. Üstündeki yazının rengini sistem seçer; hata, uyarı, "Tükendi" ve indirim gibi anlam taşıyan renkler ondan bağımsızdır (K-258, K-262). |

> **Kapsam notu (K-84):** "Ürün" terimi, firmanın kataloğa koyduğu her şeyi kapsar; **sistem ne satıldığını denetlemez.** Ek akış gerektiren ürün türleri — alkol, tütün, ilaç ve reçeteli ürünler, silah — MVP kapsamı dışındadır (`10 §3`). Engelleme mekanizması, yasaklı kategori listesi ve ürün başına mevzuat belgesi alanı **yoktur**; mevzuata uygunluk firmanın yükümlülüğüdür (K-09, K-12, K-14).

> **Ölçek notu (K-89):** `01 §3.1`'deki "birkaç yüz ürüne kadar katalog, tek depo" ifadesi bir **kabuldür ve `10 §4`'te yaşar** — kural değildir. Sistem hiçbir yerde ölçek sınırı uygulamaz: ürün adedi, boyut başına seçenek değeri sayısı ve ürün başına varyant adedi tavansızdır. K-40'ın **iki seçenek boyutu** sınırı bundan ayrıdır ve geçerliliğini korur.

### 1.3 Aktörler

Uygulamada **dört aktör** vardır: ziyaretçi · misafir alıcı · üye müşteri · firma yöneticisi (K-06, K-97). Adları, kod karşılıkları ve tanımları §1.2'dedir; her aktörün **neden kullandığı ve neden geri döndüğü** `01 §3.2`'dedir ve burada tekrarlanmaz.

Bu dokümanın iş kurallarının dayandığı üç aktör kuralı:

1. **Yönetim tarafı tek roldür, çoklu kullanıcıya açıktır.** Firma birden fazla yönetici hesabı açabilir; hepsi aynı yetkiye sahiptir. Yetki matrisi MVP'de **yoktur**. Ayrı hesaplar korunduğu için "kim ne değiştirdi" izlenebilirliği kaybolmaz (K-06). **Kimlik doğrulama rejimi müşteri tarafıyla aynıdır:** yönetici hesapları da doğrulama kapısına, oturum kurallarına, hassas işlemde yeniden doğrulamaya ve şifre politikasına tabidir; `B8-02` bu tabanı yalnız **sıkılaştırabilir**, gevşetemez (K-122).
2. **Alıcı istisnasız tüketicidir.** Ticari/kurumsal alıcıya özel hukuki rejim yoktur; cayma hakkı, ön bilgilendirme ve mesafeli satış sözleşmesi dallanmadan uygulanır (K-07).
3. **Sipariş vermek için üyelik zorunlu değildir.** Ziyaretçi hesap açmadan sipariş verebilir ve siparişini **sipariş numarası + e-posta** ile takip eder; üyelik kaldırılmamıştır, yalnız zorunluluğu kalkmıştır (K-97). Üyeliğin taşıdığı değer süreklilikte toplanır — adres defteri (K-111), sipariş geçmişi ve "beni hatırla" (K-108). Doğrulanmış e-postayla hesap açıldığında o e-postaya ait geçmiş misafir siparişleri hesaba düşer (K-98); silinmiş bir hesabın siparişleri **düşmez** (K-117).

**Aktör olmayanlar — bilinçli kararlar:**

- **Platform operatörü uygulama içi aktör değildir.** Kurulum bir deploy işidir; uygulamaya operatör paneli koymak, K-01 ile elenen kiracılığı arka kapıdan geri getirirdi (K-06).

*Kaynak: K-17 (sözlük kuralı ve adlandırma konvansiyonu) · K-06 (aktör envanteri) · K-07 (alıcının hukuki sıfatı) · K-01 · K-09 · K-12 · K-14 · K-39 · K-40 · K-41 · K-42 · K-43 · K-45 · K-50 · K-54 · K-55 · K-56 · K-57 · K-58 · K-59 · K-63 · K-64 · K-65 · K-66 · K-67 · K-70 · K-71 · K-73 · K-74 · K-75 · K-81 · K-82 · K-83 · K-84 · K-85 · K-88 · K-89 · K-90 · K-96 · K-97 · K-98 · K-100 · K-103 · K-104 · K-106 · K-108 · K-111 · K-112 · K-113 · K-122 · K-237 · K-238 · K-239 · K-240 · K-241 · K-242 · K-243 · K-244 · K-245 · K-247 · K-248 · K-249 · K-250 · K-251 · K-253 · K-258 · K-259 · K-260 · K-261 · K-262 · K-263 · K-265 · K-266 · K-269 · K-272 · K-273 · K-274 · K-275 · K-276 · K-277 · K-279.*

---

## 2. Temel akış

> **Ne yazılır:** Ürünün ana iş akışı, uçtan uca. Adım adım, dallanmasız. Detaylı akışlar `03_USER_FLOWS.md`'de.

Ürünün **sekiz akışı** vardır ve dört aktörün üzerine kuruludur: dördü müşteri tarafında, dördü firma tarafında (K-221, K-06, K-97). Satış **doğrudandır** — yayındaki her ürünün fiyatı vardır ve satın alınabilir; teklif ya da talep hattı yoktur (K-11). Üç ürün tipi — fiziksel ürün, dijital ürün, hizmet — aynı kataloğa girer, aynı sepetten geçer ve aynı şekilde satın alınır (K-81). Bu bölüm akışların listesini ve omurgasını taşır; adımların ayrıntısı ve dalları `03`'ün işidir (K-221).

### 2.1 Akış envanteri

| # | Akış | Taraf | Omurga | Kaynak |
|---|---|---|---|---|
| 1 | Satın alma | Müşteri | Vitrin → sepet → ödeme adımı → ödeme → sipariş sayfası | K-221 |
| 2 | Sipariş takibi | Müşteri | Sipariş sayfasına üç yoldan biriyle girilir: üye sipariş geçmişinden, misafir alıcı sipariş numarası + e-postayla, herkes sipariş e-postasındaki bağlantıyla | K-187, K-221 |
| 3 | İptal, cayma ve iade | Müşteri | Teslimattan önce iptal, teslimattan sonra cayma; sebepli sorun için üçüncü yol ayıp talebidir (§7) | K-195, K-226, K-221 |
| 4 | Üyelik | Müşteri | Kayıt → e-posta doğrulama → giriş → hesap yönetimi → hesap silme | K-101, K-221 |
| 5 | Katalog yönetimi | Firma | Ürün, varyant, stok, fiyat, indirim, kupon | K-221 |
| 6 | Sipariş yürütümü | Firma | Havale onayı → hazırlık → kargoya verme → teslim; hizmet tamamlama; iptal; iade teslim alma → para iadesi | K-221 |
| 7 | Kurumsal içerik | Firma | Tip seçimi → taslak → önizleme → yayın → ana sayfada gösterme → düzenleme → taslağa alma ya da silme; iki kolu marka ayarları ve duyurudur (§2.3) | K-285 |
| 8 | Mağaza ayarları | Firma | Kimlik, ödeme yöntemleri, kargo, eşikler | K-221 |

### 2.2 Uçtan uca ana akış

Ana akış, fiziksel bir ürünün vitrinden teslimata kadar yolculuğudur — müşterinin 1. akışı ile firmanın 6. akışının birleşimi. Dijital ürün ve hizmet aynı akıştan geçer, yalnız teslim adımında ayrılır: dijital kalem ödeme onaylandığı anda indirilebilir olur, hizmet kalemi firma "tamamlandı" işaretlediğinde teslim edilmiş olur (K-174, K-175, K-179).

1. **Ziyaretçi ürünü bulur ve varyantını seçer** — kategori gezinmesiyle ya da ürün adında aramayla (§3.3–§3.5).
2. **Varyantı adediyle sepete ekler.** Sepet canlıdır; fiyatı ve satın alınabilirliği güncel üründen okur (§3.16).
3. **Ödeme adımına geçer.** Üye adres defterinden seçer, misafir adresini ve e-postasını yazar; teslimat ve fatura adresi, kupon ve ödeme yöntemi burada girilir (§3.10, §3.14, §3.21).
4. **Onay özetini ve Ön Bilgilendirme Formu'nu görür, iki onay kutusunu işaretler ve siparişi onaylar** (§3.24).
5. **Sipariş oluşur.** Sepet onay anında yeniden değerlendirilir; özet değişmemişse sipariş numarası verilir, kalemler ve adresler donar, stok ayrılır. Sipariş **Alındı**, ödeme **Bekliyor** durumundadır (§3.17, §3.23, §5.3).
6. **Müşteri öder ve ödeme onaylanır** — kartta 3D Secure'dan geçen ödemenin sağlayıcıdan gelen başarı bildirimiyle, havale/EFT'de firmanın panelden "ödendi" işaretiyle. Ödeme **Ödendi**, sipariş **Hazırlanıyor** olur; ayrılan stok kesin düşer, sepet boşalır ve kargoya verme süresi başlar (§3.17, §3.21, §5.6).
7. **Firma siparişi hazırlar ve kargoya verir** — kargo şirketi ile takip numarası ya da "kendi aracımızla teslim" beyanı girilir; sipariş **Kargoya verildi** olur (§3.20).
8. **Mal müşteriye ulaşır ve sipariş Teslim edildi olur.** Bu geçişi kimin ve hangi kayıtla yaptığı karar kaydında yoktur — **açık: A-06** (§5.4).

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

Akışın iki kolu vardır: **marka ayarları** (§3.29) ve **duyuru** (§3.27.21). Kuralları §3.27–§3.29'dadır.

*Kaynak: K-221 (temel akış omurgası) · K-285 (yedinci akışın adımları) · K-11 (doğrudan satış) · K-159 (iki ödeme yolu) · K-81 · K-06 · K-97 · K-101 · K-173 · K-174 · K-175 · K-179 · K-187 · K-195 · K-226.*

## 3. İş kuralları

> **Ne yazılır:** Her kural ayrı madde. Kuralın **koşulu**, **sonucu** ve **istisnası**. Her kural bir sonraki dokümanda karşılığını bulacak şekilde somut olmalı.

**Kuralların biçimi.** Her kural bir numara (`§3.6.2`) ve kalın bir başlık cümlesi taşır; başlığı izleyen cümleler kuralın koşulunu, sonucunu ve — varsa — **istisnasını** yazar. Kaynak karar satırları parantez içindedir.

- **Sayısal değerler burada yazılmaz.** "Değeri §11'dedir" diyen kural değerini `B9-08` parametre envanterinden alır; §11 **Blok 9 kapandığında** yazılır.
- **Başka bölümde yaşayan kurallar tekrarlanmaz:** durum makineleri §5'te, hata ve istisna senaryolarının envanteri §6'da, iptal, cayma, iade ve ayıp talebi §7'dedir.
- Bir kuralın biçimini, metnini ya da ekrandaki yerini `04`'e bırakan ifade, **arayüz** kararının o dokümanda alınacağını söyler; kuralın kendisi burada tamdır.

| Gruplar | Alan |
|---|---|
| §3.1–§3.2 | Firma, satış kapısı ve satış modeli |
| §3.3–§3.12 | Katalog |
| §3.13–§3.15 | Hesap |
| §3.16–§3.26 | Sepet, sipariş, ödeme ve teslim |
| §3.27–§3.30 | Kurumsal içerik, marka ve site |
| §3.31 | Panel |

### 3.1 Firma, kurulum ve satış kapısı

**3.1.1 Uygulama kurulum başına tek bir firmaya hizmet eder.** Firma ekleme, firma seçme ve firmalar arası geçiş yoktur; ikinci firma ikinci kurulumdur. Firmanın kimlik kaydı silinemez, yalnız düzenlenir (K-01, K-02, K-09).

**3.1.2 Firmanın kimliği panelden yönetilir; altyapısı kurulumdan gelir.** Panelden: unvan, vergi kimlik ya da MERSİS numarası, adres, iletişim bilgileri, logo ve marka ayarları. Kurulumdan: alan adı, e-posta gönderim kimliği ve ödeme sağlayıcı anahtarları — yani sır ve altyapı. Kimlik alanları zorunludur ve boşaltılamaz; her değişiklik kimin ve ne zaman yaptığıyla işlem izine yazılır (K-08). **İstisna:** logo zorunlu değildir (§3.29.2, K-259).

**3.1.3 Firma tipi seçilir ve zorunlu kimlik alanları tipe göre değişir.** Şahıs işletmesi: ad-soyad ve vergi kimlik numarası. Tüzel kişi: ticaret unvanı, MERSİS numarası ve ticaret sicil numarası. İki tipte de adres, telefon ve e-posta zorunludur. Tip sonradan değiştirilebilir; değiştiğinde satış kapısı (§3.1.5) yeni tipin setini denetler — kapı yalnız kurulum anında değil **sürekli** işler (K-14).

**3.1.4 Yasal kimlik bilgileri sitede sürekli erişilebilir durur;** yerleşimi `04`'ün işidir. ETBİS kaydı firmanın yükümlülüğüdür: ürün kayıt yapmaz, kayıt bilgisini taşır ve doğrulama bandını gösterir (K-14).

**3.1.5 Satış yalnız üç koşul birlikte sağlandığında açıktır:** (1) seçilen firma tipinin zorunlu kimlik alanları doludur (K-09, K-14); (2) en az bir ödeme yöntemi açıktır (K-163); (3) "satışı geçici olarak kapat" anahtarı kapalıdır (K-235). Koşullardan biri sağlanmıyorsa sepete ekleme ve ödeme kapalıdır; vitrin ve kurumsal içerik yayında kalır, ürünler görünür ve ziyaretçi satışın kapalı olduğunu görür. Üç koşul aynı kapı mekanizmasıdır (K-09, K-235). Kurumsal taraf kapıya bağlı değildir — kimlik eksikken de yayınlanabilir (K-09).

**3.1.6 Firma satışı panelden geçici olarak kapatabilir.** Anahtar açıkken açık siparişler etkilenmez — firma onları yürütmeye devam eder, havale onayı dahil. Müşterinin sepeti korunur ve içeriği görünür; yalnız ödeme adımına geçilemez. **Bakım modu — sitenin tamamen kapanması — yoktur** (K-235).

### 3.2 Satış modeli ve ürün tipleri

**3.2.1 Satış yalnız doğrudandır.** Yayındaki her ürünün fiyatı vardır ve satın alınabilir; "fiyat sorunuz" diye bir ürün durumu, teklif ya da talep hattı yoktur. Fiyat sorusu olan ziyaretçi iletişim formunu (`B8-01`) kullanır; bu ticari bir hat değildir. Randevu ve rezervasyon modeli yoktur (K-11).

**3.2.2 Katalog üç tipte ürün taşır: fiziksel ürün, dijital ürün ve hizmet.** Fiziksel ürün kargoyla teslim edilen maldır; dijital ürün indirilebilir dosyadır; hizmet sabit fiyatlı ve randevusuzdur. Üçü de aynı kataloğa girer, aynı sepetten geçer ve aynı şekilde satın alınır (K-81).

**3.2.3 Tip ürün düzleminde yaşar ve üç değerli kapalı bir alandır.** Ürünün bütün varyantları aynı tiptedir; varyant tipi devralır, kendi tipini taşımaz (K-82).

Tipe göre ayrılan kurallar kendi gruplarındadır: stok (§3.6.2), dijital ürün (§3.12), teslimat adresi (§3.14.4), asgari sipariş tutarı (§3.18.3), kargo ücreti (§3.19.1), teslim yolu (§3.20.9), iptal ve cayma (§7).

### 3.3 Ürün, varyant ve seçenek

**3.3.1 Satılan birim varyanttır; ürün vitrin ve katalog birimidir.** Stok ve fiyat varyantta yaşar. Seçeneği olmayan ürünün de tek bir varyantı vardır ve ziyaretçi bunu fark etmez. Ziyaretçi vitrinde ürün başına **tek kart** görür ve varyant seçimini kartın içinde yapar (K-39).

**3.3.2 Bir ürün en fazla iki seçenek boyutu taşır** (ör. renk × beden); üç ve üzeri boyut yoktur (K-40).

**3.3.3 İki boyutun birleşim matrisinin tam olması gerekmez.** Firma yalnız gerçekten var olan kombinasyonları varyant olarak açar; var olmayan kombinasyon vitrinde görünür ama seçilemez — tükenmiş varyantın kalıbıyla aynı, sebebi farklıdır (K-87, K-51).

**3.3.4 Her varyant zorunlu ve tekil bir stok kodu (`SKU`) taşır;** tip ayrımı yoktur — dijital ve hizmet varyantı da kod taşır. Kodun panelde önceden doldurulması ve düzenlenebilirliği `04`'ün işidir (K-88).

**3.3.5 Ürün başına asgari adet yoktur.** Asgari adetle satmak isteyen firma ürünü paket olarak tanımlar ("10'lu paket" varyantı); paket kendi fiyatını ve kendi stoğunu taşır (K-158).

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

**3.6.3 Tükenmiş varyant seçenek listesinde "Tükendi" işaretiyle görünür ve seçilemez;** gizlenmez (K-51). Satın alınabilirlik ayrılmış adetler düşülerek hesaplanır: başka bir müşteri ödeme ekranındayken son parça kısa süreliğine "Tükendi" görünebilir ve ödeme gerçekleşmezse geri gelir (K-128).

**3.6.4 Bütün varyantları tükenmiş ürün vitrinde kalır,** "Tükendi" olarak işaretlenir ve sepete eklenemez (K-52).

**3.6.5 Stok adedi ziyaretçiye hiçbir yerde gösterilmez.** Ziyaretçi yalnız iki durum görür: satın alınabilir ya da "Tükendi". "Son 2 adet" gibi eşik uyarısı yoktur (K-53). Sepete stoktan fazla adet eklenmek istendiğinde de adet söylenmez (§3.16.8).

**3.6.6 İptal edilen kalemin adedi stoğa kendiliğinden döner; iade edilen fiziksel kalemi firma kontrol edip panelden ekler** (§7.2.6, §7.4.7; K-201, K-215).

### 3.7 Ürünün yayın durumu, arşiv ve silme

**3.7.1 Ürün ve varyant yayın durumunu ayrı ayrı taşır: Taslak, Yayında, Arşiv.** Durumların tanımı ve geçişleri §5.1'dedir. Zamanlanmış (ileri tarihli) yayın yoktur (K-54, K-55).

**3.7.2 Arşiv silme değildir.** Arşivlenen ürünün kaydı korunur, yalnız vitrinden çıkar (K-54).

**3.7.3 Bir ürün yayına ancak adı, ana kategorisi ve tipi girilmişse ve arşivlenmemiş en az bir varyantı varsa alınır.** Açıklama ve görsel zorunlu değildir; KDV oranının varsayılanı ayardan geldiği için kapıyı ayrıca tutmaz (K-86, K-45, K-55, K-58). Yayındaki her varyantın fiyatı vardır (K-11, K-39).

**3.7.4 Dijital ürünün yayın kapısı bir koşul daha taşır:** yayındaki her varyantı bir dosyaya — kendi dosyasına ya da ürününkine — bağlı olmalıdır; panel dosyası eksik varyantı gösterir. Yayındaki bir dijital ürünün herhangi bir varyantını dosyasız bırakacak dosya silme işlemi **engellenir**; firma önce yeni dosyayı yükler ya da o varyantı yayından çeker. Fiziksel ürün ve hizmetin kapısı değişmez (K-145, K-144).

**3.7.5 Taslak ürünün adresi ziyaretçiye "sayfa bulunamadı" döner; giriş yapmış firma yöneticisine ürün sayfasını "Taslak" bandıyla gösterir.** Taslak varyant, yayındaki bir ürünün seçenek listesinde ziyaretçiye görünmez, yalnız yönetici önizlemesinde görünür — tükenmiş varyantın görünür kalma kuralı (§3.6.3) yalnız stok durumu içindir (K-57, K-281).

**3.7.6 Arşivlenmiş ürünün adresi çalışmaya devam eder ve "Bu ürün artık satılmıyor" sayfası döner:** ürün adı, görseli ve durum bilgisi görünür; **fiyat ve sepete ekleme yoktur**. Ürün listelerde, kategori sayfalarında ve aramada görünmez; yalnız doğrudan adresle açılır (K-56, K-69). Arama motorlarına kapalıdır (§3.30.6).

**3.7.7 Yönetici ürünü kalıcı olarak silebilir.** Silinen ürün katalogdan tamamen kalkar ve adresi "sayfa bulunamadı" döner. Arşiv ile silme iki ayrı yoldur. Geçmiş siparişler silmeden etkilenmez — kalem kendi donmuş bilgisini taşır (K-85, K-77); dijital üründe dosyanın son hâli bu siparişler için saklanır (§3.12.7).

### 3.8 Fiyat ve KDV

**3.8.1 Fiyat varyantta yaşar ve KDV dahil girilir.** Yöneticinin yazdığı sayı ile ziyaretçinin gördüğü sayı birebir aynıdır; matrah ve KDV tutarı bu sayıdan geriye hesaplanır. Tüketiciye gösterilen fiyatın KDV dahil olması bir seçenek değil yasal kısıttır (K-39, K-58, K-60).

**3.8.2 KDV oranı ürün düzleminde tutulur ve varyantlar devralır;** varsayılanı ayardan gelir. Matrah ve KDV tutarı sipariş kaydında ayrışır (K-58, K-59).

**3.8.3 KDV kalem bazında hesaplanır ve kuruşa orada yuvarlanır.** Siparişin toplamı yuvarlanmış kalemlerin toplamıdır; müşterinin ödediği toplam KDV dahil kalem tutarlarının toplamıdır ve matrah + KDV'den yeniden türetilmez (K-61).

**3.8.4 Fiyatın hassasiyeti kuruştur — iki ondalık;** panel daha fazlasını kabul etmez (K-62).

### 3.9 İndirim ve referans fiyat

**3.9.1 İndirim ürün düzleminde yüzde olarak tanımlanır ve bütün varyantlara iner;** her varyant kendi fiyatı üzerinden indirilir (K-64, K-65).

**3.9.2 İndirim tarihlidir.** Başlangıç ve bitiş girilir; sistem indirimi kendisi başlatır ve bitirir (K-66).

**3.9.3 İndirim beyanı referans fiyata dayanır ve referansı sistem hesaplar;** yönetici giremez. Sistem her varyantın fiyat geçmişini tutar; referans, indirimin başladığı ana kadarki **30 gün** içinde o varyanta uygulanmış **en düşük** fiyattır. İndirimli fiyat da uygulanmış bir fiyattır ve geçmişe yazılır — art arda kampanyalarda referans kademeli düşer (K-63, K-64).

**3.9.4 30 günlük geçmişi olmayan üründe referans,** ürünün yayına girdiğinden beri uygulanmış en düşük fiyattır (K-67).

**3.9.5 İndirimli fiyat referansın altına inmiyorsa sistem indirimi kaydetmez** ve yöneticiyi sebebiyle birlikte uyarır (K-68).

**3.9.6 Beyan edilen referans fiyat sipariş kalemine donar** (§3.23.1, K-79).

### 3.10 Kupon

**3.10.1 Kupon, ödeme adımında girilen ve sepet toplamına inen bir koddur.** Kupon indirimli fiyattan iner ve indirimli ya da indirimsiz her kaleme aynı biçimde uygulanır — kalem ayrımı yoktur; payı kalemlere dağıtılır (§3.10.3). Örnek: %20 indirimli 960 TL'ye %10 kupon 864 TL verir (K-70, K-74, K-77).

**3.10.2 Kupon yüzde ya da sabit tutar olarak tanımlanır** (K-71).

**3.10.3 Sabit tutarlı kupon kalemlere tutarlarıyla orantılı dağıtılır;** bölünmeden kalan kuruş en büyük kaleme yazılır. Her kalemin kupon payı kaleme donar (K-72, K-77).

**3.10.4 Kupon sepet toplamını aşarsa reddedilir.** Sabit tutarlı kupon zorunlu bir **asgari sepet tutarı** taşır ve bu tutar kupon tutarının altına girilemez; yüzdesel kuponda asgari tutar isteğe bağlıdır (K-73). Asgari sepet tutarı yalnız siparişe giren kalemlere bakar (K-130).

**3.10.5 Kuponun sınırı tarih aralığı ve toplam kullanım adedidir.** Tarih aralığının dışında ya da kullanım hakkı dolmuş kupon kabul edilmez. Kişiye özel kod, toplam kullanım adedi **1** olan koddur (K-75).

**3.10.6 Kuponun kullanım hakkı stok gibi ayrılır:** sipariş onaylandığında ayrılır, ödeme başarılı olduğunda **kullanılmış** sayılır, ödeme süresi dolar ya da ödeme başarısız olursa geri döner — son kullanım hakkını iki müşterinin birden alması böylece kapanır (K-76, K-128). İptal ve iadede hakkın dönüşü §7'dedir (K-201, K-202, K-216).

**3.10.7 Kupon, ücretsiz kargo eşiğinin ve asgari sipariş tutarının tabanını düşürmez;** kupon ile ücretsiz kargo aynı siparişte birlikte geçerlidir (§3.19.4, §3.18.2; K-136, K-156).

### 3.11 Ürün görseli ve metni

**3.11.1 Görsel ürün düzleminde yaşar;** varyant isterse kendi görselini taşır, taşımıyorsa ürününkini devralır (K-90).

**3.11.2 Ayrı bir "ana görsel" işareti vardır ve galeri sırasından bağımsızdır;** vitrin kartında ve listede işaretli görsel çıkar (K-91). Galeri sırasını firma panelde elle belirler (K-94).

**3.11.3 Görsele üç kısıt uygulanır:** kabul edilen format listesi, tek dosya için boyut tavanı ve ürün başına görsel adedi tavanı; değerleri §11'dedir (K-92).

**3.11.4 Görselin alternatif metni isteğe bağlıdır ve geri düşüşlüdür:** firma yazarsa onun metni, yazmazsa ürün adı kullanılır; çıktı hiçbir zaman boş kalmaz (K-93).

**3.11.5 Ürün adına karakter tavanı konur; açıklama serbesttir.** Tavanın değeri §11'dedir (K-95).

**3.11.6 Ürün açıklaması ve kurumsal içeriğin uzun metinleri tek bir kapalı metin biçimi setini kullanır:** kalın, italik, madde listesi, bağlantı ve tek seviyeli ara başlık. Serbest editör ve ham HTML yoktur; metnin içine görsel eklenmez — görseller kaydın görsel alanındadır; setin dışındaki biçim metne yapıştırıldığında düşer. Setin geçerli olduğu alanlar: ürün açıklaması, Hakkımızda'nın uzun metni, hizmet tanıtımının ve referans işin metni, SSS cevabı, genel sayfanın metni (K-96, K-267).

**3.11.7 Garanti bilgisinin ayrı alanı yoktur.** Garanti süresi ve koşulları firmanın ürün açıklamasına yazdığı metindir. Garanti belgesi gerektiren üründe belge malla birlikte fiziksel olarak gönderilir; sistem garanti belgesi üretmez ve saklamaz (K-230).

### 3.12 Dijital ürün

**3.12.1 Dijital ürünü sistem teslim eder.** Firma dosyayı ürüne bir kez yükler. Ödeme başarılı olduğunda sipariş sayfasında dosyanın indirme düğmesi açılır ve müşteriye sipariş sayfasına giden bir e-posta gönderilir; müşteri dosyayı yeniden indirmek için sipariş sayfasına döner (K-143). Havale ile alınan dijital üründe teslim firmanın "ödendi" işaretine bağlıdır (K-174).

**3.12.2 Dosya ürün düzleminde yaşar; varyant isterse kendi dosyasını taşır, taşımıyorsa ürününkini devralır.** Her iki düzlemde de **tek dosya** vardır — birden çok dosya satan firma onları tek bir arşivde (ör. zip) toplar. Örnek: PDF ve EPUB varyantlı e-kitapta her varyant kendi dosyasını taşır; yalnız lisansla ayrılan varyantlar ürünün tek dosyasını devralır (K-144).

**3.12.3 Tek dosyanın boyut tavanı vardır;** değeri §11'dedir. Kabul edilen dosya türleri ve yüklenen dosyanın güvenliği `05`'in işidir (K-143).

**3.12.4 Dijital üründe adet her zaman 1'dir.** Adet seçici yoktur; bir sipariş kalemi dijital ürünün tek bir satışıdır. Aynı dijital varyantı sepete ikinci kez eklemeye çalışan müşteri "Bu ürün zaten sepetinde" mesajını görür. Aynı ürünün farklı dijital varyantları ayrı kalemlerdir — PDF ve EPUB aynı siparişte alınabilir (K-153).

**3.12.5 İndirmenin adet sınırı vardır, süre sınırı yoktur.** Müşteri dosyayı sipariş sayfasından belli sayıda indirebilir; hak **sipariş kalemi başına** sayılır ve değeri §11'dedir. Hakkı dolmayan müşteri sipariş sayfasına girebildiği sürece indirir (K-146). İndirmenin sayıldığı an `05`'in işidir (K-146).

**3.12.6 Hakkı dolan müşteri firmaya başvurur; firma paneldeki sipariş sayfasından o kalemin indirme hakkını yeniler ve sayaç sıfırlanır.** Müşteri dosyayı yine sipariş sayfasından indirir; misafir alıcıda da yol aynıdır (K-147). Yenileme talebinin iletildiği kanal `B8-01`'in işidir.

**3.12.7 Sipariş sayfası, kalemin bağlı olduğu varyantın o anki dosyasını verir.** Firma dosyayı değiştirdiğinde geçmiş alıcılar da yeni hâli indirir — düzeltme eski alıcılara kendiliğinden ulaşır. **Bu, kalemin dondurma kuralının (§3.23.1) tek istisnasıdır:** kalem ürünün tanımını ve tutarı dondurmaya devam eder, teslim edilen dosyanın kendisini dondurmaz. Ürün kalıcı silinirse ya da dosyası kaldırılıp yerine yenisi konmazsa geçmiş kalemler dosyanın son hâlini indirmeye devam eder (K-148).

**3.12.8 Dosya güncellemesi indirme hakkını değiştirmez;** yeni hâli indirmek de haktan bir indirme harcar. Hakkı dolan müşteri yeni hâle §3.12.6'nın yoluyla ulaşır (K-149).

**3.12.9 Daha önce alınmış dijital varyant yeniden satın alınabilir.** Üye müşteri onu sepete eklediğinde "Bu ürünü daha önce aldınız — sipariş sayfanızdan indirebilirsiniz" uyarısını görür ve isterse yine de satın alır; misafir alıcıda geçmiş bilinmediği için uyarı yoktur (K-236).

Dijital kalemin teslim hattı §5.7'de, iptal edilemezliği ve cayma istisnası §7'dedir (K-174, K-200, K-204).

### 3.13 Üyelik, e-posta doğrulama ve oturum

**3.13.1 Sipariş vermek için üyelik zorunlu değildir.** Ziyaretçi hesap açmadan sipariş verebilir ve siparişini sipariş numarası + e-posta ile takip eder; üyelik kaldırılmamıştır, yalnız zorunluluğu kalkmıştır (K-97).

**3.13.2 Hesap açıldığında, o e-postaya ait geçmiş misafir siparişleri hesaba düşer** ve "Siparişlerim"de görünür. Bağlanmanın tek dayanağı **doğrulanmış** e-postadır (K-98). **İstisna:** silinmiş bir hesabın siparişleri bağlanmaz (§3.15.3).

**3.13.3 Hesabı olan kişi giriş yapmadan misafir olarak sipariş verirse sipariş kabul edilir ve hesaba anında düşer;** giriş duvarı yoktur. Bağlanan sipariş tam bağlanır — yetki bakımından ikinci sınıf bir sipariş türü yoktur. Ödeme adımında "bu e-posta kayıtlı, giriş yaparsanız adresleriniz dolu gelir" hatırlatması gösterilir; biçimi `04`'ün işidir (K-99).

**3.13.4 E-posta doğrulaması sert bir kapıdır:** doğrulanmayan hesapla giriş yapılamaz ve sipariş verilemez. Tek tip hesap vardır — doğrulanmamış hesap bir kullanım durumu değil, bir bekleme aşamasıdır. Misafir siparişi bu kapının dışındadır (K-101).

**3.13.5 Doğrulanmamış hesap geçicidir.** Doğrulama bağlantısının bir ömrü vardır; süre dolduğunda kayıt silinir ve o e-posta yeniden kayda açılır. Süresi geçmiş bağlantıya tıklayan kullanıcı "bağlantı geçersiz, yeniden kayıt olun" mesajını görür. Sürenin değeri §11'dedir (K-102).

**3.13.6 Sosyal giriş yalnız Google ile yapılır;** Facebook ile giriş yoktur (K-103).

**3.13.7 Aynı e-posta iki giriş yolundan gelirse tek hesaba bağlanır.** Sağlayıcının **doğrulanmış** olarak verdiği e-posta mevcut bir hesabın e-postasıyla eşleşiyorsa kullanıcı doğrudan o hesaba girer ve hesap bundan sonra iki giriş yolunu da taşır. Sağlayıcı e-postayı doğrulanmamış verirse bağlama yapılmaz; kural sağlayıcıdan bağımsızdır (K-104).

**3.13.8 Google ile açılmış hesap şifresiz başlar ve sonradan şifre konabilir;** kullanıcı o günden sonra iki giriş yolunu da kullanır. Ayrı bir akış yoktur: şifresi olmayan hesap "şifremi unuttum" dediğinde sıfırlama akışı "şifre belirle" işlevi görür (K-121).

**3.13.9 Şifre sıfırlama bağlantısının bir ömrü vardır ve bağlantı tek kullanımlıktır.** Kullanılmış ya da süresi dolmuş bağlantı geçersizdir; kullanıcı yeni bir talep açar. Sürenin değeri §11'dedir (K-105).

**3.13.10 Şifre sıfırlandığında hesabın bütün cihazlardaki oturumları kapanır;** kullanıcı kendi cihazında da yeni şifreyle yeniden girer (K-106).

**3.13.11 Ekranlar hesabın varlığını iki farklı biçimde ele verir:** şifre sıfırlama ekranı **nötr** konuşur ("bu adres kayıtlıysa sıfırlama bağlantısını gönderdik"), kayıt ekranı **dürüst** konuşur ("bu e-posta zaten kayıtlı") (K-107).

**3.13.12 Oturum ömrünü kullanıcı seçer.** Giriş ekranında "beni hatırla" işaretlenirse uzun ömürlü, işaretlenmezse kısa ömürlü oturum açılır; iki sürenin değeri §11'dedir (K-108).

**3.13.13 Üç işlem, oturum açık olsa bile yeniden doğrulama ister:** şifre değiştirme, e-posta adresi değiştirme ve hesap silme. Başka hiçbir işlem istemez (K-109).

**3.13.14 Hesabın e-posta adresi, yeni adres doğrulanmadan değişmez.** Doğrulandıktan sonra §3.13.2'nin bağlama kuralı yeni adres için yeniden işler; eski adrese bağlı siparişler hesapta kalır, bağlanmış bir sipariş çözülmez (K-110).

**3.13.15 Şifre politikası uzunluk tabanlıdır.** Asgari bir uzunluk aranır (değeri §11'de); büyük harf, rakam ya da sembol dayatılmaz. Çok yaygın kullanılan şifreler reddedilir; bu liste ürünle birlikte gelir ve dış servise bağlanmaz (K-120).

**3.13.16 Bu grubun kimlik doğrulama kuralları firma yöneticisi hesaplarına da aynen uygulanır** — doğrulama kapısı (§3.13.4), oturum ömrü (§3.13.12), yeniden doğrulama (§3.13.13) ve şifre politikası (§3.13.15). `B8-02` bu tabanı yalnız **sıkılaştırabilir**, gevşetemez (K-122).

### 3.14 Adres

**3.14.1 Üye müşterinin adres defteri vardır.** Üye birden çok adres kaydeder ve adlandırır (ör. Ev, İş); siparişte defterinden seçer. **Misafir alıcının adres defteri yoktur** — her siparişte adresini yazar (K-111).

**3.14.2 Fatura adresi teslimat adresinden ayrı seçilebilir.** Varsayılan olarak aynıdır; kullanıcı "fatura adresim farklı" diyerek ikinci bir adres seçer (K-112).

**3.14.3 Adreste il serbest metin değildir;** 81 ilden oluşan kapalı listeden seçilir (K-133).

**3.14.4 Siparişin istediği adresler kalemlerin tipine bağlıdır.** Siparişte en az bir fiziksel kalem varsa teslimat ve fatura adresi istenir ve §3.14.2 işler. Yalnız dijital ve/veya hizmet kalemi taşıyan siparişte teslimat adresi alanları hiç gösterilmez; fatura adresi yasal fatura zorunluluğu nedeniyle istenir (K-194, K-143).

**3.14.5 Sipariş, adreslerini kendi içine dondurur;** adres defterindeki sonraki değişiklik ya da silme geçmiş siparişe dokunmaz (§3.23.3; K-113).

### 3.15 Hesabın silinmesi ve kişisel veri başvurusu

**3.15.1 Hesap silindiğinde hesap kapanır, sipariş kaydı yasal saklama süresi boyunca durur.** Giriş bilgileri, şifre, açık oturumlar, adres defteri ve sepet **hemen** silinir; siparişlerin içindeki kişisel veriler yasal saklama süresi dolduğunda imha edilir. Sürelerin kendisi `B8-10`'un işidir (K-115, K-123).

**3.15.2 Yürüyen sipariş hesap silmeyi engellemez.** Sipariş kendi kaydıyla yoluna devam eder; kullanıcı takibi, cayma ve iade taleplerini misafir yolundan — sipariş numarası + e-posta — sürdürür. Bunu mümkün kılan, siparişin kendi iletişim e-postasını dondurmasıdır (§3.23.3; K-116).

**3.15.3 Silinmiş bir hesabın siparişleri hiçbir hesaba bağlanmaz;** §3.13.2'nin bağlama kuralı onlara uygulanmaz. Kayıt yalnız firmanın ticari kaydı olarak yasal saklama süresi boyunca yaşar (K-117).

**3.15.4 Kişisel veriyle ilgili başvurunun kanalı iletişim formudur.** `B8-01`'in iletişim/talep formuna "KVKK talebi" seçeneği eklenir; ayrı bir KVKK modülü, ayrı durum makinesi ve süre sayacı yoktur. 30 günlük cevap süresi bir ürün kararı değil, KVKK md. 13'ün gereğidir (K-118).

**3.15.5 Uygulama içi veri indirme yoktur.** Kullanıcı verisini §3.15.4'ün kanalından ister, firma cevabı hazırlar (K-119).

### 3.16 Sepet

**3.16.1 Üyenin sepeti hesabında durur ve her cihazda aynıdır; misafirin sepeti bulunduğu tarayıcıya bağlıdır.** Üye çıkış yaptığında o tarayıcıda sepet boş görünür — sepet hesapla birlikte gider (K-123).

**3.16.2 Sepet kendiliğinden boşalmaz.** Misafirin sepeti o tarayıcının verileri silinene kadar, üyeninki hesap durdukça yaşar; süre parametresi yoktur (K-126).

**3.16.3 Giriş anında iki sepet birleşir; aynı varyant toplanmaz.** Misafirken dolan sepet, girişte hesaptaki sepetle tek sepete iner. Aynı varyant iki tarafta da varsa adetler toplanmaz, **büyük olan kalır**; farklı varyantlar ayrı kalemler olarak yan yana durur. Kural giriş yolundan bağımsızdır (K-124). Birleşen sepet de adet ve stok kurallarından geçer (§3.16.8–§3.16.10).

**3.16.4 Birleştirme, müşterinin o ziyarette görmediği bir ürünü ya da adedi sepete eklediyse müşteriye bir kez söylenir** ve eklenen ürünler adıyla listelenir ("Önceki ziyaretinden sepetine 1 ürün eklendi: Kupa"). Birleştirme müşterinin gördüğü sepete bir şey eklemediyse mesaj yoktur. Mesajın metni ve yeri `04`'ün işidir (K-125).

**3.16.5 Sepet kalemi canlıdır:** fiyatını ve satın alınabilirliğini güncel üründen okur (K-80). Kalem, varyantın sepete **ilk** eklendiği andaki birim fiyatı hatırlar; güncel fiyat bundan farklıysa satırda "Sepete eklediğinden beri fiyatı değişti" yazar. Artışta da düşüşte de aynı metin yazar; eski fiyat ve değişimin yönü gösterilmez. İşaret fiyat referansa dönerse kalkar; sonradan adet artırmak referansı değiştirmez; birleştirmede daha önce eklenmiş olanın fiyatı referans kalır (K-131).

**3.16.6 Vitrinde görünen ama satın alınamayan kalem sepette "Tükendi" işaretiyle kalır ve siparişe girmez** — tükenmiş varyant ve kontenjanı dolmuş hizmet. Stok geri geldiğinde kalem yeniden satın alınabilir olur (K-130).

**3.16.7 Vitrinden kalkan kalem sepetten çıkar ve müşteriye bir kez söylenir** — ürünün ya da varyantın arşive ya da taslağa alınması, kalıcı silinmesi. Sipariş kalan kalemlerle devam eder. **Siparişe girmeyen kalem tutarlara da girmez:** kuponun asgari sepet tutarı, kargo hesabı ve eşikler yalnız siparişe giren kalemleri görür (K-130).

**3.16.8 Stoktan fazla adet sepete eklenmez ve stok adedi söylenmez.** Müşteri "Bu adette stok yok" mesajını görür; mesaj kaç adet alınabileceğini söylemez. Stok ayrılmış adetler düşülerek okunur (K-128). Sepette adet artırma ve giriş anındaki birleştirme aynı kuraldan geçer. Kalem sepete eklendikten sonra stok sepetteki adedin altına düşerse kalem aynı mesajla sepette kalır ve adet düşürülene kadar siparişe girmez — adet kendiliğinden stoğa indirilmez, çünkü indirilen adet stoğu söylerdi. Hizmet kontenjanı aynı kuralı izler (K-150).

**3.16.9 Firma bir üründen bir siparişte alınabilecek adedi sınırlayabilir.** Ürün formunda isteğe bağlı bir "bir siparişte en fazla" alanı vardır; sınır ürünün **bütün varyantlarının toplam adedine** uygulanır — sınır 2 ise sepette 1 kırmızı + 1 mavi lamba bulunabilir, 2 kırmızı + 1 mavi bulunamaz. Sınırı aşan adet sepete eklenmez ve müşteri sınırı sayısıyla söyleyen mesajı görür ("Bu üründen bir siparişte en fazla 2 adet alınabilir"). Seçilen adet hem sınırı hem stoğu aşıyorsa firmanın sınırı söylenir, stok söylenmez. Alan boşsa tek sınır stoktur (K-151, K-152).

**3.16.10 Sonradan aşılan sınırda kalemler sepette mesajıyla kalır.** Firma sınırı düşürdüğünde ya da girişte birleşen sepette farklı varyantların toplamı sınırı aştığında, ürünün kalemleri toplam sınıra inene kadar siparişe girmez. Siparişe girmeyen kalem toplama sayılmaz (K-151, K-152).

**3.16.11 Sepetteki kalem sayısına tavan yoktur** (K-154).

**3.16.12 Sepet, ödeme başarılı olduğunda boşalır** — ve yalnız siparişe giren kalemler çıkar; ödeme sürerken sepete eklenen başka bir ürün sepette kalır. Ödeme yarıda kalırsa (yanlış doğrulama kodu, kart limiti, banka ekranının kapanması) sepet olduğu gibi durur ve müşteri aynı sepetten yeniden dener (K-127).

Dijital üründe adedin 1 olması ve daha önce alınmış dijital varyantın uyarısı §3.12.4 ve §3.12.9'dadır.

### 3.17 Sipariş onayı ve stok ayırma

**3.17.1 Sipariş, müşterinin ödeme adımında siparişi onayladığı anda oluşur.** Aynı anda sipariş numarası verilir (§3.22.2), kalemler, adresler ve sözleşme sürümleri donar (§3.23) ve stok ayrılır (§3.17.3) (K-80, K-128, K-186).

**3.17.2 Onaylanan özet ile oluşan sipariş birebir aynıdır.** Onay anında sepet yeniden değerlendirilir. Ödenecek tutarı, tutarın dökümünü (KDV, indirim, kuponun payı, kargo) ya da siparişe girecek kalemleri değiştiren **her fark** — fiyatın artması ya da düşmesi, indirimin başlaması ya da bitmesi, kuponun geçersizleşmesi, bir kalemin ayrılamaması, kargo ücretinin, ücretsiz kargo eşiğinin ya da teslimat illerinin değişmesi — siparişin oluşmasını durdurur. Müşteri güncel özeti görür ve isterse yeniden onaylar; güncel özet neyin değiştiğini açıkça söyler ("Lambanın fiyatı 1.000 TL'den 1.200 TL'ye değişti"). Değişikliğin yönü fark etmez. Ayrılamayan kalem için sayı söylenmez (K-129, K-132, K-133, K-135, K-150). Ön Bilgilendirme Formu özetle birebir aynı olduğu için aynı fark formu da değiştirir (§3.24.3).

**3.17.3 Sipariş onaylandığında kalemlerin adedi stoktan ayrılır ve müşteriye bir ödeme süresi tanınır.** Ödeme süre içinde başarılı olursa ayrılan adet stoktan **kesin düşer**; süre dolar ya da ödeme başarısız olursa — hangisi önce gelirse — adet **stoğa geri döner**. Sepete eklemek stok ayırmaz. Aynı rejim hizmet kontenjanına ve kupon kullanım hakkına (§3.10.6) uygulanır; dijital ürünün ayrılacak bir şeyi yoktur (K-128).

**3.17.4 Ödeme süresi yönteme göre ayrıdır.** Kart ödemesinin süresi dakikalarla ölçülür ve sistem sabitidir; havalenin süresi günlerle ölçülür, firma panelden girer ve sistem bir üst çit koyar. İki değer de çitleriyle §11'dedir; "iş günü"nün tanımı `B8-06`'nın işidir (K-166). Havale beklenirken ayırma süre boyunca sürer (K-165).

**3.17.5 Ödeme süresi dolan ya da kart ödemesi başarısız olan sipariş kendiliğinden iptal olur** ve ayrılan stok, hizmet kontenjanı ve kupon hakkı aynı anda serbest kalır; firmanın elle iptal etmesi beklenmez (K-180). Kart ödemesinde iptalden önce sağlayıcıya ödemenin sonucu son bir kez sorulur (§6.1.2, K-232). Geçişler §5.6'dadır.

**3.17.6 Başarısız ödemenin tekrarı yeni bir siparişle olur.** Müşteri sepetten yeniden onaylar ve yeni bir sipariş numarası alır; başarısız sipariş İptal edildi olarak kayıtta kalır. Aynı siparişi sürdürme yolu yoktur (K-181).

**3.17.7 Müşteri aynı sepetten yeni bir siparişi onayladığında, o sepete bağlı ödenmemiş önceki siparişi kendiliğinden iptal edilir** ve ayırması serbest kalır; yeni siparişin ayırması bundan sonra yapılır. Müşteri kendi ayırdığı stoğa takılmaz (K-182).

**3.17.8 Ödemesi hiç başarılı olmamış ve kendiliğinden iptal edilmiş sipariş müşterinin sipariş listesinde görünmez; firmanın panelinde görünür.** Ödemesi başarılı olmuş ve sonradan iptal edilen sipariş müşteriye görünmeye devam eder (K-184).

### 3.18 Asgari sipariş tutarı

**3.18.1 Firma isteğe bağlı tek bir asgari sipariş tutarı girebilir;** girmezse özellik kapalıdır. Sepetin tutarı bunun altındaysa sipariş verilemez ve müşteri eksik tutarı görür ("Sipariş verebilmek için 130 TL'lik daha ürün ekleyin"). Kontrol sepette ve onay anında yapılır; onay anında bir fiyat değişikliği ya da siparişe girmeyen bir kalem yüzünden toplam altına düşerse sipariş durur ve müşteri güncel özeti görür (K-155).

**3.18.2 Karşılaştırma tabanı ücretsiz kargo eşiğininkiyle aynıdır:** siparişe giren bütün kalemlerin kupondan önceki, indirimli fiyatlarla toplamı. Kupon girmek bir siparişi asgari tutarın altına düşürüp engellemez (K-156).

**3.18.3 Asgari tutar yalnız en az bir fiziksel kalem içeren siparişe uygulanır.** Uygulandığında taban yine bütün kalemlerin toplamıdır — karışık siparişte dijital ürün ve hizmet de tabana girer. Koşul **siparişe giren** kalemlere bakar: sepette yalnız "Tükendi" işaretli bir fiziksel kalem varsa kural uygulanmaz; müşteri fiziksel kalemi çıkarırsa kural kalkar (K-157).

### 3.19 Kargo ücreti ve ücretsiz kargo eşiği

**3.19.1 Kargo ücreti sipariş başına sabittir.** Firma panelde tek bir tutar girer; siparişte **en az bir fiziksel kalem** varsa sipariş bu ücreti bir kez öder — adet, kalem sayısı, ağırlık ve teslimat ili ücreti değiştirmez. Yalnız dijital ürün ve hizmetten oluşan siparişte kargo ücreti yoktur. Firma 0 TL girerse her sipariş ücretsiz kargoludur (K-132). Kargo şirketiyle entegrasyon yoktur (`08`, K-132).

**3.19.2 Kargo ücreti KDV dahil girilir ve siparişin KDV dökümünde kendi satırını taşır.** Kargo KDV oranı bir firma ayarıdır; varsayılanı genel orandır ve değeri §11'dedir (K-132, K-219).

**3.19.3 Firma isteğe bağlı bir ücretsiz kargo eşiği girebilir.** Karşılaştırma tabanı eşiğe eşit ya da üstündeyse siparişten kargo ücreti alınmaz ("500 TL ve üzeri siparişlerde kargo ücretsiz"); eşik girilmezse özellik kapalıdır (K-135).

**3.19.4 Eşiğin tabanı kupondan önceki tutardır:** siparişe giren bütün kalemlerin — fiziksel, dijital ve hizmet — indirimli fiyatlarla toplamı. İndirim tabanı düşürür, kupon düşürmez; kupon girmek bir siparişi hiçbir zaman pahalılaştırmaz (K-136, K-137).

**3.19.5 Eşik tanımlıysa ve siparişte fiziksel kalem varsa, sepette ve ödeme özetinde eşiğe kalan tutar yazar** ("Ücretsiz kargoya 80 TL kaldı"); taban eşiği karşıladığında "Kargo ücretsiz" yazar. Eşik yoksa ya da fiziksel kalem yoksa mesaj da yoktur. Metnin biçimi ve yeri `04`'ün işidir (K-138).

**3.19.6 Kargo ücreti ve ücretsiz kargo kararı sipariş düzleminde donar;** sonraki kısmi iptal ya da iade onu yeniden hesaplatmaz (§3.23.2, §7.4.6; K-132, K-213).

### 3.20 Teslim: bölge, yol, kargoya verme ve takip

**3.20.1 Firma teslimat yaptığı illeri seçer; varsayılan seçim tüm Türkiye'dir.** Fiziksel kalem içeren siparişin teslimat adresi seçili illerden birinde değilse sipariş verilemez ve müşteriye sebebi söylenir ("Bu firma Ankara'ya teslimat yapmıyor"). Kısıt il düzeyindedir; ilçe düzeyinde sınır yoktur. Kısıt varsa site bunu müşteri adres adımına gelmeden görünür kılar; biçimi `04`'ün işidir. Üyenin defterindeki teslimat yapılmayan ildeki adres ödeme adımında seçilemez. Yalnız dijital ürün ve hizmetten oluşan sipariş il kontrolüne girmez (K-133).

**3.20.2 Tek teslimat yolu kargodur.** Ödeme adımında teslimat seçeneği yoktur; mağazadan teslim alma ve adlandırılmış teslimat seçenekleri (standart kargo, hızlı kargo) yoktur. Sistem için "kargo", fiziksel kalemin müşterinin adresine gönderilmesidir — firmanın bunu bir kargo şirketiyle ya da kendi aracıyla yapması bir ayrım değildir (K-134). Şube teslim noktası değildir (K-243).

**3.20.3 Sipariş tek parça gider ve tek takip numarası taşır;** parçalı gönderim yoktur. Farklı kargoya verme süresi taşıyan kalemler birlikte bekler; acelesi olan müşteri ayrı sipariş verir (K-177).

**3.20.4 Firmanın teslimat sözü kargoya verme süresidir** ve vitrinde "2 iş günü içinde kargoya verilir" biçiminde gösterilir. **Yasal teslim üst sınırı** — satıcı malı taahhüt ettiği sürede ve her durumda en geç 30 gün içinde teslim eder — sözleşme metninde yazar. Panel kargoya verme süresine bir üst çit koyar; çitin değeri §11'de, "iş günü"nün tanımı `B8-06`'dadır. Söz yalnız fiziksel kalemi kapsar (K-139).

**3.20.5 Kargoya verme süresinin firma varsayılanı vardır; ürün kendi süresini taşıyabilir** ve taşıyorsa varsayılanı ezer; varyant ürününkini devralır. Siparişin sözü, siparişe giren fiziksel kalemlerin sürelerinin **en uzunudur** ve sipariş onaylandığı anda donar. Üst çit hem varsayılana hem ürünün süresine uygulanır (K-140).

**3.20.6 Kargoya verme süresi ödemenin onaylandığı anda başlar** — kartta sağlayıcının başarı bildiriminde, havalede firmanın "ödendi" işaretinde (K-168, K-173).

**3.20.7 Sipariş "kargoya verildi" olarak işaretlenirken ya kargo şirketi + takip numarası girilir ya da "kendi aracımızla teslim" seçeneği işaretlenir;** ikisinden biri olmadan işaret konamaz. Müşteri bu bilgiyi sipariş sayfasında görür (K-141).

**3.20.8 Kargo şirketi ürünle gelen bir listeden seçilir;** listede olmayan şirket "Diğer" seçilip adıyla yazılır. Listedeki şirketlerde müşteri sipariş sayfasında tek tıkla açılan bir "Takip et" bağlantısı görür; "Diğer"de yalnız şirket adı ve takip numarası görünür. Liste ve takip bağlantısı kalıpları ürünün kendi varlığıdır: firma panelden düzenlemez ve §11'e girmez (K-142).

**3.20.9 Kalemin teslimi tipine göre işler.** Fiziksel kalem kargo hattından geçer (§3.20.7). Dijital kalem ödeme onaylandığında sistem tarafından teslim edilir (§3.12.1, K-174). Hizmet kalemi firmanın panelden "tamamlandı" işaretiyle teslim edilmiş olur; tek bir işlemdir — randevu, takvim ya da planlama ekranı yoktur (K-175).

**3.20.10 Karışık siparişte sevkiyat tek eksende kalır ve fiziksel hattı izler;** dijital kalem bunu beklemeden ödeme onayında indirilebilir olur, hizmet kalemi ayrıca "tamamlandı" işaretlenir. Kalem düzeyinde tutulan şey **teslim işaretidir**, ayrı bir durum makinesi değildir (K-179). Hatlar §5.7'dedir.

**3.20.11 Fiziksel teslimin kaydı:** kargoya verilmiş fiziksel siparişin teslim edildi sayılmasını — kimin işaretlediğini ve teslim tarihinin nereden geldiğini — karar kaydı yazmıyor. Cayma penceresi (§7.3.1) ve ayıp talebinin süresi (§7.5.1) bu tarihten başlar. **Açık: A-06.**

### 3.21 Ödeme

**3.21.1 Satıcı firmadır; platform ticari zincirde yer almaz.** Tahsilat doğrudan firmanın kendi sanal POS / ödeme sağlayıcı hesabına geçer. Kart bilgisi sağlayıcının barındırdığı sayfada ya da çerçevede girilir; ürün kart verisini **görmez, saklamaz, loglamaz** (K-12).

**3.21.2 İki ödeme yöntemi vardır: kart ve havale/EFT.** Kapıda ödeme yoktur (K-159, K-160).

**3.21.3 Her kart ödemesi istisnasız 3D Secure doğrulamasından geçer;** doğrulama başarısızsa ödeme gerçekleşmez ve sipariş ödenmiş sayılmaz. Tutar eşiği ya da sağlayıcı takdiri yoktur (K-13).

**3.21.4 Kart yönteminin panelde açık/kapalı anahtarı yoktur.** Ödeme sağlayıcı anahtarları kurulumdan geldiği için kart, anahtarlar tanımlıysa açıktır; tanımlı değilse kart yöntemi görünmez ve havale açıksa satış yalnız havaleyle açılır (K-162, K-163).

**3.21.5 Havale/EFT panelden açılıp kapatılır; açıkken IBAN zorunludur.** Havale kapalıyken yöntem ödeme adımında hiç görünmez (K-161).

**3.21.6 Havale ile siparişi onaylayan müşteri ekranda firmanın IBAN'ını ve sipariş numarasını görür;** sipariş ödeme bekler. Firma parayı hesabında görüp panelden "ödendi" işaretlediğinde ödeme onaylanır (K-159). Ödeme süresi dolmadan müşteriye bir hatırlatma gider — işlem bildirimidir; sıklığı ve zamanı `B8-03`'ün işidir (K-167).

**3.21.7 Firma havalede gelen tutarı sisteme girmez.** Sistemde gelen tutar alanı, sipariş tutarıyla karşılaştırma ve "eksik ödeme" diye bir durum yoktur; eksik ya da fazla gelen havalede farkı isteme ya da kabul etme kararı firmanındır ve sistem dışında yürür (K-169).

**3.21.8 Süresi dolup iptal edilmiş bir sipariş sonradan "ödendi" işaretlenemez.** Firma gelen parayı müşteriye iade eder ya da müşteriden yeni sipariş vermesini ister. Bu sınır yöneticinin elle durum geçişi yetkisinin (`B8-14`) de dışındadır (K-183). Kart hattındaki karşılığı §6.2.16'dadır.

**3.21.9 Ürün taksiti bilmez.** Taksit yalnız sağlayıcının kart ekranında sunulur; vitrinde, ürün sayfasında, sepette ve ödeme özetinde taksit bilgisi gösterilmez ve sipariş **tek tutar** taşır (K-164).

**3.21.10 Sipariş hazırlığa ancak ödeme onaylandığında geçer** (§5.6; K-173).

### 3.22 Sipariş numarası ve takip erişimi

**3.22.1 Sipariş numarası tahmin edilemez bir koddur:** yıl + rastgele karakter bloğu (ör. `2026-7K4M9P`). Karışabilen karakterler (`0`/`O`, `1`/`I`/`l`) kullanılmaz; rastgele bloğun uzunluğu §11'dedir (K-185).

**3.22.2 Numara sipariş oluştuğu anda verilir ve sipariş iptal edilse bile tekrar kullanılmaz.** Sipariş numarası **fatura numarası değildir** (K-186).

**3.22.3 Sipariş sayfasına üç yoldan girilir:** üye giriş yapıp sipariş geçmişinden açar; misafir alıcı sipariş numarası + e-posta ile açar; sipariş e-postasındaki bağlantı sipariş sayfasını doğrudan açar ve ayrıca e-posta sormaz. Üçüncü yol dijital ürünün indirme sayfasına giden bağlantıyla aynı kapıdır (K-187, K-97). Takip formunun deneme limiti `B8-05`'in işidir.

**3.22.4 Sipariş sayfası her bilgiyi taşır** — durum, takip bağlantısı, indirme düğmesi, IBAN; hiçbir akış bir e-postanın ulaşmasına bağlı değildir (§6.1.1; K-234).

### 3.23 Sipariş anında donan değerler

**3.23.1 Sipariş kalemi, ödenen tutarı üreten her değeri ve ürünün o anki tanımını dondurur:** birim fiyat, KDV oranı, kalem KDV'si, indirim oranı, beyan edilen referans fiyat, kuponun kaleme düşen payı; ürün adı, seçenek değerleri ("Kırmızı / M") ve görsel. Kalem canlı üründen hiçbir şey okumadan kendini anlatır (K-77, K-79). Tek istisna dijital ürünün dosyasıdır (§3.12.7).

**3.23.2 Donan her değer sipariş kaleminde düz olarak yaşar.** Ürün düzlemli alanlar da (KDV oranı, indirim oranı) kaleme kopyalanır; sipariş düzleminde yalnız sepet düzeyindeki değerler durur — kupon kodu, kargo ücreti ve toplamlar. Ara bir gruplama katmanı yoktur (K-78, K-132).

**3.23.3 Sipariş, teslimat ve fatura adreslerini, iletişim e-postasını, firmanın o günkü yasal kimliğini (unvan, vergi/MERSİS numarası, adres, iletişim) ve onaylanan Ön Bilgilendirme Formu ile Mesafeli Satış Sözleşmesi sürümlerini kendi içine dondurur.** Adres defterindeki, hesaptaki ve firma kimliğindeki sonraki değişiklik ya da silme geçmiş siparişe dokunmaz; geçmiş siparişin sayfası onaylanan sürümü ve o günkü unvanı göstermeye devam eder (K-113, K-116, K-189, K-190).

**3.23.4 Donma anı siparişin oluştuğu andır.** Müşterinin onayladığı tutar bağlayıcıdır ve ödeme sonucundan bağımsızdır (K-80). Siparişin kargoya verme sözü de bu anda donar (K-140).

**3.23.5 Marka ayarları siparişe donmaz:** geçmiş siparişin sayfası güncel logo, marka adı ve renkle görünür; donan yalnız yasal kimliktir (K-265, K-190).

### 3.24 Onay adımı ve yasal metinler

**3.24.1 Sipariş iki ayrı onay kutusuyla onaylanır:** Ön Bilgilendirme Formu'nun okunduğunun teyidi ve Mesafeli Satış Sözleşmesi'nin onayı. İkisi de işaretlenmeden sipariş onaylanamaz (K-188).

**3.24.2 Sepette dijital kalem varsa üçüncü bir kutu çıkar:** "İndirme hemen açılacak, bu üründe cayma hakkımın düşeceğini kabul ediyorum". Kutu işaretlenmeden dijital kalem siparişe giremez (K-204; §7.3.2).

**3.24.3 Ön Bilgilendirme Formu onay özetiyle birebir aynıdır** ve üzerine dört bilgi ekler: teslimat il kısıtı, kargoya verme süresi, cayma hakkının kullanım koşulları ve süresi, firmanın kimlik bilgileri. Özeti değiştiren her fark formu da değiştirir ve siparişi durdurur (K-191, K-129). İstisna işaretli üründe istisna ve sebebi (K-206), iade kargo bedelinin firmaya ait olduğu (K-210) da formda yazar.

**3.24.4 Yaş için ayrı beyan istenmez.** Yaş kuralı üyelik sözleşmesinde ve Mesafeli Satış Sözleşmesi'nde yazılıdır; müşteri metni onaylayarak beyan etmiş olur. Onay adımında "18 yaşından büyüğüm" kutusu ve doğum tarihi alanı yoktur. Maddenin metinlere girmesi `B8-12`'nin bağlayıcı işidir (K-114, K-192).

**3.24.5 Misafir alıcının e-postası doğrulama koduyla sınanmaz.** Onay adımında özetin içinde açıkça gösterilir ("Siparişiniz şu adrese gönderilecek: x@y.com") ve müşteri onaylamadan önce düzeltebilir; adres ikinci kez yazdırılmaz (K-193).

**3.24.6 Onaylanan metinlerin sürümü ve firmanın kimliği siparişe donar** (§3.23.3). Metinlerin sürümlenme mekanizması `B8-12`'nin işidir (K-189).

### 3.25 Fatura

**3.25.1 Ürün fatura kesmez; e-Arşiv ve e-Fatura entegrasyonu yoktur.** Sistem faturanın gerektirdiği veriyi tam taşır — kalem dökümü, KDV oranları ve tutarları, indirim ve kupon payı, kargo ücreti ve KDV'si, fatura adresi, firmanın donmuş kimliği — ve sipariş dışa aktarmasıyla (`B9-14`) dışarı verir. Firma faturayı kendi muhasebe programından ya da entegratöründen keser (K-217).

**3.25.2 Fatura sistemde saklanmaz ve gösterilmez.** Sipariş sayfasına fatura yükleme, faturayı sistem içinden iletme ve fatura arşivi yoktur; firma faturayı müşteriye kendi kanalından iletir (K-218).

**3.25.3 Dışa aktarmada siparişin donmuş firma kimliği kullanılır,** panelin o anki kimliği değil (K-220).

### 3.26 İptal, cayma, iade ve ayıp talebi

Bu kuralların tamamı §7'dedir ve burada tekrarlanmaz (K-178, K-195…K-216, K-226…K-229).

### 3.27 Kurumsal içerik

**3.27.1 Kurumsal içerik hazır tiplerle ve düzeni sabit bir genel sayfayla girilir;** sayfa kurucu yoktur — firma düzen kurmaz, yalnız içerik girer. Ana sayfa bu tiplerden beslenir (K-237).

**3.27.2 Beş hazır tip vardır: Hakkımızda, hizmet tanıtımı, referans iş, sık sorulan soru ve şube;** bunlara genel sayfa ve duyuru eklenir. **Ayrı bir galeri tipi yoktur** — fotoğraf ait olduğu içerikte durur. Kariyer, belgeler, ekip, tarihçe gibi ihtiyaçlar genel sayfayla karşılanır (K-238, K-269).

**3.27.3 Hakkımızda tek kayıttır:** kısa tanıtım (düz metin, karakter tavanlı), uzun metin ve görseller. Firma ikinci bir Hakkımızda açamaz; Hakkımızda **silinmez**, yalnız taslağa alınır (K-239, K-277).

**3.27.4 Hizmet tanıtımı fiyatsızdır ve satın alınmaz.** Ad, kısa açıklama (listedeki kartta), metin, görseller ve kendi sayfası vardır; sepete ekleme yoktur, sayfadaki "Bize ulaşın" düğmesi iletişim formuna (`B8-01`) götürür. Satılan hizmet ürün tipinden ayrıdır (K-240).

**3.27.5 Referans iş, firmanın yaptığı bir işi anlatır:** başlık, kısa açıklama, metin, görseller ve kendi sayfası. Ayrı müşteri adı, yıl ve kategori alanı yoktur; müşteri adı başlıkta yazılır (K-241).

**3.27.6 Sık sorulan soru, soru ve cevaptan oluşur;** hepsi tek bir SSS sayfasında listelenir ve başlıklara bölünmez. Soru düz metin ve karakter tavanlıdır. **Kayda geçen risk:** cevap, sistemin ayarlarda tuttuğu bir kuralı (kargo ücreti, iade süresi) tekrarlarsa ayar değiştiğinde çelişkili kalır; risk `B8-12`'ye devredilmiştir (K-242).

**3.27.7 Şubeler İletişim sayfasında listelenir;** ayrı sayfaları ve menü öğeleri yoktur. Ad ve adres zorunludur — il kapalı listeden seçilir; telefon, çalışma saatleri (kısa düz metin), görsel ve harita bağlantısı isteğe bağlıdır. **Gömülü harita yoktur;** şube "Haritada aç" bağlantısı taşır. Şube teslim noktası değildir (K-243).

**3.27.8 İletişim sayfası ayrı bir içerik tipi değildir:** firma kimliğinin iletişim bilgileri, şubeler ve iletişim formu (`B8-01`) bir araya gelir; firma ayrıca metin girmez (K-243).

**3.27.9 Genel sayfa başlık, metin ve görsellerden oluşur, düzeni sabittir ve adet sınırı yoktur.** Yasal metinler genel sayfa olarak girilmez — sürümlü ayrı kayıtlardır (`B8-07`, `B8-09`, `B8-12`) (K-244).

**3.27.10 Hizmet tanıtımı ve referans iş katalogdan ürün bağlayabilir.** Bağlı ürünler içerik sayfasında "İlgili ürünler" başlığıyla vitrin kartı olarak görünür. Yalnız yayındaki ürünler gösterilir — taslağa ya da arşive alınan ürün bağda kalır ama görünmez, yeniden yayına alınınca geri gelir; silinen ürün bağdan kalkar. Bağ ürün düzlemindedir, varyant seçilmez; ürün sayfasında ters yön ("bu ürünün geçtiği referanslar") yoktur (K-245).

**3.27.11 Hizmet tanıtımı, referans iş, SSS, şube ve genel sayfa listelerini firma panelde elle sıralar;** vitrindeki liste sayfaları, ana sayfa blokları ve menü bu sırayı izler (K-247). Ürün listelerinin sabit düzeni (§3.5.3) bundan etkilenmez.

**3.27.12 Firma isteğe bağlı sosyal medya hesap bağlantıları ve bir WhatsApp numarası girer.** Platform listesi kapalıdır: Instagram, Facebook, X, LinkedIn, YouTube, TikTok. Bağlantılar sitenin üst ve alt bölümünde görünür; sitede gömülü gönderi akışı yoktur. Bağlantılar firma kimliğinin parçası değildir: zorunlu değildir ve siparişe donmaz (K-249).

**3.27.13 Hiçbir kurumsal içerik tipi zorunlu değildir;** site kurumsal içerik girilmeden de yayındadır. Kayıt düzeyinde zorunlu alanlar: her tipte ad ya da başlık, SSS'de ayrıca cevap, şubede adres. Görsel hiçbir tipte zorunlu değildir (K-250).

**3.27.14 Hakkımızda boşken ya da yayında değilken ana sayfanın kurumsal bloğu marka adını ve logoyu gösterir;** boş kutu görünmez (K-250, K-259).

**3.27.15 Kurumsal içeriğin görselleri ve kısa alanları ürünün kurallarına tabidir:** ana görsel işareti sıradan bağımsızdır, galeri elle sıralanır, alternatif metin isteğe bağlıdır ve boşsa kaydın adı ya da başlığı kullanılır; format listesi ve tek dosya boyutu tavanı aynı parametrelerdir, kayıt başına görsel adedi tavanı vardır. Kısa alanlara — ad, başlık, soru, kısa tanıtım, kısa açıklama — karakter tavanı konur; uzun metin serbesttir. Değerler §11'dedir (K-252).

**3.27.16 Uzun metinler §3.11.6'nın metin biçimi setini kullanır** (K-267).

**3.27.17 Kurumsal içerikte gömülü video oynatıcı yoktur.** Firma videoya bağlantı verir; ziyaretçi tıkladığında video kendi platformunda açılır (K-254).

**3.27.18 Kurumsal içeriğe dosya eki (PDF vb.) eklenmez.** Kalite belgesi, sertifika gibi belgeler görsel olarak yüklenir (K-255).

**3.27.19 Müşteri görüşleri ayrı bir içerik tipi değildir.** Firma bir müşterisinin sözünü referans işin metnine yazabilir; ana sayfada dönen bir alıntı bloğu yoktur (K-256).

**3.27.20 Blog yoktur.** Tarihli yazılar, yazı arşivi, kategori ya da etiket ve yazı akışı kapsam dışıdır; arada bir yazı yayımlamak isteyen firma genel sayfa açar (K-268).

**3.27.21 Duyuru şeridi vardır: tek, kısa, tarihli.** Sitenin her sayfasının en üstünde bir satırlık **metin** ve isteğe bağlı bir **bağlantı**; aynı anda tek duyuru görünür. Başlangıç ve bitiş tarihi isteğe bağlıdır; girilirse duyuru yalnız o aralıkta görünür — firmanın elle kaldırmasına gerek kalmaz. Metin düz metin ve karakter tavanlıdır (değeri §11'de). Kayan afiş (slider) ve ana sayfa kampanya görseli yoktur (K-269).

**3.27.22 Site boş kurulur.** Kurumsal içerik, katalog ve duyuru örnek kayıt taşımaz — örnek metin, ürün ya da görsel yoktur. Boş sitenin görünüşü kurallarla tanımlıdır: marka adı girilene kadar alan adı (§3.29.3), logo yoksa marka adı (§3.29.2), Hakkımızda boşken marka adı ve logo (§3.27.14), kaydı olmayan tip menüde ve ana sayfada görünmez (§3.28.3, §3.28.4). İlk kurulumda firmaya yol gösterme `B9-01`'in kapsamındadır (K-271).

**3.27.23 Her içerik kaydı kendi yayın durumunu taşır: Taslak ya da Yayında;** arşiv yoktur ve yayından kaldırılan kayıt taslağa döner. Yayına almak için ikinci bir onay yoktur — her yönetici yayımlar (K-251, K-273). Durum tanımı §5.2'dedir.

**3.27.24 Taslak kayıt yalnız adla ya da başlıkla kaydedilebilir; yayına almak §3.27.13'ün zorunlu alanlarını ister.** Zorunlu alanı eksik kayıt yayına alınamaz ve panel eksik alanı gösterir (K-275).

**3.27.25 Taslak içeriğin sayfası ziyaretçiye "sayfa bulunamadı" döner; giriş yapmış yöneticiye sayfayı "Taslak" bandıyla gösterir** — önizleme budur. Kendi sayfası olmayan kayıtlar — SSS sorusu, şube, duyuru — yöneticiye göründükleri yerde aynı işaretle önizlenir; biçimi `04`'ün işidir. Taslak kayıt menüde, ana sayfada ve listelerde ziyaretçiye görünmez (K-274).

**3.27.26 İçerikte sürüm geçmişi yoktur.** Yayındaki bir kayıt düzenlenip kaydedildiğinde değişiklik hemen yayına girer; eski hâle dönme işlemi yoktur. Büyük bir yeniden yazımda firma yeni bir taslak kayıt açar, hazır olduğunda onu yayına alır ve eskisini taslağa alır ya da siler. Değişikliği kimin yaptığı işlem izinde görünür; izin kapsamı `B8-02`'nin işidir (K-276). Yasal metinlerin sürümlenmesi ayrı bir konudur (`B8-12`, K-189).

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

**3.29.1 Firma görünümde dört şeyi ayarlar: logo, marka adı, site simgesi ve tek bir marka rengi.** Ana sayfa düzeni §3.28.1'in iki seçeneğinden seçilir. Yazı tipi, sayfa düzeni, bileşenler ve marka rengi dışındaki bütün renkler üründedir; firma değiştiremez. Marka ayarları vitrine ve e-postalara uygulanır; yönetim paneli ürünün kendi görünümündedir. Koyu tema yoktur (K-258).

**3.29.2 Logo zorunlu değildir.** Logo yoksa marka adı yazıyla gösterilir — sitenin üst bölümünde, ana sayfanın kurumsal bloğunda ve e-postalarda. Tek bir logo yüklenir ve her yerde aynısı kullanılır; logoya görsel dosya kuralları uygulanır (§3.11.3, §3.27.15). Yasal kimlik alanları zorunlu kalır (§3.1.2) (K-259).

**3.29.3 Marka adı unvandan ayrı ve zorunlu bir alandır;** bir kez girildikten sonra boşaltılamaz. Sitenin üst bölümünde (logo yoksa), tarayıcı sekmesinde ve e-postalarda görünen addır. Unvan altbilgideki yasal bilgilerde ve sözleşmede durur. **Marka adı girilene kadar site alan adını gösterir** (K-260).

**3.29.4 Site simgesi isteğe bağlıdır;** yüklenmezse marka adının baş harfi marka rengi zemin üzerinde gösterilir. Firmanın sitesinde Shopfolio simgesi çıkmaz (K-261).

**3.29.5 Marka rengi okunurluğu bozamaz.** Marka renginin üstündeki yazının siyah mı beyaz mı olacağını sistem, yeterli kontrastı veren tarafı seçerek belirler. Anlam taşıyan renkler — hata, uyarı, başarı, "Tükendi", indirim — marka renginden bağımsızdır ve ürünle sabit gelir. Renk seçilmezse ürünün varsayılan rengi kullanılır. Rengin uygulandığı yerler `04`'ün tasarım sisteminin işidir (K-262).

### 3.30 Sayfa adresleri, arama motorları ve paylaşım

**3.30.1 Ürünün adresi kategoriye bağlı değildir;** ürünün adından üretilir (ör. `/urun/ahsap-masa`; önekin kendisi `07`'nin işidir). Kategori sayfasının adresi de ağaçtaki yerinden bağımsızdır, kategorinin adından üretilir. Kırıntı yolu ana kategoriden üretilmeye devam eder (K-279).

**3.30.2 Her sayfanın adresi kaydın adından üretilir ve ilk yayında sabitlenir.** Türkçe harfler sadeleşir ("Çelik Kapı" → `celik-kapi`); aynı adres zaten varsa sonuna sayı eklenir; adreslerde dil öneki yoktur. Kayıt ilk kez yayına alınana kadar adres adla birlikte değişir; ilk yayında sabitlenir ve kaydın adı ya da menüdeki adı değişse de bir daha değişmez. Liste sayfalarının adresleri (hizmetler, referanslar, SSS, Hakkımızda, İletişim) sabittir; kategori adresi kategori oluşturulduğunda sabitlenir. Firma adresi elle düzenleyemez (K-280).

**3.30.3 Yönlendirme sistemi yoktur.** Adres değişmediği için eski adresten yenisine yönlendirme gerekmez; eski siteden gelen adreslerin taşınması kapsam dışıdır (K-280).

**3.30.4 Var olmayan, silinmiş ya da ziyaretçiye kapalı (taslak) her adres "sayfa bulunamadı" sayfasına düşer.** Sayfa sitenin görünümündedir — marka, menü, altbilgi — ve ana sayfaya ve ürünlere dönüş yolu taşır; içeriğini firma düzenlemez. Arşivlenmiş ürünün "artık satılmıyor" sayfası (§3.7.6) bundan ayrıdır (K-281).

**3.30.5 Arama motoru bilgileri otomatik üretilir;** firmanın elle doldurduğu SEO alanı yoktur. (1) **Sayfa başlığı:** kaydın adı ve marka adı ("Ahşap Masa – Yılmaz Mobilya"). (2) **Açıklama:** kaydın kısa açıklamasından ya da kısa tanıtımından; kısa alanı olmayan sayfada metnin başından; kategori sayfasında kategori adından ve marka adından. (3) **Site haritası** yayındaki ürünlerden, kategorilerden ve içerik sayfalarından kendiliğinden güncellenir. (4) **Yapılandırılmış veri:** ürün (fiyat, stok durumu), firma, kırıntı yolu ve SSS, arama motorlarının okuyacağı biçimde sayfaya eklenir. (5) **Paylaşım önizlemesi** aynı başlık ve açıklamayla, sayfanın ana görseliyle — görsel yoksa logoyla — çıkar (K-282).

**3.30.6 Arama motorlarına kapalı sayfalar:** sepet, ödeme adımı, hesap sayfaları, sipariş takip sayfası, yönetim paneli, site içi arama ve süzgeç sonuçları, taslaklar ve arşivlenmiş ürün sayfası. Arşiv sayfası doğrudan bağlantıyla açılmaya devam eder; yalnız arama sonuçlarına ve site haritasına girmez (K-283).

**3.30.7 Ürün ve kurumsal içerik sayfalarında tek bir "Paylaş" düğmesi vardır.** Telefonda cihazın kendi paylaşım menüsünü açar; paylaşım menüsü olmayan tarayıcıda sayfanın bağlantısını kopyalar. Platformların paylaşım kodları (Facebook, X vb.) siteye eklenmez. Paylaşılan bağlantının önizlemesi §3.30.5'in başlığı, açıklaması ve görseliyle çıkar (K-287).

### 3.31 Panelde eşzamanlı düzenleme

**3.31.1 Sessiz üzerine yazma yoktur.** İki yönetici aynı kaydı aynı anda düzenlerse ikinci kaydeden, kaydın o arada değiştiğini görür ve üzerine yazmadan önce uyarılır. Kural kurumsal içerik, ürün ve ayarlar için aynıdır; mekanizması `05`/`06`'nın eşzamanlılık kararıdır (K-270).

*Kaynak: K-01 · K-02 · K-08 · K-09 · K-11…K-14 · K-27 · K-39 · K-40 · K-42…K-83 · K-85…K-88 · K-90…K-99 · K-101…K-169 · K-173…K-175 · K-177…K-220 · K-226…K-230 · K-232 · K-234…K-252 · K-254…K-256 · K-258…K-262 · K-265 · K-267…K-271 · K-273…K-283 · K-286 · K-287. Etki sütununda `02 §3` taşıyan 226 satırın tamamı bu bölümde ya da — iptal, cayma, iade ve ayıp talebi için — §7'de karşılık bulur.*

## 4. Zaman kuralları

> **Ne yazılır:** Tüm süre/timeout kuralları **tek bölümde**. Aktör akışlarına gömülmez — dağıtılırsa tutarsızlık doğar.

> **Yazım kapısı (K-28):** **Blok 8 kapandığında** yazılır — besleyen bloklar: 3 ✓, 4 ✓, 5 ✓, 6 ✓, 7 ✓, 8. Girdi: karar kaydında etki sütunu `02 §4` taşıyan satırlar ve §3'ün süreye bağlı kuralları (ör. §3.17.4 ödeme süresi, §3.20.4 kargoya verme süresi, §3.27.21 duyurunun tarih aralığı). *Liste 2026-09-19'da hizalandı; önceki not "5 ve 8" diyordu.*

| # | Kural | Süre | Süre dolunca ne olur | Ayarlanabilir mi |
|---|---|---|---|---|

## 5. Durum tanımları

> **Ne yazılır:** Ana varlıkların (işlem, hesap, talep vb.) alabileceği durumlar ve geçiş koşulları. Durum makinesi mantığı burada başlar, `03`'te akışa, `06`'da şemaya döner. **Bu isimler tüm dokümanlarda birebir aynı kalır.**

Durum adları ve kod karşılıkları §1.2 sözlüğündedir; `03`, `06` ve `07` onları birebir devralır (K-17). Mağazanın satış açıklığı bir varlık durumu değildir — üç koşula bağlı bir kapıdır ve kuralı §3.1.5'tedir (K-09, K-163, K-235).

### 5.1 Ürün ve varyantın yayın durumu

Ürün ve varyant aynı üç değerli seti **ayrı ayrı** taşır: ürünün kendi durumu, varyantın kendi durumu vardır (K-54, K-55).

| Durum | Kod | Ziyaretçi ne görür | Kaynak |
|---|---|---|---|
| Taslak | `Draft` | Ürünün adresi "sayfa bulunamadı" döner; giriş yapmış yönetici sayfayı "Taslak" bandıyla görür. Yayındaki bir ürünün taslak varyantı seçenek listesinde görünmez | K-54, K-57 |
| Yayında | `Published` | Vitrinde, kategori sayfalarında ve aramada görünür | K-54 |
| Arşiv | `Archived` | Listelerde ve aramada görünmez; ürünün adresi "Bu ürün artık satılmıyor" sayfası döner — ad, görsel, durum; fiyat ve sepete ekleme yok. Yayındaki bir ürünün arşivlenmiş varyantı vitrinden kalkar. **Silme değildir**, kayıt korunur | K-54, K-56, K-69, K-130 |

- **Yayında'ya geçişin koşulu (yayın kapısı):** ürünün adı, ana kategorisi ve tipi girilmiş, arşivlenmemiş en az bir varyantı vardır (K-86, K-45, K-55). Dijital üründe ayrıca yayındaki her varyant bir dosyaya bağlıdır (K-145).
- **Kayıtta yazılı geçişler:** taslaktaki ya da arşivdeki ürün yeniden yayına alınabilir (K-245); yayındaki ürün ya da varyant taslağa çekilebilir ve arşive alınabilir (K-130). **Arşiv ile taslak arasındaki geçişler ve yayındaki bir ürünün son arşivlenmemiş varyantının arşivlenmesi kayıtta yoktur — açık: A-10.**
- **Kalıcı silme bir durum değildir:** kayıt katalogdan kalkar, adresi "sayfa bulunamadı" döner (K-85).
- **Olmayan durumlar:** zamanlanmış yayın yoktur (K-54); "fiyat sorunuz" gibi bir ürün durumu yoktur (K-11). Satışın geçici olarak kapatılması yayın durumunu değiştirmez (K-235).
- Tükenmişlik yayın durumu değildir: tükenmiş ürün ve varyant **Yayında** kalır ve "Tükendi" işaretiyle görünür (§3.6.3, §3.6.4).

### 5.2 Kurumsal içeriğin yayın durumu

Kurumsal içerik yayın durumunun **iki değerini** kullanır; her kayıt — Hakkımızda, her hizmet tanıtımı, referans iş, SSS sorusu, şube, genel sayfa ve duyuru — kendi durumunu taşır (K-251, K-273).

| Durum | Kod | Ziyaretçi ne görür | Kaynak |
|---|---|---|---|
| Taslak | `Draft` | Sayfa "sayfa bulunamadı" döner; kayıt menüde, ana sayfada ve listelerde görünmez. Yönetici sayfayı "Taslak" bandıyla önizler; kendi sayfası olmayan kayıtlar göründükleri yerde aynı işaretle önizlenir | K-274 |
| Yayında | `Published` | Kayıt sitede görünür | K-273 |

- **Taslak → Yayında:** zorunlu alanlar doludur — her tipte ad ya da başlık, SSS'de ayrıca cevap, şubede adres; ikinci bir onay yoktur, her yönetici yayımlar (K-275, K-273).
- **Yayında → Taslak:** yayından kaldırılan kayıt taslağa döner (K-273).
- **Arşiv yoktur**; ileri tarihli yayın yoktur; sürüm geçmişi yoktur — yayındaki kaydın düzenlemesi kaydedildiği anda yayındadır (K-273, K-276, K-278).
- **Duyuru:** Yayında olduğu hâlde, tarihleri girilmişse yalnız o aralıkta görünür (K-269, K-273).
- Kalıcı silme bir durum değildir; Hakkımızda silinmez, yalnız taslağa alınır (K-277).

### 5.3 Sipariş: iki eksen ve oluşma anı

**5.3.1 Sipariş iki bağımsız eksende durum taşır:** sevkiyat ekseni — Sipariş durumu (`OrderStatus`) — ve ödeme ekseni — Ödeme durumu (`PaymentStatus`). Her siparişin her an iki durumu vardır (K-170). Durum sipariş düzeyindedir; parçalı gönderim olmadığı için sevkiyat tek eksende kalır (K-177). Kalem düzeyinde durum makinesi yoktur, kayıtlar vardır (§5.8).

**5.3.2 Sipariş, müşteri onayladığı anda Alındı + Bekliyor olarak doğar.** Aynı anda sipariş numarası verilir (K-186); kalemler kalem bazında yuvarlanmış tutarlarıyla, adresler, iletişim e-postası, firma kimliği ve sözleşme sürümleri donar (K-61, K-77, K-78, K-79, K-80, K-113, K-116, K-189, K-190; §3.23); stok, hizmet kontenjanı ve kupon hakkı ayrılır (K-128). Dijital ürünün dosyası donmaz (K-148).

**5.3.3 Onay anında özet değişmişse sipariş hiç doğmaz** — durum da oluşmaz; müşteri güncel özeti görür (K-129).

**5.3.4 Her ödeme denemesi ayrı bir sipariştir.** Başarısız ödemenin tekrarı yeni bir sipariş açar; eski sipariş İptal edildi olarak kayıtta kalır (K-181). Ödemesi hiç başarılı olmamış iptal siparişi müşterinin listesinde görünmez, firmanın panelinde görünür (K-184).

**5.3.5 Sipariş hesabın kaderinden bağımsızdır:** hesap silinse de kendi kaydıyla yoluna devam eder (K-116).

### 5.4 Sevkiyat ekseni — Sipariş durumu

| Durum | Kod | Anlamı | Terminal |
|---|---|---|---|
| Alındı | `Placed` | Siparişin oluştuğu andaki durum. Ödeme onaylanana kadar sürer — kartta dakikalar, havalede günler (K-166); yalnız hizmet siparişinde firmanın tamamlama işaretine kadar | Hayır |
| Hazırlanıyor | `Preparing` | Ödemesi onaylanmış, fiziksel kalem içeren siparişin firma tarafından hazırlandığı durum; kargoya verme süresi bu anda başlar | Hayır |
| Kargoya verildi | `Shipped` | Kargo şirketi + takip numarasıyla ya da "kendi aracımızla teslim" beyanıyla yola çıkmış sipariş; müşterinin iptal yolu kapanır | Hayır |
| Teslim edildi | `Delivered` | Fiziksel siparişte malın teslimi; yalnız dijital siparişte ödeme onayı; yalnız hizmet siparişinde firmanın tamamlama işareti | **Evet** |
| Teslim edilemedi | `DeliveryFailed` | Kargonun ulaştıramayıp firmaya geri döndürdüğü sipariş | Hayır |
| İptal edildi | `Cancelled` | Teslimattan önce müşterinin, firmanın ya da ödeme süresinin dolmasının kapattığı sipariş | **Evet** |

*Kaynak: K-171 (set), K-176 (Teslim edilemedi), K-223 (terminal durumlar).*

**İzin verilen geçişler — beyaz liste.** Listede olmayan her geçiş yasaktır (K-224).

| # | Geçiş | Koşul ya da tetikleyici | Kaynak |
|---|---|---|---|
| S1 | Alındı → Hazırlanıyor | Ödeme **Ödendi** olur; sipariş en az bir fiziksel kalem taşır | K-173, K-179 |
| S2 | Alındı → Teslim edildi | Yalnız dijital: ödeme onaylanır. Yalnız hizmet: firma "tamamlandı" işaretler | K-174, K-175 |
| S3 | Alındı → İptal edildi | Müşteri iptal eder; firma sebep seçerek iptal eder; ödeme süresi dolar ya da kart ödemesi başarısız olur | K-180, K-196, K-199 |
| S4 | Hazırlanıyor → İptal edildi | Müşteri iptal eder; firma sebep seçerek iptal eder | K-196, K-199 |
| S5 | Hazırlanıyor → Kargoya verildi | Firma kargo şirketi + takip numarası ya da "kendi aracımızla teslim" girer | K-141 |
| S6 | Kargoya verildi → Teslim edildi | Tetikleyicisi karar kaydında yok — **açık: A-06** | K-224 |
| S7 | Kargoya verildi → Teslim edilemedi | Kargo malı ulaştıramayıp firmaya geri döndürür; firma panelden işaretler | K-176 |
| S8 | Teslim edilemedi → Kargoya verildi | Firma yeniden gönderir | K-176 |
| S9 | Teslim edilemedi → İptal edildi | Firma iptal eder | K-176 |

- **Yasak geçişler özellikle:** geri yönlü her geçiş — Kargoya verildi → Hazırlanıyor, Teslim edildi → herhangi biri (K-224). İptal edilmiş bir sipariş yeniden açılmaz (K-223).
- **Kalem düzeyinde iptal:** siparişin tamamı ancak bütün kalemleri iptal edildiğinde İptal edildi'ye geçer (K-197).
- **Yanlış yapılmış geçişin düzeltilmesi** bu tablonun dışındadır ve `B8-14`'ün elle müdahale yetkisine devredilmiştir (K-225).

### 5.5 Ödeme ekseni — Ödeme durumu

| Durum | Kod | Anlamı | Terminal |
|---|---|---|---|
| Bekliyor | `Pending` | Ödemesi henüz gelmemiş sipariş | Hayır |
| Ödendi | `Paid` | Ödemesi onaylanmış sipariş: kartta 3D Secure'dan geçen ödemenin sağlayıcıdan gelen başarı bildirimi, havalede firmanın "ödendi" işareti | Hayır |
| Başarısız | `Failed` | Ödeme süresi dolmuş ya da kart ödemesi başarısız olmuş sipariş; sonradan gelen ödeme işlenmez | **Evet** |
| Kısmen geri ödendi | `PartiallyRefunded` | Kalemlerinin bir kısmının parası müşteriye geri ödenmiş sipariş | Hayır |
| Geri ödendi | `Refunded` | Parasının tamamı müşteriye geri ödenmiş sipariş | **Evet** |

*Kaynak: K-172 (set), K-222 (Kısmen geri ödendi), K-223 (terminal durumlar), K-13 (3D Secure), K-183.*

| # | Geçiş | Koşul ya da tetikleyici | Kaynak |
|---|---|---|---|
| P1 | Bekliyor → Ödendi | Kartta sağlayıcının başarı bildirimi; havalede firmanın panelden "ödendi" işareti (geri alınamaz onayıyla — §5.9) | K-13, K-159, K-173, K-225 |
| P2 | Bekliyor → Başarısız | Ödeme süresi dolar ya da kart ödemesi başarısız olur; kartta iptalden önce sağlayıcıya son sorgu yapılır | K-180, K-232 |
| P3 | Ödendi → Kısmen geri ödendi | Kalemlerin bir kısmının parası geri ödenir | K-222 |
| P4 | Ödendi → Geri ödendi | Siparişin parasının tamamı geri ödenir | K-172, K-222 |
| P5 | Kısmen geri ödendi → Geri ödendi | Kalan kalemlerin de parası geri ödenir | K-222 |

- **Geri ödeme geçişlerini firma yürütür:** para iadesinin işlenmesi geri alınamaz bir geçiştir (§5.9); ödeme ekseni paranın fiilen gönderildiği anı anlatır (K-208, K-225).
- **Yasak geçişler özellikle:** Başarısız → Ödendi — süresi dolmuş siparişe sonradan gelen ödeme işlenmez (K-183, K-224). İptal edilmiş siparişe gelen kart ödemesi sistem tarafından kendiliğinden iade edilir; siparişin durumları değişmez (K-233).
- Sistemde "eksik ödeme" diye bir durum yoktur (K-169).

### 5.6 İki eksenin bağı ve kendiliğinden geçişler

**5.6.1 İki eksen tek bir noktada bağlıdır:** sipariş Hazırlanıyor'a ancak ödeme Ödendi olduğunda geçer. Bu an kargoya verme süresinin başladığı andır — kartta ödeme başarısı, havalede firmanın "ödendi" işareti (K-173, K-168). Bunun dışında eksenler bağımsızdır: geri ödeme, sipariş hangi sevkiyat durumunda olursa olsun ödeme ekseninde yaşanır ve sevkiyat eksenini değiştirmez (K-173, K-223).

**5.6.2 Yalnız dijital siparişte ödemenin onayı sevkiyatı doğrudan Teslim edildi'ye alır** (K-174).

**5.6.3 Ödeme süresi dolduğunda ya da kart ödemesi başarısız olduğunda — hangisi önce gelirse — sipariş kendiliğinden İptal edildi, ödemesi Başarısız olur;** ayrılan stok, hizmet kontenjanı ve kupon hakkı aynı anda serbest kalır. Firmanın elle iptal etmesi beklenmez (K-180). Kart ödemesinde bu iptalden önce sağlayıcıya ödemenin sonucu son bir kez sorulur; ödeme alınmışsa sipariş iptal edilmez ve ödeme Ödendi'ye geçer (K-232).

**5.6.4 Müşteri aynı sepetten yeni bir siparişi onayladığında o sepete bağlı ödenmemiş önceki sipariş kendiliğinden iptal edilir** ve ayırması serbest kalır (K-182).

**5.6.5 Ödenmeden iptal edilen siparişin ödeme ekseni** — müşterinin ya da firmanın iptalinde ve §5.6.4'ün kendiliğinden iptalinde — karar kaydında yazılı değildir; §5.6.3 yalnız süre dolması ve kart başarısızlığı için Başarısız der. **Açık: A-09.**

**5.6.6 Cayma beyanı hiçbir ekseni değiştirmez:** hakkın kullanıldığını kayda geçirir ve iade sürecini başlatır; ödeme ekseni paranın geri ödendiği anda değişir (K-208).

### 5.7 Tipe göre sevkiyat hattı

Üç ürün tipi aynı sepetten geçer ve aynı şekilde satın alınır (K-81); sevkiyat ekseninde ayrıldıkları yer teslimdir. Tipler için ayrı bir durum seti yoktur — hatların farkı **kullanılmayan durumlarla** anlatılır (K-174).

| Siparişin kalemleri | Sevkiyat hattı | Kalem düzeyi | Kaynak |
|---|---|---|---|
| En az bir fiziksel kalem | Alındı → Hazırlanıyor → Kargoya verildi → Teslim edildi | Dijital kalem ödeme onayında indirilebilir olur; hizmet kalemi firma "tamamlandı" işaretlediğinde teslim edilmiş olur — ikisi de siparişin hattını beklemez | K-171, K-179 |
| Yalnız dijital | Alındı → Teslim edildi, ödeme onayında; Hazırlanıyor ve Kargoya verildi kullanılmaz. Havalede teslim firmanın "ödendi" işaretine bağlıdır | — | K-174 |
| Yalnız hizmet | Alındı → Teslim edildi, firma "tamamlandı" işaretlediğinde; ödeme onayı ile tamamlama arasında sipariş Alındı'da bekler | — | K-175 |
| Fiziksel kalemsiz dijital + hizmet; ya da fiziksel kalemlerinin tamamı iptal edilmiş karışık sipariş | Karar kaydında yok — **açık: A-09** | — | — |

### 5.8 Sipariş kalemi düzeyindeki kayıtlar

Kalem ayrı bir durum makinesi taşımaz (K-179); aşağıdaki kayıtları taşır:

- **Teslim işareti ve teslim tarihi** — dijital kalemde ödeme onayında, hizmet kaleminde tamamlama işaretinde konur; cayma penceresi kalemin kendi teslim tarihinden işler (K-179, K-203). Fiziksel kalemin teslim tarihinin kaynağı **A-06**'dadır.
- **İptal kaydı** — kalem tek tek iptal edilir; firma iptalinde sebep kayda geçer (K-178, K-197, K-199).
- **Cayma beyanı** — tarih damgasıyla; pencerenin içinde yapılıp yapılmadığı bu damgadan okunur (K-207).
- **Ayıp talebi** — kalem ve sipariş bağlamıyla (K-227; §5.10).
- **İndirme sayacı** — dijital kalemde (K-146; §3.12.5).

İptal ve iade kalem düzeyinde işler; siparişin kalan kalemleri yoluna devam eder ve para kısmen döner (K-178).

### 5.9 Geri alınamaz geçişler

Dört geçiş panelde "geri alınamaz" onayı ister: **havale siparişinde "ödendi" işareti** (P1) · **kargoya verme** (S5) · **firma kaynaklı iptal** (S3, S4) · **para iadesinin işlenmesi** (P3–P5). Firma işlemi onaylamadan önce sonucunu tek cümleyle görür ("Müşteriye takip numarası gönderilecek. Bu işlem geri alınamaz."). **Panel "geri al" düğmesi sunmaz;** yanlış yapılmış bir geçişin düzeltilmesi `B8-14`'ün elle müdahale yetkisine ve işlem izine devredilmiştir (K-225).

### 5.10 Ayıp talebinin durumu

| Durum | Kod | Anlamı |
|---|---|---|
| Açık | `Open` † | Müşterinin bildirdiği, firmanın henüz çözmediği talep; talep bu durumda doğar |
| Çözüldü | `Resolved` † | Firmanın çözüldü olarak işaretlediği talep |

Firma talebi Çözüldü olarak işaretler; seçimlik hakların — onarım, değişim, bedel indirimi, sözleşmeden dönme — yürütümü sistem dışındadır (K-228). Çözüldü'den Açık'a dönüş karar kaydında tanımlı değildir — **açık: A-09.** Kodlar kayıtta yazılı değildir, §1.2'de † ile türetilmiştir (A-05).

### 5.11 İndirim ve kuponun zamana ve kullanıma bağlı geçerliliği

**5.11.1 İndirim tarih aralığında kendiliğinden işler:** sistem indirimi başlangıçta başlatır, bitişte bitirir; aralık içinde uygulanan indirimli fiyat fiyat geçmişine yazılır ve referans fiyatın hesabına girer (K-63, K-66). Onay anında indirimin başlaması ya da bitmesi fark sayılır (K-129).

**5.11.2 Kupon, tarih aralığının içinde ve kullanım hakkı kaldıkça geçerlidir** (K-70, K-75).

**5.11.3 Kuponun kullanım hakkının ömrü:** sipariş onaylandığında **ayrılır** → ödeme başarılı olduğunda **kullanılmış** sayılır → ödeme süresi dolar, ödeme başarısız olur, siparişin tamamı iptal ya da iade edilirse **geri döner**. Kısmi iptalde ve kısmi iadede hak geri dönmez (K-76, K-128, K-201, K-202, K-216).

**5.11.4 Kuponun kalemlere dağıtılan payı sipariş anında donar** ve iade tutarının hesabında kullanılır (K-72, K-77, K-202).

### 5.12 Hesap ve oturum

**5.12.1 Hesabın durum makinesi yoktur.** Tek tip hesap vardır; doğrulanmamış hesap bir kullanım durumu değil bir bekleme aşamasıdır — e-posta doğrulanana kadar giriş ve sipariş yoktur; doğrulama bağlantısının ömrü dolarsa kayıt silinir (K-101, K-102). Hesap silindiğinde kapanır (§3.15.1).

**5.12.2 Oturum "beni hatırla" seçimine göre kısa ya da uzun ömürlü açılır** (K-108). Şifre sıfırlandığında hesabın bütün oturumları kapanır (K-106); hesap silindiğinde açık oturumlar hemen silinir (K-115).

*Kaynak: K-170…K-176 · K-222…K-225 (sipariş durum makinesi) · K-54 · K-55 · K-251 · K-273…K-275 (yayın durumları) · K-228 (ayıp talebi) · K-09 · K-11 · K-13 · K-17 · K-45 · K-56 · K-57 · K-61 · K-63 · K-66 · K-69 · K-70 · K-72 · K-75 · K-76 · K-77 · K-78 · K-79 · K-80 · K-81 · K-85 · K-86 · K-101 · K-102 · K-106 · K-108 · K-113 · K-115 · K-116 · K-128 · K-129 · K-130 · K-145 · K-146 · K-148 · K-159 · K-163 · K-166 · K-168 · K-169 · K-177 · K-178 · K-179 · K-180 · K-181 · K-182 · K-183 · K-184 · K-186 · K-189 · K-190 · K-196 · K-197 · K-199 · K-201 · K-202 · K-203 · K-207 · K-208 · K-216 · K-227 · K-232 · K-233 · K-235 · K-245 · K-269 · K-276 · K-277 · K-278.*

## 6. Hata ve istisna senaryoları

> **Ne yazılır:** Her ana akış için "ne ters gidebilir" ve sistemin cevabı. Happy path kadar detaylı.

Envanter §2.1'in sekiz akışına göre dizilir; önce bütün akışlara uygulanan ilkeler gelir. Kuralı §3'te ya da §7'de yaşayan senaryonun cevabı burada bir cümleyle yazılır ve kurala bağlanır; kuralın tam metni orada kalır. Kullanıcıya gösterilen metinlerin biçimi ve yeri `04`'ün işidir.

### 6.1 Bütün akışlara uygulanan ilkeler

**6.1.1 Hiçbir akış bir e-postanın ulaşmasına bağlı değildir.** E-posta gönderilemediğinde sipariş, ödeme ve durum geçişleri etkilenmez; müşteri sipariş sayfasında her bilgiye — durum, takip bağlantısı, indirme düğmesi, IBAN — ulaşır. Misafir alıcı sipariş sayfasına sipariş numarası + e-postayla girer. Yeniden deneme davranışı `B8-03`'ün işidir (K-234, K-187).

**6.1.2 Sistem, müşterinin ödediği bir siparişi kendi bilgisizliği yüzünden iptal etmez.** Kart ödemesinde ödeme süresi dolduğunda kendiliğinden iptal işlemeden önce sağlayıcıya ödemenin sonucu son bir kez sorulur; ödeme alınmışsa sipariş iptal edilmez ve Ödendi'ye geçer. Sorgunun teknik biçimi `08`'in işidir (K-232).

**6.1.3 Onaylanan özet ile oluşan sipariş birebir aynıdır.** Özeti değiştiren her fark siparişin oluşmasını durdurur ve müşteriye neyin değiştiği söylenir (§3.17.2; K-129).

**6.1.4 Sepet hatalarda korunur.** Ödeme yarıda kaldığında, ödeme sağlayıcısına erişilemediğinde ve satış kapalıyken sepet olduğu gibi durur (K-127, K-231, K-235).

### 6.2 Satın alma (akış 1)

| # | Ne ters gider | Sistemin cevabı | Kaynak |
|---|---|---|---|
| 6.2.1 | Kart ödemesinde 3D Secure doğrulaması başarısız olur ya da kart ödemesi reddedilir | Ödeme gerçekleşmez ve sipariş ödenmiş sayılmaz. Sipariş kendiliğinden İptal edildi, ödemesi Başarısız olur; ayrılanlar aynı anda serbest kalır. Sepet olduğu gibi durur; müşteri sepetten yeni bir siparişle yeniden dener | K-13, K-127, K-180, K-181 |
| 6.2.2 | Müşteri banka ekranını kapatır ya da ödeme yarıda kalır | Sepet olduğu gibi durur. Ödeme süresi dolunca kartta sonuç son kez sorulur (§6.1.2); ödeme yoksa sipariş kendiliğinden iptal olur ve ayrılanlar serbest kalır. Müşteri aynı sepetten yeniden onaylarsa önceki ödenmemiş sipariş hemen iptal edilir; müşteri kendi ayırdığı stoğa takılmaz | K-127, K-180, K-182, K-232 |
| 6.2.3 | Onay anında fiyat, indirim, kupon, kargo ücreti, ücretsiz kargo eşiği ya da teslimat illeri değişmiştir veya bir kalem ayrılamaz | Sipariş oluşmaz; müşteri neyin değiştiğini söyleyen güncel özeti görür ve isterse yeniden onaylar. Ayrılamayan kalem için sayı söylenmez (§3.17.2) | K-129, K-133, K-150 |
| 6.2.4 | Sepetteki kalem tükenmiştir ya da hizmetin kontenjanı dolmuştur | Kalem "Tükendi" işaretiyle sepette kalır ve siparişe girmez; sipariş kalan kalemlerle devam eder ve fark güncel özette söylenir (§3.16.6) | K-130 |
| 6.2.5 | Sepetteki ürün ya da varyant arşive ya da taslağa alınmış veya silinmiştir | Kalem sepetten çıkar ve müşteriye bir kez söylenir (§3.16.7) | K-130 |
| 6.2.6 | Müşteri stoktan fazla adet seçer ya da stok sepetteki adedin altına düşer | "Bu adette stok yok" — adet söylenmez; kalem adet düşürülene kadar siparişe girmez (§3.16.8) | K-150 |
| 6.2.7 | Müşteri bir siparişte alınabilecek adedi aşar | Ürün sepete eklenmez ve sınır sayısıyla söylenir; sonradan aşılan sınırda kalemler mesajla sepette kalır (§3.16.9, §3.16.10) | K-151, K-152 |
| 6.2.8 | Aynı dijital varyant sepete ikinci kez eklenir | "Bu ürün zaten sepetinde" (§3.12.4) | K-153 |
| 6.2.9 | Üye, daha önce aldığı dijital varyantı yeniden sepete ekler | Engellenmez; "Bu ürünü daha önce aldınız" uyarısı gösterilir (§3.12.9) | K-236 |
| 6.2.10 | Teslimat adresi firmanın teslimat yapmadığı bir ildedir | Sipariş verilemez ve sebebi söylenir; defterdeki o adres ödeme adımında seçilemez (§3.20.1) | K-133 |
| 6.2.11 | Fiziksel kalemli sepetin tutarı asgari sipariş tutarının altındadır | Sipariş verilemez; eksik tutar gösterilir (§3.18.1) | K-155 |
| 6.2.12 | Kupon sepet toplamını aşar, tarih aralığının dışındadır ya da kullanım hakkı dolmuştur | Kupon kabul edilmez (§3.10.4, §3.10.5) | K-73, K-75 |
| 6.2.13 | Misafir alıcı e-posta adresini yanlış yazar | Onay özetinde adres açıkça gösterilir ve onaydan önce düzeltilebilir; doğrulama kodu istenmez (§3.24.5). Hata onaydan sonra fark edilirse e-posta ulaşmaz ama hiçbir akış e-postaya bağlı değildir (§6.1.1) | K-193, K-234 |
| 6.2.14 | Hesabı olan kişi giriş yapmadan misafir olarak sipariş verir | Sipariş kabul edilir ve hesaba anında düşer; ödeme adımında giriş hatırlatması gösterilir (§3.13.3) | K-99 |
| 6.2.15 | Ödeme sağlayıcısına erişilemez | Kart yöntemi "şu an kullanılamıyor" olarak görünür ve seçilemez. Havale açıksa müşteri ona yönlendirilir; değilse sipariş verilemez ve müşteri daha sonra denemesi söylenerek sepetine döner — sepet korunur. Sağlayıcıya bağlanılamadığı anda ayrılmış bir şey varsa hemen serbest kalır. Erişilemezliğin tespiti `08`'in işidir | K-231 |
| 6.2.16 | İptal edilmiş bir siparişe sağlayıcıdan başarılı ödeme bildirimi gelir — iki sekmeden aynı sepetle ödeme, kendiliğinden iptalden sonra tamamlanan eski deneme | Sistem ödemeyi sağlayıcı üzerinden kendiliğinden iade eder ve firmaya panelde bildirir; siparişin durumları değişmez. Havale hattında aynı durum firmanın elle iadesiyle çözülür (§3.21.8). Sağlayıcıya iade çağrısının biçimi `08`'in işidir | K-233, K-183 |
| 6.2.17 | Satış kapalıdır — kimlik eksik, açık ödeme yöntemi yok ya da firma satışı geçici olarak kapatmış | Ürünler görünür; sepete ekleme ve ödeme kapalıdır ve ziyaretçi satışın kapalı olduğunu görür; sepet korunur (§3.1.5) | K-09, K-163, K-235 |

### 6.3 Sipariş takibi (akış 2)

| # | Ne ters gider | Sistemin cevabı | Kaynak |
|---|---|---|---|
| 6.3.1 | Sipariş e-postası müşteriye ulaşmaz | Müşteri her bilgiye sipariş sayfasından ulaşır; misafir alıcı sayfaya sipariş numarası + e-postayla girer (§6.1.1) | K-234, K-187 |
| 6.3.2 | Misafir alıcı sipariş numarasını ya da e-postayı yanlış girer | Sipariş sayfası açılmaz; takip formunun deneme limiti `B8-05`'in işidir (§3.22.3) | K-187 |
| 6.3.3 | Hesap silinmiştir ama sipariş yürümektedir | Takip, cayma ve iade misafir yolundan — sipariş numarası + siparişe donmuş e-posta — sürer (§3.15.2) | K-116 |
| 6.3.4 | Müşterinin ödemesi hiç başarılı olmamış iptal siparişleri vardır | Müşterinin sipariş listesinde görünmezler; firmanın panelinde görünürler (§3.17.8) | K-184 |

### 6.4 İptal, cayma ve iade (akış 3)

| # | Ne ters gider | Sistemin cevabı | Kaynak |
|---|---|---|---|
| 6.4.1 | Müşteri, sipariş kargoya verildikten sonra iptal etmek ister | İptal yolu kapanmıştır; müşteri teslimden sonra cayma yolunu kullanır (§7.2.1) | K-196 |
| 6.4.2 | Firma kargoya verme süresini ya da yasal 30 günlük üst sınırı aşar | Sipariş henüz kargoya verilmediği için müşterinin iptal düğmesi açıktır; fesih hakkı o düğmeyle kullanılır (§7.2.3) | K-198 |
| 6.4.3 | Müşteri ödemesi onaylanmış dijital kalemi iptal etmek ya da ondan caymak ister | İptal edilemez — kalem teslim edilmiştir; ön onayla cayma hakkı da düşmüştür (§7.2.5, §7.3.2) | K-200, K-204 |
| 6.4.4 | Müşteri cayma istisnası işaretli bir üründen cayar | Cayma hakkı yoktur; istisna ve sebebi ön bilgilendirmede gösterilmiştir (§7.3.4) | K-206 |
| 6.4.5 | Müşteri tamamlanmış bir hizmetten cayar | Hak, firma hizmeti "tamamlandı" işaretlediğinde düşmüştür (§7.3.3) | K-205 |
| 6.4.6 | On dört günlük cayma penceresi geçmiştir | Cayma hakkı yoktur; sebepli bir sorun ayıp talebi yolundan yürür (§7.3.1, §7.5.1) | K-203, K-226 |
| 6.4.7 | Havale ile ödenmiş siparişin parası geri ödenecektir | Müşteri IBAN'ını iptal ya da cayma beyanı sırasında girer; kart hattında IBAN istenmez (§7.4.5) | K-214 |
| 6.4.8 | Firma iade edilen malı teslim almadan geri ödemeyi bekletir | Bekletebilir; ama on dört günlük çatı beyandan işler ve durmaz, panel kalan süreyi gösterir (§7.4.1) | K-209 |
| 6.4.9 | Gönderi, müşterinin adresi yanlış vermesi ya da teslimatta bulunmaması yüzünden geri döner ve sipariş iptal edilir | Ürün bedeli iade edilir, gidiş kargo bedeli edilmez; firma kaynaklı sebepte gidiş kargosu da iade edilir (§7.2.9) | K-176, K-212 |
| 6.4.10 | Kısmi iptal ya da iadeden sonra kalan tutar kuponun asgari tutarının ya da ücretsiz kargo eşiğinin altına düşer | Kupon ve kargo yeniden hesaplanmaz; müşteriden bir şey geri istenmez (§7.2.7, §7.4.6) | K-202, K-213 |

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

### 6.6 Katalog yönetimi (akış 5)

| # | Ne ters gider | Sistemin cevabı | Kaynak |
|---|---|---|---|
| 6.6.1 | İndirimli fiyat referans fiyatın altına inmez | Sistem indirimi kaydetmez ve yöneticiyi sebebiyle uyarır (§3.9.5) | K-68 |
| 6.6.2 | Firma içinde ürün ya da alt kategori bulunan bir kategoriyi siler | Silme engellenir; engelleyenler sayısıyla gösterilir (§3.4.5) | K-46 |
| 6.6.3 | Taşıma, üç seviyeyi aşan bir dal üretir ya da kategori kendi alt ağacına taşınır | Taşıma yapılamaz (§3.4.6) | K-46 |
| 6.6.4 | Yayın kapısının koşullarından biri eksiktir — ad, ana kategori, tip ya da arşivlenmemiş varyant | Ürün yayına alınamaz (§3.7.3) | K-86, K-55 |
| 6.6.5 | Dijital ürünün yayındaki bir varyantı dosyasızdır | Ürün yayına alınamaz; panel dosyası eksik varyantı gösterir (§3.7.4) | K-145 |
| 6.6.6 | Yayındaki dijital ürünün dosyasını silmek bir varyantı dosyasız bırakır | Silme engellenir; firma önce yeni dosyayı yükler ya da varyantı yayından çeker (§3.7.4) | K-145 |
| 6.6.7 | Fiyata ikiden fazla ondalık girilir | Panel kabul etmez (§3.8.4) | K-62 |
| 6.6.8 | Sabit tutarlı kuponun asgari sepet tutarı kupon tutarının altında girilir | Girilemez (§3.10.4) | K-73 |
| 6.6.9 | Ürünün kargoya verme süresi üst çiti aşar | Panel çitin üstünde değer kabul etmez (§3.20.5) | K-139, K-140 |
| 6.6.10 | İki yönetici aynı kaydı aynı anda düzenler | İkinci kaydeden kaydın değiştiğini görür ve üzerine yazmadan önce uyarılır (§3.31.1) | K-270 |

### 6.7 Sipariş yürütümü (akış 6)

| # | Ne ters gider | Sistemin cevabı | Kaynak |
|---|---|---|---|
| 6.7.1 | Havalede gelen tutar sipariş tutarından farklıdır | Sistem tutar sormaz; firma "ödendi" işaretler ya da farkı müşteriyle sistem dışında çözer (§3.21.7) | K-169 |
| 6.7.2 | Havale, ödeme süresi dolup sipariş iptal edildikten sonra gelir | İptal edilmiş sipariş "ödendi" işaretlenemez; firma parayı iade eder ya da müşteriden yeni sipariş ister (§3.21.8) | K-183 |
| 6.7.3 | Kargo gönderiyi ulaştıramayıp firmaya geri döndürür | Firma siparişi Teslim edilemedi olarak işaretler; buradan yeniden gönderir ya da iptal eder (§5.4 — S7, S8, S9) | K-176 |
| 6.7.4 | Firma kargoya verirken takip numarası girmez | Kargo şirketi + takip numarası ya da "kendi aracımızla teslim" beyanı olmadan işaret konamaz (§3.20.7) | K-141 |
| 6.7.5 | Firma bir geçişi yanlışlıkla yapar | Panel "geri al" sunmaz; dört geri alınamaz geçiş önceden onay ister; düzeltme `B8-14`'e devredilmiştir (§5.9) | K-225 |
| 6.7.6 | Firma siparişi karşılayamaz — stok hatası, ürünün bulunamaması, teslimatın mümkün olmaması | Firma sebep seçerek iptal eder; sebep müşteriye bildirilir ve işlem izine yazılır (§7.2.4) | K-199 |
| 6.7.7 | Dijital üründe müşterinin indirme hakkı dolar | Müşteri firmaya başvurur; firma o kalemin hakkını panelden yeniler (§3.12.6) | K-147 |
| 6.7.8 | Satılan dijital dosya ayıplıdır | Firma dosyayı düzeltip yeniden yükler; düzeltme eski alıcılara da ulaşır (§7.5.4) | K-229, K-148 |
| 6.7.9 | İptal edilmiş siparişe kart ödemesi gelir | Sistem kendiliğinden iade eder ve firmaya panelde bildirir (§6.2.16) | K-233 |

### 6.8 Kurumsal içerik (akış 7)

| # | Ne ters gider | Sistemin cevabı | Kaynak |
|---|---|---|---|
| 6.8.1 | Zorunlu alanı eksik kayıt yayına alınmak istenir | Yayına alınamaz; panel eksik alanı gösterir (§3.27.24) | K-275 |
| 6.8.2 | Firma Hakkımızda'yı silmek ister | Hakkımızda silinmez, yalnız taslağa alınır (§3.27.3) | K-277 |
| 6.8.3 | Hakkımızda boştur ya da yayında değildir | Ana sayfanın kurumsal bloğu marka adını ve logoyu gösterir; boş kutu görünmez (§3.27.14) | K-250 |
| 6.8.4 | İçeriğe bağlı ürün taslağa ya da arşive alınır veya silinir | Taslakta ve arşivde ürün bağda kalır ama görünmez; silinen ürün bağdan kalkar (§3.27.10) | K-245 |
| 6.8.5 | Ziyaretçi silinmiş ya da taslaktaki bir içeriğin adresine gelir | "Sayfa bulunamadı" (§3.30.4) | K-274, K-277, K-281 |
| 6.8.6 | İki yönetici aynı kaydı aynı anda düzenler | İkinci kaydeden uyarılır (§3.31.1) | K-270 |

### 6.9 Mağaza ayarları (akış 8)

| # | Ne ters gider | Sistemin cevabı | Kaynak |
|---|---|---|---|
| 6.9.1 | Firma tipinin zorunlu kimlik alanları eksiktir ya da firma tipi değişmiş ve yeni setin alanları boştur | Satış açılmaz; kurumsal taraf yayında kalır (§3.1.3, §3.1.5) | K-09, K-14 |
| 6.9.2 | Havale açılmak istenir ama IBAN boştur | Havale açılamaz (§3.21.5) | K-161 |
| 6.9.3 | Ödeme sağlayıcı anahtarları kurulumda tanımlı değildir | Kart yöntemi görünmez; havale açıksa satış yalnız havaleyle açıktır (§3.21.4) | K-162, K-163 |
| 6.9.4 | Hiçbir ödeme yöntemi açık değildir | Satış açılmaz (§3.1.5) | K-163 |
| 6.9.5 | Havale süresi ya da varsayılan kargoya verme süresi üst çiti aşar | Panel çitin üstünde değer kabul etmez (§3.17.4, §3.20.4) | K-139, K-166 |
| 6.9.6 | Marka adı girilmemiştir | Site alan adını gösterir (§3.29.3) | K-260 |
| 6.9.7 | Logo yüklenmemiştir | Marka adı yazıyla gösterilir (§3.29.2) | K-259 |
| 6.9.8 | Site simgesi yüklenmemiştir | Marka adının baş harfi marka rengi zemin üzerinde gösterilir (§3.29.4) | K-261 |
| 6.9.9 | Marka rengi seçilmemiştir ya da üstündeki yazıyı okunmaz kılacak bir renk seçilmiştir | Renk seçilmemişse ürünün varsayılan rengi kullanılır; üstündeki yazının rengini sistem kontrasta göre seçer (§3.29.5) | K-262 |

*Kaynak: K-231 · K-232 · K-233 · K-234 (`B6-15` — hata ve istisna envanteri) · K-13 · K-68 · K-129 · K-130 · K-133 · K-150 · K-151 · K-169 · K-193 — ve tabloların kaynak sütunundaki satırlar.*

## 7. İptal, itiraz ve geri alma

> **Ne yazılır:** Kullanıcının bir işlemi geri alma yolları, koşulları, sınırları.

### 7.1 Üç yol ve ortak ilkeler

**7.1.1 Müşterinin satın almadan sonra başvurabileceği üç ayrı yol vardır.** **İptal** teslimattan önce işler ve siparişin henüz yola çıkmamış olmasına dayanır. **Cayma** teslimattan sonra işler; sebepsizdir, yasal penceresi ve tipe özel istisnaları vardır. **Ayıp talebi** sebeplidir ve teslimden itibaren yasal süre boyunca açıktır. Üçü arayüzde aynı düğme değildir ve kayıtta aynı sebep kodunu paylaşmaz (K-195, K-226).

**7.1.2 Tek hukuki rejim tüketici rejimidir.** Alıcı istisnasız tüketicidir; 6502 sayılı Kanun'un korumaları her siparişte dallanmadan uygulanır (K-07).

**7.1.3 İptal ve iade kalem düzeyinde işler.** Sipariş kalemi tek tek iptal ve iade edilebilir; siparişin kalan kalemleri yoluna devam eder ve para kısmen döner. Gerekçe tercih değil **hak**tır: üç üründen birini beğenmeyen tüketici üçünü birden iade etmeye zorlanamaz (K-178).

**7.1.4 Üç yol da sipariş sayfasından kullanılır;** misafir alıcı sayfaya §3.22.3'ün yollarından girer (K-207, K-227, K-187).

**7.1.5 İtiraz — uyuşmazlık çözümü ve şikâyet kanalının sistemdeki yeri — `B8-13`'ün konusudur;** bu bölümün itiraz ayağı o kararla yazılır.

### 7.2 İptal

Ödenmemiş siparişin kendiliğinden iptali §3.17.5 ve §3.17.7'dedir.

**7.2.1 Müşteri, sipariş "Kargoya verildi"ye geçene kadar kendi iptal eder** — sipariş sayfasından, firma onayı beklemeden; iptal kendiliğinden işler. Kargoya verildikten sonra iptal yolu kapanır ve cayma devreye girer. Bedeli bilinçlidir: firma, hazırlamaya başladığı bir siparişin iptalini engelleyemez (K-196).

**7.2.2 İptal kalem düzeyindedir.** Müşteri siparişin tek bir kalemini iptal eder, kalan kalemler yoluna devam eder; siparişin tamamı ancak bütün kalemleri iptal edildiğinde İptal edildi'ye geçer (K-197).

**7.2.3 Teslimat gecikmesinde fesih için ayrı bir mekanizma yoktur.** Firma kargoya verme süresini ya da yasal 30 günlük üst sınırı aştığında sipariş tanımı gereği hâlâ kargoya verilmemiştir; müşterinin iptal düğmesi açıktır ve fesih hakkı o düğmeyle kullanılır (K-198).

**7.2.4 Firma da iptal edebilir** — stok hatası, ürünün bulunamaması, teslimatın mümkün olmaması gibi durumlarda. **Sebep seçmek zorunludur:** firma kapalı bir sebep listesinden seçer; sebep müşteriye bildirilir ve kim/ne zaman bilgisiyle işlem izine yazılır. Sebep listesinin içeriği `B8-14`'ün işidir (K-199). Firma iptali geri alınamaz onayı ister (§5.9).

**7.2.5 Dijital kalem ödeme onayından sonra iptal edilemez:** ödeme onaylandığında kalem teslim edilmiş sayılır ve bundan sonrası cayma konusudur (§7.3.2). Ödeme onayından **önce** — havale beklerken — sipariş bütünüyle iptal edilebilir ve indirme hiç açılmamış olur. **Hizmet kalemi**, firma "tamamlandı" işaretleyene kadar iptal edilebilir (K-200).

**7.2.6 İptalde sayaçlar kendiliğinden döner.** İptal edilen kalemin adedi stoğa, hizmet kontenjanı havuza döner; kuponun kullanım hakkı yalnız siparişin **tamamı** iptal edildiğinde sayaca döner (K-201, K-202). Dijital ürünün dönecek sayacı yoktur (K-83).

**7.2.7 Kısmi iptalde kupon yeniden hesaplanmaz.** İptal edilen kalemin kupon payı iade tutarına dahil edilir; kalan kalemler kendi indirimlerini korur ve kuponun kullanım hakkı geri dönmez — sipariş hâlâ yaşıyordur. Kalan tutarın kuponun asgari sepet tutarını karşılayıp karşılamadığına bakılmaz (K-202).

**7.2.8 İptal edilen ödenmiş kalemin parası, ödemenin geldiği yoldan geri ödenir** (§7.4.5; K-214). Geri ödemeyi kimin işlediği — sistemin kendiliğinden mi, firmanın panelden mi — ve hangi süre içinde işlendiği karar kaydında yazılı değildir; on dört günlük çatı (§7.4.1) yalnız cayma için kurulmuştur. **Açık: A-07.**

**7.2.9 Geri dönen gönderide iptal edilen siparişte ürün bedeli iade edilir, gidiş kargo bedeli edilmez** — sebep müşterinin adresi yanlış vermesi ya da teslimatta bulunmamasıysa; taşıma fiilen yapılmıştır. Firma kaynaklı sebeplerde — yanlış adrese gönderim, hasarlı paket — gidiş kargosu da iade edilir; ayrım firma iptalinin sebep kaydından okunur (K-212, K-176, K-199).

### 7.3 Cayma

**7.3.1 Cayma penceresi on dört gündür ve sabittir;** firma uzatamaz ve süre §11'e girmez. Başlangıç anı teslim tarihidir; iade kalem düzeyinde işlediği için pencere her kalem için kendi teslim tarihinden bağımsız işler (K-203). Fiziksel kalemin teslim tarihinin kaynağı karar kaydında yazılı değildir — **açık: A-06** (§3.20.11).

**7.3.2 Dijital üründe cayma hakkı yoktur; koşulu ayrı bir onaydır.** Mesafeli Sözleşmeler Yönetmeliği'nin "elektronik ortamda anında ifa edilen ve tüketiciye anında teslim edilen gayrimaddi mal" istisnası uygulanır. İstisna kendiliğinden işlemez: sepette dijital kalem varsa onay adımında üçüncü bir kutu çıkar ve kutu işaretlenmeden dijital kalem siparişe giremez (§3.24.2). Hak ödeme başarısında — indirmenin açıldığı anda — düşer (K-204).

**7.3.3 Hizmette cayma hakkı ifa tamamlanınca düşer.** Firma hizmet kalemini "tamamlandı" işaretleyene kadar müşteri cayabilir; işaretlendikten sonra Yönetmelik'in "ifası tüketicinin onayıyla başlamış hizmet" istisnası işler ve hak düşer. Tamamlanmamış hizmette pencere on dört gündür (K-205). Hizmetin teslim tarihi olmadığı için bu on dört günün hangi andan sayıldığı karar kaydında yazılı değildir — **açık: A-06.**

**7.3.4 Fiziksel üründe cayma istisnası ürün düzleminde isteğe bağlı bir işarettir.** Ürün formunda bir "cayma hakkı istisnası" kutusu ve kapalı bir sebep listesi bulunur: kişiye özel üretim · çabuk bozulan ya da son kullanma tarihi geçebilecek mal · ambalajı açıldığında iade edilemeyen mal. İşaretli ürün istisnayı ve sebebini ön bilgilendirmede gösterir. İşaret varyanta inmez, ürünün bütün varyantlarını kapsar; yayın kapısına girmez (K-206).

**7.3.5 Cayma sipariş sayfasından bildirilir.** Müşteri kalemi seçip cayma beyanında bulunur; bildirim **tarih damgasıyla** kayda geçer ve pencerenin içinde yapılıp yapılmadığı bu damgadan okunur. Ayrı bir form doldurma, e-posta yazma ya da noter kanalı zorunluluğu yoktur (K-207). Havale ile ödenmiş siparişte müşteri geri ödeme için IBAN'ını beyan sırasında girer (§7.4.5).

**7.3.6 Cayma beyanı siparişi kapatmaz.** Beyan hakkın kullanıldığını kayda geçirir ve iade sürecini **başlatır**; sevkiyat durumu değişmez, ödeme durumu paranın geri ödendiği anda değişir (K-208; §5.6.6).

### 7.4 İade ve geri ödeme

**7.4.1 On dört günlük geri ödeme çatısı cayma beyanı anında başlar.** Sistem sayacı beyanın tarih damgasından başlatır ve firmaya panelde kalan süreyi gösterir. Firma malı teslim alana kadar ödemeyi **bekletebilir** — Yönetmelik'in satıcıya tanıdığı tevkif hakkı — ama çatı beyandan işlemeye devam eder ve durmaz (K-209).

**7.4.2 Cayma hâlinde iade kargo bedeli firmaya aittir;** panelde bunu değiştiren bir ayar yoktur ve ön bilgilendirmede böyle yazar (K-210).

**7.4.3 Ayıplı malda iade kargo bedeli her hâlükârda firmaya aittir.** Ayıplı mal ve garanti kapsamındaki geri gönderimlerde masraf cayma rejiminden bağımsız olarak firmanındır; dayanağı ayıplı mal hükümleridir (K-211).

**7.4.4 Malın firmaya geri gönderilme biçimi** — cayma beyanından sonra müşterinin malı nasıl ve nereye göndereceği, bunun müşteriye nasıl söyleneceği — karar kaydında yazılı değildir: K-208 bu parçayı `B6-10`'a bırakmış, `B6-10` masrafı (§7.4.2) ve teslim alınan malın stoğa dönüşünü (§7.4.7) karara bağlamış ama gönderimin yolunu bağlamamıştır. **Açık: A-08.**

**7.4.5 Geri ödeme, ödemenin geldiği yoldan gider.** Kartla ödenen sipariş **karta** geri ödenir (sağlayıcı üzerinden); havale ile ödenen sipariş müşterinin **IBAN'ına** gönderilir. Havale hattında müşteri IBAN'ını iptal ya da cayma beyanı sırasında girer; kart hattında IBAN alanı yoktur ve istenmez (K-214). Para iadesinin işlenmesi geri alınamaz onayı ister (§5.9).

**7.4.6 Kısmi iadede kargo ücreti ve ücretsiz kargo kararı yeniden hesaplanmaz.** Sipariş anında hesaplanan kargo ücreti ya da ücretsiz kargo donmuştur; bir kalem iade edildiğinde kalan tutar eşiğin altına düşse bile kargo ücreti müşteriden geri istenmez ve iade tutarından düşülmez (K-213). Cayma hâlinde teslimat masrafı da tüketiciye iade edilir (K-212); siparişin tek kargo ücretinin **kısmi** caymada iade edilip edilmediği karar kaydında yazılı değildir — **açık: A-07.**

**7.4.7 İade edilen fiziksel kalemin adedi stoğa kendiliğinden eklenmez.** Firma malı teslim alır, durumunu kontrol eder ve panelden stoğa ekler. İptalden bilinçli ayrışmadır: iptalde mal hiç yola çıkmadığı için stok kendiliğinden döner, iadede mal gitmiş ve geri gelmiştir (K-215).

**7.4.8 İadede hizmet kontenjanı havuza döner; kuponun kullanım hakkı yalnız siparişin tamamı iade edildiğinde sayaca döner.** Kısmi iadede hak dönmez (K-216, K-202). Dijital ürünün dönecek sayacı yoktur ve zaten cayma kapsamı dışındadır (K-204).

### 7.5 Ayıp talebi

**7.5.1 Ayıp talebi üçüncü yoldur.** Cayma sebepsizdir ve on dört günle sınırlıdır; ayıp talebi **sebeplidir** ve teslimden itibaren yasal süre — **iki yıl** — boyunca açıktır. Süre yasal bir sabittir ve §11'e girmez (K-226).

**7.5.2 Kanal sipariş sayfasındaki "sorun bildir"dir.** Müşteri kalemi seçer ve açıklama yazar; talep kalem ve sipariş bağlamıyla birlikte firmanın paneline düşer. Fotoğraf ya da dosya yükleme yoktur — firma kanıt gerekiyorsa müşteriden e-postayla ister (K-227).

**7.5.3 Sistem talebi kaydeder ve iletir, çözümü yürütmez.** Talebin iki durumu vardır: Açık ve Çözüldü (§5.10); firma talebi çözüldü olarak işaretler. Kanunun tüketiciye tanıdığı seçimlik haklar — onarım, değişim, bedel indirimi, sözleşmeden dönme — firmanın müşteriyle sistem dışında yürüttüğü süreçlerdir. Çözüm para iadesi gerektiriyorsa §7.4'ün hattı kullanılır; kalem düzeyinde iade zaten kuruludur (K-228).

**7.5.4 Ayıplı dijital üründe doğal çözüm dosya güncellemesidir.** Talep aynı kanaldan gelir; firma dosyayı düzeltip ürüne yeniden yükler ve düzeltilmiş dosya eski alıcılara da ulaşır (§3.12.7). Bedel iadesi gerekirse §7.5.3'ün hattı işler (K-229, K-148).

Garanti bilgisinin yeri §3.11.7'dedir (K-230).

### 7.6 Geri alma sınırları

**7.6.1 Panel "geri al" düğmesi sunmaz.** Dört geçiş — havalede "ödendi" işareti, kargoya verme, firma kaynaklı iptal, para iadesinin işlenmesi — önceden geri alınamaz onayı ister; yanlış yapılmış bir geçişin düzeltilmesi `B8-14`'e devredilmiştir (§5.9; K-225).

**7.6.2 Süresi dolup iptal edilmiş siparişe gelen ödeme işlenemez;** firma parayı iade eder ya da müşteriden yeni sipariş ister (§3.21.8; K-183).

**7.6.3 İptal edilmiş ya da teslim edilmiş sipariş yeniden açılmaz;** sevkiyat ekseninde geri yönlü geçiş yoktur (§5.4; K-223, K-224).

**7.6.4 Kurumsal içerikte eski hâle dönülmez ve silme geri alınmaz:** sürüm geçmişi yoktur, silme geri alınamaz işlem uyarısıyla yapılır (§3.27.26, §3.27.27; K-276, K-277).

**7.6.5 Silinmiş bir hesabın siparişleri hiçbir hesaba geri bağlanmaz** (§3.15.3; K-117).

*Kaynak: K-195 · K-226 (üç yol) · K-07 (tek rejim) · K-178 (kalem düzeyi) · K-196…K-202 (iptal) · K-203…K-208 (cayma) · K-209…K-216 (iade ve geri ödeme) · K-226…K-230 (ayıp talebi) · K-83 · K-117 · K-148 · K-176 · K-183 · K-187 · K-223 · K-224 · K-225 · K-276 · K-277.*

## 8. Kötüye kullanım ve güvenlik kuralları

> **Ne yazılır:** Kötüye kullanım senaryoları ve karşı önlemler. Teknik güvenlik değil, **ürün düzeyinde** kurallar (limit, cooldown, doğrulama zorunluluğu).

> **Yazım kapısı (K-28):** **Blok 8 kapandığında** yazılır — besleyen bloklar: 4 ✓, 6 ✓, 8. Girdi: karar kaydında etki sütunu `02 §8` taşıyan satırlar ve `B8-05`'e bırakılan devirler. *Liste 2026-09-19'da hizalandı; önceki not "4 ve 8" diyordu.*

## 9. Bildirimler

> **Ne yazılır:** Hangi olayda kime bildirim gider. Kanal detayı `08`'de; burada **tetikleyici ve alıcı**.

> **Yazım kapısı (K-28):** **Blok 8 kapandığında** yazılır — besleyen bloklar: 5 ✓, 6 ✓, 7 ✓, 8. Girdi: karar kaydında etki sütunu `02 §9` taşıyan satırlar ve `B8-03`'e bırakılan devirler. *Liste 2026-09-19'da hizalandı; önceki not yalnız Blok 8'i yazıyordu.*

## 10. Yönetim (admin) kuralları

> **Ne yazılır:** Yöneticinin yapabildikleri ve yapamadıkları. **"Sonra ekleriz" yaklaşımı riskli** — yönetim akışları kullanıcı akışları kadar karmaşıktır, aynı derinlikte ele alınır.

> **Yazım kapısı (K-28):** **Blok 9 kapandığında** yazılır — besleyen bloklar: 1 ✓, 2 ✓, 3 ✓, 4 ✓, 6 ✓, 7 ✓, 8, 9; Blok 9'un katkısı toplu veri işlemleridir (`B9-14`). Girdi: karar kaydında etki sütunu `02 §10` taşıyan satırlar ve `B8-02`, `B8-14`, `B9-14`'e bırakılan devirler. *Liste 2026-09-19'da hizalandı; önceki not "7 ve 8" diyordu ve kapı Blok 8'di — §6.2 sütununun düzeltilmesiyle Blok 9'a geçti.*

## 11. Sayısal parametreler (özet)

> **Ne yazılır:** Dokümanın her yerine dağılmış sayıların tek tablosu. Diğer dokümanlar bu tabloyu referans alır.

> **Yazım kapısı (K-28):** **Blok 9 kapandığında** yazılır — besleyen bloklar: 3 ✓, 5 ✓, 6 ✓, 7 ✓, 9; envanterin kendisi `B9-08`'dir. Girdi: karar kaydında etki sütunu `02 §11` taşıyan satırlar ve §3'te "değeri §11'dedir" diyen her kural. *Liste 2026-09-19'da hizalandı; önceki not "3, 5, 9" diyordu ve yalnız Blok 3'ün kalemlerini sayıyordu.*

| # | Parametre | Değer | Nerede kullanılır | Runtime ayarlanabilir |
|---|---|---|---|---|

## 12. Uyumluluk ve yasal

> **Ne yazılır:** Yasal yükümlülükler, veri saklama/silme hakları, erişim kısıtları.

> **Yazım kapısı (K-28):** **Blok 8 kapandığında** yazılır — besleyen bloklar: 1 ✓, 3 ✓, 4 ✓, 5 ✓, 6 ✓, 7 ✓, 8. Girdi: karar kaydında etki sütunu `02 §12` taşıyan satırlar; K-257'nin firmaya yüklediği yükümlülük (kurumsal içerikte üçüncü kişilere ait kişisel veri) bu bölüme yazılır. *Liste 2026-09-19'da hizalandı; önceki not "1, 4, 6, 8" diyordu.*

## 13. Açık kararlar

> **Ne yazılır:** Bilinçli olarak ileriye bırakılan **detaylar** ve ne zaman karara bağlanacakları. Varlık kararı burada asla açık kalmaz.

> **Yazım kapısı (K-28):** **Blok 9 kapandığında** yazılır. Dönem boyunca açık kalan detaylar `PRODUCT_DISCOVERY_STATUS.md` §4'te izlenir; bu tablo aşama kapanışında oradan devralınır.

| # | Konu | Ne belirsiz | Ne zaman karara bağlanır |
|---|---|---|---|

---

*Shopfolio — Product Requirements v0.7 (§1–§3 ve §5–§7 taslak; §4 ve §8–§13 kendi yazım kapılarında yazılır)*
