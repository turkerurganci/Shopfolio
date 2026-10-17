# Shopfolio — Integration Specifications

**Versiyon: v0.1** | **Bağımlılıklar:** `02`, `03`, `05`, `06`, `07` | **Son güncelleme:** YYYY-AA-GG

> **Aşama:** 7 — Entegrasyon Spesifikasyonları · **Rol:** Integration Engineer
> **Traceability zorunlu:** Hayır
> **Beklenti:** Bu doküman doğası gereği `07`'ye **geriye dönük endpoint** ekletir (geri çağrım/webhook uçları). Bu bir hata değil, beklenen sonuçtur.

> **Aşama 1'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36.
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-12 · Para akışı ve kart verisi:** Tahsilat doğrudan **firmanın kendi sanal POS / ödeme sağlayıcı hesabına** geçer; platform ticari zincirde yer almaz. Kart bilgisi sağlayıcının barındırdığı sayfada veya çerçevede girilir — **ürün kart verisini görmez, saklamaz, loglamaz.** Entegrasyon bu sınırı bozacak biçimde tasarlanamaz. Firmanın ödeme sağlayıcı sözleşmesi ve anahtarları **kart yönteminin dış ön koşuludur:** tanımlı değilse kart yöntemi görünmez ve satış — havale açık ve IBAN doluysa — yalnız havaleyle açılır; entegrasyon kartsız canlı kurulumu tanımlı bir hâl olarak taşır (K-159, K-163, K-378; `10 §4.1` ÖK-4 — satır Aşama 3'ün çakışma taramasında hizalandı).
> - **K-13 · 3D Secure:** **İstisnasız zorunludur.** Tutar eşiği veya sağlayıcı takdirine bırakma yoktur; doğrulama başarısızsa ödeme gerçekleşmez ve sipariş ödenmiş sayılmaz.

> **Aşama 2'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §8.3 AK0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-705 · Sağlayıcıyla sonucu belirsiz kalan kart işlemleri:** (1) Sağlayıcıya ulaşılamayan kart iadesi sonucu bilinmeyen bir istektir; firmanın yeniden denemesi ya da müşteriye havale yolu açması parayı iki kez gönderebilir (`02 §7.2.8`). İade isteğinin tek işlenmesi ve sonucun sağlayıcıdan sorulması — ödeme tarafındaki son sorgunun (`02 §6.1.2`) iade karşılığı — bu aşamanın kararıdır; tasarım ürüne bir "sonuç alınamadı" işareti gerektirirse karar kaydına döner. (2) Kart ödemesinin onayının tek dayanağı sağlayıcının başarı bildirimidir (`02 §5.5` Ö1): bildirimin doğrulanması ve tutarının siparişin tutarıyla eşleşmesi bu aşamada tasarlanır; `03 §2.5.1.2` müşterinin dönüş anında sonucun beklendiğini gördüğünü yazar (K-704).
> - **Dizin — Aşama 2 kararlarının bu dokümana devirleri:** karar kaydında K-647…K-718'in etki sütunlarında bu dokümanı gösteren on iki atıf (on iki karar) ve Kullanıcı Akışları'nın (`03`) gövdesinde bu dokümana iş bırakan cümleler [`CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md) §6'dadır. İki iş yalnız `03`'ün gövdesinde yaşar, karar satırı yoktur: yeniden gönderilebilen e-postaların kapsamı (`03 §7.1.37`, §8.9.2 — *Aşama 3'te karara bağlandı: K-759; aşağıdaki blok*) ve ödeme sağlayıcısının anahtarlarının ya da sağlayıcının değişmesinin açık kart siparişlerine etkisi ve geçiş yöntemi (`03 §10.1.1.7`). Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Aşama 1'in dizini: `PHASE1_CONFLICT_SCAN.md` §6.

> **Aşama 3'ten park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2 ve §3, mekanizma: K-36 (yeri: §10.3 UI0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-735 · K-599, K-601, K-616 · Üç bildirimin metni:** Aşama 1'in üç kararı bir bildirim metnini Arayüz Tanımları'na (`04`) devretmişti; e-postanın metni, şablonu ve gönderim biçimi bu dokümanın işidir (`03 §7` girişi) ve devir buraya taşındı (`04 §1.3` GAP-5). Yazılacak metinler: hesabın e-postası değiştiğinde eski adrese giden bildirim — adresin değiştiği ve tarihi, yeni adres yok, "bu değişikliği ben yapmadım" bağlantısı (`02 §3.13.14`, §9.4) · B-10, siparişin adresi düzeltildiğinde — düzeltilen adres ve değişikliği istemeyen müşterinin firmaya ulaşması (`02 §9.2`, §10.4.2) · F-5, havale IBAN'ı girildiğinde ya da değiştiğinde yöneticilere — tarih ve saat, yapan yönetici, IBAN'ın yalnız son dört hanesi; tam IBAN ve geri alma bağlantısı yok (`02 §9.3.3`). İçerik kuralı `02`'dedir; bu doküman metni ve şablonu yazar. Ekrandaki izleri `04`'te kalır: geri alma ekranı, adres düzeltme ekranındaki teyit hatırlatması.
> - **K-759 · K-760 · Yeniden gönderilen bildirimler:** panelden yeniden gönderilebilen e-postalar siparişe bağlı on altı müşteri bildirimidir (B-1…B-16) — "e-posta ulaşmadı" işaretini düşüren her bildirim (`02 §9.1.6`); firma bildirimleri (F-1…F-6) ve hesap e-postaları yeniden gönderilmez. Bu aşamada yazılacak olan: yeniden gönderilen bildirimin ilk denemeden sonra değişmiş bilgiyi — düzeltilmiş takip bilgisi, yeniden başlamış ödeme süresi — nasıl taşıdığı ve e-postanın bir yeniden gönderim olduğunu alıcıya söyleyip söylemediği. B-1 siparişe donmuş sürümüyle gider ve bu değişmez (`02 §6.1.1`). Firmaya giden bildirimin (F-1…F-4) ulaşmaması panelin ana sayfasında uyarı doğurur; ulaşmadığının nasıl anlaşıldığı bu aşamanın ve Teknik Mimari'nin (`05`) işidir. Ekrandaki izleri `04`'te kalır: sipariş ayrıntısındaki ulaşmayan e-postalar listesi ve ana sayfanın uyarıları (K-761).
> - **K-824 · Tarih, saat ve tutarın e-postadaki biçimi:** arayüzün tarih, saat, sayı ve tutar biçimi Arayüz Tanımları'nda (`04 §8.2`) sabittir — "4 Ekim 2026", "14:30", "1.200 TL" ve "266,67 TL"; tek saat dilimi Türkiye saatidir (`02 §4.1.1`). Bildirimlerin metni ve şablonu yazılırken biçim buradan okunur; e-postada başka bir biçim seçilirse ayrılığın gerekçesi yazılır.
> - **K-832 · Yasal metinlerin e-postadaki okunabilirliği:** Ön Bilgilendirme Formu ve Mesafeli Satış Sözleşmesi sipariş e-postasında da `02 §3.24.3`'ün metin boyutu alt sınırıyla, açık ve okunabilir biçimde gider (Mesafeli Sözleşmeler Yönetmeliği m.6/1). Şablonun bunu nasıl sağladığı bu aşamanın kararıdır.
> - **Dizin — Aşama 3 kararlarının bu dokümana devirleri:** karar kaydında K-723…K-850'nin etki sütunlarında bu dokümanı gösteren beş atıf (beş karar) ve Arayüz Tanımları'nın (`04`) gövdesinde bu dokümana iş bırakan cümleler [`CHECKPOINT_REPORTS/PHASE3_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE3_CONFLICT_SCAN.md) §6'dadır; Aşama 3'ün atıflarının hepsi işin adını taşır. İki iş yalnız `04`'ün gövdesinde yaşar, karar satırı yoktur: marka renginin e-postadaki yeri (`04 §2.1.3`) ve bildirimlerin taşıdığı bağlantının biçimi — panel ekranına inen firma bildirimleri dahil (`04 §3.5`). Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Önceki aşamaların dizinleri: `PHASE1_CONFLICT_SCAN.md` §6, `PHASE2_CONFLICT_SCAN.md` §6.

> **Aşama 4'ten park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §12.3 TM0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-862 · E-posta gönderim altyapısı ve gider tavanı:** firmanın aylık altyapı gideri yaklaşık 10–20 $'dır ve e-posta gönderimini kapsar (K-861); gönderim altyapısı kullanım kadar ödenen ya da ücretsiz katmanlı bir servistir (Teknik Mimari `05 §0`); günlük ve aylık gönderim sınırı H-2'nin (`02 §11.3`) tepe gününün e-posta sayısını taşır — kota aşımı kanal hatasıdır (`05 §10.2.2`). Servisin seçimi ve konumu — yurt dışındaysa aktarımın şartları firmanın yükümlülüğüdür (`02 §12.2.8`; `10 §4.1` ÖK-2) — bu aşamanın kararıdır.
> - **K-892 · K-893 · K-894 · K-897 · K-940 · K-941 · K-959 · K-970 · K-974 · K-978 · Bildirimler ve ödeme sağlayıcısı:** e-postanın gövdesi gönderim anında olayın değerlerinden ve siparişin donmuş verisinden üretilir ve saklanmaz — şablonun değerleri bu dokümanda tanımlanır; sağlayıcının bildirimi bağlantı katmanında doğrulanır — imzası tutmayan istek kayda yazılmaz, işlenmez ve 4xx alır —, imzası doğru bildirim gelen bildirim kaydına yazılır ve kuyruktan işlenir; tekil olan bildirimin kendisidir — sağlayıcının bildirime verdiği kimlik, yoksa işlem kimliği olay türü ve sonuçla birlikte —, tekrar eden bildirim "alındı" yanıtı alır ve kart iadesinin sonucunu taşıyan bildirim ödeme sonucu olarak uygulanmaz (`05 §4.5.2`, §4.5.3); gelen bildirim kaydı kişisel alan taşımaz — sağlayıcının referansı, siparişin referansı, tutar, sonuç ve anlar; karta para gönderen isteğin kimliği sağlayıcıya tekrar anahtarı ya da referans olarak gider, sonucu bilinmeyen istek yeniden gönderilmez ve sonucu sorulur — sonuç beklenirken panelde "geri ödeme sonucu bekleniyor" işareti durur; sorgunun sonucuyla kalkar ya da "geri ödeme gerçekleşmedi"ye döner, firma sağlayıcının panelinde gördüğü sonucu "gerçekleşmedi" diye işaretleyebilir — işaret sorguyu durdurur; o ana kadar sorgu kendi iş satırıyla sonuç gelene ya da işarete kadar yinelenir, varsayılan beş deneme kuralı yalnız işleyicinin kod hatasında işler (`05 §4.3.4`; K-970); sonradan gelen başarılı sonuç — geç bildirim ya da işaret anında yanıtı beklenen sorgu — siparişe işlenmez, gelen bildirim kaydında ya da geri ödeme kaydında işaretli kalır ve panelde firmaya uyarı olur (K-940; `05 §4.6.2`); işaretten sonra — işaret anında yanıtı beklenen sorgunun dışında — bu uyarı yalnız sağlayıcının kart iadesinin sonucunu taşıyan bildirimiyle doğar — bildirim gelmezse ürün çakışmayı görmez, kalan risktir (K-978); yanıtsız son sorgu beş dakikada bir yinelenir (Teknik Mimari `05 §4`, §5, §10). Kart iadesinin sonucunu sorgulamak sağlayıcıdan beklenen yeteneklerdendir — sağlayıcı onu sunmuyorsa kart hattı o sağlayıcıyla kurulmaz (`10 §4.1` ÖK-4; `05 §2.2.2`, §4.6.3; K-959). Sorgunun biçimi ve sıklığı, sağlayıcının tekrar anahtarı ve kotası, son sorgunun sağlayıcıdan hata yanıtı aldığında ne yaptığı ve başarı yanıtı almayan bildirimi sağlayıcının yeniden gönderip göndermediği — kaç kez, hangi sürede; gönderilmezse Bekliyor siparişin sonucunu ödeme süresinin sonundaki son sorgu bulur, iptal edilmiş siparişe gelen geç ödemenin bildirimi ve firmanın işaretinden sonra kart iadesinin geç sonucunu taşıyan bildirim ise kalan risktir (`05 §10.1.3`, §4.6.2; K-978) —, sağlayıcının kart iadesinin sonucunu bildirimle gönderip göndermediği (K-978; `05 §4.6.2`), bildirimin tekil kimliği ve olay türleri bu aşamanın kararıdır. Aşama 2'nin K-705 satırı bu satırla okunur: (1)'de iade isteğinin tek işlenmesinin ilkesi K-894 ile Teknik Mimari'dedir (`05 §4.6`) — bu dokümana sorgunun biçimi ve sıklığı ve tekrar anahtarı kalır — ve "işaret gerekirse karar kaydına döner" cümlesi K-940 ile kapandı; (2)'de doğrulanamayan ve tutarı tutmayan bildirimin ele alınışı `05 §4.5`'tedir (K-893, K-941), imzanın ve tutar karşılaştırmasının biçimi bu dokümandadır (satır Aşama 4'ün çakışma taramasında hizalandı).
> - **K-906 · K-973 · Ödeme sağlayıcısından dönüş:** çerezler SameSite=Lax niteliğini taşır; sağlayıcıdan dönüş başka bir siteden gelen istektir ve sipariş sayfasına — misafir alıcı dahil — oturum çerezine dayanmadan inmelidir (`05 §6`; `02 §3.17.1`). Dönüş durum yazmaz — yalnız sipariş sayfasına yönlendirir, ödeme sonucunu bildirim ve son sorgu yazar; dönüş POST'la geliyorsa uç sahteciliğe karşı korumanın ikinci istisnasıdır (`05 §6.5.1`). Dönüşün yolu ve yöntemi — GET ya da POST — bu aşamanın ve API Tasarımı'nın (`07`) kararıdır.
> - **K-913 · K-941 · Gönderim sonucunun sınıfı:** e-posta gönderiminin her denemesi üç sonuçtan birini alır — gönderildi; alıcı hatası (adres geçersiz, posta kutusu yok ya da dolu); kanal hatası (servise bağlanılamaz, kimlik reddedilir, servis genel bir hata ya da kota aşımı döner) —; kanal uyarısı art arda kanal hatalarından açılır (Teknik Mimari `05 §10`). Seçilen servisin yanıt kodlarının bu sınıflara eşlenmesi ve servisin sonradan bildirdiği geri dönmenin (bounce) gönderim kaydına işlenmesi bu aşamanın kararıdır. Geri dönme bir web kancasıyla alınırsa uç imzalı makine ucudur: bağlantı katmanında imzasıyla doğrulanır, doğrulanamayan istek kayda yazılmaz; uç MK-04'ün listesine, sahteciliğe karşı korumanın istisnasına ve IP sınırının muafiyetine karar kaydıyla girer (Teknik Mimari `05 §3.6.1`).
> - **K-935 · K-942 · Ödeme sağlayıcısının çerçevesi ve içerik güvenlik politikası:** sitenin her yanıtı içerik güvenlik politikası taşır — betik yalnız sitenin kendisinden yüklenir; form gönderimi ve onu izleyen yönlendirme yalnız sitenin kendisine, ödeme sağlayıcısının adresine ve Google'ın yetkilendirme adresine gider; sağlayıcının çerçevesi kullanılıyorsa yalnız o sayfada ve yalnız sağlayıcının adresinden açılır (Teknik Mimari `05 §6.5.4`; `02 §8.3.4`). Sağlayıcının çerçevesinin ve yönlendirmesinin istediği izinler ve adresleri ve Google'ın yetkilendirme adresinin politikadaki biçimi bu aşamanın kararıdır.
> - **K-932 · Bağlantı katmanı ve desteklenen ödeme sağlayıcısının sayısı:** üç dış servis — ödeme sağlayıcısı, e-posta altyapısı, Google — bir bağlantı katmanının arkasındadır; modül ürün düzeyindeki işi çağırır, sağlayıcıya özgü biçim — istek, imza, yanıt kodları ve sınıfları — katmandadır (Teknik Mimari `05 §3.6.1`). Ürün kuralı çekirdekte kalır: sağlayıcıya bağlanılamadığı için ödeme sayfası açılmamış siparişin iptali ödeme denemesi sayılmaz (L-7; `02 §8.2`) — bağlantı hatasının ödeme hatasından ayırt edilmesi bu aşamanın işidir (`05 §3.6.2`). **Desteklenen ödeme sağlayıcısının sayısı bu aşamanın kararıdır ve aşamanın açılışında proje sahibine sorulacak çok kritik soru adayıdır** (`05 §3.6.4`, §2.2.2) — soru açılmadan önce L1–L5'e karşı taranır (`INSTRUCTIONS §2`). Katmanın sağlayıcı başına biçimi bu aşamanın kararıdır (satır Aşama 4'ün çakışma taramasında açıldı).
> - **K-900 · K-942 · K-968 · K-973 · Google ile giriş:** Google ile giriş ve yeniden doğrulama OpenID Connect'in yetki kodu akışıdır — `state`, `nonce` ve PKCE kullanılır; dönüş Google'dan sorgu dizgesiyle (GET) gelir, form_post kullanılmaz; dönüş adresi Google'da tam adresle kayıtlıdır; yalnız Google'ın doğrulanmış verdiği e-posta kabul edilir, ürün Google'dan yalnız adı ve e-postayı alır ve Google hesabının kalıcı kimliğini saklamaz; dönüşü eşleyen tek kullanımlık değerler — `state`, `nonce` ve PKCE doğrulayıcısı — akışın geçici çerezinde ya da çatının koruduğu `state` değerinin içindedir; çerez Secure, HttpOnly, SameSite=Lax, yolu kök, ömrü akış kadar (Teknik Mimari `05 §6.1.4`, 6.7.12; `02 §8.1.2`, §12.2.8; K-973). Yeniden doğrulamada istek kimlik belirtecinin doğrulama anını (`auth_time`) ister ve yalnız yeniden doğrulamanın başlatıldığı andan sonraki bir Google girişi kabul edilir — açık Google oturumuyla sessiz dönüş yeniden doğrulama sayılmaz; Google yeniden girişi zorlayan bir parametre (OpenID Connect'in `max_age`'i) destekliyorsa istek onu da taşır (`05 §6.1.5`; K-968). Doğrulama anının istekte ve Google'daki ayarda nasıl açıldığı ve Google'ın `max_age`'i destekleyip desteklemediği de bu aşamanın kararıdır. Değerlerin çerezde mi korumalı `state`'te mi taşındığı, istemci kimliği ve sırrı, dönüş adresi, kapsam ve akışın biçimi bu aşamanın kararıdır.
> - **Dizin — Aşama 4 kararlarının bu dokümana devirleri:** karar kaydında K-855…K-979'un etki sütunlarında bu dokümanı gösteren yirmi bir atıf (yirmi karar) ve Teknik Mimari'nin (`05`) gövdesinde bu dokümana iş bırakan cümleler [`CHECKPOINT_REPORTS/PHASE4_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE4_CONFLICT_SCAN.md) §6'dadır; Aşama 4'ün atıflarının hepsi işin adını taşır. Desteklenen ödeme sağlayıcısının sayısı aşamanın açılışında proje sahibine sorulacak çok kritik soru adayıdır (yukarıdaki K-932 satırı). Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Önceki aşamaların dizinleri: `PHASE1_CONFLICT_SCAN.md` §6, `PHASE2_CONFLICT_SCAN.md` §6, `PHASE3_CONFLICT_SCAN.md` §6.

---

## 0. Yazım kuralları

- **Her entegrasyon ayrı bölüm.** API limitleri, hata senaryoları, retry stratejisi, geri düşme planı.
- **Bağımlılık riski:** Her entegrasyon için *"bu servis çökerse ne olur?"* cevaplanır.
- **Alan eşlemesi yazarken `06` açık tutulur** ve her alan adı kontrol edilir. Bu aşama, veri modeliyle uyumsuz alan adlarının en sık yakalandığı yerdir.
- **"Ücretsiz seçenek yeter mi?" sorusu ÖNCE sorulur.** Ücretli bir sağlayıcı seçilip sonra ücretsizin yettiğini keşfetmek sık görülen bir israftır.
- **Dış çağrıyı yapıp yapmamayı belirleyen kural burada da yazılır.** Bir iş kuralı (ör. minimum eşik) dış servis çağrısının yapılıp yapılmayacağını belirliyorsa, kaynağı `02` olsa bile burada tekrar edilir.

---

## 1. Entegrasyon envanteri

| # | Servis | Ne için | Kritiklik | Ücretli mi | Kota / limit |
|---|---|---|---|---|---|

## 2. Entegrasyon tanımları

### 2.x <Servis adı>

- **Amaç:**
- **Kimlik doğrulama:** (key türü, nerede saklanır, nasıl döndürülür)
- **Kullanılan uçlar:**
- **Alan eşlemesi:** (dış alan → iç entity.alan — `06` ile kontrol edilmiş)

| Dış alan | İç alan | Dönüşüm | `06` doğrulandı |
|---|---|---|---|

- **Hız limitleri ve kota:**
- **Hata senaryoları ve kodları:**
- **Retry stratejisi:** (kaç deneme, hangi gecikme, hangi hatalarda)
- **Idempotency / tekilleştirme anahtarı:**
- **Geri çağrım / webhook:** (imza doğrulama **zorunlu**, tekrar gönderim davranışı)
- **Servis çökerse:** (geri düşme, kullanıcıya ne gösterilir, süreler donar mı)
- **Test / sandbox imkânı:**
- **Runbook:** [`INTEGRATION_RUNBOOKS/<SERVIS>.md`](INTEGRATION_RUNBOOKS/)

## 3. Ortak hata yönetimi

## 4. Dış varsayımlar

> **Ne yazılır:** Bu dokümanın dayandığı her dış varsayım ve **doğrulama kanıtı**. Task'ların ön-uçuş kontrolü (00 §F.3) bu tabloyu başlangıç noktası alır.

| # | Varsayım | Kanıt | Doğrulama tarihi |
|---|---|---|---|

## 5. `07`'ye geri yansıtılanlar

| # | Yeni ihtiyaç | `07`'de eklenen |
|---|---|---|

---

*Shopfolio — Integration Specifications v0.1*
