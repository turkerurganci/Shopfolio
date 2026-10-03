# Cross-Review — 02 Product Requirements (Tur 17)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/02_PRODUCT_REQUIREMENTS.md` v0.32 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde; satır sonlarındaki boşluklar atıldı). İstem önceki turlarınkiyle aynıdır: yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §3.1.5 Satış kapısı; §3.1.6 Geçici satış kapatma
> Alıntı: "Koşullardan biri sağlanmıyorsa sepete ekleme ve ödeme kapalıdır"
> Sorun: Geçici satış kapatma da dört satış koşulundan biridir; bu kural sepete eklemeyi kapatır. Ancak §3.1.6, geçici kapatmada “yalnız ödeme adımına geçilemez” der. Geçici kapatma sırasında müşterinin sepete yeni ürün ekleyip ekleyemeyeceği belirsiz ve çelişkilidir.
> Öneri: Geçici satış kapatmayı diğer kapı eksiklerinden ayırın: Sepete ekleme açık mı kapalı mı açıkça yazın; §3.1.5 ve §6.2.17’yi aynı davranışa hizalayın.
>
> BULGU-2
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §4.1.5 Sayım kuralları
> Alıntı: "kargoya verme süresinin üst çiti vardır — kargonun yolda geçen günleriyle birlikte yasal 30 günü aşmasın diye"
> Sorun: Kargoya verme süresi ödeme onayında başlar; yasal 30 günlük teslim süresi ise sipariş onayında başlar. Havale ödeme bekleme süresi ve kargonun gerçek taşıma süresi bu çitin dışında kalır; ayrıca taşıma süresi ürün tarafından sınırlandırılmamıştır. Bu nedenle 10 iş günlük çit, 30 günlük yasal teslim sınırını fiilen güvenceye alamaz.
> Öneri: Çitin gerekçesini kaldırın veya teslim taahhüdünü sipariş onayından itibaren, ödeme bekleme ve taşıyıcı süresini de kapsayan ölçülebilir bir üst sınırla tanımlayın. Yasal son tarihe yaklaşan siparişler için panelde zorunlu görünür uyarı kuralı ekleyin.
>
> BULGU-3
> Kriter: Güvenlik
> Seviye: Orta
> Yer: §8.2 Deneme limitleri; §8.2.1 Limit davranışı
> Alıntı: "Aynı IP ve aynı e-posta adresi — ayrı ayrı"
> Sorun: L-1’de yalnız bir e-posta adresine yönelik beş başarısız giriş, saldırgan farklı IP’ler kullansa bile gerçek kullanıcının girişini geçici olarak engeller. Bu, dokümanın “e-postayı bilen birinin başkasının hesabını kilitlemesine araç olurdu” gerekçesiyle reddettiği hesap kilidinin fiilî karşılığıdır.
> Öneri: Giriş limitini IP+e-posta çifti üzerinden uygulayın veya e-posta eksenindeki eşiği girişin tamamını engellemek yerine ek risk kontrolüne yönlendirin. IP tabanlı limiti koruyun.
>
> BULGU-4
> Kriter: Güvenlik
> Seviye: Orta
> Yer: §9.2 B-15; §10.4.11 Misafir siparişi e-posta düzeltmesi
> Alıntı: "Siparişin e-posta adresinin değiştirildiği, sipariş numarası"
> Sorun: Müşterinin ilk siparişte yanlış yazdığı, üçüncü kişiye ait e-posta adresine B-15 gönderilerek sipariş numarası ve sipariş ilişkisi açıklanır. Eski adresin gerçek müşteri değil, yalnızca yazım hatasıyla girilmiş bir üçüncü kişi olması mümkündür.
> Öneri: B-15’ten sipariş numarasını ve siparişe işaret eden ayrıntıları çıkarın; gerekiyorsa yalnız kimliklendirmeyen, genel bir güvenlik bildirimi gönderin. Bildirimin gönderimi için eski adresin doğrulanmış olmasını ayrıca değerlendirin.
>
> BULGU-5
> Kriter: Tutarlılık
> Seviye: Düşük
> Yer: §2 Temel akış; §3.2.1 Satış modeli
> Alıntı: "yayındaki her ürünün fiyatı vardır ve satın alınabilir"
> Sorun: Bu ifade, bütün varyantları tükenmiş ürünlerin yayında kaldığı ancak sepete eklenemediği §3.6.4 ile çelişir. “Yayında” olma, her durumda satın alınabilirlik anlamına gelmez.
> Öneri: İfadeyi “yayındaki her ürünün fiyatı vardır; stok veya kontenjan uygunsa satın alınabilir” şeklinde düzeltin.
>
> SONUÇ: 5 BULGU```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ✅ KABUL | Doğru ve küçük. §3.1.5 dört koşuldan biri sağlanmazsa *"sepete ekleme ve ödeme kapalıdır"* der; geçici satış kapatma bu koşullardan biridir. §3.1.6 ise *"Müşterinin sepeti korunur ve içeriği görünür; yalnız ödeme adımına geçilemez"* diyordu. Buradaki "yalnız", var olan sepetin durumunu anlatıyordu ama tek başına okununca sepete eklemenin açık kaldığı anlamı çıkıyordu. Karar kaydında cevap tektir: K-235 *"ürünler görünür ama sepete ekleme ve ödeme kapanır"* der. §6.2.17 de aynı davranışı yazar. | Yeni karar satırı açılmadı. §3.1.6: kapının öteki koşullarındaki davranış işler; sepete yeni ürün eklenemez ve ödeme adımına geçilemez; var olan sepet korunur ve içeriği görünür. |
| BULGU-2 | ⚠️ KISMİ | Gerekçe cümlesi sonucu abartıyordu. §4.1.5 çit için *"kargonun yolda geçen günleriyle birlikte yasal 30 günü aşmasın diye"* diyordu; çit bunu güvence altına alamaz, çünkü taşıma süresi ürünün dışındadır. Ama doküman çitin otuz günü garanti ettiğini başka bir yerde iddia etmiyor. K-448 çiti otuz günden geriye hesaplamış: otuz gün sipariş onayından, kargoya verme süresi ödeme onayından işler; üç iş günü havale, on iş günü kargoya verme ve birkaç günlük taşıma, araya bir bayram girse bile otuz takvim gününe sığar. Aşım durumu da tanımlı: otuzuncu günden sonra müşteri sipariş kargoya verilmiş olsa bile gecikme feshi yapar ve panel kanuni faiz uyarısını gösterir (§7.2.3, Z-11, §6.4.2; K-499). Yönetmelik m.16/1'in otuz günü siparişin satıcıya ulaştığı andan saydığı K-549'da resmî metinden okunmuştur. **Önerinin iki parçası alınmadı.** (1) *"Ödeme bekleme ve taşıyıcı süresini de kapsayan ölçülebilir üst sınır"*: ürün taşıyıcının süresini ne ölçer ne sınırlar; çit firmanın kendi sözüne konabilecek tek sınırdır. (2) *"Yasal son tarihe yaklaşan siparişler için panel uyarısı"*: kargoya vermeden önce firmanın kendi sözü (en çok on iş günü) otuz günden çok önce dolar ve sipariş panelin "kargoya verilecek" sayacındadır (§10.6.1). Kargoya verdikten sonra firmanın paketi hızlandıracak bir yolu yoktur. Uyarı firmaya yapacağı bir iş göstermez. | Yeni karar satırı açılmadı. §4.1.5'in gerekçesi düzeltildi: çit otuz günden geriye hesaplanmıştır (K-448); otuz günü güvence altına almaz, firmanın sözünü otuz güne yer bırakan bir sınırda tutar; taşıma süresi ürünün dışındadır ve aşımda gecikme feshi açılır (§7.2.3, Z-11). |
| BULGU-3 | ⚠️ KISMİ | Sorun gerçek ve dokümanın kendi gerekçesiyle çelişiyordu. K-329 hesap kilidini *"hedefin e-posta adresini bilen biri bilerek yanlış şifre deneyip onun hesabını kilitleyebilir"* diye eledi. §8.2.1 de bunu tekrar eder. Ama K-330'un e-posta ekseni (L-1, 5 / 15 dakika, kayan pencere) aynı aracı geri getiriyordu: on beş dakikada beş yanlış şifre gerçek kullanıcının şifreyle girişini süresiz kapalı tutar. Bunu yapmak için dağıtık IP bile gerekmez, çünkü tek bir IP'nin eşiği 20'dir. Yöneticide sonuç, firmanın kendi panelinden süresiz dışarıda kalmasıdır. K-333 yöneticiye muafiyeti eleyerek bunu zaten görmüştü: *"kendi panelinden geçici olarak engellenen firma sahibinin bekleyecek zamanı olmayabilir."* Dokümanda bu kalan risk yazılı değildi. L-5'te ise yazılıdır (K-602). **Önerinin iki parçası alınmadı.** (1) *"IP + e-posta çifti ekseni"*: her çifti eşiğin altında deneyen dağıtık saldırıyı saymaz. Bu, K-330'un *"yalnız IP"* seçeneğini eleyen gerekçeyle aynıdır: *"farklı IP'lerden tek bir hesaba yönelen dağıtık deneme hiç görünmez."* (2) *"Ek risk kontrolü"*: ürünün elindeki tek ek kontrol captcha olurdu, o da K-304 ile elendi; üçüncü taraf captcha her ziyaretçinin verisini üçüncü tarafa gönderir (§8.3.3, §12.2.8). **Seçilen yol, OWASP'ın hesap kilidi yerine önerdiği "cihaz çerezi" kalıbıdır.** Başarılı girişle tarayıcıya hesap için bir tanınma işareti bırakılır. E-posta ekseni yalnız işaretsiz tarayıcıları sayar ve yalnız onları engeller. Dağıtık deneme işaretsiz tarayıcılardan geldiği için görünmeye devam eder. | K-603 `(öneriyle kaydedildi — ⚠)`. Yeni §8.2.6 kuralları yazar. **İşaret:** başarılı girişle ya da sıfırlama bağlantısıyla şifre yenilenince doğar; zorunlu, birinci taraf bir çerezdir; ömrü son başarılı girişten 90 gündür (P-48, Z-46). Şifre sıfırlandığında, "bu değişikliği ben yapmadım" bağlantısı kullanıldığında ve hesap silindiğinde geçersizleşir. **Sayım:** işaretli tarayıcının denemeleri kendi sayacında aynı eşikle sayılır; IP ekseni herkese işler. **Google ile giriş:** L-1'e girmez. **Kalan risk:** saldırı sürerken yeni bir tarayıcıdaki gerçek kullanıcı şifresini sıfırlayarak girer; saldırganın L-2'yi doldururken gönderdiği bağlantılar da hedefin kutusuna düşer. Güncellenen yerler: §3.13.17, §4.2 Z-46 (yeni), §6.5.11, §8.2 (L-1 satırı, §8.2.1, yeni §8.2.6), §11 P-48 (yeni), §12.2.5 (çerez envanterine beşinci kullanım). |
| BULGU-4 | ❌ RET | Senaryo B-15'e yeni bir sızıntı yüklemiyor. Eski adres yanlış yazılmış bir yabancınınsa o kişiye siparişin onayında B-1 zaten gitmiştir. B-1 sipariş numarasını, sipariş sayfasının bağlantısını ve sözleşme metnini gövdede tam olarak taşır (§9.2 B-1). K-585 bunu yazar: *"yanlış adres bir yabancınınsa sipariş e-postası ve bağlantı ona gitmiştir"*. Düzeltmenin eski anahtarı geçersizleştirmesi de tam bu yüzdendir (§10.4.11 (2)). B-15 bu kişiye yeni bir şey söylemez: yeni adresi ve bağlantıyı taşımaz. Numara ise öteki senaryoda gereklidir. Firma kandırılıp gerçek sahibin adresi değiştirildiyse, hangi siparişin değiştiğini söyleyen tek bilgi numaradır; B-15'in varlık sebebi de bu uyarıdır (K-585, §6.3.5). *"Gönderimi eski adresin doğrulanmasına bağla"* önerisi B-15'i hiç göndermemek demektir, çünkü misafirin adresi hiçbir zaman doğrulanmaz (K-193). Bulgu dokümanın bir eksiğinden doğdu: numaranın neden taşındığı yazılı değildi. | Karar değişmedi (K-585). §10.4.11 (4)'e numaranın neden taşındığı ve gönderimin neden doğrulamaya bağlanamayacağı yazıldı. |
| BULGU-5 | ✅ KABUL | Doğru ve küçük. §2 ve §3.2.1'deki *"yayındaki her ürünün fiyatı vardır ve satın alınabilir"* cümlesinin amacı satış modelini anlatmaktı: teklif ya da "fiyat sorunuz" hattı yoktur (K-11). Ama cümle olgu olarak yanlıştı. Tükenmiş ürün yayında kalır ve sepete eklenemez (§3.6.4); satış kapısı kapalıyken de hiçbir ürün satın alınamaz (§3.1.5). | Yeni karar satırı açılmadı. §2 ve §3.2.1: ürün sepetten doğrudan satın alınır; stok, hizmet kontenjanı ve satış kapısı izin verdiği sürece. §3.2.1'e tükenmiş ürünün davranışı da bağlandı. |

**Dağılım:** 2 KABUL · 2 KISMİ · 1 RET.

- **BULGU-1…5:** Hiçbiri daha önce gelmemişti. BULGU-3, 16. turun BULGU-1'inden (L-5'in e-posta ekseni) farklıdır. O tur ekseni eklemişti; bu tur var olan bir eksenin yan etkisini bulmuştur.

## 3. Ek bulgular

- **Çerez politikası taslağı:** §12.5'in taslak metinleri arasında çerez politikası var ve beşinci çerezi (tanınan tarayıcı işareti) anmalı. Bu `04`/`12`'nin işi; etki yansıtmada not edilmeli.
- **Fesihten sonra kargonun malı yine de teslim etmesi (11. turdan açık, işlenmedi):** Bu turda da gelmedi. Etki yansıtmada ya da sonraki turda bakılmalı.
- **Şifre değişikliği (sıfırlama değil, 14. turdan açık):** §3.13.10 yalnız sıfırlamada oturumları kapatıyor. K-603 de işaretleri yalnız sıfırlamada geçersizleştiriyor. Konu açık kalıyor; etki yansıtmada bakılabilir.
- **Etki yansıtma için not:** `05` iki sayacı ve işaretin mekanizmasını taşımalı. `04` çerez politikası taslağına beşinci çerezi eklemeli. `01` ve `10`'da L-1'in eksenine dair bir cümle yok. Bu turda `01` ve `10`'a dokunulmadı.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`02` v0.33). Bir karar satırı açıldı:

- K-603 `(öneriyle kaydedildi — ⚠)`: K-329'un, K-330'un ve K-348'in davranışını genişletiyor; hesaba bağlı bir çerezle kişisel veriye dokunuyor.

- [x] BULGU-1 · [x] BULGU-2 · [x] BULGU-3 · [x] BULGU-4 (ret; açıklama eklendi) · [x] BULGU-5

**Hukuki kontrol** (2026-10-03):
- **BULGU-2:** Mesafeli Sözleşmeler Yönetmeliği m.16/1'in otuz günü ve başlangıcı K-549'da resmî konsolide metinden okunmuştu. Bu turda metne yeni bir hukuki iddia eklenmedi.
- **BULGU-3:** Yeni çerezin rıza gerektirmediği iddiası KVKK'nın Çerez Uygulamaları Hakkında Rehberi'ne (2022) dayanıyor. Rehber, kimlik doğrulama ve kullanıcı odaklı güvenlik çerezlerinin açık rıza gerektirmeyebileceğini söyler. Ölçüt, çerezin kullanıcının açıkça talep ettiği hizmet için kesinlikle gerekli olmasıdır (rehberin AB'den aldığı iki ölçütten B). Giriş, kullanıcının talep ettiği hizmettir. İşaret yalnız o hizmetin güvenliği için kullanılır.
- **BULGU-1, BULGU-4 ve BULGU-5:** Hukuki iddia içermiyor.
