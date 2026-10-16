# Shopfolio — Data Model

**Versiyon: v0.1** | **Bağımlılıklar:** `02`, `03`, `05`, `09`, `10_MVP_SCOPE.md` | **Son güncelleme:** YYYY-AA-GG

> **Aşama:** 5 — Veri Modeli · **Rol:** Data / Domain Modeler
> **Traceability zorunlu: EVET** — §1 tamamlanmadan §3'e (entity envanteri) geçilmez.

> **Aşama 1'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36.
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-06 · K-97 · Aktörler ve yetki derinliği:** Dört aktör — ziyaretçi · misafir alıcı · üye müşteri · firma yöneticisi (`02 §1.3`; misafir alıcıyı K-97 ekledi, satır Aşama 2'nin çakışma taramasında hizalandı); yönetim tarafı **tek rol, çoklu kullanıcı** — yönetici hesapları arasında yetki farkı yoktur, rol/izin modeli buna göre kurulur.
> - **K-17 · K-531 · Terim sözlüğü:** Sözlük `02 §1`'de yaşar (Türkçe terim · İngilizce kod karşılığı · tanım). Bu doküman sözlüğü **birebir devralır, isim icat etmez**; bir kavramın tek adı vardır. **Tek istisna:** kapalı listelerin — Firma tipi, Hesap türü, Konu tipi, İptal sebebi, Düzeltme sebebi, Cayma istisnası sebebi, Ana sayfa düzeni, Teslim tarihi — değerlerinin kod adlarını bu doküman verir; değerler `02` gövdesindeki kapalı listeden birebir adlandırılır (K-531, K-617, `02 §1.2`; Hesap türü'nü K-617 ekledi, satır Aşama 3'ün çakışma taramasında hizalandı).

> **Aşama 2'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §8.3 AK0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **Dizin — Aşama 2 kararlarının bu dokümana devirleri:** karar kaydında K-647…K-718'in etki sütunlarında bu dokümanı gösteren yirmi üç atıf (yirmi üç karar) [`CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md) §6.1'dedir; etki sütununda yalnız doküman numarası taşıyan on altı atfın işinin adını dizin verir. Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Aşama 1'in dizini: `PHASE1_CONFLICT_SCAN.md` §6.

> **Aşama 3'ten park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2 ve §3, mekanizma: K-36 (yeri: §10.3 UI0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-787 · K-790 · K-793 · K-794 · Kalemde işlem görmüş adet:** sipariş kaleminin kayıtları — iptal, çıkarma, gecikme feshi, cayma beyanı, iade teslim alma, iade reddi, "mal dönmedi" kapanışı ve ayıp talebi — kapsadıkları adedi taşır; aynı türden kayıt aynı kalemde birden çok kez doğar (ardışık işlemler, parça parça ulaşan iade) ve kalemin açık adedi bu kayıtlardan okunur (Ürün Gereksinimleri `02 §5.8`, §7.1.6). Adedin kayıtta nasıl tutulduğu, açık adedin saklanıp saklanmadığı ve aynı adedi kullanmak isteyen eşzamanlı iki işlemin nasıl engellendiği bu aşamanın kararıdır.
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
> - **K-873 · K-874 · Sürüm alanları:** düzenlenebilir kayıtlar — ürün (varyant tablosunun değişikliği dahil), kupon, kurumsal içerik, ayarlar; bölümleri ayrı kaydedilen ekranda bölüm başına — ve sipariş bir sürüm değeri taşır; siparişin sürümü sipariş, kalemleri, kalem kayıtları ve siparişe bağlı müşteri girdileri — geri ödeme IBAN'ı dahil — değiştikçe artar (`05 §7`; `02 §3.31`). Sürüm değerinin alanı ve onu artıran yazımların listesi bu aşamanın kararıdır; K-800'ün Aşama 3 park satırı bu mekanizmayla karşılanır.
> - **K-875 · K-876 · K-877 · K-944 · İmha:** imha işi her kaydın süresini başlangıç değerinden hesaplar — oluştuğu yılın sonu, kapatılış, gönderim anı (`02 §4` Z-28…Z-44); silme fiziksel silmedir; siparişte kişisel alanlar boşaltılır ve hesap bağı kaldırılır (K-582) — imha siparişe bağlı kayıtları da kapsar: iç not, e-posta adresinin düzeltme geçmişi, ayıp talebi; bağlı kayıtların alan listesi bu aşamanın işidir (`05 §7.4.3`); imha kaydının ve geçiş kaydının "yalnız eklenen" oluşu uygulama düzeyindedir, veritabanı yetkisiyle korunan yalnız iz tablosudur (`05 §4.1.4`, §7.4.2); imha kaydı yalnız eklenen ayrı bir tabloda durur — tarih, kayıt türü, sayı; sunucunun kayıtlarının kendi döngüsüyle silinen günü her gece "döngüyle silindi" satırı olarak sayı yerine tarih aralığıyla girer (`05 §9.2.2`); işlem izi tablosunda uygulamanın veritabanı kullanıcısı yalnız ekler ve okur, süresi dolan satırı veritabanındaki bir imha fonksiyonu siler (`05 §7`). Siparişe ait üretilen belgelerin — Ön Bilgilendirme Formu, Mesafeli Satış Sözleşmesi — kopya olarak mı saklandığı, donmuş şablon sürümünden mi yeniden üretildiği bu aşamanın kararıdır; kopya saklanırsa imha kopyadaki kişisel veriyi de kapsar. Başlangıç değerlerinin alanları, imha kaydının ve iz tablosunun tanımı bu aşamanın kararıdır.
> - **K-883 · K-884 · K-948 · Dosya kaydı ve indirme:** her yükleme yeni bir dosya kaydıdır; varyantın o anki dosyası ona en son bağlanan kayıttır; hiçbir varyanta bağlı olmayan ve indirilebilir hiçbir kalemin istemediği kayıt imhada silinir; kalem indirme sayacını ve kısa ömürlü indirme bağlantısını taşır (`05 §7`; `02 §3.12`). Görsel — kaynak kopya ve türevleri — ürüne, içerik kaydına ve sipariş kalemine birer bağla bağlıdır; kalem görselin kendisini değil ona bağını dondurur ve ürünün silinmesinden etkilenmez (`02 §3.23.1`); hiçbir kayda bağlı olmayan görsel imhada, kaydedilmeden bırakılan yükleme geçici kayıtların temizliğinde silinir; önce kayıt silinir, kaydı olmayan dosyayı imha işi süpürür (`05 §7.7.1`, §7.7.2). Kayıtların ve bağların tanımı bu aşamanın kararıdır.
> - **K-886 · Ürün adının arama biçimi:** ürün adının küçük harfe çevrilmiş ve Türkçe harfleri sadeleşmiş arama biçimi ayrı bir alanda tutulur ve ad değiştikçe yenilenir; arama bu alanda içerir eşleşmesiyle, trigram dizini üzerinden yapılır (`05 §7`; `02 §3.5.1`). Alanın ve dizinin tanımı bu aşamanın kararıdır.
> - **K-889 · K-890 · Geçiş kaydı ve ayırma:** siparişin her durum geçişi yalnız eklenen bir geçiş kaydına yazılır — kim (müşteri, yönetici, sistem), ne zaman, hangi geçiş, sebep; kayıt `02 §3.34.8`'in "durum geçişleri"nin karşılığıdır ve işlem izinin yerine geçmez. Sipariş onayı stoğu, hizmet kontenjanını ve kupon hakkını koşullu güncellemeyle — kalan adet yetiyorsa — ayırır (Teknik Mimari `05 §4`, §7). Geçiş kaydının tanımı ve ayrılmış adedin alanları bu aşamanın kararıdır.
> - **K-891 · K-892 · K-893 · K-894 · K-940 · K-941 · K-946 · İş kuyruğu, gönderim kaydı, gelen bildirim ve geri ödeme kaydı:** dış yan etkiler veritabanındaki bir iş kuyruğuna geçişle aynı işlemde yazılır — satır kişisel veri taşımaz, kayda kimlikle işaret eder; iş satırı deneme sayısını, sonraki deneme anını ve sonucunu taşır — kuralı yazılmamış iş türünde beşinci yeniden denemeden sonra "başarısız" hâlindedir ve sıradaki satırları bekletmez; "başarısız" satır gecelik temizlikte silinmez, kurulumu yapanın yönetim komutuyla yeniden kuyruğa alınır ya da kapatılır (`05 §4.3.4`, §8.5.9); her e-posta gönderim kaydında bir satırdır ve e-postanın kuyruğu odur — e-posta iş kuyruğuna ayrıca satır yazmaz (`05 §4.3.1`); satır tür, alıcı, ilgili kayıt, olayın değerleri, deneme sayısı, sonraki deneme anı ve sonucu taşır; gövde saklanmaz (Z-34, Z-43); ödeme adımı formunun onay anahtarı sipariş kaydında tekildir; sağlayıcının imzası doğru bildirimi gelen bildirim kaydına yazılır — imzası tutmayan istek yazılmaz — ve sağlayıcının işlem kimliği tekildir; gelen bildirim kaydı kişisel alan taşımaz — sağlayıcının referansı, siparişin referansı, tutar, sonuç ve anlar —; tutarı tutmayan ya da siparişi kayıtta olmayan satır işaretlidir, siparişe bağlanan satır siparişle silinir, hiçbir siparişe bağlanmayan satır altı ay sonra imha işinde silinir (`05 §4.5.2`, §4.5.5); karta para gönderen her istekten önce geri ödeme kaydı "istek gönderiliyor" hâliyle yazılır; kaydın hâlleri istek gönderiliyor, sonucu bekleniyor, başarılı ve gerçekleşmedi'dir — sonucu bilinmeyen istek "sonucu bekleniyor"da kalır, sorgunun sonucuyla ya da firmanın "gerçekleşmedi" işaretiyle (işlem izine düşer) çıkar — firmanın işareti sorguyu durdurur, sonradan gelen başarılı sonuç siparişe işlenmez ve gelen bildirim kaydında işaretli kalır; panelin "geri ödeme sonucu bekleniyor" ve "geri ödeme gerçekleşmedi" işaretleri bu hâlle aynı işlemde yazılır (`05 §4.6.2`; K-940). Tabloların ve hâllerin tanımı bu aşamanın kararıdır.
> - **K-895 · K-898 · K-946 · Zaman alanları, nabız ve kesinti kaydı:** zamanlanmış işlerin zamanı kendi kaydında bir alandır — ödeme süresinin bittiği an, hatırlatmanın anı, IBAN'ın silineceği gün, e-postanın sonraki denemesi — ve onayda ya da süre yeniden başladığında yazılır; gecelik işin — imha ve geçici kayıtların temizliği — son başarılı çalışmasının ve son denemesinin anı veritabanında durur — başarısız biten iş en erken bir saat sonra yeniden denenir (`05 §5.1.1`, §9.4.17); zamanlayıcının nabzı ve iki dakikayı aşan boşlukların kesinti kaydı veritabanında durur; kesintide ertelenen havale iptalinin yeni son anı siparişte saklanır ve sipariş sayfasında görünür (`05 §5`; `02 §4.2` Z-8). Alanların tanımı bu aşamanın kararıdır.
> - **K-899 · K-901 · K-903 · K-906 · K-945 · Oturum, deneme, bağlantı ve tanınan tarayıcı kayıtları:** oturum sunucuda tutulur — anahtarın özeti, hesap, hesap türü, kısa ya da uzun, son işlem anı, yeniden doğrulamanın tek işlemlik izni; müşteri ve yönetici hesapları ayrı tablolardadır; sayılan her deneme bir deneme kaydıdır — limit, eksen, değer, an — ve penceresi geçince silinir; girişte birleşen sepet iki sayacın büyüğünü taşır — penceresinde satırı çok olan sepetin deneme satırları hesabın sepetinin kimliğine yazılır, ötekininki silinir (`05 §6.3.2`); giriş kaydı (`02 §3.13.20`) iki hesap türünün girişini taşır, veritabanındadır ve okuma komutuyla okunur (`05 §3.3.11`, §9.3.1); L-7 ve L-8 siparişlerden sayılır ve siparişte IP durur; hesap bağlantıları bağlantı kaydında yalnız özetle durur — tür, hesap, oluşma anı, ömrün sonu, kullanıldığı an, yerine yenisi geçtiği an; bağlantı kaydı hesabıyla yaşar ve hesapla silinir — gecelik temizlik onu silmez, ömrü dolmuş kayıt "ömrü dolmuş", "kullanılmış" ya da "yerine yenisi geçmiş" hâlini korur (`05 §6.4.4`, §6.4.6); sipariş erişim anahtarı siparişte durur, numara ve anahtar tekildir; tanınan tarayıcı işareti sunucuda hesap başına kaydıyla tutulur ve süresi dolan ya da geçersizleşen kayıt gecelik temizlikte silinir (`05 §5.2.9`); misafir sepeti son erişim anını taşır ve 400 gün açılmazsa silinir (`05 §6`). Kayıtların tanımı bu aşamanın kararıdır.
> - **K-913 · K-918 · K-937 · Kanal hâli ve yedeğin durum kaydı:** e-posta göndericisi tek satırlık bir kanal hâli kaydı tutar — son başarılı gönderimin anı, art arda kanal hatası sayısı, dizinin ilk hatasının anı — ve her gönderim denemesinin sonucu üç sınıftan birini taşır: gönderildi, alıcı hatası, kanal hatası; firma bildirimleri uyarısı gönderim kaydından okunur, ayrı bir işaret saklanmaz. Gecelik tam yedeğin sonucunu veritabanı konteynerindeki zamanlayıcı yerel soketle veritabanındaki bir durum kaydına yazar; dosya yedeğinin sonucu veritabanında değil, yedek aracının durum dosyasındadır — yedek aracı veritabanına bağlanmaz ve ağ üzerinden bağlanan bir yedek kullanıcısı yoktur (Teknik Mimari `05 §9.4.10`, §10; K-910). Kayıtların tanımı bu aşamanın kararıdır.
> - **K-951 · Elektronik ticaret kayıtlarının on yıllık saklaması (6563 sayılı Kanun m.11/3):** siparişe bağlı müşteri bildiriminin gönderim kaydı on yıl saklanır ve imha işi satırın siparişine bakar — ödemesi alınmış siparişin satırı on yıl alıcısıyla durur ve sonra silinir; ödemesi hiç alınmamış siparişin satırında üç yılda alıcı ve olayın değerleri boşaltılır, satır on yılda silinir. Ödemesi hiç alınmamış siparişin kişisel verisiz kaydı on yıl dolmadan silinmez. İletişim talebinin süresi konu tipinden okunur — "Sipariş hakkında" on yıl, öteki tipler iki yıl (`02 §4.2` Z-33, Z-41, Z-43, §11.2 P-40; Teknik Mimari `05 §4.4.4`, §7.4.3). Gönderim kaydının siparişe bağı, siparişin ödemesi alınmış sayıldığının (`02 §4.2` Z-28, §6.2.16) hangi alandan okunduğu ve boşaltılan alanların listesi bu aşamanın kararıdır.

---

## 1. Traceability Matrix (ÖNCE BU)

> **Ne yazılır:** `02` gereksinimleri + `03` akış adımları → entity/alan eşlemesi.
> Gereksinimlerdeki her *"X bilgisi saklanır"* ifadesi bir alana dönüşmelidir. Eşlenmeyen gereksinim = **eksik entity veya alan**.

### 1.1 İleri izlenebilirlik (gereksinim/akış → entity.alan)

| Kaynak ID | Kaynak özeti | Entity | Alan | Durum |
|---|---|---|---|---|

### 1.2 Geri izlenebilirlik (entity/alan → kaynak)

| Entity.alan | Beslendiği kaynak | Durum |
|---|---|---|

### 1.3 Boşluklar (GAP) ve kararlar

| # | Boşluk | Karar | Nereye yansıdı |
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

*Shopfolio — Data Model v0.1*
