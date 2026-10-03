# Cross-Review — 02 Product Requirements (Tur 23)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/02_PRODUCT_REQUIREMENTS.md` v0.38 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde; satır sonlarındaki boşluklar atıldı). İstem önceki turlarınkiyle aynıdır: yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §6.7.6 Sipariş yürütümü; §7.2.4 İptal
> Alıntı: “Firma siparişi karşılayamaz — stok hatası, ürünün bulunamaması, teslimatın mümkün olmaması”
> Sorun: İfanın imkânsızlaşması için ayrı yasal akış yoktur. Özellikle “stokta bulunamama” yasal imkânsızlık sayılmaz; buna rağmen aynı firma iptali hattına alınmış. İmkânsızlıkta gereken kalıcı veri saklayıcısıyla bildirim ve tahsil edilen tüm bedellerin teslimat masrafıyla iadesine ilişkin süre/başlangıç da tanımlı değildir.
> Öneri: “İfa imkânsızlığı”nı firma iptalinden ayırın; öğrenmeden itibaren 3 gün içinde bildirim, bildirimden itibaren 14 gün içinde tüm tahsilatın iadesi kuralını ekleyin. Stok yokluğunu bu akıştan çıkarın. [Mesafeli Sözleşmeler Yönetmeliği m.16](https://resmigazete.gov.tr/eskiler/2014/11/20141127-6.htm)
>
> BULGU-2
> Kriter: Teknik doğruluk
> Seviye: Orta
> Yer: §3.8.5 Fiyat etiketi; §12.1.5 Fiyat tüketiciye KDV dahil gösterilir
> Alıntı: “Yerli üretim logosu firmanın görsel içeriğidir: logo kullanılacaksa firma onu ürün görseline koyar”
> Sorun: Türkiye üretimli mallarda yerli üretim logosu zorunlu etiketi bilgisidir; belge bunu isteğe bağlı ve kontrolsüz bir görsel tercihi yapıyor. Böylece fiziksel ürünün üretim yeri Türkiye seçildiğinde zorunlu işaretin gösterileceği garanti edilmiyor.
> Öneri: Üretim yeri Türkiye olduğunda resmî yerli üretim logosunu ürün sayfası/fiyat bilgisi bileşeninde sistemsel olarak gösterin; firma görseline bırakmayın. [Fiyat Etiketi Yönetmeliği m.5](https://tuketici.ticaret.gov.tr/data/5e81982d13b876a1b04c7a42/2023-6502%20Say%C4%B1l%C4%B1%20T%C3%BCketicinin%20Korunmas%C4%B1%20Hakk%C4%B1nda%20Kanun.pdf)
>
> BULGU-3
> Kriter: Teknik doğruluk
> Seviye: Orta
> Yer: §3.14.6 Adresin alanları; §12.1.10 Fatura ürünün dışındadır
> Alıntı: “Posta kodu ve T.C. kimlik numarası istenmez”
> Sorun: Doküman, tüm nihai tüketici faturalarında dış muhasebe aracının `11111111111` kullanabileceğini varsayıyor. Güncel GİB e-Arşiv teknik kılavuzu gerçek kişi alıcı bilgisinde T.C. kimlik numarasını zorunlu tanımlar; sınırlı tutarlı “nihai tüketici” istisnası da genel bir çözüm değildir. Bu nedenle bazı fatura senaryoları ürünün toplamadığı veri yüzünden kesilemeyebilir.
> Öneri: Muhasebe/e-belge sağlayıcısı ile doğrulanmış kuralı yazın; gerekli hâllerde T.C. kimlik numarasını yalnız faturalama amacıyla, koşullu ve ayrı bir akışta toplayın veya sağlayıcının uyumlu nihai tüketici mekanizmasını açıkça zorunlu kılın. [GİB e-Arşiv Teknik Kılavuzu](https://ebelge.gib.gov.tr/dosyalar/kilavuzlar/e-Arsiv_Teknik_Kilavuzu_V.1.18.pdf)
>
> BULGU-4
> Kriter: Edge case
> Seviye: Orta
> Yer: §5.5 Ödeme ekseni; §6.2.16 Satın alma
> Alıntı: “İptal edilmiş bir siparişe sağlayıcıdan başarılı ödeme bildirimi gelir”
> Sorun: Ödeme sağlayıcısının daha önce başarılı ve teslim edilmiş bir kart işlemini sonradan ters ibraz/chargeback/ters kayıtla geri alması için hiçbir akış yok. Bu durumda ödeme durumu, satış özeti, dışa aktarma, fatura verisi ve müşteriye yapılacak ek iade birbirinden kopabilir.
> Öneri: Sağlayıcıdan gelen ters ibraz/geri alma olayını ayrı bir finansal kayıt olarak tanımlayın; siparişi yeniden açmadan firmaya uyarı, muhasebe/dışa aktarma görünümü, bekleyen iş ve müşteriye yapılacak işlem kurallarını belirleyin.```
## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ⚠️ KISMİ | **Sorun gerçekti, ama önerinin iki kolu yanlıştı.** Mesafeli Sözleşmeler Yönetmeliği m.16/4 (2022'de değişik metin) satıcıya iki şey yükler: ifanın imkânsızlaştığını öğrendikten sonra üç gün içinde tüketiciye yazılı olarak ya da kalıcı veri saklayıcısıyla bildirmek ve teslimat masrafları dahil tahsil edilen tüm ödemeleri bildirimden itibaren on dört gün içinde iade etmek. Hüküm *"Malın stokta bulunmaması durumu ... imkânsızlaşması olarak kabul edilmez"* diye biter. `02` firma iptalini (§7.2.4, K-199) bu hükme hiç bağlamıyordu ve "stokta bulunamadı" sebebini (K-373) ötekilerle aynı yola koyuyordu. **Ürünün mekaniği hükmün iki sonucunu zaten veriyordu:** sebep iptal anında e-postayla bildiriliyordu (B-7; e-posta kalıcı veri saklayıcısıdır), geri ödemenin on dört günü iptalden, yani bildirimden işliyordu (§7.2.8) ve kargo ücreti §7.2.9'a göre geri ödeniyordu. Eksik olanlar şunlardı: hükmün metinde yazılmaması, B-7'nin kalem iptalini açıkça kapsamaması, üç günlük sürenin kime ait olduğu ve stok yokluğunun ayrı niteliği. **Alınmayan kollar:** (1) *"Ayrı bir ifa imkânsızlığı akışı"*: aynı iki adımı ikinci bir adla tekrarlardı. (2) *"Stok yokluğunu bu akıştan çıkarın"*: firma elinde olmayan malı yine gönderemez (K-199'un gerekçesi: sayısal stok eşzamanlılık hatasına kapalı değildir). Sebep listeden çıkarılsaydı firma iptali "diğer"le yapardı ve K-373'ün sebebe göre kargo ayrımı kaybolurdu. | **K-609 (öneriyle kaydedildi — ⚠).** §7.2.4'e hükmün yolu yazıldı: bildirim iptal anında gider, kalem iptalinde de; on dört gün iptalden işler; üç günlük süre firmanın yükümlülüğüdür. "Stokta bulunamadı" listede kalır, ama seçildiğinde panel onaydan önce firmaya stok yokluğunun yasal bir imkânsızlık sayılmadığını ve sonucun firmada olduğunu söyler. Kısmi iptalde kargo ücretinin alıkonmasının kalan riski yazıldı. §6.7.6, §9.2 B-7 (kalem iptali açıkça), §12.1.8 ve §12.5 (yeni satır) hizalandı. |
| BULGU-2 | ✅ KABUL | **Doküman kendi kararıyla çelişiyordu.** Fiyat Etiketi Yönetmeliği m.5/2-e, üretim yeri Türkiye olan malda Bakanlığın ilan ettiği logonun etikette ve fiyat listesinde bulunmasını zorunlu sayar; Ticaret Bakanlığı'nın duyurusu da bunu zorunlu diye anlatır. K-511 bu bendi kendi gerekçesinde zorunlu diye alıntılamıştı ve *"Bilgi firmanın serbest açıklamasına kalsın"*ı *"zorunlu bilgi bir alana bağlanmazsa eksik kalır"* gerekçesiyle elemişti. Ama logoyu firmanın ürün görseline bırakmıştı. Tetik zaten vardı: üretim yeri fiziksel üründe zorunlu bir alandır (§3.8.5). | **K-610 (öneriyle kaydedildi — ⚠; K-511'in logo cümlesini değiştirir).** §3.8.5: üretim yeri Türkiye olan fiziksel üründe sistem yerli üretim logosunu ürün sayfasında fiyatın yanında kendiliğinden gösterir. Logo ürünle gelen bir varlıktır, firma değiştirmez ve kapatamaz. Malın yerli üretim sayılıp sayılmadığı firmanın beyanıdır (yurt dışından gelip Türkiye'de yalnız ambalajlanan mal yerli sayılmaz). §12.1.5 hizalandı. |
| BULGU-3 | ❌ RET | **Doküman doğru. Aynı itiraz dördüncü kez geldi (7., 14. ve 22. turlar).** Bu kez dayanak GİB'in e-Arşiv Teknik Kılavuzu V.1.18 (Ağustos 2025). Kılavuz indirilip okundu: `aliciBilgileri` elemanında gerçek kişi alıcı için *"tckn: Vatandaşlık numarası"* alanını tanımlar. Ama bu bir şema alanıdır. Gelir İdaresi'nin duyurusundaki `11111111111` de tam bu alana yazılan değerdir; kılavuzda tutar sınırı ya da `11111111111`'i yasaklayan bir cümle yoktur. Bulgunun *"sınırlı tutarlı 'nihai tüketici' istisnası"* iddiası da bir hüküm göstermiyor. Fatura düzenleme sınırı (VUK m.232) ve e-Arşiv'e geçiş eşikleri faturanın düzenlenmesine ve biçimine aittir; bu 22. turda §3.14.6'ya yazıldı. Karar K-561'dir. **Konunun bu kez dönmesinin sebebi metindeki bir boşluktu:** §3.14.6 *"alıcı alanına"* diyordu, ama teknik kılavuzun o alanı "kimlik numarası" diye tanımladığını söylemiyordu. Kılavuzu okuyan biri bu yüzden alanın gerçek bir numara istediği sonucuna varıyor. | Karar değişmedi, yeni karar satırı açılmadı. §3.14.6'ya bir cümle eklendi: e-Arşiv teknik kılavuzunun gerçek kişi alıcı için tanımladığı kimlik numarası alanı bu değerle dolar; alanın şemada bulunması numaranın toplanmasını gerektirmez. |
| BULGU-4 | ⚠️ KISMİ | **Boşluk gerçekti, ama önerilen çözüm ürünün katmanını aşar.** `02` ters ibraz (harcama itirazı) için hiçbir şey söylemiyordu. K-13 3D Secure'u chargeback ispat yükü gerekçesiyle seçmiş, sonrasını yazmamıştı. Codex'in saydığı kopuklukların çoğu (satış özeti, dışa aktarma) ürünün dışında kalır: itiraz sağlayıcının ve bankanın sürecidir, sonucu haftalar sürer, bildirim biçimi sağlayıcıdan sağlayıcıya değişir. Ürünün içinde çözülmesi gereken tek gerçek risk **çift ödemedir.** Parası ters ibrazla müşteriye dönmüş kalemde kart iadesi sağlayıcıda reddedilir. Bunun üzerine firma müşteriye havale yolunu açar (§7.2.8; K-569) ve müşteri IBAN girerse para ikinci kez gider. **Alınmayan kol:** *"Ters ibrazı ayrı bir finansal kayıt olarak tanımlayın"*. Ödeme ekseni ve satış özeti sağlayıcının sürecinin ikinci bir kopyasını tutardı. Ayrıca `10 §4.1` ÖK-4'e yeni bir sağlayıcı koşulu eklenirdi. Bu, eksik havalenin sistem dışında yürüdüğü K-169'un kalıbıdır. | **K-611 (öneriyle kaydedildi — ⚠).** Yeni §3.21.11: ters ibraz ürünün dışındadır ve ödeme eksenini değiştirmez. Firma itirazı sağlayıcının panelinde cevaplar; ürünün geri ödeme kaydı ve işlem izi kanıttır. Parası ters ibrazla dönmüş kalem için ürünün içinden ayrıca geri ödeme işlenmez. "Geri ödeme gerçekleşmedi" uyarısı, havale yolunu açmadan önce ters ibrazı kontrol etmeyi hatırlatır (§7.2.8). §5.5'e eksenin ters ibrazda değişmediği yazıldı; yeni 6.2.22 satırı eklendi. Kalan risk: ters ibraz edilen sipariş satış özetinde ödenmiş görünür. |

**Dağılım:** 1 KABUL · 2 KISMİ · 1 RET.

- **BULGU-1:** Daha önce gelmemişti. 2026-10-03 tarihli audit gecikme feshini (m.16/1–3) işlemişti (K-499), m.16/4'ü işlememişti.
- **BULGU-2:** Daha önce gelmemişti; K-511'i audit kendisi açmıştı.
- **BULGU-3:** 7., 14. ve 22. turlarda gelmişti ve hepsi reddedildi. Her turda dayanak değişti: önce belge tutarı, sonra alıcının statüsü, sonra VUK m.232 sınırı, bu tur da teknik kılavuzun şema alanı. Her seferinde §3.14.6'ya, dönüşe yol açan boşluğu kapatan bir cümle eklendi.
- **BULGU-4:** Daha önce gelmemişti.

## 3. Ek bulgular

- **B-7'nin kalem iptalinde gidip gitmediği belirsizdi:** olay sütunu "İptal edildi" diyordu. Bu, siparişin durum adıdır; oysa §7.2.4 her firma iptalinde sebebin müşteriye bildirildiğini söylüyordu. m.16/4'ün bildirimi kalem iptalinde de gerektiği için olay sütunu "sipariş ya da tek bir kalem" olarak açıldı. Kalem iptalinde e-posta artık iptal edilen kalemi de taşıyor. Bu, K-609'un kapsamındadır.
- **Üç günlük sürenin ölçümü:** Ürün firmanın durumu ne zaman öğrendiğini bilemez; süre bu yüzden §12.5'te firmanın yükümlülüğü olarak yazıldı. Panelde ayrıca bir sayaç açılmadı.
- **Önceki turlardan açık kalanlar:** 21. turun "üyenin e-postasını değiştirmesinde teyidin hangi adrese yapılacağı" notu ve 20. turun K-606 hizmet ve dijital kalem notu bu turda da gelmedi; `04` bunları ele almalı.
- **Etki yansıtma için not:** `10 §2` KP-5 ziyaretçinin ürün sayfasında gördüklerini sayar. Yerli üretim logosu eklenmeli (K-610); bu bir çelişki değil, eksik sayımdır. KP-47'nin firma iptali cümlesi m.16/4'e dokunmuyor. `04`'ün tasarlaması gerekenler: "stokta bulunamadı" uyarısı, ürün sayfasında logonun yeri ve "geri ödeme gerçekleşmedi" uyarısının ters ibraz hatırlatması. `08` logoyu ürünle gelen bir varlık olarak bakımına almalı. `12`'nin Mesafeli Satış Sözleşmesi taslağı imkânsızlık maddesini Yönetmelik'in metniyle taşımalı. `01`'de bu turun değiştirdiği cümlelerin karşılığı yok.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`02` v0.39). Üç yeni karar satırı açıldı; üçü de ⚠ ile işaretli. Seçenekleri gerçekten ayrışan ve firmaya ya da müşteriye ciddi para veya hukuki risk yükleyen bir konu çıkmadı. K-609 ürünün mekaniğini değiştirmiyor; yasal yolu yazıyor ve bir uyarı ekliyor. K-610 bir yasal zorunluluğu kapatıyor ve firmaya yük getirmiyor. K-611 bir çift ödeme riskini, ürüne yeni bir durum eklemeden kapatıyor.

- [x] BULGU-1 (kısmi — K-609 ⚠)
- [x] BULGU-2 (kabul — K-610 ⚠)
- [x] BULGU-3 (red — §3.14.6'ya teknik kılavuzun alanı yazıldı)
- [x] BULGU-4 (kısmi — K-611 ⚠)
- ⚠ **K-609:** Firma iptali ifanın imkânsızlaşmasının yasal yolunu karşılar, ayrı bir akış açılmaz. Bildirim kalem iptalinde de gider; üç günlük süre firmanın yükümlülüğüdür. "Stokta bulunamadı" listede kalır, seçildiğinde panel uyarır. Gözden geçirilecek nokta: kısmi iptalde kargo ücretinin alıkonması m.16/4'ün "tüm ödemeler"iyle tartışılabilir (kalan risk olarak yazıldı).
- ⚠ **K-610:** Yerli üretim logosu, üretim yeri Türkiye olan fiziksel üründe kendiliğinden gösterilir. K-511'in "logo firmanın görselidir" cümlesini değiştirir.
- ⚠ **K-611:** Ters ibraz ürünün dışındadır. Parası ters ibrazla dönmüş kalem için ikinci geri ödeme yapılmaz; panel havale yolundan önce kontrolü hatırlatır. Elenen seçenek: ters ibrazı ayrı bir kayıt ve durum olarak izlemek.

**Hukuki kontrol** (2026-10-03):
- **BULGU-1:** Mesafeli Sözleşmeler Yönetmeliği m.16/4 (RG 23/8/2022-31932 ile değişik; mevzuat.gov.tr konsolide metninin bugün indirilmiş kopyasından okundu): *"Sipariş konusu mal ya da hizmet ediminin yerine getirilmesinin imkansızlaştığı hallerde satıcı veya sağlayıcının ... bu durumu öğrendiği tarihten itibaren üç gün içinde tüketiciye yazılı olarak veya kalıcı veri saklayıcısı ile bildirmesi ve varsa teslimat masrafları da dâhil olmak üzere tahsil edilen tüm ödemeleri bildirim tarihinden itibaren en geç on dört gün içinde iade etmesi zorunludur. Malın stokta bulunmaması durumu, mal ediminin yerine getirilmesinin imkânsızlaşması olarak kabul edilmez."* Codex'in okuması doğrudur.
- **BULGU-2:** Fiyat Etiketi Yönetmeliği m.5/2 (mevzuat.gov.tr konsolide metni): etiket ve listelerde bulunması zorunlu hususlar arasında *"e) Üretim yeri Türkiye olan mallar için Bakanlıkça tespit ve ilan edilen şekil, logo veya işaret"*; m.5/8: *"Yerli üretim logosu bulunan etiketlerde ve fiyat listelerinde ayrıca üretim yerinin belirtilmesine gerek yoktur."* Ticaret Bakanlığı'nın yerli üretim logosu duyurusu (ticaret.gov.tr; haber özetlerinden okundu) logonun Türkiye'de üretilen ürünlerin etiketlerinde, tarife ve fiyat listelerinde bulunmasını zorunlu sayar. Yurt dışından ithal edilip Türkiye'de yalnız ambalajlanan ürünü yerli üretim saymaz. Codex'in bağlantısı 6502 sayılı Kanun'un PDF'sine gidiyor, Yönetmeliğe değil. Hüküm Yönetmelik'te doğrulandı.
- **BULGU-3:** GİB e-Arşiv Teknik Kılavuzu V.1.18 (Ağustos 2025; ebelge.gib.gov.tr'den indirildi): §3.3.2.16.2 `gercekKisi` — *"Alıcı, gerçek kişi ise ilgili bilgiler yazılmalıdır. Kullanım: tckn: Vatandaşlık numarası, adiSoyadi: Adı soyadı yazılacaktır."* Kılavuzda tutar sınırı ya da `11111111111`'e dair bir kısıt yok. Gelir İdaresi'nin duyurusu (22. turun okuması; alomaliye.com metni) numarasını paylaşmak istemeyen nihai tüketici için bu alana `11111111111` girilebileceğini söyler. Bugünkü web aramasının sonuçları da aynı uygulamayı aktarır.
- **BULGU-4:** Hukuki iddia yok.
