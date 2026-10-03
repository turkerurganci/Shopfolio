# Cross-Review — 10 MVP Scope (Tur 12)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/10_MVP_SCOPE.md` v0.22 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde; yalnız satır sonu boşlukları atıldı). İstem önceki turlarla aynıdır: daraltılmış talimat (K-615, K-644), yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §2 / KP-47
> Alıntı: "Firma yöneticisi bir kalemi ya da siparişi kapalı listeden sebep seçerek iptal eder — "stokta bulunamadı" seçildiğinde panel, onaydan önce stok yokluğunun yasal bir imkânsızlık sayılmadığını ve iptalin sonucunun firmada olduğunu hatırlatır."
> Sorun: Sistem, stokta bulunmamayı yasal imkânsızlık olmadığını kabul ettiği hâlde firmaya bu sebeple tek taraflı iptal yolu açıyor. Bu, tüketicinin sipariş hakkını kaybetmesine ve firmayı mevzuata aykırı iptal/yaptırım riskine sokar. Ticaret Bakanlığı da stok yokluğunu imkânsızlık saymaz ve bu gerekçeyle tek taraflı iptali aykırılık olarak belirtir. [Kaynak](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)
> Öneri: “Stokta bulunamadı” seçeneğini firma iptal sebebi olmaktan çıkarın; yalnız gerçek yasal imkânsızlık için, zorunlu bildirim ve iade sonuçlarını içeren ayrı akış tanımlayın.
>
> BULGU-2
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §3 / KD-28
> Alıntı: "Alkol, tütün, ilaç, reçeteli ürün ve silah gibi ek akış isteyen ürünlerin satış akışı MVP kapsamı dışındadır: yaş doğrulaması, izin akışı, engelleme mekanizması, yasaklı kategori listesi ve mevzuat belgesi alanı yoktur; sistem ne satıldığını denetlemez."
> Sorun: Doküman, alkol ve tütünün tüketiciye internetten satışının yasak olduğunu kabul ederken bu ürünlerin katalogda açılmasını ve satışını engelleyecek hiçbir kontrol tanımlamıyor. Böylece MVP, yasak satış sistemi kurulmasına imkân verir; 4733 sayılı Kanun m.8/5-k idari yaptırım ve erişim engeli sonucu öngörür. [Kaynak](https://www.tarimorman.gov.tr/TADAB/Belgeler/Kanunlar/kanun_4733.pdf)
> Öneri: MVP’yi yasaklı ürünleri satmayacak firmalarla sınırlayın veya katalogda alkol/tütün ve satış için ayrı zorunlu akış gerektiren kategorileri engelleyen bir uygunluk kontrolü ekleyin.> ```

Model sonuca varmadan önce yedi konuyu web aramasıyla kontrol etti (koşum kaydında görünür, çıktıya girmedi): m.12'nin 1/1/2026'dan beri uygulanan metni ve iade taşıyıcısı, hizmette cayma ve açık onay, stok yokluğunda satıcı iptali, ETBİS kayıt yükümlülüğü, indirimde önceki otuz günün en düşük fiyatı ve 4733 sayılı Kanun m.8/5-k. Yalnız iki konuda bulgu yazdı; ikisi de dokümanın gerekçesinde zaten yazılı olan bir hükme dayanıyor.

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ❌ RET | **Konu üçüncü kez geliyor ve doküman doğru** (3., 9. ve bu tur). Öneri 3. turunkiyle aynıdır: "stokta bulunamadı" sebebini listeden çıkarmak. Bu, K-609'da (⚠) *"'Stokta bulunamadı' listeden çıkarılır (Codex'in önerisi) elendi"* diye adıyla elenmiş seçenektir. **Dayanak dokümanın kendi cümlesidir, çelişki değildir:** panel uyarısı m.16/4'ün *"Malın stokta bulunmaması durumu, mal ediminin yerine getirilmesinin imkânsızlaşması olarak kabul edilmez"* cümlesini firmaya söyler; KP-47'nin gerekçesi iptalin *"müşterinin firmaya karşı yasal taleplerini ortadan kaldırmaz"* olduğunu yazar. Sebebin listede olması stok yokluğunu yasal imkânsızlık saymak değildir; sonuç firmada kalır. **"Tüketici sipariş hakkını kaybeder" iddiası tersine işler:** elinde malı olmayan firma, sebep listede olsa da olmasa da malı gönderemez. Sebep çıkarılırsa iptal "diğer"le yapılır, sebep kaybolur ve sebebe göre kargo ayrımı bozulur; firma hiç iptal edemezse müşteriye ne bildirim gider ne geri ödeme başlar. Gerekçe bunu 9. turdan beri yazıyor: *"iptal, müşteriye sebebiyle giden bildirimi ve geri ödemeyi başlatan yoldur."* **Hukuki iddia:** modelin gösterdiği Bakanlık rehberi bu turda yeniden açıldı. Rehber *"Malın stokta bulunmaması imkansızlaşma olarak kabul edilmemektedir"* der ve imkânsızlıkta üç gün içinde bildirimi, on dört gün içinde iadeyi anlatır; ürün ikisini de iptal anında verir (K-609, `02 §7.2.4`). Rehberde stok yokluğu sebebiyle iptali "aykırılık" diye adlandıran bir cümle yoktur; 9. turda görülen Bakanlık duyurusu bu gerekçeyle iptal eden satıcılara verilen cezayı yazar. Cezanın muhatabı satıcı olan firmadır (K-12, K-14). **Metinde dönüşe yol açan yeni bir boşluk görülmedi:** satır kuralı, uyarıyı, elenen seçeneği ve sonucu yazıyor. Model gerekçeyi değil, yalnız Özellik sütununun ilk cümlesini alıntıladı. | Uygulanmadı. |
| BULGU-2 | ❌ RET | **Konu ikinci kez geliyor ve doküman doğru** (6. ve bu tur). Öneri — katalogda alkol, tütün ve ek akış isteyen kategorileri engelleyen bir uygunluk kontrolü — K-84'te elenen seçenektir: *"Engelleme mekanizması, yasaklı kategori listesi ve ürün başına mevzuat belgesi alanı yoktur."* K-641 KD-28'in bugünkü cümlesini bu dille yazdı. 6. turda aynı bulgu KISMİ kabul edildi ve gerekçeye yasak ile yükümlünün kim olduğu eklendi; bu turun bulgusu o eklenen cümleyi dayanak yapıyor. **Dokümanda çelişki yoktur:** KD-28 sistemin satışı denetlemediğini; gerekçesi ise alkol ve tütünün internetten tüketiciye satışının yasak olduğunu ve *"Satılan ürünün mevzuata uygunluğu satıcı olan firmanın yükümlülüğüdür"* olduğunu birlikte yazar. `02`'nin sorumluluk tablosu aynı şeyi K-84'le söyler. Yasağı bilen ve uygulamayı satıcıya bırakan bir kural kendi içinde tutarlıdır. **Hukuki iddia:** 4733 sayılı Kanun m.8/5-k'nin resmî metni bu turda yeniden okundu: ceza *"… elektronik ticaret araçları ya da posta ile sipariş yöntemi kullanarak yapmak üzere satış sistemi kuran veya faaliyette bulunanlara"* verilir. Shopfolio tütün ya da alkol satmak üzere kurulmuş bir sistem değildir; satıcı firmadır ve ürün ticari zincirde yer almaz (K-12). Bu ürünle yasak bir satış yapan firma, kurduğu her genel e-ticaret yazılımıyla da yapabilirdi; hüküm genel amaçlı yazılıma bir engelleme yükümlülüğü koymaz. **Önerinin ilk yarısı ("yasaklı ürünleri satmayacak firmalarla sınırlayın") zaten kuralın sonucudur:** K-84'e göre alkol ve tütün bu üründe satılamaz; kapsam satırı bunu açık eder. | Uygulanmadı. |

**Dağılım:** 0 KABUL · 0 KISMİ · 2 RET. Bu turdaki yüzde yüz ret körü körüne verilmiş bir onay değildir. İki bulgu da dokümanın gerekçesinde elenen seçenek ya da yükümlüsü yazılmış kural olarak duran kararları (K-609, K-84, K-641) yeniden açıyor. İkisinin hukuki dayanağı bu turda resmî metne karşı yeniden kontrol edildi (Bakanlık rehberi, 4733 m.8/5-k). Hiçbiri dokümanın iki yeri arasında bir çelişki ya da olgusal bir hata göstermiyor; ikisi de dokümanın kendi yazdığı yasal cümleyi, yanındaki gerekçeyi okumadan, çelişki olarak sunuyor.

## 3. Ek bulgular

- **Mekanik tarama:** sürüm başlığı, sürüm notu ve alt bilgi aynı sürümü gösteriyor (v0.23). Kapsam satırı sayısı değişmedi (77), kapsam dışı satırı sayısı değişmedi (63).
- **Tekrarlayan konular:** KP-47'nin "stokta bulunamadı" sebebi üç turda (3., 9., 12.), KD-28'in satış engeli iki turda (6., 12.) geldi. İki satır da kuralı, elenen seçeneği ve yükümlüyü adıyla yazıyor. Gerekçeye bir cümle daha eklemek dönüşü kesmez, satırı büyütür: model iki bulguda da yalnız Özellik sütununu alıntıladı. Bu konular yeni bir resmî dayanak olmadan yeniden gelirse doğrudan RET olarak kalır.
- **Ciddi bir sorun görülmedi.** Bu turda ikinci modelin listesinin dışında ciddi bir sorun saptanmadı. Modelin aradığı diğer konularda (m.12 ve iade taşıyıcısı, hizmette cayma ve açık onay, ETBİS, indirimde önceki fiyat) bulgu yazılmadı; KP-22'nin geri ödeme başlangıcı altı turdan sonra bu turda gelmedi.
- **Önceki turlardan devreden notlar** yerinde duruyor: "İşlem izi" sözlük satırı, §8.5.2, §3.34.8 ve §12.1.12'ye 4733 m.8/5-k dayanağı (isteğe bağlı); `02 §3.1.4`'teki "kayıt yükümlülüğü her firmada doğmaz" gerekçesi (8. tur, etki yansıtmaya devredildi).

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`10` v0.23). Bu turda yeni karar satırı açılmadı. K-84, K-609 ve K-641 kayıtlı kararlardır; bu tur hiçbirini değiştirmedi.

- [x] BULGU-1 (ret: üçüncü kez gelen konu; "stokta bulunamadı" sebebini listeden çıkarmak K-609'da elenmiş seçenektir. Panel uyarısı stok yokluğunun yasal imkânsızlık sayılmadığını söyler, iptal müşterinin yasal taleplerini kaldırmaz ve bildirimi ile geri ödemeyi başlatan yoldur. Doküman değişmedi)
- [x] BULGU-2 (ret: ikinci kez gelen konu; satış engeli ve yasaklı kategori listesi K-84'te elendi, KD-28'in dili K-641'dir. 4733 m.8/5-k'nin muhatabı satış sistemini kuran ya da faaliyette bulunan satıcıdır; KD-28 yükümlünün firma olduğunu yazar. Doküman değişmedi)

**Hukuki kontrol** (2026-10-03):
- **4733 sayılı Kanun m.8/5-k** (mevzuat.gov.tr konsolide metin, bu turda yeniden açıldı): *"… internet, televizyon, faks ve telefon gibi elektronik ticaret araçları ya da posta ile sipariş yöntemi kullanarak yapmak üzere satış sistemi kuran veya faaliyette bulunanlara … idarî para cezası verilir. … Satışın internet ortamında yapılması halinde, … 5651 sayılı … Kanunda öngörülen usullere göre erişimin engellenmesine karar verilir …"*
- **Ticaret Bakanlığı tüketici rehberi — mesafeli sözleşmeler** (bu turda yeniden açıldı): *"Malın stokta bulunmaması imkansızlaşma olarak kabul edilmemektedir."* · *"Sipariş konusu mal ya da hizmetin tüketiciye tesliminin/sunulmasının 'imkansızlaştığı' hallerde, satıcının üç gün içinde tüketiciye durumu bildirmesi ve varsa teslimat masrafları da dâhil olmak üzere tahsil edilen tüm ödemeleri bildirim tarihinden itibaren en geç on dört gün içinde iade etmesi zorunludur."* Rehber sebebi seçerek iptal etmeyi ayrıca düzenlemez; yükümlüyü satıcı olarak yazar.
- **Mesafeli Sözleşmeler Yönetmeliği m.16/4** (RG 23/8/2022-31932 ile değişik; 9. turda konsolide metinden okundu, K-609'da alıntılı): *"Malın stokta bulunmaması durumu, mal ediminin yerine getirilmesinin imkânsızlaşması olarak kabul edilmez."*

**Kaynaklar (hukuki kontrol):**
- [4733 sayılı Kanun — mevzuat.gov.tr konsolide metin (PDF)](https://www.mevzuat.gov.tr/MevzuatMetin/1.5.4733.pdf)
- [Ticaret Bakanlığı — Mesafeli Sözleşmeler Hakkında Bilgilendirme](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)
- [Mesafeli Sözleşmeler Yönetmeliği — mevzuat.gov.tr konsolide metin (PDF)](https://mevzuat.gov.tr/File/GeneratePdf?mevzuatNo=20237&mevzuatTur=KurumVeKurulusYonetmeligi&mevzuatTertip=5)
