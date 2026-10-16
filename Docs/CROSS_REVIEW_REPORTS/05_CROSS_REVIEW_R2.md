# Cross-Review — 05 Technical Architecture (Tur 2)

**Tarih:** 2026-10-17 · **Hedef:** `Docs/05_TECHNICAL_ARCHITECTURE.md` v0.6 (girdi commit'i `7f32b18`, 304 891 bayt, 1 481 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren uygulanır (K-952) · **Koşum:** iki biçimde, altı çağrı — tek parça çağrı ve beş parçalı kontrol koşumu (K-953); turun sonucu ikisinin birleşimidir · **Girdi:** yalnız doküman ve boş şablonu (`58cf0b9` — Aşama 4 açılmadan önceki v0.1; park blokları çıkarılmış, 87 satır), stdin'den tek metin, boş ve izole bir çalışma klasöründe; karar kaydı, önceki turun raporu, Ürün Gereksinimleri, Kullanıcı Akışları, Arayüz Tanımları, MVP Kapsamı ve audit/deep review raporları verilmedi (K-431)

## 1. Koşum

**İstem ve şablon** 1. turunkidir, değiştirilmeden (`05_CROSS_REVIEW.md` §1'in alıntı blokları — istem ve parça notu). Girdiler aynı betikle yeniden kuruldu: `istem + "=== DOKÜMAN ===" + güncel doküman + "=== ŞABLON ===" + şablon`; parçalı çağrılarda istemden sonra parça notu, parçada başlık notu. **Komut** 1. turunkidir: `codex exec -s read-only -C <boş klasör> --skip-git-repo-check --ephemeral --ignore-user-config --color never -m gpt-5.6-terra -c 'model_reasoning_effort="high"' -o <çıktı> - < <girdi>`. Altı çağrı paralel koştu.

**Parça sınırları** 1. turunkidir — bölüm başlıkları (K-953); satır numaraları v0.6'nındır.

| Parça | Bölümler | Satırlar | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|---|
| — | Tek parça — dokümanın tamamı | 1–1481 | 312 878 bayt | 118 911 | 178 sn | 1 bulgu |
| 1 | Başlık notu · §0 · §1 · §2 · §3 | 1–436 | 100 451 bayt | 46 510 | 37 sn | `SONUÇ: TEMİZ` |
| 2 | §4 durum ve olaylar · §5 arka plan işleri | 437–612 | 75 502 bayt | 67 287 | 97 sn | 2 bulgu |
| 3 | §6 güvenlik (§6.x dahil) | 613–818 | 88 969 bayt | 47 790 | 170 sn | 1 bulgu |
| 4 | §7 veri katmanı | 819–953 | 78 225 bayt | 62 575 | 155 sn | 2 bulgu |
| 5 | §8 ortamlar · §9 gözlemlenebilirlik · §10 hata toleransı · §11 ölçeklenebilirlik · §12 özet | 954–1481 | 144 949 bayt | 73 116 | 114 sn | 2 bulgu |

**Web araması:** tek parça çağrı ve parça 1 aramadı; parça 2, 3, 4 ve 5 aradı — 6563 sayılı Kanun m.11/3 (lexpera, mevzuat.adalet.gov.tr) ve Google'ın OpenID Connect belgesi (koşum kayıtlarındaki `web search` satırları). Kamuya açık mevzuat ve araç belgesi proje bilgisi değildir (1. turun raporu §1); açık olduğu yine yazılır.

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**Tek parça çağrı:**

> ```text
> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §4.5.3
> Alıntı: "bildirim de son sorgu da aynı işleve gider; işlev siparişin kilidini alır (§7.3.2) ve sonucu o anki hâle göre uygular — Bekliyor'da Ö1 ya da başarısızlık; zaten Ödendi'yse hiçbir şey yapmaz; İptal edildi'yse kart iadesini kuyruğa yazar"
> Sorun: Siparişin tek kez oluşması, aynı sipariş için ayrı ödeme başlatmalarını veya farklı sağlayıcı işlem kimlikleriyle iki başarılı tahsilatı engellemez. İlk tahsilattan sonra gelen ikinci geçerli başarı bildirimi “hiçbir şey yapmaz” yolunda sessizce yok sayılır; ikinci tutar müşteriden alınmış kalabilir. Bu, müşteriye para kaybı doğuran kural boşluğudur.
> Öneri: Ödeme başlatmayı sipariş/ödeme denemesi başına idempotent kılın; açık deneme varken aynı sağlayıcı oturumuna dönün. Buna rağmen farklı bir başarılı tahsilat gelirse otomatik iade veya firmaya zorunlu uzlaştırma uyarısı ve bloke edilmiş işlem kaydı oluşturun.
>
> SONUÇ: 1 BULGU
> ```

**Parça 1 — başlık notu, §0–§3:**

> ```text
> SONUÇ: TEMİZ
> ```

**Parça 2 — §4–§5:**

> ```text
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §4.4.4
> Alıntı: "ödemesi hiç alınmamış siparişin satırında üç yıl dolunca alıcı ve olayın değerleri boşaltılır"
> Sorun: Ödeme alınmamış sipariş için de oluşmuş e-posta ve sipariş kayıtlarının alıcı ile olay değerleri üç yılda yok edilmektedir. 6563 m.11/3, kapsamındaki iş ve işlemlere ilişkin elektronik kayıtlar için ödeme alınmış/alınmamış ayrımı yapmadan on yıllık saklama yükümlülüğü getirir. Bu, kanuni saklama süresinde ispat kaydının eksilmesine yol açar. [Kanun metni](https://www.lexpera.com.tr/mevzuat/kanunlar/elektronik-ticaretin-duzenlenmesi-hakkinda-kanun-6563)
> Öneri: Ödeme alınmamış siparişlerde de e-posta kaydını ispatlanabilir kılan alıcı ve olay değerlerini on yıl saklayın; erişimi kısıtlayarak veri güvenliğini sağlayın. Buna bağlı `02` ve §7.4 saklama kuralını aynı kararla düzeltin.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §5.4.1, §5.4.6
> Alıntı: "Böylece uygulamanın, veritabanının ya da ters vekilin durduğu ve sitenin dışarıdan açılmadığı aralık ... ölçülür." / "sunucunun dış ağı koparken kendine erişimi sürerse kesinti görünmez"
> Sorun: Aynı sunucudan ters vekile yapılan nabız, dış ağ kesilmişken başarılı kalabilir; belge bunu §5.4.6’da kabul ederken §5.4.1’de dış erişilemezlik aralığını ölçtüğünü kesin olarak söyler. Bu çelişki, havale iptalinin ertelenip ertelenmeyeceği kararını doğrudan etkiler.
> Öneri: Ya bağımsız ağdan bir nabız kaynağı kullanın ya da §5.4.1’i yalnız uygulama/veritabanı/ters vekilce gözlenebilen kesintileri ölçtüğü şekilde daraltın; §5.4.3’teki ertelemenin bu sınıra tabi olduğunu açıkça yazın.
>
> SONUÇ: 2 BULGU
> ```

**Parça 3 — §6:**

> ```text
> BULGU-1
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §6.1.4, §6.1.5
> Alıntı: “Google hesabının kalıcı kimliği saklanmaz”
> Sorun: Hesap eşleme doğrulanmış e-postaya dayanır. Google/OIDC, e-postanın değişebileceğini veya başka kullanıcıya atanabileceğini; kalıcı kimliğin `iss` + `sub` olduğunu belirtir. E-posta yeniden kullanılırsa yeni Google hesabı eski müşterinin hesabına, siparişlerine ve kişisel verilerine erişebilir. Bu, yetkisiz kişisel veri erişimi doğurur. [Google OIDC referansı](https://developers.google.com/identity/openid-connect/reference)
> Öneri: Google hesabını `iss` + `sub` ile bağlayıp saklayın; e-postayı yalnız ilk bağlama aşamasında doğrulayın. Sonraki girişlerde hesabı e-postayla değil bu sabit kimlikle bulun.
>
> SONUÇ: 1 BULGU
> ```

**Parça 4 — §7:**

> ```text
> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §7.4.3(2), §7.4.3(4)
> Alıntı: "Ödemesi hiç alınmamış siparişin kendisi üç yılda (2)'nin biçimiyle kişisel verisiz kalır ve on yıl dolmadan silinmez."
> Sorun: 6563 sayılı Kanun m.11/3, Kanun kapsamındaki iş ve işlemlere ilişkin bilgi, belge, defter ve elektronik kayıtların on yıl saklanmasını gerektirir. Ödemesiz siparişin alıcı ve olay değerlerini üç yılda silmek, bu elektronik işlem kaydını ispatlanamaz hâle getirir. Bu, mevzuata aykırılık sınıfındadır.
> Öneri: Sipariş ve sözleşme/işlem kayıtları için on yıllık zorunlu saklama tabanı tanımlansın; kişisel veri imhası ancak bu zorunlu kaydın gerektirmediği veriler için uygulansın ve saklanan kayda erişim sınırlandırılsın. [Mevzuat kaynağı](https://mevzuat.adalet.gov.tr/mevzuat/103544?query=6769)
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §7.4.5, §7.6.1, §7.6.2
> Alıntı: "Uygulama kaydı veritabanının dışında, sunucunun diskinde durur ve veritabanının dönüşüyle geri gitmez."
> Sorun: Anlık silme kaydı sunucu diskindedir; bu kayıtların dosya yedeği ise geceliktir. Son dosya yedeğinden sonra yapılan bir silme ile sunucu diskinin kalıcı kaybı birlikte yaşanırsa, eski noktaya döndürülen veritabanındaki silinen kişisel veri geri gelir; yeniden-imha komutunun okuyacağı kayıt da kaybolmuştur. Bu, “Eski bir ana dönüşte silmelerin geri gelmesi böylece kalan risk değildir” ifadesiyle çelişir ve silinmiş kişisel verinin yeniden erişilebilir olmasına yol açar.
> Öneri: Anlık silme günlüğünü, silme işleminden önce veya onunla birlikte sunucu kaybına dayanıklı bağımsız bir depoya yazın; kurtarma prosedürü veritabanını açmadan bu günlüğü idempotent olarak yeniden uygulasın.
>
> SONUÇ: 2 BULGU
> ```

**Parça 5 — §8–§12:**

> ```text
> BULGU-1
> Kriter: mevzuat
> Seviye: Yüksek
> Yer: Başlık notu — “Kalite döngüsünün çok kritik sorusu”
> Alıntı: “ödemesi hiç alınmamış siparişte kişisel veri üç yılda silinir, kayıt on yıl kalır.”
> Sorun: 6563 sayılı Kanun m.11/3, elektronik ticaret hizmet sağlayıcısının Kanun kapsamındaki iş ve işlemlere ilişkin bilgi, belge, defter ve elektronik kayıtları işlem tarihinden itibaren on yıl saklamasını gerektirir. Ödemesi alınmamış siparişin kişisel verisini üç yılda fiziksel olarak silmek, on yıl saklanacak kaydın eksik kalmasına yol açar. Bu, mevzuata aykırılık riskidir.
> Öneri: Ödeme sonucu ne olursa olsun Kanun kapsamındaki sipariş/işlem kayıtlarının on yıl boyunca eksiksiz tutulmasını; KVKK bakımından saklama dayanağı, erişim sınırlaması ve imha planının bu zorunlu saklama süresine göre düzenlenmesini yazın.
>
> BULGU-2
> Kriter: güvenlik
> Seviye: Yüksek
> Yer: §8.5.10
> Alıntı: “uygulama kaydındaki anlık silme satırlarını dönüş anından bugüne okur ve her birini aynı silme yoluyla yeniden uygular”
> Sorun: Bu güvence, kalıcı disk kaybında çalışmaz. Uygulama kayıtları yalnız gecelik dosya yedeğiyle sunucu dışına taşınmaktadır (§9.2.3); buna karşılık §10.1.9, sunucunun kalıcı disk kaybından yedekten dönüşü öngörür. Son dosya yedeğinden sonra gerçekleşen anlık silme, geri yüklenen veritabanında geri gelebilir; onu yeniden silecek uygulama kaydı ise diskle birlikte kaybolmuştur. Böylece silinmiş kişisel veri yeniden erişilebilir hâle gelir; bu, dokümanın silmelerin geri yüklemeden sonra yeniden uygulanacağı güvencesiyle çelişir ve yetkisiz kişisel veri saklama riski doğurur.
> Öneri: Anlık silme/replay kaydını gecelik dosya yedeğinden bağımsız, sunucu dışı ve veritabanı geri dönüş noktasıyla uyumlu dayanıklı bir kayıt olarak tasarlayın; kalıcı disk kaybı kurtarmasında bu kaydın da geri yüklenip uygulama açılmadan önce zorunlu olarak yeniden uygulanmasını sağlayın.
>
> SONUÇ: 2 BULGU
> ```

## 3. Bağımsız değerlendirme

Her bulgu dokümanın tamamına (v0.6), ilgili yerde Ürün Gereksinimleri'ne (`02` v0.72), Kullanıcı Akışları'na (`03` v0.26) ve karar kaydına karşı okundu — ikinci model bunları görmedi, parçalı çağrılar dokümanın öteki bölümlerini de görmedi. Sekiz ham bulgu beş ayrı bulgudur: P2 · BULGU-1, P4 · BULGU-1 ve P5 · BULGU-1 aynı iddiadır (ödemesi alınmamış siparişin üç yılı); P4 · BULGU-2 ile P5 · BULGU-2 aynı iddiadır (diskin kaybında silme satırları). Her biri tek satırda değerlendirildi.

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| P2 · BULGU-1 = P4 · BULGU-1 = P5 · BULGU-1 | ⚠️ KISMİ | **İddia bilinçli bir kararın hukuki yorumuna itirazdır; dokümanda eksik olan kararın gerekçesiydi.** Ödemesi hiç alınmamış siparişte kişisel verinin üç yılda silinip kaydın on yıl kalması proje sahibinin kararıdır (K-951, "1 · On yıl, karma"): soru 6563 m.11/3'ün tam metniyle soruldu ve modelin önerisi — ödeme sonucundan bağımsız olarak kişisel veri de on yıl — seçenek 2 olarak sunulup gerekçesiyle elendi (KVKK'nın "gerektiği kadar sakla" ilkesi; m.4/2-ç, m.4/2-d). Hüküm "iş ve işlemlerine ilişkin ... elektronik kayıtları" saklatır; işlemin kaydı on yıl durur — kişisel verinin de saklanıp saklanmayacağı yorumdur ve proje sahibi bunu kararlaştırdı. Alt ajan proje sahibinin kararını değiştiremez; soru yeni bir bilgi getirmediği için yeniden sorulacak çok kritik bir konu da değildir. **Gerçek olan:** `02 §12.2.7` iki kolun gerekçesini yazar, `05 §7.4.3` (4) ise kuralı gerekçesiz verir; başlık notu da yalnız kuralı özetler — dokümanı tek başına okuyan için bilinçli karar görünmüyordu ve üç parça aynı iddiayı getirdi. | §7.4.3 (4): iki kolun ayrımı bilinçli karardır (K-951, proje sahibinin kararı; `02 §12.2.7`) — on yıllık saklama işlemin kaydını tutar, tamamlanmamış işlemin kişisel verisi gerektiğinden uzun tutulmaz (KVKK m.4/2-ç, m.4/2-d); "kişisel veri de on yıl" seçeneği elendi. K-951 değişmedi; satırına geri işaret. |
| P2 · BULGU-2 | ✅ KABUL | **İki yerin metin farkı.** 5.4.1 nabzın "sitenin dışarıdan açılmadığı aralığı" ölçtüğünü söylüyordu; nabız sunucunun kendisinden ters vekile gider ve 5.4.6 sunucunun yalnız dış ağı koparken kesintinin görünmediğini kalan risk olarak yazar (K-898). Doğru olan 5.4.6'dır; 5.4.1 fazla söylüyordu. Ölçünün (4). sınıfı, düşük şiddet; önerinin ikinci yolu uygulandı — bağımsız ağdan nabız 5.4.6'nın elediği dış izlemeye bağlanmadır. 10.1.16'nın "yetkili DNS düşerse kesinti ölçülür" cümlesi nabzın adı sunucunun çözümleyicisiyle çözmesine dayanır ve 5.4.1'in daraltılmış hâliyle çelişmez. | 5.4.1: uygulamanın, veritabanının ya da ters vekilin durduğu aralık ölçülür; sunucunun yalnız dış ağının koptuğu aralık ölçülmez (kalan risk, 5.4.6). |
| P4 · BULGU-2 = P5 · BULGU-2 | ⚠️ KISMİ | **Senaryonun öncülü yanlış, ama dokümanın kesin cümlesi bir pencereyi saymıyordu.** Diskin kalıcı kaybında veritabanı eski bir ana değil, **en son ana** döndürülür (7.4.5, 7.6.7, §10.1.9); o ana kadarki silmeler işlem günlüğünde olduğu için yeniden uygulanmış olur — silmelerin yeniden uygulanması komutu (8.5.10) yalnız veri bozulmasında, sunucunun diski sağlamken yapılan eski bir ana dönüşün aracıdır. Kalan gerçek pay yedeğe henüz ulaşmamış penceredir (7.6.2 — bir dakika; aktarım durmuşsa daha uzun, K-939): o pencerede yapılan silme penceredeki öteki kayıtlar gibi kaybolur, uygulama kaydındaki satırı da diskle gider ve silinen kayıt geri gelir. 7.4.5'in "kalan risk değildir" cümlesi bunu saymıyordu — iki yerin metin farkı (ölçünün (4). sınıfı, düşük şiddet); pencere proje sahibinin kabul ettiği penceredir (K-888) ve `02 §3.34.8` onu bütün kayıtlar için yazar. Modelin önerdiği **ayrı, sunucu dışı silme deposu** uygulanmadı: 1. turda aynı öneri (P4 · BULGU-1) elenmişti (K-955), ve pencereyi kapatacak sunucu dışına eşzamanlı yazım, 7.6.2'nin eklemediği ikinci eşzamanlı kopyadır (§0.13). **1. turun P4 · BULGU-1'inin ilk dönüşü** (aynı öneri, yeni gerekçe — tekrar eden RET sayılmaz). | 7.4.5: diskin kalıcı kaybında komut gerekmez — dönüş en son ana yapılır; yedeğe ulaşmamış penceredeki silme geri gelir ve bu 7.6.2'nin kabul edilmiş penceresinin parçasıdır (K-888, K-939) · §7.4'ün Kaynak satırına K-888, K-939 · `DEPLOY_RUNBOOK`'un K-955 park satırı. K-955 değişmedi; satırına geri işaret. |
| Tek · BULGU-1 | ❌ RET | **Senaryo ürünün ödeme modelinde doğmaz.** Bir sipariş sağlayıcıya bir kez yönlendirilir (4.2.4); aynı onay anahtarıyla ikinci gönderim yeni ödeme açmaz, siparişin sayfasına götürür (4.5.1); kart ödemesi başarısız olursa ya da yarıda kalırsa yeniden deneme **yeni bir siparişle** olur ve önceki ödenmemiş sipariş iptal edilir (`03` 2.4.9, 2.5.1.3, 2.5.1.4; `02 §6.2.1`). Aynı sepetle iki sekmeden ödeme bu yüzden iki ayrı siparişe düşer ve iptal edilen siparişe gelen başarılı ödeme kendiliğinden geri ödenir (`02 §6.2.16`; 4.5.3'ün "İptal edildi'yse kart iadesini kuyruğa yazar" dalı). 4.5.3'ün "zaten Ödendi'yse hiçbir şey yapmaz" dalı aynı ödemenin ikinci mesajı içindir — bildirim ile son sorgunun ikisi (4.5.2'nin tekil işlem kimliği). Aynı sipariş için ikinci, ayrı bir başarılı tahsilat ancak sağlayıcının kendi hatasıyla doğar — nadir kenar durumu, ölçünün dışında. | — |
| P3 · BULGU-1 | ❌ RET | **Bilinçli karar ve güvence iddiası tutmuyor.** Google hesabının kalıcı kimliğinin saklanmaması ürün kuralıdır (`02 §8.1.2`, §12.2.8: ürün Google'dan yalnız ad ve e-posta alır) ve "kalıcı kimlik saklansın, karşılaştırma ona göre yapılsın" seçeneği gerekçesiyle elendi (K-900). Modelin senaryosu — e-posta adresinin başka bir Google hesabına geçmesi — üründe yeni bir yol açmaz: hesap e-postaya bağlıdır ve adresi eline geçiren kişi şifre sıfırlamayla (`02 §3.13.8`; 6.1.5'in son cümlesi) aynı hesaba zaten girer; Google'ın kalıcı kimliğini saklamak bu yolu kapatmaz, yalnız Google ile girişi farklılaştırır. Kişisel verinin yetkisiz kişiye açılması hesabın e-postasını kaybeden kişinin bütün e-posta tabanlı sistemlerdeki ortak riskidir; bu doküman bir kısıt koymaz. | — |

**Dağılım:** beş ayrı bulgu — 1 KABUL · 2 KISMİ · 2 RET. **%100 kabul değil, %100 ret değil:** iki RET somut yere dayanır (`03` 2.4.9 ve 2.5.1.x, `02 §6.2.16`; K-900, `02 §3.13.8`); iki KISMİ'nin ikisinde de modelin önerisi reddedildi — proje sahibinin kararı ve K-955'in mekanizması korundu — ve yalnız dokümanın kendi metni hizalandı. **Hiçbir bulgu yeni karar doğurmadı:** üç düzeltme metin hizalamasıdır — biri bir cümlenin daraltılması, biri bir kararın gerekçesinin dokümana yazılması, biri kesin bir cümlenin kabul edilmiş pencereyle sınırlanması.

## 4. Ek bulgular

- **Aynı kalıbın taraması (P2 · BULGU-2):** "dışarıdan açılmadığı" ve "kesinti ölçülür" geçen öteki yerler — §12 MK-44 (kalan riski zaten anar), 10.1.16 (yetkili DNS) — okundu; ikisi de 5.4.1'in daraltılmış hâliyle uyumlu.
- **Aynı kalıbın taraması (P2 · BULGU-1):** ödemesi alınmamış siparişin üç yılı `05`'te §4.4.4, §7.4.3 (4) ve başlık notunun v0.5 satırında geçer; §4.4.4 §7.4.3'e işaret eder, sürüm notu tarihlidir — gerekçe tek yerde, kuralın mekanizmasının yazıldığı §7.4.3'te durur. Ürün Gereksinimleri gerekçeyi zaten `02 §12.2.7`'de taşır — geri besleme yok.
- **Aynı kalıbın taraması (P4 · BULGU-2):** "silmelerin yeniden uygulanması" 7.4.5, 8.5.10, 9.1.3, §12 MK-72 ve `DEPLOY_RUNBOOK`'un park satırında geçer; 8.5.10 ve MK-72 komutu zaten "en son andan eski bir ana dönüşe" bağlar — değişmedi; park satırı diskin kaybındaki durumu almadı, eklendi.
- **Mekanik tarama:** Kaynak satırı betiği (MK- kimliklerini dışarıda bırakarak) değişiklikten önce ve sonra aynı sonucu verir — yeni eksik yok; §7.4'ün gövdesine giren K-888 ve K-939 Kaynak satırına eklendi. Başlık sürümü, yazım durumu notu ve dosya sonu dipnotu v0.7'yi gösterir. Karar kaydında çift numara yok; yeni K numarası açılmadı.
- **Ciddi bir sorun görülmedi.** Değerlendirme sırasında ikinci modelin listesi dışında ölçünün dört sınıfından birine giren bir sorun saptanmadı.
- **Önceki turlardan açık kalanlar:** yok. 1. turun yedi düzeltmesinin hiçbiri bu turda aynı iddiayla dönmedi; P4 · BULGU-2 1. turun P4 · BULGU-1'inin önerisini yeni bir gerekçeyle yineler (ilk dönüş).

## 5. Kullanıcı onay checklist'i

K-855 kararıyla (Aşama 4 boyunca ⚠ öneriyle kayıt, CI yeşilse `docs:` PR merge yetkisi ve yönetici + alt ajan düzeni) alt ajan değerlendirdi ve uyguladı (`05` v0.7). **Yeni karar yok** — ⚠ listesi 31'de kalır. Ürün Gereksinimleri'ne, Kullanıcı Akışları'na, Arayüz Tanımları'na ve MVP Kapsamı'na dönen değişiklik yok; `05`'in geri besleme tablosuna satır girmedi. Park satırı: `DEPLOY_RUNBOOK` (Yedek, kurtarma ve prova — K-955). Geri işaret: K-951 ve K-955'in etki sütunları.

- [x] P2 · BULGU-1 = P4 · BULGU-1 = P5 · BULGU-1 (kısmi — K-951 korundu; gerekçe §7.4.3 (4)'te)
- [x] P2 · BULGU-2 (kabul — 5.4.1 5.4.6'ya hizalandı)
- [x] P4 · BULGU-2 = P5 · BULGU-2 (kısmi — diskin kaybında komut gerekmez; penceredeki silmeler 7.6.2'nin penceresi; ayrı depo eklenmedi)
- [x] Tek · BULGU-1 (ret — sipariş başına tek sağlayıcı oturumu; iptal edilen siparişe gelen ödeme geri ödenir)
- [x] P3 · BULGU-1 (ret — K-900; e-posta zaten hesabın anahtarı)
- [ ] Etki yansıtma (Faz 5) — döngü TEMİZ döndükten sonra, son turun raporunda

**Hukuki kontrol** (2026-10-17): iki bulgu yasal dayanak taşır. P2/P4/P5 · BULGU-1 — 6563 sayılı Kanun m.11/3'ün son cümlesi; metin K-951'de resmî kaynaktan okunmuştu ve modelin alıntısıyla aynıdır; kişisel verinin saklanmaması KVKK m.4/2-ç, m.4/2-d'ye dayanır — proje sahibinin kararı. P3 · BULGU-1 — Google'ın OpenID Connect belgesi bir uygulama önerisidir, mevzuat değildir. Düzeltmeler yeni hukuki içerik eklemez.

**Yakınsama ölçüsü:** ayrı bulgu 7 → 5; kabul edilen ya da kısmen kabul edilen 7 → 3; yeni karar 2 (ikisi ⚠) → 0. Bu turun kabullerinin üçü de düşük şiddette metin farkıdır — mevzuata aykırılık, para ya da hak kaybı, çıkışsız akış değil. Doküman 304 891 bayttan 306 709 bayta (+1,8 KB) büyüdü; artışın yaklaşık üçte biri sürüm notu ve dipnottur. Tablolara satır girmedi. Parça 1 (başlık notu, §0–§3) iki turdur TEMİZ.

## 6. Sonuç ve 3. tur

**Sonuç: TEMİZ DEĞİL** (ölçünün dört sınıfında) — altı çağrının yalnız biri TEMİZ; beş ayrı bulgunun üçü uygulandı, ikisi reddedildi. Reddedilenler ilk kez reddedildi; skill'in "tekrar eden RET" tanımına (en az iki kez) henüz girmezler. Faz 5 (etki yansıtma) bu turda koşmaz; TEMİZ turunda koşar.

**3. tur** aynı istemle (1. turun raporunun §1'i, değiştirilmeden), aynı şablonla ve iki koşumla (K-953) yürür; parça sınırları bölüm başlıklarıdır. Önceki turların raporları, karar kaydı ve audit raporları verilmez (K-431). Tur, altı çağrının altısı da `SONUÇ: TEMİZ` dönerse TEMİZ'dir; çıkış kuralları skill'in Faz 4 madde 5'indedir — bu turun iki RET'i 3. turda aynı yer ve iddiayla döner ve yeniden reddedilirse, sonraki dönüşleri "tekrar eden RET" sayılır; beşinci turdan itibaren metin farkı sınıfının çıkış kuralı uygulanabilir.
