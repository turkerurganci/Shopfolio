# Cross-Review — 05 Technical Architecture (Tur 6)

**Tarih:** 2026-10-17 · **Hedef:** `Docs/05_TECHNICAL_ARCHITECTURE.md` v0.10 (girdi commit'i `833b27e`, 316 896 bayt, 1 485 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren uygulanır (K-952) · **Koşum:** iki biçimde, altı çağrı — tek parça çağrı ve beş parçalı kontrol koşumu (K-953); turun sonucu ikisinin birleşimidir · **Şablon:** `58cf0b9`'daki v0.1 — Aşama 4 açılmadan önceki hâl, park blokları çıkarılmış, 87 satır · **Girdi:** yalnız doküman ve boş şablonu, stdin'den tek metin, boş ve izole bir çalışma klasöründe; karar kaydı, önceki turların raporları, Ürün Gereksinimleri, Kullanıcı Akışları, Arayüz Tanımları, MVP Kapsamı ve audit/deep review raporları verilmedi (K-431) · **Çıkış kuralı:** skill Faz 4 madde 5 — 5. turdan itibaren uygulanabilir

## 1. Koşum

**İstem ve şablon** 1. turunkidir, değiştirilmeden (`05_CROSS_REVIEW.md` §1'in alıntı blokları — istem ve parça notu; istem dosyasının ve şablonun özeti 5. turunkiyle aynı). Girdiler aynı betikle yeniden kuruldu: `istem + "=== DOKÜMAN ===" + güncel doküman + "=== ŞABLON ===" + şablon`; parçalı çağrılarda istemden sonra parça notu, parçada başlık notu. **Komut** 1. turunkidir: `codex exec -s read-only -C <boş klasör> --skip-git-repo-check --ephemeral --ignore-user-config --color never -m gpt-5.6-terra -c 'model_reasoning_effort="high"' -o <çıktı> - < <girdi>`. Altı çağrı paralel koştu; altısı da hatasız döndü (çıkış kodu 0). Yedek yol (`cursor-agent`) gerekmedi.

**Parça sınırları** 1. turunkidir — bölüm başlıkları (K-953); satır numaraları v0.10'undur.

| Parça | Bölümler | Satırlar | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|---|
| — | Tek parça — dokümanın tamamı | 1–1485 | 324 883 bayt | 111 021 | 194 sn | 2 bulgu |
| 1 | Başlık notu · §0 · §1 · §2 · §3 | 1–441 | 104 659 bayt | 36 702 | 46 sn | `SONUÇ: TEMİZ` |
| 2 | §4 durum ve olaylar · §5 arka plan işleri | 442–617 | 80 593 bayt | 34 982 | 217 sn | 2 bulgu |
| 3 | §6 güvenlik (§6.x dahil) | 618–823 | 93 365 bayt | 45 128 | 25 sn | `SONUÇ: TEMİZ` |
| 4 | §7 veri katmanı | 824–958 | 83 398 bayt | 41 637 | 126 sn | 1 bulgu |
| 5 | §8 ortamlar · §9 gözlemlenebilirlik · §10 hata toleransı · §11 ölçeklenebilirlik · §12 özet | 959–1485 | 153 494 bayt | 54 044 | 85 sn | 1 bulgu |

**Web araması:** altı çağrının hiçbiri web araması yapmadı (koşum kayıtlarında `web search` satırı yok). Parça 3'ün 25 saniyelik süresi kısadır; kayıt girdinin tamamının okunduğunu ve modelin `SONUÇ: TEMİZ` yazdığını gösterir.

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**Tek parça çağrı:**

> ```text
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §7.6.1–§7.6.2, MK-25
> Alıntı: “`archive_timeout` altmış saniyeye kurulur — açık günlük dosyası en geç bir dakikada kapatılır ve arşiv komutu onu taşır”
> Sorun: `archive_timeout`, WAL dosyasını en geç belirtilen sürede arşivlemeye hazır hâle getirir; nesne depolamaya yüklemenin tamamlanmasını bir dakika içinde garanti etmez. Yükleme gecikmesi veya arşiv komutunun sürmesi hâlinde disk kaybında bir dakikadan eski onaylı işlemler de kaybolabilir. Bu, §0.13 ve §7.6.2’deki sipariş verisi için “en çok son bir dakika” güvencesini sağlamaz; para ve sipariş kaybı doğurabilir.
> Öneri: Bir dakikalık pencereyi WAL’ın uzak depoda doğrulanmış olarak bulunma zamanından ölçün. Bu kesin güvence korunacaksa, uçtan uca süreyi karşılayan senkron/uzak dayanıklı yazım mekanizması kurun; karşılanamıyorsa bu istisnayı karar kaydı ve üst gereksinim dokümanında gerçek, ölçülebilir RPO’ya göre düzeltin.
>
> BULGU-2
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §7.6.6, MK-26
> Alıntı: “her anahtar yalnız kendi önekine yazar, okur ve güncel nesneyi siler; eski sürümü silemez”
> Sorun: Sürümlü S3 uyumlu depoda “güncel nesneyi silme” yetkisi silme işareti (delete marker) oluşturabilir. Bu, yedeği normal okumada görünmez yapar; ardından eski sürümler için otuz günlük yaşam döngüsü çalıştığında gerçek yedek sürümleri kalıcı olarak silinebilir. Böylece sunucuyu ele geçiren kişi, eski sürümü doğrudan silme yetkisi olmadan yedekleri geri döndürülemez biçimde yok edebilir. Bu, yedeğin silmeye karşı korunduğu güvencesiyle çelişir ve veri/sipariş kaybı doğurur.
> Öneri: Sunucudaki iki yedek anahtarından nesne silme ve delete-marker oluşturma yetkisini kaldırın. Saklama süresini firma hesabındaki yönetim yetkisiyle yönetilen yaşam döngüsü ve uygun nesne kilidiyle uygulayın; yedek araçlarının silme/prune işlemleri bu anahtarlarla yapılamamalıdır.
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
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §5.4.1, §5.4.3
> Alıntı: “İki nabız arasındaki boşluk **iki dakikayı** aşarsa aralık — son nabızdan yeni nabza — bir **kesinti kaydı** olur.” / “ödeme süresi kesinti aralığında dolmuş her havale siparişinin son anı dönüşten sonraki ilk iş gününün 23:59'una taşınır”
> Sorun: İki dakikadan kısa gerçek bir kesinti kayda dönüşmez. Bu aralıkta havale süresi dolarsa dönüşte süre uzatılmadan otomatik iptal çalışabilir. Bu, dokümanın kesinti sırasında dolan havale süresinin dönüş sonrası erteleneceği güvencesiyle çelişir ve müşterinin sipariş/ödeme hakkı ile firmanın tahsilatı açısından kayıp doğurabilir.
> Öneri: Kesinti tespitini iki dakikalık eşik nedeniyle dışarıda kalan gerçek kesintilerde de havale sonunu koruyacak biçimde tasarlayın; dönüşte işlenmemiş havale sonları için kesintinin varlığını güvenilir biçimde belirleyen ve iptalden önce çalışan kalıcı bir kontrol ekleyin.
>
> BULGU-2
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §4.5.2
> Alıntı: “bildirim gerekli alanlarıyla gelen bildirim kaydına yazılır ve sağlayıcıya hemen ‘alındı’ denir; işleme kuyruktan yapılır (§4.3).”
> Sorun: Gelen bildirim kaydının ve onu işleyecek kuyruk satırının aynı veritabanı işleminde kalıcılaştığı, ayrıca sağlayıcıya yanıtın ancak bu işlem commit edildikten sonra verildiği yazılmamış. Yanıt verilmesinden sonra süreç çökerse sağlayıcı yeniden bildirim göndermeyebilir; ödeme sonucu hiç işlenmeyebilir. Bu, ödeme/sipariş sonucunun kaybolmasına ve müşterinin ödemiş olduğu siparişin yanlışlıkla iptal edilmesine yol açabilir.
> Öneri: İmzalı bildirimin kaydı ile bekleyen iş satırını tek veritabanı işleminde oluşturun; commit başarılı olmadan sağlayıcıya başarılı yanıt vermeyin. Yinelenen bildirimlerde de yalnız kalıcı kayıt zaten mevcutsa başarılı yanıt dönün.
>
> SONUÇ: 2 BULGU
> ```

**Parça 3 — §6:**

> ```text
> SONUÇ: TEMİZ
> ```

**Parça 4 — §7:**

> ```text
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §7.6.6
> Alıntı: “her anahtar yalnız kendi önekine yazar, okur ve güncel nesneyi siler; eski sürümü silemez”
> Sorun: Güncel nesneyi silme izni, sürümlü nesne depolamada silme işareti oluşturarak yedeği derhal erişilemez kılar. Önceki sürümler “güncel olmayan sürüm” olur ve aynı maddede tanımlanan 30 günlük yaşam döngüsüyle silinir. Dolayısıyla sunucuyu ele geçiren biri yedekleri kalıcı olarak kullanılmaz hâle getirebilir; “yedek oradan silinemez” güvencesi sağlanmıyor (sınıf 4) ve veri kaybına yol açabilir (sınıf 2).
> Öneri: Sunucu anahtarından güncel nesneyi silme ve üzerine yazma yetkisini kaldırın; saklama temizliğini yalnız kova yaşam döngüsü ve firma hesabındaki ayrı yönetim yetkisiyle yürütün. Silme işaretlerinin geri dönüşü engellemediğini kurtarma provasında doğrulayın.
>
> SONUÇ: 1 BULGU
> ```

**Parça 5 — §8–§12:**

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §9.2.1, §9.2.3
> Alıntı: “Her gecenin anlık görüntüsü kayıtların o geceki bütün hâlini — sunucuda tutulan P-45 penceresinin tamamını — taşır; otuz gün anlık görüntülerin geçmişidir, kaydın yedekteki süresi değildir.”
> Sorun: Bu teknik olarak doğru değildir. P-45 dolmadan hemen önce alınan anlık görüntü, kayıt dosyasını içerir ve otuz gün daha tutulur; dolayısıyla IP gibi kişisel veri içeren sistem kayıtları §9.2.1’deki P-45 süresinden fiilen otuz gün uzun saklanır. Bu, belirtilen saklama/imha kuralıyla çelişir ve kişisel verinin gereğinden uzun tutulmasına yol açar (4).
> Öneri: Yedeklerdeki kayıtları da P-45 sonunda erişilemez/silinmiş kılacak bir mekanizma tanımlayın; örneğin kayıtları süre bazlı ayrı yedek parçalarına ayırın ve süresi dolan parçayı tüm anlık görüntülerden silin veya kriptografik olarak imha edin. Kurtarma sonrasında da süresi dolmuş kayıtların yeniden imhasını zorunlu kılın.
>
> SONUÇ: 1 BULGU
> ```

## 3. Bağımsız değerlendirme

Her bulgu dokümanın tamamına (v0.10), ilgili yerde Ürün Gereksinimleri'ne (`02` v0.73), karar kaydına ve önceki turların raporlarına karşı okundu — ikinci model bunları görmedi, parçalı çağrılar dokümanın öteki bölümlerini de görmedi. Altı ham bulgu beş ayrı bulgudur: tek parça çağrının BULGU-2'si ile parça 4'ün bulgusu aynı yer ve iddiadır. İkisi önceki turlarda okunmuş bir iddianın dönüşüdür (tabloda **dönen**); hiçbiri skill'in "tekrar eden RET" tanımına girmez — önceki turlarda en az iki kez gerekçeyle reddedilmiş değildir. Üç sabit tekrar eden RET (Google'ın kalıcı kimliği, ödemesi alınmamış siparişin üç yılı, yedeğe ulaşmamış penceredeki silmeler) bu turda dönmedi. **Şiddet** çıkış kuralının ölçüsüyle yazılır (5. turun raporunun §3'ü): yüksek — mevzuata aykırılık ya da para veya hak kaybı; orta — çıkışsız akış ya da dokümanın koyduğu bir güvencenin mekanizmasının eksik olması; düşük — iki yerin metin farkı ya da metin eksikliği.

| # | Karar | Şiddet · sınıf | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|---|
| Tek · BULGU-2 = P4 · BULGU-1 | ✅ KABUL | **Orta** — dokümanın koyduğu bir güvencenin mekanizması eksik (ölçünün (4). sınıfı; sonucu ele geçirmede verinin kaybı) | **Güvence yazılı, mekanizma onu sağlamıyordu.** 7.6.6 "Böylece sunucu ele geçirilse de yedek oradan silinemez" der ve güvenceyi anahtarın yetkisine bağlar: anahtar güncel nesneyi siler, eski sürümü silemez (K-879, K-937 (3)). Aynı madde kovada eski sürümleri otuz günde silen yaşam döngüsü kuralını kurar (K-937 (4)). Sürüm tutan kovada güncel nesneyi silmek bir silme işareti bırakır, üzerine yazmak yeni bir sürüm — önceki sürüm her iki durumda eski sürüm olur ve aynı kuralla otuz günde düşer. Sunucuyu ele geçiren, eski sürümü silme yetkisi olmadan bütün yedeği otuz günde kalıcı olarak yok edebilir. Nesne kilidinin uyum kipi (7.6.6'nın yedek yolu) eski sürümün silinmesini engeller, silme işaretini engellemez — aynı sınır. Güvence "silinemez" değil "otuz gün geri getirilebilir"dir. K-937 sürüm kimliğiyle silmeyi tartmıştı; silme işaretinin bıraktığı eski sürümün aynı kuralla düşmesi hiçbir turda, audit'te ve deep review'da okunmamıştı (raporlarda "silme işareti" geçmiyor). **İki koşum bağımsız olarak aynı iddiayı getirdi.** **Modelin önerisi elendi:** sunucudaki anahtardan silme yetkisini almak pgBackRest'in ve dosya yedeği aracının saklamasını durdurur — ikisi de eski yedekleri kendileri siler (bilgim tarihli; D-01'de doğrulanır) — ve üzerine yazma yetkisi kaldıkça önceki sürüm yine eski sürüm olur; sınır kalkmaz. Eski sürümleri daha uzun tutmak imha edilen verinin yedekteki süresini uzatır (7.4.5, K-876; 7.6.10 doksan günü aynı gerekçeyle eler). Kovanın sunucunun dışından ayrı bir listeleme anahtarıyla düzenli denetlenmesi yeni bir sır ve kurulum başına düzenli elle iş getirir. Güvence otuz günle sınırlı yazıldı ve kalan risk adıyla kayda girdi. Firmanın verisinin kaybına dokunan bir güvenceyi daraltır — karar satırı ⚠. | 7.6.6: silme işareti ve yeni sürüm önceki sürümü eski sürüm bırakır, eski sürüm otuz günde düşer; silinen ya da üzerine yazılan yedek otuz gün eski sürümlerden geri getirilir — silme işaretleri ve sonraki sürümler firmanın hesabının yönetim girişinden kaldırılır; **kalan risk:** fark edilmeyen silme otuz günün sonunda kalıcı olur, uyarılar ve prova sunucunun üzerindedir; nesne kilidi silme işaretini engellemez · 7.6.10: üç elenen · 1.2.30 · 9.2.3 · §12 MK-26 · §7.6'nın Kaynak satırı · `DEPLOY_RUNBOOK`'un yedek park satırı (etiket ve metin). **K-961** (öneriyle kaydedildi — ⚠); K-879 ve K-937'ye geri işaret. |
| P2 · BULGU-2 | ⚠️ KISMİ | **Düşük** — metin eksikliği (§4.3.1'in kalıbı 4.5.2'de yazılı değildi) | **Yazılı olmayan yalnız işlemin sınırıydı; para kaybı doğmaz.** 4.5.2 bildirimin kaydedilip "hemen" alındı denmesini ve işlenmenin kuyruktan yapılmasını yazıyordu; kayıt ile iş satırının aynı işlemde yazıldığını ve yanıtın işlem bittikten sonra verildiğini yazmıyordu. Kalıp dokümanda iki yerde var: dış yan etki geçişle aynı işlemde iş kuyruğuna yazılır (4.3.1, outbox) ve onaylanan işlem diske yazılmadan başarı dönmez (7.1.2) — 4.5.2 onlara bağlı değildi. **Modelin para kaybı iddiası reddedildi:** bildirim işlenmeden kalsa da kart hattında kendiliğinden iptal son sorguyla yapılır ve sorgu yanıtsızken sipariş iptal edilmez (4.5.3, 4.3.2; `02 §6.1.2`, §4.3) — ödemesi alınmış sipariş iptal edilmez. Yinelenen bildirim için öneri zaten yazılıdır (işlem kimliği tekildir). | 4.5.2: kayıt onu işleyecek iş satırıyla aynı veritabanı işleminde yazılır (§4.3.1) ve "alındı" işlem diske yazıldıktan hemen sonra denir (§7.1.2). Yeni karar yok — MK-37 ve MK-39'un kalıbıdır. |
| P5 · BULGU-1 — **dönen** (4. turda RET) | ⚠️ KISMİ | **Düşük** — iki yerin metin farkı (9.2.3 ile 7.4.5) | **İddianın özü 4. turdaki gerekçeyle reddedilir; dönüşün sebebi olan cümle düzeltildi.** İmha edilen verinin yedekteki kopyalarının süresi 7.4.5'in genel kuralıdır ve dosya yedeğinin anlık görüntülerini açıkça sayar (K-876; 4. turun raporu §3); kayıt yedeğine ayrı imha — modelin önerisi — yedeğin anlık görüntü biçimini değiştirir ve elenir. Ama aynı iddia iki turda (4. ve 6.) aynı cümleye dayanarak döndü: 9.2.3'ün "otuz gün … kaydın yedekteki süresi değildir" cümlesi 7.4.5'e işaret etmiyor, P-45'i dolan kaydın yedekteki kopyasını yazmıyordu — §7'yi görmeyen parça 5 cümleyi P-45'in tek süre olduğu biçiminde okudu. İki yer çelişmez ama 9.2.3 kendi başına okunduğunda eksikti; en küçük düzeltme işarettir. | 9.2.3: P-45'i dolup imha işinin sildiği kayıt, silinmeden önceki anlık görüntülerde imha edilen her verinin yedekteki penceresiyle kaybolur (§7.4.5). Yeni karar yok. |
| Tek · BULGU-1 — **dönen** (5. turda KISMİ) | ❌ RET | — | **5. turun aynı yer ve iddiasıdır; düzeltmesi yapıldı ve önerinin kalanı K-888'in elediği seçenektir.** Aktarımın yavaşlaması ya da arşiv komutunun sürmesi 5. turda "hata vermeden geride kalan aktarım" olarak yazıldı: geride kalan aktarım durmuş sayılır, pencere aktarımın durduğu ana kadar uzar ve koşul bekleyen günlük dosyalarını da sayar (7.6.2, 7.6.9, 9.4.4, 9.4.10). Bir dakika aktarım çalışırken geçerlidir (7.6.2) — 16 MB'lık dosyanın taşınması olağan bağlantıda saniyelerdir ve tasarım değerinin içindedir (konvansiyon 5). "Senkron uzak dayanıklı yazım" §0.13'ün ve 7.6.2'nin eklemediği ikinci eşzamanlı kopyadır — proje sahibi pencereyi bu seçeneğe karşı kabul etti (K-888); "üst dokümanda gerçek RPO'ya göre düzeltin" önerisi o kararın yeniden açılmasıdır. Ölçünün dışında kalan iddia yeni bilgi taşımıyor. | — |
| P2 · BULGU-1 | ❌ RET | — | **Kesintinin ölçülmesi bu dokümanın tasarım değeridir ve senaryo ölçünün nadir kenar durumudur.** Ürün kuralı havalenin süresi kesinti sırasında dolarsa iptali erteler ve tespiti `05`'e bırakır (`02 §4.3`, §3.34.7, §3.21.8; K-666, K-667); 5.4.1 kesintiyi nabızla ölçer ve iki dakikayı aşan boşluğu kesinti sayar — otuz saniyelik nabızda dört kaçan atım; her nabız temelli ölçümün bir çözünürlüğü vardır, sıfır eşik yoktur. Erteleme kuralının gerekçesi firmanın "ödendi" işaretini kesinti boyunca koyamamasıdır (`02 §4.3`); iki dakikadan kısa bir kesintinin havale süresinin son anına denk gelmesi ve firmanın tam o aralıkta işaret koymak istemesi gerekir — müşterinin bankadan havalesini site kesintisi engellemez. Böyle bir siparişte firmanın yolu yazılıdır: gelen para geri ödenir ya da müşteri yeni sipariş verir (`02 §3.21.8`; 5.4.6'nın kalan riski aynı yolu anar). Öneri — kesintiyi her durumda belirleyen kalıcı bir kontrol — ölçünün "savunmada derinlik" sınıfıdır. | — |

**Dağılım:** beş ayrı bulgu — 1 KABUL · 2 KISMİ · 2 RET (ikisi dönen iddia: biri KISMİ, biri RET). **Şiddet:** bir orta (Tek · BULGU-2 = P4 · BULGU-1), iki düşük. **%100 kabul ya da ret değil:** RET'lerin ikisi de somut yerlere dayanır (7.6.2, 7.6.9, 9.4.4, K-888; `02 §4.3`, §3.21.8); KISMİ'lerin ikisinde modelin önerisi elendi ve en küçük metin eklendi; KABUL'de modelin önerisi elendi ve güvence mekanizmaya göre yeniden yazıldı. **Bir bulgu yeni karar doğurdu** (K-961 ⚠); ikisi metin hizalamasıdır.

## 4. Ek bulgular

- **Aynı kalıbın taraması (Tek · BULGU-2 = P4 · BULGU-1):** "silinemez", "silmeye karşı", "eski sürüm", "güncel nesne" ve "yedek oradan" `05`'te ve sonraki dokümanlarda tarandı. Güvenceyi taşıyan yüzler 1.2.30, 7.6.6, 9.2.3, §12 MK-26 ve `DEPLOY_RUNBOOK`'un yedek park satırıdır — hepsi otuz günlük sınırı aldı. 2.2.6 (sağlayıcının ölçütü), 6.x.10 (anahtarların döndürülmesi) ve 7.6.10'un "yalnız sürüm tutma" eleneni yetkiyi anlatır ve doğrudur — değişmedi. 7.4.5'in "nesne depolamanın eski sürümleri — saklama süresi dolunca kaybolur" cümlesi imha tarafını anlatır ve yeni sınırla uyumludur. 9.2.3'ün kalan riski — "ele geçiren eski kayıtları da silerse onlar silinmeden önceki anlık görüntülerde otuz gün daha durur" — yeni sınırın aynısıdır. Üst dokümanlar (`01`, `02`, `03`, `04`, `10`) yedeğin silmeye karşı korunmasını yazmaz — `10 §4.1` ÖK-2 yalnız yedeğin firmanın hesabında durmasını ister; geri besleme yok.
- **Aynı kalıbın taraması (P2 · BULGU-2):** "alındı", "yanıt" ve "200 döner" `05`'te tarandı; sağlayıcıya yanıt veren tek yer 4.5.2'dir. §12 MK-39'un gerekçe hücresi "önce kaydedilen sağlayıcı bildirimi" der — değişmedi.
- **Aynı kalıbın taraması (P5 · BULGU-1):** P-45 ile yedeğin otuz gününü birlikte anan yerler 7.4.5 ("P-45 yedeğin otuz gününden uzundur") ve 9.2.3'tür; ikisi artık birbirine işaret eder. 9.2.1'in "kaydın yazıldığı andan P-45 kadar saklanır" cümlesi sunucudaki kaydın süresidir — yedekteki kopyaların süresi bütün imhaların ortak kuralıdır (7.4.5) ve 9.2.1'e ayrıca yazılmadı.
- **Kendi okumam — güvence cümleleri:** dokümanın kesin güvence cümleleri ("kaybolmaz", "silinemez", "değiştirilemez", "açılamaz") `05`'te tarandı. Bu turun bulgusu dışında 7.1.2 (onaylanmış işlem), 4.3.1 (kuyruk satırı), 6.x.12 (döndürülmeyen şifreleme anahtarı) ve 10.1.6'nın "onaylanmış işlem kaybolmaz" hücresi kendi mekanizmalarına bağlı ve doğrudur. Ölçünün dört sınıfından birine giren başka bir sorun saptanmadı.
- **Mekanik tarama:** Kaynak satırı betiği (MK- kimliklerini de K numarası gibi yakaladığı için 5. turdaki gibi okunur) değişiklikten önce ve sonra aynı sonucu verir; tek fark §12'nin gerekçe hücresine giren K-961'dir (§12'nin Kaynak satırı "K numaraları gerekçe hücresindedir" der). §7.6'nın gövdesi K-961'i anmaz, Kaynak satırı onu taşır. Park satırının etiketi (`DEPLOY_RUNBOOK`, K-961) ile K-961'in etki sütunu iki yönde eşleşir. Başlık sürümü, kalite döngüsü notu ve dosya sonu dipnotu v0.11'i gösterir. Karar kaydında çift numara yok; K-855'ten bu yana `(öneriyle kaydedildi — ⚠)` taşıyan satırlar betikle sayıldı — 34 — ve §12.1'in listesiyle aynı. Değişen dosyalarda CR yok; diff'in eklenen satırlarında `$[0-9]` artığı yok.
- **Önceki turlardan açık kalanlar:** yok. Üç sabit tekrar eden RET bu turda dönmedi.

## 5. Kullanıcı onay checklist'i

K-855 kararıyla (Aşama 4 boyunca ⚠ öneriyle kayıt, CI yeşilse `docs:` PR merge yetkisi ve yönetici + alt ajan düzeni) alt ajan değerlendirdi ve uyguladı (`05` v0.11). Bir karar öneriyle kaydedildi: **K-961 ⚠** — yedeğin silmeye karşı koruması otuz günle sınırlıdır; sunucudan silinen ya da üzerine yazılan yedek otuz gün eski sürümlerden geri getirilir, fark edilmeyen silme otuz günün sonunda kalıcı olur (kalan risk). ⚠ listesi 34. Geri besleme yok. Park satırı: `DEPLOY_RUNBOOK` (Yedek, kurtarma ve prova — K-961). Geri işaret: K-879, K-937.

- [x] Tek · BULGU-2 = P4 · BULGU-1 (kabul, orta — K-961 ⚠: koruma otuz günle sınırlı, kalan risk adıyla)
- [x] P2 · BULGU-2 (kısmi, düşük — 4.5.2: kayıt ve iş satırı aynı işlemde, "alındı" işlemden sonra)
- [x] P5 · BULGU-1 (kısmi, düşük, dönen — 9.2.3 7.4.5'e işaret eder)
- [x] Tek · BULGU-1 (ret, dönen — 5. turun düzeltmesi ve K-888)
- [x] P2 · BULGU-1 (ret — kesintinin ölçüsü tasarım değeri, nadir kenar durumu)
- [ ] Etki yansıtma (Faz 5) — döngü TEMİZ döndükten sonra, son turun raporunda

**Hukuki kontrol** (2026-10-17): bulguların hiçbiri yasal dayanak taşımıyor ve hiçbir çağrı web araması yapmadı. P5 · BULGU-1 kişisel verinin gereğinden uzun saklanmasını öne sürer; yedekteki kopyaların süresi K-876'nın kararıdır (⚠; 7.4.5) ve 4. turda da böyle okundu. Düzeltmeler yeni hukuki içerik eklemez.

**Yakınsama ölçüsü:** ayrı bulgu 7 → 5 → 6 → 6 → 5 → 5; kabul edilen ya da kısmen kabul edilen 7 → 3 → 3 → 3 → 4 → 3; yeni karar 2 → 0 → 1 → 2 → 2 → 1 (bu turun kararı ⚠); dönen iddia 0 → 1 → 3 → 2 → 1 → 2. TEMİZ çağrı sayısı 0 → 1 → 1 → 1 → 2 → 2. Doküman 316 896 bayttan 320 460 bayta büyüdü; tablolara satır girmedi.

**Orta ve yüksek şiddetli yeni bulguların yeri:** 3. tur — güncellemede yedekten dönüşün denetiminin kapsamı (§8.7.4; K-956) · 4. tur — sipariş onayında önceki siparişin iptalinin sırası (§4.2.2; K-957) ve yedekten dönüşün denetiminin anı (§8.7.4; K-958) · 5. tur — geride kalmış kurulumda sürüm atlama (§8.7.1; K-960) · 6. tur — yedeğin silmeye karşı korunmasının sınırı (§7.6.6; K-961). Beş bulgunun dördü iki bölgededir — güncellemede göç ve yedekten dönüş (§8.7) ile yedeğin korunması (§7.6) — ve dördü de bir güvence cümlesinin ("kaybolmaz", "geri alınır", "silinemez") mekanizmaya karşı okunmasıdır. Parça 1 (başlık notu, §0–§3) 4., 5. ve 6. turda TEMİZ; parça 3 (§6) bu tur ilk kez TEMİZ.

## 6. Sonuç ve 7. tur

**Sonuç: TEMİZ DEĞİL** (ölçünün dört sınıfında) — altı çağrının ikisi TEMİZ; beş ayrı bulgunun biri kabul edildi, ikisi kısmen uygulandı, ikisi reddedildi. Faz 5 (etki yansıtma) bu turda koşmadı; TEMİZ turunda koşar.

**Çıkış kuralı bu turda uygulanmadı.** Skill'in Faz 4 madde 5'inin ikinci kuralı kabul edilen bulguların **hepsinin** düşük şiddette iki yerin metin farkı olmasını ister. İki KISMİ bu koşulu sağlar (P2 · BULGU-2, P5 · BULGU-1); Tek · BULGU-2 = P4 · BULGU-1 sağlamaz — dokümanın koyduğu bir güvencenin ("sunucu ele geçirilse de yedek oradan silinemez") mekanizması onu sağlamıyordu, düzeltme güvencenin kendisini daralttı ve bir kalan riski kabul etti. Kural bu durumda döngüyü sürdürür. Çıkış kuralını uygulamayan tur karar satırı gerektirmez (K-849 kalıbı yalnız kural uygulandığında kayda girer).

**7. tur** aynı istemle (1. turun raporunun §1'i, değiştirilmeden), aynı şablonla ve iki koşumla (K-953) yürür; parça sınırları bölüm başlıklarıdır. Önceki turların raporları, karar kaydı ve audit raporları verilmez (K-431). Tur, altı çağrının altısı da `SONUÇ: TEMİZ` dönerse — ya da yalnız tekrar eden RET'ler kalırsa — TEMİZ'dir. **7. turda tekrar eden RET sayılacaklar** (aynı yer ve iddiayla dönerlerse): Google hesabının kalıcı kimliği (§6.1.4) · ödemesi alınmamış siparişte kişisel verinin üç yılı (§7.4.3 (4), §4.4.4) · yedeğe ulaşmamış penceredeki silmeler ya da ayrı bir silme deposu (§7.4.5; K-888, K-939, K-955). **Dönen iddialar** — arşiv zaman aşımının bir dakikası (§7.6.1; 5. turda KISMİ, bu tur RET) ve sistem kayıtlarının yedekteki süresi (§9.2.3; 4. turda RET, bu tur KISMİ) — henüz iki kez gerekçeyle reddedilmedi; dönerlerse kendi gerekçeleriyle okunur. **7. turda çıkış kuralı uygulanabilir:** kabul edilen bulguların hepsi düşük şiddette iki yerin metin farkıysa bulgular uygulanır, aynı kalıp dokümanın tamamında taranır, döngü "TEMİZ — çıkış kuralıyla, 7 tur" diye kapanır, kuralın uygulanışı bir karar satırıyla kaydedilir (K-849 kalıbı) ve sınıfın kalan riski karar kaydının §12.1'inde aşamanın checkpoint'ine devredilir; kabullerden biri orta ya da yüksek şiddetteyse döngü sürer.

**Öneri (7. turdan sonrası için):** son dört turun orta şiddetli yeni bulgularının dördü §7.6 ile §8.7'de çıktı ve her biri bir güvence cümlesinin mekanizmaya karşı okunmasıydı; ikinci model bu iki bölgeyi her turda biraz daha derinden okuyor. 7. tur da bu iki bölgede orta şiddette yeni bir bulgu getirirse 8. turu körlemesine koşmak yerine §7.4.5–§7.6 ve §8.6–§8.7'nin güvence cümlelerinin tek tek mekanizmasına karşı okunduğu hedefli bir mercek (audit'in teknik doğruluk merceği, iki şüpheciyle) önerilir; mercek bulduklarını tek adımda kapatır ve döngü ondan sonraki turla sürer. 7. tur bu bölgelerde yeni bir orta bulgu getirmezse yakınsama kendiliğinden sürüyor demektir ve ek adım gerekmez.
