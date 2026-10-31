# Cross-Review — 11 Implementation Planı (Tur 9)

**Tarih:** 2026-10-31 · **Hedef:** `Docs/11_IMPLEMENTATION_PLAN.md` v0.46 (girdi commit'i `48b554c`, 760 295 bayt girdi, 7 760 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma); betiğin `status.txt` satırı: `model=codex gpt-5.6-terra high` · **Ciddiyet ölçüsü:** 1. turdan itibaren, mekanizma okuma paragrafıyla (K-1399; `11_CROSS_REVIEW.md` §1) · **Koşum:** iki koşum — tek parça çağrı ve beş parçalı kontrol koşumu · **Sonuç:** **TEMİZ DEĞİL** — 15 ham bulgu, 15 ayrı bulgu; 1 doğru · 1 kısmen · 13 çürük (dokuzu önceki turlarda reddedilmiş iddianın dönüşü); 2 şartname çifti uygulandı (T07 KK6; T336 KK8); iki kabul de Orta ve 8. turun kabulleriyle aynı sınıftadır → sıradaki adım hedefli mercek.

## 1. Koşum

Betikler, istem, parça notu, şablon ve **parça sınırları 1. turdakinin aynısıdır** (`11_CROSS_REVIEW.md` §1; K-1478 — sınır başlıkla tanımlıdır, değişmez). İstem tek karakter değiştirilmeden kullanıldı. Girdi `build_inputs.py` ile kuruldu (`uncovered lines: none`). İkinci model çağrısı 6., 7. ve 8. turdaki gibi tek betikle yapıldı (K-1484): `bash scripts/doc-scans/cross_review_ikinci_model.sh in out c1 p1 p2 p3 p4 p5`; betik asıl modeli (Codex `gpt-5.6-terra`, yüksek) sağlık çağrısıyla seçti (`used: codex gpt-5.6-terra`), altı çağrının hepsi aynı modelle, tek komutta, ilk denemede `rc=0` döndü (çıkış kodu 0; çıktı dosyaları ayrıca okundu).

| Çağrı | Bölümler | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|
| Tek parça | tüm doküman | 760 295 bayt | 415 022 | 70 sn | 2 bulgu |
| Parça 1 | §1–§4 · §5 Task yapısı · F0 · F1 (satır 65–866) | 96 710 bayt | 40 916 | 68 sn | 2 bulgu |
| Parça 2 | F2 · F3 T301–T345 (867–2841) | 197 349 bayt | 100 877 | 276 sn | 4 bulgu |
| Parça 3 | F3 T346–T390 · F4 (2842–4219) | 150 968 bayt | 107 181 | 263 sn | 5 bulgu |
| Parça 4 | F5 · F6 · §6 (4220–5633) | 149 650 bayt | 73 122 | 167 sn | 2 bulgu |
| Parça 5 | §7 · §8 · §9 (5634–dosya sonu) | 254 332 bayt | 135 714 | 114 sn | TEMİZ |

Parça 5 TEMİZ döndü; öteki beş çağrı bulgu getirdi. İki koşum arasında aynı iddia çıkmadı: on beş ham bulgunun hiçbiri bir başkasının tekrarı değil. 8. turun kabul ettiği dört yer (T118 KK1, T224 k2/KK2, T370 KK6/KK7, T390 KK8) bu turda hiçbir çağrıda yeni bulgu üretmedi; T224 yalnız çerez süzgeci iddiasında (F1), T370 yalnız faiz iddiasında (F12) anıldı.

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**(a) Tek parça çağrı:**

> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: T224 kriter 6–7; T331 kriter 3
> Alıntı: "Çerez politikası ilk kez yayına alınana kadar vitrin çerez yazmaz; ara katman çerez başlıklarını yanıttan çıkarır (...) Çerez süzgeci yalnız vitrinin yanıtlarına uygulanır"
> Sorun: T331, çerez politikası yayımlı değilken mevcut hesapların girişinin etkilenmeyeceğini söyler. Ancak müşteri girişi vitrin yanıtıdır; oturum ve sahteciliğe karşı koruma çerezlerinin `Set-Cookie` başlıkları çıkarılırsa müşteri oturumu kurulamaz. Bu, kullanıcının giriş yapamadığı çıkışsız akış ve doküman içi çelişkidir (3), (4).
> Öneri: Çerez süzgecini açıkça yalnız anonim içerik yanıtlarıyla sınırlayın; giriş, oturum, sahteciliğe karşı koruma ve gerekli dönüş akışlarını dışarıda bırakın. Alternatif olarak, bu durumda müşteri girişini de kapatın ve T331’i buna göre hizalayın.
>
> BULGU-2
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: T07 kabul kriteri 6; doğrulama kontrol listesi KK6
> Alıntı: "Otuz günden eski tam yedek ve günlükler silinir; sınırdan eski en yeni tam yedek tutulur"
> Sorun: Bu kabul kriteri, kurtarılabilir son tam yedeğin 30 günlük sınırı aşsa bile korunmasını zorunlu kılar. Buna karşılık KK6 yalnız “saklama ayarı otuz gün”ü denetler. Düz bir 30 günlük yaşam döngüsü denetimi geçip eldeki son tam yedeği de silebilir; bu da kurtarma imkânı ve firma verisi kaybı doğurur (2).
> Öneri: KK6’ya, sınırdan eski son tam yedeğin silinmediğini; ondan daha eski tam yedeklerin ve ilgili günlüklerin silindiğini doğrulayan senaryo ekleyin. Bu senaryoda geri yükleme provası da zorunlu kanıt olmalıdır.
>
> SONUÇ: 2 BULGU

**(b) Parça 1 (§1–§4 · §5 Task yapısı · F0 · F1):**

> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §3 F1 çıkış ürünü; §5 T103 ve diğer F1 göç task’ları
> Alıntı: "Çıkış ürünü: göçler ileri, idempotent ve geri alınabilir" / "Göç tek konudur; `Down` ve işlem dışı adım yoktur"
> Sorun: F1’in kapıdan geçmesi için bütün göçlerin geri alınabilir olması isteniyor; ancak entity göç task’ları `Down` yöntemini açıkça yasaklıyor. Bu, faz çıkış garantisi ile task çıktılarının çelişmesidir (4).
> Öneri: Ya her göç için doğrulanabilir geri alma yolu ekleyin ya da F1 çıkış ölçütünü forward-only göç ve tanımlı yedekten geri dönüş yaklaşımıyla değiştirip bu yaklaşımın testini ilgili task/gate’e yazın.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §5 F0, T03 kabul kriteri 6; T01 bağımlılığı ve F0 yürütme sırası
> Alıntı: "WebApplicationFactory fabrikası uygulamayı koleksiyon veritabanıyla açar ve örnek istek geçer; ilk uç testi T01'dedir" / "#### T01: Canlılık ucu - **Bağımlılık:** T02, T03"
> Sorun: T03, T01’den önce tamamlanmak zorunda olduğu hâlde çalışan bir örnek uç isteğini kabul kriteri yapıyor; ayrıca ilk uç testini T01’e atıyor. T01’in ucu henüz üretilmemişken T03’ün bu kabul kriteri tamamlanamaz. Bu, task sırası ve kabul kriterlerinin birbirini tutmamasıdır (4).
> Öneri: T03’teki isteği yalnız uygulamanın/fabrikanın başlatılmasını doğrulayan uçsuz bir kontrole çevirin; ilk başarılı uç isteğini ve entegrasyon testini yalnız T01’de tutun.
>
> SONUÇ: 2 BULGU

**(c) Parça 2 (F2 · F3 T301–T345):**

> BULGU-1
> Kriter: teknik doğruluk
> Seviye: Orta
> Yer: T201 kabul kriteri 1; Bağımlılık
> Alıntı: "İmha işi her gece Türkiye saatiyle 03:00'te çalışır ve süre envanterini sırayla işler; imha parçaları F3'tedir (T301, T302)"
> Sorun: T201, kendi kabulünün uygulanması için T301 ve T302 çıktısına dayanıyor; fakat bağımlılık listesinde yalnız T221 ve T226 var. F2’de tamamlanması gereken task’ın kabulü sonraki fazdaki task’lara bırakılmıştır. Bu, task sırası güvencesini bozar (4).
> Öneri: T201’i yalnız zamanlayıcı/iş kaydı altyapısıyla sınırlayın; imha davranışı ve testlerini T301–T302’ye taşıyın ve bu task’lara T201 bağımlılığını açıkça ekleyin. Alternatif olarak T301–T302’yi F2’ye alın.
>
> BULGU-2
> Kriter: tutarlılık
> Seviye: Yüksek
> Yer: T312 kabul kriteri 8; T312 KK8
> Alıntı: "Parça 5 MB'ı aşarsa 413, `Content-Range` yoksa ya da toplamı aşarsa 400 alır" / "uymayan başlangıçlı ve toplamı aşan parça 409 alır."
> Sorun: Aynı “toplamı aşan parça” için kabul kriteri 400, doğrulama listesi 409 öngörüyor. 409 devam ettirilebilir konum çakışmasını, 400 ise bozuk isteği ifade edeceğinden yükleme betiği hangi telafi yolunu izleyeceğini güvenilir biçimde belirleyemez; kullanıcı yüklemesi takılabilir. Bu doğrudan iç çelişkidir (4).
> Öneri: Tek durum kodunu seçin. Kabul kriterindeki ayrıma göre, yalnız alınan bayt konumuyla çakışan parçada 409; bildirilen toplamı aşan veya biçimsiz `Content-Range` için 400 kullanın ve KK8’i buna göre düzeltin.
>
> BULGU-3
> Kriter: mevzuat
> Seviye: Yüksek
> Yer: T330 kabul kriteri 7; T331 kabul kriterleri 1 ve 8
> Alıntı: "ETBİS kaydı yapılmaz; kayıt bilgisi isteğe bağlı alandır, satış kapısını tutmaz"
> Sorun: Kendi elektronik ticaret ortamında faaliyet gösteren hizmet sağlayıcının faaliyete başlamadan önce ETBİS’e kayıt olması gerekir. Plan ise kaydı yapılmayacak bir işlem olarak tanımlıyor ve satış kapısını bu yükümlülükten bağımsız açıyor. Bu, satışa mevzuata aykırı başlama riski yaratır (1). [Ticaret Bakanlığı ETBİS SSS](https://ticaret.gov.tr/ic-ticaret/sikca-sorulan-sorular/elektronik-ticaret)
> Öneri: T330’daki hükmü kaldırın; ETBİS kaydı ve değişiklik bildirimlerinin tamamlandığını kanıtlayan bir yayın öncesi gate oluşturun. T331 satış kapısında bu gate karşılanmadan satış açılmamasını sağlayın.
>
> BULGU-4
> Kriter: teknik doğruluk
> Seviye: Orta
> Yer: T336 kabul kriteri 8; T336 KK8
> Alıntı: "Ürün sayfası 'üretim yeri (ülke)' ... taşır" / "fiziksel üründe ülke zorunlu, dijitalde isteğe bağlı; birim seçili üründe net miktarsız ya da sıfır miktarlı varyant reddedilir, birim fiyat türetilir."
> Sorun: Kabul kriteri, üretim yerinin ürün sayfasında gösterilmesini zorunlu kılıyor; KK8 yalnız alanın zorunluluğunu ve ölçü birimi kurallarını sınar. Ziyaretçiye gösterim hiç test edilmediğinden kabul kriterinin güvence altına alınmaması söz konusudur (4).
> Öneri: KK8’e fiziksel ürün sayfasında üretim yerinin görünmesini; dijital üründe alan doluysa doğru görünmesini ve boşsa yanıtta yanlış/boş etiket üretilmemesini doğrulayan entegrasyon testleri ekleyin.
>
> SONUÇ: 4 BULGU

**(d) Parça 3 (F3 T346–T390 · F4):**

> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: T360 KK3; T363 kabul kriteri 2
> Alıntı: “iki durum ve arşivin yokluğu T322 k1’dedir” / “taslak ve arşivdeki adres dolu sayılır”
> Sorun: Bir task arşiv durumunun olmadığını, diğeri arşivdeki kayıtların adres tuttuğunu söylüyor. Adresin yeniden kullanılabilirliği bu iki kuralla belirlenemediğinden doğrudan doküman içi çelişkidir.
> Öneri: Arşivi ya tamamen kaldırıp T363’ten çıkarın ya da arşivin ayrı bir teknik kayıt durumu olduğunu, yayın durumlarından nasıl ayrıldığını ve adres tutma kuralını açıkça tanımlayın.
>
> BULGU-2
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: T368 kabul kriterleri 1–2
> Alıntı: “Düğme, firmaya ulaşmasından otuz gün geçmiş […] siparişte görünür”
> Sorun: Tüketicinin fesih hakkı, satıcının taahhüt ettiği sürede edimini yerine getirmemesiyle doğar; yalnız siparişten 30 gün sonra açılması, daha kısa taahhütte tüketicinin hakkını geciktirir. Bu mevzuata aykırıdır ve hak kaybı doğurur. [Mesafeli Sözleşmeler Yönetmeliği m.16](https://tuketici.ticaret.gov.tr/data/5e81982d13b876a1b04c7a42/2023-6502%20Say%C4%B1l%C4%B1%20T%C3%BCketicinin%20Korunmas%C4%B1%20Hakk%C4%B1nda%20Kanun.pdf)
> Öneri: Fesih yolunu, taahhüt edilen kargoya verme/ifa süresi geçer geçmez açın; 30 günü yalnız mal satışındaki azami süre denetimi olarak ayrıca uygulayın.
>
> BULGU-3
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: T368
> Alıntı: “Gecikme feshi ve firma iptali: iki fesih yolu, kanuni faiz uyarısı, kapalı sebep listesi, ifanın imkânsızlaşması bildirimi”
> Sorun: Başlıkta yer alan imkânsızlaşma bildirimi için kabul kriteri ve test yoktur. Stok yokluğunun imkânsızlık sayılmamasını söylemek, gerçek imkânsızlaşmada tüketiciye üç gün içinde kalıcı veri saklayıcısıyla bildirim ve en geç 14 günde iade yükümlülüğünü karşılamaz. Bu yasal yükümlülük uygulanmadan kalır. [Mesafeli Sözleşmeler Yönetmeliği m.16/4](https://tuketici.ticaret.gov.tr/data/5e81982d13b876a1b04c7a42/2023-6502%20Say%C4%B1l%C4%B1%20T%C3%BCketicinin%20Korunmas%C4%B1%20Hakk%C4%B1nda%20Kanun.pdf)
> Öneri: Gerçek imkânsızlaşma için ayrı bir kayıt, üç günlük bildirim, 14 günlük iade başlangıcı ve bunları sınayan kabul/test maddeleri ekleyin.
>
> BULGU-4
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: T368 kabul kriteri 6; T369–T370
> Alıntı: “sistem faiz hesaplamaz”
> Sorun: Süre aşımı nedeniyle fesihte tüm ödemeler yasal faizle birlikte 14 gün içinde iade edilmelidir. Plan yalnız uyarı üretip faizi hesaplama, kaydetme ve geri ödemeye katma yolunu dışarıda bırakıyor; böylece eksik geri ödeme riski doğuyor. [Mesafeli Sözleşmeler Yönetmeliği m.16/3](https://tuketici.ticaret.gov.tr/data/5e81982d13b876a1b04c7a42/2023-6502%20Say%C4%B1l%C4%B1%20T%C3%BCketicinin%20Korunmas%C4%B1%20Hakk%C4%Bında%20Kanun.pdf)
> Öneri: Faizi hesaplayıp iade tutarına ekleyen bir akış ekleyin; manuel uygulanacaksa tutar, onay, süre ve kanıtı zorunlu bir işlem olarak tanımlayın.
>
> BULGU-5
> Kriter: Kullanıcı deneyimi
> Seviye: Yüksek
> Yer: T372 kabul kriteri 8
> Alıntı: “Kargoya verilmemiş fiziksel kalemde cayma düğmesi ve kaydı yoktur; başka kanaldan gelen bildirim ‘müşteriyle anlaşıldı’ iptaliyle işlenir”
> Sorun: Tüketici, sözleşmenin kurulmasından malın teslimine kadar cayma hakkını tek taraflı kullanabilir. Bu bildirimi “müşteriyle anlaşıldı” iptaline dönüştürmek, kanuni hakkı firma teyidine/anlaşmasına bağlı bir akışa çevirir; tüketicinin fesih ve iade hakkı kaybolabilir. [Mesafeli Sözleşmeler Yönetmeliği m.9 ve m.11](https://tuketici.ticaret.gov.tr/data/5e81982d13b876a1b04c7a42/2023-6502%20Say%C4%B1l%C4%B1%20T%C3%BCketicinin%20Korunmas%C4%B1%20Hakk%C4%B1nda%20Kanun.pdf)
> Öneri: Kargoya verilmemiş mal için de cayma bildirimini kayıt altına alın; bildirimin ulaştığı zamanı saklayıp caymaya özgü iade ve bildirim akışını uygulayın.
>
> SONUÇ: 5 BULGU

**(e) Parça 4 (F5 · F6 · §6):**

> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: T508, kabul kriterleri 2 ve 7
> Alıntı: "Kalem fiyatı ve satın alınabilirliği güncel üründen okunur" / "ilk eklenme fiyatı değişmez"
> Sorun: Aynı sepet kaleminin fiyatının hem güncel üründen okunacağı hem de ilk ekleme değerinde sabit kalacağı belirtiliyor. Ürün fiyatı değiştiğinde farklı uygulamalar müşteriyi yanlış tutarla ödeme adımına taşıyabilir; bu para kaybına yol açar (2).
> Öneri: Tek fiyat kuralını açıkça seçin. UC-20’deki özel kurala göre fiyat ilk eklemede sabitse, KK2’yi “satın alınabilirlik güncel üründen okunur; kalem fiyatı ilk ekleme değeridir” diye düzeltin ve ürün fiyatı değiştikten sonra GET ile adet güncellemesinin aynı fiyatı koruduğunu test edin.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: T512 kabul kriteri 5; T529 kabul kriteri 10
> Alıntı: "IBAN alanı yalnız ödenmiş havale siparişindedir" / "`SwitchRefundToBankTransfer` `CardRefundFailed` sebepli IBAN isteği açar"
> Sorun: Kartla ödenmiş bir siparişte kart iadesi başarısız olduğunda banka havalesine geçiş için IBAN isteniyor; ancak IBAN arayüzü yalnız ödenmiş havale siparişleriyle sınırlandırılıyor. Müşteri IBAN’ını giremez veya düzeltemezse geri ödeme tamamlanamaz; bu para kaybı ve hak kaybı doğurur (2).
> Öneri: T512’de IBAN alanının, `CardRefundFailed` sonrasında açılan banka havalesi iade isteğinde de görünmesini ve `SaveRefundIban` tarafından kabul edilmesini açıkça ekleyin. Bu akışı T512 KK5 ve KK1–KK3’e, T529 KK10’a uçtan uca test olarak bağlayın.
>
> SONUÇ: 2 BULGU

**(f) Parça 5 (§7 · §8 · §9):**

> SONUÇ: TEMİZ

## 3. Bağımsız değerlendirme

Ham bulgular numaralandı: tek parça B-1…B-2 (F1–F2), parça 1 B-1…B-2 (F3–F4), parça 2 B-1…B-4 (F5–F8), parça 3 B-1…B-5 (F9–F13), parça 4 B-1…B-2 (F14–F15), parça 5 TEMİZ (15 ham, 15 ayrı bulgu; sıra: doğrulayıcı sırası). On dört `[dogrulama]` Opus / high doğrulayıcıya verildi: Yüksek bulgular tek tek (D1: F1, D2: F2, D3: F3, D4: F4, D6: F6, D7: F7, D9: F10, D10: F11, D11: F12, D12: F13, D13: F14, D14: F15), Orta bulgular tek (D5: F5) ya da aynı bölümden gruplu (D8: F8 + F9, ikisi de F3 bölümünde); toplam 14 doğrulayıcı (bekçi sınırı 20; bekçi olayı yok). Önceki turlarda reddedilen iddianın olabileceği bulgularda (F1, F3, F5, F6, F7, F10, F12, F13, F14) paketle birlikte önceki hükmün ilgili bölümleri verildi ve talimata "yeni bilgi yoksa kısa gerekçeyle tekrar eden RET" kuralı yazıldı; 8. turda değişen T118 KK1, T224 k2/KK2, T370 KK6/KK7 ve T390 KK8 cümleleri ilgili paketlere not olarak konuldu. Yasal iddialı bulgularda (F7, F10, F11, F12, F13) resmî metin istendi; F10'da Mesafeli Sözleşmeler Yönetmeliği m.16 mevzuat.gov.tr PDF'inden okundu. Hükümler `11_audit_hukum_denetle.py` ile denetlendi: sorun 0.

| # | Bulgu (kaynak) | Karar | Gerekçe | Uygulama |
|---|---|---|---|---|
| F1 | T224 k6–k7 çerez süzgeci vitrin yanıtındaki oturum ve sahteciliğe karşı koruma çerezlerini de çıkarır; T331 k3 mevcut hesap girişi sürer (tek parça B-1; T224, T331) | ❌ RET (tekrar eden: 2. tur #7, 5. tur #7; Yüksek → Düşük) | Yeni bilgi yok. Çerez politikası ilk kez yayına alınana kadar hesap kaydı ve yeni hesap açan Google girişi kapalıdır (02:507, 02:713) ve bu sürede "zorunlu çerez gerektiren işlev yoktur" (02:2156); kapı yayın anından sonra açılır ve geri kapanmaz (05:785; 02:509). Süzgecin devrede olduğu pencerede girişi gereken bir müşteri hesabı var olamaz; T331 KK3'ün "mevcut hesap girişi sürer" hâli yayından sonradır ve o anda süzgeç kalkmıştır. Panel yanıtları süzgecin dışındadır (05:427). Çelişki ve çıkışsız akış yok. | Yok |
| F2 | T07 k6 "sınırdan eski en yeni tam yedek tutulur" ↔ KK6 yalnız "saklama ayarı otuz gün" (tek parça B-2; T07) | KISMİ (Yüksek → Orta) | Olgu doğru: k6 (11:334) ve kaynak `05 §7.6.6` (05:949) iki sonuç ister (otuz günden eski yedekler silinir, otuz günün başına dönebilmek için sınırdan eski en yeni tam yedek tutulur); KK6 (11:343) ikinci sonucu sınamaz. pgBackRest'te bu sonucu zaman tipli saklama verir; sayı tipli kurulursa KK6 geçer ama otuz günün başına dönüş kaybolur. `11` §5 konvansiyon 7 (11:168) "kabul kriteri başına bir madde" ister (4). Seviye Orta: her gece yeni tam yedek alındığı için (11:330) eldeki son yedek silinmez, yalnız geri dönüş penceresi kısalır. Öneri daraltıldı: senaryo testi ve zorunlu geri yükleme provası kapsam dışı; prova adımı KK7'dedir (11:344), koşumu T601'dedir (4. tur #1 hükmü). | T07 KK6 (1 şartname) |
| F3 | F1 çıkışı "göçler geri alınabilir" der, T103 ve göç task'ları `Down` yasaklar (parça 1 B-1; F1, T103) | ❌ RET (tekrar eden: 2. tur F1, 3. tur F1–F2, 4. tur F1, 6. tur #1–#2, 7. tur #1, 8. tur #6; Yüksek → Düşük) | Yeni bilgi yok; `05` ve `09` son değişikliğinden (#266) beri aynı, 8. turun farkı 11'de geri alma ve `Down` cümlelerine dokunmadı. "Geri alma" şemayı `Down` ile geri almak değil önceki imaja dönmektir ve CI göç adımında sınanır: `05` MK-74 (05:1520), `09` §10.6 madde 1 (09:1031), §21.4 madde 3; 11:257 (T04 KK5) ve 11:5588 (T602 KK7) bunu kurar. | Yok |
| F4 | T03 k6 çalışan örnek uç isteği ister, ilk uç testi T01'dedir ve T01 T03'e bağımlıdır (parça 1 B-2; T01, T03) | ❌ RET (Yüksek → Düşük) | Çelişki yok: T03 k6'nın (11:232) kendi cümlesi "ilk uç testi T01'dedir" der, yani örnek istek bir uç testi değildir; kriterin özü fabrikanın bağlantıyı koleksiyon veritabanına çevirmesidir (09:1991 madde 5; KK6 11:243). Bu, T02'nin boş uygulamasına atılan uç gerektirmeyen bir istekle (tanımsız yolda 404) T01'den önce sınanır. Sıra T02 → T03 → T01 (11:177) ve T01 `Bağımlılık: T02, T03` (11:180), §3.1 yön kuralına (11:121) uyar; önerilen "uçsuz kontrol" ifade tercihidir, ölçü dışı. Önceki turlarda bu iddiaya hüküm yok. | Yok |
| F5 | T201 imha davranışı F3'teki T301–T302'ye dayanıyor, `Bağımlılık`'ta yok (parça 2 B-1; T201) | ❌ RET (tekrar eden: 5. tur F1, 8. tur #7; Orta → Düşük) | Yeni bilgi yok. Fazlar zincirdir, `Bağımlılık` yalnız aynı fazı yazar, sonraki faz task'ı gate'le karşılanır (11:119, K-1408 (1)); T201'in kendi F2 parçaları vardır — gecelik temizlik (06 §9.4.3) ve kayıt dosyaları (06 §9.3.4) — ve KK1–KK9 (11:886–894) onlarla F2'de sınanır. T301/T302'yi F2'ye almak faz kapsamını bozar (sipariş kaydı F3'te doğar). | Yok |
| F6 | T312 k8 "toplamı aşan parça 400" ↔ KK8 "toplamı aşan parça 409" (parça 2 B-2; T312) | ❌ RET (tekrar eden: 1. tur D4 #3, 3. tur D7 #10, 8. tur D1 #1; Yüksek → Düşük) | Yeni bilgi yok (11:1987, 11:1999, 07:4886 değişmedi). Kriter 8 toplam aşımını tek başına 400'e bağlar; KK8'in 409 örneği iki koşulu birden taşıyan parçayı (uymayan başlangıç ve toplam aşımı) sınar ve `07 §4.31.2` denetim sırası "oturum → 404 → 413 → biçim 400 → 409 → boy aşımı 400/422" gereği o parça 409 alır; başlangıcı uyan toplam aşımı KK8'deki "iki 400" ile ayrıca kapsanır. İki hata kodu `07`'de ayrı tanımlıdır (`UploadOffsetMismatch` 409, `UploadSizeExceeded` 400). | Yok |
| F7 | T330 k7 "ETBİS kaydı yapılmaz, satış kapısını tutmaz": ETBİS kaydı yasal yükümlülük (parça 2 B-3; T330, T331) | ❌ RET (tekrar eden: 2. tur F6, 3. tur F3, 4. tur F1, 6. tur #3, 8. tur #9; Yüksek → Düşük) | Yeni bilgi yok; kaynak yine Ticaret Bakanlığı SSS sayfası, `02` ve `10` son kez #265'te değişti. "ETBİS kaydı yapılmaz" ürünün kayıt yapmadığını söyler; kaydı firma yaptırır, ürün kaydı denetleyemez, alan satış kapısını tutmaz (`02` §3.1.4 K-14, K-628; §12.1.4 K-574; `10` ÖK-5). Firmanın satıştan önce kayıt ve değişiklik bildirimi ön koşulu `02 §12.5`'te firmaya atanmış, T601 k9'da (11:5562, ÖK-1…ÖK-12) runbook'a bağlıdır. Satışı ETBİS'e bağlamak `02 §3.1.4`'e aykırı olurdu. | Yok |
| F8 | T336 k8 ürün sayfası "üretim yeri (ülke)" taşır; KK8 yalnız zorunluluğu ve ölçü birimi kurallarını sınar (parça 2 B-4; T336) | KABUL (Orta) | Olgu doğru: k8 (11:2595) gösterimi ister, KK8 (11:2605) alanın zorunluluğunu sınar; gösterimi sınayan başka yer de yok (T503 k5/KK5 fiyat bloğu üretim yerini anmaz, 11:4280/4291; T501 k3 ülke adını yalnız logolu etikette yazdırır, 11:4230). Kaynak gösterimi zorunlu tutar: `02 §3.8.5` (02:605), `04` 5.4.4 (04:3106). Kabul kriteri ↔ KK farkı (4). T337 KK4 aynı yolu izler (11:2626). | T336 KK8 (1 şartname) |
| F9 | T360 KK3 / T322 k1 "arşiv yok" ↔ T363 k2 "arşivdeki adres dolu sayılır" (parça 3 B-1; T360, T363, T322) | ❌ RET (Orta → Düşük) | Çelişki yok; iki cümle aynı kayıt türünden söz etmez. T322 k1 ve T360 KK3'teki "arşiv yok" kurumsal içerik kaydı içindir (03 §1.8.4, 02 §3.27.23); T363 k2 ürün ve kategori adresini anlatır, ürünün Taslak/Yayında/Arşiv durumları vardır (03 §1.8.1; 03:1409). `11`, `02 §3.30.2`'deki cümleyi (02:1010) olduğu gibi izler. | Yok |
| F10 | T368 k1–k2 fesih düğmesi "otuz gün geçmiş" sipariş; taahhüt süresi geçince açılmalı (parça 3 B-2; T368) | ❌ RET (tekrar eden: 5. tur F3, 7. tur #6; Yüksek → Düşük) | Yeni bilgi yok. Resmî metin bu turda mevzuat.gov.tr PDF'inden okundu (Mesafeli Sözleşmeler Yönetmeliği, RG 27/11/2014-29188, değişik 23/8/2022-31932): m.16/1 taahhüt süresi ve mal satışında en çok otuz gün, m.16/2 fesih hakkı — `02`'nin K-499 aktarımıyla birebir. Taahhüt (kargoya verme süresi) aşıldığında fesih T368 k1'de mevcut iptal düğmesiyle zaten açıktır (02 §7.2.3, §3.20.4, §12.1.8; K-198); otuz gün yalnız ikinci yolun (kargodaki sipariş için Z-11 düğmesi) koşuludur (02:1129, 02:1624; K-597). | Yok |
| F11 | T368 başlığındaki "ifanın imkânsızlaşması bildirimi" için kabul kriteri ve test yok (parça 3 B-3; T368) | ❌ RET (Yüksek → Düşük) | Mevzuat okuması doğru (m.16/4: öğrenmeden itibaren üç gün içinde bildirim, bildirimden en geç on dört günde iade), ama üst doküman bu yolu bilinçli olarak firma iptaline bağlar ve ayrı kaydı açıkça eler: `02 §7.2.4` (02:1626) "Firma iptali, ifanın imkânsızlaşmasının yasal yolunu karşılar … 'Ayrı bir imkânsızlık akışı' elendi" (K-609); Z-47 "ürün sayaç tutmaz" (02:1139), üç gün firmanın yükümlülüğüdür (§12.5, 02:2202). Bildirim 11'de karşılanır: T368 k7 (B-7) ve T306 k9; on dört gün iptalden başlar (T370 k3); stok uyarısı T368 k8. Ayrı kayıt eklemek `02`'ye aykırı kod yazdırırdı. | Yok |
| F12 | T368 k6 "sistem faiz hesaplamaz": yasal faizli iade yükümlülüğü (parça 3 B-4; T368, T369, T370) | ❌ RET (tekrar eden: 2. tur F9, 3. tur F5, 4. tur F1, 6. tur #6, 7. tur #7; Yüksek → Düşük) | Yeni bilgi yok; `02`, `04`, `06`, `07` son değişikliği #266'dır, 8. tur T370'te yalnız KK6–KK7'yi değiştirdi, T368 k6'ya dokunmadı. T368 k6 (11:3359) `02 §7.2.3`'ün (02:1624) aynısıdır: "sistem faizi hesaplamaz … faiz firmanın yükümlülüğüdür"; `02 §12.5` tablosu faizi hesaplayıp ödemeyi firmaya yazar (02:2212; K-499, K-571). Önerinin iki kolu da üst dokümanla kapalıdır: `06` "faiz alanı yok" (06:1735), `04` 9.9.14 ve `07` 9.9.14 kanuni faiz hesabını "Yoktur" listesine koyar. m.16/3 faizli iadeyi satıcıya (firmaya) yükler, ürün araçtır; gösterilen kaynak yine 6502 sayılı Kanun PDF'idir. | Yok |
| F13 | T372 k8 kargoya verilmemiş kalemde cayma düğmesi ve kaydı yok, başka kanaldan gelen bildirim "müşteriyle anlaşıldı" iptaliyle işlenir (parça 3 B-5; T372) | ❌ RET (tekrar eden: 3. tur #4, 5. tur #4; Yüksek → Düşük) | Yeni bilgi yok. T372 k8 (11:3465) `02`'nin iki cümlesini birebir tüketir: §7.3.7 (02:1659) kargo öncesi iptal caymanın sonucunu aynen verir, firma onayı beklenmez, para on dört gün içinde iade edilir, başka kanaldan gelen cayma bildirimi iptal olarak işlenir; §6.4.31 (02:1484) on dört gün bildirimin firmaya ulaştığı tarihten işler. "Müşteriyle anlaşıldı" yalnız iptal sebebinin etiketidir, firmaya takdir vermez; "ulaşma zamanını sakla" önerisi T372 k6–k7'de (11:3463–3464) zaten vardır. Yönetmelik m.9/2, m.11/1, m.12/2 ile çelişki yok. | Yok |
| F14 | T508 k2 "kalem fiyatı güncel üründen okunur" ↔ k7 "ilk eklenme fiyatı değişmez" (parça 4 B-1; T508) | ❌ RET (tekrar eden: 2. tur F10, 7. tur #9; Yüksek → Düşük) | Yeni bilgi yok; 11:4407, 11:4412, 02:757 §3.16.5 ve 07:4333 (UC-20) değişmedi. İki cümle iki ayrı kavramdır: kalem fiyatı güncel üründen okunur (K-80); "ilk eklenme fiyatı" yalnız "fiyatı değişti" işaretinin karşılaştırma değeridir ve adet artınca değişmez (K-131); 11:2850 (T346 KK5) bu rolü açıkça tanımlar. Ödeme tutarı güncel üründen gelir; para kaybı doğuran boşluk yok. | Yok |
| F15 | T512 k5 "IBAN alanı yalnız ödenmiş havale siparişindedir" ↔ T529 k10 `CardRefundFailed` sebepli IBAN isteği (parça 4 B-2; T512, T529) | ❌ RET (Yüksek → Düşük) | İki cümle aynı ekranı anlatmaz. T512 k5 (11:4522) iptal ve gecikme feshi ekranının (E-17, UC-38) iptal formundaki IBAN alanıdır ve kaynağı aynen söyler (07:4475; 04 5.17.5); `CardRefundFailed` sonrası IBAN sipariş sayfasında (E-16, 04 5.16.12) açılır (07:5778) ve `SaveRefundIban` ile karşılanır (07:4455). Bu yol 11'de zaten kodlu: T512 k3/KK3 (11:4520, 11:4532), başlıktaki "E-16 IBAN formu" (11:4514), T543 k7 (11:5372). | Yok |

**Tekrar eden RET (turun sonucunu belirlemez):** F1 (2., 5. tur), F3 (2., 3., 4., 6., 7., 8. tur), F5 (5., 8. tur), F6 (1., 3., 8. tur), F7 (2., 3., 4., 6., 8. tur), F10 (5., 7. tur), F12 (2., 3., 4., 6., 7. tur), F13 (3., 5. tur), F14 (2., 7. tur). **Seviye düşenler:** Yüksek → Düşük: F1, F3, F4, F6, F7, F10, F11, F12, F13, F14, F15; Yüksek → Orta: F2; Orta → Düşük: F5, F9. **Kısmi kalış:** F2 (öneri daraltıldı: senaryo testi ve zorunlu geri yükleme provası alınmadı; KK6'ya saklama türünün zaman tipli olduğu yazıldı).

**Çıkış değerlendirmesi (Faz 4 madde 4–5).** Sonuç TEMİZ değil: tek parça ve parça 1–4 bulgu getirdi (parça 5 TEMİZ). Madde 5'in çıkış kuralı işlemez: iki kabulün ikisi de Orta şiddette (kabul kriteri ↔ doğrulama kontrol listesi farkı: kriter sınanmıyor) ve kural yalnız kabullerin hepsi düşük metin farkıysa kapatır. Tekrar eden RET kuralı F1, F3, F5, F6, F7, F10, F12, F13, F14'ü turun sonucundan ayırır. **Gözlem (Faz 4 madde 6):** 8. turun üç Orta kabulü (T118 KK1, T224 k2/KK2, T370 KK6/KK7 ile T390 KK8) ve bu turun iki Orta kabulü (T07 KK6, T336 KK8) aynı sınıftadır: kabul kriteri ↔ doğrulama kontrol listesi (KK) farkı — kriterin söylediği bir sonuç hiçbir KK'de sınanmıyor. Sınıf iki ardışık turda yinelendiği için **sıradaki adım hedefli mercektir** (KD-cross-m; görev dosyası "Hedefli mercek"): task bloklarının kendi içindeki kriter ↔ KK ↔ test beklentisi farkı faz dilimleriyle beşli dalgada aranır; hedefli merceğin kabulleri uygulandıktan sonra kalite döngüsü 10. turla sürer.

**F2 ve F8 — uygulama ve gözlem.** İki çift `11_audit_uygula.py` ile uygulandı. T07 KK6 artık "pgBackRest saklaması zaman tipli otuz gün, sayı tipli değil; kova kuralı yalnız eski sürümü siler" der (blok 2 551 bayt, sınır 2 560); T336 KK8'e "ürün sayfası ülkeyi fizikselde ve dolu dijitalde gösterir, boş dijitalde ülke satırı yok" eklendi (blok 2 417 → 2 516 bayt). Yazım kapanışı, matris kapsama, task blok denetimi (`--tam`), geri yön denetimi ve geri işaret taraması sorun 0.

## 4. Ek bulgular

- D8 doğrulayıcısı, F8'in şartnamesine katmadığı ayrı bir aday bildirdi: `02 §3.8.5`'e göre ürün sayfası fiyatın uygulanmaya başlandığı tarihi de gösterir; `11`'de bu gösterimi sınayan bir KK yok ("başlandığı tarih" araması sonuç vermedi). Bu tur ikinci modelin bulgusu değildir ve ölçü sınıfı (kabul kriteri ↔ KK) hedefli merceğin kapsamındadır; hedefli mercek paketine aday olarak verilir, bu turda uygulanmadı.
- Doğrulayıcılardan `11`'i ya da üst dokümanları etkileyen başka bir tutarsızlık bildirilmedi. Üst doküman notu yok; `00`/`CLAUDE.md`/`SETUP.md` önerisi yok.

## 5. Kullanıcı onay checklist'i

- [ ] F1 — RET, tekrar eden (uygulanmadı)
- [ ] F2 — KISMİ (uygulandı: T07 KK6)
- [ ] F3 — RET, tekrar eden (uygulanmadı)
- [ ] F4 — RET (uygulanmadı)
- [ ] F5 — RET, tekrar eden (uygulanmadı)
- [ ] F6 — RET, tekrar eden (uygulanmadı)
- [ ] F7 — RET, tekrar eden (uygulanmadı)
- [ ] F8 — KABUL (uygulandı: T336 KK8)
- [ ] F9 — RET (uygulanmadı)
- [ ] F10 — RET, tekrar eden (uygulanmadı)
- [ ] F11 — RET (uygulanmadı)
- [ ] F12 — RET, tekrar eden (uygulanmadı)
- [ ] F13 — RET, tekrar eden (uygulanmadı)
- [ ] F14 — RET, tekrar eden (uygulanmadı)
- [ ] F15 — RET (uygulanmadı)

⚠ karar: yok. Önerilen `00`/`CLAUDE.md`/`SETUP.md` değişikliği yok.
