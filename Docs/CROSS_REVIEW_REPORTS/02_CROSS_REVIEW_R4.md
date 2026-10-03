# Cross-Review — 02 Product Requirements (Tur 4)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/02_PRODUCT_REQUIREMENTS.md` v0.18 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde; satır sonlarındaki boşluklar atıldı). İstem önceki turlarınkiyle aynıdır: yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §3.8 Fiyat ve KDV
> Alıntı: "Ölçüyle satılan ürün için isteğe bağlı **'ölçü birimi + net miktar'** alanı vardır"
> Sorun: Ölçüyle satılan üründe birim fiyat, bu isteğe bağlı alan doldurulursa gösteriliyor. Ancak §12.1.5 aynı bilgiyi zorunlu fiyat etiketi bilgisi olarak tanımlıyor. Alan boş bırakılarak ürün yayına alınabilir; bu durumda dokümanın kendi uyumluluk kuralı sağlanamaz.
> Öneri: Birim fiyat gerektiren ürünlerde ölçü birimi ve net miktarı zorunlu yayın koşulu yapın; bu kapsam dışındaki ürün tiplerini açıkça tanımlayın.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §7.3 Cayma
> Alıntı: "Parçalı gönderim olmadığı için (§3.20.3) siparişin fiziksel kalemleri tek bir teslim tarihi taşır"
> Sorun: §1.2 ve §5.8, her sipariş kaleminin kendi teslim işaretini ve teslim tarihini taşıdığını söylüyor. Tek parça gönderim, yöneticiye kalemlere farklı teslim tarihleri girilmesini engelleyen bir kural değildir. Farklı tarih girilirse cayma penceresi ve ayıp talebi başlangıcı belirsizleşir.
> Öneri: Fiziksel kalemler için teslim tarihini sipariş/sevkiyat düzeyinde tek kayıt yapın veya teslim işareti konurken tüm fiziksel kalemlere aynı tarihin zorunlu olarak yazılacağını açıkça belirtin.
>
> BULGU-3
> Kriter: Eksiklik
> Seviye: Orta
> Yer: §3.1 Firma, kurulum ve satış kapısı
> Alıntı: "ürün kayıt yapmaz, kayıt bilgisini taşır ve doğrulama bandını gösterir"
> Sorun: ETBİS kayıt bilgisinin hangi alanla, kim tarafından ve hangi koşulda girileceği; doğrulama bandının hangi veriye göre gösterileceği tanımlanmamış. Firma kimliği, panel kapsamı ve kurulum kontrol listesinde bu bilgi için karşılık yoktur.
> Öneri: ETBİS yükümlülüğü bulunan firmalar için kayıt/teyit bilgisinin kaynağını, zorunluluk koşulunu, gösterim davranışını ve eksik olduğundaki panel uyarısını ürün kuralı olarak tanımlayın.
>
> BULGU-4
> Kriter: Edge case
> Seviye: Orta
> Yer: §7.4 İade ve geri ödeme
> Alıntı: "Havale hattında müşteri IBAN'ını iptal, gecikme feshi ya da cayma beyanı sırasında girer"
> Sorun: İlk girilen IBAN hatalıysa veya transfer banka tarafından reddedilirse müşterinin IBAN’ı düzeltme yolu tanımlı değil. Firma IBAN’ı panelden giremez; mevcut “IBAN bekleniyor” akışı yalnız silinmiş IBAN sonrası ya da başarısız kart iadesi için düzenlenmiş. Bu, geri ödemenin gereksiz biçimde kilitlenmesine yol açar.
> Öneri: Geri ödeme işlenmeden önce müşterinin IBAN’ını sipariş sayfasından güncelleyebilmesini ve başarısız havale sonrası yeniden IBAN isteme/ödeme akışını tanımlayın; değişikliği işlem izine yazın.
>
> SONUÇ: 4 BULGU
> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ⚠️ KISMİ | Sorun gerçek ama sınırlı. §3.8.5 alanı *"Ölçüyle satılan ürün için isteğe bağlı"* diye yazıyordu; §12.1.5 ise birim fiyatı zorunlu bilgi sayıyor. Metin, alanın neden isteğe bağlı olduğunu söylemediği için alan bir muafiyet gibi okunuyordu. Önerilen çözüm uygun değil: sistem hangi ürünün kilo, litre ya da metreyle satıldığını ürün kaydından bilemez. Ölçüyle satılan üründe alanı zorunlu yapmak için de ürünün ölçüyle satıldığını yine firmanın beyan etmesi gerekir ve bu beyan alanı doldurmakla aynı şeydir. K-511 *"Birim fiyat her üründe zorunlu"* seçeneğini bilerek eledi; K-84 sistemin ne satıldığını denetlemediğini, K-14 de yasal yükümlülüğün firmada kaldığını yazar. | K-573 `(öneriyle kaydedildi — ⚠)`: alanın isteğe bağlı olması muafiyet değildir; ölçüyle satılan üründe alanı doldurmak firmanın yükümlülüğüdür, panel bunu alanın yanında hatırlatır ve alan yayın koşulu olmaz. §3.8.5, §12.1.5, §12.5 (yeni satır: fiyat etiketinin birim fiyatı). |
| BULGU-2 | ✅ KABUL | Doğru. §3.20.11 ve S6 teslim işaretini sipariş düzeyinde bir geçiş olarak kuruyor; §7.3.1 de *"siparişin fiziksel kalemleri tek bir teslim tarihi taşır"* diyor. Ama sözlük (§1.2) ve §5.8 tarihi kalem kaydı olarak anlatıyor ve §10.4.5'in düzeltmesi kalem için mi sipariş için mi yapılıyor, belli değildi. Bu yüzden kalemlere farklı tarih girilebileceği okunabiliyordu. Kural K-177 (parçalı gönderim yok) ile K-288'in sonucudur; yeni bir karar gerekmedi. | Önerinin ikinci seçeneği uygulandı: teslim işareti sipariş düzeyinde bir kez konur; girilen tarih iptal edilmemiş ve çıkarılmamış fiziksel kalemlerin tamamına aynı yazılır; düzeltme de hepsinin tarihini birlikte değiştirir. §3.20.11, §10.4.5, §1.2 (Teslim tarihi), §5.8. |
| BULGU-3 | ✅ KABUL | Doğru. K-14 ürünün *"kayıt bilgisini taşır ve doğrulama bandını gösterir"* dediğini yazdı, ama bilginin hangi alanda ve kim tarafından girildiği `02`'de tanımlı değildi. Bandın gösterilmesi için veri yokken ne olacağı da yazılı değildi. §3.1.2'nin panelden yönetilen kimlik listesinde ve §10.1.2'de bu bilginin karşılığı yoktu. Kayıt yükümlülüğünün kaynağı Ticaret Bakanlığı'nın ETBİS kayıt ve bildirim duyurusuna karşı kontrol edildi: kayıt, faaliyete başlamadan önceki bir yükümlülüktür; doğrulama bilgisinin sitede gösterilmesine dair açık bir hüküm o metinde yoktur. Bandı göstermek K-14'ün seçimidir; bu karar yalnız bandın veri kaynağını tanımlar. Önerinin *"eksik olduğundaki panel uyarısı"* kısmı, satış kapısına bağlanmayan bir hatırlatma olarak uygulandı. `10` ÖK-5 satışın ETBİS yüzünden engellenmediğini yazar ve yükümlülük her firmada doğmaz. | K-574 `(öneriyle kaydedildi — ⚠)`: firma kimliğinin isteğe bağlı "ETBİS doğrulama bilgisi" alanı. Alan doluysa band her sayfada görünür ve doğrulama sayfasına götürür; boşsa band görünmez. Alan satış kapısını ve kurulum kontrol listesini tutmaz; panel yükümlülüğü hatırlatır; değişiklik işlem izine yazılır. §3.1.2, §3.1.4, §10.1.2, §12.1.4, §12.5. |
| BULGU-4 | ✅ KABUL | Doğru. IBAN'ın tek giriş yolu müşterinin sipariş sayfasıdır ve firma IBAN'ı panelden giremez (K-498, §8.4.2). Ama yanlış girilmiş IBAN'ın düzeltilmesi de, bankada reddedilen ya da geri dönen havale de tanımsızdı. Bu hâlde geri ödeme kilitlenir, yasal süre ise işlemeye devam ederdi (Z-16, Z-17). Önerinin bir kısmı uygulanmadı: müşterinin IBAN düzeltmesi işlem izine yazılmadı. §10.3.1'in izi yönetici işlemlerini tutar ve sipariş satırları müşterinin kişisel verisini değer olarak taşımaz. İze yazılan, firmanın "havale gerçekleşmedi" adımıdır. | K-575 `(öneriyle kaydedildi — ⚠)`: (1) IBAN girişte biçim ve sağlama basamağıyla denetlenir. (2) Müşteri IBAN'ı geri ödeme işlenene kadar düzeltebilir. (3) Firma geri ödemeyi havale gerçekleştikten sonra işler. Havale bankada gerçekleşmezse firma panelden "havale gerçekleşmedi" der: IBAN silinir, IBAN alanı yeniden açılır ve B-14 gider; süre durmaz. (4) Kalan risk: işlenmiş geri ödemeden sonra dönen havalede ödeme ekseni düzeltilmez (K-570'in sınırı). §7.4.5, yeni §6.4.27, §8.4.2, §9.2 B-14, §10.1.2, §10.3.1, §10.5.2, §10.6.1. |

**Dağılım:** 3 KABUL · 1 KISMİ · 0 RET.

Önceki üç turun konuları bu turda dönmedi. Dört bulgu da metnin açık bıraktığı bir yere işaret ediyordu; hiçbiri bilinçli bir kararı hedef almadı.

## 3. Ek bulgular

- **EK-1 (düzeltildi):** §10.6.1 "IBAN bekleniyor" kalemini anlatırken *"kart iadesi başarısız olup firma havaleyi seçmiş"* diyordu. Bu, 3. turda K-569 ile değişen ifadenin kalıntısıydı: havale yolu müşterinin seçimidir. Cümle *"firma müşteriye havale yolunu açmış"* oldu ve K-575'in dönen havale hâli eklendi.
- **Etki yansıtma için not:** `10` KP-35 ziyaretçinin ETBİS doğrulama bandına ulaştığını, ÖK-5 ise ürünün kayıt bilgisini taşıdığını söylüyor. K-574'e göre band yalnız alan doluyken görünür. Bu bir çelişki değil, ifade farkıdır; etki yansıtma adımında hizalanacak. `04` için yeni girdiler şunlar: ürün formunun ölçü birimi hatırlatması, firma kimliği formundaki ETBİS alanı, sipariş sayfasının düzeltilebilir IBAN alanı ve panelin "havale gerçekleşmedi" adımı.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`02` v0.19). Üç karar satırı açıldı: K-573, K-574 ve K-575 `(öneriyle kaydedildi — ⚠)`. Hepsi yasal yükümlülüğe, paraya ya da kişisel veriye dokunduğu için proje sahibinin gözden geçirme listesine girer. Karar kaydında K-511, K-14 ve K-498'in etki sütunlarına yeni satırlara atıf eklendi.

- [x] BULGU-1 · [x] BULGU-2 · [x] BULGU-3 · [x] BULGU-4 · [x] EK-1

**Hukuki kontrol:** BULGU-1 için Fiyat Etiketi Yönetmeliği m.5'in K-511'deki doğrulaması (mevzuat.gov.tr, 2026-10-03) esas alındı; yeni bir hukuki iddia eklenmedi. BULGU-3, Ticaret Bakanlığı'nın ETBİS kayıt ve bildirim esasları duyurusuna (ticaret.gov.tr, 2026-10-03) karşı kontrol edildi. Kayıt, faaliyete başlamadan önceki bir yükümlülüktür; doğrulama bilgisinin sitede gösterilmesine dair açık bir hüküm o metinde yoktur, bu yüzden band zorunluluk olarak değil K-14'ün seçimi olarak yazıldı. BULGU-4 yeni bir hukuki iddia taşımaz; Mesafeli Sözleşmeler Yönetmeliği m.12'nin geri ödeme kuralları (K-491, K-569) değişmedi.
