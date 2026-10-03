# Cross-Review — 02 Product Requirements (Tur 14)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/02_PRODUCT_REQUIREMENTS.md` v0.29 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde; satır sonlarındaki boşluklar atıldı). İstem önceki turlarınkiyle aynıdır: yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: Edge case
> Seviye: Yüksek
> Yer: §3.23 Sipariş anında donan değerler
> Alıntı: "Sipariş kalemi, ödenen tutarı üreten her değeri ve ürünün o anki tanımını dondurur"
> Sorun: Donan değerler arasında ürün tipi, cayma istisnası ve hizmetin ifa süresi açıkça yer almıyor. Firma bunları sonradan değiştirirse, mevcut siparişin fiziksel/dijital/hizmet teslim hattı, cayma rejimi ve hizmet taahhüdü güncel üründen mi yoksa sipariş anındaki hâlden mi okunacağı belirsiz kalır.
> Öneri: Sipariş kalemine ürün tipi, cayma istisnası ve sebebi, hizmet ifa süresi ile teslimi etkileyen diğer ürün niteliğinin donduğunu açıkça ekleyin; sonraki ürün değişikliklerinin yalnız yeni siparişleri etkilediğini belirtin.
>
> BULGU-2
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §3.14.6 Adresin alanları
> Alıntı: "Posta kodu ve T.C. kimlik numarası istenmez"
> Sorun: T.C. kimlik numarasının hiçbir siparişte toplanmaması ve muhasebe aracında sabit `11111111111` kullanılması, tutara veya alıcının statüsüne göre kimlik bilgisinin zorunlu olabildiği fatura senaryolarını karşılamaz. Dokümanın fatura dışa aktarımında da bu bilgi yoktur; firma gerekli faturayı kesemeyebilir.
> Öneri: Güncel mevzuattaki eşik ve alıcı koşullarına bağlı bir kural ekleyin. Kimlik bilgisi gerektiğinde ödeme adımında gerekçesiyle istenmeli, siparişin fatura verisine dondurulmalı ve dışa aktarıma eklenmelidir; gerekmediği durumlarda mevcut veri asgariliği yaklaşımı korunabilir.
>
> BULGU-3
> Kriter: Güvenlik
> Seviye: Orta
> Yer: §3.13.14 Hesabın e-posta adresi
> Alıntı: "E-posta adresi, yeni adres doğrulanmadan değişmez."
> Sorun: E-posta değişikliği tamamlandığında eski adrese güvenlik bildirimi veya hesap kurtarma yönlendirmesi tanımlanmamış. Yeniden doğrulama saldırı riskini azaltır; ancak ele geçirilmiş aktif oturumla yapılan değişikliği eski adres sahibi fark edemez. Aynı eksik yönetici hesapları için de geçerlidir.
> Öneri: E-posta değişikliğinde eski ve yeni adrese, gizli veri içermeyen bir güvenlik bildirimi gönderin; eski adres bildirimine yetkisiz değişiklik için destek/kurtarma yolu ekleyin. Değişiklikte diğer açık oturumların sonlandırılması kuralını da netleştirin.
>
> SONUÇ: 3 BULGU
> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ✅ KABUL | Doğru. K-77 *"kalem canlı üründen hiçbir şey okumadan kendini anlatır"* dedi, ama §3.23.1'in saydığı liste yalnız tutarı üreten değerleri ve ürünün görünen tanımını içeriyordu. Teslim hattını belirleyen tip (K-81), cayma rejimini belirleyen istisna işareti ve sebebi (§7.3.4; K-206, K-509) ve hizmetin ifa süresi (§3.2.2; K-503) listede yoktu. Z-39 süreyi *"ürünün ifa süresi"* diye canlı üründen okutuyordu. Kargoya verme sözü için aynı kural zaten vardı (K-140). Müşterinin onayladığı Ön Bilgilendirme Formu bu değerleri yazar ve formun sürümü siparişe donar. Kalem farklı bir değerle işlerse sonradan konan bir istisna işareti müşterinin cayma hakkını geriye dönük düşürebilirdi. | K-598 `(öneriyle kaydedildi — ⚠)`. Kalem ürünün tipini, istisna işaretini, sebebini ve koşulunu, hizmette ifa süresini de dondurur. Sonraki değişiklik yalnız yeni siparişlere işler. Güncellenen yerler: §1.2 (Sipariş kalemi), §3.2.2, §3.23.1, §4.2 Z-39, §7.3.4. |
| BULGU-2 | ❌ RET | Önerinin dayanağı yanlış. Vergi mükellefi olmayan nihai tüketiciye düzenlenen faturada T.C. kimlik numarası zorunlu değildir ve numara için tutar eşiği yoktur. Mevzuattaki tutar eşikleri faturanın e-Arşiv olarak düzenlenme zorunluluğuna aittir, alıcının numarasına değil. "Alıcının statüsü" kısmını bilinçli kararlar kapatır: ürünün ticari alıcıya özel akışı yoktur (K-07, §12.1.2), fatura ürünün dışındadır (K-546, §12.1.10). Numaranın toplanmaması 1. turda K-561 ile kayda geçti; 7. turda da aynı itiraz geldi ve o turda yalnız `11111111111` uygulaması metne yazıldı. **Konunun dönmesinin sebebi metindeki boşluktu:** §3.14.6 tutar eşiği olmadığını ve vergi numarasıyla fatura isteyen alıcının ne yapacağını yazmıyordu. | Karar değişmedi, yeni karar satırı açılmadı. §3.14.6'ya iki cümle eklendi: numara için tutar eşiği yoktur, tutar eşikleri e-Arşiv zorunluluğuna aittir. Faturasını vergi numarasıyla isteyen ticari alıcının bilgisini firma ürün dışında alır ve faturayı muhasebe aracında düzenler (K-07, K-546). |
| BULGU-3 | ⚠️ KISMİ | Sorun gerçek. K-109 yeniden doğrulamayı açık kalmış bir oturumun devrine karşı koydu. Ama şifreyi bilen biri e-postayı değiştirdiğinde hesabın sahibi bunu öğrenmiyordu. Değişiklikten sonra şifre sıfırlama bağlantısı da yeni adrese gidiyor, yani hesap kalıcı olarak el değiştiriyordu. Panelde müşteri hesabını düzenleyen bir araç yok (yönetici müdahaleleri yalnız siparişe dokunur, K-369), firma da hesabı geri veremez. Misafir siparişinde eski adrese bildirim zaten gidiyor (B-15, K-585). Codex'in önerisinin iki parçası alınmadı. **Yeni adrese bildirim:** yeni adres o anda doğrulama e-postasını almış ve değişikliği onaylamıştır, ikinci bir bildirim bir şey katmaz. **Değişiklikte diğer oturumları kapatmak:** değişikliği başkası yaptıysa bu hesabın sahibini dışarıda bırakır, saldırganı değil. Kural netleştirildi ve tersine yazıldı. | K-599 `(öneriyle kaydedildi — ⚠)`. Eski adrese yeni adresi taşımayan bir bildirim gider. Bildirimde tek kullanımlık, 7 gün geçerli bir "bu değişikliği ben yapmadım" bağlantısı vardır (P-47, Z-45). Bağlantı kullanılırsa e-posta geri döner, bütün oturumlar kapanır, sonradan bağlanan Google girişi kaldırılır ve şifre eski adrese giden bağlantıyla yeniden belirlenir. Bağlantı açıkken e-posta yeniden değiştirilemez. Kural yönetici hesaplarında da işler. Kalan risk yazıldı. Güncellenen yerler: §1.2 (Oturum), §3.13.14, §3.13.16, §4.2 Z-45 (yeni), §5.12.2, yeni satır 6.5.13, §8.1.5, §9.4, §11 P-47 (yeni). |

**Dağılım:** 1 KABUL · 1 KISMİ · 1 RET.

- **BULGU-1:** K-77'nin listesi teslim hattını ve cayma rejimini belirleyen nitelikleri saymıyordu. Daha önce gelmemişti.
- **BULGU-2:** İkinci kez geliyor (7. tur). Doküman doğruydu, ama tutar eşiği ve ticari alıcı sorusunu açıkça cevaplamıyordu. Belirsizlik giderildi; karar değişmedi.
- **BULGU-3:** Daha önce gelmemişti. K-110 ve K-109 değişikliğin kendisini korudu, sonradan fark edilmesini düşünmedi.

## 3. Ek bulgular

- **Fesihten sonra kargonun malı yine de teslim etmesi (11. turdan açık, işlenmedi):** Bu turda da gelmedi. Etki yansıtmada ya da sonraki turda bakılmalı.
- **Şifre değişikliği (sıfırlama değil):** §3.13.10 yalnız sıfırlamada oturumları kapatıyor. Oturum açıkken şifre değiştirildiğinde ne bildirim gidiyor ne diğer oturumlar kapanıyor. Codex bu turda bunu getirmedi. Şifreyi bilen biri şifreyi değiştirdiğinde sahibi e-postasıyla sıfırlama yapıp hesabı geri alabilir, yani BULGU-3'teki kadar ağır değil. Sonraki turda ya da etki yansıtmada bakılabilir.
- **Etki yansıtma için not:** `10 §2`'ye P-47 ve K-599 için bir KP satırı eklenmeli. `04` eski adrese giden bildirimin metnini ve geri alma ekranını yazmalı. `06` kalemin yeni donan alanlarını (K-598) taşımalı. `01`'de bu iki karar geçmiyor. Bu turda `01` ve `10`'a dokunulmadı.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`02` v0.30). İki karar satırı açıldı:

- K-598 `(öneriyle kaydedildi — ⚠)`: yasal hakka dokunuyor (cayma rejimi ve ifa sözü).
- K-599 `(öneriyle kaydedildi — ⚠)`: kişisel veriye ve hesabın güvenliğine dokunuyor; yeni bir bildirim, yeni bir parametre (P-47) ve yeni bir süre (Z-45) getiriyor.

- [x] BULGU-1 · [x] BULGU-2 (ret; §3.14.6 netleştirildi) · [x] BULGU-3

**Hukuki kontrol** (2026-10-03):

- **BULGU-1:** Mesafeli Sözleşmeler Yönetmeliği m.5 cayma hakkı ve istisnaları ön bilgilendirmeye bağlar, m.15 istisnaları sayar. Müşterinin onayladığı formdaki rejim sözleşmenin parçasıdır. Ürünün sonradan değişen bir işaretle verilmiş siparişin rejimini değiştirmemesi bunun sonucudur.
- **BULGU-2:** Gelir İdaresi duyurusu ve VUK 509 Sıra No.lu Genel Tebliğ uygulamasına göre vergi mükellefi olmayan nihai tüketicilere düzenlenen faturalarda vergi dairesi ve vergi kimlik numarası ya da T.C. kimlik numarası bulunması zorunlu değildir. Tüketici numarasını paylaşmak istemezse e-Arşiv faturada alıcı alanına `11111111111` yazılabilir. Kaynaklardaki tutar eşikleri (ör. 2023 için vergiler dahil 5.000 TL) yalnız faturanın e-Arşiv olarak düzenlenme zorunluluğuyla ilgilidir. Kaynak: alomaliye.com, "2023 Yılı Fatura Düzenleme Sınırı ve Faturaya İlişkin Bilgiler"; TÜRMOB e-kütüphane.
- **BULGU-3:** Hukuki iddia içermiyor.
