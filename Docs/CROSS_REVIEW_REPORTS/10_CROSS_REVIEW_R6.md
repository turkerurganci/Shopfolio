# Cross-Review — 10 MVP Scope (Tur 6)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/10_MVP_SCOPE.md` v0.16 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde). İstem önceki turlarla aynıdır: daraltılmış talimat (K-615, K-644), yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1  
> Kriter: Teknik doğruluk  
> Seviye: Yüksek  
> Yer: KP-22  
> Alıntı: “teslimden sonraki caymada malın firmaya ulaştığı tarihten itibaren”  
> Sorun: Firma iade taşıyıcısı belirlemediği hâlde geri ödeme süresini malın firmaya ulaşmasına bağlar. Bu durumda tüketici herhangi bir taşıyıcıyla gönderim yapabilir; süre malın kargoya teslimiyle başlar. Kural, tüketicinin geri ödemesini taşıma süresi kadar hukuka aykırı biçimde geciktirir. [Ticaret Bakanlığı açıklaması](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)  
> Öneri: Firma iade taşıyıcısı belirtmiyorsa teslimden sonraki caymada geri ödeme süresini müşterinin malı taşıyıcıya teslim ettiği tarihten başlatın; “firmaya ulaşma” başlangıcını yalnız belirtilen taşıyıcı dışındaki iadeler için kullanın.
>
> BULGU-2  
> Kriter: Yasal uyum  
> Seviye: Yüksek  
> Yer: KD-28  
> Alıntı: “sistem ne satıldığını denetlemez.”  
> Sorun: Sistem, tüketiciye satışı internet üzerinden yasak olan tütün mamulleri ve alkollü içkilerin de genel fiziksel ürün akışıyla listelenip satılmasına izin verir. Bu, yalnızca firmaya bırakılabilecek bir uyum tercihi değildir; yasak satış sisteminin kurulması/faaliyeti yaptırım ve erişim engeli riski doğurur. [4733 sayılı Kanun m.8/5-k](https://www.tarimorman.gov.tr/TADAB/Belgeler/Kanunlar/kanun_4733.pdf)  
> Öneri: MVP’de en azından tütün mamulleri ve alkollü içkilerin ürün kaydı/yayını/satışı için açık ürün düzeyi engel koyun; diğer özel mevzuatlı ürünler için de kapsam dışı kullanım yasağını kurulum ve ürün kaydı seviyesinde bağlayıcılaştırın.
>
> BULGU-3  
> Kriter: Yasal uyum  
> Seviye: Yüksek  
> Yer: ÖK-10  
> Alıntı: “Ödeme sağlayıcısının … ve Google'ın aydınlatmada adıyla anılması firmanın yükümlülüğüdür, kapının koşulu değildir”  
> Sorun: Ödeme sağlayıcısı veya Google üzerinden kişisel veri aktarımı yapılmasına rağmen, bunların aydınlatma metninde doğru şekilde yer alması satış, hesap kaydı ve Google ile ilk giriş için zorunlu tutulmamıştır. Böylece eksik aydınlatmayla veri toplama ve aktarma mümkün olur. KVKK m.10 uyarınca aydınlatma, veri elde edilirken aktarım amacı ve alıcı gruplarını kapsamak zorundadır. [KVKK açıklaması](https://www.kvkk.gov.tr/Icerik/2033/Aydinlatma-Yukumlulugu-)  
> Öneri: Kullanılan ödeme sağlayıcısı ve Google girişinin ilgili aktarım bilgileri aydınlatmada doğru ve yayımlanmış olmadıkça ilgili veri toplama akışlarını açmayın.
> ```

"Kriter: Yasal uyum" yedi kriterin dışındadır; model güvenlik ya da teknik doğruluk yerine bu adı kullandı. Değerlendirmeyi etkilemez.

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ⚠️ KISMİ | **Konu 5. turda da geldi ama bu kez başka bir gerekçeyle.** 5. turda model süreyi bildirime bağlıyordu; bu, m.12'nin 2014 metniydi ve reddedildi. Bu turun iddiası farklı: firma hiç taşıyıcı belirtmediyse "öngörülenin dışında" bir taşıyıcı yoktur, süre kargoya teslimle başlar. **Bu okuma K-491'de zaten kalan risk olarak yazılı:** *"taşıyıcı hiç belirtilmediğinde sürenin ulaşmada başlaması metnin lafzından çıkan bir sonuçtur, açık hükmü yoktur — tüketici lehine bir yorum başlangıcı malın kargoya verildiği güne çekebilir"*. Ticaret Bakanlığı'nın tüketici rehberi de bu riski gösteriyor: kuralı *"Bu süre, tüketicinin malı kargoya teslim ettiği tarihten itibaren başlar"* diye anlatıyor, istisnayı *"ön bilgilendirmede belirtilenden farklı bir kargo ile yapılmışsa"* diye. Taşıyıcının hiç belirtilmediği hâli süre bakımından ayrıca ele almıyor; o hâl için yalnız masrafın tüketiciden istenemeyeceğini söylüyor (m.12/5 ile aynı). Yönetmelik de süreyi bu hâl için ayrıca düzenlemiyor (m.12/1). **Kuralın kendisi değişmez:** K-491'de proje sahibi "her caymada beyandan say" ve "firma iade taşıyıcısı belirler" seçeneklerini bu risk ortadayken eledi; önerinin istediği başlangıç da sistemin kargo entegrasyonu olmadan öğrenemeyeceği bir tarihtir (K-132). **Ama bulgunun çıkış sebebi `10`'un metnindeydi.** 5. turda KP-22'nin gerekçesine yazılan cümle sonucu kesin bir kural gibi sunuyordu, K-491'in kalan riskini taşımıyordu. `02 §7.4.1` bu riski yazıyor. Gerekçe bu yüzden kararın dediğini söylemiyordu. | KP-22 Gerekçe: *"… m.12/1'in 1/1/2026'dan beri uygulanan kuralından çıkar: firma iade taşıyıcısı belirlemediği için her iade öngörülenin dışında bir taşıyıcıyla yapılır. Kalan risk bilinçlidir: taşıyıcı hiç belirtilmediğinde bu sonuç metnin lafzındandır, açık hükmü yoktur — tüketici lehine bir yorum başlangıcı malın kargoya verildiği güne çekebilir; firma geri ödemeyi iade gönderisinin kargoya verildiği günden on dört gün içinde yaparsa iki okumada da süreyi kaçırmaz (K-491)."* Son yarım cümle yeni bir kural değildir; karşı ödemeli gönderinin kargo kaydında görünen tarihle firmanın riskten nasıl çıkacağını söyler. Sistem davranışı ve sayaç değişmedi. Öneri uygulanmadı. |
| BULGU-2 | ⚠️ KISMİ | **Önerinin kendisi bilinçli olarak elenmiş bir seçenektir.** K-84 engelleme mekanizmasını ve yasaklı kategori listesini açıkça eledi; KD-28 bunu yazıyor. **Hukuki iddianın yükümlüsü ürün değil, satıcıdır.** 4733 sayılı Kanun m.8/5-k cezayı tütün ve alkolün tüketiciye internet üzerinden satışını *"yapmak üzere satış sistemi kuran veya faaliyette bulunanlara"* verir. Shopfolio'da satıcı firmadır ve ürün ticari zincirde yer almaz (K-12). Ürün tütün ya da alkol satmak üzere kurulmuş bir sistem değildir. Bu ürünü satan firma, kendi kurduğu her e-ticaret yazılımında olduğu gibi yasağın muhatabıdır. Satıcı ürün düzeyinde bir engelle bu yükümlülükten kurtulmaz. Engel de yasağa uyumu sağlamaz, çünkü kategori serbest metindir ve aşılabilir (K-84'ün "hiçbir davranış ona dallanmaz" gerekçesi). **Ama satırda olgusal bir eksik vardı.** KD-28 alkol ve tütünü "ek akış isteyen ürünler" arasında sayıyor ve eksiği yaş doğrulaması, izin akışı olarak gösteriyordu. Satır böylece bu ürünlerin bir akışla ileride satılabileceği izlenimini veriyordu. Oysa tüketiciye internetten satışları hiçbir akışla açılmaz, Kanun bunu yasaklar. | KD-28 Gerekçe: *"Her ek akış ürün türüne özgü bir mevzuat ister; alkol ve tütünün tüketiciye internetten satışı ise hiçbir akışla açılmaz, yasaktır (4733 sayılı Kanun m.8/5-k). Satılan ürünün mevzuata uygunluğu satıcı olan firmanın yükümlülüğüdür; ürün ticari zincirde yer almaz ve satışı engellemez (K-12, K-84, K-440, K-641)."* Satır metni (K-641) ve "Açık" cevabı değişmedi. Satış engeli önerisi uygulanmadı. |
| BULGU-3 | ⚠️ KISMİ | **Önerinin kendisi bilinçli olarak elenmiş bir seçenektir.** K-576 *"Alıcı grupları adıyla dolmadan satış açılmasın"* seçeneğini eledi. Gerekçesi, KVKK m.10/1-c ile Aydınlatma Tebliği'nin alıcıyı kategori olarak istemesidir. Kapı adlara bağlansaydı, sistem denetleyemediği bir içeriği koşul yapmış olurdu. **Hukuki iddia yerinde değil:** Tebliğ m.3/1-a alıcı grubunu *"Veri sorumlusu tarafından kişisel verilerin aktarıldığı gerçek veya tüzel kişi kategorisi"* diye tanımlıyor. m.5/1-ı *"kişisel verilerin aktarılma amacı ve aktarılacak alıcı grupları belirtilmelidir"* diyor. Taslağın alıcı grupları bölümü ödeme kuruluşunu kategori olarak zaten anıyor ve aktarılan veriyi yazıyor (`02 §3.33.2`, K-553). Kapı eksik aydınlatmayla açılmaz; açıldığında yalnız sağlayıcının adı eksik olabilir. Google girişi bir aktarım değil, bir toplama yoludur. **Ama okumanın sebebi `10`'un metnindeydi.** ÖK-10 *"Ödeme sağlayıcısının — alıcı grubu olarak … — aydınlatmada adıyla anılması … kapının koşulu değildir"* diyordu. Cümle, alıcı grubunun kendisinin de metinde olmayabileceği gibi okunuyordu. Kategorinin taslakta yazılı geldiğini söylemiyordu. | ÖK-10 Özellik: *"Alıcı grupları taslakta kategori olarak zaten yazılıdır — ödeme kuruluşu dahil, alıcının kimlik ve iletişim verisinin ona aktarıldığıyla; KVKK m.10/1-c'nin istediği budur (Aydınlatma Tebliği m.5/1-ı). Ödeme sağlayıcısının ve Google'ın aydınlatmada adıyla anılması firmanın yükümlülüğüdür, kapının koşulu değildir; taslakta yer tutucuyla gelir."* Kaynak sütununa K-553 eklendi. Öneri uygulanmadı. |

**Dağılım:** 0 KABUL · 3 KISMİ · 0 RET. Üç bulgu da aynı biçimde: öneri kayıtlı bir kararla elenmiş, ama satır o kararın bir parçasını (kalan riski, olgusal dayanağı, taslağın içeriğini) taşımıyordu. Kural değişmedi, satırlar kararın dediğini söyler oldu.

## 3. Ek bulgular

- **Mekanik tarama:** düzeltmelerde anılan karar numaraları ilgili satırların Kaynak sütununda var. K-491 KP-22'de zaten vardı; K-12 KD-28'in gerekçesine, K-553 ÖK-10'un Kaynak sütununa bu turda eklendi. Sürüm başlığı ve alt bilgi aynı sürümü gösteriyor (v0.17). Kapsam satırı sayısı değişmedi (77), kapsam dışı satırı sayısı değişmedi (63).
- **`02` ile hizalı:** `02 §7.4.1` K-491'in kalan riskini, §3.33.2 taslağın alıcı grupları bölümünü, §1.2'nin kapsam notu ve §12.1.12 K-84'ü zaten yazıyor. `02`'de çelişki doğmadı. `02` 4733 sayılı Kanun'u anmıyor; bu bir çelişki değil, etki yansıtmada §12.1.12'ye aynı yarım cümlenin eklenmesi değerlendirilebilir.
- **Tekrarlayan konular:** KP-22'nin geri ödeme başlangıcı iki tur üst üste geldi (5. ve 6. tur), ama farklı gerekçelerle. 5. turun gerekçesi eski metindi ve reddedildi. 6. turun gerekçesi K-491'in kayıtlı kalan riskidir. Satır artık bu riski açıkça yazıyor. Konu yeni bir dayanak olmadan yeniden gelirse doğrudan RET olarak kalır.
- **Ciddi bir sorun görülmedi.** Bu turda ikinci modelin listesinin dışında ciddi bir sorun saptanmadı.
- **Etki yansıtmaya devredilen:** `02 §12.1.12`'ye alkol ve tütünün internetten satış yasağının (4733 m.8/5-k) dayanağı olarak eklenmesi (isteğe bağlı, çelişki değil). 1. ve 2. turdan devreden `02` notları yerinde duruyor: "İşlem izi" sözlük satırı, §8.5.2 ve §3.34.8.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`10` v0.17). Bu turda yeni karar satırı açılmadı. K-491 proje sahibinin kararıdır ve kalan riski kayıtta yazılıdır. Bu tur o kararı değiştirmedi, yalnız riski `10`'a taşıdı.

- [x] BULGU-1 (kısmi: KP-22'nin gerekçesi K-491'in kalan riskini ve firmanın iki okumada da güvende kalma yolunu yazar. Süreyi kargoya teslimden başlatma önerisi K-491 gereği uygulanmadı)
- [x] BULGU-2 (kısmi: KD-28'in gerekçesi alkol ve tütünün tüketiciye internetten satışının yasak olduğunu ve yükümlülüğün satıcı firmada olduğunu yazar. Ürün düzeyinde satış engeli önerisi K-84 gereği uygulanmadı)
- [x] BULGU-3 (kısmi: ÖK-10 alıcı gruplarının taslakta kategori olarak yazılı geldiğini söyler. Adları kapı koşulu yapma önerisi K-576 gereği uygulanmadı)

**Hukuki kontrol** (2026-10-03):
- **Mesafeli Sözleşmeler Yönetmeliği m.12** (konsolide metin, MevzuatNo 20237; 5. turda okundu, bu turda yeniden tarandı). m.12/1 *"iade için ön bilgilendirmede belirtilen taşıyıcıya teslim"* ile *"iade için öngörülenin haricinde bir taşıyıcı ile iade"* hâllerini düzenler. m.12/5 (RG 24/5/2025-32909 ile değişik) *"Satıcının ön bilgilendirmede iade için herhangi bir taşıyıcıyı belirtmediği durumda"* yalnız masrafı düzenler, sürenin başlangıcını düzenlemez. Ticaret Bakanlığı tüketici rehberi kuralı kargoya teslimle, istisnayı "belirtilenden farklı bir kargo" ile anlatır. Taşıyıcının hiç belirtilmediği hâlin süresine ayrı bir cümle ayırmaz; yalnız masrafın tüketiciden istenemeyeceğini söyler. Sonuç: K-491'in "açık hükmü yoktur" tespiti doğru, kalan risk gerçek.
- **4733 sayılı Kanun m.8/5-k** (mevzuat.gov.tr konsolide metin): *"Tütün mamulleri veya alkollü içkilerin, etil alkol, metanol, makaron, sarmalık kıyılmış tütünün ve yaprak sigara kâğıdının tüketicilere satışını (…); internet, televizyon, faks ve telefon gibi elektronik ticaret araçları ya da posta ile sipariş yöntemi kullanarak yapmak üzere satış sistemi kuran veya faaliyette bulunanlara … idarî para cezası verilir."* İnternet satışında 5651 sayılı Kanun'a göre erişim engeli de öngörülür.
- **Aydınlatma Tebliği** (RG 10/3/2018-30356): m.3/1-a *"Alıcı grubu: Veri sorumlusu tarafından kişisel verilerin aktarıldığı gerçek veya tüzel kişi kategorisini"*; m.5/1-ı *"Aydınlatma yükümlülüğü kapsamında, kişisel verilerin aktarılma amacı ve aktarılacak alıcı grupları belirtilmelidir."*

**Kaynaklar (hukuki kontrol):**
- [Mesafeli Sözleşmeler Yönetmeliği — mevzuat.gov.tr konsolide metin (PDF)](https://www.mevzuat.gov.tr/MevzuatMetin/yonetmelik/7.5.20237.pdf)
- [Ticaret Bakanlığı — Mesafeli sözleşmeler hakkında bilgilendirme](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)
- [4733 sayılı Kanun — mevzuat.gov.tr konsolide metin (PDF)](https://www.mevzuat.gov.tr/MevzuatMetin/1.5.4733.pdf)
- [Aydınlatma Yükümlülüğünün Yerine Getirilmesinde Uyulacak Usul ve Esaslar Hakkında Tebliğ — Resmî Gazete 10/3/2018, 30356](https://www.resmigazete.gov.tr/eskiler/2018/03/20180310-5.htm)
