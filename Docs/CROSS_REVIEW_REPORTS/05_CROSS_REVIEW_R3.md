# Cross-Review — 05 Technical Architecture (Tur 3)

**Tarih:** 2026-10-17 · **Hedef:** `Docs/05_TECHNICAL_ARCHITECTURE.md` v0.7 (girdi commit'i `0d18c87`, 306 709 bayt, 1 482 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren uygulanır (K-952) · **Koşum:** iki biçimde, altı çağrı — tek parça çağrı ve beş parçalı kontrol koşumu (K-953); turun sonucu ikisinin birleşimidir · **Girdi:** yalnız doküman ve boş şablonu (`58cf0b9` — Aşama 4 açılmadan önceki v0.1; park blokları çıkarılmış, 87 satır), stdin'den tek metin, boş ve izole bir çalışma klasöründe; karar kaydı, önceki turların raporları, Ürün Gereksinimleri, Kullanıcı Akışları, Arayüz Tanımları, MVP Kapsamı ve audit/deep review raporları verilmedi (K-431)

## 1. Koşum

**İstem ve şablon** 1. turunkidir, değiştirilmeden (`05_CROSS_REVIEW.md` §1'in alıntı blokları — istem ve parça notu). Girdiler aynı betikle yeniden kuruldu: `istem + "=== DOKÜMAN ===" + güncel doküman + "=== ŞABLON ===" + şablon`; parçalı çağrılarda istemden sonra parça notu, parçada başlık notu. **Komut** 1. turunkidir: `codex exec -s read-only -C <boş klasör> --skip-git-repo-check --ephemeral --ignore-user-config --color never -m gpt-5.6-terra -c 'model_reasoning_effort="high"' -o <çıktı> - < <girdi>`. Altı çağrı paralel koştu.

**Parça sınırları** 1. turunkidir — bölüm başlıkları (K-953); satır numaraları v0.7'nindir.

| Parça | Bölümler | Satırlar | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|---|
| — | Tek parça — dokümanın tamamı | 1–1482 | 314 696 bayt | 129 385 | 143 sn | 1 bulgu |
| 1 | Başlık notu · §0 · §1 · §2 · §3 | 1–437 | 101 150 bayt | 51 169 | 134 sn | 1 bulgu |
| 2 | §4 durum ve olaylar · §5 arka plan işleri | 438–613 | 76 270 bayt | 42 587 | 47 sn | `SONUÇ: TEMİZ` |
| 3 | §6 güvenlik (§6.x dahil) | 614–819 | 89 668 bayt | 64 383 | 66 sn | 1 bulgu |
| 4 | §7 veri katmanı | 820–954 | 79 791 bayt | 69 903 | 160 sn | 2 bulgu |
| 5 | §8 ortamlar · §9 gözlemlenebilirlik · §10 hata toleransı · §11 ölçeklenebilirlik · §12 özet | 955–1482 | 145 831 bayt | 75 701 | 151 sn | 1 bulgu |

**Web araması:** parça 1 ve 2 aramadı; tek parça çağrı 5651 sayılı Kanun m.5 ile KVKK'nın çerez rehberini, parça 3 Google'ın OpenID Connect belgesini ve çerez rehberini, parça 4 ve 5 6563 sayılı Kanun m.11/3'ü aradı (koşum kayıtlarındaki `web search` satırları). Kamuya açık mevzuat ve araç belgesi proje bilgisi değildir (1. turun raporu §1); açık olduğu yine yazılır.

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**Tek parça çağrı:**

> ```text
> BULGU-1
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §6.7.1, §6.7.5
> Alıntı: “**Bütün çerezler birinci taraf ve zorunludur**” / “Tarayıcıların izin verdiği en uzun ömür — **400 gün** — ve her ziyarette yenilenir”
> Sorun: Kalıcı misafir sepeti çerezi, kullanıcıyı ziyaretler arasında 400 gün tanıyıp sunucudaki sepetine bağlar. Belgenin kendisi bunu zorunlu çerez sayarak rıza seçeneği olmadan yerleştirir. KVKK çerez rehberindeki alışveriş sepeti örneği oturum süresiyle sınırlıdır; ziyaretler arası kalıcılık, kullanıcının o an talep ettiği sepet hizmeti için zorunlu değildir. Bu, kişisel verinin hukuki dayanak olmadan işlenmesine yol açabilir.
> Öneri: Misafir sepeti çerezini tarayıcı oturumuyla sınırlayın; 400 günlük kalıcılık korunacaksa bunu zorunlu çerez sınıfından çıkarıp açık rıza mekanizmasıyla, rıza yokken çerezi yazmayacak/okumayacak şekilde tasarlayın.
>
> SONUÇ: 1 BULGU
> ```

**Parça 1 — başlık notu, §0–§3:**

> ```text
> BULGU-1
> Kriter: Güvenlik
> Seviye: Orta
> Yer: §3.6.1; §1.2.31
> Alıntı: “dış servisten gelen istek — bugün yalnız ödeme sağlayıcısının bildirimi (§4.5.2) — bağlantı katmanında imzasıyla doğrulanır”
> Sorun: Bu, dış erişilebilirlik kontrolünün durum ucuna yaptığı isteği dışarıda bırakır: §1.2.31 bu servisin “Durum ucunu beş dakikada bir yoklar” dediğini yazmaktadır. Doküman, dışarıdan gelen makine isteklerinin yalnız ödeme bildirimi olduğu ve imzalı olduğu güvence­siyle çelişmektedir. Uygulayıcı, ya izlemeyi çalıştıramaz ya da tanımlı istisna olmadan bir dış ucu açar. Bu, doküman içi çelişkidir (4).
> Öneri: Durum ucunu §3.6.1’de açıkça ikinci dış makine isteği olarak ekleyin; imzasız erişim zorunluysa bunu açık istisna yapıp ucun yalnız sağlık sonucu döndürdüğünü ve kişisel/işletimsel ayrıntı vermediğini belirtin. İlgili hız sınırı ve sahtecilik koruması istisnalarını aynı kararda tanımlayın.
>
> SONUÇ: 1 BULGU
> ```

**Parça 2 — §4–§5:**

> ```text
> SONUÇ: TEMİZ
> ```

**Parça 3 — §6:**

> ```text
> BULGU-1
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §6.1.4
> Alıntı: "Yalnız Google'ın doğrulanmış verdiği e-posta kabul edilir ve ürün Google'dan yalnız adı ve e-postayı alıp saklar — Google hesabının kalıcı kimliği saklanmaz"
> Sorun: Google OIDC’de e-posta kalıcı ve tekil bir hesap kimliği değildir; e-posta değişebilir. Kalıcı `sub` saklanmadığı için giriş eşlemesi e-postayla yapılmak zorunda kalır; aynı e-postayı daha sonra doğrulayan farklı bir Google hesabı mevcut Shopfolio hesabına bağlanabilir. Bu, sipariş ve adres gibi kişisel verilere yetkisiz erişim doğurur (1). Google da e-postanın kimlik olarak kullanılmamasını, değişmeyen `sub` değerinin kullanılmasını belirtir. [Google OIDC referansı](https://developers.google.com/identity/openid-connect/reference)
> Öneri: Google giriş bağını doğrulanmış `iss` + `sub` çiftiyle saklayıp bununla eşleştirin. E-posta değişikliğini mevcut hesaba otomatik bağlamak yerine, oturum açılmış hesabın yeniden doğrulanmasıyla açık bir bağlama/değiştirme akışına alın.
>
> SONUÇ: 1 BULGU
> ```

**Parça 4 — §7:**

> ```text
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §7.4.3 (4)
> Alıntı: "ödemesi hiç alınmamış siparişin (Z-41) satırında üç yıl dolunca alıcı ve olayın değerleri boşaltılır"
> Sorun: 6563 sayılı Kanun m.11/3, elektronik ticaret kapsamındaki iş ve işlemlere ilişkin bilgi, belge, defter ve elektronik kayıtların işlem tarihinden itibaren on yıl saklanmasını zorunlu tutar. Ödemesiz sipariş de bu kapsamda oluşmuş sipariş ve iletişim kaydıdır; alıcı ve olay değerlerinin üçüncü yılda silinmesi, firmanın on yıllık saklama ve Bakanlığa eksiksiz ibraz yükümlülüğünü ihlal eder.
> Öneri: Ödemesiz siparişe ilişkin gerekli kayıtları ve ilgili kişisel veriyi de kanuni on yıl boyunca, yalnız yasal saklama amacıyla erişimi sınırlı olarak saklayın; on yıl sonunda imha edin. Bu, `02` ile çelişiyorsa K kaydı açılarak üst dokümana geri beslenmelidir.
>
> BULGU-2
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §7.4.5, §7.6.2
> Alıntı: "Yalnız yedeğe henüz ulaşmamış pencerede (7.6.2) yapılan silme, penceredeki öteki kayıtlar gibi kaybolur — uygulama kaydındaki satırı da diskle gider — ve silinen kayıt geri gelir."
> Sorun: Hesap veya iletişim talebi gibi anlık kişisel veri silmeleri, kalıcı disk kaybının kabul edilen penceresinde hem veritabanından hem yeniden-silme kaydından kaybolur. Kurtarılan sistem silinmiş kişisel veriyi yeniden aktif hâle getirir ve artık hangi kaydın tekrar silinmesi gerektiğini bilmez. Bu, silme talebinin/işleme sebebinin ortadan kalkmasının ardından verinin süresiz yeniden işlenmesine yol açabilir; kabul edilmiş veri kaybı penceresi bu hukuki aykırılığı ortadan kaldırmaz.
> Öneri: Anlık silmenin yeniden-uygulama kaydını sunucu dışındaki dayanıklı depoya yazın veya silmeyi kullanıcıya başarılı göstermeden önce bu kaydın sunucu dışına ulaştığını doğrulayın. Kurtarmada bu kayıt zorunlu olarak yeniden uygulanmalıdır.
>
> SONUÇ: 2 BULGU
> ```

**Parça 5 — §8–§12:**

> ```text
> BULGU-1
> Kriter: Hata toleransı
> Seviye: Yüksek
> Yer: §8.7.4
> Alıntı: "Yedekten dönüş yalnız bir göç veriyi bozduğunda ve **dönüş noktasından sonra sipariş tarafında hiçbir kayıt yazılmamışsa** yapılır."
> Sorun: Dönüşten sonra sipariş yazılmamış olması, başka kalıcı verilerin yazılmadığını göstermez. Yönetici ürün, fiyat, içerik veya ayar değiştirmiş; müşteri hesabında değişiklik olmuş olabilir. Bu durumda yedekten dönüş bu kayıtları sessizce geri alır; doküman bu yazımları koruyan ya da yeniden uygulayan bir mekanizma tanımlamaz. Bu, firma veya müşteri verisinin kaybına yol açan kural boşluğudur.
> Öneri: Yedekten dönüşü dönüş noktasından sonra hiçbir kalıcı yazım olmadığında mümkün kılın; aksi durumda yalnız ileri düzeltme göçü kullanın. Alternatif olarak, sipariş dışındaki tüm yazımları da eksiksiz yeniden uygulayacak doğrulanmış bir geri-yükleme/replay mekanizması tanımlayın.
>
> SONUÇ: 1 BULGU
> ```

## 3. Bağımsız değerlendirme

Her bulgu dokümanın tamamına (v0.7), ilgili yerde Ürün Gereksinimleri'ne (`02` v0.72) ve karar kaydına karşı okundu — ikinci model bunları görmedi, parçalı çağrılar dokümanın öteki bölümlerini de görmedi. Altı ham bulgu altı ayrı bulgudur; üçü önceki turlardan dönen iddiadır (tabloda **dönen** işaretli).

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| P5 · BULGU-1 | ⚠️ KISMİ | **Koşulun kapsamı ile denetimi ayrışıyordu; önerilen çözüm uygulanamaz.** 8.7.4 yedekten dönüşü "dönüş noktasından sonra sipariş tarafında hiçbir kayıt yazılmamışsa" yapar; sipariş tarafı §0.13'ün ve `02 §3.34.8`'in tanımıdır — siparişler, ödemeler, geçişler, kalem düzeyindeki kayıtlar, iletişim talepleri (KVKK talebi dahil), donmuş yasal metin sürümleri, işlem izi. Denetim ise yalnız geçiş kaydına ve işlem izine bakıyordu (K-938 (3)). Müşterinin iletişim talebi ikisine de yazılmaz — işlem izi yalnız yönetici işlemlerini taşır (4.1.4), geçiş kaydı siparişin durumlarını —; göçün başladığı andan sonra yalnız bir talep gelmişse denetim dönüşe izin verir ve yasal süresi işleyen başvuru kaybolur. Ölçünün (2). sınıfı (hak kaybı) ve (4). sınıfı (koşul ile denetim). **Modelin iddiasının ikinci yarısı** — ürün, içerik, ayar ve hesap değişikliklerinin geri gitmesi — kural boşluğu değil, §0.13'ün sonucudur: sıfır kayıp sipariş tarafınındır; ama 8.7.4 bunu yazmıyordu ve dönüşün eski bir ana dönüş olduğunu (7.4.5'in adımları) bağlamıyordu. **Önerinin ilk yolu uygulanmadı** — "dönüş noktasından sonra hiçbir kalıcı yazım yoksa": nabız veritabanına otuz saniyede bir yazar (5.4.1, 7.6.9), koşul hiç sağlanmaz ve yedekten dönüş fiilen kalkar; **ikinci yolu da** — sipariş dışındaki yazımları yeniden uygulayan mekanizma — her tablo için ayrı bir yeniden uygulama kaydıdır; veri bozan göç nadirdir ve ileri göç her zaman açıktır (K-938). K-938'in (3)'ünü değiştirdiği için karar satırı açıldı. | 8.7.4: denetim `02 §3.34.8`'in bütün kayıtlarına bakar · dönüş noktasından sonraki öteki yazımlar dönüşle geri gider; anlık silmeler 8.5.10'un komutuyla, süreye bağlı silmeler imha işiyle yeniden uygulanır, firma dönüşün anıyla bilgilendirilir (7.4.5) · §8.7'nin Kaynak satırı · §12 MK-74 · `DEPLOY_RUNBOOK`'un K-938 park satırı. **K-956** (öneriyle kaydedildi — ⚠); K-938'e geri işaret. |
| Tek · BULGU-1 | ⚠️ KISMİ | **İddia bilinçli bir kararın hukuki yorumuna itirazdır; dokümanda eksik olan dayanaktı.** Misafir sepetinin kalıcı çerezinin dayanağı K-950'nin kararıdır (öneriyle kaydedildi — ⚠): KVKK'nın Çerez Uygulamaları Hakkında Rehberi'nin oturumla biten sepet örneğinden ayrılır ve dayanağı satış sözleşmesinin kurulmasıdır (KVKK m.5/2-c); modelin iki önerisi — sepet çerezini oturuma çevirmek ve sepet için rıza istemek — K-950'de gerekçesiyle elendi (birincisi K-351'in kalıcı sepetini, Z-6'yı değiştirir; ikincisi çerez bandını sepete bağlar). Model rehberin aynı örneğini getirdi; yeni bilgi yok. **Gerçek olan:** `02 §12.2.5` dayanağı yazar, `05 §6.7` ise "Envanterin ürün kuralı `02 §12.2.5`'tedir" deyip dayanağı vermez — dokümanı tek başına okuyan için bilinçli karar görünmüyordu (2. turun K-951 bulgusuyla aynı kalıp). K-950 ⚠ listesindedir ve proje sahibine arşiv işaretinden önce gösterilir — ayrıca sorulacak çok kritik bir konu değildir. | 6.7.8: kalıcılığın dayanağı bilinçli karardır (K-950; `02 §12.2.5`) — KVKK m.5/2-c, açık rıza gerektirmez; iki seçenek elendi · §6.7'nin Kaynak satırına K-950. K-950 değişmedi; satırına geri işaret. |
| P1 · BULGU-1 | ⚠️ KISMİ | **İki yerin metin farkı; önerilen istisnalar gereksiz.** 3.6.1'in ilkesi "dış servisten gelen istek — bugün yalnız ödeme sağlayıcısının bildirimi —" diyordu; dış erişilebilirlik kontrolü de sunucunun dışındaki bir servistir ve durum ucunu yoklar (1.2.31, 9.4.11). Cümle harfiyen yanlıştı (ölçünün (4). sınıfı, düşük şiddet). "Uygulayıcı izlemeyi çalıştıramaz" iddiası tutmaz: durum ucu 9.4.1'de tanımlıdır — gövdesinde ayrıntı taşımaz —, 1.2.7 onu makine uçları arasında sayar. Önerilen hız sınırı ve sahteciliğe karşı koruma istisnaları gereksizdir: uç GET'tir (koruma yalnız POST'a uygulanır, 6.5.1) ve beş dakikada bir yoklama sayfa isteği sınırının (dakikada 300, 6.3.5) çok altındadır. | 3.6.1: ilke "ürüne veri getiren" dış servis isteğine daraltıldı; canlılık ve durum uçları ilkenin dışında — imza istemeyen, durum yazmayan, ayrıntısız açık GET uçları (§9.4.1, §9.4.11). |
| P3 · BULGU-1 — **dönen** | ❌ RET | **2. turun RET'inin aynı yer ve aynı iddiayla dönüşü; gerekçe geçerli.** Google hesabının kalıcı kimliğinin saklanmaması bilinçli karardır (K-900; `02 §8.1.2`, §12.2.8) ve "kalıcı kimlik saklansın" seçeneği elendi. Senaryo — e-posta adresinin başka bir Google hesabına geçmesi — üründe yeni bir yol açmaz: hesap e-postaya bağlıdır ve adresi eline geçiren şifre sıfırlamayla (`02 §3.13.8`; 6.1.5'in son cümlesi) aynı hesaba zaten girer; Google'ın kalıcı kimliği bu yolu kapatmaz, yalnız Google ile girişi farklılaştırır. Modelin yeni eklediği "e-posta değişikliği için açık bağlama akışı" da aynı varsayıma — hesabın anahtarının e-posta olmamasına — dayanır. **İkinci ret:** 4. turda aynı yer ve iddiayla dönerse tekrar eden RET'tir. | — |
| P4 · BULGU-1 — **dönen** | ❌ RET | **2. turda üç parçanın getirdiği iddianın dönüşü; doküman artık kararı ve elenen seçeneği yazıyor.** Ödemesi hiç alınmamış siparişte kişisel verinin üç yılda silinip kaydın on yıl kalması proje sahibinin kararıdır (K-951, "1 · On yıl, karma"): soru 6563 m.11/3'ün tam metniyle soruldu; modelin önerisi — ödeme sonucundan bağımsız kişisel veri de on yıl — elenen seçenekti. 2. turda §7.4.3 (4)'e gerekçe ve elenen seçenek yazıldı; model bunu görüp yine aynı yorumu getirdi — yeni bir hukuki bilgi yok (hüküm metni modelin alıntısıyla aynı). Hüküm işlemin "elektronik kayıtlarını" saklatır; işlemin kaydı on yıl durur, kişisel verinin de saklanıp saklanmayacağı yorumdur ve proje sahibi kararlaştırdı. Alt ajan proje sahibinin kararını değiştiremez. **2. turda KISMİ'ydi (öneri reddedildi, gerekçe yazıldı); bu ikinci ret** — 4. turda dönerse tekrar eden RET'tir. | — |
| P4 · BULGU-2 — **dönen** | ❌ RET | **Kabul edilmiş pencerenin içindedir ve nadir bir kenar durumudur.** Sunucunun diskinin kalıcı kaybında yedeğe ulaşmamış penceredeki (en çok bir dakika; aktarım durmuşsa daha uzun) her yazım kaybolur — silmeler dahil; 7.4.5 bunu 2. turda 7.6.2'nin kabul edilmiş penceresine bağladı ve pencere proje sahibinin kararıdır (K-888; `02 §3.34.8` onu bütün kayıtlar için yazar; K-939). Senaryo iki koşulun çakışmasını ister — müşterinin silmesinin diskin kalıcı kaybından önceki son dakikaya düşmesi ve diskin kalıcı kaybı; sağlayıcı diski kendi tarafında yedekli tutar (§0.5) — ölçünün dışında bir kenar durumudur. Önerinin iki yolu da elenmişti: sunucu dışı ayrı silme deposu (1. ve 2. tur; K-955 — ikinci bileşen ve sır yüzeyi, §0.2) ve silmeyi sunucu dışına ulaşana kadar bekletmek — 7.6.2'nin eklemediği eşzamanlı ikinci kopyadır (§0.13). **Önerinin üçüncü, iddianın ikinci dönüşü** (1. tur: eski bir ana dönüş, KISMİ; 2. tur: diskin kaybı, KISMİ; bu tur RET). | — |

**Dağılım:** altı ayrı bulgu — 0 KABUL · 3 KISMİ · 3 RET. **%100 kabul değil, %100 ret değil:** üç RET'in üçü de dönen iddiadır ve somut yere dayanır (K-900 ve `02 §3.13.8`; K-951 ve §7.4.3 (4); K-888, K-939, K-955 ve 7.4.5); üç KISMİ'nin üçünde de modelin önerisi reddedildi — K-950'nin kararı korundu, gereksiz istisnalar eklenmedi, uygulanamaz koşul (hiçbir kalıcı yazım) yerine kapsam `02 §3.34.8`'e hizalandı. **Bir bulgu yeni karar doğurdu** (K-956, ⚠); ikisi metin hizalamasıdır.

## 4. Ek bulgular

- **Aynı kalıbın taraması (P5 · BULGU-1):** "geçiş kaydında ve işlem izinde" yalnız 8.7.4'te, `DEPLOY_RUNBOOK`'un K-938 park satırında ve K-938'in satırında geçiyordu — ilk ikisi güncellendi, kayıt satırı salt geri işaret aldı. "Sipariş tarafında" geçen öteki yerler — 4.1.4, 7.1.2, 8.7.6, §0.13, MK-74 — kuralı ya da gerekçesini anlatır, denetimi saymaz; MK-74'ün gerekçe hücresi K-956'yı aldı.
- **Gözlem (bulgu değil):** göç bozulmasından eski bir ana dönüşte şifre sıfırlamasının sildiği oturumlar da geri gelir — 7.4.5'in anlık silmeleri kişisel veri silmeleridir, oturumu saymaz. Göç bozulması, yedekten dönüş ve aynı aralıkta güvenlik amaçlı bir şifre sıfırlamasının çakışmasını ister; ölçünün dışında bir kenar durumudur, yeni kural yazılmadı.
- **Aynı kalıbın taraması (Tek · BULGU-1):** "400 gün" ve "misafir sepeti" `05`'te 6.7.5, 6.7.8, 6.7.11 ve §5.2.9'da geçer; dayanak tek yerde, 6.7.8'de durur. Ürün Gereksinimleri dayanağı zaten `02 §12.2.5`'te taşır — geri besleme yok.
- **Aynı kalıbın taraması (P1 · BULGU-1):** "imzalı makine uçları" 6.3.5, 6.5.1 ve 1.2.7'de geçer; üçü de "imzalı uç — bugün ödeme sağlayıcısının bildirimi" der ve doğrudur — genelleme yalnız 3.6.1'in "dış servisten gelen istek" ifadesindeydi.
- **Mekanik tarama:** Kaynak satırı betiği (MK- kimliklerini dışarıda bırakarak) değişiklikten önce ve sonra aynı sonucu verir; tek fark §12'nin gerekçe hücresine giren K-956'dır (§12'nin Kaynak satırı "K numaraları gerekçe hücresindedir" der). §6.7'nin ve §8.7'nin gövdesine giren K-950 ve K-956 Kaynak satırlarında. Başlık sürümü, yazım durumu notu ve dosya sonu dipnotu v0.8'i gösterir. Karar kaydında çift numara yok; K-855'ten bu yana `(öneriyle kaydedildi — ⚠)` taşıyan satırlar betikle sayıldı — 32 — ve §12.1'in listesiyle aynı.
- **Ciddi bir sorun görülmedi.** Değerlendirme sırasında ikinci modelin listesi dışında ölçünün dört sınıfından birine giren bir sorun saptanmadı.
- **Önceki turlardan açık kalanlar:** yok. 2. turun iki RET'i bu turda döndü ve ikinci kez reddedildi; 2. turun K-951 KISMİ'si (aynı iddia) RET oldu; ayrı silme deposu önerisi üçüncü kez geldi.

## 5. Kullanıcı onay checklist'i

K-855 kararıyla (Aşama 4 boyunca ⚠ öneriyle kayıt, CI yeşilse `docs:` PR merge yetkisi ve yönetici + alt ajan düzeni) alt ajan değerlendirdi ve uyguladı (`05` v0.8). Bir karar öneriyle kaydedildi: **K-956** (⚠) — güncellemede yedekten dönüşün denetimi sipariş tarafının bütün kayıtlarına bakar; öteki yazımlar dönüşle geri gider. ⚠ listesi 31 → 32. Ürün Gereksinimleri'ne, Kullanıcı Akışları'na, Arayüz Tanımları'na ve MVP Kapsamı'na dönen değişiklik yok — K-956 `02 §3.34.8`'i değiştirmez, ona hizalanır; `05`'in geri besleme tablosuna satır girmedi. Park satırı: `DEPLOY_RUNBOOK` (Güncelleme, geri alma — K-938, K-956). Geri işaret: K-938 ve K-950'nin etki sütunları.

- [x] P5 · BULGU-1 (kısmi — K-956 ⚠: denetim `02 §3.34.8`'in bütün kayıtlarına; öteki yazımlar geri gider; "hiçbir kalıcı yazım" koşulu eklenmedi)
- [x] Tek · BULGU-1 (kısmi — K-950 korundu; dayanak 6.7.8'de)
- [x] P1 · BULGU-1 (kısmi — 3.6.1 daraltıldı; durum ucu imzasız açık GET; istisna eklenmedi)
- [x] P3 · BULGU-1 (ret, dönen — K-900; ikinci ret)
- [x] P4 · BULGU-1 (ret, dönen — K-951, proje sahibinin kararı; ikinci ret)
- [x] P4 · BULGU-2 (ret, dönen — K-888'in kabul edilmiş penceresi; ayrı depo üçüncü kez önerildi)
- [ ] Etki yansıtma (Faz 5) — döngü TEMİZ döndükten sonra, son turun raporunda

**Hukuki kontrol** (2026-10-17): üç bulgu yasal dayanak taşır. Tek · BULGU-1 — KVKK'nın Çerez Uygulamaları Hakkında Rehberi ve KVKK m.5/2-c; K-950'de okunmuştu, model rehberin aynı örneğini getirdi. P4 · BULGU-1 — 6563 sayılı Kanun m.11/3'ün son cümlesi; metin K-951'de resmî kaynaktan okunmuştu ve modelin aktarımıyla aynıdır. P4 · BULGU-2 — KVKK'nın silme hükmü; kabul edilmiş pencere proje sahibinin kararıdır (K-888). P3 · BULGU-1'in Google belgesi bir uygulama önerisidir, mevzuat değildir. Düzeltmeler yeni hukuki içerik eklemez — K-950'nin dayanağını `05`'e taşır ve `02 §3.34.8`'in listesini denetime uygular.

**Yakınsama ölçüsü:** ayrı bulgu 7 → 5 → 6; kabul edilen ya da kısmen kabul edilen 7 → 3 → 3; yeni karar 2 → 0 → 1 (⚠); dönen iddia 0 → 1 → 3. Bu turun üç kabulünden ikisi düşük şiddette metin farkıdır; biri (P5 · BULGU-1) ölçünün (2). sınıfında — hak kaybı — ve 1. ve 2. turda okunmuş bir bölümde (§8.7) ilk kez bulundu. Doküman 306 709 bayttan 309 300 bayta büyüdü; artışın yaklaşık üçte biri sürüm notu ve dipnottur. Tablolara satır girmedi. Parça 2 (§4–§5) bu turda TEMİZ; parça 1 (başlık notu, §0–§3) iki turdan sonra bir metin farkı getirdi.

## 6. Sonuç ve 4. tur

**Sonuç: TEMİZ DEĞİL** (ölçünün dört sınıfında) — altı çağrının biri TEMİZ; altı ayrı bulgunun üçü kısmen uygulandı, üçü reddedildi. Reddedilenlerin üçü de dönen iddiadır ama hiçbiri önceki turlarda iki kez reddedilmemişti — skill'in "tekrar eden RET" tanımına (en az iki kez) bu turda girmezler. Faz 5 (etki yansıtma) bu turda koşmaz; TEMİZ turunda koşar.

**4. tur** aynı istemle (1. turun raporunun §1'i, değiştirilmeden), aynı şablonla ve iki koşumla (K-953) yürür; parça sınırları bölüm başlıklarıdır. Önceki turların raporları, karar kaydı ve audit raporları verilmez (K-431). Tur, altı çağrının altısı da `SONUÇ: TEMİZ` dönerse — ya da yalnız tekrar eden RET'ler kalırsa — TEMİZ'dir. **4. turda tekrar eden RET sayılacaklar** (aynı yer ve iddiayla dönerlerse): Google hesabının kalıcı kimliği (§6.1.4; 2. ve 3. turda reddedildi) · ödemesi alınmamış siparişte kişisel verinin üç yılı (§7.4.3 (4), §4.4.4; 2. turda öneri reddedildi, 3. turda RET) · yedeğe ulaşmamış penceredeki silmeler ve ayrı silme deposu (§7.4.5; öneri 1. ve 2. turda elendi, 3. turda RET).

**Çıkış kuralı önerisi (uygulanmadı):** yeni bir K-849 kalıbı karar bu turda gerekmiyor. Skill'in iki çıkış kuralı yeterli görünüyor: dönen üç iddia 4. turdan itibaren tekrar eden RET olarak turun sonucunu belirlemez, beşinci turdan itibaren de metin farkı sınıfının çıkış kuralı uygulanabilir. Yakınsama yarım: ayrı bulgu sayısı düşmedi (5 → 6), ama yeni iddialar üçe indi ve dönen iddialar artık kayıtla kapanıyor. Gerekçe: bu tur ölçünün (2). sınıfında gerçek bir boşluk buldu (P5 · BULGU-1) — döngü hâlâ gerçek sorun yakalıyor; erken bir çıkış kuralı onu checkpoint'e bırakırdı. 4. turda kabul edilen bulguların hepsi düşük şiddette metin farkı olur ve dönenler tekrar eden RET'e düşerse, 5. tur skill'in kuralıyla kapanabilir.
