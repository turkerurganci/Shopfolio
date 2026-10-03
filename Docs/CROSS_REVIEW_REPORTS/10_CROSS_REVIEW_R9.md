# Cross-Review — 10 MVP Scope (Tur 9)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/10_MVP_SCOPE.md` v0.19 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde). İstem önceki turlarla aynıdır: daraltılmış talimat (K-615, K-644), yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: KP-22
> Alıntı: "teslimden sonraki caymada malın firmaya ulaştığı tarihten itibaren"
> Sorun: Doküman iade taşıyıcısı belirlemediği hâlde geri ödeme süresini malın firmaya ulaşmasına bağlar. Bu, tüketicinin kargoya teslim ettiği gün yerine teslimat süresini başlatır ve geri ödemeyi belirsiz biçimde geciktirebilir. Bakanlığın güncel açıklaması, farklı bir taşıyıcı kullanılması hâlinde ulaşma tarihini; aksi durumda kargoya teslim tarihini esas alır. [Ticaret Bakanlığı](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)
> Öneri: İade taşıyıcısı belirlenmeyecekse geri ödeme süresini, tüketicinin malı herhangi bir taşıyıcıya teslim ettiği tarih ve gönderi kanıtı üzerinden başlatın; alternatif olarak ön bilgilendirmede taşıyıcı belirleyin.
>
> BULGU-2
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: KP-39
> Alıntı: "alan yayın koşulu değildir; doldurmak firmanın yükümlülüğüdür, panel bunu hatırlatır ve alan boşsa birim fiyat gösterilmez"
> Sorun: Ölçüyle satılan bir ürün, ölçü birimi ve net miktar girilmeden yayına alınabilir; sonuçta zorunlu birim fiyat gösterilmeden satışa açılır. Bu, satıcının fiyat etiketi yükümlülüğüne aykırı satış yapılmasına doğrudan izin verir. [Ticaret Bakanlığı fiyat etiketi rehberi](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/fiyat-etiketleri-hakkinda-bilgilendirme)
> Öneri: Yayından önce firma ölçüyle satılmadığını beyan etmeli veya ölçü birimi ile net miktarı girmelidir; ölçüyle satış beyanında bu alanlar zorunlu olmalıdır.
>
> BULGU-3
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: KP-47
> Alıntı: "Firma yöneticisi bir kalemi ya da siparişi kapalı listeden sebep seçerek iptal eder — \"stokta bulunamadı\" seçildiğinde panel, onaydan önce stok yokluğunun yasal bir imkânsızlık sayılmadığını ve iptalin sonucunun firmada olduğunu hatırlatır."
> Sorun: Panel, stok yokluğunun yasal imkânsızlık olmadığını bildiği hâlde firmaya bu gerekçeyle tek taraflı iptal işlemini yaptırır. Uyarı, işlemi engellemediği için tüketicinin onayı olmadan sipariş iptali akışını meşrulaştırır; bu uygulama Bakanlığın açıkça aykırı gördüğü ve yaptırım uyguladığı bir durumdur. [Ticaret Bakanlığı](https://ticaret.gov.tr/haberler/ticaret-bakanligi-e-ticarette-siparisleri-gerekcesiz-iptal-eden-firmalari-uyardi)
> Öneri: Firma kaynaklı genel iptal akışını kaldırın; yalnız kanuni imkânsızlık hâlleri için, zorunlu bildirim ve geri ödeme yükümlülüklerini içeren ayrı ve sınırlı bir süreç tanımlayın.> ```

Model sonuca varmadan önce beş konuyu web aramasıyla kontrol etti (koşum kaydında görünür, çıktıya girmedi): Mesafeli Sözleşmeler Yönetmeliği m.12'nin 1/1/2026'dan beri uygulanan metni, ETBİS kaydı, e-ticarette firmanın sipariş iptali, m.15'in cayma istisnaları ve Fiyat Etiketi Yönetmeliği'nin indirimli satış kuralı. ETBİS, cayma istisnaları ve indirimli satış için bulgu yazmadı.

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ❌ RET | **Konu dördüncü kez geliyor ve doküman doğru** (5., 6., 8. ve bu tur). İddia 6. ve 8. turdakiyle aynıdır: taşıyıcı belirtilmeyince süre kargoya verilişten başlamalı. **İddia kayıtlı kalan risktir ve satırda yazılı:** KP-22'nin gerekçesi *"taşıyıcı hiç belirtilmediğinde bu sonucun açık hükmü yoktur — tüketici lehine bir yorum başlangıcı malın kargoya verildiği güne çekebilir"* diyor (K-491). **"Geri ödemeyi belirsiz biçimde geciktirir" iddiası da karşılanmış:** gerekçe, firma geri ödemeyi iade gönderisinin kargoya verildiği günden on dört gün içinde yaparsa iki okumada da süreyi kaçırmadığını yazıyor. Müşteri malı karşı ödemeli gönderir, kargo kaydı iki tarihi de gösterir; mal hiç ulaşmazsa konu elle müdahaleye kalır (K-491, ürün sonucu 4). **Önerinin iki yarısı da elenmiş seçeneklerdir:** "ön bilgilendirmede taşıyıcı belirleyin" K-293 ve K-491'de elendi; satır bunu 8. turdan beri adıyla yazıyor (*"taşıyıcı belirlemek bilinçli olarak elendi"*). "Süreyi gönderi kanıtı üzerinden başlatın" seçeneğinin istediği tarihi sistem kargo entegrasyonu olmadan bilemez (K-132). **Hukuki iddia:** modelin dayandığı Bakanlık rehberi *"İade işlemi, ön bilgilendirmede belirtilenden farklı bir kargo ile yapılmışsa bu süre, ürünün satıcıya ulaştığı tarihte başlar"* diyor. Taşıyıcı hiç belirtilmediğinde süre için ayrı bir şey söylemiyor, yalnız masrafın tüketiciye yüklenemeyeceğini yazıyor; modelin "aksi durumda kargoya teslim tarihi esas alınır" özeti rehberde yok. Yönetmelik m.12/1'in güncel metni de aynı iki hâli düzenliyor; m.12/5 taşıyıcısız hâli yalnız masraf bakımından düzenliyor. **Dönmesinin sebebi metinde bir belirsizlik değil:** satır kuralı, dayanağını, elenen seçeneği ve kalan riski açıkça yazıyor; model bilinçli karara katılmıyor. Satıra yeni bir şey eklenmedi. | Yok. |
| BULGU-2 | ❌ RET | **Konu dördüncü kez geliyor ve doküman doğru** (2., 5., 7. ve bu tur). **Önerinin kendisi elenmiş seçenektir:** "firma ölçüyle satılmadığını beyan etsin ya da alanı doldursun" K-573'te *"Her fiziksel üründe satış birimi (adet ya da ölçü) seçimi zorunlu olsun"* adıyla elendi: adetle satılan ürünlerin çoğuna her kayıtta bir adım ekler. KP-39'un gerekçesi bunu ve kalan riski yazıyor: *"Kalan risk bilinçlidir: firma ölçüyle sattığı ürünü alanı doldurmadan yayına alırsa birim fiyat gösterilmez ve bunun yasal sonucu satıcı olan firmadadır (K-573)."* **Hukuki iddia:** Bakanlığın fiyat etiketi rehberi birim fiyat dahil etiket yükümlülüğünü satıcıya yüklüyor; yazılıma yükleyen bir hüküm yok (7. turda RG 33044/33153 değişiklikleriyle birlikte kontrol edildi). Sistem bir ürünün ölçüyle satıldığını kayıttan bilemez (K-84). "Aykırı satışa doğrudan izin verir" iddiası, firmanın kendi yükümlülüğünü yerine getirmemesidir ve kayıtlı kalan risktir. **Dönmesinin sebebi metinde bir belirsizlik değil:** satır alanın yayın koşulu olmadığını, yükümlülüğün firmada olduğunu, panelin hatırlattığını, elenen seçeneği ve kalan riski yazıyor. | Yok. |
| BULGU-3 | ❌ RET | **Konu ikinci kez geliyor** (3. turda "sebebi listeden çıkarın" olarak geldi, K-609). Bu turun önerisi daha geniş: firma kaynaklı genel iptal akışını kaldırmak. **Doküman doğru:** K-609 (⚠) firma iptalinin m.16/4'ün yolunu karşıladığını yazıyor: ürün iptal anında müşteriye sebebiyle e-posta gönderir, geri ödemenin on dört günü iptalden işler, kart hattında geri ödemeyi sistem başlatır (KP-47, `02 §7.2.4`). "Stokta bulunamadı" seçildiğinde panel onaydan önce stok yokluğunun yasal imkânsızlık sayılmadığını ve sonucun firmada olduğunu söyler; gerekçe iptalin müşterinin yasal taleplerini kaldırmadığını yazar. **Hukuki iddia:** m.16/4 *"Malın stokta bulunmaması durumu, mal ediminin yerine getirilmesinin imkânsızlaşması olarak kabul edilmez"* diyor; stok yokluğunda firma ifa yükümlülüğünden kurtulmaz, müşteri m.16/2'deki fesih hakkını ve kanunun öteki yollarını kullanabilir. Modelin gösterdiği 16/6/2023 tarihli Bakanlık duyurusu, stok yokluğu gerekçesiyle iptal eden **satıcılara** idari para cezası verildiğini yazıyor; yükümlü ve cezanın muhatabı satıcı firmadır (K-12, K-14). **Önerinin sonucu müşterinin aleyhinedir:** elinde malı olmayan firma iptal edemezse malı yine gönderemez, ama müşterinin parası geri ödenmez ve bildirim gitmez. Kalem "diğer"le iptal edilirse sebep kaybolur (K-609'da elenen yol). Ayrı bir "kanuni imkânsızlık süreci" de K-609'da *"aynı iki adımı ikinci bir adla tekrarlardı"* gerekçesiyle elendi. **Ama gerekçe önerinin bu geniş hâlini karşılamıyordu:** satır sebebin neden listede kaldığını söylüyordu, iptal akışının kendisinin müşteriye ne sağladığını söylemiyordu; model uyarıyı "aykırı iptali meşrulaştırma" diye okudu. Gerekçeye bir cümle eklendi, kural değişmedi. | KP-47 Gerekçe: *"… iptal, müşterinin firmaya karşı yasal taleplerini ortadan kaldırmaz. Firma iptalini kaldırmak müşteriyi korumaz: iptal, müşteriye sebebiyle giden bildirimi ve geri ödemeyi başlatan yoldur (K-609)."* K-609 Kaynak sütununda zaten vardı. Öneri uygulanmadı. |

**Dağılım:** 0 KABUL · 0 KISMİ · 3 RET. Bu turdaki yüzde yüz ret körü körüne verilmiş bir onay değildir. Üç bulgu da dokümanın kendi gerekçesinde elenen seçenek ya da kalan risk olarak yazılmış kararları (K-491, K-573, K-609) yeniden açıyor ve her biri güncel resmî metne karşı yeniden kontrol edildi. Hiçbiri dokümanın iki yeri arasında bir çelişki ya da olgusal bir hata göstermiyor. BULGU-3'ün geniş önerisi için gerekçeye bir cümle eklendi.

## 3. Ek bulgular

- **Mekanik tarama:** K-609 KP-47'nin Kaynak sütununda var. Sürüm başlığı, sürüm notu ve alt bilgi aynı sürümü gösteriyor (v0.20). Kapsam satırı sayısı değişmedi (77), kapsam dışı satırı sayısı değişmedi (63).
- **`02` ile hizalı:** `02 §7.2.4` ve §6.7.6 K-609'un bildirimini, geri ödeme başlangıcını ve "stokta bulunamadı" uyarısını zaten yazıyor; bu turun eklediği cümle bir gerekçedir, kural değil. Çelişki doğmadı.
- **Tekrarlayan konular:** KP-22'nin geri ödeme başlangıcı dört turda (5., 6., 8., 9.), KP-39'un ölçü birimi alanı dört turda (2., 5., 7., 9.), KP-47'nin "stokta bulunamadı" sebebi iki turda (3., 9.) geldi. Üç satır da kuralı, elenen seçeneği ve kalan riski adıyla yazıyor. Bu konular yeni bir dayanak olmadan yeniden gelirse doğrudan RET olarak kalır. İkinci modelin bu turda yeni bir konu getirmemesi, ciddi sorun sınıfında dokümanın doygunluğa yaklaştığını gösteriyor.
- **Ciddi bir sorun görülmedi.** Bu turda ikinci modelin listesinin dışında ciddi bir sorun saptanmadı.
- **Önceki turlardan devreden notlar** yerinde duruyor: "İşlem izi" sözlük satırı, §8.5.2, §3.34.8 ve §12.1.12'ye 4733 m.8/5-k dayanağı (isteğe bağlı); `02 §3.1.4`'teki "kayıt yükümlülüğü her firmada doğmaz" gerekçesi (8. tur, etki yansıtmaya devredildi).

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`10` v0.20). Bu turda yeni karar satırı açılmadı. K-491, K-573 ve K-609 kayıtlı kararlardır; bu tur hiçbirini değiştirmedi.

- [x] BULGU-1 (ret: dördüncü kez gelen konu; kalan risk K-491'de kayıtlı ve KP-22'de yazılı, iade taşıyıcısı belirleme K-293 ve K-491'de elendi. Değişiklik yok)
- [x] BULGU-2 (ret: dördüncü kez gelen konu; "her üründe satış birimi seçimi" K-573'te elendi, kalan risk KP-39'da yazılı. Değişiklik yok)
- [x] BULGU-3 (ret: firma iptali m.16/4'ün bildirim ve geri ödeme yolunu karşılar, "stokta bulunamadı" sebebi K-609 gereği listede kalır. KP-47'nin gerekçesine iptalin müşteriye bildirimi ve geri ödemeyi başlatan yol olduğu yazıldı)

**Hukuki kontrol** (2026-10-03):
- **Mesafeli Sözleşmeler Yönetmeliği m.12/1 ve m.12/5** (konsolide metin, MevzuatNo 20237; 5., 6. ve 8. turda okundu, bu turda yeniden tarandı): m.12/1 *"Satıcı, cayma hakkına konu malın, iade için ön bilgilendirmede belirtilen taşıyıcıya teslim edildiği tarihten itibaren on dört gün içinde … iade etmekle yükümlüdür. Ancak tüketicinin malı, iade için öngörülenin haricinde bir taşıyıcı ile iade etmesi durumunda söz konusu yükümlülük malın satıcıya ulaştığı tarihten itibaren başlar."* m.12/5 taşıyıcısız hâli yalnız masraf bakımından düzenler.
- **Mesafeli Sözleşmeler Yönetmeliği m.16** (aynı konsolide metin; RG 23/8/2022-31932 ile değişik): m.16/2 *"Satıcı veya sağlayıcının birinci fıkrada yer alan yükümlülüğünü yerine getirmemesi durumunda, tüketici sözleşmeyi feshedebilir."* m.16/4 *"Sipariş konusu mal ya da hizmet ediminin yerine getirilmesinin imkansızlaştığı hallerde satıcı … bu durumu öğrendiği tarihten itibaren üç gün içinde tüketiciye yazılı olarak veya kalıcı veri saklayıcısı ile bildirmesi ve … tahsil edilen tüm ödemeleri bildirim tarihinden itibaren en geç on dört gün içinde iade etmesi zorunludur. Malın stokta bulunmaması durumu, mal ediminin yerine getirilmesinin imkânsızlaşması olarak kabul edilmez."*
- **Ticaret Bakanlığı tüketici rehberi — mesafeli sözleşmeler:** *"İade işlemi, ön bilgilendirmede belirtilenden farklı bir kargo ile yapılmışsa bu süre, ürünün satıcıya ulaştığı tarihte başlar."* Taşıyıcısız hâl için süreye dair ayrı bir ifade yok.
- **Ticaret Bakanlığı duyurusu (16/6/2023):** siparişi açıklama yapmadan ya da stok yokluğu gerekçesiyle iptal eden satıcılara 2022'de 46 firmaya, 2023'ün ilk beş ayında 6 firmaya idari para cezası verildi; muhatap satıcıdır.
- **Ticaret Bakanlığı tüketici rehberi — fiyat etiketleri:** birim fiyat dahil etiket yükümlülüğü satıcıya aittir.

**Kaynaklar (hukuki kontrol):**
- [Mesafeli Sözleşmeler Yönetmeliği — mevzuat.gov.tr konsolide metin (PDF)](https://www.mevzuat.gov.tr/MevzuatMetin/yonetmelik/7.5.20237.pdf)
- [Ticaret Bakanlığı — Mesafeli sözleşmeler hakkında bilgilendirme](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)
- [Ticaret Bakanlığı — E-ticarette siparişleri gerekçesiz iptal eden firmaları uyardı](https://ticaret.gov.tr/haberler/ticaret-bakanligi-e-ticarette-siparisleri-gerekcesiz-iptal-eden-firmalari-uyardi)
- [Ticaret Bakanlığı — Fiyat etiketleri hakkında bilgilendirme](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/fiyat-etiketleri-hakkinda-bilgilendirme)
