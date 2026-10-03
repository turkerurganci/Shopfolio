# Cross-Review — 02 Product Requirements (Tur 16)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/02_PRODUCT_REQUIREMENTS.md` v0.31 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde; satır sonlarındaki boşluklar atıldı). İstem önceki turlarınkiyle aynıdır: yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §3.22 Sipariş numarası ve takip erişimi
> Alıntı: "Sipariş numarası tahmin edilemez bir koddur: yıl + rastgele karakter bloğu"
> Sorun: Sipariş takip erişimi sipariş numarası + e-posta ile açılıyor; ancak rastgele blok §11 P-19’da yalnız 6 karakterdir ve başarısız sorgu limiti yalnız IP eksenindedir. E-posta adresini bilen bir saldırgan, dağıtık IP’lerle tahmin denemeleri yaparak sipariş, adres, dijital indirme ve IBAN işlemlerine erişmeye çalışabilir.
> Öneri: Misafir takip kodu için en az 128 bit entropili bir sır tanımlayın; sipariş numarasını erişim sırrı saymayın. Başarısız sorgu limitini IP’ye ek olarak sipariş/e-posta çifti ve hesap düzeyinde uygulayın.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §10.4.11 Misafir siparişinin iletişim e-postasının düzeltilmesi
> Alıntı: "yeni adres doğrulanmış bir hesabın e-postasıysa sipariş §3.13.3'ün kuralıyla o hesaba düşer"
> Sorun: Atıf yapılan §3.13.3, mevcut hesap sahibi kişinin siparişi başlangıçta misafir olarak vermesi durumunu düzenler; yönetici tarafından sonradan değiştirilen misafir siparişi e-postasının mevcut bir hesaba bağlanmasını düzenlemez. Bu nedenle siparişin gerçekten hesaba bağlanıp bağlanmayacağı, ne zaman bağlanacağı ve o hesabın geçmiş sipariş verisine erişip erişemeyeceği belirsizdir.
> Öneri: E-posta düzeltmesi için ayrı ve açık bir bağlama kuralı ekleyin: yeni adres mevcut doğrulanmış müşteri hesabına aitse bağlamanın hemen mi yapılacağı, hangi yetkilerin verileceği ve bağlama gerçekleşmeden önce ek bir hesap sahibi teyidi gerekip gerekmediği belirtilsin.
>
> SONUÇ: 2 BULGU```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ⚠️ KISMİ | Sorun gerçek ve dokümanın kendi gerekçesiyle çelişiyordu. K-330 iki ekseni şu gerekçeyle kurdu: *"farklı IP'lerden tek bir hesaba yönelen dağıtık deneme hiç görünmez — korunmak istenen asıl senaryo tam budur."* Takip sorgusunu ise gerekçe yazmadan yalnız IP ekseninde bıraktı (L-5, P-35). Aynı senaryo takip sorgusunda da vardır: hedefin e-postasını bilen biri sipariş numarasının rastgele bloğunu farklı IP'lerden dener. K-462 altı karakteri *"takip sorgusu limitiyle birlikte tahmin edilemez kalır"* diye gerekçelendirmişti; bu sonuç yalnız IP ekseniyle dağıtık denemede tutmuyordu. Örnek: karışabilen karakterler çıkınca blok yaklaşık 32 karakterlik bir alfabeden gelir, 32⁶ ≈ 1,07 milyar. Bin IP, IP başına 10 / 15 dakikayla günde yaklaşık 960.000 deneme yapar. Tek siparişli bir hedefte beklenen süre bir buçuk yıl civarındadır, on bin IP'de iki ay civarına iner. E-posta ekseni 5 / 15 dakikayla tek adrese günde en çok 480 deneme bırakır ve bu süre pratikte sonsuza çıkar. **Önerinin iki parçası alınmadı.** (1) *"En az 128 bitlik ayrı bir takip sırrı"*: numara + e-posta yolu müşterinin elle yazdığı ve telefonda okunan bir yoldur (K-97, K-185). Yüksek entropili sır zaten e-postadaki bağlantıda vardır (§3.22.5, K-562). (2) *"Sipariş/e-posta çifti ekseni"*: çift ekseni her çifti bir kez deneyen saldırıyı saymaz. Sipariş numarası ekseni de eklenmedi: numara dolaşan bir veridir ve numarayı bilen biri e-postayı birkaç tahminle dener. Böyle bir eksen o denemeyi durdurmaz, yalnız gerçek sahibin sorgusunu kilitlerdi. | K-602 `(öneriyle kaydedildi — ⚠)`. L-5 artık iki eksende ayrı sayılır: aynı IP ve sorguya yazılan aynı e-posta adresi. E-posta başına eşik 5 / 15 dakika, IP eşiği değişmedi. Gerçek sahibin sorgusu e-posta ekseninde geçici olarak engellenebilir; bu kalan risk yazıldı. Engel kendiliğinden açılır, e-postadaki bağlantı ve üyenin hesabı sayaçtan etkilenmez. L-7 ve L-8'de misafir için e-posta eksenini eleyen gerekçeden (K-579) farkı da yazıldı. Güncellenen yerler: §3.22.3, §6.3.2, §8.2 (L-5 satırı ve §8.2.2), §8.3.5, §11 P-35. |
| BULGU-2 | ⚠️ KISMİ | Doküman yanlış değildi, ama atıf okuyanı yarı yolda bırakıyordu. §10.4.11 (5) *"sipariş §3.13.3'ün kuralıyla o hesaba düşer"* diyordu. §3.13.3 ise metinde yalnız siparişin ilk verildiği anı anlatır: hesabı olan kişi misafir olarak sipariş verirse sipariş hesaba anında düşer ve tam bağlanır (K-99). Karar kaydında cevap vardı: K-585 *"yeni adres doğrulanmış bir hesabın e-postasıysa sipariş K-99'un kuralıyla o hesaba düşer"* der. K-99'un kuralı "anında" ve "tam"dır. Ama bu iki niteliğin düzeltilen adrese de geçtiği dokümanda tek cümleyle söylenmiyordu. Codex'in üçüncü sorusu, yani bağlamadan önce hesap sahibinden ek teyit gerekip gerekmediği, yeni bir çit istemez. Yeni adresi firma müşterinin talebiyle ve kimlik teyidiyle girer (§10.4.11, §8.3.6). Hesabın adresi de doğrulanmıştır. K-99 bağlanan siparişe yetki çiti koymayı açıkça elemişti (*"bağlandıysa tam bağlanır"*). Kandırılmış düzeltmede siparişin kandıranın hesabına da düşmesi yeni bir risk değildir: kandıran yeni anahtarlı bağlantıyla sipariş sayfasına zaten girer (§8.3.6). | Yeni karar satırı açılmadı, K-585'in sonucu açık yazıldı. §10.4.11 (5): sipariş düzeltme anında yeni adresin müşteri hesabına düşer ve tam bağlanır. Hesap sahibinden ayrıca onay istenmez ve gerekçesi yazıldı. Yeni adresin hesabı yoksa sipariş misafir kalır, o adresle sonradan hesap açılırsa §3.13.2'nin kuralıyla düşer. Kandırılmış düzeltmenin kalan riski §8.3.6'ya bağlandı. §3.13.3'e de aynı kuralın düzeltilen adres için işlediği eklendi. |

**Dağılım:** 0 KABUL · 2 KISMİ · 0 RET.

- **BULGU-1:** Daha önce gelmemişti. 1. turun BULGU-2'si e-postadaki bağlantının entropisini sormuştu (K-562), sorgu limitinin eksenini sormamıştı.
- **BULGU-2:** Daha önce gelmemişti. Karar doğruydu (K-585), metindeki atıf eksikti.

## 3. Ek bulgular

- **Nötr mesaj:** §6.1.5 limit mesajının hangi eksende engellendiğini söylemediğini zaten yazıyor. L-5'in yeni ekseni için ayrı bir mesaj gerekmiyor.
- **Fesihten sonra kargonun malı yine de teslim etmesi (11. turdan açık, işlenmedi):** Bu turda da gelmedi. Etki yansıtmada ya da sonraki turda bakılmalı.
- **Şifre değişikliği (sıfırlama değil, 14. turdan açık):** §3.13.10 yalnız sıfırlamada oturumları kapatıyor. Bu turda gelmedi. Etki yansıtmada bakılabilir.
- **Etki yansıtma için not:** `05` L-5'in e-posta sayacını taşımalı. `04` takip formunun nötr mesajını iki eksen için aynı tutmalı. `01` ve `10`'da takip sorgusunun limitine dair bir cümle yok. Bu turda `01` ve `10`'a dokunulmadı.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`02` v0.32). Bir karar satırı açıldı:

- K-602 `(öneriyle kaydedildi — ⚠)`: kişisel veriye (sipariş sayfasına erişim) dokunuyor ve K-330'un takip sorgusu için seçtiği tek eksenden ayrılıyor.

- [x] BULGU-1 · [x] BULGU-2 (açıklama; karar değişmedi)

**Hukuki kontrol** (2026-10-03): İki bulgu da hukuki iddia içermiyor. BULGU-1, KVKK m.12/1'in veri sorumlusuna yüklediği "uygun güvenlik düzeyini temin etmeye yönelik gerekli her türlü teknik ve idari tedbir" yükümlülüğünün ürün içindeki bir parçasıdır. Düzeltme hukuki bir yorum gerektirmiyor. BULGU-2 dokümanın kendi kuralları arasındaki bir atıftır.
