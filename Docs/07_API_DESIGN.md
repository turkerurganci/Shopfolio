# Shopfolio — API Design

**Versiyon: v0.1** | **Bağımlılıklar:** `02`, `03`, `04`, `05`, `06`, `10_MVP_SCOPE.md` | **Son güncelleme:** YYYY-AA-GG

> **Aşama:** 6 — API Tasarımı · **Rol:** API Designer
> **Traceability zorunlu: EVET** — §1 tamamlanmadan §3'e (endpoint envanteri) geçilmez.

> **Aşama 1'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36.
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-06 · K-97 · Aktörler:** Dört aktör — ziyaretçi · misafir alıcı · üye müşteri · firma yöneticisi (`02 §1.3`; misafir alıcıyı K-97 ekledi, satır Aşama 2'nin çakışma taramasında hizalandı); yetkilendirme tasarımı **tek yönetici rolü** varsayar (çoklu kullanıcı, aynı yetki).
> - **K-17 · K-977 · K-988 · Terim sözlüğü:** `02 §1` sözlüğü **birebir devralınır**; endpoint, alan ve enum adları sözlüğün İngilizce karşılıklarından — kapalı liste değerleri ve biçimini Teknik Mimari'nin kurduğu teknik kavramlar Veri Modeli'nin (`06`) verdiği adlardan (K-531, K-977, K-988) — türer, eş anlamlı ad kullanılmaz (satır Aşama 4'ün çakışma taramasında ve checkpoint'inde hizalandı).

> **Aşama 2'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §8.3 AK0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **Dizin — Aşama 2 kararlarının bu dokümana devirleri:** karar kaydında K-647…K-718'in etki sütunlarında bu dokümanı gösteren bir atıf (bir karar) [`CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md) §6.1'dedir; etki sütununda yalnız doküman numarası taşıyan bir atfın işinin adını dizin verir. Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Aşama 1'in dizini: `PHASE1_CONFLICT_SCAN.md` §6.

> **Aşama 3'ten park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2 ve §3, mekanizma: K-36 (yeri: §10.3 UI0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-787 · K-794 · K-795 · İşlemlerin adedi:** müşterinin iptal, cayma ve ayıp talebi işlemleri ile yöneticinin firma iptali, adet azaltması, iade teslim alması, iade reddi ve "mal dönmedi" kapanışı kalem başına bir adet taşır; adet o işleme açık adedi aşamaz ve gecikme feshinde seçim yoktur (K-796; `02 §7.1.6`; Arayüz Tanımları `04` 2.12.1.8, §5.17, §5.18, §5.19). Ekranın açık adedi ve varsayılanı göstermesi için gereken verinin nasıl sunulduğu ve aşan adedin sunucuda nasıl reddedildiği bu aşamanın kararıdır.
> - **K-737 · Ürün listesinin araması:** panelin ürün listesi ürün adıyla aranır ve yayın durumuna göre süzülür (`02 §10.1.2`; `04 §9.4`). Arama ve süzme parametrelerinin biçimi bu aşamanın kararıdır.
> - **K-802 · Sipariş listesinin araması ve süzgeçleri:** sipariş listesi sipariş numarası ya da iletişim e-postasıyla tam eşleşmeyle aranır; sipariş durumu, ödeme durumu ve bekleyen iş süzgeçleriyle süzülür; en yeni önce, süreli bekleyen işte kalan süresi en az olan önce sıralanır (`04 §9.8`). Parametrelerin biçimi bu aşamanın kararıdır.
> - **Dizin — Aşama 3 kararlarının bu dokümana devirleri:** karar kaydında K-723…K-850'nin etki sütunlarında bu dokümanı gösteren altı atıf (altı karar) ve Arayüz Tanımları'nın (`04`) gövdesinde bu dokümana iş bırakan cümleler [`CHECKPOINT_REPORTS/PHASE3_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE3_CONFLICT_SCAN.md) §6'dadır; Aşama 3'ün atıflarının hepsi işin adını taşır. Bir iş yalnız `04`'ün gövdesinde yaşar, karar satırı yoktur: sitenin dışından gelinen sayfa adreslerinin biçimi (`04 §3.5`; adresin kaydın adından üretilmesi `02 §3.30.2`'dedir). Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Önceki aşamaların dizinleri: `PHASE1_CONFLICT_SCAN.md` §6, `PHASE2_CONFLICT_SCAN.md` §6.

> **Aşama 4'ten park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §12.3 TM0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-868 · Sözleşmelerin biçimi:** sayfalar sunucuda Razor Pages ile üretilir; ayrı bir JSON API katmanı yoktur — bu dokümanın uç envanteri sayfa ve form uçlarını (gönder–yönlendir–göster) ve az sayıdaki makine ucunu — ödeme sağlayıcısının bildirimi, parçalı dosya yükleme, indirme, site haritası ve robots.txt (ikisi aynı makine ucundan üretilir — `05 §3.7.4`), canlılık ve durum uçları — taşır; dış servisten gelen ve ürüne veri getiren isteği alan yeni bir uç imzalıdır ve listeye karar kaydıyla girer; canlılık ve durum uçları imzasız açık GET uçlarıdır (Teknik Mimari `05 §2` MK-04, §3.6.1). Şablonun endpoint, DTO ve hata kodu bölümlerinin bu biçime nasıl uygulanacağı bu aşamanın kararıdır.
> - **K-873 · K-874 · K-980 · Sürümün taşınması:** düzenleme formları kaydın açıldığı andaki sürümü, yöneticinin sipariş işlemleri ekranda gördüğü sürümü taşır; ürün formu stok ve kontenjan alanının gösterdiği değeri de taşır — alan ürünün sürümüyle değil o değere karşı koşullu yazılır (`05 §7.3.1`; K-980); sürüm değişmişse yanıt kaydın ya da siparişin değiştiğini söyler ve güncel hâli gösterir; "Yine de kaydet" formu güncel sürümle yeniden gönderir (`05 §7`; `04 §2.10`). Alanın adı ve yanıtların biçimi bu aşamanın kararıdır.
> - **K-883 · K-884 · Dosya uçları:** dijital ürün dosyası parçalı ve kaldığı yerden sürdürülebilir biçimde yüklenir; indirme düğmesi kaleme bağlı, bir saat ömürlü bir bağlantı üretir ve bu bağlantıdaki devam istekleri (HTTP aralık istekleri) yeni hak düşürmez; ömür yalnız isteğin kabulünde denetlenir — ömür dolarken süren aktarım kesilmez (`05 §7.7.4`; `02 §3.12.5`). Uçların tanımı bu aşamanın kararıdır.
> - **K-893 · K-899 · K-904 · K-912 · K-941 · K-970 · K-971 · Formlar, kimlik şemaları ve erişim:** ödeme adımı formu tek kullanımlık bir onay anahtarı taşır ve ikinci gönderim var olan siparişin sayfasına götürür; ödeme sağlayıcısının bildirim ucu imzası doğru bildirimi kaydedip hemen "alındı" der; aynı bildirim ikinci kez gelirse kaydedilmez, işlenmez ve yine "alındı" alır — tekil olan bildirimin kendisidir (`05 §4.5.2`; K-970); imzası tutmayan istek kaydedilmez, işlenmez ve 4xx alır; müşteri ve yönetici iki ayrı kimlik şemasıyla, ayrı adlı çerezlerle girer; yeniden doğrulamadan dönülen işlem formu tek kullanımlık bir değer taşır — izin o değere bağlıdır, gönderim onu tüketir, formun gösterimi dışındaki ilk sayfa isteği izni düşürür (`05 §6.1.5`; K-971); e-postayla gelen bağlantı yalnız ekranı açar — doğrulama ve geri alma ekrandaki düğmenin gönderdiği POST isteğiyle olur; panel varsayılan olarak kapalıdır, kayıt düzeyinde erişim sunucuda süzülür ve ekrana sunucunun karar verdiği "yapılabilir işlemler" gelir (Teknik Mimari `05 §4`, §6). Uçların ve yanıtların tanımı bu aşamanın kararıdır.
> - **K-902 · K-941 · K-947 · Gövde tavanı ve hız sınırı yanıtı:** istek gövdesinin tavanı uç başınadır — ucun taşıdığı en büyük meşru içerik kadar (görselde P-23, parçalı yüklemede bir parçanın boyu); genel hız sınırının aşımı nötr mesajlı bir yanıttır; imzalı makine uçları — bugün ödeme sağlayıcısının bildirim ucu — imzası doğru istekte IP sınırına girmez, imzası tutmayan istek sınıra sayılır; panelin parçalı dosya yükleme ucu form gönderimleri sınırına (dakikada 30) girmez, sayfa isteği sınırına (dakikada 300) girer ve yönetici oturumu ile sahteciliğe karşı koruma ister — parçanın boyu bu sınırla birlikte seçilir (`05 §6.3`; `02 §6.1.5`, §11.2 P-26). Uç başına değerler, parçanın boyu, sınırın pencere türü ve yanıtın biçimi bu aşamanın kararıdır.
> - **K-914 · K-918 · K-974 · K-978 · Hata yanıtları ve sağlık uçları:** sayfa isteğindeki beklenmeyen hata çerçevenin içinde bir hata sayfasıyla 500 döner; form isteğindeki hata formu gönderilen değerleriyle ve kalıcı mesajla yeniden gösterir; sonucu belirsiz işlemler (`04` 2.7.3.3) hatada sonucu gösteren ekrana yönlendirir; makine uçları 500 döner — sağlayıcının yeniden gönderip göndermediği Entegrasyon Spesifikasyonu'nda (`08`) doğrulanır, gönderilmezse Bekliyor siparişin sonucunu son sorgu bulur; iptal edilmiş siparişe gelen geç ödemenin bildirimi kaybolur; firmanın "gerçekleşmedi" işaretinden sonra kart iadesinin geç sonucu da ürüne ulaşmaz — kalan risk (`05 §10.3.5`, §10.1.3, §4.6.2; K-974, K-978). Canlılık ucu uygulama ve veritabanı yanıt veriyorsa 200, durum ucu bir uyarı koşulu varsa 503 döner; ikisi de ayrıntı taşımaz (Teknik Mimari `05 §9`, §10). Uçların adresi ve yanıtların biçimi bu aşamanın kararıdır.
> - **Dizin — Aşama 4 kararlarının bu dokümana devirleri:** karar kaydında K-855…K-979'un etki sütunlarında bu dokümanı gösteren yirmi atıf (yirmi karar) ve Teknik Mimari'nin (`05`) gövdesinde bu dokümana iş bırakan cümleler [`CHECKPOINT_REPORTS/PHASE4_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE4_CONFLICT_SCAN.md) §6'dadır; Aşama 4'ün atıflarının hepsi işin adını taşır. Aşama 4'ün checkpoint'inin kararları (K-980…K-988) ve K-978'in bu dokümana kolu (hata yanıtları satırı) tarama raporunda yoktur — iş yukarıdaki satırlarda etiketiyle durur. K-977'nin satırı Aşama 1 bloğunun K-17 satırıdır. Karar satırı olmadan devredilen iki iş: sayfa adreslerinin önekleri (`05 §3.7.1`; Aşama 3'ün dizinindeki `04 §3.5` devriyle aynı iş) ve parçalı yüklemede sahteciliğe karşı doğrulama değerinin istek başlığındaki biçimi (`05 §6.5.1` — Kodlama Kılavuzu'yla birlikte). Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Önceki aşamaların dizinleri: `PHASE1_CONFLICT_SCAN.md` §6, `PHASE2_CONFLICT_SCAN.md` §6, `PHASE3_CONFLICT_SCAN.md` §6.

> **Aşama 5'ten park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §14.2, yazım turu ve kapanış sırasının 2. maddesi).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-1011 · İç anahtar ve dışa görünen kimlikler:** tabloların anahtarı uygulamanın ürettiği zamana göre sıralı UUID'dir; vitrinde ürün, kategori ve içerik sayfaları sayfa adresiyle, sipariş sayfası numara ve e-postayla ya da erişim anahtarıyla açılır — iç anahtar vitrinin adresinde kullanılmaz; müşterinin kendi kaydını gösteren istekte (adres defteri girdisi, sepet kalemi) iç anahtar taşınabilir ve kaydın o oturumun hesabına ya da sepetine ait olduğu sunucuda denetlenir (Veri Modeli `06 §2`). Adreslerin ve isteklerin biçimi bu aşamanın kararıdır.
> - **K-1070 · Sayfa adreslerinin önekleri:** sayfa adresinin kayda ait parçası (`Slug`) kayıt türü içinde tekildir — ürün, kategori, hizmet tanıtımı, referans iş ve genel sayfa ayrı tablolarda kendi tekilliklerini taşır; iki türde aynı değer bulunabilir (Veri Modeli `06 §4.72`; `02 §3.30.1`, §3.30.2). Bu yüzden her türün öneki birbirinden ve sabit sayfaların adreslerinden — hizmetler, referanslar, SSS, Hakkımızda, İletişim, sepet, hesap sayfaları — ayrık olmalıdır; aksi hâlde iki kayıt aynı adresi gösterir. Öneklerin kendisi ve adresin istekteki biçimi bu aşamanın kararıdır.

---

## 1. Traceability Matrix (ÖNCE BU)

> **Ne yazılır:** `04` ekran aksiyonları + `03` akış adımları → endpoint eşlemesi.
> Ekrandaki her aksiyon ve her veri ihtiyacı bir endpoint'e eşlenmelidir. Eşlenmeyen aksiyon = **eksik endpoint**.

### 1.1 İleri izlenebilirlik (ekran aksiyonu / akış adımı → endpoint)

| Kaynak ID | Kaynak özeti | Endpoint | Durum |
|---|---|---|---|

### 1.2 Geri izlenebilirlik (endpoint → kaynak)

| Endpoint | Beslendiği kaynak | Durum |
|---|---|---|

### 1.3 Boşluklar (GAP) ve kararlar

| # | Boşluk | Karar | Nereye yansıdı |
|---|---|---|---|

---

## 2. Konvansiyonlar (ENDPOINT'LERDEN ÖNCE)

> **Ne yazılır:** URL yapısı · yanıt zarfı veya standart hata formatı · kimlik doğrulama · yetkilendirme · sayfalama · filtreleme · sıralama · sürümleme · idempotency · hız sınırlama · tarih/sayı biçimi.
>
> **Neden önce:** Konvansiyonlar baştan sabitlenip onaylandığında endpoint detayları hem hızlı hem tutarlı yazılır; her endpoint'te aynı tartışma tekrarlanmaz.

| # | Konvansiyon | Karar | Gerekçe |
|---|---|---|---|

## 3. Endpoint envanteri

| # | Method + Path | Amaç | Aktör / yetki | Kaynak ekran/akış |
|---|---|---|---|---|

## 4. Endpoint tanımları

### <METHOD> <path>

- **Amaç:** · **Yetki:** · **Hız sınırı:**
- **Request:** (parametreler, gövde, doğrulama kuralları)
- **Response (başarı):**
- **Response (hata):** (hata kodları — `06` ve `02` ile birebir)
- **Yan etkiler:** (durum geçişi, olay yayını, bildirim, denetim kaydı)
- **Idempotency:**

## 5. DTO envanteri

> **Kural:** Alan adları ve enum değerleri `06_DATA_MODEL.md` ile **birebir** eşleşir.

## 6. Hata kodları

| Kod | HTTP | Anlamı | Nerede döner |
|---|---|---|---|

## 7. Gerçek zamanlı olaylar (varsa)

## 8. İstemci karmaşıklığını azaltan kalıplar

> **Ne yazılır:** Durum × rol iş mantığının sunucuda tutulduğu yerler. Örneğin yanıta "şu an yapılabilecek aksiyonlar" listesi koymak, istemcinin bu mantığı kopyalamasını önler.

## 9. Yönetim (admin) endpoint'leri

## 10. Değişiklik kaydı

> **Ne yazılır:** `08` yazılırken bu dokümana **geriye dönük** eklenen endpoint'ler. Entegrasyon dokümanı doğası gereği yeni endpoint ihtiyacı doğurur; bu beklenen bir durumdur.

---

*Shopfolio — API Design v0.1*
