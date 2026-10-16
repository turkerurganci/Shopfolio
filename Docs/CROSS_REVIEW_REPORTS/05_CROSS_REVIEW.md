# Cross-Review — 05 Technical Architecture (Tur 1)

**Tarih:** 2026-10-17 · **Hedef:** `Docs/05_TECHNICAL_ARCHITECTURE.md` v0.5 (girdi commit'i `a67f8a7`, 300 641 bayt, 1 478 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren uygulanır (K-952) · **Koşum:** iki biçimde, altı çağrı — tek parça çağrı ve beş parçalı kontrol koşumu (K-953); turun sonucu ikisinin birleşimidir · **Girdi:** yalnız doküman ve boş şablonu (`58cf0b9` — Aşama 4 açılmadan önceki v0.1; park blokları çıkarılmış, 87 satır), stdin'den tek metin, boş ve izole bir çalışma klasöründe; karar kaydı, Ürün Gereksinimleri, Kullanıcı Akışları, Arayüz Tanımları, MVP Kapsamı ve audit/deep review raporları verilmedi (K-431)

## 1. Koşum

**İstem.** Yedi kriter, bulgu biçimi (`BULGU-N` — Kriter · Seviye · Yer · Alıntı · Sorun · Öneri — ya da `SONUÇ: TEMİZ`), `cross-review` skill'inin Faz 1 madde 4'teki bilinçli karar cümlesi değiştirilmeden ve ciddiyet ölçüsünün paragrafı (K-847'nin metni). İstem belgenin ürün kuralı koymadığını söyler — kural ayrıntısı Ürün Gereksinimleri'nde, adımlar Kullanıcı Akışları'nda, ekranlar Arayüz Tanımları'nda, kapsam MVP Kapsamı'nda yaşar ve dördü verilmedi — ve ölçüye mimari dokümanı için tek bir okuma paragrafı ekler: teknik doğruluk ve güvenlik dört sınıfın içinde okunur, savunmada derinlik bulgu değildir (K-952). Koşum öncesinde çağrı kısa bir deneme metniyle doğrulandı. **2. tur aynı istemle koşar** — metnin tamamı aşağıdadır; girdi `istem + "=== DOKÜMAN ===" + güncel doküman + "=== ŞABLON ===" + şablon` biçiminde kurulur. Şablon: `git show 58cf0b9:Docs/05_TECHNICAL_ARCHITECTURE.md`'nin 9–32. satırları (üç park bloğu) çıkarılmış hâli — park blokları dokümanın başlık notunda zaten vardır.

> ```text
> Sen bağımsız bir doküman denetçisisin. Aşağıda "=== DOKÜMAN ===" işaretinden sonra bir teknik mimari (technical architecture) dokümanının tam metni, "=== ŞABLON ===" işaretinden sonra o dokümanın boş şablonu var. Başka bir dosya okuma; çalışma klasörü boştur.
>
> Bağlam: Belge bir teknik mimari dokümanıdır. Ürün kuralı koymaz, sistemin biçimini kararlaştırır (başlık notu, konvansiyon 1): iş kurallarının ayrıntısı — süreler, limitler, durumlar, saklama süreleri, bildirimler — ayrı bir Ürün Gereksinimleri dokümanında (`02`), akış adımları ayrı bir Kullanıcı Akışları dokümanında (`03`), ekranlar ayrı bir Arayüz Tanımları dokümanında (`04`), kapsam ayrı bir MVP Kapsamı dokümanında (`10`) durur; dördü de sana verilmedi. Bu doküman o kuralları taşıyacak yapıyı yazar — kısıtlar, teknoloji, servis yapısı, durumun ve olayların yeri, arka plan işleri, güvenlik, veri katmanı, ortamlar, gözlemlenebilirlik, hata toleransı, büyüme yolu — ve kendi mimari kararlarını MK-nn kimliğiyle koyar. Kural ayrıntısının burada yeniden yazılmamış olması ya da bir kurala yalnız bölüm numarası ve kimlikle (`02 §…`, `03` …, `04` …, Z-, L-, P-, H-, B-, F-, KP-, ÖK-, SK-, E-) işaret edilmesi tek başına bulgu değildir; o dokümanların içeriğini denetleme. Veri şeması, API ve kod adları sonraki dokümanların (`06`, `07`, `08`, `09`) işidir; burada beklenmez. Dokümandaki `K-123` gibi işaretler sana verilmeyen bir karar kaydına gider. Yazım konvansiyonları başlık notundadır ("Yazım konvansiyonları", 1–12). Proje Aşama 4'tedir (teknik mimari); kod henüz yazılmadı. Ürün, Türkiye'de tüketiciye satış yapan tek firmalık bir e-ticaret + kurumsal site ürünüdür; her firma ürünü kendi kiraladığı tek bir sanal sunucuya kurar.
>
> Dokümanı yedi kriterle denetle: tutarlılık · eksiklik · belirsizlik · teknik doğruluk · edge case · güvenlik · kullanıcı deneyimi.
>
> Dokümanda açıkça bilinçli karar, elenen seçenek ya da kabul edilmiş risk olarak yazılmış bir seçimi, yalnız seçime katılmadığın için bulgu yapma. Böyle bir karar dokümanın kendi içinde çelişki, olgusal ya da hukuki hata doğuruyorsa yaz.
>
> Yalnız CİDDİ sorunları yaz: (1) yürürlükteki mevzuata aykırılık, (2) müşteriye ya da firmaya para kaybı veya hak kaybı doğuran kural boşluğu, (3) kullanıcının takılıp kaldığı, çıkışı olmayan bir akış, (4) dokümanın iki yerinin birbiriyle çelişmesi. Nadir kenar durumları, iyileştirme önerilerini, savunmada derinlik önerilerini, üslup ve ifade tercihlerini bulgu yapma. Böyle bir sorun yoksa SONUÇ: TEMİZ yaz.
>
> Teknik doğruluk ve güvenlik bu dört sınıfın içinde okunur: yazılan bir mekanizmanın dokümanın kendi koyduğu bir güvenceyi (ör. sipariş tarafında veri kaybı olmaması, bir isteğin tek işlenmesi) sağlamaması (4)'tür; bir mekanizmanın siparişin, paranın ya da kişisel verinin kaybına veya kişisel verinin yetkisiz kişiye açılmasına yol açması (1) ya da (2)'dir. Ek bir koruma katmanı, ek izleme, daha güçlü bir araç ya da algoritma önerisi savunmada derinliktir ve bulgu değildir.
>
> Çıktı biçimi — her bulgu için (Türkçe):
>
> BULGU-N
> Kriter: <yedi kriterden biri>
> Seviye: Yüksek | Orta
> Yer: <bölüm / madde kimliği>
> Alıntı: "<dokümandan aynen>"
> Sorun: <ne yanlış, neden ciddi — dört sınıftan hangisi>
> Öneri: <somut düzeltme>
>
> Bulgu yoksa yalnız şunu yaz: SONUÇ: TEMİZ
> Bulgu varsa listenin sonuna şunu yaz: SONUÇ: <N> BULGU
> ```

**Komut** (Git Bash; çalışma klasörü boş): `codex exec -s read-only -C <boş klasör> --skip-git-repo-check --ephemeral --ignore-user-config --color never -m gpt-5.6-terra -c 'model_reasoning_effort="high"' -o <çıktı> - < <girdi>`. Altı çağrı paralel koştu.

**Parçalama — kontrol koşumu (K-953).** Doküman skill'in 400 KB eşiğinin altındadır; eşik "sonraki büyük dokümanın 1. turunda yeniden okunur" dediği için 1. tur iki koşumla ölçüldü ve sonraki turların kuralı turdan önce yazıldı (K-953). Parçalar bölüm sınırındadır; her biri başlık notunu (satır 1–96: sürüm notları, park blokları, yazım durumu, on iki yazım konvansiyonu, alt bölüm haritası, geri besleme tablosu) ve şablonu taşır. Satır numaraları v0.5'indir.

| Parça | Bölümler | Satırlar | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|---|
| — | Tek parça — dokümanın tamamı | 1–1478 | 308 628 bayt | 117 807 | 214 sn | 2 bulgu |
| 1 | Başlık notu · §0 · §1 · §2 · §3 | 1–435 | 99 366 bayt | 46 917 | 61 sn | `SONUÇ: TEMİZ` |
| 2 | §4 durum ve olaylar · §5 arka plan işleri | 436–611 | 73 595 bayt | 57 390 | 201 sn | 3 bulgu |
| 3 | §6 güvenlik (§6.x dahil) | 612–817 | 87 762 bayt | 33 746 | 93 sn | 1 bulgu |
| 4 | §7 veri katmanı | 818–952 | 76 279 bayt | 64 882 | 237 sn | 1 bulgu |
| 5 | §8 ortamlar · §9 gözlemlenebilirlik · §10 hata toleransı · §11 ölçeklenebilirlik · §12 özet | 953–1478 | 142 504 bayt | 81 316 | 171 sn | 1 bulgu |

Parçalı çağrıların istemine tek bir paragraf eklendi (bilinçli karar cümlesi ve ölçü değişmedi; `<bölümler>` yukarıdaki tablonun adlarıdır):

> ```text
> Parça notu: Doküman bu çağrıda parça parça verilmektedir. Aşağıdaki metin dokümanın başlık notu (sürüm, park edilen girdiler, yazım durumu, yazım konvansiyonları, alt bölüm haritası, üst dokümanlara geri besleme) ile <bölümler> bölümlerinden oluşur; öteki bölümler bu çağrıda verilmedi. Verilmeyen bir bölüme yapılan atıf ve orada yaşayan içeriğin burada görünmemesi tek başına bulgu değildir; yalnız verilen metnin içindeki sorunları yaz.
> ```

**Kapsam sınırı.** Parça öteki bölümleri görmez; parçalar arası okumayı yalnız tek parça çağrı yapar — bu turun işlem günlüğü bulgusu (§7.6.9'un nabzı §5.4.1'le) oradan geldi. Yönetici değerlendirmesi her iddiayı dokümanın tamamına karşı okudu (§3). **Web araması:** `--ignore-user-config` aramayı kapatmıyor; parça 2, 4 ve 5 web araması yaptı — 6563 sayılı Kanun m.11/3 (mevzuat.gov.tr), PostgreSQL'in `archive_timeout` belgesi ve 5651'in trafik bilgisi; tek parça çağrı, parça 1 ve parça 3 aramadı (koşum kayıtlarındaki `web search` satırları). Girdi kuralı (K-431) dokümanın dışındaki proje bilgisini yasaklar; kamuya açık mevzuat ve araç belgesi proje bilgisi değildir — yine de aramanın açık olduğu rapora yazılır.

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**Tek parça çağrı:**

> ```text
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §7.6.9
> Alıntı: "nabız (§5.4.1) veritabanını hiç boşta bırakmadığı için her dakika tam boy (16 MB) bir günlük dosyası kapanır"
> Sorun: Veritabanına nabız kaydı yazmak WAL segmentini her dakika doldurmaz veya kapatmaz. `archive_timeout` ya da eşdeğer bir zorunlu WAL switch mekanizması yazılmadığından, düşük trafikte son arşivlenmiş WAL dakikalar/saatler geride kalabilir. Bu, §0.13 ve §7.6.2’deki “en çok son bir dakika” veri kaybı güvencesini sağlamaz; kalıcı disk kaybında sipariş ve ödeme verisi kaybına yol açar (2 ve 4).
> Öneri: PostgreSQL’de en fazla bir dakikalık `archive_timeout`/zorunlu WAL switch mekanizmasını açıkça tanımlayın; aktarımın başarıyla tamamlandığını son arşivlenmiş WAL konumu ve zamanı üzerinden denetleyin. Nabız yazımını bu güvencenin gerekçesi olmaktan çıkarın.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §4.6.2
> Alıntı: "Firmanın işareti sorguyu durdurur: işaret firmanın sağlayıcının panelinde gördüğüne dayanan beyanıdır; sonradan sağlayıcıdan gelen başarılı sonuç — geç bildirim — siparişe işlenmez"
> Sorun: Firma, sonucu bilinmeyen iade için “gerçekleşmedi” işareti koyunca yeniden iade veya havale yolu açılıyor; ardından gelen gerçek başarılı sağlayıcı sonucu ise kanonik ödeme durumuna işlenmiyor. İlk iade gerçekte gerçekleşmişse müşteri ikinci kez ödeme alabilir. Bu, aynı bölümün “Para iki kez gitmez” ilkesini bozar ve firmaya doğrudan para kaybı doğurur (2 ve 4).
> Öneri: Firma işareti sağlayıcı sorgusunu durdurmamalıdır. Geç gelen başarılı sonuç kanonik geri ödeme durumuna işlenmeli; alternatif ödeme henüz yapılmadıysa engellenmeli, yapılmışsa çift ödeme için zorunlu uzlaştırma kaydı ve görünür işlem yolu açılmalıdır.
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
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §4.5.5
> Alıntı: “Hiçbir siparişe bağlanmayan satır altı ay sonra imha işinde silinir”
> Sorun: İmzası doğrulanmış ödeme sağlayıcısı bildirimi; referans, tutar, sonuç ve zaman bilgisiyle elektronik ticaret işlemine ilişkin bir kayıttır. Siparişe bağlanamaması, onu altı ayda silinebilir yapmaz. Bu, 6563 sayılı Kanun m.11/3’teki on yıllık saklama yükümlülüğüne aykırılık riski doğurur (sınıf 1).
> Öneri: İmzası doğrulanmış, siparişle eşleşmeyen bildirimleri de on yıl saklanacak mutabakat kayıtları kapsamına alın; ilişkilendirme durumu ve inceleme sonucunu silmek yerine kayda ekleyin.
>
> BULGU-2
> Kriter: Edge case
> Seviye: Yüksek
> Yer: §4.6.2
> Alıntı: “Firmanın işareti sorguyu durdurur: ... sonradan sağlayıcıdan gelen başarılı sonuç — geç bildirim — siparişe işlenmez”
> Sorun: Firma belirsiz iade isteğini “gerçekleşmedi” diye işaretlediğinde yeniden kart iadesi veya havale yolu açılır. İlk isteğin sağlayıcıda sonradan başarılı olduğu bildirildiğinde bu sonuç siparişe uygulanmadığından, firma ikinci ödemeyi yapabilir. Panel uyarısı, ikinci ödeme yolunu teknik olarak kapatmadığı için müşteriye çift iade ve firmaya para kaybı doğurur (sınıf 2).
> Öneri: Belirsiz iadenin “gerçekleşmedi” sayılması yalnız sağlayıcıdan kesin ret/iptal sonucu alındığında yeniden ödeme yollarını açsın. Geç başarılı sonuç, önceki işareti geçersiz kılıp tüm ek iade yollarını atomik olarak kapatsın.
>
> BULGU-3
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §4.3.2 / §5.1.3
> Alıntı: “Döngü satırları sırayla, birer birer işler — görsel türevleri dahil” / “Uzun işler — imha, görsel türevleri — sınırlı eşzamanlılıkla koşar”
> Sorun: Görsel türevlerinin tek tek, sırayla mı yoksa sınırlı paralellikle mi çalışacağı çelişkilidir (sınıf 4). Kuyruğun işleme, kilit ve kaynak planı bu iki modele göre farklılaşır; uygulama hangi güvenceyi sağlaması gerektiğini belirleyemez.
> Öneri: Tek modeli açıkça seçin: görsel türevleri kuyrukta tekil işlenecekse paralellik ifadesini kaldırın; paralel işlenecekse iş satırı kiralama, eşzamanlılık sınırı ve dış ödeme/son-sorgu işlerinden izolasyon kuralını tanımlayın.
>
> SONUÇ: 3 BULGU
> ```

**Parça 3 — §6:**

> ```text
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Orta
> Yer: §6.1.4, §6.7.12
> Alıntı: "Google ile giriş ve yeniden doğrulama OpenID Connect'in **yetki kodu akışıdır:** `state`, `nonce` ve PKCE kullanılır"
> Sorun: PKCE için gerekli `code_verifier`ın dönüşte token değişiminde kullanılmak üzere nerede saklandığı yazılmamıştır; akış çerezinin envanteri yalnız `state` ve `nonce` eşini taşır. Bu hâliyle dokümanın “PKCE kullanılır” güvencesi mekanizma tarafından sağlanmaz (kendi içindeki güvenlik güvencesiyle çelişki).
> Öneri: Akış başlatılırken üretilen `code_verifier`ın, `state` ile bağlı ve tek kullanımlık olarak, dönüşte erişilebilir geçici sunucu kaydında veya şifreli akış çerezinde tutulacağını; dönüşte token değişiminde kullanılınca silineceğini açıkça belirtin.
>
> SONUÇ: 1 BULGU
> ```

**Parça 4 — §7:**

> ```text
> BULGU-1
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §7.4.5
> Alıntı: “Süreye bağlı olmayan anlık silmeler — müşterinin hesabını silmesi [...] geri gelir [...] müşterinin sitede kendi yaptığı silme bilinemez”
> Sorun: Eski bir yedeğe dönüş, müşterinin silmiş olduğu hesabı ve kişisel verilerini yeniden erişilebilir hâle getirir. Doküman, müşterinin yaptığı silmenin tekrar uygulanamadığını açıkça kabul ediyor. Bu, silme talebinin/işleminin geri alınmasına ve kişisel verinin hukuka aykırı yeniden işlenmesine yol açar; kabul edilmiş risk olsa da KVKK bakımından mevzuata aykırıdır.
> Öneri: Süreye bağlı olmayan her silme için, yedekten bağımsız ve erişimi kısıtlı asgari bir silme niyeti kaydı tutulmalı; geri dönüş tamamlanmadan bu kayıtlar yeniden uygulanmalı ve hizmet açılmamalıdır. Bu kayıtlar, ilgili eski yedeğe dönüş penceresi tamamen kapandıktan sonra imha edilmelidir.
>
> SONUÇ: 1 BULGU
> ```

**Parça 5 — §8–§12:**

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §9.2.1, §9.2.3
> Alıntı: "Sistem kayıtları ... kaydın yazıldığı andan P-45 kadar saklanır" / "sunucu kaybolsa ya da ele geçirilse de son geceye kadarki kayıtlar silmeye karşı korunan yedekte durur ... yedekteki kopyalar otuz günde düşer."
> Sorun: P-45 bugün altı aydır; ancak sunucu dışındaki kayıt yedeği yalnız otuz gün korunur. Sunucu kaybı veya ele geçirilmesinde otuz günden eski, hâlen saklanması gereken sistem kayıtları kaybedilir. Bu, kaydın P-45 boyunca korunacağı ve sunucu kaybında yedekten erişilebilir olacağı ifadeleriyle çelişir (sınıf 4) ve firmanın ihlal/erişim incelemesi için gerekli kayıtları kaybetmesine yol açabilir (sınıf 2).
> Öneri: Sistem kayıtlarının sunucu dışı, şifreli ve silmeye karşı korunan yedek saklama süresini P-45 ile eşitleyin; otuz günlük yedek kuralını yalnız bu saklama yükümlülüğü olmayan kayıt türleri için uygulayın.
>
> SONUÇ: 1 BULGU
> ```

## 3. Bağımsız değerlendirme

Her bulgu dokümanın tamamına (v0.5), ilgili yerde Ürün Gereksinimleri'ne (`02` v0.72), Arayüz Tanımları'na (`04` v0.24) ve karar kaydına karşı okundu — ikinci model bunları görmedi, parçalı çağrılar dokümanın öteki bölümlerini de görmedi. Tek parçanın BULGU-2'si ile parça 2'nin BULGU-2'si aynı yer ve aynı iddiadır; tek satırda değerlendirildi.

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| Tek · BULGU-1 | ✅ KABUL | **Mekanizma yazılı değildi ve 7.6.9'un nedensellik cümlesi teknik olarak yanlıştı.** PostgreSQL günlük dosyasını ancak dolunca (16 MB) ya da arşiv zaman aşımı (`archive_timeout`) dolduğunda kapatıp arşiv komutuna verir; nabzın otuz saniyede bir yazdığı birkaç yüz bayt bir dosyayı dakikada değil günler içinde doldurur. Doküman "en geç bir dakikada" güvencesini (§0.13, 7.6.1, 7.6.2; K-878, K-888) koyuyor ama onu sağlayan ayarı hiçbir yerde anmıyordu; 7.6.9 dakikalık kapanışı nabza bağlıyordu. Ayar yazılmadan kurulsa az trafikte aktarım saatlerce geride kalır ve diskin kalıcı kaybında pencere bir dakikayı aşar — dokümanın kendi güvencesiyle çelişki (ölçünün (4). sınıfı). 23 GB'lık günlük hesap doğrudur, ama sebebi zaman aşımıdır: nabız veritabanını boşta bırakmadığı için zaman aşımı her dakika dosyayı kapatır ve her dosya tam boy yer tutar. Önerinin ikinci yarısı — aktarımın son arşivlenen konum ve zamanla denetlenmesi — zaten yazılıdır (7.6.9, §9.4.4: aktarım on dakika durursa uyarı). | 7.6.1: arşiv zaman aşımı altmış saniye — açık dosya en geç bir dakikada kapatılır ve taşınır; ayar olmadan dosya ancak 16 MB dolunca kapanırdı · 7.6.9: dakikalık kapanış zaman aşımına, tam boy nabza bağlandı · §12 MK-25 "arşiv zaman aşımıyla". Yeni karar yok — K-878'in "en geç bir dakika"sının mekanizmasıdır. |
| Tek · BULGU-2 = P2 · BULGU-2 | ⚠️ KISMİ | **Mekanizma bilinçli karardır; dokümanda eksik olan kalan riskin adıydı.** Firmanın işaretinin sorguyu durdurması ve geç gelen başarılı sonucun siparişe işlenmemesi K-940'ın kararıdır (⚠) ve ürün kuralı `02 §7.2.8`'de yazılıdır; 4.6.4 "yolların yalnız sorgunun sonucuyla açılması"nı gerekçesiyle eler — sağlayıcının uzun kesintisinde yasal süre (Z-17) işlerken firmanın elinde yol kalmaz. Modelin önerisi (işaret sorguyu durdurmasın, geç sonuç siparişe işlensin ve ikinci ödeme yolunu atomik kapatsın) bu kararı değiştirir ve dört dokümana döner; firmanın ikinci ödemesi çoğu kez çoktan yapılmış olur — sonucu siparişe işlemek kaydı iki ödemeyi görmeden "geri ödendi" yapardı. **Gerçek olan:** çift ödeme olasılığı Arayüz Tanımları'nda yazılıdır (`04` 9.9.8 — *"aynı para ikinci kez gönderilmiş olabilir"* kalıcı uyarısı) ama `05`'te yoktu; §12'nin MK-40 gerekçesi ise *"Para iki kez gitmez"* diyordu — dokümanın iki yeri farklı şey söylüyordu (ölçünün (4). sınıfı). Yeni karar gerekmez: sonuç `02 §7.2.8` ve `04` 9.9.8'de zaten kabul edilmiştir. | 4.6.2: **kalan risk** — firmanın işaretten sonraki ikinci ödemesi ile ilk isteğin sonradan gerçekleşmesi çakışırsa aynı para ikinci kez gitmiş olur; ürünün payı kalıcı uyarıdır, iki ödemeyi ürün kendisi çözmez (`02 §7.2.8`; `04` 9.9.8) · §12 MK-40 gerekçesi: "sistem parayı kendiliğinden iki kez göndermez" ve çakışma kalan risk. |
| P2 · BULGU-1 | ✅ KABUL | **6563 m.11/3 tanınmayan bildirimde tartılmamıştı.** Altı ay K-941'in kararıdır ve gerekçesi yalnız kurtarmadan sonraki karşılaştırmaydı; K-941, 6563 m.11/3'ün tartıldığı K-951'den öncedir. İmzası doğru bildirim sağlayıcıdaki bir ödeme işleminin kaydıdır; diskin kalıcı kaybından sonra siparişi kaybolmuş bir ödemenin ürünün içindeki tek izi odur (7.6.2). Proje sahibi ödemesi alınmış siparişe bağlı kayıtlar için on yılı seçti (K-951); satır kişisel alan taşımadığı için on yılın KVKK bedeli yoktur. Ölçünün (1). sınıfına girer (mevzuat riski). Karar ⚠ öneriyle kaydedildi — hukuki yorumdur ama seçenekler yakın, bedel yok ve geri alınabilir. | 4.5.5: siparişe bağlanmayan satır **on yıl** — elektronik ticaret kayıtlarının on yılını izler, kişisel veri saklamasını uzatmaz (K-954) · 7.4.4 "on yılı dolan" · §4.5'in Kaynak satırı (K-951, K-954, `02 §12.2.7`; Z-40 çıktı) · §12 MK-39 · Veri Modeli'nin park satırı. |
| P2 · BULGU-3 | ✅ KABUL | **İki yerin metin farkı.** 4.3.2 kuyruğun satırları — görsel türevleri dahil — birer birer işlediğini söyler (K-946'nın düzeltmesi); 5.1.3 ise "uzun işler — imha, görsel türevleri — sınırlı eşzamanlılıkla koşar" diyordu — yazım turunun eski cümlesi. Doğru olan 4.3.2'dir; imha zamanlayıcının küçük parçalarla, her parçası kendi işleminde çalışan işidir (7.4.1). Ölçünün (4). sınıfı, düşük şiddet; önerinin ilk yolu uygulandı. | 5.1.3: imha küçük parçalar hâlinde, her parça kendi işleminde (7.4.1); görsel türevleri iş kuyruğunda birer birer (4.3.2). |
| P3 · BULGU-1 | ⚠️ KISMİ | **Envanter satırı PKCE doğrulayıcısını saymıyordu.** 6.1.4 PKCE'yi güvence olarak yazıyor, 6.7.12'nin akış çerezi satırı yalnız `state` ve `nonce` eşini sayıyordu — iki yerin metin farkı (ölçünün (4). sınıfı, düşük şiddet). "Güvence mekanizmayla sağlanmaz" iddiası tutmaz: akışın biçimi `08`'in, çerezin biçimi `09`'un işidir (6.1.4, 6.7.1) ve doğrulayıcının dönüşte kullanılması akışın tanımıdır. Önerinin "geçici sunucu kaydı" seçeneği uygulanmadı — dönüşü eşleyen değerler 6.1.4'e göre zaten akışın geçici çerezindedir; ikinci bir yer gerekmez. | 6.1.4 ve 6.7.12: dönüşü eşleyen ve dönüşte kullanılan tek kullanımlık değerler — `state`, `nonce` ve PKCE doğrulayıcısı — akışın geçici çerezindedir. |
| P4 · BULGU-1 | ⚠️ KISMİ | **Kalan risk bilinemezlikten doğuyordu ve bir kayıt satırıyla kapanır.** 7.4.5 eski bir ana dönüşte anlık silmelerin geri gelmesini bilinçli olarak kalan risk saydı (K-876, ⚠) — "müşterinin sitede kendi yaptığı silme bilinemez". Bilinçli karar cümlesi kararın hukuki hata doğurup doğurmadığını sormaya izin verir: geri gelen hesap yeniden girilebilir ve panelde görünür, silinmiş kişisel veri yeniden işlenir (KVKK m.7) — olay seyrek (yalnız veri bozulmasında) ama gerçekleştiğinde bir aykırılıktır. Gerekçenin öncülü ("bilinemez") bir tasarım boşluğuydu: uygulama kaydı veritabanının dışında, sunucunun diskindedir ve dönüşle geri gitmez; P-45 yedeğin otuz gününden uzundur. Modelin önerdiği **ayrı bir silme niyeti deposu** uygulanmadı — ikinci bir bileşen ve sır yüzeyidir (§0.2); uygulama kaydı aynı işi görür. "Geri dönüş tamamlanmadan hizmet açılmasın" kısmı karşılandı: komut uygulama başlamadan önce koşar. Karar ⚠ öneriyle kaydedildi (K-955; K-876'yı daraltır). | 7.4.5: her anlık silme uygulama kaydına tür ve iç kimlikle bir satır yazar; dönüşten sonra uygulama başlamadan önce kurulumu yapan **silmelerin yeniden uygulanması** komutunu çalıştırır (8.5.10) · 9.1.3'ün içerik listesi · §8.5'e 8.5.10 satırı · §7.4, §8.5, §9.1'in Kaynak satırları · §12 MK-72 · `DEPLOY_RUNBOOK`'un park satırı. |
| P5 · BULGU-1 | ⚠️ KISMİ | **İddia edilen çelişki yok; metin otuz günü kaydın süresi gibi okutuyordu.** Dosya yedeği her gece bir anlık görüntüdür (7.6.1) ve sunucudaki kayıt dizininin o geceki bütün hâlini taşır — sunucuda P-45 boyunca tutulan kayıtların tamamı en son anlık görüntüdedir; "otuz gün" anlık görüntülerin geçmişidir (7.6.6). Sunucu kaybolursa altı aylık kayıt en son anlık görüntüden döner; kayıp yoktur. Gerçek kalan yalnız şudur: sunucuyu ele geçiren eski kayıtları da silerse onlar silinmeden önceki anlık görüntülerde otuz gün durur — 9.2.3'ün kalan riskinin uzantısıdır, ölçünün sınıflarına girmez. Önerilen "kayıt yedeğini P-45 boyunca saklamak" uygulanmadı — imha edilen verinin yedekte yaşadığı süreyi bütün dosya yedeği için uzatır (7.6.10'un elediği "doksan gün ya da daha uzun saklama"). | 9.2.3: her gecenin anlık görüntüsü P-45 penceresinin tamamını taşır; otuz gün anlık görüntülerin geçmişidir · kalan riske ele geçirenin eski kayıtları silmesi eklendi. |

**Dağılım:** yedi ayrı bulgu — 3 KABUL · 4 KISMİ · 0 RET. **%100 kabul değil:** dört KISMİ'nin dördünde modelin önerisi ya da etki iddiası reddedildi — K-940'ın mekanizması korundu, "güvence sağlanmaz" ve "kayıt kaybolur" iddiaları tutmadı, ayrı silme deposu eklenmedi — ve en küçük düzeltme uygulandı. **RET'in olmaması:** ölçü ve okuma paragrafı turu dar tuttu; yedi bulgunun hiçbiri savunmada derinlik önerisi değildi ve hiçbiri audit'in uygulanmayan bulgularından ya da bilinçli bir kararın yalnız tercih farkından dönmedi — tek bilinçli karar (K-940) gerçek bir metin farkı taşıyordu (MK-40 ↔ `04` 9.9.8). İki bulgu yeni karar doğurdu (K-954, K-955 — ikisi de ⚠); beşi metni dokümanın kendi kararına ya da öteki yerine hizalar.

## 4. Ek bulgular

- **Aynı kalıbın taraması (Tek · BULGU-1):** "bir dakika" ve "16 MB" geçen öteki yerler — §0.13, 7.6.2, §12 MK-25, `DEPLOY_RUNBOOK`'un K-876 park satırı — güvenceyi ya da pencereyi anlatır; mekanizmayı yanlış bir sebebe bağlayan başka cümle yok. Arşiv zaman aşımının ayarı veritabanı imajıyla gelir (§8.2.2); runbook'un park satırı "araçların ayarları bu dokümanın kararıdır" der — ayrıca satır gerekmez.
- **Aynı kalıbın taraması (P2 · BULGU-1):** "altı ay" `05`'te yalnız 4.5.5'te gelen bildirim için geçiyordu (öteki geçişler sistem kayıtlarının P-45'idir); 7.4.4 "altı ayı dolan" ve 11.1.20 (yalnız etiket) okundu. Park satırlarında yalnız Veri Modeli'nin İş kuyruğu satırı süreyi taşıyordu — güncellendi; `07`, `08`, `09` ve `DEPLOY_RUNBOOK`'un K-941 satırları süre saymaz. Ürün Gereksinimleri gelen bildirim kaydını anmaz — geri besleme yok.
- **Aynı kalıbın taraması (P4 · BULGU-1):** "anlık silme" ve "kendi yaptığı silme" `05`'te 7.4.5'te ve 9.1.3'te, park satırlarında yalnız `DEPLOY_RUNBOOK`'ta geçer — ikisi güncellendi. Karar kaydında K-876'nın satırı geri işaret aldı.
- **Aynı kalıbın taraması (Tek · BULGU-2):** "sorguyu durdurur" `02`, `03`, `04`, `05` ve `08`'in park satırında aynı kuralı söyler; değişen yalnız `05`'in kalan risk cümlesi ve MK-40'ın gerekçesidir — ürün kuralı değişmedi. Gözlem (bulgu değil): firmanın işaretinden sonra sorgu durduğu için geç sonuç ancak sağlayıcının kendi bildirimiyle gelir; sağlayıcının iade sonucunu bildirimle iletip iletmediği `08`'in biçim kararının içindedir (`08`'in K-940 park satırı "başarılı sonuç ... uyarı olur" der) — ölçünün sınıflarına girmez, yeni talimat yazılmadı.
- **Mekanik tarama:** düzeltmelerin gövdeye eklediği K numaraları (K-951, K-954, K-955) bulundukları alt bölümün Kaynak satırında; Z-40 4.5.5'ten çıktı ve §4.5'in Kaynak satırından kaldırıldı. Başlık sürümü, yazım durumu notu ve dosya sonu dipnotu v0.6'yı gösterir. Karar kaydında çift numara yok.
- **Ciddi bir sorun görülmedi.** Değerlendirme sırasında ikinci modelin listesi dışında ölçünün dört sınıfından birine giren bir sorun saptanmadı.
- **Önceki turlardan açık kalanlar:** yok (1. tur).

## 5. Kullanıcı onay checklist'i

K-855 kararıyla (Aşama 4 boyunca ⚠ öneriyle kayıt, CI yeşilse `docs:` PR merge yetkisi ve yönetici + alt ajan düzeni) alt ajan değerlendirdi ve uyguladı (`05` v0.6). Dört karar öneriyle kaydedildi: **K-952** (turdan önce) — ciddiyet ölçüsü 1. turdan, skill'in ikinci koşulu; **K-953** (tur içinde) — tek parça çağrının yanında beş parçalı kontrol koşumu ve eşiğin yeniden okunması; ikisi de süreç kararıdır, ⚠ değildir. **K-954** (⚠) — siparişe bağlanmayan sağlayıcı bildirimi on yıl; **K-955** (⚠) — eski bir ana dönüşte anlık silmelerin uygulama kaydından yeniden uygulanması. ⚠ listesi 29 → 31. Ürün Gereksinimleri'ne, Kullanıcı Akışları'na, Arayüz Tanımları'na ve MVP Kapsamı'na dönen değişiklik yok; `05`'in geri besleme tablosuna satır girmedi. Park satırları: Veri Modeli (İş kuyruğu, gönderim kaydı, gelen bildirim — K-954), `DEPLOY_RUNBOOK` (Yedek, kurtarma ve prova — K-955).

- [x] Tek · BULGU-1 (kabul — arşiv zaman aşımı 7.6.1'de; 7.6.9'un sebebi düzeltildi)
- [x] Tek · BULGU-2 = P2 · BULGU-2 (kısmi — K-940'ın mekanizması korundu; kalan risk 4.6.2'de, MK-40 hizalandı)
- [x] P2 · BULGU-1 (kabul — K-954 ⚠: tanınmayan bildirim on yıl)
- [x] P2 · BULGU-3 (kabul — 5.1.3 4.3.2'ye hizalandı)
- [x] P3 · BULGU-1 (kısmi — PKCE doğrulayıcısı 6.1.4 ve 6.7.12'de; sunucu kaydı eklenmedi)
- [x] P4 · BULGU-1 (kısmi — K-955 ⚠: uygulama kaydı ve 8.5.10 komutu; ayrı depo eklenmedi)
- [x] P5 · BULGU-1 (kısmi — çelişki yok; 9.2.3 netleştirildi)
- [ ] Etki yansıtma (Faz 5) — döngü TEMİZ döndükten sonra, son turun raporunda

**Hukuki kontrol** (2026-10-17): iki bulgu yasal dayanak taşır. P2 · BULGU-1 — 6563 sayılı Kanun m.11/3'ün 7416 sayılı Kanunla eklenen son cümlesi (on yıl); metin audit'te resmî kaynaktan okunmuştu (K-951) ve model de mevzuat.gov.tr'deki metni aradı. P4 · BULGU-1 — KVKK m.7 (silme); iddia kararın gerekçesiyle tartıldı. Düzeltmeler yeni hukuki içerik eklemez — K-951'in kapsamını kişisel veri taşımayan bir kayda uygular ve silmenin kalıcılığını sağlar.

**Yakınsama ölçüsü:** doküman 300 641 bayttan 304 891 bayta (+4,2 KB) büyüdü; artışın yaklaşık üçte biri başlıktaki sürüm notu ve dipnottur. Tablolara bir satır girdi (8.5.10).

## 6. Sonuç ve 2. tur

**Sonuç: TEMİZ DEĞİL** (ölçünün dört sınıfında) — tek parça çağrı iki, kontrol koşumu altı bulgu (biri tek parçayla aynı); yedi ayrı bulgunun yedisi uygulandı. Faz 5 (etki yansıtma) bu turda koşmaz; TEMİZ turunda koşar.

**2. tur** aynı istemle (§1'in alıntı bloğu, değiştirilmeden) ve iki koşumla yürür (K-953): (1) tek parça — istem, `=== DOKÜMAN ===`, güncel `05`, `=== ŞABLON ===`, şablon; (2) beş parça — aynı istem, parça notu (§1), başlık notu ve şablonla; parça sınırları bölüm başlıklarıdır (`## 0.` öncesi başlık notu ile `## 0.`–`## 3.` · `## 4.`–`## 5.` · `## 6.` · `## 7.` · `## 8.`–`## 12.`) — satır numaraları sürümle kayar, başlıklar kaymaz. Önceki turun raporu, karar kaydı ve audit raporları verilmez (K-431). Tur, altı çağrının altısı da `SONUÇ: TEMİZ` dönerse TEMİZ'dir; çıkış kuralları skill'in Faz 4 madde 5'indedir.
