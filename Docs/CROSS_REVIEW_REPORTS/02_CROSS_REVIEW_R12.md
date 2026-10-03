# Cross-Review — 02 Product Requirements (Tur 12)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/02_PRODUCT_REQUIREMENTS.md` v0.27 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde; satır sonlarındaki boşluklar atıldı). İstem önceki turlarınkiyle aynıdır: yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §3.17.2 Sipariş onayı ve stok ayırma; §3.24.3 Onay adımı ve yasal metinler; §3.33.3 Yasal metinler
> Alıntı: "metni besleyen bir ayar değiştiğinde artar"
> Sorun: Ön Bilgilendirme Formu’nu besleyen iade adresi, firma kimliği veya kargoya verme süresi; müşteri kutuları işaretledikten sonra değişirse yeni sürüm oluşur. Ancak siparişin durdurulup formun yeniden gösterileceği ve iki onayın yeniden alınacağı yalnız tutar, döküm veya kalem değişiminde tanımlanmıştır. Sipariş, müşterinin onaylamadığı yeni form sürümüyle oluşabilir.
> Öneri: Ön Bilgilendirme Formu veya Mesafeli Satış Sözleşmesi sürümünü değiştiren her değişikliği onay farkı sayın; siparişi durdurun, güncel metinleri yeniden gösterin ve ilgili kutuların yeniden işaretlenmesini zorunlu kılın.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §2.3 Kurumsal içerik akışı; §3.27.24 Kurumsal içerik
> Alıntı: "Kayıt taslak olarak açılır; yalnız adla ya da başlıkla kaydedilebilir."
> Sorun: Hakkımızda ve duyuru kayıtlarında ad ya da başlık alanı yoktur; §3.27.24 bu iki kaydın boşken de taslak kaydedilebildiğini açıkça söyler. Ana akış, bu iki içerik türünde uygulanamaz bir başlangıç koşulu koymaktadır.
> Öneri: §2.3’te akışı tipe göre ayırın: ad/başlık taşıyan kayıtlar için bu koşulu koruyun; Hakkımızda ve duyurunun boş taslak olarak açılabildiğini açıkça belirtin.
>
> BULGU-3
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §5.4 Sevkiyat ekseni; §5.8 Sipariş kalemi düzeyindeki kayıtlar; §6.7.16 Sipariş yürütümü
> Alıntı: "kalan kalem yoksa firma siparişi S4 ya da S9 ile kapatır"
> Sorun: S4 ve S9, firma iptalinin gecikme feshine uğramış kaleme uygulanamayacağını açıkça söyler. Buna rağmen tüm kalan kalemleri gecikme feshine uğramış siparişin bu geçişlerle kapatılması istenmektedir. Bu durumda siparişin hangi terminal sevkiyat durumuna geçeceği çelişkilidir.
> Öneri: Gecikme feshi nedeniyle aktif fiziksel kalem kalmadığında uygulanacak tek terminal geçişi açıkça tanımlayın. Örneğin firma iptalinden ayrı bir sistem geçişiyle siparişi İptal edildi durumuna alın; geri ödemenin ikinci kez tetiklenmeyeceğini de belirtin.
>
> BULGU-4
> Kriter: Belirsizlik
> Seviye: Orta
> Yer: §4.2 Z-14 Cayma penceresi — hizmet kalemi; §5.7 Tipe göre sevkiyat hattı
> Alıntı: "teslim tarihi girilmemişse pencere başlamaz"
> Sorun: Fiziksel ve hizmet kalemi içeren siparişte fiziksel kalemlerin tamamı teslimden önce iptal edilir, çıkarılır veya gecikme feshine uğrarsa teslim tarihi hiç oluşmaz; sipariş ise fiziksel kalemsiz hatta devam eder. Hizmet kaleminin cayma penceresinin sipariş tarihinden mi başlayacağı, açık kalmaya devam mı edeceği belirtilmemiştir.
> Öneri: Fiziksel kalem kalmadığı anda hizmet kaleminin cayma penceresi için başlangıç kuralını tanımlayın; başlangıcın yeniden hesaplanıp hesaplanmayacağını ve müşteri arayüzündeki sonucu belirtin.
>
> BULGU-5
> Kriter: Edge case
> Seviye: Orta
> Yer: §3.16 Sepet; §3.17 Sipariş onayı ve stok ayırma
> Alıntı: "Sipariş kalan kalemlerle devam eder"
> Sorun: Sepetteki tüm kalemler tükenir, taslağa alınır, arşivlenir veya silinirse “kalan kalem” olmayabilir. Doküman bu durumda ödeme özetinin ve onay düğmesinin nasıl davranacağını tanımlamıyor; boş/0 TL sipariş oluşmasını engelleyen açık bir kural yoktur.
> Öneri: Siparişe girebilecek kalem sayısı sıfırsa ödeme özetini ve sipariş onayını kapatan, kullanıcıyı sepete yönlendiren ve sipariş/ödeme/stok ayırma kaydı oluşturmayan açık kural ekleyin.
>
> SONUÇ: 5 BULGU
> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ✅ KABUL | Doğru. K-129 siparişi durduran farkı tutara, dökümüne ve kalemlere bağladı. K-191 formu özetle birebir aynı yaptı, ama formun özete eklediği dört bilgi var: kargoya verme süresi, iade adresi, firma kimliği ve cayma koşulları. Bunları besleyen ayarlar özeti değiştirmeden formun sürümünü artırır (K-362). §3.17.2'deki fark listesi bu ayarları saymıyordu. Bu yüzden müşteri bir sürümü okuyup kutuyu işaretlerken sipariş başka bir sürümü dondurabiliyordu. §3.33.4'ün *"metin değiştiğinde yeniden onay alınmaz"* cümlesi de durumu örtüyordu. O cümle (K-363) onaylanmış siparişler içindir, ama sınırı metinde yazılı değildi. Yönetmelik m.5/1 bilgilendirmenin sözleşme kurulmadan önce yapılmasını ister, m.5/6 da ispat yükünü satıcıya verir. | K-591 `(öneriyle kaydedildi — ⚠)`. Formun ya da sözleşmenin sürümünü artıran fark da siparişi durdurur. Müşteri güncel metinleri, neyin değiştiğini söyleyen bir satırla görür ve iki kutuyu yeniden işaretler. Aydınlatma metni ve çerez politikası siparişi durdurmaz, çünkü ikisi onaylanan metin değildir (K-346). §3.33.4'ün kapsamı yazıldı. Güncellenen yerler: §3.17.2, §3.24.3, §3.33.4, §6.2.3, §12.1.7. |
| BULGU-2 | ✅ KABUL | Doğru. K-584 9. turda Hakkımızda ve duyurunun boş da taslak kaydedilebileceğini §3.27.24'e yazdı, ama §2.3'teki akış adımı eski cümleyi taşıyordu: *"yalnız adla ya da başlıkla kaydedilebilir"*. Yeni bir karar gerekmiyor, metin kayda hizalanacak. | §2.3'ün 2. adımı §3.27.24'e hizalandı (K-584). |
| BULGU-3 | ⚠️ KISMİ | Sorun gerçek. K-590 önce *"firma iptali (S4, S9) ona uygulanmaz"* dedi, sonra *"kalan kalem yoksa firma siparişi S4 ya da S9 ile kapatır"*. Kapanışı, uygulanmayacağı söylenen işlemle yaptırıyordu. S4 ve S9'un firma kolunda sebep seçimi, geri alınamaz onayı ve B-7 var. Feshedilmiş kalemde bir de ikinci geri ödeme sorusu çıkıyor. Kapanışta bunlardan hangisinin işlediği yazılı değildi. Aynı belirsizlik K-503'ün cayma beyanlı kalemi için de vardı. 11. turun düzeltmesi bu belirsizliği metne taşıdığı için konu bu turda yeniden geldi. Codex firma iptalinden ayrı bir sistem geçişi önerdi. Bu öneri yalnız Hazırlanıyor için alındı, çünkü orada mal yola çıkmamıştır. Kargodaki siparişte K-590'ın *"mal henüz dönmemiş olabilir"* gerekçesi geçerli kalıyor. Yeni bir geçiş numarası ya da durum da gerekmiyor: S4 ve S9 kapanışı taşıyabilir. | K-592 `(öneriyle kaydedildi — ⚠)`. Siparişin açık kalemi kalmazsa sipariş İptal edildi'ye kapanış olarak geçer. Hazırlanıyor'da bu fesihle aynı anda ve kendiliğinden olur (S4). Gönderi kargodaysa firma malın dönüşünü işaretler (S7) ve siparişi S9'un kapanışıyla kapatır. Kapanış firma iptali değildir: sebep seçilmez, iptal kaydı ve ikinci bir geri ödeme açılmaz, B-7 gitmez (müşteri B-9'u almıştır). Cayma beyanıyla kapanan kalemde de aynıdır. Güncellenen yerler: §5.4 (İptal edildi, S4, S9), §5.8, §6.7.16, §7.2.3, §9.2 B-7. |
| BULGU-4 | ✅ KABUL | Doğru, iki okuma çatışıyordu. Birincisi K-503(c): karışık siparişte hizmet kaleminin penceresi teslim tarihinden işler, *"teslim tarihi yoksa pencere açık kalır"*. İkincisi §5.7: fiziksel kalemleri iptal edilmiş sipariş *"fiziksel kalemi olmayan"* siparişin hattına düşer. Bu okuma pencereye uygulanırsa pencere sipariş tarihinden işler ve geriye dönük olarak çoktan dolmuş olabilir. Hangi okumanın geçerli olduğu yazılı değildi. | K-593 `(öneriyle kaydedildi — ⚠)`. Siparişin niteliği onay anındaki kalemlerinden okunur. Fiziksel kalemler teslimden önce kapanırsa pencere sipariş tarihine geri çekilmez ve başlamaz. Hak onaylı ifanın tamamlanmasıyla düşer ve cayma düğmesi o ana kadar görünür. Bu tüketici lehine okumadır. §5.7'nin okuması sevkiyat hattıyla sınırlandı. Güncellenen yerler: §4.2 Z-14, §5.7, §7.3.3, yeni satır 6.4.29, §12.1.9. |
| BULGU-5 | ✅ KABUL | Doğru, ama önemi düşük. §3.16.6 ve §3.16.7 *"sipariş kalan kalemlerle devam eder"* diyor, kalan kalemin sıfır olabileceğini yazmıyordu. §3.10.4'ün *"0 TL'lik sipariş doğmaz"* sonucu (K-73) yalnız kuponu kapsıyor. Kural kendiliğinden anlaşılır, ama onay anındaki durdurma (K-129) güncel özeti yeniden onaylanacak bir özet olarak gösteriyor. Boş özet için davranış yazılmalıydı. | K-594 `(öneriyle kaydedildi)`. Siparişe girebilecek kalem kalmazsa sipariş verilemez ve ödeme adımı açılmaz. Durum onay anında oluşursa sipariş oluşmaz: numara, ödeme ve ayırma doğmaz, müşteri sepete döner. Güncellenen yerler: §3.16.7, §6.2.4. |

**Dağılım:** 4 KABUL · 1 KISMİ · 0 RET.

Beş bulgunun hiçbiri bilinçli bir kararın elediği seçeneğe yönelmiyor. Hepsi bir kuralın kapsamında yazılmamış bir parçayı gösteriyor:

- **BULGU-1:** K-129'un fark listesi formun özete eklediği bilgiyi saymıyordu.
- **BULGU-2:** 9. turun düzeltmesi §2.3'e taşınmamıştı.
- **BULGU-3:** 11. turun düzeltmesi kendi içinde çelişen bir cümle bırakmıştı.
- **BULGU-4:** K-503 ile §5.7'nin okumaları çatışıyordu.
- **BULGU-5:** Boş sepetteki davranış yazılı değildi.

BULGU-3, 11. turun BULGU-2'siyle aynı konuya dokunuyor. Konu, düzeltme metninde bir belirsizlik bıraktığı için döndü; bu turda belirsizlik giderildi. Diğer dört bulgu önceki turlarda gelmedi.

## 3. Ek bulgular

- **Fesihten sonra kargonun malı yine de teslim etmesi (11. turdan açık, işlenmedi):** Kargodaki siparişte fesih yapıldıktan sonra kargo malı firmaya döndürmeyip müşteriye teslim ederse ne olacağı hâlâ yazılı değil. K-592 kapanışı malın döndüğü varsayımıyla kurdu. Bu durum ayrı bir karar istiyor ve etki yansıtmada ya da sonraki turda bakılmalı.
- **Etki yansıtma için not:** `10 §2`'de üç şey hizalanmalı. K-592 ile KP satırlarında B-7'nin kapanışta gitmediği ve Hazırlanıyor'daki kendiliğinden kapanış. K-591 ile onay adımındaki yeniden onay listesine sürüm farkı. K-593 ile karışık siparişte hizmetin cayma düğmesi. `01`'de bu dört karar geçmiyor (`grep` ile kontrol edildi). Bu turda `01` ve `10`'a dokunulmadı.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`02` v0.28). Dört karar satırı açıldı:

- K-591 `(öneriyle kaydedildi — ⚠)`: yasal hakka dokunuyor (ön bilgilendirme); K-363'ün kapsamını netleştiriyor.
- K-592 `(öneriyle kaydedildi — ⚠)`: paraya (ikinci geri ödeme) dokunuyor ve K-590'ın bir cümlesini değiştiriyor.
- K-593 `(öneriyle kaydedildi — ⚠)`: yasal hakka dokunuyor (cayma penceresi).
- K-594 `(öneriyle kaydedildi)`.

- [x] BULGU-1 · [x] BULGU-2 · [x] BULGU-3 · [x] BULGU-4 · [x] BULGU-5

**Hukuki kontrol** (Mesafeli Sözleşmeler Yönetmeliği'nin konsolide metnine karşı):

- **BULGU-1:** m.5/1'e göre tüketici, sözleşme kurulmadan ya da teklifi kabul etmeden önce bütün başlıklarda bilgilendirilmelidir. m.5/6 ön bilgilendirmenin ispat yükünü satıcıya verir. Sipariş müşterinin görmediği bir sürümü dondurursa bu ispat yapılamaz.
- **BULGU-3:** m.16/2–3 (K-499'da doğrulanmış). Fesih sözleşmeyi o kalemler için sona erdirir ve bedel bir kez geri ödenir. Kapanışın ikinci bir geri ödeme açmaması bu hükümle uyumlu.
- **BULGU-4:** m.9/2'ye göre cayma süresi hizmette sözleşmenin kurulduğu gün, malda teslim günü başlar. m.9/5'e göre mal teslimi ile hizmet ifasının birlikte yapıldığı sözleşmede mal hükümleri uygulanır. Fiziksel kalemleri sonradan kapanan siparişte hangi başlangıcın geçerli olduğunu Yönetmelik ayrıca düzenlemiyor. Ürün tüketici lehine okumayı seçti: pencere açık kalır, hak onaylı ifanın tamamlanmasıyla düşer. Bu, yasal asgarinin altına düşmüyor.
