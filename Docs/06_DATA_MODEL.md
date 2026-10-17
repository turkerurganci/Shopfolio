# Shopfolio — Data Model

**Versiyon: v0.8** | **Bağımlılıklar:** `02`, `03`, `05`, `09`, `10_MVP_SCOPE.md` | **Son güncelleme:** 2026-10-17

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
> - **K-875 · K-876 · K-877 · K-879 · K-944 · K-975 · K-979 · K-984 · İmha ve anahtar halkası:** imha işi her kaydın süresini başlangıç değerinden hesaplar — oluştuğu yılın sonu, kapatılış, gönderim anı (`02 §4` Z-28…Z-44); silme fiziksel silmedir; siparişte kişisel alanlar boşaltılır ve hesap bağı kaldırılır (K-582) — imha siparişe bağlı kayıtları da kapsar: iç not, e-posta adresinin düzeltme geçmişi, ayıp talebi — bu aşamanın matrisi adres düzeltme geçmişini de ekledi (adres geçmişi; K-1008, §1.3); bağlı kayıtların alan listesi bu aşamanın işidir (`05 §7.4.3`); imha kaydının ve geçiş kaydının "yalnız eklenen" oluşu uygulama düzeyindedir, veritabanı yetkisiyle korunan yalnız iz tablosudur (`05 §4.1.4`, §7.4.2); imha kaydı yalnız eklenen ayrı bir tabloda durur — tarih, kayıt türü, sayı; sunucunun kayıtlarının kendi döngüsüyle silinen günü her gece "döngüyle silindi" satırı olarak sayı yerine tarih aralığıyla girer (`05 §9.2.2`); işlem izi tablosunda uygulamanın veritabanı kullanıcısı yalnız ekler ve okur, süresi dolan satırı veritabanındaki bir imha fonksiyonu siler (`05 §7`); iz satırı yöneticinin adını ve e-posta adresini ve kaydın kimliğini değer olarak taşır ve izden başka tablolara silmeyle basamaklanan ya da boşaltan bağ yoktur; imha fonksiyonu sahibinin yetkisiyle çalışır, arama yolu sabittir ve çağırma yetkisi yalnız uygulamanın veritabanı kullanıcısınındır (`05 §7.5.1`); periyodik imhanın her parçası imha kaydındaki satırını parçanın işleminde yazar — var olan satırı güncellemez (`05 §7.4.2`); gecelik temizlik kişisel veri taşıyan geçici kayıtların — deneme, oturum, tanınan tarayıcı ve misafir sepeti kayıtları — silinmesini de imha kaydına kayıt türü ve sayıyla yazar (`05 §5.2.9`, §7.4.4; K-979, K-984); bekleyen e-posta değişikliğinin ömrü dolunca zamanlayıcının silmesi de imha kaydına silmeyle aynı işlemde kayıt türü ve sayıyla yazılır (`05 §5.2.7`, §7.4.2; K-984); istek içinde silinen oturum kayıtları — çıkış, şifre değişikliği ve sıfırlama, oturumları sonlandırma komutu gibi — imha değildir, oturumun işleyişidir (`05 §5.2.9`; K-984). Siparişe ait üretilen belgelerin — Ön Bilgilendirme Formu, Mesafeli Satış Sözleşmesi — kopya olarak mı saklandığı, donmuş şablon sürümünden mi yeniden üretildiği bu aşamanın kararıdır; kopya saklanırsa imha kopyadaki kişisel veriyi de kapsar. Çatının şifreleme anahtar halkası veritabanında tutulur ve onunla birlikte yedeklenir (K-879); eski bir ana dönüşte o arada üretilen veri koruma anahtarı da geri gider — onunla korunan çerezler geçersizleşir (K-975 (4); `05 §7.6.3`; kol 2b'de eklendi — K-1009). Başlangıç değerlerinin alanları, imha kaydının, iz tablosunun ve anahtar halkasının tablosunun tanımı bu aşamanın kararıdır.
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

**Alt bölüm haritası (K-998).** Matris altı oturumda kurulur (K-997; 2. oturum 2a ve 2b'ye bölündü — K-1004): **1a** (2026-10-17) kaynak birimini, envanteri ve kapsama denetimini, aday entity listesini ve Ürün Gereksinimleri'nin `02 §0`–§5'i ile MVP Kapsamı'nın `10 §2`'sinin ileri izlenebilirliğini yazdı; **1b** (2026-10-17) `02 §6`–§13'ün ileri izlenebilirliğini yazdı (K-1001); **1c** (2026-10-17) Kullanıcı Akışları'nın (`03`) ileri izlenebilirliğini yazdı (K-1002); **1d** (2026-10-17) devirleri yazdı (K-1003); **2a** (2026-10-17) geri izlenebilirliği ve gerekçesiz ekleme listesini yazdı, kesilmiş okumanın örnekleminin uygulanmadığını kayda geçirdi ve GAP adaylarını karar için hazırladı (K-1004, K-1005); **2b** (2026-10-17) GAP kararlarını, §1.3'ü ve üst dokümanlara geri beslemeyi yazdı, 2a'nın dört gözlemini kapattı (K-1006…K-1009) — **matris tamamlandı**. Şablonun §1.1, §1.2 ve §1.3 numaraları değişmez; alt düzey aşağıdaki tabloyla sabittir. Bir oturum bir alt bölümü bölmek ya da birleştirmek zorunda kalırsa bu tabloyu ve ona atıf yapan satırları aynı PR'da günceller (K-670'in kalıbı; `checklists/document-stage.md` §4).

| Alt bölüm | İçerik | Oturum | Durum |
|---|---|---|---|
| §1.1.1 | Kaynak envanteri ve sayım — aile × oturum, betik komutları, kapsama denetimi | 1a; her oturum kendi ailesinin sayımını ve denetimini ekler | ✓ (1a, 1b, 1c ve 1d'nin aileleri) |
| §1.1.2 | Aday entity listesi ve "veri doğurmaz" değer kümesi | 1a; 1b, 1c, 1d listenin sonuna aday ekleyebilir | ✓ (sonraki oturum ekleme yaparsa günceller) |
| §1.1.3 | MVP Kapsamı — `10 §2` KP satırları (77) | 1a | ✓ |
| §1.1.4 | Ürün Gereksinimleri — `02 §0`–§2: sözlük terimleri (105; 2b'de 119 — GAP-12 ve GAP-13'ün on dört terimi; workshop 1'de 121 — K-1018), kural numarası taşımayan madde ve metin (42) — 147; 2b'de 161; workshop 1'de 163 | 1a; 2b on dört, workshop 1 iki terim ekledi | ✓ |
| §1.1.5 | Ürün Gereksinimleri — `02 §3` iş kuralları: 257 kalın kural, 19 madde ve metin — 276 | 1a | ✓ |
| §1.1.6 | Ürün Gereksinimleri — `02 §4` zaman kuralları (61) ve §5 durum tanımları (109) — 170 | 1a | ✓ |
| §1.1.7 | Ürün Gereksinimleri — `02 §6` hata ve istisna senaryoları: 6 kalın kural ve bölüm metni (7); 141 hata satırı alt bölüm düzeyinde — hata tablosu taşıyan sekiz alt bölüm, §6.2–§6.9 (8) — ve kayıt, işaret, tarih, sayaç ya da bağ doğuran 66 satır kimliğiyle ayrıca (K-997, K-1001) — 81 | 1b | ✓ |
| §1.1.8 | Ürün Gereksinimleri — `02 §7`–§13: 152 kalın kural ve `02 §13`'ün dört iki düzeyli maddesi (156); L-1…L-9, B-1…B-16, F-1…F-6, P-1…P-49, H-1…H-4 (84); kural numarası taşımayan madde (77) ve metin (13) (K-1000, K-1001) — 330 | 1b | ✓ |
| §1.1.9 | Kullanıcı Akışları — `03 §1` ve §4 satır düzeyinde: numaralı tablo satırı (158 — §1 99, §4 59), kalın numaralı paragraf (16), durum satırı (20), geçiş satırı (16), madde (20) ve metin (17) (K-997, K-1002) — 247 | 1c | ✓ |
| §1.1.10 | Kullanıcı Akışları — `03 §2`, §3, §5, §6, §8, §9, §10: 39 alt bölüm düzeyinde ve bölüm metni (7); 505 satır tam okunur ve `02`'de karşılığı olmayan veri taşıyan satır kimliğiyle ayrıca girer — 1c'de çıkmadı; §7'nin 131 satırı `02 §9`'un B ve F satırlarıyla toplu — 3 alt bölüm satırı ve bölüm metni (K-997, K-1002) — 50 | 1c | ✓ |
| §1.1.11 | Devirler — K-479…K-991'in etki sütununda `06` taşıyan 126 karar satırı (Aşama 1 45, Aşama 2 23, Aşama 3 14, Aşama 4 44); park bloklarının 23 talimat maddesi; gövde cümleleri — dizinlerin 21'i (Aşama 1 on, Aşama 3 iki, Aşama 4 dokuz) ve dizinlerden sonra eklenen iki (K-1003) — 172; 2b'de 173 (K-879 — K-1009) | 1d; 2b bir devir satırı ekledi | ✓ |
| §1.1.12 | GAP adayları (`VA-n`) — karara bağlandı (2b) | 1a açtı; 1b, 1c, 1d ekler; 2a karar için hazırlar; 2b karara bağlar | ✓ — 2 aday karara bağlandı: VA-1 → GAP-12 (K-1006), VA-2 → GAP-13 (K-1007, K-1008); düşen aday yok |
| §1.2 | Geri izlenebilirlik (`Entity.alan` → kaynak); gerekçesiz ekleme listesi; kesilmiş okumanın örneklemi | 2a; 2b günceller | ✓ — 71 aday beslenir, gerekçesiz ekleme yok (K-1005); 2b'de 72 aday, hepsi beslenir (K-1006, K-1008, K-1009) |
| §1.3 | Boşluklar (GAP) ve kararlar; üst dokümanlara geri besleme tablosu (K-994) | 2b; workshop ve yazım geri besleme satırı ekler | ✓ — GAP-12, GAP-13; geri besleme tablosunun ilk satırları (2b); workshop 1'in üç satırı (K-1013, K-1016, K-1018) |

### 1.1 İleri izlenebilirlik (gereksinim/akış → entity.alan)

Her satır şablonun beş sütununu taşır: **Kaynak ID · Kaynak özeti · Entity · Alan · Durum** (K-999). **Kaynak ID** kaynağın kimliğidir: kural `02 §n.m.k`; sözlük terimi `02 §1.2` ve kod karşılığı; kimlikli tablo satırı `02 §4.2` Z-n, `02 §5.4` Sn, `02 §5.5` Ön; durum satırı `02 §5.x` ve durumun kodu; kural numarası taşımayan liste maddesi ya da tablo satırı `02 §n.m` mk — k alt bölümdeki sırasıdır —; alt bölümün kural ve madde dışındaki metni `02 §n.m` metin; kapsam satırı KP-n (K-1000); devir satırı `K-n`, park maddesi `park An-m`, gövde cümlesi `0X §bölüm gN` (K-1003). **Kaynak özeti** satırın özetidir, kuralın evi kaynaktır. **Entity** §1.1.2'nin değer kümesinden bir ya da birkaç değerdir (` · ` ile). **Alan** aday alanın Türkçe tarifidir; sözlükte kod karşılığı olan alan kod adıyla yazılır (`ShippingAddress`), öteki alanlara ad verilmez — alanın adı, türü ve kısıtı §4'ün işidir (VM2, VM4; K-17); "—" satırın kaydın kendisini gösterdiğini söyler. **Durum** üç değerden biridir: **eşlendi** (en az bir aday), **veri doğurmaz** (§1.1.2'nin adıyla yazılmış değerlerinden biri — GAP değildir), **GAP adayı (VA-n)** (§1.1.12'de numarasıyla durur). Atıflar K-669 ve K-701'in okunuşuyla yazılır.

#### 1.1.1 Kaynak envanteri ve sayım

Birim K-997'dir; 1a'nın netleştirmesi K-1000'de, 1b'ninki K-1001'de, 1c'ninki K-1002'de, 1d'ninki K-1003'tedir. Sayımlar `02` v0.76, `03` v0.28, `10` v0.51 ve karar kaydı v0.117 üzerinde, 2026-10-17'de alındı; 1b'nin sayımları aynı `02` sürümünde, 1c'ninki aynı `03` sürümünde, 1d'ninki karar kaydı v0.120, `04` v0.28 ve `05` v0.17 üzerinde, aynı gün. 2b GAP kararlarını geri besledikten sonra bütün kümeleri yeniden koştu — `02` v0.77, `03` v0.29, `04` v0.29, `05` v0.18, `10` v0.52 ve karar kaydı v0.123 üzerinde, aynı gün (K-1006…K-1009). Workshop 1 sözlüğe iki terim ekledi (K-1018); kümeler `02` v0.78, `04` v0.30 ve karar kaydı v0.124 üzerinde yeniden koştu — 1a 686, öteki kümeler eski çıktıyla satır satır aynı (`cmp`).

| Aile | Betiğin saydığı | 1a | Sonraki oturum | Matrise girmeyen |
|---|---|---|---|---|
| `10 §2` KP satırları | 77 | 77 | — | — |
| `02 §1.2` sözlük terimleri | 105; 2b'de 119; workshop 1'de 121 | 105 | 2b ✓: 14 — GAP-12'nin on üç terimi ve GAP-13'ün Adres geçmişi (`02` v0.77; K-1006, K-1008) · workshop 1 ✓: 2 — Ölçü birimi, Net miktar (`02` v0.78; K-1018) | — (K-997'nin gerekçe hücresindeki 97 betik sayımında 105'tir — K-1000) |
| `02` kalın numaralı kurallar | 438 | 280 — §3 257 · §4 5 · §5 18 | 1b ✓: 158 — §6 6 · §7 44 · §8 28 · §9 10 · §10 42 · §12 28; ayrıca `02 §13`'ün iki düzeyli dört maddesi (13.1–13.4) kural kimliğiyle — üç düzeyli kalıp onları saymaz (K-1001) | — |
| `02` kimlikli tablo satırları | 147 — Z 47 · S 11 · Ö 5 · L 9 · B 16 · F 6 · P 49 · H 4 | 63 — Z, S, Ö | 1b ✓: 84 — L 9 · B 16 · F 6 · P 49 · H 4 | — |
| `02 §5` durum satırları | 20 | 20 — §5.1 3 · §5.2 2 · §5.4 6 · §5.5 5 · §5.10 2 · §5.13 2 | — | — |
| `02` kural numarası taşımayan madde | §0–§5: 101 · §6–§13: 77 | 101 | 1b ✓: 77 — §7 4 · §9 7 · §10 32 · §12 34 | — |
| `02` alt bölümün kural ve madde dışındaki metni | §0–§5: 24 · §6–§13: 13 | 24 | 1b ✓: 13 — §6 1 · §7 3 · §8 3 · §9 2 · §10 2 · §11 2 · §12 1 | Kaynak satırları (`*Kaynak:`), şablonun "Ne yazılır" notları, sözlüğün grup başlıkları, dosya sonundaki sürüm dipnotu (`*Shopfolio — Product Requirements …*`) |
| `02 §6` hata satırları | 141 (9 alt bölüm) | — | 1b ✓: hata tablosu taşıyan 8 alt bölüm satırı (§6.2–§6.9; §6.1 hata satırı taşımaz, altı kuralıyla girer) ve kimliğiyle 66 satır — §6.2 11 · §6.3 5 · §6.4 23 · §6.5 8 · §6.6 4 · §6.7 11 · §6.8 1 · §6.9 3 | — (kimliğiyle girmeyen 75 satır alt bölüm satırıyla karşılanır; hepsi tam okundu) |
| `03` numaralı satırlar | 794 | — | 1c ✓: §1 99 · §4 59 satır düzeyinde (158); §2 99 · §3 147 · §5 33 · §6 52 · §8 106 · §9 35 · §10 33 alt bölüm düzeyinde — 39 alt bölüm satırı; §7 131 toplu — 3 alt bölüm satırı (K-1002); kimliğiyle giren satır yok | `03 §0` (yazım konvansiyonu) ve `03 §11` (`02`'ye geri besleme tablosu; `n.m` biçimli) — kimliğiyle girmeyen 636 satır alt bölüm satırıyla karşılanır; hepsi tam okundu |
| `03` numaralı tablo satırı dışındaki birimler (K-1002) | §1 ve §4: 89 · öteki bölümler: 8 | — | 1c ✓: §1 ve §4'te kalın numaralı paragraf 16, durum satırı 20, geçiş satırı 16, madde 20, metin 17 (89); §2, §3, §5–§10'un bölüm metni 8 | Alt bölüm düzeyindeki bölümlerde alt bölümün içindeki metin ve kalın paragraf — alt bölüm satırıyla karşılanır; `03 §0`'ın 21 kalın paragrafı |
| Devir satırları — K-479…K-991, etki sütununda `06` | 126 — Aşama 1 45 · Aşama 2 23 · Aşama 3 14 · Aşama 4 44 | — | 1d ✓: 126 (`K-n`); etki sütununda yalnız `06` taşıyan 44 — iş karar ve gerekçe hücresinden (K-1003); 2b ✓: K-879 — etki sütununa `06` geri işaretle eklendi (K-1009) — 127 | Aşama 5'in kendi kararları (K-992 ve sonrası) devir değildir |
| Park maddeleri | 26 | — | 1d ✓: 23 talimat maddesi — Aşama 1 2 · Aşama 3 10 · Aşama 4 11 (`park An-m`) | 3 dizin maddesi — dizinin kendisine işaret eder, devirleri karar satırlarıyla girer |
| Gövde cümleleri — önceki dokümanların gövdesinde `06` ya da "Veri Modeli" (K-1003) | 23 — `02` 11 · `04` 2 · `05` 10; dizinlerin 21'i ve dizinlerden sonra eklenen 2 | — | 1d ✓: 23 (`0X §bölüm gN`) | Başlık ve sürüm notları, `04 §1.1`–§1.2'nin eşleme hücreleri, `05 §12`; `01`, `03` ve `10`'un gövdesinde eşleşme yok |
| **1a toplamı** | | **670** — 2b'de **684** (sözlüğün on dört terimi); workshop 1'de **686** (iki terim — K-1018) | | |
| **1b toplamı** | | | **411** — beklenen küme 345 (§6 15, §7–§13 330) ve kimliğiyle giren 66 hata satırı | |
| **1c toplamı** | | | **297** — beklenen küme 297 (§1 ve §4 247, öteki bölümler 50); kimliğiyle giren satır yok | |
| **1d toplamı** | | | **172** — beklenen küme 172 (devir 126, park 23, gövde 23); 2b'de **173** (devir 127 — K-879) | |

**1a — bölüm bazında** (670): `10 §2` 77 · `02 §0` 4 · §1 117 (sözlük 105, madde 9, metin 3) · §2 26 (madde 23, metin 3) · §3 276 (kural 257, madde 11, metin 8) · §4 61 (kural 5, Z 47, madde 7, metin 2) · §5 109 (kural 18, durum 20, S ve Ö 16, madde 47, metin 8). §1.1.3 77 · §1.1.4 147 · §1.1.5 276 · §1.1.6 170. **2b:** `02 §1` 131 (sözlük 119) — §1.1.4 161; 1a'nın kümesi 684. **Workshop 1:** `02 §1` 133 (sözlük 121) — §1.1.4 163; 1a'nın kümesi 686 (K-1018).

**1b — bölüm bazında** (411): `02 §6` 81 (kural 6, metin 1, alt bölüm 8, kimliğiyle hata satırı 66) · §7 51 (kural 44, madde 4, metin 3) · §8 40 (kural 28, L 9, metin 3) · §9 41 (kural 10, B 16, F 6, madde 7, metin 2) · §10 76 (kural 42, madde 32, metin 2) · §11 55 (P 49, H 4, metin 2) · §12 63 (kural 28, madde 34, metin 1) · §13 4 (iki düzeyli kural 4). §1.1.7 81 · §1.1.8 330.

**1c — bölüm bazında** (297): `03 §1` 183 (numaralı satır 115 — tablo satırı 99, kalın numaralı paragraf 16 —, durum 20, geçiş S ve Ö 16, madde 17, metin 15) · §4 64 (numaralı satır 59, madde 3, metin 2) · §2 11 (alt bölüm 10, metin 1) · §3 6 (5, 1) · §5 4 (3, 1) · §6 4 (3, 1) · §7 4 (3, 1) · §8 10 (9, 1) · §9 6 (5, 1) · §10 5 (4, 1). §1.1.9 247 · §1.1.10 50.

**1d — aile bazında** (172): devir 126 (Aşama 1 45 — K-480…K-645 · Aşama 2 23 — K-653…K-713 · Aşama 3 14 — K-736…K-830 · Aşama 4 44 — K-870…K-988; etki sütununda yalnız `06` taşıyan 44) · park 23 (Aşama 1 2 · Aşama 3 10 · Aşama 4 11) · gövde 23 (`02` 11 — §1.1 5, §3.31 2, §4.1 1, §5 2, §11 1 · `04` 2 · `05` 10). §1.1.11 172.

**Betik komutları** (Git Bash; depo kökünden). Sayımlar yukarıdaki sayılarla aynı çıkmalı; kapsama denetimleri — 1a'nın üçü, 1b'nin üçü, 1c'nin üçü, 1d'nin üçü ve başlığa bağlılığın beşi —, konusuz park satırı denetimi, değer kümesi denetimi, 2a'nın üç denetimi ve 2b'nin denetimi boş dönmelidir (sayılar 2b'nin yeniden koşumunundur — K-1006…K-1009):

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
# devir satırları — K-479…K-991, etki sütununda 06 (127 — 1d 126; 2b K-879, K-1009)
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
# beklenen küme — 1c (297): 03 §1 ve §4 satır düzeyinde, öteki bölümler alt bölüm düzeyinde — birim kuralı K-997, K-1002
# §1 ve §4: numaralı tablo satırı ve kalın numaralı paragraf (03 §n.m.k); durum satırı (bağlam + kod — tablonun başlığı "Durum");
# geçiş satırı (bağlam + S/Ö kimliği); kimliksiz tablo satırı ve liste maddesi (bağlam + mk); metin (bağlam başına bir) —
# bağlam: alt bölümün içinde son kalın numaralı paragraf, yoksa alt bölüm. Öteki bölümler: ### alt bölüm (03 §n.m) ve bölüm metni (03 §n metin).
# sınır "## 11." başlığıdır (03 §11 geri besleme tablosu); 03 §0 matrise girmez
exp03() {
  awk '
    /^## 11\. /{exit}
    /^## 1\. /{on=1}
    !on{pt=0; next}
    /^## [0-9]+\. /{s=$2; sub(/\.$/,"",s); x=s; pt=0; d=(s=="1"||s=="4"); next}
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
# 03'ün kimliğiyle girebilen satırları — §1 ve §4 dışındaki numaralı satırlar (636) — 1c'nin isteğe bağlı kümesi (K-997)
opt03() { grep -oE '^\| [0-9]+(\.[0-9]+){2,} \|' Docs/03_USER_FLOWS.md | sed -E 's/^\| /03 §/; s/ \|$//' | grep -vE '^03 §(1|4|11)\.'; }
# bu dokümanın matris satırları — $1 alt bölüm süzgeci: 1a '[3-6]' · 1b '[78]' · 1c '(9|10)' · 1d '11' · tek alt bölüm '7'
mat() {
  awk -v h="$1" '$0 ~ ("^#### 1\\.1\\." h " "){f=1; next} /^#### /{f=0}
       f && /^\| (`0[1-5] |KP-|K-[0-9]|park A)/{split($0,c,"|"); x=c[2]; gsub(/`/,"",x); gsub(/^ +| +$/,"",x); gsub(/ +/," ",x); print x}' Docs/06_DATA_MODEL.md
}
# 1a — 686 · 686 (1a 670; 2b sözlüğün on dört terimi — K-1006, K-1008; workshop 1 iki terim — K-1018)
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
# 1c — 297 · 297 (kimliğiyle giren satır yok)
exp03 | wc -l; mat '(9|10)' | wc -l
comm -23 <(exp03 | sort) <(mat '(9|10)' | sort)                      # eksik — boş dönmeli
comm -13 <(exp03 | sort) <(mat '(9|10)' | sort) | grep -vxF -f <(opt03)  # fazla — boş dönmeli
mat '(9|10)' | sort | uniq -d                                       # yinelenen — boş dönmeli
mat 9 | grep -vE '^03 §(1|4)([. ]|$)'                               # §1.1.9 yalnız 03 §1 ve §4 — boş dönmeli
mat 10 | grep -vE '^03 §(2|3|5|6|7|8|9|10)([. ]|$)'                 # §1.1.10 yalnız öteki bölümler — boş dönmeli
exp03 | awk '{split($2,a,"."); sub(/§/,"",a[1]); t=($3 ~ /^m[0-9]+$/)?"madde":($3=="metin")?"metin":($3 ~ /^(S|Ö)[0-9]+$/)?"geçiş":($3!="")?"durum":(a[1] ~ /^(1|4)$/)?"satır":"alt bölüm"; c[a[1]" "t]++} END{for(k in c) print k, c[k]}' | sort -V
grep -oE '^\*\*[0-9]+\.[0-9]+\.[0-9]+ ' Docs/03_USER_FLOWS.md | grep -cE '^\*\*(1|4)\.'  # kalın numaralı paragraf — 16
mat 10 | grep -cxF -f <(opt03)                                      # kimliğiyle giren satır — 0
# beklenen küme — 1d (172): devir satırları (126), park maddeleri (23), gövde cümleleri (23) — birim kuralı K-1003
# devir: K-479…K-991, etki sütununda 06 — Kaynak ID K-n
dev() {
  awk -F'|' '/^\| K-[0-9]+ /{k=$2; gsub(/ /,"",k); n=substr(k,3)+0; if (n>=479 && n<=991 && $7 ~ /(^|[ ·(,`])06([ ,(·)`]|$)/) print k}' \
    Docs/PRODUCT_DISCOVERY_STATUS.md | sort -u
}
# park: dört bloğun talimat maddeleri — park An-m (n aşama, m bloktaki sıra); dizin maddeleri girmez
park() {
  awk '/^> \*\*Aşama [0-9]+.(den|ten) park edilen girdiler/{a=$0; sub(/^> \*\*Aşama /,"",a); sub(/[^0-9].*$/,"",a); m=0; next}
       /^## /{exit}
       a!="" && /^> - \*\*/ && !/^> - \*\*Dizin/{m++; print "park A" a "-" m}' Docs/06_DATA_MODEL.md
}
# gövde: önceki dokümanların gövdesinde `06`'yı ya da "Veri Modeli"ni anan paragraf ya da tablo satırı — 0X §bölüm gN
# sınır: 01, 02, 03, 10 ilk "## n." başlığından sona (başlık ve sürüm notları dışarıda); 04 "### 1.3"ten (§1.1–§1.2'nin eşleme
# hücreleri dışarıda); 05 "## 0."dan "## 12."ye; bölüm son başlıktır — 04'te tablo satırının kimliği, 05'te kalın numaralı paragrafın numarası
gov() {
  for f in 01_PROJECT_VISION 02_PRODUCT_REQUIREMENTS 03_USER_FLOWS 10_MVP_SCOPE; do
    awk -v d="${f%%_*}" '/^## [0-9]+\. /{on=1} !on{next} /^#+ [0-9]/{s=$2; sub(/\.$/,"",s); next} /^\*Shopfolio — /{next}
         /`06`|Veri Modeli/{g[s]++; print d " §" s " g" g[s]}' "Docs/$f.md"
  done
  awk '/^### 1\.3 /{on=1} !on{next} /^#+ [0-9]/{s=$2; sub(/\.$/,"",s); next} /^\*Shopfolio — /{next}
       /`06`|Veri Modeli/{x=s; if (match($0,/^\| [0-9]+(\.[0-9]+)+ \|/)) x=substr($0,3,RLENGTH-4); g[x]++; print "04 " x " g" g[x]}' Docs/04_UI_SPECS.md
  awk '/^## 0\. /{on=1} /^## 12\. /{on=0} !on{next} /^#+ [0-9]/{s=$2; sub(/\.$/,"",s); next}
       /`06`|Veri Modeli/{x=s; if (match($0,/^\*\*[0-9]+\.[0-9]+\.[0-9]+ /)) {x=substr($0,3,RLENGTH-3); sub(/ $/,"",x)}; g[x]++; print "05 §" x " g" g[x]}' Docs/05_TECHNICAL_ARCHITECTURE.md
}
e1d() { dev; park; gov; }
# 1d — 173 · 173 (1d 172; 2b K-879 — K-1009)
e1d | wc -l; mat 11 | wc -l
comm -23 <(e1d | sort) <(mat 11 | sort)                             # eksik — boş dönmeli
comm -13 <(e1d | sort) <(mat 11 | sort)                             # fazla — boş dönmeli
mat 11 | sort | uniq -d                                             # yinelenen — boş dönmeli
mat 11 | grep -vE '^(K-[0-9]+|park A[0-9]+-[0-9]+|0[1-5] .+ g[0-9]+)$'   # §1.1.11 yalnız 1d'nin birimleri — boş dönmeli
e1d | awk '{t=($1 ~ /^K-/)?"devir":($1=="park")?"park":"gövde " $1; c[t]++} END{for(k in c) print k, c[k]}' | sort
awk '/^#### 1\.1\.11 /{f=1; next} /^#### /{f=0} f && /^\| park A/ && !/VM[0-9]-[0-9]/{split($0,c,"|"); print c[2]}' Docs/06_DATA_MODEL.md  # konusuz park satırı — boş dönmeli
# "Entity" sütunu §1.1.2'nin değer kümesinin dışına çıkmıyor (boş dönmeli)
awk '/^#### 1\.1\.([3-9]|1[01]) /{f=1; next} /^#### /{f=0} f && /^\| (`0[1-5] |KP-|K-[0-9]|park A)/{split($0,c,"|"); n=split(c[4],e," · "); for(i=1;i<=n;i++){x=e[i]; gsub(/^ +| +$/,"",x); print x}}' Docs/06_DATA_MODEL.md | sort -u \
  | grep -vxF -f <(awk '/^#### 1\.1\.2 /{f=1; next} /^#### /{f=0} f && /^\| [0-9]+ \|/{split($0,c,"|"); x=c[3]; gsub(/^ +| +$/,"",x); print x} f && /^\| (veri doğurmaz|yoktur)/{split($0,c,"|"); x=c[2]; gsub(/^ +| +$/,"",x); print x}' Docs/06_DATA_MODEL.md)
# 2a — geri izlenebilirlik (K-1005): §1.1.2'nin adayları (71) ve §1.1.3–§1.1.11'in satırlarının aday başına dökümü
aday() { awk '/^#### 1\.1\.2 /{f=1; next} /^#### /{f=0} f && /^\| [0-9]+ \|/{split($0,c,"|"); x=c[3]; gsub(/^ +| +$/,"",x); print x}' Docs/06_DATA_MODEL.md; }
# "aday<TAB>kaynak kimliği" — yalnız §1.1.2'nin adayları; "veri doğurmaz" değerleri dökülmez
geri() {
  awk -F'|' 'NR==FNR{a[$0]=1; next}
       /^#### 1\.1\.([3-9]|1[01]) /{f=1; next} /^#### /{f=0}
       f && /^\| (`0[1-5] |KP-|K-[0-9]|park A)/{x=$2; gsub(/`/,"",x); gsub(/^ +| +$/,"",x); gsub(/ +/," ",x)
         n=split($4,e," · "); for(i=1;i<=n;i++){y=e[i]; gsub(/^ +| +$/,"",y); if (y in a) print y "\t" x}}' <(aday) Docs/06_DATA_MODEL.md
}
# §1.2'nin satırları — "aday<TAB>satır sayısı<TAB>durum"
geri12() {
  awk -F'|' '/^### 1\.2 /{f=1; next} /^### /{f=0}
       f && /^\| / && $3 ~ /^ [0-9]+ satır/{y=$2; sub(/ — .*/,"",y); gsub(/^ +| +$/,"",y); n=$3; sub(/^ /,"",n); sub(/ satır.*/,"",n)
         s=$4; gsub(/^ +| +$/,"",s); print y "\t" n "\t" s}' Docs/06_DATA_MODEL.md
}
# 2a — 72 · 2900 · 72 (2a 71 · 2858 · 71; 2b 2898 — K-1006, K-1008, K-1009; workshop 1 2900 — K-1018)
aday | wc -l; geri | wc -l; geri12 | wc -l
comm -23 <(aday | sort) <(geri | cut -f1 | sort -u)                 # kaynak satırı olmayan aday (gerekçesiz ekleme) — boş dönmeli
diff <(geri | cut -f1 | sort | uniq -c | awk '{c=$1; sub(/^ *[0-9]+ /,""); print $0 "\t" c}' | sort) <(geri12 | cut -f1,2 | sort)   # §1.2'nin satır sayıları — boş dönmeli
geri12 | cut -f3 | sort | uniq -c                                   # durum: beslenir 55 · biçimi park satırından 17 (2a: 54 · 16 · 1)
comm -23 <(geri12 | awk -F'\t' '$3 ~ /park satırından/{print $1}' | sort) <(geri | awk -F'\t' '$2 ~ /^park A/{print $1}' | sort -u)   # "biçimi park satırından" deyip park satırı olmayan — boş dönmeli
# 2b — karara bağlanmamış GAP adayı satırı (boş dönmeli)
awk '/^#### 1\.1\.([3-9]|1[01]) /{f=1; next} /^#### /{f=0} f && /GAP adayı \(VA-/' Docs/06_DATA_MODEL.md
```

**Yanlış pozitif ve kapsam denetimi (1a).** Kural kalıbı yalnız satır başındaki kalın `n.m.k`'yı yakalar; dört düzeyli kalın kural yoktur ve kuralın devamında liste maddesi yoktur — `02 §3`'ün maddeleri yalnız bölüm girişindedir (betikle sayıldı). KP kalıbı yalnız ilk hücreyi yakalar. Kimlikli satır kalıbı yalnız ilk hücresi kimlik olan satırı yakalar; grup başlığı satırları (`| | **…** |`) ve tablo başlıkları birim değildir. Sözlüğün 105 satırının hepsi bir kod karşılığı taşır (18'i † ile); yinelenen kod yoktur. Durum satırı ikinci hücresi tek bir kod olan satırdır; `02 §5.7`'nin hat tablosu durum taşımaz, maddesiyle girer. Matrisin 1a satırlarında yinelenen kaynak kimliği yoktur; beklenen kümeyle karşılaştırmada eksik ve fazla sıfırdır (2026-10-17). **Betik iki yönlü sınandı** (`audit` skill'i, "Koşum biçimi" 4. madde): matristen bir satır çıkarıldığında denetim o kimliği eksik, bir satır ikilendiğinde yinelenen, kaynakta olmayan bir kimlik eklendiğinde fazla diye döndürdü; değer kümesinin dışında bir "Entity" değeri yazıldığında son denetim o değeri döndürdü.

**Yanlış pozitif ve kapsam denetimi (1b).** `exp()`'in sınırı `## 6.` başlığından `## 14.` başlığına genişletildi — Ürün Gereksinimleri'nde `## 14.` yoktur, betik dosyanın sonuna kadar okur ve dosya sonundaki sürüm dipnotunu birim saymaz; 1a ile 1b aynı işlevden, bölüm süzgeciyle (`e1a`, `e1b`) okunur ve genişletilmiş işlevin 1a parçası eski işlevin çıktısıyla satır satır aynıdır (670; 2026-10-17). Üç ek kural: (1) `02 §6`'nın hata satırı (`| 6.n.k |`) alt bölüm satırını doğurur (`02 §6.n`) — kimliğiyle giren satır `opt()`'un kümesinden gelir ve fazla sayılmaz; (2) `02 §13`'ün kalın maddeleri iki düzeylidir (`**13.1 …**`) ve kural kimliğiyle girer; (3) `02 §10.1.2`, §10.5.1 ve §12.5'in tablo başlıkları ikinci hücreleriyle tanınır — ilk hücreleri ("Alan", "Sipariş", "Yükümlülük") sözlüğün "Sipariş" terimiyle çakışır; ilk denemede ilk hücreyle tanıyan kural sözlüğün `Order` satırını düşürdü ve 1a'nın kümesi 669'a indi — denetim farkı yakaladı. `02 §6`–§13'te dört düzeyli kalın kural yoktur; kimlikli satır kalıbı L, B, F, P, H ailesini yakalar, `02 §11.2`'nin grup başlığı satırları (`| | **…** |`) birim değildir; `02 §11.1`'in kaldırılmış P-2 ve P-11 satırları kimliğiyle girer. Matrisin 1b satırlarında yinelenen kaynak kimliği yoktur; beklenen kümeyle karşılaştırmada eksik ve fazla sıfırdır; §1.1.7 yalnız `02 §6`'yı, §1.1.8 yalnız `02 §7`–§13'ü taşır (2026-10-17). **Betik iki yönlü sınandı:** §1.1.8'den bir satır çıkarıldığında eksik, bir satır ikilendiğinde yinelenen, kaynakta olmayan bir hata satırı (`02 §6.2.99`) eklendiğinde fazla, §1.1.8'e bir `02 §6` satırı taşındığında başlığa bağlılık denetimi o satırı döndürdü.

**Yanlış pozitif ve kapsam denetimi (1c).** `exp03()` `exp()`'in kalıbıyla yazılmış ayrı bir işlevdir; `exp()`'e dokunulmadı. `mat()`'in satır kalıbı `03 §` satırlarını da okuyacak biçimde genişletildi (`0[23] §`) ve değer kümesi denetimi §1.1.9 ile §1.1.10'u kapsar; **önceki oturumların kümeleri eski çıktıyla satır satır aynıdır** — `e1a` (670), `e1b` (345), `opt` (141), `mat '[3-6]'` (670) ve `mat '[78]'` (411) genişletmeden önce ve sonra dosyaya yazılıp `cmp` ile karşılaştırıldı; betik bloğunun 1a ve 1b parçasının çıktısı da aynıdır (2026-10-17). Birim kuralı K-1002'dedir: K-997'nin 99 ve 59'u yalnız `| n.m.k |` biçimli tablo satırlarının sayımıdır; `03 §1`'in on altı kalın numaralı paragrafı (1.4.1…1.9.2 — biri, 1.7.4, kuralın kendisidir), durum tabloları (20 durum — kodu `02 §1.2`'den birebir), S ve Ö geçiş tabloları (16), kimliksiz tablolar (§1.1'in envanteri, §1.4.1'in birleşim tablosu), liste maddeleri (§1.4.3, §1.7.2, §4'ün girişi) ve bağlamın metni bu kalıbın dışındadır ve birim olarak girer. Durum satırı tablonun başlığı "Durum" olan ve ilk hücresi kodla biten satırdır — §1.1'in envanteri de kodla biter ama başlığı "Varlık"tır ve madde olarak girer; aynı kod bir alt bölümde iki tabloda geçtiği için (`Draft` §1.8.1 ve §1.8.4'te, `Open` §1.9.1 ve §1.9.2'de) bağlam kalın paragrafın numarasıdır. Tablo başlığı tablonun ilk satırı olarak tanınır, hücre içeriğiyle değil — 1b'nin `Order` satırını düşüren kalıp hatası (K-1001) burada doğmaz. `03 §4.1`'in grup başlığı satırları (`| | **…** |`) birim değildir. Alt bölüm düzeyindeki bölümlerde `###` başlığı birimdir, `####` alt başlıkları ve satırları onun içindedir; bölümün ilk alt bölümden önceki metni ayrı birimdir. Matrisin 1c satırlarında yinelenen kaynak kimliği yoktur; beklenen kümeyle karşılaştırmada eksik ve fazla sıfırdır; §1.1.9 yalnız `03 §1` ve §4'ü, §1.1.10 yalnız öteki bölümleri taşır; kimliğiyle giren satır yoktur (2026-10-17). **Betik iki yönlü sınandı:** belgenin bir kopyasında §1.1.9'dan bir satır çıkarıldığında eksik, bir satır ikilendiğinde yinelenen, kaynakta olmayan bir kimlik (`03 §2.1.99`) eklendiğinde fazla, §1.1.10'a bir `03 §1` satırı taşındığında başlığa bağlılık denetimi, değer kümesinin dışında bir "Entity" yazıldığında değer kümesi denetimi o satırı döndürdü; isteğe bağlı kümeden bir satırın (`03 §2.1.1`) eklenmesi fazla sayılmadı.

**Yanlış pozitif ve kapsam denetimi (1d).** Birim kuralı K-1003'tedir. `dev()` açılışın sayım komutunun işlev biçimidir (126 — karar kaydı §14.2'nin betiğiyle aynı küme). `park()` blok başlığını aşama numarasıyla tanır; ilk denemede başlık kalıbı yalnız "'den" ekini aradı, "'ten" alan Aşama 3 ve Aşama 4 başlıklarını görmedi ve 21 maddeyi Aşama 2'nin kimliğiyle yazdı — sayım 23 tuttuğu hâlde kimlikler yanlıştı; kimlikler blok blok okunarak düzeltildi ve kalıp iki eki de alır. `gov()` `06`'yı ya da "Veri Modeli"ni anan satırı sayar: aynı paragraftaki iki anma bir birimdir (`05 §7.4.3`); Arayüz Tanımları'nın başlık notu ("veri modeli yazılmaz") ve dokümanların sürüm notları sınırın dışındadır; Teknik Mimari'nin sınırı dizininkidir — §0'dan önceki başlık notu ve §12 dışarıda (`PHASE4_CONFLICT_SCAN.md` §6.2). Dizinin dokuz `05` maddesi bugün de yerindedir ve K-982'nin cümlesi (`05 §3.5.2`) onlara eklenir; Ürün Gereksinimleri'nin on cümlesine K-977'nin cümlesi (`02 §1.1` m4) eklenir — iki ekleme dizinler alındıktan sonra, Aşama 4'ün kapanışında doğdu. `mat()`'in satır kalıbı 1d'nin kimliklerini de okuyacak biçimde genişletildi (`0[1-5] `, `K-`, `park A`) ve değer kümesi denetimi §1.1.11'i kapsar; **önceki oturumların kümeleri eski çıktıyla satır satır aynıdır** — `e1a` (670), `e1b` (345), `opt` (141), `exp03` (297), `opt03` (636), `mat '[3-6]'` (670), `mat '[78]'` (411) ve `mat '(9|10)'` (297) genişletmeden önce ve sonra dosyaya yazılıp `cmp` ile karşılaştırıldı; betik bloğunun 1a–1c parçasının çıktısı da aynıdır (2026-10-17). Matrisin 1d satırlarında yinelenen kaynak kimliği yoktur; beklenen kümeyle karşılaştırmada eksik ve fazla sıfırdır; §1.1.11 yalnız 1d'nin birimlerini taşır; konusuz park satırı yoktur. **Betik iki yönlü sınandı:** belgenin bir kopyasında §1.1.11'den bir satır çıkarıldığında eksik, bir satır ikilendiğinde yinelenen, karar kaydında olmayan bir kimlik (`K-1999`) eklendiğinde fazla, §1.1.11'e bir `02 §3` satırı taşındığında başlığa bağlılık denetimi, bir park satırının konu atfı silindiğinde konusuz park denetimi, değer kümesinin dışında bir "Entity" yazıldığında değer kümesi denetimi o satırı döndürdü.

**Yanlış pozitif ve kapsam denetimi (2a).** Birim ve biçim K-1005'tedir. `aday()` §1.1.2'nin "Aday" sütununu okur (71); `geri()` §1.1.3–§1.1.11'in satırlarını `mat()`'in satır kalıbıyla okur ve "Entity" hücresini ` · ` ile böler — yalnız §1.1.2'nin adayları dökülür, "veri doğurmaz" değerleri dökülmez; bir satır birden çok adayı besliyorsa her aday için bir çift yazılır (2.858 çift, 1.550 satırın 1.260'ı en az bir aday taşır). `geri12()` §1.2'nin tablosunu okur: aday "Entity.alan" hücresinin " — "dan önceki kısmı, satır sayısı "Beslendiği kaynak" hücresinin başındaki sayıdır. Üç denetim: kaynak satırı olmayan aday (gerekçesiz ekleme), tablonun satır sayısı ile betiğin sayısının farkı ve "biçimi park satırından" deyip park satırı olmayan aday — üçü de boştur. `aday()`, `geri()` ve `geri12()` yeni işlevdir, önceki işlevlere dokunulmadı; **önceki oturumların kümeleri eski çıktıyla satır satır aynıdır** — `e1a` (670), `e1b` (345), `opt` (141), `exp03` (297), `opt03` (636), `e1d` (172), `mat '[3-6]'` (670), `mat '[78]'` (411), `mat '(9|10)'` (297) ve `mat 11` (172) eklemeden önce ve sonra dosyaya yazılıp `cmp` ile karşılaştırıldı; betik bloğunun 1a–1d parçasının çıktısı da aynıdır (2026-10-17). **Betik iki yönlü sınandı:** belgenin bir kopyasında K-975'in satırı silindiğinde ilk denetim *anahtar halkası*'nı, ikinci denetim beş adayın sayı farkını döndürdü; §1.2'de bir sayı değiştirildiğinde ikinci denetim o adayı, §1.1.2'ye kaynağı olmayan bir aday eklendiğinde ilk denetim o adayı, park satırı olmayan bir adayın durumu "biçimi park satırından" yazıldığında üçüncü denetim o adayı döndürdü.

**Yanlış pozitif ve kapsam denetimi (2b).** 2b yeni işlev yazmadı; betik bloğuna yorumlar ve bir denetim satırı eklendi (karara bağlanmamış GAP adayı satırı — boş). GAP kararlarının geri beslemesi kaynak dokümanları değiştirdiği için kümeler geri beslemeden önce ve sonra dosyaya yazılıp karşılaştırıldı (öğrenim adayı 4'ün denetimi): `e1b` (345), `opt` (141), `exp03` (297), `opt03` (636), `park` (23), `gov` (23), `mat '[78]'` (411) ve `mat '(9|10)'` (297) **satır satır aynıdır** (`cmp`) — geri beslenen metin yeni birim açmadı ve Ürün Gereksinimleri'nin, Kullanıcı Akışları'nın, Arayüz Tanımları'nın ve Teknik Mimari'nin gövdesinde bu dokümanı anan yeni cümle doğmadı (anma yalnız başlık notlarında ve dipnotlarda). Değişen üç küme beklenen farkı taşır: `e1a` ve `mat '[3-6]'` 670 → 684 — fark yalnız sözlüğün on dört yeni terimidir (`02 §1.2` Image … OrderAddressHistory); `dev`, `e1d` ve `mat 11` 172 → 173 — fark yalnız K-879'dur; `aday()` 71 → 72 — fark yalnız OrderAddressHistory'dir; `geri()` 2.858 → 2.898 — yedi adın yeni adıyla eşlenmesinden sonra 2a'nın çiftlerinin hepsi yerindedir, 40 yeni çift sözlüğün yeni satırlarından (19), K-879'dan ve `park A4-3`'ten (3) ve adres geçmişinin eklendiği satırlardan (18) gelir. Kapsama denetimlerinin hepsi boştur (2026-10-17). **Betik iki yönlü sınandı:** belgenin bir kopyasında §1.1.4'ten `02 §1.2` ItemRemoval satırı çıkarıldığında 1a'nın eksik denetimi, bir satırın durumu yeniden "GAP adayı (VA-1)" yazıldığında 2b'nin denetimi o satırı döndürdü.

**Okuma derinliği (checklist §2).** 1a her birimin metnini betikle **satırın tamamı** olarak çıkardı ve tam okudu — sözlük satırları tanım hücresiyle, KP satırları "Neden MVP'de" ve "Kaynak" hücreleriyle, Z satırları altı hücresiyle; hücre başına kesilerek okunan satır yoktur; checklist §2'nin kesilmiş okuma örneklemi bu yüzden 1a'nın satırlarına uygulanmaz. `02 §3`'ün kural dışı gövdesi, §4.2'nin grup başlıkları ve bölüm girişleri de okundu. Tam okunan yalnız matrisin kaynağıdır: `02 §6`–§13, `03` ve devirler kendi oturumlarındadır. **1b** `02 §6`–§13'ü (satır 1339–2189; 288 KB) betikle on üç parçaya bölerek ve hiçbir satırı kesmeden tam okudu — en uzun satır 6.500 karakterdir; hata satırları üç hücresiyle, L satırları ölçüm ekseni ve aşıldığında hücreleriyle, B satırları "E-posta neyi taşır" hücresiyle, P satırları altı hücresiyle, `02 §12.5`'in satırları dört hücresiyle okundu; kimliğiyle girmeyen 75 hata satırı da okundu ve verisi alt bölüm satırında ya da kurala bağlandığı `02 §3` ve §7 satırındadır. GAP adayının dayandığı yerler (`02 §3.23.3`, §10.3.1, §10.4.2, §12.2.7) ve Kullanıcı Akışları ile Arayüz Tanımları'nda "eski adres" taraması ayrıca okundu (VA-2; K-739). Kesilmiş okuma yoktur. **1c** Kullanıcı Akışları'nın `03 §1`–§10'unu (satır 136–1753; 380 KB) bölüm bölüm, hiçbir satırı kesmeden tam okudu — bu aralıkta en uzun satır 1.911 karakterdir; `03 §0` yazım konvansiyonu olarak ayrıca okundu, `03 §11` matrise girmez ve okunmadı. §1 ve §4'ün her birimi; §2, §3, §5, §6, §8, §9, §10'un 505 ve §7'nin 131 satırı adım, sistem, durum, bildirim ve kaynak hücreleriyle okundu ve her birinin veriye dokunan ayrıntısı gösterdiği `02` kuralının satırında (§1.1.4–§1.1.8) arandı; aday satırlar `02`'nin tam metnine karşı ayrıca okundu (`02 §10.4.11`, §7.5.3, §3.22.4). `03 §1`–§10'da anılan K numaralarından `02`'de geçmeyenler betikle bulundu ve okundu: beşi Kullanıcı Akışları'nın yazım konvansiyonudur (K-650, K-651, K-682, K-689, K-702); K-808'in kuralı `02 §7.5.3`'te yazılıdır; K-826 sipariş sayfasının gösterdiğidir — geri ödemenin tarihi, tutarı ve yolu `02 §5.5`'in Refund satırındadır (betiğin `\bK-` kalıbı `10`'un ÖK- kimliklerinden üç parça da döndürür). Kesilmiş okuma yoktur. **1d** devirlerin 126 karar satırını konu, karar, gerekçe ve etki hücreleriyle (338 KB, on parça), park bloklarının 26 maddesini, dört dizinin `06` başlıklarını ve 23 gövde cümlesinin paragrafını satır kesmeden tam okudu; devrin işinin karşılandığı `02` ve `03` satırları matrisin metninde arandı, gösterilen kimliklerin varlığı betikle denetlendi. Kesilmiş okuma yoktur. **2a** geri yönde §1.1.3–§1.1.11'in satırlarını betikle aday başına topladı ve her adayın "Sorumluluk" hücresindeki her alan grubu için gösterdiği kaynak satırını "Alan" hücresinin tamamıyla okudu; 1a–1d'nin kesilmiş okuma olmadığını yazan dört kaydı okudu (§1.2). GAP adaylarının dayandığı kaynaklar tam okundu — `02`, `03`, `04`, `05` ve `10`'un bölümleri ve K-527, K-585, K-586, K-601, K-617, K-645, K-675, K-977, K-988'in dört hücresi; liste §1.1.12'nin hazırlık bloğundadır (K-739). Kesilmiş okuma yoktur. **2b** geri beslemenin yüzlerini tam okudu — sözlüğün 105 terimi, kararın değiştirdiği ve kapalı listelerini taradığı Ürün Gereksinimleri bölümleri, Kullanıcı Akışları'nın, Arayüz Tanımları'nın ve Teknik Mimari'nin satırları ve K-879 ile K-975'in dört hücresi (liste karar kaydı §14.1'in okuma maddesinde); matrisin satırlarını yeniden okumadı — değişen satırlar GAP kararının dokunduğu satırlardır ve her biri tam metniyle yeniden yazıldı. Kesilmiş okuma yoktur.

#### 1.1.2 Aday entity listesi ve "veri doğurmaz" değer kümesi

Bu liste matrisin "Entity" sütununun değer kümesidir; **nihai entity envanteri değildir** — envanter §3'te, yazım turunda doğar (VM3-01…VM3-03) ve bir adayı bölebilir, birleştirebilir ya da bir alan grubuna çevirebilir: hangi adayın ayrı tablo, hangisinin bir kaydın alan grubu olacağı §4'ün kararıdır (VM4). Aday kod adıyla anılır (K-995); sözlükte (`02 §1.2`) terimi olmayan aday **italik Türkçe adıyla** yazılır ve kod adı taşımaz — teknik kavramın kod adı yazım turunda `05`'teki adından verilir (VM3-03; K-977, K-988), ürüne görünen kaydın adı sözlüğe girer (GAP-12 — 2b yedi italik adaya sözlüğün kod adını verdi; K-1006), bağ ve ayar kayıtlarının adı adlandırma kuralıyla türer (K-1012). **"Kaynak" sütunu adayın doğduğu yeri gösterir,** besleyen satırları değil — 24 (*ödeme kaydı*) ve 71 (*anahtar halkası*) Teknik Mimari'nin maddesini, 61 (*gönderim kaydı*) adlandırma kuralını (`02 §1.1` m4) gösterir; adayı besleyen satırlar §1.2'dedir (2a'nın 3. gözlemi; K-1009). 1b, 1c ve 1d yeni adayı listenin **sonuna** ekler ve bu tabloyu günceller (K-999). "Konu" sütunu adayın alanlarını kuracak workshop ya da yazım konusudur (karar kaydı §14.3).

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
| 12 | Image | `02 §3.11`, §3.27.15 | Ürünün, varyantın ve içerik kaydının görseli: sıra, ana görsel işareti, alternatif metin | VM4-15 |
| 13 | DigitalFile | `02 §3.12`; park (K-883) | Dijital ürünün yüklenen dosyası; varyantın o anki dosyası | VM4-15 |
| 14 | Discount | `02 §1.2`, §3.9 | Ürün düzleminde tarihli yüzdesel indirim | VM4-03 |
| 15 | PriceHistory | `02 §3.8.5`, §3.9.3 | Varyantın uygulanmış fiyatları ve başlangıç anları — referans fiyat ve fiyat tarihi | VM4-03 |
| 16 | Coupon | `02 §1.2`, §3.10 | Kupon: kod, tür, değer, tarih aralığı, kullanım adedi, haklar | VM4-04 |
| 17 | StockReservation | `02 §1.2`, §3.17.3 | Siparişin ayırdığı stok, kontenjan ve kupon hakkı | VM4-04 |
| 18 | Cart | `02 §1.2`, §3.16 | Üyenin ya da misafir tarayıcısının sepeti | VM4-05 |
| 19 | CartItem | `02 §1.2`, §3.16.5 | Sepetteki varyant, adet ve ilk eklendiği fiyat | VM4-05 |
| 20 | Order | `02 §1.2`, §3.17–§3.25, §5.3–§5.6 | Sipariş: numara, iki eksen, donan adresler, kimlik, sürümler, IBAN, tutarlar, kargo, zaman alanları | VM4-06 |
| 21 | OrderItem | `02 §1.2`, §3.23, §5.8 | Siparişin kalemi: donan değerler, teslim işareti, indirme hakkı ve sayacı | VM4-06 |
| 22 | OrderEmailHistory | `02 §1.2`; `02 §10.4.11` | Misafir siparişinin e-posta düzeltmelerinin eski ve yeni değeri | VM4-13 |
| 23 | CheckboxAcceptance | `02 §3.24.6` | Dijital ve hizmet kalemi için işaretlenen kutuların kaydı | VM4-11 |
| 24 | *ödeme kaydı* | `02 §1.2` (`Failed`), §5.5; `05 §4.5.3` | Siparişin ödemesi, sağlayıcının işlem kimliği ve geç gelen ödeme | VM4-08 |
| 25 | Cancellation | `02 §1.2`, §5.8 | Sipariş, kalem ya da adet iptali: adet, sebep, türü | VM4-07 |
| 26 | ItemRemoval | `02 §5.8` | Yöneticinin kalemi çıkarması ya da adedini azaltması | VM4-07 |
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
| 42 | PendingEmailChange | `02 §3.13.14` | Doğrulanmayı bekleyen yeni e-posta adresi | VM4-09 |
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
| 59 | LegalTextVersion | `02 §3.33.3` | Dört metnin sürümleri; siparişe donan sürüm | VM4-11 |
| 60 | ContactRequest | `02 §1.2`, §3.32 | İletişim talebi: konu tipi, durum, son kapatıldığı an | VM4-12 |
| 61 | *gönderim kaydı* | `02 §1.1` (m4); Z-34, Z-43; park (K-892) | Her e-postanın gönderim satırı — `Notification` kavramının kaydı | VM4-13 |
| 62 | *iş kuyruğu* | park (K-891) | Dış yan etkilerin iş satırları | VM4-13 |
| 63 | *kanal hâli kaydı* | park (K-913) | E-posta göndericisinin tek satırlık kanal hâli | VM4-13 |
| 64 | *gelen bildirim kaydı* | park (K-893, K-954) | Sağlayıcının imzası doğru bildirimleri | VM4-08 |
| 65 | *geri ödeme kaydı* | park (K-894, K-940) | Karta para gönderen isteğin hâlleri | VM4-08 |
| 66 | *geçiş kaydı* | park (K-889) | Siparişin her durum geçişi: kim, ne zaman, hangi geçiş, sebep | VM4-06 |
| 67 | AuditLog | `02 §1.2`; Z-30 | İşlem izi | VM4-14 |
| 68 | *imha kaydı* | `02 §4.2` Z-44; park (K-875) | İmha çalışmalarının ve anında silmelerin tarihi, türü, sayısı | VM4-14 |
| 69 | *nabız ve kesinti kaydı* | park (K-895) | Zamanlayıcının nabzı, kesintiler, gecelik işin çalışma anları | VM4-14 |
| 70 | *yedeğin durum kaydı* | park (K-918) | Gecelik tam yedeğin sonucu — 1a–1c satırlarında kullanılmadı; 1d eşledi (K-918, K-937, K-963, `park A4-10`) | VM4-14 |
| 71 | *anahtar halkası* | `05 §7.6.3` | Çatının şifreleme anahtar halkası — 1a–1c satırlarında kullanılmadı; 1d'de yalnız K-975'in (4). kolunda; 2b'de K-879 ve `park A4-3` (K-1009) | VM4-14 |
| 72 | OrderAddressHistory | `02 §1.2`, §10.4.2 | Teslimat ve fatura adresi düzeltmelerinin eski ve yeni değeri, yönetici, tarih — 2b'de eklendi (GAP-13; K-1008) | VM4-13 |

Aday sayısı 72'dir: sözlükte kodu olan 54, italik 18. 1b, 1c ve 1d yeni aday eklemedi; 2b yedi italik adaya sözlüğün kod adını verdi — *görsel* → Image, *dosya kaydı* → DigitalFile, *fiyat geçmişi* → PriceHistory, *onay kaydı* → CheckboxAcceptance, *çıkarma kaydı* → ItemRemoval, *bekleyen e-posta değişikliği* → PendingEmailChange, *yasal metin sürümü* → LegalTextVersion (GAP-12; K-1006) — ve 72'yi ekledi (GAP-13; K-1008); 1a–2a'nın satırlarında eski adlar "Entity" sütununda yeni adlarla değiştirildi, satırların eşlemesi değişmedi. Sözlüğün `Notification` terimi ayrı aday değildir — kaydı gönderim kaydıdır (K-892); `Visitor`, `Buyer` ve `Consumer` aktör ya da sıfattır, kaydı yoktur. Kapalı listeler ve durum setleri aday değildir — §5'in enum'larıdır (VM5).

**"Veri doğurmaz" değer kümesi (K-999).** Veri doğurmayan bir kaynak satırı GAP değildir; "Entity" sütununa aşağıdaki sabit değerlerden biri adıyla yazılır. Bir satır hem bir adaya hem bir değere eşlenebilir (` · ` ile); en az bir aday taşıyan satırın durumu "eşlendi"dir.

| Değer | Ne zaman yazılır | Gerekçesi |
|---|---|---|
| veri doğurmaz — kod | Kural koddaki bir hesap, sınır, çit, sabit metin ya da ürün sabitidir | Ürün sabitleri ve kurallar koddadır, veritabanında değil (`05 §8.4`) |
| veri doğurmaz — türetilir | Değer başka alanlardan ya da bir koşuldan okunur, saklanmaz | Panel işaretleri ayrı alan olarak saklanmaz, saklanan kayıtların koşulundan okunur; bekleyen işler sayaçları ve satış özeti koşuldan türer (K-675, K-1015); satış kapısı ayarlardan (`02 §3.1.5`). Saklanan türetilmiş değerler — ayrılmış adet ve kupon hakları, ürün adının arama biçimi, kimlik karşılaştırma sütunu — bu değeri almaz, alanıyla eşlenir (K-1015) |
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
| KP-5 | Ürün sayfası: görsel, açıklama, KDV dahil fiyat, fiyat tarihi, üretim yeri, yerli üretim logosu, birim fiyat, referans fiyat ve indirim tarihleri | Product · Variant · Image · PriceHistory · Discount | üretim yeri, ölçü birimi + net miktar, fiyat, fiyatın başlangıcı, indirim tarihleri; birim fiyat ve logo türetilir | eşlendi |
| KP-6 | Üç ürün tipi aynı katalog ve sepette; hizmette ifa süresi | Product | `ProductType`, ifa süresi | eşlendi |
| KP-7 | Arşiv sayfası fiyatsız; taslak ve silinmiş ürün "sayfa bulunamadı"; silinenin adresi serbest; taslak varyant görünmez | Product · Variant | `PublishStatus`, sayfa adresi (silmeyle serbest kalır) | eşlendi |
| KP-8 | Hesap açmadan sipariş; misafirin e-postası özette, düzeltilebilir; sonradan firma düzeltir | Order · OrderEmailHistory | iletişim e-postası; düzeltme geçmişi | eşlendi |
| KP-9 | Sepete ekleme; canlı fiyat; stok ve ürün sınırı; dijitalde adet bir; kalem tavanı yok; daha önce alınmış dijital uyarısı | CartItem · Product | adet, ilk eklendiği fiyat; bir siparişte en fazla adet | eşlendi |
| KP-10 | Üyenin sepeti hesapta, misafirinki tarayıcıda; girişte birleşir; kendiliğinden boşalmaz | Cart · CartItem | sahibi (hesap ya da tarayıcı anahtarı), son erişim anı | eşlendi |
| KP-11 | Kupon kodu; yüzde ya da sabit; sıfıra indiremez | Coupon · Order · OrderItem | kod, tür, değer, asgari sepet tutarı; siparişin kupon bağı; kalem payı | eşlendi |
| KP-12 | Teslimat ve fatura adresi; adres defteri; yalnız dijital ve hizmette teslimat adresi yok; adresin alanları; posta kodu ve kimlik numarası yok | Order · AddressBook · *il listesi* | `ShippingAddress`, `BillingAddress`; defter kaydı | eşlendi |
| KP-13 | Sabit kargo ücreti, ücretsiz kargoya kalan tutar, asgari tutar, teslimat illeri | *firma ayarları* · Order | kargo ücreti, eşik, asgari tutar, teslimat illeri; siparişin kargo ücreti | eşlendi |
| KP-14 | Onay özeti, ÖBF, sabit metin, cayma özeti, onay kutuları, ödeme yükümlülüğü düğmesi; sürüm değişince yeniden onay | CheckboxAcceptance · LegalTextVersion · Order | kutuların kaydı; onaylanan sürümler | eşlendi |
| KP-15 | Kart (3D Secure) ya da havale; numara ve IBAN gösterilir; yarıda kalırsa yeni sipariş; süresinde ödenmeyen iptal; geç gelen kart ödemesi geri ödenir | Order · *ödeme kaydı* · *geri ödeme kaydı* · *gelen bildirim kaydı* | `PaymentMethod`, donmuş IBAN, ödeme süresinin bittiği an; ödeme ve otomatik geri ödemesi | eşlendi |
| KP-16 | Stok ödeme süresi boyunca ayrılır, onayda kesinleşir; kontenjan ve kupon hakkı aynı | StockReservation | ayrılmış adet ve hâli | eşlendi |
| KP-17 | Tahmin edilemez numara; tutar, kalem tanımı, adresler, iletişim e-postası — iki istisnası firmanın adres ve misafir e-postası düzeltmesi —, kargo sözü, firma kimliği, havale IBAN'ı, yasal metin sürümleri, onay kutuları donar | Order · OrderItem · LegalTextVersion · CheckboxAcceptance · OrderAddressHistory · OrderEmailHistory | `OrderNumber` ve donan değerler (`02 §3.23`); düzeltmelerin önceki değeri adres ve e-posta geçmişinde (GAP-13) | eşlendi |
| KP-18 | Sipariş sayfası: üç giriş yolu, erişim anahtarı; durumlar, geri ödemeler, takip, indirme, IBAN; kendiliğinden iptal edilen ödenmemiş sipariş müşteriye görünmez | Order · Refund · OrderItem | `OrderAccessKey`, `OrderStatus`, `PaymentStatus`; geri ödemelerin tarih, tutar ve yolu; iptalin türü | eşlendi |
| KP-19 | Kargoya verme süresi vitrinde; kargo şirketi ve takip numarası ya da "kendi aracımızla" | Product · Order | kargoya verme süresi; kargo şirketi, takip numarası, kendi aracıyla | eşlendi |
| KP-20 | Dijital ürünü indirme hakkı kadar indirme; güncel dosya | OrderItem · DigitalFile | indirme hakkı ve sayacı; varyantın o anki dosyası | eşlendi |
| KP-21 | Müşterinin kalem ve adet iptali; 14 günde geri ödeme; havalede IBAN; ödemeden önce bütünüyle; gecikme feshi; başka kanaldan feshi firma kaydeder | Cancellation · LateDeliveryTermination · Refund | iptal edilen adet, IBAN (havalede), fesih damgası ve kapsamı | eşlendi |
| KP-22 | Cayma — adetle, ardışık beyanlar; pencereler; beyan damgası; iade malı gönderme; geri ödeme süresi; IBAN; başka kanaldan bildirimin kaydı; istisnalar | Withdrawal · Return · RefundIbanRequest · Refund · OrderItem | beyan damgası, adet, iade adresi, IBAN; donmuş istisna işareti ve sebebi | eşlendi |
| KP-23 | Ayıp talebi — ayıplı adetle; kanalın açılışı; iki yıllık zamanaşımı; çözüldü ve yeniden açılma | DefectClaim | kalem, ayıplı adet, durum | eşlendi |
| KP-24 | Kalem ve adet düzeyinde iptal ve iade; ardışık işlemler; kupon payı adetle; kargo ücretinin iki hâlde geri ödenmesi | Cancellation · Withdrawal · Refund · OrderItem | işlem başına adet, ayrılan kupon payı ve KDV (K-788) | eşlendi |
| KP-25 | Ad, e-posta ve şifreyle hesap; kapı; doğrulanana kadar çalışmaz | Customer | ad, e-posta, şifre özeti, doğrulanmış mı | eşlendi |
| KP-26 | Google ile giriş; eşleşme; bekleyen kayıt silinir; doğrulanmamış e-postada hesap açılmaz | SocialLogin · Customer | Google bağı | eşlendi |
| KP-27 | Uzun oturum; şifre sıfırlama ve değiştirme; ad ve e-posta değişikliği; geri alma; yeniden doğrulama; oturumların ve işaretlerin düşmesi | Session · Customer · PendingEmailChange · *bağlantı kaydı* · RecognizedBrowserMark | oturum türü; bekleyen değişiklik; geri alma bağlantısı | eşlendi |
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
| KP-42 | Görsel yükleme, sıralama, ana görsel, alternatif metin; biçimli açıklama | Image · Product | sıra, ana görsel işareti, alternatif metin; açıklama | eşlendi |
| KP-43 | Tarihli yüzde indirim; referans fiyatı sistem hesaplar; kupon tanımı ve sınırları | Discount · PriceHistory · Coupon | indirim alanları; fiyat geçmişi; kupon alanları ve hakları | eşlendi |
| KP-44 | Dijital dosya yükleme ve güncelleme; indirme hakkının yenilenmesi | DigitalFile · OrderItem | dosya kaydı; indirme sayacı | eşlendi |
| KP-45 | Cayma istisnası işareti ve sebebi; mutlak ya da koşullu | Product · OrderItem | istisna işareti, `WithdrawalExemptionReason`; kaleme donar | eşlendi |
| KP-46 | Sipariş listesinde numara ya da e-postayla arama, durumla süzme; havale "ödendi"; kargoya verme; teslim; teslim edilemedi; yeniden gönderim; iptal; hizmet tamamlama | Order · OrderItem · *geçiş kaydı* | `OrderNumber`, iletişim e-postası; durumlar; kargo bilgisi; teslim işaretleri | eşlendi |
| KP-47 | Firma iptali sebepli; kart geri ödemesi; başarısızlık ve sonuç bekleniyor; havale geri ödemesi; iade teslim alma; iade reddi; kanuni faiz uyarısı; havale gerçekleşmedi; başka kanaldan caymanın kaydı ve teyidi | Cancellation · Refund · *geri ödeme kaydı* · Return · ReturnRejection · Withdrawal · RefundIbanRequest | sebep, adet; geri ödemenin hâli; ulaşma tarihi ve adet; ret; kaydeden ve bildirimin ulaştığı tarih | eşlendi |
| KP-48 | Müdahaleler: adres düzeltmesi — eski ve yeni adres adres geçmişinde —, adet azaltma ve çıkarma, takip bilgisi, teslim ve ulaşma tarihinin düzeltilmesi, geçiş düzeltmesi, iç not, "mal dönmedi", başka kanal kaydı, misafir e-postası düzeltmesi; elle sipariş ve fiyat değişikliği yok; tutar bazlı kısmi geri ödeme | Order · ItemRemoval · *geçiş kaydı* · NotReturnedClosure · OrderEmailHistory · OrderAddressHistory · Refund · AuditLog | düzeltilen alanlar; iç not (`InternalNote`); e-posta ve adres düzeltme geçmişi; tutar bazlı geri ödeme; iz satırları | eşlendi |
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
| KP-61 | Aydınlatma metni ve çerez politikası taslaktan düzenlenip yayına alınır; dört metin sürümlü | PrivacyNotice · CookiePolicy · LegalTextVersion | metin, sürüm | eşlendi |
| KP-62 | Yönetici daveti, kaldırma, geri çekme; ad; bildirim; aynı yetki; son yönetici kaldırılamaz; ayrı hesap türü; e-posta değişikliği | Admin · AdminInvitation · *bağlantı kaydı* · *gönderim kaydı* · AuditLog | ad, e-posta, şifre; davet ve geri çekilme; F-6 satırları | eşlendi |
| KP-63 | İşlem izi — kapsam alanları, eski ve yeni değerle; tarih ve yönetici süzgeci; on yıl değiştirilemez | AuditLog | an, yönetici (değer olarak), alan, kayıt kimliği, eski ve yeni değer | eşlendi |
| KP-64 | İşletme parametreleri — on firma ayarı, kurulumda dolu; ürün sabitleri değişmez | *firma ayarları* · veri doğurmaz — kod | firma ayarları (varsayılanları başlangıç verisi — VM8-01); ürün sabitleri kodda | eşlendi |
| KP-65 | Kurulum kontrol listesi; site boş kurulur | veri doğurmaz — türetilir | — (ayarlardan — `02 §10.8`) | veri doğurmaz |
| KP-66 | Müşteriye on altı olayın e-postası, düzeltmeler, IBAN isteği, e-posta düzeltme bildirimi, iade reddi | *gönderim kaydı* · veri doğurmaz — e-posta (`08`) | gönderim satırı; içerik `08`'in | eşlendi |
| KP-67 | Firmaya bildirimler; IBAN ve yönetici olaylarında bütün yöneticilere | *gönderim kaydı* | gönderim satırı | eşlendi |
| KP-68 | Yanıt firmanın iletişim e-postasına; üç yeniden deneme; "e-posta ulaşmadı" işareti; yeniden gönderim; firma bildirimi uyarısı | *gönderim kaydı* · *kanal hâli kaydı* · Company | deneme sayısı, sonuç; iletişim e-postası (yanıt adresi) | eşlendi |
| KP-69 | Tek adres, duyarlı site; panel telefondan | veri doğurmaz — arayüz (`04`) | — | veri doğurmaz |
| KP-70 | Tarayıcı tabanı | veri doğurmaz — doğrulama (`12`) | — | veri doğurmaz |
| KP-71 | Erişilebilirlik tabanı ve yedi kural | veri doğurmaz — doğrulama (`12`) · Image | alternatif metin | eşlendi |
| KP-72 | Deneme limitleri; açık ödenmemiş havale tavanı; tanınan tarayıcı; giriş kaydı altı ay | *deneme kaydı* · Order · RecognizedBrowserMark · *giriş kaydı* | deneme satırları; IP ve hesap bağı; giriş kaydının alanları | eşlendi |
| KP-73 | Yalnız zorunlu çerezler; üçüncü taraf ve ölçüm yok | veri doğurmaz — mimari (`05`) | — | veri doğurmaz |
| KP-74 | Aydınlatma bağlantısı; açık rıza yok; her gün imha; imha kaydı en az üç yıl | *imha kaydı* · veri doğurmaz — arayüz (`04`) | tarih, kayıt türü, sayı | eşlendi |
| KP-75 | Sipariş tarafında veri kaybı yok; disk kaybı istisnası; sepet ve oturum kaybı kabul | Order · *ödeme kaydı* · *geçiş kaydı* · *gönderim kaydı* · AuditLog · veri doğurmaz — mimari (`05`) | sipariş tarafı kayıtların sınıfı; yedek `05 §7.6` | eşlendi |
| KP-76 | Sessiz üzerine yazma yok; siparişe işlemler güncel hâle karşı | Product · Content · Coupon · *firma ayarları* · Order | sürüm değeri (K-873, K-874) | eşlendi |

#### 1.1.4 Ürün Gereksinimleri — `02 §0`–§2: sözlük, aktörler ve temel akış (163)

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
| `02 §1.1` m4 | Sözlük ürünün terimlerini taşır; teknik kavramların kod adını `06` `05`'teki adından verir; ürüne görünen kavram sözlüğe girer | veri doğurmaz — yazım konvansiyonu | — (teknik kavramlar VM3-03; ürüne görünen on üç kavram ve adres geçmişi sözlüğe girdi — GAP-12, GAP-13) | veri doğurmaz |
| `02 §1.1` m5 | Kapalı listelerin değer kod adları `06`'da verilir | veri doğurmaz — yazım konvansiyonu | — (değerler VM5-03) | veri doğurmaz |
| `02 §1.2` metin | Kod karşılıklarının kaynağı († işareti); kapsam notu — ürün türü denetimi ve mevzuat belgesi alanı yok; ölçek notu — ölçek sınırı uygulanmaz, iki seçenek boyutu sınırı geçerli | yoktur — kapsam dışı · Option | mevzuat belgesi ve yasaklı kategori alanı yok; varyant ve seçenek değeri sayısına tavan yok; ürün başına en çok iki seçenek | eşlendi |
| `02 §1.3` metin | Dört aktör; tek başına "müşteri" ve "üye"nin anlamı | veri doğurmaz — yazım konvansiyonu | — (VM0-07) | veri doğurmaz |
| `02 §1.3` m1 | Yönetim tek rol, çoklu kullanıcı; yetki matrisi yok; kimlik doğrulama müşteriyle aynı; yönetici hesabıyla alışveriş yok, iki hesap bağlanmaz | Admin | rol ve izin alanı yok; müşteri hesabına bağ yok | eşlendi |
| `02 §1.3` m2 | Ürün tüketiciye satış için; ticari alıcıya özel akış ve alan yok | yoktur — kapsam dışı | ticari alıcı alanı yok | veri doğurmaz |
| `02 §1.3` m3 | Üyelik zorunlu değil; numara + e-postayla takip; doğrulanmış e-postayla bağlanma; silinmiş hesabın siparişi düşmez | Order · Customer | hesap bağı isteğe bağlı; iletişim e-postası | eşlendi |
| `02 §1.3` m4 | Platform operatörü aktör değil; çok kiracılık yok | yoktur — kapsam dışı | kiracı ve operatör alanı yok (SK-1; K-10) | veri doğurmaz |

**`02 §1.2` — terim sözlüğü (119 — 1a 105; 2b on dört terim ekledi: GAP-12 ve GAP-13, `02` v0.77)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §1.2` `Product` | Ürün — vitrin birimi; ad, açıklama, kategori, tip, KDV, indirim, üretim yeri, ölçü birimi ve net miktar; görsel | Product | — (alanlar `02 §3`'ün kurallarında) | eşlendi |
| `02 §1.2` `Variant` | Varyant — satılan birim; stok ve fiyat | Variant | — | eşlendi |
| `02 §1.2` `Option` | Seçenek — boyutun adı; en fazla iki | Option | ad | eşlendi |
| `02 §1.2` `OptionValue` | Seçenek değeri | OptionValue | değer | eşlendi |
| `02 §1.2` `SKU` | Stok kodu — zorunlu, tekil | Variant | `SKU` | eşlendi |
| `02 §1.2` `Image` | Görsel — ürünün, varyantın ve içerik kaydının görseli; ana görsel işareti, sıra, alternatif metin (GAP-12) | Image | — (alanlar `02 §3.11`'in kurallarında) | eşlendi |
| `02 §1.2` `ProductType` | Ürün tipi — üç değerli kapalı alan, ürün düzleminde | Product | `ProductType` | eşlendi |
| `02 §1.2` `Physical` | Fiziksel ürün — stok zorunlu | Product | `ProductType` değeri | eşlendi |
| `02 §1.2` `Digital` | Dijital ürün — stok yok | Product | `ProductType` değeri | eşlendi |
| `02 §1.2` `Service` | Hizmet — isteğe bağlı kontenjan, zorunlu ifa süresi | Product · Variant | `ProductType` değeri; ifa süresi; kontenjan | eşlendi |
| `02 §1.2` `DigitalFile` | Dijital ürün dosyası — düzlem başına tek dosya; sipariş sayfası o anki dosyayı verir (GAP-12) | DigitalFile | — (alanlar `02 §3.12`'nin kurallarında) | eşlendi |
| `02 §1.2` `Category` | Kategori — en çok üç seviye | Category | — | eşlendi |
| `02 §1.2` `PrimaryCategory` | Ana kategori — kırıntı yolu; yayın için zorunlu | Product | `PrimaryCategory` | eşlendi |
| `02 §1.2` `PublishStatus` | Yayın durumu — ürün ve varyantta üç değer, içerikte iki | Product · Variant · Content | `PublishStatus` | eşlendi |
| `02 §1.2` `Draft` | Taslak | Product · Variant · Content | `PublishStatus` değeri | eşlendi |
| `02 §1.2` `Published` | Yayında — en az bir Yayında varyant; içerikte zorunlu alanlar | Product · Variant · Content | `PublishStatus` değeri | eşlendi |
| `02 §1.2` `Archived` | Arşiv — silme değil; içerikte yok | Product · Variant | `PublishStatus` değeri | eşlendi |
| `02 §1.2` `StockQuantity` | Stok — varyantın adedi; ürün düzleminde yok | Variant | `StockQuantity` | eşlendi |
| `02 §1.2` `StockReservation` | Stok ayırma — onayda ayrılır, ödemede düşer, dönebilir; kontenjan ve kupon hakkı aynı | StockReservation | — | eşlendi |
| `02 §1.2` `VatRate` | KDV oranı — ürün düzleminde; varsayılanı ayardan | Product · *firma ayarları* | `VatRate`; varsayılan oran | eşlendi |
| `02 §1.2` `MeasurementUnit` | Ölçü birimi — dokuz değerli kapalı liste; birim fiyat temel birim için (workshop 1) | Product | `MeasurementUnit` (değerler K-1018) | eşlendi |
| `02 §1.2` `NetQuantity` | Net miktar — ölçü birimiyle birlikte, sıfırdan büyük (workshop 1) | Product | `NetQuantity` — üç ondalık (K-1013) | eşlendi |
| `02 §1.2` `PriceHistory` | Fiyat geçmişi — uygulanmış fiyatlar ve başlangıç anları; referans fiyat ve fiyat tarihi ondan (GAP-12) | PriceHistory | — | eşlendi |
| `02 §1.2` `Discount` | İndirim — ürün düzleminde yüzdesel, tarihli; sıfıra indiremez | Discount | — | eşlendi |
| `02 §1.2` `ReferencePrice` | Referans fiyat — sistem hesaplar | PriceHistory · OrderItem | hesap fiyat geçmişinden; kaleme donan `ReferencePrice` | eşlendi |
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
| `02 §1.2` `ShippingAddress` | Teslimat adresi — ad, telefon, il, ilçe, açık adres; siparişe donar; firma Kargoya verildi'ye kadar düzeltir, önceki değer adres geçmişinde | Order · OrderAddressHistory | `ShippingAddress`; düzeltmenin önceki değeri (GAP-13) | eşlendi |
| `02 §1.2` `BillingAddress` | Fatura adresi — telefonsuz; siparişe donar; düzeltmesi teslimat adresininki gibi | Order · OrderAddressHistory | `BillingAddress`; düzeltmenin önceki değeri (GAP-13) | eşlendi |
| `02 §1.2` `AccountType` | Hesap türü — yönetici ve müşteri; ayrı kapılar | Customer · Admin · Session | `AccountType` (değerler VM5-03) — iki ayrı tablo (K-899) | eşlendi |
| `02 §1.2` `RecognizedBrowserMark` | Tanınan tarayıcı işareti — zorunlu çerez; ömrü P-48 | RecognizedBrowserMark | — | eşlendi |
| `02 §1.2` `PendingEmailChange` | Bekleyen e-posta değişikliği — doğrulanmayı bekleyen yeni adres; yenisi yerine geçer, ömrüyle düşer (GAP-12) | PendingEmailChange | — | eşlendi |
| `02 §1.2` `Cart` | Sepet — üyede hesapta, misafirde tarayıcıda; kendiliğinden boşalmaz | Cart | — | eşlendi |
| `02 §1.2` `CartItem` | Sepet kalemi — canlı; ilk eklendiği fiyatı hatırlar | CartItem | — | eşlendi |
| `02 §1.2` `Order` | Sipariş — onayda oluşur, numara, iki eksen; donan değerler | Order | — | eşlendi |
| `02 §1.2` `OrderItem` | Sipariş kalemi — donan değerler; adetle iptal ve iade; teslim işareti | OrderItem | — | eşlendi |
| `02 §1.2` `DeliveryDate` | Teslim tarihi — kalemin teslim işaretinin tarihi | OrderItem | `DeliveryDate` | eşlendi |
| `02 §1.2` `OrderNumber` | Sipariş numarası — tahmin edilemez, tekrar kullanılmaz | Order | `OrderNumber` | eşlendi |
| `02 §1.2` `OrderAccessKey` | Erişim anahtarı — siparişe özgü; e-posta düzeltmesinde yenilenir | Order | `OrderAccessKey` | eşlendi |
| `02 §1.2` `OrderEmailHistory` | E-posta geçmişi — eski ve yeni adres, yönetici, tarih; siparişle imha | OrderEmailHistory | eski adres, yeni adres, yönetici, tarih | eşlendi |
| `02 §1.2` `OrderAddressHistory` | Adres geçmişi — adres düzeltmesinin eski ve yeni değeri, yönetici, tarih; panelde, müşteriye görünmez; siparişle imha (GAP-13) | OrderAddressHistory | eski değer, yeni değer (teslimat telefonu dahil), yönetici, tarih | eşlendi |
| `02 §1.2` `InternalNote` | İç not — serbest metin; müşteriye görünmez; iz metinsiz; siparişle imha (GAP-12) | Order | iç not | eşlendi |
| `02 §1.2` `EmailCorrectedMark` | "E-posta düzeltildi" işareti — kalkmaz; e-posta geçmişiyle imha (GAP-12) | OrderEmailHistory | e-posta geçmişinin varlığı (işaret türer — K-1015) | eşlendi |
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
| `02 §1.2` `PaymentOutcomeUnknownMark` | "Ödeme sonucu alınamadı" işareti — son sorgu yanıtsızken; sonuç gelince kalkar (GAP-12) | Order · *ödeme kaydı* | ödeme Bekliyor ve ödeme süresinin bittiği an (işaret türer — K-1015) | eşlendi |
| `02 §1.2` `Cancellation` | İptal — sipariş, kalem ya da adet; firma sebep seçer | Cancellation | — | eşlendi |
| `02 §1.2` `CancellationReason` | İptal sebebi — kapalı liste; "diğer"de açıklama | Cancellation | `CancellationReason` (değerler VM5-03), açıklama | eşlendi |
| `02 §1.2` `ItemRemoval` | Kalem çıkarma — firmanın ödeme onayından sonra kalemi çıkarması ya da adedini azaltması; sebep listesi yok; iz ve B-11 (GAP-12) | ItemRemoval | — (alanlar `02 §10.4.3`'te) | eşlendi |
| `02 §1.2` `Withdrawal` | Cayma — adetle; pencereler; başka kanaldan firmanın kaydı; uzayan süre | Withdrawal | — | eşlendi |
| `02 §1.2` `WithdrawalExemptionReason` | Cayma istisnası sebebi — kapalı liste, ürün düzleminde | Product · OrderItem | `WithdrawalExemptionReason` (değerler VM5-03); kaleme donar | eşlendi |
| `02 §1.2` `LateDeliveryTermination` | Gecikme feshi — ayrı kayıt tipi; açık adetlerin tamamı | LateDeliveryTermination | — | eşlendi |
| `02 §1.2` `Return` | İade — malın dönüşü; ulaşma tarihi ve adet, parça parça | Return | — | eşlendi |
| `02 §1.2` `ReturnPendingMark` | "İade malı bekleniyor" işareti — teslimden sonraki caymada mal ulaşana kadar; teslim alma, ret ya da kapanışla kalkar (GAP-12) | Withdrawal · Return | beyanın adedi ile teslim alınan adet (işaret türer — K-1015) | eşlendi |
| `02 §1.2` `Refund` | Geri ödeme — paranın dönüşü, geldiği yoldan | Refund | — | eşlendi |
| `02 §1.2` `NotReturnedClosure` | "Mal dönmedi" kapanışı | NotReturnedClosure | — | eşlendi |
| `02 §1.2` `ReturnRejection` | İade reddi | ReturnRejection | — | eşlendi |
| `02 §1.2` `RefundIbanRequest` | IBAN isteği — kaleme düşen istek ve tarihi | RefundIbanRequest | — | eşlendi |
| `02 §1.2` `IbanPendingMark` | "IBAN bekleniyor" işareti — IBAN isteği açıkken; sayaca girmez (GAP-12) | RefundIbanRequest | isteğin hâli (işaret türer — K-1015) | eşlendi |
| `02 §1.2` `RefundFailedMark` | "Geri ödeme gerçekleşmedi" işareti — panel satırında | Order · *geri ödeme kaydı* | geri ödeme kaydının "gerçekleşmedi" hâli (işaret türer — K-940, K-1015) | eşlendi |
| `02 §1.2` `RefundOutcomePendingMark` | "Geri ödeme sonucu bekleniyor" işareti | Order · *geri ödeme kaydı* | geri ödeme kaydının "sonucu bekleniyor" hâli (işaret türer — K-1015) | eşlendi |
| `02 §1.2` `DefectClaim` | Ayıp talebi — kalem ve ayıplı adetle; iki durum | DefectClaim | — | eşlendi |
| `02 §1.2` `DefectClaimStatus` | Ayıp talebi durumu | DefectClaim | `DefectClaimStatus` | eşlendi |
| `02 §1.2` `Open` | Açık — iki durum alanında ortak değer | DefectClaim · ContactRequest | durum değeri | eşlendi |
| `02 §1.2` `Resolved` | Çözüldü | DefectClaim | `DefectClaimStatus` değeri | eşlendi |
| `02 §1.2` `PreInformationForm` | Ön Bilgilendirme Formu — ayarlardan üretilir; sürüm siparişe donar | PreInformationForm · LegalTextVersion · Order | sürüm; siparişe donan sürüm bağı | eşlendi |
| `02 §1.2` `DistanceSalesContract` | Mesafeli Satış Sözleşmesi — ayarlardan üretilir; sürüm ve kimlik siparişe donar | DistanceSalesContract · LegalTextVersion · Order | sürüm; siparişe donan sürüm bağı | eşlendi |
| `02 §1.2` `PrivacyNotice` | Aydınlatma metni — taslakla gelir, firma düzenler; sürümü siparişe donar | PrivacyNotice · LegalTextVersion · Order | metin, sürüm; siparişe donan sürüm bağı | eşlendi |
| `02 §1.2` `CookiePolicy` | Çerez politikası — taslakla gelir, firma düzenler | CookiePolicy · LegalTextVersion | metin, sürüm | eşlendi |
| `02 §1.2` `LegalTextVersion` | Yasal metin sürümü — dört metnin sürümleri; kendiliğinden artar; eski sürümler silinmez (GAP-12) | LegalTextVersion | — | eşlendi |
| `02 §1.2` `CheckboxAcceptance` | Onay kutusu kaydı — kutu, gösterilen metni, kalemler; siparişe donar; KVKK rızası değildir (GAP-12) | CheckboxAcceptance | — | eşlendi |
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
| `02 §1.2` `EmailUndeliveredMark` | "E-posta ulaşmadı" işareti — üç deneme başarısızsa sipariş, ayıp talebi ya da davet satırında; yeniden gönderimle kalkar (GAP-12) | *gönderim kaydı* · Order · DefectClaim · AdminInvitation | gönderimin sonucu (işaret türer — K-913, K-1015) | eşlendi |

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
| `02 §2.1` m8 | Akış 8 — mağaza ayarları: kimlik, ödeme yöntemleri, kargo, eşikler, iade adresi, yasal metinler, yöneticiler | Company · *firma ayarları* · LegalTextVersion · Admin | — | eşlendi |
| `02 §2.2` metin | Ana akış fiziksel ürünün yolculuğu; dijital ve hizmet teslimde ayrılır; hiçbir adım e-postaya bağlı değil | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §2.2` m1 | Ürünü bulur, varyant seçer | veri doğurmaz — arayüz (`04`) | — | veri doğurmaz |
| `02 §2.2` m2 | Varyantı adediyle sepete ekler | CartItem | varyant, adet | eşlendi |
| `02 §2.2` m3 | Ödeme adımı: adres defteri ya da yeni adres; misafir adres ve e-posta; kupon ve yöntem | Order · AddressBook | adresler, iletişim e-postası, kupon, `PaymentMethod` | eşlendi |
| `02 §2.2` m4 | Onay özeti, ÖBF, cayma özeti, iki kutu, ödeme yükümlülüğü | CheckboxAcceptance · veri doğurmaz — arayüz (`04`) | kutuların kaydı | eşlendi |
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
| `02 §3.3.4` | Her varyant zorunlu ve tekil stok kodu taşır; tekillik harf ayrımsız, kod yazıldığı gibi saklanır (workshop 1) | Variant | `SKU` — zorunlu, tekil (harf ayrımsız — normalleştirilmiş sütun, K-1016) | eşlendi |
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
| `02 §3.7.4` | Dijital ürünün yayındaki her varyantı bir dosyaya bağlı; hizmetin kapısı ifa süresini ister | Variant · Product · DigitalFile | varyantın dosyası (kendi ya da ürününki); ifa süresi | eşlendi |
| `02 §3.7.5` | Taslak ürün ziyaretçiye "sayfa bulunamadı"; yönetici "Taslak" bandıyla görür | veri doğurmaz — arayüz (`04`) | — (`PublishStatus`'tan) | veri doğurmaz |
| `02 §3.7.6` | Arşivlenmiş ürün sayfası çalışır, fiyatsız; listelerde görünmez | veri doğurmaz — arayüz (`04`) | — (`PublishStatus`'tan) | veri doğurmaz |
| `02 §3.7.7` | Kalıcı silme; geçmiş siparişler etkilenmez; açık siparişli ürün ya da varyant silinebilir; ayırma varyantla düşer | Product · Variant · OrderItem · StockReservation | silme fizikseldir; kalem donmuş bilgisini taşır, ürüne bağı silmeyle kalkar; silinen varyantın ayırması düşer (K-655) | eşlendi |
| `02 §3.7.8` | Yayındaki ürünün son Yayında varyantı arşivlenemez, taslağa çekilemez, silinemez | Product · Variant | yayın kapısı — kısıt | eşlendi |
| `02 §3.8.1` | Fiyat varyantta, KDV dahil girilir; matrah ve KDV geriye hesaplanır | Variant | fiyat (KDV dahil) | eşlendi |
| `02 §3.8.2` | KDV oranı ürün düzleminde, varsayılanı ayardan; matrah ve KDV sipariş kaydında ayrışır; kargonun oranı kalemleri izler | Product · *firma ayarları* · OrderItem · Order | `VatRate`; varsayılan KDV oranı; kalemin matrahı ve KDV tutarı; kargo ücretinin KDV'si (K-500) | eşlendi |
| `02 §3.8.3` | KDV kalem bazında, kuruşa yuvarlanır; hesap sırası sabit; kupon payı, KDV ve matrah kaleme donar; yuvarlamanın yönü | OrderItem | donmuş tutar, kupon payı, KDV, matrah; hesap ve yuvarlama kodda | eşlendi |
| `02 §3.8.4` | Fiyat hassasiyeti kuruş (iki ondalık); fiyat sıfırdan büyük | Variant | fiyat — ölçek iki ondalık, sıfırdan büyük (K-1013) | eşlendi |
| `02 §3.8.5` | Fiyat etiketi: üretim yeri, ölçü birimi (dokuz değerli kapalı liste) ve net miktar, birim fiyat temel birim için türetilir — indirimde indirimli fiyattan —, fiyatın başlangıç tarihi fiyat geçmişinden, yerli üretim logosu kendiliğinden | Product · PriceHistory · veri doğurmaz — türetilir | üretim yeri (ülke); `MeasurementUnit` (değerler K-1018) + `NetQuantity`; fiyatın başlangıç tarihi geçmişten; birim fiyat ve yerli üretim logosunun gösterimi türetilir — logo ürünle gelen varlık | eşlendi |
| `02 §3.9.1` | İndirim ürün düzleminde yüzde, bütün varyantlara iner; hiçbir varyantı sıfıra indiremez | Discount | yüzde (ölçek K-1013), ürüne bağ; sıfır fiyat engeli — kısıt | eşlendi |
| `02 §3.9.2` | İndirim tarihli, sistem başlatır ve bitirir; tarihler gösterilir | Discount | başlangıç anı, bitiş anı | eşlendi |
| `02 §3.9.3` | Referans fiyatı sistem hesaplar; her varyantın fiyat geçmişi tutulur; indirimli fiyat da geçmişe yazılır | PriceHistory | varyant, fiyat, uygulanmaya başladığı an (indirimli fiyat dahil) | eşlendi |
| `02 §3.9.4` | 10 günlük geçmişi olmayan malda referans satışa sunuluştan beri en düşük fiyat | PriceHistory · Variant | varyantın satışa sunulduğu ilk an (kendisinin ve ürününün birlikte Yayında olduğu an) | eşlendi |
| `02 §3.9.5` | İndirimli fiyat referansın altına inmiyorsa indirim kaydedilmez | veri doğurmaz — türetilir | — (doğrulama kuralı, kodda) | veri doğurmaz |
| `02 §3.9.6` | Beyan edilen referans fiyat kaleme donar | OrderItem | donmuş `ReferencePrice` | eşlendi |
| `02 §3.10.1` | Kupon sepet toplamına iner; siparişe en fazla bir kupon kodu | Coupon · Order | Order: kupon bağı (en çok bir) | eşlendi |
| `02 §3.10.2` | Kupon yüzde ya da sabit tutar | Coupon | tür (yüzde ya da sabit tutar), değer | eşlendi |
| `02 §3.10.3` | Kupon payı kalem kalem; sabit tutar orantılı dağıtılır; pay kaleme donar | OrderItem | donmuş kupon payı | eşlendi |
| `02 §3.10.4` | Kupon siparişi sıfıra indiremez; sabit tutarlı kuponun zorunlu asgari sepet tutarı | Coupon | asgari sepet tutarı (sabit tutarlıda zorunlu, kupon tutarından büyük) | eşlendi |
| `02 §3.10.5` | Sınır tarih aralığı ve toplam kullanım adedi; kod tekil, harf duyarsız; kupon silinmez | Coupon | kod (tekil, harf duyarsız — K-1016), başlangıç ve bitiş, toplam kullanım adedi; silinmez | eşlendi |
| `02 §3.10.6` | Kullanım hakkı stok gibi ayrılır; toplam adet kullanılmış + ayrılmış hakların altına inemez | Coupon · StockReservation | kullanılmış ve ayrılmış haklar (K-654) | eşlendi |
| `02 §3.10.7` | Kupon kargo eşiğinin ve asgari tutarın tabanını düşürmez | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §3.11.1` | Görsel ürün düzleminde; varyant kendi görselini taşıyabilir | Image | ürüne bağ, varyanta bağ | eşlendi |
| `02 §3.11.2` | Ana görsel işareti galeri sırasından bağımsız; galeri sırası elle | Image | ana görsel işareti, galeri sırası | eşlendi |
| `02 §3.11.3` | Görselin format, boyut ve adet tavanı | veri doğurmaz — kod | — (değerler `02 §11`'de) | veri doğurmaz |
| `02 §3.11.4` | Alternatif metin isteğe bağlı, boşsa sistem üretir | Image | alternatif metin (isteğe bağlı); üretilen metin türetilir | eşlendi |
| `02 §3.11.5` | Ürün adına karakter tavanı; açıklama serbest | Product | ad (uzunluk tavanı), açıklama | eşlendi |
| `02 §3.11.6` | Uzun metinler kapalı biçim setini kullanır; ham HTML yok | Product · AboutPage · Offering · PortfolioItem · FaqItem · CustomPage · PrivacyNotice · CookiePolicy | biçimli metin alanları (set kodda) | eşlendi |
| `02 §3.11.7` | Garanti bilgisinin ayrı alanı yok; garanti belgesi saklanmaz | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §3.12.1` | Dijital ürünü sistem teslim eder; ödemede indirme açılır | OrderItem | teslim işareti (dijitalde ödeme onayı) | eşlendi |
| `02 §3.12.2` | Dosya ürün düzleminde, varyant kendi dosyasını taşıyabilir; her düzlemde tek dosya | DigitalFile · Product · Variant | ürünün ve varyantın o anki dosyası (K-883) | eşlendi |
| `02 §3.12.3` | Tek dosyanın boyut tavanı; tür ve güvenlik `05`'in işi | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §3.12.4` | Dijital üründe adet her zaman 1 | CartItem · OrderItem | adet = 1 — kısıt | eşlendi |
| `02 §3.12.5` | İndirme hakkı adetle sınırlı, kalem başına; kaleme donar | OrderItem | indirme hakkı (donmuş), indirme sayacı | eşlendi |
| `02 §3.12.6` | Hakkı dolana firma kalemin hakkını yeniler, sayaç sıfırlanır | OrderItem | indirme sayacı (sıfırlanır) | eşlendi |
| `02 §3.12.7` | Sipariş sayfası varyantın o anki dosyasını verir; silinse de son hâl indirilir | OrderItem · DigitalFile | kalemin varyant bağı ya da son dosya bağı (K-883, K-948) | eşlendi |
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
| `02 §3.13.14` | E-posta yeni adres doğrulanmadan değişmez; eski adrese bildirim ve geri alma bağlantısı; geri alma Google bağını kaldırır, şifreyi geçersizleştirir; bekleyen değişiklik görünür, yenisi yerine geçer | Customer · PendingEmailChange · *bağlantı kaydı* · SocialLogin · *gönderim kaydı* | bekleyen yeni adres ve bağlantısı; geri alma bağlantısı (ömür Z-45); Google bağının değişiklik süresince kurulduğu an; eski adrese giden bildirimin satırı | eşlendi |
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
| `02 §3.15.5` | Uygulama içi veri indirme yok; üye kaydı görünümü e-postayla aranır, salt okunur; firma talep üzerine siler, işlem izine | Customer · AuditLog | e-postayla arama (harf duyarsız — K-1016); silmenin iz satırı | eşlendi |
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
| `02 §3.17.2` | Onaylanan özet ile sipariş birebir aynı; fark durdurur; ÖBF ya da MSS sürümünü artıran fark da durdurur | Order · LegalTextVersion | onaylanan sürümler (karşılaştırma onay anında — K-591) | eşlendi |
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
| `02 §3.23.1` | Kalem ödenen tutarı üreten her değeri ve ürünün o anki tanımını dondurur: fiyat, KDV oranı ve tutarı, indirim oranı, referans fiyat, kupon payı, ad, seçenek değerleri, görsel; tip, cayma istisnası işareti, sebebi ve koşulu, ifa süresi, indirme hakkı | OrderItem · Image | donmuş birim fiyat, `VatRate`, KDV tutarı, indirim oranı, `ReferencePrice`, kupon payı, ürün adı, seçenek değerleri, görsel bağı (K-883), `ProductType`, cayma istisnası işareti, `WithdrawalExemptionReason` ve koşulu, ifa süresi, indirme hakkı | eşlendi |
| `02 §3.23.2` | Donan değer kalemde düz yaşar; sipariş düzleminde kupon kodu, kargo ücreti, toplamlar; ara gruplama yok | OrderItem · Order | Order: kupon kodu, kargo ücreti, toplamlar | eşlendi |
| `02 §3.23.3` | Sipariş adresleri, iletişim e-postası, firmanın yasal kimliği, ÖBF ve MSS sürümleri, onay kutularının kaydı, havale IBAN'ı ve aydınlatma metni sürümünü dondurur; iki istisna — firmanın adres düzeltmesi ve misafirin e-postası, önceki değer adres ya da e-posta geçmişinde; donmuş sürümler silinmez | Order · LegalTextVersion · CheckboxAcceptance · OrderAddressHistory · OrderEmailHistory | `ShippingAddress`, `BillingAddress`, iletişim e-postası, firmanın donmuş kimliği (tipe göre kimlik alanları, adres, iletişim, `LegalName`), ÖBF ve MSS ile aydınlatma metninin sürüm bağı, havale IBAN'ı; düzeltilen adresin ve e-postanın önceki değeri (GAP-13) | eşlendi |
| `02 §3.23.4` | Donma anı siparişin oluştuğu an; onaylanan tutar bağlayıcı | Order | oluşma anı | eşlendi |
| `02 §3.23.5` | Marka ayarları siparişe donmaz | Branding | — (siparişe kopyalanmaz) | eşlendi |
| `02 §3.24.1` | İki onay kutusu (ÖBF teyidi, MSS onayı); KVKK rızası kutusu yok; ödeme yükümlülüğü yazısı | CheckboxAcceptance · veri doğurmaz — arayüz (`04`) | iki kutunun işaretlenmiş olması — sipariş ancak işaretlenince oluşur | eşlendi |
| `02 §3.24.2` | Dijital kalemde ve hizmet kaleminde ek zorunlu kutu — cayma istisnasının kurucu koşulu | CheckboxAcceptance | dijital ve hizmet kutusu | eşlendi |
| `02 §3.24.3` | ÖBF onay özetiyle aynı, dört bilgi ekler; sabit metinler; "sipariş nasıl kurulur"; kısa cayma özeti; on iki punto | PreInformationForm · veri doğurmaz — arayüz (`04`) | form ayarlardan ve siparişten üretilir — yeni alan doğurmaz (sabit metinler kodda); siparişe kopya mı, sürümden mi — VM4-11 | eşlendi |
| `02 §3.24.4` | Yaş için ayrı beyan yok; üyelik sözleşmesi yok | yoktur — kapsam dışı | doğum tarihi ve yaş beyanı alanı yok | veri doğurmaz |
| `02 §3.24.5` | Misafirin e-postası kodla sınanmaz, özette gösterilir | Order | iletişim e-postası | eşlendi |
| `02 §3.24.6` | Onaylanan sürümler ve kimlik donar; dijital ve hizmet kutularının kaydı: hangi kutu, gösterilen metni, kapsadığı kalemler; ayrı zaman damgası yok | CheckboxAcceptance · Order | kutu, kutunun gösterilen metni, kapsadığı kalemler (K-595); an siparişin oluşma anı | eşlendi |
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
| `02 §3.27.3` | Hakkımızda tek kayıt: kısa tanıtım, uzun metin, görseller; silinmez | AboutPage · Image | kısa tanıtım (uzunluk tavanı), uzun metin; tekil; silinmez | eşlendi |
| `02 §3.27.4` | Hizmet tanıtımı fiyatsız: ad, kısa açıklama, metin, görseller, sayfa | Offering · Image | ad, kısa açıklama, metin, sayfa adresi | eşlendi |
| `02 §3.27.5` | Referans iş: başlık, kısa açıklama, metin, görseller, sayfa; müşteri adı, yıl, kategori alanı yok | PortfolioItem · Image | başlık, kısa açıklama, metin, sayfa adresi | eşlendi |
| `02 §3.27.6` | SSS soru ve cevap, tek sayfada; soru tavanlı | FaqItem | soru (uzunluk tavanı), cevap | eşlendi |
| `02 §3.27.7` | Şube: ad ve adres zorunlu (il listeden); telefon, çalışma saatleri, görsel, harita bağlantısı isteğe bağlı | Branch · *il listesi* · Image | ad, il, ilçe ve açık adres, telefon, çalışma saatleri (tavanlı), harita bağlantısı | eşlendi |
| `02 §3.27.8` | İletişim sayfası ayrı tip değil | veri doğurmaz — türetilir | — (Company, Branch ve form) | veri doğurmaz |
| `02 §3.27.9` | Genel sayfa başlık, metin, görseller; adet sınırı yok; yasal metin genel sayfa değil | CustomPage · Image | başlık, metin, sayfa adresi | eşlendi |
| `02 §3.27.10` | Hizmet tanıtımı ve referans iş ürün bağlayabilir; yalnız yayındaki ürün görünür; silinen ürün bağdan kalkar | *içerik–ürün bağı* | içerik kaydı, ürün (ürün düzlemi; silmeyle kalkar) | eşlendi |
| `02 §3.27.11` | Listeleri firma elle sıralar | Offering · PortfolioItem · FaqItem · Branch · CustomPage | sıra | eşlendi |
| `02 §3.27.12` | Sosyal medya bağlantıları (kapalı platform listesi) ve WhatsApp numarası; siparişe donmaz | SocialLink | platform (kapalı liste), bağlantı; WhatsApp numarası | eşlendi |
| `02 §3.27.13` | Hiçbir tip zorunlu değil; kayıt düzeyinde yayın kapısı tipe göre | Content | tipe göre zorunlu alanlar — kısıt (yayında) | eşlendi |
| `02 §3.27.14` | Hakkımızda boşken kurumsal blok marka adı ve logoyu gösterir | veri doğurmaz — arayüz (`04`) | — | veri doğurmaz |
| `02 §3.27.15` | İçerik görselleri ürünün kurallarına tabi; kısa alanlara karakter tavanı | Image · Content | ana görsel işareti, galeri sırası, alternatif metin; kısa alanların uzunluk tavanı | eşlendi |
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
| `02 §3.29.2` | Logo zorunlu değil; tek logo; görsel kuralları | Branding · Image | `Logo` (isteğe bağlı) | eşlendi |
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
| `02 §3.31.1` | Sessiz üzerine yazma yok; içerik, ürün ve ayarlarda aynı; mekanizma `05`/`06` | Product · Variant · Content · Coupon · *firma ayarları* | sürüm değeri (bölüm başına sürüm — K-873, K-1014) | eşlendi |
| `02 §3.31.2` | Siparişe yönelik her işlem güncel hâle karşı; ikinci işlem reddedilir; sipariş kilitlenmez | Order | siparişin sürüm değeri (K-874; artıran yazımlar K-1014) | eşlendi |
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
| `02 §3.33.1` | Dört yasal metin, sürümlü; ilk ikisi üretilir, son ikisi taslakla gelir ve firma düzenler | LegalTextVersion · PreInformationForm · DistanceSalesContract · PrivacyNotice · CookiePolicy | metin türü (dört değer), sürüm numarası; düzenlenen iki metnin içeriği | eşlendi |
| `02 §3.33.2` | Firmanın düzenlediği iki metin zorunlu, boşaltılamaz; aydınlatma taslağı yer tutucular, barındırma konumu bölümü, alıcı grupları bölümü | PrivacyNotice · CookiePolicy | metin (zorunlu); barındırma konumu bölümü (doluysa metin tamamlanmış); taslak kodda gelir — başlangıç verisi (VM8-01) | eşlendi |
| `02 §3.33.3` | Sürüm kendiliğinden artar; eski sürümler silinmez, siparişin süresini izler; sürüm değişikliği ize yazılır | LegalTextVersion · AuditLog | sürüm numarası, yürürlüğe giriş anı, içerik ya da üretim girdisi (VM4-11); silinmez — siparişin süresini izler; iz satırı | eşlendi |
| `02 §3.33.4` | Metin değişince yeniden onay alınmaz; onay adımı açıkken sürüm artarsa sipariş oluşmaz | Order · LegalTextVersion | onaylanan sürüm bağı; onay anında karşılaştırma (K-591) | eşlendi |
| `02 §3.33.5` | Aydınlatma bağlantısı veri açan her ekranda; çerez bandı yok; "gördü" kaydı tutulmaz | Order · veri doğurmaz — arayüz (`04`) | aydınlatma metninin donmuş sürüm bağı; kullanıcı başına gösterim kaydı yok | eşlendi |
| `02 §3.33.6` | ÖBF ve MSS onay e-postasının gövdesinde tam metin; ek yok; donmuş sürümle yeniden gönderilebilir | *gönderim kaydı* · LegalTextVersion | gönderim satırı (gövde saklanmaz — Z-34); yeniden gönderim donmuş sürümden | eşlendi |
| `02 §3.33.7` | Uyuşmazlık bilgisi iki metnin zorunlu parçası; parasal sınır rakamla yazılmaz | veri doğurmaz — kod | — (metnin sabit parçası) | veri doğurmaz |
| `02 §3.33.8` | Ayarlarla çelişen serbest metin firmanın sorumluluğunda; ürün tespit etmez | veri doğurmaz — arayüz (`04`) | — | veri doğurmaz |
| `02 §3.33.9` | Dört metin tamamlanmadan satış açılmaz | veri doğurmaz — türetilir | — (`02 §3.1.5`) | veri doğurmaz |
| `02 §3.34.1` | Tek site, duyarlı tasarım | veri doğurmaz — arayüz (`04`) | — | veri doğurmaz |
| `02 §3.34.2` | Vitrin mobil, panel masaüstü öncelikli; panelde işlev kapatılmaz | veri doğurmaz — arayüz (`04`) | — | veri doğurmaz |
| `02 §3.34.3` | Tarayıcı tabanı son iki büyük sürüm | veri doğurmaz — doğrulama (`12`) | — | veri doğurmaz |
| `02 §3.34.4` | En dar desteklenen genişlik bir sayıdır | veri doğurmaz — arayüz (`04`) | — | veri doğurmaz |
| `02 §3.34.5` | Erişilebilirlik tabanı ve yedi test edilen kural | veri doğurmaz — doğrulama (`12`) · Image | alternatif metin (`02 §3.11.4`'ün alanı) | eşlendi |
| `02 §3.34.6` | Ziyaretçi ölçümü yok; satış özeti yalnız sipariş ve iletişim talebinden | yoktur — kapsam dışı | ölçüm tablosu yok | veri doğurmaz |
| `02 §3.34.7` | Çalışma süresi taahhüdü yok; kesintide süreler dondurulmaz; havale iptali ertelenir | Order · *nabız ve kesinti kaydı* | ertelenen son an (K-898); kesinti kaydı (K-895) | eşlendi |
| `02 §3.34.8` | Sipariş tarafında veri kaybı toleransı sıfır: sipariş, ödeme, geçişler, kalem kayıtları, iletişim talepleri, donmuş sürümler, onay kaydı, gönderim kaydı, işlem izi; sepet ve oturum kaybı kabul | Order · *ödeme kaydı* · *geçiş kaydı* · ContactRequest · LegalTextVersion · CheckboxAcceptance · *gönderim kaydı* · AuditLog · veri doğurmaz — mimari (`05`) | sipariş tarafı kayıtların sınıfı (silme kategorisi K-1010); yedek ve aktarım `05 §7.6` | eşlendi |

#### 1.1.6 Ürün Gereksinimleri — `02 §4` zaman kuralları ve §5 durum tanımları (170)

Sayım kuralları kural kimliğiyle, süreler Z kimliğiyle, durumlar kodlarıyla, geçişler S ve Ö kimlikleriyle, kendiliğinden işleyen anlar ve kalem kayıtları madde sırasıyla girer (K-997, K-1000). Bir sürenin satırı başlangıç değerinin alanına eşlenir — imha işi süreyi o alandan hesaplar (K-875; VM9-01).

**`02 §4` — zaman kuralları (61)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §4` metin | Sürelerin evi; değer §11'de; firmanın resmî kurumlara karşı süreleri §12.5'te; sayaçsız yasal yükümlülükler satır olarak | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §4.1.1` | Tek saat dilimi Türkiye saati; depolama biçimi `05` ve `06`'nın | veri doğurmaz — mimari (`05`) | anların türü — saat dilimli damga, UTC (K-870, K-1013) | veri doğurmaz |
| `02 §4.1.2` | İş günü pazartesi–cuma, resmî tatiller hariç; tatil listesi ürünle gelir, firma düzenlemez | veri doğurmaz — kod | — (iş günü takvimi veritabanında değil — `05 §7.2.3`) | veri doğurmaz |
| `02 §4.1.3` | İki süre iş günüyle, gerisi takvim günüyle | veri doğurmaz — kod | — | veri doğurmaz |
| `02 §4.1.4` | Gün olarak süre ertesi gün başlar, son günün 23:59'unda dolar; dakika ve saat süreleri anından işler | veri doğurmaz — kod | — (tarih olarak anlam taşıyan değer tarih türünde — K-870) | veri doğurmaz |
| `02 §4.1.5` | Üç süre çitli: kargoya verme süresi üst, havale süresi alt ve üst, ifa süresi alt | veri doğurmaz — kod | — (çitler kodda; değerler `02 §11`) | veri doğurmaz |
| `02 §4.2` Z-1 | E-posta doğrulama bağlantısının ömrü; dolunca doğrulanmamış kayıt silinir, bekleyen değişiklik düşer | *bağlantı kaydı* · Customer · PendingEmailChange | ilk bağlantının gönderildiği an, ömrün sonu; bekleyen kaydın ve değişikliğin silinmesi | eşlendi |
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
| `02 §4.2` Z-15 | Dijital kalemde cayma hakkı ödemede, üçüncü kutuyla düşer | CheckboxAcceptance · OrderItem | dijital kutusunun kaydı; teslim işareti | eşlendi |
| `02 §4.2` Z-16 | Cayma geri ödemesi 14 gün — beyandan ya da iade malının ulaştığı tarihten; panel kalan süreyi gösterir | Withdrawal · Return · Refund | beyanın tarih damgası; ulaşma tarihi (geçmişe dönük olabilir, teslim tarihinden önce olamaz) | eşlendi |
| `02 §4.2` Z-42 | Müşterinin iade malını gönderme süresi — beyandan 14 gün; sistem denetlemez | Withdrawal | beyanın tarih damgası (süre türetilir) | eşlendi |
| `02 §4.2` Z-17 | İptalde geri ödeme 14 gün — iptal anından; kartta sistem, havalede firma | Cancellation · Refund | iptal anı | eşlendi |
| `02 §4.2` Z-47 | İfanın imkânsızlaştığının bildirimi 3 gün — firmanın yükümlülüğü, sayaç yok | veri doğurmaz — ürünün dışında | — | veri doğurmaz |
| `02 §4.2` Z-18 | Ayıp talebinin iki yıllık zamanaşımı — kalemin teslim işaretinden; dolunca kanal kapanır | OrderItem · DefectClaim | teslim işaretinin tarihi (süre türetilir) | eşlendi |
| `02 §4.2` Z-19 | İndirme hakkı — süre yok, adet var | OrderItem | indirme hakkı ve sayacı | eşlendi |
| `02 §4.2` Z-20 | İndirimin tarih aralığı | Discount | başlangıç ve bitiş anı | eşlendi |
| `02 §4.2` Z-21 | Referans fiyatın geriye bakış penceresi — 10 gün ya da satışa sunuluştan beri | PriceHistory · Variant | fiyat geçmişi; satışa sunulduğu ilk an | eşlendi |
| `02 §4.2` Z-22 | Kuponun tarih aralığı | Coupon | başlangıç ve bitiş | eşlendi |
| `02 §4.2` Z-23 | Duyurunun tarih aralığı — isteğe bağlı | Announcement | başlangıç ve bitiş (isteğe bağlı) | eşlendi |
| `02 §4.2` Z-24 | İleri tarihli yayın yok | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §4.2` Z-25 | KVKK başvurusuna 30 günde cevap — sayaç yok | veri doğurmaz — ürünün dışında | — | veri doğurmaz |
| `02 §4.2` Z-26 | Gönderilemeyen e-postanın üç yeniden denemesi; "e-posta ulaşmadı" işareti ve firma uyarısı | *gönderim kaydı* · *kanal hâli kaydı* | deneme sayısı, sonraki deneme anı, sonuç; işaret gönderim kaydından türer (K-913) | eşlendi |
| `02 §4.2` Z-27 | Deneme limitlerinin kayan penceresi | *deneme kaydı* | an (pencere geçince silinir) | eşlendi |
| `02 §4.2` Z-28 | Sipariş, ödeme, fatura verisi, sözleşme kayıtları, donmuş sürümler, onay kaydı — yılın sonundan 10 yıl; kişisel veri imha, hesap bağı kesilir; yalnız ödemesi alınmış sipariş; geç gelip geri ödenen kart ödemesi de | Order · OrderItem · *ödeme kaydı* · LegalTextVersion · CheckboxAcceptance · *imha kaydı* | oluşma yılı (başlangıç değeri — K-875); ödemesi alınmış sayılma (K-951); boşaltılan kişisel alanlar (VM9-02) | eşlendi |
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
| `02 §5.1` m1 | Yayın kapısı: ad, ana kategori, tip, en az bir Yayında varyant; dijitalde dosya, hizmette ifa süresi | Product · Variant · DigitalFile | kapının okuduğu alanlar — kısıt | eşlendi |
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
| `02 §5.4` m3 | Yönetici müdahaleleri geçiş değil; adres düzeltmesi ve çıkarmanın sınırı | ItemRemoval · AuditLog | çıkarma kaydı; müdahalelerin iz satırı (`02 §10.4`) | eşlendi |
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
| `02 §5.5` m3 | Kart iadesi başarısızsa eksen değişmez; "gerçekleşmedi" ve "sonuç bekleniyor" işaretleri | *geri ödeme kaydı* · Order | geri ödeme kaydının hâli (K-940); iki işaret ondan türer (K-1015) | eşlendi |
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
| `02 §5.8` metin | Kalem durum makinesi taşımaz; kayıtlar adedi taşır, aynı tür birden çok kez doğar; açık adet kayıtlardan okunur | OrderItem · Cancellation · ItemRemoval · LateDeliveryTermination · Withdrawal · Return · ReturnRejection · NotReturnedClosure · DefectClaim | her kaydın adedi; açık adet saklanmaz, kayıtlardan türer (K-787, K-1015) | eşlendi |
| `02 §5.8` m1 | Teslim işareti ve teslim tarihi — fizikselde sipariş düzeyinde bir kez | OrderItem | `DeliveryDate` | eşlendi |
| `02 §5.8` m2 | İptal kaydı — adet ve firma iptalinde sebep | Cancellation | kalem, adet, `CancellationReason` (firma iptalinde), açıklama ("diğer") | eşlendi |
| `02 §5.8` m3 | Çıkarma kaydı — yöneticinin kalemi çıkarması ya da adedini azaltması; işlem izine | ItemRemoval · AuditLog | kalem, adet, yönetici; iz satırı — `ItemRemoval` (GAP-12) | eşlendi |
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
| `02 §5.11.1` | İndirim tarih aralığında kendiliğinden işler; indirimli fiyat fiyat geçmişine yazılır | Discount · PriceHistory | başlangıç ve bitiş; indirimli fiyatın geçmiş satırı | eşlendi |
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
| `02 §6.1.1` | Akış e-postaya bağlı değildir; üç yeniden deneme, "e-posta ulaşmadı" işareti (işlem izine yazılmaz), yeniden gönderim (B-1…B-16; B-1 donmuş sürümle), kanal uyarısı; misafir e-postası düzeltmesi; hesap bağlantıları kuralın dışında | *gönderim kaydı* · *kanal hâli kaydı* · OrderEmailHistory · LegalTextVersion · *bağlantı kaydı* | gönderimin deneme sayısı ve sonucu; işaret gönderim kaydından türer (K-913), yeniden gönderim ayrı satır (Z-43); kanal hâli; e-posta düzeltmesinin eski ve yeni değeri; B-1'in metni siparişe donan sürümden | eşlendi |
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
| `02 §6.2.3` | Onay anında fiyat, indirim, kupon, kargo, eşik, teslimat ili, ayırma ya da yasal metin sürümü değişmişse sipariş oluşmaz; metin değiştiyse kutular yeniden işaretlenir | LegalTextVersion · CheckboxAcceptance · Order | özetin gösterildiği andaki sürüm — onay anında karşılaştırılır; siparişe donan sürüm ve kutuların kaydı | eşlendi |
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
| `02 §6.3.6` | Teslimat adresi istemeden düzeltilir: firma kendi kanallarına dönerek teyit eder; B-10; ilk adres adres geçmişinden okunup geri düzeltilir; "Kargoya verildi"den sonra düzeltme yok | Order · OrderAddressHistory · AuditLog · *gönderim kaydı* | `ShippingAddress` düzeltmesi; iz satırı sipariş, alan ve işlemi taşır, adres değerini taşımaz (`02 §10.3.1`) — eski ve yeni değer `OrderAddressHistory`'de, geri düzeltmenin dayanağı (GAP-13); teyit kaydedilmez | eşlendi |

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
| `02 §6.4.24` | Kart iadesi reddedilir: "geri ödeme gerçekleşmedi" işareti, sayaç, uyarı; yeniden deneme ya da havale yolu; zaman aşımında "geri ödeme sonucu bekleniyor", sorgu, firmanın "gerçekleşmedi" işareti | *geri ödeme kaydı* · Order · RefundIbanRequest · Refund | geri ödeme kaydının hâli (K-940); iki işaret ondan türer (K-1015); IBAN girildi mi — girildikten sonra kart denenmez | eşlendi |
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
| `02 §6.5` | 17 satır: doğrulama bağlantısı, doğrulanmamış giriş, kayıtlı e-posta, sıfırlama, zayıf şifre, Google'ın doğrulanmamış e-postası, şifresiz hesap, yeni e-posta, hesap silme, deneme limitleri, veri toplayan girişlerin kapısı, istenmeyen e-posta değişikliği, yeniden doğrulama | Customer · *bağlantı kaydı* · PendingEmailChange · SocialLogin · Session · *deneme kaydı* · RecognizedBrowserMark · veri doğurmaz — kod | doğrulanmış mı; bağlantıların ömrü; şifre özeti (boş olabilir); deneme satırları; şifre kuralı kodda | eşlendi |
| `02 §6.5.1` | Doğrulama bağlantısının süresi dolunca kayıt silinir; e-posta yeniden kayda açık | Customer · *bağlantı kaydı* | doğrulanmamış kaydın silinmesi (Z-1) | eşlendi |
| `02 §6.5.9` | Yeni e-posta doğrulanmazsa değişiklik geçerli olmaz | PendingEmailChange | bekleyen adres, ömrü | eşlendi |
| `02 §6.5.11` | Giriş, sıfırlama ve kayıt denemeleri IP ve e-posta ekseninde limitli; hesap kilitlenmez; e-posta eksenindeki engel tanınan tarayıcıyı kapatmaz | *deneme kaydı* · RecognizedBrowserMark | deneme satırı (L-1…L-3); tanınan tarayıcı kaydı | eşlendi |
| `02 §6.5.13` | E-posta istenmeden değiştirilir: eski adrese bildirim, "bu değişikliği ben yapmadım" bağlantısıyla geri alma, bütün oturumlar kapanır, şifre yeniden belirlenir; bağlantı açıkken yeniden değiştirilemez | *bağlantı kaydı* · Session · Customer | geri alma bağlantısının ömrü ve kullanılması; eski adres; oturumların silinmesi | eşlendi |
| `02 §6.5.14` | Doğrulama bağlantısı yeniden isteği L-9 ile limitli; yeni bağlantı öncekileri geçersiz kılar, kaydın ömrünü uzatmaz | *deneme kaydı* · *bağlantı kaydı* · Customer | deneme satırı; geçerli tek bağlantı; ilk bağlantının gönderildiği an | eşlendi |
| `02 §6.5.15` | Geri alma bağlantısı açıkken hesap silinmez; ekran bağlantının bitiş tarihini söyler | *bağlantı kaydı* | bağlantının bitiş anı | eşlendi |
| `02 §6.5.16` | Google'ın doğrulanmış e-postası bekleyen kayda eşleşir: kayıt devralınmaz, adı ve şifresiyle silinir; hesap Google ile şifresiz doğar; kapı kapalıysa giriş yok | Customer · SocialLogin | bekleyen kaydın silinmesi; şifresiz hesap; Google bağı | eşlendi |
| `02 §6.5.17` | Yeniden doğrulamada yanlış şifre L-1'e sayılır; işlem yapılmaz | *deneme kaydı* · Session | deneme satırı; tek işlemlik izin verilmez | eşlendi |

**§6.6 — katalog yönetimi (alt bölüm ve 4 satır)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §6.6` | 16 satır: referans fiyat, kategori silme ve taşıma, yayın kapısı, dosyasız dijital varyant, ondalık, kupon tutarları, kargoya verme süresinin çiti, eşzamanlı düzenleme, son yayındaki varyant, sıfır fiyat, %100 indirim, ayrılmış adet, kupon hakkı, açık siparişli ürünün silinmesi | veri doğurmaz — kod · Product · Variant · Category · Discount · Coupon · StockReservation · DigitalFile | sınırlar ve çitler kodda; fiyat iki ondalık; yayın durumu | eşlendi |
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
| `02 §6.7.8` | Ayıplı dijital dosya yeniden yüklenir; düzeltme eski alıcılara da ulaşır | DigitalFile · Variant | varyantın o anki dosyası — kalem dosyayı varyant üzerinden okur | eşlendi |
| `02 §6.7.11` | Teslim tarihi ve takip bilgisinin düzeltilmesi işlem izine; takip düzeltmesi bildirilir | OrderItem · Order · AuditLog · *gönderim kaydı* | `DeliveryDate`, kargo bilgisi; eski ve yeni değer izde | eşlendi |
| `02 §6.7.12` | Teslimat ya da fatura adresi "Kargoya verildi"ye kadar müşterinin talebiyle düzeltilir; teyit; müşteriye bildirilir | Order · OrderAddressHistory · AuditLog · *gönderim kaydı* | `ShippingAddress`, `BillingAddress`; iz satırı adres değerini taşımaz (`02 §10.3.1`) — eski ve yeni değer `OrderAddressHistory`'de (GAP-13) | eşlendi |
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
| `02 §7.1.6` | Kalemin işlem birimi adettir; ardışık işlemler kendi kaydı, damgası, geri ödemesi ve süresiyle; açık adet kayıtlardan okunur ve aşılamaz; gecikme feshinde seçim yok; dijitalin adedi bir; hizmette tamamlandı işareti açık adetlere | Cancellation · ItemRemoval · LateDeliveryTermination · Withdrawal · OrderItem · veri doğurmaz — türetilir | her kaydın adedi ve damgası; açık adet kayıtlardan türetilir , saklanmaz (K-787'nin park satırı; K-1015); kalemde durum alanı yok | eşlendi |
| `02 §7.1.7` | İşleme giren adedin tutarı donmuş değerlerden: kupon payı ve KDV orantılı ve aşağı yuvarlanır; son açık adetleri kapatan işlem kalanı alır; kargo adede bölünmez | OrderItem · Cancellation · ItemRemoval · LateDeliveryTermination · Withdrawal · Refund | kalemin donmuş birim fiyatı, kupon payı, KDV'si; her işlemin ayırdığı pay ve KDV (K-788'in park satırı) | eşlendi |
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
| `02 §7.3.1` | Cayma penceresi on dört gün, sabit; teslim tarihinden; malı teslim almadan cayma; geç işarette düğme kapanır, önceki beyan geçerli; eksik bilgilendirmede bir yıla uzama — kayıt §10.4.10'la | OrderItem · Withdrawal · LegalTextVersion · *gönderim kaydı* | `DeliveryDate`; beyanın damgası; bilgilendirmenin ispatı — siparişe donan form sürümü ve gönderim kaydı (Z-43); pencere kodda | eşlendi |
| `02 §7.3.2` | Dijitalde cayma yok; üçüncü kutu, kaydı siparişe donar; hak ödeme başarısında düşer | CheckboxAcceptance · Order | dijital kutunun kaydı; ödeme onayının anı | eşlendi |
| `02 §7.3.3` | Hizmette hak onaylı ifa tamamlanınca düşer; ifanın başladığı an izlenmez; kısmi ifadan cayma tam; pencere sipariş tarihinden ya da fiziksel kalemli siparişte teslimden; düğme ödeme onayından sonra | CheckboxAcceptance · OrderItem · Order · yoktur — kapsam dışı | hizmet kutusunun kaydı; tamamlandı işareti; sipariş tarihi, `DeliveryDate`; "ifaya başlandı" alanı yok | eşlendi |
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
| `02 §7.5.4` | Ayıplı dijitalde dosya güncellenir, eski alıcılara ulaşır | DigitalFile · Variant | varyantın o anki dosyası | eşlendi |
| `02 §7.5` metin | Garanti bilgisinin yeri §3.11.7 | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §7.6.1` | Panel "geri al" sunmaz; dört geçiş ve öteki işlemler geri alınamaz onayı ister; düzeltme sebep seçilen yeni geçişle, iz temizlenmez | *geçiş kaydı* · AuditLog · veri doğurmaz — arayüz (`04`) | düzeltme geçişi ve `CorrectionReason`; onayın kendisi saklanmaz | eşlendi |
| `02 §7.6.2` | Süresi dolup iptal edilmiş siparişe gelen havale işlenemez | veri doğurmaz — ürünün dışında | — (havale hattının geç parası ürün dışı — `02 §6.2.16`) | veri doğurmaz |
| `02 §7.6.3` | Müşteri akışında iptal ya da teslim edilmiş sipariş yeniden açılmaz; yöneticinin düzeltmesi §10.4.6'da | veri doğurmaz — kod | — (geçiş kuralı kodda; düzeltme `02 §10.4.6`) | veri doğurmaz |
| `02 §7.6.4` | Kurumsal içerikte sürüm geçmişi yok, silme geri alınmaz | yoktur — kapsam dışı · Content | sürüm geçmişi tablosu yok; silme fiziksel | eşlendi |
| `02 §7.6.5` | Silinmiş hesabın siparişleri hiçbir hesaba geri bağlanmaz | Order | hesap bağı silmede kalıcı olarak kalkar | eşlendi |
| `02 §7.6.6` | Yöneticinin düzeltmeleri ileri yönlü ve tutarı değiştirmez: adres, adet ya da çıkarma, takip, teslim tarihi, durum; kalem eklenemez, fiyat değişmez | Order · ItemRemoval · *geçiş kaydı* · AuditLog | onaylanan tutar değişmez — donan alanlar; düzeltmelerin izi | eşlendi |
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
| `02 §8.1.5` | Üç işlem yeniden doğrulama ister; şifresiz hesapta Google ile; yeni e-posta doğrulanmadan geçersiz; eski adrese geri alma bağlantısı | Session · PendingEmailChange · *bağlantı kaydı* | yeniden doğrulamanın tek işlemlik izni (K-899); bekleyen adres; geri alma bağlantısının ömrü | eşlendi |
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
| `02 §8.2` L-9 | Doğrulama bağlantısının yeniden istenmesi; müşteri ve yönetici; IP ve e-posta; kayıt silinmez; yeni davet limite girmez; bekleyen e-posta değişikliğinin yerine geçen istek sayılır | *deneme kaydı* · *bağlantı kaydı* · PendingEmailChange | deneme satırı; önceki bağlantı ömrü içinde geçerli | eşlendi |
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
| `02 §8.3.7` | Siparişin adresi yalnız firmanın teyidiyle "Kargoya verildi"ye kadar düzeltilir; B-10, işlem izine; eski ve yeni adres adres geçmişinde | Order · OrderAddressHistory · AuditLog · *gönderim kaydı* | `ShippingAddress`; iz satırı adres değerini taşımaz (`02 §10.3.1`) — eski ve yeni değer `OrderAddressHistory`'de (GAP-13) | eşlendi |
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
| `02 §9.2` B-1 | Sipariş alındı — e-posta düzeltmesinde yeni adrese yeniden; numara, bağlantı, donan sürümlerle form ve sözleşme tam metin | *gönderim kaydı* · Order · LegalTextVersion | gönderim satırı; `OrderNumber`, `OrderAccessKey`; siparişe donan sürümler | eşlendi |
| `02 §9.2` B-2 | Havale/EFT seçildi — donan IBAN ve numara | *gönderim kaydı* · Order | siparişe donan IBAN | eşlendi |
| `02 §9.2` B-3 | Havale ödeme hatırlatması — son iş gününün başında bir kez; düzeltmeyle yeniden başlayan sürede bir kez daha | *gönderim kaydı* · Order | ödeme süresinin bittiği an; hatırlatmanın gönderildiği — sürede bir kez (gönderim kaydından okunur) | eşlendi |
| `02 §9.2` B-4 | Ödeme onaylandı — dijitalde indirmenin hazır olduğu | *gönderim kaydı* | gönderim satırı | eşlendi |
| `02 §9.2` B-5 | Kargoya verildi — kargo şirketi, takip numarası, "Takip et" ya da kendi aracı | *gönderim kaydı* · Order | kargo şirketi, takip numarası, kendi aracıyla teslim beyanı | eşlendi |
| `02 §9.2` B-6 | Teslim edilemedi | *gönderim kaydı* | gönderim satırı | eşlendi |
| `02 §9.2` B-7 | İptal edildi — sipariş ya da kalem; sebep, kalem ve adet; m.16/4 bildirimi; kapanışlarda gitmez | *gönderim kaydı* · Cancellation | `CancellationReason`, kalem, adet | eşlendi |
| `02 §9.2` B-8 | Geri ödeme işlendi — geç gelen ödemenin geri ödemesi dahil | *gönderim kaydı* · Refund | geri ödemenin tutarı ve anı | eşlendi |
| `02 §9.2` B-9 | Cayma beyanı, gecikme feshi ya da ayıp talebi alındı — panel kaydı ve yeniden açma dahil; tarih damgası, kalemler ve adetler, iade adresi | *gönderim kaydı* · Withdrawal · LateDeliveryTermination · DefectClaim | damga, kalem, adet; beyana donan iade adresi | eşlendi |
| `02 §9.2` B-10 | Teslimat ya da fatura adresi düzeltildi | *gönderim kaydı* · Order | düzeltilen adres | eşlendi |
| `02 §9.2` B-11 | Kalem çıkarıldı ya da adedi azaltıldı | *gönderim kaydı* · ItemRemoval | değişen kalem ve adet | eşlendi |
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
| `02 §10.1` m4 | Katalog: ad ve yayın durumuyla arama; ürün, varyant, stok, fiyat, KDV, indirim, kupon, kategori, görsel, dosya; ifa süresi, üretim yeri, ölçü birimi; kalıcı silme; iade malını stoğa ekleme — elle sıralama ve toplu işlem yok | Product · Variant · Discount · Coupon · Category · Image · DigitalFile · yoktur — kapsam dışı | sıra alanı yok — kategoriler alfabetik | eşlendi |
| `02 §10.1` m5 | Kurumsal içerik ve site: içerik, ana sayfa düzeni, menü adları, duyuru, sosyal bağlantılar — sayfa kurucu ve kod yok; Hakkımızda silinmez | Content · AboutPage · Announcement · SocialLink · *firma ayarları* | `HomepageLayout`, menü adları | eşlendi |
| `02 §10.1` m6 | Marka: logo, marka adı, site simgesi, marka rengi; platform imzası | Branding · *firma ayarları* | `Logo`, `BrandName`, `Favicon`, `BrandColor`; platform imzası anahtarı | eşlendi |
| `02 §10.1` m7 | Yasal metinler: aydınlatma ve çerez politikasını düzenler ve yayına alır; form ve sözleşme ayarlardan; yayındaki metin yayından çekilmez | PrivacyNotice · CookiePolicy · LegalTextVersion | sürümler — yayındaki sürüm geri çekilmez | eşlendi |
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
| `02 §10.2.4` | Yönetici kimlik doğrulaması müşteriyle aynı; e-posta değişikliği işlem izine, iletişim e-postasını değiştirmez; ad hesabından değişir, yeniden doğrulama istemez, müşteriye görünmez | Admin · AuditLog · PendingEmailChange | yöneticinin adı ve e-postası; değişiklik izi | eşlendi |
| `02 §10.2.5` | Yöneticinin sepeti ve siparişi yok; ayrı hesap türleri, ayrı kapılar; e-posta hesap türü içinde tekil, büyük-küçük harf ayrımsız karşılaştırılır, girildiği gibi kalır | Admin · Customer · Session | e-posta — hesap türü içinde tekil, harf ayrımsız (K-982); `AccountType` | eşlendi |
| `02 §10.2.6` | Kullanılmamış davet geri çekilir; bir adres için tek geçerli davet; yönetici adresine davet yok; gönderme ve geri çekme işlem izine | AdminInvitation · AuditLog | davetin hâli (geçerli, kullanıldı, geri çekildi, geçersizleşti), adresi, gönderim anı (Z-3) | eşlendi |
| `02 §10.2.7` | Panele erişimin kaybında kurtarma kurulum tarafında; kurulum işlemi işlem izine yazılmaz | veri doğurmaz — kurulum · yoktur — kapsam dışı | kurtarma kodu alanı yok | veri doğurmaz |
| `02 §10.3.1` | İz sekiz alanda: kimlik ve marka; yayın durumu; siparişe yönelik yönetici işlemleri (teslim sırası cevabı, IBAN isteğinin tarihi dahil) — kişisel veri değer olarak girmez, adres düzeltmesinde sipariş, alan ve işlem — değerler adres geçmişinde —, iç notun metni girmez; yönetici hesapları; yasal metin sürümü; her ayar eski ve yeni değerle; dışa aktarma; üye silme — işlem ve tarih; her satır kim, ne zaman, ne değişti; fiyat, stok ve içerik metni ize girmez | AuditLog | yapan yönetici (adı, e-postası), an, alan (sekizden biri), işlemin adı, ilgili kayıt (sipariş, ayar, hesap), ayarlarda ve tarihlerde eski ve yeni değer; sipariş satırında kişisel veri değeri yok | eşlendi |
| `02 §10.3.2` | İz değiştirilemez ve silinemez; düzeltme yeni satır | AuditLog | yalnız ekleme — güncelleme ve silme yok (`05`'in mekanizması) | eşlendi |
| `02 §10.3.3` | Sistem olayları ize yazılmaz: "e-posta ulaşmadı" satırında, imha imha kaydında (tarih, tür, sayı; en az üç yıl, Z-44), giriş denemeleri giriş kaydında | *gönderim kaydı* · *imha kaydı* · *giriş kaydı* | imha çalışmasının tarihi, kayıt türü, sayısı — kişisel veri yok | eşlendi |
| `02 §10.3.4` | İz panelde düz liste; tarih ve yönetici süzgeci | veri doğurmaz — arayüz (`04`) · AuditLog | süzgeç alanları: an, yapan | eşlendi |
| `02 §10.3.5` | İz on yıl saklanır; kaldırılan yöneticinin satırları adıyla | AuditLog | oluştuğu an (Z-30) | eşlendi |

**§10.4 — sipariş müdahaleleri (21)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §10.4.1` | Dokuz müdahale: adres, adet ya da çıkarma, takip, teslim ya da ulaşma tarihi, geçiş düzeltmesi, iç not, "mal dönmedi", başka kanal kaydı, misafir e-postası; her biri işlem izine; elle sipariş yok | AuditLog · Order · ItemRemoval · *geçiş kaydı* · NotReturnedClosure · Withdrawal · LateDeliveryTermination · OrderEmailHistory | müdahalenin iz satırı; elle sipariş alanı yok | eşlendi |
| `02 §10.4.2` | Adres yalnız "Kargoya verildi"ye kadar düzeltilir (fiziksel kalemsizde Teslim edildi'ye kadar); fatura kesildiyse firmanın aracı; düzeltme bildirilir; eski ve yeni değer adres geçmişinde, siparişle imha | Order · OrderAddressHistory · AuditLog | `ShippingAddress`, `BillingAddress` yerinde düzeltilir; iz satırı adres değerini taşımaz (`02 §10.3.1`) ; eski ve yeni değer — teslimat telefonu dahil —, yönetici ve tarih `OrderAddressHistory`'de, siparişin kişisel verisiyle imha; `02 §3.23.3` iki istisnayı sayar (GAP-13) | eşlendi |
| `02 §10.4` m1 | Kimin talebiyle: teslimat telefonu dahil, müşterinin talebi; siparişin kendi kanalına dönerek teyit; ispat firmada | Order · veri doğurmaz — ürünün dışında | teslimat telefonu; teyit kaydedilmez | eşlendi |
| `02 §10.4` m2 | Bildirim: B-10 düzeltilen adresle; işlem izine | *gönderim kaydı* · AuditLog | gönderim satırı; iz satırı | eşlendi |
| `02 §10.4` m3 | Kalan risk: kandırılan firma adresi çevirir; B-10 uyarır; paket yola çıktıysa düzeltilemez | veri doğurmaz — ürünün dışında | — | veri doğurmaz |
| `02 §10.4` m4 | Elenen yol: müşterinin sipariş sayfasından onayı — ara adım ve süre sorunu | yoktur — kapsam dışı | onay bekleyen ara durum yok | veri doğurmaz |
| `02 §10.4.3` | Kalem adedi azaltılır ya da kalem çıkarılır; kalem eklenmez, fiyat değişmez; parası §7.2.8'den, tutarı §7.1.7'den; havalede IBAN isteği kayıtla düşer; sayaçlar iptalin rejimiyle; sebep listesi yok; durum makinesinde iptal gibi; teslim işaretli kalem çıkarılmaz; kartta geri alınamaz onay; kalem kaydına düşer; fiziksel kalem kargoya verildikten sonra çıkarılamaz | ItemRemoval · OrderItem · Refund · RefundIbanRequest · AuditLog · *gönderim kaydı* | çıkarılan adet, an, yapan yönetici, ayrılan kupon payı ve KDV; sebep alanı yok — `ItemRemoval` (GAP-12) | eşlendi |
| `02 §10.4.4` | Kargo şirketi ve takip numarası düzeltilir; bildirilir | Order · AuditLog · *gönderim kaydı* | kargo şirketi, takip numarası; iz satırı | eşlendi |
| `02 §10.4.5` | Teslim tarihi düzeltilir — sipariş düzeyinde; pencere ve ayıp süresi düzeltilmiş tarihten; aynı gün sorusu; kargoya verildiği günden önce ve ileri olamaz; bildirim yok; izde eski ve yeni tarih; ulaşma tarihi aynı kalıpla | Order · OrderItem · Return · AuditLog | `DeliveryDate` (fiziksel kalemlerin tamamı birlikte); kargoya verildiği an; ulaşma tarihi; izde eski ve yeni tarih | eşlendi |
| `02 §10.4.6` | Yanlış geçiş yeni geçişle düzeltilir: kapalı sebep listesi ve "diğer"de açıklama; terminalden yalnız bir önceki duruma; iptal düzeltmesinin koşulu ve yeniden ayırma; havale "ödendi" düzeltmesi iki ekseni birlikte geri alır, ödeme süresi yeniden başlar; her düzeltme işlem izine | *geçiş kaydı* · AuditLog · Order · StockReservation · Coupon · *gönderim kaydı* | `CorrectionReason` ve açıklaması; düzeltilen geçiş; yeniden ayırma; ödeme süresinin yeni başlangıcı (Z-8); B-3'ün yeni sürede bir kez daha | eşlendi |
| `02 §10.4.7` | İç not müşteriye görünmez; yazılması ve değiştirilmesi işlem izine (metin değil); not siparişin saklama süresini izler; kişisel veri riski | Order · AuditLog | iç not — siparişte tek serbest metin, düzenlenebilir; imhası siparişle; iz satırı metinsiz | eşlendi |
| `02 §10.4.8` | Firma iptalinin sebep listesi §7.2.4'te, geri alınamaz onaylar §5.9'da | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §10.4.9` | "Mal dönmedi" kapanışı: gönderme süresi geçince, ulaşmamış adetlere; işlem izine, bildirim yok, geri ödeme yok, IBAN silinir; mal ulaşırsa yeniden açılır; kapanış borca karar vermez — kaleme bağlı tutar bazlı geri ödeme, sonradan düşülür; kendiliğinden kapanış yok | NotReturnedClosure · Return · Refund · RefundIbanRequest · AuditLog | kapanışın adedi, anı, yapanı; yeniden açılma; tutar bazlı geri ödemenin kalem bağı | eşlendi |
| `02 §10.4.10` | Başka kanaldan cayma kaydı: kalem, adet, bildirimin ulaştığı tarih (geçmişe dönük); teyit; IBAN yalnız iletişim e-postasından ve denetimden geçerse; IBAN'sız kayıtta istek ve B-14; beyanın tarihi girilen tarih; aynı gün sorusu; pencere dışı ve mutlak istisnada ayrı onay, bir yıl sınırı; gönderim tarihi tutulmaz; kargoya verilmemiş kalemde iptal; gecikme feshinin kaydı — kalem ve adet seçilmez | Withdrawal · LateDeliveryTermination · RefundIbanRequest · Cancellation · AuditLog · *gönderim kaydı* | beyanın kaynağı (panel kaydı), bildirimin ulaştığı tarih ile kaydın anı, adet; pencere dışı ya da istisna onayı; teslim sırası cevabı; bildirimin gönderim tarihi alanı yok | eşlendi |
| `02 §10.4.11` | Misafir siparişinin iletişim e-postası müşterinin talebiyle düzeltilir | Order · OrderEmailHistory | siparişin iletişim e-postası — donmanın iki istisnasından biri (GAP-13) | eşlendi |
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
| `02 §11.2` P-24 | Ürün başına görsel adedi 20 — ürün ve varyantlar birlikte | veri doğurmaz — kod · Image | görselin ürün bağı | eşlendi |
| `02 §11.2` P-25 | İçerik kaydı başına görsel adedi 20 | veri doğurmaz — kod · Image | görselin içerik bağı | eşlendi |
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
| `02 §12.1.5` | Fiyat KDV dahil; üretim yeri ve yerli üretim logosu; ölçüyle satılan üründe birim fiyat; fiyatın başlangıç tarihi | Variant · Product · PriceHistory · veri doğurmaz — türetilir | KDV dahil fiyat; üretim yeri, ölçü birimi ve net miktar; fiyatın başlangıç anı; birim fiyat türetilir | eşlendi |
| `02 §12.1.6` | İndirim beyanı referans fiyata dayanır: on gün içindeki en düşük ya da hemen önceki fiyat; referans kaleme donar; başlangıç ve bitiş gösterilir | PriceHistory · Discount · OrderItem | uygulanmış fiyatlar ve anları; indirimin başlangıç ve bitişi; kaleme donan `ReferencePrice` | eşlendi |
| `02 §12.1.7` | Mesafeli satış: iki kutu; sürümler ve firma kimliği donar; dijital ve hizmet onayı gösterilen metniyle donar; onay sırasında sürüm artarsa sipariş durur; metinler B-1'de tam; işlem rehberi; dört metin sürümlü, yeniden onay yok | CheckboxAcceptance · LegalTextVersion · Order · CustomPage · *gönderim kaydı* | kutuların kaydı ve gösterilen metni; siparişe donan sürümler ve firma kimliği; işlem rehberi sayfası (`02 §3.24.8`) | eşlendi |
| `02 §12.1.8` | Teslim: taahhüt edilen süre, en geç otuz gün; kişiye özel üretimde de; gecikme feshi kalemlerle sınırlı; kanuni faiz firmada; firma iptali m.16/4 | LateDeliveryTermination · Order · Cancellation | siparişin firmaya ulaştığı an; fesih kaydı; sebepli iptal | eşlendi |
| `02 §12.1.9` | Cayma, iade ve ayıbın özeti; kuralların tamamı §7'de | Withdrawal · Return · DefectClaim | — (§7'nin satırlarıyla, §1.1.8) | eşlendi |
| `02 §12.1.10` | Fatura ürünün dışında; ürün fatura verisini taşır ve dışa aktarır; fatura saklanmaz | yoktur — kapsam dışı · Order | fatura alanı yok; donmuş fatura verisi siparişte (`02 §3.25`) | eşlendi |
| `02 §12.1.11` | Ticari elektronik ileti yok; İYS yok | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §12.1.12` | Ürün ne satıldığını denetlemez; izinli ürünler kapsam dışı | yoktur — kapsam dışı | ürün türü denetim alanı yok | veri doğurmaz |
| `02 §12.2.1` | Veri sorumlusu firma; bakım ve destekte geliştirici veri işleyen | veri doğurmaz — ürünün dışında | — | veri doğurmaz |
| `02 §12.2.2` | Aydınlatma metni sürümlü ayrı kayıt; zorunlu, boşaltılamaz; kapı; bağlantısı veri toplayan ekranlarda; onay ve "gördü" kaydı yok; sipariş o günkü sürümü dondurur | PrivacyNotice · LegalTextVersion · Order · yoktur — kapsam dışı | sürümler; siparişe donan aydınlatma sürümü; kullanıcı bazında görme kaydı yok | eşlendi |
| `02 §12.2.3` | Hukuki sebepler m.5/2'nin sayımıyla; açık rıza yok | veri doğurmaz — ürünün dışında | — (metnin içeriği) | veri doğurmaz |
| `02 §12.2.4` | Açık rıza toplanmaz; rıza ekranı ve kaydı yok; onay kutuları sözleşmeye ilişkin | yoktur — kapsam dışı · CheckboxAcceptance | rıza kaydı yok | eşlendi |
| `02 §12.2.5` | Yalnız zorunlu çerezler; misafir sepetinin çerezi kalıcı; ödeme çerçevesinin çerezleri politikada; onay bandı yok; çerez politikası sürümlü; ilk yayından önce çerez yazılmaz | veri doğurmaz — mimari (`05`) · CookiePolicy · LegalTextVersion · Cart · Session · RecognizedBrowserMark | çerez politikasının sürümleri; misafir sepetinin tarayıcı anahtarı (Z-6); oturum ve tanınan tarayıcı kayıtları sunucuda | eşlendi |
| `02 §12.2.6` | KVKK talebi tipi başvuru kanalı; kimlik doğrulama ve otuz gün firmada; sayaç yok; veri indirme yok | ContactRequest · yoktur — kapsam dışı | `ContactTopic` "KVKK talebi"; süre sayacı yok | eşlendi |
| `02 §12.2.7` | Saklama ve imha: günlük iş, düğme yok, ize yazılmaz; her imha çalışması ve anında silme imha kaydına (tarih, tür, sayı; en az üç yıl); ödemesi alınmamış siparişin kişisel verisi üç yılda; geç ödeme siparişi ödenmiş yapar; on yıllık saklamanın dört kaydı; siparişte kişisel alanlar — adres ve e-posta geçmişi dahil — boşaltılır, hesap bağı kesilir; hesap silmede giriş bilgileri, oturumlar, adres defteri, sepet silinir; IBAN'ın silinmesi; iz kişisel veri taşımaz | *imha kaydı* · Order · Customer · Session · AddressBook · Cart · RefundIbanRequest · *gönderim kaydı* · ContactRequest · OrderEmailHistory · OrderAddressHistory · PendingEmailChange · AuditLog | imha kaydının tarih, tür, sayısı (Z-44); siparişin ödemesi alınmış mı (Z-28, Z-41) ve oluştuğu yıl; kişisel alanların boşaltılması ve hesap bağının kesilmesi; kalan ticari kaydın alanları | eşlendi |
| `02 §12.2.8` | Ürün kişisel veriyi yurt dışına aktarmaz; kurulumdan doğan aktarım firmada; ödeme sağlayıcısına alıcı verisi; Google girişi aktarım değil | veri doğurmaz — ürünün dışında · veri doğurmaz — kurulum | — | veri doğurmaz |
| `02 §12.2.9` | Veri ihlali: ayrı kayıt yok; iz, giriş kaydı, sistem kayıtları; bildirim firmada; ulaşma bilgisi dışa aktarmalardan | AuditLog · *giriş kaydı* · yoktur — kapsam dışı | ihlal kaydı tablosu yok | eşlendi |
| `02 §12.2.10` | Yönetim tarafında veri erişimi ayrımı yok | yoktur — kapsam dışı | — | veri doğurmaz |
| `02 §12.2.11` | İçerikte üçüncü kişi verisinin izni firmada | veri doğurmaz — ürünün dışında | — | veri doğurmaz |
| `02 §12.2.12` | Veri asgariliği §8.4'te | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §12.3.1` | Veri firmanındır; abonelik bitse de kurulum çalışır; uyum korunmaz | veri doğurmaz — ürünün dışında | — | veri doğurmaz |
| `02 §12.3.2` | Çıkış hakkı barındırma düzleminde; sipariş tarafı panelden dışa aktarılır | veri doğurmaz — mimari (`05`) | — | veri doğurmaz |
| `02 §12.3.3` | Çalışma süresi taahhüdü yok; sipariş tarafında veri kaybına tolerans sıfır — disk kaybında en çok bir dakika | veri doğurmaz — mimari (`05`) | — (`02 §3.34.7`, §3.34.8; `05`) | veri doğurmaz |
| `02 §12.4.1` | Erişilebilirlik yasal yükümlülük; taban Kontrol Listesi A ve WCAG 2.2; beyan verilmez | veri doğurmaz — doğrulama (`12`) · Image | görselin alternatif metni (`02 §3.34.5`) | eşlendi |

**§12.5 — firmanın yükümlülükleri (34)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §12.5` m1 | ETBİS kaydı — isteğe bağlı alan ve bant; kayıt ve güncellemeler firmada | Company · veri doğurmaz — ürünün dışında | ETBİS doğrulama bilgisi | eşlendi |
| `02 §12.5` m2 | Yasal kimliğin doğruluğu — zorunlu alan ve işlem izi | Company · AuditLog | kimlik alanları; değişiklik izi | eşlendi |
| `02 §12.5` m3 | Fiyat etiketinin birim fiyatı — ölçü birimi ve net miktar | Product · veri doğurmaz — türetilir | ölçü birimi, net miktar; birim fiyat türetilir | eşlendi |
| `02 §12.5` m4 | VERBİS — ürünün sağladığı yok | veri doğurmaz — ürünün dışında | — | veri doğurmaz |
| `02 §12.5` m5 | Aydınlatma metni ve çerez politikası — taslak, yer tutucular, sürüm | PrivacyNotice · CookiePolicy · LegalTextVersion | metnin içeriği ve sürümü; taslak başlangıç verisi (VM8) | eşlendi |
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
| `02 §12.5` m23 | Kargo ücretinin KDV'sinin bölüşümü — hesap ve döküm, dışa aktarmada | Order · veri doğurmaz — türetilir | kargo ücretinin bölünmüş KDV'si — onayda siparişte donar (K-1015) | eşlendi |
| `02 §12.5` m24 | Gecikme feshinde kanuni faiz — uyarı; sistem faizi hesaplamaz | LateDeliveryTermination · yoktur — kapsam dışı | faiz alanı yok | eşlendi |
| `02 §12.5` m25 | Güncelleme almayan kurulumun yasal uyumu | veri doğurmaz — ürünün dışında | — | veri doğurmaz |
| `02 §12.5` m26 | Bakım, destek ve altyapı sağlayıcılarıyla veri işleme sözleşmesi | veri doğurmaz — ürünün dışında | — | veri doğurmaz |
| `02 §12.5` m27 | Teslim tarihinin ve ulaşma tarihinin doğruluğu — tarih alanları ve iz | OrderItem · Return · AuditLog | `DeliveryDate`, ulaşma tarihi — ileri tarihli olamaz | eşlendi |
| `02 §12.5` m28 | İade reddinin ispatı ve malın geri gönderilmesi — ret kaydı, B-16, iz; kanıt alınmaz | ReturnRejection · AuditLog · *gönderim kaydı* | ret kaydı; kanıt alanı yok | eşlendi |
| `02 §12.5` m29 | Eski iade adresine gönderilen malın teslim alınması — beyana yazılan adres | Withdrawal · Return | beyana donan iade adresi; ulaşma tarihi | eşlendi |
| `02 §12.5` m30 | Geri ödemenin yasal süre içinde yapılması — kalan süre (Z-16, Z-17) | Refund · veri doğurmaz — türetilir | kalan süre türetilir | eşlendi |
| `02 §12.5` m31 | Dışa aktarılan dosya — dışa aktarmanın izi | AuditLog | dışa aktarma satırı | eşlendi |
| `02 §12.5` m32 | Misafir e-postasını düzeltirken kimlik teyidi — eski anahtarın geçersizleşmesi, B-15, iz, e-posta geçmişi | OrderEmailHistory · Order · AuditLog | eski ve yeni adres; `OrderAccessKey` | eşlendi |
| `02 §12.5` m33 | Siparişin adresini düzeltirken talebin teyidi — düzeltme ekranı, B-10, iz satırı, adres geçmişi | Order · OrderAddressHistory · AuditLog · *gönderim kaydı* | adres düzeltmesinin iz satırı; `OrderAddressHistory` (GAP-13) | eşlendi |
| `02 §12.5` m34 | Başka kanaldan cayma ya da fesih bildiriminin teyidi — kayıt ekranı, B-9, iz; IBAN sipariş sayfasından | Withdrawal · LateDeliveryTermination · RefundIbanRequest · AuditLog | panel kaydı; IBAN isteği | eşlendi |

**§13 — açık kararlar (4)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §13.1` | Tablo `02`'nin kendi açık kararlarını taşır; dönemin kaydı tracker §4 | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §13.2` | Aşama kapanışında tracker §4'ün açık satırları dokümana atanır | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §13.3` | Kapanıştan önce çakışma taraması koşar | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `02 §13.4` | Yazım turu sonunda `02`'ye ait açık karar yok; kapanan satırların listesi | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |

#### 1.1.9 Kullanıcı Akışları — `03 §1` ve §4, satır düzeyi (247)

Kullanıcı Akışları kural koymaz, `02`'nin kurallarını adıma çevirir (`03 §0.1.1`); bu iki bölüm verinin biçimini doğrudan taşıdığı için satır düzeyindedir (K-997). Birim K-1002'dir: numaralı satır (`03 §n.m.k` — tablo satırı ya da kalın numaralı paragraf, K-669), durum satırı (`03 §1.2` `Placed` — durum kodu ve bağlamı; aynı kod bir alt bölümde iki tabloda geçiyorsa bağlam kalın paragrafın numarasıdır: `03 §1.8.1` `Draft`, `03 §1.8.4` `Draft`), geçiş satırı (`03 §1.2` S1, `03 §1.3` Ö1), kimliksiz tablo satırı ve liste maddesi (`03 §1.1` m1 — bağlamındaki sırası) ve bağlamın kural ve madde dışındaki metni (`03 §1.6.1` metin) — bağlam, alt bölümün içinde son kalın numaralı paragraf, yoksa alt bölümdür. Bir geçişin bıraktığı iz — kim, ne zaman, hangi geçiş, hangi sebeple — *geçiş kaydı*na eşlenir (park K-889); durum ve geçiş kodları `02 §1.2` ve §5'ten birebirdir. Satırın kuralı `02`'deyse "Alan" hücresi `02`'nin satırına (§1.1.4–§1.1.8) işaret eder; burada yalnız Kullanıcı Akışları'nın veriye kattığı ayrıntı yazılır.

**`03 §1` — bölüm girişi ve §1.1 envanter (8)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `03 §1` metin | Durum adları, kodları ve koşulları `02 §5`'te; bölüm geçişi kimin tetiklediğini, hangi akışta olduğunu ve bildirimini yazar | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `03 §1.1` m1 | Sipariş — sevkiyat ekseni: altı durum (`02 §5.4`) | Order | `OrderStatus` — `Placed`, `Preparing`, `Shipped`, `Delivered`, `DeliveryFailed`, `Cancelled` | eşlendi |
| `03 §1.1` m2 | Sipariş — ödeme ekseni: beş durum (`02 §5.5`) | Order | `PaymentStatus` — `Pending`, `Paid`, `Failed`, `PartiallyRefunded`, `Refunded` | eşlendi |
| `03 §1.1` m3 | Ürün ve varyant — üç yayın durumu (`02 §5.1`) | Product · Variant | `PublishStatus` — `Draft`, `Published`, `Archived` (iki düzlemde) | eşlendi |
| `03 §1.1` m4 | Kurumsal içerik kaydı — iki yayın durumu (`02 §5.2`) | Content | `PublishStatus` — `Draft`, `Published` | eşlendi |
| `03 §1.1` m5 | Ayıp talebi — iki durum (`02 §5.10`) | DefectClaim | `DefectClaimStatus` — `Open`, `Resolved` | eşlendi |
| `03 §1.1` m6 | İletişim talebi — KVKK talebi dahil, iki durum (`02 §5.13`) | ContactRequest | `ContactRequestStatus` — `Open`, `Closed` | eşlendi |
| `03 §1.1` metin | Durum makinesi taşımayanlar: kalem (kayıtlar), indirim, kupon, stok ayırma, hesap, oturum, davet (geçerlilik); duyurunun tarih aralığı; satış açıklığı bir kapı | OrderItem · Discount · Coupon · StockReservation · Customer · Admin · Session · AdminInvitation · Announcement · veri doğurmaz — türetilir | kalemde durum alanı yok; geçerlilik alanları (tarih aralığı, ömür, kullanım) satırlarında (§1.10); satış açıklığı ayarlardan türer (`02 §3.1.5`) | eşlendi |

**§1.2 — sevkiyat ekseni: durumlar ve geçişler (18)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `03 §1.2` `Placed` | Alındı — müşterinin onayıyla doğar; ödeme ya da fizikselsiz siparişte teslim işaretleri beklenir; S1, S2, S3 | Order | `OrderStatus` = `Placed` (başlangıç değeri) | eşlendi |
| `03 §1.2` `Preparing` | Hazırlanıyor — ödenmiş fiziksel sipariş; Z-10 açık fiziksel kalem varken işler; S5, S4, S10 | Order | `OrderStatus` = `Preparing`; Z-10'un başladığı an ödeme onayının anı (`02 §4.2` Z-10) | eşlendi |
| `03 §1.2` `Shipped` | Kargoya verildi — iptal yolu kapandı, cayma açıldı; S6, S7 | Order | `OrderStatus` = `Shipped` | eşlendi |
| `03 §1.2` `DeliveryFailed` | Teslim edilemedi — gönderi döndü; S8, S9, S11 | Order | `OrderStatus` = `DeliveryFailed` | eşlendi |
| `03 §1.2` `Delivered` | Teslim edildi — terminal; yalnız yöneticinin düzeltmesiyle değişir | Order · OrderItem | `OrderStatus` = `Delivered`; kalemlerin teslim işareti | eşlendi |
| `03 §1.2` `Cancelled` | İptal edildi — terminal; teslimattan önce iptal ya da açık kalemsiz kapanış | Order | `OrderStatus` = `Cancelled`; kapanışın sebebi kalem kayıtlarından (K-592) | eşlendi |
| `03 §1.2` metin | Geçiş tablosunun okunuşu; beyaz liste dışı geçiş yasak; geri yönlü geçiş müşteri akışında yok; düzeltme §1.6 | veri doğurmaz — kod | — (geçiş kuralı kodda; düzeltme *geçiş kaydı*nda — §1.6.2) | veri doğurmaz |
| `03 §1.2` S1 | Alındı → Hazırlanıyor — sistem, Ö1 ile aynı anda; fiziksel kalem var; bildirim yalnız Ö1'in B-4'ü | *geçiş kaydı* · *gönderim kaydı* | S1; aktör sistem; an (Ö1'in anıyla aynı); B-4'ün gönderim satırı Ö1'e bağlı | eşlendi |
| `03 §1.2` S2 | Alındı → Teslim edildi — kapanış kuralını tamamlayan son olay: dijitalde ödeme onayı, son "tamamlandı", son açık kalemin iptali, çıkarılması ya da cayma beyanı | *geçiş kaydı* · OrderItem · Cancellation · ItemRemoval · Withdrawal | S2; aktör sistem — tetikleyen olayla aynı an; tetikleyen kalem kaydı; kalemlerin teslim işaretleri | eşlendi |
| `03 §1.2` S3 | Alındı → İptal edildi — müşteri (bütünüyle ya da son açık kalem), yönetici (sebepli), sistem (Z-7, Z-8, kart başarısız, aynı sepetten yeni sipariş) ya da kapanış (cayma, çıkarma; teslim yok — K-672); kapanışta B-7 yok | *geçiş kaydı* · Cancellation · *gönderim kaydı* | S3; aktör (müşteri · yönetici · sistem); an; firma iptalinde `CancellationReason`; kendiliğinden iptalin türü (`02 §5.3.4`); kapanışta iptal kaydı açılmaz — sebep kalem kayıtlarından | eşlendi |
| `03 §1.2` S4 | Hazırlanıyor → İptal edildi — müşteri ya da yönetici son açık kalemi iptal eder; kapanış (fesih, cayma, çıkarma) sistemin, olayla aynı anda | *geçiş kaydı* · Cancellation · LateDeliveryTermination · Withdrawal · ItemRemoval | S4; aktör; an; firma iptalinde sebep; kapanışta tetikleyen kalem kaydı | eşlendi |
| `03 §1.2` S5 | Hazırlanıyor → Kargoya verildi — yönetici, geri alınamaz onayla; yalnız açık fiziksel kalemler gider; B-5 | *geçiş kaydı* · Order · OrderItem | S5; aktör yönetici; an; kargo şirketi, takip numarası ya da kendi aracıyla teslim beyanı (`02 §5.4` S5); gönderiye giren kalemler | eşlendi |
| `03 §1.2` S6 | Kargoya verildi → Teslim edildi — yönetici teslim işaretini ve tarihini girer; kendiliğinden geçiş yok; bildirim yok | *geçiş kaydı* · OrderItem | S6; aktör yönetici; `DeliveryDate` (fizikselde sipariş düzeyinde tek tarih — `02 §5.8` m1) | eşlendi |
| `03 §1.2` S7 | Kargoya verildi → Teslim edilemedi — yönetici işaretler; B-6 | *geçiş kaydı* | S7; aktör yönetici; an | eşlendi |
| `03 §1.2` S8 | Teslim edilemedi → Kargoya verildi — yönetici açık fiziksel kalemleri yeniden gönderir, geri alınamaz (K-677); B-5 yeniden | *geçiş kaydı* · Order | S8; aktör yönetici; an; yeniden gönderimin kargo bilgisi | eşlendi |
| `03 §1.2` S9 | Teslim edilemedi → İptal edildi — yönetici sebepli iptal ya da açık kalemsiz, teslimsiz siparişin sebepsiz kapanışı (K-717) | *geçiş kaydı* · Cancellation | S9; aktör yönetici; firma iptalinde `CancellationReason`; kapanışta sebep yok — iptal kaydı açılmaz | eşlendi |
| `03 §1.2` S10 | Hazırlanıyor → Teslim edildi — son açık fiziksel kalemin iptali, çıkarılması ya da feshi; son hizmet kaleminin iptali, çıkarılması, tamamlanması ya da caymayla kapanması | *geçiş kaydı* · OrderItem | S10; aktör sistem — tetikleyen olayla aynı an; tetikleyen kalem kaydı | eşlendi |
| `03 §1.2` S11 | Teslim edilemedi → Teslim edildi — S7'nin kendisi dahil, geri dönen fiziksel kalemler kapanmışsa | *geçiş kaydı* | S11; aktör sistem ya da S7 ile aynı an | eşlendi |

**§1.3 — ödeme ekseni: durumlar ve geçişler (11)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `03 §1.3` `Pending` | Bekliyor — ödeme gelmedi; Ö1, Ö2 | Order | `PaymentStatus` = `Pending` (başlangıç değeri) | eşlendi |
| `03 §1.3` `Paid` | Ödendi; Ö3, Ö4 | Order | `PaymentStatus` = `Paid` | eşlendi |
| `03 §1.3` `Failed` | Başarısız — terminal; sebep iptal kaydında | Order · Cancellation | `PaymentStatus` = `Failed`; sebep iptal kaydından | eşlendi |
| `03 §1.3` `PartiallyRefunded` | Kısmen geri ödendi; Ö5 | Order | `PaymentStatus` = `PartiallyRefunded` | eşlendi |
| `03 §1.3` `Refunded` | Geri ödendi — terminal | Order | `PaymentStatus` = `Refunded` | eşlendi |
| `03 §1.3` Ö1 | Bekliyor → Ödendi — sistem (sağlayıcı bildirimi ya da son sorgunun olumlu cevabı) ya da yönetici (havale işareti, geri alınamaz); B-4 | *geçiş kaydı* · *ödeme kaydı* · *gelen bildirim kaydı* | Ö1; aktör (sistem · yönetici); an; ödemenin kaynağı — sağlayıcının işlem kimliği ya da havale işareti | eşlendi |
| `03 §1.3` Ö2 | Bekliyor → Başarısız — S3 ile aynı anda, ödenmemiş siparişin her iptalinde | *geçiş kaydı* | Ö2; S3 ile aynı an ve aktör | eşlendi |
| `03 §1.3` Ö3 | Ödendi → Kısmen geri ödendi — sistem (kart iadesi gerçekleşince) ya da yönetici (havale, cayma, ayıp çözümü, tutar bazlı; geri alınamaz); B-8, sistemin iadesinde F-4 | *geçiş kaydı* · Refund · *geri ödeme kaydı* · *gönderim kaydı* | Ö3; aktör; geri ödemenin türü, tutarı, yolu, kalem bağı, işleyen; kendiliğinden olduğu (F-4) | eşlendi |
| `03 §1.3` Ö4 | Ödendi → Geri ödendi — Ö3'ün tetikleyenleri, paranın tamamı | *geçiş kaydı* · Refund | Ö4; tutarların toplamı ödenmiş tutara eşit (türetilir) | eşlendi |
| `03 §1.3` Ö5 | Kısmen geri ödendi → Geri ödendi — kalan paranın tamamı | *geçiş kaydı* · Refund | Ö5 | eşlendi |
| `03 §1.3` metin | Ekseni değiştirmeyen olaylar: kart iadesinin başarısızlığı ya da bilinmeyen sonucu (işaret), iptal edilmiş siparişe gelen kart ödemesi ve kendiliğinden geri ödemesi, ters ibraz, cayma ve teslim alma; Başarısız → Ödendi yasak; tek düzeltme havale işareti | *geri ödeme kaydı* · Order · *ödeme kaydı* · Refund · yoktur — kapsam dışı | geri ödeme kaydının hâli; `RefundFailedMark`, `RefundOutcomePendingMark` (K-940); geç gelen ödeme ve kendiliğinden geri ödemesi (K-684); ters ibraz alanı yok (`02 §3.21.11`) | eşlendi |

**§1.4 — iki eksenin bağı (27)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `03 §1.4.1` | Birleşim tablosu — her siparişin her an iki durumu var | Order | `OrderStatus` × `PaymentStatus` birleşim kısıtı (uygulama düzeyi — `02 §5.6.1`) | eşlendi |
| `03 §1.4.1` m1 | Alındı satırı: Bekliyor, Ödendi (fizikselsiz), Kısmen ve tamamen geri ödendi izinli; Başarısız yok | Order | birleşim kısıtı | eşlendi |
| `03 §1.4.1` m2 | Hazırlanıyor satırı: Bekliyor ve Başarısız yok | Order | birleşim kısıtı | eşlendi |
| `03 §1.4.1` m3 | Kargoya verildi satırı: Bekliyor ve Başarısız yok | Order | birleşim kısıtı | eşlendi |
| `03 §1.4.1` m4 | Teslim edilemedi satırı: Bekliyor ve Başarısız yok | Order | birleşim kısıtı | eşlendi |
| `03 §1.4.1` m5 | Teslim edildi satırı: Bekliyor ve Başarısız yasak | Order | birleşim kısıtı | eşlendi |
| `03 §1.4.1` m6 | İptal edildi satırı: Bekliyor yok; Ödendi ve Kısmen geri ödendi geri ödeme beklenirken | Order | birleşim kısıtı | eşlendi |
| `03 §1.4.1` metin | Harflerin okunuşu: fizikselsiz siparişte Alındı + Ödendi; havale düzeltmesinde birlikte dönüş; tutar bazlı tam geri ödeme; son sorgu yanıtsızken Alındı + Bekliyor; geri ödemesi beklenen iptal; kart iadesinin sonucu beklenirken düzeltme yok; müşteri kaynaklı dönen gönderinin kalıcı son hâli | Order · Refund · *geri ödeme kaydı* · *geçiş kaydı* | birleşim kısıtı; geri ödemenin tutarı (ödenmiş ve geri ödenmemiş tutarı aşmaz — `02 §5.5`); `RefundOutcomePendingMark` düzeltmeyi kapatır (K-940) | eşlendi |
| `03 §1.4.2` | Eşzamanlı geçişler — iki ekseni ya da ekseni ve kalemi aynı anda değiştiren olaylar | *geçiş kaydı* | aynı anda yazılan geçişler — tek işlem (`05 §7.3`) | eşlendi |
| `03 §1.4.2.1` | Ödeme onayı: S1 ya da S2; dijital kalemlerin teslim işareti; fizikselsizde kapanış kuralı | *geçiş kaydı* · OrderItem | Ö1 ile S1 ya da S2 aynı an; dijital kalemin teslim işareti | eşlendi |
| `03 §1.4.2.2` | Ödenmemiş siparişin iptali: S3 + Ö2 | *geçiş kaydı* | iki geçiş aynı an | eşlendi |
| `03 §1.4.2.3` | Son açık kalemin kapanması ya da teslim işareti: kapanış kuralı | *geçiş kaydı* · OrderItem | kapanış geçişi olayla aynı an | eşlendi |
| `03 §1.4.2.4` | Havale işaretinin düzeltilmesi: Ödendi → Bekliyor ve Hazırlanıyor → Alındı birlikte | *geçiş kaydı* | iki düzeltme geçişi aynı an; `CorrectionReason` | eşlendi |
| `03 §1.4.3` | Kapanış kuralı — açık kalem adetle okunur; fizikselsiz siparişte son açık kalemin kapanması ya da teslim işareti hattı bitirir | OrderItem · Cancellation · ItemRemoval · LateDeliveryTermination · Withdrawal · veri doğurmaz — türetilir | kalemin açık adedi kayıtlardan türetilir , saklanmaz (K-787'nin park satırı; K-1015); kalemin donmuş `ProductType`'ı | eşlendi |
| `03 §1.4.3` m1 | En az bir kalem teslim edilmişse Teslim edildi (S2, S10, S11) | *geçiş kaydı* · OrderItem | teslim işaretleri | eşlendi |
| `03 §1.4.3` m2 | Hiçbir kalem teslim edilmemişse İptal edildi: iptal ya da kapanış; kargodayken yöneticinin S7 + S9 kapanışı (1.11.40) | *geçiş kaydı* · Cancellation | geçişin kapanış mı iptal mi olduğu — iptal kaydının varlığından; kapanışta sebep yok (K-672, K-717) | eşlendi |
| `03 §1.4.3` metin | Kart son sorgusu yanıtsızken ve kesintide dolan havale süresinde iptal ertelenir | Order | ödeme sonucu alınamadı işareti (`02 §6.1.2`); ertelenen son an (K-898; `02 §3.21.8`) | eşlendi |
| `03 §1.4.4` | Geçişlerin sayaçlara etkisi — stok, kontenjan, kupon hakkı | StockReservation · Variant · Coupon | ayırmanın hâli; stok ve kontenjan sayacı; kupon hakkı sayacı | eşlendi |
| `03 §1.4.4.1` | Sipariş oluşur: stok, kontenjan ve kupon hakkı ayrılır | StockReservation | ayrılan adet ve kupon hakkı; oluşma anı | eşlendi |
| `03 §1.4.4.2` | Ödeme onayı: ayrılan kesin düşer, kupon kullanılmış | StockReservation · Variant · Coupon | ayırmanın hâli; `StockQuantity`; kupon kullanım sayacı | eşlendi |
| `03 §1.4.4.3` | Ödenmeden iptal: serbest kalır, kupon geri döner | StockReservation | ayırmanın serbest kalması | eşlendi |
| `03 §1.4.4.4` | Ödenmiş kalemin iptali ve çıkarma: adet stoğa ve havuza döner; kupon yalnız tamamı iptalde | Cancellation · ItemRemoval · Variant · Coupon | iptal edilen ve çıkarılan adet (dönüşün kaynağı — K-791) | eşlendi |
| `03 §1.4.4.5` | Gecikme feshi: kargoya verilmemiş kalemde döner; kargodan dönen mal teslim almayla; kupon kapanışta | LateDeliveryTermination · Return · Variant · Coupon | feshedilen adet; teslim alınan adet | eşlendi |
| `03 §1.4.4.6` | Cayma ve iade: teslim alınan adet kadar eklemeyle; reddedilen dönmez; hizmet kontenjanı cayılan adet kadar | Return · ReturnRejection · Withdrawal · Variant · Coupon | teslim alınan adet, stoğa ekleme (ayrı adım); cayılan adet | eşlendi |
| `03 §1.4.4.7` | Yanlış iptalin düzeltilmesi: yeniden ayrılır, yetmezse düzeltme yok | StockReservation · *geçiş kaydı* | yeniden ayırma (K-566) | eşlendi |
| `03 §1.4.4.8` | Havale işaretinin düzeltilmesi: tutulu kalır | StockReservation | ayırmanın hâli — tutulu | eşlendi |
| `03 §1.4.4.9` | Varyantın kalıcı silinmesi: silinen varyantın ayırması düşer | StockReservation · Variant | varyant bağı — fiziksel silmeyle düşer (K-655) | eşlendi |

**§1.5 — tipe göre hatlar (8)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `03 §1.5` metin | Hatların tanımı `02 §5.7`'de; tiplere ayrı durum seti yok; zorunlu elle adım sayısı (`02 §10.5.1`) | veri doğurmaz — kod | — (hat kalemlerin donmuş `ProductType`'ından) | veri doğurmaz |
| `03 §1.5.1` | Fiziksel — kart: Ö1 ve S1 sistemin; S5, S6 yöneticinin; iki elle adım | veri doğurmaz — kod | — | veri doğurmaz |
| `03 §1.5.2` | Fiziksel — havale: Ö1'i yöneticinin işareti tetikler; üç elle adım | veri doğurmaz — kod | — | veri doğurmaz |
| `03 §1.5.3` | Yalnız dijital: Ö1 ve S2 | veri doğurmaz — kod | — | veri doğurmaz |
| `03 §1.5.4` | Yalnız hizmet: Ö1, son "tamamlandı" ile S2 | veri doğurmaz — kod | — | veri doğurmaz |
| `03 §1.5.5` | Dijital + hizmet: dijital ödeme onayında teslim, son "tamamlandı" ile S2 | veri doğurmaz — kod | — | veri doğurmaz |
| `03 §1.5.6` | Karışık, fiziksel kalemli: fiziksel hat; dijital ve hizmet kalem düzeyinde teslim | OrderItem | kalemin teslim işareti | eşlendi |
| `03 §1.5.7` | Fiziksel kalemleri kapanmış karışık: bulunduğu durumda kapanış kuralıyla biter | veri doğurmaz — kod | — | veri doğurmaz |

**§1.6 — geri alınamaz geçişler ve düzeltme (16)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `03 §1.6.1` | Geri alınamaz onay isteyen işlemler — sonuç tek cümleyle; "geri al" düğmesi yok | veri doğurmaz — arayüz (`04`) | — (onayın kendisi saklanmaz; işlemin kaydı satırlarında) | veri doğurmaz |
| `03 §1.6.1.1` | Havale "ödendi" işareti — Ö1 ve eşzamanlıları; dijital kalemde düzeltilemeyeceği önceden söylenir | *geçiş kaydı* · *ödeme kaydı* | Ö1; aktör yönetici | eşlendi |
| `03 §1.6.1.2` | Kargoya verme ve yeniden gönderim — S5, S8 | *geçiş kaydı* | S5, S8; aktör yönetici | eşlendi |
| `03 §1.6.1.3` | Firma iptali — S3, S4, S9 ya da kalemin iptal kaydı; "stokta bulunamadı"da yasal uyarı | Cancellation · *geçiş kaydı* | `CancellationReason`; kalem ve adet | eşlendi |
| `03 §1.6.1.4` | Yöneticinin geri ödemeyi işlemesi — tutar bazlı dahil; Ö3–Ö5 | Refund · *geçiş kaydı* | geri ödemenin türü, tutarı, işleyen | eşlendi |
| `03 §1.6.1.5` | Kart hattında karta para gönderen kalem çıkarma | ItemRemoval · *geri ödeme kaydı* | çıkarma kaydı; sistemin kart iadesi | eşlendi |
| `03 §1.6.1.6` | İade reddi — kalemin iade reddi kaydı (K-656) | ReturnRejection | — | eşlendi |
| `03 §1.6.1.7` | Başka kanaldan gelen cayma ya da fesih kaydı — geri alınmaz, düzeltilmez (K-688, K-715) | Withdrawal · LateDeliveryTermination | kaydeden (yönetici); kaydın düzeltme alanı yok | eşlendi |
| `03 §1.6.1.8` | Üye hesabının talep üzerine silinmesi — geri alınmaz | Customer · *imha kaydı* | anında fiziksel silme; silme imha kaydına (K-708) | eşlendi |
| `03 §1.6.1` metin | Sistemin kart iadesi onay penceresi değil, F-4 ile bildirilir; siparişe dokunmayan onaylar kendi akışlarında; müşterinin geri alınamaz dört işlemi son adımda onaylanır (K-732) | *geri ödeme kaydı* · *gönderim kaydı* · veri doğurmaz — arayüz (`04`) | kendiliğinden geri ödeme; F-4'ün gönderim satırı; müşteri onayının kaydı yok | eşlendi |
| `03 §1.6.2` | Düzeltme — yanlış geçiş kapalı listeden sebep seçilerek yeni bir geçişle düzeltilir; ikisi de işlem izinde; dış sonuç geri alınmaz; B-13 | *geçiş kaydı* · AuditLog · *gönderim kaydı* | düzeltme geçişi — düzelttiği geçişe bağ, `CorrectionReason`, yapan, an; iz satırı; B-13 | eşlendi |
| `03 §1.6.2.1` | Erken ya da yanlış teslim işareti: bir önceki duruma; işaret kalkar, pencereler yeni tarihe kadar işlemez | *geçiş kaydı* · OrderItem | teslim işareti ve `DeliveryDate` boşalır; düzeltme geçişi | eşlendi |
| `03 §1.6.2.2` | Yanlış iptal: iptalden önceki duruma; yalnız ödeme Ödendi'de kalmışsa; kart iadesinin sonucu beklenirken yok (K-940); ayırma yeniden; kapanış konu değil (K-703) | *geçiş kaydı* · StockReservation · *geri ödeme kaydı* · Cancellation | düzeltme geçişi; iptalden önceki durum (geçiş kaydından okunur); geri ödeme kaydının hâli düzeltmeyi kapatır; yeniden ayırma | eşlendi |
| `03 §1.6.2.3` | Yanlış Kargoya verildi ya da Teslim edilemedi işareti: yanlış geçişten önceki duruma; aradaki kayıtlar kalır (K-703) | *geçiş kaydı* | düzeltme geçişi | eşlendi |
| `03 §1.6.2.4` | Para gelmeden konmuş havale işareti: iki eksen birlikte geri; kayıt varsa yok (K-703); Z-8 düzeltme anından, B-3 yeniden (K-685) | *geçiş kaydı* · Order · *gönderim kaydı* | düzeltme geçişleri; ödeme süresinin yeniden başladığı an ve yeni sonu; B-3'ün süre başına gönderimi | eşlendi |
| `03 §1.6.2` metin | Kartta ödeme ekseninin düzeltmesi yok; cayma beyanı düzeltilmez ve geri alınmaz (K-688); teslim tarihi, ulaşma tarihi, adres ve takip bilgisi düzeltmesi geçiş değil müdahale | Withdrawal · AuditLog | caymada düzeltme alanı yok; müdahalelerin iz satırı (`02 §10.4`) | eşlendi |

**§1.7 — kalem kayıtları ve panel işaretleri (26)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `03 §1.7.1` | Kalemin kayıtları — kalem durum taşımaz; "Hatta sayılışı" kaydın kapanış kuralına etkisi | OrderItem | kalemde durum alanı yok (K-179) | eşlendi |
| `03 §1.7.1.1` | Teslim işareti ve tarihi — fizikselde S6, sipariş düzeyinde tek tarih; dijitalde ödeme onayı; hizmette "tamamlandı" | OrderItem · *geçiş kaydı* | kalemin teslim işareti ve anı; `DeliveryDate` (fizikselde sipariş düzeyinde bir kez — `02 §5.8` m1); işareti koyan (sistem ya da yönetici) | eşlendi |
| `03 §1.7.1.2` | İptal kaydı — müşteri ya da yönetici; yöneticide sebepli; iptal edilen adet (K-794); B-7 | Cancellation | kalem, adet, kaydeden, `CancellationReason` (firma iptalinde), an | eşlendi |
| `03 §1.7.1.3` | Çıkarma kaydı — yönetici; iptal edilmiş gibi; B-11 | ItemRemoval | kalem, adet, yönetici, an — `ItemRemoval` (GAP-12) | eşlendi |
| `03 §1.7.1.4` | Gecikme feshi — müşteri ya da yöneticinin başka kanal kaydı (K-706); açık adetlerin tamamı; B-9, müşteride F-3 | LateDeliveryTermination | damga, kalemler ve adetler, kaydeden | eşlendi |
| `03 §1.7.1.5` | Cayma beyanı — müşteri ya da yönetici; cayılan adet ve iade adresi; ardışık beyanlar ayrı (K-790); B-9, müşteride F-3 | Withdrawal | damga, adet, iade adresi, kaydeden | eşlendi |
| `03 §1.7.1.6` | İade teslim alma — ulaşma tarihi ve adet; parça parça ayrı (K-794); tarih düzeltilir (K-712); stoğa ekleme ayrı adım; IBAN silinmişse B-14 | Return · AuditLog | ulaşma tarihi, ulaşan adet, beyan bağı; stoğa eklenen adet ve anı; tarih düzeltmesinin iz satırı | eşlendi |
| `03 §1.7.1.7` | İade reddi — koşullu istisna kaleminde, teslim almada; B-16 | ReturnRejection | kalem, adet, an | eşlendi |
| `03 §1.7.1.8` | "Mal dönmedi" kapanışı — Z-42 geçince, ulaşmamış adetlere (K-794) | NotReturnedClosure | adet, an, beyan bağı | eşlendi |
| `03 §1.7.1.9` | IBAN isteği — sistemin beş tetiği (teslim alma, havale gerçekleşmedi, IBAN'sız kayıt, havale yolunu açma, havale hattında firma iptali ya da çıkarma) ve yöneticinin kendi isteği (K-686, K-690, K-706); B-14 | RefundIbanRequest | kalem, tetikleyen kayıt ya da yönetici, istek anı, karşılandığı an | eşlendi |
| `03 §1.7.1.10` | Ayıp talebi — müşteri, ayıplı adetle (K-793); hattı etkilemez; B-9, F-3 | DefectClaim | kalem, ayıplı adet, `DefectClaimStatus` | eşlendi |
| `03 §1.7.1.11` | İndirme sayacı — sistem her indirmede; yönetici yeniler | OrderItem | indirme sayacı; yenilenme | eşlendi |
| `03 §1.7.2` | Kalemin yolu tipe göre | veri doğurmaz — kod | — (yol kalemin donmuş `ProductType`'ından ve kayıtlarından) | veri doğurmaz |
| `03 §1.7.2` m1 | Fiziksel kalem: iptal ve çıkarma kargoya kadar; Z-11 feshi; kargodayken fesih ya da cayma; teslimden sonra cayma, teslim alma, ret, "mal dönmedi"; kapanıştan sonra ulaşan mal kalemi yeniden açar; ayıp talebi Kargoya verildi'den, iki yıl (Z-18) | OrderItem · Return · NotReturnedClosure · DefectClaim | teslim işaretinin tarihi (Z-18'in başlangıcı); "mal dönmedi"den sonra teslim alma kaydı | eşlendi |
| `03 §1.7.2` m2 | Dijital kalem: ödeme onayında teslim ve indirme; iptal ve cayma yok; ayıp talebi açık | OrderItem | teslim işareti; indirme sayacı | eşlendi |
| `03 §1.7.2` m3 | Hizmet kalemi: iptal, çıkarma, cayma "tamamlandı"ya ya da Z-14'e kadar; ayıp talebi tamamlamadan sonra | OrderItem | tamamlama işareti ve anı | eşlendi |
| `03 §1.7.3` | Panel işaretleri — durum değil; koşul sürdükçe görünür (K-675) | veri doğurmaz — türetilir | — (işaretler ayrı alan değildir, koşulun kaydından türer — K-1015; satırlarda koşulun kaydı) | veri doğurmaz |
| `03 §1.7.3.1` | "e-posta ulaşmadı" — sipariş, ayıp talebi ya da davet satırında; Z-26'nın üç denemesi başarısız olunca; yeniden gönderim başarılı olunca ya da davet geçersizleşince kalkar | *gönderim kaydı* · AdminInvitation · Order · DefectClaim | gönderimin sonucu; işaret gönderim kaydından türer (K-913) — `EmailUndeliveredMark` (GAP-12) | eşlendi |
| `03 §1.7.3.2` | "ödeme sonucu alınamadı" — kart son sorgusu yanıtsız kalınca; sonuç gelince kalkar | Order · *ödeme kaydı* | ödeme Bekliyor ve ödeme süresinin bittiği an — işaret türer (`02 §6.1.2`; K-1015) — `PaymentOutcomeUnknownMark` (GAP-12) | eşlendi |
| `03 §1.7.3.3` | "geri ödeme gerçekleşmedi" — sağlayıcı reddi, sorgunun "gerçekleşmedi" cevabı ya da yöneticinin işareti; iade gerçekleşince, IBAN'a ödeme işlenince ya da yanlış iptal düzeltilince kalkar | *geri ödeme kaydı* · Order · Refund | `RefundFailedMark` — geri ödeme kaydının hâlinden (K-940) | eşlendi |
| `03 §1.7.3.4` | "IBAN bekleniyor" — kalemde; IBAN isteğiyle düşer, IBAN girilince kalkar | RefundIbanRequest | isteğin karşılandığı an (işaret ondan türer) — `IbanPendingMark` (GAP-12) | eşlendi |
| `03 §1.7.3.5` | "iade malı bekleniyor" — teslimden sonraki caymayla; teslim alma, ret ya da kapanışla — kalan adetler için sürer | Withdrawal · Return · ReturnRejection · NotReturnedClosure | beyanın adedi ile teslim alınan, reddedilen, kapatılan adetler (işaret türer) — `ReturnPendingMark` (GAP-12) | eşlendi |
| `03 §1.7.3.6` | "e-posta düzeltildi" — misafir e-postası düzeltilince; kalkmaz, e-posta geçmişiyle imha | OrderEmailHistory | e-posta geçmişinin varlığı (işaret türer) — `EmailCorrectedMark` (GAP-12) | eşlendi |
| `03 §1.7.3.7` | "geri ödeme sonucu bekleniyor" — sağlayıcıya ulaşılamayınca ya da zaman aşımında; sorgunun sonucuyla ya da yöneticinin "gerçekleşmedi" işaretiyle kalkar | *geri ödeme kaydı* · AuditLog | `RefundOutcomePendingMark` — geri ödeme kaydının hâlinden; yöneticinin işaretinin iz satırı (K-940) | eşlendi |
| `03 §1.7.3` metin | Bekleyen işler sayaçları `02 §10.6.1`'de | veri doğurmaz — türetilir | sayaçlar saklanmaz (K-675) | veri doğurmaz |
| `03 §1.7.4` | Kalemin adedi — işlem birimi adet; kayıtlar adedi taşır, aynı tür birden çok kez doğar; açık adet düşülerek okunur; gecikme feshinde seçim yok; dijitalin adedi bir; hizmette tamamlandı açık adetlere | OrderItem · Cancellation · ItemRemoval · LateDeliveryTermination · Withdrawal · Return · ReturnRejection · NotReturnedClosure · DefectClaim | her kaydın adedi ve damgası; ardışık kayıtlar ayrı satır; açık adet türetilir, saklanmaz (K-787'nin park satırı; K-1015) | eşlendi |

**§1.8–§1.9 — yayın ve talep durumları (18)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `03 §1.8.1` | Ürün ve varyant üç değeri ayrı taşır; geçişler yöneticinin, bildirimsiz, işlem izine | Product · Variant · AuditLog | `PublishStatus` (iki düzlemde); değişikliğin iz satırı — geçiş kaydı tutulmaz | eşlendi |
| `03 §1.8.1` `Draft` | Taslak — vitrinde yok; Yayında'ya geçiş yayın kapısına bağlı | Product · Variant | `PublishStatus` = `Draft` | eşlendi |
| `03 §1.8.1` `Published` | Yayında — tükenmişlik ayrı durum değil | Product · Variant | `PublishStatus` = `Published` | eşlendi |
| `03 §1.8.1` `Archived` | Arşiv — kaydı korunan ürün; "Bu ürün artık satılmıyor" | Product · Variant | `PublishStatus` = `Archived` | eşlendi |
| `03 §1.8.2` | Yayın kapısını bozan işlem engellenir — son Yayında varyantın arşivi ya da silinmesi, dosyasız bırakma (K-674); ürünün kalıcı silinmesi durum değil, açık siparişler donmuş kalemleriyle sürer | Product · Variant · DigitalFile · OrderItem | kapı — kısıt; fiziksel silme; kalemin donmuş değerleri ürünün silinmesinden bağımsız | eşlendi |
| `03 §1.8.3` | Vitrinden kalkan kalem sepetten çıkar ve bir kez söylenir; yayın durumu siparişi etkilemez | CartItem | kalemin silinmesi (`02 §3.16.7`'nin satırı); yayın durumu siparişin donmuş kalemine dokunmaz | eşlendi |
| `03 §1.8.4` | Kurumsal içerik iki değer; arşiv, ileri tarihli yayın ve sürüm geçmişi yok | Content · yoktur — kapsam dışı | `PublishStatus` (iki değer); sürüm geçmişi alanı yok | eşlendi |
| `03 §1.8.4` `Draft` | Taslak — sitede yok; zorunlu alanlar doluysa Yayında | Content | `PublishStatus` = `Draft` | eşlendi |
| `03 §1.8.4` `Published` | Yayında — düzenleme kaydedildiği anda yayında | Content | `PublishStatus` = `Published` | eşlendi |
| `03 §1.8.4` metin | Duyuru yalnız tarih aralığında (Z-23); Hakkımızda silinmez; ötekilerin silinmesi geri alınamaz | Announcement · AboutPage · Content | başlangıç ve bitiş (isteğe bağlı); Hakkımızda tekil, silinmez; fiziksel silme | eşlendi |
| `03 §1.9.1` | Ayıp talebi — kalem başına tek kayıt (K-676); Çözüldü'yken müşteri ve yönetici yeniden açar | DefectClaim | kalem başına tekil — kısıt; yeniden açılma | eşlendi |
| `03 §1.9.1` `Open` | Açık — doğuşta B-9 ve F-3 | DefectClaim | `DefectClaimStatus` = `Open` (başlangıç değeri) | eşlendi |
| `03 §1.9.1` `Resolved` | Çözüldü — Z-18 içinde yeniden açılır; müşterinin açmasında B-9 ve F-3 (K-681) | DefectClaim · *gönderim kaydı* | `DefectClaimStatus` = `Resolved`; yeniden açanın türü (müşteri bildirim üretir, yönetici üretmez) | eşlendi |
| `03 §1.9.1` metin | Seçimlik hakların yürütümü sistem dışı; para gerektiren çözüm geri ödeme geçişleriyle | veri doğurmaz — ürünün dışında · Refund | seçimlik hak alanı yok; geri ödemenin türü ayıp çözümü | eşlendi |
| `03 §1.9.2` | İletişim talebi — ayrı durum makinesi ve süre sayacı yok | ContactRequest | `ContactRequestStatus` | eşlendi |
| `03 §1.9.2` `Open` | Açık — doğuşta F-2 | ContactRequest | `ContactRequestStatus` = `Open` (başlangıç değeri) | eşlendi |
| `03 §1.9.2` `Closed` | Kapatıldı — yönetici yeniden açar | ContactRequest | `ContactRequestStatus` = `Closed` | eşlendi |
| `03 §1.9.2` metin | Saklama son kapatılıştan, hiç kapatılmamışta açılıştan (Z-33) | ContactRequest | son kapatıldığı an, oluşma anı | eşlendi |

**§1.10 — durum makinesi olmayan varlıklar (9)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `03 §1.10` metin | Durumu olmayan varlıkların geçerliliği zamana ya da kullanıma bağlı | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `03 §1.10.1` | İndirim — tarih aralığında (Z-20); sistem başlatır ve bitirir | Discount | başlangıç ve bitiş anı | eşlendi |
| `03 §1.10.2` | Kupon — tarih aralığı (Z-22) ve kullanım hakkı; kullanım adedi kullanılmış ve ayrılmış hakların altına inmez (K-654) | Coupon · StockReservation | başlangıç, bitiş, kullanım adedi; kullanılan ve ayrılan hak sayısı | eşlendi |
| `03 §1.10.3` | Kupon hakkı ve stok ayırma — ayrılır, kesin düşer ya da serbest kalır (§1.4.4) | StockReservation | ayırmanın hâli | eşlendi |
| `03 §1.10.4` | Duyurunun tarih aralığı — yayın durumuna ek koşul (Z-23) | Announcement | başlangıç ve bitiş (isteğe bağlı) | eşlendi |
| `03 §1.10.5` | Hesap — doğrulanmamış kayıt bekleme aşaması; Z-1 dolunca silinir; e-posta değişikliği beklemesi ve Z-45 geri alma; Google ile ilk girişte bekleyen kayıt silinir (K-696); silme geri alma bağlantısı açıkken yok (K-698); yöneticinin kaldırılması; kurulum tarafının yeni yönetici açması (K-699); limit durum değiştirmez | Customer · Admin · PendingEmailChange · *bağlantı kaydı* · SocialLogin · veri doğurmaz — kurulum | doğrulanma işareti ve anı; bekleyen yeni adres; geri alma bağlantısı; hesabın Google bağı; hesapta kilit alanı yok | eşlendi |
| `03 §1.10.6` | Oturum — Z-4 ya da Z-5; şifre sıfırlama, silme, geri alma ve kaldırma bütün oturumları, şifre değiştirme ötekileri kapatır (K-695) | Session | oturum türü (kısa, uzun); son işlem anı; hesap bağı (toplu silme) | eşlendi |
| `03 §1.10.7` | Yönetici daveti — Z-3, tek kullanımlık; kullanılınca hesap doğar; geçersizleşme: süre, geri çekme, yeni davet, gönderenin kaldırılması; gönderim ve geri çekme işlem izine; F-6 (K-692) | AdminInvitation · AuditLog · *gönderim kaydı* | davetin hâli, adresi, gönderen yönetici, gönderim anı, kullanıldığı ya da geri çekildiği an; iz satırları | eşlendi |
| `03 §1.10.8` | Mağazanın satış açıklığı — dört koşulun hepsi; geçici kapatma yeni siparişi durdurur (K-664) | *firma ayarları* · veri doğurmaz — türetilir | geçici kapatma ayarı; açıklık koşullardan türer (`02 §3.1.5`) | eşlendi |

**§1.11 — durum × rol × işlem (42)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `03 §1.11` metin | Ekran varyantları tablodan türer; işlem aralığının dışında sunulmaz; müşteride iptal, cayma ve fesih açık kalemde, adet seçimiyle; fizikselde cayma ve ayıp yalnız kargoya verilmiş kalemde | veri doğurmaz — arayüz (`04`) · veri doğurmaz — türetilir | — (aralıklar durumlardan ve kayıtlardan türer) | veri doğurmaz |
| `03 §1.11.1` | Müşteri siparişi bütünüyle iptal eder — Alındı + Bekliyor | Cancellation · *geçiş kaydı* | S3 + Ö2; kaydeden müşteri | eşlendi |
| `03 §1.11.2` | Fiziksel kalemi iptal — ödeme onayından Kargoya verildi'ye kadar; Z-17 | Cancellation · Refund | kalem, adet, an; geri ödeme | eşlendi |
| `03 §1.11.3` | Hizmet kalemini iptal — tamamlanana kadar | Cancellation | kalem, adet | eşlendi |
| `03 §1.11.4` | Dijital kalemi iptal — yok | veri doğurmaz — kod | — | veri doğurmaz |
| `03 §1.11.5` | Fiziksel kalemden caymak — Kargoya verildi'den teslim tarihinden on dört güne (Z-13); mutlak istisnada yok, koşullu istisnada koşulla | Withdrawal · OrderItem · Product | beyan; `DeliveryDate`; kalemin donmuş cayma istisnası (`WithdrawalExemptionReason`) | eşlendi |
| `03 §1.11.6` | Hizmet kaleminden caymak — ödemeden tamamlanana ya da Z-14'e kadar (K-673, K-792) | Withdrawal · OrderItem | beyan, cayılan adet; tamamlama işareti | eşlendi |
| `03 §1.11.7` | Dijital kalemden caymak — yok; hak üçüncü kutuyla düşer | CheckboxAcceptance | dijital kutusunun kaydı | eşlendi |
| `03 §1.11.8` | Gecikme nedeniyle fesih — Z-11 geçmiş, teslim tarihi girilmemiş açık fiziksel kalem varken (K-717) | LateDeliveryTermination · *geçiş kaydı* | fesih kaydı; kapanış geçişi | eşlendi |
| `03 §1.11.9` | Ayıp talebi açmak — tipe göre başlangıç; talep yokken (K-676); Z-18 | DefectClaim · OrderItem | talep; teslim işaretinin tarihi | eşlendi |
| `03 §1.11.10` | Ayıp talebini yeniden açmak — Çözüldü'yken, Z-18 içinde; B-9, F-3 (K-681) | DefectClaim | durum ve yeniden açan | eşlendi |
| `03 §1.11.11` | Geri ödeme IBAN'ını girmek — havalede iptal, fesih, cayma sırasında; IBAN isteğinde; havale yolu açılınca | RefundIbanRequest · Withdrawal · LateDeliveryTermination · Cancellation | geri ödeme IBAN'ı (yeri VM4-08), girildiği an; isteğin karşılandığı an | eşlendi |
| `03 §1.11.12` | Girilmiş IBAN'ı düzeltmek — geri ödeme işlenene kadar | RefundIbanRequest | geri ödeme IBAN'ı — düzeltilebilir (girişin zaman damgası yok — K-710) | eşlendi |
| `03 §1.11.13` | Havale bilgisini görmek — Alındı + Bekliyor; donmuş IBAN (K-663) | Order | siparişe donan IBAN | eşlendi |
| `03 §1.11.14` | Dijital dosyayı indirmek — ödemeden sonra, hak (P-8) kaldıkça | OrderItem · DigitalFile | indirme hakkı ve sayacı; varyantın o anki dosyası | eşlendi |
| `03 §1.11.15` | Havale "ödendi" işareti — Alındı + Bekliyor; geri alınamaz; B-4 | *geçiş kaydı* · *ödeme kaydı* | Ö1; aktör yönetici | eşlendi |
| `03 §1.11.16` | Kargoya vermek — Hazırlanıyor, açık fiziksel kalem varken; B-5 | *geçiş kaydı* · Order | S5; kargo bilgisi | eşlendi |
| `03 §1.11.17` | Teslim işareti ve tarihi — Kargoya verildi | *geçiş kaydı* · OrderItem | S6; `DeliveryDate` | eşlendi |
| `03 §1.11.18` | "Teslim edilemedi" — Kargoya verildi; B-6 | *geçiş kaydı* | S7 | eşlendi |
| `03 §1.11.19` | Yeniden göndermek — Teslim edilemedi; geri alınamaz (K-677); B-5 | *geçiş kaydı* · Order | S8; kargo bilgisi | eşlendi |
| `03 §1.11.20` | Hizmet kalemini "tamamlandı" — ödeme onaylıyken (K-671), kalem açıkken | OrderItem | tamamlama işareti — teslim işareti, an | eşlendi |
| `03 §1.11.21` | Firma iptali — ödeme beklenirken sipariş, sonra kalem ve adet (K-794); Teslim edilemedi'de geri dönen kalem; S9 ya da S11; havale hattında IBAN isteği ve B-14 (K-690) | Cancellation · *geçiş kaydı* · RefundIbanRequest | kalem, adet, `CancellationReason`; S9 ya da S11; IBAN isteği | eşlendi |
| `03 §1.11.22` | Kalem çıkarmak ya da azaltmak — ödemeden sonra, tipine göre sınır; B-11; havalede IBAN isteği (K-690) | ItemRemoval · RefundIbanRequest | kalem, adet, yönetici, an — `ItemRemoval` (GAP-12); IBAN isteği | eşlendi |
| `03 §1.11.23` | Adresi düzeltmek — Kargoya verildi'ye kadar; fizikselsizde fatura adresi Teslim edildi'ye kadar; B-10 | Order · OrderAddressHistory · AuditLog | `ShippingAddress`, `BillingAddress` yerinde düzeltilir; iz satırı adres değerini taşımaz; eski ve yeni değer `OrderAddressHistory`'de (GAP-13) | eşlendi |
| `03 §1.11.24` | Kargo şirketi ve takip numarasını düzeltmek — kargoya verildikten sonra; B-12 | Order · AuditLog | kargo şirketi, takip numarası; iz satırı | eşlendi |
| `03 §1.11.25` | Teslim tarihini ya da ulaşma tarihini düzeltmek (K-712); pencereler ve Z-16 yeni tarihten | OrderItem · Return · AuditLog | `DeliveryDate`; ulaşma tarihi; iz satırı | eşlendi |
| `03 §1.11.26` | Durum geçişini düzeltmek — §1.6.2; B-13 | *geçiş kaydı* | düzeltme geçişi, `CorrectionReason` | eşlendi |
| `03 §1.11.27` | İç not yazmak — her durumda | Order | siparişin iç notu — tek serbest metin; iz satırı metinsiz (`02 §10.4.7`) — `InternalNote` (GAP-12) | eşlendi |
| `03 §1.11.28` | İade malını teslim almak — ulaşan adetle; "mal dönmedi"den sonra da; ayıpta sözleşmeden dönmede (K-808); IBAN silinmişse B-14 | Return · RefundIbanRequest · DefectClaim | ulaşma tarihi, ulaşan adet, beyan ya da ayıp talebi bağı; IBAN isteği | eşlendi |
| `03 §1.11.29` | İade malını reddetmek — koşullu istisnada; geri alınamaz; B-16 | ReturnRejection | kalem, adet | eşlendi |
| `03 §1.11.30` | "Mal dönmedi" kapanışı — Z-42 geçmiş, ulaşmamış adetlere | NotReturnedClosure | adet, an | eşlendi |
| `03 §1.11.31` | Geri ödemeyi işlemek — havalede IBAN varken; caymada iki hatta; ayıp çözümünde; tutar bazlı; geri alınamaz; B-8 | Refund · *geçiş kaydı* | tür, tutar, yol, kalem bağı, işleyen, an; Ö3–Ö5 | eşlendi |
| `03 §1.11.32` | Kart iadesini yeniden denemek ya da havale yolunu açmak; sonucu beklenen iadeyi "gerçekleşmedi" işaretlemek (K-940) | *geri ödeme kaydı* · RefundIbanRequest · AuditLog | geri ödeme kaydının hâli ve yeni deneme; IBAN isteği; "gerçekleşmedi" işaretinin iz satırı | eşlendi |
| `03 §1.11.33` | "Havale gerçekleşmedi" bildirmek — geri ödeme işlenmeden önce; IBAN silinir, B-14 | RefundIbanRequest · Refund | IBAN'ın silinmesi; yeni IBAN isteği | eşlendi |
| `03 §1.11.34` | Başka kanaldan gelen caymayı kaydetmek — fizikselde önce "müşteriyle anlaşıldı" iptali, hizmette bütünüyle iptal (K-673); pencere dışında onayla (K-709); bitimden bir yıl sonrasına tarihli kayıt reddedilir; geri alınamaz (K-715); IBAN'sızsa B-14 | Withdrawal · Cancellation · RefundIbanRequest | damga (bildirimin ulaştığı tarih), adet, kaydeden yönetici; pencere dışı onayın kaydı (uzayan pencere — `02 §10.4.10`) | eşlendi |
| `03 §1.11.35` | Misafir siparişinin e-postasını düzeltmek — kişisel veriler imha edilene kadar; B-1 yeni, B-15 eski adrese | OrderEmailHistory · Order · *gönderim kaydı* | eski ve yeni adres, yönetici, an; `OrderAccessKey` yenilenir | eşlendi |
| `03 §1.11.36` | E-postayı yeniden göndermek — "e-posta ulaşmadı" işaretliyken müşteri bildirimi; davette yok (K-759) | *gönderim kaydı* | yeniden gönderim ayrı satır, ilk alıcıya (Z-43) | eşlendi |
| `03 §1.11.37` | Ayıp talebini "çözüldü" ya da yeniden açmak | DefectClaim | `DefectClaimStatus` | eşlendi |
| `03 §1.11.38` | İndirme hakkını yenilemek — sayaç sıfırlanır (K-683) | OrderItem · AuditLog | indirme sayacı | eşlendi |
| `03 §1.11.39` | Geri ödeme için IBAN istemek — havalede IBAN'sız geri ödeme (K-686) | RefundIbanRequest | tetikleyen yönetici, istek anı | eşlendi |
| `03 §1.11.40` | Siparişi kapatmak (S9 kapanışı) — açık kalem yok, teslim yok; sebep, iptal kaydı, geri ödeme ve B-7 yok (K-717) | *geçiş kaydı* | S9; aktör yönetici; kapanış — sebep yok | eşlendi |
| `03 §1.11.41` | Başka kanaldan gelen gecikme feshini kaydetmek — 1.11.8'in aralığında; geri alınamaz; IBAN'sızsa B-14 (K-706, K-715) | LateDeliveryTermination · RefundIbanRequest | damga, kalemler ve adetler, kaydeden yönetici | eşlendi |

**`03 §4` — bölüm girişi ve §4.1 süre envanterinden zaman aşımı akışları (51)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `03 §4` metin | Süreler (Z-1…Z-47) ve kendiliğinden işleyen anlar burada akışa bağlanır; değer değil kimlik; sayım kuralları `02 §4.1`'de | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `03 §4` m1 | Tek uyarı havale hatırlatmasıdır (B-3, Z-9); kalan süre göstergesi uyarı değil | *gönderim kaydı* · veri doğurmaz — türetilir | B-3'ün gönderim satırı; kalan süre türetilir | eşlendi |
| `03 §4` m2 | Dondurma yok; tek istisna havalenin kendiliğinden iptali (K-666, K-667); yasal süreler dondurulmaz | Order · *nabız ve kesinti kaydı* | ertelenen son an (K-898); kesinti kaydı (K-895) | eşlendi |
| `03 §4` m3 | "Sona erişte" sistemin kendiliğinden işlemi; "kendiliğinden işlem yok" firmanın ya da müşterinin yükümlülüğü | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `03 §4.1.1` | Z-1 — ilk bağlantının gönderiminden; yeniden istenen ömrü uzatmaz; dolunca bekleyen kayıt silinir, bekleyen değişiklik düşer (K-659) | *bağlantı kaydı* · Customer · PendingEmailChange · *imha kaydı* | ilk bağlantının gönderildiği an, ömrün sonu; bekleyen kaydın silinmesi imha kaydına (K-708) | eşlendi |
| `03 §4.1.2` | Z-2 — talep anından; tek kullanımlık; L-2 içinde yeni talep | *bağlantı kaydı* · *deneme kaydı* | talep anı, ömrün sonu, kullanıldığı an | eşlendi |
| `03 §4.1.3` | Z-3 — davetin gönderiminden; geri çekme, yeni davet, gönderenin kaldırılmasıyla da geçersizleşir | AdminInvitation · *bağlantı kaydı* | gönderim anı, ömrün sonu, geri çekildiği an, gönderen yönetici | eşlendi |
| `03 §4.1.4` | Z-4 kısa oturum — son işlemden; sepet hesapta | Session · Cart | son işlem anı; sepetin hesap bağı | eşlendi |
| `03 §4.1.5` | Z-5 uzun oturum — "beni hatırla" | Session | oturum türü; son işlem anı | eşlendi |
| `03 §4.1.6` | Z-6 sepet — dolmaz | Cart | — (süre alanı yok) | eşlendi |
| `03 §4.1.7` | Z-7 kart ödeme süresi — onaydan; son sorgu; alınmamışsa iptal ve serbest; yanıtsızsa "ödeme sonucu alınamadı" ve yinelenen sorgu | Order · StockReservation · *ödeme kaydı* | ödeme süresinin bittiği an; ödeme sonucu alınamadı işareti; ayırmanın serbest kalması | eşlendi |
| `03 §4.1.8` | Z-8 havale ödeme süresi — onaydan ya da düzeltme anından; dolunca iptal; kesintide ertelenir (4.2.12) | Order · StockReservation · *firma ayarları* | ödeme süresinin başlangıcı ve sonu (düzeltmede yeniden); ertelenen son an; havale ödeme süresi ayarı | eşlendi |
| `03 §4.1.9` | Z-9 havale hatırlatması — son iş gününün başında bir kez; düzeltmeyle bir kez daha; kesintide kaçırılırsa süre işliyorsa dönüşte (K-685) | *gönderim kaydı* · Order | B-3'ün gönderim satırı — ödeme süresi başına bir kez (gönderim kaydından okunur) | eşlendi |
| `03 §4.1.10` | Z-10 kargoya verme süresi — ödeme onayından; kendiliğinden işlem yok; iptalde kanuni faiz uyarısı; söze uyum satış özetinde | Order · *geçiş kaydı* · veri doğurmaz — türetilir | donmuş süre, ödeme onayı anı; S5'in anı; uyum ve sayaç türetilir | eşlendi |
| `03 §4.1.11` | Z-11 yasal teslim üst sınırı — onaydan; teslim tarihi girilmemiş fiziksel kalem varken fesih düğmesi; geri ödeme on dört gün (2.7.8) | Order · OrderItem · LateDeliveryTermination | oluşma anı; `DeliveryDate`; fesih bildiriminin damgası | eşlendi |
| `03 §4.1.12` | Z-12 teslim tarihi — yöneticinin girdiği; girilmezse Z-13 ve Z-18 başlamaz | OrderItem | `DeliveryDate` | eşlendi |
| `03 §4.1.13` | Z-39 ifa süresi — ödeme onayından; "tamamlanmayı bekleyen hizmet" sayacı; iptalde kanuni faiz uyarısı | OrderItem · veri doğurmaz — türetilir | donmuş ifa süresi; tamamlama işareti; sayaç türetilir | eşlendi |
| `03 §4.1.14` | Z-13 fiziksel cayma penceresi — teslim tarihinden, düzeltilmişse yeni tarihten; önceki beyan geçerli; uzama §5.2.3 | OrderItem · Withdrawal | `DeliveryDate`; beyanın damgası | eşlendi |
| `03 §4.1.15` | Z-14 hizmet cayma penceresi — sipariş tarihinden ya da fizikselli siparişte teslim tarihinden; tamamlanınca düşer | Order · OrderItem | oluşma anı; `DeliveryDate`; tamamlama işareti; onay anındaki kalemlerin `ProductType`'ı | eşlendi |
| `03 §4.1.16` | Z-15 dijital cayma hakkı — üçüncü kutuyla düşer | CheckboxAcceptance | dijital kutusunun kaydı | eşlendi |
| `03 §4.1.17` | Z-16 cayma geri ödemesi — beyandan ya da ulaşma tarihinden; kendiliğinden işlem yok; IBAN beklenirken de işler | Withdrawal · Return · Refund | beyanın damgası; ulaşma tarihi; geri ödemenin anı | eşlendi |
| `03 §4.1.18` | Z-42 iade malını gönderme — beyandan; kendiliğinden işlem yok; "mal dönmedi" açılır; havalede Z-38 başlar | Withdrawal | beyanın damgası (süre türetilir) | eşlendi |
| `03 §4.1.19` | Z-17 iptalde geri ödeme — iptal anından; kartta sistem iadeyi başlatmış, havalede firma | Cancellation · Refund · *geri ödeme kaydı* | iptal anı; kart iadesinin isteği | eşlendi |
| `03 §4.1.20` | Z-47 ifanın imkânsızlaşması — sayaç yok; B-7 iptal anında sebebiyle | Cancellation · *gönderim kaydı* | `CancellationReason`; B-7'nin gönderim satırı | eşlendi |
| `03 §4.1.21` | Z-18 ayıp talebi — teslim işaretinin tarihinden; dolunca kanal kapanır; hak iletişim formundan | OrderItem · DefectClaim | teslim işaretinin tarihi (süre türetilir) | eşlendi |
| `03 §4.1.22` | Z-19 indirme hakkı — süre yok, adet (P-8); yönetici yeniler | OrderItem | indirme hakkı ve sayacı | eşlendi |
| `03 §4.1.23` | Z-20 indirim aralığı — sistem bitirir; onay anında bitmesi özet farkı | Discount | başlangıç ve bitiş anı | eşlendi |
| `03 §4.1.24` | Z-21 referans fiyatın penceresi — indirimin başladığı andan; işlem başlatmaz | PriceHistory · Discount | fiyat geçmişi; indirimin başlangıcı | eşlendi |
| `03 §4.1.25` | Z-22 kupon aralığı — yeni siparişte kabul edilmez; ayrılmış haklar kalır | Coupon · StockReservation | başlangıç ve bitiş; ayrılan haklar | eşlendi |
| `03 §4.1.26` | Z-23 duyuru aralığı — isteğe bağlı; dolunca görünmez | Announcement | başlangıç ve bitiş (isteğe bağlı) | eşlendi |
| `03 §4.1.27` | Z-24 ileri tarihli yayın — yok | yoktur — kapsam dışı | — | veri doğurmaz |
| `03 §4.1.28` | Z-25 KVKK cevabı — başvurunun ulaştığı an talebin kaydında; sayaç yok | ContactRequest · veri doğurmaz — ürünün dışında | oluşma anı; `ContactTopic` | eşlendi |
| `03 §4.1.29` | Z-26 e-postanın yeniden denenmesi — üç deneme başarısızsa "e-posta ulaşmadı" ya da firma uyarısı; hesap e-postasında işaret yok; yeniden gönderimle kalkar | *gönderim kaydı* · *kanal hâli kaydı* | deneme sayısı, sonraki deneme anı, sonuç; firma bildirimlerinin ulaşma hâli; işaret türer (K-913) | eşlendi |
| `03 §4.1.30` | Z-27 deneme limitlerinin penceresi — kayan; geçici engel; hesap kilitlenmez | *deneme kaydı* | an (pencere geçince silinir) | eşlendi |
| `03 §4.1.31` | Z-28 sipariş, ödeme, fatura verisi, sözleşme ve onay kayıtları — yılın sonundan; kişisel veri imha, hesap bağı kesilir; kalan ticari kayıt | Order · OrderItem · *ödeme kaydı* · CheckboxAcceptance · LegalTextVersion · *imha kaydı* | oluşma yılı (başlangıç); kişisel alanların boşaltılması; hesap bağının kesilmesi | eşlendi |
| `03 §4.1.32` | Z-41 ödemesi hiç alınmamış sipariş — Başarısız olduğu andan; kişisel veri imha, kayıt on yıl (K-951) | Order · *imha kaydı* | Başarısız olduğu an (Ö2'nin anı); kişisel alanların boşaltılması | eşlendi |
| `03 §4.1.33` | Z-29 ayıp talebi kaydı — siparişle imha | DefectClaim | siparişe bağ | eşlendi |
| `03 §4.1.34` | Z-30 işlem izi — yılın sonundan; değiştirilemez, silinemez | AuditLog | yazıldığı an; yalnız eklenir (K-877) | eşlendi |
| `03 §4.1.35` | Z-31 hesap verisi — silinince hemen | Customer · Session · AddressBook · *imha kaydı* | anında fiziksel silme; silme imha kaydına | eşlendi |
| `03 §4.1.36` | Z-32 sepet — hesapla silinir | Cart | hesap bağı — hesapla silinir | eşlendi |
| `03 §4.1.37` | Z-33 iletişim talebi — son kapatılıştan, açıkta açılıştan; "Sipariş hakkında" on yıl (P-40; K-707, K-951) | ContactRequest | son kapatıldığı an, oluşma anı, `ContactTopic` | eşlendi |
| `03 §4.1.38` | Z-34 hesap, yönetim ve firma bildirimlerinin gönderim kaydı — gönderim anından | *gönderim kaydı* | gönderim anı; tür | eşlendi |
| `03 §4.1.39` | Z-43 B-1…B-16'nın gönderim kaydı — on yıl; ödemesizde kişisel veri üç yılda (K-951); bilgilendirmenin kanıtı | *gönderim kaydı* · *ödeme kaydı* | sipariş bağı, gönderim anı, alıcı ve olayın değerleri (boşaltılabilir); kol ödeme kaydından | eşlendi |
| `03 §4.1.40` | Z-35 havale geri ödeme IBAN'ı — tamamlanınca silinir, silme imha kaydına (K-708); erken silme hâlleri; mal sonra ulaşırsa yeniden istenir | RefundIbanRequest · Refund · *imha kaydı* | geri ödeme IBAN'ı, girildiği an, silinme anı; silmenin imha kaydı | eşlendi |
| `03 §4.1.41` | Z-36 periyodik imha — kendiliğinden çalışan iş; panelde düğme yok | *imha kaydı* · *nabız ve kesinti kaydı* | çalışmanın imha kaydı; gecelik işin çalışma anları | eşlendi |
| `03 §4.1.42` | Z-40 giriş kaydı ve sistem kayıtları — oluştuğu andan; periyodik imhayla, dosyaları da (K-687, K-916) | *giriş kaydı* · veri doğurmaz — mimari (`05`) | giriş kaydı: an, IP, hesap, sonuç; sistem kayıtları dosyada | eşlendi |
| `03 §4.1.43` | Z-44 imha kaydı — kişisel veri içermez | *imha kaydı* | tarih, kayıt türü, sayı | eşlendi |
| `03 §4.1.44` | Z-45 geri alma bağlantısı — değişikliğin geçerli olduğu andan; dolunca yeniden değiştirme engeli kalkar | *bağlantı kaydı* | değişikliğin geçerli olduğu an, ömrün sonu, kullanıldığı an | eşlendi |
| `03 §4.1.45` | Z-46 tanınan tarayıcı işareti — son başarılı girişten ya da sıfırlamayla şifre yenilemeden; süre dolmadan düşme hâlleri (K-695) | RecognizedBrowserMark | hesap, tarayıcı, son başarılı giriş ya da sıfırlama anı; toplu geçersizleştirme | eşlendi |
| `03 §4.1.46` | Z-37 veri ihlalinde Kurul'a bildirim — sayaç yok | veri doğurmaz — ürünün dışında | — | veri doğurmaz |
| `03 §4.1.47` | Z-38 malı dönmeyen caymada IBAN'ın silinmesi — Z-42'nin dolduğu günden; kalem kapanmaz; aynı IBAN'ı bekleyen geri ödeme varsa kalır | RefundIbanRequest · Withdrawal · *imha kaydı* | IBAN'ın silineceği gün (K-895), silme anı; IBAN'ı kullanan geri ödemeler | eşlendi |

**§4.2 — kendiliğinden işleyen anlar (13)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `03 §4.2` metin | Süresi olmayan ama zamanı kural olan anlar ve kesintideki davranış (`02 §4.3`) | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `03 §4.2.1` | Sipariş onayı — numara; kalemler, adresler, firma kimliği, metin sürümleri, havalede IBAN ve kargoya verme sözü donar; ayırma; aynı sepetin önceki siparişi iptal; B-1, B-2, F-1 (K-663) | Order · OrderItem · StockReservation · LegalTextVersion · Cancellation · *gönderim kaydı* | `OrderNumber`; donan değerler (`02 §3.17.2`, §3.23); donan sürümler; donan IBAN; donan Z-10; sepet bağı; önceki siparişin iptal türü | eşlendi |
| `03 §4.2.2` | Ödeme onayı — kesin düşer, kupon kullanılmış, kalemler sepetten çıkar, Z-10 ve Z-39 başlar, dijital teslim; B-4 | Order · StockReservation · Coupon · CartItem · OrderItem | ödeme onayı anı; ayırmanın hâli; teslim işareti | eşlendi |
| `03 §4.2.3` | Kendiliğinden iptal — Z-7, Z-8, kart başarısız, aynı sepet; İptal edildi + Başarısız; son sorgu; B-7 | Cancellation · *geçiş kaydı* · StockReservation | iptalin türü (kendiliğinden); S3 + Ö2, aktör sistem; ayırmanın serbest kalması | eşlendi |
| `03 §4.2.4` | İptal, çıkarma ve fesih — işleme giren adedin stoğu ve kontenjanı döner; kupon tamamında; kapanış kuralı (K-672, K-791) | Cancellation · ItemRemoval · LateDeliveryTermination · Variant · Coupon | işleme giren adet (dönüşün kaynağı) | eşlendi |
| `03 §4.2.5` | İade — fiziksel stok yönetici ekleyince; hizmet kontenjanı cayılan adet kadar; kupon tamamında (K-791) | Return · Withdrawal · Variant · Coupon | teslim alınan ve eklenen adet; cayılan adet | eşlendi |
| `03 §4.2.6` | Kısmi iptal ve iade — kupon ve kargo yeniden hesaplanmaz; iki hâlde kargo ücreti son geri ödemeye eklenir (K-789) | Refund · Order | geri ödemenin kargo ücreti parçası; donmuş kargo ücreti | eşlendi |
| `03 §4.2.7` | Kart hattında kendiliğinden geri ödeme — onay penceresi yok; gerçekleşince Ö3–Ö5; başarısızsa işaret, yeniden deneme yok; sonuç bilinmezse sorgu (K-683, K-684, K-940); B-8, F-4 | *geri ödeme kaydı* · Refund · *ödeme kaydı* · *geçiş kaydı* · *gönderim kaydı* | geri ödeme kaydının hâlleri; geç gelen ödemenin geri ödemesi; F-4 | eşlendi |
| `03 §4.2.8` | Malı dönmeyen caymada IBAN'ın silinmesi — kapatmayla ya da Z-38'de; kalemi kapatmaz | RefundIbanRequest · NotReturnedClosure · *imha kaydı* | IBAN'ın silinme anı | eşlendi |
| `03 §4.2.9` | Periyodik imha — her çalışmanın imha kaydı; hesap, bekleyen kayıt, IBAN ve bekleyen e-posta değişikliğinin silinmesi de kayda (K-708, K-984) | *imha kaydı* | tarih, kayıt türü, sayı — çalışma ve anında silmeler | eşlendi |
| `03 §4.2.10` | E-postanın yeniden denenmesi — artan aralıkla üç kez; art arda başarısızlıkta kanal uyarısı | *gönderim kaydı* · *kanal hâli kaydı* | deneme sayısı, sonraki deneme anı; kanal hâli | eşlendi |
| `03 §4.2.11` | Sitenin kesintisi — süreler işler; kaçırılanlar dönüşte sırayla; son sorgu ödemeyi bulur; yasal süreler dondurulmaz (K-666, K-667) | *iş kuyruğu* · *nabız ve kesinti kaydı* · Order | zamanlanmış işlerin alanları; kesinti kaydı (K-895); ertelenen son an | eşlendi |
| `03 §4.2.12` | Havale süresinin kesintide dolması — iptal ilk iş gününün sonuna ertelenir; sipariş sayfası yeni son günü gösterir; tespit `05`'in (K-667) | Order · *nabız ve kesinti kaydı* | ertelenen son an (K-898); kesintinin kaydı | eşlendi |

#### 1.1.10 Kullanıcı Akışları — öteki bölümler (50)

Alt bölüm düzeyinde (`03 §n.m` — `###` başlığı; K-997): bir satır alt bölümün bütün satırlarını ve alt başlıklarını (`####`) kapsar; bölümün ilk alt bölümden önceki metni ayrı satırla girer (`03 §n` metin — K-1002). `03 §2`, §3, §5, §6, §8, §9 ve §10'un 505 satırı ve `03 §7`'nin 131 satırı tek tek, satırın tamamıyla okundu. Ürün Gereksinimleri'nde karşılığı bulunmayan bir veri (kayıt, işaret, tarih, sayaç, bağ) taşıyan satır kimliğiyle ayrıca girecekti; **böyle satır çıkmadı:** bu bölümlerin her satırı "Kaynak" hücresinde `02`'nin kuralını gösterir — `03 §7.1`'in ve §7.3'ün kaynak sütunu olmayan satırları `02 §9`'un B, F ve hesap e-postası satırlarını adıyla taşır — ve satırların veriye dokunan ayrıntısı `02`'nin gösterilen kuralında yazılıdır (§1.1.1, okuma derinliği; betik `opt03`). Satırın "Alan" hücresi alt bölümün veri doğurmayan satırlarını da özetler. **`03 §7` `02 §9`'un B ve F satırlarıyla toplu eşlenir** (K-997): §7.1'in B-1…B-16 ve F-1…F-6 satırları §1.1.8'in `02 §9.2` ve §9.3 satırlarına, hesap ve yönetim e-postaları `02 §9.4` metin satırına, §7.2'nin satırları `02 §9.2` metin ve §9.5'in maddelerine düşer; her e-posta *gönderim kaydı*nın bir satırıdır (K-892).

**`03 §2` — müşteri tarafının ana akışları (11)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `03 §2` metin | Müşteri tarafının akışları (akış 1, 2, 3, 7); biçim, aktör ve dalların yeri | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `03 §2.1` | Vitrinde gezinme: ana sayfa düzeni ve en yeni ürünler, alfabetik kategori menüsü, ad araması, iki süzgeç, ürün sayfasının gösterdikleri, "Tükendi", ayrılmış adetle satın alınabilirlik, satış kapalıyken vitrin, arşiv ve taslak adresi (11 satır) | *firma ayarları* · Product · Variant · Category · *ürün–kategori bağı* · Discount · PriceHistory · StockReservation · Announcement · veri doğurmaz — arayüz (`04`) · veri doğurmaz — türetilir | `HomepageLayout`; ürünün oluşma ya da ilk yayın anı (en yeni sıralaması); `PrimaryCategory`; fiyatın başlangıç tarihi; `ReferencePrice` ve indirimin tarihleri; ayrılan adet (satın alınabilirlik türetilir); `PublishStatus` — satırların kuralı `02 §3.2`–§3.9, §3.28 (§1.1.5) | eşlendi |
| `03 §2.2` | Kurumsal içerik ve iletişim formu: Hakkımızda bloğu, menü ve liste sırası, içerik sayfaları, ilgili ürünler, İletişim sayfası, üst ve alt bölüm, formun kapısı, beş konu tipi, tuzak alan, L-4, F-2, talep yalnız panelde (10 satır) | Content · AboutPage · Offering · PortfolioItem · FaqItem · Branch · CustomPage · *içerik–ürün bağı* · Company · SocialLink · ContactRequest · *deneme kaydı* · *gönderim kaydı* | elle sıra, "ana sayfada göster" ve "menüde göster" işaretleri; firma kimliğinin iletişim alanları; talebin adı, e-postası, telefonu, `ContactTopic`, mesajı, oluşma anı; F-2 — `02 §3.27`, §3.32 (§1.1.5) | eşlendi |
| `03 §2.3` | Sepet: ekleme denetimi (stok, P-9, dijitalde tek), üye ve misafir sepeti, "fiyatı değişti", eşikler, satın alınamayan kalem, vitrinden kalkan kalem, girişte birleşme ve L-6'nın taşınması, ödeme adımına geçiş (8 satır) | Cart · CartItem · *deneme kaydı* · *firma ayarları* · StockReservation · veri doğurmaz — türetilir | kalemin ilk eklendiği birim fiyat; üyenin hesap bağı, misafirin tarayıcı bağı; kupon deneme satırının sepet ekseni; ayrılan adet — `02 §3.16`, §3.18, §8.2 L-6 (§1.1.5, §1.1.8) | eşlendi |
| `03 §2.4` | Ödeme adımı ve onay: iletişim e-postası, teslimat ve fatura adresi, deftere kaydetme (K-680), kupon (K-654, K-679, K-806), ödeme yöntemi ve L-8, özet ve cayma özeti, onay kutuları, onay anında yeniden değerlendirme, siparişin oluşması ve donan değerler (9 satır) | Order · OrderItem · AddressBook · Coupon · StockReservation · CheckboxAcceptance · LegalTextVersion · *deneme kaydı* · Cancellation · *gönderim kaydı* | iletişim e-postası, `ShippingAddress` (telefon dahil), `BillingAddress`; kupon ve kalemlere dağılan payı; `PaymentMethod`; onay kutularının kaydı; `OrderNumber`; donan değerler ve sürümler; önceki siparişin iptali — `02 §3.10`, §3.14, §3.17, §3.21, §3.23, §3.24 (§1.1.5) | eşlendi |
| `03 §2.5` | Ödeme ve teslim: kart (3D Secure, başarı bildirimi, ret, yarıda kalan ödeme ve son sorgu), havale (donan IBAN, B-3, "ödendi", süre dolunca iptal), ödeme onayının sonuçları, indirme, kargoya verme, teslim edilemedi, teslim tarihi, hizmetin tamamlanması (17 satır) | Order · OrderItem · *ödeme kaydı* · *gelen bildirim kaydı* · *geçiş kaydı* · StockReservation · Cancellation · DigitalFile · *gönderim kaydı* | ödemenin kaynağı ve anı; ödeme sonucu alınamadı işareti; kargo şirketi, takip numarası ya da kendi aracıyla teslim; `DeliveryDate`; tamamlama işareti; indirme sayacı — `02 §3.20`, §3.21, §5.6 (§1.1.5, §1.1.6) | eşlendi |
| `03 §2.6` | Sipariş takibi: sipariş geçmişi, numara ve e-postayla sorgu (L-5), erişim anahtarlı bağlantı, sipariş sayfasının taşıdıkları (geri ödemeler dahil — K-826), ulaşmayan e-posta, hesabı silinen üyenin siparişi, misafir e-postasının düzeltilmesi (7 satır) | Order · Refund · *deneme kaydı* · *gönderim kaydı* · OrderEmailHistory · Customer · veri doğurmaz — türetilir | hesap bağı; `OrderAccessKey` (ömrü siparişin kişisel verisine bağlı); gerçekleşmiş geri ödemelerin tarihi, tutarı, yolu; sorgu deneme satırları — `02 §3.22`, §10.4.11 (§1.1.5, §1.1.8) | eşlendi |
| `03 §2.7` | İptal ve gecikme feshi: ödeme beklenirken bütünüyle iptal, fiziksel ve hizmet kaleminin adetle iptali (K-787, K-791, K-792), dijitalde yok, havale IBAN'ı, geri ödeme (K-788, K-789), gecikme feshi ve geri ödemesi (8 satır) | Cancellation · LateDeliveryTermination · Refund · RefundIbanRequest · StockReservation · *geri ödeme kaydı* · *geçiş kaydı* | iptal edilen adet ve anı; feshin damgası ve kapsadığı adetler; geri ödemenin tutarı ve kargo payı; geri ödeme IBAN'ı (girişin zaman damgası yok — K-710) — `02 §7.1`, §7.2 (§1.1.8) | eşlendi |
| `03 §2.8` | Cayma ve iade: fiziksel kalemde düğme, beyan (cayılan adet, iade adresi, damga), teslim sırası sorusu, gönderim, teslim alma, ret, kullanılmış mal, mal dönmeyince; hizmet ve dijital kalem; geri ödeme, havalenin gerçekleşmemesi, kart iadesinin gerçekleşmemesi, başka kanaldan cayma (15 satır) | Withdrawal · Return · ReturnRejection · NotReturnedClosure · Refund · RefundIbanRequest · *geri ödeme kaydı* · OrderItem · AuditLog | beyanın damgası, adedi, iade adresi, kaydedeni; teslim sırası cevabı (iz satırı — `02 §7.4.1`); kalemin donmuş cayma istisnası; ulaşma tarihi ve adedi; `RefundFailedMark`, `RefundOutcomePendingMark` — `02 §7.3`, §7.4, §10.4.10 (§1.1.8) | eşlendi |
| `03 §2.9` | Ayıp talebi: "sorun bildir"in aralığı (Z-18), ayıplı adet ve iade adresi (K-793), talep açıkken yeni talep yok, çözüm sistem dışında, "çözüldü", yeniden açma ve tarihi (K-681) (6 satır) | DefectClaim · Refund · RefundIbanRequest · Return · *gönderim kaydı* | kalem, ayıplı adet, açıklama, iade adresi, `DefectClaimStatus`, oluşma ve yeniden açılma anları; para gerektiren çözümün geri ödemesi — `02 §7.5`, §5.10 (§1.1.6, §1.1.8) | eşlendi |
| `03 §2.10` | Uçtan uca anlatılar: fiziksel siparişin ana akışı (8 satır) ve dört anlatı — karışık sipariş, kargodayken cayılan gönderi, IBAN silindikten sonra ulaşan mal, misafir e-postasının düzeltilmesi | Order · OrderItem · Withdrawal · Return · NotReturnedClosure · RefundIbanRequest · Refund · OrderEmailHistory · *geçiş kaydı* | anlatılar §2.1–§2.9'un satırlarına işaret eder, yeni veri taşımaz; ulaşma tarihinin teslim tarihi yokken alt sınırsız olması (`02 §7.4.1`); "mal dönmedi"den sonra ödenen tutarın düşülmesi (`02 §10.4.9`) | eşlendi |

**`03 §3` — hata akışları (6)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `03 §3` metin | `02 §6`'nın her satırı burada tam bir kez kaynak olur; biçim, bildirim ve durum hücrelerinin okunuşu | veri doğurmaz — yazım konvansiyonu | — (satırlar `02 §6`'nın eşidir — §1.1.7) | veri doğurmaz |
| `03 §3.1` | Altı ilke: e-postaya bağlı olmama, ödenmiş siparişi bilgisizlikle iptal etmeme, özet ile siparişin aynılığı, sepetin korunması, nötr limit mesajı, renkle taşınmayan anlam (6 satır) | *gönderim kaydı* · *kanal hâli kaydı* · Order · Cart · *deneme kaydı* · veri doğurmaz — arayüz (`04`) | `02 §6.1.1`–§6.1.6'nın satırları (§1.1.7) | eşlendi |
| `03 §3.2` | Satın alma (22 satır) ve sipariş takibi (6 satır) hataları — `02 §6.2` ve §6.3'ün satırları birebir | Order · Cart · CartItem · Cancellation · StockReservation · *ödeme kaydı* · *geri ödeme kaydı* · *deneme kaydı* · OrderEmailHistory · OrderAddressHistory · AuditLog · yoktur — kapsam dışı | `02 §6.2` ve §6.3'ün alt bölüm ve kimlikli satırları (§1.1.7) — geri düzeltmede ilk adres `OrderAddressHistory`'den (3.2.2.6; GAP-13) | eşlendi |
| `03 §3.3` | İptal, cayma ve iade hataları (36 satır) — `02 §6.4`'ün satırları birebir | Cancellation · Withdrawal · Return · ReturnRejection · NotReturnedClosure · Refund · RefundIbanRequest · *geri ödeme kaydı* · DefectClaim · AuditLog | `02 §6.4`'ün satırları (§1.1.7) — IBAN isteğinin tarihi izde (3.3.20), teslim sırası cevabı izde (3.3.34) | eşlendi |
| `03 §3.4` | Üyelik hataları (17 satır) — `02 §6.5`'in satırları birebir | Customer · *bağlantı kaydı* · PendingEmailChange · *deneme kaydı* · Session · SocialLogin · *imha kaydı* | `02 §6.5`'in satırları (§1.1.7) | eşlendi |
| `03 §3.5` | Firma tarafı hataları: katalog (16), sipariş yürütümü (17), kurumsal içerik (11), mağaza ayarları (16) — `02 §6.6`–§6.9'un satırları birebir | Product · Variant · Category · Discount · Coupon · StockReservation · Order · Cancellation · *geçiş kaydı* · Content · ContactRequest · Company · Branding · *firma ayarları* · Admin · AdminInvitation · LegalTextVersion | `02 §6.6`–§6.9'un satırları (§1.1.7) — eşzamanlı düzenleme ve işlemde sürüm değeri (3.5.1.10, 3.5.2.15 — `05 §7.3`) | eşlendi |

**`03 §5` — itiraz ve anlaşmazlık akışları (4)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `03 §5` metin | Ayrı şikâyet kanalı yok; ürün uyuşmazlığa karar vermez, kararın sonucunu panelin işlemleriyle uygular | yoktur — kapsam dışı | şikâyet ya da itiraz kaydı yok | veri doğurmaz |
| `03 §5.1` | Üç yol ve itirazın yeri: iletişim formu ("Sipariş hakkında"), firmanın e-postayla cevabı ve var olan işlemler, yeniden açma, uyuşmazlık yolları, lehine kararın uygulanması (iç not), kanıtın dayanakları (7 satır) | ContactRequest · DefectClaim · Refund · RefundIbanRequest · Order · LegalTextVersion · CheckboxAcceptance · *gönderim kaydı* · AuditLog · OrderEmailHistory | kanıt: donan metinler ve kutuların kaydı, B-1…B-16'nın gönderim kaydı (Z-43), işlem izi, beyanların damgası, e-posta geçmişi; kararın kendisi kaydedilmez — iç not (`InternalNote`, GAP-12) | eşlendi |
| `03 §5.2` | Ürünün dışında çözülen anlaşmazlıklar: ters ibraz (4 satır), yolda kaybolan iade (5), eksik bilgilendirmede uzayan cayma (4), teslim tarihine itiraz (4) | yoktur — kapsam dışı · *geri ödeme kaydı* · Order · NotReturnedClosure · Refund · Return · Withdrawal · OrderItem · ContactRequest | ters ibraz alanı yok (`02 §3.21.11`); iç not; "mal dönmedi"den sonra ödenen tutar ve sonra ulaşan malda düşülmesi (`02 §10.4.9`); pencere dışı kaydın onayı ve tarihi (`02 §10.4.10`); `DeliveryDate` düzeltmesi | eşlendi |
| `03 §5.3` | İade reddi (6 satır) ve kullanılmış, hasarlı, eksik dönen mal (3 satır) | ReturnRejection · Return · Refund · RefundIbanRequest · AuditLog · yoktur — kapsam dışı | ret kaydı — ulaşma tarihi, adet, iz satırı; kesinti ve değer kaybı alanı yok (`02 §7.4.10`) | eşlendi |

**`03 §6` — kötüye kullanım ve inceleme akışları (4)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `03 §6` metin | Önlemler `02 §8`'de, teknik güvenlik `05`'te; teyidin ispatı ve kandırılmanın sonucu firmada | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `03 §6.1` | Deneme limitleri: L-1…L-9'un adımı, ekseni ve sonucu (9 satır), engel ve normale dönüş (4), girişe saldırıda gerçek kullanıcı (5), stok ve kupon kilitleme (4) | *deneme kaydı* · RecognizedBrowserMark · Order · StockReservation | deneme satırının limiti, ekseni, değeri, anı (`02 §8.2`, §11.2); tanınan tarayıcının ayrı sayacı; L-8 açık sipariş sayısından türer | eşlendi |
| `03 §6.2` | Teyit ve kandırılma: siparişin kanalına dönerek teyit (4 satır), adres çevirtme (3), sahte cayma ya da iptal (3), misafir e-postasını çevirtme (4), hesabın e-postasının ele geçirilmesi (3), havale IBAN'ının yetkisiz değişikliği (4) | Order · AuditLog · OrderEmailHistory · OrderAddressHistory · Withdrawal · Cancellation · RefundIbanRequest · *bağlantı kaydı* · Session · RecognizedBrowserMark · *firma ayarları* · *gönderim kaydı* · Admin | teyit kaydedilmez; iz satırı adres değerini taşımaz — geri düzeltmede ilk adres `OrderAddressHistory`'den (6.2.2.3; GAP-13); IBAN değişikliğinin izi eski ve yeni değerle (`02 §9.3.3`); geri alma bağlantısı | eşlendi |
| `03 §6.3` | Firmanın inceleme akışları: şüpheli açık havale siparişleri (3 satır), veri ihlalinde kapsam (4), etkilenen kişilere ulaşma (2) | StockReservation · Cancellation · AuditLog · *giriş kaydı* · Admin · Order · Customer · ContactRequest · veri doğurmaz — mimari (`05`) | ayrılan adedi tutan siparişler; iz satırının süzgeç alanları (tarih, yönetici); dışa aktarmanın iz satırı; giriş kaydı panelde yok — sistem kayıtlarıyla | eşlendi |

**`03 §7` — bildirim haritası (4)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `03 §7` metin | Tek kanal; alıcı adresleri; ulaşmayan e-postanın üç denemesi, işaret ve uyarı; içerik `08`'de; saklama Z-43 ve Z-34 | *gönderim kaydı* · *kanal hâli kaydı* · Company · Admin · veri doğurmaz — e-posta (`08`) | gönderimin alıcı adresi (siparişin iletişim e-postası, firmanın iletişim e-postası, yöneticinin kendi adresi); `02 §9.1`, §9.3.1 (§1.1.8) | eşlendi |
| `03 §7.1` | Bildirimler ve tetikleyicileri — B-1…B-16 (37 satır), F-1…F-6 (11), hesap ve yönetim e-postaları (6); tetikleyici, akış, alıcı ve içerik özeti | *gönderim kaydı* · veri doğurmaz — e-posta (`08`) | `02 §9.2` B-1…B-16 ve §9.3 F-1…F-6 satırlarının alanları (§1.1.8) — her satır bir gönderim satırı; hesap e-postaları `02 §9.4` metin; yeniden gönderim ayrı satır, ilk alıcıya (7.1.37) | eşlendi |
| `03 §7.2` | Bildirim üretmeyen olaylar ve olmayan bildirimler (30 satır) | veri doğurmaz — kod · yoktur — kapsam dışı | `02 §9.2` metin, §9.3.2, §9.4, §9.5'in maddeleri (§1.1.8) — gönderim satırı doğmaz | veri doğurmaz |
| `03 §7.3` | Geçiş ve olay bazlı eşleme — durum geçişinden ve olaydan bildirime (47 satır) | *gönderim kaydı* · *geçiş kaydı* | gönderim satırının tetikleyen olaya ya da geçişe bağı — S1…S11, Ö1…Ö5 ve kalem kayıtları (§1.1.9) | eşlendi |

**`03 §8` — yönetim akışları (10)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `03 §8` metin | Firma tarafının akışları; tek rol; firmanın kendi işlemi kendine bildirilmez; iz işlem izinin sekiz alanından birine yazılan işlemdir — fiyat, stok girişi ve içerik metni yazılmaz | AuditLog · veri doğurmaz — yazım konvansiyonu | iz satırının alanları (`02 §10.3.1`) | eşlendi |
| `03 §8.1` | Katalog yönetimi: ürün (tip, ad, kategoriler, ana kategori, liste araması — K-737), varyant (`SKU`, fiyat, KDV), stok ve ayrılan adet (K-653), tipe özgü alanlar, görseller, dijital dosya, yayın kapısı, arşiv, kalıcı silme, fiyat geçmişi, indirim, kupon, kategori ağacı (K-805), toplu işlem yok (15 satır) | Product · Variant · Option · OptionValue · Category · *ürün–kategori bağı* · Image · DigitalFile · PriceHistory · Discount · Coupon · StockReservation · AuditLog · yoktur — kapsam dışı | `ProductType`, `PrimaryCategory`, `SKU`, `VatRate`, `StockQuantity`, `PublishStatus`; üretim yeri, ölçü birimi, cayma istisnası (`WithdrawalExemptionReason`), ifa süresi, P-9 sınırı; sayfa adresi ilk yayında; sürüm değeri (eşzamanlı düzenleme) — `02 §3.2`–§3.12 (§1.1.5) | eşlendi |
| `03 §8.2` | Olağan hat: yeni sipariş, havale "ödendi", ödeme onayının sonuçları, hazırlık ve fatura, kargoya verme (listeden şirket ya da "Diğer" ve adı), teslim tarihi ve sırası sorusu, teslim edilemedi, yeniden gönderim, hizmetin tamamlanması, indirme hakkının yenilenmesi, sipariş arama (K-714) (11 satır) | Order · OrderItem · *geçiş kaydı* · *ödeme kaydı* · AuditLog · *gönderim kaydı* · veri doğurmaz — türetilir · veri doğurmaz — ürünün dışında | kargo şirketi (liste değeri ya da "Diğer" ve adı), takip numarası ya da kendi aracıyla teslim beyanı; `DeliveryDate` (sipariş düzeyi); tamamlama işareti; indirme sayacı; sayaçlar türetilir; fatura ürünün dışında — `02 §3.20`, §3.21, §10.6.1 | eşlendi |
| `03 §8.3` | Firma iptali (2 satır — altı sebepli kapalı liste), iade teslim alma ve reddi (5), geri ödeme (9) — IBAN isteği, kart iadesinin gerçekleşmemesi ve sonucunun beklenmesi (K-940), tutar bazlı geri ödeme, "havale gerçekleşmedi" | Cancellation · Return · ReturnRejection · *geçiş kaydı* · Refund · *geri ödeme kaydı* · RefundIbanRequest · AuditLog · Variant | `CancellationReason` ve "diğer" açıklaması; ulaşma tarihi, ulaşan adet, stoğa eklenen adet; geri ödemenin türü, tutarı, yolu, kalem bağı (isteğe bağlı), işleyen; geri ödeme kaydının hâlleri; işaretten sonra gelen başarılı sonucun uyarısı (K-940) — `02 §7.2`, §7.4, §10.4.9 (§1.1.8) | eşlendi |
| `03 §8.4` | Sipariş müdahaleleri: adres, çıkarma, takip bilgisi, teslim ve ulaşma tarihi, durum düzeltmesi (üç sebepli kapalı liste), iç not, "mal dönmedi", başka kanaldan cayma ve fesih kaydı, misafir e-postası; ayıp talebinin yönetimi (11 satır) | Order · ItemRemoval · OrderItem · Return · *geçiş kaydı* · NotReturnedClosure · Withdrawal · LateDeliveryTermination · OrderEmailHistory · OrderAddressHistory · DefectClaim · AuditLog | `ShippingAddress`, `BillingAddress` — eski ve yeni değer `OrderAddressHistory`'de (GAP-13); çıkarma kaydı — `ItemRemoval` (GAP-12); iz eski ve yeni tarihi yazar (`02 §10.4.5`); `CorrectionReason`; iç not — `InternalNote` (GAP-12); damga ve kaydeden; e-posta geçmişi — `02 §10.4` (§1.1.8) | eşlendi |
| `03 §8.5` | Manuel adım bütçesi — hat başına zorunlu adımlar, koşullu adımlar, bütçeye girmeyenler, sistem dışı adımlar (8 satır) ve bütçe notu | veri doğurmaz — kod | — (`02 §10.5`; sayılar veri doğurmaz) | veri doğurmaz |
| `03 §8.6` | Kurumsal içerik kaydı (7 satır), ana sayfa, menü ve marka (4), duyuru (2), iletişim talepleri (4) | Content · AboutPage · Offering · PortfolioItem · FaqItem · Branch · CustomPage · Announcement · *içerik–ürün bağı* · Image · *firma ayarları* · Branding · SocialLink · ContactRequest · AuditLog | `PublishStatus`, elle sıra, ana sayfa ve menü işaretleri, sayfa adresi; `HomepageLayout`, menü adları; `BrandName`, `Logo`, `Favicon`, `BrandColor`, platform imzası; duyurunun metni ve tarihleri; `ContactRequestStatus`, son kapatıldığı an, `ContactTopic` — `02 §3.27`–§3.29, §3.32 (§1.1.5) | eşlendi |
| `03 §8.7` | Mağaza ayarları: kurulum kontrol listesi ve satış kapısı (2 satır), firma kimliği ve yasal metinler (3), ödeme yöntemleri (3), kargo, teslimat, süreler, eşikler ve iade adresi (5), satışın geçici kapatılması (2) | Company · *firma ayarları* · PrivacyNotice · CookiePolicy · PreInformationForm · DistanceSalesContract · LegalTextVersion · *il listesi* · AuditLog · *gönderim kaydı* · veri doğurmaz — türetilir | `CompanyType` ve tipe göre kimlik alanları, ETBİS bilgisi; havale IBAN'ı; kargo ücreti, eşikler, teslimat illeri, süreler, P-8, P-10, iade adresi, geçici kapatma; metinlerin sürümleri ve yayını; satış kapısı ve kontrol listesi türetilir; ayar değişikliğinin izi eski ve yeni değerle — `02 §3.1`, §3.18–§3.21, §3.33, §11 (§1.1.5, §1.1.8) | eşlendi |
| `03 §8.8` | Yönetici hesapları ve davet: davet, hesabın açılması (ad — K-736), geri çekme, süre dolması, kaldırma, yanlış adrese giden davet (6 satır) | Admin · AdminInvitation · *bağlantı kaydı* · Session · AuditLog · *gönderim kaydı* | yöneticinin adı, e-postası, şifre özeti; davetin adresi, gönderen yönetici, hâli ve anları; iz satırları; F-6 — `02 §10.2` (§1.1.8) | eşlendi |
| `03 §8.9` | Bekleyen işler (altı sayaç, IBAN listesi, kanal ve firma bildirimi uyarıları), yeniden gönderim (K-761), satış özeti, işlem izinin okunması, sipariş, üye ve talep dışa aktarması (7 satır) | veri doğurmaz — türetilir · *gönderim kaydı* · *kanal hâli kaydı* · AuditLog · Order · *ödeme kaydı* · Customer · ContactRequest | sayaçlar ve özet saklanmaz (`02 §10.6.1`); ulaşmayan her gönderim ayrı satır; iz satırı — kim, ne zaman, ne değişti; dışa aktarmanın alanları (`02 §10.7.1`) ve iz satırı | eşlendi |

**`03 §9` — üyelik akışları (6)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `03 §9` metin | Üyelik akışı; siparişin ön koşulu değil; kurallar yöneticiye de; hesap e-postaları matrisin dışında, işaret düşmez | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `03 §9.1` | Kayıt, doğrulama ve Google ile ilk giriş: kapı (K-850), ad, şifre, L-3, doğrulanmamış kayıt, L-9, düğmeyle doğrulama (K-904), misafir siparişlerinin bağlanması, Google ile doğuş ve bekleyen kaydın silinmesi (K-696) (7 satır) | Customer · *bağlantı kaydı* · *deneme kaydı* · SocialLogin · Order · *imha kaydı* · *gönderim kaydı* | ad, e-posta, şifre özeti, doğrulanma işareti ve anı; bağlantının ömrü ve kullanılması; Google bağı; siparişin hesap bağı; bekleyen kaydın silinmesi imha kaydına (K-708) — `02 §3.13` (§1.1.5) | eşlendi |
| `03 §9.2` | Giriş, oturum, sıfırlama, yeniden doğrulama: "beni hatırla", L-1, giriş kaydı, tanınan tarayıcı, Google girişi ve bağı (K-733), çıkış, sıfırlama bağlantısı, oturumların kapanması, yeniden doğrulama (K-711), panel girişi ve şifresi (8 satır) | Session · *giriş kaydı* · RecognizedBrowserMark · SocialLogin · *bağlantı kaydı* · *deneme kaydı* · Customer · Admin · Cart | oturum türü ve son işlem anı; giriş kaydı — an, IP, hesap, sonuç; tanınan tarayıcı işareti; yeniden doğrulamanın tek işlemlik izni (K-899); `AccountType` — `02 §3.13`, §8.2 (§1.1.5, §1.1.8) | eşlendi |
| `03 §9.3` | Profil ve adres defteri: ad, adres defteri (K-680), şifre değiştirme (K-695), e-posta değişikliği ve bekleyen değişiklik (K-734), düğmeyle doğrulama, geri alma (K-839), yöneticinin e-postası ve adı (K-736) (7 satır) | Customer · Admin · AddressBook · PendingEmailChange · *bağlantı kaydı* · Session · RecognizedBrowserMark · SocialLogin · AuditLog · *il listesi* | defterin alanları ve adı; bekleyen yeni adres; geri alma bağlantısı; Google bağının kaldırılması; yönetici e-postası değişikliğinin izi — `02 §3.13.14`, §3.14, §10.2.4 (§1.1.5, §1.1.8) | eşlendi |
| `03 §9.4` | Hesabın silinmesi ve kişisel veri başvurusu: silme (K-698), hemen silinenler ve imha kaydı (K-708), silinmiş hesabın siparişi, KVKK talebi, üye kaydı görünümü, talep üzerine silme ve izi (K-715), yeniden kayıt (7 satır) | Customer · Session · AddressBook · Cart · SocialLogin · RecognizedBrowserMark · PendingEmailChange · *imha kaydı* · Order · ContactRequest · AuditLog · *gönderim kaydı* | anında fiziksel silme; siparişin hesap bağı kalkar; silmenin imha kaydı ve iz satırı (işlem ve tarih); hesabın silindiği bildirimi — `02 §3.15`, §12.2.7 (§1.1.5, §1.1.8) | eşlendi |
| `03 §9.5` | Tercih yönetimi — bildirim tercihi, pazarlama onayı, dil, çerez tercihi, veri indirme, firma bildirim adresi: hepsi yoktur (6 satır) | yoktur — kapsam dışı | tercih, pazarlama onayı, dil ve ayrı bildirim adresi alanı yok — `02 §9.1.3`, §9.5, §12.1.1 (§1.1.8) | veri doğurmaz |

**`03 §10` — operasyonel akışlar (5)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `03 §10` metin | Dış servisler, satışın durdurulması, sitenin kesintisi, panele erişimin kaybı; bildirim yok; tespit `05` ve `08`'de | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `03 §10.1` | Dış servis kesintileri: ödeme sağlayıcısı (7 satır — son sorgu, geç gelen ödeme, sonucu bilinmeyen iade — K-705, K-940), e-posta altyapısı (6 — denemeler, işaret, kanal uyarısı, yeniden gönderim), Google (4) | Order · *ödeme kaydı* · *gelen bildirim kaydı* · *geri ödeme kaydı* · Cancellation · *gönderim kaydı* · *kanal hâli kaydı* · AdminInvitation · SocialLogin · veri doğurmaz — kurulum | ödeme sonucu alınamadı işareti; geri ödeme kaydının hâlleri; gönderimin deneme sayısı ve sonucu; kanal hâli; sağlayıcı anahtarları kurulumda — `02 §6.1`, §6.2.15, §6.2.16, §9.1.6 (§1.1.7, §1.1.8) | eşlendi |
| `03 §10.2` | Bakım ve güncelleme: geçici kapatma, duyuru, güncelleme, yedek ve sıfır kayıp — tek istisna bir dakikalık pencere (K-888, K-939, K-983) (4 satır) | *firma ayarları* · Announcement · veri doğurmaz — mimari (`05`) · yoktur — kapsam dışı | geçici kapatma ayarı; duyurunun tarihleri; kaybolmaz listesi `02 §3.34.8`'de, mekanizması `05 §7.5`; bakım modu yok | eşlendi |
| `03 §10.3` | Sitenin kesintisi: ürün çalışmaz, süreler işler, dönüşte kaçırılan işler sırayla, havale iptalinin ertelenmesi, sayaçlar, duyuru (6 satır) | Order · *iş kuyruğu* · *nabız ve kesinti kaydı* · veri doğurmaz — türetilir | ertelenen son an (K-898); zamanlanmış işler; kesinti kaydı (K-895) — `02 §3.34.7`, §4.3 (§1.1.5, §1.1.6) | eşlendi |
| `03 §10.4` | Panele erişimin kaybı: şifre sıfırlama, L-1 engeli, davet ve kaldırma, kurulum tarafının yeni yönetici açması ve ele geçirilmiş hesabın oturumlarını sonlandırması (K-699, K-702), paneli geri alma (6 satır) | Admin · AdminInvitation · Session · *firma ayarları* · AuditLog · veri doğurmaz — kurulum | kurtarma kodu alanı yok; kurulum işlemi işlem izine yazılmaz; IBAN düzeltmesi ve kaldırmanın izi — `02 §10.2.7` (§1.1.8) | eşlendi |

#### 1.1.11 Devirler — karar satırları, park maddeleri ve gövde cümleleri (173)

Birim K-1003'tür (K-997'nin 1d netleşmesi). **Devir satırı** K numarasıyla girer (`K-n`): K-479…K-991'in etki sütununda `06` taşıyan 126 karar satırı — Aşama 1'in kalite döngüsü ve checkpoint'i 45, Aşama 2 23, Aşama 3 14, Aşama 4 44 — 2b'de 127: K-879'un etki sütununa `06` eklendi (K-1009; checklist §3, "satırı güncelleyen karar bütün kollarını taşır") ve satırı Aşama 4'ün bloğuna girdi (45). **Park maddesi** `park An-m` kimliğiyle girer — n aşama, m maddenin bloğundaki sırası; dört bloğun 23 talimat maddesi (Aşama 1 iki, Aşama 3 on, Aşama 4 on bir); üç dizin maddesi girmez, gösterdikleri devirler karar satırlarıyla girer. **Gövde cümlesi** önceki dokümanların gövdesinde `06`'yı ya da "Veri Modeli"ni anan paragraf ya da tablo satırıdır, `0X §bölüm gN` kimliğiyle — Ürün Gereksinimleri 11, Arayüz Tanımları 2, Teknik Mimari 10; dizinlerin 21 cümlesi ve dizinlerden sonra eklenen iki cümle: `02 §1.1 g2` (K-977) ve `05 §3.5.2 g1` (K-982); Proje Vizyonu, Kullanıcı Akışları ve MVP Kapsamı'nın gövdesinde sıfır. Aşama 1'in workshop satırları (K-01…K-478) ayrı satır olarak girmez, `02`'nin kural ailesiyle karşılanır (K-997; karar kaydı §14.2, betik 2).

**Satırın yazımı.** "Kaynak özeti" devrin işini adıyla yazar; etki sütununda yalnız `06` taşıyan 44 satırda iş karar ve gerekçe hücresinden çıkarıldı ve özet `(yalnız `06`)` işaretiyle başlar (checklist §1 ve §4 — K-916'nın vakası: iş yalnız elenen seçeneğin gerekçesindeydi). Devrin işi önceki bir matris satırında ya da park satırında zaten eşlenmişse satır yine girer — kapsama devir başınadır — ama alanı ikinci kez tarif etmez: "Alan" hücresinin "→" işaretinden sonrası karşılayan satırı (`02` ya da `03` kimliği ve alt bölümü — §1.1.3–§1.1.10) ve park satırını (`park An-m`) gösterir; alanın adı, türü ve biçimi gösterilen konudadır (VMn-mm). Gösterilen her `02` ve `03` kimliği matriste vardır (betikle denetlendi, 2026-10-17); `02 §6`'nın kimliğiyle girmeyen hata satırları alt bölüm satırıyla gösterilir (`02 §6.7`). "Durum" karşılayan satırın değer kümesindendir; satırların 155'i eşlendi, 17'si veri doğurmaz — yazım konvansiyonu 12, mimari (`05`) 4, kod 1.

**Okuma derinliği.** 126 karar satırı konu, karar, gerekçe ve etki hücreleriyle, kesilmeden tam okundu (338 KB, on parça); park bloklarının 26 maddesi, dört dizinin `06` başlıkları ve gövde cümleleri (`PHASE1_CONFLICT_SCAN.md` §6.1–§6.2, `PHASE2_CONFLICT_SCAN.md` §6.1–§6.2, `PHASE3_CONFLICT_SCAN.md` §6.1–§6.2, `PHASE4_CONFLICT_SCAN.md` §6.1–§6.2) ve 23 gövde cümlesinin paragrafı tam okundu. Kesilmiş okuma yoktur.

**Tarama sonuçları.** (1) **"Bu aşamanın kararıdır" diyen her madde konu planında karşılık bulur:** park bloklarının 21 maddesi (Aşama 3'ün onu, Aşama 4'ün on biri) ve "`06`'nın işidir" ya da "kararıdır" diyen gövde cümleleri bir VM konusuna bağlıdır — satırın "Alan" hücresi konuyu taşır (betik: konusuz park satırı sıfır); devirlerin ve park maddelerinin K numaralarının hepsi §14.3'te geçer (karar kaydı §14.2, betik 1 — boş). ★ konu çıkmadı. (2) **§1.1.2'nin 1a–1c'de kullanılmayan iki adayı:** 70 (*yedeğin durum kaydı*) K-918, K-937, K-963 ve `park A4-10` ile eşlendi. 71 (*anahtar halkası*) yalnız K-975'in (4). kolunda anılır — eski bir ana dönüşte veri koruma anahtarı da geri gider; halkanın veritabanında durduğu `05 §7.6.3`'tedir (K-879, MK-26) ve hiçbir park satırı onu taşımaz: K-879'un etki sütununda `06` yoktur, `park A4-3` K-975'in (4). kolunu almadı. Konu planındaki yeri VM4-14'tür; 2. oturum geri izlenebilirlikte kaynağını `05 §7.6.3` diye yazar — VM1-04'ün "teknik kayıtların kaynağı park satırıdır" kuralının park satırsız tek adayıdır. **2b (K-1009):** `park A4-3` K-879'u ve K-975'in (4). kolunu taşır, K-879'un etki sütunu `06`'yı sayar; 71'in kaynağı artık park satırıdır (§1.2). (3) **Yeni GAP adayı çıkmadı.** İki adaya devirlerin gerekçe hücrelerinden dayanak eklendi (§1.1.12): VA-1'e K-617 ve K-645'in elediği seçenekler, VA-2'ye K-527'nin elediği seçenek ve K-585'in gerekçe cümlesi. (4) **K-1015'ya not:** K-675 panel işaretlerinin saklanmadığını, doğuran koşuldan okunduğunu söyler; `park A4-7` ise "geri ödeme sonucu bekleniyor" ve "geri ödeme gerçekleşmedi" işaretlerinin geri ödeme kaydının hâliyle "aynı işlemde yazıldığını" (K-940; `05 §4.6.2`). İki okuma — işaret ayrı bir alan mı, hâlden mi okunur — denormalizasyon envanterinde ayrılır.

**Devir satırları — Aşama 1'in kalite döngüsü ve checkpoint'i (45)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| K-480 | Firma tipi üçtür — esnaf, gerçek kişi tacir, tüzel kişi; tipe göre yasal kimlik alanları (K-14'ü genişletir) | Company | `CompanyType` ve tipe göre kimlik alanları — → `02 §3.1.3`, §1.2 `CompanyType` (§1.1.4, §1.1.5); değerlerin kod adı VM5-03 | eşlendi |
| K-491 | İade malının ulaşma tarihi — teslimden sonraki caymada geri ödemenin on dört günü ondan işler; teslim alma adımı tarih alanı taşır | Return · Withdrawal · OrderItem | ulaşma tarihi (geçmişe dönük, ileri tarihli olamaz); beyanın damgası; `DeliveryDate` — → `02 §4.2` Z-16, §6.4.8, §7.4.1, §7.4.7 (§1.1.6–§1.1.8) | eşlendi |
| K-496 | Kalemin kapanış kaydı — "mal dönmedi" kapanışı, gönderme süresi geçince; mal ulaşırsa yeniden açılır | NotReturnedClosure · AuditLog | kapanışın adedi, anı, yapanı; yeniden açılma — → `02 §10.4.9`, §6.4.18, `03 §1.7.1.8` (§1.1.7–§1.1.9) | eşlendi |
| K-497 | IBAN'ın silinme anları — havale hattında iki ek an: "mal dönmedi" kapanışı ve gönderme süresinden otuz gün sonra | Withdrawal · RefundIbanRequest · Order | IBAN'ın silineceği gün ve silme anı (K-895) — → `02 §4.2` Z-35, Z-38 (§1.1.6) | eşlendi |
| K-498 | IBAN isteği ve tarihi — silinen IBAN'ın sipariş sayfasından yeniden istenmesi, B-14, süre durmaz, "IBAN bekleniyor" | RefundIbanRequest · *gönderim kaydı* | isteğin tarihi ve kalem bağı; B-14'ün gönderim satırı; işaret türetilir — → `02 §6.4.19`, §7.4.5 (§1.1.7, §1.1.8) | eşlendi |
| K-500 | (yalnız `06`) Kargo ücretinin KDV'si fiziksel kalemlerin oranını izler ve orantılı bölünür; kargo KDV oranı ayarı kalkar (P-2) | Order | kargo ücretinin oranlara bölüşümü ve parçaların KDV'si (donmuş); ayarda oran yok — → `02 §3.19.2`, §11.1 P-2 (§1.1.5, §1.1.8) | eşlendi |
| K-508 | (yalnız `06`) Referans fiyatın penceresi — malda on gün içindeki en düşük fiyat, hizmette ve çabuk bozulanda önceki fiyat | PriceHistory · Product | varyantın uygulanmış fiyatları ve başlangıç anları; kuralı seçen `ProductType` ve `WithdrawalExemptionReason` — → `02 §3.9.3`, §3.9.4 (§1.1.5) | eşlendi |
| K-511 | (yalnız `06`) Fiyat etiketinin zorunlu bilgileri — üretim yeri, ölçü birimi ve net miktar, fiyatın başlangıç tarihi; birim fiyat türetilir | Product · PriceHistory | üretim yeri (ülke); `MeasurementUnit` (değerler K-1018) + `NetQuantity`; fiyatın başlangıç tarihi geçmişten — → `02 §3.8.5` (§1.1.5) | eşlendi |
| K-515 | (yalnız `06`) Giriş kaydı — iki hesap türünün her girişi: an, IP, hesap, sonuç; altı ay | *giriş kaydı* | an, IP, hesap türü, e-posta, sonuç; süresi Z-40 — → `02 §3.13.20`, §4.2 Z-40 (§1.1.5, §1.1.6) | eşlendi |
| K-519 | (yalnız `06`) Ödemesi hiç alınmamış siparişin kişisel verisi üç yıl; kişisel verisiz kayıt kalır | Order · *imha kaydı* | Başarısız olduğu an (başlangıç değeri); kişisel alanların boşaltılması — → `02 §4.2` Z-41 (§1.1.6) | eşlendi |
| K-526 | (yalnız `06`) Tek başına "müşteri" (`Buyer`) aktör değildir; `Customer` yalnız üye müşteridir | veri doğurmaz — yazım konvansiyonu · Customer | `Buyer`'ın kaydı yoktur; `Customer` üye müşterinin hesabıdır — → `02 §1.2` `Buyer`, `Customer`, §1.3 metin (§1.1.4); VM0-06, VM0-07 | eşlendi |
| K-527 | (yalnız `06`) İşlem izinin sipariş tarafındaki kapsamı — firmanın elle geçişleri ve kayıtları; iz müşterinin kişisel verisini değer olarak taşımaz | AuditLog | iz satırının alanı ve işlemi; siparişi adlandırır, adres değerini taşımaz — → `02 §10.3.1` (§1.1.8); adres düzeltmesinde eski ve yeni değer izde değil `OrderAddressHistory`'de (GAP-13; K-527 istisnasız kalır) | eşlendi |
| K-531 | (yalnız `06`) Taslak tanımı, eksen bağı ve kapalı listelerin sözlükteki yeri — değerlerin kod adlarını `06` verir | veri doğurmaz — yazım konvansiyonu · Product · Order | `PublishStatus` `Draft`; iki eksenin birleşim kısıtı; kapalı liste değerlerinin kod adı VM5-03 — → `02 §1.1` m5, §1.2 `Draft`, §5.6.1 (§1.1.4, §1.1.6) | eşlendi |
| K-532 | (yalnız `06`) Aynı harfle başlayan kimlik aileleri — ödeme geçişleri Ö1…Ö5, sevkiyat S1…S11; kimlikler olduğu gibi kullanılır | *geçiş kaydı* · veri doğurmaz — yazım konvansiyonu | geçişin kimliği (S1…S11, Ö1…Ö5) geçiş kaydının değeridir (VM5-02) — → `02 §5.5` Ö1…Ö5, §5.4 S1…S11 (§1.1.6); VM0-06 | eşlendi |
| K-548 | (yalnız `06`) Siparişe bağlı müşteri bildirimlerinin gönderim kaydının saklaması (Z-43) — K-951 on yıla çevirdi | *gönderim kaydı* · Order | gönderim anı (başlangıç değeri); sipariş bağı; süre kolu K-951'in satırında — → `02 §4.2` Z-43, Z-34 (§1.1.6) | eşlendi |
| K-561 | (yalnız `06`) Adresin alanları — alıcı adı, il (kapalı liste), ilçe, açık adres; teslimat adresinde zorunlu telefon; posta kodu ve T.C. kimlik no yok | Order · AddressBook · *il listesi* | `ShippingAddress` (telefon dahil), `BillingAddress` (telefonsuz); adres defteri kaydında telefon isteğe bağlı; il kapalı listeden — → `02 §3.14.6`, §1.2 `ShippingAddress`, `BillingAddress` (§1.1.4, §1.1.5) | eşlendi |
| K-563 | (yalnız `06`) On günden kısa geçmişli varyantta referans fiyatın başlangıcı — varyantın satışa sunulduğu ilk an | PriceHistory · Variant | fiyat geçmişinin başlangıç anı; varyantın ve ürününün birlikte Yayında olduğu ilk an — → `02 §3.9.4` (§1.1.5) | eşlendi |
| K-565 | (yalnız `06`) Kupon siparişi sıfır tutara indiremez; sabit kuponun asgari sepet tutarı kupon tutarından büyük | Coupon | kupon tutarı ve asgari sepet tutarı (kısıt: asgari > tutar; yüzde < 100) — → `02 §3.10.4` (§1.1.5) | eşlendi |
| K-566 | (yalnız `06`) Yanlış iptalin düzeltilmesi — yalnız ödeme Ödendi'de kalmışsa; düzeltme ayırmaları yeniden yapar, yetmezse yapılmaz | StockReservation · Order · Coupon · AuditLog | ayrılan adet ve kupon hakkının yeniden ayrılması; düzeltmenin iz satırı — → `02 §10.4.6`, §5.4 (§1.1.6, §1.1.8) | eşlendi |
| K-567 | (yalnız `06`) Sipariş numarası bütün siparişler arasında tekildir; çakışmada yeni numara üretilir | Order | sipariş numarası (tekil kısıt; P-19) — → `02 §3.22.2` (§1.1.5); K-1011, VM7-01 | eşlendi |
| K-570 | (yalnız `06`) Para gelmeden konmuş havale "ödendi" işaretinin düzeltilmesi — iki eksen birlikte döner, ayırmalar tutulu kalır, Z-8 yeniden başlar | Order · StockReservation · AuditLog · *gönderim kaydı* | iki eksenin değeri; ödeme süresinin bittiği an (yeniden yazılır — K-895); B-13 gönderimi; iz satırı — → `02 §10.4.6`, §5.5, §4.2 Z-8 (§1.1.6, §1.1.8) | eşlendi |
| K-575 | (yalnız `06`) Geri ödeme IBAN'ının düzeltilmesi ve bankada gerçekleşmeyen havale — IBAN silinir, alan yeniden açılır, B-14 | Withdrawal · RefundIbanRequest · Refund · AuditLog | IBAN (girişte biçim denetimi), düzeltilebilirlik geri ödeme işlenene kadar; "havale gerçekleşmedi" adımının iz satırı; IBAN isteği — → `02 §7.4.5`, §6.4.27 (§1.1.7, §1.1.8) | eşlendi |
| K-582 | (yalnız `06`) Süresi dolan siparişte kalan ticari kayıt — kişisel alanlar boşalır, hesap bağı kesilir; kalanların listesi | Order · OrderItem | boşaltılan kişisel alanlar ve hesap bağı; kalan ticari alanlar (numara, kalemler, tutarlar, KDV, pay, yöntem, tarihler, durumlar, donmuş firma kimliği) — → `02 §12.2.7`, §4.2 Z-28, Z-41 (§1.1.6, §1.1.8); alan listesi VM9-02 | eşlendi |
| K-585 | (yalnız `06`) Misafir siparişinin iletişim e-postasının firmaca düzeltilmesi — yeni erişim anahtarı, B-1 ve B-15, hesap bağı, "e-posta düzeltildi" işareti | Order · OrderEmailHistory · *gönderim kaydı* · AuditLog | iletişim e-postası (donmanın tek istisnası); erişim anahtarı yenilenir; eski ve yeni değer (K-586 ile e-posta geçmişinde) — → `02 §10.4.11`, §3.23.3, §6.3.5 (§1.1.5, §1.1.7, §1.1.8) | eşlendi |
| K-586 | E-posta geçmişi siparişin parçası — eski ve yeni adres siparişin kaydında, izde değil; sipariş verisiyle imha | OrderEmailHistory · AuditLog | eski ve yeni değer, düzeltmeyi yapan yönetici, tarih; iz satırı değer taşımaz — → `02 §10.4.11`, §10.3.1 (§1.1.8); imhası VM9-02 | eşlendi |
| K-587 | Eşzamanlılık mekanizması — siparişe yönelik her işlem güncel hâline karşı; ikinci işlem uygulanmaz | Order | siparişin sürüm değeri (K-874) — → `02 §3.31.2` (§1.1.5); K-1014 | eşlendi |
| K-588 | Hesap türü ve e-posta tekilliği — iki tür iki kapı; e-posta kendi türünde tekil | Customer · Admin | iki ayrı hesap tablosu; e-posta tekilliği tür içinde (K-982) — → `02 §10.2.5`, §8.1.7, §3.13.7 (§1.1.5, §1.1.8); K-1016 | eşlendi |
| K-589 | Siparişin ödeme kaydı — iptal edilmiş siparişe geç gelip sistemce geri ödenen kart ödemesi siparişi ödemesi alınmış yapar | *ödeme kaydı* · Refund · Order | ödemenin ve geri ödemesinin tarih ve tutarı; "ödemesi alınmış" Z-28'i seçer (K-951) — → `02 §6.2.16`, §4.2 Z-28 (§1.1.6, §1.1.7) | eşlendi |
| K-590 | Geçiş koşulları — gecikme feshine uğramış fiziksel kalem kargoya verilmez, yeniden gönderilmez, kapanışta iptal edilmiş kalem gibi sayılır; stok iptalin kalıbıyla döner | LateDeliveryTermination · OrderItem · Order · Variant | feshin kalem ve adet bağı (S5, S8, S10, S11'in koşulu kayıttan okunur); stoğun dönüşü — → `02 §5.4` S5, S8, §6.7, §7.2.3 (§1.1.6–§1.1.8) | eşlendi |
| K-591 | Onay anında sürüm karşılaştırması — formun ya da sözleşmenin sürümü artarsa sipariş oluşmaz; siparişe görülen sürüm donar | LegalTextVersion · Order | metnin sürümü; siparişe donan Ön Bilgilendirme Formu ve sözleşme sürümü — → `02 §3.24.3`, §3.33.4, §3.17.2 (§1.1.5) | eşlendi |
| K-592 | Kapanış kaydı — açık kalemi kalmayan sipariş İptal edildi'ye kapanış olarak geçer; firma iptali değildir, iptal kaydı düşmez | Order · *geçiş kaydı* | kapanışın geçişi (S4'ün sistem kolu, S9) ve sebebi kayıtta cayma ya da fesih; kaleme iptal kaydı yok — → `02 §5.4` Cancelled, S4, S9, §6.7 (§1.1.6, §1.1.7) | eşlendi |
| K-593 | (yalnız `06`) Karışık siparişte fiziksel kalemleri teslimden önce kapanırsa hizmet kaleminin penceresi tamamlanma işaretine kadar açık kalır | OrderItem · Order | hizmetin "tamamlandı" işareti; onay anındaki kalemlerin tipi (donmuş `ProductType`); `DeliveryDate` yok — → `02 §7.3.3`, §6.4.29, §4.2 Z-14 (§1.1.6–§1.1.8) | eşlendi |
| K-594 | (yalnız `06`) Siparişe girebilecek kalemi kalmayan sepetten sipariş verilemez; onay anında oluşursa numara, ödeme ve ayırma doğmaz | veri doğurmaz — kod | — (sepetin kalemleri `Cart`, `CartItem`'da; kural koddadır) — → `02 §3.16.7`, §6.2 (§1.1.5, §1.1.7) | veri doğurmaz |
| K-595 | Onay kutusu kaydı — hangi kutu, gösterilen metni, kapsadığı kalemler siparişe donar; ayrı damga yok | CheckboxAcceptance · Order | kutunun türü, metni, kalem bağı; anı siparişin oluşma anı — → `02 §3.24.6`, §3.23.3 (§1.1.5); `CheckboxAcceptance` (GAP-12) | eşlendi |
| K-596 | (yalnız `06`) "Mal dönmedi" kapatması geri ödeme borcuna karar vermez; firmanın kararıyla kaleme bağlı tutar bazlı geri ödeme | Refund · NotReturnedClosure · AuditLog | tutar bazlı geri ödemenin kalem bağı; yeniden açılmada düşülen kısım — → `02 §10.4.9`, §6.4.30, §5.5 (§1.1.6–§1.1.8) | eşlendi |
| K-598 | Kalem alanları — kalem ürün tipini, cayma istisnası işaretini, sebebini ve koşulunu, hizmette ifa süresini dondurur | OrderItem | donmuş `ProductType`, `WithdrawalExemptionReason` ve koşulu, ifa süresi — → `02 §3.23.1`, §7.3.4, §4.2 Z-39 (§1.1.5, §1.1.6, §1.1.8) | eşlendi |
| K-600 | (yalnız `06`) Varyant fiyatı sıfırdan büyük; indirim fiyatı sıfıra indiremez (%100 indirim yok) | Variant · Discount | fiyat (en küçük 0,01 TL); indirim oranı < 100 ve indirimli fiyat > 0 (kısıt) — → `02 §3.8.4`, §3.9.1, §6.6 (§1.1.5, §1.1.7) | eşlendi |
| K-604 | S5'in koşulu — kargoya verilmemiş kalemde cayma kaydı açılmaz, bildirim iptalle işlenir; S5 açık fiziksel kalemi sayar | Cancellation · OrderItem · Order | iptalin sebebi "müşteriyle anlaşıldı (müşteri talebi)"; S5'in koşulu kalem kayıtlarından okunur — → `02 §5.4` S5, §6.4, §7.3.7 (§1.1.6–§1.1.8) | eşlendi |
| K-605 | Hesabın ad alanı — zorunlu ad, Google'dan gelen ad, kullanıcı değiştirir; siparişin alıcı adı değildir | Customer | hesabın adı — → `02 §3.13.21`, §3.15.1 (§1.1.5) | eşlendi |
| K-606 | (yalnız `06`) Eksik bilgilendirmede cayma süresinin yasal uzaması — firma pencere dışında ve istisnalı kalemde de beyan kaydedebilir, bir yıl sınırı | Withdrawal · AuditLog | beyanın tarihi (pencere dışı, ayrı onay); kaydın iz satırı — → `02 §7.3.1`, §6.4.32, §10.4.10, §4.2 Z-13 (§1.1.6–§1.1.8) | eşlendi |
| K-608 | (yalnız `06`) Teslim günü cayma beyanında teslimin sırası — sipariş sayfasından gelen beyan teslim işaretinden sonraysa sonra; ötekinde firmanın zorunlu cevabı | Withdrawal · OrderItem · AuditLog | teslim sırası cevabı (beyan kaydında); beyanın saati; teslim işaretinin anı — → `02 §7.4.1`, §6.4.34, §3.20.11 (§1.1.5, §1.1.7, §1.1.8) | eşlendi |
| K-610 | (yalnız `06`) Yerli üretim logosu — üretim yeri Türkiye olan fiziksel üründe kendiliğinden gösterilir; ürünle gelen varlık | Product · veri doğurmaz — kod | üretim yeri (tetik); logo koddadır, alan yok — → `02 §3.8.5` (§1.1.5) | eşlendi |
| K-617 | Beş adın şemadaki karşılığı — `OrderAccessKey`, `OrderEmailHistory`, `AccountType`, `RecognizedBrowserMark`, `Chargeback` sözlüğe girdi | Order · OrderEmailHistory · Customer · Admin · RecognizedBrowserMark · yoktur — kapsam dışı | `OrderAccessKey`; e-posta geçmişi kaydı; `AccountType` (iki tablo; değerler VM5-03); tanınan tarayıcı kaydı; ters ibrazın kaydı yok — → `02 §1.2` beş terim (§1.1.4) | eşlendi |
| K-627 | (yalnız `06`) Firma kimliğinin iletişim bilgileri — KEP adresi (üç tipte zorunlu, boşaltılamaz), işletme adı ya da tescilli marka, meslekle ilgili davranış kuralları | Company | KEP adresi; işletme adı ya da marka; davranış kuralları (bağlantı ya da kısa metin) — → `02 §3.1.2`, §3.1.3 (§1.1.5) | eşlendi |
| K-645 | Üç adın şemadaki karşılığı — `NotReturnedClosure`, `RefundIbanRequest`, `RefundFailedMark` sözlüğe girdi | NotReturnedClosure · RefundIbanRequest · Order · *geri ödeme kaydı* | üç kaydın ve işaretin kod adı sözlükten — → `02 §1.2` `NotReturnedClosure`, `RefundIbanRequest`, `RefundFailedMark` (§1.1.4) | eşlendi |

**Devir satırları — Aşama 2 (23)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| K-653 | (yalnız `06`) Varyantın ayrılmış adedi ve onu tutan siparişler; stok ve kontenjan ayrılmış adedin altına inmez | Variant · StockReservation | ayrılmış adet ve ayırmanın siparişi; koşullu güncelleme (K-980) — → `02 §3.6.7`, §6.6.14 (§1.1.5, §1.1.7); saklanır — sayaç satırda (K-1015) | eşlendi |
| K-654 | (yalnız `06`) Kuponun kullanılmış ve ayrılmış hak sayıları; toplam adet onların altına inmez | Coupon · StockReservation | kullanılmış ve ayrılmış haklar; toplam kullanım adedi — → `02 §3.10.6`, §6.6.15 (§1.1.5, §1.1.7); saklanır — sayaç (K-1015) | eşlendi |
| K-655 | (yalnız `06`) Silinen ürüne ya da varyanta bağlı açık siparişlerin sayımı; ayırma varyantla düşer, sipariş donmuş kalemle sürer | OrderItem · StockReservation · Variant | kalemin donmuş değerleri; varyant bağının silmeye dayanması; ayırmanın düşmesi — → `02 §3.7.7`, §6.6.16 (§1.1.5, §1.1.7) | eşlendi |
| K-656 | Ret kaydı — koşullu istisnalı kalemde "iade reddedildi — koruyucu ambalaj açılmış"; ulaşma tarihi, iz, IBAN silinir, geri alınmaz | ReturnRejection · AuditLog · RefundIbanRequest | ret kaydı — ulaşma tarihi, adet; iz satırı — → `02 §7.4.9`, §6.4.35, §1.2 `ReturnRejection` (§1.1.4, §1.1.7, §1.1.8) | eşlendi |
| K-659 | (yalnız `06`) Kayıt başına tek geçerli doğrulama bağlantısı; ömür ilk gönderimden işler (Z-1) | *bağlantı kaydı* · Customer | bağlantının oluşma anı, ömrün sonu, yerine yenisi geçtiği an; bekleyen kaydın oluşma anı — → `02 §3.13.5`, §4.2 Z-1 (§1.1.5, §1.1.6) | eşlendi |
| K-660 | (yalnız `06`) Yönetici davetinin geri çekilmesi; davetin gönderilmesi ve geri çekilmesi işlem izine | AdminInvitation · *bağlantı kaydı* · AuditLog | davetin hâli (geri çekildi); iz satırları — → `02 §10.2.6`, §4.2 Z-3 (§1.1.6, §1.1.8) | eşlendi |
| K-661 | (yalnız `06`) Adres başına tek geçerli yönetici daveti; yönetici adresine davet yok | AdminInvitation · *bağlantı kaydı* · Admin | davetin adresi ve hâli (yenisi öncekini geçersiz kılar); yönetici e-postasının tekilliği — → `02 §10.2.6`, §10.2.5 (§1.1.8) | eşlendi |
| K-663 | Siparişte IBAN — havale IBAN'ı siparişe donar; ayardaki değişiklik yalnız yeni siparişlere | Order · *firma ayarları* | siparişin donmuş havale IBAN'ı; ayarın havale IBAN'ı — → `02 §3.21.5`, §3.21.6, §3.23.3 (§1.1.5) | eşlendi |
| K-665 | Beyan kaydında adres — cayma ve ayıp talebinin beyanına güncel iade adresi yazılır, sonradan değişmez | Withdrawal · DefectClaim · *firma ayarları* | beyana donan iade adresi; ayarın iade adresi — → `02 §3.1.7`, §7.3.5 (§1.1.5, §1.1.8) | eşlendi |
| K-671 | (yalnız `06`) Hizmet tamamlama işaretinin ve S2'nin koşulu — ödeme Bekliyor ya da Başarısız değilken | OrderItem · Order | hizmetin "tamamlandı" işareti; ödeme ekseninin değeri (koşul) — → `02 §3.20.9`, §5.4 S2 (§1.1.5, §1.1.6) | eşlendi |
| K-672 | Kapanış kaydı — Alındı ya da Hazırlanıyor'da son açık kalem cayma ya da çıkarmayla kapanırsa sipariş kendiliğinden İptal edildi'ye kapanır | Order · *geçiş kaydı* | kapanışın geçişi (S3, S4) ve sebebi kayıtta — → `02 §5.6.6`, §5.4 S3, S4 (§1.1.6) | eşlendi |
| K-675 | (yalnız `06`) Panel işaretleri durum değildir, koşuldan türer ve koşulla kalkar; "e-posta düzeltildi" e-posta geçmişiyle imha | veri doğurmaz — türetilir · OrderEmailHistory · *geri ödeme kaydı* | işaretler saklanmaz — doğuran kayıtlardan okunur (gönderim sonucu, sorgu, geri ödeme kaydının hâli, kalem kayıtları, e-posta geçmişi) — → `02 §6.1.1`, §6.1.2, §7.4.1, §10.4.11 (§1.1.7, §1.1.8); ayrı alan olarak saklanmaz — K-1015; sözlükteki iki işaret K-940'ın satırında | eşlendi |
| K-676 | (yalnız `06`) Kalem başına tek ayıp talebi kaydı; çözülmüş talep yeniden açılır | DefectClaim | kalem başına tekil (kısıt); yeniden açılma — → `02 §5.10` m2, m3 (§1.1.6) | eşlendi |
| K-679 | (yalnız `06`) Siparişte en fazla bir kupon kodu | Order · Coupon | siparişin kupon bağı (en çok bir) — → `02 §3.10.1` (§1.1.5) | eşlendi |
| K-680 | (yalnız `06`) Siparişe donan adres ve adres defterine isteğe bağlı kayıt | Order · AddressBook | `ShippingAddress`, `BillingAddress` (donmuş); defter kaydı yalnız üyenin seçimiyle — → `02 §3.14.1` (§1.1.5) | eşlendi |
| K-685 | (yalnız `06`) Havale hatırlatması ödeme süresi başına bir kez; düzeltmede yeniden başlayan sürede bir kez daha (Z-9) | Order · *gönderim kaydı* | hatırlatmanın anı (sürenin yeniden başlamasıyla yeniden yazılır — K-895); B-3'ün gönderim satırı — → `02 §3.21.6`, §4.2 Z-9 (§1.1.5, §1.1.6) | eşlendi |
| K-686 | (yalnız `06`) Yöneticinin açtığı IBAN isteği — havale hattında IBAN'ı olmayan geri ödemede; "IBAN bekleniyor" listesi, iz satırı | RefundIbanRequest · *gönderim kaydı* · AuditLog | isteğin kalem bağı, tarihi, açan yönetici; B-14; iz satırı — → `02 §7.4.5` (§1.1.8) | eşlendi |
| K-695 | (yalnız `06`) Şifre değişikliğinde öteki oturumlar kapanır ve öteki tarayıcıların tanınan tarayıcı işaretleri düşer | Session · RecognizedBrowserMark | oturum ve işaret kayıtlarının hesap bağı (değiştiren oturum hariç silinir — imha değil, K-984) — → `02 §3.13.10`, §8.2.6 (§1.1.5, §1.1.8) | eşlendi |
| K-696 | (yalnız `06`) Google ile ilk girişte doğrulanmamış bekleyen kayıt silinir, hesap şifresiz ve Google'ın adıyla doğar | Customer · SocialLogin · *imha kaydı* | bekleyen kaydın silinmesi (imha kaydına — K-708); şifresiz hesap; Google bağı — → `02 §3.13.7`, §6.5.16 (§1.1.5, §1.1.7) | eşlendi |
| K-701 | (yalnız `06`) `03` atıflarının okunuşu — önsüz `§` dizideki öneki devralır | veri doğurmaz — yazım konvansiyonu | — (matrisin atıf biçimi K-669 ve K-701'in okunuşudur — §1.1'in girişi; VM0-06) | veri doğurmaz |
| K-707 | Talebin tipi saklamayı belirler — "Sipariş hakkında" tipi uzun süre (K-951 on yıla çevirdi), öteki tipler iki yıl | ContactRequest | `ContactTopic` süreyi seçer; son kapatılış anı (başlangıç değeri) — → `02 §3.32.4`, §4.2 Z-33, §11.2 P-40 (§1.1.5, §1.1.6, §1.1.8) | eşlendi |
| K-708 | Kayıt türleri — periyodik imha dışındaki anlık silmeler de imha kaydına: hesap, bekleyen kayıt, IBAN | *imha kaydı* | tarih, kayıt türü (anlık silmeler dahil), sayı — kişisel veri yok; değerler VM5-05 — → `02 §4.2` Z-44, §12.2.7 (§1.1.6, §1.1.8) | eşlendi |
| K-713 | Kalemin alanı — indirme hakkı (P-8) sipariş anında kaleme donar; yenileme sıfırlar | OrderItem | donmuş indirme hakkı ve sayacı — → `02 §3.12.5`, §3.23.1 (§1.1.5) | eşlendi |

**Devir satırları — Aşama 3 (14)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| K-736 | Yönetici hesabının ad alanı — zorunlu, sahibi yazar, ilk yöneticininki kurulumda; müşteriye görünmez | Admin | yöneticinin adı — → `02 §10.2.1`, §10.2.4 (§1.1.8); park A3-4 | eşlendi |
| K-787 | Kalem kayıtlarında işlem görmüş adedin tutulması — iptal, cayma ve iadede adet seçilir | Cancellation · Withdrawal · Return · OrderItem | kaydın adedi; kalemin adedi — → `02 §5.8`, §7.1.6 (§1.1.6, §1.1.8); park A3-1 | eşlendi |
| K-788 | İşlem başına ayrılan kupon payı ve KDV'nin tutulması — adetle orantılı, aşağı yuvarlanır; son işlem kalanı alır | Cancellation · ItemRemoval · LateDeliveryTermination · Withdrawal · OrderItem | işlemin ayırdığı kupon payı ve KDV; kalemin donmuş payı ve KDV'si — → `02 §7.1.7` (§1.1.8); park A3-2 | eşlendi |
| K-790 | Kalem kayıtlarında adet, açık adedin okunuşu, aynı adedi kullanan eşzamanlı işlemin önlenmesi — yeni durum yok | Cancellation · ItemRemoval · LateDeliveryTermination · Withdrawal · Return · ReturnRejection · NotReturnedClosure · DefectClaim · Order | kayıtların adedi; açık adet kayıtlardan okunur; siparişin satır kilidi ve sürümü (K-874) — → `02 §7.1.6`, §5.6.6 (§1.1.6, §1.1.8); park A3-1 | eşlendi |
| K-791 | Sayaçların adetle dönüşü — stok ve kontenjan işleme giren adet kadar; kupon hakkı yalnız bütün adetler kapanınca | Variant · StockReservation · Coupon | dönen adet; dönüşün okunduğu kayıt — → `02 §3.6.6`, §7.2.6, §7.4.8 (§1.1.5, §1.1.8); park A3-3 | eşlendi |
| K-793 | Ayıp talebinde adet — kalemin ayıplı adedi; üst sınır talebe açık adet (K-830) | DefectClaim | ayıplı adet — → `02 §7.5.2`, §5.8 (§1.1.6, §1.1.8); park A3-1 | eşlendi |
| K-794 | Kalem kayıtlarında firma işlemlerinin adedi — firma iptali, teslim alma, iade reddi, "mal dönmedi" adetle | Cancellation · Return · ReturnRejection · NotReturnedClosure | kaydın adedi; parça parça ulaşan iade ayrı kayıt — → `02 §7.2.4`, §7.4.7, §10.4.9 (§1.1.8); park A3-1 | eşlendi |
| K-800 | Araya giren kaydın karşılaştırılan hâli — kayıt varyantında iki yol: güncel hâli aç, yine de kaydet | Product · Coupon · Content · *firma ayarları* | düzenlenebilir kaydın sürüm değeri (K-873; bölümleri ayrı kaydedilen ekranda bölüm başına) — → `02 §3.31.1` (§1.1.5); park A3-5, A4-2 | eşlendi |
| K-804 | Gönderim kaydının siparişe bağlanması — sipariş ayrıntısının "E-posta geçmişi" bildirimleri ve e-posta düzeltmelerini gösterir | *gönderim kaydı* · OrderEmailHistory · Order | gönderim satırının sipariş bağı, bildirimin türü, tarihi, alıcısı, sonucu; yeniden gönderim ayrı satır — → `02 §4.2` Z-43, §10.4.11 (§1.1.6, §1.1.8); park A3-6 | eşlendi |
| K-806 | Kupon kodunun tekilliği — büyük-küçük harf ayrımsız eşleşir; alanlar düzenlenir, yeni siparişlere işler; kupon silinmez | Coupon | kod (harf duyarsız tekil — K-1016); kupon silinmez — → `02 §3.10.5` (§1.1.5); park A3-7 | eşlendi |
| K-811 | Doğrulanmamış kaydın tutulması ve aramada bulunması — e-postayla tam eşleşme, harf ayrımsız | Customer | doğrulama hâli (doğrulanmamış kayıt da müşteri tablosunda); e-postanın harf duyarsız araması — → `02 §3.15.5`, §3.13.4 (§1.1.5); park A3-8 | eşlendi |
| K-817 | İade adresinin alanları — alıcı adı, il (kapalı liste), ilçe, açık adres zorunlu, telefon isteğe bağlı; firma IBAN'ının denetimi | *firma ayarları* · *il listesi* | iade adresi alanları; havale IBAN'ı (girişte biçim denetimi) — → `02 §3.1.7`, §3.21.5 (§1.1.5); park A3-9 | eşlendi |
| K-825 | Telefon ve IBAN alanının saklanan biçimi — Türkiye numarası on hane, IBAN boşluksuz denetlenir (park gerekmez) | Order · AddressBook · Withdrawal · *firma ayarları* · Company | teslimat telefonu, adres defterinin telefonu, firma telefonu; müşterinin ve firmanın IBAN'ı — saklanan biçim K-1013 — → `02 §3.14.6`, §7.4.5 (§1.1.5, §1.1.8) | eşlendi |
| K-830 | (yalnız `06`) Ayıplı adedin üst sınırı kalemin talebe açık adedidir — teslim işaretine bağlı değil | DefectClaim · OrderItem | ayıplı adet; açık adet kayıtlardan okunur — → `02 §7.5.2` (§1.1.8); park A3-1 (K-793'ün satırı değişmedi) | eşlendi |

**Devir satırları — Aşama 4 (45 — K-879 2b'de eklendi)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| K-870 | Zaman, tarih ve para alanlarının türleri — an UTC damga, tarih tarih, para ondalıklı | veri doğurmaz — mimari (`05`) | — (her alanın türü K-1013; veritabanı PostgreSQL 18 — `05 §7.1`, §7.2) — → `02 §4.1.1` (§1.1.6); park A4-1 | veri doğurmaz |
| K-873 | Düzenlenebilir kayıtların sürüm alanı; ürünün sürümünün varyantları kapsaması; bölüm başına sürüm | Product · Variant · Coupon · Content · *firma ayarları* | sürüm değeri (ürününki varyant tablosunu kapsar; ayarlarda bölüm başına) — → `02 §3.31.1` (§1.1.5); park A4-2; biçimi K-1014 | eşlendi |
| K-874 | Siparişin sürüm alanı ve sürümü artıran değişiklikler; işlem siparişin satır kilidiyle | Order | siparişin sürüm değeri — kalemlerin, kalem kayıtlarının ve müşteri girdilerinin (IBAN dahil) değişikliğinde artar — → `02 §3.31.2` (§1.1.5); park A4-2, A3-1 | eşlendi |
| K-875 | İmha kaydının tablosu; sürelerin başlangıç değerleri — gecelik tek iş, süre veriden hesaplanır | *imha kaydı* · Order · ContactRequest · *gönderim kaydı* · *giriş kaydı* · AuditLog | imha kaydı (tarih, tür, sayı — yalnız eklenen); her kaydın başlangıç değeri alanı — → `02 §4.2` Z-44, §12.2.7 (§1.1.6, §1.1.8); park A4-3; başlangıç alanları VM9-01 | eşlendi |
| K-876 | Fiziksel silme; siparişin kişisel alanları boşaltılır; üretilen belgelerin saklanma biçimi ve imha kapsamı | Order · PreInformationForm · DistanceSalesContract · LegalTextVersion | silme fiziksel — "silindi" işaretli satır yok; boşaltılan alanlar; belgenin kopya mı yeniden üretim mi olduğu VM4-11'in kararı — → `02 §12.2.7`, §3.24.3 (§1.1.5, §1.1.8); park A4-3 | eşlendi |
| K-877 | İz tablosunun yetkileri ve imha fonksiyonu — uygulama yalnız ekler ve okur | AuditLog | iz satırı; silme yalnız veritabanındaki imha fonksiyonuyla (Z-30) — → `02 §8.5.2`, §10.3.1 (§1.1.8); park A4-3 | eşlendi |
| K-879 | Yedeğin kapsamı — çatının şifreleme anahtar halkası veritabanında tutulur ve onunla birlikte yedeklenir; yedek sunucunun dışında, şifreli, otuz gün — etki sütununa `06` 2b'de eklendi (K-1009) | *anahtar halkası* · veri doğurmaz — mimari (`05`) | anahtar halkasının tablosu VM4-14; yedeğin yeri, şifrelenmesi ve saklanması altyapıdadır (`05 §7.6`) — → park A4-3 | eşlendi |
| K-883 | Dosya kaydı; varyanta ve kaleme bağı — her yükleme yeni kayıt, o anki dosya en son bağlanan; tamamlanmayan parçalı yükleme bir gün sonra silinir | DigitalFile · Variant · OrderItem | dosya kaydı, varyant bağı ve bağlanma anı; kalemin indirebildiği dosya — → `02 §3.12.7` (§1.1.5); park A4-4 | eşlendi |
| K-884 | Kalemin indirme sayacı ve indirme bağlantısı — hak düğmede ve gönderim başlayınca bir kez düşer; bir saatlik bağlantı | OrderItem | indirme sayacı; kısa ömürlü indirme bağlantısı (hak koşullu güncellemeyle — K-980) — → `02 §3.12.5` (§1.1.5); park A4-4 | eşlendi |
| K-886 | Ürünün arama biçimi alanı — küçük harfli ve sadeleşmiş ad, trigram dizini, içerir eşleşmesi | Product | adın arama biçimi (ad değiştikçe yenilenir); trigram dizini (VM7-01) — → `02 §3.5.1` (§1.1.5); park A4-5 | eşlendi |
| K-889 | Geçiş kaydı — her durum geçişi yalnız eklenen satır: kim, ne zaman, hangi geçiş, sebep; durum alanlarına tek yazıcı | *geçiş kaydı* · Order | geçişin yapanı (müşteri, yönetici, sistem), anı, kimliği (S, Ö, kalem kaydı), sebebi — → `02 §3.34.8`, §5.4 (§1.1.5, §1.1.6); park A4-6, A3-1 | eşlendi |
| K-890 | Ayrılmış adet alanları ve koşullu güncelleme — onay tek işlemde: kapı, parmak izi ve sürümler, ayırma, numara | StockReservation · Variant · Coupon · Order | ayrılmış adet (yalnız kalan yetiyorsa yazılır); siparişe donan sürümler; numara ve erişim anahtarı — → `02 §3.17.3`, §4.3 m1 (§1.1.5, §1.1.6); park A4-6 | eşlendi |
| K-891 | İş kuyruğu tablosu — dış yan etkiler geçişle aynı işlemde kuyruğa; satır kişisel veri taşımaz, kayda kimlikle işaret eder | *iş kuyruğu* | iş satırı: tür, kayda işaret, deneme sayısı, sonraki deneme anı, sonuç — → `02 §4.3` m7 (§1.1.6); park A4-7 | eşlendi |
| K-892 | Gönderim kaydının alanları — her e-posta bir satır ve kuyruğu odur; gövde saklanmaz; yeniden gönderim yeni satır | *gönderim kaydı* | tür, alıcı, ilgili kayıt, olayın değerleri, deneme sayısı, sonraki deneme anı, sonuç — → `02 §9.1.6`, §4.2 Z-26, Z-34, Z-43 (§1.1.6, §1.1.8); park A4-7 | eşlendi |
| K-893 | Onay anahtarı; gelen bildirim kaydı ve işlem kimliğinin tekilliği — bildirim önce kaydedilir, sonra işlenir | Order · *gelen bildirim kaydı* · *ödeme kaydı* | onay anahtarı (siparişte tekil); bildirimin kimliği, olay türü, sonuç (tekil — K-970); ödeme işlem kimliği — → `02 §6.2.16`, §3.31.2 (§1.1.5, §1.1.7); park A4-7 | eşlendi |
| K-894 | Geri ödeme kaydının hâlleri ve istek kimliği — karta para gönderen her istekten önce yazılır | *geri ödeme kaydı* | tutar, kalem ve adetler, hâl (istek gönderiliyor, sonucu bekleniyor, başarılı, gerçekleşmedi — K-940); kaydın kimliği tekrar anahtarı — → `02 §7.2.8` (§1.1.8); park A4-7 | eşlendi |
| K-895 | Kayıtlardaki zaman alanları — zamanlanmış işin zamanı kendi kaydında; zamanlayıcı dakikada bir sorar; tek çalışma kilidi | Order · Withdrawal · *gönderim kaydı* · *nabız ve kesinti kaydı* | ödeme süresinin bittiği an, hatırlatmanın anı, IBAN'ın silineceği gün, sonraki deneme anı; gecelik işin çalışma anları — → `02 §4.3` m7, §4.2 Z-36 (§1.1.6); park A4-8 | eşlendi |
| K-898 | Nabız ve kesinti kaydı; ertelenen son an — iki dakikayı aşan boşluk kesintidir; dönüşte havale iptali ilk iş gününe | *nabız ve kesinti kaydı* · Order | nabız; kesintinin başlangıç ve bitişi; ertelenen son an (sipariş sayfasında görünür) — → `02 §3.34.7`, §6.7.17 (§1.1.5, §1.1.7); park A4-8 | eşlendi |
| K-899 | Oturum kaydı; iki hesap türünün tabloları — oturum sunucuda, kapatmak kaydı silmektir | Session · Customer · Admin | anahtarın özeti, hesap, hesap türü, kısa ya da uzun, son işlem anı; iki ayrı hesap tablosu — → `02 §3.13.12`, §10.2.2 (§1.1.5, §1.1.8); park A4-9 | eşlendi |
| K-901 | Deneme kaydı; sepetin birleşmesinde sayaç — L-7 ve L-8 siparişlerden sayılır | *deneme kaydı* · Order · Cart | limit, eksen, değer, an; siparişin IP'si ve hesabı; sepet kimliği — → `02 §8.2.2` (§1.1.8); park A4-9 | eşlendi |
| K-903 | Bağlantı kaydı; erişim anahtarı ve numaranın tekilliği — hesap bağlantıları yalnız özetle | *bağlantı kaydı* · Order | bağlantının türü, hesabı, özeti, oluşma anı, ömrün sonu, kullanıldığı an, yerine yenisi geçtiği an; siparişin erişim anahtarı ve numarası (tekil) — → `02 §3.22.1`, §3.22.2, §3.13.5 (§1.1.5); park A4-9 | eşlendi |
| K-906 | Tanınan tarayıcı kaydı; misafir sepetinin son erişim anı — sepet 400 gün açılmazsa silinir | RecognizedBrowserMark · Cart | hesap başına işaret kaydı; misafir sepetinin son erişim anı — → `02 §4.2` Z-6, Z-46 (§1.1.6); park A4-9 | eşlendi |
| K-913 | Kanal hâli kaydı; gönderim denemesinin sonuç sınıfı — iki uyarı saklanmaz, gönderim kaydından okunur | *kanal hâli kaydı* · *gönderim kaydı* · veri doğurmaz — türetilir | tek satır: son başarılı gönderimin anı, art arda kanal hatası sayısı, dizinin ilk hatasının anı; denemenin sonucu üç sınıf (VM5-03) — → `02 §6.1.1`, §9.1.6 (§1.1.7, §1.1.8); park A4-10 | eşlendi |
| K-918 | Yedek aracının durum kaydı — gecelik çalışmanın sonucu veritabanında; durum ucunun koşulları | *yedeğin durum kaydı* · veri doğurmaz — mimari (`05`) | gecelik tam yedeğin sonucu ve anı (durum ucunun "son başarılı yedek" koşulu bundan okunur); uçlar ve dış kontrol mimaridedir — → park A4-10 (`02`'de satırı yok: yedeğin kendisi mimaridir — `02 §3.34.8`, §1.1.5) | eşlendi |
| K-937 | Yedeğin durum kaydı — tam yedeğin sonucunu veritabanı konteynerindeki zamanlayıcı yerel soketle yazar; dosya yedeğininki durum dosyasında | *yedeğin durum kaydı* · veri doğurmaz — mimari (`05`) | tam yedeğin sonucu; dosya yedeğinin sonucu veritabanında değil — → park A4-10 | eşlendi |
| K-940 | Geri ödeme kaydının hâlleri — sonucu bilinmeyen kart iadesi "geri ödeme sonucu bekleniyor"; firmanın "gerçekleşmedi" işareti sorguyu durdurur; geç sonuç işaretli kalır | *geri ödeme kaydı* · Order · *gelen bildirim kaydı* · AuditLog | kaydın dört hâli; `RefundOutcomePendingMark`, `RefundFailedMark` (hâlle aynı işlemde); geç sonucun işareti; firmanın işaretinin iz satırı — → `02 §7.2.8`, §1.2 `RefundOutcomePendingMark` (§1.1.4, §1.1.8); park A4-7 | eşlendi |
| K-941 | Gelen bildirim kaydı, uç, imzalı makine uçları — imzası tutmayan istek yazılmaz; kişisel alan yok | *gelen bildirim kaydı* | sağlayıcının referansı, siparişin referansı, tutar, sonuç, anlar; tutmayan ya da tanınmayan satırın işareti; saklaması K-954 — → `02 §6.2.16` (§1.1.7); park A4-7 | eşlendi |
| K-944 | İmha — sunucunun kayıtları altı ay, kendi döngüsüyle silinir; silinen gün imha kaydına "döngüyle silindi" satırıyla, tarih aralığıyla | *imha kaydı* · veri doğurmaz — mimari (`05`) | imha kaydının tarih aralıklı satırı (sayı yerine); sunucunun kayıtları dosyadadır — → `02 §8.5.1`, §4.2 Z-40 (§1.1.6, §1.1.8); park A4-3 | eşlendi |
| K-945 | Oturum ve bağlantı kayıtları — bağlantı kaydı hesabıyla yaşar, gecelik temizlik yalnız süresi dolan oturumu siler | *bağlantı kaydı* · Session | bağlantı kaydının ömrü hesaba bağlı; "kullanılmış" ve "yerine yenisi geçmiş" hâlleri korunur — → `02 §3.13.5` (§1.1.5); park A4-9 | eşlendi |
| K-946 | İş kuyruğu, zaman, uyarılar — varsayılan beş yeniden deneme, "başarısız" hâli; gecelik işin son başarılı çalışma anı | *iş kuyruğu* · *nabız ve kesinti kaydı* | iş satırının deneme sayısı, sonraki deneme anı, "başarısız" hâli; gecelik işin son başarılı çalışması ve son denemesinin anı — → `02 §4.3` m7 (§1.1.6); park A4-7, A4-8 | eşlendi |
| K-948 | Dosya kaydı — bağsız görsel imhada silinir; kalem görselin bağını dondurur; kaydedilmeden bırakılan yükleme temizlikte | Image · OrderItem · Product · Content | görselin ürüne, içerik kaydına ve kaleme bağı; kaynak kopya ve türevler — → `02 §3.23.1`, §3.11 (§1.1.5); park A4-4 | eşlendi |
| K-951 | Gönderim kaydının iki kolu, siparişin ödemesi alınmış sayılmasının okunduğu alan, boşaltılan alanlar, talebin tipe göre süresi — on yıl, karma (6563 m.11/3) | *gönderim kaydı* · Order · *ödeme kaydı* · ContactRequest · *imha kaydı* | gönderim satırının sipariş bağı, alıcısı ve olayın değerleri (üç yılda boşalan kol); "ödemesi alınmış" ödeme kaydından okunur; Z-41'in kişisel verisiz kaydı; `ContactTopic` süreyi seçer — → `02 §4.2` Z-28, Z-33, Z-41, Z-43, §12.2.7 (§1.1.6, §1.1.8); park A4-11; alan listesi VM9-02 | eşlendi |
| K-954 | Gelen bildirim kaydının saklanması — siparişe bağlanmayan satır on yıl; siparişe bağlanan siparişle silinir | *gelen bildirim kaydı* · *imha kaydı* | satırın sipariş bağı (silmeyi seçer); on yıllık başlangıç değeri — → `02 §6.2.16` (§1.1.7); park A4-7 | eşlendi |
| K-963 | Yedeğin durum kaydı — sunucu dışı tam yedek; durum ucunun koşulu sunucunun dışındaki depoya alınan son başarılı tam yedeğe bakar | *yedeğin durum kaydı* · veri doğurmaz — mimari (`05`) | son başarılı sunucu dışı tam yedeğin anı (deposuyla); prova kaydı `DEPLOY_RUNBOOK`'tadır — → park A4-10 | eşlendi |
| K-970 | İş kuyruğu, gönderim kaydı, gelen bildirim ve geri ödeme kaydı — ödemenin ve bildirimin tekilliği, geri ödeme kaydının kesinleşmesi, sorgunun yinelenmesi, onay anahtarının yeri | *ödeme kaydı* · *gelen bildirim kaydı* · *geri ödeme kaydı* · *iş kuyruğu* · Order | ödeme işlem kimliği (ödeme kaydında tekil); bildirimin kimliği ya da işlem kimliği + olay türü + sonuç (tekil); geri ödeme kaydının kimliği dış isteğin anahtarı; sorgunun iş satırı; onay anahtarı — → `02 §6.2.16`, §7.2.8, §3.17.7 (§1.1.5, §1.1.7, §1.1.8); park A4-7 | eşlendi |
| K-971 | Oturum, deneme, bağlantı ve tanınan tarayıcı kayıtları — oturumların silinmesiyle girişin sırası, yeniden doğrulama izni, bağlantının tüketimi, davetin göndereni | Session · *bağlantı kaydı* · AdminInvitation · Customer · Admin | yeniden doğrulamanın tek işlemlik izni (forma konan değere bağlı); bağlantının koşullu tüketimi; davetin göndereni; şifre özeti — → `02 §8.1.5`, §10.2.2, §10.2.7 (§1.1.8); park A4-9 | eşlendi |
| K-972 | Oturum, deneme, bağlantı ve tanınan tarayıcı kayıtları — başarısızlıkta sayılan limitte satır denemeden önce kesinleşir, başarıda silinir | *deneme kaydı* | deneme satırının yazılma ve silinme sırası; L-7 ve L-8 siparişlerden — → `02 §8.2.2`, §8.2.3 (§1.1.8); park A4-9 | eşlendi |
| K-975 | İmha — iz satırı yöneticinin adını, e-postasını ve kaydın kimliğini değer olarak taşır, basamaklanan bağ yok; imha kaydı parçanın işleminde; nabız ve taşıma tek işlemde | AuditLog · *imha kaydı* · *nabız ve kesinti kaydı* · Order · *anahtar halkası* | iz satırının değer alanları (yabancı anahtar değil); imha kaydının parça satırı; kesinti kaydı ve ertelenen son an aynı işlemde; (4) eski bir ana dönüşte veri koruma anahtarı da geri gider — anahtar halkası veritabanındadır (`05 §7.6.3`; park A4-3 — 2b'de eklendi, K-1009) — → `02 §10.2.2`, §8.5.2 (§1.1.8); park A4-3 | eşlendi |
| K-977 | K-17 park satırları — Teknik Mimari'nin doğurduğu teknik kavramların ve kapalı listelerinin kod adını Veri Modeli verir | veri doğurmaz — yazım konvansiyonu | — (teknik kavramların kod adı `05`'teki adından — VM3-03, VM5-03; ürüne görünen kavram sözlüğe — GAP-12) — → `02 §1.1` m4 (§1.1.4); park A1-2 | veri doğurmaz |
| K-979 | İmha — gecelik temizliğin kişisel veri taşıyan geçici kayıtlarının (deneme, oturum, tanınan tarayıcı) silinmesi imha kaydına; istekteki sayım silmesi imha değil | *imha kaydı* · *deneme kaydı* · Session · RecognizedBrowserMark | imha kaydının kayıt türleri (geçici kayıtlar) ve sayısı — değerler VM5-05 — → `02 §12.2.7` (§1.1.8); park A4-3 | eşlendi |
| K-980 | Sürüm alanları; dosya kaydı ve indirme — stok alanı kendi koşullu güncellemesiyle, sistem hareketleri ürünün sürümünü artırmaz; indirme hakkı koşullu düşer; süpürme bir günden eski dosyada | Variant · Product · OrderItem · DigitalFile | stok ve kontenjan alanı (koşullu güncelleme: form değeri ve ayrılmış adet); ürünün sürümü; indirme sayacı; dosya kaydının oluşma anı — → `02 §3.6.7`, §3.12.5 (§1.1.5); park A4-2, A4-4 | eşlendi |
| K-982 | Oturum, deneme, bağlantı ve tanınan tarayıcı kayıtları — e-posta büyük-küçük harf ayrımı olmadan karşılaştırılır, girildiği gibi gösterilir; tekillik veritabanında | Customer · Admin · *deneme kaydı* · Order | e-posta (tekillik tür içinde, harf duyarsız — biçimi K-1016); deneme kaydının e-posta ekseni; siparişin iletişim e-postası (hesaba bağlanma) — → `02 §10.2.5` (§1.1.8); park A4-9 | eşlendi |
| K-984 | İmha — bekleyen e-posta değişikliğinin ömrü dolunca silinmesi ve misafir sepetinin gecelik silinmesi imha kaydına; istekte silinen oturum imha değil | *imha kaydı* · PendingEmailChange · Cart · Session | imha kaydının kayıt türleri (bekleyen değişiklik, misafir sepeti); bekleyen değişikliğin ömrü — → `02 §4.2` Z-44, §12.2.7 (§1.1.6, §1.1.8); park A4-3 | eşlendi |
| K-985 | İş kuyruğu, gönderim kaydı, gelen bildirim ve geri ödeme kaydı — kapatılan iş satırı tamamlanmış sayılır; dosya silen imha parçası satırını silmeden önce yazar | *iş kuyruğu* · *imha kaydı* | iş satırının "kapatıldı" sonucu; imha kaydı satırının yazılma sırası — → park A4-7, A4-3 (`02`'de satırı yok: iş satırının hâli teknik kavramdır — VM3-03) | eşlendi |
| K-988 | Terim sözlüğü — K-977'nin tanımının hizalanması: adı ürün kuralında geçen teknik kayıtlar da teknik kavram; geri ödeme kaydının hâlleri teknik kalır | veri doğurmaz — yazım konvansiyonu · *geri ödeme kaydı* | geri ödeme kaydının dört hâlinin kod adı `05`'teki adından (VM5-03); panel etiketi `04`'te — → `02 §1.1` m4 (§1.1.4); park A1-2 | eşlendi |

**Park maddeleri (23)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| park A1-1 | K-06 · K-97 · Aktörler ve yetki derinliği — dört aktör; yönetim tek rol, çoklu kullanıcı; rol ve izin modeli buna göre | Admin · Customer · Order · yoktur — kapsam dışı | rol ve izin alanı yok; misafir alıcının kaydı siparişin iletişim alanlarıdır; kiracı alanı yok — → `02 §1.3` m1, m3, m4 (§1.1.4); VM0-07 | eşlendi |
| park A1-2 | K-17 · K-531 · K-977 · K-988 · Terim sözlüğü — sözlük birebir devralınır; kapalı liste değerlerinin ve teknik kavramların kod adlarını bu doküman verir | veri doğurmaz — yazım konvansiyonu | — (adlar VM0-06; teknik kavramlar VM3-03; kapalı listeler ve teknik listeler VM5-03) — → `02 §1.1` m3, m4, m5 (§1.1.4) | veri doğurmaz |
| park A3-1 | K-787 · K-790 · K-793 · K-794 · Kalemde işlem görmüş adet — kalem kayıtları adedi taşır; açık adet kayıtlardan; aynı adedi kullanan iki işlem siparişin satır kilidiyle | Cancellation · ItemRemoval · LateDeliveryTermination · Withdrawal · Return · ReturnRejection · NotReturnedClosure · DefectClaim · OrderItem · Order | kayıtların adedi; açık adet saklanmaz (K-1015), alanlar VM4-07'nin; siparişin sürümü (K-874) — → `02 §5.8` metin, §7.1.6 (§1.1.6, §1.1.8); K-satırları K-787, K-790, K-793, K-794 | eşlendi |
| park A3-2 | K-788 · Adet başına ayrılan tutar — işlemin ayırdığı kupon payı ve KDV kayda geçer; son işlem kalanı alır | Cancellation · ItemRemoval · LateDeliveryTermination · Withdrawal · OrderItem | işlem başına pay ve KDV; yeri ve biçimi VM4-07 — → `02 §7.1.7` (§1.1.8); K-788 | eşlendi |
| park A3-3 | K-791 · Sayaçların adetle dönüşü — stok, kontenjan ve kupon hakkı; dönüşün hangi kayıttan türetildiği | Variant · StockReservation · Coupon · Cancellation · Return | dönüşün kaynağı kalem kayıtlarının adedi, kaydı yazan geçişte (K-1015) — VM4-04 — → `02 §7.2.6`, §7.4.8, §4.3 m3 (§1.1.6, §1.1.8); K-791 | eşlendi |
| park A3-4 | K-736 · Yönetici hesabının adı — zorunlu; davetli yazar, ilk yöneticininki kurulumda | Admin | yöneticinin adı — VM4-09 — → `02 §10.2.1`, §10.2.4 (§1.1.8); K-736 | eşlendi |
| park A3-5 | K-800 · Eşzamanlı düzenleme — açıldığı andaki hâlin kaydetmede karşılaştırılması; `05`'in mekanizmasıyla | Product · Variant · Coupon · Content · *firma ayarları* | sürüm değeri (K-873 — park A4-2 karşılar) — VM2-05, K-1014 — → `02 §3.31.1` (§1.1.5); K-800 | eşlendi |
| park A3-6 | K-804 · Siparişin e-posta geçmişi — bildirimler ve misafir e-posta düzeltmeleri; siparişin saklama kuralıyla imha; gönderim kaydının siparişe bağı | *gönderim kaydı* · OrderEmailHistory · Order | gönderim satırının sipariş bağı — VM4-13 — → `02 §4.2` Z-43, §10.4.11 (§1.1.6, §1.1.8); K-804 | eşlendi |
| park A3-7 | K-806 · Kupon kodunun tekilliği — harf ayrımsız eşleşir; kupon silinmez; tekilliğin veride korunuşu | Coupon | kod — harf duyarsız tekillik K-1016, VM4-04 — → `02 §3.10.5` (§1.1.5); K-806 | eşlendi |
| park A3-8 | K-811 · Doğrulanmamış kayıt — e-postayla aranır ve bulunur; tutuluşu ve aramada bulunuşu | Customer | doğrulama hâli; e-postanın harf duyarsız araması — VM4-09 — → `02 §3.13.4`, §3.15.5 (§1.1.5); K-811 | eşlendi |
| park A3-9 | K-817 · İade adresinin alanları — alıcı adı, il (kapalı liste), ilçe, açık adres zorunlu; telefon isteğe bağlı | *firma ayarları* · *il listesi* | iade adresinin alanları — VM4-01 — → `02 §3.1.7` (§1.1.5); K-817 | eşlendi |
| park A3-10 | `02 §3.8.5` · Ölçü birimlerinin listesi (FORM-16) — "ölçü birimi + net miktar" tek alan; listenin içeriği Fiyat Etiketi Yönetmeliği m.5'e göre | Product | ölçü birimi + net miktar; birim listesi enum'dur, değerleri `02 §3.8.5`'te — VM5-04, K-1018 (⚠) — → `02 §3.8.5` (§1.1.5); `04 7.2.4.13 g1` | eşlendi |
| park A4-1 | K-870 · Zamanın, tarihin ve paranın türleri — an UTC damga, tarih tarih, para ondalıklı; her alanın türü | veri doğurmaz — mimari (`05`) | — (türlerin ataması VM2-04, K-1013) — → `02 §4.1.1` (§1.1.6); K-870 | veri doğurmaz |
| park A4-2 | K-873 · K-874 · K-980 · Sürüm alanları — düzenlenebilir kayıtlar ve sipariş sürüm taşır; sistemin stok hareketleri ürünün sürümünü artırmaz; stok alanı kendi koşullu güncellemesiyle | Product · Variant · Coupon · Content · *firma ayarları* · Order | sürüm değeri ve artıran yazımların listesi — VM2-05, K-1014; stok ve kontenjan alanının koşullu güncellemesi — → `02 §3.31.1`, §3.31.2, §3.6.7 (§1.1.5); K-873, K-874, K-980 | eşlendi |
| park A4-3 | K-875 · K-876 · K-877 · K-879 · K-944 · K-975 · K-979 · K-984 · İmha ve anahtar halkası — süre başlangıç değerinden; fiziksel silme; siparişte boşaltma ve bağlı kayıtlar; imha kaydı ve iz tablosu; geçici kayıtlar; üretilen belgelerin saklanma biçimi; anahtar halkası veritabanında, onunla yedeklenir (K-879, K-975 (4) — 2b, K-1009) | *imha kaydı* · AuditLog · Order · OrderEmailHistory · OrderAddressHistory · DefectClaim · PreInformationForm · DistanceSalesContract · LegalTextVersion · *deneme kaydı* · Session · RecognizedBrowserMark · Cart · PendingEmailChange · *anahtar halkası* | başlangıç değeri alanları VM9-01; bağlı kayıtların alan listesi VM9-02; imha kaydı ve iz tablosu VM4-14; belgenin kopya mı yeniden üretim mi olduğu VM4-11 (W ⚠) ; anahtar halkasının tablosu VM4-14 — → `02 §4.2` Z-28…Z-44, §12.2.7 (§1.1.6, §1.1.8); K-satırları | eşlendi |
| park A4-4 | K-883 · K-884 · K-948 · K-980 · Dosya kaydı ve indirme — her yükleme yeni kayıt; indirme sayacı ve bağlantısı; görsel ve bağları; süpürme | DigitalFile · Image · Variant · OrderItem · Product · Content | dosya kaydı ve varyant bağı; kalemin indirme sayacı ve bağlantısı; görselin üç bağı — VM4-15 — → `02 §3.12.5`, §3.12.7, §3.23.1 (§1.1.5); K-satırları | eşlendi |
| park A4-5 | K-886 · Ürün adının arama biçimi — ayrı alan, trigram dizini | Product | adın arama biçimi; trigram dizini — VM4-02, VM7-01 — → `02 §3.5.1` (§1.1.5); K-886 | eşlendi |
| park A4-6 | K-889 · K-890 · Geçiş kaydı ve ayırma — yalnız eklenen geçiş kaydı; onay koşullu güncellemeyle ayırır | *geçiş kaydı* · StockReservation · Variant · Coupon | geçiş kaydının alanları VM4-06; ayrılmış adet VM4-04 — → `02 §3.34.8`, §3.17.3 (§1.1.5); K-889, K-890 | eşlendi |
| park A4-7 | K-891 · K-892 · K-893 · K-894 · K-940 · K-941 · K-946 · K-954 · K-970 · K-985 · İş kuyruğu, gönderim kaydı, gelen bildirim ve geri ödeme kaydı — tabloların ve hâllerin tanımı; onay anahtarı; ödeme kaydında işlem kimliği | *iş kuyruğu* · *gönderim kaydı* · *gelen bildirim kaydı* · *geri ödeme kaydı* · *ödeme kaydı* · Order | iş satırı ve hâlleri; gönderim satırı; bildirim satırı ve tekilliği; geri ödeme kaydının hâlleri; onay anahtarı — VM4-13, VM4-08, VM4-06; hâllerin kod adları VM5-03 — → `02 §9.1.6`, §6.2.16, §7.2.8 (§1.1.7, §1.1.8); K-satırları | eşlendi |
| park A4-8 | K-895 · K-898 · K-946 · Zaman alanları, nabız ve kesinti kaydı — zamanlanmış işin zamanı kendi kaydında; gecelik işin anları; ertelenen son an | Order · Withdrawal · *gönderim kaydı* · *nabız ve kesinti kaydı* | zaman alanları VM4-06; nabız, kesinti ve gecelik işin anları VM4-14 — → `02 §4.3` m7, §3.34.7 (§1.1.5, §1.1.6); K-satırları | eşlendi |
| park A4-9 | K-899 · K-901 · K-903 · K-906 · K-945 · K-971 · K-972 · K-982 · Oturum, deneme, bağlantı ve tanınan tarayıcı kayıtları — iki hesap tablosu, e-posta tekilliği, giriş kaydı, misafir sepeti, erişim anahtarı | Session · Customer · Admin · *deneme kaydı* · *bağlantı kaydı* · RecognizedBrowserMark · *giriş kaydı* · Cart · Order | kayıtların tanımı VM4-09; e-posta tekilliğinin veride korunuşu K-1016; misafir sepetinin son erişim anı VM4-05; erişim anahtarı ve numara VM4-06 — → `02 §3.13.12`, §3.13.20, §8.2.2, §10.2.5 (§1.1.5, §1.1.8); K-satırları | eşlendi |
| park A4-10 | K-913 · K-918 · K-937 · K-963 · Kanal hâli ve yedeğin durum kaydı — tek satırlık kanal hâli; sonuç üç sınıf; tam yedeğin sonucu veritabanındaki durum kaydında, dosya yedeğininki dosyada | *kanal hâli kaydı* · *gönderim kaydı* · *yedeğin durum kaydı* · veri doğurmaz — mimari (`05`) | kanal hâli kaydı VM4-13; yedeğin durum kaydı VM4-14 — §1.1.2'nin 70. adayı burada eşlenir — → `02 §6.1.1` (§1.1.7); K-satırları | eşlendi |
| park A4-11 | K-951 · Elektronik ticaret kayıtlarının on yıllık saklaması — gönderim kaydının iki kolu; ödemesi alınmamış siparişin kişisel verisiz kaydı; talebin süresi konu tipinden | *gönderim kaydı* · Order · *ödeme kaydı* · ContactRequest | gönderim kaydının siparişe bağı, "ödemesi alınmış"ın okunduğu alan, boşaltılan alanlar — VM9-02, VM4-13; talebin tipi VM4-12 — → `02 §4.2` Z-33, Z-41, Z-43 (§1.1.6); K-951 | eşlendi |

**Gövde cümleleri (23)**

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|
| `02 §1.1 g1` | "`06`, `07` ve `09` bu sözlüğü birebir devralır, isim icat etmez" (Aşama 1 dizini) | veri doğurmaz — yazım konvansiyonu | — (VM0-06) — → `02 §1.1` m3 (§1.1.4); park A1-2 | veri doğurmaz |
| `02 §1.1 g2` | Sözlük ürünün terimlerini taşır; teknik kavramların kod adını `06` `05`'teki adından verir; ürüne görünen kavram sözlüğe girer (K-977, K-988 — dizinden sonra eklendi) | veri doğurmaz — yazım konvansiyonu | — (teknik kavramlar VM3-03; ürüne görünen kavramlar sözlükte — GAP-12) — → `02 §1.1` m4 (§1.1.4); park A1-2 | veri doğurmaz |
| `02 §1.1 g3` | Kapalı listelerin değerlerinin kod adları `06`'da verilir — ilk kuralın tek istisnası (K-531; Aşama 1 dizini) | veri doğurmaz — yazım konvansiyonu | — (VM5-03) — → `02 §1.1` m5 (§1.1.4); park A1-2 | veri doğurmaz |
| `02 §1.1 g4` | Kuralın gerekçesi `03` ve `04`'ün `06`'dan önce yazılmasıdır (K-17; Aşama 1 dizini) | veri doğurmaz — yazım konvansiyonu | — — → `02 §1.1` metin (§1.1.4) | veri doğurmaz |
| `02 §1.1 g5` | Bedeli: `06` yazılırken birkaç satırın revize edilmesi beklenir (K-17; Aşama 1 dizini) | veri doğurmaz — yazım konvansiyonu | — (revize gerekirse K-652'nin geri beslemesi) — → `02 §1.1` metin (§1.1.4) | veri doğurmaz |
| `02 §3.31 g1` | Sessiz üzerine yazma yoktur; mekanizması `05`/`06`'nın eşzamanlılık kararıdır (K-270; Aşama 1 dizini) | Product · Variant · Content · Coupon · *firma ayarları* | sürüm değeri (K-873) — K-1014 — → `02 §3.31.1` (§1.1.5); park A3-5, A4-2 | eşlendi |
| `02 §3.31 g2` | Siparişe yönelik işlemde mekanizma `05`/`06`'nın eşzamanlılık kararıdır (K-587; Aşama 1 dizini) | Order | siparişin sürüm değeri (K-874) — K-1014 — → `02 §3.31.2` (§1.1.5); park A4-2 | eşlendi |
| `02 §4.1 g1` | Depolamanın biçimi `05` ve `06`'nın işidir (K-335; Aşama 1 dizini) | veri doğurmaz — mimari (`05`) | anların türü — saat dilimli damga, UTC (K-870) — K-1013 — → `02 §4.1.1` (§1.1.6); park A4-1 | veri doğurmaz |
| `02 §5 g1` | Durum makinesi mantığı burada başlar, `03`'te akışa, `06`'da şemaya döner (bölümün "Ne yazılır" notu; Aşama 1 dizini) | veri doğurmaz — yazım konvansiyonu | — (durum setleri VM5-02) — → `02 §5` metin (§1.1.6) | veri doğurmaz |
| `02 §5 g2` | Durum adları ve kodları sözlüktedir; `06`, `07` ve `09` birebir devralır (K-17; Aşama 1 dizini) | veri doğurmaz — yazım konvansiyonu | — (VM5-02) — → `02 §5` metin (§1.1.6) | veri doğurmaz |
| `02 §11 g1` | `06`, `07` ve `09` parametrenin değerini buradan okur (K-411; Aşama 1 dizini) | *firma ayarları* · veri doğurmaz — kod | firma ayarlarının değerleri ve varsayılanları (VM4-01, VM8-01); ürün sabitleri kodda — → `02 §11` metin (§1.1.8) | eşlendi |
| `04 2.10 g1` | Eşzamanlı düzenleme uyarısının mekanizması Teknik Mimari ile Veri Modeli'nin kararıdır (Aşama 3 dizini) | Product · Variant · Content · Coupon · *firma ayarları* · Order | sürüm değeri — K-1014 — → `02 §3.31.1`, §3.31.2 (§1.1.5); park A3-5 | eşlendi |
| `04 7.2.4.13 g1` | Ölçü biriminin listesi `02 §3.8.5`'tedir, kod adları Veri Modeli'nde (FORM-16; Aşama 3 dizini; K-1018) | Product | ölçü birimi + net miktar; liste `02 §3.8.5` — K-1018 (⚠) — → `02 §3.8.5` (§1.1.5); park A3-10 | eşlendi |
| `05 §3.5.2 g1` | E-posta tekilliği veritabanında korunur — biçimi `06`'nın işidir (K-982 — dizinden sonra, checkpoint'te eklendi) | Customer · Admin | e-posta tekilliği tür içinde, harf duyarsız — K-1016 — → `02 §10.2.5` (§1.1.8); park A4-9 | eşlendi |
| `05 §3.7.1 g1` | Sayfa adresinin veride tutuluşu `06`'nın işidir — karar satırı olmadan devredildi (Aşama 4 dizini) | Product · Category · Offering · PortfolioItem · CustomPage | sayfa adresi — tekil, ilk yayında sabitlenir (VM4-16) — → `02 §3.30.1`, §3.30.2 (§1.1.5) | eşlendi |
| `05 §4.6.3 g1` | Geri ödeme kaydının hâlleri teknik kalır, biçimi `06`'nın (park satırı) işidir (K-988; Aşama 4 dizini) | *geri ödeme kaydı* | dört hâl — kod adı VM5-03 — → `02 §7.2.8` (§1.1.8); park A4-7 | eşlendi |
| `05 §7 g1` | §7'nin "Ne yazılır" notu: şema `06`'da (Aşama 4 dizini — anma) | veri doğurmaz — yazım konvansiyonu | — | veri doğurmaz |
| `05 §7 g2` | §7'nin girişi: tabloların, alanların ve türlerin kendisi `06`'nın işidir; veriye düşen yüzler park bloğunda (Aşama 4 dizini — anma) | veri doğurmaz — yazım konvansiyonu | — (VM0-06; park blokları bu tablonun park satırlarıdır) | veri doğurmaz |
| `05 §7.2.5 g1` | Para ondalıklı sayıyla; alanların türleri `06`'nın işidir (Aşama 4 dizini — K-870 satırı) | veri doğurmaz — mimari (`05`) | para alanlarının türü — ondalıklı, kuruş hassasiyeti (K-1013) — → `02 §3.8.4` (§1.1.5); park A4-1 | veri doğurmaz |
| `05 §7.3.1 g1` | Sürümün veride tutuluşu `06`'nın işidir (Aşama 4 dizini — K-873 satırı) | Product · Variant · Coupon · Content · *firma ayarları* | sürüm değerinin biçimi — K-1014 — → `02 §3.31.1` (§1.1.5); park A4-2 | eşlendi |
| `05 §7.4.3 g1` | Bağlı kayıtların alan listesi ve belgenin kopya mı yeniden üretim mi olduğu `06`'nın kararıdır (Aşama 4 dizini — park satırı) | Order · OrderEmailHistory · OrderAddressHistory · DefectClaim · PreInformationForm · DistanceSalesContract | boşaltılan alanlar ve bağlı kayıtlar VM9-02; belgenin saklanma biçimi VM4-11 (W ⚠) — → `02 §12.2.7` (§1.1.8); park A4-3 | eşlendi |
| `05 §7.5.1 g1` | İmha fonksiyonunun yetki biçimi `06`'nın işidir (Aşama 4 dizini — park satırı) | AuditLog | iz tablosu ve imha fonksiyonu — VM4-14 — → `02 §8.5.2` (§1.1.8); park A4-3 | eşlendi |
| `05 §7.7.2 g1` | Görselin bağlarının biçimi `06`'nın işidir (Aşama 4 dizini — park satırı) | Image · Product · Content · OrderItem | görselin üç bağı — VM4-15 — → `02 §3.23.1`, §3.11 (§1.1.5); park A4-4 | eşlendi |

#### 1.1.12 GAP adayları — karara bağlandı (2b)

**2b (2026-10-17; K-1006…K-1008).** İki aday karara bağlandı — VA-1 → **GAP-12** (K-1006), VA-2 → **GAP-13** (K-1007, K-1008); düşen aday yok; kararlar §1.3'te ve karar kaydının §3'ündedir. Adayların satırları §1.1'de "eşlendi" ya da — yazım konvansiyonu satırlarında — "veri doğurmaz" durumunu aldı; yedi italik aday sözlüğün kod adını, 72 (OrderAddressHistory) yeni aday olarak listeye girdi (§1.1.2). Aşağıdaki tablo ve hazırlık bloğu kararın girdisi olarak kalır.

**Kapı: matrisin 2b oturumu (K-1004).** Aşağıdakiler **adaydır, karar değildir**: 2a her adayı kaynakların tam metnine karşı doğruladı ve karar için hazırladı (tablonun altındaki hazırlık bloğu); 2b düşeni gerekçesiyle düşürür, kalanı karara bağlar — belirgin önerisi olan ve bir W konusuna bağlanmayan aday 2b'de öneriyle, bir W konusuna bağlanan aday o konuda (karar kaydı §14.1) — ve kararını §1.3'e ve karar kaydının §3'üne (GAP-12'den) yazar (VM1-05). 1b, 1c ve 1d buldukları adayı bu tabloya ekler. Bir adayın satırları §1.1'in tablolarında "GAP adayı (VA-n)" durumunu taşır.

| # | Aday | Dayandığı satırlar | Not |
|---|---|---|---|
| VA-1 | Ürüne görünen ama sözlükte terimi ve kod karşılığı olmayan kayıtların adı | `02 §1.1` m4, `02 §5.8` m3, `02 §10.4.3` · §1.1.2'nin italik adayları · `03 §1.7.1.3`, §1.7.3, §1.11.22, §1.11.27 · K-617, K-645, K-977, K-988 (§1.1.11) | `02 §1.1` m4 ürüne görünen kavramın — panelde ya da müşteriye görünen kayıt, işaret, sayaç — sözlüğe girdiğini, `06`'nın kod adını yalnız teknik kavramlara ve kapalı liste değerlerine verdiğini söyler (K-17, K-531, K-977, K-988). Sözlüğün 105 terimi tam okundu: **ürüne görünen ve terimi olmayanlar** — çıkarma kaydı (`02 §5.8`; panelde kalemin satırı), bekleyen e-posta değişikliği (`02 §3.13.14`; hesap ekranında görünür), onay kutularının kaydı (`02 §3.24.6`; sipariş sayfasında görünür), siparişe donan yasal metin sürümü (`02 §3.23.3`; sipariş sayfası gösterir), fiyat geçmişi (`02 §3.8.5`; fiyatın başlangıç tarihi ürün sayfasında), görsel (`02 §3.11`); **`05`'te kurulan ama K-977'nin teknik kavram listesinde olmayanlar** — siparişin ödeme kaydı (`05 §4.5.3`), dosya kaydı (`05 §7.7`), giriş kaydı (`02 §3.13.20`), yedeğin durum kaydı, anahtar halkası. 2. oturum ayrımı doğrular: teknik olan VM3-03'e, ürüne görünen `02 §1.2`'ye geri beslenecek bir terime (K-652) gider. Bağ ve ayar kayıtları (ürün–kategori bağı, içerik–ürün bağı, firma ayarları, il listesi) adayın dışındadır — adları K-1012'ün adlandırma kuralıyla türer **1b (2026-10-17):** çıkarma kaydının tanımı `02 §10.4.3`'tedir — kalemin çıkarılması ya da adedinin azaltılması; adet, an ve yönetici; sebep listesi istemez, işlem izine yazılır ve B-11 üretir; sözlükte terimi yoktur. `02 §6`–§13'te ürüne görünen ama sözlükte terimi olmayan **işaret ve alanlar** da vardır — panelde: "e-posta ulaşmadı" işareti (`02 §6.1.1`, §9.1.6), "ödeme sonucu alınamadı" işareti (`02 §6.1.2`), "e-posta düzeltildi" işareti (`02 §10.4.11`), "iade malı bekleniyor" ve "IBAN bekleniyor" işaretleri (`02 §7.4.1`, §7.4.5) ve siparişin iç notu (`02 §10.4.7`); sözlük aynı sınıftan iki işareti taşır (`RefundFailedMark`, `RefundOutcomePendingMark`). Bu satırlar eşlendi — alanın yeri bellidir, adı açıktır; 2. oturum ayrımı bunlara da uygular; saklanmayan, türetilen işaretin adı K-1015'nın kararına bağlıdır. **1c (2026-10-17):** Kullanıcı Akışları'nın panel işaretleri tablosu (`03 §1.7.3`) yedi işaret sayar — ikisi sözlükte (`RefundFailedMark`, `RefundOutcomePendingMark`), beşi yukarıdaki listededir; Kullanıcı Akışları'nda yeni bir işaret ya da terimsiz kayıt çıkmadı. Çıkarma kaydının satırları (`03 §1.7.1.3`, §1.11.22) bu adayın durumunu taşır; iç not `03 §1.11.27`, §5.1.6 ve §8.4.6'da anılır. **1d (2026-10-17):** devirlerin gerekçe hücreleri ayrımın yönünü yazar — K-645 *"§1.2'nin başına 'panel işaretlerinin kod adları `06`'da verilir' notu yazılsın"* seçeneğini K-17'ye ikinci bir istisna açacağı için eledi ve iki kalem kaydını ve bir panel işaretini sözlüğe aldı; K-617 *"kavramları sözlüğün dışında bırakmak"* seçeneğini *"`06` adları kendisi türetmek zorunda kalırdı"* diye eledi; K-977 ve K-988 ürüne görünen kavramı sözlüğe, teknik kavramı `06`'ya verir. Ürüne görünen kayıt ve işaretler için önceki kararların kalıbı `02 §1.2`'ye geri beslemedir (K-652); 2. oturumun önerisi bu kalıbın dışına çıkarsa önceki karardan ayrılır (`INSTRUCTIONS §2`'nin üçüncü grubu) |
| VA-2 | Siparişin adres düzeltmesinde eski adresin yeri ve donma kuralının istisnası | `02 §10.4.2` · `02 §3.23.3`, §10.3.1 · `02 §6.3.6`, §6.7.12, §8.3.7, §12.5 m33 · `03 §1.11.23`, §3.2.2.6, §6.2.2.3, §8.4.1 · K-527, K-585, K-586 (§1.1.11) | `02 §10.4.2` teslimat ve fatura adresini — teslimat telefonu dahil — "Kargoya verildi"ye kadar (fiziksel kalemsiz siparişte Teslim edildi'ye kadar) düzelttirir ve düzeltme B-10 ile bildirilir. `02 §10.3.1` iz satırının adres düzeltmesinde eski ve yeni adresi değil siparişi, alanı ve işlemi taşıdığını söyler; misafir e-postasının düzeltmesindeki "e-posta geçmişi"nin (`02 §10.4.11`) adres için bir eşi yazılı değildir; `02 §3.23.3` siparişe donan değerlerin "tek istisna"sını yalnız misafir siparişinin e-postası diye sayar, adres düzeltmesini saymaz; `02 §12.2.7`'nin imha listesi adres geçmişi anmaz. Sonuç: düzeltmeden sonra eski adres hiçbir kayıtta durmaz — kandırılmış düzeltmede (`02 §6.3.6`, §8.3.7) firmanın adresi "geri düzeltmesi" ve teyidin ispatı (§12.5) ürünün içinde dayanaksızdır. Kaynakların tam metni okundu; Kullanıcı Akışları, Arayüz Tanımları ve Teknik Mimari'de "eski adres", "önceki adres" ve "eski ve yeni adres" betikle tarandı — adres düzeltmesine ait karşılık yok (K-739). 2. oturum iki parçayı ayırır: (1) `02 §3.23.3`'ün "tek istisna" cümlesinin adres düzeltmesini de sayması — hizalama, K-652'nin geri beslemesi; (2) eski adresin tutulup tutulmayacağı ve nerede — kişisel veri saklama sorusudur, bir W konusuna bağlanır (VM4-06, VM9). **1c (2026-10-17):** Kullanıcı Akışları adres düzeltmesini dört satırda yürütür — aralık (`03 §1.11.23`), kandırılmış düzeltme (§3.2.2.6), geri düzeltme (§6.2.2.3: sahibi ulaşırsa "yönetici adresi geri düzeltir") ve müdahale (§8.4.1: "iz alanı ve işlemi yazar, adres değerini yazmaz"); geri düzeltme eski adresin bilinmesini gerektirir, dört satırın hiçbiri onu bir kayda yazmaz. Aday güçlenir, kapsamı değişmez. **1d (2026-10-17):** karar kaydı adres değerinin yerini iki kez tarttı. K-527 *"İz adres değerini taşısın, siparişle birlikte imha edilsin"* seçeneğini izin değiştirilemezliğine istisna açacağı için eledi — adresin izde durmamasının kararıdır, başka bir kayıtta durup durmamasının değil. K-585'in gerekçesi e-posta düzeltmesinde değerlerin tutulmasını *"K-527'nin elediği adres değeri teslimat adresiydi ve düzeltme önceki adresi kanıt olarak gerektirmiyordu"* diye adres düzeltmesinden ayırır; K-586 e-postanın değerlerini siparişin e-posta geçmişine taşıdı. Bu cümle (2). parçanın öncülüdür: eski adresin tutulması önerilirse K-585'in öncülünden ayrılır — öneri `INSTRUCTIONS §2`'nin üçüncü grubuna (önceki karardan ayrılan) ve kişisel veriye dokunduğu için ⚠ grubuna girer; öncülü sınayan dayanaklar geri düzeltme (`03 §6.2.2.3`) ve teyidin ispatıdır (`02 §12.5` m33) |

**2a'nın karar hazırlığı (2026-10-17; K-1004).** Hazırlık karar değildir: her aday için sorun, kaynakların tam okuması (checklist §2; K-739), seçenekler, öneri, ⚠ görüşü ve karar yeri 2b'nin girdisidir ve 2b onu buradan okur (K-30). 2a yeni GAP adayı açmadı: geri izlenebilirlik (§1.2) bir boşluk değil iki hizalama buldu — `park A4-3` ve VM3-03'ün listesi; ikisi de 2b'ye öneri olarak gider.

**VA-1 — sözlükte terimi olmayan ürün kavramlarının adı.**
- **Sorun (sade dille):** panelde ya da müşteriye görünen ya da ürün kuralının tanımladığı on üç kavramın — kayıt, alan ya da işaret — Ürün Gereksinimleri'nin sözlüğünde adı yok. Veri Modeli isim icat edemez (`02 §1.1` m3); bu kavramların tablosuna ve alanına ad verilemiyor.
- **Tam okunan kaynaklar:** `02 §1.1` (beş madde) ve §1.2'nin 105 terimi; `02 §3.8.5`, §3.9.3, §3.11.1–§3.11.5, §3.12.1, §3.12.2, §3.12.7, §3.13.14, §3.13.20, §3.24.6, §5.8 m3, §6.1.1, §6.1.2, §7.4.1, §7.4.5, §9.1.6, §10.4.3, §10.4.7, §10.4.11; `03 §1.7.1.3`, §1.7.3 (yedi işaret), §1.11.22, §1.11.27; `05 §4.5.3`, §7.6.3, §7.7.1, §7.7.2; K-617, K-645, K-977, K-988 (dört hücresiyle). **Ölçüt** K-988'in (1)'idir ve `02 §1.1` m4'te yazılıdır: teknik kavram biçimini Teknik Mimari'nin kurduğu, sözlükte yer almayan kayıt ya da yapıdır — adı ürün kuralında geçse de; öteki her kavram ürünün terimidir ve sözlüğe girer — ürüne görünen işaret, durum, sayaç ya da ekran öğesi dahil.
- **Ayrım (doğrulandı).** **Ürünün terimi — sözlüğe girer (13):** kayıt ve alan — çıkarma kaydı (`02 §10.4.3`, §5.8 m3; panelde kalemin satırı, B-11), bekleyen e-posta değişikliği (`02 §3.13.14`; hesap ekranında yeni adresiyle görünür — K-734), onay kutularının kaydı (`02 §3.24.6`; sipariş sayfasında görünür), yasal metin sürümü (`02 §3.33.3`, §3.23.3; sipariş sayfası onaylanan sürümü gösterir), fiyat geçmişi (`02 §3.9.3`; biçimini ürün kuralı kurar — varyant, fiyat, an; fiyatın başlangıç tarihi ve referans fiyat ondan okunur — `02 §3.8.5`), görsel (`02 §3.11`, §3.27.15), dijital ürün dosyası (`02 §3.12.2`; panelde yüklenir, sipariş sayfasından indirilir — her yüklemenin ayrı kayıt olması `05`'in biçimidir, K-883), iç not (`02 §10.4.7`; panelde); işaret — "e-posta ulaşmadı" (`02 §6.1.1`, §9.1.6), "ödeme sonucu alınamadı" (`02 §6.1.2`), "e-posta düzeltildi" (`02 §10.4.11`), "iade malı bekleniyor" (`02 §7.4.1`), "IBAN bekleniyor" (`02 §7.4.5`) — `03 §1.7.3`'ün yedi işaretinden sözlükte olmayan beşi. **Teknik — VM3-03'e girer, adı `05`'teki adından:** siparişin ödeme kaydı (`05 §4.5.3`; panelde görünen ödeme ekseni ve ödeme yöntemi sözlükte — `PaymentStatus`, `PaymentMethod`), giriş kaydı (`02 §3.13.20`; adı ürün kuralında geçer, panelde yoktur — `03 §6.3`), yedeğin durum kaydı ve anahtar halkası (ikisi VM3-03'te). **VM3-03'ün hizalanması:** "oturum" ve "tanınan tarayıcı" listeden çıkar — sözlükte terimleri vardır (`Session`, `RecognizedBrowserMark`), kayıtlarının biçimi park satırındandır (`park A4-9`); ödeme kaydı ve giriş kaydı listeye girer (§1.2, 2. gözlem).
- **Seçenekler:** **(1) Ürünün on üç terimini sözlüğe geri beslemek, teknikleri VM3-03'e eklemek** — K-617, K-645, K-977 ve K-988'in kalıbı; sözlük 105'ten 118 terime çıkar, karşılıklar sözlüğün konvansiyonuyla türetilir ve † taşır, tanımlar gösterilen kuralın cümlesinden alınır. Bedeli: Ürün Gereksinimleri'nin sürüm artışı ve gövdedeki anmaların sözlüğün adıyla hizalanması. **(2) Adları Veri Modeli versin** (sözlüğün dışında) — K-617 *"kavramları sözlüğün dışında bırakmak"* seçeneğini *"`06` adları kendisi türetmek zorunda kalırdı"* diye, K-645 *"panel işaretlerinin kod adları `06`'da verilir"* notunu *"K-17'nin kuralına ikinci bir istisna açardı"* diye eledi; önerilirse önceki karardan ayrılır (`INSTRUCTIONS §2`'nin üçüncü grubu). **(3) Kayıtlar sözlüğe, işaretler sözlüğün dışında** (yalnız Arayüz Tanımları'nın etiketi) — K-977 ve K-988 işareti ürüne görünen kavram sayar; `RefundFailedMark` ve `RefundOutcomePendingMark` sözlüktedir — iki işaret içeride, beşi dışarıda tek adlandırma kuralını böler.
- **Öneri: (1).** Alt ayrıntı — **"IBAN bekleniyor":** K-645 panelin "havale gerçekleşmedi" adımına ayrı terim vermedi (saklanan bir kayıt ya da işaret değil, panel adımıdır); "IBAN bekleniyor" ise `03 §1.7.3`'te bir işarettir ve panelde bir listedir — ayrı terim alır, tanımı IBAN isteğine (`RefundIbanRequest`) işaret eder. **Karşılık adayları** (2b doğrular; işaretlerde sözlüğün `…Mark` konvansiyonu): Kalem çıkarma `ItemRemoval` · Bekleyen e-posta değişikliği `PendingEmailChange` · Onay kutusu kaydı `CheckboxAcceptance` — `Consent` elenir: kutular sözleşmeye ilişkindir, KVKK rızası değildir (`02 §3.24.1`) · Yasal metin sürümü `LegalTextVersion` — eşzamanlılığın "sürüm değeri"yle (K-977'nin teknik listesi) karışmasın diye tam adıyla · Fiyat geçmişi `PriceHistory` · Görsel `Image` · Dijital ürün dosyası `DigitalFile` · İç not `InternalNote` · "E-posta ulaşmadı" işareti `EmailUndeliveredMark` · "Ödeme sonucu alınamadı" işareti `PaymentOutcomeUnknownMark` · "E-posta düzeltildi" işareti `EmailCorrectedMark` · "İade malı bekleniyor" işareti `ReturnPendingMark` · "IBAN bekleniyor" işareti `IbanPendingMark`. İşaretin saklanıp saklanmadığı adlandırmayı değiştirmez — sözlük kavramı adlandırır, alanı değil (K-1015).
- **⚠ görüşü:** ⚠ değildir — adlandırma ayrıntısıdır; varlık, para ya da yasal hak kararı değildir (K-617'nin gerekçe cümlesi) ve öneri önceki kararlardan ayrılmaz. Çok kritik değildir.
- **Karar yeri:** 2b, K-992 moduyla `(öneriyle kaydedildi)` — bir W konusuna bağlı değildir; işaretlerin saklanması K-1015'nın işidir ve adlandırmayı beklemez. **Geri besleme yüzleri** (K-652; checklist §3, betik 1 ve 2): `02 §1.2` — on üç satır (grupları: İptal, cayma ve iade · Hesap · Sipariş · Katalog · Fiyat · Yasal metinler) ve giriş notunun † sayımı; `02`'nin sürümü ve başlık notu; gövdede eski adlar ("çıkarma kaydı", "kalem çıkarma", "onay kutularının kaydı", "fiyat geçmişi", "iç not", beş işaretin tırnaklı adı) aranır ve sözlüğün adıyla okunur; `03` ve `04`'teki anmalar okunur. `06`: §1.1.2'nin italik adaylarından terim alanlar kod adı alır, §1.1'in "sözlükte terimi yok" ve "adı VA-1" ibareli satırları güncellenir; VM3-03'ün listesi (karar kaydı §14.3) hizalanır.

**VA-2 — siparişin adres düzeltmesinde eski adres.**
- **Sorun (sade dille):** firma siparişin teslimat ya da fatura adresini müşterinin talebiyle düzelttiğinde eski adres hiçbir yerde kalmıyor. Ürün Gereksinimleri "siparişe donan değerlerin tek istisnası misafirin e-postasıdır" der, adres düzeltmesini saymaz; kandırılmış bir düzeltmede firmanın adresi "geri düzeltmesini" ister, ama eski adres kaydedilmez.
- **Tam okunan kaynaklar:** `02 §3.17.2`, §3.23.3, §3.24.1–§3.24.3, §3.33.1, §3.33.3, §6.3.5, §6.3.6, §6.7.12, §8.3.7, §9.2 B-1 ve B-10, §10.3.1 (sekiz alan), §10.4.2, §10.4.11 (dört madde), §12.2.7, §12.5'in satırları (m32–m34 dahil); `03 §1.11.23`, §3.2.2.6, §6.2.2.3, §8.4.1; `04`'te siparişe donmuş adresi gösteren iki yer (siparişin sayfasının (7). bölümü ve müşteri bilgisinin satırı); `05 §7.4.3`; `10` KP-17, KP-48; K-527, K-585, K-586, K-601 (dört hücresiyle).
- **Parça (1) — "tek istisna" cümlesi (hizalama).** `02 §3.23.3` donmanın tek istisnasını misafirin e-postası diye sayar; `02 §10.4.2` adresleri — teslimat telefonu dahil — "Kargoya verildi"ye kadar düzelttirir. Kural evinde yazılıdır, özet cümle eksiktir: boşluk değil hizalama hatasıdır (`INSTRUCTIONS §2`). Tek seçenek: cümle iki istisnayı sayar — firmanın düzeltmesiyle değişen teslimat ve fatura adresi (§10.4.2) ve misafir siparişinin iletişim e-postası (§10.4.11). **Yüzler** (2b'nin betik 1'i; terimler "tek istisna", "donar", "dondurur", "donmuş hâliyle"): `02 §3.23.3`; §10.4.11'in (1). sonucu ("§3.23.3'ün donmasına tek istisna"); §1.2'nin `ShippingAddress` ve `BillingAddress` tanımları ("siparişin içine donar"); `10` KP-17 ("tek istisnası misafir siparişinde firmanın düzeltmesidir"); `04`'ün iki yeri. ⚠ değildir. Parça (2)'nin kararı cümleye eski adresin yerini ekleyebildiği için ikisi aynı satırda kaydedilir.
- **Parça (2) — eski adresin tutulması (⚠).**
  - **Öncülün sınanması.** Bugünkü hâlin dayanağı K-585'in gerekçe cümlesidir: *"K-527'nin elediği adres değeri teslimat adresiydi ve düzeltme önceki adresi kanıt olarak gerektirmiyordu."* Cümle Ürün Gereksinimleri'nin cross-review'ının 9. turunda yazıldı. Kandırılmış adres düzeltmesi, firmanın teyit yükü ve geri düzeltme 15. turda K-601 ile geldi — `02 §6.3.6` ve §8.3.7 K-601'in yeni maddeleridir —; K-601 eski adresin tutulmasını tartmadı. Öncülü sınayan üç dayanak: **(a)** geri düzeltme (`03 §6.2.2.3`) — gerçek sahip adresini yeniden söyleyebilir, ama firma ilk adresi göremez ve talebi siparişin bilgisiyle karşılaştıramaz; **(b)** teyidin ispatı firmadadır (`02 §12.5` m33) — ispatın konusu teyidin kendisidir, adresin değeri değil: bu dayanak (2)'yi tek başına zorunlu kılmaz; **(c)** siparişe ait belgeler — Ön Bilgilendirme Formu ve Mesafeli Satış Sözleşmesi kişisel veriyi siparişin alanlarından alır (`05 §7.4.3` (3)); belgenin kopya olarak mı saklandığı, donmuş şablondan mı yeniden üretildiği VM4-11'in kararıdır (W ⚠). Yeniden üretim seçilir ve eski adres tutulmazsa düzeltmeden sonra üretilen belge müşterinin onayladığından farklı adres taşır — belgenin adres taşıyıp taşımadığını `02 §3.24.3` yazmaz; VM4-11'in tamlık taraması bunu okur. **Simetri:** K-586 e-posta düzeltmesinde *"değerler hiç tutulmasın"* seçeneğini *"sahtekârlıkta gerçek sahibin hangi adresin erişim aldığını gösteren kaydı kalmaz; firmanın ispat yükü karşılıksız kalır"* diye eledi; adres düzeltmesinin kandırılma hâli (`02 §6.3.6`) e-postanınkiyle (§6.3.5) aynı biçimdedir.
  - **Seçenekler.** **1 · Tutulmaz** (bugünkü yazılı hâl) — adres yerinde değişir; geri düzeltmede gerçek sahip adresini yeniden verir. Saklama: yok. Kim görür: kimse. Bedeli: kandırılmış düzeltmede ilk adres ürünün içinde okunamaz; düzeltilen siparişin onaylanan adresi kaybolur ve siparişin "kendi kaydıyla ispat değeri taşır" cümlesi (`10` KP-17, "Neden MVP'de") o siparişte zayıflar; VM4-11 yeniden üretimi seçerse belge onaylanandan ayrışır. Yeni alan, ekran öğesi ya da imha kalemi yok; KVKK'nın ölçülülük ilkesine (m.4/2-ç) en yakın seçenek. **2 · Adres düzeltme geçmişi siparişin kaydında — e-posta geçmişinin kalıbıyla (K-586)** — her düzeltmede eski ve yeni değer (teslimat telefonu dahil), düzelten yönetici ve tarih siparişin kendi kaydında durur. Saklama: siparişin kişisel verileriyle birlikte imha edilir (Z-28, Z-41) — yeni bir süre sınıfı doğmaz, aynı siparişin adresi zaten bu süre boyunca tutulur; imhanın alan listesine girer (VM9-02; `05 §7.4.3` (2)'nin bağlı kayıtları). Kim görür: panelde siparişin sayfasında yöneticiler; müşteriye görünmez; işlem izi değer taşımamaya devam eder (K-527 istisnasız kalır). Bedeli: yeni bir alan grubu; Ürün Gereksinimleri (§10.4.2, §3.23.3, §10.3.1, §12.2.7, §12.5 m33), Arayüz Tanımları (panelin sipariş ayrıntısı — e-posta geçmişinin yanında), Teknik Mimari (`05 §7.4.3` (2)) ve MVP Kapsamı (KP-48) geri beslenir; K-585'in öncül cümlesinden ayrılır. **3 · Yalnız düzeltme penceresinde tutulur** — eski adres sipariş "Kargoya verildi"ye (fiziksel kalemsizde Teslim edildi'ye) geçene kadar durur ve geçişle silinir: geri düzeltmenin penceresi kapanınca amacı biter. Kim görür: panelde yöneticiler. Bedeli: duruma bağlı yeni bir silme tetiği — kişisel veriyi hemen silen her işlem imha kaydına yazılır (`02 §12.2.7`); pencereden sonra doğan uyuşmazlıkta (cayma, ayıp, kandırılma iddiası) değer yoktur; VM4-11'in yeniden üretim seçeneğindeki ayrışmayı çözmez.
  - **Öneri: 2.** K-586'nın eleme gerekçesi adres düzeltmesinin kandırılma hâline birebir uyar; geri düzeltme akışı ilk adresi bilmeyi varsayar; saklama yeni bir süre sınıfı doğurmaz ve görünürlük e-posta geçmişininkiyle aynıdır; VM4-11'in iki seçeneği de ayrışmasız kalır. 3 elenir: silme tetiği ekler, pencere dışındaki uyuşmazlığı karşılamaz. 1 elenir: kandırılmış düzeltmede ve belgenin yeniden üretiminde kayıp doğurur.
  - **⚠ ve çok kritik görüşü.** ⚠ grubundadır — kişisel veriye dokunur ve öneri K-585'in öncülünden ayrılır (`INSTRUCTIONS §2`'nin ikinci ve üçüncü grubu). **Çok kritik değildir:** seçenekler yasal olarak yakındır — üçü de KVKK'ya uygun kurulabilir: 1 ölçülülüğe (m.4/2-ç), 2 ve 3 sözleşmenin ifası ve bir hakkın korunması işleme şartına (m.5/2-c, m.5/2-e) dayanır ve süre siparişin kişisel verisinin süresini aşmaz (m.4/2-d); firmaya düşen para yükü değişmez — kandırılmanın sonucu üç seçenekte de firmadadır (`02 §8.3.7`); ürünün kimliğine dokunmaz; geri dönüşü ucuzdur — yayından önce veri yoktur, sonradan eklemek yalnız eklemeden önceki düzeltmelerin geçmişini kaybeder. K-992'nin ⚠ öneriyle kayıt moduyla `(öneriyle kaydedildi — ⚠)` kaydedilebilir.
  - **Karar yeri: 2b.** Kural ürün kuralıdır ve evi `02 §10.4.2`'dir; K-652 geri beslemeyi aynı PR'da ister. VM9-02 (W) ve VM4-06 (T) yalnız alanı devralır; VM4-11 (W ⚠) belgenin biçimini verirken bu kararı okur. **Elenen karar yeri — VM9-02:** imhanın alan listesi bir ürün kuralını kuramaz; kural Ürün Gereksinimleri'ne geri yazılacaksa kararın evi matrisin GAP satırıdır.

### 1.2 Geri izlenebilirlik (entity/alan → kaynak)

**2a (2026-10-17; K-1004, K-1005).** Tablo §1.1.2'nin adaylarından ve §1.1'in satırlarından betikle kurulur; kaynağı olmayan aday ya da alan gerekçesiz eklemedir ve listelenir; Teknik Mimari'nin doğurduğu teknik kayıtların kaynağı park satırıdır (VM1-04). 1a'nın ara denetimi: §1.1.2'nin 71 adayının 69'u §1.1.3–§1.1.6'nın en az bir satırında geçer; kalan ikisi (70, 71) devir adayıdır (§1.1.11). 1b'nin ara denetimi: §1.1.7–§1.1.8'in satırları 71 adayın 62'sini kullanır; kullanmadığı dokuzun yedisi (OptionValue, *ürün–kategori bağı*, Offering, PortfolioItem, FaqItem, Branch, *iş kuyruğu*) 1a'nın satırlarında eşlidir, ikisi (70, 71) devir adayı olarak kalır. 1c'nin ara denetimi: §1.1.9–§1.1.10'un satırları 71 adayın 69'unu kullanır; kalan ikisi (70, 71) devir adayıdır. 1d'nin ara denetimi: §1.1.11'in satırları 71 adayın 58'ini kullanır — 70 ve 71 dahil; kullanmadığı on üçü (Branding, SocialLink, Option, OptionValue, *ürün–kategori bağı*, CartItem, AboutPage, FaqItem, Branch, Announcement, *içerik–ürün bağı*, PrivacyNotice, CookiePolicy) önceki oturumların satırlarında eşlidir; 71 adayın hepsi §1.1.3–§1.1.11'in en az bir satırında geçer — 71'in park satırı yoktur (§1.1.11).

**Birim ve biçim (K-1005; matrisin 2a oturumu, 2026-10-17).** Satırın birimi §1.1.2'nin adayıdır; alan düzeyi adayın "Sorumluluk" hücresindeki alan gruplarıdır — alanın adı, türü ve kısıtı §4'ün işi olduğu için (K-17; VM4) gruplar Türkçe tarifleriyle ve §1.1.2'deki sırasıyla yazılır. **Entity.alan** adayı ve alan gruplarını taşır. **Beslendiği kaynak** adayın §1.1.3–§1.1.11'deki satır sayısını ve aile dağılımını, her alan grubu için en az bir kaynak satırını (`←`) — önce kuralın evi: `02` kuralı ya da sözlük satırı; yoksa `03`, `10`, devir — ve Teknik Mimari'nin biçimini kurduğu kayıtta biçimi veren park satırını taşır (VM1-04). Adayın kaynak satırlarının tam listesi tabloda tekrar edilmez, `geri()` ile üretilir (§1.1.1) — tablo §1.1'in satırlarını ters yönde ikinci kez yazmaz (K-1003'ün (4). maddesinin kalıbı); satır sayıları betikle karşılaştırılır. **Durum** dört değerden biridir: **beslenir** · **beslenir — biçimi park satırından** · **beslenir — park satırı yok** · **gerekçesiz ekleme** (kaynak satırı olmayan aday ya da kaynak satırında karşılığı olmayan alan grubu). §1.1'in her "Alan" tarifi bir kaynak satırının üzerinde durduğu için (K-999) satır düzeyinde gerekçesiz alan doğmaz; geri yönün denetimi adayın ve "Sorumluluk" hücresinin denetimidir.

**2b (2026-10-17; K-1006, K-1008, K-1009).** GAP kararları tabloyu dört yerden değiştirdi: yedi aday sözlüğün kod adını aldı (GAP-12); sözlüğün on dört yeni terimi §1.1.4'e satır olarak girdi; 72 (OrderAddressHistory) eklendi (GAP-13); K-879 devir satırı oldu ve 71 (*anahtar halkası*) `park A4-3`'ten biçim aldı (K-1009). Satır sayıları ve aile dağılımları betikle yeniden üretildi (`geri()`); **72 adayın hepsi beslenir** — 2.898 aday–satır çifti (1.565 satırın 1.275'i en az bir aday taşır); 55'i "beslenir", 17'si "beslenir — biçimi park satırından"; park satırsız aday kalmadı; gerekçesiz ekleme yoktur. ContactRequest'in alan grubu kaynağın sözüyle yazıldı — "son kapatıldığı an" (2a'nın 4. gözlemi; alanın adı yine VM4-12'nindir). **Workshop 1 (2026-10-17; K-1018):** sözlüğün iki yeni terimi Product'a satır ekledi — 105 satır, 2.900 aday–satır çifti; öteki adayların satırları değişmedi.

**Sonuç (2a):** 71 adayın hepsi beslenir — 2.858 aday–satır çifti; 54'ü "beslenir", 16'sı "beslenir — biçimi park satırından", biri (71) "beslenir — park satırı yok"; gerekçesiz ekleme yoktur (betik `06 §1.1.1`, 2026-10-17). Aday başına satır sayısı 1 (71) ile 353 (Order) arasındadır.

| Entity.alan | Beslendiği kaynak | Durum |
|---|---|---|
| Company — tip · tipe göre yasal kimlik alanları · iletişim · KEP · ETBİS bağlantısı | 29 satır — `10` 4 · `02` 18 · `03` 4 · devir 3. tip ← `02 §3.1.3`, `02 §1.2` CompanyType · tipe göre yasal kimlik alanları ← `02 §3.1.2`, `02 §3.1.3` · iletişim ← `02 §3.1.3`, `02 §3.13.16` · KEP ← `02 §3.1.3`, KP-35 · ETBİS bağlantısı ← `02 §3.1.4`, `02 §12.1.4` | beslenir |
| *firma ayarları* — ödeme yöntemleri · havale IBAN'ı · kargo ücreti · eşikler · teslimat illeri · süreler · iade adresi · geçici kapatma · ana sayfa düzeni · menü adları · platform imzası | 75 satır — `10` 9 · `02` 43 · `03` 10 · devir 6 · park 3 · gövde 4. ödeme yöntemleri ← `02 §3.1.5`, `02 §1.2` BankTransfer · havale IBAN'ı ← `02 §3.21.5`, `02 §6.9.14` · kargo ücreti ← `02 §3.8.2`, `02 §3.19.1` · eşikler ← `02 §3.18.1`, `02 §3.19.3` · teslimat illeri ← `02 §3.20.1`, `02 §10.1` m3 · süreler ← `02 §3.17.4`, `02 §3.20.4` · iade adresi ← `02 §3.1.5`, `02 §3.1.7` · geçici kapatma ← `02 §3.1.5`, `02 §3.1.6` · ana sayfa düzeni ← `02 §3.28.1`, `02 §1.2` HomepageLayout · menü adları ← `02 §10.1` m5, `03 §8.6` · platform imzası ← `02 §3.29.6`, `02 §10.1` m6 | beslenir |
| Branding — marka adı · logo · site simgesi · marka rengi | 18 satır — `10` 1 · `02` 15 · `03` 2. marka adı ← `02 §3.29.1`, `02 §3.29.3` · logo ← `02 §3.29.1`, `02 §3.29.2` · site simgesi ← `02 §3.29.1`, `02 §3.29.4` · marka rengi ← `02 §3.29.1`, `02 §3.29.5` | beslenir |
| SocialLink — kapalı platform listesindeki hesap bağlantıları · WhatsApp numarası | 7 satır — `10` 2 · `02` 3 · `03` 2. kapalı platform listesindeki hesap bağlantıları ← `02 §3.27.12`, `02 §1.2` SocialLink · WhatsApp numarası ← `02 §3.27.12` | beslenir |
| *il listesi* — 81 ilin kapalı listesi | 11 satır — `10` 1 · `02` 5 · `03` 2 · devir 2 · park 1. 81 ilin kapalı listesi ← `02 §3.14.3`, `02 §3.14.6` | beslenir |
| Product — ad · açıklama · tip · KDV oranı · üretim yeri · ölçü birimi · sınırlar · sayfa adresi | 105 satır — `10` 14 · `02` 61 · `03` 10 · devir 9 · park 5 · gövde 6. ad ← `02 §3.7.3`, `02 §3.11.5` · açıklama ← `02 §3.11.5`, KP-42 · tip ← `02 §3.2.2`, `02 §3.2.3` · KDV oranı ← `02 §3.8.2`, `02 §1.2` VatRate · üretim yeri ← `02 §3.8.5`, `02 §12.1.5` · ölçü birimi ← `02 §3.8.5`, `02 §1.2` MeasurementUnit, `02 §1.2` NetQuantity · sınırlar ← `02 §3.16.9`, `02 §6.6` · sayfa adresi ← `02 §3.30.1`, `02 §3.30.2` | beslenir |
| Variant — stok kodu · fiyat · stok ya da kontenjan · yayın durumu | 90 satır — `10` 6 · `02` 48 · `03` 18 · devir 10 · park 5 · gövde 3. stok kodu ← `02 §3.3.4`, `02 §1.2` SKU · fiyat ← `02 §3.3.1`, `02 §3.7.3` · stok ya da kontenjan ← `02 §3.6.1`, `02 §3.6.2` · yayın durumu ← `02 §3.7.1`, `02 §3.7.2` | beslenir |
| Option — seçenek boyutu (en çok iki) | 6 satır — `10` 1 · `02` 4 · `03` 1. seçenek boyutu (en çok iki) ← `02 §3.3.2`, `02 §1.2` Option | beslenir |
| OptionValue — seçeneğin değeri; varyantın kombinasyonu | 4 satır — `10` 1 · `02` 2 · `03` 1. seçeneğin değeri; varyantın kombinasyonu ← `02 §3.3.3`, `02 §1.2` OptionValue | beslenir |
| Category — üç seviyeli ağacın düğümü | 17 satır — `10` 3 · `02` 10 · `03` 3 · gövde 1. üç seviyeli ağacın düğümü ← `02 §3.4.1`, `02 §3.4.6` | beslenir |
| *ürün–kategori bağı* — ürünün asıldığı kategoriler | 7 satır — `10` 2 · `02` 3 · `03` 2. ürünün asıldığı kategoriler ← `02 §3.4.4`, `03 §2.1` | beslenir |
| Image — sıra · ana görsel işareti · alternatif metin | 25 satır — `10` 3 · `02` 17 · `03` 2 · devir 1 · park 1 · gövde 1. sıra ← `02 §3.11.2`, `02 §3.27.15` · ana görsel işareti ← `02 §3.11.2`, `02 §3.27.15` · alternatif metin ← `02 §3.11.4`, `02 §3.27.15` | beslenir |
| DigitalFile — yüklenen dosya · varyantın o anki dosyası | 18 satır — `10` 2 · `02` 9 · `03` 4 · devir 2 · park 1. yüklenen dosya ← `02 §3.7.4`, `02 §3.12.2` · varyantın o anki dosyası ← `02 §3.12.2`, `02 §6.7.8` · biçimi ← park A4-4 | beslenir — biçimi park satırından |
| Discount — tarihli · yüzdesel indirim | 19 satır — `10` 2 · `02` 9 · `03` 7 · devir 1. tarihli ← `02 §3.9.2`, `02 §5.11.1` · yüzdesel indirim ← `02 §3.9.1`, K-600 | beslenir |
| PriceHistory — uygulanmış fiyatlar ve başlangıç anları · referans fiyat · fiyat tarihi | 17 satır — `10` 2 · `02` 9 · `03` 3 · devir 3. uygulanmış fiyatlar ve başlangıç anları ← `02 §3.9.3`, `02 §12.1.5` · referans fiyat ← `02 §12.1.6`, `02 §1.2` ReferencePrice · fiyat tarihi ← `02 §3.8.5`, `03 §2.1` | beslenir |
| Coupon — kod · tür · değer · tarih aralığı · kullanım adedi · haklar | 56 satır — `10` 3 · `02` 22 · `03` 14 · devir 9 · park 5 · gövde 3. kod ← `02 §3.10.5`, KP-11 · tür ← `02 §3.10.2`, `02 §5.11.2` · değer ← `02 §3.10.2`, `02 §3.10.4` · tarih aralığı ← `02 §3.10.5`, `02 §5.11.2` · kullanım adedi ← `02 §3.10.5`, `03 §1.4.4.2` · haklar ← `02 §3.10.6`, `02 §5.11.2` | beslenir |
| StockReservation — stok · kontenjan · kupon hakkı | 62 satır — `10` 2 · `02` 24 · `03` 27 · devir 7 · park 2. stok ← `02 §3.17.3`, `02 §3.6.7` · kontenjan ← `02 §3.17.3`, `03 §1.4.4` · kupon hakkı ← `02 §3.10.6`, `02 §3.17.3` | beslenir |
| Cart — üyenin ya da misafir tarayıcısının sepeti | 32 satır — `10` 2 · `02` 17 · `03` 8 · devir 3 · park 2. üyenin ya da misafir tarayıcısının sepeti ← `02 §3.16.1`, `02 §3.16.2` | beslenir |
| CartItem — varyant · adet · ilk eklendiği fiyat | 15 satır — `10` 2 · `02` 9 · `03` 4. varyant ← `02 §3.16.5`, `02 §3.16.7` · adet ← `02 §3.12.4`, `02 §3.16.3` · ilk eklendiği fiyat ← `02 §3.16.5`, `03 §2.3` | beslenir |
| Order — numara · iki eksen · donan adresler · kimlik · sürümler · IBAN · tutarlar · kargo · zaman alanları | 356 satır — `10` 18 · `02` 212 · `03` 69 · devir 45 · park 9 · gövde 3. numara ← `02 §1.2` OrderNumber, `02 §3.22.1` · iki eksen ← `02 §5.3.1`, `02 §5.6.1` · donan adresler ← `02 §3.23.3`, `02 §3.14.6` · kimlik ← `02 §3.23.3` · sürümler ← `02 §3.23.3`, `02 §3.17.2` · IBAN ← `02 §3.21.5`, `02 §3.23.3` · tutarlar ← `02 §3.21.9`, `02 §3.8.2` · kargo ← `02 §3.8.2`, `02 §3.19.1` · zaman alanları ← `02 §3.17.4`, `02 §4.2` Z-7 | beslenir |
| OrderItem — donan değerler · teslim işareti · indirme hakkı · indirme sayacı | 152 satır — `10` 10 · `02` 76 · `03` 45 · devir 17 · park 3 · gövde 1. donan değerler ← `02 §3.23.1`, `02 §1.2` ReferencePrice · teslim işareti ← `02 §3.20.9`, `02 §3.12.1` · indirme hakkı ← `02 §3.12.5`, `02 §3.23.1` · indirme sayacı ← `02 §3.12.5`, `02 §3.12.6` | beslenir |
| OrderEmailHistory — e-posta düzeltmelerinin eski ve yeni değeri | 34 satır — `10` 3 · `02` 15 · `03` 8 · devir 5 · park 2 · gövde 1. e-posta düzeltmelerinin eski ve yeni değeri ← `02 §10.4.11`, `02 §1.2` OrderEmailHistory | beslenir |
| OrderAddressHistory — adres düzeltmelerinin eski ve yeni değeri · yönetici · tarih | 18 satır — `10` 2 · `02` 10 · `03` 4 · park 1 · gövde 1. adres düzeltmelerinin eski ve yeni değeri ← `02 §10.4.2`, `02 §1.2` OrderAddressHistory · yönetici · tarih ← `02 §10.4.2`, `03 §1.11.23` | beslenir |
| CheckboxAcceptance — işaretlenen kutuların kaydı | 22 satır — `10` 2 · `02` 14 · `03` 5 · devir 1. işaretlenen kutuların kaydı ← `02 §3.24.6`, `02 §3.24.2` | beslenir |
| *ödeme kaydı* — ödeme · sağlayıcının işlem kimliği · geç gelen ödeme | 39 satır — `10` 2 · `02` 17 · `03` 14 · devir 4 · park 2. ödeme ← `02 §5.5` Paid, `02 §6.2.15` · sağlayıcının işlem kimliği ← `03 §1.3` Ö1, K-893 · geç gelen ödeme ← `02 §6.2.16`, `02 §5.5` Failed · biçimi ← park A4-7, park A4-11 | beslenir — biçimi park satırından |
| Cancellation — adet · sebep · türü | 82 satır — `10` 3 · `02` 38 · `03` 33 · devir 5 · park 3. adet ← `02 §7.1.6`, `02 §5.8` m2 · sebep ← `02 §5.3.4`, `02 §7.1.1` · türü ← `02 §7.1.1`, `02 §3.17.8` | beslenir |
| ItemRemoval — kalemi çıkarma ya da adedini azaltma | 25 satır — `10` 1 · `02` 10 · `03` 10 · devir 2 · park 2. kalemi çıkarma ya da adedini azaltma ← `02 §10.4.3`, `02 §5.8` m3 | beslenir |
| LateDeliveryTermination — damga · kapsadığı kalemler · adetler | 38 satır — `10` 1 · `02` 19 · `03` 13 · devir 3 · park 2. damga ← `02 §5.8` m4, `02 §7.2.3` · kapsadığı kalemler ← `02 §5.8` m4, `02 §7.2.3` · adetler ← `02 §5.8` m4, `02 §7.1.6` | beslenir |
| Withdrawal — damga · adet · iade adresi · teslim sırası cevabı | 87 satır — `10` 4 · `02` 45 · `03` 24 · devir 11 · park 3. damga ← `02 §5.8` m5, `02 §7.3.5` · adet ← `02 §5.8` m5, `02 §7.1.6` · iade adresi ← `02 §3.1.7`, `02 §5.8` m5 · teslim sırası cevabı ← `02 §7.4.1`, `02 §6.4.34` | beslenir |
| Return — ulaşma tarihi · ulaşan adet | 51 satır — `10` 2 · `02` 25 · `03` 18 · devir 4 · park 2. ulaşma tarihi ← `02 §5.8` m6, `02 §7.4.7` · ulaşan adet ← `02 §5.8` m6, `02 §7.4.7` | beslenir |
| ReturnRejection — iade reddi | 25 satır — `10` 1 · `02` 10 · `03` 10 · devir 3 · park 1. iade reddi ← `02 §7.4.9`, `02 §1.2` ReturnRejection | beslenir |
| NotReturnedClosure — "mal dönmedi" kapanışı | 28 satır — `10` 1 · `02` 10 · `03` 11 · devir 5 · park 1. "mal dönmedi" kapanışı ← `02 §10.4.9`, `02 §1.2` NotReturnedClosure | beslenir |
| RefundIbanRequest — IBAN isteği · tarihi | 66 satır — `10` 2 · `02` 34 · `03` 24 · devir 6. IBAN isteği ← `02 §7.4.5`, `02 §5.8` m9 · tarihi ← `02 §5.8` m9, `02 §7.4.5` | beslenir |
| DefectClaim — kalem · ayıplı adet · durum | 54 satır — `10` 2 · `02` 26 · `03` 18 · devir 5 · park 2 · gövde 1. kalem ← `02 §5.8` m10, `02 §7.5.2` · ayıplı adet ← `02 §7.5.2`, `02 §5.8` m10 · durum ← `02 §5.10` m2, `02 §1.2` Open | beslenir |
| Refund — tür · tutar · yol · kalem bağı | 73 satır — `10` 6 · `02` 38 · `03` 26 · devir 3. tür ← `02 §5.5` m1, `02 §7.2.3` · tutar ← `02 §5.5` m1, `02 §7.1.7` · yol ← `02 §7.2.8`, `03 §1.11.31` · kalem bağı ← `02 §5.5` m1, `02 §7.1.3` | beslenir |
| Customer — ad · e-posta · şifre · doğrulama | 65 satır — `10` 7 · `02` 31 · `03` 13 · devir 10 · park 3 · gövde 1. ad ← `02 §3.13.21`, `02 §10.7.4` · e-posta ← `02 §3.13.2`, `02 §3.15.5` · şifre ← `02 §3.13.8`, `02 §6.5.16` · doğrulama ← `02 §3.13.2`, `02 §3.13.4` | beslenir |
| Admin — ad · e-posta · şifre | 40 satır — `10` 1 · `02` 18 · `03` 10 · devir 7 · park 3 · gövde 1. ad ← `02 §10.2.1`, `02 §10.2.2` · e-posta ← `02 §3.13.16`, `02 §10.2.1` · şifre ← `02 §3.13.16`, `02 §8.1.7` | beslenir |
| AdminInvitation — davet | 26 satır — `10` 1 · `02` 14 · `03` 8 · devir 3. davet ← `02 §1.2` AdminInvitation, `02 §10.2.6` | beslenir |
| Session — sunucudaki oturum | 44 satır — `10` 2 · `02` 22 · `03` 12 · devir 6 · park 2. sunucudaki oturum ← `02 §3.13.10`, `02 §6.5.13` · biçimi ← park A4-3, park A4-9 | beslenir — biçimi park satırından |
| SocialLogin — Google bağı | 17 satır — `10` 1 · `02` 8 · `03` 7 · devir 1. Google bağı ← `02 §3.13.6`, `02 §3.13.7` | beslenir |
| RecognizedBrowserMark — hesap başına tanınan tarayıcı kaydı | 25 satır — `10` 3 · `02` 10 · `03` 6 · devir 4 · park 2. hesap başına tanınan tarayıcı kaydı ← `02 §6.5.11`, `02 §8.2.6` · biçimi ← park A4-3, park A4-9 | beslenir — biçimi park satırından |
| AddressBook — adlandırılmış adresler | 17 satır — `10` 2 · `02` 8 · `03` 4 · devir 3. adlandırılmış adresler ← `02 §3.14.1`, `02 §3.14.6` | beslenir |
| PendingEmailChange — doğrulanmayı bekleyen yeni adres | 17 satır — `10` 1 · `02` 9 · `03` 5 · devir 1 · park 1. doğrulanmayı bekleyen yeni adres ← `02 §3.13.14`, `02 §6.5.9` | beslenir |
| *giriş kaydı* — an · IP · hesap · sonuç | 15 satır — `10` 1 · `02` 8 · `03` 3 · devir 2 · park 1. an ← `02 §3.13.20`, `02 §8.5.1` · IP ← `02 §3.13.20`, `02 §8.5.1` · hesap ← `02 §3.13.20`, `02 §8.5.1` · sonuç ← `02 §3.13.20`, `02 §4.2` Z-40 · biçimi ← park A4-9 | beslenir — biçimi park satırından |
| *deneme kaydı* — limit · eksen · değer · an | 60 satır — `10` 1 · `02` 41 · `03` 12 · devir 4 · park 2. limit ← `02 §3.13.17`, `02 §3.22.3` · eksen ← `02 §3.13.17`, `02 §6.2.18` · değer ← `02 §3.13.17`, `02 §8.2.2` · an ← `02 §3.13.13`, `02 §3.13.17` · biçimi ← park A4-3, park A4-9 | beslenir — biçimi park satırından |
| *bağlantı kaydı* — tür · ömür · kullanılma | 45 satır — `10` 2 · `02` 25 · `03` 11 · devir 6 · park 1. tür ← `02 §3.13.5`, `02 §3.13.9` · ömür ← `02 §3.13.14`, `02 §8.1.3` · kullanılma ← `02 §3.13.9`, `02 §3.15.6` · biçimi ← park A4-9 | beslenir — biçimi park satırından |
| Content — tip · yayın durumu · sıra | 51 satır — `10` 4 · `02` 29 · `03` 8 · devir 3 · park 3 · gövde 4. tip ← `02 §3.27.1`, `02 §3.27.2` · yayın durumu ← `02 §3.27.13`, `02 §3.27.23` · sıra ← `02 §3.27.15`, `03 §2.2` | beslenir |
| AboutPage — tekil Hakkımızda | 10 satır — `10` 1 · `02` 6 · `03` 3. tekil Hakkımızda ← `02 §3.27.3`, `02 §1.2` AboutPage | beslenir |
| Offering — hizmet tanıtımı | 10 satır — `02` 7 · `03` 2 · gövde 1. hizmet tanıtımı ← `02 §3.27.4`, `02 §1.2` Offering | beslenir |
| PortfolioItem — referans iş | 10 satır — `02` 7 · `03` 2 · gövde 1. referans iş ← `02 §3.27.5`, `02 §1.2` PortfolioItem | beslenir |
| FaqItem — sık sorulan soru | 6 satır — `02` 4 · `03` 2. sık sorulan soru ← `02 §3.27.6`, `02 §1.2` FaqItem | beslenir |
| Branch — şube | 6 satır — `10` 1 · `02` 3 · `03` 2. şube ← `02 §3.27.7`, `02 §1.2` Branch | beslenir |
| CustomPage — genel sayfa | 12 satır — `10` 1 · `02` 8 · `03` 2 · gövde 1. genel sayfa ← `02 §3.27.9`, `02 §1.2` CustomPage | beslenir |
| Announcement — tekil duyuru; tarih aralığı | 15 satır — `10` 2 · `02` 6 · `03` 7. tekil duyuru; tarih aralığı ← `02 §3.27.21`, `03 §1.1` metin | beslenir |
| *içerik–ürün bağı* — bağlı ürünler | 7 satır — `10` 2 · `02` 3 · `03` 2. bağlı ürünler ← `02 §3.27.10`, `02 §3.27.27` | beslenir |
| PreInformationForm — ayarlardan üretilen form | 8 satır — `02` 4 · `03` 1 · devir 1 · park 1 · gövde 1. ayarlardan üretilen form ← `02 §3.24.3`, `02 §3.33.1` | beslenir |
| DistanceSalesContract — ayarlardan üretilen sözleşme | 7 satır — `02` 3 · `03` 1 · devir 1 · park 1 · gövde 1. ayarlardan üretilen sözleşme ← `02 §3.33.1`, `02 §1.2` DistanceSalesContract | beslenir |
| PrivacyNotice — firmanın düzenlediği metin | 11 satır — `10` 1 · `02` 9 · `03` 1. firmanın düzenlediği metin ← `02 §3.33.2`, `02 §1.2` PrivacyNotice | beslenir |
| CookiePolicy — firmanın düzenlediği metin | 10 satır — `10` 1 · `02` 8 · `03` 1. firmanın düzenlediği metin ← `02 §3.33.2`, `02 §1.2` CookiePolicy | beslenir |
| LegalTextVersion — dört metnin sürümleri · siparişe donan sürüm | 35 satır — `10` 3 · `02` 23 · `03` 6 · devir 2 · park 1. dört metnin sürümleri ← `02 §3.33.3`, `02 §3.33.1` · siparişe donan sürüm ← `02 §3.23.3`, `02 §1.2` PreInformationForm | beslenir |
| ContactRequest — konu tipi · durum · son kapatıldığı an | 56 satır — `10` 4 · `02` 33 · `03` 15 · devir 3 · park 1. konu tipi ← `02 §3.32.3`, `02 §1.2` ContactTopic · durum ← `02 §3.32.4`, `02 §1.2` Open · son kapatıldığı an ← `02 §3.32.4`, `02 §4.2` Z-33 | beslenir |
| *gönderim kaydı* — her e-postanın gönderim satırı | 141 satır — `10` 6 · `02` 80 · `03` 38 · devir 12 · park 5. her e-postanın gönderim satırı ← `02 §9.1.6`, `02 §4.2` Z-43 · biçimi ← park A3-6, park A4-7, park A4-8, park A4-10, park A4-11 | beslenir — biçimi park satırından |
| *iş kuyruğu* — dış yan etkilerin iş satırları | 8 satır — `02` 1 · `03` 2 · devir 4 · park 1. dış yan etkilerin iş satırları ← K-891, K-946 · biçimi ← park A4-7 | beslenir — biçimi park satırından |
| *kanal hâli kaydı* — kanal hâli | 14 satır — `10` 2 · `02` 4 · `03` 6 · devir 1 · park 1. kanal hâli ← `02 §6.1.1`, `02 §10.6.1` · biçimi ← park A4-10 | beslenir — biçimi park satırından |
| *gelen bildirim kaydı* — imzası doğru bildirimler | 11 satır — `10` 1 · `02` 1 · `03` 3 · devir 5 · park 1. imzası doğru bildirimler ← K-893, K-970 · biçimi ← park A4-7 | beslenir — biçimi park satırından |
| *geri ödeme kaydı* — isteğin hâlleri | 38 satır — `10` 2 · `02` 10 · `03` 18 · devir 6 · park 1 · gövde 1. isteğin hâlleri ← `02 §6.4.24`, `02 §7.2.8` · biçimi ← park A4-7 | beslenir — biçimi park satırından |
| *geçiş kaydı* — kim · ne zaman · hangi geçiş · sebep | 99 satır — `10` 3 · `02` 34 · `03` 57 · devir 4 · park 1. kim ← `02 §3.17.5`, `02 §5.4` metin · ne zaman ← `02 §6.3.4`, `02 §6.7.15` · hangi geçiş ← `02 §3.17.5`, `02 §6.3.4` · sebep ← `02 §5.3.4`, `02 §6.7.5` · biçimi ← park A4-6 | beslenir — biçimi park satırından |
| AuditLog — işlem izi | 142 satır — `10` 6 · `02` 86 · `03` 31 · devir 17 · park 1 · gövde 1. işlem izi ← `02 §10.3.1`, `02 §1.2` AuditLog | beslenir |
| *imha kaydı* — tarih · tür · sayı | 37 satır — `10` 2 · `02` 9 · `03` 14 · devir 11 · park 1. tarih ← `02 §10.3.3`, `02 §12.2.7` · tür ← `02 §10.3.3`, `02 §12.2.7` · sayı ← `02 §10.3.3`, `02 §12.2.7` · biçimi ← park A4-3 | beslenir — biçimi park satırından |
| *nabız ve kesinti kaydı* — nabız · kesintiler · gecelik işin çalışma anları | 15 satır — `02` 5 · `03` 5 · devir 4 · park 1. nabız ← K-898, park A4-8 · kesintiler ← `02 §3.34.7`, `02 §6.7.17` · gecelik işin çalışma anları ← `02 §4.2` Z-36, `03 §4.1.41` · biçimi ← park A4-8 | beslenir — biçimi park satırından |
| *yedeğin durum kaydı* — gecelik tam yedeğin sonucu | 4 satır — devir 3 · park 1. gecelik tam yedeğin sonucu ← K-918, K-937 · biçimi ← park A4-10 | beslenir — biçimi park satırından |
| *anahtar halkası* — çatının şifreleme anahtar halkası | 3 satır — devir 2 · park 1. çatının şifreleme anahtar halkası ← K-975, K-879 · biçimi ← park A4-3, `05 §7.6.3` (park satırının kolu 2b'de eklendi — K-1009) | beslenir — biçimi park satırından |

**Gerekçesiz ekleme listesi (checklist §2; K-1005) — boş.** Kaynak satırı olmayan aday yoktur (betik); her adayın "Sorumluluk" hücresindeki her alan grubu en az bir satırın "Alan" hücresinde karşılık bulur — tablonun `←` kimlikleri, her biri ilgili satırda okundu; biçimini Teknik Mimari'nin kurduğu on yedi kaydın on altısının biçimi bir park satırından gelir (VM1-04) — 2b'den sonra on yedisinin de (K-1009). Gerekçesiz ekleme olmayan ama kayda geçmesi gereken dört gözlem:

1. **71 *anahtar halkası* — park satırı yok.** Tek kaynak satırı K-975'in (4). koludur, biçiminin yeri `05 §7.6.3`'tür; `park A4-3` K-975'in bu kolunu taşımaz (1d — checklist §3'ün "satırı güncelleyen karar bütün kollarını taşır" maddesinin kaçağı). Aday gerekçesiz değildir — kaynağı bir devir satırı ve Teknik Mimari'nin maddesidir. **2b'ye hizalama önerisi:** `park A4-3`'e kol eklenir ("anahtar halkası veritabanında tutulur ve onunla birlikte yedeklenir — tablonun biçimi bu aşamanın kararıdır; `05 §7.6.3`") — park satırı talimat yazar, karar yazmaz (K-36). **2b'de kapandı (K-1009):** `park A4-3` K-879'u ve K-975'in (4). kolunu taşır; K-879 ve K-975'in etki sütunu `06`'yı sayar.
2. **VM3-03'ün listesi ile sözlük ayrışır.** VM3-03 (karar kaydı §14.3) "oturum" ve "tanınan tarayıcı"yı "sözlükte yer almaz, kod adı `05`'teki adından" diye sayar; ikisi sözlükte terimi olan ürün kavramlarıdır (`Session`, `RecognizedBrowserMark` — `02 §1.2`): adları sözlükten, kayıtlarının biçimi park satırındandır (`park A4-9`). Siparişin ödeme kaydı (`05 §4.5.3`) ve giriş kaydı (`02 §3.13.20`) ise teknik kayıttır ve listede yoktur. **2b'ye hizalama önerisi:** VA-1'in kararıyla birlikte liste düzeltilir (§1.1.12, hazırlık bloğu). **2b'de kapandı (K-1009):** VM3-03'ün listesinden oturum ve tanınan tarayıcı çıktı, siparişin ödeme kaydı ve giriş kaydı girdi (karar kaydı §14.3).
3. **§1.1.2'nin "Kaynak" sütunu adayın doğduğu yeri gösterir.** 24 (*ödeme kaydı*) `05 §4.5.3`'ü, 61 (*gönderim kaydı*) `02 §1.1` m4'ü, 71 `05 §7.6.3`'ü gösterir; üçü de §1.1'de bu adayı besleyen satır değildir — Teknik Mimari'nin gövdesi matrise yalnız `06`'yı anan cümleleriyle girer (K-1003) ve `02 §1.1` m4 adlandırma kuralıdır (VA-1). Besleyen satırlar bu tablodadır; düzeltme gerekmez. **2b (K-1009):** §1.1.2'nin girişi sütunun doğum yerini gösterdiğini yazar; sütun değişmedi.
4. **İfade farkı.** 60 (ContactRequest) "kapanış anı" der, kaynak "son kapatıldığı an" der (`02 §3.32.4`, §4.2 Z-33); alanın adı §4'ün işidir (VM4-12). **2b (K-1009):** §1.1.2 ve bu tablo kaynağın sözünü yazar.

**Kesilmiş okumanın örneklemi — uygulanmadı (checklist §2, okuma derinliği).** Örneklem hücre başına kesilerek okunan satırlar içindir. 1a, 1b, 1c ve 1d satırlarını tam okudu ve dördü de bunu kayda yazdı — §1.1.1'in okuma derinliği paragrafı ("kesilmiş okuma yoktur" — dört oturum) ve §1.1.11'in başı; karar kaydı §14.1'in matris maddesi ("kesilmiş okuma yok" — dört kez). 2a dört oturumun kaydını okudu ve doğruladı (2026-10-17); örneklemin kümesi boştur.

### 1.3 Boşluklar (GAP) ve kararlar

**Kapı: matrisin 2b oturumu (K-1004) — geçildi.** GAP kararları buraya ve karar kaydının §3'üne yazılır; "#" sütunu karar kaydının GAP ailesini GAP-12'den sürdürür (K-995). Adaylar ve 2a'nın karar hazırlığı §1.1.12'dedir.

**2b (2026-10-17; K-1006…K-1008).** İki aday karara bağlandı, düşen aday yok; ikisi de bir W konusuna bağlanmadı — kuralın evi Ürün Gereksinimleri'dir ve karar matrisin GAP satırında verildi (K-1004; hazırlık bloğu, §1.1.12). Matris tamamlandı: checklist §2'nin son iki maddesi — GAP'ler karara bağlandı, çıktı üretimine geçilir — karşılandı; §3'ün (entity envanteri) kapısı açıktır (`06` şablonu başlık notu).

| # | Boşluk | Karar | Nereye yansıdı |
|---|---|---|---|
| GAP-12 | Ürüne görünen on üç kavramın — sekiz kayıt ve alan, beş panel işareti — sözlükte adı ve kod karşılığı yoktu; Veri Modeli isim icat edemez (`02 §1.1` m3); iki teknik kayıt — siparişin ödeme kaydı, giriş kaydı — VM3-03'ün listesinde değildi (aday VA-1) | **K-1006 (öneriyle kaydedildi).** On üç terim Ürün Gereksinimleri'nin sözlüğüne girdi — Görsel `Image`, Dijital ürün dosyası `DigitalFile`, Fiyat geçmişi `PriceHistory`, Bekleyen e-posta değişikliği `PendingEmailChange`, İç not `InternalNote`, Kalem çıkarma `ItemRemoval`, Yasal metin sürümü `LegalTextVersion`, Onay kutusu kaydı `CheckboxAcceptance` ve beş işaret: `EmailUndeliveredMark`, `PaymentOutcomeUnknownMark`, `EmailCorrectedMark`, `ReturnPendingMark`, `IbanPendingMark`; karşılıklar † ile türetildi. Onay kutusu kaydı KVKK rızası değildir — `Consent` elendi. İki teknik kayıt VM3-03'te (K-1009) | `02 §1.2` (v0.77) · §1.1.2 (yedi adayın kod adı), §1.1.4 (on üç satır), §1.1.3–§1.1.11 (VA-1'in satırları), §1.2 · karar kaydı §3, §14.3 VM3-03 |
| GAP-13 | Siparişin adres düzeltmesinde eski adres hiçbir kayıtta kalmıyordu — kandırılmış düzeltmede geri düzeltmenin dayanağı yoktu; `02 §3.23.3` donmanın tek istisnasını misafirin e-postası diye sayıyordu (aday VA-2) | **K-1007 (öneriyle kaydedildi):** donmanın iki istisnası vardır, ikisi de firmanın düzeltmesidir — adres ("Kargoya verildi"ye kadar) ve misafir siparişinin e-postası. **K-1008 (öneriyle kaydedildi — ⚠):** eski ve yeni adres — teslimat telefonu dahil —, düzeltmeyi yapan yönetici ve tarih siparişin kendi kaydında "adres geçmişi"nde durur (e-posta geçmişinin kalıbı); panelde yöneticiye görünür, müşteriye görünmez; işlem izi değer taşımaz; siparişin kişisel verileriyle imha edilir — yeni süre ya da silme tetiği yok. Sözlüğe Adres geçmişi `OrderAddressHistory` girdi; belgenin saklanma biçimiyle bağı VM4-11'dedir | `02 §1.2`, §3.23.3, §6.3.6, §8.3.7, §10.3.1, §10.4.2, §10.4.11, §12.2.7, §12.5 (v0.77) · `03` 3.2.2.6, 6.2.2.3, 8.4.1 (v0.29) · `04` 5.16.10, 9.9.10, 9.9.31, 9.9.40 (v0.29) · `05` 7.4.3 (v0.18) · `10` KP-17, KP-48 (v0.52) · §1.1.2 (aday 72), §1.1.3–§1.1.11 (VA-2'nin ve adres düzeltmesinin satırları), §1.2, başlık (`park A4-3`) · karar kaydı §3, §14.3 VM4-11, VM4-13, VM9-02 |

**Üst dokümanlara geri besleme** (K-994). Matrisin dışında — workshop'ta ya da yazımda — alınıp Ürün Gereksinimleri'ne, Kullanıcı Akışları'na, Teknik Mimari'ye ya da MVP Kapsamı'na dönen her karar bu tabloya bir satırla girer (K-652; `checklists/document-stage.md` §3). Matrisin GAP kararlarının geri beslemesi de buradadır (2b). Üst dokümanların ✓ durumu korunur, kalite döngüleri yeniden açılmaz; her birinin başlık notunda ve dipnotunda sürüm satırı vardır.

| Bulgu | Karar | Değişen bölümler | Sürüm |
|---|---|---|---|
| GAP-12 — sözlükte adı olmayan ürün kavramları (matrisin 2b oturumu) | K-1006 (öneriyle kaydedildi) | `02 §1.2` — on üç satır, giriş notunun † cümlesi; §1'in Kaynak satırı | Ürün Gereksinimleri v0.77 |
| GAP-13 (1) — donmanın istisna cümlesi (2b) | K-1007 (öneriyle kaydedildi) | `02 §3.23.3`, §10.4.11 (1), §1.2 (Teslimat adresi, Fatura adresi); §3'ün ve §10'un Kaynak satırları · `10` KP-17 | Ürün Gereksinimleri v0.77 · MVP Kapsamı v0.52 |
| GAP-13 (2) — adres düzeltmesinde eski adres (2b) | K-1008 (öneriyle kaydedildi — ⚠) | `02 §1.2` (Adres geçmişi), §6.3.6, §8.3.7, §10.3.1, §10.4.2, §12.2.7, §12.5; §1, §6, §8, §10 ve §12'nin Kaynak satırları · `03` 3.2.2.6, 6.2.2.3, 8.4.1 · `04` 5.16.10, 9.9.10, 9.9.31, 9.9.40 · `05` 7.4.3 · `10` KP-17, KP-48 | Ürün Gereksinimleri v0.77 · Kullanıcı Akışları v0.29 · Arayüz Tanımları v0.29 · Teknik Mimari v0.18 · MVP Kapsamı v0.52 |
| Yüzde alanlarının hassasiyeti ve KDV oranının aralığı (workshop 1, VM2-04) | K-1013 (öneriyle kaydedildi) | `04` 7.2.4.13 (net miktarın üç ondalığı), 7.3.2.9; §7.2'nin ve §7.3'ün Kaynak satırları | Arayüz Tanımları v0.30 |
| Kimlik karşılaştırması ve stok kodunun harf duyarsız tekilliği (workshop 1, VM2-07) | K-1016 (öneriyle kaydedildi — ⚠) | `02 §3.3.4`; §3'ün Kaynak satırı · `04` 8.2.10; §8.2'nin Kaynak satırı | Ürün Gereksinimleri v0.78 · Arayüz Tanımları v0.30 |
| Ölçü birimlerinin listesi ve birim fiyatın temel birimi (workshop 1, VM5-04) | K-1018 (öneriyle kaydedildi — ⚠) | `02 §1.2` (Ölçü birimi, Net miktar; giriş notu), §3.8.5; §1'in ve §3'ün Kaynak satırları · `04` 7.2.4.13; §7.2'nin Kaynak satırı | Ürün Gereksinimleri v0.78 · Arayüz Tanımları v0.30 |

**Geri beslemenin taraması (checklist §3; 2b, 2026-10-17).** **Betik 1 — kuralın yüzleri.** Terimler Proje Vizyonu'nda, Ürün Gereksinimleri'nde, Kullanıcı Akışları'nda, Arayüz Tanımları'nda, Teknik Mimari'de ve MVP Kapsamı'nda arandı, her eşleşme okundu. (a) *GAP-13 — kuralın adı, nesnesi ve işlemi, iki ses biçimi:* "tek istisna" · "donar", "dondurur", "donmuş", "donma" (adresle birlikte) · "eski adres", "önceki adres" · "geri düzelt" · "adres düzelt", "adresini düzelt" · "düzeltme geçmiş" · kuralın eşi "e-posta geçmişi". Güncellenenler yukarıdaki tablodadır. Gerekçesiyle elenenler: "tek istisna"nın öteki eşleşmeleri başka kurallardır (dijital dosya `02 §3.12.7`, çerezler, IBAN girişi, sıfır kaybın istisnası, ileri tarihli yayın, göç ve yama, hizmet ifa süresi); "donmuş fatura adresi" (`02 §3.25` ve §10.7'nin fatura verisi, `03` 10.2.4) siparişin kaydındaki değeri anar — istisna `02 §3.23.3`'te tanımlıdır, cümle doğru kalır; "eski adres"in hesap e-postasına, iade adresine ve misafir e-postasına ait eşleşmeleri başka kurallardır; e-posta geçmişinin yalnız e-posta düzeltmesine ait yüzleri (`02 §6.3.5`, §8.3.6, §10.4.11, §12.5'in e-posta satırı; `04` 2.5.3.6, 9.9.12; `03` 8.4.9) değişmedi; `03` 1.11.23 aralığı ve bildirimi yazar, geçmişi 8.4.1 taşır. (b) *Sınıfı ölçüt alan kapalı listeler — adres geçmişi kişisel veridir:* imhanın biçimi (`02 §12.2.7` — güncellendi), siparişin imha parçasında bağlı kayıtlar (`05 §7.4.3` (2) — güncellendi; `park A4-3` — güncellendi), anında silme ve imha kaydı listeleri (`02 §12.2.7`, `10` KP-74 — değişmez: geçmiş anında silinmez, yeni silme tetiği yok), süre tablosu (`02 §4.2` Z-28, Z-29, Z-41 — değişmez: geçmiş siparişin verisidir ve Z-28/Z-41'i izler; e-posta geçmişinin de ayrı satırı yoktur), sıfır kayıp listesi (`02 §3.34.8` — değişmez: "onaylanmış sipariş" kaydın parçalarını kapsar, e-posta geçmişi de ayrı sayılmaz), hukuki sebepler (`02 §12.2.3` — değişmez: sipariş verisi m.5/2-c), veri asgariliği (`02 §8.4` — değişmez). (c) *GAP-12 — sözlük terimleri ve gövdedeki anmalar:* "çıkarma kaydı", "kalem çıkarma", "onay kutularının kaydı", "fiyat geçmişi", "iç not", "görsel", "dosya", "bekleyen e-posta değişikliği", "yasal metin sürümü" ve beş işaretin tırnaklı adı — gövdedeki anmalar sözlüğün adıyla aynıdır; "çıkarma kaydı" kalemin kaydıdır (`02 §5.8` — "iptal kaydı"nın kalıbı) ve değişmez. (d) *Kuralın evi — Kaynak hücreleri:* KP-17 ve KP-48'in Kaynak hücrelerindeki `02 §3.23.3`, §10.4.2, §10.4.11 yeni hâliyle okundu; `02 §10.4.2`'nin Kaynak'ı (§10'un satırı) K-1007 ve K-1008'i taşır. (e) *Matrisin özet hücreleri:* bu dokümanın §1.1'inde `02 §3.23.3`, §10.3.1, §10.4.2, §10.4.11, §12.2.7, §12.5 m33, sözlüğün iki adres terimi, KP-17, KP-48 ve `03` satırlarının özeti güncellendi; Arayüz Tanımları'nın matrisindeki KP-17, `02 §3.23`, §10.4, `03 §8.4.1`, §3.2.2.6, §6.2.2.3 özetleri genel kalır, değişmez. **Betik 2 — Kaynak satırları:** Ürün Gereksinimleri'nin gövdesine giren K-1006, K-1007 ve K-1008 bulundukları bölümün Kaynak satırında (§1, §3, §6, §8, §10, §12) — betik boş döndü; Kullanıcı Akışları, Arayüz Tanımları ve Teknik Mimari'nin gövdesine yeni K numarası girmedi (bölüm atfıyla yazıldı); MVP Kapsamı'nda K numaraları satırların Kaynak hücresindedir. Teknik Mimari'nin mimari karar özet tablosu değişen bir maddeyi taşımaz (MK-23'ün gerekçesi bağlı kayıtları saymaz).

**Geri beslemenin taraması (checklist §3; workshop 1, 2026-10-17).** **Betik 1 — kuralın yüzleri.** Terimler Proje Vizyonu'nda, Ürün Gereksinimleri'nde, Kullanıcı Akışları'nda, Arayüz Tanımları'nda, Teknik Mimari'de ve MVP Kapsamı'nda arandı, her eşleşme okundu. (a) *K-1018 — kuralın adı, nesnesi ve işlemi:* "ölçü birimi", "net miktar", "birim fiyat" — güncellenenler yukarıdaki tablodadır; gerekçesiyle elenenler: `02 §1.2` Ürün satırı alanın ürün düzleminde yaşadığını söyler (düzlem K-1018'in konusu değildir — karar kaydı §14.1'in çok kritik sorusu), `02 §10.1.2` panel tablosu, §12.1.5 ve §12.5'in yükümlülük satırı alanı ve yükümlülüğü anar, listeyi anmaz — değişmez; `03` 8.1.4, `04` 5.4.4, 9.5.6, `10` KP-5 ve KP-39 alanı ve birim fiyatın gösterimini anar — değişmez; `04 §1.1`–§1.2'nin eşleme hücreleri genel kalır. (b) *K-1016 — "stok kodu" + "tekil", "büyük-küçük harf", "harf duyarsız", "harf ayrımsız":* `02 §1.2` Stok kodu, `03` 8.1.2, `04` 7.2.4.7 ve 9.5.21, `05` MK-32 ve 7.8.1 tekilliği anar, karşılaştırmanın kuralı `02 §3.3.4`'te tek yerde yaşar — değişmez; e-posta ve kupon kodunun yüzleri (`02 §3.10.5`, §10.2.5; `03` 2.4.4; `04` 7.3.2.1, 7.3.2.19; `05` 3.5.2; `10` KP-2) "büyük-küçük harf ayrımı olmadan" der ve biçimi Veri Modeli'ne bırakır — değişmez; `04` 8.2.10 Türkçe dönüşümü genel kural olarak yazıyordu — güncellendi. (c) *K-1013 — "yüzde", "oran", "KDV oranı", "ondalık":* `02 §3.8.2`, §3.9.1, §3.10.2, §3.10.4 ve `04` 7.2.4.11, 7.2.21.5 alanı ve sınırları anar, hassasiyeti anmaz — kuralın evi `04` 7.3.2.9'dur; `02 §3.8.4` tutarın kuruş hassasiyetini yazar — değişmez. (d) *Kuralın evi — Kaynak hücreleri:* `04` 7.2.4.13'ün Kaynak hücresi `02 §3.8.5`'i gösterir ve yeni hâliyle okundu. (e) *Matrisin özet hücreleri:* bu dokümanın §1.1'inde `02 §3.3.4`'ün ve §3.8.5'in özeti, sözlüğün iki yeni satırı, `park A3-10` ve `04 7.2.4.13 g1` güncellendi; Arayüz Tanımları'nın matrisindeki `02 §3.3.4` ve §3.8.5 özetleri genel kalır, değişmez. **Betik 2 — Kaynak satırları:** Ürün Gereksinimleri'nin gövdesine giren K-1016 ve K-1018 §1'in ve §3'ün Kaynak satırında; Arayüz Tanımları'nın gövdesine giren K-1013, K-1016 ve K-1018 7.2'nin, 7.3'ün ve 8.2'nin Kaynak satırında — betik boş döndü. **Matrisin kümesi:** sözlüğün iki terimi 1a'yı 684'ten 686'ya çıkardı; öteki kümeler eski çıktıyla satır satır aynı (`cmp`) — §14.1'in öğrenim adayı 9'un ikinci uygulaması.

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

*Shopfolio — Data Model v0.8*
