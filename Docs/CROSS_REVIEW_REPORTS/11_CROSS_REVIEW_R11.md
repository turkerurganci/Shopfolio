# Cross-Review — 11 Implementation Planı (Tur 11)

**Tarih:** 2026-11-01 · **Hedef:** `Docs/11_IMPLEMENTATION_PLAN.md` v0.50 (girdi commit'i `de509ec`, 762 675 bayt tek parça girdi, 7 760 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma); betiğin `status.txt` satırı: `model=codex gpt-5.6-terra high` · **Ciddiyet ölçüsü:** 1. turdan itibaren, mekanizma okuma paragrafıyla (K-1399; `11_CROSS_REVIEW.md` §1) · **Koşum:** iki koşum — tek parça çağrı ve beş parçalı kontrol koşumu · **Sonuç:** **TEMİZ — çıkış kuralıyla, 11 tur** (`cross-review` skill'i Faz 4 madde 5; K-849, K-976, K-1129, K-1223 kalıbı) — 15 ham bulgu, 14 ayrı bulgu (tek parça B-1 ile parça 1 B-2 aynı iddia); **0 doğru · 1 kısmen · 13 çürük** (on tanesi önceki turlarda reddedilmiş iddianın dönüşü); tek kabul düşük şiddette iki yerin metin farkıdır (T355 k4 ↔ `02 §3.23.3`), mevzuata aykırılık, para ya da hak kaybı, çıkışsız akış yoktur; ⚠ yok.

## 1. Koşum

Betikler, istem, parça notu, şablon ve **parça sınırları 1. turdakinin aynısıdır** (`11_CROSS_REVIEW.md` §1; K-1478 — sınır başlıkla tanımlıdır, değişmez). İstem tek karakter değiştirilmeden kullanıldı. Girdi `build_inputs.py` ile güncel `11`'den kuruldu (`uncovered lines: none`). İkinci model K-1484'ün tek betiğiyle çağrıldı (`cross_review_ikinci_model.sh in out c1 p1 p2 p3 p4 p5`): Codex `gpt-5.6-terra`, yüksek; altı çağrı tek komutta, ilk denemede çıkış 0, kullanım sınırı yok. Tur 10'daki gibi 600 sn aşımı olmadı.

| Çağrı | Bölümler | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|
| Tek parça | tüm doküman | 762 675 bayt | 411 065 | 206 sn | 1 bulgu |
| Parça 1 | §1–§4 · §5 Task yapısı · F0 · F1 (satır 65–866) | 97 524 bayt | 41 456 | 181 sn | 3 bulgu |
| Parça 2 | F2 · F3 T301–T345 (867–2841) | 197 923 bayt | 101 065 | 234 sn | 3 bulgu |
| Parça 3 | F3 T346–T390 · F4 (2842–4219) | 151 795 bayt | 103 406 | 125 sn | 4 bulgu |
| Parça 4 | F5 · F6 · §6 (4220–5633) | 149 815 bayt | 71 726 | 123 sn | 3 bulgu |
| Parça 5 | §7 · §8 · §9 (5634–dosya sonu) | 254 332 bayt | 109 904 | 103 sn | 1 bulgu |

Altı çağrının hepsi bulgu getirdi (parça 5 bu kez TEMİZ dönmedi). İki koşum arasında bir iddia çıktı: tek parça B-1 ile parça 1 B-2 aynı bulgudur (T117 k3 ↔ KK3, `Undelivered`'da gönderim anı); on beş ham bulgu on dört ayrı bulgudur. Tur 10'da anılan T103, T514, T504 ve T370 değişiklikleri bu turda yeniden bulguya konu olmadı.

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**(a) Tek parça çağrı:**

> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Orta
> Yer: §5 / T117, kabul kriteri 3 ve KK3
> Alıntı: "Gönderim anı `Sent`'te dolu, `Queued`'da boştur; `Undelivered`'da yalnız gönderildikten sonra geri dönen satırda doludur" / "gönderim anı dolu da boş da `Undelivered` satır kabul edilir"
> Sorun: Aynı durum hem gönderim anı zorunlu hem de isteğe bağlı tanımlanmıştır. Doğrulama testi, kabul kriterinin yasakladığı kaydı kabul eder; bu planın kabul kriteri–test güvencesiyle çelişir (4). Başarısız gönderim, hiç gönderilmemiş olmasına rağmen “ulaşmadı” sayılabilir ve yeniden deneme/bildirim durumu yanlış ilerleyebilir.
> Öneri: `Undelivered` için tek kural belirlenmeli. Kriterdeki amaç korunacaksa KK3, `SentAt` boş olan `Undelivered` kaydını reddetmeli; buna uygun veritabanı kısıtı ve entegrasyon testi eklenmelidir.
>
> SONUÇ: 1 BULGU

**(b) Parça 1:**

> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §3 F1 çıkış ürünü; T103 kabul kriteri 7; T04 kabul kriteri 5
> Alıntı: "**Çıkış ürünü:** göçler ileri, idempotent ve geri alınabilir" / "Göç tek konudur; `Down` ve işlem dışı adım yoktur"
> Sorun: F1, göçlerin geri alınabilir olmasını ve CI’ın geri alma sınaması yapmasını zorunlu kılıyor; T103 ise göçte `Down` yöntemini yasaklıyor. Geri alma için alternatif, sınanabilir bir mekanizma da tanımlanmamış. Bu, F1 gate’inin aynı anda sağlanamamasına ve hatalı veri göçünün güvenilir biçimde geri alınamamasına yol açar. (4), ayrıca üretimde veri kaybı riski nedeniyle (2).
> Öneri: Tek bir geri alma yaklaşımını açıkça seçip F1, T04 ve tüm göç task’larıyla hizalayın: ya sınanan `Down` işlemlerini zorunlu kılın ya da geri almayı yedekten/ileri düzeltme göçünden yapacak somut prosedürü, CI sınamasını ve kabul kriterlerini yazın.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: T117 kabul kriteri 3; T117 KK3
> Alıntı: "`Undelivered`'da yalnız gönderildikten sonra geri dönen satırda doludur" / "`gönderim anı dolu da boş da `Undelivered` satır kabul edilir`"
> Sorun: Kabul kriteri, `Undelivered` kaydının gönderimden sonra geri dönen bir kayıt olmasını öngörürken doğrulama listesi gönderim anı olmayan `Undelivered` kaydını kabul ediyor. Bu, gönderilmemiş bir bildirimin teslim edilemedi diye kayda alınmasına ve bildirim akışının yanlış kapanmasına yol açabilir. (4)
> Öneri: Kaynak kurala göre tek davranışı belirleyin. `Undelivered` yalnız geri dönüş anlamındaysa gönderim anını zorunlu kılın ve KK3’te boş değeri reddedin; gönderim öncesi hatalar da bu durumdaysa ayrı durum tanımlayıp kabul kriterini buna göre düzeltin.
>
> BULGU-3
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: T08, T10
> Alıntı: "`05 §6.x`" / "`05 §6.x.1`" / "`05 §6.x.2`"
> Sorun: Sır düzeni ve veritabanı rol yetkileri için gerekli kaynak bölümleri somut kimlik yerine `§6.x` yer tutucusuyla gösterilmiş. Task ajanı ilgili sır-bileşen tablosunu, sahiplik kurallarını ve rol ayarlarını güvenilir biçimde bulamaz; bu da yanlış sır paylaşımı veya aşırı veritabanı yetkisi riskini doğurur. Dokümanın her task’ın gerekli girdilerini hazır tutma güvencesi sağlanmıyor. (4), kişisel veri ve erişim güvenliği bakımından (2).
> Öneri: T08 ve T10’daki tüm `05 §6.x...` atıflarını gerçek, tekil `05` bölüm kimlikleriyle değiştirin; sır-bileşen matrisi, rol ayarları ve döndürme tablosunun okunacak kaynak birimlerini açıkça belirtin.
>
> SONUÇ: 3 BULGU

**(c) Parça 2:**

> BULGU-1
> Kriter: teknik doğruluk
> Seviye: Yüksek
> Yer: §5 / F3 / T330, kabul kriteri 7
> Alıntı: "ETBİS kaydı yapılmaz; kayıt bilgisi isteğe bağlı alandır, satış kapısını tutmaz..."
> Sorun: Kendi e-ticaret ortamında tüketiciyle sözleşme kuran hizmet sağlayıcının faaliyete başlamadan önce ETBİS’e kayıt olması gerekir. Planın kayıt yapılmamasını ve satış kapısının bundan etkilenmemesini emretmesi yürürlükteki yükümlülüğe aykırıdır; idari para cezası riski doğurur. [Ticaret Bakanlığı güncel SSS](https://ticaret.gov.tr/ic-ticaret/sikca-sorulan-sorular/elektronik-ticaret)
> Öneri: Kaynakla çelişkiyi §8’de GAP olarak kaydedin; ETBİS kaydını yayın öncesi zorunlu dış operasyon olarak ekleyin ve satış/yayın kapısına kayıt kanıtı kontrolü koyun.
>
> BULGU-2
> Kriter: tutarlılık
> Seviye: Orta
> Yer: §5 / F2 / T204 kabul kriteri 4; §5 / F3 / T304 kabul kriteri 6
> Alıntı: "kanal hatası da alıcı hatası da ilk denemeden 5 dk, 30 dk ve 2 saat sonraki deneme anlarını kurar" / "Aşan satır ilk denemede `Undelivered` ve `RecipientError` olur; servise çağrı yapılmaz"
> Sorun: Alıcı sınırını aşan kayıt `RecipientError` olarak sınıflanıyor; fakat aynı sınıf T204’te yeniden denenecek hata olarak tanımlanıyor. Bu, limit nedeniyle ilk denemede gönderilmeyen e-postanın sonraki denemede sağlayıcıya gönderilmesine ve alıcı sınırının fiilen aşılmasına yol açar. Bu doküman içi çelişkidir.
> Öneri: Sınır aşımına yeniden denenmeyen ayrı bir terminal sonuç tanımlayın veya `RecipientError` için sınır-aşımı istisnasını açıkça belirtin; sonraki denemelerde de sağlayıcı çağrısı yapılmadığını test edin.
>
> BULGU-3
> Kriter: tutarlılık
> Seviye: Orta
> Yer: Başlık notu / “Okuma tablosu ve boyut”; §5 / F2 / T201
> Alıntı: "task bloğu en çok 2,5 KB"
> Sorun: T201 bloğu tek başına bu sınırı aşmaktadır. Buna rağmen doküman, sınır aşımında karar kaydı açılmasını zorunlu tutup `11_task_blok_denetle.py --tam` sonucunun sorunsuz olduğunu söylüyor. Bu, ajanların sınırlı görev-okuma güvencesini ve blok denetiminin güvenilirliğini bozan bir doküman içi çelişkidir.
> Öneri: T201’i 2,5 KB altına indirin veya sınır aşımı için karar kaydı açıp blok boyutu kuralını ve denetim sonucunu tutarlı biçimde güncelleyin.
>
> SONUÇ: 3 BULGU

**(d) Parça 3:**

> BULGU-1
> Kriter: teknik doğruluk
> Seviye: Yüksek
> Yer: T368, kabul kriteri 2
> Alıntı: “Düğme, firmaya ulaşmasından otuz gün geçmiş, teslim tarihi girilmemiş, işleme açık adedi kalan fiziksel kalemli siparişte görünür”
> Sorun: Fesih hakkı, taahhüt edilen ifa/teslim süresi kaçırıldığında doğar; otuz gün geçmesi ve teslim tarihi girilmemesi koşullarına bağlanamaz. Bu koşul, taahhüt süresi aşılmış ancak 30 gün dolmamış veya teslim tarihi girilmiş tüketiciyi kanunî fesih yolundan mahrum bırakır. Bu, mevzuata aykırı hak kaybıdır. [Mesafeli Sözleşmeler Yönetmeliği m.16](https://tuketici.ticaret.gov.tr/data/5e81982d13b876a1b04c7a42/2023-6502%20Say%C4%B1l%C4%B1%20T%C3%BCketicinin%20Korunmas%C4%B1%20Hakk%C4%B1nda%20Kanun.pdf)
> Öneri: Fesih düğmesini, siparişin taahhüt edilen ifa süresinin aşılmasıyla açın; 30 gün ve teslim tarihi yokluğu koşullarını kaldırın. Kargodaki siparişler için de bu hakkı koruyun.
>
> BULGU-2
> Kriter: eksiklik
> Seviye: Yüksek
> Yer: T368, kabul kriteri 6
> Alıntı: “Panel fesih satırında ve taahhüt süresi sonrası iptalde kanuni faiz uyarısı gösterir; sistem faiz hesaplamaz”
> Sorun: Süresi içinde ifa edilmemesi nedeniyle fesihte satıcı, tahsil edilen tüm ödemeleri kanunî faiziyle birlikte iade etmek zorundadır. Sadece uyarı gösterip faizi hesaplamayan ya da iade hattına aktarmayan plan, faizli iadenin uygulanmadan kalmasına ve müşterinin parasal hak kaybına yol açar. [Mesafeli Sözleşmeler Yönetmeliği m.16/3](https://tuketici.ticaret.gov.tr/data/5e81982d13b876a1b04c7a42/2023-6502%20Say%C4%B1l%C4%B1%20T%C3%BCketicinin%20Korunmas%C4%B1%20Hakkında%20Kanun.pdf)
> Öneri: T368–T370’ye fesih bildiriminin ulaşmasından iade anına kadar kanunî faiz hesaplama, tutarı geri ödeme kaydına ekleme ve iade kontrolünde sınama kriterleri ekleyin.
>
> BULGU-3
> Kriter: teknik doğruluk
> Seviye: Yüksek
> Yer: T370, kabul kriteri 3
> Alıntı: “teslimden sonraki caymada malın ulaştığı tarihle beyandan geç olanından başlar”
> Sorun: Satıcının caymada iade yükümlülüğü, cayma bildiriminin kendisine ulaştığı tarihten itibaren 14 gündür; plan bunu malın ulaşmasına bağlayarak süreyi tüketici aleyhine uzatıyor. Bu, iadenin gecikmesine ve parasal hak kaybına neden olur. [Mesafeli Sözleşmeler Yönetmeliği m.12](https://tuketici.ticaret.gov.tr/data/5e81982d13b876a1b04c7a42/2023-6502%20Say%C4%B1l%C4%B1%20T%C3%BCketicinin%20Korunmas%C4%B1%20Hakk%C4%B1nda%20Kanun.pdf)
> Öneri: Cayma geri ödemesi için süre başlangıcını cayma bildiriminin ulaşması olarak değiştirin; varsa malın iadesine ilişkin operasyonel kontrolleri bu kanunî süreyi ertelemeyecek şekilde ayrı modelleyin.
>
> BULGU-4
> Kriter: tutarlılık
> Seviye: Orta
> Yer: T355 kabul kriteri 4; T375
> Alıntı: “iki istisna firmanın adres ve misafir e-postası düzeltmesidir, önceki değer geçmişte durur (T375)”
> Sorun: İşaret edilen T375, yalnız teslimat/fatura adresi ve misafir sipariş e-postası düzeltmesini tanımlar; firmanın donmuş adresinin düzeltilmesine ilişkin kabul kriteri, işlem ve test içermez. Böylece T355’in açıkça T375’e bağladığı istisnanın task karşılığı yoktur; donmuş yasal kimlikteki yanlış firma adresi düzeltilemeyebilir. Bu, planın kendi ileri/geri izlenebilirlik güvencesini bozar.
> Öneri: T375’e firmanın siparişe donmuş adresini düzeltme işlemini, eski-yeni değerin korunmasını, etkilenen donmuş form/sözleşme gösterimini ve testini ekleyin; ya da T355’teki istisnayı karşılayan doğru task’a yönlendirin.
>
> SONUÇ: 4 BULGU

**(e) Parça 4:**

> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: T508 kabul kriterleri 2 ve 7
> Alıntı: “Kalem fiyatı ve satın alınabilirliği güncel üründen okunur” / “ilk eklenme fiyatı değişmez”
> Sorun: Aynı sepet kaleminin fiyatı için birbirini dışlayan iki kural konmuş. Bu, sepet toplamı ve ödeme tutarının hangi fiyattan hesaplanacağını belirsiz bırakır; müşteriye veya firmaya parasal kayıp doğurabilir.
> Öneri: Kaynak kuralı esas alınarak tek fiyat davranışını belirtin; diğer kriteri, kontrol listesini ve ilgili testleri aynı davranışa hizalayın.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: T539 kabul kriteri 1; T548 kabul kriterleri 3–4
> Alıntı: “betikli yol ayrı uç çağırmaz” / “Başlatma `POST /panel/yukleme` ile…” / “Parça … `POST /panel/yukleme/{id}`'e gider”
> Sorun: T539, JavaScript yolunda ayrı uç çağrılmasını yasaklarken T548, parçalı yüklemenin ayrı başlatma ve parça uçlarıyla yapılmasını zorunlu tutuyor. İki task aynı anda karşılanamaz; zorunlu dijital dosya yükleme akışı uygulanmadan kalır.
> Öneri: T539’daki yasağı standart form akışlarıyla sınırlayın ve T548 parçalı yükleme uçlarını açık istisna olarak ekleyin; doğrulama kontrol listesini de bu istisnayı denetleyecek biçimde güncelleyin.
>
> BULGU-3
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: T540 kabul kriteri 4; T510 kabul kriterleri 3–4
> Alıntı: “ölçüm, dış betik ve gömme içerik yoktur” / “sağlayıcı formu çerçevede açılır”
> Sorun: Ödeme sağlayıcısının çerçevede açılması gömülü dış içeriktir; T540 bunu tamamen yasaklıyor. Bu çelişki, kartla ödeme akışının ya da tasarım tabanının kabul kriterlerini karşılamasını imkânsız kılar.
> Öneri: T540’a, yalnız T510’daki ödeme sağlayıcısı çerçevesinin açık ve dar kapsamlı istisna olduğunu ekleyin; izinli kaynak ve ilgili çerçeve politikalarını T510’daki kısıtlarla bağlayın.
>
> SONUÇ: 3 BULGU

**(f) Parça 5:**

> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §7.1 Aday task tablosu T549; §7.1.2 UC-04; §7.2 T549
> Alıntı: "| T549 | F5 | Vitrin: arşivlenmiş ürün sayfası (E-05) — "bu ürün artık satılmıyor" bilgisi, fiyatsız gösterim (KP-7, `02 §3.7`, §5.1) | Katalog | `04` E-05; `07` UC-04 (K-1437) |" / "| 07 UC-04 GET `/urun/{Slug}` — Ürün adresi | T503 | | |" / "| T549 | `04` E-05; `02` §3.7.6; `10` KP-7 | ✓ 3 kaynak (özgün 3, alt bölüm 0, devir 0) |"
> Sorun: T549 aday tablosunda UC-04'e dayanıyor; ancak ileri matriste UC-04 yalnız T503'e, geri matriste de T549 UC-04 dışarıda bırakılarak bağlanmış. Bu, çift yönlü izlenebilirlik güvencesini bozar (4); T549’un aynı ürün adresindeki arşiv davranışına ilişkin API girdisi gate denetiminden kaçabilir.
> Öneri: `07 UC-04` ileri satırına T549’u ekleyin; T549’un §7.2 satırına UC-04’ü ekleyin ve bağ/toplam sayılarını yeniden üretip güncelleyin.
>
> SONUÇ: 1 BULGU

## 3. Bağımsız değerlendirme

Ham bulgular numaralandı: tek parça B-1 (F2, parça 1 B-2 ile aynı), parça 1 B-1…B-3 (F1–F3), parça 2 B-1…B-3 (F4–F6), parça 3 B-1…B-4 (F7–F10), parça 4 B-1…B-3 (F11–F13), parça 5 B-1 (F14); 15 ham, 14 ayrı bulgu. On iki `[dogrulama]` Opus / high doğrulayıcıya verildi (bekçi sınırı 20; bekçi olayı yok): Yüksek bulgular F1, F2, F3, F4, F7, F8, F9, F11, F12, F13 **tek tek** (D1, D2, D3, D4, D7, D8, D9, D11, D12, D13); Orta bulgular **ikili gruplarda** (D5+D6: F5 ve F6; D10+D14: F10 ve F14). Önceki turlarda reddedilmiş olabilecek bulgularda paketle birlikte önceki hükümlerin ilgili bölümleri verildi ve talimata "yeni bilgi yoksa kısa gerekçeyle tekrar eden RET" kuralı yazıldı. Yasal iddia taşıyan F4, F7, F8, F9 resmî metne (Mesafeli Sözleşmeler Yönetmeliği, RG 27/11/2014-29188, değişik 23/8/2022-31932; 6502 sayılı Kanun) karşı okundu; F7 ve F9 doğrulayıcıları metni mevzuat.gov.tr / Resmî Gazete'den bu turda yeniden okudu, F4 ve F8 doğrulayıcıları önceki turlardaki resmî metin okumasına ve üst doküman kararlarına dayandı (üst dokümanlar o okumalardan sonra değişmedi).

| # | Bulgu (kaynak) | Karar | Gerekçe | Uygulama |
|---|---|---|---|---|
| F1 | F1 çıkışı ve T04 k5 "göçler geri alınabilir" ↔ T103 k7 `Down` yasağı (parça 1 B-1; F1, T04, T103) | ❌ RET (tekrar eden: 2. tur F1, 3. tur F1–F2, 4. tur F1, 6. tur #1–#2, 7. tur #1, 8. tur #6, 9. tur #3, 10. tur F1; Yüksek → Düşük) | Yeni bilgi yok; `05` ve `09` son değişikliği #266'dır, 11:106, 11:257, 11:372, 11:469, 11:5588 değişmedi. Projede "geri alma" şemayı `Down` ile geri almak değil önceki imaja dönmektir ve CI göç adımında sınanır (`05` §8.7.3 MK-74, 05:1132; `09` §10.6 madde 1, 09:1031 "`Down` yöntemi silinir — şema geri alınmaz"). Önerinin "sınanan `Down`" kolu bu kurallarla çelişirdi; "ileri düzeltme / yedekten dönüş" kolu planda zaten var (11:259 `migration-fix` istisnası, 11:5589 yedekten dönüş). | Yok |
| F2 | T117 k3 "`Undelivered`'da gönderim anı yalnız gönderildikten sonra geri dönen satırda dolu" ↔ KK3 "dolu da boş da kabul edilir" (tek parça B-1, parça 1 B-2; T117) | ❌ RET (tekrar eden: 1., 3., 5., 6., 7., 8. tur; Yüksek → Düşük) | Yeni bilgi yok; metin `de509ec`'te değişmedi (11:781, 11:789). `06` §4.58 Kısıtlar (06:4420) ve §5 kısıt özeti (06:5727) `SentAt`'i iki hal için açık bırakır: P-38'in yeniden denemeleri de başarısız kalıp hiç gönderilmemiş satırda boş, gönderildikten sonra geri dönen satırda dolu (06:4388; K-1120). KK3 her iki hali kabul eder; kriter ve kaynakla birebir, çelişki yok. Önerinin zorunlu kılması `06` §4.58'e aykırı olurdu. | Yok |
| F3 | T08, T10 "`05 §6.x…`" atıfları yer tutucu (parça 1 B-3; T08, T10) | ❌ RET (tekrar eden: 2. tur D4 #5; Yüksek → Düşük) | Yeni bilgi yok. `6.x` yer tutucu değildir, `05`'in gerçek başlık numarasıdır (05:821 `### 6.x Secret yönetimi (bu alt bölüm zorunludur)`); T08/T10'un andığı alt kimliklerin hepsi `05`'te tanımlıdır: 6.x.1 (05:827), 6.x.2 tablosu (05:829), 6.x.3–6.x.7, 6.x.18, 6.x.19 (05:833-839), 6.x.8 (05:841), 6.x.9 (05:843), 6.x.12 (05:849), 6.x.16 (05:856); `11` bu kimlik biçimini kendisi kayda geçirir (11:5678, 11:7268). Atıf çözümlenir. | Yok |
| F4 | T330 k7 "ETBİS kaydı yapılmaz": satış kapısı kayda bağlanmalı (parça 2 B-1; T330) | ❌ RET (tekrar eden: 2. tur F6, 3. tur F3, 4. tur F1, 6. tur #2, 8. tur #7, 9. tur #7; Yüksek → Düşük) | Yeni bilgi yok: kaynak yine Ticaret Bakanlığı ETBİS SSS sayfası, `02` ve `10` son kez #265'te değişti. "ETBİS kaydı yapılmaz" ürünün kayıt yapmadığını söyler; kayıt firmanın yükümlülüğüdür, ürün kaydın varlığını denetleyemez, alan satış kapısını ve kurulum listesini tutmaz (`02` §3.1.4, §12.1.4, §12.5; K-574, K-628; `10` ÖK-5). T330 k7 (11:2442) kaynak cümleyle birebirdir; firmanın ön koşulu T601 k9'da (11:5562, ÖK-1…ÖK-12) runbook'a bağlıdır. Satışı kayda bağlamak `02` §3.1.4'e aykırı olurdu. Doğrulayıcı mevzuat metnini bu turda yeniden açmadı; yükümlülük tartışmasız ve `10` ÖK-5 Tebliğ m.5/1-a'ya dayanarak kabul eder. | Yok |
| F5 | T204 k4 alıcı hatası da kanal hatası da yeniden denenir ↔ T304 k6 kota aşımı `RecipientError` (parça 2 B-2; T204, T304) | ❌ RET (tekrar eden: 10. tur F3; Orta → Düşük) | Yeni bilgi yok (`08` son değişikliği #267, tur 10'dan önce). Alıntı T204 k4'ü yarıda keser; cümlenin devamı "yeniden denenmeyen sınır aşımı T385'tedir" der (11:956), T385 k4 "yeniden denenmeden" der (11:3810). Kaynakta aşan satır ilk denemede `Undelivered` olur ve servise çağrı yapılmaz (08:452); `Undelivered` son haldir (08:410); "yeniden denemelerde sınır sorulmaz" (08:456) sınırdan geçmiş satırlara aittir. T304 k6/k9/KK6 (11:1766, 1769, 1777) karşılığıdır. | Yok |
| F6 | T201 bloğu 2,5 KB sınırını aşıyor (parça 2 B-3; T201) | ❌ RET (olgu yanlış; benzer iddialar 3. tur D8, 5. tur D8, 6. tur D6; Orta → Düşük) | Doğrulayıcı T201 bloğunu denetim betiğinin yöntemiyle ölçtü: 2 551 bayt; sınır 2 560 bayttır (2,5 KB = 2,5 × 1024; `11_task_blok_denetle.py` `MAKS_BAYT = 2560`, K-1406 (4)) ve `--tam` `de509ec`'te "TOPLAM SORUN: 0" verir. Sınır aşılmadığı için "karar kaydında konu açılır" koşulu doğmaz; 11:34 ve 11:169 betikle aynı ölçüyü söyler. | Yok |
| F7 | T368 k2 fesih düğmesi "otuz gün geçmiş, teslim tarihi girilmemiş" koşuluna bağlı (parça 3 B-1; T368) | ❌ RET (tekrar eden: 5. tur F3, 7. tur #6; Yüksek → Düşük) | Yeni bilgi yok; T368 k2 (11:3355) `02` §7.2.3'ün (02:1624) düğme cümlesinin türetimidir (aynı koşul 02:1129 Z-11, K-499). Resmî metin bu turda mevzuat.gov.tr'den okundu (Mesafeli Sözleşmeler Yönetmeliği RG 27/11/2014-29188, değişik 23/8/2022-31932): m.16/1 "taahhüt ettiği süre içinde edimini yerine getirmek zorundadır … mal satışlarında bu süre her hâlükârda otuz günü geçemez", m.16/2 yerine getirilmezse fesih; yani otuz gün `11`'in uydurduğu kısıt değil yasal üst sınırdır. Firmanın taahhüdü kargoya verme süresidir (02:825 §3.20.4); aşıldığında fesih T368 k1'in iptal düğmesiyle kullanılır (11:3354), kargodaki sipariş için yol k1'in ikinci yarısı ve k2'dir. | Yok |
| F8 | T368 k6 "sistem faiz hesaplamaz": yasal faizli iade (parça 3 B-2; T368) | ❌ RET (tekrar eden: 2. tur F9, 3. tur F5, 4. tur F1, 6. tur #6, 7. tur #7, 9. tur #12, 10. tur F6; Yüksek → Düşük) | Yeni bilgi yok; `02`, `04`, `06`, `07` son değişikliği #266'dır. T368 k6 (11:3359) `02` §7.2.3'ün (02:1624) aynısıdır; `02` §12.5 tablosu faizi hesaplayıp ödemeyi firmaya yazar (02:2212; K-499, K-571); `06` "faiz alanı yok" (06:1735). m.16/3 faizli iadeyi satıcıya (firmaya) yükler, ürün araçtır. İkinci modelin kaynağı 6502 sayılı Kanun PDF'idir, Yönetmelik metni değildir. Doğrulayıcı resmî metni bu turda yeniden açmadı; 9. turdaki mevzuat.gov.tr okumasına ve K-499'a dayandı. | Yok |
| F9 | T370 k3 teslimden sonraki caymada iade süresi malın ulaştığı tarihle beyandan geç olanından başlar (parça 3 B-3; T370) | ❌ RET (tekrar eden: 2. tur D2 #2, 4. tur D3 #1, 5. tur D6 #6, 10. tur D4 #4; Yüksek → Düşük) | Yeni bilgi yok. Resmî metin bu turda yeniden okundu (RG 23/8/2022-31932 Değişiklik Yönetmeliği m.9 ile değişen m.12/1, resmigazete.gov.tr): süre iade için ön bilgilendirmede belirtilen taşıyıcıya teslim tarihinden, başka taşıyıcıyla iadede malın satıcıya ulaştığı tarihten başlar; "bildirimin ulaştığı tarih" kuralı yalnız teslimden önceki cayma (m.12/2) içindir ve 11:3406 bunu zaten beyandan işletir. Firma iade taşıyıcısı belirlemez; bu yüzden süre ulaşma tarihinden işler (`02` §7.4.1, §7.4.2, §7.4.4; K-293, K-491, K-492, K-493, K-554). 11:3406 kaynak cümleyi aynen tüketir. Bulgunun 6502 PDF'i türetilmiş kaynaktır. | Yok |
| F10 | T355 k4 "iki istisna firmanın adres ve misafir e-postası düzeltmesidir (T375)": firmanın donmuş adresini düzeltme task'ı yok (parça 3 B-4; T355, T375) | KISMİ (Orta → Düşük) | Bulgunun öncülü yanlış, önerisi `02`'ye aykırı: `02` §3.23.3'ün "iki istisna"sı firmanın yaptığı iki düzeltmedir — teslimat/fatura adresi ve misafir e-postası; firma kimliğindeki sonraki değişiklik geçmiş siparişe dokunmaz (02:884; K-113, K-1007, K-585). T375 bu ikisini tam karşılar (11:3542-3547); T375'e donmuş firma adresini düzeltme eklemek kuralı bozardı. Ancak 11:3057'deki ifade, hemen önündeki "firmanın o günkü yasal kimliğini" ögesiyle birlikte "firmanın adresi" diye okunabiliyor (ikinci model tam bu okumayı yaptı) ve bu okuma `02` §3.23.3 ve T375 ile çelişir (4). Düzeltme yalnız ifadeyi `02`'nin cümlesine hizalamaktır. Blok 2 255 → 2 308 bayt. | T355 k4 (1 şartname) |
| F11 | T508 k2 kalem fiyatı güncel üründen okunur ↔ k7 "ilk eklenme fiyatı değişmez" (parça 4 B-1; T508) | ❌ RET (tekrar eden: 2. tur F10, 7. tur #9, 9. tur #14; Yüksek → Düşük) | Yeni bilgi yok; KK2 (11:4407) ve KK7 (11:4412) aynı, kaynaklar değişmedi. İki cümle `02` §3.16.5'in iki ayrı kavramını anlatır: kalem fiyatı güncel üründen okunur (K-80); "ilk eklenme fiyatı" yalnız "fiyatı değişti" işaretinin referansıdır ve adet değişiminde değişmez (K-131; 06:3386 `FirstAddedUnitPrice`; 07:4333 UC-20; 11:2850 T346 KK5 bu rolü tanımlar). Ödeme tutarı referans fiyattan değil güncel üründen ve sipariş anında donan kalemden gelir (02:273). | Yok |
| F12 | T539 k1 "betikli yol ayrı uç çağırmaz" ↔ T548 k3–4 parçalı yükleme uçları (parça 4 B-2; T539, T548) | ❌ RET (yeni iddia, baştan sınandı; Yüksek → Düşük) | Çelişki yok: T539 k1 betikli yolun ayrı uç çağırmasını yasaklar, aynı task'ın 9. maddesi parçalı dijital dosya yüklemesini açıkça "tek istisna" sayar (11:5266) ve KK9 bu istisnayı denetler (11:5278). Yapı kaynağın aynısıdır: `07` (07:5419) önce aynı kuralı koyar, aynı paragrafta "Tek istisna parçalı dijital dosya yüklemesidir" der (K-1171). T548, T539'a bağımlıdır ve uçlarını `07 §4.31.2` ile §5.4.2'den alır; iki task birlikte karşılanabilir. | Yok |
| F13 | T540 k4 "ölçüm, dış betik ve gömme içerik yoktur" ↔ T510 k3–4 sağlayıcı formu çerçevede açılır (parça 4 B-3; T540, T510) | ❌ RET (daha önce 1. tur D2 #5 yan yüzünde karara bağlanmıştı; Yüksek → Düşük) | Yeni bilgi yok. T540 k4 (11:5289) parantezle `02 §8.3.3`'e işaret eder; o kaynak "gömme içerik"i gömülü harita, video, sohbet aracı ve captcha olarak tanımlar ve aynı paragrafta "tek istisna ödeme sağlayıcısının çerçevesidir" der (02:1771); `02 §8.3.4` "Çerçeve elenmez" der (02:1773), `05 §6.5.4` `frame-src`'yi yalnız ödeme sayfasında açar (05:740). İstisna işaret edilen kaynak cümlenin içindedir (K-1445, ayrıntı kopyalanmaz). Mutlak lafzin çerçeveyi gerçekten kırabileceği tek yer CSP kuralı olan T209 k7 ve T224 KK9'dur ve ikisi de istisnayı taşır (11:1083, 11:1466). T540 tasarım tabanıdır; T510 ona bağımlıdır (11:4459). | Yok |
| F14 | T549 aday tablosunda UC-04'e dayanıyor, ileri matriste UC-04 yalnız T503'e, geri matriste T549'da UC-04 yok (parça 5 B-1; T549) | ❌ RET (tekrar eden: 1. tur F9, 4. tur F4, 5. tur F12; Orta → Düşük) | Yeni bilgi yok. UC-04'ün yalnız T503'e bağlı kalması K-1437 (3)'ün bilinçli kararıdır (11:5906, 11:6084, 11:6325, 11:7719; `PRODUCT_DISCOVERY_STATUS` K-1437). `11_matris_kapsama.py` bu turda (`de509ec`) iki yönün birebir eşit olduğunu ve toplam sorunun 0 olduğunu verir. | Yok |

**Tekrar eden RET (turun sonucunu belirlemez):** F1 (2., 3., 4., 6., 7., 8., 9., 10. tur), F2 (1., 3., 5., 6., 7., 8. tur), F3 (2. tur), F4 (2., 3., 4., 6., 8., 9. tur), F5 (10. tur), F7 (5., 7. tur), F8 (2., 3., 4., 6., 7., 9., 10. tur), F9 (2., 4., 5., 10. tur), F11 (2., 7., 9. tur), F14 (1., 4., 5. tur). **Seviye düşenler:** hepsi Düşük'e: Yüksek → Düşük F1, F2, F3, F4, F7, F8, F9, F11, F12, F13; Orta → Düşük F5, F6, F10, F14. **Kısmi kalış:** F10 (öncül çürük, öneri reddedildi; yalnız T355 k4'ün "firmanın adresi" diye okunabilen ifadesi kabul).

**Çıkış değerlendirmesi (Faz 4 madde 5).** Kural koşulları sağlanır: (i) tur beşincinin ardındadır (11.); (ii) kabul edilen bulgunun tamamı (tek: F10, KISMİ) **düşük şiddette iki yerin metin farkıdır** — `11` T355 k4'ün ifadesi ↔ `02` §3.23.3 / T375; mevzuata aykırılık, para ya da hak kaybı, çıkışsız akış değildir (doğrulayıcı seviyeyi Düşük'e indirdi; hak kaybı yok, `02` ve T375 kuralı doğru karşılar); (iii) on dört ayrı bulgunun on üçü ret (on tanesi tekrar eden RET, biri olgu yanlışı, ikisi baştan sınanıp çürük). **Aynı kalıp dokümanın tamamında tarandı:** "iki istisna" ve "firmanın adres" ifadeleri `11`'de `grep` ile arandı — 11:1156 (409 istisnaları, başka kavram), 11:5372 (IBAN alanı istisnaları, başka kavram), 11:3057 (düzeltilen yer); aynı sözü "firmanın adresi düzeltmesi" diye okutabilecek ikinci bir yer yok. Kabulün uygulanmasından sonra tur **kapanır**; sonuç "TEMİZ — çıkış kuralıyla, 11 tur" diye yazılır (K-1492). Seçenekler: (a) 12. tur koşmak; (b) çıkış kuralıyla kapatmak. **Öneri (b)** — (toplu onayla kaydedildi): tek kabul Düşük ifade farkı, ardışık iki turda (10., 11.) kabul edilen Orta bulgu yok (10. tur F7 Orta tek, bu tur sıfır), tur 11'de on üç ret; bir tur daha altı çağrı (~0,84 M token) ve yeniden dönen ret demektir. **Kalan risk (devredilen sınıf):** "task bloğunda kabul kriteri ya da KK'nin, kaynak cümlenin yanlış okunabilen kısaltması" — 10. turun F8'i (T504 k4 ↔ `04` 2.2.9), 8.–9. turların KK farkları ve bu turun F10'u örnektir; `checkpoint` skill'inin iç tutarlılık merceği (Aşama 9 checkpoint'i) bu sınıfı dokümanın tamamında arar, açık süreç maddesine karar kaydı §22.1'de yazıldı. Hedefli mercek (Faz 4 madde 6) gerekmez: ardışık iki turda aynı sınıftan Orta kabul yok.

Yazım kapanışı, matris kapsama, task blok denetimi (`--tam`), geri yön denetimi ve geri işaret taraması sorun 0; T355 2 308 bayt (sınır 2 560).

## 4. Ek bulgular

- Doğrulayıcılar yeni bir **üst doküman** uyumsuzluğu bildirmedi. Önceki turlardan taşınan KD-etki notları (uygulanmadı; 11 tüketicidir): `04` 2.7.3.2 / `05` §10.3.2 / `07` §4.34.2 hata sayfası cümlesinin çerçevesiz panel ekranlarını (`04` 2.2.9: E-29, E-30, E-54) anması (K-1491); `02` §7.3.4 ve `06` `Perishable` bölme kararı hizalaması (K-1483). **KD-etki** (cross-review Faz 5) ayrı adımda karşılar.
- `00`/`CLAUDE.md`/`SETUP.md` önerisi yok.

## 5. Kullanıcı onay checklist'i

- [ ] F1 — RET, tekrar eden (uygulanmadı)
- [ ] F2 — RET, tekrar eden (uygulanmadı)
- [ ] F3 — RET, tekrar eden (uygulanmadı)
- [ ] F4 — RET, tekrar eden (uygulanmadı)
- [ ] F5 — RET, tekrar eden (uygulanmadı)
- [ ] F6 — RET, olgu yanlış (uygulanmadı)
- [ ] F7 — RET, tekrar eden (uygulanmadı)
- [ ] F8 — RET, tekrar eden (uygulanmadı)
- [ ] F9 — RET, tekrar eden (uygulanmadı)
- [ ] F10 — KISMİ (uygulandı: T355 k4)
- [ ] F11 — RET, tekrar eden (uygulanmadı)
- [ ] F12 — RET (uygulanmadı)
- [ ] F13 — RET (uygulanmadı)
- [ ] F14 — RET, tekrar eden (uygulanmadı)
- [ ] Çıkış kuralıyla kapanış — "TEMİZ — çıkış kuralıyla, 11 tur" (toplu onayla kaydedildi)

⚠ karar: yok. Önerilen `00`/`CLAUDE.md`/`SETUP.md` değişikliği yok.
