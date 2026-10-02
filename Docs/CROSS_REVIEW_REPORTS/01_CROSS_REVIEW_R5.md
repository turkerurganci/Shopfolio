# Cross-Review — 01 Project Vision (Tur 5)

**Tarih:** 2026-10-02 · **Hedef:** `Docs/01_PROJECT_VISION.md` v0.13 · **İkinci model:** `cursor-agent` (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu, yeni ve izole bir çalışma alanında; model önceki turları görmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde). Model son bulguyu ve sonuç satırını cevabın başında bir kez daha tekrar etti.

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §3.2 — Misafir alıcı / “Neden geri döner”
> Alıntı: "Geri dönmesi beklenmez: tanımı gereği hesap sürekliliği yoktur — bir kez alır, gider (K-100). ... Siteye o siparişin durumunu görmek için döner."
> Sorun: Aynı hücre hem geri dönüşün beklenmediğini hem de sipariş takibi için siteye dönüldüğünü söylüyor. “Neden kullanır” satırı da takip etmeyi misafir kullanımına yazdığı için sütunun cevabı kendi içinde ikiye ayrılıyor.
> Öneri: “Neden geri döner” hücresini tek tutarlı çerçeveye indir: hesap sürekliliği yoktur; geri dönüş yalnızca sipariş durumu görüntüleme (ve isteğe bağlı yeni bağımsız sipariş) içindir. “Bir kez alır, gider”i bu sınırlı geri dönüşle çelişmeyecek biçimde yeniden yaz.
> 
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §8 — karar kuralı ve V-2
> Alıntı: "her satırın \"Nasıl doğrulanacak\" hücresi neye bakılacağını ve neyin görülmesi hâlinde varsayımın yanlışlanmış sayılacağını yazar (K-487)" / "günde ortalama 10, tepe günde 50 sipariş" / "Ortalama günlük hacim (10) yanlışlama koşulu değil"
> Sorun: §8 her varsayım satırında yanlışlama koşulu ister; V-2 varsayımı “ortalama 10”u içerir ama bu bileşen için yanlışlama tanımlamaz. Ortalama 2 veya 30 olsa da (tepe ≤50 kaldıkça) varsayım yanlışlanmış sayılmaz.
> Öneri: Ya “ortalama 10”u varsayım metninden çıkarıp yalnız tepe 50’yi bırak; ya da ortalama için ayrı, ölçülebilir bir yanlışlama eşiği yaz (ör. üç aylık ortalama günlük siparişin belirli bir aralığın dışında kalması).
> 
> BULGU-3
> Kriter: Belirsizlik
> Seviye: Orta
> Yer: §1, §3.1 ve §8 V-1
> Alıntı: "fiziksel üründe birkaç yüz ürüne kadar katalog" / "yayındaki fiziksel ürün sayısı 1.000'i aşarsa — \"birkaç yüz\" ile 1.000 arası varsayımın içinde sayılır"
> Sorun: “Birkaç yüz” düz dilde yüzlerce ürünü (kabaca 200–500) anlatır; doğrulama kuralı ise 999’a kadarını varsayım içinde sayar. Ölçek iddiası ile yanlışlama eşiği aynı büyüklüğü işaret etmiyor.
> Öneri: Varsayım cümlesini eşiğe hizala (ör. “1000’in altında fiziksel ürün”) veya eşiği “birkaç yüz”ün üst sınırına çek; “birkaç yüz” ile 1000 arasını varsayımın kendisi sayma.
> 
> BULGU-4
> Kriter: Edge case
> Seviye: Orta
> Yer: §6 — ölçüm penceresi ve M-4
> Alıntı: "üç ay, satışın o kurulumda ilk açıldığı gün başlar (K-486)" / "cayma penceresi on dört gün (K-203), geri ödeme çatısı on dört gündür (K-209); bir aylık ölçüm iade oranını yarım gösterirdi" / "dönem içinde ödemesi tamamlanmış siparişlerden en az bir kalemi iptal edilen ya da iade alanların oranı"
> Sorun: Üç aylık pencerenin gerekçesi iade takvimidir; M-3 paydayı “süresi dönem içinde dolan” siparişlerle olgunlaştırır, M-4 ise dönem sonundaki ödemesi tamamlanmış siparişleri cayma/iade olgunlaşma payı olmadan oranlar. Dönemin son 14–28 gününde verilen siparişlerin iadesi pencere dışında kalırsa M-4 düşük okunur; bu kenar durum tanımlı değildir.
> Öneri: M-4 için M-3’tekine benzer olgunlaşma kuralı yaz (ör. dönem siparişleri, dönemin bitiminden sonra cayma+iade çatısı kadar beklenerek sayılır) veya paydayı “iade/cayma penceresi kapanmış siparişler” diye sınırla.
> 
> BULGU-5
> Kriter: Tutarlılık
> Seviye: Düşük
> Yer: §6 — M-3
> Alıntı: "Sipariş işleme süresi — kargoya verme sözüne uyum oranı." / "ölçünün değeri bir süre değil, orandır."
> Sorun: Kriter adı “işleme süresi” ile başlar; tanım değerin süre değil oran olduğunu söyler. Ad ile ölçü çelişir.
> Öneri: Adı yalnız oran diline çek (ör. “Kargoya verme sözüne uyum oranı”) ve “süresi” ifadesini başlıktan kaldır.
> 
> BULGU-6
> Kriter: Eksiklik
> Seviye: Düşük
> Yer: §4 — Değer önerisi (şablon: her aktör)
> Alıntı: "Misafir alıcıya ayrı bir değer satırı açılmaz: tanımı gereği sürekliliği yoktur ve yazılabilecek tek içerik — hesap açmadan satın alabilmesi — üyeliğin zorunlu olmaması kararının kendisidir (K-97, K-100)."
> Sorun: Şablon her aktör için “bu ürün olmasaydı ne olurdu” ister; §3.2’de dördüncü aktör olan misafir alıcı için bu cümle/satır yoktur. Bilinçli dışlama gerekçesi yazılmış olsa da şablon beklentisi karşılanmamış kalır.
> Öneri: Misafir için tek satırlık türev değer yaz (olmasaydı: kayıt engeline takılır / Shopfolio: hesapsız alım + numara+e-posta ile takip) veya misafiri aktör tablosundan çıkarıp üye-olmayan sipariş durumu olarak yeniden konumlandır.
> 
> SONUÇ: 6 BULGU
> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ✅ KABUL | Tur 4'te eklenen "beklentidir, kural değildir" cümlesi hücreyi düzeltmek yerine ikinci bir çerçeve ekledi; hücre hâlâ "geri dönmesi beklenmez" diye başlayıp "döner" diye bitiyordu. | Misafir alıcı: "Hesap sürekliliği yoktur — K-100'ün deyişiyle bir kez alır, gider. Siteye döndüğünde iki şeyden biri için döner: o siparişin durumunu görmek ya da öncekilere bağlanmayan yeni, bağımsız bir sipariş vermek (K-97, K-100)." |
| BULGU-2 | ⚠️ KISMİ | Ortalama için ayrı bir eşik yazılmaz: K-474 ortalamayı açıkça "tetikleyici değil, V-2'nin doğrulama ölçüsü" saydı. Ortalamanın 50'yi aşıp tepenin aşmaması mümkün değildir; ortalamanın çok düşük kalması V-4'ün yanlışlama koşuludur. Eksik olan bu ilişkinin yazılmamasıydı. | V-2: "Ortalama günlük hacim (10) varsayımın ölçüsüdür ama ayrı bir yanlışlama koşulu taşımaz: tasarımı zorlayan tepe gündür, ortalamanın çok düşük kalması ise V-4'ün konusudur." |
| BULGU-3 | ⚠️ KISMİ | Tur 4'teki BULGU-6'nın tekrarı. Varsayım 1.000'e, eşik 500'e çekilmez: K-03 "birkaç yüz" der ve K-487 eşiği bilinçli olarak bir büyüklük mertebesi üstüne koydu. Tur 4'te eklenen "varsayımın içinde sayılır" ifadesi ise gerçekten yanıltıcıydı — eşiğin altı, varsayımın doğrulandığı anlamına gelmez, yalnız yanlışlanmadığı anlamına gelir. | V-1: ara bölge cümlesi kaldırıldı; yerine "Eşik 'birkaç yüz'ün bilinçli olarak bir büyüklük mertebesi üstündedir: yanlışlama varsayımdan her sapmayı değil, katalog ekranlarının yeniden düşünülmesini gerektiren büyüklüğü ölçer (K-487)." |
| BULGU-4 | ✅ KABUL | Gerçek bir kenar durumu: K-418 üç aylık pencereyi iade takvimiyle gerekçelendirdi, ama pencerenin son haftalarında verilen siparişlerin cayma penceresi ve geri ödeme çatısı pencere dışında kapanıyor; M-4 o siparişler için eksik okunur. Hiçbir karar okunma anını yazmıyordu. | M-4 "Ne zaman ölçülür": "dönem bittikten 28 gün sonra okunur — cayma penceresi (K-203) ve geri ödeme çatısı (K-209) dönemin son siparişleri için de kapanmış olsun diye (K-488)". K-488 öneriyle kaydedildi; `02 §10.6.3`'e yansıtıldı. |
| BULGU-5 | ✅ KABUL | Tur 3'te ad ile ölçü birleştirilmişti ama ad hâlâ "süre" diye başlıyordu. Ölçünün adı `02 §10.6.3`'te zaten "kargoya verme sözüne uyum oranı"dır; K-416'nın adı izlenebilirlik için parantezde kalır. | M-3: "**Kargoya verme sözüne uyum oranı** (K-416'nın 'sipariş işleme süresi')." |
| BULGU-6 | ⚠️ KISMİ | Misafir alıcıya §4'te değer satırı açılmaz: K-100 bunu adıyla eledi ("§4.4 açılsın" seçeneği) ve K-19'un üç aktörlü ağırlık merkezini korudu. Önerinin ikinci yolu — misafiri aktör tablosundan çıkarmak — K-97'ye aykırıdır. Şablonun sorusu ise tek cümleyle, satır açmadan cevaplanabilir. | §4 ağırlık merkezi paragrafına: "Şablonun 'bu ürün olmasaydı ne olurdu' sorusunun misafir alıcı için cevabı tek cümledir: satın almadan önce bir kayıt duvarına takılırdı — o duvarı K-97 kaldırır." |

**Dağılım:** 3 KABUL · 3 KISMİ · 0 RET. BULGU-3, BULGU-5 ve BULGU-6 önceki turlarda düzeltilmiş yerlere döndü; her birinde önerinin yalnız karar kaydıyla uyuşan kısmı alındı.

## 3. Ek bulgular

Yok.

## 4. Kullanıcı onay checklist'i

Oturum açılışındaki talimat gereği uygulandı (`01` v0.14). Bir karar satırı açıldı: K-488 (öneriyle kaydedildi) — M-4'ün okunma anı.

- [x] BULGU-1 · [x] BULGU-2 · [x] BULGU-3 · [x] BULGU-4 · [x] BULGU-5 · [x] BULGU-6
