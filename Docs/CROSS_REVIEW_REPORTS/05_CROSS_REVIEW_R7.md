# Cross-Review — 05 Technical Architecture (Tur 7)

**Tarih:** 2026-10-17 · **Hedef:** `Docs/05_TECHNICAL_ARCHITECTURE.md` v0.12 — güvence merceğinin düzeltmeleri uygulandıktan sonraki hâl (girdi: `a9afa09` üstündeki çalışma ağacı, 333 867 bayt, 1 490 satır; merceğin raporu [`AUDIT_REPORTS/05_GUARANTEE_LENS.md`](../AUDIT_REPORTS/05_GUARANTEE_LENS.md)) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren uygulanır (K-952) · **Koşum:** iki biçimde, altı çağrı — tek parça çağrı ve beş parçalı kontrol koşumu (K-953); turun sonucu ikisinin birleşimidir · **Şablon:** `58cf0b9`'daki v0.1 — Aşama 4 açılmadan önceki hâl, park blokları çıkarılmış, 87 satır · **Girdi:** yalnız doküman ve boş şablonu, stdin'den tek metin, boş ve izole bir çalışma klasöründe; karar kaydı, önceki turların raporları, merceğin raporu, Ürün Gereksinimleri, Kullanıcı Akışları, Arayüz Tanımları, MVP Kapsamı ve audit/deep review raporları verilmedi (K-431) · **Çıkış kuralı:** skill Faz 4 madde 5 — 5. turdan itibaren uygulanabilir

## 1. Koşum

**Hedefli mercekten sonra.** 6. turun önerisi (6. turun raporu §6) üzerine 7. turdan önce yedeğin korunmasının ve güncellemede göç ile yedekten dönüşün (§7.4.5–§7.6, §8.5–§8.7) güvence cümleleri betikle envantere çıkarılıp mekanizmaya karşı okundu: üç mercek, iki şüpheci, 23 ayrı konu, dört karar (K-962…K-965). Tur, merceğin düzeltmeleri uygulandıktan sonraki dokümanla koştu; merceğin raporu ikinci modele verilmedi.

**İstem ve şablon** 1. turunkidir, değiştirilmeden (`05_CROSS_REVIEW.md` §1'in alıntı blokları — istem ve parça notu; istem dosyasının SHA-256'sı `a146e8e2581d375a…`, şablonunki `fdf1b15e21d82277…`). Girdiler aynı betikle yeniden kuruldu: `istem + "=== DOKÜMAN ===" + güncel doküman + "=== ŞABLON ===" + şablon`; parçalı çağrılarda istemden sonra parça notu, parçada başlık notu. **Komut** 1. turunkidir: `codex exec -s read-only -C <boş klasör> --skip-git-repo-check --ephemeral --ignore-user-config --color never -m gpt-5.6-terra -c 'model_reasoning_effort="high"' -o <çıktı> - < <girdi>`. Altı çağrı paralel koştu; altısı da hatasız döndü (çıkış kodu 0). Yedek yol (`cursor-agent`) gerekmedi.

**Parça sınırları** 1. turunkidir — bölüm başlıkları (K-953); satır numaraları girdinin (v0.12, merceğin düzeltmelerinden sonra) satırlarıdır.

| Parça | Bölümler | Satırlar | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|---|
| — | Tek parça — dokümanın tamamı | 1–1490 | 341 854 bayt | 115 965 | 185 sn | 1 bulgu |
| 1 | Başlık notu · §0 · §1 · §2 · §3 | 1–444 | 109 026 bayt | 39 065 | 73 sn | `SONUÇ: TEMİZ` |
| 2 | §4 durum ve olaylar · §5 arka plan işleri | 445–620 | 84 276 bayt | 41 594 | 102 sn | 1 bulgu |
| 3 | §6 güvenlik (§6.x dahil) | 621–826 | 96 934 bayt | 82 478 | 86 sn | 2 bulgu |
| 4 | §7 veri katmanı | 827–961 | 92 102 bayt | 67 651 | 176 sn | 2 bulgu |
| 5 | §8 ortamlar · §9 gözlemlenebilirlik · §10 hata toleransı · §11 ölçeklenebilirlik · §12 özet | 962–1490 | 164 418 bayt | 56 092 | 49 sn | `SONUÇ: TEMİZ` |

**Web araması:** bu turda üç çağrı web araması yaptı — parça 2, 3 ve 4 (koşum kayıtlarında `web search` satırları). Parça 4'ün bulgusu bir mevzuat derleme sitesine bağlantı verir (`mevzuat.mturkoglu.av.tr` — resmî kaynak değil). Tek parça, parça 1 ve parça 5 aramadı.

**Girdiden sonraki değişiklik:** tur koştuktan sonra merceğin üç özet hücresi hizalandı — §0.5'in geçici disk ölçüsü (veritabanının ana sürüm geçişi), MK-25'in "durum ucunda" sözü ve MK-70'in karar ve gerekçe hücreleri; üçü merceğin kararlarının (K-965) yüzleridir ve turun bulgularıyla ilişkisizdir.

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**Tek parça çağrı:**

> ```text
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §7.6.1, §7.6.2
> Alıntı: "PostgreSQL'in arşiv zaman aşımı (`archive_timeout`) altmış saniyeye kurulur — açık günlük dosyası en geç bir dakikada kapatılır ve arşiv komutu onu taşır"
> Sorun: `archive_timeout`, WAL dosyasını en geç dakikada bir kapatır; nesne depolamaya yüklemenin bir dakika içinde tamamlandığını garanti etmez. Arşiv komutu kuyrukta bekleyebilir veya ağ yavaşlığı nedeniyle dakikaları bulabilir. Bu sırada aktarım “çalışıyor” görünse de, disk kaybında son bir dakikadan fazla onaylanmış sipariş kaybolabilir. Bu, §0.13 ve §7.6.2’deki sıfır kayıp güvencesinin tek istisnası olan bir dakikalık pencereyle çelişir.
> Öneri: Sipariş başarı yanıtı için sunucu dışındaki WAL/işlem kopyasının gerçekten ulaştığını en fazla bir dakika içinde doğrulayan bir mekanizma kurun; bu süre aşılırsa yeni sipariş yazımlarını güvenli biçimde durdurun veya bir dakikalık RPO’yu sağlayacak senkron, sunucu dışı dayanıklılık katmanı kullanın. Arşiv dosyasının yalnız kapatılmasını değil, dış depoda başarıyla doğrulanmış olmasını ölçün.
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
> Sorun: 6563 sayılı Kanun m.11/3, Kanun kapsamındaki iş ve işlemlere ilişkin elektronik kayıtların işlem tarihinden itibaren on yıl saklanmasını gerektirir. Ödemesiz kalan sipariş de sipariş oluşturma, iptal ve bildirim işlemlerini içerir. Alıcı ve olay değerlerinin üç yılda silinmesi, gönderim kaydını bu süre boyunca denetlenemez hâle getirerek saklama yükümlülüğüne aykırılık doğurur.
> Öneri: Ödemesi alınmamış siparişlere bağlı gönderim kayıtlarında da işlem ve bildirim kanıtını oluşturan alıcı ile olay değerlerini on yıl saklayın; kişisel veri minimizasyonu için farklı bir hukuki dayanak varsa bunu karar kaydında açıkça belgeleyin.
>
> SONUÇ: 1 BULGU
> ```

**Parça 3 — §6:**

> ```text
> BULGU-1
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §6.1.4
> Alıntı: "Yalnız Google'ın doğrulanmış verdiği e-posta kabul edilir ve ürün Google'dan yalnız adı ve e-postayı alıp saklar — Google hesabının kalıcı kimliği saklanmaz"
> Sorun: Google OIDC’de e-posta kalıcı ve tekil hesap kimliği değildir; değişebilir veya önceki sahibi Google hesabını kullanmaya devam edebilir. Kalıcı `sub` kimliğinin bilerek saklanmaması, yalnız doğrulanmış e-postayla mevcut Shopfolio hesabının eşlenmesine yol açar. Bu, başka bir Google hesabının müşteri hesabına ve kişisel verilerine erişmesi riskini doğurur (1: yetkisiz kişisel veri erişimi).
> Öneri: Google giriş bağını doğrulanmış `iss` + `sub` çiftiyle, hesapla ilişkili ayrı bir kimlik kaydı olarak saklayın. E-postayı yalnız iletişim verisi olarak kullanın; mevcut e-posta hesabına Google kimliği bağlama işlemi mevcut oturum/şifre doğrulaması veya açık hesap-bağlama onayı gerektirsin.
>
> BULGU-2
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §6.7.5, §6.7.8
> Alıntı: "Misafir sepeti çerezi ... Tarayıcıların izin verdiği en uzun ömür — 400 gün — ve her ziyarette yenilenir"
> Sorun: Doküman, alışveriş sepeti için kullanıcı girdili çerez örneğinden ayrılan, her ziyarette yenilenen 400 günlük kalıcı tanımlayıcıyı açık rıza olmadan sözleşmenin kurulması dayanağıyla kullanıyor. Ziyaretçinin sözleşmenin tarafı olmadığı ve kalıcılığı açıkça talep etmediği dönemlerde bu dayanak uygulanamaz; KVKK çerez rehberindeki kullanıcı girdili/sepet çerezi yaklaşımı oturumla sınırlıdır. Bu, hukuka aykırı kişisel veri işleme riskidir (1).
> Öneri: Misafir sepetini oturum çereziyle sınırlayın. Kalıcı sepet zorunluysa, üst dokümanlarda yeni karar alınarak açık ve ayrı kullanıcı tercihi/rıza mekanizması ile uygun saklama süresi tanımlayın.
>
> SONUÇ: 2 BULGU
> ```

**Parça 4 — §7:**

> ```text
> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §7.4.3 (4)
> Alıntı: “ödemesi hiç alınmamış siparişin (Z-41) satırında üç yıl dolunca alıcı ve olayın değerleri boşaltılır”
> Sorun: Ödeme alınmamış olsa da elektronik ortamda verilmiş sipariş ve ona ilişkin alındı teyidi, 6563 sayılı Kanun kapsamındaki işleme ait bilgi/belge/elektronik kayıttır. Bunların on yıl saklanması gerekir; üç yılda alıcı ve olay değerlerini silmek denetimde eksik ve gerçeğe uygun olmayan kayıt bırakır. Bu, kanuna aykırılıktır. [Kanun m.11/3](https://mevzuat.mturkoglu.av.tr/belge/6563/madde/11)
> Öneri: Ödeme sonucundan bağımsız olarak sipariş ve siparişe bağlı bildirim kayıtlarının denetim için gerekli özgün içeriğini on yıl saklayın; erişimi kısıtlama ve ayrı saklama ile veri minimizasyonunu sağlayın. K-951 ile üst dokümanlardaki süre kuralı da buna göre düzeltilmelidir.
>
> BULGU-2
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §7.6.1, §7.6.2, §7.6.9
> Alıntı: “PostgreSQL'in işlem günlüğü (WAL) en geç bir dakikada bir sunucunun dışındaki nesne depolamaya taşınır: PostgreSQL'in arşiv zaman aşımı (`archive_timeout`) altmış saniyeye kurulur”
> Sorun: `archive_timeout`, WAL segmentini en geç belirtilen sürede kapatmaya zorlar; nesne depolamaya yüklemenin o sürede tamamlanmasını garanti etmez. Arşiv komutu ağ ya da depolama gecikmesiyle dakikalarca geride kalabilir. Dokümanın on dakikaya kadar uyarı üretmeyen gerilik modeli de bunu doğrular. Bu yüzden aktarım “çalışıyor” görünürken kalıcı disk kaybında bir dakikadan fazla onaylı sipariş kaybolabilir; §7.6.2’nin en çok bir dakika kayıp güvencesi sağlanmamaktadır.
> Öneri: Bir dakika sınırı korunacaksa uzak depoda dayanıklı yazım onayı gecikmesini en fazla bir dakikayla sınırlayan ve sınır aşılınca sipariş onayını durduran/senkron uzak kopyaya geçen mekanizma ekleyin. Bu seçilmeyecekse, bir dakikalık azami kayıp iddiasını kaldırıp kabul edilen pencereyi gerçek uzak-arşiv gecikmesi olarak K kaydı ve üst ürün dokümanlarında düzeltin.
>
> SONUÇ: 2 BULGU
> ```

**Parça 5 — §8–§12:**

> ```text
> SONUÇ: TEMİZ
> ```

## 3. Bağımsız değerlendirme

Her bulgu dokümanın tamamına (v0.12), ilgili yerde Ürün Gereksinimleri'ne (`02` v0.73), karar kaydına, önceki turların raporlarına ve merceğin raporuna karşı okundu — ikinci model bunları görmedi, parçalı çağrılar dokümanın öteki bölümlerini de görmedi. Altı ham bulgu dört ayrı bulgudur: tek parça çağrının BULGU-1'i ile parça 4'ün BULGU-2'si aynı iddiadır; parça 2'nin bulgusu ile parça 4'ün BULGU-1'i aynı iddianın iki yeridir (§4.4.4 ve §7.4.3 (4)). **Şiddet** çıkış kuralının ölçüsüyle yazılır (5. turun raporunun §3'ü): yüksek — mevzuata aykırılık ya da para veya hak kaybı; orta — çıkışsız akış ya da dokümanın koyduğu bir güvencenin mekanizmasının eksik olması; düşük — iki yerin metin farkı ya da metin eksikliği.

| # | Karar | Şiddet · sınıf | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|---|
| Tek · BULGU-1 = P4 · BULGU-2 — **dönen** (5. turda KISMİ, 6. turda RET) | ⚠️ KISMİ | **Orta** — dokümanın koyduğu bir güvencenin mekanizması onu tam sağlamıyordu (sıfır kaybın penceresi; etkisi olağan bağlantıda saniyeler) | **Cümle mekanizmayla taşınma süresi kadar tutmuyordu; önerinin kalanı elenmiş seçeneklerdir.** 7.6.1 "işlem günlüğü en geç bir dakikada bir … nesne depolamaya taşınır" diyor ve mekanizma olarak altmış saniyelik arşiv zaman aşımını veriyordu. Zaman aşımı açık dosyayı belirtilen sürede kapatır; taşınmanın o sürede bitmesini garanti etmez (PostgreSQL 18 belgesi, "archive_timeout" — okundu 2026-10-17). Altmış saniyeyle bir kayıt, olağan bağlantıda bile, bir dakika artı kapanan dosyanın taşınma süresi kadar yedeğe ulaşmamış kalabilir. 6. turdaki RET'in gerekçesi — taşınma saniyelerdir, tasarım değerinin içindedir — farkın küçüklüğünü söyler, cümlenin doğru olduğunu söylemez; iddia üç turdur ve bu turda iki koşumda bağımsız olarak döndü. Hedefli mercek bu cümleyi önceki ret yüzünden bulgu yapmamıştı. **Mekanizma cümleye uyduruldu:** zaman aşımı elli saniyeye indi; dakikanın kalan on saniyesi kapanan dosyanın taşınmasınındır — olağan bağlantıda saniyelerdir — ve taşınması daha uzun süren dosya aktarımı geride bırakır; geride kalan aktarım zaten durmuş sayılır ve pencere aktarımın durduğu ana kadar uzar (7.6.9, K-939'un kalan riski). Böylece "en çok son bir dakika" (§0.13, `02 §3.34.8`) mekanizmasıyla doğrudur ve üst dokümanların metni değişmez. Bedeli kesintide biriken günlüğün günde yaklaşık 23 GB'tan 28 GB'a çıkmasıdır. **Modelin önerileri elendi:** "senkron, sunucu dışı dayanıklılık katmanı" ikinci eşzamanlı kopyadır — proje sahibi pencereyi bu seçeneğe karşı kabul etti (K-888); "sınır aşılınca sipariş yazımını durdur" K-939'un elediği "aktarım geride kalınca satışın kendiliğinden kapanması"dır; "pencereyi gerçek gecikmeye göre üst dokümanlarda düzelt" mekanizma onu sağladığı için gerekmez. Sıfır kaybın penceresinin mekanizmasıdır — iki yerin metin farkı değildir; şiddet orta. | 7.6.1: zaman aşımı elli saniye, kalan on saniye taşınmanın, daha uzun taşınma geride kalan aktarım · 7.6.9: "her elli saniyede", günde yaklaşık 28 GB, geride kalmanın tanımına taşınmanın on saniyesi · 10.1.10: 28 GB · §7.6'nın Kaynak satırı · §12 MK-25'in gerekçesi · `DEPLOY_RUNBOOK`'un yedek park satırı (etiket ve metin). **K-966** (öneriyle kaydedildi); K-878, K-888 ve K-939'a geri işaret. |
| P3 · BULGU-2 — **dönen** (3. turda KISMİ) | ❌ RET | — | **Bilinçli karardır; dayanağı ve elenen iki seçeneği dokümanda yazılıdır.** 6.7.8 misafir sepetinin kalıcı çerezini "kalıcılığın dayanağı bilinçli karardır" diye yazar: çerez KVKK'nın Çerez Uygulamaları Hakkında Rehberi'nin (Temmuz 2025) oturumla biten "kullanıcı girdili çerez" örneğinden ayrılır ve dayanağı satış sözleşmesinin kurulmasıdır (KVKK m.5/2-c); "sepet çerezi oturum çerezine çevrilsin" (Z-6'nın kalıcı sepetini değiştirir) ve "sepet için rıza istensin" (çerez bandını sepete bağlar) elendi (K-950; `02 §12.2.5`). Modelin iki önerisi bu iki elenen seçenektir. İstem bilinçli kararı, kendi içinde çelişki ya da olgusal veya hukuki hata doğurmadıkça bulgu saymaz; modelin iddiası bir hukuki yorumdur ve K-950 aynı yorumu tartmıştır — karar ⚠'dır, proje sahibinin gözden geçirmesine gider ve avukat teyidi bu dönemde istenmez (K-950'nin gerekçesi, K-721'in kalıbı). 3. turda aynı iddia KISMİ okunmuş, eksik olan dayanak 6.7.8'e yazılmıştı; bu tur yeni bir olgu getirmiyor. | — |
| P2 · BULGU-1 = P4 · BULGU-1 — **tekrar eden RET** | ❌ RET (tekrar eden) | — | Ödemesi alınmamış siparişte kişisel verinin üç yılda silinmesi proje sahibinin kararıdır (K-951 — 6563 m.11/3 tartılarak; "ödemesi alınmamış siparişte de kişisel veri on yıl" seçeneği elendi; gerekçesi §7.4.3 (4)'te yazılı). Önceki turlarda aynı yer ve iddiayla en az iki kez gerekçeyle reddedildi (6. turun raporu §6'nın listesi). Turun sonucunu belirlemez. | — |
| P3 · BULGU-1 — **tekrar eden RET** | ❌ RET (tekrar eden) | — | Google hesabının kalıcı kimliğinin saklanmaması bilinçli karardır (K-900; §6.1.4); önceki turlarda aynı yer ve iddiayla en az iki kez gerekçeyle reddedildi (6. turun raporu §6'nın listesi). Turun sonucunu belirlemez. | — |

**Dağılım:** dört ayrı bulgu — 1 KISMİ · 1 RET · 2 tekrar eden RET. **Şiddet:** bir orta (Tek · BULGU-1 = P4 · BULGU-2). **%100 ret değil:** kabul edilen tek bulguda modelin önerisi elendi ve mekanizma güvencenin cümlesine uyduruldu; RET somut yerlere dayanır (6.7.8, K-950, `02 §12.2.5`). **Bir bulgu yeni karar doğurdu** (K-966, ⚠ dışı).

## 4. Ek bulgular

- **Aynı kalıbın taraması (Tek · BULGU-1 = P4 · BULGU-2):** "altmış saniye", "arşiv zaman aşımı", "her dakika kapat", "en geç dakikada bir" ve "23 GB" `05`'te ve sonraki dokümanlarda tarandı. Değeri taşıyan yerler 7.6.1, 7.6.9 ve 10.1.10'dur — üçü hizalandı. "En geç bir dakika" diyen yüzler — §0.13, 1.2.22, 7.6.2, 10.1.9, MK-25, `DEPLOY_RUNBOOK`'un yedek satırı ve üst dokümanlar (`02 §3.34.8`, §12.3.3, `10 §1`, KP-75, `03` 10.2.4, `04 §1.1`) — artık mekanizmasıyla doğrudur ve değişmedi; runbook satırı değeri anar. Kodlama Kılavuzu, Veri Modeli ve Doğrulama Protokolü zaman aşımını anmaz.
- **Aynı kalıbın taraması (P3 · BULGU-2):** "400 gün" ve "misafir sepeti" 6.7.5, 6.7.8, 6.7.11 ve §5.2.9'da geçer; dayanak tek yerde, 6.7.8'dedir (3. turun taraması) — değişiklik yok.
- **Kendi okumam — merceğin düzeltmeleri:** ikinci modelin bu turu merceğin değiştirdiği bölgelerde (§7.4.5, §7.6, §8.5–§8.8, §9.2–§9.4) yeni bir orta bulgu getirmedi; §8–§12'yi taşıyan parça 5 ilk kez TEMİZ döndü. Merceğin kararları yeni bir ürün kuralı eklemez — `02 §3.34.8` değişmeden kalır, `05` ona hizalandı.
- **Mekanik tarama:** Kaynak satırı betiği değişiklikten önce ve sonra aynı sonucu verir; tek fark §12'nin gerekçe hücresine giren K-962…K-966'dır (§12'nin Kaynak satırı "K numaraları gerekçe hücresindedir" der). Park satırlarının etiketleri (`09` dört satır, `DEPLOY_RUNBOOK` iki satır) ile K-962…K-966'nın etki sütunları iki yönde eşleşir. Başlık sürümü, kalite döngüsü notları ve dosya sonu dipnotu v0.12'yi gösterir. Karar kaydında çift numara yok; K-855'ten bu yana `(öneriyle kaydedildi — ⚠)` taşıyan satırlar betikle sayıldı — 36 — ve §12.1'in listesiyle aynı. Değişen dosyalarda CR yok; diff'in eklenen satırlarında `$[0-9]` artığı yok.
- **Önceki turlardan açık kalanlar:** yok — 6. turun önerisi (hedefli mercek) bu adımda koştu ve kapandı.

## 5. Kullanıcı onay checklist'i

K-855 kararıyla (Aşama 4 boyunca ⚠ öneriyle kayıt, CI yeşilse `docs:` PR merge yetkisi ve yönetici + alt ajan düzeni) alt ajan değerlendirdi ve uyguladı (`05` v0.12). Bu turun kararı öneriyle kaydedildi ve ⚠ dışıdır: **K-966** — arşiv zaman aşımı elli saniye, dakikanın kalan on saniyesi kapanan dosyanın taşınmasının; daha uzun taşınma geride kalan aktarımdır. Merceğin dört kararı ve iki ⚠'ı (K-962, K-964) merceğin raporundadır; ⚠ listesi 36. Geri besleme bu turda yok (merceğin geri beslemesi: Ürün Gereksinimleri v0.74, MVP Kapsamı v0.49). Park satırı: `DEPLOY_RUNBOOK` (Yedek, kurtarma ve prova — K-966). Geri işaret: K-878, K-888, K-939.

- [x] Tek · BULGU-1 = P4 · BULGU-2 (kısmi, orta, dönen — K-966: zaman aşımı elli saniye, taşınmanın on saniyesi)
- [x] P3 · BULGU-2 (ret, dönen — bilinçli karar, K-950)
- [x] P2 · BULGU-1 = P4 · BULGU-1 (tekrar eden ret — K-951)
- [x] P3 · BULGU-1 (tekrar eden ret — K-900)
- [ ] Etki yansıtma (Faz 5) — döngü TEMİZ döndükten sonra, son turun raporunda

**Hukuki kontrol** (2026-10-17): iki bulgu yasal dayanak taşır. 6563 sayılı Kanun m.11/3 iddiası (P2, P4) proje sahibinin kararıdır (K-951); karar maddenin 7416 sayılı Kanunla eklenen son cümlesi okunarak verildi ve bu turun kaynağı resmî metin değil, bir derleme sitesidir — yeni bilgi yok. KVKK'nın çerez rehberi iddiası (P3 · BULGU-2) K-950'nin tarttığı yorumdur — rehber (Temmuz 2025) §5.1'in sepet örneği kararın gerekçesinde anılır; avukat teyidi bu dönemde istenmez. Düzeltme yeni hukuki içerik eklemez.

**Yakınsama ölçüsü:** ayrı bulgu 7 → 5 → 6 → 6 → 5 → 5 → 4; kabul edilen ya da kısmen kabul edilen 7 → 3 → 3 → 3 → 4 → 3 → 1; turun yeni kararı 2 → 0 → 1 → 2 → 2 → 1 → 1 (bu turun kararı ⚠ dışı); dönen iddia 0 → 1 → 3 → 2 → 1 → 2 → 2; tekrar eden RET 0 → 0 → 0 → 2 → 1 → 0 → 2. TEMİZ çağrı 0 → 1 → 1 → 1 → 2 → 2 → 2. Doküman 320 460 bayttan (v0.11) mercekle 333 867 bayta, bu turla 335 KB'a büyüdü; tablolara bir satır girdi (8.5.11).

**Orta ve yüksek şiddetli yeni bulguların yeri:** 3.–6. turların beşi (§8.7.4 iki kez, §4.2.2, §8.7.1, §7.6.6); 7. turda tek orta kabul §7.6.1'dedir ve yeni değildir — 5. turdan beri dönen bir iddianın mekanizmayla kapanmasıdır. Merceğin düzelttiği bölgelerde (§7.4.5–§7.6'nın öteki maddeleri, §8.5–§8.8) ikinci model yeni bir bulgu getirmedi.

## 6. Sonuç ve 8. tur

**Sonuç: TEMİZ DEĞİL** (ölçünün dört sınıfında) — altı çağrının ikisi TEMİZ; dört ayrı bulgunun biri kısmen uygulandı, biri reddedildi, ikisi tekrar eden RET. Faz 5 (etki yansıtma) bu turda koşmadı; TEMİZ turunda koşar.

**Çıkış kuralı bu turda uygulanmadı.** Skill'in Faz 4 madde 5'inin ikinci kuralı kabul edilen bulguların **hepsinin** düşük şiddette iki yerin metin farkı olmasını ister — mevzuata aykırılık, para ya da hak kaybı, çıkışsız akış değilse. Bu turun tek kabulü sıfır kaybın penceresinin mekanizmasıdır: düzeltme bir tasarım değerini değiştirdi ve iki yerin metnini hizalamadı; iddia sipariş kaybına dayanır. Etkisi saniyeler olsa da kuralın harfi sağlanmaz. Kural bu durumda döngüyü sürdürür; çıkış kuralını uygulamayan tur karar satırı gerektirmez (K-849 kalıbı yalnız kural uygulandığında kayda girer).

**8. tur** aynı istemle (1. turun raporunun §1'i, değiştirilmeden), aynı şablonla ve iki koşumla (K-953) yürür; parça sınırları bölüm başlıklarıdır. Önceki turların raporları, karar kaydı, audit raporları ve merceğin raporu verilmez (K-431). Tur, altı çağrının altısı da `SONUÇ: TEMİZ` dönerse — ya da yalnız tekrar eden RET'ler kalırsa — TEMİZ'dir. **8. turda tekrar eden RET sayılacaklar** (aynı yer ve iddiayla dönerlerse): Google hesabının kalıcı kimliği (§6.1.4) · ödemesi alınmamış siparişte kişisel verinin üç yılı (§7.4.3 (4), §4.4.4) · yedeğe ulaşmamış penceredeki silmeler ya da ayrı bir silme deposu (§7.4.5; K-888, K-939, K-955). **Dönen iddialar** — misafir sepetinin kalıcı çerezi (§6.7.8; 3. turda KISMİ, bu tur RET) ve sistem kayıtlarının yedekteki süresi (§9.2.3; 4. turda RET, 6. turda KISMİ) — henüz iki kez gerekçeyle reddedilmedi; dönerlerse kendi gerekçeleriyle okunur. Arşiv zaman aşımının bir dakikası bu turda mekanizmayla kapandı; aynı iddia elli saniyelik ayara karşı dönerse yeni iddia gibi okunur. **8. turda çıkış kuralı uygulanabilir:** kabul edilen bulguların hepsi düşük şiddette iki yerin metin farkıysa bulgular uygulanır, aynı kalıp dokümanın tamamında taranır, döngü "TEMİZ — çıkış kuralıyla, 8 tur" diye kapanır, kuralın uygulanışı bir karar satırıyla kaydedilir (K-849 kalıbı) ve sınıfın kalan riski karar kaydının §12.1'inde aşamanın checkpoint'ine devredilir; kabullerden biri orta ya da yüksekse döngü sürer.

**Yakınsama gözlemi ve öneri:** hedefli mercek ikinci modelin her turda bir tanesini bulduğu sınıfı — güvence cümlesi ile mekanizmanın farkını — iki bölgede bir kerede kapattı ve bu turun ikinci modeli o bölgelerde yeni bir bulgu getirmedi; parça 5 (§8–§12) ilk kez TEMİZ döndü, kabul sayısı 3'ten 1'e indi ve tek kabul üç turdur dönen bir iddiaydı. Yeni bulgu kalmadı denebilir; kalan dört bulgunun üçü kararla kapalı iddialardır. **Öneri:** 8. tur ek adımsız koşulur. 8. turda yalnız tekrar eden ya da kararla kapalı iddialar dönerse tur TEMİZ'dir; düşük şiddette metin farkları dönerse çıkış kuralı uygulanır. 8. tur yine güvence ile mekanizma farkı sınıfında orta bir bulgu getirirse — mercek bu sınıfı yalnız iki bölgede aradı — aynı merceğin dokümanın kalan bölgelerinde (§4, §5, §6) koşması önerilir; o bölgelerin güvence cümleleri (ör. "tek işlenir", "kaybolmaz", "çift gönderilmez") bu adımda envantere girmedi.
