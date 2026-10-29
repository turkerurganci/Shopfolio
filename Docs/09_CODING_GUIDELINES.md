# Shopfolio — Coding Guidelines

**Versiyon: v0.3** | **Bağımlılıklar:** `02`, `04`, `05`, `06`, `07`, `08`, `10_MVP_SCOPE.md` | **Son güncelleme:** 2026-10-29

> **Aşama:** 8 — Kodlama Kılavuzu · **Rol:** Tech Lead
> **Amaç:** Ajanın tutarlı kod üretmesi. Bu doküman **her kodlama görevinde** ajana verilir.
> **Uyarı:** Bu doküman diğerlerinden zordur — hem çok sayıda dokümanla tutarlılık hem implementasyon düzeyi detay ister; kalite döngüsü genelde daha uzun sürer. Burada eklenen her yeni alan/parametre `06`'da tanımlı mı diye **kontrol edilmelidir**.

> **Aşama 1'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36.
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-17 · K-977 · K-988 · K-1036 · K-1038 · Terim sözlüğü:** `02 §1` sözlüğü **birebir devralınır**; sembol, dosya ve tip adları sözlüğün İngilizce karşılıklarıyla — kapalı liste değerleri, kayıt düzeyindeki kapalı değerler, kimlikli listeler, biçimini Teknik Mimari'nin kurduğu teknik kavramlar ve sözlükte terimi olmayan alanlar (kaydın nitelikleri) Veri Modeli'nin (`06`; alan adları `06 §4`, kapalı değerlerin ve kimlikli listelerin kod adları `06 §5`) verdiği adlarla (K-531, K-977, K-988, K-1036 (2), K-1038 (1); satır Aşama 4'ün çakışma taramasında ve checkpoint'inde, Aşama 5'in çakışma taramasında hizalandı) — üretilir. Eş anlamlı ad kullanımı yasaktır — bir kavramın tek adı vardır.

> **Aşama 4'ten park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §12.3 TM0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-866 · K-867 · K-1310 · K-1312 · Dil, çatı ve kütüphane ekleme:** kod C# ile, .NET 10 (LTS), ASP.NET Core ve Entity Framework Core üzerinde yazılır; ürün yalnız LTS sürümlerinde yürür; eklenen her kütüphanenin lisansı ücretsiz ve ticari kullanıma açıktır ve bakımı süren bir projedir (Teknik Mimari `05 §2`). Ölçüt güncellemede de uygulanır — ana sürüm önerisi ve bakım sürümünün kütüphane güncellemesi lisans değişikliğine bakılarak alınır (`05 §8.8.2`, §8.8.4); koşullu lisanslı (gelir eşikli, ticari ya da açık kaynak şartlı) kütüphane alınabilir — eşiği aşan firmada lisans bedelini kurulumu yapan öder ya da firmaya yansıtır; açık lisanslı eşdeğer yeterliyse o seçilir ve koşul tabloda ile kurulum belgesinde yazılı durur (K-1310, K-1312; karar kaydı §20.1); kopyaleft lisans (GPL, LGPL, AGPL) kabul edilmez — repo MIT lisanslıdır ve her kurulum ayrı bir firmadır (K-1305). C# kod standartları, kütüphane eklemenin işletimi ve koşullu lisanslı kütüphanenin kabulü bu aşamanın kararıdır.
> - **K-868 · K-869 · Sayfa ve ön yüz düzeni:** sayfalar Razor Pages ile sunucuda üretilir, form gönderimi gönder–yönlendir–göster düzenindedir; temel akışlar JavaScript kapalıyken de tamamlanır; ön yüz çatısı ve npm derleme hattı yoktur — düz JavaScript modülleri ve düz CSS; yalnız tarayıcı tabanının (son iki büyük sürüm, KP-70) desteklediği özellikler kullanılır (`05 §3.8.4`); üçüncü taraf betik ve yazı tipi sunucusu kullanılmaz (`05 §2`). Dosya düzeni ve adlandırma bu aşamanın kararıdır.
> - **K-871 · K-881 · K-887 · K-932 · K-980 · Modül sınırları, dosya deposu ve önbellek:** ürün tek dağıtılabilir modüler monolittir ve dokuz modüldür (`05 §3.3`); bir modül öteki modülün verisine yalnız onun açık arayüzünden erişir — yazma da okuma da —, çağrı çağıranın veritabanı işleminin içindedir ve uygulama içi olay yayını yoktur (`05 §3.4`); dosyalara bir dosya deposu arayüzü üzerinden erişilir; önbellek uygulamanın belleğindedir ve ilgili kayıt kaydedildiğinde geçersiz kılınır (`05 §3`, §7) — geçersiz kılma işlem kesinleştikten sonra yapılır ve araya giren eski okumanın önbelleğe yazılmasını bir kuşak sayacı önler; zamana bağlı görünürlük (duyurunun aralığı gibi) önbellekteki kayda karşı her istekte hesaplanır; sipariş onayının işlemi satış kapısının ve tutarı etkileyen ayarların değerlerini önbellekten değil veritabanından okur (`05 §7.10.1`; K-980). Sınırların kodda nasıl korunduğu, arayüzün kullanımı ve geçersiz kılmanın ve kuşak sayacının kalıbı bu aşamanın kararıdır.
> - **K-882 · K-973 · Görüntü işleme kütüphanesi:** yüklenen görsel sunucuda yeniden kodlanır — önce yön bilgisi piksellere uygulanır, ardından meta veri (EXIF, XMP, IPTC) açıkça atılır; çözülen görselden kendiliğinden düşmez —, hareketli görsel ilk karesiyle kodlanır — çözme de ilk kareyle sınırlanır, kare sayısı belleği çoğaltmaz (`05 §6.6.2`) — ve WebP türevleri üretilir (`05 §7.7.2`); kütüphanenin lisansı ücretsiz ve ticari kullanıma açıktır ve bu görevleri — boyutu çözmeden okumak (piksel tavanı), yönü uygulamak, meta veriyi atmak, hareketli kaynakta çözmeyi ilk kareyle sınırlamak — taşır (`05 §6.6.2`). Kütüphanenin seçimi ve ilk kare sınırının kütüphanede uygulanışı bu aşamanın kararıdır.
> - **K-889 · K-891 · K-895 · K-946 · K-1111 · K-1132 · Geçiş birimi, iş kuyruğu ve zaman:** siparişin durum alanlarına yalnız geçiş birimi yazar — yöneticinin düzeltme geçişleri de onun adı olan işlemleridir, müşteri akışının beyaz listesine girmez (`05 §4.1.1`); dış yan etkiler iş kuyruğuna yazılır ve işleyiciler aynı satırı ikinci kez gördüğünde sonucu değiştirmez; kuyruğun çalıştırıcısı satırları sırayla işler — işlem bitince uyandırılır, ayrıca beş saniyede bir kuyruğa bakar — ve bir satırın hatasıyla durmaz, döngünün kendisi düşerse süreç durur ve konteyner yeniden başlar — arka plan hizmetinin hatasını yok sayan ayar kullanılmaz; kuralı yazılmamış iş türünde ve işleyicinin hatasında — kod hatası ya da geri alınan veritabanı işlemi; kart iadesinin geri ödeme kaydı yazılmadan düşen denemesi dahil — satır 1, 5, 15, 60 ve 240 dakika sonra, en çok beş kez yeniden denenir, sonra "başarısız" olur (`05 §4.3.2`, §4.3.4; K-1111, K-1132); nabız işlerden ayrı bir döngüdedir ve gecelik iş son başarılı çalışmasının anıyla başlar — başarısız biterse en erken bir saat sonra yeniden denenir (`05 §5.1.1`, §5.4.1); "başarısız" iş satırı kendiliğinden silinmez, yönetim komutuyla yeniden kuyruğa alınır ya da kapatılır (`05 §8.5.9`); zaman tek bir kaynaktan okunur (Teknik Mimari `05 §4`, §5). Kuralların kodda nasıl korunduğu — mimari testler dahil —, zamanın sınanabilirliği ve veritabanı oturumunun kilit bekleme, sorgu ve boşta işlem zaman aşımlarının değerleri — aşım `05 §10.3`'ün hâllerine düşer — bu aşamanın kararıdır.
> - **K-899 · K-900 · K-901 · K-903 · K-909 · K-912 · K-942 · K-972 · K-973 · K-1081 · Güvenlik kalıpları:** oturum sunucuda tutulur, şifre çatının özetleyicisiyle saklanır; istemci IP'si yalnız ters vekilin ilettiği başlıktan okunur — güven ters vekilin iç ağıyla tanımlanır (`05 §6.3.3`) —, IPv6'da /64; anahtarlar kriptografik rastgele üreteçle üretilir — hesap bağlantısının anahtarı ise saklanmaz: e-postanın gövdesi üretilirken bağlantı kaydının kimliğinden, o üreteçle üretilmiş kurulum sırrıyla (HMAC) türetilir, bağlantı kaydında yalnız özeti durur, gönderim kaydı yalnız bağlantı kaydının kimliğini taşır ve yeniden deneme aynı anahtarı yeniden türetir (`05 §6.4.1`, §6.4.4; K-1081); şifre, anahtar, sır, kart verisi, IBAN, e-posta, adres ve telefon hiçbir sistem kaydına yazılmaz (`05 §9.1`), bağlantı anahtarı taşıyan adres maskelenir; yetki sunucuda, varsayılan kapalı politikayla ve kayıt düzeyinde süzülerek denetlenir; oturum anahtarı yalnız başarılı girişte üretilir; girişten ve yeniden doğrulamadan sonraki dönüş adresinin sitenin kendi yolu olduğu çatının yerel adres denetimiyle denetlenir (`05 §6.5.6`); Google ile girişin akış çerezi çatının varsayılanıyla (SameSite=None) değil Secure, HttpOnly, SameSite=Lax yazılır ve yolu `__Host-` öneki için köktür (`05 §6`, 6.7.12). Kalıpların kodda kullanımı ile türetmenin ve sırrın ayardan okunuşunun kodda kalıbı bu aşamanın kararıdır.
> - **K-914 · K-915 · K-962 · K-974 · K-975 · K-986 · Hata yakalama ve kayıt:** beklenmeyen hata tek bir yakalama katmanında üç hâle ayrılır ve sonucu belirsiz işlemler kodda adıyla işaretlidir; uygulama kaydı yapılandırılmıştır (her satır bir JSON nesnesi), istek kimliğini erişim kaydıyla aynı değerle taşır — kimliği ters vekil üretir ve uygulamaya iletir (`05 §9.1.5`; K-986) —, hesabı iç kimlikle anar ve şifre, anahtar, sır, kart verisi, IBAN, e-posta, adres, telefon, form gövdesi ve çerez değerini hiçbir düzeyde yazmaz (Teknik Mimari `05 §9`, §10); anlık silmenin uygulama kaydındaki satırı silme işlemi diske yazıldıktan sonra yazılır ve diske inmesi beklenir — geri alınan silme satır bırakmaz, kesinleşen silmenin satırı tamponda kalmaz; satırın yazım ve diske indirme hatası çağırana döner, sessizce yutulmaz (`05 §7.4.5`); çatının ve veri erişim katmanının kayıt kategorileri uyarı düzeyindedir — kurulum ayarı yalnız uygulamanın kategorilerinin düzeyini değiştirir (`05 §9.1.6`); anlık silme satırı ve yönetim komutlarının satırları kayıt düzeyi ayarından bağımsız yazılır — ayar yalnız öteki bilgi satırlarını kapatır (`05 §9.1.6`, §7.4.5, §8.5.7; K-986); yönetim komutları uygulama kaydı satırlarını aynı dizinde kendi dosyalarına yazar — uygulamanın dosyasına ikinci bir süreç yazmaz; imha işi, okuma komutu ve silmelerin yeniden uygulanması bu dosyaları da uygulama kaydı sayar; okuma komutunun satırı okumadan önce yazılır ve diske inmesi beklenir (`05 §8.5.7`, §9.3.4; K-986). Kayıt kütüphanesi, alanların adları ve maskelemenin kodda uygulanışı bu aşamanın kararıdır.
> - **K-919 · K-920 · K-921 · Geliştirme ortamı, imaj ve testler:** geliştirme ortamı kurulumun Compose tanımının bir varyantıdır — aynı veritabanı sürümü, ödeme sağlayıcısının test ortamı ve e-postaları yakalayan yerel bir araç; uygulama imajı çok aşamalı derlenir, kök olmayan kullanıcıyla çalışır ve göç demetini taşır; birim testleri modül sınırlarının mimari testlerini içerir, entegrasyon testleri gerçek PostgreSQL 18'e karşı koşar (`05 §8`). Dosya düzeni, test projelerinin ayrımı ve araçlar bu aşamanın kararıdır.
> - **K-922 · K-923 · K-962 · K-971 · K-1096 · Ayarlar ve yönetim komutları:** ayarlar açılışta doğrulanır — zorunlu ayar eksikse uygulama açılmaz; ürün sabitleri kodda tek bir yerdedir ve ayar dosyası onları değiştiremez; Teknik Mimari'nin tasarım değerleri kodda sabittir — yalnız `05 §8.4.5`'in saydıkları (hız sınırı, durum ucunun eşikleri, kayıt düzeyi) varsayılanla kodda, değiştirilebilir olarak kurulum ayar dosyasındadır; kurulum tarafının işlemleri uygulama imajındaki yönetim komutlarıdır ve web'den çağrılamaz; komut süreci arka plan çalıştırıcılarını — zamanlayıcıyı, nabzı, iş kuyruğunu — başlatmaz ve kişisel veriyi — ad, e-posta — komut satırı argümanından değil etkileşimli girişten okur (`05 §8`, §8.5.1; K-971); dönüş denetimi komutu `02 §3.34.8`'in kayıtlarının tablolarını ve gelen bildirim kaydını kodda tek bir listeden okur ve dönüş noktasından sonra kesinleşen işlemlerin eklediği ya da güncellediği satırları sayar — imhanın ve silmelerin sipariş tarafına yazdığı boşaltmalar dahil (`05 §8.5.11`; K-1096). Ayar sınıflarının ve komutların kodda düzeni bu aşamanın kararıdır.
> - **K-925 · K-965 · K-981 · Göçlerin yazılışı:** göçler genişlet–daralt kuralıyla yazılır — bir sürümün göçü önceki sürümün kodunu bozmaz; kural değerleri de kapsar — yeni bir durum, tür, iş türü ya da yük biçimi önce onu okuyabilen sürümle, sonra onu yazan sürümle gelir; CI'ın göç adımı son yayımlanan sürümün imajını yeni göçlerin uygulandığı şemaya karşı açar ve geri alma yolunu o sürümün entegrasyon testlerini yeni şemaya karşı koşarak sınar — canlılık ucu yalnız açılışı sınar (`05 §8.3.9`, §8.7.3; K-981); göç iz tablosunda yalnız ekler — var olan satırın değerini değiştirmez, sütun kaldırmaz, ad değiştirmez (`05 §7.5.2`); yama sürümü göç taşımaz — tek istisnası veri bozan bir göçün düzeltmesidir (`05 §8.7.4`); büyük veri dönüşümleri göçte değil, kuyruktaki bir arka plan işiyle parça parça yapılır (`05 §8`). Her göç kendi veritabanı işlemindedir — sürümün göçlerinden biri yarıda kalırsa öncekiler kalır ve eski sürüm ara şemayla çalışır (`05 §8.7.2`); bu yüzden göç işlem dışında çalışmak zorunda olan bir adım — kilitlemeden dizin kurma gibi — içermez; hacim tabanında olağan dizin kurma işlemin içinde kısa bir kilitle biter (`05 §8.7.2`). Kuralın kodda ve incelemede nasıl denetlendiği bu aşamanın kararıdır.
> - **K-934 · Kuruşa yuvarlama:** tutarlar kodda tek bir yuvarlama işlevinden yuvarlanır ve yön açıkça yazılır — tek bir tutar en yakın kuruşa, tam yarım kuruş yukarı; parçalara orantılı bölünen tutarda parçalar aşağı, kalan kuruş en büyük parçaya; işleme giren adetlerin payı `02 §7.1.7`'nin kuralıyla (`02 §3.8.3`; `05 §7.2.6`). .NET'in varsayılan yuvarlaması tam yarımı çift sayıya götürür ve bu kurala uymaz. İşlevin yeri ve sınanması bu aşamanın kararıdır.
> - **K-935 · K-936 · K-941 · K-942 · K-973 · Form ve site güvenliği, dış çağrılar:** vitrinin ve panelin bütün POST istekleri çatının sahteciliğe karşı korumasından varsayılan olarak geçer — varsayılan, tarayıcıdan POST alan her ucun sayfa işleyicisi olarak ya da doğrulama başarısızken isteği kesen genel bir doğrulama süzgecinin altında yazılmasıyla sağlanır — yalnız işaretleyen ara katman yetmez —, parçalı yüklemede doğrulama değeri istek başlığındadır; istisna imzalı makine uçlarıdır — bugün ödeme sağlayıcısının bildirim ucu — ve, sağlayıcı dönüşü POST'la yapıyorsa, durum yazmayan ödeme dönüşü ucu (`05 §6.5.1`) — sağlayıcı PayTR'dir ve dönüş GET'tir, bu ikinci istisna doğmaz (`08 §2.1.4`; K-1255 (1)); firmanın uzun metinleri `02 §3.11.6`'nın kapalı biçim setinin izinli etiket listesiyle süzülür ve bağlantılar yalnız `https`, `http` ve `mailto` taşır; her yanıt içerik güvenlik politikasını taşır — sunucunun yazdığı satır içi stil bloğu (marka rengi) her yanıtta üretilen tek kullanımlık değerle izinlidir — değer yalnız bloğu kapsar, renk bir öğenin stil niteliğiyle yazılmaz (`05 §6.5.4`) —, yapılandırılmış verinin veri bloğu izin gerektirmez; form gönderimi ve onu izleyen yönlendirme yalnız sitenin kendisine, ödeme sağlayıcısının adresine ve Google'ın yetkilendirme adresine gider; bağlantı katmanının her dış çağrısı sürelidir (kullanıcının beklediği çağrıda 15, kuyruktaki çağrıda 30 saniye) ve HTTP katmanında kendiliğinden yeniden deneme yoktur (Teknik Mimari `05 §6.5`, §10.1). Süzgecin kütüphanesi, sahteciliğe karşı korumanın sayfa işleyicisi olmayan uçlarda kurulması, politikanın ve tek kullanımlık değerin kodda üretilmesi ve dış çağrıların kalıbı bu aşamanın kararıdır.
> - **K-937 · K-962 · K-963 · K-964 · Veritabanı imajı ve yedek araçları:** veritabanı imajı projede derlenir — resmî PostgreSQL 18 imajının üstüne pgBackRest ve gecelik tam yedeği başlatan hafif bir zamanlayıcı eklenir; gecelik tam yedek sunucunun dışındaki depoya depo açıkça seçilerek alınır (`05 §7.6.1`); imaj veritabanının işlemlerin kesinleşme anını tutmasını açar (`track_commit_timestamp` — dönüş denetimi, `05 §8.5.11`); pgBackRest'in resmî konteyner imajı yoktur (bilgim tarihli, D-01'de doğrulanır); tam yedeğin sonucu yerel soketle veritabanındaki durum kaydına yazılır. Yedek aracı imajı dosya yedeği aracını ve gecelik zamanlamasını taşır; aracın ölçütü ücretsiz ve açık lisans, S3 uyumlu hedef, istemci tarafı şifreleme, süreli anlık görüntü saklama, düşen anlık görüntünün verisinin aynı çalışmada depodan silinmesi — kullanılmayan veri bırakmaz; restic'in varsayılan budaması kullanılmayan verinin bir payını bırakır, pay ayarla sıfıra çekilir (bilgim tarihli; `05 §7.6.6`; K-964) — ve veriyi okuyarak bütünlük denetimidir (`05 §2.2.9`, §7.6.6, §7.6.8) — aday restic (bilgim tarihli); sonucunu uygulamanın salt okuma okuduğu bir durum dosyasına yazar (Teknik Mimari `05 §2.2.9`, §8.2.2, §9.4.10). İki imajın derleme dosyaları, zamanlayıcının biçimi, durum dosyasının biçimi, dosya yedeği aracının seçimi ve budamasının ayarı bu aşamanın kararıdır.
> - **Dizin — Aşama 4 kararlarının bu dokümana devirleri:** karar kaydında K-855…K-979'un etki sütunlarında bu dokümanı gösteren kırk dört atıf (kırk dört karar) ve Teknik Mimari'nin (`05`) gövdesinde bu dokümana iş bırakan cümleler [`CHECKPOINT_REPORTS/PHASE4_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE4_CONFLICT_SCAN.md) §6'dadır; Aşama 4'ün atıflarının hepsi işin adını taşır. Aşama 4'ün checkpoint'inin kararları (K-980…K-988) tarama raporunda yoktur — bu dokümana bıraktıkları iş yukarıdaki satırlarda etiketiyle durur. K-977'nin satırı Aşama 1 bloğunun K-17 satırıdır. Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Önceki aşamaların dizinleri: `PHASE1_CONFLICT_SCAN.md` §6, `PHASE2_CONFLICT_SCAN.md` §6, `PHASE3_CONFLICT_SCAN.md` §6.

> **Aşama 5'ten park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §14.2, yazım turu ve kapanış sırasının 2. maddesi).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-1010 · K-1038 · İlişkilerin silme davranışı:** çatının zorunlu ilişki varsayılanı (basamaklı silme) kullanılmaz; her ilişkinin silme davranışı — sahibiyle silinen bağlı kayıtta basamaklı, geçmişi taşıyan kayda ve yönetici hesabına giden bağda boşaltma, başka her bağda kısıtlayıcı — modelde açıkça yazılır (Veri Modeli `06 §2`; Teknik Mimari `05 §7.5.1`). Kısıtlayıcı bağ veritabanında `NO ACTION`'dır — denetim deyimin sonundadır, sahibinin basamaklanmasıyla aynı deyimde silinen bağlı kayıt kısıta takılmaz; `RESTRICT` kullanılmaz (`06 §4`'ün girişi; K-1038). Kalıbı ve çatıdaki eşlenişi bu aşamanın kararıdır.
> - **K-1011 · K-1012 · K-1017 · Anahtar, adlandırma ve enum:** birincil anahtar uygulamanın ürettiği sürüm 7 UUID'dir; tek istisnası çatının veri koruma paketinin varlığıdır (`06 §2.3.1`, §4.68; aşağıdaki K-1035 · K-1068 · K-1082 · K-1098 satırı); tablo ve sütun adları küçük harf ve alt çizgiyle, kod adından türer, tablo adı çoğuldur, dizin ve kısıt adları çatının kuralıyla verilir; enum veritabanında kod adıyla metin olarak durur ve sütun izinli değerlerin kısıtını taşır (`06 §2`). Üretimin, adlandırma kuralının — kütüphane ya da kendi kuralı, kütüphane ölçütü K-867 — ve dönüşümün çatıdaki uygulanışı bu aşamanın kararıdır.
> - **K-1013 · K-1014 · K-1015 · K-1016 · K-1040 · K-1047 · K-1089 · K-1119 · Türler, sürüm, sayaçlar ve karşılaştırma:** para `decimal`; oranların tek ölçeği iki ondalıklı yüzdedir (20 = %20); sürüm değeri açık bir tamsayı sütunudur ve kaydı yazan güncellemede artar — siparişin sürümünü artıran ve artırmayan yazımların listesi `06 §2.6.4`'te, hangi kayda yazan işlemin artırdığı `06 §4.17`'dedir; sürümü artırmayan sistem yazımları — kalemin dosya bağının yazımı (ürün ya da varyant silinirken ve dosya yerine yenisi konmadan kaldırılırken son dosya, düzleme yeni dosya bağlanırken yeni dosya), ürün ve varyantın silinmesinin kalemlere ve ayırmalara öteki yazımları, müşterinin indirmesi, hesabın silinmesinin ve hesaba bağlanmanın siparişe yazımı — `06 §2.6.4`'te tamdır (K-1047, K-1119); ayrılmış adet ve kupon hakları yalnız geçiş biriminden yazılır; ürün adının arama biçimi ve kimlik karşılaştırmasının normalleştirilmiş sütunu — e-posta, kupon kodu, stok kodu — değeri yazan tek işlevden geçer ve normalleştirme ("I", "İ", "ı", "i" tek harf; dilden bağımsız küçük harf) tek bir işlevdedir (`06 §2`); kategorinin düzeyi kategoriyi yazan tek işlevden — taşımada taşınan alt ağacın satırlarıyla aynı işlemde — yazılır; oluşturma ve taşıma ağacın tamamına bağlı tek bir danışma kilidini işlemin başında alır (`06 §2.7.1`, §4.9; `05 §7.3.5`; K-1040 (4), K-1089). Kalıpların kodda uygulanışı bu aşamanın kararıdır.
> - **K-1021 · Bölüm başına sürüm:** firmanın ayarları tek satırlık `CompanySettings` kaydında bölüm bölüm sütunlardır ve her bağımsız kaydedilen bölüm kendi sürüm sütununu taşır; bölümün kaydı yalnız kendi sütunlarını ve sürümünü koşul olarak yazar, öteki bölümlerin sürümü kaydı durdurmaz (Veri Modeli `06 §4`; K-1014). Bir tabloya birden çok sürüm sütununun çatıda eşlenişi — tablonun bölüm bölüm ayrı varlık türleriyle eşlenmesi ya da başka bir kalıp — bu aşamanın kararıdır.
> - **K-1026 · K-1122 · Kapalı metin biçimi setinin temizlenmesi:** uzun metin — ürün açıklaması, kurumsal içeriğin uzun metinleri, aydınlatma metni ve çerez politikası — izinli etiketlerle sınırlı, yazımda sunucuda temizlenmiş HTML olarak saklanır: kalın, italik, madde listesi, bağlantı, tek düzeyli ara başlık; bağlantının adresi yalnız `http`, `https` ve `mailto` olabilir; temizleme değeri yazan tek işlevdedir ve izin listesi yalnız genişler — gösterimde de aynı liste süzdüğü için daraltma yayımlanmış metnin gösterimini değiştirirdi (`06 §2.10.2`; K-1017 (3)'ün kalıbı); gösterimde aynı izin listesiyle yeniden süzülür — saklanan değer yeniden yazılmaz, yayımlanmış yasal metin sürümü dahil (Veri Modeli `06 §2.10.2`; `05 §6.5.3`; `02 §3.11.6`; K-1122). Temizleyicinin seçimi — kütüphane ölçütü K-867 — ve izin listesinin kodda tek yeri bu aşamanın kararıdır.
> - **K-1027 · Belgenin kişisel verisiz kopyası:** onay anında Ön Bilgilendirme Formu ile Mesafeli Satış Sözleşmesi sürümün metninden ve siparişin donan değerlerinden üretilir ve müşterinin kişisel alanları yer tutucu olarak kalır; sipariş sayfası, panel, B-1'in gövdesi ve panelden yeniden gönderim yer tutucuları siparişin onay anındaki değerleriyle — düzeltilmişse adres ve e-posta geçmişinin ilk eski değeriyle — doldurur (Veri Modeli `06 §4`; `02 §3.23.3`, §3.33.6). Kopyayı üreten ve yer tutucuları dolduran işlevin tek yeri ve ürünle gelen şablonun sürüm kimliği bu aşamanın kararıdır.
> - **K-1067 · İşlem izinin işlem kod adları:** iz satırı işlemi metin olarak, kod adıyla taşır (`Operation`); iz yalnız eklenen bir tablodur ve satırları göçle dönüştürülemez — bir kez verilen kod değişmez (Veri Modeli `06 §4.63`; `05 §7.5.2`; K-965). Her yönetici işleminin (`02 §10.3.1`'in sekiz alanı) kod adının tek listesi ve kodda tek yeri bu aşamanın kararıdır. İşlem izinin imha fonksiyonu (`dispose_expired_audit_logs` — sahibi göç kullanıcısı, `SECURITY DEFINER`, sabit arama yolu, çağırma yetkisi yalnız uygulamanın kullanıcısında; süre gövdededir ve değişirse göçle değişir — `06 §4.63`; `05 §7.5.1`; K-1067 (7)) veritabanında tanımlıdır; gövdesinin göçteki yazım kalıbı da bu aşamanın kararıdır.
> - **K-1035 · K-1068 · K-1082 · K-1098 · Veri koruma paketinin tablosu, kalıcılığı ve şifresizliği; yedeğin durum kaydının yazarı:** anahtar halkası çatının veri koruma paketinin kendi varlık türüyle tutulur; Veri Modeli onu paketin tamsayı anahtarıyla ve oluşturulma anı olmadan yazdı — K-1011'in UUID kuralının ve `06 §2.8.4`'ün tek istisnası (`06 §4.68`; bilgim tarihli). Paketin bugünkü sürümünün anahtar türü ve sütunları, süresi dolan anahtarı halkadan silmediği (`06 §4.68`'in kalıcılık okuması; K-1035 (3)) ve anahtar şifreleyicisi kurulmadığında anahtarı `Xml`'de şifresiz yazdığı (`06 §4.68`; `05 §7.6.3`; K-1082) **doğrulanır**; ayrışırsa istisna ve okuma Veri Modeli'nde güncellenir; salt okuma kullanıcısının yetkisinden tablonun dışarıda bırakılışı (`05 §7.1.3`) ve adlandırma kuralının (`06 §2.4`) paketin varlığına uygulanışı bu aşamanın kararıdır. Yedeğin durum kaydını (`06 §4.67`) veritabanı konteynerindeki zamanlayıcı, yalnız o satırı güncelleyebilen ayrı bir yerel soket kullanıcısıyla yazar; pgBackRest ve tam yedeği başlatan bağlantı kurucu kullanıcıyla kalır (`05 §7.1.3`, §3.2.5; K-1098 (4)). Kullanıcının ve yetkisinin biçimi bu aşamanın kararıdır.
> - **K-1070 · Sadeleştirme işlevi:** sayfa adresi kaydın adından tek ortak sadeleştirme işleviyle üretilir — site içi aramanın ve önerilen stok kodunun kullandığı işlevle aynıdır (`05 §3.7.1`); değer yalnız küçük ASCII harf, rakam ve tekli tirelerden oluşur ve ad sadeleşince boş kalırsa kök kaydın türünün adıdır (Veri Modeli `06 §4.72`; `02 §3.30.2`). İşlevin kodu, tekil dizin çakışmasında sıradaki sayıyla yinelemenin kalıbı ve kodda tek yeri bu aşamanın kararıdır.
> - **K-925 · K-1121 · Daraltma göçünün ön denetimi:** büyük veri dönüşümünün dokunduğu eski yapıyı kaldıran göç önce dönüşecek satır kalmadığını denetler ve kalmışsa hata verir (`05 §8.7.5`); bitişin veriden okunması — dönüşmemiş satır sayısı — kod kalıbıdır, iş satırı tamamlandıktan sonra gecelik temizlikte silindiği için iz olamaz; dönüşüm işinin satırı ilerlemesini kendi kaydında ya da iş satırının bağıyla taşır — biçimi Kodlama Kılavuzu'nundur.
> - **K-1124 · Trigram dizininin eklentisi:** ürün aramasının trigram dizini PostgreSQL'in `pg_trgm` eklentisine dayanır. Eklentiyi ilk kurulumda veritabanının kurucu kullanıcısı kurar; şema sahibi kullanıcısı veritabanında yeni yetki almaz (Teknik Mimari `05 §7.1.3`; Veri Modeli `06 §4.5`, §7.5.1, §8.5.2; kurulum adımı `DEPLOY_RUNBOOK`'un K-1124 satırında). Göç demetinin eklentiyi bildirmesi (göç, kurucu kullanıcının kurduğu eklentiye dayanır) ve dizinin operatör sınıfı bu aşamanın kararıdır.
> - **K-1095 · İç notun imha edilmiş siparişe yazılmaması:** iç not yazımı siparişin satırını paylaşımlı kilitle (`FOR SHARE`) okur ve iletişim e-postası boşsa (sipariş imha edilmişse) yazmaz. Paylaşımlı kilit siparişin sürümünü artırmaz ve imha parçasının satır kilidiyle çakışır; yazım imhanın kesinleşmesini bekler (Veri Modeli `06 §4.20`, §9.2.1; `02 §10.4.7`). İki kilidin PostgreSQL'in bugünkü sürümünde çakıştığı **doğrulanır** (bilgim tarihli); ayrışırsa Veri Modeli güncellenir. Kalıbın kodda uygulanışı bu aşamanın kararıdır.
> - **Dizin — Aşama 5 kararlarının bu dokümana devirleri:** karar kaydında K-992…K-1136'nın etki sütunlarında bu dokümanı gösteren otuz dört atıf (otuz dört karar) ve Veri Modeli'nin (`06`) gövdesinde bu dokümana iş bırakan cümleler [`CHECKPOINT_REPORTS/PHASE5_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE5_CONFLICT_SCAN.md) §6'dadır; Aşama 5'in atıflarının hepsi işin adını taşır. Üç karar etki sütununda "park gerekmez" der (K-1034, K-1044, K-1088); beşi bu dokümanın Aşama 1 ve 4 park satırlarını güncelledi — iş o satırlarda etiketiyle durur. `06`'nın "bilgim tarihli" işaretlerinin doğrulama yeri bu bloğun K-1035 · K-1068 · K-1082 · K-1098 ve K-1095 satırlarıdır. Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Önceki aşamaların dizinleri: `PHASE1_CONFLICT_SCAN.md` §6, `PHASE2_CONFLICT_SCAN.md` §6, `PHASE3_CONFLICT_SCAN.md` §6, `PHASE4_CONFLICT_SCAN.md` §6.

> **Aşama 6'dan park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §16.2, yazım turu, kalite döngüsü ve kapanış sırasının 2. maddesi).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-1162 · K-1163 · K-1164 · Adresler, sayfa işleyicileri ve kimlik şemaları:** ilk yol bölümünün kapalı listesi ve kayıt türlerinin önekleri API Tasarımı'ndadır; eşleme yolun harf büyüklüğüne ve sondaki eğik çizgiye duyarsızdır, erişim anahtarı, hesap bağlantısının anahtarı ve indirme bağlantısının değeri harf duyarlıdır; yönlendirme yoktur. Her ekran bir Razor sayfasıdır, her POST işlemi aynı sayfanın adlandırılmış işleyicisidir (`handler`); OB-04'ün işlemleri onay işareti (`Confirmed`) olmadan uygulanmaz; GET hiçbir kaydı değiştirmez. `/panel` altı varsayılan olarak panelin şemasını, `/hesap` altı müşterinin şemasını ister; çerez adları `__Host-session`, `__Host-admin-session`, `__Host-antiforgery`, `__Host-cart`, `__Host-known-device`, `__Host-checkout`, `__Host-external-login`; sahiplik denetiminden geçmeyen istek ile var olmayan kayıt aynı 404'ü alır (API Tasarımı `07 §2`). Kalıpların kod düzeni bu dokümanın işidir.
> - **K-1165 · K-1166 · K-1168 · K-1172 · Form modelleri, gizli alanlar ve hata kodları:** her işleyici yalnız kendi girdi modelini bağlar — modelde olmayan alan yok sayılır, veri modelinin varlıkları doğrudan bağlanmaz, sunucunun türettiği değer formdan okunmaz; geri gelen gizli alanlar — sürüm (`Version` ve bölüm sürümleri), `Original` önekli gösterilen değer, onay anahtarı, yeniden doğrulamanın değeri, yüklemenin ve görselin kimliği, kalemin iç anahtarı — sunucuda doğrulanır; model adları `…Input`, `…Query`, `…Request`, `…Response`; sahteciliğe karşı doğrulama hatasında form yeni bir doğrulama değeriyle yeniden gösterilir; hata kodları İngilizce PascalCase'tir, bir kez verilen kod değişmez ve kullanıcıya gösterilmez (API Tasarımı `07 §2`, §5, §6). Bağlamanın ve doğrulama süzgecinin kalıbı bu dokümanın işidir.
> - **K-1169 · K-1171 · Hız sınırı ve dosya uçları:** genel sınır çatının hız sınırlama ara katmanında kayan penceredir — altmış saniye, altı dilim, eksen IP ve IPv6'da /64; imzası doğru bildirim ve parçalı yüklemenin form gönderimi sınırı muafiyetleri; 429'da `Retry-After` yoktur. Parçalı yükleme üç isteklidir — başlatma, `Content-Range` başlıklı parça, durum —, parça 5 MB'tır, sahteciliğe karşı değer `RequestVerificationToken` başlığındadır ve satırı silinmiş yüklemeye 404 döner, betik baştan başlar; görsel yükleme formun `UploadImage` işleyicisidir (API Tasarımı `07 §2`, §4). Ara katmanın ayarı, yükleme protokolünün sunucu ve betik kodu bu dokümanın işidir.
> - **K-884 · K-1171 · İndirme ucunun aralık yanıtları:** UC-37'nin 206 yanıtının bütünün baytlarıyla aynı dosyaya ait olması çatının dosya yanıtına bırakılır — yanıt dosya satırına bağlı güçlü `ETag` ve `Last-Modified` taşır, tutmayan `If-Range` tam dosyayı (200) alır; firma dosyayı iki aralık isteği arasında değiştirirse bozuk indirme kalan risktir (API Tasarımı `07 §4.31.1`; `02 §3.12.7`). Doğrulayıcıların dosya satırından üretilmesi ve çatının dosya yanıtına verilişi bu dokümanın işidir.
> - **K-1228 · Listelerin kesin sırası:** her sayfalı liste sorgusu ORDER BY'ı kaydın iç anahtarıyla bitirir — yeni önce sıralarda azalan, öteki sıralarda artan; galeri ve elle sıra dahil (API Tasarımı `07 §2.6.5`; `06 §4.5`, §4.47). Test: aynı `CreatedAt`'li iki ürün sayfa sınırında ne yinelenir ne kaybolur. Sorgu kalıbı bu dokümanın işidir.
> - **K-1173 · K-1174 · K-1205 · İşleyici adları ve istekteki değerin biçimi:** işleyici adı İngilizce bir fiil ve işlemin nesnesinin sözlükteki ya da Veri Modeli'ndeki adıdır, Pascal yazımıyla — nesnenin adı birebirdir; nesnesi ekranın kendisi olan hesap ve oturum işlemleri (`Register`, `SignIn`, `SignOut`, `Reauthenticate`) ile ödeme adımının iki ara işleyicisi (`Recalculate`, `Leave`) yalnız fiildir, ayrı formlar ve ayrı adreslerse ekleme ve düzenleme `Create…`/`Update…` alır, aynı formun aynı adresi `Save…` alır (K-1205, K-1226); adların listesi API Tasarımı'nın uç envanterindedir (`07 §3`; başlık notunun yazım konvansiyonu 3). Tarih alanı istekte `yyyy-AA-GG`'dir; tutar ve ondalıklı sayı Türkçe kültürle ayrıştırılır — ondalık ayırıcı virgül, binlik ayırıcı yok; istek saat taşımaz (`07 §2.9`). Bağlamanın ve ayrıştırmanın kod kalıbı bu dokümanın işidir; dizinli liste bağlama kalıbı ve boş satırın atlanması da öyle. Tutar/ondalık ayrıştırması binlik ayırıcıya izin vermez (`NumberStyles.AllowDecimalPoint` yalnız, tr-TR); nokta içeren değer reddedilir (`07 §2.9.4`). Sorgu parametresinde ayrıştırılamayan tutar süzgeci uygulamaz. Eşit sıralama anahtarında ikincil anahtar kaydın iç kimliğidir; ürün dizinine kimlik eklenir (`07 §2.6.5`).
> - **K-1183 · K-1184 · K-1186 · Hesap uçlarının kalıpları (yazım turunun 2b oturumu):** kayıttan sonraki hâlin değeri (`pendingRegistration`) bekleyen kaydın iç anahtarını taşıyan, çatının veri koruma anahtarıyla şifreli ve imzalı kayıtsız değerdir — amacı ve biçimi bu dokümanın işidir; yeniden doğrulamanın tek kullanımlık değeri yönlendirme adresinde (`reauthenticationToken`) ve işlem formunun gizli alanında taşınır, yalnız işlemi uygulayan gönderimde tüketilir; Google dönüşü (`/giris/google`) GET olduğu hâlde yazar ve isteği akış çerezinin tek kullanımlık değerleri bağlar; dönüş yolu ve yeniden doğrulamanın işlemi korumalı `state` değerinin içindedir (bilgim tarihli — doğrulanır); bir kez söylenen mesajın iki işareti (`IsMergeNoticePending`, `IsPaymentUnavailableNoticePending`) gösterildiği istekte kaldırılır; `/hesap/silindi` kaydı silinmiş oturum çerezini işaret sayar ve yanıtında siler (API Tasarımı `07 §2.2.3`, §2.2.5, §4.20–§4.25, §4.34). Kod kalıbı bu dokümanın işidir.
> - **K-1189 · K-1190 · Panelin form kalıpları (yazım turunun 3a oturumu):** davet kabulünden sonra giriş ekranına giden davetin korumalı değeri (`acceptedInvitation`) davetin iç anahtarını taşıyan, çatının veri koruma anahtarıyla şifreli ve imzalı kayıtsız değerdir — `pendingRegistration`'ın kalıbı; amacı ve biçimi bu dokümanın işidir · ürün formu betiksiz yolda bir boş seçenek boyutu girdisi, boyut başına bir boş değer girdisi ve bir boş varyant satırı üretir; boş bırakılan yeni girdi ve satır bağlanmaz · önerilen stok kodu sunucuda üretilir — sayfanın her üretiminde adı bilinen ürünün yeni varyant satırına, kayıtta stok kodu boş yeni varyanta; betik öneri üretmez (API Tasarımı `07 §9.1`, §9.2, §9.5). Kod kalıbı bu dokümanın işidir.
> - **K-1193 · K-1195 · Panelin içerik formu ve dışa aktarma (yazım turunun 3b oturumu):** kurumsal içerik formunun ilgili ürün araması aynı formun ara gönderimidir (`SearchProducts`) — formun bütün girdilerini alır, hiçbir şey yazmaz ve formu sonuçlarla yeniden üretir; Hakkımızda ve duyurunun yeni kolu o arada açılmış kayda takılınca kayıt varyantının çakışmasıdır ve yeniden gösterilen form var olan kaydın adresine ve sürümüne bağlanır · dışa aktarma dosyası önce sunucuda yedeğe girmeyen geçici bir dizinde bütünüyle üretilir, iz satırı ondan sonra ve gönderimden önce yazılır, dosya gönderimden sonra silinir; dosyanın akışla yazılması, üretimin süre ve boyut tavanı ve ters vekilin zaman aşımı bu dokümanın işidir; CSV'nin iç biçimi — sütun adları, ayırıcı, ondalık ve tarih biçimi, kodlama işareti — ve formül karakteriyle (`=`, `+`, `-`, `@`), sekmeyle ya da CR ile başlayan müşteri girdisi hücrelerinin etkisizleştirilmesi (ön ek tırnak) bu dokümanın işidir (API Tasarımı `07 §9.13`, §9.24). Kod kalıbı bu dokümanın işidir.
> - **K-1196 · K-1198 · Girdi modellerinin adları ve hata kodlarının kaydı (yazım turunun 4. oturumu):** model adı formun asıl işleyicisinin adı ve türüdür (`…Input`, `…Query`, `…Request`, `…Response`); aynı formu gönderen işleyiciler — ödeme adımının ara gönderimleri ve ayrılışı, içerik formunun `SearchProducts`'ı, yasal metnin yayına alınması — aynı modeli bağlar; aynı adlı işleyicinin vitrinde ve panelde ayrı alanları varsa panelin modeli `Admin` önekini alır; iç anahtarın alan adı varlığın adı ve `Id`'dir. Görsel yükleme (`UploadImage`) bulunduğu formun modelini bağlar ve modelin yalnız onun okuduğu iki alanını taşır — `ImageFile`, `ImageTarget` (hedef alanın modeldeki yolu); formun kaydı onları okumaz. `notice` sorgu parametresinin değeri API Tasarımı'nın kapalı listesidir — bilinmeyen değer yok sayılır, serbest metin gösterilmez. Hata kodu yanıtın gövdesine ve başlığına yazılmaz; uygulama kaydı her reddin kodunu — alan doğrulamasında her alanın kodunu — yazar, sonucu belirsiz işlemde `OutcomeUncertain` (API Tasarımı `07 §5`, §6).
> - **K-1217 · Derin gözden geçirmeden — K'sız bulgular L4a-4, L4a-7, L8-5, `AUDIT_REPORTS/07_DEEP_REVIEW.md` (arama, veri koruma amacı, gönderim sınırı):** arama sözcüğünde `%` ve `_` harfi harfine eşlenir (LIKE meta karakterleri kaçışlanır; sorgu parametreli kalır) ve sözcük ürün adının tavanıyla (P-27) kırpılır — test: `%` yazan sözcük bütün ürünleri getirmez · elle yazılan veri koruma kullanımlarının (indirme imzası, `pendingRegistration`, dış girişin `state`'i) amaç dizgesi ayrıdır — test: bir amacın değeri öteki amaçta çözülmez · e-posta gövdesinde müşteri metni kaçışlanır; sipariş e-postasının alıcı başına gönderim sayacı `08`'in e-posta gönderim tasarımında çözülür (sipariş engellenmez) · canlılık ucunun veritabanı denetimi sabit kısa bir süreyle sınırlıdır, dolarsa 503 (`07` UC-81; değer kurulum ayarı).

> **Aşama 7'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §18.1, yazım turunun 5. oturumu, `ENT5-02`).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-1257 · K-1258 · Dış servis çağrılarının kalıpları (Entegrasyon Spesifikasyonları `08 §3.1`, §3.2):** bağlantı katmanı servis başına bir arayüzdür; modül yalnız ürün düzeyindeki işi çağırır ve sağlayıcının adı yalnız ayar anahtarlarında ve katmanın kendi adlarında geçer · her dış çağrının zaman aşımı kodda sabittir — kullanıcının beklediği çağrıda 15, iş kuyruğundan giden çağrıda 30 saniye — ve katman kendiliğinden yeniden denemez (`05 §10.1.1`; MK-61) · sonuç sınıfları katmanın dönüş tipidir (ödemede başarı ve bağlantı hatası; sorguda sonuç var, ödeme yok, yanıt alınamadı; iadede başarılı, gerçekleşmedi, sonucu bilinmeyen; e-postada gönderildi, alıcı hatası, kanal hatası) ve hiçbiri istisna olarak dışarı sızmaz — servis tablosunda olmayan her yanıt sınıfa eşlenir ve kayda yazılır (`08 §2.1.8`, §2.2.2) · imza ve tutar doğrulaması bağlantı katmanındadır ve işleyiciden önce koşar; imzası tutmayan istek kayda yazılmaz (`08 §3.2`) · servisin yanıtında alan eşlemesinin saymadığı alan okunmaz ve uygulama kaydına yazılmaz; kayda hata nedeni yazılır, istek alanlarının değeri ve anahtarlar yazılmaz · ödeme sağlayıcısına sipariş numarası tiresiz verilir (yalnız harf ve rakam, en çok 64 karakter) ve tutar çağrıya göre dönüşür — başlatmada tutar × 100 tam sayı, bildirimde kuruştan ondalığa, iadede ondalık nokta, sorguda ondalık ayracı virgül ya da nokta olan metin (ayrıştırma kuralı `08 §2.1.6`, DV-12) (`08 §2.1.4`…§2.1.9); her dönüşüm tek işlevdedir ve yuvarlamaz — karşılaştırma tam sayı kuruş üzerindedir, ikiden fazla ondalık haneli yanıt tanınmaz (`08 §2.1.5`, §2.1.6); kuruşa yuvarlama kuralı (K-934) tutar hesabındır · kargo takip numarası bağlantı kalıbına yüzde kodlanarak girer (`08 §2.4`) · yerli üretim logosu ürünle gelen dosyadır, kod onu ayardan okumaz (`08 §4.3`).
> - **K-1245 · K-1272 · K-1281 · K-1292 · Google kimlik belirtecinin doğrulanması (Entegrasyon Spesifikasyonları `08 §2.3`):** kod değişiminin adresi sabittir, keşiften okunmaz; keşif belgesinden yalnız `issuer` ve `jwks_uri` alınır, çatı keşif belgesini ve anahtar kümesini önbelleğe alır ve her girişte yeniden çekmez — çatının işleyicisi keşif belgesini varsayılan olarak başlatmada çekiyorsa adresler sabit olarak yapılandırılır; önbelleğin tazeleme biçimi çatının davranışıdır ve ilk bağlanan task'ta doğrulanır (bilgim tarihli; `08` DV-31) · `exp` için saat sapması toleransı çatının ayarıdır ve değeri bu dokümanın işidir; `auth_time` toleranssız karşılaştırılır · `userinfo` çağrılmaz; kimlik belirtecinin alanlarından yalnız alan eşlemesinin saydıkları okunur (`hd` dahil — Google'ın adres için yetkili olup olmadığı buradan okunur) ve `sub` saklanmaz (K-1061).
> - **K-1027 · K-1271 · Değişken metnin kaçışı (Entegrasyon Spesifikasyonları `08 §2.2.6` — B-1'in iki yasal metninin yerleşimi, madde 4; §2.2.4):** e-postaya ve belgeye giren her değer — firmanın ayarı, kalemin adı, yer tutucunun müşteri değeri — HTML'de kaçışlanır, düz metinde satır sonu karakterleri boşluğa çevrilir; metin ham HTML olarak basılmaz, biçim yalnız şablonun kendi yapısıdır ve bağlantı yalnız `https`, `http` ve `mailto` şemasını taşır (`05 §6.5.3`) · değerin donmuş kopyada kaçışlı mı saklandığı, gösterimde mi kaçışlandığı bu aşamanın kararıdır; sipariş sayfasındaki gösterim aynı kuralı izler (K-1027 (4), (5)).

> **Yazım durumu (K-28, K-30, K-432; karar kaydı §20.1): yazım turu sürüyor (v0.3).** Doküman Kodlama kılavuzu aşamasının **tek yazım turunda**, altı oturumda yazılır (K-1302 (1)); 1. oturum yöneticinin kararıyla baştan ikiye bölündü (`INSTRUCTIONS §9` (8)). **Yazıldı:** Y-1a — başlık notu, §1, §2, §3 (K-1329, K-1330); Y-1b — §4, §5, §6 (K-1331, K-1332). **Sırada:** 2. oturum §7–§9, 3. oturum §10, 4. oturum §11–§14, 5. oturum §15–§18, 6. oturum §19–§25 (alt bölüm haritasının "Oturum" sütunu). Workshop tamamlandı (K-1305…K-1328). Özet ↔ ayrıntı taraması ve kalite döngüsü yazım turundan sonra gelir.
>
> **Park maddelerinin durumu (K-1302 (6)).** Park blokları talimattır ve yazımda değişmez; kapanışları burada izlenir. Bir park maddesi birden çok bölüme dokunuyorsa kural ilk konunun bölümünde yazılır, öteki bölümler ona atıf yapar. **Kapandı:** P1.1 (terim sözlüğünün devralınması — §5.1); P4.1 (dil, çatı ve kütüphane ekleme — §3.1, §3.2, §3.3 ve Kütüphaneler tablosu). **Kısmen kapandı:** P4.2 — sayfaların düzeni §4.1'de yazıldı, ön yüz dosya düzeni §16'da; P4.3 — modül sınırlarının korunması ve dosya deposu arayüzü §6'da yazıldı, önbellek ve kuşak sayacının kalıbı §10.4'te; P4.5 — mimari testle korunan kısım §6'da yazıldı, geçiş birimi ve iş kuyruğunun kalıbı §9 ve §13'te; P4.8 — dosya düzeni ve test projelerinin ayrımı §4.1 ve §4.3'te yazıldı, test araçları §19'da; P4.9 — ayarlar ve ürün sabitleri §3.5'te yazıldı, yönetim komutları §13'te; P4.13 — iki imajın derleme dosyalarının yeri §4.3'te yazıldı, içerikleri §17'de; P5.2 — adlandırma kuralının uygulanışı §5.2'de yazıldı, anahtar üretimi ve enum §10.1'de; P5.8 — veri koruma paketinin varlığına adlandırmanın uygulanışı §5.2'de yazıldı, kullanıcıların ve yetkisinin biçimi §10.8'de; P6.1 — işleyici adları ve sayfa düzeni §5.3 ve §4.1'de yazıldı, kimlik şemaları §15.1'de; P6.2 — girdi doğrulama süzgeci §3.4'te, model ve işleyici adları §5.3'te yazıldı, hata kodlarının kaydı ve gösterilmemesi §8'de; P6.6 — işleyici adları §5.3'te yazıldı, istekteki değerin biçimi §7.2'de; P6.10 — model ve hata kodu adları §5.3'te yazıldı, hata kodlarının kaydı §8.2'de. **Açık:** öteki yirmi altı madde — yazılacakları bölümün oturumunda kapanır.

**Yazım konvansiyonları (K-1302, K-1329).** Bu on madde §1–§25'in bütün yazım oturumlarını bağlar; "yazım konvansiyonu n" diye anılır.

1. **Bu doküman ürün kuralı koymaz.** Kodun **nasıl** yazıldığını söyler: dil ve çatı kalıpları, düzen, adlandırma, sınırların kodda korunması, hata, veri erişimi, güvenlik kalıplarının kullanımı, test, git ve CI, yasaklar; kütüphane seçimlerini ve kılavuzun koyduğu sayısal değerleri yazar. Davranış `02`'de, ekranın davranışı `04`'te, mekanizma `05`'te, alan ve değer adları `06`'da, uçlar `07`'de, dış servisin biçimi `08`'dedir. Yazılırken eksik çıkan kural bir **boşluktur:** uydurulmaz, karar kaydında bir K satırıyla karara bağlanır; kaynak dokümana dönüyorsa aynı PR'da yazılır ve §2'nin geri besleme tablosuna satır girer (K-652).
2. **Adlar sözlükten ve Veri Modeli'nden birebirdir** (`02 §1`; `06 §4`, §5; K-17): sembol, dosya ve tip adları sözlüğün İngilizce karşılıklarıdır; alan, tablo, kapalı değer ve işlem adları `06`'dadır ve her biri yazılırken `06`'da aranır. Bulunmayan ad yazılmaz — konvansiyon 1'in boşluğudur. Dış servisin adları (alan, parametre, değer) servisin yazılışıyla ve kod biçiminde yazılır (`08`).
3. **Dış olgu "bilgim tarihli" yazılır ve doğrulanır.** Bir çatının, paketin ya da aracın API'sinden, ayar adından, sürümünden okunan her değer bilgim tarihlidir (2026-10-29): işaretlenir ve yerine göre yazım oturumunda belgeye karşı doğrulanır ya da "ilk bağlanan task'ta doğrulanır" der (§1, ilke 5). Okunamayan olgu tahmin edilmez.
4. **Kimlikler** (K-1302 (3)): bu doküman yeni bir kural kimliği ailesi açmaz — kural alt bölüm numarasıyla anılır ("`09 §3.3`"), inceleme listesi (§25), Implementation Planı ve Doğrulama Protokolü bölümü gösterir. Konu kimliği `KKn-mm` ve park maddesi kısaltması `Pa.n` yalnız karar kaydında kullanılır; bu dokümanın gövdesine girmez.
5. **Atıf biçimi** (K-701): önsüz `§` bu dokümanı gösterir; başka dokümana atıf numarasıyla yazılır (`02 §3.20.8`, `05 §6.1.4`); `0X §` ile başlayan dizideki sonraki önsüz `§`'ler o dokümanı gösterir — dizi ` · `, `;`, parantezin kapanışı ya da cümle sonuyla biter. Karar kaydının bölümleri "karar kaydı §n", bir kararın kolu "K-1305 (2)" diye anılır.
6. **Kod örneklerinin biçimi** (K-1302 (5)): örnek kısa bir C# parçasıdır (gerektiğinde JavaScript, CSS ya da SQL); **adlarını sözlükten, `06`'dan ve `07`'den birebir alır — örnek için ad uydurulmaz;** kalıbı gösterir, sözleşmeyi göstermez; yasak kalıpta "yanlış" ve "doğru" biçim yan yana verilir. Çatı API'sinin adı bilgim tarihliyse yazım oturumu belgeye karşı doğrular; doğrulanamayan ad parçada `// bilgim tarihli — ilk task'ta doğrulanır` yorumuyla işaretlenir.
7. **Kural ayrıntısı evinde kalır** (`checklists/document-stage.md` §4): bu doküman başka bir dokümanın kuralına dayandığında ayrıntıyı kopyalamaz, bölüm numarasıyla işaret eder. **Tek ev** (K-1302 (6)): her kuralın bu dokümanda tek yeri vardır — başka bölüm kuralı yinelemez, atıf yapar. **Kütüphane yalnız §3'ün Kütüphaneler tablosunda seçilir;** kuralı kullanan bölüm tabloya atıf yapar.
8. **Üst dokümanlara geri besleme:** bu dokümandan `02`–`08` ve `10`'a dönen her fark §2'nin sonundaki numarasız tabloya bir satırla girer (K-652; konvansiyon 1). Kaynak doküman kazanır; fark kaynakta düzeltilir.
9. **Açık kalem ve devir:** bu dokümanın şablonunda açık kararlar bölümü yoktur — açık kalem karar kaydının §4'ünde A-19'dan sürer; sonraki dokümana — Implementation Planı (`11`), Doğrulama Protokolü (`12`), `DEPLOY_RUNBOOK` — talimat bırakan karar o dokümanın "Aşama 8'den park edilen girdiler" bloğuna girer, blok yoksa açılır (K-36). Henüz yazılmamış bir bölüme bağlanan atıf hedef bölümü ve oturumunu taşır — "§10 (yazım 3)" —; o bölümü yazan oturum atfı aynı PR'da madde numarasına çevirir (K-682).
10. **Özet ile ayrıntı birlikte değişir; ölü placeholder ve belirsiz ifade yoktur.** Başlık notunun tabloları, §3'ün Kütüphaneler ve süre tabloları ile dokümanın kapalı listeleri ayrıntının özetidir; bir ayrıntıyı değiştiren her düzeltme onu anan özet yerlerini betikle bulur ve aynı PR'da okur. Yazılmamış bölüm bir kapıya bağlıdır ve kapısını adıyla taşır (alt bölüm haritasının "Oturum" sütunu). "Muhtemelen", "belki", "gereksinime göre" yazılmaz.

**Alt bölüm haritası (K-1302, K-1329).** Şablonun §1…§25 numaraları değişmez; alt düzey aşağıdaki tabloyla sabittir. Bir oturum bir alt bölümü bölmek ya da birleştirmek zorunda kalırsa bu tabloyu ve ona atıf yapan satırları aynı PR'da günceller (K-670'in kalıbı; `checklists/document-stage.md` §4).

| Bölüm | Alt bölümler | Oturum |
|---|---|---|
| Başlık notu | Yazım durumu · park maddelerinin durumu · yazım konvansiyonları (1–10) · alt bölüm haritası · okuma tablosu | Y-1a ✓ |
| §1 Temel ilkeler | Beş ilke (alt bölümsüz) | Y-1a ✓ |
| §2 Source of truth hiyerarşisi | Kaynak tablosu · çelişkide işleyiş · Üst dokümanlara geri besleme (numarasız) | Y-1a ✓ |
| §3 Genel kod yazım kuralları | 3.1 C# standartları ve derleme ayarları · 3.2 Paket sürüm yönetimi · 3.3 Kütüphane eklemenin işletimi · 3.4 Girdi doğrulamanın kalıbı · 3.5 Ayarlar, ürün sabitleri ve süre ve tavan değerleri · Kütüphaneler (numarasız tablo) | Y-1a ✓ |
| §4 Proje yapısı ve klasör organizasyonu | 4.1 Çözüm ve proje düzeni · 4.2 Klasör düzeninin `CONTEXT.md` §3'e yazılışı · 4.3 İmaj, Compose ve geliştirme ortamı dosyalarının yeri | Y-1b ✓ |
| §5 Adlandırma konvansiyonları | 5.1 Sözlüğün devralınması ve kod adları · 5.2 Veritabanı adlandırma kuralının uygulanışı · 5.3 Sayfa, işleyici, model, hata kodu ve çerez adları | Y-1b ✓ |
| §6 Mimari sınırlar | Modül sınırlarının kodda korunması ve dosya deposu arayüzü (alt bölümsüz) | Y-1b ✓ |
| §7 Zaman ve biçim standartları | 7.1 Tek zaman kaynağı · 7.2 İstekteki tarih, tutar ve ondalık biçimi | 2 |
| §8 Hata yönetimi | 8.1 Tek yakalama katmanı, üç hâl, sonucu belirsiz işlem · 8.2 Hata kodlarının kaydı ve gösterilmemesi | 2 |
| §9 Domain kuralları | 9.1 Geçiş birimi, beyaz liste, yan etki · 9.2 İşlem izinin kod adları · 9.3 Belgenin kişisel verisiz kopyası · 9.4 Tek işlevler | 2 |
| §10 Veri erişimi | 10.1 Silme davranışı, anahtar, enum · 10.2 Sürüm, eşzamanlılık, sayaç · 10.3 Bölüm sürümünün çatıda eşlenişi · 10.4 Önbellek ve kuşak sayacı · 10.5 Liste sırası, sayfalama, arama sözcüğü · 10.6 Göçlerin yazılışı · 10.7 Veritabanı oturumunun süreleri · 10.8 Veritabanı kullanıcıları · 10.9 İç notun kilidi · 10.10 Dışa aktarma dosyası | 3 |
| §11 Entegrasyon kuralları | 11.1 Bağlantı katmanının kalıbı · 11.2 Dış servis istemcileri · 11.3 Google kimlik belirteci · 11.4 E-posta ve belge metninin üretimi | 4 |
| §12 Sözleşme ve olay sürümleme | Değerlerin sürümlenmesi (alt bölümsüz) | 4 |
| §13 Arka plan iş kuralları | 13.1 İş kuyruğu, yeniden deneme, nabız · 13.2 Yönetim komutları · 13.3 Büyük veri dönüşümünün ilerleme kaydı | 4 |
| §14 Hassas hesaplama kuralları | 14.1 Kuruşa yuvarlama · 14.2 Sağlayıcı tutar dönüşümleri | 4 |
| §15 Güvenlik kuralları | 15.1 Oturum, şifre, anahtar, çerez, ters vekil · 15.2 Sahtecilik koruması, içerik güvenlik politikası · 15.3 HTML temizleyici · 15.4 Hız sınırı · 15.5 Veri koruma amaçları · 15.6 Görüntü işleme · 15.7 Dosya uçları | 5 |
| §16 İstemci tarafı kurallar | 16.1 Ön yüz dosya düzeni ve adlandırma · 16.2 Betiksiz yol, form kalıpları, yükleme betiği | 5 |
| §17 Yardımcı servis / sidecar standartları | Veritabanı ve yedek aracı imajları (alt bölümsüz) | 5 |
| §18 Loglama ve gözlemlenebilirlik | 18.1 Kayıt kütüphanesi ve yazıcılar · 18.2 Kayıt alanları, maskeleme, kategoriler | 5 |
| §19 Test kuralları | 19.1 Test araçları · 19.2 Test katmanları, mimari testler | 6 |
| §20–§25 | §20 Performans · §21 Git ve CI/CD (şablonun 21.1–21.5'i) · §22 Refactor · §23 Yasaklar · §24 AI çalışma kuralları · §25 Review checklist | 6 |

**Okuma tablosu (K-1302 (4)).** Bu doküman her kodlama görevinde ajana verilir (`00 §C.6`); 100 KB'ı aşan dosya bütün okunamaz (`INSTRUCTIONS §9` (7)) — dosyanın boyutu `wc -c` ile ölçülür ve **bölüm bölüm okunur** (`grep -n '^## '` ile başlıkların satırı, sonra `Read` ile satır aralığı). Her bölüm tek okumada okunabilir kalır. **Çekirdek bölümler** her görevde okunur ve toplamı 100 KB'ın altında tutulur: §1, §2, §5, §6, §23, §24, §25. Öteki bölümler görevin dokunduğu alana göre okunur; Implementation Planı her task satırında okunacak bölümleri yazar (`11` — "Aşama 8'den park edilen girdiler" bloğu yazım turunda açılır).

| Görevin dokunduğu alan | Çekirdeğe ek olarak okunur |
|---|---|
| C# kodu yazan her görev | §3.1, §3.2 |
| Kütüphane ekleyen ya da güncelleyen görev | §3.3, Kütüphaneler tablosu |
| Form, girdi modeli, doğrulama | §3.4, §8 |
| Ayar, ürün sabiti, süre ve tavan değeri | §3.5 |
| Çözüm, proje, klasör, imaj, Compose | §4 |
| Zaman, tarih, tutar biçimi | §7 |
| Hata yakalama, hata kodu | §8 |
| Durum geçişi, yan etki, belge kopyası, tek işlev | §9 |
| Veritabanı, varlık, göç, sorgu, sürüm, önbellek, dışa aktarma | §10 (yeni değer eklenirse §12) |
| Dış servis çağrısı, e-posta, belge metni, Google girişi | §11, §14 (tutar dönüşümü) |
| İş kuyruğu, arka plan işi, yönetim komutu | §13 |
| Oturum, şifre, çerez, form güvenliği, yükleme, görsel, HTML metin | §15 |
| Ön yüz (JavaScript, CSS) | §16 |
| Veritabanı ve yedek imajı | §17 |
| Kayıt (log) | §18 |
| Test yazan görev | §19 |
| Liste, sorgu sınırı, performans | §20 |
| Dal, commit, PR, CI | §21 |
| Refactor | §22 |

*Kaynak: K-1302 · K-1305, K-1306, K-1310, K-1312, K-1319, K-1322…K-1328 (yazım konvansiyonları ve §3'ün içeriği) · K-17, K-36, K-652, K-670, K-682, K-701 · `00 §C.6` · `INSTRUCTIONS §9` (7) · `checklists/document-stage.md` §4 · `08` başlık notu (kalıp).*

---

## 1. Temel ilkeler

- Somut örnekle tanımla. "Hata yönetimi yapılmalı" değil, "hata yönetimi şöyle yapılır: (somut örnek)".
- Tek kaynak, tek doğru. Bir kural iki yerde farklı anlatılmaz.
- **Kaynak doküman kazanır; kod dokümanla çelişmez.** Kod yazarken bir doküman cümlesi koda uymuyorsa kodu dokümana uydur; doküman yanlışsa sessizce kod yazma — task BLOCKED olur ve çelişki bildirilir (`00 §D.1`, §D.8; `INSTRUCTIONS §4`). Örnek: `07` bir alan hatası için `TextTooLong` kodunu veriyorsa kod `TextTooLong` olur — "daha anlaşılır" diye `TooLong` yazılmaz.
- **Ad uydurulmaz.** Enum değeri, durum adı, hata kodu, alan ve tablo adı `06` ve `07`'den birebir alınır; bu dokümanda ya da kaynaklarda bulunmayan ad bir boşluktur, uydurulup geçilmez (BLOCKED, `SPEC_GAP`). Örnek: kargo ücretinin firma ayarı `ShippingFee` ise (`06 §4.2`) kodda `ShippingCost` ya da `DeliveryFee` yazılmaz; bir kavramın tek adı vardır.
- **"Bilgim tarihli" olgu kullanılmadan doğrulanır.** Bir çatının, paketin ya da aracın API'sinin adı, ayarın adı, sürümün davranışı bilgiden geliyorsa belgeye ya da SDK'ya karşı sınanmadan koda girmez. Doğrulama sonucu PR açıklamasına yazılır; belgeden okunamıyorsa task BLOCKED olur. Örnek: bu dokümanda "bilgim tarihli" diye işaretli her ad (ör. §3.1'in `Directory.Build.props` ayar adları) ilk kullanan task'ta SDK'ya karşı derlenerek sınanır, sınanmadan kopyalanmaz.

## 2. Source of truth hiyerarşisi

Çelişki durumunda kazanan doküman konuya göre belirlenir; bu doküman kural koymaz, kalıp koyar (yazım konvansiyonu 1).

| Konu | Kazanan doküman |
|---|---|
| Ürün davranışı, iş kuralı, sözlük terimleri, sayısal parametreler | Ürün Gereksinimleri (`02`; sözlük §1, parametre envanteri §11) |
| Ekranın davranışı, form kuralları, doğrulama kuralları | Arayüz Tanımları (`04`) |
| Mekanizma — katmanlar, durum geçişi, güvenlik, ortam, dağıtım | Teknik Mimari (`05`) |
| Alan ve tablo adı, tip, kapalı değer, işlem adı | Veri Modeli (`06`) |
| Uç, adres, yöntem, zarf, hata kodu | API Tasarımı (`07`) |
| Dış servisin biçimi, e-posta şablonu | Entegrasyon Spesifikasyonları (`08`) |
| MVP'nin kapsamı | MVP Kapsamı (`10`) |
| Kodun kalıbı — kütüphane, düzen, adlandırma, test, git | Kodlama Kılavuzu (bu doküman) |
| Süreç — task akışı, kapılar, yasaklar | `00`, `.claude/INSTRUCTIONS.md`, `.claude/GUARDRAILS.md` |

**Çelişki nasıl işler.** (1) Bu dokümanın bir kuralı bir kaynak dokümanla çelişirse **kaynak kazanır:** kod kaynağa göre yazılır, fark kaynakta düzeltilir ya da bu dokümanın cümlesi düzeltilir, ve fark aşağıdaki tabloya satır olur. (2) İki kaynak doküman birbiriyle çelişirse kod yazılmaz — task BLOCKED olur (`00 §D.8`, `SPEC_GAP`); hangisinin kazandığına ajan karar vermez. (3) Doküman sessizse ve kod bir seçim gerektiriyorsa seçim uydurulmaz; boşluk olarak bildirilir (§1, "ad uydurulmaz").

#### Üst dokümanlara geri besleme

Bu dokümanın yazımında `02`–`08` ve `10`'a dönen her fark bir satırla izlenir (K-652). Satır, üst dokümanın bölümünü ve düzelen cümleyi, düzeltmenin sürümünü ve kararı taşır. Workshop'ta gelen satırlar karar kaydı §20.2'den taşındı (K-1302 (2c)); yazım turunda doğan satırlar buraya eklenir.

| Üst doküman ve bölüm | Sürüm | Karar |
|---|---|---|
| `02 §3.11.6` (paragraf ve satır sonu setin biçimi değil taşıyıcısıdır) | v0.102 | K-1318 |
| `05 §7.4.5`, §9.3.4 ("diske inmesi beklenir" — işletim sistemine teslim) | v0.37 | K-1317 |
| `05 §6.5.3` (taşıyıcı etiketler set dışı sayılmaz) | v0.37 | K-1318 |
| `06 §2.10.2` (paragraf ve satır sonu yazımda düşmez) | v0.39 | K-1318 |
| `05 §3.8.2` (genişlik sınıfı sınırlarının tek tanımı; kullanım başka dosyada olabilir) | v0.38 | K-1326 |

## 3. Genel kod yazım kuralları

### 3.1 C# standartları ve derleme ayarları

Yerleşik .NET araçlarıyla, kökte tek ayar dosyası kümesiyle ve uyarıların hata sayılmasıyla (K-1306).

1. **Üçüncü taraf çözümleyici paketi eklenmez** (StyleCop vb.); .NET SDK'nın kendi çözümleyicileri yeter — boş olabilir başvuru denetimini, kaynak kullanımı ve güvenlik kurallarını taşırlar.
2. **Kökteki `Directory.Build.props` bütün projelere uygulanır:** boş olabilir başvuru denetimi açık, örtük `using`'ler açık, çözümleyici düzeyi "son sürüm, önerilen kural kümesi", kod stili derlemede uygulanır ve **uyarılar her yapılandırmada hata sayılır** — yerelde geçen derleme CI'da düşmez (`05 §2.3.5`).

   ```xml
   <Project>
     <PropertyGroup>
       <Nullable>enable</Nullable>
       <ImplicitUsings>enable</ImplicitUsings>
       <AnalysisLevel>latest-recommended</AnalysisLevel>
       <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
       <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
     </PropertyGroup>
   </Project>
   ```
   Ayar adları ve düzey değerleri bilgim tarihlidir — ilk task'ta SDK'ya karşı derlenerek doğrulanır.
3. **Kökte tek `.editorconfig`;** biçim denetimi `dotnet format` ile "değişiklik yok" doğrulamasıdır (CI'ın Lint adımı).
4. **Stil.** Dosya kapsamlı ad alanı · özel alanlar `_camelCase` · asenkron metot `Async` soneki alır ve `CancellationToken` son parametredir · değişmez veri taşıyıcısı `record` · `var` yalnız tür sağ tarafta açıksa · sınıflar varsayılan `sealed`.

   ```csharp
   namespace Shopfolio.Modules.<Modül>;                       // dosya kapsamlı ad alanı (K-1307)

   private readonly ILogger<Product> _logger;                 // özel alan: _camelCase
   public async Task<Product?> FindAsync(Guid id, CancellationToken ct)   // Async soneki; ct son parametre
   ```
   Değişmez veri taşıyıcısı `record`'dur; `var` yalnız sağ taraf türü açıkça söylüyorsa (`var product = new Product();`) kullanılır, `var x = Get();` yazılmaz.
5. **Yasaklar.** Null-forgiving `!` (gerekçe yorumuyla tek satır istisna) · `async void` (olay işleyici hariç) · `dynamic` · uyarı bastırma yalnız gerekçe yorumuyla ve en dar kapsamda.

   ```csharp
   // Yanlış: derleyicinin uyarısı susturuldu, açıklama boşsa çalışma anında düşer.
   var description = product.Description!;

   // Doğru: boş durum açıkça ele alınır.
   if (product.Description is not { } description)
       return;
   ```

### 3.2 Paket sürüm yönetimi

Paket sürümü merkezîdir (K-1306 (6)): kökte `Directory.Packages.props` tek sürüm kaynağıdır; **sürümler tam sabittir** (kayan aralık yok); `packages.lock.json` repoda durur ve CI'da kilitli geri yükleme koşar; paket kaynağı yalnız nuget.org'dur; .NET SDK sürümü `global.json` ile sabitlenir. Bu dosyalar `Directory.Build.props` ile aynı kökte ve aynı PR'dadır.

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <RestorePackagesWithLockFile>true</RestorePackagesWithLockFile>   <!-- bilgim tarihli — ilk task'ta doğrulanır -->
  </PropertyGroup>
  <ItemGroup>
    <!-- Sürüm yalnız burada, tam sabit. Proje dosyasında Version yazılmaz. -->
    <PackageVersion Include="Serilog" Version="4.3.1" />
  </ItemGroup>
</Project>
```

Merkezî paket dosyasındaki her paket sürümünün Kütüphaneler tablosunda satırı vardır; eşitlik CI'ın Lint adımında denetlenir (betiğin yazımı §21'dedir, yazım 6 — iş adı: kütüphane tablosu eşitliği denetimi).

### 3.3 Kütüphane eklemenin işletimi

Kütüphaneyi bir implementation task'ının PR'ı ekler ve her ekleme kanıtla gelir (K-1305, K-1310, K-1312). Ölçüt `05 §2`'nin ölçütüdür (MK-02): lisans ücretsiz ve ticari kullanıma açıktır ve proje bakımı sürüyordur.

1. **Kim ve nasıl.** Kütüphaneyi yalnız bir task'ın PR'ı ekler; ekleme task'ın kabul kriterlerinde yazılıdır ya da PR açıklamasında gerekçelendirilir. Bağımsız doğrulama kanıtı yeniden okur. Dependabot PR'ları ve ana sürüm güncellemeleri de aynı yoldan geçer. Gerekmedikçe yeni bağımlılık eklenmez (`INSTRUCTIONS §4`): yerleşik .NET ve tablodaki bir kütüphane yetmiyorsa eklenir.
2. **Kanıt** (PR açıklamasına ve tablo satırına): paket adı ve tam sürüm · lisans — SPDX ifadesi, NuGet paket sayfasından ve deponun LICENSE dosyasından okunur, ikisi uyuşur · son yayın tarihi ve bakımcı (son on iki ayda yayın ya da bakımcının yanıtı) · geçişli bağımlılıkların lisansı (aynı ölçüt) · neden gerekli (tek cümle) · erişim tarihi.
3. **Tablo ve aynı PR.** Kütüphane aşağıdaki Kütüphaneler tablosuna aynı PR'da satır olarak girer (§3.2'nin eşitlik denetimi).
4. **Lisans ölçütü.** İzinli lisans (MIT, Apache-2.0, BSD-2/3, ISC, MS-PL gibi) kabul edilir. **Kopyaleft lisans (GPL, LGPL, AGPL) kabul edilmez** — repo MIT lisanslıdır ve her kurulum ayrı bir firmadır.
5. **Koşullu lisanslı kütüphane alınabilir** (gelir eşikli, ticari ya da açık kaynak şartlı): eşiği aşan firmada lisans bedelini kurulumu yapan öder ya da firmaya yansıtır (K-1310). **Varsayılan açık lisanstır** — aynı işi gören koşulsuz bir aday görevin gereğini karşılıyorsa o seçilir; koşullu aday yalnız eşdeğer görevi karşılamıyorsa ya da gerekçeyle belirgin üstünse seçilir (K-1312). Koşullu adayın kanıtı (2)'ye şunları ekler: koşulun metni ve kaynağı · eşik (ölçü ve para birimiyle) · bedelin türü ve tutarı (yıllık, geliştirici başına, kurulum başına) ya da doğrulanamadığı · açık lisanslı eşdeğerin adı ve neden yetmediği · koşulun geçişli bağımlılığa uygulanıp uygulanmadığı. Tablodaki koşullu satırın eşiği ya da bedeli boş olamaz.
6. **Firmaya bildirim.** `DEPLOY_RUNBOOK`'taki "koşullu lisanslı kütüphaneler" listesi tablonun koşullu satırlarından üretilir (kütüphane, eşik, bedel, kim öder); kurulumu yapan her kurulumda firmanın gelirini eşikle karşılaştırır, eşiği aşıyorsa lisansı alır ya da bedeli firmaya yansıtır ve sonucu kurulum kaydına yazar. Liste her kütüphane değişikliğinde aynı PR'da güncellenir.
7. **Güncelleme.** Sürümler tam sabittir; her güncellemede lisans satırı, koşul, eşik ve bedel yeniden okunur ve tablo satırı aynı PR'da güncellenir. **Koşulsuzdan koşullu ya da ticari lisansa geçen sürüme geçilmez** — kütüphane son açık lisanslı sürümde kalır; geçiş koşullu aday sayılır ve (5)'ten yeniden geçer; bakımı bitiyorsa yerine açık lisanslı bir aday için task açılır (`05 §8.8.2`, §8.8.4).
8. **Doğrulanamayan olgu.** Kanıt satırında okunamayan değer (lisans metni, sürüm tarihi, bedel) tahmin edilmez; "ekleme task'ında doğrulanır" yazılır ve o task'ın kabul kriteridir.

### 3.4 Girdi doğrulamanın kalıbı

Araç çatının nitelik tabanlı doğrulamasıdır (DataAnnotations); ek kütüphane yoktur. Her kural türü kendi kodunu taşıyan bir nitelik sınıfıdır; tek bir sayfa süzgeci hataları kaydeder ve mesajı koddan üretir (K-1319). Kuralların kendisi `04 §7`'de, hata kodlarının listesi `07 §6.2.2`'dedir.

1. **Nitelikler.** Her alan kodu (`FieldRequired`, `InvalidEmail`, `TextTooLong`, …) bir nitelik sınıfıdır ve kodunu hata iletisi alanında taşır. Yerleşik `Required`, `StringLength`, `EmailAddress`, `Range` ve benzerleri girdi ve sorgu modellerinde kullanılmaz — kodları `04 §7`'nin kuralına bağlı değildir (mimari test: model özellikleri yalnız projenin niteliklerini taşır). **Sınır değeri nitelikte sayı olarak yazılmaz,** parametre sabitinden okunur (§3.5).

   ```csharp
   // Yanlış: kod yok, sınır nitelikte sayı.
   [StringLength(150)] public string Name { get; set; } = "";

   // Doğru: kural türünün kodu nitelikte, sınır ürün sabitinden (P-27).
   [FieldRequired]
   [TextTooLong(ProductConstants.ProductNameMaxLength)]
   public string Name { get; set; } = "";
   ```
2. **Alana özgü kural** (iki alanın ilişkisi, tarih sırası) modelin `IValidatableObject` yöntemidir ve kendi kodunu üretir (`07 §6.1.3`). **Veritabanı gerektiren kural** (stok kodu tekilliği) model doğrulamasında değil işleyicidedir: aynı kodla `ModelState`'e eklenir ve süzgeç onu da aynı yoldan işler.
3. **Süzgeç.** Model bağlama ve doğrulamadan sonra, işleyiciden önce tek bir sayfa süzgeci koşar; hata varsa (a) her hatanın alan adı ve kodunu uygulama kaydına yazar (`07 §6.1.3`), (b) mesajı koddan ve alanın etiketinden mesaj kataloğuyla üretip `ModelState`'e koyar, (c) 422 ve formun yeniden gösterimini işleyiciye bırakmadan sürdürür (`07 §2.7.1`). Bir istekteki bütün hatalar birlikte işaretlenir (`04 §7.3.5`). **Kod yanıta ve sayfaya yazılmaz** (`07 §2.7.3`).
4. **Mesaj kataloğu.** Tek sınıf: kod → Türkçe mesaj kalıbı; kalıp `04 §7.3.1`'in biçimindedir (alanın adı ve neyin eksik ya da yanlış olduğu), sınırın değeri parametredir ve mesajda görünür. Test: her kodun bir kalıbı vardır ve her kalıbın kodu `07 §6.2.2`'nin listesindendir.
5. **Boşluk kırpma** (`04 §7.3.1`). Metin girdileri bağlama aşamasında özel bir model bağlayıcıyla kırpılır; yalnız boşluktan oluşan değer boş dizgidir (`null` değil) ve zorunlu alanda `FieldRequired` doğurur. Şifre alanları ve kırpılmaz işaretli alanlar kırpılmaz.
6. **İstemci tarafı.** HTML'in `required`, `maxlength` ve `type` nitelikleri aynı parametre sabitlerinden üretilir ve iyileştirmedir (`GUARDRAILS §8`: sunucu yetkilidir); çatının `data-val-*` üretimi kapatılır (düz JavaScript, MK-01; bilgim tarihli — çatı seçeneği ilk task'ta sınanır).
7. **Test.** Nitelik başına birim testi (geçerli ve geçersiz değer, kod); süzgeç testi (422, `ModelState` mesajı, kayıt satırı, bütün hataların birlikte gelmesi); kırpma testi (boşluk, yalnız boşluk, şifre).

Model adları ve işleyici adları §5.3'te, hata kodlarının kaydı ve gösterilmemesi §8'de (yazım 2) yazılır; bu alt bölüm süzgecin kalıbının evidir.

### 3.5 Ayarlar, ürün sabitleri ve süre ve tavan değerleri

Parametrenin üç katmanı (`02 §10.1.3`, `05 §8.4`; K-412) kodda üç ayrı evdir; yeni bir parametrenin katmanı tartışılmaz, `02 §11`'in satırından okunur (K-1330).

| Katman | Kodda evi | Değişiklik |
|---|---|---|
| Firma ayarı (`02 §11.1`) | Veritabanında firma ayarları satırı (`CompanySettings`, `06 §4.2`); okuma ve bölüm kaydı §10.3'ün kalıbıyla (yazım 3) | Panelden, hemen geçerli |
| Kurulum ayarı | Ayar dosyası ve sırlar; seçenekler sınıfına bağlanır ve açılışta doğrulanır | Uygulamanın yeniden başlatılmasıyla |
| Ürün sabiti (`02 §11.2`) | `Infrastructure/` altında tek statik sınıf: `ProductConstants` | Yeni sürümle; ayar dosyası değiştiremez |

1. **Ürün sabitleri tek yerdedir.** `ProductConstants`'taki her sabit `02 §11.2`'nin bir P satırına karşılık gelir; sabitin adı varlığın ve alanın adından kurulur (`ProductNameMaxLength` — `Product.Name`, P-27), özet yorumu envanter kimliğini taşır (`/// P-27`) ve değeri `const`'tır. Aynı P satırı birden çok alanı kapsıyorsa tek sabit vardır. **Sayı başka yerde yazılmaz:** nitelik, HTML üretimi ve test aynı sabite başvurur.

   ```csharp
   public static class ProductConstants
   {
       /// P-27 — ürün adının ve kurumsal içerikte ad ya da başlığın karakter tavanı.
       public const int ProductNameMaxLength = 150;
   }
   ```
   Envanter tablosu ile sabitler eşit tutulur: her P satırı için aynı değerde bir sabit vardır ve tersi (eşitliğin denetim betiği §21'dedir, yazım 6 — iş adı: parametre envanteri eşitliği denetimi). Envantere girmeyen değerler (yasal süreler, kargo şirketi listesi — `02 §11` "Envantere girmeyenler") ürün sabiti sınıfına yazılmaz; kendi evlerindedir.
2. **Kurulum ayarı** güçlü tipli seçenekler sınıfıdır; açılışta bütün ayarlar doğrulanır. **Zorunlu ayar** (alan adı, gönderen adresi, e-posta gönderim kimliği, veritabanı bağlantısı) eksik ya da biçimsizse uygulama açılmaz ve eksik ayarı adıyla kaydeder; **koşullu ayar** (ödeme sağlayıcısının anahtarları, Google uygulaması) eksikse uygulama açılır, ilgili hat kapalıdır ve bu hâl açılışta kayda yazılır (`05 §8.4.6`). Ayar dosyası ürün sabitini değiştiremez; dosyada ürün sabitinin adı bulunursa açılış reddedilir.
3. **Teknik Mimari'nin tasarım değerleri kodda sabittir;** yalnız `05 §8.4.5`'in saydıkları (hız sınırı, durum ucunun eşikleri, kayıt düzeyi) varsayılanla kodda, değiştirilebilir olarak kurulum ayar dosyasındadır ve her biri `DEPLOY_RUNBOOK §C`'de varsayılanıyla yazılıdır.
4. **Süre ve tavan değerlerinin tek yeri.** Kılavuzun koyduğu süre ve tavan değerleri aşağıdaki tablodadır (K-1328); ilgili bölüm değeri yinelemez, bu tabloya atıf yapar. Değerler bilgim tarihli tasarım değerleridir; ilk yük denemesinde ve D-01'de sınanır.

| Değer | Katman | Kaynak |
|---|---|---|
| Dış çağrı süresi: kullanıcının beklediği çağrıda 15, iş kuyruğundan giden çağrıda 30 saniye; `HttpClient.Timeout` sonsuz, süre çağrı başına kurulur | Kodda sabit | K-1257, K-1258, K-1322 (3) |
| Amazon SES istemcisi: süre 30 saniye, yeniden deneme 0 | Kodda sabit | K-1322 (1) |
| Veritabanı oturumu (sunucu taraflı): `lock_timeout` 5, `statement_timeout` 15, `idle_in_transaction_session_timeout` 30 saniye; istemci `CommandTimeout` 20 saniye | Kodda sabit | K-1328 (2) |
| Göç hizmeti ve yönetim komutları: `statement_timeout` 10 dakika (öteki iki değer aynı) | Kodda sabit | K-1328 (2) |
| Google kimlik belirtecinin `exp` saat sapması toleransı 60 saniye; `auth_time` toleranssız | Kodda sabit | K-1328 (4), K-1272 |
| Canlılık ucunun veritabanı denetim süresi 2 saniye (aşılırsa 503) | Kurulum ayarı | K-1328 (5), `07 §4.33.1` |
| Dışa aktarma: üretim süresi en çok 120 saniye, dosya en çok 100 MB, sipariş okuma sayfası 500 sipariş | Kodda sabit | K-1321 (4), (8) |
| Ters vekilin dışa aktarma yollarında yanıt başlığı bekleme süresi 150 saniye | Ters vekil yapılandırması (ürünle gelir) | K-1321 (9) |
| Geçici dışa aktarma dosyasının süpürme yaşı 1 saat | Kodda sabit | K-1321 (7) |
| İş kuyruğu çalıştırıcısının kuyruğa bakma aralığı 5 saniye | Kodda sabit | K-889 |

Çelişki taraması (K-1328 (6)): 15 saniyelik kullanıcı çağrısı 20 saniyelik komut aşımından kısadır; 120 saniyelik üretim tavanı 15 saniyelik sayfa sorgularının toplamından uzundur; 150 saniyelik ters vekil süresi 120 saniyelik tavanı taşır.

### Kütüphaneler

Kütüphane yalnız bu tabloda seçilir; kuralı kullanan bölüm tabloya atıf yapar (yazım konvansiyonu 7). Her satırın eklenmesi §3.3'ün kanıtıyla gelir. Erişim tarihi hepsinde 2026-10-29'dur; "ekleme task'ında doğrulanır" diye işaretli olgu o task'ın kabul kriteridir. Çatıyla gelen araçlar (DataAnnotations — §3.4; `HtmlRenderer` — §11.4, yazım 4) ek paket ve lisans satırı taşımaz.

| Kütüphane | Görevi | Lisans | Lisans koşulu | Bakım kanıtı | Karar |
|---|---|---|---|---|---|
| `xunit.v3` | Test çatısı (iki test projesi) | Apache-2.0 (proje README'si) | yok | Kararlı 3.2.x; .NET 10 için açık destek cümlesi bulunamadı, ilk task'ta doğrulanır | K-1313 |
| `Microsoft.AspNetCore.Mvc.Testing` | Sayfa ve uç testleri (`WebApplicationFactory`) | Lisansı okunamadı; ekleme task'ında doğrulanır | doğrulanır | Microsoft paketi | K-1313 |
| Mailpit | Geliştirme ortamının e-posta yakalayıcısı (geliştirme aracı; ürünle dağıtılmaz) | MIT | yok | 1.28.x ikincil kaynakta; imaj etiketi tam sürüm | K-1313 |
| `Serilog` | Yapılandırılmış kayıt | Apache-2.0 | yok | 4.3.1 | K-1314 |
| `Serilog.Sinks.File` | Dosya yazıcısı; kütüphanenin kendi silme sınırı kapatılır | Apache-2.0 | yok | 7.0.0 | K-1314 |
| `Serilog.AspNetCore` | Serilog'u çatının kayıt arayüzüne bağlar | Apache-2.0 (ikincil kaynak) | doğrulanır | 10.0.0 ikincil kaynakta; ekleme task'ında doğrulanır | K-1314 |
| `HtmlSanitizer` (Ganss.Xss) | Kapalı metin biçimi setinin temizlenmesi | MIT (deponun LICENSE dosyası ekleme task'ında okunur) | yok | Kararlı 9.0.892 (9.1.x yalnız beta); CVE-2026-25543 9.0.892'de kapalı; geçişli bağımlılığı AngleSharp ekleme anında okunur; .NET 10 hedefi ilk task'ta sınanır | K-1315 |
| `Magick.NET-Q8-AnyCPU` | Görüntü işleme (başlık okuma, ilk kare, yön, meta veriyi atma, WebP türevi) | Apache-2.0 | yok | 14.14.0 ve üstü (DCM çözücüsündeki CVE-2026-49218 o sürümde kapalı); yerel kod taşır; belgeden doğrulanamayan ayrıntılar ilk task'ta sınanır | K-1316 |
| `SixLabors.ImageSharp` 3.1.x | **Yedek aday** — Magick.NET'in beş doğrulama kapısından biri tutmazsa geçilir | Six Labors Split License | Gelir eşiği 1 milyon ABD doları: altındaki kâr amaçlı şirketler ve kâr amacı gütmeyenler için Apache-2.0, ötekiler için ticari lisans; geçişli bağımlılıkta uygulanışı belirsiz; bedel bulunamadı (kaynak: nuget.org/packages/SixLabors.ImageSharp/3.1.3/License); geçişte eşik ve bedel §3.3 (5)'in kanıtıyla yazılır | 3.1.x ile sabit kalınır (4 derleme için lisans dosyası ister — ikincil kaynak); yönetilen koddur | K-1316, K-1312 |
| `EFCore.NamingConventions` | Veritabanı adlarının küçük harf ve alt çizgiye dönüşümü | Apache-2.0 (deponun LICENSE dosyası ekleme task'ında okunur) | yok | 10.0.1, Aralık 2025; EF Core 10 çizgisini destekler; çıkış yolu: kendi model kuralı | K-1309 |
| `TngTech.ArchUnitNET` | Modül sınırlarının mimari testleri | Apache-2.0 | yok | 0.13.3; 0.13.0 ve 0.13.1 Aralık 2025'te yayımlandı; 0.13.3'ün tarihi ve son sürüm ekleme anında tabloya yazılır | K-1308 |
| `AWSSDK.SimpleEmailV2` | Amazon SES istemcisi (yeniden deneme kapalı, kimlik açık) | Lisansı paket sayfasından okunamadı; ekleme task'ında NuGet sayfasından ve deponun LICENSE dosyasından okunur | doğrulanır | 4.0.102.1, 27 Temmuz 2026 (ikincil kaynak); SES v2 yapılandırma sınıfının adı ilk task'ta doğrulanır | K-1322 |
| restic | Dosya yedeği aracı (yedek aracı imajında; kütüphane değil, ikili) | BSD-2-Clause (bilgim tarihli, ilk task'ta doğrulanır) | yok | Sürüm ve ikilinin SHA-256 özeti derleme dosyasında sabit; sürüm ekleme anında | K-1327 |

**Kütüphanesiz kararlar** (aday ya da kütüphane elendi; K-1306, K-1313, K-1319, K-1321, K-1323, K-1324, K-1325): üçüncü taraf çözümleyici paketi (StyleCop) · onaylama kütüphanesi (yerleşik `Assert`) ve sahte nesne kütüphanesi (elle yazılmış durumlu sahteler) · doğrulama kütüphanesi (FluentValidation) · CSV kütüphanesi (kendi yazıcımız) · şablon motoru (Razor bileşenleri) · komut satırı ayrıştırıcısı (`System.CommandLine`; kendi küçük dağıtıcımız) · ön yüz çerçevesi, paketleyici ve CSS ön işlemcisi. Teknik Mimari'nin bu dokümana bıraktığı seçimlerin (`05 §2.2.3`, 2.2.4, 2.2.8, 2.2.9; MK-02) hepsi yukarıdaki satırlardadır; Teknik Mimari'nin kendi seçtiği altyapı bileşenleri (PostgreSQL, ters vekil, pgBackRest) kütüphane değildir ve `05 §2`'dedir.

*Kaynak: K-1305, K-1306, K-1310, K-1312 · K-1313, K-1314, K-1315, K-1316, K-1308, K-1309, K-1319, K-1321, K-1322, K-1323, K-1324, K-1325, K-1327, K-1328 · K-411, K-412, K-922, K-889, K-1257, K-1258, K-1272 · `05 §2.3.5`, §8.4, §8.8.2, §8.8.4 · `02 §10.1.3`, §11 · `04 §7` · `07 §2.7`, §6 · `INSTRUCTIONS §4`.*

## 4. Proje yapısı ve klasör organizasyonu

### 4.1 Çözüm ve proje düzeni

Uygulama **tek projedir**; modüller o projenin klasörü ve ad alanıdır, derleme projesi değildir (K-1307). Modüller çevrimsel çağırır — sipariş onayı ödemenin başlatılmasını ister, ödeme bildirimi siparişin geçişini ister (`05 §3.3.3`–§3.3.5, §4.5) — ve derleyici çevrimsel proje başvurusunu reddeder; sınırı derleyici değil mimari test korur (§6).

1. **Çözüm üç projedir.** Kökteki çözüm dosyası bunları toplar (`Shopfolio.slnx` — .NET 10 SDK'nın yeni çözüm biçimi, bilgim tarihli; SDK `.slnx`'i üretmiyorsa `Shopfolio.sln` yazılır ve bu satır aynı PR'da düzeltilir).

   ```text
   src/Shopfolio.Web/                 uygulamanın tamamı: modüller, altyapı, sayfalar, göçler, yönetim komutları
   tests/Shopfolio.UnitTests/         birim testleri ve modül sınırlarının mimari testleri; veritabanı gerektirmez
   tests/Shopfolio.IntegrationTests/  gerçek PostgreSQL 18'e karşı
   ```

   Ayrım CI'ın Unit ve Integration adımlarına birebir oturur (`05 §2.3.5`). Dördüncü bir proje (ortak kütüphane, "Core", "Domain") açılmaz — `05 §3.3.13` modülleri teknik katmanlara bölmeyi eledi.
2. **Uygulama projesinin içi** üç klasördür:

   ```text
   src/Shopfolio.Web/
     Modules/<Modül>/        her modül için bir klasör ve ad alanı: Shopfolio.Modules.<Modül>
     Infrastructure/         ortak altyapı (05 §3.3.11): zaman kaynağı, dosya deposu, önbellek, hata yakalama,
                             kayıt, bağlantı katmanı, veritabanı bağlamı ve göçler
     Pages/                  Razor sayfaları
   ```

   Modül klasörünün adı §5.1'in modül kod adıdır. Modülün varlık yapılandırmaları kendi klasöründedir; **veritabanı bağlamı tektir ve `Infrastructure/` altındadır** — bir modül kendi bağlamını açmaz. Göçler `Infrastructure/` altındadır, bir modülün klasörüne girmez (göç tek şemanın sırasıdır — §10.6, yazım 3).
3. **Sayfalar modül klasörüne girmez** (`05 §3.3.12`): sayfa modül değildir, modülün arayüzünü çağırır. `Pages/` altında üç grup vardır — `Storefront/` (vitrin), `Account/` (müşterinin hesabı), `Panel/` (yönetim paneli); ortak parçalar `Pages/Shared/`. Sayfa dosyası ve `PageModel` sınıfı §5.3'ün adıyla yazılır. **Adres klasörden türemez:** her sayfa adresini `@page` şablonuyla açıkça yazar ve adres `07 §2.1`'in Türkçe adresidir; dosya ve klasör adı İngilizce kod adıdır.

   ```cshtml
   @* src/Shopfolio.Web/Pages/Storefront/Cart.cshtml — dosya adı Cart, adres Türkçe *@
   @page "/sepet"
   @model Shopfolio.Pages.Storefront.CartModel
   ```
4. **Ön yüz dosyaları** `src/Shopfolio.Web/wwwroot/` altındadır; düzenleri §16'dadır (yazım 5).
5. **Repo kökündeki öteki yerler** §4.3'tedir. `scripts/`, `.github/` ve `Docs/` süreç katmanıdır (L2, L5) ve uygulama kodu taşımaz; `src/`, `tests/` ve `deploy/` ürünün kodudur.

**Yasak.** Modül başına derleme projesi, "Core/Domain/Application/Infrastructure" katmanlı projeler, sayfa klasörünü modül klasörünün altına koymak, ikinci veritabanı bağlamı. **Örnek:** sepete ürün ekleyen yeni bir iş kuralı `Modules/Ordering/` altına girer; ona bağlanan sayfa `Pages/Storefront/` altındadır ve `Modules/Ordering/Pages/` diye bir klasör açılmaz.

*Kaynak: K-1307 · K-1331 (1) · `05 §2.3.5`, §3.3, §3.3.11–§3.3.13, §3.4.5 · `07 §2.1`.*

### 4.2 Klasör düzeninin `CONTEXT.md` §3'e yazılışı

Klasör düzeni bu dokümanın işidir ve D-01'in kapanışında, bu dokümanla birlikte `CONTEXT.md` §3'e **klasör düzeyinde** yazılır (`05 §2.3.2`; `DEFERRED_BACKLOG.md` D-01). Dosya listesi yazılmaz.

1. **Yazılacak satırlar** §4.1 ve §4.3'ün klasörleridir: `src/Shopfolio.Web/Modules/<Modül>/` · `src/Shopfolio.Web/Infrastructure/` · `src/Shopfolio.Web/Pages/` (`Storefront`, `Account`, `Panel`) · `src/Shopfolio.Web/wwwroot/` · `tests/Shopfolio.UnitTests/` · `tests/Shopfolio.IntegrationTests/` · `deploy/`. `CONTEXT.md` §3'ün "kod klasörü henüz yok" satırı o PR'da kalkar.
2. **Kim yazar.** D-01'i kapatan iş; klasörler henüz yoktur, satırlar bu dokümandan alınır ve ilk implementation task'ı klasörü açarken satırın doğruluğunu sınar. `SETUP.md`'nin değişikliği proje sahibinin onayıyla yapılır (GUARDRAILS §2).
3. **Sonradan değişiklik.** Yeni bir klasör düzeyi bu dokümanın §4'ünü ve `CONTEXT.md` §3'ü aynı PR'da günceller (`CONTEXT.md` §4).

*Kaynak: K-1307 (4) · K-1331 (2) · `05 §2.3.2` · `CONTEXT.md` §3, §4.*

### 4.3 İmaj, Compose ve geliştirme ortamı dosyalarının yeri

İmaj, Compose ve ters vekil dosyaları ürünün parçasıdır; repoda ve sürümle gelir (`05 §8.2.4`). Hepsi kökteki **`deploy/`** altındadır; uygulama kodunun klasörüne ve `scripts/`'e (playbook betikleri) karışmaz (K-1331 (3)).

```text
deploy/
  compose.yaml                  kurulumun Compose tanımı: dört hizmet ve tek seferlik göç hizmeti (05 §8.2.4)
  compose.dev.yaml              geliştirme varyantı: ödeme sağlayıcısının test ortamı ve e-posta yakalayıcı
  .env.example                  kurulumun sır olmayan ayar dosyasının şablonu (05 §2.3.4)
  caddy/Caddyfile               ters vekil yapılandırması
  images/app/Dockerfile         uygulama imajı (çok aşamalı, kök olmayan kullanıcı, göç demeti)
  images/database/              veritabanı imajının derleme dosyası ve zamanlayıcısı
  images/backup/                yedek aracı imajının derleme dosyası ve zamanlayıcısı
```

1. **Derleme bağlamı repo köküdür.** Üç imajın derleme komutu `docker build -f deploy/images/<ad>/Dockerfile .` biçimindedir; böylece uygulama imajı `src/` ve `Directory.Packages.props`'u görür. Dockerfile `src/` altına konmaz.
2. **Geliştirme ortamı kurulumun Compose tanımının bir varyantıdır** (`05 §8.1.2`): `compose.dev.yaml`, `compose.yaml`'ın üstüne bindirilir (`docker compose -f deploy/compose.yaml -f deploy/compose.dev.yaml up`) ve ayrı bir veritabanı sürümü ya da ayrı bir imaj tanımlamaz. Geliştirme varyantı e-posta yakalayıcısını ekler (Kütüphaneler tablosundaki Mailpit); hiçbir e-posta dışarı çıkmaz.
3. **Sır ve kuruluma özgü değer repoya girmez.** `.env.example` yalnız sır olmayan ayarların şablonudur; ayar ve sır dosyaları sunucudadır (`05 §2.3.3`, §8.4.3). Tabloya bir ayar eklenirse `.env.example` aynı PR'da güncellenir.
4. **Taban imajlar etiket ve özetle sabitlenir** (`05 §8.2.3`): derleme dosyasında `imaj:sürüm@sha256:…` yazılır, `latest` yazılmaz. Dockerfile'ın içeriği (adımlar, kullanıcı, göç demeti) `05 §8.2.1`'dedir; veritabanı ve yedek aracı imajlarının içeriği §17'dedir (yazım 5).
5. **Değişen ya da eklenen dosya** bu bölümün ağacını ve `CONTEXT.md` §3'ü aynı PR'da günceller (§4.2).

*Kaynak: K-919, K-920, K-937, K-1327 (2) · K-1331 (3) · `05 §2.3.3`, §2.3.4, §8.1.2, §8.2.1–§8.2.4, §8.4.3.*

## 5. Adlandırma konvansiyonları

### 5.1 Sözlüğün devralınması ve kod adları

Bu bölümün kuralı iki cümledir: **ad sözlükten ve Veri Modeli'nden birebir alınır** (`02 §1.2`; `06 §4`, §5) ve **bir kavramın tek adı vardır** (§1, "ad uydurulmaz"; yazım konvansiyonu 2). Altındaki kurallar bu ikisinin biçimi ve boşluklarıdır.

1. **Ürün kavramı sözlüğün İngilizce karşılığıyla yazılır.** Sipariş `Order`, sipariş kalemi `OrderItem`, sepet `Cart`, kupon `Coupon`, geri ödeme `Refund`, ayıp talebi `DefectClaim`. Tür adı, dosya adı ve değişken adı aynı kökü kullanır; `Basket` ya da `Purchase` yazılmaz.
2. **Kapalı listenin değeri ve kayıt düzeyindeki kapalı değer `06 §5`'in kod adıdır.** `OrderStatus.Placed`, `PaymentStatus.Paid`, `ProductType.Digital`. Terimi sözlükte olmayan değerin kod adını `06 §5` verir; bu dokümanda bulunmayan değer bir boşluktur (BLOCKED, `SPEC_GAP`).
3. **Alan adı `06 §4`'ten alınır.** Kodun özellik adı alanın kod adıdır: `ShippingFee`, `VatRate`, `IsMergeNoticePending`. "Daha anlaşılır" diye değişmez.
4. **Teknik kavramın kod adı `06`'nın verdiği addır.** Veri koruma paketinin varlığı `DataProtectionKey`, işlem izi `AuditLog`, yedeğin durum kaydı `BackupStatusRecord`, iş kuyruğunun satırı `Job` (`06 §4.68`, §4.63, §4.67, §4.59).
5. **Modülün kod adı** sözlükte ve `06`'da yoktur (`05 §3.3` Türkçe adlarını verir) ve bu dokümanın kararıdır (K-1331 (4)):

   | `05 §3.3` | Kod adı | `05 §3.3` | Kod adı |
   |---|---|---|---|
   | Katalog | `Catalog` | Üyelik | `Membership` |
   | Sepet ve sipariş | `Ordering` | Kurumsal içerik | `CorporateContent` |
   | Ödeme | `Payment` | Bildirim | `Notifications` |
   | İptal, cayma ve iade | `AfterSales` | Yönetim ve işlem izi | `Administration` |
   | Ayarlar | `Settings` | | |

   **Modül adı hiçbir tür adıyla çakışmaz:** `Shopfolio.Modules.X` içindeki `X`, sözlüğün ya da `06`'nın bir tür adı olamaz — C# bir ad alanı bölümü ile aynı adlı tür arasında kararsız kalır. Bu yüzden iptal, cayma ve iade modülü `Cancellation` ya da `Return` değil `AfterSales`, kurumsal içerik modülü `Content` değil `CorporateContent`, bildirim modülü `Notification` değil `Notifications`'tır (`Cancellation`, `Return`, `Content` ve `Notification` sözlüğün terimleridir).
6. **Yasak.** Eş anlamlı ad (`Order` varken `Purchase`), Türkçe sembol adı (`Siparis`), sözlükte olmayan kısaltma (`Ord`, `Qty`). **Örnek — doğru ve yanlış yan yana:**

   ```csharp
   // Yanlış: Basket sözlükte yok (Cart), ShippingCost 06'da yok (ShippingFee).
   public decimal BasketShippingCost { get; init; }

   // Doğru: sözlüğün Cart'ı ve 06 §4.2'nin alanı.
   public decimal CartShippingFee { get; init; }
   ```

   Yeni ürün kavramı gerekirse önce `02 §1.2`'ye girer; kod onu sözlükten alır (K-17). Sözlükte olmayan bir yardımcı tür (örneğin bir yardımcı sınıf) ürün kavramını adlandırmıyorsa C# kuralıyla (madde 7) serbestçe adlanır.
7. **C# adları** §3.1'in stilini izler (`_camelCase` özel alan, `Async` soneki, `record`) ve şunları ekler: tür, metot, özellik ve ad alanı PascalCase; parametre ve yerel değişken camelCase; arayüz `I` önekiyle (`IFileStore`); bir dosyada bir üst düzey tür ve dosya adı türün adıdır (`OrderItem.cs`); kapalı değer `enum`'u tekil ad alır ve değerler PascalCase'tir (`OrderStatus`, değeri `Placed`); mantıksal özellik `Is`, `Has` ya da `Can` ile başlar (`06 §2.5.1`: mantıksal alanın adı `Is…` ile başlar); ad alanı klasör yolunu izler (`Modules/Ordering/` → `Shopfolio.Modules.Ordering`).

*Kaynak: K-17, K-977, K-988, K-1036, K-1038 (P1.1) · K-41, K-54, K-253, K-266, K-272, K-455, K-458, K-1034 · K-1307 (2) · K-1331 (4) · `02 §1.2` · `05 §3.3` · `06 §2.5.1`, §4, §5.1.*

### 5.2 Veritabanı adlandırma kuralının uygulanışı

Kural `06 §2.4`'tedir (küçük harf ve alt çizgi, çoğul tablo adı, `pk_`/`fk_`/`ix_`); bu alt bölüm **kodda nasıl uygulandığını** yazar (K-1309).

1. **Dönüşümü bir kütüphane yapar.** Veritabanı bağlamının kurulumunda tek çağrı `UseSnakeCaseNamingConvention()` (EFCore.NamingConventions — Kütüphaneler tablosu): tablo, sütun, anahtar, dizin ve kısıt adları kod adından küçük harf ve alt çizgiye çevrilir. Tek çağrı noktası bağlamın kurulumudur; model kuralı başka yerde çağrılmaz.
2. **Tablo adı çoğuldur ve `DbSet` özelliğinin adından gelir.** Her varlık türünün — bağ tabloları dahil — bağlamda bir `DbSet` özelliği vardır ve özelliğin adı varlık adının çoğuludur; çoğullaştırıcı kütüphane kullanılmaz.

   ```csharp
   // Veritabanı bağlamı (Infrastructure/): özellik adı çoğul, tablo adı ondan.
   public DbSet<Order> Orders => Set<Order>();                  // orders
   public DbSet<OrderItem> OrderItems => Set<OrderItem>();      // order_items
   public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();  // product_categories (bağ tablosu)
   ```
3. **Elle ad verilmez.** `ToTable("…")` ve `HasColumnName("…")` yazılmaz; dizin ve kısıt adı verilmez (`06 §2.4.3`). Tek istisna aşağıdaki veri koruma paketinin varlığıdır ve o da elle ad gerektirmez.
4. **Veri koruma paketinin varlığına kural istisnasız uygulanır** (`06 §4.68`): paketin bağlam arayüzü özelliğin adını `DataProtectionKeys` verir ve kütüphane `data_protection_keys`, `friendly_name`, `xml` üretir. Bu olgular paketin sürümüne bağlıdır ve bilgim tarihlidir; paketi bağlayan ilk task'ta sınanır (§1, ilke 5). Anahtar türü ve sütunlar ayrışırsa `06 §4.68` güncellenir.
5. **Model testi (entegrasyon testi).** Bütün tablo, sütun, anahtar, dizin ve kısıt adları `^[a-z][a-z0-9_]*$` desenindedir ve 63 bayttan uzun değildir (PostgreSQL'in tanımlayıcı sınırı). İhlalde ad elle kısaltılmaz — task BLOCKED olur ve `06` ile hizalanır.

   ```csharp
   // IntegrationTests: model kurulumundan geçen her ad desene uyar. Model API'si bilgim tarihli — ilk task'ta doğrulanır.
   foreach (var entity in context.Model.GetEntityTypes())
       Assert.Matches("^[a-z][a-z0-9_]*$", entity.GetTableName()!);   // sütun, anahtar, dizin ve kısıt için aynı denetim
   ```
6. **Çıkış yolu.** Kütüphane bir EF ana sürümüne geç yetişirse çağrının yerine aynı çıktıyı üreten kendi model kuralı (`IModelFinalizingConvention`) konur; tek çağrı noktası olduğu için değişiklik tek yerdedir.

*Kaynak: K-1309, K-1012 · K-1035 (veri koruma paketi) · `06 §2.4`, §4.68 · Kütüphaneler tablosu (EFCore.NamingConventions).*

### 5.3 Sayfa, işleyici, model, hata kodu ve çerez adları

Adların kaynağı `07`'dir; bu alt bölüm kodun onları nasıl taşıdığını yazar. Adı `07`'de olan bir ad kodda değişmez (§1, "kaynak doküman kazanır").

1. **İşleyici.** Adı İngilizce bir fiil ve işlemin nesnesinin sözlükteki ya da Veri Modeli'ndeki adıdır, Pascal yazımıyla: `RemoveCartItem`, `SaveProduct`. Nesnesi ekranın kendisi olan işlemler yalnız fiildir (`Register`, `SignIn`, `SignOut`, `Reauthenticate`; ödeme adımının `Recalculate` ve `Leave`'i). Ayrı form ve ayrı adreslerse ekleme ve düzenleme `Create…`/`Update…`, aynı formun aynı adresi `Save…` alır. Kodda işleyici `On<Yöntem><Ad>Async` metodudur (`OnPostRemoveCartItemAsync`); `handler` sorgu parametresindeki ad `07 §3`'teki adla birebirdir. Adların listesi `07 §3`'ün uç envanterindedir; envanterde olmayan işleyici adı yazılmaz.
2. **Girdi modeli.** Adı formun asıl işleyicisinin adı ve türüdür: `…Input`, `…Query`, `…Request`, `…Response` (`AddCartItemInput`, `SaveShippingFeeInput`). Aynı formu gönderen işleyiciler (ara gönderimler, içerik formunun `SearchProducts`'ı) aynı modeli bağlar. Aynı adlı işleyicinin vitrinde ve panelde ayrı alanları varsa panelin modeli `Admin` önekini alır. İç anahtarın alan adı varlığın adı ve `Id`'dir. Her işleyici yalnız kendi girdi modelini bağlar; veri modelinin varlığı doğrudan bağlanmaz (§3.4).
3. **Sayfa dosyası ve `PageModel`.** Sayfa dosyasının adı ekranın nesnesinin sözlük adıdır; `PageModel` sınıfı çatının kuralıyla `<Sayfa>Model` olur (`Cart.cshtml` → `CartModel`, ad alanı klasör yolunu izler — `Shopfolio.Pages.Storefront`). Bir kayıt türünün liste sayfası çoğul klasörün `Index` sayfasıdır (`Pages/Panel/Orders/Index.cshtml`), kayıt formu `Edit` sayfasıdır (yeni ve var olan kayıt aynı sayfanın iki hâlidir — `07 §3.1.1`). Adres her zaman `@page` şablonundadır (§4.1).
4. **Hata kodu.** İngilizce PascalCase'tir (`TextTooLong`, `FieldRequired`); bir kez verilen kod değişmez; yanıtın gövdesine ve başlığına yazılmaz ve kullanıcıya gösterilmez. Kayıtlar her reddin kodunu yazar (§8.2, yazım 2). Kod listesi `07 §6.2.2`'dedir; yeni kod bu dokümanda doğmaz.
5. **Çerez adı** `07`'nin kapalı listesidir ve sabit olarak bir yerde tanımlanır (`Infrastructure/`): `__Host-session`, `__Host-admin-session`, `__Host-antiforgery`, `__Host-cart`, `__Host-known-device`, `__Host-checkout`, `__Host-external-login`. Çerez adı başka yerde dizgi olarak yazılmaz; listeye yeni ad eklemek `07 §2` ve `02 §12.2.5`'in kapalı envanterini değiştirir.
6. **`notice` değeri** `07 §6.3`'ün kapalı listesidir; bilinmeyen değer yok sayılır, serbest metin gösterilmez.

*Kaynak: K-1162, K-1163, K-1165, K-1166, K-1168, K-1172, K-1173, K-1174, K-1196, K-1198, K-1205, K-1226 · K-1331 (5) · `07 §2.1`, §2.2, §3, §3.1.1, §5, §6.*

## 6. Mimari sınırlar

Sınır dokuz modül arasındadır (`05 §3.3`); derleyici korumaz (§4.1), mimari test korur. Bu bölüm sınırın kodda biçimini, kuralları ve testin aracını yazar (K-1308).

1. **Açık yüzey bir `Contracts` ad alanıdır.** Her modülün başka modüle verdiği yüzey `Shopfolio.Modules.<Modül>.Contracts` ad alanındadır: genel arayüzler ve onların aldığı ve verdiği **değişmez `record`'lar**. Varlıklar `Contracts`'tan dışarı çıkmaz. Modülün geri kalanı — varlıklar, iş mantığı, yapılandırmalar, kuyruk işleyicileri — `internal` olur.
2. **Başvuru kuralı.** Bir modül ötekine yalnız onun `Contracts` ad alanından başvurur — alan, parametre, dönüş türü ve metot gövdesindeki çağrı dahil. Başka modülün varlığına **veritabanı bağlamı üzerinden de dokunulmaz.** Sayfalar, yönetim komutları ve arka plan işleri de yalnız `Contracts`'a başvurur ve veritabanı bağlamına ya da varlığa dokunmaz (`05 §3.3.12`). Ortak altyapı modül türlerine başvurmaz.
3. **Kayıt ve kullanım.** Her modül `Add<Modül>Module` uzantısıyla kendi hizmetlerini kaydeder (`AddOrderingModule`); `Program.cs` dokuzunu çağırır. Arayüz kurucudan enjekte edilir. **Arayüz metodu kendi veritabanı işlemini açmaz ve kesinleştirmez** — çağıranın işlemine katılır (`05 §3.4.2`); işlemin sahibi §9'dadır (yazım 2).

   ```csharp
   // Yanlış: sayfa, katalog modülünün varlığına bağlam üzerinden dokunuyor (kural 2'yi bozar).
   var product = await _context.Products.FindAsync(id);

   // Doğru: sayfa yalnız Catalog.Contracts arayüzünü çağırır. Arayüzün adı ve metodu ilgili task'ın tasarımında
   // 07'nin ucuna göre yazılır; burada yalnız kalıp gösterilir.
   var view = await _catalog.GetProductViewAsync(id, ct);
   ```
4. **Mimari testler.** Araç ArchUnitNET'tir (Kütüphaneler tablosu); birim testi projesinde `Architecture/` klasöründedir. Yukarıdaki üç kuralın — başvuru kuralı, sayfa ve komutların bağlama dokunmaması, altyapının modüle başvurmaması — her biri bir testtir. **Her kural kasıtlı ihlal eden bir örnek kodla kırmızıya döndüğü kanıtlanır** (test kendi kendini sınar): ihlal örneği test projesinde ayrı bir derlemededir ve test o derlemeye karşı da koşar, kuralın o derlemede kırıldığını doğrular.

   ```csharp
   // UnitTests/Architecture/ModuleBoundaryTests.cs — ArchUnitNET API'si bilgim tarihli, ilk task'ta doğrulanır.
   // Kalıp: Modules.Ordering içindeki hiçbir tür Modules.Catalog'un Contracts dışındaki bir türüne bağımlı olamaz.
   ```
5. **Tek yazıcı kuralı** (`05 §3.4.4`) mimari testle sınanamaz; kalıbı ve sınaması §9.1'dedir (yazım 2).
6. **Dosya deposu arayüzü ortak altyapıdadır** (`Infrastructure/`; `05 §7.7.1`, K-1332). Görseller ve dijital ürün dosyaları iki bölümdür; bölüm kapalı bir değerdir (`FileArea`: `Image`, `DigitalFile`). İmza:

   ```csharp
   public enum FileArea { Image, DigitalFile }

   public interface IFileStore
   {
       Task WriteAsync(FileArea area, string key, Stream content, CancellationToken ct);       // önce geçici dosyaya, sonra adlandırma
       Task AppendAsync(FileArea area, string key, Stream content, CancellationToken ct);      // parçalı yükleme (05 §7.7.3); yalnız DigitalFile
       Task<Stream> OpenReadAsync(FileArea area, string key, CancellationToken ct);            // aralık okumaya uygun (aranabilir) akış
       Task<bool> ExistsAsync(FileArea area, string key, CancellationToken ct);
       Task<long> GetSizeAsync(FileArea area, string key, CancellationToken ct);
       Task DeleteAsync(FileArea area, string key, CancellationToken ct);
       IAsyncEnumerable<string> ListAsync(FileArea area, TimeSpan minAge, CancellationToken ct);  // imha süpürmesi için, yaşa göre
   }
   ```

   **Anahtarı uygulama üretir:** `Guid.CreateVersion7().ToString("N")` (32 küçük harfli onaltılık hane); kullanıcı girdisi anahtar ya da yol olamaz ve bu biçime uymayan anahtar `ArgumentException` ile reddedilir (yol ayırıcı ve üst dizin dahil). **Yazım yarım dosya göstermez:** içerik önce aynı birimdeki geçici bir dosyaya yazılır, tamamlanınca adlandırılır. Tek gerçekleme diskteki dosya alanıdır (`DiskFileStore`); nesne depolamaya geçiş yalnız gerçeklemeyi değiştirir (`05 §11.3.3`).
7. **Yasak.** Başka modülün `internal` türüne `InternalsVisibleTo` ile erişmek; `Contracts` içinde varlık döndürmek; arayüz metodunda `SaveChanges` çağırmak; modülün `Contracts`'ı dışında kalan bir türü bir sayfaya enjekte etmek.

*Kaynak: K-1308, K-871, K-881, K-932 · K-1332 · K-889 (P4.5; tek yazıcı) · `05 §3.3.12`, §3.4.1–§3.4.5, §7.7.1, §7.7.3, §11.3.3.*

## 7. Zaman ve biçim standartları

> **Ne yazılır:** Zaman damgası kaynağı (yerel saat kullanımı yasağı), tarih/sayı biçimleri, zaman dilimi politikası.

## 8. Hata yönetimi

## 9. Domain kuralları

> **Ne yazılır:** Durum makinesi kullanımı, geçiş koruma, yan etki yerleşimi, olay yayını, denetim kaydı.

## 10. Veri erişimi

> **Ne yazılır:** Sorgu kuralları, sayfalama zorunluluğu, izleme kapalı sorgular, işlem sınırları.

## 11. Entegrasyon kuralları

## 12. Sözleşme ve olay sürümleme

## 13. Arka plan iş kuralları

## 14. Hassas hesaplama kuralları

> **Ne yazılır:** Para/oran/ölçü hesaplamalarında tip seçimi, yuvarlama politikası (**ara adımda yuvarlama yok**), test zorunluluğu.

## 15. Güvenlik kuralları

## 16. İstemci tarafı kurallar

## 17. Yardımcı servis / sidecar standartları (varsa)

## 18. Loglama ve gözlemlenebilirlik

## 19. Test kuralları

> **Ne yazılır:** Test katmanları, hangi davranış hangi katmanda test edilir, test verisi stratejisi, izolasyon.
> **Ek kural:** `skip` / `only` / boş assertion içeren test **yazılmaz**; geçen ama hiçbir şey doğrulamayan test, boşluğun gizlenmiş hâlidir (gate check bunu tarar).

## 20. Performans kuralları

## 21. Git ve CI/CD kuralları

### 21.1 Dal stratejisi

> **Kural:** Bu bölüm `.claude/INSTRUCTIONS.md §3.2` ile **birebir aynı** olmalıdır. İki farklı yerde iki farklı dal stratejisi, bu dokümanların en sık çelişkisidir.

| Dal | Amaç |
|---|---|
| `main` | Çalışır durumda kod. Doğrudan push **yasak**. |
| `task/TXX-*` | Tek bir task. `main`'den ayrılır, `main`'e squash merge olur. |
| `chore/*`, `docs/*`, `infra/*` | Task'a ait olmayan işler — ayrı PR. |

### 21.2 Commit mesaj formatı

```
{tip}: {kısa açıklama}      # task dışı
TXX: {kısa açıklama}        # task commit'i
```

Tipler: `feat` · `fix` · `refactor` · `test` · `docs` · `chore` · `migration`

### 21.3 PR kuralları

- PR tek bir amaca hizmet eder. Karışık değişiklik yasak.
- **Bundled-PR yasağı** — başka bir task'ın PR'ına commit gömülmez.
- CI geçmeden merge yasak. Bağımsız validator PASS olmadan merge yasak.
- Merge'ü validator chat yapar.

### 21.4 CI pipeline sıralaması

```
0. Guard (direct push)
1. Lint
2. Build
3. Unit test
4. Integration test
5. Contract test
6. Migration dry-run
7. Paketleme doğrulaması
8. E2E (advisory)
→ CI Gate (toplu kapı)
```

Komutlar: `.github/ci-commands.env` · Kurulum: `Docs/CI_CD_SETUP.md`

### 21.5 Savunma katmanları

> Tek bir savunma katmanı yetmez. Katmanların tam listesi ve gerekçeleri: `00_PROJECT_METHODOLOGY.md` §F.
> Bu bölüm o listeye referans verir; **kuralı burada tekrar tanımlama** — iki yerde iki farklı ifade çelişki üretir.

## 22. Refactor kuralları

- Refactor davranış değiştirmeden yapılır. Davranış değişecekse **ayrı commit/PR**.
- Refactor PR'ı feature PR'ıyla karıştırılmaz.
- Kritik akışa dokunan kod, test veya koruma olmadan taşınmaz.

## 23. Yasaklar

> **Ne yazılır:** Koşulsuz yasaklar — iş kuralı uydurma, sessiz kapsam genişletme, katman ihlali, secret gömme, hassas veri loglama, exception yutma, sınırsız liste dönme, testsiz kritik davranış değişikliği, CI geçmeden merge.
> Süreç yasakları için ayrıca: `.claude/GUARDRAILS.md` §8.

## 24. AI çalışma kuralları

> **Ne yazılır:** Kod yazmadan önce hangi source-of-truth kontrol edilir · küçük diff · mevcut mimariyi bozmama · gereksiz bağımlılık eklememe · kritik akışlara özel kontroller (durum geçişi eklerken guard + yan etki + olay + denetim kaydının tamlığı gibi).

## 25. Review checklist

> **Ne yazılır:** Her kod tesliminde sorulacak sorular — doküman uyumu, mimari, durum/olay, güvenlik, test, performans.

---

*Shopfolio — Coding Guidelines v0.2*
