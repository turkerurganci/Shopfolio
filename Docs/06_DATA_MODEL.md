# Shopfolio — Data Model

**Versiyon: v0.2** | **Bağımlılıklar:** `02`, `03`, `05`, `09`, `10_MVP_SCOPE.md` | **Son güncelleme:** 2026-10-17

> **Aşama:** 5 — Veri Modeli · **Rol:** Data / Domain Modeler
> **Traceability zorunlu: EVET** — §1 tamamlanmadan §3'e (entity envanteri) geçilmez.

> **Aşama 1'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36.
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-06 · K-97 · Aktörler ve yetki derinliği:** Dört aktör — ziyaretçi · misafir alıcı · üye müşteri · firma yöneticisi (`02 §1.3`; misafir alıcıyı K-97 ekledi, satır Aşama 2'nin çakışma taramasında hizalandı); yönetim tarafı **tek rol, çoklu kullanıcı** — yönetici hesapları arasında yetki farkı yoktur, rol/izin modeli buna göre kurulur.
> - **K-17 · K-531 · K-977 · K-988 · Terim sözlüğü:** Sözlük `02 §1`'de yaşar (Türkçe terim · İngilizce kod karşılığı · tanım). Bu doküman sözlüğü **birebir devralır, isim icat etmez**; bir kavramın tek adı vardır. **Tek istisna:** kapalı listelerin — Firma tipi, Hesap türü, Konu tipi, İptal sebebi, Düzeltme sebebi, Cayma istisnası sebebi, Ana sayfa düzeni, Teslim tarihi — değerlerinin kod adlarını bu doküman verir; değerler `02` gövdesindeki kapalı listeden birebir adlandırılır (K-531, K-617, `02 §1.2`; Hesap türü'nü K-617 ekledi, satır Aşama 3'ün çakışma taramasında hizalandı). **Sözlük ürünün terimlerini taşır:** biçimini Teknik Mimari'nin kurduğu, sözlükte (`02 §1.2`) yer almayan teknik kavramların — adı ürün kuralında geçenler dahil: iş kuyruğu ve iş satırı, gönderim kaydı, gelen bildirim kaydı, geri ödeme kaydı, kanal hâli kaydı, nabız ve kesinti kaydı, deneme kaydı, bağlantı kaydı, onay anahtarı, imha kaydı, geçiş kaydı, sürüm değeri — ve onların kapalı listelerinin — geri ödeme kaydının, bağlantı kaydının ve iş satırının hâlleri, gönderim sonucunun sınıfı — kod adlarını bu doküman kavramın `05`'teki adından birebir verir; API Tasarımı ve Kodlama Kılavuzu onları buradan alır. Ürüne görünen kavram sözlüğe girer (`02 §1.1`; K-977 — Aşama 4'ün çakışma taraması; tanım K-988 ile Aşama 4'ün checkpoint'inde hizalandı).

> **Aşama 2'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §8.3 AK0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **Dizin — Aşama 2 kararlarının bu dokümana devirleri:** karar kaydında K-647…K-718'in etki sütunlarında bu dokümanı gösteren yirmi üç atıf (yirmi üç karar) [`CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md) §6.1'dedir; etki sütununda yalnız doküman numarası taşıyan on altı atfın işinin adını dizin verir. Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Aşama 1'in dizini: `PHASE1_CONFLICT_SCAN.md` §6.

> **Aşama 3'ten park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2 ve §3, mekanizma: K-36 (yeri: §10.3 UI0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-787 · K-790 · K-793 · K-794 · Kalemde işlem görmüş adet:** sipariş kaleminin kayıtları — iptal, çıkarma, gecikme feshi, cayma beyanı, iade teslim alma, iade reddi, "mal dönmedi" kapanışı ve ayıp talebi — kapsadıkları adedi taşır; aynı türden kayıt aynı kalemde birden çok kez doğar (ardışık işlemler, parça parça ulaşan iade) ve kalemin açık adedi bu kayıtlardan okunur (Ürün Gereksinimleri `02 §5.8`, §7.1.6). Adedin kayıtta nasıl tutulduğu ve açık adedin saklanıp saklanmadığı bu aşamanın kararıdır; aynı adedi kullanmak isteyen eşzamanlı iki işlem siparişin satır kilidiyle sıraya girer ve sonraki, koşulunu güncel açık adede karşı denetler (Teknik Mimari `05 §4.1.2`, §7.3.2; K-874, K-889 — satır Aşama 4'ün çakışma taramasında hizalandı).
> - **K-788 · Adet başına ayrılan tutar:** kalemin bir kısmı işleme girdiğinde kupon payı ve KDV kalemin donmuş değerlerinden adetle orantılı ve aşağı yuvarlanarak ayrılır; kalemin son açık adetlerini kapatan işlem kalanı alır (`02 §7.1.7`). Son işlemin kalanı hesaplayabilmesi için her işlemin ayırdığı pay ve KDV'nin kayda geçmesi gerekir; nerede ve hangi biçimde tutulduğu bu aşamanın kararıdır.
> - **K-791 · Sayaçların adetle dönüşü:** stok, hizmet kontenjanı ve kupon hakkı işleme giren adet kadar döner; kupon hakkı yalnız siparişin bütün kalemlerinin bütün adetleri iptal ya da iade edildiğinde (`02 §7.2.6`, §7.4.8). Dönüşün hangi kayıttan türetildiği bu aşamanın kararıdır.
> - **K-736 · Yönetici hesabının adı:** yönetici hesabı zorunlu bir ad taşır — davetli hesabını açarken yazar, ilk yöneticininki kurulumda verilir, yönetici adını kendi hesabından değiştirir (`02 §10.2.1`, §10.2.4). Alanın tanımı bu aşamanın kararıdır.
> - **K-800 · Eşzamanlı düzenleme:** düzenleme ekranlarında araya giren kaydın yakalanması için kaydın düzenlemeye açıldığı andaki hâlinin kaydetme anında karşılaştırılması gerekir (`02 §3.31.1`; `04 §2.10`). Karşılaştırmanın veri karşılığı bu aşamanın kararıdır ve Teknik Mimari'nin (`05`) mekanizmasıyla birlikte tasarlanır.
> - **K-804 · Siparişin e-posta geçmişi:** sipariş ayrıntısının e-posta geçmişi siparişin müşteriye giden bildirimlerini — adı, gönderim tarihi, gittiği adres ve sonucu; yeniden gönderim ayrı satır — ve misafir siparişinin e-posta düzeltmelerini taşır; kayıt siparişin saklama kuralıyla imha edilir (`02 §4` Z-43; `04 §9.9`). Gönderim kaydının siparişe nasıl bağlandığı bu aşamanın kararıdır.
> - **K-806 · Kupon kodunun tekilliği:** kupon kodu kuponlar arasında tekildir ve büyük-küçük harf ayrımı olmadan eşleşir; kupon silinmez (`02 §3.10.5`). Tekilliğin veride nasıl korunduğu bu aşamanın kararıdır.
> - **K-811 · Doğrulanmamış kayıt:** panelin üye kaydı görünümü e-postayla aranır ve e-postası doğrulanmamış kaydı da bulur (`02 §3.13.4`, §3.15.5; `04 §9.12`). Doğrulanmamış kaydın nasıl tutulduğu ve aramada nasıl bulunduğu bu aşamanın kararıdır.
> - **K-817 · İade adresinin alanları:** firmanın iade adresi alıcının adını, ili (kapalı liste), ilçeyi ve açık adresi zorunlu, telefonu isteğe bağlı taşır (`02 §3.1.7`; `04 §9.18`). Alanların tanımı bu aşamanın kararıdır.
> - **`02 §3.8.5` · Ölçü birimlerinin listesi (Arayüz Tanımları'nın audit'i, bulgu FORM-16):** ürünün "ölçü birimi + net miktar" alanı tek alandır — ikisi birlikte girilir, net miktar sıfırdan büyüktür (`04` 7.2.4.13). Birimin seçildiği listenin içeriği — birim fiyatın hesaplandığı birimler (Fiyat Etiketi Yönetmeliği m.5) — bu aşamanın kararıdır.
> - **Dizin — Aşama 3 kararlarının bu dokümana devirleri:** karar kaydında K-723…K-850'nin etki sütunlarında bu dokümanı gösteren on dört atıf (on dört karar) ve Arayüz Tanımları'nın (`04`) gövdesinde bu dokümana iş bırakan cümleler [`CHECKPOINT_REPORTS/PHASE3_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE3_CONFLICT_SCAN.md) §6'dadır; Aşama 3'ün atıflarının hepsi işin adını taşır. Yukarıdaki ölçü birimi satırının kaynağı karar satırı değil Ürün Gereksinimleri ve audit bulgusudur. Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Önceki aşamaların dizinleri: `PHASE1_CONFLICT_SCAN.md` §6, `PHASE2_CONFLICT_SCAN.md` §6.

> **Aşama 4'ten park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §12.3 TM0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-870 · Zamanın, tarihin ve paranın türleri:** an saat dilimli damga (UTC) olarak tutulur, Türkiye saatiyle sayılır ve gösterilir; tarih olarak anlam taşıyan değer — son gün, iş günü — tarih olarak tutulur; para ondalıklı sayıyla tutulur, kayan noktalı sayı kullanılmaz (Teknik Mimari `05 §7`; `02 §4.1`; K-335). Her alanın hangi türü taşıdığı bu aşamanın kararıdır.
> - **K-873 · K-874 · K-980 · Sürüm alanları:** düzenlenebilir kayıtlar — ürün (varyant tablosunun değişikliği dahil), kupon, kurumsal içerik, ayarlar; bölümleri ayrı kaydedilen ekranda bölüm başına — ve sipariş bir sürüm değeri taşır; siparişin sürümü sipariş, kalemleri, kalem kayıtları ve siparişe bağlı müşteri girdileri — geri ödeme IBAN'ı dahil — değiştikçe artar (`05 §7`; `02 §3.31`). Sistemin stok ve kontenjan hareketleri — ayırma, kesin düşme, dönüş — ürünün sürümünü artırmaz; panelin stok ve kontenjan alanı ürünün sürümüyle değil kendi koşullu güncellemesiyle yazılır — yeni değer yalnız alanın değeri formun gösterdiği değerle hâlâ aynıysa ve yeni değer ayrılmış adetten az değilse yazılır, değilse `04` 2.10.1'in mesajı çıkar (`05 §7.3.1`; `02 §3.6.7`; K-980). Sürüm değerinin alanı ve onu artıran yazımların listesi bu aşamanın kararıdır; K-800'ün Aşama 3 park satırı bu mekanizmayla karşılanır.
> - **K-875 · K-876 · K-877 · K-944 · K-975 · K-979 · K-984 · İmha:** imha işi her kaydın süresini başlangıç değerinden hesaplar — oluştuğu yılın sonu, kapatılış, gönderim anı (`02 §4` Z-28…Z-44); silme fiziksel silmedir; siparişte kişisel alanlar boşaltılır ve hesap bağı kaldırılır (K-582) — imha siparişe bağlı kayıtları da kapsar: iç not, e-posta adresinin düzeltme geçmişi, ayıp talebi; bağlı kayıtların alan listesi bu aşamanın işidir (`05 §7.4.3`); imha kaydının ve geçiş kaydının "yalnız eklenen" oluşu uygulama düzeyindedir, veritabanı yetkisiyle korunan yalnız iz tablosudur (`05 §4.1.4`, §7.4.2); imha kaydı yalnız eklenen ayrı bir tabloda durur — tarih, kayıt türü, sayı; sunucunun kayıtlarının kendi döngüsüyle silinen günü her gece "döngüyle silindi" satırı olarak sayı yerine tarih aralığıyla girer (`05 §9.2.2`); işlem izi tablosunda uygulamanın veritabanı kullanıcısı yalnız ekler ve okur, süresi dolan satırı veritabanındaki bir imha fonksiyonu siler (`05 §7`); iz satırı yöneticinin adını ve e-posta adresini ve kaydın kimliğini değer olarak taşır ve izden başka tablolara silmeyle basamaklanan ya da boşaltan bağ yoktur; imha fonksiyonu sahibinin yetkisiyle çalışır, arama yolu sabittir ve çağırma yetkisi yalnız uygulamanın veritabanı kullanıcısınındır (`05 §7.5.1`); periyodik imhanın her parçası imha kaydındaki satırını parçanın işleminde yazar — var olan satırı güncellemez (`05 §7.4.2`); gecelik temizlik kişisel veri taşıyan geçici kayıtların — deneme, oturum, tanınan tarayıcı ve misafir sepeti kayıtları — silinmesini de imha kaydına kayıt türü ve sayıyla yazar (`05 §5.2.9`, §7.4.4; K-979, K-984); bekleyen e-posta değişikliğinin ömrü dolunca zamanlayıcının silmesi de imha kaydına silmeyle aynı işlemde kayıt türü ve sayıyla yazılır (`05 §5.2.7`, §7.4.2; K-984); istek içinde silinen oturum kayıtları — çıkış, şifre değişikliği ve sıfırlama, oturumları sonlandırma komutu gibi — imha değildir, oturumun işleyişidir (`05 §5.2.9`; K-984). Siparişe ait üretilen belgelerin — Ön Bilgilendirme Formu, Mesafeli Satış Sözleşmesi — kopya olarak mı saklandığı, donmuş şablon sürümünden mi yeniden üretildiği bu aşamanın kararıdır; kopya saklanırsa imha kopyadaki kişisel veriyi de kapsar. Başlangıç değerlerinin alanları, imha kaydının ve iz tablosunun tanımı bu aşamanın kararıdır.
> - **K-883 · K-884 · K-948 · K-980 · Dosya kaydı ve indirme:** her yükleme yeni bir dosya kaydıdır; varyantın o anki dosyası ona en son bağlanan kayıttır; hiçbir varyanta bağlı olmayan ve indirilebilir hiçbir kalemin istemediği kayıt imhada silinir; kalem indirme sayacını ve kısa ömürlü indirme bağlantısını taşır; hak tek bir koşullu güncellemeyle düşer — kalan hak yoksa bağlantı üretilmez ya da dosya gönderilmez (`05 §7`, §7.7.4; `02 §3.12`; K-980). Görsel — kaynak kopya ve türevleri — ürüne, içerik kaydına ve sipariş kalemine birer bağla bağlıdır; kalem görselin kendisini değil ona bağını dondurur ve ürünün silinmesinden etkilenmez (`02 §3.23.1`); hiçbir kayda bağlı olmayan görsel imhada, kaydedilmeden bırakılan yükleme geçici kayıtların temizliğinde silinir; önce kayıt silinir, kaydı olmayan dosyayı imha işi süpürür (`05 §7.7.1`, §7.7.2). Kayıtların ve bağların tanımı bu aşamanın kararıdır.
> - **K-886 · Ürün adının arama biçimi:** ürün adının küçük harfe çevrilmiş ve Türkçe harfleri sadeleşmiş arama biçimi ayrı bir alanda tutulur ve ad değiştikçe yenilenir; arama bu alanda içerir eşleşmesiyle, trigram dizini üzerinden yapılır (`05 §7`; `02 §3.5.1`). Alanın ve dizinin tanımı bu aşamanın kararıdır.
> - **K-889 · K-890 · Geçiş kaydı ve ayırma:** siparişin her durum geçişi yalnız eklenen bir geçiş kaydına yazılır — kim (müşteri, yönetici, sistem), ne zaman, hangi geçiş, sebep; kayıt `02 §3.34.8`'in "durum geçişleri"nin karşılığıdır ve işlem izinin yerine geçmez. Sipariş onayı stoğu, hizmet kontenjanını ve kupon hakkını koşullu güncellemeyle — kalan adet yetiyorsa — ayırır (Teknik Mimari `05 §4`, §7). Geçiş kaydının tanımı ve ayrılmış adedin alanları bu aşamanın kararıdır.
> - **K-891 · K-892 · K-893 · K-894 · K-940 · K-941 · K-946 · K-954 · K-970 · K-985 · İş kuyruğu, gönderim kaydı, gelen bildirim ve geri ödeme kaydı:** dış yan etkiler veritabanındaki bir iş kuyruğuna geçişle aynı işlemde yazılır — satır kişisel veri taşımaz, kayda kimlikle işaret eder; iş satırı deneme sayısını, sonraki deneme anını ve sonucunu taşır — kuralı yazılmamış iş türünde beşinci yeniden denemeden sonra "başarısız" hâlindedir ve sıradaki satırları bekletmez; "başarısız" satır gecelik temizlikte silinmez, kurulumu yapanın yönetim komutuyla yeniden kuyruğa alınır ya da kapatılır — kapatılan satır tamamlanmış sayılır ve gecelik temizlikte silinir (`05 §4.3.4`, §5.2.9, §8.5.9; K-985); her e-posta gönderim kaydında bir satırdır ve e-postanın kuyruğu odur — e-posta iş kuyruğuna ayrıca satır yazmaz (`05 §4.3.1`); satır tür, alıcı, ilgili kayıt, olayın değerleri, deneme sayısı, sonraki deneme anı ve sonucu taşır; gövde saklanmaz (Z-34, Z-43); ödeme adımı formunun onay anahtarı sipariş kaydında tekildir; sağlayıcının imzası doğru bildirimi gelen bildirim kaydına yazılır — imzası tutmayan istek yazılmaz — ve tekil olan bildirimin kendisidir: sağlayıcının bildirime verdiği kimlik, yoksa işlem kimliği olay türü ve sonuçla birlikte (`05 §4.5.2`); siparişin ödeme kaydı ödemeyi sağlayıcının ödeme işlem kimliğiyle taşır ve kimlik orada tekildir — aynı ödeme ikinci kez gelirse işlenmez, iptal edilmiş siparişte tek kart iadesi doğar (`05 §4.5.3`); gelen bildirim kaydı kişisel alan taşımaz — sağlayıcının referansı, siparişin referansı, tutar, sonuç ve anlar —; tutarı tutmayan ya da siparişi kayıtta olmayan satır işaretlidir, siparişe bağlanan satır siparişle silinir, hiçbir siparişe bağlanmayan satır on yıl sonra imha işinde silinir — kişisel alan taşımaz, elektronik ticaret kayıtlarının on yılını izler (K-954; `05 §4.5.2`, §4.5.5, §7.4.4); karta para gönderen her istekten önce geri ödeme kaydı "istek gönderiliyor" hâliyle yazılır ve kendi işleminde kesinleşir — istek ondan sonra gider (`05 §4.6.1`); kaydın hâlleri istek gönderiliyor, sonucu bekleniyor, başarılı ve gerçekleşmedi'dir — sonucu bilinmeyen istek "sonucu bekleniyor"da kalır, sorgunun sonucuyla ya da firmanın "gerçekleşmedi" işaretiyle (işlem izine düşer) çıkar — firmanın işareti sorguyu durdurur, sonradan gelen başarılı sonuç — geç bildirim ya da işaret anında yanıtı beklenen sorgu — siparişe işlenmez, gelen bildirim kaydında ya da geri ödeme kaydında işaretli kalır ve panelde uyarı olur; panelin "geri ödeme sonucu bekleniyor" ve "geri ödeme gerçekleşmedi" işaretleri bu hâlle aynı işlemde yazılır (`05 §4.6.2`; K-940). Tabloların ve hâllerin tanımı bu aşamanın kararıdır.
> - **K-895 · K-898 · K-946 · Zaman alanları, nabız ve kesinti kaydı:** zamanlanmış işlerin zamanı kendi kaydında bir alandır — ödeme süresinin bittiği an, hatırlatmanın anı, IBAN'ın silineceği gün, e-postanın sonraki denemesi — ve onayda ya da süre yeniden başladığında yazılır; gecelik işin — imha ve geçici kayıtların temizliği — son başarılı çalışmasının ve son denemesinin anı veritabanında durur — başarısız biten iş en erken bir saat sonra yeniden denenir (`05 §5.1.1`, §9.4.17); zamanlayıcının nabzı ve iki dakikayı aşan boşlukların kesinti kaydı veritabanında durur; kesintide ertelenen havale iptalinin yeni son anı siparişte saklanır ve sipariş sayfasında görünür (`05 §5`; `02 §4.2` Z-8). Alanların tanımı bu aşamanın kararıdır.
> - **K-899 · K-901 · K-903 · K-906 · K-945 · K-971 · K-972 · K-982 · Oturum, deneme, bağlantı ve tanınan tarayıcı kayıtları:** oturum sunucuda tutulur — anahtarın özeti, hesap, hesap türü, kısa ya da uzun, son işlem anı, yeniden doğrulamanın tek işlemlik izni — izin dönülen işlem formuna konan tek kullanımlık değere bağlıdır (`05 §6.1.5`); müşteri ve yönetici hesapları ayrı tablolardadır — e-posta adresi kendi hesap türü içinde tekildir; tekillikte, girişte, deneme limitlerinin ve L-7/L-8'in e-posta ekseninde ve misafir siparişlerinin hesaba bağlanmasında büyük-küçük harf ayrımı olmadan karşılaştırılır, gösterimde ve gönderimde girildiği gibi kalır; tekillik veritabanında korunur (`05 §3.5.2`; K-982); sayılan her deneme bir deneme kaydıdır — limit, eksen, değer, an — ve penceresi geçince silinir; başarısızlıkta sayılan limitlerde satır denemeden önce yazılıp kesinleşir, başarılı ya da eşikte durdurulan denemede silinir (`05 §6.3.1`); girişte birleşen sepet iki sayacın büyüğünü taşır — penceresinde satırı çok olan sepetin deneme satırları hesabın sepetinin kimliğine yazılır, ötekininki silinir (`05 §6.3.2`); giriş kaydı (`02 §3.13.20`) iki hesap türünün girişini taşır, veritabanındadır ve okuma komutuyla okunur (`05 §3.3.11`, §9.3.1); L-7 ve L-8 siparişlerden sayılır ve siparişte IP durur; hesap bağlantıları bağlantı kaydında yalnız özetle durur — tür, hesap, oluşma anı, ömrün sonu, kullanıldığı an, yerine yenisi geçtiği an — yönetici davetinde daveti gönderen yönetici de; bağlantı tek bir koşullu güncellemeyle tüketilir (`05 §6.4.6`); bağlantı kaydı hesabıyla yaşar ve hesapla silinir — gecelik temizlik onu silmez, ömrü dolmuş kayıt "ömrü dolmuş", "kullanılmış" ya da "yerine yenisi geçmiş" hâlini korur (`05 §6.4.4`, §6.4.6); sipariş erişim anahtarı siparişte durur, numara ve anahtar tekildir; tanınan tarayıcı işareti sunucuda hesap başına kaydıyla tutulur ve süresi dolan ya da geçersizleşen kayıt gecelik temizlikte silinir (`05 §5.2.9`); misafir sepeti son erişim anını taşır ve 400 gün açılmazsa silinir (`05 §6`). Kayıtların tanımı ve e-posta tekilliğinin veride korunuşu bu aşamanın kararıdır.
> - **K-913 · K-918 · K-937 · K-963 · Kanal hâli ve yedeğin durum kaydı:** e-posta göndericisi tek satırlık bir kanal hâli kaydı tutar — son başarılı gönderimin anı, art arda kanal hatası sayısı, dizinin ilk hatasının anı — ve her gönderim denemesinin sonucu üç sınıftan birini taşır: gönderildi, alıcı hatası, kanal hatası; firma bildirimleri uyarısı gönderim kaydından okunur, ayrı bir işaret saklanmaz. Sunucunun dışındaki depoya alınan gecelik tam yedeğin sonucunu — yerel kopyanınki sayılmaz — veritabanı konteynerindeki zamanlayıcı yerel soketle veritabanındaki bir durum kaydına yazar; dosya yedeğinin sonucu veritabanında değil, yedek aracının durum dosyasındadır — yedek aracı veritabanına bağlanmaz ve ağ üzerinden bağlanan bir yedek kullanıcısı yoktur (Teknik Mimari `05 §9.4.5`, §9.4.10, §10). Kayıtların tanımı bu aşamanın kararıdır.
> - **K-951 · Elektronik ticaret kayıtlarının on yıllık saklaması (6563 sayılı Kanun m.11/3):** siparişe bağlı müşteri bildiriminin gönderim kaydı on yıl saklanır ve imha işi satırın siparişine bakar — ödemesi alınmış siparişin satırı on yıl alıcısıyla durur ve sonra silinir; ödemesi hiç alınmamış siparişin satırında üç yılda alıcı ve olayın değerleri boşaltılır, satır on yılda silinir. Ödemesi hiç alınmamış siparişin kişisel verisiz kaydı on yıl dolmadan silinmez. İletişim talebinin süresi konu tipinden okunur — "Sipariş hakkında" on yıl, öteki tipler iki yıl (`02 §4.2` Z-33, Z-41, Z-43, §11.2 P-40; Teknik Mimari `05 §4.4.4`, §7.4.3). Gönderim kaydının siparişe bağı, siparişin ödemesi alınmış sayıldığının (`02 §4.2` Z-28, §6.2.16) hangi alandan okunduğu ve boşaltılan alanların listesi bu aşamanın kararıdır.
> - **Dizin — Aşama 4 kararlarının bu dokümana devirleri:** karar kaydında K-855…K-979'un etki sütunlarında bu dokümanı gösteren kırk bir atıf (otuz dokuz karar) ve Teknik Mimari'nin (`05`) gövdesinde bu dokümana iş bırakan cümleler [`CHECKPOINT_REPORTS/PHASE4_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE4_CONFLICT_SCAN.md) §6'dadır; Aşama 4'ün atıflarının hepsi işin adını taşır. Aşama 4'ün checkpoint'inin kararları (K-980…K-988) tarama raporunda yoktur — bu dokümana bıraktıkları iş yukarıdaki satırlarda etiketiyle durur. K-977'nin satırı Aşama 1 bloğunun K-17 satırıdır. Sayfa adresinin veride tutuluşu (`05 §3.7.1`) karar satırı olmadan devredilmiştir — tek yazılı yeri o cümledir. Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Önceki aşamaların dizinleri: `PHASE1_CONFLICT_SCAN.md` §6, `PHASE2_CONFLICT_SCAN.md` §6, `PHASE3_CONFLICT_SCAN.md` §6.

---

## 1. Traceability Matrix (ÖNCE BU)

> **Ne yazılır:** `02` gereksinimleri + `03` akış adımları → entity/alan eşlemesi.
> Gereksinimlerdeki her *"X bilgisi saklanır"* ifadesi bir alana dönüşmelidir. Eşlenmeyen gereksinim = **eksik entity veya alan**.

**Alt bölüm haritası (K-998).** Matris beş oturumda kurulur (K-997): **1a** (2026-10-17) kaynak birimini, envanteri ve kapsama denetimini, aday entity listesini ve Ürün Gereksinimleri'nin `02 §0`–§5'i ile MVP Kapsamı'nın `10 §2`'sinin ileri izlenebilirliğini yazdı; **1b** `02 §6`–§13'ü, **1c** Kullanıcı Akışları'nı (`03`), **1d** devirleri yazar; **2** geri izlenebilirliği, kesilmiş okumanın örneklemini, GAP ve gerekçesiz ekleme listesini ve GAP kararlarını yazar. Şablonun §1.1, §1.2 ve §1.3 numaraları değişmez; alt düzey aşağıdaki tabloyla sabittir. Bir oturum bir alt bölümü bölmek ya da birleştirmek zorunda kalırsa bu tabloyu ve ona atıf yapan satırları aynı PR'da günceller (K-670'in kalıbı; `checklists/document-stage.md` §4).

| Alt bölüm | İçerik | Oturum | Durum |
|---|---|---|---|
| §1.1.1 | Kaynak envanteri ve sayım — aile × oturum, betik komutları, kapsama denetimi | 1a; her oturum kendi ailesinin sayımını ve denetimini ekler | ✓ (1a'nın aileleri) |
| §1.1.2 | Aday entity listesi ve "veri doğurmaz" değer kümesi | 1a; 1b, 1c, 1d listenin sonuna aday ekleyebilir | ✓ (sonraki oturum ekleme yaparsa günceller) |
| §1.1.3 | MVP Kapsamı — `10 §2` KP satırları (77) | 1a | ✓ |
| §1.1.4 | Ürün Gereksinimleri — `02 §0`–§2: sözlük terimleri (105), kural numarası taşımayan madde ve metin (42) — 147 | 1a | ✓ |
| §1.1.5 | Ürün Gereksinimleri — `02 §3` iş kuralları: 257 kalın kural, 19 madde ve metin — 276 | 1a | ✓ |
| §1.1.6 | Ürün Gereksinimleri — `02 §4` zaman kuralları (61) ve §5 durum tanımları (109) — 170 | 1a | ✓ |
| §1.1.7 | Ürün Gereksinimleri — `02 §6` hata ve istisna senaryoları: 6 kalın kural; 141 hata satırı 9 alt bölüm düzeyinde, kayıt, işaret ya da tarih doğuran satır kimliğiyle ayrıca (K-997) | 1b | ⬚ |
| §1.1.8 | Ürün Gereksinimleri — `02 §7`–§13: 152 kalın kural; L-1…L-9, B-1…B-16, F-1…F-6, P-1…P-49, H-1…H-4 (84); kural numarası taşımayan madde ve metin (§1.1.1'in birim kuralıyla, K-1000) | 1b | ⬚ |
| §1.1.9 | Kullanıcı Akışları — `03 §1` (99) ve §4 (59) satır düzeyinde (158) | 1c | ⬚ |
| §1.1.10 | Kullanıcı Akışları — `03 §2`, §3, §5, §6, §8, §9, §10: 39 alt bölüm düzeyinde, 505 satır tam okunur ve `02`'de karşılığı olmayan veri taşıyan satır kimliğiyle ayrıca girer; §7'nin 131 satırı `02 §9`'un B ve F satırlarıyla toplu (K-997) | 1c | ⬚ |
| §1.1.11 | Devirler — K-479…K-991'in etki sütununda `06` taşıyan 126 karar satırı; park bloklarının 23 talimat maddesi; dizinlerin gövde cümleleri — Aşama 1 on, Aşama 3 iki, Aşama 4 dokuz (21) | 1d | ⬚ |
| §1.1.12 | GAP adayları (`VA-n`) — karara bağlanacak | 1a açtı; 1b, 1c, 1d ekler; 2 karara bağlar | 1 aday |
| §1.2 | Geri izlenebilirlik (`Entity.alan` → kaynak); gerekçesiz ekleme listesi | 2 | ⬚ |
| §1.3 | Boşluklar (GAP) ve kararlar; üst dokümanlara geri besleme tablosu (K-994) | 2; workshop ve yazım geri besleme satırı ekler | ⬚ |

### 1.1 İleri izlenebilirlik (gereksinim/akış → entity.alan)

Her satır şablonun beş sütununu taşır: **Kaynak ID · Kaynak özeti · Entity · Alan · Durum** (K-999). **Kaynak ID** kaynağın kimliğidir: kural `02 §n.m.k`; sözlük terimi `02 §1.2` ve kod karşılığı; kimlikli tablo satırı `02 §4.2` Z-n, `02 §5.4` Sn, `02 §5.5` Ön; durum satırı `02 §5.x` ve durumun kodu; kural numarası taşımayan liste maddesi ya da tablo satırı `02 §n.m` mk — k alt bölümdeki sırasıdır —; alt bölümün kural ve madde dışındaki metni `02 §n.m` metin; kapsam satırı KP-n (K-1000). **Kaynak özeti** satırın özetidir, kuralın evi kaynaktır. **Entity** §1.1.2'nin değer kümesinden bir ya da birkaç değerdir (` · ` ile). **Alan** aday alanın Türkçe tarifidir; sözlükte kod karşılığı olan alan kod adıyla yazılır (`ShippingAddress`), öteki alanlara ad verilmez — alanın adı, türü ve kısıtı §4'ün işidir (VM2, VM4; K-17); "—" satırın kaydın kendisini gösterdiğini söyler. **Durum** üç değerden biridir: **eşlendi** (en az bir aday), **veri doğurmaz** (§1.1.2'nin adıyla yazılmış değerlerinden biri — GAP değildir), **GAP adayı (VA-n)** (§1.1.12'de numarasıyla durur). Atıflar K-669 ve K-701'in okunuşuyla yazılır.

#### 1.1.1 Kaynak envanteri ve sayım

Birim K-997'dir; 1a'nın netleştirmesi K-1000'dedir. Sayımlar `02` v0.76, `03` v0.28, `10` v0.51 ve karar kaydı v0.117 üzerinde, 2026-10-17'de alındı.

| Aile | Betiğin saydığı | 1a | Sonraki oturum | Matrise girmeyen |
|---|---|---|---|---|
| `10 §2` KP satırları | 77 | 77 | — | — |
| `02 §1.2` sözlük terimleri | 105 | 105 | — | — (K-997'nin gerekçe hücresindeki 97 betik sayımında 105'tir — K-1000) |
| `02` kalın numaralı kurallar | 438 | 280 — §3 257 · §4 5 · §5 18 | 1b: 158 — §6 6 · §7 44 · §8 28 · §9 10 · §10 42 · §12 28 | — |
| `02` kimlikli tablo satırları | 147 — Z 47 · S 11 · Ö 5 · L 9 · B 16 · F 6 · P 49 · H 4 | 63 — Z, S, Ö | 1b: 84 — L, B, F, P, H | — |
| `02 §5` durum satırları | 20 | 20 — §5.1 3 · §5.2 2 · §5.4 6 · §5.5 5 · §5.10 2 · §5.13 2 | — | — |
| `02` kural numarası taşımayan madde | §0–§5: 101 | 101 | 1b: §6–§13'ünkiler (aynı betikle) | — |
| `02` alt bölümün kural ve madde dışındaki metni | §0–§5: 24 | 24 | 1b: §6–§13'ünkiler (aynı betikle) | Kaynak satırları (`*Kaynak:`), şablonun "Ne yazılır" notları, sözlüğün grup başlıkları |
| `02 §6` hata satırları | 141 (9 alt bölüm) | — | 1b | — |
| `03` numaralı satırlar | 794 | — | 1c: §1 99 · §4 59 satır düzeyinde; §2 99 · §3 147 · §5 33 · §6 52 · §8 106 · §9 35 · §10 33 alt bölüm düzeyinde (39 alt bölüm); §7 131 toplu | `03 §0` (yazım konvansiyonu) ve `03 §11` (`02`'ye geri besleme tablosu; `n.m` biçimli) |
| Devir satırları — K-479…K-991, etki sütununda `06` | 126 | — | 1d | Aşama 5'in kendi kararları (K-992 ve sonrası) devir değildir |
| Park maddeleri | 26 | — | 1d: 23 talimat maddesi | 3 dizin maddesi — dizinin kendisine işaret eder, devirleri karar satırlarıyla girer |
| Gövde devirleri — dizinlerin §6.2'si | 21 | — | 1d | — |
| **1a toplamı** | | **670** | | |

**1a — bölüm bazında** (670): `10 §2` 77 · `02 §0` 4 · §1 117 (sözlük 105, madde 9, metin 3) · §2 26 (madde 23, metin 3) · §3 276 (kural 257, madde 11, metin 8) · §4 61 (kural 5, Z 47, madde 7, metin 2) · §5 109 (kural 18, durum 20, S ve Ö 16, madde 47, metin 8). §1.1.3 77 · §1.1.4 147 · §1.1.5 276 · §1.1.6 170.

**Betik komutları** (Git Bash; depo kökünden). Sayımlar yukarıdaki sayılarla aynı çıkmalı, üç kapsama denetimi boş dönmelidir:

```sh
# 10 §2 — KP satırları (77)
grep -cE '^\| KP-[0-9]+ ' Docs/10_MVP_SCOPE.md
# 02 — kalın numaralı kurallar, üst bölüm bazında (438)
grep -oE '^\*\*[0-9]+\.[0-9]+\.[0-9]+ ' Docs/02_PRODUCT_REQUIREMENTS.md | tr -d '*' \
  | awk -F. '{c[$1]++} END{for(k in c) print "S"k, c[k]}' | sort -V
# 02 — kimlikli tablo satırları, aile bazında (147)
grep -oE '^\| ((Z|L|B|F|P|H)-|S|Ö)[0-9]+ ' Docs/02_PRODUCT_REQUIREMENTS.md | sed -E 's/^\| //; s/[0-9]+ $//' | sort | uniq -c
# 02 §6 — hata satırları (141) ve alt bölümleri (9)
awk '/^## 6\. /{f=1} /^## 7\. /{f=0} f' Docs/02_PRODUCT_REQUIREMENTS.md | grep -cE '^\| [0-9]+\.[0-9]+\.[0-9]+ \|'
awk '/^## 6\. /{f=1} /^## 7\. /{f=0} f' Docs/02_PRODUCT_REQUIREMENTS.md | grep -cE '^### '
# 03 — numaralı satırlar, üst bölüm bazında (794) ve alt bölümler
grep -oE '^\| [0-9]+(\.[0-9]+){2,} \|' Docs/03_USER_FLOWS.md \
  | awk '{split($2,a,"."); c[a[1]]++} END{for(k in c) print "S"k, c[k]}' | sort -V
grep -oE '^### [0-9]+\.[0-9]+ ' Docs/03_USER_FLOWS.md | awk '{split($2,a,"."); c[a[1]]++} END{for(k in c) print "S"k, c[k]}' | sort -V
# devir satırları — K-479…K-991, etki sütununda 06 (126)
awk -F'|' '/^\| K-[0-9]+ /{k=$2; gsub(/ /,"",k); n=substr(k,3)+0; if (n>=479 && n<=991 && $7 ~ /(^|[ ·(,`])06([ ,(·)`]|$)/) print k}' \
  Docs/PRODUCT_DISCOVERY_STATUS.md | sort -u | wc -l
# park maddeleri (26 — 23 talimat, 3 dizin)
grep -cE '^> - \*\*' Docs/06_DATA_MODEL.md
grep -cE '^> - \*\*Dizin' Docs/06_DATA_MODEL.md

# 1a'nın beklenen kümesi (670): 02 §0–§5'in birimleri ve KP satırları — birim kuralı K-1000
exp() {
  awk '
    /^## 6\. /{exit}
    /^## 0\. /{on=1}
    !on{next}
    /^##+ [0-9]/{s=$2; sub(/\.$/,"",s); next}
    /^[[:space:]]*$/ || /^---/ || /^\|[-| ]+\|[[:space:]]*$/{next}
    /^\*\*[0-9]+\.[0-9]+\.[0-9]+ /{r=$1; gsub(/\*/,"",r); print "02 §" r; next}
    /^\|/{
      n=split($0,c,"|"); for(i=1;i<=n;i++){gsub(/^[[:space:]]+|[[:space:]]+$/,"",c[i])}
      if (c[2] ~ /^(Türkçe terim|#|Durum|Siparişin kalemleri|Akış|Aktör|Gruplar)$/) next
      if (s=="1.2"){x=c[3]; gsub(/[`†]/,"",x); gsub(/^[[:space:]]+|[[:space:]]+$/,"",x); print "02 §1.2 " x; next}
      if (c[2] ~ /^(Z-[0-9]+|S[0-9]+|Ö[0-9]+)$/){print "02 §" s " " c[2]; next}
      if (c[2]=="") next
      if (s ~ /^5\.(1|2|4|5|10|13)$/ && c[3] ~ /^`[A-Za-z]+`$/){x=c[3]; gsub(/`/,"",x); print "02 §" s " " x; next}
      m[s]++; print "02 §" s " m" m[s]; next
    }
    /^[[:space:]]*(- |[0-9]+\. )/{m[s]++; print "02 §" s " m" m[s]; next}
    /^\*Kaynak:/ || /^> \*\*Ne yazılır/{next}
    s=="1.2" && /^\*\*[^*]+\*\*[[:space:]]*$/{next}
    !(s in t){t[s]=1; print "02 §" s " metin"}
  ' Docs/02_PRODUCT_REQUIREMENTS.md
  grep -oE '^\| KP-[0-9]+ ' Docs/10_MVP_SCOPE.md | sed 's/^| //; s/ $//'
}
# bu dokümanın 1a satırları (§1.1.3–§1.1.6)
mat() {
  awk '/^#### 1\.1\.[3-6] /{f=1; next} /^#### /{f=0}
       f && /^\| (`02 §|KP-)/{split($0,c,"|"); x=c[2]; gsub(/`/,"",x); gsub(/^ +| +$/,"",x); gsub(/ +/," ",x); print x}' Docs/06_DATA_MODEL.md
}
exp | wc -l; mat | wc -l                     # 670 · 670
comm -23 <(exp | sort) <(mat | sort)         # eksik — boş dönmeli
comm -13 <(exp | sort) <(mat | sort)         # fazla — boş dönmeli
mat | sort | uniq -d                         # yinelenen — boş dönmeli
exp | awk '{split($2,a,"."); sub(/§/,"",a[1]); t=($3 ~ /^m[0-9]+$/)?"madde":($3=="metin")?"metin":($3=="")?"kural":"kimlik"; if ($1 ~ /^KP-/) {a[1]="KP"; t="KP"} c[a[1]" "t]++} END{for(k in c) print k, c[k]}' | sort -V
# "Entity" sütunu §1.1.2'nin değer kümesinin dışına çıkmıyor (boş dönmeli)
awk '/^#### 1\.1\.[3-6] /{f=1; next} /^#### /{f=0} f && /^\| (`02 §|KP-)/{split($0,c,"|"); n=split(c[4],e," · "); for(i=1;i<=n;i++){x=e[i]; gsub(/^ +| +$/,"",x); print x}}' Docs/06_DATA_MODEL.md | sort -u \
  | grep -vxF -f <(awk '/^#### 1\.1\.2 /{f=1; next} /^#### /{f=0} f && /^\| [0-9]+ \|/{split($0,c,"|"); x=c[3]; gsub(/^ +| +$/,"",x); print x} f && /^\| (veri doğurmaz|yoktur)/{split($0,c,"|"); x=c[2]; gsub(/^ +| +$/,"",x); print x}' Docs/06_DATA_MODEL.md)
```

**Yanlış pozitif ve kapsam denetimi (1a).** Kural kalıbı yalnız satır başındaki kalın `n.m.k`'yı yakalar; dört düzeyli kalın kural yoktur ve kuralın devamında liste maddesi yoktur — `02 §3`'ün maddeleri yalnız bölüm girişindedir (betikle sayıldı). KP kalıbı yalnız ilk hücreyi yakalar. Kimlikli satır kalıbı yalnız ilk hücresi kimlik olan satırı yakalar; grup başlığı satırları (`| | **…** |`) ve tablo başlıkları birim değildir. Sözlüğün 105 satırının hepsi bir kod karşılığı taşır (18'i † ile); yinelenen kod yoktur. Durum satırı ikinci hücresi tek bir kod olan satırdır; `02 §5.7`'nin hat tablosu durum taşımaz, maddesiyle girer. Matrisin 1a satırlarında yinelenen kaynak kimliği yoktur; beklenen kümeyle karşılaştırmada eksik ve fazla sıfırdır (2026-10-17). **Betik iki yönlü sınandı** (`audit` skill'i, "Koşum biçimi" 4. madde): matristen bir satır çıkarıldığında denetim o kimliği eksik, bir satır ikilendiğinde yinelenen, kaynakta olmayan bir kimlik eklendiğinde fazla diye döndürdü; değer kümesinin dışında bir "Entity" değeri yazıldığında son denetim o değeri döndürdü.

**Okuma derinliği (checklist §2).** 1a her birimin metnini betikle **satırın tamamı** olarak çıkardı ve tam okudu — sözlük satırları tanım hücresiyle, KP satırları "Neden MVP'de" ve "Kaynak" hücreleriyle, Z satırları altı hücresiyle; hücre başına kesilerek okunan satır yoktur; checklist §2'nin kesilmiş okuma örneklemi bu yüzden 1a'nın satırlarına uygulanmaz. `02 §3`'ün kural dışı gövdesi, §4.2'nin grup başlıkları ve bölüm girişleri de okundu. Tam okunan yalnız matrisin kaynağıdır: `02 §6`–§13, `03` ve devirler kendi oturumlarındadır.

#### 1.1.2 Aday entity listesi ve "veri doğurmaz" değer kümesi

Bu liste matrisin "Entity" sütununun değer kümesidir; **nihai entity envanteri değildir** — envanter §3'te, yazım turunda doğar (VM3-01…VM3-03) ve bir adayı bölebilir, birleştirebilir ya da bir alan grubuna çevirebilir: hangi adayın ayrı tablo, hangisinin bir kaydın alan grubu olacağı §4'ün kararıdır (VM4). Aday kod adıyla anılır (K-995); sözlükte (`02 §1.2`) terimi olmayan aday **italik Türkçe adıyla** yazılır ve kod adı taşımaz — teknik kavramın kod adı yazım turunda `05`'teki adından verilir (VM3-03; K-977, K-988), ürüne görünen kaydın adı GAP adayıdır (VA-1), bağ ve ayar kayıtlarının adı adlandırma kuralıyla türer (VM2-03). 1b, 1c ve 1d yeni adayı listenin **sonuna** ekler ve bu tabloyu günceller (K-999). "Konu" sütunu adayın alanlarını kuracak workshop ya da yazım konusudur (karar kaydı §14.3).

| # | Aday | Kaynak | Sorumluluk | Konu |
|---|---|---|---|---|
| 1 | Company | `02 §1.2`, §3.1 | Kurulumun tekil firması: tip, tipe göre yasal kimlik alanları, iletişim, KEP, ETBİS bağlantısı | VM4-01 |
| 2 | *firma ayarları* | `02 §3.1`, §3.18–§3.21, §3.28, §3.29.6, §11 | Firmanın panelden değiştirdiği ayarlar: ödeme yöntemleri ve havale IBAN'ı, kargo ücreti, eşikler, teslimat illeri, süreler, iade adresi, geçici kapatma, ana sayfa düzeni, menü adları, platform imzası | VM4-01 |
| 3 | Branding | `02 §1.2`, §3.29 | Marka kimliğinin dört ayarı: marka adı, logo, site simgesi, marka rengi | VM4-10 |
| 4 | SocialLink | `02 §1.2`, §3.27.12 | Kapalı platform listesindeki hesap bağlantıları ve WhatsApp numarası | VM4-10 |
| 5 | *il listesi* | `02 §3.14.3` | 81 ilin kapalı listesi — başlangıç verisi | VM8-01 |
| 6 | Product | `02 §1.2`, §3.2–§3.11 | Vitrin ve katalog birimi: ad, açıklama, tip, KDV oranı, üretim yeri, ölçü birimi, sınırlar, sayfa adresi | VM4-02 |
| 7 | Variant | `02 §1.2`, §3.3, §3.6, §3.8 | Satılan birim: stok kodu, fiyat, stok ya da kontenjan, yayın durumu | VM4-02 |
| 8 | Option | `02 §1.2`, §3.3.2 | Ürünün seçenek boyutu (en çok iki) | VM4-02 |
| 9 | OptionValue | `02 §1.2`, §3.3.3 | Seçeneğin değeri; varyantın kombinasyonu | VM4-02 |
| 10 | Category | `02 §1.2`, §3.4 | Üç seviyeli kategori ağacının düğümü | VM4-02 |
| 11 | *ürün–kategori bağı* | `02 §3.4.2`, §3.4.4 | Ürünün asıldığı kategoriler | VM4-02 |
| 12 | *görsel* | `02 §3.11`, §3.27.15 | Ürünün, varyantın ve içerik kaydının görseli: sıra, ana görsel işareti, alternatif metin | VM4-15 |
| 13 | *dosya kaydı* | `02 §3.12`; park (K-883) | Dijital ürünün yüklenen dosyası; varyantın o anki dosyası | VM4-15 |
| 14 | Discount | `02 §1.2`, §3.9 | Ürün düzleminde tarihli yüzdesel indirim | VM4-03 |
| 15 | *fiyat geçmişi* | `02 §3.8.5`, §3.9.3 | Varyantın uygulanmış fiyatları ve başlangıç anları — referans fiyat ve fiyat tarihi | VM4-03 |
| 16 | Coupon | `02 §1.2`, §3.10 | Kupon: kod, tür, değer, tarih aralığı, kullanım adedi, haklar | VM4-04 |
| 17 | StockReservation | `02 §1.2`, §3.17.3 | Siparişin ayırdığı stok, kontenjan ve kupon hakkı | VM4-04 |
| 18 | Cart | `02 §1.2`, §3.16 | Üyenin ya da misafir tarayıcısının sepeti | VM4-05 |
| 19 | CartItem | `02 §1.2`, §3.16.5 | Sepetteki varyant, adet ve ilk eklendiği fiyat | VM4-05 |
| 20 | Order | `02 §1.2`, §3.17–§3.25, §5.3–§5.6 | Sipariş: numara, iki eksen, donan adresler, kimlik, sürümler, IBAN, tutarlar, kargo, zaman alanları | VM4-06 |
| 21 | OrderItem | `02 §1.2`, §3.23, §5.8 | Siparişin kalemi: donan değerler, teslim işareti, indirme hakkı ve sayacı | VM4-06 |
| 22 | OrderEmailHistory | `02 §1.2`; `02 §10.4.11` | Misafir siparişinin e-posta düzeltmelerinin eski ve yeni değeri | VM4-13 |
| 23 | *onay kaydı* | `02 §3.24.6` | Dijital ve hizmet kalemi için işaretlenen kutuların kaydı | VM4-11 |
| 24 | *ödeme kaydı* | `02 §1.2` (`Failed`), §5.5; `05 §4.5.3` | Siparişin ödemesi, sağlayıcının işlem kimliği ve geç gelen ödeme | VM4-08 |
| 25 | Cancellation | `02 §1.2`, §5.8 | Sipariş, kalem ya da adet iptali: adet, sebep, türü | VM4-07 |
| 26 | *çıkarma kaydı* | `02 §5.8` | Yöneticinin kalemi çıkarması ya da adedini azaltması | VM4-07 |
| 27 | LateDeliveryTermination | `02 §1.2`, §5.8 | Gecikme feshi: damga, kapsadığı kalemler ve adetler | VM4-07 |
| 28 | Withdrawal | `02 §1.2`, §5.8 | Cayma beyanı: damga, adet, iade adresi, teslim sırası cevabı | VM4-07 |
| 29 | Return | `02 §1.2`, §5.8 | İade teslim alma: ulaşma tarihi, ulaşan adet | VM4-07 |
| 30 | ReturnRejection | `02 §1.2`, §5.8 | İade reddi | VM4-07 |
| 31 | NotReturnedClosure | `02 §1.2`, §5.8 | "Mal dönmedi" kapanışı | VM4-07 |
| 32 | RefundIbanRequest | `02 §1.2`, §5.8 | Kaleme düşen IBAN isteği ve tarihi | VM4-07 |
| 33 | DefectClaim | `02 §1.2`, §5.8, §5.10 | Ayıp talebi: kalem, ayıplı adet, durum | VM4-07 |
| 34 | Refund | `02 §1.2`, §5.5 | Paranın müşteriye dönüşü: tür, tutar, yol, kalem bağı | VM4-08 |
| 35 | Customer | `02 §1.2`, §3.13 | Müşteri hesabı: ad, e-posta, şifre, doğrulama | VM4-09 |
| 36 | Admin | `02 §1.2`, §1.3 | Yönetici hesabı: ad, e-posta, şifre | VM4-09 |
| 37 | AdminInvitation | `02 §1.2`; Z-3 | Yönetici daveti | VM4-09 |
| 38 | Session | `02 §1.2`, §3.13.12 | Sunucudaki oturum | VM4-09 |
| 39 | SocialLogin | `02 §1.2`, §3.13.7 | Müşteri hesabının Google bağı | VM4-09 |
| 40 | RecognizedBrowserMark | `02 §1.2`; Z-46 | Hesap başına tanınan tarayıcı kaydı | VM4-09 |
| 41 | AddressBook | `02 §1.2`, §3.14 | Üyenin adlandırılmış adresleri | VM4-09 |
| 42 | *bekleyen e-posta değişikliği* | `02 §3.13.14` | Doğrulanmayı bekleyen yeni e-posta adresi | VM4-09 |
| 43 | *giriş kaydı* | `02 §3.13.20`; Z-40 | Her giriş denemesi: an, IP, hesap, sonuç | VM4-09 |
| 44 | *deneme kaydı* | `02 §3.13.17`; Z-27; park (K-901) | Sayılan her deneme: limit, eksen, değer, an | VM4-09 |
| 45 | *bağlantı kaydı* | `02 §3.13.5`, §3.13.9, §3.13.14; Z-1…Z-3, Z-45; park (K-903) | Hesap bağlantılarının özeti: tür, ömür, kullanılma | VM4-09 |
| 46 | Content | `02 §1.2`, §3.27, §5.2 | Kurumsal içerik kaydı: tip, yayın durumu, sıra | VM4-10 |
| 47 | AboutPage | `02 §1.2`, §3.27.3 | Tekil Hakkımızda | VM4-10 |
| 48 | Offering | `02 §1.2`, §3.27.4 | Hizmet tanıtımı | VM4-10 |
| 49 | PortfolioItem | `02 §1.2`, §3.27.5 | Referans iş | VM4-10 |
| 50 | FaqItem | `02 §1.2`, §3.27.6 | Sık sorulan soru | VM4-10 |
| 51 | Branch | `02 §1.2`, §3.27.7 | Şube | VM4-10 |
| 52 | CustomPage | `02 §1.2`, §3.27.9 | Genel sayfa | VM4-10 |
| 53 | Announcement | `02 §1.2`, §3.27.21 | Tekil duyuru | VM4-10 |
| 54 | *içerik–ürün bağı* | `02 §3.27.10` | Hizmet tanıtımının ve referans işin bağlı ürünleri | VM4-10 |
| 55 | PreInformationForm | `02 §1.2`, §3.24.3 | Ayarlardan üretilen form | VM4-11 |
| 56 | DistanceSalesContract | `02 §1.2`, §3.33.1 | Ayarlardan üretilen sözleşme | VM4-11 |
| 57 | PrivacyNotice | `02 §1.2`, §3.33.2 | Firmanın düzenlediği aydınlatma metni | VM4-11 |
| 58 | CookiePolicy | `02 §1.2`, §3.33.2 | Firmanın düzenlediği çerez politikası | VM4-11 |
| 59 | *yasal metin sürümü* | `02 §3.33.3` | Dört metnin sürümleri; siparişe donan sürüm | VM4-11 |
| 60 | ContactRequest | `02 §1.2`, §3.32 | İletişim talebi: konu tipi, durum, kapanış anı | VM4-12 |
| 61 | *gönderim kaydı* | `02 §1.1` (m4); Z-34, Z-43; park (K-892) | Her e-postanın gönderim satırı — `Notification` kavramının kaydı | VM4-13 |
| 62 | *iş kuyruğu* | park (K-891) | Dış yan etkilerin iş satırları | VM4-13 |
| 63 | *kanal hâli kaydı* | park (K-913) | E-posta göndericisinin tek satırlık kanal hâli | VM4-13 |
| 64 | *gelen bildirim kaydı* | park (K-893, K-954) | Sağlayıcının imzası doğru bildirimleri | VM4-08 |
| 65 | *geri ödeme kaydı* | park (K-894, K-940) | Karta para gönderen isteğin hâlleri | VM4-08 |
| 66 | *geçiş kaydı* | park (K-889) | Siparişin her durum geçişi: kim, ne zaman, hangi geçiş, sebep | VM4-06 |
| 67 | AuditLog | `02 §1.2`; Z-30 | İşlem izi | VM4-14 |
| 68 | *imha kaydı* | `02 §4.2` Z-44; park (K-875) | İmha çalışmalarının ve anında silmelerin tarihi, türü, sayısı | VM4-14 |
| 69 | *nabız ve kesinti kaydı* | park (K-895) | Zamanlayıcının nabzı, kesintiler, gecelik işin çalışma anları | VM4-14 |
| 70 | *yedeğin durum kaydı* | park (K-918) | Gecelik tam yedeğin sonucu — 1a satırlarında kullanılmadı, 1d eşler | VM4-14 |
| 71 | *anahtar halkası* | `05 §7.6.3` | Çatının şifreleme anahtar halkası — 1a satırlarında kullanılmadı, 1d eşler | VM4-14 |

Aday sayısı 71'dir: sözlükte kodu olan 46, italik 25. Sözlüğün `Notification` terimi ayrı aday değildir — kaydı gönderim kaydıdır (K-892); `Visitor`, `Buyer` ve `Consumer` aktör ya da sıfattır, kaydı yoktur. Kapalı listeler ve durum setleri aday değildir — §5'in enum'larıdır (VM5).

**"Veri doğurmaz" değer kümesi (K-999).** Veri doğurmayan bir kaynak satırı GAP değildir; "Entity" sütununa aşağıdaki sabit değerlerden biri adıyla yazılır. Bir satır hem bir adaya hem bir değere eşlenebilir (` · ` ile); en az bir aday taşıyan satırın durumu "eşlendi"dir.

| Değer | Ne zaman yazılır | Gerekçesi |
|---|---|---|
| veri doğurmaz — kod | Kural koddaki bir hesap, sınır, çit, sabit metin ya da ürün sabitidir | Ürün sabitleri ve kurallar koddadır, veritabanında değil (`05 §8.4`) |
| veri doğurmaz — türetilir | Değer başka alanlardan ya da bir koşuldan okunur, saklanmaz | Panel işaretleri ve sayaçlar koşuldan türer (K-675); satış kapısı ayarlardan (`02 §3.1.5`). Saklanıp saklanmayacağı tartışmalı türetilmiş değer bu değeri almaz, alanıyla eşlenir — kararı VM2-06'dır |
| veri doğurmaz — arayüz (`04`) | Satır yalnız gösterimi, metni ya da ekranın davranışını söyler | Arayüz Tanımları'nın işidir; kaydedilen bir değer yoktur |
| veri doğurmaz — mimari (`05`) | Karşılığı altyapıdadır: çerez, bellek, dosya, yedek, sistem kayıtları | Teknik Mimari'nin mekanizmasıdır; veritabanında satırı yoktur |
| veri doğurmaz — ürünün dışında | Sağlayıcının, bankanın, kargonun ya da firmanın sistem dışı yükümlülüğü | Ürün o adımı izlemez ve sayaç tutmaz |
| veri doğurmaz — e-posta (`08`) | Satırın çıktısı bir e-postanın içeriğidir | İçerik Entegrasyon Spesifikasyonu'nun işidir; gönderimin satırı gönderim kaydına eşlenir |
| veri doğurmaz — yazım konvansiyonu | Sözlük kuralı, atıf biçimi, dokümanın kendi düzeni | `06`'nın başlık notuna yazılır (K-994; VM0-06) |
| veri doğurmaz — kurulum | Kurulum tarafının işi: alan adı, sırlar, sağlayıcı anahtarları | Veritabanında değil, kurulumdadır (`02 §3.1.2`; `10 §4.1`) |
| veri doğurmaz — doğrulama (`12`) | Yalnız doğrulanan kural: tarayıcı tabanı, erişilebilirlik | Doğrulama ölçütüne dönüşür |
| yoktur — kapsam dışı | Kaynağın "yoktur" dediği yetenek ya da alan | Modelde yer tutmaz, yer ayırma alanı açılmaz (karar kaydı §14.2, kapsam notu) |

#### 1.1.3 MVP Kapsamı — `10 §2` KP satırları (77)

Kapsam ağıdır: her satır en az bir adaya ya da adıyla yazılmış bir "veri doğurmaz" değerine eşlenir (K-997). Sıra `10 §2`'nin sırasıdır — KP-77 komşu satırıyla birlikte okunduğu için üyelik grubundadır (`10 §2` girişi). Satırın ayrıntısı kuralın satırındadır (§1.1.4–§1.1.6 ve 1b).

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| KP-1 | Üç seviyeli kategori ağacında gezinme; alt dallar listelenir; boş kategori menüde yok | Category · *ürün–kategori bağı* | üst kategori bağı; menü görünürlüğü türetilir | eşlendi |
| KP-2 | Ürün adında arama; harf ve Türkçe karakter duyarsız | Product | ürün adının arama biçimi (K-886) | eşlendi |
| KP-3 | Fiyat aralığı ve stok durumuyla süzme; en yeni önce | Product · Variant | oluşturulma anı; fiyat; satın alınabilirlik türetilir | eşlendi |
| KP-4 | Varyant seçimi, en çok iki boyut; tükenmiş ya da açılmamış kombinasyon seçilemez | Variant · Option · OptionValue | varyantın seçenek değerleri | eşlendi |
| KP-5 | Ürün sayfası: görsel, açıklama, KDV dahil fiyat, fiyat tarihi, üretim yeri, yerli üretim logosu, birim fiyat, referans fiyat ve indirim tarihleri | Product · Variant · *görsel* · *fiyat geçmişi* · Discount | üretim yeri, ölçü birimi + net miktar, fiyat, fiyatın başlangıcı, indirim tarihleri; birim fiyat ve logo türetilir | eşlendi |
| KP-6 | Üç ürün tipi aynı katalog ve sepette; hizmette ifa süresi | Product | `ProductType`, ifa süresi | eşlendi |
| KP-7 | Arşiv sayfası fiyatsız; taslak ve silinmiş ürün "sayfa bulunamadı"; silinenin adresi serbest; taslak varyant görünmez | Product · Variant | `PublishStatus`, sayfa adresi (silmeyle serbest kalır) | eşlendi |
| KP-8 | Hesap açmadan sipariş; misafirin e-postası özette, düzeltilebilir; sonradan firma düzeltir | Order · OrderEmailHistory | iletişim e-postası; düzeltme geçmişi | eşlendi |
| KP-9 | Sepete ekleme; canlı fiyat; stok ve ürün sınırı; dijitalde adet bir; kalem tavanı yok; daha önce alınmış dijital uyarısı | CartItem · Product | adet, ilk eklendiği fiyat; bir siparişte en fazla adet | eşlendi |
| KP-10 | Üyenin sepeti hesapta, misafirinki tarayıcıda; girişte birleşir; kendiliğinden boşalmaz | Cart · CartItem | sahibi (hesap ya da tarayıcı anahtarı), son erişim anı | eşlendi |
| KP-11 | Kupon kodu; yüzde ya da sabit; sıfıra indiremez | Coupon · Order · OrderItem | kod, tür, değer, asgari sepet tutarı; siparişin kupon bağı; kalem payı | eşlendi |
| KP-12 | Teslimat ve fatura adresi; adres defteri; yalnız dijital ve hizmette teslimat adresi yok; adresin alanları; posta kodu ve kimlik numarası yok | Order · AddressBook · *il listesi* | `ShippingAddress`, `BillingAddress`; defter kaydı | eşlendi |
| KP-13 | Sabit kargo ücreti, ücretsiz kargoya kalan tutar, asgari tutar, teslimat illeri | *firma ayarları* · Order | kargo ücreti, eşik, asgari tutar, teslimat illeri; siparişin kargo ücreti | eşlendi |
| KP-14 | Onay özeti, ÖBF, sabit metin, cayma özeti, onay kutuları, ödeme yükümlülüğü düğmesi; sürüm değişince yeniden onay | *onay kaydı* · *yasal metin sürümü* · Order | kutuların kaydı; onaylanan sürümler | eşlendi |
| KP-15 | Kart (3D Secure) ya da havale; numara ve IBAN gösterilir; yarıda kalırsa yeni sipariş; süresinde ödenmeyen iptal; geç gelen kart ödemesi geri ödenir | Order · *ödeme kaydı* · *geri ödeme kaydı* · *gelen bildirim kaydı* | `PaymentMethod`, donmuş IBAN, ödeme süresinin bittiği an; ödeme ve otomatik geri ödemesi | eşlendi |
| KP-16 | Stok ödeme süresi boyunca ayrılır, onayda kesinleşir; kontenjan ve kupon hakkı aynı | StockReservation | ayrılmış adet ve hâli | eşlendi |
| KP-17 | Tahmin edilemez numara; tutar, kalem tanımı, adresler, iletişim e-postası, kargo sözü, firma kimliği, havale IBAN'ı, yasal metin sürümleri, onay kutuları donar | Order · OrderItem · *yasal metin sürümü* · *onay kaydı* | `OrderNumber` ve donan değerler (`02 §3.23`) | eşlendi |
| KP-18 | Sipariş sayfası: üç giriş yolu, erişim anahtarı; durumlar, geri ödemeler, takip, indirme, IBAN; kendiliğinden iptal edilen ödenmemiş sipariş müşteriye görünmez | Order · Refund · OrderItem | `OrderAccessKey`, `OrderStatus`, `PaymentStatus`; geri ödemelerin tarih, tutar ve yolu; iptalin türü | eşlendi |
| KP-19 | Kargoya verme süresi vitrinde; kargo şirketi ve takip numarası ya da "kendi aracımızla" | Product · Order | kargoya verme süresi; kargo şirketi, takip numarası, kendi aracıyla | eşlendi |
| KP-20 | Dijital ürünü indirme hakkı kadar indirme; güncel dosya | OrderItem · *dosya kaydı* | indirme hakkı ve sayacı; varyantın o anki dosyası | eşlendi |
| KP-21 | Müşterinin kalem ve adet iptali; 14 günde geri ödeme; havalede IBAN; ödemeden önce bütünüyle; gecikme feshi; başka kanaldan feshi firma kaydeder | Cancellation · LateDeliveryTermination · Refund | iptal edilen adet, IBAN (havalede), fesih damgası ve kapsamı | eşlendi |
| KP-22 | Cayma — adetle, ardışık beyanlar; pencereler; beyan damgası; iade malı gönderme; geri ödeme süresi; IBAN; başka kanaldan bildirimin kaydı; istisnalar | Withdrawal · Return · RefundIbanRequest · Refund · OrderItem | beyan damgası, adet, iade adresi, IBAN; donmuş istisna işareti ve sebebi | eşlendi |
| KP-23 | Ayıp talebi — ayıplı adetle; kanalın açılışı; iki yıllık zamanaşımı; çözüldü ve yeniden açılma | DefectClaim | kalem, ayıplı adet, durum | eşlendi |
| KP-24 | Kalem ve adet düzeyinde iptal ve iade; ardışık işlemler; kupon payı adetle; kargo ücretinin iki hâlde geri ödenmesi | Cancellation · Withdrawal · Refund · OrderItem | işlem başına adet, ayrılan kupon payı ve KDV (K-788) | eşlendi |
| KP-25 | Ad, e-posta ve şifreyle hesap; kapı; doğrulanana kadar çalışmaz | Customer | ad, e-posta, şifre özeti, doğrulanmış mı | eşlendi |
| KP-26 | Google ile giriş; eşleşme; bekleyen kayıt silinir; doğrulanmamış e-postada hesap açılmaz | SocialLogin · Customer | Google bağı | eşlendi |
| KP-27 | Uzun oturum; şifre sıfırlama ve değiştirme; ad ve e-posta değişikliği; geri alma; yeniden doğrulama; oturumların ve işaretlerin düşmesi | Session · Customer · *bekleyen e-posta değişikliği* · *bağlantı kaydı* · RecognizedBrowserMark | oturum türü; bekleyen değişiklik; geri alma bağlantısı | eşlendi |
| KP-28 | Misafir siparişlerinin hesaba düşmesi; e-posta düzeltmesinde bağın değişmesi | Order · Customer | hesap bağı | eşlendi |
| KP-29 | Hesabın silinmesi — hemen silinenler; siparişler ticari kayıt; e-posta; geri alma bağlantısı açıkken silinmez | Customer · Session · AddressBook · Cart · RecognizedBrowserMark · Order · *imha kaydı* | anında fiziksel silme; siparişte hesap bağı kalkar | eşlendi |
| KP-30 | KVKK başvurusu iletişim formunun "KVKK talebi" tipiyle | ContactRequest | `ContactTopic` | eşlendi |
| KP-77 | Üye kaydı görünümü — e-postayla arama, salt okunur; talep üzerine silme, işlem izine | Customer · AuditLog | e-postayla arama (harf duyarsız); silmenin iz satırı | eşlendi |
| KP-31 | Ana sayfada tanıtım ve vitrin birlikte; düzen sırayı değiştirir; vitrin en yeni ürünler | *firma ayarları* · AboutPage | `HomepageLayout` | eşlendi |
| KP-32 | Hakkımızda, hizmet tanıtımı, referans iş, SSS, genel sayfa; "İlgili ürünler" | Content · *içerik–ürün bağı* | — | eşlendi |
| KP-33 | İletişim sayfası — iletişim bilgileri, şubeler, form | Company · Branch | iletişim bilgileri; şubeler | eşlendi |
| KP-34 | İletişim formu girişsiz; talep panele düşer | ContactRequest | — | eşlendi |
| KP-35 | Yasal kimlik ve iletişim bilgileri her sayfada; ETBİS bandı; yasal metin ve "İşlem rehberi" bağlantıları | Company · veri doğurmaz — arayüz (`04`) | kimlik alanları, KEP, işletme adı ya da marka, meslek odası, davranış kuralları, ETBİS bağlantısı | eşlendi |
| KP-36 | Sosyal medya ve WhatsApp bağlantıları; duyuru; "Paylaş" | SocialLink · Announcement | — | eşlendi |
| KP-37 | Arama motoru bilgileri kendiliğinden; kapalı sayfalar; adresler ilk yayında sabit | Product · Category · Content | sayfa adresi; arama motoru bilgileri türetilir | eşlendi |
| KP-38 | Satış kapalıyken vitrin yayında, sepet ve ödeme kapalı | *firma ayarları* · veri doğurmaz — türetilir | geçici kapatma anahtarı; kapı türetilir | eşlendi |
| KP-39 | Ürünü ad, ana kategori ve tiple yayına alma; varyant, stok kodu, fiyat, KDV, stok, üretim yeri, ölçü birimi, ifa süresi, kontenjan; ayrılmış adet; ürün listesinde arama ve süzme | Product · Variant · StockReservation | ürün ve varyant alanları; ayrılmış adet; adla arama | eşlendi |
| KP-40 | Yayın durumları arasında taşıma; önizleme; kalıcı silme; açık siparişli silmede sayı | Product · Variant · OrderItem | `PublishStatus`; fiziksel silme | eşlendi |
| KP-41 | Kategori ağacı, taşıma, çoklu kategori, ana kategori; dolu kategori silinemez | Category · *ürün–kategori bağı* · Product | `PrimaryCategory` | eşlendi |
| KP-42 | Görsel yükleme, sıralama, ana görsel, alternatif metin; biçimli açıklama | *görsel* · Product | sıra, ana görsel işareti, alternatif metin; açıklama | eşlendi |
| KP-43 | Tarihli yüzde indirim; referans fiyatı sistem hesaplar; kupon tanımı ve sınırları | Discount · *fiyat geçmişi* · Coupon | indirim alanları; fiyat geçmişi; kupon alanları ve hakları | eşlendi |
| KP-44 | Dijital dosya yükleme ve güncelleme; indirme hakkının yenilenmesi | *dosya kaydı* · OrderItem | dosya kaydı; indirme sayacı | eşlendi |
| KP-45 | Cayma istisnası işareti ve sebebi; mutlak ya da koşullu | Product · OrderItem | istisna işareti, `WithdrawalExemptionReason`; kaleme donar | eşlendi |
| KP-46 | Sipariş listesinde numara ya da e-postayla arama, durumla süzme; havale "ödendi"; kargoya verme; teslim; teslim edilemedi; yeniden gönderim; iptal; hizmet tamamlama | Order · OrderItem · *geçiş kaydı* | `OrderNumber`, iletişim e-postası; durumlar; kargo bilgisi; teslim işaretleri | eşlendi |
| KP-47 | Firma iptali sebepli; kart geri ödemesi; başarısızlık ve sonuç bekleniyor; havale geri ödemesi; iade teslim alma; iade reddi; kanuni faiz uyarısı; havale gerçekleşmedi; başka kanaldan caymanın kaydı ve teyidi | Cancellation · Refund · *geri ödeme kaydı* · Return · ReturnRejection · Withdrawal · RefundIbanRequest | sebep, adet; geri ödemenin hâli; ulaşma tarihi ve adet; ret; kaydeden ve bildirimin ulaştığı tarih | eşlendi |
| KP-48 | Müdahaleler: adres düzeltmesi, adet azaltma ve çıkarma, takip bilgisi, teslim ve ulaşma tarihinin düzeltilmesi, geçiş düzeltmesi, iç not, "mal dönmedi", başka kanal kaydı, misafir e-postası düzeltmesi; elle sipariş ve fiyat değişikliği yok; tutar bazlı kısmi geri ödeme | Order · *çıkarma kaydı* · *geçiş kaydı* · NotReturnedClosure · OrderEmailHistory · Refund · AuditLog | düzeltilen alanlar; iç not; düzeltme geçmişi; tutar bazlı geri ödeme; iz satırları | eşlendi |
| KP-49 | Ayıp talebini çözme ve yeniden açma; iletişim taleplerini kapatma ve yeniden açma | DefectClaim · ContactRequest | durumlar | eşlendi |
| KP-50 | Panel ana sayfasında altı sayaç; kanal uyarısı; firma bildirimi uyarısı; "e-posta ulaşmadı" süzgeci | veri doğurmaz — türetilir · *kanal hâli kaydı* · *gönderim kaydı* | sayaçlar saklanmaz (K-675); kanal hâli; gönderim sonucu | eşlendi |
| KP-51 | Manuel adım bütçesi; fatura sistem dışında | veri doğurmaz — kod | — | veri doğurmaz |
| KP-52 | Sipariş, üye listesi ve iletişim talepleri CSV dışa aktarma; işlem izine | Order · Customer · ContactRequest · AuditLog | dışa aktarılan alanlar kayıtlardan; dışa aktarmanın iz satırı | eşlendi |
| KP-53 | Satış özeti ölçüleri — kayıtlardan hesaplanır, yeni veri tutmaz | veri doğurmaz — türetilir | — (sipariş, ödeme ve iletişim talebinden) | veri doğurmaz |
| KP-54 | Kurumsal içerik girişi: taslak, önizleme, yayın, sıralama, ana sayfa, ürün bağı, silme | Content · *içerik–ürün bağı* | `PublishStatus`, sıra, ana sayfada göster | eşlendi |
| KP-55 | Ana sayfa düzeni, menü adları, genel sayfayı menüde gösterme, sosyal bağlantılar ve WhatsApp | *firma ayarları* · CustomPage · SocialLink | `HomepageLayout`, menü adları; menüde göster işareti | eşlendi |
| KP-56 | Logo, marka adı, site simgesi, marka rengi; platform imzası | Branding · *firma ayarları* | `Logo`, `BrandName`, `Favicon`, `BrandColor`; platform imzası anahtarı | eşlendi |
| KP-57 | Duyuru kurumsal içerik kaydı; taslak, önizleme, tarihli yayın | Announcement | metin, bağlantı, tarihler, `PublishStatus` | eşlendi |
| KP-58 | Firma kimliği — tip, zorunlu alanlar, KEP, isteğe bağlı alanlar, ETBİS; silinmez | Company | kimlik alanları | eşlendi |
| KP-59 | Havale ve IBAN (siparişe donar); kargo ücreti, eşik, asgari tutar, teslimat illeri, süreler, iade adresi | *firma ayarları* · Order · Withdrawal | ayarlar; siparişin donmuş IBAN'ı; beyanın iade adresi | eşlendi |
| KP-60 | Satışın geçici kapatılması; açık siparişler etkilenmez | *firma ayarları* | geçici kapatma anahtarı | eşlendi |
| KP-61 | Aydınlatma metni ve çerez politikası taslaktan düzenlenip yayına alınır; dört metin sürümlü | PrivacyNotice · CookiePolicy · *yasal metin sürümü* | metin, sürüm | eşlendi |
| KP-62 | Yönetici daveti, kaldırma, geri çekme; ad; bildirim; aynı yetki; son yönetici kaldırılamaz; ayrı hesap türü; e-posta değişikliği | Admin · AdminInvitation · *bağlantı kaydı* · *gönderim kaydı* · AuditLog | ad, e-posta, şifre; davet ve geri çekilme; F-6 satırları | eşlendi |
| KP-63 | İşlem izi — kapsam alanları, eski ve yeni değerle; tarih ve yönetici süzgeci; on yıl değiştirilemez | AuditLog | an, yönetici (değer olarak), alan, kayıt kimliği, eski ve yeni değer | eşlendi |
| KP-64 | İşletme parametreleri — on firma ayarı, kurulumda dolu; ürün sabitleri değişmez | *firma ayarları* · veri doğurmaz — kod | firma ayarları (varsayılanları başlangıç verisi — VM8-01); ürün sabitleri kodda | eşlendi |
| KP-65 | Kurulum kontrol listesi; site boş kurulur | veri doğurmaz — türetilir | — (ayarlardan — `02 §10.8`) | veri doğurmaz |
| KP-66 | Müşteriye on altı olayın e-postası, düzeltmeler, IBAN isteği, e-posta düzeltme bildirimi, iade reddi | *gönderim kaydı* · veri doğurmaz — e-posta (`08`) | gönderim satırı; içerik `08`'in | eşlendi |
| KP-67 | Firmaya bildirimler; IBAN ve yönetici olaylarında bütün yöneticilere | *gönderim kaydı* | gönderim satırı | eşlendi |
| KP-68 | Yanıt firmanın iletişim e-postasına; üç yeniden deneme; "e-posta ulaşmadı" işareti; yeniden gönderim; firma bildirimi uyarısı | *gönderim kaydı* · *kanal hâli kaydı* · Company | deneme sayısı, sonuç; iletişim e-postası (yanıt adresi) | eşlendi |
| KP-69 | Tek adres, duyarlı site; panel telefondan | veri doğurmaz — arayüz (`04`) | — | veri doğurmaz |
| KP-70 | Tarayıcı tabanı | veri doğurmaz — doğrulama (`12`) | — | veri doğurmaz |
| KP-71 | Erişilebilirlik tabanı ve yedi kural | veri doğurmaz — doğrulama (`12`) · *görsel* | alternatif metin | eşlendi |
| KP-72 | Deneme limitleri; açık ödenmemiş havale tavanı; tanınan tarayıcı; giriş kaydı altı ay | *deneme kaydı* · Order · RecognizedBrowserMark · *giriş kaydı* | deneme satırları; IP ve hesap bağı; giriş kaydının alanları | eşlendi |
| KP-73 | Yalnız zorunlu çerezler; üçüncü taraf ve ölçüm yok | veri doğurmaz — mimari (`05`) | — | veri doğurmaz |
| KP-74 | Aydınlatma bağlantısı; açık rıza yok; her gün imha; imha kaydı en az üç yıl | *imha kaydı* · veri doğurmaz — arayüz (`04`) | tarih, kayıt türü, sayı | eşlendi |
| KP-75 | Sipariş tarafında veri kaybı yok; disk kaybı istisnası; sepet ve oturum kaybı kabul | Order · *ödeme kaydı* · *geçiş kaydı* · *gönderim kaydı* · AuditLog · veri doğurmaz — mimari (`05`) | sipariş tarafı kayıtların sınıfı; yedek `05 §7.6` | eşlendi |
| KP-76 | Sessiz üzerine yazma yok; siparişe işlemler güncel hâle karşı | Product · Content · Coupon · *firma ayarları* · Order | sürüm değeri (K-873, K-874) | eşlendi |

#### 1.1.4 Ürün Gereksinimleri — `02 §0`–§2: sözlük, aktörler ve temel akış (147)

Sözlük terimi kod karşılığıyla, bölümlerin kural numarası taşımayan maddeleri ve metni K-1000'in birimiyle girer. Terimin alanları kuralın satırında ayrıca eşlenir; terim satırı adayın ya da alanın adını sabitler (K-17).

**`02 §0` ve §1'in kural dışı metni (16)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §0` m1 | Her iş kuralı numaralanır ve bölüm referansıyla anılır | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §0` m2 | Sayısal parametreler tablo hâlinde toplanır | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §0` m3 | Firma ayarı ile ürün sabiti ayrımı; katman §11'de | *firma ayarları* · veri doğurmaz — kod | firma ayarı olan parametreler veride, ürün sabitleri kodda (`05 §8.4`) — liste `02 §11` (1b) | eşlendi |
| `02 §0` m4 | Belirsiz ifade yasak; varlık kararı burada | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §1.1` metin | Sözlük üç sütunlu, burada yaşar; gerekçe ve bedeli — `06` yazılırken birkaç satır revize edilebilir | veri doğurmaz — yazım konvansiyonu | — (adlar sözlükten — VM0-06) | veri doğurmaz |
| `02 §1.1` m1 | İngilizce karşılık Aşama 1'de yazılır | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §1.1` m2 | Bir kavramın tek adı vardır | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §1.1` m3 | `06`, `07`, `09` sözlüğü birebir devralır, isim icat etmez | veri doğurmaz — yazım konvansiyonu | — (VM0-06) | veri doğurmaz |
| `02 §1.1` m4 | Sözlük ürünün terimlerini taşır; teknik kavramların kod adını `06` `05`'teki adından verir; ürüne görünen kavram sözlüğe girer | veri doğurmaz — yazım konvansiyonu | — (teknik kavramlar VM3-03; sözlükte terimi olmayan ürün kayıtları VA-1) | GAP adayı (VA-1) |
| `02 §1.1` m5 | Kapalı listelerin değer kod adları `06`'da verilir | veri doğurmaz — yazım konvansiyonu | — (değerler VM5-03) | veri doğurmaz |
| `02 §1.2` metin | Kod karşılıklarının kaynağı († işareti); kapsam notu — ürün türü denetimi ve mevzuat belgesi alanı yok; ölçek notu — ölçek sınırı uygulanmaz, iki seçenek boyutu sınırı geçerli | yoktur — kapsam dışı · Option | mevzuat belgesi ve yasaklı kategori alanı yok; varyant ve seçenek değeri sayısına tavan yok; ürün başına en çok iki seçenek | eşlendi |
| `02 §1.3` metin | Dört aktör; tek başına "müşteri" ve "üye"nin anlamı | veri doğurmaz — yazım konvansiyonu | — (VM0-07) | veri doğurmaz |
| `02 §1.3` m1 | Yönetim tek rol, çoklu kullanıcı; yetki matrisi yok; kimlik doğrulama müşteriyle aynı; yönetici hesabıyla alışveriş yok, iki hesap bağlanmaz | Admin | rol ve izin alanı yok; müşteri hesabına bağ yok | eşlendi |
| `02 §1.3` m2 | Ürün tüketiciye satış için; ticari alıcıya özel akış ve alan yok | yoktur — kapsam dışı | ticari alıcı alanı yok | veri doğurmaz |
| `02 §1.3` m3 | Üyelik zorunlu değil; numara + e-postayla takip; doğrulanmış e-postayla bağlanma; silinmiş hesabın siparişi düşmez | Order · Customer | hesap bağı isteğe bağlı; iletişim e-postası | eşlendi |
| `02 §1.3` m4 | Platform operatörü aktör değil; çok kiracılık yok | yoktur — kapsam dışı | kiracı ve operatör alanı yok (SK-1; K-10) | veri doğurmaz |

**`02 §1.2` — terim sözlüğü (105)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §1.2` `Product` | Ürün — vitrin birimi; ad, açıklama, kategori, tip, KDV, indirim, üretim yeri, ölçü birimi ve net miktar; görsel | Product | — (alanlar `02 §3`'ün kurallarında) | eşlendi |
| `02 §1.2` `Variant` | Varyant — satılan birim; stok ve fiyat | Variant | — | eşlendi |
| `02 §1.2` `Option` | Seçenek — boyutun adı; en fazla iki | Option | ad | eşlendi |
| `02 §1.2` `OptionValue` | Seçenek değeri | OptionValue | değer | eşlendi |
| `02 §1.2` `SKU` | Stok kodu — zorunlu, tekil | Variant | `SKU` | eşlendi |
| `02 §1.2` `ProductType` | Ürün tipi — üç değerli kapalı alan, ürün düzleminde | Product | `ProductType` | eşlendi |
| `02 §1.2` `Physical` | Fiziksel ürün — stok zorunlu | Product | `ProductType` değeri | eşlendi |
| `02 §1.2` `Digital` | Dijital ürün — stok yok | Product | `ProductType` değeri | eşlendi |
| `02 §1.2` `Service` | Hizmet — isteğe bağlı kontenjan, zorunlu ifa süresi | Product · Variant | `ProductType` değeri; ifa süresi; kontenjan | eşlendi |
| `02 §1.2` `Category` | Kategori — en çok üç seviye | Category | — | eşlendi |
| `02 §1.2` `PrimaryCategory` | Ana kategori — kırıntı yolu; yayın için zorunlu | Product | `PrimaryCategory` | eşlendi |
| `02 §1.2` `PublishStatus` | Yayın durumu — ürün ve varyantta üç değer, içerikte iki | Product · Variant · Content | `PublishStatus` | eşlendi |
| `02 §1.2` `Draft` | Taslak | Product · Variant · Content | `PublishStatus` değeri | eşlendi |
| `02 §1.2` `Published` | Yayında — en az bir Yayında varyant; içerikte zorunlu alanlar | Product · Variant · Content | `PublishStatus` değeri | eşlendi |
| `02 §1.2` `Archived` | Arşiv — silme değil; içerikte yok | Product · Variant | `PublishStatus` değeri | eşlendi |
| `02 §1.2` `StockQuantity` | Stok — varyantın adedi; ürün düzleminde yok | Variant | `StockQuantity` | eşlendi |
| `02 §1.2` `StockReservation` | Stok ayırma — onayda ayrılır, ödemede düşer, dönebilir; kontenjan ve kupon hakkı aynı | StockReservation | — | eşlendi |
| `02 §1.2` `VatRate` | KDV oranı — ürün düzleminde; varsayılanı ayardan | Product · *firma ayarları* | `VatRate`; varsayılan oran | eşlendi |
| `02 §1.2` `Discount` | İndirim — ürün düzleminde yüzdesel, tarihli; sıfıra indiremez | Discount | — | eşlendi |
| `02 §1.2` `ReferencePrice` | Referans fiyat — sistem hesaplar | *fiyat geçmişi* · OrderItem | hesap fiyat geçmişinden; kaleme donan `ReferencePrice` | eşlendi |
| `02 §1.2` `Coupon` | Kupon — sepet toplamına iner; yüzde ya da sabit; tarih ve kullanım sınırı | Coupon | — | eşlendi |
| `02 §1.2` `Company` | Firma — kurulumun sahibi, tekil; unvan ve marka adı; veri sorumlusu | Company | — | eşlendi |
| `02 §1.2` `CompanyType` | Firma tipi — üç değerli kapalı liste | Company | `CompanyType` (değerler VM5-03) | eşlendi |
| `02 §1.2` `Visitor` | Ziyaretçi | veri doğurmaz — yazım konvansiyonu | — (aktör; kaydı yok) | veri doğurmaz |
| `02 §1.2` `GuestBuyer` | Misafir alıcı — numara + e-postayla takip; e-posta düzeltilebilir | Order | hesap bağı boş sipariş; iletişim e-postası | eşlendi |
| `02 §1.2` `Customer` | Üye müşteri | Customer | — | eşlendi |
| `02 §1.2` `Buyer` | Müşteri — aktör değil, ortak ad | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §1.2` `Admin` | Firma yöneticisi — tek rol; ayrı hesap türü; e-posta kendi türünde tekil | Admin | e-posta (türünde tekil) | eşlendi |
| `02 §1.2` `Consumer` | Tüketici — hukuki sıfat, aktör değil | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §1.2` `Session` | Oturum — kısa ya da uzun; düşme hâlleri | Session | — | eşlendi |
| `02 §1.2` `SocialLogin` | Sosyal giriş — yalnız Google; doğrulanmış e-postayla bağ | SocialLogin | — | eşlendi |
| `02 §1.2` `AddressBook` | Adres defteri — üyenin adlandırılmış adresleri; misafirin yok | AddressBook | — | eşlendi |
| `02 §1.2` `ShippingAddress` | Teslimat adresi — ad, telefon, il, ilçe, açık adres; siparişe donar | Order | `ShippingAddress` | eşlendi |
| `02 §1.2` `BillingAddress` | Fatura adresi — telefonsuz; siparişe donar | Order | `BillingAddress` | eşlendi |
| `02 §1.2` `AccountType` | Hesap türü — yönetici ve müşteri; ayrı kapılar | Customer · Admin · Session | `AccountType` (değerler VM5-03) — iki ayrı tablo (K-899) | eşlendi |
| `02 §1.2` `RecognizedBrowserMark` | Tanınan tarayıcı işareti — zorunlu çerez; ömrü P-48 | RecognizedBrowserMark | — | eşlendi |
| `02 §1.2` `Cart` | Sepet — üyede hesapta, misafirde tarayıcıda; kendiliğinden boşalmaz | Cart | — | eşlendi |
| `02 §1.2` `CartItem` | Sepet kalemi — canlı; ilk eklendiği fiyatı hatırlar | CartItem | — | eşlendi |
| `02 §1.2` `Order` | Sipariş — onayda oluşur, numara, iki eksen; donan değerler | Order | — | eşlendi |
| `02 §1.2` `OrderItem` | Sipariş kalemi — donan değerler; adetle iptal ve iade; teslim işareti | OrderItem | — | eşlendi |
| `02 §1.2` `DeliveryDate` | Teslim tarihi — kalemin teslim işaretinin tarihi | OrderItem | `DeliveryDate` | eşlendi |
| `02 §1.2` `OrderNumber` | Sipariş numarası — tahmin edilemez, tekrar kullanılmaz | Order | `OrderNumber` | eşlendi |
| `02 §1.2` `OrderAccessKey` | Erişim anahtarı — siparişe özgü; e-posta düzeltmesinde yenilenir | Order | `OrderAccessKey` | eşlendi |
| `02 §1.2` `OrderEmailHistory` | E-posta geçmişi — eski ve yeni adres, yönetici, tarih; siparişle imha | OrderEmailHistory | eski adres, yeni adres, yönetici, tarih | eşlendi |
| `02 §1.2` `OrderStatus` | Sipariş durumu — sevkiyat ekseni | Order | `OrderStatus` | eşlendi |
| `02 §1.2` `Placed` | Alındı | Order | `OrderStatus` değeri | eşlendi |
| `02 §1.2` `Preparing` | Hazırlanıyor | Order | `OrderStatus` değeri | eşlendi |
| `02 §1.2` `Shipped` | Kargoya verildi | Order | `OrderStatus` değeri | eşlendi |
| `02 §1.2` `Delivered` | Teslim edildi — terminal | Order | `OrderStatus` değeri | eşlendi |
| `02 §1.2` `DeliveryFailed` | Teslim edilemedi | Order | `OrderStatus` değeri | eşlendi |
| `02 §1.2` `Cancelled` | İptal edildi — terminal; açık kalemsiz kapanış da | Order | `OrderStatus` değeri | eşlendi |
| `02 §1.2` `PaymentMethod` | Ödeme yöntemi — kart ve havale | Order | `PaymentMethod` | eşlendi |
| `02 §1.2` `BankTransfer` | Havale/EFT — firma açar, IBAN zorunlu; firma "ödendi" işaretler | Order · *firma ayarları* | `PaymentMethod` değeri; havale açık mı, IBAN | eşlendi |
| `02 §1.2` `PaymentWindow` | Ödeme süresi — kartta sabit, havalede firma ayarı | Order · *firma ayarları* | ödeme süresinin bittiği an; havale ödeme süresi | eşlendi |
| `02 §1.2` `PaymentStatus` | Ödeme durumu — ödeme ekseni | Order | `PaymentStatus` | eşlendi |
| `02 §1.2` `Pending` | Bekliyor | Order | `PaymentStatus` değeri | eşlendi |
| `02 §1.2` `Paid` | Ödendi | Order | `PaymentStatus` değeri | eşlendi |
| `02 §1.2` `Failed` | Başarısız — terminal; geç gelen ödeme ödeme kaydında | Order · *ödeme kaydı* | `PaymentStatus` değeri | eşlendi |
| `02 §1.2` `PartiallyRefunded` | Kısmen geri ödendi | Order | `PaymentStatus` değeri | eşlendi |
| `02 §1.2` `Refunded` | Geri ödendi — terminal | Order | `PaymentStatus` değeri | eşlendi |
| `02 §1.2` `Chargeback` | Ters ibraz — ürünün dışında; kayıt ve durum yok | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §1.2` `Cancellation` | İptal — sipariş, kalem ya da adet; firma sebep seçer | Cancellation | — | eşlendi |
| `02 §1.2` `CancellationReason` | İptal sebebi — kapalı liste; "diğer"de açıklama | Cancellation | `CancellationReason` (değerler VM5-03), açıklama | eşlendi |
| `02 §1.2` `Withdrawal` | Cayma — adetle; pencereler; başka kanaldan firmanın kaydı; uzayan süre | Withdrawal | — | eşlendi |
| `02 §1.2` `WithdrawalExemptionReason` | Cayma istisnası sebebi — kapalı liste, ürün düzleminde | Product · OrderItem | `WithdrawalExemptionReason` (değerler VM5-03); kaleme donar | eşlendi |
| `02 §1.2` `LateDeliveryTermination` | Gecikme feshi — ayrı kayıt tipi; açık adetlerin tamamı | LateDeliveryTermination | — | eşlendi |
| `02 §1.2` `Return` | İade — malın dönüşü; ulaşma tarihi ve adet, parça parça | Return | — | eşlendi |
| `02 §1.2` `Refund` | Geri ödeme — paranın dönüşü, geldiği yoldan | Refund | — | eşlendi |
| `02 §1.2` `NotReturnedClosure` | "Mal dönmedi" kapanışı | NotReturnedClosure | — | eşlendi |
| `02 §1.2` `ReturnRejection` | İade reddi | ReturnRejection | — | eşlendi |
| `02 §1.2` `RefundIbanRequest` | IBAN isteği — kaleme düşen istek ve tarihi | RefundIbanRequest | — | eşlendi |
| `02 §1.2` `RefundFailedMark` | "Geri ödeme gerçekleşmedi" işareti — panel satırında | Order · *geri ödeme kaydı* | `RefundFailedMark` — geri ödeme kaydının hâliyle aynı işlemde (K-940) | eşlendi |
| `02 §1.2` `RefundOutcomePendingMark` | "Geri ödeme sonucu bekleniyor" işareti | Order · *geri ödeme kaydı* | `RefundOutcomePendingMark` | eşlendi |
| `02 §1.2` `DefectClaim` | Ayıp talebi — kalem ve ayıplı adetle; iki durum | DefectClaim | — | eşlendi |
| `02 §1.2` `DefectClaimStatus` | Ayıp talebi durumu | DefectClaim | `DefectClaimStatus` | eşlendi |
| `02 §1.2` `Open` | Açık — iki durum alanında ortak değer | DefectClaim · ContactRequest | durum değeri | eşlendi |
| `02 §1.2` `Resolved` | Çözüldü | DefectClaim | `DefectClaimStatus` değeri | eşlendi |
| `02 §1.2` `PreInformationForm` | Ön Bilgilendirme Formu — ayarlardan üretilir; sürüm siparişe donar | PreInformationForm · *yasal metin sürümü* · Order | sürüm; siparişe donan sürüm bağı | eşlendi |
| `02 §1.2` `DistanceSalesContract` | Mesafeli Satış Sözleşmesi — ayarlardan üretilir; sürüm ve kimlik siparişe donar | DistanceSalesContract · *yasal metin sürümü* · Order | sürüm; siparişe donan sürüm bağı | eşlendi |
| `02 §1.2` `PrivacyNotice` | Aydınlatma metni — taslakla gelir, firma düzenler; sürümü siparişe donar | PrivacyNotice · *yasal metin sürümü* · Order | metin, sürüm; siparişe donan sürüm bağı | eşlendi |
| `02 §1.2` `CookiePolicy` | Çerez politikası — taslakla gelir, firma düzenler | CookiePolicy · *yasal metin sürümü* | metin, sürüm | eşlendi |
| `02 §1.2` `Content` | Kurumsal içerik — yedi tip; her kayıt kendi yayın durumu | Content | — | eşlendi |
| `02 §1.2` `AboutPage` | Hakkımızda — tekil; silinmez | AboutPage | — | eşlendi |
| `02 §1.2` `Offering` | Hizmet tanıtımı — fiyatsız; ürün bağı | Offering | — | eşlendi |
| `02 §1.2` `PortfolioItem` | Referans iş — ürün bağı | PortfolioItem | — | eşlendi |
| `02 §1.2` `FaqItem` | Sık sorulan soru | FaqItem | — | eşlendi |
| `02 §1.2` `Branch` | Şube — ad ve adres zorunlu | Branch | — | eşlendi |
| `02 §1.2` `CustomPage` | Genel sayfa | CustomPage | — | eşlendi |
| `02 §1.2` `Announcement` | Duyuru — tekil; tarih aralığı; yayın durumu | Announcement | — | eşlendi |
| `02 §1.2` `SocialLink` | Sosyal medya bağlantısı — kapalı platform listesi; WhatsApp | SocialLink | platform, bağlantı | eşlendi |
| `02 §1.2` `Branding` | Marka kimliği — dört ayar; siparişe donmaz; platform imzası ayrı | Branding | — | eşlendi |
| `02 §1.2` `BrandName` | Marka adı — zorunlu | Branding | `BrandName` | eşlendi |
| `02 §1.2` `LegalName` | Unvan — tipe göre; siparişe donar | Company · Order | `LegalName`; donmuş unvan | eşlendi |
| `02 §1.2` `Logo` | Logo — zorunlu değil | Branding | `Logo` | eşlendi |
| `02 §1.2` `Favicon` | Site simgesi — isteğe bağlı | Branding | `Favicon` | eşlendi |
| `02 §1.2` `BrandColor` | Marka rengi | Branding | `BrandColor` | eşlendi |
| `02 §1.2` `HomepageLayout` | Ana sayfa düzeni — iki değerli kapalı liste | *firma ayarları* | `HomepageLayout` (değerler VM5-03) | eşlendi |
| `02 §1.2` `ContactRequest` | İletişim talebi | ContactRequest | — | eşlendi |
| `02 §1.2` `ContactRequestStatus` | İletişim talebi durumu — Açık, Kapatıldı | ContactRequest | `ContactRequestStatus` | eşlendi |
| `02 §1.2` `Closed` | Kapatıldı — yeniden açılabilir | ContactRequest | `ContactRequestStatus` değeri | eşlendi |
| `02 §1.2` `ContactTopic` | Konu tipi — beş değerli kapalı liste | ContactRequest | `ContactTopic` (değerler VM5-03) | eşlendi |
| `02 §1.2` `AdminInvitation` | Yönetici daveti — tek kullanımlık, süreli; geri çekilebilir | AdminInvitation · *bağlantı kaydı* | davet edilen e-posta, daveti gönderen; bağlantı | eşlendi |
| `02 §1.2` `CorrectionReason` | Düzeltme sebebi — kapalı liste; "diğer"de açıklama | *geçiş kaydı* · AuditLog | `CorrectionReason` (değerler VM5-03), açıklama | eşlendi |
| `02 §1.2` `AuditLog` | İşlem izi — kim, ne zaman, neyi; on yıl değiştirilemez | AuditLog | — | eşlendi |
| `02 §1.2` `Notification` | Bildirim — e-posta; firma bildirimleri iletişim e-postasına, F-5 ve F-6 yöneticilere | *gönderim kaydı* | bildirimin satırı — gövde saklanmaz (B-, F- listesi `02 §9`, 1b) | eşlendi |

**`02 §2` — temel akış (26)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §2` metin | Sekiz akış; satış doğrudan; teklif hattı yok; üç tip aynı sepetten | veri doğurmaz — yazım konvansiyonu | — (akışların verisi `02 §3`'ün satırlarında) | veri doğurmaz |
| `02 §2.1` m1 | Akış 1 — satın alma | Cart · Order | — | eşlendi |
| `02 §2.1` m2 | Akış 2 — sipariş takibi, üç yol | Order | `OrderNumber`, iletişim e-postası, `OrderAccessKey`, hesap bağı | eşlendi |
| `02 §2.1` m3 | Akış 3 — iptal, cayma, iade; ayıp talebi | Cancellation · Withdrawal · Return · DefectClaim | — | eşlendi |
| `02 §2.1` m4 | Akış 4 — üyelik | Customer | — | eşlendi |
| `02 §2.1` m5 | Akış 5 — katalog yönetimi | Product · Variant · Discount · Coupon | — | eşlendi |
| `02 §2.1` m6 | Akış 6 — sipariş yürütümü; müdahaleler | Order · OrderItem · Refund | — | eşlendi |
| `02 §2.1` m7 | Akış 7 — kurumsal içerik; üç kol | Content · Branding · ContactRequest | — | eşlendi |
| `02 §2.1` m8 | Akış 8 — mağaza ayarları: kimlik, ödeme yöntemleri, kargo, eşikler, iade adresi, yasal metinler, yöneticiler | Company · *firma ayarları* · *yasal metin sürümü* · Admin | — | eşlendi |
| `02 §2.2` metin | Ana akış fiziksel ürünün yolculuğu; dijital ve hizmet teslimde ayrılır; hiçbir adım e-postaya bağlı değil | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §2.2` m1 | Ürünü bulur, varyant seçer | veri doğurmaz — arayüz (`04`) | — | veri doğurmaz |
| `02 §2.2` m2 | Varyantı adediyle sepete ekler | CartItem | varyant, adet | eşlendi |
| `02 §2.2` m3 | Ödeme adımı: adres defteri ya da yeni adres; misafir adres ve e-posta; kupon ve yöntem | Order · AddressBook | adresler, iletişim e-postası, kupon, `PaymentMethod` | eşlendi |
| `02 §2.2` m4 | Onay özeti, ÖBF, cayma özeti, iki kutu, ödeme yükümlülüğü | *onay kaydı* · veri doğurmaz — arayüz (`04`) | kutuların kaydı | eşlendi |
| `02 §2.2` m5 | Sipariş oluşur: yeniden değerlendirme, numara, donma, ayırma; Alındı + Bekliyor; e-posta | Order · StockReservation · *gönderim kaydı* | — | eşlendi |
| `02 §2.2` m6 | Ödeme onaylanır; Ödendi + Hazırlanıyor; kesin düşme; sepet boşalır; süre başlar | Order · StockReservation · CartItem | ödeme onayı anı | eşlendi |
| `02 §2.2` m7 | Kargoya verilir; takip bilgisi; e-posta | Order · *gönderim kaydı* | kargo şirketi, takip numarası | eşlendi |
| `02 §2.2` m8 | Teslim işareti ve tarihi; cayma ve ayıbın başlangıcı | OrderItem | `DeliveryDate` | eşlendi |
| `02 §2.3` metin | Yedinci akışın adımları; üç kolu: marka, duyuru, iletişim talebi | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §2.3` m1 | Firma içerik tipini seçer | Content | tip | eşlendi |
| `02 §2.3` m2 | Kayıt taslak açılır; adla kaydedilebilir; Hakkımızda ve duyuru boş da | Content | `PublishStatus` = `Draft`; ad ya da başlık | eşlendi |
| `02 §2.3` m3 | Yönetici "Taslak" bandıyla önizler | veri doğurmaz — arayüz (`04`) | — | veri doğurmaz |
| `02 §2.3` m4 | Zorunlu alanlar dolunca yayına alınır | Content | `PublishStatus` | eşlendi |
| `02 §2.3` m5 | Ana sayfada gösterilir; genel sayfa menüde | Offering · PortfolioItem · CustomPage | ana sayfada göster işareti; menüde göster işareti | eşlendi |
| `02 §2.3` m6 | Yayındaki kaydın düzenlemesi anında yayında | veri doğurmaz — kod | — (sürüm geçmişi yok) | veri doğurmaz |
| `02 §2.3` m7 | Kayıt taslağa alınır ya da silinir | Content | `PublishStatus`; fiziksel silme | eşlendi |

#### 1.1.5 Ürün Gereksinimleri — `02 §3` iş kuralları (276)

Kural kimliğiyle (K-997); grupları `02 §3`'ün girişindeki grup tablosudur. Bölümün kural numarası taşımayan maddeleri ve metni K-1000'in birimiyle girer.

**`02 §3` — bölüm girişi (12)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §3` metin | Kuralların biçimi: numara, kalın başlık, koşul–sonuç–istisna | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §3` m1 | Ayarlanabilir parametrelerin değeri §11'de; yapısal sınırlar ve yasal süreler kuralın kendisi | veri doğurmaz — yazım konvansiyonu | — (değerler `02 §11`'in satırlarıyla, 1b) | veri doğurmaz |
| `02 §3` m2 | Başka bölümde yaşayan kurallar burada tekrarlanmaz | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §3` m3 | Kapsamın sınırları `01 §7`, `01 §6`, `10 §3`, §5'te | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §3` m4 | Biçimi `04`'e bırakan ifade arayüz kararıdır | veri doğurmaz — arayüz (`04`) | — | veri doğurmaz |
| `02 §3` m5 | Grup tablosu: §3.1–§3.2 firma, satış kapısı ve satış modeli | veri doğurmaz — yazım konvansiyonu | — (satırlar kendi kurallarıyla) | veri doğurmaz |
| `02 §3` m6 | Grup tablosu: §3.3–§3.12 katalog | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §3` m7 | Grup tablosu: §3.13–§3.15 hesap | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §3` m8 | Grup tablosu: §3.16–§3.26 sepet, sipariş, ödeme ve teslim | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §3` m9 | Grup tablosu: §3.27–§3.30 kurumsal içerik, marka ve site | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §3` m10 | Grup tablosu: §3.31 panel | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §3` m11 | Grup tablosu: §3.32–§3.34 iletişim talebi, yasal metinler, site geneli | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |

**§3.1–§3.2 — firma, satış kapısı ve satış modeli (11)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §3.1.1` | Kurulum başına tek firma; firmanın kimlik kaydı silinemez, yalnız düzenlenir | Company | tekil kayıt — kiracı alanı yok (SK-1); silinmez | eşlendi |
| `02 §3.1.2` | Kimlik panelden (tip, kimlik alanları, adres, iletişim, KEP, işletme adı ya da marka, davranış kuralları, ETBİS, logo); altyapı kurulumdan; değişiklik işlem izine | Company · AuditLog · veri doğurmaz — kurulum | Company: kimlik alanları; AuditLog: kimlik değişikliğinin satırı; alan adı, gönderim kimliği, sağlayıcı anahtarları ve Google kimlik bilgileri veritabanında değil | eşlendi |
| `02 §3.1.3` | Firma tipi (esnaf, gerçek kişi tacir, tüzel kişi) ve tipe göre zorunlu alanlar; KEP zorunlu ve boşaltılamaz; meslek odası, işletme adı ya da marka, davranış kuralları isteğe bağlı | Company | `CompanyType`; ad-soyad, vergi kimlik numarası, ticaret unvanı (`LegalName`), MERSİS numarası, ticaret sicil numarası, adres, telefon, e-posta, KEP adresi, meslek odası, işletme adı ya da tescilli marka, davranış kuralları (bağlantı ya da kısa metin) | eşlendi |
| `02 §3.1.4` | Yasal kimlik sürekli erişilebilir; ETBİS doğrulama bilgisi isteğe bağlı alan; değişikliği işlem izine | Company · AuditLog | Company: ETBİS doğrulama bağlantısı; AuditLog: alanın değişikliği | eşlendi |
| `02 §3.1.5` | Satış kapısı dört koşul (kimlik, ödeme yöntemi, geçici kapatma anahtarı, dört yasal metin ve iade adresi); veri toplayan girişlerin ayrı kapısı | *firma ayarları* · veri doğurmaz — türetilir | Kapı saklanmaz, koşullardan okunur: Company'nin kimlik alanları, ödeme yöntemlerinin açıklığı, geçici kapatma anahtarı, yasal metinlerin yayın hâli, iade adresi; aydınlatma metninin "tamamlandı" hâli `02 §3.33`'ün alanlarından | eşlendi |
| `02 §3.1.6` | Satışın geçici kapatılması; açık siparişler etkilenmez; bakım modu yoktur | *firma ayarları* · yoktur — kapsam dışı | satışı geçici kapatma anahtarı; bakım modu ve planlı bakım penceresi yok | eşlendi |
| `02 §3.1.7` | İade adresi zorunlu firma ayarı; beyanla birlikte beyan kaydına yazılır; panel iade malı beklenen kalem sayısını söyler | *firma ayarları* · Withdrawal | *firma ayarları*: iade adresi (kimlik adresinden ayrı alan); Withdrawal: beyan anındaki iade adresi; bekleyen kalem sayısı türetilir | eşlendi |
| `02 §3.2.1` | Satış yalnız doğrudan; teklif, talep, randevu ve rezervasyon yoktur | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §3.2.2` | Üç ürün tipi; hizmette zorunlu ifa süresi — kaleme donar | Product · OrderItem | Product: `ProductType`, ifa süresi (gün); OrderItem: donmuş ifa süresi | eşlendi |
| `02 §3.2.3` | Tip ürün düzleminde, üç değerli kapalı alan; varyant devralır | Product | `ProductType` | eşlendi |
| `02 §3.2` metin | Tipe göre ayrılan kurallar kendi gruplarında | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |

**§3.3–§3.12 — katalog (66)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §3.3.1` | Satılan birim varyant; seçeneksiz ürünün de tek varyantı; vitrinde ürün başına tek kart | Product · Variant | Variant: ürüne bağ; stok ve fiyat varyantta | eşlendi |
| `02 §3.3.2` | En fazla iki seçenek boyutu | Option | ürüne bağ; ürün başına en çok iki — kısıt | eşlendi |
| `02 §3.3.3` | Birleşim matrisi tam olmak zorunda değil; yalnız var olan kombinasyon varyant | Variant · OptionValue | varyantın seçenek değerleri (en çok iki) | eşlendi |
| `02 §3.3.4` | Her varyant zorunlu ve tekil stok kodu taşır | Variant | `SKU` — zorunlu, tekil | eşlendi |
| `02 §3.3.5` | Asgari adet yoktur; paket varyant olarak tanımlanır | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §3.3.6` | Yorum, puan ve öneri bloğu yoktur; "İlgili ürünler" elle kurulan bağdır | yoktur — kapsam dışı | — (elle kurulan bağ `02 §3.27.10`'da) | veri doğurmaz |
| `02 §3.4.1` | Ağaç en fazla üç seviye; kategori üründe; alfabetik sıra, elle sıralama yok | Category | ad, üst kategori bağı (seviye en çok üç) | eşlendi |
| `02 §3.4.2` | Ürün ağacın herhangi bir düğümüne asılabilir | *ürün–kategori bağı* | — | eşlendi |
| `02 §3.4.3` | Kategori sayfası alt dallardaki ürünleri de gösterir | veri doğurmaz — türetilir | — (ağaçtan okunur) | veri doğurmaz |
| `02 §3.4.4` | Ürün birden çok kategoriye asılabilir; biri ana kategori, yayın için zorunlu | *ürün–kategori bağı* · Product | `PrimaryCategory` | eşlendi |
| `02 §3.4.5` | İçinde ürün ya da alt kategori olan kategori silinemez | Category · *ürün–kategori bağı* | silme engeli — kısıt | eşlendi |
| `02 §3.4.6` | Taşıma serbest; üç seviyeyi aşamaz, kendi alt ağacına taşınamaz | Category | üst kategori bağı — kısıt | eşlendi |
| `02 §3.4` metin | Boş kategorinin menüdeki görünürlüğü §3.28.6'da | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §3.5.1` | Arama yalnız ürün adında; büyük-küçük harf ve Türkçe karakter duyarsız | Product | ürün adının arama biçimi (K-886) | eşlendi |
| `02 §3.5.2` | Süzgeç fiyat aralığı ve stok durumu; seçenek değerine göre süzme yok | veri doğurmaz — türetilir | — (varyant fiyatı ve satın alınabilirlikten) | veri doğurmaz |
| `02 §3.5.3` | Sıralama seçeneği yok; en yeni önce; ürünlerde elle sıralama yok | Product | oluşturulma anı (sıralama ölçüsü) | eşlendi |
| `02 §3.6.1` | Stok varyantın sayısal adedi; ürünün stok durumu türetilir | Variant | `StockQuantity` | eşlendi |
| `02 §3.6.2` | Stok tipe göre: fizikselde zorunlu, dijitalde yok, hizmette isteğe bağlı kontenjan | Variant | `StockQuantity` — fizikselde zorunlu, dijitalde yok; hizmet kontenjanı (boş = sınırsız) | eşlendi |
| `02 §3.6.3` | Tükenmiş varyant "Tükendi"; satın alınabilirlik ayrılmış adetler düşülerek | Variant · StockReservation | ayrılmış adet (K-890) | eşlendi |
| `02 §3.6.4` | Bütün varyantları tükenmiş ürün vitrinde kalır, sepete eklenemez | veri doğurmaz — türetilir | — | veri doğurmaz |
| `02 §3.6.5` | Stok adedi ziyaretçiye gösterilmez | veri doğurmaz — arayüz (`04`) | — | veri doğurmaz |
| `02 §3.6.6` | İptal edilen adet kendiliğinden döner; iade edilen adedi firma panelden ekler | Variant · Cancellation · Return | dönüşün okunduğu kayıt (K-791) | eşlendi |
| `02 §3.6.7` | Stok ve kontenjan ayrılmış adedin altına indirilemez; panel ayrılmış adedi ve tutan siparişleri gösterir | Variant · StockReservation | ayrılmış adet, ayırmanın siparişi; koşullu güncelleme (K-980) | eşlendi |
| `02 §3.7.1` | Ürün ve varyant yayın durumunu ayrı taşır; zamanlanmış yayın yok; değişiklik işlem izine | Product · Variant · AuditLog | `PublishStatus` (iki düzlemde); AuditLog: yayın durumu değişikliği | eşlendi |
| `02 §3.7.2` | Arşiv silme değildir | Product · Variant | `PublishStatus` = `Archived` | eşlendi |
| `02 §3.7.3` | Yayın kapısı: ad, ana kategori, tip, en az bir Yayında varyant; yayındaki varyantın fiyatı sıfırdan büyük | Product · Variant | ad, `PrimaryCategory`, `ProductType`; varyant fiyatı sıfırdan büyük — kısıt | eşlendi |
| `02 §3.7.4` | Dijital ürünün yayındaki her varyantı bir dosyaya bağlı; hizmetin kapısı ifa süresini ister | Variant · Product · *dosya kaydı* | varyantın dosyası (kendi ya da ürününki); ifa süresi | eşlendi |
| `02 §3.7.5` | Taslak ürün ziyaretçiye "sayfa bulunamadı"; yönetici "Taslak" bandıyla görür | veri doğurmaz — arayüz (`04`) | — (`PublishStatus`'tan) | veri doğurmaz |
| `02 §3.7.6` | Arşivlenmiş ürün sayfası çalışır, fiyatsız; listelerde görünmez | veri doğurmaz — arayüz (`04`) | — (`PublishStatus`'tan) | veri doğurmaz |
| `02 §3.7.7` | Kalıcı silme; geçmiş siparişler etkilenmez; açık siparişli ürün ya da varyant silinebilir; ayırma varyantla düşer | Product · Variant · OrderItem · StockReservation | silme fizikseldir; kalem donmuş bilgisini taşır, ürüne bağı silmeyle kalkar; silinen varyantın ayırması düşer (K-655) | eşlendi |
| `02 §3.7.8` | Yayındaki ürünün son Yayında varyantı arşivlenemez, taslağa çekilemez, silinemez | Product · Variant | yayın kapısı — kısıt | eşlendi |
| `02 §3.8.1` | Fiyat varyantta, KDV dahil girilir; matrah ve KDV geriye hesaplanır | Variant | fiyat (KDV dahil) | eşlendi |
| `02 §3.8.2` | KDV oranı ürün düzleminde, varsayılanı ayardan; matrah ve KDV sipariş kaydında ayrışır; kargonun oranı kalemleri izler | Product · *firma ayarları* · OrderItem · Order | `VatRate`; varsayılan KDV oranı; kalemin matrahı ve KDV tutarı; kargo ücretinin KDV'si (K-500) | eşlendi |
| `02 §3.8.3` | KDV kalem bazında, kuruşa yuvarlanır; hesap sırası sabit; kupon payı, KDV ve matrah kaleme donar; yuvarlamanın yönü | OrderItem | donmuş tutar, kupon payı, KDV, matrah; hesap ve yuvarlama kodda | eşlendi |
| `02 §3.8.4` | Fiyat hassasiyeti kuruş (iki ondalık); fiyat sıfırdan büyük | Variant | fiyat — ölçek iki ondalık, sıfırdan büyük (VM2-04) | eşlendi |
| `02 §3.8.5` | Fiyat etiketi: üretim yeri, ölçü birimi ve net miktar, birim fiyat türetilir, fiyatın başlangıç tarihi fiyat geçmişinden, yerli üretim logosu kendiliğinden | Product · *fiyat geçmişi* · veri doğurmaz — türetilir | üretim yeri (ülke); ölçü birimi + net miktar (liste VM5-04); fiyatın başlangıç tarihi geçmişten; birim fiyat ve yerli üretim logosunun gösterimi türetilir — logo ürünle gelen varlık | eşlendi |
| `02 §3.9.1` | İndirim ürün düzleminde yüzde, bütün varyantlara iner; hiçbir varyantı sıfıra indiremez | Discount | yüzde (ölçek VM2-04), ürüne bağ; sıfır fiyat engeli — kısıt | eşlendi |
| `02 §3.9.2` | İndirim tarihli, sistem başlatır ve bitirir; tarihler gösterilir | Discount | başlangıç anı, bitiş anı | eşlendi |
| `02 §3.9.3` | Referans fiyatı sistem hesaplar; her varyantın fiyat geçmişi tutulur; indirimli fiyat da geçmişe yazılır | *fiyat geçmişi* | varyant, fiyat, uygulanmaya başladığı an (indirimli fiyat dahil) | eşlendi |
| `02 §3.9.4` | 10 günlük geçmişi olmayan malda referans satışa sunuluştan beri en düşük fiyat | *fiyat geçmişi* · Variant | varyantın satışa sunulduğu ilk an (kendisinin ve ürününün birlikte Yayında olduğu an) | eşlendi |
| `02 §3.9.5` | İndirimli fiyat referansın altına inmiyorsa indirim kaydedilmez | veri doğurmaz — türetilir | — (doğrulama kuralı, kodda) | veri doğurmaz |
| `02 §3.9.6` | Beyan edilen referans fiyat kaleme donar | OrderItem | donmuş `ReferencePrice` | eşlendi |
| `02 §3.10.1` | Kupon sepet toplamına iner; siparişe en fazla bir kupon kodu | Coupon · Order | Order: kupon bağı (en çok bir) | eşlendi |
| `02 §3.10.2` | Kupon yüzde ya da sabit tutar | Coupon | tür (yüzde ya da sabit tutar), değer | eşlendi |
| `02 §3.10.3` | Kupon payı kalem kalem; sabit tutar orantılı dağıtılır; pay kaleme donar | OrderItem | donmuş kupon payı | eşlendi |
| `02 §3.10.4` | Kupon siparişi sıfıra indiremez; sabit tutarlı kuponun zorunlu asgari sepet tutarı | Coupon | asgari sepet tutarı (sabit tutarlıda zorunlu, kupon tutarından büyük) | eşlendi |
| `02 §3.10.5` | Sınır tarih aralığı ve toplam kullanım adedi; kod tekil, harf duyarsız; kupon silinmez | Coupon | kod (tekil, harf duyarsız — VM2-07), başlangıç ve bitiş, toplam kullanım adedi; silinmez | eşlendi |
| `02 §3.10.6` | Kullanım hakkı stok gibi ayrılır; toplam adet kullanılmış + ayrılmış hakların altına inemez | Coupon · StockReservation | kullanılmış ve ayrılmış haklar (K-654) | eşlendi |
| `02 §3.10.7` | Kupon kargo eşiğinin ve asgari tutarın tabanını düşürmez | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §3.11.1` | Görsel ürün düzleminde; varyant kendi görselini taşıyabilir | *görsel* | ürüne bağ, varyanta bağ | eşlendi |
| `02 §3.11.2` | Ana görsel işareti galeri sırasından bağımsız; galeri sırası elle | *görsel* | ana görsel işareti, galeri sırası | eşlendi |
| `02 §3.11.3` | Görselin format, boyut ve adet tavanı | veri doğurmaz — kod | — (değerler `02 §11`'de) | veri doğurmaz |
| `02 §3.11.4` | Alternatif metin isteğe bağlı, boşsa sistem üretir | *görsel* | alternatif metin (isteğe bağlı); üretilen metin türetilir | eşlendi |
| `02 §3.11.5` | Ürün adına karakter tavanı; açıklama serbest | Product | ad (uzunluk tavanı), açıklama | eşlendi |
| `02 §3.11.6` | Uzun metinler kapalı biçim setini kullanır; ham HTML yok | Product · AboutPage · Offering · PortfolioItem · FaqItem · CustomPage · PrivacyNotice · CookiePolicy | biçimli metin alanları (set kodda) | eşlendi |
| `02 §3.11.7` | Garanti bilgisinin ayrı alanı yok; garanti belgesi saklanmaz | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §3.12.1` | Dijital ürünü sistem teslim eder; ödemede indirme açılır | OrderItem | teslim işareti (dijitalde ödeme onayı) | eşlendi |
| `02 §3.12.2` | Dosya ürün düzleminde, varyant kendi dosyasını taşıyabilir; her düzlemde tek dosya | *dosya kaydı* · Product · Variant | ürünün ve varyantın o anki dosyası (K-883) | eşlendi |
| `02 §3.12.3` | Tek dosyanın boyut tavanı; tür ve güvenlik `05`'in işi | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §3.12.4` | Dijital üründe adet her zaman 1 | CartItem · OrderItem | adet = 1 — kısıt | eşlendi |
| `02 §3.12.5` | İndirme hakkı adetle sınırlı, kalem başına; kaleme donar | OrderItem | indirme hakkı (donmuş), indirme sayacı | eşlendi |
| `02 §3.12.6` | Hakkı dolana firma kalemin hakkını yeniler, sayaç sıfırlanır | OrderItem | indirme sayacı (sıfırlanır) | eşlendi |
| `02 §3.12.7` | Sipariş sayfası varyantın o anki dosyasını verir; silinse de son hâl indirilir | OrderItem · *dosya kaydı* | kalemin varyant bağı ya da son dosya bağı (K-883, K-948) | eşlendi |
| `02 §3.12.8` | Dosya güncellemesi hakkı değiştirmez | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §3.12.9` | Daha önce alınmış dijital varyant yeniden alınabilir; üyeye uyarı | veri doğurmaz — türetilir | — (üyenin siparişlerinden okunur) | veri doğurmaz |
| `02 §3.12` metin | Dijital kalemin teslim hattı §5.7'de, iptali ve cayma istisnası §7'de | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |

**§3.13–§3.15 — hesap (33)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §3.13.1` | Sipariş için üyelik zorunlu değil; misafir numara + e-postayla takip eder | Order | hesap bağı isteğe bağlı; iletişim e-postası, `OrderNumber` | eşlendi |
| `02 §3.13.2` | Hesap açılınca o e-postanın misafir siparişleri hesaba düşer; dayanak doğrulanmış e-posta; silinmiş hesabın siparişi bağlanmaz | Order · Customer | Order: hesap bağı; Customer: doğrulanmış e-posta | eşlendi |
| `02 §3.13.3` | Hesabı olan kişinin misafir siparişi hesaba anında düşer; e-posta düzeltmesinde bağ kesilir ya da yeni hesaba düşer | Order · Customer | Order: hesap bağı (düzeltmede yeniden kurulur); hatırlatma `04`'te | eşlendi |
| `02 §3.13.4` | E-posta doğrulaması sert kapı; doğrulanmamış hesap bekleme aşaması | Customer | doğrulanmış mı — doğrulanmamış kaydın biçimi (K-811, VM4-09) | eşlendi |
| `02 §3.13.5` | Doğrulanmamış hesap geçici; bağlantının ömrü; doğrulama ekrandaki düğmeyle; yeniden istenen bağlantı öncekini geçersiz kılar, ömrü uzatmaz | Customer · *bağlantı kaydı* | bekleyen kaydın oluşma anı; bağlantının türü, oluşma anı, ömrün sonu, yerine yenisi geçtiği an (K-903) | eşlendi |
| `02 §3.13.6` | Sosyal giriş yalnız Google; kimlik bilgileri kurulum ayarı; panelde anahtar yok | SocialLogin · veri doğurmaz — kurulum | Google bağı; uygulama kimlik bilgileri veritabanında değil | eşlendi |
| `02 §3.13.7` | Aynı e-posta iki yoldan tek hesaba; doğrulanmamış bekleyen kayıt devralınmaz, silinir; üye Google bağını kaldıramaz | Customer · SocialLogin | Google bağı (sağlayıcının kullanıcı kimliği); bekleyen kaydın silinmesi | eşlendi |
| `02 §3.13.8` | Google ile açılan hesap şifresiz başlar, sonradan şifre konabilir | Customer | şifre özeti (boş olabilir) | eşlendi |
| `02 §3.13.9` | Şifre sıfırlama bağlantısı süreli ve tek kullanımlık | *bağlantı kaydı* | tür = sıfırlama; ömrün sonu; kullanıldığı an | eşlendi |
| `02 §3.13.10` | Sıfırlamada bütün oturumlar, hesaptan değişiklikte öteki oturumlar ve öteki tarayıcıların işaretleri düşer | Session · RecognizedBrowserMark | hesap başına silme; değişikliği yapan oturumun ayrımı | eşlendi |
| `02 §3.13.11` | Sıfırlama ekranı nötr, kayıt ekranı dürüst; e-posta değişikliği dürüst | veri doğurmaz — arayüz (`04`) | — | veri doğurmaz |
| `02 §3.13.12` | Oturum ömrünü kullanıcı seçer: beni hatırla — uzun, değilse kısa | Session | kısa ya da uzun; son işlem anı | eşlendi |
| `02 §3.13.13` | Üç işlem yeniden doğrulama ister; Google hesabında Google ile; başarısız deneme L-1'e sayılır | Session · *deneme kaydı* | yeniden doğrulamanın tek işlemlik izni (K-899); deneme satırı | eşlendi |
| `02 §3.13.14` | E-posta yeni adres doğrulanmadan değişmez; eski adrese bildirim ve geri alma bağlantısı; geri alma Google bağını kaldırır, şifreyi geçersizleştirir; bekleyen değişiklik görünür, yenisi yerine geçer | Customer · *bekleyen e-posta değişikliği* · *bağlantı kaydı* · SocialLogin · *gönderim kaydı* | bekleyen yeni adres ve bağlantısı; geri alma bağlantısı (ömür Z-45); Google bağının değişiklik süresince kurulduğu an; eski adrese giden bildirimin satırı | eşlendi |
| `02 §3.13.15` | Şifre politikası uzunluk tabanlı; yaygın şifre listesi ürünle gelir | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §3.13.16` | Kimlik doğrulama kuralları yöneticiye de aynen; yöneticinin e-postası firmanın iletişim e-postasından ayrı; geri alma işlem izine | Admin · Company · AuditLog | Admin: e-posta, şifre, doğrulama; Company: iletişim e-postası (ayrı alan); AuditLog: değişiklik ve geri alması | eşlendi |
| `02 §3.13.17` | Giriş, sıfırlama, kayıt ve yeniden istek limitli (L-9); hesap kilitlenmez; tanınan tarayıcı e-posta ekseninde engellenmez | *deneme kaydı* · RecognizedBrowserMark | limit, eksen, değer, an (K-901) | eşlendi |
| `02 §3.13.18` | Kayıtta ve hesapta pazarlama onayı yok; alan tutulmaz | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §3.13.19` | Kayıt ve Google ile ilk giriş veri toplayan girişlerin kapısına bağlı | veri doğurmaz — türetilir | — (yasal metinlerin yayın hâlinden) | veri doğurmaz |
| `02 §3.13.20` | Her giriş denemesi giriş kaydına: zaman, IP, hesap (e-posta), sonuç; altı ay | *giriş kaydı* | an, IP, hesap türü, e-posta, sonuç; süresi Z-40 | eşlendi |
| `02 §3.13.21` | Hesap zorunlu bir ad taşır; Google'da Google'ın adı; değiştirilebilir; siparişin alıcı adı değildir; silmede hemen silinir | Customer | ad (zorunlu) | eşlendi |
| `02 §3.14.1` | Üyenin adres defteri; adlandırılmış kayıtlar; ödeme adımında yazılan adres isteğe bağlı deftere kaydedilir | AddressBook | müşteri bağı, kaydın adı (Ev, İş), adres alanları | eşlendi |
| `02 §3.14.2` | Fatura adresi teslimattan ayrı seçilebilir; varsayılan aynı | Order | `ShippingAddress`, `BillingAddress` | eşlendi |
| `02 §3.14.3` | İl 81 ilden oluşan kapalı listeden | *il listesi* | il (kapalı liste; başlangıç verisi — VM8-01) | eşlendi |
| `02 §3.14.4` | Siparişin istediği adresler kalem tipine bağlı; yalnız dijital ve hizmette teslimat adresi yok | Order | `ShippingAddress` isteğe bağlı (fiziksel kalem varsa zorunlu), `BillingAddress` zorunlu | eşlendi |
| `02 §3.14.5` | Sipariş adreslerini kendi içine dondurur | Order | donmuş `ShippingAddress` ve `BillingAddress` | eşlendi |
| `02 §3.14.6` | Adresin alanları: ad-soyad, il, ilçe (serbest), açık adres; teslimatta telefon zorunlu; posta kodu ve T.C. kimlik numarası yok; defterde telefon isteğe bağlı | Order · AddressBook · *il listesi* | ad ve soyad, il, ilçe, açık adres, telefon (teslimatta zorunlu, defterde isteğe bağlı); posta kodu ve kimlik numarası alanı yok | eşlendi |
| `02 §3.15.1` | Hesap silinince ad, giriş bilgileri, şifre, oturumlar, adres defteri ve sepet hemen silinir; siparişteki kişisel veri süresinde imha; "hesabınız silindi" e-postası | Customer · Session · AddressBook · Cart · Order · *imha kaydı* · *gönderim kaydı* | anında fiziksel silme; Order: hesap bağı kalkar; silme imha kaydına (K-708); bildirim satırı | eşlendi |
| `02 §3.15.2` | Yürüyen sipariş silmeyi engellemez; sipariş kendi iletişim e-postasını dondurur | Order | donmuş iletişim e-postası | eşlendi |
| `02 §3.15.3` | Silinmiş hesabın siparişleri hiçbir hesaba bağlanmaz | Order | silinmiş hesaba ait olduğunun işareti — `02 §3.13.2`'nin bağlamasından dışlar (biçimi VM4-06) | eşlendi |
| `02 §3.15.4` | Başvuru kanalı iletişim formu, "KVKK talebi" konu tipi; ayrı modül ve sayaç yok | ContactRequest | `ContactTopic` = KVKK talebi; süre sayacı yok | eşlendi |
| `02 §3.15.5` | Uygulama içi veri indirme yok; üye kaydı görünümü e-postayla aranır, salt okunur; firma talep üzerine siler, işlem izine | Customer · AuditLog | e-postayla arama (harf duyarsız — VM2-07); silmenin iz satırı | eşlendi |
| `02 §3.15.6` | Geri alma bağlantısı açıkken hesap silinmez | *bağlantı kaydı* | geri alma bağlantısının ömrün sonu ve kullanıldığı an | eşlendi |

**§3.16–§3.26 — sepet, sipariş, ödeme ve teslim (76)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §3.16.1` | Üyenin sepeti hesapta, her cihazda aynı; misafirinki tarayıcıya bağlı | Cart | sahibi: müşteri hesabı ya da misafir sepetinin tarayıcı anahtarı | eşlendi |
| `02 §3.16.2` | Sepet kendiliğinden boşalmaz; süre parametresi yok | Cart | misafir sepetinin son erişim anı (K-906 — `05 §6`'nın temizliği) | eşlendi |
| `02 §3.16.3` | Girişte iki sepet birleşir; aynı varyantta büyük adet kalır | Cart · CartItem | adet; birleşmede kalemlerin taşınması | eşlendi |
| `02 §3.16.4` | Birleştirmenin eklediği ürün bir kez söylenir | veri doğurmaz — arayüz (`04`) | — (birleşme anında hesaplanır) | veri doğurmaz |
| `02 §3.16.5` | Sepet kalemi canlı; ilk eklendiği andaki birim fiyatı hatırlar | CartItem | varyant bağı, adet, ilk eklendiği andaki birim fiyat | eşlendi |
| `02 §3.16.6` | Satın alınamayan kalem "Tükendi" işaretiyle kalır, siparişe girmez | veri doğurmaz — türetilir | — (stoktan ve ayrılmış adetten) | veri doğurmaz |
| `02 §3.16.7` | Vitrinden kalkan kalem sepetten çıkar ve bir kez söylenir; siparişe girecek kalem kalmazsa sipariş verilemez | CartItem | kalemin silinmesi (varyant silinince ya da yayından çekilince) | eşlendi |
| `02 §3.16.8` | Stoktan fazla adet eklenmez; adet söylenmez | veri doğurmaz — türetilir | — (stok ve ayrılmış adetten) | veri doğurmaz |
| `02 §3.16.9` | Ürüne "bir siparişte en fazla" sınırı; boşsa yok, doluysa en az 1 | Product | bir siparişte en fazla adet (isteğe bağlı, en az 1) | eşlendi |
| `02 §3.16.10` | Sonradan aşılan sınırda kalemler sepette mesajıyla kalır | veri doğurmaz — türetilir | — | veri doğurmaz |
| `02 §3.16.11` | Sepette kalem tavanı yok; elli kalem performans kabulü | veri doğurmaz — mimari (`05`) | — | veri doğurmaz |
| `02 §3.16.12` | Sepet ödeme başarılı olunca boşalır, yalnız siparişe giren kalemler çıkar | CartItem · Order | sepetin kalemi ile siparişin kalemi arasındaki karşılık (varyant) | eşlendi |
| `02 §3.16.13` | İstek listesi ve sepet hatırlatması yok | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §3.16` metin | Dijital adet ve uyarı §3.12'de; misafir sepetinin çerezi zorunlu çerez | veri doğurmaz — mimari (`05`) | — (çerez `05 §6.7`'de) | veri doğurmaz |
| `02 §3.17.1` | Sipariş onay anında oluşur; numara, donma, ayırma; site numarayı gösterir; B-1 | Order | oluşma anı, `OrderNumber` | eşlendi |
| `02 §3.17.2` | Onaylanan özet ile sipariş birebir aynı; fark durdurur; ÖBF ya da MSS sürümünü artıran fark da durdurur | Order · *yasal metin sürümü* | onaylanan sürümler (karşılaştırma onay anında — K-591) | eşlendi |
| `02 §3.17.3` | Onayda adet ayrılır, ödeme süresi tanınır; başarıda kesin düşer, süre dolunca döner; kontenjan ve kupon hakkı aynı | StockReservation · Variant · Coupon | ayrılmış adet ve hâli (ayrıldı, kesin düştü, döndü) | eşlendi |
| `02 §3.17.4` | Ödeme süresi yönteme göre; kartta ürün sabiti, havalede firma ayarı ve çitli | Order · *firma ayarları* | ödeme süresinin bittiği an (K-895); havale ödeme süresi (iş günü) | eşlendi |
| `02 §3.17.5` | Süre dolan ya da kart ödemesi başarısız sipariş kendiliğinden iptal; son sorgu; ödeme Başarısız | Order · *geçiş kaydı* | `OrderStatus`, `PaymentStatus`; geçişin kimi = sistem | eşlendi |
| `02 §3.17.6` | Başarısız ödemenin tekrarı yeni siparişle | Order | — (yeni kayıt) | eşlendi |
| `02 §3.17.7` | Aynı sepetten yeni sipariş, o sepete bağlı ödenmemiş önceki siparişi iptal eder | Order | sepet bağı (siparişin verildiği sepet) | eşlendi |
| `02 §3.17.8` | Kendiliğinden iptal edilmiş ödenmemiş sipariş müşteriye görünmez; ödeme denemesi limiti iki eksende; açık ödenmemiş havale tavanı; ödemesi alınmamış siparişin süreleri | Order · *geçiş kaydı* · Cancellation | iptalin türü (kendiliğinden — süre, kart başarısızlığı, aynı sepet, ödeme sayfası açılmadı); IP; üyenin hesap bağı; ödemesi alınmış sayılması (K-951) | eşlendi |
| `02 §3.18.1` | İsteğe bağlı tek asgari sipariş tutarı; sepette ve onayda denetlenir | *firma ayarları* | asgari sipariş tutarı (boş = kapalı) | eşlendi |
| `02 §3.18.2` | Karşılaştırma tabanı kupondan önceki indirimli toplam | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §3.18.3` | Asgari tutar yalnız fiziksel kalem içeren siparişe | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §3.19.1` | Kargo ücreti sipariş başına sabit tek tutar; fiziksel kalem varsa | *firma ayarları* · Order | kargo ücreti (ayar); siparişin kargo ücreti | eşlendi |
| `02 §3.19.2` | Kargo ücreti KDV dahil; KDV fiziksel kalemlerin oranını izler, orantılı bölünür, bölüşüm donar | Order | kargo ücretinin oranlara bölüşümü ve parçaların KDV'si (donmuş) | eşlendi |
| `02 §3.19.3` | İsteğe bağlı ücretsiz kargo eşiği | *firma ayarları* | ücretsiz kargo eşiği (boş = kapalı) | eşlendi |
| `02 §3.19.4` | Eşiğin tabanı kupondan önceki tutar | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §3.19.5` | Eşiğe kalan tutar ve "Kargo ücretsiz" yazar; kampanya değildir | veri doğurmaz — arayüz (`04`) | — | veri doğurmaz |
| `02 §3.19.6` | Kargo ücreti ve ücretsiz kargo kararı donar; iki hâlde geri ödenir | Order · Refund | donmuş kargo ücreti ve ücretsiz kargo kararı; kargo ücretinin geri ödenmesi | eşlendi |
| `02 §3.20.1` | Firma teslimat illerini seçer, varsayılan tüm Türkiye; kısıt il düzeyinde | *firma ayarları* · *il listesi* | teslimat yapılan iller (il listesinin alt kümesi; varsayılan hepsi) | eşlendi |
| `02 §3.20.2` | Tek teslimat yolu kargo; mağazadan teslim ve adlandırılmış seçenekler yok | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §3.20.3` | Sipariş tek parça gider, tek takip numarası | Order | tek takip numarası — parçalı gönderim alanı yok | eşlendi |
| `02 §3.20.4` | Teslimat sözü kargoya verme süresi; yasal 30 gün sözleşmede; panelde üst çit | *firma ayarları* · Product | kargoya verme süresi (iş günü; çit kodda) | eşlendi |
| `02 §3.20.5` | Firma varsayılanı, ürün kendi süresini taşıyabilir; siparişin sözü en uzunu, onayda donar | *firma ayarları* · Product · Order | varsayılan süre; ürünün süresi (isteğe bağlı); siparişin donmuş kargoya verme süresi | eşlendi |
| `02 §3.20.6` | Kargoya verme süresi ödeme onayında başlar | Order | ödeme onayı anı | eşlendi |
| `02 §3.20.7` | Kargoya verildi işaretinde kargo şirketi + takip numarası ya da "kendi aracımızla" | Order | kargo şirketi, takip numarası, kendi aracıyla teslim işareti, kargoya verildiği an | eşlendi |
| `02 §3.20.8` | Kargo şirketi ürünle gelen listeden, "Diğer" adıyla; liste ve takip kalıbı ürünün varlığı (`08`) | Order · veri doğurmaz — kod | Order: seçilen şirket ya da "Diğer"in adı; liste ve kalıplar kodda | eşlendi |
| `02 §3.20.9` | Kalemin teslimi tipine göre; hizmet "tamamlandı" işareti ödeme onaylıyken | OrderItem | teslim işareti (`DeliveryDate`) — fizikselde firma, dijitalde ödeme onayı, hizmette tamamlama | eşlendi |
| `02 §3.20.10` | Karışık siparişte tek eksen; kalem düzeyinde teslim işareti, ayrı durum makinesi yok | OrderItem | teslim işareti | eşlendi |
| `02 §3.20.11` | Fiziksel teslimi firma işaretler, tarih geçmişe dönük olabilir, ileri ve kargo gününden önce olamaz; sipariş düzeyinde bir kez, kalemlere yazılır; düzeltilebilir | OrderItem · AuditLog | `DeliveryDate` (fiziksel kalemlerde aynı tarih); tarihin düzeltilmesi (`02 §10.4`) | eşlendi |
| `02 §3.20.12` | Fiziksel kalemi olmayan sipariş kargo adımlarından geçmez | veri doğurmaz — kod | — (hat `02 §5.7`) | veri doğurmaz |
| `02 §3.21.1` | Satıcı firma; ürün kart verisini görmez, saklamaz; sağlayıcıya aktarım | yoktur — kapsam dışı · veri doğurmaz — ürünün dışında | kart verisi alanı yok | veri doğurmaz |
| `02 §3.21.2` | İki ödeme yöntemi: kart ve havale; kapıda ödeme yok | Order | `PaymentMethod` | eşlendi |
| `02 §3.21.3` | Her kart ödemesi 3D Secure'dan geçer | veri doğurmaz — ürünün dışında | — | veri doğurmaz |
| `02 §3.21.4` | Kart yönteminin anahtarı yok; sağlayıcı anahtarları kurulumdan | veri doğurmaz — kurulum | — | veri doğurmaz |
| `02 §3.21.5` | Havale panelden açılıp kapanır, IBAN zorunlu; L-8 doluyken seçilemez; IBAN değişikliği bütün yöneticilere bildirilir; IBAN siparişe donar | *firma ayarları* · Order · *gönderim kaydı* | havale açık mı, havale IBAN'ı; siparişin donmuş IBAN'ı (K-663); F-5'in satırları | eşlendi |
| `02 §3.21.6` | Havalede müşteri donmuş IBAN'ı görür; firma "ödendi" işaretler; son iş günü bir kez hatırlatma | Order · *gönderim kaydı* | ödeme onayı anı ve onaylayan; hatırlatmanın anı (K-685, K-895) | eşlendi |
| `02 §3.21.7` | Havalede gelen tutar girilmez; eksik ödeme durumu yok | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §3.21.8` | Süresi dolup iptal edilmiş sipariş sonradan "ödendi" işaretlenemez; kesintide iptal ertelenir | Order | ertelenen son an (K-898) | eşlendi |
| `02 §3.21.9` | Ürün taksiti bilmez; sipariş tek tutar | Order | tek toplam tutar — taksit alanı yok | eşlendi |
| `02 §3.21.10` | Sipariş hazırlığa ödeme onaylanınca geçer | veri doğurmaz — kod | — (geçiş koşulu `02 §5.6`) | veri doğurmaz |
| `02 §3.21.11` | Ters ibraz ürünün dışında; eksen değişmez; iç nota yazılabilir; çift ödemeye karşı firmanın elle adımı | veri doğurmaz — ürünün dışında · Order | Order: iç not (`02 §10.4.7`); ters ibraz alanı yok | eşlendi |
| `02 §3.22.1` | Sipariş numarası yıl + rastgele blok, karışan karakter yok | Order | `OrderNumber` (biçim kodda) | eşlendi |
| `02 §3.22.2` | Numara oluşta verilir, tekrar kullanılmaz, tekildir; fatura numarası değildir | Order | `OrderNumber` — tekil | eşlendi |
| `02 §3.22.3` | Sipariş sayfasına üç yol; numara + e-posta sorgusu iki eksende limitli | Order · *deneme kaydı* | `OrderNumber`, iletişim e-postası, `OrderAccessKey`; L-5 denemeleri | eşlendi |
| `02 §3.22.4` | Sipariş sayfası her bilgiyi taşır; e-postaya bağlı akış yok | veri doğurmaz — arayüz (`04`) | — | veri doğurmaz |
| `02 §3.22.5` | Erişim anahtarı numaradan ayrı, tahmin edilemez; ömrü yok; e-posta düzeltmesinde yenilenir | Order | `OrderAccessKey` — tekil (K-562, K-903) | eşlendi |
| `02 §3.23.1` | Kalem ödenen tutarı üreten her değeri ve ürünün o anki tanımını dondurur: fiyat, KDV oranı ve tutarı, indirim oranı, referans fiyat, kupon payı, ad, seçenek değerleri, görsel; tip, cayma istisnası işareti, sebebi ve koşulu, ifa süresi, indirme hakkı | OrderItem · *görsel* | donmuş birim fiyat, `VatRate`, KDV tutarı, indirim oranı, `ReferencePrice`, kupon payı, ürün adı, seçenek değerleri, görsel bağı (K-883), `ProductType`, cayma istisnası işareti, `WithdrawalExemptionReason` ve koşulu, ifa süresi, indirme hakkı | eşlendi |
| `02 §3.23.2` | Donan değer kalemde düz yaşar; sipariş düzleminde kupon kodu, kargo ücreti, toplamlar; ara gruplama yok | OrderItem · Order | Order: kupon kodu, kargo ücreti, toplamlar | eşlendi |
| `02 §3.23.3` | Sipariş adresleri, iletişim e-postası, firmanın yasal kimliği, ÖBF ve MSS sürümleri, onay kutularının kaydı, havale IBAN'ı ve aydınlatma metni sürümünü dondurur; misafirin e-postası düzeltilebilir; donmuş sürümler silinmez | Order · *yasal metin sürümü* · *onay kaydı* | `ShippingAddress`, `BillingAddress`, iletişim e-postası, firmanın donmuş kimliği (tipe göre kimlik alanları, adres, iletişim, `LegalName`), ÖBF ve MSS ile aydınlatma metninin sürüm bağı, havale IBAN'ı | eşlendi |
| `02 §3.23.4` | Donma anı siparişin oluştuğu an; onaylanan tutar bağlayıcı | Order | oluşma anı | eşlendi |
| `02 §3.23.5` | Marka ayarları siparişe donmaz | Branding | — (siparişe kopyalanmaz) | eşlendi |
| `02 §3.24.1` | İki onay kutusu (ÖBF teyidi, MSS onayı); KVKK rızası kutusu yok; ödeme yükümlülüğü yazısı | *onay kaydı* · veri doğurmaz — arayüz (`04`) | iki kutunun işaretlenmiş olması — sipariş ancak işaretlenince oluşur | eşlendi |
| `02 §3.24.2` | Dijital kalemde ve hizmet kaleminde ek zorunlu kutu — cayma istisnasının kurucu koşulu | *onay kaydı* | dijital ve hizmet kutusu | eşlendi |
| `02 §3.24.3` | ÖBF onay özetiyle aynı, dört bilgi ekler; sabit metinler; "sipariş nasıl kurulur"; kısa cayma özeti; on iki punto | PreInformationForm · veri doğurmaz — arayüz (`04`) | form ayarlardan ve siparişten üretilir — yeni alan doğurmaz (sabit metinler kodda); siparişe kopya mı, sürümden mi — VM4-11 | eşlendi |
| `02 §3.24.4` | Yaş için ayrı beyan yok; üyelik sözleşmesi yok | yoktur — kapsam dışı | doğum tarihi ve yaş beyanı alanı yok | veri doğurmaz |
| `02 §3.24.5` | Misafirin e-postası kodla sınanmaz, özette gösterilir | Order | iletişim e-postası | eşlendi |
| `02 §3.24.6` | Onaylanan sürümler ve kimlik donar; dijital ve hizmet kutularının kaydı: hangi kutu, gösterilen metni, kapsadığı kalemler; ayrı zaman damgası yok | *onay kaydı* · Order | kutu, kutunun gösterilen metni, kapsadığı kalemler (K-595); an siparişin oluşma anı | eşlendi |
| `02 §3.24.7` | Ödeme adımında müşteri notu alanı yok | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §3.24.8` | "İşlem rehberi" sabit sayfası; ürünün ürettiği sabit metin | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §3.25.1` | Ürün fatura kesmez; fatura verisini taşır ve dışa aktarır | Order · OrderItem | fatura verisinin alanları siparişte ve kalemde — dışa aktarma `02 §10.7` (1b) | eşlendi |
| `02 §3.25.2` | Fatura sistemde saklanmaz ve gösterilmez | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §3.25.3` | Dışa aktarmada siparişin donmuş firma kimliği kullanılır | Order | firmanın donmuş kimliği | eşlendi |
| `02 §3.26` metin | İptal, cayma, iade ve ayıp kuralları §7'de | veri doğurmaz — yazım konvansiyonu | — (`02 §7` 1b'de) | veri doğurmaz |

**§3.27–§3.30 — kurumsal içerik, marka ve site (47)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §3.27.1` | Kurumsal içerik hazır tiplerle ve düzeni sabit genel sayfayla; sayfa kurucu yok | Content | tip (kayıt türü) — modeli VM4-10 | eşlendi |
| `02 §3.27.2` | Beş hazır tip + genel sayfa ve duyuru; galeri tipi yok | Content | içerik tipleri: AboutPage, Offering, PortfolioItem, FaqItem, Branch, CustomPage, Announcement | eşlendi |
| `02 §3.27.3` | Hakkımızda tek kayıt: kısa tanıtım, uzun metin, görseller; silinmez | AboutPage · *görsel* | kısa tanıtım (uzunluk tavanı), uzun metin; tekil; silinmez | eşlendi |
| `02 §3.27.4` | Hizmet tanıtımı fiyatsız: ad, kısa açıklama, metin, görseller, sayfa | Offering · *görsel* | ad, kısa açıklama, metin, sayfa adresi | eşlendi |
| `02 §3.27.5` | Referans iş: başlık, kısa açıklama, metin, görseller, sayfa; müşteri adı, yıl, kategori alanı yok | PortfolioItem · *görsel* | başlık, kısa açıklama, metin, sayfa adresi | eşlendi |
| `02 §3.27.6` | SSS soru ve cevap, tek sayfada; soru tavanlı | FaqItem | soru (uzunluk tavanı), cevap | eşlendi |
| `02 §3.27.7` | Şube: ad ve adres zorunlu (il listeden); telefon, çalışma saatleri, görsel, harita bağlantısı isteğe bağlı | Branch · *il listesi* · *görsel* | ad, il, ilçe ve açık adres, telefon, çalışma saatleri (tavanlı), harita bağlantısı | eşlendi |
| `02 §3.27.8` | İletişim sayfası ayrı tip değil | veri doğurmaz — türetilir | — (Company, Branch ve form) | veri doğurmaz |
| `02 §3.27.9` | Genel sayfa başlık, metin, görseller; adet sınırı yok; yasal metin genel sayfa değil | CustomPage · *görsel* | başlık, metin, sayfa adresi | eşlendi |
| `02 §3.27.10` | Hizmet tanıtımı ve referans iş ürün bağlayabilir; yalnız yayındaki ürün görünür; silinen ürün bağdan kalkar | *içerik–ürün bağı* | içerik kaydı, ürün (ürün düzlemi; silmeyle kalkar) | eşlendi |
| `02 §3.27.11` | Listeleri firma elle sıralar | Offering · PortfolioItem · FaqItem · Branch · CustomPage | sıra | eşlendi |
| `02 §3.27.12` | Sosyal medya bağlantıları (kapalı platform listesi) ve WhatsApp numarası; siparişe donmaz | SocialLink | platform (kapalı liste), bağlantı; WhatsApp numarası | eşlendi |
| `02 §3.27.13` | Hiçbir tip zorunlu değil; kayıt düzeyinde yayın kapısı tipe göre | Content | tipe göre zorunlu alanlar — kısıt (yayında) | eşlendi |
| `02 §3.27.14` | Hakkımızda boşken kurumsal blok marka adı ve logoyu gösterir | veri doğurmaz — arayüz (`04`) | — | veri doğurmaz |
| `02 §3.27.15` | İçerik görselleri ürünün kurallarına tabi; kısa alanlara karakter tavanı | *görsel* · Content | ana görsel işareti, galeri sırası, alternatif metin; kısa alanların uzunluk tavanı | eşlendi |
| `02 §3.27.16` | Uzun metinler metin biçimi setini kullanır | Content | biçimli metin alanları | eşlendi |
| `02 §3.27.17` | Gömülü video oynatıcı yok | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §3.27.18` | İçeriğe dosya eki eklenmez | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §3.27.19` | Müşteri görüşleri ayrı tip değil | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §3.27.20` | Blog yok | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §3.27.21` | Duyuru tekil kayıt: metin, isteğe bağlı bağlantı, isteğe bağlı tarih aralığı; yayın durumu; metin düzenlemesi ize yazılmaz | Announcement · AuditLog | metin (tavanlı), bağlantı, başlangıç ve bitiş tarihi; `PublishStatus`; yayın durumu değişikliğinin iz satırı | eşlendi |
| `02 §3.27.22` | Site boş kurulur; örnek kayıt yok | veri doğurmaz — kod | — (başlangıç verisi taşımaz — VM8-01) | veri doğurmaz |
| `02 §3.27.23` | Her içerik kaydı yayın durumu taşır: Taslak ya da Yayında | Content | `PublishStatus` (iki değer) | eşlendi |
| `02 §3.27.24` | Taslak adla kaydedilebilir; yayın zorunlu alanları ister; yayın kapısını bozan düzenleme kaydedilmez | Content | ad ya da başlık (taslakta yeterli); kapı — kısıt | eşlendi |
| `02 §3.27.25` | Taslak sayfası ziyaretçiye "sayfa bulunamadı", yöneticiye bantla | veri doğurmaz — arayüz (`04`) | — | veri doğurmaz |
| `02 §3.27.26` | İçerikte sürüm geçmişi yok; yayın değişikliği ize yazılır, metin düzenlemesi yazılmaz | Content · AuditLog | sürüm geçmişi tablosu yok; yayın durumu değişikliğinin iz satırı | eşlendi |
| `02 §3.27.27` | Kurumsal içerik kalıcı silinebilir; işaretler ve ürün bağları kalkar; Hakkımızda silinmez | Content · *içerik–ürün bağı* | fiziksel silme; bağların silinmesi | eşlendi |
| `02 §3.27.28` | İleri tarihli yayın yok; istisna duyurunun tarih aralığı | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §3.28.1` | İki hazır ana sayfa düzeni; vitrin en yeni ürünleri gösterir; kurulumda tanıtım öncelikli | *firma ayarları* | `HomepageLayout` (varsayılan tanıtım öncelikli) | eşlendi |
| `02 §3.28.2` | Kurumsal blok her zaman Hakkımızda | veri doğurmaz — arayüz (`04`) | — | veri doğurmaz |
| `02 §3.28.3` | Hizmet tanıtımı ve referans iş "ana sayfada göster" işareti taşır | Offering · PortfolioItem | ana sayfada göster işareti | eşlendi |
| `02 §3.28.4` | Menü düzenleyici yok; sabit iskelet; firma menü adlarını değiştirebilir, varsayılanlar ürünle gelir | *firma ayarları* | menü öğelerinin adları (varsayılanı kodda) | eşlendi |
| `02 §3.28.5` | Genel sayfalar altbilgide; "menüde göster" işareti | CustomPage | menüde göster işareti | eşlendi |
| `02 §3.28.6` | Yayında ürünü olmayan kategori menüde görünmez | veri doğurmaz — türetilir | — | veri doğurmaz |
| `02 §3.29.1` | Dört görünüm ayarı: logo, marka adı, site simgesi, marka rengi; koyu tema yok; platform imzası ayrı | Branding | `Logo`, `BrandName`, `Favicon`, `BrandColor` | eşlendi |
| `02 §3.29.2` | Logo zorunlu değil; tek logo; görsel kuralları | Branding · *görsel* | `Logo` (isteğe bağlı) | eşlendi |
| `02 §3.29.3` | Marka adı unvandan ayrı, zorunlu, boşaltılamaz | Branding | `BrandName` | eşlendi |
| `02 §3.29.4` | Site simgesi isteğe bağlı | Branding | `Favicon` (isteğe bağlı) | eşlendi |
| `02 §3.29.5` | Marka rengi okunurluğu bozamaz; seçilmezse varsayılan | Branding | `BrandColor` (boş = varsayılan) | eşlendi |
| `02 §3.29.6` | Platform imzası varsayılan açık, kapatılabilir; değişikliği ize yazılır | *firma ayarları* · AuditLog | platform imzası anahtarı | eşlendi |
| `02 §3.30.1` | Ürünün ve kategorinin adresi adından üretilir, ağaçtan bağımsız; önek `07`'nin | Product · Category | sayfa adresi (VM4-16) | eşlendi |
| `02 §3.30.2` | Adres kaydın adından üretilir, ilk yayında sabitlenir; çakışmada sayı eklenir; taslak ve arşivdeki adres dolu; silinenin adresi serbest; kategori adresi oluşturulunca sabit; elle düzenlenmez | Product · Category · Offering · PortfolioItem · CustomPage | sayfa adresi — tekil (silinenler dışında), ilk yayında sabitlenir; ilk yayının anı ya da işareti | eşlendi |
| `02 §3.30.3` | Yönlendirme sistemi yok | yoktur — kapsam dışı | yönlendirme tablosu yok | veri doğurmaz |
| `02 §3.30.4` | Var olmayan, silinmiş ya da taslak adres "sayfa bulunamadı" | veri doğurmaz — arayüz (`04`) | — | veri doğurmaz |
| `02 §3.30.5` | Arama motoru bilgileri kendiliğinden üretilir; SEO alanı yok | veri doğurmaz — türetilir | — (ad, kısa açıklama, ana görsel, marka adından) | veri doğurmaz |
| `02 §3.30.6` | Arama motorlarına kapalı sayfalar | veri doğurmaz — mimari (`05`) | — | veri doğurmaz |
| `02 §3.30.7` | Tek "Paylaş" düğmesi; platform kodları yok | veri doğurmaz — arayüz (`04`) | — | veri doğurmaz |

**§3.31 — panel (3)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §3.31.1` | Sessiz üzerine yazma yok; içerik, ürün ve ayarlarda aynı; mekanizma `05`/`06` | Product · Variant · Content · Coupon · *firma ayarları* | sürüm değeri (bölüm başına sürüm — K-873, VM2-05) | eşlendi |
| `02 §3.31.2` | Siparişe yönelik her işlem güncel hâle karşı; ikinci işlem reddedilir; sipariş kilitlenmez | Order | siparişin sürüm değeri (K-874; artıran yazımlar VM2-05) | eşlendi |
| `02 §3.31` metin | Yönetim tarafının öteki kuralları §10'da | veri doğurmaz — yazım konvansiyonu | — (`02 §10` 1b'de) | veri doğurmaz |

**§3.32–§3.34 — iletişim talebi, yasal metinler, site geneli (28)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §3.32.1` | İletişim formu girişsiz açık; üyede ad ve e-posta ön dolu; her form bir iletişim talebi | ContactRequest | — (kayıt) | eşlendi |
| `02 §3.32.2` | Form alanları: ad, e-posta, konu tipi, mesaj zorunlu; telefon isteğe bağlı; dosya eki ve sipariş numarası alanı yok | ContactRequest | ad, e-posta, `ContactTopic`, mesaj, telefon (isteğe bağlı) | eşlendi |
| `02 §3.32.3` | Konu tipi kapalı beş değer; ayıp talebi listede yok | ContactRequest | `ContactTopic` — değerler VM5-03 | eşlendi |
| `02 §3.32.4` | İki durum Açık ve Kapatıldı; saklama son kapatılıştan, açık talep açılıştan; "Sipariş hakkında" on yıl | ContactRequest | `ContactRequestStatus`, oluşma anı, son kapatıldığı an; süre konu tipinden (K-951) | eşlendi |
| `02 §3.32.5` | Sistem talebi kaydeder ve iletir, yazışma yürütmez | ContactRequest · *gönderim kaydı* | firma bildiriminin satırı; yazışma tablosu yok | eşlendi |
| `02 §3.32.6` | Müşteri talebini takip etmez; numara ve "taleplerim" yok | yoktur — kapsam dışı | müşteriye görünen talep numarası yok | veri doğurmaz |
| `02 §3.32.7` | Spam'e karşı görünmez tuzak alan ve IP başına limit | *deneme kaydı* · veri doğurmaz — kod | IP başına gönderim denemeleri; tuzak alan saklanmaz | eşlendi |
| `02 §3.32.8` | Aydınlatma metni ve çerez politikası yayında değilse form kapalı | veri doğurmaz — türetilir | — | veri doğurmaz |
| `02 §3.32.9` | Ayrı şikâyet kanalı ve canlı destek yok | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §3.32.10` | İndirme hakkı yenileme ve siparişe dair talepler "Sipariş hakkında" tipiyle | ContactRequest | `ContactTopic` | eşlendi |
| `02 §3.32` metin | Kapatılan talebin saklama süresi §4'te | veri doğurmaz — yazım konvansiyonu | — (`02 §4.2` Z-33) | veri doğurmaz |
| `02 §3.33.1` | Dört yasal metin, sürümlü; ilk ikisi üretilir, son ikisi taslakla gelir ve firma düzenler | *yasal metin sürümü* · PreInformationForm · DistanceSalesContract · PrivacyNotice · CookiePolicy | metin türü (dört değer), sürüm numarası; düzenlenen iki metnin içeriği | eşlendi |
| `02 §3.33.2` | Firmanın düzenlediği iki metin zorunlu, boşaltılamaz; aydınlatma taslağı yer tutucular, barındırma konumu bölümü, alıcı grupları bölümü | PrivacyNotice · CookiePolicy | metin (zorunlu); barındırma konumu bölümü (doluysa metin tamamlanmış); taslak kodda gelir — başlangıç verisi (VM8-01) | eşlendi |
| `02 §3.33.3` | Sürüm kendiliğinden artar; eski sürümler silinmez, siparişin süresini izler; sürüm değişikliği ize yazılır | *yasal metin sürümü* · AuditLog | sürüm numarası, yürürlüğe giriş anı, içerik ya da üretim girdisi (VM4-11); silinmez — siparişin süresini izler; iz satırı | eşlendi |
| `02 §3.33.4` | Metin değişince yeniden onay alınmaz; onay adımı açıkken sürüm artarsa sipariş oluşmaz | Order · *yasal metin sürümü* | onaylanan sürüm bağı; onay anında karşılaştırma (K-591) | eşlendi |
| `02 §3.33.5` | Aydınlatma bağlantısı veri açan her ekranda; çerez bandı yok; "gördü" kaydı tutulmaz | Order · veri doğurmaz — arayüz (`04`) | aydınlatma metninin donmuş sürüm bağı; kullanıcı başına gösterim kaydı yok | eşlendi |
| `02 §3.33.6` | ÖBF ve MSS onay e-postasının gövdesinde tam metin; ek yok; donmuş sürümle yeniden gönderilebilir | *gönderim kaydı* · *yasal metin sürümü* | gönderim satırı (gövde saklanmaz — Z-34); yeniden gönderim donmuş sürümden | eşlendi |
| `02 §3.33.7` | Uyuşmazlık bilgisi iki metnin zorunlu parçası; parasal sınır rakamla yazılmaz | veri doğurmaz — kod | — (metnin sabit parçası) | veri doğurmaz |
| `02 §3.33.8` | Ayarlarla çelişen serbest metin firmanın sorumluluğunda; ürün tespit etmez | veri doğurmaz — arayüz (`04`) | — | veri doğurmaz |
| `02 §3.33.9` | Dört metin tamamlanmadan satış açılmaz | veri doğurmaz — türetilir | — (`02 §3.1.5`) | veri doğurmaz |
| `02 §3.34.1` | Tek site, duyarlı tasarım | veri doğurmaz — arayüz (`04`) | — | veri doğurmaz |
| `02 §3.34.2` | Vitrin mobil, panel masaüstü öncelikli; panelde işlev kapatılmaz | veri doğurmaz — arayüz (`04`) | — | veri doğurmaz |
| `02 §3.34.3` | Tarayıcı tabanı son iki büyük sürüm | veri doğurmaz — doğrulama (`12`) | — | veri doğurmaz |
| `02 §3.34.4` | En dar desteklenen genişlik bir sayıdır | veri doğurmaz — arayüz (`04`) | — | veri doğurmaz |
| `02 §3.34.5` | Erişilebilirlik tabanı ve yedi test edilen kural | veri doğurmaz — doğrulama (`12`) · *görsel* | alternatif metin (`02 §3.11.4`'ün alanı) | eşlendi |
| `02 §3.34.6` | Ziyaretçi ölçümü yok; satış özeti yalnız sipariş ve iletişim talebinden | yoktur — kapsam dışı | ölçüm tablosu yok | veri doğurmaz |
| `02 §3.34.7` | Çalışma süresi taahhüdü yok; kesintide süreler dondurulmaz; havale iptali ertelenir | Order · *nabız ve kesinti kaydı* | ertelenen son an (K-898); kesinti kaydı (K-895) | eşlendi |
| `02 §3.34.8` | Sipariş tarafında veri kaybı toleransı sıfır: sipariş, ödeme, geçişler, kalem kayıtları, iletişim talepleri, donmuş sürümler, onay kaydı, gönderim kaydı, işlem izi; sepet ve oturum kaybı kabul | Order · *ödeme kaydı* · *geçiş kaydı* · ContactRequest · *yasal metin sürümü* · *onay kaydı* · *gönderim kaydı* · AuditLog · veri doğurmaz — mimari (`05`) | sipariş tarafı kayıtların sınıfı (silme kategorisi VM2-01); yedek ve aktarım `05 §7.6` | eşlendi |

#### 1.1.6 Ürün Gereksinimleri — `02 §4` zaman kuralları ve §5 durum tanımları (170)

Sayım kuralları kural kimliğiyle, süreler Z kimliğiyle, durumlar kodlarıyla, geçişler S ve Ö kimlikleriyle, kendiliğinden işleyen anlar ve kalem kayıtları madde sırasıyla girer (K-997, K-1000). Bir sürenin satırı başlangıç değerinin alanına eşlenir — imha işi süreyi o alandan hesaplar (K-875; VM9-01).

**`02 §4` — zaman kuralları (61)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §4` metin | Sürelerin evi; değer §11'de; firmanın resmî kurumlara karşı süreleri §12.5'te; sayaçsız yasal yükümlülükler satır olarak | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §4.1.1` | Tek saat dilimi Türkiye saati; depolama biçimi `05` ve `06`'nın | veri doğurmaz — mimari (`05`) | anların türü — saat dilimli damga, UTC (K-870, VM2-04) | veri doğurmaz |
| `02 §4.1.2` | İş günü pazartesi–cuma, resmî tatiller hariç; tatil listesi ürünle gelir, firma düzenlemez | veri doğurmaz — kod | — (iş günü takvimi veritabanında değil — `05 §7.2.3`) | veri doğurmaz |
| `02 §4.1.3` | İki süre iş günüyle, gerisi takvim günüyle | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §4.1.4` | Gün olarak süre ertesi gün başlar, son günün 23:59'unda dolar; dakika ve saat süreleri anından işler | veri doğurmaz — kod | — (tarih olarak anlam taşıyan değer tarih türünde — K-870) | veri doğurmaz |
| `02 §4.1.5` | Üç süre çitli: kargoya verme süresi üst, havale süresi alt ve üst, ifa süresi alt | veri doğurmaz — kod | — (çitler kodda; değerler `02 §11`) | veri doğurmaz |
| `02 §4.2` Z-1 | E-posta doğrulama bağlantısının ömrü; dolunca doğrulanmamış kayıt silinir, bekleyen değişiklik düşer | *bağlantı kaydı* · Customer · *bekleyen e-posta değişikliği* | ilk bağlantının gönderildiği an, ömrün sonu; bekleyen kaydın ve değişikliğin silinmesi | eşlendi |
| `02 §4.2` Z-2 | Şifre sıfırlama bağlantısının ömrü; tek kullanımlık | *bağlantı kaydı* | talep anı, ömrün sonu, kullanıldığı an | eşlendi |
| `02 §4.2` Z-3 | Yönetici daveti bağlantısının ömrü; geri çekme, yeni davet ya da gönderenin kaldırılmasıyla geçersizleşir | *bağlantı kaydı* · AdminInvitation | gönderildiği an, ömrün sonu, kullanıldığı an, geri çekildiği an, daveti gönderen yönetici | eşlendi |
| `02 §4.2` Z-4 | Kısa oturum — son işlemden | Session | kısa; son işlem anı | eşlendi |
| `02 §4.2` Z-5 | Uzun oturum — son işlemden | Session | uzun; son işlem anı | eşlendi |
| `02 §4.2` Z-6 | Sepetin ömrü süresiz | Cart | — (süre alanı yok; misafir sepetinin son erişim anı K-906) | eşlendi |
| `02 §4.2` Z-7 | Kart ödeme süresi; dolunca iptal; son sorgu yanıtsızsa iptal edilmez, "ödeme sonucu alınamadı" işareti | Order · StockReservation | ödeme süresinin bittiği an; ödeme sonucu alınamadı işareti (`02 §6.1.2`, 1b) | eşlendi |
| `02 §4.2` Z-8 | Havale ödeme süresi; düzeltmede yeniden başlar; kesintide iptal ertelenir, sipariş sayfası yeni son günü gösterir | Order · *firma ayarları* | ödeme süresinin bittiği an; ertelenen son an (K-898); havale ödeme süresi ayarı | eşlendi |
| `02 §4.2` Z-9 | Havale ödeme hatırlatması — son iş gününün başında bir kez, ödeme süresi başına | Order · *gönderim kaydı* | hatırlatmanın anı (K-895); süre başına bir kez — gönderildi bilgisi | eşlendi |
| `02 §4.2` Z-10 | Kargoya verme süresi — ödeme onayından; siparişin sözü en uzun, donar | Order · Product · *firma ayarları* | donmuş süre, ödeme onayı anı | eşlendi |
| `02 §4.2` Z-11 | Yasal teslim üst sınırı 30 gün — sipariş onayından; aşılırsa gecikme feshi | Order · LateDeliveryTermination | oluşma anı (sınırın başlangıcı) | eşlendi |
| `02 §4.2` Z-12 | Teslim tarihi — firmanın girdiği, kargo gününden önce ve ileri olamaz | OrderItem | `DeliveryDate` | eşlendi |
| `02 §4.2` Z-39 | İfa süresi — ödeme onayından, kaleme donmuş | OrderItem | donmuş ifa süresi | eşlendi |
| `02 §4.2` Z-13 | Cayma penceresi fiziksel kalem — teslim tarihinden 14 gün; eksik bilgilendirmede bir yıla kadar uzar, firma kaydeder | OrderItem · Withdrawal | `DeliveryDate`; uzayan pencerenin kaydı (K-606, `02 §10.4.10`) | eşlendi |
| `02 §4.2` Z-14 | Cayma penceresi hizmet — sipariş tarihinden ya da fizikselli siparişte teslim tarihinden; ifa tamamlanınca düşer | Order · OrderItem | oluşma anı, `DeliveryDate`, hizmetin tamamlama işareti | eşlendi |
| `02 §4.2` Z-15 | Dijital kalemde cayma hakkı ödemede, üçüncü kutuyla düşer | *onay kaydı* · OrderItem | dijital kutusunun kaydı; teslim işareti | eşlendi |
| `02 §4.2` Z-16 | Cayma geri ödemesi 14 gün — beyandan ya da iade malının ulaştığı tarihten; panel kalan süreyi gösterir | Withdrawal · Return · Refund | beyanın tarih damgası; ulaşma tarihi (geçmişe dönük olabilir, teslim tarihinden önce olamaz) | eşlendi |
| `02 §4.2` Z-42 | Müşterinin iade malını gönderme süresi — beyandan 14 gün; sistem denetlemez | Withdrawal | beyanın tarih damgası (süre türetilir) | eşlendi |
| `02 §4.2` Z-17 | İptalde geri ödeme 14 gün — iptal anından; kartta sistem, havalede firma | Cancellation · Refund | iptal anı | eşlendi |
| `02 §4.2` Z-47 | İfanın imkânsızlaştığının bildirimi 3 gün — firmanın yükümlülüğü, sayaç yok | veri doğurmaz — ürünün dışında | — | veri doğurmaz |
| `02 §4.2` Z-18 | Ayıp talebinin iki yıllık zamanaşımı — kalemin teslim işaretinden; dolunca kanal kapanır | OrderItem · DefectClaim | teslim işaretinin tarihi (süre türetilir) | eşlendi |
| `02 §4.2` Z-19 | İndirme hakkı — süre yok, adet var | OrderItem | indirme hakkı ve sayacı | eşlendi |
| `02 §4.2` Z-20 | İndirimin tarih aralığı | Discount | başlangıç ve bitiş anı | eşlendi |
| `02 §4.2` Z-21 | Referans fiyatın geriye bakış penceresi — 10 gün ya da satışa sunuluştan beri | *fiyat geçmişi* · Variant | fiyat geçmişi; satışa sunulduğu ilk an | eşlendi |
| `02 §4.2` Z-22 | Kuponun tarih aralığı | Coupon | başlangıç ve bitiş | eşlendi |
| `02 §4.2` Z-23 | Duyurunun tarih aralığı — isteğe bağlı | Announcement | başlangıç ve bitiş (isteğe bağlı) | eşlendi |
| `02 §4.2` Z-24 | İleri tarihli yayın yok | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §4.2` Z-25 | KVKK başvurusuna 30 günde cevap — sayaç yok | veri doğurmaz — ürünün dışında | — | veri doğurmaz |
| `02 §4.2` Z-26 | Gönderilemeyen e-postanın üç yeniden denemesi; "e-posta ulaşmadı" işareti ve firma uyarısı | *gönderim kaydı* · *kanal hâli kaydı* | deneme sayısı, sonraki deneme anı, sonuç; işaret gönderim kaydından türer (K-913) | eşlendi |
| `02 §4.2` Z-27 | Deneme limitlerinin kayan penceresi | *deneme kaydı* | an (pencere geçince silinir) | eşlendi |
| `02 §4.2` Z-28 | Sipariş, ödeme, fatura verisi, sözleşme kayıtları, donmuş sürümler, onay kaydı — yılın sonundan 10 yıl; kişisel veri imha, hesap bağı kesilir; yalnız ödemesi alınmış sipariş; geç gelip geri ödenen kart ödemesi de | Order · OrderItem · *ödeme kaydı* · *yasal metin sürümü* · *onay kaydı* · *imha kaydı* | oluşma yılı (başlangıç değeri — K-875); ödemesi alınmış sayılma (K-951); boşaltılan kişisel alanlar (VM9-02) | eşlendi |
| `02 §4.2` Z-41 | Ödemesi hiç alınmamış siparişin kişisel verisi 3 yıl, kişisel verisiz kaydı en az 10 yıl — Başarısız olduğu andan | Order · *imha kaydı* | Başarısız olduğu an (başlangıç değeri); kişisel alanların boşaltılması | eşlendi |
| `02 §4.2` Z-29 | Ayıp talebi kaydı siparişin süresini izler | DefectClaim | siparişe bağ — siparişle imha | eşlendi |
| `02 §4.2` Z-30 | İşlem izi — yılın sonundan 10 yıl; değiştirilemez, silinemez | AuditLog | yazıldığı an (başlangıç değeri); yalnız eklenir (K-877) | eşlendi |
| `02 §4.2` Z-31 | Hesap verisi hesap durdukça; silinince hemen | Customer · Session · AddressBook | anında fiziksel silme | eşlendi |
| `02 §4.2` Z-32 | Sepet süresiz; hesapla silinir | Cart | hesapla silinir | eşlendi |
| `02 §4.2` Z-33 | İletişim talebi — son kapatılıştan, açık talepte açılıştan; "Sipariş hakkında" yasal | ContactRequest | son kapatıldığı an, oluşma anı, `ContactTopic` | eşlendi |
| `02 §4.2` Z-34 | Bildirim gönderim kaydı — hesap, yönetim ve firma bildirimleri; gönderim anından | *gönderim kaydı* | gönderim anı (başlangıç değeri); tür | eşlendi |
| `02 §4.2` Z-43 | Siparişe bağlı müşteri bildirimlerinin gönderim kaydı 10 yıl; ödemesiz siparişte kişisel veri 3 yıl; kol ödeme kaydından okunur | *gönderim kaydı* · *ödeme kaydı* | sipariş bağı, gönderim anı, alıcı ve olayın değerleri (boşaltılabilir); kol ödeme kaydından okunur (K-951) | eşlendi |
| `02 §4.2` Z-35 | Havale hattında geri ödeme IBAN'ı — geri ödeme tamamlanana kadar; erken silinme hâlleri; ödemenin kaydı kalır | Withdrawal · LateDeliveryTermination · Cancellation · RefundIbanRequest · Refund | geri ödeme IBAN'ı (yeri VM4-08), girildiği an, silinme anı; geri ödemenin tarih, tutar, sipariş kaydı | eşlendi |
| `02 §4.2` Z-36 | Periyodik imha — kendiliğinden çalışan iş | *imha kaydı* · *nabız ve kesinti kaydı* | gecelik işin son çalışma anları (K-895) | eşlendi |
| `02 §4.2` Z-40 | Giriş kaydı ve sistem kayıtları; içerikleri `05 §9` | *giriş kaydı* · veri doğurmaz — mimari (`05`) | giriş kaydı: an, IP, hesap, sonuç; sistem kayıtları dosyada | eşlendi |
| `02 §4.2` Z-44 | İmha kaydı — tarih, kayıt türü, sayı; kişisel veri yok; en az 3 yıl | *imha kaydı* | tarih, kayıt türü (VM5-05), sayı | eşlendi |
| `02 §4.2` Z-45 | Geri alma bağlantısının ömrü; tek kullanımlık | *bağlantı kaydı* | değişikliğin geçerli olduğu an, ömrün sonu, kullanıldığı an | eşlendi |
| `02 §4.2` Z-46 | Tanınan tarayıcı işaretinin ömrü; süre dolmadan geçersizleşme hâlleri | RecognizedBrowserMark | son başarılı giriş ya da sıfırlama anı; geçersizleşme | eşlendi |
| `02 §4.2` Z-37 | Veri ihlalinde Kurul'a bildirim 72 saat — firmanın yükümlülüğü | veri doğurmaz — ürünün dışında | — | veri doğurmaz |
| `02 §4.2` Z-38 | Malı dönmeyen caymada havale IBAN'ının kendiliğinden silinmesi; başka kalem kullanıyorsa kalır | Withdrawal · Order | IBAN'ın silineceği gün (K-895); silme anı | eşlendi |
| `02 §4.3` metin | Süresi olmayan ama zamanı kural olan anlar | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §4.3` m1 | Sipariş onayı: numara, donma, ayırma; aynı sepetin önceki siparişi iptal | Order · StockReservation | oluşma anı | eşlendi |
| `02 §4.3` m2 | Ödeme onayı: kesin düşme, kupon kullanılmış, kalemler sepetten çıkar, süre başlar, dijital teslim | Order · StockReservation · Coupon · OrderItem | ödeme onayı anı; ayırmanın hâli; teslim işareti | eşlendi |
| `02 §4.3` m3 | İptal: adet stoğa ve kontenjana döner; kupon hakkı yalnız tamamı iptalde | Cancellation · Variant · Coupon | iptal edilen adet (dönüşün kaynağı — K-791) | eşlendi |
| `02 §4.3` m4 | İade: fiziksel stok firma ekleyince; hizmet kontenjanı cayılan adet kadar; kupon tamamında | Return · Withdrawal · Coupon | teslim alınan adet; cayılan adet | eşlendi |
| `02 §4.3` m5 | Kısmi iptal ve iade: kupon ve kargo yeniden hesaplanmaz; kargo ücreti iki hâlde son geri ödemeye eklenir | Refund · Order | geri ödemenin kargo ücreti parçası | eşlendi |
| `02 §4.3` m6 | Malı dönmeyen caymada IBAN'ın silinmesi — kapanışta ya da otuz gün sonra | NotReturnedClosure · Withdrawal | IBAN'ın silinme anı | eşlendi |
| `02 §4.3` m7 | Kesintide süreler işler; kaçırılanlar sırayla çalışır; yasal süreler dondurulmaz; havale iptali ertelenir | Order · *nabız ve kesinti kaydı* · *iş kuyruğu* | ertelenen son an; kesinti kaydı; zamanlanmış işlerin alanları (K-895) | eşlendi |

**`02 §5` — durum tanımları (109)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §5` metin | Durum adları ve kodları sözlükte; `06` birebir devralır; satış açıklığı varlık durumu değil kapı | veri doğurmaz — yazım konvansiyonu | — (kodlar VM5-02) | veri doğurmaz |
| `02 §5.1` metin | Ürün ve varyant aynı üç değerli seti ayrı ayrı taşır | Product · Variant | `PublishStatus` (iki düzlemde) | eşlendi |
| `02 §5.1` `Draft` | Taslak — ziyaretçiye "sayfa bulunamadı", yöneticiye bantla; taslak varyant listede görünmez | Product · Variant | `PublishStatus` = `Draft` | eşlendi |
| `02 §5.1` `Published` | Yayında — vitrinde, kategoride ve aramada | Product · Variant | `PublishStatus` = `Published` | eşlendi |
| `02 §5.1` `Archived` | Arşiv — listelerde yok, "artık satılmıyor"; silme değil | Product · Variant | `PublishStatus` = `Archived` | eşlendi |
| `02 §5.1` m1 | Yayın kapısı: ad, ana kategori, tip, en az bir Yayında varyant; dijitalde dosya, hizmette ifa süresi | Product · Variant · *dosya kaydı* | kapının okuduğu alanlar — kısıt | eşlendi |
| `02 §5.1` m2 | Altı geçiş serbest; tek sınır yayın kapısı | veri doğurmaz — kod | — (geçiş kaydı tutulmaz; değişiklik işlem izinde — `02 §3.7.1`) | veri doğurmaz |
| `02 §5.1` m3 | Yayın kapısını bozan işlem engellenir | Product · Variant | kapı — kısıt | eşlendi |
| `02 §5.1` m4 | Kalıcı silme bir durum değil | Product · Variant | fiziksel silme — `PublishStatus`'a "silindi" değeri eklenmez | eşlendi |
| `02 §5.1` m5 | Zamanlanmış yayın ve "fiyat sorunuz" durumu yok; satışın kapatılması yayın durumunu değiştirmez | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §5.1` m6 | Tükenmişlik yayın durumu değil | veri doğurmaz — türetilir | — | veri doğurmaz |
| `02 §5.2` metin | Kurumsal içerik yayın durumunun iki değerini kullanır; her kayıt kendi durumunu taşır | Content | `PublishStatus` (iki değer) | eşlendi |
| `02 §5.2` `Draft` | Taslak — sayfa "sayfa bulunamadı", menüde ve listelerde yok; yönetici önizler | Content | `PublishStatus` = `Draft` | eşlendi |
| `02 §5.2` `Published` | Yayında — kayıt sitede görünür | Content | `PublishStatus` = `Published` | eşlendi |
| `02 §5.2` m1 | Taslak → Yayında: zorunlu alanlar dolu; ikinci onay yok | Content | tipe göre zorunlu alanlar — kısıt | eşlendi |
| `02 §5.2` m2 | Yayında → Taslak | Content | `PublishStatus` | eşlendi |
| `02 §5.2` m3 | Arşiv, ileri tarihli yayın ve sürüm geçmişi yok | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §5.2` m4 | Yayın kapısını bozan düzenleme kaydedilmez | Content | kapı — kısıt | eşlendi |
| `02 §5.2` m5 | Duyuru tekil; tarihleri girilmişse yalnız aralıkta görünür | Announcement | tekil; başlangıç ve bitiş | eşlendi |
| `02 §5.2` m6 | Kalıcı silme durum değil; Hakkımızda silinmez | Content · AboutPage | fiziksel silme; Hakkımızda silinmez | eşlendi |
| `02 §5.3.1` | Sipariş iki bağımsız eksende durum taşır; kalem düzeyinde durum makinesi yok, kayıtlar var | Order | `OrderStatus`, `PaymentStatus` | eşlendi |
| `02 §5.3.2` | Sipariş Alındı + Bekliyor doğar; numara, donma, ayırma; dijital dosya donmaz | Order · OrderItem · StockReservation | başlangıç değerleri `Placed`, `Pending`; oluşma anı | eşlendi |
| `02 §5.3.3` | Onay anında özet değişmişse sipariş doğmaz | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §5.3.4` | Her ödeme denemesi ayrı sipariş; kendiliğinden iptal edilen ödenmemiş sipariş müşteriye görünmez; limite yalnız iki tür sayılır | Order · Cancellation · *geçiş kaydı* | iptalin türü (kendiliğinden — sebep) | eşlendi |
| `02 §5.3.5` | Sipariş hesabın kaderinden bağımsız | Order | hesap bağı isteğe bağlı (hesap silinince kalkar) | eşlendi |
| `02 §5.4` metin | İzin verilen geçişler beyaz liste | *geçiş kaydı* | geçiş kimliği S1…S11 (değer — K-889) | eşlendi |
| `02 §5.4` `Placed` | Alındı — ödeme onayına ya da fizikselsizde teslim işaretlerine kadar | Order | `OrderStatus` = `Placed` | eşlendi |
| `02 §5.4` `Preparing` | Hazırlanıyor — ödenmiş fiziksel sipariş; kargoya verme süresi başlar | Order | `OrderStatus` = `Preparing` | eşlendi |
| `02 §5.4` `Shipped` | Kargoya verildi — kargo bilgisiyle; iptal kapanır, cayma açılır | Order | `OrderStatus` = `Shipped` | eşlendi |
| `02 §5.4` `Delivered` | Teslim edildi — terminal; teslim işaretleri | Order · OrderItem | `OrderStatus` = `Delivered`; kalemlerin teslim işareti | eşlendi |
| `02 §5.4` `DeliveryFailed` | Teslim edilemedi — kargo geri döndürdü | Order | `OrderStatus` = `DeliveryFailed` | eşlendi |
| `02 §5.4` `Cancelled` | İptal edildi — terminal; açık kalemi kalmamış siparişin kapanışı da | Order | `OrderStatus` = `Cancelled`; kapanışın sebebi kalem kayıtlarından (K-592) | eşlendi |
| `02 §5.4` S1 | Alındı → Hazırlanıyor — ödeme Ödendi, fiziksel kalem var | *geçiş kaydı* | S1 | eşlendi |
| `02 §5.4` S2 | Alındı → Teslim edildi — ödeme onaylı, fizikselsiz siparişin kalemleri teslim işareti aldı | *geçiş kaydı* · OrderItem | S2; teslim işaretleri | eşlendi |
| `02 §5.4` S3 | Alındı → İptal edildi — müşteri, firma (sebepli), süre, kart, aynı sepet ya da son açık kalemin kapanışı | *geçiş kaydı* · Cancellation | S3; kim, sebep; kapanış firma iptali değil | eşlendi |
| `02 §5.4` S4 | Hazırlanıyor → İptal edildi — müşteri, firma ya da fesih, cayma, çıkarmayla kapanış | *geçiş kaydı* · Cancellation | S4; kim, sebep | eşlendi |
| `02 §5.4` S5 | Hazırlanıyor → Kargoya verildi — açık fiziksel kalem; kargo bilgisi | *geçiş kaydı* · Order | S5; kargo şirketi, takip numarası ya da kendi aracıyla | eşlendi |
| `02 §5.4` S6 | Kargoya verildi → Teslim edildi — teslim işareti ve tarihi | *geçiş kaydı* · OrderItem | S6; `DeliveryDate` | eşlendi |
| `02 §5.4` S7 | Kargoya verildi → Teslim edilemedi — firma işaretler | *geçiş kaydı* | S7 | eşlendi |
| `02 §5.4` S8 | Teslim edilemedi → Kargoya verildi — yeniden gönderim, uygun kalemler | *geçiş kaydı* · Order | S8; kargo bilgisi yeniden | eşlendi |
| `02 §5.4` S9 | Teslim edilemedi → İptal edildi — firma iptali ya da açık kalemsiz kapanış | *geçiş kaydı* · Cancellation | S9; sebep ya da kapanış | eşlendi |
| `02 §5.4` S10 | Hazırlanıyor → Teslim edildi — fiziksel kalemler kapandı, kalanlar teslim işareti aldı | *geçiş kaydı* | S10 | eşlendi |
| `02 §5.4` S11 | Teslim edilemedi → Teslim edildi — geri dönen fiziksel kalemler kapandı, kalanlar teslim işareti aldı | *geçiş kaydı* | S11 | eşlendi |
| `02 §5.4` m1 | Geri yönlü geçiş yasak; iptal yeniden açılmaz; düzeltme ayrı | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §5.4` m2 | Hangi geçişin e-posta ürettiği §9.2'de | veri doğurmaz — yazım konvansiyonu | — (`02 §9.2` 1b'de) | veri doğurmaz |
| `02 §5.4` m3 | Yönetici müdahaleleri geçiş değil; adres düzeltmesi ve çıkarmanın sınırı | *çıkarma kaydı* · AuditLog | çıkarma kaydı; müdahalelerin iz satırı (`02 §10.4`) | eşlendi |
| `02 §5.4` m4 | Kalem ve adet düzeyinde iptal; ödenmemiş sipariş bütünüyle iptal | Cancellation | kalem, adet | eşlendi |
| `02 §5.4` m5 | Yanlış geçişin düzeltilmesi yeni bir geçiş; sebep seçilir; iz temizlenmez; terminalden yalnız bir öncekine | *geçiş kaydı* · AuditLog · StockReservation | düzeltme geçişi ve `CorrectionReason`; ayırmaların yeniden istenmesi (K-566) | eşlendi |
| `02 §5.5` `Pending` | Bekliyor | Order | `PaymentStatus` = `Pending` | eşlendi |
| `02 §5.5` `Paid` | Ödendi — kartta sağlayıcı bildirimi, havalede firmanın işareti | Order · *ödeme kaydı* | `PaymentStatus` = `Paid`; ödemenin kaydı | eşlendi |
| `02 §5.5` `Failed` | Başarısız — terminal; sonradan gelen kart ödemesi geri ödenir, ödeme kaydında durur | Order · *ödeme kaydı* | `PaymentStatus` = `Failed`; geç gelen ödeme ve geri ödemesi | eşlendi |
| `02 §5.5` `PartiallyRefunded` | Kısmen geri ödendi | Order | `PaymentStatus` = `PartiallyRefunded` | eşlendi |
| `02 §5.5` `Refunded` | Geri ödendi — terminal | Order | `PaymentStatus` = `Refunded` | eşlendi |
| `02 §5.5` Ö1 | Bekliyor → Ödendi — sağlayıcı bildirimi ya da havale işareti (geri alınamaz) | *geçiş kaydı* · *ödeme kaydı* | Ö1 | eşlendi |
| `02 §5.5` Ö2 | Bekliyor → Başarısız — süre, kart ya da ödenmeden iptal | *geçiş kaydı* | Ö2 | eşlendi |
| `02 §5.5` Ö3 | Ödendi → Kısmen geri ödendi | *geçiş kaydı* · Refund | Ö3 | eşlendi |
| `02 §5.5` Ö4 | Ödendi → Geri ödendi | *geçiş kaydı* · Refund | Ö4 | eşlendi |
| `02 §5.5` Ö5 | Kısmen geri ödendi → Geri ödendi | *geçiş kaydı* · Refund | Ö5 | eşlendi |
| `02 §5.5` m1 | Geri ödemenin tetikleyicileri; tutar bazlı kısmi geri ödeme kalemden bağımsız ya da kaleme bağlı, ödenmiş tutarı aşamaz; havalede IBAN yoksa istenir | Refund · RefundIbanRequest | tür (iptal, cayma, ayıp, tutar bazlı), tutar, kalem bağı (isteğe bağlı), kim işledi, gönderildiği an | eşlendi |
| `02 §5.5` m2 | Başarısız → Ödendi yasak; iptal edilmiş siparişe gelen kart ödemesi kendiliğinden geri ödenir | *ödeme kaydı* · *geri ödeme kaydı* | geç ödeme ve otomatik geri ödemesi | eşlendi |
| `02 §5.5` m3 | Kart iadesi başarısızsa eksen değişmez; "gerçekleşmedi" ve "sonuç bekleniyor" işaretleri | *geri ödeme kaydı* · Order | geri ödeme kaydının hâli (K-940); `RefundFailedMark`, `RefundOutcomePendingMark` | eşlendi |
| `02 §5.5` m4 | Tek düzeltme yanlış havale "ödendi" işareti | *geçiş kaydı* · AuditLog | düzeltme geçişi | eşlendi |
| `02 §5.5` m5 | Ödeme eksenine "İptal edildi" eklenmez; sebep iptal kaydından | Cancellation | iptal sebebi | eşlendi |
| `02 §5.5` m6 | Ters ibraz ekseni değiştirmez | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §5.5` m7 | Eksik ödeme durumu yok | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §5.6.1` | Tek kapı koşulu; eşzamanlı kendiliğinden geçişler; yasak birleşimler; düzeltme bağı bozamaz | Order | iki eksenin birleşim kısıtı (uygulama düzeyi) | eşlendi |
| `02 §5.6.2` | Yalnız dijital siparişte ödeme onayı Teslim edildi'ye alır | *geçiş kaydı* | S2 (sistem) | eşlendi |
| `02 §5.6.3` | Süre dolunca ya da kart başarısızsa kendiliğinden iptal; son sorgu; yanıtsızsa iptal edilmez | Order · StockReservation | ödeme süresinin bittiği an; ödeme sonucu alınamadı işareti | eşlendi |
| `02 §5.6.4` | Aynı sepetten yeni sipariş öncekini iptal eder | Order | sepet bağı | eşlendi |
| `02 §5.6.5` | Ödenmeden iptal edilen siparişin ödemesi Başarısız; sebep iptal kaydında | Cancellation · Order | iptal kaydı; `PaymentStatus` | eşlendi |
| `02 §5.6.6` | Cayma beyanı ekseni değiştirmez; teslim işaretsiz kalemde cayılan adetleri kapatır; açık kalem kalmazsa kendiliğinden kapanış | Withdrawal | cayılan adet; kalemin açık adedi kayıtlardan (K-790) | eşlendi |
| `02 §5.7` metin | Tipler için ayrı durum seti yok; hatlar kullanılmayan durumlarla | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §5.7` m1 | En az bir fiziksel kalem: tam hat; dijital ve hizmet hattı beklemez | veri doğurmaz — kod | — (hat kalemlerin `ProductType`'ından) | veri doğurmaz |
| `02 §5.7` m2 | Yalnız dijital: ödeme onayında Teslim edildi | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §5.7` m3 | Yalnız hizmet: tamamlamada Teslim edildi | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §5.7` m4 | Dijital + hizmet: Alındı'da bekler | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §5.7` m5 | Fiziksel kalemleri iptal edilmiş karışık sipariş fizikselsiz hatta düşer; hizmetin penceresi onay anındaki kalemlerden | veri doğurmaz — kod · OrderItem | kalemin donmuş `ProductType`'ı | eşlendi |
| `02 §5.8` metin | Kalem durum makinesi taşımaz; kayıtlar adedi taşır, aynı tür birden çok kez doğar; açık adet kayıtlardan okunur | OrderItem · Cancellation · *çıkarma kaydı* · LateDeliveryTermination · Withdrawal · Return · ReturnRejection · NotReturnedClosure · DefectClaim | her kaydın adedi; açık adedin saklanıp saklanmadığı VM2-06 (K-787) | eşlendi |
| `02 §5.8` m1 | Teslim işareti ve teslim tarihi — fizikselde sipariş düzeyinde bir kez | OrderItem | `DeliveryDate` | eşlendi |
| `02 §5.8` m2 | İptal kaydı — adet ve firma iptalinde sebep | Cancellation | kalem, adet, `CancellationReason` (firma iptalinde), açıklama ("diğer") | eşlendi |
| `02 §5.8` m3 | Çıkarma kaydı — yöneticinin kalemi çıkarması ya da adedini azaltması; işlem izine | *çıkarma kaydı* · AuditLog | kalem, adet, yönetici; iz satırı — sözlükte terimi yok | GAP adayı (VA-1) |
| `02 §5.8` m4 | Gecikme feshi — tarih damgası, teslim edilmemiş fiziksel kalemlerin açık adetleri; sipariş sayfasından ya da firmanın kaydıyla | LateDeliveryTermination | tarih damgası (başka kanalda bildirimin ulaştığı tarih), kapsadığı kalemler ve adetler, kaydeden (müşteri ya da yönetici) | eşlendi |
| `02 §5.8` m5 | Cayma beyanı — damga, cayılan adet, iade adresi, gönderme süresi, geri ödemesi; ardışık beyanlar ayrı; teslim günü çakışmasında firmanın cevabı | Withdrawal | tarih damgası, adet, iade adresi, kaydeden, teslimden önce mi sonra mı cevabı (K-608) | eşlendi |
| `02 §5.8` m6 | İade teslim alma — ulaşma tarihi ve ulaşan adet; parça parça ayrı kayıt; stoğa ekleme | Return | ulaşma tarihi, ulaşan adet, beyan bağı | eşlendi |
| `02 §5.8` m7 | İade reddi — ulaşma tarihi; işlem izine; geri ödeme yok, IBAN silinir; geri alınmaz | ReturnRejection · AuditLog | ulaşma tarihi, adet; iz satırı | eşlendi |
| `02 §5.8` m8 | "Mal dönmedi" kapanışı — malı dönmemiş adetlere; işlem izine; mal ulaşırsa yeniden açılır; IBAN'ı siler | NotReturnedClosure · AuditLog | adet, an; iz satırı; yeniden açılma (teslim alma kaydıyla) | eşlendi |
| `02 §5.8` m9 | IBAN isteği — kaleme düşen istek ve tarihi; altı tetikleyici; IBAN girilene kadar "IBAN bekleniyor"; işlem izine | RefundIbanRequest · AuditLog | kalem, istek tarihi, tetikleyen kayıt, karşılandığı an | eşlendi |
| `02 §5.8` m10 | Ayıp talebi — kalem, ayıplı adet, sipariş; yeniden açılabilir; iade adresini taşır | DefectClaim | kalem, ayıplı adet, iade adresi, açıklama | eşlendi |
| `02 §5.8` m11 | İndirme sayacı — dijital kalemde | OrderItem | indirme sayacı | eşlendi |
| `02 §5.9` metin | Dört geçiş "geri alınamaz" onayı ister; müşterinin dört işlemi de son adımda onaylanır | veri doğurmaz — arayüz (`04`) | — | veri doğurmaz |
| `02 §5.10` metin | Ayıp talebinin kodları kayda geçmiş | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §5.10` `Open` | Açık — talep bu durumda doğar | DefectClaim | `DefectClaimStatus` = `Open` | eşlendi |
| `02 §5.10` `Resolved` | Çözüldü | DefectClaim | `DefectClaimStatus` = `Resolved` | eşlendi |
| `02 §5.10` m1 | Açık → Çözüldü; seçimlik hakların yürütümü sistem dışında | DefectClaim | `DefectClaimStatus`; seçimlik hak alanı yok | eşlendi |
| `02 §5.10` m2 | Çözüldü → Açık; firma ya da müşteri; tek kayıt | DefectClaim | durum ve yeniden açılma | eşlendi |
| `02 §5.10` m3 | Kalem başına tek ayıp talebi kaydı | DefectClaim | kalem başına tekil — kısıt | eşlendi |
| `02 §5.11.1` | İndirim tarih aralığında kendiliğinden işler; indirimli fiyat fiyat geçmişine yazılır | Discount · *fiyat geçmişi* | başlangıç ve bitiş; indirimli fiyatın geçmiş satırı | eşlendi |
| `02 §5.11.2` | Kupon tarih aralığında ve hak kaldıkça geçerli | Coupon | tarih aralığı, kalan hak (türetilir) | eşlendi |
| `02 §5.11.3` | Kupon hakkının ömrü: ayrılır → kullanılır → döner | Coupon · StockReservation | hakkın hâli | eşlendi |
| `02 §5.11.4` | Kuponun kalem payı donar, geri ödemede kullanılır | OrderItem | donmuş kupon payı | eşlendi |
| `02 §5.12.1` | Hesabın durum makinesi yok; doğrulanmamış hesap bekleme aşaması | Customer | durum alanı yok; doğrulanmış mı | eşlendi |
| `02 §5.12.2` | Oturum kısa ya da uzun; kapanma hâlleri | Session | kısa ya da uzun; hesap başına silme | eşlendi |
| `02 §5.12.3` | Deneme limiti hesabın durumunu değiştirmez | *deneme kaydı* | — (hesapta kilit alanı yok) | eşlendi |
| `02 §5.13` `Open` | Açık — talep bu durumda doğar | ContactRequest | `ContactRequestStatus` = `Open` | eşlendi |
| `02 §5.13` `Closed` | Kapatıldı | ContactRequest | `ContactRequestStatus` = `Closed` | eşlendi |
| `02 §5.13` m1 | Açık → Kapatıldı; "Cevaplandı" ara durumu yok | ContactRequest | `ContactRequestStatus` | eşlendi |
| `02 §5.13` m2 | Kapatıldı → Açık; firmanın işlemi | ContactRequest | durum | eşlendi |
| `02 §5.13` m3 | KVKK talebi aynı iki durumu kullanır | ContactRequest | `ContactTopic` | eşlendi |
| `02 §5.13` m4 | Saklama son kapatılıştan; açık talep açılıştan | ContactRequest | son kapatıldığı an, oluşma anı | eşlendi |

#### 1.1.7 Ürün Gereksinimleri — `02 §6` hata ve istisna senaryoları

**Kapı: matrisin 1b oturumu.** Kapsam: `02 §6`'nın 6 kalın kuralı kural düzeyinde; 141 hata satırı 9 alt bölüm düzeyinde (`02 §6.1`…§6.9) — satır tam okunur ve bir kayıt, işaret ya da tarih doğuran satır kimliğiyle ayrıca girer (K-997); bölümün kural numarası taşımayan maddesi ve metni §1.1.1'in birim kuralıyla (K-1000). Sütunlar ve değer kümesi §1.1'in başındaki ve §1.1.2'deki gibidir; 1b sayımını §1.1.1'e, betiğini aynı kod bloğuna ekler.

#### 1.1.8 Ürün Gereksinimleri — `02 §7`–§13

**Kapı: matrisin 1b oturumu.** Kapsam: 152 kalın kural (§7 44 · §8 28 · §9 10 · §10 42 · §12 28); kimlikli tablo satırları — L-1…L-9 (`02 §8.2`), B-1…B-16 ve F-1…F-6 (`02 §9`), P-1…P-49 ve H-1…H-4 (`02 §11`) — 84; §6–§13'ün kural numarası taşımayan maddesi ve metni (§1.1.1'in birim kuralıyla, K-1000; `02 §11`'in parametre tablosu kimlikli satırlarıyla, §13'ün açık kararlar listesi maddeleriyle girer).

#### 1.1.9 Kullanıcı Akışları — `03 §1` ve §4, satır düzeyi

**Kapı: matrisin 1c oturumu.** Kapsam: `03 §1`'in 99 ve §4'ün 59 satırı satır kimliğiyle (`03 §n.m.k`, K-669) tek tek (K-997).

#### 1.1.10 Kullanıcı Akışları — öteki bölümler

**Kapı: matrisin 1c oturumu.** Kapsam: `03 §2` (10 alt bölüm), §3 (5), §5 (3), §6 (3), §8 (9), §9 (5), §10 (4) alt bölüm düzeyinde — 39 satır; bu bölümlerin 505 satırı tam okunur ve Ürün Gereksinimleri'nde karşılığı bulunmayan bir veri (kayıt, işaret, tarih, sayaç, bağ) taşıyan satır kimliğiyle ayrıca girer; `03 §7`'nin bildirim haritası (131 satır) `02 §9`'un B ve F satırlarıyla toplu eşlenir (K-997). `03 §0` ve §11 matrise girmez (§1.1.1).

#### 1.1.11 Devirler — karar satırları, park maddeleri ve gövde cümleleri

**Kapı: matrisin 1d oturumu.** Kapsam: K-479…K-991'in etki sütununda `06` taşıyan 126 karar satırı — Aşama 1'in kalite döngüsü ve checkpoint'i 45, Aşama 2 23, Aşama 3 14, Aşama 4 44 —; bu dokümanın dört park bloğunun 23 talimat maddesi (Aşama 1 iki, Aşama 3 on, Aşama 4 on bir; üç dizin maddesi dizine işaret eder ve devirleri karar satırlarıyla girer); dizinlerin gövde cümleleri — `PHASE1_CONFLICT_SCAN.md` §6.2'nin on, `PHASE3_CONFLICT_SCAN.md` §6.2'nin iki, `PHASE4_CONFLICT_SCAN.md` §6.2'nin dokuz maddesi. Satır düzeyindeki eşleme — hangi entity, hangi alan — burada yapılır (VM1-06); Aşama 1'in workshop satırları (K-01…K-478) ayrı satır olarak girmez, `02`'nin kural ailesiyle karşılanır (K-997; karar kaydı §14.2, betik 2). §1.1.2'nin listesinde 1a satırlarında kullanılmayan iki aday (70, 71) bu oturumda eşlenir.

#### 1.1.12 GAP adayları — karara bağlanacak

**Kapı: matrisin 2. oturumu.** Aşağıdakiler **adaydır, karar değildir**: 2. oturum her adayı kaynakların tam metnine karşı doğrular, düşeni gerekçesiyle düşürür, kalanı karara bağlar — belirgin önerisi olan ve bir W konusuna bağlanmayan aday 2. oturumda öneriyle, bir W konusuna bağlanan aday o konuda (karar kaydı §14.1) — ve kararını §1.3'e ve karar kaydının §3'üne (GAP-12'den) yazar (VM1-05). 1b, 1c ve 1d buldukları adayı bu tabloya ekler. Bir adayın satırları §1.1'in tablolarında "GAP adayı (VA-n)" durumunu taşır.

| # | Aday | Dayandığı satırlar | Not |
|---|---|---|---|
| VA-1 | Ürüne görünen ama sözlükte terimi ve kod karşılığı olmayan kayıtların adı | `02 §1.1` m4, `02 §5.8` m3 · §1.1.2'nin italik adayları | `02 §1.1` m4 ürüne görünen kavramın — panelde ya da müşteriye görünen kayıt, işaret, sayaç — sözlüğe girdiğini, `06`'nın kod adını yalnız teknik kavramlara ve kapalı liste değerlerine verdiğini söyler (K-17, K-531, K-977, K-988). Sözlüğün 105 terimi tam okundu: **ürüne görünen ve terimi olmayanlar** — çıkarma kaydı (`02 §5.8`; panelde kalemin satırı), bekleyen e-posta değişikliği (`02 §3.13.14`; hesap ekranında görünür), onay kutularının kaydı (`02 §3.24.6`; sipariş sayfasında görünür), siparişe donan yasal metin sürümü (`02 §3.23.3`; sipariş sayfası gösterir), fiyat geçmişi (`02 §3.8.5`; fiyatın başlangıç tarihi ürün sayfasında), görsel (`02 §3.11`); **`05`'te kurulan ama K-977'nin teknik kavram listesinde olmayanlar** — siparişin ödeme kaydı (`05 §4.5.3`), dosya kaydı (`05 §7.7`), giriş kaydı (`02 §3.13.20`), yedeğin durum kaydı, anahtar halkası. 2. oturum ayrımı doğrular: teknik olan VM3-03'e, ürüne görünen `02 §1.2`'ye geri beslenecek bir terime (K-652) gider. Bağ ve ayar kayıtları (ürün–kategori bağı, içerik–ürün bağı, firma ayarları, il listesi) adayın dışındadır — adları VM2-03'ün adlandırma kuralıyla türer |

### 1.2 Geri izlenebilirlik (entity/alan → kaynak)

**Kapı: matrisin 2. oturumu.** Tablo §1.1.2'nin adaylarından ve §1.1'in satırlarından betikle kurulur; kaynağı olmayan aday ya da alan gerekçesiz eklemedir ve listelenir; Teknik Mimari'nin doğurduğu teknik kayıtların kaynağı park satırıdır (VM1-04). 1a'nın ara denetimi: §1.1.2'nin 71 adayının 69'u §1.1.3–§1.1.6'nın en az bir satırında geçer; kalan ikisi (70, 71) devir adayıdır (§1.1.11).

| Entity.alan | Beslendiği kaynak | Durum |
|---|---|---|

### 1.3 Boşluklar (GAP) ve kararlar

**Kapı: matrisin 2. oturumu.** GAP kararları buraya ve karar kaydının §3'üne yazılır; "#" sütunu karar kaydının GAP ailesini GAP-12'den sürdürür (K-995). Adaylar §1.1.12'dedir.

| # | Boşluk | Karar | Nereye yansıdı |
|---|---|---|---|

**Üst dokümanlara geri besleme** (K-994). Matrisin dışında — workshop'ta ya da yazımda — alınıp Ürün Gereksinimleri'ne, Kullanıcı Akışları'na, Teknik Mimari'ye ya da MVP Kapsamı'na dönen her karar bu tabloya bir satırla girer (K-652; `checklists/document-stage.md` §3).

| Bulgu | Karar | Değişen bölümler | Sürüm |
|---|---|---|---|

---

## 2. Modelleme kuralları

> **Ne yazılır (bu bölüm dolu başlar — hepsi geç öğrenilen dersler):**

- **Enum'lar kaynak dokümandan birebir alınır**, şablondan kopyalanmaz. Genel bir şablondan gelen değerler bu projede uygulanamaz çıkar; kaynak `02 §5` ve `03 §1`'dir.
- **Silme stratejisi erken tanımlanır.** Her entity üç kategoriden birine girer: kalıcı silinir · yumuşak silinir · asla silinmez (denetim/uyumluluk). Kategori entity başına **baştan** kararlaştırılır.
- **Eşzamanlılık kontrolü baştan eklenir.** Durum makinesi + eşzamanlı geri çağrım ihtimali varsa iyimser eşzamanlılık (satır sürümü) MVP'de bile zorunludur; sonradan eklemek risklidir.
- **Denormalizasyon bilinçli bir karardır.** Her denormalize alan için *"nerede güncellenir, tutarsızlık riski nedir?"* cevaplanır.
- **Yumuşak silme + bileşik birincil anahtar birlikte kullanılmaz.** Silinen kaydın aynı kombinasyonla yeniden oluşturulması anahtar ihlali verir. Çözüm: vekil anahtar + filtreli tekil indeks.
- **Sayısal alanların ölçeği tek yerde tanımlanır** (oran mı yüzde mi, kaç ondalık). İki doküman iki farklı ölçek kullanırsa hata sessizdir.

---

## 3. Entity envanteri

| # | Entity | Sorumluluk | Silme kategorisi |
|---|---|---|---|

## 4. Entity tanımları

### 4.x <EntityAdı>

| Alan | Tip | Zorunlu | Varsayılan | Açıklama | Kaynak |
|---|---|---|---|---|---|

- **İlişkiler:**
- **Kısıtlar:**
- **İndeksler:**
- **Silme davranışı:**

## 5. Enum tanımları

> **Kaynak:** `02 §5` ve `03 §1`. Değerler **birebir** aynı olmalıdır.

| Enum | Değerler | Kaynak referansı |
|---|---|---|

## 6. İlişki diyagramı

## 7. İndeks ve kısıt özeti

## 8. Seed / başlangıç verisi

> **Ne yazılır:** Sistem açılışında gerekli kayıtlar. **Hangilerinin yokluğunda sistem açılmamalı** (fail-fast) açıkça belirtilir — bu liste `DEPLOY_RUNBOOK.md §A`'yı besler.

**Üç sınıf ayrılır:**

| Sınıf | Tanım | Nerede yaşar |
|---|---|---|
| **Seed** | Her kurulumda aynı olan referans verisi (durum kodları, varsayılan parametreler) | Migration / seed dosyası |
| **Fail-fast zorunlu** | Kodda varsayılanı **olmaması gereken** iş-kritik değerler — yoksa sistem açılmaz | `DEPLOY_RUNBOOK §A` |
| **Kuruluma özgü (seed edilemez)** | İlk yönetici, operatör/servis hesabı — kimin olacağı kuruluma bağlıdır, kodda varsayılanı olamaz | `DEPLOY_RUNBOOK §H` + izlenen bootstrap script'i |

> Üçüncü sınıf en sık atlanandır ve ilk deploy'da elle veritabanı komutu yazılmasına yol açar — izlenmez, tekrarlanamaz, ikinci ortamda baştan keşfedilir.

## 9. Saklama ve arşivleme

---

*Shopfolio — Data Model v0.2*
