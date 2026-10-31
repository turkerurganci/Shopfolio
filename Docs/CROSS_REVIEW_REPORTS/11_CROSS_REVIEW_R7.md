# Cross-Review — 11 Implementation Planı (Tur 7)

**Tarih:** 2026-10-31 · **Hedef:** `Docs/11_IMPLEMENTATION_PLAN.md` v0.44 (girdi commit'i `c57b4d8`, 759 922 bayt girdi, 7 760 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma); betiğin `status.txt` satırı: `model=codex gpt-5.6-terra high` · **Ciddiyet ölçüsü:** 1. turdan itibaren, mekanizma okuma paragrafıyla (K-1399; `11_CROSS_REVIEW.md` §1) · **Koşum:** iki koşum — tek parça çağrı ve beş parçalı kontrol koşumu · **Sonuç:** **TEMİZ DEĞİL** — 13 ham bulgu, 12 ayrı bulgu; 0 doğru · 2 kısmen · 10 çürük (yedisi önceki turlarda reddedilmiş iddianın dönüşü); 7 şartname çifti uygulandı (T119 k8, T201 k5 ve k9, T302 k9 ve KK9, T366 başlık ve KK3; v0.44 → v0.45); kabullerden biri Orta (F3: imha dosya parçası, kriter kapsamı ↔ kaynak karar), Faz 4 madde 5 çıkış kuralı uygulanamaz; tur 8 gerekir.

## 1. Koşum

Betikler, istem, parça notu, şablon ve **parça sınırları 1. turdakinin aynısıdır** (`11_CROSS_REVIEW.md` §1; K-1478 — sınır başlıkla tanımlıdır, değişmez). İstem tek karakter değiştirilmeden kullanıldı. Girdi `build_inputs.py` ile kuruldu (`uncovered lines: none`). İkinci model çağrısı 6. turdaki gibi tek betikle yapıldı (K-1484): `bash scripts/doc-scans/cross_review_ikinci_model.sh in out c1 p1 p2 p3 p4 p5`; betik asıl modeli (Codex `gpt-5.6-terra`, yüksek) sağlık çağrısıyla seçti (`used: codex gpt-5.6-terra`), altı çağrının hepsi aynı modelle, tek komutta, ilk denemede `rc=0` döndü (çıkış kodu 0; çıktı dosyaları ayrıca okundu). Yedek model (Cursor) devreye girmedi, tur bölünmedi, kullanım sınırı dolmadı. Çalışma dizini: `C:\Users\teknodeveloper\.claude\helper-state\shopfolio\kd-cross\r7\` (girdiler `in/`, ham çıktılar `out/`, doğrulayıcı paketleri `dogrulayici_<k>.md`, hükümler `hukum_D<k>.md`, talimat `talimat_dogrulayici.md`; paket betiği `betik/paket7.py`).

| Çağrı | Bölümler | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|
| Tek parça | tüm doküman | 759 922 bayt | 403 160 | 266 sn | 2 bulgu |
| Parça 1 | §1–§4 · §5 Task yapısı · F0 · F1 (satır 65–866) | 96 603 bayt | 49 291 | 156 sn | 3 bulgu |
| Parça 2 | F2 · F3 T301–T345 (867–2841) | 197 089 bayt | 115 100 | 300 sn | 4 bulgu |
| Parça 3 | F3 T346–T390 · F4 (2842–4219) | 150 962 bayt | 115 467 | 280 sn | 3 bulgu |
| Parça 4 | F5 · F6 · §6 (4220–5633) | 149 650 bayt | 81 864 | 60 sn | 1 bulgu |
| Parça 5 | §7 · §8 · §9 (5634–dosya sonu) | 254 332 bayt | 130 064 | 44 sn | TEMİZ |

Parça 5 TEMİZ döndü; öteki beş çağrı bulgu getirdi. İki koşum bu turda da sürdü; iki koşum arasındaki örtüşme: tek parça B-2 = parça 1 B-2 (T117 KK3). Tek parça B-1 (geri alma/`Down`) bu kez parçalı koşumda çıkmadı.

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**(a) Tek parça çağrı:**

> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §3 F1; T04 KK5; T103–T119
> Alıntı: "Çıkış ürünü: göçler ileri, idempotent ve geri alınabilir"
> Sorun: F1 geri alınabilir göçleri ve T04 geri alma sınamasını zorunlu kılıyor; buna karşılık veri göçü task’ları `Down` yöntemini açıkça yasaklıyor. Geri alma mekanizması ayrıca tanımlanmadığından, plan kendi faz çıkışını ve CI güvencesini sağlayamaz. Bu, kabul kriteri–test beklentisi çelişkisidir (4).
> Öneri: Forward-only göç kararını koruyarak F1 ve T04’ü “önceki uygulama sürümü yeni şemayla çalışır” geri alma modeliyle somutlaştırın; `Down`/şema geri alma sınamasını kaldırıp bunun yerine uyumluluk ve ileri düzeltme göçü testlerini yazın. Alternatif olarak `Down` yasağını kaldırıp gerçek geri alma task ve testlerini ekleyin.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: T117, Kabul kriteri 3; doğrulama kontrol listesi KK3
> Alıntı: "Gönderim anı `Sent`'te dolu, `Queued`'da boştur; `Undelivered`'da yalnız gönderildikten sonra geri dönen satırda doludur"
> Sorun: Aynı task’ın KK3 maddesi, gönderim anı “dolu da boş da” olan `Undelivered` satırını kabul ediyor. Böylece gönderilmemiş bir bildirim teslim edilemedi diye kaydedilebilir. Kabul kriteri ile test birbirini çürüttüğü için teslimat kaydı güvencesi sağlanmaz (4).
> Öneri: KK3’ü, `Undelivered` için gönderim anını zorunlu kılacak şekilde düzeltin; gönderim anı boş `Undelivered` kaydını reddeden veritabanı/servis ve entegrasyon testi ekleyin.
>
> SONUÇ: 2 BULGU

**(b) Parça 1 (§1–§4 · §5 Task yapısı · F0 · F1):**

> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §5, T119 kabul kriteri 8
> Alıntı: “üretilen iki yasal metnin (`PreInformationForm`, `DistanceSalesContract`) ilk sürümü uygulamanın açılışında yazılır, işlem izine girmez (yazımı T366)”
> Sorun: T119, F1’dedir; T366 ise F3’tedir. F1 gate’i geçilmeden F3 başlayamaz ve task bağımlılıkları yalnız aynı fazdaki task’ları gösterebilir. Bu nedenle T119, kendi kabul kriterini karşılaması için henüz başlamamış T366’nın çıktısına dayanır. Ayrıca kontrol listesi yalnız göçte bu satırların bulunmadığını sınar; açılışta oluşturulma ve işlem izine girmeme hiç sınanmaz. Bu, task sırası ve kabul–doğrulama güvencesi çelişkisidir (4).
> Öneri: T119’u yalnız “göçte seed edilmez” sınırıyla kapatın. Oluşturma davranışı, işlem izi yasağı ve bunların doğrulamasını T366’ya taşıyın; T119’da bunu açık ileri işaret olarak belirtin.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §5, T117 kabul kriteri 3 ve doğrulama kontrol listesi KK3
> Alıntı: “`Undelivered`'da yalnız gönderildikten sonra geri dönen satırda doludur”
> Sorun: Aynı task’ın KK3 maddesi, “gönderim anı dolu da boş da `Undelivered` satır kabul edilir” der. Böylece kabul kriterinin zorunlu tuttuğu gönderilmiş-olma kanıtı test tarafından kabul edilmektedir. Gönderilmemiş bir bildirimin teslim edilemedi olarak kaydedilmesi engellenmez. Bu, kabul kriteri ile doğrulamasının çelişmesidir (4).
> Öneri: `Undelivered` durumunda gönderim anını zorunlu kılın ve KK3’ü, gönderim anı boş `Undelivered` satırını reddedecek şekilde düzeltin.
>
> BULGU-3
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §5, “Task yapısı” konvansiyonu 8; T10
> Alıntı: “Blok en çok 2,5 KB (başlık satırından sonraki başlığa kadar)”
> Sorun: T10 bloğu, yalnız kabul kriterleri ve kontrol listesiyle dahi 2,5 KB sınırını aşmaktadır. Buna rağmen başlık notu `11_task_blok_denetle.py --tam` sonucunun sorunsuz olduğunu söyler. Bu, planın ajanların sınırlı hedef blok okumasını koruyan kendi boyut güvencesinin sağlanmadığını gösterir (4).
> Öneri: T10’u bağımsız task bloklarına bölün veya karar kaydıyla blok boyutu kuralını ve denetim aracını tutarlı biçimde güncelleyin.
>
> SONUÇ: 3 BULGU

**(c) Parça 2 (F2 · F3 T301–T345):**

> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: T201 KK5; T302 madde 9
> Alıntı: “İmha kaydı silmeyle aynı veritabanı işleminde yazılır” / “Dosya silen parça satırını dosyayı silmeden önce yazar”
> Sorun: Dosya silme veritabanı işlemiyle atomik değildir. T302, dosya silinmeden önce imha kaydını kalıcılaştırır; disk silmesi başarısız olursa kişisel veri dosyada kalırken imha kaydı silinmiş gibi görünür. Bu, kişisel verinin yanlış imha akışına yol açar ve iki task’ın güvence tanımıyla çelişir (1, 4).
> Öneri: Dosya imhası için bekleyen/tamamlandı durumlu dayanıklı bir iş kaydı tanımlayın; dosya gerçekten silinmeden nihai imha sayısını yazmayın. Başarısız silmeyi yeniden denenecek durumda tutun ve T201’deki atomiklik kriterini bu mekanizmayla uyumlu olarak düzeltin.
>
> BULGU-2
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: T335 madde 10
> Alıntı: “yaş doğrulaması, izin akışı, engelleme mekanizması, yasaklı kategori listesi ve mevzuat belgesi alanı yoktur”
> Sorun: Plan, genel fiziksel ürün satışında mevzuatça internetten tüketiciye satışı yasak ürünleri de kataloglayıp satmaya engel bırakmıyor. Tütün mamulleri ve alkollü içkilerin elektronik ticaret araçlarıyla tüketiciye satışına ilişkin yasak yürürlüktedir; bu nedenle doğrudan satış modeli hukuka aykırı satışa izin verebilir. [Tarım ve Orman Bakanlığı açıklaması](https://www.tarimorman.gov.tr/TADAB/Sayfalar/Detay.aspx?TermId=dcbda54d-586a-4170-8f13-9bd6f4df4eed&TermSetId=429287a1-e5aa-4eb2-b299-4176bdf8330c&TermStoreId=368e785b-af33-487d-a98d-c11d5495130b&UrlSuffix=24)
> Öneri: `02`ye GAP açarak izin verilmeyen ürünlerin kapsam dışı olduğunu veya yasal kategori denetiminin nasıl çalışacağını karara bağlayın; T332/T335’te katalogya alma ve satış kapısının bu kararı zorunlu olarak uyguladığını test edin.
>
> BULGU-3
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: T216 madde 7; T341 madde 2
> Alıntı: “`I`, `İ`, `ı` ve `i` aynı harf sayılır” / “tek dayanak doğrulanmış e-postadır”
> Sorun: Dört farklı Unicode karakterini aynı e-posta kabul etmek, gerçekte farklı olabilen iki posta kutusunu tek kimlikte birleştirir. T341 bu eşitliği geçmiş misafir siparişlerini doğrulanmış hesaba bağlamak için kullandığından, başka bir kişinin sipariş ve kişisel verileri yanlış hesaba açılabilir (1).
> Öneri: E-posta için Türkçe kültür dönüşümü kullanmayın. Yerel bölümü doğrulanan saklı değerle ya da açıkça tanımlı, kültürden bağımsız bir kanonikleştirmeyle eşleştirin; alan adı için IDNA kanonikleştirmesi uygulayın. Mevcut çakışmaları otomatik bağlamayın; yeniden doğrulama isteyin.
>
> BULGU-4
> Kriter: Teknik doğruluk
> Seviye: Orta
> Yer: Okuma tablosu ve boyut; T301
> Alıntı: “task bloğu en çok 2,5 KB”
> Sorun: T301 bloğu, kabul kriterleri ve dokuz maddelik doğrulama listesiyle bu üst sınırı aşmaktadır. Buna rağmen doküman, sınır aşımında karar kaydı açılacağını ve denetim betiğinin sorunsuz olduğunu söylüyor. Bu, task-okuma ve blok boyutu güvencesiyle çelişir (4).
> Öneri: T301’i, kural ayrıntılarını kaynak kimliklerine bırakarak 2,5 KB altına indirin veya izinli alt task’lara bölün; ardından blok denetimini ve beyan edilen sayımları güncelleyin.
>
> SONUÇ: 4 BULGU

**(d) Parça 3 (F3 T346–T390 · F4):**

> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: T350 KK5; T368 kabul kriteri 2
> Alıntı: “Teslimat sözü kargoya verme süresidir” / “Düğme, firmaya ulaşmasından otuz gün geçmiş … siparişte görünür”
> Sorun: Taahhüt edilen kargoya verme süresi aşıldığında fesih hakkını 30 gün koşuluna bağlamak, tüketicinin taahhüt edilen sürede ifa edilmemesi hâlindeki fesih hakkını engeller. Mesafeli Sözleşmeler Yönetmeliği m.16/2, taahhüt edilen sürenin ihlalinde feshe izin verir; 30 gün, taahhüt yoksa üst sınırdır. Bu hak kaybı doğurur.
> Öneri: Gecikme feshi düğmesini, taahhüt edilen kargoya verme süresi aşılıp edim gerçekleşmediğinde açın; 30 günü yalnız taahhüt yoksa yasal son tarih olarak kullanın. Bu iki senaryoyu ayrı kabul kriteri ve testle sınayın. [Ticaret Bakanlığı bilgilendirmesi](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)
>
> BULGU-2
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: T368 kabul kriteri 6
> Alıntı: “sistem faiz hesaplamaz”
> Sorun: Gecikme nedeniyle fesihte yalnız faiz uyarısı göstermek, kanuni faizin iade tutarına eklenmesini güvenceye almaz. Mesafeli Sözleşmeler Yönetmeliği m.16/3, fesih bildiriminden itibaren en geç 14 gün içinde tahsil edilen tutarların kanuni faiziyle iadesini zorunlu kılar. Planın geri ödeme tutarı kuralları da bu faizi üreten bir yol tanımlamıyor.
> Öneri: T369–T370’ye fesih bildirimi ile ödeme tarihi arasındaki kanuni faizin hesaplanması, geri ödeme tutarına eklenmesi ve 14 günlük sürenin test edilmesi kriterlerini ekleyin. [Mesafeli Sözleşmeler Yönetmeliği m.16](https://tuketici.ticaret.gov.tr/data/5e81982d13b876a1b04c7a42/2023-6502%20Say%C4%B1l%C4%B1%20T%C3%BCketicinin%20Korunmas%C4%B1%20Hakk%C4%B1nda%20Kanun.pdf)
>
> BULGU-3
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: T409 kabul kriteri 6; KK6
> Alıntı: “`sub` alınmaz, kayda yazılmaz”
> Sorun: Bağlayıcı yalnız e-posta olduğunda, e-posta değişimi veya aynı e-posta değerinin başka bir Google hesabına bağlanması mevcut müşteri hesabına girişe yol açabilir. Bu, sipariş ve kişisel verilere yetkisiz erişim riskidir. Google, e-postanın değişebileceğini veya benzersiz olmayabileceğini; hesap bağında benzersiz ve kalıcı `sub` değerinin kullanılması gerektiğini açıkça belirtir.
> Öneri: `iss` + `sub` çiftini korumalı bir harici kimlik bağı olarak saklayın ve sonraki Google girişleri ile yeniden doğrulamayı bu bağ üzerinden yapın. E-postayı yalnız doğrulanmış profil bilgisi olarak kullanın; mevcut hesaba ilk bağlama için ayrıca doğrulanmış, oturumlu hesap onayı isteyin. Bu kaynak kararla çeliştiğinden önce §8’e GAP açıp T409’u buna göre hizalayın. [Google OpenID Connect dokümantasyonu](https://developers.google.com/identity/openid-connect/openid-connect)
>
> SONUÇ: 3 BULGU

**(e) Parça 4 (F5 · F6 · §6):**

> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §5 / T508 kabul kriterleri 2 ve 7; KK2 ve KK7
> Alıntı: “Kalem fiyatı ve satın alınabilirliği güncel üründen okunur” / “ilk eklenme fiyatı değişmez”
> Sorun: Aynı sepet kaleminin fiyatı için iki karşıt kural konuyor; doğrulama listesi de birinde fiyat değişince satırın güncellenmesini, diğerinde ilk eklenme fiyatının sabit kalmasını istiyor. Bu, tutarın farklı hesaplanmasına ve müşterinin ya da firmanın para kaybına yol açar (2, 4).
> Öneri: `02 §3.16.5`teki fiyat kuralını tek kaynak kabul edip T508’in iki kabul kriterini ve KK2/KK7’yi aynı davranışı sınayacak biçimde hizalayın.
>
> SONUÇ: 1 BULGU

**(f) Parça 5 (§7 · §8 · §9):**

> SONUÇ: TEMİZ

## 3. Bağımsız değerlendirme

Ham bulgular numaralandı: tek parça B-1…B-2, parça 1 B-1…B-3, parça 2 B-1…B-4, parça 3 B-1…B-3, parça 4 B-1, parça 5 TEMİZ (13 ham). Bir çift aynı iddiadır (tek parça B-2 = parça 1 B-2, T117 KK3) → 12 ayrı bulgu (F1–F12; sıra: doğrulayıcı sırası). On `[dogrulama]` Opus / high doğrulayıcıya verildi: Yüksek bulgular tek tek (D1: F1, D2: F2, D3: F3, D4: F4, D5: F5, D6: F6, D7: F7, D8: F8, D9: F9), Orta bulgular aynı bölümden (§5 blok konvansiyonu ve T117) gruplu (D10: F10 + F11 + F12, dört ham satır); toplam 10 doğrulayıcı (bekçi sınırı 20; bekçi olayı yok). Önceki turlarda reddedilen iddianın olabileceği bulgularda (F1, F6, F7, F9, F10, F11, F12; F3 için yalnız sıra iddiası) paketle birlikte önceki hükmün tam dosyası verildi ve talimata "yeni bilgi yoksa kısa gerekçeyle tekrar eden RET" kuralı yazıldı; 6. turda değişen T04 k1/k6/k8 cümleleri F1 paketine konuldu. Hükümler `11_audit_hukum_denetle.py` ile denetlendi: sorun 0.

| # | Bulgu (kaynak) | Karar | Gerekçe | Uygulama |
|---|---|---|---|---|
| F1 | F1 çıkışı ve T04 KK5 "geri alınabilir" der, T103 `Down` yöntemini yasaklar (tek parça B-1; T103, T04, T602) | ❌ RET (tekrar eden: 2. tur F1, 3. tur F1, 4. tur F1, 6. tur F1; Yüksek → Düşük) | Yeni bilgi yok; `05` ve `09` son değişikliğinden (#266) beri aynı, 6. turda T04 KK5 satırına dokunulmadı (11:257). "Geri alma" şemayı `Down` ile geri almak değil önceki imaja dönmektir ve CI göç adımında sınanır: `05` MK-74, §8.7.1; `09` §10.6 madde 1, §21.4 madde 3. Bulgunun ilk önerisi ("önceki sürüm yeni şemayla çalışır") zaten seçilmiş modeldir; ikinci kolu (`Down` eklemek) MK-74 ve `09` §10.6 ile çelişirdi. | Yok |
| F2 | T119 k8 yasal metinlerin ilk sürümü açılışta yazılır, işlem izine girmez (yazımı T366); T119 F1'de, T366 F3'te; KK yalnız göçte satır olmadığını sınar (parça 1 B-1; T119, T366) | KISMİ (Yüksek → Düşük) | Sıra iddiası çürük: `Bağımlılık` yalnız aynı fazı yazar, sonraki faz gate'le karşılanır (11:119, K-1408 (1)); T119 k8'in F1'de gözlenen sonucu "göçte yazılmaz"dır ve KK8 bunu tek başına sınar, davranış açık ileri işaretle T366'ya bırakılmıştır (4. tur F2, T331 k8 kalıbı). Kalan geçerli parça: "ilk sürüm açılışta yazılır, işlem izine girmez" (`06 §8.4.2`, `02 §3.33.3`) T366'da yalnız "şablon değişince" ile genel kalır ve KK3 ilk sürümü adıyla sınamaz. Düzeltme: T119 işareti T366 k3'e daraltılır, KK3 ilk sürümü kapsar; T366 bloğu 2.559 bayttı, ek 20 bayt başlık kısaltmasıyla (konvansiyon 2) dengelendi (2.555). | T119 k8; T366 başlık, KK3 (3 şartname) |
| F3 | T201 KK5 "imha kaydı silmeyle aynı veritabanı işleminde" ↔ T302 madde 9 "satırı dosyayı silmeden önce yazar"; dosya silme işlemle atomik değil, başarısız silme imha kaydında silinmiş görünür (parça 2 B-1; T201, T302) | KISMİ (Yüksek → Orta) | Öneri (bekliyor/tamamlandı iş kaydı) gereksiz: kaynak bilinçli karar vermiştir — dosya silen parça satırı dosyadan önce yazar, arada çökerse kalan dosya sonraki gece yeniden silinir, "fazla sayım eksik kayda yeğlenir" (K-985 (1); `06 §9.3.4`; `05` §7.7.1, §7.4.4); `06 §9.4.1` atomikliği yalnız anlık silmelerin kapalı listesidir, mevzuat iddiası tutmaz. Geçerli çekirdek (4): T201 madde 5 kapsamsız yazılmış, oysa T201'in kendi dosya parçası (madde 9) var ve onda "önce satır" kuralı yok; K-985'in yeniden silme güvencesi taşıyan "kaydı silinmiş, diskte kalmış dosya" parçası (`OrphanFile`) 11'in hiçbir task'ında kriter değildi. | T201 k5, k9; T302 k9, KK9 (4 şartname) |
| F4 | T335 madde 10 yaş doğrulaması, engelleme ve yasaklı kategori listesi yok; tütün ve alkolün elektronik ticaretle satışı yasak (parça 2 B-2; T335) | ❌ RET (Yüksek → Düşük) | Yasak gerçek (4733 sayılı Kanun m.8/5-k, mevzuat.gov.tr konsolide metni; muhatap "satış sistemi kuran veya faaliyette bulunanlar" yani satıcı firma), ama kural boşluğu yok: ürünün ne satıldığını denetlememesi ve engelleme ya da yasaklı kategori listesinin olmaması kayıtlı karardır (K-84, K-641; K-12 satıcıyı firma yapar), `02` §1.2 kapsam notu, §12.1.12 (alkol ve tütün satışının yasak olduğunu, uygunluğun firmanın yükümlülüğü olduğunu yazar) ve `10` KD-28 bunu söyler; T335 madde 10 KD-28'i aynen tüketir. İkinci modelin kaynağı Bakanlık duyurusu (türetilmiş); aynı iddia `10` cross-review 6., 12., 13. turunda resmî metinle sınanıp reddedildi. GAP önerisi üst doküman kararını yeniden açar; 11 tüketicidir (K-1406 (2)(d)). | Yok |
| F5 | T216 madde 7 "`I`, `İ`, `ı`, `i` aynı harf sayılır" ve T341 madde 2 "tek dayanak doğrulanmış e-postadır": farklı posta kutuları tek kimlikte birleşir (parça 2 B-3; T216, T341) | ❌ RET (Yüksek → Düşük) | T216 madde 7 `07 §2.9.5`, `04` 8.2.10 ve `06 §2.8.1`–§2.8.2'yi doğru tüketir; ikinci modelin "Türkçe kültür dönüşümü kullanmayın" önerisi zaten metnin kendisidir. Risk (yalnız ı/i ile ayrılan iki gerçek e-posta birleşir, misafir siparişi yanlış hesaba düşebilir) K-1016'da açıkça tartışılıp kabul edilmiştir; elenen seçenekler gerekçelidir (Türkçe kural: ASCII "I" sorunu; dilden bağımsız dönüşüm: "İNDİRİM" kuponu eşleşmez), e-posta sağlayıcıları ASCII dışı yerel kısmı neredeyse hiç vermez. T341 madde 2 `02 §8.1.2` ile birebirdir. Mevzuata aykırılık kanıtlanmadı; kalan durum nadir kenar durumdur. | Yok |
| F6 | T350 KK5 "teslimat sözü kargoya verme süresidir" ↔ T368 k2 fesih düğmesi 30 gün koşulunda; taahhüt aşımında fesih engellenir (m.16/2) (parça 3 B-1; T350, T368) | ❌ RET (tekrar eden: 5. tur F3; Yüksek → Düşük) | Yeni bilgi yok. T350 KK5 `02 §3.20.4`'ün (02:825) aynen türevidir. Taahhüt edilen sürenin aşımıyla doğan fesih T368 k1'de zaten karşılanır ("Kargoya verme süresi aşılınca fesih mevcut iptal düğmesiyle kullanılır", `02 §7.2.3`, Z-10, §12.1.8; K-198, K-499); k2'nin 30 günü yalnız ikinci yol olan Z-11 düğmesinin (kargodaki sipariş) koşuludur. Resmî metin: K-499 mevzuat.gov.tr konsolide m.16/1–3'ü aktarmıştır; bu turda doğrulayıcı mevzuat.gov.tr metnini araçla okuyamadı (PDF), K-499'un aktarımı esas alındı; Codex'in kaynağı Bakanlık bilgilendirme sayfası (derleme). Çelişki ve hak kaybı yok. | Yok |
| F7 | T368 k6 "sistem faiz hesaplamaz"; m.16/3 kanuni faizli iadeyi zorunlu kılar (parça 3 B-2; T368, T369, T370) | ❌ RET (tekrar eden: 2. tur F9, 3. tur F5, 4. tur F1, 6. tur F4; Yüksek → Düşük) | Yeni bilgi yok; `02`, `04`, `06`, `07` son değişikliği #266. Faiz yükümlülüğü satıcı firmaya aittir ve üst doküman bunu bilinçli karar olarak firmaya verir: `02` §7.2.3, §12.1.8, §12.5 tablosu ("Kanuni faizi hesaplar ve müşteriye öder"; K-499, K-571); `06` "faiz alanı yok", `04` ve `07` §9.9.14 "yoktur: kanuni faizin hesabı". T369/T370'e faiz hesabı eklemek üst dokümanlara aykırı kod yazdırırdı. Gösterilen kaynak Yönetmelik değil, 6502 sayılı Kanun PDF'i. | Yok |
| F8 | T409 kriter 6 "`sub` alınmaz, kayda yazılmaz": e-posta tek bağlayıcıyken hesap ele geçirilebilir (parça 3 B-3; T409) | ❌ RET (Yüksek → Düşük) | T409 KK6 kaynak kararın aynen tüketicisidir: `08 §2.3` (`sub` "saklanmaz", K-1061; Google'ın e-posta uyarısını anıp riski kalan risk olarak kabul eder), `09 §11.3` madde 4, `06 §4.36`, `02 §3.13.7`; K-900 `sub`'a göre eşleşmeyi elemiştir. Önerinin "yetkili olmayan adreste ilk bağda ayrı teyit" kısmı kaynakta zaten var (K-1281; e-posta değişince bağ kalkar, K-1117). Aynı öneri üst doküman turlarında karara bağlandı (`08` cross-review R4 H2 RET, K-1290; `05` R2–R9). Google OpenID Connect belgesi mevzuat değil rehberdir; 11 içinde çelişki yok; "§8'e GAP aç" üst doküman kararını yeniden açar, 11 tüketicidir. | Yok |
| F9 | T508 KK2 / kriter 2 "fiyat güncel üründen okunur" ↔ kriter 7 / KK7 "ilk eklenme fiyatı değişmez" (parça 4 B-1; T508) | ❌ RET (tekrar eden: 2. tur F10; Yüksek → Düşük) | Yeni bilgi yok; T508 kriterleri (11:4407, 11:4412) 2. turdaki metinle aynı. İki cümle `02 §3.16.5`'in (K-80, K-131) iki ayrı kavramıdır: kalem fiyatı güncel üründen okunur, "ilk eklenme fiyatı" yalnız "fiyatı değişti" işaretinin referansıdır (`07` UC-20; T346 kriter 5). Bulgunun "KK7 ilk eklenme fiyatının sabit kalmasını istiyor" okuması yanlıştır: KK7 (11:4424) "mutlak adet, tekrar aynı sonuç"tur, fiyat sınamaz. | Yok |
| F10 | T10 bloğu 2,5 KB sınırını aşar; başlık notu `--tam` sorunsuz der (parça 1 B-3; T10) | ❌ RET (tekrar eden sınıf: 3. tur F13, 5. tur F11, 6. tur F6–F7; Orta → Düşük) | Olgu yanlış: T10 bloğu 2.534 bayt (sınır 2.560, `11_task_blok_denetle.py` `MAKS_BAYT`; 11:41, 11:169 madde 8); `--tam` TOPLAM SORUN 0. İkinci model 2,5 KB'ı 2.500 bayt saymış olmalı. | Yok |
| F11 | T301 bloğu aynı sınırı aşar (parça 2 B-4; T301) | ❌ RET (tekrar eden sınıf; Orta → Düşük) | Olgu yanlış: T301 bloğu 2.554 bayt (sınır 2.560); `--tam` TOPLAM SORUN 0; 11:41'in "sınır aşılırsa karar kaydında konu açılır" koşulu doğmadı. Not (bulgu değil): T301 sınıra 6 bayt uzaktır; bu bloğa sonradan eklenecek metin bloğu bölmeyi (K-1409 (2)) ya da kısaltmayı gerektirir. | Yok |
| F12 | T117 kriter 3 ↔ KK3: `Undelivered`'da gönderim anı hem dolu hem boş kabul edilir (tek parça B-2, parça 1 B-2; T117) | ❌ RET (tekrar eden: 1. tur D3 #2, 3. tur #6, 5. tur F10, 6. tur F8; Orta → Düşük) | Yeni bilgi yok; metin değişmedi (11:781, 11:789). `06 §4.58` Kısıtlar ve §5 kısıt özeti `Undelivered`'da `SentAt`'i iki hâl için açık bırakır: yeniden denemeleri de başarısız kalıp hiç gönderilmemiş satırda boş, gönderildikten sonra geri dönen satırda dolu (K-1120). KK3 kuralın doğrudan karşılığıdır; alanı zorunlu kılmak `06 §4.58` ve P-38 ile çelişirdi. | Yok |

**Tekrar eden RET (turun sonucunu belirlemez):** F1 (2., 3., 4., 6. tur), F6 (5. tur), F7 (2., 3., 4., 6. tur), F9 (2. tur), F12 (1., 3., 5., 6. tur); F10 ve F11 aynı sınıfın (blok bayt ölçüsü) dönüşüdür (3., 5., 6. tur). **Seviye düşenler:** Yüksek → Düşük: F1, F2, F4, F5, F6, F7, F8, F9; Yüksek → Orta: F3; Orta → Düşük: F10, F11, F12. **Kısmi kalış:** F2 (sıra iddiası çürük, yalnız KK3 ve işaret düzeltildi); F3 (öneri daraltıldı: iş kaydı mekanizması gereksiz, K-985 bilinçli karar).

**Çıkış değerlendirmesi (Faz 4 madde 4–5).** Sonuç TEMİZ değil: tek parça ve parça 1–4 bulgu getirdi (parça 5 TEMİZ). Madde 5'in çıkış kuralı işlemez: kabullerden F3 Orta şiddette (kriter kapsamı ↔ kaynak karar farkı; mevzuata aykırılık, para ya da hak kaybı değil ama "düşük metin farkı" sınıfında da değil) ve kural yalnız kabullerin hepsi düşük metin farkıysa kapatır. F2 Düşük. Tekrar eden RET kuralı F1, F6, F7, F9, F12'yi turun sonucundan ayırır. **Tur 8 gerekir.**

**F2–F3 — uygulama ve gözlem.** Yedi çift `11_audit_uygula.py` ile uygulandı; yazım kapanışı, matris kapsama, task blok denetimi (`--tam`), geri yön denetimi ve geri işaret taraması sorun 0. Blok boyutları: T119 2.471, T201 2.552, T302 2.558, T366 2.556 bayt (sınır 2.560). 6. turda eklenen T04 k6 cümlesi ("Paketleme ve yayın komutları imajlarla T06'da girer; o güne dek boş komut uyarıyla atlanır") ile T04 k9 arasındaki çelişki bu turda hiçbir çağrıda bulgu olarak çıkmadı. Gözlem: T302 (2.558) ve T301 (2.554) sınıra çok yakındır; sonraki ekler bloğu bölmeyi gerektirir.

## 4. Ek bulgular

- Doğrulayıcılardan `11`'i ya da üst dokümanları etkileyen ek bir tutarsızlık bildirilmedi. Üst doküman notu yok; `00`/`CLAUDE.md`/`SETUP.md` önerisi yok.
- F6 için kayıt: doğrulayıcı mevzuat.gov.tr'deki Yönetmelik metnini araçla açamadı (sayfa madde metnini vermedi, PDF okunamadı); hüküm K-499'un konsolide metin aktarımına ve 02'nin birebir karşılığına dayanır. Hükmün dayandığı yer değişmedi; yeni bir resmî dayanak da ortaya çıkmadı.

## 5. Kullanıcı onay checklist'i

- [ ] F1 — RET, tekrar eden (uygulanmadı)
- [ ] F2 — KISMİ (uygulandı: T119 k8; T366 başlık, KK3)
- [ ] F3 — KISMİ (uygulandı: T201 k5, k9; T302 k9, KK9)
- [ ] F4 — RET (uygulanmadı)
- [ ] F5 — RET (uygulanmadı)
- [ ] F6 — RET, tekrar eden (uygulanmadı)
- [ ] F7 — RET, tekrar eden (uygulanmadı)
- [ ] F8 — RET (uygulanmadı)
- [ ] F9 — RET, tekrar eden (uygulanmadı)
- [ ] F10 — RET (uygulanmadı)
- [ ] F11 — RET (uygulanmadı)
- [ ] F12 — RET, tekrar eden (uygulanmadı)

⚠ karar: yok. Önerilen `00`/`CLAUDE.md`/`SETUP.md` değişikliği yok.
