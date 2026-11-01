# Shopfolio — Validation Protocol

**Versiyon: v0.1** | **Bağımlılıklar:** `02`–`11` | **Son güncelleme:** YYYY-AA-GG

> **Aşama:** 10 — Doğrulama Protokolü · **Rol:** QA / Verification Lead
> **Bu bir test case deposu değildir.** Cevapladığı soru: **"Bu ürün bir sonraki aşamaya geçilebilecek olgunlukta mı?"**
> **Kapsam `10_MVP_SCOPE.md` ile birebir hizalıdır.**

> **Aşama 1'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36.
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-06 · K-97 · Aktörler:** Doğrulama senaryoları dört aktör üzerinden kurulur — ziyaretçi · misafir alıcı · üye müşteri · firma yöneticisi (`02 §1.3`; misafir alıcıyı K-97 ekledi, satır Aşama 2'nin çakışma taramasında hizalandı).
> - **K-22 · K-415 · K-417 · K-418 · MVP başarı çıtası:** Firma siteyi kendi kendine ayaklar, içeriğini ve kataloğunu yönetir, gelen siparişleri panelden uçtan uca işletir ve düzenli sipariş alır. Çıtanın sınanan karşılığı Proje Vizyonu'nun (`01 §6`) ürün düzeyi kriterleridir — Ü-1…Ü-4, kabul kapısı (K-415, K-418); sipariş hacmi başarı kriteri değildir ve senaryoya çevrilmez (K-417; satır Aşama 3'ün çakışma taramasında hizalandı).
> - **K-23 · Kabul kanıtı:** **Referans kurulum · canlı ortam · gerçek para.** Gerçek kartla küçük tutarlı sipariş, 3DS geçişi, siparişin panelden uçtan uca işletilmesi ve ardından iadesi — bu zincir bu dokümanın kabul kanıtıdır.
> - **K-24 · Kapsam cümleleri:** `10 §2` ve `10 §3` satırları **çeviri yapılmadan, olduğu gibi** doğrulama senaryosuna devralınır.
> - **K-27 · İki ana sayfa düzeni:** Tanıtım öncelikli ve mağaza öncelikli düzenler **ayrı ayrı** doğrulanır.

> **Aşama 2'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §8.3 AK0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **Dizin — Aşama 2 kararlarının bu dokümana devirleri:** karar kaydında K-647…K-718'in etki sütunlarında bu dokümanı gösteren elli sekiz atıf (elli sekiz karar) ve Kullanıcı Akışları'nın (`03`) gövdesinde bu dokümana iş bırakan cümleler [`CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md) §6'dadır; etki sütununda yalnız doküman numarası taşıyan elli sekiz atfın işinin adını dizin verir. Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Aşama 1'in dizini: `PHASE1_CONFLICT_SCAN.md` §6.

> **Aşama 3'ten park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2 ve §3, mekanizma: K-36 (yeri: §10.3 UI0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-740 · K-741 · Genişlik sınıfları:** arayüz üç genişlik sınıfında doğrulanır — dar (P-42'den 767 CSS pikseline), orta (768–1023) ve geniş (1024 ve üstü); iki tarafta da hiçbir içerik ve işlev bir sınıfta gizlenmez (Arayüz Tanımları `04 §2`). Hangi ekranın hangi sınıfta doğrulanacağı bu aşamanın kararıdır.
> - **K-747 · Yasal kimlik bloğu ve ETBİS bandı:** firmanın yasal kimlik setinin tamamı ve — alan doluysa — ETBİS doğrulama bandı vitrinin her sayfasının altbilgisinde ve İletişim sayfasında durur; sepet, ödeme adımı ve giriş ekranları dahildir (`02 §3.1.4`; `04 §2`, §3).
> - **K-756 · K-831 · K-833 · Onay bölümü:** ödeme adımının onay bölümünde onay özeti, Ön Bilgilendirme Formu'nun ve Mesafeli Satış Sözleşmesi'nin tam metni, cayma özeti, onay kutuları ve ödeme yükümlülüğünü yazan düğme aynı bölümde, bu sırayla ve bir bağlantının arkasına saklanmadan durur; kutular işaretsiz gelir ve onay anındaki özet farkı Ön Bilgilendirme Formu'nun içeriğini değiştirdiğinde iki yasal metnin kutularının işareti kalkar (`02 §3.24`; `04 §5`).
> - **K-787…K-791 · K-794 · K-796 · Kısmi adet senaryoları:** adedi birden büyük kalemde (1) müşterinin kısmi iptali, (2) aynı kalemde ardışık iki kısmi cayma — her beyanın kendi damgası, gönderme süresi ve ulaşma tarihi, (3) eksik ulaşan iadede ulaşan adedin teslim alınması ve kalanların "mal dönmedi" kapanışı, (4) kupon payının kuruş yuvarlaması — kalemin adetleri tek tek işlendiğinde geri ödemelerin toplamı kalemin ödenmiş tutarına eşittir, (5) kargo ücretinin iki tetiği — bir adet kalırken geri ödenmez, son adetle geri ödenir, (6) firma iptalinde adet ve B-7'nin içeriği, (7) kapanış kuralının son açık adetle işlemesi, (8) kupon hakkının yalnız bütün adetlerle dönmesi, (9) gecikme feshinin işleme açık adetlerin tamamına uygulanması doğrulanır (`02 §7.1.6`, §7.1.7, §7.2.9, §7.4.6, §7.4.7). Senaryoların biçimi ve sırası bu aşamanın kararıdır.
> - **K-798 · Panelin yasal uyarıları:** yedi uyarının işlemin formunda, onay penceresinden önce yerinde kalıcı mesaj olarak göründüğü hâller doğrulanır — kanuni faiz uyarısı gecikme feshinin kaydında, başka kanaldan fesih kaydında ve Z-10 ya da Z-39 aşıldıktan sonra yapılmış iptalin kaydında da görünür (`04 §2.4.3`, §9.9). Senaryoların biçimi bu aşamanın kararıdır.
> - **K-808 · Ayıpta sözleşmeden dönme:** talebin ayıplı adetleri için iade hattının işlediği doğrulanır — fiziksel kalemde teslim alma ve ardından geri ödeme, dijital ve hizmet kaleminde doğrudan geri ödeme, havale hattında önce IBAN isteği; geri ödemenin süre sayacı yoktur (`02 §7.5.3`, §12.5; `04 §9.9`).
> - **K-823 · Dışa aktarmanın izi:** her dışa aktarmanın — siparişler, üye listesi, iletişim talepleri — işlem izine yazıldığı ve boş sonuçta dosya üretilmediği doğrulanır (`02 §10.7`; `04 §9.24`).
> - **K-834 · K-850 · İlk kurulumda çerez:** çerez politikası ilk kez yayına alınana kadar vitrinin ziyaretçinin tarayıcısına çerez yazmadığı ve iletişim formunun, hesap kaydının ve Google ile ilk girişin — aydınlatma metni önce yayına alınmış olsa da — kapalı kaldığı doğrulanır (`02 §3.1.5`, §12.2.5; `04 §5.11`; K-850 — satır Aşama 3'ün çakışma taramasında hizalandı).
> - **Dizin — Aşama 3 kararlarının bu dokümana devirleri:** karar kaydında K-723…K-850'nin etki sütunlarında bu dokümanı gösteren on sekiz atıf (on sekiz karar) ve Arayüz Tanımları'nın (`04`) gövdesinde bu dokümana iş bırakan cümleler [`CHECKPOINT_REPORTS/PHASE3_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE3_CONFLICT_SCAN.md) §6'dadır; Aşama 3'ün atıflarının hepsi işin adını taşır. Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Önceki aşamaların dizinleri: `PHASE1_CONFLICT_SCAN.md` §6, `PHASE2_CONFLICT_SCAN.md` §6.

> **Aşama 4'ten park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §12.3 TM0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-884 · K-980 · İndirme hakkının düştüğü an:** hak, indirme düğmesine basılıp dosyanın gönderimi başladığında bir kez düşer; bir saat ömürlü bağlantıdaki devam istekleri hak düşürmez; ömür yalnız isteğin kabulünde denetlenir — ömür dolarken süren aktarım kesilmez; ömür dolduktan sonraki indirme yeni bir haktır; son hakkı kalan kalemde aynı anda iki indirmeden yalnız biri hak düşürür ve dosyayı alır (Teknik Mimari `05 §7`, §7.7.4; `02 §3.12.5`; K-980). Doğrulama senaryoları bu aşamanın kararıdır.
> - **K-890 · K-893 · K-898 · K-946 · K-957 · K-967 · K-970 · K-975 · K-980 · Eşzamanlılık, tek işlenme ve kesinti:** (1) aynı son adedi aynı anda onaylayan iki müşteriden yalnız biri ayırır, öteki kalemin ayrılamadığını görür; (2) ödeme adımında iki kez gönderilen onay tek sipariş açar — onay anahtarı onayın ilk adımında aranır, ikinci gönderim bir şey yazmadan var olan siparişin teyit ekranına gider (`05 §4.2.2`; K-970; K-1199); (3) aynı sağlayıcı bildirimi iki kez gelir ve ödeme bir kez uygulanır — bildirim ile son sorgu yarışırsa da; iptal edilmiş siparişe aynı ödeme hem son sorguyla hem bildirimle gelirse tek kart iadesi doğar; aynı işlemin başarısız ve başarılı denemesi ayrı bildirimlerdir (`05 §4.5.2`, §4.5.3; K-970); (4) havale ödeme süresi kesintide dolar — iptal dönüşten sonraki ilk iş gününün sonuna ertelenir, sipariş sayfası yeni son günü gösterir, yeni hatırlatma gitmez ve kesinti hatırlatmanın anını da kapsadıysa kaçırılmış hatırlatma da gitmez; kaçan işler zamanlarına göre sırayla çalışır, kesinti 03:00'ı kapsadıysa gecelik iş onlardan sonra ilk uyanışta çalışır; iki dakikadan kısa nabız boşluğu kesinti değildir ve iptali ertelemez; son anı bir kesintinin — kısa ya da uzun — ilk iki dakikasına denk gelen sipariş ertelenmez, kalan risktir; yalnız ters vekil durduğunda öteki işler sürer, yalnız havale iptali bekler (`05 §5.4.1`, §5.4.3, §5.4.6; K-975); (5) aynı sepetin ödenmemiş önceki siparişinin ödeme süresi dolarken müşteri aynı sepetle yeniden onaylar — ölü kilit doğmaz, iki işlemden biri sırasını bekler; (6) önceki ödenmemiş sipariş bir ürünün son adedini ayırmışken müşteri aynı sepetle yeniden onaylar — önceki sipariş iptal edilir ve son adet yeni siparişe ayrılır, müşteri kendi ayırdığı stoğa takılmaz (`02 §3.17.7`); (7) aynı sepetle iki sekmeden, ayrı onay anahtarlarıyla iki onay aynı anda gönderilir — biri sepetin kilidini bekler ve öncekinin siparişini ödenmemiş önceki sipariş olarak iptal eder; aynı sepette iki ödenmemiş sipariş kalmaz (`05 §4.2.2`; K-967); (8) yönetici ürün formunu açtıktan sonra bir satış stoğu düşürür — kaydetme stoğu formun gösterdiği değerle geri yazmaz; stok ayrılmış adedin altına indirilemez (`05 §7.3.1`; K-980) (Teknik Mimari `05 §4`, §5, §7.3.5; `02 §4.3`). Senaryoların kurulumu bu aşamanın kararıdır.
> - **K-904 · K-907 · K-908 · K-971 · K-1161 · Bağlantılar ve çerezler:** (1) e-postadaki doğrulama ve geri alma bağlantısının açılması bağlantının, hesabın ve siparişin hiçbir kaydını değiştirmez — oturumun son işlem anı, form güvenliği çerezi, hız sınırının sayacı ve erişim kaydı bunun dışındadır (`05 §6.4.7`; K-971); doğrulama ve geri alma ekrandaki düğmeyle olur; (2) ödeme adımından girişe gidip dönen ya da "Sepete dön"le sepete dönüp ödeme adımına yeniden gelen müşterinin yazdıkları durur — JavaScript kapalıyken de ve sipariş onaylanınca o çerez silinir (K-1161); (3) çerez politikası yayına alınana kadar vitrinin hiçbir yanıtı çerez yazmaz — ara katman yanıttan çerez başlıklarını çıkarır (`05 §6`; `02 §8.1.3`, §12.2.5). Senaryoların kurulumu bu aşamanın kararıdır.
> - **K-913 · K-914 · K-918 · K-946 · K-975 · Uyarılar ve hata hâlleri:** (1) e-posta servisine art arda beş deneme ve en az on beş dakika boyunca kanal hatasıyla ulaşılamazsa kanal uyarısı açılır, ilk başarılı gönderimde kalkar; alıcı hatası uyarı açmaz; (2) firma bildirimleri uyarısı bir sonraki firma bildirimi gönderildiğinde ve iletişim e-postası değiştirildiğinde kalkar; (3) sayfada, formda ve sonucu belirsiz işlemde beklenmeyen hata `04` 2.7.3'ün üç hâlini gösterir ve teknik ayrıntı taşımaz; (4) durum ucunun her uyarı koşulu 503 döndürür, canlılık ucu bundan etkilenmez — iş kuyruğunun zamanı gelmiş en eski satırı on beş dakikadan eskiyse ya da "başarısız" satır varsa ve gecelik imhanın son başarılı çalışması yirmi altı saatten eskiyse dahil; bir koşulun kaynağı okunamazsa — yetki, bağlantı ya da dosya hatası — koşul açık sayılır ve uç 503 döner (`05 §9.4.10`; K-975); (5) kuralı yazılmamış bir iş türünde sürekli hata veren satır beş yeniden denemeden sonra "başarısız" olur ve sıradaki satırları bekletmez; uyarı gecelik temizlikle kalkmaz, satır yönetim komutuyla yeniden kuyruğa alınınca ya da kapatılınca kalkar; başarısız biten gecelik iş bir saat dolmadan yeniden başlamaz (Teknik Mimari `05 §4.3`, §9, §10). Senaryoların kurulumu bu aşamanın kararıdır.
> - **K-928 · Tasarım değerleri:** elli kalemli sepette ödeme öncesi yeniden değerlendirme ve sipariş onayı isteklerin %95'inde bir saniye içinde; dakikada yüz farklı ziyaretçi yükünde sayfa yanıtlarının %95'i 500 milisaniye içinde — referans sunucu ölçüsünde (2 sanal işlemci, 4 GB) (`05 §11`; `02 §11.3` H-3, H-4). Yük senaryosunun kurulumu bu aşamanın kararıdır.
> - **K-951 · K-1092 · Elektronik ticaret kayıtlarının on yıllık saklaması:** zaman ilerletilerek doğrulanır — (1) ödemesi alınmış siparişin gönderim kaydı üç yıl dolunca silinmez, on yıl dolunca silinir; (2) ödemesi hiç alınmamış siparişin gönderim kaydında üç yıl dolunca alıcı ve olayın değerleri boşalır, satır on yıla kadar kalır (`05 §7.4.3`); siparişin kişisel verisi üç yılda silinir, kişisel verisiz kaydı on yıl dolmadan silinmez; (3) "Sipariş hakkında" tipindeki iletişim talebi on yıl, öteki tipler iki yıl dolunca silinir; (4) iptalden sonra gelip sistemce geri ödenen kart ödemesi siparişi ödemesi alınmış koluna koyar; (5) ayıp talebi siparişin imhasıyla silinmez — durumunun son değiştiği andan, hiç değişmemişse açıldığı andan on yıl dolunca silinir ve siparişin imha parçasından önce silinmez; ödemesi alınmış siparişe bağlanan gelen bildirim oluşturulmasından on yıl dolunca silinir ve siparişin imha parçasından önce silinmez (`05 §7.4.4`; Veri Modeli `06 §4.35`; K-1092 (3)) (`02 §4.2` Z-28, Z-29, Z-33, Z-41, Z-43, §6.2.16, §12.2.7; Teknik Mimari `05 §7.4.3`). Senaryoların kurulumu bu aşamanın kararıdır.
> - **Dizin — Aşama 4 kararlarının bu dokümana devirleri:** karar kaydında K-855…K-979'un etki sütunlarında bu dokümanı gösteren yirmi atıf (yirmi karar) ve Teknik Mimari'nin (`05`) gövdesinde bu dokümana iş bırakan cümleler [`CHECKPOINT_REPORTS/PHASE4_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE4_CONFLICT_SCAN.md) §6'dadır; Aşama 4'ün atıflarının hepsi işin adını taşır. Aşama 4'ün checkpoint'inin kararları (K-980…K-988) tarama raporunda yoktur — bu dokümana bıraktıkları iş yukarıdaki satırlarda etiketiyle durur. Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Önceki aşamaların dizinleri: `PHASE1_CONFLICT_SCAN.md` §6, `PHASE2_CONFLICT_SCAN.md` §6, `PHASE3_CONFLICT_SCAN.md` §6.

> **Aşama 5'ten park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §14.2, yazım turu ve kapanış sırasının 2. maddesi).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-1015 · K-1042 · K-1043 · K-1091 · Sayaçların değişmezi:** varyantın ve hizmet kontenjanının ayrılmış adedi ödemesi beklenen siparişlerdeki ayırmaların toplamına eşittir — kontenjansız hizmetin ayırması da sayılır; kuponda kullanılmış ve ayrılmış haklar toplam kullanım adedini aşmaz; toplam kullanım adedinin panel yazımı da yeni değer iki hakkın toplamından az değilse yazılır (`06 §4.13`; K-654) — tek istisna yanlış iptalin düzeltilmesidir: hak bu arada dolmuşsa da yeniden kullanılmış sayılır (Veri Modeli `06 §2.7.1`, §4.13, §4.14; `02 §3.6.7`, §3.10.6, §10.4.6). Onaydan, ödemeden, süre dolumundan, başarısızlıktan, iptalden, varyantın kalıcı silinmesinden, toplam kullanım adedinin panelden düşürülmesinden, havale "ödendi" işaretinin düzeltilmesinden — ayırma yeniden kurulur, satılabilir adet değişmez, silinmiş varyantın satırı yazılmaz — ve yanlış iptalin düzeltilmesinden — adet yeniden alınıp kesin düşer, ayırma satırı kalmaz; varyantı bu arada kalıcı silinmiş kalem adet almaz ve düzeltmeyi durdurmaz (`02 §10.4.6`) — sonra değişmezin tuttuğunu ve istisnanın yalnız bu düzeltmede doğduğunu gösteren senaryolar bu aşamanın kararıdır.
> - **K-1018 · K-1019 · K-1020 · Birim fiyat:** birim fiyat her varyantın kendi fiyatından ve kendi net miktarından — ölçü birimi ürün düzleminde, net miktar varyant düzlemindedir; ürüne birim seçildiyse her varyantın net miktarı zorunludur (K-1019, K-1020) — temel birimin bir ölçüsü için hesaplanır — gram, mililitre ve santimetre girişi kilograma, litreye ve metreye çevrilir —, kuruşa en yakın, yarım yukarı yuvarlanır, indirimdeki varyantta indirimli fiyattan hesaplanır ve satış fiyatıyla aynıysa gösterilmez (`02 §3.8.5`, §3.8.3); ürün sayfasında birim fiyat seçilen varyantınkidir, varyant seçilmeden önce varsayılan varyantınki; birim değiştirilince var olan net miktarlar silinmez, yeni birimle okunur (K-1020 (3), (5); `04` 5.4.4, 7.2.4.13). Hesabın ve gösterimin senaryoları bu aşamanın kararıdır.
> - **K-1023 · K-1041 · Tarih aralığının sınırları ve referans fiyatın penceresi:** indirimin, kuponun ve duyurunun tarih aralığı başlangıç gününün 00.00'ında başlar ve bitiş gününün 23.59'unda biter — Türkiye saatiyle; sistem indirimi bu anlarda başlatıp bitirir ve fiyat geçmişine o anla yazar — zamanlayıcı kesintide geç yazsa da satıra aralığın sınırı yazılır (K-1041 (1)); indirimin başladığı an referans fiyat penceresinin başıdır, pencere varyantın ilk satışa sunulduğu andan önceye gitmez ve pencere içindeki satışa kapalı aralıklar da sayılır (K-1023 (2)) (`02 §4.1.4`, §3.9.3, §3.9.4; Veri Modeli `06 §2.5.2`, §4.6 `FirstOnSaleAt`, §4.11). Sınır anlarının ve pencerenin senaryoları bu aşamanın kararıdır.
> - **K-1027 · Belgenin kopyası ve onay anındaki değerler:** sipariş sayfasında, panelde ve B-1'de gösterilen Ön Bilgilendirme Formu ve Mesafeli Satış Sözleşmesi siparişin onay anında üretilen kişisel verisiz kopyasıyla aynıdır; adres ya da e-posta düzeltilmiş siparişte belge onay anındaki değeri gösterir; imhadan sonra kopya kişisel veri taşımaz (`02 §3.23.3`, §12.2.7; Veri Modeli `06 §4`, §9). Senaryoları bu aşamanın kararıdır.
> - **K-1054 · K-1056 · K-1057 · K-1076 · K-1077 · K-1079 · K-1104 · K-1131 · Kalem kayıtlarının ve geri ödemenin değişmezleri:** ayıp talebinin tekrar sayısı ve yeniden açılış tarihleri talebin durum geçmişinden okunur ve her durum değişikliği geçmişe bir girdi ekler (Veri Modeli `06 §4.30`; `02 §5.10`); ayıpta sözleşmeden dönülen adet iptale ve caymaya açık değildir — işleme açık adetten düşer, ayıpta dönülmüş adedi olan kalemde kalemin işleme açık son adetlerini kapatan işlem — cayma dahil — kuponun payının ve KDV'nin kalanını alır ve geri ödemelerin toplamı kalemin ödenmiş tutarını aşmaz: ayıpta bir adetten dönme ile kalan adetten cayma iki sırada da aynı toplamı verir (`02 §7.1.6`, §7.1.7 (4); `06 §4.18`, §4.33); yanlış firma iptali düzeltilince iptalin adetleri yeniden açık sayılır, ayırdığı kupon payı ve KDV işleme açık son adetleri kapatan işlemin kalanından düşülmez, gönderilmemiş geri ödemesi geçersizleşir, açık IBAN isteği kapanır ve o iptal için girilmiş IBAN silinip imha kaydına yazılır — IBAN'ı geri ödemesi işlenmeyi bekleyen başka bir kalem ya da aynı kalemin başka bir geri ödemesi kullanıyorsa onu kullanan son geri ödeme tamamlanınca (K-1131) (`02 §10.4.6`, §4.2 Z-35; `06 §4.22`, §4.29, §4.31, §4.33); cayma beyanının adetleri parça parça teslim alındığında beyanın bütün adetleri geri ödenince parçaların kupon payı ve KDV toplamı beyanın payına ve KDV'sine eşittir (`02 §7.1.7` (5); `06 §4.33`); teslimden sonraki caymada kargo ücreti gönderilen fiziksel kalemlerin son adetlerini teslim alan geri ödemeye bir kez eklenir, iade reddi alan ya da "mal dönmedi" ile kapanmış adet varken eklenmez (`02 §7.4.6`; `06 §4.33`). Senaryoları bu aşamanın kararıdır.
> - **K-1064 · K-1065 · K-1083 · Hesap tarafının silmeleri ve eski adres:** yöneticinin kaldırılması imha kaydına `Admin` türüyle ve uygulama kaydına bir satır yazar; eski bir ana dönüşten sonra silmelerin yeniden uygulanması kaldırılan yöneticiyi yeniden siler — ele geçirilmiş hesap geri gelmez; yeniden uygulama kullanıcı işleminin kapılarını (son yönetici, işlemi yapanın yöneticiliği) uygulamaz, dönüş noktasından sonraki kaldırma işlem izine yazıldığı için dönüş denetimi eski bir ana dönüşü zaten durdurur — yeniden uygulama savunmada derinliktir (`02 §12.2.7`, Z-44; `05 §7.4.5`, §8.5.10, §8.7.4; Veri Modeli `06 §4.37`; K-1083); üyenin adres defteri girdisini silmesi imha kaydına yazılmaz (`06 §4.45`); yönetici daveti — kabul edilen, geri çekilen, geçersizleşen, ömrü dolan — gecelik temizlikte bağlantı kaydıyla silinir ve imha kaydına yazılır (`05 §5.2.9`; `06 §4.38`); e-posta değişikliğini geri alma bağlantısı geri almada hesabın e-postasını değişiklikten önceki adrese döndürür, kullanılmadan ömrü dolarsa eski adres gecelik temizlikte boşaltılır ve imha kaydına `LinkRecord` türüyle yazılır (`02 §4.2` Z-45; `05 §5.2.9`; `06 §4.42`). Senaryoları bu aşamanın kararıdır.
> - **Dizin — Aşama 5 kararlarının bu dokümana devirleri:** karar kaydında K-992…K-1136'nın etki sütunlarında bu dokümanı gösteren yirmi iki atıf (yirmi iki karar) ve Veri Modeli'nin (`06`) gövdesinde bu dokümana iş bırakan cümleler [`CHECKPOINT_REPORTS/PHASE5_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE5_CONFLICT_SCAN.md) §6'dadır; Aşama 5'in atıflarının hepsi işin adını taşır. Biri bu dokümanın Aşama 4 park satırını güncelledi (K-1092 — on yıllık saklama) — iş o satırda etiketiyle durur. Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Önceki aşamaların dizinleri: `PHASE1_CONFLICT_SCAN.md` §6, `PHASE2_CONFLICT_SCAN.md` §6, `PHASE3_CONFLICT_SCAN.md` §6, `PHASE4_CONFLICT_SCAN.md` §6.

> **Aşama 6'dan park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §16.2, yazım turu, kalite döngüsü ve kapanış sırasının 2. maddesi).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-1163 · K-1164 · K-1165 · K-1166 · K-1167 · K-1168 · K-1169 · K-1170 · K-1171 · K-1172 · K-1202 · K-1203 · Uçların doğrulanacak davranışları:** OB-04'ün işlemi onay işareti olmadan uygulanmaz · GET hiçbir kaydı değiştirmez — `07 §2.2.5`'in istisnaları dışında (K-1179 (3)) · başka bir müşterinin kaydını ya da var olmayan bir kaydı isteyen istek — iç anahtar, sipariş numarası, erişim anahtarı — aynı 404'ü alır ve hiçbir şey yazmaz · misafirin takip sorgusu erişim anahtarlı adrese iner · sürüm çakışmasının iki varyantı (409) · sahteciliğe karşı korumanın dışında yalnız bildirim ucu ve — POST'sa — ödeme dönüşü kalır (PayTR'de dönüş GET'tir, ikinci istisna doğmaz — `08 §2.1.4`; K-1255 (1), K-1292); doğrulama hatasında hiçbir şey yazılmaz · geçersiz liste parametresi süzgeci uygulamaz ve hata vermez · durum kodları API Tasarımı'nın tablosuyladır — genel sınır ve deneme limitleri 429, gövde tavanı 413 · kalem işleminde aşan ya da gösterilenden farklı açık adet isteği bütünüyle reddeder — kısmi seçimin tekrar gönderimi ikinci işlem olmaz (K-1202), gecikme feshinde gelen adet yok sayılır · parçalı yüklemenin sürdürülmesi, eşzamanlı ikinci parçanın kilidi bekleyip 409 alması, son parçadan sonra çöken yüklemenin sonraki parça isteğinde bayt eklenmeden tamamlanması (K-1203), satırı silinmiş yüklemeye 404 ve baştan başlama, silinmiş yüklemeyle form kaydı (422) · indirme bağlantısında aralık isteği hak düşürmez, ömrü dolmuş değer 404 alır · formda olmayan alan bağlanmaz (API Tasarımı `07 §2`, §4). Senaryoların yazımı bu dokümanın işidir. İşleyicisi bulunamayan ya da `handler`'ı eksik POST'un çatı yanıtı (404 ya da 400) bir doğrulama senaryosuyla sabitlenir (`07 §2.7.1`).
> - **K-1178 · K-1179 · K-1181 · K-1199 · Vitrinin uçlarında doğrulanacak davranışlar (yazım turunun 2a oturumu):** güncel hâlle karşılaştırılmayan dört işlem — sepete ekleme, iletişim talebi, hesapta adres ekleme, indirme bağlantısının üretimi — tekrar gönderimde ikinci kez işlenir; betikli yolda düğmenin kilidi ve gönder–yönlendir–göster çift gönderimi önler · o sepette bulunmayan sepet kalemine adet değişikliği ve çıkarma 404 alır ve hiçbir şey yazmaz · sepetin GET'i gösterdiği işaretli kalemi siler, gösterdiği ödeme yöntemi işaretini kaldırır (K-1183) ve misafir sepetinin son erişim anını yazar — başka bir kayıt değişmez · ürün sayfasında tutmayan varyant seçimi varsayılan varyanta düşer ve hata vermez; seçim parametreli adres `noindex` taşır · ödeme adımının onayı: alan hatası, özet farkı ve satışın kapanması 422 ile siparişsiz yeniden gösterim, L-7 ve havalede L-8 429, kalemsiz sepet sepete, aynı onay anahtarı var olan siparişin teyit ekranına (K-1199, K-1226), oturumu kapanmış üye girişe — yazdıkları çerezde · "Teslimat adresimle aynı" işaretliyken gönderilen fatura alanları siparişe girmez (API Tasarımı `07 §2.2.5`, §2.5.1, §4.4, §4.12, §4.13). Senaryoların yazımı bu dokümanın işidir.
> - **K-1183 · K-1184 · K-1185 · K-1186 · K-1200 · Hesap uçlarında ve makine uçlarında doğrulanacak davranışlar (yazım turunun 2b oturumu):** girişteki birleşmenin mesajı ve ödeme yöntemi kalmadığı için ve teyit ekranından sağlayıcıya bağlanılamadığı için (K-1200, K-1226) sepete dönüşün mesajı bir kez görünür, yenilemede gelmez · hesap silmenin sonuç hâli bir kez görünür; yenileme, geçmiş ve elle açılış ana sayfaya iner · bağlantı ekranında sonuç hâlinin kodu kullanılmamış bağlantıda yok sayılır; tüketimi tutmayan bağlantı o anki hâli gösterir ve hiçbir şey yazmaz · kayıttan sonraki hâlin değeri çözülmüyorsa ya da kayıt bekleyen kayıt değilse form boş gelir · yeniden doğrulamanın değeri doğrulama hatasında ve onay işaretsiz ilk gönderimde tüketilmez, tüketilmiş değerle gönderim yeniden doğrulamaya gider · Google dönüşünün değerleri ikinci kez tutmaz · ödeme dönüşü durum yazmaz, oturumsuz da erişim anahtarlı köke iner, kayıt okumaz; biçimsiz değer 404, biçimi doğru ama geçersiz ya da yenilenmiş anahtar önce 303 alır ve 404'ü sipariş sayfası verir · bildirim ucunda imzası tutmayan istek 400, tekrar eden bildirim 200 ve kayıtsız · parçalı yüklemenin aynı parçası 409, bayt ikinci kez eklenmez (API Tasarımı `07 §4.20`–§4.34). Senaryoların yazımı bu dokümanın işidir.
> - **K-1188 · K-1189 · K-1190 · K-1191 · K-1192 · Panelin uçlarında doğrulanacak davranışlar (yazım turunun 3a oturumu):** yöneticinin kalem işleminde sürüm aynıyken işleme açık adedi aşan istek bütünüyle uygulanmaz, hiçbir kalem işlenmez (422); sürüm değişmişse 409 ve güncel hâl · başka kanaldan gecikme feshinin kaydında gelen kalem ve adet alanları yok sayılır · başka siparişin kalemi ya da kaydı 404 · yayın geçişi ve silme sürüm taşımaz, kapıya ve son Yayında varyanta karşı denetlenir · adı girilmiş yeni üründe stok kodu boş yeni varyanta öneri yazılır; ad da boşsa iki alan zorunluluğun mesajını alır · davetin korumalı değeri çözülmüyorsa giriş formu boş gelir; kullanılmış davet bağlantısı sonuç göstermez · yeni ürün ve kategori formunun tekrar gönderimi ikinci kayıt açar, yeni kuponunki kodun tekilliğine takılır · imha edilen iletişim talebinin adresi ve ona gelen kapatma 404 (API Tasarımı `07 §9.1`–§9.12). Senaryoların yazımı bu dokümanın işidir.
> - **K-1193 · K-1194 · K-1195 · K-1213 · K-1217 · Panelin içerik, ayar ve rapor uçlarında doğrulanacak davranışlar (yazım turunun 3b oturumu):** içerik kaydının yayın geçişi ve silmesi sürüm taşımaz, kapıya ve Hakkımızda'nın silinmezliğine karşı denetlenir · Hakkımızda ya da duyurunun ikinci yeni kolu 409 alır ve var olan kayda bağlanır · elle sıranın gösterilen sırası tutmuyorsa 409, hiçbir sıra yazılmaz · geçici kapatma anahtarı o arada değiştiyse işlem uygulanmaz — 303 ve güncel hâl · ödeme yöntemlerinde ve iade adresinde onay yalnız riskli değişiklikte istenir, ilk IBAN ve ilk adres onaysızdır · tamamlanmamış aydınlatma metninin yayına alınması bütünüyle reddedilir, taslak da yazılmaz · dışa aktarma 200 ve ek olarak CSV döner, `no-store` taşır; boş kümede 422 ve iz yazılmaz; tekrar gönderim ikinci dosya ve ikinci iz satırıdır · sipariş dosyası yalnız ödemesi tamamlanmış siparişleri, kalem başına bir satırla taşır; kişisel verisi imha edilmiş siparişin satırı kalır ve kişisel sütunları boş gelir; formül karakteriyle (`=`, `+`, `-`, `@`), sekmeyle ya da CR ile başlayan müşteri girdisi hücreleri etkisizleştirilmiş gelir (K-1213 · K-1217; API Tasarımı `07 §9.13`–§9.24). Senaryoların yazımı bu dokümanın işidir.
> - **K-1196 · K-1198 · Hata kodları ve girdi modellerinde doğrulanacak davranışlar (yazım turunun 4. oturumu):** senaryonun birimi API Tasarımı'nın hata kodudur (`07 §6.2`, §6.3) — her kod en az bir senaryoda; `02 §6`'nın hata yanıtı doğurmayan satırları (`07 §6.2.10`) başarı yolu, görünüm ya da sistem işi senaryosudur · formda modelin dışındaki alan yok sayılır, sunucunun türettiği değer — fiyat, toplam, açık adet, durum — formdan okunmaz · görünmez tuzak alan (`Website`) doluysa yanıt başarının aynısıdır ve talep yazılmaz · parçalı yüklemenin 422'si gövdede yalnız P-26'nın değerini taşır, kod taşımaz · `notice`'in listedışı değeri yok sayılır · kalan riskler (`07 §6.4.3`; K-974, K-978) kullanıcıya yanıt doğurmaz, senaryoyla sınanır: kaydedilemeyen sağlayıcı bildirimi 500 alır ve Bekliyor siparişin sonucunu son sorgu bulur; "gerçekleşmedi" işaretinden sonra gelen kart iadesi sonucu siparişe işlenmez, geri ödemenin satırında yerinde kalıcı uyarı durur; imzası doğru ama tutarı ya da siparişi tutmayan bildirim 200 alır, işaretli kaydedilir ve işlenmez.
> - **K-1204 · K-1211 · K-1212 · K-1216 · Derin gözden geçirmeden doğrulanacak davranışlar — K'sız bulgular L6a-2, L6a-1, L4a-4, L4a-7, `AUDIT_REPORTS/07_DEEP_REVIEW.md` (ödeme, arama, veri koruma):** Ödendi siparişe farklı işlem kimliğiyle gelen ikinci başarılı ödeme iade edilir, sipariş Ödendi kalır, firmaya F-4 ve müşteriye B-8 gider (`05 §4.5.3`) — bu doğrulama PayTR'de üretilemez: PayTR'nin bildiriminde işlem kimliği `merchant_oid`'dir ve aynı `merchant_oid`'li ikinci bildirim tekrar sayılır; PayTR'de doğrulanan, DV-11'in sonucudur (aynı referansla ikinci ödeme kabul ediliyorsa tespit ve iade kurulana kadar kart anahtarları tanımlanmaz — `08 §2.1.7`, §4.1 DV-11; K-1284) · imzası doğru ama olay türü tanınmayan bildirim işaretli kaydedilir ve 200 alır · ödeme sağlayıcısına bağlanılamadığında sepetin ödeme işareti isteği yapan tarayıcının sepetine yazılır, tarayıcıda sepet yoksa yazılmaz · ödeme adımı çerezine onay kutuları, onay anahtarı, parmak izi ve sürüm kimlikleri yazılmaz · KDV oranı değişen sepetin onayı özet farkıyla 422 verir · arama sözcüğündeki `%` ve `_` harfi harfine eşlenir · bir amacın veri koruma değeri öteki amaçta çözülmez · hesap bulunmayan e-postayla girişin yanıt süresi sahte özet sayesinde hesabı ele vermez.

> **Aşama 7'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §18.1, yazım turunun 5. oturumu, `ENT5-02`).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-1257 · K-1258 · K-1259 · K-1272 · K-1291 · Dış servislerin doğrulanacak davranışları (Entegrasyon Spesifikasyonları `08 §3`, §4):** (1) `08 §4.1`'in 43 DV satırının her biri bir doğrulama kanıtı bekler; kanıtı hangi tarafın (kurulumu yapan, ilk bağlanan task, kabul) alacağı satırda yazılıdır ve kabul kanıtına düşen satırlar — DV-6, DV-7, DV-10 ve test modunun 3D sayfası bankanın gerçek adresi değilse DV-5'in banka sayfası gözlemi — canlı kanıtla birlikte okunur (`08 §2.1.10`); DV-43'ün kabulü K-1281'in sonucudur ve (5)'teki senaryoyla sınanır; kanıt beklenen sonucu vermezse satırın "yanlış çıkarsa" hükmü işler · (2) ödeme bildirimi: imzası tutmayan ya da imzasız istek 400 alır ve kayda yazılmaz; imzası doğru bildirim `OK` alır; aynı bildirimin tekrarı kaydedilmez ve yine `OK` alır; tutarı ya da siparişi tutmayan ve olay türü tanınmayan bildirim işaretli kaydedilir; kaydedilemeyen bildirim `OK` almaz (500) (`08 §3.2`) · (3) servis çökme senaryoları: ödeme sağlayıcısına onaydan sonra bağlanılamaması siparişi kendiliğinden iptal eder ve L-7'ye sayılmaz; yanıtsız son sorgu siparişi iptal etmez; e-posta altyapısı çökerse sipariş, ödeme ve durum geçişleri sürer ve satır yeniden denenir; Google çökerse şifreyle giriş, girişsiz sipariş ve panel sürer (`08 §3.3`) · (4) çerçeve: ödeme formu yalnız ödeme sayfasında ve yalnız PayTR adresinden (ve çerçevede yüklenen geçiş sayfası için sitenin kendisinden) açılır; dönüş geçiş sayfasıyla üst pencerede sipariş sayfasını açar — betik kapalıyken de `target="_top"` bağlantısıyla —, çerçeve izni (`frame-ancestors 'self'`) yalnız geçiş sayfasındadır ve sipariş sayfası hiçbir çerçevede açılmaz (`05 §6.5.4`; `08 §2.1.4` madde 3; K-1259) · (5) üyenin sipariş e-postası bağlantısı girişsiz açılmaz; misafirin bağlantısı erişim anahtarıyla açılır (`02 §3.22.5`) · Google ile girişte Google adres için yetkili değilse (`@gmail.com` değil, `hd` yok) ve hesap Google'a bağlı değilse oturum açılmaz, bağ kurulmaz ve hesabın e-postasına teyit bağlantısı gider; onaydan sonra bağ kurulur ve sonraki giriş doğrudan açılır (`02 §3.13.7`; `08 §2.3`; K-1281) · (6) dış sistemde kayıtlı adreslerin (`08 §4.2`) değişiminde dış kaydın aynı değişiklikte güncellendiği · (7) Google ile giriş, kayıt ve yeniden doğrulama (`08 §2.3`; `07 §4.22.3`): hata tablosunun her satırı iki akışta — giriş ve kayıt, yeniden doğrulama — kendi kodu ve ekranıyla sınanır, hiçbirinde oturum açılmaz ve izin yazılmaz; kimlik belirtecinin imzası, `iss`, `aud`, `exp` ya da `nonce` tutmazsa akış tutmaz; `email_verified` `false` ya da yoksa hesap açılmaz ve bağ kurulmaz; `auth_time` gelmezse ya da başlatmadan önceyse izin yazılmaz; birincisi yanıtlanmadan gelen ikinci dönüş hiçbir şey yazmaz (K-1206); `name` gelmezse yeni hesabın adı e-postanın `@` öncesidir (K-1265); istemci kimliği ve sırrı tanımlı değilse düğme görünmez ve başlatma 422 alır (ÖK-6) · (8) e-posta gönderimi (`08 §2.2.2`–§2.2.4): yanıtın sınıfı hata eşlemesine göre yazılır, tabloda olmayan yanıt kanal hatasıdır; geri dönen adres sorgusu eşleşen gönderilmiş satırı "ulaşmadı" yapar, başarısız tur boş geçer ve kanal hâlini etkilemez; alıcı başına 24 saatte 20 müşteri bildirimini aşan satır servise çağrı yapılmadan "ulaşmadı" olur ve sipariş etkilenmez.

> **Aşama 8'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §20.1, çakışma taraması, K-1391).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-1325 · K-1354 · K-1355 · Ön yüzün tarayıcıda doğrulanması:** Kodlama Kılavuzu ön yüzü yalnız kaynak taramasıyla sınar ve tarayıcıda çalışan betiği ve uçtan uca akışı kendi test katmanlarının dışında bırakır (`09 §16.1` madde 9, `§19.2` madde 1); ön yüz betiklerinin — ortak betik ve sayfa modülleri — tarayıcıdaki davranışı, betikli ve betiksiz yol, bu dokümanın senaryosudur ve CI'ın 8. adımına (E2E, advisory) bağlanır (`05 §8.3.11`). Parçalı yüklemenin uç senaryoları (Aşama 6 bloğu, K-1163 ve K-1183 satırları) gerçek yükleme betiğiyle tarayıcıda da koşulur — 409'dan sürme, 404'te baştan başlama, yönlendirmede durma, ağ hatasında üç deneme (`09 §16.2` madde 7, `§15.7`); betiğin parça başlığı üretimi ve sunucu davranışı `09`'un kendi testindedir, burada yinelenmez. Hangi akışın hangi tarayıcıda sınanacağı (taban: son iki büyük sürüm — `05 §3.8.4`) bu aşamanın kararıdır.
> - **K-1334 · Çatının kendi yanıtları:** yöntemi uymayan istek (405), aralığın dışındaki dosya isteği (416) ve işleyicisi bulunamayan ya da `handler`'ı eksik POST (404 ya da 400) çatının yanıtıdır; beklenmeyen hatanın yakalama katmanına düşmez — 500'e, sonucu belirsiz hâlin 303'üne ya da beklenmeyen hata kayıtlarına çevrilmez (`09 §8` madde 3; `07 §2.7.1`). Üçünün de yanıtı bir doğrulama senaryosuyla sabitlenir; işleyicisi bulunamayan POST'un satırı Aşama 6 bloğundadır (K-1163 satırının son cümlesi) ve bu madde onu 405 ve 416 ile genişletir.

> **Aşama 9'dan park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §22.1, çakışma taraması, K-1494).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-755 · K-841 · Vitrinin eksik satır kuralı:** ana sayfanın ürün vitrini orta sınıfta son satırı doldurmayan kartları göstermez; en az bir tam satırı dolduramayan vitrin ise kartlarının tamamını gösterir. Kural iki düzenin kayıt sayılarıyla (mağaza öncelikli düzende sekiz, tanıtım öncelikli düzende dört) tarayıcıda doğrulanır; Kodlama Kılavuzu yerleşimi tarayıcıda sınamaz (`04` 5.1.13; `11` T502 kabul kriteri 2, ileri işaret).
> - **K-625 · K-1411 · Erişilebilirlik taraması F5 kapısında:** A seviyesi tarama F5'in gate-check'inde koşar ve temiz çıkmalıdır (`11 §6.2` F5 satırı); bileşen tabanı T540, T543 ve T544'tedir — klavye, odak ve %200 yakınlaştırma (T540 kabul kriteri 4). Taramanın maddeleri (Kontrol Listesi A seviyesi ve WCAG 2.2 A; K-625) ve yöntemi bu dokümanda, F5 kapısından önce hazır olur.
> - **K-23 · K-1453 · F6 kabul kanıtının senaryolara bağlanışı:** `11` T601'in kabul kriteri 10'u ve `11 §6.2`'nin F6 notu ileri işarettir (ileri işaret: 12). Bu doküman yazılınca canlı kanıt zincirinin — gerçek kartla küçük tutarlı sipariş, 3D Secure, panelden uçtan uca işletme, iade — senaryo kimlikleri T601'in kabul kriteri 10'una eşlenir ve `11`'e geri yazılır.

---

## 1. Amaç ve kapsam

### 1.1 Amaç
### 1.2 Kapsam dahili

| Alan | Kapsam |
|---|---|

### 1.3 Kapsam dışı

| Alan | Neden |
|---|---|

---

## 2. Doğrulama prensipleri

> Bu bölüm **dolu başlar** — evrensel prensiplerdir, projeye göre değişmez.

| # | Prensip | Anlamı |
|---|---|---|
| **P1** | Gereksinim bazlı doğrulama | Her madde bir iş kuralına/akış adımına/spesifikasyona dayanır. "Çalışıyor gibi görünüyor" kabul edilmez; beklenen sonuç **önceden** tanımlıdır. |
| **P2** | Kanıt zorunluluğu | Kanıtsız PASS geçersizdir. Her sonuç en az bir kanıt türüyle desteklenir (§7). |
| **P3** | Çok katmanlı kanıt | Kritik akışlarda tek bir arayüz gözlemi yetmez; durum, veri, log ve olay düzeyinde tutarlılık aranır. |
| **P4** | Failure ve geri alma dahil | Yalnız happy path doğrulanmaz. "Başarısız olduğunda güvenli duruma dönüyor mu?" da kanıtlanır. |
| **P5** | Yapan ≠ denetleyen | Kodu yazan ajan kendi çıktısını doğrulayamaz. Doğrulama **ayrı context**te yapılır. |
| **P6** | Tekrar edilebilirlik | Manuel doğrulama geçici olarak kabul edilir; otomatize edilebilenler zamanla otomatize edilir. |
| **P7** | Kapsam hizalaması | MVP'de olmayan özellik kriter olamaz; MVP'de olan çıkarılamaz. |
| **P8** | Tıkanma yasağı | FAIL'de net düzeltme akışı vardır (§8). Kritik olmayan açık bulgular **kayıtlı** risk olarak kabul edilebilir. |

---

## 3. Doğrulama seviyeleri

> **Not:** Doğrulama **seviyeleri** (bu bölüm) ile kalite **kapıları** (00 §D.6) farklı eksenlerdir. Seviyeler *neyin* doğrulandığını, kapılar *ne zaman* doğrulandığını söyler.

| Seviye | Soru | Ne zaman tetiklenir | Kaynak |
|---|---|---|---|
| **A — Gereksinim** | İş kuralları gerçekten karşılanıyor mu? | İlgili task bitince | `02`, `10` |
| **B — Fonksiyonel** | Akışlar uçtan uca çalışıyor mu? | Task + faz | `02`, `03`, `10` |
| **C — Durum makinesi** | İzinli geçişler çalışıyor, yasak geçişler engelleniyor mu? | Durum makinesine dokunan her task | `03`, `06` |
| **D — Veri** | Veri doğru saklanıyor, dönüşüyor, siliniyor mu? | Veri katmanı task'ları | `06`, `09` |
| **E — Entegrasyon** | Dış servislerle sözleşme doğru mu? | Entegrasyon task'ları | `08` |
| **F — Operasyonel** | Sistem gerçek koşullarda ayakta kalıyor mu? | Faz sonu + final | `05`, `08`, `09` |

---

## 4. Doğrulama matrisi

> **Ne yazılır:** Her doğrulama maddesi bir satır.

| Sütun | Açıklama |
|---|---|
| **ID** | Benzersiz tanımlayıcı (`VAL-NNN`) |
| **Seviye** | A–F (§3) |
| **Kural / Gereksinim** | Doğrulanacak iş kuralı veya davranış |
| **Kaynak** | Kuralın tanımlandığı doküman ve bölüm |
| **Ön koşul** | Doğrulama öncesi sağlanması gereken durum |
| **Beklenen sonuç** | PASS için sağlanması gereken koşul |
| **Kanıt türü** | Kabul edilen kanıt(lar) — §7 |
| **Kanıt ortamı** | `mock` / `sandbox` / `gerçek` — kanıtın hangi ortamda üretildiği |
| **Severity** | KRİTİK / ORTA / DÜŞÜK — §8 |
| **Durum** | `BEKLEMEDE` / `PASS` / `FAIL` / `KABUL EDİLMİŞ RİSK` |

| ID | Seviye | Kural | Kaynak | Ön koşul | Beklenen sonuç | Kanıt türü | Kanıt ortamı | Severity | Durum |
|---|---|---|---|---|---|---|---|---|---|
| VAL-A001 | A | | 02 §… | | | | mock/sandbox/gerçek | KRİTİK/ORTA/DÜŞÜK | BEKLEMEDE |

### 4.1 Durum tanımları

| Durum | Anlamı |
|---|---|
| `BEKLEMEDE` | Ön koşulu sağlanmadığı için **henüz doğrulanamadı**. PASS da FAIL da değildir. |
| `PASS` | Kanıtla doğrulandı |
| `FAIL` | Beklenen sonuç sağlanmadı — §8 akışı başlar |
| `KABUL EDİLMİŞ RİSK` | FAIL ama proje sahibi onayıyla kayda alındı (§8.4). KRİTİK olamaz. |

**Neden ayrı bir alan:** `§6.3` final çıkışı *"hiçbir KRİTİK madde FAIL veya BEKLEMEDE olamaz"* der. Durum bir alanda tutulmuyorsa bu soru **mekanik olarak cevaplanamaz** — "hangi maddeler hâlâ beklemede?" sorusu 200 madde arasında elle aranır ve pratikte hiç sorulmaz.

**Neden kanıt ortamı ayrı bir alan:** `§6.3` ve `§11` mock ile alınmış PASS'lere iki farklı sonuç bağlar (final gate'i geçemez · gerçek entegrasyona geçince geçersizleşir). Hangi maddenin mock ile PASS aldığı kaydedilmiyorsa **her iki kural da sessizce devre dışı kalır** — bu, playbook'un önlemek için var olduğu defekt sınıfının ta kendisidir.

---

## 5. Giriş kriterleri

> **Ne yazılır:** Bir doğrulamanın başlayabilmesi için sağlanması gerekenler (seviye bazlı).

**Değişmez kapılar (00 §F.1):** working tree temiz · ana dal CI son 3 run yeşil · repo hafızası güncel · hook'lar kurulu (`git config core.hooksPath`).

**Değişmez giriş kuralları:**

| # | Kural |
|---|---|
| 1 | **Ön koşulu sağlanmayan madde `BEKLEMEDE` kalır — PASS veya FAIL verilemez.** Doğrulanamayan bir maddeyi FAIL saymak yanlış sinyal, PASS saymak kanıtsız onaydır. |
| 2 | **Mock/gerçek ayrımı kayıt altındadır.** Seviye E (entegrasyon) doğrulaması, hangi dış servisin mock, hangisinin gerçek olduğu kaydedilmeden **başlatılamaz**. Kayıt yeri: matrisin "Kanıt ortamı" sütunu. |
| 3 | Task bazlı doğrulama, task'ın kabul kriterleri ve doğrulama kontrol listesi `11`'de tanımlı olmadan başlatılamaz. |

---

## 6. Çıkış kriterleri

### 6.1 Task bazlı çıkış

| Kriter | Açıklama |
|---|---|
| İlgili maddeler PASS | Task'a ait tüm doğrulama maddeleri PASS |
| KRİTİK açık bulgu yok | |
| ORTA bulgular kayıt altında | `DEFERRED_BACKLOG.md`'ye yazılmış |
| Kanıt üretilmiş | §7'ye uygun |

### 6.2 Faz bazlı çıkış

| Kriter | Açıklama |
|---|---|
| Faz içi tüm task'lar §6.1'i karşılamış | |
| Entegrasyon kontrolü yapılmış | |
| Regresyon kontrolü yapılmış | Önceki fazlar dahil |
| Açık risk envanteri güncel | Proje sahibi tarafından kabul edilmiş |
| Öğrenim terfisi yapılmış | 00 §K |

### 6.3 Final çıkış (MVP release gate)

| Kriter | Açıklama |
|---|---|
| Tüm KRİTİK maddeler PASS | Hiçbiri FAIL veya BEKLEMEDE olamaz |
| ORTA maddeler PASS veya kabul edilmiş risk | |
| DÜŞÜK maddeler bloklamaz | Ama kayıtlıdır |
| Operasyonel doğrulama tamamlanmış | Seviye F |
| Happy path uçtan uca geçmiş | |
| Çekirdek entegrasyonlar **mock olmayan** ortamda doğrulanmış | Mock PASS bu maddeler için final gate'i geçemez — BEKLEMEDE kalır |
| Kanıt arşivi erişilebilir | |
| Proje sahibi onayı | |

---

## 7. Kanıt standardı

### 7.1 Kabul edilen kanıt türleri

| Tür | Örnek |
|---|---|
| API/arayüz yanıtı | İstek + tam yanıt (durum kodu + gövde) |
| Veri kaydı | İlgili tablo satırının snapshot'ı |
| Yapılandırılmış log | Korelasyon kimliğiyle filtrelenmiş çıktı |
| Olay kaydı | Yayınlanmış/işlenmiş olay satırı |
| Test raporu | Otomatik test çıktısı (**tekrar edilebilirlik açısından tercih edilir**) |
| Dış sistem kaydı | Üçüncü parti tarafındaki işlem kaydı |
| İzleme/uyarı | Tetiklenen uyarı veya pano görüntüsü |

### 7.2 Severity'ye göre minimum kanıt

| Severity | Minimum |
|---|---|
| KRİTİK | **En az 2 farklı türde** kanıt (çok katmanlı — P3) |
| ORTA | 1 kanıt türü |
| DÜŞÜK | 1 kanıt türü |

### 7.3 Kanıt kuralları

- Kanıt **doğrulama anında** üretilmiş olmalıdır — eski veya başka ortamdan alınan kanıt geçersizdir.
- Kanıtlar madde ID'siyle eşleştirilerek saklanır.
- Tek bir arayüz gözlemi KRİTİK akışlarda tek başına yeterli değildir.
- *"Lokal makinemde geçiyor"* CI kanıtını ikame etmez.

---

## 8. Severity ve hata yönetimi

### 8.1 Sınıflandırma

| Severity | Tanım |
|---|---|
| **KRİTİK** | Veri/varlık kaybı, yanlış sahiplik, yasak durum geçişi, mükerrer tamamlanma, güvenlik ihlali. Release'i mutlak bloklar. |
| **ORTA** | Yanlış retry davranışı, eksik log, kurtarma zayıflığı, kayıp yaratmayan iş kuralı defekti. Kayıtlı risk olarak kabul edilebilir. |
| **DÜŞÜK** | Arayüz kusuru, metin hatası, operasyonu bloklamayan görünürlük sorunu. |

### 8.2 FAIL akışı

```
FAIL tespit edilir
  → Defect kaydı (madde ID, severity, beklenen vs gerçekleşen, etkilenen komşu maddeler)
  → Düzeltme (etki alanı belirlenir)
  → Yeniden doğrulama (+ regresyon — §8.3)
  → PASS → kapatılır, kanıt eklenir | Tekrar FAIL → döngüye döner
```

### 8.3 Regresyon kuralları

- KRİTİK durum-akışı veya veri-akışı düzeltmelerinde **aynı akışa ait tüm maddeler** yeniden doğrulanır. Dar kapsam kabul edilmez.
- ORTA/DÜŞÜK düzeltmelerde yalnız doğrudan etkilenen maddeler.
- **Şüphe durumunda yeniden doğrulanır.** Şüpheli maddeyi atlamak kabul edilmez.

### 8.4 Kabul edilmiş risk

Koşullar: severity **KRİTİK olamaz** · proje sahibi onayı zorunlu · risk kaydı oluşturulur (madde ID, açıklama, olası etki, mitigasyon, kabul tarihi) · `DEFERRED_BACKLOG.md`'ye yazılır · final çıkışta yeniden değerlendirilir.

### 8.5 Tıkanma yasağı

- Bir FAIL düzeltilirken bağımsız task'ların doğrulaması devam eder.
- Düzeltme **2 iterasyonda** çözülemezse proje sahibine eskale edilir: kapsam daraltma, alternatif yaklaşım veya kabul edilmiş risk.

---

## 9. Ajan çalışma modeli

| Rol | Sorumluluk | Context |
|---|---|---|
| **Coding agent** | Task'ı implement eder, testleri yazar | Kendi context'i |
| **Reviewer agent** | Doğrulama maddelerini çalıştırır, kanıt üretir, PASS/FAIL verir | **Ayrı context** — coding context'ini görmez |

Aynı ajan aynı task için iki rolü üstlenemez.

**Reviewer'a verilecek dokümanlar:** 00 §C.6 katman tablosu + task tanımı + değişen kod.
**Kaynak-güdümlü ek:** Bir maddenin kaynak sütununda referans verilen doküman katman setinde yoksa **yine de eklenir**.

---

## 10. Roller ve onay

| Karar | Onay veren |
|---|---|
| Madde PASS / FAIL | Reviewer agent (kanıtla) |
| Kabul edilmiş risk | **Proje sahibi** |
| Faz bazlı çıkış | Proje sahibi |
| Final çıkış | Proje sahibi |
| Eskalasyon | Proje sahibi |
| Regresyon kapsamını genişletme | Teknik karar verici |

**Sorumluluk sınırları:** Reviewer PASS/FAIL verir ama kabul edilmiş risk kararı **alamaz**. Coding agent kendi kodunu review edemez. Hiçbir rol tek başına final çıkış kararı veremez.

---

## 11. Yeniden doğrulama

| Tetikleyici | Kapsam |
|---|---|
| Defect düzeltmesi | FAIL alan madde + etkilenen komşu maddeler |
| Mock → gerçek entegrasyon geçişi | O entegrasyonun **tüm** maddeleri + bağımlı fonksiyonel maddeler. **Mock ile alınmış PASS'ler geçersiz sayılır.** |
| Faz geçişi | Yeni kodun etkileyebileceği önceki faz maddeleri |
| Konfigürasyon değişikliği | Değişen parametreye bağımlı maddeler |
| Altyapı değişikliği | Şema değişimi, ortam değişimi, bağımlılık güncellemesi → etkilenen seviyedeki tüm maddeler |

### 11.1 Yeniden doğrulama sonuçları

| # | Kural |
|---|---|
| 1 | Re-validation sonucu orijinal maddenin **Durum** alanını günceller. |
| 2 | **Re-validation'da FAIL çıkarsa bu bir regresyondur** — normal bir FAIL gibi işlenmez; §8 akışı **S2 Kırılma** olarak başlar. Daha önce PASS almış bir maddenin geri düşmesi, yeni bir maddenin FAIL almasından farklı bir sinyaldir. |
| 3 | **En güncel kanıt geçerlidir.** Yeni kanıt eskisinin yerine geçer; eski kanıt arşivde kalır ama geçerliliğini yitirir. Aynı madde için iki çelişen kanıt yan yana bırakılmaz. |
| 4 | Mock ile alınmış PASS'ler gerçek entegrasyona geçildiğinde **geçersiz sayılır** ve madde `BEKLEMEDE`'ye döner (§6.3). |

---

## 12. Proje-spesifik ekler

> **Ne yazılır:** Bu projenin domain'ine özel doğrulama tabloları — durum geçiş matrisi, kritik hesaplama doğrulamaları, sahiplik/yetki kontrolleri, idempotency kontrolleri, denetim izi kontrolleri.
> Bu bölüm **projeye göre tamamen değişir**; yukarıdaki §1–§11 değişmez.

---

*Shopfolio — Validation Protocol v0.1*
