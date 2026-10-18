# Shopfolio — API Design

**Versiyon: v0.7** | **Bağımlılıklar:** `02`, `03`, `04`, `05`, `06`, `10_MVP_SCOPE.md` | **Son güncelleme:** 2026-10-18

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

**Alt bölüm haritası (K-1152).** Matris yedi oturumda kurulur (K-1150): **1a** (2026-10-18) kaynak birimini netleştirdi (K-1151), dört ailenin envanterini ve 1a'nın kapsama denetimini, aday uç listesini ve "Uç" sütununun değer kümesini (K-1153) ve Arayüz Tanımları'nın vitrin ekranlarının (`04 §5`), navigasyon haritasının (§3), ortak bileşenlerinin (§2) ve ekran olmayan yüzeylerinin (§4.4) ileri izlenebilirliğini yazdı; **1b** (2026-10-18) panel ekranlarının (`04 §9`) ve §4.2'nin metninin ileri izlenebilirliğini yazdı ve aday listesine panelin POST işlemlerini, görsel yüklemeyi ve dışa aktarmanın dosyalarını ekledi (UC-110…UC-187; K-1154, K-1155); **1c** (2026-10-18) durum × rol matrisinin (`04 §6`), formların ve ortak alan kurallarının (§7) ve lokalizasyonun (§8) ileri izlenebilirliğini yazdı — aday eklemedi, eşlemenin okunuşu K-1156'dadır; **1d** (2026-10-18) Kullanıcı Akışları'nın durum makinesinin (`03 §1`), ana akışlarının (§2), hata akışlarının (§3) ve zaman aşımı yönetiminin (§4) ileri izlenebilirliğini yazdı — aday eklemedi, eşlemenin okunuşu K-1157'dedir; **1e** (2026-10-18) Kullanıcı Akışları'nın itiraz (`03 §5`), kötüye kullanım (§6), bildirim haritası (§7 — toplu; bağlantı taşıyan 14 satır kimliğiyle), yönetim (§8), destekleyici (§9) ve operasyonel (§10) akışlarının ileri izlenebilirliğini yazdı — aday eklemedi, eşlemenin okunuşu K-1158'dedir; **1f** (2026-10-18) devirlerin — etki sütununda `07` taşıyan karar satırları, park maddeleri, dizin satırları, gövde cümleleri ve Entegrasyon Spesifikasyonu'nun park satırı — ve Teknik Mimari'nin MK-04'ünün ve uç düzeyindeki bölümlerinin ileri izlenebilirliğini yazdı — aday eklemedi, eşlemenin okunuşu K-1159'dadır; **2** (2026-10-18) geri izlenebilirliği (§1.2) betikle kurdu — 187 adayın hepsi beslenir, gerekçesiz ekleme yok —, aday uç listesini 187 adayla kapattı, kesilmiş okumanın kümesinin boş olduğunu kaydetti ve 42 satırlık örneklemi yeniden okuttu (ayrışma %2,4), tek GAP adayını karara bağladı — AA-1 → GAP-14 (§1.3) — ve geri beslemeyi Teknik Mimari'ye, Ürün Gereksinimleri'ne ve Doğrulama Protokolü'nün park satırına yazdı (K-1160, K-1161); oturum bölünmedi. Matris tamamlandı. Şablonun §1.1, §1.2 ve §1.3 numaraları değişmez; alt düzey aşağıdaki tabloyla sabittir. Bir oturum bir alt bölümü bölmek ya da birleştirmek zorunda kalırsa bu tabloyu ve ona atıf yapan satırları aynı PR'da günceller (K-670'in kalıbı; `checklists/document-stage.md` §4).

| Alt bölüm | İçerik | Oturum | Durum |
|---|---|---|---|
| §1.1.1 | Kaynak envanteri ve sayım — dört aile, açılışın sayımıyla fark, betik komutları, kapsama denetimi | 1a; her oturum kendi ailesinin denetimini ve okuma derinliği kaydını ekler | ✓ (dört ailenin envanteri; 1a'nın, 1b'nin, 1c'nin, 1d'nin, 1e'nin ve 1f'nin denetimi; 2'nin geri izlenebilirlik, GAP ve örneklem denetimleri) |
| §1.1.2 | Aday uç listesi (`UC-nn`) ve "Uç" sütununun değer kümesi | 1a; 1b–1f listenin sonuna aday ekler; 2 geri yönde kapatır | ✓ — UC-01…UC-187: 1a UC-01…UC-109, 1b UC-110…UC-187; 1c, 1d, 1e ve 1f aday eklemedi; 2 listeyi 187 adayla kapattı — UC-26'nın adı GAP-14 ile değişti (K-1160, K-1161) |
| §1.1.3 | Arayüz Tanımları — `04 §5` vitrin ekranları: 28 ekran, 364 kalın numaralı madde, 23 kimliksiz madde — 415 | 1a | ✓ |
| §1.1.4 | Arayüz Tanımları — `04 §3` navigasyon haritası: 124 geçiş satırı, 5 kalın numaralı paragraf, 6 metin — 135 | 1a | ✓ |
| §1.1.5 | Arayüz Tanımları — `04 §2` ortak bileşenler bileşen kimliğiyle (20) ve §4.4 ekran olmayan yüzeyler (12 satır ve metin) — 33 | 1a | ✓ |
| §1.1.6 | Arayüz Tanımları — `04 §9` panel ekranları: 25 ekran, 371 kalın numaralı madde, 34 kimliksiz madde — ve §4.2'nin metni — 431 | 1b | ✓ |
| §1.1.7 | Arayüz Tanımları — `04 §6` durum × rol: 286 satır, 53 ekran bloğu, §6.1'in 17 kalın numaralı paragrafı, 2 metin — 358 | 1c | ✓ |
| §1.1.8 | Arayüz Tanımları — `04 §7` formlar: 42 form, 193 alan satırı, §7.3'ün 5 kuralı — ve §8 alt bölüm düzeyinde (3) — 243 | 1c | ✓ |
| §1.1.9 | Kullanıcı Akışları — `03 §1`–§4 satır düzeyinde: numaralı satır ve kalın paragraf 420, durum 20, geçiş 16, madde 45, metin 35 — 536 | 1d | ✓ |
| §1.1.10 | Kullanıcı Akışları — `03 §5`–§10 satır düzeyinde (§7 toplu: 3 alt bölüm satırı ve bölüm metni; bağlantı taşıyan bildirim satırı kimliğiyle ayrıca): numaralı satır 259, madde 10, metin 18, §7 4 — 291; kimliğiyle giren bağlantılı bildirim satırı 14 (K-1158) — matriste 305 | 1e | ✓ |
| §1.1.11 | Devirler — 51 karar satırı, 14 park maddesi, 4 dizin satırı, 20 gövde cümlesi, başka dokümanın 1 park satırı — 90 | 1f | ✓ |
| §1.1.12 | Teknik Mimari — MK-04, uç düzeyindeki 13 bölümün 96 numaralı maddesi ve 5 kimliksiz maddesi — 102 | 1f | ✓ |
| §1.1.13 | GAP adayları (`AA-n`) — karara bağlandı (2) | 1a açtı; 1b–1f ekler; 2 karara bağlar | ✓ — 1 aday (AA-1) karara bağlandı: AA-1 → GAP-14 (K-1161); 1b, 1c, 1d, 1e ve 1f aday eklemedi |
| §1.2 | Geri izlenebilirlik (uç → kaynak); gerekçesiz ekleme listesi; kesilmiş okumanın örneklemi; aday uç listesinin kapanışı | 2 | ✓ — 187 aday, 5.051 çift; gerekçesiz ekleme yok; kesilmiş okuma yok, örneklem 42 satır, ayrışma 1 (K-1160) |
| §1.3 | Boşluklar (GAP) ve kararlar; üst dokümanlara geri besleme tablosu (K-1147) | 2; workshop ve yazım geri besleme satırı ekler | ✓ (2) — GAP-14; geri besleme satırı 1 — workshop ve yazım ekler |

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

**1b — bölüm bazında** (431): `04 §9` 430 (ekran 25, kalın numaralı madde 371, kimliksiz madde 34 — alan başlığı 20, 9.13.4'ün, 9.16.4'ün ve 9.19.3'ün alt maddesi 14) · §4.2 metin 1. §1.1.6 431.

**1c — bölüm bazında** (601): `04 §6` 358 (ekran bloğu 53 — 6.2.n 28, 6.3.n 25; satır 286; §6.1'in kalın numaralı paragrafı 17; metin 2 — §6.1 ve §6.3) · §7 240 (form 42; alan satırı 193 — formların alanları 165, §7.3.2'nin alan türleri 19, §7.3.3'ün limitleri 9; §7.3'ün kalın kuralı 5) · §8 3 (§8.1, §8.2, §8.3). §1.1.7 358 · §1.1.8 243.

**1d — bölüm bazında** (536): `03 §1` 183 (numaralı tablo satırı 99, kalın numaralı paragraf 16, durum 20, geçiş 16, madde 17, metin 15) · §2 137 (numaralı 99, madde 22, metin 16) · §3 152 (numaralı 147, madde 3, metin 2) · §4 64 (numaralı 59, madde 3, metin 2). §1.1.9 536.

**1e — bölüm bazında** (291 + 14): `03 §5` 34 (numaralı 33, metin 1) · §6 54 (numaralı 52, metin 2) · §7 4 (alt bölüm 3, bölüm metni 1) ve kimliğiyle giren bağlantılı bildirim satırı 14 (`opt03()`'ün 131 satırından; "bağlantı" geçen 18 adayın 14'ü) · §8 121 (numaralı 106, madde 5, metin 10) · §9 40 (numaralı 35, madde 3, metin 2) · §10 38 (numaralı 33, madde 2, metin 3). §1.1.10 305.

**1f — bölüm bazında** (192): devirler 90 — karar satırı 51 (Aşama 1'in kalite döngüsü 5, Aşama 2 1, Aşama 3 6, Aşama 4 23, Aşama 5 16), park maddesi 14 (Aşama 1 2, Aşama 3 3, Aşama 4 6, Aşama 5 3), dizin satırı 4, gövde cümlesi 20 (`02` 5, `04` 1, `05` 7, `06` 7), başka dokümanın park satırı 1 (`08 park A4-3`) · Teknik Mimari 102 — MK-04 1, numaralı madde 96 (§3.5 6, §3.6 6, §3.7 5, §4.5 5, §6.1 6, §6.2 5, §6.3 7, §6.4 9, §6.5 6, §6.7 12, §7.7 5, §9.4 17, §10.3 7), kimliksiz madde 5 (`05 §6.5` m1–m5). §1.1.11 90 · §1.1.12 102.

**Betik komutları** (Git Bash; depo kökünden). Sayımlar yukarıdaki sayılarla aynı çıkmalı; 1a'nın, 1b'nin, 1c'nin, 1d'nin, 1e'nin ve 1f'nin kapsama denetimleri — eksik, fazla, yinelenen, başlığa bağlılığın dördü —, değer kümesi denetimi, "Durum" denetimi, aday listesi denetimi ve GAP adayı denetimi boş dönmelidir:

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
# GAP adayları: satırlardaki AA-n ile §1.1.13'ün karara bağlanmamış adayları iki yönlü aynı olmalı (2. oturumdan beri karara
# bağlanan adayın son hücresi "Karar: GAP-n" taşır ve satırları "GAP adayı" durumunu bırakır)
gap07() {
  diff <(awk '/^#### 1\.1\.([3-9]|1[0-2]) /{f=1; next} /^#### /{f=0} f && /GAP adayı \(AA-/{match($0,/AA-[0-9]+/); print substr($0,RSTART,RLENGTH)}' Docs/07_API_DESIGN.md | sort -u) \
       <(awk '/^#### 1\.1\.13 /{f=1; next} /^#{2,4} /{f=0} f && /^\| AA-[0-9]+ \|/ && !/Karar: GAP-[0-9]+/{split($0,c,"|"); x=c[2]; gsub(/ /,"",x); print x}' Docs/07_API_DESIGN.md | sort -u)
}
# karara bağlanan aday §1.3'te "(aday AA-n)" ile durmalı — iki yönlü
gapk07() {
  diff <(awk '/^#### 1\.1\.13 /{f=1; next} /^#{2,4} /{f=0} f && /^\| AA-[0-9]+ \|/ && /Karar: GAP-[0-9]+/{split($0,c,"|"); x=c[2]; gsub(/ /,"",x); match($0,/Karar: GAP-[0-9]+/); print x " " substr($0,RSTART+7,RLENGTH-7)}' Docs/07_API_DESIGN.md | sort) \
       <(awk '/^### 1\.3 /{f=1; next} /^#{2,3} /{f=0} f && /^\| GAP-[0-9]+ \|/{split($0,c,"|"); g=c[2]; gsub(/ /,"",g); if (match($0,/\(aday AA-[0-9]+\)/)) print substr($0,RSTART+6,RLENGTH-7) " " g}' Docs/07_API_DESIGN.md | sort)
}
# geri izlenebilirlik (2. oturum; K-1160): aday–satır çiftleri "UC-nn<TAB>Kaynak ID<TAB>aile" §1.1.3–§1.1.12'den
geri07() {
  awk -F'|' '/^#### 1\.1\.([3-9]|1[0-2]) /{f=1; next} /^#### /{f=0}
       f && /^\| (`0[2-8]|K-[0-9]|park A|dizin A)/{x=$2; gsub(/`/,"",x); gsub(/^ +| +$/,"",x); gsub(/ +/," ",x)
         fam=(x ~ /^04 /)?"04":(x ~ /^03 /)?"03":(x ~ /^05 §/ && x !~ / g[0-9]+$/)?"05":"devir"
         n=split($4,e," · "); for(i=1;i<=n;i++){u=e[i]; gsub(/^ +| +$/,"",u); if (u ~ /^UC-[0-9]+$/) print u "\t" x "\t" fam}}' Docs/07_API_DESIGN.md
}
# aday başına satır sayısı ve aile dağılımı: UC · toplam · 04 · 03 · devir · 05
geri07say() { geri07 | awk -F'\t' '{c[$1]++; d[$1 SUBSEP $3]++} END{for(u in c) printf "%s\t%d\t%d\t%d\t%d\t%d\n", u, c[u], d[u SUBSEP "04"], d[u SUBSEP "03"], d[u SUBSEP "devir"], d[u SUBSEP "05"]}' | sort -t- -k2,2n; }
# makine ucu olmayıp ekran ya da akış satırı taşımayan aday (makine uçları K-868, MK-04: UC-37, UC-79…UC-83)
geri07ek() { geri07say | awk -F'\t' '$3+$4==0 && $1 !~ /^UC-(37|79|8[0-3])$/'; }
# §1.2'nin tablosu: aday, satır sayısı ve durum
tab07() { awk -F'|' '/^### 1\.2 /{f=1; next} /^#{2,4} /{f=0} f && /^\| UC-[0-9]+ /{u=$2; sub(/^ +/,"",u); sub(/ .*/,"",u); n=$3; sub(/^ +/,"",n); sub(/ .*/,"",n); d=$4; gsub(/^ +| +$/,"",d); print u "\t" n "\t" d}' Docs/07_API_DESIGN.md; }
# örneklem (2. oturum): her oturumdan yedi ileri satır, sabit tohum
ornek07() {
  awk '/^#### 1\.1\.([3-9]|1[0-2]) /{s=$2; f=1; next} /^#### /{f=0} f && /^\| (`0[2-8]|K-[0-9]|park A|dizin A)/{print s "\t" NR "\t" $0}' Docs/07_API_DESIGN.md \
    | awk -F'\t' 'BEGIN{srand(20261018)} {o=($1~/^1\.1\.[345]$/)?"1a":($1=="1.1.6")?"1b":($1~/^1\.1\.[78]$/)?"1c":($1=="1.1.9")?"1d":($1=="1.1.10")?"1e":"1f"; print rand() "\t" o "\t" $0}' \
    | sort -k2,2 -k1,1n | awk -F'\t' '{if (++n[$2]<=7) print $2 "\t" $5}' | cut -d'|' -f1-2
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
# 1b — 431 · 431 (§1.1.6)
mat07 6 | wc -l
comm -23 <(e1b | sort) <(mat07 6 | sort)                               # eksik — boş dönmeli
comm -13 <(e1b | sort) <(mat07 6 | sort)                               # fazla — boş dönmeli
mat07 '[3-6]' | sort | uniq -d                                         # yinelenen — 1a ve 1b birlikte; boş dönmeli
mat07 6 | grep -vE '^04 (E-(29|[3-5][0-9])|9\.|§9\.|§4\.2 )'            # §1.1.6 yalnız 04 §9 ve §4.2 — boş dönmeli
# 1c — 601 · 601 (§1.1.7 358 · §1.1.8 243)
mat07 '[78]' | wc -l; mat07 7 | wc -l; mat07 8 | wc -l
comm -23 <(e1c | sort) <(mat07 '[78]' | sort)                          # eksik — boş dönmeli
comm -13 <(e1c | sort) <(mat07 '[78]' | sort)                          # fazla — boş dönmeli
mat07 '[3-8]' | sort | uniq -d                                         # yinelenen — 1a, 1b ve 1c birlikte; boş dönmeli
mat07 7 | grep -vE '^04 (6\.|§6\.)'                                    # §1.1.7 yalnız 04 §6 — boş dönmeli
mat07 8 | grep -vE '^04 (7\.|§7\.|§8\.)'                              # §1.1.8 yalnız 04 §7 ve §8 — boş dönmeli
# 1d — 536 · 536 (§1.1.9)
mat07 9 | wc -l
comm -23 <(e1d | sort) <(mat07 9 | sort)                               # eksik — boş dönmeli
comm -13 <(e1d | sort) <(mat07 9 | sort)                               # fazla — boş dönmeli
mat07 '[3-9]' | sort | uniq -d                                         # yinelenen — 1a, 1b, 1c ve 1d birlikte; boş dönmeli
mat07 9 | grep -vE '^03 §[1-4]([. ]|$)'                                # §1.1.9 yalnız 03 §1–§4 — boş dönmeli
# 1e — 291 + 14 · 305 (§1.1.10; beklenen küme e1e 291, opt03'ten kimliğiyle giren bildirim satırı 14)
mat07 10 | wc -l; comm -12 <(opt03 | sort) <(mat07 10 | sort) | wc -l   # 305 · 14
comm -23 <(e1e | sort) <(mat07 10 | sort)                              # eksik — boş dönmeli
comm -13 <({ e1e; opt03; } | sort) <(mat07 10 | sort)                  # fazla — kümede ve isteğe bağlı kümede olmayan; boş dönmeli
mat07 '([3-9]|10)' | sort | uniq -d                                    # yinelenen — 1a…1e birlikte; boş dönmeli
mat07 10 | grep -vE '^03 §([5-9]|10)([. ]|$)'                          # §1.1.10 yalnız 03 §5–§10 — boş dönmeli
# 1f — 192 · 192 (§1.1.11 90 · §1.1.12 102)
mat07 '1[12]' | wc -l; mat07 11 | wc -l; mat07 12 | wc -l
comm -23 <(e1f | sort) <(mat07 '1[12]' | sort)                         # eksik — boş dönmeli
comm -13 <(e1f | sort) <(mat07 '1[12]' | sort)                         # fazla — boş dönmeli
mat07 '([3-9]|1[0-2])' | sort | uniq -d                                # yinelenen — 1a…1f birlikte; boş dönmeli
mat07 11 | grep -vE '^(K-[0-9]+|park A[0-9]-[0-9]+|dizin A[0-9]|0[2-6] §[0-9.]+ g[0-9]+|08 park A[0-9]-[0-9]+)$'   # §1.1.11 yalnız devirler — boş dönmeli
mat07 12 | grep -vE '^05 §'                                            # §1.1.12 yalnız 05 — boş dönmeli
deger07                                                                 # "Uç" değer kümesinin dışında — boş dönmeli
durum07                                                                 # "Durum" "Uç"tan türemiyor — boş dönmeli
comm -23 <(uc07 | grep -E '^UC-' | sort -u) <(aday07 | sort -u)         # listede olmayan aday — boş dönmeli
aday07 | sort | uniq -d                                                 # listede yinelenen kimlik — boş dönmeli
gap07                                                                   # satırların GAP adayı ile §1.1.13'ün karara bağlanmamış adayları — boş dönmeli (2. oturumdan sonra iki taraf da boş)
gapk07                                                                  # §1.1.13'ün karara bağlanan adayı ile §1.3'ün GAP satırı — boş dönmeli (AA-1 → GAP-14)
aday07 | wc -l; uc07 | grep -E '^UC-' | sort -u | wc -l                 # 187 · 187 — 1a…1f'nin satırlarının andığı aday (1d bildirim ucunu — UC-80 —, 1f canlılık ve durum ucunu — UC-81, UC-82 — andı; her aday en az bir ileri satır taşır)
# 2 — geri izlenebilirlik (§1.2; K-1160): 5.051 çift, 2.318 satır, 187 aday
geri07 | wc -l; uc07 | grep -cE '^UC-[0-9]+$'                            # 5051 · 5051 — çiftler "Uç" hücrelerinin aday değerleriyle aynı sayıda
geri07 | cut -f2 | sort -u | wc -l; geri07say | wc -l                    # 2318 · 187 — en az bir aday taşıyan satır; çifti olan aday
comm -3 <(aday07 | sort -u) <(geri07 | cut -f1 | sort -u)                # listede olup çifti olmayan (gerekçesiz ekleme) ya da çifti olup listede olmayan — boş dönmeli
geri07ek                                                                 # makine ucu olmayıp ekran ya da akış satırı taşımayan aday — boş dönmeli
diff <(geri07say | cut -f1,2) <(tab07 | cut -f1,2)                       # §1.2'nin satır sayıları betikle aynı — boş dönmeli
tab07 | cut -f3 | sort | uniq -c                                         # 181 beslenir · 6 beslenir — makine ucu
geri07 | cut -f3 | sort | uniq -c                                        # 03 1575 · 04 2954 · 05 244 · devir 278
ornek07 | wc -l                                                          # 42 — örneklemin satırları (oturum ve Kaynak ID)
mat07 '[3-5]' | wc -l; awk -F'|' '/^#### 1\.1\.[3-5] /{f=1; next} /^#### /{f=0} f && /^\| `0/{d=$5; gsub(/^ +| +$/,"",d); sub(/ \(AA-[0-9]+\)/,"",d); c[d]++} END{for(k in c) print k, c[k]}' Docs/07_API_DESIGN.md | sort   # durum dağılımı
mat07 6 | wc -l; awk -F'|' '/^#### 1\.1\.6 /{f=1; next} /^#### /{f=0} f && /^\| `0/{d=$5; gsub(/^ +| +$/,"",d); sub(/ \(AA-[0-9]+\)/,"",d); c[d]++} END{for(k in c) print k, c[k]}' Docs/07_API_DESIGN.md | sort   # durum dağılımı — 1b
mat07 '[78]' | wc -l; awk -F'|' '/^#### 1\.1\.[78] /{f=1; next} /^#### /{f=0} f && /^\| `0/{d=$5; gsub(/^ +| +$/,"",d); sub(/ \(AA-[0-9]+\)/,"",d); c[d]++} END{for(k in c) print k, c[k]}' Docs/07_API_DESIGN.md | sort   # durum dağılımı — 1c
mat07 9 | wc -l; awk -F'|' '/^#### 1\.1\.9 /{f=1; next} /^#### /{f=0} f && /^\| `0/{d=$5; gsub(/^ +| +$/,"",d); sub(/ \(AA-[0-9]+\)/,"",d); c[d]++} END{for(k in c) print k, c[k]}' Docs/07_API_DESIGN.md | sort   # durum dağılımı — 1d (eşlendi 477, konvansiyon 18, uç doğurmaz 41)
mat07 10 | wc -l; awk -F'|' '/^#### 1\.1\.10 /{f=1; next} /^#### /{f=0} f && /^\| `0/{d=$5; gsub(/^ +| +$/,"",d); sub(/ \(AA-[0-9]+\)/,"",d); c[d]++} END{for(k in c) print k, c[k]}' Docs/07_API_DESIGN.md | sort   # durum dağılımı — 1e (eşlendi 254, konvansiyon 11, uç doğurmaz 40)
mat07 '1[12]' | wc -l; awk -F'|' '/^#### 1\.1\.1[12] /{f=1; next} /^#### /{f=0} f && /^\| (`0|K-[0-9]|park A|dizin A)/{d=$5; gsub(/^ +| +$/,"",d); sub(/ \(AA-[0-9]+\)/,"",d); c[d]++} END{for(k in c) print k, c[k]}' Docs/07_API_DESIGN.md | sort   # durum dağılımı — 1f (eşlendi 147, konvansiyon 41, uç doğurmaz 4; §1.1.11 69 · 20 · 1, §1.1.12 78 · 21 · 3)
```

**Yanlış pozitif ve kapsam denetimi (1a).** `exp04()` Arayüz Tanımları'nın §2–§9'unu satır satır sınıflar: `### 5.n E-n` ve `### 9.n E-n` başlığı ekran birimidir; satır başındaki ya da madde imli kalın `n.m.k` — dört düzeyli dahil — maddedir ve sonraki satırın bağlamını değiştirmez; ilk hücresi `n.m.k` olan tablo satırı birimdir, öteki tablo satırları (başlık, ayraç, §2'nin giriş tablosu, §4.1–§4.3'ün tabloları, §5–§9'un form ve kural tabloları) birim değildir — `04 §6` ve §7'nin tablo satırlarının hepsi `n.m.k.l` kimliği taşır (betikle sayıldı). §5 ve §9'un beş alan başlığı ("Bilgi hiyerarşisi", "Aksiyonlar", "Validasyonlar", "Durum × rol varyantları", "Boş / yükleniyor / hata durumları") başlıktan sonra içerik taşıyorsa kimliksiz maddedir, yalnız parantezli bir işaret taşıyorsa değildir; dört boşlukla girintili madde imli satır kimliksiz maddedir (5.13.9'un (a)–(h)'si, §9'un alt maddeleri), madde imsiz girintili satır önceki maddenin devamıdır. §2'nin birimi bileşendir — başlığındaki `(OB-n)`; `### 2.12 Form bileşenleri` başlığı OB taşımaz ve altındaki `####` başlıkları taşır. §8'in birimi `### 8.n` başlığıdır. §4'te yalnız §4.2'nin metni ve §4.4 birim üretir. Kimlik biçimi `mat07()`'nin okuduğu biçimdir: ters tırnaklar silinir, boşluklar teke iner — `` `04` 5.13.6 `` → `04 5.13.6`, `` `04 §5.13` m2 `` → `04 §5.13 m2`. `exp03()` 06'nın işlevinin (`06 §1.1.1`) bütün bölümlere uygulanmış hâlidir — `d` değişkeni §7 dışındaki her bölümde satır düzeyini açar; §7 `###` alt bölüm başına bir birim ve bölüm metniyle girer. `dev07()` açılışın sayım komutunun işlev biçimidir (karar kaydı §16.2, betik 1'in ilk kolu); üst sınır K-1143'tür — Aşama 6'nın kararları devir değildir. `park07()` `06`'nın `park()`'ının kalıbıdır ve dizin satırlarını `dizin An` kimliğiyle ayrıca yazar (K-1150 dizin satırlarını kaynak satırı sayar). `gov07()` `06`'nın `gov()`'unun kalıbıdır: arama "`` `07` ``" ya da "API Tasarım"dır; Arayüz Tanımları'nın sınırı `## 2.`'dir, Veri Modeli'ninki `## 2.`'dir — iki dokümanın §1'i kendi matrisidir ve satırları devir değildir. `exp05()` 13 bölümün `###` başlığıyla sınırlıdır; `05 §2.1`'in MK-04 satırı ilk eşleşmedir (§12'nin özet tablosundaki ikinci MK-04 satırı birim değildir). **Betik iki yönlü sınandı** (`audit` skill'i, "Koşum biçimi" 4. madde; 2026-10-18): belgenin bir kopyasında §1.1.3'ten bir satır çıkarıldığında eksik, bir satır ikilendiğinde yinelenen, kaynakta olmayan bir kimlik (`04 5.4.99`) eklendiğinde fazla, §1.1.4'e bir `04 §5` satırı taşındığında başlığa bağlılık denetimi, "Uç" hücresine kümenin dışında bir değer yazıldığında değer kümesi denetimi, bir satırın "Durum"u "Uç"una aykırı yazıldığında "Durum" denetimi, listede olmayan bir aday (`UC-999`) yazıldığında aday listesi denetimi ve bir satırın GAP adayı kimliği §1.1.13'ten silindiğinde GAP adayı denetimi o satırı döndürdü.

**Okuma derinliği (checklist §2).** 1a kaynak satırlarını **satırın tamamıyla** okudu: Arayüz Tanımları'nın §2'si (satır 2337–2705), §3'ü (2706–2916), §4'ü (2917–3027; §4.1'in ekran satırları ve §4.2'nin metni dahil) ve §5'i (3028–3610) hiçbir satırı kesmeden — bu aralıkta en uzun satır 1.610 karakterdir; hücre başına kesilerek okunan satır yoktur, checklist §2'nin kesilmiş okuma örneklemi 1a'nın satırlarına uygulanmaz. Bir satırın uca dokunan ayrıntısı başka bir dokümanın kuralına işaret ettiğinde kural kaynağında okundu: ödeme adımında yazılanların çerezi (`05 §6.7.9`, §6.7.7 — 5.13.5, 5.13.19, 3.6.2), görsellerin ve dijital dosyanın sunuluşu (`05 §7.7.1`–§7.7.4 — 5.16.13, 4.4.6), `08`'in park satırı (K-906 · K-973 — 3.5.8). Öteki ailelerin gövdesi bu oturumda okunmadı; envanter onları betikle saydı — tam okuma kendi oturumlarındadır (§1 alt bölüm haritası).

**Kapsam denetimi ve okuma derinliği (1b).** 1b `e1b()` ile `mat07 6`'yı karşılaştırır; betik ailesi değişmedi — `exp04()`, `e1a()`…`e1f()` ve öteki işlevler 1a'nın hâliyle koşar ve 1a'nın kümesi (583) aynı komutla aynı çıkar: `e1a` ile `mat07 '[3-5]'`'in karşılaştırması 1b'nin satırları eklendikten sonra da boş döner. Yeni olan dört satırdır — 1b'nin eksik, fazla ve başlığa bağlılık denetimleri ve yinelenen denetiminin 1a ile 1b'yi birlikte okuması (`mat07 '[3-6]'`). **Betik iki yönlü sınandı** (2026-10-18): belgenin bir kopyasında §1.1.6'dan bir satır çıkarıldığında eksik, bir satır ikilendiğinde yinelenen, kaynakta olmayan bir kimlik (`04 9.9.99`) eklendiğinde fazla, §1.1.6'ya bir `04 §5` satırı taşındığında başlığa bağlılık, §1.1.6'da "Uç" hücresine kümenin dışında bir değer yazıldığında değer kümesi, bir satırın "Durum"u "Uç"una aykırı yazıldığında "Durum" ve listede olmayan bir aday (`UC-999`) yazıldığında aday listesi denetimi o satırı döndürdü. **Okuma derinliği:** 1b Arayüz Tanımları'nın §4.2'sini (satır 2954–2985) ve §9'unu (4698–5276) hiçbir satırı kesmeden okudu — bu aralıkta en uzun satır 3.085 karakterdir; hücre başına kesilerek okunan satır yoktur. Bir satırın uca dokunan ayrıntısı başka bir dokümanın kuralına işaret ettiğinde kural kaynağında okundu: panelden e-postanın yeniden gönderimi (`05 §4.4.3` — 9.9.33: yeniden gönderim iş kuyruğuna yeni bir satırdır, sonucu sayfa yeniden açılınca görünür; API7-01), siparişin sürümünü artıran yazımlar (`06 §2.6.4` — 9.9.21, 9.9.34: müşterinin IBAN girişi sürümü artırır), ayarların bölüm sürümleri ve geçici kapatma anahtarı (`06 §2.6.2`, §2.6.3 — 9.14.8, 9.16.9, 9.17.9, 9.18.11). 1b yeni GAP adayı bulmadı: işlemlerin hepsinin cevabı kaynakta yazılıdır ve uçlar §1.1.2'nin adaylarıdır.

**Kapsam denetimi ve okuma derinliği (1c).** 1c `e1c()` ile `mat07 '[78]'`'i karşılaştırır; betik ailesi değişmedi — `exp04()`, `e1a()`…`e1f()` ve öteki işlevler 1a'nın hâliyle koşar. Önceki oturumların kümeleri sessizce değişmedi: `e1a()`…`e1f()`, `mat07 '[3-5]'`, `mat07 6` ve `aday07()`'nin çıktısı 1c'den önce alındı ve 1c'den sonra `cmp` ile aynı çıktı; `uc07()`'nin ilk 2.286 değeri — 1a ve 1b'nin satırlarının "Uç" değerleri — de `cmp` ile aynıdır, 1c'nin değerleri listenin sonuna eklenir. Yeni olan altı satırdır — 1c'nin eksik, fazla ve iki başlığa bağlılık denetimi ve yinelenen denetiminin 1a, 1b ve 1c'yi birlikte okuması (`mat07 '[3-8]'`). **Betik iki yönlü sınandı** (2026-10-18): belgenin bir kopyasında §1.1.7'den bir satır çıkarıldığında eksik, bir satır ikilendiğinde yinelenen, kaynakta olmayan bir kimlik (`04 6.2.99.1`) eklendiğinde fazla, §1.1.7'ye bir `04 §7` satırı taşındığında başlığa bağlılık, §1.1.8'de "Uç" hücresine kümenin dışında bir değer yazıldığında değer kümesi, bir satırın "Durum"u "Uç"una aykırı yazıldığında "Durum" ve listede olmayan bir aday (`UC-999`) yazıldığında aday listesi denetimi o satırı döndürdü. **Okuma derinliği:** 1c Arayüz Tanımları'nın §6'sını (satır 3611–4207), §7'sini (4208–4642) ve §8'ini (4643–4697) hiçbir satırı kesmeden okudu — bu aralıkta en uzun satır 1.083 karakterdir; hücre başına kesilerek okunan satır yoktur. Bir satırın uca dokunan ayrıntısı başka bir dokümanın kuralına işaret ettiğinde kural kaynağında okundu: oturumu ödeme adımında kapanan üyenin yazdıklarının korunması (`05 §6.7.9` — 6.2.13.13: form oturumun yenilenmesi için ayrılışta da çereze yazılır, cevap kaynakta yazılıdır; AA-1 "Sepete dön"le ayrılışın adayıdır, bu satırı kapsamaz). 1c yeni GAP adayı bulmadı: durum × rol satırlarının işlemleri ve formların alanları 1a ve 1b'nin adaylarındadır, satırların kuralı kaynakta yazılıdır.

**Kapsam denetimi ve okuma derinliği (1d).** 1d `e1d()` ile `mat07 9`'u karşılaştırır; betik ailesi değişmedi — `exp03()`, `e1a()`…`e1f()` ve öteki işlevler 1a'nın hâliyle koşar. Önceki oturumların kümeleri sessizce değişmedi: `e1a()`…`e1f()`, `mat07 '[3-5]'`, `mat07 6`, `mat07 '[78]'` ve `aday07()`'nin çıktısı 1d'den önce alındı ve 1d'den sonra `cmp` ile aynı çıktı; `uc07()`'nin ilk 3.785 değeri — 1a, 1b ve 1c'nin satırlarının "Uç" değerleri — de `cmp` ile aynıdır, 1d'nin 1.410 değeri listenin sonuna eklenir. Yeni olan beş satırdır — 1d'nin eksik, fazla ve başlığa bağlılık denetimi, yinelenen denetiminin 1a…1d'yi birlikte okuması (`mat07 '[3-9]'`) ve durum dağılımı. **Betik iki yönlü sınandı** (2026-10-18): belgenin bir kopyasında §1.1.9'dan bir satır (`03 §1.2` S5) çıkarıldığında eksik, satır ikilendiğinde yinelenen, kaynakta olmayan bir kimlik (`03 §1.2` S99) yazıldığında fazla, §1.1.9'a bir `04 §5` satırı taşındığında başlığa bağlılık, §1.1.9'da "Uç" hücresine kümenin dışında bir değer yazıldığında değer kümesi, bir satırın "Durum"u "Uç"una aykırı yazıldığında "Durum", listede olmayan bir aday (`UC-999`) yazıldığında aday listesi ve §1.1.13'te olmayan bir GAP adayı (`AA-9`) yazıldığında GAP adayı denetimi o satırı döndürdü. **Okuma derinliği:** 1d Kullanıcı Akışları'nın §1'ini (satır 146–453), §2'sini (454–706), §3'ünü (707–914) ve §4'ünü (915–1002) hiçbir satırı kesmeden okudu — bu aralıkta en uzun satır 1.835 karakterdir; hücre başına kesilerek okunan satır yoktur. 1d yeni GAP adayı bulmadı: akışların andığı işlemlerin hepsi §1.1.2'nin adaylarındadır — vitrinin işlemleri 1a'nın, panelin işlemleri 1b'nin listesinde —, satırların kuralı kaynakta ya da gösterdikleri `02` maddesinde yazılıdır; satırların andığı 146 aday arasında 1a'dan beri 1f'yi bekleyen bildirim ucu (UC-80) ilk kez kaynak satırı aldı.

**Kapsam denetimi ve okuma derinliği (1e).** 1e `e1e()` ile `mat07 10`'u karşılaştırır; beklenen küme 291'dir ve `03 §7`'nin bağlantı taşıyan 14 bildirim satırı isteğe bağlı kümeden (`opt03()`) kimliğiyle girer (K-1150, K-1158) — fazla denetimi bu yüzden iki kümenin birleşimine karşı koşar. Betik ailesi değişmedi — `exp03()`, `opt03()`, `e1a()`…`e1f()` ve öteki işlevler 1a'nın hâliyle koşar. Önceki oturumların kümeleri sessizce değişmedi: `e1a()`…`e1f()`, `mat07 '[3-5]'`, `mat07 6`, `mat07 '[78]'`, `mat07 9` ve `aday07()`'nin çıktısı 1e'den önce alındı ve 1e'den sonra `cmp` ile aynı çıktı; `uc07()`'nin ilk 5.195 değeri — 1a…1d'nin satırlarının "Uç" değerleri — de `cmp` ile aynıdır, 1e'nin 840 değeri listenin sonuna eklenir. Yeni olan altı satırdır — 1e'nin satır ve isteğe bağlı küme sayımı, eksik, fazla ve başlığa bağlılık denetimi, yinelenen denetiminin 1a…1e'yi birlikte okuması (`mat07 '([3-9]|10)'`) ve durum dağılımı. **Betik iki yönlü sınandı** (2026-10-18): belgenin bir kopyasında §1.1.10'dan bir satır (`03 §8.2.5`) çıkarıldığında eksik, satır ikilendiğinde yinelenen, kaynakta olmayan bir kimlik (`03 §8.2.99`) yazıldığında fazla, `opt03()`'te olmayan bir bildirim kimliği (`03 §7.1.99`) eklendiğinde fazla, §1.1.10'a bir `04 §5` satırı taşındığında başlığa bağlılık, "Uç" hücresine kümenin dışında bir değer yazıldığında değer kümesi, bir satırın "Durum"u "Uç"una aykırı yazıldığında "Durum", listede olmayan bir aday (`UC-999`) yazıldığında aday listesi ve §1.1.13'te olmayan bir GAP adayı (`AA-9`) yazıldığında GAP adayı denetimi o satırı döndürdü. **Okuma derinliği:** 1e Kullanıcı Akışları'nın §5'ini (satır 1003–1078), §6'sını (1079–1207), §7'sini (1208–1379), §8'ini (1380–1613), §9'unu (1614–1685) ve §10'unu (1686–1763) hiçbir satırı kesmeden okudu — bu aralıkta en uzun satır 1.940 karakterdir; hücre başına kesilerek okunan satır yoktur; §7'nin 131 bildirim satırı toplu eşlenmesine karşın tek tek okundu (K-1150). Bir satırın uca dokunan ayrıntısı başka bir dokümanın maddesine işaret ettiğinde madde kaynağında okundu: duyurunun panelde kurumsal içeriğin bir kaydı olarak yönetilmesi (`04` 9.13.3, 9.13.7 — 6.3.3.2, 8.6.3.1, 10.2.2: duyuru E-41'in içerik kaydıdır, UC-158 ve UC-159). 1e yeni GAP adayı bulmadı: akışların andığı her işlem §1.1.2'nin adaylarındadır, satırların kuralı kaynakta ya da gösterdikleri `02` maddesinde yazılıdır; satırlar 166 adayı anar, aday eklenmedi — liste 187'de kalır.

**Kapsam denetimi ve okuma derinliği (1f).** 1f `e1f()` ile `mat07 '1[12]'`'yi karşılaştırır; beklenen küme 192'dir — devirler (`e1f_dev()`, 90) ve Teknik Mimari (`exp05()`, 102). Karar satırları, park ve dizin satırları kimliklerini ters tırnaksız taşır (`K-947`, `park A4-5`, `dizin A4`), gövde cümleleri ve öteki dokümanların satırları önekleriyle (`` `05 §6.3.5` g1 ``, `` `08` park A4-3 ``); `mat07()`'nin süzgeci ikisini de okur. Betik ailesi değişmedi — `dev07()`, `park07()`, `gov07()`, `parkx07()`, `exp05()`, `e1a()`…`e1f()` ve öteki işlevler 1a'nın hâliyle koşar. Önceki oturumların kümeleri sessizce değişmedi: `e1a()`…`e1f()`, `mat07 '[3-5]'`, `mat07 6`, `mat07 '[78]'`, `mat07 9`, `mat07 10` ve `aday07()`'nin çıktısı 1f'den önce alındı ve 1f'den sonra `cmp` ile aynı çıktı; `uc07()`'nin ilk 6.035 değeri — 1a…1e'nin satırlarının "Uç" değerleri — de `cmp` ile aynıdır, 1f'nin 826 değeri listenin sonuna eklenir. Yeni olan yedi satırdır — 1f'nin satır sayımı, eksik, fazla, yinelenen denetiminin 1a…1f'yi birlikte okuması (`mat07 '([3-9]|1[0-2])'`), iki başlığa bağlılık denetimi ve durum dağılımı; aday sayımının yorumu güncellendi (187 · 187). **Betik iki yönlü sınandı** (2026-10-18): belgenin bir kopyasında §1.1.11'den bir satır (`K-947`) çıkarıldığında eksik, §1.1.12'nin bir satırı (`05 §9.4.13`) ikilendiğinde yinelenen, kaynakta olmayan bir kimlik (`K-9999`) yazıldığında fazla, §1.1.11'e bir `05` satırı (`05 §9.4.99`) eklendiğinde başlığa bağlılık ve fazla, bir satırın "Durum"u "Uç"una aykırı yazıldığında "Durum", "Uç" hücresine kümenin dışında bir değer yazıldığında değer kümesi, listede olmayan bir aday (`UC-999`) yazıldığında aday listesi ve §1.1.13'te olmayan bir GAP adayı (`AA-9`) yazıldığında GAP adayı denetimi o satırı döndürdü. **Okuma derinliği:** 1f devirlerin 51 karar satırını karar kaydından betikle çıkarıp satırın tamamıyla — konu, karar, gerekçe, etki — okudu (en uzun satır 6.768 karakter); `07`'nin beş park bloğunu, 20 gövde cümlesini (`gov07()`'nin bulduğu satırın tamamı) ve `08`'in park satırını, Teknik Mimari'nin on üç bölümünü (`exp05()`'in sınırları içinde, bölümlerin tablo başlıkları ve `*Kaynak:` satırları dahil) hiçbir satırı kesmeden okudu; hücre başına kesilerek okunan satır yoktur. Bir satırın uca dokunan ayrıntısı başka bir maddeye işaret ettiğinde madde kaynağında okundu: ödeme adımında yazılanların çerezi (`05 §6.7.9` — AA-1'in dayanağı; satır yeni bir aday doğurmaz, AA-1'in kararı 2. oturumdadır). 1f yeni GAP adayı bulmadı: devirlerin ve Teknik Mimari'nin andığı her uç §1.1.2'dedir — makine uçları (UC-37, UC-79, UC-80…UC-83) yeni kaynak satırlarını, canlılık ve durum ucu (UC-81, UC-82) ilk kaynak satırını burada alır —, işlerin her biri §16.3'ün bir konusuna düşer; aday eklenmedi — liste 187'de kalır, UC-188 açılmadı ve listenin her adayı en az bir ileri satır taşır.

**Kapsam denetimi, geri izlenebilirlik ve okuma derinliği (2).** 2. oturum yeni küme açmadı; betik bloğuna altı işlev — `geri07()` (aday–satır çiftleri), `geri07say()` (aday başına sayı ve aile dağılımı), `geri07ek()` (makine ucu olmayıp ekran ya da akış satırı taşımayan aday), `tab07()` (§1.2'nin tablosu), `gapk07()` (karara bağlanan adayın §1.3'teki satırı), `ornek07()` (örneklemin seçimi) — ve on denetim ve sayım satırı ekledi; `gap07()` karara bağlanmamış adaylara daraldı (karara bağlanan adayın son hücresi "Karar: GAP-n" taşır). §1.2'nin tablosu `geri07()`'den üretildi; satır sayıları `tab07()` ile betiğe karşı denetlenir. **GAP kararının ve örneklemin düzeltmesinin etkisi:** beş ileri satırın "Uç" hücresi değişti — `04` 5.13.19 (GAP adayı → eşlendi; UC-26), 3.1.10, 3.3.15 ve 6.2.13.12 (UC-26), `05 §6.4.6` (UC-175, UC-176, UC-177; örneklem) — ve `05 §6.7.9`'un özeti; `uc07()` 6.861 → 6.868 değer, farkı yalnız bu yedi değerdir (`diff`); 1a'nın durum dağılımı eşlendi 502, konvansiyon 8, uç doğurmaz 73, GAP adayı yok. **Geri besleme kümeyi değiştirmedi** (checklist §2): `exp04()`, `exp03()`, `opt03()`, `e1f_dev()`, `gov07()`, `exp05()`, `e1a()`…`e1f()`, `mat07()`'nin altı süzgeci ve `aday07()` geri beslemeden önce ve sonra dosyaya yazılıp `cmp` ile karşılaştırıldı — satır satır aynıdır: Teknik Mimari'nin 6.7.9'u ve Ürün Gereksinimleri'nin §12.2.5'i var olan birimlerin metnidir, Doğrulama Protokolü'nün park satırı matrisin ailesi değildir ve geri beslenen metin hiçbir gövdede bu dokümanı anan yeni cümle açmadı (anma yalnız başlık notlarında ve dipnotlarda). Kapsama denetimlerinin hepsi boştur. **Betik iki yönlü sınandı** (2026-10-18): belgenin bir kopyasında listeye kaynak satırı olmayan bir aday (`UC-188`) eklendiğinde gerekçesiz ekleme denetimi, §1.2'de bir adayın satır sayısı bozulduğunda tablo denetimi, `04` 5.13.19'un durumu yeniden "GAP adayı (AA-1)" yazıldığında `gap07()`, §1.3'ün GAP-14 satırından "(aday AA-1)" silindiğinde `gapk07()` ve makine ucu olmayan bir adayın (UC-109) ekran satırları başka adaya yazıldığında `geri07ek()` o adayı döndürdü. **Okuma derinliği:** 2. oturum AA-1'in dayandığı yerleri tam okudu — `04` 5.13.19, 5.13.5, 3.1.10, 3.3.15, 2.2.7, §3.6.2; `05 §6.7.7`, §6.7.9, §6.7.11; `02 §12.2.5`, §3.34.5; K-757, K-758, K-907'nin satırları; `12`'nin park satırı (K-904 · K-907 · K-908 · K-971) — ve geri izlenebilirliği betikle kurdu; örneklemin 42 satırını ilk doğrulayıcı kaynağın tam metnine karşı okudu (§1.2).

#### 1.1.2 Aday uç listesi ve "Uç" sütununun değer kümesi

Liste nihai envanter değildir (K-1153): uç envanteri `07 §3`'te, GAP kararlarından sonra yazım turunda doğar (şablonun başlık notu; API3-01) ve bir adayı bölebilir, birleştirebilir ya da bir işleyiciye çevirebilir; kimlik envantere geçer ve adres değişse de kalır (K-1148). Birim adres + yöntem + (varsa) sayfa işleyicisidir (K-1149): ekranın GET'i bir aday, ekranın her POST işlemi ayrı bir aday, makine ucu kendi adayıdır. Adresin biçimi (API2-01), işlemlerin aynı sayfanın adlandırılmış işleyicileri mi ayrı adresler mi olduğu (API2-02) ve oturum ve erişim (API2-03) workshop'un kararıdır; listenin "Not" sütunu yalnız adayın dayandığı kaynağı gösterir. 1a vitrinin bütün adaylarını, bilinen makine uçlarını (K-868; kaynak satırları 1f'de) ve panel ekranlarının GET adaylarını (§3'ün geçişleri onları hedef aldığı için) açtı; 1b panelin POST işlemlerini, görsel yüklemeyi ve dışa aktarmanın dosyalarını listenin sonuna ekledi — UC-110…UC-187 (K-1154). 1c aday eklemedi: durum × rol satırlarının, formların ve ortak alan kurallarının andığı her işlem ve her sorgu listededir (K-1156). 1d de eklemedi: Kullanıcı Akışları'nın `03 §1`–§4'ünün andığı her müşteri ve yönetici işlemi, her sayfa ve ödeme sağlayıcısının bildirimi listededir; geçişlerin ve sürelerin geri kalanı sistemin kendi işidir (K-1157). 1e de eklemedi: `03 §5`–§10'un andığı her işlem ve sayfa ve `03 §7`'nin bağlantılı bildirimlerinin indiği her uç listededir; kurulum tarafının ve ürünün dışındaki adımlar uç doğurmaz (K-1158). 1f de eklemedi: devirlerin ve Teknik Mimari'nin andığı her uç — makine uçlarının hepsi dahil — listededir; devirlerin işi uca ve karara bağlanacağı konuya düşer (K-1159). **2. oturum listeyi geri yönde kapattı (K-1160):** 187 adayın hepsi ileri satırlardan beslenir, gerekçesiz ekleme yoktur ve liste uç envanterine (§3) 187 adayla devredilir; değişen tek aday UC-26'dır — GAP-14 "Sepete dön"ü onun ikinci hedefi yaptı (K-1161). Panelin GET adayları 1a'nın biriminde kalır: UC-84 E-29'un üç adımının, UC-108 E-54'ün üç adımının GET'idir — 1a'nın satırları sıfırlama, doğrulama ve geri alma bağlantılarını onlara eşledi (3.5.5–3.5.7); adımların ayrı işleyicilere bölünmesi API2-02'nin işidir. Numara iki basamakla başlar, 100'den sonra üç basamaklıdır.

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
| UC-26 | Ödeme adımından girişe ya da sepete — formu çereze yazıp yönlendirme | POST | E-13 | Müşteri | `05 §6.7.9`; alanlar doğrulanmaz; "Sepete dön" de (GAP-14, K-1161); bir ya da iki işleyici API2-02 |
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
| UC-108 | Yönetici hesabının doğrulama ekranları | GET | E-54 | Yönetici · Kullanıcı | Üç adımın GET'i — 1b bölmedi, POST'lar UC-185…UC-187 (K-1154) |
| UC-109 | Yönetici çıkışı | POST | panel çerçevesi (OB-02) | Yönetici | §3.6.4 |
| UC-110 | Panel girişi | POST | E-29 | Yönetici | L-1; "beni hatırla"; tanınan tarayıcı; ayrı kimlik şeması (K-588, K-899) |
| UC-111 | Panel şifre sıfırlama isteği | POST | E-29 | Kullanıcı | L-2; nötr metin; yalnız yönetici hesapları |
| UC-112 | Panelde yeni şifrenin kurulması | POST | E-29 | Kullanıcı | Bağlantı tek kullanımlık (Z-2); oturumlar kapanır, oturum açmaz |
| UC-113 | Davetle yönetici hesabını açma | POST | E-30 | Kullanıcı | Davetin geçerliliği sunucuda (K-1085); oturum açmaz (K-807) |
| UC-114 | Ürünü kaydetme | POST | E-33 | Yönetici | Yeni ürün Taslak doğar; ürünün sürümü (OB-10); stok ve kontenjan değere karşı (K-980) |
| UC-115 | Ürünün ya da varyantın yayın geçişi | POST | E-33 | Yönetici | Yayına al · taslağa al · arşive al; yayın kapısı sunucuda |
| UC-116 | Ürünü ya da varyantı kalıcı silme | POST | E-33 | Yönetici | Onay penceresi; son Yayında varyant silinmez |
| UC-117 | Görsel yükleme | POST | E-33, E-41, E-43 (OB-19) | Yönetici | Gövde tavanı P-23 (K-902); yükleme kayda form kaydında bağlanır (K-1100); biçimi API4-07 |
| UC-118 | Kategori ekleme | POST | E-34 | Yönetici | Birinci seviye ya da alt kategori; P-20 |
| UC-119 | Kategoriyi yeniden adlandırma | POST | E-34 | Yönetici | Sürüm taşımaz (`06 §2.6.5`) |
| UC-120 | Kategoriyi taşıma | POST | E-34 | Yönetici | Üç seviye ve kendi alt dalı sunucuda reddedilir |
| UC-121 | Kategoriyi silme | POST | E-34 | Yönetici | İçi doluysa engel; onay yok |
| UC-122 | Kuponu kaydetme | POST | E-35 | Yönetici | Yeni ve var olan kupon; sürüm (OB-10); kullanım adedinin alt sınırı (K-654) |
| UC-123 | Havale siparişini "Ödendi" işaretleme | POST | E-37 | Yönetici | Onay; Ö1; siparişin sürümü (K-874) — E-37'nin bütün işlemlerinde |
| UC-124 | Kargoya verme | POST | E-37 | Yönetici | Kargo şirketi ve takip numarası ya da araç beyanı; onay; S5 |
| UC-125 | Kargoya yeniden verme | POST | E-37 | Yönetici | Teslim edilemedi siparişi; onay; S8 |
| UC-126 | Teslim işareti ve tarihi | POST | E-37 | Yönetici | Olay tarihi, sıra sorusu (K-608); S6 |
| UC-127 | "Teslim edilemedi" işareti | POST | E-37 | Yönetici | S7, gerekirse S11 |
| UC-128 | Teslim edilemedi siparişini kapatma | POST | E-37 | Yönetici | Sebep ve geri ödeme yok (K-592, K-717) |
| UC-129 | Firma iptali | POST | E-37 | Yönetici | Kapsam, kalem başına adet (K-794), sebep (OB-15); onay |
| UC-130 | Başka kanaldan gelen cayma ya da gecikme feshi bildiriminin kaydı | POST | E-37 | Yönetici | Kalem ve adet (cayma), bildirimin tarihi, aktarılan IBAN; onay (K-715) |
| UC-131 | Geri ödemeyi işleme | POST | E-37 | Yönetici | Kalemin ya da siparişin bekleyen geri ödemesi; onay; pencere açıkken IBAN düzeltildiyse uygulanmaz |
| UC-132 | "Havale gerçekleşmedi" | POST | E-37 | Yönetici | IBAN silinir, "IBAN bekleniyor"a döner (K-1131) |
| UC-133 | Tutar bazlı kısmi geri ödeme | POST | E-37 | Yönetici | Tutar, isteğe bağlı kalem ve kapanış (K-1105); onay |
| UC-134 | IBAN isteme | POST | E-37 | Yönetici | Kalem ve sebep; B-14 |
| UC-135 | Kart iadesini yeniden deneme | POST | E-37 | Yönetici | "Geri ödeme gerçekleşmedi"de; onay |
| UC-136 | Havale yolunu açma | POST | E-37 | Yönetici | IBAN isteği ve B-14 |
| UC-137 | Sonucu beklenen kart iadesini "gerçekleşmedi" işaretleme | POST | E-37 | Yönetici | Sorgu durur (K-940) |
| UC-138 | Kalem çıkarma ya da adet azaltma | POST | E-37 | Yönetici | Adet (K-794); kart hattında onay |
| UC-139 | Hizmet kalemini tamamlama | POST | E-37 | Yönetici | Adet seçilmez (K-792) |
| UC-140 | İade malını teslim alma ya da reddetme | POST | E-37 | Yönetici | Ulaşma tarihi ve adet; ret onay ister (K-656) |
| UC-141 | İade malını stoğa ekleme | POST | E-37 | Yönetici | Teslim alınan adet kadar (K-791) |
| UC-142 | "Mal dönmedi" kapanışı | POST | E-37 | Yönetici | Adet (K-794); geri ödeme yok (K-596) |
| UC-143 | İndirme hakkını yenileme | POST | E-37 | Yönetici | Sayaç sıfırlanır (K-683) |
| UC-144 | Ayıp talebini çözüldü işaretleme | POST | E-37 | Yönetici | Bildirim yok |
| UC-145 | Ayıp talebini yeniden açma — panel | POST | E-37 | Yönetici | Z-18 içinde |
| UC-146 | Adres düzeltmesi | POST | E-37 | Yönetici | OB-13'ün panel varyantı; adres geçmişi (K-1008) |
| UC-147 | Kargo şirketi ve takip numarası düzeltmesi | POST | E-37 | Yönetici | B-12 |
| UC-148 | Teslim tarihi düzeltmesi | POST | E-37 | Yönetici | Sıra sorusu (K-608) |
| UC-149 | Ulaşma tarihi düzeltmesi | POST | E-37 | Yönetici | Teslim alma kaydının satırında (K-712) |
| UC-150 | Durum geçişi düzeltmesi | POST | E-37 | Yönetici | Sebep (OB-15); engel (K-703) |
| UC-151 | Misafir siparişinin e-postasını düzeltme | POST | E-37 | Yönetici | Yeni erişim anahtarı; B-1, B-15 (K-585, K-586) |
| UC-152 | İç notu kaydetme | POST | E-37 | Yönetici | Değere karşı (K-1051); kişisel veri imha edilmişse kapalı (K-1095) |
| UC-153 | Ulaşmayan e-postayı yeniden gönderme | POST | E-37 | Yönetici | Satır başına; onay yok (K-759); satır kilidi (`05 §4.4.3`) |
| UC-154 | Ulaşmayan bütün e-postaları yeniden gönderme | POST | E-37 | Yönetici | Siparişin içinde; siparişler arası yok (K-761) |
| UC-155 | İletişim talebini kapatma | POST | E-39 | Yönetici | Saklama süresi son kapatılıştan (Z-33) |
| UC-156 | İletişim talebini yeniden açma | POST | E-39 | Yönetici | Süre durmaz (K-1099) |
| UC-157 | Üye hesabını silme — panel | POST | E-40 | Yönetici | Onay; geri alma bağlantısı açıkken kapalı (K-698) |
| UC-158 | İçerik kaydını kaydetme | POST | E-41 | Yönetici | Yedi tip; yeni kayıt Taslak doğar; sürüm (OB-10) |
| UC-159 | İçerik kaydının yayın geçişi | POST | E-41 | Yönetici | Yayına al · taslağa al; yayın kapısı (K-584) |
| UC-160 | İçerik kaydını kalıcı silme | POST | E-41 | Yönetici | Onay; Hakkımızda'da yok |
| UC-161 | İçerik kaydının elle sırası | POST | E-41 | Yönetici | Yukarı · aşağı; değere karşı (K-1063) |
| UC-162 | Ana sayfa ve menü ayarlarını kaydetme | POST | E-42 | Yönetici | Düzen, menü adları, sosyal bağlantılar; bölüm sürümü (K-1014) |
| UC-163 | Marka ayarlarını kaydetme | POST | E-43 | Yönetici | Logo ve simge UC-117 ile; sürüm (OB-10) |
| UC-164 | Firma kimliğini kaydetme | POST | E-44 | Yönetici | Sürüm (OB-10); üretilen metinlerin sürümü artar |
| UC-165 | Satışı geçici olarak kapatma | POST | E-44 | Yönetici | Onay; anahtar değere karşı (K-1021) |
| UC-166 | Satışı yeniden açma | POST | E-44 | Yönetici | Anahtar değere karşı (K-1021); kapının öteki koşulları |
| UC-167 | Ödeme yöntemlerini kaydetme — havale anahtarı ve IBAN | POST | E-45 | Yönetici | IBAN değişikliği ve havalenin kapanışı onay ister (K-663, K-664); bölüm sürümü |
| UC-168 | Kargo ücreti ve eşikleri kaydetme | POST | E-46 | Yönetici | Bölüm sürümü (K-1014) |
| UC-169 | Teslimat illerini kaydetme | POST | E-46 | Yönetici | Bölüm sürümü; üretilen metinlerin sürümü artar |
| UC-170 | Süreleri kaydetme | POST | E-46 | Yönetici | Bölüm sürümü; P-5 ve P-7'nin çitleri |
| UC-171 | Ürün varsayılanlarını kaydetme | POST | E-46 | Yönetici | Bölüm sürümü |
| UC-172 | İade adresini kaydetme | POST | E-46 | Yönetici | Bölüm sürümü; değişiklik onay ister (K-665) |
| UC-173 | Yasal metnin taslağını kaydetme | POST | E-47 | Yönetici | Aydınlatma metni ya da çerez politikası; sürüm (OB-10) |
| UC-174 | Yasal metni yayına alma | POST | E-47 | Yönetici | Sürüm kendiliğinden artar; aydınlatma tamamlanmadan reddedilir |
| UC-175 | Yönetici daveti gönderme | POST | E-49 | Yönetici | "Yeni davet gönder" de bu aday; F-6 |
| UC-176 | Daveti geri çekme | POST | E-49 | Yönetici | Bağlantı o anda geçersiz |
| UC-177 | Yöneticiyi kaldırma | POST | E-49 | Yönetici | Onay; kendi satırı ve son yönetici engeli; oturumlar kapanır |
| UC-178 | Yöneticinin adını değiştirme | POST | E-50 | Yönetici | Yeniden doğrulama istemez |
| UC-179 | Yöneticinin e-postasını değiştirme | POST | E-50 | Yönetici | Tek kullanımlık değer (K-971); yönetici hesapları içinde tekil |
| UC-180 | Yönetici e-posta değişikliğinin bağlantısını yeniden isteme | POST | E-50 | Yönetici | L-9 |
| UC-181 | Yöneticinin şifresini değiştirme | POST | E-50 | Yönetici | Tek kullanımlık değer (K-971); öteki oturumlar kapanır |
| UC-182 | Siparişlerin dışa aktarılması | GET ya da POST (API9-05) | E-53 | Yönetici | Tarih aralığı; CSV; işlem izine yazılır |
| UC-183 | Üye listesinin dışa aktarılması | GET ya da POST (API9-05) | E-53 | Yönetici | CSV; işlem izine yazılır |
| UC-184 | İletişim taleplerinin dışa aktarılması | GET ya da POST (API9-05) | E-53 | Yönetici | CSV; işlem izine yazılır |
| UC-185 | Yöneticinin şifreyle yeniden doğrulaması | POST | E-54 | Yönetici | L-1; tek kullanımlık değer üretir (K-971) |
| UC-186 | Yöneticinin yeni adresinin doğrulanması | POST | E-54 | Kullanıcı | Bağlantının anahtarını taşır (K-904; `05 §6.4.6`) |
| UC-187 | Yöneticinin e-posta değişikliğini geri alması | POST | E-54 | Kullanıcı | Bağlantının anahtarını taşır (K-839); bütün oturumlar kapanır |

**"Uç" sütununun değer kümesi (K-1153).** (1) **Aday uç** — `UC-nn`, yukarıdaki listeden. (2) **Konvansiyon** — `konvansiyon — APIn-mm`: kaynak satırının kuralı tek bir uca değil, bütün uçlara ya da bir uç sınıfına uygulanır ve evi `07 §2` (ya da §5, §6, §8) olur — bir uç sınıfının kuralında evi gösterilen konunun hedefidir: panelin uçları için §9 (API9-03, API9-05), aktörler için başlık notu ve §3 (API0-07), vitrinin makine uçları için §4 (API4-07, API4-09) (K-1155); değer kuralın karara bağlanacağı konunun kimliğini taşır (karar kaydı §16.3) — ör. hata gösterimi API2-07, listelerin parametreleri API2-06, yapılabilir işlemler API8-01, çerçevenin her sayfa yanıtındaki verisi API5-01. Bir satır hem aday uç hem konvansiyon taşıyabilir. (3) **Sabit "uç doğurmaz" değerleri**, adıyla: *uç doğurmaz — görünüm* (yerleşim, sıra, responsive; yanıtın içeriğini değiştirmeyen sunum) · *uç doğurmaz — istemci* (tarayıcıda ya da cihazda kalan davranış: paylaşım menüsü, kopyalama) · *uç doğurmaz — dış sayfa* (ürünün dışındaki sayfa: sağlayıcı, Google, sosyal medya, ETBİS, kargo takibi) · *uç doğurmaz — ürünün dışında* (sistem dışı adım) · *uç doğurmaz — e-posta (`08`)* · *uç doğurmaz — sistem işi (`05 §5`)* · *uç doğurmaz — yoktur* (kaynağın yokluğunu yazdığı işlev — uç açılmaz) · *uç doğurmaz — yazım konvansiyonu* · *uç doğurmaz — kurulum*. Serbest metin gerekçe yoktur; betik "Uç" değerlerinin kümenin dışına çıkmadığını denetler (§1.1.1, `deger07()`).

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
| `04` 5.13.19 | "Sepete dön" → E-12; yazılanlar o tarayıcıda ve o ziyaret boyunca durur — "Sepete dön" formu çereze yazıp sepete yönlendiren düğmedir (`05 §6.7.9`; GAP-14) | UC-26 · UC-19 · UC-22 | eşlendi |
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
| `04` 3.1.10 | E-13'ün sadeleşmiş üst bölümü: logo → E-01, "Sepete dön" → E-12 (formu çereze yazarak — GAP-14) | UC-01 · UC-26 · UC-19 | eşlendi |
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
| `04` 3.3.15 | E-13 "Sepete dön" · girebilecek kalem kalmaması · hiçbir yöntemin kullanılamaması → E-12 | UC-26 · UC-19 · UC-27 | eşlendi |
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

#### 1.1.6 Arayüz Tanımları — `04 §9` panel ekranları ve §4.2'nin metni (431)

Önce §4.2'nin metni, sonra ekran ekran §9'un sırasıyla: ekranın kendisi (`04` E-n — ekranın veri ihtiyacı onun GET ucudur; K-1150), maddeleri ve kimliksiz maddeleri. Panel ekranlarının GET adayları 1a'nındır (UC-84…UC-108), ekranların POST işlemleri ve dosya uçları 1b'nin adaylarıdır (UC-110…UC-187 — §1.1.2; K-1154). İşlemin onay penceresi ve sayfanın içinde açılan formu ayrı aday değildir — formun son adımıdır ve biçimi API2-02'nindir. E-37'nin bütün işlemleri siparişin sürümünü taşır ve sipariş değiştiyse uygulanmaz (OB-10'un sipariş varyantı — API9-03); kalıp satır başına yinelenmez, 9.9.34 ve §9.9'un aksiyon metni onu taşır. "Durum × rol varyantları" maddesinin düğmeleri sunucunun kararıdır (konvansiyon — API8-01); panelin tek rolü ve aktörleri API0-07'dir (K-1155). Matrisin işi eşlemedir: uçların istek ve yanıt ayrıntısı §9'un işidir (yazım turu).

| Kaynak ID | Kaynak özeti | Uç | Durum |
|---|---|---|---|
| `04 §4.2` metin | Panel ekranlarının aktörü Yönetici, yönetim tarafı tek rol; davet, sıfırlama ve e-postadaki bağlantıyla açılan ekran ve adımlarda aktör Kullanıcı — panelin oturumsuz açılan sayfaları; "Menü" sütunu panel menüsündeki yer | konvansiyon — API0-07 · konvansiyon — API2-03 | konvansiyon |
| `04` E-29 | Panel girişi ve şifre sıfırlama — yöneticiyi şifreyle panele alır, panel şifresini sıfırlatır; oturumdan önce açılır (§4.2) | UC-84 · UC-110 · UC-111 · UC-112 | eşlendi |
| `04` 9.1.1 | Aktör Yönetici · Kullanıcı; üç adım — giriş, sıfırlama isteği, yeni şifre; giriş: panelin adresi, oturum gerektiren panel ekranının adresi (3.5.14), çıkış, kendiliğinden kapanan oturum, e-postadaki sıfırlama bağlantısı, davet kabulünden sonra ön dolu adres; çıkış: E-31 ya da istenen panel ekranı | UC-84 · UC-86 · konvansiyon — API2-03 | eşlendi |
| `04` 9.1.2 | İlk görülen: e-posta ve şifre alanları, birincil düğme giriş; panel çerçevesi yok | UC-84 | eşlendi |
| `04 §9.1` m1 | Bilgi hiyerarşisi üç adımdır; düzen müşteri tarafındaki giriş ve sıfırlama ekranlarının kalıbı, kurallar `02 §10.2.4` gereği aynı | UC-84 | eşlendi |
| `04` 9.1.3 | Giriş adımı: başlık, form — e-posta, şifre, "beni hatırla" işaretsiz gelir; işaretliyse uzun, değilse kısa oturum (Z-4, Z-5) —, giriş düğmesi, sıfırlama bağlantısı | UC-84 · UC-110 | eşlendi |
| `04` 9.1.4 | Sıfırlama isteği adımı: e-posta alanı ve gönderme; gönderimden sonra formun yerinde nötr metin | UC-84 · UC-111 | eşlendi |
| `04` 9.1.5 | Yeni şifre adımı e-postadaki bağlantıyla açılır (3.5.5): yeni şifre ve kurma; kurulduktan sonra oturumların kapandığı ve giriş adımına dönen bağlantı | UC-84 · UC-112 | eşlendi |
| `04` 9.1.6 | Yoktur: Google ile giriş, kayıt bağlantısı, kendi kendine yönetici kaydı, iki adımlı doğrulama, müşteri hesabının aranması, ürünün içinden erişim kurtarma | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 9.1.7 | Girmek: oturum açılır, tarayıcı o hesap için tanınan tarayıcı olur, E-31'e ya da istenen ekrana iner; vitrinin hesap alanı ve sepeti etkilenmez — iki ayrı kimlik şeması | UC-110 · UC-86 · konvansiyon — API2-03 | eşlendi |
| `04` 9.1.8 | Hatalı giriş: formun başında K-779'un metni, hangi alanın yanlış olduğunu ve hesabın varlığını söylemez; L-1 aşılınca nötr limit mesajı, muafiyet ve kilit yok; tanınan tarayıcı e-posta ekseninde engellenmez | UC-110 · konvansiyon — API2-07 · konvansiyon — API2-08 | eşlendi |
| `04` 9.1.9 | Sıfırlama bağlantısını istemek: onay yok; adresin yönetici hesabı olup olmadığını söylemez; L-2'de nötr limit mesajı; bağlantı yöneticinin kendi adresine | UC-111 · konvansiyon — API2-08 | eşlendi |
| `04` 9.1.10 | Yeni şifreyi kurmak: bağlantı tek kullanımlık; hesabın bütün oturumları kapanır, tarayıcı tanınan tarayıcı olur; kurma oturum açmaz | UC-112 · konvansiyon — API2-05 | eşlendi |
| `04` 9.1.11 | Validasyon: e-posta ve şifre zorunlu, e-posta biçimi; yeni şifrede P-17 ve yaygın şifreler; ret alan mesajı; alanlar 7.2.1 | UC-110 · UC-111 · UC-112 · konvansiyon — API2-07 | eşlendi |
| `04` 9.1.12 | Tek rol; kullanılmış ya da süresi (Z-2) dolmuş bağlantıda yeni şifre adımı açılmaz, ekran geçersizliği söyler ve isteme adımına götürür; kendiliğinden kapanan oturumla gelen mesajla görür; satış kapısı etkilemez | UC-84 · konvansiyon — API2-03 | eşlendi |
| `04` 9.1.13 | Boş hâl yok; gönderim sürerken düğme ikinci kez basılamaz; beklenmeyen hata işlem düzeyinde; e-posta kesintisinde sıfırlama bağlantısı ulaşmaz — kalan risk; Google'ın kesintisi etkilemez | UC-84 · konvansiyon — API2-07 | eşlendi |
| `04` 9.1.14 | Responsive: tek sütun | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-30 | Yönetici daveti kabul ekranı — davet bağlantısından ad ve şifreyle yönetici hesabını açtırır (§4.2) | UC-85 · UC-113 | eşlendi |
| `04` 9.2.1 | Aktör Kullanıcı — davetli; giriş davet e-postasının bağlantısı (3.5.10); çıkış: hesap açılınca panel girişi, davetin adresi ön dolu; geçersiz bağlantıda panel girişi bağlantısı | UC-85 · UC-84 | eşlendi |
| `04` 9.2.2 | İlk görülen: hesabın açılacağı adres, ad ve şifre; birincil düğme hesabı açmak; panel çerçevesi yok | UC-85 | eşlendi |
| `04 §9.2` m1 | Bilgi hiyerarşisi iki hâldir — geçerli davet ve geçersiz bağlantı; hâli davetin bağlantısı belirler | UC-85 | eşlendi |
| `04` 9.2.3 | Geçerli davet: başlık, davetin adresi salt okunur — hesap bu adresle doğar —, form: ad zorunlu ve şifre, hesabı açan düğme | UC-85 · UC-113 | eşlendi |
| `04` 9.2.4 | Geçersiz bağlantı — kullanılmış, süresi (Z-3) dolmuş, geri çekilmiş, yeni davetle, gönderenin kaldırılmasıyla ya da adres bir yönetici hesabının olmuşsa (K-1085): hesap açılmaz, sebebi ayırt etmeyen mesaj, panel girişinin bağlantısı | UC-85 · UC-84 · konvansiyon — API2-07 | eşlendi |
| `04` 9.2.5 | Yoktur: Google ile hesap açma, kendi kendine kayıt, adresin bu ekranda değiştirilmesi | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 9.2.6 | Hesabı açmak: onay yok; hesap o anda doğar, şifre kuralları müşteriyle aynı; müşteri hesabının adresi de olsa ayrı yönetici hesabı; işlem izine yazılır; açma oturum açmaz, panel girişine götürür (K-807) | UC-113 · UC-84 | eşlendi |
| `04` 9.2.7 | Validasyon: ad boş olamaz, şifre politikası; ret alan mesajı; alanlar 7.2.2 | UC-113 · konvansiyon — API2-07 | eşlendi |
| `04` 9.2.8 | Ekran oturumdan bağımsız; hâli davetin geçerliliği belirler; satış kapısı etkilemez | UC-85 · konvansiyon — API2-03 | eşlendi |
| `04` 9.2.9 | Boş hâl yok; gönderim sürerken düğme ikinci kez basılamaz; hesap açılamazsa form korunur ve yeniden denenir — bağlantı o arada geçersizleştiyse 9.2.4'ün hâli; davet e-postası ulaşmazsa yol yeni davet | UC-113 · UC-85 · konvansiyon — API2-07 | eşlendi |
| `04` 9.2.10 | Responsive: tek sütun | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-31 | Panel ana sayfası — uyarılar, kurulum kontrol listesi, bekleyen işlerin sayaçları ve "IBAN bekleniyor" listesi (§4.2) | UC-86 | eşlendi |
| `04` 9.3.1 | Aktör Yönetici; giriş menü, panel girişi, vitrinin "Panele dön"ü; çıkış: sayaçlar E-36'nın ya da E-38'in süzülmüş hâli, uyarı satırları, kontrol listesi maddeleri, "Müşteriden beklenenler" satırları | UC-86 | eşlendi |
| `04` 9.3.2 | İlk görülen: varsa uyarılar, yoksa kurulum kontrol listesi, o da yoksa sayaçlar; birincil düğme yok | UC-86 | eşlendi |
| `04 §9.3` m1 | Panel çerçevesinin içinde yukarıdan aşağıya dört bölüm; boş bölüm görünmez (K-750) | UC-86 | eşlendi |
| `04` 9.3.3 | Uyarılar, sırasıyla: satışın kapalı olduğu ve sebebi, kanal uyarısı (geçiş yok), "firma bildirimleri ulaşmıyor", "e-posta ulaşmadı" işaretli sipariş sayısı, gerçekleşmeyen ve sonucu beklenen kart iadesinin sayısı — koşul sürerken; götürdükleri yer 3.4.3 | UC-86 | eşlendi |
| `04` 9.3.4 | Kurulum kontrol listesi — satışın açılması için eksik maddeler adıyla: firma tipinin zorunlu alanları, açık ödeme yöntemi ve IBAN, iki yasal metnin yayını — veri toplayan girişlerin kapısıyla birlikte —, iade adresi; tamamlanınca bölüm kaybolur | UC-86 | eşlendi |
| `04` 9.3.5 | Bekleyen işler — altı sayaç `02 §10.6.1`'in sırasıyla ve adlarıyla, sıfır da görünür; sayaç kalan süre göstermez | UC-86 | eşlendi |
| `04` 9.3.6 | Müşteriden beklenenler — "IBAN bekleniyor" listesi kalem kalem (geç gelen ödemenin geri ödemesinde kalemsiz satır — K-1108), sebep, tarih, kalan süre; kalan süresi en az olan önce; ilk on satır, tamamı sipariş listesinin süzgecinde | UC-86 · UC-91 | eşlendi |
| `04` 9.3.7 | Yoktur: satış özeti, yedinci sayaç, bekleyen işlerin e-postayla hatırlatılması, ayrı "bekleyen işler" ekranı, kurulum sihirbazı | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 9.3.8 | Aksiyonlar geçiştir: uyarı → 3.4.3'ün hedefi, kontrol listesi maddesi → ayar ekranı, sayaç → süzülmüş liste, "IBAN bekleniyor" satırı → E-37, "Tümünü gör" → E-36'nın süzgeci; ekranın kendi işlemi yok | UC-91 · UC-92 · UC-93 · UC-99 · UC-100 · UC-101 · UC-102 · konvansiyon — API2-06 | eşlendi |
| `04` 9.3.9 | Validasyon: form yok | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 9.3.10 | İlk kurulumda: uyarılarda satışın kapalı olduğu ve karşılanmayan koşullar, kontrol listesi; sayaçlar sıfır; "Müşteriden beklenenler" görünmez | UC-86 | eşlendi |
| `04` 9.3.11 | Satış kapalıyken uyarı sebebi adıyla söyler; geçici kapatmada liste tamamlanmış olsa da satır durur; açık siparişler, sayaçlar ve liste işler | UC-86 | eşlendi |
| `04` 9.3.12 | Uyarı koşulu kalkınca kendiliğinden kalkar; "firma bildirimleri ulaşmıyor" sonraki bildirim ulaşınca ya da iletişim e-postası değişince; kesintiden dönüşte sayaçlar mevcut durumlardan hesaplanır | UC-86 | eşlendi |
| `04` 9.3.13 | Boş bölüm görünmez, bekleyen işler hiç boş değil; bölümler kendi yerlerinde yüklenir; sayfa açılamazsa panel çerçevesinde hata sayfası | UC-86 · UC-78 | eşlendi |
| `04` 9.3.14 | Responsive: bölümler alt alta, sayaçlar ikişerli, üçerli ya da tek sıra; sayaçtan siparişe en çok üç adım | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-32 | Ürün listesi — ürünleri yayın durumlarıyla listeler, adla aratır, yayın durumuna göre süzdürür (§4.2) | UC-87 | eşlendi |
| `04` 9.4.1 | Aktör Yönetici; giriş menü ve ürün formundan dönüş; çıkış "Yeni ürün" ve satır → E-33 | UC-87 · UC-88 | eşlendi |
| `04` 9.4.2 | İlk görülen: en yeni eklenen önce, arama ve yayın durumu süzgeci; birincil düğme "Yeni ürün" | UC-87 | eşlendi |
| `04` 9.4.3 | Başlık ve "Yeni ürün"; ürün adıyla arama ve yayın durumu süzgeci — Taslak · Yayında · Arşiv (K-737); tablo — görsel, ad, tip, rozet, varyant sayısı, fiyat ya da aralığı, stok durumu; sayfa başına 25 | UC-87 · konvansiyon — API2-06 | eşlendi |
| `04` 9.4.4 | Yoktur: satırda işlem, stok, kategori ve tip süzgeci, elle sıra, içe aktarma, listeden toplu güncelleme | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 9.4.5 | Aramak ve süzmek — liste ilk sayfasına döner; arşivdeki ve taslaktaki ürün yalnız burada aranarak bulunur — sorgu parametreleri | UC-87 · konvansiyon — API2-06 | eşlendi |
| `04` 9.4.6 | "Yeni ürün" → E-33'ün boş formu, ilk kayıtta Taslak doğar; ürün satırı → E-33 | UC-88 · UC-114 | eşlendi |
| `04` 9.4.7 | Validasyon: arama serbest metin, biçim denetimi yok | UC-87 · konvansiyon — API2-06 | eşlendi |
| `04` 9.4.8 | Satır yayın durumunu rozetle taşır; tükenmiş fiziksel ürün "Tükendi" ile listede kalır; satış kapısı etkilemez | UC-87 | eşlendi |
| `04` 9.4.9 | Hiç ürün yoksa boş hâl "Yeni ürün"le; sonuç boşsa aramayı ve süzgeci kaldırma yolu; tablo kendi yerinde yüklenir; sayfa açılamazsa hata sayfası | UC-87 · UC-88 · UC-78 | eşlendi |
| `04` 9.4.10 | Responsive: orta sınıfta sütunlar satırın altına, dar sınıfta kart listesi | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-33 | Ürün formu — ürünü, varyantlarını, stoğunu, görsellerini, indirimini, dijital dosyasını ve cayma istisnasını düzenletir (§4.2) | UC-88 · UC-114 · UC-115 · UC-116 · UC-117 · UC-83 | eşlendi |
| `04` 9.5.1 | Aktör Yönetici; giriş listenin "Yeni ürün"ü ve satırı, "Taslak" bandının "Düzenlemeye dön"ü; çıkış: "Önizle" ya da "Sitede gör" → E-04 (arşivde E-05), stok alanının yanındaki sipariş → E-37, listeye dönüş | UC-88 · UC-04 · UC-92 · UC-87 | eşlendi |
| `04` 9.5.2 | İlk görülen: ad, tip, yayın durumu rozeti; birincil "Kaydet"; yayın geçişleri ve kalıcı silme ikincil düğmeler | UC-88 · UC-114 · UC-115 · UC-116 | eşlendi |
| `04 §9.5` m1 | Bölümlü form; geniş sınıfta bölümler solda, yayın durumu yan sütunda | uç doğurmaz — görünüm | uç doğurmaz |
| `04` 9.5.3 | Temel bilgiler: tip — ürün düzleminde, varyantlar devralır —, ad (P-27), kategoriler — ağaçtan çoklu seçim, biri ana kategori; ağaç E-34'ün sırasıyla —, açıklama — kapalı metin biçimi seti | UC-88 · UC-114 | eşlendi |
| `04` 9.5.4 | Görseller (OB-19, galeri): yükleme, elle sıra, ana görsel, görsel başına alternatif metin — boşken sistemin üreteceği metin | UC-117 · UC-114 · konvansiyon — API4-07 | eşlendi |
| `04` 9.5.5 | Seçenekler ve varyantlar: en fazla iki boyut (P-21); varyant tablosu — seçenek değerleri, stok kodu, fiyat, net miktar, stok ya da kontenjan, ödemesi beklenen siparişlerin ayırdığı adet ve onları tutan siparişlere bağlantı, varyantın rozeti, görseli, dijitalde dosyası | UC-88 · UC-114 · UC-92 · UC-83 | eşlendi |
| `04` 9.5.6 | Fiyat etiketi ve vergi: KDV oranı — yeni üründe P-10 varsayılanıyla —, üretim yeri ülke listesinden, ölçü birimi ve K-798'in hatırlatması; fiyatın başlangıç tarihi ve yerli üretim logosu sistemin gösterdiği bilgiler | UC-88 · UC-114 | eşlendi |
| `04` 9.5.7 | Teslim ve cayma: kargoya verme süresi (P-6, çiti P-5), hizmette ifa süresi (P-44), bir siparişte en fazla adet (P-9), cayma istisnası ve kapalı listeden sebebi; değişiklik yalnız yeni siparişlere işler | UC-88 · UC-114 | eşlendi |
| `04` 9.5.8 | İndirim: yüzde, başlangıç ve bitiş (OB-16); varyant başına sistemin hesapladığı referans ve indirimli fiyat salt okunur; indirimin hâli satır metni | UC-88 · UC-114 · konvansiyon — API2-09 | eşlendi |
| `04` 9.5.9 | Dijital dosya: ürün düzleminde tek dosya, varyant kendi dosyasını taşıyabilir (P-26); dosyasız yayındaki varyant işaretli; güncellemenin geçmiş alıcılara etkisi bir cümleyle | UC-83 · UC-88 · UC-114 · konvansiyon — API4-07 | eşlendi |
| `04` 9.5.10 | Yayın durumu yan sütunu: rozet, yayın kapısının karşılanmayan koşulları adıyla, "Önizle" ya da "Sitede gör" (arşivde E-05), yayın geçişlerinin düğmeleri ve kalıcı silme, ilk yayında sabitlenen adres | UC-88 · UC-115 · UC-116 · UC-04 · konvansiyon — API8-01 · konvansiyon — API2-01 | eşlendi |
| `04` 9.5.11 | Yoktur: ileri tarihli yayın, elle sıra, garanti alanı, referans fiyatın elle girilmesi, içe aktarma ve toplu güncelleme | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 9.5.12 | Kaydetmek: yeni ürün ilk kayıtta Taslak; zorunlu alanı boşaltan ya da yayın kapısını bozan kayıt kaydedilmez, engel yerinde; fiyat değişikliği fiyat geçmişine; fiyat ve stok işlem izine yazılmaz; onay yok | UC-114 · konvansiyon — API2-07 | eşlendi |
| `04` 9.5.13 | Yayına almak: kapı eksikse alınmaz, eksik koşullar yan sütunda; onay yok; değişiklik işlem izine | UC-115 · konvansiyon — API2-07 | eşlendi |
| `04` 9.5.14 | Taslağa ve arşive almak — ürünün ve varyantın kendi satırından; yayındaki ürünün son Yayında varyantının geçişi engellenir; vitrinden kalkan kalem sepetlerden çıkar | UC-115 · konvansiyon — API2-07 | eşlendi |
| `04` 9.5.15 | Kalıcı silmek — ürün yan sütundan, varyant kendi satırından; onay penceresi açık sipariş sayısını söyler, silme engellenmez; son Yayında varyant silinmez | UC-116 · konvansiyon — API2-02 | eşlendi |
| `04` 9.5.16 | Önizlemek → E-04, taslak ürün "Taslak" bandıyla; kaydedilmiş hâli gösterir | UC-04 · konvansiyon — API2-03 | eşlendi |
| `04` 9.5.17 | İndirim tanımlamak: indirimli fiyat referansın altına inmiyorsa, %100 ya da yuvarlanmış fiyat sıfırsa kaydedilmez ve sebebi yerinde | UC-114 · konvansiyon — API2-07 | eşlendi |
| `04` 9.5.18 | Stoğu ya da kontenjanı girmek: ayrılmış adedin altına inen değer reddedilir, mesaj ayrılmış adedi ve siparişleri gösterir — alan ürünün sürümüyle değil o değere karşı yazılır (K-980) | UC-114 · konvansiyon — API2-04 | eşlendi |
| `04` 9.5.19 | Dosya yüklemek ve silmek: yükleme ilerlemeyi gösterir ve sürerken form kaydedilemez; yayındaki varyantı dosyasız bırakacak silme engellenir; güncelleme bildirim üretmez | UC-83 · UC-114 · konvansiyon — API4-07 · konvansiyon — API8-02 | eşlendi |
| `04` 9.5.20 | Eşzamanlı düzenleme (OB-10, kayıt varyantı): güncel hâli açmak ya da uyarıyı görerek üzerine yazmak | UC-114 · konvansiyon — API2-04 | eşlendi |
| `04` 9.5.21 | Validasyon: ad P-27, fiyat sıfırdan büyük ve iki ondalık, indirim yürürken sıfıra inen fiyat, stok kodu zorunlu ve tekil, stok tam sayı ve ayrılmış adedin üstünde, süre çitleri, görsel ve dosya sınırları, iki seçenek boyutu, üretim yeri, net miktar; alanlar 7.2.4 | UC-114 · UC-117 · UC-83 · konvansiyon — API2-07 | eşlendi |
| `04` 9.5.22 | Tipe göre alanlar, yayın durumuna göre yan sütunun düğmeleri; arşivdeki ürünün adresi "Bu ürün artık satılmıyor" döner, "Sitede gör" onu açar | UC-88 · UC-04 · konvansiyon — API8-01 | eşlendi |
| `04` 9.5.23 | Yeni ürünün formu boş — KDV varsayılanla, stok kodu adla sistemin önerisiyle dolar; yükleme kendi yerinde; kayıt tamamlanamazsa form korunur; sayfa açılamazsa hata sayfası | UC-88 · UC-114 · UC-78 · konvansiyon — API2-07 | eşlendi |
| `04` 9.5.24 | Responsive: tek sütun, yan sütun formun başına, varyant kartları | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-34 | Kategori ağacı — ağacı kurdurur ve dalları taşıtır (§4.2) | UC-89 · UC-118 · UC-119 · UC-120 · UC-121 | eşlendi |
| `04` 9.6.1 | Aktör Yönetici; giriş menü; çıkış çerçevenin geçişleri; ürünün asılması ürün formunda | UC-89 | eşlendi |
| `04` 9.6.2 | İlk görülen: ağaç; birincil "Yeni kategori" — birinci seviye | UC-89 · UC-118 | eşlendi |
| `04` 9.6.3 | Başlık ve "Yeni kategori"; ağaç en fazla üç seviye (P-20), aynı seviyede alfabetik; düğüm: ad, doğrudan ürün ve alt kategori sayısı, vitrinde görünmediğini söyleyen satır, işlemler — alt kategori ekle, yeniden adlandır, taşı, sil | UC-89 · UC-118 · UC-119 · UC-120 · UC-121 · konvansiyon — API8-01 | eşlendi |
| `04` 9.6.4 | Yoktur: elle sıra, üç seviyeden derin dal, ürünün bu ekrandan asılması | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 9.6.5 | Eklemek ve yeniden adlandırmak: ad girilir, başarı kısa bildirimle; onay yok | UC-118 · UC-119 · konvansiyon — API2-02 | eşlendi |
| `04` 9.6.6 | Taşımak: yeni üst kategori ya da birinci seviye seçim listesinden — klavyeyle; üç seviyeyi aşan ya da kendi alt dalına götüren taşıma yapılmaz ve sebebi yerinde | UC-120 · konvansiyon — API2-07 | eşlendi |
| `04` 9.6.7 | Silmek: içinde ürün ya da alt kategori varsa silinmez, engel sayılarla ve taşıma önerisiyle; boş kategori onaysız silinir | UC-121 · konvansiyon — API2-07 | eşlendi |
| `04` 9.6.8 | Validasyon: ad zorunlu; alanlar 7.2.5 | UC-118 · UC-119 · konvansiyon — API2-07 | eşlendi |
| `04` 9.6.9 | Kategorinin durumu yok; menüdeki görünürlüğü yayındaki ürünlerden okunur; ürünsüz kategorinin adresi vitrinde boş kategori sayfası | UC-89 · UC-02 | eşlendi |
| `04` 9.6.10 | Hiç kategori yoksa boş hâl "Yeni kategori"yle ve ana kategorisiz ürünün yayına alınamadığı; sayfa açılamazsa hata sayfası | UC-89 · UC-118 · UC-78 | eşlendi |
| `04` 9.6.11 | Responsive: dar sınıfta ağaç katlanır, düğüm menüsü, taşıma seçim listesiyle | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-35 | Kuponlar — kuponları listeler ve düzenletir (§4.2) | UC-90 · UC-122 | eşlendi |
| `04` 9.7.1 | Aktör Yönetici; giriş menü; liste ve kuponun formu aynı ekranda | UC-90 | eşlendi |
| `04` 9.7.2 | İlk görülen: kupon listesi en yeni önce; birincil "Yeni kupon" | UC-90 | eşlendi |
| `04` 9.7.3 | Liste: kod, tür ve değer, asgari tutar, geçerlilik aralığı, kullanılmış, ayrılmış ve toplam adet, geçerlilik hâli satır metni — durum makinesi yok | UC-90 | eşlendi |
| `04` 9.7.4 | Kuponun formu ekranın içinde: kod tekil ve harf ayrımsız (K-806), tür, değer, asgari tutar, başlangıç ve bitiş, toplam kullanım adedi, kullanılmış ve ayrılmış haklar salt okunur | UC-90 · UC-122 | eşlendi |
| `04` 9.7.5 | Yoktur: kuponun silinmesi, arama ve süzgeç, siparişe ikinci kod | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 9.7.6 | Kaydetmek: onay yok; değişiklik yalnız yeni siparişlere; kullanım adedi kullanılmış ve ayrılmış hakların altına inmez (K-654); kampanya bitiş tarihi öne çekilerek durdurulur | UC-122 · konvansiyon — API2-07 | eşlendi |
| `04` 9.7.7 | Eşzamanlı düzenleme — OB-10 kayıt varyantı | UC-122 · konvansiyon — API2-04 | eşlendi |
| `04` 9.7.8 | Validasyon: kod zorunlu ve tekil, değer sıfırdan büyük, yüzde %100'e ulaşmaz, sabit tutarda asgari tutar kupon tutarından büyük, bitiş başlangıçtan önce olamaz, adet sınırı; alanlar 7.2.6 | UC-122 · konvansiyon — API2-07 | eşlendi |
| `04` 9.7.9 | Geçerlilik hâli tarih aralığından ve kalan haktan okunur (Z-22); dolmuş kupon düzenlenebilir ve yeniden geçerli olabilir | UC-90 | eşlendi |
| `04` 9.7.10 | Hiç kupon yoksa boş hâl "Yeni kupon"la; kayıt tamamlanamazsa form korunur; sayfa açılamazsa hata sayfası | UC-90 · UC-122 · UC-78 | eşlendi |
| `04` 9.7.11 | Responsive: form listenin yanında ya da yerinde, dar sınıfta kart listesi | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-36 | Sipariş listesi — siparişleri aratır, iki eksenin durumuna, bekleyen işe ve işarete göre süzdürür; sayaçların süzülmüş listesi (§4.2) | UC-91 | eşlendi |
| `04` 9.8.1 | Aktör Yönetici; giriş menü, ana sayfanın sayaçları ve uyarıları, "Tümünü gör", ayrıntıdan dönüş, iletişim talebinin "Siparişlerde ara"sı; çıkış satır → E-37 | UC-91 · UC-92 | eşlendi |
| `04` 9.8.2 | İlk görülen: en yeni önce; sayaçtan ya da uyarıdan gelindiyse süzgeç seçili ve adı listenin başında — süzgeç adreste taşınır; birincil düğme yok | UC-91 · konvansiyon — API2-06 | eşlendi |
| `04` 9.8.3 | Arama — tek alan: sipariş numarası ya da iletişim e-postası, tam eşleşme, e-postada harf ayrımı yok (K-802) | UC-91 · konvansiyon — API2-06 | eşlendi |
| `04` 9.8.4 | Süzgeçler: Sipariş durumu altı değer çoklu, Ödeme durumu beş değer çoklu, bekleyen iş ve işaret tek seçim — beş sayaç, "IBAN bekleniyor", "e-posta ulaşmadı"; arama ile birlikte uygulanır | UC-91 · konvansiyon — API2-06 | eşlendi |
| `04` 9.8.5 | Tablo: numara, tarih, alıcı ve e-posta, kalemler, toplam, ödeme yöntemi, iki rozet, panel işaretleri, işleyen sürelerden kalanı en az olan | UC-91 | eşlendi |
| `04` 9.8.6 | Sayfa gezinmesi — sayfa başına 25 | UC-91 · konvansiyon — API2-06 | eşlendi |
| `04` 9.8.7 | Sıra: en yeni önce; süreli bekleyen iş süzgecinde kalan süresi en az olan önce, aşılmış en başta, sayaçsız satır sonda; sıralama seçeneği yok | UC-91 · konvansiyon — API2-06 | eşlendi |
| `04` 9.8.8 | Yoktur: toplu işlem, satırdan işlem, tarih, yöntem ve tutar süzgeci, elle sipariş, listeden dışa aktarma | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 9.8.9 | Aramak ve süzmek — ilk sayfaya döner; seçili süzgeçlerin adı listenin başında, tek dokunuşla kaldırılır | UC-91 · konvansiyon — API2-06 | eşlendi |
| `04` 9.8.10 | Siparişi açmak → E-37; listeye dönüş bırakılan sayfayı, aramayı ve süzgeci bulur | UC-92 · UC-91 · konvansiyon — API2-06 | eşlendi |
| `04` 9.8.11 | Validasyon: arama serbest metin, biçim denetimi yok — eşleşme yoksa boş hâl | UC-91 · konvansiyon — API2-06 | eşlendi |
| `04` 9.8.12 | Kendiliğinden iptal edilmiş ödenmemiş sipariş burada İptal edildi + Başarısız rozetleriyle görünür; açık ödenmemişler "ödeme onayı bekleyen" ve Bekliyor süzgeçleriyle; Teslim edilemedi "kargoya verilecek"te; satış kapısı etkilemez | UC-91 | eşlendi |
| `04` 9.8.13 | Hiç sipariş yoksa boş hâl, ilk kaydı açan düğme yok; sonuç boşsa kaldırma yolu; sıfır sayaçtan gelinen liste boş hâlle; sayfa açılamazsa hata sayfası | UC-91 · UC-78 | eşlendi |
| `04` 9.8.14 | Responsive: orta sınıfta sütunlar satırın altına, dar sınıfta kart listesi, süzgeçler açılır bölümde | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-37 | Sipariş ayrıntısı — siparişin bütün bilgisi, yürütüm işlemleri ve müdahaleler, siparişin ayıp talepleri (§4.2) | UC-92 · konvansiyon — API9-03 · konvansiyon — API8-01 | eşlendi |
| `04` 9.9.1 | Aktör Yönetici; giriş: listenin satırı, "IBAN bekleniyor" satırı, Talepler'in ayıp talebi satırı, üye kaydının bağlı siparişi, ürün formunun ayrılmış adedi, oturumsuz gelişte girişten sonra; çıkış: listeye dönüş, işlemler ekranda kalır, kargo takip sayfası | UC-92 · UC-91 · konvansiyon — API2-03 · uç doğurmaz — dış sayfa | eşlendi |
| `04` 9.9.2 | İlk görülen: numara, iki rozet, panel işaretleri; altında hattın sıradaki zorunlu adımı birincil düğme, işleyen süresiyle (K-803) | UC-92 · konvansiyon — API8-01 | eşlendi |
| `04 §9.9` m1 | Bilgi hiyerarşisi geniş sınıfta iki sütun — solda işin, sağda kaydın bölümleri (K-803) | uç doğurmaz — görünüm | uç doğurmaz |
| `04` 9.9.3 | Başlık: numara ve tarih, iki rozet, ödeme yöntemi, üye hesabıyla mı girişsiz mi verildiği — e-posta düzeltmesi yalnız girişsiz siparişte —, "Listeye dön" | UC-92 · UC-91 | eşlendi |
| `04` 9.9.4 | İşaretler: "e-posta ulaşmadı" — ulaşmayan e-postalar tek tek, satırda "Yeniden gönder", birden çoksa "Hepsini yeniden gönder"; "ödeme sonucu alınamadı" — işlem yok; "geri ödeme gerçekleşmedi" — yeniden deneme ve havale yolu; "geri ödeme sonucu bekleniyor" — "Gerçekleşmedi olarak işaretle"; "e-posta düzeltildi" — geçmişe bağlantı; kalem işaretleri kalemin satırında | UC-92 · UC-153 · UC-154 · UC-135 · UC-136 · UC-137 | eşlendi |
| `04` 9.9.5 | Sıradaki adım birincil düğme — havalede "Ödendi olarak işaretle" (donmuş IBAN, toplam, son gün), "Kargoya ver", "Teslim edildi olarak işaretle", ilk açık hizmet kaleminin "Tamamlandı"sı; süre aşımında kanuni faiz uyarısı; "Sipariş işlemleri" ve "Düzeltmeler" grupları; açık olmayan işlem gösterilmez | UC-92 · UC-123 · UC-124 · UC-126 · UC-139 · UC-127 · UC-125 · UC-128 · UC-129 · UC-130 · UC-133 · UC-134 · UC-146 · UC-147 · UC-148 · UC-150 · UC-151 · konvansiyon — API8-01 | eşlendi |
| `04` 9.9.6 | Kalemler donmuş hâliyle: adet ve açık adet, fiyatlar ve paylar, cayma istisnası; kalemin kayıtları ayrı ayrı — teslim, iptal, çıkarma, gecikme feshi, cayma beyanı, iade teslim alma ve reddi, "mal dönmedi", IBAN isteği, indirme sayacı; kalemin işaretleri; bekleyen geri ödeme — adetler, sistemin hesapladığı tutar, hat, müşterinin IBAN'ı salt okunur, kalan süre; kalemin işlemleri | UC-92 · konvansiyon — API8-01 · konvansiyon — API5-02 | eşlendi |
| `04` 9.9.7 | Ayıp talepleri bölümü: kalem ve ayıplı adet, açıklama, açılış ve yeniden açılış tarihleri, rozet, iade adresi; B-9 ulaşmadıysa satırda işaret; işlemler 9.9.30 | UC-92 · UC-144 · UC-145 | eşlendi |
| `04` 9.9.8 | Tutar ve ödeme kaydı: donmuş döküm, kısmi iptalde yeniden hesaplanmaz; ödeme ve her geri ödeme — tarih, tutar, yol, bağ, kart iadesinin sonucu; "gerçekleşmedi" işaretinden sonra gelen başarı sonucu işlenmez ve satırda kalıcı uyarı (K-940); iptal edilmiş siparişe geç gelen ödeme ve iadesi | UC-92 · konvansiyon — API6-02 | eşlendi |
| `04` 9.9.9 | Teslimat: kargoya verme sözü, kargo şirketi ve takip numarası ya da araç beyanı, "Takip et", teslim tarihi; fiziksel kalemsiz siparişte yok | UC-92 · uç doğurmaz — dış sayfa | eşlendi |
| `04` 9.9.10 | Müşteri ve adresler — donmuş ya da düzeltilmiş hâliyle; adres geçmişi — eski ve yeni değer, yönetici, tarih; müşteriye görünmez (K-1008) | UC-92 · UC-146 | eşlendi |
| `04` 9.9.11 | İç not — tek serbest metin alanı, müşteriye görünmediği yazar; kişisel veri imha edilmişse alan kapalı (K-1095) | UC-92 · UC-152 · konvansiyon — API8-01 | eşlendi |
| `04` 9.9.12 | E-posta geçmişi: müşteriye giden bildirimler — ad, tarih, adres, sonuç; panelden yeniden gönderim ayrı satır; satırlar gönderim kaydının süresince (Z-43); misafir siparişinin e-posta düzeltmeleri; müşteriye görünmez | UC-92 | eşlendi |
| `04` 9.9.13 | Belgeler: onaylanan sürümleriyle Ön Bilgilendirme Formu ve Mesafeli Satış Sözleşmesi, o günkü aydınlatma metni, onay kutularının kaydı; belgeler onay anındaki değerleri gösterir (K-1027) | UC-92 | eşlendi |
| `04` 9.9.14 | Yoktur: elle sipariş, kalem ekleme ve fiyat değiştirme, panelden yanıt, "geri al", fatura, gelen tutar alanı, müşterinin IBAN'ını panelden girme — başka kanaldan gelen bildirimdeki aktarma dışında —, siparişe süzülmüş iz, ters ibraz, kanuni faizin hesabı | uç doğurmaz — yoktur | uç doğurmaz |
| `04 §9.9` m2 | Aksiyonların ortak kuralı: açık olduğu aralık `03 §1.11.15`–§1.11.41'de; form sayfanın içinde açılır, onay isteyen işlem formun son adımında pencereyi açar; sonuç siparişin güncel hâlinde kalıcı durur — kısa bildirimle verilmez | konvansiyon — API2-02 · konvansiyon — API8-01 · konvansiyon — API9-03 | konvansiyon |
| `04` 9.9.15 | "Ödendi" işareti: pencere siparişin IBAN'ını ve tutarını yineler; dijital kalemde K-798'in uyarısı; gelen tutar sorulmaz; iptal edilmiş siparişte yok — kesintide ertelenen iptalde o ana kadar; onay ister; Ö1, B-4 | UC-123 · konvansiyon — API2-02 · konvansiyon — API8-01 | eşlendi |
| `04` 9.9.16 | Kargoya vermek ve yeniden vermek: kargo şirketi listeden ya da "Diğer" ve adı, takip numarası, ya da araç beyanı; gidecek kalemler salt okunur, seçim yok — tek parça; ikisi de onay ister; S5 ya da S8, B-5 | UC-124 · UC-125 · konvansiyon — API2-02 | eşlendi |
| `04` 9.9.17 | Teslim işareti ve tarihi: olay tarihi (OB-16); cayma beyanının günüyle çakışırsa sıra sorusu zorunlu (K-608); onay yok; S6 | UC-126 · konvansiyon — API2-09 | eşlendi |
| `04` 9.9.18 | "Teslim edilemedi" onaysız — S7, gerekirse S11, B-6, "kargoya verilecek" sayacı; açık kalemsiz ve teslimsiz Teslim edilemedi siparişinde "Siparişi kapat" — sebep yok, geri ödeme yok, B-7 yok, onay yok | UC-127 · UC-128 | eşlendi |
| `04` 9.9.19 | Firma iptali: kapsam — ödemeden önce siparişin tamamı, sonra açık kalemler seçilir ve kalem başına adet (üst sınır iptale açık adet, varsayılan tamamı — K-794, K-795); sebep (OB-15); yasal uyarılar; onay ister; ödenmemişte S3 + Ö2, ödenmişte iptal kaydı; kart iadesi kendiliğinden, havalede IBAN isteği ve B-14 | UC-129 · konvansiyon — API2-02 · konvansiyon — API9-03 | eşlendi |
| `04` 9.9.20 | Başka kanaldan gelen bildirimin kaydı: cayma — kalem ve adet; gecikme feshi — kalem ve adet yok, salt okunur kapsam; bildirimin tarihi; siparişin e-postasından geldiği seçeneği ve aktarılan IBAN; teyit hatırlatması; pencere dışı ya da istisnalı kalemde onay kutusu; sıra sorusu; onay ister (K-715) | UC-130 · konvansiyon — API2-02 · konvansiyon — API9-03 | eşlendi |
| `04` 9.9.21 | Geri ödemeyi işlemek — kalemin, geç gelen ödemede siparişin bekleyen geri ödemesinin yanında; havalede IBAN yoksa beklendiği yazar; pencere tutarı ve IBAN'ı yineler, açıkken IBAN düzeltildiyse uygulanmaz; "Havale gerçekleşmedi" onaysız — IBAN silinir, "IBAN bekleniyor"a döner (K-1131); Ö3–Ö5, B-8 | UC-131 · UC-132 · konvansiyon — API9-03 | eşlendi |
| `04` 9.9.22 | Tutar bazlı kısmi geri ödeme: tutar ve isteğe bağlı kalem — açık "mal dönmedi" kapanışı varsa kapanış da seçilir (K-1105); ödenmiş ve geri ödenmemiş tutarı aşamaz; havalede önce IBAN; onay ister | UC-133 · UC-134 · konvansiyon — API2-07 | eşlendi |
| `04` 9.9.23 | IBAN istemek — havale hattında IBAN'ı olmayan geri ödeme için; kalem ve sebep seçilir; onay yok; IBAN isteği, B-14, ana sayfanın listesinde satır; yönetici IBAN girmez | UC-134 · UC-86 | eşlendi |
| `04` 9.9.24 | Kart iadesini yeniden denemek — bir geri ödemenin işlenmesi, onay ister, IBAN girildikten sonra yok; havale yolunu açmak onaysız — IBAN isteği ve B-14; sonucu beklenen iadede yalnız "Gerçekleşmedi olarak işaretle" — onay yok, işaret döner, sorgu durur, işlem izine yazılır (K-940) | UC-135 · UC-136 · UC-137 · konvansiyon — API8-01 | eşlendi |
| `04` 9.9.25 | Kalem çıkarmak ya da adedini azaltmak: kalemin satırında, adet alanı (üst sınır açık adet); sebep yok; kart hattında karta para gönderen çıkarma onay ister; çıkarma kaydı, B-11, son açık kalemde kapanış | UC-138 · konvansiyon — API9-03 | eşlendi |
| `04` 9.9.26 | Hizmet kalemini tamamlamak: ödeme onaylıyken; adet seçilmez (K-792); onay yok; ödenmemiş siparişte yok | UC-139 · konvansiyon — API8-01 | eşlendi |
| `04` 9.9.27 | İade malını teslim almak ya da reddetmek: ulaşma tarihi ve ulaşan adet; koşullu istisnada "iade reddedildi — koruyucu ambalaj açılmış" seçimi, ret onay ister; ardından "Stoğa ekle" — teslim alınan adet kadar, varyant silinmişse ve retta yok; havalede IBAN silinmişse IBAN isteği | UC-140 · UC-141 · konvansiyon — API9-03 | eşlendi |
| `04` 9.9.28 | "Mal dönmedi" kapanışı: Z-42 geçmiş ve ulaşmamış adet varken; adet alanı; onay yok; geri ödeme yapılmaz (K-596); mal sonradan ulaşırsa teslim alma kalemi yeniden açar | UC-142 · UC-140 | eşlendi |
| `04` 9.9.29 | İndirme hakkını yenilemek: dijital kalemde; onay yok; sayaç sıfırlanır, bildirim yok (K-683) | UC-143 | eşlendi |
| `04` 9.9.30 | Ayıp talebini yürütmek: "Çözüldü olarak işaretle" ve Z-18 içinde "Yeniden aç" — onaysız, bildirimsiz; para gerektiren çözüm iade hattıyla, tutar bazlı geri ödemeyle ya da önce IBAN isteğiyle; ayıplı dijital kalemde çözüm dosya güncellemesi — ürün formunda | UC-144 · UC-145 · UC-140 · UC-131 · UC-133 · UC-134 · UC-83 | eşlendi |
| `04` 9.9.31 | Düzeltmeler — onaysız: adres ve teslimat telefonu (OB-13 panel varyantı, teyit hatırlatması, B-10, adres geçmişi) · kargo şirketi ve takip numarası (B-12) · teslim tarihi (sıra sorusu) ve ulaşma tarihi · durum geçişi (sebep, engel, B-13) · misafir siparişinin e-postası (yeni erişim anahtarı, B-1, B-15, işaret) | UC-146 · UC-147 · UC-148 · UC-149 · UC-150 · UC-151 · konvansiyon — API8-01 | eşlendi |
| `04` 9.9.32 | İç not: kaydetme onaysız; yazılması ve değiştirilmesi işlem izine yazılır, metni yazılmaz | UC-152 · konvansiyon — API2-04 | eşlendi |
| `04` 9.9.33 | E-postayı yeniden göndermek — işaretin altındaki satırdan; onaysız; gönderim sürerken düğme ikinci kez basılamaz; başarıda satır düşer, son satırla işaret kalkar; yine başarısızsa satır kalır ve mesaj — sonuç sayfa yeniden açılınca görünür; B-1 donmuş sürümüyle; siparişler arası toplu gönderim yok | UC-153 · UC-154 · konvansiyon — API2-05 | eşlendi |
| `04` 9.9.34 | Eşzamanlı değişiklik (OB-10, sipariş varyantı): yöneticinin gördüğü hâlden sonra sipariş değiştiyse işlem uygulanmaz, mesaj ve güncel hâl; açık pencere kapanır; üzerine yazma yok | konvansiyon — API2-04 · konvansiyon — API9-03 | konvansiyon |
| `04` 9.9.35 | Manuel adım bütçesi: ekran sipariş başına yeni zorunlu elle adım eklemez; hat başına sayılar değişmez | uç doğurmaz — yazım konvansiyonu | uç doğurmaz |
| `04` 9.9.36 | Validasyon: kargo bilgisi ya da araç beyanı zorunlu; tarihler OB-16'nın sınırlarında; adet 2.12.1.8'in sınırlarında; sebepsiz iptal ve düzeltme gönderilmez; aktarılan IBAN biçim ve sağlama basamağı; tutar sınırı; adres alanları; alanlar 7.2.7–7.2.15 | UC-124 · UC-125 · UC-126 · UC-129 · UC-130 · UC-133 · UC-138 · UC-140 · UC-142 · UC-146 · UC-148 · UC-149 · UC-150 · konvansiyon — API2-07 | eşlendi |
| `04` 9.9.37 | İşlemler iki eksenin hâline ve kalemin kayıtlarına göre açılır; ekran durum türetmez; tipe göre — fiziksel kalemsiz siparişte teslimat yok, dijitalde indirme sayacı ve hak yenileme, hizmette tamamlama; hatta göre — havalede "ödendi" ve IBAN satırları, kartta iadenin sonucu ve yeniden deneme | UC-92 · konvansiyon — API8-01 | eşlendi |
| `04` 9.9.38 | Ödenmemiş siparişte kalem düzeyinde iptal ve çıkarma yok; kendiliğinden iptal edilmiş ödenmemiş sipariş salt okunur — iç not, yeniden gönderim ve girişsiz siparişte e-posta düzeltmesi açık; durum düzeltmesi firmanın yanlış iptali için; satış kapısı açık siparişi etkilemez | UC-92 · konvansiyon — API8-01 | eşlendi |
| `04` 9.9.39 | Panel tek rol; her yönetici her işlemi yapar ve bütün veriyi görür | konvansiyon — API0-07 | konvansiyon |
| `04` 9.9.40 | Boş hâl yok, boş bölüm görünmez; bölümler kendi yerlerinde yüklenir; işlem sürerken düğme ikinci kez basılamaz ve sürdüğünü söyler | UC-92 | eşlendi |
| `04` 9.9.41 | Onay isteyen işlem sonuçsuz kalırsa ekran "işlem yapılmadı" demez, güncel hâli yükler ve sonucu gösterir (2.7.3.3); onaysız işlemde mesaj yerinde, form korunur; sayfa açılamazsa hata sayfası; e-postanın ulaşmaması işlemi durdurmaz | UC-92 · UC-78 · konvansiyon — API2-07 | eşlendi |
| `04` 9.9.42 | Responsive: iki sütun, tek sütun, işlem grupları açılır bölüm, onay penceresi tam ekran; iki rozet birleşmez | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-38 | Talepler — iletişim ve ayıp taleplerini tür sütunuyla tek listede gösterir, türe ve duruma göre süzdürür (§4.2) | UC-93 | eşlendi |
| `04` 9.10.1 | Aktör Yönetici; giriş menü, "açık talep" sayacı — süzülmüş hâliyle, ayrıntılardan dönüş; çıkış: iletişim talebi → E-39, ayıp talebi → E-37'nin ayıp talepleri bölümü | UC-93 · UC-94 · UC-92 · konvansiyon — API2-06 | eşlendi |
| `04` 9.10.2 | İlk görülen: en yeni açılan önce, tür ve durum süzgeçleri; sayaçtan gelindiyse "Açık" seçili; birincil düğme yok — talep panelden açılmaz | UC-93 · konvansiyon — API2-06 | eşlendi |
| `04` 9.10.3 | Süzgeçler: tür — İletişim talebi · Ayıp talebi; durum — Açık · Kapatıldı · Çözüldü, çoklu; ikisi birlikte | UC-93 · konvansiyon — API2-06 | eşlendi |
| `04` 9.10.4 | Tablo — iki türün satırı aynı sütunlarda: tür, talebin özü, açılış ve son açılış tarihi, durum rozeti, ayıp talebinde B-9 ulaşmadıysa işaret | UC-93 | eşlendi |
| `04` 9.10.5 | Sayfa gezinmesi — sayfa başına 25 | UC-93 · konvansiyon — API2-06 | eşlendi |
| `04` 9.10.6 | Sıra: açılış tarihine göre en yeni önce, yeniden açılan ayıp talebi son açılışla; sıralama seçeneği yok | UC-93 · konvansiyon — API2-06 | eşlendi |
| `04` 9.10.7 | Yoktur: arama ve konu tipi süzgeci, panelden yanıt, ayrı şikâyet listesi, ayıp talebinin ayrı ekranı, listeden kapatma ve toplu kapatma, listeden dışa aktarma | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 9.10.8 | Süzmek — ilk sayfaya döner; seçili süzgeçlerin adı listenin başında, tek dokunuşla kaldırılır | UC-93 · konvansiyon — API2-06 | eşlendi |
| `04` 9.10.9 | Talebi açmak — iletişim talebi → E-39, ayıp talebi → E-37'nin bölümü, talebin satırına odaklanmış; ekranın kendi işlemi yok | UC-94 · UC-92 | eşlendi |
| `04` 9.10.10 | Validasyon: süzgeçler kapalı değerler, serbest metin yok; alanlar 7.2.3 | UC-93 · konvansiyon — API2-06 | eşlendi |
| `04` 9.10.11 | "Açık" süzgeciyle liste sayacın kapsamıdır; talep saklama süresinin sonunda düşer (Z-33); veri toplayan girişlerin kapısı kapalıyken yeni iletişim talebi gelmez | UC-93 · UC-86 | eşlendi |
| `04` 9.10.12 | Hiç talep yoksa boş hâl, ilk kaydı açan düğme yok; süzgeç sonucu boşsa kaldırma yolu; sayfa açılamazsa hata sayfası | UC-93 · UC-78 | eşlendi |
| `04` 9.10.13 | Responsive: ikinci satır alta, dar sınıfta kart listesi, süzgeçler açılır bölümde | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-39 | İletişim talebi ayrıntısı — talebi okutur, kapattırır ve yeniden açtırır (§4.2) | UC-94 · UC-155 · UC-156 | eşlendi |
| `04` 9.11.1 | Aktör Yönetici; giriş Talepler'in satırı, oturumsuz gelişte girişten sonra; çıkış: listeye dönüş, "Siparişlerde ara" → E-36 ve "Üye kaydında ara" → E-40, talebin e-postasıyla aranmış | UC-94 · UC-93 · UC-91 · UC-95 · konvansiyon — API2-03 · konvansiyon — API2-06 | eşlendi |
| `04` 9.11.2 | İlk görülen: konu tipi, rozet, mesaj; birincil düğme Açık'ta "Talebi kapat", Kapatıldı'da "Yeniden aç" | UC-94 · konvansiyon — API8-01 | eşlendi |
| `04` 9.11.3 | Başlık: konu tipi, rozet, ulaştığı an — KVKK talebinde başvurunun ulaştığı an —, son kapatılış tarihi ve sürenin ondan işlediği — yeniden açma süreyi durdurmaz (K-1099), "Listeye dön" | UC-94 · UC-93 | eşlendi |
| `04` 9.11.4 | Gönderen: ad, e-posta — kopyalanabilen metin —, telefon; altında iki arama bağlantısı | UC-94 · UC-91 · UC-95 · uç doğurmaz — istemci | eşlendi |
| `04` 9.11.5 | Mesaj — metnin tamamı satır sonlarıyla, kısaltılmaz; sipariş numarası mesajın içinde | UC-94 | eşlendi |
| `04` 9.11.6 | İşlem: "Talebi kapat" ya da "Yeniden aç" | UC-155 · UC-156 | eşlendi |
| `04` 9.11.7 | Yoktur: yanıt yazma, yazışma dizisi, şablon, dosya eki, KVKK modülü, "e-posta ulaşmadı" işareti, elle silme, numara ve durum sorgusu | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 9.11.8 | Kapatmak ve yeniden açmak: onaysız, kısa bildirim, müşteriye bildirim yok; kapatma süreyi son kapatılıştan başlatır, yeniden açılan Açık'a ve sayaca döner; başka yönetici durumu değiştirmişse işlem tekrarlanmaz ve güncel durum gösterilir — sürüm yok, değere karşı | UC-155 · UC-156 · konvansiyon — API2-04 | eşlendi |
| `04` 9.11.9 | Siparişlerde ve üye kaydında aramak — E-36 ve E-40 talebin e-postasıyla aranmış açılır; sipariş işlemi siparişin ayrıntısında yapılır; hesabın talep üzerine silinmesi E-40'ta; teyit firmanın yükümlülüğü | UC-91 · UC-95 · UC-92 · UC-157 · konvansiyon — API2-06 | eşlendi |
| `04` 9.11.10 | Validasyon: form yok | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 9.11.11 | Açık'ta "Talebi kapat", Kapatıldı'da "Yeniden aç"; konu tipi düzeni değiştirmez; satış kapısı etkilemez | UC-94 · konvansiyon — API8-01 | eşlendi |
| `04` 9.11.12 | Boş hâl yok; imha edilmiş talepte ekran talebin bulunmadığını söyler ve listeye götürür; durum değişikliği sürerken düğme ikinci kez basılamaz; sayfa açılamazsa hata sayfası | UC-94 · UC-93 · UC-78 · konvansiyon — API2-07 | eşlendi |
| `04` 9.11.13 | Responsive: başlık ve gönderen yan yana ya da alt alta; uzun mesaj satır kırar | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-40 | Üye kaydı görünümü — üyeyi e-postayla aratır, kaydını salt okunur gösterir, talep üzerine sildirir (§4.2) | UC-95 · UC-157 | eşlendi |
| `04` 9.12.1 | Aktör Yönetici; giriş menü ve iletişim talebinin "Üye kaydında ara"sı — talebin e-postasıyla; çıkış bağlı siparişin satırı → E-37 | UC-95 · UC-92 · konvansiyon — API2-06 | eşlendi |
| `04` 9.12.2 | İlk görülen: e-posta arama alanı, birincil "Ara"; hesap bulunduysa e-posta, ad, giriş yolları | UC-95 | eşlendi |
| `04 §9.12` m1 | Bilgi hiyerarşisi: arama ve tek hesabın salt okunur kaydı | UC-95 | eşlendi |
| `04` 9.12.3 | Arama — tek alan e-posta; müşteri hesaplarında tam eşleşme, harf ayrımı yok; yönetici hesapları aranmaz; tek hesap ya da hiçbiri — sorgu parametresi | UC-95 · konvansiyon — API2-06 | eşlendi |
| `04` 9.12.4 | Hesap: e-posta, ad, doğrulanmamış kayıtta satır metni, giriş yolları — şifre var ya da yok, Google bağlı ya da değil | UC-95 | eşlendi |
| `04` 9.12.5 | Adres defteri — kayıtlı adresler salt okunur; yoksa tek satır | UC-95 | eşlendi |
| `04` 9.12.6 | Bağlı siparişler en yeni önce — numara, tarih, toplam, iki rozet; sayfa başına 25; satır E-37'yi açar | UC-95 · UC-92 · konvansiyon — API2-06 | eşlendi |
| `04` 9.12.7 | Hesabı sil — silmenin sonucu ve düğme; geri alma bağlantısı açıkken düğme kapalı, sebebi ve bitiş tarihi yerinde (K-698) | UC-95 · UC-157 · konvansiyon — API8-01 | eşlendi |
| `04` 9.12.8 | Yoktur: kaydın düzenlenmesi, üye listesi ve adla arama, uygulama içi veri indirme, görünümden dışa aktarma, yönetici hesabının aranması | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 9.12.9 | Aramak — onaysız; bulunamazsa K-811'in satırı; arama işlem izine yazılmaz | UC-95 · konvansiyon — API2-06 | eşlendi |
| `04` 9.12.10 | Hesabı silmek: onay penceresi (geri alınamaz) K-797'nin cümlesiyle; hesap `02 §3.15.1`'in rejimiyle silinir, "hesabınız silindi" e-postası, işlem izi; ekran arama alanına döner, yerinde kalıcı mesaj — kısa bildirim değil | UC-157 · UC-95 · konvansiyon — API2-02 | eşlendi |
| `04` 9.12.11 | Bağlı siparişi açmak → E-37 | UC-92 | eşlendi |
| `04` 9.12.12 | Validasyon: e-posta biçimi; ret alan mesajı; alanlar 7.2.3 | UC-95 · konvansiyon — API2-07 | eşlendi |
| `04` 9.12.13 | Sonuç üç hâlli — hesap yok, doğrulanmamış, doğrulanmış; giriş yollarına göre; geri alma bağlantısı açıkken (Z-45) silme kapalı; yürüyen sipariş silmeyi engellemez | UC-95 · UC-157 · konvansiyon — API8-01 | eşlendi |
| `04` 9.12.14 | Arama yapılmadan yalnız alan; bulunamama hata değil; sonuç kendi yerinde; silmenin onayı sonuçsuz kalırsa arama yenilenir ve sonuç gösterilir (2.7.3.3); sayfa açılamazsa hata sayfası | UC-95 · UC-157 · UC-78 · konvansiyon — API2-07 | eşlendi |
| `04` 9.12.15 | Responsive: bölümler yan yana ya da alt alta, bağlı siparişler kart listesi, "Hesabı sil" en altta | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-41 | Kurumsal içerik — içerik tiplerinin, genel sayfaların ve duyurunun kayıtlarını listeler ve düzenletir (§4.2) | UC-96 · UC-158 · UC-159 · UC-160 · UC-161 | eşlendi |
| `04` 9.13.1 | Aktör Yönetici; giriş menü ve "Taslak" bandının "Düzenlemeye dön"ü; çıkış: kayıt satırı ya da yeni kayıt → kaydın formu, "Önizle" ya da "Sitede gör" → E-08, E-09, E-10 ya da duyuru şeridi | UC-96 · UC-09 · UC-10 · UC-11 · UC-12 · UC-13 · UC-14 | eşlendi |
| `04` 9.13.2 | İlk görülen: tipe göre gruplanmış liste ve rozetler; her grup kendi "Yeni"si, tekil iki kayıt kendi satırından; formda birincil "Kaydet" | UC-96 · UC-158 | eşlendi |
| `04 §9.13` m1 | Bilgi hiyerarşisi iki görünümdür — liste ve kaydın formu | UC-96 · konvansiyon — API2-02 | eşlendi |
| `04` 9.13.3 | Liste — yedi grup bu sırayla; satır: ad ya da başlık, rozet, "ana sayfada göster" ya da "menüde göster" işareti, elle sıranın "yukarı" ve "aşağı"sı; duyurunun görünürlüğü; işaretli kayıt sayısı; gruplar sayfalanmaz (K-843) | UC-96 · UC-161 | eşlendi |
| `04` 9.13.4 | Kaydın formu — tipe göre alanlar, yayın için zorunlu alanlar etiketli; uzun metinler kapalı biçim setiyle; görseller OB-19'un galerisi; ilgili ürünler adla aranarak seçilir, seçim sırasıyla (K-1103), Yayında olmayan işaretli | UC-96 · UC-158 · UC-117 · konvansiyon — API4-07 | eşlendi |
| `04 §9.13` m2 | Hakkımızda: kısa tanıtım düz metin (P-29) yayın için zorunlu, uzun metin, görseller | UC-96 · UC-158 | eşlendi |
| `04 §9.13` m3 | Hizmet tanıtımı: ad (P-27) yayın için zorunlu, kısa açıklama (P-28), metin, görseller, ilgili ürünler, "ana sayfada göster" | UC-96 · UC-158 | eşlendi |
| `04 §9.13` m4 | Referans iş: başlık (P-27) yayın için zorunlu, kısa açıklama, metin, görseller, ilgili ürünler, "ana sayfada göster"; müşteri adı başlıkta | UC-96 · UC-158 | eşlendi |
| `04 §9.13` m5 | Sık sorulan soru: soru (P-28) ve cevap, ikisi de yayın için zorunlu | UC-96 · UC-158 | eşlendi |
| `04 §9.13` m6 | Şube: ad ve adres — il kapalı listeden — yayın için zorunlu, telefon, çalışma saatleri, görsel, harita bağlantısı | UC-96 · UC-158 | eşlendi |
| `04 §9.13` m7 | Genel sayfa: başlık (P-27) yayın için zorunlu, metin, görseller, "menüde göster" | UC-96 · UC-158 | eşlendi |
| `04 §9.13` m8 | Duyuru: metin (P-30) yayın için zorunlu, isteğe bağlı bağlantı, başlangıç ve bitiş (OB-16) | UC-96 · UC-158 · konvansiyon — API2-09 | eşlendi |
| `04` 9.13.5 | Yayın durumu yan sütunu: rozet, yayın kapısının alanları, "Önizle" ya da "Sitede gör", "Yayına al" ya da "Taslağa al", "Kalıcı olarak sil" — Hakkımızda'da yok —, kendi sayfası olan kayıtta ilk yayında sabitlenen adres | UC-96 · UC-159 · UC-160 · UC-09 · UC-10 · UC-11 · UC-12 · konvansiyon — API8-01 · konvansiyon — API2-01 | eşlendi |
| `04` 9.13.6 | Yoktur: sayfa kurucu, galeri, blog ve müşteri görüşü tipi, ikinci Hakkımızda ve duyuru, gömülü video ve dosya eki, ileri tarihli yayın, sürüm geçmişi, arşiv durumu, listede arama, şube ve SSS sayfası, ayarlarla çelişen metnin denetimi | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 9.13.7 | Kaydetmek: yeni kayıt ilk kayıtta Taslak, adla — Hakkımızda ve duyuru boş da; yayındaki kaydın düzenlemesi anında yayında; yayın kapısını bozan düzenleme kaydedilmez, mesaj alanı adıyla söyler; metin düzenlemesi işlem izine yazılmaz | UC-158 · konvansiyon — API2-07 | eşlendi |
| `04` 9.13.8 | Yayına almak ve taslağa almak: kapı eksikse alınmaz, eksik alanlar yan sütunda; ikinci onay yok; taslağa alınan kayıt menüden, ana sayfadan ve listelerden kalkar; işlem izine yazılır | UC-159 · konvansiyon — API2-07 | eşlendi |
| `04` 9.13.9 | Önizlemek: kendi sayfası olan kayıt E-08'de "Taslak" bandıyla; SSS sorusu, şube ve duyuru göründükleri yerde "Taslak" etiketiyle | UC-09 · UC-10 · UC-11 · UC-12 · UC-13 · UC-14 · konvansiyon — API2-03 · konvansiyon — API5-01 | eşlendi |
| `04` 9.13.10 | Kalıcı olarak silmek: onay penceresi (geri alınamaz); silinen kaydın sayfası "sayfa bulunamadı", bağlar kalkar, adres serbest kalır; Hakkımızda'da yok — taslağa alma sunulur | UC-160 · UC-06 · konvansiyon — API2-02 | eşlendi |
| `04` 9.13.11 | Elle sıralamak — "yukarı" ve "aşağı" grubun içinde bir sıra kaydırır, anında kaydedilir; klavyeyle; "ana sayfada göster" ve "menüde göster" formda değişir, sıradan bağımsız; onay yok | UC-161 · UC-158 · konvansiyon — API2-04 | eşlendi |
| `04` 9.13.12 | Eşzamanlı düzenleme (OB-10, kayıt varyantı): "Güncel hâli aç" · "Yine de kaydet" | UC-158 · konvansiyon — API2-04 | eşlendi |
| `04` 9.13.13 | Validasyon: karakter tavanları P-27…P-30, görsel sınırları, il kapalı listeden, duyurunun bitişi başlangıçtan önce olamaz, bağlantılar adres biçimiyle; taslağı adsız bırakmak engeldir; alanlar 7.2.16 | UC-158 · UC-117 · konvansiyon — API2-07 | eşlendi |
| `04` 9.13.14 | Her kayıt kendi yayın durumunu taşır — Taslak'ta "Önizle" ve "Yayına al", Yayında'da "Sitede gör" ve "Taslağa al"; aralığın dışındaki duyuru sitede görünmez ve satırı söyler; Hakkımızda boşken kurumsal bloğun geri düşüşü vitrinin kuralı | UC-96 · konvansiyon — API8-01 | eşlendi |
| `04` 9.13.15 | İlk kurulumda site boş: Hakkımızda ve duyuru satırları boş formu açar, öteki gruplar boş hâlle; kurumsal taraf satış kapısına bağlı değil | UC-96 · UC-158 | eşlendi |
| `04` 9.13.16 | Boş grup boş hâl ve "Yeni"; görsel yüklemesi sürerken form kaydedilemez; kayıt tamamlanamazsa form korunur; silme onayı sonuçsuz kalırsa liste yenilenir (2.7.3.3); sayfa açılamazsa hata sayfası | UC-96 · UC-158 · UC-117 · UC-78 · konvansiyon — API2-07 | eşlendi |
| `04` 9.13.17 | Responsive: tek sütun, yan sütun formun başına, kart listesi, "yukarı" ve "aşağı" kartın menüsünde | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-42 | Ana sayfa ve menü — ana sayfa düzenini seçtirir, menü adlarını ve sosyal bağlantıları düzenletir (§4.2) | UC-97 · UC-162 | eşlendi |
| `04` 9.14.1 | Aktör Yönetici; giriş menü; çıkış çerçevenin geçişleri ve "Siteyi görüntüle" | UC-97 · UC-01 | eşlendi |
| `04` 9.14.2 | İlk görülen: seçili düzen; birincil "Kaydet" | UC-97 | eşlendi |
| `04 §9.14` m1 | Bilgi hiyerarşisi: tek form, üç bölüm, tek "Kaydet" (K-813) | UC-97 · UC-162 | eşlendi |
| `04` 9.14.3 | Ana sayfa düzeni — tanıtım öncelikli · mağaza öncelikli, blok sıraları yazılı; kurulumda tanıtım öncelikli | UC-97 · UC-162 | eşlendi |
| `04` 9.14.4 | Menü adları — sabit iskeletin öğeleri seçili düzenin sırasıyla, her birinde ad alanı, varsayılan ad ve görünmüyorsa sebebi; "menüde göster" işaretli genel sayfalar salt okunur | UC-97 · UC-162 | eşlendi |
| `04` 9.14.5 | Sosyal bağlantılar — kapalı platform listesi ve WhatsApp numarası; üst bölümde ve altbilgide görünür | UC-97 · UC-162 | eşlendi |
| `04` 9.14.6 | Yoktur: serbest sayfa kurgusu, menü düzenleyici, ana sayfa için ürün seçme, kayan afiş, gömülü akış ve sohbet penceresi, listenin dışında platform | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 9.14.7 | Kaydetmek: onaysız, kısa bildirim, vitrinde hemen geçerli; değişen her ayar eski ve yeni değerle işlem izine; sosyal bağlantılar siparişe donmaz | UC-162 | eşlendi |
| `04` 9.14.8 | Eşzamanlı düzenleme (OB-10, kayıt varyantı) — ana sayfa ve menünün bölüm sürümü | UC-162 · konvansiyon — API2-04 | eşlendi |
| `04` 9.14.9 | Validasyon: menü adı boş kaydedilmez, mesaj varsayılan adı hatırlatır; bağlantılar adres, WhatsApp telefon biçimiyle; alanlar 7.2.17 | UC-162 · konvansiyon — API2-07 | eşlendi |
| `04` 9.14.10 | Menü öğesinin görünürlüğü içerikten ve katalogdan okunur; düzen ana sayfanın ve menünün sırasını değiştirir; satış kapısı etkilemez | UC-97 · UC-01 · konvansiyon — API5-01 | eşlendi |
| `04` 9.14.11 | Boş hâl yok — ilk kurulumda varsayılanlar; kayıt tamamlanamazsa form korunur; sayfa açılamazsa hata sayfası | UC-97 · UC-162 · UC-78 | eşlendi |
| `04` 9.14.12 | Responsive: bölümler alt alta, tek sütun | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-43 | Marka — logo, marka adı, site simgesi, marka rengi ve platform imzası (§4.2) | UC-98 · UC-163 · UC-117 | eşlendi |
| `04` 9.15.1 | Aktör Yönetici; giriş menü; çıkış çerçevenin geçişleri ve "Siteyi görüntüle" | UC-98 · UC-01 | eşlendi |
| `04` 9.15.2 | İlk görülen: marka adı ve logo; birincil "Kaydet" | UC-98 | eşlendi |
| `04 §9.15` m1 | Bilgi hiyerarşisi: tek form, beş bölüm, tek "Kaydet"; marka vitrine ve e-postalara uygulanır, siparişe donmaz | UC-98 · UC-163 | eşlendi |
| `04` 9.15.3 | Marka adı — zorunlu, girildikten sonra boşaltılamaz; girilene kadar alan adı gösterilir; üst bölümde, sekmede ve e-postalarda | UC-98 · UC-163 · konvansiyon — API5-01 | eşlendi |
| `04` 9.15.4 | Logo — isteğe bağlı tek görsel (OB-19); yoksa marka adı yazıyla | UC-98 · UC-117 · UC-163 | eşlendi |
| `04` 9.15.5 | Site simgesi — isteğe bağlı tek görsel; yoksa sistemin simgesi örnek olarak | UC-98 · UC-117 · UC-163 | eşlendi |
| `04` 9.15.6 | Marka rengi — tek renk, boş bırakılabilir; yanında renk örneği; vitrinde yalnız dolu zemin | UC-98 · UC-163 | eşlendi |
| `04` 9.15.7 | Platform imzası — açık ya da kapalı, varsayılan açık, yalnız altbilgide | UC-98 · UC-163 | eşlendi |
| `04` 9.15.8 | Yoktur: yazı tipi, düzen ve başka renk seçimi, koyu tema, ikinci renk ve e-posta logosu, siteye kod ekleme, panelin marka rengiyle boyanması | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 9.15.9 | Kaydetmek: onaysız, kısa bildirim, vitrinde hemen geçerli; eski ve yeni değerle işlem izine | UC-163 | eşlendi |
| `04` 9.15.10 | Görsel yüklemek ve kaldırmak — tek görsel varyantı; yükleme sürerken form kaydedilemez; kaldırılanın yerine geri düşüş | UC-117 · UC-163 · konvansiyon — API4-07 | eşlendi |
| `04` 9.15.11 | Eşzamanlı düzenleme (OB-10, kayıt varyantı) | UC-163 · konvansiyon — API2-04 | eşlendi |
| `04` 9.15.12 | Validasyon: marka adı boşaltılamaz; logo ve simge P-22, P-23'ün sınırlarında; alanlar 7.2.18 | UC-163 · UC-117 · konvansiyon — API2-07 | eşlendi |
| `04` 9.15.13 | Girilmemiş bölüm geri düşüşünü gösterir; satış kapısı etkilemez, marka kontrol listesinde değil | UC-98 | eşlendi |
| `04` 9.15.14 | İlk kurulumda alanlar boş, imza açık; kayıt tamamlanamazsa form korunur; sayfa açılamazsa hata sayfası | UC-98 · UC-163 · UC-78 | eşlendi |
| `04` 9.15.15 | Responsive: örnekler yanda ya da alanın altında | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-44 | Firma kimliği ve satış — satış durumunu ve kapının koşullarını gösterir, satışı geçici olarak kapattırır, firma tipine göre kimlik ve iletişim bilgilerini girdirir (§4.2) | UC-99 · UC-164 · UC-165 · UC-166 | eşlendi |
| `04` 9.16.1 | Aktör Yönetici; giriş menü, üst satırın satış durumu göstergesi, ana sayfanın uyarıları, kontrol listesinin kimlik maddesi, yasal metinlerin eksik kimlik alanı bağlantısı; çıkış: karşılanmayan koşulun bağlantısı → kimlik formu, E-45, E-46 ya da E-47 | UC-99 · UC-100 · UC-101 · UC-102 | eşlendi |
| `04` 9.16.2 | İlk görülen: "Satış durumu" — açık mı kapalı mı ve sebebi; birincil kimlik formunun "Kaydet"i; geçici kapatma anahtarı kendi onayıyla, formdan bağımsız (K-752) | UC-99 · UC-164 · UC-165 | eşlendi |
| `04 §9.16` m1 | Bilgi hiyerarşisi iki bölümdür — satış durumu ve firma kimliği (K-752) | UC-99 | eşlendi |
| `04` 9.16.3 | Satış durumu: "Satış açık" ya da "Satış kapalı", kapının koşulları tek tek ve karşılanmayan adıyla, düzeltileceği ekranın bağlantısıyla; yasal metin koşulunda veri toplayan girişlerin kapısı; geçici kapatma anahtarı — "Satışı geçici olarak kapat" ya da "Satışı yeniden aç" | UC-99 · UC-165 · UC-166 · UC-100 · UC-101 · UC-102 · konvansiyon — API8-01 | eşlendi |
| `04` 9.16.4 | Firma kimliği — bölümlü form; kimliğin her siparişe o günkü hâliyle yazıldığı ve değişikliğin iki belgenin yeni sürümünü ürettiği | UC-99 · UC-164 | eşlendi |
| `04 §9.16` m2 | Firma tipi — Esnaf · Gerçek kişi tacir · Tüzel kişi; tip seçilmeden alanlar açılmaz; tip değişirken yeni tipin zorunlu alanları dolana kadar satışın kapalı kalacağı | UC-99 · UC-164 | eşlendi |
| `04 §9.16` m3 | Tipin zorunlu alanları — esnafta ad-soyad ve VKN; tacirde unvan, MERSİS, sicil numarası; tüzel kişide unvan, MERSİS, sicil numarası | UC-99 · UC-164 | eşlendi |
| `04 §9.16` m4 | Üç tipte zorunlu: adres — il kapalı listeden —, telefon, e-posta — firmaya giden bildirimlerin adresi —, KEP adresi | UC-99 · UC-164 | eşlendi |
| `04 §9.16` m5 | İsteğe bağlı: meslek odası, işletme adı ya da marka, davranış kuralları; satış kapısını tutmaz | UC-99 · UC-164 | eşlendi |
| `04 §9.16` m6 | ETBİS doğrulama bilgisi — isteğe bağlı bağlantı; yanında yükümlülüğün hatırlatması (K-815); boşken bant görünmez | UC-99 · UC-164 | eşlendi |
| `04` 9.16.5 | Yoktur: kimliğin silinmesi, zorunlu alanın boşaltılması, bakım modu, ürünün içinden ETBİS kaydı, kurulum ayarlarının panelden değişmesi, kimliğin doğruluk denetimi | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 9.16.6 | Kimliği kaydetmek: onaysız, kısa bildirim; dolu zorunlu alanı boşaltan kayıt kaydedilmez; tip değişikliği kaydedilir, kapı yeni seti denetler; eski ve yeni değerle işlem izi; satış durumu bölümü ve gösterge hemen güncellenir | UC-164 · konvansiyon — API2-07 | eşlendi |
| `04` 9.16.7 | Satışı geçici olarak kapatmak: onay penceresi açık siparişlerin etkilenmeyeceğini söyler, "Satışı kapat" (K-814); sepete ekleme ve ödeme kapanır, vitrin yayında, sepet korunur, açık siparişler yürür; işlem izine yazılır | UC-165 · konvansiyon — API2-02 | eşlendi |
| `04` 9.16.8 | Satışı yeniden açmak: onaysız; öteki koşullar karşılanıyorsa satış açılır, değilse anahtar kapanır ama satış kapalı kalır — anahtar tek başına açmaz | UC-166 · konvansiyon — API8-01 | eşlendi |
| `04` 9.16.9 | Eşzamanlı düzenleme (OB-10) kimlik formunda; anahtar o arada değiştiyse işlem tekrarlanmaz, güncel hâl — anahtar sürüm taşımaz, değere karşı yazılır (K-1021) | UC-164 · UC-165 · UC-166 · konvansiyon — API2-04 | eşlendi |
| `04` 9.16.10 | Validasyon: zorunlu alanlar boşaltılamaz, e-posta ve KEP e-posta biçimiyle, bağlantılar adres biçimiyle, il kapalı listeden; numaraların doğruluğu denetlenmez; alanlar 7.2.19 | UC-164 · konvansiyon — API2-07 | eşlendi |
| `04` 9.16.11 | İlk kurulumda kimlik boş ve tip seçili değil; bölüm satışın kapalı olduğunu ve karşılanmayan koşulları kontrol listesiyle aynı adlarla gösterir; kurumsal taraf kapıya bağlı değil | UC-99 · UC-86 | eşlendi |
| `04` 9.16.12 | Satış kapalıyken bölüm sebebi adıyla — geçici kapatma ya da karşılanmayan koşul, ikisi birlikte de; koşulun düşmesi açık siparişleri etkilemez | UC-99 · konvansiyon — API8-01 | eşlendi |
| `04` 9.16.13 | Boş hâl yok; kayıt tamamlanamazsa form korunur; anahtarın onayı sonuçsuz kalırsa bölüm yenilenir ve güncel durum (2.7.3.3); sayfa açılamazsa hata sayfası | UC-99 · UC-164 · UC-165 · UC-78 · konvansiyon — API2-07 | eşlendi |
| `04` 9.16.14 | Responsive: "Satış durumu" en üstte, koşul satırları alt alta, form tek sütun | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-45 | Ödeme yöntemleri — havaleyi açtırır, kapattırır ve IBAN'ı girdirir (§4.2) | UC-100 · UC-167 | eşlendi |
| `04` 9.17.1 | Aktör Yönetici; giriş menü, kontrol listesinin ödeme maddesi, "Satış durumu"nun koşulu, ana sayfanın uyarısı; çıkış çerçevenin geçişleri | UC-100 | eşlendi |
| `04` 9.17.2 | İlk görülen: iki yöntemin açık ya da kapalı olduğu; birincil "Kaydet" | UC-100 · UC-167 | eşlendi |
| `04 §9.17` m1 | Bilgi hiyerarşisi: tek form | UC-100 · UC-167 | eşlendi |
| `04` 9.17.3 | Kart — salt okunur: sağlayıcı anahtarları kurulumda tanımlıysa açık, değilse ödeme adımında görünmez | UC-100 | eşlendi |
| `04` 9.17.4 | Havale/EFT — anahtar ve IBAN, açıkken IBAN zorunlu; IBAN siparişe onay anındaki hâliyle yazılır, girilmesi ve değişmesi bütün yöneticilere bildirilir; havale süresi E-46'da; serbest metin hatırlatması | UC-100 · UC-167 · UC-101 | eşlendi |
| `04` 9.17.5 | Yoktur: kapıda ödeme, kartın panelden açılıp kapatılması ve anahtarların girilmesi, birden çok IBAN, bekleyen siparişlerin IBAN'ının değiştirilmesi | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 9.17.6 | Havaleyi açmak ve ilk IBAN'ı girmek: onaysız; IBAN boşken havale açılmaz (K-817); ilk giriş de bildirilir; eski ve yeni değerle işlem izi | UC-167 · konvansiyon — API2-07 | eşlendi |
| `04` 9.17.7 | IBAN'ı değiştirmek: onay penceresi bekleyen havale siparişlerinin sayısını ve eski IBAN'ı göstermeye devam edeceklerini söyler (K-663); yeni IBAN yalnız yeni siparişlere; gerekirse o siparişler firma iptaliyle kapatılır | UC-167 · UC-129 · konvansiyon — API2-02 | eşlendi |
| `04` 9.17.8 | Havaleyi kapatmak: onay penceresi bekleyen siparişlerin etkilenmeyeceğini ve başka açık yöntem yoksa satışın kapanacağını söyler (K-664); bekleyen sipariş donmuş IBAN'ıyla sürer | UC-167 · konvansiyon — API2-02 | eşlendi |
| `04` 9.17.9 | Eşzamanlı düzenleme (OB-10, kayıt varyantı) — ödeme yöntemlerinin bölüm sürümü | UC-167 · konvansiyon — API2-04 | eşlendi |
| `04` 9.17.10 | Validasyon: IBAN biçim ve sağlama basamağı (K-817), havale açıkken IBAN boşaltılamaz; alanlar 7.2.20 | UC-167 · konvansiyon — API2-07 | eşlendi |
| `04` 9.17.11 | Kart tanımlı ya da tanımsız, havale açık ya da kapalı — dört hâl; ikisi kapalıysa satış kapısının koşulu karşılanmaz | UC-100 · UC-99 | eşlendi |
| `04` 9.17.12 | İlk kurulumda havale kapalı, IBAN boş; kart kurulumdan; kontrol listesinin ödeme maddesi buraya götürür | UC-100 · UC-86 | eşlendi |
| `04` 9.17.13 | Yetkisiz değişiklik şüphesinde IBAN burada düzeltilir; yapanı işlem izinden okur, eski IBAN'lı siparişleri firma iptaliyle kapatır, şifresini E-50'den değiştirir | UC-167 · UC-106 · UC-129 · UC-104 | eşlendi |
| `04` 9.17.14 | Boş hâl yok; kayıt tamamlanamazsa form korunur; onay sonuçsuz kalırsa güncel hâl yeniden yüklenir (2.7.3.3); sayfa açılamazsa hata sayfası | UC-100 · UC-167 · UC-78 · konvansiyon — API2-07 | eşlendi |
| `04` 9.17.15 | Responsive: tek sütun; IBAN dörderli gruplarla | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-46 | Kargo, süreler ve varsayılanlar — kargo ücreti ve eşikler, teslimat illeri, süreler, ürün varsayılanları, iade adresi (§4.2) | UC-101 · UC-168 · UC-169 · UC-170 · UC-171 · UC-172 | eşlendi |
| `04` 9.18.1 | Aktör Yönetici; giriş menü, kontrol listesinin iade adresi maddesi, "Satış durumu"nun koşulu, ana sayfanın uyarısı; çıkış çerçevenin geçişleri | UC-101 | eşlendi |
| `04` 9.18.2 | İlk görülen: beş bölüm dolu değerleriyle; birincil düğme yok — her bölüm kendi "Kaydet"i (K-752) | UC-101 · UC-168 · UC-169 · UC-170 · UC-171 · UC-172 | eşlendi |
| `04 §9.18` m1 | Bilgi hiyerarşisi: beş bölüm bu sırayla; her bölümün başında değerlerin siparişe donduğu ve yalnız yeni siparişlere işlediği | UC-101 | eşlendi |
| `04` 9.18.3 | Kargo ücreti ve eşikler: ücret (P-1), ücretsiz kargo eşiği (P-3), asgari sipariş tutarı (P-4) — boşken kapalı; kargonun KDV'si ayar değil | UC-101 · UC-168 · konvansiyon — API2-09 | eşlendi |
| `04` 9.18.4 | Teslimat illeri — çoklu seçim, varsayılan tüm Türkiye, "Tümünü seç" ve "Seçimi kaldır"; hiçbiri seçili değilse fiziksel siparişin verilemeyeceği; değişiklik üretilen metinlerin sürümünü artırır | UC-101 · UC-169 · uç doğurmaz — istemci | eşlendi |
| `04` 9.18.5 | Süreler: kargoya verme süresinin varsayılanı (P-5), havale ödeme süresi (P-7), iş günüyle | UC-101 · UC-170 | eşlendi |
| `04` 9.18.6 | Ürün varsayılanları: indirme hakkı (P-8), KDV varsayılanı (P-10); var olan ürünün oranı değişmez | UC-101 · UC-171 | eşlendi |
| `04` 9.18.7 | İade adresi — alıcı, il, ilçe, açık adres, isteğe bağlı telefon; zorunlu, satış kapısının koşulu, firma adresinden ayrı | UC-101 · UC-172 | eşlendi |
| `04` 9.18.8 | Yoktur: ağırlık, adet ya da ile göre kargo, kargo entegrasyonu, ilçe kısıtı, teslimat seçenekleri, çit dışı süre, kargonun KDV ayarı, ürün sabitlerinin değiştirilmesi | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 9.18.9 | Bölümü kaydetmek: onaysız, kısa bildirim; süre, ücret ve il değişikliği üretilen metinlerin sürümünü artırır; eski ve yeni değerle işlem izi; açık sepet değişikliği görür, onay anına denk gelirse özet farkı | UC-168 · UC-169 · UC-170 · UC-171 · UC-27 | eşlendi |
| `04` 9.18.10 | İade adresini girmek ve değiştirmek: ilk giriş onaysız; değişiklikte onay penceresi "iade malı bekleniyor" kalemlerin sayısıyla (K-665); yeni beyanlar güncel adresi alır | UC-172 · konvansiyon — API2-02 | eşlendi |
| `04` 9.18.11 | Eşzamanlı düzenleme (OB-10) — uyarı yalnız kaydedilen bölüm için; bölüm sürümleri (K-1014) | UC-168 · UC-169 · UC-170 · UC-171 · UC-172 · konvansiyon — API2-04 | eşlendi |
| `04` 9.18.12 | Validasyon: tutarlar sıfır ya da büyük, iki ondalık; süreler P-5 ve P-7'nin çitinde, çit dışı değer sebebiyle reddedilir (K-817); indirme hakkı en az bir; iade adresinin zorunlu alanları; alanlar 7.2.21 | UC-168 · UC-170 · UC-171 · UC-172 · konvansiyon — API2-07 | eşlendi |
| `04` 9.18.13 | İlk kurulumda ayarlar `02 §11`'in değerleriyle dolu, iade adresi boş — ekrandaki tek kapı koşulu; kontrol listesinin maddesi buraya | UC-101 · UC-86 | eşlendi |
| `04` 9.18.14 | İade adresi boşaltılamadığı için koşul düşmez; satış kapalıyken ekran aynı işler | UC-101 | eşlendi |
| `04` 9.18.15 | Boş hâl yok; her bölüm kendi yerinde kaydedilir, beklenmeyen hatada mesaj bölümün içinde; iade adresinin onayı sonuçsuz kalırsa güncel adres yeniden yüklenir (2.7.3.3); sayfa açılamazsa hata sayfası | UC-101 · UC-172 · UC-78 · konvansiyon — API2-07 | eşlendi |
| `04` 9.18.16 | Responsive: bölümler alt alta, il listesi açılır bölüm | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-47 | Yasal metinler — aydınlatma metnini ve çerez politikasını düzenletip yayımlatır, üretilen iki metni gösterir (§4.2) | UC-102 · UC-173 · UC-174 | eşlendi |
| `04` 9.19.1 | Aktör Yönetici; giriş menü, kontrol listesi, "Satış durumu"nun koşulu, ana sayfanın uyarısı; çıkış eksik kimlik alanının bağlantısı → E-44 | UC-102 · UC-99 | eşlendi |
| `04` 9.19.2 | İlk görülen: dört metnin satırları — yayındaki sürüm ve tarih ya da yayına alınmadığı; birincil düzenlenen metnin "Yayına al"ı | UC-102 · UC-174 | eşlendi |
| `04 §9.19` m1 | Bilgi hiyerarşisi: dört metin, iki grup (K-818) | UC-102 | eşlendi |
| `04` 9.19.3 | Firmanın düzenlediği iki metin — her biri bir bölüm: yayındaki sürüm ve tarih, tamamlanma satırı, düzenlenen metin, "Taslağı kaydet" ve "Yayına al"; yayındakinden farkı; ürünün taslağıyla başlar, kapalı biçim seti | UC-102 · UC-173 · UC-174 | eşlendi |
| `04 §9.19` m2 | Aydınlatma metni: yer tutucular kimlikteki değerlerle, eksik kimlik alanı adıyla ve E-44'e bağlantıyla; barındırma konumu ayrı zorunlu alan, alıcı grupları; yayına alınana kadar veri toplayan girişlerin kapalı olduğu | UC-102 · UC-173 · UC-99 | eşlendi |
| `04 §9.19` m3 | Çerez politikası: zorunlu çerezler ve sağlayıcının çerçevesi için yer tutucu; ilk yayınına kadar veri toplayan girişlerin kapalı olduğu ve vitrinin çerez yazmadığı | UC-102 · UC-173 | eşlendi |
| `04` 9.19.4 | Ayarlardan üretilen iki metin — Ön Bilgilendirme Formu ve sözleşme salt okunur: güncel sürüm, tarih, metin, siparişe göre değişen yerler yer tutucu; besleyen ayarlar sayılır; şablonun bakım sürümüyle de sürüm artar | UC-102 | eşlendi |
| `04` 9.19.5 | Yoktur: yayındaki metni yayından çekmek, ileri tarihli yayın ve yeniden onay, belgelerin elle düzenlenmesi, işlem rehberinin düzenlenmesi, ek yasal metin, eski sürümlerin listesi, çerez onay bandı | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 9.19.6 | Taslağı kaydetmek — yayındaki sürümü değiştirmez; onaysız, kısa bildirim; metin boşaltılamaz | UC-173 · konvansiyon — API2-07 | eşlendi |
| `04` 9.19.7 | Yayına almak: aydınlatma tamamlanmadan alınmaz; sürüm kendiliğinden artar, vitrinde güncel sürüm (E-11); yeniden onay ve bildirim yok; onay penceresi yok, yeni sürüm kalıcı gösterilir; işlem izine; iki metnin ilk yayını kapıların koşulu | UC-174 · UC-16 · UC-17 · konvansiyon — API2-07 | eşlendi |
| `04` 9.19.8 | Eşzamanlı düzenleme (OB-10, kayıt varyantı) — her metin kendi bölümünde | UC-173 · UC-174 · konvansiyon — API2-04 | eşlendi |
| `04` 9.19.9 | Validasyon: iki metin ve barındırma konumu boşaltılamaz; tamamlanma eksiği yerinde kalıcı; alanlar 7.2.22 | UC-173 · UC-174 · konvansiyon — API2-07 | eşlendi |
| `04` 9.19.10 | İlk kurulumda iki metin taslakla, hiç yayına alınmamış; vitrinde bağlantıları yok; tamamlanana ve yayına alınana kadar satış ve veri toplayan girişler kapalı | UC-102 · UC-16 · UC-17 · UC-86 | eşlendi |
| `04` 9.19.11 | Kimlik alanı sonradan değişirse yer tutucu yeni değeri gösterir, üretilen iki metnin sürümü artar; sürüm artışı onay adımı açık müşteride siparişi durdurur — sipariş onayının kuralı | UC-102 · UC-164 · UC-27 | eşlendi |
| `04` 9.19.12 | Boş hâl yok; yayına alma sürerken düğme ikinci kez basılamaz; beklenmeyen hatada metin korunur; sayfa açılamazsa hata sayfası | UC-102 · UC-174 · UC-78 · konvansiyon — API2-07 | eşlendi |
| `04` 9.19.13 | Responsive: satırlar alt alta, düzenleme alanı tam genişlik | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-49 | Yönetici hesapları — yöneticileri listeler, davet ettirir, daveti geri çektirir, yönetici kaldırtır (§4.2) | UC-103 · UC-175 · UC-176 · UC-177 | eşlendi |
| `04` 9.20.1 | Aktör Yönetici; giriş menü; davet, geri çekme ve kaldırma ekranda kalır | UC-103 | eşlendi |
| `04` 9.20.2 | İlk görülen: yöneticiler ve geçerli davetler; birincil "Yönetici davet et" | UC-103 · UC-175 | eşlendi |
| `04 §9.20` m1 | Bilgi hiyerarşisi: iki liste (K-820) | UC-103 | eşlendi |
| `04` 9.20.3 | Yöneticiler — ad, e-posta, kendi satırında "siz"; alfabetik; satırın işlemi "Yöneticiyi kaldır", kendi satırında yok | UC-103 · UC-177 · konvansiyon — API8-01 | eşlendi |
| `04` 9.20.4 | Davetler — yalnız geçerliler: adres, gönderen, tarih, son gün (Z-3), ulaşmadıysa işaret — yeniden gönder yok; işlemler "Daveti geri çek" ve "Yeni davet gönder"; geçersizleşen davet listeden çıkar; davet yoksa liste görünmez | UC-103 · UC-176 · UC-175 | eşlendi |
| `04` 9.20.5 | Davet formu ekranın içinde — e-posta ve "Daveti gönder"; adrese geçerli davet varsa yeni davetin onu geçersiz kılacağı | UC-103 · UC-175 | eşlendi |
| `04` 9.20.6 | Yoktur: rol ve yetki matrisi, pasifleştirme, kendi hesabını kaldırma, başkasının hesabını değiştirme, davet e-postasının yeniden gönderilmesi, ürünün içinden erişim kurtarma | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 9.20.7 | Davet göndermek: onaysız; adres bir yönetici hesabınınsa gönderilmez — müşteri hesabının adresi engel değil; yeni davet öncekini geçersiz kılar; satır görünür, kısa bildirim; işlem izine ve bütün yöneticilere (F-6) | UC-175 · konvansiyon — API2-07 | eşlendi |
| `04` 9.20.8 | Daveti geri çekmek: onaysız; bağlantı o anda geçersiz, satır çıkar; davetliye bildirim yok; işlem izine | UC-176 | eşlendi |
| `04` 9.20.9 | Yeni davet göndermek — "e-posta ulaşmadı"nın yolu: satırın adresiyle davet; yanlış adreste geri çekip doğru adrese davet, kullanılmış davette açılan hesabı kaldırmak | UC-175 · UC-176 · UC-177 | eşlendi |
| `04` 9.20.10 | Yöneticiyi kaldırmak: onay penceresi (geri alınamaz) ad, e-posta, oturumların kapanacağı, kullanılmamış davet sayısı ve bildirim (K-662); hesap kalkar, oturumlar anında sonlanır, davetleri geçersizleşir; iz satırları okunur kalır; kaldırılan paneldeyse sonraki işleminde girişe iner | UC-177 · UC-84 · konvansiyon — API2-02 · konvansiyon — API2-03 | eşlendi |
| `04` 9.20.11 | Engeller: kendi satırında kaldırma yok; iki yönetici birbirini aynı anda kaldırırsa ikincisi uygulanmaz — son yönetici kaldırılamaz, engel yerinde kalıcı | UC-177 · konvansiyon — API2-07 | eşlendi |
| `04` 9.20.12 | Validasyon: davetin adresi e-posta biçimiyle ve yönetici hesapları içinde kullanılmamış; alanlar 7.2.23 | UC-175 · konvansiyon — API2-07 | eşlendi |
| `04` 9.20.13 | Tek rol: her yönetici kendisi dışındakileri kaldırır ve her daveti geri çeker; tek yönetici varken kaldırma görünmez | UC-103 · konvansiyon — API0-07 · konvansiyon — API8-01 | eşlendi |
| `04` 9.20.14 | İlk kurulumda kurulumda açılan ilk yönetici, davet yok; satış kapısı etkilemez | UC-103 | eşlendi |
| `04` 9.20.15 | Ele geçirilmiş hesabın kaldırılması olağan kaldırmadır, ardından IBAN denetimi (E-45) ve işlem izi (E-52); erişimini kaybeden yöneticinin yolu yeni davet ve eski hesabın kaldırılması | UC-177 · UC-100 · UC-106 · UC-175 | eşlendi |
| `04` 9.20.16 | Boş hâl yok — en az bir yönetici; kaldırmanın onayı sonuçsuz kalırsa liste yenilenir (2.7.3.3); davet gönderimi tamamlanamazsa form korunur; sayfa açılamazsa hata sayfası; e-posta kesintisinde yol yeni davet | UC-103 · UC-175 · UC-177 · UC-78 · konvansiyon — API2-07 | eşlendi |
| `04` 9.20.17 | Responsive: tablolar ve kart listesi, işlemler kartın menüsünde | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-50 | Yöneticinin kendi hesabı — adını, e-postasını ve şifresini değiştirtir (§4.2) | UC-104 · UC-178 · UC-179 · UC-180 · UC-181 | eşlendi |
| `04` 9.21.1 | Aktör Yönetici; giriş üst satırın "Hesabım"ı, yeniden doğrulamadan dönüş, yeni adresin doğrulamasından sonra; çıkış: şifre ya da e-posta değiştirme → E-54'ün yeniden doğrulaması → E-50 | UC-104 · UC-108 | eşlendi |
| `04` 9.21.2 | İlk görülen: ad ve e-posta; birincil düğme yok — her bölüm kendi düğmesi | UC-104 | eşlendi |
| `04 §9.21` m1 | Bilgi hiyerarşisi: üç bölüm — ad, e-posta, şifre (5.25'in kalıbı) | UC-104 | eşlendi |
| `04` 9.21.3 | Ad — düzenleme, yeniden doğrulama istemez; listede, izde ve bildirimlerde okunur, müşteriye görünmez | UC-104 · UC-178 | eşlendi |
| `04` 9.21.4 | E-posta — "E-postayı değiştir"; firma iletişim adresi değildir; bekleyen değişiklik yeni adres, son gün ve bağlantıyı yeniden isteyen düğmeyle; geri alma bağlantısı açıkken (Z-45) değiştirme kapalı, sebebi ve bitiş tarihi | UC-104 · UC-179 · UC-180 · konvansiyon — API8-01 | eşlendi |
| `04` 9.21.5 | Şifre — "Şifreyi değiştir" | UC-104 · UC-181 | eşlendi |
| `04` 9.21.6 | Yoktur: kendi hesabını silme, Google girişi ve bağlama, iki adımlı doğrulama, sepet, sipariş ve adres defteri, bildirim tercihi | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 9.21.7 | Adı değiştirmek: onaysız, kısa bildirim; yeni ad üst satırda, listede, izin sonraki satırlarında | UC-178 | eşlendi |
| `04` 9.21.8 | E-postayı değiştirmek: E-54'ün yeniden doğrulamasından sonra yeni adres alanı; yeni adrese doğrulama bağlantısı, bekleyen değişiklik; hesap doğrulanana kadar eski adresle; başka yönetici hesabının adresiyse başlamaz — müşteri hesabı engel değil; işlem izine | UC-179 · UC-185 · konvansiyon — API2-05 · konvansiyon — API2-07 | eşlendi |
| `04` 9.21.9 | Bağlantıyı yeniden istemek — L-9; ilk bağlantının ömrünü uzatmaz; mesajı K-779'un ikinci metni | UC-180 · konvansiyon — API2-08 | eşlendi |
| `04` 9.21.10 | Şifreyi değiştirmek: yeniden doğrulamadan sonra yeni şifre alanı; öteki oturumlar kapanır, bu oturum açık kalır — yerinde kalıcı mesaj | UC-181 · UC-185 · konvansiyon — API2-05 | eşlendi |
| `04` 9.21.11 | Validasyon: ad boş olamaz, yeni e-posta biçimi ve yönetici hesapları içinde tekil, şifre politikası; L-9'da nötr limit mesajı; alanlar 7.2.24 | UC-178 · UC-179 · UC-181 · konvansiyon — API2-07 · konvansiyon — API2-08 | eşlendi |
| `04` 9.21.12 | Bekleyen değişiklik ile geri alma engeli birlikte durmaz; satış kapısı etkilemez | UC-104 · konvansiyon — API8-01 | eşlendi |
| `04` 9.21.13 | Boş hâl yok; her bölüm kendi yerinde bekler; beklenmeyen hatada mesaj bölümün içinde; oturum ekrandayken kapanırsa §3.6.5 | UC-104 · konvansiyon — API2-03 · konvansiyon — API2-07 | eşlendi |
| `04` 9.21.14 | Responsive: bölümler alt alta | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-51 | Satış özeti — seçilen dönemin satış ölçülerini gösterir (§4.2) | UC-105 | eşlendi |
| `04` 9.22.1 | Aktör Yönetici; giriş menü; özetin satırları başka ekrana götürmez | UC-105 | eşlendi |
| `04` 9.22.2 | İlk görülen: seçili dönem, sipariş sayısı ve ciro; birincil düğme yok — dönem seçimi özeti hemen yeniler | UC-105 · konvansiyon — API2-06 | eşlendi |
| `04 §9.22` m1 | Bilgi hiyerarşisi: dönem seçimi ve üç bölge | UC-105 | eşlendi |
| `04` 9.22.3 | Dönem seçimi — hazır dönemler, ölçüm penceresi ve özel aralık (OB-16); Son 30 gün seçili açılır; tek gün tepe gün sayısını verir; ölçüm penceresi satış hiç açılmadıysa yok — sorgu parametresi | UC-105 · konvansiyon — API2-06 · konvansiyon — API2-09 | eşlendi |
| `04` 9.22.4 | Sayılar: sipariş sayısı, ciro — ödemesi tamamlanmış siparişler, geri ödemeler düşülmez —, iptal ve iade sayısı | UC-105 | eşlendi |
| `04` 9.22.5 | Ölçüler: dört oran ya da sayı kartı; pay ve payda; sıfır paydada "Değerlendirilemez" | UC-105 | eşlendi |
| `04` 9.22.6 | Dağılımlar: en çok satan on ürün, ödeme yöntemi dağılımı | UC-105 | eşlendi |
| `04` 9.22.7 | Yoktur: ziyaretçi verisi, karşılaştırma ve grafik, dışa aktarma ve saklama, satırdan geçiş | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 9.22.8 | Dönem seçmek — özet hemen yeniden hesaplanır; özel aralığın bitişi başlangıçtan önce olamaz | UC-105 · konvansiyon — API2-06 | eşlendi |
| `04` 9.22.9 | Validasyon: özel aralıkta iki tarih zorunlu, ileri tarih ve ters aralık alan mesajı; alanlar 7.2.25 | UC-105 · konvansiyon — API2-06 · konvansiyon — API2-07 | eşlendi |
| `04` 9.22.10 | Özet her açılışta kayıttan hesaplanır, saklanmaz; ödeme süresi dolmamış deneme paydaya süre dolunca girer; satış kapalıyken de okunur | UC-105 | eşlendi |
| `04` 9.22.11 | İlk kurulumda ve siparişsiz dönemde sıfırlar ve "Değerlendirilemez"; hesap sürerken kartlar bekler; sayfa açılamazsa hata sayfası | UC-105 · UC-78 | eşlendi |
| `04` 9.22.12 | Responsive: kart sıraları, tablolar kart listesine, dönem seçimi açılır liste | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-52 | İşlem izi — yönetici işlemlerinin izini tarih ve yönetici süzgeciyle okutur (§4.2) | UC-106 | eşlendi |
| `04` 9.23.1 | Aktör Yönetici; giriş menü; satırlar başka ekrana götürmez | UC-106 | eşlendi |
| `04` 9.23.2 | İlk görülen: en yeni önce, tarih aralığı ve yönetici süzgeçleri; birincil düğme yok | UC-106 · konvansiyon — API2-06 | eşlendi |
| `04 §9.23` m1 | Bilgi hiyerarşisi: düz bir liste | UC-106 | eşlendi |
| `04` 9.23.3 | Süzgeçler — tarih aralığı (OB-16), yönetici kapalı listeden, kaldırılmışlar "kaldırıldı" ekiyle; süzgeçsiz açılır, ikisi birlikte | UC-106 · konvansiyon — API2-06 | eşlendi |
| `04` 9.23.4 | Liste — kim, ne zaman, ne değişti: tarih, yönetici, izin alanı, işlem ve kayıt, ayarda eski ve yeni değer; sipariş satırı müşterinin kişisel verisini değer olarak taşımaz | UC-106 | eşlendi |
| `04` 9.23.5 | Sayfa gezinmesi — sayfa başına 25 | UC-106 · konvansiyon — API2-06 | eşlendi |
| `04` 9.23.6 | Sıra: en yeni önce, sıralama seçeneği yok | UC-106 · konvansiyon — API2-06 | eşlendi |
| `04` 9.23.7 | Yoktur: arama, gruplama, dışa aktarma, satırın düzeltilmesi ve silinmesi, sistem olayları, ihlal formu, ize yazılmayan işlemler, siparişe süzülmüş iz, satırdan geçiş | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 9.23.8 | Süzmek — ilk sayfaya döner; süzgeçlerin adı listenin başında, tek dokunuşla kaldırılır | UC-106 · konvansiyon — API2-06 | eşlendi |
| `04` 9.23.9 | Validasyon: tarih aralığının bitişi başlangıçtan önce olamaz, ileri tarih reddedilir; alanlar 7.2.3 | UC-106 · konvansiyon — API2-06 · konvansiyon — API2-07 | eşlendi |
| `04` 9.23.10 | İz bütün yöneticilere aynı; satırlar saklama süresinin sonunda imha edilir (Z-30); ihlal şüphesinde süzgeçlerle okunur | UC-106 | eşlendi |
| `04` 9.23.11 | Süzgeç sonucu boşsa kaldırma yolu; kurulumdan sonra boş hâl; sayfa açılamazsa hata sayfası | UC-106 · UC-78 | eşlendi |
| `04` 9.23.12 | Responsive: satırın altına inen sütunlar, kart listesi | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-53 | Dışa aktarma — siparişleri, üye listesini ve iletişim taleplerini dosya olarak dışa aktartır (§4.2) | UC-107 · UC-182 · UC-183 · UC-184 | eşlendi |
| `04` 9.24.1 | Aktör Yönetici; giriş menü; dosya tarayıcının indirmesiyle iner, ekran değişmez (§4.4.8) | UC-107 · UC-182 · UC-183 · UC-184 · konvansiyon — API9-05 | eşlendi |
| `04` 9.24.2 | İlk görülen: üç bölüm ve dosyanın sorumluluğunu söyleyen satır; birincil siparişlerin "CSV olarak indir"i | UC-107 · UC-182 | eşlendi |
| `04 §9.24` m1 | Başta tek satır: her dışa aktarma işlem izine yazılır, dosyadaki kişisel verinin sorumluluğu firmaya geçer (K-823); altında üç bölüm | UC-107 | eşlendi |
| `04` 9.24.3 | Siparişler — tarih aralığı, siparişin oluştuğu güne göre; "CSV olarak indir"; dosyanın alanları `02 §10.7.1`'de | UC-182 · konvansiyon — API2-06 · konvansiyon — API9-05 | eşlendi |
| `04` 9.24.4 | Üye listesi — ad ve e-posta, aralık yok, bütün üyeler | UC-183 · konvansiyon — API9-05 | eşlendi |
| `04` 9.24.5 | İletişim talepleri — formdaki ad ve e-posta, aralık yok, saklanan bütün talepler | UC-184 · konvansiyon — API9-05 | eşlendi |
| `04` 9.24.6 | Yoktur: katalog, içerik, iz ve özet dışa aktarması, içe aktarma, zamanlanmış ve e-postalı dışa aktarma, fatura üretimi, toplu e-posta hattı | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 9.24.7 | Dışa aktarmak: onaysız; dosya hazırlanırken düğme ikinci kez basılamaz; işlem izine yazılır; aralıkta sipariş yoksa dosya üretilmez, bölümde kalıcı mesaj | UC-182 · UC-183 · UC-184 · konvansiyon — API9-05 | eşlendi |
| `04` 9.24.8 | Validasyon: sipariş aralığında iki tarih zorunlu, ileri tarih ve ters aralık alan mesajı; alanlar 7.2.25 | UC-182 · konvansiyon — API2-07 | eşlendi |
| `04` 9.24.9 | Her yönetici dışa aktarır; ihlalde etkilenenlerin listesi bu üç dosyadan; satış kapısı etkilemez | UC-182 · UC-183 · UC-184 | eşlendi |
| `04` 9.24.10 | İlk kurulumda üç bölüm görünür, boş listede dosya üretilmez; hazırlanamazsa mesaj bölümde, seçim korunur; sayfa açılamazsa hata sayfası | UC-107 · UC-182 · UC-183 · UC-184 · UC-78 · konvansiyon — API2-07 | eşlendi |
| `04` 9.24.11 | Responsive: bölümler alt alta | uç doğurmaz — görünüm | uç doğurmaz |
| `04` E-54 | Yönetici hesabının doğrulama ekranları — şifre ve e-posta değişikliğinden önce yeniden doğrulama, yeni adresin doğrulaması tek düğmeyle (K-904), geri alma tek düğmeyle (K-839) (§4.2) | UC-108 · UC-185 · UC-186 · UC-187 | eşlendi |
| `04` 9.25.1 | Aktör Yönetici ya da Kullanıcı; üç adım; giriş E-50'nin işlemleri, yeni adresin doğrulama bağlantısı (3.5.6), geri alma bağlantısı (3.5.7); çıkış E-50, oturum açıksa E-50 değilse E-29; geri almadan sonra şifre sıfırlama bağlantısıyla E-29'da | UC-108 · UC-104 · UC-84 · konvansiyon — API2-03 | eşlendi |
| `04` 9.25.2 | İlk görülen: yeniden doğrulamada işlem ve şifre alanı; bağlantıların geçerli hâlinde tek cümle ve işlemin adını taşıyan tek düğme; öteki hâllerde sonuç ve sonraki adımın bağlantısı | UC-108 · UC-185 · UC-186 · UC-187 | eşlendi |
| `04 §9.25` m1 | Bilgi hiyerarşisi: üç adım, E-24, E-21 ve E-28'in kalıbı; yalnız farklar yazılır | UC-108 | eşlendi |
| `04` 9.25.3 | Yeniden doğrulama — panel çerçevesinde; işlem iki tane — şifre ve e-posta değiştirme, hesap silme yok —, yalnız şifreyle, Google yok | UC-108 · UC-185 | eşlendi |
| `04` 9.25.4 | Yeni adresin doğrulama bağlantısının inişi — çerçevesiz; geçerli bağlantıda tek düğme, açılış doğrulamaz (K-904); yalnız e-posta değişikliğinin iki hâli — doğrulandı, bağlantı geçersiz; sonraki adım oturuma göre | UC-108 · UC-186 · UC-104 · UC-84 · konvansiyon — API2-02 | eşlendi |
| `04` 9.25.5 | Geri alma bağlantısının inişi — çerçevesiz; E-28'in geri alma adımı aynen; geri alındı — e-posta önceki adrese döner, oturumlar kapanır, tanınan tarayıcılar düşer, şifre geçersizleşir, sıfırlama bağlantısı gider; bağlantı geçersiz — yol başka yönetici ya da kurulum | UC-108 · UC-187 · konvansiyon — API2-02 | eşlendi |
| `04` 9.25.6 | Yoktur: iki adımlı doğrulama, iniş ekranlarında şifre alanı, geri almada ek soru ve onay penceresi | uç doğurmaz — yoktur | uç doğurmaz |
| `04` 9.25.7 | Doğrulamak: şifreyle; başarıda E-50'ye, başlatılan işlemin formuna; onaysız; yalnız başlatılan işlem için — tek kullanımlık değer | UC-185 · UC-104 · konvansiyon — API2-05 | eşlendi |
| `04` 9.25.8 | İniş ekranlarında yalnız sonraki adımın bağlantısı — iki geçerli hâlin tek düğmesi dışında; bağlantının hâlinin tespiti Teknik Mimari'nin | UC-108 · UC-186 · UC-187 | eşlendi |
| `04` 9.25.9 | Validasyon: şifre zorunlu; yanlış şifre alan mesajı ve L-1'e sayılır; eşik aşılınca nötr limit mesajı, işlem yapılmaz — muafiyet yok | UC-185 · konvansiyon — API2-07 · konvansiyon — API2-08 | eşlendi |
| `04` 9.25.10 | Yeniden doğrulama oturum ister; iniş ekranları oturumdan bağımsız, sonraki adımları oturuma göre; geri alma açık oturumu da kapatır | UC-108 · konvansiyon — API2-03 | eşlendi |
| `04` 9.25.11 | Boş hâl yok; tanınmayan bağlantı "bağlantı geçersiz"; doğrulama sürerken düğme ikinci kez basılamaz; oturum kapanırsa girişe; sonucu belirsiz kalırsa bağlantı yeniden açılınca söylenir (2.7.3.3); sayfa açılamazsa hata sayfası | UC-108 · UC-78 · konvansiyon — API2-07 | eşlendi |
| `04` 9.25.12 | Responsive: tek sütun | uç doğurmaz — görünüm | uç doğurmaz |

#### 1.1.7 Arayüz Tanımları — `04 §6` durum × rol (358)

Önce §6.1'in okunuş kuralları ve metni, sonra blok blok §6.2'nin ve §6.3'ün sırasıyla: bloğun başlığı (`04` 6.2.n, 6.3.n — ekranın durumdan bağımsız notu; ekranın GET adayıdır) ve satırları. Satırın "Ne gösterilir" hücresi ekranın GET adayının yanıtıdır; "Hangi aksiyonlar aktif" hücresinin her işlemi o ekranın POST adayına, her geçişi hedef ekranın GET adayına eşlenir (§1.1.2; K-1150). Hücre bir işlemi sunmadığını ya da açıklığını duruma, hâle ya da aralığa bağladığını yazdığında satır ayrıca `konvansiyon — API8-01` taşır — kural sunucuda kalır, sayfa düğmenin görünürlüğünü sunucunun kararından alır ve POST işleyicisi aynı kuralı yeniden denetler (6.1.4; K-912); değer her satıra mekanik eklenmez (K-1156). Bloğun girişindeki ortak kalıp — E-37'nin her durumda açık üç işlemi ve sipariş sürümü (API9-03) — satır başına yinelenmez. Hâl satırlarının hata ve limit yanıtı API2-07 ve API2-08'in, işlemin ekranındayken değişen kaydın yanıtı vitrinde API2-04'ün, panelin sipariş işlemlerinde API9-03'ün konusudur (K-1155).

| Kaynak ID | Kaynak özeti | Uç | Durum |
|---|---|---|---|
| `04` 6.1.1 | Birim ekrandır, her ekran bir blok: müşteri tarafı 6.2.n = E-n, panel 6.3.n = §9.n'nin ekranı; satır kimliği 6.2.n.k / 6.3.n.k; sütunlar Rol · Durum · Ne gösterilir · Hangi aksiyonlar aktif · Kaynak | uç doğurmaz — yazım konvansiyonu | uç doğurmaz |
| `04` 6.1.2 | "Durum" sütununun değerleri: varlık durumu adıyla (siparişin iki ekseni `Sevkiyat + Ödeme`, yayın, talep durumları — `02 §5`), kalemin kaydı ve panel işareti durum değildir, "Hâl:" önekli koşullar (satış kapısı, veri toplayan girişlerin kapısı, bağlantının geçerliliği, oturum, deneme limiti, dış servis kesintisi) | konvansiyon — API5-02 · konvansiyon — API8-01 | konvansiyon |
| `04` 6.1.3 | "Rol" sütununun değerleri: Ziyaretçi · Misafir alıcı · Üye · Müşteri · Yönetici · Kullanıcı — `02 §1.3`'ün dört aktörü; vitrinde Yönetici panel oturumu açık tarayıcıdaki yöneticidir, müşteri hesabı sayılmaz; panel tek rol, bağlantıyla açılan adımda Kullanıcı | konvansiyon — API0-07 · konvansiyon — API2-03 | konvansiyon |
| `04` 6.1.4 | Okunuş: "Hangi aksiyonlar aktif" işlemi `03 §1.11`'in adıyla yazar; aralığın dışında ekran işlemi sunmaz — düğme pasif hâlde de durmaz; "sunulmaz" bilinçli yokluktur | konvansiyon — API8-01 | konvansiyon |
| `04` 6.1.5 | Satırlarda yinelenmeyen ortak varyantlar — her ekranda aynı olanlar, bloklar yalnız farkı yazar | konvansiyon — API8-01 | konvansiyon |
| `04` 6.1.5.1 | Yönetici vitrinde: ziyaretçinin gördüğü ve yönetici şeridi ("Panele dön" → E-31); hesap alanı ve sepet müşteri oturumuna göre; Yönetici satırı yalnız taslak önizlemesi ya da "Taslak" etiketi olan ekranlarda; taslak duyuru yöneticiye etiketle | konvansiyon — API2-03 · konvansiyon — API5-01 · UC-86 | eşlendi |
| `04` 6.1.5.2 | Satış kapısı vitrinde yalnız E-04, E-12, E-13'ü, veri toplayan girişlerin kapısı E-10, E-20, E-22'yi değiştirir; açık siparişi etkilemez (E-16…E-19, E-37); panelde E-31, E-44…E-47 | konvansiyon — API8-01 · UC-05 · UC-22 · UC-27 · UC-15 · UC-48 · UC-51 | eşlendi |
| `04` 6.1.5.3 | Yükleniyor ve beklenmeyen hata her ekranda OB-07'nin kalıbı; eşzamanlı düzenleme kayıt formlarında OB-10'un kayıt varyantı; bloklar yalnız ekrana özgü boş hâli ve sonucu belirsiz işlemi yazar | konvansiyon — API2-07 · konvansiyon — API2-04 · UC-78 | eşlendi |
| `04` 6.1.5.4 | Genişlik sınıfı varyant değildir — hiçbir içerik ve işlev bir sınıfta gizlenmez | uç doğurmaz — görünüm | uç doğurmaz |
| `04` 6.1.5.5 | Misafir alıcı ve üye sipariş sayfasında ve işlemlerde aynı ekranı görür, fark girişin yoludur; fark yalnız E-04, E-10, E-12, E-13, E-14'te | konvansiyon — API2-03 · UC-33 · UC-34 | eşlendi |
| `04` 6.1.6 | Durum taşıyan ekranlar: ağırlık sipariş sayfasında (6.2.16) ve sipariş ayrıntısında (6.3.9); öteki durum taşıyan ekranların listesi | uç doğurmaz — yazım konvansiyonu | uç doğurmaz |
| `04` 6.1.7 | Matrisin yakaladığı dört eksik varyant — öneriyle karara bağlandı ve ekran tanımına yazıldı | uç doğurmaz — yazım konvansiyonu | uç doğurmaz |
| `04` 6.1.7.1 | Sipariş sayfası geri ödemede tutarı, tarihi ve yolu taşır (K-826; 5.16.7) | UC-33 · UC-34 | eşlendi |
| `04` 6.1.7.2 | Arşiv'deki ürünün formunda yan sütunun bağlantısı ve geçişleri (K-827; 9.5.10, 9.5.22) | UC-88 · UC-115 | eşlendi |
| `04` 6.1.7.3 | Hiçbir il seçilmeden kaydedilen teslimat illerinde vitrinin il kısıtı satırı (K-828) | UC-04 · UC-19 · UC-22 | eşlendi |
| `04` 6.1.7.4 | Taslak ürünün önizlemesinde yöneticiye sepete ekleme sunulmaz (K-829) | UC-04 · konvansiyon — API8-01 | eşlendi |
| `04 §6.1` metin | Matrisin iki hizalaması: taslaktaki ürünün arşive alınabilmesi (9.5.22) ve kendiliğinden iptal edilmiş siparişte misafir e-postasının düzeltmesi (9.9.38) | UC-115 · UC-151 | eşlendi |
| `04` 6.1.8 | Kapsama denetimi (betikle): `03 §1.11`'in kırk bir işlemi, 53 ekran bloğu, durum adları `02 §5`'le birebir | uç doğurmaz — yazım konvansiyonu | uç doğurmaz |
| `04` 6.2.1 | E-01 bloğu: satış kapısı ana sayfayı değiştirmez | UC-01 | eşlendi |
| `04` 6.2.1.1 | Tanıtım öncelikli düzen: kurumsal blok geniş, Hizmetlerimiz, Referanslarımız, ürün vitrini tek satır; blokların bağlantıları | UC-01 | eşlendi |
| `04` 6.2.1.2 | Mağaza öncelikli düzen: ürün vitrini iki satır, kurumsal blok dar; blokların bağlantıları | UC-01 | eşlendi |
| `04` 6.2.1.3 | İşaretli kayıt ya da yayında ürün yok — ilk kurulum dahil: boş blok görünmez, kurumsal bloğun geri düşüşü | UC-01 | eşlendi |
| `04` 6.2.1.4 | Satış kapalı: düzen aynı, ürün vitrini görünür kalır | UC-01 | eşlendi |
| `04` 6.2.2 | E-02 bloğu | UC-02 | eşlendi |
| `04` 6.2.2.1 | Yayında ürün var: kırıntı yolu, süzgeçler, ızgara, "Tükendi" ve indirimli kart; süzmek, ürün kartı → E-04, sayfa gezinmesi | UC-02 · UC-04 · konvansiyon — API2-06 | eşlendi |
| `04` 6.2.2.2 | Yayında ürün yok ya da süzgecin sonucu boş: boş hâl satırı — dönüş, süzgeci kaldırmak | UC-02 · konvansiyon — API2-06 | eşlendi |
| `04` 6.2.3 | E-03 bloğu | UC-03 | eşlendi |
| `04` 6.2.3.1 | Sonuç var: aranan sözcük, süzgeçler, ızgara; Arşiv'deki ürün sonuçta yok; aramayı yenilemek, süzmek, ürün kartı → E-04 | UC-03 · UC-04 · konvansiyon — API2-06 | eşlendi |
| `04` 6.2.3.2 | Sonuç yok: boş hâl satırı, kategori bağlantıları | UC-03 · UC-02 | eşlendi |
| `04` 6.2.4 | E-04 bloğu: ürün Yayında; Taslak ve Arşiv'de adres başka bir sayfa döner | UC-04 | eşlendi |
| `04` 6.2.4.1 | Yayında, varyant satın alınabilir, satış açık: galeri, fiyat, varyant seçimi, adet, teslimat bilgisi; varyantı seçmek, sepete eklemek, paylaşmak | UC-04 · UC-05 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.4.2 | Tükenmiş ya da açılmamış kombinasyon seçilemez; taslak ve arşivlenmiş varyant görünmez | UC-04 · UC-05 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.4.3 | Bütün varyantlar tükenmiş: düğmenin yerinde "Tükendi"; sepete ekleme sunulmaz | UC-04 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.4.4 | Satış kapalı: ürün görünür, düğmenin yerinde satışın kapalı olduğu; sepete ekleme sunulmaz | UC-04 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.4.5 | Adet stoğu ya da P-9'u aşıyor, dijital varyant zaten sepette: yerinde kalıcı mesaj; ekleme o adetle yapılmaz | UC-05 · konvansiyon — API2-07 | eşlendi |
| `04` 6.2.4.6 | Üye dijital varyantı daha önce almış: uyarı, sepete ekleme açık | UC-04 · UC-05 | eşlendi |
| `04` 6.2.4.7 | Firmanın teslimat il kısıtı — fiziksel üründe: kısıt satırı, boş seçimde K-828'in satırı | UC-04 | eşlendi |
| `04` 6.2.4.8 | Taslak ya da silinmiş ürün ziyaretçiye: E-06 | UC-04 · UC-06 | eşlendi |
| `04` 6.2.4.9 | Yönetici — Taslak ya da taslak varyant: "Taslak" bandıyla, taslak varyantlar dahil; "Düzenlemeye dön" → E-33; sepete ekleme sunulmaz | UC-04 · UC-88 · konvansiyon — API2-03 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.4.10 | Arşiv: E-05 — her rol | UC-04 | eşlendi |
| `04` 6.2.5 | E-05 bloğu | UC-04 | eşlendi |
| `04` 6.2.5.1 | Arşiv: ad, ana görsel, "Bu ürün artık satılmıyor"; fiyat ve sepete ekleme yok; yöneticiye aynı; dönüş bağlantıları | UC-04 | eşlendi |
| `04` 6.2.5.2 | Yayında'ya alındı ya da kalıcı silindi: adres E-04'ü ya da E-06'yı döner | UC-04 · UC-06 | eşlendi |
| `04` 6.2.6 | E-06 bloğu | UC-06 | eşlendi |
| `04` 6.2.6.1 | Var olmayan adres, taslak ya da silinmiş kayıt, geçersiz erişim anahtarı, imha edilmiş siparişin bağlantısı, hiç yayına alınmamış yasal metin: sebep ayırt edilmez; iki dönüş yolu | UC-06 · konvansiyon — API2-07 | eşlendi |
| `04` 6.2.6.2 | Yönetici, panel oturumu açık, taslak kaydın adresi: bu sayfa değil — kayıt "Taslak" bandıyla açılır | UC-04 · UC-10 · UC-11 · UC-12 · konvansiyon — API2-03 | eşlendi |
| `04` 6.2.6.3 | Yönetici, panel oturumu kapanmış, taslak kaydın adresi: bu sayfa | UC-06 · konvansiyon — API2-03 | eşlendi |
| `04` 6.2.7 | E-07 bloğu | UC-07 · UC-08 | eşlendi |
| `04` 6.2.7.1 | Tipin Yayında kaydı var: kartlar firmanın sırasıyla; Taslak kayıt yöneticiye de listede görünmez; içerik kartı → E-08 | UC-07 · UC-08 · UC-10 · UC-11 | eşlendi |
| `04` 6.2.7.2 | Tipin Yayında kaydı yok, adres doğrudan açıldı: boş hâl satırı, menü öğesi görünmez | UC-07 · UC-08 | eşlendi |
| `04` 6.2.8 | E-08 bloğu | UC-09 · UC-10 · UC-11 · UC-12 | eşlendi |
| `04` 6.2.8.1 | Yayında — Hakkımızda, genel sayfa: ad, görseller, metin; paylaşmak, video bağlantısı | UC-09 · UC-12 | eşlendi |
| `04` 6.2.8.2 | Yayında — hizmet tanıtımı, referans iş: "İlgili ürünler" yalnız Yayında'kiler; "Bize ulaşın" → E-10, ilgili ürün → E-04 | UC-10 · UC-11 · UC-14 · UC-04 | eşlendi |
| `04` 6.2.8.3 | Taslak ya da silinmiş kayıt ziyaretçiye: E-06 | UC-10 · UC-11 · UC-12 · UC-06 | eşlendi |
| `04` 6.2.8.4 | Yönetici — Taslak: "Taslak" bandıyla; "Düzenlemeye dön" → E-41 | UC-09 · UC-10 · UC-11 · UC-12 · UC-96 · konvansiyon — API2-03 | eşlendi |
| `04` 6.2.9 | E-09 bloğu | UC-13 | eşlendi |
| `04` 6.2.9.1 | Yayında soru var: firmanın sırasıyla, açık hâlde; cevaptaki bağlantılar | UC-13 | eşlendi |
| `04` 6.2.9.2 | Yönetici — taslak soru yerinde "Taslak" etiketiyle | UC-13 · konvansiyon — API2-03 | eşlendi |
| `04` 6.2.9.3 | Yayında soru yok, adres doğrudan açıldı: boş hâl satırı | UC-13 | eşlendi |
| `04` 6.2.10 | E-10 bloğu | UC-14 · UC-15 | eşlendi |
| `04` 6.2.10.1 | Veri toplayan girişlerin kapısı açık: kimlik bloğu, boş form, şubeler; formu göndermek, "Haritada aç" | UC-14 · UC-15 | eşlendi |
| `04` 6.2.10.2 | Üye, kapı açık: ad ve e-posta ön dolu ve düzenlenebilir | UC-14 · UC-15 | eşlendi |
| `04` 6.2.10.3 | Gönderim geçti — tuzak alanı dolu gönderim dahil: formun yerinde gönderim sonrası hâli, yeni form açan bağlantı | UC-15 · UC-14 · konvansiyon — API2-02 | eşlendi |
| `04` 6.2.10.4 | L-4 aşıldı: nötr limit mesajı, yazılanlar durur | UC-15 · konvansiyon — API2-08 · konvansiyon — API2-07 | eşlendi |
| `04` 6.2.10.5 | Veri toplayan girişlerin kapısı kapalı: form yerinde kapalı olduğu; gönderim sunulmaz | UC-14 · UC-15 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.10.6 | Yönetici — taslak şube "Taslak" etiketiyle | UC-14 · konvansiyon — API2-03 | eşlendi |
| `04` 6.2.10.7 | Şube yok: şubeler bölgesi görünmez | UC-14 | eşlendi |
| `04` 6.2.11 | E-11 bloğu | UC-16 · UC-17 · UC-18 | eşlendi |
| `04` 6.2.11.1 | Aydınlatma metni ya da çerez politikası yayında: güncel sürümün tam metni | UC-16 · UC-17 | eşlendi |
| `04` 6.2.11.2 | Metin hiç yayına alınmamış — ilk kurulum: altbilgide bağlantı yok, adres E-06 | UC-16 · UC-17 · UC-06 | eşlendi |
| `04` 6.2.11.3 | İşlem rehberi — her hâl: ürünün sabit metni, satış kapalıyken de | UC-18 | eşlendi |
| `04` 6.2.12 | E-12 bloğu: rol Ziyaretçi · Müşteri; üyenin sepeti hesabında, misafirinki tarayıcıya bağlı | UC-19 · konvansiyon — API2-03 | eşlendi |
| `04` 6.2.12.1 | Satış açık, siparişe girebilecek kalem var, asgari tutar karşılanıyor: kalemler, il kısıtı satırı, döküm; adedi değiştirmek, çıkarmak, kalem → E-04, ödeme adımına geçmek | UC-19 · UC-20 · UC-21 · UC-04 · UC-22 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.12.2 | Kalemin hâli — tükendi, kontenjan doldu, fiyat değişti, stok adedin altında, sınır aşıldı: satırda yerinde kalıcı mesaj, kalem siparişe ve tutara girmez | UC-19 · UC-20 · UC-21 | eşlendi |
| `04` 6.2.12.3 | Fiziksel kalemli sepet asgari tutarın altında: eksik tutar; ödeme adımına geçiş sunulmaz | UC-19 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.12.4 | Siparişe girebilecek kalem yok: geçişin yerinde mesaj; geçiş sunulmaz | UC-19 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.12.5 | Satış kapalı: sepet görünür, geçişin yerinde satışın kapalı olduğu; geçiş sunulmaz | UC-19 · UC-20 · UC-21 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.12.6 | Girişte sepet birleşti, kalem vitrinden kalktı, ödeme yöntemi kalmadığı için ödeme adımından dönüldü: bir kez söylenen mesaj | UC-19 · UC-55 · konvansiyon — API2-02 | eşlendi |
| `04` 6.2.12.7 | Ödeme yarıda kaldı, sağlayıcıya erişilemedi: sepet olduğu gibi durur; aynı sepetten yeniden denemek | UC-19 · UC-22 | eşlendi |
| `04` 6.2.12.8 | Teslimat il kısıtı, hiçbir il seçili değil: kısıt satırı, K-828'in satırı | UC-19 | eşlendi |
| `04` 6.2.12.9 | Sepet boş — çıkış yapan üyenin tarayıcısı dahil: boş hâl satırı, ürünlere dönüş | UC-19 | eşlendi |
| `04` 6.2.13 | E-13 bloğu: ekran satış açıkken ve siparişe girebilecek kalem varken açılır | UC-22 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.13.1 | Misafir alıcı, satış açık: giriş bağlantısı, e-posta alanı, "Siparişiniz şu adrese gönderilecek" ve "Düzelt"; e-posta, giriş → E-22 → E-13, adresler, kupon, yöntem, kutular, onaylamak → E-14 | UC-22 · UC-25 · UC-26 · UC-23 · UC-24 · UC-27 · UC-28 | eşlendi |
| `04` 6.2.13.2 | Yazılan e-posta kayıtlı bir müşteri hesabının: hatırlatma ve "Giriş yap" — siparişi engellemez | UC-25 · UC-26 | eşlendi |
| `04` 6.2.13.3 | Üye, satış açık: hesabın e-postası, adres defteri ve deftere kaydetme seçimi; adres seçmek, yazmak, kaydetmek, kupon, yöntem, kutular, onaylamak | UC-22 · UC-23 · UC-24 · UC-27 | eşlendi |
| `04` 6.2.13.4 | Sepette fiziksel kalem yok: teslimat bölümü, il ve kargo satırı yok, fatura formu açık | UC-22 · UC-27 | eşlendi |
| `04` 6.2.13.5 | Dijital ya da hizmet kalemi var: üçüncü ya da dördüncü kutu, kalemleri adıyla | UC-22 · UC-27 | eşlendi |
| `04` 6.2.13.6 | Teslimat il kısıtı, teslimat yapılmayan ildeki defter kaydı, hiçbir il seçili değil: kısıt satırı, kayıt seçilemez ve sebebini yazar | UC-22 · UC-27 · UC-25 | eşlendi |
| `04` 6.2.13.7 | Ödeme yöntemlerinin hâli — tek yöntem, kart sağlayıcısına erişilemiyor, L-8 dolu: tek yöntem seçili gelir, kullanılamayan yöntem listede ve sebebiyle | UC-22 · UC-27 · konvansiyon — API2-08 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.13.8 | Hiçbir ödeme yöntemi kullanılamıyor: sipariş verilemez, sepete dönüş | UC-22 · UC-19 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.13.9 | Kupon alanı — kapalı, açık, uygulanmış, L-6 aşıldı: OB-17'nin varyantı; uygulamak, değiştirmek, kaldırmak — L-6'da alan kapalı | UC-23 · UC-24 · konvansiyon — API2-08 | eşlendi |
| `04` 6.2.13.10 | Onay anında özet farkı, metin sürümü değişti, siparişe girebilecek kalem kalmadı: farkı söyleyen mesaj, kutuların işareti kalkar; yeniden onaylamak ya da → E-12 | UC-27 · UC-22 · UC-19 · konvansiyon — API2-07 | eşlendi |
| `04` 6.2.13.11 | L-7 eşiği aşıldı: nötr mesaj, sipariş oluşmaz; onay sunulmaz | UC-27 · konvansiyon — API2-08 | eşlendi |
| `04` 6.2.13.12 | Müşteri ekrandayken satış kapandı: onayda satışın kapalı olduğu, sepet korunur; "Sepete dön" | UC-27 · UC-26 · UC-19 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.13.13 | Üyenin oturumu ödeme adımında kapandı: girişe götürür, dönüşte yazılanlar durur | UC-26 · UC-54 · UC-55 · UC-22 · konvansiyon — API2-03 | eşlendi |
| `04` 6.2.13.14 | Sepete bağlı ödenmemiş önceki sipariş var: önceki siparişin numarası ve bu onayla iptal edileceği; onaylamak | UC-22 · UC-27 | eşlendi |
| `04` 6.2.14 | E-14 bloğu: sipariş Alındı + Bekliyor'da; ekran rozet göstermez | UC-28 | eşlendi |
| `04` 6.2.14.1 | Kart: siparişin alındığı, numara, ödemenin sağlayıcının sayfasında yapılacağı; sağlayıcının sayfasına geçmek | UC-28 · UC-29 | eşlendi |
| `04` 6.2.14.2 | Üye, havale: donmuş IBAN, toplam, son ödeme günü; IBAN'ı kopyalamak, → E-16 | UC-28 · UC-33 · uç doğurmaz — istemci | eşlendi |
| `04` 6.2.14.3 | Misafir alıcı, havale: siparişe ulaşmanın iki yolu; IBAN'ı kopyalamak, → E-15 | UC-28 · UC-31 · uç doğurmaz — istemci | eşlendi |
| `04` 6.2.14.4 | Sağlayıcıya onaydan sonra bağlanılamadı: sipariş kendiliğinden iptal, sepet korunur; → E-12 | UC-29 · UC-19 | eşlendi |
| `04` 6.2.14.5 | Ekran yeniden açıldı, kart siparişi artık Alındı + Bekliyor değil: geçiş sunulmaz; üye → E-16, misafir → E-15 | UC-28 · UC-33 · UC-31 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.15 | E-15 bloğu | UC-31 · UC-32 | eşlendi |
| `04` 6.2.15.1 | Sorgu yok: iki alan, sorgu düğmesi, iki yolun hatırlatması; sorgulamak → E-16 | UC-31 · UC-32 · UC-33 | eşlendi |
| `04` 6.2.15.2 | Numara ve e-posta eşleşmedi: formun başında nötr mesaj | UC-32 · konvansiyon — API2-07 | eşlendi |
| `04` 6.2.15.3 | L-5 aşıldı: nötr limit mesajı; yol e-postadaki bağlantı | UC-32 · konvansiyon — API2-08 · UC-34 | eşlendi |
| `04` 6.2.15.4 | Hesap silinmiş, e-posta değişmiş, misafir e-postası yanlış yazılmış: donmuş e-postayla açılır; yanlış yazılmışta açılmaz, yol iletişim formu | UC-32 · UC-33 · UC-14 | eşlendi |
| `04` 6.2.16 | E-16 bloğu: dört eksen — iki eksen, kalemin tipi ve kaydı, aktör, sayfayı değiştirmeyen hâller; kalem düzeyindeki işlemler her durumda | UC-33 · UC-34 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.16.1 | Alındı + Bekliyor — havale: donmuş IBAN, toplam, son gün; siparişi bütünüyle iptal etmek → E-17, IBAN'ı kopyalamak; kalem işlemleri ve indirme sunulmaz | UC-33 · UC-38 · uç doğurmaz — istemci · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.16.2 | Alındı + Bekliyor — kart, sonuç gelmemiş: kart ödemesinin beklendiği; siparişi bütünüyle iptal etmek → E-17 | UC-33 · UC-38 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.16.3 | İptal edildi + Başarısız: iptal kaydı, ödemenin gerçekleşmediği; geç gelen ödeme ve geri ödemesi; işlem sunulmaz, kendiliğinden iptal edilmiş sipariş e-postadaki bağlantıyla açılır | UC-33 · UC-34 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.16.4 | Ödenmiş, fiziksel açık kalemi olmayan sipariş: dijital kalemde indirme, hizmet kalemi; hizmet iptali → E-17, hizmetten cayma → E-18, ayıp talebi → E-19, indirmek | UC-33 · UC-38 · UC-42 · UC-44 · UC-36 · UC-37 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.16.5 | Hazırlanıyor: fiziksel ve hizmet kalemini iptal, hizmetten cayma, gecikme feshi — Z-11 geçmiş ve teslim tarihi yok, ayıp talebi, indirme | UC-33 · UC-38 · UC-42 · UC-44 · UC-36 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.16.6 | Kargoya verildi: kargo şirketi ve takip ya da araç beyanı; fiziksel kalemden caymak, gecikme feshi, ayıp talebi, hizmet ve dijital işlemler, "Takip et"; fiziksel kalem iptali sunulmaz | UC-33 · UC-42 · UC-38 · UC-44 · UC-36 · uç doğurmaz — dış sayfa · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.16.7 | Teslim edilemedi: gönderinin firmaya döndüğü; 6.2.16.6'nın işlemleri, takip bağlantısı | UC-33 · UC-42 · UC-38 · UC-44 · uç doğurmaz — dış sayfa · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.16.8 | Teslim edildi: teslim tarihi; Z-13 içinde caymak, tamamlanmamış hizmette iptal ve cayma, Z-18 içinde ayıp talebi, indirmek; fiziksel iptal ve fesih sunulmaz | UC-33 · UC-42 · UC-38 · UC-44 · UC-36 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.16.9 | İptal edildi + Ödendi ya da Kısmen geri ödendi, geri ödeme bekleniyor: kayıtlar, gerçekleşmiş geri ödemeler, havalede IBAN alanı; IBAN girmek ve düzeltmek; iptal, cayma ve "sorun bildir" sunulmaz | UC-33 · UC-35 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.16.10 | İptal edildi — kalıcı son hâl, bekleyen geri ödeme yok: belgeleri okumak, işlem sunulmaz | UC-33 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.16.11 | Adedi birden büyük kalemin bir kısmı kapanmış: adet ve açık adet, kayıtlar kapsadıkları adetle; seçilebilecek adet işleme açık adettir | UC-33 · UC-38 · UC-42 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.16.12 | Kalem kapanmış — iptal, çıkarma, gecikme feshi: kaydın adı, tarihi, adedi; işlem sunulmaz — IBAN alanı ve bekleyen geri ödeme hariç | UC-33 · UC-35 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.16.13 | Fiziksel kalem — cayma beyanı: tarih, adet, beyana yazılan iade adresi; kalan açık adette caymak, beyan taşımayan adette ayıp talebi | UC-33 · UC-42 · UC-44 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.16.14 | İade teslim alma, iade reddi, "mal dönmedi": kaydın adı ve tarihi, retta sebep; işlem sunulmaz — IBAN ve geri ödeme hariç; itiraz iletişim formundan | UC-33 · UC-35 · UC-14 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.16.15 | Kalemin cayma istisnası — mutlak, koşullu: mutlakta caymak sunulmaz, koşulluda açık ve E-18 koşulu yazar | UC-33 · UC-42 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.16.16 | Dijital kalem — hak var, hak dolmuş, havalede ödeme bekleniyor: indirme düğmesi ya da mesaj; iptal ve cayma yok | UC-33 · UC-36 · UC-37 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.16.17 | Hizmet kalemi — tamamlanmadan önce: iptal ve Z-14 içinde cayma; sonra: ayıp talebi | UC-33 · UC-38 · UC-42 · UC-44 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.16.18 | Havale hattı — IBAN isteği açık: kalemin satırında alan ve sebep; geç gelen ödemede alan müşteriden beklenen bölgesinde; IBAN girmek | UC-33 · UC-35 | eşlendi |
| `04` 6.2.16.19 | IBAN girildi, geri ödeme işlenmedi: girilmiş IBAN'ı düzeltmek; işlenince alan kalkar | UC-33 · UC-35 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.16.20 | Kart iadesi gerçekleşmedi ve firma havale yolunu açtı: IBAN girmek isteğe bağlı; yeniden deneme IBAN'sız gerçekleşirse alan kapanır | UC-33 · UC-35 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.16.21 | Ayıp talebi Açık: rozet, açılış tarihi; talepte işlem sunulmaz — ek bilgi firmanın e-postasına | UC-33 · uç doğurmaz — ürünün dışında · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.16.22 | Ayıp talebi Çözüldü, Z-18 içinde: yeniden açmak → E-19 | UC-33 · UC-44 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.16.23 | Çözüldü ya da talep yok, Z-18 dolmuş: yeniden açma ve "sorun bildir" sunulmaz; yol iletişim formu | UC-33 · UC-14 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.16.24 | Fiziksel kalem, teslim tarihi girilmemiş: caymak açık, ayıp talebi Kargoya verildi'den açık — Z-18 işlemez | UC-33 · UC-42 · UC-44 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.16.25 | Üye ve misafir alıcı — her durum: aynı sayfa, fark girişin yolu | UC-33 · UC-34 · konvansiyon — API2-03 | eşlendi |
| `04` 6.2.16.26 | Hesabı silinmiş ya da e-postası değişmiş müşteri: aynı sayfa, misafir yolundan ya da bağlantıyla | UC-32 · UC-34 · konvansiyon — API2-03 | eşlendi |
| `04` 6.2.16.27 | Geçersiz erişim anahtarı, kişisel verileri imha edilmiş sipariş: E-06, varlık açığa çıkmaz | UC-34 · UC-06 · konvansiyon — API2-07 | eşlendi |
| `04` 6.2.16.28 | İşlemin ekranındayken sipariş değişti: onay uygulanmaz, sebep ve güncel hâl | UC-39 · UC-40 · UC-41 · UC-43 · UC-45 · UC-46 · UC-35 · konvansiyon — API2-04 | eşlendi |
| `04` 6.2.16.29 | Satış kapalı, ürün Arşiv'e alındı: sayfa etkilenmez | UC-33 | eşlendi |
| `04` 6.2.16.30 | Sipariş e-postası ulaşmadı, e-posta altyapısı kesik: sayfa etkilenmez, her bilgiyi taşır | UC-33 | eşlendi |
| `04` 6.2.17 | E-17 bloğu: üç işlem siparişin durumuna göre; kart ve havale hattı geri ödemede ve IBAN'da ayrılır; misafir ile üye aynı | UC-38 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.17.1 | Alındı + Bekliyor — siparişin bütünüyle iptali: kalemler salt okunur, geri ödeme satırı ve IBAN yok; "Siparişi iptal et", "Vazgeç" | UC-38 · UC-39 · UC-33 | eşlendi |
| `04` 6.2.17.2 | Ödenmiş, kargodan ya da tamamlanmadan önce — kart: kalem ve adet seçimi, geri ödeme karta, kargo ücreti koşulu; "Seçilen ürünleri iptal et" | UC-38 · UC-40 · UC-33 | eşlendi |
| `04` 6.2.17.3 | Aynı durumlar — havale: IBAN alanı zorunlu ve boş | UC-38 · UC-40 | eşlendi |
| `04` 6.2.17.4 | Gecikme feshi — kart: kapsam salt okunur (teslim edilmemiş fiziksel kalemlerin işleme açık adetleri), kargo ücreti, adet alanı yok; "Gecikme nedeniyle feshet" | UC-38 · UC-41 · UC-33 | eşlendi |
| `04` 6.2.17.5 | Gecikme feshi — havale: IBAN alanı zorunlu | UC-38 · UC-41 | eşlendi |
| `04` 6.2.17.6 | İşlem o arada kapandı: onay uygulanmaz, sebep ve E-16'nın güncel hâli | UC-39 · UC-40 · UC-41 · UC-33 · konvansiyon — API2-04 | eşlendi |
| `04` 6.2.17.7 | Onay sonuçsuz kaldı: "işlem yapılmadı" denmez, sonucu görmek için E-16'ya | UC-39 · UC-40 · UC-41 · UC-33 · konvansiyon — API2-07 | eşlendi |
| `04` 6.2.18 | E-18 bloğu | UC-42 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.18.1 | Kargoya verildi, Teslim edilemedi — teslim işaretsiz fiziksel kalem: kalem ve adet, iade bilgisi, geri ödemenin süresi; "Cayma beyanını gönder" | UC-42 · UC-43 · UC-33 | eşlendi |
| `04` 6.2.18.2 | Teslim edildi, Z-13 içinde: geri ödemenin süresi malın ulaşmasından | UC-42 · UC-43 | eşlendi |
| `04` 6.2.18.3 | Hizmet kalemi, tamamlanmamış ve Z-14 içinde: iade bilgisi yok, cayılan adetlerin bedeli | UC-42 · UC-43 | eşlendi |
| `04` 6.2.18.4 | Kalemin koşullu istisnası: satırda koşul; seçmek açık | UC-42 · UC-43 | eşlendi |
| `04` 6.2.18.5 | Havale hattı: IBAN alanı zorunlu ve boş | UC-42 · UC-43 | eşlendi |
| `04` 6.2.18.6 | Seçim kargoyla gönderilen fiziksel kalemlerin bütün adetlerini kapsıyor: kargo ücretinin de geri ödeneceği | UC-42 · UC-43 | eşlendi |
| `04` 6.2.18.7 | Mutlak istisna, dijital, tamamlanmış hizmet, penceresi geçmiş kalem: listede yok, sunulmaz; yol iletişim formu | UC-42 · UC-14 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.18.8 | Pencere ya da hak o arada kapandı: onay uygulanmaz, sebep | UC-43 · UC-33 · konvansiyon — API2-04 | eşlendi |
| `04` 6.2.19 | E-19 bloğu | UC-44 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.19.1 | Yeni talep — fiziksel kalem, Kargoya verildi'den: kalem, ayıplı adet (varsayılan bir), açıklama, iade adresi, aydınlatma bağlantısı; göndermek | UC-44 · UC-45 | eşlendi |
| `04` 6.2.19.2 | Yeni talep — dijital ödemeden, hizmet tamamlandıktan sonra: iade adresi satırı yok | UC-44 · UC-45 | eşlendi |
| `04` 6.2.19.3 | Çözüldü, Z-18 içinde: talebin kalemi, adedi, ilk açıklaması, tarihi, rozeti; açıklama alanı yok; yeniden açmak | UC-44 · UC-46 | eşlendi |
| `04` 6.2.19.4 | Açık ya da Z-18 dolmuş: ekrana gidilmez — sipariş sayfası düğmeyi sunmaz | UC-44 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.20 | E-20 bloğu | UC-47 · UC-48 | eşlendi |
| `04` 6.2.20.1 | Kapı açık, Google tanımlı: "Google ile giriş", form, aydınlatma bağlantısı; kaydolmak | UC-47 · UC-48 · UC-50 | eşlendi |
| `04` 6.2.20.2 | Kayıt gönderildi — doğrulanmamış kayıt: gönderim sonrası hâli, son gün; bağlantıyı yeniden istemek | UC-48 · UC-49 · konvansiyon — API2-02 | eşlendi |
| `04` 6.2.20.3 | E-posta bir müşteri hesabının ya da bekleyen kaydın: alan mesajı ve "Giriş yap" | UC-48 · UC-54 · konvansiyon — API2-07 | eşlendi |
| `04` 6.2.20.4 | L-3 ya da L-9 aşıldı: nötr limit mesajı | UC-48 · UC-49 · konvansiyon — API2-08 | eşlendi |
| `04` 6.2.20.5 | Veri toplayan girişlerin kapısı kapalı: form kapalı, Google ile yeni hesap açılmaz; giriş | UC-47 · UC-48 · UC-51 · UC-54 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.20.6 | Google tanımlı değil: "Google ile giriş" görünmez | UC-47 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.20.7 | Google e-postayı doğrulanmamış verdi: formun başında yönlendirme mesajı | UC-51 · UC-47 | eşlendi |
| `04` 6.2.21 | E-21 bloğu: oturumdan bağımsız; geçerli bağlantıda doğrulama tek düğmeyle (K-904) | UC-52 · UC-53 · konvansiyon — API2-05 | eşlendi |
| `04` 6.2.21.1 | Kayıt bağlantısı Z-1 içinde ve kullanılmamış: doğrulama adımı, düğmeden sonra doğrulandı; sonra giriş | UC-52 · UC-53 · UC-54 | eşlendi |
| `04` 6.2.21.2 | Kayıt bağlantısının süresi dolmuş, tanınmayan bağlantı: kayda dönüş | UC-52 · UC-47 | eşlendi |
| `04` 6.2.21.3 | Bağlantının yerine yenisi gönderilmiş: girişe | UC-52 · UC-54 | eşlendi |
| `04` 6.2.21.4 | Kayıt bağlantısı kullanılmış: girişe | UC-52 · UC-54 | eşlendi |
| `04` 6.2.21.5 | Yeni e-posta bağlantısı geçerli: doğrulama adımı, düğmeden sonra doğrulandı; oturum açıksa E-25, değilse E-22 | UC-52 · UC-53 · UC-63 · UC-54 · konvansiyon — API2-03 | eşlendi |
| `04` 6.2.21.6 | Yeni e-posta bağlantısı geçersiz: sonraki adım 6.2.21.5'in bağlantısı | UC-52 · UC-63 · UC-54 | eşlendi |
| `04` 6.2.22 | E-22 bloğu | UC-54 · UC-55 | eşlendi |
| `04` 6.2.22.1 | Oturum yok: form, "Google ile giriş"; girmek, Google ile girmek, "şifremi unuttum", kayıt | UC-54 · UC-55 · UC-50 · UC-56 · UC-47 | eşlendi |
| `04` 6.2.22.2 | E-posta ya da şifre yanlış: formun başında tek mesaj | UC-55 · konvansiyon — API2-07 | eşlendi |
| `04` 6.2.22.3 | L-1 aşıldı: nötr limit mesajı, hesap kilitlenmez; tanınan tarayıcı engellenmez | UC-55 · konvansiyon — API2-08 | eşlendi |
| `04` 6.2.22.4 | Doğrulanmamış hesap, bilgi doğru: doğrulamanın beklendiği; bağlantıyı yeniden istemek — L-9 | UC-55 · UC-49 · konvansiyon — API2-08 | eşlendi |
| `04` 6.2.22.5 | Veri toplayan girişlerin kapısı kapalı: yeni hesap açacak girişte mesaj; mevcut hesapla girmek | UC-55 · UC-51 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.22.6 | Google tanımlı değil ya da erişilemiyor: düğme görünmez ya da giriş tamamlanmaz; şifreyle girmek | UC-54 · UC-50 · UC-51 · UC-55 | eşlendi |
| `04` 6.2.22.7 | Ödeme adımından gelindi, oturum kendiliğinden kapandı: dönüş ödeme adımına, yazılanlar durur; kapanan oturumda mesaj | UC-55 · UC-22 · konvansiyon — API2-03 | eşlendi |
| `04` 6.2.23 | E-23 bloğu | UC-56 · UC-58 | eşlendi |
| `04` 6.2.23.1 | İsteme adımı: e-posta alanı, hesap ekranından gelindiyse ön dolu; bağlantıyı istemek | UC-56 · UC-57 | eşlendi |
| `04` 6.2.23.2 | İstek gönderildi: nötr metin | UC-57 · konvansiyon — API2-02 | eşlendi |
| `04` 6.2.23.3 | L-2 aşıldı: nötr limit mesajı | UC-57 · konvansiyon — API2-08 | eşlendi |
| `04` 6.2.23.4 | Kurma adımı, geçerli bağlantı — hesapta şifre var ya da yok: yeni şifre alanı, şifresizde başlık belirleme | UC-58 · UC-59 | eşlendi |
| `04` 6.2.23.5 | Şifre kuruldu: oturumların kapandığı, giriş bağlantısı | UC-59 · UC-54 | eşlendi |
| `04` 6.2.23.6 | Bağlantı kullanılmış ya da Z-2 dolmuş: geçersizlik, isteme adımına | UC-58 · UC-56 | eşlendi |
| `04` 6.2.24 | E-24 bloğu | UC-60 | eşlendi |
| `04` 6.2.24.1 | Hesapta şifre var: işlemin adı, şifre alanı; doğrulamak, "Vazgeç" | UC-60 · UC-61 · UC-63 | eşlendi |
| `04` 6.2.24.2 | Şifresiz, Google girişli hesap: Google ile doğrulama düğmesi | UC-60 · UC-62 | eşlendi |
| `04` 6.2.24.3 | Şifreli ve Google girişli hesap: iki yol | UC-60 · UC-61 · UC-62 | eşlendi |
| `04` 6.2.24.4 | L-1 aşıldı: nötr limit mesajı, işlem yapılmaz; Google ile doğrulamak — varsa | UC-61 · UC-62 · konvansiyon — API2-08 | eşlendi |
| `04` 6.2.24.5 | Şifresiz hesap, Google'a ulaşılamıyor ya da doğrulama tutmadı: sebep ve şifre belirlemenin yolu; Google ile yeniden denemek, "Vazgeç" → E-25 → E-23 | UC-60 · UC-62 · UC-63 · UC-56 | eşlendi |
| `04` 6.2.25 | E-25 bloğu | UC-63 | eşlendi |
| `04` 6.2.25.1 | Şifreli hesap: beş bölüm, giriş yolları; adı değiştirmek, şifre ve e-posta değiştirmek → E-24, hesabı silmek → E-24 | UC-63 · UC-64 · UC-60 · UC-65 · UC-66 · UC-68 | eşlendi |
| `04` 6.2.25.2 | Şifresiz Google hesabı ya da ikisi birden: şifre belirlemenin yolu ya da değiştirme | UC-63 · UC-56 · UC-65 | eşlendi |
| `04` 6.2.25.3 | Bekleyen e-posta değişikliği: yeni adres, son gün; e-postayı değiştirmek — bekleyenin yerine geçer, bağlantıyı yeniden istemek — L-9 | UC-63 · UC-66 · UC-67 · konvansiyon — API2-08 | eşlendi |
| `04` 6.2.25.4 | Geri alma bağlantısı açık (Z-45): e-posta değiştirme ve hesap silme kapalı, sebep ve bitiş; ad ve şifre açık | UC-63 · UC-64 · UC-65 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.25.5 | Yeni adres başka bir müşteri hesabının: alan mesajı, değişiklik başlamaz | UC-66 · konvansiyon — API2-07 | eşlendi |
| `04` 6.2.25.6 | Şifresiz hesap, Google'a ulaşılamıyor: üç işlem yapılamaz; ad ve şifre belirleme | UC-63 · UC-64 · UC-56 · konvansiyon — API8-01 | eşlendi |
| `04` 6.2.25.7 | Hesap silindi: sonuç hâli bir kez, oturum kapalı | UC-69 · UC-68 | eşlendi |
| `04` 6.2.26 | E-26 bloğu | UC-70 | eşlendi |
| `04` 6.2.26.1 | Kayıtlı adres var: kayıtlar, telefonu olmayan ve teslimat dışı ildeki kayıt durur; eklemek, düzenlemek, silmek | UC-70 · UC-71 · UC-72 · UC-73 | eşlendi |
| `04` 6.2.26.2 | Kayıtlı adres yok: boş hâl satırı, adres eklemek | UC-70 · UC-71 | eşlendi |
| `04` 6.2.27 | E-27 bloğu | UC-74 | eşlendi |
| `04` 6.2.27.1 | İki eksenin bütün değerleri: satır ve iki rozet; satır → E-16 | UC-74 · UC-33 | eşlendi |
| `04` 6.2.27.2 | Kendiliğinden iptal edilmiş ödenmemiş sipariş, firmanın e-postasını düzelttiği misafir siparişi: listede görünmez | UC-74 | eşlendi |
| `04` 6.2.27.3 | Sipariş yok: boş hâl satırı, ürünlere dönüş | UC-74 | eşlendi |
| `04` 6.2.28 | E-28 bloğu: oturumdan bağımsız | UC-75 · UC-76 | eşlendi |
| `04` 6.2.28.1 | Bağlantı Z-45 içinde ve kullanılmamış: geri alma adımı, tek düğme; sonra yeni şifre bağlantıyla → E-23 | UC-75 · UC-76 · UC-58 | eşlendi |
| `04` 6.2.28.2 | Süresi dolmuş ya da kullanılmış: bağlantı geçersiz; çerçeve — İletişim | UC-75 · UC-14 | eşlendi |
| `04 §6.3` metin | Panel tek rol: rol sütunu Yönetici — panel girişi dahil —, davet, sıfırlama ve e-postadaki bağlantıyla açılan adımlarda Kullanıcı | konvansiyon — API0-07 · konvansiyon — API2-03 | konvansiyon |
| `04` 6.3.1 | E-29 bloğu: satış kapısı ve kurulum ekranı etkilemez | UC-84 | eşlendi |
| `04` 6.3.1.1 | Giriş adımı, oturum yok: form ve sıfırlama bağlantısı; girmek → E-31 ya da istenen ekran | UC-84 · UC-110 · UC-86 | eşlendi |
| `04` 6.3.1.2 | E-posta ya da şifre hatalı, L-1 aşıldı: K-779'un metni ya da nötr limit mesajı; tanınan tarayıcı, sıfırlama | UC-110 · konvansiyon — API2-07 · konvansiyon — API2-08 | eşlendi |
| `04` 6.3.1.3 | Oturum kendiliğinden kapandı: mesaj; girmek → istenen ekran | UC-84 · UC-110 · konvansiyon — API2-03 | eşlendi |
| `04` 6.3.1.4 | Sıfırlama isteği — gönderildi, L-2 aşıldı: nötr metin ya da nötr limit mesajı | UC-111 · konvansiyon — API2-08 | eşlendi |
| `04` 6.3.1.5 | Yeni şifre adımı — geçerli, kullanılmış ya da Z-2 dolmuş bağlantı: kurma düğmesi ya da sıfırlama isteğine | UC-84 · UC-112 · UC-111 | eşlendi |
| `04` 6.3.2 | E-30 bloğu: hâli davetin geçerliliği belirler | UC-85 | eşlendi |
| `04` 6.3.2.1 | Geçerli davet: adres salt okunur, ad ve şifre; hesabı açmak → E-29 | UC-85 · UC-113 · UC-84 | eşlendi |
| `04` 6.3.2.2 | Kullanılmış, Z-3 dolmuş, geri çekilmiş ya da geçersizleşmiş davet: süresi dolmuş davetin mesajı — sebep ayırt edilmez; panel girişi | UC-85 · UC-113 · UC-84 | eşlendi |
| `04` 6.3.3 | E-31 bloğu | UC-86 | eşlendi |
| `04` 6.3.3.1 | İlk kurulum — satış kapalı, kurulum eksik: uyarılar, kontrol listesi, sayaçlar sıfır; uyarı ve madde → ayar ekranı, sayaç → boş liste | UC-86 · UC-99 · UC-100 · UC-101 · UC-102 · UC-91 · UC-93 | eşlendi |
| `04` 6.3.3.2 | Satış açık, kurulum tamam: sayaçlar; sayaç → E-36 ya da E-38, "IBAN bekleniyor" satırı → E-37 | UC-86 · UC-91 · UC-93 · UC-92 · konvansiyon — API9-02 | eşlendi |
| `04` 6.3.3.3 | Satış kapalı — geçici kapatma: uyarı satırı; uyarı → E-44 | UC-86 · UC-99 | eşlendi |
| `04` 6.3.3.4 | Satış kapalı — koşul karşılanmıyor: uyarı koşulun adıyla, eksik madde; → koşulun ekranı | UC-86 · UC-99 · UC-100 · UC-101 · UC-102 | eşlendi |
| `04` 6.3.3.5 | Kanal uyarısı, "firma bildirimleri ulaşmıyor", "e-posta ulaşmadı", kart iadesi gerçekleşmedi, sonucu bekleniyor: uyarı satırları, koşul kalkınca kalkar; → E-44 ya da E-36'nın süzülmüş hâli | UC-86 · UC-99 · UC-91 · konvansiyon — API9-02 | eşlendi |
| `04` 6.3.3.6 | "IBAN bekleniyor" kalemi var ya da yok: ilk on satır kalan süreye göre; satır → E-37, "Tümünü gör" → E-36 | UC-86 · UC-92 · UC-91 · konvansiyon — API9-02 | eşlendi |
| `04` 6.3.3.7 | Sitenin kesintisinden dönüş: sayaçlar mevcut durumlardan hesaplanır | UC-86 | eşlendi |
| `04` 6.3.4 | E-32 bloğu | UC-87 | eşlendi |
| `04` 6.3.4.1 | Ürünlerin Taslak · Yayında · Arşiv'i: rozet, stok durumu, "Tükendi"; aramak, süzmek, satır ve "Yeni ürün" → E-33 | UC-87 · UC-88 · konvansiyon — API9-02 | eşlendi |
| `04` 6.3.4.2 | Ürün yok ya da sonuç boş: boş hâl satırı; "Yeni ürün", süzgeci kaldırmak | UC-87 · UC-88 · konvansiyon — API9-02 | eşlendi |
| `04` 6.3.5 | E-33 bloğu | UC-88 · konvansiyon — API8-01 | eşlendi |
| `04` 6.3.5.1 | Yeni ürün, kaydedilmemiş: boş form, KDV varsayılanı, stok kodu önerisi — ad boşken öneri yok; kaydetmek — Taslak doğar | UC-88 · UC-114 | eşlendi |
| `04` 6.3.5.2 | Taslak: yan sütunda rozet ve kapının koşulları; kaydetmek, "Önizle" → E-04 bandla, yayına almak — kapı, arşive almak, kalıcı silmek | UC-88 · UC-114 · UC-04 · UC-115 · UC-116 · konvansiyon — API8-01 | eşlendi |
| `04` 6.3.5.3 | Yayında: adres sabitlenmiş; kaydetmek — kapıyı bozan kayıt engellenir, "Sitede gör", taslağa ve arşive almak, kalıcı silmek — son Yayında varyant engeli | UC-88 · UC-114 · UC-04 · UC-115 · UC-116 · konvansiyon — API8-01 | eşlendi |
| `04` 6.3.5.4 | Arşiv: kaydetmek, "Sitede gör" → E-05, yayına almak — kapı, taslağa almak, kalıcı silmek | UC-88 · UC-114 · UC-04 · UC-115 · UC-116 · konvansiyon — API8-01 | eşlendi |
| `04` 6.3.5.5 | Varyantın Taslak · Yayında · Arşiv'i: rozet; varyantın satırından geçişler ve silme — son Yayında varyantta engel | UC-88 · UC-115 · UC-116 · konvansiyon — API8-01 | eşlendi |
| `04` 6.3.5.6 | Tip — fiziksel, dijital, hizmet: tipin alanları (stok, üretim yeri, süre; dosya; ifa süresi, kontenjan) | UC-88 · UC-114 · UC-83 | eşlendi |
| `04` 6.3.5.7 | Ödemesi beklenen sipariş adedi ayırmış: ayrılmış adet ve siparişler; sipariş → E-37, stok ayrılmış adedin altına inmez | UC-88 · UC-92 · UC-114 | eşlendi |
| `04` 6.3.5.8 | İndirim başlamadı, sürüyor, bitti: satır metni; indirimi düzenlemek | UC-88 · UC-114 | eşlendi |
| `04` 6.3.5.9 | Dijital üründe dosyasız yayındaki varyant: işaret; dosya yüklemek, dosyasız bırakan silme engellenir | UC-88 · UC-83 · UC-114 · UC-116 | eşlendi |
| `04` 6.3.6 | E-34 bloğu | UC-89 | eşlendi |
| `04` 6.3.6.1 | Kategori var: ağaç alfabetik, yayında ürünü yoksa satır; eklemek, yeniden adlandırmak, taşımak, silmek — üçüncü seviyede alt kategori yok | UC-89 · UC-118 · UC-119 · UC-120 · UC-121 · konvansiyon — API8-01 | eşlendi |
| `04` 6.3.6.2 | Kategoride ürün ya da alt kategori var: silme engeli sayılarla; taşımak | UC-89 · UC-121 · UC-120 · konvansiyon — API8-01 | eşlendi |
| `04` 6.3.6.3 | Kategori yok: boş hâl satırı, "Yeni kategori" | UC-89 · UC-118 | eşlendi |
| `04` 6.3.7 | E-35 bloğu: kuponun durum makinesi yok, geçerlilik hâli satır metni | UC-90 | eşlendi |
| `04` 6.3.7.1 | Başlamadı, geçerli, süresi doldu, hakkı doldu: satırda hâl ve kullanım; her hâlde düzenlemek | UC-90 · UC-122 | eşlendi |
| `04` 6.3.7.2 | Kullanılmış ya da ayrılmış hak var: salt okunur sayılar; adet bu sayının altına inmez | UC-90 · UC-122 | eşlendi |
| `04` 6.3.7.3 | Kupon yok: boş hâl satırı, "Yeni kupon" | UC-90 · UC-122 | eşlendi |
| `04` 6.3.8 | E-36 bloğu | UC-91 | eşlendi |
| `04` 6.3.8.1 | İki eksenin bütün değerleri, panel işaretleri: rozetler, işaretler, kalanı en az süre; aramak, süzmek, satır → E-37 | UC-91 · UC-92 · konvansiyon — API9-02 | eşlendi |
| `04` 6.3.8.2 | Sayaçtan ya da uyarıdan gelindi: süzgeç seçili ve adı başta; süzgeci kaldırmak | UC-91 · konvansiyon — API9-02 | eşlendi |
| `04` 6.3.8.3 | Kendiliğinden iptal edilmiş ve açık ödenmemiş sipariş: satırda görünür; satır → E-37 | UC-91 · UC-92 | eşlendi |
| `04` 6.3.8.4 | Sipariş yok, sıfır sayaçtan, sonuç boş: boş hâl satırı; süzgeci kaldırmak | UC-91 · konvansiyon — API9-02 | eşlendi |
| `04` 6.3.9 | E-37 bloğu: dört eksen; birincil düğme hattın sıradaki zorunlu adımı; her durumda açık üç işlem — iç not, e-postayı yeniden göndermek, misafir e-postasının düzeltmesi | UC-92 · UC-152 · UC-153 · UC-154 · UC-151 · konvansiyon — API8-01 · konvansiyon — API9-03 | eşlendi |
| `04` 6.3.9.1 | Alındı + Bekliyor — havale: birincil "ödendi" işareti — onay, siparişin tamamının iptali, adres düzeltmesi; kalem iptali ve çıkarma sunulmaz | UC-92 · UC-123 · UC-129 · UC-146 · konvansiyon — API8-01 | eşlendi |
| `04` 6.3.9.2 | Alındı + Bekliyor — kart, sonuç bekleniyor: işaret, birincil yok; siparişin tamamının iptali, adres düzeltmesi | UC-92 · UC-129 · UC-146 · konvansiyon — API8-01 | eşlendi |
| `04` 6.3.9.3 | Ödenmiş, fiziksel açık kalemi olmayan sipariş: hizmet tamamlama, firma iptali, çıkarma, cayma kaydı, geri ödeme — tutar bazlı dahil, IBAN isteme, fatura adresi, durum düzeltmesi | UC-92 · UC-139 · UC-129 · UC-138 · UC-130 · UC-131 · UC-133 · UC-134 · UC-146 · UC-150 · konvansiyon — API8-01 | eşlendi |
| `04` 6.3.9.4 | Hazırlanıyor: birincil "Kargoya ver" — onay; iptal, çıkarma, gecikme feshi ve cayma kaydı, tamamlama, geri ödeme, IBAN, adres, durum düzeltmesi | UC-92 · UC-124 · UC-129 · UC-138 · UC-130 · UC-139 · UC-131 · UC-134 · UC-146 · UC-150 · konvansiyon — API8-01 | eşlendi |
| `04` 6.3.9.5 | Kargoya verildi: birincil teslim işareti — sıra sorusuyla; "Teslim edilemedi", cayma ve fesih kaydı, hizmet işlemleri, geri ödeme, IBAN, takip düzeltmesi, durum düzeltmesi; adres düzeltmesi sunulmaz | UC-92 · UC-126 · UC-127 · UC-130 · UC-129 · UC-138 · UC-139 · UC-131 · UC-134 · UC-147 · UC-150 · konvansiyon — API8-01 | eşlendi |
| `04` 6.3.9.6 | Teslim edilemedi: yeniden göndermek — onay, geri dönen kalemin iptali, hizmet işlemleri, cayma ve fesih kaydı, iade malını teslim almak, geri ödeme, IBAN, takip ve durum düzeltmesi | UC-92 · UC-125 · UC-129 · UC-138 · UC-139 · UC-130 · UC-140 · UC-131 · UC-134 · UC-147 · UC-150 · konvansiyon — API8-01 | eşlendi |
| `04` 6.3.9.7 | Teslim edilemedi — açık kalem yok, hiçbiri teslim edilmemiş: "Siparişi kapat" — onaysız | UC-92 · UC-128 | eşlendi |
| `04` 6.3.9.8 | Teslim edildi: cayma kaydı — pencere dışında uyarı ve kutu, iade hattı, geri ödeme yolları, IBAN, ayıp talebi, indirme hakkı, hizmet işlemleri, teslim tarihi, takip ve durum düzeltmesi | UC-92 · UC-130 · UC-140 · UC-141 · UC-142 · UC-131 · UC-135 · UC-136 · UC-137 · UC-132 · UC-134 · UC-144 · UC-145 · UC-143 · UC-129 · UC-138 · UC-139 · UC-148 · UC-147 · UC-150 · konvansiyon — API8-01 | eşlendi |
| `04` 6.3.9.9 | İptal edildi + Başarısız: salt okunur, geç ödeme ve sistemin geri ödemesi; ortak üç işlem, durum düzeltmesi yok | UC-92 · UC-152 · UC-153 · UC-151 · konvansiyon — API8-01 | eşlendi |
| `04` 6.3.9.10 | İptal edildi + ödenmiş: kayıtlar, bekleyen geri ödeme; geri ödeme yolları, IBAN, iade malını teslim almak, durum düzeltmesi — yalnız Ödendi'de, kart iadesi beklenirken engel mesajı | UC-92 · UC-131 · UC-135 · UC-136 · UC-137 · UC-132 · UC-134 · UC-140 · UC-150 · konvansiyon — API8-01 · konvansiyon — API2-07 | eşlendi |
| `04` 6.3.9.11 | Fiziksel kalem — açık, Kargoya verildi'den önce, ödenmiş: adet ve açık adet; adetle iptal, çıkarma — kartta onay | UC-92 · UC-129 · UC-138 | eşlendi |
| `04` 6.3.9.12 | Kalem kapanmış — iptal, çıkarma: kayıt, firma iptalinde sebep; işlem yok — geri ödeme ve IBAN isteği hariç | UC-92 · UC-131 · UC-134 · konvansiyon — API8-01 | eşlendi |
| `04` 6.3.9.13 | Gecikme feshi — kargoya verilmemiş, kargodan dönen: kanuni faiz uyarısı, firma iptali uygulanmaz; dönen malda teslim almak ve "Stoğa ekle" | UC-92 · UC-140 · UC-141 · konvansiyon — API8-01 | eşlendi |
| `04` 6.3.9.14 | Cayma beyanı — mal bekleniyor: damga, iade adresi, süre, bekleyen adet; teslim almak — ulaşma tarihi ve adet, reddetmek — koşullu istisnada, onay | UC-92 · UC-140 | eşlendi |
| `04` 6.3.9.15 | Cayma beyanı — Z-42 geçmiş, ulaşmamış adet: "Mal dönmedi" kapanışı adetle | UC-92 · UC-142 | eşlendi |
| `04` 6.3.9.16 | İade teslim alma, ret, "mal dönmedi": kayıtlar; stoğa eklemek, ulaşma tarihini düzeltmek, kapanıştan sonra ulaşan malı teslim almak | UC-92 · UC-141 · UC-149 · UC-140 | eşlendi |
| `04` 6.3.9.17 | Bekleyen geri ödeme — havale, IBAN var: tutar, adet, IBAN salt okunur, kalan süre; işlemek — onay, "Havale gerçekleşmedi" | UC-92 · UC-131 · UC-132 | eşlendi |
| `04` 6.3.9.18 | Bekleyen geri ödeme — havale, IBAN yok: düğmenin yerinde IBAN isteğinin beklendiği | UC-92 · konvansiyon — API8-01 | eşlendi |
| `04` 6.3.9.19 | Kart iadesi — başlatıldı, sonucu bekleniyor, gerçekleşti, gerçekleşmedi: sonuç; bekleniyor'da "gerçekleşmedi" işareti, gerçekleşmedi'de yeniden deneme ya da havale yolu | UC-92 · UC-137 · UC-135 · UC-136 | eşlendi |
| `04` 6.3.9.20 | Dijital kalem: indirme sayacı ve hak; iptal ve çıkarma sunulmaz; indirme hakkını yenilemek | UC-92 · UC-143 · konvansiyon — API8-01 | eşlendi |
| `04` 6.3.9.21 | Hizmet kalemi — tamamlanmamış, tamamlanmış: ifa süresinin kalanı ya da teslim işareti; tamamlama, iptal, çıkarma, cayma kaydı; ödenmemişte tamamlama sunulmaz | UC-92 · UC-139 · UC-129 · UC-138 · UC-130 · konvansiyon — API8-01 | eşlendi |
| `04` 6.3.9.22 | Ayıp talebi Açık, Çözüldü: rozet ve tarihler; çözüldü işareti, Z-18 içinde yeniden açma, para gerektiren çözüm — teslim alma, geri ödeme, IBAN | UC-92 · UC-144 · UC-145 · UC-140 · UC-131 · UC-134 | eşlendi |
| `04` 6.3.9.23 | İşaret "e-posta ulaşmadı": ulaşmayan e-postalar ve adres hatırlatması; satırı ya da hepsini yeniden göndermek | UC-92 · UC-153 · UC-154 | eşlendi |
| `04` 6.3.9.24 | İşaret "ödeme sonucu alınamadı": sonucun beklendiği; işlem yok | UC-92 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `04` 6.3.9.25 | İşaret "geri ödeme gerçekleşmedi": K-798'in uyarısı; yeniden deneme ya da havale yolu | UC-92 · UC-135 · UC-136 | eşlendi |
| `04` 6.3.9.26 | İşaret "e-posta düzeltildi": e-posta geçmişine bağlantı, işaret kalkmaz | UC-92 | eşlendi |
| `04` 6.3.9.27 | Yöneticinin gördüğü hâlden sonra sipariş değişti: işlem uygulanmaz, K-799'un mesajı ve güncel hâl | UC-92 · konvansiyon — API9-03 | eşlendi |
| `04` 6.3.9.28 | Onay isteyen işlem sonuçsuz kaldı: güncel hâl yeniden yüklenir | UC-92 · konvansiyon — API2-07 | eşlendi |
| `04` 6.3.9.29 | Fiziksel kalemsiz sipariş, havale ve kart hattı: teslimat, kargo ve teslim işareti yok; hatta göre IBAN satırları ya da kart iadesinin sonucu | UC-92 · konvansiyon — API8-01 | eşlendi |
| `04` 6.3.9.30 | İşaret "geri ödeme sonucu bekleniyor": uyarı; "gerçekleşmedi" işareti — onaysız; yeniden deneme ve havale yolu yok | UC-92 · UC-137 · konvansiyon — API8-01 | eşlendi |
| `04` 6.3.10 | E-38 bloğu | UC-93 | eşlendi |
| `04` 6.3.10.1 | İletişim talebi Açık · Kapatıldı, ayıp talebi Açık · Çözüldü: tür, öz, tarih, rozet, B-9 işareti; süzmek, satır → E-39 ya da E-37 | UC-93 · UC-94 · UC-92 · konvansiyon — API9-02 | eşlendi |
| `04` 6.3.10.2 | "Açık talep" sayacından gelindi: "Açık" süzgeci seçili, satır sayısı sayaca eşit; süzgeci kaldırmak | UC-93 · konvansiyon — API9-02 | eşlendi |
| `04` 6.3.10.3 | Talep yok, sonuç boş, saklama süresi doldu: boş hâl satırı, süresi dolan talep listeden düşer | UC-93 · konvansiyon — API9-02 | eşlendi |
| `04` 6.3.11 | E-39 bloğu | UC-94 | eşlendi |
| `04` 6.3.11.1 | İletişim talebi Açık: konu, ulaşma anı, gönderen, mesaj, son kapatılış; birincil "Talebi kapat", "Siparişlerde ara", "Üye kaydında ara" | UC-94 · UC-155 · UC-91 · UC-95 | eşlendi |
| `04` 6.3.11.2 | Kapatıldı: son kapatılış; birincil "Yeniden aç", iki arama | UC-94 · UC-156 · UC-91 · UC-95 | eşlendi |
| `04` 6.3.11.3 | Başka bir yönetici durumu değiştirdi, talep imha edildi: güncel durum ya da talebin bulunmadığı; listeye dönüş | UC-94 · UC-155 · UC-156 · UC-93 · konvansiyon — API2-07 | eşlendi |
| `04` 6.3.12 | E-40 bloğu | UC-95 | eşlendi |
| `04` 6.3.12.1 | Arama yapılmadı, hesap bulunamadı: arama alanı, K-811'in satırı; aramak | UC-95 · konvansiyon — API9-02 | eşlendi |
| `04` 6.3.12.2 | Doğrulanmamış kayıt ya da doğrulanmış hesap: hesap, giriş yolları, adres defteri, bağlı siparişler; hesabı silmek — onay, sipariş → E-37 | UC-95 · UC-157 · UC-92 | eşlendi |
| `04` 6.3.12.3 | Geri alma bağlantısı açık (Z-45): silme düğmesi kapalı, sebep ve bitiş; silme sunulmaz | UC-95 · konvansiyon — API8-01 | eşlendi |
| `04` 6.3.12.4 | Hesap silindi: arama alanı ve silmenin yapıldığı mesaj | UC-157 · UC-95 · konvansiyon — API2-02 | eşlendi |
| `04` 6.3.13 | E-41 bloğu: kurumsal taraf satış kapısına bağlı değil | UC-96 | eşlendi |
| `04` 6.3.13.1 | Liste — Taslak · Yayında: yedi grup, rozetler, işaretler; kaydı açmak, "Yeni", yukarı ve aşağı | UC-96 · UC-161 | eşlendi |
| `04` 6.3.13.2 | Kayıt Taslak: form, kapının alanları; kaydetmek, "Önizle", yayına almak — kapı, kalıcı silmek — Hakkımızda'da yok | UC-96 · UC-158 · UC-159 · UC-160 · UC-09 · UC-10 · UC-11 · UC-12 · konvansiyon — API8-01 | eşlendi |
| `04` 6.3.13.3 | Kayıt Yayında: adres sabitlenmiş; kaydetmek — kapıyı bozan engellenir, "Sitede gör", taslağa almak, kalıcı silmek | UC-96 · UC-158 · UC-159 · UC-160 · UC-09 · UC-10 · UC-11 · UC-12 · konvansiyon — API8-01 | eşlendi |
| `04` 6.3.13.4 | Duyuru Yayında — aralığın içinde ya da dışında: satır görünürlüğü söyler | UC-96 | eşlendi |
| `04` 6.3.13.5 | İlk kurulum, grup boş: Hakkımızda ve duyuruda kayıt yok; boş formu açmak, "Yeni" | UC-96 · UC-158 | eşlendi |
| `04` 6.3.14 | E-42 bloğu | UC-97 | eşlendi |
| `04` 6.3.14.1 | Düzen seçili, menü öğesi görünür ya da görünmüyor: görünmeme sebebi; düzeni seçmek, adları ve bağlantıları düzenlemek, kaydetmek | UC-97 · UC-162 | eşlendi |
| `04` 6.3.14.2 | İlk kurulum: tanıtım öncelikli seçili, varsayılan adlar | UC-97 · UC-162 | eşlendi |
| `04` 6.3.15 | E-43 bloğu | UC-98 | eşlendi |
| `04` 6.3.15.1 | Alan dolu ya da boş — ilk kurulum dahil: geri düşüş, renk örneği; kaydetmek, görsel yüklemek ve kaldırmak | UC-98 · UC-163 · UC-117 | eşlendi |
| `04` 6.3.15.2 | Marka adı bir kez girilmiş: alan boşaltılamaz; değiştirmek | UC-98 · UC-163 | eşlendi |
| `04` 6.3.16 | E-44 bloğu | UC-99 | eşlendi |
| `04` 6.3.16.1 | İlk kurulum — tip seçilmemiş, satış kapalı: "Satış kapalı" ve koşullar, anahtar; tipi seçmek, kimliği kaydetmek, koşul bağlantıları, geçici kapatmak — onay | UC-99 · UC-164 · UC-165 · UC-100 · UC-101 · UC-102 | eşlendi |
| `04` 6.3.16.2 | Satış açık: "Satış açık", beş satır; kimliği kaydetmek, geçici kapatmak — onay | UC-99 · UC-164 · UC-165 | eşlendi |
| `04` 6.3.16.3 | Satış kapalı — geçici kapatma, koşul eksiği olsun ya da olmasın: sebepler, anahtar "Satışı yeniden aç"; yeniden açmak — onaysız, koşul eksikse kapalı kalır | UC-99 · UC-166 · UC-164 · UC-100 · UC-101 · UC-102 | eşlendi |
| `04` 6.3.16.4 | Firma tipi değişiyor: yeni tipin alanları ve satışın kapalı kalacağı notu; kaydetmek | UC-99 · UC-164 | eşlendi |
| `04` 6.3.16.5 | "Firma bildirimleri ulaşmıyor" sürüyor: e-posta alanının yanında K-799'un metni; adresi değiştirmek | UC-99 · UC-164 | eşlendi |
| `04` 6.3.16.6 | Satış kapalı — yalnız koşul karşılanmıyor: koşul adıyla, anahtar "Satışı geçici olarak kapat"; kaydetmek, koşul bağlantıları, geçici kapatmak | UC-99 · UC-164 · UC-165 · UC-100 · UC-101 · UC-102 | eşlendi |
| `04` 6.3.17 | E-45 bloğu | UC-100 | eşlendi |
| `04` 6.3.17.1 | Kart tanımlı ya da tanımsız, havale kapalı — ilk kurulum dahil: kartın satırı, IBAN boş; havaleyi açmak ve IBAN girmek | UC-100 · UC-167 | eşlendi |
| `04` 6.3.17.2 | Havale açık: IBAN ve hatırlatma; IBAN'ı değiştirmek ve havaleyi kapatmak — onay | UC-100 · UC-167 | eşlendi |
| `04` 6.3.17.3 | Kart tanımsız ve havale açık ya da ikisi kapalı: satışın havaleyle açık olduğu ya da kapının karşılanmadığı; kapatırken satışın kapanacağı onayda | UC-100 · UC-167 | eşlendi |
| `04` 6.3.18 | E-46 bloğu | UC-101 | eşlendi |
| `04` 6.3.18.1 | İlk kurulum — iade adresi boş: varsayılan değerler; bölümü kaydetmek, iade adresini ilk kez girmek — onaysız | UC-101 · UC-168 · UC-169 · UC-170 · UC-171 · UC-172 | eşlendi |
| `04` 6.3.18.2 | İade adresi girilmiş: boşaltılamaz; değiştirmek — onay | UC-101 · UC-172 | eşlendi |
| `04` 6.3.18.3 | Hiçbir teslimat ili seçili değil: bölümün satırı; kaydetmek engellenmez | UC-101 · UC-169 | eşlendi |
| `04` 6.3.19 | E-47 bloğu | UC-102 | eşlendi |
| `04` 6.3.19.1 | İlk kurulum — iki metin hiç yayına alınmamış, aydınlatma tamamlanmamış: tamamlanma satırı; taslağı kaydetmek, eksik alan → E-44, aydınlatmada yayına alma tamamlanana kadar yok, çerez politikasını yayına almak | UC-102 · UC-173 · UC-174 · UC-99 · konvansiyon — API8-01 | eşlendi |
| `04` 6.3.19.2 | Yayında, taslakta değişiklik var ya da yok: sürüm ve tarih, farkta K-818'in satırı; taslağı kaydetmek, yayına almak — yeni sürüm | UC-102 · UC-173 · UC-174 | eşlendi |
| `04` 6.3.19.3 | Üretilen iki metin — her hâl: güncel sürüm salt okunur; besleyen ayar ya da şablon değişince sürüm artar | UC-102 · UC-164 · UC-168 · UC-169 · UC-170 · UC-172 | eşlendi |
| `04` 6.3.20 | E-49 bloğu | UC-103 | eşlendi |
| `04` 6.3.20.1 | Tek yönetici — ilk kurulum dahil: tek satır, kaldırma yok; "Yönetici davet et" | UC-103 · UC-175 · konvansiyon — API8-01 | eşlendi |
| `04` 6.3.20.2 | Birden çok yönetici: kendi satırında kaldırma yok; yöneticiyi kaldırmak — onay | UC-103 · UC-177 · konvansiyon — API8-01 | eşlendi |
| `04` 6.3.20.3 | Geçerli davet — e-postası ulaştı ya da ulaşmadı: davet satırı, son gün, işaret; geri çekmek, yeni davet göndermek | UC-103 · UC-176 · UC-175 | eşlendi |
| `04` 6.3.20.4 | Aynı anda karşılıklı kaldırma — son yönetici: engel | UC-177 · konvansiyon — API2-07 | eşlendi |
| `04` 6.3.20.5 | Kaldırılan yönetici paneldeyken: oturumu kapanır, bir sonraki işlemde E-29 | UC-84 · konvansiyon — API2-03 | eşlendi |
| `04` 6.3.21 | E-50 bloğu | UC-104 | eşlendi |
| `04` 6.3.21.1 | Değişiklik yok: üç bölüm; adı değiştirmek, e-posta ve şifre → E-54 | UC-104 · UC-178 · UC-108 · UC-179 · UC-181 | eşlendi |
| `04` 6.3.21.2 | Bekleyen e-posta değişikliği: yeni adres ve son gün; bağlantıyı yeniden istemek — L-9 | UC-104 · UC-180 · konvansiyon — API2-08 | eşlendi |
| `04` 6.3.21.3 | Geri alma bağlantısı açık (Z-45): "E-postayı değiştir" kapalı; ad ve şifre açık | UC-104 · UC-178 · UC-181 · konvansiyon — API8-01 | eşlendi |
| `04` 6.3.22 | E-51 bloğu | UC-105 | eşlendi |
| `04` 6.3.22.1 | Seçili dönemde sipariş var: sayılar, ölçüler, dağılımlar; dönem seçmek | UC-105 · konvansiyon — API9-05 | eşlendi |
| `04` 6.3.22.2 | Siparişsiz dönem, ilk kurulum, payda sıfır: sayılar sıfır, oranlar "Değerlendirilemez" | UC-105 · konvansiyon — API9-05 | eşlendi |
| `04` 6.3.22.3 | Satış hiç açılmadı: ölçüm penceresi seçeneği görünmez | UC-105 · konvansiyon — API9-05 | eşlendi |
| `04` 6.3.23 | E-52 bloğu | UC-106 | eşlendi |
| `04` 6.3.23.1 | Satır var: en yeni önce, kaldırılmış yönetici adıyla; süzmek | UC-106 · konvansiyon — API9-02 · konvansiyon — API9-05 | eşlendi |
| `04` 6.3.23.2 | Süzgecin sonucu boş, ilk işlemlerden önce: boş hâl satırı; süzgeci kaldırmak | UC-106 · konvansiyon — API9-02 | eşlendi |
| `04` 6.3.24 | E-53 bloğu | UC-107 | eşlendi |
| `04` 6.3.24.1 | Dışa aktarılacak kayıt var: sorumluluk satırı ve üç bölüm; dışa aktarmak | UC-107 · UC-182 · UC-183 · UC-184 | eşlendi |
| `04` 6.3.24.2 | Aralıkta sipariş yok, üye ya da talep yok: dosya üretilmez, bölümde mesaj; aralığı değiştirmek | UC-182 · UC-183 · UC-184 · UC-107 · konvansiyon — API9-05 | eşlendi |
| `04` 6.3.25 | E-54 bloğu | UC-108 | eşlendi |
| `04` 6.3.25.1 | Yeniden doğrulama, oturum açık: işlemin adı ve şifre; doğrulamak → E-50 | UC-108 · UC-185 · UC-104 | eşlendi |
| `04` 6.3.25.2 | L-1 aşıldı, oturum kapandı: nötr limit mesajı ya da girişe | UC-185 · UC-84 · konvansiyon — API2-08 · konvansiyon — API2-03 | eşlendi |
| `04` 6.3.25.3 | Yeni adres doğrulandı, bağlantı geçersiz: geçerli bağlantıda doğrulama adımı ve tek düğme; sonra oturum açıksa E-50, değilse E-29 | UC-108 · UC-186 · UC-104 · UC-84 | eşlendi |
| `04` 6.3.25.4 | Geri alındı, geri alma bağlantısı geçersiz: geri alma adımı ve tek düğme; sonra yeni şifre bağlantıyla E-29'da | UC-108 · UC-187 · UC-84 | eşlendi |

#### 1.1.8 Arayüz Tanımları — `04 §7` formlar ve §8 lokalizasyon (243)

Önce §7.1'in ve §7.2'nin formları blok blok — formun başlığı (`04` 7.n.m) formun POST adayıdır, alan satırı (`04` 7.n.m.k) aynı adaya eşlenir ve formun girdi modelinin alanıdır (API5-01'in envanteri); sorgu formları (7.1.1, 7.2.3, 7.2.25) listenin GET adayına ve parametre konvansiyonuna (API2-06, API9-02, API9-05) —, sonra §7.3'ün ortak alan kuralları ve §8 alt bölüm düzeyinde. §7.3'ün kuralı tek bir uca değil bütün form uçlarına düşer: alan türlerinin denetimi `konvansiyon — API5-01` (girdi modelinin bağlanması ve doğrulaması), biçimleri API2-09, deneme limitleri API2-08, gönderimin davranışı API2-07 ve API2-05; 7.3.2'nin "Kullanıldığı formlar" sütunu kuralı formlara bağlar ve kural form satırlarında yinelenmez (K-1156). Aday anan §7.3 satırları kendi uçlarını taşır: görsel ve dosya (7.3.2.18), kupon kodu (7.3.2.19), limitlerin satırları (7.3.3.n). §8.2'nin biçimleri istekte ve yanıtta API2-09'un konusudur; §8.1 çoklu dilin yokluğudur — adreslerin dil öneki taşımaması API2-01'e düşer —, §8.3 yerleşimdir.

| Kaynak ID | Kaynak özeti | Uç | Durum |
|---|---|---|---|
| `04` 7.1.1 | E-02 · E-03 süzgeç ve arama formu — sorgu parametreleri | UC-02 · UC-03 · konvansiyon — API2-06 | eşlendi |
| `04` 7.1.1.1 | Fiyat aralığı — isteğe bağlı, tutar; alt değer üstten büyükse süzgeç uygulanmaz; alan mesajı | UC-02 · UC-03 · konvansiyon — API2-06 · konvansiyon — API2-09 | eşlendi |
| `04` 7.1.1.2 | Stok durumu seçimi — varsayılan tükenmişleri de gösterir | UC-02 · UC-03 · konvansiyon — API2-06 | eşlendi |
| `04` 7.1.1.3 | Arama sözcüğü — zorunlu, boş arama gönderilmez; ürün adında, harf ve Türkçe karaktere duyarsız (çerçevenin araması dahil) | UC-03 · konvansiyon — API2-06 | eşlendi |
| `04` 7.1.2 | E-04 sepete ekleme formu | UC-05 | eşlendi |
| `04` 7.1.2.1 | Varyantın seçenek boyutları — seçenek varsa zorunlu; tükenmiş ve açılmamış kombinasyon seçilemez; seçilmemiş boyutta alan mesajı | UC-05 · konvansiyon — API2-07 | eşlendi |
| `04` 7.1.2.2 | Adet — dijitalde yok; üst sınır stok ve P-9; yerinde kalıcı mesaj | UC-05 · konvansiyon — API2-07 | eşlendi |
| `04` 7.1.3 | E-10 iletişim formu | UC-15 | eşlendi |
| `04` 7.1.3.1 | Ad — zorunlu, üyede ön dolu; L-4 | UC-15 · UC-14 | eşlendi |
| `04` 7.1.3.2 | E-posta — zorunlu, üyede ön dolu; L-4 | UC-15 · UC-14 | eşlendi |
| `04` 7.1.3.3 | Telefon — isteğe bağlı | UC-15 | eşlendi |
| `04` 7.1.3.4 | Konu tipi — zorunlu, kapalı beş değer | UC-15 · konvansiyon — API5-02 | eşlendi |
| `04` 7.1.3.5 | Mesaj — zorunlu, uzun metin | UC-15 | eşlendi |
| `04` 7.1.3.6 | Görünmez tuzak alan — doluysa gönderim sessizce düşer, gönderim sonrası hâli gösterilir | UC-15 | eşlendi |
| `04` 7.1.4 | E-12 sepetin adet alanı | UC-20 | eşlendi |
| `04` 7.1.4.1 | Kalemin adedi — dijitalde yok; artırma sepete eklemenin denetiminden geçer; satırda yerinde kalıcı mesaj | UC-20 · UC-19 | eşlendi |
| `04` 7.1.5 | E-13 ödeme adımı formu: eksik ya da hatalı bölümde sipariş oluşmaz, hatalar birlikte, ekran ilk eksiğe | UC-27 · konvansiyon — API2-07 | eşlendi |
| `04` 7.1.5.1 | İletişim e-postası — misafirde zorunlu, üyede yok; doğrulama kodu yok; kayıtlı adreste hatırlatma — engel değil | UC-27 · UC-25 | eşlendi |
| `04` 7.1.5.2 | Teslimat adresi — fiziksel kalemde zorunlu, üyede defterden seçim ya da yeni; il teslimat yapılan illerden | UC-27 · UC-25 · UC-22 | eşlendi |
| `04` 7.1.5.3 | Teslimat telefonu — fiziksel kalemde zorunlu; telefonsuz defter kaydında burada istenir | UC-27 | eşlendi |
| `04` 7.1.5.4 | Yeni adresi deftere kaydetme — üyede isteğe bağlı, kendiliğinden işaretlenmez | UC-27 | eşlendi |
| `04` 7.1.5.5 | "Teslimat adresimle aynı" — işaretli gelir; kaldırılınca fatura formu | UC-27 · UC-25 · konvansiyon — API8-02 | eşlendi |
| `04` 7.1.5.6 | Fatura adresi — işaret kaldırıldıysa ya da fiziksel kalem yoksa zorunlu; telefon, posta kodu, T.C. kimlik no yok | UC-27 | eşlendi |
| `04` 7.1.5.7 | Kupon kodu — harf ayrımsız, en fazla bir; geçersiz kodda ve asgari tutarın altında ayrı mesaj; L-6 | UC-23 · konvansiyon — API2-08 | eşlendi |
| `04` 7.1.5.8 | Ödeme yöntemi — zorunlu, kullanılamayan seçilemez; L-8 (havale) | UC-27 · konvansiyon — API2-08 | eşlendi |
| `04` 7.1.5.9 | Onay kutuları — işaretli gelmez; metin sürümü değişince ya da özet farkı içeriği değiştirince iki yasal metnin kutusunun işareti kalkar | UC-27 · UC-22 | eşlendi |
| `04` 7.1.5.10 | Onay — sepet onay anında yeniden değerlendirilir; özet farkı, satış kapalı, L-7 yerinde kalıcı | UC-27 · konvansiyon — API2-05 · konvansiyon — API2-08 | eşlendi |
| `04` 7.1.6 | E-15 sipariş takibi formu | UC-32 | eşlendi |
| `04` 7.1.6.1 | Sipariş numarası — zorunlu, tek başına açmaz; eşleşmeyende nötr metin; L-5 | UC-32 · konvansiyon — API2-08 | eşlendi |
| `04` 7.1.6.2 | E-posta — zorunlu; L-5 | UC-32 | eşlendi |
| `04` 7.1.7 | E-16 geri ödeme IBAN'ı formu | UC-35 | eşlendi |
| `04` 7.1.7.1 | IBAN — istekle açılan alan ya da düzeltme; kart iadesi gerçekleşmeyip havale yolu açıldığında isteğe bağlı; işlenene kadar düzeltilir | UC-35 | eşlendi |
| `04` 7.1.8 | E-17 iptal ve gecikme feshi formu | UC-39 · UC-40 · UC-41 | eşlendi |
| `04` 7.1.8.1 | Kalem seçimi — kalem iptalinde en az bir; siparişin iptalinde ve fesihte seçim yok; yalnız iptale açık kalemler | UC-40 · UC-39 · UC-41 | eşlendi |
| `04` 7.1.8.2 | Adet — adedi birden büyükte zorunlu, fesihte yok; üst sınır iptale açık adet, varsayılan tamamı | UC-40 · konvansiyon — API8-01 | eşlendi |
| `04` 7.1.8.3 | IBAN — ödenmiş havale siparişinde zorunlu, kartta yok; boş gelir | UC-40 · UC-41 | eşlendi |
| `04` 7.1.9 | E-18 cayma beyanı formu | UC-43 | eşlendi |
| `04` 7.1.9.1 | Kalem seçimi — en az bir; yalnız caymaya açık kalemler | UC-43 | eşlendi |
| `04` 7.1.9.2 | Adet — üst sınır caymaya açık adet, varsayılan tamamı | UC-43 · konvansiyon — API8-01 | eşlendi |
| `04` 7.1.9.3 | IBAN — havale hattında zorunlu | UC-43 | eşlendi |
| `04` 7.1.10 | E-19 ayıp talebi formu | UC-45 · UC-46 | eşlendi |
| `04` 7.1.10.1 | Kalem — tek kalem, talebin açık olduğu kalemlerden | UC-45 | eşlendi |
| `04` 7.1.10.2 | Ayıplı adet — varsayılan bir, üst sınır talebe açık adet, teslim işaretine bağlı değil | UC-45 · konvansiyon — API8-01 | eşlendi |
| `04` 7.1.10.3 | Sorunun açıklaması — yeni talepte zorunlu, yeniden açmada alan yok; dosya eki yok | UC-45 · UC-46 | eşlendi |
| `04` 7.1.11 | E-20 kayıt formu | UC-48 | eşlendi |
| `04` 7.1.11.1 | Ad — zorunlu; L-3 | UC-48 | eşlendi |
| `04` 7.1.11.2 | E-posta — müşteri hesaplarında, bekleyen kayıt dahil tekil; kayıtlıda "bu e-posta zaten kayıtlı" ve "Giriş yap" | UC-48 · konvansiyon — API2-07 | eşlendi |
| `04` 7.1.11.3 | Şifre — zorunlu | UC-48 | eşlendi |
| `04` 7.1.11.4 | Bağlantıyı yeniden istemek — yeni bağlantı kaydın ömrünü uzatmaz; K-779'un ikinci metni; L-9 | UC-49 · konvansiyon — API2-08 | eşlendi |
| `04` 7.1.12 | E-22 müşteri girişi formu | UC-55 | eşlendi |
| `04` 7.1.12.1 | E-posta — yalnız müşteri hesapları aranır; yanlış bilgide formun başında K-779'un dördüncü metni; L-1 | UC-55 · konvansiyon — API2-03 · konvansiyon — API2-07 | eşlendi |
| `04` 7.1.12.2 | Şifre — zorunlu; L-1 | UC-55 | eşlendi |
| `04` 7.1.12.3 | "Beni hatırla" — işaretsiz gelir, oturumun süresini seçer (Z-4, Z-5) | UC-55 · konvansiyon — API2-03 | eşlendi |
| `04` 7.1.12.4 | Bağlantıyı yeniden istemek — doğrulanmamış hesapla giriş; L-9 | UC-49 · konvansiyon — API2-08 | eşlendi |
| `04` 7.1.13 | E-23 şifre sıfırlama formları | UC-57 · UC-59 | eşlendi |
| `04` 7.1.13.1 | E-posta — isteme adımı; kayıtlı olup olmadığı söylenmez; nötr metin; L-2 | UC-57 · konvansiyon — API2-08 | eşlendi |
| `04` 7.1.13.2 | Yeni şifre — kurma adımı | UC-59 | eşlendi |
| `04` 7.1.14 | E-24 yeniden doğrulama formu | UC-61 | eşlendi |
| `04` 7.1.14.1 | Şifre — şifreli hesapta zorunlu, Google'da alan yok; yanlış şifre alan mesajı; L-1 (Google girmez) | UC-61 · UC-62 · konvansiyon — API2-08 | eşlendi |
| `04` 7.1.15 | E-25 hesap formları | UC-64 · UC-65 · UC-66 · UC-67 | eşlendi |
| `04` 7.1.15.1 | Ad — yeniden doğrulama istemez | UC-64 | eşlendi |
| `04` 7.1.15.2 | Yeni e-posta — müşteri hesapları içinde tekil, başka hesaptakinde K-786'nın metni; L-9 | UC-66 · konvansiyon — API2-08 | eşlendi |
| `04` 7.1.15.3 | Yeni şifre | UC-65 | eşlendi |
| `04` 7.1.15.4 | Bağlantıyı yeniden istemek — bekleyen değişiklik; L-9 | UC-67 · konvansiyon — API2-08 | eşlendi |
| `04` 7.1.16 | E-26 adres defteri formu | UC-71 · UC-72 | eşlendi |
| `04` 7.1.16.1 | Adres adı — zorunlu, serbest kısa metin | UC-71 · UC-72 | eşlendi |
| `04` 7.1.16.2 | Alıcının adı ve soyadı, il, ilçe, açık adres — zorunlu | UC-71 · UC-72 | eşlendi |
| `04` 7.1.16.3 | Telefon — isteğe bağlı | UC-71 · UC-72 | eşlendi |
| `04` 7.2.1 | E-29 panel girişi ve şifre sıfırlama formları | UC-110 · UC-111 · UC-112 | eşlendi |
| `04` 7.2.1.1 | E-posta — giriş; yalnız yönetici hesapları aranır; yanlış bilgide K-779'un dördüncü metni; L-1, panele muafiyet yok | UC-110 · konvansiyon — API2-03 · konvansiyon — API2-08 | eşlendi |
| `04` 7.2.1.2 | Şifre — giriş; L-1 | UC-110 | eşlendi |
| `04` 7.2.1.3 | "Beni hatırla" — işaretsiz gelir (Z-4, Z-5) | UC-110 · konvansiyon — API2-03 | eşlendi |
| `04` 7.2.1.4 | E-posta — sıfırlama isteği; nötr metin; L-2 | UC-111 · konvansiyon — API2-08 | eşlendi |
| `04` 7.2.1.5 | Yeni şifre | UC-112 | eşlendi |
| `04` 7.2.2 | E-30 davet kabulü formu | UC-113 | eşlendi |
| `04` 7.2.2.1 | E-posta — salt okunur, davetin adresi | UC-113 · UC-85 | eşlendi |
| `04` 7.2.2.2 | Ad — zorunlu | UC-113 | eşlendi |
| `04` 7.2.2.3 | Şifre — zorunlu | UC-113 | eşlendi |
| `04` 7.2.3 | Panel listelerinin araması ve süzgeçleri — E-32, E-36, E-38, E-40, E-52 | UC-87 · UC-91 · UC-93 · UC-95 · UC-106 · konvansiyon — API9-02 · konvansiyon — API2-06 | eşlendi |
| `04` 7.2.3.1 | E-32 — ürün adı araması ve yayın durumu süzgeci; biçim denetimi yok, boşta boş hâl satırı | UC-87 · konvansiyon — API9-02 | eşlendi |
| `04` 7.2.3.2 | E-36 — sipariş numarası ya da iletişim e-postası, tam eşleşme, e-postada harf ayrımı yok; üç süzgeç | UC-91 · konvansiyon — API9-02 | eşlendi |
| `04` 7.2.3.3 | E-38 — tür ve durum süzgeci, kapalı değerler | UC-93 · konvansiyon — API9-02 · konvansiyon — API5-02 | eşlendi |
| `04` 7.2.3.4 | E-40 — e-posta araması, müşteri hesaplarında tam eşleşme; biçim mesajı, bulunamazsa K-811'in satırı | UC-95 · konvansiyon — API9-02 | eşlendi |
| `04` 7.2.3.5 | E-52 — tarih aralığı (ileri tarih yok) ve yönetici kapalı listeden | UC-106 · konvansiyon — API9-02 · konvansiyon — API2-09 | eşlendi |
| `04` 7.2.4 | E-33 ürün formu: zorunlu alanı boşaltan ya da yayın kapısını bozan kayıt kaydedilmez | UC-114 · konvansiyon — API8-01 | eşlendi |
| `04` 7.2.4.1 | Tip — fiziksel, dijital, hizmet | UC-114 · konvansiyon — API5-02 | eşlendi |
| `04` 7.2.4.2 | Ad — kısa metin, P-27 | UC-114 | eşlendi |
| `04` 7.2.4.3 | Kategoriler ve ana kategori — yayın için zorunlu ana kategori; yan sütunda kapının koşulu | UC-114 · UC-115 | eşlendi |
| `04` 7.2.4.4 | Açıklama — uzun metin, kapalı biçim seti | UC-114 | eşlendi |
| `04` 7.2.4.5 | Görseller ve alternatif metin — P-22, P-23, P-24; boş alternatif metin üretilir | UC-117 · UC-114 · konvansiyon — API2-08 | eşlendi |
| `04` 7.2.4.6 | Seçenek boyutları ve değerleri — en fazla P-21 boyut | UC-114 | eşlendi |
| `04` 7.2.4.7 | Varyantın stok kodu — zorunlu, tekil; ad girilince öneri | UC-114 | eşlendi |
| `04` 7.2.4.8 | Varyantın fiyatı — tutar, sıfırdan büyük; indirim yürürken indirimli fiyatı sıfıra indiren fiyat reddedilir | UC-114 · konvansiyon — API2-09 | eşlendi |
| `04` 7.2.4.9 | Stok ve kontenjan — tam sayı, ayrılmış adedin altına inmez; yerinde kalıcı mesaj ve siparişler; ekranın gösterdiği değere karşı yazılır | UC-114 · konvansiyon — API2-04 | eşlendi |
| `04` 7.2.4.10 | Varyant görseli ve dijital dosya — P-26; yayındaki dijital varyantta dosya zorunlu; dosyasız bırakan silmede yerinde kalıcı | UC-117 · UC-83 · UC-114 · UC-116 | eşlendi |
| `04` 7.2.4.11 | KDV oranı — varsayılanla (P-10) | UC-114 | eşlendi |
| `04` 7.2.4.12 | Üretim yeri — fizikselde zorunlu, ülke listesinden | UC-114 · konvansiyon — API5-02 | eşlendi |
| `04` 7.2.4.13 | Ölçü birimi ve varyantın net miktarı — birim kapalı listeden, seçilirse net miktar zorunlu, en çok üç ondalık | UC-114 · konvansiyon — API5-02 · konvansiyon — API2-09 | eşlendi |
| `04` 7.2.4.14 | Kargoya verme süresi — ürün düzeyi, boşsa firma varsayılanı; P-5'in çiti | UC-114 | eşlendi |
| `04` 7.2.4.15 | İfa süresi — hizmette yayın için zorunlu (P-44) | UC-114 · UC-115 | eşlendi |
| `04` 7.2.4.16 | Bir siparişte en fazla adet — P-9'un çiti | UC-114 | eşlendi |
| `04` 7.2.4.17 | Cayma istisnası ve sebebi — işaretlenince sebep zorunlu, kapalı liste | UC-114 · konvansiyon — API5-02 | eşlendi |
| `04` 7.2.4.18 | İndirim — yüzde (%100 olamaz), başlangıç ve bitiş; indirimli fiyat referansın altında ve sıfırın üstünde | UC-114 · konvansiyon — API2-09 | eşlendi |
| `04` 7.2.5 | E-34 kategori formu | UC-118 · UC-119 · UC-120 | eşlendi |
| `04` 7.2.5.1 | Ad — zorunlu, kısa metin | UC-118 · UC-119 | eşlendi |
| `04` 7.2.5.2 | Yeni üst kategori — taşıma; üç seviyeyi aşmaz, kendi alt dalına taşınmaz; yerinde kalıcı mesaj | UC-120 | eşlendi |
| `04` 7.2.6 | E-35 kupon formu | UC-122 | eşlendi |
| `04` 7.2.6.1 | Kod — kuponlar arasında tekil, harf ayrımsız | UC-122 | eşlendi |
| `04` 7.2.6.2 | Tür — yüzde ya da sabit tutar | UC-122 · konvansiyon — API5-02 | eşlendi |
| `04` 7.2.6.3 | Değer — sıfırdan büyük, yüzdesel %100'e ulaşamaz | UC-122 | eşlendi |
| `04` 7.2.6.4 | Asgari sepet tutarı — sabit tutarlıda zorunlu ve kupon tutarından büyük | UC-122 | eşlendi |
| `04` 7.2.6.5 | Başlangıç ve bitiş — tarih aralığı, ileri tarih serbest | UC-122 · konvansiyon — API2-09 | eşlendi |
| `04` 7.2.6.6 | Toplam kullanım adedi — kullanılmış ve ayrılmış hakların altına inemez | UC-122 | eşlendi |
| `04` 7.2.7 | E-37 kargoya verme ve yeniden verme formu | UC-124 · UC-125 | eşlendi |
| `04` 7.2.7.1 | Kargo şirketi — araç beyanı yoksa zorunlu; ürünle gelen liste ya da "Diğer" ve ad | UC-124 · UC-125 · konvansiyon — API5-02 | eşlendi |
| `04` 7.2.7.2 | Takip numarası — kargo şirketiyle zorunlu | UC-124 · UC-125 | eşlendi |
| `04` 7.2.7.3 | "Kendi aracımızla teslim" — iki alanın yerine geçer | UC-124 · UC-125 | eşlendi |
| `04` 7.2.8 | E-37 teslim tarihi ve ulaşma tarihi formu | UC-126 · UC-148 · UC-140 · UC-149 | eşlendi |
| `04` 7.2.8.1 | Teslim tarihi — işaret ve düzeltme; ileri ya da kargo gününden önce olamaz | UC-126 · UC-148 · konvansiyon — API2-09 | eşlendi |
| `04` 7.2.8.2 | Sıra sorusu — tarih bir cayma beyanının günüyle çakışırsa zorunlu | UC-126 · UC-148 | eşlendi |
| `04` 7.2.8.3 | Ulaşma tarihi — teslim almada ve düzeltmede; ileri ya da teslimden önce olamaz, kargodan dönende alt sınır yok | UC-140 · UC-149 | eşlendi |
| `04` 7.2.9 | E-37 firma iptali formu | UC-129 | eşlendi |
| `04` 7.2.9.1 | Kapsam — kalemler; ödemeden sonra en az bir, ödeme beklenirken siparişin tamamı | UC-129 · konvansiyon — API8-01 | eşlendi |
| `04` 7.2.9.2 | Adet — varsayılan tamamı | UC-129 · konvansiyon — API8-01 | eşlendi |
| `04` 7.2.9.3 | Sebep — kapalı liste (OB-15); boşken onay açılmaz | UC-129 · konvansiyon — API5-02 · konvansiyon — API2-02 | eşlendi |
| `04` 7.2.9.4 | Açıklama — "diğer"de zorunlu | UC-129 | eşlendi |
| `04` 7.2.10 | E-37 başka kanaldan gelen bildirimin kaydı formu | UC-130 | eşlendi |
| `04` 7.2.10.1 | Bildirimin türü — cayma ya da gecikme feshi | UC-130 | eşlendi |
| `04` 7.2.10.2 | Kalem ve adet — cayma; kargoya verilmiş fiziksel ve onaylı hizmet kalemi; kargoya verilmemiş kalemde yolun iptal olduğu | UC-130 · konvansiyon — API8-01 | eşlendi |
| `04` 7.2.10.3 | Bildirimin firmaya ulaştığı tarih — ileri tarih ve kabul süresinin dışı reddedilir | UC-130 · konvansiyon — API2-09 | eşlendi |
| `04` 7.2.10.4 | Siparişin e-postasından geldiğini söyleyen seçenek ve IBAN — işaretlenince IBAN açılır, işaretsizse IBAN'sız | UC-130 | eşlendi |
| `04` 7.2.10.5 | Ayrı onayın kutusu — tarih pencere dışındaysa ya da kalem mutlak istisnalıysa zorunlu; işaretlenmeden onay penceresi açılmaz | UC-130 · konvansiyon — API2-02 | eşlendi |
| `04` 7.2.10.6 | Sıra sorusu — tarih kalemin teslim günüyse | UC-130 | eşlendi |
| `04` 7.2.11 | E-37 kalem çıkarma ve "mal dönmedi" kapanışı formu | UC-138 · UC-142 | eşlendi |
| `04` 7.2.11.1 | Çıkarılan adet — üst sınır açık adet, sebep listesi yok | UC-138 | eşlendi |
| `04` 7.2.11.2 | Kapatılan adet — üst sınır ve varsayılan ulaşmamış adet | UC-142 | eşlendi |
| `04` 7.2.12 | E-37 iade teslim alma ve ret formu | UC-140 · UC-141 | eşlendi |
| `04` 7.2.12.1 | Ulaşma tarihi — 7.2.8.3'ün kuralı | UC-140 | eşlendi |
| `04` 7.2.12.2 | Ulaşan adet — varsayılan ulaşmamış adet; ayıpta teslim alınmamış ayıplı adet | UC-140 | eşlendi |
| `04` 7.2.12.3 | Teslim al ya da "iade reddedildi — koruyucu ambalaj açılmış" — koşullu istisnada; ret onay ister | UC-140 · konvansiyon — API2-02 | eşlendi |
| `04` 7.2.12.4 | Stoğa eklenen adet — teslim alınan kadar; varyant silinmişse ve retta yok | UC-141 | eşlendi |
| `04` 7.2.13 | E-37 tutar bazlı geri ödeme ve IBAN isteği formu | UC-133 · UC-134 | eşlendi |
| `04` 7.2.13.1 | Tutar — ödenmiş ve geri ödenmemiş tutarı aşamaz | UC-133 · konvansiyon — API2-09 | eşlendi |
| `04` 7.2.13.2 | Bağlı kalem — isteğe bağlı; açık "mal dönmedi" kapanışı varsa kapanış da seçilir | UC-133 | eşlendi |
| `04` 7.2.13.3 | IBAN isteğinin kalemi ve sebebi — havale hattında IBAN'sız geri ödeme için | UC-134 | eşlendi |
| `04` 7.2.14 | E-37 düzeltmeler formu: hiçbiri onay istemez; adres ve misafir e-postasında teyit hatırlatması | UC-146 · UC-147 · UC-150 · UC-151 · konvansiyon — API2-02 | eşlendi |
| `04` 7.2.14.1 | Teslimat ve fatura adresi, teslimat telefonu — OB-13'ün panel varyantı | UC-146 | eşlendi |
| `04` 7.2.14.2 | Kargo şirketi ve takip numarası — 7.2.7'nin kuralı | UC-147 | eşlendi |
| `04` 7.2.14.3 | Durum düzeltmesi — dönülecek durum, sebep, açıklama; kapalı liste; yapılamayan düzeltme engellenir | UC-150 · konvansiyon — API5-02 · konvansiyon — API8-01 | eşlendi |
| `04` 7.2.14.4 | Misafir siparişinin yeni e-postası | UC-151 | eşlendi |
| `04` 7.2.15 | E-37 iç not formu | UC-152 | eşlendi |
| `04` 7.2.15.1 | İç not — tek serbest metin, müşteriye görünmez; ekranın gösterdiği değere karşı yazılır | UC-152 · konvansiyon — API2-04 | eşlendi |
| `04` 7.2.16 | E-41 kurumsal içerik formu: "yayın için zorunlu" yayını engeller; adsız taslak kaydedilmez — Hakkımızda ve duyuru dışında | UC-158 · UC-159 | eşlendi |
| `04` 7.2.16.1 | Ad, başlık, soru — yayın için zorunlu, taslakta da (Hakkımızda ve duyuru dışında); P-27, P-28; yayın engelinde K-812'nin metni | UC-158 · UC-159 | eşlendi |
| `04` 7.2.16.2 | Kısa açıklama, kısa tanıtım, çalışma saatleri — Hakkımızda'nın kısa tanıtımı yayın için zorunlu; P-28, P-29 | UC-158 · UC-159 | eşlendi |
| `04` 7.2.16.3 | Uzun metin — SSS cevabı yayın için zorunlu; kapalı biçim seti | UC-158 · UC-159 | eşlendi |
| `04` 7.2.16.4 | Duyuru metni — yayın için zorunlu, P-30 | UC-158 · UC-159 | eşlendi |
| `04` 7.2.16.5 | Şubenin adı ve adresi — il kapalı listeden; yayın için zorunlu | UC-158 · UC-159 | eşlendi |
| `04` 7.2.16.6 | Şubenin telefonu — isteğe bağlı | UC-158 | eşlendi |
| `04` 7.2.16.7 | Bağlantı — duyurunun, şubenin haritası | UC-158 | eşlendi |
| `04` 7.2.16.8 | Duyurunun başlangıcı ve bitişi — ileri tarih serbest | UC-158 · konvansiyon — API2-09 | eşlendi |
| `04` 7.2.16.9 | Görseller ve alternatif metin — P-22, P-23, P-25 | UC-117 · UC-158 · konvansiyon — API2-08 | eşlendi |
| `04` 7.2.16.10 | İlgili ürünler, "ana sayfada göster", "menüde göster" — katalogdan arayarak seçim | UC-158 · UC-87 | eşlendi |
| `04` 7.2.17 | E-42 ana sayfa ve menü formu | UC-162 | eşlendi |
| `04` 7.2.17.1 | Ana sayfa düzeni — iki seçenek, kurulumda seçili | UC-162 | eşlendi |
| `04` 7.2.17.2 | Menü adları — boş kaydedilmez, K-813'ün metni | UC-162 | eşlendi |
| `04` 7.2.17.3 | Sosyal bağlantılar — bağlantı biçimi | UC-162 | eşlendi |
| `04` 7.2.17.4 | WhatsApp numarası — telefon biçimi | UC-162 | eşlendi |
| `04` 7.2.18 | E-43 marka formu | UC-163 | eşlendi |
| `04` 7.2.18.1 | Marka adı — bir kez girildikten sonra boşaltılamaz | UC-163 | eşlendi |
| `04` 7.2.18.2 | Logo ve site simgesi — P-22, P-23 | UC-117 · UC-163 · konvansiyon — API2-08 | eşlendi |
| `04` 7.2.18.3 | Marka rengi — tek renk seçimi, boşaltılabilir | UC-163 | eşlendi |
| `04` 7.2.18.4 | Platform imzası — açık gelir | UC-163 | eşlendi |
| `04` 7.2.19 | E-44 firma kimliği formu: dolu zorunlu alan boşaltılamaz; ürün numaraların doğruluğunu denetlemez | UC-164 | eşlendi |
| `04` 7.2.19.1 | Firma tipi — kapalı liste: Esnaf · Gerçek kişi tacir · Tüzel kişi | UC-164 · konvansiyon — API5-02 | eşlendi |
| `04` 7.2.19.2 | Tipin zorunlu alanları — seçili tipe göre | UC-164 | eşlendi |
| `04` 7.2.19.3 | Adres — il kapalı listeden, ilçe, açık adres | UC-164 | eşlendi |
| `04` 7.2.19.4 | Telefon — zorunlu | UC-164 | eşlendi |
| `04` 7.2.19.5 | E-posta ve KEP adresi — zorunlu; KEP'in geçerliliği doğrulanmaz | UC-164 | eşlendi |
| `04` 7.2.19.6 | Meslek odası, işletme adı ya da marka, davranış kuralları — isteğe bağlı | UC-164 | eşlendi |
| `04` 7.2.19.7 | ETBİS doğrulama bilgisi — bağlantı | UC-164 | eşlendi |
| `04` 7.2.20 | E-45 ödeme yöntemleri formu | UC-167 | eşlendi |
| `04` 7.2.20.1 | Havale açık ya da kapalı — IBAN'sız açılmaz, K-817'nin metni | UC-167 | eşlendi |
| `04` 7.2.20.2 | Firmanın IBAN'ı — havale açıkken zorunlu, boşaltılamaz | UC-167 | eşlendi |
| `04` 7.2.21 | E-46 kargo, süreler ve varsayılanlar formu: her bölüm kendi kaydıyla denetlenir | UC-168 · UC-169 · UC-170 · UC-171 · UC-172 · konvansiyon — API2-04 | eşlendi |
| `04` 7.2.21.1 | Kargo ücreti (P-1) — sıfır ya da büyük | UC-168 | eşlendi |
| `04` 7.2.21.2 | Ücretsiz kargo eşiği (P-3), asgari sipariş tutarı (P-4) — boşken özellik kapalı | UC-168 | eşlendi |
| `04` 7.2.21.3 | Teslimat illeri — varsayılan tüm Türkiye; hiçbiri seçili değilse kayıt engellenmez, satır uyarır | UC-169 | eşlendi |
| `04` 7.2.21.4 | Kargoya verme süresi (P-5), havale ödeme süresi (P-7) — iş günü, çitler; K-817'nin iki çit metni | UC-170 | eşlendi |
| `04` 7.2.21.5 | İndirme hakkı (P-8), KDV oranı varsayılanı (P-10) — P-8'in çiti | UC-171 | eşlendi |
| `04` 7.2.21.6 | İade adresi — alıcının adı, il, ilçe, açık adres; girildikten sonra boşaltılamaz | UC-172 | eşlendi |
| `04` 7.2.21.7 | İade adresinin telefonu — isteğe bağlı | UC-172 | eşlendi |
| `04` 7.2.22 | E-47 yasal metinler formu | UC-173 · UC-174 | eşlendi |
| `04` 7.2.22.1 | Aydınlatma metni ve çerez politikası — boşaltılamaz, kapalı biçim seti | UC-173 | eşlendi |
| `04` 7.2.22.2 | Barındırma ve e-posta altyapısının konumu — aydınlatmanın tamamlanması için zorunlu; tamamlanma satırı | UC-173 · UC-174 | eşlendi |
| `04` 7.2.23 | E-49 yönetici daveti formu | UC-175 | eşlendi |
| `04` 7.2.23.1 | Davet edilen e-posta — yönetici hesapları içinde kullanılmamış, müşteri hesabının adresi engel değil; K-820'nin metni ve satırı | UC-175 · konvansiyon — API2-07 | eşlendi |
| `04` 7.2.24 | E-50 yöneticinin kendi hesabı formları | UC-178 · UC-179 · UC-180 · UC-181 | eşlendi |
| `04` 7.2.24.1 | Ad — zorunlu | UC-178 | eşlendi |
| `04` 7.2.24.2 | Yeni e-posta — yönetici hesapları içinde tekil, K-819'un metni; L-9 | UC-179 · konvansiyon — API2-08 | eşlendi |
| `04` 7.2.24.3 | Yeni şifre | UC-181 | eşlendi |
| `04` 7.2.24.4 | Bağlantıyı yeniden istemek — K-779'un ikinci metni; L-9 | UC-180 · konvansiyon — API2-08 | eşlendi |
| `04` 7.2.25 | E-51 · E-53 dönem ve dışa aktarma aralığı | UC-105 · UC-182 · konvansiyon — API9-05 | eşlendi |
| `04` 7.2.25.1 | E-51 — dönem; "Son 30 gün" seçili, "Ölçüm penceresi" satış hiç açılmadıysa yok | UC-105 · konvansiyon — API9-05 | eşlendi |
| `04` 7.2.25.2 | E-51 özel aralık ve E-53 sipariş aralığı — iki tarih zorunlu, ileri tarih yok | UC-105 · UC-182 · konvansiyon — API9-05 · konvansiyon — API2-09 | eşlendi |
| `04` 7.2.26 | E-54 yeniden doğrulama formu | UC-185 | eşlendi |
| `04` 7.2.26.1 | Şifre — yalnız şifreyle; yanlış şifre alan mesajı; L-1 | UC-185 · konvansiyon — API2-08 | eşlendi |
| `04` 7.3.1 | Zorunluluk ve alan mesajının kalıbı: görünür etiket, zorunlu ve isteğe bağlı metinle ayrılır; "yayın için zorunlu" yayını engeller; mesaj alanın adını ve neyin yanlış olduğunu söyler, sınırın değeri parametreden | konvansiyon — API2-07 · konvansiyon — API5-01 | konvansiyon |
| `04` 7.3.2 | Alan türlerinin kuralları — bloklar bu satırlara işaret eder | konvansiyon — API5-01 | konvansiyon |
| `04` 7.3.2.1 | E-posta — tek adres, baştaki ve sondaki boşluk yok sayılır; tekillik kendi hesap türü içinde | konvansiyon — API5-01 · konvansiyon — API2-03 | konvansiyon |
| `04` 7.3.2.2 | Şifre — asgari uzunluk P-17, yaygın şifreler listesi, karakter dayatması yok | konvansiyon — API5-01 | konvansiyon |
| `04` 7.3.2.3 | Ad — boş olamaz | konvansiyon — API5-01 | konvansiyon |
| `04` 7.3.2.4 | Telefon — Türkiye numarası, on hane, "0" ya da "+90", boşluk, tire, parantez yok sayılır | konvansiyon — API5-01 · konvansiyon — API2-09 | konvansiyon |
| `04` 7.3.2.5 | Adres — dört alan zorunlu, ilçe serbest metin; teslimat adresinde il teslimat yapılan illerden | konvansiyon — API5-01 | konvansiyon |
| `04` 7.3.2.6 | İl — kapalı liste | konvansiyon — API5-01 · konvansiyon — API5-02 | konvansiyon |
| `04` 7.3.2.7 | IBAN — biçim ve sağlama basamağı, boşluklar yok sayılır; müşteri tarafında aydınlatma bağlantısı | konvansiyon — API5-01 · konvansiyon — API2-09 | konvansiyon |
| `04` 7.3.2.8 | Tutar — en çok iki ondalık; fiyat sıfırdan büyük, ayar tutarları sıfır ya da büyük; biçim 8.2.4 | konvansiyon — API5-01 · konvansiyon — API2-09 | konvansiyon |
| `04` 7.3.2.9 | Yüzde — en çok iki ondalık, tek ölçek yüzde; indirim ve kupon (0, 100), KDV [0, 100); biçim 8.2.3 | konvansiyon — API5-01 · konvansiyon — API2-09 | konvansiyon |
| `04` 7.3.2.10 | Adet — tam sayı; birden küçük ve üst sınırı aşan reddedilir; dijitalde ve adedi bir olanda alan yok | konvansiyon — API5-01 · konvansiyon — API8-01 | konvansiyon |
| `04` 7.3.2.11 | Süre ve olay tarihi — çitin içinde; olay tarihi ileri olamaz, alt sınırın altında reddedilir; biçim 8.2.1 | konvansiyon — API5-01 · konvansiyon — API2-09 | konvansiyon |
| `04` 7.3.2.12 | Tarih aralığı — bitiş başlangıçtan önce olamaz; ileri tarih indirimde, kuponda ve duyuruda serbest, rapor ve iz süzgeçlerinde reddedilir | konvansiyon — API5-01 · konvansiyon — API2-06 · konvansiyon — API2-09 | konvansiyon |
| `04` 7.3.2.13 | Bağlantı — "http://" ya da "https://" ile başlayan tam adres | konvansiyon — API5-01 | konvansiyon |
| `04` 7.3.2.14 | Kısa metin — karakter tavanları P-27…P-30 | konvansiyon — API5-01 | konvansiyon |
| `04` 7.3.2.15 | Uzun metin — tavan yok, kapalı biçim seti yalnız `02 §3.11.6`'nın alanlarında, müşteri formları düz metin | konvansiyon — API5-01 | konvansiyon |
| `04` 7.3.2.16 | Kapalı liste — ürünle gelir, firma düzenlemez, serbest metin kabul etmez | konvansiyon — API5-01 · konvansiyon — API5-02 | konvansiyon |
| `04` 7.3.2.17 | Arama alanı — serbest metin, biçim denetimi yok; vitrinde harf ve Türkçe karakter duyarsız; boş sonuçta boş hâl satırı | konvansiyon — API2-06 · konvansiyon — API5-01 | konvansiyon |
| `04` 7.3.2.18 | Görsel ve dosya — P-22…P-26; kaydedilmeden bir günden eski kalıp silinmiş yüklemenin hâli | UC-117 · UC-83 · konvansiyon — API2-08 · konvansiyon — API6-02 · konvansiyon — API4-07 | eşlendi |
| `04` 7.3.2.19 | Kupon kodu — harf ayrımsız, panelde kuponlar arasında tekil | konvansiyon — API5-01 · UC-23 · UC-122 | eşlendi |
| `04` 7.3.3 | Deneme limitlerinin formlardaki karşılığı: yalnız o işlem engellenir, nötr limit mesajı düğmenin yanında yerinde kalıcı; eşik ve pencere `02 §11.2`'de | konvansiyon — API2-08 | konvansiyon |
| `04` 7.3.3.1 | L-1 — şifreyle giriş ve şifreyle yeniden doğrulama; tanınan tarayıcı e-posta ekseninde engellenmez, Google limite girmez | UC-55 · UC-110 · UC-61 · UC-185 · konvansiyon — API2-08 | eşlendi |
| `04` 7.3.3.2 | L-2 — şifre sıfırlama isteği | UC-57 · UC-111 · konvansiyon — API2-08 | eşlendi |
| `04` 7.3.3.3 | L-3 — hesap kaydı | UC-48 · konvansiyon — API2-08 | eşlendi |
| `04` 7.3.3.4 | L-4 — iletişim formu; yazılanlar durur | UC-15 · konvansiyon — API2-08 | eşlendi |
| `04` 7.3.3.5 | L-5 — başarısız sipariş takibi sorgusu; yol e-postadaki bağlantı ve sipariş geçmişi | UC-32 · UC-34 · UC-74 · konvansiyon — API2-08 | eşlendi |
| `04` 7.3.3.6 | L-6 — geçersiz kupon kodu; kupon alanı kapanır, sipariş kuponsuz sürer | UC-23 · konvansiyon — API2-08 | eşlendi |
| `04` 7.3.3.7 | L-7 — kendiliğinden iptal edilen ödenmemiş siparişler, onayda; sipariş oluşmaz, sepet durur | UC-27 · konvansiyon — API2-08 | eşlendi |
| `04` 7.3.3.8 | L-8 — açık ödenmemiş sipariş tavanı, havale; havale seçilemez, kart açık | UC-22 · UC-27 · konvansiyon — API2-08 | eşlendi |
| `04` 7.3.3.9 | L-9 — doğrulama bağlantısının yeniden istenmesi ve bekleyenin yerine geçen yeni e-posta isteği; önceki bağlantı geçerli kalır | UC-49 · UC-67 · UC-66 · UC-180 · UC-179 · konvansiyon — API2-08 | eşlendi |
| `04` 7.3.4 | Engelin dili: nötr mesaj kalan deneme, süre ve ekseni söylemez; hesap kilitlenmez; sayaç ya da geri sayım yok; yazılanlar durur | konvansiyon — API2-08 · konvansiyon — API2-07 | konvansiyon |
| `04` 7.3.5 | Gönderimin davranışı: hatalar birlikte, özet ve odak; düğme ikinci kez basılamaz; beklenmeyen hatada içerik korunur; üyeye ön dolu; yeni kişisel veri alanında aydınlatma bağlantısı, onay kutusu yok | konvansiyon — API2-07 · konvansiyon — API2-05 · konvansiyon — API5-01 | konvansiyon |
| `04 §8.1` | Tek dil Türkçe — dil seçimi yok, içerik dil başına alan taşımaz, adresler dil öneki taşımaz, tek yazı yönü, tek para birimi TRY; çoklu dil yol haritası adayı | uç doğurmaz — yoktur · konvansiyon — API2-01 | konvansiyon |
| `04 §8.2` | Türkçenin biçimleri — tarih, saat, sayı ve yüzde, tutar, süre, telefon ve IBAN, ekler, çoğul, hitap, harf ve alfabetik sıra; alana giriş biçimleri; vitrinde, panelde ve ekranlarda aynı | konvansiyon — API2-09 · konvansiyon — API2-06 | konvansiyon |
| `04 §8.3` | Metin uzunluğunun yerleşime etkisi — tavanlar yerleşimin dayanağı, metin kesilmez satır kırar (üç istisna), düğme metni kesilmez, kırılmayan bloklar | uç doğurmaz — görünüm | uç doğurmaz |

#### 1.1.9 Kullanıcı Akışları — `03 §1`–§4 durum makinesi, ana akışlar, hata akışları ve zaman aşımı (536)

Kaynağın sırasıyla: `03 §1` (durum makinesi — durumlar, geçişler, kalem kayıtları, panel işaretleri, durum × rol × işlem), §2 (müşteri tarafının ana akışları), §3 (hata akışları), §4 (zaman aşımı); kimlik biçimi K-1002'ninkidir (§1.1). Eşlemenin okunuşu (K-1157): akış adımı aktörün işleminin aday ucuna düşer — müşterinin ve yöneticinin işlemi ekranın POST adayına, sayfanın açılışı ve gösterdiği veri ekranın GET adayına; durum tablosunun satırı durumun görüldüğü ekranların GET adayına (sipariş sayfası UC-33, sipariş ayrıntısı UC-92 ve ötekiler) ve `konvansiyon — API5-02`'ye (durum adları `02 §5`'ten birebir); geçiş satırı tetikleyenin işlemine — müşterinin ya da yöneticinin adayına, ödeme sağlayıcısının bildirimine (UC-80) ya da *uç doğurmaz — sistem işi (`05 §5`)*'e. §1.11'in "açık olduğu aralık" satırı işlemin adayını ve `konvansiyon — API8-01`'i taşır; §3'ün hata satırı akış adımının adayını, gösterimi `konvansiyon — API2-07`'yi, deneme limiti `konvansiyon — API2-08`'i; §4'ün süre satırı süreyi gösteren ya da süre içinde açık kalan işlemin adayını, kendiliğinden işi *uç doğurmaz — sistem işi (`05 §5`)*'i. Satırın "Bildirim" hücresi ayrıca değer doğurmaz — e-posta uç değildir; *uç doğurmaz — e-posta (`08`)* yalnız bildirimin kendisini anlatan satırda yazılır. Ürünün dışında yürüyen adım (havalenin bankada yapılması, seçimlik hakların yürütümü, iade gönderisi) *uç doğurmaz — ürünün dışında*'dır.

| Kaynak ID | Kaynak özeti | Uç | Durum |
|---|---|---|---|
| `03 §1` metin | Durum makinesinin okunuşu: adlar, kodlar ve koşullar `02 §5`'te; bölüm geçişi kimin tetiklediğini, hangi akışta olduğunu ve kime bildirim gittiğini yazar, her rolün durum başına işlemleri §1.11'de | konvansiyon — API5-02 · konvansiyon — API8-01 | konvansiyon |
| `03 §1.1` m1 | Sipariş — sevkiyat ekseni (`OrderStatus`), altı durum (`02 §5.4`) | konvansiyon — API5-02 · UC-33 · UC-92 | eşlendi |
| `03 §1.1` m2 | Sipariş — ödeme ekseni (`PaymentStatus`), beş durum (`02 §5.5`) | konvansiyon — API5-02 · UC-33 · UC-92 | eşlendi |
| `03 §1.1` m3 | Ürün ve varyant (`PublishStatus`) — Taslak · Yayında · Arşiv (`02 §5.1`) | konvansiyon — API5-02 · UC-88 · UC-04 | eşlendi |
| `03 §1.1` m4 | Kurumsal içerik kaydı (`PublishStatus`) — Taslak · Yayında (`02 §5.2`) | konvansiyon — API5-02 · UC-96 | eşlendi |
| `03 §1.1` m5 | Ayıp talebi (`DefectClaimStatus`) — Açık · Çözüldü (`02 §5.10`) | konvansiyon — API5-02 · UC-33 · UC-92 · UC-93 | eşlendi |
| `03 §1.1` m6 | İletişim talebi, KVKK talebi dahil (`ContactRequestStatus`) — Açık · Kapatıldı (`02 §5.13`) | konvansiyon — API5-02 · UC-93 · UC-94 | eşlendi |
| `03 §1.1` metin | Durum makinesi taşımayanlar: kalem kayıtları (§1.7); indirim, kupon, stok ayırma, hesap, oturum ve davet zamana ya da kullanıma bağlı geçerlilik (§1.10); duyurunun tarih aralığı; satış açıklığı dört koşullu kapıdır | konvansiyon — API5-02 · konvansiyon — API8-01 | konvansiyon |
| `03 §1.2` `Placed` | Alındı — müşterinin onayıyla doğar; ödeme ya da fiziksel kalemsiz siparişte teslim işaretleri beklenir; sonrakiler S1, S2, S3 | UC-27 · UC-33 · UC-92 · konvansiyon — API5-02 | eşlendi |
| `03 §1.2` `Preparing` | Hazırlanıyor — ödenmiş fiziksel siparişin hazırlığı; Z-10 açık fiziksel kalem varken işler; sonrakiler S5, S4, S10 | UC-33 · UC-92 · konvansiyon — API5-02 | eşlendi |
| `03 §1.2` `Shipped` | Kargoya verildi — fiziksel kalemin iptal yolu kapanır, cayma düğmesi açılır; sonrakiler S6, S7 | UC-33 · UC-92 · konvansiyon — API5-02 · konvansiyon — API8-01 | eşlendi |
| `03 §1.2` `DeliveryFailed` | Teslim edilemedi — gönderi firmaya döndü; sonrakiler S8, S9, S11 | UC-33 · UC-92 · konvansiyon — API5-02 | eşlendi |
| `03 §1.2` `Delivered` | Teslim edildi — terminal; yalnız yöneticinin düzeltmesi (§1.6) | UC-33 · UC-92 · UC-150 · konvansiyon — API5-02 | eşlendi |
| `03 §1.2` `Cancelled` | İptal edildi — terminal; yalnız yöneticinin düzeltmesi (§1.6) | UC-33 · UC-92 · UC-150 · konvansiyon — API5-02 | eşlendi |
| `03 §1.2` metin | Geçişlerin koşulları `02 §5.4`'ün beyaz listesinde, listede olmayan geçiş yasak; geri yönlü geçiş müşteri akışında yok, yöneticinin düzeltmesi §1.6'da | konvansiyon — API8-01 · UC-150 | eşlendi |
| `03 §1.2` S1 | Alındı → Hazırlanıyor — sistem, ödeme onayıyla (Ö1): kartta sağlayıcının başarı bildirimi, havalede yöneticinin "ödendi" işareti; B-4 | UC-80 · UC-123 · uç doğurmaz — e-posta (`08`) | eşlendi |
| `03 §1.2` S2 | Alındı → Teslim edildi — kapanış kuralını tamamlayan son olay: dijital siparişte ödeme onayı, son hizmet kaleminin "tamamlandı"sı, son açık kalemin iptali, çıkarılması ya da cayma beyanı | UC-80 · UC-123 · UC-139 · UC-40 · UC-43 · UC-129 · UC-138 · UC-130 | eşlendi |
| `03 §1.2` S3 | Alındı → İptal edildi — müşteri iptali, yöneticinin sebepli iptali, sistem (ödeme süresi, başarısız kart, aynı sepetten yeni sipariş), kapanış (K-672); B-7 | UC-39 · UC-40 · UC-129 · UC-27 · UC-80 · uç doğurmaz — sistem işi (`05 §5`) · uç doğurmaz — e-posta (`08`) | eşlendi |
| `03 §1.2` S4 | Hazırlanıyor → İptal edildi — müşteri ya da yönetici son açık kalemi iptal eder; kapanış — gecikme feshi, cayma ya da çıkarma (K-672) | UC-40 · UC-129 · UC-41 · UC-43 · UC-138 · UC-130 | eşlendi |
| `03 §1.2` S5 | Hazırlanıyor → Kargoya verildi — yönetici, geri alınamaz onayla; yalnız açık fiziksel kalemler; B-5 | UC-124 · konvansiyon — API9-03 | eşlendi |
| `03 §1.2` S6 | Kargoya verildi → Teslim edildi — yönetici teslim işaretini ve tarihini girer; kendiliğinden geçiş yok | UC-126 | eşlendi |
| `03 §1.2` S7 | Kargoya verildi → Teslim edilemedi — yönetici geri dönen gönderiyi işaretler; B-6 | UC-127 | eşlendi |
| `03 §1.2` S8 | Teslim edilemedi → Kargoya verildi — yönetici yeniden gönderir, geri alınamaz onayla (K-677); B-5 | UC-125 | eşlendi |
| `03 §1.2` S9 | Teslim edilemedi → İptal edildi — yöneticinin sebepli iptali; açık kalem kalmamışsa sebepsiz kapanış (K-717) | UC-129 · UC-128 | eşlendi |
| `03 §1.2` S10 | Hazırlanıyor → Teslim edildi — kapanış kuralının son olayı: son açık fiziksel kalemin iptali, çıkarılması, feshi; son hizmet kaleminin iptali, çıkarılması, "tamamlandı"sı ya da cayması | UC-40 · UC-129 · UC-138 · UC-41 · UC-139 · UC-43 · UC-130 | eşlendi |
| `03 §1.2` S11 | Teslim edilemedi → Teslim edildi — kapanış kuralının son olayı, S7'nin kendisi dahil | UC-127 · UC-129 · UC-43 · UC-41 · UC-130 | eşlendi |
| `03 §1.3` `Pending` | Bekliyor — ödeme gelmedi; sonrakiler Ö1, Ö2 | UC-33 · UC-92 · konvansiyon — API5-02 | eşlendi |
| `03 §1.3` `Paid` | Ödendi — ödeme onaylandı; sonrakiler Ö3, Ö4 | UC-33 · UC-92 · konvansiyon — API5-02 | eşlendi |
| `03 §1.3` `Failed` | Başarısız — terminal; sebep iptal kaydında | UC-33 · UC-92 · konvansiyon — API5-02 | eşlendi |
| `03 §1.3` `PartiallyRefunded` | Kısmen geri ödendi; sonraki Ö5 | UC-33 · UC-92 · konvansiyon — API5-02 | eşlendi |
| `03 §1.3` `Refunded` | Geri ödendi — terminal | UC-33 · UC-92 · konvansiyon — API5-02 | eşlendi |
| `03 §1.3` Ö1 | Bekliyor → Ödendi — sistem: sağlayıcının başarı bildirimi ya da süre dolduğunda son sorgunun olumlu cevabı; yönetici: havalede "ödendi", geri alınamaz onayla; B-4 | UC-80 · UC-123 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §1.3` Ö2 | Bekliyor → Başarısız — S3 ile aynı anda, ödenmemiş siparişin her iptali | UC-39 · UC-129 · UC-80 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §1.3` Ö3 | Ödendi → Kısmen geri ödendi — sistem: kart kaleminin iadesi sağlayıcıda gerçekleşir; yönetici: havale hattında, caymada, ayıpta ve tutar bazlı kısmi geri ödemede işler; B-8, F-4 | UC-131 · UC-133 · uç doğurmaz — sistem işi (`05 §5`) · uç doğurmaz — e-posta (`08`) | eşlendi |
| `03 §1.3` Ö4 | Ödendi → Geri ödendi — Ö3'ün tetikleyenleri, paranın tamamı | UC-131 · UC-133 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §1.3` Ö5 | Kısmen geri ödendi → Geri ödendi — Ö3'ün tetikleyenleri, kalanın tamamı | UC-131 · UC-133 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §1.3` metin | Ekseni değiştirmeyen olaylar: başarısız ya da sonucu bilinmeyen kart iadesi panel işareti düşürür (K-940); iptal edilmiş siparişe gelen ödemenin kendiliğinden iadesi (K-684); ters ibraz ürünün dışında; yasak geçiş Başarısız → Ödendi; tek düzeltme havale işareti | UC-80 · UC-92 · UC-150 · uç doğurmaz — sistem işi (`05 §5`) · uç doğurmaz — ürünün dışında | eşlendi |
| `03 §1.4.1` | Birleşim tablosu — her siparişin her an iki durumu var; ✓ izinli, ✗ ulaşılamaz ya da yasak | konvansiyon — API5-02 · konvansiyon — API8-01 | konvansiyon |
| `03 §1.4.1` m1 | Alındı satırı — Bekliyor (yeni sipariş), Ödendi ve Kısmen geri ödendi yalnız fiziksel açık kalemsiz siparişte, Geri ödendi tutar bazlı geri ödemeyle | UC-33 · UC-92 · konvansiyon — API8-01 | eşlendi |
| `03 §1.4.1` m2 | Hazırlanıyor satırı — Bekliyor ulaşılamaz (kapı koşulu), Başarısız yasak | UC-33 · UC-92 · konvansiyon — API8-01 | eşlendi |
| `03 §1.4.1` m3 | Kargoya verildi satırı — Bekliyor ve Başarısız ulaşılamaz | UC-33 · UC-92 · konvansiyon — API8-01 | eşlendi |
| `03 §1.4.1` m4 | Teslim edilemedi satırı — Bekliyor ve Başarısız ulaşılamaz | UC-33 · UC-92 · konvansiyon — API8-01 | eşlendi |
| `03 §1.4.1` m5 | Teslim edildi satırı — Bekliyor ve Başarısız adıyla yasak (`02 §5.6.1`) | UC-33 · UC-92 · konvansiyon — API8-01 | eşlendi |
| `03 §1.4.1` m6 | İptal edildi satırı — Bekliyor ulaşılamaz; Ödendi ve Kısmen geri ödendi geri ödemesi beklenen iptal | UC-33 · UC-92 · konvansiyon — API8-01 | eşlendi |
| `03 §1.4.1` metin | Harflerin açıklaması (a)–(h): kapı koşulu, havale işaretinin düzeltilmesi, ödenmeden iptal, son sorgu yanıtsızken Alındı + Bekliyor, yanlış firma iptalinin düzeltilmesi yalnız geri ödeme gerçekleşmemişken ve sonuç beklenmiyorken (K-940) | UC-150 · UC-92 · konvansiyon — API8-01 | eşlendi |
| `03 §1.4.2` | Eşzamanlı geçişler — iki ekseni ya da ekseni ve kalemi aynı anda değiştiren olaylar (`02 §5.6`) | uç doğurmaz — sistem işi (`05 §5`) | uç doğurmaz |
| `03 §1.4.2.1` | Ödeme onayı — S1 ya da S2 ve dijital kalemlerin teslim işareti aynı anda | UC-80 · UC-123 | eşlendi |
| `03 §1.4.2.2` | Ödenmemiş siparişin iptali — S3 + Ö2 | UC-39 · UC-129 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §1.4.2.3` | Son açık kalemin kapanması ya da teslim işareti — kapanış kuralı | UC-40 · UC-41 · UC-43 · UC-129 · UC-138 · UC-139 | eşlendi |
| `03 §1.4.2.4` | Havale "ödendi" işaretinin düzeltilmesi — Ödendi → Bekliyor ve Hazırlanıyor → Alındı birlikte | UC-150 | eşlendi |
| `03 §1.4.3` | Kapanış kuralı — açık kalem adetle okunur; fiziksel açık kalemi olmayan siparişte son açık kalemin kapanışı ya da teslim işareti siparişi bitirir | uç doğurmaz — sistem işi (`05 §5`) · konvansiyon — API9-03 | konvansiyon |
| `03 §1.4.3` m1 | En az bir kalem teslim edilmişse Teslim edildi — S2, S10, S11 | uç doğurmaz — sistem işi (`05 §5`) | uç doğurmaz |
| `03 §1.4.3` m2 | Hiçbir kalem teslim edilmemişse İptal edildi — iptal S3 ya da S4 ve B-7; cayma, fesih ya da çıkarma kapanıştır; Kargoya verildi ve Teslim edilemedi'de yöneticinin S7'si ve S9'un sebepsiz kapanışı (1.11.40; K-717) | UC-40 · UC-129 · UC-127 · UC-128 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §1.4.3` metin | Kart son sorgusu yanıtsızken ve kesintide dolan havale süresinde iptal ertelenir — sipariş Alındı + Bekliyor'da kalır (K-667) | uç doğurmaz — sistem işi (`05 §5`) | uç doğurmaz |
| `03 §1.4.4` | Geçişlerin stok, hizmet kontenjanı ve kupon hakkına etkisi (`02 §4.3`) | uç doğurmaz — sistem işi (`05 §5`) | uç doğurmaz |
| `03 §1.4.4.1` | Sipariş oluşur — stok ve kupon hakkı ayrılır | UC-27 | eşlendi |
| `03 §1.4.4.2` | Ödeme onayı — ayrılan kesin düşer, kupon kullanılmış sayılır | UC-80 · UC-123 | eşlendi |
| `03 §1.4.4.3` | Ödenmeden iptal — serbest kalır, kupon geri döner | UC-39 · UC-129 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §1.4.4.4` | Ödenmiş kalemin iptali ve çıkarma — adet stoğa ve havuza kendiliğinden döner; kupon yalnız siparişin tamamında (K-791) | UC-40 · UC-129 · UC-138 | eşlendi |
| `03 §1.4.4.5` | Gecikme feshi — kargoya verilmemiş kalemde kendiliğinden, kargodan dönen mal teslim almayla; kupon koşullu (K-1106) | UC-41 · UC-130 · UC-140 | eşlendi |
| `03 §1.4.4.6` | Cayma ve iade — mal teslim alınan adet kadar eklemeyle döner, reddedilen dönmez; hizmet kontenjanı cayılan adet kadar | UC-43 · UC-130 · UC-140 · UC-141 | eşlendi |
| `03 §1.4.4.7` | Yanlış iptalin düzeltilmesi — yeniden ayrılır, yetmezse düzeltme yapılamaz (K-1091) | UC-150 | eşlendi |
| `03 §1.4.4.8` | Havale işaretinin düzeltilmesi — ayırma ve kupon tutulu kalır | UC-150 | eşlendi |
| `03 §1.4.4.9` | Varyantın kalıcı silinmesi — ayırması düşer (K-655) | UC-116 | eşlendi |
| `03 §1.5` metin | Tipe göre hatlar `02 §5.7`'de; tablo olağan diziyi, adımı kimin attığını ve zorunlu elle adım sayısını gösterir | uç doğurmaz — yazım konvansiyonu | uç doğurmaz |
| `03 §1.5.1` | Fiziksel — kart: sistem Ö1 ve S1, yönetici S5 ve S6; iki elle adım | UC-80 · UC-124 · UC-126 | eşlendi |
| `03 §1.5.2` | Fiziksel — havale: Ö1'i yöneticinin "ödendi" işareti tetikler; üç elle adım | UC-123 · UC-124 · UC-126 | eşlendi |
| `03 §1.5.3` | Yalnız dijital — Ö1 ve S2; kartta sistem, havalede yönetici | UC-80 · UC-123 | eşlendi |
| `03 §1.5.4` | Yalnız hizmet — Ö1, ardından yöneticinin son "tamamlandı"sı ile S2 | UC-80 · UC-123 · UC-139 | eşlendi |
| `03 §1.5.5` | Dijital + hizmet — dijital kalemler ödemede teslim, son "tamamlandı" ile S2 | UC-80 · UC-123 · UC-139 | eşlendi |
| `03 §1.5.6` | Karışık, fiziksel kalemli — fiziksel hat; dijital ödemede, hizmet "tamamlandı"da kalem düzeyinde teslim | UC-124 · UC-126 · UC-139 | eşlendi |
| `03 §1.5.7` | Fiziksel kalemleri kapanmış karışık — bulunduğu durumda kapanış kuralıyla biter | uç doğurmaz — sistem işi (`05 §5`) | uç doğurmaz |
| `03 §1.6.1` | Geri alınamaz onay isteyen işlemler — yönetici sonucu tek cümleyle görür; "geri al" düğmesi yok | konvansiyon — API2-02 · konvansiyon — API9-03 | konvansiyon |
| `03 §1.6.1.1` | Havale "ödendi" işareti — dijital kalemli siparişte düzeltilemeyeceği önceden söylenir | UC-123 | eşlendi |
| `03 §1.6.1.2` | Kargoya verme ve yeniden gönderim (K-677) | UC-124 · UC-125 | eşlendi |
| `03 §1.6.1.3` | Firma iptali — sipariş ya da kalem; "stokta bulunamadı"da yasal uyarı | UC-129 | eşlendi |
| `03 §1.6.1.4` | Yöneticinin geri ödemeyi işlemesi — tutar bazlı kısmi dahil | UC-131 · UC-133 | eşlendi |
| `03 §1.6.1.5` | Kart hattında karta para gönderen kalem çıkarma | UC-138 | eşlendi |
| `03 §1.6.1.6` | İade reddi — onay sonucu tek cümleyle söyler (K-656) | UC-140 | eşlendi |
| `03 §1.6.1.7` | Başka kanaldan gelen cayma ya da gecikme feshi bildiriminin kaydı — geri alınmaz, düzeltilmez (K-688, K-715) | UC-130 | eşlendi |
| `03 §1.6.1.8` | Üye hesabının talep üzerine silinmesi — geri alınmaz | UC-157 | eşlendi |
| `03 §1.6.1` metin | Sistemin kart iadesi onay penceresi değildir (F-4); siparişe dokunmayan onaylar kendi akışlarında; müşterinin geri alınamaz dört işlemi — iptal, gecikme feshi, cayma, hesap silme — son adımda onayla tamamlanır, onay ekranın son adımıdır (K-732) | UC-39 · UC-40 · UC-41 · UC-43 · UC-68 · konvansiyon — API2-02 | eşlendi |
| `03 §1.6.2` | Düzeltme — kapalı listeden sebeple yeni bir geçiş; ikisi işlem izinde, dış dünyaya çıkan sonuç geri alınmaz, her düzeltme B-13 ile bildirilir | UC-150 · konvansiyon — API9-03 | eşlendi |
| `03 §1.6.2.1` | Erken ya da yanlış teslim işareti — bir önceki duruma; cayma penceresi ve ayıp süresi işlemez | UC-150 | eşlendi |
| `03 §1.6.2.2` | Yanlış iptal — iptalden önceki duruma; ödeme Ödendi'de kalmışsa, sonuç beklenirken yapılamaz (K-940); ayırmalar yeniden (K-1091); başlatılmış geri ödeme, IBAN isteği ve IBAN kapanır (K-1056, K-1131) | UC-150 | eşlendi |
| `03 §1.6.2.3` | Yanlış Kargoya verildi ya da Teslim edilemedi işareti — önceki duruma; aralıkta doğan cayma ve ayıp kalır, gerekirse kapanış kuralı | UC-150 | eşlendi |
| `03 §1.6.2.4` | Para gelmeden konmuş havale "ödendi" işareti — iki eksen birlikte geri; kayıt varsa yapılamaz (K-703); Z-8 yeniden başlar, B-3 yeniden gider | UC-150 | eşlendi |
| `03 §1.6.2` metin | Kart hattında ödeme ekseninin düzeltmesi yok; cayma beyanı düzeltilmez ve geri alınmaz (K-688); teslim tarihi, ulaşma tarihi, adres ve takip bilgisinin düzeltilmesi müdahaledir | UC-148 · UC-149 · UC-146 · UC-147 · konvansiyon — API8-01 | eşlendi |
| `03 §1.7.1` | Kalemin kayıtları — kalem durum makinesi taşımaz; kayıtların kapanış kuralına etkisi | konvansiyon — API5-02 · UC-33 · UC-92 | eşlendi |
| `03 §1.7.1.1` | Teslim işareti ve tarihi — fiziksel S6, dijital ödeme onayı, hizmet "tamamlandı" | UC-126 · UC-80 · UC-123 · UC-139 | eşlendi |
| `03 §1.7.1.2` | İptal kaydı — müşteri ya da sebepli yönetici, iptal edilen adetle (K-794); B-7 | UC-40 · UC-129 · konvansiyon — API4-04 · konvansiyon — API9-03 | eşlendi |
| `03 §1.7.1.3` | Çıkarma kaydı — yönetici; B-11 | UC-138 | eşlendi |
| `03 §1.7.1.4` | Gecikme feshi — müşteri ya da başka kanaldan yönetici (K-706); teslim edilmemiş fiziksel kalemlerin açık adetlerinin tamamı; B-9, F-3 | UC-41 · UC-130 | eşlendi |
| `03 §1.7.1.5` | Cayma beyanı — müşteri ya da yönetici; cayılan adetle ve iade adresiyle; ardışık beyanlar ayrı kayıt (K-790); B-9, F-3 | UC-43 · UC-130 · konvansiyon — API4-04 | eşlendi |
| `03 §1.7.1.6` | İade teslim alma — ulaşma tarihi ve adetle, her teslim alma ayrı kayıt (K-794); tarih müdahaleyle düzeltilir (K-712); stoğa ekleme ayrı adım; IBAN silinmişse B-14 | UC-140 · UC-141 · UC-149 | eşlendi |
| `03 §1.7.1.7` | İade reddi — koşullu istisna kaleminde, teslim alma adımında; B-16 | UC-140 | eşlendi |
| `03 §1.7.1.8` | "Mal dönmedi" kapanışı — Z-42 geçtikten sonra, ulaşmamış adetlere | UC-142 | eşlendi |
| `03 §1.7.1.9` | IBAN isteği — sistemin yöneticinin adımlarıyla açtığı ve yöneticinin kendisinin açtığı (K-686, K-690, K-706); B-14 | UC-134 · UC-140 · UC-132 · UC-130 · UC-136 · UC-129 · UC-138 | eşlendi |
| `03 §1.7.1.10` | Ayıp talebi — müşteri, ayıplı adetle (K-793); B-9, F-3 | UC-45 · konvansiyon — API4-04 | eşlendi |
| `03 §1.7.1.11` | İndirme sayacı — sistem her indirmede, yönetici yeniler | UC-37 · UC-143 | eşlendi |
| `03 §1.7.2` | Kalemin yolu tipe göre | konvansiyon — API8-01 | konvansiyon |
| `03 §1.7.2` m1 | Fiziksel kalem — iptal ve çıkarma kargoya kadar, Z-11 dolunca fesih, kargoda fesih ya da cayma, teslim, pencere içinde cayma, teslim alma · ret · "mal dönmedi"; ayıp Kargoya verildi'den, Z-18 | UC-40 · UC-129 · UC-138 · UC-41 · UC-43 · UC-140 · UC-142 · UC-45 · konvansiyon — API8-01 | eşlendi |
| `03 §1.7.2` m2 | Dijital kalem — ödemede teslim ve indirme; iptal ve cayma yok, ayıp açık | UC-36 · UC-37 · UC-45 · konvansiyon — API8-01 | eşlendi |
| `03 §1.7.2` m3 | Hizmet kalemi — iptal, çıkarma ya da cayma "tamamlandı"ya kadar, cayma ayrıca Z-14'e kadar; ayıp tamamlamadan sonra | UC-40 · UC-138 · UC-43 · UC-139 · UC-45 · konvansiyon — API8-01 | eşlendi |
| `03 §1.7.3` | Panel işaretleri — durum değildir; koşul sürdükçe görünür (K-675) | UC-91 · UC-92 · konvansiyon — API8-01 | eşlendi |
| `03 §1.7.3.1` | "e-posta ulaşmadı" — siparişin, talebin ya da davetin satırında; üç başarısız denemede düşer; siparişte yeniden gönderimin başarısıyla, davette davet geçersizleşince kalkar | UC-92 · UC-103 · UC-153 · UC-154 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §1.7.3.2` | "ödeme sonucu alınamadı" — kart son sorgusu yanıtsız kalınca; sonuç gelince olağan geçiş | UC-91 · UC-92 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §1.7.3.3` | "geri ödeme gerçekleşmedi" — kart iadesi başarısız olunca; gerçekleşince, IBAN'a işlenince ya da yanlış iptal düzeltilince kalkar | UC-92 · UC-135 · UC-136 · UC-137 · UC-131 · UC-150 | eşlendi |
| `03 §1.7.3.4` | "IBAN bekleniyor" — kalemde ya da kalemsiz geri ödemede siparişte; müşteri IBAN'ı girince kalkar (K-1075, K-1108) | UC-92 · UC-134 · UC-35 | eşlendi |
| `03 §1.7.3.5` | "iade malı bekleniyor" — teslimden sonraki caymayla; teslim alma, ret ya da "mal dönmedi" ile beyanın bütün adetleri için | UC-92 · UC-140 · UC-142 | eşlendi |
| `03 §1.7.3.6` | "e-posta düzeltildi" — misafir siparişinin e-postası düzeltilince; kalkmaz | UC-92 · UC-151 | eşlendi |
| `03 §1.7.3.7` | "geri ödeme sonucu bekleniyor" — sağlayıcıya ulaşılamayınca; istek yeniden gönderilmez, sonuç sorulur; yönetici "gerçekleşmedi" işaretleyince sorgu durur (K-940) | UC-92 · UC-137 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §1.7.3` metin | Bekleyen işler sayaçları `02 §10.6.1`'de, akışı §8.9 | UC-86 · UC-91 | eşlendi |
| `03 §1.7.4` | Kalemin adedi — işlemin birimi adettir; seçilebilecek adet işleme açık adettir, aşan değer kabul edilmez; kayıtlar adedi taşır; gecikme feshinde seçim yok (K-796); dijital adedi bir, hizmette "tamamlandı" açık adetlerin tamamına | konvansiyon — API4-04 · konvansiyon — API9-03 · UC-40 · UC-43 · UC-45 · UC-129 · UC-138 · UC-140 · UC-142 | eşlendi |
| `03 §1.8.1` | Ürün ve varyant üç değeri ayrı taşır; bütün geçişler yöneticinin panel işlemi, bildirim yok, işlem izine yazılır | UC-115 · konvansiyon — API9-04 | eşlendi |
| `03 §1.8.1` `Draft` | Taslak — vitrinde yok; adresi ziyaretçiye "sayfa bulunamadı", yöneticiye "Taslak" bandıyla; Yayında'ya geçiş yayın kapısına bağlı | UC-04 · UC-06 · UC-115 · konvansiyon — API5-02 | eşlendi |
| `03 §1.8.1` `Published` | Yayında — vitrinde, listelerde ve aramada; tükenmişlik durum değil; kapıyı bozan geçiş engellenir | UC-04 · UC-02 · UC-03 · UC-115 · konvansiyon — API5-02 | eşlendi |
| `03 §1.8.1` `Archived` | Arşiv — vitrinden çıkmış, adresi "Bu ürün artık satılmıyor" sayfası; Yayında'ya geçiş kapıya bağlı | UC-04 · UC-115 · konvansiyon — API5-02 | eşlendi |
| `03 §1.8.2` | Yayın kapısını bozan işlem engellenir, sebebi söylenir: son Yayında varyantın arşive ya da taslağa alınması, varyantı dosyasız bırakan dosya silme, son Yayında varyantın kalıcı silinmesi (K-674); ürünün kalıcı silinmesinde açık siparişlerin sayısı onaydan önce | UC-115 · UC-116 · UC-114 · konvansiyon — API8-01 · konvansiyon — API2-07 | eşlendi |
| `03 §1.8.3` | Vitrinden kalkan kalem sepetten çıkar ve müşteriye bir kez söylenir; yayın durumu siparişi etkilemez; geçici kapatma yayın durumunu değiştirmez | UC-19 · UC-22 | eşlendi |
| `03 §1.8.4` | Kurumsal içerik kaydı iki değer taşır; arşiv, ileri tarihli yayın ve sürüm geçmişi yok | UC-159 · konvansiyon — API5-02 | eşlendi |
| `03 §1.8.4` `Draft` | Taslak — sitede yok, yönetici "Taslak" bandıyla önizler; zorunlu alanlar doluysa Yayında'ya | UC-10 · UC-11 · UC-12 · UC-09 · UC-159 · konvansiyon — API5-02 | eşlendi |
| `03 §1.8.4` `Published` | Yayında — düzenleme kaydedildiği anda yayında; kapıyı bozan düzenleme kaydedilmez | UC-158 · UC-159 · konvansiyon — API5-02 · konvansiyon — API2-07 | eşlendi |
| `03 §1.8.4` metin | Duyuru tarihleri girilmişse yalnız aralığında görünür (Z-23); Hakkımızda silinmez, yalnız taslağa alınır; öteki kayıtların silinmesi geri alınamaz uyarısıyla | UC-160 · UC-159 · konvansiyon — API8-01 | eşlendi |
| `03 §1.9.1` | Ayıp talebi — kalem başına tek kayıt (K-676); talep yokken "sorun bildir", Açık'ken yeni talep yok, Çözüldü'yken müşteri ve yönetici yeniden açar | UC-45 · UC-46 · UC-145 · konvansiyon — API8-01 | eşlendi |
| `03 §1.9.1` `Open` | Açık — talep bu durumda doğar; yönetici "çözüldü" işaretler; B-9, F-3 | UC-45 · UC-144 · konvansiyon — API5-02 | eşlendi |
| `03 §1.9.1` `Resolved` | Çözüldü — müşteri ya da yönetici Z-18 içinde yeniden açar; müşterininkinde B-9 ve F-3 (K-681) | UC-46 · UC-145 · konvansiyon — API5-02 | eşlendi |
| `03 §1.9.1` metin | Seçimlik hakların yürütümü sistem dışı; para gerektiren çözüm §1.3'ün geri ödeme geçişleriyle | uç doğurmaz — ürünün dışında · UC-131 · UC-133 | eşlendi |
| `03 §1.9.2` | İletişim talebi, KVKK dahil — ayrı durum makinesi ve süre sayacı yok | UC-93 · UC-94 · konvansiyon — API5-02 | eşlendi |
| `03 §1.9.2` `Open` | Açık — formdan gelen talep bu durumda doğar; yönetici kapatır; F-2 | UC-15 · UC-155 · konvansiyon — API5-02 | eşlendi |
| `03 §1.9.2` `Closed` | Kapatıldı — yönetici yeniden açar | UC-156 · konvansiyon — API5-02 | eşlendi |
| `03 §1.9.2` metin | Saklama süresi son kapatılıştan, hiç kapatılmamışta açılıştan işler (Z-33) | uç doğurmaz — sistem işi (`05 §5`) | uç doğurmaz |
| `03 §1.10` metin | Durum makinesi olmayan varlıklar — geçerlilik zamana ya da kullanıma bağlı | uç doğurmaz — yazım konvansiyonu | uç doğurmaz |
| `03 §1.10.1` | İndirim — tarih aralığında (Z-20); sistem başlatır ve bitirir; onay anında başlaması ya da bitmesi özet farkıdır | UC-114 · UC-27 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §1.10.2` | Kupon — tarih aralığında ve hak kaldıkça; kullanım adedi kullanılmış ve ayrılmış hakların altına indirilemez (K-654) | UC-122 · UC-23 | eşlendi |
| `03 §1.10.3` | Kupon hakkı ve stok ayırma — §1.4.4'ün anları | UC-27 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §1.10.4` | Duyurunun tarih aralığı — Yayında'yken aralığında görünür (Z-23) | UC-159 · konvansiyon — API5-01 | eşlendi |
| `03 §1.10.5` | Hesap — doğrulanmamış kayıt bekleme aşaması; Z-1 dolunca silinir, yeniden istenen bağlantı ömrü uzatmaz (K-659); e-posta değişikliği doğrulanana kadar bekler, Z-45 boyunca geri alınır; Google ile ilk girişte bekleyen kayıt silinir (K-696); K-1080; silme geri alma açıkken yok (K-698); kurulumun yeni yönetici açması (K-699) | UC-53 · UC-49 · UC-66 · UC-76 · UC-51 · UC-68 · UC-157 · UC-177 · UC-186 · UC-187 · uç doğurmaz — sistem işi (`05 §5`) · uç doğurmaz — kurulum | eşlendi |
| `03 §1.10.6` | Oturum — "beni hatırla"ya göre Z-4 ya da Z-5; sıfırlama, silme, geri alma ve kaldırma bütün oturumları, şifre değiştirme ötekileri kapatır (K-695) | UC-55 · UC-110 · UC-59 · UC-112 · UC-68 · UC-76 · UC-187 · UC-177 · UC-65 · UC-181 | eşlendi |
| `03 §1.10.7` | Yönetici daveti — Z-3 boyunca, tek kullanımlık; kullanılınca hesap doğar; geri çekme, yeni davet, gönderenin kaldırılması ve kabulde adres çakışmasıyla geçersizleşir (K-1085); F-6 (K-692) | UC-175 · UC-176 · UC-113 · UC-85 · UC-177 · uç doğurmaz — e-posta (`08`) | eşlendi |
| `03 §1.10.8` | Satış açıklığı — dört koşul; düşmesi ya da geçici kapatma yeni siparişi durdurur, açık siparişi etkilemez | UC-165 · UC-166 · UC-05 · UC-27 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11` metin | Durum × rol × işlem — ekran varyantları bu tablodan türer; aralığın dışında sipariş sayfası ve panel işlemi sunmaz; müşteride iptal, cayma ve fesih yalnız açık kalemde, adet seçimi işleme açık adetle, fiziksel kalemde cayma ve ayıp yalnız kargoya verilmişte | konvansiyon — API8-01 · konvansiyon — API4-04 · UC-33 · UC-92 | eşlendi |
| `03 §1.11.1` | Siparişi bütünüyle iptal — Alındı + Bekliyor; S3 + Ö2, B-7 | UC-39 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.2` | Fiziksel kalemi iptal — ödemeden sonra Kargoya verildi'ye kadar; geri ödeme Z-17 | UC-40 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.3` | Hizmet kalemini iptal — ödemeden sonra "tamamlandı"ya kadar, sevkiyattan bağımsız | UC-40 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.4` | Dijital kalemi iptal — yoktur; önce yalnız siparişin bütünüyle iptali | uç doğurmaz — yoktur · UC-39 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.5` | Fiziksel kalemden cayma — Kargoya verildi'den sonra, teslimden Z-13; tarih girilmemişse açık; mutlak istisnada yok, koşullu istisnada koşulla | UC-43 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.6` | Hizmet kaleminden cayma — ödemeden sonra (K-673), "tamamlandı"ya ya da Z-14'e kadar | UC-43 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.7` | Dijital kalemden cayma — yoktur; hak üçüncü onay kutusuyla düşer | uç doğurmaz — yoktur · konvansiyon — API8-01 | konvansiyon |
| `03 §1.11.8` | Gecikme feshi — Z-11 geçmiş, teslim tarihi girilmemiş, açık adedi kalan fiziksel kalem varken; Hazırlanıyor'da S4, öteki iki durumda yöneticinin S7 ve S9'u (K-717) | UC-41 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.9` | Ayıp talebi açma — kalem tipine göre başlangıç, talep yokken (K-676), Z-18; tarih girilmemişse süre işlemez | UC-44 · UC-45 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.10` | Ayıp talebini yeniden açma — Çözüldü'yken, Z-18 içinde; B-9, F-3 | UC-46 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.11` | Geri ödeme IBAN'ını girme — havale hattında işlem sırasında, IBAN isteğinde, havale yolu açıldığında | UC-35 · UC-40 · UC-41 · UC-43 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.12` | Girilmiş IBAN'ı düzeltme — yönetici geri ödemeyi işleyene kadar | UC-35 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.13` | Havale bilgisini görme — Alındı + Bekliyor havale siparişinde, donmuş IBAN (K-663) | UC-33 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.14` | Dijital dosyayı indirme — ödemeden sonra, indirme hakkı (P-8) kaldıkça | UC-36 · UC-37 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.15` | Havale "ödendi" işareti — Alındı + Bekliyor, iptalden sonra yok; geri alınamaz | UC-123 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.16` | Kargoya verme — Hazırlanıyor, açık fiziksel kalem varken | UC-124 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.17` | Teslim işareti ve tarihi — Kargoya verildi | UC-126 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.18` | "Teslim edilemedi" — Kargoya verildi | UC-127 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.19` | Yeniden gönderme — Teslim edilemedi, açık fiziksel kalem varken (K-677) | UC-125 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.20` | Hizmet kalemini "tamamlandı" — ödeme onaylanmışken (K-671), kalem açıkken | UC-139 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.21` | Firma iptali — ödeme beklenirken yalnız sipariş, ödemeden sonra kalem ve adet düzeyinde (K-794); Teslim edilemedi'de geri dönen kalem; cayma beyanlı ve feshedilmiş kaleme uygulanmaz; havalede IBAN isteği (K-690) | UC-129 · konvansiyon — API8-01 · konvansiyon — API9-03 | eşlendi |
| `03 §1.11.22` | Kalem çıkarma ya da adet azaltma — ödemeden sonra, tipe göre sınırla; kart hattında geri alınamaz; havalede IBAN isteği | UC-138 · konvansiyon — API8-01 · konvansiyon — API9-03 | eşlendi |
| `03 §1.11.23` | Adres düzeltme — Kargoya verildi'ye kadar; fiziksel kalemsizde fatura adresi Teslim edildi'ye kadar; B-10 | UC-146 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.24` | Kargo şirketi ve takip numarası düzeltme — kargoya verildikten sonra; B-12 | UC-147 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.25` | Teslim tarihini ya da ulaşma tarihini düzeltme (K-712) — pencereler ve Z-16 yeni tarihten | UC-148 · UC-149 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.26` | Durum geçişini düzeltme — §1.6.2'nin satırları; B-13 | UC-150 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.27` | İç not — her durumda, kişisel veri imha edilene kadar (K-1095) | UC-152 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.28` | İade malını teslim alma — ulaşan adetle, kalanlar ulaşınca yeniden; "mal dönmedi" kalemini yeniden açar; ayıpta sözleşmeden dönmede de (K-808) | UC-140 · konvansiyon — API8-01 · konvansiyon — API9-03 | eşlendi |
| `03 §1.11.29` | İade malını reddetme — koşullu istisna kaleminde, teslim alma adımında; geri alınamaz; B-16 | UC-140 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.30` | "Mal dönmedi" kapanışı — Z-42 geçmiş, ulaşmamış adet varken | UC-142 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.31` | Geri ödemeyi işleme — havalede IBAN varken, caymada iki hatta, ayıpta, tutar bazlıda kalan tutar kadar, geç gelen ödemenin havale yolunda (K-686, K-1075) | UC-131 · UC-133 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.32` | Kart iadesini yeniden deneme ya da havale yolunu açma; sonucu beklenen iadeyi "gerçekleşmedi" işaretleme (K-940) | UC-135 · UC-136 · UC-137 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.33` | "Havale gerçekleşmedi" — geri ödeme işlenmeden önce; IBAN silinir, IBAN isteği (K-1131) | UC-132 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.34` | Başka kanaldan gelen caymanın kaydı — tipe göre önkoşul (K-673); pencere dışında ve mutlak istisnada ayrıca onay (K-709); pencereden bir yıl sonrasına tarihli kayıt reddedilir | UC-130 · konvansiyon — API8-01 · konvansiyon — API2-07 | eşlendi |
| `03 §1.11.35` | Misafir siparişinin e-postasını düzeltme — imhaya kadar; B-1 yeni, B-15 eski adrese | UC-151 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.36` | E-postayı yeniden gönderme — "e-posta ulaşmadı" işaretliyken, müşteri bildirimi için; davette yok (K-759) | UC-153 · UC-154 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.37` | Ayıp talebini "çözüldü" işaretleme ya da yeniden açma | UC-144 · UC-145 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.38` | İndirme hakkını yenileme — dijital kalemde; sayaç sıfırlanır (K-683) | UC-143 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.39` | Geri ödeme için IBAN isteme — havale hattında IBAN'sız geri ödeme işlenecekken (K-686) | UC-134 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.40` | Siparişi kapatma (S9 kapanışı) — Teslim edilemedi'de açık kalem kalmamış, teslim yokken; sebep, iptal kaydı ve geri ödeme yok (K-717) | UC-128 · konvansiyon — API8-01 | eşlendi |
| `03 §1.11.41` | Başka kanaldan gelen gecikme feshinin kaydı — 1.11.8'in aralığında; geri alınamaz (K-715); IBAN'sızsa B-14 | UC-130 · konvansiyon — API8-01 | eşlendi |
| `03 §2` metin | Müşteri tarafının akışları — satın alma, takip, iptal, cayma ve iade, kurumsal içeriğin vitrin yüzü (K-650); üyelik §9'da, firma tarafı §8'de, uçtan uca akış §2.10'da (K-651) | uç doğurmaz — yazım konvansiyonu | uç doğurmaz |
| `03 §2` m1 | Biçim — adım tablosu §0.4.1; durum §1'e uyar, işlemin açık olduğu aralık §1.11'de | uç doğurmaz — yazım konvansiyonu | uç doğurmaz |
| `03 §2` m2 | Aktör — §2.1 ve §2.2 girişsiz, Ziyaretçi; §2.3–§2.9 Müşteri, üyenin farkı satır içinde; yöneticinin adımları tek satırla | konvansiyon — API0-07 · konvansiyon — API2-03 | konvansiyon |
| `03 §2` m3 | Dallar — olağan yol ve dallar; hatalar §3'te satır numarasına bağlı, zaman aşımları §4'te, bildirimler §7'de | uç doğurmaz — yazım konvansiyonu | uç doğurmaz |
| `03 §2.1` metin | Akış 1'in başı; satış kapısı kapalıyken vitrin ve ürünler görünür, yalnız sepete ekleme ve ödeme kapanır | konvansiyon — API8-01 · UC-05 · UC-22 | eşlendi |
| `03 §2.1.1` | Ana sayfa — firmanın düzeni, kurumsal tanıtım ve en yeni ürünler; duyuru şeridi her sayfanın üstünde, aralığında (Z-23) | UC-01 · konvansiyon — API5-01 | eşlendi |
| `03 §2.1.2` | Kategori sayfası — menü sabit iskeletten, alfabetik (K-805); ürünü olmayan kategori menüde yok; alt dallarla tek liste, en yeni önce, sıralama seçeneği yok; kırıntı yolu; boş kategori sayfası; indirim kartı | UC-02 · konvansiyon — API5-01 · konvansiyon — API2-06 | eşlendi |
| `03 §2.1.3` | Ürün adıyla arama — büyük-küçük harfe ve Türkçe karaktere duyarsız; açıklama, kategori ve seçenek aranmaz | UC-03 · konvansiyon — API2-06 | eşlendi |
| `03 §2.1.4` | Listeyi süzme — fiyat aralığı ve stok durumu; varsayılan tükenmişleri de gösterir; seçenek değeriyle süzme yok | UC-02 · UC-03 · konvansiyon — API2-06 | eşlendi |
| `03 §2.1.5` | Ürün sayfası — görseller, açıklama, fiyat ve tarihi, üretim yeri, birim fiyat, indirim, kargoya verme ve ifa süresi; stok adedi gösterilmez; "Paylaş" | UC-04 · uç doğurmaz — istemci | eşlendi |
| `03 §2.1.6` | Varyant seçimi — en fazla iki boyut; tükenmiş ve açılmamış kombinasyon seçilemez, taslak görünmez; satın alınabilirlik ayrılmış adetler düşülerek | UC-04 · konvansiyon — API8-02 | eşlendi |
| `03 §2.1.7` | Bütün varyantları tükenmiş ürün — "Tükendi", sepete eklenemez; "Stokta haber ver" ve istek listesi yok | UC-04 · uç doğurmaz — yoktur | eşlendi |
| `03 §2.1.8` | Varyantı adediyle sepete ekleme — sepet akışına | UC-05 | eşlendi |
| `03 §2.1.9` | Satış kapalıyken gezinme — ürünler ve içerik görünür; sepete ekleme ve ödeme kapalı, satışın kapalı olduğu görünür; sepet korunur; bakım modu yok | UC-05 · UC-19 · UC-22 · konvansiyon — API8-01 | eşlendi |
| `03 §2.1.10` | Arşivlenmiş ürünün adresi — "Bu ürün artık satılmıyor": ad, görsel, durum; fiyat ve sepete ekleme yok; listelerde ve aramada görünmez | UC-04 | eşlendi |
| `03 §2.1.11` | Taslak ya da kalıcı silinmiş ürünün adresi — "Sayfa bulunamadı"; yöneticiye taslak "Taslak" bandıyla | UC-04 · UC-06 · konvansiyon — API2-03 | eşlendi |
| `03 §2.2` metin | Akış 7'nin vitrin yüzü ve iletişim talebi; kurumsal taraf satış kapısına bağlı değil | UC-14 · UC-15 | eşlendi |
| `03 §2.2.1` | Ana sayfanın kurumsal blokları — Hakkımızda ya da boşken marka adı ve logo; "Ana sayfada göster" işaretli hizmet ve referanslar elle sırasıyla | UC-01 | eşlendi |
| `03 §2.2.2` | Menüden içerik tipine giriş — yayında kaydı olmayan tip menüde yok; genel sayfalar altbilgide ve işaretliyse üst menüde; liste sayfaları elle sırayla | UC-07 · UC-08 · UC-09 · UC-13 · UC-14 · konvansiyon — API5-01 | eşlendi |
| `03 §2.2.3` | İçerik sayfası — Hakkımızda, hizmet tanıtımı, referans iş, genel sayfa kendi sayfasında, SSS tek sayfada; hizmet fiyatsız, "Bize ulaşın"; video dış bağlantı | UC-09 · UC-10 · UC-11 · UC-12 · UC-13 · uç doğurmaz — dış sayfa | eşlendi |
| `03 §2.2.4` | "İlgili ürünler"den ürüne geçiş — yalnız yayındakiler; ters yön yok | UC-10 · UC-11 · UC-04 | eşlendi |
| `03 §2.2.5` | İletişim sayfası — firma kimliği, yasal kimlik ve KEP, şubeler ve form; "Haritada aç" bağlantısı | UC-14 · uç doğurmaz — dış sayfa | eşlendi |
| `03 §2.2.6` | Her sayfanın üst ve alt bölümü — sosyal ve WhatsApp bağlantıları, yasal metinlerin ve "İşlem rehberi"nin bağlantıları, genel sayfalar, platform imzası, ETBİS bandı; çerez bandı yok | konvansiyon — API5-01 · UC-16 · UC-17 · UC-18 · UC-12 · uç doğurmaz — dış sayfa | eşlendi |
| `03 §2.2.7` | İletişim formunun açılışı — girişsiz; aydınlatma bağlantısı; yasal metinler yayında değilse form kapalı (K-850); üyeye ad ve e-posta ön dolu | UC-14 · konvansiyon — API8-01 · konvansiyon — API5-01 | eşlendi |
| `03 §2.2.8` | Formun gönderimi — konu tipi beş değerden; dosya eki yok; tuzak alan doluysa sessizce düşer; L-4; iletişim talebi Açık; F-2, alındı e-postası yok (K-678) | UC-15 · konvansiyon — API2-08 · uç doğurmaz — e-posta (`08`) | eşlendi |
| `03 §2.2.9` | Cevabı bekleme — talep yalnız panelde; numara ya da durum sorgusu yok; cevap sistem dışında e-postayla, firma kapatır | uç doğurmaz — ürünün dışında · UC-155 | eşlendi |
| `03 §2.2.10` | Taslak ya da silinmiş içeriğin adresi — "Sayfa bulunamadı"; taslak menüde ve listelerde yok | UC-06 · UC-10 · UC-11 · UC-12 | eşlendi |
| `03 §2.3` metin | Sepet kendiliğinden boşalmaz (Z-6); sepet hatırlatması ve istek listesi yok | UC-19 · uç doğurmaz — yoktur | eşlendi |
| `03 §2.3.1` | Sepete ekleme — satış kapalıysa eklenmez; stok ayrılmış adetler düşülerek, adet söylenmez; P-9 bütün varyantların toplamına, sınır stoktan önce söylenir; dijitalde adet 1, ikinci kez eklenmez; üyeye daha önce aldığı dijital uyarısı; stok ayırmaz, ilk fiyatı hatırlar | UC-05 · konvansiyon — API2-07 | eşlendi |
| `03 §2.3.2` | Sepeti açma — üyenin sepeti hesapta, misafirinki tarayıcıda; güncel fiyat ve satın alınabilirlik, fiyat değişimi notu; kargo eşiği, asgari tutar, il kısıtı; taksit yok | UC-19 | eşlendi |
| `03 §2.3.3` | Adedi değiştirme ya da kalemi çıkarma — artırma 2.3.1'in denetiminden | UC-20 · UC-21 | eşlendi |
| `03 §2.3.4` | Satın alınamaz kalem — sepette kalır, siparişe girmez, mesajı yazar; adet kendiliğinden düşmez; tutara ve eşiklere girmez | UC-19 · UC-22 | eşlendi |
| `03 §2.3.5` | Vitrinden kalkan kalem — sepetten çıkar, bir kez söylenir | UC-19 | eşlendi |
| `03 §2.3.6` | Misafirin sepetle girişi — iki sepet birleşir, büyük adet kalır; görmediği ekleme bir kez söylenir; kurallardan geçer; L-6 sayacı büyüğü taşır | UC-55 · UC-51 · UC-19 | eşlendi |
| `03 §2.3.7` | Üyenin çıkışı — sepet hesapla gider, tarayıcıda boş görünür | UC-77 · UC-19 | eşlendi |
| `03 §2.3.8` | Ödeme adımına geçiş — siparişe girebilecek kalem şart, satış açık; fiziksel kalemde asgari sipariş tutarı | UC-22 · UC-19 · konvansiyon — API8-01 | eşlendi |
| `03 §2.4` metin | Ödeme adımı misafire ve üyeye aynı; giriş duvarı yok; sipariş onay anında doğar | UC-22 · UC-27 · konvansiyon — API2-03 | eşlendi |
| `03 §2.4.1` | Ödeme adımının açılışı — üyede hesabın e-postası; misafir e-postasını yazar, doğrulanmaz; kayıtlı e-postanın hatırlatması; giriş yaparsa sepetler birleşir | UC-22 · UC-25 · UC-26 · UC-55 | eşlendi |
| `03 §2.4.2` | Teslimat adresi — üye defterden seçer ya da yazar, deftere kaydeder (K-680); alanlar; il kısıtı; fiziksel kalem yoksa alanlar yok; teslimat seçeneği yok | UC-22 · UC-25 · UC-27 · konvansiyon — API5-01 | eşlendi |
| `03 §2.4.3` | Fatura adresi — varsayılan teslimatla aynı; fiziksel kalemsizde de istenir; telefon ve kurumsal alan yok | UC-22 · UC-27 · konvansiyon — API5-01 | eşlendi |
| `03 §2.4.4` | Kupon kodu — tek kod, değiştirilir ya da kaldırılır (K-679); harf ayrımsız (K-806); tarih ve hak (K-654); pay kalemlere dağılır; L-6 | UC-23 · UC-24 · konvansiyon — API2-08 | eşlendi |
| `03 §2.4.5` | Ödeme yöntemi — kart sağlayıcı tanımlıysa, erişilemiyorsa seçilemez; havale firma açtıysa, L-8 doluysa seçilemez, mesaj nötr; kapıda ödeme yok | UC-22 · UC-25 · konvansiyon — API8-01 · konvansiyon — API2-08 | eşlendi |
| `03 §2.4.6` | Onay özeti — kalemler, döküm, toplam; misafirde gideceği e-posta yazar ve düzeltmeye açık; Ön Bilgilendirme Formu, cayma özeti, aydınlatma bağlantısı | UC-22 · UC-25 · UC-16 | eşlendi |
| `03 §2.4.7` | Onay kutuları — iki kutu; dijitalde ve hizmette birer kutu daha; işaretlenmeden onay yok | UC-27 · konvansiyon — API5-01 | eşlendi |
| `03 §2.4.8` | "Siparişi onayla" — L-7; sepet onay anında yeniden değerlendirilir; her fark ve metin sürümü artışı siparişi durdurur, güncel özet ve yeniden onay; kalem kalmadıysa sepete | UC-27 · UC-22 · UC-19 · konvansiyon — API2-05 · konvansiyon — API2-08 · konvansiyon — API2-07 | eşlendi |
| `03 §2.4.9` | Siparişin oluşması — tahmin edilemez numara; kalemler, adresler, sürümler, IBAN donar; ayırmalar, Z-7 ve Z-8; aynı sepetin ödenmemiş siparişi iptal; doğrulanmış hesabın e-postasıysa hesaba düşer; ekran numarayı gösterir (K-704); B-1, B-2, F-1 | UC-27 · UC-28 · UC-29 · uç doğurmaz — e-posta (`08`) | eşlendi |
| `03 §2.5` metin | Ödeme onayı iki ekseni ve kalemleri birlikte değiştirir; yöneticinin teslim adımları §8.2'de | UC-80 · UC-123 | eşlendi |
| `03 §2.5.1.1` | Kart bilgisi sağlayıcıda girilir; ürün kart verisini görmez; her ödeme 3D Secure; taksit sağlayıcıda | UC-29 · uç doğurmaz — dış sayfa | eşlendi |
| `03 §2.5.1.2` | 3D Secure ve siteye dönüş — sipariş sayfasına iner; ödeme sağlayıcının başarı bildirimiyle onaylanır; bildirim gelmemişse bekleme söylenir; doğrulama `08`'in (K-705) | UC-30 · UC-33 · UC-80 | eşlendi |
| `03 §2.5.1.3` | Doğrulama başarısız ya da kart reddi — sipariş kendiliğinden iptal, ayırmalar serbest; müşterinin listesinde yok, panelde var, L-7'ye sayılır; sipariş sayfası söyler (K-704); sepet durur | UC-80 · UC-30 · UC-33 · UC-27 | eşlendi |
| `03 §2.5.1.4` | Banka ekranı kapanır ya da yarıda kalır — Z-7 dolunca son sorgu; yanıtsızsa "ödeme sonucu alınamadı"; aynı sepetten yeni sipariş öncekini iptal eder | uç doğurmaz — sistem işi (`05 §5`) · UC-27 · UC-92 | eşlendi |
| `03 §2.5.2.1` | Havaleyle onay — donmuş IBAN ve numara ekranda, B-2'de ve sipariş sayfasında; Z-8 onaydan | UC-28 · UC-33 | eşlendi |
| `03 §2.5.2.2` | Bankadan havale — sistem dışı; tutar karşılaştırılmaz | uç doğurmaz — ürünün dışında | uç doğurmaz |
| `03 §2.5.2.3` | Hatırlatma — son iş gününde bir kez (Z-9), düzeltmeyle yeniden başlayan sürede bir kez daha; B-3 | uç doğurmaz — sistem işi (`05 §5`) · uç doğurmaz — e-posta (`08`) | uç doğurmaz |
| `03 §2.5.2.4` | Yöneticinin "ödendi" işareti — panel donmuş IBAN'ı gösterir, geri alınamaz | UC-123 · UC-92 | eşlendi |
| `03 §2.5.2.5` | Süre dolduğunda işaretsiz ödeme — kendiliğinden iptal; sonradan "ödendi" yapılamaz; kesintide ertelenir (K-667) | uç doğurmaz — sistem işi (`05 §5`) · konvansiyon — API8-01 | konvansiyon |
| `03 §2.5.2.6` | Ödeme beklenirken vazgeçme ya da kartla ödeme isteği — siparişi bütünüyle iptal; kart için aynı sepetten yeni sipariş | UC-39 · UC-27 | eşlendi |
| `03 §2.5.3.1` | Ödeme onayının işlenmesi — stok kesin düşer, kupon kullanılmış; siparişe giren kalemler sepetten çıkar; Z-10 ve Z-39 başlar; dijital kalemler teslim; B-4 | UC-80 · UC-123 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §2.5.3.2` | Dijital dosyanın indirilmesi — hak kaldıkça (P-8; K-713), süre sınırı yok; güncellenen dosya iner; hak dolunca iletişim formu, yönetici yeniler; "hakkımı yenile" yok | UC-36 · UC-37 · UC-15 · UC-143 | eşlendi |
| `03 §2.5.3.3` | Kargoya verme — şirket ve takip numarası ya da araç beyanı; sipariş sayfası "Takip et" bağlantısını gösterir; iptal yolu kapanır, cayma ve "sorun bildir" açılır | UC-124 · UC-33 · uç doğurmaz — dış sayfa | eşlendi |
| `03 §2.5.3.4` | "Teslim edilemedi" — gönderi döner; yeniden gönderim ya da iptal | UC-127 · UC-125 · UC-129 | eşlendi |
| `03 §2.5.3.5` | Teslim işareti ve tarihi — kargoya verilmeden önceki ve ileri tarih reddedilir; fiziksel kalemlerin tamamına; kendiliğinden geçiş yok; Z-13 ve Z-18 bu tarihten | UC-126 · konvansiyon — API2-07 | eşlendi |
| `03 §2.5.3.6` | Hizmet kalemini "tamamlandı" — cayma düşer, "sorun bildir" açılır; randevu yok; son açık kalemse kapanış kuralı | UC-139 | eşlendi |
| `03 §2.5.3.7` | Fiziksel kalemsiz siparişin izlenmesi — Alındı'da bekler, açık kalemlerin tamamı teslim işaretini alınca Teslim edildi | UC-33 | eşlendi |
| `03 §2.6` metin | Sipariş sayfasına üç yoldan girilir ve sayfa her bilgiyi taşır; hiçbir akış bir e-postanın ulaşmasına bağlı değil (K-187) | UC-33 · UC-34 · UC-74 · UC-32 | eşlendi |
| `03 §2.6.1` | Üyenin sipariş geçmişinden açması — hesaba düşmüş misafir siparişleri dahil; ödemesi alınmamış ve kendiliğinden iptal edilmiş sipariş listede yok | UC-74 · UC-33 · konvansiyon — API2-03 | eşlendi |
| `03 §2.6.2` | Misafirin numara ve e-postayla sorgusu — ikisi eşleşirse açılır; L-5 IP ve e-posta ekseninde ayrı | UC-31 · UC-32 · konvansiyon — API2-08 | eşlendi |
| `03 §2.6.3` | Sipariş e-postasındaki bağlantı — erişim anahtarı sayfayı e-posta sormadan açar; ömrü imhaya kadar; hesap silme ve e-posta değişikliğinden etkilenmez; indirme bağlantısı aynı kapı | UC-34 · UC-37 · konvansiyon — API2-03 | eşlendi |
| `03 §2.6.4` | Sipariş sayfasının içeriği — iki eksen, donmuş kalemler, geri ödemeler, adresler, takip, indirme, havale IBAN'ı ve son gün, metin sürümleri, onay kaydı, o anda açık işlemler; fatura yok; arama motorlarına kapalı | UC-33 · konvansiyon — API8-01 · konvansiyon — API4-09 | eşlendi |
| `03 §2.6.5` | Sipariş e-postası ulaşmaz — her bilgi sipariş sayfasında; Z-26'dan sonra "e-posta ulaşmadı", yönetici yeniden gönderir | UC-33 · UC-153 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §2.6.6` | Sipariş yürürken hesap silme ya da e-posta değişikliği — sipariş sürer; silinmişse misafir yolu ya da bağlantı; e-posta değiştiyse siparişler hesapta | UC-32 · UC-34 · UC-74 | eşlendi |
| `03 §2.6.7` | Misafirin yanlış e-postası — iki yol da o adrese bağlı; firmaya ulaşır, yönetici teyit edip düzeltir; B-1 yeni, B-15 eski adrese | UC-151 · uç doğurmaz — ürünün dışında | eşlendi |
| `03 §2.7` metin | İptal firma onayı beklemez; aralıklar §1.11'de; müşterinin iptali firmaya e-postayla bildirilmez, panelin sayaçlarında görünür | UC-39 · UC-40 · UC-86 · konvansiyon — API8-01 | eşlendi |
| `03 §2.7.1` | Ödeme beklenirken iptal — yalnız sipariş bütünüyle; ayırmalar serbest; listede görünmeye devam eder; B-7 | UC-38 · UC-39 | eşlendi |
| `03 §2.7.2` | Ödenmiş siparişte fiziksel kalemin ya da adedin iptali — Kargoya verildi'ye kadar; adet stoğa döner (K-787, K-791); kupon yalnız tamamında; havalede IBAN bu adımda; Z-10 aşıldıysa panelde kanuni faiz uyarısı | UC-38 · UC-40 · UC-35 · UC-92 · konvansiyon — API4-04 | eşlendi |
| `03 §2.7.3` | Hizmet kaleminin ya da adedin iptali — "tamamlandı"ya kadar; kontenjan döner (K-792); Z-39 aşıldıysa kanuni faiz uyarısı | UC-38 · UC-40 · UC-92 · konvansiyon — API4-04 | eşlendi |
| `03 §2.7.4` | Ödenmiş dijital kalemin iptali — yoktur; sebepli sorun ayıp talebi yolundan | uç doğurmaz — yoktur · UC-45 | eşlendi |
| `03 §2.7.5` | Havale hattında IBAN girişi — iptal, fesih ya da caymada ya da IBAN isteğiyle; biçim ve sağlama basamağı denetimi, aydınlatma bağlantısı; işlenene kadar düzeltilebilir; kartta istenmez (K-710) | UC-35 · UC-40 · UC-41 · UC-43 · konvansiyon — API2-09 | eşlendi |
| `03 §2.7.6` | İptalin geri ödemesi — Z-17; kartta sistem kendiliğinden, havalede yönetici işler; tutar `02 §7.1.7` ile (K-788); kargo ücreti koşulu (K-789); gerçekleşmezse 2.8.4.3 (K-940); B-8, F-4 | UC-131 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §2.7.7` | Gecikme feshi — Z-11 geçmiş, teslim tarihi girilmemiş fiziksel kalem varken; kalem ve adet seçilmez; havalede IBAN; feshedilen kalem kargoya verilmez; panelde kanuni faiz uyarısı; başka kanaldan bildirimi yönetici kaydeder (K-706) | UC-38 · UC-41 · UC-35 · UC-130 · UC-92 · konvansiyon — API4-04 | eşlendi |
| `03 §2.7.8` | Feshin geri ödemesi ve kargodaki malın dönüşü — on dört gün, kartta sistem, havalede yönetici; dönen mal teslim alma ve stoğa ekleme; açık kalem kalmadıysa S7 ve S9 kapanışı | UC-131 · UC-140 · UC-141 · UC-127 · UC-128 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §2.8` metin | Sebepsiz ikinci yol; cayma beyanı ekseni değiştirmez, ödeme ekseni para gönderildiğinde değişir | UC-43 · konvansiyon — API8-01 | eşlendi |
| `03 §2.8.1.1` | Fiziksel kalemin cayma düğmesi — Kargoya verildi'den Z-13'e, tarih girilmemişse açık; kargodan önce yol iptal; mutlak istisnada yok, koşullu istisnada koşul metniyle | UC-33 · UC-42 · konvansiyon — API8-01 | eşlendi |
| `03 §2.8.1.2` | Cayma beyanı — kalem ve adet, ardışık beyan ayrı kayıt; ekran iade adresini ve Z-42 yükümlülüğünü gösterir (K-838); adres kayda yazılır; havalede IBAN; tarih damgası; "iade malı bekleniyor" ya da kargodaysa adetleri kapatır; B-9, F-3 | UC-42 · UC-43 · UC-35 · konvansiyon — API4-04 | eşlendi |
| `03 §2.8.1.3` | Teslim beyanla aynı güne işaretlenir — panel teslimin beyandan önce mi sonra mı olduğunu sorar | UC-126 · UC-148 | eşlendi |
| `03 §2.8.1.4` | Müşteri malı gönderir — Z-42 içinde, karşı ödemeli; ürün iade etiketi üretmez, takip istemez | uç doğurmaz — ürünün dışında | uç doğurmaz |
| `03 §2.8.1.5` | İade malının teslim alınması — ulaşma tarihi ve adet; Z-16 ulaşma tarihinden; stok kontrolden sonra eklenir | UC-140 · UC-141 | eşlendi |
| `03 §2.8.1.6` | Koşullu istisna kaleminin ambalajı açılmış dönmesi — iade reddi; geri ödeme yok, IBAN silinir, mal geri gönderilir; geri alınmaz; B-16 | UC-140 | eşlendi |
| `03 §2.8.1.7` | Mal kullanılmış, hasarlı ya da eksik döner — ret ve kesinti yok; değer kaybı talebi ürünün dışında (K-668) | UC-140 · UC-131 · uç doğurmaz — ürünün dışında | eşlendi |
| `03 §2.8.1.8` | Mal hiç gönderilmez — geri ödeme süresi başlamaz; Z-42'den sonra isteğe bağlı "mal dönmedi"; IBAN kapatmayla ya da Z-38 ile silinir; sonradan ulaşırsa kalem yeniden açılır | UC-92 · UC-142 · UC-140 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §2.8.2.1` | Hizmet kaleminin cayma düğmesi — ödemeden sonra, "tamamlandı"ya ya da Z-14'e kadar; pencerenin başlangıcı (K-673); ödeme beklenirken yol siparişin iptali | UC-33 · UC-42 · konvansiyon — API8-01 | eşlendi |
| `03 §2.8.2.2` | Hizmet kaleminden cayma — iade adresi ve gönderme yükümlülüğü yok; havalede IBAN; kısmi ifa da tam cayma; kontenjan döner; Z-16 beyandan; kapanış kuralı (K-717) | UC-42 · UC-43 · UC-35 · konvansiyon — API4-04 | eşlendi |
| `03 §2.8.3.1` | Dijital kalemden cayma — yoktur; ayıplı dosyada yol ayıp talebi | uç doğurmaz — yoktur · UC-45 | eşlendi |
| `03 §2.8.4.1` | Caymanın geri ödemesi — yönetici işler; kart sağlayıcı üzerinden, havale gerçekleştikten sonra; teslim alma başlatmaz; tutar K-788; kargo ücreti kuralı (K-789, K-1076); kupon yalnız tamamında | UC-131 · konvansiyon — API9-03 | eşlendi |
| `03 §2.8.4.2` | Geri ödeme havalesi gerçekleşmez — "havale gerçekleşmedi": IBAN silinir, sipariş sayfasında alan yeniden açılır; süre durmaz; B-14 | UC-132 · UC-35 | eşlendi |
| `03 §2.8.4.3` | Kart iadesi gerçekleşmez — "geri ödeme gerçekleşmedi" ya da önce "sonuç bekleniyor" (K-940); yeniden deneme ya da havale yolu, sipariş sayfası IBAN alanı açar; geç gelen ödemede sipariş düzeyinde (K-1075); başarısızlıkta bildirim yok (K-683) | UC-135 · UC-136 · UC-137 · UC-35 · UC-33 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §2.8.4.4` | Caymanın e-posta, mektup ya da formla bildirimi — yönetici teyit edip kaydeder; tarih ulaşma tarihi; IBAN yalnız iletişim e-postasından aktarılır, yoksa alan açılır; kargodan önce ve ödeme beklenirken "müşteriyle anlaşıldı" iptali; pencere dışı ve mutlak istisnada ayrıca onay (K-709) | UC-130 · UC-129 · UC-35 · uç doğurmaz — ürünün dışında | eşlendi |
| `03 §2.9` metin | Sebepli üçüncü yol — ayıp talebi; durumları §1.9.1'de, firmanın adımları §8.4'te | UC-45 | eşlendi |
| `03 §2.9.1` | "Sorun bildir"in görünmesi — tipe göre başlangıç, talep yokken; Z-18, tarih girilmemişse işlemez; süre dolunca kanal kapanır, hak iletişim formundan sürer | UC-33 · UC-44 · UC-15 · konvansiyon — API8-01 | eşlendi |
| `03 §2.9.2` | Ayıp talebinin gönderimi — kalem, ayıplı adet (K-793), açıklama; fotoğraf ve dosya yok; fiziksel kalemde iade adresi talebe yazılır; aydınlatma bağlantısı; B-9, F-3 | UC-44 · UC-45 · konvansiyon — API4-04 | eşlendi |
| `03 §2.9.3` | Talep açıkken yeni sorun — yeni talep açılmaz, sipariş sayfası talebin durumunu gösterir | UC-33 · konvansiyon — API8-01 | eşlendi |
| `03 §2.9.4` | Çözüm sistemin dışında — seçimlik haklar ürünün dışında; para gerekiyorsa iade hattı ya da tutar bazlı kısmi geri ödeme; havalede IBAN isteği | uç doğurmaz — ürünün dışında · UC-140 · UC-131 · UC-133 · UC-134 | eşlendi |
| `03 §2.9.5` | Yöneticinin "çözüldü" işareti — Açık → Çözüldü | UC-144 | eşlendi |
| `03 §2.9.6` | Müşterinin talebi yeniden açması — Z-18 içinde; geçmiş tek kayıtta; B-9, F-3 (K-681) | UC-44 · UC-46 | eşlendi |
| `03 §2.10` metin | Uçtan uca anlatılar — if/then'e sığmayan senaryolar ve fiziksel siparişin ana akışı (K-651); kural ve süre tekrarlanmaz | uç doğurmaz — yazım konvansiyonu | uç doğurmaz |
| `03 §2.10.1` metin | Fiziksel siparişin ana akışı — `02 §2.2`'nin sekiz adımı; kartta iki, havalede üç elle adım; müşteri her adımı sipariş sayfasından izler, sonra cayma ve ayıp | UC-33 · uç doğurmaz — yazım konvansiyonu | eşlendi |
| `03 §2.10.1.1` | Ürünü bulma ve varyant seçimi — 2.1.2–2.1.6 | UC-02 · UC-03 · UC-04 | eşlendi |
| `03 §2.10.1.2` | Sepete ekleme — stok ayrılmaz | UC-05 | eşlendi |
| `03 §2.10.1.3` | Ödeme adımında adresler, kupon, ödeme yöntemi — 2.4.1–2.4.5 | UC-22 · UC-23 · UC-25 | eşlendi |
| `03 §2.10.1.4` | Özet, Ön Bilgilendirme Formu, cayma özeti, iki kutu ve onay — yeniden değerlendirme (K-831) | UC-22 · UC-27 | eşlendi |
| `03 §2.10.1.5` | Siparişin oluşması — numara, donma, ayırma; B-1, B-2, F-1 | UC-27 · UC-28 · uç doğurmaz — e-posta (`08`) | eşlendi |
| `03 §2.10.1.6` | Ödeme — kartta 3D Secure, havalede yöneticinin işareti; Ö1, S1 | UC-29 · UC-30 · UC-80 · UC-123 | eşlendi |
| `03 §2.10.1.7` | Hazırlama ve kargoya verme — S5, B-5 | UC-124 | eşlendi |
| `03 §2.10.1.8` | Teslim işareti ve tarihi — S6; pencereler bu tarihten | UC-126 | eşlendi |
| `03 §2.10.2` metin | Karışık sipariş — üye fiziksel, dijital ve hizmet kalemini kartla alır | uç doğurmaz — yazım konvansiyonu | uç doğurmaz |
| `03 §2.10.2` m1 | Onay — dijital ve hizmet için iki kutu daha; Alındı + Bekliyor, B-1 | UC-27 | eşlendi |
| `03 §2.10.2` m2 | Ödeme onayı — sağlayıcının bildirimiyle Ö1 ve S1; kılavuz teslim, B-4; süreler başlar | UC-80 · UC-36 | eşlendi |
| `03 §2.10.2` m3 | Danışmanlığın cayma penceresi — siparişin teslim tarihinden; tarih girilene kadar açık | UC-42 · konvansiyon — API8-01 | eşlendi |
| `03 §2.10.2` m4 | Olağan yol — S5, S6; danışmanlık hattı beklemez, "tamamlandı" ile cayma düşer | UC-124 · UC-126 · UC-139 | eşlendi |
| `03 §2.10.2` m5 | Dal — lamba kargodan önce iptal: B-7, sistemin kart iadesi ve kargo ücreti, Ö3; kupon dönmez; fiziksel kalemsiz hat; danışmanlığın "tamamlandı"sı ile S10 (K-671) | UC-40 · UC-139 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §2.10.3` metin | Kargodayken cayılıp geri dönen gönderi — misafir, tek fiziksel kalem, havale; Kargoya verildi + Ödendi | uç doğurmaz — yazım konvansiyonu | uç doğurmaz |
| `03 §2.10.3` m1 | Beyan — mal yoldayken cayma; IBAN ve iade adresi kayda; B-9, F-3; kalem kapanır, Z-16 beyandan; sipariş Kargoya verildi'de kalır | UC-43 · UC-35 | eşlendi |
| `03 §2.10.3` m2 | Dönüş — S7, B-6; S9'un kapanışı, B-7 gitmez; teslim edilmiş kalem olsaydı S11 | UC-127 · UC-128 | eşlendi |
| `03 §2.10.3` m3 | Teslim alma — ulaşma tarihinin alt sınırı uygulanmaz; stoğa ekleme; firma iptali uygulanmaz | UC-140 · UC-141 | eşlendi |
| `03 §2.10.3` m4 | Geri ödeme — IBAN'a gönderim ve panelden işleme; kargo ücreti eklenir; Ö4, B-8 | UC-131 | eşlendi |
| `03 §2.10.4` metin | IBAN silindikten sonra ulaşan iade malı — üye, havale, teslimden sonra cayma ve IBAN | uç doğurmaz — yazım konvansiyonu | uç doğurmaz |
| `03 §2.10.4` m1 | Beyan — "iade malı bekleniyor"; süre mal ulaşmadan başlamaz | UC-43 · UC-92 | eşlendi |
| `03 §2.10.4` m2 | Mal gelmez — "mal dönmedi" kapanışı IBAN'ı siler, bildirim yok; kapatılmazsa Z-38 ile silinir | UC-142 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §2.10.4` m3 | Mal ulaşır — teslim alma kalemi yeniden açar; IBAN isteği düşer, sipariş sayfasında alan, B-14; yönetici IBAN girmez | UC-140 · UC-35 | eşlendi |
| `03 §2.10.4` m4 | Bekleme — Z-16 ulaşma tarihinden, durmaz; panel "IBAN bekleniyor" ve kalan süre | UC-92 · UC-91 | eşlendi |
| `03 §2.10.4` m5 | Kapanış — IBAN girilir, geri ödeme işlenir (Ö3–Ö5, B-8), IBAN silinir; ödenmiş tutar düşülür; IBAN girilmezse borç sürer, sayaçta sayılmaz | UC-35 · UC-131 · UC-86 | eşlendi |
| `03 §2.10.5` metin | Misafir siparişinin e-posta düzeltmesi — yanlış e-postayla havale siparişi | uç doğurmaz — yazım konvansiyonu | uç doğurmaz |
| `03 §2.10.5` m1 | Yanlış adres — özet gösterir, görülmez; B-1 ve B-2 yanlış adrese; hesabınsa sipariş o hesaba düşer | UC-22 · UC-27 | eşlendi |
| `03 §2.10.5` m2 | Ödeme — havale ve "ödendi" işareti, B-4 yanlış adrese; müşteri sayfayı açamaz | UC-123 · UC-32 · UC-34 | eşlendi |
| `03 §2.10.5` m3 | Teyit — müşteri firmaya ulaşır, yönetici kimliği siparişin bilgileriyle teyit eder | uç doğurmaz — ürünün dışında · UC-92 | eşlendi |
| `03 §2.10.5` m4 | Düzeltme — yeni erişim anahtarı, eski bağlantılar geçersiz; B-1 yeni, B-15 eski adrese; hesap bağı değişir; "e-posta düzeltildi" işareti ve e-posta geçmişi | UC-151 · UC-34 | eşlendi |
| `03 §2.10.5` m5 | Devam — yeni bağlantıyla sipariş sayfası; yeniden düzeltmede aradaki işlemler geri alınmaz; işlem izi yalnız yönetici işlemlerini tutar (K-710) | UC-34 · UC-151 · UC-106 | eşlendi |
| `03 §3` metin | Hata akışları — `02 §6`'nın envanterini akışa bağlar; her satır bir akış adımından ayrılır; `02 §6`'nın her satırı burada tam bir kez Kaynak; süreler §4'te, itiraz §5'te, kötüye kullanım §6'da | konvansiyon — API6-01 · konvansiyon — API2-07 | konvansiyon |
| `03 §3` m1 | Biçim — §0.4.2; "Akış adımı" §2'nin, §8'in ya da §9'un satırı; hedef alt bölümden satır numarasına çevrilme (K-682) | uç doğurmaz — yazım konvansiyonu | uç doğurmaz |
| `03 §3` m2 | Bildirim — "—" ekranda reddedilen ve kayıt doğurmayan işlem; kayıt ya da durum değiştiren olay bildirim kimliğini taşır; "olağan" ana akışın satırını gösterir | konvansiyon — API2-07 · uç doğurmaz — yazım konvansiyonu | konvansiyon |
| `03 §3` m3 | Durum — §1'e uyar; "—" eksenlerin ve kalem kaydının değişmediğini söyler | uç doğurmaz — yazım konvansiyonu | uç doğurmaz |
| `03 §3.1.1` | Hiçbir akış bir e-postanın ulaşmasına bağlı değil — sipariş sayfası her bilgiyi taşır; ulaşmayan bildirim "e-posta ulaşmadı" işareti ve yeniden gönderim; firma bildirimi panelin ana sayfasında uyarı; hesap ve yönetim bağlantıları kuralın dışında — ekrandan yeniden istenir (L-9), davette yeni davet | UC-33 · UC-153 · UC-86 · UC-49 · UC-67 · UC-180 · UC-175 | eşlendi |
| `03 §3.1.2` | Sistem ödenmiş siparişi bilgisizliği yüzünden iptal etmez — son sorgu; yanıtsızsa "ödeme sonucu alınamadı"; kesintide havale iptali ertelenir (K-667) | uç doğurmaz — sistem işi (`05 §5`) · UC-92 | eşlendi |
| `03 §3.1.3` | Onaylanan özet ile oluşan sipariş birebir aynı; her fark oluşmayı durdurur ve söylenir | UC-27 · konvansiyon — API2-07 | eşlendi |
| `03 §3.1.4` | Sepet hatalarda korunur — ödeme yarıda kaldığında, sağlayıcıya erişilemediğinde, satış kapalıyken | UC-19 · konvansiyon — API2-07 | eşlendi |
| `03 §3.1.5` | Limit aşımı nötr konuşur — kalan deneme, süre ve eksen söylenmez; hesap kilitlenmez | konvansiyon — API2-08 | konvansiyon |
| `03 §3.1.6` | Hata ve uyarı yalnız renkle taşınmaz, metin taşır; biçimi `04`'ün işi | uç doğurmaz — görünüm | uç doğurmaz |
| `03 §3.2.1.1` | 3D Secure başarısız ya da kart reddi — kendiliğinden iptal, ayırmalar serbest; sepet durur; L-7'ye sayılır; listede yok, panelde var; B-7 | UC-80 · UC-30 · UC-33 · konvansiyon — API2-08 | eşlendi |
| `03 §3.2.1.2` | Banka ekranı kapanır ya da yarıda kalır — Z-7 sonunda son sorgu; aynı sepetten yeniden onay öncekini hemen iptal eder, ayırma serbest | uç doğurmaz — sistem işi (`05 §5`) · UC-27 | eşlendi |
| `03 §3.2.1.3` | Onay anında fiyat, indirim, kupon, kargo, eşik, il değişmiş, kalem ayrılamaz ya da metin sürümü artmış — sipariş oluşmaz, güncel özet ve yeniden onay | UC-27 · UC-22 · konvansiyon — API2-07 | eşlendi |
| `03 §3.2.1.4` | Kalem tükenmiş ya da kontenjan dolmuş — "Tükendi", sepette kalır; kalem kalmadıysa ödeme adımı açılmaz ya da sepete dönülür | UC-19 · UC-22 · UC-27 | eşlendi |
| `03 §3.2.1.5` | Ürün ya da varyant arşive, taslağa alınmış ya da silinmiş — kalem sepetten çıkar, bir kez söylenir | UC-19 | eşlendi |
| `03 §3.2.1.6` | Stoktan fazla adet — "Bu adette stok yok", adet söylenmez | UC-05 · UC-20 · UC-19 · konvansiyon — API2-07 | eşlendi |
| `03 §3.2.1.7` | P-9 aşılır — eklenmez, sınır sayısıyla söylenir; sınır sonradan düşerse kalemler mesajla kalır | UC-05 · UC-20 · UC-19 · konvansiyon — API2-07 | eşlendi |
| `03 §3.2.1.8` | Aynı dijital varyant ikinci kez — "Bu ürün zaten sepetinde" | UC-05 · konvansiyon — API2-07 | eşlendi |
| `03 §3.2.1.9` | Üye daha önce aldığı dijital varyantı ekler — engellenmez, uyarı gösterilir | UC-05 | eşlendi |
| `03 §3.2.1.10` | Teslimat adresi teslimat yapılmayan ilde — kabul edilmez, sebep söylenir; defterdeki adres seçilemez | UC-25 · UC-27 · konvansiyon — API2-07 | eşlendi |
| `03 §3.2.1.11` | Asgari sipariş tutarının altı — ödeme adımına geçilmez, eksik tutar gösterilir | UC-19 · UC-22 | eşlendi |
| `03 §3.2.1.12` | Kupon toplamı sıfıra indirir, tarih dışı ya da hakkı dolmuş — kabul edilmez | UC-23 · konvansiyon — API2-07 | eşlendi |
| `03 §3.2.1.13` | Misafir e-postasını yanlış yazar — özet gösterir ve düzeltmeye açık; onaydan sonra yönetici teyit edip düzeltir; hesap bağı kesilir; B-1, B-15 | UC-22 · UC-151 | eşlendi |
| `03 §3.2.1.14` | Hesabı olan kişi misafir olarak sipariş verir — kabul, hesaba düşer; giriş hatırlatması | UC-25 · UC-27 | eşlendi |
| `03 §3.2.1.15` | Sağlayıcıya erişilemez — onaydan önce kart seçilemez; onaydan sonra ödeme sayfası açılmazsa kart başarısızlığı, iptal, L-7'ye sayılmaz; tespiti `08`'in | UC-22 · UC-29 · konvansiyon — API2-08 | eşlendi |
| `03 §3.2.1.16` | İptal edilmiş siparişe başarılı ödeme bildirimi — sistem kendiliğinden geri öder; sipariş İptal edildi + Başarısız kalır, ödeme kaydında durur; B-8, F-4 (K-684) | UC-80 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §3.2.1.17` | Satış kapalı — ürünler görünür; sepete ekleme ve ödeme kapalı, satışın kapalı olduğu görünür; sepet korunur; açık siparişler etkilenmez | UC-05 · UC-22 · konvansiyon — API8-01 | eşlendi |
| `03 §3.2.1.18` | Art arda geçersiz kupon — L-6 aşılınca kupon alanı geçici kapanır, nötr | UC-23 · konvansiyon — API2-08 | eşlendi |
| `03 §3.2.1.19` | L-7'nin eşiği — bir süre yeni sipariş onaylanamaz, nötr; misafir e-posta ekseninde, ödeme sayfası açılmamış sipariş sayılmaz | UC-27 · konvansiyon — API2-08 | eşlendi |
| `03 §3.2.1.20` | Sipariş notu istenir — alan yok; iletişim formunun "Sipariş hakkında" tipi | uç doğurmaz — yoktur · UC-15 | eşlendi |
| `03 §3.2.1.21` | L-8 doluyken havale — seçilemez, nötr; kart açık; tavan ödeme ya da iptalle boşalır | UC-22 · UC-27 · konvansiyon — API2-08 | eşlendi |
| `03 §3.2.1.22` | Harcama itirazı ve ters ibraz — ürün bilmez; eksen ve satış özeti değişmez | uç doğurmaz — ürünün dışında | uç doğurmaz |
| `03 §3.2.2.1` | Sipariş e-postası ulaşmaz — sipariş sayfası; yanlış yazıldıysa 3.2.1.13; panelde "e-posta ulaşmadı" | UC-33 · UC-32 · UC-92 | eşlendi |
| `03 §3.2.2.2` | Misafir numara ya da e-postayı yanlış girer — sayfa açılmaz; L-5 iki eksende; engellenen sahip bağlantıdan ya da hesabından | UC-32 · UC-34 · UC-74 · konvansiyon — API2-08 | eşlendi |
| `03 §3.2.2.3` | Hesap silinmiş, sipariş yürüyor — misafir yolu ya da bağlantı | UC-32 · UC-34 | eşlendi |
| `03 §3.2.2.4` | Ödemesi alınmamış ve kendiliğinden iptal edilmiş siparişler — müşterinin listesinde yok, panelde var | UC-74 · UC-91 | eşlendi |
| `03 §3.2.2.5` | E-posta sahibi istemeden düzeltilir — B-15; yönetici yeniden düzeltir, yeni anahtar; aradaki işlemler geri alınmaz (K-710) | UC-151 | eşlendi |
| `03 §3.2.2.6` | Teslimat adresi sahibi istemeden düzeltilir — yönetici teyit eder; B-10; Kargoya verildi'den önce adres geçmişinden geri düzeltme | UC-146 · UC-92 | eşlendi |
| `03 §3.3.1` | Kargoya verildikten sonra fiziksel kalemin iptali — düğme sunulmaz, yol cayma | UC-33 · UC-42 · konvansiyon — API8-01 | eşlendi |
| `03 §3.3.2` | Z-10 ya da Z-11 aşılır — iptal düğmesi açık ve kanuni faiz uyarısı; Z-11'de gecikme feshi düğmesi, kişiye özel üretimde de | UC-40 · UC-41 · UC-92 · konvansiyon — API8-01 | eşlendi |
| `03 §3.3.3` | Dijital kalemin iptali ya da ondan cayma — iki düğme de sunulmaz | uç doğurmaz — yoktur · konvansiyon — API8-01 | konvansiyon |
| `03 §3.3.4` | Cayma istisnalı kalem — mutlakta düğme yok; koşulluda koşul yazılır, beyan kayda geçer; ret teslim almada | UC-42 · UC-43 · UC-140 · konvansiyon — API8-01 | eşlendi |
| `03 §3.3.5` | Tamamlanmış hizmetten cayma — düğme sunulmaz | uç doğurmaz — yoktur · konvansiyon — API8-01 | konvansiyon |
| `03 §3.3.6` | Cayma penceresi geçmiş — düğme kapanır, önceki beyan geçerli | UC-33 · konvansiyon — API8-01 | eşlendi |
| `03 §3.3.7` | Havale ile ödenmiş siparişin geri ödemesi — IBAN üç olayda girilir; dışında istenir (K-686) | UC-35 · UC-134 | eşlendi |
| `03 §3.3.8` | Teslimden sonraki caymada geri ödeme mal ulaşmadan yapılmaz — Z-16 ulaşma tarihinden; teslimden önce ve hizmette beyandan; panel kalan süreyi gösterir | UC-92 · UC-131 | eşlendi |
| `03 §3.3.9` | Gönderi müşteri kaynaklı döner ve iptal — "müşteri kaynaklı" sebebi: ürün bedeli geri ödenir, gidiş kargosu ödenmez; havalede IBAN isteği (K-690) | UC-129 · konvansiyon — API9-03 | eşlendi |
| `03 §3.3.10` | Kısmi iptal ya da iadeden sonra kupon ve kargo eşiği — yeniden hesaplanmaz; kargo ücreti yalnız iki tetikte (K-789) | uç doğurmaz — sistem işi (`05 §5`) · UC-131 | eşlendi |
| `03 §3.3.11` | Kargoya verildikten sonra, mal ulaşmadan vazgeçme — iptal kapalı, cayma açık; beyan kalemi kapatır | UC-43 | eşlendi |
| `03 §3.3.12` | Teslim geç işaretlenir, pencere işaret anında dolmuş — düğme kapalı; pencere yeniden başlamaz | UC-126 · konvansiyon — API8-01 | eşlendi |
| `03 §3.3.13` | Bir kalemden cayma — kalan fiziksel kalem varsa kargo ücreti girmez | UC-131 | eşlendi |
| `03 §3.3.14` | İade kargo ücretini önden ödememe — karşı ödemeli gönderim | uç doğurmaz — ürünün dışında | uç doğurmaz |
| `03 §3.3.15` | Çözülmüş ayıbın tekrarı — müşteri ya da yönetici yeniden açar | UC-46 · UC-145 | eşlendi |
| `03 §3.3.16` | Teslimden sonra cayma, mal gönderilmez — süre başlamaz; panel beyandan geçen günü gösterir; Z-42'den sonra "mal dönmedi"; IBAN silinir | UC-92 · UC-142 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §3.3.17` | Teslim alma işareti gecikir — süre girilen tarihten, geçmişe dönük girilir | UC-140 · UC-149 | eşlendi |
| `03 §3.3.18` | "Mal dönmedi"den sonra mal ulaşır — teslim alma kalemi yeniden açar | UC-140 | eşlendi |
| `03 §3.3.19` | IBAN silindikten sonra mal ulaşır — teslim almayla IBAN isteği, sipariş sayfasında alan; yönetici IBAN girmez; B-14 | UC-140 · UC-35 · UC-92 | eşlendi |
| `03 §3.3.20` | Yeniden istenen IBAN geç girilir — süre durmaz; alan e-postadan bağımsız; isteğin tarihi işlem izinde | UC-33 · UC-35 · UC-106 | eşlendi |
| `03 §3.3.21` | Tamamlanmamış hizmetten cayma, hiç ifa yok — beyan kalemi kapatır; Z-16 beyandan; kapanış kuralı (K-717) | UC-43 | eşlendi |
| `03 §3.3.22` | Kargodayken cayılan gönderi döner — yönetici iptal etmez, teslim alır; geri ödeme caymanın hattından; S7, S9 kapanışı | UC-140 · UC-127 · UC-128 · UC-131 | eşlendi |
| `03 §3.3.23` | Havale bekleyen siparişte kalem ya da adet iptali — sunulmaz; sipariş bütünüyle iptal | UC-39 · konvansiyon — API8-01 | eşlendi |
| `03 §3.3.24` | Kart iadesi başarısız — reddedilirse "geri ödeme gerçekleşmedi", sayaç ve uyarı; yeniden deneme ya da havale yolu; ulaşılamazsa "sonuç bekleniyor", istek yeniden gönderilmez, sorgu; yol kapalı; "gerçekleşmedi" işareti (K-940) | UC-92 · UC-86 · UC-135 · UC-136 · UC-137 · uç doğurmaz — sistem işi (`05 §5`) · konvansiyon — API8-01 | eşlendi |
| `03 §3.3.25` | Başka kanaldan cayma bildirimi — teyit ve kayıt; kalem, adet, ulaşma tarihi — geçmişe dönük olabilir, ileri olamaz; IBAN yalnız iletişim e-postasından; geri alınmaz (K-688); geç ulaşan bildirim onayla (K-709) | UC-130 · konvansiyon — API2-07 | eşlendi |
| `03 §3.3.26` | Yeniden istenen IBAN hiç girilmez — kalem kapanmaz, "IBAN bekleniyor" listesinde, sayaçta değil | UC-91 · UC-92 · UC-86 | eşlendi |
| `03 §3.3.27` | IBAN yanlış ya da banka reddeder — işlenmeden önce düzeltilir; "havale gerçekleşmedi"; işlenmişten sonra dönen havalede eksen düzeltilmez | UC-35 · UC-132 | eşlendi |
| `03 §3.3.28` | Kısmen ifa edilmiş hizmetten cayma — cayılan adetlerin bedelinin tamamı, kesinti yok | UC-43 | eşlendi |
| `03 §3.3.29` | Fiziksel kalemler teslimden önce kapanır — hizmetin penceresi başlamaz, hak "tamamlandı"ya kadar | UC-33 · konvansiyon — API8-01 | eşlendi |
| `03 §3.3.30` | İade gönderisi yolda kaybolur — ürün taşıyıcı belgesini almaz; uyuşmazlık ürünün dışında; ödeme kararıyla B-8 | uç doğurmaz — ürünün dışında · UC-131 | eşlendi |
| `03 §3.3.31` | Kargodan önce başka kanaldan cayma bildirimi — cayma kaydı açılmaz; "müşteriyle anlaşıldı" iptali; Z-16 bildirimin tarihinden (K-690) | UC-129 · konvansiyon — API8-01 | eşlendi |
| `03 §3.3.32` | Pencereden sonra ya da mutlak istisnada bilgilendirilmedim iddiası — düğme açılmaz; değerlendirme firmanın, kayıt bu yoldan (K-709) | UC-130 · konvansiyon — API8-01 | eşlendi |
| `03 §3.3.33` | Sahibi olmayanın başka kanaldan cayma bildirimi — kayıttan önce kanala dönerek teyit; IBAN aktarılmaz | UC-130 · uç doğurmaz — ürünün dışında | eşlendi |
| `03 §3.3.34` | Aynı gün cayma ve teslim — panel teslimin beyandan önce mi sonra mı olduğunu sorar, beyanın saatini gösterir; işlem izinde | UC-126 · UC-92 | eşlendi |
| `03 §3.3.35` | Koşullu kalemin ambalajı açılmış döner — teslim almada iade reddi; geri alınmaz; B-16 | UC-140 | eşlendi |
| `03 §3.3.36` | Mal kullanılmış, hasarlı ya da eksik döner, ret edilemez kalem — ret ve kesinti yok; tam geri ödeme | UC-140 · UC-131 | eşlendi |
| `03 §3.4.1` | Doğrulama bağlantısının Z-1'i dolmuş — kayıt silinir; "bağlantı geçersiz, yeniden kayıt olun"; e-posta yeniden kayda açık | UC-52 · UC-53 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §3.4.2` | Doğrulanmamış hesapla giriş — yapılamaz; doğrulamanın beklendiği ve yeniden istemenin yolu; varlığı ele vermeme `02 §8.1.4` | UC-55 · UC-49 · konvansiyon — API2-07 | eşlendi |
| `03 §3.4.3` | Kayıtta e-posta zaten kayıtlı — "Bu e-posta zaten kayıtlı"; L-3 daraltır | UC-48 · konvansiyon — API2-08 | eşlendi |
| `03 §3.4.4` | Sıfırlamada kayıtlı olmayan e-posta — nötr mesaj | UC-57 · konvansiyon — API2-07 | eşlendi |
| `03 §3.4.5` | Sıfırlama bağlantısı kullanılmış ya da Z-2 dolmuş — geçersiz; yeni talep, L-2 içinde | UC-58 · UC-57 | eşlendi |
| `03 §3.4.6` | Kısa ya da çok yaygın şifre — reddedilir | UC-48 · UC-59 · UC-65 · konvansiyon — API5-01 | eşlendi |
| `03 §3.4.7` | Kimlik sağlayıcısı e-postayı doğrulanmamış verir — bağlama ve yeni hesap yok; kayda yönlendirme (K-697) | UC-51 · UC-47 | eşlendi |
| `03 §3.4.8` | Google ile açılmış şifresiz hesabın "şifremi unuttum"u — sıfırlama "şifre belirle" işlevi görür | UC-57 · UC-59 | eşlendi |
| `03 §3.4.9` | Yeni e-posta doğrulanmaz — değişiklik geçerli olmaz; bekleyen değişiklik ilk bağlantının ömrüyle düşer (K-659) | UC-63 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §3.4.10` | Yürüyen siparişi olan hesabın silinmesi — engellenmez; sipariş misafir yolundan sürer (K-693) | UC-68 · UC-32 · UC-34 | eşlendi |
| `03 §3.4.11` | L-1, L-2, L-3 eşiği — o işlem o eksende geçici engellenir, nötr; tanınan tarayıcı engellenmez | UC-55 · UC-57 · UC-48 · konvansiyon — API2-08 | eşlendi |
| `03 §3.4.12` | Yasal metinler hazır değilken kayıt ya da ilk Google girişi — kayıt ve yeni hesap açacak Google girişi kapalı; mevcut girişler etkilenmez (K-850) | UC-47 · UC-48 · UC-51 · konvansiyon — API8-01 | eşlendi |
| `03 §3.4.13` | E-posta sahibinin haberi olmadan değiştirilir — eski adrese bildirim; Z-45 içinde geri alma bağlantısı | UC-75 · UC-76 · uç doğurmaz — e-posta (`08`) | eşlendi |
| `03 §3.4.14` | Art arda doğrulama bağlantısı — L-9 aşılınca geçici engel, nötr; yeni bağlantı öncekini geçersiz kılar, ömrü uzatmaz | UC-49 · UC-67 · konvansiyon — API2-08 | eşlendi |
| `03 §3.4.15` | Geri alma bağlantısı açıkken hesap silme — yapılmaz; sebep ve bitiş tarihi gösterilir (K-698) | UC-63 · UC-68 · UC-95 · UC-157 · konvansiyon — API8-01 | eşlendi |
| `03 §3.4.16` | Google'ın e-postası doğrulanmamış bekleyen kayda eşleşir — kayıt silinir, hesap Google ile doğar; kapı kapalıysa giriş yok (K-694, K-696) | UC-51 · konvansiyon — API8-01 | eşlendi |
| `03 §3.4.17` | Yeniden doğrulamada art arda yanlış şifre — L-1'e sayılır; eşikte engel, işlem yapılmaz (K-711) | UC-61 · konvansiyon — API2-08 | eşlendi |
| `03 §3.5` metin | Firma tarafının hataları panelin kurallarıdır — çoğu panelin değeri sebebiyle reddettiği ya da işlemi durdurduğu satırlar; müşteri yüzü §2'de | konvansiyon — API2-07 · konvansiyon — API8-01 | konvansiyon |
| `03 §3.5.1.1` | İndirimli fiyat referans fiyatın altına inmez — indirim kaydedilmez, sebep söylenir | UC-114 · konvansiyon — API2-07 | eşlendi |
| `03 §3.5.1.2` | İçi dolu kategori silinmek istenir — engellenir, engelleyenler sayısıyla | UC-121 · konvansiyon — API2-07 | eşlendi |
| `03 §3.5.1.3` | Taşıma üç seviyeyi aşar ya da kendi alt ağacına — yapılmaz | UC-120 · konvansiyon — API2-07 | eşlendi |
| `03 §3.5.1.4` | Yayın kapısının koşulu eksik — yayına alınamaz, eksik koşul gösterilir | UC-115 · konvansiyon — API2-07 | eşlendi |
| `03 §3.5.1.5` | Dijital ürünün yayındaki varyantı dosyasız — yayına alınamaz, varyant gösterilir | UC-115 · konvansiyon — API2-07 | eşlendi |
| `03 §3.5.1.6` | Dosya silme yayındaki varyantı dosyasız bırakır — engellenir | UC-114 · UC-83 · konvansiyon — API8-01 | eşlendi |
| `03 §3.5.1.7` | Fiyata ikiden fazla ondalık — kabul edilmez | UC-114 · konvansiyon — API2-09 · konvansiyon — API5-01 | eşlendi |
| `03 §3.5.1.8` | Sabit kuponun asgari tutarı kupon tutarına eşit ya da altında; yüzdesel kupon %100 — girilemez | UC-122 · konvansiyon — API5-01 | eşlendi |
| `03 §3.5.1.9` | Kargoya verme süresi üst çiti aşar — kabul edilmez | UC-114 · konvansiyon — API5-01 | eşlendi |
| `03 §3.5.1.10` | İki yönetici aynı kaydı aynı anda düzenler — ikinci kaydeden değişikliği görür, uyarılır | UC-114 · konvansiyon — API2-04 | eşlendi |
| `03 §3.5.1.11` | Son Yayında varyantın arşivlenmesi ya da taslağa çekilmesi — engellenir, önce ürünü taşıma yönlendirmesi | UC-115 · konvansiyon — API8-01 | eşlendi |
| `03 §3.5.1.12` | Varyanta 0,00 TL fiyat — kabul edilmez | UC-114 · konvansiyon — API5-01 | eşlendi |
| `03 §3.5.1.13` | İndirim %100 ya da yuvarlanmış fiyat 0,00 TL — kaydedilmez; yürürken böyle fiyat da girilemez | UC-114 · konvansiyon — API5-01 | eşlendi |
| `03 §3.5.1.14` | Stok ya da kontenjan ayrılmış adedin altına — değer sebebiyle reddedilir, ayırmayı tutan siparişler gösterilir; mal yoksa firma iptali (K-653) | UC-114 · UC-129 · konvansiyon — API2-04 · konvansiyon — API2-07 | eşlendi |
| `03 §3.5.1.15` | Kupon kullanım adedi kullanılmış ve ayrılmışın altına — reddedilir; yol bitiş tarihini öne çekmek (K-654) | UC-122 · konvansiyon — API2-07 | eşlendi |
| `03 §3.5.1.16` | Açık siparişi olan ürünün ya da varyantın silinmesi — engellenmez, onaydan önce açık sipariş sayısı; sipariş donmuş kalemiyle sürer; son Yayında varyant silinemez (K-655, K-674) | UC-116 · konvansiyon — API8-01 | eşlendi |
| `03 §3.5.2.1` | Havalede farklı tutar — sistem tutar sormaz; yönetici işaretler ya da sistem dışında çözer | UC-123 · uç doğurmaz — ürünün dışında | eşlendi |
| `03 §3.5.2.2` | Havale iptalden sonra gelir — iptal edilmiş sipariş "ödendi" yapılamaz; para sistem dışında; yeni sipariş varsa ona işaret | UC-123 · konvansiyon — API8-01 · uç doğurmaz — ürünün dışında | eşlendi |
| `03 §3.5.2.3` | Gönderi geri döner — Teslim edilemedi, yeniden gönderim ya da iptal; cayma beyanlı ya da feshedilmiş kalem teslim almayla; "kargoya verilecek" sayacı | UC-127 · UC-125 · UC-129 · UC-140 · UC-91 | eşlendi |
| `03 §3.5.2.4` | Kargoya verirken takip numarası girilmez — şirket ve numara ya da araç beyanı olmadan işaret konamaz | UC-124 · konvansiyon — API5-01 | eşlendi |
| `03 §3.5.2.5` | Yanlış geçiş — "geri al" yok; düzeltme sebepli yeni geçiş; havale işaretinin düzeltilmesinde Z-8 yeniden, B-3 (K-685); B-13 | UC-150 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §3.5.2.6` | Firma karşılayamaz — sebepli firma iptali, sebep e-postayla; Z-17 iptalden; "stokta bulunamadı"da yasal uyarı; havalede IBAN isteği (K-690) | UC-129 · konvansiyon — API9-03 | eşlendi |
| `03 §3.5.2.7` | İndirme hakkı dolar — iletişim formu, yönetici yeniler, sipariş sayfasından indirme (K-683) | UC-15 · UC-143 · UC-37 | eşlendi |
| `03 §3.5.2.8` | Dijital dosya ayıplı — düzeltilip yeniden yüklenir, eski alıcılara da iner (K-683) | UC-83 · UC-114 · UC-37 | eşlendi |
| `03 §3.5.2.9` | İptal edilmiş siparişe kart ödemesi — sistem kendiliğinden geri öder (K-684) | UC-80 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §3.5.2.10` | Teslim hiç işaretlenmez — kendiliğinden geçiş yok; pencere başlamaz, Z-18 işlemez; "teslim işareti bekleyen" sayacı | UC-126 · UC-86 · UC-91 | eşlendi |
| `03 §3.5.2.11` | Teslim tarihi ya da takip bilgisi yanlış — panelden düzeltme, işlem izinde; pencereler yeniden; B-12 | UC-148 · UC-147 | eşlendi |
| `03 §3.5.2.12` | Adres yanlış — müşterinin talebiyle Kargoya verildi'ye kadar; kanaldan teyit; B-10 | UC-146 · konvansiyon — API8-01 | eşlendi |
| `03 §3.5.2.13` | E-posta üç denemede ulaşmaz — "e-posta ulaşmadı"; yeniden gönderim, başarıyla kalkar | UC-92 · UC-153 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §3.5.2.14` | Ödenmemiş siparişte hizmet kalemini "tamamlandı" — sunulmaz (K-671) | UC-139 · konvansiyon — API8-01 | eşlendi |
| `03 §3.5.2.15` | Eşzamanlı işlem — önce tamamlanan uygulanır; sipariş arada değiştiyse sonraki uygulanmaz, güncel hâl; IBAN düzeltmesi de değişiklik; aynı kayıt iki kez işlenmez | konvansiyon — API9-03 · konvansiyon — API2-04 · UC-131 · UC-35 | eşlendi |
| `03 §3.5.2.16` | Gecikme feshi ve kargodan dönen gönderi — feshedilen kalem gönderilmez, iptal edilmez, ikinci geri ödeme yok; S4 kapanışı ya da S7 ve S9 kapanışı | UC-41 · UC-130 · UC-140 · UC-127 · UC-128 | eşlendi |
| `03 §3.5.2.17` | Kesintide dolan havale süresi — iptal ilk iş gününün sonuna ertelenir; yönetici o ana kadar işaretler; sipariş sayfası yeni son günü gösterir (K-666, K-667) | uç doğurmaz — sistem işi (`05 §5`) · UC-123 · UC-33 | eşlendi |
| `03 §3.5.3.1` | Zorunlu alanı eksik kayıt yayına — alınamaz, eksik alan gösterilir | UC-159 · konvansiyon — API2-07 | eşlendi |
| `03 §3.5.3.2` | Hakkımızda'nın silinmesi — silinmez, yalnız taslağa alınır | UC-160 · UC-159 · konvansiyon — API8-01 | eşlendi |
| `03 §3.5.3.3` | Hakkımızda boş ya da yayında değil — ana sayfada marka adı ve logo | UC-01 | eşlendi |
| `03 §3.5.3.4` | İçeriğe bağlı ürün taslak, arşiv ya da silinmiş — bağda kalır ama görünmez, silinen bağdan kalkar | UC-10 · UC-11 · UC-115 · UC-116 | eşlendi |
| `03 §3.5.3.5` | Silinmiş ya da taslak içeriğin adresi — "Sayfa bulunamadı" | UC-06 · UC-10 · UC-11 · UC-12 | eşlendi |
| `03 §3.5.3.6` | İki yönetici aynı içerik kaydını düzenler — ikinci kaydeden uyarılır | UC-158 · konvansiyon — API2-04 | eşlendi |
| `03 §3.5.3.7` | Bot tuzak alanı doldurur — gönderim sessizce düşer, kayıt oluşmaz | UC-15 · konvansiyon — API2-05 | eşlendi |
| `03 §3.5.3.8` | Aynı IP'den çok form (L-4) — geçici engel, nötr | UC-15 · konvansiyon — API2-08 | eşlendi |
| `03 §3.5.3.9` | Yasal metinler hazır değilken form — çalışmaz; kurumsal sayfalar yayında (K-850) | UC-14 · UC-15 · konvansiyon — API8-01 | eşlendi |
| `03 §3.5.3.10` | Müşteri firmanın e-postasına yanıt verir — firma kimliğindeki adrese düşer, sisteme girmez | uç doğurmaz — ürünün dışında | uç doğurmaz |
| `03 §3.5.3.11` | Yayındaki kaydın zorunlu alanı boşaltılır — kaydedilmez, önce taslağa alma yönlendirmesi | UC-158 · konvansiyon — API2-07 | eşlendi |
| `03 §3.5.4.1` | Firma tipinin zorunlu alanları eksik ya da tip değişmiş — satış açılmaz; kurumsal taraf ve açık siparişler etkilenmez (K-664) | UC-164 · konvansiyon — API8-01 | eşlendi |
| `03 §3.5.4.2` | IBAN boşken havale açılmak istenir — açılamaz | UC-167 · konvansiyon — API2-07 | eşlendi |
| `03 §3.5.4.3` | Sağlayıcı anahtarları kurulumda tanımsız — kart görünmez; havale açıksa yalnız havale | UC-100 · UC-22 · uç doğurmaz — kurulum | eşlendi |
| `03 §3.5.4.4` | Hiçbir ödeme yöntemi açık değil — satış açılmaz | UC-167 · konvansiyon — API8-01 | eşlendi |
| `03 §3.5.4.5` | Havale süresi çit dışında ya da varsayılan kargoya verme süresi çiti aşar — kabul edilmez, sebep söylenir | UC-170 · UC-171 · konvansiyon — API5-01 | eşlendi |
| `03 §3.5.4.6` | Marka adı yok — site alan adını gösterir | UC-163 · konvansiyon — API5-01 | eşlendi |
| `03 §3.5.4.7` | Logo yok — marka adı yazıyla | UC-163 · uç doğurmaz — görünüm | eşlendi |
| `03 §3.5.4.8` | Site simgesi yok — baş harf marka rengi zeminde | UC-163 · uç doğurmaz — görünüm | eşlendi |
| `03 §3.5.4.9` | Marka rengi yok ya da okunmaz — varsayılan renk; yazı rengi kontrasta göre | UC-163 · uç doğurmaz — görünüm | eşlendi |
| `03 §3.5.4.10` | Aydınlatma metni, çerez politikası ya da iade adresi eksik — satış açılmaz, panel eksik olanı gösterir; form, kayıt ve ilk Google girişi kapalı (K-850) | UC-173 · UC-174 · UC-172 · UC-86 · konvansiyon — API8-01 | eşlendi |
| `03 §3.5.4.11` | Son yöneticinin ya da kendi hesabının kaldırılması — engellenir, sebep söylenir | UC-177 · konvansiyon — API8-01 | eşlendi |
| `03 §3.5.4.12` | Davet bağlantısı geçersizleşmiş — hesap açılmaz, süresi dolmuş davetin mesajı; yeni davet (K-1085) | UC-85 · UC-113 · UC-175 | eşlendi |
| `03 §3.5.4.13` | SSS ya da sayfa metni ayarla çelişir — tespit edilmez; bağlayıcı olan üretilen form ve sözleşme; panelde hatırlatma satırı | uç doğurmaz — görünüm · UC-99 · UC-101 | eşlendi |
| `03 §3.5.4.14` | Bekleyen havale siparişleri varken IBAN değişir — yeni siparişlere işler, onay bekleyen sayısını söyler; gerekirse firma iptali; F-5 | UC-167 · UC-129 · uç doğurmaz — e-posta (`08`) | eşlendi |
| `03 §3.5.4.15` | Bekleyen havale siparişleri varken havale kapatılır ya da kapı düşer — açık siparişler etkilenmez; panel sayıyı söyler; isteyen firma iptali | UC-167 · UC-129 · konvansiyon — API8-01 | eşlendi |
| `03 §3.5.4.16` | İade malı beklenirken iade adresi değişir — beyanın adresi değişmez, yeni beyanlar günceli alır; panel bekleyen sayıyı söyler (K-665) | UC-172 · UC-33 | eşlendi |
| `03 §4` metin | Zaman aşımı yönetimi — bütün süreler (Z-1…Z-47) ve kendiliğinden işleyen anlar burada akışa bağlanır; değer değil kimlik yazılır; sayım kuralları `02 §4.1`'de | uç doğurmaz — sistem işi (`05 §5`) · konvansiyon — API2-09 | konvansiyon |
| `03 §4` m1 | Uyarı — tek önceden uyarı havale hatırlatması (B-3, Z-9); panelin kalan süresi ve son ödeme günü göstergedir | uç doğurmaz — e-posta (`08`) · UC-33 · UC-92 | eşlendi |
| `03 §4` m2 | Dondurma yok — süreler kesintide de işler; tek istisna havalenin kendiliğinden iptali (K-666, K-667); yasal süreler dondurulmaz | uç doğurmaz — sistem işi (`05 §5`) | uç doğurmaz |
| `03 §4` m3 | Kendiliğinden işlem — "Sona erişte" sistemin işini yazar; "kendiliğinden işlem yok" yükümlülüktür, sistem izler ve gösterir | uç doğurmaz — sistem işi (`05 §5`) · konvansiyon — API8-01 | konvansiyon |
| `03 §4.1.1` | Z-1 doğrulama bağlantısının ömrü — yeniden istenen bağlantı ömrü uzatmaz; dolunca kayıt silinir, bekleyen değişiklik düşer (K-659) | UC-52 · UC-53 · UC-49 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §4.1.2` | Z-2 sıfırlama bağlantısının ömrü — tek kullanımlık; yeni talep L-2 içinde | UC-58 · UC-59 · UC-112 · UC-57 | eşlendi |
| `03 §4.1.3` | Z-3 davet bağlantısının ömrü — dolunca ya da geri çekme, yeni davet, gönderenin kaldırılması, adres çakışmasıyla geçersiz (K-1085) | UC-85 · UC-113 · UC-175 · UC-176 | eşlendi |
| `03 §4.1.4` | Z-4 kısa oturum — son işlemden; dolunca oturum kapanır | konvansiyon — API2-03 · UC-55 | eşlendi |
| `03 §4.1.5` | Z-5 uzun oturum, "beni hatırla" — son işlemden | konvansiyon — API2-03 · UC-55 · UC-110 | eşlendi |
| `03 §4.1.6` | Z-6 sepetin ömrü — dolmaz | UC-19 | eşlendi |
| `03 §4.1.7` | Z-7 kart ödeme süresi — dolunca son sorgu; olumluysa Ö1, değilse iptal, yanıtsızsa "ödeme sonucu alınamadı" ve sorgu yinelenir | uç doğurmaz — sistem işi (`05 §5`) · UC-92 | eşlendi |
| `03 §4.1.8` | Z-8 havale ödeme süresi — onaydan ya da düzeltmeden; dolunca iptal; kesintide ertelenir | uç doğurmaz — sistem işi (`05 §5`) · UC-33 | eşlendi |
| `03 §4.1.9` | Z-9 havale hatırlatması — son iş gününde B-3 bir kez, düzeltmeyle bir kez daha; kesintide kaçırılırsa dönüşte (K-685) | uç doğurmaz — sistem işi (`05 §5`) · uç doğurmaz — e-posta (`08`) | uç doğurmaz |
| `03 §4.1.10` | Z-10 kargoya verme süresi — kendiliğinden işlem yok; "kargoya verilecek" sayacı; iptal düğmesi ve kanuni faiz uyarısı | UC-91 · UC-86 · UC-40 · UC-92 | eşlendi |
| `03 §4.1.11` | Z-11 yasal teslim sınırı — gecikme feshi düğmesi açılır, kargodayken de | UC-41 · konvansiyon — API8-01 | eşlendi |
| `03 §4.1.12` | Z-12 teslim tarihi — süre değil; girilmezse Z-13 ve Z-18 başlamaz | UC-126 | eşlendi |
| `03 §4.1.13` | Z-39 ifa süresi — kendiliğinden işlem yok; "tamamlanmayı bekleyen hizmet" sayacı; iptalde kanuni faiz uyarısı | UC-86 · UC-139 · UC-40 · UC-92 | eşlendi |
| `03 §4.1.14` | Z-13 fiziksel kalemin cayma penceresi — teslim tarihinden; dolunca düğme kapanır | UC-42 · UC-43 · konvansiyon — API8-01 | eşlendi |
| `03 §4.1.15` | Z-14 hizmet kaleminin cayma penceresi — sipariş ya da teslim tarihinden; hak düşer ya da "tamamlandı" ile | UC-42 · UC-43 · konvansiyon — API8-01 | eşlendi |
| `03 §4.1.16` | Z-15 dijital kalemin cayma hakkı — üçüncü onay kutusuyla düşer | UC-27 | eşlendi |
| `03 §4.1.17` | Z-16 caymanın geri ödemesi — beyandan ya da ulaşma tarihinden; panel kalan süreyi gösterir; IBAN beklenirken de işler | UC-92 · UC-131 | eşlendi |
| `03 §4.1.18` | Z-42 iade malını gönderme süresi — dolunca "mal dönmedi" kapanışı açılır, Z-38 başlar | UC-142 · UC-140 · konvansiyon — API8-01 | eşlendi |
| `03 §4.1.19` | Z-17 iptalin geri ödemesi — kartta iptal anında sistemin iadesi, havalede yükümlülük; panel kalan süreyi gösterir | UC-92 · UC-131 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §4.1.20` | Z-47 ifanın imkânsızlaştığının bildirimi — sayaç yok; firma iptali bildirimi sebebiyle (B-7) | UC-129 | eşlendi |
| `03 §4.1.21` | Z-18 ayıp talebinin iki yılı — sipariş sayfasındaki kanal kapanır; hak iletişim formundan | UC-44 · UC-45 · UC-46 · UC-15 · konvansiyon — API8-01 | eşlendi |
| `03 §4.1.22` | Z-19 indirme hakkı — süre yok; P-8 dolunca kapanır; yönetici yeniler | UC-37 · UC-143 | eşlendi |
| `03 §4.1.23` | Z-20 indirimin tarih aralığı — sistem bitirir; onay anında bitmesi özet farkı | UC-114 · UC-27 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §4.1.24` | Z-21 referans fiyatın geriye bakış penceresi — hesap penceresi, işlem başlatmaz | UC-114 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §4.1.25` | Z-22 kuponun tarih aralığı — yeni siparişte kabul edilmez, ayrılmış haklar kalır | UC-23 · UC-122 | eşlendi |
| `03 §4.1.26` | Z-23 duyurunun tarih aralığı — dışında görünmez | UC-159 · konvansiyon — API5-01 | eşlendi |
| `03 §4.1.27` | Z-24 ileri tarihli yayın — yoktur | uç doğurmaz — yoktur | uç doğurmaz |
| `03 §4.1.28` | Z-25 KVKK başvurusuna cevap — sayaç yok; yükümlülük firmanın | UC-94 · uç doğurmaz — ürünün dışında | eşlendi |
| `03 §4.1.29` | Z-26 e-postanın yeniden denenmesi — üç başarısızlıkta siparişin ya da davetin satırına işaret, firma bildiriminde ana sayfa uyarısı; hesap e-postasında işaret yok, bağlantı yeniden istenir | uç doğurmaz — sistem işi (`05 §5`) · UC-92 · UC-103 · UC-86 · UC-153 | eşlendi |
| `03 §4.1.30` | Z-27 deneme limitlerinin penceresi — kayan pencere; engel kendiliğinden açılır | konvansiyon — API2-08 | konvansiyon |
| `03 §4.1.31` | Z-28 sipariş, ödeme ve fatura verisi — yıl sonundan; periyodik imha kişisel veriyi imha eder, hesap bağını keser | uç doğurmaz — sistem işi (`05 §5`) | uç doğurmaz |
| `03 §4.1.32` | Z-41 ödemesi alınmamış siparişin kişisel verisi — imha, kayıt kişisel verisiz kalır (K-951) | uç doğurmaz — sistem işi (`05 §5`) | uç doğurmaz |
| `03 §4.1.33` | Z-29 ayıp talebi kaydı — on yıl dolunca silinir | uç doğurmaz — sistem işi (`05 §5`) | uç doğurmaz |
| `03 §4.1.34` | Z-30 işlem izi — yıl sonundan; süre boyunca değiştirilemez ve silinemez | UC-106 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §4.1.35` | Z-31 hesap verisi — hesap silinince hemen silinir | UC-68 · UC-157 | eşlendi |
| `03 §4.1.36` | Z-32 sepet — hesapla birlikte silinir | UC-68 · UC-157 | eşlendi |
| `03 §4.1.37` | Z-33 iletişim talebi — son kapatılıştan; "Sipariş hakkında" tipinde on yıl (K-707, K-951) | uç doğurmaz — sistem işi (`05 §5`) | uç doğurmaz |
| `03 §4.1.38` | Z-34 hesap, yönetim ve firma bildirimlerinin gönderim kaydı — silinir | uç doğurmaz — sistem işi (`05 §5`) | uç doğurmaz |
| `03 §4.1.39` | Z-43 B-1…B-16'nın gönderim kaydı — ödenmiş siparişte on yıl; ödenmemişte kişisel veri üç yıl, kayıt on yıl (K-951) | uç doğurmaz — sistem işi (`05 §5`) | uç doğurmaz |
| `03 §4.1.40` | Z-35 müşterinin geri ödeme IBAN'ı — son geri ödemeyle, kapatmayla, Z-38'le, retle, düzeltmeyle ya da "havale gerçekleşmedi" ile silinir (K-708, K-1110, K-1127, K-1056, K-1131, K-1097); mal sonra ulaşırsa yeniden istenir | UC-131 · UC-142 · UC-140 · UC-150 · UC-132 · UC-35 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §4.1.41` | Z-36 periyodik imha — kendiliğinden çalışan iş; panelde düğme yok | uç doğurmaz — sistem işi (`05 §5`) · uç doğurmaz — yoktur | uç doğurmaz |
| `03 §4.1.42` | Z-40 giriş kaydı ve sistem kayıtları — periyodik imhayla, dosyalar dahil (K-687, K-916) | uç doğurmaz — sistem işi (`05 §5`) | uç doğurmaz |
| `03 §4.1.43` | Z-44 imha kaydı — silinir; kişisel veri içermez | uç doğurmaz — sistem işi (`05 §5`) | uç doğurmaz |
| `03 §4.1.44` | Z-45 geri alma bağlantısının ömrü — dolunca yeniden değiştirme engeli kalkar, eski adres silinir (K-1065) | UC-75 · UC-76 · UC-187 · UC-66 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §4.1.45` | Z-46 tanınan tarayıcı işareti — son başarılı girişten; dolunca e-posta ekseninde sayılır; sıfırlama, şifre değiştirme, geri alma ve silme düşürür (K-695) | UC-55 · UC-110 · UC-59 · UC-65 · UC-76 · UC-68 · konvansiyon — API2-08 | eşlendi |
| `03 §4.1.46` | Z-37 veri ihlalinde Kurul'a bildirim — sayaç yok; yükümlülük firmanın | uç doğurmaz — ürünün dışında | uç doğurmaz |
| `03 §4.1.47` | Z-38 malı dönmeyen caymada IBAN'ın kendiliğinden silinmesi — Z-42'nin dolduğu gün; kalem kapanmaz; başka geri ödeme bekliyorsa kalır | uç doğurmaz — sistem işi (`05 §5`) | uç doğurmaz |
| `03 §4.2` metin | Kendiliğinden işleyen anlar — süresi olmayan ama zamanı kural olan anlar ve kesintideki davranış | uç doğurmaz — sistem işi (`05 §5`) | uç doğurmaz |
| `03 §4.2.1` | Sipariş onayı — numara, donma, ayırma, aynı sepetin önceki siparişinin iptali; B-1, B-2, F-1 | UC-27 | eşlendi |
| `03 §4.2.2` | Ödeme onayı — kesin düşüm, kupon, sepetten çıkma, Z-10 ve Z-39, dijital teslim; B-4 | UC-80 · UC-123 | eşlendi |
| `03 §4.2.3` | Kendiliğinden iptal — Z-7, Z-8, başarısız kart, aynı sepetten yeni sipariş; kartta son sorgu; B-7 | uç doğurmaz — sistem işi (`05 §5`) · UC-80 · UC-27 | eşlendi |
| `03 §4.2.4` | İptal, çıkarma ve gecikme feshi — adet stoğa ve kontenjana anında döner; kupon yalnız tamamında; kapanış kuralı (K-672, K-791, K-1106) | UC-40 · UC-41 · UC-129 · UC-138 · UC-130 | eşlendi |
| `03 §4.2.5` | İade — stok kendiliğinden dönmez, teslim alma ve ekleme; hizmet kontenjanı caymada döner (K-791) | UC-140 · UC-141 · UC-43 | eşlendi |
| `03 §4.2.6` | Kısmi iptal ve iade — kupon ve kargo yeniden hesaplanmaz; kargo ücreti iki tetikte (K-789) | uç doğurmaz — sistem işi (`05 §5`) · UC-131 | eşlendi |
| `03 §4.2.7` | Kart hattında kendiliğinden geri ödeme — onay penceresi yok; gerçekleşince Ö3–Ö5; başarısızlıkta işaret, yeniden denenmez; sonucu bilinmezse sorgu (K-940); B-8, F-4 | uç doğurmaz — sistem işi (`05 §5`) · UC-92 | eşlendi |
| `03 §4.2.8` | Malı dönmeyen caymada IBAN'ın silinmesi — kapatmayla ya da Z-38 ile; silme kalemi kapatmaz (K-1110, K-1127) | UC-142 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §4.2.9` | Periyodik imha (Z-36) — imha kaydı (Z-44); hesap silme, yönetici kaldırma, bekleyen kayıt, IBAN ve bekleyen e-posta değişikliğinin silinmesi de kayda (K-708, K-984, K-1064) | uç doğurmaz — sistem işi (`05 §5`) | uç doğurmaz |
| `03 §4.2.10` | E-postanın yeniden denenmesi (Z-26) — artan aralıkla üç kez; art arda başarısızlıkta ana sayfada kanal uyarısı | uç doğurmaz — sistem işi (`05 §5`) · UC-86 | eşlendi |
| `03 §4.2.11` | Sitenin kesintisi — süreler işler; kaçırılan işler dönüşte sırayla çalışır; son sorgu kesintideki ödemeyi bulur; yasal süreler dondurulmaz, başka kanal açık | uç doğurmaz — sistem işi (`05 §5`) · UC-130 | eşlendi |
| `03 §4.2.12` | Havale süresinin kesintide dolması — iptal ilk iş gününün sonuna ertelenir; yönetici işaretler; sipariş sayfası yeni son günü gösterir; tespit `05`'in (K-667) | uç doğurmaz — sistem işi (`05 §5`) · UC-123 · UC-33 | eşlendi |

#### 1.1.10 Kullanıcı Akışları — `03 §5`–§10 itiraz, kötüye kullanım, bildirim haritası, yönetim, destekleyici ve operasyonel akışlar (305)

Kaynağın sırasıyla: `03 §5` (itiraz ve anlaşmazlık), §6 (kötüye kullanım ve inceleme — deneme limitleri, teyit ve kandırılma, firmanın incelemesi), §7 (bildirim haritası — toplu), §8 (yönetim akışları), §9 (destekleyici akışlar — kayıt, giriş, profil, hesap silme, tercihler), §10 (operasyonel akışlar — dış servis kesintileri, bakım, sitenin kesintisi, panele erişimin kaybı); kimlik biçimi K-1002'ninkidir (§1.1). Eşlemenin okunuşu K-1157'nin beş kuralıdır, 1e onları bu bölümlere uygular (K-1158): adım aktörün işleminin adayına ya da açılan sayfanın GET adayına; yöneticinin adımı panelin adayına; işlemin açık olduğu aralığı ya da koşulunu yazan satır ayrıca `konvansiyon — API8-01`'i, deneme limiti `konvansiyon — API2-08`'i, panelin reddettiği değer `konvansiyon — API5-01`'i, onay adımı `konvansiyon — API2-02`'yi taşır; kendiliğinden iş *uç doğurmaz — sistem işi (`05 §5`)*, ürünün dışında yürüyen adım — firmanın e-postası, teyit, sağlayıcının paneli, taşıyıcı — *uç doğurmaz — ürünün dışında* ya da *uç doğurmaz — dış sayfa*, kurulum tarafının işi *uç doğurmaz — kurulum*, kaynağın "yoktur" dediği işlev *uç doğurmaz — yoktur*. **§7 toplu girer** (K-1150): bölüm metni ve üç alt bölüm satırı; bildirimin kendisi *uç doğurmaz — e-posta (`08`)*'dir, tetikleyen olay akışın satırında eşlenir. **Bağlantı taşıyan bildirim satırı kimliğiyle ayrıca girer** — `opt03()`'ün isteğe bağlı kümesinden 14 satır: bağlantısı ürünün içine inen 13 satır (B-1'in iki satırı, B-14'ün altı satırı, beş hesap ve yönetim e-postası) bağlantının indiği uca ve bağlantının işlediği POST adayına, kargo şirketinin sayfasına inen "Takip et" (7.1.9) *uç doğurmaz — dış sayfa*'ya; "bağlantı" geçen öteki dört satır bağlantı taşımaz — 7.1.35, 7.1.46 ve 7.1.54 taşımadığını yazar, 7.2.26 bir olay listesidir — ve alt bölüm satırıyla karşılanır.

| Kaynak ID | Kaynak özeti | Uç | Durum |
|---|---|---|---|
| `03 §5` metin | İtiraz ve anlaşmazlık akışlarının okunuşu: ayrı şikâyet ya da itiraz kanalı yoktur; ürün uyuşmazlığa karar vermez, kararın sonucunu panelin var olan işlemleriyle uygular | uç doğurmaz — yoktur · uç doğurmaz — ürünün dışında | uç doğurmaz |
| `03 §5.1.1` | Sipariş sayfası üç yolu ayrı düğmeyle sunar — iptal, cayma, ayıp talebi; her birinin açık olduğu aralık §1.11'de | UC-33 · UC-38 · UC-42 · UC-44 · konvansiyon — API8-01 | eşlendi |
| `03 §5.1.2` | Üç yola girmeyen konu iletişim formunun "Sipariş hakkında" tipiyle gelir; şikâyet listesi, canlı destek ve talep takibi yok; F-2 | UC-14 · UC-15 · uç doğurmaz — yoktur | eşlendi |
| `03 §5.1.3` | Yönetici talebi okur, cevabı sistemin dışından e-postayla verir; sonuç bir sipariş işlemi gerektiriyorsa panelin var olan işlemiyle — başka kanaldan cayma kaydı, firma iptali, tutar bazlı geri ödeme, teslim tarihi düzeltmesi; talep kapatılır | UC-94 · UC-155 · UC-130 · UC-129 · UC-133 · UC-148 · uç doğurmaz — ürünün dışında | eşlendi |
| `03 §5.1.4` | Çözülmüş ayıbın tekrarı — talep yeniden açılır, yeni talep açılmaz; Z-18 dolduysa yol iletişim formu | UC-46 · UC-15 · konvansiyon — API8-01 | eşlendi |
| `03 §5.1.5` | Müşteri cevabı kabul etmez — hakem heyeti ve mahkeme yolu siparişe donmuş metinlerde, sipariş sayfasında ve B-1'in gövdesinde; ürün karar vermez | UC-33 · uç doğurmaz — ürünün dışında | eşlendi |
| `03 §5.1.6` | Müşteri lehine karar — tutar bazlı kısmi geri ödeme ya da kalemin iade hattı, havalede IBAN isteği ve sipariş sayfasından giriş (K-686), başka kanaldan cayma kaydı; karar ayrıca kaydedilmez, iç not | UC-133 · UC-131 · UC-134 · UC-35 · UC-130 · UC-152 | eşlendi |
| `03 §5.1.7` | İtiraza karşı kanıt — siparişe donmuş metinler, onay kutularının kaydı, gönderim kaydı, işlem izi, beyanların tarih damgası, e-posta geçmişi; ürün fotoğraf, belge ve kanıt dosyası almaz | UC-92 · UC-106 · uç doğurmaz — yoktur | eşlendi |
| `03 §5.2.1.1` | Ters ibraz — ürün bilmez: sağlayıcıdan bilgi almaz; ödeme ekseni, satış özeti ve dışa aktarma değişmez | uç doğurmaz — yoktur · uç doğurmaz — ürünün dışında | uç doğurmaz |
| `03 §5.2.1.2` | Yönetici itirazı sağlayıcının panelinde cevaplar; sonucu iç nota yazar (§1.11.27) | uç doğurmaz — dış sayfa · UC-152 | eşlendi |
| `03 §5.2.1.3` | Geri ödeme adımından önce sağlayıcının paneline bakılır — yeniden deneme, havale yolu, havale geri ödemesi, "gerçekleşmedi" işareti; işaretlerdeki uyarı hatırlatır (K-940) | UC-135 · UC-136 · UC-131 · UC-137 · UC-92 · uç doğurmaz — dış sayfa | eşlendi |
| `03 §5.2.1.4` | Kart hattında iptal ve ters ibraz edilmiş işlemde kendiliğinden iade — para ikinci kez gider; fazla ürünün dışında çözülür, kalan risk | UC-39 · UC-40 · uç doğurmaz — sistem işi (`05 §5`) · uç doğurmaz — ürünün dışında | eşlendi |
| `03 §5.2.2.1` | Yolda kaybolan iade gönderisi — kalem "iade malı bekleniyor"da, geri ödeme süresi mal ulaşmadan başlamaz | UC-33 · UC-92 · uç doğurmaz — ürünün dışında | eşlendi |
| `03 §5.2.2.2` | Kayıp iletişim formundan bildirilir; formda dosya eki yok, yazışma sistemin dışında | UC-15 · uç doğurmaz — yoktur | eşlendi |
| `03 §5.2.2.3` | Z-42'den sonra "mal dönmedi" kapanışı — isteğe bağlı; geri ödeme borcuna karar vermez, havalede IBAN silinir | UC-142 · konvansiyon — API8-01 | eşlendi |
| `03 §5.2.2.4` | Firma ödemeye karar verir — kaleme bağlı tutar bazlı geri ödeme; IBAN silinmişse IBAN isteği ve sipariş sayfasından giriş (K-686) | UC-133 · UC-134 · UC-35 | eşlendi |
| `03 §5.2.2.5` | Mal sonradan ulaşır — teslim alma kalemi yeniden açar; ödenmiş tutar düşülür | UC-140 | eşlendi |
| `03 §5.2.3.1` | Pencere kapandıktan ya da istisna kaleminden eksik bilgilendirmeyle cayma isteği — sipariş sayfasındaki düğme açılmaz; formdan gelirse iletişim talebi | UC-33 · konvansiyon — API8-01 · UC-15 | eşlendi |
| `03 §5.2.3.2` | Yönetici başka kanaldan cayma kaydı yapar — panel pencere dışı tarihi ya da istisnayı söyler ve ayrıca onaylatır, bir yıldan sonraya tarihli kaydı reddeder; kayıt geri alınmaz (K-688, K-709) | UC-130 · konvansiyon — API2-02 · konvansiyon — API5-01 | eşlendi |
| `03 §5.2.3.3` | Kayıttan sonra olağan cayma hattı işler (§2.8) | UC-140 · UC-141 · UC-131 | eşlendi |
| `03 §5.2.3.4` | Bilgilendirme eksik değilse kayıt yapılmaz; cevap firmanın e-postası | uç doğurmaz — ürünün dışında | uç doğurmaz |
| `03 §5.2.4.1` | Teslim tarihine itiraz — düğme açılmaz; pencere fiilî teslimden işler, ispat firmada | UC-33 · konvansiyon — API8-01 | eşlendi |
| `03 §5.2.4.2` | Cayma başka kanaldan bildirilir; yönetici teyit eder (§6.2.1) | UC-15 · uç doğurmaz — ürünün dışında | eşlendi |
| `03 §5.2.4.3` | Yönetici teslim tarihini düzeltir ya da bildirimi kaydeder — pencereler düzeltilmiş tarihten işler; çakışmada panel teslimin sırasını sorar | UC-148 · UC-130 | eşlendi |
| `03 §5.2.4.4` | Tarih doğruysa ürün karar vermez; uyuşmazlık 5.1.5'in yolundan | uç doğurmaz — ürünün dışında | uç doğurmaz |
| `03 §5.3.1.1` | Koşullu istisna kaleminden cayma — beyan ekranı "koruyucu ambalajı açılmamışsa" koşulunu yazar | UC-42 · UC-43 | eşlendi |
| `03 §5.3.1.2` | İade reddi — yalnız koşullu istisna kaleminde sunulur; onay sonucu söyler, geri alınmaz; kayıt ulaşma tarihini taşır, işlem izine (K-656, K-657) | UC-140 · konvansiyon — API8-01 · konvansiyon — API2-02 | eşlendi |
| `03 §5.3.1.3` | Reddin sonuçları — geri ödeme yok, sayaçtan düşme, "iade malı bekleniyor" kalkar, havalede IBAN silinir, stoğa girmez; eksenler değişmez | UC-140 | eşlendi |
| `03 §5.3.1.4` | Malın geri gönderimi firmanın kendi taşıyıcısıyla — sistemin dışında, takip numarası istenmez | uç doğurmaz — ürünün dışında | uç doğurmaz |
| `03 §5.3.1.5` | Müşteri reddi kabul etmez — iletişim formu ve uyuşmazlık yolu; ürün kanıt almaz | UC-15 · uç doğurmaz — ürünün dışında | eşlendi |
| `03 §5.3.1.6` | Firma kararından döner — kaleme bağlı tutar bazlı geri ödeme; ret kaydı izde kalır; IBAN silinmişse IBAN isteği ve sipariş sayfasından giriş (K-686) | UC-133 · UC-134 · UC-35 | eşlendi |
| `03 §5.3.2.1` | Kullanılmış, hasarlı ya da eksik dönen mal — teslim alma olağan; ret seçeneği sunulmaz (K-668) | UC-140 · konvansiyon — API8-01 | eşlendi |
| `03 §5.3.2.2` | Geri ödeme tam işlenir, kesinti alanı yok; stoğa ekleme yöneticinin kontrolünde | UC-131 · UC-141 · uç doğurmaz — yoktur | eşlendi |
| `03 §5.3.2.3` | Değer kaybı ürünün dışında istenir; ürün değerlendirmez ve kaydetmez, iç not | UC-152 · uç doğurmaz — ürünün dışında | eşlendi |
| `03 §6` metin | Kötüye kullanım ve inceleme akışlarının okunuşu: ürün düzeyindeki önlemler `02 §8`'de; genel istek hızı sınırı ve dosya güvenliği `05`'te; teyidin ispatı ve kandırılmanın sonucu firmada | konvansiyon — API2-08 · uç doğurmaz — ürünün dışında | konvansiyon |
| `03 §6.1.1` metin | Limitlerin eşikleri ve pencereleri `02 §11.2`'de; pencere kayan pencere (Z-27), L-8 açık kayıt sayar | konvansiyon — API2-08 | konvansiyon |
| `03 §6.1.1.1` | L-1 başarısız şifreli giriş ve yeniden doğrulama — müşteri ve yönetici; IP ve e-posta ekseni, tanınan tarayıcının sayacı; Google L-1'e girmez (K-711) | UC-55 · UC-61 · UC-110 · UC-185 · konvansiyon — API2-08 | eşlendi |
| `03 §6.1.1.2` | L-2 şifre sıfırlama talebi — IP ve e-posta | UC-57 · UC-111 · konvansiyon — API2-08 | eşlendi |
| `03 §6.1.1.3` | L-3 hesap kaydı — IP ve e-posta | UC-48 · konvansiyon — API2-08 | eşlendi |
| `03 §6.1.1.4` | L-4 iletişim formu — IP; tuzak alanı dolduran gönderim sessizce düşer | UC-15 · konvansiyon — API2-08 | eşlendi |
| `03 §6.1.1.5` | L-5 başarısız misafir sipariş takibi sorgusu — IP ve sorguya yazılan e-posta; e-postadaki bağlantı ve üyenin hesabı açık kalır | UC-32 · UC-34 · konvansiyon — API2-08 | eşlendi |
| `03 §6.1.1.6` | L-6 geçersiz kupon kodu — sepet ekseni; girişte birleşen sepet iki sayacın büyüğünü taşır | UC-23 · konvansiyon — API2-08 | eşlendi |
| `03 §6.1.1.7` | L-7 kendiliğinden iptal edilen ödenmemiş sipariş — yeni sipariş onaylanamaz; yalnız giriş yapmış üyenin siparişleri e-posta ekseninde | UC-27 · konvansiyon — API2-08 | eşlendi |
| `03 §6.1.1.8` | L-8 aynı anda açık ödenmemiş sipariş — havale seçilemez, kart yolu açık | UC-22 · UC-27 · konvansiyon — API2-08 | eşlendi |
| `03 §6.1.1.9` | L-9 doğrulama bağlantısının yeniden istenmesi — kayıt ve yeni e-posta, müşteri ve yönetici; önceki bağlantı geçerli kalır; yeni davet limite girmez (K-658, K-734, K-759) | UC-49 · UC-67 · UC-180 · UC-175 · konvansiyon — API2-08 | eşlendi |
| `03 §6.1.2.1` | Eşiği aşan deneme — yalnız o işlem, o eksende geçici engellenir; mesaj nötr; hesap kilitlenmez | konvansiyon — API2-08 · konvansiyon — API2-07 | konvansiyon |
| `03 §6.1.2.2` | Engel kendiliğinden açılır (Z-27); L-8'de açık siparişlerden biri kapanınca tavan boşalır | konvansiyon — API2-08 | konvansiyon |
| `03 §6.1.2.3` | Yönetici limite takılır — panel girişine muafiyet yok | UC-110 · konvansiyon — API2-08 | eşlendi |
| `03 §6.1.2.4` | Ürün düzeyinde limiti olmayan işlem — sepete ekleme, stok yoklama; genel istek hızı sınırı `05`'in | UC-05 · konvansiyon — API2-08 | eşlendi |
| `03 §6.1.3.1` | Girişe yönelik saldırı — L-1'in e-posta ekseni işaretsiz tarayıcılardan gelen denemeleri sayar ve engeller; IP ekseni ayrıca | UC-55 · konvansiyon — API2-08 | eşlendi |
| `03 §6.1.3.2` | Gerçek kullanıcı tanınan tarayıcıdan girer — e-posta ekseninde engellenmez, kendi sayacında sayılır (Z-46) | UC-55 · konvansiyon — API2-08 | eşlendi |
| `03 §6.1.3.3` | Yeni tarayıcıdan giriş — şifre sıfırlama bağlantısı; şifre yenilenir, oturumlar kapanır, sıfırlayan tarayıcı tanınır | UC-57 · UC-58 · UC-59 · konvansiyon — API2-08 | eşlendi |
| `03 §6.1.3.4` | Saldırgan sıfırlama taleplerini de doldurur — L-2 engeller; kalan risk | UC-57 · konvansiyon — API2-08 | eşlendi |
| `03 §6.1.3.5` | Google ile giriş — L-1'in engeli kapatmaz | UC-50 · UC-51 | eşlendi |
| `03 §6.1.4.1` | Art arda ödenmemiş sipariş — süre dolunca kendiliğinden iptal ve ayırmanın bırakılması; L-7 yeni onayı engeller | uç doğurmaz — sistem işi (`05 §5`) · UC-27 · konvansiyon — API2-08 | eşlendi |
| `03 §6.1.4.2` | Havale siparişleriyle stok kilitleme — L-8 tavan doluyken havaleyi kapatır; kart açık | UC-22 · UC-27 · konvansiyon — API2-08 | eşlendi |
| `03 §6.1.4.3` | Çok IP'den misafir havale siparişleri — ürün kendiliğinden durdurmaz; inceleme yolu §6.3.1 | UC-91 · UC-129 | eşlendi |
| `03 §6.1.4.4` | Paylaşılan IP'nin arkasındaki gerçek müşteri — engel kendiliğinden açılır; muafiyet ve kurtarma yolu yok, kalan risk | konvansiyon — API2-08 · uç doğurmaz — yoktur | konvansiyon |
| `03 §6.2.1.1` | Siparişe dokunan talep siparişin iletişim e-postasından geldiyse teyit budur | uç doğurmaz — ürünün dışında | uç doğurmaz |
| `03 §6.2.1.2` | Başka kanaldan gelen talep — yönetici siparişi numarasıyla ya da e-postasıyla bulur (8.2.11; K-714) ve siparişin kendi kanalına döner | UC-91 · konvansiyon — API9-02 · uç doğurmaz — ürünün dışında | eşlendi |
| `03 §6.2.1.3` | Misafir siparişinin e-posta düzeltmesinde kimlik siparişteki bilgilerle teyit edilir — ad, teslimat telefonu, kalemler, tutar | UC-92 · uç doğurmaz — ürünün dışında | eşlendi |
| `03 §6.2.1.4` | Sahibine ulaşılamazsa karar firmanın; teyit kaydın tarihini değiştirmez | uç doğurmaz — ürünün dışında | uç doğurmaz |
| `03 §6.2.2.1` | Adres çevirtme girişimi — 6.2.1'in kalıbıyla teyit | uç doğurmaz — ürünün dışında | uç doğurmaz |
| `03 §6.2.2.2` | Kandırılmış adres düzeltmesi — işlem izine alan ve işlem olarak, adres değeri ize girmez; B-10 | UC-146 | eşlendi |
| `03 §6.2.2.3` | Gerçek sahip B-10'u görür — Kargoya verildi'ye geçmediyse ilk adres adres geçmişinden okunur ve geri düzeltilir; yola çıktıysa düzeltilemez | UC-92 · UC-146 · konvansiyon — API8-01 | eşlendi |
| `03 §6.2.3.1` | Sahte cayma ya da iptal bildirimi — teyit; sahibi yapmadığını söylerse kayıt yapılmaz | uç doğurmaz — ürünün dışında | uç doğurmaz |
| `03 §6.2.3.2` | Teyit atlanır — cayma kaydı ya da "müşteriyle anlaşıldı" iptali; bildirimdeki IBAN aktarılmaz, para siparişin yoluna, havalede yalnız sipariş sayfasından girilen IBAN'a (K-690) | UC-130 · UC-129 · UC-35 | eşlendi |
| `03 §6.2.3.3` | Gerçek sahip ulaşır — yanlış iptal yalnız Ödendi'de kalmışken düzeltilir; cayma beyanı geri alınmaz, Z-42'den sonra "mal dönmedi" (K-688) | UC-150 · UC-142 · konvansiyon — API8-01 | eşlendi |
| `03 §6.2.4.1` | Misafir siparişinin e-postasını çevirtme girişimi — kimlik siparişteki bilgilerle teyit | uç doğurmaz — ürünün dışında | uç doğurmaz |
| `03 §6.2.4.2` | Kandırılmış e-posta düzeltmesi — yeni erişim anahtarı, eski bağlantılar açılmaz; sipariş hesaba düşer ya da kopar; "e-posta düzeltildi" işareti (K-1116) | UC-151 · UC-34 · UC-92 | eşlendi |
| `03 §6.2.4.3` | Kandıran sipariş sayfasına girer — iptal, cayma, IBAN girişi ya da düzeltmesi; para siparişin yoluna | UC-34 · UC-33 · UC-39 · UC-40 · UC-43 · UC-35 | eşlendi |
| `03 §6.2.4.4` | Gerçek sahip B-15'i görür — e-posta yeniden düzeltilir, yeni anahtar; aradaki işlemler kalem kayıtlarından ve e-posta geçmişinden okunur; IBAN sipariş sayfasından düzeltilir (K-710) | UC-151 · UC-92 · UC-35 | eşlendi |
| `03 §6.2.5.1` | Ele geçirilmiş oturumdan hesabın e-postasının değiştirilmesi — yeniden doğrulama ister; yeni adres doğrulanınca geçerli | UC-66 · UC-60 · UC-61 · UC-62 · UC-52 · UC-53 | eşlendi |
| `03 §6.2.5.2` | "Bu değişikliği ben yapmadım" bağlantısı Z-45 içinde — e-posta geri alınır, oturumlar kapanır, şifre eski adrese giden bağlantıyla; bağlantı açıkken e-posta değiştirilemez | UC-75 · UC-76 · UC-58 · UC-59 · konvansiyon — API8-01 | eşlendi |
| `03 §6.2.5.3` | Z-45 dolmuş ya da eski posta kutusu da ele geçirilmiş — ürün içinde geri alma yolu yok; siparişlerin erişim anahtarı etkilenmez | UC-34 · uç doğurmaz — yoktur | eşlendi |
| `03 §6.2.6.1` | Ele geçirilmiş yönetici hesabıyla havale IBAN'ı değiştirilir — işlem izine eski ve yeni değerle; yeni IBAN yalnız yeni siparişlere; F-5 bütün yöneticilere (K-663) | UC-167 | eşlendi |
| `03 §6.2.6.2` | Yönetici F-5'i tanımaz — IBAN'ı düzeltir ve şifresini değiştirir; ürün kendiliğinden geri almaz | UC-167 · UC-181 | eşlendi |
| `03 §6.2.6.3` | Yanlış IBAN'ı taşıyan siparişler firma iptaliyle kapatılır ("diğer", açıklamayla) | UC-129 | eşlendi |
| `03 §6.2.6.4` | Saldırgan öteki yöneticileri kaldırır — her kaldırma F-6; son yönetici kaldırılamaz; erişimi kaybeden firmanın yolu kurulum tarafının kurtarması (K-692, K-699) | UC-177 · konvansiyon — API8-01 · uç doğurmaz — kurulum | eşlendi |
| `03 §6.3.1.1` | Açık havale siparişlerinin kilidi — "Ödeme onayı bekleyen" sayacı süzülmüş listeye götürür; stok alanının yanında ayrılmış adet ve siparişler (K-653) | UC-86 · UC-91 · UC-88 | eşlendi |
| `03 §6.3.1.2` | Şüpheli siparişler firma iptaliyle kapatılır; ayrılanlar serbest kalır | UC-129 | eşlendi |
| `03 §6.3.1.3` | Havalenin geçici kapatılması — yeni siparişte havale görünmez; açık siparişler etkilenmez (K-664) | UC-167 | eşlendi |
| `03 §6.3.2.1` | Veri ihlalinde ürün tespit etmez, uyarı üretmez; panelde ihlal ekranı yok; Kurul'a ve kişilere bildirim firmanın | uç doğurmaz — yoktur · uç doğurmaz — ürünün dışında | uç doğurmaz |
| `03 §6.3.2.2` | İşlem izi tarih ve yönetici süzgeciyle okunur — dışa aktarmalar dahil; değiştirilemez | UC-106 · konvansiyon — API9-02 | eşlendi |
| `03 §6.3.2.3` | Giriş kaydı panelde görünmez — sistem kayıtlarıyla barındırma tarafında okunur (K-687) | uç doğurmaz — yoktur · uç doğurmaz — kurulum | uç doğurmaz |
| `03 §6.3.2.4` | Ele geçirilmiş yönetici hesabının kaldırılması — oturumlar sonlanır, davetleri düşer; son yönetici kaldırılamaz (K-662, K-692) | UC-177 | eşlendi |
| `03 §6.3.3.1` | Etkilenen kişilerin listesi — sipariş, üye ve iletişim talebi dışa aktarmaları; her dışa aktarma işlem izine | UC-182 · UC-183 · UC-184 | eşlendi |
| `03 §6.3.3.2` | Firma kişilere kendi e-posta aracıyla yazar — ürünün toplu e-posta hattı yok; sitede yayım gerekirse duyuru şeridi ve genel sayfa | uç doğurmaz — yoktur · uç doğurmaz — ürünün dışında · UC-158 · UC-159 | eşlendi |
| `03 §7` metin | Bildirim haritasının okunuşu: akışların bildirim hücrelerinden çıkarılmıştır; tek kanal e-posta, işlem bildirimleri kapatılamaz; adresler; ulaşmayan e-postada işaret ve paneldeki yeniden gönderim (8.9.2); içerik ve şablon `08`'in | uç doğurmaz — e-posta (`08`) · UC-153 · UC-154 | eşlendi |
| `03 §7.1` | Bildirimler ve tetikleyicileri — 54 satır: B-1…B-16, F-1…F-6 ve hesap e-postaları; tetikleyen olay akışın satırındadır ve orada eşlenir; bağlantı taşıyan satırlar aşağıda kimliğiyle; 7.1.37 yöneticinin yeniden gönderimi | uç doğurmaz — e-posta (`08`) · UC-153 · UC-154 | eşlendi |
| `03 §7.1.1` | B-1 Sipariş alındı — sipariş sayfasının bağlantısını ve donmuş metinleri taşır | UC-34 · uç doğurmaz — e-posta (`08`) | eşlendi |
| `03 §7.1.2` | B-1 misafir siparişinin e-posta düzeltmesinde yeni adrese — bağlantı yeni erişim anahtarını taşır | UC-34 · UC-151 · uç doğurmaz — e-posta (`08`) | eşlendi |
| `03 §7.1.9` | B-5 Kargoya verildi — listedeki şirketlerde "Takip et" bağlantısı kargo şirketinin sayfasına | uç doğurmaz — dış sayfa · uç doğurmaz — e-posta (`08`) | uç doğurmaz |
| `03 §7.1.29` | B-14 IBAN gerekli — IBAN silindikten sonra ulaşan iade malı; sipariş sayfasının bağlantısı | UC-34 · UC-35 · uç doğurmaz — e-posta (`08`) | eşlendi |
| `03 §7.1.30` | B-14 — kart iadesi gerçekleşmez ve havale yolu açılır; bağlantı | UC-34 · UC-35 · uç doğurmaz — e-posta (`08`) | eşlendi |
| `03 §7.1.31` | B-14 — geri ödeme havalesi bankada gerçekleşmez; bağlantı | UC-34 · UC-35 · uç doğurmaz — e-posta (`08`) | eşlendi |
| `03 §7.1.32` | B-14 — havale hattında IBAN'sız cayma ya da gecikme feshi kaydı (K-706); bağlantı | UC-34 · UC-35 · uç doğurmaz — e-posta (`08`) | eşlendi |
| `03 §7.1.33` | B-14 — IBAN'ı olmayan geri ödeme için IBAN isteği; bağlantı | UC-34 · UC-35 · uç doğurmaz — e-posta (`08`) | eşlendi |
| `03 §7.1.34` | B-14 — havale hattında ödenmiş kalemde firma iptali ya da kalem çıkarma; bağlantı | UC-34 · UC-35 · uç doğurmaz — e-posta (`08`) | eşlendi |
| `03 §7.1.49` | E-posta doğrulama bağlantısı — kayıt ve yeniden istenen bağlantı (Z-1); öncekileri geçersiz kılar (K-696) | UC-52 · UC-53 · uç doğurmaz — e-posta (`08`) | eşlendi |
| `03 §7.1.50` | Şifre sıfırlama bağlantısı — müşteri ve yönetici; şifre belirleme ve geri almanın sıfırlaması dahil (Z-2) | UC-58 · UC-59 · UC-84 · UC-112 · uç doğurmaz — e-posta (`08`) | eşlendi |
| `03 §7.1.51` | Yeni e-posta adresinin doğrulanması — müşteri ve yönetici hesabı | UC-52 · UC-53 · UC-108 · UC-186 · uç doğurmaz — e-posta (`08`) | eşlendi |
| `03 §7.1.52` | E-posta değişikliğinin eski adrese bildirimi — "bu değişikliği ben yapmadım" bağlantısı (Z-45); yeni adresi taşımaz | UC-75 · UC-76 · UC-108 · UC-187 · uç doğurmaz — e-posta (`08`) | eşlendi |
| `03 §7.1.53` | Yönetici daveti — tek kullanımlık davet bağlantısı (Z-3) | UC-85 · UC-113 · uç doğurmaz — e-posta (`08`) | eşlendi |
| `03 §7.2` | Bildirim üretmeyen olaylar ve olmayan bildirimler — 30 satır; ekranda söylenen mesaj bildirim değildir | uç doğurmaz — e-posta (`08`) · uç doğurmaz — yoktur | uç doğurmaz |
| `03 §7.3` | Geçiş ve olay bazlı eşleme — 47 satır; §7.1 ve §7.2'yi tersinden okur | uç doğurmaz — e-posta (`08`) | uç doğurmaz |
| `03 §8` metin | Yönetim akışlarının okunuşu: katalog (akış 5), sipariş yürütümü ve müdahaleler (akış 6), kurumsal içeriğin panel yüzü (akış 7), mağaza ayarları ve yönetici hesapları (akış 8); üye kaydı ve talep üzerine silme §9.4'te | uç doğurmaz — yazım konvansiyonu | uç doğurmaz |
| `03 §8` m1 | Biçim ve aktör — panel tek roldür, her yönetici her işlemi yapar ve bütün müşteri verisini görür; mobilde kapatılan işlev yok; işlemin açık olduğu aralık §1.11'de | konvansiyon — API0-07 · konvansiyon — API8-01 | konvansiyon |
| `03 §8` m2 | Bildirim — firmanın panelde kendi yaptığı işlem firmaya bildirim üretmez; istisnalar F-5 ve F-6 | uç doğurmaz — e-posta (`08`) | uç doğurmaz |
| `03 §8` m3 | İz — işlem izinin sekiz alanından birine yazılan işlem; fiyat, stok girişi ve içeriğin metni ize yazılmaz | UC-106 · konvansiyon — API9-01 | eşlendi |
| `03 §8` m4 | Elle adım — sipariş başına zorunlu ve koşullu adımlar §8.5'te sayılır | uç doğurmaz — yazım konvansiyonu | uç doğurmaz |
| `03 §8` m5 | Dallar — hataların tamamı §3.5'te, bu bölümün satırlarına bağlanır; süreler §4'te | konvansiyon — API2-07 | konvansiyon |
| `03 §8.1` metin | Katalog panelden tek tek yönetilir; yayın durumları §1.8'de; katalog değişikliği verilmiş siparişe dokunmaz | UC-87 · UC-88 · UC-89 · UC-90 | eşlendi |
| `03 §8.1.1` | Ürün oluşturma ve düzenleme — tip, ad, kategoriler, ana kategori; yeni ürün Taslak doğar; liste adla aranır, yayın durumuyla süzülür (K-737); eşzamanlı düzenlemede kaydın değiştiği uyarısı | UC-88 · UC-114 · UC-87 · konvansiyon — API9-02 · konvansiyon — API2-04 | eşlendi |
| `03 §8.1.2` | Varyantlar — en fazla iki boyut (P-21), tekil `SKU`, fiyat ve net miktar (K-1019); varyant kendi yayın durumunu taşır | UC-114 · konvansiyon — API5-01 | eşlendi |
| `03 §8.1.3` | Stok ve kontenjan — yanında ayrılmış adet ve siparişler; ayrılmış adedin altına inen değer reddedilir, mal yoksa ayırmayı tutan sipariş firma iptaliyle kapanır (K-653) | UC-88 · UC-114 · konvansiyon — API2-04 · UC-129 | eşlendi |
| `03 §8.1.4` | Tipe özgü alanlar — üretim yeri, ölçü birimi, P-6'nın P-5 çiti, cayma istisnası ve sebebi, ifa süresi, siparişte en fazla adet; değişiklik yalnız yeni siparişlere | UC-114 · konvansiyon — API5-01 | eşlendi |
| `03 §8.1.5` | Görseller — yükleme, sıra, ana görsel, açıklama; P-22, P-23, P-24; alternatif metin boşsa sistem üretir | UC-117 · UC-114 · konvansiyon — API4-07 | eşlendi |
| `03 §8.1.6` | Dijital dosyanın yüklenmesi ve güncellenmesi — tek dosya (P-26); güncellenen dosya geçmiş alıcılara da iner; yayındaki varyantı dosyasız bırakan silme engellenir | UC-83 · UC-114 · UC-116 · konvansiyon — API4-07 | eşlendi |
| `03 §8.1.7` | Taslak ürünün "Taslak" bandıyla önizlenmesi ve yayına alınması — yayın kapısı, eksik koşul panelde; adres ilk yayında sabitlenir; ize yazılır | UC-04 · UC-115 · konvansiyon — API8-01 | eşlendi |
| `03 §8.1.8` | Ürünü ya da varyantı taslağa, arşive ya da yeniden yayına alma — son Yayında varyantın arşivi engellenir; vitrinden kalkan kalem sepetlerden çıkar | UC-115 · konvansiyon — API8-01 | eşlendi |
| `03 §8.1.9` | Ürünü ya da varyantı kalıcı silme — onaydan önce bağlı açık siparişlerin sayısı; son Yayında varyant silinmez; dosyanın son hâli saklanır, adres serbest kalır (K-655, K-674) | UC-116 · konvansiyon — API2-02 · konvansiyon — API8-01 | eşlendi |
| `03 §8.1.10` | Varyant fiyatının değişmesi — fiyat geçmişine yazılır; sepette "fiyatı değişti" satırı; ize yazılmaz | UC-114 · UC-19 | eşlendi |
| `03 §8.1.11` | Tarihli yüzde indirim — referans fiyatın altına inmeyen, %100 ya da 0,00 TL'ye inen indirim kaydedilmez ve sebebi söylenir; başlangıç ve bitiş sistemde (Z-20) | UC-114 · konvansiyon — API5-01 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §8.1.12` | Kupon tanımı — yüzde ya da sabit tutar, asgari sepet, tarih aralığı, kullanım adedi; kod tekil, harf ayrımsız; kupon silinmez (K-806) | UC-122 · UC-90 · konvansiyon — API5-01 | eşlendi |
| `03 §8.1.13` | Kuponun kullanım adedi ve kampanyanın durdurulması — adet kullanılmış ve ayrılmış hakların altına inmez; durdurma bitiş tarihini öne çekmektir (K-654) | UC-122 · UC-90 · konvansiyon — API5-01 | eşlendi |
| `03 §8.1.14` | Kategori ağacı — üç seviye (P-20), alfabetik sıra, taşıma sınırları, dolu kategori silinmez ve engelleyenler sayısıyla gösterilir (K-805) | UC-89 · UC-118 · UC-119 · UC-120 · UC-121 | eşlendi |
| `03 §8.1.15` | Toplu yükleme ve sıralama yoktur — CSV ile içe aktarma, toplu fiyat ve stok güncellemesi, ürünlerin elle sırası | uç doğurmaz — yoktur | uç doğurmaz |
| `03 §8.2` metin | Siparişin firma tarafındaki olağan hattı; tipe göre hatlar §1.5'te, adım sayıları §8.5'te | UC-91 · UC-92 | eşlendi |
| `03 §8.2.1` | Yeni siparişin öğrenilmesi — sipariş listesinde, havale "ödeme onayı bekleyen" sayacında; F-1 | UC-91 · UC-86 | eşlendi |
| `03 §8.2.2` | Havale "ödendi" işareti — donmuş IBAN ve tutar gösterilir, gelen tutar sorulmaz; onay geri alınamaz; iptal edilmiş sipariş işaretlenemez, kesintide ertelenen ana kadar işaretlenir (K-663, K-667) | UC-123 · konvansiyon — API8-01 · konvansiyon — API9-03 | eşlendi |
| `03 §8.2.3` | Ödeme onayının işlenmesi — kartta sağlayıcının bildirimiyle, havalede işaretle; kesin düşüm, Z-10 ve Z-39 başlar, dijital teslim, sayaçlar | UC-80 · UC-123 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `03 §8.2.4` | Hazırlık ve fatura sistemin dışında — panel kargoya verme sözünü ve kalan süreyi gösterir; Z-10 aşıldıktan sonraki iptalde kanuni faiz uyarısı; faturanın verisi sipariş dışa aktarmasında | UC-92 · UC-182 · uç doğurmaz — ürünün dışında | eşlendi |
| `03 §8.2.5` | Kargoya verme — şirket ve takip numarası ya da "kendi aracımızla teslim"; onay geri alınamaz; yalnız açık fiziksel kalemler gider; S5 | UC-124 · konvansiyon — API9-03 | eşlendi |
| `03 §8.2.6` | Teslim işareti ve tarihi — ileri ve kargo gününden önceki tarih reddedilir; beyan günüyle çakışmada sıra sorusu; işaretlenene kadar sayaçta; S6 | UC-126 · UC-86 · konvansiyon — API5-01 | eşlendi |
| `03 §8.2.7` | "Teslim edilemedi" işareti — sipariş "kargoya verilecek" sayacına döner; yeniden gönderme ya da iptal; beyanlı kalemin malı teslim almayla; S7, S11 | UC-127 · UC-125 · UC-129 · UC-140 | eşlendi |
| `03 §8.2.8` | Açık fiziksel kalemlerin yeniden gönderilmesi — yeni takip bilgisiyle, onay geri alınamaz (K-677); S8 | UC-125 | eşlendi |
| `03 §8.2.9` | Hizmet kaleminin "tamamlandı" işareti — ödeme onaylanmışken sunulur (K-671); randevu ve takvim yok; kapanış kuralı | UC-139 · konvansiyon — API8-01 | eşlendi |
| `03 §8.2.10` | İndirme hakkının yenilenmesi — başvuru iletişim formundan; sayaç sıfırlanır, müşteri sipariş sayfasından indirir (K-683) | UC-143 · UC-15 · UC-36 | eşlendi |
| `03 §8.2.11` | Siparişin bulunması — numara ya da iletişim e-postasıyla arama, durum süzgeci (K-714) | UC-91 · konvansiyon — API9-02 | eşlendi |
| `03 §8.3` metin | Akış 6'nın iptal, iade ve geri ödeme kolları; müşterinin yüzü §2.7 ve §2.8'de | UC-129 · UC-140 · UC-131 | eşlendi |
| `03 §8.3.1.1` | Firma iptali — kapalı listeden sebep; ödeme öncesi siparişin tamamı, sonrası kalem ve adet (K-794); stok yokluğu ve kanuni faiz uyarıları; "müşteriyle anlaşıldı"da teyit; onay geri alınamaz; havalede IBAN isteği (K-690, K-788) | UC-129 · konvansiyon — API8-01 · konvansiyon — API2-02 · konvansiyon — API9-03 | eşlendi |
| `03 §8.3.1.2` | Müşteri kaynaklı dönen gönderinin iptali — gidiş kargo bedeli ödenmez; ayrım sebep kaydından | UC-129 | eşlendi |
| `03 §8.3.2.1` | İade malının teslim alınması — ulaşma tarihi ve adedi; tarih sınırları; varsayılan adet; kalan adet "iade malı bekleniyor"da; Z-16 ulaşma tarihinden; IBAN silinmişse IBAN isteği (K-712, K-716, K-794, K-808) | UC-140 · konvansiyon — API5-01 · konvansiyon — API9-03 | eşlendi |
| `03 §8.3.2.2` | Stoğa ekleme — teslim alınan adet kadar (K-791); varyant silinmişse sunulmaz; değer kaybı iç nota | UC-141 · konvansiyon — API8-01 · UC-152 | eşlendi |
| `03 §8.3.2.3` | İade reddi — yalnız koşullu istisna kaleminde, teslim alınan adetle; onay geri alınmaz; sonuçlar; firma dönerse tutar bazlı geri ödeme (K-656, K-657) | UC-140 · konvansiyon — API8-01 · konvansiyon — API2-02 | eşlendi |
| `03 §8.3.2.4` | Reddedilen malın geri gönderilmesi — sistemin dışında | uç doğurmaz — ürünün dışında | uç doğurmaz |
| `03 §8.3.2.5` | Açık kalemi kalmamış ve gönderisi dönmüş siparişin kapatılması — sebep ve ikinci geri ödeme yok (K-672, K-717); S9 kapanışı | UC-128 · konvansiyon — API8-01 | eşlendi |
| `03 §8.3.3.1` | Kart hattında geri ödemenin kendiliğinden başlaması — iptal edilen ödenmiş kalem, gecikme feshi, kalem çıkarma, iptal edilmiş siparişe gelen ödeme; onay penceresi yok (K-684, K-691) | uç doğurmaz — sistem işi (`05 §5`) · UC-39 · UC-40 · UC-41 · UC-129 · UC-138 · UC-80 | eşlendi |
| `03 §8.3.3.2` | Havale hattındaki geri ödemenin işlenmesi — kalan süre; IBAN müşterinin girdiği; panel IBAN'ı gösterdikten sonra müşteri düzelttiyse işlem uygulanmaz ve güncel IBAN gösterilir; IBAN son geri ödemeyle silinir (K-1075, K-1110) | UC-131 · UC-35 · konvansiyon — API9-03 | eşlendi |
| `03 §8.3.3.3` | Caymanın geri ödemesi kart hattında — tutar `02 §7.1.7`'yle, kargo ücretinin eklenmesi, kupon hakkı; onay geri alınamaz (K-788, K-789, K-1076) | UC-131 | eşlendi |
| `03 §8.3.3.4` | Kart iadesinin gerçekleşmemesi — sağlayıcı reddederse "geri ödeme gerçekleşmedi", ulaşılamazsa "geri ödeme sonucu bekleniyor" ve sorgu; uyarılar; yöneticinin "gerçekleşmedi" işareti (K-705, K-940) | uç doğurmaz — sistem işi (`05 §5`) · UC-137 · UC-92 · konvansiyon — API8-01 | eşlendi |
| `03 §8.3.3.5` | Kart iadesinin yeniden denenmesi ya da havale yolunun açılması — "sonucu bekleniyor"da sunulmaz; sipariş sayfasında IBAN alanı; IBAN girildikten sonra yeniden deneme yok (K-675, K-940, K-1075) | UC-135 · UC-136 · UC-35 · UC-33 · konvansiyon — API8-01 | eşlendi |
| `03 §8.3.3.6` | IBAN isteği — sipariş sayfasında alan açılır; "IBAN bekleniyor" listesi; yönetici IBAN'ı panelden girmez (K-686, K-716) | UC-134 · UC-35 · UC-86 | eşlendi |
| `03 §8.3.3.7` | Tutar bazlı kısmi geri ödeme — kaleme bağlı ya da bağsız, ödenmiş ve geri ödenmemiş tutarı aşamaz; onay geri alınamaz; "mal dönmedi" kapanışına bağlanma (K-1105) | UC-133 · konvansiyon — API5-01 | eşlendi |
| `03 §8.3.3.8` | "Havale gerçekleşmedi" — işlenmeden önce; IBAN silinir, alan yeniden açılır, liste; işlenmişten sonra dönen havale ürünün dışında (K-1075, K-1108, K-1131) | UC-132 · UC-35 · uç doğurmaz — ürünün dışında | eşlendi |
| `03 §8.3.3.9` | Müşteri IBAN isteğini cevaplamaz — kendiliğinden kapanma yok, kalem listede kalır; iletişim bilgisi görünür | UC-86 · UC-92 · uç doğurmaz — yoktur | eşlendi |
| `03 §8.4` metin | Dokuz müdahale ve ayıp talebinin yönetimi; her müdahale ize yazılır; elle sipariş oluşturma, kalem ekleme ve fiyat değiştirme yoktur | UC-92 · uç doğurmaz — yoktur | eşlendi |
| `03 §8.4.1` | Adres düzeltmesi — teslimat telefonu dahil; teyit; sınır Kargoya verildi; adres geçmişi, iz değeri yazmaz (K-1136); B-10 | UC-146 · konvansiyon — API8-01 | eşlendi |
| `03 §8.4.2` | Kalem adedinin azaltılması ya da kalemin çıkarılması — aralık; kart hattında geri alınamaz onay; geri ödeme iptalin hattından; havalede IBAN isteği; kapanış kuralı (K-690, K-717, K-788) | UC-138 · konvansiyon — API8-01 · konvansiyon — API2-02 | eşlendi |
| `03 §8.4.3` | Kargo şirketi ve takip numarasının düzeltilmesi — kargoya verildikten sonra; B-12 | UC-147 · konvansiyon — API8-01 | eşlendi |
| `03 §8.4.4` | Teslim tarihinin ya da ulaşma tarihinin düzeltilmesi — tarih sınırları; pencereler düzeltilmiş tarihten; sıra sorusu; ulaşma tarihi teslim alma kaydı düzeyinde (K-712, K-794) | UC-148 · UC-149 · konvansiyon — API5-01 · konvansiyon — API8-01 | eşlendi |
| `03 §8.4.5` | Yanlış durum geçişinin düzeltilmesi — kapalı listeden sebep; "geri al" yok, yeni geçiş; havale işaretinde Z-8 yeniden işler (K-685) | UC-150 · konvansiyon — API8-01 | eşlendi |
| `03 §8.4.6` | İç not — müşteriye görünmez; yazılması ize, metni yazılmaz | UC-152 | eşlendi |
| `03 §8.4.7` | "Mal dönmedi" kapanışı — Z-42 geçmiş ve mal ulaşmamışken; ulaşmamış adetlere; geri ödeme yok; IBAN silinir (K-686, K-794) | UC-142 · konvansiyon — API8-01 | eşlendi |
| `03 §8.4.8` | Başka kanaldan cayma ya da gecikme feshi kaydı — kalem ve adet (cayma), bildirimin tarihi; teyit; pencere dışı kayıt ayrıca onaylatılır, bir yıldan sonraya tarihli reddedilir; IBAN aktarımının koşulu; kayıt açılmayan hâller; geri alınamaz onay (K-706, K-709, K-715, K-796) | UC-130 · konvansiyon — API8-01 · konvansiyon — API5-01 · konvansiyon — API2-02 | eşlendi |
| `03 §8.4.9` | Misafir siparişinin iletişim e-postasının düzeltilmesi — yalnız misafir siparişinde ve imhaya kadar; yeni erişim anahtarı, eski bağlantılar açılmaz; "e-posta düzeltildi" işareti (K-1138) | UC-151 · UC-34 · konvansiyon — API8-01 | eşlendi |
| `03 §8.4.10` | Yeni ayıp talebinin okunması — kalem, ayıplı adet, iade adresi; kanıt e-postayla; para gerekiyorsa iade hattı ya da tutar bazlı geri ödeme, havalede IBAN isteği; dijitalde dosya güncellemesi (K-793) | UC-92 · UC-93 · UC-140 · UC-131 · UC-133 · UC-134 · UC-83 | eşlendi |
| `03 §8.4.11` | Ayıp talebinin "çözüldü" işareti ya da yeniden açılması — Z-18 içinde, yeni talep açılmaz (K-676) | UC-144 · UC-145 · konvansiyon — API8-01 | eşlendi |
| `03 §8.5` metin | Manuel adım bütçesinin okunuşu — tavan üç; Aşama 2 ve kısmi adet kararları zorunlu adım eklemez | uç doğurmaz — yazım konvansiyonu | uç doğurmaz |
| `03 §8.5.1` | Kart ile ödenen fiziksel sipariş — iki adım: kargoya verme, teslim işareti | UC-124 · UC-126 | eşlendi |
| `03 §8.5.2` | Havale ile ödenen fiziksel sipariş — üç adım: "ödendi" ve 8.5.1'in ikisi | UC-123 · UC-124 · UC-126 | eşlendi |
| `03 §8.5.3` | Hizmet siparişi — "tamamlandı", havalede ayrıca "ödendi" | UC-139 · UC-123 | eşlendi |
| `03 §8.5.4` | Dijital sipariş — sistem teslim eder, havalede "ödendi" | uç doğurmaz — sistem işi (`05 §5`) · UC-123 | eşlendi |
| `03 §8.5.5` | Karışık sipariş — hatların adımları toplanır, "ödendi" bir kez | UC-123 · UC-124 · UC-126 · UC-139 | eşlendi |
| `03 §8.5.6` | Koşullu adımlar — firma iptali, teslim edilemedi ve yeniden gönderme, teslim alma ve stoğa ekleme, geri ödemeyi elle işleme, ayıbı çözüldü işaretleme, indirme hakkını yenileme (K-794) | UC-129 · UC-127 · UC-125 · UC-140 · UC-141 · UC-131 · UC-133 · UC-144 · UC-143 | eşlendi |
| `03 §8.5.7` | Bütçeye girmeyen işler — müdahaleler, iade reddi, yeniden deneme ve havale yolu, "gerçekleşmedi" işareti, IBAN isteği, "havale gerçekleşmedi", e-postanın yeniden gönderilmesi, üye hesabının silinmesi (K-686, K-690, K-940) | UC-146 · UC-138 · UC-147 · UC-148 · UC-149 · UC-150 · UC-152 · UC-142 · UC-130 · UC-151 · UC-140 · UC-135 · UC-136 · UC-137 · UC-134 · UC-132 · UC-153 · UC-157 | eşlendi |
| `03 §8.5.8` | Sistem dışı adımlar — fatura, reddedilen malın geri gönderilmesi, havale hattında paranın bankadan gönderilmesi | uç doğurmaz — ürünün dışında | uç doğurmaz |
| `03 §8.6` metin | Akış 7'nin panel yüzü ve üç kolu — marka, duyuru, iletişim talebi; kurumsal taraf satış kapısına bağlı değil | UC-96 · UC-98 · UC-93 | eşlendi |
| `03 §8.6.1.1` | İçerik tipinin seçilmesi ve kaydın açılması — Hakkımızda tek kayıt; taslak kayıt adla, başlıkla ya da soruyla; Hakkımızda ve duyuru boş da kaydedilir; galeri, blog ve müşteri görüşü tipi yok | UC-96 · UC-158 · uç doğurmaz — yoktur | eşlendi |
| `03 §8.6.1.2` | İçeriğin girilmesi — metin, görseller, video bağlantısı, ürün bağları; P-27…P-29, P-22, P-23, P-25; gömülü video ve dosya eki yok; metin düzenlemesi ize yazılmaz | UC-158 · UC-117 · konvansiyon — API5-01 | eşlendi |
| `03 §8.6.1.3` | Taslağın "Taslak" bandıyla önizlenmesi — sayfası olmayan kayıtlar göründükleri yerde | UC-09 · UC-10 · UC-11 · UC-12 · UC-13 · UC-14 · konvansiyon — API2-02 | eşlendi |
| `03 §8.6.1.4` | Kaydın yayına alınması — zorunlu alanlar dolmadan yayına alınmaz, panel eksik alanı gösterir; ileri tarihli yayın yok; adres ilk yayında sabitlenir | UC-159 · konvansiyon — API8-01 | eşlendi |
| `03 §8.6.1.5` | Ana sayfada gösterme, menüde gösterme ve listelerin elle sırası | UC-158 · UC-161 · UC-01 | eşlendi |
| `03 §8.6.1.6` | Yayındaki kaydın düzenlenmesi — anında yayında; sürüm geçmişi yok; yayın kapısını bozan düzenleme kaydedilmez; eşzamanlı düzenlemede uyarı | UC-158 · konvansiyon — API2-04 · konvansiyon — API5-01 | eşlendi |
| `03 §8.6.1.7` | Kaydın taslağa alınması ya da silinmesi — geri alınamaz uyarısı; bağlar kalkar, adres serbest kalır; Hakkımızda silinmez | UC-159 · UC-160 · konvansiyon — API8-01 · konvansiyon — API2-02 | eşlendi |
| `03 §8.6.2.1` | İki hazır ana sayfa düzeninden birinin seçilmesi — serbest kurgu yok; ize eski ve yeni değerle | UC-162 · UC-97 | eşlendi |
| `03 §8.6.2.2` | Menüdeki adların değişmesi — iskelet sabit, menü düzenleyici yok | UC-162 | eşlendi |
| `03 §8.6.2.3` | Sosyal medya bağlantıları ve WhatsApp numarası — platform listesi kapalı; sohbet penceresi yok | UC-162 | eşlendi |
| `03 §8.6.2.4` | Marka ayarları — logo, marka adı, site simgesi, marka rengi, platform imzası; marka adı boşaltılamaz; siteye kod eklenmez | UC-163 · UC-117 · UC-98 · uç doğurmaz — yoktur | eşlendi |
| `03 §8.6.3.1` | Duyurunun hazırlanması, önizlenmesi ve yayına alınması — tek duyuru, P-30, isteğe bağlı bağlantı ve tarihler; kimseye gönderilmez | UC-158 · UC-159 | eşlendi |
| `03 §8.6.3.2` | Duyurunun tarih aralığında gösterilmesi (Z-23); taslağa alınarak kaldırılması | konvansiyon — API5-01 · UC-159 | eşlendi |
| `03 §8.6.4.1` | Yeni iletişim talebi panele düşer, "açık talep" sayacında; F-2 | UC-93 · UC-86 | eşlendi |
| `03 §8.6.4.2` | Talebin okunması, cevabın firmanın kendi e-postasıyla — yanıt ekranı, yazışma dizisi ve şablon yok; gereken sipariş işlemi panelin var olan işlemiyle | UC-94 · uç doğurmaz — yoktur · UC-143 · UC-146 · UC-130 · UC-151 · UC-129 | eşlendi |
| `03 §8.6.4.3` | KVKK talebinin işlenmesi — iletişim talebinin iki durumu, süre sayacı yok (Z-25); cevabın dayanağı sipariş listesi ve üye kaydı görünümü; hesabın silinmesi §9.4.6 | UC-94 · UC-91 · UC-95 · UC-157 | eşlendi |
| `03 §8.6.4.4` | Talebin kapatılması ya da yeniden açılması — saklama süresi son kapatılıştan (Z-33; K-707, K-951) | UC-155 · UC-156 | eşlendi |
| `03 §8.7` metin | Mağaza ayarları panelden; kurulumun dış ön koşulları `10 §4.1`'in kontrol listesinde ve panelden değişmez; panel ayarı ize eski ve yeni değerle; donan değerler yalnız yeni siparişlere | UC-99 · UC-100 · UC-101 · UC-102 · uç doğurmaz — kurulum | eşlendi |
| `03 §8.7.1.1` | Panele ilk giriş — ana sayfanın kurulum kontrol listesi satış kapısının eksik koşullarını adıyla sayar (ÖK-8…ÖK-11) | UC-86 | eşlendi |
| `03 §8.7.1.2` | Satış kapısının sürekli denetimi — dört koşul; düşerse sepete ekleme ve ödeme kapanır; veri toplayan girişlerin kapısı (K-850); ölçüm penceresinin başlangıcı | konvansiyon — API8-01 · UC-05 · UC-27 · UC-15 · UC-48 · UC-50 · UC-86 | eşlendi |
| `03 §8.7.2.1` | Firma tipi ve kimlik alanları — zorunlu alanlar boşaltılamaz; ETBİS hatırlatması ve doğrulama bandı; kimlik siparişe donar, üretilen metinlerin sürümü artar | UC-164 · UC-99 · konvansiyon — API5-01 | eşlendi |
| `03 §8.7.2.2` | Aydınlatma metni ve çerez politikası — taslaktan düzenleme ve yayın; tamamlanma koşulu; sürüm kendiliğinden artar, bildirim yok | UC-173 · UC-174 · UC-102 | eşlendi |
| `03 §8.7.2.3` | Ön Bilgilendirme Formu ve sözleşmenin ayarlardan üretilmesi — besleyen ayar değişince sürüm artar ve açık onay adımı durur; eski sürümler silinmez | UC-164 · UC-168 · UC-169 · UC-170 · UC-172 · UC-27 | eşlendi |
| `03 §8.7.3.1` | Havale/EFT'nin açılması ve IBAN — kartın panelde anahtarı yok; IBAN'ın ilk girişi de F-5 | UC-167 · UC-100 | eşlendi |
| `03 §8.7.3.2` | Havale IBAN'ının değişmesi — onay bekleyen havale siparişlerinin sayısını söyler; yeni IBAN yalnız yeni siparişlere; gerekirse firma iptali (K-663) | UC-167 · konvansiyon — API2-02 · UC-129 | eşlendi |
| `03 §8.7.3.3` | Havalenin kapatılması — yeni siparişte görünmez; açık siparişler etkilenmez; başka yöntem yoksa satış kapanır (K-664) | UC-167 · konvansiyon — API8-01 | eşlendi |
| `03 §8.7.4.1` | Kargo ücreti, ücretsiz kargo eşiği ve asgari sipariş tutarı (P-1, P-3, P-4) — değerler siparişe donar, kargo ücreti metinlerin sürümünü artırır | UC-168 · UC-101 | eşlendi |
| `03 §8.7.4.2` | Teslimat illeri — varsayılan tüm Türkiye; değişiklik metinlerin sürümünü artırır | UC-169 | eşlendi |
| `03 §8.7.4.3` | Kargoya verme süresinin varsayılanı (P-5) ve havale ödeme süresi (P-7) — çitlerin dışındaki değer reddedilir | UC-170 · konvansiyon — API5-01 | eşlendi |
| `03 §8.7.4.4` | İndirme hakkı (P-8) ve KDV oranının varsayılanı (P-10) — yalnız yeni siparişlere (K-713); ürün formundaki ayarlar 8.1.4'te | UC-171 · UC-114 | eşlendi |
| `03 §8.7.4.5` | İade adresi — zorunlu ve satış kapısının koşulu; değişiklikte "iade malı bekleniyor" kalemlerin sayısı; yapılmış beyanların adresi değişmez (K-665) | UC-172 · konvansiyon — API2-02 | eşlendi |
| `03 §8.7.5.1` | Satışın geçici kapatılması — sepete ekleme ve ödeme kapanır, sepet korunur, açık siparişler yürür; bakım modu yok (K-664) | UC-165 · konvansiyon — API8-01 | eşlendi |
| `03 §8.7.5.2` | Satışın yeniden açılması — kapının öteki üç koşulu sağlanıyorsa | UC-166 · konvansiyon — API8-01 | eşlendi |
| `03 §8.8` metin | Yönetici hesaplarının okunuşu — ilk yönetici kurulumda, sonrakiler davetle; aynı yetki, yetki matrisi yok; ayrı hesap türü, sepeti ve siparişi olmaz; giriş kuralları müşteriyle aynı; erişimin kaybında §10.4 | konvansiyon — API0-07 · UC-103 · uç doğurmaz — kurulum | eşlendi |
| `03 §8.8.1` | Yönetici daveti — yönetici hesabının adresine gönderilemez; yeni davet öncekini geçersiz kılar (Z-3); listede satır; e-posta ulaşmazsa işaret ve yol yeni davet; F-6 (K-660, K-661, K-692) | UC-175 · UC-103 · konvansiyon — API5-01 | eşlendi |
| `03 §8.8.2` | Davetlinin bağlantıdan girip adını ve şifresini kurması — geçerliyse hesap doğar; kendi kendine kayıt ve panelde Google yok; geçersiz bağlantıda süresi dolmuş davetin mesajı (K-694, K-736) | UC-85 · UC-113 · uç doğurmaz — yoktur | eşlendi |
| `03 §8.8.3` | Kullanılmamış davetin geri çekilmesi — bağlantı o anda geçersiz (K-660) | UC-176 | eşlendi |
| `03 §8.8.4` | Süresi dolan davet — Z-3 dolunca bağlantı çalışmaz, yol yeni davet | UC-85 · UC-113 · UC-175 | eşlendi |
| `03 §8.8.5` | Başka bir yöneticinin kaldırılması — son yönetici ve kendi hesabı kaldırılamaz; onay geri alınamaz, davetleri düşer; oturumlar sonlanır; ize ve imha kaydına (K-662, K-692, K-1064) | UC-177 · konvansiyon — API8-01 · konvansiyon — API2-02 | eşlendi |
| `03 §8.8.6` | Yanlış adrese giden davet — kullanılmamışsa geri çekilir, kullanılmışsa hesap kaldırılır, işlem izi okunur (K-660, K-692, K-694) | UC-103 · UC-176 · UC-177 · UC-106 | eşlendi |
| `03 §8.9` metin | Panelin ana sayfası, raporları ve toplu veri işlemleri; bekleyen işler e-postayla hatırlatılmaz | UC-86 · UC-105 · UC-106 · UC-107 | eşlendi |
| `03 §8.9.1` | Bekleyen işler — altı sayaç ve süzülmüş listeleri; "IBAN bekleniyor" listesi ayrı; kanal ve "firma bildirimleri ulaşmıyor" uyarısı; "e-posta ulaşmadı" süzgecinin satırı; sayaçlar saklanmaz (K-760, K-761, K-940) | UC-86 · UC-91 · konvansiyon — API9-02 | eşlendi |
| `03 §8.9.2` | Ulaşmayan e-postanın sipariş ayrıntısından yeniden gönderilmesi — tek tek; B-1…B-16; donmuş sürüm; başarılı gönderimle işaret kalkar (K-759, K-761, K-1115) | UC-153 · UC-154 · UC-92 | eşlendi |
| `03 §8.9.3` | Satış özeti — seçilen dönem; sayılar ve dört ölçü; analitik değil, saklanmaz ve dışa aktarılmaz | UC-105 · konvansiyon — API9-05 | eşlendi |
| `03 §8.9.4` | İşlem izi — tarih aralığı ve yönetici süzgeci; arama, gruplama ve dışa aktarma yok; Z-30 boyunca değiştirilemez (K-687) | UC-106 · konvansiyon — API9-02 | eşlendi |
| `03 §8.9.5` | Siparişlerin tarih aralığıyla CSV dışa aktarması — fatura verisi, ihlalde ulaşma bilgisi, geç gelen ödeme; ize yazılır | UC-182 · UC-107 · konvansiyon — API9-05 | eşlendi |
| `03 §8.9.6` | Üye listesinin ve iletişim taleplerinin dışa aktarması — ad ve e-posta CSV; ize yazılır | UC-183 · UC-184 · konvansiyon — API9-05 | eşlendi |
| `03 §8.9.7` | Katalog, içerik, işlem izi ve satış özetinin dışa aktarması yoktur — çıkış hakkı barındırma düzleminde | uç doğurmaz — yoktur | uç doğurmaz |
| `03 §9` metin | Destekleyici akışların okunuşu: üyelik akışı; üyelik siparişin ön koşulu değildir; kimlik doğrulama kuralları yönetici hesabına da aynı değerlerle işler | konvansiyon — API2-03 · konvansiyon — API0-07 | konvansiyon |
| `03 §9` m1 | Biçim ve aktör — Ziyaretçi, Üye, bağlantıyı açan Kullanıcı; hesap durum makinesi taşımaz, geçerliliği §1.10.5'te, oturum §1.10.6'da | konvansiyon — API0-07 | konvansiyon |
| `03 §9` m2 | Bildirim — hesap ve yönetim e-postaları matrisin dışında; ulaşmazsa işaret düşmez, kullanıcı bağlantıyı ekrandan yeniden ister | uç doğurmaz — e-posta (`08`) · UC-49 · UC-67 | eşlendi |
| `03 §9` m3 | Dallar — hatalar §3.4'te, limitler §6.1'de, süreler §4.1'de, ele geçirme §6.2.5'te | konvansiyon — API2-07 · konvansiyon — API2-08 | konvansiyon |
| `03 §9.1.1` | Kayıt ekranı — veri toplayan girişlerin kapısı kapalıysa kayıt ve yeni hesap açan Google girişi kapalı (K-850); aydınlatma metninin bağlantısı; pazarlama onayı yok; Google düğmesi kurulumda tanımlıysa | UC-47 · UC-50 · UC-16 · konvansiyon — API8-01 | eşlendi |
| `03 §9.1.2` | Kayıt — ad, e-posta, şifre (P-17, yaygın şifre listesi); "bu e-posta zaten kayıtlı"; L-3; doğrulanmamış kayıt açılır (K-696) | UC-48 · konvansiyon — API5-01 · konvansiyon — API2-08 | eşlendi |
| `03 §9.1.3` | Doğrulama bağlantısının yeniden istenmesi — öncekileri geçersiz kılar, ömrü uzatmaz; L-9, nötr mesaj (K-658, K-659) | UC-49 · konvansiyon — API2-08 | eşlendi |
| `03 §9.1.4` | Doğrulama — bağlantının açılması doğrulamaz, ekrandaki düğme doğrular (K-904); geçmiş misafir siparişleri hesaba düşer | UC-52 · UC-53 · UC-74 | eşlendi |
| `03 §9.1.5` | Z-1 içinde doğrulanmayan kayıt silinir; süresi dolmuş bağlantı "bağlantı geçersiz" der (K-904) | uç doğurmaz — sistem işi (`05 §5`) · UC-52 · konvansiyon — API2-07 | eşlendi |
| `03 §9.1.6` | Google ile ilk giriş — kapı açıksa hesap doğrulanmış ve şifresiz doğar; bekleyen kayıt devralınmaz, silinir (K-696, K-708); kapı kapalıysa giriş yok | UC-50 · UC-51 · konvansiyon — API8-01 | eşlendi |
| `03 §9.1.7` | Google e-postayı doğrulanmamış verir — hesap açılmaz, bağlama yok; kayda yönlendirme (K-697) | UC-51 · UC-47 | eşlendi |
| `03 §9.2.1` | Müşteri girişi — yalnız müşteri hesapları; "beni hatırla" (Z-4, Z-5); L-1; giriş kaydı; tanınan tarayıcı işareti (Z-46); sepet birleşmesi | UC-54 · UC-55 · konvansiyon — API2-03 · konvansiyon — API2-08 | eşlendi |
| `03 §9.2.2` | Google ile giriş mevcut hesaba — iki giriş yolu; L-1'e girmez; bağ üyece kaldırılamaz (K-733, K-1117) | UC-50 · UC-51 · uç doğurmaz — yoktur | eşlendi |
| `03 §9.2.3` | Çıkış ya da oturumun kendiliğinden kapanması — sepet hesapta kalır | UC-77 · konvansiyon — API2-03 | eşlendi |
| `03 §9.2.4` | "Şifremi unuttum" — nötr metin; L-2; bağlantı tek kullanımlık (Z-2); şifresiz hesapta "şifre belirle" | UC-56 · UC-57 · konvansiyon — API2-08 | eşlendi |
| `03 §9.2.5` | Bağlantıdan yeni şifrenin kurulması — oturumlar kapanır, sıfırlayan tarayıcı tanınır | UC-58 · UC-59 | eşlendi |
| `03 §9.2.6` | Yeniden doğrulama isteyen işlemler — şifre değiştirme, e-posta değiştirme, hesap silme; şifre ya da Google; L-1 (K-711) | UC-60 · UC-61 · UC-62 · UC-65 · UC-66 · UC-68 · konvansiyon — API2-05 · konvansiyon — API2-08 | eşlendi |
| `03 §9.2.7` | Panel girişi — yalnız yönetici hesapları, şifreyle; panelde Google ve kendi kendine kayıt yok; aynı oturum ve limit kuralları | UC-84 · UC-110 · konvansiyon — API2-03 · uç doğurmaz — yoktur | eşlendi |
| `03 §9.2.8` | Panel şifre sıfırlaması — yalnız yönetici hesaplarında; hiçbir yönetici giremiyorsa §10.4.4 | UC-84 · UC-111 · UC-112 | eşlendi |
| `03 §9.3.1` | Adın değişmesi — yeniden doğrulama istemez; geçmiş siparişe dokunmaz | UC-63 · UC-64 | eşlendi |
| `03 §9.3.2` | Adres defteri — ekleme, adlandırma, düzenleme, silme; ödeme adımında yazılan adresin deftere kaydı (K-680) | UC-70 · UC-71 · UC-72 · UC-73 · UC-27 | eşlendi |
| `03 §9.3.3` | Şifre değiştirme — yeniden doğrulama; öteki oturumlar kapanır, öteki tarayıcıların işaretleri düşer (K-695) | UC-65 · UC-61 | eşlendi |
| `03 §9.3.4` | E-posta değişikliği isteği — yeniden doğrulama; tekillik (K-1134); geri alma bağlantısı açıkken değiştirilemez; bekleyen değişiklik hesap ekranında; L-9; yeni istek bekleyenin yerine geçer, "vazgeç" yok (K-734) | UC-66 · UC-67 · UC-63 · konvansiyon — API5-01 · konvansiyon — API8-01 | eşlendi |
| `03 §9.3.5` | Yeni adresin doğrulanması — düğmeyle (K-904); bağlama kuralı; Google bağı kalkar (K-1117); eski adrese geri alma bağlantısı; adres başka hesabın olmuşsa doğrulanmaz | UC-52 · UC-53 | eşlendi |
| `03 §9.3.6` | Geri alma — eski adresteki bağlantı ve ekrandaki düğme (K-839); oturumlar kapanır, Google bağı ve şifre düşer | UC-75 · UC-76 | eşlendi |
| `03 §9.3.7` | Yöneticinin kendi e-postasını değiştirmesi — 9.3.4–9.3.6'nın kuralları; yönetici hesapları içinde tekil; ize yazılır; ad değişikliği (K-736) | UC-104 · UC-178 · UC-179 · UC-180 · UC-108 · UC-185 · UC-186 · UC-187 | eşlendi |
| `03 §9.4.1` | Hesap silme isteği — yeniden doğrulama; geri alma bağlantısı açıkken silinmez, ekran bitiş tarihini söyler (K-698) | UC-68 · UC-63 · konvansiyon — API8-01 | eşlendi |
| `03 §9.4.2` | Hesabın silinmesi — ad, giriş bilgileri, oturumlar, adres defteri ve sepet hemen silinir; imha kaydı; siparişler kendi kaydıyla durur (K-693, K-708) | UC-68 · UC-69 · UC-157 | eşlendi |
| `03 §9.4.3` | Silinmiş hesabın açık siparişi — misafir yolundan ya da e-postadaki bağlantıyla | UC-31 · UC-32 · UC-34 · UC-33 | eşlendi |
| `03 §9.4.4` | Kişisel veri başvurusu — iletişim formunun "KVKK talebi" tipi; ayrı KVKK modülü, süre sayacı ve veri indirme yok | UC-14 · UC-15 · uç doğurmaz — yoktur | eşlendi |
| `03 §9.4.5` | Üye kaydı görünümünde e-postayla arama — salt okunur; siparişler sipariş listesinden | UC-95 · UC-91 | eşlendi |
| `03 §9.4.6` | Yöneticinin talep üzerine hesabı silmesi — geri alınamaz onay (K-715); geri alma bağlantısı açıkken silinmez (K-698); ize işlem olarak | UC-157 · konvansiyon — API8-01 · konvansiyon — API2-02 | eşlendi |
| `03 §9.4.7` | Aynı e-postayla yeniden kayıt — yeni hesap, eski siparişler bağlanmaz | UC-48 | eşlendi |
| `03 §9.5` metin | Tercih yönetiminin ürünün içinde karşılığı yoktur; her tercih gerekçesiyle "yoktur" | uç doğurmaz — yoktur | uç doğurmaz |
| `03 §9.5.1` | Bildirim tercihi yoktur — işlem bildirimleri kapatılamaz | uç doğurmaz — yoktur | uç doğurmaz |
| `03 §9.5.2` | Kampanya ve bülten onayı yoktur — pazarlama iletisi gönderilmez | uç doğurmaz — yoktur | uç doğurmaz |
| `03 §9.5.3` | Dil seçimi yoktur — tek dil Türkçe | uç doğurmaz — yoktur · konvansiyon — API2-01 | konvansiyon |
| `03 §9.5.4` | Çerez tercihi yoktur — yalnız zorunlu çerezler, onay bandı yok | uç doğurmaz — yoktur | uç doğurmaz |
| `03 §9.5.5` | Hesaptan veri indirme yoktur — yol kişisel veri başvurusu | uç doğurmaz — yoktur | uç doğurmaz |
| `03 §9.5.6` | Firma bildirimlerinin adresi ve tercihi yoktur — firma kimliğindeki iletişim e-postası; F-5 ve F-6 her yöneticinin kendi adresine | uç doğurmaz — yoktur | uç doğurmaz |
| `03 §10` metin | Operasyonel akışların okunuşu: dış servisler, satışın durdurulması, sitenin kesintisi, panele erişimin kaybı; çalışma süresi taahhüdü yok, barındırma kurulum tarafında; sürelerin kaderi §4'te | uç doğurmaz — kurulum | uç doğurmaz |
| `03 §10` m1 | Biçim — adım tablosu; hataların müşteri yüzü §3'te | uç doğurmaz — yazım konvansiyonu | uç doğurmaz |
| `03 §10` m2 | Kullanıcı bilgilendirmesi — ürün kesinti bildirimi göndermez; firmanın yolu duyuru şeridi; kesintinin tespiti `08`'in ve `05`'in | uç doğurmaz — yoktur · UC-159 | eşlendi |
| `03 §10.1.1.1` | Sağlayıcıya erişilemez — kart "şu an kullanılamıyor", seçilemez; havale yoksa sepete dönüş, sepet korunur | UC-22 · UC-19 · konvansiyon — API8-01 | eşlendi |
| `03 §10.1.1.2` | Onaydan sonra sağlayıcıya bağlanılamaz — kendiliğinden iptal, L-7'ye sayılmaz, sepet korunur | UC-29 · UC-19 · konvansiyon — API2-07 | eşlendi |
| `03 §10.1.1.3` | Z-7 dolar, son sorgu yanıtsız — sipariş Bekliyor'da kalır, "ödeme sonucu alınamadı" işareti, sorgu yinelenir (K-683) | uç doğurmaz — sistem işi (`05 §5`) · UC-92 | eşlendi |
| `03 §10.1.1.4` | Sağlayıcı döner — sorgunun sonucu onay ya da iptal; kart yeniden görünür; iptal edilmiş siparişe gelen ödemenin kendiliğinden iadesi (K-675, K-684) | uç doğurmaz — sistem işi (`05 §5`) · UC-80 · UC-22 | eşlendi |
| `03 §10.1.1.5` | Kesintide kart iadesi gerçekleşmez ya da sonucu bilinmez — "sonucu bekleniyor", sorgu; yöneticinin "gerçekleşmedi" işareti; sonra yeniden deneme ya da havale yolu (K-705, K-940) | uç doğurmaz — sistem işi (`05 §5`) · UC-137 · UC-135 · UC-136 | eşlendi |
| `03 §10.1.1.6` | Kesintide satışı sürdürmek ya da durdurmak — havaleyle sürer; geçici kapatma; duyuru şeridi | UC-165 · UC-158 · UC-159 | eşlendi |
| `03 §10.1.1.7` | Sağlayıcının anahtarlarının ya da sağlayıcının değişmesi — panel işlemi değil; `08` ve `DEPLOY_RUNBOOK` | uç doğurmaz — kurulum | uç doğurmaz |
| `03 §10.1.2.1` | E-posta gönderilemez — Z-26'nın üç denemesi; akışlar etkilenmez, sipariş sayfası her bilgiyi taşır | uç doğurmaz — sistem işi (`05 §5`) · UC-33 | eşlendi |
| `03 §10.1.2.2` | Üç deneme başarısız — siparişin ve davetin satırına işaret; firma bildirimlerinde ana sayfa uyarısı; hesap e-postalarında bağlantı ekrandan yeniden istenir (K-760) | UC-92 · UC-103 · UC-86 · UC-49 · UC-57 | eşlendi |
| `03 §10.1.2.3` | Art arda başarısız gönderim — ana sayfada kanal uyarısı; eşiği `05`'in; satışı durdurmanın yolu geçici kapatma | UC-86 · UC-165 | eşlendi |
| `03 §10.1.2.4` | Kesintide sipariş ve izleme — e-postaya bağlı değil; üye hesabından, misafir numara ve e-postayla | UC-74 · UC-31 · UC-32 · UC-33 | eşlendi |
| `03 §10.1.2.5` | Kesintide hesap bağlantısı ya da davet — bağlantı ulaşmadan işlem tamamlanmaz; dönüşte yeniden istenir (L-9, L-2); ulaşmayan davetin yolu yeni davet | UC-49 · UC-57 · UC-67 · UC-175 | eşlendi |
| `03 §10.1.2.6` | Altyapı döner — ürün kendiliğinden yeniden göndermez; işaretli siparişler süzülmüş listeden bulunur ve yeniden gönderilir (K-675, K-761, K-1115) | UC-86 · UC-91 · UC-153 · UC-154 · konvansiyon — API9-02 | eşlendi |
| `03 §10.1.3.1` | Google'a ulaşılamaz — Google girişi tamamlanmaz; şifreli giriş ve girişsiz sipariş açık | UC-50 · UC-55 · UC-22 | eşlendi |
| `03 §10.1.3.2` | Şifresiz hesabın sahibi Google'a ulaşamaz — "şifremi unuttum" ile şifre belirler; o zamana kadar yeniden doğrulama isteyen işlemler yapılamaz | UC-57 · UC-58 · UC-59 · konvansiyon — API8-01 | eşlendi |
| `03 §10.1.3.3` | Google uygulaması kurulumda tanımlı değil — düğme görünmez; panelde açık/kapalı anahtarı yok | UC-47 · UC-54 · konvansiyon — API8-01 · uç doğurmaz — kurulum | eşlendi |
| `03 §10.1.3.4` | Google kesintisinde panel etkilenmez — panelde Google ile giriş yok | UC-110 · uç doğurmaz — yoktur | eşlendi |
| `03 §10.2.1` | Bir süre sipariş almamak — satışın geçici kapatılması; bakım modu yok | UC-165 · uç doğurmaz — yoktur | eşlendi |
| `03 §10.2.2` | Ziyaretçiyi bilgilendirmek — duyuru şeridi, tarih aralığıyla (Z-23); e-postayla gönderilmez | UC-158 · UC-159 | eşlendi |
| `03 §10.2.3` | Kurulum tarafının güncellemesi — planlı bakım penceresi yok; panelin işi değil | uç doğurmaz — kurulum | uç doğurmaz |
| `03 §10.2.4` | Yedekleme ve yedekten dönüş — ürünün akışı değil; veri kaybı toleransı ve kalan risk; yöntem `05` ve `DEPLOY_RUNBOOK` (K-888, K-939, K-983) | uç doğurmaz — kurulum | uç doğurmaz |
| `03 §10.3` metin | Sitenin kesintisinde sürelerin ve kendiliğinden işlerin kuralı §4.2.11 ve §4.2.12'de | uç doğurmaz — sistem işi (`05 §5`) | uç doğurmaz |
| `03 §10.3.1` | Siteye ulaşılamaz — ürün çalışmaz, içinden bilgilendirme yok | uç doğurmaz — kurulum · uç doğurmaz — yoktur | uç doğurmaz |
| `03 §10.3.2` | Kesinti sürer — süreler dondurulmaz; caymanın ve feshin başka kanaldan bildirimi açık, tarih ulaşma tarihi (K-666, K-706) | uç doğurmaz — sistem işi (`05 §5`) · UC-130 | eşlendi |
| `03 §10.3.3` | Site döner — kaçırılan kendiliğinden işler sırayla çalışır | uç doğurmaz — sistem işi (`05 §5`) | uç doğurmaz |
| `03 §10.3.4` | Havale süresi kesintide dolmuş — iptal ertelenir, sipariş sayfası yeni son günü gösterir; o ana kadar "ödendi" işaretlenebilir (K-667) | uç doğurmaz — sistem işi (`05 §5`) · UC-33 · UC-123 | eşlendi |
| `03 §10.3.5` | Dönüşte panel — sayaçlar güncel hâli gösterir; kesintide gelen cayma ulaşma tarihiyle kaydedilir | UC-86 · UC-130 | eşlendi |
| `03 §10.3.6` | Kesintiyi duyurmak — ürün bildirim göndermez; firmanın yolu duyuru şeridi | UC-158 · UC-159 · uç doğurmaz — yoktur | eşlendi |
| `03 §10.4` metin | Panele erişimin kaybı — platform operatörü ve operatör paneli yok; kurtarma ürünün dışında, kurulum tarafının işi (K-699) | uç doğurmaz — yoktur · uç doğurmaz — kurulum | uç doğurmaz |
| `03 §10.4.1` | Yöneticinin şifresini unutması — panelin şifre sıfırlaması | UC-84 · UC-111 · UC-112 | eşlendi |
| `03 §10.4.2` | Yönetici L-1'in engeline takılır — tanınan tarayıcıdan girer ya da sıfırlama bağlantısını kullanır; engel kendiliğinden açılır | UC-110 · UC-112 · konvansiyon — API2-08 | eşlendi |
| `03 §10.4.3` | Bir yönetici erişimini kaybeder, başka yönetici vardır — yeni adrese davet ve eski hesabın kaldırılması (K-692) | UC-175 · UC-177 | eşlendi |
| `03 §10.4.4` | Hiçbir yönetici giremez — kurulum tarafı yeni yönetici hesabı açar; ürünün içinde kurtarma yolu yok; işlem izine yazılmaz (K-699, K-702) | uç doğurmaz — kurulum · uç doğurmaz — yoktur | uç doğurmaz |
| `03 §10.4.5` | Ele geçirilmiş tek yönetici — kurulum tarafı yeni hesap açar, oturumları sonlandırır ve şifreyi geçersiz kılar (K-699) | uç doğurmaz — kurulum | uç doğurmaz |
| `03 §10.4.6` | Yeni yönetici paneli geri alır — hesabı kaldırır, IBAN'ı denetler ve düzeltir, yetkisiz IBAN'lı siparişleri iptal eder, davetleri geri çeker ve yeniden davet eder, işlem izini okur (K-663, K-692, K-699) | UC-177 · UC-167 · UC-129 · UC-176 · UC-175 · UC-106 | eşlendi |

#### 1.1.11 Devirler — karar satırları, park maddeleri, dizin satırları, gövde cümleleri ve başka dokümanın park satırı (90)

Kaynağın sırasıyla: etki sütununda `07` taşıyan K-479…K-1143'ün 51 karar satırı numara sırasıyla (`dev07()`), `07`'nin beş park bloğunun 14 maddesi ve 4 dizin satırı bloğun sırasıyla (`park07()`), önceki dokümanların gövdesinde `07`'yi ya da "API Tasarım"ı anan 20 cümle (`gov07()` — bugünkü metinden) ve Entegrasyon Spesifikasyonu'nun `07`'ye karar bırakan park satırı (`parkx07()`); kimlik biçimi K-1151'in (4). maddesidir (§1.1). Eşlemenin okunuşu K-1159'dadır: **devir satırı devrin indiği aday uca ve devrin karara bağlanacağı konuya** (`konvansiyon — APIn-mm`) eşlenir — karar kaydının §16.2'sindeki konu düzeyindeki eşleme ve §16.3'ün Kaynak satırları başlangıç noktasıdır; kararın kendisi yalnız adlara ya da `07`'nin yazımına dokunuyorsa satır yalnız konuyu taşır (API0-06, API5-02). Dizin satırı gösterdiği devri ikinci kez kaydetmez — yalnız adını kendisi taşıyan karar satırsız işin konusunu taşır; Aşama 2'nin dizini (tek atfı K-701'in kendi satırındadır) *uç doğurmaz — yazım konvansiyonu*'dur. Etki sütununda `07` yalnız numarasıyla geçen satırın özeti *(yalnız `07`)* ile başlar ve işi karar hücresinden çıkarır; park maddesiyle karşılanan devir yine satır olarak girer, özeti kuralı yeniden tarif etmez, "→ park An-m" ile maddeyi gösterir (K-1003'ün (1). ve (4). maddesinin kalıbı). Aşama 1'in 72 workshop satırı bu alt bölüme girmez — kural ailesiyle karşılanır (karar kaydı §16.2, betik 2).

| Kaynak ID | Kaynak özeti | Uç | Durum |
|---|---|---|---|
| K-526 | (yalnız `07`) Tek başına "müşteri" sipariş veren alıcıdır (`Buyer`), aktör değildir; `Customer` yalnız üye müşteri — "müşteri iptal eder" kuralı yalnız üyeye bağlanmaz; uç ve alan adları sözlükten | konvansiyon — API0-06 · konvansiyon — API0-07 | konvansiyon |
| K-562 | (yalnız `07`) Sipariş e-postasındaki bağlantı numaradan ayrı, tahmin edilemez bir erişim anahtarı taşır; numara tek başına sipariş sayfasını açmaz, numarayla açılan her yol e-postayı da sorar; bağlantının ömrü yok — kişisel veri imhasına kadar | UC-34 · UC-32 · UC-33 · konvansiyon — API2-01 · konvansiyon — API4-03 | eşlendi |
| K-568 | (yalnız `07`) Kalıcı silinen kaydın adresi serbest kalır, aynı adla doğan kayıt onu alır; taslak ve arşivdeki kaydın adresi dolu; silinmiş ve yeniden verilmemiş adres "sayfa bulunamadı" | UC-04 · UC-06 · konvansiyon — API2-01 | eşlendi |
| K-579 | (yalnız `07`) L-7 ve L-8'in e-posta ekseni yalnız giriş yapmış üyenin siparişlerini hesabın e-postasıyla sayar; misafir siparişi yalnız IP ekseninde; engele takılana kurtarma yolu yok | UC-27 · konvansiyon — API2-08 | eşlendi |
| K-588 | (yalnız `07`) Yönetici ve müşteri hesabı ayrı türler, ayrı kapılardan girilir; e-posta tür içinde tekil; vitrinin kayıt, giriş, sıfırlama ve Google girişi yalnız müşteri hesabında; panelde Google girişi yok | UC-55 · UC-50 · UC-51 · UC-110 · UC-111 · UC-113 · konvansiyon — API2-03 · konvansiyon — API4-05 · konvansiyon — API9-06 | eşlendi |
| K-701 | (yalnız `07`) Atıf dizisinde önsüz `§` dizinin dokümanını gösterir; dizi ` · `, `;`, parantezin kapanışı ya da cümle sonuyla biter | konvansiyon — API0-06 | konvansiyon |
| K-737 | Panelin ürün listesi ürün adıyla aranır ve yayın durumuyla süzülür; arama ve süzme parametrelerinin biçimi `07`'nin — → park A3-2 | UC-87 · konvansiyon — API2-06 · konvansiyon — API9-02 | eşlendi |
| K-787 | Müşterinin iptal, cayma ve iadesi kalem başına adetle; para işleme giren adet kadar döner; `07`: müşteri işlemlerinin adet parametresi — → park A3-1 | UC-40 · UC-43 · UC-45 · konvansiyon — API4-04 | eşlendi |
| K-794 | Firma iptali, iade teslim alma, iade reddi ve "mal dönmedi" kapanışı adetle; `07`: yöneticinin kalem işlemlerinin adet parametresi — → park A3-1 | UC-129 · UC-138 · UC-140 · UC-141 · UC-142 · konvansiyon — API9-03 | eşlendi |
| K-795 | Adet alanı OB-12'nin varyantı; üst sınır kalemin o işleme açık adedi, varsayılan iptalde ve caymada açık adedin tamamı, ayıpta bir; `07`: ekranın açık adedi ve varsayılanı göstermesi için verinin sunulması — → park A3-1 | UC-38 · UC-42 · UC-44 · UC-92 · konvansiyon — API4-04 · konvansiyon — API9-03 | eşlendi |
| K-802 | Sipariş listesinin sırası, araması (numara ya da e-posta, tam eşleşme) ve süzgeçleri (sipariş durumu, ödeme durumu — çoklu; bekleyen iş ve işaret — tekli); `07`: parametrelerin biçimi — → park A3-3 | UC-91 · konvansiyon — API2-06 · konvansiyon — API9-02 | eşlendi |
| K-830 | Ayıplı adedin üst sınırı kalemin talebe açık adedidir, teslim işaretine bağlı değildir; `07`'nin K-793 park satırı değişmedi — "açık adet" okunuşu — → park A3-1 | UC-44 · UC-45 · konvansiyon — API4-04 | eşlendi |
| K-868 | Sayfalar sunucuda Razor Pages ile üretilir, gönder–yönlendir–göster; ayrı JSON API katmanı yok; makine uçları — bildirim, parçalı yükleme, site haritası; `07`: sözleşmelerin biçimi — → park A4-1 | UC-79 · UC-80 · UC-83 · konvansiyon — API0-04 · konvansiyon — API2-02 · konvansiyon — API2-10 · konvansiyon — API4-09 | eşlendi |
| K-873 | İyimser eşzamanlılık: düzenleme formu kaydın açıldığı andaki sürümü taşır; sürüm değişmişse "Güncel hâli aç" ve "Yine de kaydet"; bölümleri ayrı kaydedilen ekranda bölüm sürümü; `07`: düzenleme formlarının sürümü taşıması — → park A4-2 | UC-114 · UC-122 · UC-158 · UC-163 · UC-164 · UC-173 · konvansiyon — API2-04 · konvansiyon — API9-04 | eşlendi |
| K-874 | Sipariş sürüm taşır, işlem satır kilidiyle uygulanır; yöneticinin işlemi gördüğü sürümü taşır, değişmişse uygulanmaz ve güncel hâl gösterilir; müşterinin işlemi o anki hâle karşı denetlenir; `07`: sipariş işlemlerinin sürümü ve "sipariş değişti" yanıtı — → park A4-2 | UC-92 · UC-123 · UC-35 · UC-39 · konvansiyon — API9-03 · konvansiyon — API2-04 · konvansiyon — API8-01 | eşlendi |
| K-883 | Dijital dosyanın yüklemesi parçalı, kesilen yükleme kaldığı yerden sürer; tamamlanmayan yükleme bir gün sonra silinir; her yükleme yeni kayıt; `07`: parçalı yükleme ucu — → park A4-3 | UC-83 · konvansiyon — API4-07 | eşlendi |
| K-884 | Hak indirme düğmesine basılıp gönderim başladığında bir kez düşer; düğme kaleme bağlı bir saat ömürlü bağlantı üretir, aralık istekleri yeni hak düşürmez; hakkı dolmuş kalemde bağlantı üretilmez; `07`: indirme ucu — → park A4-3 | UC-36 · UC-37 · konvansiyon — API4-07 | eşlendi |
| K-893 | Sipariş bir kez oluşur — ödeme adımı formu tek kullanımlık onay anahtarı taşır, ikinci gönderim var olan siparişin sayfasına götürür; bildirim önce kaydedilir, hemen "alındı" alır, sonra kuyruktan işlenir; `07`: onay anahtarı ve bildirim ucunun hemen "alındı"sı — → park A4-4 | UC-22 · UC-27 · UC-33 · UC-80 · konvansiyon — API2-05 · konvansiyon — API4-02 · konvansiyon — API4-08 | eşlendi |
| K-899 | Oturum sunucuda tutulur, çerez yalnız rastgele anahtar taşır; kısa ve uzun oturum; oturumu kapatmak kaydı silmektir; müşteri ve yönetici iki ayrı kimlik şeması, ayrı adlı çerezler; `07`: iki kimlik şeması ve çerez adları — → park A4-4 | UC-55 · UC-77 · UC-110 · UC-109 · konvansiyon — API2-03 · konvansiyon — API9-06 | eşlendi |
| K-902 | Genel hız sınırı uygulamanın içinde, IP ekseninde — sayfa ve form istekleri dakikada 300, POST 30; bildirim ucu IP sınırının dışında; aşımda nötr mesaj; gövde tavanı uç başına — görselde P-23, parçalı yüklemede parçanın boyu; `07`: uç başına gövde tavanı ve 429 yanıtı — → park A4-5 | UC-117 · UC-83 · UC-80 · konvansiyon — API2-08 · konvansiyon — API2-07 | eşlendi |
| K-904 | E-postayla gelen hiçbir bağlantının açılması işlem yapmaz — GET yalnız ekranı açar, işlem ekrandaki tek düğmenin POST'u; kayıt ve yeni e-posta doğrulaması (müşteri ve yönetici), geri alma, sıfırlama, davet; sipariş e-postasının bağlantısı yalnız sipariş sayfasını açar; `07`: doğrulama adımının POST ucu — → park A4-4 | UC-52 · UC-53 · UC-75 · UC-76 · UC-58 · UC-59 · UC-85 · UC-113 · UC-108 · UC-186 · UC-187 · UC-34 · konvansiyon — API2-02 · konvansiyon — API4-05 | eşlendi |
| K-912 | Yetki sunucuda, sayfa ve kayıt düzeyinde: panel varsayılan kapalı — açık sayfalar panel girişi, sıfırlama, davet kabulü, doğrulama ekranları; vitrinin hesap sayfaları müşteri oturumu ister; kayıt düzeyinde süzme; sunucunun karar verdiği "yapılabilir işlemler"; `07`: sayfaların erişim kuralı — → park A4-4 | UC-84 · UC-85 · UC-108 · konvansiyon — API2-03 · konvansiyon — API8-01 · konvansiyon — API9-01 | eşlendi |
| K-914 | Beklenmeyen hata üç hâle ayrılır — sayfa isteğinde hata sayfası ve 500, form isteğinde gönderilen değerlerle yeniden gösterim, sonucu belirsiz işlemde sonucu gösteren ekrana yönlendirme; adı konmuş hatalar kendi mesajını taşır; makine ucunda 500; `07`: sayfa ve form uçlarının hata yanıtı — → park A4-6 | UC-78 · UC-80 · konvansiyon — API2-07 · konvansiyon — API6-01 · konvansiyon — API6-02 | eşlendi |
| K-918 | Canlılık ucu uygulama ve veritabanı yanıt veriyorsa 200, durum ucu bir uyarı koşulu varsa 503; ikisi de ayrıntı taşımaz; dış erişilebilirlik kontrolü durum ucunu yoklar; `07`: canlılık ve durum uçları — → park A4-6 | UC-81 · UC-82 · konvansiyon — API4-10 · konvansiyon — API2-07 | eşlendi |
| K-932 | Dokuz modül ve ortak altyapı; vitrin ve panel aynı alan adında, panel ayrı yol alanı — yolun adı `07`'nin (park gerekmez: `05 §3.5.1` işaret eder); yönetici oturumu vitrinde yalnız yönetici şeridi ve taslak önizlemesi için tanınır, müşteri sayılmaz | UC-04 · UC-86 · konvansiyon — API2-01 · konvansiyon — API2-03 · konvansiyon — API9-01 | eşlendi |
| K-933 | Yapılandırılmış veri JSON-LD, paylaşım önizlemesi Open Graph; site haritası makine ucundan istek anında üretilir ve önbellekte tutulur; kapalı sayfalar ve panel `noindex` — meta etiketi ya da `X-Robots-Tag`; robots.txt kapalı sayfaları engellemez (`07`: park gerekmez — K-868'in park satırı) | UC-79 · UC-01 · UC-04 · konvansiyon — API4-09 · konvansiyon — API4-01 | eşlendi |
| K-941 | İmzası tutmayan istek bildirim değildir — kaydedilmez, işlenmez, 4xx alır, IP sınırına sayılır; imzası doğru ama tanınmayan bildirim işaretli kalır; ilke her imzalı makine ucuna uygulanır, yeni imzalı uç karar kaydıyla girer; `07`: uç, imzalı makine uçları — → park A4-4, park A4-5 | UC-80 · konvansiyon — API4-08 · konvansiyon — API2-08 · konvansiyon — API2-07 | eşlendi |
| K-947 | Panelin parçalı yükleme ucu form gönderimleri sınırına girmez, sayfa isteği sınırına girer; yönetici oturumu ve sahteciliğe karşı koruma ister; parçanın boyu `07`'nin uç envanterinde — → park A4-5 | UC-83 · konvansiyon — API2-08 · konvansiyon — API4-07 | eşlendi |
| K-970 | Ödeme kaydında ödeme işlem kimliği tekil; gelen bildirimde tekil olan bildirimin kendisi — tekrar eden bildirim kaydedilmez, işlenmez ve "alındı" alır; sipariş onayı önce onay anahtarını arar, anahtarla sipariş varsa o siparişin sayfasına; `07`: tekrar eden bildirimin yanıtı — → park A4-4 | UC-80 · UC-27 · UC-33 · konvansiyon — API4-08 · konvansiyon — API2-05 | eşlendi |
| K-971 | Giriş şifre değişikliği ve oturum silmeyle sıraya girer; yeniden doğrulama izni dönülen işlem formunun tek kullanımlık değerine bağlı, gönderim tüketir, formun gösterimi dışındaki ilk sayfa isteği düşürür; hesap bağlantısı tek koşullu güncellemeyle tüketilir; bağlantının açılması kayıt değiştirmez; `07`: yeniden doğrulama formunun tek kullanımlık değeri — → park A4-4 | UC-61 · UC-62 · UC-65 · UC-66 · UC-68 · UC-185 · UC-179 · UC-181 · UC-53 · UC-55 · UC-110 · UC-177 · konvansiyon — API2-05 · konvansiyon — API4-05 | eşlendi |
| K-974 | Kalan riskler — iptal edilmiş siparişe gelen geç ödemenin bildirimi yeniden gönderilmezse ürün onu görmez; yeniden gönderimin doğrulanması `08`'de; `07`: makine ucunun 500'ü — → park A4-6 | UC-80 · konvansiyon — API4-08 · konvansiyon — API2-07 · konvansiyon — API6-02 | eşlendi |
| K-977 | Sözlük ürünün terimlerini taşır; teknik kavramların ve kapalı listelerinin kod adını `06` verir, `07` ve `09` onu `06`'dan birebir alır, eş anlamlı ad yok — → park A1-2 | konvansiyon — API0-06 · konvansiyon — API5-02 | konvansiyon |
| K-978 | Firmanın "geri ödeme gerçekleşmedi" işaretinden sonra kart iadesinin geç sonucu ürüne yalnız sağlayıcının bildirimiyle ulaşır — kalan risk; `07`: hata yanıtları ve sağlık uçları — → park A4-6 (kolu dizinde yok — dizin A4) | UC-80 · UC-137 · konvansiyon — API4-08 · konvansiyon — API6-02 | eşlendi |
| K-980 | Stok ve kontenjan alanı kendi koşullu güncellemesiyle — formun gösterdiği değere karşı — yazılır; indirme hakkı tek koşullu güncellemeyle düşer; yükleme disk eşiğini aşacaksa kabulde reddedilir; `07`: sürümün taşınması — → park A4-2 | UC-114 · UC-36 · UC-37 · UC-83 · UC-117 · konvansiyon — API2-04 · konvansiyon — API9-04 · konvansiyon — API4-07 | eşlendi |
| K-988 | Teknik kavramın tanımı hizalanır — biçimini Teknik Mimari'nin kurduğu, sözlükte olmayan kayıt; ürüne görünen kavram — işaret, durum, sayaç, ekran öğesi — sözlükte; geri ödeme kaydının hâlleri teknik, panelde `04` 9.9.8'in etiketleriyle — → park A1-2 | konvansiyon — API0-06 · konvansiyon — API5-02 | konvansiyon |
| K-1011 | Birincil anahtar zamana göre sıralı UUID; dışa görünen kimlikler ayrı ve tekil; iç anahtar vitrinin adresinde kullanılmaz — müşterinin kendi kaydında taşınır ve sahiplik sunucuda denetlenir, panelde serbest; `07`: iç anahtarın adreste kullanımı ve sahiplik denetimi — → park A5-1 | UC-20 · UC-21 · UC-72 · UC-73 · UC-33 · UC-92 · konvansiyon — API2-01 · konvansiyon — API2-03 · konvansiyon — API9-01 | eşlendi |
| K-1014 | Sürüm açık tamsayı sütunu; sürüm taşıyan kayıtlar OB-10'un ekranları — ayarlarda bölüm başına; siparişin sürümünü artıran ve artırmayan yazımlar; OB-10'u olmayan kayıtlar sürüm taşımaz; `07`: sürümün taşınması — → park A4-2 | UC-114 · UC-122 · UC-158 · UC-162 · UC-163 · UC-164 · UC-167 · UC-168 · UC-169 · UC-170 · UC-171 · UC-172 · UC-173 · konvansiyon — API2-04 · konvansiyon — API9-03 · konvansiyon — API9-04 | eşlendi |
| K-1021 | Firma kimliği tek satırlık `Company`, ayarlar `CompanySettings`'te bölüm bölüm; her bölüm kendi sürüm sütununu taşır; geçici kapatma anahtarı sürüm taşımaz, o anki değere karşı yazılır; `07`: anahtarın istekte taşıdığı değer — → park A4-2 | UC-164 · UC-165 · UC-166 · UC-167 · UC-168 · UC-169 · UC-170 · UC-171 · UC-172 · UC-162 · konvansiyon — API2-04 · konvansiyon — API9-04 | eşlendi |
| K-1034 | Entity envanteri ve teknik kavramların kod adları — "… kaydı" `…Record` sonekiyle; `07` kod adlarını `06 §3.3`'ten alır (park satırı gerekmez) | konvansiyon — API0-06 · konvansiyon — API5-02 | konvansiyon |
| K-1036 | `06 §5`'in yapısı ve kod adlarının kaynağı — sözlük, `06 §5`, teknik listeler, kimlikli listeler; `TransitionCode`, `LinkState`, `DispatchType`, `AttemptLimit`; `07`: K-17 park satırı güncellendi — → park A1-2 | konvansiyon — API0-06 · konvansiyon — API5-02 | konvansiyon |
| K-1038 | Alan adının kaynağı — sözlükte terimi olan alan sözlüğün kod karşılığı, olmayan Türkçe adın birebir İngilizcesi, kalıplar (`Is…`, `…At`, `…Date`, `SortOrder`, `…Id`, `…Version`); `07` alan adlarını `06 §4`'ten alır — → park A1-2 | konvansiyon — API0-06 · konvansiyon — API5-02 | konvansiyon |
| K-1051 | İç notun kaydı koşullu güncellemedir — notun o anki metni formun gösterdiği metinle aynıysa yazılır, değilse `04` 2.10.2'nin mesajı; siparişin sürümünü artırmaz; boşaltılan not silinir; `07`: iç notun gösterilen metni — → park A4-2 | UC-152 · konvansiyon — API2-04 · konvansiyon — API9-03 | eşlendi |
| K-1062 | Hesap kayıtlarının alanları — bağlantı kaydının üç bağı, bekleyen kaydın ömrü bağlantının `ExpiresAt`'inden, oturumun hesap türü bağdan, yeniden doğrulama izninin iki alanı; `07`: oturum, bağlantı ve yeniden doğrulamanın uçları — park gerekmez, alanlar `06 §4`'te | UC-52 · UC-53 · UC-55 · UC-60 · UC-61 · UC-110 · UC-185 · konvansiyon — API4-05 · konvansiyon — API5-02 | eşlendi |
| K-1063 | Hakkımızda kurulumda yok, ilk kaydında doğar; içerik kaydının sürümü elle sırayı kapsamaz — "yukarı" ve "aşağı" iki satırın sırasını aynı işlemde yazar, sürümü artırmaz; `07`: elle sıranın istekte taşıdığı değer — → park A4-2 | UC-158 · UC-161 · konvansiyon — API2-04 · konvansiyon — API9-04 | eşlendi |
| K-1066 | Gönderim kaydının ilgili kaydı iki alan — tür ve kimlik değeri; "e-posta ulaşmadı" işaretinin okunuşu; panelden yeniden gönderim gruba yeni satır ekler; `07`: park gerekmez — alanlar `06 §4.58`'de | UC-153 · UC-154 · UC-92 · konvansiyon — API5-02 | eşlendi |
| K-1068 | `DataProtectionKey` çatının varlık türüdür — tamsayı anahtar, K-1011 (1)'in UUID kuralının tek istisnası; uygulamanın kodu tabloya yazmaz; sistem kayıtlarının alanları; `07`: anahtar türünün istisnası — → park A5-1 | konvansiyon — API2-01 | konvansiyon |
| K-1070 | Sayfa adresinin kayda ait parçası `Slug`, kayıt türü içinde tekil; ad sadeleşince boş kalırsa kök türün adı; öneklerin birbirinden ve sabit sayfaların adreslerinden ayrıklığı `07`'nin — → park A5-2 | UC-04 · UC-02 · UC-10 · UC-11 · UC-12 · UC-07 · UC-08 · UC-09 · konvansiyon — API2-01 | eşlendi |
| K-1077 | Ayıpta sözleşmeden dönülen adet başka bir işlemin konusu olamaz — işleme açık adet o adetleri ayrıca düşer; `07`: işlemlerin adedi — → park A3-1 | UC-38 · UC-40 · UC-41 · UC-42 · UC-43 · konvansiyon — API4-04 | eşlendi |
| K-1100 | Bir günden uzun açık tutulan formun yüklemesi silinir, formun kaydı kısıtlayıcı bağa takılır; bir günü aşan parçalı yüklemenin sürdürülmesi baştan başlar; `07`: formun kaydının hata yanıtı ve parçalı yüklemenin sürdürme yanıtı — → park A5-3 | UC-114 · UC-158 · UC-163 · UC-117 · UC-83 · konvansiyon — API4-07 · konvansiyon — API6-02 | eşlendi |
| K-1118 | Hiç bağlanmamış ve bir günden eski tamamlanmış dijital dosya yüklemesi de imhada silinir; görsel ve tamamlanmış dosya yüklemesi imhada, parçalı yükleme gecelik temizlikte; `07`'nin K-1100 park satırı düzeltildi — → park A5-3 | UC-114 · UC-83 · konvansiyon — API4-07 · konvansiyon — API6-02 | eşlendi |
| K-1130 | Etki yansıtma — `04` 2.12.8.7: silinmiş yüklemeyle form kaydedilmez, hata yüklemenin alanında yeniden yüklemeyi isteyen alan mesajıyla gösterilir, öteki girdiler silinmez; `07`: K-1100 · K-1118 park satırı ekrandaki hâlin yerine işaret eder — → park A5-3 | UC-114 · UC-158 · UC-163 · konvansiyon — API6-02 · konvansiyon — API2-07 | eşlendi |
| park A1-1 | K-06 · K-97 · Aktörler — dört aktör (ziyaretçi, misafir alıcı, üye müşteri, firma yöneticisi); yetkilendirme tek yönetici rolünü varsayar — çoklu kullanıcı, aynı yetki | konvansiyon — API0-07 · konvansiyon — API2-03 | konvansiyon |
| park A1-2 | K-17 · K-977 · K-988 · K-1036 · K-1038 · Terim sözlüğü — `02 §1` birebir devralınır; uç, alan ve enum adları sözlüğün İngilizce karşılıklarından; sözlükte olmayanların adı `06 §4`, §5, §3.3'ten; eş anlamlı ad yok | konvansiyon — API0-06 · konvansiyon — API5-02 | konvansiyon |
| dizin A2 | Aşama 2 kararlarının devirleri — K-647…K-718'de bir atıf (K-701); dizin `PHASE2_CONFLICT_SCAN.md` §6.1'de; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez | uç doğurmaz — yazım konvansiyonu | uç doğurmaz |
| park A3-1 | K-787 · K-794 · K-795 · K-1077 · İşlemlerin adedi — müşterinin ve yöneticinin kalem işlemleri kalem başına adet taşır, adet işleme açık adedi aşamaz, gecikme feshinde seçim yok; açık adedin ve varsayılanın sunuluşu ve aşan adedin sunucuda reddi bu aşamanın | UC-40 · UC-43 · UC-45 · UC-38 · UC-42 · UC-44 · UC-129 · UC-138 · UC-140 · UC-142 · konvansiyon — API4-04 · konvansiyon — API9-03 | eşlendi |
| park A3-2 | K-737 · Ürün listesinin araması — ürün adıyla arama, yayın durumuyla süzme; parametrelerin biçimi bu aşamanın | UC-87 · konvansiyon — API2-06 · konvansiyon — API9-02 | eşlendi |
| park A3-3 | K-802 · Sipariş listesinin araması ve süzgeçleri — numara ya da e-postayla tam eşleşme; sipariş durumu, ödeme durumu, bekleyen iş süzgeçleri; sıra; parametrelerin biçimi bu aşamanın | UC-91 · konvansiyon — API2-06 · konvansiyon — API9-02 | eşlendi |
| dizin A3 | Aşama 3 kararlarının devirleri — K-723…K-850'de altı atıf ve `04`'ün gövdesinde iş bırakan cümleler `PHASE3_CONFLICT_SCAN.md` §6'da; karar satırsız tek iş: sitenin dışından gelinen sayfa adreslerinin biçimi (`04 §3.5` — `04 §3.5` g1) | konvansiyon — API2-01 · uç doğurmaz — yazım konvansiyonu | konvansiyon |
| park A4-1 | K-868 · Sözleşmelerin biçimi — sayfalar sunucuda Razor Pages, ayrı JSON API yok; uç envanteri sayfa ve form uçlarını ve makine uçlarını — bildirim, parçalı yükleme, indirme, site haritası ve robots.txt, canlılık ve durum — taşır; yeni imzalı uç karar kaydıyla girer; canlılık ve durum imzasız açık GET | UC-80 · UC-83 · UC-37 · UC-79 · UC-81 · UC-82 · konvansiyon — API0-04 · konvansiyon — API2-02 · konvansiyon — API2-10 · konvansiyon — API4-09 | eşlendi |
| park A4-2 | K-873 · K-874 · K-980 · K-1014 · K-1021 · K-1051 · K-1063 · Sürümün taşınması — OB-10'un düzenleme formları ve ayarların bölüm sürümleri açılıştaki sürümü, yöneticinin sipariş işlemleri gördüğü sürümü taşır; stok ve kontenjan, iç not, elle sıra ve geçici kapatma anahtarı ekranın gösterdiği değeri taşır; sürüm değişmişse yanıt ve "Yine de kaydet" | UC-114 · UC-122 · UC-158 · UC-161 · UC-162 · UC-163 · UC-164 · UC-165 · UC-166 · UC-167 · UC-168 · UC-173 · UC-152 · UC-123 · konvansiyon — API2-04 · konvansiyon — API9-03 · konvansiyon — API9-04 | eşlendi |
| park A4-3 | K-883 · K-884 · Dosya uçları — dijital dosya parçalı ve sürdürülebilir yüklenir; indirme düğmesi kaleme bağlı bir saat ömürlü bağlantı üretir, aralık istekleri yeni hak düşürmez, ömür yalnız isteğin kabulünde denetlenir | UC-83 · UC-36 · UC-37 · konvansiyon — API4-07 | eşlendi |
| park A4-4 | K-893 · K-899 · K-904 · K-912 · K-941 · K-970 · K-971 · Formlar, kimlik şemaları ve erişim — onay anahtarı; bildirim ucunun "alındı"sı, tekrar eden ve imzası tutmayan bildirimin yanıtı; iki kimlik şeması ve çerezler; yeniden doğrulamanın tek kullanımlık değeri; bağlantı yalnız ekranı açar, işlem POST; panel varsayılan kapalı, kayıt düzeyinde süzme, yapılabilir işlemler | UC-27 · UC-80 · UC-55 · UC-110 · UC-61 · UC-185 · UC-52 · UC-53 · UC-75 · UC-76 · UC-186 · UC-187 · konvansiyon — API2-02 · konvansiyon — API2-03 · konvansiyon — API2-05 · konvansiyon — API4-02 · konvansiyon — API4-05 · konvansiyon — API4-08 · konvansiyon — API8-01 | eşlendi |
| park A4-5 | K-902 · K-941 · K-947 · Gövde tavanı ve hız sınırı yanıtı — tavan uç başına (görselde P-23, parçalı yüklemede parçanın boyu); aşım nötr mesaj; imzalı makine ucu imzası doğru istekte IP sınırına girmez; parçalı yükleme ucu gönderim sınırına girmez, sayfa sınırına girer; uç başına değerler, parçanın boyu, pencerenin türü ve yanıt bu aşamanın | UC-117 · UC-83 · UC-80 · konvansiyon — API2-08 · konvansiyon — API4-07 | eşlendi |
| park A4-6 | K-914 · K-918 · K-974 · K-978 · Hata yanıtları ve sağlık uçları — sayfa isteğinde 500 ve hata sayfası, form isteğinde yeniden gösterim, sonucu belirsiz işlemde yönlendirme, makine ucunda 500; geç ödemenin ve kart iadesinin geç sonucunun kalan riski; canlılık 200, durum 503; uçların adresi ve yanıtın biçimi bu aşamanın | UC-78 · UC-80 · UC-81 · UC-82 · konvansiyon — API2-07 · konvansiyon — API4-10 · konvansiyon — API6-02 | eşlendi |
| dizin A4 | Aşama 4 kararlarının devirleri — K-855…K-979'da yirmi atıf ve `05`'in gövdesinde iş bırakan cümleler `PHASE4_CONFLICT_SCAN.md` §6'da; checkpoint'in kararları ve K-978'in kolu yalnız park satırlarında; karar satırsız iki iş: sayfa adreslerinin önekleri (`05 §3.7.1`) ve parçalı yüklemede doğrulama değerinin istek başlığındaki biçimi (`05 §6.5.1`) | konvansiyon — API2-01 · konvansiyon — API2-05 · konvansiyon — API6-02 | konvansiyon |
| park A5-1 | K-1011 · K-1068 · İç anahtar ve dışa görünen kimlikler — anahtar sıralı UUID (veri koruma anahtarı istisna); vitrinde sayfa adresi, numara ve e-posta ya da erişim anahtarı — iç anahtar yok; müşterinin kendi kaydında iç anahtar ve sahiplik denetimi; panelde serbest | UC-20 · UC-21 · UC-72 · UC-73 · UC-33 · UC-34 · konvansiyon — API2-01 · konvansiyon — API2-03 · konvansiyon — API9-01 | eşlendi |
| park A5-2 | K-1070 · Sayfa adreslerinin önekleri — `Slug` kayıt türü içinde tekil; her türün öneki birbirinden ve sabit sayfaların adreslerinden ayrık olmalı; önekler ve adresin istekteki biçimi bu aşamanın | UC-04 · UC-02 · UC-10 · UC-11 · UC-12 · UC-07 · UC-08 · UC-09 · UC-13 · UC-14 · UC-19 · konvansiyon — API2-01 | eşlendi |
| park A5-3 | K-1100 · K-1118 · Silinmiş yüklemeyle form kaydı — formun kaydının hata yanıtı hangi yüklemenin olmadığını söyler ve yeniden yüklemeyi ister (ekrandaki hâl `04` 2.12.8.7); satırı silinmiş parçalı yüklemeye gelen sürdürme isteği baştan başlar | UC-114 · UC-158 · UC-163 · UC-83 · konvansiyon — API4-07 · konvansiyon — API6-02 | eşlendi |
| dizin A5 | Aşama 5 kararlarının devirleri — K-992…K-1136'da on altı atıf ve `06`'nın gövdesinde iş bırakan cümleler `PHASE5_CONFLICT_SCAN.md` §6'da; üç karar "park gerekmez" (K-1034, K-1062, K-1066), yedisi Aşama 1, 3 ve 4 park satırlarını güncelledi | konvansiyon — API0-06 · konvansiyon — API4-05 · konvansiyon — API5-02 | konvansiyon |
| `02 §1.1` g1 | `06`, `07` ve `09` sözlüğü birebir devralır, isim icat etmez | konvansiyon — API0-06 | konvansiyon |
| `02 §1.1` g2 | Teknik kavramların ve kapalı listelerinin kod adlarını `06` verir; `07` ve `09` onları `06`'dan alır; ürüne görünen kavram sözlükte (K-977, K-988) | konvansiyon — API0-06 · konvansiyon — API5-02 | konvansiyon |
| `02 §3.30` g1 | Ürünün adresi kategoriye bağlı değildir, adından üretilir (`/urun/ahsap-masa`); önekin kendisi `07`'nin işi; kategori adresi ağaçtaki yerinden bağımsız | UC-04 · UC-02 · konvansiyon — API2-01 | eşlendi |
| `02 §5` g1 | Durum adları ve kod karşılıkları sözlükte; `06`, `07` ve `09` birebir devralır | konvansiyon — API5-02 | konvansiyon |
| `02 §11` g1 | Sayısal parametreler koda gömülmez; `06`, `07` ve `09` değeri `02 §11`'den okur | konvansiyon — API2-09 · konvansiyon — API2-08 | konvansiyon |
| `04 §3.5` g1 | E-postadaki bağlantıyla, dış sayfadan dönüşle ya da doğrudan açılan girişler; e-postanın metni `08`'in, adreslerin biçimi `07`'nin işi | UC-34 · UC-52 · UC-58 · UC-75 · UC-30 · UC-51 · konvansiyon — API2-01 | eşlendi |
| `05 §3.5.1` g1 | Vitrin ve panel aynı alan adında iki sayfa alanı; panel ayrı yol alanı, yolun adı `07`'nin | UC-86 · konvansiyon — API2-01 | eşlendi |
| `05 §3.7.1` g1 | Adres kaydın adından sunucuda üretilir ve ilk yayında sabitlenir; adreslerin önekleri `07`'nin | UC-04 · UC-10 · UC-11 · UC-12 · UC-02 · konvansiyon — API2-01 | eşlendi |
| `05 §6.3.5` g1 | Genel hız sınırı dakikada 300 ve 30; imzalı makine ucu ve parçalı yükleme ucunun muafiyeti; parçanın boyu `07`'nin uç envanterinde; pencerenin türü `07`'nin | UC-80 · UC-83 · konvansiyon — API2-08 · konvansiyon — API4-07 | eşlendi |
| `05 §6.3.6` g1 | Gövde tavanı uç başına — genel formlarda küçük, görselde P-23, parçalı yüklemede parçanın boyu (`07`); uç başına değerler `07`'nin uç envanteriyle yazılır | UC-117 · UC-83 · konvansiyon — API2-08 | eşlendi |
| `05 §6.5.1` g1 | Her form gönderimi sahteciliğe karşı korumadan geçer; parçalı yüklemede doğrulama değeri istek başlığında — biçimi `07` ve `09`'da; istisna imzalı makine ucu; ödeme dönüşü POST'la geliyorsa ikinci istisna | UC-83 · UC-80 · UC-30 · konvansiyon — API2-05 | eşlendi |
| `05 §6.7.1` g1 | Çerezler birinci taraf, Secure, HttpOnly, SameSite=Lax, `__Host-`; ödeme sağlayıcısından dönüşün sipariş sayfasına oturum çerezine dayanmadan inmesi `07` ile `08`'in işi | UC-30 · UC-33 · konvansiyon — API4-02 · konvansiyon — API2-03 | eşlendi |
| `05 §7.7.3` g1 | Dijital dosyanın yüklemesi parçalı — parçanın boyu `07`'de; kesilen yükleme kaldığı yerden sürer, tamamlanmayan yükleme bir gün sonra silinir | UC-83 · konvansiyon — API4-07 | eşlendi |
| `06 §2.3.4` g1 | İç anahtar vitrinin adresinde kullanılmaz; müşterinin kendi kaydında taşınır ve sahiplik denetlenir; panelde serbest; adreste kullanımı `07`'nin işi | UC-20 · UC-21 · UC-72 · UC-73 · konvansiyon — API2-01 · konvansiyon — API2-03 | eşlendi |
| `06 §3.3` g1 | Teknik kayıtların kod adı `06 §3.3`'te — `…Record` soneki; API Tasarımı onları buradan alır | konvansiyon — API0-06 · konvansiyon — API5-02 | konvansiyon |
| `06 §4` g1 | Alan adının kuralı — sözlükte terimi olan alan sözlüğün kod karşılığı, olmayan birebir İngilizce; kalıplar; API Tasarımı alan adlarını `06 §4`'ten alır | konvansiyon — API0-06 · konvansiyon — API5-02 | konvansiyon |
| `06 §4.70` g1 | Dijital dosya yükleme başına bir satır; parçanın boyu ve sürdürmenin biçimi `07`'nin; dosya genel adresle sunulmaz, indirme sipariş sayfasının erişimi denetlenerek | UC-83 · UC-37 · konvansiyon — API4-07 | eşlendi |
| `06 §4.72` g1 | `Slug` önek olmadan; adresin tamamı `07`'nin verdiği önek ve bu değer | UC-04 · UC-02 · UC-10 · UC-11 · UC-12 · konvansiyon — API2-01 | eşlendi |
| `06 §4.72.1` g1 | Tekillik kayıt türü içinde; öneklerin birbirinden ve sabit sayfaların adreslerinden ayrıklığı `07`'nin işi | konvansiyon — API2-01 | konvansiyon |
| `06 §5.4.3` g1 | `LinkState` — bağlantının hâli kaydın anlarından okunur; kod adı API Tasarımı'nın ve Kodlama Kılavuzu'nun ortak adı | UC-52 · UC-58 · UC-75 · konvansiyon — API5-02 | eşlendi |
| `08` park A4-3 | K-906 · K-973 · Ödeme sağlayıcısından dönüş — başka siteden gelen istek, sipariş sayfasına oturum çerezine dayanmadan iner; dönüş durum yazmaz; POST'la gelirse sahteciliğe karşı korumanın ikinci istisnası; yolu ve yöntemi — GET ya da POST — bu aşamanın ve `07`'nin kararı | UC-30 · UC-33 · konvansiyon — API4-02 · konvansiyon — API2-05 | eşlendi |

#### 1.1.12 Teknik Mimari — MK-04 ve uç düzeyindeki bölümler (102)

Kaynağın sırasıyla: `05 §2.1`'in MK-04 satırı ve on üç bölümün — §3.5 (vitrin ve panel), §3.6 (dış servislerin sınırı), §3.7 (sayfa adresleri ve arama motoru verileri), §4.5 (tek işlenme), §6.1 (kimlik doğrulama ve oturum), §6.2 (yetkilendirme), §6.3 (deneme limitleri ve istek hızı), §6.4 (anahtarlar ve bağlantılar), §6.5 (form ve site güvenliği — güvenlik başlıklarının beş liste maddesi kimliksiz madde), §6.7 (çerezler), §7.7 (dosya depolama ve dijital teslim), §9.4 (izleme ve uyarı), §10.3 (beklenmeyen hata) — numaralı maddeleri, `exp05()`'in sırasıyla (tablo satırları dahil; 6.7.12 ve 9.4.16–9.4.17 tablodaki yerlerinde). Eşlemenin okunuşu K-1159'dadır: maddenin uca dokunan kuralı makine ucunun ya da ekranın adayına ve kuralın karara bağlanacağı konuya; yalnız mekanizma olan madde adıyla yazılır — kendiliğinden iş *sistem işi (`05 §5`)*, kurulum tarafının işi *kurulum*, ürünün içinde olmayan işlev *yoktur*; "Elenen" maddesi seçilen kuralın konusuna düşer. Bütün yanıtlara uygulanan güvenlik başlıkları (§6.5.4 ve beş liste maddesi) `konvansiyon — API2-05`'tir — konunun sahteciliğe karşı koruma kolu yanıtın tarayıcıya verdiği korumayı da taşır, kural evinde (`05`) kalır; form gönderiminin hedeflerini ya da anahtar taşıyan sayfayı anan madde ayrıca adayları taşır. Durum ucunun uyarı koşulları (9.4.2–9.4.17) durum ucunun (UC-82) 503'ünü belirler ve onun satırıdır. Makine uçlarının adaylarından canlılık ve durum ucu (UC-81, UC-82) ilk kaynak satırını burada ve §1.1.11'de alır.

| Kaynak ID | Kaynak özeti | Uç | Durum |
|---|---|---|---|
| `05 §2.1` MK-04 | Sayfalar sunucuda Razor Pages ile tam HTML; gönder–yönlendir–göster; ayrı JSON API yok; makine uçları — bildirim (§4.5), parçalı yükleme ve indirme (§7.7), site haritası ve robots.txt (§3.7), canlılık ve durum (§9.4); yeni imzalı uç karar kaydıyla; canlılık ve durum imzasız açık GET | UC-80 · UC-83 · UC-37 · UC-79 · UC-81 · UC-82 · konvansiyon — API0-04 · konvansiyon — API2-02 · konvansiyon — API2-10 | eşlendi |
| `05 §3.5.1` | Vitrin ve panel aynı uygulamanın iki sayfa alanı, aynı alan adında; panel ayrı yol alanı — yolun adı `07`'nin | UC-86 · konvansiyon — API2-01 | eşlendi |
| `05 §3.5.2` | İki hesap türü iki kapıdan girer — ayrı tablo, ayrı adlı çerez, ayrı kimlik şeması; e-posta tür içinde tekil ve harf ayrımsız karşılaştırılır; panelde Google girişi yok | UC-55 · UC-48 · UC-50 · UC-110 · konvansiyon — API2-03 | eşlendi |
| `05 §3.5.3` | Panel varsayılan kapalı — açık sayfalar panel girişi, şifre sıfırlama, davet kabulü, doğrulama ekranları; panelin bütün sayfaları arama motorlarına kapalı | UC-84 · UC-85 · UC-108 · konvansiyon — API9-01 · konvansiyon — API4-09 | eşlendi |
| `05 §3.5.4` | Vitrin panelin oturumunu yalnız yönetici şeridi ve taslak önizlemesi için tanır; yönetici oturumu vitrinde müşteri sayılmaz | UC-04 · UC-10 · UC-11 · UC-12 · konvansiyon — API2-03 · konvansiyon — API2-02 | eşlendi |
| `05 §3.5.5` | Çerez süzgeci yalnız vitrinin yanıtlarına uygulanır | konvansiyon — API2-03 | konvansiyon |
| `05 §3.5.6` | Elenen — panel için alt alan adı: ikinci DNS kaydı ve sertifika; şerit ve önizleme oturumun alan adları arasında taşınmasını isterdi | konvansiyon — API2-01 | konvansiyon |
| `05 §3.6.1` | Üç dış servis bağlantı katmanının arkasında — biçim `08`'in; imzalı makine uçlarının ilkesi — bugün yalnız bildirim, doğrulanamayan istek kayda yazılmaz; canlılık ve durum imzasız açık GET; yeni imzalı uç MK-04'e, §6.5.1'in istisnasına ve §6.3.5'in muafiyetine karar kaydıyla | UC-80 · UC-81 · UC-82 · konvansiyon — API4-08 · konvansiyon — API4-10 · konvansiyon — API2-05 | eşlendi |
| `05 §3.6.2` | Ürün kuralı çekirdekte, bağlantı katmanına inmez — son sorgu, ödeme sayfası açılmamış siparişin iptali L-7'ye sayılmaz, kart iadesi bir kez, e-postanın üç sınıfı | UC-29 · uç doğurmaz — sistem işi (`05 §5`) · uç doğurmaz — e-posta (`08`) | eşlendi |
| `05 §3.6.3` | Dış çağrı veritabanı kilidinin içinde beklenmez; kart hattında sağlayıcıya yönlendirme sipariş onayının işleminden sonra; bildirim önce kaydedilir, sonra işlenir | UC-27 · UC-29 · UC-80 | eşlendi |
| `05 §3.6.4` | Desteklenen ödeme sağlayıcısının sayısı `08`'in kararı; bağlantı katmanı ödeme işlerini sağlayıcıdan bağımsız adlarla taşır — geçiş, dönüş ve bildirim uçları sağlayıcının adını taşımaz | UC-29 · UC-30 · UC-80 · konvansiyon — API4-02 · konvansiyon — API4-08 | eşlendi |
| `05 §3.6.5` | Altyapı aynı kalıpta — dosya deposu arayüzü; yol haritası adaylarının dış servisleri aynı kalıpla eklenir — dosya uçları depoya arayüzden erişir | UC-37 · UC-83 · UC-117 | eşlendi |
| `05 §3.6.6` | Bir dış servis çökerse yeniden deneme, geri düşme ve kullanıcının gördüğü `05 §10.1`'de, ürün davranışı `03 §10.1`'de | uç doğurmaz — sistem işi (`05 §5`) | uç doğurmaz |
| `05 §3.7.1` | Adres kaydın adından sunucuda üretilir ve ilk yayında sabitlenir; yönlendirme yok; tekillik veritabanında; önekler `07`'nin | UC-04 · UC-02 · UC-10 · UC-11 · UC-12 · UC-06 · konvansiyon — API2-01 | eşlendi |
| `05 §3.7.2` | Arama motoru verileri ilk yanıtta — başlık ve açıklama, JSON-LD (ürün, firma, kırıntı yolu, SSS), Open Graph | UC-01 · UC-02 · UC-04 · UC-08 · UC-10 · UC-11 · UC-12 · UC-13 · konvansiyon — API4-01 | eşlendi |
| `05 §3.7.3` | Site haritası makine ucundan, XML, istek anında yayındaki kayıtlardan; bellek içi önbellek; arşiv ve taslak girmez | UC-79 · konvansiyon — API4-09 | eşlendi |
| `05 §3.7.4` | Kapalı sayfalar ve panel her yanıtta `noindex` — meta etiketi ya da `X-Robots-Tag`; robots.txt site haritasıyla aynı makine ucundan, kapalı sayfaları engellemez | UC-79 · UC-04 · konvansiyon — API4-09 | eşlendi |
| `05 §3.7.5` | Elenen — microdata ve kapalı sayfaları robots.txt ile engellemek | konvansiyon — API4-09 | konvansiyon |
| `05 §4.5.1` | Sipariş bir kez oluşur — ödeme adımı her gösterildiğinde tek kullanımlık onay anahtarı; ikinci gönderim var olan siparişin sayfasına | UC-22 · UC-27 · UC-33 · konvansiyon — API2-05 · konvansiyon — API4-02 | eşlendi |
| `05 §4.5.2` | Bildirim bağlantı katmanında doğrulanır, iş satırıyla aynı işlemde kaydedilir, diske yazıldıktan hemen sonra "alındı"; tekrar eden bildirim kaydedilmez ve "alındı" alır; imzası tutmayan istek 4xx, IP sınırına sayılır; imzası doğru istek sınırın dışında | UC-80 · konvansiyon — API4-08 · konvansiyon — API2-07 · konvansiyon — API2-08 | eşlendi |
| `05 §4.5.3` | Ödeme sonucu tek yoldan — bildirim ve son sorgu aynı işleve; ödeme işlem kimliği tekil; kart iadesinin sonucu §4.6.2'nin yolunda | UC-80 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `05 §4.5.4` | Müşterinin ve yöneticinin tekrarlanan işlemi güncel hâle göre — yapılmış işlem ikinci kez işlenmez, kullanıcı güncel hâli görür | konvansiyon — API2-04 · konvansiyon — API9-03 · konvansiyon — API2-05 | konvansiyon |
| `05 §4.5.5` | Tanınmayan bildirim işlenmez, işaretli kalır; bildirim siparişten önce gelmez; saklama on yıl; "sonucu belirsiz" hâl §10.3'te | UC-80 · konvansiyon — API4-08 | eşlendi |
| `05 §6.1.1` | Oturum sunucuda — çerez yalnız rastgele anahtar; anahtar yalnız başarılı girişte üretilir; yeniden doğrulama anahtarı değiştirmez; kısa ve uzun oturum | UC-55 · UC-110 · UC-61 · konvansiyon — API2-03 | eşlendi |
| `05 §6.1.2` | Oturumu kapatmak kaydı silmek — sıfırlama, geri alma, hesap silme, yöneticinin kaldırılması bütün oturumları, şifre değişikliği değiştiren dışındakileri; giriş silmeyle sıraya girer; kaldırılanın sonraki isteği oturumsuz | UC-59 · UC-76 · UC-68 · UC-177 · UC-65 · UC-181 · UC-112 · UC-187 · UC-55 · UC-110 · konvansiyon — API2-03 | eşlendi |
| `05 §6.1.3` | İki hesap türü iki ayrı kimlik şeması, ayrı adlı çerezler; yönetici aynı doğrulama, oturum, yeniden doğrulama ve şifre rejiminde | UC-55 · UC-110 · konvansiyon — API2-03 · konvansiyon — API9-06 | eşlendi |
| `05 §6.1.4` | Şifre çatının özetleyicisiyle; Google girişi yetki kodu akışı — `state`, `nonce`, PKCE; dönüş Google'dan GET ile, tam adresle kayıtlı; akışın biçimi `08`'in | UC-50 · UC-51 · UC-55 · UC-48 · konvansiyon — API2-03 · konvansiyon — API4-05 | eşlendi |
| `05 §6.1.5` | Yeniden doğrulama şifreyle ya da taze Google girişiyle (`auth_time`); izin yalnız başlatılan işlem için, dönülen formun tek kullanımlık değerine bağlı; gönderim tek koşullu güncellemeyle tüketir; ilk başka sayfa isteği izni düşürür | UC-60 · UC-61 · UC-62 · UC-185 · UC-65 · UC-66 · UC-68 · UC-179 · UC-181 · konvansiyon — API2-05 · konvansiyon — API4-05 | eşlendi |
| `05 §6.1.6` | Elenenler — imzalı çerez kimliği, Identity'nin tamamı, ayrı kimlik sunucusu, Argon2id, iznin süresi, açık Google oturumu, geriye dönük pencere, `form_post`; Google girişinin GET bağlantısıyla başlatılması — girişi başlatan istek form gönderimidir | UC-50 · UC-62 · konvansiyon — API2-03 · konvansiyon — API2-05 | eşlendi |
| `05 §6.2.1` | Yetki sunucuda, sayfa ve kayıt düzeyinde; sunucunun karar verdiği "yapılabilir işlemler" ekrana gelir, her işlem yeniden denetlenir | konvansiyon — API8-01 · konvansiyon — API2-03 | konvansiyon |
| `05 §6.2.2` | Panel varsayılan kapalı — açık sayfalar panel girişi, sıfırlama, davet kabulü, doğrulama; bütün yöneticiler aynı yetkide, rol ve izin modeli yok | UC-84 · UC-85 · UC-108 · konvansiyon — API9-01 · konvansiyon — API0-07 | eşlendi |
| `05 §6.2.3` | Vitrinin hesap sayfaları müşteri oturumu ister; yönetici oturumu vitrinde müşteri sayılmaz | UC-63 · UC-70 · UC-74 · konvansiyon — API2-03 | eşlendi |
| `05 §6.2.4` | Kayıt düzeyinde denetim — sipariş, adres ve talep sorgusu sahibine göre süzülür (üye hesabı; misafir numara + e-posta ya da erişim anahtarı); adresteki kimliği değiştirmek başkasının kaydını açmaz | UC-33 · UC-34 · UC-32 · UC-72 · UC-73 · UC-20 · UC-21 · konvansiyon — API2-03 | eşlendi |
| `05 §6.2.5` | Elenenler — sayfa başına yetki işareti, şimdiden rol ve izin modeli | konvansiyon — API9-01 · konvansiyon — API0-07 | konvansiyon |
| `05 §6.3.1` | Deneme limitleri (L-1…L-9) veritabanında deneme kaydı; kayan pencere; başarısızlıkta sayılanlarda satır önce kesinleşir; aşımda nötr mesaj, hesap kilitlenmez | UC-55 · UC-110 · UC-32 · UC-23 · UC-57 · UC-111 · UC-48 · UC-15 · UC-49 · konvansiyon — API2-08 | eşlendi |
| `05 §6.3.2` | Eksenlere özgü mekanizmalar — L-7 ve L-8 siparişlerden sayılır; L-6 sepet ekseni, birleşmede büyük sayaç; tanınan tarayıcı; L-5 yazılan adres; L-1 yeniden doğrulamayı da sayar; L-9 iki hesap türünün yeniden istemesi | UC-27 · UC-23 · UC-55 · UC-32 · UC-61 · UC-185 · UC-49 · UC-67 · UC-180 · konvansiyon — API2-08 | eşlendi |
| `05 §6.3.3` | IP yalnız ters vekilin ilettiği başlıktan; IPv6'da /64; ters vekilin istemcinin gerçek adresini görmesi önkoşul — kurulumda denetlenir | konvansiyon — API2-08 · uç doğurmaz — kurulum | konvansiyon |
| `05 §6.3.4` | Deneme kaydı kısa yaşar, gecelik temizlikte silinir; sayaç kaba bir fren | uç doğurmaz — sistem işi (`05 §5`) · konvansiyon — API2-08 | konvansiyon |
| `05 §6.3.5` | Genel hız sınırı — sayfa ve form istekleri dakikada 300, POST 30; statik dosya ve görseller dışarıda; imzalı makine ucu ve parçalı yükleme ucunun muafiyeti; aşımda nötr mesaj; pencerenin türü `07`'nin | UC-80 · UC-83 · konvansiyon — API2-08 · konvansiyon — API2-07 | eşlendi |
| `05 §6.3.6` | Gövde tavanı uç başına — genel formlarda küçük, görselde P-23, parçalı yüklemede parçanın boyu; uç başına değerler `07`'nin uç envanteriyle | UC-117 · UC-83 · konvansiyon — API2-08 | eşlendi |
| `05 §6.3.7` | Elenenler — bellekte deneme sayacı, L-7 ve L-8'e ayrı sayaç, IPv6'da tam adres, ters vekilde hız sınırı, içerik dağıtım ağı, sınırsızlık; dağıtık saldırı kalan risk | konvansiyon — API2-08 · uç doğurmaz — kurulum | konvansiyon |
| `05 §6.4.1` | Bütün anahtarlar işletim sisteminin kriptografik rastgele üreteciyle; hesap bağlantılarının anahtarı türetme sırrından | konvansiyon — API2-01 | konvansiyon |
| `05 §6.4.2` | Sipariş numarası — yıl ve P-19 kadar rastgele karakter, karışabilen karakterler yok; açık, tekil | UC-32 · UC-33 · UC-27 | eşlendi |
| `05 §6.4.3` | Sipariş erişim anahtarı — en az 128 bit, siparişte açık; yeniden gönderilen e-posta aynı bağlantıyı taşır; e-postanın düzeltilmesi yeni anahtar üretir | UC-34 · UC-151 · konvansiyon — API2-01 | eşlendi |
| `05 §6.4.4` | Hesap bağlantılarının anahtarları — kayıt ve yeni e-posta doğrulaması, şifre sıfırlama, davet, geri alma; HMAC ile türetilir, yalnız özeti bağlantı kaydında | UC-52 · UC-58 · UC-75 · UC-85 · UC-84 · UC-108 · konvansiyon — API2-01 | eşlendi |
| `05 §6.4.5` | Oturum anahtarı, tanınan tarayıcı işareti ve onay anahtarı aynı üreteçle | UC-55 · UC-110 · UC-27 | eşlendi |
| `05 §6.4.6` | Bağlantının hâli kayıttan okunur — kayıt yok, ömrü dolmuş, kullanılmış, yerine yenisi geçmiş, geçerli; tek koşullu güncellemeyle tüketilir, güncellenen satır yoksa ekran o anki hâli gösterir; davetin ömrünü çeken olaylar — geri çekme, aynı adrese yeni davet, gönderenin kaldırılması | UC-52 · UC-53 · UC-58 · UC-59 · UC-75 · UC-76 · UC-85 · UC-113 · UC-108 · UC-186 · UC-187 · UC-175 · UC-176 · UC-177 · konvansiyon — API2-05 | eşlendi |
| `05 §6.4.7` | E-postayla gelen bağlantının açılması işlem yapmaz — GET yalnız ekranı açar, hiçbir kaydı değiştirmez; işlem ekrandaki tek düğmenin POST'u; sipariş e-postasının bağlantıları yalnız sipariş sayfasını açar, indirme bağlantısı e-postayla gitmez | UC-52 · UC-53 · UC-58 · UC-59 · UC-75 · UC-76 · UC-85 · UC-113 · UC-108 · UC-186 · UC-187 · UC-34 · UC-36 · konvansiyon — API2-02 | eşlendi |
| `05 §6.4.8` | Anahtar sistem kayıtlarına yazılmaz — anahtar taşıyan adres maskelenir; bağlantı sayfaları başka siteye giden istekte adresi taşımaz | konvansiyon — API2-01 | konvansiyon |
| `05 §6.4.9` | Elenenler — erişim anahtarının yalnız özetiyle saklanması (kalan risk), ham anahtarın gönderim kaydında tutulması, sıralı numara, imzalı kayıtsız bağlantı, açılışla doğrulama, betikle kendiliğinden gönderim, ömrü dolan kaydın silinmesi | UC-34 · UC-52 · konvansiyon — API2-01 · konvansiyon — API2-02 | eşlendi |
| `05 §6.5.1` | Her form gönderimi sahteciliğe karşı korumadan geçer — varsayılan, parçalı yükleme dahil; parçalı yüklemede değer istek başlığında, biçimi `07` ve `09`'da; istisna imzalı makine ucu; ödeme dönüşü POST'la geliyorsa ikinci istisna | UC-83 · UC-80 · UC-30 · konvansiyon — API2-05 | eşlendi |
| `05 §6.5.2` | İletişim formunun tuzak alanı sunucuda — doluysa gönderim sessizce düşer; L-4; captcha yok | UC-15 · konvansiyon — API2-08 | eşlendi |
| `05 §6.5.3` | Siteye kod girmez — değerler HTML'e kodlanır; uzun metinler kapalı biçim setinin izinli etiket listesiyle süzülür, bağlantı yalnız `https`, `http`, `mailto` | UC-114 · UC-158 · UC-173 · konvansiyon — API5-01 | eşlendi |
| `05 §6.5.4` | Her yanıt tarayıcıya güvenlik başlıklarını verir | konvansiyon — API2-05 | konvansiyon |
| `05 §6.5` m1 | İçerik güvenlik politikası — kaynaklar yalnız siteden; satır içi stil bloğu tek kullanımlık değerle; form gönderimi ve yönlendirme yalnız siteye, ödeme sağlayıcısına ve Google'ın yetkilendirme adresine; site çerçevede açılmaz, sağlayıcının çerçevesi yalnız o sayfada | UC-22 · UC-29 · UC-50 · UC-62 · konvansiyon — API2-05 | eşlendi |
| `05 §6.5` m2 | HSTS — bir yıl; alt alan adları ve ön yükleme yok | konvansiyon — API2-05 | konvansiyon |
| `05 §6.5` m3 | Türün tahmin edilmemesi; dijital dosyanın sunuluşu §6.6.3'te | UC-37 · konvansiyon — API2-05 | eşlendi |
| `05 §6.5` m4 | Yönlendiren adresin sınırı — başka siteye yalnız kök; bağlantı anahtarı taşıyan sayfalar hiçbir şey göndermez | UC-34 · UC-52 · UC-58 · UC-75 · UC-85 · UC-108 · konvansiyon — API2-05 | eşlendi |
| `05 §6.5` m5 | Tarayıcı izinleri — kamera, mikrofon, konum kapalı | konvansiyon — API2-05 | konvansiyon |
| `05 §6.5.5` | Elenenler — korumanın uç uç eklenmesi, ham HTML, politikanın yalnız raporlama kipi, `unsafe-inline` | konvansiyon — API2-05 · konvansiyon — API5-01 | konvansiyon |
| `05 §6.5.6` | Girişten ve yeniden doğrulamadan sonraki dönüş adresi yalnız sitenin kendi yolu — değilse yok sayılır, vitrinde ana sayfaya, panelde panelin ana sayfasına | UC-54 · UC-55 · UC-60 · UC-61 · UC-62 · UC-84 · UC-110 · UC-185 · konvansiyon — API2-03 | eşlendi |
| `05 §6.7.1` | Çerezler birinci taraf, Secure, HttpOnly, SameSite=Lax, `__Host-`; Google akış çerezi de Lax — dönüş GET; dönüşün sipariş sayfasına oturum çerezine dayanmadan inmesi `07` ile `08`'in | UC-30 · UC-33 · UC-51 · konvansiyon — API2-03 · konvansiyon — API4-02 | eşlendi |
| `05 §6.7.2` | Vitrinin oturum çerezi — rastgele anahtar; "beni hatırla"ya göre oturumluk ya da kalıcı | UC-55 · konvansiyon — API2-03 | eşlendi |
| `05 §6.7.3` | Panelin oturum çerezi — ayrı adla, aynı biçim | UC-110 · konvansiyon — API2-03 | eşlendi |
| `05 §6.7.4` | Form güvenliği çerezi — sahteciliğe karşı korumanın değeri, tarayıcı oturumu | konvansiyon — API2-05 | konvansiyon |
| `05 §6.7.5` | Misafir sepeti çerezi — rastgele sepet kimliği, 400 gün, her ziyarette yenilenir | UC-05 · UC-19 · UC-55 · konvansiyon — API4-02 | eşlendi |
| `05 §6.7.6` | Tanınan tarayıcı işareti — rastgele değer, sunucuda hesap başına kayıt; P-48 | UC-55 · UC-110 · konvansiyon — API2-08 | eşlendi |
| `05 §6.7.7` | Ödeme adımında yazılanlar — şifreli ve imzalı form değerleri; sipariş onaylanınca ya da sepet boşalınca silinir | UC-22 · UC-26 · UC-27 | eşlendi |
| `05 §6.7.12` | Google ile girişin akış çerezi — `state`, `nonce`, PKCE; yolu kök; akış kadar ömür | UC-50 · UC-51 · UC-62 · konvansiyon — API2-03 | eşlendi |
| `05 §6.7.8` | Misafir sepeti çerezi 400 gün açılmazsa sunucudaki sepet gecelik temizlikte silinir; kalıcılığın dayanağı satış sözleşmesi | UC-19 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `05 §6.7.9` | Ödeme adımında yazılanlar tarayıcıda oturumluk şifreli çerezde; girişe giden iki öğe ve "Sepete dön" formu gönderen düğmelerdir — alanlar doğrulanmadan çereze yazılır, JavaScript kapalıyken de; sunucuda kayıt oluşmaz (AA-1'in dayanağı; "Sepete dön" GAP-14 ile — K-1161) | UC-22 · UC-26 · UC-25 · konvansiyon — API2-02 · konvansiyon — API8-02 | eşlendi |
| `05 §6.7.10` | Çerez politikası ilk kez yayına alınana kadar vitrin çerez yazmaz — çerez başlıklarını çıkaran ara katman; panel kapsam dışı | konvansiyon — API2-03 | konvansiyon |
| `05 §6.7.11` | Elenenler — SameSite=Strict, adreste sepet kimliği, misafir sepetinin hiç silinmemesi, oturum deposu ve JavaScript, sunucuda geçici kayıt, verinin adreste taşınması | konvansiyon — API2-03 · konvansiyon — API2-01 · konvansiyon — API8-02 | konvansiyon |
| `05 §7.7.1` | Dosyalar sunucunun diskinde, dosya deposu arayüzüyle; görseller herkese açık, parmak izli adresle ters vekilden; dijital dosya genel adresle sunulmaz — indirme sipariş sayfasının erişimi denetlenerek; disk eşiğini aşacak yükleme kabulde reddedilir | UC-37 · UC-117 · UC-83 · konvansiyon — API4-07 | eşlendi |
| `05 §7.7.2` | Görsel sunucuda yeniden kodlanır — kaynak kopya ve WebP türevleri, meta veri atılır; türevler kuyrukta; bağsız ve bir günden eski görsel imhada silinir, açık tutulan formun kaydı silinmiş yüklemeye takılır | UC-117 · konvansiyon — API4-07 · konvansiyon — API6-02 · uç doğurmaz — sistem işi (`05 §5`) | eşlendi |
| `05 §7.7.3` | Dijital dosya — yükleme parçalı, parçanın boyu `07`'de; kesilen yükleme sürer, tamamlanmayan bir gün sonra silinir; her yükleme yeni kayıt; bağsız dosyanın silinmesi | UC-83 · UC-114 · konvansiyon — API4-07 | eşlendi |
| `05 §7.7.4` | İndirmenin sayıldığı an — düğmeye basılıp gönderim başlayınca bir kez, tek koşullu güncellemeyle; bir saat ömürlü imzalı bağlantı, aralık istekleri yeni hak düşürmez; ömür yalnız kabulde denetlenir; bağlantı e-postayla gitmez | UC-36 · UC-37 · konvansiyon — API4-07 | eşlendi |
| `05 §7.7.5` | Elenenler — baştan nesne depolama, veritabanında dosya, süreli imzalı genel adres, kalemin görsel kopyası, görsellerin hiç silinmemesi | konvansiyon — API4-07 | konvansiyon |
| `05 §9.4.1` | Uyarı tek kanaldan — dış erişilebilirlik kontrolü; canlılık ucu uygulama ve veritabanı yanıt veriyorsa 200, durum ucu uyarı koşulunda 503; ikisi de ayrıntı taşımaz | UC-81 · UC-82 · konvansiyon — API4-10 | eşlendi |
| `05 §9.4.2` | Durum ucunun uyarı koşulları — eşikler tasarım değeri, kurulum ayar dosyasında | UC-82 | eşlendi |
| `05 §9.4.3` | Disk doluluğu %85 | UC-82 | eşlendi |
| `05 §9.4.4` | İşlem günlüğünün aktarımı on dakikadır duruyor ya da geride kalıyor | UC-82 | eşlendi |
| `05 §9.4.5` | Sunucu dışındaki son başarılı tam yedek ya da dosya yedeği yirmi altı saatten eski | UC-82 | eşlendi |
| `05 §9.4.6` | Son nabız beş dakikadan eski | UC-82 | eşlendi |
| `05 §9.4.7` | E-posta kanal uyarısı açık | UC-82 | eşlendi |
| `05 §9.4.8` | Son on beş dakikada en az on beklenmeyen hata | UC-82 | eşlendi |
| `05 §9.4.9` | Sitenin sertifikasının bitişine on dört günden az | UC-82 | eşlendi |
| `05 §9.4.16` | İş kuyruğunun en eski işlenmemiş satırı on beş dakikadan eski ya da "başarısız" satır var | UC-82 | eşlendi |
| `05 §9.4.17` | Gecelik imha ve temizliğin son başarılı çalışması yirmi altı saatten eski | UC-82 | eşlendi |
| `05 §9.4.10` | Durumun okunduğu yer — yedeğin durum kaydı, dosya yedeğinin durum dosyası, arşivleme istatistiği, kuyruk, bellek içi hata sayacı; kaynağı okunamayan koşul açık sayılır, 503 | UC-82 · konvansiyon — API4-10 | eşlendi |
| `05 §9.4.11` | Dış kontrol beş dakikada bir durum ucunu yoklar, iki ardışık başarısızlıkta kurulumu yapana e-posta; kurulumu `DEPLOY_RUNBOOK`'ta | UC-82 · uç doğurmaz — kurulum | eşlendi |
| `05 §9.4.12` | Firma bu uyarıları almaz; ürünün içinden kesinti bilgilendirmesi yok | uç doğurmaz — yoktur | uç doğurmaz |
| `05 §9.4.13` | Konteynerler çöktüğünde ve sunucu açıldığında kendiliğinden yeniden başlar | uç doğurmaz — kurulum | uç doğurmaz |
| `05 §9.4.14` | Kalan risk — uyarının geç görülmesi, aktarım penceresi, ücretsiz katmanın koşulları; açık koşul yeni uyarıyı örter | UC-82 · uç doğurmaz — kurulum | eşlendi |
| `05 §9.4.15` | Elenenler — uygulamanın kendisinin e-posta göndermesi, uyarı koşullarının canlılık ucunda olması, ücretli izleme, firmaya uyarı | UC-81 · UC-82 · konvansiyon — API4-10 | eşlendi |
| `05 §10.3.1` | Beklenmeyen hata tek hata yakalama katmanında üç hâle ayrılır — isteğin türüne ve işlemin listesine göre | konvansiyon — API2-07 | konvansiyon |
| `05 §10.3.2` | Sayfa düzeyi — GET'te beklenmeyen hata çerçevenin içinde hata sayfası, 500 | UC-78 · konvansiyon — API2-07 | eşlendi |
| `05 §10.3.3` | İşlem düzeyi — POST'ta beklenmeyen hata işlem geri alınmışsa formun gönderilen değerleriyle yeniden gösterim ve kalıcı mesaj | konvansiyon — API2-07 | konvansiyon |
| `05 §10.3.4` | Sonucu belirsiz — `04` 2.7.3.3'ün işlemleri kodda işaretli; hata nerede olursa olsun sonucu gösteren ekrana yönlendirme; onay anahtarı ikinci denemeyi var olan siparişe bağlar | UC-27 · UC-39 · UC-40 · UC-41 · UC-43 · UC-76 · konvansiyon — API2-07 · konvansiyon — API6-02 | eşlendi |
| `05 §10.3.5` | Kapsam dışı — adı konmuş hatalar kendi mesajını taşır; makine ucunda kaydedilemeyen bildirim 500; geç ödemenin ve kart iadesinin geç sonucunun kalan riski; arka plan işi ekran hâli değil | UC-80 · UC-06 · konvansiyon — API2-07 · konvansiyon — API2-04 · konvansiyon — API2-08 · konvansiyon — API6-02 | eşlendi |
| `05 §10.3.6` | Kayıt — beklenmeyen hata uygulama kaydına bir satır; kullanıcıya hata kodu, istek kimliği ve teknik ayrıntı gösterilmez; son on beş dakikanın sayısı durum ucunun koşulu | UC-82 · konvansiyon — API2-07 | eşlendi |
| `05 §10.3.7` | Elenenler ve kalan durum — ekran başına yakalayıcı, iki yol, başvuru numarası; listede olmayan formun yeniden gönderimi sürüm çakışmasına ya da ikinci kayda düşer | UC-15 · konvansiyon — API2-07 · konvansiyon — API2-04 | eşlendi |

#### 1.1.13 GAP adayları (`AA-n`) — karara bağlandı (2)

**2 (2026-10-18; K-1161).** Tek aday karara bağlandı — AA-1 → **GAP-14** (K-1161, öneriyle kaydedildi — ⚠); düşen aday yok; karar §1.3'te ve karar kaydının §3'ündedir. Adayın satırı (`04` 5.13.19, §1.1.3) "eşlendi" durumunu aldı ve UC-26'yı taşır; "Sepete dön"ü anan üç satır (`04` 3.1.10, 3.3.15, 6.2.13.12) UC-26'yı, `05 §6.7.9`'un satırı kararın özetini aldı; UC-26'nın adı iki hedefi söyler (§1.1.2). 2. oturum yeni aday açmadı: geri izlenebilirlik (§1.2) bir boşluk bulmadı, örneklemin tek ayrışması bir satırın eksik adaylarıdır (§1.2). Aşağıdaki tablo kararın girdisi olarak kalır.

Eşlenmeyen ya da cevabı kaynakta görünmeyen kaynak satırı burada numarasıyla durur (K-1148); aday, dayandığı bölüm tam okunmadan GAP diye yazılmaz ve cevabı kaynakta yazılı çıkan aday boşluk değil hizalama hatasıdır (checklist §2; K-739). Adayın kararı matrisin 2. oturumunundur — karar kaydının §3'üne GAP-14'ten ve §1.3'e (K-1150); bir W konusuna bağlanan aday o konuda karara bağlanır. Karara bağlanan adayın son hücresi kararın GAP numarasını taşır ve satırları §1.1'in tablolarında artık "GAP adayı (AA-n)" durumunu taşımaz (`gap07()`, §1.1.1). **Kapı:** matrisin 2. oturumu — geçildi.

| # | Kaynak satırı | Aday | Dayandığı yerler (tam okundu) | Karar yeri ve seçenekler |
|---|---|---|---|---|
| AA-1 | `04` 5.13.19 (§1.1.3) | Ödeme adımından "Sepete dön"le ayrılan müşterinin yazdıkları "o tarayıcıda ve o ziyaret boyunca durur"; Teknik Mimari'nin çerezi formu yalnız girişe giden iki öğenin — "Giriş yapın" ve hatırlatma — gönderimiyle çereze yazar, "Sepete dön" sadeleşmiş çerçevenin bağlantısıdır ve formu göndermez; betiksiz yolda yazılanların durması bir form gönderimi ister (MK-05) | `04` 5.13.19, 3.1.10, 2.2.7, §3.6.2 · `05 §6.7.7`, §6.7.9 (MK-56) · K-758, K-907 | Matrisin 2. oturumu; konu API2-02 ile bağlıdır (sayfa ve form ucunun biçimi). (a) "Sepete dön" de formu çereze yazıp yönlendiren düğmedir — `05 §6.7.9`'un listesi genişler, uç UC-26'nın ikinci hedefi ya da yeni aday · (b) 5.13.19'un cümlesi girişe ve oturumun yenilenmesine daralır — `04` geri beslenir · (c) yazılanlar yalnız betikle, tarayıcıda tutulur — `05 §6.7.9`'un "JavaScript kapalıyken de" ilkesine aykırıdır **Karar: GAP-14 (K-1161) — seçenek (a).** |

### 1.2 Geri izlenebilirlik (endpoint → kaynak)

**2 (2026-10-18; K-1160).** **Birim ve biçim.** Satırın birimi §1.1.2'nin aday ucudur (`UC-nn`) — uç envanteri yazım turunda doğar (API3-01) ve geri yönün denetimi adayın denetimidir (Veri modeli aşamasının kalıbı, K-1005). **Uç** adayı, adını ve yöntemini taşır. **Beslendiği kaynak** adayın §1.1.3–§1.1.12'deki satır sayısını ve aile dağılımını (`04` · `03` · devir · `05`) ve her ailedeki ilk satırını (`←`; Arayüz Tanımları'nda önce ekran) gösterir; adayın kaynak satırlarının tam listesi tabloda tekrar edilmez, `geri07()` ile üretilir (§1.1.1) — tablo §1.1'in satırlarını ters yönde ikinci kez yazmaz. **Durum** üç değerden biridir: **beslenir** — en az bir Arayüz Tanımları ya da Kullanıcı Akışları satırı (şablonun "ekran aksiyonu / akış adımı") · **beslenir — makine ucu** — K-868'in ve MK-04'ün makine uçları (UC-37, UC-79…UC-83): kaynakları Teknik Mimari'nin MK-04 satırı ve devirlerdir, ekran satırı da taşıyabilirler (API1-04) · **gerekçesiz ekleme** — kaynak satırı olmayan aday. Makine ucu olmayıp yalnız devir ya da Teknik Mimari satırı taşıyan aday ayrıca aranır ve listelenir (`geri07ek()`).

**Sonuç.** 187 adayın hepsi beslenir — 5.051 aday–satır çifti; matrisin 2.648 ileri satırının 2.318'i en az bir aday taşır (§1.1.10'un kimliğiyle giren 14 bildirim satırı dahil); 181 aday "beslenir", 6'sı "beslenir — makine ucu"; makine ucu olmayıp ekran ya da akış satırı taşımayan aday yoktur — canlılık ve durum ucu (UC-81, UC-82) yalnız devir ve Teknik Mimari satırı taşır, ikisi de makine ucudur; gerekçesiz ekleme yoktur. Aday başına satır sayısı 4 (UC-109, yönetici çıkışı) ile 163 (UC-92, sipariş ayrıntısı) arasındadır. Çiftlerin aileye dağılımı: `04` 2.954 · `03` 1.575 · devir 278 · `05` 244. Betik iki yönlü sınandı (§1.1.1).

**Aday uç listesinin kapanışı (K-1160).** Liste 187 adayla kapanır ve uç envanterine (§3) böyle devredilir: 2. oturum aday eklemedi, çıkarmadı, bölmedi. Değişen tek aday UC-26'dır — GAP-14 "Sepete dön"ü onun ikinci hedefi yaptı (adı ve notu); bir ya da iki işleyici olduğu API2-02'nin işidir. Envanter bir adayı bölebilir, birleştirebilir ya da bir işleyiciye çevirebilir; kimlik envantere geçer ve adres değişse de kalır (K-1148, K-1153).

**Gerekçesiz ekleme listesi (checklist §2) — boş.** Kaynak satırı olmayan aday yoktur (betik). 1a'nın kaynak satırını sonraki oturumlara bırakarak açtığı üç makine adayı satırlarını aldı — bildirim ucu (UC-80) 1d'de, canlılık ve durum ucu (UC-81, UC-82) 1f'de; listenin bütün adayları ileri satırlardan beslenir.

**Kesilmiş okumanın örneklemi (checklist §2, okuma derinliği).** Örneklem hücre başına kesilerek okunan satırlar içindir: 1a…1f satırlarını tam okudu ve altısı da bunu kayda yazdı (§1.1.1'in okuma derinliği paragrafları; karar kaydı §16.1'in matris maddesi) — **kesilmiş okumanın kümesi boştur** (Veri modeli aşamasının 2a'sının kaydı, `06 §1.2`). 2. oturum buna ek olarak ileri satırlardan tabakalı bir örneklemi kaynağın tam metnine karşı yeniden okuttu: her oturumdan yedi satır, 42 satır (seçim komutu §1.1.1'de, `ornek07()`). İlk doğrulayıcı 41 satırı tutarlı buldu, biri ayrıştı: `05 §6.4.6`'nın satırı davetin ömrünü çeken üç olayı — geri çekme, aynı adrese yeni davet, gönderenin kaldırılması — anıyor, olayların uçlarını (UC-176, UC-175, UC-177) taşımıyordu; satır düzeltildi (düşük önem — üç aday aynı olayları `03 §1.10.7`'nin satırında zaten taşıyordu). Ayrışma oranı %2,4'tür (1/42) — checklist §2'nin %10 eşiğinin altında; doğrulama bütün kümeye genişlemez.

| Uç | Beslendiği kaynak | Durum |
|---|---|---|
| UC-01 — Ana sayfa (GET) | 35 satır — `04` 29 · `03` 4 · devir 1 · `05` 1. ← `04` E-01, `03 §2.1.1`, K-933, `05 §3.7.2` | beslenir |
| UC-02 — Kategori sayfası (GET) | 39 satır — `04` 28 · `03` 4 · devir 5 · `05` 2. ← `04` E-02, `03 §1.8.1` `Published`, K-1070, `05 §3.7.1` | beslenir |
| UC-03 — Arama sonuçları (GET) | 24 satır — `04` 20 · `03` 4. ← `04` E-03, `03 §1.8.1` `Published` | beslenir |
| UC-04 — Ürün adresi (GET) | 86 satır — `04` 62 · `03` 12 · devir 8 · `05` 4. ← `04` E-04, `03 §1.1` m3, K-568, `05 §3.5.4` | beslenir |
| UC-05 — Sepete ekleme (POST) | 34 satır — `04` 20 · `03` 13 · `05` 1. ← `04` E-04, `03 §1.10.8`, `05 §6.7.5` | beslenir |
| UC-06 — Bulunamayan adres — "Sayfa bulunamadı" yanıtı (her GET'in yanıtı) | 32 satır — `04` 25 · `03` 4 · devir 1 · `05` 2. ← `04` E-06, `03 §1.8.1` `Draft`, K-568, `05 §3.7.1` | beslenir |
| UC-07 — Hizmetlerimiz listesi (GET) | 18 satır — `04` 15 · `03` 1 · devir 2. ← `04` E-07, `03 §2.2.2`, K-1070 | beslenir |
| UC-08 — Referanslarımız listesi (GET) | 19 satır — `04` 15 · `03` 1 · devir 2 · `05` 1. ← `04` E-07, `03 §2.2.2`, K-1070, `05 §3.7.2` | beslenir |
| UC-09 — Hakkımızda (GET) | 28 satır — `04` 22 · `03` 4 · devir 2. ← `04` E-08, `03 §1.8.4` `Draft`, K-1070 | beslenir |
| UC-10 — Hizmet tanıtımı sayfası (GET) | 44 satır — `04` 30 · `03` 7 · devir 4 · `05` 3. ← `04` E-08, `03 §1.8.4` `Draft`, K-1070, `05 §3.5.4` | beslenir |
| UC-11 — Referans iş sayfası (GET) | 44 satır — `04` 30 · `03` 7 · devir 4 · `05` 3. ← `04` E-08, `03 §1.8.4` `Draft`, K-1070, `05 §3.5.4` | beslenir |
| UC-12 — Genel sayfa (GET) | 38 satır — `04` 25 · `03` 6 · devir 4 · `05` 3. ← `04` E-08, `03 §1.8.4` `Draft`, K-1070, `05 §3.5.4` | beslenir |
| UC-13 — Sık sorulan sorular (GET) | 19 satır — `04` 14 · `03` 3 · devir 1 · `05` 1. ← `04` E-09, `03 §2.2.2`, park A5-2, `05 §3.7.2` | beslenir |
| UC-14 — İletişim sayfası (GET) | 43 satır — `04` 34 · `03` 8 · devir 1. ← `04` E-10, `03 §2.2` metin, park A5-2 | beslenir |
| UC-15 — İletişim formunun gönderimi (POST) | 53 satır — `04` 29 · `03` 21 · `05` 3. ← `04` E-10, `03 §1.9.2` `Open`, `05 §6.3.1` | beslenir |
| UC-16 — Aydınlatma metni (GET) | 18 satır — `04` 15 · `03` 3. ← `04` E-11, `03 §2.2.6` | beslenir |
| UC-17 — Çerez politikası (GET) | 15 satır — `04` 14 · `03` 1. ← `04` E-11, `03 §2.2.6` | beslenir |
| UC-18 — İşlem rehberi (GET) | 11 satır — `04` 10 · `03` 1. ← `04` E-11, `03 §2.2.6` | beslenir |
| UC-19 — Sepet (GET) | 62 satır — `04` 39 · `03` 20 · devir 1 · `05` 2. ← `04` E-12, `03 §1.8.3`, park A5-2, `05 §6.7.5` | beslenir |
| UC-20 — Sepet kaleminin adedini değiştirme (POST) | 18 satır — `04` 11 · `03` 3 · devir 3 · `05` 1. ← `04` E-12, `03 §2.3.3`, K-1011, `05 §6.2.4` | beslenir |
| UC-21 — Sepet kalemini çıkarma (POST) | 12 satır — `04` 7 · `03` 1 · devir 3 · `05` 1. ← `04` E-12, `03 §2.3.3`, K-1011, `05 §6.2.4` | beslenir |
| UC-22 — Ödeme adımı (GET) | 89 satır — `04` 56 · `03` 28 · devir 1 · `05` 4. ← `04` E-13, `03 §1.8.3`, K-893, `05 §4.5.1` | beslenir |
| UC-23 — Kuponu uygulama ya da değiştirme (POST) | 21 satır — `04` 12 · `03` 7 · `05` 2. ← `04` E-13, `03 §1.10.2`, `05 §6.3.1` | beslenir |
| UC-24 — Kuponu kaldırma (POST) | 9 satır — `04` 8 · `03` 1. ← `04` E-13, `03 §2.4.4` | beslenir |
| UC-25 — Ödeme adımının ara gönderimi — özetin yeniden hesabı, kayıtlı e-postanın hatırlatması (POST) | 21 satır — `04` 13 · `03` 7 · `05` 1. ← `04` E-13, `03 §2.4.1`, `05 §6.7.9` | beslenir |
| UC-26 — Ödeme adımından girişe ya da sepete — formu çereze yazıp yönlendirme (POST) | 18 satır — `04` 15 · `03` 1 · `05` 2. ← `04` E-13, `03 §2.4.1`, `05 §6.7.7` | beslenir |
| UC-27 — Siparişi onaylama (POST) | 103 satır — `04` 54 · `03` 38 · devir 4 · `05` 7. ← `04` E-13, `03 §1.2` `Placed`, K-579, `05 §3.6.3` | beslenir |
| UC-28 — Sipariş teyit ve bekleme ekranı (GET) | 20 satır — `04` 17 · `03` 3. ← `04` E-14, `03 §2.4.9` | beslenir |
| UC-29 — Ödeme sağlayıcısına geçiş (POST) | 20 satır — `04` 11 · `03` 5 · `05` 4. ← `04` E-14, `03 §2.4.9`, `05 §3.6.2` | beslenir |
| UC-30 — Ödeme sağlayıcısından dönüş (yolu ve yöntemi `08`'in park satırı) | 17 satır — `04` 7 · `03` 4 · devir 3 · `05` 3. ← `04` 5.12.15, `03 §2.5.1.2`, `05 §6.5.1` g1, `05 §3.6.4` | beslenir |
| UC-31 — Sipariş takibi girişi (GET) | 18 satır — `04` 15 · `03` 3. ← `04` E-15, `03 §2.6.2` | beslenir |
| UC-32 — Sipariş takibi sorgusu (POST) | 33 satır — `04` 17 · `03` 11 · devir 1 · `05` 4. ← `04` E-15, `03 §2.6` metin, K-562, `05 §6.2.4` | beslenir |
| UC-33 — Sipariş sayfası (GET) | 162 satır — `04` 90 · `03` 61 · devir 7 · `05` 4. ← `04` E-16, `03 §1.1` m1, K-562, `05 §4.5.1` | beslenir |
| UC-34 — Erişim anahtarlı sipariş adresi (GET) | 51 satır — `04` 20 · `03` 23 · devir 3 · `05` 5. ← `04` E-16, `03 §2.6` metin, K-562, `05 §6.2.4` | beslenir |
| UC-35 — Geri ödeme IBAN'ının girişi ve düzeltmesi (POST) | 55 satır — `04` 18 · `03` 36 · devir 1. ← `04` E-16, `03 §1.7.3.4`, K-874 | beslenir |
| UC-36 — İndirme bağlantısının üretimi (POST) | 18 satır — `04` 8 · `03` 5 · devir 3 · `05` 2. ← `04` E-16, `03 §1.7.2` m2, K-884, `05 §6.4.7` | beslenir |
| UC-37 — İndirme ucu (GET) | 21 satır — `04` 3 · `03` 8 · devir 5 · `05` 5. ← `04` 5.16.13, `03 §1.7.1.11`, K-884, `05 §2.1` MK-04 | beslenir — makine ucu |
| UC-38 — İptal ve gecikme feshi ekranı (GET) | 35 satır — `04` 27 · `03` 5 · devir 3. ← `04` E-17, `03 §2.7.1`, K-795 | beslenir |
| UC-39 — Siparişin iptali (POST) | 31 satır — `04` 15 · `03` 14 · devir 1 · `05` 1. ← `04` E-17, `03 §1.2` S3, K-874, `05 §10.3.4` | beslenir |
| UC-40 — Kalem iptali (POST) | 53 satır — `04` 22 · `03` 27 · devir 3 · `05` 1. ← `04` E-17, `03 §1.2` S2, K-787, `05 §10.3.4` | beslenir |
| UC-41 — Gecikme feshi (POST) | 39 satır — `04` 20 · `03` 17 · devir 1 · `05` 1. ← `04` E-17, `03 §1.2` S4, K-1077, `05 §10.3.4` | beslenir |
| UC-42 — Cayma beyanı ekranı (GET) | 44 satır — `04` 30 · `03` 11 · devir 3. ← `04` E-18, `03 §2.8.1.1`, K-795 | beslenir |
| UC-43 — Cayma beyanı (POST) | 58 satır — `04` 25 · `03` 29 · devir 3 · `05` 1. ← `04` E-18, `03 §1.2` S2, K-787, `05 §10.3.4` | beslenir |
| UC-44 — Ayıp talebi ekranı (GET) | 33 satır — `04` 24 · `03` 6 · devir 3. ← `04` E-19, `03 §1.11.9`, K-795 | beslenir |
| UC-45 — Ayıp talebinin gönderimi (POST) | 30 satır — `04` 14 · `03` 13 · devir 3. ← `04` E-19, `03 §1.7.1.10`, K-787 | beslenir |
| UC-46 — Ayıp talebinin yeniden açılması (POST) | 16 satır — `04` 9 · `03` 7. ← `04` E-19, `03 §1.9.1` | beslenir |
| UC-47 — Kayıt ekranı (GET) | 26 satır — `04` 21 · `03` 5. ← `04` E-20, `03 §3.4.7` | beslenir |
| UC-48 — Kayıt (POST) | 33 satır — `04` 22 · `03` 8 · `05` 3. ← `04` E-20, `03 §3.4.3`, `05 §3.5.2` | beslenir |
| UC-49 — Kayıt doğrulama bağlantısını yeniden isteme (POST) | 23 satır — `04` 11 · `03` 10 · `05` 2. ← `04` E-20, `03 §1.10.5`, `05 §6.3.1` | beslenir |
| UC-50 — Google ile giriş — yönlendirme (GET ya da POST (API2-03)) | 26 satır — `04` 14 · `03` 6 · devir 1 · `05` 5. ← `04` E-22, `03 §6.1.3.5`, K-588, `05 §3.5.2` | beslenir |
| UC-51 — Google dönüşü (GET) | 30 satır — `04` 17 · `03` 9 · devir 1 · `05` 3. ← `04` 5.20.1, `03 §1.10.5`, K-588, `05 §6.1.4` | beslenir |
| UC-52 — Doğrulama bağlantısının inişi — kayıt ve yeni e-posta (GET) | 38 satır — `04` 21 · `03` 8 · devir 4 · `05` 5. ← `04` E-21, `03 §3.4.1`, K-904, `05 §6.4.4` | beslenir |
| UC-53 — Doğrulama (POST) | 28 satır — `04` 14 · `03` 8 · devir 4 · `05` 2. ← `04` E-21, `03 §1.10.5`, K-904, `05 §6.4.6` | beslenir |
| UC-54 — Müşteri girişi ekranı (GET) | 36 satır — `04` 33 · `03` 2 · `05` 1. ← `04` E-22, `03 §9.2.1`, `05 §6.5.6` | beslenir |
| UC-55 — Müşteri girişi (POST) | 59 satır — `04` 29 · `03` 13 · devir 5 · `05` 12. ← `04` E-22, `03 §1.10.6`, K-588, `05 §3.5.2` | beslenir |
| UC-56 — Şifre sıfırlama — isteme adımı (GET) | 19 satır — `04` 18 · `03` 1. ← `04` E-23, `03 §9.2.4` | beslenir |
| UC-57 — Şifre sıfırlama isteği (POST) | 24 satır — `04` 11 · `03` 12 · `05` 1. ← `04` E-23, `03 §3.4.4`, `05 §6.3.1` | beslenir |
| UC-58 — Sıfırlama bağlantısının inişi — kurma adımı (GET) | 26 satır — `04` 13 · `03` 7 · devir 2 · `05` 4. ← `04` E-23, `03 §3.4.5`, K-904, `05 §6.4.4` | beslenir |
| UC-59 — Yeni şifrenin kurulması (POST) | 22 satır — `04` 8 · `03` 10 · devir 1 · `05` 3. ← `04` E-23, `03 §1.10.6`, K-904, `05 §6.1.2` | beslenir |
| UC-60 — Yeniden doğrulama ekranı (GET) | 21 satır — `04` 16 · `03` 2 · devir 1 · `05` 2. ← `04` E-24, `03 §6.2.5.1`, K-1062, `05 §6.1.5` | beslenir |
| UC-61 — Şifreyle yeniden doğrulama (POST) | 24 satır — `04` 12 · `03` 5 · devir 3 · `05` 4. ← `04` E-24, `03 §3.4.17`, K-971, `05 §6.1.1` | beslenir |
| UC-62 — Google ile yeniden doğrulama — yönlendirme ve dönüş (GET) | 22 satır — `04` 14 · `03` 2 · devir 1 · `05` 5. ← `04` E-24, `03 §6.2.5.1`, K-971, `05 §6.1.5` | beslenir |
| UC-63 — Hesap — profil ve güvenlik (GET) | 37 satır — `04` 31 · `03` 5 · `05` 1. ← `04` E-25, `03 §3.4.9`, `05 §6.2.3` | beslenir |
| UC-64 — Adı değiştirme (POST) | 10 satır — `04` 9 · `03` 1. ← `04` E-25, `03 §9.3.1` | beslenir |
| UC-65 — Şifreyi değiştirme (POST) | 16 satır — `04` 8 · `03` 5 · devir 1 · `05` 2. ← `04` E-25, `03 §1.10.6`, K-971, `05 §6.1.2` | beslenir |
| UC-66 — E-postayı değiştirme (POST) | 17 satır — `04` 10 · `03` 5 · devir 1 · `05` 1. ← `04` E-25, `03 §1.10.5`, K-971, `05 §6.1.5` | beslenir |
| UC-67 — Bekleyen e-posta değişikliğinin bağlantısını yeniden isteme (POST) | 15 satır — `04` 8 · `03` 6 · `05` 1. ← `04` E-25, `03 §3.1.1`, `05 §6.3.2` | beslenir |
| UC-68 — Hesabı silme (POST) | 21 satır — `04` 7 · `03` 11 · devir 1 · `05` 2. ← `04` E-25, `03 §1.6.1` metin, K-971, `05 §6.1.2` | beslenir |
| UC-69 — Hesap silmenin sonuç hâli (GET) | 6 satır — `04` 5 · `03` 1. ← `04` E-25, `03 §9.4.2` | beslenir |
| UC-70 — Adres defteri (GET) | 16 satır — `04` 14 · `03` 1 · `05` 1. ← `04` E-26, `03 §9.3.2`, `05 §6.2.3` | beslenir |
| UC-71 — Adres ekleme (POST) | 13 satır — `04` 12 · `03` 1. ← `04` E-26, `03 §9.3.2` | beslenir |
| UC-72 — Adres düzenleme (POST) | 16 satır — `04` 11 · `03` 1 · devir 3 · `05` 1. ← `04` E-26, `03 §9.3.2`, K-1011, `05 §6.2.4` | beslenir |
| UC-73 — Adres silme (POST) | 9 satır — `04` 4 · `03` 1 · devir 3 · `05` 1. ← `04` E-26, `03 §9.3.2`, K-1011, `05 §6.2.4` | beslenir |
| UC-74 — Sipariş geçmişi (GET) | 31 satır — `04` 23 · `03` 7 · `05` 1. ← `04` E-27, `03 §2.6` metin, `05 §6.2.3` | beslenir |
| UC-75 — Geri alma bağlantısının inişi (GET) | 25 satır — `04` 13 · `03` 5 · devir 3 · `05` 4. ← `04` E-28, `03 §3.4.13`, K-904, `05 §6.4.4` | beslenir |
| UC-76 — E-posta değişikliğini geri alma (POST) | 24 satır — `04` 10 · `03` 8 · devir 2 · `05` 4. ← `04` E-28, `03 §1.10.5`, K-904, `05 §6.1.2` | beslenir |
| UC-77 — Müşteri çıkışı (POST) | 6 satır — `04` 3 · `03` 2 · devir 1. ← `04` 3.1.8, `03 §2.3.7`, K-899 | beslenir |
| UC-78 — Beklenmeyen hata sayfası (sayfa isteğinin yanıtı) | 37 satır — `04` 34 · devir 2 · `05` 1. ← `04` 5.1.12, K-914, `05 §10.3.2` | beslenir |
| UC-79 — Site haritası ve robots.txt (GET) | 8 satır — `04` 2 · devir 3 · `05` 3. ← `04` 5.5.4, K-868, `05 §2.1` MK-04 | beslenir — makine ucu |
| UC-80 — Ödeme sağlayıcısının bildirim ucu (POST) | 51 satır — `03` 27 · devir 14 · `05` 10. ← `03 §1.2` S1, K-868, `05 §2.1` MK-04 | beslenir — makine ucu |
| UC-81 — Canlılık ucu (GET) | 7 satır — devir 3 · `05` 4. ← K-918, `05 §2.1` MK-04 | beslenir — makine ucu |
| UC-82 — Durum ucu (GET) | 21 satır — devir 3 · `05` 18. ← K-918, `05 §2.1` MK-04 | beslenir — makine ucu |
| UC-83 — Parçalı dosya yükleme ucu (POST) | 38 satır — `04` 11 · `03` 4 · devir 16 · `05` 7. ← `04` E-33, `03 §3.5.1.6`, K-868, `05 §2.1` MK-04 | beslenir — makine ucu |
| UC-84 — Panel girişi ve şifre sıfırlama (GET) | 44 satır — `04` 35 · `03` 4 · devir 1 · `05` 4. ← `04` E-29, `03 §7.1.50`, K-912, `05 §3.5.3` | beslenir |
| UC-85 — Yönetici daveti kabul ekranı (GET) | 29 satır — `04` 15 · `03` 6 · devir 2 · `05` 6. ← `04` E-30, `03 §1.10.7`, K-904, `05 §3.5.3` | beslenir |
| UC-86 — Panel ana sayfası (GET) | 68 satır — `04` 39 · `03` 26 · devir 2 · `05` 1. ← `04` E-31, `03 §1.7.3` metin, K-932, `05 §3.5.1` | beslenir |
| UC-87 — Ürün listesi (GET) | 22 satır — `04` 18 · `03` 2 · devir 2. ← `04` E-32, `03 §8.1` metin, K-737 | beslenir |
| UC-88 — Ürün formu (GET) | 41 satır — `04` 36 · `03` 5. ← `04` E-33, `03 §1.1` m3 | beslenir |
| UC-89 — Kategori ağacı (GET) | 13 satır — `04` 11 · `03` 2. ← `04` E-34, `03 §8.1` metin | beslenir |
| UC-90 — Kuponlar (GET) | 16 satır — `04` 13 · `03` 3. ← `04` E-35, `03 §8.1` metin | beslenir |
| UC-91 — Sipariş listesi (GET) | 64 satır — `04` 43 · `03` 19 · devir 2. ← `04` E-36, `03 §1.7.3`, K-802 | beslenir |
| UC-92 — Sipariş ayrıntısı (GET) | 163 satır — `04` 83 · `03` 76 · devir 4. ← `04` E-37, `03 §1.1` m1, K-795 | beslenir |
| UC-93 — Talepler (GET) | 34 satır — `04` 28 · `03` 6. ← `04` E-38, `03 §1.1` m5 | beslenir |
| UC-94 — İletişim talebi ayrıntısı (GET) | 24 satır — `04` 18 · `03` 6. ← `04` E-39, `03 §1.1` m6 | beslenir |
| UC-95 — Üye kaydı görünümü (GET) | 32 satır — `04` 29 · `03` 3. ← `04` E-40, `03 §3.4.15` | beslenir |
| UC-96 — Kurumsal içerik (GET) | 33 satır — `04` 30 · `03` 3. ← `04` E-41, `03 §1.1` m4 | beslenir |
| UC-97 — Ana sayfa ve menü (GET) | 14 satır — `04` 13 · `03` 1. ← `04` E-42, `03 §8.6.2.1` | beslenir |
| UC-98 — Marka (GET) | 18 satır — `04` 16 · `03` 2. ← `04` E-43, `03 §8.6` metin | beslenir |
| UC-99 — Firma kimliği ve satış (GET) | 40 satır — `04` 37 · `03` 3. ← `04` E-44, `03 §3.5.4.13` | beslenir |
| UC-100 — Ödeme yöntemleri (GET) | 29 satır — `04` 26 · `03` 3. ← `04` E-45, `03 §3.5.4.3` | beslenir |
| UC-101 — Kargo, süreler ve varsayılanlar (GET) | 32 satır — `04` 29 · `03` 3. ← `04` E-46, `03 §3.5.4.13` | beslenir |
| UC-102 — Yasal metinler (GET) | 29 satır — `04` 27 · `03` 2. ← `04` E-47, `03 §8.7` metin | beslenir |
| UC-103 — Yönetici hesapları (GET) | 22 satır — `04` 16 · `03` 6. ← `04` E-49, `03 §1.7.3.1` | beslenir |
| UC-104 — Yöneticinin kendi hesabı (GET) | 24 satır — `04` 23 · `03` 1. ← `04` E-50, `03 §9.3.7` | beslenir |
| UC-105 — Satış özeti (GET) | 23 satır — `04` 21 · `03` 2. ← `04` E-51, `03 §8.9` metin | beslenir |
| UC-106 — İşlem izi (GET) | 32 satır — `04` 22 · `03` 10. ← `04` E-52, `03 §2.10.5` m5 | beslenir |
| UC-107 — Dışa aktarma (GET) | 13 satır — `04` 11 · `03` 2. ← `04` E-53, `03 §8.9` metin | beslenir |
| UC-108 — Yönetici hesabının doğrulama ekranları (GET) | 31 satır — `04` 20 · `03` 3 · devir 2 · `05` 6. ← `04` E-54, `03 §7.1.51`, K-904, `05 §3.5.3` | beslenir |
| UC-109 — Yönetici çıkışı (POST) | 4 satır — `04` 3 · devir 1. ← `04` 3.2.22, K-899 | beslenir |
| UC-110 — Panel girişi (POST) | 35 satır — `04` 13 · `03` 8 · devir 5 · `05` 9. ← `04` E-29, `03 §1.10.6`, K-588, `05 §3.5.2` | beslenir |
| UC-111 — Panel şifre sıfırlama isteği (POST) | 14 satır — `04` 9 · `03` 3 · devir 1 · `05` 1. ← `04` E-29, `03 §6.1.1.2`, K-588, `05 §6.3.1` | beslenir |
| UC-112 — Panelde yeni şifrenin kurulması (POST) | 14 satır — `04` 7 · `03` 6 · `05` 1. ← `04` E-29, `03 §1.10.6`, `05 §6.1.2` | beslenir |
| UC-113 — Davetle yönetici hesabını açma (POST) | 21 satır — `04` 11 · `03` 6 · devir 2 · `05` 2. ← `04` E-30, `03 §1.10.7`, K-588, `05 §6.4.6` | beslenir |
| UC-114 — Ürünü kaydetme (POST) | 76 satır — `04` 44 · `03` 22 · devir 8 · `05` 2. ← `04` E-33, `03 §1.8.2`, K-873, `05 §6.5.3` | beslenir |
| UC-115 — Ürünün ya da varyantın yayın geçişi (POST) | 24 satır — `04` 13 · `03` 11. ← `04` E-33, `03 §1.8.1` | beslenir |
| UC-116 — Ürünü ya da varyantı kalıcı silme (POST) | 16 satır — `04` 10 · `03` 6. ← `04` E-33, `03 §1.4.4.9` | beslenir |
| UC-117 — Görsel yükleme (POST) | 29 satır — `04` 17 · `03` 3 · devir 5 · `05` 4. ← `04` E-33, `03 §8.1.5`, K-902, `05 §3.6.5` | beslenir |
| UC-118 — Kategori ekleme (POST) | 11 satır — `04` 10 · `03` 1. ← `04` E-34, `03 §8.1.14` | beslenir |
| UC-119 — Kategoriyi yeniden adlandırma (POST) | 8 satır — `04` 7 · `03` 1. ← `04` E-34, `03 §8.1.14` | beslenir |
| UC-120 — Kategoriyi taşıma (POST) | 9 satır — `04` 7 · `03` 2. ← `04` E-34, `03 §3.5.1.3` | beslenir |
| UC-121 — Kategoriyi silme (POST) | 7 satır — `04` 5 · `03` 2. ← `04` E-34, `03 §3.5.1.2` | beslenir |
| UC-122 — Kuponu kaydetme (POST) | 26 satır — `04` 17 · `03` 6 · devir 3. ← `04` E-35, `03 §1.10.2`, K-873 | beslenir |
| UC-123 — Havale siparişini "Ödendi" işaretleme (POST) | 34 satır — `04` 3 · `03` 29 · devir 2. ← `04` 9.9.5, `03 §1.2` S1, K-874 | beslenir |
| UC-124 — Kargoya verme (POST) | 22 satır — `04` 8 · `03` 14. ← `04` 9.9.5, `03 §1.2` S5 | beslenir |
| UC-125 — Kargoya yeniden verme (POST) | 16 satır — `04` 8 · `03` 8. ← `04` 9.9.5, `03 §1.2` S8 | beslenir |
| UC-126 — Teslim işareti ve tarihi (POST) | 25 satır — `04` 7 · `03` 18. ← `04` 9.9.5, `03 §1.2` S6 | beslenir |
| UC-127 — "Teslim edilemedi" işareti (POST) | 15 satır — `04` 3 · `03` 12. ← `04` 9.9.5, `03 §1.2` S7 | beslenir |
| UC-128 — Teslim edilemedi siparişini kapatma (POST) | 11 satır — `04` 3 · `03` 8. ← `04` 9.9.5, `03 §1.2` S9 | beslenir |
| UC-129 — Firma iptali (POST) | 65 satır — `04` 19 · `03` 44 · devir 2. ← `04` 9.9.5, `03 §1.2` S2, K-794 | beslenir |
| UC-130 — Başka kanaldan gelen cayma ya da gecikme feshi bildiriminin kaydı (POST) | 46 satır — `04` 16 · `03` 30. ← `04` 9.9.5, `03 §1.2` S2 | beslenir |
| UC-131 — Geri ödemeyi işleme (POST) | 45 satır — `04` 11 · `03` 34. ← `04` 9.9.21, `03 §1.3` Ö3 | beslenir |
| UC-132 — "Havale gerçekleşmedi" (POST) | 11 satır — `04` 4 · `03` 7. ← `04` 9.9.21, `03 §1.7.1.9` | beslenir |
| UC-133 — Tutar bazlı kısmi geri ödeme (POST) | 22 satır — `04` 8 · `03` 14. ← `04` 9.9.5, `03 §1.3` Ö3 | beslenir |
| UC-134 — IBAN isteme (POST) | 25 satır — `04` 14 · `03` 11. ← `04` 9.9.5, `03 §1.7.1.9` | beslenir |
| UC-135 — Kart iadesini yeniden deneme (POST) | 14 satır — `04` 6 · `03` 8. ← `04` 9.9.4, `03 §1.7.3.3` | beslenir |
| UC-136 — Havale yolunu açma (POST) | 15 satır — `04` 6 · `03` 9. ← `04` 9.9.4, `03 §1.7.1.9` | beslenir |
| UC-137 — Sonucu beklenen kart iadesini "gerçekleşmedi" işaretleme (POST) | 16 satır — `04` 6 · `03` 9 · devir 1. ← `04` 9.9.4, `03 §1.7.3.3`, K-978 | beslenir |
| UC-138 — Kalem çıkarma ya da adet azaltma (POST) | 29 satır — `04` 11 · `03` 16 · devir 2. ← `04` 9.9.25, `03 §1.2` S2, K-794 | beslenir |
| UC-139 — Hizmet kalemini tamamlama (POST) | 25 satır — `04` 8 · `03` 17. ← `04` 9.9.5, `03 §1.2` S2 | beslenir |
| UC-140 — İade malını teslim alma ya da reddetme (POST) | 62 satır — `04` 17 · `03` 43 · devir 2. ← `04` 9.9.27, `03 §1.4.4.5`, K-794 | beslenir |
| UC-141 — İade malını stoğa ekleme (POST) | 17 satır — `04` 6 · `03` 10 · devir 1. ← `04` 9.9.27, `03 §1.4.4.6`, K-794 | beslenir |
| UC-142 — "Mal dönmedi" kapanışı (POST) | 23 satır — `04` 6 · `03` 15 · devir 2. ← `04` 9.9.28, `03 §1.7.1.8`, K-794 | beslenir |
| UC-143 — İndirme hakkını yenileme (POST) | 11 satır — `04` 3 · `03` 8. ← `04` 9.9.29, `03 §1.7.1.11` | beslenir |
| UC-144 — Ayıp talebini çözüldü işaretleme (POST) | 9 satır — `04` 4 · `03` 5. ← `04` 9.9.7, `03 §1.9.1` `Open` | beslenir |
| UC-145 — Ayıp talebini yeniden açma — panel (POST) | 9 satır — `04` 4 · `03` 5. ← `04` 9.9.7, `03 §1.9.1` | beslenir |
| UC-146 — Adres düzeltmesi (POST) | 19 satır — `04` 10 · `03` 9. ← `04` 9.9.5, `03 §1.6.2` metin | beslenir |
| UC-147 — Kargo şirketi ve takip numarası düzeltmesi (POST) | 12 satır — `04` 7 · `03` 5. ← `04` 9.9.5, `03 §1.6.2` metin | beslenir |
| UC-148 — Teslim tarihi düzeltmesi (POST) | 15 satır — `04` 7 · `03` 8. ← `04` 9.9.5, `03 §1.6.2` metin | beslenir |
| UC-149 — Ulaşma tarihi düzeltmesi (POST) | 11 satır — `04` 5 · `03` 6. ← `04` 9.9.31, `03 §1.6.2` metin | beslenir |
| UC-150 — Durum geçişi düzeltmesi (POST) | 31 satır — `04` 11 · `03` 20. ← `04` 9.9.5, `03 §1.2` `Delivered` | beslenir |
| UC-151 — Misafir siparişinin e-postasını düzeltme (POST) | 21 satır — `04` 7 · `03` 13 · `05` 1. ← `04` 9.9.5, `03 §1.7.3.6`, `05 §6.4.3` | beslenir |
| UC-152 — İç notu kaydetme (POST) | 15 satır — `04` 6 · `03` 7 · devir 2. ← `04` 9.9.11, `03 §1.11.27`, K-1051 | beslenir |
| UC-153 — Ulaşmayan e-postayı yeniden gönderme (POST) | 17 satır — `04` 5 · `03` 11 · devir 1. ← `04` 9.9.4, `03 §1.7.3.1`, K-1066 | beslenir |
| UC-154 — Ulaşmayan bütün e-postaları yeniden gönderme (POST) | 11 satır — `04` 4 · `03` 6 · devir 1. ← `04` 9.9.4, `03 §1.7.3.1`, K-1066 | beslenir |
| UC-155 — İletişim talebini kapatma (POST) | 9 satır — `04` 5 · `03` 4. ← `04` E-39, `03 §1.9.2` `Open` | beslenir |
| UC-156 — İletişim talebini yeniden açma (POST) | 7 satır — `04` 5 · `03` 2. ← `04` E-39, `03 §1.9.2` `Closed` | beslenir |
| UC-157 — Üye hesabını silme — panel (POST) | 17 satır — `04` 8 · `03` 9. ← `04` E-40, `03 §1.6.1.8` | beslenir |
| UC-158 — İçerik kaydını kaydetme (POST) | 50 satır — `04` 30 · `03` 12 · devir 7 · `05` 1. ← `04` E-41, `03 §1.8.4` `Published`, K-873, `05 §6.5.3` | beslenir |
| UC-159 — İçerik kaydının yayın geçişi (POST) | 28 satır — `04` 11 · `03` 17. ← `04` E-41, `03 §1.8.4` | beslenir |
| UC-160 — İçerik kaydını kalıcı silme (POST) | 8 satır — `04` 5 · `03` 3. ← `04` E-41, `03 §1.8.4` metin | beslenir |
| UC-161 — İçerik kaydının elle sırası (POST) | 7 satır — `04` 4 · `03` 1 · devir 2. ← `04` E-41, `03 §8.6.1.5`, K-1063 | beslenir |
| UC-162 — Ana sayfa ve menü ayarlarını kaydetme (POST) | 22 satır — `04` 16 · `03` 3 · devir 3. ← `04` E-42, `03 §8.6.2.1`, K-1014 | beslenir |
| UC-163 — Marka ayarlarını kaydetme (POST) | 30 satır — `04` 19 · `03` 5 · devir 6. ← `04` E-43, `03 §3.5.4.6`, K-873 | beslenir |
| UC-164 — Firma kimliğini kaydetme (POST) | 35 satır — `04` 28 · `03` 3 · devir 4. ← `04` E-44, `03 §3.5.4.1`, K-873 | beslenir |
| UC-165 — Satışı geçici olarak kapatma (POST) | 16 satır — `04` 9 · `03` 5 · devir 2. ← `04` E-44, `03 §1.10.8`, K-1021 | beslenir |
| UC-166 — Satışı yeniden açma (POST) | 9 satır — `04` 5 · `03` 2 · devir 2. ← `04` E-44, `03 §1.10.8`, K-1021 | beslenir |
| UC-167 — Ödeme yöntemlerini kaydetme — havale anahtarı ve IBAN (POST) | 31 satır — `04` 17 · `03` 11 · devir 3. ← `04` E-45, `03 §3.5.4.2`, K-1014 | beslenir |
| UC-168 — Kargo ücreti ve eşikleri kaydetme (POST) | 16 satır — `04` 11 · `03` 2 · devir 3. ← `04` E-46, `03 §8.7.2.3`, K-1014 | beslenir |
| UC-169 — Teslimat illerini kaydetme (POST) | 14 satır — `04` 10 · `03` 2 · devir 2. ← `04` E-46, `03 §8.7.2.3`, K-1014 | beslenir |
| UC-170 — Süreleri kaydetme (POST) | 15 satır — `04` 10 · `03` 3 · devir 2. ← `04` E-46, `03 §3.5.4.5`, K-1014 | beslenir |
| UC-171 — Ürün varsayılanlarını kaydetme (POST) | 13 satır — `04` 9 · `03` 2 · devir 2. ← `04` E-46, `03 §3.5.4.5`, K-1014 | beslenir |
| UC-172 — İade adresini kaydetme (POST) | 19 satır — `04` 13 · `03` 4 · devir 2. ← `04` E-46, `03 §3.5.4.10`, K-1014 | beslenir |
| UC-173 — Yasal metnin taslağını kaydetme (POST) | 18 satır — `04` 12 · `03` 2 · devir 3 · `05` 1. ← `04` E-47, `03 §3.5.4.10`, K-873, `05 §6.5.3` | beslenir |
| UC-174 — Yasal metni yayına alma (POST) | 13 satır — `04` 11 · `03` 2. ← `04` E-47, `03 §3.5.4.10` | beslenir |
| UC-175 — Yönetici daveti gönderme (POST) | 24 satır — `04` 13 · `03` 10 · `05` 1. ← `04` E-49, `03 §1.10.7`, `05 §6.4.6` | beslenir |
| UC-176 — Daveti geri çekme (POST) | 11 satır — `04` 5 · `03` 5 · `05` 1. ← `04` E-49, `03 §1.10.7`, `05 §6.4.6` | beslenir |
| UC-177 — Yöneticiyi kaldırma (POST) | 22 satır — `04` 9 · `03` 10 · devir 1 · `05` 2. ← `04` E-49, `03 §1.10.5`, K-971, `05 §6.1.2` | beslenir |
| UC-178 — Yöneticinin adını değiştirme (POST) | 9 satır — `04` 8 · `03` 1. ← `04` E-50, `03 §9.3.7` | beslenir |
| UC-179 — Yöneticinin e-postasını değiştirme (POST) | 11 satır — `04` 8 · `03` 1 · devir 1 · `05` 1. ← `04` E-50, `03 §9.3.7`, K-971, `05 §6.1.5` | beslenir |
| UC-180 — Yönetici e-posta değişikliğinin bağlantısını yeniden isteme (POST) | 11 satır — `04` 7 · `03` 3 · `05` 1. ← `04` E-50, `03 §3.1.1`, `05 §6.3.2` | beslenir |
| UC-181 — Yöneticinin şifresini değiştirme (POST) | 13 satır — `04` 8 · `03` 2 · devir 1 · `05` 2. ← `04` E-50, `03 §1.10.6`, K-971, `05 §6.1.2` | beslenir |
| UC-182 — Siparişlerin dışa aktarılması (GET ya da POST (API9-05)) | 15 satır — `04` 12 · `03` 3. ← `04` E-53, `03 §6.3.3.1` | beslenir |
| UC-183 — Üye listesinin dışa aktarılması (GET ya da POST (API9-05)) | 10 satır — `04` 8 · `03` 2. ← `04` E-53, `03 §6.3.3.1` | beslenir |
| UC-184 — İletişim taleplerinin dışa aktarılması (GET ya da POST (API9-05)) | 10 satır — `04` 8 · `03` 2. ← `04` E-53, `03 §6.3.3.1` | beslenir |
| UC-185 — Yöneticinin şifreyle yeniden doğrulaması (POST) | 20 satır — `04` 12 · `03` 2 · devir 3 · `05` 3. ← `04` E-54, `03 §6.1.1.1`, K-971, `05 §6.1.5` | beslenir |
| UC-186 — Yöneticinin yeni adresinin doğrulanması (POST) | 12 satır — `04` 5 · `03` 3 · devir 2 · `05` 2. ← `04` E-54, `03 §1.10.5`, K-904, `05 §6.4.6` | beslenir |
| UC-187 — Yöneticinin e-posta değişikliğini geri alması (POST) | 15 satır — `04` 5 · `03` 5 · devir 2 · `05` 3. ← `04` E-54, `03 §1.10.5`, K-904, `05 §6.1.2` | beslenir |

### 1.3 Boşluklar (GAP) ve kararlar

**Kapı: matrisin 2. oturumu — geçildi.** GAP kararları buraya ve karar kaydının §3'üne yazılır; "#" sütunu karar kaydının GAP ailesini GAP-14'ten sürdürür (K-1148). Adaylar §1.1.13'tedir.

**2 (2026-10-18; K-1161).** Tek aday karara bağlandı, düşen aday yok. Karar bir W konusuna bağlanmadı: "Sepete dön"de yazılanların durması Arayüz Tanımları ile Teknik Mimari arasındaki bir ayrışmadır ve ürün davranışıdır — matrisin GAP satırında verildi; UC-26'nın bir ya da iki işleyici olduğu API2-02'nin işidir (karar kaydı §16.3). Matris tamamlandı: checklist §2'nin son iki maddesi — GAP'ler karara bağlandı, çıktı üretimine geçilir — karşılandı; §3'ün (uç envanteri) kapısı açıktır (şablonun başlık notu); sıradaki adım workshop'tur (K-1146).

| # | Boşluk | Karar | Nereye yansıdı |
|---|---|---|---|
| GAP-14 | Ödeme adımından "Sepete dön"le ayrılan müşterinin yazdıkları Arayüz Tanımları'na göre "o tarayıcıda ve o ziyaret boyunca durur" (`04` 5.13.19); Teknik Mimari'nin çerezi formu yalnız girişe giden iki öğenin gönderimiyle yazıyordu ve "Sepete dön" formu göndermeyen bir bağlantıydı — betiksiz yolda yazılanlar kaybolurdu (aday AA-1) | **K-1161 (öneriyle kaydedildi — ⚠):** "Sepete dön" de ödeme adımının formunu gönderen ve bağlantı gibi görünen düğmedir — gönderim alanları doğrulamadan çereze yazar ve sepete yönlendirir, JavaScript kapalıyken de. Çerezin işlevi, ömrü ve dayanağı değişmez: sunucuda kayıt oluşmaz, sipariş onaylanınca ya da sepet boşalınca silinir, onay gerektirmez. Logo ve altbilgi bağlantıları formu göndermez. Uç: UC-26'nın ikinci hedefi | `05` 6.7.9 ve §6.7'nin Kaynak satırı (v0.26) · `02 §12.2.5` ve §12'nin Kaynak satırı (v0.90) · `12` park satırı (K-904 · K-907 · K-908 · K-971 · K-1161) · §1.1.2 (UC-26), §1.1.3, §1.1.4, §1.1.7, §1.1.12 (beş satır), §1.1.13, §1.2 · karar kaydı §3, K-907 (geri işaret) |

**Üst dokümanlara geri besleme** (K-1147). Matrisin, workshop'un ve yazımın alıp Ürün Gereksinimleri'ne, Kullanıcı Akışları'na, Arayüz Tanımları'na, Teknik Mimari'ye, Veri Modeli'ne ya da MVP Kapsamı'na dönen her karar bu tabloya bir satırla girer (K-652; `checklists/document-stage.md` §3). Üst dokümanların ✓ durumu korunur, kalite döngüleri yeniden açılmaz; her birinin başlık notunda ve dipnotunda sürüm satırı vardır. Geri besleme matrisin kümesini değiştirirse kapsama betikleri yeniden koşar (checklist §2; §1.1.1'in 2. oturum paragrafı).

| Bulgu | Karar | Değişen bölümler | Sürüm |
|---|---|---|---|
| GAP-14 — "Sepete dön"de ödeme adımında yazılanlar (matrisin 2. oturumu) | K-1161 (öneriyle kaydedildi — ⚠) | `05` 6.7.9; §6.7'nin Kaynak satırı · `02 §12.2.5`; §12'nin Kaynak satırı · `12` park satırı (2. kol ve etiket) | Teknik Mimari v0.26 · Ürün Gereksinimleri v0.90 · Doğrulama Protokolü şablonu — sürüm alanı değişmez |

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

*Shopfolio — API Design v0.7*
