# Cross-Review — 05 Technical Architecture (Tur 4)

**Tarih:** 2026-10-17 · **Hedef:** `Docs/05_TECHNICAL_ARCHITECTURE.md` v0.8 (girdi commit'i `81cfe23`, 309 297 bayt, 1 482 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren uygulanır (K-952) · **Koşum:** iki biçimde, altı çağrı — tek parça çağrı ve beş parçalı kontrol koşumu (K-953); turun sonucu ikisinin birleşimidir · **Girdi:** yalnız doküman ve boş şablonu (`58cf0b9` — Aşama 4 açılmadan önceki v0.1; park blokları çıkarılmış, 87 satır), stdin'den tek metin, boş ve izole bir çalışma klasöründe; karar kaydı, önceki turların raporları, Ürün Gereksinimleri, Kullanıcı Akışları, Arayüz Tanımları, MVP Kapsamı ve audit/deep review raporları verilmedi (K-431)

## 1. Koşum

**İstem ve şablon** 1. turunkidir, değiştirilmeden (`05_CROSS_REVIEW.md` §1'in alıntı blokları — istem ve parça notu). Girdiler aynı betikle yeniden kuruldu: `istem + "=== DOKÜMAN ===" + güncel doküman + "=== ŞABLON ===" + şablon`; parçalı çağrılarda istemden sonra parça notu, parçada başlık notu. **Komut** 1. turunkidir: `codex exec -s read-only -C <boş klasör> --skip-git-repo-check --ephemeral --ignore-user-config --color never -m gpt-5.6-terra -c 'model_reasoning_effort="high"' -o <çıktı> - < <girdi>`. Altı çağrı paralel koştu; altısı da hatasız döndü.

**Parça sınırları** 1. turunkidir — bölüm başlıkları (K-953); satır numaraları v0.8'indir.

| Parça | Bölümler | Satırlar | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|---|
| — | Tek parça — dokümanın tamamı | 1–1482 | 317 284 bayt | 132 152 | 265 sn | 2 bulgu |
| 1 | Başlık notu · §0 · §1 · §2 · §3 | 1–438 | 102 088 bayt | 37 540 | 38 sn | `SONUÇ: TEMİZ` |
| 2 | §4 durum ve olaylar · §5 arka plan işleri | 439–614 | 76 958 bayt | 42 040 | 113 sn | 2 bulgu |
| 3 | §6 güvenlik (§6.x dahil) | 615–820 | 90 900 bayt | 55 336 | 120 sn | 1 bulgu |
| 4 | §7 veri katmanı | 821–955 | 80 479 bayt | 55 448 | 170 sn | 1 bulgu |
| 5 | §8 ortamlar · §9 gözlemlenebilirlik · §10 hata toleransı · §11 ölçeklenebilirlik · §12 özet | 956–1482 | 147 625 bayt | 55 300 | 118 sn | 1 bulgu |

**Web araması:** parça 1 ve 5 aramadı; tek parça çağrı ve parça 3 KVKK'nın Çerez Uygulamaları Hakkında Rehberi'ni (ikisi de çerez bulgusu yazmadı), parça 2 ve 4 6563 sayılı Kanun m.11/3'ü aradı (koşum kayıtlarındaki `web search` satırları). Kamuya açık mevzuat ve araç belgesi proje bilgisi değildir (1. turun raporu §1); açık olduğu yine yazılır.

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**Tek parça çağrı:**

> ```text
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Orta
> Yer: §4.2.2 (3)–(4)
> Alıntı: "stoğu, hizmet kontenjanını ve kupon hakkını koşullu güncellemeyle ayırır: ayırma yalnız kalan adet yetiyorsa yazılır; [...] aynı sepete bağlı ödenmemiş önceki siparişin satır kilidi bunlardan önce alınır (§7.3.5) → [...] aynı sepete bağlı ödenmemiş önceki siparişi iptal eder"
> Sorun: Önceki ödenmemiş siparişin ayırdığı son stok/kontenjan, yeni siparişin ayırmasından sonra bırakılıyor. Son ürün önceki siparişte ayrılmışsa yeni ayırma başarısız olur, işlem geri alınır ve eski sipariş de iptal edilmez. Müşteri kendi eski ödenmemiş siparişi nedeniyle yeniden ödeme akışına geçemez; süre sonunu beklemek zorunda kalır. Bu, çıkışı olmayan akış ve satış kaybı riski doğurur.
> Öneri: Aynı sepetin önceki ödenmemiş siparişinin ayırmalarını, aynı atomik işlemde yeni ayırmadan önce serbest bırakıp siparişi iptal edin; alternatif olarak yeni ayırma hesabında o siparişin mevcut ayırmasını müşterinin kullanılabilir bakiyesi olarak hesaba katın.
>
> BULGU-2
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §8.7.4
> Alıntı: "Yedekten dönüş yalnız bir göç veriyi bozduğunda ve dönüş noktasından sonra sipariş tarafında hiçbir kayıt yazılmamışsa yapılır. [...] kurulumu yapan dönüşten önce [...] dönüş noktasından sonraki satırlara bakar"
> Sorun: Kontrol ile yedekten dönüş arasında eski uygulama sipariş almaya devam edebilir; doküman bu aralıkta yeni yazımı engelleyen bir bakım/kapama, münhasır kilit veya dönüş öncesi son atomik kontrol tanımlamıyor. Kontrolden hemen sonra yazılan sipariş ya da ödeme geri dönüşle kaybolabilir. Bu, §0.13’teki sipariş tarafında sıfır veri kaybı güvencesini bozar.
> Öneri: Yedekten dönüş yolunu açıkça atomikleştirin: uygulamayı yeni sipariş yazamayacak biçimde durdurun, sağlayıcı bildirimlerini güvenli yeniden denemeye bırakın, ardından veritabanında son kontrolü yapıp dönüşü gerçekleştirin. Kontrolden sonra yeni yazım tespit edilirse geri dönüşü iptal edin.
>
> SONUÇ: 2 BULGU
> ```

**Parça 1 — başlık notu, §0–§3:**

> ```text
> SONUÇ: TEMİZ
> ```

**Parça 2 — §4–§5:**

> ```text
> BULGU-1
> Kriter: teknik doğruluk
> Seviye: Yüksek
> Yer: §4.3.3, §4.6.1–§4.6.3
> Alıntı: "Teslim en az bir kezdir."
> Sorun: Kart iadesi isteği dış sağlayıcıya gönderildikten sonra, sonuç yazılmadan süreç çökerse iş en az bir kez teslim nedeniyle yeniden çalışır. Sağlayıcının tekrar anahtarını destekleyip desteklemediği ise zorunlu bir mimari kısıt değil, `08`'e bırakılmıştır. Destek yoksa aynı iade iki kez gönderilebilir ve firmaya/müşteriye para kaybı doğurur (2).
> Öneri: Sağlayıcıda idempotency/referansla tekilleştirme desteğini zorunlu entegrasyon koşulu yapın; desteklenmiyorsa yeniden göndermeden önce sağlayıcıdan önceki isteğin sonucunu kesin olarak sorgulayan ve belirsizlikte yeni iade isteğini engelleyen akışı belirtin.
>
> BULGU-2
> Kriter: güvenlik
> Seviye: Yüksek
> Yer: §4.4.4
> Alıntı: "ödemesi hiç alınmamış siparişin satırında üç yıl dolunca alıcı ve olayın değerleri boşaltılır ve satır on yılda silinir"
> Sorun: 6563 sayılı Kanun kapsamındaki iş ve işlemlere ilişkin bilgi, belge, defter ve elektronik kayıtların işlem tarihinden itibaren on yıl saklanmasını gerektirir. Ödemesi alınmamış olsa da oluşturulmuş sipariş ve buna bağlı gönderim kaydı, hizmet sağlayıcının elektronik ticaret işlemi/kaydıdır; alıcı ve olay değerlerinin üçüncü yılda silinmesi kaydı denetimde noksansız sunulamaz hâle getirir (1).
> Öneri: Kanuni saklama kapsamındaki sipariş ve gönderim kayıtlarının gerekli kişisel verilerini on yıl boyunca, erişimi amaçla sınırlı tutarak saklayın; yalnız kanunen bu kapsamın dışında olduğu açıkça belirlenmiş kayıtlar için üç yıllık anonimleştirme uygulayın.
>
> SONUÇ: 2 BULGU
> ```

**Parça 3 — §6:**

> ```text
> BULGU-1
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §6.1.4
> Alıntı: "Yalnız Google'ın doğrulanmış verdiği e-posta kabul edilir ve ürün Google'dan yalnız adı ve e-postayı alıp saklar — Google hesabının kalıcı kimliği saklanmaz"
> Sorun: Google e-posta adresini kalıcı hesap kimliği olarak kullanmayı açıkça yasaklar; adres değişebilir ve tekil kalmayabilir. Doküman da Google e-postası değişince yeniden doğrulamanın tutmayacağını kabul etmektedir. Bu tasarımda değişmiş/yeniden atanmış bir doğrulanmış e-posta, yanlış hesaba bağlanabilir; sipariş, adres ve hesap verileri yetkisiz kişiye açılabilir. Bu, kişisel verinin yetkisiz açılmasıdır (1).
> Öneri: Google ile ilişkiyi doğrulanmış e-postaya değil, Google’ın değişmeyen `iss` + `sub` kimliğine bağlayın. E-postayı profil/iletişim alanı olarak ayrı yönetin; Google dönüşünde `sub` eşleşmesini zorunlu kılın.
>
> SONUÇ: 1 BULGU
> ```

**Parça 4 — §7:**

> ```text
> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §7.4.3 (4)
> Alıntı: “ödemesi hiç alınmamış siparişin … üç yıl dolunca alıcı ve olayın değerleri boşaltılır”
> Sorun: 6563 sayılı Kanun m.11/3, hizmet sağlayıcının Kanun kapsamındaki iş ve işlemlerine ilişkin bilgi, belge, defter ve elektronik kayıtları işlem tarihinden itibaren on yıl saklamasını zorunlu kılar; ödeme alınmış olmayı koşul yapmaz. Ödemesiz siparişlerde alıcı ve olay verilerinin üç yılda silinmesi, kapsamdaki işlem kayıtlarının on yıl boyunca eksik kalmasına yol açar. Bu, kabul edilmiş bir tercih olsa da mevzuata aykırılık oluşturur. [Kanun metni](https://mevzuat.adalet.gov.tr/mevzuat/103057?query=Madde+1)
> Öneri: K-951 revize edilerek, Kanun kapsamındaki tüm sipariş/işlem kayıtlarının gerekli bilgi ve belgeleriyle on yıl saklanması sağlanmalı; yalnız Kanun kapsamına hiç girmeyen taslak/terk edilmiş işlem türleri açık ve gerekçeli biçimde ayrılmalıdır.
>
> SONUÇ: 1 BULGU
> ```

**Parça 5 — §8–§12:**

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §9.2.1, §9.2.3
> Alıntı: “Sistem kayıtları … kaydın yazıldığı andan P-45 kadar saklanır” / “Her gecenin anlık görüntüsü kayıtların o geceki bütün hâlini — sunucuda tutulan P-45 penceresinin tamamını — taşır; otuz gün anlık görüntülerin geçmişidir, kaydın yedekteki süresi değildir.”
> Sorun: P-45’i dolduran bir kayıt, son gece anlık görüntüsüne girdiyse yedek kopyada ayrıca otuz güne kadar kalır. Bu, kayıtların P-45 sonunda silineceği güvencesiyle çelişir ve kişisel veri içeren kayıtların tanımlı saklama süresinden sistematik olarak uzun tutulmasına yol açar. (4)
> Öneri: Kayıt yedeklerini kayıt bazında/yaşına göre P-45 sonunda da silinecek şekilde tasarlayın; örneğin günlük kayıt nesnelerine `yazım zamanı + P-45` yaşam döngüsü uygulayın veya anlık görüntüden süresi dolan kayıtları aynı anda kaldıran, yedek kopyaları da kapsayan imha mekanizması tanımlayın.
>
> SONUÇ: 1 BULGU
> ```

## 3. Bağımsız değerlendirme

Her bulgu dokümanın tamamına (v0.8), ilgili yerde Ürün Gereksinimleri'ne (`02` v0.72), Kullanıcı Akışları'na (`03` v0.26) ve karar kaydına karşı okundu — ikinci model bunları görmedi, parçalı çağrılar dokümanın öteki bölümlerini de görmedi. Yedi ham bulgu altı ayrı bulgudur — parça 2 ve parça 4 aynı iddiayı iki yerde getirdi; ikisi önceki turlarda iki kez reddedilmiş iddianın aynı yer ve iddiayla dönüşüdür (tabloda **tekrar eden RET**, skill Faz 4 madde 5).

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| Tek · BULGU-1 | ✅ KABUL | **Gerçek çelişki — `05` ile `02` arasında; doküman içinden de görünür.** 4.2.2'nin sırası (3) ayırma → (4) önceki siparişin iptaliydi (K-890). Son adedi aynı sepetin ödenmemiş önceki siparişi ayırmışsa yeni siparişin koşullu güncellemesi düşer, işlem bütünüyle geri alınır (4.2.3) ve önceki sipariş de iptal edilmez: müşteri önceki siparişin ödeme süresi (Z-7, Z-8) dolana kadar aynı sepetle yeniden onaylayamaz. Ürün Gereksinimleri tersini yazar: *"o sepete bağlı ödenmemiş önceki siparişi kendiliğinden iptal edilir ve ayırması serbest kalır; yeni siparişin ayırması bundan sonra yapılır. Müşteri kendi ayırdığı stoğa takılmaz"* (`02 §3.17.7`, §5.6.4, §6.2.2; K-182; `03` 3.2.1.2). Kart ödemesi yarıda kalıp aynı sepetle yeniden deneyen müşterinin olağan yoludur (`02 §3.16.12`). Ölçünün (3). sınıfı (süre dolana kadar çıkışsız) ve (4). sınıfı (iki dokümanın aynı kuralı ters yazması); şiddet orta. **Modelin ilk önerisi uygulandı;** ikinci önerisi — önceki siparişin ayırmasını yeni ayırmada müşterinin kullanılabilir adedine saymak — elendi: ikinci bir hesap yoludur, iptal zaten aynı işlemdedir. Kilit sırası (7.3.5) korunur. K-890'ın sırasını değiştirdiği için karar satırı açıldı. | 4.2.2: (3) önceki siparişin iptali ve ayırmasının serbest bırakılması → (4) yeni ayırma → (5) numara, dondurma, bildirimler; kilitlerin sırası ayrı cümlede — önce önceki siparişin satırı, sonra serbest bırakılan ve ayrılan varyantlar birlikte, kimlik sırasıyla · 4.2.3: geri almada önceki siparişin iptali de geri alınır · §4.2'nin Kaynak satırı · §12 MK-36 · Doğrulama Protokolü'nün K-890 park satırına (6). senaryo. **K-957** (öneriyle kaydedildi); K-890'a geri işaret. |
| Tek · BULGU-2 | ✅ KABUL | **Mekanizma dokümanın kendi güvencesini sağlamıyordu.** 8.7.4 yedekten dönüşü dönüş noktasından sonra sipariş tarafında yazım olmamasına bağlar ve denetimi kurulumu yapana verir; ama eski sürüm göç sırasında da sipariş alır (8.7.2) ve doküman denetimin uygulama çalışırken mi durmuşken mi yapıldığını yazmıyordu. Denetimden sonra, dönüşten önce yazılan sipariş, ödeme ya da talep dönüşle kaybolur — §0.13'ün sipariş tarafındaki sıfır kaybı (ölçünün (4). sınıfı; kaybolan sipariş (2)); şiddet orta — pencere dar, ama güvence koşulsuzdur. Veritabanını eski bir ana döndürmek zaten veritabanının durmasını ister; eksik olan durmanın denetimden **önce** olmasıydı. **Modelin önerisi uygulandı**, son cümlesi hariç: uygulama durduğu için denetimden sonra yazım doğmaz, ikinci denetim gerekmez. "Satışı anahtarıyla kapatıp denetlemek" elendi — anahtar firmanındır (`02 §3.1.5`) ve panel ile iletişim formu yazmaya devam eder. K-938'in ve K-956'nın denetimine sıra eklediği için karar satırı açıldı. | 8.7.4: kurulumu yapan önce uygulamayı durdurur — vitrin, panel ve iş kuyruğu yazmaz —, sonra denetler; yazım varsa uygulama yeniden başlatılır ve ileri göç; dururken gelen sağlayıcı bildirimi kaydedilmez, sonucu yeniden gönderim ya da son sorgu bulur (§4.5.3, §10.1) · §8.7'nin Kaynak satırı · §12 MK-74 · `DEPLOY_RUNBOOK`'un K-938 park satırı. **K-958** (öneriyle kaydedildi); K-938 ve K-956'ya geri işaret. |
| P2 · BULGU-1 | ⚠️ KISMİ | **Para kaybı doğmaz, ama bağ dokümanda yazılı değildi.** Karta para gönderen istekten önce geri ödeme kaydı "istek gönderiliyor" hâliyle yazılır (4.6.1) ve sonucu bilinmeyen istek yeniden gönderilmez, sağlayıcıya sorulur (4.6.2; K-894); 4.3.4 de "kart iadesi kendiliğinden yeniden denenmez" der. Modelin senaryosu — istek gönderildikten sonra, sonuç yazılmadan çöken süreç ve iş satırının en az bir kez teslimle yeniden gelmesi (4.3.3) — K-894'ün Edge'inin karşıladığı durumdur ("sağlayıcı tekrar anahtarını desteklemiyorsa sorgu tek güvencedir"); ama 4.6.2 sonucu bilinmeyen hâli yalnız "zaman aşımı, bağlantının kopması" diye sayıyordu ve 4.3.3 çöken süreçte yalnız e-postanın ikinci kez gittiğini yazıyordu — işleyicinin kaydı "istek gönderiliyor" hâlinde bulduğunda ne yaptığı yazılı değildi. Ölçünün (2). sınıfına dokunan bir metin eksikliği; şiddet düşük. **Önerinin ilk yolu elendi** — sağlayıcının tekrar anahtarını zorunlu entegrasyon koşulu yapmak: `08`'in kararıdır (4.6.3; K-705'in park satırı) ve sorgu onsuz da yeter; ikinci yolu — yeniden göndermeden önce sorgulamak — zaten K-894'tür. Yeni karar yok. | 4.3.3: kart iadesi ikinci kez gitmez — "istek gönderiliyor" hâlindeki kayıt yeniden gönderilmez, sonucu sorulur · 4.6.2: sonucu bilinmeyen hâl çöken süreci ve yeniden gelen iş satırını sayar. K-894 değişmedi; etki sütununa iz. |
| P5 · BULGU-1 | ❌ RET | **Kural parçanın görmediği bölümde, bilinçli karar olarak yazılı.** İddia: P-45'i dolan kayıt son anlık görüntüde kaldığı için yedekte otuz gün daha durur ve bu "P-45 kadar saklanır" ile çelişir. İmha edilen verinin yedekteki kopyalarının süresi 7.4.5'in genel kuralıdır — *"imha edilen veri yedekteki her kopyada — tam yedek, işlem günlüğü, dosya yedeğinin anlık görüntüleri ve nesne depolamanın eski sürümleri — saklama süresi dolunca kendiliğinden kaybolur (§7.6.6); en uzun pencere otuz gün artı …"* (K-876; 7.6.10 "doksan gün ya da daha uzun saklama"yı bu yüzden eler). Kayıt dosyaları ve sunucunun kayıtları dosya yedeğine girer (7.6.1, 9.2.3) ve kural onları da kapsar; imha işinin silmesi (9.2.2) ile yedekteki kopyanın ayrı kaybolması çelişki değildir. §7 parça 5'e verilmedi — parça notunun "verilmeyen bölümde yaşayan içerik" hâli; tek parça çağrı dokümanın tamamını gördü ve bulgu yapmadı. Modelin önerisi — kayıt yedeklerine kayıt başına yaşam döngüsü — yedeğin anlık görüntü biçimini (7.6.1) değiştirir. **Dönen değil:** 1. turun aynı yerdeki bulgusu (P5 · BULGU-1) ters yöndeydi — yedeğin P-45'ten kısa olduğu — ve KISMİ'ydi. | — |
| P3 · BULGU-1 — **tekrar eden RET** | ❌ RET | Google hesabının kalıcı kimliğinin saklanmaması — 2. ve 3. turda aynı yer (§6.1.4) ve aynı iddiayla gerekçesiyle reddedildi (K-900; `02 §8.1.2`, §12.2.8; e-posta adresini ele geçiren şifre sıfırlamayla aynı hesaba zaten girer — `02 §3.13.8`, 6.1.5). Bu tur yeni bilgi yok: "Google e-postayı kalıcı kimlik olarak kullanmayı yasaklar" bir uygulama önerisidir, mevzuat değildir; "doküman da e-posta değişince yeniden doğrulamanın tutmadığını kabul eder" 6.1.5'in bilinçli sonucudur. Turun sonucunu belirlemez. | — |
| P2 · BULGU-2 ve P4 · BULGU-1 — **tekrar eden RET** | ❌ RET | Ödemesi hiç alınmamış siparişte kişisel verinin üç yılda silinmesi 6563 m.11/3'e aykırı denildi — 2. turda §4.4.4 ve §7.4.3 (4)'te (önerisi reddedildi, gerekçe yazıldı), 3. turda §7.4.3 (4)'te reddedildi; bu tur aynı iki yerde, aynı iddiayla. Karar proje sahibinindir (K-951, "1 · On yıl, karma") ve modelin önerisi — ödeme sonucundan bağımsız kişisel veri de on yıl — elenen seçenekti; gerekçe ve elenen seçenek §7.4.3 (4)'te yazılıdır. Modelin gösterdiği hüküm metni K-951'de okunan metinle aynıdır; yeni hukuki bilgi yok. Alt ajan proje sahibinin kararını değiştiremez. Turun sonucunu belirlemez. | — |

**Dağılım:** altı ayrı bulgu — 2 KABUL · 1 KISMİ · 1 RET · 2 tekrar eden RET. **%100 kabul değil, %100 ret değil:** RET somut yere dayanır (7.4.5, K-876); iki tekrar eden RET skill'in tanımına girer (en az iki kez gerekçeyle reddedilmiş, aynı yer ve iddia); KISMİ'de modelin önerisi reddedildi ve en küçük metin eklendi. **İki bulgu yeni karar doğurdu** (K-957, K-958 — ikisi de ⚠ dışı teknik mekanizma); biri metin hizalamasıdır.

## 4. Ek bulgular

- **Aynı kalıbın taraması (Tek · BULGU-1):** "önceki sipariş" `05`'te 4.2.2 ve 7.3.5'te geçer; 7.3.5 "sipariş onayı … önceki siparişin satır kilidini ayırmadan önce alır" der ve yeni sırayla da doğrudur — değişmedi. Ürün Gereksinimleri (`02 §3.17.7`, §5.6.4, §6.2.2, §4.3) ve Kullanıcı Akışları (`03` 2.4.9, 3.2.1.2, 4.2.1) kuralı zaten doğru yazar — geri besleme yok. Doğrulama Protokolü'nün K-890 park satırı (5). senaryoda aynı sepetle yeniden onayı yalnız ölü kilit için sayıyordu; (6). senaryo eklendi (K-957'nin etki sütunu ile park etiketi iki yönde eşleşir).
- **Aynı kalıbın taraması (Tek · BULGU-2):** "yedekten dönüş" `05`'te 8.7.4, 8.7.6, 7.6.10 ve MK-74'te geçer; denetimin anını yalnız 8.7.4 ve `DEPLOY_RUNBOOK`'un K-938 park satırı anlatır — ikisi güncellendi, MK-74'ün iki hücresi K-958'i aldı. 7.4.5 "dönüşten sonra, uygulama başlamadan önce" silmelerin yeniden uygulanmasını yazar — yeni sırayla tutarlıdır.
- **Aynı kalıbın taraması (P2 · BULGU-1):** "istek gönderiliyor" 4.6.1 ve 4.6.3'te hâl adı olarak geçer; MK-40 "sonucu bilinmeyen istek yeniden gönderilmez" der ve çöken süreci de kapsar — değişmedi.
- **Gözlem (bulgu değil):** uygulama dururken nabız (5.4.1) yazmaz; dönüşte kesintinin tespiti (§5.4) durmayı kesinti sayar ve havale sürelerinin ertelenmesi olağan kesinti kuralıyla işler. Yeni kural gerekmez.
- **Mekanik tarama:** Kaynak satırı betiği (MK- kimliklerini dışarıda bırakarak) değişiklikten önce ve sonra aynı sonucu verir; tek fark §12'nin gerekçe hücrelerine giren K-957 ve K-958'dir (§12'nin Kaynak satırı "K numaraları gerekçe hücresindedir" der). §4.2'nin ve §8.7'nin gövdesine giren K-957 ve K-958 Kaynak satırlarında. Başlık sürümü, yazım durumu notu ve dosya sonu dipnotu v0.9'u gösterir. Karar kaydında çift numara yok; K-855'ten bu yana `(öneriyle kaydedildi — ⚠)` taşıyan satırlar betikle sayıldı — 32 — ve §12.1'in listesiyle aynı. Karar kaydının §12 Kaynak satırına 3. turdan eksik kalan K-956 de eklendi.
- **Ciddi bir sorun görülmedi.** Değerlendirme sırasında ikinci modelin listesi dışında ölçünün dört sınıfından birine giren bir sorun saptanmadı.
- **Önceki turlardan açık kalanlar:** yok. Yedeğe ulaşmamış penceredeki silmeler (K-888, K-955) bu turda dönmedi.

## 5. Kullanıcı onay checklist'i

K-855 kararıyla (Aşama 4 boyunca ⚠ öneriyle kayıt, CI yeşilse `docs:` PR merge yetkisi ve yönetici + alt ajan düzeni) alt ajan değerlendirdi ve uyguladı (`05` v0.9). İki karar öneriyle kaydedildi, ikisi de ⚠ dışı: **K-957** — sipariş onayında aynı sepetin ödenmemiş önceki siparişi yeni ayırmadan önce iptal edilir (K-890'ın sırası `02 §3.17.7`'ye hizalanır) · **K-958** — güncellemede yedekten dönüşün denetimi uygulama durdurulduktan sonra yapılır. ⚠ listesi 32'de kalır. Ürün Gereksinimleri'ne, Kullanıcı Akışları'na, Arayüz Tanımları'na ve MVP Kapsamı'na dönen değişiklik yok — K-957 `02 §3.17.7`'ye hizalanır, K-958 bir işletim sırasıdır; `05`'in geri besleme tablosuna satır girmedi. Park satırları: Doğrulama Protokolü (Eşzamanlılık — K-957) · `DEPLOY_RUNBOOK` (Güncelleme, geri alma — K-958). Geri işaret: K-890, K-894, K-938 ve K-956'nın etki sütunları.

- [x] Tek · BULGU-1 (kabul — K-957: önceki siparişin iptali ayırmadan önce)
- [x] Tek · BULGU-2 (kabul — K-958: denetimden önce uygulama durur)
- [x] P2 · BULGU-1 (kısmi — "istek gönderiliyor" hâlindeki kart iadesi yeniden gönderilmez; tekrar anahtarını zorunlu kılma önerisi elendi)
- [x] P5 · BULGU-1 (ret — yedekteki kopyaların süresi 7.4.5'te, K-876)
- [x] P3 · BULGU-1 (tekrar eden RET — K-900)
- [x] P2 · BULGU-2 ve P4 · BULGU-1 (tekrar eden RET — K-951, proje sahibinin kararı)
- [ ] Etki yansıtma (Faz 5) — döngü TEMİZ döndükten sonra, son turun raporunda

**Hukuki kontrol** (2026-10-17): iki bulgu yasal dayanak taşır. P2 · BULGU-2 ve P4 · BULGU-1 — 6563 sayılı Kanun m.11/3'ün son cümlesi; metin K-951'de resmî kaynaktan okunmuştu ve modelin aktarımıyla aynıdır. P5 · BULGU-1 kişisel verinin saklama süresine dokunur; yedekteki kopyaların süresi K-876'nın kararıdır. P3 · BULGU-1'in Google belgesi bir uygulama önerisidir, mevzuat değildir. Düzeltmeler yeni hukuki içerik eklemez.

**Yakınsama ölçüsü:** ayrı bulgu 7 → 5 → 6 → 6; kabul edilen ya da kısmen kabul edilen 7 → 3 → 3 → 3; yeni karar 2 → 0 → 1 → 2 (bu turun ikisi de ⚠ dışı); dönen iddia 0 → 1 → 3 → 2 (bu tur ikisi de tekrar eden RET). Bu turun üç kabulünden ikisi orta şiddette ve 1.–3. turda okunmuş bölümlerde (§4.2, §8.7) ilk kez bulundu — biri iki dokümanın aynı kuralı ters yazması, biri bir güvencenin işletim sırası; biri düşük şiddette metin eksikliği. Doküman 309 297 bayttan 311 756 bayta büyüdü; tablolara satır girmedi. Parça 1 (başlık notu, §0–§3) bu turda yeniden TEMİZ.

## 6. Sonuç ve 5. tur

**Sonuç: TEMİZ DEĞİL** (ölçünün dört sınıfında) — altı çağrının biri TEMİZ; altı ayrı bulgunun ikisi kabul edildi, biri kısmen uygulandı, biri reddedildi, ikisi tekrar eden RET'tir. Tekrar eden RET'ler turun sonucunu belirlemez; ama iki KABUL ve bir KISMİ kalır. Faz 5 (etki yansıtma) bu turda koşmaz; TEMİZ turunda koşar.

**Çıkış kuralı bu turda uygulanmadı.** Skill'in Faz 4 madde 5'inin ikinci kuralı "beşinci turdan itibaren" uygulanır — bu 4. turdur; ayrıca kabul edilen bulguların ikisi orta şiddettedir (çıkışsız akış ve sipariş tarafında kayıp), kural bu durumda zaten döngüyü sürdürür. Yeni bir K-849 kalıbı karar gerekmiyor.

**5. tur** aynı istemle (1. turun raporunun §1'i, değiştirilmeden), aynı şablonla ve iki koşumla (K-953) yürür; parça sınırları bölüm başlıklarıdır. Önceki turların raporları, karar kaydı ve audit raporları verilmez (K-431). Tur, altı çağrının altısı da `SONUÇ: TEMİZ` dönerse — ya da yalnız tekrar eden RET'ler kalırsa — TEMİZ'dir. **5. turda tekrar eden RET sayılacaklar** (aynı yer ve iddiayla dönerlerse): Google hesabının kalıcı kimliği (§6.1.4) · ödemesi alınmamış siparişte kişisel verinin üç yılı (§7.4.3 (4), §4.4.4). Yedeğe ulaşmamış penceredeki silmeler (§7.4.5) önceki turlarda bir kez RET aldı — dönerse kendi gerekçesiyle okunur. **5. turda çıkış kuralı uygulanabilir:** kabul edilen bulguların hepsi düşük şiddette iki yerin metin farkıysa bulgular uygulanır, aynı kalıp dokümanın tamamında taranır, döngü "TEMİZ — çıkış kuralıyla, 5 tur" diye kapanır ve sınıfın kalan riski karar kaydının §12.1'inde aşamanın checkpoint'ine devredilir; kabullerden biri orta ya da yüksek şiddetteyse döngü sürer.
