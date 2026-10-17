# Shopfolio — Coding Guidelines

**Versiyon: v0.1** | **Bağımlılıklar:** `02`, `04`, `05`, `06`, `07`, `08`, `10_MVP_SCOPE.md` | **Son güncelleme:** YYYY-AA-GG

> **Aşama:** 8 — Kodlama Kılavuzu · **Rol:** Tech Lead
> **Amaç:** Ajanın tutarlı kod üretmesi. Bu doküman **her kodlama görevinde** ajana verilir.
> **Uyarı:** Bu doküman diğerlerinden zordur — hem çok sayıda dokümanla tutarlılık hem implementasyon düzeyi detay ister; kalite döngüsü genelde daha uzun sürer. Burada eklenen her yeni alan/parametre `06`'da tanımlı mı diye **kontrol edilmelidir**.

> **Aşama 1'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36.
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-17 · K-977 · K-988 · Terim sözlüğü:** `02 §1` sözlüğü **birebir devralınır**; sembol, dosya ve tip adları sözlüğün İngilizce karşılıklarıyla — kapalı liste değerleri ve biçimini Teknik Mimari'nin kurduğu teknik kavramlar Veri Modeli'nin (`06`) verdiği adlarla (K-531, K-977, K-988; satır Aşama 4'ün çakışma taramasında ve checkpoint'inde hizalandı) — üretilir. Eş anlamlı ad kullanımı yasaktır — bir kavramın tek adı vardır.

> **Aşama 4'ten park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §12.3 TM0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-866 · K-867 · Dil, çatı ve kütüphane ekleme:** kod C# ile, .NET 10 (LTS), ASP.NET Core ve Entity Framework Core üzerinde yazılır; ürün yalnız LTS sürümlerinde yürür; eklenen her kütüphanenin lisansı ücretsiz ve ticari kullanıma açıktır ve bakımı süren bir projedir (Teknik Mimari `05 §2`). Ölçüt güncellemede de uygulanır — ana sürüm önerisi ve bakım sürümünün kütüphane güncellemesi lisans değişikliğine bakılarak alınır (`05 §8.8.2`, §8.8.4); koşullu lisanslı (gelir eşikli ya da açık kaynak şartlı) bir kütüphane alınırsa koşulun bu projede neden sağlandığı yazılır — repo MIT lisanslıdır. C# kod standartları, kütüphane eklemenin işletimi ve koşullu lisanslı kütüphanenin kabulü bu aşamanın kararıdır.
> - **K-868 · K-869 · Sayfa ve ön yüz düzeni:** sayfalar Razor Pages ile sunucuda üretilir, form gönderimi gönder–yönlendir–göster düzenindedir; temel akışlar JavaScript kapalıyken de tamamlanır; ön yüz çatısı ve npm derleme hattı yoktur — düz JavaScript modülleri ve düz CSS; yalnız tarayıcı tabanının (son iki büyük sürüm, KP-70) desteklediği özellikler kullanılır (`05 §3.8.4`); üçüncü taraf betik ve yazı tipi sunucusu kullanılmaz (`05 §2`). Dosya düzeni ve adlandırma bu aşamanın kararıdır.
> - **K-871 · K-881 · K-887 · K-932 · K-980 · Modül sınırları, dosya deposu ve önbellek:** ürün tek dağıtılabilir modüler monolittir ve dokuz modüldür (`05 §3.3`); bir modül öteki modülün verisine yalnız onun açık arayüzünden erişir — yazma da okuma da —, çağrı çağıranın veritabanı işleminin içindedir ve uygulama içi olay yayını yoktur (`05 §3.4`); dosyalara bir dosya deposu arayüzü üzerinden erişilir; önbellek uygulamanın belleğindedir ve ilgili kayıt kaydedildiğinde geçersiz kılınır (`05 §3`, §7) — geçersiz kılma işlem kesinleştikten sonra yapılır ve araya giren eski okumanın önbelleğe yazılmasını bir kuşak sayacı önler; zamana bağlı görünürlük (duyurunun aralığı gibi) önbellekteki kayda karşı her istekte hesaplanır; sipariş onayının işlemi satış kapısının ve tutarı etkileyen ayarların değerlerini önbellekten değil veritabanından okur (`05 §7.10.1`; K-980). Sınırların kodda nasıl korunduğu, arayüzün kullanımı ve geçersiz kılmanın ve kuşak sayacının kalıbı bu aşamanın kararıdır.
> - **K-882 · K-973 · Görüntü işleme kütüphanesi:** yüklenen görsel sunucuda yeniden kodlanır — önce yön bilgisi piksellere uygulanır, ardından meta veri (EXIF, XMP, IPTC) açıkça atılır; çözülen görselden kendiliğinden düşmez —, hareketli görsel ilk karesiyle kodlanır — çözme de ilk kareyle sınırlanır, kare sayısı belleği çoğaltmaz (`05 §6.6.2`) — ve WebP türevleri üretilir (`05 §7.7.2`); kütüphanenin lisansı ücretsiz ve ticari kullanıma açıktır ve bu görevleri — boyutu çözmeden okumak (piksel tavanı), yönü uygulamak, meta veriyi atmak, hareketli kaynakta çözmeyi ilk kareyle sınırlamak — taşır (`05 §6.6.2`). Kütüphanenin seçimi ve ilk kare sınırının kütüphanede uygulanışı bu aşamanın kararıdır.
> - **K-889 · K-891 · K-895 · K-946 · Geçiş birimi, iş kuyruğu ve zaman:** siparişin durum alanlarına yalnız geçiş birimi yazar — yöneticinin düzeltme geçişleri de onun adı olan işlemleridir, müşteri akışının beyaz listesine girmez (`05 §4.1.1`); dış yan etkiler iş kuyruğuna yazılır ve işleyiciler aynı satırı ikinci kez gördüğünde sonucu değiştirmez; kuyruğun çalıştırıcısı satırları sırayla işler — işlem bitince uyandırılır, ayrıca beş saniyede bir kuyruğa bakar — ve bir satırın hatasıyla durmaz, döngünün kendisi düşerse süreç durur ve konteyner yeniden başlar — arka plan hizmetinin hatasını yok sayan ayar kullanılmaz; kuralı yazılmamış iş türünde ve işleyicinin kod hatasında satır 1, 5, 15, 60 ve 240 dakika sonra, en çok beş kez yeniden denenir, sonra "başarısız" olur (`05 §4.3.2`, §4.3.4); nabız işlerden ayrı bir döngüdedir ve gecelik iş son başarılı çalışmasının anıyla başlar — başarısız biterse en erken bir saat sonra yeniden denenir (`05 §5.1.1`, §5.4.1); "başarısız" iş satırı kendiliğinden silinmez, yönetim komutuyla yeniden kuyruğa alınır ya da kapatılır (`05 §8.5.9`); zaman tek bir kaynaktan okunur (Teknik Mimari `05 §4`, §5). Kuralların kodda nasıl korunduğu — mimari testler dahil —, zamanın sınanabilirliği ve veritabanı oturumunun kilit bekleme, sorgu ve boşta işlem zaman aşımlarının değerleri — aşım `05 §10.3`'ün hâllerine düşer — bu aşamanın kararıdır.
> - **K-899 · K-900 · K-901 · K-903 · K-909 · K-912 · K-942 · K-972 · K-973 · Güvenlik kalıpları:** oturum sunucuda tutulur, şifre çatının özetleyicisiyle saklanır; istemci IP'si yalnız ters vekilin ilettiği başlıktan okunur — güven ters vekilin iç ağıyla tanımlanır (`05 §6.3.3`) —, IPv6'da /64; anahtarlar kriptografik rastgele üreteçle üretilir; şifre, anahtar, sır, kart verisi, IBAN, e-posta, adres ve telefon hiçbir sistem kaydına yazılmaz (`05 §9.1`), bağlantı anahtarı taşıyan adres maskelenir; yetki sunucuda, varsayılan kapalı politikayla ve kayıt düzeyinde süzülerek denetlenir; oturum anahtarı yalnız başarılı girişte üretilir; girişten ve yeniden doğrulamadan sonraki dönüş adresinin sitenin kendi yolu olduğu çatının yerel adres denetimiyle denetlenir (`05 §6.5.6`); Google ile girişin akış çerezi çatının varsayılanıyla (SameSite=None) değil Secure, HttpOnly, SameSite=Lax yazılır ve yolu `__Host-` öneki için köktür (`05 §6`, 6.7.12). Kalıpların kodda kullanımı bu aşamanın kararıdır.
> - **K-914 · K-915 · K-962 · K-974 · K-975 · K-986 · Hata yakalama ve kayıt:** beklenmeyen hata tek bir yakalama katmanında üç hâle ayrılır ve sonucu belirsiz işlemler kodda adıyla işaretlidir; uygulama kaydı yapılandırılmıştır (her satır bir JSON nesnesi), istek kimliğini erişim kaydıyla aynı değerle taşır — kimliği ters vekil üretir ve uygulamaya iletir (`05 §9.1.5`; K-986) —, hesabı iç kimlikle anar ve şifre, anahtar, sır, kart verisi, IBAN, e-posta, adres, telefon, form gövdesi ve çerez değerini hiçbir düzeyde yazmaz (Teknik Mimari `05 §9`, §10); anlık silmenin uygulama kaydındaki satırı silme işlemi diske yazıldıktan sonra yazılır ve diske inmesi beklenir — geri alınan silme satır bırakmaz, kesinleşen silmenin satırı tamponda kalmaz; satırın yazım ve diske indirme hatası çağırana döner, sessizce yutulmaz (`05 §7.4.5`); çatının ve veri erişim katmanının kayıt kategorileri uyarı düzeyindedir — kurulum ayarı yalnız uygulamanın kategorilerinin düzeyini değiştirir (`05 §9.1.6`); anlık silme satırı ve yönetim komutlarının satırları kayıt düzeyi ayarından bağımsız yazılır — ayar yalnız öteki bilgi satırlarını kapatır (`05 §9.1.6`, §7.4.5, §8.5.7; K-986); yönetim komutları uygulama kaydı satırlarını aynı dizinde kendi dosyalarına yazar — uygulamanın dosyasına ikinci bir süreç yazmaz; imha işi, okuma komutu ve silmelerin yeniden uygulanması bu dosyaları da uygulama kaydı sayar; okuma komutunun satırı okumadan önce yazılır ve diske inmesi beklenir (`05 §8.5.7`, §9.3.4; K-986). Kayıt kütüphanesi, alanların adları ve maskelemenin kodda uygulanışı bu aşamanın kararıdır.
> - **K-919 · K-920 · K-921 · Geliştirme ortamı, imaj ve testler:** geliştirme ortamı kurulumun Compose tanımının bir varyantıdır — aynı veritabanı sürümü, ödeme sağlayıcısının test ortamı ve e-postaları yakalayan yerel bir araç; uygulama imajı çok aşamalı derlenir, kök olmayan kullanıcıyla çalışır ve göç demetini taşır; birim testleri modül sınırlarının mimari testlerini içerir, entegrasyon testleri gerçek PostgreSQL 18'e karşı koşar (`05 §8`). Dosya düzeni, test projelerinin ayrımı ve araçlar bu aşamanın kararıdır.
> - **K-922 · K-923 · K-962 · K-971 · Ayarlar ve yönetim komutları:** ayarlar açılışta doğrulanır — zorunlu ayar eksikse uygulama açılmaz; ürün sabitleri kodda tek bir yerdedir ve ayar dosyası onları değiştiremez; Teknik Mimari'nin tasarım değerleri kodda sabittir — yalnız `05 §8.4.5`'in saydıkları (hız sınırı, durum ucunun eşikleri, kayıt düzeyi) varsayılanla kodda, değiştirilebilir olarak kurulum ayar dosyasındadır; kurulum tarafının işlemleri uygulama imajındaki yönetim komutlarıdır ve web'den çağrılamaz; komut süreci arka plan çalıştırıcılarını — zamanlayıcıyı, nabzı, iş kuyruğunu — başlatmaz ve kişisel veriyi — ad, e-posta — komut satırı argümanından değil etkileşimli girişten okur (`05 §8`, §8.5.1; K-971); dönüş denetimi komutu `02 §3.34.8`'in kayıtlarının tablolarını kodda tek bir listeden okur ve dönüş noktasından sonra kesinleşen işlemlerin eklediği ya da güncellediği satırları sayar (`05 §8.5.11`). Ayar sınıflarının ve komutların kodda düzeni bu aşamanın kararıdır.
> - **K-925 · K-965 · K-981 · Göçlerin yazılışı:** göçler genişlet–daralt kuralıyla yazılır — bir sürümün göçü önceki sürümün kodunu bozmaz; kural değerleri de kapsar — yeni bir durum, tür, iş türü ya da yük biçimi önce onu okuyabilen sürümle, sonra onu yazan sürümle gelir; CI'ın göç adımı son yayımlanan sürümün imajını yeni göçlerin uygulandığı şemaya karşı açar ve geri alma yolunu o sürümün entegrasyon testlerini yeni şemaya karşı koşarak sınar — canlılık ucu yalnız açılışı sınar (`05 §8.3.9`, §8.7.3; K-981); göç iz tablosunda yalnız ekler — var olan satırın değerini değiştirmez, sütun kaldırmaz, ad değiştirmez (`05 §7.5.2`); yama sürümü göç taşımaz — tek istisnası veri bozan bir göçün düzeltmesidir (`05 §8.7.4`); büyük veri dönüşümleri göçte değil, kuyruktaki bir arka plan işiyle parça parça yapılır (`05 §8`). Her göç kendi veritabanı işlemindedir — sürümün göçlerinden biri yarıda kalırsa öncekiler kalır ve eski sürüm ara şemayla çalışır (`05 §8.7.2`); bu yüzden göç işlem dışında çalışmak zorunda olan bir adım — kilitlemeden dizin kurma gibi — içermez; hacim tabanında olağan dizin kurma işlemin içinde kısa bir kilitle biter (`05 §8.7.2`). Kuralın kodda ve incelemede nasıl denetlendiği bu aşamanın kararıdır.
> - **K-934 · Kuruşa yuvarlama:** tutarlar kodda tek bir yuvarlama işlevinden yuvarlanır ve yön açıkça yazılır — tek bir tutar en yakın kuruşa, tam yarım kuruş yukarı; parçalara orantılı bölünen tutarda parçalar aşağı, kalan kuruş en büyük parçaya; işleme giren adetlerin payı `02 §7.1.7`'nin kuralıyla (`02 §3.8.3`; `05 §7.2.6`). .NET'in varsayılan yuvarlaması tam yarımı çift sayıya götürür ve bu kurala uymaz. İşlevin yeri ve sınanması bu aşamanın kararıdır.
> - **K-935 · K-936 · K-941 · K-942 · K-973 · Form ve site güvenliği, dış çağrılar:** vitrinin ve panelin bütün POST istekleri çatının sahteciliğe karşı korumasından varsayılan olarak geçer — varsayılan, tarayıcıdan POST alan her ucun sayfa işleyicisi olarak ya da doğrulama başarısızken isteği kesen genel bir doğrulama süzgecinin altında yazılmasıyla sağlanır — yalnız işaretleyen ara katman yetmez —, parçalı yüklemede doğrulama değeri istek başlığındadır; istisna imzalı makine uçlarıdır — bugün ödeme sağlayıcısının bildirim ucu — ve, sağlayıcı dönüşü POST'la yapıyorsa, durum yazmayan ödeme dönüşü ucu (`05 §6.5.1`); firmanın uzun metinleri `02 §3.11.6`'nın kapalı biçim setinin izinli etiket listesiyle süzülür ve bağlantılar yalnız `https`, `http` ve `mailto` taşır; her yanıt içerik güvenlik politikasını taşır — sunucunun yazdığı satır içi stil bloğu (marka rengi) her yanıtta üretilen tek kullanımlık değerle izinlidir — değer yalnız bloğu kapsar, renk bir öğenin stil niteliğiyle yazılmaz (`05 §6.5.4`) —, yapılandırılmış verinin veri bloğu izin gerektirmez; form gönderimi ve onu izleyen yönlendirme yalnız sitenin kendisine, ödeme sağlayıcısının adresine ve Google'ın yetkilendirme adresine gider; bağlantı katmanının her dış çağrısı sürelidir (kullanıcının beklediği çağrıda 15, kuyruktaki çağrıda 30 saniye) ve HTTP katmanında kendiliğinden yeniden deneme yoktur (Teknik Mimari `05 §6.5`, §10.1). Süzgecin kütüphanesi, sahteciliğe karşı korumanın sayfa işleyicisi olmayan uçlarda kurulması, politikanın ve tek kullanımlık değerin kodda üretilmesi ve dış çağrıların kalıbı bu aşamanın kararıdır.
> - **K-937 · K-962 · K-963 · K-964 · Veritabanı imajı ve yedek araçları:** veritabanı imajı projede derlenir — resmî PostgreSQL 18 imajının üstüne pgBackRest ve gecelik tam yedeği başlatan hafif bir zamanlayıcı eklenir; gecelik tam yedek sunucunun dışındaki depoya depo açıkça seçilerek alınır (`05 §7.6.1`); imaj veritabanının işlemlerin kesinleşme anını tutmasını açar (`track_commit_timestamp` — dönüş denetimi, `05 §8.5.11`); pgBackRest'in resmî konteyner imajı yoktur (bilgim tarihli, D-01'de doğrulanır); tam yedeğin sonucu yerel soketle veritabanındaki durum kaydına yazılır. Yedek aracı imajı dosya yedeği aracını ve gecelik zamanlamasını taşır; aracın ölçütü ücretsiz ve açık lisans, S3 uyumlu hedef, istemci tarafı şifreleme, süreli anlık görüntü saklama, düşen anlık görüntünün verisinin aynı çalışmada depodan silinmesi — kullanılmayan veri bırakmaz; restic'in varsayılan budaması kullanılmayan verinin bir payını bırakır, pay ayarla sıfıra çekilir (bilgim tarihli; `05 §7.6.6`; K-964) — ve veriyi okuyarak bütünlük denetimidir (`05 §2.2.9`, §7.6.6, §7.6.8) — aday restic (bilgim tarihli); sonucunu uygulamanın salt okuma okuduğu bir durum dosyasına yazar (Teknik Mimari `05 §2.2.9`, §8.2.2, §9.4.10). İki imajın derleme dosyaları, zamanlayıcının biçimi, durum dosyasının biçimi, dosya yedeği aracının seçimi ve budamasının ayarı bu aşamanın kararıdır.
> - **Dizin — Aşama 4 kararlarının bu dokümana devirleri:** karar kaydında K-855…K-979'un etki sütunlarında bu dokümanı gösteren kırk dört atıf (kırk dört karar) ve Teknik Mimari'nin (`05`) gövdesinde bu dokümana iş bırakan cümleler [`CHECKPOINT_REPORTS/PHASE4_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE4_CONFLICT_SCAN.md) §6'dadır; Aşama 4'ün atıflarının hepsi işin adını taşır. Aşama 4'ün checkpoint'inin kararları (K-980…K-988) tarama raporunda yoktur — bu dokümana bıraktıkları iş yukarıdaki satırlarda etiketiyle durur. K-977'nin satırı Aşama 1 bloğunun K-17 satırıdır. Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Önceki aşamaların dizinleri: `PHASE1_CONFLICT_SCAN.md` §6, `PHASE2_CONFLICT_SCAN.md` §6, `PHASE3_CONFLICT_SCAN.md` §6.

> **Aşama 5'ten park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §14.2, yazım turu ve kapanış sırasının 2. maddesi).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-1010 · K-1038 · İlişkilerin silme davranışı:** çatının zorunlu ilişki varsayılanı (basamaklı silme) kullanılmaz; her ilişkinin silme davranışı — sahibiyle silinen bağlı kayıtta basamaklı, geçmişi taşıyan kayda ve yönetici hesabına giden bağda boşaltma, başka her bağda kısıtlayıcı — modelde açıkça yazılır (Veri Modeli `06 §2`; Teknik Mimari `05 §7.5.1`). Kısıtlayıcı bağ veritabanında `NO ACTION`'dır — denetim deyimin sonundadır, sahibinin basamaklanmasıyla aynı deyimde silinen bağlı kayıt kısıta takılmaz; `RESTRICT` kullanılmaz (`06 §4`'ün girişi; K-1038). Kalıbı ve çatıdaki eşlenişi bu aşamanın kararıdır.
> - **K-1011 · K-1012 · K-1017 · Anahtar, adlandırma ve enum:** birincil anahtar uygulamanın ürettiği sürüm 7 UUID'dir; tablo ve sütun adları küçük harf ve alt çizgiyle, kod adından türer, tablo adı çoğuldur, dizin ve kısıt adları çatının kuralıyla verilir; enum veritabanında kod adıyla metin olarak durur ve sütun izinli değerlerin kısıtını taşır (`06 §2`). Üretimin, adlandırma kuralının — kütüphane ya da kendi kuralı, kütüphane ölçütü K-867 — ve dönüşümün çatıdaki uygulanışı bu aşamanın kararıdır.
> - **K-1013 · K-1014 · K-1015 · K-1016 · K-1040 · K-1047 · Türler, sürüm, sayaçlar ve karşılaştırma:** para `decimal`; oranların tek ölçeği iki ondalıklı yüzdedir (20 = %20); sürüm değeri açık bir tamsayı sütunudur ve kaydı yazan güncellemede artar — siparişin sürümünü artıran ve artırmayan yazımların listesi `06 §2.6.4`'te, hangi kayda yazan işlemin artırdığı `06 §4.17`'dedir — ürün ve varyantın silinmesinin kalemlere ve ayırmalara yazımı ve müşterinin indirmesi sürümü artırmaz (K-1047); ayrılmış adet ve kupon hakları yalnız geçiş biriminden yazılır; ürün adının arama biçimi ve kimlik karşılaştırmasının normalleştirilmiş sütunu — e-posta, kupon kodu, stok kodu — değeri yazan tek işlevden geçer ve normalleştirme ("I", "İ", "ı", "i" tek harf; dilden bağımsız küçük harf) tek bir işlevdedir (`06 §2`); kategorinin düzeyi kategoriyi yazan tek işlevden — taşımada taşınan alt ağacın satırlarıyla aynı işlemde — yazılır (`06 §2.7.1`, §4.9; K-1040 (4)). Kalıpların kodda uygulanışı bu aşamanın kararıdır.
> - **K-1021 · Bölüm başına sürüm:** firmanın ayarları tek satırlık `CompanySettings` kaydında bölüm bölüm sütunlardır ve her bağımsız kaydedilen bölüm kendi sürüm sütununu taşır; bölümün kaydı yalnız kendi sütunlarını ve sürümünü koşul olarak yazar, öteki bölümlerin sürümü kaydı durdurmaz (Veri Modeli `06 §4`; K-1014). Bir tabloya birden çok sürüm sütununun çatıda eşlenişi — tablonun bölüm bölüm ayrı varlık türleriyle eşlenmesi ya da başka bir kalıp — bu aşamanın kararıdır.
> - **K-1026 · Kapalı metin biçimi setinin temizlenmesi:** uzun metin — ürün açıklaması, kurumsal içeriğin uzun metinleri, aydınlatma metni ve çerez politikası — izinli etiketlerle sınırlı, yazımda sunucuda temizlenmiş HTML olarak saklanır: kalın, italik, madde listesi, bağlantı, tek düzeyli ara başlık; bağlantının adresi yalnız `http`, `https` ve `mailto` olabilir; temizleme değeri yazan tek işlevdedir ve okumada yinelenmez (Veri Modeli `06 §2`; `02 §3.11.6`). Temizleyicinin seçimi — kütüphane ölçütü K-867 — ve izin listesinin kodda tek yeri bu aşamanın kararıdır.
> - **K-1027 · Belgenin kişisel verisiz kopyası:** onay anında Ön Bilgilendirme Formu ile Mesafeli Satış Sözleşmesi sürümün metninden ve siparişin donan değerlerinden üretilir ve müşterinin kişisel alanları yer tutucu olarak kalır; sipariş sayfası, panel, B-1'in gövdesi ve panelden yeniden gönderim yer tutucuları siparişin onay anındaki değerleriyle — düzeltilmişse adres ve e-posta geçmişinin ilk eski değeriyle — doldurur (Veri Modeli `06 §4`; `02 §3.23.3`, §3.33.6). Kopyayı üreten ve yer tutucuları dolduran işlevin tek yeri ve ürünle gelen şablonun sürüm kimliği bu aşamanın kararıdır.

---

## 1. Temel ilkeler

- Somut örnekle tanımla. "Hata yönetimi yapılmalı" değil, "hata yönetimi şöyle yapılır: (somut örnek)".
- Tek kaynak, tek doğru. Bir kural iki yerde farklı anlatılmaz.

## 2. Source of truth hiyerarşisi

> **Ne yazılır:** Çelişki durumunda hangi doküman kazanır. Örn. iş kuralı → `02`; şema → `06`; sözleşme → `07`.

## 3. Genel kod yazım kuralları

## 4. Proje yapısı ve klasör organizasyonu

## 5. Adlandırma konvansiyonları

## 6. Mimari sınırlar

> **Ne yazılır:** Katman sınırları, modüller arası iletişim kuralı, iş kuralının hangi katmanda yaşayacağı.

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

*Shopfolio — Coding Guidelines v0.1*
