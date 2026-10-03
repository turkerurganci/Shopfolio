# Cross-Review — 02 Product Requirements (Tur 22)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/02_PRODUCT_REQUIREMENTS.md` v0.37 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde; satır sonlarındaki boşluklar atıldı). İstem önceki turlarınkiyle aynıdır: yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: teknik doğruluk
> Seviye: Yüksek
> Yer: §7.4.1 Geri ödemenin başlangıcı
> Alıntı: "beyan teslim tarihiyle aynı gün yapılmışsa teslimden sonra sayılır"
> Sorun: Müşteri, mal fiilen teslim edilmeden önce aynı gün cayarsa sistem bunu teslim sonrası cayma sayıp geri ödeme süresini malın firmaya ulaşmasına bağlar. Teslim edilmemiş malda süre cayma bildiriminin satıcıya ulaştığı anda başlar; bu kural tüketicinin yasal geri ödeme süresini geciktirir. [Ticaret Bakanlığı rehberi](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)
> Öneri: Teslim gününde beyanın teslimden önce/sonra olduğunu güvenilir biçimde ayıramıyorsa süreyi tüketici lehine beyan tarihinden başlatın; alternatif olarak doğrulanabilir teslim saati kaynağı tanımlayın.
>
> BULGU-2
> Kriter: teknik doğruluk
> Seviye: Yüksek
> Yer: §3.14.6 Adres; §12.1.10 Fatura
> Alıntı: "Numara için tutar eşiği yoktur"
> Sorun: Nihai tüketici için T.C. kimlik numarası hiç toplanmayacak ve `11111111111` her durumda kullanılacak varsayımı, fatura düzenleme haddini aşan e-Arşiv faturalar için gerekli alıcı bilgilerini karşılamaz. 2026 fatura düzenleme haddi 12.000 TL’dir; belge düzenleme ve alıcı bilgisi kuralları tutara göre değerlendirilmelidir. [GİB fatura düzenleme haddi](https://agri.gib.gov.tr/yardim-ve-kaynaklar/yararli-bilgiler/fatura-duzenleme-siniri), [VUK 509 e-Arşiv fatura bilgileri](https://www.lexpera.com.tr/mevzuat/tebligler/vergi-usul-kanunu-genel-tebligi-sira-no-509-2)
> Öneri: Güncel VUK hadlerini izleyen, yalnız gerektiğinde açılan koşullu T.C. kimlik no/alıcı vergi bilgisi akışı tanımlayın; eşik altındaki nihai tüketici için veri toplamama kuralını koruyun.
>
> BULGU-3
> Kriter: eksiklik
> Seviye: Orta
> Yer: §3.20.8 Kargo şirketi
> Alıntı: "Kargo şirketi ürünle gelen bir listeden seçilir"
> Sorun: Kapalı kargo şirketi listesi, her şirketin “Takip et” bağlantı kuralı ve liste güncelleme sahipliği belirtilmemiştir. Buna göre hangi şirketin “Diğer” sayılacağı ve hangi takip bağlantısının üretileceği belirlenemez.
> Öneri: Sözlüğe/listeler bölümüne şirket adlarını, takip bağlantısı şablonlarını, “Diğer” davranışını ve liste güncelleme sorumlusunu ekleyin.```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ⚠️ KISMİ | **Konu ikinci kez geldi (20. tur BULGU-2). Bu kez dönmesinin sebebi metindeki bir eksik değil, kuralın kendisiydi.** §7.4.1 aynı gün yapılan beyanı her durumda teslimden sonra sayıyordu (K-520). 20. tur bu seçimin kalan riskini metne yazdı: mal o gün beyandan sonra teslim edildiyse Mesafeli Sözleşmeler Yönetmeliği m.12/2'nin on dört günü bildirimden işler, panel ise süreyi malın firmaya ulaşmasından sayar. Yani ürünün kendi sayacı bu hâlde yasal süreden geç başlıyordu. Bu, talimattaki "bilinçli karar hukuki hata doğuruyorsa yaz" koşuluna girer. 20. turun ek bulgusu ("firmanın erken ödeme yolu") da açık kalmıştı. Ayrımın bir varsayıma dayanması da gerekmiyor. Sipariş sayfasından gelen beyan teslim işaretinden sonra geldiyse sıra kesindir, çünkü firma teslimi gerçekleştikten sonra işaretler. Öteki hâllerde sırayı bilen taraf firmadır, çünkü kargo şirketinin teslim kaydı saati taşır. **Önerinin iki kolu alınmadı.** (1) *"Süreyi tüketici lehine beyandan başlatın"*: mal çoğu zaman beyandan önce teslim edilmiştir (müşteri malı görüp aynı gün cayar). O durumda firma, malı elinde tutan müşteriye malı görmeden ödemek zorunda kalırdı (K-520'nin gerekçesi). (2) *"Doğrulanabilir teslim saati kaynağı tanımlayın"*: her teslim işaretinde saat istemek, K-288'in geriye dönük işaretine her siparişte bir veri aktarma yükü ekler. Bunun yerine soru yalnız tarihlerin çakıştığı seyrek hâlde sorulur. | **K-608 (öneriyle kaydedildi — ⚠).** §7.4.1'de ayrım artık iki yoldan yapılıyor. Sipariş sayfasından yapılan beyan teslim işaretinden sonra geldiyse teslimden sonra sayılır. Öteki hâllerde panel firmaya teslimin beyandan önce mi sonra mı olduğunu sorar: beyan işaretten önce gelmiş ve teslim tarihi beyanın günü olarak girilmiş ya da o güne düzeltilmişse, ya da başka kanaldan gelen bildirim teslim günü tarihiyle kaydediliyorsa. Panel beyanın saatini gösterir ve cevap zorunludur. "Teslim beyandan sonra oldu" cevabı beyanı teslimden önce sayar ve geri ödeme süresi beyandan işler. Cevap işlem izinde durur; ispat yükü firmadadır. §3.20.11, §5'in kalem kayıtları (cayma beyanı), §10.3.1, §10.4.5, §10.4.10 ve §12.1.9 hizalandı; yeni 6.4.34 satırı eklendi. |
| BULGU-2 | ❌ RET | **Doküman doğru; aynı itiraz üçüncü kez geldi (7. ve 14. turlar).** Gelir İdaresi'nin e-Arşiv duyurusu, vergi mükellefi olmayan nihai tüketici numarasını paylaşmak istemediğinde alıcı alanına `11111111111` yazılabileceğini söyler ve bir tutar sınırı koymaz. Codex'in gösterdiği 12.000 TL, Vergi Usul Kanunu m.232'nin fatura düzenleme sınırıdır. Bu sınır faturanın düzenlenip düzenlenmeyeceğini belirler, alıcının kimlik numarasını değil. Bulgu, sınırı aşan faturada numaranın gerektiğini söyleyen bir hüküm göstermiyor; VUK 509 Sıra No.lu Genel Tebliğ bağlantısı da böyle bir kural göstermiyor (7. ve 14. turların okuması). Karar K-561'dir ve 2026-10-03'te kontrol edildi. **Konunun dönmesinin sebebi metindeki bir belirsizlikti:** §3.14.6 *"mevzuattaki tutar eşikleri faturanın e-Arşiv olarak düzenlenme zorunluluğuna aittir"* diyordu. Bu cümle VUK m.232'nin fatura düzenleme sınırını saymıyordu; 12.000 TL'yi gören okur bu yüzden bir boşluk buldu. | Karar değişmedi, yeni karar satırı açılmadı. §3.14.6'da eşikler adıyla yazıldı: VUK m.232'nin her yıl güncellenen fatura düzenleme sınırı ve e-Arşiv'e geçiş eşikleri, faturanın düzenlenip düzenlenmeyeceğine ve biçimine aittir, alıcının numarasına değil. Gelir İdaresi'nin duyurusunun `11111111111` için tutar sınırı koymadığı da eklendi. Rakam yazılmadı, çünkü her yıl değişir. |
| BULGU-3 | ⚠️ KISMİ | **Önerinin çoğu yanlış katmanda, ama bir boşluk gerçekti.** Codex'in eksik dediği dört şeyden ikisi zaten yazılıydı. "Diğer"in davranışı §3.20.8'deydi: listede olmayan şirket "Diğer" seçilip adıyla yazılır ve yalnız ad ile takip numarası görünür. Güncellemenin kimde olduğu da yazılıydı: liste ürünün kendi varlığıdır ve firma panelden düzenlemez (K-142). Şirket adlarını ve takip bağlantısı kalıplarını `02`'ye yazmak alınmadı. Bunlar bakım verisidir, bir kargo şirketi takip sayfasını değiştirdiğinde değişir ve Aşama 1'in ürün gereksinimine girmez; K-142 bunları açıkça `08`'e devretmişti. **Gerçek boşluk:** bu devir ve bakımın sonucu `02`'de yazılı değildi. Okur listenin içeriğinin nerede tanımlanacağını göremiyordu. | Karar değişmedi. §3.20.8'e üç şey yazıldı: listede olmayan her şirket "Diğer"dir; listenin içeriği ve takip bağlantısı kalıpları `08`'in işidir (K-142'nin devri); güncellenmesi ürünün bakımıdır (K-18). Bir şirket takip sayfasının adresini değiştirdiğinde güncelleme gelene kadar bağlantı kırılır, takip numarası görünmeye devam eder. |

**Dağılım:** 0 KABUL · 2 KISMİ · 1 RET.

- **BULGU-1:** 20. turda gelmişti (BULGU-2). O tur kalan riski metne yazdı, kararı değiştirmedi. Bu tur değiştirdi: kalan risk ürünün kendi sayacının yasal süreden geç başlamasıydı ve sırayı bilen taraf olan firmaya sormak bunu ucuza kapatıyor.
- **BULGU-2:** 7. ve 14. turlarda gelmişti; ikisi de reddedildi ve her seferinde §3.14.6 biraz daha açıldı. Bu tur da red; kalan belirsizlik (hangi eşik) giderildi.
- **BULGU-3:** Daha önce gelmemişti.

## 3. Ek bulgular

- **Teslim işaretinden sonra gelen beyanın kesinliği:** K-608'in birinci yolu "firma teslimi gerçekleştikten sonra işaretler" varsayımına dayanır. Firma malı teslim edilmeden işaretlerse (ör. kargo "dağıtımda" iken) sıra yanlış olur. Bu, K-288'in genel kuralıyla aynı risktir: yanlış ya da erken işaretin sonucu firmadadır. Yeni bir kural eklenmedi.
- **20. turun açık notu kapandı:** "Aynı gün caymada firmanın erken ödeme yolu" ayrı bir yol açılmadan çözüldü. Teslim beyandan sonraysa beyan artık teslimden önce sayılır ve kalem teslimden önceki caymanın hattına girer: sayaç beyandan işler.
- **Önceki turlardan açık kalanlar:** 21. turun "üyenin e-postasını değiştirmesinde teyidin hangi adrese yapılacağı" notu ve 20. turun K-606 hizmet ve dijital kalem notu bu turda gelmedi; `04` bunları ele almalı.
- **Etki yansıtma için not:** `04` teslim işaretinde, tarih düzeltmesinde ve başka kanaldan gelen bildirimin kaydında sıra sorusunu tasarlamalı ve firmaya beyanın saatini göstermeli. `06` cevabı cayma kaydının bir alanı olarak taşımalı. `12` Ön Bilgilendirme Formu taslağı Yönetmelik'in ayrımını (teslimden önce/sonra) aynen taşır; teslim günü caymasını ayrıca anlatması gerekmez. `10` ve `01`'de bu turun değiştirdiği cümlelerin karşılığı yok. Kargo listesinin içeriği `08`'in açık işidir (K-142).

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`02` v0.38). Bir yeni karar satırı açıldı ve ⚠ ile işaretli. Seçenekleri gerçekten ayrışan ve firmaya ya da müşteriye ciddi para veya hukuki risk yükleyen bir konu çıkmadı: K-608 bir yasal süre riskini kapatır, firmaya yalnız tarihlerin çakıştığı seyrek hâlde bir soru ekler ve malı elinde tutan müşteriye erken ödemeyi gerektirmez.

- [x] BULGU-1 (kısmi — K-608 ⚠)
- [x] BULGU-2 (red — §3.14.6'da eşikler adıyla yazıldı)
- [x] BULGU-3 (kısmi — §3.20.8'e `08`'e devir ve bakımın sonucu yazıldı)
- ⚠ **K-608:** Teslim günü yapılan cayma beyanı, sipariş sayfasından ve teslim işaretinden sonra geldiyse teslimden sonra sayılır. Öteki hâllerde panel firmaya teslimin beyandan önce mi sonra mı olduğunu sorar ve cevap zorunludur. Gözden geçirilecek nokta şu: bu, K-520'nin "aynı gün her zaman teslimden sonra" kuralını daraltır ve teslim işaretine bir soru ekler. Elenen seçenekler: aynı günü her zaman teslimden önce saymak (firma malı görmeden öder) ve her teslimde saat istemek (her siparişte yük).

**Hukuki kontrol** (2026-10-03):
- **BULGU-1:** Mesafeli Sözleşmeler Yönetmeliği m.12/2 (mevzuat.gov.tr konsolide metni; bugün indirilmiş kopyadan okundu, sitenin bağlantısı bu oturumda 404 döndü): *"Malın tesliminden önce cayma hakkının kullanılması durumunda satıcı ... cayma hakkının kullanıldığına ilişkin bildirimin kendisine ulaştığı tarihten itibaren on dört gün içinde ... tahsil edilen tüm ödemeleri iade etmekle yükümlüdür."* m.12/1, malın *"iade için öngörülenin haricinde bir taşıyıcı ile"* iadesinde süreyi *"malın satıcıya ulaştığı tarihten itibaren"* başlatır. Ayrım fiilî teslime bağlıdır; Codex'in okuması doğrudur. K-608 süreyi fiilî sıraya bağlar.
- **BULGU-2:** Gelir İdaresi Başkanlığı'nın e-Arşiv fatura düzenleyen mükelleflere duyurusu (1 Ağustos 2018; alomaliye.com'daki metninden okundu): *"Vergi mükellefi olmayan nihai tüketici mahiyetindeki müşterilerin T.C. Kimlik Numarası bilgilerinin paylaşılmak istenmediği hallerde, e-Arşiv Faturalarında alıcı hesap numarası alanına '11111111111' girilebilecektir."* Duyuru tutar sınırı koymaz. Fatura düzenleme sınırına ilişkin bilgi notu da (alomaliye.com, 2023) şöyle der: *"Vergi mükellefi olmayan nihai tüketicilere düzenlenecek faturalarda vergi dairesi ve vergi kimlik numarası veya T.C. Kimlik Numarası bilgilerine yer verilmesi zorunluluğu bulunmamaktadır."* 2026'nın fatura düzenleme sınırı (VUK m.232) 12.000 TL'dir (31/12/2025 tarihli Resmî Gazete'de yayımlanan tebliğ; muhasebetr.com). Bu sınır faturanın düzenlenmesine aittir, alıcının numarasına değil. Duyurunun resmî GİB sayfası bu oturumda da açılmadı, ikincil kaynaktan okundu.
- **BULGU-3:** Hukuki iddia yok.
