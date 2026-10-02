# Cross-Review — 01 Project Vision (Tur 8)

**Tarih:** 2026-10-02 · **Hedef:** `Docs/01_PROJECT_VISION.md` v0.16 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra` (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu, yeni ve izole bir çalışma alanında; model önceki turları görmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde). **İkinci model bu turda değişti:** `cursor-agent` Cursor hesabının kullanım limitine takıldı; proje sahibi ChatGPT hesabıyla devam edilmesini istedi. Codex CLI (`codex exec`, ChatGPT girişi, `gpt-5.6-terra`, düşünme düzeyi yüksek), boş bir klasörde, salt okunur ve kişisel ayarlar yüklenmeden koştu. Doküman, şablon ve talimat istemin içinde verildi; karar kaydı verilmedi. `gpt-5.6-sol` denendi, hesap bu modeli artık açmıyor.

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §1 “Bugün nasıl çözülüyor ve neden yetersiz”, §5.2 “Ismarlama yazılım”
> Alıntı: “Mevcut seçeneklerin hiçbiri iki ihtiyacı tek üründe eşit ağırlıkta karşılamaz (K-04).” / “İki ihtiyacı da tam karşılayan, firmaya özel çözüm”
> Sorun: §1 hiçbir mevcut seçeneğin iki ihtiyacı birlikte karşılamadığını söylerken, §5.2 ısmarlama yazılımın ikisini de tam karşıladığını söyler.
> Öneri: §1’deki ifadeyi bütçe erişilebilirliğiyle sınırlandırın: örneğin “KOBİ bütçesinde erişilebilir mevcut seçeneklerin hiçbiri...” deyin.
> 
> BULGU-2
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §3.2 “Aktör olmayanlar — Ticari / kurumsal alıcı”
> Alıntı: “ürün 6502 sayılı Kanun'un tüketici korumalarını, alıcının sıfatına bakmaksızın her siparişe uygular”
> Sorun: Bir alıcının tüketici niteliği ürünün verdiği etikete değil, işlemi ticari veya mesleki amaçla yapıp yapmadığına bağlıdır. Ürün politikasıyla 6502’nin kapsamı tüm siparişlere genişletilemez; metin bunu sonraki cümlede kabul etse de operasyonel sonucu tanımsız bırakır.
> Öneri: “Tüketici benzeri sözleşmesel haklar uygular” olarak ayırın; ticari amaçlı alıcı ortaya çıktığında hukuki sınıflandırmanın ve fatura/iptal akışının nasıl ele alınacağını açıkça tanımlayın.
> 
> BULGU-3
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §3.2, Misafir alıcı
> Alıntı: “Hesap açarsa doğrulanmış e-postasına ait geçmiş misafir siparişleri hesabına düşer”
> Sorun: E-posta doğrulaması yalnız adresin mevcut sahibini doğrular; adres devredilmiş veya yeniden atanmışsa yeni sahip, önceki alıcının sipariş geçmişine ve kişisel verilerine erişebilir.
> Öneri: Misafir siparişlerini otomatik bağlamayın; her sipariş için alıcının elindeki siparişe özgü ek bir sırla açık talep/bağlama doğrulaması isteyin.
> 
> BULGU-4
> Kriter: Edge case
> Seviye: Orta
> Yer: §2, §6 M-3
> Alıntı: “üçü de aynı kataloğa girer, aynı sepetten geçer ve aynı şekilde satın alınır” / “Fiziksel siparişte ... Dijital ve hizmet siparişinde kargoya verme olmadığı için bu ölçüye girmez”
> Sorun: Fiziksel, dijital ve hizmet kalemlerinin aynı siparişte bulunduğu durumlarda M-3’ün hesap birimi belirsizdir. Karışık siparişin tek sipariş olarak mı, yalnız fiziksel satırlarıyla mı, yoksa her fiziksel kalem için ayrı mı sayılacağı ölçüm sonucunu değiştirir.
> Öneri: M-3’ü fiziksel sipariş satırı veya fiziksel sevkiyat birimi üzerinden tanımlayın; karışık siparişlerin ve farklı kargoya verme sözü taşıyan kalemlerin nasıl sayılacağını ekleyin.
> 
> BULGU-5
> Kriter: Eksiklik
> Seviye: Orta
> Yer: §6 “Mağaza düzeyi”
> Alıntı: “Mağaza düzeyi ölçüler hedef değer taşımaz”
> Sorun: Şablonun başarı kriterleri için istediği “başarılı sayılması için ne olmalı?” sorusunun sayısal cevabı M-1–M-4 için yoktur. Bu satırlar başarı kriteri değil, hedefsiz izleme ölçüleridir.
> Öneri: M-1–M-4 için sayısal başarı/eşik değerleri belirleyin veya bunları §6’dan “post-MVP izleme ölçüleri” olarak ayrı bir bölüme taşıyın.
> 
> SONUÇ: 5 BULGU
> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ✅ KABUL | §1 "hiçbiri karşılamaz" diyor, §5.2 ise ısmarlama yazılımın ikisini de karşıladığını yazıyor. Ayıran eksen bütçedir (K-21); §1'in sonraki maddesi de bunu söylüyor ama ana cümle sınırsızdı. | §1: "KOBİ bütçesine sığan mevcut seçeneklerin hiçbiri …" |
| BULGU-2 | ⚠️ KISMİ | Aynı konu tur 2 ve tur 4'te de geldi ve cümle her seferinde daraldı. Bugünkü hâli zaten bunun bir ürün politikası olduğunu ve kanunun kapsamını genişletmediğini söylüyor; ticari amaçla alanın siparişinin nasıl işlediği de yazılı (tüketici siparişi olarak, kurumsal fatura alanı olmadan — K-112). Önerilen "ticari alıcı ayrı akış" bir varlık kararıdır ve K-07'ye aykırıdır. Kalan tek pürüz "uygular" fiiliydi: kanunu ürün uygulamaz, korumaları tanır. | §3.2: "ürün, 6502 sayılı Kanun'un tüketiciye tanıdığı korumaları alıcının sıfatına bakmaksızın her siparişte tanır" |
| BULGU-3 | ⚠️ KISMİ | Risk gerçek: e-posta doğrulaması adresin bugünkü sahibini kanıtlar. Ama önerilen çözüm (otomatik bağlamayı kaldırmak) K-98'i değiştirir ve kişisel veriye dokunur — ⚠ grubu, proje sahibine soruldu (Soru 5). Karar: kural kalır, risk bilinçle kabul edilir. | K-489 (proje sahibinin kararı): `02 §3.13.2`'ye kalan risk cümlesi. `01` değişmedi. |
| BULGU-4 | ✅ KABUL | Karışık siparişte M-3'ün birimi yazılı değildi. Kayıt cevabı veriyor: sipariş tek parça gider (`02 §3.20.3`) ve siparişin sözü fiziksel kalemlerinin en uzun süresidir (K-140); birim sipariştir. | M-3: "Birim sipariştir: sipariş tek parça gönderilir ve sözü fiziksel kalemlerinin en uzun süresidir; karışık siparişin fiziksel hattı bir kez sayılır (K-140)." |
| BULGU-5 | ⚠️ KISMİ | Tur 1'deki BULGU-3'ün tekrarı. Sayısal hedef konmaz (K-418; tablo biçimi K-433) ve ölçüler §6'dan ayrı bölüme taşınmaz — K-433 iki ekseni bilinçli olarak tek tabloda tuttu. Ama "başarı kriteri" başlığı altında hedefsiz bir ölçü okuru şaşırtıyor; adı konarak giderildi. | §6: "Mağaza düzeyi ölçüler bir başarı eşiği değil, post-MVP sıralamasını besleyen **izleme ölçüleridir** ve hedef değer taşımaz" |

**Dağılım:** 2 KABUL · 3 KISMİ · 0 RET.

## 3. Ek bulgular

Yok.

## 4. Kullanıcı onay checklist'i

Oturum açılışındaki talimat gereği uygulandı (`01` v0.17). Bir karar satırı açıldı: K-489 — proje sahibinin kararı (⚠ grubu: kişisel veri), işaretsiz.

- [x] BULGU-1 · [x] BULGU-2 · [x] BULGU-3 · [x] BULGU-4 · [x] BULGU-5
