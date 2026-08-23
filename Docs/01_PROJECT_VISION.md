# Shopfolio — Project Vision

**Versiyon: v0.2** | **Bağımlılıklar:** `PRODUCT_DISCOVERY_STATUS.md` | **Son güncelleme:** 2026-08-23

> **Aşama:** 1 — Product Discovery · **Rol:** Product Manager / Discovery Facilitator
> **Traceability zorunlu:** Hayır (kök doküman)
> **Ne zaman `✓ Tamamlandı`:** Doküman Tamamlama Protokolü (00 §C.4) + kalite döngüsü (00 §C.5) geçildiğinde.

> **Taslak durumu (K-28):** §1–§5 **taslak** olarak yazılmıştır; besleyen bloklar (Blok 1 ve Blok 2) kapalıdır. §6–§8 **Blok 9 kapandığında** yazılır — o bölümlerin girdisi henüz alınmamıştır. Kalite döngüsü (audit → deep review → cross-review → etki yansıtma → checkpoint) bu dokümanda **tüm bloklar kapandıktan sonra** ve **bir kez** koşar (00 §C.5).
> Taslak yazıldıktan sonra alınan bir karar bu bölümlere dokunursa, karar satırı `(taslak güncellenecek)` işaretini taşır ve güncelleme aynı bloğun `docs:` PR'ında yapılır (K-29).

> **Karar referansları:** Metindeki `K-xx` işaretleri `PRODUCT_DISCOVERY_STATUS.md` §2 karar kaydına, `Bx-yy` işaretleri aynı dosyanın §6.3 blok içeriklerine gider.

---

## 1. Problem

**Kimin problemi:** Kurumsal tanıtıma ve çevrimiçi satışa **aynı anda** ihtiyaç duyan, sektör bağımsız KOBİ ve küçük işletme (K-03). Ölçek varsayımı: birkaç yüz ürüne kadar katalog, tek depo.

**Problem:** Bu işletme bugün iki ihtiyacını **iki ayrı sistemde** yürütüyor — tanıtımı bir web sitesinde veya sosyal medya hesabında, satışı bir pazaryerinde veya hazır e-ticaret paketinde. Ürün adı, açıklaması, görselleri, fiyatı ve firmanın iletişim bilgileri **iki yerde ayrı ayrı** giriliyor ve ayrı ayrı güncelleniyor.

Bunun somut sonucu üç maddedir:

1. **Çift giriş yükü.** Tek bir ürünün fiyatını değiştirmek iki ayrı panele girmeyi ve aynı işi iki ayrı arayüzde tekrarlamayı gerektirir. Yük, katalog büyüdükçe ve güncelleme sıklaştıkça artar.
2. **Tutarsızlık riski.** İki güncellemeden biri unutulduğunda site ile mağaza birbiriyle çelişir: tanıtım yüzünde görünen fiyat ile satış kanalındaki fiyat, ya da sitede yazan iletişim bilgisi ile mağaza kaydındaki bilgi ayrışır. Ziyaretçi hangisinin doğru olduğunu bilemez.
3. **Yükün sahibi işletmenin kendisidir.** Bu iş bir çalışanın günlük mesaisinden çıkar; teknik personeli olmayan bir KOBİ'de çoğu zaman işi yürüten kişinin üstündedir.

**Bugün nasıl çözülüyor ve neden yetersiz:** Mevcut seçeneklerin hiçbiri iki ihtiyacı tek üründe eşit ağırlıkta karşılamaz.

- **Mağaza odaklı hazır paketler** satışı çözer; kurumsal katman sonradan eklenmiş bir ek sayfa olarak kalır.
- **Ajans kurumsal siteleri** tanıtımı çözer, satış yapmaz.
- **Ismarlama yazılım** ikisini de çözebilir ama KOBİ bütçesinin üstünde kalır.

Alternatiflerin tam envanteri ve neden yetersiz kaldıkları §5'tedir.

*Kaynak: K-15 (problem merkezi) · K-01 · K-02 · K-03 · K-04 · K-05.*

---

## 2. Çözüm

Shopfolio, bir firmanın **kurumsal tanıtım sitesi** ile **çevrimiçi mağazasını** tek bir üründe birleştiren web uygulamasıdır. Firma; ürün kataloğunu, kurumsal içeriğini ve kimlik bilgilerini **tek bir yönetim panelinden** yönetir — aynı bilgi ikinci bir yere girilmez. Ziyaretçi aynı site içinde firmayı tanır, ürüne bakar, üye olur veya Google/Facebook hesabıyla giriş yapar, sepetini oluşturur, kartla öder ve siparişini takip eder. Satış **doğrudandır**: yayındaki her ürünün fiyatı vardır ve satın alınabilir; "fiyat sorunuz" gibi bir ürün durumu, teklif veya talep hattı yoktur (K-11). Uygulama **kurulum başına tek firmaya** hizmet eder; ikinci müşteri ikinci kurulumdur (K-02, K-09).

*Kaynak: K-11 (satış modeli) · K-02 · K-09.*

---

## 3. Hedef kitle

Bu bölüm iki ayrı soruyu yanıtlar: **ürünü kim satın alır** ve **ürünü kim kullanır**.

### 3.1 Ürünü satın alan: müşteri segmenti

Shopfolio **ticari bir üründür** — baştan itibaren birden çok KOBİ'ye kurulmak üzere geliştirilir ve ilk müşteri henüz belirli değildir (K-05). Müşteri segmenti **sektör bağımsız KOBİ / küçük işletmedir** (K-03): hem kurumsal bir tanıtım yüzüne hem küçük ölçekli çevrimiçi satışa ihtiyaç duyan işletmeler.

| Alan | Değer |
|---|---|
| Segment | Sektör bağımsız KOBİ / küçük işletme (K-03) |
| Firma tipi | Şahıs işletmesi **veya** tüzel kişi — ikisi de kapsamdadır; zorunlu kimlik alanları tipe göre değişir (K-14) |
| Katalog ölçeği | Birkaç yüz ürüne kadar (K-03) |
| Operasyon ölçeği | Tek depo (K-03) |
| Pazar | Türkiye; tek dil Türkçe, tek para birimi TRY (K-16) |
| Kurulum modeli | Kurulum başına bir firma; firma kimliği koda gömülmez, panelden yönetilir (K-02, K-08) |

**Kapsam dışı bırakılan segmentler — bilinçli karar:** Dikey daraltma (moda, dekorasyon) varyant ve iade akışını MVP'de zorunlu kılarak kapsamı büyütürdü; B2B / bayi modeli ise açık fiyat + sepet + kartla ödeme akışını teklif ve cari mutabakatla değiştirip ürün tanımıyla çelişirdi (K-03).

### 3.2 Ürünü kullanan: aktörler

Uygulamada **üç aktör** vardır. Yönetim tarafı **tek roldür ve çoklu kullanıcıya** açıktır: firma birden fazla yönetici hesabı açabilir, hepsi aynı yetkiye sahiptir; ayrı hesaplar sayesinde "kim ne değiştirdi" izlenebilir kalır (K-06).

| Aktör | Kim | Neden kullanır | Neden geri döner |
|---|---|---|---|
| **Ziyaretçi** | Siteye üye olmadan gelen kişi | Firmayı tanımak, kurumsal içeriğe ve iletişim bilgisine ulaşmak, ürün kataloğuna bakmak | Firma bilgisi ve katalog aynı adreste ve güncel tutulduğu için; aradığını bulmak için ikinci bir kanala gitmesi gerekmez |
| **Üye müşteri** | Hesabı olan tüketici; kişisel ihtiyacı için satın alan gerçek kişi (K-07) | Sipariş vermek, ödemesini yapmak, siparişinin durumunu takip etmek | Hesap sürekliliği: tekrar satın alma ilk siparişten daha az adım gerektirir ve sipariş takibi tek yerden yapılır (K-19) |
| **Firma yöneticisi (admin)** | Firmanın panel kullanıcısı; tek rol, çoklu kullanıcı (K-06) | Ürün kataloğunu, kurumsal içeriği, firma kimlik bilgilerini ve gelen siparişleri yönetmek | Günlük işini burada yapar — sipariş geldikçe ve katalog değiştikçe panele girer; panel tek doğruluk kaynağıdır (K-19) |

**Aktör olmayanlar — bilinçli kararlar:**

- **Platform operatörü uygulama içi aktör değildir.** Kurulum bir deploy işidir (Aşama 4 + `DEPLOY_RUNBOOK.md`); uygulamaya operatör paneli koymak, K-01 ile elenen kiracılığı arka kapıdan geri getirirdi (K-06).
- **Ticari / kurumsal alıcıya özel bir aktör veya hukuki rejim yoktur.** MVP'de alıcı yalnız tüketicidir; 6502 sayılı Kanun'un tüketici korumaları istisnasız her siparişte uygulanır (K-07).
- **Misafir alıcı henüz aktör değildir.** Üyeliksiz sipariş kararına bağlıdır; o karar `B4-01`'de alınır (K-06).

*Kaynak: K-06 (aktör envanteri) · K-03 · K-05 · K-07 · K-08 · K-14 · K-16 · K-19 · B1-09 (aktör tablosuna "neden geri döner" sütunu).*

---

## 4. Değer önerisi

**Ağırlık merkezi:** Üç aktörün de değeri yazılır; **firma yöneticisi birincildir**, ziyaretçi ve üye müşteri **türevdir**. Ürünü satın alan firmadır (K-18); türev aktörlerin değeri, firmanın değerine hizmet ettiği ölçüde yazılır ve her satır "neden yazılır" gerekçesini taşır (K-19).

### 4.1 Firma (birincil)

| # | Bu ürün olmasaydı ne olurdu | Shopfolio ne veriyor |
|---|---|---|
| D-1 | Aynı ürün bilgisi iki ayrı panelde ayrı ayrı güncellenirdi; biri unutulduğunda site ile mağaza çelişirdi (K-15) | **Tek yerden yönetim ve tek doğruluk kaynağı.** Ürün, kurumsal içerik ve firma kimliği tek panelden yönetilir; ikinci bir kayıt yeri yoktur |
| D-2 | Kurumsal tanıtım ile mağaza iki ayrı yüzde kalır, ziyaretçinin yolculuğu ikisi arasında kopardı | **Tanıtım ile vitrin tek sitede birleşir.** Ana sayfada ikisi birlikte bulunur; firma yalnız hangisinin önce geleceğini seçer (K-27) |
| D-3 | Firma unvanı, vergi/MERSİS numarası veya adresi değiştiğinde her seferinde geliştirici müdahalesi gerekirdi | **Firma kendi kimliğini kendisi yönetir.** Kimlik alanlarının tamamı panelden düzenlenir; her değişiklik kim/ne zaman bilgisiyle işlem izine yazılır (K-08) |
| D-4 | Site kendi alan adında yayınlansa bile firmaya "kendi sitesi" hissi vermezdi | **Site firmanın kendi alan adında yayınlanır**; footer'daki platform imzası panelden kapatılabilir (K-20) |
| D-5 | Mevzuat değiştiğinde (KVKK, 6502, ETBİS, ödeme sağlayıcı API'leri) kurulu site zamanla uyumsuzlaşırdı | **Yıllık bakım/destek aboneliği uyum bakımını finanse eder** — kurulum bedeli + yıllık abonelik modelinin karşılığı budur (K-18) |

### 4.2 Ziyaretçi (türev)

**Değer: kesintisiz yolculuk.** Ziyaretçi firmayı tanıdığı yerden ürüne, üründen satın almaya kanal değiştirmeden geçer.

**Neden yazılır:** Ziyaretçi Shopfolio'yu seçmez — karşılaşır. Bu satır, kesintisiz yolculuğun **firmanın dönüşümünü artırdığı** için yazılır; değer firmaya döndüğü ölçüde değerdir (K-19).

### 4.3 Üye müşteri (türev)

**Değer: hesap sürekliliği.** Üye müşterinin hesabı siparişleri arasında sürer; tekrar satın alma ilk siparişten daha az adım gerektirir ve sipariş takibi tek yerden yapılır.

**Neden yazılır:** Bu satır, hesap sürekliliğinin **tekrar satın almayı kolaylaştırdığı** için yazılır — kazanç yine firmanın tarafındadır (K-19).

> **Ölçüye bağlanma:** §6'nın sayısal hedefleri firma tarafında toplanır; ziyaretçi tarafının huni ölçüsü türev olarak devralınır (K-19). §6 **Blok 9 kapandığında** yazılır (K-28).

*Kaynak: K-19 (ağırlık merkezi) · K-04 · K-05 · K-08 · K-15 · K-18 · K-20 · K-27.*

---

## 5. Konumlandırma ve farklılaşma

### 5.1 Birincil farklılaşma ekseni

**Kurumsal tanıtım sitesi ile mağazanın tek üründe eşit ağırlıkta buluşması.** Hazır e-ticaret paketleri mağaza odaklıdır ve kurumsal katmanı sonradan eklenmiştir; ajans siteleri satış yapmaz. Shopfolio bu boşluğu hedefler — ürün adı da (Shop + folio) bunu taşır (K-04).

**Destekleyici eksenler** — birincil değildir; MVP'de "en iyi çalışması gereken şey"e dönüşmezler (K-04):

- **Sahiplik ve maliyet:** kendi alan adı ve kapatılabilir platform imzası (K-20), kimliğin geliştiriciye bağlı olmaması (K-08).
- **Kasıtlı sadelik:** teklif hattı yok (K-11), serbest sayfa kurgusu yok (K-27), firma seçici yok (K-09).

### 5.2 Alternatif envanteri

Alternatifler **tanıtım gücü × satış gücü** ekseninde konumlanır. "Rakip yok" cevabı geçerli değildir — KOBİ bugün bu problemi bir şekilde çözüyor.

| Alternatif | Tanıtım gücü | Satış gücü | Ne veriyor | Ne vermiyor | KOBİ neden orada kalıyor |
|---|---|---|---|---|---|
| **Pazaryeri** | Düşük | Yüksek | Hazır alıcı trafiği, ödeme ve kargo altyapısı, sıfır kurulum | Kendi alan adı, kurumsal kimlik yüzü, müşteri ilişkisinin sahipliği; firma kimliği pazaryerinin şablonuna sıkışır | Satış oradan geliyor; ayrılmak ciroyu doğrudan riske atar |
| **Hazır e-ticaret paketi** | Düşük | Yüksek | Kendi alan adında mağaza, ödeme ve kargo entegrasyonları, düşük kurulum maliyeti | Tanıtım ile mağazanın eşit ağırlıkta tek yüzde buluşması; kurumsal içerik sonradan eklenmiş bir ek sayfa olarak kalır | Satış tarafı çalışıyor; kurumsal eksiğini ayrı bir site veya sosyal medya ile kapatıyor — bu da §1'deki iki ayrı sistem problemini doğuruyor |
| **Ajans kurumsal sitesi** | Yüksek | Yok | Firmaya özel tasarım, güçlü kurumsal anlatı, kendi alan adı | Katalog, sepet, ödeme, sipariş takibi; içerik güncellemesi çoğu zaman ajansa bağlı kalır | Kurumsal yüz ihtiyacı karşılanıyor; satışı başka bir kanala taşımış durumda |
| **Sosyal medya / WhatsApp** | Orta | Düşük | Sıfır maliyet, hızlı başlangıç, müşteriyle doğrudan iletişim | Kendi alan adı, fiyatlı ürün kataloğu, ödeme ve sipariş kaydı; satış mesajlaşmayla yürür, sipariş izi kalmaz | Zaten orada; müşteri de orada |
| **Ismarlama yazılım** | Yüksek | Yüksek | İki ihtiyacı da tam karşılayan, firmaya özel çözüm | KOBİ bütçesine sığan bir maliyet ve teslim süresi; bakım ve mevzuat uyumu her seferinde ayrı bir iş kalemi | **Kalmıyor** — bu seçeneğe bütçe nedeniyle hiç giremiyor |

**Ismarlama yazılım neden ayrı satırda:** Her iki eksende de yüksek olabildiği için matrisin geri kalanıyla aynı mantıkla ayrışmaz; onu ayıran eksen **bütçedir** (K-21).

### 5.3 Shopfolio'nun konumu

Shopfolio her iki eksende de **yüksek** olmayı hedefler ve ısmarlama yazılımın karşıladığı ihtiyacı **bütçe engeli olmadan** karşılamayı amaçlar. Bunu mümkün kılan yapısal karar, ürünün **yeniden kurulabilir** olmasıdır: firma kimliği, kurumsal içerik ve sayısal parametreler koda gömülmez, ayardan ve panelden gelir (K-02); geliştirme maliyeti tek bir firmaya değil, birden çok kuruluma yayılır (K-05).

Bu konumlandırmanın somut karşılıkları:

| Konumlandırma iddiası | Üründeki somut karşılığı |
|---|---|
| Tanıtım ile mağaza eşit ağırlıkta | Ana sayfada ikisi **birlikte** bulunur; firma iki hazır düzenden birini seçer (tanıtım öncelikli / mağaza öncelikli) ve hiçbir düzen kurumsal tanıtımı ana sayfadan çıkaramaz (K-27) |
| Firma kendi kimliğinin sahibidir | Unvan, vergi/MERSİS, adres, iletişim, logo ve marka panelden yönetilir; kurulumdan yalnız sır ve altyapı gelir — alan adı, e-posta gönderim kimliği, ödeme sağlayıcı anahtarları (K-08) |
| Sektör bağımsız KOBİ kapsanır | Firma tipi seçilir: şahıs işletmesi (ad-soyad + vergi kimlik no) veya tüzel kişi (ticaret unvanı + MERSİS no + ticaret sicil no); tek zorunlu alan seti şahıs işletmesini ürünün dışına atardı (K-14) |
| Site firmanındır | Kendi alan adında yayınlanır, Shopfolio alt alan adı yoktur; platform imzası varsayılan açıktır ve panelden kapatılabilir (K-20) |

*Kaynak: K-21 (alternatif envanteri ve biçimi) · K-04 (farklılaşma ekseni) · K-02 · K-05 · K-08 · K-14 · K-15 · K-20 · K-27.*

---

## 6. Başarı kriterleri

> **Ne yazılır:** Ölçülebilir hedefler. "Başarılı sayılması için ne olmalı?" sorusunun sayısal cevabı.

> **Yazım kapısı (K-28):** Bu bölüm **Blok 9 kapandığında** yazılır — besleyen konular arasında B9-09 (firma kendi kendine yeter kriteri), B9-10 (huni ölçüsü) ve B9-14 vardır. Bugün elde olan girdiler: K-05 (ticari ürün), K-18 (gelir modeli), K-19 (sayısal hedefler firma tarafında toplanır), K-22 (MVP başarı çıtası), K-23 ("bitti" kabul çizgisi).

| # | Kriter | Ölçüm | Hedef |
|---|---|---|---|

## 7. Sınırlar

> **Ne yazılır:** Bu ürünün **ne olmadığı**. Kapsam dışı bırakma bilinçli bir karardır ve gerekçesiyle kaydedilir. Detaylı kapsam `10_MVP_SCOPE.md`'de.

> **Yazım kapısı (K-28):** Bu bölüm **Blok 9 kapandığında** yazılır. Bugün karara bağlanmış sınırlar: pazaryeri / çok satıcı modeli **kalıcı ürün sınırıdır**, çok kiracılı SaaS post-MVP yol haritası adayıdır (K-01, K-10); platform ticari zincirde yer almaz, tahsilat firmanın kendi hesabına geçer (K-12); uygulamada firma tekildir, firma ekleme/seçme kavramı yoktur (K-09); tek pazar Türkiye, tek dil Türkçe, tek para birimi TRY (K-16). Bu satırların nihai biçimi ve `10 §3` / `10 §4` ile hizası Blok 9'da sabitlenir (K-25, K-26).

## 8. Riskler ve varsayımlar

> **Ne yazılır:** Ürünün dayandığı varsayımlar ve bunlar yanlışsa ne olacağı. Teknik varsayımlar değil, **ürün** varsayımları.

> **Yazım kapısı (K-28):** Bu bölüm **Blok 9 kapandığında** yazılır. K-05 bu bölümün uzun olacağını baştan kabul etti: ürün, gerçek kullanıcı geri bildirimi olmadan geliştirilir — bu yüzden her varsayımın "nasıl doğrulanacak" alanı **doldurulacaktır**. Bugün bilinen varsayım kaynakları: K-03 (segment ve ölçek varsayımı), K-04 (farklılaşma ekseninin gerçek bir boşluğu hedeflediği varsayımı), K-05 (ilk müşteri belirli değil), K-08 (zorunlu alan + işlem izi boş bırakmayı engeller, yanlış yazmayı engellemez — kalan risk bilinçlidir).

| # | Varsayım | Yanlışsa ne olur | Nasıl doğrulanacak |
|---|---|---|---|

---

*Shopfolio — Project Vision v0.2 (§1–§5 taslak; §6–§8 Blok 9'da yazılır)*
