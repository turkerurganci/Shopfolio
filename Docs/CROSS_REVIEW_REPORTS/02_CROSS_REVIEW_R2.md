# Cross-Review — 02 Product Requirements (Tur 2)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/02_PRODUCT_REQUIREMENTS.md` v0.16 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki tur ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde). İstem 1. turunkiyle aynıdır: yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: Edge case
> Seviye: Yüksek
> Yer: §3.10 Kupon
> Alıntı: "Kupon sepet toplamını aşarsa reddedilir."
> Sorun: Sepet toplamına eşit sabit tutarlı kupon veya %100 indirim/kupon kabul edilebilir; böylece ödenecek tutar 0 TL olur. Doküman yalnız kart ve havale/EFT ödeme akışını tanımlar; 0 TL siparişin ödeme durumu, stok ayırması ve onay akışı tanımlı değildir.
> Öneri: Ödenecek toplamın her zaman pozitif olması kuralını ekleyin veya 0 TL sipariş için ödeme yöntemsiz, doğrudan onaylanan ayrı bir akış tanımlayın.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §5.8 Sipariş kalemi düzeyindeki kayıtlar
> Alıntı: "Kalem ayrı bir durum makinesi taşımaz"
> Sorun: Aynı bölümde cayma beyanı sonrası kalemin "Cayıldı" olduğu yazılıyor. “Cayıldı” sözlükte tanımlı, kodlanmış bir terim veya durum değildir; kalemin durum taşımadığı kuralıyla da çelişir. İptal, iade teslim alma ve cayma arasındaki uygulama durumu belirsiz kalır.
> Öneri: “Cayıldı”yı kullanmayıp caymayı tarih damgalı bir kayıt olarak açıkça modelleyin; ya da bunu sözlüğe ve kalem durum modeline kodu, geçişleri ve diğer kayıtlarla ilişkisiyle ekleyin.
>
> BULGU-3
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §10.6.3 Ölçülerin tanımı
> Alıntı: "Cayma, beyan edildiği anda iade sayılır"
> Sorun: Sözlükte “İade”, malın firmaya geri gönderilmesi olarak tanımlanmıştır. Bu ölçü ise mal hiç gönderilmese, kalem “mal dönmedi” gerekçesiyle kapatılsa ve geri ödeme yapılmasa bile cayma beyanını iade sayar. Aynı terim iki farklı olguyu ifade eder.
> Öneri: Ölçüyü “iptal ve cayma beyanı oranı” olarak adlandırın veya “iade edilen mal oranı” ile “cayma beyanı oranı”nı ayrı ölçüler olarak tanımlayın.
>
> BULGU-4
> Kriter: Teknik doğruluk
> Seviye: Orta
> Yer: §10.4.6 Yanlış yapılmış durum geçişi
> Alıntı: "Stok, hizmet kontenjanı, kupon hakkı ve ödeme kendiliğinden değişmez"
> Sorun: Ödenmemiş bir siparişin yanlışlıkla iptal edilmesi, ödeme eksenini Başarısız’a geçirir ve ayırmaları serbest bırakır. Sevkiyat ekseni İptal edildi’den önceki duruma düzeltilirse sipariş Alındı + Başarısız birleşiminde kalır; Başarısız → Ödendi yasaktır ve ayırmalar da geri gelmez. Sipariş yeniden yürütülemez.
> Öneri: Ödeme Başarısız’a geçmiş iptallerin durum düzeltmesiyle geri açılamayacağını belirtin ve yeni sipariş yolunu zorunlu tutun; alternatif olarak, ödeme ve ayırmaları güvenli biçimde telafi eden ayrı, denetlenebilir bir düzeltme akışı tanımlayın.
>
> BULGU-5
> Kriter: Edge case
> Seviye: Orta
> Yer: §3.22 Sipariş numarası ve takip erişimi
> Alıntı: "yıl + rastgele karakter bloğu"
> Sorun: Altı karakterli rastgele blok için çakışma denetimi veya yeniden üretme kuralı yoktur. Buna rağmen sipariş numarasının tekrar kullanılmayacağı belirtilir; numara havale açıklamasında, müşteri iletişiminde ve takipte kullanılır. Aynı yıl içindeki çakışma ödeme ve destek eşleştirmesini belirsizleştirir.
> Öneri: Sipariş numarasının tüm siparişler arasında tekil olması zorunluluğunu yazın; çakışmada numaranın sipariş oluşmadan yeniden üretileceğini belirtin.
>
> BULGU-6
> Kriter: Kullanıcı deneyimi
> Seviye: Orta
> Yer: §3.30 Sayfa adresleri, arama motorları ve paylaşım
> Alıntı: "ilk yayında sabitlenir"
> Sorun: Silinen ürünün adresinin 404 döneceği belirtilmiş, ancak silinen kaydın adresinin daha sonra aynı adlı yeni kayıt tarafından yeniden kullanılıp kullanılamayacağı tanımlanmamıştır. Yeniden kullanım eski bağlantının farklı bir ürüne açılmasına ve “silinen ürün 404 döner” kuralının bozulmasına yol açabilir.
> Öneri: İlk kez yayımlanmış her adresin, kayıt silinse dahi kalıcı olarak rezerve edildiğini; yeni kaydın sayısal ekli yeni bir adres alacağını açıkça yazın.
>
> SONUÇ: 6 BULGU> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ✅ KABUL | Doğru. K-73 "sepete kadar uygula" seçeneğini 0 TL'lik sipariş hattı açtığı için elemiş ve asgari sepet tutarını siparişin "her zaman ödenebilir" kalması için koymuştu; ama §3.10.4 yalnız "aşarsa reddedilir" diyordu ve asgari tutar kupon tutarına **eşit** girilebiliyordu (§6.6.8 yalnız "altında"yı engelliyordu). Sepet toplamına eşit sabit kupon ya da %100 yüzdesel kupon, yalnız dijital ya da hizmet kalemli siparişi (kargo ücreti yok — §3.19.1) 0 TL'ye indiriyordu. Önerinin ikinci yolu (ödemesiz onay akışı) K-73'ün elediği hattın aynısıdır, uygulanmadı. | K-564 (öneriyle kaydedildi): §3.10.4 — kupon siparişi sıfır tutara indiremez; sabit kuponun asgari sepet tutarı kupon tutarından büyüktür; yüzdesel kupon %100'e ulaşamaz; yuvarlama sıfıra indirirse kupon reddedilir. §1.2 Kupon, §6.2.12 ve §6.6.8 hizalandı. |
| BULGU-2 | ✅ KABUL | Doğru. §5.8 "Kalem ayrı bir durum makinesi taşımaz" (K-179) derken aynı maddede kalın **Cayıldı** etiketi bir kalem durumu gibi okunuyordu; sözlükte de, durum tablolarında da, karar kaydında da (K-503) böyle bir durum yok. Önerinin ilk yolu doğrudur: kapanış zaten tarih damgalı cayma beyanı kaydının sonucudur; ikinci yol (yeni kalem durumu) K-179'u bozar, uygulanmadı. Karar gerektirmez — etiket metnin kendi kuralıyla çelişiyordu. | §5.8 — "kalem kaydı **Cayıldı**" yerine "bu ayrı bir kalem durumu değildir, cayma beyanı kaydının sonucudur"; §6.4.21'deki "(Cayıldı)" kalktı. |
| BULGU-3 | ⚠️ KISMİ | Ölçünün sayma kuralı bilinçli bir karardır ve doğru yazılmıştır: K-495 "cayma, beyan edildiği anda iade sayılır" der ve sonucunu açıkça kabul eder ("beyanda bulunup malı hiç göndermeyen müşteri de oranda iade sayılır"); ölçü yeniden adlandırılmaz — M-4'ün adı `01 §6`'da, `10`'un KP-53'ünde ve satış özetinde (§10.6.2) aynıdır, ad değişikliği üç dokümana yayılır ve çözdüğü şey bir terim notudur. **Ama terim çakışması gerçektir:** sözlükteki İade "malın firmaya geri gönderilmesi"dir (§1.2), ölçü ise malı dönmeyen caymayı da "iade" sayar; metin bunu söylemiyordu. Bulgunun sonraki turlarda dönmemesi için belirsizlik metinde giderildi. | §10.6.3 — ölçünün adındaki "iade"nin sözlükteki İade'den geniş olduğu ve malın geri gönderilmesini değil cayma beyanını saydığı yazıldı. Yeni karar yok (K-495'in açıklaması). |
| BULGU-4 | ✅ KABUL | Doğru ve önerinin ötesinde bir boşluk var. K-504 yanlış iptalin "iptalden önceki duruma" döndürülmesine izin verdi ama ödeme ve ayırmaların kendiliğinden değişmeyeceğini yazdı. (1) Ödenmemiş siparişin iptali ödemeyi Başarısız yapar (§5.6.5, K-296); Başarısız terminaldir ve Başarısız → Ödendi yasaktır (K-183) — düzeltilen sipariş **Alındı + Başarısız**'da kalır, ne ödenir ne yürür. (2) İptal stoğu, kontenjanı ve kupon hakkını serbest bırakır (K-201) ve onları geri getiren bir müdahale yoktur (§10.4.1'in sekiz müdahalesi) — Hazırlanıyor'a dönen sipariş stokta karşılığı olmadan yürür; K-183'ün diriltmeye karşı gerekçesi tam da budur. (3) Parası karta geri gönderilmiş siparişin yeniden açılması firmaya bedeli alınmamış bir gönderim yükler. | K-565 (öneriyle kaydedildi — ⚠, önceki karardan ayrılır): §10.4.6 — iptal yalnız ödeme Ödendi'de kalmışsa (para henüz geri gönderilmemişse) düzeltilir; Başarısız ya da parası gönderilmiş iptalde müşteri yeni sipariş verir; düzeltme stoğu ve kontenjanı yeniden ayırır, yetmezse yapılamaz; kupon hakkı yeniden kullanılmış sayılır. §5.4, §7.6.3 ve §6.7.5 hizalandı. |
| BULGU-5 | ✅ KABUL | Doğru ama düşük etkili. §3.22.2 "tekrar kullanılmaz" diyor, rastgele bloğun (P-19: altı karakter) çakışabileceğini ve çakışmada ne olacağını söylemiyordu. Olasılık çok düşüktür ama sıfır değildir; numara havale açıklamasında ve misafir takibinde siparişi tek başına gösterir. Ürün kuralı olarak tekillik yazılır; üretimin nasıl yapılacağı `06`'nın işidir. | K-566 (öneriyle kaydedildi): §3.22.2 — numara bütün siparişler arasında tekildir; çakışmada sipariş oluşmadan yeni numara üretilir. |
| BULGU-6 | ⚠️ KISMİ | Belirsizlik gerçektir: §3.30.2 "aynı adres zaten varsa sonuna sayı eklenir" diyor, silinmiş kaydın adresinin "var" sayılıp sayılmadığını söylemiyordu. Önerilen çözüm — silinen adresin kalıcı rezervi — uygulanmadı: K-85 kalıcı silmeyi "yanlış yayınlanan ürünün izinin kalmaması" için kurdu, silinen adresi saklamak adını da saklamaktır. Adres addan üretildiği için adresi alan yeni kayıt aynı adı taşır; eski bağlantı çoğu zaman aynı şeyi arayanı doğru kayda götürür. "Silinen ürün 404 döner" kuralı bozulmaz — adres artık silinmiş bir kaydın değil yaşayan bir kaydın adresidir. | K-567 (öneriyle kaydedildi): §3.30.2 — taslak ve arşivdeki kaydın adresi dolu sayılır; kalıcı silinen kaydın adresi serbest kalır ve aynı adlı yeni kayıt onu alabilir; kalan risk adıyla yazıldı. §3.30.4 "silinmiş ve yeni bir kayda verilmemiş" diye hizalandı. |

**Dağılım:** 4 KABUL · 2 KISMİ · 0 RET.

Tur 1'in konuları (adres alanları, sipariş bağlantısı, kısa geçmişli varyant, alternatif metin) bu turda dönmedi.

## 3. Ek bulgular

Yok. BULGU-4'ün düzeltmesi sırasında §6.7.5'in yanlış geçiş satırının iptal düzeltmesinin koşulunu taşımadığı görüldü; aynı kararla (K-565) kapandı.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`02` v0.17). Dört karar satırı açıldı: K-565 `(öneriyle kaydedildi — ⚠)` — önceki bir kararın (K-504) yan etki kuralından ayrıldığı için proje sahibinin gözden geçirme listesine girer; K-564, K-566 ve K-567 `(öneriyle kaydedildi)`. BULGU-2 ve BULGU-3 metin düzeltmesidir, karar açmadı. `10`'un KP-11 (kupon) ve KP-48 (müdahaleler) satırlarına dokunan etki yansıtma sonraki adımda toplu yapılır; bu turda bariz bir çelişki doğmadı.

- [x] BULGU-1 · [x] BULGU-2 · [x] BULGU-3 · [x] BULGU-4 · [x] BULGU-5 · [x] BULGU-6

**Hukuki kontrol:** bu turun bulguları hukuki bir iddia taşımadı; resmî metne karşı doğrulama gerekmedi.
