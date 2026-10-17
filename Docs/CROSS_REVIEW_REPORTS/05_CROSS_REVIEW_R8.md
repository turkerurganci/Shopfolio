# Cross-Review — 05 Technical Architecture (Tur 8)

**Tarih:** 2026-10-17 · **Hedef:** `Docs/05_TECHNICAL_ARCHITECTURE.md` v0.12 (girdi commit'i `eefa262`, 335 160 bayt, 1 490 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren uygulanır (K-952) · **Koşum:** iki biçimde, altı çağrı — tek parça çağrı ve beş parçalı kontrol koşumu (K-953); turun sonucu ikisinin birleşimidir · **Şablon:** `58cf0b9`'daki v0.1 — Aşama 4 açılmadan önceki hâl, park blokları çıkarılmış, 87 satır · **Girdi:** yalnız doküman ve boş şablonu, stdin'den tek metin, boş ve izole bir çalışma klasöründe; karar kaydı, önceki turların raporları, güvence merceğinin raporu, Ürün Gereksinimleri, Kullanıcı Akışları, Arayüz Tanımları, MVP Kapsamı ve audit/deep review raporları verilmedi (K-431) · **Çıkış kuralı:** skill Faz 4 madde 5 — 5. turdan itibaren uygulanabilir

## 1. Koşum

**İstem ve şablon** 1. turunkidir, değiştirilmeden (`05_CROSS_REVIEW.md` §1'in alıntı blokları — istem ve parça notu; istem dosyasının SHA-256'sı `a146e8e2581d375a…`, şablonunki `fdf1b15e21d82277…` — 7. turunkilerle aynı). Girdiler aynı betikle yeniden kuruldu: `istem + "=== DOKÜMAN ===" + güncel doküman + "=== ŞABLON ===" + şablon`; parçalı çağrılarda istemden sonra parça notu, parçada başlık notu. **Komut** 1. turunkidir: `codex exec -s read-only -C <boş klasör> --skip-git-repo-check --ephemeral --ignore-user-config --color never -m gpt-5.6-terra -c 'model_reasoning_effort="high"' -o <çıktı> - < <girdi>`. Altı çağrı paralel koştu; altısı da hatasız döndü (çıkış kodu 0). Yedek yol (`cursor-agent`) gerekmedi.

**Parça sınırları** 1. turunkidir — bölüm başlıkları (K-953); satır numaraları girdinin (v0.12) satırlarıdır.

| Parça | Bölümler | Satırlar | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|---|
| — | Tek parça — dokümanın tamamı | 1–1490 | 343 147 bayt | 160 232 | 134 sn | 1 bulgu |
| 1 | Başlık notu · §0 · §1 · §2 · §3 | 1–445 | 109 706 bayt | 38 022 | 31 sn | `SONUÇ: TEMİZ` |
| 2 | §4 durum ve olaylar · §5 arka plan işleri | 446–621 | 84 878 bayt | 33 971 | 142 sn | 2 bulgu |
| 3 | §6 güvenlik (§6.x dahil) | 622–827 | 97 536 bayt | 53 649 | 140 sn | 2 bulgu |
| 4 | §7 veri katmanı | 828–962 | 93 025 bayt | 40 398 | 219 sn | 1 bulgu |
| 5 | §8 ortamlar · §9 gözlemlenebilirlik · §10 hata toleransı · §11 ölçeklenebilirlik · §12 özet | 963–1490 | 165 312 bayt | 58 745 | 114 sn | `SONUÇ: TEMİZ` |

**Web araması:** iki çağrı web araması yaptı — tek parça ve parça 3 (koşum kayıtlarında `web search` satırları; KVKK'nın sitesinde çerez rehberi ve kişisel verinin tanımı, Google'ın geliştirici belgesinde `sub`). Tek parçanın bulgusu KVKK'nın sitesine bağlantı verir. Parça 1, 2, 4 ve 5 aramadı.

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**Tek parça çağrı:**

> ```text
> BULGU-1
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §6.4.4, §6.4.9
> Alıntı: “kayıt hesabıyla yaşar ve hesapla silinir — gecelik temizlik onu silmez”
> Sorun: Bağlantı kaydı; hesap kimliği, kullanım/değiştirilme zamanları ve anahtar özetini taşıdığı için “kişisel veri taşımaz” değildir. Firma bu kaydı hesapla eşleyerek kişiyi belirleyebilir. Geçersiz, kullanılmış ve değiştirilmiş bağlantıların hesap silinene kadar tutulması için süre veya zorunlu amaç tanımlanmamıştır. Bu, KVKK’daki amaçla sınırlılık ve gerekli süre kadar saklama ilkelerine aykırı sürekli kişisel veri saklama riski doğurur. [KVKK’nın kişisel veri ve saklama ilkesi açıklaması](https://www.kvkk.gov.tr/Icerik/7192/2022-137).
> Öneri: Bağlantı kaydını kişisel veri olarak envantere alın. Geçerliliği biten/kullanılan/değiştirilen kayıtlar için `02`de hukuki amaç ve üst saklama süresi tanımlayın; süre sonunda kayıtları silin veya hesap bağını ve anahtar özetini geri döndürülemez biçimde kaldırın. Geçmiş bağlantı ekranı hâlleri gerekiyorsa bu süreyle sınırlı tutulmalıdır.
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
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §4.2.2, §4.5.1
> Alıntı: "ödeme adımı her gösterildiğinde forma tek kullanımlık bir onay anahtarı konur"
> Sorun: Her ödeme adımı gösteriminde yeni anahtar üretildiği hâlde, aynı sepete ait eşzamanlı iki farklı anahtarlı onayı serileştiren sepet kilidi ya da tek açık/ödenmemiş siparişi zorlayan veritabanı kısıtı tanımlanmamıştır. İki istek, önceki ödenmemiş siparişi görmeden başlayabilir; ikisi de stok ayırıp ayrı sipariş ve kart yönlendirmesi oluşturabilir. Bu, aynı müşterinin iki kez ödeme yapmasına yol açabilir (2).
> Öneri: Sipariş onayını sepet düzeyinde kilitleyin veya aynı sepet için tek ödenmemiş siparişi zorlayan veritabanı kısıtı ekleyin; çakışmada ikinci isteği mevcut siparişe yönlendirin.
>
> BULGU-2
> Kriter: Edge case
> Seviye: Yüksek
> Yer: §5.4.1, §5.4.3
> Alıntı: "İki nabız arasındaki boşluk iki dakikayı aşarsa aralık — son nabızdan yeni nabza — bir kesinti kaydı olur."
> Sorun: İki dakikadan kısa gerçek bir site kesintisinde havale ödeme süresi dolarsa kesinti kaydı oluşmaz. Bu nedenle §5.4.3’teki ilk iş günü sonuna erteleme uygulanmadan zamanlayıcı siparişi iptal eder. Park edilen kural, kesinti sırasında dolan havale süresinin ertelenmesini koşulsuz tanımlar; kısa kesinti istisnası yazılmamıştır. Müşteri ödeme hakkını ve firma satışı kaybedebilir (2).
> Öneri: Kesintiyi, normal nabız aralığını aşan her doğrulanmış boşluk için kaydedin; en azından süresi bu boşlukta dolan havale siparişlerinde ertelemeyi tetikleyecek bir kayıt mekanizması kurun.
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
> Sorun: Google, e-posta bilgisinin değişebileceğini ve benzersiz, kalıcı kullanıcı kimliği olarak kullanılmaması gerektiğini; bunun yerine yeniden kullanılmayan `sub` değerinin kullanılmasını belirtir. E-posta ile hesap eşlemek, e-posta değişimi veya yeniden atamasında başka bir Google hesabının mevcut müşteri hesabına erişmesine yol açabilir. Bu, kişisel verilere ve sipariş geçmişine yetkisiz erişimdir (1).
> Öneri: Google hesabını kalıcı `sub` değeriyle eşleştirip saklayın; e-postayı yalnız iletişim bilgisi olarak ele alın. Mevcut e-posta-temelli eşlemeler için güvenli geçiş ve hesap birleştirme/uyuşmazlık davranışını `08`e bırakacak şekilde mimari sınırı yazın.
>
> BULGU-2
> Kriter: Teknik doğruluk
> Seviye: Orta
> Yer: §6.1.5
> Alıntı: "Yeniden doğrulama şifreyle ya da Google'dan taze bir girişle — girişle aynı akış (6.1.4) — yapılır"
> Sorun: §6.1.4’teki standart yetki kodu akışı, kullanıcının mevcut Google oturumunu sessizce kullanabilir; tek başına “taze giriş” sağlamaz. Böylece yeniden doğrulama gerektiren işlemler, Google’da hâlihazırda açık oturumla yeniden parola/kimlik kanıtı olmadan tamamlanabilir. Dokümanın koyduğu yeniden doğrulama güvencesi sağlanmaz (4).
> Öneri: Yeniden doğrulama isteğinde Google’ın taze kimlik doğrulama zorlamasını (`prompt=login` veya doğrulanmış `max_age`) kullanın; dönen kimlik belirtecindeki kimlik doğrulama zamanını doğrulayın.
>
> SONUÇ: 2 BULGU
> ```

**Parça 4 — §7:**

> ```text
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §7.4.5
> Alıntı: “Satır silme işlemi diske yazıldıktan sonra yazılır ve diske inmesi beklenir: geri alınan silme satır bırakmaz, kesinleşen silmenin satırı tamponda kalmaz (K-962).”
> Sorun: Silme işlemi kesinleştikten sonra, uygulama kaydı yazılmadan süreç ya da sunucu çökerse silme kalıcılaşır fakat geri uygulama kaydı oluşmaz. Daha sonra eski bir ana dönüldüğünde silinen kişisel veri yeniden gelir; komut bu silmeyi bilemeyeceği için tekrar uygulayamaz. Bu, “Eski bir ana dönüşte silmelerin geri gelmesi böylece kalan risk değildir” güvencesiyle çelişir ve kişisel verinin yeniden erişilebilir olmasına yol açar (4; ayrıca hak kaybı riski).
> Öneri: Silmeyi yazma-öncelikli ve dayanıklı bir protokole bağlayın: silme niyeti/kimliği önce fsync edilen, geri dönüşten etkilenmeyen günlüğe yazılsın; bu kayıt başarısızsa veritabanı silmesi kesinleşmesin. Günlük kaydı oluştuğunda işlem idempotent biçimde tamamlanacak kabul edilsin; dönüş komutu bu kayıtları tekrar oynatsın.
>
> SONUÇ: 1 BULGU
> ```

**Parça 5 — §8–§12:**

> ```text
> SONUÇ: TEMİZ
> ```

## 3. Bağımsız değerlendirme

Her bulgu dokümanın tamamına (v0.12), ilgili yerde Ürün Gereksinimleri'ne (`02` v0.74), Kullanıcı Akışları'na (`03` v0.26), Arayüz Tanımları'na (`04` v0.24), karar kaydına ve önceki turların raporlarına karşı okundu — ikinci model bunları görmedi, parçalı çağrılar dokümanın öteki bölümlerini de görmedi. Altı ham bulgu altı ayrı bulgudur; hiçbiri iki koşumda birden gelmedi. **Şiddet** 5. turun raporunun §3'ündeki ölçüyle yazılır — yüksek: mevzuata aykırılık ya da para veya hak kaybı; orta: çıkışsız akış ya da dokümanın koyduğu bir güvencenin mekanizmasının eksik olması; düşük: iki yerin metin farkı ya da metin eksikliği. Bu turda ölçünün sınırı bir kez daha yazıldı: milisaniyeler ya da saniyeler düzeyinde, yalnız nadir bir arızada ya da yarışta açılan ve sonuçlarının çıkışı olan bir boşluk, mekanizmayı ilgilendirse de orta sayılmaz — gerekçesi satırında.

| # | Karar | Şiddet · sınıf | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|---|
| P3 · BULGU-2 — yeni | ⚠️ KISMİ | **Orta** — dokümanın koyduğu bir güvencenin mekanizması eksikti (yeniden doğrulama; sonuç kişisel veriye ve hesaba yetkisiz erişim) | **Sorun gerçek.** 6.1.5 Google ile yeniden doğrulamayı "Google'dan taze bir girişle — girişle aynı akış" diye yazar; girişin akışı Google'da açık oturum varsa kullanıcıya şifre sormadan döner. Açık tarayıcıya erişen biri — yeniden doğrulamanın karşıladığı tehdit tam budur — Google girişli hesapta e-postayı değiştirebilir ya da hesabı silebilirdi; `02 §8.1.5`'in yeniden doğrulaması bu hesapta sağlanmıyordu. Durum nadir değildir: Google'da oturumun açık olması olağan hâldir. **Önerinin yarısı elendi:** Google'ın belgesi `prompt` için yalnız `none`, `consent` ve `select_account`'u sayar — `login` belgede yoktur — ve `max_age`'i anmaz; `auth_time`'ı istekle ve uygulamanın ayarıyla açılan bir alan olarak verir (Google, "OpenID Connect" geliştirici belgesi — okundu 2026-10-17). Kanıt doğrulama anıdır: yeniden doğrulama yalnız başlatıldığı andan sonraki bir Google girişini kabul eder; Google `max_age`'i destekliyorsa istek onu da taşır (biçimi `08`'in). Doğrulama anı eskiyse ekran sebebini söyler ve Google'da yeniden giriş ile şifre belirlemenin yolunu gösterir — Arayüz Tanımları'nın E-24'ünde bu hâl yoktu (Google'ın verdiği e-postanın eşleşmediği, 6.1.5'te zaten yazılı hâl de yoktu); geri beslendi. **"Açık oturumu yeniden doğrulama sayıp kalan riski yazmak" elendi:** K-900'ün "taze giriş"inden ayrılır ve `02 §8.1.5`'i boşaltır. ⚠: kişisel veriye ve hesabın güvenliğine dokunur, Google girişli hesapta üç işlemin yolunu değiştirir; belirgin öneri vardır (var olan kararın sözü), çok kritik değildir. | 6.1.5: doğrulama anının denetimi, tutmayan doğrulamanın çıkışı, `max_age` · 6.1.6: iki elenen · §6.1'in Kaynak satırı · §12 MK-46 · Arayüz Tanımları v0.25 — 5.24.7, 6.2.24.5, §5.24'ün ve §6.2'nin Kaynak satırları, başlık notu, dipnot · 05'in geri besleme tablosu · Entegrasyon Spesifikasyonu'nun park satırı (K-900 · K-942 · K-968). **K-968** (öneriyle kaydedildi — ⚠); K-900'e geri işaret. |
| P2 · BULGU-1 — yeni | ⚠️ KISMİ | **Düşük** — mekanizmanın eşzamanlı yarışta açığı; ölçünün "orta"sına harfiyen girer, ama pencere milisaniyelerdir ve sonuçların çıkışı vardır | **Açık gerçek.** 4.2.2'nin adımları ardışık onayda tutar: sonraki onay, öncekinin siparişini (3)'te ödenmemiş önceki sipariş olarak bulup iptal eder (`02 §3.17.7`). İki onay aynı anda başlarsa ikisi de önceki siparişi görmeden — ya da aynı önceki siparişi — işler ve iki ödenmemiş sipariş açılır. **Neden orta değil:** pencere tek bir veritabanı işleminin süresidir; aynı müşterinin iki ayrı onay formunu — iki sekmeden, ayrı onay anahtarlarıyla — aynı anda göndermesini ister; çift tıklama, geri tuşu ve yeniden gönderim zaten onay anahtarıyla tek siparişe iner (MK-39). Sonuçların çıkışı vardır: ödenmemiş ikinci sipariş ödeme süresi dolunca kendiliğinden iptal olur; ikisi de ödenirse müşterinin iptali ve geri ödemesi açıktır. Düzeltme tek bir kilittir. **Önerinin iki kolu elendi:** "sepet başına tek ödenmemiş sipariş kısıtı" ikinci onayı hatayla düşürür, `02 §3.17.7` öncekini iptal etmesini ister; "ikinci isteği var olan siparişe yönlendirmek" yeni onayın önceki siparişin yerine geçmesine aykırıdır. **Sepetin satır kilidi de elendi:** ödeme sonucunun sepetten kalem çıkarması sipariş → sepet sırasıyla kilitler ve onayla ters sırada bekleşirdi; veritabanının danışma kilidi satır kilitlerinin sırasına girmez. | 4.2.2: onay önce sepetin danışma kilidini alır; kilit sırası cümlesi · 7.3.5: danışma kilidini yalnız onay alır, sırayı bozmaz · §4.2 ve §7.3'ün Kaynak satırları · §12 MK-36 · Doğrulama Protokolü'nün park satırı — senaryo (7). **K-967** (öneriyle kaydedildi); K-957 ve K-890'a geri işaret. |
| P4 · BULGU-1 — yeni (yer §7.4.5, iddia tekrar eden RET'ten ayrı) | ⚠️ KISMİ | **Düşük** — güvence cümlesinin aşırı kesinliği; pencere bir anlık, yalnız iki nadir olayın birlikte gerçekleşmesinde açılır | **Cümle tam tutmuyordu.** 7.4.5 anlık silmenin uygulama kaydındaki satırını işlem kesinleştikten sonra yazar ve diske inmesini bekler (K-962) ve "eski bir ana dönüşte silmelerin geri gelmesi böylece kalan risk değildir" der. Silmenin kesinleşmesi ile satırın diske inmesi arasındaki anda süreç ya da sunucu çökerse silme kalıcıdır ama satırı yoktur; sonradan veri bozulmasından eski bir ana dönülürse komut o silmeyi göremez. **Neden düşük:** pencere işlemin kesinleşmesi ile tek bir dosya yazımının diske inmesi arasındaki andır; o anda bir çökme ve ardından eski bir ana dönüş — kendisi yalnız veri bozulmasından kurtarmada ve sipariş tarafında yazım yoksa yapılır (K-962) — birlikte gerekir. Mekanizma değişmez, cümle kalan riskiyle yazılır. **Öneri elendi:** "silme niyetini önce dayanıklı günlüğe yaz, komutla oynat" — işlemi geri alınan silme de satır bırakırdı ve komut kullanıcının tamamlanmamış silmesini uygulardı; K-962'nin sırası tam bunun için seçildi; "niyet, sonra sonuç" diye iki satır yazmak çökmede aynı belirsizliği "niyet var, sonuç yok" satırına taşır. **Tekrar eden RET değildir:** aynı yerdeki tekrar eden iddia yedeğe ulaşmamış penceredeki silmeler ve ayrı silme deposudur (K-888, K-939, K-955); bu iddia diskin kaybını değil yerel çökmeyi anlatır. ⚠: kişisel verinin silinmesine dair kabul edilmiş risk (K-955 ve K-962 ⚠'dı). | 7.4.5: "tek istisna" ve elenen · §7.4'ün Kaynak satırı · `DEPLOY_RUNBOOK`'un yedek park satırı (etiket ve metin). **K-969** (öneriyle kaydedildi — ⚠); K-955 ve K-962'ye geri işaret. |
| P2 · BULGU-2 — yeni | ⚠️ KISMİ | **Düşük** — iki yerin metin farkı: `02 §4.3`'ün koşulsuz "kesinti sırasında" cümlesi ile `05`'in eşiği; kayıp ödeme süresinin sonundaki en çok iki dakikadır | **Fark gerçek, mekanizma bilinçli.** 5.4.1 iki dakikayı aşan nabız boşluğunu kesinti sayar (K-898'in tasarım değeri); `02 §4.3` havale iptalinin ertelenmesini "kesinti sırasında" koşulsuz yazar ve kesintinin tespitini `05`'e bırakır. İki dakikadan kısa kesinti ödeme süresinin son anına denk gelirse iptal ertelenmez; ama ertelemenin gerekçesi firmanın kesintide işaret koyamamasıdır (Z-8) ve bu durumda firmanın kaybı çok iş günlük sürenin sonundaki en çok iki dakikadır; parası gelmiş iptal edilmiş siparişte firmanın yolu `02 §3.21.8`'dedir — hak ya da para kaybı doğmaz. **Öneri elendi:** "nabız aralığını aşan her boşluk kesinti olsun" — tek bir gecikmiş nabız ya da güncellemenin saniyelik yeniden başlaması (§8.6.3) kesinti kaydı açar ve o gün dolan havale iptallerini ilk iş gününe ertelerdi, ayrılan stok o kadar kilitli kalırdı. Eşiğin anlamı ve kalan riski yazıldı; karar değişmedi (K-898). | 5.4.1: "iki dakikadan kısa boşluk kesinti değildir" · 5.4.6: elenen ve kalan risk · §12 MK-44 · Doğrulama Protokolü'nün park satırı — senaryo (4). Yeni karar yok (K-898'in yazımı). |
| Tek · BULGU-1 — yeni | ⚠️ KISMİ | **Düşük** — olgusal bir cümle hatası; süre `02`'de tanımlıdır | **Cümle yanlıştı, saklama değil.** 6.4.9 bağlantı kaydı için "kişisel veri taşımaz" diyordu; kayıt hesaba bağlıdır ve hesabı belirli bir kişiye ait olduğu için kişisel veridir (KVKK m.3). Aynı kalıp 6.1.1'de oturum kaydı için ("kişisel veri değil, hesaba bağdır") ve 4.3.4'te iş satırı için vardı. **Saklamanın "süresiz ve amaçsız" olduğu iddiası RET:** bağlantı kaydı hesabın giriş bilgisidir ve süresi Ürün Gereksinimleri'nin süre envanterindeki hesap verisi satırıdır — Z-31, "hesap durduğu sürece, hesap silindiğinde hemen" (`02 §4.2`); amacı bağlantının hâlinin doğru gösterilmesidir ve gecelik temizlikte silinmesi 6.4.9'da gerekçesiyle elendi (`04` 5.21.12). Ayrı ve kısa bir süre `02 §4`'e yeni bir süre eklerdi. | 6.4.4: "hesabın verisidir — hesap verisinin süresiyle (Z-31)" · 6.4.9: gerekçe hücresinin cümlesi · §6.4'ün Kaynak satırına `02 §4.2` Z-31 · 6.1.1: oturum kaydı aynı kalıpla · 4.3.4: "kişisel alan taşımaz". Yeni karar yok. |
| P3 · BULGU-1 — **tekrar eden RET** | ❌ RET (tekrar eden) | — | Google hesabının kalıcı kimliğinin saklanmaması bilinçli karardır (K-900; §6.1.4); önceki turlarda aynı yer ve iddiayla en az iki kez gerekçeyle reddedildi (6. turun raporu §6'nın listesi; 7. tur). Turun sonucunu belirlemez. | — |

**Dağılım:** altı ayrı bulgu — 5 KISMİ · 1 tekrar eden RET; RET yok. **Şiddet:** bir orta (P3 · BULGU-2), dört düşük. **%100 kabul değil:** beş kısmi kabulün beşinde modelin önerisi ya da iddianın bir kolu somut yerle elendi (Google'ın belgesi, `02 §3.17.7`, K-962'nin sırası, §8.6.3, `02 §4.2` Z-31). **Üç bulgu yeni karar doğurdu** (K-967 ⚠ dışı; K-968 ve K-969 ⚠).

## 4. Ek bulgular

- **Aynı kalıbın taraması (Tek · BULGU-1):** "kişisel veri taşımaz", "kişisel veri değil" ve "kişisel alan taşımaz" `05`'te tarandı. "Kişisel veri" diyen dört yer: 4.3.4 (iş satırı — "kişisel alan taşımaz"a çevrildi; satır kayda kimlikle işaret eder), 6.1.1 (oturum kaydı — hizalandı), 6.4.9 (bağlantı kaydı — hizalandı), 9.2.3 (konteynerin açılış ve çökme iletileri — hesaba bağlı değildir, doğrudur). "Kişisel alan taşımaz" diyen yerler (4.5.2, 6.8, MK-39'un hücreleri) alan düzeyinde doğrudur.
- **Aynı kalıbın taraması (P3 · BULGU-2):** "taze", "yeniden doğrulama" ve "Google ile" `05`'te tarandı: 6.1.1, 6.1.3, 6.1.4, 6.3, 6.5.4, 6.5.6, 6.7'nin akış çerezi satırı ve 10.1.5 akışın kendisini anar ve doğrudur; özet yüz MK-46'dır — hizalandı. Üst dokümanların terim taraması karar kaydının §12.4'ündedir: güncellenen yalnız Arayüz Tanımları'nın E-24'üdür.
- **Aynı kalıbın taraması (P2 · BULGU-1):** "kilit sırası", "Kilitler sabit", "aynı sepete bağlı" — 4.2.2, 4.2.3, 7.3.5, MK-21 ve MK-36. MK-21'in "sipariş, varyant, kupon" sırası satır kilitlerinindir ve değişmedi; danışma kilidi bu sıraya girmez.
- **Aynı kalıbın taraması (P4 · BULGU-1):** "kalan risk değil", "silmelerin geri gel", "silme satır", "tamponda" — §7.4.5 tek yerdir; §8.5.10, MK-72 ve `DEPLOY_RUNBOOK`'un iki satırı komutun işleyişini anar, güvence cümlesi kurmaz. Runbook'un yedek satırı kalan riski aldı; güncelleme satırı değişmedi. Kodlama Kılavuzu'nun park satırı satırın sırasını anar (K-962) — değişmedi, K-969 "park gerekmez" der.
- **Aynı kalıbın taraması (P2 · BULGU-2):** "iki dakika", "kesinti kaydı" — 5.4.1, 8.6.3 ("kesinti saniyelerdir"), 9.4.15'in elenen satırı, 10.1.14 ve 10.1.16 (saat ve ad çözücü — sahte kesinti) ve MK-44; ilk ve sonuncusu hizalandı, ötekiler eşiğin tanımıyla tutarlıdır.
- **Kendi okumam — bulguların yeri:** beş kısmi kabulün beşi de güvence cümlesi ile mekanizmanın farkı sınıfındadır ("taze giriş", "önceki sipariş iptal edilir", "kalan risk değildir", "kesinti sırasında", "kişisel veri taşımaz"). Dördü §4, §5 ve §6'dadır — 7. turun raporunun güvence merceğinin taramadığı bölgeler olarak adlandırdığı yerler; beşincisi (§7.4.5) merceğin bölgesindedir ve merceğin kendi kararının (K-962) cümlesidir. Parça 1 (§0–§3) ve parça 5 (§8–§12) TEMİZ döndü.
- **Mekanik tarama:** Kaynak satırı betiği değişiklikten önce ve sonra aynı sonucu verir — tek fark §12'nin gerekçe hücresine giren K-967, K-968 ve K-969'dur (§12'nin Kaynak satırı "K numaraları gerekçe hücresindedir" der); `04`'te K-968 §5.24'ün ve §6.2'nin Kaynak satırındadır. Park satırlarının etiketleri (Entegrasyon Spesifikasyonu, Doğrulama Protokolü, `DEPLOY_RUNBOOK`) ile K-967…K-969'un etki sütunları iki yönde eşleşir. Karar kaydında çift numara yok; K-855'ten bu yana `(öneriyle kaydedildi — ⚠)` taşıyan satırlar betikle sayıldı — 38 — ve §12.1'in listesiyle aynı. Değişen dosyalarda CR yok; geri başvurulu değiştirme kullanılmadı.
- **Önceki turlardan açık kalanlar:** yok — 7. turun önerisi (güvence–mekanizma sınıfında orta bulgu gelirse merceği §4, §5, §6'da koşmak) bu turun sonucuyla tetiklendi; §6'da öneri olarak taşınır.

## 5. Kullanıcı onay checklist'i

K-855 kararıyla (Aşama 4 boyunca ⚠ öneriyle kayıt, CI yeşilse `docs:` PR merge yetkisi ve yönetici + alt ajan düzeni) alt ajan değerlendirdi ve uyguladı (`05` v0.13). Bu turun kararları öneriyle kaydedildi: **K-967** (⚠ dışı — aynı sepetin onayları sepetin danışma kilidiyle sıraya girer); **K-968 ⚠** (Google ile yeniden doğrulamada girişin tazeliği doğrulama anıyla denetlenir; Google'da oturumu açık kullanıcı Google'dan çıkıp yeniden girer ya da şifre koyar); **K-969 ⚠** (anlık silmenin kesinleşmesi ile satırının diske inmesi arasındaki çökme, eski bir ana dönüşte o silmeyi geri getirir — kalan risk). ⚠ listesi 38. Geri besleme: Arayüz Tanımları v0.25 (K-968). Park satırları: Entegrasyon Spesifikasyonu (Google ile giriş — K-968), Doğrulama Protokolü (eşzamanlılık ve kesinti — K-967, K-898'in eşiği), `DEPLOY_RUNBOOK` (yedek, kurtarma ve prova — K-969). Geri işaret: K-890, K-900, K-955, K-957, K-962.

- [x] P3 · BULGU-2 (kısmi, orta — K-968 ⚠: doğrulama anı; Arayüz Tanımları v0.25)
- [x] P2 · BULGU-1 (kısmi, düşük — K-967: sepetin danışma kilidi)
- [x] P4 · BULGU-1 (kısmi, düşük — K-969 ⚠: çökme penceresi kalan risk)
- [x] P2 · BULGU-2 (kısmi, düşük — kısa nabız boşluğunun tanımı ve kalan riski; K-898)
- [x] Tek · BULGU-1 (kısmi, düşük — hesabın kayıtları hesabın verisidir, Z-31)
- [x] P3 · BULGU-1 (tekrar eden ret — K-900)
- [ ] Etki yansıtma (Faz 5) — döngü TEMİZ döndükten sonra, son turun raporunda

**Hukuki kontrol** (2026-10-17): iki bulgu yasal dayanak taşır. Tek · BULGU-1 KVKK'nın kişisel veri tanımına (m.3) ve saklama ilkelerine (m.4/2) dayanır — tanım kısmı kabul edildi; saklama süresi `02 §4.2`'nin Z-31'idir ve amacı 6.4.9'da yazılıdır, yeni hukuki içerik eklenmedi. P2 · BULGU-2 "hak kaybı" der — ertelemenin dayanağı ürün kuralıdır (`02 §4.3`, Z-8), yasal bir süre değildir; yasal süreler hiçbir durumda dondurulmaz (`02 §4.3`). P3 · BULGU-2'nin dayanağı Google'ın belgesidir (okundu 2026-10-17; bilgim tarihli — implementation'da doğrulanır).

**Yakınsama ölçüsü:** ayrı bulgu 7 → 5 → 6 → 6 → 5 → 5 → 4 → 6; kabul edilen ya da kısmen kabul edilen 7 → 3 → 3 → 3 → 4 → 3 → 1 → 5; turun yeni kararı 2 → 0 → 1 → 2 → 2 → 1 → 1 → 3 (bu turun ikisi ⚠); dönen iddia 0 → 1 → 3 → 2 → 1 → 2 → 2 → 0; tekrar eden RET 0 → 0 → 0 → 2 → 1 → 0 → 2 → 1. TEMİZ çağrı 0 → 1 → 1 → 1 → 2 → 2 → 2 → 2. Doküman 335 160 bayttan 341 516 bayta büyüdü; tablolara satır girmedi.

**Orta ve yüksek şiddetli yeni bulguların yeri:** 3.–6. turların beşi (§8.7.4 iki kez, §4.2.2, §8.7.1, §7.6.6); 7. turda §7.6.1 (dönen); 8. turda §6.1.5. Güvence merceğinin düzelttiği bölgelerde (§7.6, §8.5–§8.8) bu turda bulgu yok; §7.4.5'teki düşük bulgu merceğin kendi kararının cümlesidir.

## 6. Sonuç ve 9. tur

**Sonuç: TEMİZ DEĞİL** (ölçünün dört sınıfında) — altı çağrının ikisi TEMİZ; altı ayrı bulgunun beşi kısmen uygulandı, biri tekrar eden RET. Faz 5 (etki yansıtma) bu turda koşmadı; TEMİZ turunda koşar.

**Çıkış kuralı bu turda uygulanmadı.** Skill'in Faz 4 madde 5'inin ikinci kuralı kabul edilen bulguların **hepsinin** düşük şiddette iki yerin metin farkı olmasını ister. P3 · BULGU-2'nin kabulü orta şiddettedir: Google ile yeniden doğrulamada dokümanın koyduğu güvencenin mekanizması yoktu, düzeltme mekanizmayı değiştirdi ve bir üst dokümana geri beslendi; durum nadir değildir. Kural bu durumda döngüyü sürdürür; karar satırı gerektirmez (K-849 kalıbı yalnız kural uygulandığında kayda girer).

**Yakınsama gözlemi.** 7. turun önerisinin koşulu bu turda gerçekleşti: orta bulgu yine güvence ile mekanizma farkı sınıfındadır ve kısmi kabullerin beşi de bu sınıftadır; dördü güvence merceğinin taramadığı §4, §5 ve §6'dadır. 7. turda mercek iki bölgede bu sınıfı bir kerede kapatmış, ikinci model o bölgelerde yeni bir orta bulgu getirmemişti; bu tur da §7.6 ve §8.5–§8.8'de bulgu getirmedi. İkinci model sınıfı her turda başka bir bölgede, birkaç tane bulmaya devam ediyor — kabul 1'den 5'e çıktı.

**Öneri — 9. turdan önce güvence merceği §4, §5 ve §6'da.** Bu alt ajan uygulamadı. 7. turdaki mercekle aynı biçimde: §4 (durum ve olaylar — "tek işlenir", "kaybolmaz", "çift gönderilmez", "yeni sipariş açmaz"), §5 (arka plan işleri — "kaçırılan iş dönüşte çalışır", "sonucu değiştirmez", kesinti ve nabız) ve §6 (güvenlik — "yeniden doğrulama", "anında", "tek kullanımlık", "silinemez", "yalnız … erişir") alt bölümlerinin güvence cümleleri betikle envantere çıkarılır, mercekler — durum ve eşzamanlılık, arka plan ve kesinti, kimlik ve oturum, özet yüzler — her cümleyi bileşen, ayar, sıra, eşzamanlı yarış ve arıza hâline karşı okur, iki şüpheci bulguları doğrular; düzeltmeler uygulanır ve 9. tur merceğin düzeltmelerinden sonraki dokümanla koşar. §7.4 ve §7.5 de merceğin dışındaydı; bu turun §7.4.5 bulgusu yüzünden kapsama eklenmesi önerilir.

**9. tur** aynı istemle (1. turun raporunun §1'i, değiştirilmeden), aynı şablonla ve iki koşumla (K-953) yürür; parça sınırları bölüm başlıklarıdır. Önceki turların raporları, karar kaydı, audit raporları ve merceğin raporu verilmez (K-431). Tur, altı çağrının altısı da `SONUÇ: TEMİZ` dönerse — ya da yalnız tekrar eden RET'ler kalırsa — TEMİZ'dir. **9. turda tekrar eden RET sayılacaklar** (aynı yer ve iddiayla dönerlerse): Google hesabının kalıcı kimliği (§6.1.4; K-900) · ödemesi alınmamış siparişte kişisel verinin üç yılı (§7.4.3 (4), §4.4.4; K-951) · yedeğe ulaşmamış penceredeki silmeler ya da ayrı bir silme deposu (§7.4.5; K-888, K-939, K-955). **Bir kez reddedilmiş, dönerse kendi gerekçesiyle okunacaklar:** misafir sepetinin kalıcı çerezi (§6.7.8; 3. turda KISMİ, 7. turda RET — bir kez daha reddedilirse sonrası tekrar eden RET) · sistem kayıtlarının yedekteki süresi (§9.2.3; 4. turda RET, 6. turda KISMİ). Bu turun beş kısmi kabulünün iddiaları düzeltmelerle kapandı; aynı iddia düzeltmeden sonraki metne karşı dönerse yeni iddia gibi okunur. **9. turda çıkış kuralı uygulanabilir:** kabul edilen bulguların hepsi düşük şiddette iki yerin metin farkıysa bulgular uygulanır, aynı kalıp dokümanın tamamında taranır, döngü "TEMİZ — çıkış kuralıyla, 9 tur" diye kapanır, kuralın uygulanışı bir karar satırıyla kaydedilir (K-849 kalıbı) ve sınıfın kalan riski karar kaydının §12.1'inde aşamanın checkpoint'ine devredilir; kabullerden biri orta ya da yüksekse döngü sürer.
