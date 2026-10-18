# Shopfolio — API Design

**Versiyon: v0.1** | **Bağımlılıklar:** `02`, `03`, `04`, `05`, `06`, `10_MVP_SCOPE.md` | **Son güncelleme:** 2026-10-18

> **Aşama:** 6 — API Tasarımı · **Rol:** API Designer
> **Traceability zorunlu: EVET** — §1 tamamlanmadan §3'e (endpoint envanteri) geçilmez.

> **Aşama 1'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36.
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-06 · K-97 · Aktörler:** Dört aktör — ziyaretçi · misafir alıcı · üye müşteri · firma yöneticisi (`02 §1.3`; misafir alıcıyı K-97 ekledi, satır Aşama 2'nin çakışma taramasında hizalandı); yetkilendirme tasarımı **tek yönetici rolü** varsayar (çoklu kullanıcı, aynı yetki).
> - **K-17 · K-977 · K-988 · K-1036 · K-1038 · Terim sözlüğü:** `02 §1` sözlüğü **birebir devralınır**; endpoint, alan ve enum adları sözlüğün İngilizce karşılıklarından türer; sözlükte terimi olmayanların adını Veri Modeli (`06`) verir — alan adları `06 §4`'ten, kapalı liste değerlerinin, kayıt düzeyindeki kapalı değerlerin ve kimlikli listelerin kod adları `06 §5`'ten, biçimini Teknik Mimari'nin kurduğu teknik kavramların adları `06 §3.3`'ten alınır (K-531, K-977, K-988, K-1036 (2), K-1038 (1)); eş anlamlı ad kullanılmaz (satır Aşama 4'ün çakışma taramasında ve checkpoint'inde, Aşama 5'in çakışma taramasında hizalandı).

> **Aşama 2'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §8.3 AK0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **Dizin — Aşama 2 kararlarının bu dokümana devirleri:** karar kaydında K-647…K-718'in etki sütunlarında bu dokümanı gösteren bir atıf (bir karar) [`CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md) §6.1'dedir; etki sütununda yalnız doküman numarası taşıyan bir atfın işinin adını dizin verir. Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Aşama 1'in dizini: `PHASE1_CONFLICT_SCAN.md` §6.

> **Aşama 3'ten park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2 ve §3, mekanizma: K-36 (yeri: §10.3 UI0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-787 · K-794 · K-795 · K-1077 · İşlemlerin adedi:** müşterinin iptal, cayma ve ayıp talebi işlemleri ile yöneticinin firma iptali, adet azaltması, iade teslim alması, iade reddi ve "mal dönmedi" kapanışı kalem başına bir adet taşır; adet o işleme açık adedi aşamaz ve gecikme feshinde seçim yoktur (K-796; `02 §7.1.6`; Arayüz Tanımları `04` 2.12.1.8, §5.17, §5.18, §5.19). Ekranın kalemin o işleme açık adedini ve varsayılanı göstermesi için (ayıpta sözleşmeden dönülen adet işleme açık adetten ayrıca düşülür; kalemin açık adedi ve kapanışı değişmez — `02 §7.1.6`; K-1077; Veri Modeli `06 §2.7.2`) gereken verinin nasıl sunulduğu ve aşan adedin sunucuda nasıl reddedildiği bu aşamanın kararıdır.
> - **K-737 · Ürün listesinin araması:** panelin ürün listesi ürün adıyla aranır ve yayın durumuna göre süzülür (`02 §10.1.2`; `04 §9.4`). Arama ve süzme parametrelerinin biçimi bu aşamanın kararıdır.
> - **K-802 · Sipariş listesinin araması ve süzgeçleri:** sipariş listesi sipariş numarası ya da iletişim e-postasıyla tam eşleşmeyle aranır; sipariş durumu, ödeme durumu ve bekleyen iş süzgeçleriyle süzülür; en yeni önce, süreli bekleyen işte kalan süresi en az olan önce sıralanır (`04 §9.8`). Parametrelerin biçimi bu aşamanın kararıdır.
> - **Dizin — Aşama 3 kararlarının bu dokümana devirleri:** karar kaydında K-723…K-850'nin etki sütunlarında bu dokümanı gösteren altı atıf (altı karar) ve Arayüz Tanımları'nın (`04`) gövdesinde bu dokümana iş bırakan cümleler [`CHECKPOINT_REPORTS/PHASE3_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE3_CONFLICT_SCAN.md) §6'dadır; Aşama 3'ün atıflarının hepsi işin adını taşır. Bir iş yalnız `04`'ün gövdesinde yaşar, karar satırı yoktur: sitenin dışından gelinen sayfa adreslerinin biçimi (`04 §3.5`; adresin kaydın adından üretilmesi `02 §3.30.2`'dedir). Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Önceki aşamaların dizinleri: `PHASE1_CONFLICT_SCAN.md` §6, `PHASE2_CONFLICT_SCAN.md` §6.

> **Aşama 4'ten park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §12.3 TM0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-868 · Sözleşmelerin biçimi:** sayfalar sunucuda Razor Pages ile üretilir; ayrı bir JSON API katmanı yoktur — bu dokümanın uç envanteri sayfa ve form uçlarını (gönder–yönlendir–göster) ve az sayıdaki makine ucunu — ödeme sağlayıcısının bildirimi, parçalı dosya yükleme, indirme, site haritası ve robots.txt (ikisi aynı makine ucundan üretilir — `05 §3.7.4`), canlılık ve durum uçları — taşır; dış servisten gelen ve ürüne veri getiren isteği alan yeni bir uç imzalıdır ve listeye karar kaydıyla girer; canlılık ve durum uçları imzasız açık GET uçlarıdır (Teknik Mimari `05 §2` MK-04, §3.6.1). Şablonun endpoint, DTO ve hata kodu bölümlerinin bu biçime nasıl uygulanacağı bu aşamanın kararıdır.
> - **K-873 · K-874 · K-980 · K-1014 · K-1021 · K-1051 · K-1063 · Sürümün taşınması:** sürüm taşıyan kayıtların düzenleme formları — `04`'ün OB-10 kayıt varyantının ekranları; ayarlarda her bölüm kendi sürümünü taşır (Veri Modeli `06 §2.6.2`, §2.6.3) — kaydın açıldığı andaki sürümü taşır; yöneticinin sipariş işlemleri ekranda gördüğü sürümü taşır (siparişin sürümünü artıran ve artırmayan yazımlar `06 §2.6.4`'te). Sürüm yerine ekranın gösterdiği değere karşı koşullu yazılanlar istekte o değeri taşır: ürün formunun stok ve kontenjan alanı — alan ürünün sürümüyle değil o değere karşı yazılır (`05 §7.3.1`; K-980) —, iç not, kurumsal içeriğin elle sırası ve geçici kapatma anahtarı. OB-10'u olmayan kayıtlar — kategori, iletişim talebi, yönetici hesapları, üye kaydı, adres defteri — sürüm taşımaz (`06 §2.6.5`). Sürüm değişmişse yanıt kaydın ya da siparişin değiştiğini söyler ve güncel hâli gösterir; "Yine de kaydet" formu güncel sürümle yeniden gönderir (`05 §7`; `04 §2.10`). Sürüm alanlarının adları `06 §4`'tedir (`Version` ve bölüm sürümleri). Değerin formda taşınışı ve yanıtların biçimi bu aşamanın kararıdır.
> - **K-883 · K-884 · Dosya uçları:** dijital ürün dosyası parçalı ve kaldığı yerden sürdürülebilir biçimde yüklenir; indirme düğmesi kaleme bağlı, bir saat ömürlü bir bağlantı üretir ve bu bağlantıdaki devam istekleri (HTTP aralık istekleri) yeni hak düşürmez; ömür yalnız isteğin kabulünde denetlenir — ömür dolarken süren aktarım kesilmez (`05 §7.7.4`; `02 §3.12.5`). Uçların tanımı bu aşamanın kararıdır.
> - **K-893 · K-899 · K-904 · K-912 · K-941 · K-970 · K-971 · Formlar, kimlik şemaları ve erişim:** ödeme adımı formu tek kullanımlık bir onay anahtarı taşır ve ikinci gönderim var olan siparişin sayfasına götürür; ödeme sağlayıcısının bildirim ucu imzası doğru bildirimi kaydedip hemen "alındı" der; aynı bildirim ikinci kez gelirse kaydedilmez, işlenmez ve yine "alındı" alır — tekil olan bildirimin kendisidir (`05 §4.5.2`; K-970); imzası tutmayan istek kaydedilmez, işlenmez ve 4xx alır; müşteri ve yönetici iki ayrı kimlik şemasıyla, ayrı adlı çerezlerle girer; yeniden doğrulamadan dönülen işlem formu tek kullanımlık bir değer taşır — izin o değere bağlıdır, gönderim onu tüketir, formun gösterimi dışındaki ilk sayfa isteği izni düşürür (`05 §6.1.5`; K-971); e-postayla gelen bağlantı yalnız ekranı açar — doğrulama ve geri alma ekrandaki düğmenin gönderdiği POST isteğiyle olur; panel varsayılan olarak kapalıdır, kayıt düzeyinde erişim sunucuda süzülür ve ekrana sunucunun karar verdiği "yapılabilir işlemler" gelir (Teknik Mimari `05 §4`, §6). Uçların ve yanıtların tanımı bu aşamanın kararıdır.
> - **K-902 · K-941 · K-947 · Gövde tavanı ve hız sınırı yanıtı:** istek gövdesinin tavanı uç başınadır — ucun taşıdığı en büyük meşru içerik kadar (görselde P-23, parçalı yüklemede bir parçanın boyu); genel hız sınırının aşımı nötr mesajlı bir yanıttır; imzalı makine uçları — bugün ödeme sağlayıcısının bildirim ucu — imzası doğru istekte IP sınırına girmez, imzası tutmayan istek sınıra sayılır; panelin parçalı dosya yükleme ucu form gönderimleri sınırına (dakikada 30) girmez, sayfa isteği sınırına (dakikada 300) girer ve yönetici oturumu ile sahteciliğe karşı koruma ister — parçanın boyu bu sınırla birlikte seçilir (`05 §6.3`; `02 §6.1.5`, §11.2 P-26). Uç başına değerler, parçanın boyu, sınırın pencere türü ve yanıtın biçimi bu aşamanın kararıdır.
> - **K-914 · K-918 · K-974 · K-978 · Hata yanıtları ve sağlık uçları:** sayfa isteğindeki beklenmeyen hata çerçevenin içinde bir hata sayfasıyla 500 döner; form isteğindeki hata formu gönderilen değerleriyle ve kalıcı mesajla yeniden gösterir; sonucu belirsiz işlemler (`04` 2.7.3.3) hatada sonucu gösteren ekrana yönlendirir; makine uçları 500 döner — sağlayıcının yeniden gönderip göndermediği Entegrasyon Spesifikasyonu'nda (`08`) doğrulanır, gönderilmezse Bekliyor siparişin sonucunu son sorgu bulur; iptal edilmiş siparişe gelen geç ödemenin bildirimi kaybolur; firmanın "gerçekleşmedi" işaretinden sonra kart iadesinin geç sonucu da ürüne ulaşmaz — kalan risk (`05 §10.3.5`, §10.1.3, §4.6.2; K-974, K-978). Canlılık ucu uygulama ve veritabanı yanıt veriyorsa 200, durum ucu bir uyarı koşulu varsa 503 döner; ikisi de ayrıntı taşımaz (Teknik Mimari `05 §9`, §10). Uçların adresi ve yanıtların biçimi bu aşamanın kararıdır.
> - **Dizin — Aşama 4 kararlarının bu dokümana devirleri:** karar kaydında K-855…K-979'un etki sütunlarında bu dokümanı gösteren yirmi atıf (yirmi karar) ve Teknik Mimari'nin (`05`) gövdesinde bu dokümana iş bırakan cümleler [`CHECKPOINT_REPORTS/PHASE4_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE4_CONFLICT_SCAN.md) §6'dadır; Aşama 4'ün atıflarının hepsi işin adını taşır. Aşama 4'ün checkpoint'inin kararları (K-980…K-988) ve K-978'in bu dokümana kolu (hata yanıtları satırı) tarama raporunda yoktur — iş yukarıdaki satırlarda etiketiyle durur. K-977'nin satırı Aşama 1 bloğunun K-17 satırıdır. Karar satırı olmadan devredilen iki iş: sayfa adreslerinin önekleri (`05 §3.7.1`; Aşama 3'ün dizinindeki `04 §3.5` devriyle aynı iş) ve parçalı yüklemede sahteciliğe karşı doğrulama değerinin istek başlığındaki biçimi (`05 §6.5.1` — Kodlama Kılavuzu'yla birlikte). Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Önceki aşamaların dizinleri: `PHASE1_CONFLICT_SCAN.md` §6, `PHASE2_CONFLICT_SCAN.md` §6, `PHASE3_CONFLICT_SCAN.md` §6.

> **Aşama 5'ten park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §14.2, yazım turu ve kapanış sırasının 2. maddesi).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-1011 · K-1068 · İç anahtar ve dışa görünen kimlikler:** tabloların anahtarı uygulamanın ürettiği zamana göre sıralı UUID'dir — tek istisna, çatının yazdığı veri koruma anahtarı tablosudur (K-1068; `06 §2.3.1`); vitrinde ürün, kategori ve içerik sayfaları sayfa adresiyle, sipariş sayfası numara ve e-postayla ya da erişim anahtarıyla açılır — iç anahtar vitrinin adresinde kullanılmaz; müşterinin kendi kaydını gösteren istekte (adres defteri girdisi, sepet kalemi) iç anahtar taşınabilir ve kaydın o oturumun hesabına ya da sepetine ait olduğu sunucuda denetlenir; panelde iç anahtar serbesttir — bütün yöneticiler bütün kayıtları görür (`02 §10.2.3`) (Veri Modeli `06 §2.3.4`). Adreslerin ve isteklerin biçimi bu aşamanın kararıdır.
> - **K-1070 · Sayfa adreslerinin önekleri:** sayfa adresinin kayda ait parçası (`Slug`) kayıt türü içinde tekildir — ürün, kategori, hizmet tanıtımı, referans iş ve genel sayfa ayrı tablolarda kendi tekilliklerini taşır; iki türde aynı değer bulunabilir (Veri Modeli `06 §4.72`; `02 §3.30.1`, §3.30.2). Bu yüzden her türün öneki birbirinden ve sabit sayfaların adreslerinden — hizmetler, referanslar, SSS, Hakkımızda, İletişim, sepet, hesap sayfaları — ayrık olmalıdır; aksi hâlde iki kayıt aynı adresi gösterir. Öneklerin kendisi ve adresin istekteki biçimi bu aşamanın kararıdır.
> - **K-1100 · K-1118 · Silinmiş yüklemeyle form kaydı:** bir günden uzun açık tutulan formun yüklemesi — ve bir günü aşan parçalı dijital dosya yüklemesinin sürdürülmesi — silinir: görsel ve tamamlanmış dosya yüklemesi imhada, parçalı yükleme gecelik temizlikte; formun kaydı görselde kısıtlayıcı bağa takılır, dosyada bağlanacak satırı bulamaz (Veri Modeli `06 §4.69`, §4.70). Kaydın hata yanıtı bu aşamanın kararıdır: yanıt hangi yüklemenin artık olmadığını söyler ve yeniden yüklemeyi ister; ekrandaki hâl Arayüz Tanımları'ndadır (`04` 2.12.8.7 — Veri modeli aşamasının etki yansıtmasında yazıldı; K-1130). Parçalı yükleme ucunun, satırı gecelik temizlikte silinmiş yüklemeye gelen sürdürme isteğine yanıtı da bu aşamanın kararıdır — yükleme kaldığı yerden sürmez, baştan başlar (Veri Modeli `06 §4.70` `UploadCompletedAt`; `04` 2.12.8.7).
> - **Dizin — Aşama 5 kararlarının bu dokümana devirleri:** karar kaydında K-992…K-1136'nın etki sütunlarında bu dokümanı gösteren on altı atıf (on altı karar) ve Veri Modeli'nin (`06`) gövdesinde bu dokümana iş bırakan cümleler [`CHECKPOINT_REPORTS/PHASE5_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE5_CONFLICT_SCAN.md) §6'dadır; Aşama 5'in atıflarının hepsi işin adını taşır. Üç karar etki sütununda "park gerekmez" der (K-1034, K-1062, K-1066); yedisi bu dokümanın Aşama 1, 3 ve 4 park satırlarını güncelledi — iş o satırlarda etiketiyle durur. Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Önceki aşamaların dizinleri: `PHASE1_CONFLICT_SCAN.md` §6, `PHASE2_CONFLICT_SCAN.md` §6, `PHASE3_CONFLICT_SCAN.md` §6, `PHASE4_CONFLICT_SCAN.md` §6.

---

## 1. Traceability Matrix (ÖNCE BU)

> **Ne yazılır:** `04` ekran aksiyonları + `03` akış adımları → endpoint eşlemesi.
> Ekrandaki her aksiyon ve her veri ihtiyacı bir endpoint'e eşlenmelidir. Eşlenmeyen aksiyon = **eksik endpoint**.

**Alt bölüm haritası (K-1152).** Matris yedi oturumda kurulur (K-1150): **1a** (2026-10-18) kaynak birimini netleştirdi (K-1151), dört ailenin envanterini ve 1a'nın kapsama denetimini, aday uç listesini ve "Uç" sütununun değer kümesini (K-1153) ve Arayüz Tanımları'nın vitrin ekranlarının (`04 §5`), navigasyon haritasının (§3), ortak bileşenlerinin (§2) ve ekran olmayan yüzeylerinin (§4.4) ileri izlenebilirliğini yazdı; sonraki oturumların kapsamı tablonun satırlarıdır. Şablonun §1.1, §1.2 ve §1.3 numaraları değişmez; alt düzey aşağıdaki tabloyla sabittir. Bir oturum bir alt bölümü bölmek ya da birleştirmek zorunda kalırsa bu tabloyu ve ona atıf yapan satırları aynı PR'da günceller (K-670'in kalıbı; `checklists/document-stage.md` §4).

| Alt bölüm | İçerik | Oturum | Durum |
|---|---|---|---|
| §1.1.1 | Kaynak envanteri ve sayım — dört aile, açılışın sayımıyla fark, betik komutları, kapsama denetimi | 1a; her oturum kendi ailesinin denetimini ve okuma derinliği kaydını ekler | ✓ (dört ailenin envanteri; 1a'nın denetimi) |
| §1.1.2 | Aday uç listesi (`UC-nn`) ve "Uç" sütununun değer kümesi | 1a; 1b–1f listenin sonuna aday ekler; 2 geri yönde kapatır | ✓ — UC-01…UC-109 (sonraki oturum ekleme yaparsa günceller) |
| §1.1.3 | Arayüz Tanımları — `04 §5` vitrin ekranları: 28 ekran, 364 kalın numaralı madde, 23 kimliksiz madde — 415 | 1a | ✓ |
| §1.1.4 | Arayüz Tanımları — `04 §3` navigasyon haritası: 124 geçiş satırı, 5 kalın numaralı paragraf, 6 metin — 135 | 1a | ✓ |
| §1.1.5 | Arayüz Tanımları — `04 §2` ortak bileşenler bileşen kimliğiyle (20) ve §4.4 ekran olmayan yüzeyler (12 satır ve metin) — 33 | 1a | ✓ |
| §1.1.6 | Arayüz Tanımları — `04 §9` panel ekranları: 25 ekran, 371 kalın numaralı madde, 34 kimliksiz madde — ve §4.2'nin metni — 431 | 1b | ⬚ |
| §1.1.7 | Arayüz Tanımları — `04 §6` durum × rol: 286 satır, 53 ekran bloğu, §6.1'in 17 kalın numaralı paragrafı, 2 metin — 358 | 1c | ⬚ |
| §1.1.8 | Arayüz Tanımları — `04 §7` formlar: 42 form, 193 alan satırı, §7.3'ün 5 kuralı — ve §8 alt bölüm düzeyinde (3) — 243 | 1c | ⬚ |
| §1.1.9 | Kullanıcı Akışları — `03 §1`–§4 satır düzeyinde: numaralı satır ve kalın paragraf 420, durum 20, geçiş 16, madde 45, metin 35 — 536 | 1d | ⬚ |
| §1.1.10 | Kullanıcı Akışları — `03 §5`–§10 satır düzeyinde (§7 toplu: 3 alt bölüm satırı ve bölüm metni; bağlantı taşıyan bildirim satırı kimliğiyle ayrıca): numaralı satır 259, madde 10, metin 18, §7 4 — 291 | 1e | ⬚ |
| §1.1.11 | Devirler — 51 karar satırı, 14 park maddesi, 4 dizin satırı, 20 gövde cümlesi, başka dokümanın 1 park satırı — 90 | 1f | ⬚ |
| §1.1.12 | Teknik Mimari — MK-04, uç düzeyindeki 13 bölümün 96 numaralı maddesi ve 5 kimliksiz maddesi — 102 | 1f | ⬚ |
| §1.1.13 | GAP adayları (`AA-n`) | 1a açtı; 1b–1f ekler; 2 karara bağlar | ⬚ — 1 aday (AA-1) |
| §1.2 | Geri izlenebilirlik (uç → kaynak); gerekçesiz ekleme listesi; kesilmiş okumanın örneklemi; aday uç listesinin kapanışı | 2 | ⬚ |
| §1.3 | Boşluklar (GAP) ve kararlar; üst dokümanlara geri besleme tablosu (K-1147) | 2; workshop ve yazım geri besleme satırı ekler | ⬚ |

### 1.1 İleri izlenebilirlik (ekran aksiyonu / akış adımı → endpoint)

Her satır şablonun dört sütununu taşır: **Kaynak ID · Kaynak özeti · Uç · Durum** — "Uç" şablonun "Endpoint" sütunudur (K-1153). **Kaynak ID** kaynağın kendi kimliğidir (K-1148): Arayüz Tanımları'nda ekran `04` E-n, kalın numaralı madde, tablo satırı ve kalın paragraf `04` n.m.k, ortak bileşen `04` OB-n, kimliksiz madde `04 §n.m` mk — k bağlamdaki sırasıdır —, alt bölümün kural ve madde dışındaki metni `04 §n.m` metin, alt bölüm düzeyindeki birim `04 §8.n`; Kullanıcı Akışları'nda K-1002'nin biçimleri (`03 §n.m.k`, `03 §n.m` S1, `03 §n.m` `Placed`, `03 §n.m` mk, `03 §n.m` metin, `03 §7.n`); devirde `K-n`, `park An-m`, `dizin An`, `0X §bölüm gN`, `0X park An-m`; Teknik Mimari'de `05 §n.m.k`, `05 §2.1` MK-04, `05 §n.m` mk (K-1151). **Kaynak özeti** satırın uca dokunan özetidir — kuralın evi kaynaktır, özet onu kopyalamaz. **Uç** §1.1.2'nin değer kümesinden bir ya da birkaç değerdir (` · ` ile): aday uç `UC-nn`, konvansiyon (`konvansiyon — APIn-mm`) ya da adıyla yazılmış bir "uç doğurmaz" gerekçesi. **Durum** dört değerden biridir ve "Uç" hücresinden türer: **eşlendi** (en az bir aday uç), **konvansiyon** (aday uç yok; kural bütün uçlara ya da bir uç sınıfına uygulanır, evi `07 §2` ya da §5, §6, §8 — gösterilen konuda), **uç doğurmaz** (yalnız "uç doğurmaz" değerleri — GAP değildir), **GAP adayı (AA-n)** (§1.1.13'te numarasıyla durur). Atıflar K-669 ve K-701'in okunuşuyla yazılır; parantez içindeki önsüz numara (5.4.10, 2.12.1.8) satırın kaynak dokümanının maddesidir.

#### 1.1.1 Kaynak envanteri ve sayım

Birim K-1150'dir; 1a'nın netleştirmesi K-1151'dedir. Sayımlar 2026-10-18'de, `02` v0.89, `03` v0.38, `04` v0.36, `05` v0.25, `06` v0.27, `10` v0.55 ve karar kaydı v0.145 üzerinde alındı. Açılışın sayımları (karar kaydı §16.2'nin girdiler notu) çerçevedir; 1a envanteri kalıp dışı içerikle birlikte yeniden çıkardı ve fark aşağıdaki tablonun "Fark" sütunundadır — her farkın komutu betik bloğundadır (checklist §1, §2; K-1000'in dersi).

| Aile | Açılışın sayımı (K-1150) | Betiğin saydığı (1a) | Fark ve sebebi | Oturum | Matrise girmeyen |
|---|---|---|---|---|---|
| `04` ekranlar | 53 | 53 — `### 5.n E-n` 28, `### 9.n E-n` 25 | — | 1a 28 · 1b 25 | §4.1 ve §4.2'nin envanter satırları — aynı ekranlar, ekran birimiyle girer; §4.3 — aday listeden envantere farklar, envanterin değişim kaydı |
| `04 §5` ve §9'un kalın numaralı maddeleri | 364 · 371 | 364 · 371 | — | 1a · 1b | — |
| `04 §5` ve §9'un kimliksiz maddeleri | — | 23 · 34 | **+57:** içerik taşıyan alan başlıkları (§5 15, §9 20) ve numaralı maddenin altındaki liste maddeleri (§5 8 — 5.13.9'un (a)–(h); §9 14 — 9.13, 9.16 ve 9.19'un alt maddeleri) | 1a · 1b | İçeriği yalnız bir işaret olan alan başlıkları — "Aksiyonlar", "(OB-07)", "(tam matris §6.2'de — …)", "(alan envanteri §7.1'de — …)" (§5 52, §9 39); maddenin girintili devam satırı maddenin parçasıdır |
| `04 §3` navigasyon | 124 | 135 — satır 124, kalın numaralı paragraf 5 (3.6.1–3.6.5), metin 6 | **+11:** §3.6'nın beş kalın paragrafı tablo satırı değildir; altı alt bölümün tablo dışı metni (§3.6 hariç) | 1a | — |
| `04 §2` ortak bileşenler | 20 | 20 — bileşen başına bir birim (`### 2.n` ve `#### 2.12.n` başlığındaki OB kimliği) | — | 1a | §2'nin giriş tablosu — bileşenlerin özetidir, satırı bileşenin birimindedir; bileşenin kalın maddeleri ve tabloları birim değildir, bileşen satırıyla tam okunur (K-1150) |
| `04 §4.4` ekran olmayan yüzeyler | 12 | 13 — satır 12, metin 1 | **+1:** alt bölümün giriş cümlesi | 1a | — |
| `04 §4.2` metni | — | 1 | **+1:** panel ekranlarının aktörü ve tek rol cümlesi — yetkinin kaynağı | 1b | — |
| `04 §6` durum × rol | 286 | 358 — satır 286, ekran bloğu 53 (6.2.n, 6.3.n), §6.1'in kalın numaralı paragrafı 17, metin 2 | **+72:** blok başlıkları kalın paragraftır ve ekranın durumdan bağımsız notunu taşır; §6.1'in okunuş kuralları | 1c | — |
| `04 §7` formlar | 42 form · 193 alan | 240 — form 42, alan satırı 193, §7.3'ün kalın kuralı 5 | **+5:** §7.3'ün ortak alan kuralları | 1c | Tablo başlıkları |
| `04 §8` | alt bölüm düzeyinde | 3 — §8.1, §8.2, §8.3 | — | 1c | — |
| **`04` toplamı** | **1.468** | **1.615** | **+147** | | Bütün bölümlerde: `*Kaynak:` satırları, "Ne yazılır" notları, "Yazım turunun …" oturum notları (9), dosya sonundaki sürüm dipnotu, §1 (Arayüz Tanımları'nın kendi matrisi — K-1150) |
| `03` numaralı satırlar ve kalın paragraflar | 794 · 16 | 682 — §7 dışındaki numaralı satır 663, kalın paragraf 16 (§1); §7 toplu: 3 alt bölüm satırı | **−131 + 3:** §7'nin 131 bildirim satırı toplu eşlenir (K-1150); bağlantı taşıyan satır 1e'de kimliğiyle ayrıca girer — isteğe bağlı küme `opt03()` (131; "bağlantı" geçen 18 satır adaydır) | 1d · 1e | — |
| `03` kalıp dışı birimler (K-1002'nin biçimi) | — | 145 — durum 20, geçiş 16, madde 55, metin 54 (§7'nin bölüm metni dahil) | **+144 + 1:** K-1002 bu birimleri yalnız `03 §1` ve §4'te saydı; K-1150 bütün bölümleri satır düzeyinde istediği için birim bütün bölümlere uygulanır (K-1151) | 1d · 1e | `03 §0` (yazım konvansiyonu), `03 §11` (`02`'ye geri besleme tablosu) |
| **`03` toplamı** | **810** | **827** — 1d 536, 1e 291 | **+17** | | |
| Devir satırları — K-479…K-1143, etki sütununda `07` | 51 | 51 | — | 1f | Aşama 1'in 72 workshop satırı — kural ailesiyle karşılanır (karar kaydı §16.2, betik 2); Aşama 6'nın kendi kararları (K-1144 ve sonrası) |
| Park maddeleri ve dizin satırları | 14 · 4 | 14 · 4 | — | 1f | — |
| Gövde cümleleri — önceki dokümanların gövdesinde `07` ya da "API Tasarım" | 19 | 20 — `02` 5, `04` 1, `05` 7, `06` 7 | **+1:** `02 §1.1` g2 — teknik kavramların adının `06`'dan alınacağı cümle (K-977); Aşama 1'in dizininden sonra, Aşama 4'ün kapanışında eklendi — dizin kapanışın anlık görüntüsüdür (checklist §4; K-1003'ün dersi) | 1f | Başlık ve sürüm notları, park blokları, `04 §1` ve `06 §1` (iki dokümanın kendi matrisi), `05`'in §0'dan önceki başlık notu ve §12 |
| Başka dokümanların park satırları — `08`, `09`, `11`, `12`, `DEPLOY_RUNBOOK` | 1 | 1 — `08 park A4-3` (K-906 · K-973) | — | 1f | — |
| **Devir toplamı** | **89** | **90** | **+1** | | |
| Teknik Mimari — MK-04 ve uç düzeyindeki bölümler | 96 madde ve MK-04 | 102 — MK-04 1, numaralı madde 96, kimliksiz madde 5 (`05 §6.5`'in güvenlik başlıkları listesi) | **+5:** liste maddeleri | 1f | Bölümlerin `*Kaynak:` satırları ve tablo başlıkları |
| **Genel toplam** | **2.464** | **2.634** | **+170** | | |

**1a — bölüm bazında** (583): `04 §5` 415 (ekran 28, kalın numaralı madde 364, kimliksiz madde 23 — alan başlığı 15, 5.13.9'un alt maddesi 8) · §3 135 (satır 124, kalın paragraf 5, metin 6) · §2 20 · §4.4 13. §1.1.3 415 · §1.1.4 135 · §1.1.5 33.

**Betik komutları** (Git Bash; depo kökünden). Sayımlar yukarıdaki sayılarla aynı çıkmalı; 1a'nın kapsama denetimleri — eksik, fazla, yinelenen, başlığa bağlılığın üçü —, değer kümesi denetimi, "Durum" denetimi, aday listesi denetimi ve GAP adayı denetimi boş dönmelidir:

```sh
# API Tasarımı (07) — izlenebilirlik matrisinin kaynak envanteri (Git Bash; depo kökünden)
# birim kuralı K-1150, K-1151; aile 1: Arayüz Tanımları (04) §2–§9
exp04() {
  awk '
    function unit(x) { print x }
    function mk(c)   { m[c]++; print "04 §" c " m" m[c] }
    /^## [0-9]+\. /{ s=$2; sub(/\.$/,"",s); c=s; ob=""; it=""; next }
    s=="" || s+0<2 || s+0>9 { next }
    /^### / { c=$2; it=""; ob=""; if (match($0,/\(OB-[0-9]+\)/)) ob=substr($0,RSTART+1,RLENGTH-2)
              if (s=="5" || s=="9") { if (match($0,/ E-[0-9]+ /)) { e=substr($0,RSTART+1,RLENGTH-2); unit("04 " e) } }
              if (s=="8") unit("04 §" c)
              next }
    /^#### / { c=$2; it=""; ob=""; if (match($0,/\(OB-[0-9]+\)/)) ob=substr($0,RSTART+1,RLENGTH-2); next }
    /^[[:space:]]*$/ || /^---/ || /^\|[-| :]+\|[[:space:]]*$/ { next }
    /^\*Kaynak/ || /^\*Shopfolio — / || /^> / { next }
    /^\*\*Yazım turunun/ { next }
    s=="2" { if (c ~ /^2\.[0-9]+/ && ob!="" && !(ob in o)) { o[ob]=1; unit("04 " ob) }; next }
    s=="8" { next }
    s=="4" && c !~ /^4\.[24]$/ { next }
    /^[[:space:]]*(- )?\*\*[0-9]+\.[0-9]+(\.[0-9]+)+/ {
      x=$0; sub(/^[[:space:]]*(- )?\*\*/,"",x); split(x,a," "); id=a[1]; sub(/[.:*]+$/,"",id); it=id; unit("04 " id); next }
    /^\|/ {
      n=split($0,f,"|"); for(i=1;i<=n;i++){gsub(/^[[:space:]]+|[[:space:]]+$/,"",f[i])}
      if (f[2] ~ /^[0-9]+\.[0-9]+(\.[0-9]+)+$/) { unit("04 " f[2]); next }
      next }
    /^[[:space:]]*- \*\*(Aksiyonlar|Bilgi hiyerarşisi|Boş \/ yükleniyor \/ hata durumları|Durum × rol varyantları|Validasyonlar)\*\*/ {
      t=$0; sub(/^[[:space:]]*- \*\*[^*]+\*\*/,"",t); gsub(/^[[:space:]:—-]+|[[:space:]]+$/,"",t)
      it=""; if (t=="" || t ~ /^\([^)]*\)$/) next
      mk(c); next }
    /^    +[^ -]/ { next }
    /^[[:space:]]*(- |[0-9]+\. )/ { mk(c); next }
    { if (!(c in tx)) { tx[c]=1; print "04 §" c " metin" } }
  ' Docs/04_UI_SPECS.md
}
# aile 2: Kullanıcı Akışları (03) §1–§10 — K-1002'nin birimi bütün bölümlere uygulanır (K-1151); §7 toplu: alt bölüm ve bölüm metni
# sınır "## 11." başlığıdır (03 §11 geri besleme tablosu); 03 §0 matrise girmez
exp03() {
  awk '
    /^## 11\. /{exit}
    /^## 1\. /{on=1}
    !on{pt=0; next}
    /^## [0-9]+\. /{s=$2; sub(/\.$/,"",s); x=s; pt=0; d=(s!="7"); next}
    /^### [0-9]/{s=$2; x=s; pt=0; if (!d) print "03 §" s; next}
    /^#### [0-9]/{if (d){s=$2; x=s}; pt=0; next}
    !d && s ~ /\./{pt=0; next}
    /^\|/{
      if (!pt){pt=1; n=split($0,c,"|"); h=c[2]; gsub(/^[[:space:]]+|[[:space:]]+$/,"",h); next}
      if ($0 ~ /^\|[-| ]+\|[[:space:]]*$/) next
      if (!d){next}
      n=split($0,c,"|"); for(i=1;i<=n;i++){gsub(/^[[:space:]]+|[[:space:]]+$/,"",c[i])}
      if (c[2]=="") next
      if (c[2] ~ /^[0-9]+(\.[0-9]+){2,}$/){print "03 §" c[2]; next}
      if (c[2] ~ /^(S|Ö)[0-9]+$/){print "03 §" x " " c[2]; next}
      if (h=="Durum" && match(c[2], /\(`[A-Za-z]+`\)$/)){k=substr(c[2],RSTART+2,RLENGTH-4); print "03 §" x " " k; next}
      m[x]++; print "03 §" x " m" m[x]; next
    }
    {pt=0}
    /^[[:space:]]*$/ || /^---/ || /^> \*\*Ne yazılır/{next}
    !d{ if (!(s in t)){t[s]=1; print "03 §" s " metin"}; next }
    /^\*\*[0-9]+\.[0-9]+\.[0-9]+ /{r=$1; gsub(/\*/,"",r); x=r; print "03 §" r; next}
    /^[[:space:]]*(- |[0-9]+\. )/{m[x]++; print "03 §" x " m" m[x]; next}
    !(x in t){t[x]=1; print "03 §" x " metin"}
  ' Docs/03_USER_FLOWS.md
}
# aile 3: devirler — devir satırı K-n (K-479…K-1143, etki sütununda 07), park maddesi park An-m, dizin satırı dizin An,
# gövde cümlesi 0X §bölüm gN (bugünkü metinden; 02, 03, 10 ilk "## n."den, 04 "## 2."den, 05 "## 0."dan "## 12."ye, 06 "## 2."den),
# başka dokümanın park satırı 0X park An-m (08, 09, 11, 12, DEPLOY_RUNBOOK'un park bloklarında 07'yi ya da "API Tasarım"ı anan madde)
dev07() {
  awk -F'|' '/^\| K-[0-9]+ /{k=$2; gsub(/ /,"",k); n=substr(k,3)+0; if (n>=479 && n<=1143 && $7 ~ /(^|[ ·(,`])07([ ,(·)`;:]|$)/) print k}' \
    Docs/PRODUCT_DISCOVERY_STATUS.md | sort -u
}
park07() {
  awk '/^> \*\*Aşama [0-9]+.(den|ten) park edilen girdiler/{a=$0; sub(/^> \*\*Aşama /,"",a); sub(/[^0-9].*$/,"",a); m=0; next}
       /^## /{exit}
       a!="" && /^> - \*\*Dizin/{print "dizin A" a; next}
       a!="" && /^> - \*\*/{m++; print "park A" a "-" m}' Docs/07_API_DESIGN.md
}
gov07() {
  for f in 01_PROJECT_VISION 02_PRODUCT_REQUIREMENTS 03_USER_FLOWS 10_MVP_SCOPE; do
    awk -v d="${f%%_*}" '/^## [0-9]+\. /{on=1} !on{next} /^#+ [0-9]/{s=$2; sub(/\.$/,"",s); next} /^\*Shopfolio — /{next}
         /`07`|API Tasarım/{g[s]++; print d " §" s " g" g[s]}' "Docs/$f.md"
  done
  awk '/^## 2\. /{on=1} !on{next} /^#+ [0-9]/{s=$2; sub(/\.$/,"",s); next} /^\*Shopfolio — /{next}
       /`07`|API Tasarım/{g[s]++; print "04 §" s " g" g[s]}' Docs/04_UI_SPECS.md
  awk '/^## 0\. /{on=1} /^## 12\. /{on=0} !on{next} /^#+ [0-9]/{s=$2; sub(/\.$/,"",s); next}
       /`07`|API Tasarım/{x=s; if (match($0,/^\*\*[0-9]+\.[0-9]+\.[0-9]+ /)) {x=substr($0,3,RLENGTH-3); sub(/ $/,"",x)}; g[x]++; print "05 §" x " g" g[x]}' Docs/05_TECHNICAL_ARCHITECTURE.md
  awk '/^## 2\. /{on=1} !on{next} /^#+ [0-9]/{s=$2; sub(/\.$/,"",s); next} /^\*Shopfolio — /{next}
       /`07`|API Tasarım/{x=s; if (match($0,/^\*\*[0-9]+\.[0-9]+\.[0-9]+ /)) {x=substr($0,3,RLENGTH-3); sub(/ $/,"",x)}; g[x]++; print "06 §" x " g" g[x]}' Docs/06_DATA_MODEL.md
}
parkx07() {
  for f in 08_INTEGRATION_SPEC 09_CODING_GUIDELINES 11_IMPLEMENTATION_PLAN 12_VALIDATION_PROTOCOL DEPLOY_RUNBOOK; do
    awk -v d="${f%%_*}" '/^> \*\*Aşama [0-9]+.(den|ten) park/{a=$0; sub(/^> \*\*Aşama /,"",a); sub(/[^0-9].*$/,"",a); m=0; next} /^## /{exit}
         a!="" && /^> - \*\*/{m++; if ($0 ~ /`07`|API Tasarım/) print d " park A" a "-" m}' "Docs/$f.md"
  done
}
e1f_dev() { dev07; park07; gov07; parkx07; }
# aile 4: Teknik Mimari (05) — MK-04 (§2.1) ve uç düzeyindeki bölümlerin numaralı maddeleri ve kimliksiz maddeleri (bölüm + mk)
exp05() {
  awk '
    /^## /{p=0}
    /^### /{s=$2; p=(s ~ /^(3\.5|3\.6|3\.7|4\.5|6\.1|6\.2|6\.3|6\.4|6\.5|6\.7|7\.7|9\.4|10\.3)$/); next}
    /^\| MK-04 \|/ && !mk4 {mk4=1; print "05 §2.1 MK-04"; next}
    !p{next}
    /^(\*\*|\| )[0-9]+\.[0-9]+\.[0-9]+/{x=$0; sub(/^(\*\*|\| )/,"",x); split(x,a,/[ *|]/); print "05 §" a[1]; next}
    /^[[:space:]]*(- |[0-9]+\. )/{m[s]++; print "05 §" s " m" m[s]; next}
  ' Docs/05_TECHNICAL_ARCHITECTURE.md
}
# 03 §7'nin kimliğiyle girebilen satırları (131) — 1e'nin isteğe bağlı kümesi; bağlantı taşıyan satır 1e'de kimliğiyle girer (K-1150)
opt03() { awk '/^## 7\. /{f=1} /^## 8\. /{f=0} f' Docs/03_USER_FLOWS.md | grep -oE '^\| 7\.[0-9]+\.[0-9]+ \|' | sed -E 's/^\| /03 §/; s/ \|$//'; }
# oturumların beklenen kümeleri (K-1152'nin haritası)
e1a() { exp04 | grep -E '^04 (E-(0[1-9]|1[0-9]|2[0-8])|5\.|§5\.|3\.|§3\.|OB-|4\.4\.|§4\.4 )'; }
e1b() { exp04 | grep -E '^04 (E-(29|[3-5][0-9])|9\.|§9\.|§4\.2 )'; }
e1c() { exp04 | grep -E '^04 (6\.|§6\.|7\.|§7\.|§8\.)'; }
e1d() { exp03 | grep -E '^03 §[1-4]([. ]|$)'; }
e1e() { exp03 | grep -E '^03 §([5-9]|10)([. ]|$)'; }
e1f() { e1f_dev; exp05; }
# bu dokümanın matris satırları — $1 alt bölüm süzgeci: 1a '[3-5]' · 1b '6' · 1c '[78]' · 1d '9' · 1e '10' · 1f '1[12]'
mat07() {
  awk -v h="$1" '$0 ~ ("^#### 1\\.1\\." h " "){f=1; next} /^#### /{f=0}
       f && /^\| (`0[2-8]|K-[0-9]|park A|dizin A)/{split($0,c,"|"); x=c[2]; gsub(/`/,"",x); gsub(/^ +| +$/,"",x); gsub(/ +/," ",x); print x}' Docs/07_API_DESIGN.md
}
# §1.1.2'nin aday uçları ve "Uç" sütununun değer kümesi
aday07() { awk '/^#### 1\.1\.2 /{f=1; next} /^#### /{f=0} f && /^\| UC-[0-9]+ \|/{split($0,c,"|"); x=c[2]; gsub(/ /,"",x); print x}' Docs/07_API_DESIGN.md; }
uc07() {
  awk '/^#### 1\.1\.([3-9]|1[0-2]) /{f=1; next} /^#### /{f=0}
       f && /^\| (`0[2-8]|K-[0-9]|park A|dizin A)/{split($0,c,"|"); n=split(c[4],e," · "); for(i=1;i<=n;i++){x=e[i]; gsub(/^ +| +$/,"",x); print x}}' Docs/07_API_DESIGN.md
}
deger07() { uc07 | sort -u | grep -vE '^UC-[0-9]+$|^konvansiyon — API[0-9]+-[0-9]+$|^uç doğurmaz — (görünüm|istemci|dış sayfa|ürünün dışında|e-posta \(`08`\)|sistem işi \(`05 §5`\)|yoktur|yazım konvansiyonu|kurulum)$'; }
# "Durum" sütunu "Uç" sütunundan türer: UC varsa eşlendi; yoksa konvansiyon varsa konvansiyon; yoksa uç doğurmaz; GAP adayı ayrıca
durum07() {
  awk -F'|' '/^#### 1\.1\.([3-9]|1[0-2]) /{f=1; next} /^#### /{f=0}
       f && /^\| (`0[2-8]|K-[0-9]|park A|dizin A)/{u=$4; d=$5; gsub(/^ +| +$/,"",d)
         w=(u ~ /UC-[0-9]/)?"eşlendi":(u ~ /konvansiyon — /)?"konvansiyon":"uç doğurmaz"
         if (d ~ /^GAP adayı \(AA-[0-9]+\)$/) next
         if (d!=w) print $2 "\t" d " ≠ " w}' Docs/07_API_DESIGN.md
}
# GAP adayları: satırlardaki AA-n ile §1.1.13'ün listesi iki yönlü aynı olmalı
gap07() {
  diff <(awk '/^#### 1\.1\.([3-9]|1[0-2]) /{f=1; next} /^#### /{f=0} f && /GAP adayı \(AA-/{match($0,/AA-[0-9]+/); print substr($0,RSTART,RLENGTH)}' Docs/07_API_DESIGN.md | sort -u) \
       <(awk '/^#### 1\.1\.13 /{f=1; next} /^#{2,4} /{f=0} f && /^\| AA-[0-9]+ \|/{split($0,c,"|"); x=c[2]; gsub(/ /,"",x); print x}' Docs/07_API_DESIGN.md | sort -u)
}
# sayımlar (2026-10-18)
exp04 | wc -l; exp03 | wc -l; e1f_dev | wc -l; exp05 | wc -l             # 1615 · 827 · 90 · 102 — toplam 2634
e1a | wc -l; e1b | wc -l; e1c | wc -l; e1d | wc -l; e1e | wc -l; e1f | wc -l   # 583 · 431 · 601 · 536 · 291 · 192
exp04 | awk '{x=$2; t=(x ~ /^E-/)?"ekran":(x ~ /^OB-/)?"bileşen":($3 ~ /^m[0-9]+$/)?"kimliksiz madde":($3=="metin")?"metin":(x ~ /^§8\./)?"alt bölüm":"numaralı"
  y=x; sub(/^§/,"",y); split(y,a,"."); if (x ~ /^E-/) a[1]=(substr(x,3)+0<=28)?"5":"9"; if (x ~ /^OB-/) a[1]="2"; c["§" a[1] " " t]++}
  END{for(k in c) print k, c[k]}' | sort -V                               # §2 bileşen 20 · §3 metin 6, numaralı 129 · §4 metin 2, numaralı 12 · §5 ekran 28, kimliksiz madde 23, numaralı 364 · §6 metin 2, numaralı 356 · §7 numaralı 240 · §8 alt bölüm 3 · §9 ekran 25, kimliksiz madde 34, numaralı 371
exp03 | awk '{split($2,a,"."); sub(/§/,"",a[1]); t=($3 ~ /^m[0-9]+$/)?"madde":($3=="metin")?"metin":($3 ~ /^(S|Ö)[0-9]+$/)?"geçiş":($3!="")?"durum":(a[1]=="7")?"alt bölüm":"numaralı"; c["§" a[1] " " t]++}
  END{for(k in c) print k, c[k]}' | sort -V                               # §1 durum 20, geçiş 16, madde 17, metin 15, numaralı 115 · §2 madde 22, metin 16, numaralı 99 · §3 3, 2, 147 · §4 3, 2, 59 · §5 metin 1, numaralı 33 · §6 2, 52 · §7 alt bölüm 3, metin 1 · §8 madde 5, metin 10, numaralı 106 · §9 3, 2, 35 · §10 2, 3, 33
opt03 | wc -l; opt03 | while read -r a b; do awk -v k="| ${b#§} |" 'index($0,k)==1' Docs/03_USER_FLOWS.md; done | grep -c 'bağlantı'   # 131 · 18 ("bağlantı" geçen aday satır — 1e)
e1f_dev | awk '{t=($1 ~ /^K-/)?"devir":($1=="park")?"park":($1=="dizin")?"dizin":($2=="park")?"başka park":"gövde " $1; c[t]++} END{for(k in c) print k, c[k]}' | sort   # başka park 1 · devir 51 · dizin 4 · gövde 02 5 · gövde 04 1 · gövde 05 7 · gövde 06 7 · park 14
# 1a — 583 · 583 (§1.1.3 415 · §1.1.4 135 · §1.1.5 33)
mat07 '[3-5]' | wc -l; mat07 3 | wc -l; mat07 4 | wc -l; mat07 5 | wc -l
comm -23 <(e1a | sort) <(mat07 '[3-5]' | sort)                         # eksik — boş dönmeli
comm -13 <(e1a | sort) <(mat07 '[3-5]' | sort)                         # fazla — boş dönmeli
mat07 '[3-5]' | sort | uniq -d                                         # yinelenen — boş dönmeli
mat07 3 | grep -vE '^04 (E-|5\.|§5\.)'                                 # §1.1.3 yalnız 04 §5 — boş dönmeli
mat07 4 | grep -vE '^04 (3\.|§3\.)'                                    # §1.1.4 yalnız 04 §3 — boş dönmeli
mat07 5 | grep -vE '^04 (OB-|4\.4\.|§4\.4 )'                           # §1.1.5 yalnız 04 §2 ve §4.4 — boş dönmeli
deger07                                                                 # "Uç" değer kümesinin dışında — boş dönmeli
durum07                                                                 # "Durum" "Uç"tan türemiyor — boş dönmeli
comm -23 <(uc07 | grep -E '^UC-' | sort -u) <(aday07 | sort -u)         # listede olmayan aday — boş dönmeli
aday07 | sort | uniq -d                                                 # listede yinelenen kimlik — boş dönmeli
gap07                                                                   # satırların GAP adayı ile §1.1.13'ün listesi — boş dönmeli
aday07 | wc -l; uc07 | grep -E '^UC-' | sort -u | wc -l                 # 109 · 1a'nın satırlarının andığı aday
mat07 '[3-5]' | wc -l; awk -F'|' '/^#### 1\.1\.[3-5] /{f=1; next} /^#### /{f=0} f && /^\| `0/{d=$5; gsub(/^ +| +$/,"",d); sub(/ \(AA-[0-9]+\)/,"",d); c[d]++} END{for(k in c) print k, c[k]}' Docs/07_API_DESIGN.md | sort   # durum dağılımı
```

**Yanlış pozitif ve kapsam denetimi (1a).** `exp04()` Arayüz Tanımları'nın §2–§9'unu satır satır sınıflar: `### 5.n E-n` ve `### 9.n E-n` başlığı ekran birimidir; satır başındaki ya da madde imli kalın `n.m.k` — dört düzeyli dahil — maddedir ve sonraki satırın bağlamını değiştirmez; ilk hücresi `n.m.k` olan tablo satırı birimdir, öteki tablo satırları (başlık, ayraç, §2'nin giriş tablosu, §4.1–§4.3'ün tabloları, §5–§9'un form ve kural tabloları) birim değildir — `04 §6` ve §7'nin tablo satırlarının hepsi `n.m.k.l` kimliği taşır (betikle sayıldı). §5 ve §9'un beş alan başlığı ("Bilgi hiyerarşisi", "Aksiyonlar", "Validasyonlar", "Durum × rol varyantları", "Boş / yükleniyor / hata durumları") başlıktan sonra içerik taşıyorsa kimliksiz maddedir, yalnız parantezli bir işaret taşıyorsa değildir; dört boşlukla girintili madde imli satır kimliksiz maddedir (5.13.9'un (a)–(h)'si, §9'un alt maddeleri), madde imsiz girintili satır önceki maddenin devamıdır. §2'nin birimi bileşendir — başlığındaki `(OB-n)`; `### 2.12 Form bileşenleri` başlığı OB taşımaz ve altındaki `####` başlıkları taşır. §8'in birimi `### 8.n` başlığıdır. §4'te yalnız §4.2'nin metni ve §4.4 birim üretir. Kimlik biçimi `mat07()`'nin okuduğu biçimdir: ters tırnaklar silinir, boşluklar teke iner — `` `04` 5.13.6 `` → `04 5.13.6`, `` `04 §5.13` m2 `` → `04 §5.13 m2`. `exp03()` 06'nın işlevinin (`06 §1.1.1`) bütün bölümlere uygulanmış hâlidir — `d` değişkeni §7 dışındaki her bölümde satır düzeyini açar; §7 `###` alt bölüm başına bir birim ve bölüm metniyle girer. `dev07()` açılışın sayım komutunun işlev biçimidir (karar kaydı §16.2, betik 1'in ilk kolu); üst sınır K-1143'tür — Aşama 6'nın kararları devir değildir. `park07()` `06`'nın `park()`'ının kalıbıdır ve dizin satırlarını `dizin An` kimliğiyle ayrıca yazar (K-1150 dizin satırlarını kaynak satırı sayar). `gov07()` `06`'nın `gov()`'unun kalıbıdır: arama "`` `07` ``" ya da "API Tasarım"dır; Arayüz Tanımları'nın sınırı `## 2.`'dir, Veri Modeli'ninki `## 2.`'dir — iki dokümanın §1'i kendi matrisidir ve satırları devir değildir. `exp05()` 13 bölümün `###` başlığıyla sınırlıdır; `05 §2.1`'in MK-04 satırı ilk eşleşmedir (§12'nin özet tablosundaki ikinci MK-04 satırı birim değildir). **Betik iki yönlü sınandı** (`audit` skill'i, "Koşum biçimi" 4. madde; 2026-10-18): belgenin bir kopyasında §1.1.3'ten bir satır çıkarıldığında eksik, bir satır ikilendiğinde yinelenen, kaynakta olmayan bir kimlik (`04 5.4.99`) eklendiğinde fazla, §1.1.4'e bir `04 §5` satırı taşındığında başlığa bağlılık denetimi, "Uç" hücresine kümenin dışında bir değer yazıldığında değer kümesi denetimi, bir satırın "Durum"u "Uç"una aykırı yazıldığında "Durum" denetimi, listede olmayan bir aday (`UC-999`) yazıldığında aday listesi denetimi ve bir satırın GAP adayı kimliği §1.1.13'ten silindiğinde GAP adayı denetimi o satırı döndürdü.

**Okuma derinliği (checklist §2).** 1a kaynak satırlarını **satırın tamamıyla** okudu: Arayüz Tanımları'nın §2'si (satır 2337–2705), §3'ü (2706–2916), §4'ü (2917–3027; §4.1'in ekran satırları ve §4.2'nin metni dahil) ve §5'i (3028–3610) hiçbir satırı kesmeden — bu aralıkta en uzun satır 1.610 karakterdir; hücre başına kesilerek okunan satır yoktur, checklist §2'nin kesilmiş okuma örneklemi 1a'nın satırlarına uygulanmaz. Bir satırın uca dokunan ayrıntısı başka bir dokümanın kuralına işaret ettiğinde kural kaynağında okundu: ödeme adımında yazılanların çerezi (`05 §6.7.9`, §6.7.7 — 5.13.5, 5.13.19, 3.6.2), görsellerin ve dijital dosyanın sunuluşu (`05 §7.7.1`–§7.7.4 — 5.16.13, 4.4.6), `08`'in park satırı (K-906 · K-973 — 3.5.8). Öteki ailelerin gövdesi bu oturumda okunmadı; envanter onları betikle saydı — tam okuma kendi oturumlarındadır (§1 alt bölüm haritası).

#### 1.1.2 Aday uç listesi ve "Uç" sütununun değer kümesi

Liste nihai envanter değildir (K-1153): uç envanteri `07 §3`'te, GAP kararlarından sonra yazım turunda doğar (şablonun başlık notu; API3-01) ve bir adayı bölebilir, birleştirebilir ya da bir işleyiciye çevirebilir; kimlik envantere geçer ve adres değişse de kalır (K-1148). Birim adres + yöntem + (varsa) sayfa işleyicisidir (K-1149): ekranın GET'i bir aday, ekranın her POST işlemi ayrı bir aday, makine ucu kendi adayıdır. Adresin biçimi (API2-01), işlemlerin aynı sayfanın adlandırılmış işleyicileri mi ayrı adresler mi olduğu (API2-02) ve oturum ve erişim (API2-03) workshop'un kararıdır; listenin "Not" sütunu yalnız adayın dayandığı kaynağı gösterir. 1a vitrinin bütün adaylarını, bilinen makine uçlarını (K-868; kaynak satırları 1f'de) ve panel ekranlarının GET adaylarını (§3'ün geçişleri onları hedef aldığı için) açtı; panelin POST işlemlerini 1b listenin sonuna, UC-110'dan ekler. Numara iki basamakla başlar, 100'den sonra üç basamaklıdır.

| # | Aday uç | Yöntem | Ekran / yüzey | Aktör | Not |
|---|---|---|---|---|---|
| UC-01 | Ana sayfa | GET | E-01 | Ziyaretçi · Müşteri | Düzen firma ayarı (K-753) |
| UC-02 | Kategori sayfası | GET | E-02 | Ziyaretçi · Müşteri | Süzgeç ve sayfa parametreleri (API2-06) |
| UC-03 | Arama sonuçları | GET | E-03 | Ziyaretçi · Müşteri | Sözcük, süzgeç, sayfa |
| UC-04 | Ürün adresi | GET | E-04; arşivde E-05; taslak ya da silinmişte E-06 — yöneticiye taslak bantla | Ziyaretçi · Müşteri · Yönetici (önizleme) | Aynı adres üç görünümü döner (3.5.11); seçili varyant (API8-02) |
| UC-05 | Sepete ekleme | POST | E-04 | Ziyaretçi · Müşteri | Varyant, adet; satış kapısı |
| UC-06 | Bulunamayan adres — "Sayfa bulunamadı" yanıtı | — (her GET'in yanıtı) | E-06 | Ziyaretçi · Müşteri | Varlığı açığa çıkarmaz (5.6.4); durum kodu API2-07 |
| UC-07 | Hizmetlerimiz listesi | GET | E-07 | Ziyaretçi · Müşteri | Sabit sayfa adresi (K-1070) |
| UC-08 | Referanslarımız listesi | GET | E-07 | Ziyaretçi · Müşteri | Sabit sayfa adresi (K-1070) |
| UC-09 | Hakkımızda | GET | E-08 | Ziyaretçi · Müşteri · Yönetici (önizleme) | Sabit sayfa adresi |
| UC-10 | Hizmet tanıtımı sayfası | GET | E-08 | Ziyaretçi · Müşteri · Yönetici (önizleme) | Tür öneki (K-1070) |
| UC-11 | Referans iş sayfası | GET | E-08 | Ziyaretçi · Müşteri · Yönetici (önizleme) | Tür öneki (K-1070) |
| UC-12 | Genel sayfa | GET | E-08 | Ziyaretçi · Müşteri · Yönetici (önizleme) | Tür öneki (K-1070) |
| UC-13 | Sık sorulan sorular | GET | E-09 | Ziyaretçi · Müşteri | — |
| UC-14 | İletişim sayfası | GET | E-10 | Ziyaretçi · Müşteri | Formun başına inen giriş (3.3.9) |
| UC-15 | İletişim formunun gönderimi | POST | E-10 | Ziyaretçi · Müşteri | L-4, tuzak alan, veri toplayan girişlerin kapısı |
| UC-16 | Aydınlatma metni | GET | E-11 | Ziyaretçi · Müşteri | Hiç yayına alınmamışsa E-06 (5.11.9) |
| UC-17 | Çerez politikası | GET | E-11 | Ziyaretçi · Müşteri | Hiç yayına alınmamışsa E-06 |
| UC-18 | İşlem rehberi | GET | E-11 | Ziyaretçi · Müşteri | Ürünün sabit metni |
| UC-19 | Sepet | GET | E-12 | Ziyaretçi · Müşteri | Misafir sepetinin çerezi (`05 §6.7`) |
| UC-20 | Sepet kaleminin adedini değiştirme | POST | E-12 | Ziyaretçi · Müşteri | Kalemin iç anahtarı; sahiplik (K-1011) |
| UC-21 | Sepet kalemini çıkarma | POST | E-12 | Ziyaretçi · Müşteri | Kalemin iç anahtarı; sahiplik (K-1011) |
| UC-22 | Ödeme adımı | GET | E-13 | Müşteri | Yazılanların çerezi okunur (`05 §6.7.9`) |
| UC-23 | Kuponu uygulama ya da değiştirme | POST | E-13 | Müşteri | L-6 |
| UC-24 | Kuponu kaldırma | POST | E-13 | Müşteri | — |
| UC-25 | Ödeme adımının ara gönderimi — özetin yeniden hesabı, kayıtlı e-postanın hatırlatması | POST | E-13 | Müşteri | 5.13.5, 5.13.30; işleyici biçimi API2-02, betiksiz yolu API8-02 |
| UC-26 | Ödeme adımından girişe — formu çereze yazıp yönlendirme | POST | E-13 | Müşteri | `05 §6.7.9`; alanlar doğrulanmaz |
| UC-27 | Siparişi onaylama | POST | E-13 | Müşteri | Tek kullanımlık onay anahtarı (K-893); L-7, L-8 |
| UC-28 | Sipariş teyit ve bekleme ekranı | GET | E-14 | Müşteri | Yalnız onaydan sonra; yenileme sipariş doğurmaz |
| UC-29 | Ödeme sağlayıcısına geçiş | POST | E-14 (kart) | Müşteri | Bağlanılamazsa sipariş iptal, sepete (3.3.38) |
| UC-30 | Ödeme sağlayıcısından dönüş | — (yolu ve yöntemi `08`'in park satırı) | E-16'ya iner | Müşteri | `08 park A4-3` (K-906 · K-973); API4-02; durum yazmaz |
| UC-31 | Sipariş takibi girişi | GET | E-15 | Misafir alıcı | — |
| UC-32 | Sipariş takibi sorgusu | POST | E-15 | Misafir alıcı | Numara + e-posta; L-5; nötr mesaj |
| UC-33 | Sipariş sayfası | GET | E-16 | Müşteri | Üye oturumu ya da sorgunun izni; sahiplik (K-1011; API2-03) |
| UC-34 | Erişim anahtarlı sipariş adresi | GET | E-16 | Müşteri | E-postadaki bağlantı (K-562); geçersiz anahtar E-06 |
| UC-35 | Geri ödeme IBAN'ının girişi ve düzeltmesi | POST | E-16 | Müşteri | Kalem ya da sipariş düzeyi (K-1075) |
| UC-36 | İndirme bağlantısının üretimi | POST | E-16 | Müşteri | Kaleme bağlı, bir saat ömürlü imzalı değer (`05 §7.7.4`) |
| UC-37 | İndirme ucu | GET | dosya | Müşteri | Makine ucu (K-868, K-884); aralık istekleri hak düşürmez |
| UC-38 | İptal ve gecikme feshi ekranı | GET | E-17 | Müşteri | İşlem türü isteğin parçası |
| UC-39 | Siparişin iptali | POST | E-17 | Müşteri | Alındı + Bekliyor |
| UC-40 | Kalem iptali | POST | E-17 | Müşteri | Kalem, adet, havalede IBAN (K-787, K-795) |
| UC-41 | Gecikme feshi | POST | E-17 | Müşteri | Adet seçimi yok (K-796); havalede IBAN |
| UC-42 | Cayma beyanı ekranı | GET | E-18 | Müşteri | — |
| UC-43 | Cayma beyanı | POST | E-18 | Müşteri | Kalem, adet, havalede IBAN |
| UC-44 | Ayıp talebi ekranı | GET | E-19 | Müşteri | Yeni talep ya da yeniden açma hâli |
| UC-45 | Ayıp talebinin gönderimi | POST | E-19 | Müşteri | Kalem, ayıplı adet, açıklama (K-830) |
| UC-46 | Ayıp talebinin yeniden açılması | POST | E-19 | Müşteri | Z-18 içinde |
| UC-47 | Kayıt ekranı | GET | E-20 | Ziyaretçi | — |
| UC-48 | Kayıt | POST | E-20 | Ziyaretçi | L-3; veri toplayan girişlerin kapısı |
| UC-49 | Kayıt doğrulama bağlantısını yeniden isteme | POST | E-20, E-22 | Ziyaretçi · Üye | L-9 |
| UC-50 | Google ile giriş — yönlendirme | GET ya da POST (API2-03) | E-20, E-22 | Ziyaretçi · Üye | Dış kimlik sağlayıcısı (`08`) |
| UC-51 | Google dönüşü | GET | §3.6'nın dönüş yeri; E-20, E-22 | Ziyaretçi · Üye | Hesabın eşleşmesi, bekleyen kaydın silinmesi (`03 §3.4.16`) |
| UC-52 | Doğrulama bağlantısının inişi — kayıt ve yeni e-posta | GET | E-21 | Kullanıcı | Yalnız ekranı açar (K-904); hâlin tespiti `05` |
| UC-53 | Doğrulama | POST | E-21 | Kullanıcı | Bağlantının anahtarını taşır (K-904; `05 §6.4.6`) |
| UC-54 | Müşteri girişi ekranı | GET | E-22 | Ziyaretçi · Üye | Dönüş adresi (§3.6; API2-03) |
| UC-55 | Müşteri girişi | POST | E-22 | Ziyaretçi · Üye | L-1; "beni hatırla"; sepet birleşmesi |
| UC-56 | Şifre sıfırlama — isteme adımı | GET | E-23 | Kullanıcı | E-posta ön dolu gelebilir (3.3.36) |
| UC-57 | Şifre sıfırlama isteği | POST | E-23 | Kullanıcı | L-2; nötr metin |
| UC-58 | Sıfırlama bağlantısının inişi — kurma adımı | GET | E-23 | Kullanıcı | Bağlantı tek kullanımlık, Z-2 |
| UC-59 | Yeni şifrenin kurulması | POST | E-23 | Kullanıcı | Oturumlar kapanır |
| UC-60 | Yeniden doğrulama ekranı | GET | E-24 | Üye | Başlatılan işlem |
| UC-61 | Şifreyle yeniden doğrulama | POST | E-24 | Üye | L-1; tek kullanımlık değer üretir (K-971) |
| UC-62 | Google ile yeniden doğrulama — yönlendirme ve dönüş | GET | E-24 | Üye | Giriş anı denetimi (K-968; `05 §6.1.5`) |
| UC-63 | Hesap — profil ve güvenlik | GET | E-25 | Üye | Bekleyen değişiklik, geri alma engeli |
| UC-64 | Adı değiştirme | POST | E-25 | Üye | Yeniden doğrulama istemez |
| UC-65 | Şifreyi değiştirme | POST | E-25 | Üye | Tek kullanımlık değer (K-971) |
| UC-66 | E-postayı değiştirme | POST | E-25 | Üye | Tek kullanımlık değer; tekillik (K-1134) |
| UC-67 | Bekleyen e-posta değişikliğinin bağlantısını yeniden isteme | POST | E-25 | Üye | L-9 |
| UC-68 | Hesabı silme | POST | E-25 | Üye | Tek kullanımlık değer; geri alma bağlantısı açıkken kapalı |
| UC-69 | Hesap silmenin sonuç hâli | GET | E-25 | Kullanıcı | Bir kez görünür; oturum kapanmıştır (5.25.13) |
| UC-70 | Adres defteri | GET | E-26 | Üye | — |
| UC-71 | Adres ekleme | POST | E-26 | Üye | — |
| UC-72 | Adres düzenleme | POST | E-26 | Üye | Kaydın iç anahtarı; sahiplik (K-1011) |
| UC-73 | Adres silme | POST | E-26 | Üye | Kaydın iç anahtarı; sahiplik (K-1011) |
| UC-74 | Sipariş geçmişi | GET | E-27 | Üye | Sayfa başına 25 (K-784) |
| UC-75 | Geri alma bağlantısının inişi | GET | E-28 | Kullanıcı | Yalnız ekranı açar (K-839) |
| UC-76 | E-posta değişikliğini geri alma | POST | E-28 | Kullanıcı | Bağlantının anahtarını taşır |
| UC-77 | Müşteri çıkışı | POST | çerçeve (OB-02) | Üye | §3.6.4 |
| UC-78 | Beklenmeyen hata sayfası | — (sayfa isteğinin yanıtı) | §2.7.3.2 | Bütün aktörler | 500 (K-914; API2-07) |
| UC-79 | Site haritası ve robots.txt | GET | §4.4.5 | Arama motorları | Aynı makine ucu (K-868, K-933); API4-09 |
| UC-80 | Ödeme sağlayıcısının bildirim ucu | POST | makine | Ödeme sağlayıcısı | İmzalı (K-970, K-974, K-941); kaynak satırları 1f |
| UC-81 | Canlılık ucu | GET | makine | İzleme | İmzasız açık (K-918); kaynak satırları 1f |
| UC-82 | Durum ucu | GET | makine | İzleme | İmzasız açık (K-918); kaynak satırları 1f |
| UC-83 | Parçalı dosya yükleme ucu | POST | E-33 (dijital dosya), OB-19 | Yönetici | Makine ucu (K-868, K-883, K-902); kaynak satırları 1b ve 1f |
| UC-84 | Panel girişi ve şifre sıfırlama | GET | E-29 | Yönetici · Kullanıcı | POST işlemleri 1b'de |
| UC-85 | Yönetici daveti kabul ekranı | GET | E-30 | Kullanıcı | POST işlemi 1b'de |
| UC-86 | Panel ana sayfası | GET | E-31 | Yönetici | — |
| UC-87 | Ürün listesi | GET | E-32 | Yönetici | Arama ve süzgeç (K-737) |
| UC-88 | Ürün formu | GET | E-33 | Yönetici | POST işlemleri 1b'de |
| UC-89 | Kategori ağacı | GET | E-34 | Yönetici | POST işlemleri 1b'de |
| UC-90 | Kuponlar | GET | E-35 | Yönetici | POST işlemleri 1b'de |
| UC-91 | Sipariş listesi | GET | E-36 | Yönetici | Arama ve süzgeçler (K-802) |
| UC-92 | Sipariş ayrıntısı | GET | E-37 | Yönetici | Siparişin işlemleri 1b'de |
| UC-93 | Talepler | GET | E-38 | Yönetici | Tür ve durum süzgeci (K-751) |
| UC-94 | İletişim talebi ayrıntısı | GET | E-39 | Yönetici | POST işlemleri 1b'de |
| UC-95 | Üye kaydı görünümü | GET | E-40 | Yönetici | E-postayla arama |
| UC-96 | Kurumsal içerik | GET | E-41 | Yönetici | Liste ve kaydın formu; POST işlemleri 1b'de |
| UC-97 | Ana sayfa ve menü | GET | E-42 | Yönetici | POST işlemleri 1b'de |
| UC-98 | Marka | GET | E-43 | Yönetici | POST işlemleri 1b'de |
| UC-99 | Firma kimliği ve satış | GET | E-44 | Yönetici | POST işlemleri 1b'de |
| UC-100 | Ödeme yöntemleri | GET | E-45 | Yönetici | POST işlemleri 1b'de |
| UC-101 | Kargo, süreler ve varsayılanlar | GET | E-46 | Yönetici | POST işlemleri 1b'de |
| UC-102 | Yasal metinler | GET | E-47 | Yönetici | POST işlemleri 1b'de |
| UC-103 | Yönetici hesapları | GET | E-49 | Yönetici | POST işlemleri 1b'de |
| UC-104 | Yöneticinin kendi hesabı | GET | E-50 | Yönetici | POST işlemleri 1b'de |
| UC-105 | Satış özeti | GET | E-51 | Yönetici | Dönem süzgeci |
| UC-106 | İşlem izi | GET | E-52 | Yönetici | Tarih aralığı ve yönetici süzgeci |
| UC-107 | Dışa aktarma | GET | E-53 | Yönetici | Dosyanın ucu 1b'de (API9-05) |
| UC-108 | Yönetici hesabının doğrulama ekranları | GET | E-54 | Yönetici · Kullanıcı | Üç adım; 1b gerekirse böler |
| UC-109 | Yönetici çıkışı | POST | panel çerçevesi (OB-02) | Yönetici | §3.6.4 |

**"Uç" sütununun değer kümesi (K-1153).** (1) **Aday uç** — `UC-nn`, yukarıdaki listeden. (2) **Konvansiyon** — `konvansiyon — APIn-mm`: kaynak satırının kuralı tek bir uca değil, bütün uçlara ya da bir uç sınıfına uygulanır ve evi `07 §2` (ya da §5, §6, §8) olur; değer kuralın karara bağlanacağı konunun kimliğini taşır (karar kaydı §16.3) — ör. hata gösterimi API2-07, listelerin parametreleri API2-06, yapılabilir işlemler API8-01, çerçevenin her sayfa yanıtındaki verisi API5-01. Bir satır hem aday uç hem konvansiyon taşıyabilir. (3) **Sabit "uç doğurmaz" değerleri**, adıyla: *uç doğurmaz — görünüm* (yerleşim, sıra, responsive; yanıtın içeriğini değiştirmeyen sunum) · *uç doğurmaz — istemci* (tarayıcıda ya da cihazda kalan davranış: paylaşım menüsü, kopyalama) · *uç doğurmaz — dış sayfa* (ürünün dışındaki sayfa: sağlayıcı, Google, sosyal medya, ETBİS, kargo takibi) · *uç doğurmaz — ürünün dışında* (sistem dışı adım) · *uç doğurmaz — e-posta (`08`)* · *uç doğurmaz — sistem işi (`05 §5`)* · *uç doğurmaz — yoktur* (kaynağın yokluğunu yazdığı işlev — uç açılmaz) · *uç doğurmaz — yazım konvansiyonu* · *uç doğurmaz — kurulum*. Serbest metin gerekçe yoktur; betik "Uç" değerlerinin kümenin dışına çıkmadığını denetler (§1.1.1, `deger07()`).

#### 1.1.3 Arayüz Tanımları — `04 §5` vitrin ekranları (415)

Ekran ekran, §5'in sırasıyla: önce ekranın kendisi (`04` E-n — ekranın veri ihtiyacı onun GET ucudur; K-1150), sonra maddeleri ve kimliksiz maddeleri. Satırın "Uç" hücresi ekranın uçlarını ve maddenin dokunduğu öteki ekranların uçlarını gösterir; ekranın görünümünü ve yerleşimini yazan madde uç doğurmaz. Matrisin işi eşlemedir: uçların istek ve yanıt ayrıntısı §4'ün işidir (yazım turu).

| Kaynak ID | Kaynak özeti | Uç | Durum |
|---|---|---|---|
| `04` E-01 | Ana sayfa — kurumsal tanıtımı ve ürün vitrinini firmanın seçtiği düzende birlikte gösterir (§4.1) | UC-01 | eşlendi |
| `04` 5.1.1 | Aktör ziyaretçi · müşteri; giriş: logo, dönüş bağlantıları, "Siteyi görüntüle", girişten ve çıkıştan dönüş; çıkış: blokların ve çerçevenin geçişleri | UC-01 | eşlendi |
| `04` 5.1.2 | İlk görülen: düzenin ilk bloğu; birincil düğme yok — her blok kendi bağlantısını taşır | UC-01 | eşlendi |
| `04 §5.1` m1 | Çerçevenin içinde dört blok; sırası ana sayfa düzenine göre (K-753) — düzen firma ayarıdır, yanıt onu okur | UC-01 | eşlendi |
| `04` 5.1.3 | Tanıtım öncelikli düzenin sırası: kurumsal blok geniş, Hizmetlerimiz, Referanslarımız, ürün vitrini tek satır | UC-01 | eşlendi |
| `04` 5.1.4 | Mağaza öncelikli düzenin sırası: ürün vitrini iki satır, kurumsal blok dar, Hizmetlerimiz, Referanslarımız | UC-01 | eşlendi |
| `04` 5.1.5 | Blok başlığı menüdeki ad ve liste sayfasının bağlantısı; en çok üç "ana sayfada göster" işaretli kayıt, elle sırayla; fazlası "Tümünü gör" | UC-01 · UC-07 · UC-08 | eşlendi |
| `04` 5.1.6 | Ürün vitrini: yayındaki en yeni ürünler (mağaza önceliklide 8, tanıtım önceliklide 4), tükenmiş "Tükendi" ile; altında yayında ürünü olan birinci seviye kategoriler alfabetik; "tüm ürünler" sayfası yok | UC-01 · UC-02 | eşlendi |
| `04` 5.1.7 | Yoktur: kayan afiş, kampanya görseli, alıntı bloğu, kategori vitrini; SSS, şube ve genel sayfa ana sayfaya çıkmaz | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 5.1.8 | Aksiyonlar: ürün kartı → E-04 · kategori → E-02 · Hakkımızda → E-08 · içerik kartı → E-08 · "Tümünü gör" → E-07; onay ve mesaj yok | UC-04 · UC-02 · UC-09 · UC-10 · UC-11 · UC-07 · UC-08 | eşlendi |
| `04` 5.1.9 | Validasyon: form yok | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 5.1.10 | Aktörle değişen parça yok; giriş yapmış yöneticiye yönetici şeridi; satış kapalıyken ürün vitrini görünür | UC-01 · konvansiyon — API2-03 | eşlendi |
| `04` 5.1.11 | Görünmez bloklar; Hakkımızda boşken kurumsal bloğun geri düşüşü (marka adı, logo); içeriksiz yeni sitede yalnız geri düşüş | UC-01 | eşlendi |
| `04` 5.1.12 | Bloklar kendi yerinde yüklenir; sayfa açılamazsa sayfa düzeyindeki hata hâli — bloklar ilk yanıttadır (MK-04), bekleme göstergesi betikli iyileştirmenin hâlidir | UC-01 · UC-78 | eşlendi |
| `04` 5.1.13 | Responsive: bloklar alt alta; kart sayısı sınıfın sütun sayısıyla satır doldurur — gizleme yerleşimin işidir | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-02 | Kategori sayfası — kategorinin ve alt dallarının ürünlerini tek listede gösterir ve süzdürür (§4.1) | UC-02 | eşlendi |
| `04` 5.2.1 | Aktör ziyaretçi · müşteri; giriş: menü, ana sayfanın kategori bağlantısı, kırıntı yolu, dönüş bağlantıları; çıkış: ürün kartı, kırıntı yolu | UC-02 | eşlendi |
| `04` 5.2.2 | İlk görülen: kategorinin adı ve ürün ızgarası | UC-02 | eşlendi |
| `04` 5.2.3 | Kırıntı yolu, ad, süzgeçler (fiyat aralığı, stok), ızgara — doğrudan asılı ve alt dallardaki ürünler tek listede, en yeni önce — ve sayfa gezinmesi | UC-02 · konvansiyon — API2-06 | eşlendi |
| `04` 5.2.4 | Yoktur: sıralama seçeneği, seçenek değerine göre süzgeç, kartta sepete ekleme | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 5.2.5 | Süzgeci uygulamak ya da kaldırmak — liste ilk sayfasına döner; stok süzgecinin varsayılanı tükenmişleri de gösterir — sorgu parametreleri | UC-02 · konvansiyon — API2-06 | eşlendi |
| `04` 5.2.6 | Ürün kartı → E-04; sayfa değiştirmek ekranın içinde — sayfa parametresi | UC-04 · UC-02 | eşlendi |
| `04` 5.2.7 | Fiyat aralığı sayı; alt değer üst değerden büyükse alan mesajı ve süzgeç uygulanmaz; alanlar 7.1.1 — GET parametresinin doğrulaması | UC-02 · konvansiyon — API2-06 | eşlendi |
| `04` 5.2.8 | Aktörle değişen parça yok; tükenmiş kart "Tükendi", indirimli kart | UC-02 | eşlendi |
| `04` 5.2.9 | Yayında ürünü olmayan kategori boş kategori sayfası döner — "sayfa bulunamadı" değil; süzgeç sonucu boşsa süzgeci kaldırma yolu | UC-02 | eşlendi |
| `04` 5.2.10 | Izgara kendi yerinde yüklenir; sayfa açılamazsa sayfa düzeyindeki hata | UC-02 · UC-78 | eşlendi |
| `04` 5.2.11 | Responsive: sütun sayısı ve süzgeçlerin yeri | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-03 | Arama sonuçları — ürün adındaki aramanın sonucu, kategori sayfasının düzeniyle (§4.1) | UC-03 | eşlendi |
| `04` 5.3.1 | Aktör ziyaretçi · müşteri; giriş üst bölümün araması; çıkış ürün kartı | UC-03 | eşlendi |
| `04` 5.3.2 | İlk görülen: aranan sözcük ve sonuç ızgarası | UC-03 | eşlendi |
| `04` 5.3.3 | Aranan sözcük düzenlenebilir, süzgeçler (iki eksen), sonuç ızgarası en yeni önce, sayfa gezinmesi | UC-03 · konvansiyon — API2-06 | eşlendi |
| `04` 5.3.4 | Arama yalnız ürün adında, büyük-küçük harfe ve Türkçe karaktere duyarsız; arşivlenmiş ürün sonuçta yok | UC-03 | eşlendi |
| `04` 5.3.5 | Aramayı yenilemek ve süzmek — ilk sayfaya döner; ürün kartı → E-04 | UC-03 · UC-04 | eşlendi |
| `04` 5.3.6 | Boş arama gönderilmez; fiyat aralığı 5.2.7 gibi | UC-03 · konvansiyon — API2-06 | eşlendi |
| `04` 5.3.7 | Aktörle değişen parça yok; kart varyantları kategori sayfası gibi | UC-03 | eşlendi |
| `04` 5.3.8 | Sonuçsuz arama boş hâl satırı ve kategori bağlantılarına dönüş; yükleniyor ve hata 5.2.10 gibi | UC-03 · UC-78 | eşlendi |
| `04` 5.3.9 | Responsive; dar sınıfta arama alanı açılır menüde | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-04 | Ürün sayfası — ürünün bilgisi, varyant seçimi ve sepete ekleme (§4.1) | UC-04 · UC-05 | eşlendi |
| `04` 5.4.1 | Aktör ziyaretçi · müşteri; giriş: ürün kartı, sepet kalemi, paylaşılmış ya da arama motorundan gelen adres, yöneticinin önizlemesi; çıkış: kırıntı yolu, sepete ekleme — sayfada kalır, sepet, "Düzenlemeye dön" | UC-04 · UC-05 | eşlendi |
| `04` 5.4.2 | İlk görülen: görsel, ad, KDV dahil fiyat ve varyant seçimi; birincil düğme sepete ekleme | UC-04 · UC-05 | eşlendi |
| `04 §5.4` m1 | Bilgi hiyerarşisi dar sınıfın sırasıyla | uç doğurmaz — görünüm | uç doğurmaz |
| `04` 5.4.3 | Kırıntı yolu ana kategoriden, galeri (seçili varyantın görseli), ad, fiyat bloğu, varyant seçimi, adet ve sepete ekleme, teslimat bilgisi, açıklama, "Paylaş" | UC-04 | eşlendi |
| `04` 5.4.4 | Fiyat bloğu: seçili varyantın fiyatı, fiyatın başladığı tarih, birim fiyat, üretim yeri ve yerli üretim logosu; indirimde referans fiyat ve tarihler; seçim fiyatı ve birim fiyatı günceller — varyantların değerleri yanıtta | UC-04 · konvansiyon — API2-09 | eşlendi |
| `04` 5.4.5 | Teslimat bilgisi ürün tipine göre: kargoya verme süresi, teslimat il kısıtı satırı (ilk beş il ve kalan sayı, tamamı satırın içinde açılır), hizmette ifa süresi; stok adedi gösterilmez | UC-04 | eşlendi |
| `04` 5.4.6 | Varyant seçimi en çok iki boyut; tükenmiş ve açılmamış kombinasyon seçilemez; taslak varyant ziyaretçiye görünmez; satın alınabilirlik ayrılmış adetler düşülerek | UC-04 | eşlendi |
| `04` 5.4.7 | Adet alanı fiziksel ürün ve hizmette, dijitalde yok; sepete ekleme birincil düğme | UC-05 | eşlendi |
| `04` 5.4.8 | Yoktur: yorum, puan, öneri; "bu ürünün geçtiği referanslar"; "stokta haber ver", istek listesi; gömülü paylaşım kodu; ayrı cayma bloğu | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 5.4.9 | Varyantı seçmek — fiyat, görsel ve satın alınabilirlik seçilene göre güncellenir; betiksiz yolda seçili varyant isteğin parametresidir (API8-02) | UC-04 · konvansiyon — API8-02 | eşlendi |
| `04` 5.4.10 | Varyantı adediyle sepete eklemek — satış açık ve satın alınabilirken; başarı kısa süreli bildirim ve sepet sayısı; ret mesajları düğmenin yanında: stok, "bir siparişte en fazla" sınırı (sınır önce), aynı dijital varyant; üyeye daha önce alınmış dijital varyant uyarısı | UC-05 · konvansiyon — API2-07 | eşlendi |
| `04` 5.4.11 | Paylaşmak — cihazın paylaşım menüsü ya da bağlantıyı kopyalama | uç doğurmaz — istemci | uç doğurmaz |
| `04` 5.4.12 | Varyant seçilmeden ekleme yok; adet en az bir, üst sınır stok ve P-9; alanlar 7.1.2 | UC-05 | eşlendi |
| `04` 5.4.13 | Bütün varyantları tükenmiş ürün: sayfa açılır, düğmenin yerinde "Tükendi" | UC-04 | eşlendi |
| `04` 5.4.14 | Satış kapalı: ürün görünür, düğmenin yerinde satışın kapalı olduğu — ekleme sunucuda da reddedilir | UC-04 · UC-05 · konvansiyon — API8-01 | eşlendi |
| `04` 5.4.15 | Taslak ürün ziyaretçiye E-06; giriş yapmış yöneticiye "Taslak" bandıyla, taslak varyantlar dahil; önizlemede sepete ekleme yok | UC-04 · UC-06 · konvansiyon — API2-03 | eşlendi |
| `04` 5.4.16 | Misafir ile üye farkı yalnız dijital varyant uyarısı; kart, kupon ve hesabın burada yüzü yok | UC-05 | eşlendi |
| `04` 5.4.17 | Görselsiz ürün alternatif metinle; açıklama boşsa bölge yok; ekleme sürerken düğme ikinci kez basılamaz; beklenmeyen hata işlem düzeyinde | UC-04 · UC-05 · konvansiyon — API2-07 | eşlendi |
| `04` 5.4.18 | Responsive: geniş sınıfta iki sütun, dar sınıfta tek sütun; indirim tarihleri ve logo gizlenmez | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-05 | Arşivlenmiş ürün sayfası — "Bu ürün artık satılmıyor", fiyatsız (§4.1); ürünün adresi arşivde bu görünümü döner | UC-04 | eşlendi |
| `04` 5.5.1 | Aktör ziyaretçi · müşteri; giriş: arşivlenmiş ürünün paylaşılmış adresi ve yöneticinin "Sitede gör"ü; çıkış dönüş bağlantıları | UC-04 | eşlendi |
| `04` 5.5.2 | İlk görülen: ad ve "Bu ürün artık satılmıyor"; birincil düğme yok | UC-04 | eşlendi |
| `04` 5.5.3 | Ad, ana görsel, durum bilgisi metinli, dönüş bağlantıları | UC-04 | eşlendi |
| `04` 5.5.4 | Yalnız üç bilgi; fiyat ve sepete ekleme yok; listelerde, aramada ve site haritasında görünmez — yalnız doğrudan adresle açılır | UC-04 · UC-79 | eşlendi |
| `04` 5.5.5 | Aksiyon: yalnız dönüş bağlantıları | UC-01 · UC-02 | eşlendi |
| `04` 5.5.6 | Validasyon: form yok | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 5.5.7 | Ziyaretçiye ve yöneticiye aynı; yeniden yayına alınan ürünün adresi E-04, kalıcı silineninki E-06 döner | UC-04 · UC-06 | eşlendi |
| `04` 5.5.8 | Görselsiz arşiv ürünü yalnız adıyla; yükleniyor ve hata OB-07 | UC-04 · UC-78 | eşlendi |
| `04` 5.5.9 | Responsive: tek sütun | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-06 | "Sayfa bulunamadı" — var olmayan ya da ziyaretçiye kapalı adreste dönüş yolları (§4.1) | UC-06 | eşlendi |
| `04` 5.6.1 | Giriş: taslak ya da silinmiş ürünün ve içeriğin adresi, var olmayan adres, yönetici oturumu kapanmışken taslak adresi; boş kategori buraya düşmez | UC-06 · UC-04 · UC-10 · UC-11 · UC-12 | eşlendi |
| `04` 5.6.2 | İlk görülen: başlık ve iki dönüş yolu | UC-06 | eşlendi |
| `04` 5.6.3 | Vitrinin tam çerçevesi; başlık, ana sayfaya dönüş, ürünlere dönüş (birinci seviye kategoriler); içeriği firma düzenlemez | UC-06 | eşlendi |
| `04` 5.6.4 | Sayfa adresin neden bulunamadığını ayırt etmez; kapalı kaydın varlığı açığa çıkmaz — yanıt aynı | UC-06 · konvansiyon — API2-07 | eşlendi |
| `04` 5.6.5 | Aksiyon: yalnız dönüş bağlantıları | UC-01 · UC-02 | eşlendi |
| `04` 5.6.6 | Validasyon: form yok | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 5.6.7 | Taslak adresi giriş yapmış yöneticiye kaydı bantla açar; panel oturumu kapanınca yine E-06 | UC-04 · UC-10 · UC-11 · UC-12 · konvansiyon — API2-03 | eşlendi |
| `04` 5.6.8 | Beklenmeyen hata sayfasından ayrıdır; ekrana özgü boş hâl yok | UC-06 · UC-78 | eşlendi |
| `04` 5.6.9 | Responsive: tek sütun | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-07 | İçerik liste sayfası — hizmet tanıtımlarını ya da referans işleri firmanın sırasıyla (§4.1) | UC-07 · UC-08 | eşlendi |
| `04` 5.7.1 | Giriş: menüdeki Hizmetlerimiz ya da Referanslarımız, ana sayfa bloğunun "Tümünü gör"ü; çıkış içerik kartı | UC-07 · UC-08 | eşlendi |
| `04` 5.7.2 | İlk görülen: tipin menüdeki adı ve kartlar | UC-07 · UC-08 | eşlendi |
| `04` 5.7.3 | Tipin adı, içerik kartları elle sırayla, sayfa başına 24 kaydı aşarsa sayfa gezinmesi (K-851) | UC-07 · UC-08 · konvansiyon — API2-06 | eşlendi |
| `04` 5.7.4 | Yoktur: fiyat, sepete ekleme, süzgeç ve arama | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 5.7.5 | Aksiyon: içerik kartı → E-08 | UC-10 · UC-11 | eşlendi |
| `04` 5.7.6 | Validasyon: form yok | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 5.7.7 | İki tipte aynı düzen; taslak kayıt ziyaretçiye listede görünmez | UC-07 · UC-08 | eşlendi |
| `04` 5.7.8 | Yayında kaydı olmayan tipin menü öğesi görünmez; adres doğrudan açılırsa boş hâl satırı; görselsiz kart | UC-07 · UC-08 | eşlendi |
| `04` 5.7.9 | Responsive: ürün ızgarasının sütun düzeni | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-08 | İçerik sayfası — Hakkımızda, hizmet tanıtımı, referans iş ya da genel sayfa (§4.1); dört türün ayrı adres önekleri (K-1070) | UC-09 · UC-10 · UC-11 · UC-12 | eşlendi |
| `04` 5.8.1 | Giriş: menü ve altbilgi, ana sayfa, liste sayfası, paylaşılmış adres, yöneticinin önizlemesi; çıkış: "Bize ulaşın" → E-10, "İlgili ürünler" → E-04, "Düzenlemeye dön" | UC-09 · UC-10 · UC-11 · UC-12 | eşlendi |
| `04` 5.8.2 | İlk görülen: ad ya da başlık ve metin; hizmet tanıtımında birincil düğme "Bize ulaşın" | UC-09 · UC-10 · UC-11 · UC-12 | eşlendi |
| `04 §5.8` m1 | Dört tür aynı iskeletle | UC-09 · UC-10 · UC-11 · UC-12 | eşlendi |
| `04` 5.8.3 | Ad, görseller galeri sırasıyla, metin (kapalı biçim seti), türe özgü bölge, "Paylaş" | UC-09 · UC-10 · UC-11 · UC-12 | eşlendi |
| `04` 5.8.4 | Türe özgü bölge: Hakkımızda uzun metin; hizmet tanıtımı fiyatsız, "Bize ulaşın" ve "İlgili ürünler"; referans iş başlıkta müşteri adı ve "İlgili ürünler"; genel sayfa sabit düzen | UC-09 · UC-10 · UC-11 · UC-12 | eşlendi |
| `04` 5.8.5 | "İlgili ürünler" bağlı ürünlerden yalnız yayındakiler; yoksa başlığıyla görünmez | UC-10 · UC-11 | eşlendi |
| `04` 5.8.6 | Video firmanın bağlantısıdır, kendi platformunda açılır; gömülü oynatıcı, dosya eki ve blog yok | uç doğurmaz — dış sayfa · uç doğurmaz — yoktur | uç doğurmaz |
| `04` 5.8.7 | Aksiyonlar: "Bize ulaşın" → E-10'un formu · "İlgili ürünler" → E-04 · "Paylaş" · video → dış sayfa | UC-14 · UC-04 · uç doğurmaz — istemci · uç doğurmaz — dış sayfa | eşlendi |
| `04` 5.8.8 | Validasyon: form yok | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 5.8.9 | Taslak kayıt ziyaretçiye E-06, yöneticiye bantla; Hakkımızda silinmez, taslaktaysa E-06; bağlı ürün taslağa ya da arşive alınırsa kartı görünmez | UC-09 · UC-10 · UC-11 · UC-12 · UC-06 · konvansiyon — API2-03 | eşlendi |
| `04` 5.8.10 | Görselsiz kayıt yalnız metinle; yükleniyor ve hata OB-07 | UC-09 · UC-10 · UC-11 · UC-12 · UC-78 | eşlendi |
| `04` 5.8.11 | Responsive: tek sütun, galeri | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-09 | Sık sorulan sorular — bütün sorular ve cevaplar tek sayfada (§4.1) | UC-13 | eşlendi |
| `04` 5.9.1 | Giriş: menüdeki SSS, yöneticinin taslak soru önizlemesi | UC-13 | eşlendi |
| `04` 5.9.2 | İlk görülen: soruların listesi | UC-13 | eşlendi |
| `04` 5.9.3 | Menüdeki ad, sorular ve cevaplar tek listede elle sırayla, açık hâlde; "Paylaş" | UC-13 | eşlendi |
| `04` 5.9.4 | Yoktur: soru kategorisi, ara başlık, sayfa içi arama | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 5.9.5 | Aksiyon: cevabın içindeki bağlantılar | uç doğurmaz — görünüm | uç doğurmaz |
| `04` 5.9.6 | Validasyon: form yok | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 5.9.7 | Taslak soru ziyaretçiye görünmez, yöneticiye "Taslak" etiketiyle | UC-13 · konvansiyon — API2-03 | eşlendi |
| `04` 5.9.8 | Yayında soru yoksa menü öğesi yok; adres doğrudan açılırsa boş hâl satırı | UC-13 | eşlendi |
| `04` 5.9.9 | Responsive: tek sütun | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-10 | İletişim sayfası ve formu — kimlik ve iletişim bilgileri, şubeler, iletişim formu ve gönderim sonrası hâli (§4.1) | UC-14 · UC-15 | eşlendi |
| `04` 5.10.1 | Giriş: menünün son öğesi, altbilginin "İletişim" başlığı, "Bize ulaşın" — formun başına iner —, yöneticinin taslak şube önizlemesi; çıkış: gönderim ekranda kalır, "Haritada aç" | UC-14 · UC-15 · uç doğurmaz — dış sayfa | eşlendi |
| `04` 5.10.2 | İlk görülen: kimlik bilgileri ve form; birincil düğme formun gönderimi | UC-14 · UC-15 | eşlendi |
| `04 §5.10` m1 | Bilgi hiyerarşisi dar sınıfın sırasıyla | uç doğurmaz — görünüm | uç doğurmaz |
| `04` 5.10.3 | Kimlik bloğu altbilgidekiyle aynı tam set; ETBİS bandı doluysa; firma bu sayfaya ayrıca metin girmez | UC-14 | eşlendi |
| `04` 5.10.4 | İletişim formu ve şubeler elle sırayla — ad, adres, telefon, saatler, görsel, "Haritada aç"; gömülü harita yok | UC-14 | eşlendi |
| `04` 5.10.5 | Alanlar: ad, e-posta, konu tipi (kapalı beş değer), mesaj zorunlu, telefon isteğe bağlı; aydınlatma bağlantısı; görünmez tuzak alan | UC-14 · UC-15 | eşlendi |
| `04` 5.10.6 | Yoktur: dosya eki, sipariş numarası alanı, ayıp talebi konu tipi, captcha, talep numarası ve durum sorgusu, canlı destek | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 5.10.7 | Formu göndermek: onay yok; geçen gönderim iletişim talebi açar (Açık), alındı e-postası yok; tuzak alanı dolu gönderim sessizce düşer ve aynı hâli gösterir; L-4 aşılınca nötr limit mesajı, yazılanlar silinmez | UC-15 · konvansiyon — API2-08 | eşlendi |
| `04` 5.10.8 | Gönderim sonrası hâli formun yerinde, sayfa değişmeden; talep numarası ve takip yolu yok; yeni form açan bağlantı — gönder–yönlendir–göster ve geçici mesaj (API2-02) | UC-15 · UC-14 · konvansiyon — API2-02 | eşlendi |
| `04` 5.10.9 | Formun kullanıldığı başka yollar konu tipiyle gelir (fiyat sorusu, özel istek, indirme hakkı, e-posta düzeltmesi, KVKK başvurusu …); ayrı alan yok | UC-15 | eşlendi |
| `04` 5.10.10 | Zorunlu dört alan ve e-posta biçimi alan mesajıyla; hatalar birlikte, özet; konu tipi serbest metin kabul etmez | UC-15 · konvansiyon — API2-07 | eşlendi |
| `04` 5.10.11 | Üye girişliyken ad ve e-posta ön dolu, düzenlenebilir | UC-14 | eşlendi |
| `04` 5.10.12 | Veri toplayan girişlerin kapısı kapalıyken form kapalı olduğunu söyler ve çalışmaz — gönderim sunucuda da reddedilir | UC-14 · UC-15 · konvansiyon — API8-01 | eşlendi |
| `04` 5.10.13 | Taslak şube ziyaretçiye görünmez, yöneticiye etiketle | UC-14 · konvansiyon — API2-03 | eşlendi |
| `04` 5.10.14 | Şubesiz firmada bölge yok; gönderim sürerken düğme ikinci kez basılamaz; beklenmeyen hatada mesaj ve yazılanlar korunur | UC-14 · UC-15 · konvansiyon — API2-07 | eşlendi |
| `04` 5.10.15 | Responsive: geniş sınıfta iki sütun; kimlik bloğu katlanmaz | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-11 | Yasal metin sayfası — aydınlatma metni, çerez politikası ya da "İşlem rehberi" (§4.1) | UC-16 · UC-17 · UC-18 | eşlendi |
| `04` 5.11.1 | Giriş: altbilginin yasal bağlantıları ve kişisel veri alanı açan ekranlardaki aydınlatma bağlantısı | UC-16 · UC-17 · UC-18 | eşlendi |
| `04` 5.11.2 | İlk görülen: başlık ve metin | UC-16 · UC-17 · UC-18 | eşlendi |
| `04 §5.11` m1 | Üç sayfa aynı iskeletle | UC-16 · UC-17 · UC-18 | eşlendi |
| `04` 5.11.3 | Başlık ve metnin tamamı tek sayfada | UC-16 · UC-17 · UC-18 | eşlendi |
| `04` 5.11.4 | Aydınlatma metni ve çerez politikası firmanın yayına aldığı güncel sürüm; donmuş sürümler sipariş sayfasında | UC-16 · UC-17 · UC-33 | eşlendi |
| `04` 5.11.5 | "İşlem rehberi" sabit "sipariş nasıl kurulur" metni; satış kapalıyken de görünür | UC-18 | eşlendi |
| `04` 5.11.6 | Yoktur: genel Ön Bilgilendirme Formu ve sözleşme sayfası, üyelik sözleşmesi, çerez onay bandı | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 5.11.7 | Aksiyon: metnin içindeki bağlantılar | uç doğurmaz — görünüm | uç doğurmaz |
| `04` 5.11.8 | Validasyon: form yok | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 5.11.9 | Hiç yayına alınmamış metnin bağlantısı yok ve adresi E-06; iki metin yayına alınana kadar satış ve veri toplayan girişler kapalı; çerez politikası yayına alınana kadar vitrin çerez yazmaz | UC-16 · UC-17 · UC-06 · konvansiyon — API2-03 | eşlendi |
| `04` 5.11.10 | Ekrana özgü boş hâl yok; yükleniyor ve hata OB-07 | UC-16 · UC-17 · UC-18 · UC-78 | eşlendi |
| `04` 5.11.11 | Responsive: tek sütun | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-12 | Sepet — kalemler güncel fiyat ve satın alınabilirlikle; ödeme adımına geçiş (§4.1) | UC-19 · UC-20 · UC-21 | eşlendi |
| `04` 5.12.1 | Giriş: çerçevenin sepeti, "Sepete dön" ve ödeme adımının geri gönderdiği hâller, sağlayıcıya bağlanılamaması; çıkış: kalem → E-04, ödeme adımı → E-13 | UC-19 · UC-04 · UC-22 | eşlendi |
| `04` 5.12.2 | İlk görülen: kalemler ve ödenecek tutar; birincil düğme ödeme adımına geçiş | UC-19 | eşlendi |
| `04 §5.12` m1 | Bilgi hiyerarşisi dar sınıfın sırasıyla | uç doğurmaz — görünüm | uç doğurmaz |
| `04` 5.12.3 | Bir kez söylenen mesajlar (birleşme, sepetten çıkan kalem), kalemler, il kısıtı satırı, tutarın dökümü, geçiş düğmesi — bir kez söylenen mesajın taşınması yanıtın işidir | UC-19 · konvansiyon — API2-02 | eşlendi |
| `04` 5.12.4 | Kalem satırı: görsel ve ad, seçenek değerleri, güncel birim fiyat, adet alanı, satır tutarı, çıkarma; dijitalde adet yok; fiyat ve satın alınabilirlik güncel üründen | UC-19 · UC-20 · UC-21 | eşlendi |
| `04` 5.12.5 | Kalem satırındaki mesajlar: fiyat değişti, "Tükendi", stok yok, sınır aşıldı — bu kalemler siparişe ve tutara girmez, adet kendiliğinden düşmez | UC-19 | eşlendi |
| `04` 5.12.6 | Döküm: kalemlerin toplamı, kargo ücreti, ücretsiz kargo mesajı, ödenecek toplam; asgari tutarın eksiği geçiş düğmesinin yerinde; sepette kupon alanı yok | UC-19 | eşlendi |
| `04` 5.12.7 | Yoktur: taksit, stok adedi, istek listesi, sepet hatırlatması, sepetin süresi, sepette varyant değiştirme | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 5.12.8 | Adedi değiştirmek — artırma sepete eklemenin denetiminden geçer; aşım satırda; onay yok; tutar yerinde güncellenir | UC-20 · konvansiyon — API8-02 | eşlendi |
| `04` 5.12.9 | Kalemi çıkarmak — onay yok; ayrı bildirim yok | UC-21 | eşlendi |
| `04` 5.12.10 | Ödeme adımına geçmek — satış açık, siparişe girebilecek kalem var, asgari tutar karşılanıyorsa; değilse düğmenin yerinde sebebi | UC-22 · UC-19 · konvansiyon — API8-01 | eşlendi |
| `04` 5.12.11 | Kalemin adı ya da görseli → E-04 | UC-04 | eşlendi |
| `04` 5.12.12 | Adet en az bir ve tam sayı; üst sınırın aşımı satırın mesajlarıyla; alanlar 7.1.4 | UC-20 | eşlendi |
| `04` 5.12.13 | Üye ve misafir aynı ekranı görür; üyenin sepeti hesabında, misafirinki tarayıcıda; çıkış yapan üyenin tarayıcısında sepet boş | UC-19 · konvansiyon — API2-03 | eşlendi |
| `04` 5.12.14 | Satış kapalıyken sepet görünür; geçiş düğmesinin yerinde satışın kapalı olduğu | UC-19 · konvansiyon — API8-01 | eşlendi |
| `04` 5.12.15 | Yarıda kalan ödemede ve sağlayıcıya erişilemezlikte sepet durur; başarılı ödemede yalnız siparişe giren kalemler çıkar; yöntem kalmadığı için dönen müşteriye bir kez söylenen mesaj | UC-19 · UC-30 · konvansiyon — API2-02 | eşlendi |
| `04` 5.12.16 | Boş sepet boş hâl satırı; döküm ve geçiş düğmesi görünmez | UC-19 | eşlendi |
| `04` 5.12.17 | Adet değişikliği ve çıkarma sürerken satırın denetimi ikinci kez kullanılamaz; beklenmeyen hatada mesaj satırda | UC-20 · UC-21 · konvansiyon — API2-07 | eşlendi |
| `04` 5.12.18 | Responsive: geniş sınıfta iki sütun, dar sınıfta tek sütun | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-13 | Ödeme adımı — tek ekranda e-posta, adresler, kupon, ödeme yöntemi, onay özeti, iki yasal metin, onay kutuları ve onay (§4.1) | UC-22 · UC-23 · UC-24 · UC-25 · UC-26 · UC-27 | eşlendi |
| `04` 5.13.1 | Aktör müşteri; giriş: sepetten geçiş, girişten dönüş, kapanan oturumdan dönüş; çıkış: "Sepete dön" ve sepete geri gönderen hâller, giriş → E-22 → E-13, onay → E-14 | UC-22 · UC-26 · UC-27 · UC-19 | eşlendi |
| `04` 5.13.2 | İlk görülen: İletişim bölümü ve sipariş özeti; tek birincil düğme "Siparişi onayla — ödeme yükümlülüğü doğar" | UC-22 · UC-27 | eşlendi |
| `04 §5.13` m1 | Ekran tek sayfadır; bölümler alt alta ve hepsi açık, ardışık ekranlara bölünmez (K-756) — tek GET, tek onay POST'u | UC-22 · UC-27 | eşlendi |
| `04` 5.13.3 | Sadeleşmiş vitrin çerçevesi: logo ve "Sepete dön"; menü, arama, duyuru yok; altbilgi eksiksiz | UC-22 | eşlendi |
| `04` 5.13.4 | Bölümlerin sırası: İletişim, teslimat adresi (fiziksel kalem varsa), fatura adresi, ödeme yöntemi, onay | UC-22 | eşlendi |
| `04` 5.13.5 | İletişim: misafirde "Giriş yapın" ve e-posta alanı; kayıtlı hesabın e-postasında hatırlatma — yerinde kalıcı, siparişi engellemez; üyede hesabın doğrulanmış e-postası — iki giriş öğesi formu çereze yazan düğmelerdir (`05 §6.7.9`); hatırlatmanın tetiklenmesi ara gönderimdir | UC-22 · UC-25 · UC-26 | eşlendi |
| `04` 5.13.6 | Teslimat adresi: üye defterden seçer ya da yeni adres yazar ve deftere kaydetmeyi seçebilir; telefon zorunlu; il kısıtı satırı; teslimat yapılmayan ildeki kayıt seçilemez; fiziksel kalem yoksa bölüm yok | UC-22 · UC-27 | eşlendi |
| `04` 5.13.7 | Fatura adresi: "Teslimat adresimle aynı" işaretli gelir; fiziksel kalemsizde form doğrudan; telefon, kurumsal fatura, posta kodu, T.C. kimlik numarası yok | UC-22 · UC-27 | eşlendi |
| `04` 5.13.8 | Ödeme yöntemi: kart (anahtarlar tanımlıysa) ve havale (açıksa); tek yöntem seçili gelir; kullanılamayan yöntem seçilemez ve sebebini yazar — sağlayıcıya erişilemiyor, L-8 dolu | UC-22 · UC-27 · konvansiyon — API2-08 | eşlendi |
| `04` 5.13.9 | Onay bölümü yasal bilgilendirmeyi tek yerde, sırayla ve hiçbirini bağlantının ya da pencerenin arkasına koymadan taşır | UC-22 | eşlendi |
| `04 §5.13` m2 | (a) Özet farkı mesajı; sepete bağlı ödenmemiş önceki siparişin numarası ve bu onayla iptal edileceği (K-837) | UC-22 · UC-27 | eşlendi |
| `04 §5.13` m3 | (b) Onay özeti: kalemler, döküm, ödenecek toplam; misafirde teslimat adresi satırı ve İletişim bölümüne giden "Düzelt" (sayfa içi) | UC-22 | eşlendi |
| `04 §5.13` m4 | (c) Ön Bilgilendirme Formu'nun tam metni, kaydırılan kutuda, özetle birebir — siparişe göre üretilir | UC-22 | eşlendi |
| `04 §5.13` m5 | (d) Mesafeli Satış Sözleşmesi'nin tam metni | UC-22 | eşlendi |
| `04 §5.13` m6 | (e) Sabit "sipariş nasıl kurulur" metni ve aydınlatma bağlantısı | UC-22 · UC-16 | eşlendi |
| `04 §5.13` m7 | (f) Cayma özeti onay kutularının üstünde; yeni alan ya da ayar doğurmaz | UC-22 | eşlendi |
| `04 §5.13` m8 | (g) Onay kutuları: iki kutu, dijital kalemde üçüncü, hizmet kaleminde dördüncü; hiçbiri işaretli gelmez — işaretler onay POST'unun alanlarıdır | UC-22 · UC-27 | eşlendi |
| `04 §5.13` m9 | (h) Ödenecek toplam ve "Siparişi onayla — ödeme yükümlülüğü doğar"; kutular ile düğme arasında içerik yok | UC-22 · UC-27 | eşlendi |
| `04` 5.13.10 | Sipariş özeti sütunu: kalemler, döküm, kargo, kupon alanı ("Kupon kodum var" kapalı satır), toplam | UC-22 · UC-23 · UC-24 | eşlendi |
| `04` 5.13.11 | Yoktur: sipariş notu, KVKK rıza kutusu, yaş beyanı, pazarlama onayı, e-posta doğrulama kodu, üyelik sözleşmesi | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 5.13.12 | E-postayı yazmak (misafir) — kayıtlı adreste hatırlatma | UC-25 | eşlendi |
| `04` 5.13.13 | Giriş yapmak → E-22 ve geri: dönen müşteri üye olarak görür; sepetler birleşir, bir kez söylenen mesaj; yazılan adres yeni adres olarak durur; kupon yeniden uygulanır; vazgeçen yazdıklarıyla döner | UC-26 · UC-54 · UC-55 · UC-22 | eşlendi |
| `04` 5.13.14 | Adresleri seçmek, yazmak ve deftere kaydetmeyi seçmek — kayıt müşterinin seçimidir, onayla birlikte yapılır | UC-27 | eşlendi |
| `04` 5.13.15 | Kuponu uygulamak, değiştirmek ya da kaldırmak: kabul edilen kod adı ve tutarıyla; ret alan mesajı — asgari tutarın altındaki geçerli kod ayrı mesajla, öteki retler sebep ayırt etmeyen tek mesajla; L-6 aşılınca alan kapanır | UC-23 · UC-24 · konvansiyon — API2-08 | eşlendi |
| `04` 5.13.16 | Ödeme yöntemini seçmek — onay POST'unun alanı; seçim özeti değiştirirse yeniden hesap | UC-27 · UC-25 | eşlendi |
| `04` 5.13.17 | Kutuları işaretlemek — onay POST'unun alanları | UC-27 | eşlendi |
| `04` 5.13.18 | Siparişi onaylamak: pencere yok; eksik bölümde sipariş oluşmaz, eksikler işaretlenir; sepet onay anında yeniden değerlendirilir; fark yoksa sipariş doğar (Alındı + Bekliyor) → E-14; aynı sepetin ödenmemiş önceki siparişi iptal edilir | UC-27 · UC-28 · konvansiyon — API2-05 | eşlendi |
| `04` 5.13.19 | "Sepete dön" → E-12; yazılanlar o tarayıcıda ve o ziyaret boyunca durur — `05 §6.7.9` çereze yazmayı yalnız girişe giden iki öğe için sayar | UC-19 · UC-22 | GAP adayı (AA-1) |
| `04` 5.13.20 | Onay anında fark çıkarsa sipariş oluşmaz: onay bölümüne dönülür, neyin değiştiğini söyleyen mesaj, güncel özet ve metinler; sürüm ya da içerik değiştiyse kutuların işareti kalkar; kalem kalmadıysa E-12 | UC-27 · UC-22 · UC-19 | eşlendi |
| `04` 5.13.21 | Onaylanamayan hâller düğmenin yanında: L-7 nötr mesaj, satışın kapanmış olması; hiçbir yöntem kullanılamıyorsa sepete dönüş | UC-27 · UC-19 · konvansiyon — API2-08 | eşlendi |
| `04` 5.13.22 | İletişim: e-posta zorunlu ve biçimi geçerli (misafir) | UC-27 | eşlendi |
| `04` 5.13.23 | Adresler: ad-soyad, il (kapalı liste), ilçe, açık adres zorunlu; teslimatta telefon; teslimat ili firmanın illerinden olmalı — alan mesajı sebebi söyler | UC-27 · UC-25 | eşlendi |
| `04` 5.13.24 | Ödeme yöntemi seçilmiş, kutular işaretli olmalı; kupon `02 §3.10` ve L-6 ile; L-7, L-8 nötr mesajla | UC-27 · UC-23 · konvansiyon — API2-08 | eşlendi |
| `04` 5.13.25 | Misafir ile üye farkları; giriş yapmadan sipariş veren hesap sahibinin siparişi hesabına anında düşer | UC-22 · UC-27 | eşlendi |
| `04` 5.13.26 | Sepetin içeriğine göre: fiziksel kalem yoksa teslimat bölümü ve kargo yok; dijitalde üçüncü, hizmette dördüncü kutu; yönteme göre ekran ve düğme metni değişmez | UC-22 | eşlendi |
| `04` 5.13.27 | Satış kapalıyken ödeme adımına geçilemez; ekrandayken kapanırsa onay sipariş doğurmaz, sepet korunur | UC-22 · UC-27 · konvansiyon — API8-01 | eşlendi |
| `04` 5.13.28 | Oturumu ödeme adımında kapanan üye girişe götürülür; dönüşü 5.13.13 | UC-26 · UC-54 · konvansiyon — API2-03 | eşlendi |
| `04` 5.13.29 | Boş hâl yok: siparişe girebilecek kalemi olmayan sepette ödeme adımı açılmaz | UC-22 · konvansiyon — API8-01 | eşlendi |
| `04` 5.13.30 | Kupon, adres ya da yöntem değişince özet kendi yerinde yeniden hesaplanır ve bekleme gösterilir; onay sürerken düğme ikinci kez basılamaz | UC-25 · UC-23 · UC-24 · UC-27 · konvansiyon — API2-05 | eşlendi |
| `04` 5.13.31 | Onay sonuçsuz kalırsa "işlem yapılmadı" denmez: üye sipariş geçmişine, misafir sipariş takibine gider; sağlayıcıya erişilemezlik "şu an kullanılamıyor" hâlidir | UC-27 · UC-74 · UC-31 · konvansiyon — API2-07 | eşlendi |
| `04` 5.13.32 | Responsive: geniş sınıfta iki sütun; dar ve orta sınıfta katlanmış özet ve sabit toplam satırı; metinler katlanmaz | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-14 | Sipariş teyit ve bekleme ekranı — siparişin alındığı, numarası, havalede IBAN, kartta ödemenin beklendiği (§4.1) | UC-28 · UC-29 | eşlendi |
| `04` 5.14.1 | Giriş yalnız onaydan hemen sonra; kart dönüşü sipariş sayfasına iner; çıkış: sağlayıcının sayfası, havalede üyeye sipariş sayfası, misafire takip girişi; bağlanılamazsa sepet; yeniden açılınca artık ödeme beklemeyen kart siparişinde E-16 ya da E-15 | UC-28 · UC-29 · UC-33 · UC-31 · UC-19 | eşlendi |
| `04` 5.14.2 | İlk görülen: siparişin alındığı ve numarası; kartta birincil düğme sağlayıcının sayfasına geçiş | UC-28 · UC-29 | eşlendi |
| `04 §5.14` m1 | İki varyant (K-776) — kart ve havale | UC-28 | eşlendi |
| `04` 5.14.3 | Kart: numara, ödemenin henüz alınmadığı, sağlayıcının sayfasına geçiş düğmesi; süreli yönlendirme yok; kart bilgisi üründe girilmez | UC-28 · UC-29 | eşlendi |
| `04` 5.14.4 | Havale: numara, siparişe donmuş IBAN (salt okunur), ödenecek toplam, son ödeme günü, siparişe ulaşma yolu | UC-28 · UC-33 · UC-31 | eşlendi |
| `04` 5.14.5 | Aynı bilgi sipariş e-postasıyla da gider (B-1, B-2); ekran e-postanın metnini taşımaz | uç doğurmaz — e-posta (`08`) | uç doğurmaz |
| `04` 5.14.6 | Kart: sağlayıcının sayfasına geçmek; yarıda kalırsa sepet durur; dönüş sipariş sayfasına iner, sonuç gelmemişse ödemenin beklendiği | UC-29 · UC-30 · UC-33 | eşlendi |
| `04` 5.14.7 | Havale: IBAN'ı kopyalamak · siparişe gitmek | uç doğurmaz — istemci · UC-33 · UC-31 | eşlendi |
| `04` 5.14.8 | Validasyon: form yok | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 5.14.9 | Sipariş Alındı + Bekliyor; rozet yok; üye ile misafir farkı siparişe ulaşma yolu | UC-28 · konvansiyon — API2-03 | eşlendi |
| `04` 5.14.10 | Yenilenen ya da geri gelinen ekran sipariş oluşturmaz; kart siparişi o arada Ödendi ya da İptal edildi olduysa geçiş sunulmaz, E-16 ya da E-15; onaydan sonra sağlayıcıya bağlanılamazsa ödeme sayfası açılmaz, sipariş iptal edilir, sepet korunur | UC-28 · UC-29 · konvansiyon — API2-05 | eşlendi |
| `04` 5.14.11 | Responsive: tek sütun; IBAN kopyalanabilir tek blok | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-15 | Sipariş takibi girişi — numara ve e-postayla sipariş sayfasını açtırır (§4.1) | UC-31 · UC-32 | eşlendi |
| `04` 5.15.1 | Aktör misafir alıcı; oturumsuz her kullanıcıya açık; çıkış: eşleşen sorgu → E-16, hatırlatmanın "Giriş yap"ı → E-22 | UC-31 · UC-32 · UC-54 | eşlendi |
| `04` 5.15.2 | İlk görülen: numara ve e-posta alanları; birincil düğme sorgu | UC-31 | eşlendi |
| `04` 5.15.3 | Başlık, alanlar, sorgu düğmesi, öteki iki yolun hatırlatması (e-postadaki bağlantı, üyenin sipariş geçmişi) | UC-31 · UC-34 · UC-74 | eşlendi |
| `04` 5.15.4 | Sorgulamak: eşleşirse E-16; numara tek başına açmaz; eşleşmezse nötr mesaj — hangi alan ve siparişin varlığı söylenmez; L-5 iki eksende, eşikte nötr limit mesajı; onay yok | UC-32 · UC-33 · konvansiyon — API2-03 · konvansiyon — API2-08 | eşlendi |
| `04` 5.15.5 | İki alan zorunlu; e-posta biçimi; alanlar 7.1.6 | UC-32 | eşlendi |
| `04` 5.15.6 | Hesabı silinmiş müşterinin ve e-postası değişmiş hesabın siparişi siparişe donmuş e-postayla açılır; e-postası yanlış yazılmış misafir siparişi açılamaz | UC-32 | eşlendi |
| `04` 5.15.7 | Boş hâl yok; sorgu sürerken düğme ikinci kez basılamaz; kesintide de bu yol açık | UC-32 · konvansiyon — API2-07 | eşlendi |
| `04` 5.15.8 | Responsive: tek sütun | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-16 | Sipariş sayfası — siparişin durumu ve bütün bilgisi; o anda açık işlemler (§4.1) | UC-33 · UC-34 · UC-35 · UC-36 | eşlendi |
| `04` 5.16.1 | Giriş üç yoldan — üyenin sipariş geçmişi, misafirin takip sorgusu, e-postadaki bağlantı (indirme ve B-14 dahil); ayrıca kart dönüşü, teyit ekranı, işlem ekranlarından dönüş; çıkış: E-17, E-18, E-19, kargo takibi, "Siparişlerim" | UC-33 · UC-34 · UC-30 · UC-38 · UC-42 · UC-44 · uç doğurmaz — dış sayfa | eşlendi |
| `04` 5.16.2 | İlk görülen: numara ve iki rozet; müşteriden beklenen; birincil düğme yalnız açılmış IBAN alanının kaydı | UC-33 · UC-35 | eşlendi |
| `04 §5.16` m1 | Bilgi hiyerarşisi dar sınıfın sırasıyla | uç doğurmaz — görünüm | uç doğurmaz |
| `04` 5.16.3 | Başlık (numara, tarih, iki rozet), müşteriden beklenen, kalemler, işlemler, döküm ve geri ödemeler, teslimat, donmuş adresler ve e-posta, belgeler | UC-33 | eşlendi |
| `04` 5.16.4 | Müşteriden beklenen: havale ödemesi (donmuş IBAN, toplam, numara, son gün), kart ödemesinin beklendiği — sonuç gelince güncellenir, gerçekleşmediği, IBAN gereken kalemler, geç gelen ödemenin geri ödemesi için sipariş düzeyinde IBAN | UC-33 · UC-35 | eşlendi |
| `04` 5.16.5 | Kalem satırı donmuş hâliyle: adet ve açık adet, istisna, kalemin kayıtları adetleriyle, ayıp talebi rozeti, IBAN alanı, dijitalde indirme düğmesi; kalem durum rozeti taşımaz | UC-33 · UC-35 · UC-36 | eşlendi |
| `04` 5.16.6 | İşlemler bölümü: o anda açık olanlar — siparişin iptali, kalem iptali, gecikme feshi, cayma, "sorun bildir"; açık olmayan işlem gösterilmez | UC-33 · konvansiyon — API8-01 | eşlendi |
| `04` 5.16.7 | Döküm donmuş değerlerle; geri ödemeler gerçekleştikçe tarih sırasıyla — tarih, tutar, yol, kalem; IBAN'ın kendisi gösterilmez | UC-33 | eşlendi |
| `04` 5.16.8 | Teslimat: kargo şirketi, takip numarası, "Takip et"; teslim tarihi; Teslim edilemedi | UC-33 · uç doğurmaz — dış sayfa | eşlendi |
| `04` 5.16.9 | Belgeler sayfanın içinde açılıp kapanır: onaylanan sürümleriyle iki yasal metin, onay günündeki aydınlatma metni, onay kutularının kaydı; belgeler onay anındaki adı ve adresi gösterir (K-1027) | UC-33 | eşlendi |
| `04` 5.16.10 | Yoktur: fatura, e-posta ve adres geçmişi, müşterinin adres düzeltmesi, "hakkımı yenile", sipariş notu, ayrı şikâyet kanalı; sayfa arama motorlarına kapalı | uç doğurmaz — yoktur · UC-33 · konvansiyon — API4-09 | eşlendi |
| `04 §5.16` m2 | İşlemin açık olduğu aralık `03 §1.11`'dedir; aralığın dışında düğme sunulmaz — sunucunun yapılabilir işlemler kararı | UC-33 · konvansiyon — API8-01 | eşlendi |
| `04` 5.16.11 | İşlemleri başlatmak sayfada onay istemez, işlemin ekranını açar: siparişin iptali, kalem iptali, gecikme feshi → E-17; cayma → E-18; "sorun bildir" ve yeniden açma → E-19 | UC-38 · UC-42 · UC-44 | eşlendi |
| `04` 5.16.12 | IBAN'ı girmek ya da düzeltmek — sayfada kalır: istekle açılan alan ve sebebi; kart iadesi gerçekleşmediyse girmek seçimdir; kaydetme onay istemez ve e-posta üretmez; işlenene kadar düzeltilir, işlenince alan kalkar | UC-35 | eşlendi |
| `04` 5.16.13 | Dijital dosyayı indirmek — varyantın güncel dosyası; hak dolunca düğmenin yerinde mesaj; havalede "ödendi" işaretiyle açılır | UC-36 · UC-37 | eşlendi |
| `04` 5.16.14 | IBAN'ı kopyalamak · "Takip et" | uç doğurmaz — istemci · uç doğurmaz — dış sayfa | uç doğurmaz |
| `04` 5.16.15 | Eşzamanlı değişiklik: işlem o arada geçersizleşirse onay uygulanmaz, sebep ve güncel hâl | UC-39 · UC-40 · UC-41 · UC-43 · konvansiyon — API2-04 | eşlendi |
| `04` 5.16.16 | IBAN biçim ve sağlama basamağıyla; sayfanın başka formu yok; alanlar 7.1.7 | UC-35 | eşlendi |
| `04` 5.16.17 | İki eksenin izinli çiftlerinde rozetler ve açık işlemler; kendiliğinden iptal edilmiş ödenmemiş sipariş geçmişte görünmez, bağlantıyla açılır; sayfa kendi başına durum türetmez | UC-33 · UC-34 · konvansiyon — API8-01 | eşlendi |
| `04` 5.16.18 | Aktör farkı yok; hesabı silinmiş müşterinin siparişi misafir yolundan ya da bağlantıyla açılır; satış kapısı ve arşiv açık siparişi etkilemez | UC-33 · UC-34 · konvansiyon — API2-03 | eşlendi |
| `04` 5.16.19 | Boş hâl yok; ödeme sonucu beklenen sipariş yükleniyor hâli değildir; e-postanın ulaşmaması sayfayı etkilemez | UC-33 | eşlendi |
| `04` 5.16.20 | Geçersiz erişim anahtarı (e-postası düzeltilmiş misafir siparişinin eski bağlantısı) ve imha edilmiş siparişin bağlantısı var olmayan adrestir — E-06, varlık açığa çıkmaz; IBAN kaydı tamamlanmazsa işlem düzeyinde hata | UC-34 · UC-06 · UC-35 · konvansiyon — API2-03 | eşlendi |
| `04` 5.16.21 | Responsive: geniş sınıfta iki sütun; rozetler tek rozete inmez | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-17 | İptal ve gecikme feshi ekranı — siparişi, kalemi ya da kalemin bir kısmını iptal ya da gecikme nedeniyle fesih; havalede IBAN; son adımda onay (§4.1) | UC-38 · UC-39 · UC-40 · UC-41 | eşlendi |
| `04` 5.17.1 | Giriş sipariş sayfasının iptal ve fesih düğmeleri; çıkış onay ve "Vazgeç" → E-16 | UC-38 · UC-33 | eşlendi |
| `04` 5.17.2 | İlk görülen: işlemin adı ve kapsadığı kalemler; birincil düğme son adımın onayı | UC-38 | eşlendi |
| `04 §5.17` m1 | Ekran üç işlemden birini aynı iskeletle taşır (K-744) — işlem türü isteğin parçasıdır | UC-38 · UC-39 · UC-40 · UC-41 | eşlendi |
| `04` 5.17.3 | İşlemin adı, kapsam, geri ödeme ve havalede IBAN alanı, son adım | UC-38 | eşlendi |
| `04` 5.17.4 | Kapsam: siparişin iptalinde bütün kalemler salt okunur; kalem iptalinde açık kalemler seçilebilir, adet alanı (üst sınır iptale açık adet, varsayılan tamamı); gecikme feshinde seçim yok, işleme açık adetler salt okunur — o işleme açık adet yanıtta | UC-38 · UC-40 · UC-41 · konvansiyon — API8-01 | eşlendi |
| `04` 5.17.5 | Geri ödeme yolu ve süresi gün olarak; seçim bütün açık fiziksel adetleri kapsıyorsa kargo ücretinin de geri ödeneceği; IBAN alanı yalnız ödenmiş havalede, zorunlu, boş gelir, aydınlatma bağlantısıyla | UC-38 · UC-40 · UC-41 | eşlendi |
| `04` 5.17.6 | Son adım sayfanın içinde: seçilen kalemler adıyla ve adetle, sonuç cümlesi, "Bu işlem geri alınamaz", işlemin adını taşıyan onay düğmesi, "Vazgeç"; firma onayı beklenmez | UC-39 · UC-40 · UC-41 · UC-33 | eşlendi |
| `04` 5.17.7 | Yoktur: sebep alanı, kanuni faiz bilgisi, ödenmemiş siparişte kalem iptali | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 5.17.8 | Kalemleri ve adetlerini seçmek, IBAN yazmak; seçim değişince son adımın listesi, sonuç cümlesi ve kargo satırı yerinde güncellenir — kargo kuralı sunucunundur; betiksiz yolu (API8-01, API8-02) | UC-38 · konvansiyon — API8-02 | eşlendi |
| `04` 5.17.9 | Onaylamak: kalemler iptal kaydını ya da gecikme feshini alır, E-16'ya dönüş, sonuç kalıcı; kayıt kopyası e-postayla (B-7, B-9) | UC-39 · UC-40 · UC-41 · UC-33 · uç doğurmaz — e-posta (`08`) | eşlendi |
| `04` 5.17.10 | "Vazgeç" → E-16; kayıt doğmaz, girilen IBAN saklanmaz | UC-33 | eşlendi |
| `04` 5.17.11 | İşlem o arada kapandıysa onay uygulanmaz; sebep ve E-16'nın güncel hâli | UC-39 · UC-40 · UC-41 · konvansiyon — API2-04 | eşlendi |
| `04` 5.17.12 | Kalem iptalinde en az bir kalem; adet sınırları; havalede IBAN zorunlu, biçim ve sağlama; alanlar 7.1.8 | UC-40 · UC-41 · UC-39 | eşlendi |
| `04` 5.17.13 | Üç işlem siparişin durumuna göre açılır; kart ve havale yalnız geri ödemede ayrılır; üye ile misafir farkı yok | UC-38 · konvansiyon — API8-01 | eşlendi |
| `04` 5.17.14 | Boş hâl yok; onay sürerken düğme ikinci kez basılamaz; sonuçsuz onayda E-16'ya götürür | UC-39 · UC-40 · UC-41 · UC-33 · konvansiyon — API2-07 | eşlendi |
| `04` 5.17.15 | Responsive: tek sütun; son adımın parçaları ayrılmaz | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-18 | Cayma beyanı ekranı — kalem ve adet, iade adresi ve yükümlülük, beyan, havalede IBAN; son adımda onay (§4.1) | UC-42 · UC-43 | eşlendi |
| `04` 5.18.1 | Giriş sipariş sayfasının cayma düğmesi; çıkış onay ve "Vazgeç" → E-16 | UC-42 · UC-33 | eşlendi |
| `04` 5.18.2 | İlk görülen: caymanın açık olduğu kalemler | UC-42 | eşlendi |
| `04` 5.18.3 | İşlemin adı, kalem seçimi, fiziksel kalemde iade bilgisi, geri ödeme ve IBAN, son adım | UC-42 | eşlendi |
| `04` 5.18.4 | Caymanın açık olduğu kalemler seçilebilir (fiziksel kalem pencere içinde, hizmet tamamlanana kadar); koşullu istisna satırı; adet alanı — üst sınır caymaya açık adet, varsayılan tamamı | UC-42 · UC-43 · konvansiyon — API8-01 | eşlendi |
| `04` 5.18.5 | İade bilgisi: güncel iade adresi beyana yazılır; gönderme süresi ve son günü; karşı ödemeli gönderim; kargodaki kalemde iade notu; yalnız hizmette bölüm yok | UC-42 · UC-43 | eşlendi |
| `04` 5.18.6 | Geri ödeme yolu ve süresinin başladığı an; kısmi ifa edilmiş hizmet; bütün adetler seçildiyse kargo ücreti; IBAN 5.17.5'in kuralıyla | UC-42 · UC-43 | eşlendi |
| `04` 5.18.7 | Son adım: kalemler ve adetler, sonuç cümlesi, "Bu işlem geri alınamaz", "Cayma beyanını gönder", "Vazgeç" | UC-43 · UC-33 | eşlendi |
| `04` 5.18.8 | Yoktur: cayma sebebi, iade etiketi ve taşıyıcı seçimi, dosya yükleme | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 5.18.9 | Kalemleri ve adetleri seçmek, IBAN yazmak; iade bilgisi, liste ve sonuç cümlesi yerinde güncellenir — betiksiz yolu (API8-02) | UC-42 · konvansiyon — API8-02 | eşlendi |
| `04` 5.18.10 | Onaylamak: kalemler cayma beyanını tarih damgasıyla alır; teslim işaretsiz kalemi beyan kapatır; kayıt kopyası B-9 | UC-43 · UC-33 · uç doğurmaz — e-posta (`08`) | eşlendi |
| `04` 5.18.11 | "Vazgeç" → E-16; pencere ya da hak o arada kapandıysa onay uygulanmaz | UC-33 · UC-43 · konvansiyon — API2-04 | eşlendi |
| `04` 5.18.12 | En az bir kalem; adet sınırları; havalede IBAN; alanlar 7.1.9 | UC-43 | eşlendi |
| `04` 5.18.13 | Kalem tipine ve siparişin durumuna göre varyantlar; üye ile misafir farkı yok | UC-42 · konvansiyon — API8-01 | eşlendi |
| `04` 5.18.14 | Boş hâl yok; onay sürerken düğme basılamaz; sonuçsuz onay E-16; pencere dışı caymanın yolu iletişim formu | UC-43 · UC-33 · UC-15 · konvansiyon — API2-07 | eşlendi |
| `04` 5.18.15 | Responsive: tek sütun | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-19 | Ayıp talebi ekranı — kalem ve ayıplı adet, sorunun açıklaması; talebi yeniden açma (§4.1) | UC-44 · UC-45 · UC-46 | eşlendi |
| `04` 5.19.1 | Giriş "sorun bildir" ve Çözüldü talebin yeniden açma düğmesi; çıkış gönderim, yeniden açma ve "Vazgeç" → E-16 | UC-44 · UC-33 | eşlendi |
| `04` 5.19.2 | İlk görülen: kalemler ve açıklama alanı; yeniden açmada talebin kendisi | UC-44 | eşlendi |
| `04 §5.19` m1 | İki hâl — yeni talep ve yeniden açma | UC-44 | eşlendi |
| `04` 5.19.3 | Yeni talep: talebin açık olduğu kalemlerden biri, ayıplı adet (varsayılan bir, üst sınır talebe açık adet), zorunlu açıklama, fiziksel kalemde güncel iade adresi talebe yazılır, aydınlatma bağlantısı | UC-44 · UC-45 · konvansiyon — API8-01 | eşlendi |
| `04` 5.19.4 | Yeniden açma: talebin kalemi, adedi, açıklaması, tarihi ve rozeti; yeni talep açılmaz; ek bilgi firmanın e-postasına; açıklama alanı yok | UC-44 · UC-46 | eşlendi |
| `04` 5.19.5 | Yoktur: dosya yükleme, seçimlik hakkın ekranda seçilmesi, onay adımı | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 5.19.6 | Talebi göndermek: onay yok; talep Açık doğar ve panele düşer; E-16'ya dönüş; kayıt kopyası B-9 | UC-45 · UC-33 · uç doğurmaz — e-posta (`08`) | eşlendi |
| `04` 5.19.7 | Talebi yeniden açmak: Çözüldü → Açık; E-16'ya dönüş; B-9 | UC-46 · UC-33 · uç doğurmaz — e-posta (`08`) | eşlendi |
| `04` 5.19.8 | "Vazgeç" → E-16; açıklama saklanmaz | UC-33 | eşlendi |
| `04` 5.19.9 | Kalem seçili, adet sınırlarında, açıklama dolu; alanlar 7.1.10 | UC-45 | eşlendi |
| `04` 5.19.10 | Fiziksel kalemde iade adresi; Açık talepte "sorun bildir" yok; Çözüldü'de yeniden açma Z-18 içinde; Z-18 dolunca ikisi yok, yol iletişim formu | UC-44 · UC-46 · UC-15 · konvansiyon — API8-01 | eşlendi |
| `04` 5.19.11 | Boş hâl yok; gönderim sürerken düğme basılamaz; beklenmeyen hatada açıklama korunur | UC-45 · konvansiyon — API2-07 | eşlendi |
| `04` 5.19.12 | Responsive: tek sütun | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-20 | Kayıt ekranı — ad, e-posta ve şifreyle hesap; doğrulama bağlantısını yeniden isteme (§4.1) | UC-47 · UC-48 · UC-49 | eşlendi |
| `04` 5.20.1 | Giriş: girişin kayıt bağlantısı, doğrulama iniş ekranı, Google'ın doğrulanmamış e-posta dönüşü; çıkış: gönderim ekranda kalır, "Google ile giriş", "Giriş yap" | UC-47 · UC-48 · UC-50 · UC-51 · UC-54 | eşlendi |
| `04` 5.20.2 | İlk görülen: ad, e-posta, şifre; birincil düğme kayıt | UC-47 | eşlendi |
| `04` 5.20.3 | Başlık, "Google ile giriş" (tanımlıysa), form — üç alan zorunlu, aydınlatma bağlantısı, kayıt düğmesi | UC-47 · UC-48 · UC-50 | eşlendi |
| `04` 5.20.4 | Gönderim sonrası hâli formun yerinde: bağlantının gönderildiği, son gün, yeniden isteme düğmesi; giriş ve sipariş açmaz | UC-48 · UC-49 · UC-47 · konvansiyon — API2-02 | eşlendi |
| `04` 5.20.5 | Yoktur: pazarlama onayı, üyelik sözleşmesi, aydınlatma kutusu, şifrede karakter dayatması | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 5.20.6 | Kaydolmak: onay yok; doğrulanmamış kayıt açılır; bağlantı kaydın adresine gider | UC-48 · uç doğurmaz — e-posta (`08`) | eşlendi |
| `04` 5.20.7 | Bağlantıyı yeniden istemek: yeni bağlantı öncekileri geçersiz kılar, kaydın ömrü uzamaz; L-9 nötr limit mesajı | UC-49 · konvansiyon — API2-08 | eşlendi |
| `04` 5.20.8 | "Google ile giriş": dönüşte hesap doğrulanmış ve şifresiz doğar ya da doğrulanmamış e-postada bu ekrana yönlendirme mesajıyla dönüş | UC-50 · UC-51 · UC-47 | eşlendi |
| `04` 5.20.9 | Ad boş olamaz, e-posta biçimi, şifre P-17 ve yaygın şifre listesi; kayıtlı e-posta (bekleyen kayıt dahil) "bu e-posta zaten kayıtlı" ve giriş bağlantısı — varlığı bilerek söyler, L-3 daraltır | UC-48 · konvansiyon — API2-08 | eşlendi |
| `04` 5.20.10 | Veri toplayan girişlerin kapısı kapalıyken form çalışmaz, Google ile yeni hesap açılmaz — sunucuda da reddedilir | UC-47 · UC-48 · UC-51 · konvansiyon — API8-01 | eşlendi |
| `04` 5.20.11 | Google tanımlı değilse düğme yok; kayıt yalnız müşteri hesabı açar | UC-47 · UC-50 | eşlendi |
| `04` 5.20.12 | Boş hâl yok; gönderim sürerken düğme basılamaz; e-posta kesintisinde bağlantı yeniden istenir | UC-48 · UC-49 · konvansiyon — API2-07 | eşlendi |
| `04` 5.20.13 | Responsive: tek sütun | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-21 | Doğrulama bağlantısının iniş ekranı — kayıt ya da yeni e-posta doğrulaması tek düğmeyle; sonuç ve geçersiz bağlantı (§4.1; K-904) | UC-52 · UC-53 | eşlendi |
| `04` 5.21.1 | Aktör kullanıcı; giriş kaydın ve yeni e-posta adresinin doğrulama bağlantısı; çıkış: giriş, yeniden kayıt, hesap | UC-52 · UC-53 · UC-54 · UC-47 · UC-63 | eşlendi |
| `04` 5.21.2 | Geçerli bağlantıda işlemi söyleyen tek cümle ve tek düğme — doğrulama düğmeyle olur, açılışla değil (K-904) | UC-52 · UC-53 | eşlendi |
| `04 §5.21` m1 | Ekran bağlantının hâline göre altı hâlden birini gösterir; geçerli bağlantıda önce doğrulama adımı; bağlantının kaç kez açıldığına bakılmaz — GET yalnız ekranı açar | UC-52 · UC-53 · konvansiyon — API2-02 | eşlendi |
| `04` 5.21.3 | Kayıt doğrulandı: hesap açıldı, o e-postanın misafir siparişleri hesaba düştü; doğrulama oturum açmaz | UC-53 | eşlendi |
| `04` 5.21.4 | Kayıt bağlantısının süresi dolmuş: "bağlantı geçersiz, yeniden kayıt olun"; kayıt silinmiş | UC-52 | eşlendi |
| `04` 5.21.5 | Yerine yenisi gönderilmiş bağlantı: en son gönderilenin kullanılacağı; sonraki adım giriş | UC-52 | eşlendi |
| `04` 5.21.6 | Daha önce kullanılmış: hesap zaten doğrulanmış | UC-52 | eşlendi |
| `04` 5.21.7 | Yeni e-posta adresi doğrulandı: hesap yeni adresle çalışır, eski adrese bildirim; yeni adresin misafir siparişleri hesaba düşer; sonraki adım oturuma göre | UC-53 · uç doğurmaz — e-posta (`08`) | eşlendi |
| `04` 5.21.8 | Yeni e-posta bağlantısı geçersiz — süresi dolmuş, yerine yeni istek geçmiş, kullanılmış ya da adres başka bir hesabın olmuş (K-1080) | UC-52 · UC-53 | eşlendi |
| `04` 5.21.9 | Aksiyon: doğrulamak — tek düğme, pencere yok; öteki hâllerde yalnız sonraki adımın bağlantısı; hâlin tespiti Teknik Mimari'nin (K-903) | UC-53 · UC-52 | eşlendi |
| `04` 5.21.10 | Validasyon: form yok — düğmenin POST'u bağlantının anahtarını taşır | UC-53 | eşlendi |
| `04` 5.21.11 | Ekran oturumdan bağımsız; yalnız sonraki adım oturuma göre; yöneticinin yeni adresi E-54'e iner | UC-52 · konvansiyon — API2-03 | eşlendi |
| `04` 5.21.12 | Tanınmayan bağlantı süresi dolmuş hâli; doğrulama sürerken düğme basılamaz; sonuç belirsizse bağlantı yeniden açılınca söylenir | UC-52 · UC-53 · konvansiyon — API2-07 | eşlendi |
| `04` 5.21.13 | Responsive: tek sütun | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-22 | Müşteri girişi — e-posta ve şifreyle ya da Google ile (§4.1) | UC-54 · UC-55 · UC-50 | eşlendi |
| `04` 5.22.1 | Giriş: "Giriş yap", ödeme adımının giriş öğeleri, oturum gerektiren ekranın adresi, doğrulama ve sıfırlama ekranlarının bağlantısı; çıkış §3.6'nın dönüşü, kayıt, "şifremi unuttum", Google | UC-54 · UC-55 · UC-47 · UC-56 · UC-50 · konvansiyon — API2-03 | eşlendi |
| `04` 5.22.2 | İlk görülen: e-posta ve şifre; birincil düğme giriş | UC-54 | eşlendi |
| `04` 5.22.3 | Başlık, "Google ile giriş" ve aydınlatma bağlantısı, form — e-posta, şifre, "beni hatırla" (uzun ya da kısa oturum, Z-4, Z-5), bağlantılar | UC-54 · UC-55 · UC-50 | eşlendi |
| `04` 5.22.4 | Yalnız müşteri hesaplarını arar; yönetici panelin kendi girişinden girer — iki kapı (K-588) | UC-55 · konvansiyon — API2-03 | eşlendi |
| `04` 5.22.5 | E-posta ve şifreyle girmek: oturum açılır, tanınan tarayıcı olur, misafir sepeti birleşir — bir kez söylenen mesaj; dönüş §3.6 | UC-55 · konvansiyon — API2-02 | eşlendi |
| `04` 5.22.6 | Google ile girmek: eşleşen hesaba girilir ve iki yol taşır; eşleşme yoksa şifresiz hesap doğar; doğrulanmamış bekleyen kayıt silinir ve hesap Google ile doğar; doğrulanmamış e-postada kayda yönlendirme; L-1'e girmez | UC-50 · UC-51 | eşlendi |
| `04` 5.22.7 | Hatalı giriş: hangi alanın yanlış olduğunu ve hesabın varlığını söylemeyen tek mesaj; L-1 nötr limit mesajı, hesap kilitlenmez; tanınan tarayıcı e-posta ekseninde engellenmez | UC-55 · konvansiyon — API2-08 | eşlendi |
| `04` 5.22.8 | Doğrulanmamış hesapla girmek: doğru bilgide doğrulamanın beklendiği ve yeniden isteme düğmesi (L-9); yanlışta 5.22.7'nin mesajı | UC-55 · UC-49 | eşlendi |
| `04` 5.22.9 | E-posta ve şifre zorunlu; biçim; alanlar 7.1.12 | UC-55 | eşlendi |
| `04` 5.22.10 | Veri toplayan girişlerin kapısı kapalıyken mevcut hesap Google ile girer; yeni hesap açılmaz, dönüşte formun başında mesaj | UC-51 · UC-54 · konvansiyon — API8-01 | eşlendi |
| `04` 5.22.11 | Google tanımlı değilse düğme yok; Google'a ulaşılamazsa şifreyle ya da "şifremi unuttum" ile | UC-54 · UC-50 · UC-56 | eşlendi |
| `04` 5.22.12 | Ödeme adımından gelen müşteri ödeme adımına döner, yazdıkları durur; kendiliğinden kapanmış oturumda mesaj | UC-55 · UC-22 · konvansiyon — API2-03 | eşlendi |
| `04` 5.22.13 | Boş hâl yok; giriş sürerken düğme basılamaz; beklenmeyen hata işlem düzeyinde | UC-55 · konvansiyon — API2-07 | eşlendi |
| `04` 5.22.14 | Responsive: tek sütun | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-23 | Şifre sıfırlama ekranları — bağlantıyı isteme ve yeni şifreyi kurma (§4.1) | UC-56 · UC-57 · UC-58 · UC-59 | eşlendi |
| `04` 5.23.1 | İki adım; giriş: "şifremi unuttum", hesabın şifre belirleme bağlantısı, e-postadaki sıfırlama bağlantısı; çıkış giriş | UC-56 · UC-58 · UC-54 | eşlendi |
| `04` 5.23.2 | İlk görülen: isteme adımında e-posta, kurma adımında yeni şifre | UC-56 · UC-58 | eşlendi |
| `04` 5.23.3 | İsteme adımı: e-posta (hesap ekranından gelindiyse ön dolu), düğme; gönderimden sonra nötr metin "bu adres kayıtlıysa…" | UC-56 · UC-57 | eşlendi |
| `04` 5.23.4 | Kurma adımı: şifresiz hesapta "şifre belirleme" adı; kurulunca bütün oturumların kapandığı ve giriş bağlantısı | UC-58 · UC-59 | eşlendi |
| `04` 5.23.5 | Bağlantıyı istemek: onay yok; adresin kayıtlı olup olmadığı söylenmez; L-2 nötr limit mesajı | UC-57 · konvansiyon — API2-08 | eşlendi |
| `04` 5.23.6 | Yeni şifreyi kurmak: bağlantı tek kullanımlık; oturumlar kapanır, tarayıcı tanınır, işaretler düşer; Google hesabı iki yol taşır; kurma oturum açmaz | UC-59 · konvansiyon — API2-05 | eşlendi |
| `04` 5.23.7 | E-posta biçimi; şifre politikası P-17 ve yaygın şifreler; alanlar 7.1.13 | UC-57 · UC-59 | eşlendi |
| `04` 5.23.8 | Kullanılmış ya da süresi dolmuş bağlantıda kurma adımı açılmaz, isteme adımına götürür; yöneticinin sıfırlaması E-29'da; geri almadan sonraki sıfırlama eski adrese giden bağlantıyla | UC-58 · UC-56 | eşlendi |
| `04` 5.23.9 | Boş hâl yok; gönderim sürerken düğme basılamaz; e-posta kesintisi kalan risk | UC-57 · konvansiyon — API2-07 | eşlendi |
| `04` 5.23.10 | Responsive: tek sütun | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-24 | Yeniden doğrulama ekranı — şifre, e-posta değişikliği ve hesap silmeden önce kimliği yeniden doğrular (§4.1) | UC-60 · UC-61 · UC-62 | eşlendi |
| `04` 5.24.1 | Aktör üye; giriş hesap ekranının üç işlemi; çıkış: doğrulamadan sonra işlemin formuna, Google, "Vazgeç" | UC-60 · UC-62 · UC-63 | eşlendi |
| `04` 5.24.2 | İlk görülen: işlemin adı ve şifre alanı ya da Google düğmesi | UC-60 | eşlendi |
| `04` 5.24.3 | İşlemin adı, şifre alanı (şifre varsa), Google ile doğrulama (Google girişi varsa), düğme ve "Vazgeç" | UC-60 · UC-61 · UC-62 | eşlendi |
| `04` 5.24.4 | Yeniden doğrulama yalnız başlatılan işlem içindir; bir işlemden ötekine taşınmaz — tek kullanımlık değere bağlı (K-971) | UC-61 · UC-62 · konvansiyon — API2-05 | eşlendi |
| `04` 5.24.5 | Doğrulamak — şifreyle ya da Google ile; başarıda işlemin formuna dönüş | UC-61 · UC-62 · UC-63 | eşlendi |
| `04` 5.24.6 | Şifre zorunlu; yanlış şifre L-1'e sayılır; eşikte o eksende yeniden doğrulama ve giriş engellenir; Google ile doğrulama L-1'e girmez | UC-61 · konvansiyon — API2-08 | eşlendi |
| `04` 5.24.7 | Şifresiz hesapta Google'a ulaşılamıyorsa üç işlem yapılamaz; Google doğrulaması tutmazsa (giriş anı eski ya da e-posta eşleşmiyor) işlem yapılmaz (K-968); panelde Google ile yeniden doğrulama yok | UC-62 · UC-60 | eşlendi |
| `04` 5.24.8 | Boş hâl yok; doğrulama sürerken düğme basılamaz; oturum kapanırsa girişe | UC-61 · konvansiyon — API2-03 | eşlendi |
| `04` 5.24.9 | Responsive: tek sütun | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-25 | Hesap — profil ve güvenlik: ad, e-posta ve şifre değişikliği, bekleyen e-posta değişikliği, hesap silme (§4.1) | UC-63 · UC-64 · UC-65 · UC-66 · UC-67 · UC-68 · UC-69 | eşlendi |
| `04` 5.25.1 | Aktör üye; giriş "Hesabım", iç gezinme, girişten sonra, doğrulama ekranlarından dönüş; çıkış: üç işlem → E-24, şifre belirleme → E-23, iç gezinme, silmenin sonuç hâli | UC-63 · UC-60 · UC-56 · UC-70 · UC-74 · UC-69 | eşlendi |
| `04` 5.25.2 | İlk görülen: adı ve e-postası; her bölüm kendi düğmesini taşır | UC-63 | eşlendi |
| `04 §5.25` m1 | Üstte iç gezinme (Hesabım · Adres defteri · Siparişlerim), altında beş bölüm | UC-63 · UC-70 · UC-74 | eşlendi |
| `04` 5.25.3 | Ad ve düzenleme; yeniden doğrulama istemez | UC-63 · UC-64 | eşlendi |
| `04` 5.25.4 | E-posta ve değiştirme düğmesi; bekleyen değişiklik — yeni adres, son gün, yeniden isteme; yeni adres bekleyenin yerine geçer; geri alma bağlantısı açıkken değiştirme kapalı ve bitiş tarihi | UC-63 · UC-66 · UC-67 · konvansiyon — API8-01 | eşlendi |
| `04` 5.25.5 | Şifre: değiştirme düğmesi; şifresiz Google hesabında şifre belirleme → E-23, e-posta ön dolu | UC-63 · UC-56 | eşlendi |
| `04` 5.25.6 | Giriş yolları salt okunur; Google bağını kaldırma ve bağlama işlemi yok | UC-63 · uç doğurmaz — yoktur | eşlendi |
| `04` 5.25.7 | Hesabı sil: silinenler ve sürenler; yürüyen sipariş engellemez; geri alma bağlantısı açıkken düğme kapalı | UC-63 · UC-68 · konvansiyon — API8-01 | eşlendi |
| `04` 5.25.8 | Yoktur: bildirim tercihi, pazarlama onayı, dil seçimi, uygulama içi veri indirme, iki adımlı doğrulama | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 5.25.9 | Adı değiştirmek: onay yok, kısa süreli bildirim; geçmiş siparişe dokunmaz | UC-64 · konvansiyon — API2-02 | eşlendi |
| `04` 5.25.10 | Şifreyi değiştirmek: E-24'ten sonra bölümün içinde alan; öteki oturumlar kapanır, bu oturum açık kalır | UC-65 · UC-60 · konvansiyon — API2-05 | eşlendi |
| `04` 5.25.11 | E-postayı değiştirmek: E-24'ten sonra alan; yeni adrese bağlantı; doğrulanmış başka hesabın adresi kabul edilmez — bekleyen kayıt durdurmaz (K-1134); geçerli olunca oturumlar kapanmaz | UC-66 · UC-60 · uç doğurmaz — e-posta (`08`) | eşlendi |
| `04` 5.25.12 | Bekleyen değişikliğin bağlantısını yeniden istemek — L-9 | UC-67 · konvansiyon — API2-08 | eşlendi |
| `04` 5.25.13 | Hesabı silmek: E-24'ten sonra son adım; onayla silinir, sonuç hâli bir kez görünür, oturum kapanmıştır; yenilenirse ana sayfa; silinme e-postası | UC-68 · UC-69 · UC-60 · uç doğurmaz — e-posta (`08`) | eşlendi |
| `04` 5.25.14 | Ad boş olamaz; yeni e-posta biçimi ve müşteri hesapları içinde tekil; şifre politikası; L-9; alanlar 7.1.15 | UC-64 · UC-65 · UC-66 · UC-67 | eşlendi |
| `04` 5.25.15 | Giriş yollarına göre varyantlar; bekleyen değişiklik ile geri alma engeli birlikte durmaz | UC-63 | eşlendi |
| `04` 5.25.16 | Şifresiz hesapta Google'a ulaşılamıyorsa üç işlem yapılamaz | UC-63 · UC-62 | eşlendi |
| `04` 5.25.17 | Boş hâl yok; bölümün kaydı kendi yerinde bekler; silmenin onayı sonuçsuz kalırsa yeniden açılan ekran hesabı ya da girişi gösterir; oturum kapanırsa §3.6.5 | UC-63 · UC-68 · UC-54 · konvansiyon — API2-07 | eşlendi |
| `04` 5.25.18 | Responsive: iç gezinme geniş sınıfta solda, dar sınıfta sekmeler | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-26 | Adres defteri — kayıtlı adresleri ekleme, düzenleme, silme (§4.1) | UC-70 · UC-71 · UC-72 · UC-73 | eşlendi |
| `04` 5.26.1 | Aktör üye; giriş iç gezinme ve girişten sonra; çıkış iç gezinme | UC-70 · UC-63 · UC-74 | eşlendi |
| `04` 5.26.2 | İlk görülen: kayıtlı adresler; birincil düğme ekleme | UC-70 | eşlendi |
| `04` 5.26.3 | İç gezinme, ekleme düğmesi, adres kayıtları ve kayıt başına düzenle ve sil — kaydın iç anahtarı istekte, sahiplik sunucuda (K-1011) | UC-70 · UC-72 · UC-73 · konvansiyon — API2-03 | eşlendi |
| `04` 5.26.4 | Adres formu defter varyantı: adres adı ve teslimat alanları; il kapalı liste, telefon isteğe bağlı; form ekranın içinde açılır | UC-70 · UC-71 · UC-72 | eşlendi |
| `04` 5.26.5 | Defterdeki değişiklik geçmiş siparişe dokunmaz; telefonsuz kayıt ödeme adımında telefon ister; teslimat yapılmayan ildeki kayıt tutulur | UC-70 | eşlendi |
| `04` 5.26.6 | Eklemek, düzenlemek, silmek: onay yok; ekleme ve düzenlemede kısa süreli bildirim; ödeme adımında yazılan adres orada deftere kaydedilir | UC-71 · UC-72 · UC-73 · UC-27 | eşlendi |
| `04` 5.26.7 | Zorunlu alanlar, telefon isteğe bağlı; alanlar 7.1.16 | UC-71 · UC-72 | eşlendi |
| `04` 5.26.8 | Aktörle değişen parça yok | UC-70 | eşlendi |
| `04` 5.26.9 | Adresi olmayan defter boş hâl satırı ve ekleme düğmesi; kayıt sürerken düğme basılamaz; beklenmeyen hatada form korunur | UC-70 · UC-71 · konvansiyon — API2-07 | eşlendi |
| `04` 5.26.10 | Responsive: iki sütunlu kart ızgarası | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-27 | Sipariş geçmişi — hesaba bağlı siparişler ve sipariş sayfasına geçiş (§4.1) | UC-74 | eşlendi |
| `04` 5.27.1 | Aktör üye; giriş "Siparişlerim", iç gezinme, girişten sonra; çıkış sipariş satırı → E-16 | UC-74 · UC-33 | eşlendi |
| `04` 5.27.2 | İlk görülen: siparişler en yeni önce | UC-74 | eşlendi |
| `04` 5.27.3 | İç gezinme, liste, sayfa başına 25 sipariş (K-784) | UC-74 · konvansiyon — API2-06 | eşlendi |
| `04` 5.27.4 | Satır: numara, tarih, ilk kalem ve kalan sayı, toplam, iki rozet; satır sipariş sayfasını açar | UC-74 · UC-33 | eşlendi |
| `04` 5.27.5 | Görünenler: hesaba bağlı bütün siparişler, hesaba düşen misafir siparişleri dahil; görünmeyen: kendiliğinden iptal edilmiş ödenmemiş sipariş, e-postası düzeltilip başka adrese geçen misafir siparişi | UC-74 | eşlendi |
| `04` 5.27.6 | Yoktur: arama, süzgeç, listeden işlem | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 5.27.7 | Sipariş satırı → E-16; sayfa değiştirmek ekranın içinde, dönen üye bıraktığı sayfayı bulur | UC-33 · UC-74 · konvansiyon — API2-06 | eşlendi |
| `04` 5.27.8 | Validasyon: form yok | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 5.27.9 | Rozetler iki eksenin bütün değerlerini alır; hesap silinirse liste de silinir | UC-74 | eşlendi |
| `04` 5.27.10 | Siparişi olmayan üyede boş hâl satırı; liste kendi yerinde yüklenir; sayfa açılamazsa hata | UC-74 · UC-78 | eşlendi |
| `04` 5.27.11 | Responsive: tablo ya da kart; yatay kaydırma yok | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-28 | E-posta değişikliğini geri alma ekranı — tek düğmeyle geri alma, sonuç; yeni şifre eski adrese giden bağlantıyla (§4.1; K-839) | UC-75 · UC-76 | eşlendi |
| `04` 5.28.1 | Aktör kullanıcı — adresin önceki sahibi; giriş "bu değişikliği ben yapmadım" bağlantısı; çıkış: yeni şifre E-23'te, çerçeve | UC-75 · UC-58 | eşlendi |
| `04` 5.28.2 | Geçerli bağlantıda tek cümle ve tek düğme — geri alma düğmeyle olur (K-839); sonrasında ve geçersiz bağlantıda birincil düğme yok | UC-75 · UC-76 | eşlendi |
| `04 §5.28` m1 | Geri alma adımı ve iki hâl (K-782, K-839) | UC-75 · UC-76 | eşlendi |
| `04` 5.28.3 | Geri alındı — Z-45 içinde ve kullanılmamış bağlantıda önce geri alma adımı; açılış geri almaz; düğmeyle: e-posta önceki adrese, oturumlar kapanır, Google bağı kalkar, şifre geçersizleşir ve eski adrese sıfırlama bağlantısı, siparişler hesapta kalır | UC-75 · UC-76 · uç doğurmaz — e-posta (`08`) | eşlendi |
| `04` 5.28.4 | Bağlantı geçersiz — süresi dolmuş ya da kullanılmış; ürünün içinden yol kalmadığı; kullanılmışsa geri almanın yapıldığı | UC-75 | eşlendi |
| `04` 5.28.5 | Yoktur: şifre alanı, değişikliği yapanın kimliği ve yeni adres, ek soru ve onay penceresi | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 5.28.6 | Aksiyon: değişikliği geri almak — tek düğme, pencere yok; bildirimin metni `08` | UC-76 | eşlendi |
| `04` 5.28.7 | Validasyon: form yok — düğmenin POST'u bağlantının anahtarını taşır | UC-76 | eşlendi |
| `04` 5.28.8 | Ekran oturumdan bağımsız; geri alma açık oturumu da kapatır; yöneticinin karşılığı E-54 | UC-75 · UC-76 · konvansiyon — API2-03 | eşlendi |
| `04` 5.28.9 | Boş hâl yok; sayfa açılamazsa hata; sonuç belirsizse bağlantı yeniden açılınca söylenir | UC-75 · UC-78 · konvansiyon — API2-07 | eşlendi |
| `04` 5.28.10 | Responsive: tek sütun | uç doğurmaz — görünüm | uç doğurmaz |

#### 1.1.4 Arayüz Tanımları — `04 §3` navigasyon haritası (135)

Bir geçişin ucu hedef ekranın ucudur — tetikleyici bir işlemse işlemin ucu da; aynı ekranın içinde kalan işlemler ekranın satırlarındadır (§1.1.3). Panelin geçişleri panel ekranlarının GET adaylarına eşlenir; işlemlerin uçları 1b'nindir (§1.1.6).

| Kaynak ID | Kaynak özeti | Uç | Durum |
|---|---|---|---|
| `04 §3.1` metin | Vitrin çerçevesinin her bölgesinin götürdüğü yer, E-01…E-28'de aynı; dar sınıfta arama ve menü açılır menüde, hedef değişmez; menünün sırası ana sayfa düzenine göre | konvansiyon — API5-01 | konvansiyon |
| `04` 3.1.1 | Duyuru şeridinin bağlantısı → firmanın girdiği adres; duyuru Yayında ve aralığında | konvansiyon — API5-01 | konvansiyon |
| `04` 3.1.2 | Logo, marka adı ya da alan adı → E-01 | UC-01 | eşlendi |
| `04` 3.1.3 | Üst bölümün araması → E-03 | UC-03 | eşlendi |
| `04` 3.1.4 | Hesap alanı (oturum yok) "Giriş yap" → E-22; dönüş §3.6 | UC-54 | eşlendi |
| `04` 3.1.5 | Hesap alanı (oturum yok) "Sipariş takibi" → E-15 | UC-31 | eşlendi |
| `04` 3.1.6 | Hesap alanı (üye) "Hesabım" → E-25 | UC-63 | eşlendi |
| `04` 3.1.7 | Hesap alanı (üye) "Siparişlerim" → E-27 | UC-74 | eşlendi |
| `04` 3.1.8 | Hesap alanı (üye) "Çıkış" → §3.6.4 | UC-77 | eşlendi |
| `04` 3.1.9 | Sepet → E-12 | UC-19 | eşlendi |
| `04` 3.1.10 | E-13'ün sadeleşmiş üst bölümü: logo → E-01, "Sepete dön" → E-12 | UC-01 · UC-19 | eşlendi |
| `04` 3.1.11 | Menü "Ürünler" → birinci, ikinci ya da üçüncü seviye kategori → E-02; yayında ürünü olan kategori | UC-02 · konvansiyon — API5-01 | eşlendi |
| `04` 3.1.12 | Menü Hizmetlerimiz · Referanslarımız → E-07; tipin yayında kaydı varsa | UC-07 · UC-08 | eşlendi |
| `04` 3.1.13 | Menü Hakkımızda → E-08; Yayında'ysa | UC-09 | eşlendi |
| `04` 3.1.14 | Menü SSS → E-09; yayında soru varsa | UC-13 | eşlendi |
| `04` 3.1.15 | Menüde gösterilen genel sayfa → E-08 | UC-12 | eşlendi |
| `04` 3.1.16 | Menü İletişim → E-10; her zaman, son öğe | UC-14 | eşlendi |
| `04` 3.1.17 | Sosyal medya ve WhatsApp → platformun sayfası | uç doğurmaz — dış sayfa | uç doğurmaz |
| `04` 3.1.18 | Altbilgi genel sayfalar → E-08 | UC-12 | eşlendi |
| `04` 3.1.19 | Altbilgi aydınlatma metni · çerez politikası · "İşlem rehberi" → E-11; hiç yayına alınmamış metnin bağlantısı yok | UC-16 · UC-17 · UC-18 | eşlendi |
| `04` 3.1.20 | ETBİS doğrulama bandı → ETBİS'in sayfası | uç doğurmaz — dış sayfa | uç doğurmaz |
| `04` 3.1.21 | Kimlik bloğunun "İletişim" başlığı → E-10; E-13 dahil her sayfada | UC-14 | eşlendi |
| `04 §3.2` metin | Panel menüsü sekiz girişli; üst satır dört geçiş taşır; menüde yeri olmayan ekranlar — E-29, E-30 oturumdan önce, E-37, E-39, E-33 listelerinden, E-54 E-50'den ve bağlantıdan, E-50 üst satırdan; E-48 ekran değildir | UC-84 · UC-85 · UC-92 · UC-94 · UC-88 · UC-108 · UC-104 · konvansiyon — API5-01 | eşlendi |
| `04` 3.2.1 | Menü Ana sayfa → E-31 | UC-86 | eşlendi |
| `04` 3.2.2 | Siparişler → E-36 → E-37 | UC-91 · UC-92 | eşlendi |
| `04` 3.2.3 | Talepler → E-38 → E-39 ya da E-37 | UC-93 · UC-94 · UC-92 | eşlendi |
| `04` 3.2.4 | Katalog · Ürünler → E-32 → E-33 | UC-87 · UC-88 | eşlendi |
| `04` 3.2.5 | Katalog · Kategoriler → E-34 | UC-89 | eşlendi |
| `04` 3.2.6 | Katalog · Kuponlar → E-35 | UC-90 | eşlendi |
| `04` 3.2.7 | İçerik ve site · Kurumsal içerik → E-41 | UC-96 | eşlendi |
| `04` 3.2.8 | İçerik ve site · Ana sayfa ve menü → E-42 | UC-97 | eşlendi |
| `04` 3.2.9 | İçerik ve site · Marka → E-43 | UC-98 | eşlendi |
| `04` 3.2.10 | Üyeler → E-40 | UC-95 | eşlendi |
| `04` 3.2.11 | Raporlar · Satış özeti → E-51 | UC-105 | eşlendi |
| `04` 3.2.12 | Raporlar · İşlem izi → E-52 | UC-106 | eşlendi |
| `04` 3.2.13 | Raporlar · Dışa aktarma → E-53 | UC-107 | eşlendi |
| `04` 3.2.14 | Ayarlar · Firma kimliği ve satış → E-44 | UC-99 | eşlendi |
| `04` 3.2.15 | Ayarlar · Ödeme yöntemleri → E-45 | UC-100 | eşlendi |
| `04` 3.2.16 | Ayarlar · Kargo, süreler ve varsayılanlar → E-46 | UC-101 | eşlendi |
| `04` 3.2.17 | Ayarlar · Yasal metinler → E-47 | UC-102 | eşlendi |
| `04` 3.2.18 | Ayarlar · Yönetici hesapları → E-49 | UC-103 | eşlendi |
| `04` 3.2.19 | Üst satırın satış durumu göstergesi → E-44 "Satış durumu" bölümü | UC-99 · konvansiyon — API5-01 | eşlendi |
| `04` 3.2.20 | Üst satır "Siteyi görüntüle" → E-01 | UC-01 | eşlendi |
| `04` 3.2.21 | Yöneticinin adı "Hesabım" → E-50 | UC-104 | eşlendi |
| `04` 3.2.22 | Yöneticinin adı "Çıkış" → E-29 | UC-109 · UC-84 | eşlendi |
| `04 §3.3` metin | Çerçevenin geçişleri §3.1'de; sipariş sayfasına üç yoldan girilir — sipariş geçmişi, takip girişi, e-postadaki bağlantı; kart dönüşü de sipariş sayfasına iner | UC-33 · UC-34 · UC-30 | eşlendi |
| `04` 3.3.1 | E-01 ürün vitrini, ürün kartı → E-04 | UC-04 | eşlendi |
| `04` 3.3.2 | E-01 birinci seviye kategori bağlantısı → E-02 | UC-02 | eşlendi |
| `04` 3.3.3 | E-01 kurumsal blok, Hakkımızda bağlantısı → E-08 | UC-09 | eşlendi |
| `04` 3.3.4 | E-01 Hizmetlerimiz ve Referanslarımız: içerik kartı → E-08, "Tümünü gör" → E-07 | UC-10 · UC-11 · UC-07 · UC-08 | eşlendi |
| `04` 3.3.5 | E-02 · E-03 ürün kartı → E-04 | UC-04 | eşlendi |
| `04` 3.3.6 | E-02 · E-04 kırıntı yolu → üst kategori; ürün sayfasında yol ana kategoriden | UC-02 | eşlendi |
| `04` 3.3.7 | E-04 sepete ekleme → E-04'te kalır, kısa süreli bildirim; satış açık, varyant satın alınabilir | UC-05 · UC-04 · konvansiyon — API2-02 | eşlendi |
| `04` 3.3.8 | E-07 içerik kartı → E-08 | UC-10 · UC-11 | eşlendi |
| `04` 3.3.9 | E-08 hizmet tanıtımı "Bize ulaşın" → E-10 iletişim formu | UC-14 | eşlendi |
| `04` 3.3.10 | E-08 "İlgili ürünler" ürün kartı → E-04; bağlı ürün Yayında | UC-04 | eşlendi |
| `04` 3.3.11 | E-10 formun gönderimi → E-10'da kalır, gönderim sonrası hâli; form açık (2.8.3) | UC-15 · UC-14 | eşlendi |
| `04` 3.3.12 | E-05, E-06 ve boş hâl satırlarının dönüş bağlantıları → E-01 · E-02 (birinci seviye kategoriler) | UC-01 · UC-02 | eşlendi |
| `04` 3.3.13 | E-12 kalemin adı ya da görseli → E-04; ürün Yayında | UC-04 | eşlendi |
| `04` 3.3.14 | E-12 ödeme adımına geçiş → E-13; satış açık, girebilecek kalem var, asgari tutar karşılanıyor | UC-22 · konvansiyon — API8-01 | eşlendi |
| `04` 3.3.15 | E-13 "Sepete dön" · girebilecek kalem kalmaması · hiçbir yöntemin kullanılamaması → E-12 | UC-19 · UC-27 | eşlendi |
| `04` 3.3.16 | E-13 İletişim bölümünün "Giriş yap"ı ya da hatırlatması → E-22 → E-13; dönüş §3.6.2 | UC-26 · UC-54 · UC-22 | eşlendi |
| `04` 3.3.17 | E-13 "Siparişi onayla — ödeme yükümlülüğü doğar" → E-14; özet değişmemiş, kutular işaretli | UC-27 · UC-28 | eşlendi |
| `04` 3.3.18 | E-14 (kart) numara gösterildikten sonra → ödeme sağlayıcısının sayfası | UC-29 · uç doğurmaz — dış sayfa | eşlendi |
| `04` 3.3.19 | E-14 (havale) siparişe ulaşma bağlantısı → üyede E-16, misafirde E-15; misafire yeni giriş yolu açılmaz | UC-33 · UC-31 | eşlendi |
| `04` 3.3.20 | E-27 sipariş satırı → E-16; üye oturumu açık | UC-33 | eşlendi |
| `04` 3.3.21 | E-15 numara ve e-postayla sorgu → E-16; eşleşmezse E-15'te kalır | UC-32 · UC-33 | eşlendi |
| `04` 3.3.22 | E-16 iptal · gecikme feshi → E-17; işlem açıksa | UC-38 · konvansiyon — API8-01 | eşlendi |
| `04` 3.3.23 | E-16 cayma → E-18; işlem açıksa | UC-42 · konvansiyon — API8-01 | eşlendi |
| `04` 3.3.24 | E-16 "sorun bildir" · yeniden açma → E-19; işlem açıksa | UC-44 · konvansiyon — API8-01 | eşlendi |
| `04` 3.3.25 | E-17 · E-18 · E-19 onay, gönderim, yeniden açma ya da "Vazgeç" → E-16; sonuç kalıcı | UC-39 · UC-40 · UC-41 · UC-43 · UC-45 · UC-46 · UC-33 | eşlendi |
| `04` 3.3.26 | E-22 kayıt bağlantısı · "şifremi unuttum" → E-20 · E-23; kayıt ekranı açıksa | UC-47 · UC-56 | eşlendi |
| `04` 3.3.27 | E-22 · E-20 "Google ile giriş" → Google'ın sayfası → §3.6; Google tanımlıysa | UC-50 · UC-51 | eşlendi |
| `04` 3.3.28 | E-20 kaydın gönderimi → E-20'de kalır, bağlantının gönderildiği hâli | UC-48 · UC-47 | eşlendi |
| `04` 3.3.29 | E-21 · E-23 (şifre kurulduktan sonra) giriş bağlantısı → E-22 | UC-54 | eşlendi |
| `04` 3.3.30 | E-25 şifre değiştirme · e-posta değiştirme · hesap silme → E-24 → E-25 | UC-60 · UC-63 | eşlendi |
| `04` 3.3.31 | E-25 hesap silmenin son adımının onayı → sonuç hâli; oturum kapanmıştır; geri alma bağlantısı açık değilse | UC-68 · UC-69 | eşlendi |
| `04` 3.3.32 | E-25 · E-26 · E-27 iç gezinme | UC-63 · UC-70 · UC-74 | eşlendi |
| `04` 3.3.33 | E-28 şifre sıfırlama bağlantısı eski adrese gönderilir → E-28'de kalır; yeni şifre E-23'te | UC-76 · UC-58 · uç doğurmaz — e-posta (`08`) | eşlendi |
| `04` 3.3.34 | E-21 (kayıt bağlantısının süresi dolmuş) → E-20 | UC-47 | eşlendi |
| `04` 3.3.35 | E-21 (yeni e-posta bağlantısı) sonraki adım → oturum açıksa E-25, değilse E-22 | UC-63 · UC-54 | eşlendi |
| `04` 3.3.36 | E-25 (şifresiz hesap) şifre belirleme → E-23 isteme adımı, e-posta ön dolu | UC-56 | eşlendi |
| `04` 3.3.37 | E-24 Google ile doğrulama → Google'ın sayfası → E-25, işlemin formu | UC-62 · UC-63 | eşlendi |
| `04` 3.3.38 | E-14 (kart) sağlayıcıya onaydan sonra bağlanılamaması → E-12; sipariş iptal, sepet korunur | UC-29 · UC-19 | eşlendi |
| `04` 3.3.39 | E-14 (kart) yeniden açılınca, sipariş artık Alındı + Bekliyor değilse → üyede E-16, misafirde E-15 | UC-28 · UC-33 · UC-31 | eşlendi |
| `04 §3.4` metin | Menünün geçişleri §3.2'de; panelin omurgası sayaçtan siparişe — ana sayfa → süzülmüş liste → kayıt → işlem | UC-86 · UC-91 · UC-92 | eşlendi |
| `04` 3.4.1 | E-29 giriş → E-31 ya da istenen panel ekranı; yönetici hesabı, şifreyle — giriş POST'u §9.1'de (1b) | UC-84 · UC-86 · konvansiyon — API2-03 | eşlendi |
| `04` 3.4.2 | E-31 sayaç → E-36'nın süzülmüş hâli; "açık talep" sayacında E-38'in süzülmüş hâli; sıfır sayaç boş listeye | UC-91 · UC-93 · konvansiyon — API2-06 | eşlendi |
| `04` 3.4.3 | E-31 uyarı satırı → karşılanmayan koşulun ekranı (E-44…E-47), "e-posta ulaşmadı" ve kart iadesi uyarılarında E-36'nın süzülmüş hâli; kanal uyarısı geçiş taşımaz | UC-99 · UC-100 · UC-101 · UC-102 · UC-91 · uç doğurmaz — kurulum | eşlendi |
| `04` 3.4.4 | E-31 kurulum kontrol listesi maddesi → E-44, E-45, E-47, E-46 | UC-99 · UC-100 · UC-102 · UC-101 | eşlendi |
| `04` 3.4.5 | E-31 "Müşteriden beklenenler" satırı → E-37; "Tümünü gör" → E-36'nın "IBAN bekleniyor" süzgeci | UC-92 · UC-91 · konvansiyon — API2-06 | eşlendi |
| `04` 3.4.6 | E-36 sipariş satırı → E-37 | UC-92 | eşlendi |
| `04` 3.4.7 | E-37 sipariş işlemi ya da müdahale → E-37'de kalır: formu, onay penceresi, güncel hâl — işlemlerin uçları §9.9'un maddeleridir (1b) | UC-92 · konvansiyon — API2-02 | eşlendi |
| `04` 3.4.8 | E-38 talep satırı → iletişim talebinde E-39, ayıp talebinde E-37'nin ayıp talebi bölümü | UC-94 · UC-92 | eşlendi |
| `04` 3.4.9 | E-39 · E-37 (ayıp talebi bölümü) listeye dönüş → E-38, bırakılan süzgeçle | UC-93 · konvansiyon — API2-06 | eşlendi |
| `04` 3.4.10 | E-32 yeni ürün · ürün satırı → E-33 | UC-88 | eşlendi |
| `04` 3.4.11 | E-33 "Önizle" ya da "Sitede gör" → E-04, arşivde E-05; taslakta bantla | UC-04 · konvansiyon — API2-03 | eşlendi |
| `04` 3.4.12 | E-41 kayıt satırı · yeni kayıt → E-41'in kayıt formu | UC-96 | eşlendi |
| `04` 3.4.13 | E-41 kayıt formu "Önizle" ya da "Sitede gör" → E-08; sayfası olmayan kayıtta E-09, E-10 ya da duyuru şeridi | UC-09 · UC-10 · UC-11 · UC-12 · UC-13 · UC-14 · konvansiyon — API2-03 | eşlendi |
| `04` 3.4.14 | E-40 üyenin bağlı siparişi → E-37; kayıt bulunmuşsa | UC-92 | eşlendi |
| `04` 3.4.15 | E-44 "Satış durumu" bölümünün koşul bağlantısı → E-44'ün kimlik formu, E-45, E-46, E-47 | UC-99 · UC-100 · UC-101 · UC-102 | eşlendi |
| `04` 3.4.16 | E-50 şifre ya da e-posta değiştirme → E-54 (yeniden doğrulama) → E-50 | UC-108 · UC-104 | eşlendi |
| `04` 3.4.17 | E-49 davet · geri çekme · kaldırma → E-49'da kalır — işlemlerin uçları §9.20'nin maddeleridir (1b) | UC-103 · konvansiyon — API2-02 | eşlendi |
| `04` 3.4.18 | E-30 hesabın açılması → E-29, davetin adresi ön dolu; geçersiz bağlantıda mesajın bağlantısıyla | UC-85 · UC-84 | eşlendi |
| `04` 3.4.19 | E-33 stok ya da kontenjan alanı, ayrılmış adedi tutan sipariş → E-37 | UC-92 | eşlendi |
| `04` 3.4.20 | E-33 · E-37 "Listeye dön" → E-32 · E-36, bırakılan sayfa, arama ve süzgeçle | UC-87 · UC-91 · konvansiyon — API2-06 | eşlendi |
| `04` 3.4.21 | E-39 "Siparişlerde ara" · "Üye kaydında ara" → E-36 · E-40, talebin e-postasıyla arama | UC-91 · UC-95 · konvansiyon — API2-06 | eşlendi |
| `04` 3.4.22 | E-47 aydınlatmanın tamamlanma satırı → E-44 kimlik formu; kimlik alanı boşken | UC-99 | eşlendi |
| `04` 3.4.23 | E-54 yeni adresin ve geri almanın bağlantısı sonraki adım → oturum açıksa E-50, değilse E-29 | UC-104 · UC-84 | eşlendi |
| `04 §3.5` metin | Sitenin dışından girişler; e-postanın metni ve bağlantının biçimi `08`'in, adreslerin biçimi `07`'nin işi; F-1…F-6'nın bağlantısı `08`'de, panel ekranına iniyorsa 3.5.14 işler | konvansiyon — API2-01 · uç doğurmaz — e-posta (`08`) | konvansiyon |
| `04` 3.5.1 | Sipariş e-postasındaki sipariş sayfası bağlantısı (erişim anahtarı) → E-16, e-posta sormadan; kişisel veriler imha edilmemişse | UC-34 · konvansiyon — API2-01 | eşlendi |
| `04` 3.5.2 | Sipariş e-postasındaki dijital ürünün indirme bağlantısı → E-16, aynı kapı | UC-34 | eşlendi |
| `04` 3.5.3 | B-14 "IBAN gerekli" e-postasının bağlantısı → E-16, IBAN alanı açık | UC-34 · UC-35 | eşlendi |
| `04` 3.5.4 | Kaydın e-posta doğrulama bağlantısı → E-21; geçerliyse doğrulama adımı, açılış doğrulamaz (K-904); süresi dolmuşsa söyler | UC-52 · UC-53 · konvansiyon — API2-01 | eşlendi |
| `04` 3.5.5 | Şifre sıfırlama bağlantısı → müşteride E-23, yöneticide E-29; tek kullanımlık ve ömrü içinde | UC-58 · UC-84 · konvansiyon — API2-01 | eşlendi |
| `04` 3.5.6 | Yeni e-posta adresinin doğrulama bağlantısı → müşteride E-21, yöneticide E-54 | UC-52 · UC-108 | eşlendi |
| `04` 3.5.7 | "Bu değişikliği ben yapmadım" bağlantısı → müşteride E-28, yöneticide E-54; Z-45 içinde | UC-75 · UC-108 | eşlendi |
| `04` 3.5.8 | Ödeme sağlayıcısının sayfasından 3D Secure dönüşü → E-16; sonuç gelmemişse ödemenin beklendiği — dönüşün yolu ve yöntemi `08`'in park satırı (K-906 · K-973) | UC-30 · UC-33 | eşlendi |
| `04` 3.5.9 | Google'dan dönüş → §3.6'nın dönüş yeri; doğrulanmamış e-postada E-20; veri toplayan girişlerin kapısı kapalıyken yeni hesapta E-22, mesajla | UC-51 · UC-47 · UC-54 | eşlendi |
| `04` 3.5.10 | Yönetici daveti e-postasının bağlantısı → E-30; kullanılmış, süresi dolmuş ya da geçersizleşmiş davette mesaj | UC-85 · konvansiyon — API2-01 | eşlendi |
| `04` 3.5.11 | Paylaşılmış ya da arama motorundan gelen ürün adresi → Yayında E-04, Arşiv E-05, taslak ya da silinmiş E-06 (yöneticiye taslak bantla) | UC-04 · UC-06 · konvansiyon — API2-01 | eşlendi |
| `04` 3.5.12 | Paylaşılmış içerik adresi → Yayında E-08, taslak ya da silinmiş E-06 | UC-09 · UC-10 · UC-11 · UC-12 · UC-06 | eşlendi |
| `04` 3.5.13 | Var olmayan adres → E-06 | UC-06 | eşlendi |
| `04` 3.5.14 | Oturum gerektiren ekranın adresi, oturum yokken → vitrinde E-22, panelde E-29 → istenen ekran | UC-54 · UC-84 · konvansiyon — API2-03 | eşlendi |
| `04` 3.6.1 | Oturum gerektiren ekrana oturumsuz gelen kendi kapısının girişine gider ve girişten sonra istediği ekrana iner; iki kapı ayrı; panel girişini doğrudan açan yönetici E-31'e | UC-54 · UC-55 · UC-84 · UC-86 · konvansiyon — API2-03 | eşlendi |
| `04` 3.6.2 | Ödeme adımından girişe giden müşteri ödeme adımına döner, yazdıkları durur; üye olarak görür, sepetler birleşir, kupon yeniden uygulanır; yazılanlar yalnız o tarayıcıda, onaydan önce kayıt oluşturmaz | UC-26 · UC-55 · UC-51 · UC-22 | eşlendi |
| `04` 3.6.3 | Girişi çerçeveden açan müşteri geldiği vitrin sayfasına döner; giriş, kayıt, sıfırlama ya da iniş ekranından geldiyse ana sayfaya; Google aynı dönüşü kullanır | UC-55 · UC-51 · UC-01 · konvansiyon — API2-03 | eşlendi |
| `04` 3.6.4 | Çıkış: müşteri oturum gerektirmeyen sayfada kalır, gerektirende ana sayfaya; sepeti hesapta kalır; yönetici panel girişine | UC-77 · UC-109 · UC-01 · UC-84 | eşlendi |
| `04` 3.6.5 | Kendiliğinden kapanan oturum: oturum gerektiren işlemde mesaj ve girişe; ödeme adımında dönüş 3.6.2; panelde ve hesap ekranlarında kaydetme anında kapanmışsa form gönderilmez, değişikliklerin korunmayacağı söylenir (K-844) | UC-54 · UC-84 · UC-26 · konvansiyon — API2-03 | eşlendi |
| `04 §3.7` metin | Yönetici oturumu vitrinin hesap alanını ve sepetini değiştirmez; hesap alanı o tarayıcının müşteri oturumuna göre | konvansiyon — API2-03 | konvansiyon |
| `04` 3.7.1 | Panelin her ekranı "Siteyi görüntüle" → E-01, yönetici şeridiyle; panel oturumu açık | UC-01 · konvansiyon — API2-03 | eşlendi |
| `04` 3.7.2 | E-33 · E-41 "Önizle" ya da "Sitede gör" → kaydın vitrindeki sayfası; taslakta bantla; kayıt kaydedilmiş | UC-04 · UC-09 · UC-10 · UC-11 · UC-12 · konvansiyon — API2-03 | eşlendi |
| `04` 3.7.3 | Vitrinin her sayfasının yönetici şeridi "Panele dön" → E-31 | UC-86 | eşlendi |
| `04` 3.7.4 | Önizlenen taslak kaydın "Düzenlemeye dön"ü → E-33 ya da E-41 | UC-88 · UC-96 | eşlendi |
| `04` 3.7.5 | Yönetici oturumu kapanmışken taslak adresi → E-06 | UC-06 · konvansiyon — API2-03 | eşlendi |

#### 1.1.5 Arayüz Tanımları — `04 §2` ortak bileşenler ve §4.4 ekran olmayan yüzeyler (33)

Ortak bileşen bileşen kimliğiyle tek satırdır; bileşenin bütün maddeleri ve tabloları tam okunur ve özet uca dokunan maddeleri sayar (K-1150). Bileşenin kuralı çoğunlukla bütün uçlara uygulanır — değeri konvansiyondur.

| Kaynak ID | Kaynak özeti | Uç | Durum |
|---|---|---|---|
| `04` OB-01 | Tasarım tabanı (§2.1): üç genişlik sınıfı, dar ya da geniş sınıftan başlayan tasarım — hiçbir içerik ve işlev bir sınıfta gizlenmez —, marka renginin yerleri, erişilebilirlik tabanı (klavye, odak, etiket, %200 yakınlaştırma, tutarlı yardım, tekrarlı giriş) | uç doğurmaz — görünüm | uç doğurmaz |
| `04` OB-02 | Çerçeve (§2.2): vitrinin beş bölgesi — duyuru şeridi (Yayında ve aralığında), üst bölüm (logo, arama, hesap alanı, sepetin kalem sayısı), menü (düzene göre sıra, kategoriler alfabetik, "Diğer"), içerik, altbilgi (kimlik bloğu, ETBİS, yasal bağlantılar, platform imzası); ödeme adımının sadeleşmiş çerçevesi; panel çerçevesi (satış durumu, "Siteyi görüntüle", "Hesabım", "Çıkış", menünün bekleyen iş işareti); çerçevesiz panel ekranları — çerçevenin verisi her sayfa ucunun yanıtındadır | konvansiyon — API5-01 · UC-03 · UC-19 · UC-77 · UC-109 | eşlendi |
| `04` OB-03 | Mesaj (§2.3): dört yer — alan mesajı, yerinde kalıcı mesaj, bir kez söylenen mesaj (sayfa yenilenince geri gelmez), kısa süreli bildirim; form gönderiminde hatalar birlikte ve özet, yazılanlar silinmez; panelin engel mesajı; nötr limit mesajı; hata ve geri alınamaz işlemin sonucu kısa süreli bildirimle verilmez — mesajın yanıtta taşınması ve formun değerleriyle yeniden gösterim | konvansiyon — API2-07 · konvansiyon — API2-02 | konvansiyon |
| `04` OB-04 | Onay (§2.4): tek anatomi; onay isteyen işlemler kaynakta sayılıdır; yöneticinin penceresi — girdiler formda, pencere formun son adımı, açıkken sipariş değişirse uygulanmaz; müşterinin son adımı sayfanın içinde; onaydan sonra düğme ikinci kez basılamaz; sonucu belirsiz onay; "geri al" yok — betiksiz onay adımının uç biçimi (GAP-1, K-732) | konvansiyon — API2-02 · konvansiyon — API2-04 · konvansiyon — API2-05 | konvansiyon |
| `04` OB-05 | Rozet ve işaretler (§2.5): sipariş, ödeme, yayın, talep durumlarının rozetleri — iki eksen iki rozet; kalem kayıtları; yedi panel işareti; "e-posta ulaşmadı" — satırda "Yeniden gönder" ve "Hepsini yeniden gönder" (uçları §9.9'da, 1b); panel ana sayfasının uyarıları; vitrin işaretleri | konvansiyon — API5-01 · UC-86 · UC-91 · UC-92 · UC-33 · UC-74 | eşlendi |
| `04` OB-06 | Sayaç ve süre göstergeleri (§2.6): altı bekleyen iş sayacı ve süzülmüş listeleri, "IBAN bekleniyor" listesi (ilk on satır), panelde kalan süre ve aşım, müşteriye görünen süreler, eşiğe kalan tutar; süre gün ve son günün tarihiyle, canlı geri sayım yok | UC-86 · UC-91 · UC-92 · UC-33 · UC-19 · UC-22 · konvansiyon — API2-09 | eşlendi |
| `04` OB-07 | Boş, yükleniyor ve hata hâlleri (§2.7): boş hâl — görünmez ya da boş hâl satırı; yükleniyor — işlem sürerken düğme ikinci kez basılamaz; beklenmeyen hatanın üç hâli — işlem düzeyi, sayfa düzeyinde hata sayfası ("Sayfa bulunamadı"dan ayrı), sonucu belirsiz işlemde sonucu göstermeye yönlendirme; adı konmuş hatalar kendi mesajlarıyla; kesinti ve bakım ekranı yok | UC-78 · UC-06 · konvansiyon — API2-07 · konvansiyon — API2-05 | eşlendi |
| `04` OB-08 | Satış kapalı hâli (§2.8): vitrinde sepete ekleme ve ödeme adımına geçiş kapalı, sepet korunur; panelde dört yer; veri toplayan girişlerin kapısı — iletişim formu, kayıt ve yeni hesap açacak Google girişi kapalı — kapı sunucuda da denetlenir | UC-05 · UC-22 · UC-27 · UC-15 · UC-48 · UC-51 · UC-99 · UC-86 · konvansiyon — API8-01 | eşlendi |
| `04` OB-09 | "Taslak" bandı ve yönetici şeridi (§2.9): şerit yalnız panel oturumu olan tarayıcıda, "Panele dön"; taslak önizlemede bant ve "Düzenlemeye dön"; kendi sayfası olmayan taslakta etiket; vitrinin hesap alanı ve sepeti yönetici oturumundan etkilenmez | konvansiyon — API2-03 · UC-86 · UC-88 · UC-96 | eşlendi |
| `04` OB-10 | Eşzamanlı düzenleme uyarısı (§2.10): kayıt varyantı — ikinci kaydeden uyarıyı görür, güncel hâli açar ya da uyarıyı görerek üzerine yazar; bölümleri ayrı kaydedilen ekranda bölüm başına; sipariş varyantı — işlem uygulanmaz, güncel hâl gösterilir, üzerine yazma yok | konvansiyon — API2-04 | konvansiyon |
| `04` OB-11 | Ürün kartı (§2.11): ana görsel, ad, KDV dahil fiyat; tek eylemi ürün sayfası; indirimli ve tükenmiş kart; içerik kartı; ızgaranın sütunları | UC-01 · UC-02 · UC-03 · UC-07 · UC-08 · UC-10 · UC-11 | eşlendi |
| `04` OB-12 | Form alanı ve form düzeni (§2.12.1): etiket ve zorunluluk, alan mesajı; hatalar birlikte; kapalı liste serbest metin kabul etmez; sayısal sınırın aşımı; kaydetme başarısı ve engel; aydınlatma bağlantısı; üyeye ön dolu; adet alanı — sepete eklemede stok ve P-9, işlem adedinde o işleme açık adet | konvansiyon — API2-07 · konvansiyon — API5-01 · UC-05 · UC-20 · UC-40 · UC-43 · UC-45 | eşlendi |
| `04` OB-13 | Adres formu (§2.12.2): alanlar `02 §3.14.6`'dan; teslimat, fatura, defter ve panelde düzeltme varyantları; teslimat yapılmayan ildeki adres kabul edilmez | UC-27 · UC-25 · UC-71 · UC-72 · UC-92 | eşlendi |
| `04` OB-14 | IBAN alanı (§2.12.3): havale hattında; biçim ve sağlama basamağı; beyanla giriş, istekle açılan alan, panelde salt okunur; işlenene kadar düzeltilir; donmuş firma IBAN'ı bu bileşen değildir | UC-35 · UC-40 · UC-41 · UC-43 · UC-92 | eşlendi |
| `04` OB-15 | Sebep seçimi (§2.12.4): firma iptali ve durum düzeltmesi kapalı listeden; "Diğer"de açıklama zorunlu; sebep kayıtta kalır ve sonucu belirler — uçları §9.9'da (1b) | UC-92 | eşlendi |
| `04` OB-16 | Tarih alanı (§2.12.5): olay tarihi — ileri tarih reddedilir, alt sınır; sıra sorusu formun alanıdır; tarih aralığı gün başından gün sonuna (K-1023); yazım biçimi §8.2 | konvansiyon — API2-09 · UC-92 · UC-88 · UC-90 · UC-96 · UC-105 · UC-106 · UC-107 | eşlendi |
| `04` OB-17 | Kupon alanı (§2.12.6): ödeme adımında kapalı satır; büyük-küçük harf ayrımı yok; "Değiştir" ve "Kaldır"; en fazla bir kod; geçersiz kod ve asgari tutar mesajı; L-6; girişten dönüşte yeniden uygulanır | UC-22 · UC-23 · UC-24 · konvansiyon — API2-08 | eşlendi |
| `04` OB-18 | Onay kutuları (§2.12.7): iki ile dört kutu, hiçbiri işaretli gelmez; işaretsiz kutuyla sipariş oluşmaz; sürüm ya da içerik değişirse işaret kalkar; rıza, yaş ve pazarlama kutusu yok; sipariş sayfasında kutuların kaydı salt okunur | UC-22 · UC-27 · UC-33 | eşlendi |
| `04` OB-19 | Görsel ve dosya yükleme (§2.12.8): galeri ve elle sıra, alternatif metin, biçim ve boyut sınırları (P-22…P-25), yükleme sürerken form kaydedilemez, tek görsel, dijital dosya (P-26); müşteri tarafında yükleme yok; silinmiş yüklemeyle kayıt — kayıt yapılmaz, alan mesajı yeniden yüklemeyi ister (K-1100, K-1130) | UC-83 · UC-88 · UC-96 · UC-98 · konvansiyon — API6-02 | eşlendi |
| `04` OB-20 | Liste ve tablo (§2.13): ürün listelerinde sabit sıra; arama ve süzgeç yalnız kaynağın saydığı listelerde; sayfa numaralı gezinme — vitrin 24, içerik 24, sipariş geçmişi 25, panel 25; arama ve süzgeç değişince ilk sayfa, dönen kullanıcı bıraktığı sayfayı bulur; panel tabloları ve kart listesi; toplu işlem yok | konvansiyon — API2-06 · UC-02 · UC-03 · UC-07 · UC-08 · UC-74 · UC-87 · UC-91 · UC-93 · UC-95 · UC-106 | eşlendi |
| `04 §4.4` metin | Kullanıcının karşısına çıkan ama ekran olarak tanımlanmayan yüzeyler adıyla ve evine işaretle yazılır | uç doğurmaz — yazım konvansiyonu | uç doğurmaz |
| `04` 4.4.1 | E-postalar (B-1…B-16, F-1…F-6, hesap ve yönetim e-postaları) — metin ve şablon `08`; bağlantılarının indiği uçlar §3.5'in satırlarıdır | uç doğurmaz — e-posta (`08`) | uç doğurmaz |
| `04` 4.4.2 | Ödeme sağlayıcısının sayfası ve 3D Secure — ürünün dışında; kart bilgisi sağlayıcıda girilir; geçiş ve dönüş uçları ürünündür | UC-29 · UC-30 · uç doğurmaz — dış sayfa | eşlendi |
| `04` 4.4.3 | Google'ın giriş sayfası — ürünün dışında; yönlendirme ve dönüş uçları ürünündür | UC-50 · UC-51 · UC-62 · uç doğurmaz — dış sayfa | eşlendi |
| `04` 4.4.4 | ETBİS, sosyal medya, WhatsApp, kargo takip, video platformu, şubenin harita bağlantısı — dış sayfalar | uç doğurmaz — dış sayfa | uç doğurmaz |
| `04` 4.4.5 | Arama motorlarına verilen veriler, site haritası ve paylaşım önizlemesi — sayfanın taşıdığı veri; evi `05` | UC-79 · konvansiyon — API4-09 | eşlendi |
| `04` 4.4.6 | Tarayıcı sekmesinin başlığı ve site simgesi — kuralı `02`'de; başlık her sayfa ucunun yanıtında, simge görsel adresiyle (ters vekil — `05 §7.7.1`) | konvansiyon — API5-01 · konvansiyon — API2-01 | konvansiyon |
| `04` 4.4.7 | Cihazın paylaşım menüsü | uç doğurmaz — istemci | uç doğurmaz |
| `04` 4.4.8 | Dışa aktarılan dosyalar — taşıdığı alanlar `02 §10.7`'de; dosyanın ucu §9.24'te (1b; API9-05) | UC-107 | eşlendi |
| `04` 4.4.9 | Onay penceresi (OB-04), hata sayfası ve bekleme göstergesi (OB-07) — kendi ekran kimliği olmayan kalıplar | UC-78 · konvansiyon — API2-02 | eşlendi |
| `04` 4.4.10 | Kesinti ve bakım ekranı yoktur | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 4.4.11 | Bekleyen işler, toplu işlem ve kurulum sihirbazı ekranı yoktur — sayaçlar süzülmüş listeye götürür | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 4.4.12 | Fatura, banka havalesi ve kargo teslimi — sistem dışı adımlar, ürün izlemez | uç doğurmaz — ürünün dışında | uç doğurmaz |

#### 1.1.13 GAP adayları (`AA-n`)

Eşlenmeyen ya da cevabı kaynakta görünmeyen kaynak satırı burada numarasıyla durur (K-1148); aday, dayandığı bölüm tam okunmadan GAP diye yazılmaz ve cevabı kaynakta yazılı çıkan aday boşluk değil hizalama hatasıdır (checklist §2; K-739). Adayın kararı matrisin 2. oturumunundur — karar kaydının §3'üne GAP-14'ten ve §1.3'e (K-1150); bir W konusuna bağlanan aday o konuda karara bağlanır. **Kapı:** matrisin 2. oturumu.

| # | Kaynak satırı | Aday | Dayandığı yerler (tam okundu) | Karar yeri ve seçenekler |
|---|---|---|---|---|
| AA-1 | `04` 5.13.19 (§1.1.3) | Ödeme adımından "Sepete dön"le ayrılan müşterinin yazdıkları "o tarayıcıda ve o ziyaret boyunca durur"; Teknik Mimari'nin çerezi formu yalnız girişe giden iki öğenin — "Giriş yapın" ve hatırlatma — gönderimiyle çereze yazar, "Sepete dön" sadeleşmiş çerçevenin bağlantısıdır ve formu göndermez; betiksiz yolda yazılanların durması bir form gönderimi ister (MK-05) | `04` 5.13.19, 3.1.10, 2.2.7, §3.6.2 · `05 §6.7.7`, §6.7.9 (MK-56) · K-758, K-907 | Matrisin 2. oturumu; konu API2-02 ile bağlıdır (sayfa ve form ucunun biçimi). (a) "Sepete dön" de formu çereze yazıp yönlendiren düğmedir — `05 §6.7.9`'un listesi genişler, uç UC-26'nın ikinci hedefi ya da yeni aday · (b) 5.13.19'un cümlesi girişe ve oturumun yenilenmesine daralır — `04` geri beslenir · (c) yazılanlar yalnız betikle, tarayıcıda tutulur — `05 §6.7.9`'un "JavaScript kapalıyken de" ilkesine aykırıdır |

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
