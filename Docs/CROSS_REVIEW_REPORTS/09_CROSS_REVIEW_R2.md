# Cross-Review — 09 Coding Guidelines (Tur 2)

**Tarih:** 2026-10-30 · **Hedef:** `Docs/09_CODING_GUIDELINES.md` v0.19 (girdi commit'i `3239906`, 454 839 bayt) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma); 1. turla aynı model · **Ciddiyet ölçüsü:** 1. turdan beri uygulanır, mekanizma okuma paragrafıyla (K-1383) · **Koşum:** iki koşum — tek parça çağrı ve 1. turun beş parçası (sınırlar değişmedi) · **Sonuç:** **TEMİZ DEĞİL** — sekiz ayrı bulgu (dokuz ham); biri kabul, üçü kısmi, dördü reddedildi (ikisi 1. turdan **tekrar eden RET**).

## 1. Koşum

**Betikler:** 1. turun betikleri aynen kullanıldı (`C:\Users\TEKNOD~1\AppData\Local\Temp\claude\c--projects-Shopfolio\2ac3bfe4-1eaa-4636-a2d5-5112be6cc06a\scratchpad\cr09\`): `python -I build_inputs.py <doküman> template09.md prompt.txt partnote.txt in2` (parça sınırları başlıkla; `uncovered lines: none`), `bash run_codex.sh in2 out2 c1 p1 p2 p3 p4 p5`. İstem (`prompt.txt`) ve parça notu (`partnote.txt`) 1. turdakiyle aynıdır; girdi yalnız doküman ve şablondur.

**Komut:** 1. turdaki gibi (`codex exec -s read-only -C <boş klasör> --skip-git-repo-check --ephemeral --ignore-user-config --color never -m gpt-5.6-terra -c 'model_reasoning_effort="high"' -o <çıktı> - < <girdi>`). Altı çağrı paralel koştu; kullanım sınırı dolmadı, çıktısız dönen çağrı olmadı, yedek modele geçilmedi.

| Çağrı | Kapsam | Girdi | Token | Süre | Web araması | Sonuç |
|---|---|---|---|---|---|---|
| c1 | tek parça | 464 283 bayt (428 064 karakter) | 165 913 | 220 sn | 4 | 2 bulgu |
| p1 | §1–§6 | 128 573 bayt | 85 110 | 90 sn | 6 | 1 bulgu |
| p2 | §7 · §8 · §9 · §10.1–§10.5 | 153 646 bayt | 52 737 | 118 sn | 0 | 1 bulgu |
| p3 | §10.6–§13 | 169 667 bayt | 76 933 | 175 sn | 4 | 1 bulgu |
| p4 | §14–§16 | 154 367 bayt | 66 287 | 114 sn | 4 | 1 bulgu |
| p5 | §17–§25 | 144 185 bayt | 80 709 | 211 sn | 6 | 3 bulgu |

**Ölçüm (skill, Faz 1 madde 9).** Tek parça çağrı iki bulgu getirdi: F1'in tekrarı ve §18.2 maskelemesi (G1; parça 5 de buldu). Parçalı koşum öteki altısını getirdi (F5'in tekrarı, G2, G3, G4, G5, G6); G2…G6 tek parça çağrının görmediği bulgulardır ve dördü kabul ya da kısmi oldu. **İki koşum sonraki turlarda da sürer.** Web aramaları PostgreSQL ve .NET belgelerine gitti; hiçbir bulgu bir yasal iddiaya dayanmadı. Platform iddiası taşıyan G4, doğrulayıcıda .NET sayısal biçim dizgeleri belgesine karşı okundu (okuma tarihi 2026-10-30).

## 2. Ham bulgular

### Çağrı (a) — tek parça

> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §10.8, madde 7
> Alıntı: "`data_protection_keys`'e kimlik sütunlu `INSERT` ek bir dizi yetkisi olmadan çalışır"
> Sorun: PostgreSQL’de tabloya verilen `INSERT` yetkisi, `int` kimlik sütununun bağlı olduğu dizide `nextval` çalıştırma yetkisi vermez. Veri koruma paketinin tamsayı kimliği otomatik üretildiğinde uygulama kullanıcısı yeni anahtar kaydını yazamaz. Bu, anahtar üretimi/döndürmesi sırasında antiforgery, oturum ve korumalı değer akışlarını çalışamaz hâle getirerek kullanıcıyı akışta bırakabilir. (3)
> Öneri: `data_protection_keys` kimlik dizisine uygulama kullanıcısı için en az `USAGE` yetkisini, kullanıcı adını rol ayarından okuyarak tabloyu oluşturan göçte verin; bunu gerçek `PersistKeysToDbContext` anahtar yazımıyla entegrasyon testine ekleyin.
>
> BULGU-2
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §18.2, madde 2
> Alıntı: "iletide (iç istisnalar dahil) e-posta, IBAN ve telefon örüntüleri `*` olur"
> Sorun: Doküman şifre, anahtar, sır, kart verisi, adres ve çerez değerinin hiç kayda girmeyeceğini söyler; ancak istisna iletisinde yalnız e-posta, IBAN ve telefon örüntülerini maskeler. Bir istisna iletisi veya serbest adlı yapılandırılmış özellik parola, erişim anahtarı, token, kart verisi ya da adres taşıdığında biçimleyici bunları yazabilir. Bu, kişisel veri ve sırların yetkisiz log erişimine açılmasıdır. (1)/(2)
> Öneri: İstisna iletisini ve serbest yapılandırılmış özellik değerlerini loglamayın; yalnız kapalı olay kodu, istisna türü ve izinli alanlardan oluşan bir şema yazın. Gerekliyse iletileri serbest metin yerine kapalı hata koduna eşleyin.
>
> SONUÇ: 2 BULGU

### Parça 1 — §1–§6

> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §4.1 madde 2; §5.2 madde 2; §6 madde 2
> Alıntı: “veritabanı bağlamı tektir ve `Infrastructure/` altındadır”; “Her varlık türünün — bağ tabloları dahil — bağlamda bir `DbSet` özelliği vardır”; “Ortak altyapı modül türlerine başvurmaz.”
> Sorun: Dokümanın kendi içinde çelişki var (4). `Infrastructure/` altındaki tek `DbContext`, her modül varlığı için örneğin `DbSet<Order>` taşımak zorunda; bu, altyapının `Order` gibi modül türlerine doğrudan başvurmasıdır. Mimari test son cümleyi mutlak uygularsa model/göçler kurala takılır; uygulama için gevşetilirse yazılı mimari kural ihlal edilir.
> Öneri: `DbContext` için açık ve dar bir istisna yazın: yalnız EF model metadatası, `DbSet<T>` ve varlık yapılandırmaları amacıyla modül türlerine başvurabilir; bunun dışındaki `Infrastructure` türleri modül türlerine başvuramaz. Mimari testi de bu istisnayı kapsayacak şekilde sınırlandırın.
>
> SONUÇ: 1 BULGU

### Parça 2 — §7–§10.5

> BULGU-1
> Kriter: tutarlılık
> Seviye: Yüksek
> Yer: Başlık notu — K-899… güvenlik kalıpları; §9.2 madde 4
> Alıntı: "şifre, anahtar, sır, kart verisi, IBAN, e-posta, adres ve telefon hiçbir sistem kaydına yazılmaz" / "firmanın ayarındaki değerler (IBAN, iade adresi) `UpdateSettings`'te eski/yeni değerle yazılır"
> Sorun: Doküman, IBAN ve adresin hiçbir sistem kaydına yazılmayacağını söylerken denetim izine eski/yeni IBAN ve iade adresi yazılmasını emrediyor. Bu doğrudan iç çelişkidir (4) ve denetim kaydına hassas finansal/adres verisi taşır.
> Öneri: `UpdateSettings` denetim izinde IBAN ve iade adresi değerlerini değil, yalnız değişen alan adlarını ve “değiştirildi” durumunu saklayın; §9.2’deki istisnayı kaldırın ve ilgili testi değerlerin bulunmadığını doğrulayacak şekilde değiştirin.
>
> SONUÇ: 1 BULGU

### Parça 3 — §10.6–§13

> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §13.3 madde 2
> Alıntı: "`RescheduleAfter(TimeSpan.Zero)` — iş kuyruğu her turda en eski zamanı gelmiş satırı aldığı için dönüşüm öteki satırları aç bırakmaz."
> Sorun: Bu sıralama tersine aç bırakma yaratır. Dönüşüm satırı her parçadan sonra hemen zamanı gelmiş olur; o anda veya sonrasında eklenen normal işler ondan daha yeni kalır. Büyük bir dönüşüm, yeni ödeme bildirimi, iade veya e-posta işlerini dönüşüm bitene kadar sürekli geride bırakabilir. Bu, para ve müşteri iletişimi işlemlerinin süresiz gecikmesine yol açabilir (2).
> Öneri: Dönüşüm parçasını sıfır süreyle değil, normal işlerin araya gireceği sabit kısa bir gecikmeyle yeniden planlayın; veya çalıştırıcıda dönüşüm işleri için açık bir adalet/round-robin kuralı koyun ve bunu entegrasyon testiyle doğrulayın.
>
> SONUÇ: 1 BULGU

### Parça 4 — §14–§16

> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §14.2 madde 1
> Alıntı: "`ToPlainText(decimal amount)` — kültürden bağımsız (`CultureInfo.InvariantCulture`), ondalık nokta, her zaman iki hane"
> Sorun: Aynı bölüm “hiçbiri yuvarlamaz” derken `ToPlainText` iki ondalıktan fazla haneli girdiyi reddetmiyor. İki haneli sabit-nokta biçimleme, örneğin 10,123 değerini sağlayıcıya 10.12/10.13 olarak göndererek sessiz yuvarlar. Bu, tahsilat veya iade tutarının sipariş tutarından sapmasına yol açabilir; kılavuzun yuvarlamasız sağlayıcı dönüşümü güvencesini bozar. [.NET biçimleme davranışı](https://learn.microsoft.com/en-us/dotnet/standard/base-types/standard-numeric-format-strings)
> Öneri: `ToPlainText` öncesinde `ToKurus` ile aynı ölçek ve negatiflik denetimini zorunlu kılın; iki ondalıktan fazla veya negatif tutarda istisna atın. 10,123 ve 10,125 için reddetme testleri ekleyin.
>
> SONUÇ: 1 BULGU

### Parça 5 — §17–§25

> BULGU-1
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §18.2 madde 2
> Alıntı: “iletide (iç istisnalar dahil) e-posta, IBAN ve telefon örüntüleri `*` olur”
> Sorun: Doküman, şifre, anahtar, sır, kart verisi ve adresin de hiçbir kayda yazılmamasını emrediyor; fakat beklenmeyen istisna iletisinde yalnız e-posta, IBAN ve telefon maskeleniyor. İstisna iletisi bu diğer değerlerden birini taşıdığında normal hata kaydıyla sızabilir. Bu, kişisel verinin yetkisiz açığa çıkmasına yol açabilecek bir boşluktur (sınıf 1).
> Öneri: İstisna iletisini varsayılan olarak kayda hiç yazmayın; yalnız açıkça güvenli kabul edilmiş hata türleri için sabit/izinli bir ileti yazın. İleti zorunluysa tüm hassas değer sınıflarını kapsayan, değer taşımayan bir hata kimliğine dönüştürün.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §18.1 madde 2; §18.2 madde 5
> Alıntı: “Uygulama süreci `audit-…`, komut süreci `command-…` dosyasıyla kurar.” / “İkisi de düzey ayarından bağımsız, denetim yolundan yazılır (§18.1).”
> Sorun: Yönetim komutu kaydının `command-…` yazıcısına mı, yoksa denetim yolundaki `audit-…` yazıcısına mı gideceği çelişkilidir (sınıf 4). Uygulama iki farklı davranışla kurulabilir; komut kayıtları zorunlu dosyasında bulunmayabilir veya gereksiz çift kayıt oluşabilir.
> Öneri: §18.2 madde 5’i açıkça ayırın: anlık silme yalnız denetim yazıcısına ve `audit-…` dosyasına; yönetim komutu yalnız komut yazıcısına ve `command-…` dosyasına yazılır.
>
> BULGU-3
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §19.2 madde 2–3; §21.4 madde 1
> Alıntı: “Birim testi veritabanına, dosya sistemine ve ağa dokunmaz.” / “Kaynak metin taramaları … sıradan birim testleridir.” / “repo kökünü `Shopfolio.slnx`’ten bulur ve `Docs/` dosyalarındaki tabloyu okur.”
> Sorun: Kaynak ve eşitlik taramaları `UnitTests` içinde sıradan birim testi olarak tanımlanmış, ancak zorunlu olarak kaynak ve `Docs/` dosyalarını okurlar. Aynı test türü için dosya sistemi hem yasaklanmış hem zorunlu kılınmıştır (sınıf 4).
> Öneri: Dosya sistemi yasağını yalnız davranış birim testleriyle sınırlandırın; kaynak/eşitlik taramalarını açıkça “statik denetim” olarak sınıflandırıp salt-okunur repo erişimine izin verin.
>
> SONUÇ: 3 BULGU

## 3. Bağımsız değerlendirme

Dokuz ham bulgu sekiz ayrı bulguya indi: G1 iki çağrıdan (tek parça B-2, parça 5 B-1), öteki yedisi birer çağrıdan geldi. Altı yeni bulgu birer `[dogrulama]` Opus / high doğrulayıcıya verildi (yüksek G1, G2, G3, G4 tek tek; orta G5 ve G6 farklı bölümlerden olduğu için ayrı ayrı; toplam altı). Tekrarlayan iki bulgu (F1, F5) 1. turun doğrulayıcı hükmüne ve metne karşı okundu; metin 1. turdan beri değişmedi (`09` §10.8 madde 7, §9.2 madde 4, başlık notu K-914 dokunulmadı), yeni bilgi yok: ayrı doğrulayıcı açılmadı. Doğrulayıcı girdileri ve hüküm dosyaları `C:\Users\TEKNOD~1\AppData\Local\Temp\claude\c--projects-Shopfolio\2ac3bfe4-1eaa-4636-a2d5-5112be6cc06a\scratchpad\cr09\dr2\` altındadır. Tablo %100 KABUL/%100 RET değildir.

| # | Bulgu (kaynak) | Karar | Gerekçe | Uygulama |
|---|---|---|---|---|
| G1 | İstisna iletisinde yalnız e-posta, IBAN, telefon maskelenir; adres, anahtar, sır sızabilir (tek parça B-2, parça 5 B-1; §18.2 madde 2) | ⚠️ KISMİ | Gerçek (4): `05 §9.1.5` adres, anahtar, sır, kart verisini de "hiçbir sistem kaydına" yazdırmaz; `05 §10.3.6` hata satırının alanlarını sayar ve ileti saymaz; §13.1 madde 2 `LastErrorSummary`'ye iletiyi "kişisel veri ya da parametre değeri taşıyabilir" diye yazmaz — aynı ileti uygulama kaydına örüntü maskesiyle gidiyordu; örüntü üç türü yakalar, adresi, anahtarı, sırrı değil (ör. PostgreSQL 22P02 iletisi değeri ana iletide taşır, bilgim tarihli). K-1374 (2) iletiyi hiç yazmama seçeneğini elemişti (gerekçe: "örüntü maskesi korur"); gerekçe üç tür için doğru, listenin tamamı için değil — bilinçli karar cümlesine göre karar kendi içinde çelişki doğuruyor. Reddedilen kısım: sınıf (1)/(2) değil, para ya da hak kaybı yok; seviye yüksek değil, (4) orta | K-1386 ⚠: §8 madde 7, §18.2 alan tablosu (`innerExceptionTypes`, `sqlState`; ileti yok), madde 2c (ileti iç istisna dahil hiç yazılmaz; veri sözlüğü yalnız anahtar), madde 7 testi. Üst doküman etkisi yok |
| G2 | Tek `DbContext` `Infrastructure/` altında ve her varlığın `DbSet`'i var ↔ "Ortak altyapı modül türlerine başvurmaz" (parça 1 B-1; §4.1 madde 2, §5.2 madde 2, §6 madde 2) | ✅ KABUL | Çelişki gerçek (4): §6 madde 2 "ortak altyapının bağlam dışındaki türleri" diyerek bağlamı ortak altyapı sayar; §6 madde 4 kuralı istisnasız mimari test yapar ve kasıtlı ihlalle kırmızı, gerçek kodda yeşil ister; sözlük gibi yazılan test `AppDbContext` yüzünden ilk gün kırmızıdır; istisna dokümanın hiçbir yerinde yok (`grep`: "bağlam dışındaki", "istisna", "DbContext"). Uygulayıcı testi gevşetir (§22 madde 3, §19.2 madde 3 yasak) ya da istisnayı uydurur. Göçler için istisna şart değil: model anlık görüntüsü varlıklara adla başvurur (bilgim tarihli, iskelette sınanır). Seviye: model yüksek, doğrulayıcı (4) orta | K-1386: §6 madde 2'ye tek istisna olarak bağlam sınıfı (`AppDbContext`) ve sınırı (yalnız `DbSet` ve varlık yapılandırması; iş, sorgu, modül çağrısı yok); §6 madde 4 testi istisnayı sınıf adıyla dışarıda bırakır; yan gözlem: §9.2 madde 1'in "`DbSet<AuditLog>` yalnız `AuditTrail`'dedir" cümlesi aynı çelişkiyi taşıdığı için düzeltildi. Üst doküman etkisi yok |
| G3 | `RescheduleAfter(TimeSpan.Zero)` yeni işleri aç bırakır (parça 3 B-1; §13.3 madde 2) | ❌ RET | Mantık ters kurulmuş. §13.1 madde 3: zamanı gelmiş satırlar `COALESCE(next_attempt_at, created_at)` artan sırayla alınır; `RescheduleAfter(span)` sonraki deneme anını sonucun yazıldığı an + süre yapar (`06 §4.59`, `05 §4.3.4`; §13.1 madde 5 tablosu), yani sıfır süreyle dönüşüm satırı **en yeni** anahtarı alır, en eskisini değil. Sonradan eklenen ödeme bildirimi ya da iade satırı en çok bir parça bekler (500 satır, `statement_timeout` içinde); sıra fiilen dönüşümlüdür. Örnekteki e-posta iş kuyruğunda değildir (`06 §4.59`). Sabit gecikme önerisi ek korumadır. Yeni bilgi yok | Yok |
| G4 | `ToPlainText` iki ondalıktan fazlasını sessiz yuvarlar (parça 4 B-1; §14.2 madde 1) | ⚠️ KISMİ | Gerçek (4): §14.2 "hiçbiri yuvarlamaz" der; güvenceyi yalnız `ToKurus` (ikiden fazla haneyi `ArgumentOutOfRangeException` ile reddeder) sağlar; `ToPlainText` için yalnız "her zaman iki hane" yazılı ve madde 4 `ToString("F2")`'yi yasaklayıp işlev içinde doğal uygulamayı `F2` yapar; .NET belgesi: fazla hane yuvarlanır ("the fractional value is rounded"), `10.123` → `10.12`. Kart iadesinde (`return_amount`) `ToKurus` yolu yok. Reddedilen kısım: (2) para kaybı ve yüksek seviye — `Order.Total` ve `Refund.Amount` `numeric(12,2)`, firmanın girdisi en çok iki ondalıkla doğrulanır (`04`, `07`), üç haneli değer ancak hesap hatasıyla oluşur ve sapma kuruşun altındadır; seviye düşük (4) | K-1386: §14.2 madde 1'de `ToPlainText` biçimlemeden önce `ToKurus`'un hane ve negatiflik denetimini aynı istisnayla uygular; madde 5 testine `10,123` ve negatif tutarın istisnası. Üst doküman etkisi yok |
| G5 | Yönetim komutu kaydı `command-…` mi `audit-…` mı (parça 5 B-2; §18.1 madde 2, §18.2 madde 5) | ❌ RET | "Denetim yolu" dosya adı değil mekanizmanın adıdır: §18.1 madde 2 (denetim ve komut satırı `IAuditLineWriter` ile, Serilog `AuditTo` yolundan, `ILogger<T>` ile değil); dosyayı süreç seçer ("Uygulama süreci `audit-…`, komut süreci `command-…` dosyasıyla kurar"); her süreçte tek yazıcı tek dosyaya yazar, çift ya da eksik kayıt oluşamaz. §18.2 madde 5 "ikisi de denetim yolundan" der — hedef dosya söylemez, §18.1'e gönderir. `05 §8.5.7` aynı düzeni kurar (komut kendi dosyasına yazar), K-986. Önerilen ayrım bugünkü metinle aynı sonucu verir. Yeni bilgi yok | Yok |
| G6 | Birim testi dosya sistemine dokunmaz ↔ kaynak taraması ve eşitlik denetimi "sıradan birim testi" ve `Docs/` okur (parça 5 B-3; §19.2 madde 2–3, §21.4 madde 1) | ⚠️ KISMİ | Gerçek (4) düşük: §19.2 madde 2 niteleyicisizdir (özne bütün birim testleri); madde 3, §21.4 madde 1, §10.6 madde 11 ve §10.8 dosya okuyan taramaları açıkça "birim" diye etiketler. `InMemoryFileStore` parantezi niyeti sezdirir ve özel hüküm genel olanı ezer; asıl risk bağımsız doğrulayıcının eşitlik denetimlerini yasağa aykırı bulması. Reddedilen kısım: "statik denetim" adıyla yeni test türü — §19.2 tablosu, §21.4 ve onlarca "Kaynak taraması (birim)" etiketi değişirdi; tek cümlenin kapsamını daraltmak çelişkiyi kapatır | K-1386: §19.2 madde 2 "Davranış sınayan birim testi …" olur, repo dosyalarını yalnız okuyan mimari test, kaynak taraması ve eşitlik denetimi istisna olarak yazıldı (veritabanına ve ağa dokunmazlar, dosya yazmazlar). Üst doküman etkisi yok |
| F1 | (1. tur F1 tekrarı) `data_protection_keys` için dizi yetkisi (tek parça B-1; §10.8 madde 7) | ❌ RET — **tekrar eden RET** | 1. turdaki iddiayla aynı (identity sütunu için dizi `USAGE`); yer bu kez yalnız §10.8 madde 7 olarak gösterildi, metin değişmedi. PostgreSQL 18 kaynağı: kimlik varsayılanı `NextValueExpr` → `nextval_internal(seqid, false)` dizi yetkisini denetlemez; `serial` için doğru, identity için yanlış; §10.8 madde 7 varsayımı gerçek PostgreSQL'de çalışan entegrasyon testine bağlamıştır. Yeni bilgi yok (`dr/hukum_F1.md`) | Yok |
| F5 | (1. tur F5 tekrarı) IBAN ve iade adresi "hiçbir sistem kaydına" ↔ `UpdateSettings` izinde eski/yeni değer (parça 2 B-1; başlık notu K-914, §9.2 madde 4) | ❌ RET — **tekrar eden RET** | 1. turdaki iddiayla aynı, metin değişmedi. "Sistem kaydı" `05 §9.1.1`'de tanımlıdır (erişim kaydı, uygulama kaydı, sunucu kayıtları: disk dosyaları); işlem izi veritabanındaki ayrı kayıttır (`05 §4.7`, `06 §4.63`; `02 §10.3.3`); ayarın eski ve yeni değeri üründür (`02 §10.3.1` (6), K-414, K-523 — ele geçirilmiş hesap IBAN'ı değiştirip geri alırsa iz kalmalı). Yeni bilgi yok (`dr/hukum_F5.md`) | Yok |

**Tekrar eden RET kaydı (skill, Faz 4 madde 5).** Kural önceki turlarda en az iki kez gerekçeyle reddedilmiş bulguyu ister; F1 ve F5 yalnız 1. turda bir kez reddedilmişti. Yürütme talimatıyla (yönetici) kural bu turda uygulandı: ikisi de ikinci kez, aynı kanıtla reddedildi ve "tekrar eden RET" diye kaydedildi; bu tur sonucunu belirlemez (zaten G1, G2, G4, G6 kabul ya da kısmi). F3 ve F6 bu turda dönmedi. Üçüncü dönüşte kural tam koşulunu sağlar.

## 4. Ek bulgular

- **EK-1 (G2'nin doğrulayıcısı):** §9.2 madde 1'in `DbSet<AuditLog>` cümlesi §5.2 madde 2 ile aynı çelişkiyi taşıyordu; K-1386'da düzeltildi.
- **EK-2 (G3'ün doğrulayıcısı):** §13.1 madde 4 `RescheduleAfter`'ın sonraki deneme anını "sonucun yazıldığı an + süre" diye yazmıyor; güvence bu okumaya dayanır, `RetryAt(now + …)` kalıbı zaten veriyor — bulgu değil, netleştirme olarak uygulanmadı.
- **EK-3 (G5'in doğrulayıcısı):** `reapply-deletions` komut süreci `InstantDeletion` satırı yazarsa `command-…` dosyasına düşer ve `05 §8.5.7` bu dosyayı da uygulama kaydı sayar; kayıp doğmuyor, bulgu değil.

## 5. Kullanıcı onay checklist'i

- [x] G1 — istisna iletisi kayda hiç yazılmaz; K-1374 (2)'nin elenen seçeneği geri alındı (K-1386) ⚠
- [x] G2 — bağlam sınıfı altyapı kuralının adıyla yazılı istisnası (K-1386)
- [ ] G3 — RET (uygulanmadı)
- [x] G4 — `ToPlainText` fazla haneyi reddeder (K-1386)
- [ ] G5 — RET (uygulanmadı)
- [x] G6 — dosya sistemi yasağı davranış birim testiyle sınırlandı (K-1386)
- [ ] F1 — tekrar eden RET (uygulanmadı)
- [ ] F5 — tekrar eden RET (uygulanmadı)

Gösterilmeden kaydedilen ⚠ karar: G1 (K-1374 (2)'nin elenen seçeneğini geri alır; öneriyle kaydedildi). Çok kritik soru yok.

## Sonuç

**SONUÇ: TEMİZ DEĞİL — 8 ayrı bulgu (9 ham), 1 kabul, 3 kısmi, 4 ret (2 tekrar eden RET).** Skill'in çıkış kuralı: bütün çağrılar TEMİZ dönmedi (altı çağrının altısı da bulgu getirdi); 5. tur eşiği gelmedi; kabul ve kısmilerin hiçbiri mevzuat ya da para/hak kaybı sınıfından değil — hepsi (4), orta ya da düşük (iki yerin çelişmesi ya da dokümanın kendi güvencesini sağlamayan mekanizma). Sonraki tur: 3. tur (`09_CROSS_REVIEW_R3.md`), aynı betikler ve aynı parça sınırlarıyla, `09` v0.20 üzerinde. Etki yansıtma (Faz 5) döngü TEMİZ olunca ayrı adımdır; bu turda yapılmadı.
