# Shopfolio — Product Requirements

**Versiyon: v0.3** | **Bağımlılıklar:** `01_PROJECT_VISION.md`, `PRODUCT_DISCOVERY_STATUS.md` | **Son güncelleme:** 2026-09-16

> **Aşama:** 1 — Product Discovery · **Rol:** Product Manager
> **Traceability zorunlu:** Hayır (kaynak doküman — sonraki aşamalar buraya izlenir)
> **Bu doküman tüm iş kurallarının tek kaynağıdır.** Kod bu dokümanla çelişemez.

> **Taslak durumu (K-28):** §1 **taslak** olarak yazılmıştır; yazıldığı anda §6.2'nin "Hedef doküman" sütunu bu bölümü Blok 1 ve Blok 3'e bağlıyordu ve ikisi de kapalıydı. §2–§13 henüz yazılmamıştır — her bölümün yazım kapısı kendi başlığının altındadır. Kalite döngüsü (audit → deep review → cross-review → etki yansıtma → checkpoint) bu dokümanda **tüm bloklar kapandıktan sonra** ve **bir kez** koşar; sıra `01` → `02` → `10`'dur (00 §C.5).
> Taslak yazıldıktan sonra alınan bir karar §1'e dokunursa, karar satırı `(taslak güncellenecek)` işaretini taşır ve güncelleme aynı bloğun `docs:` PR'ında yapılır (K-29).

> **§1 kapanmamıştır ve Blok 8'den önce kapanmaz.** Sözlük ve aktör envanteri, diğer bölümlerin aksine **tek bir blokta bitmez** — terim üreten her blok ona satır ekler. §6.2'nin sütunu bunu görmüyordu; **2026-09-15'te düzeltildi** ve `02 §1` Blok 4–8'e de bağlandı (gerekçe: `PRODUCT_DISCOVERY_STATUS.md` §6.2, tablo altındaki not). Bugün bilinen eklemeler: K-17 sipariş durum adlarının sözlüğe ilk satırlarını `B6-03`'e bırakmıştır; **Sepet** ve **Sipariş kalemi** terimleri `B5-01`/`B5-03` ve `B6-04` ile tanımlanacaktır.
> **Kapalı Blok 3'ten gelen eksik kapatıldı (2026-09-15):** İndirim, Kupon ve Referans fiyat tam karara bağlı olduğu hâlde sözlükte satır taşımıyordu — kararları `02 §3`/`§5`'e yönlendirilmiş, `§1`'e yönlendirilmemişti. Üçü de eklendi; K-63 ve K-70'in etki sütunları düzeltildi. **Sepet** ve **Sipariş kalemi** hâlâ eksiktir ve bilinçli olarak bekletilmektedir — tanımları `B5-01`/`B5-03` ve `B6-04`'e bağlı, bugün yazılamaz.
> **Blok 4 eklemeleri (2026-09-16):** K-06'nın `B4-01`'e bıraktığı soru kapandı — **misafir alıcı dördüncü aktördür** (K-97) ve §1.3 üçten dörde çıktı, listedeki "aktör değildir" maddesi kalktı. Sözlüğe altı satır girdi: **Misafir alıcı · Oturum · Sosyal giriş · Adres defteri · Teslimat adresi · Fatura adresi** (K-97, K-98, K-103, K-104, K-106, K-108, K-111, K-112, K-113). Altısı da türetilmiştir (**†**) ve **A-05**'e eklenmiştir; sözlük 25 → **31 terim**. §1.3'e ayrıca üçüncü bir aktör kuralı ve yönetici tarafının kimlik doğrulama tabanı yazıldı (K-122).

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
| Hizmet | `Service` † | Tip değeri. Sabit fiyatlı, randevusuz hizmet. Firma isterse toplam adet **kontenjanı** girer; girmezse sınırsızdır (K-81, K-83). |

**Sınıflandırma**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Kategori | `Category` | Ürünün kataloğdaki yeri. Ağaç **en fazla üç seviyedir**; ürün ağacın herhangi bir düğümüne asılabilir, yaprak zorunluluğu yoktur (K-42, K-43). |
| Ana kategori | `PrimaryCategory` | Ürünün asıldığı kategorilerden biri. Kırıntı yolu ve ürünün adresi bundan üretilir; yayına çıkacak ürün için **zorunludur** (K-45). |

**Yayın durumu**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Yayın durumu | `PublishStatus` † | Üç değerli alan. **Her iki düzlemde de yaşar:** ürünün kendi durumu, varyantın kendi durumu vardır (K-54, K-55). |
| Taslak | `Draft` | Henüz yayınlanmamış kayıt. Adresi ziyaretçiye 404 döner; giriş yapmış firma yöneticisine ürün sayfasını "Taslak" bandıyla gösterir (K-54, K-57). |
| Yayında | `Published` | Vitrinde görünen kayıt. Bir ürünün "Yayında" olabilmesi için **arşivlenmemiş en az bir varyantı** olmalıdır (K-54, K-55). |
| Arşiv | `Archived` | Vitrinden çıkmış ama kaydı korunan ürün. **Silme değildir** — adresi çalışmaya devam eder ve "Bu ürün artık satılmıyor" sayfası döner (K-54, K-56). Kalıcı silme ayrı bir yoldur; silinen ürünün adresi 404 döner (K-85). |

**Stok ve fiyat**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Stok | `StockQuantity` † | Varyantın sayısal adedi. Ürün düzleminde ayrı bir stok alanı **yoktur**; ürünün stok durumu varyantlarından türetilir (K-50). |
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
| Firma | `Company` † | Kurulumun sahibi ve **satıcı**. Uygulamada tekildir: firma ekleme, firma seçme ve firmalar arası geçiş kavramı yoktur (K-01, K-09). Tahsilat firmanın kendi ödeme sağlayıcı hesabına geçer; platform ticari zincirde yer almaz (K-12). |
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

*Kaynak: K-17 (sözlük kuralı ve adlandırma konvansiyonu) · K-06 (aktör envanteri) · K-07 (alıcının hukuki sıfatı) · K-01 · K-09 · K-12 · K-14 · K-39 · K-40 · K-41 · K-42 · K-43 · K-45 · K-50 · K-54 · K-55 · K-56 · K-57 · K-58 · K-59 · K-63 · K-64 · K-65 · K-66 · K-67 · K-70 · K-71 · K-73 · K-74 · K-75 · K-81 · K-82 · K-83 · K-84 · K-85 · K-88 · K-89 · K-90 · K-96 · K-97 · K-98 · K-100 · K-103 · K-104 · K-106 · K-108 · K-111 · K-112 · K-113 · K-122.*

---

## 2. Temel akış

> **Ne yazılır:** Ürünün ana iş akışı, uçtan uca. Adım adım, dallanmasız. Detaylı akışlar `03_USER_FLOWS.md`'de.

> **Yazım kapısı (K-28):** **Blok 6 kapandığında** yazılır — besleyen konu `B6-12` (temel akış omurgası: kaç akış, hangi adımlarla). Bugün elde olan girdiler: K-11 (satış doğrudandır, teklif/talep hattı yoktur), K-81 (üç ürün tipi de aynı kataloğa girer, aynı sepetten geçer, aynı şekilde satın alınır).

## 3. İş kuralları

> **Ne yazılır:** Her kural ayrı madde. Kuralın **koşulu**, **sonucu** ve **istisnası**. Her kural bir sonraki dokümanda karşılığını bulacak şekilde somut olmalı.

> **Yazım kapısı (K-28):** **Blok 7 kapandığında** yazılır — besleyen bloklar: 1 ✓, 3 ✓, 4, 5, 6, 7. Dokümanın en geniş bölümüdür ve Blok 3'ün elli sekiz kararının büyük bölümü buraya düşer; besleyen küme kapanmadan yazılamaz.

### 3.x <Kural grubu>

## 4. Zaman kuralları

> **Ne yazılır:** Tüm süre/timeout kuralları **tek bölümde**. Aktör akışlarına gömülmez — dağıtılırsa tutarsızlık doğar.

> **Yazım kapısı (K-28):** **Blok 8 kapandığında** yazılır — besleyen bloklar: 5 ve 8. Bugün elde olan girdiler: K-66 (indirim tarihlidir, sistem başlatır ve bitirir), K-75 (kupon sınırı tarih + toplam kullanım adedi), K-80 (snapshot sipariş anında donar).

| # | Kural | Süre | Süre dolunca ne olur | Ayarlanabilir mi |
|---|---|---|---|---|

## 5. Durum tanımları

> **Ne yazılır:** Ana varlıkların (işlem, hesap, talep vb.) alabileceği durumlar ve geçiş koşulları. Durum makinesi mantığı burada başlar, `03`'te akışa, `06`'da şemaya döner. **Bu isimler tüm dokümanlarda birebir aynı kalır.**

> **Yazım kapısı (K-28):** **Blok 7 kapandığında** yazılır — besleyen bloklar: 3 ✓, 4, 6, 7. Blok 3'ün katkısı yayın durumu setidir (K-54, K-55) ve adları §1.2 sözlüğünde sabittir; sipariş durum makinesi `B6-03`'te karara bağlanır.

## 6. Hata ve istisna senaryoları

> **Ne yazılır:** Her ana akış için "ne ters gidebilir" ve sistemin cevabı. Happy path kadar detaylı.

> **Yazım kapısı (K-28):** **Blok 6 kapandığında** yazılır — besleyen bloklar: 5 ve 6; envanterin kendisi `B6-15`'tir. Bugün elde olan girdi: K-68 (indirimli fiyat referansın altına inmiyorsa sistem indirimi kaydetmez, yönetici sebebiyle uyarılır).

## 7. İptal, itiraz ve geri alma

> **Ne yazılır:** Kullanıcının bir işlemi geri alma yolları, koşulları, sınırları.

> **Yazım kapısı (K-28):** **Blok 6 kapandığında** yazılır — besleyen konular `B6-08` (iptal yolları), `B6-09` (cayma hakkı ve tipe göre istisna), `B6-10` (iade ve geri ödeme yürütümü). K-07 bu bölümün tek hukuki rejimini şimdiden sabitlemiştir; K-81'in üç tipi `B6-09`'a **tipe göre cayma hakkı istisnası** sorusu olarak devredilmiştir.

## 8. Kötüye kullanım ve güvenlik kuralları

> **Ne yazılır:** Kötüye kullanım senaryoları ve karşı önlemler. Teknik güvenlik değil, **ürün düzeyinde** kurallar (limit, cooldown, doğrulama zorunluluğu).

> **Yazım kapısı (K-28):** **Blok 8 kapandığında** yazılır — besleyen bloklar: 4 ve 8.

## 9. Bildirimler

> **Ne yazılır:** Hangi olayda kime bildirim gider. Kanal detayı `08`'de; burada **tetikleyici ve alıcı**.

> **Yazım kapısı (K-28):** **Blok 8 kapandığında** yazılır — besleyen blok: 8.

## 10. Yönetim (admin) kuralları

> **Ne yazılır:** Yöneticinin yapabildikleri ve yapamadıkları. **"Sonra ekleriz" yaklaşımı riskli** — yönetim akışları kullanıcı akışları kadar karmaşıktır, aynı derinlikte ele alınır.

> **Yazım kapısı (K-28):** **Blok 8 kapandığında** yazılır — besleyen bloklar: 7 ve 8. Bugün elde olan girdiler: K-01 ve K-10 (panel tek firmayı yönetir, firma seçimi yoktur), K-06 (tek rol, çoklu kullanıcı), K-20 (platform imzası panelden kapatılabilir), K-85 (yönetici ürünü kalıcı silebilir).

## 11. Sayısal parametreler (özet)

> **Ne yazılır:** Dokümanın her yerine dağılmış sayıların tek tablosu. Diğer dokümanlar bu tabloyu referans alır.

> **Yazım kapısı (K-28):** **Blok 9 kapandığında** yazılır — besleyen bloklar: 3 ✓, 5, 9; envanterin kendisi `B9-08`'dir. Blok 3'ün bu tabloya devrettiği kalemler: kategori derinliği (K-42), seçenek boyutu sayısı (K-40), KDV oran listesi (K-58, K-59), görsel format listesi · tek dosya boyutu tavanı · ürün başına görsel adedi tavanı (K-92), ürün adı karakter tavanı (K-95).

| # | Parametre | Değer | Nerede kullanılır | Runtime ayarlanabilir |
|---|---|---|---|---|

## 12. Uyumluluk ve yasal

> **Ne yazılır:** Yasal yükümlülükler, veri saklama/silme hakları, erişim kısıtları.

> **Yazım kapısı (K-28):** **Blok 8 kapandığında** yazılır — besleyen bloklar: 1 ✓, 4, 6, 8. Bugün elde olan girdiler: K-07 (6502 tüketici rejimi), K-12 (kart verisi ürüne girmez), K-14 (ETBİS ve sitede sürekli gösterilecek kimlik bilgileri), K-16 (tek pazar Türkiye), K-63 (İndirimli Satışlara İlişkin Yönetmelik referans kuralı), K-79 (beyan edilen referans fiyat sipariş kaleminde donar).

## 13. Açık kararlar

> **Ne yazılır:** Bilinçli olarak ileriye bırakılan **detaylar** ve ne zaman karara bağlanacakları. Varlık kararı burada asla açık kalmaz.

> **Yazım kapısı (K-28):** **Blok 9 kapandığında** yazılır. Dönem boyunca açık kalan detaylar `PRODUCT_DISCOVERY_STATUS.md` §4'te izlenir; bu tablo aşama kapanışında oradan devralınır.

| # | Konu | Ne belirsiz | Ne zaman karara bağlanır |
|---|---|---|---|

---

*Shopfolio — Product Requirements v0.2 (§1 taslak; §2–§13 kendi yazım kapılarında yazılır)*
