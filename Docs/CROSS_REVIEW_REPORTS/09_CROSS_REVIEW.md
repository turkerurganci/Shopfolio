# Cross-Review — 09 Coding Guidelines (Tur 1)

**Tarih:** 2026-10-30 · **Hedef:** `Docs/09_CODING_GUIDELINES.md` v0.18 (girdi commit'i `5f4a64c`, 453 772 bayt, 2 385 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren uygulanır, mekanizma okuma paragrafıyla (K-1383; kuralın ayrıntısını üst dokümanlardan devralan ve kendi mekanizma kararlarını koyan türetim dokümanı — `cross-review` skill'i, "Ne zaman uygulanır" ikinci madde) · **Koşum:** iki koşum — tek parça çağrı ve beş parçalı kontrol koşumu (doküman 400 KB'ı aşar) · **Sonuç:** **TEMİZ DEĞİL** — yedi ayrı bulgu (sekiz ham); üçü kabul edilip uygulandı, dördü reddedildi.

## 1. Koşum

**Betikler** (sonraki turlar aynılarını kullanır; depoda değil, adım ajanının scratchpad'inde): `C:\Users\TEKNOD~1\AppData\Local\Temp\claude\c--projects-Shopfolio\2ac3bfe4-1eaa-4636-a2d5-5112be6cc06a\scratchpad\cr09\` — `build_inputs.py` (girdileri kurar: `python -I build_inputs.py <doküman> template09.md prompt.txt partnote.txt <girdi klasörü>`; parça sınırları betiğin `BOUNDS` listesinde, başlık önekiyle; kapsanmayan satırları basar — bu turda `none`), `run_codex.sh` (çağrıları paralel koşar: `bash run_codex.sh <girdi> <çıktı> c1 p1 p2 p3 p4 p5`), `prompt.txt`, `partnote.txt`, `template09.md` (şablon: `git show e74883f:Docs/09_CODING_GUIDELINES.md`, 5 125 bayt).

**İstem.** Entegrasyon Spesifikasyonları turunun istemi (`08_CROSS_REVIEW.md` §1) bu dokümana uyarlandı: yedi kriter, bulgu biçimi (`BULGU-N` — Kriter · Seviye · Yer · Alıntı · Sorun · Öneri — ya da `SONUÇ: TEMİZ`), `cross-review` skill'inin Faz 1 madde 4'teki bilinçli karar cümlesi değiştirilmeden, ciddiyet ölçüsünün paragrafı ve skill'in "mekanizma kararı koyan dokümanda okuma paragrafı" bu dokümanın mekanizmalarına (sürüm ve eşzamanlılık kalıbı, geçiş kuralı, yeniden deneme ve kuyruk, yuvarlama ve tutar dönüşümü, veritabanı kullanıcısının yetkisi, oturum/çerez/anahtar, maskeleme) uyarlanmış hâliyle. Girdi yalnız doküman ve şablondur (Faz 1 madde 2); karar kaydı ve önceki turların değerlendirmesi verilmedi.

> Sen bağımsız bir doküman denetçisisin. Aşağıda "=== DOKÜMAN ===" işaretinden sonra bir kodlama kılavuzu (coding guidelines) dokümanının metni, "=== ŞABLON ===" işaretinden sonra o dokümanın boş şablonu var. Başka bir dosya okuma; çalışma klasörü boştur.
>
> Bağlam: Belge bir kodlama kılavuzu dokümanıdır; kodlayan bir yapay zeka ajanına her kodlama görevinde verilir. Ürün kuralı koymaz: ürünün davranışı ayrı bir Ürün Gereksinimleri dokümanında (`02`), ekranların davranışı `04`'te, mekanizma ayrı bir Teknik Mimari dokümanında (`05`), alan ve değer adları ayrı bir Veri Modeli dokümanında (`06`), uç envanteri ve adresler ayrı bir API Tasarımı dokümanında (`07`), dış servislerle konuşma biçimi ayrı bir Entegrasyon Spesifikasyonları dokümanında (`08`), kapsam `10`'da durur; hiçbiri sana verilmedi. Bu doküman o dokümanların kuralını kod düzeyinde nasıl uygulayacağını yazar ve kendi mekanizma kararlarını koyar — proje ve klasör düzeni, adlandırma, tek zaman kaynağı, hata yönetimi, geçiş birimi, sürüm ve eşzamanlılık, önbellek, göç yazımı, veritabanı kullanıcıları ve süreleri, iş kuyruğu ve yeniden deneme, kuruşa yuvarlama, güvenlik yapılandırmaları, hız sınırı, kayıt ve maskeleme, test ve CI kuralları. Kural ayrıntısının burada yeniden yazılmamış olması ya da bir kurala yalnız bölüm numarası ve kimlikle (`02 §…`, `04 §…`, `05 §…`, `06 §…`, `07 §…`, `08 §…`, UC-nn, ÖK-n, YH-n) işaret edilmesi tek başına bulgu değildir; o dokümanların içeriğini denetleme. Dokümandaki `K-123` gibi işaretler sana verilmeyen bir karar kaydına gider. Yazım konvansiyonları başlık notundadır ("Yazım konvansiyonları"). Proje Aşama 8'dedir (kodlama kılavuzu); kod henüz yazılmadı; veritabanı PostgreSQL 18, çatı ASP.NET Core Razor Pages ve EF Core. Ürün, Türkiye'de tüketiciye satış yapan tek firmalık bir e-ticaret + kurumsal sitedir. Yasal iddiaları (mesafeli satış, KVKK, ödeme hizmetleri, e-ticaret mevzuatı) yürürlükteki resmî metne göre değerlendir.
>
> Dokümanı yedi kriterle denetle: tutarlılık · eksiklik · belirsizlik · teknik doğruluk · edge case · güvenlik · kullanıcı deneyimi.
>
> Dokümanda açıkça bilinçli karar, elenen seçenek ya da kabul edilmiş risk olarak yazılmış bir seçimi, yalnız seçime katılmadığın için bulgu yapma. Böyle bir karar dokümanın kendi içinde çelişki, olgusal ya da hukuki hata doğuruyorsa yaz.
>
> Yalnız CİDDİ sorunları yaz: (1) yürürlükteki mevzuata aykırılık, (2) müşteriye ya da firmaya para kaybı veya hak kaybı doğuran kural boşluğu, (3) kullanıcının takılıp kaldığı, çıkışı olmayan bir akış, (4) dokümanın iki yerinin birbiriyle çelişmesi. Nadir kenar durumları, iyileştirme önerilerini, savunmada derinlik önerilerini, üslup ve ifade tercihlerini bulgu yapma. Böyle bir sorun yoksa SONUÇ: TEMİZ yaz.
>
> Teknik doğruluk ve güvenlik bu dört sınıfın içinde okunur: yazılan bir mekanizmanın — bir sürüm ya da eşzamanlılık kalıbı, bir geçiş kuralı, bir yeniden deneme ya da kuyruk kuralı, bir yuvarlama ya da tutar dönüşümü, bir veritabanı kullanıcısının yetkisi, bir oturum, çerez ya da anahtar kuralı, bir maskeleme kuralı — dokümanın kendi koyduğu bir güvenceyi (ör. bir tahsilatın ya da iadenin tek kez uygulanması, bir tutarın kuruşu şaşırmadan hesaplanması, bir kişisel verinin kayda ya da dışarıya sızmaması, bir işlemin yarım kalmaması) sağlamaması (4)'tür; bir mekanizmanın siparişin, paranın ya da kişisel verinin kaybına veya kişisel verinin yetkisiz kişiye açılmasına yol açması (1) ya da (2)'dir. Ek bir koruma katmanı, ek izleme, daha güçlü bir araç ya da algoritma önerisi savunmada derinliktir ve bulgu değildir.
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

Parçalı çağrıların istemine aynı tek paragraflık parça notu eklenir (`partnote.txt`; `<bölümler>` aşağıdaki tablonun adlarıdır): verilmeyen bölüme yapılan atıf ve orada yaşayan içeriğin görünmemesi bulgu değildir.

**Komut** (Git Bash; çalışma klasörü boş): `codex exec -s read-only -C <boş klasör> --skip-git-repo-check --ephemeral --ignore-user-config --color never -m gpt-5.6-terra -c 'model_reasoning_effort="high"' -o <çıktı> - < <girdi>`. Altı çağrı paralel koştu; kullanım sınırı dolmadı, çıktısız dönen çağrı olmadı, yedek modele geçilmedi.

**Parça sınırları — kayıt (K-1383).** Sınır satır numarasıyla değil **başlıkla** tanımlıdır ve sonraki turlarda değişmez. **Başlık notu** dokümanın ilk satırından `## 1. Temel ilkeler` başlığına kadardır (sürüm, dört park bloğu, yazım durumu, yazım konvansiyonları, alt bölüm haritası, okuma tablosu; satır 1–144, yaklaşık 61 KB); her parça başlık notunu ve şablonu taşır.

| Parça | Başlar | Biter (dahil değil) | Bölümler | Satırlar | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|---|---|---|
| 1 | `## 1. ` | `## 7. ` | §1–§6 | 145–561 | 128 432 bayt | 68 525 | 187 sn | 1 bulgu |
| 2 | `## 7. ` | `### 10.6 ` | §7 · §8 · §9 · §10.1–§10.5 | 562–1011 | 152 648 bayt | 66 232 | 192 sn | 2 bulgu |
| 3 | `### 10.6 ` | `## 14. ` | §10.6–§10.10 · §11 · §12 · §13 | 1012–1507 | 169 460 bayt | 85 586 | 55 sn | 1 bulgu |
| 4 | `## 14. ` | `## 17. ` | §14 · §15 · §16 | 1508–1831 | 154 226 bayt | 69 027 | 216 sn | 1 bulgu |
| 5 | `## 17. ` | dosya sonu | §17–§25 | 1832–2385 | 144 041 bayt | 70 514 | 114 sn | 1 bulgu |

Parça girdileri 128–169 KB'tır (başlık notu 61 KB'ı her parçada tekrarlar); kapsanmayan satır yoktur (betiğin denetimi: `uncovered lines: none`).

**(a) Tek parça çağrı.** Girdi 463 216 bayt (427 083 karakter; aracın 1 048 576 karakterlik sınırının çok altında), 187 203 token, 237 sn: 2 bulgu. Tek parça girdi sınırına takılmadığı için birleşim çağrısı gerekmedi.

**Kapsam sınırı.** Parça öteki bölümleri görmez; tek parça çağrı parçalar arası okumayı yapar. Değerlendirme her iddiayı dokümanın tamamına ve ilgili yerde Ürün Gereksinimleri'ne (`02`), Arayüz Tanımları'na (`04`), Teknik Mimari'ye (`05`), Veri Modeli'ne (`06`), API Tasarımı'na (`07`) ve Entegrasyon Spesifikasyonları'na (`08`) karşı okudu (§3). **Web araması:** çağrı günlüklerindeki web araması satırı sayısı: tek parça 6 (PostgreSQL belgesi), parça 1: 4, parça 2: 0, parça 3: 6, parça 4: 0, parça 5: 4; hiçbir bulgu bir yasal iddiaya dayanmadı. Platform iddiası taşıyan F1, doğrulayıcıda PostgreSQL 18 kaynak koduna ve resmî belgeye, F4 EF Core resmî belgesine, F3 C# dil belirtimine karşı okundu (okuma tarihi 2026-10-30).

**İki koşumun ölçümü (skill, Faz 1 madde 9).** Parçalı koşum tek parça çağrının görmediği beş ayrı bulguyu getirdi (F3, F4, F5, F6, F7); üçü reddedildi (F3, F5, F6), ikisi (F4, F7) kabul edildi. Tek parça çağrı F2'yi tek başına buldu (iki bölümü birlikte okuyarak, kabul), F1'i parça 3'le birlikte buldu. Parçalı koşum kabul edilen bulgudan ikisini (F4, F7) getirdi: **iki koşum sonraki turlarda da sürer.**

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**(a) Tek parça çağrı:**
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §10.8 madde 2–3; §15.5 madde 1
> Alıntı: "`ALTER DEFAULT PRIVILEGES FOR ROLE shopfolio_migrator IN SCHEMA public GRANT SELECT, INSERT, UPDATE, DELETE ON TABLES TO shopfolio_app;`"
> Sorun: Uygulama kullanıcısına yalnız tablo yetkileri veriliyor; `data_protection_keys` tamsayı kimlikli olduğundan EF/Npgsql’nin üreteceği identity dizisinden değer alabilmesi için ayrıca sequence `USAGE` yetkisi gerekir. PostgreSQL’de tablo `INSERT` yetkisi sequence `nextval` yetkisini vermez ve diziler varsayılan olarak `PUBLIC`e açık değildir. Bu nedenle veri koruma anahtar halkasına yeni anahtar yazılamaz; oturum, antiforgery ve korumalı değer akışları başlangıçta veya anahtar dönüşümünde çalışamaz hâle gelir. [PostgreSQL 18 yetki dokümanı](https://www.postgresql.org/docs/current/ddl-priv.html)
> Öneri: `data_protection_keys` identity dizisi için uygulama kullanıcısına yalnız `USAGE` verin; bunu tabloyu oluşturan göçe ekleyin ve yetki matrisi testinde doğrulayın. Tablo yeniden kurulursa bu özel yetkinin de yeniden verildiğini zorunlu kılın.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §8.1 madde 6; §10.7 madde 6
> Alıntı: "Geniş `catch` yalnız beş yerdedir: yakalama katmanı; iş kuyruğunun satır döngüsü; zamanlayıcının kayıt döngüsü; yönetim komutlarının dağıtıcısı; dış servis bağlantı katmanlarının sonuç eşlemesi."
> Sorun: §10.7’de canlılık denetimi ayrıca `catch (Exception ex) when (!ct.IsCancellationRequested)` kullanıyor ve bunu beş izinli yerden biri olarak tanımlıyor. Canlılık denetimi §8.1’deki beş yerden biri değildir. Kaynak taraması/CI ya bu kodu yasaklayacak ya da dokümanın “yalnız beş yer” güvencesi ihlal edilecektir; makine ucunun 503’e güvenli dönüşü ile hata-yakalama kuralı aynı anda uygulanamaz. Bu, dokümanın kendi içinde çelişkidir.
> Öneri: Canlılık denetimini açıkça altıncı izinli geniş `catch` yeri olarak §8.1, §10.7 ve §23’te tanımlayın; kaynak taramasını bu kapalı altı yerle hizalayın.
>
> SONUÇ: 2 BULGU

**(b) Parça 1 (§1–§6):**
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §5.2 madde 2 / §6 madde 1
> Alıntı: "`public DbSet<Order> Orders => Set<Order>();`"; "Modülün geri kalanı — varlıklar, iş mantığı, yapılandırmalar, kuyruk işleyicileri — `internal` olur"
> Sorun: `Order` içsel (`internal`) bir türken `public DbSet<Order>` genel erişimli bir özellik olamaz; C# erişilebilirlik denetimi derleme hatası verir. Bu, kılavuzun kendi örneği ile modül görünürlük kuralının çelişmesidir (4).
> Öneri: Modül varlıkları içsel kalacaksa `DbSet` özelliklerini de `internal` yapın; örneği `internal DbSet<Order> Orders => Set<Order>();` olarak düzeltin.
>
> SONUÇ: 1 BULGU

**(b) Parça 2 (§7–§10.5):**
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Orta
> Yer: §10.1, madde 2
> Alıntı: “Model sonlandırma kuralı her `Guid` birincil anahtarını `ValueGeneratedNever` yapar; böylece kimliği atanmamış varlık sessizce `Guid.Empty` ile kaydedilmez”
> Sorun: `ValueGeneratedNever`, `Guid.Empty` değerini doğrulamaz; EF Core bu değeri üretilmiş değer saymadan PostgreSQL’e gönderebilir. Böylece ilk hatalı kayıt sıfır UUID ile yazılabilir, sonraki kayıtlar çakışabilir. Dokümanın kendi kimlik bütünlüğü güvencesi sağlanmıyor (sınıf 4).
> Öneri: `Added` durumundaki UUID birincil anahtarlarını SQL çalışmadan önce doğrulayan merkezi bir `SaveChanges` interceptorü veya eşdeğer zorunlu koruma ekleyin; `Guid.Empty` için hata verin. `ValueGeneratedNever` yalnız veritabanı üretimini engelleyen ayar olarak açıklansın.
>
> BULGU-2
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: Başlık notu, Aşama 4 park girdileri K-914; §9.2, madde 4
> Alıntı: “IBAN, e-posta, adres ve telefon hiçbir sistem kaydına yazılmaz” / “firmanın ayarındaki değerler (IBAN, iade adresi) `UpdateSettings`'te eski/yeni değerle yazılır”
> Sorun: İşlem izi bir sistem kaydı olduğu hâlde firma IBAN’ı ve iade adresi on yıl saklanacak audit satırlarına açık değer olarak yazdırılıyor. Bu, dokümanın mutlak maskeleme/yazmama kuralıyla çelişir ve hassas banka ile adres verisinin denetim kaydı, yedek ve yönetim paneli erişimlerine yayılmasına yol açar (sınıf 4).
> Öneri: `UpdateSettings` audit ayrıntılarından IBAN ve adres değerlerini çıkarın; yalnız alan adı/değişti bilgisi kaydedilsin. Değer denetimi gerekiyorsa geri döndürülemez bir değişiklik göstergesi kullanın.
>
> SONUÇ: 2 BULGU

**(b) Parça 3 (§10.6–§13):**
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §10.8 madde 2 ve 7
> Alıntı: "`data_protection_keys`'e kimlik sütunlu `INSERT` ek bir dizi yetkisi olmadan çalışır"
> Sorun: PostgreSQL’de identity sütununun varsayılan değerini üreten dizide `nextval` için ayrıca `USAGE` (veya `UPDATE`) yetkisi gerekir; kurucu betik yalnız tablolar için varsayılan yetki veriyor. Bu nedenle uygulama kullanıcısı ilk veri koruma anahtarını veya anahtar dönüşümündeki yeni anahtarı yazamaz. Yeni kurulumda sahtecilik koruması, oturum ve korumalı değerler hata vererek kullanıcıların form/giriş akışlarında takılmasına yol açar (3).
> Öneri: Veri koruma anahtar tablosunu oluşturan göçte, uygulama kullanıcısına yalnız ilgili identity dizisi üzerinde `USAGE` verin; kullanıcı adını mevcut `current_setting('shopfolio.application_user')` kalıbıyla alın. Yetki matrisi testini bu yetkiyi bekleyecek ve gerçek anahtar eklemeyi sınayacak biçimde düzeltin.
>
> SONUÇ: 1 BULGU

**(b) Parça 4 (§14–§16):**
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: Aşama 6’dan park edilen girdiler — K-1162…K-1164 / K-1183…K-1186
> Alıntı: “GET hiçbir kaydı değiştirmez.” / “Google dönüşü (`/giris/google`) GET olduğu hâlde yazar”
> Sorun: Doküman GET istekleri için mutlak yazmama kuralı koyarken Google dönüşünü bunun tersine yazan bir GET olarak tanımlar. İstisna, genel kurala açık ve dar kapsamlı bir istisna olarak tanımlanmadığından uygulayıcı iki çelişen talimattan birini seçmek zorunda kalır; Google giriş akışı bloke olabilir veya genel GET-yazmama güvencesi ihlal edilir. Bu, dokümanın kendi içindeki çelişkidir (4).
> Öneri: Genel kuralı “GET kayıt değiştirmez; yalnız `/giris/google` doğrulanmış akış çerezi, `state` ve `nonce` ile bağlı OAuth dönüşü olarak istisnadır” şeklinde güncelleyin. İstisnanın yalnız hangi kayıtları yazabileceğini ve tek kullanımlık akış çerezinin aynı yanıtta silineceğini aynı maddede belirtin.
>
> SONUÇ: 1 BULGU

**(b) Parça 5 (§17–§25):**
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: Başlık notu / belge alt bilgisi
> Alıntı: "**Versiyon: v0.18**" / "*Shopfolio — Coding Guidelines v0.17*"
> Sorun: Dokümanın sürümü iki farklı yerde farklıdır; bu, hangi sürümün denetlendiği ve kodlama görevlerinde kullanılacağı konusunda doğrudan çelişkidir (sınıf 4).
> Öneri: Alt bilgideki sürümü `v0.18` yapın ya da başlık notunu gerçekten amaçlanan sürüme eşitleyin.
>
> SONUÇ: 1 BULGU

## 3. Bağımsız değerlendirme

Sekiz ham bulgu yedi ayrı bulguya indi: F1 iki çağrıdan (tek parça B-1, parça 3 B-1), öteki altısı birer çağrıdan geldi. Altı ayrı bulgu bir `[dogrulama]` Opus / high doğrulayıcıya verildi (yüksek F1, F3, F5, F6 tek tek; orta F2 ve F4 ayrı bölümlerden olduğu için ayrı ayrı; toplam altı doğrulayıcı); F7 (belge alt bilgisinin sürümü) bir sürüm dizesi karşılaştırmasıdır ve `grep` ile mekanik doğrulandı, ayrı doğrulayıcı açılmadı. Doğrulayıcı girdileri ve hüküm dosyaları `C:\Users\TEKNOD~1\AppData\Local\Temp\claude\c--projects-Shopfolio\2ac3bfe4-1eaa-4636-a2d5-5112be6cc06a\scratchpad\cr09\dr\` altındadır. %100 KABUL/%100 RET tablosu görünmüyor: üç kabul, dört ret.

| # | Bulgu (kaynak) | Karar | Gerekçe | Uygulama |
|---|---|---|---|---|
| F1 | `data_protection_keys` identity dizisi için `USAGE` yetkisi yok (tek parça B-1, parça 3 B-1; §10.8 madde 2–3 ve 7; §15.5 madde 1) | ❌ RET | İddia `serial` için doğru, identity sütunu için yanlış. PostgreSQL 18 kaynağında (REL_18_STABLE) kimlik varsayılanı `NextValueExpr` düğümüdür; `ExecEvalNextValueExpr` → `nextval_internal(seqid, false)` dizi yetkisini denetlemez (`check_permissions=false`); `serial`'ın `nextval('…'::regclass)` işlev çağrısı denetler. Npgsql tamsayı anahtarda varsayılan olarak identity kullanır (`UseSerialColumns` 09'da geçmez). §10.8 madde 7 bu varsayımı ("bilgim tarihli") gerçek PostgreSQL'de çalışan bir entegrasyon testine bağlamış; strateji `serial`'a dönerse test düşer. §15.5 madde 1'de yetkiden söz yoktur (bulgunun yer göstermesi kısmen yanlış). Resmî kılavuzun ilgili sayfaları (`sql-createtable`, `ddl-identity-columns`) konuda sessiz; belirleyici kanıt kaynak koddur. Yeni bilgi yok. | Yok |
| F2 | Geniş `catch` "yalnız beş yerdedir" ↔ §10.7 canlılık denetimi `catch (Exception ex) when (...)` (tek parça B-2; §8.1 madde 6, §10.7 madde 6) | ✅ KABUL | Çelişki gerçek: §10.7 madde 6'nın örneği kendini "§8.1: … geniş catch'in izinli yeri" diye sunar, ama §8.1 madde 6'nın beş yerinin hiçbiri değildir (yakalama katmanı hâle çevirip 500 üretir; canlılık denetimi `false` → 503 üretir; veritabanı §11'in "dış servis"i değildir). Yasağı yineleyen yerler §13.1 madde 10, §23, §25; §23'e göre çelişki task'ı BLOCKED yapar. `07 §4.33.1` "vermiyor"u 503 ister, yani geniş yakalama gereklidir. Mekanizma eki: CA1031 değişkenli ve süzgeçli yakalamayı işaretlemez (`dotnet/sdk` `DoNotCatchGeneralUnlessRethrown`), bu yüzden "çözümleyiciyle zorlanır" güvencesi `when`'li biçimi kaçırırdı. Seviye: ikinci model Orta, doğrulayıcı (4) orta. | K-1384: canlılık ucu altıncı izinli yer olarak §8.1 madde 6'ya yazıldı; "beş yer" anlatımı §8.1 madde 6, madde 9 sonu, §10.7 örneği, §13.1 madde 10, §23 (iki yer), §25'te "altı"ya çevrildi; kaynak taramasının süzgeçli biçimi de yakaladığı yazıldı; dağıtıcı "dördüncü yer" oldu (yan gözlem). Üst doküman etkisi yok |
| F3 | `public DbSet<Order>` ↔ varlıklar `internal` (parça 1 B-1; §5.2 madde 2, §6 madde 1) | ❌ RET | İkinci model veritabanı bağlamının `public` olduğunu varsaydı; §6 madde 1 "veritabanı bağlamı da `internal sealed`dır (`DbSet<Order>` özelliği `internal` varlıkla tutarlı kalır)" der ve bu K-1377 (4) ile deep review'da eklenmiştir; §4.1 tek proje, tek bağlam, modül başına derleme yasak. §5.2'nin örneği sınıf bildirimi taşımayan bir alıntıdır. C# dil belirtimi (Basic concepts §7.4.3 ve §7.4.5): `internal` bir türde `public` bildirilen üyenin erişim alanı türün erişim alanıdır; özelliğin türü (`DbSet<Order>`) de aynı derlemeyle sınırlıdır — CS0053 oluşmaz. `IDataProtectionKeyContext.DataProtectionKeys` özelliği örtük arayüz uygulaması olarak `public` olmak zorundadır; öneri ("hepsi internal") ona çelişirdi. Yeni bilgi değil. | Yok |
| F4 | `ValueGeneratedNever` `Guid.Empty`'yi engellemiyor (parça 2 B-1; §10.1 madde 2) | ✅ KABUL | EF Core belgesi ("No value generation"): değer üretimi yoksa değeri uygulama sağlar; atanmamış değer için doğrulama tanımlı değildir, `Guid.Empty` yalnız üretim açıkken "atanmamış" sayılır. Ayar ters yönde çalışır: EF kendi kimliğini üretmez (cümlenin ikinci yarısı doğru) ama sıfır UUID'yi olduğu gibi gönderir; `uuid` sütunu kabul eder, ilk kayıt sessizce yazılır. Dokümanda başka yakalayıcı yok (`grep` yalnız bu satırı verir); §10.1 madde 5'in testi ayarın varlığını sınar; `06 §2.3.1` ve `06 §4` sıfır kimlik için kısıt koymaz. K-1337 (2) yalnız "her `Guid` anahtarı `ValueGeneratedNever`" der; "böylece…" gerekçesi yazımda eklenmiştir. Doğrulayıcı seviyeyi düşük okudu (tek sessiz satır, para/hak kaybı yok; ikinci kayıt yüksek sesle düşer); sınıf (4). | K-1384: §10.1 madde 2'ye `Infrastructure/EmptyKeyGuard` (`SaveChangesInterceptor`, `Added` durumunda boş `Guid` → SQL'den önce `InvalidOperationException`) yazıldı; olay dağıtımı ve önbellek atımının ona bağlanmadığı belirtildi; birincil anahtar testi (madde 5) kaydın hata verdiğini de sınar. Üst doküman etkisi yok |
| F5 | IBAN ve iade adresi işlem izine açık yazılıyor ↔ "hiçbir sistem kaydına yazılmaz" (parça 2 B-2; başlık notu K-914; §9.2 madde 4) | ❌ RET | İki cümle aynı kapsamda değil. "Sistem kaydı" tanımlıdır: `05 §9.1.1` (K-915) üç kaynak sayar (erişim kaydı, uygulama kaydı, sunucunun kayıtları) ve hepsi sunucu diskindeki günlük dosyalardır; başlık notu yasağı `(05 §9.1)` kaynağıyla verir, `05 §9.1.5` ve `§6.8.3` aynı kapsamla yazar. İşlem izi ayrı bir kayıttır (veritabanı `AuditLog` tablosu; `05 §4.7`, `06 §4.63`); `02 §10.3.3` iz, imha kaydı ve sistem kayıtları (Z-40) ayrımını açıkça yapar. Ayarın eski ve yeni değeri üründür: `02 §10.3.1` (6) "IBAN, iade adresi … eski ve yeni değerle" (K-414, K-523; gerekçe: ele geçirilmiş hesap IBAN'ı değiştirip geri alırsa iz kalmalı). Aynı iz satırı yönetici e-postasını da değer olarak taşır (`06 §4.63`, K-975) — yasağın izi kapsamadığının ayrı kanıtı. Önerinin "yalnız değişti bilgisi" yolu K-523'ün amacını ortadan kaldırır, üst dokümanla çelişir. | Yok (isteğe bağlı "(işlem izi değil)" netleştirmesi üslup düzeyindedir, bulgu değil) |
| F6 | "GET hiçbir kaydı değiştirmez" ↔ "Google dönüşü GET olduğu hâlde yazar" (parça 4 B-1; Aşama 6 park girdileri) | ❌ RET | İki cümle ayrı park satırlarındadır (satır 52: K-1162…K-1164, kaynak `07 §2`; satır 58: K-1183, K-1184, K-1186, kaynak `07 §2.2.3`, §4.20–§4.25). Park blokları talimattır, karar değildir ve yazımda değişmez (başlık notu, K-1302 (6)); önerilen "genel kuralı güncelle" park satırını değiştirmek olurdu. Bağlayıcı kaynak çelişkiyi taşımaz: `07 §2.2.5` kuralı ve kapalı istisna listesini aynı paragrafta yazar (yazabileceği kayıtlar sayılı); `07` matrisi ve UC-51 aynı listeyi yineler. `09` gövdesinde (§1–§25) genel "GET kayıt değiştirmez" cümlesi yoktur; gövdedeki GET kuralları dar kapsamlıdır (§15.1 madde 4 anahtarlı bağlantı sayfaları, §15.7 madde 7 yükleme durumu, §16.2 madde 5 seçim formları) ve Google dönüşüyle çakışmaz; akış çerezinin dönüşte okunup aynı yanıtta silinmesi §15.1 madde 7'dedir. `05 §6.4.7` ve §6.7.1 de Google dönüşünün GET olduğunu ayrı yazar. Yeni bilgi değil. | Yok |
| F7 | Belge sürümü: başlık v0.18, alt bilgi v0.17 (parça 5 B-1; başlık notu ↔ belge sonu) | ✅ KABUL | Mekanik doğrulama: `grep` başlıkta `**Versiyon: v0.18**`, belge sonunda `*Shopfolio — Coding Guidelines v0.17*` (tek geçiş) gösterir; deep review sürümü yükseltirken alt bilgiyi atlamıştır. Gerçek (4): hangi sürümün denetlendiği belirsiz. Yan taramada başka bir alt bilgi/sürüm uyuşmazlığı yok. Seviye: ikinci model Orta, ölçü dışı derecede düşük ama sınıf (4). | K-1384: alt bilgi ve başlık v0.19 (bu turun sürümü); Yazım durumu paragrafı güncellendi |

**Kısmi kalış yok;** üç kabulün her biri ikinci modelin önerisinin kendisinden ya da ona yakın bir biçimden uygulandı (F2'nin doğrulayıcısı önerinin ötesinde CA1031 mekanizma ekini getirdi; F4'ün çözümü ikinci modelin interceptor önerisiyle aynı). Dört ret, doğrulayıcıda kaynakla (PostgreSQL kaynağı, `07 §2.2.5`, `02 §10.3.1`, §6 madde 1) kanıtlandı; "ben böyle düşünüyorum" gerekçesi yoktur.

## 4. Ek bulgular

- **EK-1 (F2'nin doğrulayıcısı):** §13.1 madde 10 sonundaki "(geniş `catch`'in beşinci yeri, §8.1 madde 6)" anlatımı §8.1'in sırasına göre dördüncü yerdir (dağıtıcı); K-1384'te düzeltildi.
- **EK-2 (F1'in doğrulayıcısı):** §10.8 madde 7'deki "(bilgim tarihli)" notu PostgreSQL 18 kaynağıyla doğrulandı; not istenirse kaldırılabilir, ama bulgu değil — test kalmalıdır. Uygulanmadı.
- **EK-3 (F5'in doğrulayıcısı):** başlık notundaki "hiçbir sistem kaydına" ifadesinin yanına "(işlem izi değil)" eklenebilir; üslup düzeyindedir, park bloğu yazımda değişmez kuralı (K-1302 (6)) nedeniyle uygulanmadı.

## 5. Kullanıcı onay checklist'i

- [ ] F1 — RET (uygulanmadı)
- [x] F2 — canlılık ucu altıncı geniş `catch` yeri; süzgeçli biçim kaynak taramasında (K-1384)
- [ ] F3 — RET (uygulanmadı)
- [x] F4 — `EmptyKeyGuard` kesicisi (K-1384)
- [ ] F5 — RET (uygulanmadı)
- [ ] F6 — RET (uygulanmadı)
- [x] F7 — alt bilgi sürümü (K-1384)

Gösterilmeden kaydedilen ⚠ karar yok; çok kritik soru yok.

## Sonuç

**SONUÇ: TEMİZ DEĞİL — 7 ayrı bulgu (8 ham), 3 kabul, 4 ret.** Skill'in çıkış kuralı: bütün çağrılar aynı anda TEMİZ dönmedi (altı çağrının altısı da bulgu getirdi) ve 5. tur eşiği (beşinci turdan itibaren çıkış kuralı) henüz gelmedi; kabul edilen bulgular arasında mevzuat ya da para/hak kaybı sınıfından olan yok (F2 ve F4 sınıf (4) mekanizma güvencesi, F7 sınıf (4) metin farkı). Sonraki tur: 2. tur (`09_CROSS_REVIEW_R2.md`), aynı betikler ve aynı parça sınırlarıyla, `09` v0.19 üzerinde. Etki yansıtma (Faz 5) döngü TEMİZ olunca ayrı adımdır; bu turda yapılmadı.
