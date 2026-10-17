# Shopfolio — Deploy Runbook

**Son güncelleme:** YYYY-AA-GG | **Durum:** ⬚ Doldurulmayı bekliyor

> **Ne zaman doldurulur:** Entegrasyon fazından itibaren parça parça; **kapanış ritüelinde finalize edilir** (00 §L adım 5).
>
> **Bu dokümanın varlık sebebi:** Kod tamam olsa bile, prod'da bir ayar eksikse sistem ya **hiç açılmaz** ya da bir kural **sessizce devre dışı kalır**. İkinci durum tehlikelidir çünkü hiçbir test bunu yakalamaz — yalnız bu doküman yakalar.

> **Aşama 2'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §8.3 AK0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; doldurulurken karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-699 · Panele erişimin kaybında kurtarma:** hiçbir yönetici panele giremediğinde — tek yöneticinin şifresi ve e-posta kutusu kaybolmuşsa ya da ele geçirilmiş bir hesap tek yönetici kalmışsa — kurulumu yapan, ilk yöneticiyi açtığı yolla (§H; `10` ÖK-7) yeni bir yönetici hesabı açar; hesap ele geçirilmişse o hesabın oturumlarını sonlandırır ve şifresini geçersiz kılar. Ürün içinde kurtarma yolu yoktur (`02 §10.2.7`, `03 §10.4`). Adımların kendisi, kurtarmayı kimin isteyebileceği ve isteğin nasıl doğrulanacağı bu dokümanın kararıdır.
> - **Dizin — Aşama 2 kararlarının bu dokümana devirleri:** karar kaydında K-647…K-718'in etki sütunlarında bu dokümanı gösteren bir atıf (bir karar) ve Kullanıcı Akışları'nın (`03`) gövdesinde bu dokümana iş bırakan cümleler [`CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md) §6'dadır. Bir iş yalnız `03`'ün gövdesinde yaşar, karar satırı yoktur: ödeme sağlayıcısının anahtarlarının ya da sağlayıcının değişmesinde geçişin yöntemi (`03 §10.1.1.7`). Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Aşama 1'in dizini: `PHASE1_CONFLICT_SCAN.md` §6.

> **Aşama 3'ten park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §10.3 UI0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; doldurulurken karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-736 · İlk yöneticinin adı:** yönetici hesabı zorunlu bir ad taşır ve ilk yöneticinin adı kurulumda verilir — ilk yöneticiyi açan yol (§H) e-posta ve şifrenin yanında adı da alır; kurtarmada (K-699) açılan yönetici hesabı için de aynısı geçerlidir (`02 §10.2.1`, §10.2.4, §10.2.7; `10 §4.1` ÖK-7). Ad yönetici listesinde, işlem izinde ve yöneticilere giden bildirimlerde okunur. Ad, e-postayla birlikte yönetici açma komutunun etkileşimli girişinden okunur — komut satırına parametre olarak yazılmaz (Teknik Mimari `05 §8.5.1`, §8.5.2; K-923, K-971); §H'nin adımları bu dokümanın kararıdır (satır Aşama 3'ün çakışma taramasında açıldı, Aşama 4'ünkinde hizalandı).
> - **Dizin — Aşama 3 kararlarının bu dokümana devirleri:** karar kaydında K-723…K-850'nin etki sütunlarında bu dokümanı gösteren bir atıf (bir karar) [`CHECKPOINT_REPORTS/PHASE3_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE3_CONFLICT_SCAN.md) §6'dadır; Arayüz Tanımları'nın (`04`) gövdesinde bu dokümana iş bırakan cümle yoktur. Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Önceki aşamaların dizinleri: `PHASE1_CONFLICT_SCAN.md` §6, `PHASE2_CONFLICT_SCAN.md` §6.

> **Aşama 4'ten park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §12.3 TM0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; doldurulurken karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-863 · K-864 · K-865 · K-938 · K-949 · K-963 · Sunucunun seçimi ve kurulumu:** kurulum firmanın adına kiralanan bir sanal sunucuda konteyner olarak kurulur — tek desteklenen hedef; sunucu Linux, en az 2 sanal işlemci, 4 GB bellek, 60 GB SSD ve sabit bir genel IP adresi taşır; seçim ölçütleri: sağlayıcı diski kendi tarafında yedekli tutar, veri merkezi ve sağlayıcı Türkiye'dedir ve veriye yurt dışından erişilmez (kurulum önerisi, ürün kuralı değil — aktarım konumdan değil erişimden doğar, `05 §0.4`; yurt dışı seçimde aktarımın şartları firmanın yükümlülüğüdür ve konum aydınlatma metnine yazılır), aylık bedel tavanda yedeğe ve e-postaya yer bırakır — her güncellemenin provası nesne depolamadan veritabanı boyu kadar çıkış trafiği doğurur (K-963) —, anlık görüntü alınabilir — anlık görüntü sunucunun bütününe (işletim sistemi, yapılandırma, imajlar) hızlı geri dönüş içindir, verinin dönüş noktası değildir (`05 §0.5`, §8.7.4) (Teknik Mimari `05 §0`, §8; `10 §4.1` ÖK-2). Sunucunun ya da yedek alanının sağlayıcısı, ülkesi veya adresi değişirse — kurtarma ve taşıma dahil — kurulumu yapan firmayı uyarır: ETBİS'teki veri tabanı bilgisi otuz gün içinde güncellenir ve aydınlatma metninin barındırma bölümü düzeltilir (`02 §12.5`, §12.2.8; K-949). Adımların kendisi bu dokümanın kararıdır.
> - **K-861 · K-862 · Kurulumun aylık gider kalemleri:** tavan yaklaşık 10–20 $'dır ve sunucuyu, sunucunun dışındaki yedek alanını, e-posta gönderimini ve dış erişilebilirlik kontrolünü taşır; ödeme sağlayıcısının komisyonu ve alan adı tavanın dışındadır (K-861; `05 §0`). Kalemlerin kurulumdaki listesi bu dokümanın kararıdır.
> - **K-872 · K-972 · Kurulumun sırası:** sunucuda dört konteyner — uygulama, veritabanı, ters vekil (Caddy), yedek aracı — tek bir Compose tanımıyla kurulur; alan adı sunucuya yönlendirilmeden (`10 §4.1` ÖK-1) sertifika alınamaz (`05 §3`); ters vekil istemcinin gerçek adresini görür — ağında IPv6 açıktır ya da kullanıcı alanı vekili kapalıdır — ve kurulumda bir IPv4 ve bir IPv6 isteğinin istemci adresi ters vekilin erişim kaydında ve uygulamanın gördüğü adreste denetlenir — uygulamanın güveni ters vekilin konteyner adresine değil iç ağına tanımlanır; ikisi aynı değilse uygulama ters vekile güvenmiyordur (`05 §6.3.3`; K-972). Adımların sırası bu dokümanın kararıdır.
> - **K-870 · K-927 · Veritabanının inceleme araçları:** veritabanı PostgreSQL'dir; arızada kullanılacak inceleme ve sorgu araçları bu dokümana yazılır (`05 §2`). İnceleme salt okuma kullanıcısıyla yapılır; veri yazan işler yalnız göç, yönetim komutları ve kurtarmadır (`05 §8.9.5`, §6.x.18; K-927). Araçlar ve bağlantının adımları bu dokümanın kararıdır.
> - **K-876 · K-878 · K-879 · K-880 · K-888 · K-937 · K-939 · K-941 · K-944 · K-955 · K-961 · K-962 · K-963 · K-964 · K-966 · K-969 · K-974 · Yedek, kurtarma ve prova:** veritabanının işlem günlüğü en geç bir dakikada bir — arşiv zaman aşımı elli saniye, kalan on saniye kapanan dosyanın taşınmasının; daha uzun süren taşınma geride kalan aktarımdır (`05 §7.6.1`; K-966) —, tam yedeği her gece pgBackRest ile — veritabanı konteynerinde, arşiv komutuyla ve imajla gelen zamanlayıcıyla; tam yedeğin saati imhanın (03:00) öncesinde bitecek biçimde seçilir (`05 §5.3.2`) — sunucunun dışındaki, firmanın hesabındaki S3 uyumlu nesne depolamaya şifreli gider; görseller, dijital ürün dosyaları, kayıt dosyaları ve sunucunun kayıtları — yedek aracı dosya bölümlerini, işletim sisteminin günlük dizinini ve veritabanının ve yedek aracının kendi kayıt dosyalarını salt okuma bağlar — her gece yedek aracı konteynerindeki dosya yedeği aracıyla aynı kovaya, ayrı bir öneke şifreli gider. İki yedeğin ayrı nesne depolama anahtarı ve ayrı şifreleme anahtarı vardır; iki şifreleme anahtarı da kurulumda firmaya teslim edilir ve kurulumu yapanda da durur. Saklama her kopyada otuz gündür: pgBackRest'in saklaması zamanla, dosya yedeğinin anlık görüntüleri otuz günle ayarlanır ve düşen anlık görüntülerin verisi aynı çalışmada depodan silinir — kullanılmayan veri bırakılmaz; kovada sürüm tutma açılır ve eski sürümleri otuz günde silen yaşam döngüsü kuralı kurulur — kural tamamlanmamış çok parçalı yüklemeleri de birkaç günde durdurur (`05 §7.6.6`; K-964). Sunucudaki nesne depolama anahtarları firmanın hesabının yönetim girişinde, yalnız kendi önekine yazan, okuyan ve güncel nesneyi silen yetkiyle üretilir — eski sürümü silemez, sürüm tutmayı, nesne kilidini ve yaşam döngüsü kuralını değiştiremez, kovanın bu ayarlarını okuyabilir — provadaki kural denetimi bununla yapılır (`05 §7.6.6`, 7.6.8); sağlayıcı bu ayrımı vermiyorsa nesne kilidi uyum kipinde ve otuz günlük sürüyle açılır — o kipte koruma sürümün yazıldığı andan otuz gündür, otuz günden eski parçalar ele geçirmede silinebilir (kalan risk; `05 §7.6.6`; K-964). Sunucudan silinen ya da üzerine yazılan yedek otuz gün eski sürümlerden geri getirilir — silme işaretlerinin ve sonraki sürümlerin firmanın hesabının yönetim girişinden kaldırılmasının adımları bu dokümanın kararıdır; fark edilmeyen silme otuz günün sonunda kalıcı olur — kalan risk (`05 §7.6.6`). Veritabanının ve yedek aracının dışarıya yalnız nesne depolamaya çıkması sunucunun güvenlik duvarında hedef adresli bir kuralla sağlanır — Compose'un ağı çıkışı hedefe göre süzmez (`05 §3.2.5`). Kurtarma hedefi yeni bir sunucuda aynı gündür; kurtarma eski sunucu sağlayıcıdan durdurulduktan ve sunucudaki nesne depolama anahtarları geçersiz kılındıktan sonra başlar (`05 §7.6.7`). Gecelik tam yedek sunucunun dışındaki depoya depo açıkça seçilerek alınır ve durum ucunun gecelik yedek koşulu ona bakar; prova kurulumda ve her güncellemede sunucunun dışındaki depodan yapılır — yerel kopya provayı karşılamaz —, dosya yedeğinden de son anlık görüntünün bir örneği geri alınır ve aracın depo bütünlük denetimi çalışır (`05 §7.6.8`; K-963); sonucu buraya yazılır — prova kaydı dönülen depoyu, geri dönüşün süresini — indirme dahil — ve dönülen son anı taşır, yaşam döngüsü kuralının varlığı provada denetlenir. En son andan eski bir ana dönüş — sebebi ne olursa olsun — yalnız dönüş noktasından sonra sipariş tarafında yazım yoksa yapılır ve denetimi dönüş denetimi komutuyladır (`05 §8.7.4`, §8.5.11; K-962). Dönüşten sonra imha işi çalıştırılır ve süreye bağlı silmeleri yeniden uygular; kurulumu yapan firmayı dönüşün anıyla bilgilendirir — geri gelen anlık silmeleri (hesabın ve üye hesabının silinmesi) imha işi bulamaz — uygulama başlamadan önce kurulumu yapan silmelerin yeniden uygulanması komutunu çalıştırır: komut uygulama kaydındaki silme satırlarını (silmenin türü ve iç kimlik) dönüş anından bugüne okur ve aynı silme yoluyla yeniden uygular (K-955; `05 §7.4.5`, §8.5.10) — diskin kalıcı kaybında dönüş en son ana yapılır ve komut gerekmez; yedeğe ulaşmamış penceredeki silmeler penceredeki öteki kayıtlarla birlikte geri gelir (`05 §7.4.5`); eski bir ana dönüşte, silmenin kesinleştiği an ile satırının diske indiği an arasında süreç ya da sunucu çökmüşse ya da satırın yazımı hata vermişse o silme komutun dışında kalır ve geri gelir — kalan risk (`05 §7.4.5`; K-969, K-974) (`05 §7`; `10 §4.1` ÖK-2). Adımlar, araçların ayarları, anahtarların teslim biçimi ve prova kaydı bu dokümanın kararıdır. Sunucunun diski kalıcı olarak kaybolursa en çok son bir dakikanın kayıtları yedeğe ulaşmamış olabilir — proje sahibi bu pencereyi kabul etti (K-888); bir dakika aktarım çalışırken geçerlidir: aktarım durmuşken (nesne depolamaya ulaşılamıyor ya da nesne depolama anahtarı geçersiz; hata vermeden geride kalan aktarım da durmuş sayılır — `05 §7.6.9`; K-966) disk de kaybolursa pencere aktarımın durduğu ana kadar uzar — kalan risktir; aktarımın durması on dakikada durum ucunda uyarı koşulu olur ve dış kontrolün yoklamasıyla kurulumu yapana gelir — başka bir koşul zaten açıkken yeni uyarı gitmez (`05 §7.6.2`, §9.4.4, §9.4.11, §9.4.14; K-974); kurtarmadan sonra kurulumu yapan kurtarılan son anı firmaya bildirir ve imzası doğru ama eşleşmeyen sağlayıcı bildirimlerini gelen bildirim kaydından firmaya iletir — imzası tutmayan istek bu kayda girmez (`05 §4.5.5`); firmanın adımları `02 §3.34.8`'dedir. Dosyalarda ve sistem kayıtlarında pencere son gecelik yedektir: kurtarmadan sonra kurulumu yapan veritabanında kaydı olup diskte bulunmayan dosyaları listeler ve firmaya bildirir, firma onları yeniden yükler (`02 §3.12.7`; `05 §7.6.2`).
> - **K-896 · K-898 · Sunucu saati ve sağlık ucu:** sunucunun saati ağ zaman servisiyle (NTP) eşitlenir — bütün süreler ona bağlıdır; saatin eşitlenme durumu ve saat farkı kurulumda ve güncelleme turunun ön denetiminde okunur — ayrı bir uyarı kanalı yoktur, durum ucu saat farkını saymaz (`05 §9.4.1`, §10.1.14); uygulama sitenin genel adresindeki canlılık ucuna (`05 §9.4.1`) kendi sunucusundan ters vekil üzerinden istek yapar ve kesintiyi bununla ölçer — ters vekilin ve güvenlik duvarının bu isteği geçirmesi gerekir (`05 §5`). Adımlar ve nabız isteğinin genel adı nasıl çözdüğü — sunucunun çözümleyicisiyle mi, konteyner ağının içinden ters vekile eşlemeyle mi; çözümleyiciyle çözülürse çözümleyicinin arızası sahte bir kesinti kaydı açabilir (`05 §10.1.16`) — bu dokümanın kararıdır.
> - **K-910 · K-911 · K-937 · K-971 · K-973 · Sırlar:** sırlar sunucuda, yalnız köke açık bir dizinde (0700), bileşen başına ayrı dosyalarda durur ve yalnız onu kullanan konteynere bağlanır — Compose dosya kaynaklı sırda sahipliği ve izni değiştirmediği için her dosyanın sahibi onu okuyan konteyner kullanıcısıdır ve izni 0400'dür (`05 §6.x.1`); §B'nin envanteri — uygulama, göç, yedek aracı, veritabanı, okuma komutunun, dönüş denetiminin ve veri yazan yönetim komutlarının tek seferlik çalışması için ayrı satırlar, "Hangisi ALMAMALI" sütunu dahil — `05 §6.x`'e göre doldurulur: veritabanı satırı kurucu (süper) kullanıcının yalnız ilk kurulumda kullanılan parolasını ve veritabanı yedeğinin nesne depolama ve şifreleme anahtarlarını, yedek aracının satırı dosya yedeğininkileri taşır; salt okuma kullanıcısının parolası yalnız okuma komutunun ve dönüş denetiminin tek seferlik konteynerine ve bakımdaki incelemeye geçer, sürekli çalışan uygulamaya geçmez (`05 §6.x.18`); veri yazan yönetim komutları uygulama kullanıcısının parolasını alır, uygulamanın dış servis sırlarını almaz (`05 §6.x.19`); ağ üzerinden bağlanan bir yedek kullanıcısı ve parolası yoktur; repo özel yapılırsa her sunucunun salt okuma imaj çekme belirteci envanterde bir satırdır (`05 §8.2.5`, §6.x.8); veritabanı parolaları ve iki yedeğin nesne depolama anahtarları yılda bir ve olay üzerine, sağlayıcı anahtarları olay üzerine döner — veritabanı parolası açık metinle bir deyime yazılmadan, istemcinin parola değiştirme komutuyla döndürülür ve sızıntı şüphesinde o kullanıcının açık bağlantıları sonlandırılır (`05 §6.x.10`, §6.x.16); iki yedeğin şifreleme anahtarı döndürülmez; kurtarmada sırlar vereninden yeniden alınır ya da yeniden üretilir (`05 §6.x`). Envanter, döndürme ve kurtarma adımları bu dokümanın kararıdır.
> - **K-915 · K-916 · K-917 · K-937 · K-944 · K-975 · Sistem kayıtları:** uygulamanın ve ters vekilin kayıt dosyaları sunucuda günlük dosyalardadır ve gecelik dosya yedeğiyle nesne depolamaya da gider; altı ay saklanır — dosyaları yalnız gecelik imha işi siler, ters vekilin ve kayıt kütüphanesinin kendi saklama ve silme sınırları kapatılır; sunucunun kayıtları — işletim sisteminin günlüğü (SSH oturumları dahil), konteyner çalıştırıcısının, veritabanının ve yedek aracının kaydı — da altı ay tutulur: işletim sisteminin döngüsü altı aya ayarlanır, veritabanının ve yedek aracının kaydı konteyner çıktısına değil kendi dosyasına yazılır ve aynı süreyle tutulur — bu dosyaları da işletim sisteminin döndürme aracı altı ayda siler, veritabanının kendi döndürmesi yaşa göre silmez (`05 §9.2.2`) —, konteynerin açılış ve çökme çıktısı küçük döngüyle tutulur; veritabanının kaydı sorgu parametrelerini yazmaz ve hata kaydı anahtar değerlerini yazmayan ayrıntısız kipte tutulur (`05 §9.1.5`); sunucunun kayıtları da gecelik dosya yedeğine girer — yedek aracı işletim sisteminin günlük dizinini ve veritabanının ve yedek aracının kendi kayıt dosyalarını salt okuma bağlar; imha işi her gece sunucunun kayıtlarının altı ayı dolan gününü imha kaydına "döngüyle silindi" satırı olarak tarih aralığıyla yazar (`05 §9.2.2`); kayıtları kurulumu yapan firmanın yazılı isteğiyle okuma komutuyla okur — komutun konteynerine erişim kaydı ve sunucunun kayıtları salt okuma, uygulama kaydının dizini yazılabilir bağlanır (`05 §9.3.1`) —, sonucu CSV dosyası olarak firmaya şifreli iletir ve sunucudan siler (`05 §9`; `02 §8.5.1`). Dosyaların yeri ve döngüsü, sunucunun kayıtlarının döngü ve dosya ayarları, okuma komutunun kullanımı ve dosyanın iletim biçimi bu dokümanın kararıdır.
> - **K-918 · K-946 · K-974 · K-975 · İzleme:** kurulum başına bir dış erişilebilirlik kontrolü beş dakikada bir durum ucunu yoklar ve iki ardışık başarısızlıkta kurulumu yapana e-posta gönderir; hesap kurulumu yapanındır; konteynerler çöktüğünde ve sunucu açıldığında kendiliğinden kalkar; durum komutu uyarının hangi koşuldan geldiğini söyler (`05 §9`); dış kontrol durumun değişiminde uyarır — bir koşul açıkken açılan yeni koşul yeni e-posta doğurmaz, bu yüzden açık koşul bekletilmez (`05 §9.4.14`); kaynağı okunamayan koşul açık sayılır (`05 §9.4.10`). Durum ucunun koşulları iş kuyruğunu — zamanı gelmiş en eski satır on beş dakikadan eski ya da "başarısız" satır var — ve gecelik imhayı — son başarılı çalışması yirmi altı saatten eski — da kapsar (`05 §9.4.16`, §9.4.17); "başarısız" satırın uyarısı kendiliğinden kalkmaz — kurulumu yapan satırı durum komutuyla görür ve iş satırı komutuyla yeniden kuyruğa alır ya da kapatır (`05 §8.5.9`). Kontrolün kurulumu ve uyarı eşiklerinin §C satırları — bu iki eşik dahil — bu dokümanın kararıdır.
> - **K-919 · K-920 · K-922 · K-923 · K-943 · K-971 · Kurulumun yapısı, ayarları ve ilk kayıtlar:** referans kurulum her sürümün ilk indiği kurulumdur; kurulum repodaki sürümlü Compose tanımıyla — dört hizmet ve tek seferlik göç hizmeti — kurulur; kuruluma özgü değerler sunucudaki ayar dosyasında ve sır dosyalarındadır, veri adlandırılmış bölümlerdedir, imajlar etiket ve özetle (`imaj:sürüm@sha256:…`) çekilir — güncelleme turu referans kurulumda denetlenen imajların özetini kaydeder ve kurulumlara o özeti çeker, taban imajların özeti aylık bakım sürümünde yenilenir (`05 §8.2.3`); zorunlu ayar eksikse uygulama açılmaz (§A), koşullu ayarlar ve Teknik Mimari'nin kurulum ayar dosyasında değiştirilebilen tasarım değerleri — hız sınırı, durum ucunun eşikleri, kayıt düzeyi (`05 §8.4.5`) — varsayılanlarıyla §C'de yazılır; ilk yönetici ve kurtarmada açılan yönetici uygulama imajındaki yönetim komutuyla açılır — ad ve e-posta komut satırından değil etkileşimli girişten, şifre gizli girişle (§H; `05 §8.5.1`, §8.5.2; K-971). Adımlar bu dokümanın kararıdır.
> - **K-924 · K-925 · K-926 · K-927 · K-938 · K-956 · K-958 · K-960 · K-962 · K-965 · Güncelleme, geri alma, güvenlik güncellemeleri ve bakım erişimi:** güncelleme turu kurulumu yapanın makinesinden tek komutla, önce referans kurulumda, sonra kurulum kurulum yürür — ön denetim — boş disk yeni imajları ve provanın geri dönüşünü karşılar —, güncelleme öncesi artımlı yedek (pgBackRest, veritabanı konteynerinde), göç, yeniden başlatma, uçların denetimi, kurtarma provası ve kullanılmayan eski imajların silinmesi — bir önceki sürümün imajı geri alma için kalır (`05 §8.6.3`); geride kalmış kurulum — çalışan sürümü sunucudan, durum komutundan okunur; kurulum listesi kayıttır, ayrışırsa tur durur (`05 §8.6.3`; K-965) — aradaki her ara sürümün son yamasından sırayla geçirilir, sürüm atlanmaz: her adımda çekme, göç, yeniden başlatma ve uçların denetimi, artımlı yedek başta, kurtarma provası sonda bir kez (`05 §8.7.1`; K-960); başarısızlık turu durdurur ve kurulum önceki imaja geri alınır (§G); yedekten dönüş — göç ya da başka bir sebep veriyi bozduğunda — yalnız dönüş noktasından — göçte göçün başladığı an: tur göçten hemen önce veritabanında adlandırılmış bir dönüş noktası açar ve onu sunucunun saatiyle prova kaydına yazar (noktasal geri dönüş) — sonra sipariş tarafında hiçbir kayıt yazılmamışsa yapılır (K-962): kurulumu yapan önce uygulamayı durdurur — denetim ile dönüş arasında yazım doğmaz (K-958) —, sonra dönüş denetimi komutuyla sipariş tarafının bütün kayıtlarında — `02 §3.34.8`'in listesi: siparişler, ödemeleri ve geçiş kaydı, kalem düzeyindeki kayıtlar, iletişim talepleri, donmuş yasal metin sürümleri, onay kutularının kaydı ve işlem izi — dönüş noktasından sonra kesinleşen işlemlerin eklediği ya da güncellediği satırlara bakar (K-956, K-962), yazım varsa uygulama yeniden başlatılır ve bozulma ileriye doğru — göçle ya da yönetim komutuyla — düzeltilir, bozulmadan önceki hâl gerekiyorsa yedek geçici bir konteynerde o ana döndürülüp okunur (`05 §8.7.4`); göçün bozduğu veriden dönüldükten sonra kurulum dönüş noktasındaki sürümün imajıyla açılır, kurulum listesindeki çalışan sürüm geri yazılır ve kurulum bozan göçü düzelten yamaya kadar turdan çıkar (K-965); dönüşte öteki yazımlar geri gider, anlık silmeler silmelerin yeniden uygulanması komutuyla yeniden uygulanır ve firma dönüşün anıyla bilgilendirilir (`05 §7.4.5`, §8.5.10); sağlayıcının anlık görüntüsü güncelleme adımlarında zorunlu değildir; turun komutu yeniden çalıştırılabilir — yarıda kalan turda kurulumun hâli durum komutuyla okunur; işletim sistemi güvenlik güncellemelerini kendiliğinden alır ve gerekirse 04:30'da yeniden başlar; aylık bakım sürümü ve acil yama aynı turla gider, veritabanının ana sürüm geçişi ayrı bir sürümdür ve turdan iki noktada ayrılır — uygulama durdurulur, geçiş eski veri dizinini koruyan kopya kipindedir ve geri dönüş eski dizine ve eski veritabanı imajına dönmektir; yedek deposu yükseltilir ve yeni kümenin ilk tam yedeği uygulama açılmadan sunucunun dışındaki depoya alınır; ön denetimin disk ölçüsü veritabanının iki katıdır (`05 §8.8.5`; K-965); yayımlanan sürüm imajları kayıt defterinden silinmez (`05 §8.2.5`); sunucuya yalnız SSH ile, kurulum başına parolayla korunan anahtarla girilir, açık kapılar 80, 443 ve SSH'tır; firma kurulumu yapanın anahtarını kaldırabilir; kurulumu yapanın makinesi ve yayın hesabı bütün kurulumların ortak noktasıdır — kurulum listesi ve bütün anahtarlar o makinededir, kalan risktir; makine kaybolursa erişim firmanın sağlayıcı konsolundan yeni bir anahtarla yeniden kurulur (`05 §8`, §8.9.2). Turun komutu, kurulum listesi, adımlar, prova kaydı, makinenin ve yayın hesabının korunması ve kurulum listesinin ve anahtarların makine dışındaki yedeği bu dokümanın kararıdır.
> - **K-928 · K-929 · Tepe dakikanın okunması:** kurulumu yapan güncelleme turunda her kurulumun son ayındaki en yüksek dakikalık farklı ziyaretçi sayısını erişim kaydından okur; aynı turda sunucunun bellek ve işlemci kullanımını okur ve uzun işlerin saatlerindeki sayfa yanıt sürelerine bakar — büyüme yolunun tetikleyicileri (`05 §11.3.2`, §11.3.6); 100'ün aşılması `10 §4.2` SK-5'in eşzamanlı ziyaretçi tetikleyicisidir (`05 §11`). Okumaların komutu bu dokümanın kararıdır.
> - **Dizin — Aşama 4 kararlarının bu dokümana devirleri:** karar kaydında K-855…K-979'un etki sütunlarında bu dokümanı gösteren elli sekiz atıf (elli dört karar) ve Teknik Mimari'nin (`05`) gövdesinde bu dokümana iş bırakan cümleler [`CHECKPOINT_REPORTS/PHASE4_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE4_CONFLICT_SCAN.md) §6'dadır; Aşama 4'ün atıflarının hepsi işin adını taşır. Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Önceki aşamaların dizinleri: `PHASE1_CONFLICT_SCAN.md` §6, `PHASE2_CONFLICT_SCAN.md` §6, `PHASE3_CONFLICT_SCAN.md` §6.

---

## 0. Hızlı özet

| Soru | Cevap |
|---|---|
| Sistem hangi ayarlar olmadan **açılmaz**? | §A |
| Hangi secret'lar **zorunlu**? | §B |
| Hangi ayarlar set edilmezse bir kural **sessizce devre dışı kalır**? | §C ⚠ |
| Hangi ayarlar yeniden başlatma gerektirir? | §D |
| Hangileri çalışırken değiştirilebilir? | §E |
| Deploy sonrası ne doğrulanır? | §F |
| Geri alma | §G |
| Seed edilemeyen ilk kayıtlar (ilk yönetici vb.) nasıl oluşur? | §H |
| Gerçek konfigürasyonla boot provası | §I ⚠ |

---

## A. Zorunlu ayarlar (yoksa sistem açılmaz)

> **Kaynak:** `06_DATA_MODEL.md` §8 fail-fast listesi.
> Bunlar bilinçli olarak varsayılan değer **taşımaz** — iş-kritik bir değerin sessizce yanlış varsayılanla çalışması, açılmamasından daha tehlikelidir.

| # | Ayar | Tip | Örnek değer | Ne işe yarar |
|---|---|---|---|---|

## B. Operasyonel secret'lar (zorunlu)

> Strateji `05_TECHNICAL_ARCHITECTURE.md §6`'da; burada **envanter** tutulur.
> **Dağıtım yarıçapı kolonu boş bırakılmaz** — "hepsine verilir" bir cevap değil, bir karardır ve gerekçesi yazılır.

| # | Secret | Nereden alınır | **Hangi bileşen alır** | **Hangisi ALMAMALI** | Rotation | Runbook |
|---|---|---|---|---|---|---|

## C. ⚠ Prod'da ayarlanması gereken ayarlar (yoksa kural sessiz kalır)

> **En kritik bölüm.** Buradaki bir ayar eksikse sistem sorunsuz açılır, testler geçer, ama bir iş kuralı **hiç çalışmaz**.
> Her satır için: varsayılan neden yetersiz + hangi kuralın devre dışı kalacağı.

| # | Ayar | Varsayılan | Varsayılanla ne olur | Prod'da ne olmalı |
|---|---|---|---|---|

## D. Bileşenler arası konfigürasyon eşleşmesi

> **Ne yazılır:** Aynı değerin birden fazla yerde tanımlandığı durumlar (ör. bir zamanlama aralığı hem arka planda hem yardımcı serviste). Hangisi otoriter, değişince ne yeniden başlatılmalı.

| Ayar | Nerelerde tanımlı | Otoriter olan | Değişince |
|---|---|---|---|

## E. Çalışırken değiştirilebilen ayarlar

| # | Ayar | Etki süresi | Not |
|---|---|---|---|

## F. Deploy sonrası doğrulama

- [ ] Sağlık uçları yanıt veriyor
- [ ] Zorunlu ayarların hepsi yüklü (§A)
- [ ] §C ayarları **açıkça** kontrol edildi (varsayılanda kalan yok)
- [ ] Arka plan işleri kayıtlı ve çalışıyor
- [ ] Dış entegrasyonlar canlı uçlara bağlanabiliyor
- [ ] Loglar akıyor, uyarı kanalı çalışıyor
- [ ] Geri alma (rollback) yolu denendi

## G. Geri alma

> **Ne yazılır:** Bir deploy geri alınacaksa hangi adımlar; şema değişikliği geri alınabilir mi, alınamıyorsa ne yapılır.

---

## H. Seed edilemeyen ilk kayıtlar

> **Ne yazılır:** Migration ve seed'in kapsayamayacağı, **kuruluma özgü** ilk kayıtlar ve bunları oluşturan **izlenen, sırsız** bootstrap script'i.

Yetkilendirmesi olan her sistem ilk deploy'da aynı duvara çarpar: *"sistem açıldı, migration geçti, testler yeşil — ama kimse yönetici değil ve yönetici yaratacak bir uç yok."* Bu an geldiğinde çözüm elle bir veritabanı komutu olur: izlenmez, tekrarlanamaz, ikinci ortamda baştan keşfedilir ve genelde sır içeren bir komut geçmişi bırakır.

**Kural:** Bu kayıtlar için repo'da **izlenen** bir bootstrap script'i bulunur. Değerler script'e **gömülmez** — parametre veya ortam değişkeni olarak dışarıdan verilir.

| # | Kayıt | Neden seed edilemez | Bootstrap script'i | Parametreler |
|---|---|---|---|---|
| 1 | İlk yönetici + rol ataması | Kimin yönetici olacağı kuruluma özgüdür; kodda varsayılanı olamaz | | |
| 2 | Operatör / servis hesabı | Aynı | | |

---

## I. Gerçek konfigürasyonla boot provası ⚠

> **Ne zaman:** Kapanış ritüelinde, **zorunlu** (`00 §L` adım 6, detay `00 §L.1`).
> **Neden:** Faz gate'leri "tüm servisleri ayağa kaldır" der ama test/E2E konfigürasyonuyla tatmin edilebilir. Bu prova **gerçek dağıtım konfigürasyonunu** ilk kez sınar.

### I.1 Ön koşullar
- [ ] §A zorunlu ayarların tamamı gerçek değerlerle hazır
- [ ] §B secret'ları hedef ortamda tanımlı
- [ ] §H bootstrap parametreleri belirlenmiş

### I.2 Boot reçetesi

> **Ne yazılır:** Adım adım, ikinci bir kişinin tekrarlayabileceği açıklıkta. "Bilinen" adımlar da yazılır.

### I.3 Prova sonucu

| # | Adım | Sonuç | Kırıldıysa: kök neden | Runbook'a eklenen satır |
|---|---|---|---|---|

> **Provanın asıl çıktısı bu tablodur.** Kırılan her şey bir runbook satırına dönüşür; prova "başarılı oldu" diye kapanmaz, **runbook'u tamamladı** diye kapanır.

### I.4 Doğrulama
- [ ] Sağlık uçları gerçek konfigürasyonda yanıt veriyor
- [ ] Bileşenler arası kimlik ve adresler gerçek değerlerle çözülüyor
- [ ] §C ayarları varsayılanda kalmadı (kural sessiz değil)
- [ ] Reçete ikinci kez, sıfırdan tekrarlandı
