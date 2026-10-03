# Cross-Review — 02 Product Requirements (Tur 20)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/02_PRODUCT_REQUIREMENTS.md` v0.35 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde; satır sonlarındaki boşluklar atıldı). İstem önceki turlarınkiyle aynıdır: yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi. Codex bu turda Yönetmelik metni için web araması yaptı ve iki bulguya kaynak bağlantısı ekledi; bağlantılar ham çıktının parçasıdır.

> ```text
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §7.3.1 — Cayma; §4.2 Z-13
> Alıntı: "Cayma penceresi on dört gündür ve sabittir; firma uzatamaz"
> Sorun: Cayma hakkında gereği gibi bilgilendirme yapılmadığında tüketici 14 günlük süreyle bağlı değildir; süre en fazla bir yıl uzar, sonradan doğru bilgilendirme yapılırsa yeniden 14 gün işler. Doküman cayma düğmesini koşulsuz kapattığı için bu yasal istisnayı işletmez. [Mesafeli Sözleşmeler Yönetmeliği m.10](https://tuketici.ticaret.gov.tr/data/5e819a8e13b876a1b04c7a42/Mesafeli%20S%C3%B6zle%C5%9Fmeler%20Y%C3%B6netmeli%C4%9Fi.pdf)
> Öneri: Bilgilendirmenin eksik/geçersiz olduğu hâllerde cayma süresini mevzuattaki uzatılmış kuralla hesaplayan ayrı bir istisna kuralı ekleyin; düğme ve panel işlemleri bu süreyi izlesin.
>
> BULGU-2
> Kriter: Edge case
> Seviye: Yüksek
> Yer: §7.4.1 — Geri ödemenin başlangıcı
> Alıntı: "beyan teslim tarihiyle aynı gün yapılmışsa teslimden sonra sayılır"
> Sorun: Sistem yalnız tarih tuttuğu hâlde aynı gün içindeki olay sırasını kesin kabul ediyor. Tüketici sabah caymış, mal öğleden sonra teslim edilmiş olabilir; bu durumda beyan teslimden öncedir. Kural bunu teslim sonrası sayarak geri ödeme başlangıcını malın firmaya ulaşmasına kadar geciktirebilir. Yönetmelik ayrımı takvim gününe değil fiilî teslim anına bağlar. [Mesafeli Sözleşmeler Yönetmeliği m.9](https://tuketici.ticaret.gov.tr/data/5e819a8e13b876a1b04c7a42/Mesafeli%20S%C3%B6zle%C5%9Fmeler%20Y%C3%B6netmeli%C4%9Fi.pdf)
> Öneri: Teslim ve cayma için zaman damgası tutun; bu mümkün değilse aynı gün vakalarını tüketici lehine teslimden önce yapılmış kabul edin veya firma tarafından kanıtlanabilir fiilî teslim sırasını esas alın.
>
> BULGU-3
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §4.1.5 — Süre çitleri
> Alıntı: "araya bir bayram girse bile otuz takvim gününe sığar"
> Sorun: En yüksek havale süresi (3 iş günü) ile en yüksek kargoya verme süresi (10 iş günü), hafta sonları ve uzun resmî tatiller nedeniyle tek başına teslim için anlamlı bir takvim günü marjı bırakmaz; taşıma süresi de ürün dışında ve sınırsızdır. Aynı madde sonradan çitin 30 günü güvence altına almadığını söylüyor; gerekçe ve sonuç birbiriyle çelişiyor.
> Öneri: Çitin 30 günlük yasal teslim sınırını karşıladığı iddiasını kaldırın; sipariş anından başlayan toplam takvim günü hesabıyla izlenen ayrı bir teslim-risk kuralı tanımlayın ve kargoya verme vaadini buna göre sınırlandırın.
>
> SONUÇ: 3 BULGU```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ⚠️ KISMİ | **Hukuki boşluk gerçekti; önerilen çözüm alınmadı.** Yönetmelik m.10 cayma hakkı konusunda gerektiği şekilde bilgilendirilmeyen tüketicinin on dört günlük süreyle bağlı olmadığını, sürenin en geç pencerenin bitiminden bir yıl sonra sona erdiğini söyler. §7.3.1 pencereyi koşulsuz *"sabittir"* diye yazıyordu. Karar kaydında m.10'u ele alan bir satır yok: K-203'ün *"firma uzatamaz"* kuralı firmanın gönüllü uzatmasıyla ilgilidir (gerekçesi sözleşme metninin sürümlenmesidir), yasal uzamayla değil. Ürün cayma bilgisini her siparişte Ön Bilgilendirme Formu'nda ve beyan ekranında verir (§3.24.3; K-191), ama bilgilendirme yine eksik kalabilir: firma bir ürünü Yönetmelik'te karşılığı olmayan bir istisna sebebiyle işaretleyebilir (K-206, K-509). O zaman tüketiciye "cayma hakkınız yok" denmiştir. Bu hâlde ürünün hiçbir yolu pencere dışındaki bir caymayı kaydetmiyordu: düğme kapalıydı, §10.4.10'un kaydı da pencereyi okuyordu. **Codex'in önerisi ürünün süreyi "uzatılmış kuralla hesaplaması"ydı. Bu alınmadı:** bilgilendirmenin "gerektiği şekilde" olup olmadığı bir hukuki değerlendirmedir ve ürünün elindeki veriden okunamaz. Kendiliğinden bir yıl açık kalan bir düğme ise doğru bilgilendirilmiş her müşteriye yasal olmayan bir hak verirdi. | **K-606 (öneriyle kaydedildi — ⚠).** §7.3.1'e m.10'un kuralı ve ürünün buna yaklaşımı yazıldı. Ürün pencereyi on dört gün saymaya devam eder ve kapanmış düğmeyi açmaz. Firma bildirimi §10.4.10'un kaydıyla alır: kayıt pencere kapandıktan sonra ve mutlak istisna işaretli kalemde de yapılabilir. Panel bu durumu söyler ve kaydı ayrıca onaylatır. Pencerenin bitiminden bir yıldan sonraya tarihli kaydı reddeder. Bilgilendirmenin ispat yükü satıcıdadır ve ürünün aracı siparişe donan form ile gönderim kaydıdır (§3.23.3, Z-43). Z-13'e, §12.1.9'a ve yeni 6.4.32 satırına aynı kural kısaca yazıldı. |
| BULGU-2 | ⚠️ KISMİ | **Karar bilinçliydi, ama dokümanda bilinçli olarak yazılı değildi.** K-520 aynı gün yapılan beyanı teslimden sonra sayar ve kalan riski kayda geçirmiştir: *"mal o gün beyandan sonra teslim edildiyse ayrım müşteri aleyhine olabilir"*. *"Aynı gün teslimden önce sayılır"* seçeneğini de elemiştir: malı elinde tutan müşteride firma malı görmeden ödemek zorunda kalırdı. §7.4.1 ise yalnız *"mal o gün müşteridedir"* diyordu. Kalan risk ve elenen seçenek metinde yoktu. Bu yüzden Codex kuralı farkında olunmadan yapılmış bir varsayım olarak okudu. Codex'in hukuki okuması doğrudur: beyan teslimden önceyse Yönetmelik m.12/2'nin on dört günü bildirimden işler. **Önerinin iki kolu alınmadı.** (1) *"Teslim için zaman damgası tutun"*: teslim tarihi gün olarak ve firmanın elle, çoğu zaman geriye dönük girdiği bir tarihtir (§3.20.11; K-288). Saat alanı firmaya kargo kaydından ayrıca bir veri aktarmayı yükler. Saat girilmediğinde ayrım yine bir varsayıma döner. (2) *"Aynı günü tüketici lehine teslimden önce sayın"*: K-520'nin eleme gerekçesi aynen işler. | Karar değişmedi (K-520). §7.4.1'e kuralın bilinçli olduğu, kalan riski ve iki elenen seçenek yazıldı. Kalan risk şöyle yazıldı: mal o gün beyandan sonra teslim edildiyse yasal süre bildirimden işler, panel süreyi malın ulaşmasından sayar ve aradaki farkın riski firmadadır. |
| BULGU-3 | ❌ RET | **Hesap doğrudur ve çelişki yoktur.** En uzun ayarlarla havale ödeme süresi (P-7 üst çiti üç iş günü) ve kargoya verme süresi (P-5 üst çiti on iş günü) birlikte on üç iş günüdür. Cuma verilen bir siparişte bu en çok on dokuz takvim günü eder. Araya dört–beş iş gününe denk gelen bir bayram tatili girerse yirmi beş–yirmi altı takvim günü eder ve taşımaya dört–beş gün kalır. Türkiye içinde kargonun olağan süresi bu aralığa sığar. Yani *"araya bir bayram girse bile otuz takvim gününe sığar"* cümlesi hesap olarak doğrudur. Codex'in *"anlamlı bir takvim günü marjı bırakmaz"* iddiası bu hesapla tutmaz. *"Gerekçe ve sonuç birbiriyle çelişiyor"* iddiası da tutmaz: birinci cümle olağan taşımayla yapılmış bir hesaptır, ikinci cümle taşıma ürünün dışında olduğu için bunun bir güvence olmadığını söyler. Aşım durumu da tanımlıdır: gecikme feshi açılır (§7.2.3, Z-11; K-499). Önerinin *"ayrı bir teslim-risk kuralı"* kolu 17. turda reddedildi: kargoya verdikten sonra firmanın paketi hızlandıracak bir yolu yoktur ve kargoya vermeden önce firmanın kendi sözü otuz günden çok önce dolar. **Konu 17. turun BULGU-2'sinden sonra ikinci kez döndü.** Sebebi metindeki belirsizlikti: hesap sayısız yazılmıştı ve "sığar" ile "güvence altına almaz" yan yana bir iddia ve onun reddi gibi okunuyordu. | Karar değişmedi (K-448). Konunun yeniden dönmemesi için §4.1.5'e hesap açık sayılarla yazıldı: on üç iş günü, en çok on dokuz takvim günü, bayramla yirmi beş–yirmi altı takvim günü, taşımaya dört–beş gün. Ardından "Bu bir hesap, güvence değildir" cümlesi geliyor. |

**Dağılım:** 0 KABUL · 2 KISMİ · 1 RET.

- **BULGU-1:** Daha önce gelmemişti. Yönetmelik m.10 önceki turlarda yalnız bilgilendirmenin ispatı için anılmıştı (13. tur, Z-43). Sürenin uzaması ilk kez geldi.
- **BULGU-2:** Daha önce cross-review'da gelmemişti. Konu deep review'da bulunmuş ve K-520 ile karara bağlanmıştı. Dönmesinin sebebi kararın kalan riskinin metne taşınmamasıydı.
- **BULGU-3:** 17. turun BULGU-2'siyle aynı konudur. O turda gerekçe cümlesi düzeltilmişti, ama hesap sayısız kaldı. Bu tur sayılar yazıldı.
- **KABUL yok:** Üç bulgunun ikisinde sorun gerçekti, ama önerilen çözüm uygun değildi. Üçüncüsünde iddia hesapla çürüdü. Her biri karar kaydına ve Yönetmelik'in konsolide metnine karşı kontrol edildi.

## 3. Ek bulgular

- **Aynı gün caymada firmanın erken ödeme yolu:** K-520'nin kalan riski, firmanın teslimin beyandan sonra olduğunu bildiği durumda geri ödemeyi mal ulaşmadan yapmasını gerektirebilir. §5.5 caymanın geri ödemesini firmanın panelden işlediğini söyler, ama teslimden sonraki caymada bu adımın malın teslim alınmasından önce açık olup olmadığını yazmaz. Bu tura metin eklenmedi; `04` ve `06` geri ödeme adımının koşulunu tanımlarken bu durumu hesaba katmalı.
- **K-606'nın hizmet ve dijital kalemi:** Kayıt "pencere kapandıktan sonra" diyor. Hizmet kaleminde hak tamamlanma işaretiyle, dijital kalemde ödeme onayıyla düşer ve ikisinin de onay kutusu siparişe donar (§3.24.6). Bu kalemlerde bilgilendirmenin eksik kalması olağan bir durum değildir, ama kayıt onları dışlamaz; değerlendirme yine firmanındır.
- **Önceki turlardan açık kalanlar:** 19. turun ek bulguları bu turda gelmedi: ödenmemiş siparişte kargodan önce gelen cayma bildirimi (K-505 ile sipariş bütünüyle iptal) ve K-537'nin geçiş listesindeki S11 farkı. 18. turun ek bulguları da gelmedi: kuponlu çok oranlı siparişte kargo ücretinin bölünmesi ve K-180'in kendiliğinden iptalinde B-7.
- **Etki yansıtma için not:** `10` KP-47'nin (K-510) satırı K-606'nın genişlemesini taşımalı: kayıt pencere kapandıktan sonra ve mutlak istisna işaretli kalemde de yapılabilir. `04` panelde pencere dışı ve istisnalı kalem uyarısını ve ayrı onayı tasarlamalı. `12` Ön Bilgilendirme Formu taslağında m.10'u anmalı. `01`'de bu turun değiştirdiği cümlelerin karşılığı yok.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`02` v0.36). Bir yeni karar satırı açıldı ve ⚠ ile işaretli. Seçenekleri gerçekten ayrışan ve firmaya ya da müşteriye ciddi para veya hukuki risk yükleyen bir konu çıkmadı: K-606 firmaya yeni bir yükümlülük getirmez, var olan yasal yükümlülüğü yerine getirmesinin yolunu açar.

- [x] BULGU-1 (kısmi — K-606 ⚠) · [x] BULGU-2 (kısmi; karar değişmedi, §7.4.1'e kalan risk yazıldı) · [x] BULGU-3 (ret; §4.1.5'e hesap açık sayılarla yazıldı)
- ⚠ **K-606:** Eksik bilgilendirmede firma cayma bildirimini pencere kapandıktan sonra ve mutlak istisna işaretli kalemde de kaydedebilir. Panel uyarır, ayrı onay ister ve bir yıldan sonrasını reddeder. Gözden geçirilecek nokta şu: bilgilendirmenin eksik kalıp kalmadığına ürün değil firma karar verir. Elenen seçenek, ürünün süreyi kendiliğinden uzatmasıydı; bu, doğru bilgilendirilmiş her müşteriye yasal olmayan bir hak verirdi.

**Hukuki kontrol** (2026-10-03, Mesafeli Sözleşmeler Yönetmeliği'nin mevzuat.gov.tr konsolide metni):
- **BULGU-1:** m.10/1: *"Satıcı veya sağlayıcı ile aracı hizmet sağlayıcı, cayma hakkı konusunda tüketicinin bilgilendirildiğini ispat etmekle yükümlüdür. Tüketici, cayma hakkı konusunda gerektiği şekilde bilgilendirilmezse, cayma hakkını kullanmak için on dört günlük süreyle bağlı değildir. Bu süre her halükarda cayma süresinin bittiği tarihten itibaren bir yıl sonra sona erer."* m.10/2: *"Cayma hakkı konusunda gerektiği şekilde bilgilendirmenin bir yıllık süre içinde yapılması halinde, on dört günlük cayma hakkı süresi, bu bilgilendirmenin gereği gibi yapıldığı günden itibaren işlemeye başlar."* 6502 sayılı Kanun m.48/4 aynı kuralı taşır. §7.3.1'in yeni metni bu iki fıkrayı aktarır.
- **BULGU-2:** m.12/1: satıcı geri ödemeyi malın iade için belirtilen taşıyıcıya teslim edildiği tarihten, başka bir taşıyıcıyla iadede *"malın satıcıya ulaştığı tarihten itibaren"* on dört gün içinde yapar. m.12/2: *"Malın tesliminden önce cayma hakkının kullanılması durumunda ... cayma hakkının kullanıldığına ilişkin bildirimin kendisine ulaştığı tarihten itibaren on dört gün içinde"*. Ayrım fiilî teslime bağlıdır; Codex'in okuması doğrudur. K-520 bunu kalan risk olarak kabul etmişti; §7.4.1 artık aynısını yazıyor.
- **BULGU-3:** m.16/1: *"Satıcı veya sağlayıcı, tüketicinin siparişinin kendisine ulaştığı tarihten itibaren taahhüt ettiği süre içinde edimini yerine getirmek zorundadır. Tüketicinin isteği veya kişisel ihtiyaçları doğrultusunda hazırlanan mallara ilişkin sözleşmeler haricinde mal satışlarında bu süre her halükarda otuz günü geçemez."* §4.1.5'in hesabı otuz günü sipariş onayından sayar ve buna uygundur.
