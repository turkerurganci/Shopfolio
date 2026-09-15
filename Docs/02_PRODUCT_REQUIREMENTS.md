# Shopfolio — Product Requirements

**Versiyon: v0.2** | **Bağımlılıklar:** `01_PROJECT_VISION.md`, `PRODUCT_DISCOVERY_STATUS.md` | **Son güncelleme:** 2026-09-15

> **Aşama:** 1 — Product Discovery · **Rol:** Product Manager
> **Traceability zorunlu:** Hayır (kaynak doküman — sonraki aşamalar buraya izlenir)
> **Bu doküman tüm iş kurallarının tek kaynağıdır.** Kod bu dokümanla çelişemez.

> **Taslak durumu (K-28):** §1 **taslak** olarak yazılmıştır; yazıldığı anda §6.2'nin "Hedef doküman" sütunu bu bölümü Blok 1 ve Blok 3'e bağlıyordu ve ikisi de kapalıydı. §2–§13 henüz yazılmamıştır — her bölümün yazım kapısı kendi başlığının altındadır. Kalite döngüsü (audit → deep review → cross-review → etki yansıtma → checkpoint) bu dokümanda **tüm bloklar kapandıktan sonra** ve **bir kez** koşar; sıra `01` → `02` → `10`'dur (00 §C.5).
> Taslak yazıldıktan sonra alınan bir karar §1'e dokunursa, karar satırı `(taslak güncellenecek)` işaretini taşır ve güncelleme aynı bloğun `docs:` PR'ında yapılır (K-29).

> **§1 kapanmamıştır ve Blok 8'den önce kapanmaz.** Sözlük ve aktör envanteri, diğer bölümlerin aksine **tek bir blokta bitmez** — terim üreten her blok ona satır ekler. §6.2'nin sütunu bunu görmüyordu; **2026-09-15'te düzeltildi** ve `02 §1` Blok 4–8'e de bağlandı (gerekçe: `PRODUCT_DISCOVERY_STATUS.md` §6.2, tablo altındaki not). Bugün bilinen eklemeler: K-06 misafir alıcının aktör olup olmadığını `B4-01`'e, K-17 sipariş durum adlarının sözlüğe ilk satırlarını `B6-03`'e bırakmıştır; **Sepet** ve **Sipariş kalemi** terimleri `B5-01`/`B5-03` ve `B6-04` ile tanımlanacaktır.
> **Kapalı Blok 3'ten gelen bir eksik de vardır:** İndirim, Kupon ve Referans fiyat kavramları tam karara bağlı olduğu hâlde sözlükte satır taşımıyor — kararları `02 §3`/`§5`'e yönlendirilmişti. Karar bekliyor: `PRODUCT_DISCOVERY_STATUS.md` §6.1.

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

**Aktörler ve taraflar**

| Türkçe terim | İngilizce kod karşılığı | Tanım |
|---|---|---|
| Firma | `Company` † | Kurulumun sahibi ve **satıcı**. Uygulamada tekildir: firma ekleme, firma seçme ve firmalar arası geçiş kavramı yoktur (K-01, K-09). Tahsilat firmanın kendi ödeme sağlayıcı hesabına geçer; platform ticari zincirde yer almaz (K-12). |
| Ziyaretçi | `Visitor` † | Siteye üye olmadan gelen kişi (K-06). |
| Üye müşteri | `Customer` † | Hesabı olan alıcı (K-06). "Üye" ve "müşteri" **ayrı terimler olarak kullanılmaz** — tek terim budur (K-17). |
| Firma yöneticisi | `Admin` † | Firmanın panel kullanıcısı. Yönetim tarafı **tek roldür ve çoklu kullanıcıya açıktır** (K-06). |
| Tüketici | `Consumer` † | Alıcının **hukuki sıfatı**: kişisel ihtiyacı için alan gerçek kişi. Ayrı bir aktör değildir — MVP'de alıcı istisnasız tüketicidir ve 6502 sayılı Kanun'un korumaları her siparişte uygulanır (K-07). |

> **Kapsam notu (K-84):** "Ürün" terimi, firmanın kataloğa koyduğu her şeyi kapsar; **sistem ne satıldığını denetlemez.** Ek akış gerektiren ürün türleri — alkol, tütün, ilaç ve reçeteli ürünler, silah — MVP kapsamı dışındadır (`10 §3`). Engelleme mekanizması, yasaklı kategori listesi ve ürün başına mevzuat belgesi alanı **yoktur**; mevzuata uygunluk firmanın yükümlülüğüdür (K-09, K-12, K-14).

> **Ölçek notu (K-89):** `01 §3.1`'deki "birkaç yüz ürüne kadar katalog, tek depo" ifadesi bir **kabuldür ve `10 §4`'te yaşar** — kural değildir. Sistem hiçbir yerde ölçek sınırı uygulamaz: ürün adedi, boyut başına seçenek değeri sayısı ve ürün başına varyant adedi tavansızdır. K-40'ın **iki seçenek boyutu** sınırı bundan ayrıdır ve geçerliliğini korur.

### 1.3 Aktörler

Uygulamada **üç aktör** vardır: ziyaretçi · üye müşteri · firma yöneticisi (K-06). Adları, kod karşılıkları ve tanımları §1.2'dedir; her aktörün **neden kullandığı ve neden geri döndüğü** `01 §3.2`'dedir ve burada tekrarlanmaz.

Bu dokümanın iş kurallarının dayandığı iki aktör kuralı:

1. **Yönetim tarafı tek roldür, çoklu kullanıcıya açıktır.** Firma birden fazla yönetici hesabı açabilir; hepsi aynı yetkiye sahiptir. Yetki matrisi MVP'de **yoktur**. Ayrı hesaplar korunduğu için "kim ne değiştirdi" izlenebilirliği kaybolmaz (K-06).
2. **Alıcı istisnasız tüketicidir.** Ticari/kurumsal alıcıya özel hukuki rejim yoktur; cayma hakkı, ön bilgilendirme ve mesafeli satış sözleşmesi dallanmadan uygulanır (K-07).

**Aktör olmayanlar — bilinçli kararlar:**

- **Platform operatörü uygulama içi aktör değildir.** Kurulum bir deploy işidir; uygulamaya operatör paneli koymak, K-01 ile elenen kiracılığı arka kapıdan geri getirirdi (K-06).
- **Misafir alıcı henüz aktör değildir.** Üyeliksiz sipariş kararına bağlıdır; o karar `B4-01`'de alınır (K-06). Karar "misafir alışverişi vardır" yönünde çıkarsa §1.2 sözlüğüne ve bu listeye birer satır eklenir.

*Kaynak: K-17 (sözlük kuralı ve adlandırma konvansiyonu) · K-06 (aktör envanteri) · K-07 (alıcının hukuki sıfatı) · K-01 · K-09 · K-12 · K-14 · K-39 · K-40 · K-41 · K-42 · K-43 · K-45 · K-50 · K-54 · K-55 · K-56 · K-57 · K-58 · K-59 · K-64 · K-81 · K-82 · K-83 · K-84 · K-85 · K-88 · K-89 · K-90 · K-96.*

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
