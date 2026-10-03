# Cross-Review — 02 Product Requirements (Tur 19)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/02_PRODUCT_REQUIREMENTS.md` v0.34 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde; satır sonlarındaki boşluklar atıldı). İstem önceki turlarınkiyle aynıdır: yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §7.2.3 Gecikme feshi; §12.1.8 Teslim
> Alıntı: "tahsil edilen tüm ödemeleri fesih bildiriminden itibaren on dört gün içinde"
> Sorun: §12.1.8 gecikme feshinde tüm tahsil edilen ödemelerin iadesini yasal sonuç olarak yazarken, §7.2.3 geri ödemeyi yalnız gecikmenin dokunduğu fiziksel kalemlerin bedeli ve kargo ücretiyle sınırlar. Bu, özellikle karışık siparişte tamamlanmış dijital veya hizmet kalemlerinin bedeli bakımından hukuki sonuç ve ürün davranışını çelişkili bırakır.
> Öneri: Gecikme feshinin karışık siparişte sözleşmenin tamamına mı, yalnız geciken kalemlere mi uygulanacağını hukuki doğrulamayla tek kurala bağlayın; §7.2.3 ve §12.1.8’i aynı geri ödeme kapsamı ve tutarıyla güncelleyin.
>
> BULGU-2
> Kriter: Edge case
> Seviye: Yüksek
> Yer: §5.4 Sevkiyat ekseni, S5
> Alıntı: "Sipariş iptal edilmemiş ve gecikme feshine uğramamış fiziksel kalem taşır"
> Sorun: Kargoya verme geçişi, sipariş düzeyinde iptal edilmemeyi ve gecikme feshini kontrol ediyor; kalem bazında iptal edilmiş, yönetici tarafından çıkarılmış veya cayma beyanıyla kapanmış fiziksel kalemleri açıkça dışlamıyor. Bu, özellikle karışık siparişte kapatılmış fiziksel kalemin kargoya verilmesine yol açabilir. §5.8 ise cayma beyanıyla kapanan kalemin “kargoya verilmez” olduğunu söyler.
> Öneri: S5 koşulunu “en az bir iptal edilmemiş, çıkarılmamış, cayma beyanıyla kapanmamış ve gecikme feshine uğramamış fiziksel kalem” olarak değiştirin; bu kalem kalmadığında sevkiyat geçişini engelleyip kalan kalemlerin hattına yönlendirin.
>
> BULGU-3
> Kriter: Eksiklik
> Seviye: Orta
> Yer: §10.7.4 Sipariş dışındaki kişilere ulaşma listesi
> Alıntı: "Hiç sipariş vermemiş üyeler ... ad ve e-postayı CSV olarak indirir."
> Sorun: Üyelik kuralları hesap oluştururken ad alanı toplamıyor; §3.15.5’teki üye kaydı görünümü de ad alanını tanımlamıyor. Hiç sipariş vermemiş bir üyede dışa aktarılacak “ad”ın kaynağı belirsizdir.
> Öneri: Ya hesap kaydına ad alanını ve buna ilişkin aydınlatma/hukuki sebebi ekleyin ya da hiç sipariş vermemiş üyelerin dışa aktarımını e-posta ile sınırlayın.
>
> BULGU-4
> Kriter: Tutarlılık
> Seviye: Düşük
> Yer: §11 Sayısal parametreler
> Alıntı: "on firma ayarı ile otuz dört ürün sabiti, toplam kırk dört parametre vardır."
> Sorun: Aktif satırlar on firma ayarı ve otuz altı ürün sabiti içerir; P-47 ve P-48 de ürün sabiti olarak tabloda yer alır. Dolayısıyla aktif toplam 46’dır, 44 değildir.
> Öneri: Envanter özetini “10 firma ayarı, 36 ürün sabiti, toplam 46 parametre” olarak düzeltin veya sayım dışında bırakılacak satırları açıkça belirtin.
>
> SONUÇ: 4 BULGU```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ⚠️ KISMİ | **Çelişki gerçekti, ama karar değişmedi.** §12.1.8 Yönetmelik m.16/3'ü aktarıp *"tutarı sistem geri öder"* diyordu. Bu, sistemin tahsil edilen tüm ödemeleri geri ödediği gibi okunuyordu. §7.2.3 ise geri ödemeyi feshedilen kalemlerin bedeli ve kargo ücretiyle sınırlar. Bu sınır bilinçli bir karardır: K-571 *"Fesih gecikmenin dokunduğu kalemlerle sınırlıdır"* der ve *"Bütün sipariş feshedilir"* seçeneğini, teslim edilmiş dijital ürünün geri alınamaması ve gecikmenin ona dokunmamış olması gerekçesiyle eler. Kalan riski de yazar: m.16/3'ün *"tahsil edilen tüm ödemeleri"* ifadesi karışık siparişte tüketici lehine okunabilir. Bu risk §7.2.3'te yazılıydı, §12.1.8'de yoktu. Codex'in *"hukuki doğrulamayla tek kurala bağlayın"* önerisi K-571'de zaten yapılmış: m.16 mevzuat.gov.tr'deki konsolide metinden okunmuş ve karar kalan riskle birlikte kaydedilmişti. **Önerinin "geri ödeme kapsamını yeniden seçin" kolu alınmadı.** | Karar değişmedi (K-571). §12.1.8 iki cümleye ayrıldı. Birinci cümle Yönetmelik'in satıcıya yüklediği yükümlülüğü söyler. İkinci cümle ürünün ne yaptığını söyler: sistem feshedilen fiziksel kalemlerin bedelini ve kargo ücretini geri öder, teslim edilmiş dijital ve hizmet kalemleri ile tamamlanmamış hizmet kalemi feshin dışındadır, kanuni faiz firmanındır. K-571'in kalan riski aynı maddeye eklendi ve kaynak sütununa K-571 yazıldı. |
| BULGU-2 | ⚠️ KISMİ | **Boşluk gerçekti; önerinin bir kolu başka bir yoldan kapatıldı.** S5'in koşulu *"Sipariş iptal edilmemiş ve gecikme feshine uğramamış fiziksel kalem taşır"* diyordu. Çıkarılan kalem için kural §10.4.3'teydi (*"sevkiyat hattında çıkarılan kalem iptal edilmiş kalem gibi sayılır — S3, S4, S10 ve İptal edildi'ye geçiş"*), ama S5 bu listede yoktu. Bu yüzden fiziksel kalemleri çıkarılmış siparişin kargoya verilip verilemeyeceği metinden okunmuyordu. **Cayma beyanı kolu:** müşterinin sipariş sayfasında kargoya verilmemiş fiziksel kalemde cayma beyanı doğmaz, çünkü cayma düğmesi Kargoya verildi'de açılır (§7.3.7; K-340, K-452). Ama §10.4.10'un panel kaydı (K-510) kalemin durumuna bakmıyordu. Bu yüzden e-postayla gelen bir bildirim kargoya verilmemiş kaleme cayma kaydı olarak düşebiliyordu. Böyle bir kaydın stoğu, kargo ücreti ve siparişin kapanışı için kural yoktu. Yönetmelik m.9/2 tüketicinin malın teslimine kadar da cayabileceğini söyler, yani bildirim geçerlidir. K-452 bu aralıkta tüketicinin yolunu iptal yapmış ve iptalin caymanın sonucunu aynen verdiğini yazmıştı. **Codex'in önerisi S5'e "cayma beyanıyla kapanmamış" eklemekti. Bu alınmadı:** kargoya verilmemiş kalemde cayma kaydı hiç doğmayacak. Bildirim iptal olarak işlenecek. | **K-604 (öneriyle kaydedildi — ⚠).** S5 açık fiziksel kalemi tam sayıyor: iptal edilmemiş, çıkarılmamış ve gecikme feshine uğramamış kalem. Yalnız bu kalemler kargoya verilir. S5'te kargoya verilmemiş kalemde cayma kaydı doğmadığı da yazıldı. §5.4'ün yönetici müdahaleleri maddesinde ve §10.4.3'te çıkarmanın geçiş listesine S5 eklendi. §10.4.10'a göre kargoya verilmemiş fiziksel kalemde kayıt açılmaz. Firma bildirimi kalemi "müşteriyle anlaşıldı (müşteri talebi)" sebebiyle iptal ederek işler. Stok döner, geri ödeme iptalin hattından yürür, kargo ücreti §7.2.9'un tetiğiyle geri ödenir. Yasal on dört gün bildirimin ulaştığı tarihten işler (m.12/2). Bu yüzden firma iptali bildirimi aldığı gün yapar. §7.3.7'ye aynı kural kısaca yazıldı ve yeni 6.4.31 satırı eklendi. |
| BULGU-3 | ✅ KABUL | Doğru. Üç kural hesapta bir ad olduğunu varsayıyordu: §10.7.4'ün ulaşma listesi (K-514: üyeler için *"ad ve e-posta"*), §3.32.1'in ön doldurması (K-300: *"Üye girişliyken ad ve e-posta ön dolu gelir"*) ve §12.2.8'in *"ürün yalnız dönen ad ve e-postayı alır"* cümlesi. Ama §3.13 kayıt formunun ad istediğini yazmıyordu. §3.15.5'in üye kaydı görünümü (K-512) de adı saymıyordu. Karar kaydında hesabın adını tanımlayan bir satır yok. **Codex'in iki seçeneğinden ilki seçildi:** hesaba ad alanı eklendi. İkinci seçenek ("dışa aktarmayı e-postayla sınırlayın") daha az veri toplardı, ama K-300'ü ve K-514'ü geri alırdı. Hukuki sebep §12.2.3'te zaten yazılı: hesap verisi m.5/2-c'ye dayanır. **"Ad adres defterinden okunsun" seçeneği de elendi:** adresteki ad alıcınındır ve hesap sahibinden farklı olabilir (§3.14.6). | **K-605 (öneriyle kaydedildi — ⚠).** Yeni §3.13.21: kayıt formu ad, e-posta ve şifre ister ve ad zorunludur. Google ile açılan hesapta ad Google'dan gelir. Kullanıcı adını değiştirebilir ve bunun için yeniden doğrulama gerekmez. Hesabın adı siparişin alıcı adı değildir: sipariş alıcı adını adresinden alır ve dondurur. Hesap silinirken ad hemen silinir (§3.15.1). Üye kaydı görünümü (§3.15.5, §10.1.2) adı gösterir. §10.7.4 hangi adın dışa aktarıldığını söyler. |
| BULGU-4 | ✅ KABUL | Doğru. §11.1'de on firma ayarı var (P-2 ve P-11 kaldırıldı, P-44 girdi). §11.2'de otuz altı ürün sabiti var: 17. turda P-47 (K-599) ve P-48 (K-603) eklendi, ama özet cümle güncellenmedi. Doğru toplam kırk altıdır. | §11'in giriş paragrafında P-47 ve P-48 giren parametreler arasına eklendi. Sayım "on firma ayarı ile otuz altı ürün sabiti, toplam kırk altı parametre" oldu. `10` KP-64 yalnız "on firma ayarı"nı anıyor; o sayı değişmedi. |

**Dağılım:** 2 KABUL · 2 KISMİ · 0 RET.

- **BULGU-1:** 3. turun BULGU-5'inin (K-571) akrabasıdır. Karar o turda verilmişti; bu kez dönmesinin sebebi §12.1.8'in kararı yansıtmamasıydı. Kalan risk artık iki yerde de yazılı.
- **BULGU-2:** Kalem çıkarmanın S5'teki karşılığı ve kargoya verilmemiş kalemde başka kanaldan gelen cayma bildirimi daha önce gelmemişti. Codex'in istediği koşul S5'e eklenmedi; onun yerine böyle bir kaydın doğması engellendi (K-604).
- **BULGU-3 ve BULGU-4:** Daha önce gelmemişti. BULGU-4 17. turun eklediği satırların özet sayıma yansımamasından doğdu.
- **RET yok:** Dört bulgunun dördü de metinde gerçek bir boşluk ya da çelişki gösterdi. Her biri karar kaydına karşı kontrol edildi. BULGU-1'de önerinin "kapsamı yeniden seç" kolu, BULGU-2'de "S5'e cayma beyanını ekle" kolu reddedildi.

## 3. Ek bulgular

- **Ödenmemiş siparişte kargodan önce gelen cayma bildirimi:** K-604'e göre firma bu bildirimi iptalle işler. Ama ödeme onayından önce iptal yalnız sipariş bütünüyle yapılır (K-505). Havale bekleyen siparişte tek bir kalemden cayan müşteri için firma siparişi bütünüyle iptal eder ya da müşteriyle konuşur. Para tahsil edilmemiş olduğu için geri ödeme riski yoktur. Bu tura metin eklenmedi.
- **K-537'nin geçiş listesi:** Karar satırı çıkarmanın geçiş listesinde S11'i de sayıyor, `02 §10.4.3` saymıyor. Fiziksel kalem Kargoya verildi'den sonra çıkarılamadığı için S11'de çıkarılmış fiziksel kalem bulunmaz ve fark davranışı değiştirmez. Etki yansıtmada liste hizalanabilir.
- **Önceki turlardan açık kalanlar:** 18. turun ek bulguları: kuponlu çok oranlı siparişte kargo ücretinin hangi tutara orantılı bölündüğü (§3.19.2) ve K-180'in kendiliğinden iptalinde B-7'nin gidip gitmediği. Ayrıca fesihten sonra kargonun malı yine de teslim etmesi (11. tur) ve şifre değişikliğinde oturumlar ile tanınan tarayıcı işaretleri (14. ve 17. tur). Bu turda bunların hiçbiri gelmedi.
- **Etki yansıtma için not:** `10` KP-47'nin (K-510) satırı K-604'ün sınırını taşımalı: kargoya verilmemiş fiziksel kalemde cayma kaydı açılmaz. `04` kayıt formuna ad alanını ve hesapta adı değiştirmeyi eklemeli, `06` hesabın ad alanını tanımlamalı. `12`'nin aydınlatma taslağı hesap verisi olarak adı anmalı. `01`'de bu turun değiştirdiği cümlelerin karşılığı yok.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`02` v0.35). İki yeni karar satırı açıldı ve ikisi de ⚠ ile işaretli. Seçenekleri gerçekten ayrışan ve firmaya ya da müşteriye ciddi para veya hukuki risk yükleyen bir konu çıkmadı.

- [x] BULGU-1 (kısmi; karar değişmedi, §12.1.8 hizalandı) · [x] BULGU-2 (kısmi — K-604 ⚠) · [x] BULGU-3 (K-605 ⚠) · [x] BULGU-4
- ⚠ **K-604:** Kargoya verilmemiş fiziksel kalemde başka kanaldan gelen cayma bildirimi firmanın iptaliyle işlenir ve cayma kaydı açılmaz. Gözden geçirilecek nokta şu: panelin geri ödeme süresi iptalden sayılır, yasal süre ise bildirimden. Firma iptali geciktirirse aradaki fark firmanın riskidir.
- ⚠ **K-605:** Hesap bir ad taşır ve kayıt formu adı zorunlu ister. Gözden geçirilecek nokta şu: bu kişisel verinin kapsamını genişletir. Elenen seçenek, ulaşma listesini ve ön doldurmayı yalnız e-postayla yapmaktı; o seçenek K-300'ü ve K-514'ü geri alırdı.

**Hukuki kontrol** (2026-10-03, Mesafeli Sözleşmeler Yönetmeliği'nin mevzuat.gov.tr konsolide metni):
- **BULGU-1:** m.16/3: *"Sözleşmenin feshi durumunda, satıcı veya sağlayıcı, varsa teslimat masrafları da dâhil olmak üzere tahsil edilen tüm ödemeleri fesih bildiriminin kendisine ulaştığı tarihten itibaren on dört gün içinde ... kanuni faiziyle birlikte geri ödemek ... zorundadır."* §12.1.8'in yeni ilk cümlesi bu metni aktarır. Ürünün kalem düzeyindeki sınırı K-571'in kalan riski olarak yazılıdır.
- **BULGU-2:** m.9/2: *"Ancak tüketici, sözleşmenin kurulmasından malın teslimine kadar olan süre içinde de cayma hakkını kullanabilir."* m.12/2: malın tesliminden önce caymada satıcı, *"cayma hakkının kullanıldığına ilişkin bildirimin kendisine ulaştığı tarihten itibaren on dört gün içinde, varsa malın tüketiciye teslim masrafları da dahil olmak üzere tahsil edilen tüm ödemeleri"* iade eder. K-604 bildirimin geçerliliğini değiştirmez, yalnız kaydın yolunu seçer. Kargo ücretinin geri ödenmesi m.12/2'nin teslim masraflarıyla uyumludur: bütün fiziksel kalemler kargodan önce kapanırsa ücret geri ödenir.
- **BULGU-3:** KVKK m.5/2-c (sözleşmenin kurulması ve ifası). Hesap verisinin hukuki sebebi §12.2.3'te zaten yazılı; yeni bir hukuki iddia eklenmedi.
- **BULGU-4:** Hukuki iddia içermiyor.
