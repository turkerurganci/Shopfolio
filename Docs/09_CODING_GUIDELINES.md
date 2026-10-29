# Shopfolio — Coding Guidelines

**Versiyon: v0.9** | **Bağımlılıklar:** `02`, `04`, `05`, `06`, `07`, `08`, `10_MVP_SCOPE.md` | **Son güncelleme:** 2026-10-29

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

> **Yazım durumu (K-28, K-30, K-432; karar kaydı §20.1): yazım turu sürüyor (v0.9).** Doküman Kodlama kılavuzu aşamasının **tek yazım turunda**, altı oturumda yazılır (K-1302 (1)); 1. ve 2. oturum yöneticinin kararıyla baştan ikiye, 3. oturum üçe bölündü (`INSTRUCTIONS §9` (8)): Y-1a ve Y-1b, Y-2a (§7, §8) ve Y-2b (§9), Y-3a (§10.1–§10.3), Y-3b (§10.4–§10.6) ve Y-3c (§10.7–§10.10); 3. oturum tamamdır; 4. oturum da ikiye bölündü: Y-4a (§11) ve Y-4b (§12–§14). **Yazıldı:** Y-1a — başlık notu, §1, §2, §3 (K-1329, K-1330); Y-1b — §4, §5, §6 (K-1331, K-1332); Y-2a — §7, §8 (K-1333, K-1334); Y-2b — §9 (K-1335, K-1336); Y-3a — §10.1, §10.2, §10.3 (K-1337, K-1338); Y-3b — §10.4, §10.5, §10.6 (K-1339, K-1340, K-1341); Y-3c — §10.7, §10.8, §10.9, §10.10 (K-1342, K-1343, K-1344, K-1345); Y-4a — §11.1, §11.2, §11.3, §11.4 (K-1346, K-1347). **Sırada:** Y-4b §12–§14, 5. oturum §15–§18, 6. oturum §19–§25 (alt bölüm haritasının "Oturum" sütunu). Workshop tamamlandı (K-1305…K-1328). Özet ↔ ayrıntı taraması ve kalite döngüsü yazım turundan sonra gelir.
>
> **Park maddelerinin durumu (K-1302 (6)).** Park blokları talimattır ve yazımda değişmez; kapanışları burada izlenir. Bir park maddesi birden çok bölüme dokunuyorsa kural ilk konunun bölümünde yazılır, öteki bölümler ona atıf yapar. **Kapandı:** P1.1 (terim sözlüğünün devralınması — §5.1); P4.1 (dil, çatı ve kütüphane ekleme — §3.1, §3.2, §3.3 ve Kütüphaneler tablosu); P6.2 (girdi doğrulama süzgeci §3.4, model ve işleyici adları §5.3, hata kodlarının kaydı ve gösterilmemesi §8.2); P6.6 (işleyici adları §5.3, istekteki değerin biçimi ve bağlama kalıbı §7.2); P5.9 (kategori düzeyinin yazılışı — §9.4); P5.1 (silme davranışının kodda uygulanışı — §10.1); P5.2 (adlandırma §5.2, anahtar üretimi ve enum §10.1); P5.3 (normalleştirme ve kategori düzeyi §9.4, sürüm ve sayaç §10.2); P5.4 (bölüm başına sürüm — §10.3); P4.3 (önbellek ve kuşak sayacı — §10.4; modül sınırlarının korunması ve dosya deposu arayüzü §6); P5.11 (trigram eklentisi ve operatör sınıfı — §10.6); P5.12 (iç notun kilidi — §10.9); P5.6 (kopya ve doldurma §9.3, şablon sürüm kimliği ve üretilen yasal metin işlevi §11.4); P7.2 (Google kimlik belirtecinin doğrulanması — §11.3, saat sapması §3.5); P7.3 (değişken metnin kaçışı ve e-posta ve belge metninin üretimi — §11.4); P6.9 (dışa aktarmanın geçici dizini, çökme artığı, tavanlar ve CSV'nin biçimi — §10.10, süre ve tavan değerleri §3.5 ve §10.7). **Kısmen kapandı:** P4.2 — sayfaların düzeni §4.1'de yazıldı, ön yüz dosya düzeni §16'da; P4.5 — veritabanı oturumunun süreleri §10.7'de, mimari testle korunan kısım §6'da, zamanın tek kaynağı ve sınanabilirliği §7.1'de yazıldı, geçiş biriminin ve yan etkinin yeri §9.1'de yazıldı, iş kuyruğunun kalıbı §13'te; P4.7 — hatanın üç hâli ve sonucu belirsiz işlem §8.1'de yazıldı, kayıt kütüphanesi ve kayıt alanları §18'de; P4.8 — dosya düzeni ve test projelerinin ayrımı §4.1 ve §4.3'te yazıldı, test araçları §19'da; P4.9 — ayarlar ve ürün sabitleri §3.5'te yazıldı, yönetim komutları §13'te; P4.13 — iki imajın derleme dosyalarının yeri §4.3'te yazıldı, içerikleri §17'de; P7.1 — bağlantı katmanının kalıbı, istemciler ve imza doğrulaması §11'de yazıldı, tutar dönüşümü §14.2'de; P4.12 — bağlantı katmanının kalıbı §11.1'de yazıldı, HTML temizleyici ve görüntü işleme §15'te; P5.7 — kod adları ve imha göçü §9.2'de yazıldı, kodun sürümlenmesi §12'de; P5.8 — veri koruma paketinin varlığına adlandırmanın uygulanışı §5.2'de yazıldı, kullanıcıların ve yetkisinin biçimi §10.8'de yazıldı, kalan kısmı §15.5 ve §17'de; P6.1 — işleyici adları ve sayfa düzeni §5.3 ve §4.1'de yazıldı, kimlik şemaları §15.1'de; P6.10 — model ve hata kodu adları §5.3'te, hata kodlarının kaydı §8.2'de yazıldı; kayıt alanlarının adları §18.2'de, kodun sürümlenmesi §12'de; P4.10 — göçlerin yazılışı §10.6'da yazıldı, değerlerin sürümlenmesi §12'de; P5.10 — daraltma göçünün ön denetimi §10.6'da yazıldı, dönüşüm işinin ilerleme kaydı §13.3'te; P6.5 — listelerin kesin sırası ve arama sözcüğü §10.5'te yazıldı, sınama katmanları §19'da; P6.11 — arama sözcüğü §10.5'te yazıldı, canlılık ucunun süresi §10.7'de, veri koruma amaçları §15.5'te, e-posta gövdesinin kaçışı §11.4'te, test planı §19'da. **Açık:** öteki sekiz madde — yazılacakları bölümün oturumunda kapanır.

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
| §7 Zaman ve biçim standartları | 7.1 Tek zaman kaynağı · 7.2 İstekteki tarih, tutar ve ondalık biçimi | Y-2a ✓ |
| §8 Hata yönetimi | 8.1 Tek yakalama katmanı, üç hâl, sonucu belirsiz işlem · 8.2 Hata kodlarının kaydı ve gösterilmemesi | Y-2a ✓ |
| §9 Domain kuralları | 9.1 Geçiş birimi, beyaz liste, yan etki · 9.2 İşlem izinin kod adları · 9.3 Belgenin kişisel verisiz kopyası · 9.4 Tek işlevler | Y-2b ✓ |
| §10 Veri erişimi | 10.1 Silme davranışı, anahtar, enum · 10.2 Sürüm, eşzamanlılık, sayaç · 10.3 Bölüm sürümünün çatıda eşlenişi · 10.4 Önbellek ve kuşak sayacı · 10.5 Liste sırası, sayfalama, arama sözcüğü · 10.6 Göçlerin yazılışı · 10.7 Veritabanı oturumunun süreleri · 10.8 Veritabanı kullanıcıları · 10.9 İç notun kilidi · 10.10 Dışa aktarma dosyası | Y-3a (10.1–10.3) ✓ · Y-3b (10.4–10.6) ✓ · Y-3c (10.7–10.10) ✓ |
| §11 Entegrasyon kuralları | 11.1 Bağlantı katmanının kalıbı · 11.2 Dış servis istemcileri · 11.3 Google kimlik belirteci · 11.4 E-posta ve belge metninin üretimi | Y-4a ✓ |
| §12 Sözleşme ve olay sürümleme | Değerlerin sürümlenmesi (alt bölümsüz) | Y-4b |
| §13 Arka plan iş kuralları | 13.1 İş kuyruğu, yeniden deneme, nabız · 13.2 Yönetim komutları · 13.3 Büyük veri dönüşümünün ilerleme kaydı | Y-4b |
| §14 Hassas hesaplama kuralları | 14.1 Kuruşa yuvarlama · 14.2 Sağlayıcı tutar dönüşümleri | Y-4b |
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
| Durum geçişi, yan etki, işlem izi, belge kopyası, tek işlev | §9 |
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
| `06 §4.63` (imha fonksiyonunun imzası — ikinci parametre `as_of`) | v0.40 | K-1335 |
| `06 §2.4.3` (kontrol kısıtının adı — `ck_` ön eki) | v0.41 | K-1341 |

**Anahtar terim taraması (K-1335).** İmza değişikliğinin terimleri bütün `Docs/`'ta arandı: `dispose_expired_audit_logs` ve `max_rows`. `06 §4.63` imzayı taşır ve güncellendi; `06 §7.3.3` ile imha fonksiyonunu adıyla anan `06` satırı imzayı yazmaz, değişmez; `05 §7.5.1` ve `§4.7.3` fonksiyonu adsız ve imzasız anar ("süreyi kendisi hesaplar" cümlesi `as_of` ile de doğrudur), değişmez; bu dokümanın park bloğundaki K-1067 satırı talimattır ve değişmez; karar kaydındaki eski K satırları tarihçedir ve değişmez. Kalan eşleşme yoktur.

**Anahtar terim taraması (K-1341).** Kontrol kısıtının adının terimleri bütün `Docs/`'ta arandı: `ck_` ve "kontrol kısıtı". `ck_` yalnız bu dokümanda geçer (§5.2, §10.1 madde 4); `06 §2.4.3` kısıt adlarını sayan tek yerdir ve `ck_`'yi eklemekle güncellendi. "Kontrol kısıtı" `06`'da 42, `08`'de 2 satırda geçer; hepsi kısıtı adsız ve kavram olarak anar (kavramın tanımı `06 §2.9` ve `§7.3`), ad vermez, değişmez. `05`, `07`, `02`, `04`, `10` ve `DEPLOY_RUNBOOK`'ta eşleşme yoktur; karar kaydındaki eski K satırları tarihçedir ve değişmez. Kalan eşleşme yoktur.

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

Model adları ve işleyici adları §5.3'te, hata kodlarının kaydı ve gösterilmemesi §8.2'de yazılır; bu alt bölüm süzgecin kalıbının evidir.

### 3.5 Ayarlar, ürün sabitleri ve süre ve tavan değerleri

Parametrenin üç katmanı (`02 §10.1.3`, `05 §8.4`; K-412) kodda üç ayrı evdir; yeni bir parametrenin katmanı tartışılmaz, `02 §11`'in satırından okunur (K-1330).

| Katman | Kodda evi | Değişiklik |
|---|---|---|
| Firma ayarı (`02 §11.1`) | Veritabanında firma ayarları satırı (`CompanySettings`, `06 §4.2`); okuma ve bölüm kaydı §10.3'ün kalıbıyla | Panelden, hemen geçerli |
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
| Tekil dizin çakışmasında işlemin yeniden yürütülmesi: ilk deneme dahil en çok 3 deneme | Kodda sabit | K-1336 |

Çelişki taraması (K-1328 (6)): 15 saniyelik kullanıcı çağrısı 20 saniyelik komut aşımından kısadır; 120 saniyelik üretim tavanı 15 saniyelik sayfa sorgularının toplamından uzundur; 150 saniyelik ters vekil süresi 120 saniyelik tavanı taşır.

### Kütüphaneler

Kütüphane yalnız bu tabloda seçilir; kuralı kullanan bölüm tabloya atıf yapar (yazım konvansiyonu 7). Her satırın eklenmesi §3.3'ün kanıtıyla gelir. Erişim tarihi hepsinde 2026-10-29'dur; "ekleme task'ında doğrulanır" diye işaretli olgu o task'ın kabul kriteridir. Çatıyla gelen araçlar (DataAnnotations — §3.4; `TimeProvider` — §7.1; `HtmlRenderer` — §11.4) ek paket ve lisans satırı taşımaz.

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
| `Microsoft.IdentityModel.Protocols.OpenIdConnect` | Google keşif belgesi ve anahtar kümesinin önbellekli alınması (`ConfigurationManager`) | MIT (bilgim tarihli; ekleme task'ında NuGet sayfasından ve deponun LICENSE dosyasından okunur) | doğrulanır | Microsoft paketi; .NET 10 uyumu ve sürüm ekleme anında tabloya yazılır | K-1346 |
| `Microsoft.IdentityModel.JsonWebTokens` | Kimlik belirtecinin imza, `iss`, `aud` ve ömür doğrulaması (`JsonWebTokenHandler`) | MIT (bilgim tarihli; ekleme task'ında okunur) | doğrulanır | Microsoft paketi; yukarıdaki paketin geçişli bağımlılığıdır, sürümü onunla sabitlenir | K-1346 |
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

   Modül klasörünün adı §5.1'in modül kod adıdır. Modülün varlık yapılandırmaları kendi klasöründedir; **veritabanı bağlamı tektir ve `Infrastructure/` altındadır** — bir modül kendi bağlamını açmaz. Göçler `Infrastructure/` altındadır, bir modülün klasörüne girmez (göç tek şemanın sırasıdır — §10.6).
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

Kural `06 §2.4`'tedir (küçük harf ve alt çizgi, çoğul tablo adı, `pk_`/`fk_`/`ix_`, kontrol kısıtında `ck_`); bu alt bölüm **kodda nasıl uygulandığını** yazar (K-1309).

1. **Dönüşümü bir kütüphane yapar.** Veritabanı bağlamının kurulumunda tek çağrı `UseSnakeCaseNamingConvention()` (EFCore.NamingConventions — Kütüphaneler tablosu): tablo, sütun, anahtar, dizin ve kısıt adları kod adından küçük harf ve alt çizgiye çevrilir. Tek çağrı noktası bağlamın kurulumudur; model kuralı başka yerde çağrılmaz.
2. **Tablo adı çoğuldur ve `DbSet` özelliğinin adından gelir.** Her varlık türünün — bağ tabloları dahil — bağlamda bir `DbSet` özelliği vardır ve özelliğin adı varlık adının çoğuludur; çoğullaştırıcı kütüphane kullanılmaz.

   ```csharp
   // Veritabanı bağlamı (Infrastructure/): özellik adı çoğul, tablo adı ondan.
   public DbSet<Order> Orders => Set<Order>();                  // orders
   public DbSet<OrderItem> OrderItems => Set<OrderItem>();      // order_items
   public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();  // product_categories (bağ tablosu)
   ```
3. **Elle ad verilmez.** `ToTable("…")` ve `HasColumnName("…")` yazılmaz; dizin, anahtar ve yabancı anahtar adı verilmez (`06 §2.4.3`); kontrol kısıtının adı çatıda zorunludur ve §10.1 madde 4'ün kuralından türer (K-1337). Tek istisna aşağıdaki veri koruma paketinin varlığıdır ve o da elle ad gerektirmez.
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
4. **Hata kodu.** İngilizce PascalCase'tir (`TextTooLong`, `FieldRequired`); bir kez verilen kod değişmez; yanıtın gövdesine ve başlığına yazılmaz ve kullanıcıya gösterilmez. Kayıtlar her reddin kodunu yazar (§8.2). Kod listesi `07 §6.2.2`'dedir; yeni kod bu dokümanda doğmaz.
5. **Çerez adı** `07`'nin kapalı listesidir ve sabit olarak bir yerde tanımlanır (`Infrastructure/`): `__Host-session`, `__Host-admin-session`, `__Host-antiforgery`, `__Host-cart`, `__Host-known-device`, `__Host-checkout`, `__Host-external-login`. Çerez adı başka yerde dizgi olarak yazılmaz; listeye yeni ad eklemek `07 §2` ve `02 §12.2.5`'in kapalı envanterini değiştirir.
6. **`notice` değeri** `07 §6.3`'ün kapalı listesidir; bilinmeyen değer yok sayılır, serbest metin gösterilmez.

*Kaynak: K-1162, K-1163, K-1165, K-1166, K-1168, K-1172, K-1173, K-1174, K-1196, K-1198, K-1205, K-1226 · K-1331 (5) · `07 §2.1`, §2.2, §3, §3.1.1, §5, §6.*

## 6. Mimari sınırlar

Sınır dokuz modül arasındadır (`05 §3.3`); derleyici korumaz (§4.1), mimari test korur. Bu bölüm sınırın kodda biçimini, kuralları ve testin aracını yazar (K-1308).

1. **Açık yüzey bir `Contracts` ad alanıdır.** Her modülün başka modüle verdiği yüzey `Shopfolio.Modules.<Modül>.Contracts` ad alanındadır: genel arayüzler ve onların aldığı ve verdiği **değişmez `record`'lar**. Varlıklar `Contracts`'tan dışarı çıkmaz. Modülün geri kalanı — varlıklar, iş mantığı, yapılandırmalar, kuyruk işleyicileri — `internal` olur.
2. **Başvuru kuralı.** Bir modül ötekine yalnız onun `Contracts` ad alanından başvurur — alan, parametre, dönüş türü ve metot gövdesindeki çağrı dahil. Başka modülün varlığına **veritabanı bağlamı üzerinden de dokunulmaz.** Sayfalar, yönetim komutları ve arka plan işleri de yalnız `Contracts`'a başvurur ve veritabanı bağlamına ya da varlığa dokunmaz (`05 §3.3.12`). Ortak altyapı modül türlerine başvurmaz.
3. **Kayıt ve kullanım.** Her modül `Add<Modül>Module` uzantısıyla kendi hizmetlerini kaydeder (`AddOrderingModule`); `Program.cs` dokuzunu çağırır. Arayüz kurucudan enjekte edilir. **Arayüz metodu kendi veritabanı işlemini açmaz ve kesinleştirmez** — çağıranın işlemine katılır (`05 §3.4.2`); işlemin sahibi §9.1, madde 3'tedir.

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
5. **Tek yazıcı kuralı** (`05 §3.4.4`) mimari testle sınanamaz; kalıbı ve sınaması §9.1, madde 8'dedir.
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

Zamanın kaynağı ve temsili `05 §7.2`'dedir; istekteki tarih ve tutarın biçimi `07 §2.9`'dadır. Bu bölüm ikisinin kodda nasıl korunduğunu yazar (K-1333). Sayfanın gösterdiği tarih, saat, tutar ve sayı `04 §8.2`'nindir ve bu bölümün konusu değildir.

### 7.1 Tek zaman kaynağı

1. **Zaman tek kaynaktan okunur: `TimeProvider`** (`System` ad alanı, .NET 8'den; bilgim tarihli). Uygulama `TimeProvider.System`'i bir kez tekil olarak kaydeder (`Infrastructure/`; `05 §7.2.4`); zamana ihtiyaç duyan sınıf `TimeProvider`'ı kurucudan alır ve anı `GetUtcNow()` ile okur. Başka okuma yolu yoktur: `DateTime.Now`, `DateTime.UtcNow`, `DateTime.Today`, `DateTimeOffset.Now`, `DateTimeOffset.UtcNow` ve `TimeZoneInfo.Local` uygulama kodunda çağrılmaz. **Veritabanı da zaman kaynağı değildir:** SQL'de `now()`, `current_timestamp`, `clock_timestamp()` yazılmaz ve sütun varsayılanı olarak an atanmaz; uygulamanın okuduğu an sorguya parametre olarak verilir — "zamanı gelmiş kayıtlar" sorgusu dahil (`05 §5.1.1`) — ve oluşturulma anını (`created_at`, `06 §2.8.4`) uygulama yazar. İmha fonksiyonunun (`06 §2.2.3`) anı nasıl aldığı §9.2, madde 6'da yazılır; kural aynıdır: an uygulamadan gelir. Süre ölçümü (bir isteğin süresi gibi) iş zamanı değildir; `Stopwatch` kullanılabilir. Döngülerin bekleme aralığı (`PeriodicTimer`, `Task.Delay`) iş zamanını belirlemez — iş zamanı kaydın kendi anıdır (`05 §5.1.1`).

   ```csharp
   // Yanlış: sunucunun saatine ve saat dilimine bağlı; sınanamaz.
   var isOpen = DateTime.Today >= discount.StartDate.ToDateTime(TimeOnly.MinValue);

   // Doğru: an tek kaynaktan, gün sınırı Türkiye saatiyle (kural 3).
   var now = _time.GetUtcNow();
   var isOpen = now >= TurkeyTime.StartOf(discount.StartDate)
             && now <  TurkeyTime.StartOf(discount.EndDate.AddDays(1));
   ```
2. **Türler** `06 §2.5.1`'in satırlarının kodudur. **An** `DateTimeOffset`'tir ve ofseti sıfırdır (UTC; `timestamptz`); **tarih** `DateOnly`'dir (`date`); **para** `decimal`'dir (`numeric(12,2)`). Varlıkta, modelde ve arayüzde `DateTime` bulunmaz: `Kind`'ı yanlış ya da belirsiz kalan değer saat dilimini sessizce kaydırır. Veri sağlayıcı `timestamptz` sütununa yalnız ofseti sıfır olan değeri yazar (bilgim tarihli, ilk task'ta doğrulanır); bu yüzden Türkiye ofsetli bir `DateTimeOffset` hiçbir zaman varlığa ya da sorguya girmez — Türkiye saati yalnız kural 3'ün yardımcısında, gün sınırı hesabında doğar.
3. **Türkiye saati tek yardımcıdadır.** `Infrastructure/` altında statik `TurkeyTime`: sabit ofset (+03:00 — ürün tek saat dilimiyle çalışır ve yaz saati geçişi yoktur, `02 §4.1.1`), `DateOf(an)` (anın Türkiye saatiyle günü, `DateOnly`) ve `StartOf(gün)` (o günün 00:00'ının UTC anı). Sunucunun saat dilimi sonucu etkilemez ve saat dilimi veritabanına (`TimeZoneInfo.FindSystemTimeZoneById`) bağlı olunmaz. Gün ve süre sayımının hepsi bu iki işlevden geçer:
   - **Gün olarak belirlenen süre** başlangıç gününün ertesi günü başlar ve son günün sonunda dolar (`05 §7.2.2`; `02 §4.1.4`): `StartOf(başlangıç.AddDays(1)) <= an < StartOf(başlangıç.AddDays(n + 1))`. Tarih aralığında bitiş günü dahildir: `an < StartOf(bitiş.AddDays(1))` (örnek yukarıda). Gün sınırını yazılı `23:59` gibi bir saatle karşılaştıran kod yazılmaz — saat bazlı sayım yoktur (`02 §4.1.4`) ve tarih alanı gündür.
   - **Dakika ve saatle ölçülen süre** başladığı andan işler: `an + TimeSpan` (gün sayımına girmez, `02 §4.1.4`).
   - **İş günü** hesabı ürünle gelen tatil dosyasından okunur (`05 §7.2.3`) ve gün cinsinden `DateOnly` üzerinde yürür; sonuç anı `StartOf` ile UTC'ye çevrilir.
4. **Sınanabilirlik.** Birim testleri `TimeProvider.System` kullanmaz; `TimeProvider`'dan türeyen, `GetUtcNow()`'u ezen ve `SetUtcNow`, `Advance` yöntemleri olan **elle yazılmış durumlu bir sahte** kullanır (sahte nesne kütüphanesi yoktur — K-1306; çatının test paketi Kütüphaneler tablosunda olmadığından eklenmez, eklenirse §3.3'ün kanıtıyla tabloya girer). Sahte `UnitTests` projesinin ortak yardımcılarındadır. Zamana bağlı her karar — "zamanı gelmiş kayıtlar", sürenin dolması, gecelik işin başlama koşulu (`05 §5.1.1`) — anı parametre alan bir yöntemdir ve döngü yalnız onu çağırır: testler beklemez, anı sahteyle ilerletir. Sınır anları sınanır: gün sonundan bir tik önce içeride, ertesi günün 00:00'ında dışarıda; test sunucunun saat diliminden bağımsız geçmelidir (CI UTC'de, geliştirme makinesi +03:00'te koşar).
5. **Mimari test.** Kural 1'in yasak listesi `Architecture/` klasöründe bir testtir (§6, madde 4) ve kendi kendini sınar: kasıtlı ihlal eden örnek kod ayrı derlemededir ve test o derlemede kırıldığını da doğrular. ArchUnitNET'in yöntem çağrısı bağımlılığını denetleyebildiği bilgim tarihlidir ve ilk task'ta doğrulanır; denetleyemiyorsa aynı kural kaynak metin taramasıyla sınanır. SQL metinlerindeki `now()` ve benzerleri için ayrı bir tarama testi vardır (göç dosyaları dahil; göçteki veri düzeltmesi anı sabit değer olarak yazar).

*Kaynak: K-1333 · K-870, K-934, K-895, K-1306 · `05 §4`, §5.1.1, §5.2.14, §7.2 · `06 §2.2.3`, §2.5.1, §2.5.2, §2.8.4 · `02 §4.1`.*

### 7.2 İstekteki tarih, tutar ve ondalık biçimi

Biçimin kuralı `07 §2.9`'dadır ve burada yinelenmez; bu alt bölüm bağlama ve ayrıştırmanın kalıbını yazar (K-1333). Metin girdisinin kırpılması §3.4'tedir (madde 5): aşağıdaki ayrıştırıcılar kırpılmış değeri alır.

1. **Kültür açıktır, tek yerdedir.** İstekteki tutar ve ondalık sayı her zaman Türkçe kültürle ayrıştırılır; kültür `Infrastructure/` altında statik `RequestFormat.Culture` (`tr-TR`) olarak bir kez tanımlanır. İş parçacığının kültürü ve tarayıcının `Accept-Language`'ı ayrıştırmayı etkilemez (`07 §2.9.9`). Uygulama değişmez küreselleşme kipinde (`InvariantGlobalization`) çalışmaz: Türkçe kültür ve Türkçe sıralama ICU ister (`06 §2.8.3`); uygulama imajında ICU'nun bulunması ilk task'ta doğrulanır ve `tr-TR` ondalık ayırıcısının virgül olduğunu sınayan bir birim testi vardır (bilgim tarihli).
2. **Tarih** istekte `yyyy-AA-GG`'dir (`07 §2.9.3`) ve `DateOnly` olarak ayrıştırılır; biçim sabit olduğundan kültür değişmezdir. İstek saat taşımaz; `DateTime` ve `DateTimeOffset` bağlanmaz. Gün, Türkiye günü olarak okunur (§7.1, madde 3). Ayrıştırılamayan değerin kodu `InvalidDate`'tir (`07 §6.2.2`).
3. **Tutar ve ondalıklı sayı** — yüzde ve net miktar dahil — `decimal`'dir ve **yalnız `AllowDecimalPoint` stiliyle** ayrıştırılır: binlik ayırıcı, işaret, boşluk, üstel gösterim ve para birimi stilde yoktur. `tr-TR`'de nokta binlik ayırıcıdır; stil onu kabul etmediğinden `12.50` ayrıştırılamaz ve sessizce 1250 olmaz (`07 §2.9.4`). Kabul edilen biçim rakamlar ve en çok bir virgüldür. Hassasiyet ve sınırlar ayrıştırmadan sonra, alanın doğrulama niteliğindedir (§3.4): ayrıştırılamayan değerin kodu alanın kural türünün kodudur (tutarda `InvalidAmount`, yüzdede `InvalidPercentage`); ikiden fazla ondalık, negatif değer ve `numeric(12,2)` sınırı da niteliğin işidir. **Adet** ve gün sayısı gibi tam sayı alanları tarayıcının sayı alanıdır ve `int` olarak, `NumberStyles.None` ile ve değişmez kültürle ayrıştırılır (adetin kodu `InvalidQuantity`).

   ```csharp
   public static class RequestFormat
   {
       public static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("tr-TR");

       public static bool TryParseDecimal(string value, out decimal result) =>
           decimal.TryParse(value, NumberStyles.AllowDecimalPoint, Culture, out result);

       public static bool TryParseDate(string value, out DateOnly result) =>
           DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out result);
   }

   // Yanlış: iş parçacığının kültürü ve geniş varsayılan stil — tr-TR'de "12.50" 1250 olur.
   var amount = decimal.Parse(input);

   // Doğru: açık kültür, dar stil — "12.50" ayrıştırılamaz, "12,5" 12,5 olur.
   if (!RequestFormat.TryParseDecimal(input, out var amount)) { /* InvalidAmount */ }
   ```
4. **Model bağlayıcı.** Ayrıştırma çatının varsayılan basit tür bağlamasına bırakılmaz (o, iş parçacığının kültürüyle ve geniş stille çalışır). `decimal`, `decimal?`, `DateOnly` ve `DateOnly?` için projenin iki model bağlayıcısı vardır ve bağlayıcı sağlayıcı listesinin başına eklenir (bilgim tarihli, ilk task'ta doğrulanır). Bağlayıcı ayrıştırma başarısızlığında `ModelState`'e kural türünün kodunu hata iletisi olarak ekler (§3.4, madde 1); gerisini sayfa süzgeci yapar (§3.4, madde 3). Boş değer `null`'dur (zorunluysa `FieldRequired`). Sorgu modelinde (GET) aynı bağlayıcılar çalışır; geçersiz sorgu parametresinin süzgeci uygulamaması ve sayfanın durumu `07 §2.6.6`'dadır, bu bölüm onu değiştirmez.
5. **Dizinli liste.** Birden çok satırlı alan (`Options`, `Values`, `Variants`, `Images`, `Items`, `Categories` — `07 §5.2`) çatının dizinli bağlamasıyla `Ad[n].Alan` adıyla bağlanır; `n` satırın formdaki sırasıdır (görsel yüklemenin hedefi `Variants[n].ImageId` gibi bu sıraya başvurur — `07 §5.1`). **Yeni ve tamamen boş bırakılan satır okunmaz** (`07 §9`, ürün formu; K-1189 (2)): kimliği olmayan (`VariantId`, `OptionId` gibi) ve bütün alanları kırpıldıktan sonra boş olan satır, bağlamadan sonra ve doğrulamadan önce çalışan tek bir yardımcıyla listeden çıkarılır ve hata doğurmaz. Kimliği olan satır hiçbir zaman atlanmaz: alanı boşsa `FieldRequired` doğar. Atlama, formdaki `n` sırasına bağlı başvuruları (yüklemenin hedefi) bozmaz — bozmadığı testle sınanır.
6. **Test.** Ayrıştırıcı tablosu: `12,5` ve `12` geçer; `12.50`, `1.250,50`, `12,5,1`, `-5`, `12 TL`, `1e3` ve boş değer ayrıştırılamaz (`12,` ve `,5` çatının ayrıştırıcısının sonucuna bağlıdır ve ilk task'ta tabloya yazılır — bilgim tarihli). Tarih tablosu: `2026-10-04` geçer; `04.10.2026`, `2026-4-1`, `2026-10-04T10:00` ve var olmayan gün (`2026-02-30`) `InvalidDate` doğurur. Bağlayıcı testi: form alanında 422 ve alan kodu; sorgu parametresinde süzgeç uygulanmaz. Dizinli liste testi: boş yeni satır atlanır, kimlikli boş satır `FieldRequired` doğurur, atlanan satır sonraki satırların hedef başvurusunu bozmaz.

*Kaynak: K-1333 · K-1174, K-1214, K-1319 (5) · K-1189 (2) · `07 §2.6.6`, §2.9, §5.1, §5.2, §6.2.2, §9 · `06 §2.5.1`, §2.8.3 · `04 §8.2`.*

## 8. Hata yönetimi

Beklenmeyen hatanın ekranı `04 §2.7.3`'te, mekanizması `05 §10.3`'te, HTTP durumları `07 §2.7`'de, kodları `07 §6`'dadır. Girdi doğrulama hatasının kalıbı §3.4'te yazılıdır ve burada yinelenmez; bu bölüm beklenmeyen hatanın yakalanışını (8.1) ve hata kodlarının kaydını (8.2) yazar (K-1334).

### 8.1 Tek yakalama katmanı, üç hâl, sonucu belirsiz işlem

1. **İstisna yalnız beklenmeyen hâl içindir.** Adı konmuş red — doğrulama, iş kuralı, sürüm çakışması, bulunamayan kayıt, hız sınırı — istisna değil işlemin sonucudur (`05 §10.3.5`): modül arayüzü reddi dönüş değeriyle bildirir (§8.2), sayfa onu `07 §6`'nın durumuna çevirir. Hata sınıfı atılan istisna türünden değil, kodundan okunur.
2. **Yakalama katmanı tek sınıftır** (`Infrastructure/`, §4.1). Sınıf istisnayı hâle çevirir, kaydeder ve yanıtı üretir; çatının iki giriş noktası onu çağırır ve ikisi de karar vermez: (a) işleyiciyi saran genel bir istisna süzgeci — POST'ta formun bağlanmış modeline ve `ModelState`'e erişmenin tek yeri budur — ve (b) işleyiciye ulaşmadan düşen hatalar için çatının istisna işleme ara katmanı (yedek). Çatı sınıflarının adı ve kayıt biçimi bilgim tarihlidir ve ilk task'ta doğrulanır; iki giriş noktası tek sınıfa indirilemezse (a) korunur.
3. **Üç hâl** isteğin türüne ve işlemin işaretine göre ayrılır (`05 §10.3.1`); ayrım saf bir işlevdir (istek yöntemi, makine ucu mu, işaretli işlem mi) ve tabloyu birebir uygular:

   | Hâl | İstek | Yanıt | Kaydedilen kod | Kaynak |
   |---|---|---|---|---|
   | Sayfa düzeyi | GET | 500; vitrinin ya da panelin çerçevesi içinde hata sayfası | `UnexpectedPageError` | `05 §10.3.2`; `04` 2.7.3.2 |
   | İşlem düzeyi | POST | 500; sayfa formun gönderilen değerleriyle yeniden gösterilir ve kalıcı mesaj çıkar; kesinleşmemiş işlem geri alınmıştır | `UnexpectedOperationError` | `05 §10.3.3`; `04` 2.7.3.1 |
   | Sonucu belirsiz | işaretli işlemin POST'u | 303; sonucu gösteren ekran; "işlem yapılmadı" gösterilmez | `OutcomeUncertain` | `05 §10.3.4`; `04` 2.7.3.3; `07 §6.4.2` |
   | Makine ucu | bildirim, durum, canlılık, parçalı yükleme | gövdesiz 500; ekran yok | `UnexpectedOperationError` | `05 §10.3.5`; `07 §2.7.2` |

   Makine ucu işleyicinin üzerindeki bir nitelikle işaretlidir; ayrım istek yolundan türetilmez. Katman yalnız bu dört satırı üretir; çatının kendi yanıtları (405, 416, işleyicisi bulunamayan POST) katmana düşmez ve davranışı Doğrulama Protokolü'nün senaryosuyla sabitlenir (`07 §2.7.1`).
4. **Sonucu belirsiz işlem kodda adıyla işaretlidir:** işleyici metodunda bir nitelik (`[OutcomeUncertain]`) taşır; işaretli işlemlerin listesi `04` 2.7.3.3 ve `07 §6.4.2`'dir (sipariş onayı, müşterinin geri alınamaz dört işlemi, e-posta değişikliğinin geri alınması, yöneticinin onay isteyen işlemleri). İşaretli işlemde hata **nerede olursa olsun** — işlemin içinde, kesinleşmeden sonra, yanıt üretilirken — katman 303 ile sonucu gösteren ekrana yönlendirir; hedef ekran `07 §6.4.2`'nin satırıdır ve hesap türüne bağlı hedef (sipariş onayında üyede `/hesap/siparisler`, misafirde `/siparis-takibi`) hedefi seçen tek yöntemle sayfa modelinde yazılır. Listede olmayan formun işlemi işlem düzeyidir (`05 §10.3.7`, "kalan durum"). İşaretli işleyici kümesi `07 §6.4.2`'nin UC listesine eşit tutulur (eşitliğin denetim betiği §21'dedir, yazım 6 — iş adı: işaretli işlem listesi eşitliği denetimi).
5. **İşlemi geri almak kodun işi değildir, kesinleştirmemektir.** Veritabanı işleminin sahibi §9.1, madde 3'tedir; katman işlem kesinleştirmez, kesinleşmemiş işlem istisnayla elden çıkar ve geri alınır. `finally` içinde kesinleştirme çağrısı yazılmaz.
6. **Geniş `catch` yalnız üç yerdedir:** yakalama katmanı; iş kuyruğunun satır döngüsü — bir satırın hatası satıra yazılır, döngü durmaz (`05 §4.3.2`; §13, yazım 4); dış servis bağlantı katmanlarının sonuç eşlemesi — özel durum sonuç sınıfına çevrilir, hiçbiri dışarı sızmaz (`08 §2.1.8`; §11, yazım 4). Başka yerde `catch (Exception)`, türsüz `catch` ve hatayı yutan `catch` yoktur; bilinen bir istisnayı bilinen bir reddin sonucuna çevirmek için belirli türü yakalamak serbesttir. Kural yerleşik çözümleyiciyle zorlanır: `.editorconfig`'te CA1031'in önem derecesi `error`'dır (bilgim tarihli; ilk task'ta doğrulanır) ve üç yerin bastırması gerekçesini taşır (§3.1, madde 3). Arka plan hizmetinin hatasını yok sayan ayar kullanılmaz (§13).

   ```csharp
   // Yanlış: işleyici beklenmeyen hatayı yutuyor; yakalama katmanı onu hiç görmez, kayıt ve sayaç eksik kalır.
   try { await _ordering.PlaceOrderAsync(command, ct); }
   catch (Exception) { return Page(); }

   // Doğru: işleyici yakalamaz; red dönüş değeridir, beklenmeyen hata katmana çıkar.
   // Arayüzün adı ve metodu ilgili task'ın tasarımında 07'nin ucuna göre yazılır; burada yalnız kalıp gösterilir.
   var result = await _ordering.PlaceOrderAsync(command, ct);
   ```
7. **Kayıt ve sayaç.** Her beklenmeyen hata uygulama kaydına hata düzeyinde bir satırdır; alanlar `05 §10.3.6`'nındır — zaman, istek kimliği, istek yöntemi ve maskelenmiş yol, hatanın türü ve yığın izi, düştüğü hâl, işaretli işlemin adı, hesap türü ve hesabın iç kimliği. Form değeri, çerez ve `05 §9.1.5`'in maskeledikleri yazılmaz; istisnanın iletisi ve veri sözlüğü de maskelemeden geçer. Alan adları ve maskelemenin uygulanışı §18.2'dedir (yazım 5). Katman durum ucunun son on beş dakikalık sayacını besler (`05 §9.4.8`): 500'ler sayılır; 422, 409 ve 429 beklenmeyen hata değildir ve girmez (`07 §2.7.5`). Kullanıcıya hata kodu, istek kimliği ve teknik ayrıntı gösterilmez. Hata sayfasının kendisi üretilemezse katman kendi hatasını yakalar ve çatının en yalın 500 yanıtını döner (`05 §10.3.6`).
8. **İstemcinin vazgeçmesi beklenmeyen hata değildir.** İstemcinin bağlantıyı kesmesinden doğan iptal (`RequestAborted` ve onun `OperationCanceledException`'ı) hata satırı yazmaz ve sayaca girmez; erişim kaydı isteği zaten taşır. İptal edilen işlem kesinleşmemişse geri alınır.
9. **Test.** Sınıflandırma tablosunun her satırı (birim); her hâlde yanıt (entegrasyon): GET'te çerçeveli hata sayfası, POST'ta formun değerleriyle yeniden gösterim ve geri alınmış işlem, işaretli işlemde 303 ve hedef, makine ucunda gövdesiz 500; işaretli işlemde **kesinleştikten sonra** düşen hatanın da 303 verdiği (kesinleşmeden sonra istisna atan sahteyle); kayıt satırı alanları ve satırda form değeri ile çerezin bulunmayışı; hata sayfası düşerse en yalın 500; istemci iptalinde satırın yazılmayışı; işaretli işleyici kümesinin `07 §6.4.2`'ye eşitliği; CA1031'in üç yerin dışında derlemeyi kırması.

*Kaynak: K-1334 · K-914, K-975, K-873, K-874, K-891, K-893, K-902, K-909, K-915, K-917, K-918 · K-1111, K-1132 (arka plan döngüsü) · `05 §4.3.2`, §9.1.5, §9.4.8, §10.3 · `04` 2.7.3, 2.7.4 · `07 §2.7`, §6.1, §6.4 · `08 §2.1.8`.*

### 8.2 Hata kodlarının kaydı ve gösterilmemesi

1. **Kayıt tek enum'dur: `ErrorCode`** (`Infrastructure/`; ortak altyapı modül türlerine başvurmaz, §6 madde 2). Üyeler `07 §6.2`'nin (6.2.1…6.2.9) "Kod" sütunundaki adlardır ve aynı yazılışlıdır; bunlara `OutcomeUncertain` eklenir (`07 §6.4.2`; yalnız kayıtta). Kısa süreli bildirim kodları ayrı bir enum'dur: `NoticeCode`, `07 §6.3`'ün listesi (§5.3, madde 6). **Yeni kod bu dokümanda doğmaz** (§5.3, madde 4): bir ucun yeni hâli önce `07 §6`'ya yazılır, sonra enum'a girer. Bir kez verilen kod değişmez ve silinmez (`07 §2.7.3`); üyenin sayısal değeri anlam taşımaz — kodlar kayda ve karşılaştırmaya adıyla girer, yeniden numaralamak kaydı bozmaz. Kodun HTTP durumu `07 §6.2`'nin HTTP sütunudur ve kodda tek bir eşlemede durur (`ErrorCode` → durum); sayfa durumu o eşlemeden alır.
2. **Red kodla taşınır.** İş kuralı reddi modül arayüzünden `ErrorCode` ile döner (§8.1, madde 1); alan doğrulamasında kodu üreten nitelik ve süzgeçtir (§3.4). Kod başka bir yerde üretilmez: hata iletisi, durum ya da yönlendirme kodun kendisinden çıkar.
3. **Kayıt her reddin kodunu yazar** (`07 §6.1.3`; K-1198). Alan doğrulamasında satır her alanın adını ve kodunu taşır (§3.4, madde 3); öteki redlerde satır kodu, ucu ve istek kimliğini taşır. **Alanın değeri yazılmaz** (`05 §9.1.5`). Satır varsayılan kayıt düzeyinde yazılır ve düzey ayarı onu kapatabilir (`05 §9.1.6`). Satırı yazan iki yer vardır: sayfa süzgeci ve redleri sayfa sonucuna çeviren tek yardımcı; sonucu belirsiz hâlin `OutcomeUncertain` satırını §8.1'in katmanı yazar. Kayıt alanlarının adları §18.2'dedir (yazım 5).
4. **Kod gösterilmez** (`04` 2.7.3; `07 §2.7.3`). Kod yanıtın gövdesine ve başlığına, sayfanın metnine ve niteliklerine (`data-*` dahil), çereze ve yönlendirme adresine yazılmaz; tek istisna `notice` parametresidir ve onun değeri `NoticeCode`'dur (hata kodu değil). Kullanıcının gördüğü mesaj koddan mesaj kataloğuyla üretilir (§3.4, madde 4); görünüm yalnız mesajı alır. Aynı kod gerektiren bir ayrım sayfada gerekirse (ör. hangi alanın işaretleneceği) alanın adı ya da durum taşınır, kod değil.
5. **Test.** Enum üyeleri ile `07 §6.2` kod sütununun eşitliği (denetim betiği §21'dedir, yazım 6 — iş adı: hata kodu envanteri eşitliği denetimi); her üyenin bir HTTP durumu vardır ve eşleme `07 §6.2`'nin sütunuyla uyuşur; **her hâl için yanıtın gövdesi ve başlıkları kapalı listenin hiçbir üyesinin adını içermez** (entegrasyon testi: 422, 409, 404, 429, 500 ve 303 hâlleri; yanlış pozitif — gövdede doğal bir sözcüğün enum üyesiyle çakışması — testte listeden dışlanır ve gerekçesi yazılır); reddin kayıt satırı kodu taşır, değeri taşımaz; `OutcomeUncertain` satırı; mesaj kataloğu testi §3.4'tedir.

*Kaynak: K-1334 · K-1319 (1), (3), (4) · K-1196, K-1198, K-1168 · `07 §2.7.3`, §6.1, §6.2, §6.3, §6.4.2 · `05 §9.1.5`, §9.1.6 · `04` 2.7.3.*

## 9. Domain kuralları

Bu bölüm durum geçişinin, yan etkinin, işlem izinin, belge kopyasının ve tek işlevlerin **nasıl kodlandığını** yazar; kuralın kendisini yinelemez. Kural kaynaktadır: geçiş tabloları `05 §4.1`, `06 §5.2`; yan etki sırası `05 §4.3`, `05 §3.4`; işlem izi `05 §4.7`, `02 §10.3`; belge kopyası `06 §4.17`, `02 §3.23.3`. Burada olmayan bir ad uydurulmaz (§1); bu bölümün koyduğu adlar yazımın kararıdır ve ilk task'ta kodda doğrulanır.

### 9.1 Geçiş birimi, beyaz liste, yan etki

1. **Geçiş birimi tek sınıftır.** `Shopfolio.Modules.Ordering`'in açık yüzü `Contracts.IOrderTransitions`, uygulaması `internal OrderTransitions`'tır. Her geçiş bir metottur; metodun adı işlemin adıdır (§5.3: fiil ve sözlük nesnesi) ve geçişin `TransitionCode` sabiti metodun içinde tek yerde durur. Düzeltmeler (`Correction`) ayrı metotlardır ve yalnız yönetici çağırır (`ActorKind`). Metot listesi burada yazılmaz: ilgili task'ın tasarımı onu `02 §5.4`, `§5.5` ve `§10.4`'ten çıkarır. `IOrderTransitions`, `OrderTransitions` ve `TransitionTable` adları yazımın kararıdır (K-1335).
2. **Beyaz liste `TransitionTable`'dır** (`internal static`, `Ordering/`): `ShipmentTransition(TransitionCode, OrderStatus From, OrderStatus To)` ve `PaymentTransition(TransitionCode, PaymentStatus From, PaymentStatus To)` kayıtlarının tek listesi. On altı satırdır (`06 §5.2.1`, `§5.2.2`, `§5.2.6` ile eşit):

   | Kod | Geçiş | Kod | Geçiş |
   |---|---|---|---|
   | `S1` | `Placed` → `Preparing` | `S8` | `DeliveryFailed` → `Shipped` |
   | `S2` | `Placed` → `Delivered` | `S9` | `DeliveryFailed` → `Cancelled` |
   | `S3` | `Placed` → `Cancelled` | `S10` | `Preparing` → `Delivered` |
   | `S4` | `Preparing` → `Cancelled` | `S11` | `DeliveryFailed` → `Delivered` |
   | `S5` | `Preparing` → `Shipped` | `O1` | `Pending` → `Paid` |
   | `S6` | `Shipped` → `Delivered` | `O2` | `Pending` → `Failed` |
   | `S7` | `Shipped` → `DeliveryFailed` | `O3` | `Paid` → `PartiallyRefunded` |
   | `O4` | `Paid` → `Refunded` | `O5` | `PartiallyRefunded` → `Refunded` |

   `Correction` tabloda değildir: durumu değiştirmez. Koşul (gecikme feshi, açık kalem, hizmet penceresi) tabloda değil metodun gövdesindedir; sıra önce beyaz liste, sonra koşuldur.
3. **İşlemin sahibi giriş noktasıdır.** Sayfalar ve işler `DbContext`'e dokunmaz (§6 madde 2); işlemi `Infrastructure/ITransactionRunner` açar: `RunAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken ct)`. İş dönerse `SaveChangesAsync` çağrılır ve işlem kesinleşir; istisnada geri alınır ve istisna yeniden fırlatılır. Modülün arayüz metodu `RunAsync` çağırmaz, çağıranın işlemine katılır; iç içe `RunAsync` programlama hatasıdır. `finally`'de kesinleştirme yoktur (§8.1 madde 5). Dış çağrı — ödeme yönlendirmesi, e-posta, bağlantı katmanı — `RunAsync`'in dışındadır (`05 §3.6.3`). `ITransactionRunner` adı ve kalıbı yazımın kararıdır (K-1335).
4. **Sıra sabittir** (`05 §4.1.2`): kilit → beyaz liste → koşul → durum alanı → `TransitionRecord` → iç yan etkiler → dış yan etkiler. Durum alanını yalnız `Order.ApplyTransition` (`internal`) yazar ve onu yalnız `OrderTransitions` çağırır; `OrderStatus` ve `PaymentStatus` ayarlayıcısı `private`'tır. Geçiş kaydı (`TransitionRecord`) sipariş, `TransitionCode`, `ActorKind`, yönetici kimliği ve adı, gerekçe (`TransitionReason`, `CancellationReason` ya da `CorrectionReason`), serbest not ve düzeltilen kayıt (`CorrectedTransitionRecordId`, `06 §4.19`) alanlarını taşır. Kalem kaydı geçiş kaydına yazılmaz; kapanış geçişi (`S3`, `S4`, `S9`; `TransitionReason.Closure`) aynı işlemde ve aynı anda yazılır.

   ```csharp
   // Sıra yorumdadır; red dönüş değeridir, kod 07 §6.2'den okunur — burada ErrorCode üyesi uydurulmaz.
   await OrderLocks.LockAsync(new[] { orderId }, ct);          // 1 kilit
   var order = await _db.Orders.SingleAsync(o => o.Id == orderId, ct);
   if (!TransitionTable.Allows(TransitionCode.S5, order))      // 2 beyaz liste
       return Result.Rejected(/* 07 §6.2 kodu */);
   // 3 koşul (bu geçişe özgü)
   order.ApplyTransition(TransitionCode.S5);                   // 4 durum alanı
   _db.TransitionRecords.Add(/* 5 geçiş kaydı */);
   await _catalog.ReleaseAsync(/* 6 iç yan etki: Catalog arayüzü */);
   _db.Jobs.Add(/* 7 dış yan etki: Job satırı */);
   ```
5. **Kilit tek yerdedir.** `Ordering/OrderLocks.LockAsync(IReadOnlyCollection<Guid> orderIds, ct)` (`internal`) `SELECT … FOR UPDATE` ham SQL'ini (`FromSql`; bilgim tarihli) çalıştırır; siparişleri kimliğe göre sıralı kilitler (`05 §7.3.5`). Yönetici işleminde ekrandaki sürüm kilit altında karşılaştırılır (`05 §7.3.2`); sürüm artışının kalıbı §10.2'dedir. Kaynak taraması: sipariş satırını kilitleyen `FOR UPDATE` ve `FOR SHARE` (iç not, §10.9) yalnız `OrderLocks`'tadır.
6. **Yan etki iki türdür.** İç yan etki — stok, kontenjan ve kupon dönüşü (`Catalog` arayüzü), iz satırı (`Administration` arayüzü, §9.2), panel işaretleri — gövdede açık çağrıdır ve aynı işlemdedir. Dış yan etki `Job` satırıdır (`JobType`, `RelatedRecordId`, `JobState.Pending`; `06 §4.59`) ve aynı işlemde yazılır; kuyruğun arayüzü ve işleyicinin kalıbı §13.1'dedir (yazım 4). Geçiş birimi bir dış servisin arayüzüne ve `HttpClient`'a bağlı değildir; e-posta ayrı bir iş satırı yazmaz — gönderim kaydı onun kuyruğudur (`05 §4.4.1`; §11.4).
7. **Uygulama içi olay yayını yoktur** (`05 §3.4.6`): olay veriyolu kütüphanesi (§3.3 tablosunda yoktur), kendi yayıncı/abone arayüzü (`IEventBus`, `IDomainEvent`, `INotification`, `INotificationHandler<>` ad kalıpları), varlıkta biriken alan olayı listesi ve `SaveChanges` kesicisiyle dağıtım yazılmaz. Gerekçe: iç yan etkinin geçişle aynı işlemde yazıldığı gövdeden okunamaz olur. Bir geçişe tepki, arayüz çağrısıdır.
8. **Test.** Tablo on altı satırdır; her `TransitionCode` (`Correction` hariç) bir kez bulunur ve `02 §5.4`, `§5.5` ile eşittir (denetim betiği §21'dedir, yazım 6 — iş adı: geçiş listesi eşitliği denetimi). Birim: izinli geçiş tek geçiş kaydı yazar; tablo dışı kaynak durum reddedilir ve hiçbir şey yazmaz. Entegrasyon (gerçek PostgreSQL, §19.1): iç yan etki hatasında durum, kayıt, iz ve `Job` birlikte geri alınır; eşzamanlı iki geçiş sıraya girer; iki sipariş ters sırada kilitlenmez ve kilitlenme olmaz; sürüm uyuşmazlığı reddedilir. Mimari test (§6 madde 4; kasıtlı ihlalli örnekle kırmızı olduğu gösterilir): durum özelliklerinin ayarlayıcısı `private`; `ApplyTransition`'ı çağıran yalnız `OrderTransitions`; `ExecuteUpdate` ve ham SQL metninde `order_status` ve `payment_status` yoktur (göçler hariç); yasak ad listesi; `OrderTransitions` `HttpClient`'a bağlı değil. ArchUnitNET'in özellik ayarlayıcısı ve yöntem çağrısı denetimi bilgim tarihlidir; yoksa kaynak metin taraması (§7.1 madde 5 gibi).

### 9.2 İşlem izinin kod adları ve imha fonksiyonunun göçteki kalıbı

1. **Yazan tek yol** `Administration.Contracts.IAuditTrail.WriteAsync(AuditEntry, ct)`'tir. `AuditEntry` değişmez bir `record`'dur: `Operation` (`AuditOperation`), `TargetRecordType` (`06 §3`'ün kod adı; boş olabilir), `TargetRecordId`, `Details` (`AuditDetails`). Alan (`AuditArea`) çağırandan alınmaz: `AuditOperation` → `AuditArea` tek eşlemede durur. Yönetici kimliği, adı ve e-postası oturumdaki yöneticiden alınır (istek bağlamı; kimlik şemasının kalıbı §15.1, yazım 5). Yönetici yoksa — kuyruk, yönetim komutu, müşteri isteği — istisna: kurulum ve sistem olayları ize yazılmaz (`02 §10.3.3`, `05 §4.7.4`). Metot çağıranın işlemine katılır (§9.1 madde 3). `DbSet<AuditLog>` yalnız `Administration`'ın `AuditTrail` sınıfındadır (mimari test).
2. **`AuditOperation` bir C# `enum`'udur** (`Administration.Contracts`); veritabanına üye adıyla metin olarak yazılır (EF dizgi dönüşümü), sütunda `CHECK` yoktur. Tanınmayan kod panelde kodun kendisiyle gösterilir. Kod bir kez verilir; silinmez ve yeniden adlandırılmaz. Panelin Türkçe adı koddan mesaj kataloğuyla gelir (§3.4 madde 4). **Kilit dosyası (snapshot) testi:** üye adları kayıtlı listeyle karşılaştırılır; silme ve ad değişikliği testi kırar, ekleme dosyaya satır eklemekle olur. Her üyenin alanı ve etiketi vardır (test).
3. **Kod listesi (43 kod).** Yeni kod bu listeye eklenmeden kullanılmaz (kaynak: `02 §10.3.1` (1)–(8), `§10.4.1`'in dokuz müdahalesi).

   | Alan | Kodlar | Konu |
   |---|---|---|
   | `CompanyAndBranding` | `UpdateCompany` (konu `Company`), `UpdateBranding` (konu `Branding`) | Firma ve marka |
   | `Publication` | `ChangePublishStatus` | Kaydın türü (`06 §5.2.3`) |
   | `Order` — durum işlemleri (12) | `MarkBankTransferPaid`, `MarkShipped`, `MarkDelivered`, `SetDeliveryOrderAgainstWithdrawal`, `MarkDeliveryFailed`, `CompleteService`, `CancelByCompany`, `RefundManually`, `ReceiveReturn`, `ReceiveReturnWithIbanRequest`, `OpenRefundIbanRequest`, `RejectReturn` | Sipariş; kalem ve kayıt `Details`'ta |
   | `Order` — düzeltme ve not (12) | `CorrectAddress`, `ReduceItemQuantity`, `RemoveItem`, `CorrectShippingInfo`, `CorrectDeliveryDate`, `CorrectReturnArrivalDate`, `CorrectTransition`, `SaveInternalNote`, `CloseNotReturned`, `RecordWithdrawalFromOtherChannel`, `RecordLateDeliveryTerminationFromOtherChannel`, `CorrectContactEmail` | Sipariş |
   | `Order` — iade ve e-posta (5) | `RetryCardRefund`, `RefundByBankTransfer`, `MarkCardRefundNotCompleted`, `ReportRefundTransferNotCompleted`, `ResendEmail` | Sipariş |
   | `AdminAccounts` | `SendAdminInvitation`, `WithdrawAdminInvitation` (konu `AdminInvitation`); `CreateAdminAccount`, `RemoveAdminAccount`, `ChangeAdminEmail` (konu `Admin`) | Yönetici hesapları |
   | `LegalTextVersion` | `PublishLegalText` | `LegalTextVersion` |
   | `Settings` | `UpdateSettings` | `CompanySettings`; teslimat illerinde `Province` |
   | `Export` | `ExportOrders`, `ExportCustomers`, `ExportContactRequests` | Konu boştur |
   | `CustomerDeletion` | `DeleteCustomerAccount` | `Customer` |

   Toplam: 2 + 1 + 29 + 5 + 1 + 1 + 3 + 1 = 43; tabloyla `enum` eşitliği kilit dosyası testindedir.
4. **`Details` kişisel veri taşımaz.** `AuditDetails` kurucusu ayarlarda alan adını ve eski/yeni değeri, görünen adı ve işlem değerlerini kabul eder. `CorrectAddress`, `CorrectContactEmail`, `SaveInternalNote` ve `DeleteCustomerAccount`'ta eski/yeni değer çifti ve kişisel değer yazılamaz; `WriteAsync` doğrular, ihlal programlama hatasıdır. Test: bilinen ad, adres, e-posta ve not metniyle yazılan satırın `Details`'i bu değeri içermez.
5. **Satır işlemle aynı işlemde yazılır** (`05 §4.7.2`); işlem geri alınırsa satır da gider.
6. **İmha fonksiyonu** `dispose_expired_audit_logs(max_rows integer, as_of timestamptz)`'tir. `as_of`, imha işinin `TimeProvider`'dan verdiği andır; fonksiyon `now()` okumaz (§7.1). İmza `06 §4.63`'te tek parametreliydi; ikinci parametre bu yazımın geri beslemesidir (K-1335). Süre, oluşturma anının Türkiye saatindeki takvim yılının sonundan on yıldır (Z-30); Türkiye sabit +03:00'dır (§7.1 madde 3), bu yüzden SQL'de saat dilimi adı yoktur. İmha işinin anı nasıl aldığı da burada kapanır: an uygulamadan parametre olarak gelir.
7. **Göçteki kalıp** (`Infrastructure/Migrations`, `migrationBuilder.Sql`, `$$` gövde):

   ```sql
   CREATE FUNCTION public.dispose_expired_audit_logs(max_rows integer, as_of timestamptz)
   RETURNS integer LANGUAGE plpgsql SECURITY DEFINER
   SET search_path = pg_catalog, public
   AS $$
   DECLARE
     cutoff timestamptz;
     removed integer;
   BEGIN
     IF max_rows < 1 THEN RAISE EXCEPTION 'max_rows must be positive'; END IF;
     cutoff := make_timestamp(
         extract(year FROM ((as_of AT TIME ZONE 'UTC') + interval '3 hours'))::integer - 10,
         1, 1, 0, 0, 0) AT TIME ZONE 'UTC' - interval '3 hours';
     WITH expired AS (
       SELECT id FROM public.audit_logs WHERE created_at < cutoff ORDER BY created_at LIMIT max_rows)
     DELETE FROM public.audit_logs AS log USING expired WHERE log.id = expired.id;
     GET DIAGNOSTICS removed = ROW_COUNT;
     RETURN removed;
   END $$;
   REVOKE ALL ON FUNCTION public.dispose_expired_audit_logs(integer, timestamptz) FROM PUBLIC;
   DO $$ BEGIN
     EXECUTE format('GRANT EXECUTE ON FUNCTION public.dispose_expired_audit_logs(integer, timestamptz) TO %I',
       current_setting('shopfolio.application_user'));
   END $$;
   ```

   Kesim anı yerel yılın (Ly) on yıl öncesinin 1 Ocak 00:00'ıdır; `created_at < cutoff` ⇔ satırın yerel yılı ≤ Ly − 11. `GRANT` yalnız uygulama kullanıcısınadır: uygulama kullanıcısının adı `shopfolio.application_user` ayarında gelir; ayarı göç kodu kurmaz, kurucu kullanıcı ilk kurulumda göç kullanıcısının rol ayarına yazar (§10.6 madde 9; K-1340); aynı mekanizma §10.8'de salt okuma ve yedek durum kullanıcılarının adlarına genişler (K-1343). Ayar kurulmamışsa `current_setting` hata verir ve göç geri alınır. Sahip göç kullanıcısıdır (`OWNER` yazılmaz). Değişiklik yeni bir göçte `CREATE OR REPLACE`'tir (sahip ve yetki korunur); imza değişirse eski işlev aynı göçte `DROP` edilir ve yetki yeniden verilir. Göçün `Down` yöntemi yok sayılır (şema geri alınmaz, `05 §8.7.3`); yazılmaz.
8. **Test** (entegrasyon, gerçek PostgreSQL, §19.1): uygulama kullanıcısı `UPDATE`, `DELETE`, `TRUNCATE` ile reddedilir, `INSERT` ve `SELECT` çalışır; fonksiyon uygulama kullanıcısıyla çalışır, `PUBLIC` ve salt okuma kullanıcısı çağıramaz; katalogda `prosecdef` doğru, `proconfig` içinde `search_path` sabit, sahip göç kullanıcısıdır; sınır: `created_at` 2016-12-31T20:59:59Z (yerel 23:59:59) olan satır `as_of` 2026-12-31T20:59:59Z'de silinmez, 2026-12-31T21:00:00Z'de silinir; `max_rows` sınırı ve dönen sayı; `max_rows < 1` hata verir; yeni satıra dokunulmaz; göç dosyalarında `now()` yoktur (§7.1 madde 5 taraması).

### 9.3 Belgenin kişisel verisiz kopyası ve yer tutucular

1. **Kopya iki alandır:** `Order.PreInformationFormText` ve `Order.DistanceSalesContractText`. Ham düz metin saklanır; kaçış ve HTML saklanmaz (K-1323 (3)). Kopya onay işleminde bir kez yazılır, sonra yazılmaz (ayarlayıcı `private`; yazan tek yöntem `OrderDocuments`'ındır).
2. **Belirteç `{{Ad}}`'dır;** ad `06 §4.17`'nin alan adıdır. Kişisel veri belirteçleri: `ShippingRecipientName`, `ShippingPhone`, `ShippingProvince`, `ShippingDistrict`, `ShippingAddressLine`, `BillingRecipientName`, `BillingProvince`, `BillingDistrict`, `BillingAddressLine`, `ContactEmail`. İl alanı kimlikle saklandığından belirteç `Id`'siz yazılır ve değeri `Province.Name`'dir; faturada telefon yoktur. Bunlar kopyada daima belirteç olarak kalır. Donan değerler (`Total`, `ShippingFee`, `CompanyLegalName`, `BankTransferIban` …) üretim anında ham yazılır; belirteç kümesi kapalıdır (`Order` ve `OrderItem` alan adları), küme dışı belirteç şablon testini kırar ve sabit metinde başka amaçla `{{` bulunmaz. Kalem satırlarının ve tutar tablosunun yazılışı şablonun işidir, §11.4'tedir (yazım 4).
3. **Doldurma tek işlevdir:** `Infrastructure/PlaceholderFiller.Fill(string template, IReadOnlyDictionary<string, string?> values, bool requireAll)`. `Regex.Replace` (`\{\{([A-Za-z]+)\}\}`, `MatchEvaluator`) kullanır; değer yeniden taranmaz (değerin içindeki `{{Total}}` genişlemez). Sözlükte olmayan belirteç olduğu gibi kalır (üretim aşaması); `null` değer boş metin olur (imha edilmiş alan); `requireAll` sözlükte olmayan her belirteçte hata verir — sonuç değil şablon taranır, çünkü değer `{{` içerebilir. Üretim ve gösterim aynı işlevi kullanır.
4. **Tek yer `Ordering`'tir:** `internal OrderDocuments` (üretim: onay işleminde sürüm metni ve donan değerler; gösterim) ve açık yüzü `Contracts.IOrderDocuments.GetTextAsync(Guid orderId, LegalTextType type, ct)` (yalnız `PreInformationForm` ve `DistanceSalesContract`; öteki tür `ArgumentException`). Sipariş sayfası, panel, B-1 gövdesi (`Notifications`) ve panelden yeniden gönderim bunu çağırır; başka yerde doldurulmaz.
5. **Onay anındaki değer** ilgili geçmişin ilk satırının eski değeridir. Adres: `OrderAddressHistory`'de (`AddressType` teslimat → `Shipping…`, fatura → `Billing…`) en eski `CreatedAt` satırının `OldRecipientName`, `OldPhone`, `OldProvinceId` (→ `Province.Name`), `OldDistrict`, `OldAddressLine` alanları. `ContactEmail`: `OrderEmailHistory`'nin ilk satırının `OldEmail` alanı. Geçmiş yoksa siparişin kendi alanı kullanılır; imhadan sonra alanlar `null` → boş olur.
6. **Kaçış gösterim kanalındadır** (K-1323): doldurma kaçış yapmaz. `Razor`/`MarkupString` yasağı ve kanalın işlenişi §11.4'tedir; şablon sürüm kimliği (`TemplateVersion`, SHA-256'nın ilk 12 hanesi) de orada yazılır.
7. **Test.** Doldurma birimleri: genişlememe, kalan belirteç, `null`, `requireAll`. Üretilen kopyada bilinen ad, adres, e-posta ve telefon dizeleri yoktur, belirteçler durur, donan değerler ham yazılmıştır. Adres ve e-postası iki kez düzeltilmiş siparişte gösterim ilk eski değeri verir; düzeltmesiz siparişte alandan okunur; imhadan sonra hata vermez. Dört çağıran (sayfa, panel, B-1, yeniden gönderim) aynı metni üretir. Kopyayı yazan başka yer yoktur (mimari test).

### 9.4 Tek işlevler

1. **Kimlik alanı normalleştirmesi:** `Infrastructure/TextNormalizer.NormalizeIdentity(string)` = `value.Trim().Replace('I','i').Replace('İ','i').Replace('ı','i').ToLowerInvariant()`; kültüre bağlı `ToLower` yasaktır. Sütunlar: `NormalizedEmail` (müşteri, yönetici, davet), `NormalizedContactEmail`, `NormalizedNewEmail`, `NormalizedCode` (kupon), `NormalizedSKU`, `NormalizedValue` (deneme kaydının e-posta ekseni) ve giriş kaydının `NormalizedEmail`'i. Sütunu yazan tek yer, sahibi varlığın görünen değeri yazan yöntemdir (ayarlayıcı `private`); arama değeri aynı işlevden geçer. `citext`, `lower()` dizini, `ToLower`, `ToUpper`, `ILike` ve büyük/küçük harf duyarsız karşılaştırma kimlik alanlarında yoktur (`06 §2.8.2`). İşlev değişirse sütun, göçün veri adımında yeniden hesaplanır; göç işlevi çağırmaz, o günkü mantığın donmuş kopyasını taşır (§10.6).
2. **Aranan ad biçimi:** `TextNormalizer.Simplify(string)` — `I`/`İ`/`ı` → `i`, `Ç` → `c`, `Ğ` → `g`, `Ö` → `o`, `Ş` → `s`, `Ü` → `u` (büyük ve küçük); kalan aksanlı Latin harfler `FormD` ile ayrıştırılıp birleşik işaret atılır; sonra `ToLowerInvariant`. Başka karaktere dokunulmaz. `Product.NameSearchForm = Simplify(Name)`; `SetName` ikisini birlikte yazar (`06 §2.7.1`). Arama ifadesi de `Simplify`'dan geçer (kaçış ve içerir eşleşmesi §10.5'tedir).
3. **Sayfa adresi:** `Slug.From(name, rootName)` — `Simplify` → `[^a-z0-9]+` tek tire → baştaki ve sondaki tire atılır; sonuç boşsa `rootName` (ürün, kategori, hizmet, referans, sayfa; `06 §4.72`). Kırpma yoktur (ad tavanı P-27). **Çakışma:** aynı tabloda taban ya da `taban-<sayı>` olarak kullanılan sayılar okunur, 2'den başlayarak ilk boş sayı verilir (`çelik-kapı` → `celik-kapi`, `celik-kapi-2`); `LIKE` meta karakteri yoktur (adres yalnız `[a-z0-9-]`). **Eşzamanlı çakışma:** tekil dizin ihlali (SQLSTATE `23505`; dizin adı §5.2'nin kuralından) işlemin bütünüyle yeniden yürütülmesidir: `ITransactionRunner.RunAsync(work, retryOnUniqueIndexes, ct)` aşırı yüklemesi, ilk deneme dahil en çok 3 deneme (`ProductConstants`'ta değil, kodda sabit; §3.5 tablosunda satırı vardır). `SlugFixedAt` boşken adres adla birlikte yeniden üretilir, doluysa dokunulmaz (`06 §4.72.3`). Önerilen stok kodu (`05 §7.8.1`): adresten büyük harf, ilk 20 karakter, kırpmada sondaki tire atılır, sonra `-NN`.
4. **Kategori düzeyi:** `Catalog/CategoryTree` (`internal`) `CreateAsync` ve `MoveAsync` ile `Category.Level`'ı yazan tek yerdir (ayarlayıcı `private`): (a) işlemin başında ağaç danışma kilidi alınır (`05 §7.3.5`; alınış kalıbı §10.2), (b) ağacın tamamı (`Id`, `ParentCategoryId`, `Level`) tek sorguyla okunur, (c) kök 1, değilse üst + 1 yazılır, (d) `ProductConstants.CategoryLevelMax` (P-20, 3) aşımı ve kendi alt ağacına taşıma dönüş değeriyle reddedilir (`07 §6.2` kodu), (e) taşımada alt ağacın her satırı aynı farkla aynı `SaveChanges`'te yazılır. Hesap saf `CategoryLevels` işlevindedir. Test: oluşturma, dördüncü düzey reddi, taşımada alt ağacın birlikte kayması, aşım reddinde hiçbir düzeyin değişmemesi, kendi alt ağacına taşıma, eşzamanlı iki karşıt taşıma (gerçek veritabanı), her yazımdan sonra `Level = üst + 1` değişmezi, ayarlayıcının çağıranları.
5. **Mimari test:** Türkçe harf sabitleri (`'ı'`, `'İ'` …) ve `tr-TR` kültürü uygulama projesinde yalnız `TextNormalizer.cs`'te bulunur.

*Kaynak: K-1335, K-1336 · K-1333, K-1334 (§7.1 zaman, §8.1 işlem) · K-1323 (3) · K-889 · `05 §4.1`, §4.3, §4.7, §3.4 · `06 §4.63`, §4.72, §2.7, §2.8, §4.9, §4.17, §4.19, §4.59, §4.62, §5.2.6, §5.6.10 · `02 §3.23.3`, §10.3.1, §10.4.1, §3.30.2.*

## 10. Veri erişimi

Bu bölüm veritabanına erişimin **kodda nasıl yazıldığını** söyler; kuralın kendisi Veri Modeli'nde (`06 §2`) ve Teknik Mimari'dedir (`05 §7`). Veritabanı bağlamı tektir ve `Infrastructure/` altındadır (§4.1); işlemi `ITransactionRunner` açar (§9.1, madde 3). Alt bölümler yazım turunun 3. oturumunda üç parçada yazılır: 10.1–10.3 Y-3a'da, 10.4–10.6 Y-3b'de, 10.7–10.10 Y-3c'de yazıldı.

### 10.1 Silme davranışı, anahtar, enum

1. **Silme davranışı her ilişkide açık yazılır** (`06 §2.2.5`; K-1337). Üç davranışın kodda ve veritabanındaki karşılığı sabittir:

   | `06 §2.2.5` | Kodda | Veritabanında |
   |---|---|---|
   | Basamaklı | `DeleteBehavior.Cascade` | `ON DELETE CASCADE` |
   | Boşaltılır | `DeleteBehavior.SetNull` (yabancı anahtar sütunu boş olabilir) | `ON DELETE SET NULL` |
   | Kısıtlayıcı | `DeleteBehavior.NoAction` | `NO ACTION` |

   `Restrict`, `ClientCascade`, `ClientSetNull` ve `ClientNoAction` yazılmaz: `Restrict` veritabanında `RESTRICT` olur (`06` onu yasaklar), `Client…` değerleri silmeyi çatının izlediği kayda bırakır. Çatının zorunlu ilişki varsayılanı (basamaklı silme) hiçbir ilişkiye sessizce uygulanmaz: `Infrastructure/` altındaki bir model sonlandırma kuralı (`IModelFinalizingConvention`) silme davranışı yapılandırılmamış her ilişkiyi `NoAction` yapar; yapılandırılıp yapılandırılmadığının okunuşu (`GetDeleteBehaviorConfigurationSource`) bilgim tarihlidir — okunamazsa kural kalkar, aşağıdaki model testi aynı güvenceyi verir. Yapılandırma sahibi kaydın `IEntityTypeConfiguration` sınıfındadır.

   ```csharp
   // Catalog/VariantConfiguration.cs — OnDelete her ilişkide yazılır; 06 §4.6 ve §2.2.5.
   builder.HasOne<Product>().WithMany().HasForeignKey(v => v.ProductId)
          .OnDelete(DeleteBehavior.Cascade);        // basamaklı: ürünle silinir
   builder.HasOne<OptionValue>().WithMany().HasForeignKey(v => v.FirstOptionValueId)
          .OnDelete(DeleteBehavior.NoAction);       // kısıtlayıcı: NO ACTION, RESTRICT değil
   ```
   **Boşaltmayı veritabanı yapar:** siparişin hesap bağı (`Order.CustomerId`) gibi boşaltılan bağ `SetNull`'dır; hesap silinince veritabanı aynı deyimde bağı boşaltır ve kodda ayrı bir boşaltma yazımı yoktur. Veritabanının boşaltması kaydın sürümünü artırmaz; sürümü artırması gereken bir yazımı (`06 §2.6.4`) silen işlem ayrıca kendisi yapar. **Silme sahibe, tek deyimle yapılır:** sahibiyle silinen bağlı kayıt sahibinden önce ayrı silinmez — `NO ACTION` denetimi deyimin sonundadır ve ancak aynı deyimde basamaklanan bağlı kayıt kısıta takılmaz (`06 §4`'ün girişi; K-1038).
2. **Birincil anahtarı uygulama üretir ve üretim tek yerdedir** (`06 §2.3.1`; K-1337). `Infrastructure/EntityIds.New()` sürüm 7 UUID'yi üretir; kimlik varlığın oluşturma yönteminde (fabrika ya da kurucu) atanır — alan başlatıcısında değil, çünkü EF okuduğu her satırda başlatıcıyı çalıştırır. Kimliğin zaman öneki sıralama yerelliği içindir, iş zamanı değildir (§7.1 kural 1'in yasağı onu kapsamaz) ve kimlikten zaman okunmaz.

   ```csharp
   // Infrastructure/EntityIds.cs — kimliğin tek üretim yeri.
   internal static class EntityIds
   {
       public static Guid New() => Guid.CreateVersion7();   // .NET 9'dan; bilgim tarihli — ilk task'ta doğrulanır
   }
   ```
   Model sonlandırma kuralı her `Guid` birincil anahtarını `ValueGeneratedNever` yapar; böylece kimliği atanmamış varlık sessizce `Guid.Empty` ile kaydedilmez, EF de kendi kimliğini üretmez. Tek istisna `DataProtectionKey`'in tamsayı anahtarıdır (`06 §4.68`; §5.2 madde 4) ve kuralın kapsamı dışındadır. **Yasak:** `Guid.NewGuid()` ve `Guid.CreateVersion7()` `EntityIds.cs`'in dışında çağrılmaz; veritabanında kimlik varsayılanı (`gen_random_uuid()`, `uuidv7()`, `uuid_generate_v4()`) yazılmaz (§7.1 kural 1'in SQL taramasına eklenir). Kriptografik anahtar kimlikle üretilmez (§15.1).
3. **Enum veritabanında kod adıyla metin olarak durur ve sütun kapalı kümeyi kısıtlar** (`06 §2.9.1`, §2.9.2; K-1337). Dönüşümü ve kısıtı model kurulumu bütün enum'lara birden verir; enum özelliğinin yapılandırmasında ikisi de elle yazılmaz.

   ```csharp
   // Infrastructure/ — bağlamın kurulumu: uygulama derlemesindeki her enum özelliği kod adıyla metindir.
   protected override void ConfigureConventions(ModelConfigurationBuilder configuration)
   {
       foreach (var enumType in typeof(EntityIds).Assembly.GetTypes().Where(t => t.IsEnum))
           configuration.Properties(enumType)
                        .HaveConversion(typeof(EnumToStringConverter<>).MakeGenericType(enumType));  // bilgim tarihli
   }
   ```
   Bir model sonlandırma kuralı her enum sütununa izinli değerlerin kontrol kısıtını ekler (`"status" IN ('Placed', …)`); sütun adı kütüphanenin ad kuralından sonra, modelden okunur — kuralın sıralaması ilk task'ta sınanır. Enum'un yalnız bir alt kümesini kullanan alan alt kümesini bildirir (`HasAllowedValues(PublishStatus.Draft, PublishStatus.Published)` — `06 §2.9.2`); bildirimsiz alanda kümenin hepsi izinlidir. **İstisna:** işlem izinin `Operation` sütununda kısıt yoktur — iz göçle dönüştürülemez ve tanınmayan kod olduğu gibi gösterilir (§9.2 madde 2); istisna kuralın içindeki tek `Operation` satırıdır, başka istisna açılmaz. Sayısal dönüşüm (`HasConversion<int>`, `(int)` ile yazım) yoktur. Yeni değerin göçle gelişi §10.6'dadır.
4. **Kontrol kısıtının adı türetilir** (K-1337). Çatı kontrol kısıtına ad ister; elle ad vermemenin (`06 §2.4.3`) karşılığı adın kuraldan çıkmasıdır: ad `ck_` + tablo adı + `_` + kural adıdır. Enum kısıtında kural adı sütun adıdır; elle yazılan kısıt (ör. havale açıksa IBAN dolu — `06 §4.2`) yapılandırmada yalnız kural adını verir: `HasRule("bank_transfer_iban_required", "…")`. Sonlandırma kuralı adı birleştirir ve kısıtı ekler; tam ad §5.2 madde 5'in desen ve 63 bayt testinden geçer.
5. **Test.** Model testi (entegrasyon): her yabancı anahtarın silme davranışı `Cascade`, `SetNull` ya da `NoAction`'dır; `Cascade` ilişkilerin kümesi `06 §2.2.5`'in basamaklı satırıyla eşittir (kayıtlı liste testtedir, listeyi `06 §2.2.5` ile elle hizalamak ekleme task'ının işidir); `SetNull` ilişkinin sütunu boş olabilir. Gerçek veritabanında `pg_constraint` yabancı anahtarın silme eylemini `a` (no action), `c` ya da `n` gösterir, `r` (restrict) göstermez. Birincil anahtar testi: her `Guid` anahtarı `ValueGeneratedNever`'dır; `EntityIds.New()` sürüm 7 üretir (sürüm basamağı 7) ve ardışık iki çağrı farklıdır. Enum testi (gerçek veritabanı): her enum üyesi yazılır, geri okunur ve ham değer üye adıdır; kümede olmayan metin kısıta takılır (ham SQL ile eklenir); alt küme bildiren alan dışarıdaki üyeyi reddeder. Kaynak taraması: yasak listesi (madde 2) ve `HasConversion<int>` yoktur.

*Kaynak: K-1337 · K-1010, K-1038 (silme davranışı) · K-1011, K-1012, K-1017 (anahtar, adlandırma, enum) · K-1309 · `06 §2.2.5`, §2.3.1, §2.4.3, §2.9, §4 (girişi), §4.2, §4.6, §4.68 · `05 §7.5.1`, §8.7.1 · §5.2, §7.1, §9.2.*

### 10.2 Sürüm, eşzamanlılık, sayaç, danışma kilidi

Kural `06 §2.6` ve `05 §7.3`'tedir: iyimser sürüm, siparişte kısa satır kilidi, sayaçta koşullu güncelleme. Bu alt bölüm üçünün kodunu ve dördüncüsünü, danışma kilidini yazar (K-1337).

1. **Sürüm sütunu sıradan bir tamsayı özelliğidir** (`06 §2.6.1`): `public int Version { get; private set; } = 1;`. Ayarlayıcı `private`tır ve artışı varlığın `BumpVersion()` yöntemi yapar. `xmin` ve `IsRowVersion` kullanılmaz. **Tek sürüm sütunlu kayıtta** (ürün, kupon, kurumsal içerik, marka, firma kimliği, yasal metin — `06 §2.6.2`) özellik `IsConcurrencyToken()` ile işaretlenir: EF güncellemeye yükleme anındaki değeri koşul koyar. Birden çok sürüm sütunlu tablonun kalıbı farklıdır, §10.3'tedir.
2. **Kayıt varyantının kaydı:** kayıt izlenerek yüklenir, formun sürümü yüklenen değerle karşılaştırılır, değerler uygulanır, `BumpVersion()` çağrılır ve `SaveChangesAsync` tek koşullu güncellemeyi yazar. Karşılaştırma erken ret, belirteç ise iki kaydın aynı anda gelmesine karşı güvencedir. Ürünün sürümü varyantlarını, seçeneklerini ve bağlarını kapsar (`06 §2.6.2`): ürün formunun kaydı yalnız alt satır değişse de `BumpVersion()`'ı koşulsuz çağırır ve alt satırlar aynı `SaveChanges`'tedir.

   ```csharp
   // Catalog içinde, ITransactionRunner.RunAsync'in gövdesinde. Red dönüş değeridir; kod 07 §6.2'nin RecordVersionConflict'idir.
   var product = await _db.Products /* alt satırlarıyla yüklenir */ .SingleAsync(p => p.Id == input.Id, ct);   // izlenir
   if (product.Version != input.Version)
       return Result.Rejected(ErrorCode.RecordVersionConflict);
   // … formun değerleri uygulanır …
   product.BumpVersion();                                   // kaydı yazan her güncellemede, alt satır değişse de
   try { await _db.SaveChangesAsync(ct); }
   catch (DbUpdateConcurrencyException) { return Result.Rejected(ErrorCode.RecordVersionConflict); }   // eşzamanlı yazım
   ```
   `DbUpdateConcurrencyException` yalnız kayıt varyantının kaydında bu biçimde yakalanır ve ret olur; öteki her yerde beklenmeyen hatadır (§8.1). "Yine de kaydet" formu güncel sürümle yeniden gönderir (`05 §7.3.1`) ve aynı yoldan geçer; ayrı kod yoktur.
3. **Sipariş varyantı kilit altında karşılaştırır** (`05 §7.3.2`; sıra §9.1 madde 4): önce `OrderLocks.LockAsync`, sonra sipariş yüklenir — kilitten önce yüklenen sipariş eskimiş olabilir —, sonra yöneticinin işleminde formun sürümü karşılaştırılır (tutmuyorsa `OrderVersionConflict`; müşterinin işlemi sürüm taşımaz ve o anki hâle karşı denetlenir — `07 §2.4.2`). Sürümü artıran her yazım `BumpVersion()` çağırır; artırmayan sistem yazımları `06 §2.6.4`'ün sayımıdır ve sürüme dokunmaz. `Order.Version` da `IsConcurrencyToken()` taşır, ama burada belirteç yalnız ikinci güvencedir: kilit altında `DbUpdateConcurrencyException` atılması kilidin atlandığını gösterir ve **beklenmeyen hatadır** — ret olarak yakalanıp sessizce geçilmez.
4. **Sürüm taşımayan yazım koşullu güncellemedir** (`06 §2.6.5`): koşul `Where`'de, yazım `ExecuteUpdateAsync`'tedir ve etkilenen satır sayısı 0 ise yazım olmamıştır. **Sayaç hiçbir zaman okunup hesaplanıp yazılmaz;** artış veritabanında yapılır (`ReservedQuantity + adet`) ve koşul aynı deyimdedir. Sayaç özelliklerinin (`ReservedQuantity`, `UsedCount`, `ReservedCount`, indirme sayacı) ayarlayıcısı `private`tır; yazan yer yalnız `Catalog`'un geçiş biriminden çağrılan ayırma yöntemleridir (§9.1 madde 6; K-1013).

   ```csharp
   // Catalog içinde: ayırma yalnız kalan adet yetiyorsa yazılır (05 §7.3.3). Sürüm artmaz.
   var written = await _db.Variants
       .Where(v => v.Id == variantId
                && (v.StockQuantity == null || v.StockQuantity - v.ReservedQuantity >= quantity))
       .ExecuteUpdateAsync(s => s.SetProperty(v => v.ReservedQuantity, v => v.ReservedQuantity + quantity), ct);
   if (written == 0) { /* yetmedi: dönüş değeriyle ret */ }

   // Panelin stok alanı: gösterilen değere karşı (K-980). 0 satır ise neden aynı işlemde okunur —
   // değer değişmişse DisplayedValueConflict, ayrılmış adedin altındaysa 02 §3.6.7'nin reddi.
   var saved = await _db.Variants
       .Where(v => v.Id == variantId && v.StockQuantity == originalStock
                && (newStock == null || newStock >= v.ReservedQuantity))
       .ExecuteUpdateAsync(s => s.SetProperty(v => v.StockQuantity, newStock), ct);
   ```
   Öteki sürüm taşımayan yazımlar (bağlantı kaydının tüketilmesi, yeniden doğrulama izni, kurumsal içeriğin elle sırası, durum değiştiren işlemler) aynı kalıptır: koşul o anki hâle ya da gösterilen değere, etkilenen satır 0 ise ret. İç notun yazımı §10.9'dadır.
5. **Danışma kilidi tek yerdedir:** `Infrastructure/AdvisoryLocks.LockAsync(AdvisoryLockScope scope, Guid? key, ct)` (`internal`; bağlama `OrderLocks`'la aynı yoldan ulaşır). Kilit işlem kapsamlıdır (`pg_advisory_xact_lock`) — işlem bitince düşer, ayrıca açma yoktur — ve işlem açık değilse yöntem istisna atar. Anahtar kapsam adından ve varsa kimlikten kurulan metnin 64 bitlik özetidir (`hashtextextended`); iki ayrı anahtar çakışırsa yalnız fazladan sıralanır, yanlış sonuç doğmaz. Kapsamlar üçtür: `Cart` (sepet kimliğiyle; sipariş onayı — `05 §4.2.2`), `CategoryTree` (anahtarsız; oluşturma ve taşıma — §9.4 madde 4) ve `AdminSet` (anahtarsız; yöneticinin kaldırılması — yönetici kümesinin kilidi, `05 §6.1.2`). Kilit **işlemin ilk deyimidir** — kilitten önce okunan değer eskimiş olabilir — ve bekleme `lock_timeout` ile sınırlıdır (§3.5 tablosu; aşım `05 §10.3`'ün hâllerine düşer).

   ```csharp
   // Infrastructure/AdvisoryLocks.cs
   public static async Task LockAsync(AdvisoryLockScope scope, Guid? key, CancellationToken ct)
   {
       // CurrentTransaction boşsa InvalidOperationException (kilit işlemin dışında anlamsızdır).
       var name = key is null ? scope.ToString() : $"{scope}:{key}";
       await _db.Database.ExecuteSqlAsync($"SELECT pg_advisory_xact_lock(hashtextextended({name}, 0))", ct);  // bilgim tarihli
   }
   ```
   **Kilit sırası** `05 §7.3.5`'tedir ve bu sınıf onu zorlamaz, test sınar: sepet kilidi sipariş tarafının en başındadır; `CategoryTree` ve `AdminSet` sipariş, varyant ya da kupon kilidi alan işlemle birleşmez.
6. **Test.** Birim: `BumpVersion` sürümü bir artırır. Entegrasyon (gerçek veritabanı, §19.1): aynı kaydı iki işlem aynı sürümle kaydeder — biri geçer, öteki `RecordVersionConflict` alır ve sürüm yalnız bir artmıştır; ürün formunun yalnız varyantı değişen kaydı ürünün sürümünü artırır; stok alanı gösterilen değerle ve ayrılmış adedin altı için iki ayrı ret verir; ayırma yetmezse 0 satır ve sayaç değişmez; eşzamanlı iki ayırma toplamı aşmaz; aynı sepetin iki onayı sıraya girer; `CategoryTree` kilidi iki karşıt taşımayı sıralar; işlem dışında `AdvisoryLocks` istisna atar. Model ve mimari testi: sürüm ve sayaç ayarlayıcıları `private`; tek sürüm sütunlu varlığın belirteci vardır, birden çok sürüm sütunlu `CompanySettings`'inkiler belirteç değildir (§10.3); `pg_advisory` ve `FOR UPDATE` metinleri yalnız `AdvisoryLocks.cs`, `OrderLocks.cs` ve §10.3'ün ayar kilidindedir (kaynak taraması).

*Kaynak: K-1337 · K-1014, K-1013, K-1040 (4), K-1047, K-1089, K-980, K-967 · K-1119 · `06 §2.6`, §4.9, §4.17, §4.18 · `05 §4.2.2`, §6.1.2, §7.3, §10.3 · `07 §2.4`, §6.2 · `02 §3.6.7` · §3.5, §8.1, §9.1, §9.4.*

### 10.3 Bölüm sürümünün çatıda eşlenişi

`CompanySettings` tek satırdır ve yedi bağımsız kaydedilen bölümün her biri kendi sürüm sütununu taşır (`06 §2.6.3`, §4.2): `PaymentMethodsVersion`, `ShippingFeeVersion`, `DeliveryProvincesVersion`, `PeriodsVersion`, `ProductDefaultsVersion`, `ReturnAddressVersion`, `HomepageVersion`. Kalıp K-1320'nin kararıdır; bu alt bölüm onu kodlar (K-1338).

1. **Varlık düz bir varlıktır:** yedi sürüm özelliği sıradan tamsayıdır ve `IsConcurrencyToken()` taşımaz — EF güncellemenin koşuluna işaretli her belirteci koyardı ve bir bölümün kaydı öteki altı bölümün sürümüne takılırdı (§10.2 madde 1'in tek sürümlü kalıbından farkı budur). Satır okunurken izlenmez (`AsNoTracking`) ve `SaveChanges` ile güncellenmez.
2. **Tek sınıf yazar:** `Settings/CompanySettingsWriter` (`internal`) bütün bölüm kayıtlarını yazar; her bölüm bir yöntemdir, yeni bölüm `06`'da sütun, bu sınıfta bir yöntem ve testte bir satırdır. Bölüm kaydının sırası sabittir: (a) ayar satırının kilidi, (b) bölümün koşullu güncellemesi, (c) bölümün alt kayıtları. Kilit (a) `CompanySettingsWriter`'in kendi yöntemidir ve `OrderLocks`'un ham SQL kalıbıyla (`SELECT … FOR UPDATE`; §9.1 madde 5) tek satırı kilitler: üretilen yasal metni yazan işlemle aynı satırda sıralanır (`05 §7.3.5`; K-1088); o işlevin evi ve üretimin kalıbı §11.4 madde 6'dadır (K-1346). Koşul yalnız o bölümün sürümüdür; güncelleme yalnız o bölümün sütunlarını ve sürümünü yazar. Alt kayıtlar — teslimat illerinin satırları (`CompanyDeliveryProvince`), sosyal bağlantılar (`SocialLink`) — güncellemeden sonra, aynı işlemde yazılır; güncelleme 0 satır etkilediyse ret dönüşüyle çıkılır ve yazılmış bir alt kayıt kalmaz.

   ```csharp
   // Settings/CompanySettingsWriter.cs — iade adresi bölümü örnektir. Red dönüş değeridir.
   await LockAsync(ct);                                                      // (a) satır kilidi
   var written = await _db.CompanySettings
       .Where(s => s.ReturnAddressVersion == input.ReturnAddressVersion)     // (b) koşul: yalnız bu bölümün sürümü
       .ExecuteUpdateAsync(s => s
           .SetProperty(x => x.ReturnRecipientName, input.ReturnRecipientName)
           .SetProperty(x => x.ReturnProvinceId, input.ReturnProvinceId)
           .SetProperty(x => x.ReturnDistrict, input.ReturnDistrict)
           .SetProperty(x => x.ReturnAddressLine, input.ReturnAddressLine)
           .SetProperty(x => x.ReturnPhone, input.ReturnPhone)
           .SetProperty(x => x.ReturnAddressVersion, x => x.ReturnAddressVersion + 1), ct);
   if (written == 0)
       return Result.Rejected(ErrorCode.RecordVersionConflict);              // alt kayıt yazılmadı
   // (c) bölümün alt kayıtları bu noktadan sonra, aynı işlemde
   ```
   Girdi modelindeki sürüm alanı sütunun adını taşır (`07 §2.4.1`); formun sürümü satırdan `AsNoTracking` okumayla gelir.
3. **Farklı bölümler çakışmaz, aynı bölümün eski sürümü reddedilir:** koşul kendi bölümünün sütunundadır; kilit yalnız yazımları sıralar. **Geçici kapatma anahtarı sürüm taşımaz** (`06 §2.6.3`): aynı sınıfta, o anki değere karşı koşullu güncellemedir (`Where(s => s.IsSalesTemporarilyClosed == original)`) ve satır kilidi almaz.
4. **Tek yer ve koruma:** `CompanySettings` satırını yazan başka yer yoktur. Mimari test (kaynak taraması): hedefi bu varlık olan `ExecuteUpdate`/`ExecuteDelete` çağrısı yalnız `CompanySettingsWriter.cs`'tedir; ayar satırını `SaveChanges` ile yazan kod yoktur — model testi yedi sürüm özelliğinin belirteç olmadığını da sınar.
5. **Test (entegrasyon, gerçek veritabanı):** B bölümünün eski sürümü varken A bölümü kaydedilir ve geçer; A'nın eski sürümü `RecordVersionConflict` alır; iki bölüm aynı anda kaydedilir ve ikisi de geçer; bölüm kaydı yalnız kendi sütunlarını değiştirir (öteki sütunlar ve sürümler aynı kalır); alt kaydı olan bölümde ret çıkınca alt kayıt değişmez.

*Kaynak: K-1338 · K-1320 · K-1021, K-1088, K-1014 · `06 §2.6.3`, §4.2, §4.55, §4.56 · `05 §7.3.5` · `07 §2.4.1` · §3.5, §9.1, §10.2.*

### 10.4 Önbellek ve kuşak sayacı

Önbellek uygulamanın belleğindedir; ayrı bir önbellek sunucusu yoktur ve tek örnek çalıştığı için geçersiz kılma tek süreçtedir (`05 §7.10.1`; MK-34). Bu alt bölüm geçersiz kılmanın ve kuşak sayacının kodunu yazar (K-1339). Kütüphane eklenmez: kilit ve sayaç yerleşik araçlardır (§3.3).

1. **Önbellekte beş alan vardır** ve `Infrastructure/CacheArea` enum'u onları sayar (`05 §7.10.1`): `Menu`, `CategoryTree`, `Settings`, `HomepageBlocks`, `Sitemap`. Her alanın tek bir değeri vardır ve değerin türü sabittir; parametreli anahtar yoktur. Yeni alan önce `05 §7.10`'a, sonra bu enum'a girer. **Fiyat, stok, sepet ve satış kapısının hâli önbelleğe girmez** — her istekte veritabanından okunur ve hesaplanır (`05 §4.1.5`; K-980). Önbelleğe giren değer **değişmez bir okuma kaydıdır:** sorgu `AsNoTracking` ile okunur ve `record`'a ya da salt okunur listeye yansıtılır; EF varlığı ve `DbContext`'e bağlı hiçbir nesne önbelleğe girmez, önbellekteki değer yazılmaz.
2. **Önbelleği `Infrastructure/AppCache` tutar** (`internal`, tekil). Her alanın bir yuvası vardır: değer, kuşak sayacı ve yuvanın kilidi. **Okuyan, yükten önce kuşağı okur; geçersiz kılan kuşağı artırır ve değeri atar; okuyan değeri ancak kuşak değişmemişse yazar.** Kuşağın okunması, karşılaştırılması ve değerin yazılması yuvanın kilidi altındadır — karşılaştırma ile yazma arasına geçersiz kılma girerse eski değer kalıcı olurdu. Kilidin içinde `await` yoktur.

   ```csharp
   // Infrastructure/AppCache.cs — kalıp. Her alanın değer türü sabittir; tutmayan tür programlama hatasıdır (InvalidCastException).
   public async Task<T> GetOrLoadAsync<T>(CacheArea area, Func<CancellationToken, Task<T>> load, CancellationToken ct)
       where T : class
   {
       var slot = _slots[(int)area];
       long generation;
       lock (slot)
       {
           if (slot.Value is not null) return (T)slot.Value;
           generation = slot.Generation;                 // kuşak, veritabanı okumasından ÖNCE alınır
       }
       var value = await load(ct);                       // eski bir okuma burada sürüyor olabilir
       lock (slot)
       {
           if (slot.Generation == generation) slot.Value = value;   // araya geçersiz kılma girdiyse değer yazılmaz
       }
       return value;                                     // çağıran okuduğu değeri yine de kullanır
   }

   public void Invalidate(CacheArea area)
   {
       var slot = _slots[(int)area];
       lock (slot) { slot.Generation++; slot.Value = null; }
   }
   ```
   Değer yoksa iki istek birden yükleyebilir; ikisi de geçerli bir değer okur ve biri ötekinin üstüne yazar — tek uçuş (single-flight) kilidi yoktur: küçük sunucuda (`05 §0.5`) çift yük, yük kilidinin bekletmesinden ucuzdur. Önbellekte **süre sonu yoktur:** değer ancak geçersiz kılınınca ya da uygulama yeniden başlayınca düşer; ısıtma yoktur, ilk istek yükler.
3. **Geçersiz kılma işlem kesinleştikten sonra yapılır** (`05 §7.10.1`). Alanı besleyen kaydı yazan işlem, gövdesinde `CacheInvalidations.Add(CacheArea)` çağırır (`Infrastructure/CacheInvalidations`, kapsamlı); bu yalnız alanı bir kümeye ekler. `ITransactionRunner` (§9.1 madde 3) işlem kesinleştikten **sonra** kümedeki her alan için `AppCache.Invalidate` çağırır; geri almada ve istisnada kümeyi boşaltır. İki ayrı yarış iki ayrı önlemle kapanır: **kuşak sayacı** geçersiz kılmadan önce başlamış bir okumanın eski değerini yuvaya yazmasını önler; **kesinleşmeden sonra atmak** ise geçersiz kılma ile kesinleşme arasında başlayan bir okumanın hâlâ eski satırı görüp yeni kuşakla yazmasını önler — kesinleşmeden önce atılan alan, kesinleşene kadar eski satırı okuyan istekle yeniden dolardı.

   ```csharp
   // Yanlış: kesinleşmeden önce atılır; araya giren okuma eski satırı okur ve yeni kuşakla yuvaya yazar.
   _cache.Invalidate(CacheArea.Settings);
   await _db.SaveChangesAsync(ct);

   // Doğru: alan kümeye eklenir; ITransactionRunner kesinleştirdikten sonra atar.
   _invalidations.Add(CacheArea.Settings);
   ```
   Ham SQL ya da `ExecuteUpdate` ile yazan işlem de aynı çağrıyı yapar — `SaveChanges`'e bağlı bir kanca yoktur (§10.2 ve §10.3'ün koşullu güncellemeleri `SaveChanges` kullanmaz). **Hangi yazımın hangi alanı attığı task'ın tasarımında çıkar** ve testte bir satırdır; bir yazım birden çok alanı etkiliyorsa hepsini ekler.
4. **Zamana bağlı görünürlük önbellekte saklanmaz** (`05 §7.10.1`; K-980). Önbellek duyuruyu aralığıyla birlikte taşır; "şimdi görünür mü" sorusu her istekte `TimeProvider`'dan (§7.1) hesaplanır. Geçersiz kılma aralığın bitişini beklemez — bu yüzden bitiş için bir süre sonu da gerekmez.

   ```csharp
   var now = _time.GetUtcNow();
   var isVisible = announcement.PublishStatus == PublishStatus.Published
       && (announcement.StartDate is null || now >= TurkeyTime.StartOf(announcement.StartDate.Value))
       && (announcement.EndDate is null || now < TurkeyTime.StartOf(announcement.EndDate.Value.AddDays(1)));
   ```
5. **Sipariş onayının işlemi önbellekten okumaz** (K-980): satış kapısı (`CompanySettings.IsSalesTemporarilyClosed`) ve tutarı etkileyen ayarlar o işlemde veritabanından okunur — önbellekteki ayar bir önceki kaydın görüntüsü olabilir. Önbellekli okuma yalnız sayfaların gösterim yoludur; aynı alanı hem gösterim hem hesap için okuyan modül iki ayrı okuma yolu sunar ve onay işlemi veritabanı yolunu çağırır.
6. **Test.** Birim (veritabanı gerektirmez, §19.1): okuma yük sürerken `Invalidate` çağrılır — yük döner, değer çağırana verilir, ama yuvaya yazılmaz ve sonraki çağrı yeniden yükler; geçersiz kılma değeri atar ve kuşağı artırır; yük hata verirse yuva boş kalır ve kuşak değişmez; ikinci çağrı yükü çalıştırmaz; iki alan birbirini etkilemez. Entegrasyon: alanı besleyen kayıt kaydedilir, ardından okunan değer yenidir; işlem geri alınırsa önbellek atılmaz; kesinleşmeden önce başlayıp işlem açıkken durdurulan bir okuma eski değeri yuvaya yazamaz (kesinleşmeden sonra serbest bırakılınca sonraki çağrı yeniden yükler); zamana bağlı duyuru, sahte `TimeProvider` ilerletilince geçersiz kılma olmadan görünür ve kaybolur; sipariş onayı, ayar satırı önbelleği atlayan doğrudan SQL ile değiştirildiğinde yeni değeri okur. Mimari test: `AppCache`, `CacheArea` ve `CacheInvalidations` yalnız `Infrastructure/` altında tanımlıdır; `AppCache.Invalidate` çağrısı `ITransactionRunner`'ın dışında yoktur (kaynak taraması).

*Kaynak: K-1339 · K-887, K-980, K-871, K-933 · `05 §7.10`, §4.1.5 · `06 §4.2`, §4.52 · §3.3, §7.1, §9.1.*

### 10.5 Liste sırası, sayfalama, arama sözcüğü

Listenin düzeni sunucunun sabitidir ve sıralama parametresi yoktur (`07 §2.6.5`; `02 §3.5.3`); bu alt bölüm düzenin, sayfalamanın ve arama sözcüğünün sorguda yazılışını kurar (K-1339). Sorgu modelinin adları ve `page` ile `search` parametreleri `07 §2.6`'dadır.

1. **Her sayfalı liste ORDER BY'ı kaydın iç anahtarıyla (`Id`) bitirir** (K-1228). İkinci anahtarın yönü birincisini izler: "yeni önce" sıralarda azalan, öteki sıralarda artan; galerinin ve elle sıranın (`SortOrder`) eşit değerinde de aynıdır (`07 §2.6.5`). `Id` sürüm 7 UUID'dir (§10.1) ve zaman önekli olsa da iş zamanı değildir; yalnız eşit anahtarı kesinleştirir.

   ```csharp
   // Vitrinin ürün listesi: (PublishStatus, CreatedAt, Id) dizini (06 §7.5.1) bu sırayı tarar.
   _db.Products.AsNoTracking()
       .Where(p => p.PublishStatus == PublishStatus.Published)
       .OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.Id);

   // Elle sıralı liste ve galeri: firmanın sırası, eşit değerde Id artan.
   _db.Branches.AsNoTracking().OrderBy(b => b.SortOrder).ThenBy(b => b.Id);
   ```
   Süreli bekleyen iş süzgecinde panelin sipariş listesi kalan süresi en az olanı öne alır (`04` 9.8.7): sıra son tarih artan ve `Id` artandır. Sipariş listesinin aramasının numara ve e-posta eşleşmesi tam eşleşmedir (`07 §2.6.4`) ve aşağıdaki desen işlevini kullanmaz.
2. **Sayfalama tek yardımcıdadır:** `Infrastructure/Paging.ToPageAsync(this IOrderedQueryable<T>, int page, int pageSize, ct)` bir `PagedList<T>` döner (`Items`, `PageNumber`, `PageSize`, `TotalCount`, `PageCount`). Parametre `IOrderedQueryable` olduğu için sırasız sorgu **derlenmez**; sıranın `Id` ile bittiği ise testlerin işidir (madde 6). Yardımcı önce sayar, `PageCount = max(1, ceil(TotalCount / pageSize))` hesaplar, istenen sayfayı `PageCount`'a kırpar ve `Skip((page - 1) * pageSize).Take(pageSize)` ile okur — son sayfayı aşan değer son sayfayı gösterir (`07 §2.6.2`). Sayma ile okuma ayrı sorgudur; ikisinin arasındaki yazım gösterimi bir kayıt kaydırabilir ve bu kabuldür (liste bir anlık görüntü değildir).
3. **`page` metin olarak bağlanır ve `PageNumber.Parse` ile okunur** (`Infrastructure/PageNumber`): boş, sayı olmayan ya da birden küçük değer 1'dir; `int` aralığını aşan pozitif değer `int.MaxValue`'dur (yardımcı onu son sayfaya kırpar). Sorgu modelinde alan `public string? Page { get; init; }`'tir — `int` olarak bağlanan alan, `07 §2.6.2`'nin "aralığı aşan değer son sayfadır" kuralını sağlayamaz. Geçersiz değer hata değildir (`07 §2.6.6`).
4. **Sayfa boyu parametre değildir ve tek yerdedir:** `Infrastructure/PageSizes` sabitleri `04` 2.13.3'ün sayılarıdır — vitrinin ürün listeleri 24, içerik liste sayfası 24, üyenin sipariş geçmişi 25, panelin listeleri 25. Değerin evi `04`'tür; sınıf onu `const` olarak taşır ve sayı başka yerde yazılmaz. Kurumsal içeriğin elle sıralı grupları sayfalanmaz (`04` 9.13.3).
5. **Arama sözcüğü tek işlevden desene çevrilir:** `Infrastructure/SearchTerm.ToLikePattern(string? raw)` (`string?` döner; boş sonuç liste üretmeyen sözcüktür — `07 §2.6.3`) şu sırayla çalışır: kırp (§3.4 madde 5) → `TextNormalizer.Simplify` (§9.4 madde 2; aranan ad, `NameSearchForm`'la aynı biçime gelir) → `ProductNameMaxLength` (P-27, 150) karakterine kes (kesim bir vekil (surrogate) çiftinin ortasına denk gelirse çiftin yarısı da atılır) → `\`, `%` ve `_` karakterlerinin önüne `\` koy → başa ve sona `%` ekle. Kesim kaçıştan **önce** yapılır; kaçıştan sonraki kesim `\%` çiftini bölebilirdi. Sorgu parametrelidir ve `LIKE` deseni parametre olarak gider; sözcük sorgu metnine yazılmaz.

   ```csharp
   // Catalog içinde. İşlev, NameSearchForm'daki trigram dizinini (gin_trgm_ops) kullanabilen LIKE'a çevrilir;
   // string.Contains strpos'a çevrildiği için dizini kullanmaz. Like'ın üç parametreli biçimi bilgim tarihli — ilk task'ta doğrulanır.
   var pattern = SearchTerm.ToLikePattern(query.Search);
   if (pattern is null) { /* sonuç listesi üretilmez; arama alanı gösterilir */ }
   var products = _db.Products.AsNoTracking()
       .Where(p => p.PublishStatus == PublishStatus.Published
                && EF.Functions.Like(p.NameSearchForm, pattern, "\\"));
   ```
   Panelin ürün listesinin `search`'ü de aynı işlevi kullanır; ürün adında arama yapan başka yer yoktur. Üç karakterden kısa sözcükte trigram dizini kullanılmaz ve sorgu tabloyu tarar — sonuç doğrudur, hacim tabanında kabuldür. Dizinin kendisi §10.6 madde 8'dedir.
6. **Test.** Birim: `PageNumber.Parse` tablosu (`null`, boş, `abc`, `0`, `-3`, `2`, `99999999999`); `SearchTerm`: `%`, `_` ve `\` desende kaçışlıdır, 150'den uzun sözcük 150'ye kesilir ve kesim kaçışı bölmez, yalnız boşluk `null` döner, `Isıtıcı` aranan biçime `isitici` olarak gelir. Entegrasyon (gerçek veritabanı, §19.1): aynı `CreatedAt`'li iki ürün sayfa sınırına düşürülür — ikisi de her sayfa boyu için ne yinelenir ne kaybolur (vitrin, panel, galeri ve elle sıralı liste için ayrı ayrı; azalan ve artan yön); son sayfayı ve `int` aralığını aşan `page` son sayfayı verir; `%` yazan sözcük bütün ürünleri getirmez, `_` bir harfle eşleşmez, `\` harfi harfine aranır; `AMPUL` ve `isıtıcı` ürününü bulur; tek sayfaya sığan listenin `PageCount`'u 1'dir. Her sayfalı liste için sınır testi zorunludur: yeni liste, testi olmadan eklenmez (§25, yazım 6).

*Kaynak: K-1339 · K-1228, K-1217, K-766, K-851, K-784 · `07 §2.6` · `06 §4.5`, §4.47, §7.5.1 · `04` 2.13.3, 9.8.7 · `02 §3.5`, §11.2 P-27 · §3.4, §3.5, §9.4, §10.1, §10.6.*

### 10.6 Göçlerin yazılışı

Kural `05 §8.7`'dedir: göçler genişlet–daralt kuralıyla yazılır, her göç kendi işleminde çalışır, geri alma şemayı geri almaz önceki imaja döner. Bu alt bölüm kuralın kodda nasıl yazıldığını ve incelemede nasıl denetlendiğini söyler (K-1340). Göç demeti, EF Core göçlerinden üretilen tek çalıştırılabilir dosyadır ve uygulama imajıyla gelir (`05 §8.2.1`; MK-03).

1. **Göçün yeri ve biçimi.** Göçler `Infrastructure/Migrations/` altındadır (§4.1) ve `dotnet ef migrations add <Ad>` ile üretilir; model anlık görüntüsü dosyası depodadır. **Ad**, tek değişikliği anlatan İngilizce Pascal ifadedir (`AddProductNameSearchIndex`). **Bir göç bir konudur;** birbirinden bağımsız iki değişiklik iki göçtür. **`Down` yöntemi silinir** — şema geri alınmaz (`05 §8.7.3`; §9.2 madde 7) ve basılmış `Down` arızada ilk kez çalışacak sınanmamış koddur. Elle yazılan SQL `migrationBuilder.Sql` ile yazılır, nesne adları `public.` ile nitelenir ve `now()` içermez (§7.1 madde 5). EF'in ürettiği göç dosyası elle düzeltilebilir; düzeltme PR'da gerekçelenir.
2. **Her göç kendi işleminde çalışır ve işlem dışı adım içermez** (`05 §8.7.2`). `suppressTransaction: true`, `CREATE INDEX CONCURRENTLY`, `VACUUM`, `ALTER SYSTEM` ve `CREATE DATABASE` göçte yazılmaz; hacim tabanında (H-1) olağan dizin kurma işlemin içinde kısa bir kilitle biter. Bir sürüm birden çok göç taşıyorsa yarıda kalan sürümde öncekiler kalır ve eski sürüm ara şemayla çalışır: **bu yüzden her göç tek başına önceki sürümün koduyla uyumludur,** yalnız sürümün sonundaki şema değil.
3. **Genişlet–daralt kuralı göçlerin üç sınıfıdır** (`05 §8.7.1`; K-965). Önceki sürüm, geri almanın hedefidir; kural onun koduna göre okunur:

   | Göç ne yapar | Sınıf | Ne zaman |
   |---|---|---|
   | Tablo, işlev, dizin eklemek; boş olabilen ya da varsayılanlı sütun eklemek | Genişletme | Her sürümde |
   | Bir sütunu zorunlu yapmak, kısıtı sıkılaştırmak, sütun, tablo ya da kapalı değer kaldırmak, ad değiştirmek | Daraltma | Yalnız önceki sürümün kodu o yapıyı artık kullanmıyorsa |
   | Yeni durum, tür, iş türü ya da yük biçimi eklemek | Genişletme, iki adımda | Önce onu **okuyabilen** sürüm, sonra onu **yazan** sürüm |

   **Değerin okunması ve yazılması iki sürümdür.** Okuyan sürüm enum üyesini, onu işleyen kodu ve enum sütununun izinli değer kısıtındaki yeni değeri taşır (kısıt sonlandırma kuralından türer — §10.1 madde 3; göç kısıtı bırakıp yeniden kurar) ama değeri yazmaz; yazan sürüm bir sonrakidir. Geri alınan imaj yeni sürümün yazdığı değeri tanır. Yük biçimi (`jsonb`) aynıdır: okuyan sürüm iki biçimi de okur, yazan sürüm yeni biçimi yazar. **Ad değiştirme iki sürüme bölünür:** birinci sürüm yeni sütunu ekler, kod ikisini de yazar ve yenisini okur (eskiye düşerek), veri kopyalanır; ikinci sürüm eskiyi kaldırır. Sürüm atlanamaz (`05 §8.7.1`); kural ardışık iki sürüm arasında tutar.
4. **Veri yazan göç yalnız hacmi iş hacminden bağımsız tablolarda yazar** — ayar satırı (`company_settings`) ve sabit listeler (`provinces`, `countries`) gibi. Siparişle, ürünle, hesapla ya da kayıtla büyüyen bir tabloya `UPDATE` ya da `DELETE` yazan göç yoktur: **büyük veri dönüşümü göçte değil, kuyruktaki bir arka plan işiyle parça parça yapılır** (`05 §8.7.5`; ilerleme kaydı §13.3). Yeni sürümün göçü yalnız yapıyı ekler (yeni sütun, dönüşüm işinin işaret sütunu); dönüşüm işini yeni sürümün kodu kuyruğa bırakır.
5. **Daraltma göçünün ön denetimi** (`05 §8.7.5`; K-1121). Dönüşümün dokunduğu eski yapıyı kaldıran göç, ilk deyiminde dönüşmemiş satır kalmadığını denetler ve kalmışsa hata verir; hata göçü geri alır ve güncelleme turu durur (`05 §8.7.1`). **Bitiş veriden okunur** — dönüşmemiş satır sayısı sıfırdır; iş satırı gecelik temizlikte silindiği için iş kaydı kanıt sayılmaz.

   ```sql
   -- Daraltma göçünün ilk deyimi (kalıp). <tablo> ve <dönüşmemiş koşulu> o göçün konusudur.
   DO $$
   DECLARE remaining bigint;
   BEGIN
     SELECT count(*) INTO remaining FROM public.<tablo> WHERE <dönüşmemiş koşulu>;
     IF remaining > 0 THEN
       RAISE EXCEPTION 'conversion incomplete: % rows remain in <tablo>', remaining;
     END IF;
   END $$;
   ```
   Koşul, dönüşüm işinin yazdığı işaret sütununa dayanır ve sayım göçün başladığı andaki durumu okur. İşin kendi ilerleme kaydı §13.3'tedir.
6. **İz tablosunda göç yalnız ekler** (`05 §7.5.2`; K-965): `audit_logs` için sütun ve dizin eklenir; var olan satırın değeri değiştirilmez, sütun kaldırılmaz, ad değiştirilmez ve satır silinmez (imha yalnız fonksiyonla — §9.2). İz biçimi değişirse yeni alan yalnız yeni satırlarda dolar. Bir kez verilen işlem kodu değişmez (§9.2).
7. **Yama sürümü göç taşımaz** (`05 §8.7.3`, §8.3.12). Tek istisna veri bozan bir göçün düzeltmesidir (`05 §8.7.4`); düzeltme yeni bir göçtür, bozan göçün dosyası değiştirilmez. Kuralın CI'daki denetimi §21.4'tedir (yazım 6).
8. **Trigram dizini ve eklenti.** `pg_trgm` eklentisini ilk kurulumda veritabanının kurucu kullanıcısı kurar; şema sahibi kullanıcısı yeni yetki almaz (`05 §7.1.3`; K-1124). Bu yüzden **göç eklentiyi kurmaz, varlığını denetler:** dizini kuran göçün ilk deyimi eklentinin kurulu olduğunu sınar ve değilse açık iletiyle hata verir. Modelde `HasPostgresExtension` kullanılmaz — çağrı göçe `CREATE EXTENSION` yazar ve şema sahibi onu yapamaz (bilgim tarihli — ilk task'ta doğrulanır). Dizin `NameSearchForm` üzerindedir; yöntemi GIN, operatör sınıfı `gin_trgm_ops`'tur (`LIKE '%…%'` eşleşmesi için; bilgim tarihli). Adı adlandırma kuralından türer: `ix_products_name_search_form` (§5.2).

   ```sql
   -- AddProductNameSearchIndex göçünün ilk deyimi: eklenti kurulu değilse göç durur.
   DO $$ BEGIN
     IF NOT EXISTS (SELECT 1 FROM pg_extension WHERE extname = 'pg_trgm') THEN
       RAISE EXCEPTION 'pg_trgm extension is not installed; the database creator user installs it (DEPLOY_RUNBOOK)';
     END IF;
   END $$;
   ```
   ```csharp
   // Model kurulumunda (Catalog'un Product yapılandırması). Göç aynı dizini migrationBuilder.CreateIndex ile
   // Npgsql:IndexMethod = "gin" ve Npgsql:IndexOperators = ["gin_trgm_ops"] ek açıklamalarıyla yazar. Ad bilgim tarihli — ilk task'ta doğrulanır.
   builder.HasIndex(p => p.NameSearchForm).HasMethod("gin").HasOperators("gin_trgm_ops");
   ```
9. **Uygulama kullanıcısının adı göç kullanıcısının rol ayarıdır** (§9.2 madde 7). Göç demeti bağlantı açıldıktan sonra deyim çalıştıramaz (yalnız bağlantı dizgisini alır; bilgim tarihli — ilk task'ta doğrulanır), bu yüzden ayarı göç kodu kurmaz: kurucu kullanıcı ilk kurulumda göç kullanıcısının rol ayarına yazar — `ALTER ROLE <göç kullanıcısı> IN DATABASE <veritabanı> SET shopfolio.application_user = '<uygulama kullanıcısı>'` — ve göç kullanıcısının her oturumu ayarı hazır bulur (adımı `DEPLOY_RUNBOOK`'tadır). İşlevin `GRANT`'ı `current_setting('shopfolio.application_user')`'ı okur; ayar yoksa `current_setting` hata verir ve göç geri alınır. Başka bir göç de uygulama kullanıcısına yetki verecekse aynı ayarı okur; kullanıcının adı göç dosyasına yazılmaz.
10. **İnceleme listesi** (§25'e bu madde taşınır, yazım 6): göç dosyası taşıyan PR'da (a) her göç genişletme mi, daraltma mı — daraltmaysa önceki sürümün kodu o yapıyı kullanmıyor mu; (b) yeni değer okuyan ve yazan sürüm olarak iki sürüme bölünmüş mü; (c) `Down`, `suppressTransaction`, `CONCURRENTLY`, `now()` yok mu; (d) siparişle büyüyen tabloya `UPDATE` ya da `DELETE` yok mu; (e) kaldıran göç ön denetimle başlıyor mu; (f) `audit_logs`'a yalnız ekleme var mı; (g) kullanıcı adı ve eklenti kurulumu göçe yazılmamış mı.
11. **Test.** Kaynak taraması (birim, veritabanı gerektirmez): göç dosyalarında `Down`, `suppressTransaction`, `CONCURRENTLY`, `now()`, `CREATE EXTENSION` yoktur; `UPDATE` ve `DELETE FROM` yalnız izinli tablolardadır; `audit_logs` için `UPDATE`, `DELETE`, `DROP COLUMN`, `RENAME` yoktur. Entegrasyon (gerçek PostgreSQL, §19.1): bütün göçler boş veritabanına uygulanır ve model anlık görüntüsü modelle uyuşur (bekleyen model değişikliği yok); `pg_index` kataloğunda `ix_products_name_search_form` GIN yöntemli ve `gin_trgm_ops` operatör sınıflıdır, arama sorgusunun planı (tarama kapalı) dizini kullanır; eklentisiz veritabanında dizin göçü açık iletiyle düşer ve öncekiler kalır; göç kullanıcısının rol ayarı yokken `GRANT` göçü geri alınır, ayarlıyken yetki doğru kullanıcıdadır (`has_function_privilege`). **Geri alma sınaması CI'ın göç adımındadır** (`05 §8.3.9`, §8.7.3; §21.4, yazım 6): son yayımlanan sürümün imajı yeni göçlerin uygulandığı şemaya karşı açılır ve o sürümün entegrasyon testleri koşar — canlılık ucu yalnız açılışı sınar.

*Kaynak: K-1340 · K-925, K-965, K-981, K-1121, K-1124, K-1337 · `05 §8.7`, §8.2.1, §8.3.9, §7.5.2, §7.1.3 · `06 §4.5`, §4.63, §7.5.1, §8.5.2 · `DEPLOY_RUNBOOK` (K-1124 satırı) · §4.1, §5.2, §7.1, §9.2, §10.1, §13.3, §21.4.*

### 10.7 Veritabanı oturumunun süreleri

Bu alt bölüm veritabanı oturumunun üç sunucu taraflı zaman aşımını ve istemci yedeğini bağlantıya **nasıl kurduğunu**, aşımın kodda nasıl karşılandığını ve canlılık ucunun veritabanı denetimini yazar. **Değerlerin tek yeri §3.5'in tablosudur;** burada yinelenmez (yazım konvansiyonu 7). Tablonun öteki satırlarının kod yerleri kendi bölümlerindedir: dış çağrı süresi §11, iş kuyruğu aralığı §13, Google belirtecinin `exp` toleransı §15 (yazım 5), dışa aktarmanın tavanları ve süpürme yaşı §10.10, ters vekil süresi `deploy/caddy/Caddyfile`'da (§4.3, §10.10 madde 7).

1. **Değerler `Infrastructure/DatabaseTimeouts`'ta durur.** `internal static` sınıf; her değer §3.5 tablosunun bir satırına karşılık gelen adlı bir `TimeSpan` sabitidir: `LockTimeout`, `StatementTimeout`, `MaintenanceStatementTimeout`, `IdleInTransactionTimeout`, `CommandTimeout`. Süre ve tavan değerleri `02 §11.2`'nin envanterine girmez; bu yüzden `ProductConstants`'a yazılmaz (§3.5 madde 1), bu sınıfın kendi evidir. Sayı başka yerde yazılmaz: bağlantı dizisi, test ve canlılık denetimi bu sabitlere başvurur.
2. **Bağlantıya kuruluş tek yerdedir:** `Infrastructure/DatabaseConnection.Configure(NpgsqlConnectionStringBuilder builder, DatabaseSessionProfile profile)` üç sunucu ayarını `Options` anahtarıyla, istemci süresini `CommandTimeout` ile kurar. Profil ikidir: `Web` (uygulama hizmeti ve içindeki iş kuyruğu çalıştırıcısı; canlılık ucu ve dışa aktarma dahil) ve `Maintenance` (göç hizmeti ve yönetim komutları; yalnız `statement_timeout` uzar, öteki iki değer aynıdır). Profili kayıt kökü giriş noktasına göre seçer; bir istek ya da komut profil seçmez.

   ```csharp
   // DatabaseTimeouts'ın değerleri §3.5 tablosundadır; bu örnek onları yinelemez.
   // Options ve CommandTimeout adları Npgsql'in sürümüne bağlıdır — bilgim tarihli — ilk task'ta doğrulanır.
   public enum DatabaseSessionProfile { Web, Maintenance }

   internal static class DatabaseConnection
   {
       public static void Configure(NpgsqlConnectionStringBuilder builder, DatabaseSessionProfile profile)
       {
           var statement = profile == DatabaseSessionProfile.Maintenance
               ? DatabaseTimeouts.MaintenanceStatementTimeout
               : DatabaseTimeouts.StatementTimeout;
           builder.Options =
               $"-c lock_timeout={(int)DatabaseTimeouts.LockTimeout.TotalMilliseconds} " +
               $"-c statement_timeout={(int)statement.TotalMilliseconds} " +
               $"-c idle_in_transaction_session_timeout={(int)DatabaseTimeouts.IdleInTransactionTimeout.TotalMilliseconds}";
           builder.CommandTimeout = (int)DatabaseTimeouts.CommandTimeout.TotalSeconds;
       }
   }
   ```
   Ayar bağlantı kurulurken sunucuya gider ve oturum boyunca geçerlidir; havuzdaki bağlantı ayarı korur. Bu yüzden iki profil **ayrı bağlantı dizisidir** (ayrı havuz); bir bağlantıda `SET` ile süre değiştirilmez. Kaynak taraması: `SET statement_timeout`, `SET lock_timeout`, `SET idle_in_transaction_session_timeout` ve `SET LOCAL` biçimleri yalnız `DatabaseConnection`'da ve testlerde geçer.
3. **İstemci süresi yedektir.** `CommandTimeout` her sunucu süresinden uzundur (§3.5): sunucu iptali önce gelir, istemci yalnız sunucu yanıt vermezse keser. Komut başına `CommandTimeout` atanmaz; uzun süren iş yeni bir süre değil, sayfalamadır (§10.10 madde 4).
4. **Aşım ayrı bir hâl değildir.** Kilit bekleme aşımı (`55P03`), sorgu iptali (`57014`) ve boşta işlem aşımının kestiği bağlantı beklenmeyen hatadır ve tek yakalama katmanında üç hâlden birine düşer (§8.1; `05 §10.3`). Kod bunları **yakalamaz, çevirmez, yeniden denemez:** `PostgresException`'ı `SqlState`'ine göre ayıran `catch` yalnız tekil dizin çakışmasında (`23505`, §10.2) vardır. Ayrı ekran ya da ileti yoktur. Kuyruk satırı bu hatalarda iş türünün kuralına göre denenir (`05 §4.3.4`; §13).
5. **Öncül: işlemin içinde dış çağrı yoktur.** Boşta işlem süresi, açık işlemin bir dış servisi beklemediği varsayımına dayanır; dış yan etki iş kuyruğuna yazılır ve çağrı işlemin dışındadır (§9.1 madde 6). Kaynak taraması (birim, `Infrastructure/` dışında): `ITransactionRunner` gövdesinde `HttpClient` ya da dış servis arayüzü kullanımı yoktur.
6. **Canlılık ucunun veritabanı denetimi.** `Infrastructure/LivenessProbe.CheckDatabaseAsync(ct)` bir bağlantı açar ve `SELECT 1` çalıştırır; süre **kurulum ayarıdır** (§3.5 tablosunun satırı; varsayılanı tabloda): ayar anahtarı `Database:LivenessTimeoutSeconds`, seçenekler sınıfı `DatabaseOptions`. Açılışta doğrulanır: sıfırdan büyük ve `CommandTimeout`'tan kısa olmalıdır; değilse uygulama açılmaz ve ayarı adıyla kaydeder (§3.5 madde 2). Denetim hem bağlantıyı açmayı hem komutu kapsar: istek iptali belirteciyle bağlı bir `CancellationTokenSource` `CancelAfter(ayar)` ile kurulur; süre dolarsa ya da komut hata verirse yöntem `false` döner ve uç gövdesiz 503 yazar (`07 §4.33.1`; makine ucu, §8.1). Süre dolması bir kayıt satırı yazar (§18, yazım 5); istek iptali yazmaz.

   ```csharp
   public static async Task<bool> CheckDatabaseAsync(NpgsqlDataSource dataSource, TimeSpan limit, CancellationToken ct)
   {
       using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
       timeout.CancelAfter(limit);
       try
       {
           await using var connection = await dataSource.OpenConnectionAsync(timeout.Token);
           await using var command = new NpgsqlCommand("SELECT 1", connection);
           await command.ExecuteScalarAsync(timeout.Token);
           return true;
       }
       catch (Exception) when (!ct.IsCancellationRequested) { return false; } // §8.1: makine ucunda geniş catch'in izinli yeri
   }
   ```
   Compose sağlık denetiminin kendi zaman aşımı bu süreden uzun olmalıdır; değeri D-01'in işidir ve burada yazılmaz.
7. **Test.** Birim: üç sunucu ayarı ve istemci süresi `Web` ve `Maintenance` profillerinde bağlantı dizisinde doğru yazılır; `DatabaseTimeouts` sabitleri §3.5'in çelişki taramasını sağlar (kilit < sorgu < istemci süresi; dışa aktarma sayfa sorgusu < üretim tavanı < ters vekil süresi); `DatabaseOptions` doğrulaması (sıfır, negatif ve istemci süresinden uzun değer açılışı reddeder); kaynak taramaları (madde 2 ve 5). Entegrasyon (gerçek PostgreSQL, §19.1): iki işlemle kilit bekleme — ikincisi `lock_timeout` içinde düşer ve beklenmeyen hâle gider; `pg_sleep` ile sorgu iptali; boşta kalan açık işlemin bağlantısı kesilir; `Maintenance` profilinde `SHOW statement_timeout` uzun değeri, `Web` profilinde kısa değeri gösterir. Canlılık denetimi: yavaş veritabanında `false` ve 503; erişilemeyen veritabanında `false`; istek iptalinde `false` döner ama kayıt yazılmaz.

*Kaynak: K-1342 · K-1328 · K-1257, K-1258, K-1322 · K-889, K-1272 · `05 §10.3`, §10.1.14, §4.3.4 · `07 §4.33.1` · §3.5, §8.1, §9.1, §10.2, §10.10, §13, §15.*

### 10.8 Veritabanı kullanıcıları

Veritabanının kendi kullanıcıları hesap kaydı değildir (`06 §8.3.2`); hangi bileşenin hangi kullanıcıyla bağlandığı `05 §7.1.3`'te, sırlarının dağılımı `05 §6.x`'te yazılıdır. Bu alt bölüm kullanıcıların **biçimini ve yetkisini**, yetkinin göçlerde nasıl korunduğunu ve nasıl sınandığını yazar. Kullanıcıları ve yetkileri ilk kurulumda veritabanının kurucu kullanıcısı kurar; betiğin dosyası ve imajdaki yeri §17'dedir (yazım 5), `DEPLOY_RUNBOOK`'ta adımı vardır.

1. **Beş kullanıcı, beş iş.** Adlar sabittir; kodda ve göçte yazılmaz, bağlantı ayarından ve madde 5'in rol ayarlarından gelir.

   | Kullanıcı | Ad | Kim bağlanır | Yetki |
   |---|---|---|---|
   | Kurucu | imajın kurucu kullanıcısı | Yalnız ilk kurulumda kurucu betik; hiçbir bileşen ağdan bağlanmaz (`05 §7.1.3`; K-987) | Süper kullanıcı; kullanıcıları, yetkileri ve `pg_trgm`'i kurar (§10.6 madde 8) |
   | Şema sahibi (göç) | `shopfolio_migrator` | Yalnız göç hizmeti (`05 §8.7.2`) | Veritabanının ve `public` şemasının sahibi; nesneleri o oluşturur; süper kullanıcı değildir, rol ve veritabanı yaratamaz |
   | Uygulama | `shopfolio_app` | Uygulama; veri yazan yönetim komutları ve durum komutu (`05 §6.x.19`) | `CONNECT`; `public` şemasında `USAGE`; her tabloda `SELECT`, `INSERT`, `UPDATE`, `DELETE`; `TRUNCATE`, `REFERENCES`, `TRIGGER` ve `CREATE` yok; istisnalar madde 3'te |
   | Salt okuma | `shopfolio_reader` | Okuma komutu, bakım, dönüş denetimi (`05 §9.3.1`, §8.5.11) | `CONNECT`, `USAGE`; her tabloda yalnız `SELECT`; `data_protection_keys`'te yok; oturumu `default_transaction_read_only = on` |
   | Yedeğin durum kaydı | `shopfolio_backup_status` | Veritabanı konteynerindeki zamanlayıcı, yerel soketle (`06 §4.67`) | `backup_status_records`'ta yalnız `last_succeeded_at` ve `last_failed_at` sütunlarının `UPDATE`'i |

   Rollerin hiçbiri `SUPERUSER`, `CREATEDB`, `CREATEROLE`, `REPLICATION` ve `BYPASSRLS` taşımaz. `PUBLIC`'ten veritabanı ve şema yetkisi geri alınır; her kullanıcıya yetkisi ayrıca verilir. Yedeğin durum kullanıcısının parolası yoktur ve yalnız yerel soketten bağlanır; kimlik doğrulamasının biçimi (`peer` ya da `trust`) D-01'de doğrulanır (bilgim tarihli); ağdan bağlanan, parolalı bir yedek kullanıcısı yoktur (`05 §3.2.5`). Öteki üç kullanıcının parolası ayrı sırdır ve açık metinle bir deyime yazılmaz (`05 §6.x.16`).
2. **Yetkinin verildiği yer.** Kurucu betik şunları yapar (örnek yer tutucularla; sıra: kullanıcılar, sahiplik, varsayılan yetkiler):

   ```sql
   CREATE ROLE shopfolio_migrator LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE;
   CREATE ROLE shopfolio_app LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE;
   CREATE ROLE shopfolio_reader LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE;
   CREATE ROLE shopfolio_backup_status LOGIN NOSUPERUSER NOCREATEDB NOCREATEROLE;
   ALTER ROLE shopfolio_reader SET default_transaction_read_only = on;
   REVOKE ALL ON DATABASE <veritabanı> FROM PUBLIC;
   GRANT CONNECT ON DATABASE <veritabanı> TO shopfolio_migrator, shopfolio_app, shopfolio_reader, shopfolio_backup_status;
   ALTER DATABASE <veritabanı> OWNER TO shopfolio_migrator;
   ALTER SCHEMA public OWNER TO shopfolio_migrator;
   REVOKE ALL ON SCHEMA public FROM PUBLIC;
   GRANT USAGE ON SCHEMA public TO shopfolio_app, shopfolio_reader, shopfolio_backup_status;
   ALTER DEFAULT PRIVILEGES FOR ROLE shopfolio_migrator IN SCHEMA public
     GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO shopfolio_app;
   ALTER DEFAULT PRIVILEGES FOR ROLE shopfolio_migrator IN SCHEMA public
     GRANT SELECT ON TABLES TO shopfolio_reader;
   GRANT EXECUTE ON FUNCTION pg_catalog.pg_ls_archive_statusdir() TO shopfolio_app;
   ```
   Son satır durum komutunun bekleyen günlük dosyaları listesini okuyabilmesi içindir (`05 §9.4.10`); izleme rolünün (`pg_monitor`) tamamı verilmez. Varsayılan yetkiler göçle sonradan oluşan her tabloya uygulanır: yeni tablo için `GRANT` yazılmaz. Ad ve işlev imzası bilgim tarihlidir; ilk task'ta doğrulanır. `default_transaction_read_only` bir **güvenlik sınırı değildir** (okuyucu onu `SET` ile kapatabilir); sınır yalnız yetkidir, ayar kazaya karşıdır.
3. **Üç tablo varsayılandan ayrılır; ayrım tabloyu kuran göçtedir.**

   | Tablo | Uygulama kullanıcısı | Salt okuma | Yedek durum kullanıcısı | Sebep |
   |---|---|---|---|---|
   | `audit_logs` | `SELECT`, `INSERT` (`UPDATE`, `DELETE` geri alınır) | `SELECT` | — | İz saklama süresi boyunca değiştirilemez ve silinemez (`05 §7.5.1`; §9.2) |
   | `data_protection_keys` | varsayılan | `SELECT` geri alınır | — | Anahtar halkasını yalnız uygulama okur (`05 §7.1.3`; K-1082) |
   | `backup_status_records` | varsayılan | varsayılan | `last_succeeded_at`, `last_failed_at` sütunlarında `UPDATE` | Satırı zamanlayıcı yazar (`06 §4.67`) |

   Göç, ayrımı varsayılanın üstüne `REVOKE` ve `GRANT` ile yazar; kullanıcı adını göç dosyasına yazmaz, madde 5'in rol ayarından okur. Kalıp (`audit_logs`'u kuran göçte):

   ```sql
   DO $$ BEGIN
     EXECUTE format('REVOKE UPDATE, DELETE ON public.audit_logs FROM %I', current_setting('shopfolio.application_user'));
   END $$;
   ```
   `data_protection_keys` için aynı kalıp `shopfolio.reader_user` ile `REVOKE SELECT`; `backup_status_records` için `shopfolio.backup_status_user` ile `GRANT UPDATE (last_succeeded_at, last_failed_at)`. Zamanlayıcının güncellemesi `WHERE`'siz tek deyimdir (tablo tek satırdır, `06 §4.67`); bu yüzden `SELECT` verilmez. Tabloya sütun eklenirse yetki sütun listesi kapsamaz: sonradan eklenen sütunu zamanlayıcı yazamaz — istenirse yeni göçte açıkça verilir.
4. **Ayrım tabloyla yaşar.** Tablo yeniden adlandırılırsa yetkiler tabloyla gider; yeniden adlandırma iki sürüme bölündüğü için (§10.6) eski ve yeni ad birlikte var olduğu sürümde ayrım ikisinde de bulunur, yetki matrisi sınaması (madde 7) yeni adı istisna listesine alır. Üç tablodan birini silip yeniden kuran göç, ayrımı yeniden yazmak zorundadır.
5. **Kullanıcı adları rol ayarıyla gelir.** §10.6 madde 9'un mekanizması üç ayara genişler: kurucu betik, göç kullanıcısının rol ayarına `shopfolio.application_user`, `shopfolio.reader_user` ve `shopfolio.backup_status_user` değerlerini yazar (`ALTER ROLE shopfolio_migrator IN DATABASE <veritabanı> SET … = '…'`); ayar yokken `current_setting` hata verir ve göç geri alınır. Üç ayar da kullanıcı yeniden adlandırılırsa güncellenir (`DEPLOY_RUNBOOK`).
6. **Kodda tek kural: bileşen kendi kullanıcısıyla bağlanır.** Uygulama ve komutlar `shopfolio_app`, göç hizmeti `shopfolio_migrator`, okuma komutu `shopfolio_reader` bağlantı ayarını okur; bir bileşenin başka bileşenin bağlantı ayarını okuması yasaktır (`05 §6.x.8`). Uygulama açılışında yetki sınanmaz; yetkinin doğruluğu aşağıdaki testlerdedir. Uygulama kullanıcısı şema değiştiremediği için `EnsureCreated`, `Migrate` ve model değişikliği uygulayan her çağrı yalnız göç hizmetinde ve testlerin göç kullanıcısı bağlantısında bulunur (kaynak taraması).
7. **Test** (entegrasyon, gerçek PostgreSQL, §19.1; testin göç kullanıcısı bağlantısıyla kurduğu veritabanında):
   - *Yetki matrisi:* `information_schema` ve `has_table_privilege` ile `public` şemasındaki **her tablo** için beklenen yetki karşılaştırılır; beklenen, madde 2'nin varsayılanı ile madde 3'ün üç istisnasıdır. Yeni bir tablo istisna listesinde olmadan varsayılandan ayrışırsa test düşer.
   - *Uygulama kullanıcısı:* `CREATE TABLE`, `DROP TABLE`, `ALTER TABLE` ve `audit_logs`'ta `UPDATE`, `DELETE`, `TRUNCATE` reddedilir; `INSERT` ve `SELECT` çalışır; `data_protection_keys`'e kimlik sütunlu `INSERT` ek bir dizi yetkisi olmadan çalışır (bilgim tarihli); `pg_ls_archive_statusdir()` çağrılabilir, `pg_stat_activity` başka oturumun sorgusunu göstermez.
   - *Salt okuma kullanıcısı:* her tabloda `SELECT` (üç istisna dışında); herhangi bir `INSERT`, `UPDATE`, `DELETE` ve `data_protection_keys` okuması reddedilir.
   - *Yedek durum kullanıcısı:* iki sütunun `UPDATE`'i çalışır; başka sütun, başka tablo, `SELECT`, `INSERT`, `DELETE` reddedilir. Yerel sokette bağlanıp ağdan bağlanamaması Compose entegrasyon denemesindedir (D-01).
   - *Göç kullanıcısı:* süper kullanıcı değildir (`rolsuper` yanlıştır); rol ve veritabanı yaratamaz.
   - *Rol ayarları:* ayar yokken `audit_logs` göçü geri alınır; ayarlıyken yetki doğru kullanıcıdadır.
   - *Kaynak taraması* (birim): göç dosyalarında `shopfolio_` ile başlayan sabit kullanıcı adı yoktur; `GRANT`, `REVOKE` ve `ALTER DEFAULT PRIVILEGES` yalnız `current_setting` kalıbıyla ve kurucu betikte bulunur.
8. **İnceleme listesi** (§25'e taşınır, yazım 6): kullanıcı ya da tablo ekleyen PR'da (a) yeni tablo varsayılan yetkiyle doğru mu, değilse istisna tablosu ve test güncel mi; (b) kullanıcı adı göçe yazılmamış, rol ayarıyla mı geliyor; (c) uygulama kullanıcısına şema, süper kullanıcı ya da izleme yetkisi eklenmemiş mi; (d) bir bileşen başka bileşenin sırrını alıyor mu (`05 §6.x.8`).

*Kaynak: K-1343 · K-1098 (4), K-1082, K-1068 (5), K-975, K-986, K-987, K-1124, K-1340 · `05 §7.1.3`, §7.5.1, §7.5.2, §6.x.4, §6.x.6, §6.x.8, §6.x.16, §6.x.19, §3.2.5, §8.7.2, §9.4.10 · `06 §4.63`, §4.67, §4.68, §8.3.2 · `DEPLOY_RUNBOOK` (K-1340 ve K-1343 satırları) · §5.2, §9.2, §10.6, §17 (yazım 5), §19, §25.*

### 10.9 İç notun kilidi

İç not siparişe bağlı serbest metindir ve kişisel veri taşıyabilir (`06 §4.20`); sipariş imha edilince notun satırı imha parçasında silinir (`05 §7.4.3` (2)). Yazım, imha edilmiş siparişe not bırakmamalıdır. Kuralın kendisi Veri Modeli'ndedir (`06 §4.20`, §9.2.1; `02 §10.4.7`); bu alt bölüm kilidin ve koşullu yazımın **kodda yazılışını** söyler.

1. **Kilit paylaşımlı satır kilididir.** Yazım, siparişin satırını `FOR SHARE` ile okur. Bu kilit siparişin sürümünü artırmaz (sahte çakışma doğurmaz, `06 §2.6.4`) ve imha parçasının satır kilidiyle çakışır: imha önce kesinleşmemişse yazım bekler ve kesinleşince boşalmış siparişi görür; yazım önce kilitlemişse imha onun bitmesini bekler. İki not yazımı birbirini beklemez (paylaşımlı kilitler uyuşur); aralarındaki çakışma notun kendi koşuludur (madde 3). Bekleme `lock_timeout` ile sınırlıdır (§3.5 tablosu); aşım beklenmeyen hatadır ve form isteğinde işlem düzeyindedir (§10.7 madde 4).
2. **Kilit `OrderLocks`'tadır.** Sipariş satırı kilidinin tek evi `Ordering/OrderLocks`'tır (§9.1 madde 5); paylaşımlı biçim aynı sınıfa eklenir: `OrderLocks.LockSharedAsync(Guid orderId, ct)`, `SELECT … FOR SHARE` ham SQL'ini çalıştırır (`FromSql`; bilgim tarihli) ve siparişin iletişim e-postasını döndürür; sipariş yoksa `null` döner. İletişim e-postası boş (`null` ya da boş metin) ise sipariş imha edilmiştir. Kaynak taraması §9.1 madde 5'in cümlesi genişler: sipariş satırını kilitleyen `FOR UPDATE` ve `FOR SHARE` yalnız `OrderLocks`'tadır. Kilit işlemin ilk deyimidir ve işlem açık değilse yöntem istisna atar (`AdvisoryLocks` ile aynı kural, §10.2 madde 5).
3. **Yazım koşullu güncellemedir** (`06 §4.20`; K-1051; §10.2 madde 4'ün kalıbı). Geçiş birimi `Ordering/SaveInternalNote` (`SaveInternalNote`, §9.2 işlem adı) şu sırayla işler:
   1. İşlem açılır; `LockSharedAsync` çağrılır. Sipariş yoksa ya da iletişim e-postası boşsa hiçbir şey yazılmaz ve sonuç `OperationNotAvailable`'dır (422; `07 §9.9.31`).
   2. Gönderilen metin boş değilse: satır varken, satırın metni gösterilen metinle (`OriginalText`) aynıysa `ExecuteUpdateAsync` ile yazılır; satır yokken ve gösterilen metin boşken `INSERT` yapılır. Gösterilen metin boş değilken satır yoksa ya da farklıysa sonuç `DisplayedValueConflict`'tir (409) ve güncel not okunup döndürülür.
   3. Gönderilen metin boşsa satır `ExecuteDeleteAsync` ile, `Text == OriginalText` koşuluyla silinir; 0 satır etkilendiyse `DisplayedValueConflict`'tir (satır yoksa ve gösterilen metin boşsa zaten bir şey yapılmaz ve başarıdır).
   4. İz satırı aynı işlemde yazılır (`SaveInternalNote`; `Details` notun metnini taşımaz, §9.2 madde 4). Başarısız sonuçlarda iz yazılmaz.

   Koşul aynı kayda iki yöneticinin eşzamanlı ilk yazımını da kapsar: ikisi de satır yokken `INSERT` ederse ikincisi `OrderId`'nin tekil dizinine takılır (`23505`) ve §10.2'nin kuralıyla işlem yeniden yürütülür; ikinci geçişte not artık var ve gösterilen metin boş kaldığı için `DisplayedValueConflict` döner.

   ```csharp
   // Ordering/SaveInternalNote — kilit işlemin ilk deyimi; ad ve sonuç türleri §9.1'in kalıbındandır.
   var contactEmail = await _orderLocks.LockSharedAsync(orderId, ct);
   if (string.IsNullOrEmpty(contactEmail)) return SaveInternalNoteResult.OperationNotAvailable;

   var written = await _db.InternalNotes
       .Where(n => n.OrderId == orderId && n.Text == originalText)
       .ExecuteUpdateAsync(s => s.SetProperty(n => n.Text, text)
                                 .SetProperty(n => n.AdminId, adminId)
                                 .SetProperty(n => n.AdminName, adminName), ct);
   if (written == 0 && originalText.Length == 0) { /* INSERT; 23505 → işlemin yeniden yürütülmesi (§10.2) */ }
   else if (written == 0) return SaveInternalNoteResult.DisplayedValueConflict;
   ```
   Not siparişin sürümünü artırmaz: `Order` satırına `UPDATE` yazılmaz (kaynak taraması: `SaveInternalNote` içinde `Orders` üzerinde `ExecuteUpdate` ya da değişiklik izleme yoktur).
4. **Öteki işlemlerle ilişkisi.** Siparişi `FOR UPDATE` ile kilitleyen yönetici işlemi (§9.1 madde 5) tutulurken not yazımı onu bekler; not yazımı `FOR SHARE` tuttuğu kısa süre içinde o işlemi bekletir, başka hiçbir işlemi bekletmez. Not yazımı tek sipariş kilitler ve çok siparişli kilit sırasının (§9.1 madde 5) konusu değildir.
5. **Test** (entegrasyon, gerçek PostgreSQL, §19.1): iki gerçek işlemle — (a) imha parçası siparişi güncellerken (kilit tutulurken) not yazımı bekler, imha kesinleşince `OperationNotAvailable` döner ve hiçbir satır yazılmaz; (b) not yazımı kilitliyken imha onun bitmesini bekler; (c) iki not yazımı birbirini bloklamaz; (d) iki yöneticinin ilk yazımı: biri başarılı, öteki `DisplayedValueConflict`; (e) gösterilen metin eskiyken yazım `DisplayedValueConflict` verir ve güncel notu döndürür; (f) boş metin satırı siler; (g) sipariş sürümü hiçbir yazımda değişmez; (h) not metni iz satırına girmez. Kilit çakışmasının PostgreSQL'in bugünkü sürümünde gerçekten oluştuğu (a) ve (b) ile doğrulanır; çakışma yoksa Veri Modeli güncellenir (`06 §4.20`; K-1095).

*Kaynak: K-1344 · K-1095, K-1051, K-1014 (3), K-1010 (4), K-1034 (4) · `05 §7.3.2`, §7.4.3 · `06 §2.6.4`, §4.20, §9.2.1 · `07 §9.9.31`, hata kodları (`OperationNotAvailable`, `DisplayedValueConflict`) · `02 §10.4.7` · §3.5, §9.1, §9.2, §10.2, §10.3, §10.7.*

### 10.10 Dışa aktarma dosyası

Üç dışa aktarma — siparişler, üye listesi, iletişim talepleri (`07 §9.24.2`–§9.24.4) — aynı yazıcıyı ve aynı dosya yolunu kullanır. Dosyanın içeriği ve iz satırı `07`'de, yedeğe girmeyen dizinde üretilmesi `05 §6.8.4`'te yazılıdır; bu alt bölüm **CSV'nin iç biçimini, yazıcıyı, akışla üretimi, geçici dizini ve çökme artığını** yazar. Kütüphane yoktur (§3'ün "Kütüphanesiz kararlar" listesi).

1. **Yazıcı tek sınıftır.** `Infrastructure/CsvExportWriter`, sütun tanımlarını tipli alır: `CsvColumn<TRow>(string Header, CsvColumnKind Kind, Func<TRow, object?> Value)`; `CsvColumnKind` altı değerlidir: `Text`, `Amount`, `Quantity`, `Rate`, `Date`, `Moment`. Her hücre kendi türünün yazıcısından geçer; tür kuralı hücreyi yazan kodun değil, tanımın işidir. Üç dışa aktarma kendi sütun listesini `OrderExportColumns`, `CustomerExportColumns`, `ContactRequestExportColumns` adlarıyla satırı sağlayan modülün klasöründe tanımlar (sipariş `Ordering`, üye listesi `Membership`, iletişim talebi `CorporateContent`; `05 §3.3.7`); yazıcı modülleri bilmez ve `Infrastructure/` içindedir.
2. **Biçim** (`07 §9.24.2`'nin bıraktığı yerler; K-1321 (2)):
   - Kodlama UTF-8, başında BOM (`new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)`); satır sonu `\r\n`; hücre ayırıcı noktalı virgül.
   - Hücre `;`, `"`, CR ya da LF içeriyorsa çift tırnağa alınır ve içindeki `"` ikilenir. Başlık satırı Türkçe sütun adlarıdır (madde 5).
   - `Amount`: iki ondalık, virgül ayracı, binlik ayracı yok (`1250,00`; negatif `-12,50`); `Rate`: yüzde sayısı, sondaki sıfırlar yok (`20`, `7,5`); `Quantity`: tam sayı; `Date`: `yyyy-MM-dd`; `Moment`: `yyyy-MM-dd HH:mm`; ikisi de Türkiye saatiyledir (§7.1 madde 3: `TurkeyTime.DateOf`, anın sabit ofsetle yazımı); `null` boş hücredir. Sayı yazımı kültüre bağlı değildir (`CultureInfo.InvariantCulture` ve ayraç değişimi): konteynerde `tr-TR` kültür verisi bulunmayabilir (§7.2).
   - Noktalı virgül ve virgül ayracı Excel'in Türkçe yerel ayarının liste ve ondalık ayracıdır (bilgim tarihli); ilk task'ta Excel'de açılış denenir (K-1321 (2)).
3. **Formül etkisizleştirme yalnız `Text` hücrelerindedir.** Hücrenin ilk karakteri `=`, `+`, `-`, `@`, sekme (`\t`) ya da CR ise hücrenin başına tek tırnak (`'`) konur; **sonra** tırnaklama kuralı uygulanır. Sayısal türler (`Amount`, `Quantity`, `Rate`) ve tarih türleri metin yazıcısından geçmez: negatif tutarın (geç gelip iade edilen ödeme) başına ön ek konmaz, Excel onu sayı olarak toplar. Kural üç dışa aktarmanın bütün metin sütunlarında, ad, e-posta, adres ve mesaj dahil geçerlidir (K-1217 (3)).

   ```csharp
   // Yanlış: tırnaklayıp etkisizleştirmek ya da sayıya da uygulamak.
   //   var cell = Quote(value); if (IsFormulaStart(cell)) cell = "'" + cell;       // tırnak ilk karakter olur, kural işlemez
   //   decimal tutar → TextCell(tutar.ToString(...))                              // negatif tutar metne dönüşür
   // Doğru:
   private static string TextCell(string? value)
   {
       if (string.IsNullOrEmpty(value)) return string.Empty;
       if (value[0] is '=' or '+' or '-' or '@' or '\t' or '\r') value = "'" + value;
       return NeedsQuoting(value) ? "\"" + value.Replace("\"", "\"\"") + "\"" : value;
   }
   ```
4. **Akışla yazım.** Yazıcı bir `Stream`'e, `IAsyncEnumerable<TRow>` kaynağından satır satır yazar; bütün küme belleğe alınmaz (`ToList` yasaktır). Sipariş kümesi anahtar sırasıyla (`Id`) 500 siparişlik sayfalarla okunur (`Id > son anahtar`, `OrderBy(Id)`, `Take`; `AsNoTracking`); sayfanın kalemleri tek ek sorguyla alınır ve her kalem bir satırdır, sipariş alanları kalemlerde yinelenir. Sayfa sorgusu §3.5'teki sorgu süresinin içindedir (§10.7); üretim süresi tavanı toplamı sınırlar. Üye listesi ve iletişim talepleri aynı sayfalama kalıbıyla (`Id` anahtarı) okunur. Dosya sırası listelerin kesin sırası kuralının (§10.5) dışındadır: dosya bir liste ekranı değil, anahtar sırasıyla bir dökümdür.
5. **Sütunlar.** Başlıklar sözlüğün Türkçe terimleridir; alanların hepsi `06 §4`'ten alınır ve her tanım kaynak alanını yorumda taşır. Sipariş dosyasında satır bir sipariş kalemidir (`02 §10.7.1`):

   | Grup | Sütun (başlık) | Kaynak |
   |---|---|---|
   | Sipariş | Sipariş numarası, Sipariş tarihi (`Date`), Ödeme yöntemi | `Order.OrderNumber`, `Order.CreatedAt`, `Order.PaymentMethod` |
   | Kalem | Ürün adı, Birinci seçenek, İkinci seçenek, Adet, Birim fiyat, İndirim yüzdesi, İndirimli birim fiyat, Kupon payı, KDV oranı, KDV tutarı | `OrderItem.ProductName`, `FirstOptionValue`, `SecondOptionValue`, `Quantity`, `UnitPrice`, `DiscountPercentage`, `DiscountedUnitPrice`, `CouponShare`, `VatRate`, `VatAmount` |
   | Sipariş toplamı | Sipariş toplamı, Kargo ücreti, Kargo ücretinin KDV dökümü (`Text`) | `Order.Total`, `Order.ShippingFee`, `Order.ShippingFeeVatBreakdown` |
   | Fatura adresi | Alıcı adı, İl, İlçe, Adres | `Order.BillingRecipientName`, `BillingProvinceId` (il adı), `BillingDistrict`, `BillingAddressLine` |
   | Firma kimliği | Ticaret unvanı, Vergi numarası, Mersis numarası, Ticaret sicil numarası, İl, İlçe, Adres | `Order.CompanyLegalName`, `CompanyTaxIdNumber`, `CompanyMersisNumber`, `CompanyTradeRegistryNumber`, `CompanyProvinceId` (il adı), `CompanyDistrict`, `CompanyAddressLine` |
   | Ulaşma | İletişim e-postası, Teslimat alıcı adı | `Order.ContactEmail`, `Order.ShippingRecipientName` |
   | Kargo | Kargo şirketi, Kargoya verme tarihi (`Date`) | `Order.Carrier` ya da `OtherCarrierName`; fiziksel kalemde kargoya verildi geçişinin günü, dijital ve hizmet kaleminde `OrderItem.DeliveryMarkedAt`'in günü |
   | Ödeme | Ödeme onay tarihi (`Date`) | Son ödeme onayı geçiş kaydının günü (`06 §4.19`; `02 §3.20.6`'nın eşlemesi) |
   | Geç ödeme | Geç ödeme tarihi (`Date`), Geç ödeme tutarı, Geri ödeme tarihi (`Date`), Geri ödeme tutarı | `PaymentRecord.CreatedAt`, `Order.Total`, `Refund.SentAt`, `Refund.Amount` — yalnız iptal edilmiş siparişe geç gelip sistemce geri ödenen ödemede dolu, aksi hâlde boş |

   Enum değerli sütunlar (`Ödeme yöntemi`, `Kargo şirketi`) değerin kod adını değil, ekranlarda kullanılan Türkçe terimi yazar; eşleme `06 §5`'in değer listesinden tek bir `CsvEnumLabels` sözlüğündedir ve eksik değer açılışta testle yakalanır. Kargo KDV dökümü hücresi `oran=tutar` çiftlerinin `|` ile ayrılmış listesidir (`20=12,50|10=3,20`). **Kişisel verisi imha edilmiş siparişte** ad, e-posta ve adres sütunları boştur (`Fatura adresi` grubu, `İletişim e-postası`, `Teslimat alıcı adı`); firma kimliği ve tutarlar kalır (ticari kayıt; `06 §9.2.1`). **Üye listesi** iki sütundur: `Ad`, `E-posta` (`Customer.Name`, `Customer.Email`; doğrulanmamış bekleyen kayıt dahil). **İletişim talepleri** iki sütundur: `Ad`, `E-posta` (`ContactRequest.Name`, `ContactRequest.Email`).
6. **Geçici dizin ve dosya.** Dizin kurulum ayarıdır: anahtar `Export:TempDirectory`; varsayılanı konteynerin geçici dizini altında `shopfolio-export`'tur ve Compose'ta bir birime bağlanmaz — yedeğe girmez (`05 §7.6.3`, §9.3.3). Uygulama açılışta dizini oluşturur ve yazma yetkisini sınar; yoksa ya da yazılamıyorsa açılış reddedilir (§3.5 madde 2). Dosya adı rastgele bir `Guid`'dir (`{guid:N}.csv`) ve firmanın ya da yöneticinin adını taşımaz; indirme adı `07 §9.24.2`'dedir ve diskteki adla ilgisizdir. Dosya yalnız uygulamanın kullanıcısının okuyabileceği kiple açılır; ikinci bir yedek kopya ya da günlük kaydı bu dizinden okunmaz.
7. **Silme ve tavanlar.** Akış `FileOptions.DeleteOnClose` ile açılır: gönderim biter, hata olur ya da istek iptal edilirse akış kapanınca dosya silinir; yöntem `await using` ile kapsanır. Üretim süresi ve dosya boyutu tavanı §3.5 tablosundadır. Süre `CancelAfter` ile, boyut yazıcının yazdığı bayt sayısıyla sınanır (her sayfa sonunda); aşılırsa üretim durur, dosya silinir, iz yazılmaz ve yanıt `UnexpectedOperationError` ile 500'dür (`07 §9.24.2`; §8.1). Süre dolduğundaki iptal ile istemcinin bağlantıyı kesmesi karışmaz: `CancellationTokenSource` zaman aşımı kaynağı ayrıdır ve yalnız o tetiklenmişse dahili `ExportLimitExceededException` atılır. İz satırı dosya üretildikten sonra ve gönderimden önce yazılır (`07 §9.24.2`; `Export` işlemi, §9.2). Ters vekilin dışa aktarma yollarındaki yanıt başlığı bekleme süresi §3.5 tablosundadır ve `deploy/caddy/Caddyfile`'da yalnız dışa aktarma yolları için ayrı bir `reverse_proxy` bloğunda `transport http` altında kurulur (`response_header_timeout`; ad bilgim tarihlidir ve ilk task'ta belgeye karşı doğrulanır — doğrulanmazsa değer aynı kalır, ad düzelir).

   ```csharp
   // Dosya yalnız uygulamanın kullanıcısına açılır; kapanınca silinir. UnixCreateMode: .NET 7'den — bilgim tarihli.
   await using var file = new FileStream(path, new FileStreamOptions
   {
       Mode = FileMode.CreateNew, Access = FileAccess.ReadWrite, Share = FileShare.None,
       Options = FileOptions.DeleteOnClose | FileOptions.Asynchronous | FileOptions.SequentialScan,
       UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite,
   });
   ```
8. **Çökme artığı.** Süreç akış kapanmadan düşerse dosya kalır. Uygulama açılırken dizindeki her dosya silinir (tek örnek: yarıda iş yoktur) ve her dışa aktarma isteği başlarken §3.5'teki süpürme yaşından eski dosyalar silinir. Süpürme hatası isteği düşürmez, kayda yazılır (§18, yazım 5). Süpürmenin yeri `Infrastructure/ExportTempDirectory`'dir; üç dışa aktarma da dizini yalnız onun üzerinden açar. Süpürme gecelik temizliğin envanterine (`05 §5.2.9`) girmez: yeni iş yoktur.
9. **Test.** Birim: formül örnekleri (`=1+1`, `+x`, `-x`, `@x`, sekme, CR başında tek tırnak, sonra tırnaklama); sayısal sütunda negatif tutar ön eksiz (`-12,50`); `;`, `"`, CR, LF içeren hücre; BOM ve `\r\n`; `Amount`, `Rate`, `Date`, `Moment` biçimleri ve Türkiye günü (21:00 UTC bir sonraki gün); `null` boş hücre; kargo KDV dökümü biçimi; `CsvEnumLabels`'ın her `PaymentMethod` ve `Carrier` değerini kapsaması; kaynak taraması (`ToList` ve `ToArray` dışa aktarma kodunda yok). Entegrasyon (§19.1): imha edilmiş siparişte boş sütunlar ve dolu tutarlar; gönderimden ve hatadan sonra dizinde dosya kalmaması; açılış süpürmesi (tüm dosyalar) ve istek süpürmesi (yalnız eski dosyalar); tavanın aşılması (kısa tavan ayarlanamadığından sahte saat ve sahte sayaçla: süre için `TimeProvider` benzeri bir zaman kaynağı, boyut için küçük bir sınır verilen yazıcı) — dosya silinir, iz yazılmaz, 500; Excel'de açılış elle denenir (madde 2).

*Kaynak: K-1342 · K-1321 · K-1217 (3), K-1193, K-1195 · `07 §9.24.2`–§9.24.4 · `05 §6.8.4`, §7.6.3, §9.3.3, §5.2.9 · `02 §10.7.1`, §10.7.4, §3.20.6 · `06 §4.17`, §4.18, §4.19, §4.32, §4.33, §4.36, §4.57, §9.2.1 · §3.5, §4.1, §5.2, §7.1, §7.2, §8.1, §9.2, §10.5, §10.7, §18.*

## 11. Entegrasyon kuralları

Bu bölüm üç dış servisin bağlantı katmanının **kodda nasıl yazıldığını** söyler; servisin biçimi (istek, imza, yanıt kodu) `08`'dedir ve burada kopyalanmaz. Alt bölümler: 11.1 bağlantı katmanının kalıbı · 11.2 dış servis istemcileri · 11.3 Google kimlik belirteci · 11.4 e-posta ve belge metninin üretimi. Süre ve tavan değerleri §3.5'in tablosundadır, kütüphaneler §3'ün Kütüphaneler tablosundadır; bu bölüm onlara atıf yapar.

### 11.1 Bağlantı katmanının kalıbı

(`08 §3.1`, §3.2; `05 §3.6.1`–§3.6.3, §10.1.1; K-1257, K-1258, K-1346.)

1. **Servis başına bir arayüz, `Infrastructure/Integrations/` altında:** `IPaymentGateway` (ödemeyi başlat, ödemenin sonucunu sor, kart iadesi iste, kart iadesinin sonucunu sor, gelen bildirimi doğrula), `IEmailGateway` (iletiyi gönder, geri dönen adresleri sor), `IGoogleIdentityGateway` (kimliği doğrula). Arayüz ve sonuç tipleri sağlayıcının adını taşımaz; uygulamanın adı taşıyabilir (`PayTrPaymentGateway`, `SesEmailGateway`, `GoogleIdentityGateway`) — sağlayıcının adı yalnız ayar anahtarlarında ve katmanın kendi adlarında geçer (`08 §3.1`, madde 1). Modül arayüzü kurucudan alır; sağlayıcıya özgü tip, istek ve yanıt katmandan dışarı çıkmaz. Ortak altyapı modül türlerine başvurmaz (§6, madde 2): arayüzün parametreleri ve sonuçları katmanın kendi basit tipleridir; ürün sonucunu (sipariş iptali, "ödeme yöntemi kullanılamıyor" süresi) çağıran modül verir (`08 §3.1`, madde 4).
2. **Sonuç sınıfları dönüş tipidir; hiçbiri istisna olarak sızmaz.** Her iş kapalı bir kayıt ailesi döndürür (soyut `record` ve `sealed` türevleri); çağıran `switch` ifadesiyle ele alır ve kapsanmayan sınıf derleme uyarısıdır (`TreatWarningsAsErrors`, §3.1).

   | Arayüz işi | Sonuç tipi ve sınıfları (`08 §3.1` tablosu) |
   |---|---|
   | Ödemeyi başlat | `PaymentStartResult`: `Started`, `ConnectionFailed`, `SecondTokenRejected` |
   | Ödemenin sonucunu sor | `PaymentQueryResult`: `Paid`, `NoPayment`, `NoResponse` |
   | Kart iadesi iste | `RefundRequestResult`: `Accepted`, `NotCompleted`, `Unknown` |
   | Kart iadesinin sonucunu sor | `RefundQueryResult`: `Completed`, `NoResult` |
   | Gelen bildirimi doğrula | `NotificationVerification`: `SignatureValid`, `SignatureInvalid` |
   | İletiyi gönder | `EmailSendResult`: `Sent`, `RecipientError`, `ChannelError` |
   | Geri dönen adresleri sor | `BounceQueryResult`: `List`, `QueryFailed` |
   | Kimliği doğrula | `GoogleIdentityResult`: `Succeeded`, `Failed`, `Unavailable` |

   ```csharp
   // Infrastructure/Integrations/EmailSendResult.cs — kalıp: kapalı aileyi ele alan çağıranın switch'i tam olur.
   public abstract record EmailSendResult
   {
       public sealed record Sent : EmailSendResult;
       public sealed record RecipientError(string ReasonCode) : EmailSendResult;
       public sealed record ChannelError(string ReasonCode) : EmailSendResult;
   }
   ```
   Sonuç adları `08 §3.1`'in sınıflarının İngilizce karşılığıdır; sınıf eklemek önce `08`'e girer. Alan adlarıyla sınıfın kendi alanları aynı yerde yazılır: `ReasonCode` sağlayıcının hata kodudur, istek alanı taşımaz.
3. **Eşleme saf işlevdir ve tek yerdedir.** Servisin yanıt kodunu ya da özel durumunu sonuç sınıfına çeviren işlev girdisini alır ve yan etkisiz döner (SES için §11.2, madde 4). **Tabloda olmayan her yanıt** ödemede bağlantı hatası, e-postada kanal hatası, Google'da başarısız sınıfına eşlenir ve hata nedeniyle kayda yazılır (`08 §3.1`; DV-12, DV-23). Eşleme tablosunun her satırı bir birim testidir; tabloda olmayan girdi için ayrı bir test vardır.
4. **Süre çağrıdadır, yeniden deneme yoktur.** Her çağrının iptal kaynağına süre `CancelAfter` ile verilir; süre `Infrastructure/ExternalCallTimeouts` sınıfındaki iki sabittir: `UserWaiting` (kullanıcının beklediği çağrı) ve `Job` (iş kuyruğundan giden çağrı); değerler §3.5'in tablosundadır. Aşım **zaman aşımı sonucudur**: para gönderen çağrıda "sonucu bilinmeyen", e-postada kanal hatası (`08 §3.1`, madde 3). Katman kendiliğinden yeniden denemez ve `Polly` ya da dayanıklılık işleyicisi eklenmez (§11.2, madde 3); yeniden deneme iş türünün kuralıdır (§13).
5. **İmza ve tutar doğrulaması işleyiciden önce koşar.** Ödeme bildirimi ucu (UC-80) gövdeyi okur ve ilk işi `IPaymentGateway`'in doğrulamasını çağırmaktır; ondan önce kayıt yazılmaz, iş satırı açılmaz. Hesaplanan imza gelenle `CryptographicOperations.FixedTimeEquals` ile karşılaştırılır (sabit zamanlı; bilgim tarihli). İmzasız ya da imzası tutmayan istek 400 ve gövdesiz yanıt alır, uygulama kaydına güvenlik olayı olarak seyreltilerek yazılır (`08 §3.2`; `07 §4.30.1`). Kaydedilemeyen bildirim "alındı" demez (500). Tutarın karşılaştırması tam sayı kuruş üzerindedir; dönüşümün kalıbı §14.2'dedir.
6. **Yanıtta okunmayan alan okunmaz ve kayda girmez.** Her serviste okunan alanları `08`'in alan eşlemesi sayar; katman yalnız onları yanıt tipine alır (diğer alanlar için sınıf alanı açılmaz). Hata kaydına yanıtın hata nedeni (`reason`, hata kodu) yazılır; istek alanlarının değeri, adresler ve anahtarlar yazılmaz (§18.2; `05 §9.1`). Kart ve kişisel veri sayılabilecek alan okunmaz.
7. **Öteki kurallar.** Kargo takip numarası bağlantı kalıbına yüzde kodlanarak girer (§11.4, madde 2). Yerli üretim logosu ürünle gelen dosyadır (`08 §4.3`); kod onu ayardan okumaz, derlemeye gömülü kaynak olarak okur.
8. **Test.** Her arayüzün sahte uygulaması elle yazılmış durumlu bir sahtedir (§3.3; kütüphanesiz karar); modül testleri sonuç ailesinin her sınıfı için ayrı bir durum koşar. Katman testleri: eşleme tablosu, süre (sahte işleyiciyle aşım sonucu), yeniden denemenin olmayışı (çağrı sayısı bir) ve imza (tutan, tutmayan, imzasız).

### 11.2 Dış servis istemcileri

(`08 §2.2.1`, §2.2.2; P7.1; K-1322.) Kütüphane §3'ün Kütüphaneler tablosundadır (`AWSSDK.SimpleEmailV2`); değerler §3.5'tedir.

1. **SES'e AWS'nin resmî .NET istemcisiyle gidilir.** İstemci tekildir (ömrü uygulama kadar; SDK istemcisi iş parçacığı güvenlidir — bilgim tarihli) ve `SesEmailGateway` onu kurucudan alır. Yapılandırma: yeniden deneme kapalı (`MaxErrorRetry = 0`), süre 30 saniye ve çağrıya ayrıca 30 saniyelik iptal belirteci (§3.5), bölge kurulum ayarındaki bölge.

   ```csharp
   // Infrastructure/Integrations/SesClientFactory.cs — kalıp. Sınıf adı bilgim tarihli — ilk task'ta doğrulanır.
   var config = new AmazonSimpleEmailServiceV2Config
   {
       RegionEndpoint = RegionEndpoint.GetBySystemName(options.Region),
       MaxErrorRetry = 0,
       Timeout = ExternalCallTimeouts.Job,
   };
   var credentials = new BasicAWSCredentials(options.AccessKeyId, options.SecretAccessKey);
   return new AmazonSimpleEmailServiceV2Client(credentials, config);
   ```
2. **Kimlik açıktır.** Kurulum ayarındaki erişim anahtarı kimliği ve gizli anahtar istemciye doğrudan verilir (`08 §2.2.1`); SDK'nın ortam değişkeni, profil ve örnek rolü zinciri kullanılmaz — sunucuda başka bir kimlik varsa sessizce o kullanılırdı. Parçadaki `BasicAWSCredentials` zincirin dışındadır.
3. **Öteki dış servisler `IHttpClientFactory` ile gider.** Servis başına bir adlandırılmış istemci (ödeme sağlayıcısı, Google); taban adres ve sabit başlıklar kurulumda verilir, sır istemciye kurulurken değil çağrıda verilir. `HttpClient.Timeout` sonsuzdur (`Timeout.InfiniteTimeSpan`); süre çağrı başına bağlantı katmanında kurulur (§11.1, madde 4). İşleyici: otomatik yönlendirme kapalı (`AllowAutoRedirect = false`), çerez kapsayıcısı kapalı (`UseCookies = false`). **Yeniden deneme ya da dayanıklılık işleyicisi eklenmez** (`Microsoft.Extensions.Http.Resilience`, `Polly`): yeniden deneme gönderim kaydının işidir (`05 §4.4.3`, §10.1.1).
4. **SES özel durumları katmanda çevrilir.** SDK'nın özel durumları `08 §2.2.2`'nin hata eşlemesi tablosuna çevrilir ve hiçbiri dışarı sızmaz (P7.1): tabloda olmayan her özel durum, zaman aşımı ve bağlantı hatası kanal hatasıdır. Eşleme özel durum türünü ve hata kodunu alan saf bir işlevdir (§11.1, madde 3). Uygulama kaydına yalnız AWS hata kodu ve sınıf yazılır.
5. **Yasak ve kayıt.** `new HttpClient()` yazılmaz (mimari test: `HttpClient` yalnız fabrikadan gelir; §19). `System.Net.Http.HttpClient` kayıt kategorileri uyarı düzeyinde kalır (§18.2) ve istek gövdesi ile başlıkları hiçbir kayıtta yazılmaz.
6. **Test.** Özel durum → sınıf eşlemesi (tablonun her satırı); süre ve yeniden denemenin kapalı oluşu (sahte işleyiciyle çağrı sayısı bir); ortam değişkeni verilse de ayardaki kimliğin gidişi.

### 11.3 Google kimlik belirteci

(`08 §2.3`; P7.2; K-1245, K-1272, K-1281, K-1292, K-1346.) Kütüphane Kütüphaneler tablosundadır (`Microsoft.IdentityModel.Protocols.OpenIdConnect`, `Microsoft.IdentityModel.JsonWebTokens`); çatının kendi OpenID Connect işleyicisi (`Microsoft.AspNetCore.Authentication.OpenIdConnect`) kullanılmaz — akış çerezini, `state`'i ve dönüşü `07 §4.22.3` yazar (`05 §6.7.1`).

1. **Adresler sabittir.** Kod değişimi (`https://oauth2.googleapis.com/token`) ve yetkilendirme adresleri `Infrastructure/Integrations/GoogleEndpoints` içinde sabittir, keşiften okunmaz. Keşif belgesinden yalnız `issuer` ve `jwks_uri` alınır (`08 §2.3`).
2. **Anahtar kümesi önbelleğe alınır.** `ConfigurationManager<OpenIdConnectConfiguration>` tekil kaydedilir (keşif adresi `https://accounts.google.com/.well-known/openid-configuration`, `HttpDocumentRetriever` ile; çağrı süresi §3.5); her girişte yeniden çekilmez. Tazeleme aralıkları ve bilinmeyen `kid` için istekle yenileme çatının davranışıdır — varsayılanlarına dayanılır ve ilk bağlanan task'ta doğrulanır (bilgim tarihli; `08` DV-31). Çatı keşif belgesini başlatmada çekiyorsa sabit adreslerle yapılandırılır.
3. **Doğrulama parametreleri:** yalnız `RS256` (`ValidAlgorithms`), imza zorunlu (`RequireSignedTokens`), `ValidIssuers` iki biçim (`https://accounts.google.com`, `accounts.google.com`), `ValidAudience` istemci kimliği, ömür doğrulanır ve `ClockSkew` §3.5'teki 60 saniyedir. **`auth_time` toleranssız** karşılaştırılır; karşılaştırma bu katmanın değil çekirdeğin işidir (`08 §3.1`, madde 4): katman `auth_time`'ı ve `nonce`'ı sonuca alanlar olarak koyar.
4. **Sonuç.** `GoogleIdentityResult.Succeeded` yalnız `08 §2.3`'ün alan eşlemesinin saydıklarını taşır: `Email`, `EmailVerified`, `Name` (boş olabilir), `HostedDomain` (`hd`; yalnız okunur) ve `AuthTime` (yalnız yeniden doğrulamada). **`sub` sonuca alınmaz, saklanmaz ve kayda yazılmaz** (K-1061); `userinfo` çağrılmaz. `email` yoksa ya da boşsa doğrulama tutmaz. Kod değişimi bağlanamaz, 15 saniyelik aşım ya da Google hata yanıtı verirse `Unavailable`; doğrulama tutmazsa `Failed`. Yetki kodu tek kullanımlıktır — başarısız değişimin kodu yeniden gönderilmez.
5. **Test.** Sahte anahtar kümesiyle her satır: geçerli belirteç, `alg` farklı, `kid` bilinmeyen, `iss` ya da `aud` yanlış, süresi geçmiş, `exp` 60 saniye sapma içinde, `email` yok, `nonce` eşitsiz (çekirdek testi); keşif adreslerinin sabit oluşu (ağa çıkmadan).

### 11.4 E-posta ve belge metninin üretimi

(`08 §2.2.4`–§2.2.8; P7.3, P5.6, P6.11; K-1027, K-1323, K-1346.) `HtmlRenderer` çatıyla gelir; ek paket yoktur.

1. **Araç ve içerik modeli.** HTML parçası Razor bileşenleriyle, çatının dışarıda görüntüleme aracı (`HtmlRenderer`) üzerinden üretilir. Her e-posta türü tipli bir **içerik modeli** üretir — blok dizisi: başlık, paragraf, anahtar–değer satırı, düğme, yasal metin bloğu —; çerçeve (başlık, 600 piksel tek sütun, altbilgi, logo) tek bileşendir ve `08 §2.2.5`'in kalıbını tek yerde taşır. İki yazıcı aynı modeli işler: HTML yazıcısı (Razor) ve düz metin yazıcısı (bağlantılar tam adresle). "Düz metin aynı içeriğin eşdeğeridir" (`08 §2.2.5`, madde 1) böylece yapısaldır. `HtmlRenderer`'ın kuruluşu (bağımlılık enjeksiyonu, günlük, render tamamlanma beklemesi) ilk task'ta belgeye karşı sınanır (bilgim tarihli).
2. **Kaçış gösterim kanalındadır.** HTML'de Razor'un kodlaması (otomatik; öznitelik değeri de). `MarkupString`, `Html.Raw` ve `HtmlString` e-posta ve belge ad alanlarında yasaktır (mimari test). Düz metin yazıcısı değerdeki satır sonu karakterlerini boşluğa çevirir. Bağlantı adresi bir `Uri` olarak kurulur; takip numarası adrese `Uri.EscapeDataString` ile girer (`08 §2.4`). Konu satırı ve gönderen adı `08 §2.2.2`'nin arındırmasından geçer.
3. **Donmuş kopya kaçışsız ham düz metindir.** Ön Bilgilendirme Formu ve Mesafeli Satış Sözleşmesi'nin kopyası siparişte `text` olarak durur (`06 §4.17`): kaçış saklanmaz, HTML etiketi saklanmaz. Müşterinin kişisel alanları yer tutucu belirteçtir (`{{Ad}}`, §9.3); ürün adı, firma kimliği ve IBAN gibi donan değerler kopyaya ham yazılır. Sabit metinde başka amaçla çift küme ayracı yoktur — şablon testi sınar.
4. **Doldurma tek işlevdir:** `PlaceholderFiller.Fill` (§9.3, madde 3); sipariş sayfası, panel, B-1'in gövdesi ve panelden yeniden gönderim onu çağırır (K-1027). Çıktı düz metindir; kanal onu ayrıca işler: HTML'de Razor kodlar — boş satırla ayrılmış bloklar paragraf, tek satır sonu satır sonu etiketidir —, düz metin parçasında olduğu gibi gider, sayfada Razor kodlar.
5. **Şablon sürüm kimliği (`LegalTextVersion.TemplateVersion`).** İki belge şablonunun — ürünle gelen, derlemeye gömülü sabit metin dosyaları — içeriğinin SHA-256 özetinin ilk on iki onaltılık hanesidir; açılışta hesaplanır ve türün son yayımlanmış satırınınkiyle karşılaştırılır (`06 §4.56`). Şablon değişince kimlik kendiliğinden değişir; elle artırılan sürüm sabiti yoktur. Biçimlendirme kodu kimliğe girmez: kopya donmuştur, yeniden üretilmez.
6. **Üretilen yasal metnin yeni sürümünü yazan işlevin evi** (`05 §7.3.5`, `06 §4.56`; K-1338, K-1346). Ev `Settings/LegalTextGenerator`'dır (`internal`; `CompanySettingsWriter` ile aynı klasör, §10.3). Yazım iki yerden çağrılır: firma kimliğinin ve metni besleyen ayar bölümünün kaydı ile uygulama açılışı (şablon kimliği değişmişse). Sıra §10.3'ündür: ayar satırının kilidi alınır, **metin kilit altında, güncel değerlerden** üretilir (`PlaceholderFiller.Fill`; firma değerleri doldurulur, siparişin belirteçleri yerinde kalır — `requireAll: false`), türün son yayımlanmış satırının `Text`'iyle karşılaştırılır; aynıysa yeni satır yazılmaz, farklıysa `PublishLegalText` (§9.2) yeni sürümü ekler ve `TemplateVersion` o anki şablon kimliğidir. Metin kilidin dışında üretilip içeri taşınmaz. Test: aynı değerlerle iki kayıt tek satır bırakır; firma kimliği değişince yeni satır ve artan `VersionNumber`; iki eşzamanlı kayıt sıralanır.
7. **Test.** HTML özel karakteri ve etiket içeren ürün adı, alıcı adı ve iade sebebiyle her kalıbın HTML parçası kaçışlı çıkar, düz metin parçasında satır sonu ve etiket yoktur; yer tutucu değerinde belirteç biçimli dize genişlemez; her e-posta türü için iki parça aynı modelden gelir (içerik eşdeğerliği: aynı bağlantı ve aynı değerler); sipariş sayfası ve B-1 aynı doldurma işlevinden aynı metni üretir; şablon dosyası değişince kimlik değişir.

*Kaynak: K-1257, K-1258, K-1322 · K-1245, K-1272, K-1281, K-1292 · K-1027, K-1271, K-1323 · K-1338, K-1346 · `08 §2.2.1`–§2.2.8, §2.3, §2.4, §3.1, §3.2, §4.3 · `05 §3.6.1`–§3.6.3, §4.4.3, §7.3.5, §10.1.1 · `06 §4.17`, §4.56 · `07 §4.22.3`, §4.30.1 · §3.5, §6, §9.2, §9.3, §10.3, §14.2.*

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
