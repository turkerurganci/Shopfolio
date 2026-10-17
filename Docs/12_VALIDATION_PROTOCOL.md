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
> - **K-787…K-791 · K-794 · K-796 · Kısmi adet senaryoları:** adedi birden büyük kalemde (1) müşterinin kısmi iptali, (2) aynı kalemde ardışık iki kısmi cayma — her beyanın kendi damgası, gönderme süresi ve ulaşma tarihi, (3) eksik ulaşan iadede ulaşan adedin teslim alınması ve kalanların "mal dönmedi" kapanışı, (4) kupon payının kuruş yuvarlaması — kalemin adetleri tek tek işlendiğinde geri ödemelerin toplamı kalemin ödenmiş tutarına eşittir, (5) kargo ücretinin iki tetiği — bir adet kalırken geri ödenmez, son adetle geri ödenir, (6) firma iptalinde adet ve B-7'nin içeriği, (7) kapanış kuralının son açık adetle işlemesi, (8) kupon hakkının yalnız bütün adetlerle dönmesi, (9) gecikme feshinin açık adetlerin tamamına uygulanması doğrulanır (`02 §7.1.6`, §7.1.7, §7.2.9, §7.4.6, §7.4.7). Senaryoların biçimi ve sırası bu aşamanın kararıdır.
> - **K-798 · Panelin yasal uyarıları:** yedi uyarının işlemin formunda, onay penceresinden önce yerinde kalıcı mesaj olarak göründüğü hâller doğrulanır — kanuni faiz uyarısı gecikme feshinin kaydında, başka kanaldan fesih kaydında ve Z-10 ya da Z-39 aşıldıktan sonra yapılmış iptalin kaydında da görünür (`04 §2.4.3`, §9.9). Senaryoların biçimi bu aşamanın kararıdır.
> - **K-808 · Ayıpta sözleşmeden dönme:** talebin ayıplı adetleri için iade hattının işlediği doğrulanır — fiziksel kalemde teslim alma ve ardından geri ödeme, dijital ve hizmet kaleminde doğrudan geri ödeme, havale hattında önce IBAN isteği; geri ödemenin süre sayacı yoktur (`02 §7.5.3`, §12.5; `04 §9.9`).
> - **K-823 · Dışa aktarmanın izi:** her dışa aktarmanın — siparişler, üye listesi, iletişim talepleri — işlem izine yazıldığı ve boş sonuçta dosya üretilmediği doğrulanır (`02 §10.7`; `04 §9.24`).
> - **K-834 · K-850 · İlk kurulumda çerez:** çerez politikası ilk kez yayına alınana kadar vitrinin ziyaretçinin tarayıcısına çerez yazmadığı ve iletişim formunun, hesap kaydının ve Google ile ilk girişin — aydınlatma metni önce yayına alınmış olsa da — kapalı kaldığı doğrulanır (`02 §3.1.5`, §12.2.5; `04 §5.11`; K-850 — satır Aşama 3'ün çakışma taramasında hizalandı).
> - **Dizin — Aşama 3 kararlarının bu dokümana devirleri:** karar kaydında K-723…K-850'nin etki sütunlarında bu dokümanı gösteren on sekiz atıf (on sekiz karar) ve Arayüz Tanımları'nın (`04`) gövdesinde bu dokümana iş bırakan cümleler [`CHECKPOINT_REPORTS/PHASE3_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE3_CONFLICT_SCAN.md) §6'dadır; Aşama 3'ün atıflarının hepsi işin adını taşır. Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Önceki aşamaların dizinleri: `PHASE1_CONFLICT_SCAN.md` §6, `PHASE2_CONFLICT_SCAN.md` §6.

> **Aşama 4'ten park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §12.3 TM0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-884 · K-980 · İndirme hakkının düştüğü an:** hak, indirme düğmesine basılıp dosyanın gönderimi başladığında bir kez düşer; bir saat ömürlü bağlantıdaki devam istekleri hak düşürmez; ömür yalnız isteğin kabulünde denetlenir — ömür dolarken süren aktarım kesilmez; ömür dolduktan sonraki indirme yeni bir haktır; son hakkı kalan kalemde aynı anda iki indirmeden yalnız biri hak düşürür ve dosyayı alır (Teknik Mimari `05 §7`, §7.7.4; `02 §3.12.5`; K-980). Doğrulama senaryoları bu aşamanın kararıdır.
> - **K-890 · K-893 · K-898 · K-946 · K-957 · K-967 · K-970 · K-975 · K-980 · Eşzamanlılık, tek işlenme ve kesinti:** (1) aynı son adedi aynı anda onaylayan iki müşteriden yalnız biri ayırır, öteki kalemin ayrılamadığını görür; (2) ödeme adımında iki kez gönderilen onay tek sipariş açar — onay anahtarı onayın ilk adımında aranır, ikinci gönderim bir şey yazmadan var olan siparişin sayfasına gider (`05 §4.2.2`; K-970); (3) aynı sağlayıcı bildirimi iki kez gelir ve ödeme bir kez uygulanır — bildirim ile son sorgu yarışırsa da; iptal edilmiş siparişe aynı ödeme hem son sorguyla hem bildirimle gelirse tek kart iadesi doğar; aynı işlemin başarısız ve başarılı denemesi ayrı bildirimlerdir (`05 §4.5.2`, §4.5.3; K-970); (4) havale ödeme süresi kesintide dolar — iptal dönüşten sonraki ilk iş gününün sonuna ertelenir, sipariş sayfası yeni son günü gösterir, yeni hatırlatma gitmez ve kesinti hatırlatmanın anını da kapsadıysa kaçırılmış hatırlatma da gitmez; kaçan işler zamanlarına göre sırayla çalışır, kesinti 03:00'ı kapsadıysa gecelik iş onlardan sonra ilk uyanışta çalışır; iki dakikadan kısa nabız boşluğu kesinti değildir ve iptali ertelemez; son anı bir kesintinin — kısa ya da uzun — ilk iki dakikasına denk gelen sipariş ertelenmez, kalan risktir; yalnız ters vekil durduğunda öteki işler sürer, yalnız havale iptali bekler (`05 §5.4.1`, §5.4.3, §5.4.6; K-975); (5) aynı sepetin ödenmemiş önceki siparişinin ödeme süresi dolarken müşteri aynı sepetle yeniden onaylar — ölü kilit doğmaz, iki işlemden biri sırasını bekler; (6) önceki ödenmemiş sipariş bir ürünün son adedini ayırmışken müşteri aynı sepetle yeniden onaylar — önceki sipariş iptal edilir ve son adet yeni siparişe ayrılır, müşteri kendi ayırdığı stoğa takılmaz (`02 §3.17.7`); (7) aynı sepetle iki sekmeden, ayrı onay anahtarlarıyla iki onay aynı anda gönderilir — biri sepetin kilidini bekler ve öncekinin siparişini ödenmemiş önceki sipariş olarak iptal eder; aynı sepette iki ödenmemiş sipariş kalmaz (`05 §4.2.2`; K-967); (8) yönetici ürün formunu açtıktan sonra bir satış stoğu düşürür — kaydetme stoğu formun gösterdiği değerle geri yazmaz; stok ayrılmış adedin altına indirilemez (`05 §7.3.1`; K-980) (Teknik Mimari `05 §4`, §5, §7.3.5; `02 §4.3`). Senaryoların kurulumu bu aşamanın kararıdır.
> - **K-904 · K-907 · K-908 · K-971 · Bağlantılar ve çerezler:** (1) e-postadaki doğrulama ve geri alma bağlantısının açılması bağlantının, hesabın ve siparişin hiçbir kaydını değiştirmez — oturumun son işlem anı, form güvenliği çerezi, hız sınırının sayacı ve erişim kaydı bunun dışındadır (`05 §6.4.7`; K-971); doğrulama ve geri alma ekrandaki düğmeyle olur; (2) ödeme adımından girişe gidip dönen müşterinin yazdıkları durur — JavaScript kapalıyken de ve sipariş onaylanınca o çerez silinir; (3) çerez politikası yayına alınana kadar vitrinin hiçbir yanıtı çerez yazmaz — ara katman yanıttan çerez başlıklarını çıkarır (`05 §6`; `02 §8.1.3`, §12.2.5). Senaryoların kurulumu bu aşamanın kararıdır.
> - **K-913 · K-914 · K-918 · K-946 · K-975 · Uyarılar ve hata hâlleri:** (1) e-posta servisine art arda beş deneme ve en az on beş dakika boyunca kanal hatasıyla ulaşılamazsa kanal uyarısı açılır, ilk başarılı gönderimde kalkar; alıcı hatası uyarı açmaz; (2) firma bildirimleri uyarısı bir sonraki firma bildirimi gönderildiğinde ve iletişim e-postası değiştirildiğinde kalkar; (3) sayfada, formda ve sonucu belirsiz işlemde beklenmeyen hata `04` 2.7.3'ün üç hâlini gösterir ve teknik ayrıntı taşımaz; (4) durum ucunun her uyarı koşulu 503 döndürür, canlılık ucu bundan etkilenmez — iş kuyruğunun zamanı gelmiş en eski satırı on beş dakikadan eskiyse ya da "başarısız" satır varsa ve gecelik imhanın son başarılı çalışması yirmi altı saatten eskiyse dahil; bir koşulun kaynağı okunamazsa — yetki, bağlantı ya da dosya hatası — koşul açık sayılır ve uç 503 döner (`05 §9.4.10`; K-975); (5) kuralı yazılmamış bir iş türünde sürekli hata veren satır beş yeniden denemeden sonra "başarısız" olur ve sıradaki satırları bekletmez; uyarı gecelik temizlikle kalkmaz, satır yönetim komutuyla yeniden kuyruğa alınınca ya da kapatılınca kalkar; başarısız biten gecelik iş bir saat dolmadan yeniden başlamaz (Teknik Mimari `05 §4.3`, §9, §10). Senaryoların kurulumu bu aşamanın kararıdır.
> - **K-928 · Tasarım değerleri:** elli kalemli sepette ödeme öncesi yeniden değerlendirme ve sipariş onayı isteklerin %95'inde bir saniye içinde; dakikada yüz farklı ziyaretçi yükünde sayfa yanıtlarının %95'i 500 milisaniye içinde — referans sunucu ölçüsünde (2 sanal işlemci, 4 GB) (`05 §11`; `02 §11.3` H-3, H-4). Yük senaryosunun kurulumu bu aşamanın kararıdır.
> - **K-951 · Elektronik ticaret kayıtlarının on yıllık saklaması:** zaman ilerletilerek doğrulanır — (1) ödemesi alınmış siparişin gönderim kaydı üç yıl dolunca silinmez, on yıl dolunca silinir; (2) ödemesi hiç alınmamış siparişin gönderim kaydında üç yıl dolunca alıcı ve olayın değerleri boşalır, satır on yıla kadar kalır (`05 §7.4.3`); siparişin kişisel verisi üç yılda silinir, kişisel verisiz kaydı on yıl dolmadan silinmez; (3) "Sipariş hakkında" tipindeki iletişim talebi on yıl, öteki tipler iki yıl dolunca silinir; (4) iptalden sonra gelip sistemce geri ödenen kart ödemesi siparişi ödemesi alınmış koluna koyar (`02 §4.2` Z-28, Z-33, Z-41, Z-43, §6.2.16; Teknik Mimari `05 §7.4.3`). Senaryoların kurulumu bu aşamanın kararıdır.
> - **Dizin — Aşama 4 kararlarının bu dokümana devirleri:** karar kaydında K-855…K-979'un etki sütunlarında bu dokümanı gösteren yirmi atıf (yirmi karar) ve Teknik Mimari'nin (`05`) gövdesinde bu dokümana iş bırakan cümleler [`CHECKPOINT_REPORTS/PHASE4_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE4_CONFLICT_SCAN.md) §6'dadır; Aşama 4'ün atıflarının hepsi işin adını taşır. Aşama 4'ün checkpoint'inin kararları (K-980…K-988) tarama raporunda yoktur — bu dokümana bıraktıkları iş yukarıdaki satırlarda etiketiyle durur. Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Önceki aşamaların dizinleri: `PHASE1_CONFLICT_SCAN.md` §6, `PHASE2_CONFLICT_SCAN.md` §6, `PHASE3_CONFLICT_SCAN.md` §6.

> **Aşama 5'ten park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §14.2, yazım turu ve kapanış sırasının 2. maddesi).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-1015 · Sayaçların değişmezi:** varyantın ve hizmet kontenjanının ayrılmış adedi ödemesi beklenen siparişlerdeki ayırmaların toplamına eşittir; kuponda kullanılmış ve ayrılmış haklar toplam kullanım adedini aşmaz (Veri Modeli `06 §2`; `02 §3.6.7`, §3.10.6). Onaydan, ödemeden, süre dolumundan, başarısızlıktan, iptalden ve varyantın kalıcı silinmesinden sonra değişmezin tuttuğunu gösteren senaryolar bu aşamanın kararıdır.
> - **K-1018 · Birim fiyat:** birim fiyat temel birimin bir ölçüsü için hesaplanır — gram, mililitre ve santimetre girişi kilograma, litreye ve metreye çevrilir —, kuruşa en yakın, yarım yukarı yuvarlanır, indirimdeki varyantta indirimli fiyattan hesaplanır ve satış fiyatıyla aynıysa gösterilmez (`02 §3.8.5`, §3.8.3). Hesabın ve gösterimin senaryoları bu aşamanın kararıdır.

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
