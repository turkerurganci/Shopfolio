# Cross-Review — 02 Product Requirements (Tur 24)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/02_PRODUCT_REQUIREMENTS.md` v0.39 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde; satır sonlarındaki boşluklar atıldı). İstem önceki turlarınkiyle aynıdır: yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: teknik doğruluk
> Seviye: Orta
> Yer: §3.21.11 Ters ibraz; §6.2.22
> Alıntı: “ürün sağlayıcıdan ters ibraz bilgisi almaz”
> Sorun: Ürün ters ibrazı hiç öğrenmediğini söylerken, ters ibrazla iade edilmiş kaleme ikinci geri ödemenin yapılmayacağını kural olarak koyuyor. Bu durum ürün tarafından güvenilir biçimde belirlenemez; sağlayıcının iade isteğini reddetmediği senaryoda çift ödeme oluşabilir.
> Öneri: Sağlayıcıdan ters ibraz durumunu güvenilir biçimde alma zorunluluğu ekleyin; bu yoksa kuralı otomatik engel değil, sağlayıcı panelinde zorunlu kontrol ve yönetici onayı gerektiren manuel süreç olarak yeniden yazın.
>
> BULGU-2
> Kriter: eksiklik
> Seviye: Yüksek
> Yer: §7.3.4 Fiziksel üründe cayma istisnası; §12.1.12
> Alıntı: “Listede olmayan bentler … ihtiyaç doğarsa sebep bir karar satırıyla eklenir.”
> Sorun: Sistem satılan ürünü denetlemezken cayma istisnası listesi güncel Yönetmelik m.15’teki bütün ilgili sözleşme türlerini kapsamaz; örneğin ayrıştırılamaz biçimde karışan mallar, süreli yayınlar ve belirli tarihte/dönemde sunulan konaklama, taşıma veya boş zaman hizmetleri eksiktir. Bu ürünler satılırsa sistem tüketiciye yanlış cayma akışı ve ön bilgilendirme sunar. [Ticaret Bakanlığı’nın güncel istisna özeti](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)
> Öneri: Ya katalog kapsamını mevcut istisna setiyle uyumlu ürünlerle açıkça sınırlandırın ve bunu yayın kapısında denetleyin, ya da m.15 kapsamındaki tüm uygulanabilir istisnaları ürün türü/özelliği bazında modelleyin ve Ön Bilgilendirme Formu’na yansıtın.
>
> BULGU-3
> Kriter: güvenlik
> Seviye: Orta
> Yer: §8.2 Deneme limitleri; §8.2.6 Girişin e-posta ekseni tanınan tarayıcıyı engellemez; §11.2 P-31
> Alıntı: “işaretli tarayıcının denemeleri hesap ve tarayıcı başına kendi sayacında aynı eşikle sayılır”
> Sorun: L-1 tablosu ve P-31 yalnız IP ve e-posta eksenlerini parametreleştirirken, §8.2.6 üçüncü bir “hesap + tanınan tarayıcı” sayacı tanımlıyor. Bu sayacın penceresi, engel kapsamı ve IP/e-posta sayaçlarıyla birleşme kuralı net değil; uygulamalar arasında farklı yorum, beklenmedik kilitlenme veya brute-force toleransı doğurur.
> Öneri: L-1’i üç ayrı ölçüm ekseniyle yeniden tanımlayın; tanınan tarayıcı sayacının anahtarını, P-31’e bağlı eşik/penceresini, IP engeliyle birlikte nasıl çalıştığını ve çerez silinmesi/yenilenmesi hâlindeki davranışı açıkça yazın.
> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ⚠️ KISMİ | **Çelişki gerçekti.** §3.21.11 (K-611) iki şeyi birlikte söylüyordu: *"ürün sağlayıcıdan ters ibraz bilgisi almaz"* ve *"ters ibrazla parası müşteriye dönmüş bir kalem için firma ürünün içinden ayrıca geri ödeme işlemez"*. İkinci cümle bir kural gibi yazılmıştı, ama ürünün onu uygulayacak bilgisi yoktu. Kural yalnız iki koşul birlikte varsa işler: sağlayıcı iadeyi reddetmiş olmalı ve sonraki adım firmanın elle adımı olmalı. Kart hattında müşterinin kendi iptali parayı firmanın dikkatine bağlamaz, iadeyi sistem kendiliğinden başlatır (§7.2.8; K-196). Sağlayıcı ters ibraz edilmiş işlemde iadeyi reddetmezse para ikinci kez gider; metin bu hâli hiç anmıyordu. **Alınmayan kollar:** (1) *"Sağlayıcıdan ters ibraz durumunu güvenilir biçimde alma zorunluluğu"*. Bu, K-611'in 23. turda elediği ayrı kaydın ve `10 §4.1` ÖK-4'e eklenecek sağlayıcı koşulunun aynısıdır; doküman bunu elenen seçenek olarak yazmıştı. (2) *"Manuel süreç ve yönetici onayı"*. Her kart iadesine bir onay adımı koymak K-196'yı bozar: kart hattında müşteri iptalinin parası firmayı beklemez, onay adımı on dört günlük yasal süreyi firmanın elle adımına yüklerdi. | **K-612 (öneriyle kaydedildi — ⚠; K-611'in çift ödeme kuralını daraltır).** §3.21.11'e şu yazıldı: kural ürünün değil, firmanın elle adımlarının kuralıdır. Firma bir siparişte bildirilmiş bir ters ibrazı biliyorsa geri ödeme adımı işlemeden önce sağlayıcının panelinde işleme bakar. Bu adımlar şunlardır: başarısız kart iadesini yeniden denemek, havale yolunu açmak, havale hattında geri ödemeyi işlemek. Kendiliğinden başlayan kart iadesi ters ibrazı beklemez. Bunun doğurduğu fazla ödeme kalan risktir ve firma bunu ürünün dışında çözer. İki seçeneğin neden elendiği yazıldı. 6.2.22 ve §7.2.8 hizalandı; §12.5'e yeni satır eklendi. |
| BULGU-2 | ❌ RET | **Doküman doğru; seçim bilinçli ve kayıtlı.** §7.3.4 listede olmayan bentleri adıyla sayıyor ve gerekçesini yazıyor: karışan mallar, süreli yayın, belirli tarihli konaklama ve eğlence hizmetleri hedef kitlede olağan değildir. Karar K-509'dur ve *"(d), (f), (g) bentleri de listeye girsin"* seçeneği orada açıkça elenmiştir. Bulgu bu seçimi bir hukuki hata olarak sunuyor (*"tüketiciye yanlış cayma akışı ve ön bilgilendirme"*), ama öyle değil. Mesafeli Sözleşmeler Yönetmeliği m.15/1 *"Taraflarca aksi kararlaştırılmadıkça, tüketici aşağıdaki sözleşmelerde cayma hakkını kullanamaz"* diye başlar. İstisnalar satıcıya tanınmış bir imkândır, zorunluluk değildir. Listede olmayan bentteki malda ürün cayma hakkını tanır ve ön bilgilendirme bunu yazar. Bu tüketici lehine bir sözleşme hükmüdür; tüketicinin hiçbir hakkı eksilmez, sonucu firmanın maliyetidir. Hizmette de durum aynıdır: §7.3.3 hakkı ifanın tamamlanmasına kadar açık tutar. **Bulgunun bu kez gelmesinin sebebi metindeki bir boşluktu:** §7.3.4 listede olmayan bentte ne olduğunu ve bunun hukuken neden sorun olmadığını söylemiyordu. | Karar değişmedi, yeni karar satırı açılmadı. §7.3.4'e şu yazıldı: listede olmayan bentteki malda ürün cayma hakkını tanır. m.15/1 "taraflarca aksi kararlaştırılmadıkça" diye başladığı için bu hukuka aykırı değildir, tüketici lehinedir ve maliyeti firmadadır. Listede adı geçmeyen (a) bendi de (fiyatı finansal piyasalara bağlı mal) aynı sebeple yazıldı. |
| BULGU-3 | ⚠️ KISMİ | **Belirsizlik gerçekti, ama dar.** §8.2.6 tanınan tarayıcının sayacını *"hesap ve tarayıcı başına kendi sayacında aynı eşikle"* diye tanımlıyordu. Penceresi, aşılınca neyi engellediği, IP ekseniyle ilişkisi ve çerez silinince ne olduğu metinden ancak çıkarımla okunabiliyordu. P-31 bu sayacı hiç anmıyordu. Sayaç K-603'ün (17. tur) kendi kararıdır. **Alınmayan kol:** *"L-1'i üç ayrı ölçüm ekseniyle yeniden tanımlayın"*. Üçüncü sayaç ayrı bir eksen değildir, e-posta ekseninin tanınan tarayıcıya düşen payıdır; aynı değeri taşır ve §11'de ayrı bir parametre gerektirmez. *"Brute-force toleransı"* kaygısının dayanağı yoktur: işaret yalnız başarılı girişle doğar (§8.2.6). İşareti taşıyan tarayıcı şifreyi zaten bilen birinindir ve IP ekseni ona da işler. | Karar değişmedi (K-603). §8.2.6'ya şunlar yazıldı: sayacın eşiği ve penceresi e-posta ekseninin değeridir (P-31). Sayaç aşılırsa yalnız o tarayıcıdan o hesaba şifreyle giriş engellenir. IP ekseniyle birlikte iki sayaçtan hangisi aşılırsa deneme engellenir. Çerezi silinen ya da işaretinin ömrü dolan tarayıcı işaretsizdir ve e-posta ekseninde sayılır. L-1 satırı ve P-31 hizalandı; §8 ve §11'in kaynak satırlarına K-603 eklendi. |

**Dağılım:** 0 KABUL · 2 KISMİ · 1 RET.

- **BULGU-1:** Daha önce gelmemişti; 23. turda açılan K-611'in kendi iç çelişkisidir.
- **BULGU-2:** Cross-review'da ilk kez geldi. Liste 2026-10-03 tarihli audit'te Yönetmelik'in güncel metnine çekilmişti (K-509).
- **BULGU-3:** Daha önce gelmemişti. Tanınan tarayıcı K-603 ile 17. turda girdi; 18. ve 19. turların ek bulguları işaretlerin şifre değişikliğindeki durumuna bakmıştı, sayacın tanımına bakmamıştı.

## 3. Ek bulgular

- **§8 ve §11'in kaynak satırlarında K-603 yoktu.** Tanınan tarayıcı kararı L-1 satırında ve §8.2.6'da anılıyordu, ama bölümlerin kaynak listesine girmemişti. İki satıra eklendi.
- **§12.5'te ters ibraz satırı yoktu.** K-611 firmaya bir kontrol yükümlülüğü yüklemişti, ama sorumluluk tablosu bunu taşımıyordu. K-612 ile satır eklendi; §12'nin kaynak satırına K-611 ve K-612 girdi.
- **Önceki turlardan açık kalanlar:** 21. turun "üyenin e-postasını değiştirmesinde teyidin hangi adrese yapılacağı" notu ile 20. turun K-606 hizmet ve dijital kalem notu bu turda da gelmedi; `04` bunları ele almalı.
- **Etki yansıtma için not:** `01` ve `10`'da ters ibraz ya da tanınan tarayıcının sayacı için cümle yok; çelişki doğmadı. `04` "geri ödeme gerçekleşmedi" uyarısındaki ters ibraz hatırlatmasının metnini tasarlamalı. `12`'nin Mesafeli Satış Sözleşmesi ve Ön Bilgilendirme Formu taslakları, listede olmayan bentteki malda cayma hakkının tanındığını yazmalı (K-509; §7.3.4).

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`02` v0.40). Bir yeni karar satırı açıldı ve ⚠ ile işaretli. Seçenekleri gerçekten ayrışan ve firmaya ya da müşteriye ciddi para veya hukuki risk yükleyen bir konu çıkmadı. K-612 ürünün mekaniğini değiştirmiyor: K-611'in vaat ettiği ama ürünün yerine getiremeyeceği bir korumayı doğru sınırına çekiyor ve kalan riski adıyla yazıyor.

- [x] BULGU-1 (kısmi — K-612 ⚠)
- [x] BULGU-2 (red — K-509; §7.3.4'e m.15/1'in "taraflarca aksi kararlaştırılmadıkça" hükmü yazıldı)
- [x] BULGU-3 (kısmi — K-603; sayacın tanımı açık yazıldı)
- ⚠ **K-612:** Ters ibrazda çift ödemeye karşı kural firmanın elle adımlarına bağlıdır. Kart hattında müşteri iptalinde sistemin kendiliğinden başlattığı iade ters ibrazı beklemez; sağlayıcı reddetmezse para ikinci kez gider ve firma bunu ürünün dışında çözer. Elenen seçenekler: sağlayıcıdan ters ibraz bilgisi almak (K-611'in elediği kayıt) ve her kart iadesine firma onayı koymak (K-196'yı bozar). Gözden geçirilecek nokta: kalan riskin büyüklüğü, sağlayıcının ters ibraz edilmiş işlemde iadeyi reddedip reddetmediğine bağlıdır. Bu, sağlayıcı seçiminde (`10 §4.1`) sorulabilecek bir sorudur.

**Hukuki kontrol** (2026-10-03):
- **BULGU-1:** Hukuki iddia yok.
- **BULGU-2:** Mesafeli Sözleşmeler Yönetmeliği m.15/1 (mevzuat.gov.tr konsolide metni; bugün indirilmiş kopyadan okundu, Danıştay Onuncu Dairesi'nin 6/5/2026 tarihli iptal kararını işliyor): *"Taraflarca aksi kararlaştırılmadıkça, tüketici aşağıdaki sözleşmelerde cayma hakkını kullanamaz"*. Bentler: (a) finansal piyasaya bağlı fiyat, (b) kişiye özel mal, (c) çabuk bozulan mal, (ç) hijyen, (d) karışan mal, (e) maddi ortamdaki kitap, dijital içerik ve sarf malzemesi, (f) süreli yayın, (g) belirli tarihli konaklama, taşıma, kiralama, yiyecek-içecek ve boş zaman, (ğ) anında ifa edilen elektronik hizmet ve gayrimaddi mal, (h) tüketicinin onayıyla ifasına başlanan hizmet. (ı), (j), (k) iptal edildi, (i) mülgadır. Codex'in saydığı (d), (f), (g) bentleri metinde vardır; bu okuması doğrudur. Ancak giriş cümlesi istisnayı tarafların aksine anlaşmasına bıraktığı için istisnayı uygulamamak hukuka aykırı değildir.
- **BULGU-3:** Hukuki iddia yok.
