# Shopfolio — Validation Protocol

**Versiyon: v0.4** | **Bağımlılıklar:** `02`–`11` | **Son güncelleme:** 2026-11-01

> **Aşama:** 10 — Doğrulama Protokolü · **Rol:** QA / Verification Lead
> **Bu bir test case deposu değildir.** Cevapladığı soru: **"Bu ürün bir sonraki aşamaya geçilebilecek olgunlukta mı?"**
> **Kapsam `10_MVP_SCOPE.md` ile birebir hizalıdır.**

> **Yazım durumu (K-28, K-30, K-1516; karar kaydı §24): yazım turu sürüyor (v0.4).** Yazım oturumları: Y1 ✓ (bu başlık notu), Y2 ✓ (çerçeve bölümleri §3, §5…§11; K-1517…K-1520); Y3a-1 ✓ (§4 matris biçimi ve KP-1…KP-30, KP-77 matrisi; K-1521…K-1523); Y3a-2, Y3b, Y4, Y5 bekliyor (harita, konvansiyonlar ve okuma tablosu park bloklarının altındadır). Sıra: başlık notu → çerçeve bölümleri (§3, §5…§11) → matris (§4) → proje-spesifik ek (§12) → §1 ve §2 son → kalite döngüsü. Bölümlerin gövdesi şablonun yönerge ve örnek metniyle durur; her oturum kendi bölümünde yönergeyi gerçek içerikle değiştirir.

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

**Park bloklarının durumu (K-1516; konu planı karar kaydı §24.2).** Dokuz park bloğu (42 madde) yazım turu bitene kadar yerinde kalır; her madde bir konuya ve bir yazım oturumuna bağlıdır, Y5 kapanışta her satırı gövdede doğrular (maddenin işi sınanıp karşılandığında bu tabloya "karşılandı" yazılır). `Pa.n` — "Aşama a'dan park edilen girdiler" bloğunun n. maddesi (dosyadaki sırayla).

| Park maddesi | Konu | Oturum | Durum |
|---|---|---|---|
| P1.1 — aktörler (K-06, K-97) | `VP1-01` | Y5 | bekliyor |
| P1.2 — MVP başarı çıtası (K-22, K-415, K-417, K-418) | `VP1-02` | Y5 | bekliyor |
| P1.3 — kabul kanıtı (K-23) | `VP12-01` | Y4 | bekliyor |
| P1.4 — kapsam cümleleri (K-24) | `VP1-01`, `VP4-01` | Y5 · Y3a-1 | §4.2 yazıldı; Y5 doğrular |
| P1.5 — iki ana sayfa düzeni (K-27) | `VP4-02` | Y3a-1 | §4.3 yazıldı; Y5 doğrular |
| P2.1 — Aşama 2 dizini | `VP0-01` | Y1 | dizin yerinde (58 atıf; Y1 doğruladı) |
| P3.1 — genişlik sınıfları (K-740, K-741) | `VP4-13` | Y3b | bekliyor (karar K-1513) |
| P3.2 — yasal kimlik bloğu ve ETBİS bandı (K-747) | `VP12-08` | Y4 | bekliyor |
| P3.3 — onay bölümü (K-756, K-831, K-833) | `VP12-08` | Y4 | bekliyor |
| P3.4 — kısmi adet senaryoları (K-787…K-796) | `VP12-09` (kuruş yuvarlaması: `VP12-04`) | Y4 | bekliyor |
| P3.5 — panelin yasal uyarıları (K-798) | `VP12-13` | Y4 | bekliyor (karar K-1514) |
| P3.6 — ayıpta sözleşmeden dönme (K-808) | `VP12-09` | Y4 | bekliyor |
| P3.7 — dışa aktarmanın izi (K-823) | `VP12-07` | Y4 | bekliyor |
| P3.8 — ilk kurulumda çerez (K-834, K-850) | `VP12-08` | Y4 | bekliyor |
| P3.9 — Aşama 3 dizini | `VP0-01` | Y1 | dizin yerinde (18 atıf; Y1 doğruladı) |
| P4.1 — indirme hakkının düştüğü an (K-884, K-980) | `VP12-10` | Y4 | bekliyor |
| P4.2 — eşzamanlılık, tek işlenme, kesinti (K-890…K-980) | `VP12-06` | Y4 | bekliyor |
| P4.3 — bağlantılar ve çerezler (K-904…K-1161) | `VP12-10` | Y4 | bekliyor |
| P4.4 — uyarılar ve hata hâlleri (K-913…K-975) | `VP12-11` | Y4 | bekliyor |
| P4.5 — tasarım değerleri, yük (K-928) | `VP12-12` (+ `VP3-02`) | Y4 | bekliyor (karar K-1515, K-1512) |
| P4.6 — on yıllık saklama (K-951, K-1092) | `VP12-07` | Y4 | bekliyor |
| P4.7 — Aşama 4 dizini | `VP0-01` | Y1 | dizin yerinde (20 atıf; Y1 doğruladı) |
| P5.1 — sayaçların değişmezi (K-1015…K-1091) | `VP12-04` (+ `VP12-06`) | Y4 | bekliyor |
| P5.2 — birim fiyat (K-1018…K-1020) | `VP12-04` | Y4 | bekliyor |
| P5.3 — tarih aralığı sınırları (K-1023, K-1041) | `VP12-04` | Y4 | bekliyor |
| P5.4 — belge kopyası ve onay anı (K-1027) | `VP12-08`, `VP12-07` | Y4 | bekliyor |
| P5.5 — kalem kayıtlarının ve geri ödemenin değişmezleri (K-1054…K-1131) | `VP12-09` | Y4 | bekliyor |
| P5.6 — hesap tarafının silmeleri (K-1064, K-1065, K-1083) | `VP12-05` | Y4 | bekliyor |
| P5.7 — Aşama 5 dizini | `VP0-01` | Y1 | dizin yerinde (22 atıf; Y1 doğruladı) |
| P6.1…P6.5 — uçların doğrulanacak davranışları (K-1163…K-1203, K-1213, K-1217) | `VP4-18` (P6.1 ayrıca `VP12-05`) | Y3b | bekliyor |
| P6.6, P6.7 — hata kodları, girdi modelleri, derin gözden geçirme (K-1196…K-1216) | `VP4-19` | Y3b | bekliyor |
| P7.1 — dış servislerin davranışları (K-1257…K-1291) | `VP4-15` | Y3b | bekliyor |
| P8.1 — ön yüzün tarayıcıda doğrulanması (K-1325, K-1354, K-1355) | `VP4-16` (+ `VP3-02`) | Y3b | bekliyor |
| P8.2 — çatının kendi yanıtları (K-1334) | `VP4-17` | Y3b | bekliyor |
| P9.1 — vitrinin eksik satır kuralı (K-755, K-841) | `VP4-14` | Y3b | bekliyor |
| P9.2 — erişilebilirlik taraması (K-625, K-1411) | `VP12-02` (+ `VP3-02`) | Y4 | bekliyor |
| P9.3 — F6 kabul kanıtının senaryolara bağlanışı (K-23, K-1453) | `VP12-01` | Y4 | bekliyor |

Aşama 1 ve Aşama 6…9 bloklarında dizin satırı yoktur ve eklenmedi (devrin evi karar satırıdır). Dizin satırlarında Y1'de değişiklik gerekmedi: dizin ile etki sütunu açılışta karşılaştırıldı (karar kaydı §24.1 fark C, K-1508); Y1 dört dizinin gösterdiği raporların (`CHECKPOINT_REPORTS/PHASE2…5_CONFLICT_SCAN.md` §6) varlığını sınadı ve satırların yalnız işaret ettiğini, devri ikinci kez kaydetmediğini doğruladı.

**Yazım konvansiyonları (K-1516; checklist §4).** (1) **Sıra:** başlık notu (Y1) → çerçeve bölümleri §3, §5…§11 (Y2) → §4 matris (Y3a-1, Y3a-2, Y3b) → §12 proje-spesifik ek (Y4) → §1, §2 son (Y5; sayımlar matris bitince kesinleşir) → özet ↔ ayrıntı taraması → kalite döngüsü. (2) **Dil:** bu doküman Türkçe; kod, sembol ve dosya adları İngilizce. (3) **Kimlikler:** konu `VPn-mm` (karar kaydı §24.2); matris satırı `VAL-<seviye><NNN>` (örnek `VAL-A001`) — seviye kimlikte okunur ve "Seviye" sütunuyla çelişemez (K-1511 (2)); kaynak birimi `10` KP, KD, ÖK, SK, YH kimliği ya da K / bölüm numarasıdır. Severity (KRİTİK / ORTA / DÜŞÜK) ile `00 §D.10`'un S1 / S2 / S3 hata sınıfı iki ayrı eksendir (K-1511 (3)). (4) **Atıf biçimi:** `02 §3.8.3`, `10 KP-37`, `09 §16.1` — önsüz `§` bu dokümanın bölümüdür; başka doküman her zaman numarasıyla anılır. (5) **Tüketici ilkesi:** bu doküman `02`–`11`'i tüketir, onlara geriye etki yaratmaz; kaynakla çelişen bir kural çıkarsa kaynak kazanır ve fark bu dokümana hizalama olarak yazılır. (6) **Kural ayrıntısı evinde kalır:** satır kuralı kopyalamaz, kimlikle işaret eder (kapsam cümleleri hariç: `10 §2` ve `§3` cümleleri çevrilmeden devralınır, K-24). (7) **Kapsam hizası:** MVP'de olmayan özellik kriter olamaz, MVP'de olan çıkarılamaz; KD ve YH satırlarına kriter yazılmaz. (8) **Tür etiketi (K-1512 (2)):** matris satırının `Kural` hücresi gerektiğinde köşeli etiketle başlar — `[tarayıcı]`, `[erişilebilirlik]`, `[güvenlik]`, `[yük]`; yeni seviye ya da sütun açılmaz. (9) **Açık kararlar bölümü yoktur:** açık kalem karar kaydının §4'ündedir, sonraki dokümana talimat o dokümanın park bloğundadır (PF-158). (10) **Betik:** matris kimlik tekilliği, seviye–kimlik uyumu ve KP kapsaması `scripts/doc-scans/12_matris_denetle.py` ile denetlenir (ilk matris oturumu yazar, sonrakiler koşar); kapsam hizası `12_kapsam_hizasi.py` ile yazımdan önce sınandı, kapanışta yeniden koşulur.

**Okuma tablosu ve boyut (K-1511 (1), K-1516).** Bu doküman 100 KB'ı aşacaktır ve `INSTRUCTIONS §9` (7) bütün okumayı yasaklar; `validate` skill'i ve doğrulayıcılar hedef task'ın VAL satırlarını bu tabloyla okur, bütün dosyayı okumaz. Dosyanın boyutu `wc -c` ile ölçülür; 100 KB'ın altında bütün okunabilir. Komutlar depo kökünden, dosya `Docs/12_VALIDATION_PROTOCOL.md`:

- **Bölüm ve alt bölüm başlıkları:** `grep -n '^## \|^### ' <dosya>` — satır numaraları; bölüm `sed -n 'a,bp'` ya da `Read` ile aralıkla okunur.
- **Tek matris satırı:** `grep -n '^| VAL-C012 |' <dosya>` — satırın ilk hücresi kimliktir.
- **Bir seviyenin satırları:** `grep -n '^| VAL-C' <dosya>` — kimlik seviyeyi taşır.
- **Bir kaynağın satırları:** `grep -nE 'KP-37([^0-9]|$)' <dosya>` — kaynak hücresindeki kimlik; sonuna rakam gelen kimlikler (KP-370 gibi) ayıklanır. Task için: `11` §7.1'deki task'ın kaynak birimleri aynı komutla aranır.
- **Tür etiketi:** `grep -nF '[yük]' <dosya>` (ve `[tarayıcı]`, `[erişilebilirlik]`, `[güvenlik]`) — türün bütün satırları.
- **Durum:** `grep -n 'BEKLEMEDE' <dosya>`; `grep -nE '\| (FAIL|KABUL EDİLMİŞ RİSK) \|$' <dosya>`.
- **Park maddesi:** `grep -n '^> - \*\*K-' <dosya>`.
- **Proje-spesifik ek:** `grep -n '^### 12\.' <dosya>` — alt bölüm başlıkları (başlıklar Y4'te yazılır).
- **Kaynak satırı ve aralığı:** `grep -n '^| KP-37 ' Docs/10_MVP_SCOPE.md`; başka kaynak için `grep -n '<kimliğin başlığı>' Docs/0N_*.md`, sonra `sed -n 'a,bp'` — yalnız satırın kaynak birimi.

**Boyut hedefi yoktur;** doküman bölünmez (dosya adı ve bölüm numaraları değişmez; PF-144'ün `12` karşılığı). Büyüme matris ve ekten gelir; okuma tablosu bunu karşılar.

**Yazım turunun haritası (K-1516).** Yazım oturumları beş tanedir, matris iki parçadır (Y3a, Y3b; Y3a baştan ikiye bölündü — Y3a-1, Y3a-2, K-1522); harita burada sabittir, bir oturum bölünürse karar satırıyla tabloya ve ona atıf yapan satırlara aynı PR'da işlenir. **Bölüm çizgisi konu sırasıyla çekilir:** Y3a, KP gruplarının matrisini (`VP4-02`…`VP4-11`, KP-1…KP-77) ve matris biçimini (`VP4-01`) yazar; Y3b, KP kümesinin dışında kalan teknik kesitleri (`VP4-12`…`VP4-19`: KP-37'nin arama motoru verileri, genişlik sınıfları, vitrin kuralı, seviye E, ön yüz, çatının yanıtları, uçlar, hata kodları) yazar. Çizgi KP gruplarıyla teknik kesit arasındadır, çünkü Y3a'nın çıktısı KP kapsamasını betikle sınar (KP-37 hariç: onun satırı `VP4-12` ile Y3b'de gelir) ve Y3b uç ve hata kodu satırlarını `07`'den çeker. Y3a 250k bağlama sığmayacağı için baştan yedek çizgiyle ikiye bölündü: `VP4-06`/`VP4-07` (31 KP + 46 KP; KP dağılımı konu planındaki gibi) — Y3a-1 KP-1…KP-30 ve KP-77'yi, Y3a-2 KP-31…KP-76'yı (KP-37 hariç) yazar. Boyut tahminleri (K-32 — tahmin, kural değil) oturumun ekleyeceği metindir.

| Oturum | Kapsam | Konu | Tahmini boyut | Durum |
|---|---|---|---|---|
| Y1 | Başlık notu: sürüm satırı, yazım durumu, park bloklarının durumu, konvansiyonlar, okuma tablosu, harita; dizin satırları | `VP0-01`, `VP0-02` | +11 KB | ✓ |
| Y2 | §3 seviyeler (+ W-1 etiketleri), §5 giriş, §6 çıkış, §7 kanıt, §8 severity, defect ve risk, §9 ajan modeli, §10 roller, §11 yeniden doğrulama | `VP3-01`, `VP3-02`, `VP5-01`, `VP6-01`, `VP7-01`, `VP8-01`…`VP8-03`, `VP9-01`, `VP10-01`, `VP11-01` | +35 KB | ✓ |
| Y3a-1 | §4 matris biçimi (§4.2) ve KP gruplarının matrisi, birinci yarı: KP-1…KP-30 ve KP-77; `12_matris_denetle.py` yazıldı (K-1521…K-1523) | `VP4-01`…`VP4-06` | +66 KB (gerçekleşen 66 KB, 186 satır) | ✓ |
| Y3a-2 | KP gruplarının matrisi, ikinci yarı: KP-31…KP-76 (KP-37 hariç); betik `--kapsam` argümanıyla koşar | `VP4-07`…`VP4-11` | +44 KB | bekliyor |
| Y3b | §4 matris devamı: KP-37, genişlik sınıfları, vitrin kuralı, seviye E, ön yüz, çatı, uçlar, hata kodları | `VP4-12`…`VP4-19` | +70 KB | bekliyor |
| Y4 | §12 proje-spesifik ek; F6 kabul kanıtı senaryolarının `11` T601 kabul kriteri 10'a geri yazımı | `VP12-01`…`VP12-13` | +60 KB | bekliyor |
| Y5 | §1 kapsam, §2 prensipler; yazım turunun kapanışı (park bloklarının son durumu, kapsam hizası betiği yeniden) | `VP1-01`…`VP1-04`, `VP2-01` | +20 KB | bekliyor |

Tahminle toplam yaklaşık 360 KB'a ulaşır.

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

> **Not:** Doğrulama **seviyeleri** (bu bölüm) ile kalite **kapıları** (`00 §D.6`) farklı eksenlerdir. Seviyeler *neyin* doğrulandığını, kapılar *ne zaman* doğrulandığını söyler. Seviye, matris satırının kimliğinde okunur (`VAL-C012` — C seviyesi) ve "Seviye" sütunuyla çelişemez (K-1511 (2)). Bu bölümün kararları K-1517'dedir (`VP3-01`) ve K-1512'dir (`VP3-02`).

### 3.1 Seviyeler

| Seviye | Soru | Ne zaman tetiklenir | Kaynak |
|---|---|---|---|
| **A — Gereksinim** | `10 §2` kapsam cümleleri ve `02`'nin iş kuralları gerçekten karşılanıyor mu? | İlgili task bitince (task doğrulaması); sonraki fazların gate'inde regresyon olarak yeniden | `02`, `10`; `[erişilebilirlik]` satırlarında `04` |
| **B — Fonksiyonel** | Akışlar uçtan uca çalışıyor mu — servis düzeyinde (F3) ve ekran düzeyinde (F5)? | Task doğrulaması ve faz gate'i (F3'te ilk uçtan uca akış, F5'te betiksiz yol) | `02`, `03`, `04`, `07`, `10` |
| **C — Durum makinesi** | İzinli geçişler çalışıyor, yasak geçişler engelleniyor mu? | Bir durum makinesine dokunan her task (sipariş, ödeme, iptal–cayma–iade…); F3 gate'inde geçiş tablosunun eşitliği | `02 §5.4`, `§5.5`; `03`; `06` |
| **D — Veri** | Veri doğru saklanıyor, dönüşüyor, siliniyor ve saklama süresi dolunca imha ediliyor mu? | Veri katmanı task'ları (F1) ve veriye dokunan her task; göç ve saklama senaryolarında F1 gate'i | `06`, `05`, `09` |
| **E — Entegrasyon** | Dış servislerle — ödeme sağlayıcısı, e-posta altyapısı, Google — sözleşme doğru mu? | Entegrasyon task'ları (F4) ve bir dış servisin sonucuna dayanan her task; F4 gate'i | `08`; makine uçları için `07` |
| **F — Operasyonel** | Sistem gerçek koşullarda ayakta kalıyor mu — kurulum, yedek, canlılık, yük, güvenlik taraması, canlı kanıt? | Faz sonu (gate) ve final çıkış; sürüm referans kuruluma indiğinde açılış provası | `05`, `08`, `09`, `DEPLOY_RUNBOOK` |

Her satır **tek** seviyededir: aynı kuralın iki seviyede satırı olmaz (K-1512). Bir kural hem iş kuralı hem veri davranışı taşıyorsa satır, sınadığı şeye göre seçilir — kuralın kendisi A'da, saklanışı D'dedir ve iki ayrı kuraldır.

### 3.2 Seviye ve kapı

Kapılar `00 §D.6` ve §E.2'dedir ve burada tekrarlanmaz; tablo hangi kapının hangi seviyeleri okuduğunu söyler.

| Kapı | Okunan satırlar | Kural evi |
|---|---|---|
| **Katman 1 — task doğrulaması** | Task'ın kabul kriterlerine ve kaynak birimlerine bağlı A–E satırları; her kabul kriteri en az bir satıra denk gelir | `11` §5 "Task yapısı" madde 5, §7.1; `skills/validate` |
| **Katman 2 — PR / CI** | Satır değildir: CI, satırın **kanıtı** olan otomatik testleri koşar; CI yeşili satırın yerine geçmez | `00 §D.6`; `INSTRUCTIONS §3.3` |
| **Katman 3 — faz gate'i** | Önceki fazlar dahil A–E regresyonu; fazın kapı ölçütlerinin satırları (§6.2); F satırları; ayrı chat'te | `00 §E.2`; `11 §6`; `skills/gate-check` |
| **Final çıkış** | Bütün satırlar (§6.3) | §6.3 |

### 3.3 Seviye dışı doğrulama türleri

Tarayıcıda ön yüz, erişilebilirlik, güvenlik ve yük A–F'nin bir "neyin"i değil bir "nasıl"ıdır. **Yeni seviye ve yeni matris sütunu açılmaz** (K-1512 (1)); dört tür, matriste `Kural` hücresinin başındaki köşeli **tür etiketiyle** taşınır. Bir satır en çok bir etiket taşır; iki tür birlikte sınanıyorsa iki satır yazılır. Etiket betikle sayılır (tür başına satır, seviye dağılımı). Türlerin satırları §4 matrisinde kendi KP gruplarının yanında durur; yalnız projeye özgü kurulum — yük senaryosu ve yasal uyarıların senaryo biçimi — §12'dedir (`VP12-12`, `VP12-13`).

| Etiket | Ne sınar | Seviye | Hangi kapıda okunur | Ayrıntının evi |
|---|---|---|---|---|
| `[tarayıcı]` | Tarayıcıda koşan ön yüz davranışı ve yerleşim: ortak betik, sayfa modülleri, betikli ve betiksiz yol, genişlik sınıfları (K-1325, K-1354, K-1355) | **B**; iş kuralının kendisi tarayıcıda gösterilen satır **A** | Ekranın task doğrulaması (F5); betiksiz yol F5 gate'i | §4 (`VP4-13`, `VP4-14`, `VP4-16`); `09 §16.1`, §19.2 |
| `[erişilebilirlik]` | Kontrol Listesi A seviyesi ve WCAG 2.2 A maddeleri — bir gereksinimdir (KP-71) | **A** | A seviyesi tarama F5 gate'inde koşar ve temiz çıkmalıdır (K-625, K-1411); bileşen tabanı T540, T543, T544 | §12 (`VP12-02`); `11 §6.2` F5 |
| `[güvenlik]` | Yetki, oturum ve veri koruma davranışı; F6'nın güvenlik ve bağımlılık taraması | Tek seviyeye bağlanmaz: davranış kendi konusunun seviyesindedir (A, C, D); F6 taraması **F** | Davranış satırları task doğrulamasında; tarama F6 gate'inde | §4 (ilgili KP grupları); `11 §6.2` F6 |
| `[yük]` | H-3 ve H-4 tasarım değerleri (K-928) — elli kalemli sepet, dakikada yüz ziyaretçi, referans sunucu ölçüsü | **F** | Final çıkışın seviye F koşulu; koşu F5 gate'i PASS aldıktan sonra yapılır (§6.3) | §12 (`VP12-12`, k6 — K-1515) |

**Kanıt ve zamanlama bu etiketlerin kimliğinde yazılmaz:** kanıt ortamı §7.4'te, kapıya bağ §6'dadır; tarayıcı akışlarının listesi ve hangi akışın hangi tarayıcıda sınanacağı `VP4-16`'dadır (taban: son iki büyük sürüm — `05 §3.8.4`).

---

## 4. Doğrulama matrisi

Her doğrulama maddesi bir satırdır. Matris iki katmanda yazılır: biçim ve KP gruplarının satırları (`VP4-01`…`VP4-06`, K-1521…K-1523) ve devamı. Satır yazım kuralları §4.2'dedir; satırlar §4.3'ten itibaren kaynak grubuna göre durur ve tek bir ortak sütun düzenini paylaşır.

| Sütun | Açıklama |
|---|---|
| **ID** | Benzersiz tanımlayıcı: `VAL-<seviye><NNN>` (örnek `VAL-A001`); seviye kimlikte okunur (K-1511 (2)) |
| **Seviye** | A–F (§3); kimlikteki harfle aynıdır |
| **Kural / Gereksinim** | Doğrulanacak iş kuralı veya davranış; gerektiğinde başında tür etiketi (§3.3) |
| **Kaynak** | Kuralın tanımlandığı kaynak birimi: `10` KP (ÖK, SK) kimliği ve `02`–`09` bölümü ya da K numarası |
| **Ön koşul** | Doğrulama öncesi sağlanması gereken durum |
| **Beklenen sonuç** | PASS için sağlanması gereken koşul |
| **Kanıt türü** | Kabul edilen kanıt(lar) — §7.1; KRİTİK satırda en az iki tür `+` ile |
| **Kanıt ortamı** | `mock` / `sandbox` / `gerçek` / `yok` — kanıtın dış servisin hangi hâlinde üretildiği (§7.4) |
| **Severity** | KRİTİK / ORTA / DÜŞÜK — §8 |
| **Durum** | `BEKLEMEDE` / `PASS` / `FAIL` / `KABUL EDİLMİŞ RİSK` |

Satırların tablo başlığı her bölümde aynıdır:

`| ID | Seviye | Kural | Kaynak | Ön koşul | Beklenen sonuç | Kanıt türü | Kanıt ortamı | Severity | Durum |`

### 4.1 Durum tanımları

| Durum | Anlamı |
|---|---|
| `BEKLEMEDE` | Ön koşulu sağlanmadığı için **henüz doğrulanamadı**. PASS da FAIL da değildir. |
| `PASS` | Kanıtla doğrulandı |
| `FAIL` | Beklenen sonuç sağlanmadı — §8 akışı başlar |
| `KABUL EDİLMİŞ RİSK` | FAIL ama proje sahibi onayıyla kayda alındı (§8.4). KRİTİK olamaz. |

**Neden ayrı bir alan:** `§6.3` final çıkışı *"hiçbir KRİTİK madde FAIL veya BEKLEMEDE olamaz"* der. Durum bir alanda tutulmuyorsa bu soru **mekanik olarak cevaplanamaz** — "hangi maddeler hâlâ beklemede?" sorusu 200 madde arasında elle aranır ve pratikte hiç sorulmaz.

**Neden kanıt ortamı ayrı bir alan:** `§6.3` ve `§11` mock ile alınmış PASS'lere iki farklı sonuç bağlar (final gate'i geçemez · gerçek entegrasyona geçince geçersizleşir). Hangi maddenin mock ile PASS aldığı kaydedilmiyorsa **her iki kural da sessizce devre dışı kalır** — bu, playbook'un önlemek için var olduğu defekt sınıfının ta kendisidir.

### 4.2 Satır yazım kuralları

Kararlar K-1521'dedir (`VP4-01`). Kurallar matrisin bütün bölümleri (§4.3 ve sonrası) için geçerlidir; sonraki matris oturumları aynı kuralları uygular.

1. **Birim.** Bir `10 §2` KP satırı çeviri yapılmadan devralınır (K-24): satırın doğrulanabilir her kural parçası ayrı bir matris satırıdır. Bir parçanın olumsuz hâli — olmaması gereken davranış — kendi satırıdır. Bir satır tek seviyededir (§3.1).
2. **Kimlik.** `VAL-<seviye><NNN>`; harf seviyeyi, üç haneli sayı o seviyenin sırasını verir. Numaralar seviye başına 001'den, dokümandaki sırayla verilir; bir kimlik hiçbir zaman yeniden kullanılmaz ve satır silinirse numarası boş kalır. Sonraki matris oturumu her seviyede son numaradan devam eder; tekillik betikle sınanır.
3. **Kural hücresi.** KP cümlesinin ilgili parçasını anlamını değiştirmeden taşır; kuralın ayrıntısı (süre, tutar, durum geçişi) `Kaynak` hücresindeki bölümde kalır ve burada kopyalanmaz. Tür etiketi (§3.3) hücrenin başına yazılır.
4. **Kaynak hücresi.** `10 KP-n` biçiminde kaynak birimi, ardından kuralı tanımlayan `02`–`09` bölümü ya da K numarası. KD ve YH kimliği hiçbir satırda yazılmaz (kapsam hizası); ÖK kimliği yalnız satırın ön koşulunu dış ön koşula bağlıyorsa yazılır.
5. **Seviye seçimi.** **A:** kapsam cümlesinin kuralı, ekranda ya da uçta gözlenen davranış. **B:** uçtan uca akış. **C:** durum geçişi — izinli geçiş ya da yasak geçişin engellenmesi. **D:** verinin saklanışı, dondurulması, silinmesi. Seviye E (dış servis sözleşmesi) ve seviye F (operasyonel) satırları ayrı konulardadır (`VP4-15`, `VP12-12`); bu bölümlerin kart, e-posta ve Google ile ilgili satırları sözleşmeyi değil, ürünün o servisin sonucuna verdiği tepkiyi sınar.
6. **Severity (§8.1'in uygulanışı).** KRİTİK: tutarı ya da para hareketini, yasal metni ve cayma, iade, fesih sürelerini, yetkiyi ve başka müşterinin verisini, kişisel verinin silinmesini, yasak durum geçişini doğrudan etkileyen kural. ORTA: kayıp yaratmayan iş kuralı. DÜŞÜK: görünürlük ve metin.
7. **Kanıt türü.** §7.1'in tür adlarıyla; KRİTİK satırda en az iki farklı tür `+` ile yazılır (§7.2); birincisi sonucun kendisini, ikincisi yan etkisini gösterir.
8. **Kanıt ortamı.** Dış servisin sonucuna dayanan satır — ödeme sağlayıcısı, e-posta altyapısı, Google — `mock` yazar ve final çıkışta çekirdek entegrasyon kanıtı sayılmaz (§6.3); öteki bütün satırlar `yok`. `sandbox` ve `gerçek` değerleri seviye E ve canlı kanıt satırlarının işidir.
9. **Ön koşul.** Boş bırakılmaz; ön koşulu sağlanmayan satır `BEKLEMEDE` kalır (§5 kural 1).
10. **Denetim.** `python -I scripts/doc-scans/12_matris_denetle.py . --kapsam <KP aralığı>` kimlik tekilliğini, seviye–kimlik uyumunu, değer kümelerini, KRİTİK satırın iki kanıt türünü, KD ve YH yasağını ve verilen KP kümesinin kapsamasını sınar.

### 4.3 Vitrin ve katalog (KP-1…KP-7; `VP4-02`)

Kaynak: `10 §2` grup "Vitrin ve katalog"; K-27 (iki ana sayfa düzeni, P1.5). İki ana sayfa düzeni ayrı ayrı doğrulanır; vitrinin eksik satır kuralı ve genişlik sınıfları ayrı konulardadır (`VP4-14`, `VP4-13`).

| ID | Seviye | Kural | Kaynak | Ön koşul | Beklenen sonuç | Kanıt türü | Kanıt ortamı | Severity | Durum |
|---|---|---|---|---|---|---|---|---|---|
| VAL-A001 | A | Ziyaretçi ürünleri en fazla üç seviyeli kategori ağacında gezerek bulur. | 10 KP-1 · 02 §3.4, §11 P-20 | Her seviyesinde yayında ürün olan üç seviyeli kategori ağacı | Menü üç seviyeyi bütünüyle gösterir; her seviyeden kategori sayfasına inilir ve ürüne ulaşılır | Tarayıcı gözlemi | yok | ORTA | BEKLEMEDE |
| VAL-A002 | A | Kategori sayfası alt dallarındaki ürünleri de listeler. | 10 KP-1 · 02 §3.4, §3.28.6 | Üst kategorinin altında iki alt seviyede yayında ürünler | Üst kategorinin sayfası kendi ve alt dallarının yayındaki ürünlerinin hepsini listeler | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-A003 | A | Yayında ürünü olmayan kategori menüde görünmez. | 10 KP-1 · 02 §3.4 | Kendisinde ve alt dallarında yayında ürün olmayan kategori | Kategori menüde yoktur; içinde ürün yayına alınınca menüde görünür | Uç yanıtı | yok | DÜŞÜK | BEKLEMEDE |
| VAL-A004 | A | Ziyaretçi ürün adında arama yapar. | 10 KP-2 · 02 §3.5 | Adları farklı yayında ürünler | Arama sözcüğünü ürün adında taşıyan yayındaki ürünler döner, öteki ürünler dönmez | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-A005 | A | Arama büyük-küçük harfe ve Türkçe karakterlere duyarsızdır. | 10 KP-2 · 02 §3.5 | Adında Türkçe karakter (ç, ğ, ı, İ, ö, ş, ü) taşıyan yayında ürün | Aynı sözcüğün büyük harfli, küçük harfli ve Türkçe karaktersiz yazımlarının hepsi aynı ürünü getirir | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-A006 | A | Ziyaretçi ürün listesini fiyat aralığına göre süzer. | 10 KP-3 · 02 §3.5 | Fiyatları farklı yayında ürünler | Liste yalnız fiyatı seçilen aralığa giren ürünleri gösterir | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-A007 | A | Ziyaretçi ürün listesini stok durumuna göre süzer. | 10 KP-3 · 02 §3.5 | Satın alınabilir ve tükenmiş ürünler | Liste yalnız seçilen stok durumundaki ürünleri gösterir | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-A008 | A | Liste tek bir sabit düzendedir ve en yeni ürün önce gelir. | 10 KP-3 · 02 §3.5 | Farklı zamanlarda eklenmiş yayında ürünler | Ürünler en yeniden eskiye gelir; ziyaretçiye sıralama seçeneği sunulmaz | Uç yanıtı | yok | DÜŞÜK | BEKLEMEDE |
| VAL-A009 | A | Ziyaretçi ürünün varyantını — en fazla iki seçenek boyutu — ürün sayfasında seçer. | 10 KP-4 · 02 §3.3, §3.6, §11 P-21 | İki seçenek boyutlu (renk × beden) yayında ürün | Ürün sayfası her boyut için seçim sunar; seçilen kombinasyon seçilir ve sepete eklenebilir | Tarayıcı gözlemi | yok | ORTA | BEKLEMEDE |
| VAL-A010 | A | Vitrinde ürün başına tek kart vardır, varyant başına kart yoktur. | 10 KP-4 · 02 §3.6 | Birden çok varyantlı yayında ürün | Kategori listesinde, aramada ve ana sayfa vitrininde ürün tek kartla görünür | Tarayıcı gözlemi | yok | DÜŞÜK | BEKLEMEDE |
| VAL-A011 | A | Tükenmiş ya da firmanın açmadığı bir kombinasyon görünür ama seçilemez. | 10 KP-4 · 02 §3.6 | Bir kombinasyonu tükenmiş, biri hiç açılmamış varyantlı ürün | İki kombinasyon da görünür; ikisi de seçilemez ve sepete eklenemez | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-A012 | A | Ürün sayfasında görseller ve açıklama görünür. | 10 KP-5 · 02 §3.6 | Görselli ve açıklamalı yayında ürün | Görseller ve açıklama ürün sayfasında yer alır | Tarayıcı gözlemi | yok | DÜŞÜK | BEKLEMEDE |
| VAL-A013 | A | Ürün sayfasında fiyat KDV dahil gösterilir. | 10 KP-5 · 02 §3.6, §3.8 | KDV oranı tanımlı yayında ürün | Gösterilen fiyat KDV dahil tutardır; tutar ürünün kayıtlı fiyatıyla eşleşir | Uç yanıtı + Tarayıcı gözlemi | yok | KRİTİK | BEKLEMEDE |
| VAL-A014 | A | Fiyatın uygulanmaya başlandığı tarih ürün sayfasında görünür. | 10 KP-5 · 02 §3.8, §3.9 | Fiyatı belirli bir tarihte uygulanmaya başlamış ürün | Fiyatın yanında uygulanmaya başlandığı tarih yazar | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-A015 | A | Üretim yeri fiziksel üründe, dijital üründe de doluysa görünür. | 10 KP-5 · 02 §3.6, §3.11 | Üretim yeri dolu fiziksel ürün; üretim yeri dolu ve boş birer dijital ürün | Fiziksel üründe üretim yeri yazar; dijital üründe yalnız doluysa yazar | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-A016 | A | Üretim yeri Türkiye olan fiziksel üründe fiyatın yanında Bakanlığın yerli üretim logosu görünür; firma logoyu kapatamaz. | 10 KP-5 · 02 §3.11 | Üretim yeri Türkiye olan ve olmayan fiziksel ürünler | Türkiye üretimli üründe fiyatın yanında logo vardır ve panelde kapatma seçeneği yoktur; öteki üründe logo yoktur | Tarayıcı gözlemi | yok | ORTA | BEKLEMEDE |
| VAL-A017 | A | Ölçüyle satılan üründe firmanın girdiği ölçü birimi ve net miktardan birim fiyat görünür. | 10 KP-5 · 02 §3.8 | Ölçü birimi ve net miktar girilmiş ürün | Ürün sayfasında birim fiyat görünür; ölçüsüz üründe görünmez | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-A018 | A | İndirimdeki üründe referans fiyat ve indirimin başlangıç ve bitiş tarihi ürün sayfasında ve vitrin kartında görünür. | 10 KP-5 · 02 §3.9 | İndirimi sürmekte olan yayında ürün | Ürün sayfasında ve vitrin kartında referans fiyat, indirimin başlangıç tarihi ve bitiş tarihi görünür | Uç yanıtı + Tarayıcı gözlemi | yok | KRİTİK | BEKLEMEDE |
| VAL-A019 | A | Stok adedi gösterilmez; ürün ya satın alınabilir ya "Tükendi"dir. | 10 KP-5 · 02 §3.6 | Stoğu olan ve stoğu biten ürün | Hiçbir sayfada ve yanıtta stok adedi yoktur; stoğu biten ürün "Tükendi" gösterir | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-B001 | B | Ziyaretçi üç tipte ürün satın alır; üçü aynı kataloğa girer ve aynı sepetten geçer. | 10 KP-6 · 02 §3.2 | Yayında birer fiziksel, dijital ve hizmet ürünü | Üç ürün aynı katalogda listelenir, tek sepete eklenir ve tek siparişte onaylanır | Uç yanıtı + Veri kaydı | yok | ORTA | BEKLEMEDE |
| VAL-A020 | A | Hizmet, ödeme onayından itibaren en geç kaç gün içinde ifa edileceğini söyleyen ifa süresi taşır; süre vitrinde ve Ön Bilgilendirme Formu'nda yazar. | 10 KP-6 · 02 §3.2, §3.20.10 | İfa süresi girilmiş hizmet ürünü | İfa süresi ürün sayfasında ve sepet onay bölümündeki Ön Bilgilendirme Formu'nda aynı değerle yazar | Tarayıcı gözlemi + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-A021 | A | Arşivlenmiş ürünün adresinde fiyatsız bir "Bu ürün artık satılmıyor" sayfası görünür. | 10 KP-7 · 02 §3.7 | Arşivlenmiş ürün | Adres fiyatsız "Bu ürün artık satılmıyor" sayfasını verir; ürün sepete eklenemez | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-A022 | A | Taslak ürünün adresi "sayfa bulunamadı" sayfasına düşer. | 10 KP-7 · 02 §3.7 | Taslak ürün | Adres "sayfa bulunamadı" sayfasını verir | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-A023 | A | Silinmiş ve adresi yeni bir kayda verilmemiş ürünün adresi "sayfa bulunamadı" sayfasına düşer. | 10 KP-7 · 02 §3.7, §3.30.2 | Silinmiş ürün | Adres "sayfa bulunamadı" sayfasını verir | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-D001 | D | Kalıcı silinen ürünün adresi serbest kalır ve aynı adla sonradan açılan kayıt onu alabilir. | 10 KP-7 · 02 §3.30.2, §3.30.4 | Kalıcı silinmiş ürünün adı | Aynı adla açılan yeni ürün silinen ürünün adresini alır | Veri kaydı + Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-A024 | A | Yayındaki ürünün taslak varyantı seçenek listesinde ziyaretçiye görünmez. | 10 KP-7 · 02 §3.7 | Bir varyantı taslak olan yayında ürün | Taslak varyant seçenek listesinde yoktur | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-B002 | B | [tarayıcı] Firma tanıtım öncelikli ana sayfa düzenini seçtiğinde kurumsal blok ilk görülür ve ürün vitrini en çok dört kayıt taşır. | K-27 · P1.5 · 04 §5.1.2, §5.1.6 | Panelde tanıtım öncelikli düzen seçili; beşten çok yayında ürün | Ana sayfada ilk blok kurumsal bloktur; ürün vitrini en çok dört ürünle ve kurumsal bloğun altında durur | Tarayıcı gözlemi | yok | ORTA | BEKLEMEDE |
| VAL-B003 | B | [tarayıcı] Firma mağaza öncelikli ana sayfa düzenini seçtiğinde ürün vitrini ilk görülür ve en çok sekiz kayıt taşır. | K-27 · P1.5 · 04 §5.1.2, §5.1.6 | Panelde mağaza öncelikli düzen seçili; sekizden çok yayında ürün | Ana sayfada ilk blok ürün vitrinidir ve en çok sekiz ürün taşır; kurumsal blok vitrinin altındadır | Tarayıcı gözlemi | yok | ORTA | BEKLEMEDE |
| VAL-A025 | A | Her iki düzende de kurumsal tanıtım ve ürün vitrini ana sayfada birlikte bulunur; değişen yalnız ağırlık ve sıradır. | K-27 · P1.5 · 04 §5.1 | Her iki düzen sırayla seçilir | İki düzende de kurumsal blok ve ürün vitrini ana sayfada vardır; hiçbir düzen birini çıkarmaz | Tarayıcı gözlemi | yok | ORTA | BEKLEMEDE |
| VAL-B004 | B | Firmanın panelde yaptığı düzen seçimi ana sayfaya yansır. | K-27 · P1.5 · 04 §5.1 | Panel oturumu; bir düzen seçili | Düzen değiştirilip kaydedilince ana sayfa yeni düzenle açılır | Tarayıcı gözlemi + Veri kaydı | yok | ORTA | BEKLEMEDE |

### 4.4 Sepet, ödeme ve sipariş (KP-8…KP-17; `VP4-03`)

Kaynak: `10 §2` grup "Sepet, ödeme ve sipariş". Kart ve havale satırları ödeme sağlayıcısını sahte uçla (`mock`) sınar; sağlayıcının sözleşmesi `VP4-15`'te, gerçek kartlı zincir `VP12-01`'dedir. Onay bölümünün yerleşim ve işaret ayrıntısı (K-756, K-831, K-833) ile eşzamanlılık senaryoları §12'dedir.

| ID | Seviye | Kural | Kaynak | Ön koşul | Beklenen sonuç | Kanıt türü | Kanıt ortamı | Severity | Durum |
|---|---|---|---|---|---|---|---|---|---|
| VAL-B005 | B | Ziyaretçi hesap açmadan sipariş verir. | 10 KP-8 · 02 §3.13.1 | Oturumsuz ziyaretçi; sepette satın alınabilir ürün | Hesap açılmadan ödeme adımına gelinir ve sipariş oluşur; ziyaretçi için hesap kaydı oluşmaz | Uç yanıtı + Veri kaydı | yok | ORTA | BEKLEMEDE |
| VAL-A026 | A | Misafir alıcının e-postası onay özetinde açıkça gösterilir. | 10 KP-8 · 02 §3.24.5 | Misafir alıcı; ödeme adımı | Onay özeti alıcının girdiği e-postayı gösterir | Tarayıcı gözlemi | yok | ORTA | BEKLEMEDE |
| VAL-A027 | A | Misafir alıcı e-postasını onaylamadan önce düzeltebilir. | 10 KP-8 · 02 §3.24.5 | Misafir alıcı; onay özetinde yanlış e-posta | Düzeltilen e-posta onay özetine ve oluşan siparişe yazılır | Uç yanıtı + Veri kaydı | yok | ORTA | BEKLEMEDE |
| VAL-A028 | A | Müşteri varyantı adediyle sepete ekler. | 10 KP-9 · 02 §3.16 | Stoğu yeterli varyant | Sepette varyant seçilen adetle durur | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-A029 | A | Sepet fiyatı ve satın alınabilirliği güncel üründen okur ve fiyatı değişen kalemi işaretler. | 10 KP-9 · 02 §3.16 | Sepette kalem; ürünün fiyatı sonradan değiştirilmiş | Sepet güncel fiyatı gösterir, fiyatı değişen kalemi işaretler ve toplam güncel fiyatlarla hesaplanır | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-A030 | A | Stoktan fazla adet sepete eklenemez ve stok adedi söylenmez. | 10 KP-9 · 02 §3.16 | Stoğu üç olan varyant | Dördüncü adet eklenemez; yanıt stok adedini yazmaz | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-A031 | A | Firmanın ürün başına koyduğu sınırdan fazla adet sepete eklenemez. | 10 KP-9 · 02 §3.16 | Ürün başına adet sınırı tanımlı, stoğu sınırdan fazla ürün | Sınırı aşan adet eklenemez; yanıt stok adedini yazmaz | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-A032 | A | Dijital üründe adet birdir. | 10 KP-9 · 02 §3.16, §3.12.9 | Yayında dijital ürün | Dijital ürün sepete tek adetle girer; ikinci adet eklenemez | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-A033 | A | Sepetteki kalem sayısına tavan yoktur. | 10 KP-9 · 02 §3.16 | Elliden çok farklı kalem | Kalemlerin hepsi sepete girer; kalem sayısı için bir sınır mesajı çıkmaz | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-A034 | A | Daha önce aldığı dijital varyantı sepete ekleyen üye "Bu ürünü daha önce aldınız — sipariş sayfanızdan indirebilirsiniz" uyarısını görür. | 10 KP-9 · 02 §3.12.9 | Dijital varyantı daha önce satın almış üye | Sepete eklerken uyarı metni görünür | Tarayıcı gözlemi | yok | ORTA | BEKLEMEDE |
| VAL-A035 | A | Aynı üye uyarıdan sonra isterse dijital varyantı yine satın alır. | 10 KP-9 · 02 §3.12.9 | Önceki satırın durumu | Varyant sepette kalır ve sipariş verilebilir; uyarı engel olmaz | Uç yanıtı + Veri kaydı | yok | ORTA | BEKLEMEDE |
| VAL-B006 | B | Üye müşterinin sepeti hesabında durur ve her cihazda aynıdır. | 10 KP-10 · 02 §3.16 | Üye; iki ayrı tarayıcı oturumu | Bir oturumda eklenen kalem ötekinde aynı adetle görünür | Uç yanıtı + Veri kaydı | yok | ORTA | BEKLEMEDE |
| VAL-B007 | B | Misafirin sepeti tarayıcıda durur ve girişte hesabın sepetiyle birleşir. | 10 KP-10 · 02 §3.16 | Misafir sepeti dolu; hesabın sepetinde de kalem var | Girişten sonra sepet iki sepetin kalemlerini birlikte taşır ve kalem kaybolmaz | Uç yanıtı + Veri kaydı | yok | ORTA | BEKLEMEDE |
| VAL-D002 | D | Sepet kendiliğinden boşalmaz. | 10 KP-10 · 02 §3.16 | Dolu üye sepeti ve dolu misafir sepeti; zaman ileri alınır | Zaman geçse de kalemler yerinde durur; sepeti boşaltan bir zamanlı iş yoktur | Veri kaydı + İş kaydı | yok | ORTA | BEKLEMEDE |
| VAL-A036 | A | Yüzde tutarlı kupon sepet toplamından, indirimli fiyatın üzerinden düşer. | 10 KP-11 · 02 §3.10 | Yüzde tutarlı kupon; indirimli bir kalem | İndirim, kalemin indirimli fiyatı üzerinden hesaplanan toplamdan yüzdeyle düşer ve ödenecek tutar buna göre yazılır | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-A037 | A | Sabit tutarlı kupon sepet toplamından düşer. | 10 KP-11 · 02 §3.10 | Sabit tutarlı kupon | İndirim sabit tutardır ve ödenecek tutar buna göre yazılır | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-A038 | A | Kupon sipariş tutarını sıfıra indiremez; sepet toplamına eşit ya da onu aşan kupon reddedilir. | 10 KP-11 · 02 §3.10 | Tutarı sepet toplamına eşit ve toplamı aşan kuponlar | İki kupon da reddedilir; sepet toplamı değişmez ve kupon kullanımı yazılmaz | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-A039 | A | Müşteri ödeme adımında teslimat ve fatura adresini girer. | 10 KP-12 · 02 §3.14 | Fiziksel ürünlü sepet | İki adres girilir ve siparişe yazılır | Uç yanıtı + Veri kaydı | yok | ORTA | BEKLEMEDE |
| VAL-A040 | A | Üye müşteri adres defterinden seçer ya da yeni adres yazıp isterse deftere kaydeder. | 10 KP-12 · 02 §3.14 | Adres defterinde kayıt olan üye | Defterdeki adres seçilebilir; yeni adres istenirse deftere eklenir, istenmezse eklenmez | Uç yanıtı + Veri kaydı | yok | ORTA | BEKLEMEDE |
| VAL-A041 | A | Yalnız dijital ürün ve hizmet içeren siparişte teslimat adresi istenmez. | 10 KP-12 · 02 §3.14.6 | Yalnız dijital ürün ve hizmet içeren sepet; fiziksel ürünlü sepet | İlk sepette teslimat adresi alanı yoktur; ikincisinde zorunludur | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-A042 | A | Adres alıcının adını ve soyadını, ili, ilçeyi ve açık adresi taşır; teslimat adresi ayrıca zorunlu bir telefon taşır; posta kodu ve T.C. kimlik numarası istenmez. | 10 KP-12 · 02 §3.14 | Ödeme adımının adres formu | Belirtilen alanlar zorunludur; telefon yalnız teslimat adresinde zorunludur; posta kodu ve T.C. kimlik numarası alanı yoktur | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-A043 | A | Müşteri sipariş başına sabit kargo ücretini görür. | 10 KP-13 · 02 §3.18 | Kargo ücreti tanımlı mağaza | Sepet ve onay özeti kargo ücretini gösterir ve ödenecek tutara bir kez ekler | Uç yanıtı + Tarayıcı gözlemi | yok | KRİTİK | BEKLEMEDE |
| VAL-A044 | A | Firma eşik tanımladıysa müşteri ücretsiz kargoya kalan tutarı görür. | 10 KP-13 · 02 §3.19 | Ücretsiz kargo eşiği tanımlı; sepet eşiğin altında | Sepet kalan tutarı gösterir; eşiğe ulaşınca kargo ücreti kalkar | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-A045 | A | Asgari sipariş tutarının altında sipariş verilemez ve sebebi görünür. | 10 KP-13 · 02 §3.20 | Asgari sipariş tutarı tanımlı; sepet tutarın altında | Sipariş onaylanamaz ve sebep metni görünür | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-A046 | A | Firmanın teslimat yapmadığı bir ile sipariş verilemez ve sebebi görünür. | 10 KP-13 · 02 §3.20 | Teslimat yapılmayan il tanımlı mağaza | O ili seçen müşteri siparişi onaylayamaz ve sebep metni görünür | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-A047 | A | Müşteri onay özetini, Ön Bilgilendirme Formu'nu, siparişin nasıl kurulduğunu anlatan sabit metni ve kutuların hemen üstündeki kısa cayma özetini görür. | 10 KP-14 · 02 §3.17, §3.24 | Ödeme adımı | Dört parça da onay bölümünde ve kutulardan önce görünür | Tarayıcı gözlemi + Uç yanıtı | yok | KRİTİK | BEKLEMEDE |
| VAL-A048 | A | Müşteri siparişi iki onay kutusuyla ve siparişin ödeme yükümlülüğü doğurduğunu söyleyen düğmeyle onaylar. | 10 KP-14 · 02 §3.24 | Ödeme adımı | İki onay kutusu ve ödeme yükümlülüğünü yazan düğme vardır; sipariş bu onayla oluşur | Tarayıcı gözlemi + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-A049 | A | Sepette dijital kalem varsa indirmenin ödeme onaylandığı anda açılacağı ve cayma hakkının düşeceği üçüncü bir kutuyla kabul edilir. | 10 KP-14 · 02 §3.24 | Dijital kalem içeren sepet; dijital kalemsiz sepet | İlk sepette üçüncü kutu vardır ve kabul kaydı siparişe yazılır; ikincisinde kutu yoktur | Tarayıcı gözlemi + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-A050 | A | Sepette hizmet kalemi varsa ifanın cayma süresi dolmadan başlaması ve ifa tamamlanınca hakkın düşmesi ayrı bir kutuyla kabul edilir. | 10 KP-14 · 02 §3.24 | Hizmet kalemi içeren sepet; hizmet kalemsiz sepet | İlk sepette ayrı kutu vardır ve kabul kaydı siparişe yazılır; ikincisinde kutu yoktur | Tarayıcı gözlemi + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-A051 | A | Onay anında özetin ya da Ön Bilgilendirme Formu'nun veya Mesafeli Satış Sözleşmesi'nin sürümü değişmişse sipariş oluşmaz. | 10 KP-14 · 02 §3.24 | Ödeme adımı açık; onaydan önce fiyat, sözleşme metni ya da yasal kimlik değiştirilmiş | Onay reddedilir ve sipariş kaydı yazılmaz | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-A052 | A | Sürüm değiştiğinde müşteri neyin değiştiğini görür ve kutuları yeniden işaretler. | 10 KP-14 · 02 §3.24 | Önceki satırın durumu | Yeniden gösterilen ödeme adımı değişikliği söyler; yasal metin kutuları işaretsizdir ve yeni sürümle onaylanınca sipariş oluşur | Tarayıcı gözlemi + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-B008 | B | Müşteri kartla — firmanın ödeme sağlayıcısının sayfasında, her ödemede 3D Secure'la — öder. | 10 KP-15 · 02 §3.21 | Sahte ödeme sağlayıcısı (`mock`); kart yöntemi açık | Müşteri sağlayıcının sayfasına yönlenir, 3D Secure adımından geçer ve sağlayıcının bildirimi ödemeyi siparişe işler | Uç yanıtı + Veri kaydı | mock | KRİTİK | BEKLEMEDE |
| VAL-B009 | B | Müşteri havale/EFT ile öder ve ekranda firmanın IBAN'ını görür. | 10 KP-15 · 02 §3.17 | Havale yöntemi açık; firmanın IBAN'ı kayıtlı | Sipariş sayfası ve teyit ekranı kayıtlı IBAN'ı eksiksiz gösterir | Uç yanıtı + Tarayıcı gözlemi | yok | KRİTİK | BEKLEMEDE |
| VAL-B010 | B | İki ödeme yolunda da ekranda siparişin alındığı ve sipariş numarası görünür. | 10 KP-15 · 02 §3.17 | Bir kart, bir havale siparişi | İki yolda da teyit ekranı siparişin alındığını ve sipariş numarasını yazar | Tarayıcı gözlemi | mock | ORTA | BEKLEMEDE |
| VAL-B011 | B | Kartta 3D Secure dönüşü sipariş sayfasına iner; sonuç henüz gelmemişse sayfa ödemenin beklendiğini söyler. | 10 KP-15 · 02 §3.17, §3.21 | Sahte sağlayıcı; bildirimi henüz göndermemiş | Dönüş sipariş sayfasını açar ve sayfa ödemenin beklendiğini yazar | Tarayıcı gözlemi | mock | ORTA | BEKLEMEDE |
| VAL-B012 | B | Ödeme yarıda kalırsa sepet olduğu gibi durur ve müşteri aynı sepetten yeni bir siparişle yeniden dener. | 10 KP-15 · 02 §3.17 | Ödeme sayfasından dönmeden çıkılmış kart ödemesi | Sepet kalemleriyle durur; aynı sepetle yeni sipariş onaylanabilir | Uç yanıtı + Veri kaydı | mock | ORTA | BEKLEMEDE |
| VAL-C001 | C | Süresi içinde ödenmeyen sipariş kendiliğinden iptal olur. | 10 KP-15 · 02 §3.17, §6.1.2 | Ödemesi beklenen sipariş; ödeme süresi ileri alınarak aşılır | Sipariş iptal durumuna geçer ve zamanlı iş izi yazılır; süre dolmadan geçiş olmaz | Veri kaydı + İş kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-C002 | C | İptal edilmiş siparişe sağlayıcıdan geç gelen kart ödemesi sistem tarafından kendiliğinden geri ödenir. | 10 KP-15 · 02 §6.2.16 | Kendiliğinden iptal olmuş sipariş; sahte sağlayıcı geç ödeme bildirimi gönderir | Ödeme geri ödenir; sipariş iptal durumunda kalır ve ikinci bir geri ödeme doğmaz | Veri kaydı + İş kaydı | mock | KRİTİK | BEKLEMEDE |
| VAL-A053 | A | Geç gelen ödeme geri ödenince müşteriye geri ödeme e-postası gider. | 10 KP-15 · 02 §6.2.16 | Önceki satırın durumu; sahte e-posta altyapısı | Müşteriye geri ödeme e-postası gönderilir | İş kaydı | mock | ORTA | BEKLEMEDE |
| VAL-D003 | D | Ödeme ve geri ödeme siparişin ödeme kaydında durur. | 10 KP-15 · 02 §3.21 | Ödenmiş ve geri ödenmiş bir sipariş | Ödeme kaydı ödemeyi ve geri ödemeyi tarih ve tutarla taşır; sipariş sayfası aynısını gösterir | Veri kaydı + Uç yanıtı | mock | KRİTİK | BEKLEMEDE |
| VAL-D004 | D | [güvenlik] Kart verisi ürüne hiç gelmez. | 10 KP-15 · 02 §3.21 | Kartla verilmiş sipariş | Kart numarası ve güvenlik kodu uygulamanın hiçbir isteğinde, tablosunda ve kayıt satırında yer almaz | Veri kaydı + Uygulama kaydı | mock | KRİTİK | BEKLEMEDE |
| VAL-D005 | D | Onaylanan siparişin stoğu ödeme süresi boyunca ayrılır. | 10 KP-16 · 02 §3.17.3 | Stoğu sınırlı varyant; ödemesi beklenen sipariş | Ayrılan adet kadar satılabilir adet azalır; ayırma kaydı yazılır | Veri kaydı + Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-D006 | D | Ayrılan stok ödeme onayında kesinleşir. | 10 KP-16 · 02 §3.17.3 | Stoğu ayrılmış sipariş; ödeme onaylanır | Stok kesin düşer; ayırma satırı kalmaz ve satılabilir adet bir daha değişmez | Veri kaydı | mock | ORTA | BEKLEMEDE |
| VAL-D007 | D | Hizmet kontenjanı aynı rejimle ayrılır ve kesinleşir. | 10 KP-16 · 02 §3.17.3 | Kontenjanlı hizmet; ödemesi beklenen sipariş | Kontenjan ödeme süresince ayrılır, ödeme onayında kesinleşir | Veri kaydı | mock | ORTA | BEKLEMEDE |
| VAL-D008 | D | Kupon hakkı aynı rejimle ayrılır ve kesinleşir. | 10 KP-16 · 02 §3.17.3 | Kullanım adedi sınırlı kupon; ödemesi beklenen sipariş | Kupon hakkı ödeme süresince ayrılır, ödeme onayında kesinleşir | Veri kaydı | mock | ORTA | BEKLEMEDE |
| VAL-B013 | B | Ödeme sürerken aynı stok başka bir müşteriye satılmaz. | 10 KP-16 · 02 §3.17.3 | Son adedi başka müşterinin ödemesini bekleyen varyant | İkinci müşteri o adedi ayıramaz ve kalemin ayrılamadığını görür | Uç yanıtı + Veri kaydı | yok | ORTA | BEKLEMEDE |
| VAL-D009 | D | Müşteri, sipariş oluştuğu anda tahmin edilemez bir sipariş numarası alır. | 10 KP-17 · 02 §3.22 | Art arda verilmiş siparişler | Sipariş numaraları art arda sayılar değildir; bir numaradan ötekisi türetilemez | Veri kaydı | yok | ORTA | BEKLEMEDE |
| VAL-D010 | D | Sipariş tutarı ve kalemlerin o anki tanımı — ürün tipi, cayma istisnası işareti ve sebebi, hizmette ifa süresi, dijital kalemin indirme hakkı — siparişin içine dondurulur. | 10 KP-17 · 02 §3.23, §3.23.3 | Verilmiş sipariş; sonra ürünün fiyatı, tipi, istisna işareti ve ifa süresi değiştirilir | Sipariş sayfası, siparişin kaydı ve tutarı onay anındaki değerlerde kalır | Veri kaydı + Uç yanıtı | yok | KRİTİK | BEKLEMEDE |
| VAL-D011 | D | Adresler ve siparişin iletişim e-postası siparişe dondurulur; adresler "Kargoya verildi"ye kadar müşterinin talebiyle firma tarafından, misafir siparişinin e-postası müşterinin talebiyle düzeltilir ve önceki değer geçmişte durur. | 10 KP-17 · 02 §3.22, §10.4.2 | Verilmiş sipariş; adres ve e-posta firmaca düzeltilir | Düzeltme dışında adres ve e-posta değişmez; düzeltilen sipariş yeni değeri gösterir ve önceki değer adres ya da e-posta geçmişinde durur | Veri kaydı + Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-D012 | D | Sipariş kargoya verme sözünü kendi içine dondurur. | 10 KP-17 · 02 §3.23 | Verilmiş sipariş; sonra ürünün kargoya verilme süresi değiştirilir | Siparişin kargoya verme sözü onay anındaki değerde kalır | Veri kaydı | yok | ORTA | BEKLEMEDE |
| VAL-D013 | D | Sipariş firmanın o günkü yasal kimliğini kendi içine dondurur. | 10 KP-17 · 02 §3.23, §3.23.4 | Verilmiş sipariş; sonra firmanın yasal kimliği değiştirilir | Sipariş sayfası ve siparişin belgeleri onay anındaki yasal kimliği gösterir | Veri kaydı + Uç yanıtı | yok | KRİTİK | BEKLEMEDE |
| VAL-D014 | D | Havale ile verilen sipariş firmanın o günkü havale IBAN'ını kendi içine dondurur. | 10 KP-17 · 02 §3.23 | Havale siparişi; sonra firmanın IBAN'ı değiştirilir | Sipariş sayfası onay anındaki IBAN'ı gösterir | Veri kaydı + Uç yanıtı | yok | KRİTİK | BEKLEMEDE |
| VAL-D015 | D | Sipariş müşterinin onay adımında gördüğü yasal metin sürümlerini kendi içine dondurur. | 10 KP-17 · 02 §3.23.3 | Verilmiş sipariş; sonra yasal metinlerin yeni sürümü yayına alınır | Siparişteki Ön Bilgilendirme Formu ve Mesafeli Satış Sözleşmesi onay anındaki sürümdür | Veri kaydı + Uç yanıtı | yok | KRİTİK | BEKLEMEDE |
| VAL-D016 | D | Sipariş, dijital ve hizmet kalemi için işaretlenen onay kutularının kaydını kendi içine dondurur. | 10 KP-17 · 02 §3.23, §3.24 | Dijital ve hizmet kalemli, kutuları işaretlenmiş sipariş | Siparişte her kutunun işaret kaydı vardır ve sonradan değişmez | Veri kaydı + Uç yanıtı | yok | KRİTİK | BEKLEMEDE |

### 4.5 Sipariş takibi ve teslim (KP-18…KP-20; `VP4-04`)

Kaynak: `10 §2` grup "Sipariş takibi ve teslim". Erişim anahtarı, 404 ve çatı davranışlarının uç düzeyindeki senaryoları `VP4-18`'dedir; indirme hakkının düştüğü an `VP12-10`'dadır.

| ID | Seviye | Kural | Kaynak | Ön koşul | Beklenen sonuç | Kanıt türü | Kanıt ortamı | Severity | Durum |
|---|---|---|---|---|---|---|---|---|---|
| VAL-B014 | B | Üye müşteri siparişini sipariş geçmişinden açıp sipariş sayfasından izler. | 10 KP-18 · 02 §3.22 | Siparişi olan üye | Sipariş geçmişindeki satırdan sipariş sayfası açılır | Tarayıcı gözlemi | yok | ORTA | BEKLEMEDE |
| VAL-B015 | B | Misafir alıcı sipariş sayfasına sipariş numarası ve e-postasıyla girer. | 10 KP-18 · 02 §3.22.5 | Misafir siparişi | Numara ve sipariş e-postası doğruysa sipariş sayfası açılır; e-posta yanlışsa açılmaz | Uç yanıtı + Uygulama kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-B016 | B | Herkes sipariş e-postasındaki, siparişe özgü erişim anahtarı taşıyan bağlantıyla girer. | 10 KP-18 · 02 §3.22.5 | Sipariş e-postasındaki bağlantı | Bağlantı oturumsuz da sipariş sayfasını açar | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-A054 | A | [güvenlik] Sipariş numarası tek başına sayfayı açmaz. | 10 KP-18 · 02 §3.22.5 | Var olan bir siparişin numarası; oturum, e-posta ve erişim anahtarı yok | İstek sayfayı açmaz ve sipariş verisi dönmez | Uç yanıtı + Uygulama kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-A055 | A | Sipariş sayfası siparişin ve ödemenin durumunu gösterir. | 10 KP-18 · 02 §3.22, §5.3–§5.5 | Her ödeme durumunda birer sipariş | Sayfa siparişin ve ödemenin güncel durumunu yazar | Tarayıcı gözlemi | yok | ORTA | BEKLEMEDE |
| VAL-A056 | A | Sipariş sayfası gerçekleşmiş geri ödemeleri — tarih, tutar ve yolla — gösterir. | 10 KP-18 · 02 §3.22, §9.1.2 | Geri ödemesi gerçekleşmiş sipariş | Her geri ödeme tarih, tutar ve yolla listelenir; gerçekleşmemiş geri ödeme listelenmez | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-A057 | A | Sipariş sayfası takip bilgisini ve indirme düğmesini taşır. | 10 KP-18 · 02 §3.22, §3.17.8 | Kargoya verilmiş fiziksel kalemli sipariş; ödemesi alınmış dijital kalemli sipariş | Kargolu siparişte takip bilgisi, dijital siparişte indirme düğmesi görünür | Tarayıcı gözlemi | yok | ORTA | BEKLEMEDE |
| VAL-A058 | A | Havale ile verilen siparişin sayfası IBAN'ı taşır. | 10 KP-18 · 02 §3.22 | Havale siparişi | Sayfa siparişe dondurulmuş IBAN'ı eksiksiz gösterir | Uç yanıtı + Tarayıcı gözlemi | yok | KRİTİK | BEKLEMEDE |
| VAL-A059 | A | Ödemesi hiç alınmamış ve kendiliğinden iptal edilmiş sipariş müşterinin listesinde görünmez, firmanın panelinde görünür. | 10 KP-18 · 02 §3.22, §3.17.8 | Ödemesi hiç alınmadan kendiliğinden iptal olmuş sipariş | Müşterinin sipariş geçmişinde yoktur; panelin sipariş listesinde vardır | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-B017 | B | Hiçbir sipariş akışı bir e-postanın ulaşmasına bağlı değildir. | 10 KP-18 · 02 §3.22, §9.1.2 | Sahte e-posta altyapısı gönderimi reddediyor | Sipariş oluşur, sipariş sayfası numara ve e-postayla açılır ve müşteri işlemlerini sayfadan yapabilir | Uç yanıtı + İş kaydı | mock | ORTA | BEKLEMEDE |
| VAL-A060 | A | Ziyaretçi ürünün kargoya verilme süresini vitrinde görür. | 10 KP-19 · 02 §3.20 | Kargoya verilme süresi girilmiş ürün | Ürün sayfasında kargoya verilme süresi yazar | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-A061 | A | Müşteri kargoya verilen siparişin kargo şirketini ve takip numarasını sipariş sayfasında görür. | 10 KP-19 · 02 §3.20 | Kargoya verilmiş sipariş | Sayfa kargo şirketini ve takip numarasını gösterir | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-A062 | A | Listedeki kargo şirketlerinde "Takip et" bağlantısı görünür. | 10 KP-19 · 02 §3.20 | Listede olan ve olmayan şirketle kargoya verilmiş iki sipariş | Listedeki şirketin siparişinde "Takip et" bağlantısı vardır; öteki sipariş yalnız takip numarasını gösterir | Tarayıcı gözlemi | yok | DÜŞÜK | BEKLEMEDE |
| VAL-A063 | A | Firmanın "kendi aracımızla teslim" beyanı sipariş sayfasında görünür. | 10 KP-19 · 02 §3.20 | "Kendi aracımızla teslim" beyanıyla kargoya verilmiş sipariş | Sipariş sayfası beyanı yazar; kargo şirketi ve takip numarası yazmaz | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-B018 | B | Müşteri dijital ürünü ödeme onaylanınca sipariş sayfasından indirir. | 10 KP-20 · 02 §3.12 | Ödemesi onaylanmış dijital kalemli sipariş | Sipariş sayfasındaki düğmeyle dosya indirilir | Uç yanıtı + Veri kaydı | yok | ORTA | BEKLEMEDE |
| VAL-A064 | A | Ödeme onaylanmadan dijital ürünün indirmesi açılmaz. | 10 KP-20 · 02 §3.12 | Ödemesi beklenen dijital kalemli sipariş | Sayfada indirme düğmesi yoktur ve indirme isteği dosyayı vermez | Uç yanıtı + Uygulama kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-A065 | A | Müşteri dijital ürünü kalem başına tanınan indirme hakkı kadar indirir. | 10 KP-20 · 02 §3.12 | İndirme hakkı belirli dijital kalem | Hak bitene kadar indirme çalışır; hak bitince bir sonraki indirme dosyayı vermez | Uç yanıtı + Veri kaydı | yok | ORTA | BEKLEMEDE |
| VAL-A066 | A | Firma dosyayı güncellediğinde müşteri yeni hâli de indirir. | 10 KP-20 · 02 §3.12 | İndirilmiş dijital kalem; firma dosyayı yeniler | Sonraki indirme yeni dosyayı verir | Uç yanıtı | yok | ORTA | BEKLEMEDE |

### 4.6 İptal, cayma, iade ve ayıp talebi (KP-21…KP-24; `VP4-05`)

Kaynak: `10 §2` grup "İptal, cayma, iade ve ayıp talebi". Kısmi adet senaryolarının ayrıntısı, kuruş yuvarlaması ve geri ödeme değişmezleri (P3.4, P3.6, P5.5) §12'dedir (`VP12-04`, `VP12-09`); burada kapsam cümlesinin kuralı sınanır. Geri ödemenin ödeme sağlayıcısındaki hâli sahte uçla (`mock`) sınanır.

| ID | Seviye | Kural | Kaynak | Ön koşul | Beklenen sonuç | Kanıt türü | Kanıt ortamı | Severity | Durum |
|---|---|---|---|---|---|---|---|---|---|
| VAL-C003 | C | Müşteri fiziksel kalemi ya da kalemin bir kısmını (adet), sipariş kargoya verilene kadar, firma onayı beklemeden sipariş sayfasından iptal eder. | 10 KP-21 · 02 §7.1.6, §7.2 | Ödemesi alınmış, kargoya verilmemiş fiziksel kalemli sipariş | İptal firma işlemi olmadan tamamlanır; iptal edilen adetler kapanır, kalan adetler açık kalır | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-C004 | C | Sipariş kargoya verildikten sonra fiziksel kalem iptal düğmesiyle iptal edilemez. | 10 KP-21 · 02 §7.2 | Kargoya verilmiş fiziksel kalemli sipariş | İptal düğmesi yoktur ve iptal isteği reddedilir; kalem durumu değişmez | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-C005 | C | Müşteri hizmet kalemini tamamlanana kadar sipariş sayfasından iptal eder. | 10 KP-21 · 02 §7.2 | Ödemesi alınmış, tamamlanmamış hizmet kalemli sipariş | İptal tamamlanır; tamamlanmış hizmet kalemi için iptal düğmesi yoktur ve istek reddedilir | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-A067 | A | İptalde ödenmiş tutar on dört gün içinde ödemenin geldiği yoldan geri ödenir. | 10 KP-21 · 02 §7.1.6, §7.2 | Kartla ödenmiş ve iptal edilmiş kalem | Geri ödeme ödemenin geldiği yola, iptalden itibaren on dört gün içinde ve işleme giren adet kadar yazılır | Veri kaydı + İş kaydı | mock | KRİTİK | BEKLEMEDE |
| VAL-A068 | A | Havale ile ödenmiş siparişte müşteri geri ödeme için IBAN'ını iptal sırasında girer. | 10 KP-21 · 02 §7.2 | Havale ile ödenmiş sipariş; iptal ekranı | İptal ekranı IBAN ister; girilen IBAN geri ödemeye bağlanır | Uç yanıtı + Veri kaydı | yok | ORTA | BEKLEMEDE |
| VAL-C006 | C | Kalem düzeyi iptal ödeme onayından sonra açılır. | 10 KP-21 · 02 §7.2 | Ödemesi onaylanmış çok kalemli sipariş | Tek kalemi iptal etme seçeneği açıktır; kalan kalemler yoluna devam eder | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-C007 | C | Ödeme onayından önce — dijital kalem dahil — sipariş yalnız bütünüyle iptal edilir. | 10 KP-21 · 02 §7.2 | Ödemesi beklenen çok kalemli sipariş, dijital kalem dahil | Kalem düzeyi iptal sunulmaz ve istenirse reddedilir; bütünüyle iptal çalışır | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-C008 | C | Kargoya verme süresi aşıldığında fesih de bu iptal düğmesiyle yapılır. | 10 KP-21 · 02 §7.2 | Kargoya verme süresi aşılmış, kargoya verilmemiş sipariş | İptal düğmesi açıktır ve işlem fesih olarak kaydedilir | Uç yanıtı + Veri kaydı | yok | ORTA | BEKLEMEDE |
| VAL-C009 | C | Otuz günlük teslim sınırı aşıldığında müşteri, sipariş kargoya verilmiş olsa bile, teslim edilmemiş fiziksel kalemler için bütün işleme açık adetleriyle sipariş sayfasından "gecikme nedeniyle fesih" yapar; fesihte seçim yoktur. | 10 KP-21 · 02 §7.2.3 | Otuz günlük sınırı aşmış, kargoya verilmiş ama teslim edilmemiş fiziksel kalemler | Fesih ekranı adet seçtirmez; teslim edilmemiş kalemlerin işleme açık adetlerinin hepsi feshedilir; sınır aşılmadan düğme yoktur | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-A069 | A | Gecikme feshinde o kalemlerin bedeli ve kargo ücreti on dört gün içinde geri ödenir; havalede IBAN fesih sırasında girilir. | 10 KP-21 · 02 §7.2.3 | Gecikme nedeniyle feshedilmiş kalemler; kartla ve havale ile ödenmiş birer sipariş | Geri ödeme kalem bedelini ve kargo ücretini kapsar ve on dört gün içinde yazılır; havalede IBAN fesih ekranında alınır | Veri kaydı + İş kaydı | mock | KRİTİK | BEKLEMEDE |
| VAL-C010 | C | Gecikme feshinden sonra öteki kalemler yoluna devam eder ve açık kalemi kalmayan sipariş kapanışla İptal edildi'ye geçer. | 10 KP-21 · 02 §7.2.3 | İki kalemli sipariş; biri feshedilir, sonra öteki feshedilir | İlk fesihte sipariş açık kalır; son açık kalem kapanınca sipariş İptal edildi olur | Veri kaydı + Uç yanıtı | yok | KRİTİK | BEKLEMEDE |
| VAL-A070 | A | Feshi e-postayla, mektupla ya da site kesintisindeyken başka bir kanaldan bildiren müşterinin feshini firma kaydeder. | 10 KP-21 · 02 §7.2.3, §7.1.6 | Panel oturumu; başka kanaldan gelmiş fesih bildirimi | Firma feshi panelden kaydeder ve kayıt müşterinin sipariş sayfasındaki fesihle aynı sonucu verir | Uç yanıtı + Veri kaydı | yok | ORTA | BEKLEMEDE |
| VAL-C011 | C | Müşteri fiziksel kalemden, sipariş kargoya verildikten sonra teslimden itibaren on dört güne kadar sipariş sayfasından cayar. | 10 KP-22 · 02 §7.3, §7.4 | Teslim edilmiş fiziksel kalemli sipariş | Cayma beyanı pencere içinde kabul edilir ve tarih damgasıyla kayda geçer | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-C012 | C | Teslimden on dört gün sonra sipariş sayfasından cayma beyanı yapılamaz. | 10 KP-22 · 02 §7.4 | Teslimi on dört günü aşmış fiziksel kalemli sipariş; zaman ileri alınır | Cayma düğmesi yoktur ve beyan isteği reddedilir | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-C013 | C | Hizmette cayma, ödeme onaylandıktan sonra, sipariş tarihinden — fiziksel kalem içeren siparişte teslimden — itibaren on dört gün içinde ve hizmet tamamlanmadan önce yapılır. | 10 KP-22 · 02 §7.3, §7.4 | Hizmet kalemli iki sipariş: yalnız hizmetli, hizmet ve fiziksel kalemli | Pencere içinde ve tamamlanmadan önce cayma kabul edilir; tamamlanmış hizmette ya da pencere dışında reddedilir; başlangıç sayımı iki siparişte belirtilen tarihlerden yapılır | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-C014 | C | Ödeme beklenirken müşterinin yolu siparişin bütünüyle iptalidir; cayma yolu açık değildir. | 10 KP-22 · KP-21 · 02 §7.3 | Ödemesi beklenen sipariş | Cayma düğmesi yoktur; bütünüyle iptal çalışır | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-C015 | C | Fiziksel kalemleri teslimden önce kapanan siparişte hizmetin cayma hakkı tamamlanma işaretine kadar sürer. | 10 KP-22 · 02 §7.4 | Fiziksel kalemleri kapanmış, hizmet kalemi tamamlanmamış sipariş | Hizmet için cayma tamamlanma işaretine kadar açıktır; işaretten sonra kapanır | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-A071 | A | Müşteri kalemin bir kısmından (adet) ve ardışık beyanlarla cayabilir. | 10 KP-22 · 02 §7.1.7, §7.4 | Adedi birden büyük teslim edilmiş kalem | İki ardışık beyan kabul edilir; her biri kendi adedini ve damgasını taşır | Veri kaydı + Uç yanıtı | yok | KRİTİK | BEKLEMEDE |
| VAL-A072 | A | Cayma beyanı tarih damgasıyla kayda geçer. | 10 KP-22 · 02 §7.4 | Cayma beyanı yapılmış sipariş | Beyanın kaydı beyan anının damgasını taşır ve sonradan değişmez | Veri kaydı + Uç yanıtı | yok | KRİTİK | BEKLEMEDE |
| VAL-A073 | A | İade kargo bedeli firmanındır; iade için taşıyıcı belirlenmez ve geri ödeme tutarından iade kargo bedeli düşülmez. | 10 KP-22 · 02 §7.4 | Cayması kabul edilmiş fiziksel kalem | Geri ödeme tutarı kalemin ödenmiş tutarından iade kargo bedeli düşülmeden hesaplanır; sistem taşıyıcı seçtirmez | Veri kaydı + Uç yanıtı | yok | KRİTİK | BEKLEMEDE |
| VAL-A074 | A | Teslimden önceki caymada ve hizmette geri ödeme süresi beyandan itibaren on dört gündür. | 10 KP-22 · 02 §7.4 | Teslimden önce kapanan sipariş ve hizmet için birer cayma | Geri ödeme süresi beyan tarihinden sayılır ve on dört gün içinde yazılır | Veri kaydı + İş kaydı | mock | KRİTİK | BEKLEMEDE |
| VAL-A075 | A | Teslimden sonraki caymada geri ödeme süresi malın firmaya ulaştığı tarihten itibaren on dört gündür. | 10 KP-22 · 02 §7.4 | Teslimden sonra cayılmış ve malı firmaya ulaşmış kalem | Süre ulaşma tarihinden sayılır; ulaşma işaretlenmeden geri ödeme süresi başlamaz | Veri kaydı + İş kaydı | mock | KRİTİK | BEKLEMEDE |
| VAL-A076 | A | Kısmen ifa edilmiş hizmetten caymada cayılan adetlerin bedelinin tamamı geri ödenir. | 10 KP-22 · 02 §7.4 | Kısmen ifa edilmiş çok adetli hizmet kalemi; cayma | Cayılan adetlerin bedelinin tamamı geri ödenir | Veri kaydı + Uç yanıtı | mock | KRİTİK | BEKLEMEDE |
| VAL-A077 | A | Havale ile ödenmiş siparişte müşteri geri ödeme için IBAN'ını cayma beyanı sırasında girer. | 10 KP-22 · 02 §7.4 | Havale ile ödenmiş sipariş; cayma ekranı | Beyan ekranı IBAN ister ve girilen IBAN geri ödemeye bağlanır | Uç yanıtı + Veri kaydı | yok | ORTA | BEKLEMEDE |
| VAL-A078 | A | IBAN sonradan yeniden gerektiğinde — mal dönmediği için silinmiş IBAN'dan sonra ulaşan malda, karta geri ödeme sağlayıcıda başarısız olup müşteri havale yolunu seçerse, bankada gerçekleşmeyen geri ödeme havalesinde ya da firmanın IBAN'sız kaydettiği cayma bildiriminde — müşteri IBAN'ı sipariş sayfasından girer. | 10 KP-22 · 02 §7.4, §10.4.10 | Dört hâlin her biri için bir sipariş | Dört hâlde de sipariş sayfası IBAN girişini sunar ve girilen IBAN geri ödemeye bağlanır | Uç yanıtı + Veri kaydı | mock | ORTA | BEKLEMEDE |
| VAL-A079 | A | Müşteri IBAN'ı geri ödeme işlenene kadar düzeltebilir; süre bu arada durmaz. | 10 KP-22 · 02 §7.4 | IBAN girilmiş, geri ödemesi işlenmemiş cayma | Düzeltme kabul edilir; geri ödemenin son günü değişmez; işlendikten sonra düzeltme yoktur | Uç yanıtı + Veri kaydı | yok | ORTA | BEKLEMEDE |
| VAL-A080 | A | Müşteri caymayı e-postayla ya da mektupla bildirdiğinde firma bildirimi panelden kaydeder; pencere kapandıktan sonra gelen süresinde gönderilmiş bildirim de kaydedilir. | 10 KP-22 · 02 §7.4 | Panel oturumu; pencere içinde ve pencere dışında gelmiş ama süresinde gönderilmiş bildirim | Firma iki bildirimi de kaydeder; kayıt gönderim tarihini taşır | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-A081 | A | Eksik bilgilendirmede cayma süresi yasal olarak uzar; süreye uygunluk bildirimin süresinde gönderilmesiyle ölçülür. | 10 KP-22 · 02 §7.4 | Cayma bilgilendirmesi eksik yapılmış sipariş; süre sonunda gönderilmiş bildirim | Uzayan süre içindeki bildirim kabul edilir; süre dışı gönderilen bildirim kabul edilmez | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-C016 | C | Cayma istisnası işaretli üründe mutlak sebepte — kişiye özel üretim, çabuk bozulan ya da son kullanma tarihi geçebilecek mal — cayma hakkı yoktur. | 10 KP-22 · 02 §7.3 | Mutlak sebepli istisna işaretli teslim edilmiş kalem | Cayma düğmesi yoktur ve beyan isteği reddedilir | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-C017 | C | İndirmesi açılmış dijital üründe cayma hakkı yoktur. | 10 KP-22 · 02 §7.3 | İndirmesi açılmış dijital kalem | Cayma düğmesi yoktur ve beyan isteği reddedilir | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-C018 | C | Koşullu sebepte — hijyen ürünü, maddi ortamdaki kitap, dijital içerik ve bilgisayar sarf malzemesi — koruyucu ambalajı açılmamış malda hak sürer. | 10 KP-22 · 02 §7.3 | Koşullu sebepli kalem; ambalajı açılmış ve açılmamış olarak iki sipariş | Ambalajı açılmamış malda cayma kabul edilir; açılmış malda kabul edilmez | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-C019 | C | Gecikme feshi cayma değildir ve cayma istisnası işaretli fiziksel üründe de açıktır. | 10 KP-22 · KP-21 · 02 §7.2.3, §7.3 | Cayma istisnası işaretli, otuz günlük sınırı aşmış fiziksel kalem | Cayma reddedilir; gecikme nedeniyle fesih kabul edilir | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-A082 | A | Müşteri ayıplı kalem için sipariş sayfasından ayıp talebi açar; adedi birden büyükse kaç adedinin ayıplı olduğunu seçer. | 10 KP-23 · 02 §7.5 | Adedi birden büyük, ayıp talebi açılabilir kalem | Talep seçilen ayıplı adetle açılır ve kalem bağlamıyla firmaya ulaşır | Uç yanıtı + Veri kaydı | yok | ORTA | BEKLEMEDE |
| VAL-C020 | C | Ayıp talebi fiziksel kalemde sipariş kargoya verildiği andan itibaren açılabilir. | 10 KP-23 · 02 §7.5 | Kargoya verilmiş ve verilmemiş fiziksel kalemli iki sipariş | Kargoya verilmiş siparişte talep açılır; kargoya verilmemişte açılamaz | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-C021 | C | Ayıp talebi dijital kalemde ödeme onayından itibaren açılabilir. | 10 KP-23 · 02 §7.5 | Ödemesi onaylanmış ve beklenen dijital kalemli iki sipariş | Onaylanmış siparişte talep açılır; ödemesi beklenende açılamaz | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-C022 | C | Ayıp talebi hizmette tamamlanma işaretinden itibaren açılabilir. | 10 KP-23 · 02 §7.5 | Tamamlanmış ve tamamlanmamış hizmet kalemli iki sipariş | Tamamlanmış hizmette talep açılır; tamamlanmamışta açılamaz | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-C023 | C | İki yıllık zamanaşımı teslimden işler. | 10 KP-23 · 02 §7.4.3, §7.4.4 | Teslim tarihi girilmiş sipariş; zaman ileri alınır | Teslimden iki yıl dolmadan talep açılır; dolunca ayıp talebi kanalı kapanır | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-C024 | C | Teslim tarihi girilmemişse zamanaşımı işlemez. | 10 KP-23 · 02 §7.4.3 | Teslim tarihi girilmemiş sipariş; zaman iki yıldan fazla ileri alınır | Ayıp talebi kanalı açık kalır | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-A083 | A | Süre dolunca ayıp talebi kanalı kapanır; hile ya da ağır kusur hâli iletişim formundan yürür. | 10 KP-23 · 02 §7.5 | Zamanaşımı dolmuş sipariş | Sipariş sayfası ayıp talebi sunmaz ve iletişim formuna yönlendirir; iletişim formu talebi alır | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-A084 | A | Ayıp talebinde geri gönderimin masrafı firmanındır. | 10 KP-23 · 02 §7.5 | Geri gönderimi gereken ayıp talebi | Müşteriden geri gönderim bedeli istenmez ve hiçbir tutardan düşülmez | Uç yanıtı + Veri kaydı | yok | ORTA | BEKLEMEDE |
| VAL-C025 | C | Firma ayıp talebini çözüldü işaretler; çözüm tutmazsa talep yeniden açılır. | 10 KP-23 · 02 §7.5, §5.10 | Açık ayıp talebi | Firma talebi çözüldü yapar; müşteri çözümü tutmadığını bildirince talep yeniden açılır ve durum geçmişinde iki geçiş yazılıdır | Uç yanıtı + Veri kaydı | yok | ORTA | BEKLEMEDE |
| VAL-B019 | B | Müşteri siparişin tek bir kalemini ödeme onaylandıktan sonra iptal eder ya da iade eder; kalan kalemler yoluna devam eder. | 10 KP-24 · 02 §7.1.3, §7.1.6 | Ödemesi onaylanmış üç kalemli sipariş | Seçilen kalem işlenir; öteki iki kalem durumunu ve adetlerini korur | Uç yanıtı + Veri kaydı | yok | ORTA | BEKLEMEDE |
| VAL-A085 | A | Adedi birden büyük kalemde müşteri kaç adedini işleme sokacağını seçer ve aynı kalemde art arda işlem yapabilir. | 10 KP-24 · 02 §7.1.7, §7.2.9 | Adedi birden büyük kalem | Seçilen adet işlenir; kalan adetle ikinci işlem yapılabilir | Uç yanıtı + Veri kaydı | yok | ORTA | BEKLEMEDE |
| VAL-A086 | A | Para işleme giren adet kadar döner. | 10 KP-24 · 02 §7.1.7 | Üç adetli kalem; bir adet işlenir | Geri ödeme kalemin bir adetlik payıdır; kalan iki adet ödenmiş sayılır | Veri kaydı + Uç yanıtı | mock | KRİTİK | BEKLEMEDE |
| VAL-A087 | A | Kupon payı adetle ayrılır ve son işlem kalan kuruşu alır. | 10 KP-24 · 02 §7.1.7 | Kuponlu, kuruşa bölünmeyen tutarlı çok adetli kalem; adetler tek tek işlenir | Geri ödemelerin toplamı kalemin ödenmiş tutarına eşittir ve son işlem kalan kuruşu alır | Veri kaydı + Uç yanıtı | mock | KRİTİK | BEKLEMEDE |
| VAL-A088 | A | Fiziksel kalemlerin bütün adetleri kargodan önce iptal edilirse ödenmiş kargo ücreti de geri ödenir. | 10 KP-24 · 02 §7.4.6 | Kargo ücreti ödenmiş, kargoya verilmemiş fiziksel kalemli sipariş | Son fiziksel adet iptal edilince kargo ücreti geri ödemeye eklenir | Veri kaydı + Uç yanıtı | mock | KRİTİK | BEKLEMEDE |
| VAL-A089 | A | Gönderilen fiziksel kalemlerin bütün adetlerinden cayılırsa ödenmiş kargo ücreti de geri ödenir. | 10 KP-24 · 02 §7.4.6 | Kargo ücreti ödenmiş, gönderilmiş fiziksel kalemli sipariş | Son gönderilmiş fiziksel adetten cayılıp mal teslim alınınca kargo ücreti geri ödemeye bir kez eklenir | Veri kaydı + Uç yanıtı | mock | KRİTİK | BEKLEMEDE |

### 4.7 Üyelik ve hesap (KP-25…KP-30 ve KP-77; `VP4-06`)

Kaynak: `10 §2` grup "Üyelik ve hesap". Hesap tarafının silmelerinin imha kaydı ve eski ana dönüş senaryoları (P5.6) ile ilk kurulumda çerez (P3.8) §12'dedir; Google sözleşmesi ve hata tablosu `VP4-15`'tedir. Google satırları sahte Google ucunu (`mock`) kullanır.

| ID | Seviye | Kural | Kaynak | Ön koşul | Beklenen sonuç | Kanıt türü | Kanıt ortamı | Severity | Durum |
|---|---|---|---|---|---|---|---|---|---|
| VAL-B020 | B | Ziyaretçi ad, e-posta ve şifreyle hesap açar. | 10 KP-25 · 02 §3.13 | Aydınlatma metni ve çerez politikası yayında | Kayıt hesabı doğrulanmamış açar ve doğrulama e-postası gönderir | Uç yanıtı + Veri kaydı | mock | ORTA | BEKLEMEDE |
| VAL-A090 | A | Hesap açma aydınlatma metni tamamlanıp yayına alındıysa ve çerez politikası yayına alındıysa açıktır. | 10 KP-25 · ÖK-10 · 02 §3.13.21 | Aydınlatma metni ya da çerez politikası yayında olmayan mağaza | Kayıt formu kapalıdır ve kayıt isteği hesap açmaz; ikisi de yayına alınınca açılır | Uç yanıtı + Uygulama kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-C026 | C | Hesap e-posta adresi doğrulanana kadar giriş için çalışmaz. | 10 KP-25 · 02 §3.13 | Doğrulanmamış hesap | Doğru şifreyle giriş oturum açmaz; doğrulandıktan sonra açar | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-C027 | C | Hesap e-posta adresi doğrulanana kadar sipariş için çalışmaz. | 10 KP-25 · 02 §3.13 | Doğrulanmamış hesap | Hesapla sipariş verilemez ve geçmiş misafir siparişleri hesaba bağlanmaz | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-B021 | B | Ziyaretçi Google hesabıyla giriş yapar. | 10 KP-26 · ÖK-6 · 02 §3.13 | Google uygulaması tanımlı; sahte Google ucu | Giriş tamamlanır ve oturum açılır | Uç yanıtı + Veri kaydı | mock | ORTA | BEKLEMEDE |
| VAL-A091 | A | Google ile giriş kurulumda Google uygulaması tanımlı değilse sunulmaz. | 10 KP-26 · ÖK-6 · 02 §3.13 | Google uygulaması tanımsız kurulum | Giriş ve kayıt ekranlarında Google düğmesi yoktur | Tarayıcı gözlemi | yok | ORTA | BEKLEMEDE |
| VAL-A092 | A | Yeni hesap açan ilk Google girişi aydınlatma metni tamamlanıp yayına alındıysa ve çerez politikası yayına alındıysa açıktır. | 10 KP-26 · ÖK-10 · 02 §3.13 | Aydınlatma metni ya da çerez politikası yayında olmayan mağaza; Google'da hesabı olan yeni kişi | Yeni hesap açılmaz; ikisi de yayına alınınca açılır | Uç yanıtı + Uygulama kaydı | mock | KRİTİK | BEKLEMEDE |
| VAL-A093 | A | Google'ın doğrulanmış verdiği e-postayla eşleşen hesap varsa Google ile giren o hesaba girer. | 10 KP-26 · 02 §3.13 | Doğrulanmış hesap; aynı e-postayla Google girişi | Giriş mevcut hesaba iner ve ikinci hesap açılmaz | Uç yanıtı + Veri kaydı | mock | KRİTİK | BEKLEMEDE |
| VAL-A094 | A | E-posta doğrulanmamış bekleyen bir kayda eşleşirse kayıt devralınmaz, silinir ve hesap Google ile doğar. | 10 KP-26 · 02 §3.13 | Doğrulanmamış bekleyen kayıt; aynı e-postayla Google girişi | Bekleyen kayıt silinir, hesap Google bağıyla yeniden doğar ve bekleyen kaydın şifresiyle giriş yapılamaz | Veri kaydı + Uç yanıtı | mock | KRİTİK | BEKLEMEDE |
| VAL-A095 | A | Google e-postayı doğrulanmamış verirse hesap açılmaz. | 10 KP-26 · 02 §3.13 | Sahte Google e-postayı doğrulanmamış döndürür | Hesap açılmaz ve oturum açılmaz | Uç yanıtı + Veri kaydı | mock | KRİTİK | BEKLEMEDE |
| VAL-A096 | A | Üye müşteri "beni hatırla" ile uzun oturum açar. | 10 KP-27 · 02 §3.13.10 | Kayıtlı üye | İşaretli girişin oturumu işaretsiz girişinkinden uzun yaşar | Uç yanıtı + Veri kaydı | yok | ORTA | BEKLEMEDE |
| VAL-A097 | A | Üye şifresini sıfırlar. | 10 KP-27 · 02 §3.13 | Kayıtlı üye; sahte e-posta altyapısı | Sıfırlama e-postası gider; bağlantıyla yeni şifre belirlenir ve eski şifre geçmez | Uç yanıtı + İş kaydı | mock | ORTA | BEKLEMEDE |
| VAL-A098 | A | Üye şifresini değiştirir. | 10 KP-27 · 02 §3.13 | Oturumu açık üye | Yeni şifre kabul edilir ve eski şifre geçmez | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-A099 | A | Üye adını değiştirir. | 10 KP-27 · 02 §3.13 | Oturumu açık üye | Yeni ad hesapta ve ekranlarda görünür | Uç yanıtı | yok | DÜŞÜK | BEKLEMEDE |
| VAL-A100 | A | Üye e-posta adresini değiştirir. | 10 KP-27 · 02 §3.13.14 | Oturumu açık üye; sahte e-posta altyapısı | Yeni adres doğrulama sürecine girer ve doğrulanınca hesabın e-postası olur | Uç yanıtı + Veri kaydı | mock | ORTA | BEKLEMEDE |
| VAL-A101 | A | Üye adres defterini yönetir. | 10 KP-27 · 02 §3.14 | Oturumu açık üye | Adres eklenir, düzenlenir ve silinir; ekran defteri güncel gösterir | Uç yanıtı + Veri kaydı | yok | ORTA | BEKLEMEDE |
| VAL-A102 | A | E-posta değiştiğinde eski adrese bildirim gider. | 10 KP-27 · 02 §3.13.14, §8.2.6 | E-postası değiştirilmiş hesap; sahte e-posta altyapısı | Eski adrese değişikliği bildiren ve geri alma bağlantısı taşıyan e-posta gönderilir | İş kaydı | mock | ORTA | BEKLEMEDE |
| VAL-C028 | C | Bildirimdeki bağlantıyla e-posta değişikliği yedi gün içinde geri alınır: hesap eski adrese döner, bütün oturumlar kapanır ve şifre sıfırlatılır. | 10 KP-27 · 02 §3.13.14, §8.2.6 | E-postası değiştirilmiş hesap; geri alma bağlantısı | Geri almada hesabın e-postası eski adres olur, hesabın bütün oturumları kapanır ve şifre sıfırlatılana kadar giriş yapılamaz | Veri kaydı + Uç yanıtı | yok | KRİTİK | BEKLEMEDE |
| VAL-C029 | C | E-posta değişikliği yedi günden sonra geri alınamaz. | 10 KP-27 · 02 §3.13.14 | Aynı bağlantı; zaman yedi günü aşacak şekilde ileri alınır | Bağlantı geri alma yapmaz ve hesap yeni adreste kalır | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-A103 | A | Şifre değiştirme, e-posta değiştirme ve hesap silme oturum açıkken de yeniden doğrulama ister. | 10 KP-27 · 02 §8.2.6, §11 P-47 | Oturumu açık üye | Üç işlemin her biri yeniden doğrulama yapılmadan uygulanmaz | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-D017 | D | Şifre değiştiğinde hesabın öteki oturumları kapanır ve öteki tarayıcıların tanınan tarayıcı işaretleri düşer. | 10 KP-27 · 02 §3.13, §8.2.6 | İki tarayıcıda açık oturumlu üye | Değişikliğin yapıldığı oturum dışındaki oturumlar kapanır ve öteki tarayıcıdaki tanınan tarayıcı işareti geçersizleşir | Veri kaydı + Uç yanıtı | yok | KRİTİK | BEKLEMEDE |
| VAL-D018 | D | Hesap açan kişinin doğrulanmış e-postasına ait geçmiş misafir siparişleri hesabına düşer. | 10 KP-28 · 02 §3.13 | Aynı e-postayla verilmiş misafir siparişleri; e-postası doğrulanan yeni hesap | Siparişler hesabın sipariş geçmişinde görünür | Veri kaydı + Uç yanıtı | yok | KRİTİK | BEKLEMEDE |
| VAL-D019 | D | Doğrulanmamış e-posta adresine ait misafir siparişleri hesaba düşmez. | 10 KP-28 · 02 §3.13 | Aynı e-postayla verilmiş misafir siparişleri; e-postası doğrulanmamış hesap | Siparişler hesaba bağlanmaz ve hesabın geçmişinde görünmez | Veri kaydı + Uç yanıtı | yok | KRİTİK | BEKLEMEDE |
| VAL-D020 | D | Hesabı olan kişinin giriş yapmadan verdiği sipariş de hesabına düşer. | 10 KP-28 · 02 §3.13 | Hesabı olan e-posta; oturumsuz verilmiş sipariş | Sipariş hesabın sipariş geçmişinde görünür | Veri kaydı + Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-D021 | D | Firma misafir siparişinin e-postasını düzelttiğinde sipariş eski adresin hesabından düşer; yeni adres doğrulanmış bir müşteri hesabınınsa o hesaba düşer. | 10 KP-28 · 02 §10.4.11 | Hesaba bağlı misafir siparişi; yeni adres doğrulanmış başka bir hesabın adresi | Sipariş eski hesabın geçmişinden çıkar ve yeni adresin hesabının geçmişine girer; yeni adres hesapsızsa hiçbir hesapta görünmez | Veri kaydı + Uç yanıtı | yok | KRİTİK | BEKLEMEDE |
| VAL-D022 | D | Hesap silinince hesabın adı, giriş bilgileri, oturumlar, adres defteri ve sepet hemen silinir ve tanınan tarayıcı işaretleri geçersizleşir. | 10 KP-29 · 02 §3.15, §3.15.1 | Adres defteri, sepeti ve açık oturumları olan üye | Beş veri kümesi silinir; silinen hesapla giriş yapılamaz ve eski tanınan tarayıcı işareti geçmez | Veri kaydı + Uygulama kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-D023 | D | Hesap silinince siparişler yasal saklama süresi boyunca firmanın ticari kaydı olarak kalır ve misafir yolundan izlenmeye devam eder. | 10 KP-29 · 02 §3.15.6 | Siparişi olan üye; hesap silinir | Sipariş kayıtları yerinde durur; sipariş numarası ve e-postayla sipariş sayfası açılır | Veri kaydı + Uç yanıtı | yok | KRİTİK | BEKLEMEDE |
| VAL-A104 | A | Silinen hesabın adresine silindiğini ve açık siparişlere girme yolunu söyleyen bir e-posta gider. | 10 KP-29 · 02 §3.15.1, §8.2.6 | Açık siparişi olan üye; hesap silinir; sahte e-posta altyapısı | Hesabın e-postasına silindiğini ve açık siparişlere girme yolunu söyleyen e-posta gönderilir | İş kaydı | mock | ORTA | BEKLEMEDE |
| VAL-C030 | C | E-posta değişikliğini geri alma bağlantısı açıkken hesap silinmez. | 10 KP-29 · 02 §3.15.6 | E-postası yeni değiştirilmiş hesap; geri alma bağlantısı kullanılmamış | Silme isteği reddedilir ve hesap yerinde kalır; bağlantı kapanınca silinebilir | Uç yanıtı + Veri kaydı | yok | ORTA | BEKLEMEDE |
| VAL-A105 | A | İlgili kişi kişisel verisiyle ilgili başvurusunu iletişim formunun "KVKK talebi" tipiyle yapar. | 10 KP-30 · 02 §3.15.4, §12.2.6 | Ziyaretçi ya da müşteri | İletişim formunda "KVKK talebi" tipi seçilebilir ve talep firmanın panelinde bu tiple görünür | Uç yanıtı + Veri kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-A106 | A | Tebliğ'in diğer başvuru yolları açık kalır ve aydınlatmada sayılır. | 10 KP-30 · 02 §12.2.6 | Yayında aydınlatma metni | Aydınlatma metni iletişim formu dışındaki başvuru yollarını sayar | Tarayıcı gözlemi | yok | ORTA | BEKLEMEDE |
| VAL-A107 | A | Firma yöneticisi panelin salt okunur üye kaydı görünümünde e-postayla arar. | 10 KP-77 · 02 §3.15.5, §10.1.2 | Panel oturumu; kayıtlı üyeler | E-postası verilen üye bulunur; olmayan e-posta sonuç vermez | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-A108 | A | Görünüm hesabın varlığını ve adını, adres defterini, giriş yollarını ve bağlı siparişleri gösterir. | 10 KP-77 · 02 §3.15.5 | Adres defteri, giriş yolu ve siparişi olan üye | Beş bilgi görünümde yer alır | Uç yanıtı + Tarayıcı gözlemi | yok | ORTA | BEKLEMEDE |
| VAL-A109 | A | Görünümde düzenleme yoktur. | 10 KP-77 · 02 §3.15.5 | Panelde üye kaydı görünümü | Görünümde değiştirme düğmesi ve alanı yoktur; değiştirme isteği reddedilir | Uç yanıtı | yok | ORTA | BEKLEMEDE |
| VAL-A110 | A | [güvenlik] Üye kaydı görünümü yalnız firma yöneticisine açıktır. | 10 KP-77 · 02 §3.15.5, §10.1.2 | Müşteri oturumu ve oturumsuz istek | İki istek de görünümü açmaz ve üye verisi dönmez | Uç yanıtı + Uygulama kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-D024 | D | Firma yöneticisi ilgili kişinin talebi üzerine hesabı siler ve silme işlem izine yazılır. | 10 KP-77 · KP-29 · 02 §3.15.6 | Panel oturumu; silinecek üye | Hesap KP-29'un veri kümeleriyle silinir ve işlem izine yöneticiyi ve silinen hesabı gösteren satır yazılır | Veri kaydı + İş kaydı | yok | KRİTİK | BEKLEMEDE |
| VAL-C031 | C | E-posta değişikliğini geri alma bağlantısı açıkken firma hesabı silemez. | 10 KP-77 · 02 §3.15.6 | E-postası yeni değiştirilmiş hesap; panel oturumu | Silme reddedilir ve hesap yerinde kalır | Uç yanıtı + Veri kaydı | yok | ORTA | BEKLEMEDE |

---

## 5. Giriş kriterleri

Bir doğrulamanın başlayabilmesi için sağlanması gerekenler; önce değişmez kapılar ve kurallar, sonra seviyeye göre ek girişler. Kararlar K-1517'dedir (`VP5-01`).

**Değişmez kapılar (`00 §F.1`; HARD STOP).** Her doğrulama (task doğrulaması ve gate) üç kapıdan geçerek başlar; kapı geçilmeden hiçbir satıra durum verilmez.

| # | Kapı | Kontrol |
|---|---|---|
| -1 | Working tree temiz | `git status --short` boş; ayrıca hook'lar kurulu — `git config core.hooksPath` değeri `scripts/git-hooks` (`skills/validate` Adım -1) |
| 0 | Ana dal CI | Ana dalın son 3 tamamlanmış run'ı `success` |
| 0b | Repo hafızası güncel *(yalnız task doğrulaması)* | `.claude/memory/MEMORY.md`'de task'ın satırı var; yoksa BLOCKED (`DEPENDENCY_MISMATCH`) |

Kapıların başarısızlıkta işleyişi ve yasak rasyonelizasyonlar `00 §F.1`'dedir ve burada yinelenmez.

**Değişmez giriş kuralları:**

| # | Kural |
|---|---|
| 1 | **Ön koşulu sağlanmayan madde `BEKLEMEDE` kalır — PASS veya FAIL verilemez.** Doğrulanamayan bir maddeyi FAIL saymak yanlış sinyal, PASS saymak kanıtsız onaydır. Kabul kriteri düzeyindeki karşılığı `? Doğrulanamadı`'dır (`00 §D.7`) — FAIL değildir, PASS için çözülmesi gerekir. |
| 2 | **Kanıt ortamı kayıt altındadır.** Beklenen sonucu bir dış servisin davranışına dayanan satır (seviye E ve onlara bağlı B, C, D, F satırları), hangi servisin `mock`, `sandbox` ya da `gerçek` olduğu satırın "Kanıt ortamı" sütununa yazılmadan **başlatılamaz** (§7.4). |
| 3 | Task bazlı doğrulama, task'ın kabul kriterleri ve doğrulama kontrol listesi `11`'de tanımlı olmadan başlatılamaz (`11` §5 "Task yapısı" madde 5 ve 7). |

**Seviyeye göre ek girişler:**

| Doğrulama | Ek giriş koşulu |
|---|---|
| Task doğrulaması (A–E) | Task'ın yapım PR'ı açıktır; task dalının CI'ında en az bir run `success`'tir — hiç run yoksa ya da son run `failure` ise bu bir bulgudur, sessizce geçilmez; dal yoksa ya da PR açılmamışsa BLOCKED (`skills/validate` Adım 8a). Doğrulayıcının girdisi §9'daki listedir; yapım raporu girmez. |
| Seviye E, `sandbox` ya da `gerçek` ortam | Ortamın dış ön koşulu hazırdır: e-posta altyapısı (ÖK-3), ödeme sağlayıcısı (ÖK-4), Google uygulaması (ÖK-6) ve sağlayıcıya bağlanılan referans kurulum (ÖK-12; `10 §4.1`). Hazır değilse satır `BEKLEMEDE` kalır. `mock` ortamı ön koşul istemez. |
| Faz gate'i (F satırları dahil) | Gate ayrı chat'te koşar; fazın bütün task'ları ✓ ve raporları finalizedir; sürüm referans kuruluma inmiştir (`05 §8.1.4`; `11 §6.1` madde 2); gate raporu ortam denetiminin hangi yapılandırmayla koştuğunu yazar (`skills/gate-check` ön kontrol). |
| `[yük]` satırları | F5 gate'i PASS almıştır; ortam K-1515'in kurulumudur (§12, `VP12-12`). |
| Final çıkış | Yedi fazın hepsinin gate'i PASS'tır (`phase/FX-pass` etiketleri); F6'nın canlı kanıtı alınmıştır (§6.3). |

---

## 6. Çıkış kriterleri

Kapıya bağ (hangi kapı hangi seviyeyi ve türü okur) ve kararlar K-1517'dedir (`VP6-01`). Üç çıkış birbirini içerir: faz çıkışı task çıkışını, final çıkış faz çıkışını varsayar.

### 6.1 Task bazlı çıkış

| Kriter | Açıklama |
|---|---|
| İlgili maddeler PASS | Task'ın kabul kriterlerine ve kontrol listesine bağlı bütün satırlar PASS. Kabul kriterinin sonucu satıra şöyle yansır: `✓ Karşılandı` → `PASS`; `✗ Karşılanmadı` → `FAIL`; `? Doğrulanamadı` → `BEKLEMEDE`; `~ Kısmi` → satır kısmi PASS olamaz — beklenen sonucun tamamı karşılanmadıysa `FAIL`, kriter birden çok satıra bölünmüşse karşılananlar PASS, karşılanmayanlar FAIL |
| Verdict PASS | `skills/validate` verdict'i: tüm kriterler `✓`, güvenlik temiz, testler geçiyor, CI yeşil; FAIL ve BLOCKED'da merge yok |
| KRİTİK açık bulgu yok | Hiçbir KRİTİK satır FAIL ya da BEKLEMEDE değil |
| ORTA bulgular kayıt altında | `DEFERRED_BACKLOG.md`'ye yazılmış (§8.4); kaydı olmayan bulgu yoktur (`00 §E.3`) |
| Mini güvenlik kontrolü temiz | Secret sızıntısı · auth etkisi · girdi doğrulaması · yeni dış bağımlılık (`00 §D.6`) |
| Kanıt üretilmiş | §7'ye uygun; her kanıt satır kimliğiyle eşleşmiş |
| Rapor, status ve hafıza merge'den önce | Önce rapor, sonra status (`00 §D.9`); satırların `Durum` hücresi aynı commit'te güncellenir (§7.5) |

### 6.2 Faz bazlı çıkış

| Kriter | Açıklama |
|---|---|
| Faz içi tüm task'lar §6.1'i karşılamış | Raporlar finalize, status tutarlı |
| Entegrasyon kontrolü yapılmış | Gate'in build, ortam ve faz-spesifik kontrolleri (`11 §6.2`) |
| Regresyon kontrolü yapılmış | Önceki fazlar dahil A–E satırları yeniden okunur; yeni FAIL §11.1'e göre işler |
| Açık risk envanteri güncel | `DEFERRED_BACKLOG.md`'deki kabul edilmiş riskler proje sahibince görülmüş ve kabul edilmiş (§8.4) |
| Öğrenim terfisi yapılmış | `00 §K` |

Fazın ölçütleri `11 §6.2`'dedir ve bu bölümde yinelenmez; tablo yalnız **hangi satırların** gate'te okunduğunu söyler:

| Faz | Gate'te okunan satırlar |
|---|---|
| F0 | F — boot provasının 1. turu, canlılık ucu |
| F1 | D — göçün ileri, idempotent ve geri alınabilir olması; model eşitlik denetimleri |
| F2 | D — kayıt maskeleme |
| F3 | B — ilk uçtan uca akış (T329) · C — durum makinesi geçiş tablosunun `02 §5.4` ve §5.5 ile eşitliği |
| F4 | E — PayTR, SES ve Google sandbox kanıtı, imza doğrulaması; DV-1, DV-2, DV-9 |
| F5 | A `[erişilebilirlik]` — A seviyesi tarama temiz · B `[tarayıcı]` — betiksiz yol |
| F6 | F — açılış provası, canlı kanıt, `[güvenlik]` taraması · E `gerçek` — K-23'ün zinciri |

### 6.3 Final çıkış (MVP release gate)

| Kriter | Açıklama |
|---|---|
| Tüm KRİTİK maddeler PASS | Hiçbiri FAIL veya BEKLEMEDE olamaz |
| ORTA maddeler PASS veya kabul edilmiş risk | |
| DÜŞÜK maddeler bloklamaz | Ama kayıtlıdır |
| Operasyonel doğrulama tamamlanmış | Seviye F: açılış provası, yedek ve kurtarma, canlılık, `[güvenlik]` taraması ve `[yük]` iki satırı. **Yük koşusu faz kapısı değildir:** F5 gate'i PASS aldıktan sonra, final çıkıştan önce koşar ve final çıkışın bu koşuluna girer (K-1515 (8)) |
| Happy path uçtan uca geçmiş | Ürün → sepet → sipariş → ödeme → kargo → teslim; yönetici panelinden işletme |
| Çekirdek entegrasyonlar **mock olmayan** ortamda doğrulanmış | Seviye E satırları (ödeme sağlayıcısı, e-posta altyapısı, Google) `mock` ile PASS olarak sayılmaz — BEKLEMEDE kalır. K-23'ün canlı kanıt zincirine ve `08 §2.1.10`'un kabul kanıtına düşürdüğü DV satırlarına (DV-6, DV-7, DV-10; test modunun 3D sayfası bankanın gerçek adresi değilse DV-5) yalnız `gerçek` PASS verir |
| Kabul kanıtı alınmış | Referans kurulum, canlı ortam, gerçek para: gerçek kartla küçük tutarlı sipariş, 3D Secure, panelden uçtan uca işletme, iade (K-23; senaryolar `VP12-01`) |
| MVP başarı çıtası karşılanmış | Ü-1…Ü-4 kabul kapısı (`01 §6`; K-415, K-418) |
| Kanıt arşivi erişilebilir | `Docs/TASK_REPORTS/`, `Docs/CHECKPOINT_REPORTS/` ve `DEPLOY_RUNBOOK`'un canlı kanıt satırları (§7.5) |
| Proje sahibi onayı | |

---

## 7. Kanıt standardı

Kararlar K-1518'dedir (`VP7-01`).

### 7.1 Kabul edilen kanıt türleri

| Tür | Örnek |
|---|---|
| Uç yanıtı | İstek + tam yanıt: yöntem, adres, durum kodu, başlıklar ve gövde (vitrin, hesap, panel ve makine uçları — `07`) |
| Tarayıcı gözlemi | Sayfanın tarayıcıdaki hâli: ekran görüntüsü ve gözlenen genişlik/tarayıcı; betikli ve betiksiz yolun ikisi için ayrı (`[tarayıcı]`, `[erişilebilirlik]`) |
| Veri kaydı | İlgili tablo satırının snapshot'ı (`06`); kişisel veri maskeli |
| Uygulama kaydı | İlgili işlemin kayıt satırları, süzülmüş çıktı; sır ve kişisel veri taşımaz |
| İş kaydı | İş kuyruğu satırı, e-posta gönderim kaydı, işlem izi satırı — olayın işlendiğini ya da işlenmediğini gösterir |
| Test raporu | Otomatik test çıktısı ve k6 çıktı özeti (**tekrar edilebilirlik açısından tercih edilir**); commit ve CI run numarası ile |
| Dış sistem kaydı | Üçüncü parti tarafındaki işlem kaydı: ödeme sağlayıcısı panelindeki işlem, e-posta altyapısındaki gönderim ve geri dönme kaydı, Google tarafındaki oturum kaydı |
| İzleme/uyarı | Durum ucunun yanıtı (200 / 503), panelde açılan uyarı |
| Runbook kaydı | `DEPLOY_RUNBOOK`'ta işaretlenmiş prova satırı ya da kutu (DV-1, DV-2, DV-9'un gözlemi referans kurulumun işaretli kaydından okunur — `08 §2.1.10`) |

### 7.2 Severity'ye göre minimum kanıt

| Severity | Minimum |
|---|---|
| KRİTİK | **En az 2 farklı türde** kanıt (çok katmanlı — §2 P3); biri sonucun kendisini, öteki yan etkisini (kayıt, iz, dış sistem) göstermelidir |
| ORTA | 1 kanıt türü |
| DÜŞÜK | 1 kanıt türü |

### 7.3 Kanıt kuralları

- Kanıt **doğrulama anında** üretilmiş olmalıdır — eski veya başka ortamdan alınan kanıt geçersizdir; kanıtın yanında çalıştırılan komut ve bakılan commit yazılır (`00 §D.7`).
- Kanıtlar satır kimliğiyle eşleştirilerek saklanır (§7.5).
- Tek bir arayüz gözlemi KRİTİK satırlarda tek başına yeterli değildir.
- *"Lokal makinemde geçiyor"* CI kanıtını ikame etmez.
- Kanıt **sır ve kart verisi taşımaz:** anahtar, parola, kart numarası ve CVV kanıta yazılmaz; kişisel veri maskelenir. Canlı kanıt satırları (gerçek kartlı sipariş) sipariş numarasını ve tutarı yazar, kart bilgisini yazmaz.

### 7.4 Kanıt ortamı

"Kanıt ortamı" sütunu, satırın sonucunun **dış servisin hangi hâlinde** üretildiğini kaydeder. Değer kümesi dörttür:

| Değer | Anlamı |
|---|---|
| `mock` | Dış servis sahtedir: `FakePaymentGateway`, `FakeEmailGateway`, `FakeGoogleIdentityGateway` (`09 §19.1` madde 3, 6) — CI ve geliştirme makinesi; veritabanı gerçek PostgreSQL'dir |
| `sandbox` | Servisin kendi test imkânı: PayTR'de referans mağazanın `test_mode` = `1` işlemi, SES sandbox'ı ve posta kutusu simülatörü, Google'ın test uygulaması (`08 §2.1.10`, §2.2) |
| `gerçek` | Referans kurulum (ÖK-12), gerçek yapılandırma, gerçek servis — gerçek kart, SES üretim erişimi, Google üretim uygulaması (K-23) |
| `yok` | Beklenen sonuç hiçbir dış servisin davranışına dayanmıyor — çoğu A, C ve D satırı, tarayıcı ve erişilebilirlik satırları |

Sahte bir sağlayıcıyla koşan ama sonucu sağlayıcının davranışına dayanmayan satır (ör. yük koşusunun ölçtüğü iki istek — sağlayıcıya yönlendirme işlemden sonradır, K-1515) `mock` yazar. Ortam değerinin sonuçları: `gerçek` olmayan bir değer final çıkışta çekirdek entegrasyon satırını PASS saymaz (§6.3), ortam değişince önceki PASS geçersizleşir (§11.1 madde 4).

### 7.5 Kanıtın ve durumun saklanışı

- **Task satırları:** kanıt, task doğrulama raporunun (`Docs/TASK_REPORTS/TXX_REPORT.md`, "Doğrulama") kabul kriterleri ve bulgu tablosundadır; her kanıt satır kimliğiyle anılır.
- **Faz satırları:** gate raporunda (`Docs/CHECKPOINT_REPORTS/GATE_CHECK_FX.md`).
- **Canlı kanıt:** `DEPLOY_RUNBOOK`'un canlı kanıt satırlarında; final çıkış raporu satırlara bu kayıtla bağlanır.
- **Durum:** matrisin `Durum` sütunu **işletim alanıdır**, kural değildir. Yazımda her satır `BEKLEMEDE` doğar; durumu doğrulayıcı kendi commit'inde (rapor + status + hafıza, `skills/validate` adım 16) günceller. `Durum` hücresinin değişmesi `12`'nin sürümünü artırmaz; `Kural`, `Beklenen sonuç`, `Severity`, `Kanıt ortamı` hücrelerinin değişmesi artırır ve bir karar satırı ister.

---

## 8. Severity ve hata yönetimi

Kararlar K-1519'dadır (`VP8-01`, `VP8-02`, `VP8-03`).

### 8.1 Sınıflandırma

Severity, hatanın **etkisinin ağırlığını** söyler. `00 §D.10`'un S1 / S2 / S3 hata sınıfı ise hatanın **türünü** söyler. İki ayrı eksendir (K-1511 (3)); biri ötekini belirlemez.

| Severity | Tanım |
|---|---|
| **KRİTİK** | Veri/varlık kaybı, yanlış sahiplik, yasak durum geçişi, mükerrer tamamlanma, güvenlik ihlali. Bu üründe ayrıca: yanlış tutar ya da çift tahsilat, başka bir müşterinin verisinin görünmesi, yasal yükümlülüğü atlayan davranış (yasal metnin eksik ya da yanlış gösterilmesi, cayma ve iade süresinin yanlış işlemesi). Release'i mutlak bloklar. |
| **ORTA** | Yanlış retry davranışı, eksik log, kurtarma zayıflığı, kayıp yaratmayan iş kuralı defekti. Kayıtlı risk olarak kabul edilebilir. |
| **DÜŞÜK** | Arayüz kusuru, metin hatası, operasyonu bloklamayan görünürlük sorunu. |

| Hata sınıfı (`00 §D.10`) | Anlamı | Doğrulamada nerede doğar |
|---|---|---|
| **S1 — Sapma** | Task tamamlandı ama dokümanla uyumsuz | Task doğrulamasında FAIL alan satır |
| **S2 — Kırılma** | Bir task başka bir task'ın çıktısını bozuyor | Gate regresyonunda ya da yeniden doğrulamada (§11.1 madde 2) PASS'ten FAIL'e düşen satır |
| **S3 — Eksik** | Eşlenmiş öğe implement edilmemiş | Gate'in boşluk taramasında — bir satırın kaynak birimine bağlı task'ın çıktısı yok |

Defect kaydı **ikisini birlikte** taşır. S2 sınıfı otomatik KRİTİK değildir; yeniden doğrulamada FAIL çıkan satır severity'sini ayrıca alır.

### 8.2 FAIL akışı

```
FAIL tespit edilir
  → Defect kaydı (doğrulama raporunun bulgu tablosu — aşağıda)
  → Düzeltme (etki alanı belirlenir; numara ve dal 11 §4'e göre)
  → Yeniden doğrulama (task'ın tamamı + regresyon — §8.3, §11)
  → PASS → kapatılır, kanıt eklenir | Tekrar FAIL → döngüye döner (§8.5)
```

**Defect kaydı** Implementation döneminde ayrı bir dosya değildir: kaydın evi, task doğrulama raporunun (gate bulgularında gate raporunun) **bulgu tablosudur** (K-1511 (4)). Tablo şu alanları taşır:

| Alan | İçerik |
|---|---|
| # | Bulgu sırası |
| Madde | `VAL-…` kimliği |
| Severity | KRİTİK / ORTA / DÜŞÜK |
| Hata sınıfı | S1 / S2 / S3 |
| Beklenen ↔ gerçekleşen | Satırın beklenen sonucu ve gözlenen sonuç, kanıta bağlı |
| Etkilenen komşu maddeler | Aynı akışa ait satırlar; yeniden doğrulamanın kapsamı buradan okunur |
| Karar | Bu PR'da düzeltildi / `DEFERRED_BACKLOG`'e yazıldı (kimliği) / kabul edilmiş risk |

Düzeltme `11 §4`'ün kuralıyladır: merge'den önce bulunan S1 aynı PR'da düzelir; merge'den sonrakinin düzeltmesi `TXXa` numarasını alır; düzeltme sonraki faza ertelenmez. Kapanmayan ORTA ve DÜŞÜK bulgu `DEFERRED_BACKLOG.md`'ye taşınır (§8.4); yeni bir kayıt dosyası açılmaz.

### 8.3 Regresyon kuralları

- **FAIL alan task'ın yeniden doğrulaması task'ın bütün satırlarını kapsar** — hedefli kapsam yoktur (`INSTRUCTIONS §3.3`, `[yeniden-dogrulama]`). Aşağıdaki kurallar **öteki task'ların** satırları içindir.
- KRİTİK durum-akışı veya veri-akışı düzeltmelerinde **aynı akışa ait tüm maddeler** yeniden doğrulanır. Dar kapsam kabul edilmez.
- ORTA/DÜŞÜK düzeltmelerde yalnız doğrudan etkilenen maddeler (defect kaydının "etkilenen komşu maddeler" alanı).
- **Şüphe durumunda yeniden doğrulanır.** Şüpheli maddeyi atlamak kabul edilmez.

### 8.4 Kabul edilmiş risk

Koşullar: severity **KRİTİK olamaz** · proje sahibinin **o risk için** açık onayı zorunlu (toplu onay ve ⚠ öneriyle kayıt modları bunun yerine geçmez) · risk kaydı oluşturulur · `DEFERRED_BACKLOG.md`'ye yazılır · final çıkışta yeniden değerlendirilir.

**Kayıt biçimi.** Backlog satırı `00 §H`'nin alanlarını taşır (ID, kanıt, tip `k-note`, öncelik, "neyi bloklar") ve açıklaması "KABUL EDİLMİŞ RİSK — `VAL-…`" diye başlayıp madde kimliğini, açıklamayı, olası etkiyi, mitigasyonu ve kabul tarihini yazar. Matrisin `Durum` hücresi `KABUL EDİLMİŞ RİSK` olur ve backlog kimliğini anar; onay alınmadan hücre `FAIL` kalır. Her faz gate'inde açık risk kalemleri kod düzeyinde yeniden doğrulanır (`00 §H` hijyen taraması).

### 8.5 Tıkanma yasağı

- Bir FAIL düzeltilirken bağımsız task'ların doğrulaması devam eder (faz içi paralellik kuralı: `11 §3.1`).
- Düzeltme **2 iterasyonda** çözülemezse proje sahibine eskale edilir: kapsam daraltma, alternatif yaklaşım veya kabul edilmiş risk. İterasyon, bir düzeltme ve onu izleyen tam yeniden doğrulamadır.
- **BLOCKED, FAIL değildir:** doğrulama yapılamıyorsa (kod eksik, dal yok, önceki task'ın borcu) BLOCKED verilir ve alt türü yazılır (`SPEC_GAP`, `DEPENDENCY_MISMATCH`, `PLAN_CORRECTION_REQUIRED`, `EXTERNAL_BLOCKER`); akış `00 §D.8`'dedir ve BLOCKED sessizce geçilmez.

---

## 9. Ajan çalışma modeli

Kararlar K-1520'dedir (`VP9-01`). Düzenin kuralları `INSTRUCTIONS §3.1`, §3.3'tedir; bu bölüm onları tekrarlamaz, doğrulamanın rollerini ve girdisini söyler.

| Rol | Sorumluluk | Context |
|---|---|---|
| **Yapım işçisi** (coding agent) | Task'ı implement eder, testleri yazar, PR açar, rapor taslağını yazar; dal CI'ını izlemez, doğrulamayı başlatmaz | Kendi context'i — temiz bağlam |
| **Doğrulayıcı** (validator; reviewer agent) | Satırları çalıştırır, kanıt üretir, PASS / FAIL / BLOCKED verir; `skills/validate`'i 16. adıma kadar koşar. Yönetici yardımcı düzeninde **merge etmez** | **Ayrı, izole context** — yapım raporunu görmez; `general-purpose` (fork değil), Opus + `high` |
| **Gate-check** | Faz sonu regresyonu ve kapı ölçütleri; `skills/gate-check` | Ayrı chat |
| **Yardımcı** (yönetici yardımcı düzeni açıkken) | Talimatı yazar, doğrulayıcıyı kendisi açar, sonucu rapor dosyasından okur; PASS ve dal CI yeşilse merge eder | Kodlamaz, doğrulamaz, PASS / FAIL vermez |

Aynı ajan aynı task için iki rolü üstlenemez (§2 P5). Yardımcı doğrulayıcı da olamaz; doğrulayıcının merge-öncesi veto yetkisi korunur — merge'ün tek kapısı doğrulayıcının PASS'idir (`00 §D.9` madde 4).

**Doğrulayıcıya verilecekler** (`INSTRUCTIONS §3.3`; başka bir şey verilmez): task tanımı, kabul kriterleri, doğrulama kontrol listesi, referans dokümanlar, dal kodu, CI sonuçları; ek olarak task'ın **`12` satırları**. Yapım raporu doğrulayıcı kendi verdict'ini oluşturduktan sonra açılır; commit mesajı ve dal adı gibi ipuçları verdict'e girmez (anchoring yasağı).

**Katman seti ve büyük doküman.** Doğrulayıcı `00 §C.6` tablosundaki katmanlardan görevinin gerektirenini alır: teknik (`05`–`08`), standartlar (`09` — çekirdek ve task'ın ek bölümleri), görev planı (`11` — yalnız task bloğu) ve doğrulama (`12`). Bir katmandaki doküman 100 KB'ı aşıyorsa bütünü verilmez (`INSTRUCTIONS §9` (7)); `12` için görevin satırları başlık notundaki okuma tablosunun komutlarıyla çekilir — task'ın kaynak birimlerinden (`11` §7.1) `grep -nE 'KP-37([^0-9]|$)'` gibi. **Kaynak-güdümlü ek:** bir satırın kaynak sütununda anılan doküman katman setinde yoksa **yine de eklenir** (anılan bölüm okunur, doküman bütünüyle değil).

---

## 10. Roller ve onay

Kararlar K-1520'dedir (`VP10-01`).

| Karar | Onay veren |
|---|---|
| Madde PASS / FAIL / BLOCKED | Doğrulayıcı (kanıtla) |
| Task PR'ının merge'ü | Doğrulayıcının PASS'i ve dal CI yeşili şartıyla: validator chat; yönetici yardımcı düzeninde yardımcı (`00 §D.9` madde 4; K-1505, K-1506) |
| Kabul edilmiş risk | **Proje sahibi** |
| Faz bazlı çıkış | Gate-check verdict'i (`00 §E.3`); açık risk envanterinin kabulü proje sahibinindir |
| Final çıkış | **Proje sahibi** |
| Eskalasyon | **Proje sahibi** |
| Regresyon kapsamını genişletme | Doğrulayıcı (şüphe durumunda genişletir, §8.3); yardımcı ya da ana oturum da genişletilmesini isteyebilir. Daraltma yalnız ORTA/DÜŞÜK düzeltmede §8.3'ün sınırı içindedir |

**Sorumluluk sınırları:** Doğrulayıcı PASS/FAIL verir ama kabul edilmiş risk kararı **alamaz**. Yapım işçisi kendi kodunu doğrulayamaz. Yardımcı PASS ya da FAIL veremez; yalnız doğrulayıcının PASS'inden sonra merge eder. Hiçbir rol tek başına final çıkış kararı veremez. Toplu onay ve ⚠ öneriyle kayıt (K-1504) doküman kararlarının cevaplanışını kolaylaştırır; kabul edilmiş risk, final çıkış ve eskalasyon kararlarını kapsamaz — onlar proje sahibinin o karar için açık onayıdır.

---

## 11. Yeniden doğrulama

Kararlar K-1520'dedir (`VP11-01`).

| Tetikleyici | Kapsam |
|---|---|
| Defect düzeltmesi | FAIL alan task'ın **bütün** satırları (tam yeniden doğrulama — `INSTRUCTIONS §3.3`) + etkilenen komşu maddeler (§8.3) |
| Kanıt ortamı yükselmesi (`mock` → `sandbox` → `gerçek`) | O entegrasyonun **tüm** E satırları + sonucu o servise dayanan bağımlı B, C, D, F satırları. **Düşük ortamla alınmış PASS'ler geçersiz sayılır.** Yükselme F4'te (sahte bağlayıcıdan gerçeğine) ve F6'da (referans kurulumda gerçek yapılandırmaya) olur |
| Faz geçişi | Yeni kodun etkileyebileceği önceki faz satırları (gate regresyonu, A–E) |
| Konfigürasyon değişikliği | Değişen ayara (`02 §11`), sırra ya da dış hesaba bağımlı satırlar |
| Altyapı değişikliği | Şema değişimi (göç), ortam değişimi, bağımlılık ya da imaj sürümü güncellemesi → etkilenen seviyedeki tüm maddeler; yeni sürümün referans kuruluma inişinde F seviyesi açılış provası (`05 §8.1.4`) |

### 11.1 Yeniden doğrulama sonuçları

| # | Kural |
|---|---|
| 1 | Re-validation sonucu orijinal maddenin **Durum** alanını günceller (§7.5). |
| 2 | **Re-validation'da FAIL çıkarsa bu bir regresyondur** — normal bir FAIL gibi işlenmez; §8 akışı **S2 Kırılma** olarak başlar. Daha önce PASS almış bir maddenin geri düşmesi, yeni bir maddenin FAIL almasından farklı bir sinyaldir. S2 sınıfı satırın severity'sini belirlemez; satır severity'sini ayrıca alır (§8.1). |
| 3 | **En güncel kanıt geçerlidir.** Yeni kanıt eskisinin yerine geçer; eski kanıt arşivde kalır ama geçerliliğini yitirir. Aynı madde için iki çelişen kanıt yan yana bırakılmaz. |
| 4 | Düşük kanıt ortamında alınmış PASS'ler üst ortama geçildiğinde **geçersiz sayılır** ve madde `BEKLEMEDE`'ye döner (§6.3). Dış servise dayanmayan (`yok`) satırlar bundan etkilenmez. |

---

## 12. Proje-spesifik ekler

> **Ne yazılır:** Bu projenin domain'ine özel doğrulama tabloları — durum geçiş matrisi, kritik hesaplama doğrulamaları, sahiplik/yetki kontrolleri, idempotency kontrolleri, denetim izi kontrolleri.
> Bu bölüm **projeye göre tamamen değişir**; yukarıdaki §1–§11 değişmez.

---

*Shopfolio — Validation Protocol v0.3*
