# Cross-Review — 01 Project Vision (Tur 10)

**Tarih:** 2026-10-02 · **Hedef:** `Docs/01_PROJECT_VISION.md` v0.18 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra` (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu, yeni ve izole bir çalışma alanında; model önceki turları görmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde).

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Düşük
> Yer: Başlangıç sürüm/tarihçe notu, “Yazım turu (2026-10-01, v0.6 — K-432)” / “Anlamsal K-29 kaçakları”
> Alıntı: "yirmi iki karar §1–§5'e işlendi"
> Sorun: Ardından sıralanan kararlar 22 değil 24 adettir: §1’de 2, §2’de 2, §3’te 8, §4’te 8 ve §5’te 4 karar vardır.
> Öneri: “Yirmi iki” ifadesini “yirmi dört” olarak düzeltin.
> 
> BULGU-2
> Kriter: Teknik doğruluk
> Seviye: Orta
> Yer: §6, Ü-1
> Alıntı: "dış ön koşullar: alan adı (K-20), barındırma ve konumu (K-358), e-posta gönderim yapılandırması (K-378), firmanın ödeme sağlayıcı sözleşmesi (K-12), ETBİS kaydı (K-14)"
> Sorun: ETBİS kaydı tüm firma tipleri için koşulsuz kurulum ön koşulu yapılmıştır; oysa hedef segment açıkça esnafı da kapsar ve esnaf/sanatkârlar ETBİS kayıt yükümlülüğünden istisna olabilir. Bu kural, hukuken kayıt zorunluluğu olmayan bir firmanın Ü-1 kabulünü gereksiz yere engeller.
> Öneri: ETBİS’i “mevzuat uyarınca kayıt yükümlülüğü bulunan firma için ETBİS kaydı” şeklinde koşullu yazın.
> 
> BULGU-3
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §3.2, “Misafir alıcı” satırı; §3.2, “Aktör olmayanlar” ikinci madde; §7, S-6
> Alıntı: "Hesap açmadan sipariş veren tüketici"
> Sorun: Doküman misafir alıcıyı ve genel olarak alıcıyı tüketici olarak tanımlarken, ticari amaçla alan kişinin de aynı akıştan geçebileceğini ve tüketici sayılıp sayılmayacağını mevzuatın belirlediğini kabul eder. Buna rağmen S-6 “Alıcı tüketicidir” ve ürünün “tek hukuki rejimi tüketici rejimidir” der. Ticari alıcıya fiilen izin veren bir ürün, her alıcının hukuken tüketici olduğu veya 6502 korumalarının her siparişte geçerli olduğu iddiasını taşıyamaz.
> Öneri: Aktör tanımlarında “tüketici” yerine “alıcı” kullanın; tüketici korumalarının yalnız hukuken tüketici sayılan alıcılara kanunen uygulanacağını, diğer alıcıların ise aynı ürün akışını kullanabildiğini açıkça yazın.
> 
> BULGU-4
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §3.2, “Misafir alıcı” satırı
> Alıntı: "Hesap açarsa doğrulanmış e-postasına ait geçmiş misafir siparişleri hesabına düşer"
> Sorun: E-posta adresinin bugün doğrulanması, geçmişteki siparişi veren kişinin aynı kişi olduğunu kanıtlamaz. E-posta hesabının devri, yeniden atanması veya ele geçirilmesi hâlinde yeni hesap sahibi geçmiş sipariş, adres ve iletişim verilerine erişebilir.
> Öneri: Geçmiş misafir siparişlerini otomatik bağlamayın; her sipariş için ayrıca siparişe özgü doğrulama/onanım isteyin veya yalnız hesap açıldıktan sonraki siparişleri hesaba bağlayın.
> 
> BULGU-5
> Kriter: Edge case
> Seviye: Orta
> Yer: §8, V-7
> Alıntı: "ikisinden biri yoksa yanlışlanır"
> Sorun: Doğrulama kuralı, firmanın tanıtım ve satış için iki ayrı sistemi bulunmasına rağmen aynı bilgiyi iki yere girmediği veya bunun yükünü taşımadığı durumu tanımlamaz. Bu durumda varsayımın doğrulanmış mı, yanlışlanmış mı sayılacağı belirsizdir.
> Öneri: Karar kuralını tüm koşulları kapsayacak şekilde yazın: iki sistem, aynı bilginin çift girişi ve bunun operasyonel yükünün her birinin gözlemlenmesi halinde doğrulansın; bunlardan herhangi birinin yokluğunda yanlışlansın.
> 
> SONUÇ: 5 BULGU
> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ⚠️ KISMİ | Sayım doğrudur: listede yirmi dört kayıt var ama K-81 ve K-245 ikişer bölümde geçiyor; ayrı karar sayısı yirmi ikidir. Tarihçe notu kendi anını doğru anlatıyor, değiştirilmez; okuyucunun aynı hesabı yapmaması için tek bir açıklama eklendi. | v0.6 notu: "… yirmi iki karar §1–§5'e işlendi (K-81 ve K-245 ikişer bölümde geçtiği için liste yirmi dört kayıt taşır)" |
| BULGU-2 | ✅ KABUL | Ü-1 ETBİS kaydını koşulsuz sayıyordu. Ürün ETBİS kaydını denetlemez ve satışı engellemez (`10` ÖK-5); kayıt firmanın yükümlülüğüdür (K-14). Hangi firmanın yükümlü olduğu mevzuatın işidir; esnafın kayıt yükümlülüğünün kapsamını doğrulayamadım, ama koşullu ifade iki durumda da doğrudur. | Ü-1: "ETBİS kaydı — kayıt yükümlülüğü olan firmada (K-14)" |
| BULGU-3 | ✅ KABUL | Tur 9'un düzeltmesinin doğal sonucu: alıcının hukuki sıfatını mevzuat belirler dedikten sonra aktör tablosu ve S-6 alıcıyı hâlâ "tüketici" diye tanımlıyordu. `02 §1.2` sözlüğü de iki aktörü "alıcı" diye tanımlıyor; deep review'daki ICT-7 bunu isteğe bağlı iyileştirme diye bırakmıştı. Kalıcı sınır değişmedi: ürün tüketiciye satış için kurulmuştur ve tek akışı tüketici akışıdır (K-07). | Misafir alıcı ve üye müşteri "alıcı" diye tanımlandı; S-6: "Ürün tüketiciye satış için kurulmuştur … ürünün tek akışı tüketici akışıdır ve ticari alıcı için ayrı bir akış yoktur (K-07, K-112)" |
| BULGU-4 | ❌ RET | Tur 8'in BULGU-3'ünün tekrarı. Proje sahibi bu riski aynı oturumda değerlendirdi ve kuralın kalmasına, riskin bilinçle kabul edilip `02 §3.13.2`'ye yazılmasına karar verdi (K-489). Önerilen "otomatik bağlamayı kaldır" seçeneği K-489'da adıyla elendi. İkinci model kararı göremediği için bulguyu yeniden üretti; bu yüzden riskin kabul edildiği `01`'de de bir cümleyle anıldı. | Misafir alıcı satırına: "adres el değiştirmişse eski siparişlerin yeni sahibine görünmesi bilinçle kabul edilmiş bir risktir (K-489)" |
| BULGU-5 | ⚠️ KISMİ | Kural aslında tanımlıydı — iki koşuldan biri yoksa yanlışlanır — ama varsayımın üçüncü öğesi ("yükünü taşır") kuralda adıyla geçmiyordu. Yük, §1'de tanımlandığı gibi çift giriştir; ayrı bir koşul değildir. Kural iki soruyla yeniden yazıldı. | V-7: "iki soru sorulur — … iki ayrı sistemde mi yürütüyordu, aynı bilgiyi iki yere mi giriyordu (§1'in yükü budur). İkisine de 'evet' derse doğrulanır; herhangi birine 'hayır' derse yanlışlanır" |

**Dağılım:** 2 KABUL · 2 KISMİ · 1 RET.

> **Sonradan güncellendi (2026-10-03, K-628):** BULGU-2'nin kabulü esnafın kayıt yükümlülüğünü doğrulayamadan koşullu ifadeyi seçmişti. `10`'un deep review'ında ETBİS Tebliği m.5'in resmî metni okundu: kendine ait elektronik ticaret ortamında faaliyet gösteren hizmet sağlayıcı faaliyete başlamadan önce kaydolur ve metinde esnaf için muafiyet yoktur — m.5/2-a esnafın kaydolduğu bilgileri sayar. Bulgunun "esnaf istisna olabilir" dayanağı çürüdü. Ü-1'in niteleyicisi "kayıt yükümlülüğü olan firmada" yerine "satış yapacak firmada" oldu (`01` v0.31): satışı hiç açmayan kurulum elektronik ticaret yapan hizmet sağlayıcı değildir. Kabulün sonucu — ürün kaydı denetlemez ve satışı engellemez — değişmedi.

## 3. Ek bulgular

Yok.

## 4. Kullanıcı onay checklist'i

Oturum açılışındaki talimat gereği uygulandı (`01` v0.19). Yeni karar satırı açılmadı.

- [x] BULGU-1 · [x] BULGU-2 · [x] BULGU-3 · [x] BULGU-4 · [x] BULGU-5
