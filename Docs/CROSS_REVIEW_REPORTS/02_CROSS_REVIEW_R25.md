# Cross-Review — 02 Product Requirements (Tur 25)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/02_PRODUCT_REQUIREMENTS.md` v0.40 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde; satır sonlarındaki boşluklar atıldı). İstem önceki turlarınkiyle aynıdır: yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi. BULGU-3'ün kriteri ("Uyumluluk") yedi kriterin dışındadır; model öyle yazdı, değerlendirmede güvenlik ve eksiklik ölçüsüyle okundu.

> ```text
> BULGU-1
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §8.2.2, §8.2 L-7/L-8, §3.17.8
> Alıntı: "misafir siparişi yalnız IP ekseninde sayılır"
> Sorun: IP değiştiren saldırgan, misafir havale siparişleriyle L-7 ve L-8’i aşmadan aynı varyantın stokunu, hizmet kontenjanını veya kupon hakkını çok sayıda ödenmemiş siparişte kilitleyebilir. Dağıtık saldırıda IP limiti yeterli değildir.
> Öneri: Misafir havale rezervasyonları için IP’den bağımsız bir kötüye kullanım çiti ekleyin: birinci taraf tarayıcı sinyali ve varyant düzeyinde toplam bekleyen ayırma baskısı eşiği; eşik aşılınca ek havale rezervasyonunu reddetme veya ek doğrulama kuralı tanımlayın.
>
> BULGU-2
> Kriter: Edge case
> Seviye: Orta
> Yer: §6.2.15, §3.17.8, §8.2 L-7
> Alıntı: "bu kart ödemesinin başarısızlığıdır"
> Sorun: Ödeme sağlayıcısına erişilememesi nedeniyle ödeme sayfası hiç açılamayan sipariş, kart başarısızlığı olarak kapanır. Bu siparişler L-7’ye sayıldığından, sağlayıcı kesintisi yaşayan gerçek müşteri beş deneme sonunda 24 saat yeni sipariş onaylayamaz.
> Öneri: Sağlayıcı erişilemezliği/oturum başlatılamaması ile banka veya 3D Secure kaynaklı ödeme reddini ayrı nedenler olarak kaydedin; altyapı kaynaklı kapanışları L-7 sayacından hariç tutun.
>
> BULGU-3
> Kriter: Uyumluluk
> Seviye: Orta
> Yer: §3.33.5, §7.2.3
> Alıntı: "IBAN'ın girildiği ekranlar (iptal ve cayma beyanı, sipariş sayfasındaki IBAN alanı)"
> Sorun: Gecikme feshi sırasında da müşteriden IBAN alınması öngörülüyor; ancak aydınlatma metni bağlantısının zorunlu olduğu ekranlar arasında gecikme feshi sayılmıyor. Dokümanın kendi “yeni kişisel veri alanı açan her ekran” kuralı eksik kalıyor.
> Öneri: §3.33.5’e gecikme feshi IBAN alanını açıkça ekleyin; §7.2.3’te bu ekranda aydınlatma metni bağlantısının gösterileceğini belirtin.
>
> BULGU-4
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §1.2 Ödeme, §5.5, §6.2.16, §4.2 Z-28
> Alıntı: "sipariş İptal edildi ve Başarısız kalır"
> Sorun: “Başarısız” ödeme alınmadan kapanan sipariş olarak tanımlanırken, geç gelen kart ödemesinde para tahsil edilip otomatik iade edilmesine rağmen sipariş Başarısız kalıyor ve on yıllık ticari saklamaya giriyor. Bu, ödeme durumu, finansal kayıt, dışa aktarma ve raporların hangi anlamı taşıdığını belirsizleştiriyor.
> Öneri: “Başarısız” tanımını sipariş ödemesinin başarıyla sonuçlanmadığı biçiminde daraltın; geç tahsilat ve otomatik iadeyi ayrı, zorunlu alanları tanımlı bir finansal olay kaydı olarak modelleyin ve raporlama/dışa aktarma kurallarını buna bağlayın.
> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ⚠️ KISMİ | **Risk gerçek ve metinde adı yoktu.** L-8 IP başına sayar. Misafir siparişi e-posta ekseninde sayılmaz (§8.2.2; K-579). Çok sayıda IP kullanan biri misafir havale siparişleriyle bir ürünün stokunu, hizmet kontenjanını ya da kuponun son haklarını yine kilitleyebilir. §8.2.2 yalnız paylaşılan IP'nin kalan riskini yazıyordu; IP değiştiren saldırganı hiç anmıyordu. Firmanın elinde araç da vardı: ödenmemiş siparişi firma iptaliyle kapatabilir (§3.17.8, §7.2.4) ve havaleyi panelden kapatabilir (§3.21.5). Ama metin bunu saldırıya karşı yol olarak bağlamıyordu. **Alınmayan kollar:** (1) *"Birinci taraf tarayıcı sinyali"*. İm silinir; kararlı saldırganı durdurmaz, yalnız sıradan kullanıcıyı sayar. (2) *"Varyant düzeyinde bekleyen ayırma eşiği; aşılınca ek havale rezervasyonunu reddetme"*. Saldırgan eşiği doldurarak o ürünün havale yolunu gerçek müşterilerin hepsine kapatırdı. Saldırının bedeli saldırgandan müşteriye geçer, üstelik yeni bir parametre açılırdı. (3) *"Ek doğrulama"*. Üçüncü taraf captcha K-304 ile elenmiştir (§8.3.3). Kalan risk dar ve görünürdür: kilit havale ödeme süresiyle (Z-8) sınırlıdır, saldırı panelde açık sipariş olarak görünür ve firma tek adımla çözer. | **K-613 (öneriyle kaydedildi).** §8.2.3'e "Kalan risk — IP değiştiren saldırgan" yazıldı. Ürün bunu kendiliğinden durdurmaz. Firma açık ödenmemiş siparişleri görür, şüpheli olanları firma iptaliyle ("diğer" sebebi, açıklamayla) kapatır ve gerekirse havaleyi geçici olarak kapatır; kart yolu açık kalır. Elenen iki seçenek gerekçesiyle yazıldı; §8'in kaynak satırına K-613 girdi. |
| BULGU-2 | ✅ KABUL | **Doğru ve dokümanın kendi içinden çıkıyor.** 6.2.15 şunu söylüyordu: *"Sipariş onaylandıktan sonra sağlayıcıya bağlanılamaz ve müşteri ödeme sayfasına hiç gidemezse bu kart ödemesinin başarısızlığıdır"*. §3.17.8 ve L-7 de kart başarısızlığıyla kendiliğinden iptal edilen siparişi sayıyordu. Sağlayıcı kesintisinde art arda deneyen gerçek müşteri beş denemede bir gün sipariş onaylayamaz hâle gelirdi. IP ekseninde bu, paylaşılan bir IP'nin arkasındaki bütün müşterilere yayılırdı (K-579'un kalan riski). Kart yönteminin "şu an kullanılamıyor" görünmesi (K-231) bu boşluğu kapatmaz: kesinti tespit edilene kadar geçen aralık tam olarak bu durumdur. **Muafiyetin kötüye kullanım kapısı yoktur.** Bağlantı hatası ürünle sağlayıcı arasındadır ve kullanıcı onu tetikleyemez. Sipariş hiçbir şeyi kilitlemeden kapanır, yani L-7'nin ölçtüğü tekrar doğmaz. **Daraltılan kol:** bulgu *"altyapı kaynaklı kapanışları"* genel olarak hariç tutmak istiyordu. Ödeme sayfasına gidilmiş siparişte başarısızlığın kaynağını (banka mı, sağlayıcı mı) ürün güvenilir biçimde ayıramaz. Kural, ürünün kendisinin bildiği tek ayrıma bağlandı: ödeme sayfası açıldı mı. | **K-614 (öneriyle kaydedildi — ⚠; K-332'nin sayımını daraltır).** Ödeme sayfası hiç açılmamış siparişin iptali L-7'ye sayılmaz. Sipariş yine İptal edildi ve Başarısız olur, listede görünmez; yalnız sayaca girmez. §3.17.8, 6.2.15, 6.2.19, §8.2 L-7 satırı ve §8.2.3 hizalandı. §3, §6 ve §8'in kaynak satırlarına K-614 girdi. P-37'nin değeri değişmedi. |
| BULGU-3 | ✅ KABUL | **Doğru.** §7.2.3 ve §7.4.5 havale hattında IBAN'ın gecikme feshi anında müşteriden alındığını söylüyor (K-499). §3.33.5'in listesi ise yalnız *"iptal ve cayma beyanı"* ile sipariş sayfasındaki IBAN alanını sayıyordu. Kural "yeni bir kişisel veri alanı açan her ekran" diye kurulduğu için (K-553) liste eksik kalmıştı. 6.4.7 de aynı eksikliği taşıyordu. KVKK m.10 aydınlatmayı kişisel verinin elde edildiği anda ister. | §3.33.5'in listesine gecikme feshi eklendi (§7.2.3, §7.4.5 bağlantısıyla). 6.4.7 hizalandı ve kaynak sütununa K-499 girdi. §7.2.3'e ayrı bir cümle eklenmedi: aydınlatmanın gösterim noktalarının tek evi §3.33.5'tir. Yeni karar satırı açılmadı; K-553'ün uygulamasıdır. |
| BULGU-4 | ❌ RET | **Doküman doğru; dört konunun dördü de metinde yazılı.** Bulgu, geç gelen kart ödemesinde Başarısız durumunun, finansal kaydın, dışa aktarmanın ve raporların ne anlama geldiğinin belirsiz olduğunu söylüyor. Metin bunların her birini cevaplıyor (§6.2.16; K-233, K-589). (1) **Durum:** sipariş İptal edildi ve Başarısız kalır; Başarısız siparişin ödemesinin alınmadan kapandığını söyler; geç gelen para siparişe işlenmez, siparişin ödemesi olmaz (§5.5, K-183). (2) **Finansal kayıt:** *"Geç gelen ödeme ve geri ödemesi siparişin ödeme kaydında tarih ve tutarla durur"*. Bulgunun istediği "ayrı finansal olay kaydı" budur. (3) **Dışa aktarma:** kayıt *"siparişlerin dışa aktarmasında (§10.7.1) sipariş satırıyla çıkar"*. (4) **Raporlar:** sipariş *"satış özetine ve ödeme tamamlama oranına ödenmiş sipariş olarak girmez"*. Saklama Z-28 ve Z-41'de açıkça ayrılmıştır. Tanımı daraltmak (*"ödemesi başarıyla sonuçlanmadı"*) bir şey değiştirmez, çünkü ödeme kaydındaki para siparişin ödemesi değildir. Bu, 11. turda K-589 ile kapanan konunun ikinci kez gelmesidir. **Dönüşün sebebi metindeki bir boşluktu:** sözlükteki ve §5.5'teki Başarısız tanımı *"sonradan gelen ödeme işlenmez"* deyip duruyordu. Geç paranın nereye gittiğini söyleyen §6.2.16'ya bağlanmıyordu. | Karar değişmedi, yeni karar satırı açılmadı. Sözlükteki Başarısız tanımına şu yazıldı: geç gelen kart ödemesini sistem kendiliğinden geri öder, ödeme ve geri ödemesi siparişin ödeme kaydında durur, durum Başarısız kalır ve sipariş on yıllık süreyi izler (§6.2.16, Z-28). §5.5 tablosundaki satır da aynı biçimde §6.2.16'ya bağlandı. §5.5'in kaynak satırına K-233 ve K-589 girdi. |

**Dağılım:** 2 KABUL · 1 KISMİ · 1 RET.

- **BULGU-1:** Daha önce bu biçimde gelmemişti. L-8 K-524 ile deep review'da, misafirin e-posta ekseninden çıkarılması K-579 ile 6. turda girdi. IP değiştiren saldırgan o gün K-579'un gerekçesinde *"onu IP ekseni sayar"* diye anılmış, ama IP'nin de değiştirilebildiği yazılmamıştı.
- **BULGU-2:** Daha önce gelmemişti. 6.2.15'in "bağlanılamazsa kart başarısızlığıdır" cümlesi 18. turda girdi (K-180, K-231); L-7 ile kesişmesine o gün bakılmamıştı.
- **BULGU-3:** Daha önce gelmemişti. Gecikme feshinde IBAN alanı K-499 ile girdi, §3.33.5'in listesi K-553 ile kuruldu; ikisi aynı turda birbirine bağlanmamıştı.
- **BULGU-4:** 11. turda (K-589) gelen konunun tanım üzerinden ikinci kez gelmesidir.

## 3. Ek bulgular

- **6.4.7 ile §7.4.5 arasındaki fark:** BULGU-3'ün aynı eksikliği 6.4.7'de de vardı (*"iptal ya da cayma beyanı sırasında"*). Bulgu §7.2.3'ü gösteriyordu; 6.4.7'yi göstermiyordu. Aynı turda hizalandı.
- **§5.5'in kaynak satırında K-233 ve K-589 yoktu.** Başarısız tanımı geç gelen ödemeyle ilgili iki kararı anmıyordu; eklendi.
- **Önceki turlardan açık kalanlar:** 21. turun "üyenin e-postasını değiştirmesinde teyidin hangi adrese yapılacağı" notu ile 20. turun K-606 hizmet ve dijital kalem notu bu turda da gelmedi; `04` bunları ele almalı.
- **Etki yansıtma için not:** `10` KP-72 L-7'nin sayım kapsamını yazmıyor; K-614 ile çelişki doğmadı. `01`'de L-7 ya da aydınlatmanın gösterim noktaları için cümle yok. `08` ödeme sayfasının hiç açılmadığı bağlantı hatasını ödeme sayfasında doğan başarısızlıktan ayırmalı (K-614). `04` açık ödenmemiş siparişlerin panel görünümünü firma iptaline bir adımda götürmeli (K-613). `12`'nin aydınlatma metni taslağı gecikme feshindeki IBAN'ı veri kategorileri arasında saymalı.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`02` v0.41). İki yeni karar satırı açıldı; biri ⚠ ile işaretli. Seçenekleri gerçekten ayrışan ve firmaya ya da müşteriye ciddi para veya hukuki risk yükleyen bir konu çıkmadı. K-614 bir sayaçtan dar bir hâli çıkarıyor ve kötüye kullanım kapısı açmıyor. K-613 mekaniği değiştirmiyor, kalan riski ve firmanın yolunu yazıyor.

- [x] BULGU-1 (kısmi — K-613)
- [x] BULGU-2 (kabul — K-614 ⚠)
- [x] BULGU-3 (kabul — K-553'ün uygulaması; §3.33.5 ve 6.4.7)
- [x] BULGU-4 (red — K-233, K-589; tanım §6.2.16'ya bağlandı)
- ⚠ **K-614:** Sağlayıcıya bağlanılamadığı için ödeme sayfası hiç açılmamış kart siparişinin iptali ödeme denemesi limitine (L-7) sayılmaz; K-332'nin sayımını daraltır. Elenen seçenek: altyapı kaynaklı her başarısızlığı muaf tutmak (ödeme sayfasına gidilmiş siparişte kaynak güvenilir biçimde ayrılamaz). Gözden geçirilecek nokta: `08` bu ayrımı kurabilmeli; kuramazsa sipariş sayılmaya devam eder ve eski davranış geri gelir.

**Hukuki kontrol** (2026-10-03):
- **BULGU-1:** Hukuki iddia yok.
- **BULGU-2:** Hukuki iddia yok.
- **BULGU-3:** 6698 sayılı KVKK m.10/1: veri sorumlusu aydınlatmayı *"kişisel verilerin elde edilmesi sırasında"* yapar. Gecikme feshinde IBAN o ekranda elde edildiği için bağlantı orada görünmelidir. Dokümanın kendi kuralı (§3.33.5, K-553) bu hükmün uygulamasıdır; bulgu kuralın listesindeki eksikliği gösterdi.
- **BULGU-4:** Hukuki iddia yok. Saklama süresinin ayrımı (Z-28, Z-41) K-519 ve K-589'da Mesafeli Sözleşmeler Yönetmeliği ve VUK'a karşı kontrol edilmişti; bu turda değişmedi.
