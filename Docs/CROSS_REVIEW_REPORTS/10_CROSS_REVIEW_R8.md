# Cross-Review — 10 MVP Scope (Tur 8)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/10_MVP_SCOPE.md` v0.18 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde). İstem önceki turlarla aynıdır: daraltılmış talimat (K-615, K-644), yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §2 / KP-22
> Alıntı: "firma iade taşıyıcısı belirlemediği için her iade öngörülenin dışında bir taşıyıcıyla yapılır."
> Sorun: Bu yorumla teslim sonrası caymada geri ödeme, mal firmaya ulaşana kadar bekletiliyor. İade taşıyıcısı ön bilgilendirmede hiç belirtilmemişse tüketicinin herhangi bir taşıyıcıyı kullanması, tüketicinin aleyhine “öngörülenin dışındaki taşıyıcı” sayılamaz. Geri ödeme süresinin malın taşıyıcıya verildiği tarihte başlaması gerekir. Mevcut kural, firmayı 14 günlük yasal süreyi aşma ve tüketicinin parasına geç ulaşma riskiyle bırakır.
> Öneri: Ön Bilgilendirme Formu’nda bir iade taşıyıcısı belirleyin; yalnız bunun dışındaki taşıyıcıda sürenin firmaya ulaşmada başlamasını uygulayın. Taşıyıcı belirtilmeyecekse geri ödeme süresini malın taşıyıcıya teslim tarihinden başlatın.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §4.1 / ÖK-5
> Alıntı: "Ürün satışı engellemez — kaydı göremez; yükümlülük ve sonucu firmadadır."
> Sorun: Aynı satır ETBİS kaydını satıştan önce karşılanması gereken ön koşul olarak tanımlarken, satışın kayıtsız da açılmasına izin veriyor. Kendi e-ticaret ortamında satış yapan firma için ETBİS kaydı zorunludur; bu boşluk, dokümanın ön koşuluyla çelişir ve mevzuata aykırı satışa izin verir.
> Öneri: ETBİS kaydının tamamlandığı firma tarafından beyan edilip kurulumda doğrulanmadan satışın açılmamasını satış kapısına ekleyin.
> ```

Model sonuca varmadan önce üç konuyu web aramasıyla kontrol etti (koşum kaydında görünür, çıktıya girmedi): Mesafeli Sözleşmeler Yönetmeliği m.12'nin 1/1/2026'dan beri uygulanan metni ve iade taşıyıcısı, dijital içerikte cayma istisnası, kısmen ifa edilmiş hizmetten cayma. Son ikisi için bulgu yazmadı.

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ❌ RET | **Konu üçüncü kez geliyor ve doküman doğru.** 5. turda (süreyi bildirime bağlama, eski metin) ve 6. turda (taşıyıcı belirtilmediğinde süre kargoya verilişten başlar) geldi. Bu turun iddiası 6. turunkiyle aynıdır ve yeni bir dayanak getirmiyor. **İddia kayıtlı kalan risktir:** K-491 *"taşıyıcı hiç belirtilmediğinde sürenin ulaşmada başlaması metnin lafzından çıkan bir sonuçtur, açık hükmü yoktur — tüketici lehine bir yorum başlangıcı malın kargoya verildiği güne çekebilir"* diyor; KP-22'nin gerekçesi bunu 6. turdan beri yazıyor. **"On dört günü aşma riski" de satırda karşılanmış:** gerekçe, firma geri ödemeyi iade gönderisinin kargoya verildiği günden on dört gün içinde yaparsa iki okumada da süreyi kaçırmadığını yazıyor; karşı ödemeli gönderinin kargo kaydı bu tarihi gösterir. Müşteri için bir para kaybı doğmuyor. **Önerinin iki yarısı da elenmiş seçeneklerdir.** "Ön bilgilendirmede iade taşıyıcısı belirleyin" seçeneğini K-491'de proje sahibi eledi: K-293'ü ve K-142'nin *"taşıyıcı belirleme mekanizması değildir"* çizgisini tersine çevirir, ayarlara taşıyıcı alanı ve takip numarası girişi ister, taşıyıcıya teslim tarihini sistem entegrasyonsuz öğrenemez (K-132). "Süreyi taşıyıcıya teslimden başlatın" seçeneğinin istediği tarihi de sistem kargo entegrasyonu olmadan bilemez. **Hukuki iddia:** m.12/1'in güncel metni yalnız iki hâli düzenliyor: belirtilen taşıyıcı ve öngörülenin haricinde bir taşıyıcı. Taşıyıcının hiç belirtilmediği hâli m.12/5 yalnız masraf bakımından düzenliyor, süre bakımından düzenlemiyor. Metin 1/1/2026'dan beri aynı (RG 24/5/2025-32909 ve RG 23/8/2022-31932). "Açık hükmü yoktur" tespiti doğru. **Dönmesinin sebebi:** model gerekçenin ilk cümlesini alıntıladı. Cümle sonucu *"kuralından çıkar"* diye kesin bir kural gibi sunuyordu, taşıyıcı belirlememeyi de bir olgu gibi anlatıyordu (*"belirlemediği için"*). Taşıyıcı belirlemenin bilinçli olarak elenen bir seçenek olduğunu söylemiyordu. Talimattaki "elenen seçenek" ayrımı bu yüzden satıra açıkça uymuyordu. Bu belirsizlik giderildi; kural değişmedi. | KP-22 Gerekçe: *"… m.12/1'in 1/1/2026'dan beri uygulanan kuralının lafzından çıkar: firma iade taşıyıcısı belirlemez, her iade öngörülenin dışında bir taşıyıcıyla yapılır; taşıyıcı belirlemek bilinçli olarak elendi (K-293, K-491). Kalan risk bilinçlidir: taşıyıcı hiç belirtilmediğinde bu sonucun açık hükmü yoktur — …"* "Metnin lafzındandır" ifadesi ilk cümleye taşındı; gerekçe neredeyse uzamadı. K-293 ve K-491 Kaynak sütununda zaten vardı. Öneri uygulanmadı. |
| BULGU-2 | ⚠️ KISMİ | **Çelişki yok.** §4.1'in giriş paragrafı listeyi iki gruba ayırıyor: panelin dışında karşılanan **dış ön koşullar** ve panelde tamamlanan **satış kapısının koşulları** (`02 §3.1.5`). ÖK-5 birinci gruptadır. Dış ön koşul, ürünün sınamadığı ama her kurulumda karşılanması gereken koşuldur. "Ön koşul" demek "ürün satışı engeller" demek değildir; satırın "Karşılanmazsa" sütunu da bunu söylüyor. **Hukuki iddianın yükümlüsü ürün değil, firmadır.** ETBİS Tebliği m.5/1-a kaydı *"kendilerine ait elektronik ticaret ortamında faaliyet gösteren hizmet sağlayıcılar"*a yükler; hizmet sağlayıcı firmadır (K-12, K-14). §4.2'nin giriş paragrafı rol dağılımında ETBİS kaydını adıyla firmanın yükümlülüğü sayıyor, `02 §12.5`'in ETBİS satırı da aynı. Firma kayıt yaptırmadan satarsa ihlal ve idari para cezası firmanındır; ürün kaydı ne yapabilir ne de doğrulayabilir. **Öneri elenmiş bir seçenektir.** K-574 (⚠) *"Alan satış kapısına bağlansın"* seçeneğini eledi, çünkü ürün kaydın varlığını denetleyemez. K-628 (⚠) ÖK-5'in niteleyicisini düzeltirken *"ürün kaydı denetlemez, satışı engellemez"* hücrelerini açıkça korudu. Önerinin "firma beyan etsin" yolu aynı sonuca varır: doğrulama alanı zaten firmanın beyanıdır, onu ayrı bir onay kutusuna çevirmek kaydın gerçekliğine bir şey katmadan kapıya bir adım ekler (K-573'teki beyan gerekçesiyle aynı). "Kurulumda doğrulansın" yolu da kapı olamaz: kayıt kurulumdan sonra, satış açılmadan önce de yapılabilir; satışı firma panelden açar ve kurulumu yapan o anda yoktur. **Ama satır kararın bir parçasını taşımıyordu.** ÖK-5 *"kaydı göremez"* diyordu ama kendi "Kim sağlar" hücresindeki doğrulama alanının neden kapıyı tutmadığını söylemiyordu. Panelin alanın yanında yükümlülüğü hatırlattığını da söylemiyordu (K-574). Satır böylece ürünün elinde bir kayıt bilgisi varken satışa hiç bakmadığı gibi okunuyordu. | ÖK-5 Karşılanmazsa: *"Ürün satışı engellemez — kaydı göremez; doğrulama alanı da firmanın beyanıdır, satış kapısını tutmaz ve panel yükümlülüğü alanın yanında hatırlatır (K-574). Yükümlülük ve sonucu firmadadır. …"* K-574 Kaynak sütununda zaten vardı. Satış kapısına ekleme önerisi uygulanmadı. |

**Dağılım:** 0 KABUL · 1 KISMİ · 1 RET. BULGU-1 üçüncü kez gelen ve kalan riski kayıtlı bir konudur; satıra yalnız taşıyıcı belirlemenin elenmiş bir seçenek olduğu yazıldı. BULGU-2 yeni bir konudur; öneri kayıtlı iki kararla elenmişti, ama satır kararın gerekçesini ve panelin hatırlatmasını taşımıyordu.

## 3. Ek bulgular

- **Mekanik tarama:** düzeltmelerde anılan karar numaraları ilgili satırlarda var (K-293 ve K-491 KP-22'nin, K-574 ÖK-5'in Kaynak sütununda). Sürüm başlığı ve alt bilgi aynı sürümü gösteriyor (v0.19). Kapsam satırı sayısı değişmedi (77), kapsam dışı satırı sayısı değişmedi (63).
- **`02` ile hizalı:** `02 §7.4.1` K-491'in kalan riskini, `02 §3.1.4` doğrulama alanının satış kapısını tutmadığını ve panelin hatırlatmasını zaten yazıyor. Çelişki doğmadı.
- **Etki yansıtmaya devredilen (`02` ve karar kaydı):** `02 §3.1.4` alanın kapıyı tutmamasını iki gerekçeyle yazıyor: *"kayıt yükümlülüğü her firmada doğmaz ve ürün kaydın varlığını denetleyemez"*. Birinci gerekçe K-574'ten gelir ve K-628'den önceki niteleyiciye dayanır. K-628 kaydı satış yapacak her firmaya bağladı; satış kapısı yalnız satışı açan firmayı ilgilendirdiği için bu gerekçe kapı bağlamında artık bir şey taşımıyor, kararı ikinci gerekçe tek başına taşıyor. Sonuç değişmediği için çelişki değildir. Etki yansıtmada §3.1.4'ten birinci gerekçenin çıkarılması değerlendirilebilir; K-574'ün satırına K-628'e bir çapraz referans da eklenebilir.
- **Tekrarlayan konular:** KP-22'nin geri ödeme başlangıcı üç turda geldi (5., 6., 8.). Satır artık hem kalan riski hem elenen seçeneği adıyla yazıyor. Konu yeni bir dayanak olmadan yeniden gelirse doğrudan RET olarak kalır.
- **Ciddi bir sorun görülmedi.** Bu turda ikinci modelin listesinin dışında ciddi bir sorun saptanmadı.
- **Önceki turlardan devreden notlar** yerinde duruyor: "İşlem izi" sözlük satırı, §8.5.2, §3.34.8 ve §12.1.12'ye 4733 m.8/5-k dayanağı (isteğe bağlı).

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`10` v0.19). Bu turda yeni karar satırı açılmadı. K-491, K-574 ve K-628 kayıtlı kararlardır; bu tur hiçbirini değiştirmedi, yalnız satırlara eksik parçalarını taşıdı.

- [x] BULGU-1 (ret: üçüncü kez gelen konu; kalan risk K-491'de kayıtlı ve KP-22'de yazılı, iade taşıyıcısı belirleme K-491'de elendi. KP-22'nin gerekçesine elenen seçenek yazıldı)
- [x] BULGU-2 (kısmi: ÖK-5 doğrulama alanının firmanın beyanı olduğunu, satış kapısını tutmadığını ve panelin yükümlülüğü hatırlattığını yazar — K-574. ETBİS kaydını satış kapısına bağlama önerisi K-574 ve K-628 gereği uygulanmadı)

**Hukuki kontrol** (2026-10-03):
- **Mesafeli Sözleşmeler Yönetmeliği m.12** (konsolide metin, MevzuatNo 20237; 5. ve 6. turda okundu, bu turda yeniden tarandı): m.12/1 *"Satıcı, cayma hakkına konu malın, iade için ön bilgilendirmede belirtilen taşıyıcıya teslim edildiği tarihten itibaren on dört gün içinde … iade etmekle yükümlüdür. Ancak tüketicinin malı, iade için öngörülenin haricinde bir taşıyıcı ile iade etmesi durumunda söz konusu yükümlülük malın satıcıya ulaştığı tarihten itibaren başlar."* m.12/5 *"Satıcının ön bilgilendirmede iade için herhangi bir taşıyıcıyı belirtmediği durumda ise tüketiciden iade masrafına ilişkin herhangi bir bedel talep edilemez"* — yalnız masrafı düzenler. RG 24/5/2025-32909 değişikliği 1/1/2026'dan beri uygulanıyor; bu tarihten sonra m.12'ye dokunan yeni bir değişiklik bulunamadı.
- **ETBİS Tebliği** (RG 11/8/2017-30151, konsolide metin; K-628'de 2026-10-03'te okundu): m.5/1-a *"Aşağıda belirtilen gerçek veya tüzel kişiler faaliyete başlamadan önce ETBİS'e kayıt olur: a) Kendilerine ait elektronik ticaret ortamında faaliyet gösteren hizmet sağlayıcılar."* Yükümlü hizmet sağlayıcıdır; kaydın ve bildirimin yapılmamasının idari para cezası da ona verilir (2026 için 143.102 TL – 715.516 TL). Kaydı ya da doğrulamasını yazılıma yükleyen bir hüküm yoktur.

**Kaynaklar (hukuki kontrol):**
- [Mesafeli Sözleşmeler Yönetmeliği — mevzuat.gov.tr konsolide metin (PDF)](https://www.mevzuat.gov.tr/MevzuatMetin/yonetmelik/7.5.20237.pdf)
- [Mesafeli Sözleşmeler Yönetmeliğinde Değişiklik — 24.05.2025 (Alomaliye)](https://www.alomaliye.com/2025/05/24/mesafeli-sozlesmeler-yonetmeliginde-degisiklik-24-05-2025/)
- [Ticaret Bakanlığı — Elektronik ticaret mevzuatı](https://ticaret.gov.tr/ic-ticaret/elektronik-ticaret/mevzuat)
- [Elektronik Ticaret Kanunu kapsamında idari para cezaları 2026 (Erdem & Erdem)](https://www.erdem-erdem.av.tr/bilgi-bankasi/elektronik-ticaret-kanunu-kapsaminda-idari-para-cezalari-2026-yili-icin-guncellendi)
