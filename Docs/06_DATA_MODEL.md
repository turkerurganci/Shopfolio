# Shopfolio — Data Model

**Versiyon: v0.3** | **Bağımlılıklar:** `02`, `03`, `05`, `09`, `10_MVP_SCOPE.md` | **Son güncelleme:** 2026-10-17

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

**Alt bölüm haritası (K-998).** Matris beş oturumda kurulur (K-997): **1a** (2026-10-17) kaynak birimini, envanteri ve kapsama denetimini, aday entity listesini ve Ürün Gereksinimleri'nin `02 §0`–§5'i ile MVP Kapsamı'nın `10 §2`'sinin ileri izlenebilirliğini yazdı; **1b** (2026-10-17) `02 §6`–§13'ün ileri izlenebilirliğini yazdı (K-1001); **1c** Kullanıcı Akışları'nı (`03`), **1d** devirleri yazar; **2** geri izlenebilirliği, kesilmiş okumanın örneklemini, GAP ve gerekçesiz ekleme listesini ve GAP kararlarını yazar. Şablonun §1.1, §1.2 ve §1.3 numaraları değişmez; alt düzey aşağıdaki tabloyla sabittir. Bir oturum bir alt bölümü bölmek ya da birleştirmek zorunda kalırsa bu tabloyu ve ona atıf yapan satırları aynı PR'da günceller (K-670'in kalıbı; `checklists/document-stage.md` §4).

| Alt bölüm | İçerik | Oturum | Durum |
|---|---|---|---|
| §1.1.1 | Kaynak envanteri ve sayım — aile × oturum, betik komutları, kapsama denetimi | 1a; her oturum kendi ailesinin sayımını ve denetimini ekler | ✓ (1a ve 1b'nin aileleri) |
| §1.1.2 | Aday entity listesi ve "veri doğurmaz" değer kümesi | 1a; 1b, 1c, 1d listenin sonuna aday ekleyebilir | ✓ (sonraki oturum ekleme yaparsa günceller) |
| §1.1.3 | MVP Kapsamı — `10 §2` KP satırları (77) | 1a | ✓ |
| §1.1.4 | Ürün Gereksinimleri — `02 §0`–§2: sözlük terimleri (105), kural numarası taşımayan madde ve metin (42) — 147 | 1a | ✓ |
| §1.1.5 | Ürün Gereksinimleri — `02 §3` iş kuralları: 257 kalın kural, 19 madde ve metin — 276 | 1a | ✓ |
| §1.1.6 | Ürün Gereksinimleri — `02 §4` zaman kuralları (61) ve §5 durum tanımları (109) — 170 | 1a | ✓ |
| §1.1.7 | Ürün Gereksinimleri — `02 §6` hata ve istisna senaryoları: 6 kalın kural ve bölüm metni (7); 141 hata satırı alt bölüm düzeyinde — hata tablosu taşıyan sekiz alt bölüm, §6.2–§6.9 (8) — ve kayıt, işaret, tarih, sayaç ya da bağ doğuran 66 satır kimliğiyle ayrıca (K-997, K-1001) — 81 | 1b | ✓ |
| §1.1.8 | Ürün Gereksinimleri — `02 §7`–§13: 152 kalın kural ve `02 §13`'ün dört iki düzeyli maddesi (156); L-1…L-9, B-1…B-16, F-1…F-6, P-1…P-49, H-1…H-4 (84); kural numarası taşımayan madde (77) ve metin (13) (K-1000, K-1001) — 330 | 1b | ✓ |
| §1.1.9 | Kullanıcı Akışları — `03 §1` (99) ve §4 (59) satır düzeyinde (158) | 1c | ⬚ |
| §1.1.10 | Kullanıcı Akışları — `03 §2`, §3, §5, §6, §8, §9, §10: 39 alt bölüm düzeyinde, 505 satır tam okunur ve `02`'de karşılığı olmayan veri taşıyan satır kimliğiyle ayrıca girer; §7'nin 131 satırı `02 §9`'un B ve F satırlarıyla toplu (K-997) | 1c | ⬚ |
| §1.1.11 | Devirler — K-479…K-991'in etki sütununda `06` taşıyan 126 karar satırı; park bloklarının 23 talimat maddesi; dizinlerin gövde cümleleri — Aşama 1 on, Aşama 3 iki, Aşama 4 dokuz (21) | 1d | ⬚ |
| §1.1.12 | GAP adayları (`VA-n`) — karara bağlanacak | 1a açtı; 1b, 1c, 1d ekler; 2 karara bağlar | 2 aday (VA-1, VA-2) |
| §1.2 | Geri izlenebilirlik (`Entity.alan` → kaynak); gerekçesiz ekleme listesi | 2 | ⬚ |
| §1.3 | Boşluklar (GAP) ve kararlar; üst dokümanlara geri besleme tablosu (K-994) | 2; workshop ve yazım geri besleme satırı ekler | ⬚ |

### 1.1 İleri izlenebilirlik (gereksinim/akış → entity.alan)

Her satır şablonun beş sütununu taşır: **Kaynak ID · Kaynak özeti · Entity · Alan · Durum** (K-999). **Kaynak ID** kaynağın kimliğidir: kural `02 §n.m.k`; sözlük terimi `02 §1.2` ve kod karşılığı; kimlikli tablo satırı `02 §4.2` Z-n, `02 §5.4` Sn, `02 §5.5` Ön; durum satırı `02 §5.x` ve durumun kodu; kural numarası taşımayan liste maddesi ya da tablo satırı `02 §n.m` mk — k alt bölümdeki sırasıdır —; alt bölümün kural ve madde dışındaki metni `02 §n.m` metin; kapsam satırı KP-n (K-1000). **Kaynak özeti** satırın özetidir, kuralın evi kaynaktır. **Entity** §1.1.2'nin değer kümesinden bir ya da birkaç değerdir (` · ` ile). **Alan** aday alanın Türkçe tarifidir; sözlükte kod karşılığı olan alan kod adıyla yazılır (`ShippingAddress`), öteki alanlara ad verilmez — alanın adı, türü ve kısıtı §4'ün işidir (VM2, VM4; K-17); "—" satırın kaydın kendisini gösterdiğini söyler. **Durum** üç değerden biridir: **eşlendi** (en az bir aday), **veri doğurmaz** (§1.1.2'nin adıyla yazılmış değerlerinden biri — GAP değildir), **GAP adayı (VA-n)** (§1.1.12'de numarasıyla durur). Atıflar K-669 ve K-701'in okunuşuyla yazılır.

#### 1.1.1 Kaynak envanteri ve sayım

Birim K-997'dir; 1a'nın netleştirmesi K-1000'de, 1b'ninki K-1001'dedir. Sayımlar `02` v0.76, `03` v0.28, `10` v0.51 ve karar kaydı v0.117 üzerinde, 2026-10-17'de alındı; 1b'nin sayımları aynı `02` sürümünde, aynı gün.

| Aile | Betiğin saydığı | 1a | Sonraki oturum | Matrise girmeyen |
|---|---|---|---|---|
| `10 §2` KP satırları | 77 | 77 | — | — |
| `02 §1.2` sözlük terimleri | 105 | 105 | — | — (K-997'nin gerekçe hücresindeki 97 betik sayımında 105'tir — K-1000) |
| `02` kalın numaralı kurallar | 438 | 280 — §3 257 · §4 5 · §5 18 | 1b ✓: 158 — §6 6 · §7 44 · §8 28 · §9 10 · §10 42 · §12 28; ayrıca `02 §13`'ün iki düzeyli dört maddesi (13.1–13.4) kural kimliğiyle — üç düzeyli kalıp onları saymaz (K-1001) | — |
| `02` kimlikli tablo satırları | 147 — Z 47 · S 11 · Ö 5 · L 9 · B 16 · F 6 · P 49 · H 4 | 63 — Z, S, Ö | 1b ✓: 84 — L 9 · B 16 · F 6 · P 49 · H 4 | — |
| `02 §5` durum satırları | 20 | 20 — §5.1 3 · §5.2 2 · §5.4 6 · §5.5 5 · §5.10 2 · §5.13 2 | — | — |
| `02` kural numarası taşımayan madde | §0–§5: 101 · §6–§13: 77 | 101 | 1b ✓: 77 — §7 4 · §9 7 · §10 32 · §12 34 | — |
| `02` alt bölümün kural ve madde dışındaki metni | §0–§5: 24 · §6–§13: 13 | 24 | 1b ✓: 13 — §6 1 · §7 3 · §8 3 · §9 2 · §10 2 · §11 2 · §12 1 | Kaynak satırları (`*Kaynak:`), şablonun "Ne yazılır" notları, sözlüğün grup başlıkları, dosya sonundaki sürüm dipnotu (`*Shopfolio — Product Requirements …*`) |
| `02 §6` hata satırları | 141 (9 alt bölüm) | — | 1b ✓: hata tablosu taşıyan 8 alt bölüm satırı (§6.2–§6.9; §6.1 hata satırı taşımaz, altı kuralıyla girer) ve kimliğiyle 66 satır — §6.2 11 · §6.3 5 · §6.4 23 · §6.5 8 · §6.6 4 · §6.7 11 · §6.8 1 · §6.9 3 | — (kimliğiyle girmeyen 75 satır alt bölüm satırıyla karşılanır; hepsi tam okundu) |
| `03` numaralı satırlar | 794 | — | 1c: §1 99 · §4 59 satır düzeyinde; §2 99 · §3 147 · §5 33 · §6 52 · §8 106 · §9 35 · §10 33 alt bölüm düzeyinde (39 alt bölüm); §7 131 toplu | `03 §0` (yazım konvansiyonu) ve `03 §11` (`02`'ye geri besleme tablosu; `n.m` biçimli) |
| Devir satırları — K-479…K-991, etki sütununda `06` | 126 | — | 1d | Aşama 5'in kendi kararları (K-992 ve sonrası) devir değildir |
| Park maddeleri | 26 | — | 1d: 23 talimat maddesi | 3 dizin maddesi — dizinin kendisine işaret eder, devirleri karar satırlarıyla girer |
| Gövde devirleri — dizinlerin §6.2'si | 21 | — | 1d | — |
| **1a toplamı** | | **670** | | |
| **1b toplamı** | | | **411** — beklenen küme 345 (§6 15, §7–§13 330) ve kimliğiyle giren 66 hata satırı | |

**1a — bölüm bazında** (670): `10 §2` 77 · `02 §0` 4 · §1 117 (sözlük 105, madde 9, metin 3) · §2 26 (madde 23, metin 3) · §3 276 (kural 257, madde 11, metin 8) · §4 61 (kural 5, Z 47, madde 7, metin 2) · §5 109 (kural 18, durum 20, S ve Ö 16, madde 47, metin 8). §1.1.3 77 · §1.1.4 147 · §1.1.5 276 · §1.1.6 170.

**1b — bölüm bazında** (411): `02 §6` 81 (kural 6, metin 1, alt bölüm 8, kimliğiyle hata satırı 66) · §7 51 (kural 44, madde 4, metin 3) · §8 40 (kural 28, L 9, metin 3) · §9 41 (kural 10, B 16, F 6, madde 7, metin 2) · §10 76 (kural 42, madde 32, metin 2) · §11 55 (P 49, H 4, metin 2) · §12 63 (kural 28, madde 34, metin 1) · §13 4 (iki düzeyli kural 4). §1.1.7 81 · §1.1.8 330.

**Betik komutları** (Git Bash; depo kökünden). Sayımlar yukarıdaki sayılarla aynı çıkmalı; kapsama denetimleri — 1a'nın üçü, 1b'nin üçü ve başlığa bağlılığın ikisi — ve değer kümesi denetimi boş dönmelidir:

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

# beklenen küme — 1a (670): 02 §0–§5'in birimleri ve KP satırları; 1b (345): 02 §6–§13'ün birimleri — birim kuralı K-1000, K-1001
# sınır "## 14." başlığıdır (02'de yok — dosyanın sonuna kadar okur); dosya sonu sürüm dipnotu birim değildir
exp() {
  awk '
    /^## 14\. /{exit}
    /^## 0\. /{on=1}
    !on{next}
    /^##+ [0-9]/{s=$2; sub(/\.$/,"",s); next}
    /^[[:space:]]*$/ || /^---/ || /^\|[-| ]+\|[[:space:]]*$/{next}
    /^\*\*[0-9]+\.[0-9]+\.[0-9]+ /{r=$1; gsub(/\*/,"",r); print "02 §" r; next}
    /^\*\*13\.[0-9]+ /{r=$1; gsub(/\*/,"",r); print "02 §" r; next}
    /^\|/{
      n=split($0,c,"|"); for(i=1;i<=n;i++){gsub(/^[[:space:]]+|[[:space:]]+$/,"",c[i])}
      if (c[2] ~ /^(Türkçe terim|#|Durum|Siparişin kalemleri|Akış|Aktör|Gruplar)$/) next
      if (c[3] ~ /^(Yönetici ne yapar|Adım|Ürünün sağladığı)$/) next
      if (s=="1.2"){x=c[3]; gsub(/[`†]/,"",x); gsub(/^[[:space:]]+|[[:space:]]+$/,"",x); print "02 §1.2 " x; next}
      if (c[2] ~ /^6\.[0-9]+\.[0-9]+$/){if (!(s in g)){g[s]=1; print "02 §" s}; next}
      if (c[2] ~ /^((Z|L|B|F|P|H)-[0-9]+|S[0-9]+|Ö[0-9]+)$/){print "02 §" s " " c[2]; next}
      if (c[2]=="") next
      if (s ~ /^5\.(1|2|4|5|10|13)$/ && c[3] ~ /^`[A-Za-z]+`$/){x=c[3]; gsub(/`/,"",x); print "02 §" s " " x; next}
      m[s]++; print "02 §" s " m" m[s]; next
    }
    /^[[:space:]]*(- |[0-9]+\. )/{m[s]++; print "02 §" s " m" m[s]; next}
    /^\*Kaynak:/ || /^> \*\*Ne yazılır/ || /^\*Shopfolio — /{next}
    s=="1.2" && /^\*\*[^*]+\*\*[[:space:]]*$/{next}
    !(s in t){t[s]=1; print "02 §" s " metin"}
  ' Docs/02_PRODUCT_REQUIREMENTS.md
  grep -oE '^\| KP-[0-9]+ ' Docs/10_MVP_SCOPE.md | sed 's/^| //; s/ $//'
}
e1a() { exp | grep -vE '^02 §(6|7|8|9|10|11|12|13)([. ]|$)'; }
e1b() { exp | grep -E '^02 §(6|7|8|9|10|11|12|13)([. ]|$)'; }
# 02 §6'nın kimliğiyle girebilen hata satırları (141) — 1b'nin isteğe bağlı kümesi (K-997)
opt() { awk '/^## 6\. /{f=1} /^## 7\. /{f=0} f' Docs/02_PRODUCT_REQUIREMENTS.md | grep -oE '^\| 6\.[0-9]+\.[0-9]+ ' | sed 's/^| /02 §/; s/ $//'; }
# bu dokümanın matris satırları — $1 alt bölüm süzgeci: 1a '[3-6]' · 1b '[78]' · tek alt bölüm '7'
mat() {
  awk -v h="$1" '$0 ~ ("^#### 1\\.1\\." h " "){f=1; next} /^#### /{f=0}
       f && /^\| (`02 §|KP-)/{split($0,c,"|"); x=c[2]; gsub(/`/,"",x); gsub(/^ +| +$/,"",x); gsub(/ +/," ",x); print x}' Docs/06_DATA_MODEL.md
}
# 1a — 670 · 670
e1a | wc -l; mat '[3-6]' | wc -l
comm -23 <(e1a | sort) <(mat '[3-6]' | sort)                        # eksik — boş dönmeli
comm -13 <(e1a | sort) <(mat '[3-6]' | sort)                        # fazla — boş dönmeli
mat '[3-6]' | sort | uniq -d                                        # yinelenen — boş dönmeli
# 1b — 345 · 411 (345 + kimliğiyle giren 66 hata satırı)
e1b | wc -l; mat '[78]' | wc -l
comm -23 <(e1b | sort) <(mat '[78]' | sort)                         # eksik — boş dönmeli
comm -13 <(e1b | sort) <(mat '[78]' | sort) | grep -vxF -f <(opt)   # fazla — boş dönmeli
mat '[78]' | sort | uniq -d                                         # yinelenen — boş dönmeli
mat 7 | grep -vE '^02 §6([. ]|$)'                                   # §1.1.7 yalnız 02 §6 — boş dönmeli
mat 8 | grep -vE '^02 §(7|8|9|10|11|12|13)([. ]|$)'                 # §1.1.8 yalnız 02 §7–§13 — boş dönmeli
exp | awk '{split($2,a,"."); sub(/§/,"",a[1]); t=($3 ~ /^m[0-9]+$/)?"madde":($3=="metin")?"metin":($3=="" && $2 ~ /^§6\.[0-9]+$/)?"alt bölüm":($3=="")?"kural":"kimlik"; if ($1 ~ /^KP-/) {a[1]="KP"; t="KP"} c[a[1]" "t]++} END{for(k in c) print k, c[k]}' | sort -V
mat 7 | grep -cxF -f <(opt)                                         # kimliğiyle giren hata satırı — 66
# "Entity" sütunu §1.1.2'nin değer kümesinin dışına çıkmıyor (boş dönmeli)
awk '/^#### 1\.1\.[3-8] /{f=1; next} /^#### /{f=0} f && /^\| (`02 §|KP-)/{split($0,c,"|"); n=split(c[4],e," · "); for(i=1;i<=n;i++){x=e[i]; gsub(/^ +| +$/,"",x); print x}}' Docs/06_DATA_MODEL.md | sort -u \
  | grep -vxF -f <(awk '/^#### 1\.1\.2 /{f=1; next} /^#### /{f=0} f && /^\| [0-9]+ \|/{split($0,c,"|"); x=c[3]; gsub(/^ +| +$/,"",x); print x} f && /^\| (veri doğurmaz|yoktur)/{split($0,c,"|"); x=c[2]; gsub(/^ +| +$/,"",x); print x}' Docs/06_DATA_MODEL.md)
```

**Yanlış pozitif ve kapsam denetimi (1a).** Kural kalıbı yalnız satır başındaki kalın `n.m.k`'yı yakalar; dört düzeyli kalın kural yoktur ve kuralın devamında liste maddesi yoktur — `02 §3`'ün maddeleri yalnız bölüm girişindedir (betikle sayıldı). KP kalıbı yalnız ilk hücreyi yakalar. Kimlikli satır kalıbı yalnız ilk hücresi kimlik olan satırı yakalar; grup başlığı satırları (`| | **…** |`) ve tablo başlıkları birim değildir. Sözlüğün 105 satırının hepsi bir kod karşılığı taşır (18'i † ile); yinelenen kod yoktur. Durum satırı ikinci hücresi tek bir kod olan satırdır; `02 §5.7`'nin hat tablosu durum taşımaz, maddesiyle girer. Matrisin 1a satırlarında yinelenen kaynak kimliği yoktur; beklenen kümeyle karşılaştırmada eksik ve fazla sıfırdır (2026-10-17). **Betik iki yönlü sınandı** (`audit` skill'i, "Koşum biçimi" 4. madde): matristen bir satır çıkarıldığında denetim o kimliği eksik, bir satır ikilendiğinde yinelenen, kaynakta olmayan bir kimlik eklendiğinde fazla diye döndürdü; değer kümesinin dışında bir "Entity" değeri yazıldığında son denetim o değeri döndürdü.

**Yanlış pozitif ve kapsam denetimi (1b).** `exp()`'in sınırı `## 6.` başlığından `## 14.` başlığına genişletildi — Ürün Gereksinimleri'nde `## 14.` yoktur, betik dosyanın sonuna kadar okur ve dosya sonundaki sürüm dipnotunu birim saymaz; 1a ile 1b aynı işlevden, bölüm süzgeciyle (`e1a`, `e1b`) okunur ve genişletilmiş işlevin 1a parçası eski işlevin çıktısıyla satır satır aynıdır (670; 2026-10-17). Üç ek kural: (1) `02 §6`'nın hata satırı (`| 6.n.k |`) alt bölüm satırını doğurur (`02 §6.n`) — kimliğiyle giren satır `opt()`'un kümesinden gelir ve fazla sayılmaz; (2) `02 §13`'ün kalın maddeleri iki düzeylidir (`**13.1 …**`) ve kural kimliğiyle girer; (3) `02 §10.1.2`, §10.5.1 ve §12.5'in tablo başlıkları ikinci hücreleriyle tanınır — ilk hücreleri ("Alan", "Sipariş", "Yükümlülük") sözlüğün "Sipariş" terimiyle çakışır; ilk denemede ilk hücreyle tanıyan kural sözlüğün `Order` satırını düşürdü ve 1a'nın kümesi 669'a indi — denetim farkı yakaladı. `02 §6`–§13'te dört düzeyli kalın kural yoktur; kimlikli satır kalıbı L, B, F, P, H ailesini yakalar, `02 §11.2`'nin grup başlığı satırları (`| | **…** |`) birim değildir; `02 §11.1`'in kaldırılmış P-2 ve P-11 satırları kimliğiyle girer. Matrisin 1b satırlarında yinelenen kaynak kimliği yoktur; beklenen kümeyle karşılaştırmada eksik ve fazla sıfırdır; §1.1.7 yalnız `02 §6`'yı, §1.1.8 yalnız `02 §7`–§13'ü taşır (2026-10-17). **Betik iki yönlü sınandı:** §1.1.8'den bir satır çıkarıldığında eksik, bir satır ikilendiğinde yinelenen, kaynakta olmayan bir hata satırı (`02 §6.2.99`) eklendiğinde fazla, §1.1.8'e bir `02 §6` satırı taşındığında başlığa bağlılık denetimi o satırı döndürdü.

**Okuma derinliği (checklist §2).** 1a her birimin metnini betikle **satırın tamamı** olarak çıkardı ve tam okudu — sözlük satırları tanım hücresiyle, KP satırları "Neden MVP'de" ve "Kaynak" hücreleriyle, Z satırları altı hücresiyle; hücre başına kesilerek okunan satır yoktur; checklist §2'nin kesilmiş okuma örneklemi bu yüzden 1a'nın satırlarına uygulanmaz. `02 §3`'ün kural dışı gövdesi, §4.2'nin grup başlıkları ve bölüm girişleri de okundu. Tam okunan yalnız matrisin kaynağıdır: `02 §6`–§13, `03` ve devirler kendi oturumlarındadır. **1b** `02 §6`–§13'ü (satır 1339–2189; 288 KB) betikle on üç parçaya bölerek ve hiçbir satırı kesmeden tam okudu — en uzun satır 6.500 karakterdir; hata satırları üç hücresiyle, L satırları ölçüm ekseni ve aşıldığında hücreleriyle, B satırları "E-posta neyi taşır" hücresiyle, P satırları altı hücresiyle, `02 §12.5`'in satırları dört hücresiyle okundu; kimliğiyle girmeyen 75 hata satırı da okundu ve verisi alt bölüm satırında ya da kurala bağlandığı `02 §3` ve §7 satırındadır. GAP adayının dayandığı yerler (`02 §3.23.3`, §10.3.1, §10.4.2, §12.2.7) ve Kullanıcı Akışları ile Arayüz Tanımları'nda "eski adres" taraması ayrıca okundu (VA-2; K-739). Kesilmiş okuma yoktur.

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

Bölüm girişi ve §6.1'in altı ilkesi kural kimliğiyle; §6.2–§6.9'un 141 hata satırı alt bölüm düzeyinde (`02 §6.n`, K-997) — her satır tam okundu; bir kayıt, işaret, tarih, sayaç ya da bağ doğuran satır kimliğiyle (`02 §6.n.k`) ayrıca girer. `02 §6`'nın dokuz alt bölümünün sekizi hata tablosu taşır; §6.1 kural taşır ve alt bölüm satırı almaz (§1.1.1). Alt bölüm satırının "Alan" hücresi alt bölümün veri doğurmayan satırlarını da özetler; kimliğiyle girmeyen satırın verisi aynı alt bölümün kurala bağlandığı `02 §3` ya da §7 satırındadır (§1.1.5, §1.1.8).

**`02 §6` — bölüm girişi ve §6.1 ilkeler (7)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §6` metin | Envanter §2.1'in sekiz akışına göre dizilir; kuralın tam metni §3 ya da §7'de, metnin biçimi `04`'te | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §6.1.1` | Akış e-postaya bağlı değildir; üç yeniden deneme, "e-posta ulaşmadı" işareti (işlem izine yazılmaz), yeniden gönderim (B-1…B-16; B-1 donmuş sürümle), kanal uyarısı; misafir e-postası düzeltmesi; hesap bağlantıları kuralın dışında | *gönderim kaydı* · *kanal hâli kaydı* · OrderEmailHistory · *yasal metin sürümü* · *bağlantı kaydı* | gönderimin deneme sayısı ve sonucu; işaret gönderim kaydından türer (K-913), yeniden gönderim ayrı satır (Z-43); kanal hâli; e-posta düzeltmesinin eski ve yeni değeri; B-1'in metni siparişe donan sürümden | eşlendi |
| `02 §6.1.2` | Kart ödeme süresi dolunca son sorgu; yanıtsızsa iptal yok, ayrılanlar tutulur, "ödeme sonucu alınamadı" işareti; geç başarı bildirimi §6.2.16 | Order · StockReservation · *ödeme kaydı* | ödeme süresinin bittiği an; ödeme sonucu alınamadı işareti (sorgu yanıt verince kalkar); ayırmanın süresi | eşlendi |
| `02 §6.1.3` | Onaylanan özet ile oluşan sipariş birebir aynı; fark siparişi durdurur | veri doğurmaz — kod | — (karşılaştırma onay anında; donan değerler `02 §3.17.2`'nin satırında) | veri doğurmaz |
| `02 §6.1.4` | Sepet hatalarda korunur | Cart | — (sepet silinmez; sipariş oluşunca boşalır — `02 §3.16`) | eşlendi |
| `02 §6.1.5` | Limit aşımı nötr mesaj; hesap kilitlenmez | veri doğurmaz — arayüz (`04`) · *deneme kaydı* | hesapta kilit alanı yok | eşlendi |
| `02 §6.1.6` | Hata ve uyarının anlamı yalnız renkle taşınmaz | veri doğurmaz — arayüz (`04`) | — | veri doğurmaz |

**§6.2 — satın alma (alt bölüm ve 11 satır)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §6.2` | 22 satır: kart reddi, yarım kalan ödeme, onay anında değişen özet, tükenen kalem, arşivlenen ürün, stok ve adet sınırı, dijitalin ikinci kez eklenmesi, teslimat ili, asgari tutar, kupon, misafir e-postası, misafir sipariş, sağlayıcı kesintisi, geç ödeme, satış kapısı, kupon denemesi, kendiliğinden iptal tavanı, sipariş notu, havale tavanı, ters ibraz | Order · OrderItem · Cart · CartItem · StockReservation · Coupon · *ödeme kaydı* · *deneme kaydı* · veri doğurmaz — kod · veri doğurmaz — türetilir | durum ve ödeme ekseni (`02 §5.4`, §5.5); donan değerler ve sürümler; sepetin kalemleri; ayırma; tavanlar siparişlerden sayılır (L-8); sipariş notu alanı yok (6.2.20) | eşlendi |
| `02 §6.2.2` | Aynı sepetten yeniden onayda önceki ödenmemiş sipariş hemen iptal edilir; müşteri kendi ayırdığı stoğa takılmaz | Order · Cart · StockReservation | siparişin doğduğu sepetle bağı; ayırmanın sahibi | eşlendi |
| `02 §6.2.3` | Onay anında fiyat, indirim, kupon, kargo, eşik, teslimat ili, ayırma ya da yasal metin sürümü değişmişse sipariş oluşmaz; metin değiştiyse kutular yeniden işaretlenir | *yasal metin sürümü* · *onay kaydı* · Order | özetin gösterildiği andaki sürüm — onay anında karşılaştırılır; siparişe donan sürüm ve kutuların kaydı | eşlendi |
| `02 §6.2.13` | Misafir e-postası onaydan sonra yanlışsa firma düzeltir: yeni bağlantı, eski adrese B-15; yanlış hesaba düşen siparişin bağı kesilir | OrderEmailHistory · Order · *gönderim kaydı* | eski ve yeni değer; `OrderAccessKey` yenilenir; siparişin hesap bağı (kesilebilir) | eşlendi |
| `02 §6.2.14` | Hesabı olan kişinin misafir siparişi hesaba anında düşer | Order · Customer | siparişin hesap bağı — e-posta eşleşmesiyle (`02 §3.13.3`) | eşlendi |
| `02 §6.2.15` | Sağlayıcıya erişilemez: kart yöntemi seçilemez; ödeme sayfası hiç açılmayan sipariş Başarısız iptal olur, müşteri listesinde görünmez ve L-7'ye sayılmaz | *ödeme kaydı* · Order | ödeme sayfasının açılıp açılmadığı — L-7 sayımından ve müşteri listesinden ayırır; kendiliğinden iptal | eşlendi |
| `02 §6.2.16` | İptal edilmiş siparişe geç başarı bildirimi: kendiliğinden geri ödeme, F-4, B-8; durumlar değişmez; geç ödeme ve geri ödemesi ödeme kaydında tarih ve tutarla, dışa aktarmada; Z-28 ve Z-41 | *ödeme kaydı* · Refund · *geri ödeme kaydı* · *gelen bildirim kaydı* · *gönderim kaydı* | geç ödemenin tarihi ve tutarı; geri ödemenin tarihi ve tutarı; siparişin ödemesi alınmış sayılması — saklama süresinin başlangıcı (Z-28); havale hattında kayıt yok | eşlendi |
| `02 §6.2.18` | Sepette art arda geçersiz kupon: eşikte o sepetin kupon alanı geçici kapanır | *deneme kaydı* · Cart | deneme satırı — sepet ekseni (L-6) | eşlendi |
| `02 §6.2.19` | E-posta (yalnız üye) ve IP ekseninde kendiliğinden iptal edilen ödenmemiş siparişler eşiği aşınca yeni sipariş onaylanamaz; ödeme sayfası açılmamış sipariş hariç | Order · *ödeme kaydı* | L-7 siparişlerin kendisinden sayılır, deneme satırı yazmaz (`05 §6.3.2`): siparişin IP'si, giriş yapmış üyenin hesabı, ödeme durumu, iptalin kendiliğinden olduğu ve anı | eşlendi |
| `02 §6.2.20` | Ödeme adımında not alanı yoktur; müşteri iletişim formunu kullanır | yoktur — kapsam dışı · ContactRequest | siparişte müşteri notu alanı yok; `ContactTopic` "Sipariş hakkında" | eşlendi |
| `02 §6.2.21` | Açık ödenmemiş havale siparişleri tavanda: havale seçilemez (L-8) | veri doğurmaz — türetilir · Order | tavan açık siparişlerden sayılır — siparişin e-postası (üye) ve IP'si | eşlendi |
| `02 §6.2.22` | Ters ibraz ürünün dışında; eksen ve satış özeti değişmez; firma sonucu iç nota yazabilir; kart iadesi reddinde panel hatırlatır | veri doğurmaz — ürünün dışında · Order | iç not (`02 §10.4.7`); ters ibraz alanı yok | eşlendi |

**§6.3 — sipariş takibi (alt bölüm ve 5 satır)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §6.3` | 6 satır: ulaşmayan sipariş e-postası, yanlış sorgu, silinen hesap, müşteriye görünmeyen siparişler, e-postanın ve adresin istenmeden düzeltilmesi | Order · *deneme kaydı* · OrderEmailHistory · AuditLog | siparişe donan e-posta; sorgu denemeleri; düzeltmelerin izi | eşlendi |
| `02 §6.3.2` | Misafir sorgusu IP ve e-posta ekseninde ayrı ayrı limitli (L-5) | *deneme kaydı* | deneme satırı — iki eksen | eşlendi |
| `02 §6.3.3` | Silinen hesabın siparişi misafir yolundan sürer — numara + siparişe donmuş e-posta | Order | `OrderNumber`; siparişe donan iletişim e-postası; hesap bağı silmede kalkar (`02 §3.15.2`) | eşlendi |
| `02 §6.3.4` | Ödemesi hiç başarılı olmamış ve kendiliğinden iptal edilmiş sipariş müşterinin listesinde görünmez; müşterinin ya da firmanın iptal ettiği görünür | Order · *geçiş kaydı* · veri doğurmaz — türetilir | iptalin kendiliğinden mi kişiyle mi olduğu — geçişin yapanı; görünürlük türetilir | eşlendi |
| `02 §6.3.5` | Misafir e-postası istemeden düzeltilir: eski adrese B-15, yeni anahtar, öncekiler geçersiz; aradaki işlemler kalem kayıtlarından ve e-posta geçmişinden okunur; IBAN girişinin zamanı kayıtlı değildir (K-710) | OrderEmailHistory · Order · RefundIbanRequest | eski ve yeni değer, an; `OrderAccessKey` yenilenir; IBAN girişinin zaman damgası yok | eşlendi |
| `02 §6.3.6` | Teslimat adresi istemeden düzeltilir: firma kendi kanallarına dönerek teyit eder; B-10; "Kargoya verildi"den sonra düzeltme yok | Order · AuditLog · *gönderim kaydı* | `ShippingAddress` düzeltmesi; iz satırı sipariş, alan ve işlemi taşır, adres değerini taşımaz (`02 §10.3.1`) — eski adresin yeri VA-2; teyit kaydedilmez | eşlendi |

**§6.4 — iptal, cayma ve iade (alt bölüm ve 23 satır)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §6.4` | 36 satır: iptal ve caymanın sınırları, istisnalar, hizmet, pencere, havale IBAN'ı, geri ödeme süresi, iade malı, kısmi işlemde kupon ve kargo, ayıp, başka kanaldan bildirim, kart iadesinin başarısızlığı, IBAN hataları, kayıp gönderi, iade reddi, değer kaybı | Cancellation · Withdrawal · LateDeliveryTermination · Return · ReturnRejection · NotReturnedClosure · RefundIbanRequest · Refund · *geri ödeme kaydı* · DefectClaim · OrderItem · AuditLog | kalem kayıtları ve adetleri (`02 §5.8`); tarihler; IBAN; geri ödemenin hâli; işaretler | eşlendi |
| `02 §6.4.1` | Kargoya verildikten sonra iptal yolu kapalı; cayma düğmesi o anda açılır | Order · veri doğurmaz — türetilir | `OrderStatus` = `Shipped`; düğmeler durumdan türetilir | eşlendi |
| `02 §6.4.2` | Kargoya verme süresi aşımında iptal düğmesi; otuz günde gecikme feshi — kargo dahil geri ödeme, kanuni faiz uyarısı; kişiye özel üretimde de otuz gün | LateDeliveryTermination · OrderItem · veri doğurmaz — türetilir | fesih damgası, kapsadığı kalemler ve adetler; siparişin firmaya ulaştığı an; düğme ve uyarı türetilir | eşlendi |
| `02 §6.4.4` | Mutlak istisnada cayma düğmesi yok; koşullu istisnada beyan kayda geçer, ambalajı açılmış malda ret teslim alma adımında kaydedilir | Product · Withdrawal · ReturnRejection | `WithdrawalExemptionReason` (ürünün istisna işareti — `02 §3.5`); beyan; ret kaydı | eşlendi |
| `02 §6.4.7` | Havale hattında IBAN iptal, fesih ya da cayma beyanında alınır; kartta istenmez | Cancellation · LateDeliveryTermination · Withdrawal · RefundIbanRequest | geri ödeme IBAN'ı — işlemin kaydıyla | eşlendi |
| `02 §6.4.8` | Teslimden sonraki caymada geri ödeme süresi malın ulaştığı tarihten; teslimden önce ve hizmette beyandan; panel kalan süreyi gösterir | Withdrawal · Return · OrderItem · veri doğurmaz — türetilir | beyanın damgası, `DeliveryDate`, ulaşma tarihi; kalan süre türetilir | eşlendi |
| `02 §6.4.9` | "Gönderi teslim edilemedi — müşteri kaynaklı" sebebiyle iptalde gidiş kargosu ödenmez | Cancellation · Refund | `CancellationReason`; geri ödemeye kargo girip girmediği | eşlendi |
| `02 §6.4.12` | Teslim geç işaretlenir ve pencere işaret anında dolmuştur: düğme kapanır, önceki beyan geçerli | OrderItem · Withdrawal | `DeliveryDate` ile teslim işaretinin konduğu an ayrı; beyanın damgası | eşlendi |
| `02 §6.4.13` | Bir kalemden caymada gidiş kargosu yalnız gönderilen fiziksel kalemlerin tamamından cayıldığında son caymanın geri ödemesine girer | Refund · Withdrawal | geri ödemeye kargonun girip girmediği | eşlendi |
| `02 §6.4.15` | Çözülmüş ayıp tekrarlar: talep yeniden açılır, yeni talep açılmaz | DefectClaim | `DefectClaimStatus`; kalem başına tek kayıt | eşlendi |
| `02 §6.4.16` | Teslimden sonra cayılıp mal gönderilmez: "iade malı bekleniyor", beyandan geçen gün; süre geçince "mal dönmedi" kapanışı (işlem izine, bildirim yok); havalede IBAN kapanışla ya da otuz gün sonra kendiliğinden silinir | Withdrawal · NotReturnedClosure · RefundIbanRequest · AuditLog · veri doğurmaz — türetilir | beyanın damgası; kapanışın anı ve yapanı; IBAN'ın silinme anı (Z-21); bekleme işareti ve gün sayısı türetilir | eşlendi |
| `02 §6.4.17` | Teslim alma işareti gecikirse süre girilen ulaşma tarihinden; tarih geçmişe dönük | Return | ulaşma tarihi (girilen) ile kaydın anı ayrı | eşlendi |
| `02 §6.4.18` | "Mal dönmedi"den sonra mal ulaşırsa kalem yeniden açılır; süre ulaşma tarihinden | NotReturnedClosure · Return | kapanışın geri açılması; ulaşma tarihi | eşlendi |
| `02 §6.4.19` | IBAN silindikten sonra mal ulaşır: kalem yeniden açılır, IBAN alanı açılır, B-14; "IBAN bekleniyor" ve kalan süre | Return · RefundIbanRequest · *gönderim kaydı* · veri doğurmaz — türetilir | ulaşma tarihi; IBAN isteği ve tarihi; işaret türetilir | eşlendi |
| `02 §6.4.20` | IBAN isteğinin tarihi işlem izinde durur; süre durmaz | RefundIbanRequest · AuditLog | isteğin tarihi; iz satırı | eşlendi |
| `02 §6.4.24` | Kart iadesi reddedilir: "geri ödeme gerçekleşmedi" işareti, sayaç, uyarı; yeniden deneme ya da havale yolu; zaman aşımında "geri ödeme sonucu bekleniyor", sorgu, firmanın "gerçekleşmedi" işareti | *geri ödeme kaydı* · Order · RefundIbanRequest · Refund | geri ödeme kaydının hâli (K-940); `RefundFailedMark`, `RefundOutcomePendingMark`; IBAN girildi mi — girildikten sonra kart denenmez | eşlendi |
| `02 §6.4.25` | Başka kanaldan cayma: teyit, panelden kayıt — kalem, adet, bildirimin ulaştığı tarih (geçmişe dönük, ileri tarihli olamaz), IBAN yalnız iletişim e-postasından; B-9; işlem izine | Withdrawal · RefundIbanRequest · AuditLog · *gönderim kaydı* | beyanın kaynağı (panel kaydı), bildirimin ulaştığı tarih ile kaydın anı, adet; IBAN'sız kayıtta IBAN isteği; kaydı yapan | eşlendi |
| `02 §6.4.27` | Yanlış IBAN müşteri tarafından düzeltilir; "havale gerçekleşmedi": IBAN silinir, alan açılır, B-14 | RefundIbanRequest · AuditLog | IBAN'ın düzeltilmesi ve silinmesi; yeni istek | eşlendi |
| `02 §6.4.29` | Fiziksel kalemlerin tamamı teslimden önce kapanırsa teslim tarihi oluşmaz; hizmetin penceresi tamamlanmaya kadar sürer | OrderItem · Order | `DeliveryDate` boş kalır; hizmetin tamamlandı işareti | eşlendi |
| `02 §6.4.30` | Kayıp iade gönderisi: ürün taşıyıcı belgesini almaz; firma öderse kaleme bağlı tutar bazlı geri ödeme, mal sonradan ulaşırsa düşülür | Refund · NotReturnedClosure · veri doğurmaz — ürünün dışında | tutar bazlı geri ödemenin kalem bağı ve tutarı; taşıyıcı belgesi alanı yok | eşlendi |
| `02 §6.4.32` | Pencere dışı ya da mutlak istisnalı kalemden eksik bilgilendirme gerekçesiyle cayma: panel kaydı ayrıca onay ister; pencere bitiminden bir yıldan sonraya tarihli kayıt reddedilir | Withdrawal · AuditLog | bildirimin tarihi; pencere dışı ya da istisna onayı — kayıtta ve izde | eşlendi |
| `02 §6.4.34` | Teslim tarihi beyan günüyle çakışır: panel teslimin beyandan önce mi sonra mı olduğunu sorar; cevap işlem izinde | Withdrawal · OrderItem · AuditLog | beyanın damgası (saat); teslim sırası cevabı (`02 §3.20.11`) | eşlendi |
| `02 §6.4.35` | Ambalajı açılmış dönen mal: "iade reddedildi — koruyucu ambalaj açılmış"; geri ödeme yok, IBAN silinir, stoğa girmez, B-16; ret geri alınmaz | ReturnRejection · RefundIbanRequest · *gönderim kaydı* · Refund | ret kaydı — ulaşma tarihi, adet; geri alınmaz; sonradan tutar bazlı geri ödeme | eşlendi |
| `02 §6.4.36` | Kullanılmış ya da hasarlı dönen mal reddedilemez; değer kaybı ürünün dışında, değerlendirilmez ve kaydedilmez | yoktur — kapsam dışı · veri doğurmaz — ürünün dışında | değer kaybı ve kesinti alanı yok | veri doğurmaz |

**§6.5 — üyelik (alt bölüm ve 8 satır)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §6.5` | 17 satır: doğrulama bağlantısı, doğrulanmamış giriş, kayıtlı e-posta, sıfırlama, zayıf şifre, Google'ın doğrulanmamış e-postası, şifresiz hesap, yeni e-posta, hesap silme, deneme limitleri, veri toplayan girişlerin kapısı, istenmeyen e-posta değişikliği, yeniden doğrulama | Customer · *bağlantı kaydı* · *bekleyen e-posta değişikliği* · SocialLogin · Session · *deneme kaydı* · RecognizedBrowserMark · veri doğurmaz — kod | doğrulanmış mı; bağlantıların ömrü; şifre özeti (boş olabilir); deneme satırları; şifre kuralı kodda | eşlendi |
| `02 §6.5.1` | Doğrulama bağlantısının süresi dolunca kayıt silinir; e-posta yeniden kayda açık | Customer · *bağlantı kaydı* | doğrulanmamış kaydın silinmesi (Z-1) | eşlendi |
| `02 §6.5.9` | Yeni e-posta doğrulanmazsa değişiklik geçerli olmaz | *bekleyen e-posta değişikliği* | bekleyen adres, ömrü | eşlendi |
| `02 §6.5.11` | Giriş, sıfırlama ve kayıt denemeleri IP ve e-posta ekseninde limitli; hesap kilitlenmez; e-posta eksenindeki engel tanınan tarayıcıyı kapatmaz | *deneme kaydı* · RecognizedBrowserMark | deneme satırı (L-1…L-3); tanınan tarayıcı kaydı | eşlendi |
| `02 §6.5.13` | E-posta istenmeden değiştirilir: eski adrese bildirim, "bu değişikliği ben yapmadım" bağlantısıyla geri alma, bütün oturumlar kapanır, şifre yeniden belirlenir; bağlantı açıkken yeniden değiştirilemez | *bağlantı kaydı* · Session · Customer | geri alma bağlantısının ömrü ve kullanılması; eski adres; oturumların silinmesi | eşlendi |
| `02 §6.5.14` | Doğrulama bağlantısı yeniden isteği L-9 ile limitli; yeni bağlantı öncekileri geçersiz kılar, kaydın ömrünü uzatmaz | *deneme kaydı* · *bağlantı kaydı* · Customer | deneme satırı; geçerli tek bağlantı; ilk bağlantının gönderildiği an | eşlendi |
| `02 §6.5.15` | Geri alma bağlantısı açıkken hesap silinmez; ekran bağlantının bitiş tarihini söyler | *bağlantı kaydı* | bağlantının bitiş anı | eşlendi |
| `02 §6.5.16` | Google'ın doğrulanmış e-postası bekleyen kayda eşleşir: kayıt devralınmaz, adı ve şifresiyle silinir; hesap Google ile şifresiz doğar; kapı kapalıysa giriş yok | Customer · SocialLogin | bekleyen kaydın silinmesi; şifresiz hesap; Google bağı | eşlendi |
| `02 §6.5.17` | Yeniden doğrulamada yanlış şifre L-1'e sayılır; işlem yapılmaz | *deneme kaydı* · Session | deneme satırı; tek işlemlik izin verilmez | eşlendi |

**§6.6 — katalog yönetimi (alt bölüm ve 4 satır)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §6.6` | 16 satır: referans fiyat, kategori silme ve taşıma, yayın kapısı, dosyasız dijital varyant, ondalık, kupon tutarları, kargoya verme süresinin çiti, eşzamanlı düzenleme, son yayındaki varyant, sıfır fiyat, %100 indirim, ayrılmış adet, kupon hakkı, açık siparişli ürünün silinmesi | veri doğurmaz — kod · Product · Variant · Category · Discount · Coupon · StockReservation · *dosya kaydı* | sınırlar ve çitler kodda; fiyat iki ondalık; yayın durumu | eşlendi |
| `02 §6.6.10` | İki yönetici aynı kaydı düzenler: ikinci kaydeden uyarılır | veri doğurmaz — mimari (`05`) | kaydın düzenlemeye açıldığı andaki hâlinin karşılaştırılması — veri karşılığı K-800'ün park satırı (VM2) | veri doğurmaz |
| `02 §6.6.14` | Stok ya da kontenjan ayrılmış adedin altına inemez; ayırmayı tutan siparişler gösterilir | Variant · StockReservation | `StockQuantity`; ayrılmış adet ve siparişi | eşlendi |
| `02 §6.6.15` | Kuponun toplam kullanım adedi kullanılmış ve ayrılmış hakların altına inemez | Coupon · StockReservation | toplam adet; kullanılmış ve ayrılmış haklar | eşlendi |
| `02 §6.6.16` | Açık siparişli ürün ya da varyant silinir: sipariş donmuş kalemiyle sürer, silinen varyantın ayırması düşer | OrderItem · StockReservation · Variant | donan ürün değerleri; varyant bağı silinmeye dayanır; ayırmanın düşmesi | eşlendi |

**§6.7 — sipariş yürütümü (alt bölüm ve 11 satır)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §6.7` | 17 satır: havale tutarı ve geç havale, geri dönen gönderi, takip numarası, yanlış geçiş, karşılanamayan sipariş, indirme hakkı, ayıplı dosya, iptale gelen kart ödemesi, işaretlenmeyen teslim, yanlış tarih ve adres, ulaşmayan e-posta, ödenmemiş hizmet, eşzamanlı işlem, gecikme feshi, kesintide dolan havale süresi | Order · OrderItem · Cancellation · LateDeliveryTermination · Return · Refund · *geçiş kaydı* · AuditLog · *gönderim kaydı* · *nabız ve kesinti kaydı* | durumlar; kargo bilgisi; sebepler; tarih düzeltmeleri; havale tutarı alanı yok (6.7.1) | eşlendi |
| `02 §6.7.4` | Kargo şirketi + takip numarası ya da "kendi aracımızla teslim" olmadan kargoya verildi işareti konamaz | Order | kargo şirketi, takip numarası, kendi aracıyla teslim beyanı | eşlendi |
| `02 §6.7.5` | Yanlış geçiş: geri al yok; düzeltme sebep seçilerek yeni geçiş; iz temizlenmez; yanlış iptalin ve erken "ödendi"nin düzeltilme koşulları | *geçiş kaydı* · AuditLog · Order | geçişin düzeltme olduğu ve `CorrectionReason`; düzeltme koşulları kalem kayıtlarından ve geri ödemelerden okunur | eşlendi |
| `02 §6.7.6` | Firma sebep seçerek iptal eder; sebep müşteriye bildirilir ve işlem izine; "stokta bulunamadı" uyarısı | Cancellation · AuditLog · *gönderim kaydı* | `CancellationReason`, adet, an, yapan | eşlendi |
| `02 §6.7.7` | İndirme hakkı dolar; firma kalemin hakkını yeniler | OrderItem · AuditLog | indirme sayacı ve hakkı; yenileme | eşlendi |
| `02 §6.7.8` | Ayıplı dijital dosya yeniden yüklenir; düzeltme eski alıcılara da ulaşır | *dosya kaydı* · Variant | varyantın o anki dosyası — kalem dosyayı varyant üzerinden okur | eşlendi |
| `02 §6.7.11` | Teslim tarihi ve takip bilgisinin düzeltilmesi işlem izine; takip düzeltmesi bildirilir | OrderItem · Order · AuditLog · *gönderim kaydı* | `DeliveryDate`, kargo bilgisi; eski ve yeni değer izde | eşlendi |
| `02 §6.7.12` | Teslimat ya da fatura adresi "Kargoya verildi"ye kadar müşterinin talebiyle düzeltilir; teyit; müşteriye bildirilir | Order · AuditLog · *gönderim kaydı* | `ShippingAddress`, `BillingAddress`; iz satırı adres değerini taşımaz (`02 §10.3.1`) — eski adresin yeri VA-2 | eşlendi |
| `02 §6.7.13` | E-posta üç denemeden sonra ulaşmaz: "e-posta ulaşmadı" işareti; yeniden gönderim | *gönderim kaydı* | sonuç; işaret türer (K-913) | eşlendi |
| `02 §6.7.14` | Hizmetin "tamamlandı" işareti ödeme onayından sonra | OrderItem · Order | tamamlandı işareti; `PaymentStatus` | eşlendi |
| `02 §6.7.15` | Eşzamanlı işlem: önce tamamlanan uygulanır; aynı geçiş ya da geri ödeme kaydı iki kez işlenmez | veri doğurmaz — mimari (`05`) · *geçiş kaydı* · Refund | siparişin sürüm karşılaştırması — mekanizması `05`, veri karşılığı VM2; tekilleştirme | eşlendi |
| `02 §6.7.17` | Site kesintisinde dolan havale süresi: iptal sitenin dönüşünden sonraki ilk iş gününün sonuna ertelenir | *nabız ve kesinti kaydı* · Order | kesintinin başlangıç ve bitişi; ertelenen iptal anı (Z-8) | eşlendi |

**§6.8 — kurumsal içerik (alt bölüm ve 1 satır)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §6.8` | 11 satır: zorunlu alan, Hakkımızda silinmez, boş Hakkımızda, içeriğe bağlı ürün, silinmiş içeriğin adresi, eşzamanlı düzenleme, bot tuzağı, form limiti, veri toplayan girişlerin kapısı, e-posta yanıtı, yayındaki kaydın zorunlu alanı | Content · AboutPage · *içerik–ürün bağı* · ContactRequest · *deneme kaydı* · veri doğurmaz — arayüz (`04`) · veri doğurmaz — türetilir | yayın durumu; bağ (taslak ve arşivde kalır, silmede kalkar); tuzağa düşen gönderim kayıt doğurmaz; yanıt sisteme girmez | eşlendi |
| `02 §6.8.8` | Aynı IP'den çok form gönderimi eşikte engellenir (L-4) | *deneme kaydı* | deneme satırı — IP ekseni | eşlendi |

**§6.9 — mağaza ayarları (alt bölüm ve 3 satır)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §6.9` | 16 satır: kimlik alanları, IBAN'sız havale, sağlayıcı anahtarları, ödeme yöntemi, çitler, marka kimliğinin dört boşluğu, yasal metinler ve iade adresi, son yönetici, davet bağlantısı, çelişen serbest metin, IBAN değişikliği, havalenin kapatılması, iade adresi değişikliği | Company · *firma ayarları* · Branding · PrivacyNotice · CookiePolicy · Admin · AdminInvitation · veri doğurmaz — türetilir · veri doğurmaz — kurulum | satış kapısı koşullardan türer; marka kimliğinin boş hâlleri arayüzün varsayılanı; hatırlatma satırı arayüzde | eşlendi |
| `02 §6.9.12` | Davet bağlantısı kullanılmış, süresi dolmuş, geri çekilmiş, yeni davetle ya da gönderenin kaldırılmasıyla geçersiz | AdminInvitation · *bağlantı kaydı* | davetin hâli, ömrü, gönderen yönetici | eşlendi |
| `02 §6.9.14` | Havale IBAN'ı değişir: yeni IBAN yalnız yeni siparişlere; bekleyen sipariş donan IBAN'ı gösterir | *firma ayarları* · Order | havale IBAN'ı; siparişe donan IBAN | eşlendi |
| `02 §6.9.16` | İade adresi değişir: yapılmış beyanın adresi değişmez | Withdrawal · *firma ayarları* | beyana donan iade adresi | eşlendi |

#### 1.1.8 Ürün Gereksinimleri — `02 §7`–§13

Kural kimliğiyle; kimlikli tablo satırları (L, B, F, P, H) kimliğiyle; kural numarası taşımayan madde ve metin K-1000'in birimiyle (`02 §n.m` mk, `02 §n.m` metin). `02 §13`'ün dört iki düzeyli kalın maddesi kural kimliğiyle girer (`02 §13.n` — §1.1.1). Bir parametrenin satırı (`02 §11`) değerin saklandığı alana eşlenir: firma ayarı *firma ayarları*na, ürün sabiti koda (`05 §8.4`).

**§7.1–§7.2 — üç yol, ortak ilkeler ve iptal (17)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §7.1.1` | Üç yol — iptal, cayma, ayıp talebi; sınırları kalem tipine göre; kayıtta aynı sebep kodunu paylaşmazlar | Cancellation · Withdrawal · DefectClaim | üç ayrı kayıt türü; ayrı sebep alanları | eşlendi |
| `02 §7.1.2` | Tek akış tüketici akışıdır; ürün alıcıdan ticari bilgi istemez | yoktur — kapsam dışı | alıcının tüketici ya da tacir sıfatı alanı yok | veri doğurmaz |
| `02 §7.1.3` | İptal ve iade kalem düzeyinde ve adetle; para işleme giren adet kadar döner | Cancellation · Withdrawal · Return · Refund · OrderItem | işlemin kalem bağı ve adedi; geri ödemenin kalem bağı | eşlendi |
| `02 §7.1.4` | Üç yol sipariş sayfasından; işlemin tarihini taşıyan kayıt kopyası e-postayla | *gönderim kaydı* · veri doğurmaz — e-posta (`08`) | gönderim satırı; içerik `08`'in | eşlendi |
| `02 §7.1.5` | Ayrı şikâyet kanalı yok; uyuşmazlık yolları form ve sözleşmede | veri doğurmaz — kod · PreInformationForm · DistanceSalesContract | — (metin ayarlardan üretilir) | eşlendi |
| `02 §7.1.6` | Kalemin işlem birimi adettir; ardışık işlemler kendi kaydı, damgası, geri ödemesi ve süresiyle; açık adet kayıtlardan okunur ve aşılamaz; gecikme feshinde seçim yok; dijitalin adedi bir; hizmette tamamlandı işareti açık adetlere | Cancellation · *çıkarma kaydı* · LateDeliveryTermination · Withdrawal · OrderItem · veri doğurmaz — türetilir | her kaydın adedi ve damgası; açık adet kayıtlardan türetilir — saklanıp saklanmayacağı VM2-06 (K-787'nin park satırı); kalemde durum alanı yok | eşlendi |
| `02 §7.1.7` | İşleme giren adedin tutarı donmuş değerlerden: kupon payı ve KDV orantılı ve aşağı yuvarlanır; son açık adetleri kapatan işlem kalanı alır; kargo adede bölünmez | OrderItem · Cancellation · *çıkarma kaydı* · LateDeliveryTermination · Withdrawal · Refund | kalemin donmuş birim fiyatı, kupon payı, KDV'si; her işlemin ayırdığı pay ve KDV (K-788'in park satırı) | eşlendi |
| `02 §7.2` metin | Ödenmemiş siparişin kendiliğinden iptali §3.17.5 ve §3.17.7'de | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §7.2.1` | Müşteri fiziksel kalemi "Kargoya verildi"ye kadar firma onayı beklemeden iptal eder | Cancellation · Order | iptalin yapanı (müşteri); `OrderStatus` | eşlendi |
| `02 §7.2.2` | İptal kalem ve adet düzeyinde, ödeme onayından sonra; önce yalnız bütünüyle; karışık siparişte kalan hatta düşer | Cancellation · Order | kalem ve adet; sipariş düzeyindeki iptal | eşlendi |
| `02 §7.2.3` | Gecikme feshi: otuz günde ayrı kayıt tipi; kişiye özel üretimde de; başka kanaldan kayıt (bildirimin ulaştığı tarih); teslim edilmemiş fiziksel kalemlerin tamamı; kargo dahil geri ödeme; IBAN fesihte; kalem kargoya verilmez, stok döner, dönen mal teslim alma adımıyla; kanuni faiz uyarısı | LateDeliveryTermination · RefundIbanRequest · Return · Refund · Order · veri doğurmaz — türetilir | fesih damgası, kalemler ve adetler, kaynağı (düğme ya da panel kaydı) ve bildirimin tarihi; siparişin firmaya ulaştığı an; uyarı türetilir | eşlendi |
| `02 §7.2.4` | Firma iptali — sipariş, kalem, adet; kapalı sebep listesi, "diğer"de açıklama; teyit; geri alınamaz onayı; Yönetmelik m.16/4; "stokta bulunamadı" uyarısı | Cancellation · AuditLog · *gönderim kaydı* | `CancellationReason` ve açıklaması, adet, yapan, an (B-7) | eşlendi |
| `02 §7.2.5` | Dijital kalem ödeme onayından sonra iptal edilemez; hizmet tamamlandı işaretine kadar; ifa süresi aşımında faiz uyarısı | OrderItem · veri doğurmaz — türetilir | tamamlandı işareti; ifa süresinin bittiği an türetilir (sipariş tarihi + ürünün ifa süresi) | eşlendi |
| `02 §7.2.6` | İptalde stok ve kontenjan adet kadar döner; kupon hakkı yalnız siparişin tamamında | Variant · Coupon · StockReservation · Cancellation | `StockQuantity`; kupon hakkının dönüşü — iptal kayıtlarından (K-791'in park satırı) | eşlendi |
| `02 §7.2.7` | Kısmi iptalde kupon yeniden hesaplanmaz; iptal edilen adetlerin payı geri ödemeye | Refund · OrderItem | ayrılan kupon payı (`02 §7.1.7`) | eşlendi |
| `02 §7.2.8` | İptalin parası geldiği yoldan: kartta sistem kendiliğinden, havalede firma elle; firma iptalinde IBAN isteği düşer; on dört gün; başarısız kart iadesi, "sonuç bekleniyor", firmanın "gerçekleşmedi" işareti (işlem izine), sonradan gelen gerçekleşme uyarı olarak; havale yolu müşterinin seçimi | Refund · *geri ödeme kaydı* · RefundIbanRequest · Order · AuditLog | geri ödemenin yolu ve hâli; `RefundFailedMark`, `RefundOutcomePendingMark`; IBAN girildi mi; firmanın işaretinden sonra gelen gerçekleşme siparişe işlenmez | eşlendi |
| `02 §7.2.9` | İptalde kargo ücreti: fiziksel kalemlerin bütün adetleri kargodan önce iptal edilirse son iptale eklenir; müşteri kaynaklı teslim edilemeyen gönderide gidiş kargosu ödenmez; fesihte her durumda | Refund · Cancellation · Order | geri ödemeye kargo girip girmediği; `CancellationReason`; siparişin donmuş kargo ücreti | eşlendi |

**§7.3–§7.6 — cayma, iade ve geri ödeme, ayıp talebi, geri alma sınırları (34)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §7.3.1` | Cayma penceresi on dört gün, sabit; teslim tarihinden; malı teslim almadan cayma; geç işarette düğme kapanır, önceki beyan geçerli; eksik bilgilendirmede bir yıla uzama — kayıt §10.4.10'la | OrderItem · Withdrawal · *yasal metin sürümü* · *gönderim kaydı* | `DeliveryDate`; beyanın damgası; bilgilendirmenin ispatı — siparişe donan form sürümü ve gönderim kaydı (Z-43); pencere kodda | eşlendi |
| `02 §7.3.2` | Dijitalde cayma yok; üçüncü kutu, kaydı siparişe donar; hak ödeme başarısında düşer | *onay kaydı* · Order | dijital kutunun kaydı; ödeme onayının anı | eşlendi |
| `02 §7.3.3` | Hizmette hak onaylı ifa tamamlanınca düşer; ifanın başladığı an izlenmez; kısmi ifadan cayma tam; pencere sipariş tarihinden ya da fiziksel kalemli siparişte teslimden; düğme ödeme onayından sonra | *onay kaydı* · OrderItem · Order · yoktur — kapsam dışı | hizmet kutusunun kaydı; tamamlandı işareti; sipariş tarihi, `DeliveryDate`; "ifaya başlandı" alanı yok | eşlendi |
| `02 §7.3.4` | Fiziksel üründe cayma istisnası ürün düzleminde isteğe bağlı işaret; kapalı sebep listesi (m.15/1) | Product | istisna işareti, `WithdrawalExemptionReason` | eşlendi |
| `02 §7.3` m1 | (b) kişiye özel hazırlanan mal — mutlak | Product | `WithdrawalExemptionReason` değeri (kod adı VM5) | eşlendi |
| `02 §7.3` m2 | (c) çabuk bozulan ya da son kullanma tarihi geçebilecek mal — mutlak | Product | `WithdrawalExemptionReason` değeri | eşlendi |
| `02 §7.3` m3 | (ç) koruyucu unsuru açılınca sağlık ve hijyen açısından iadesi uygun olmayan — koşullu | Product | `WithdrawalExemptionReason` değeri | eşlendi |
| `02 §7.3` m4 | (e) koruyucu unsuru açılmış maddi ortamdaki kitap, dijital içerik, sarf malzemesi — koşullu | Product | `WithdrawalExemptionReason` değeri | eşlendi |
| `02 §7.3` metin | Mutlakta düğme yok, koşulluda ret firmanın kararı; işaret varyanta inmez, yayın kapısına girmez; işaret, sebep ve koşul sipariş anında kaleme donar; listede olmayan bentte hak tanınır | Product · OrderItem | ürün düzleminde tek işaret; kaleme donan istisna, sebep ve koşul (`02 §3.23.1`) | eşlendi |
| `02 §7.3.5` | Cayma sipariş sayfasından, kalem ve adetle, tarih damgasıyla; ardışık beyan; başka kanaldan firma kaydı; iade adresi beyana yazılır; IBAN beyan sırasında; B-9 | Withdrawal · RefundIbanRequest · *gönderim kaydı* | beyanın damgası, kalem, adet, kaynağı; beyana donan iade adresi; IBAN | eşlendi |
| `02 §7.3.6` | Cayma beyanı siparişi kapatmaz; sevkiyat değişmez, ödeme paranın döndüğü anda | Withdrawal · Order | — (`PaymentStatus` geri ödemeyle değişir) | eşlendi |
| `02 §7.3.7` | Kargoya verilmemiş fiziksel kalemde yol iptaldir; başka kanaldan gelen cayma da iptal olarak işlenir — cayma kaydı açılmaz | Cancellation | `CancellationReason` "müşteriyle anlaşıldı (müşteri talebi)" | eşlendi |
| `02 §7.4.1` | Geri ödemenin on dört günü beyandan ya da ulaşma tarihinden (geç olanı); teslim tarihiyle damganın karşılaştırması; aynı gün iki yol — teslim sırası sorusu, cevap işlem izine; kısmi iadede her ulaşma kendi tarihiyle; panelin iki modu, "iade malı bekleniyor" işareti | Withdrawal · Return · OrderItem · AuditLog · veri doğurmaz — türetilir | beyanın damgası (saatli); ulaşma tarihi; `DeliveryDate` ve teslim işaretinin anı; teslim sırası cevabı; kalan gün ve bekleme işareti türetilir | eşlendi |
| `02 §7.4.2` | Caymada iade kargosu firmaya ait; ayar yok | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §7.4.3` | Ayıplı malda iade kargosu her hâlde firmaya ait | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §7.4.4` | Müşteri malı iade adresine karşı ödemeli gönderir; adres beyan kaydına yazılır; taşıyıcıyı müşteri seçer; gönderme süresi on dört gün; süre dolunca kendiliğinden işlem yok — yalnız IBAN otuz gün sonra silinir | Withdrawal · *firma ayarları* · RefundIbanRequest · yoktur — kapsam dışı | beyana donan iade adresi; IBAN'ın silinme anı (Z-38); taşıyıcı ve iade etiketi alanı yok | eşlendi |
| `02 §7.4.5` | Geri ödeme geldiği yoldan; IBAN biçim denetimi, düzeltme; "havale gerçekleşmedi"; IBAN yalnız o geri ödeme için, gönderilince silinir; malı dönmeyen caymada erken silme; yeniden istek, B-14; "IBAN bekleniyor" sayaca girmez; ayıp, tutar bazlı ve ret sonrası ödemede istek; firma iptalinde ve çıkarmada istek kayıtla düşer | RefundIbanRequest · Refund · AuditLog · *gönderim kaydı* · veri doğurmaz — türetilir | IBAN, isteğin tarihi ve kaynağı (beyan, firmanın isteği, iptal ya da çıkarma); silinme anı ve sebebi; ödeme kaydı ticari kayıt olarak kalır; aynı beyanın IBAN'ını paylaşan kalemler | eşlendi |
| `02 §7.4.6` | Kısmi iadede kargo yeniden hesaplanmaz; gönderilen fiziksel kalemlerin bütün adetlerinden cayılınca son caymaya eklenir | Refund · Order | geri ödemeye kargo girip girmediği; donmuş kargo ücreti | eşlendi |
| `02 §7.4.7` | İade edilen adet stoğa kendiliğinden eklenmez; teslim alma — ulaşma tarihi (geçmişe dönük, ileri ve teslim tarihinden önce olamaz), ulaşan adet; kalan adetler bekler; stoğa ekleme ayrı | Return · Variant | ulaşma tarihi, ulaşan adet, beyan ya da fesih bağı; stoğa eklenen adet | eşlendi |
| `02 §7.4.8` | Caymada kontenjan adet kadar döner; kupon hakkı yalnız siparişin tamamı iade edildiğinde | Variant · Coupon · Withdrawal | kontenjan; kupon hakkının dönüşü kayıtlardan (K-791) | eşlendi |
| `02 §7.4.9` | Koşullu istisnada "iade reddedildi — koruyucu ambalaj açılmış": adetle, ulaşma tarihiyle, işlem izine; geri ödeme yok, IBAN silinir, stoğa girmez, oran değişmez; B-16; ret geri alınmaz; sonradan tutar bazlı geri ödeme | ReturnRejection · OrderItem · RefundIbanRequest · Refund · AuditLog · *gönderim kaydı* | ret kaydı — ulaşma tarihi, adet; kaleme donan istisna sebebi; kanıt alanı yok | eşlendi |
| `02 §7.4.10` | Kullanılmış ya da hasarlı dönen malda tam geri ödeme; değer kaybı ürünün dışında, tutar alanı yok; iç nota yazılabilir | yoktur — kapsam dışı · Order | değer kaybı alanı yok; iç not (`02 §10.4.7`) | eşlendi |
| `02 §7.5.1` | Ayıp talebi sebepli; kanal iki yıllık zamanaşımı boyunca açık; süre teslim işaretinin tarihinden — fiziksel teslim tarihi, hizmet tamamlandı, dijital ödeme onayı | DefectClaim · OrderItem · veri doğurmaz — türetilir | talebin damgası; `DeliveryDate`, tamamlandı işareti, ödeme onayı; kanalın açıklığı türetilir (Z-18) | eşlendi |
| `02 §7.5.2` | "Sorun bildir": kalem, ayıplı adet, açıklama; fotoğraf yok; kanal kargoya verilince, dijitalde ödeme onayında, hizmette tamamlandıda açılır | DefectClaim | kalem, ayıplı adet, açıklama; dosya alanı yok | eşlendi |
| `02 §7.5.3` | Sistem talebi kaydeder, çözümü yürütmez; Açık ve Çözüldü; yeniden açılır; sözleşmeden dönme iade hattıyla, bedel indirimi tutar bazlı geri ödemeyle; IBAN isteği | DefectClaim · Refund · Return · RefundIbanRequest | `DefectClaimStatus`; geri ödemenin ayıp bağı; seçimlik hak alanı yok | eşlendi |
| `02 §7.5.4` | Ayıplı dijitalde dosya güncellenir, eski alıcılara ulaşır | *dosya kaydı* · Variant | varyantın o anki dosyası | eşlendi |
| `02 §7.5` metin | Garanti bilgisinin yeri §3.11.7 | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §7.6.1` | Panel "geri al" sunmaz; dört geçiş ve öteki işlemler geri alınamaz onayı ister; düzeltme sebep seçilen yeni geçişle, iz temizlenmez | *geçiş kaydı* · AuditLog · veri doğurmaz — arayüz (`04`) | düzeltme geçişi ve `CorrectionReason`; onayın kendisi saklanmaz | eşlendi |
| `02 §7.6.2` | Süresi dolup iptal edilmiş siparişe gelen havale işlenemez | veri doğurmaz — ürünün dışında | — (havale hattının geç parası ürün dışı — `02 §6.2.16`) | veri doğurmaz |
| `02 §7.6.3` | Müşteri akışında iptal ya da teslim edilmiş sipariş yeniden açılmaz; yöneticinin düzeltmesi §10.4.6'da | veri doğurmaz — kod | — (geçiş kuralı kodda; düzeltme `02 §10.4.6`) | veri doğurmaz |
| `02 §7.6.4` | Kurumsal içerikte sürüm geçmişi yok, silme geri alınmaz | yoktur — kapsam dışı · Content | sürüm geçmişi tablosu yok; silme fiziksel | eşlendi |
| `02 §7.6.5` | Silinmiş hesabın siparişleri hiçbir hesaba geri bağlanmaz | Order | hesap bağı silmede kalıcı olarak kalkar | eşlendi |
| `02 §7.6.6` | Yöneticinin düzeltmeleri ileri yönlü ve tutarı değiştirmez: adres, adet ya da çıkarma, takip, teslim tarihi, durum; kalem eklenemez, fiyat değişmez | Order · *çıkarma kaydı* · *geçiş kaydı* · AuditLog | onaylanan tutar değişmez — donan alanlar; düzeltmelerin izi | eşlendi |
| `02 §7.6.7` | Cayma beyanı geri alınmaz ve düzeltilmez; kayıt kimin yaptığıyla işlem izinde | Withdrawal · AuditLog | beyanda düzeltme alanı yok; kaydı yapan (müşteri ya da yönetici) | eşlendi |

**§8 — kötüye kullanım ve güvenlik kuralları (40)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §8` metin | Ürün düzeyindeki önlemler; teknik güvenlik `05`'te; eşikler §11'de, süreler §4'te | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §8.1` metin | Kuralların tam metni §3.13'te, burada güvenlik açısından toplanır | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §8.1.1` | Doğrulanmamış hesapla giriş ve sipariş yok; kayıt bağlantının ömrü dolunca silinir | Customer · *bağlantı kaydı* | doğrulanmış mı; ilk bağlantının ömrü (Z-1) | eşlendi |
| `02 §8.1.2` | Sipariş ya da giriş yolunun hesaba bağlanmasının tek dayanağı doğrulanmış e-posta; misafir siparişi hesaba düşer; e-posta düzeltmesinde bağ kesilir | Order · Customer · SocialLogin | siparişin hesap bağı; Google bağı yalnız doğrulanmış e-postayla | eşlendi |
| `02 §8.1.3` | Bağlantılar süreli ve tek kullanımlık; şifre sıfırlanınca oturumlar kapanır; bağlantının açılması işlem yapmaz — ekrandaki düğme | *bağlantı kaydı* · Session · AdminInvitation | ömür, kullanıldığı an; oturumların silinmesi | eşlendi |
| `02 §8.1.4` | Ekranlar hesabın varlığını kayıtta, ödeme hatırlatmasında ve e-posta değişikliğinde ele verir; L-3 daraltır | veri doğurmaz — arayüz (`04`) | — | veri doğurmaz |
| `02 §8.1.5` | Üç işlem yeniden doğrulama ister; şifresiz hesapta Google ile; yeni e-posta doğrulanmadan geçersiz; eski adrese geri alma bağlantısı | Session · *bekleyen e-posta değişikliği* · *bağlantı kaydı* | yeniden doğrulamanın tek işlemlik izni (K-899); bekleyen adres; geri alma bağlantısının ömrü | eşlendi |
| `02 §8.1.6` | Şifre politikası uzunluk tabanlı, yaygın şifre listesi ürünle gelir; Google hesabına şifre "şifremi unuttum"la | veri doğurmaz — kod · Customer | şifre özeti (boş olabilir) | eşlendi |
| `02 §8.1.7` | Yönetici tarafı aynı rejimde; iki adımlı doğrulama yok; kaldırılan yöneticinin oturumları anında sonlanır; panel yalnız yönetici hesaplarını arar, Google yok | Admin · Session · yoktur — kapsam dışı | şifre özeti; oturumun hesap türü (`AccountType`); iki adımlı doğrulama alanı yok | eşlendi |
| `02 §8.2` metin | Dokuz limit; eşik ve pencere §11'de; kayan pencere; L-8 aynı anda açık kayıt sayar | *deneme kaydı* · veri doğurmaz — türetilir | deneme satırı ve anı (Z-27); L-8 siparişlerden sayılır | eşlendi |
| `02 §8.2` L-1 | Başarısız şifreli giriş ve yeniden doğrulama; IP ve e-posta ekseni; tanınan tarayıcının denemeleri hesap ve tarayıcı başına kendi sayacında; Google girmez | *deneme kaydı* · RecognizedBrowserMark | deneme satırı — limit, eksen (IP, e-posta, hesap + tarayıcı), değer, an | eşlendi |
| `02 §8.2` L-2 | Şifre sıfırlama talebi; IP ve e-posta ekseni | *deneme kaydı* | deneme satırı | eşlendi |
| `02 §8.2` L-3 | Hesap kaydı; IP ve e-posta ekseni | *deneme kaydı* | deneme satırı | eşlendi |
| `02 §8.2` L-4 | İletişim formu gönderimi; IP ekseni | *deneme kaydı* | deneme satırı | eşlendi |
| `02 §8.2` L-5 | Başarısız misafir sipariş sorgusu; IP ve sorguya yazılan e-posta ekseni | *deneme kaydı* | deneme satırı | eşlendi |
| `02 §8.2` L-6 | Geçersiz kupon kodu; sepet ekseni; birleşen sepet iki sayacın büyüğünü taşır | *deneme kaydı* · Cart | deneme satırı — sepet ekseni; birleşmede sayacın taşınması (mekanizma `05`) | eşlendi |
| `02 §8.2` L-7 | Kendiliğinden iptal edilen ödenmemiş sipariş; ödeme sayfası açılmamış sipariş sayılmaz; e-posta ekseni yalnız üyenin siparişleri | *deneme kaydı* · Order · *ödeme kaydı* | deneme satırı — iptal anında; siparişin IP'si, hesap bağı; ödeme sayfasının açılıp açılmadığı | eşlendi |
| `02 §8.2` L-8 | Aynı anda açık ödenmemiş sipariş tavanı; IP ve e-posta (yalnız üye); yalnız havale yolunu kapatır | veri doğurmaz — türetilir · Order | açık siparişlerden sayılır — `PaymentStatus` = `Pending`, siparişin IP'si, hesap bağı | eşlendi |
| `02 §8.2` L-9 | Doğrulama bağlantısının yeniden istenmesi; müşteri ve yönetici; IP ve e-posta; kayıt silinmez; yeni davet limite girmez; bekleyen e-posta değişikliğinin yerine geçen istek sayılır | *deneme kaydı* · *bağlantı kaydı* · *bekleyen e-posta değişikliği* | deneme satırı; önceki bağlantı ömrü içinde geçerli | eşlendi |
| `02 §8.2.1` | Limit aşılınca yalnız o işlem geçici engellenir, kendiliğinden açılır; hesap kilitlenmez; nötr mesaj | *deneme kaydı* · yoktur — kapsam dışı | hesapta kilit alanı yok | eşlendi |
| `02 §8.2.2` | İki eksen ayrı sayılır; kupon sepet ekseninde; L-5'te e-posta sorguya yazılan adres; sipariş numarası ekseni yok; L-7 ve L-8'de e-posta yalnız üyenin siparişleri | *deneme kaydı* · Order | eksen ve değer; siparişin hesap bağı ve IP'si | eşlendi |
| `02 §8.2.3` | L-7 tekrarı ölçer; sağlayıcıya bağlanılamayan sipariş sayılmaz; L-8 eşzamanlı kilidi durdurur; IP değiştiren saldırganda firmanın yolu panel | *deneme kaydı* · Order · *ödeme kaydı* | — (L-7 ve L-8'in satırlarıyla) | eşlendi |
| `02 §8.2.4` | Yönetici de aynı limitlere tabi | *deneme kaydı* | — (eksen e-posta; hesap türünden bağımsız) | eşlendi |
| `02 §8.2.5` | Sepet ve stok tarafında ürün düzeyinde limit yok; genel istek hızı sınırı `05`'te | veri doğurmaz — mimari (`05`) | — | veri doğurmaz |
| `02 §8.2.6` | Girişin e-posta ekseni tanınan tarayıcıyı engellemez: başarılı girişte ve sıfırlama bağlantısıyla şifre yenilemede işaret; ömrü son başarılı girişten P-48 (Z-46); şifre değişiminde, geri almada ve hesap silmede işaretler geçersizleşir | RecognizedBrowserMark · *deneme kaydı* | hesap, tarayıcı, son başarılı giriş anı; toplu geçersizleştirme (işlemi yapan tarayıcınınki hariç) | eşlendi |
| `02 §8.3.1` | İletişim formu captcha kullanmaz; tuzak alan ve L-4 | *deneme kaydı* | — (tuzağa düşen gönderim kayıt doğurmaz) | eşlendi |
| `02 §8.3.2` | Siteye kod eklenmez | yoktur — kapsam dışı | betik ya da etiket alanı yok | veri doğurmaz |
| `02 §8.3.3` | Üçüncü taraf gömme içerik yok; çerez envanteri zorunlu ve birinci taraf | yoktur — kapsam dışı · veri doğurmaz — mimari (`05`) | gömme alanı yok; çerezler altyapıda | veri doğurmaz |
| `02 §8.3.4` | Ürün kart verisini görmez, saklamaz, loglamaz; 3D Secure | yoktur — kapsam dışı · veri doğurmaz — ürünün dışında | kart verisi alanı yok | veri doğurmaz |
| `02 §8.3.5` | Sipariş erişimi tahmin edilemez anahtarlara dayanır: numara ve ayrı erişim anahtarı; L-5 | Order · *deneme kaydı* | `OrderNumber` (tahmin edilemez), `OrderAccessKey` | eşlendi |
| `02 §8.3.6` | Misafir e-postası yalnız firmanın teyidiyle değişir: eski anahtar geçersiz, B-15, işlem izine; eski ve yeni adres e-posta geçmişinde siparişin kişisel verileriyle | OrderEmailHistory · Order · AuditLog · *gönderim kaydı* | eski ve yeni değer; `OrderAccessKey` yenilenir; imhası siparişle | eşlendi |
| `02 §8.3.7` | Siparişin adresi yalnız firmanın teyidiyle "Kargoya verildi"ye kadar düzeltilir; B-10, işlem izine | Order · AuditLog · *gönderim kaydı* | `ShippingAddress`; iz satırı adres değerini taşımaz (`02 §10.3.1`) — eski adresin yeri VA-2 | eşlendi |
| `02 §8.3.8` | Başka kanaldan cayma yalnız teyitle işlenir; tarih bildirimin ulaştığı tarih; IBAN yalnız iletişim e-postasından gelen bildirimden aktarılır | Withdrawal · RefundIbanRequest | bildirimin ulaştığı tarih; IBAN'ın kaynağı; teyit kaydedilmez | eşlendi |
| `02 §8.4.1` | Yalnız işlenen kişisel veri: pazarlama onayı, açık rıza, doğum tarihi, ziyaretçi ölçümü, dosya eki yok | yoktur — kapsam dışı | bu alanların hiçbiri açılmaz | veri doğurmaz |
| `02 §8.4.2` | IBAN yalnız gerektiğinde, iş bitince silinir; malı dönmeyen caymada erken silme; tek giriş yolu müşterinin beyanı, tek istisna iletişim e-postasından gelen bildirim | RefundIbanRequest | IBAN, kaynağı, silinme anı; hesapta IBAN alanı yok | eşlendi |
| `02 §8.4.3` | Serbest metin alanları risk taşır: iç not siparişle imha edilir; müşteri notu alanı yok | Order · yoktur — kapsam dışı | iç not — imhası siparişle; müşteri notu alanı yok | eşlendi |
| `02 §8.4.4` | Çerezler yalnız zorunlu; onay bandı yok; misafir sepetinin çerezi zorunlu | veri doğurmaz — mimari (`05`) · Cart | misafir sepetinin tarayıcı anahtarı (`02 §3.16.1`); çerez altyapıda | eşlendi |
| `02 §8.4.5` | Saklama süreleri §4'te, imha ve yükümlülükler §12'de | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §8.5.1` | Ayrı ihlal kaydı yok; kapsam işlem izi, giriş kaydı (altı ay, panelde görünmez) ve sistem kayıtlarından; limit sayaçları kaynak değil | AuditLog · *giriş kaydı* · veri doğurmaz — mimari (`05`) · yoktur — kapsam dışı | giriş kaydının an, IP, hesap, sonucu (Z-40); ihlal kaydı tablosu yok | eşlendi |
| `02 §8.5.2` | İşlem izi on yıl değiştirilemez ve silinemez; kaldırılan yöneticinin satırları adıyla kalır; süre dolunca imha | AuditLog | satırın yapanı adıyla — yönetici silinse de; değiştirilemezlik; oluştuğu an (Z-30) | eşlendi |

**§9 — bildirimler (41)**

Bildirim satırları (B, F) gönderim kaydına eşlenir — her e-postanın gönderim satırı (K-892); e-postanın içeriği Entegrasyon Spesifikasyonu'nun işidir. "Alan" hücresi e-postanın taşıdığı ve kayıtta durması gereken değeri gösterir; `03 §7`'nin bildirim haritası bu satırlarla toplu eşlenir (1c; K-997).

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §9.1.1` | Tek kanal e-posta; SMS, anlık bildirim ve bildirim merkezi yok | yoktur — kapsam dışı · *gönderim kaydı* | kanal alanı yok — tek kanal | eşlendi |
| `02 §9.1.2` | Akış e-postaya bağlı değil; hesap ve yönetim bağlantıları dışında | veri doğurmaz — kod | — (`02 §6.1.1`'in satırıyla) | veri doğurmaz |
| `02 §9.1.3` | İşlem bildirimleri ticari ileti değil, kapatılamaz; tercih ekranı yok | yoktur — kapsam dışı | bildirim tercihi ve İYS alanı yok | veri doğurmaz |
| `02 §9.1.4` | Gönderen adı marka adı ya da alan adı; gönderen adresi kurulumdan; yanıt iletişim e-postasına, sisteme girmez; logo ve marka rengi | Branding · Company · veri doğurmaz — kurulum | `BrandName`, `Logo`, `BrandColor`; iletişim e-postası (yanıt adresi); gönderen adresi kurulumda | eşlendi |
| `02 §9.1.5` | Çalışan e-posta yapılandırması dış ön koşuldur | veri doğurmaz — kurulum | — | veri doğurmaz |
| `02 §9.1.6` | Üç yeniden deneme; müşteri bildiriminde "e-posta ulaşmadı" işareti (B-9'da talep satırında da), işlem izine yazılmaz; yeniden gönderim ilk alıcısına, B-1 donmuş sürümle; firma bildiriminde işaret yok, "firma bildirimleri ulaşmıyor" uyarısı — sonraki bildirim ulaşınca ya da iletişim e-postası değişince kalkar; kanal uyarısı; saklama Z-43 ve Z-34 | *gönderim kaydı* · *kanal hâli kaydı* · DefectClaim · Order · Company | gönderimin alıcısı, deneme sayısı, sonucu, sipariş ya da talep bağı; işaretler gönderim kaydından türer (K-913); firma bildirimlerinin ulaşma hâli — iletişim e-postasının değiştiği an; gönderim anı (Z-43, Z-34) | eşlendi |
| `02 §9.2` B-1 | Sipariş alındı — e-posta düzeltmesinde yeni adrese yeniden; numara, bağlantı, donan sürümlerle form ve sözleşme tam metin | *gönderim kaydı* · Order · *yasal metin sürümü* | gönderim satırı; `OrderNumber`, `OrderAccessKey`; siparişe donan sürümler | eşlendi |
| `02 §9.2` B-2 | Havale/EFT seçildi — donan IBAN ve numara | *gönderim kaydı* · Order | siparişe donan IBAN | eşlendi |
| `02 §9.2` B-3 | Havale ödeme hatırlatması — son iş gününün başında bir kez; düzeltmeyle yeniden başlayan sürede bir kez daha | *gönderim kaydı* · Order | ödeme süresinin bittiği an; hatırlatmanın gönderildiği — sürede bir kez (gönderim kaydından okunur) | eşlendi |
| `02 §9.2` B-4 | Ödeme onaylandı — dijitalde indirmenin hazır olduğu | *gönderim kaydı* | gönderim satırı | eşlendi |
| `02 §9.2` B-5 | Kargoya verildi — kargo şirketi, takip numarası, "Takip et" ya da kendi aracı | *gönderim kaydı* · Order | kargo şirketi, takip numarası, kendi aracıyla teslim beyanı | eşlendi |
| `02 §9.2` B-6 | Teslim edilemedi | *gönderim kaydı* | gönderim satırı | eşlendi |
| `02 §9.2` B-7 | İptal edildi — sipariş ya da kalem; sebep, kalem ve adet; m.16/4 bildirimi; kapanışlarda gitmez | *gönderim kaydı* · Cancellation | `CancellationReason`, kalem, adet | eşlendi |
| `02 §9.2` B-8 | Geri ödeme işlendi — geç gelen ödemenin geri ödemesi dahil | *gönderim kaydı* · Refund | geri ödemenin tutarı ve anı | eşlendi |
| `02 §9.2` B-9 | Cayma beyanı, gecikme feshi ya da ayıp talebi alındı — panel kaydı ve yeniden açma dahil; tarih damgası, kalemler ve adetler, iade adresi | *gönderim kaydı* · Withdrawal · LateDeliveryTermination · DefectClaim | damga, kalem, adet; beyana donan iade adresi | eşlendi |
| `02 §9.2` B-10 | Teslimat ya da fatura adresi düzeltildi | *gönderim kaydı* · Order | düzeltilen adres | eşlendi |
| `02 §9.2` B-11 | Kalem çıkarıldı ya da adedi azaltıldı | *gönderim kaydı* · *çıkarma kaydı* | değişen kalem ve adet | eşlendi |
| `02 §9.2` B-12 | Kargo şirketi ya da takip numarası düzeltildi | *gönderim kaydı* · Order | düzeltilmiş takip bilgisi | eşlendi |
| `02 §9.2` B-13 | Yanlış durum geçişi düzeltildi — havale düzeltmesinde donan havale bilgisi ve yeni son ödeme günü | *gönderim kaydı* · *geçiş kaydı* · Order | düzeltilmiş durum; yeni ödeme süresinin sonu | eşlendi |
| `02 §9.2` B-14 | IBAN'ın sipariş sayfasından girilmesi istenir — altı tetik: IBAN silindikten sonra ulaşan mal, başarısız kart iadesi, gerçekleşmeyen havale, IBAN'sız panel kaydı, firmanın IBAN isteği, havale hattında firma iptali ya da çıkarma | *gönderim kaydı* · RefundIbanRequest | isteğin tetiği ve tarihi | eşlendi |
| `02 §9.2` B-15 | Misafir e-postası düzeltildi — eski adrese; numara ve tarih; yeni adres ve bağlantı yok | *gönderim kaydı* · OrderEmailHistory | eski adres, değişikliğin tarihi | eşlendi |
| `02 §9.2` B-16 | İade reddedildi — kalem, adet, sebep, geri ödeme yok, geri gönderim, uyuşmazlık yolları | *gönderim kaydı* · ReturnRejection | reddedilen kalem ve adet | eşlendi |
| `02 §9.2` metin | Bildirim üretmeyen olaylar: teslim işareti, Hazırlanıyor, teslim tarihi düzeltmesi ve iç not, teslim alma (B-14 dışında), "mal dönmedi", IBAN silinmesi, ayıbın Çözüldü işareti ve firmanın yeniden açması, kart iadesinin başarısızlığı, son sorgu, indirme hakkının yenilenmesi, dosya güncellemesi, IBAN girişi, ulaşma tarihi düzeltmesi | veri doğurmaz — kod | — (bu olaylar gönderim satırı doğurmaz) | veri doğurmaz |
| `02 §9.3` F-1 | Yeni sipariş | *gönderim kaydı* | gönderim satırı — firmanın iletişim e-postasına | eşlendi |
| `02 §9.3` F-2 | Yeni iletişim talebi | *gönderim kaydı* · ContactRequest | gönderim satırı | eşlendi |
| `02 §9.3` F-3 | Yeni cayma beyanı, gecikme feshi ya da ayıp talebi — yeniden açma dahil | *gönderim kaydı* | gönderim satırı | eşlendi |
| `02 §9.3` F-4 | Sistemin kendiliğinden yaptığı karta geri ödeme — geç ödeme, iptal, fesih, çıkarma | *gönderim kaydı* · Refund | gönderim satırı; geri ödemenin kendiliğinden olduğu | eşlendi |
| `02 §9.3` F-5 | Havale IBAN'ı girildi ya da değişti — bütün yöneticilerin kendi adreslerine | *gönderim kaydı* · Admin · AuditLog | yöneticinin e-postası; değişikliği yapan, an, yeni IBAN'ın son dört hanesi (izden) | eşlendi |
| `02 §9.3` F-6 | Yönetici daveti gönderildi ya da yönetici kaldırıldı — bütün yöneticilere | *gönderim kaydı* · Admin · AdminInvitation | yöneticinin e-postası; işlem, yapan, an, davet edilen adres ya da kaldırılan yönetici | eşlendi |
| `02 §9.3.1` | Firma bildirimleri iletişim e-postasına; yöneticilere ve ayrı bildirim adresine gitmez; ulaşmayınca uyarı; iki istisna | Company · *gönderim kaydı* | iletişim e-postası; bildirim adresi alanı yok | eşlendi |
| `02 §9.3.2` | Firmanın kendi işlemi kendisine bildirilmez; bekleyen işler sayaçla | veri doğurmaz — türetilir | sayaçlar saklanmaz (K-675) | veri doğurmaz |
| `02 §9.3.3` | IBAN değişikliği bütün yöneticilere: an, yapan, son dört hane; geri alma yok; işlem izi eski ve yeni değerle; işaret düşmez | *gönderim kaydı* · AuditLog · Admin · *firma ayarları* | havale IBAN'ı; izde eski ve yeni değer, yapan, an | eşlendi |
| `02 §9.3.4` | Davet ve kaldırma bütün yöneticilere; geri çekme ve süre dolması bildirim üretmez | *gönderim kaydı* · AdminInvitation · Admin | davetin ve kaldırmanın yapanı ve anı | eşlendi |
| `02 §9.4` metin | Hesap ve yönetim e-postaları: doğrulama, sıfırlama, yeni e-posta doğrulaması, eski adrese bildirim, yönetici daveti (işaret davet satırına), hesabın silindiği bildirimi; üç deneme, işaret yok; bildirim üretmeyen hesap olayları | *gönderim kaydı* · AdminInvitation · *bağlantı kaydı* | gönderim satırı; davetin "e-posta ulaşmadı" işareti; hesabın silindiği bildirimi silme anında | eşlendi |
| `02 §9.5` m1 | Pazarlama iletisi yok; İYS bağlantısı ve pazarlama onayı yok | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §9.5` m2 | Terk edilmiş sepet hatırlatması ve "stokta haber ver" yok | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §9.5` m3 | Yasal metin değişince bildirim yok | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §9.5` m4 | Veri ihlalinde toplu bildirim yok | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §9.5` m5 | Duyuru şeridi gönderilmez | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §9.5` m6 | İletişim formuna alındı e-postası yok; talebin firmaya ulaştığı an talebin kaydında | ContactRequest | oluşma anı | eşlendi |
| `02 §9.5` m7 | Müşterinin kendi iptali firmaya bildirilmez; sayaç ve F-4 | veri doğurmaz — türetilir | — | veri doğurmaz |

**§10.1–§10.3 — panelin kapsamı, yönetici hesapları, işlem izi (30)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §10` metin | Tek rol, çoklu kullanıcı; bölümün içeriği; kurallar kendi bölümlerinde, tabloda işaretlenir | veri doğurmaz — yazım konvansiyonu · Admin | yönetici hesabında rol alanı yok (`02 §1.3`) | eşlendi |
| `02 §10.1.1` | Panel tek firmayı yönetir; firma seçimi ve çok kiracılık yok; operatör paneli yok; kurtarma kurulum tarafında | Company · yoktur — kapsam dışı | tekil kayıt — kiracı alanı yok (SK-1, K-10) | eşlendi |
| `02 §10.1.2` | Yöneticinin yapabildikleri ve yapamadıkları — alan tablosu | veri doğurmaz — yazım konvansiyonu | — (satırlar maddeleriyle) | veri doğurmaz |
| `02 §10.1` m1 | Firma kimliği ve satış: kimlik alanları, KEP, tip, meslek odası, işletme adı ya da marka, davranış kuralları, ETBİS; satışı geçici kapatma — silinmez, bakım modu yok | Company · *firma ayarları* | kimlik alanları; geçici kapatma anahtarı | eşlendi |
| `02 §10.1` m2 | Ödeme yöntemleri: havaleyi açar ve kapatır, IBAN girer; kart kurulumdan | *firma ayarları* · veri doğurmaz — kurulum | havalenin açıklığı, IBAN | eşlendi |
| `02 §10.1` m3 | Kargo, teslimat ve ödeme süreleri: kargo ücreti, eşik, asgari tutar, teslimat illeri, kargoya verme ve havale süresi, iade adresi; kargo KDV'si girilmez | *firma ayarları* · *il listesi* | P-1, P-3…P-7'nin değerleri; teslimat illeri; iade adresi | eşlendi |
| `02 §10.1` m4 | Katalog: ad ve yayın durumuyla arama; ürün, varyant, stok, fiyat, KDV, indirim, kupon, kategori, görsel, dosya; ifa süresi, üretim yeri, ölçü birimi; kalıcı silme; iade malını stoğa ekleme — elle sıralama ve toplu işlem yok | Product · Variant · Discount · Coupon · Category · *görsel* · *dosya kaydı* · yoktur — kapsam dışı | sıra alanı yok — kategoriler alfabetik | eşlendi |
| `02 §10.1` m5 | Kurumsal içerik ve site: içerik, ana sayfa düzeni, menü adları, duyuru, sosyal bağlantılar — sayfa kurucu ve kod yok; Hakkımızda silinmez | Content · AboutPage · Announcement · SocialLink · *firma ayarları* | `HomepageLayout`, menü adları | eşlendi |
| `02 §10.1` m6 | Marka: logo, marka adı, site simgesi, marka rengi; platform imzası | Branding · *firma ayarları* | `Logo`, `BrandName`, `Favicon`, `BrandColor`; platform imzası anahtarı | eşlendi |
| `02 §10.1` m7 | Yasal metinler: aydınlatma ve çerez politikasını düzenler ve yayına alır; form ve sözleşme ayarlardan; yayındaki metin yayından çekilmez | PrivacyNotice · CookiePolicy · *yasal metin sürümü* | sürümler — yayındaki sürüm geri çekilmez | eşlendi |
| `02 §10.1` m8 | Siparişler: numara ve e-postayla arama, durum süzgeci; bütün sipariş işlemleri ve müdahaleler — elle sipariş, tutar, kalem ekleme yok; IBAN girilmez (başka kanal istisnası); ileri tarihli ulaşma ve bildirim tarihi yok | Order · OrderItem · Cancellation · Return · ReturnRejection · Refund · *geri ödeme kaydı* · RefundIbanRequest · *gönderim kaydı* | `OrderNumber`, iletişim e-postası (aranır), durumlar; yapamadıkları kodda | eşlendi |
| `02 §10.1` m9 | Talepler: iletişim taleplerini listeler, kapatır, açar; ayıbı çözer ve açar — panelden yanıt yok | ContactRequest · DefectClaim | durumlar; yanıt alanı yok | eşlendi |
| `02 §10.1` m10 | Üye hesapları: e-postayla arama; ad, adres defteri, giriş yolları, bağlı siparişler; talep üzerine silme — salt okunur | Customer · AddressBook · SocialLogin · Order · AuditLog | hesap, giriş yolları, sipariş bağı; silme izi (`02 §10.3.1` (8)) | eşlendi |
| `02 §10.1` m11 | Yönetici hesapları: davet, geri çekme, kaldırma — son yönetici ve kendisi kaldırılamaz; yönetici adresine davet yok | Admin · AdminInvitation | davetin hâli; yönetici sayısı türetilir | eşlendi |
| `02 §10.1` m12 | İz, özet ve dışa aktarma: izi ve özeti görür; siparişleri, üyeleri ve talepleri dışa aktarır — izi düzeltemez, izi ve özeti dışa aktaramaz | AuditLog · veri doğurmaz — türetilir | dışa aktarma izi; özet saklanmaz | eşlendi |
| `02 §10.1.3` | Parametreler üç katmanda: firma ayarı, kurulum ayarı, ürün sabiti; atama ölçütü; firma ayarları kurulumda dolu gelir | *firma ayarları* · veri doğurmaz — kurulum · veri doğurmaz — kod | firma ayarları varsayılanlarıyla — başlangıç verisi (VM8) | eşlendi |
| `02 §10.1.4` | Panel mobilde kapatılmaz; eşzamanlı düzenlemede sessiz üzerine yazma yok; siparişte önce tamamlanan işlem | veri doğurmaz — arayüz (`04`) · veri doğurmaz — mimari (`05`) | karşılaştırmanın veri karşılığı K-800'ün park satırı (VM2) | veri doğurmaz |
| `02 §10.1.5` | Firmaya giden bildirimler §9.3'te | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §10.2.1` | İlk yönetici kurulumda, sonrakiler davetle; davetli adını yazar ve şifresini kurar; ad zorunlu | Admin · AdminInvitation · *bağlantı kaydı* | yöneticinin adı (K-736'nın park satırı), e-postası, şifre özeti; davetin adresi ve ömrü | eşlendi |
| `02 §10.2.2` | Yönetici kaldırılır; son yönetici ve kendi hesabı değil; oturumlar anında sonlanır; iz satırları adıyla ve e-postasıyla kalır; pasifleştirme yok; gönderdiği davetler geçersizleşir; F-6 | Admin · Session · AuditLog · AdminInvitation | iz satırında yöneticinin adı ve e-postası — hesap silinse de okunur; davetin gönderen yöneticisi; aktiflik alanı yok | eşlendi |
| `02 §10.2.3` | Bütün yöneticiler aynı yetkide; rol ve veri kısıtı yok | yoktur — kapsam dışı | yetki matrisi ve rol tablosu yok | veri doğurmaz |
| `02 §10.2.4` | Yönetici kimlik doğrulaması müşteriyle aynı; e-posta değişikliği işlem izine, iletişim e-postasını değiştirmez; ad hesabından değişir, yeniden doğrulama istemez, müşteriye görünmez | Admin · AuditLog · *bekleyen e-posta değişikliği* | yöneticinin adı ve e-postası; değişiklik izi | eşlendi |
| `02 §10.2.5` | Yöneticinin sepeti ve siparişi yok; ayrı hesap türleri, ayrı kapılar; e-posta hesap türü içinde tekil, büyük-küçük harf ayrımsız karşılaştırılır, girildiği gibi kalır | Admin · Customer · Session | e-posta — hesap türü içinde tekil, harf ayrımsız (K-982); `AccountType` | eşlendi |
| `02 §10.2.6` | Kullanılmamış davet geri çekilir; bir adres için tek geçerli davet; yönetici adresine davet yok; gönderme ve geri çekme işlem izine | AdminInvitation · AuditLog | davetin hâli (geçerli, kullanıldı, geri çekildi, geçersizleşti), adresi, gönderim anı (Z-3) | eşlendi |
| `02 §10.2.7` | Panele erişimin kaybında kurtarma kurulum tarafında; kurulum işlemi işlem izine yazılmaz | veri doğurmaz — kurulum · yoktur — kapsam dışı | kurtarma kodu alanı yok | veri doğurmaz |
| `02 §10.3.1` | İz sekiz alanda: kimlik ve marka; yayın durumu; siparişe yönelik yönetici işlemleri (teslim sırası cevabı, IBAN isteğinin tarihi dahil) — kişisel veri değer olarak girmez, adres düzeltmesinde sipariş, alan ve işlem, iç notun metni girmez; yönetici hesapları; yasal metin sürümü; her ayar eski ve yeni değerle; dışa aktarma; üye silme — işlem ve tarih; her satır kim, ne zaman, ne değişti; fiyat, stok ve içerik metni ize girmez | AuditLog | yapan yönetici (adı, e-postası), an, alan (sekizden biri), işlemin adı, ilgili kayıt (sipariş, ayar, hesap), ayarlarda ve tarihlerde eski ve yeni değer; sipariş satırında kişisel veri değeri yok | eşlendi |
| `02 §10.3.2` | İz değiştirilemez ve silinemez; düzeltme yeni satır | AuditLog | yalnız ekleme — güncelleme ve silme yok (`05`'in mekanizması) | eşlendi |
| `02 §10.3.3` | Sistem olayları ize yazılmaz: "e-posta ulaşmadı" satırında, imha imha kaydında (tarih, tür, sayı; en az üç yıl, Z-44), giriş denemeleri giriş kaydında | *gönderim kaydı* · *imha kaydı* · *giriş kaydı* | imha çalışmasının tarihi, kayıt türü, sayısı — kişisel veri yok | eşlendi |
| `02 §10.3.4` | İz panelde düz liste; tarih ve yönetici süzgeci | veri doğurmaz — arayüz (`04`) · AuditLog | süzgeç alanları: an, yapan | eşlendi |
| `02 §10.3.5` | İz on yıl saklanır; kaldırılan yöneticinin satırları adıyla | AuditLog | oluştuğu an (Z-30) | eşlendi |

**§10.4 — sipariş müdahaleleri (21)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §10.4.1` | Dokuz müdahale: adres, adet ya da çıkarma, takip, teslim ya da ulaşma tarihi, geçiş düzeltmesi, iç not, "mal dönmedi", başka kanal kaydı, misafir e-postası; her biri işlem izine; elle sipariş yok | AuditLog · Order · *çıkarma kaydı* · *geçiş kaydı* · NotReturnedClosure · Withdrawal · LateDeliveryTermination · OrderEmailHistory | müdahalenin iz satırı; elle sipariş alanı yok | eşlendi |
| `02 §10.4.2` | Adres yalnız "Kargoya verildi"ye kadar düzeltilir (fiziksel kalemsizde Teslim edildi'ye kadar); fatura kesildiyse firmanın aracı; düzeltme bildirilir | Order · AuditLog | `ShippingAddress`, `BillingAddress` yerinde düzeltilir; iz satırı adres değerini taşımaz (`02 §10.3.1`) — eski adresin tutulup tutulmadığı yazılı değil ve `02 §3.23.3`'ün "tek istisna"sı yalnız e-postayı sayar | GAP adayı (VA-2) |
| `02 §10.4` m1 | Kimin talebiyle: teslimat telefonu dahil, müşterinin talebi; siparişin kendi kanalına dönerek teyit; ispat firmada | Order · veri doğurmaz — ürünün dışında | teslimat telefonu; teyit kaydedilmez | eşlendi |
| `02 §10.4` m2 | Bildirim: B-10 düzeltilen adresle; işlem izine | *gönderim kaydı* · AuditLog | gönderim satırı; iz satırı | eşlendi |
| `02 §10.4` m3 | Kalan risk: kandırılan firma adresi çevirir; B-10 uyarır; paket yola çıktıysa düzeltilemez | veri doğurmaz — ürünün dışında | — | veri doğurmaz |
| `02 §10.4` m4 | Elenen yol: müşterinin sipariş sayfasından onayı — ara adım ve süre sorunu | yoktur — kapsam dışı | onay bekleyen ara durum yok | veri doğurmaz |
| `02 §10.4.3` | Kalem adedi azaltılır ya da kalem çıkarılır; kalem eklenmez, fiyat değişmez; parası §7.2.8'den, tutarı §7.1.7'den; havalede IBAN isteği kayıtla düşer; sayaçlar iptalin rejimiyle; sebep listesi yok; durum makinesinde iptal gibi; teslim işaretli kalem çıkarılmaz; kartta geri alınamaz onay; kalem kaydına düşer; fiziksel kalem kargoya verildikten sonra çıkarılamaz | *çıkarma kaydı* · OrderItem · Refund · RefundIbanRequest · AuditLog · *gönderim kaydı* | çıkarılan adet, an, yapan yönetici, ayrılan kupon payı ve KDV; sebep alanı yok — sözlükte terimi yok | GAP adayı (VA-1) |
| `02 §10.4.4` | Kargo şirketi ve takip numarası düzeltilir; bildirilir | Order · AuditLog · *gönderim kaydı* | kargo şirketi, takip numarası; iz satırı | eşlendi |
| `02 §10.4.5` | Teslim tarihi düzeltilir — sipariş düzeyinde; pencere ve ayıp süresi düzeltilmiş tarihten; aynı gün sorusu; kargoya verildiği günden önce ve ileri olamaz; bildirim yok; izde eski ve yeni tarih; ulaşma tarihi aynı kalıpla | Order · OrderItem · Return · AuditLog | `DeliveryDate` (fiziksel kalemlerin tamamı birlikte); kargoya verildiği an; ulaşma tarihi; izde eski ve yeni tarih | eşlendi |
| `02 §10.4.6` | Yanlış geçiş yeni geçişle düzeltilir: kapalı sebep listesi ve "diğer"de açıklama; terminalden yalnız bir önceki duruma; iptal düzeltmesinin koşulu ve yeniden ayırma; havale "ödendi" düzeltmesi iki ekseni birlikte geri alır, ödeme süresi yeniden başlar; her düzeltme işlem izine | *geçiş kaydı* · AuditLog · Order · StockReservation · Coupon · *gönderim kaydı* | `CorrectionReason` ve açıklaması; düzeltilen geçiş; yeniden ayırma; ödeme süresinin yeni başlangıcı (Z-8); B-3'ün yeni sürede bir kez daha | eşlendi |
| `02 §10.4.7` | İç not müşteriye görünmez; yazılması ve değiştirilmesi işlem izine (metin değil); not siparişin saklama süresini izler; kişisel veri riski | Order · AuditLog | iç not — siparişte tek serbest metin, düzenlenebilir; imhası siparişle; iz satırı metinsiz | eşlendi |
| `02 §10.4.8` | Firma iptalinin sebep listesi §7.2.4'te, geri alınamaz onaylar §5.9'da | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §10.4.9` | "Mal dönmedi" kapanışı: gönderme süresi geçince, ulaşmamış adetlere; işlem izine, bildirim yok, geri ödeme yok, IBAN silinir; mal ulaşırsa yeniden açılır; kapanış borca karar vermez — kaleme bağlı tutar bazlı geri ödeme, sonradan düşülür; kendiliğinden kapanış yok | NotReturnedClosure · Return · Refund · RefundIbanRequest · AuditLog | kapanışın adedi, anı, yapanı; yeniden açılma; tutar bazlı geri ödemenin kalem bağı | eşlendi |
| `02 §10.4.10` | Başka kanaldan cayma kaydı: kalem, adet, bildirimin ulaştığı tarih (geçmişe dönük); teyit; IBAN yalnız iletişim e-postasından ve denetimden geçerse; IBAN'sız kayıtta istek ve B-14; beyanın tarihi girilen tarih; aynı gün sorusu; pencere dışı ve mutlak istisnada ayrı onay, bir yıl sınırı; gönderim tarihi tutulmaz; kargoya verilmemiş kalemde iptal; gecikme feshinin kaydı — kalem ve adet seçilmez | Withdrawal · LateDeliveryTermination · RefundIbanRequest · Cancellation · AuditLog · *gönderim kaydı* | beyanın kaynağı (panel kaydı), bildirimin ulaştığı tarih ile kaydın anı, adet; pencere dışı ya da istisna onayı; teslim sırası cevabı; bildirimin gönderim tarihi alanı yok | eşlendi |
| `02 §10.4.11` | Misafir siparişinin iletişim e-postası müşterinin talebiyle düzeltilir | Order · OrderEmailHistory | siparişin iletişim e-postası — donmanın tek istisnası | eşlendi |
| `02 §10.4` m5 | Kapsam: yalnız giriş yapılmadan verilmiş sipariş; kişisel veriler imha edilene kadar | Order | siparişin giriş yapılarak mı verildiği — hesap bağından ayrı (sonradan düşen misafir siparişi düzeltilebilir) | eşlendi |
| `02 §10.4` m6 | Teyit: siparişteki bilgilerle; ürün denetlemez | veri doğurmaz — ürünün dışında | — | veri doğurmaz |
| `02 §10.4` m7 | Sonuçlar: e-posta yeni değer; yeni erişim anahtarı; yeni adrese B-1, eskisine B-15; hesap bağı kesilir ya da yeni hesaba düşer; işlem izine — değer taşımaz; eski ve yeni adres e-posta geçmişinde yönetici ve tarihle, müşteriye görünmez, siparişle imha; "e-posta düzeltildi" işareti kalkmaz | OrderEmailHistory · Order · AuditLog · *gönderim kaydı* · veri doğurmaz — türetilir | eski ve yeni adres, yönetici, an; `OrderAccessKey` yenilenir; hesap bağı; işaret geçmişin varlığından türer | eşlendi |
| `02 §10.4` m8 | Ne değişmez: IBAN, durumlar, süreler, kalemler | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §10.4` m9 | Kalan risk: aradaki işlemler kalem kayıtlarından ve e-posta geçmişinden okunur; IBAN girişinin zamanı kayıtlı değildir (K-710) | OrderEmailHistory · RefundIbanRequest | IBAN girişinin zaman damgası yok — bilinçli | eşlendi |
| `02 §10.4` m10 | Elenen yollar: kurtarma kodu, telefonla IBAN, numara ve telefonla erişim | yoktur — kapsam dışı | kurtarma kodu alanı yok | veri doğurmaz |

**§10.5–§10.8 — manuel adımlar, bekleyen işler ve satış özeti, toplu veri, ilk kurulum (25)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §10.5.1` | Sipariş başına sistem içi zorunlu elle adımlar | veri doğurmaz — kod | — (adımların verisi kendi satırlarında) | veri doğurmaz |
| `02 §10.5` m1 | Kart ile ödenen fiziksel: 2 — kargoya verme ve takip; teslim işareti ve tarihi | Order · OrderItem | kargo bilgisi; `DeliveryDate` | eşlendi |
| `02 §10.5` m2 | Havale ile ödenen fiziksel: 3 — + "ödendi" işareti | Order | `PaymentStatus` ve ödeme onayının anı | eşlendi |
| `02 §10.5` m3 | Hizmet siparişi: 1 — "tamamlandı" işareti | OrderItem | tamamlandı işareti | eşlendi |
| `02 §10.5` m4 | Dijital sipariş: 0 — sistem teslim eder | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §10.5` metin | "Ödendi" işareti sipariş düzeyinde bir kez; karışık siparişte adımlar toplanır; bütçe hat başına | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §10.5.2` | Koşullu adımlar ve bütçeye girmeyen olağan dışı işler; kısmi adet yeni adım eklemez | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §10.5.3` | Fatura sistem dışı bir adım; ürün göremez | yoktur — kapsam dışı · veri doğurmaz — ürünün dışında | fatura numarası ve kesilme alanı yok | veri doğurmaz |
| `02 §10.5.4` | Bütçe hat başına üçü geçmez; ileriye dönük kural | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §10.5.5` | Hacim varsayımı: günde 10, tepe 50 sipariş | veri doğurmaz — mimari (`05`) | — (hacim kabulleri `02 §11.3`) | veri doğurmaz |
| `02 §10.6.1` | Panel ana sayfası altı sayaç; "IBAN bekleniyor" sayılmaz; kanal ve firma bildirimi uyarıları; "e-posta ulaşmadı" satırı; sayaçlar hesaplanır, saklanmaz | veri doğurmaz — türetilir · *kanal hâli kaydı* · *gönderim kaydı* | sayaçlar durumlardan ve kalem kayıtlarından (K-405, K-675); uyarılar kanal hâlinden ve gönderim kaydından | eşlendi |
| `02 §10.6.2` | Satış özeti analitik değil: sipariş sayısı ve cirosu (ödemesi tamamlanmış, oluştuğu tarihin dönemine), en çok satanlar, ödeme yöntemi dağılımı, iptal ve iade sayısı, oranlar, talep sayısı; hesaplanır, saklanmaz, dışa aktarılmaz | veri doğurmaz — türetilir · Order · OrderItem | sipariş tarihi, onaylanan toplam, `PaymentMethod`, `PaymentStatus`; kalemin donmuş ürün değerleri | eşlendi |
| `02 §10.6.3` | Ölçülerin tanımı | veri doğurmaz — türetilir | — (maddeleriyle) | veri doğurmaz |
| `02 §10.6` m1 | Ödeme tamamlama oranı: onaya gelen sepetler; aynı sepetten art arda siparişler tek deneme; sepet ilk siparişinin dönemine | veri doğurmaz — türetilir · Order · Cart | siparişin doğduğu sepetin kimliği — sepet boşalsa da kalıcı (`02 §6.2.2`); sipariş tarihi | eşlendi |
| `02 §10.6` m2 | İletişim talebi sayısı: KVKK ve "Sipariş hakkında" dışındakiler | veri doğurmaz — türetilir · ContactRequest | `ContactTopic`, oluşma anı | eşlendi |
| `02 §10.6` m3 | Kargoya verme sözüne uyum: ödeme onayından kargoya verilene, donmuş söz iş günüyle | veri doğurmaz — türetilir · Order | ödeme onayının anı, kargoya verildiği an, donmuş kargoya verme sözü | eşlendi |
| `02 §10.6` m4 | İptal ve iade oranı: iptal, fesih ve cayma kayıtlarından; caymayı beyan anında sayar; "mal dönmedi" ve ret değiştirmez | veri doğurmaz — türetilir · Cancellation · LateDeliveryTermination · Withdrawal | kayıtların damgaları | eşlendi |
| `02 §10.6` m5 | Sıfır payda "değerlendirilemez" | veri doğurmaz — arayüz (`04`) | — | veri doğurmaz |
| `02 §10.6` m6 | Ölçüm penceresi satışın ilk açıldığı gün başlar — dört koşulun ilk kez birlikte sağlandığı gün; iptal ve iade oranının okunma günü | veri doğurmaz — türetilir · AuditLog | ilk açılış günü saklanmaz — dört koşulun değişiklikleri işlem izinin (1), (5) ve (6) alanlarında eski ve yeni değerle durur (`02 §10.3.1`); yeni alan açılmaz (`01 §6`, "yeni veri tutulmaz") | eşlendi |
| `02 §10.7.1` | Siparişler CSV dışa aktarılır: fatura verisi, donmuş fatura adresi ve firma kimliği, ödeme yöntemi ve onay tarihi, kargoya verme tarihi, kargo şirketi, alan adı, iletişim e-postası ve alıcı adı; geç ödeme ve geri ödemesi; işlem izine | Order · OrderItem · *ödeme kaydı* · Refund · AuditLog · veri doğurmaz — kurulum | donmuş tutarlar, KDV'ler, kupon payları, kargo ücreti ve KDV bölüşümü; ödeme onayının ve kargoya verilmenin anı (hizmette tamamlanma, dijitalde ödeme onayı); alıcı adı; dışa aktarma izi; alan adı kurulumdan | eşlendi |
| `02 §10.7.2` | Ürün içe aktarma ve toplu güncelleme yok | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §10.7.3` | Katalog ve içerik için dışa aktarma yok; iz ve özet dışa aktarılmaz | yoktur — kapsam dışı · veri doğurmaz — mimari (`05`) | — (çıkış hakkı barındırma düzleminde) | veri doğurmaz |
| `02 §10.7.4` | Sipariş vermemiş üyeler ve iletişim talebi sahipleri için ad ve e-posta CSV; işlem izine | Customer · ContactRequest · AuditLog | hesabın adı (`02 §3.13.21`), formun ad alanı, e-postalar; dışa aktarma izi | eşlendi |
| `02 §10.8.1` | Site boş kurulur; örnek içerik yok | veri doğurmaz — kod | — (başlangıç verisinde örnek kayıt yok — VM8) | veri doğurmaz |
| `02 §10.8.2` | Kurulum kontrol listesi: satış koşulları ve zorunlu doldurulacaklar; madde tamamlandıkça işaretlenir, hepsi tamamlanınca kaybolur; dış ön koşullar `10 §4.1`'de | veri doğurmaz — türetilir | liste koşullardan okunur, saklanmaz | veri doğurmaz |

**§11 — sayısal parametreler (55)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §11` metin | Parametreler koda gömülmez; envanterin evi; katmanlar; firma ayarları dolu gelir — ifa süresi istisna; firma ayarı değişikliği eski ve yeni değerle işlem izine; çıkan ve giren kimlikler; envantere girmeyenler — yasal süreler, ürünün listeleri, ölçüler, kurulum ayarları | *firma ayarları* · AuditLog · veri doğurmaz — kod · veri doğurmaz — kurulum | firma ayarlarının varsayılanları (VM8); izde eski ve yeni değer; ürünün listeleri — kargo şirketi, resmî tatil, yaygın şifre, konu tipleri, sebepler — kodda ya da başlangıç verisinde (VM5, VM8) | eşlendi |
| `02 §11.1` P-1 | Kargo ücreti — sipariş başına, KDV dahil; varsayılan 0 TL | *firma ayarları* · Order | kargo ücreti; siparişe donan kargo ücreti | eşlendi |
| `02 §11.1` P-2 | Kaldırıldı — kargonun KDV'si fiziksel kalemlerin oranını izler | veri doğurmaz — kod | — (kimlik yeniden kullanılmaz) | veri doğurmaz |
| `02 §11.1` P-3 | Ücretsiz kargo eşiği; boş — kapalı | *firma ayarları* | eşik (boş olabilir) | eşlendi |
| `02 §11.1` P-4 | Asgari sipariş tutarı; boş — kapalı | *firma ayarları* | tutar (boş olabilir) | eşlendi |
| `02 §11.1` P-5 | Kargoya verme süresi — firma varsayılanı 2 iş günü; üst çit 10 | *firma ayarları* · veri doğurmaz — kod | süre; çit kodda | eşlendi |
| `02 §11.1` P-6 | Kargoya verme süresi — ürün düzeyi; boş — varsayılanı devralır | Product · Order | ürünün süresi (boş olabilir); siparişe donan kargoya verme sözü | eşlendi |
| `02 §11.1` P-7 | Havale ödeme süresi 2 iş günü; çit 1–3 | *firma ayarları* · Order | süre; siparişin ödeme süresinin bittiği an | eşlendi |
| `02 §11.1` P-8 | İndirme hakkı — kalem başına 5; en az 1 | *firma ayarları* · OrderItem | hak; kaleme donan hak ve sayaç | eşlendi |
| `02 §11.1` P-9 | Bir siparişte en fazla adet — ürün başına; boş | Product | sınır (boş olabilir) | eşlendi |
| `02 §11.1` P-10 | KDV oranı varsayılanı %20; ürün kendi oranını taşır | *firma ayarları* · Product | varsayılan oran; `VatRate` | eşlendi |
| `02 §11.1` P-11 | Kaldırıldı — duyuru metni kurumsal içerik kaydıdır | Announcement | — (kimlik yeniden kullanılmaz) | eşlendi |
| `02 §11.1` P-44 | İfa süresi — hizmet ürünü; varsayılan yok, zorunlu; alt çit 1 gün | Product · OrderItem | ifa süresi; kaleme donan ifa süresi | eşlendi |
| `02 §11.2` P-12 | E-posta doğrulama bağlantısının ömrü 24 saat | veri doğurmaz — kod · *bağlantı kaydı* | ömrün sonu kayıtta (Z-1) | eşlendi |
| `02 §11.2` P-13 | Şifre sıfırlama bağlantısının ömrü 60 dakika | veri doğurmaz — kod · *bağlantı kaydı* | ömrün sonu | eşlendi |
| `02 §11.2` P-14 | Yönetici daveti bağlantısının ömrü 72 saat | veri doğurmaz — kod · AdminInvitation | ömrün sonu | eşlendi |
| `02 §11.2` P-47 | E-posta değişikliğini geri alma bağlantısının ömrü 7 gün | veri doğurmaz — kod · *bağlantı kaydı* | ömrün sonu (Z-45) | eşlendi |
| `02 §11.2` P-15 | Kısa oturum 2 saat, son işlemden | veri doğurmaz — kod · Session | son işlem anı | eşlendi |
| `02 §11.2` P-16 | Uzun oturum 30 gün, son işlemden | veri doğurmaz — kod · Session | son işlem anı; kısa ya da uzun | eşlendi |
| `02 §11.2` P-48 | Tanınan tarayıcı işaretinin ömrü 90 gün, son başarılı girişten | veri doğurmaz — kod · RecognizedBrowserMark | son başarılı giriş anı (Z-46) | eşlendi |
| `02 §11.2` P-17 | Asgari şifre uzunluğu 10 karakter | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §11.2` P-18 | Kart ödeme süresi 30 dakika | veri doğurmaz — kod · Order | ödeme süresinin bittiği an (Z-7) | eşlendi |
| `02 §11.2` P-19 | Sipariş numarasının rastgele blok uzunluğu 6 karakter | veri doğurmaz — kod · Order | `OrderNumber` — biçimi kodda, tekil | eşlendi |
| `02 §11.2` P-20 | Kategori ağacının derinliği 3 seviye | veri doğurmaz — kod · Category | üst kategori bağı; derinlik kodda | eşlendi |
| `02 §11.2` P-21 | Ürün başına seçenek boyutu 2 | veri doğurmaz — kod · Option | — | eşlendi |
| `02 §11.2` P-22 | Görsel biçimleri JPEG, PNG, WebP | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §11.2` P-23 | Tek görselin boyut tavanı 10 MB | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §11.2` P-24 | Ürün başına görsel adedi 20 — ürün ve varyantlar birlikte | veri doğurmaz — kod · *görsel* | görselin ürün bağı | eşlendi |
| `02 §11.2` P-25 | İçerik kaydı başına görsel adedi 20 | veri doğurmaz — kod · *görsel* | görselin içerik bağı | eşlendi |
| `02 §11.2` P-26 | Dijital dosyanın boyut tavanı 500 MB | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §11.2` P-27 | Ürün adı ve içerikte ad ya da başlık 150 karakter | veri doğurmaz — kod | — (alan uzunluğu VM4) | veri doğurmaz |
| `02 §11.2` P-28 | SSS sorusu, kısa açıklama, şube çalışma saatleri 200 karakter | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §11.2` P-29 | Hakkımızda kısa tanıtımı 400 karakter | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §11.2` P-30 | Duyuru metni 120 karakter | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §11.2` P-31 | L-1 — e-posta başına 5 / 15 dakika (tanınan tarayıcının sayacı da); IP başına 20 / 15 dakika | veri doğurmaz — kod · *deneme kaydı* | deneme satırının anı; eşik kodda | eşlendi |
| `02 §11.2` P-32 | L-2 — e-posta 3 / 1 saat; IP 10 / 1 saat | veri doğurmaz — kod · *deneme kaydı* | deneme satırı | eşlendi |
| `02 §11.2` P-33 | L-3 — e-posta 3 / 1 saat; IP 5 / 1 saat | veri doğurmaz — kod · *deneme kaydı* | deneme satırı | eşlendi |
| `02 §11.2` P-34 | L-4 — IP 5 / 1 saat | veri doğurmaz — kod · *deneme kaydı* | deneme satırı | eşlendi |
| `02 §11.2` P-35 | L-5 — e-posta 5 / 15 dakika; IP 10 / 15 dakika | veri doğurmaz — kod · *deneme kaydı* | deneme satırı | eşlendi |
| `02 §11.2` P-36 | L-6 — sepet 5 / 15 dakika; birleşen sepet büyüğü taşır | veri doğurmaz — kod · *deneme kaydı* | deneme satırı — sepet ekseni | eşlendi |
| `02 §11.2` P-37 | L-7 — e-posta (üye) ya da IP 5 / 24 saat | veri doğurmaz — kod · *deneme kaydı* | deneme satırı | eşlendi |
| `02 §11.2` P-46 | L-8 — e-posta (üye) ya da IP 3 açık sipariş; pencere yok | veri doğurmaz — kod · veri doğurmaz — türetilir | açık siparişlerden sayılır | veri doğurmaz |
| `02 §11.2` P-49 | L-9 — e-posta 3 / 1 saat; IP 10 / 1 saat | veri doğurmaz — kod · *deneme kaydı* | deneme satırı | eşlendi |
| `02 §11.2` P-38 | Yeniden deneme aralıkları 5 dakika, 30 dakika, 2 saat | veri doğurmaz — kod · *gönderim kaydı* | sonraki deneme anı (Z-26) | eşlendi |
| `02 §11.2` P-39 | Periyodik imha günde bir | veri doğurmaz — kod · *imha kaydı* | imha çalışmasının tarihi | eşlendi |
| `02 §11.2` P-40 | İletişim talebinin saklama süresi: son kapatılıştan 2 yıl, "Sipariş hakkında" 10 yıl; açık talepte açılıştan | veri doğurmaz — kod · ContactRequest | son kapatıldığı an, oluşma anı, `ContactTopic` (Z-33) | eşlendi |
| `02 §11.2` P-41 | Hesap, yönetim ve firma bildirimlerinin gönderim kaydı 1 yıl; siparişe bağlı müşteri bildirimleri 10 yıl ve envantere girmez | veri doğurmaz — kod · *gönderim kaydı* | gönderim anı, bildirimin türü — sipariş bağı süreyi ayırır (Z-34, Z-43) | eşlendi |
| `02 §11.2` P-43 | Malı dönmeyen caymada IBAN'ın saklaması — gönderme süresinin bitiminden 30 gün | veri doğurmaz — kod · RefundIbanRequest · Withdrawal | beyanın damgası (gönderme süresinin başlangıcı); IBAN'ın silinme anı (Z-38) | eşlendi |
| `02 §11.2` P-45 | Giriş kaydının ve sistem kayıtlarının saklaması 6 ay | veri doğurmaz — kod · *giriş kaydı* · veri doğurmaz — mimari (`05`) | giriş kaydının oluştuğu an (Z-40) | eşlendi |
| `02 §11.2` P-42 | En dar ekran genişliği 320 CSS pikseli | veri doğurmaz — doğrulama (`12`) | — | veri doğurmaz |
| `02 §11.3` metin | Hacim kabulleri parametre değildir; tabandır, tavan değil; aşımın tetikleyicisi `10 §4`'te | veri doğurmaz — mimari (`05`) | — | veri doğurmaz |
| `02 §11.3` H-1 | Katalog — birkaç yüz ürün; tavan yok; tek depo kalıcı sınır | veri doğurmaz — mimari (`05`) · yoktur — kapsam dışı | depo alanı yok (`01 §7` S-5) | veri doğurmaz |
| `02 §11.3` H-2 | Sipariş hacmi — günde 10, tepe 50; tetikleyici ödemesi tamamlanmış siparişleri sayar | veri doğurmaz — mimari (`05`) · veri doğurmaz — türetilir | — (günlük sayı satış özetinden — `02 §10.6.2`) | veri doğurmaz |
| `02 §11.3` H-3 | Sepet — elli kaleme kadar yeniden değerlendirme; kalem sayısına tavan yok | veri doğurmaz — mimari (`05`) | — | veri doğurmaz |
| `02 §11.3` H-4 | Eşzamanlı ziyaretçi — `05`'in tasarım girdisi | veri doğurmaz — mimari (`05`) | — | veri doğurmaz |

**§12.1–§12.4 — ticari çerçeve, kişisel veriler, veri sahipliği, erişilebilirlik (29)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §12` metin | Ürün mekanizmayı sağlar, yükümlülük firmanındır; firmanın yükümlülükleri §12.5'te | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §12.1.1` | Tek pazar Türkiye, tek dil Türkçe, tek para birimi TRY | veri doğurmaz — kod · yoktur — kapsam dışı | para birimi ve dil alanı yok | veri doğurmaz |
| `02 §12.1.2` | Ürün tüketiciye satış için; ticari alıcıya özel akış yok | yoktur — kapsam dışı | alıcının ticari bilgisi alanı yok | veri doğurmaz |
| `02 §12.1.3` | Satıcı firmadır; tahsilat firmanın sağlayıcı hesabına; kart verisi yok | veri doğurmaz — ürünün dışında · yoktur — kapsam dışı | kart verisi alanı yok | veri doğurmaz |
| `02 §12.1.4` | ETBİS kaydı firmanın; isteğe bağlı ETBİS doğrulama bilgisi alanı ve bant; yasal kimlik sürekli erişilebilir | Company | ETBİS doğrulama bilgisi (boş olabilir); kimlik alanları | eşlendi |
| `02 §12.1.5` | Fiyat KDV dahil; üretim yeri ve yerli üretim logosu; ölçüyle satılan üründe birim fiyat; fiyatın başlangıç tarihi | Variant · Product · *fiyat geçmişi* · veri doğurmaz — türetilir | KDV dahil fiyat; üretim yeri, ölçü birimi ve net miktar; fiyatın başlangıç anı; birim fiyat türetilir | eşlendi |
| `02 §12.1.6` | İndirim beyanı referans fiyata dayanır: on gün içindeki en düşük ya da hemen önceki fiyat; referans kaleme donar; başlangıç ve bitiş gösterilir | *fiyat geçmişi* · Discount · OrderItem | uygulanmış fiyatlar ve anları; indirimin başlangıç ve bitişi; kaleme donan `ReferencePrice` | eşlendi |
| `02 §12.1.7` | Mesafeli satış: iki kutu; sürümler ve firma kimliği donar; dijital ve hizmet onayı gösterilen metniyle donar; onay sırasında sürüm artarsa sipariş durur; metinler B-1'de tam; işlem rehberi; dört metin sürümlü, yeniden onay yok | *onay kaydı* · *yasal metin sürümü* · Order · CustomPage · *gönderim kaydı* | kutuların kaydı ve gösterilen metni; siparişe donan sürümler ve firma kimliği; işlem rehberi sayfası (`02 §3.24.8`) | eşlendi |
| `02 §12.1.8` | Teslim: taahhüt edilen süre, en geç otuz gün; kişiye özel üretimde de; gecikme feshi kalemlerle sınırlı; kanuni faiz firmada; firma iptali m.16/4 | LateDeliveryTermination · Order · Cancellation | siparişin firmaya ulaştığı an; fesih kaydı; sebepli iptal | eşlendi |
| `02 §12.1.9` | Cayma, iade ve ayıbın özeti; kuralların tamamı §7'de | Withdrawal · Return · DefectClaim | — (§7'nin satırlarıyla, §1.1.8) | eşlendi |
| `02 §12.1.10` | Fatura ürünün dışında; ürün fatura verisini taşır ve dışa aktarır; fatura saklanmaz | yoktur — kapsam dışı · Order | fatura alanı yok; donmuş fatura verisi siparişte (`02 §3.25`) | eşlendi |
| `02 §12.1.11` | Ticari elektronik ileti yok; İYS yok | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §12.1.12` | Ürün ne satıldığını denetlemez; izinli ürünler kapsam dışı | yoktur — kapsam dışı | ürün türü denetim alanı yok | veri doğurmaz |
| `02 §12.2.1` | Veri sorumlusu firma; bakım ve destekte geliştirici veri işleyen | veri doğurmaz — ürünün dışında | — | veri doğurmaz |
| `02 §12.2.2` | Aydınlatma metni sürümlü ayrı kayıt; zorunlu, boşaltılamaz; kapı; bağlantısı veri toplayan ekranlarda; onay ve "gördü" kaydı yok; sipariş o günkü sürümü dondurur | PrivacyNotice · *yasal metin sürümü* · Order · yoktur — kapsam dışı | sürümler; siparişe donan aydınlatma sürümü; kullanıcı bazında görme kaydı yok | eşlendi |
| `02 §12.2.3` | Hukuki sebepler m.5/2'nin sayımıyla; açık rıza yok | veri doğurmaz — ürünün dışında | — (metnin içeriği) | veri doğurmaz |
| `02 §12.2.4` | Açık rıza toplanmaz; rıza ekranı ve kaydı yok; onay kutuları sözleşmeye ilişkin | yoktur — kapsam dışı · *onay kaydı* | rıza kaydı yok | eşlendi |
| `02 §12.2.5` | Yalnız zorunlu çerezler; misafir sepetinin çerezi kalıcı; ödeme çerçevesinin çerezleri politikada; onay bandı yok; çerez politikası sürümlü; ilk yayından önce çerez yazılmaz | veri doğurmaz — mimari (`05`) · CookiePolicy · *yasal metin sürümü* · Cart · Session · RecognizedBrowserMark | çerez politikasının sürümleri; misafir sepetinin tarayıcı anahtarı (Z-6); oturum ve tanınan tarayıcı kayıtları sunucuda | eşlendi |
| `02 §12.2.6` | KVKK talebi tipi başvuru kanalı; kimlik doğrulama ve otuz gün firmada; sayaç yok; veri indirme yok | ContactRequest · yoktur — kapsam dışı | `ContactTopic` "KVKK talebi"; süre sayacı yok | eşlendi |
| `02 §12.2.7` | Saklama ve imha: günlük iş, düğme yok, ize yazılmaz; her imha çalışması ve anında silme imha kaydına (tarih, tür, sayı; en az üç yıl); ödemesi alınmamış siparişin kişisel verisi üç yılda; geç ödeme siparişi ödenmiş yapar; on yıllık saklamanın dört kaydı; siparişte kişisel alanlar boşaltılır, hesap bağı kesilir; hesap silmede giriş bilgileri, oturumlar, adres defteri, sepet silinir; IBAN'ın silinmesi; iz kişisel veri taşımaz | *imha kaydı* · Order · Customer · Session · AddressBook · Cart · RefundIbanRequest · *gönderim kaydı* · ContactRequest · OrderEmailHistory · *bekleyen e-posta değişikliği* · AuditLog | imha kaydının tarih, tür, sayısı (Z-44); siparişin ödemesi alınmış mı (Z-28, Z-41) ve oluştuğu yıl; kişisel alanların boşaltılması ve hesap bağının kesilmesi; kalan ticari kaydın alanları | eşlendi |
| `02 §12.2.8` | Ürün kişisel veriyi yurt dışına aktarmaz; kurulumdan doğan aktarım firmada; ödeme sağlayıcısına alıcı verisi; Google girişi aktarım değil | veri doğurmaz — ürünün dışında · veri doğurmaz — kurulum | — | veri doğurmaz |
| `02 §12.2.9` | Veri ihlali: ayrı kayıt yok; iz, giriş kaydı, sistem kayıtları; bildirim firmada; ulaşma bilgisi dışa aktarmalardan | AuditLog · *giriş kaydı* · yoktur — kapsam dışı | ihlal kaydı tablosu yok | eşlendi |
| `02 §12.2.10` | Yönetim tarafında veri erişimi ayrımı yok | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §12.2.11` | İçerikte üçüncü kişi verisinin izni firmada | veri doğurmaz — ürünün dışında | — | veri doğurmaz |
| `02 §12.2.12` | Veri asgariliği §8.4'te | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §12.3.1` | Veri firmanındır; abonelik bitse de kurulum çalışır; uyum korunmaz | veri doğurmaz — ürünün dışında | — | veri doğurmaz |
| `02 §12.3.2` | Çıkış hakkı barındırma düzleminde; sipariş tarafı panelden dışa aktarılır | veri doğurmaz — mimari (`05`) | — | veri doğurmaz |
| `02 §12.3.3` | Çalışma süresi taahhüdü yok; sipariş tarafında veri kaybına tolerans sıfır — disk kaybında en çok bir dakika | veri doğurmaz — mimari (`05`) | — (`02 §3.34.7`, §3.34.8; `05`) | veri doğurmaz |
| `02 §12.4.1` | Erişilebilirlik yasal yükümlülük; taban Kontrol Listesi A ve WCAG 2.2; beyan verilmez | veri doğurmaz — doğrulama (`12`) · *görsel* | görselin alternatif metni (`02 §3.34.5`) | eşlendi |

**§12.5 — firmanın yükümlülükleri (34)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §12.5` m1 | ETBİS kaydı — isteğe bağlı alan ve bant; kayıt ve güncellemeler firmada | Company · veri doğurmaz — ürünün dışında | ETBİS doğrulama bilgisi | eşlendi |
| `02 §12.5` m2 | Yasal kimliğin doğruluğu — zorunlu alan ve işlem izi | Company · AuditLog | kimlik alanları; değişiklik izi | eşlendi |
| `02 §12.5` m3 | Fiyat etiketinin birim fiyatı — ölçü birimi ve net miktar | Product · veri doğurmaz — türetilir | ölçü birimi, net miktar; birim fiyat türetilir | eşlendi |
| `02 §12.5` m4 | VERBİS — ürünün sağladığı yok | veri doğurmaz — ürünün dışında | — | veri doğurmaz |
| `02 §12.5` m5 | Aydınlatma metni ve çerez politikası — taslak, yer tutucular, sürüm | PrivacyNotice · CookiePolicy · *yasal metin sürümü* | metnin içeriği ve sürümü; taslak başlangıç verisi (VM8) | eşlendi |
| `02 §12.5` m6 | Barındırma, e-posta ve ödeme sağlayıcısının konumu; yurt dışı aktarım şartları | veri doğurmaz — ürünün dışında | — | veri doğurmaz |
| `02 §12.5` m7 | Ödeme sağlayıcısı ve Google'ın aydınlatmada anılması — yer tutucu | PrivacyNotice | — (metnin içeriği) | eşlendi |
| `02 §12.5` m8 | KVKK başvurusuna cevap — kanal ve talep kaydı; üye kaydı görünümü ve silme | ContactRequest · Customer | talep; hesap | eşlendi |
| `02 §12.5` m9 | Veri ihlali bildirimi — iz, giriş kaydı, dışa aktarmalar | AuditLog · *giriş kaydı* | — | eşlendi |
| `02 §12.5` m10 | İhlal kaydı ve müdahale planı — iz ve giriş kaydı; kayıt firmada | AuditLog · *giriş kaydı* · veri doğurmaz — ürünün dışında | — | eşlendi |
| `02 §12.5` m11 | Saklama ve imha politikası — süre tablosu, periyodik imha, imha kaydı; ürün dışı kayıtların imhası firmada | *imha kaydı* | imha kaydı (Z-44) | eşlendi |
| `02 §12.5` m12 | Elektronik ticaret kayıtlarının on yıllık saklaması — sipariş, gönderim kaydı, "Sipariş hakkında" talebi | Order · *gönderim kaydı* · ContactRequest | süre başlangıçları (Z-28, Z-41, Z-43, Z-33) | eşlendi |
| `02 §12.5` m13 | İçerikte üçüncü kişi verisi — yayım izni firmada | veri doğurmaz — ürünün dışında | — | veri doğurmaz |
| `02 §12.5` m14 | Yönetici hesabı açılan kişiler — tek rol, işlem izi | Admin · AuditLog | — | eşlendi |
| `02 §12.5` m15 | İfanın imkânsızlaştığının bildirimi — B-7 ve geri ödeme hattı; uyarı | Cancellation · *gönderim kaydı* | `CancellationReason`, iptal anı | eşlendi |
| `02 §12.5` m16 | Ters ibrazda ikinci ödeme yapmamak — hatırlatma; çift ödemenin riski firmada | veri doğurmaz — ürünün dışında · *geri ödeme kaydı* | ters ibraz alanı yok; geri ödemenin hâli | eşlendi |
| `02 §12.5` m17 | Ayıpta geri ödemenin zamanı — süre sayacı yok | Refund · RefundIbanRequest · DefectClaim | — (sayaç yok) | eşlendi |
| `02 §12.5` m18 | Faturanın kesilmesi — fatura verisi ve dışa aktarma | Order · OrderItem | donmuş fatura verisi | eşlendi |
| `02 §12.5` m19 | Satılan ürünün mevzuata uygunluğu | veri doğurmaz — ürünün dışında | — | veri doğurmaz |
| `02 §12.5` m20 | İkinci el malın ayrı kategoride sunulması — kategori ağacı; ikinci el alanı eklenmez | Category · yoktur — kapsam dışı | ikinci el alanı yok | eşlendi |
| `02 §12.5` m21 | Erişilebilirlik incelemesi — uygun site | veri doğurmaz — doğrulama (`12`) | — | veri doğurmaz |
| `02 §12.5` m22 | Serbest metnin ayarlarla tutarlılığı — hatırlatma | veri doğurmaz — arayüz (`04`) | — | veri doğurmaz |
| `02 §12.5` m23 | Kargo ücretinin KDV'sinin bölüşümü — hesap ve döküm, dışa aktarmada | Order · veri doğurmaz — türetilir | kargo ücretinin bölünmüş KDV'si — siparişte donar ya da türetilir (VM2-06) | eşlendi |
| `02 §12.5` m24 | Gecikme feshinde kanuni faiz — uyarı; sistem faizi hesaplamaz | LateDeliveryTermination · yoktur — kapsam dışı | faiz alanı yok | eşlendi |
| `02 §12.5` m25 | Güncelleme almayan kurulumun yasal uyumu | veri doğurmaz — ürünün dışında | — | veri doğurmaz |
| `02 §12.5` m26 | Bakım, destek ve altyapı sağlayıcılarıyla veri işleme sözleşmesi | veri doğurmaz — ürünün dışında | — | veri doğurmaz |
| `02 §12.5` m27 | Teslim tarihinin ve ulaşma tarihinin doğruluğu — tarih alanları ve iz | OrderItem · Return · AuditLog | `DeliveryDate`, ulaşma tarihi — ileri tarihli olamaz | eşlendi |
| `02 §12.5` m28 | İade reddinin ispatı ve malın geri gönderilmesi — ret kaydı, B-16, iz; kanıt alınmaz | ReturnRejection · AuditLog · *gönderim kaydı* | ret kaydı; kanıt alanı yok | eşlendi |
| `02 §12.5` m29 | Eski iade adresine gönderilen malın teslim alınması — beyana yazılan adres | Withdrawal · Return | beyana donan iade adresi; ulaşma tarihi | eşlendi |
| `02 §12.5` m30 | Geri ödemenin yasal süre içinde yapılması — kalan süre (Z-16, Z-17) | Refund · veri doğurmaz — türetilir | kalan süre türetilir | eşlendi |
| `02 §12.5` m31 | Dışa aktarılan dosya — dışa aktarmanın izi | AuditLog | dışa aktarma satırı | eşlendi |
| `02 §12.5` m32 | Misafir e-postasını düzeltirken kimlik teyidi — eski anahtarın geçersizleşmesi, B-15, iz, e-posta geçmişi | OrderEmailHistory · Order · AuditLog | eski ve yeni adres; `OrderAccessKey` | eşlendi |
| `02 §12.5` m33 | Siparişin adresini düzeltirken talebin teyidi — düzeltme ekranı, B-10, iz satırı | Order · AuditLog · *gönderim kaydı* | adres düzeltmesinin iz satırı — eski adresin yeri VA-2 | eşlendi |
| `02 §12.5` m34 | Başka kanaldan cayma ya da fesih bildiriminin teyidi — kayıt ekranı, B-9, iz; IBAN sipariş sayfasından | Withdrawal · LateDeliveryTermination · RefundIbanRequest · AuditLog | panel kaydı; IBAN isteği | eşlendi |

**§13 — açık kararlar (4)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §13.1` | Tablo `02`'nin kendi açık kararlarını taşır; dönemin kaydı tracker §4 | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §13.2` | Aşama kapanışında tracker §4'ün açık satırları dokümana atanır | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §13.3` | Kapanıştan önce çakışma taraması koşar | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §13.4` | Yazım turu sonunda `02`'ye ait açık karar yok; kapanan satırların listesi | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |

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
| VA-1 | Ürüne görünen ama sözlükte terimi ve kod karşılığı olmayan kayıtların adı | `02 §1.1` m4, `02 §5.8` m3, `02 §10.4.3` · §1.1.2'nin italik adayları | `02 §1.1` m4 ürüne görünen kavramın — panelde ya da müşteriye görünen kayıt, işaret, sayaç — sözlüğe girdiğini, `06`'nın kod adını yalnız teknik kavramlara ve kapalı liste değerlerine verdiğini söyler (K-17, K-531, K-977, K-988). Sözlüğün 105 terimi tam okundu: **ürüne görünen ve terimi olmayanlar** — çıkarma kaydı (`02 §5.8`; panelde kalemin satırı), bekleyen e-posta değişikliği (`02 §3.13.14`; hesap ekranında görünür), onay kutularının kaydı (`02 §3.24.6`; sipariş sayfasında görünür), siparişe donan yasal metin sürümü (`02 §3.23.3`; sipariş sayfası gösterir), fiyat geçmişi (`02 §3.8.5`; fiyatın başlangıç tarihi ürün sayfasında), görsel (`02 §3.11`); **`05`'te kurulan ama K-977'nin teknik kavram listesinde olmayanlar** — siparişin ödeme kaydı (`05 §4.5.3`), dosya kaydı (`05 §7.7`), giriş kaydı (`02 §3.13.20`), yedeğin durum kaydı, anahtar halkası. 2. oturum ayrımı doğrular: teknik olan VM3-03'e, ürüne görünen `02 §1.2`'ye geri beslenecek bir terime (K-652) gider. Bağ ve ayar kayıtları (ürün–kategori bağı, içerik–ürün bağı, firma ayarları, il listesi) adayın dışındadır — adları VM2-03'ün adlandırma kuralıyla türer **1b (2026-10-17):** çıkarma kaydının tanımı `02 §10.4.3`'tedir — kalemin çıkarılması ya da adedinin azaltılması; adet, an ve yönetici; sebep listesi istemez, işlem izine yazılır ve B-11 üretir; sözlükte terimi yoktur. `02 §6`–§13'te ürüne görünen ama sözlükte terimi olmayan **işaret ve alanlar** da vardır — panelde: "e-posta ulaşmadı" işareti (`02 §6.1.1`, §9.1.6), "ödeme sonucu alınamadı" işareti (`02 §6.1.2`), "e-posta düzeltildi" işareti (`02 §10.4.11`), "iade malı bekleniyor" ve "IBAN bekleniyor" işaretleri (`02 §7.4.1`, §7.4.5) ve siparişin iç notu (`02 §10.4.7`); sözlük aynı sınıftan iki işareti taşır (`RefundFailedMark`, `RefundOutcomePendingMark`). Bu satırlar eşlendi — alanın yeri bellidir, adı açıktır; 2. oturum ayrımı bunlara da uygular; saklanmayan, türetilen işaretin adı VM2-06'nın kararına bağlıdır |
| VA-2 | Siparişin adres düzeltmesinde eski adresin yeri ve donma kuralının istisnası | `02 §10.4.2` · `02 §3.23.3`, §10.3.1 · `02 §6.3.6`, §6.7.12, §8.3.7, §12.5 m33 | `02 §10.4.2` teslimat ve fatura adresini — teslimat telefonu dahil — "Kargoya verildi"ye kadar (fiziksel kalemsiz siparişte Teslim edildi'ye kadar) düzelttirir ve düzeltme B-10 ile bildirilir. `02 §10.3.1` iz satırının adres düzeltmesinde eski ve yeni adresi değil siparişi, alanı ve işlemi taşıdığını söyler; misafir e-postasının düzeltmesindeki "e-posta geçmişi"nin (`02 §10.4.11`) adres için bir eşi yazılı değildir; `02 §3.23.3` siparişe donan değerlerin "tek istisna"sını yalnız misafir siparişinin e-postası diye sayar, adres düzeltmesini saymaz; `02 §12.2.7`'nin imha listesi adres geçmişi anmaz. Sonuç: düzeltmeden sonra eski adres hiçbir kayıtta durmaz — kandırılmış düzeltmede (`02 §6.3.6`, §8.3.7) firmanın adresi "geri düzeltmesi" ve teyidin ispatı (§12.5) ürünün içinde dayanaksızdır. Kaynakların tam metni okundu; Kullanıcı Akışları, Arayüz Tanımları ve Teknik Mimari'de "eski adres", "önceki adres" ve "eski ve yeni adres" betikle tarandı — adres düzeltmesine ait karşılık yok (K-739). 2. oturum iki parçayı ayırır: (1) `02 §3.23.3`'ün "tek istisna" cümlesinin adres düzeltmesini de sayması — hizalama, K-652'nin geri beslemesi; (2) eski adresin tutulup tutulmayacağı ve nerede — kişisel veri saklama sorusudur, bir W konusuna bağlanır (VM4-06, VM9) |

### 1.2 Geri izlenebilirlik (entity/alan → kaynak)

**Kapı: matrisin 2. oturumu.** Tablo §1.1.2'nin adaylarından ve §1.1'in satırlarından betikle kurulur; kaynağı olmayan aday ya da alan gerekçesiz eklemedir ve listelenir; Teknik Mimari'nin doğurduğu teknik kayıtların kaynağı park satırıdır (VM1-04). 1a'nın ara denetimi: §1.1.2'nin 71 adayının 69'u §1.1.3–§1.1.6'nın en az bir satırında geçer; kalan ikisi (70, 71) devir adayıdır (§1.1.11). 1b'nin ara denetimi: §1.1.7–§1.1.8'in satırları 71 adayın 62'sini kullanır; kullanmadığı dokuzun yedisi (OptionValue, *ürün–kategori bağı*, Offering, PortfolioItem, FaqItem, Branch, *iş kuyruğu*) 1a'nın satırlarında eşlidir, ikisi (70, 71) devir adayı olarak kalır.

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

*Shopfolio — Data Model v0.3*
