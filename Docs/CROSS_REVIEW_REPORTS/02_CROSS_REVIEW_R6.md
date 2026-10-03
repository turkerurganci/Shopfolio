# Cross-Review — 02 Product Requirements (Tur 6)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/02_PRODUCT_REQUIREMENTS.md` v0.20 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde; satır sonlarındaki boşluklar atıldı). İstem önceki turlarınkiyle aynıdır: yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §3.9 İndirim ve referans fiyat
> Alıntı: "indirimin başladığı andan önceki **10 gün** içinde"
> Sorun: Referans Fiyat Etiketi Yönetmeliği’ndeki genel indirim referansı penceresi 10 değil 30 gündür. Hizmeti otomatik olarak “hemen önceki fiyat” istisnasına almak da genel kuralın kapsamıyla uyumlu değildir. Yanlış referans fiyat, hukuka aykırı indirim beyanına yol açar.
> Öneri: §3.9.3, §3.9.4 ve Z-21’i güncel mevzuata göre 30 günlük genel pencereyle düzeltin; yalnız mevzuatta açıkça tanınan istisnaları ayrı tanımlayın.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §3.16 Sepet; §3.17 Sipariş onayı ve stok ayırma
> Alıntı: "sipariş kalan kalemlerle devam eder"
> Sorun: §3.16.6, §3.16.7 ve §6.2.4; tükenen veya vitrinden kalkmış kalem çıkarılarak siparişin kalanlarla devam edeceğini söyler. Buna karşılık §3.17.2, siparişe girecek kalemlerdeki her farkın sipariş oluşumunu durdurup güncel özet için yeniden onay gerektirdiğini belirtir. Bu çelişki, müşterinin onayladığından farklı bir sepetin yeniden onaysız siparişleşmesine neden olabilir.
> Öneri: Kalem çıkarılması dahil her özet farkında mevcut onayı geçersiz kılın; güncel özet ve Ön Bilgilendirme Formu gösterildikten sonra müşterinin yeniden onayıyla yalnız kalan kalemlerden sipariş oluşturulacağını açıkça yazın.
>
> BULGU-3
> Kriter: Güvenlik
> Seviye: Orta
> Yer: §8.2 Deneme limitleri
> Alıntı: "Aynı IP ve aynı e-posta adresi — ayrı ayrı"
> Sorun: L-7, doğrulanmamış misafir e-posta adresini tek başına 24 saatlik yeni sipariş engeli için kullanır. Saldırgan, hedefin e-posta adresiyle başarısız kart denemeleri oluşturup hedefin sipariş vermesini engelleyebilir. Ortak IP kullanan gerçek kullanıcılar da aynı biçimde etkilenebilir.
> Öneri: L-7’nin e-posta eksenini yalnız doğrulanmış ya da giriş yapılmış hesaplarda uygulayın; misafir akışında e-posta tek başına engel sebebi olmasın ve IP tabanlı engel için güvenli bir kurtarma/alternatif ödeme yolu tanımlayın.
>
> BULGU-4
> Kriter: Edge case
> Seviye: Orta
> Yer: §10.6.3 Ölçülerin tanımı
> Alıntı: "İptal kayıtlarından ve cayma beyanlarından hesaplanır."
> Sorun: Gecikme feshi, §7.2.3 ve §5.8’de iptalden ve caymadan ayrı bir kayıt türü olarak tanımlanır. Bu nedenle gecikme nedeniyle feshedilip geri ödenen fiziksel kalemler, “iptal ve iade oranı” hesabına mevcut kaynak tanımıyla girmez. Rapor, müşteri kaybını ve teslimat sorunlarını olduğundan düşük gösterebilir.
> Öneri: Gecikme feshinin oran ve “iptal ve iade sayısı” içinde sayılıp sayılmayacağını açıkça belirleyin; sayılacaksa hesap kaynaklarına gecikme feshi kayıtlarını ekleyin.
>
> SONUÇ: 4 BULGU
> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ❌ RET | Bulgu, Fiyat Etiketi Yönetmeliği m.11/1'in eski metnine dayanıyor. 11/10/2025 tarihli değişiklikten (RG 33044, m.5) sonraki metin şöyle: *"indirimin uygulandığı tarihten önceki on gün içinde uygulanan en düşük fiyat esas alınır. Meyve ve sebze gibi çabuk bozulabilen mallar ile hizmetlere ilişkin [...] indirimli fiyattan bir önceki fiyat esas alınır."* Değişiklik yayımı tarihinde yürürlüğe girdi (m.7). Yani on gün doğru. Hizmetin "hemen önceki fiyat" kuralına alınması da Yönetmelik'in kendi istisnasıdır, ürünün uydurduğu bir istisna değil. Bu, K-508'in resmî metne karşı doğrulanmış kararıdır. Ama bulgunun kaynağı metindeki bir hataydı: §3.9.3 ve §12.1.6 yönetmeliği *"Referans Fiyat Etiketi Yönetmeliği"* diye anıyordu ve değişikliğin tarihini yazmıyordu. Böyle bir yönetmelik yok; okuyan biri kuralı eski otuz günlük metinle karşılaştırabilir. Bu belirsizlik giderildi. Konu önceki beş turda gelmedi. | Karar kaydı gerekmedi. §3.9.3 ve §12.1.6'da ad *"Fiyat Etiketi Yönetmeliği"* olarak düzeltildi ve *"RG 11/10/2025-33044 ile değişik"* ibaresi eklendi; §3.9.3 eski metnin otuz gününün bu tarihte on güne indiğini de söylüyor. |
| BULGU-2 | ⚠️ KISMİ | Çelişki iddiası doğru değil, ama metin belirsiz. §3.16.6 ve §3.16.7 sepetin kuralıdır: satın alınamayan kalem siparişe girmez ve özet onsuz hesaplanır. §3.17.2 onay anının kuralıdır: özet gösterildikten sonra oluşan her fark, bir kalemin ayrılamaması dahil, siparişi durdurur ve yeniden onay ister. K-130'un kendisi *"sipariş kalan kalemlerle devam eder ve bu fark K-129 gereği güncel özette söylenir"* diyor. Yani önerilen davranış zaten kural. Ama `02`'deki §3.16.7 bu bağı yazmıyordu. *"Sipariş kalan kalemlerle devam eder"* cümlesi tek başına okununca, onaylanmamış bir özetle sipariş oluşuyormuş gibi anlaşılıyordu. Yeni karar gerekmedi. | §3.16.6 ve §3.16.7'ye bağ yazıldı: kalem özet gösterilmeden düştüyse özet onsuz hesaplanır; özet gösterildikten sonra düştüyse §3.17.2 siparişi durdurur ve müşteri yeniden onaylar (K-129). §6.2.4 de aynı şekilde güncellendi. |
| BULGU-3 | ⚠️ KISMİ | Birinci yarısı doğru. L-7 ve L-8'in e-posta ekseni misafirin ödeme adımında yazdığı doğrulanmamış adresi sayıyordu. Hedefin adresini yazıp art arda ödenmemiş sipariş açan biri, L-7 ile o kişinin 24 saat boyunca sipariş vermesini, L-8 ile havale yolunu kapatabilirdi. Üye de hedef olabiliyordu, çünkü hesabı olan e-posta misafir siparişine yazılabiliyor ve sipariş hesaba anında düşüyor (§3.13.3, K-99). Bu, K-329'un hesap kilidini eleme gerekçesinin aynısı, ama L-7 ve L-8 için düşünülmemişti. Daraltmanın bedeli düşük. Misafir e-postasını serbestçe değiştirebildiği için e-posta ekseni kasıtlı misafir saldırganını zaten durdurmuyordu; onu IP ekseni sayıyor. İkinci yarısı reddedildi: IP engeline kurtarma ya da alternatif yol açılması. Engel pencereyle kendiliğinden açılıyor (§8.2.1, K-329) ve L-8'de kart yolu zaten açık. Ayrı bir kurtarma yolu limiti atlatmanın kanalı olur. | K-579 `(öneriyle kaydedildi — ⚠)`: L-7 ve L-8'de e-posta ekseni yalnız giriş yapmış üyenin siparişlerini, hesabın e-postasıyla sayar. Misafir siparişi yalnız IP ekseninde sayılır. Kurtarma yolu açılmaz; paylaşılan IP arkasındaki müşterinin geçici engeli kalan risk olarak yazıldı. Güncellenen yerler: §3.17.8, §3.21.5, §6.2.19, §6.2.21, §8.2 (L-7, L-8, 8.2.2), §11.2 P-37 ve P-46. |
| BULGU-4 | ✅ KABUL | Doğru. §10.6.3 iptal ve iade oranını *"iptal kayıtlarından ve cayma beyanlarından"* hesaplıyordu. Gecikme feshi ise K-499 ile iptalden ayrı bir kayıt tipi oldu (§5.8, §7.2.3). Böylece otuz günlük sınır aşılıp feshedilen ve geri ödenen sipariş ölçüden düşüyordu, oysa ölçünün yakalamak istediği kayıp tam budur. Aynı bölümdeki kargoya verme sözüne uyum oranı feshi zaten "uyulmadı" sayıyor; iki ölçü aynı olayı farklı okuyordu. | K-580 `(öneriyle kaydedildi)`: gecikme feshi iptal ve iade oranına ve satış özetinin iptal ve iade sayısına girer. Feshedilen kalem, fesih bildiriminin tarih damgasında iptal edilen kalem gibi sayılır. Fesih ayrı bir kayıt tipi olarak kalır. §10.6.3. |

**Dağılım:** 1 KABUL · 2 KISMİ · 1 RET.

Önceki beş turun konuları bu turda dönmedi. BULGU-1 doğrulanmış bir karara yöneldi, ama metindeki yanlış yönetmelik adından ve eksik tarihten beslenmiş olabilir. Bu yüzden karar korunarak metin düzeltildi.

## 3. Ek bulgular

- **Etki yansıtma için not:** İki kararın başka dokümanlarda karşılığı var:
  - `01` §6 M-4 iptal ve iade oranını *"İptal kayıtlarından ve cayma beyanlarından"* hesaplıyor. K-580'den sonra kaynaklara gecikme feshi kayıtları da girer. `02` §10.6.2 bu ölçünün `01`'deki M-4 olduğunu söylediği için iki tanım hizalanmalı.
  - `10` §2 KP-72 *"Aynı e-posta ya da IP için açık ödenmemiş sipariş tavanı"* diyor. K-579'dan sonra e-posta ekseni yalnız üyenin siparişlerini sayar.

  İkisi de çelişki değil, ifade farkı: `01`'in tanımı eksik kalıyor, `10`'un satırı ekseni genel anıyor. Etki yansıtma adımında toplu hizalanacak.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`02` v0.21). İki karar satırı açıldı:

- K-579 `(öneriyle kaydedildi — ⚠)`
- K-580 `(öneriyle kaydedildi)`

K-579 önceki iki kararın (K-528, K-524) eksenini daralttığı ve güvenlik dengesine dokunduğu için proje sahibinin gözden geçirme listesine girer. Seçenekler gerçekten ayrışmıyor: daraltma kasıtlı saldırgana karşı korumayı azaltmıyor, yalnız mağdur senaryosunu kapatıyor. Karar kaydında K-528, K-499 ve K-416'nın etki sütunlarına yeni satırlara atıf eklendi.

- [x] BULGU-1 · [x] BULGU-2 · [x] BULGU-3 · [x] BULGU-4

**Hukuki kontrol:**

- **BULGU-1:** Fiyat Etiketi Yönetmeliğinde Değişiklik Yapılmasına Dair Yönetmelik'in Resmî Gazete'deki metnine (11/10/2025, sayı 33044; resmigazete.gov.tr/eskiler/2025/10/20251011-6.htm, 2026-10-03'te okundu) karşı kontrol edildi: m.5 (m.11/1'in ikinci ve üçüncü cümlesi) ve m.7 (yürürlük: yayımı tarihi). Ticaret Bakanlığı'nın değişiklik duyurusu da "indirimden önceki 10 gün içinde uygulanan en düşük fiyat" diyor.
- **BULGU-3 ve BULGU-4:** Hukuki iddia taşımıyor; doküman ve karar kaydına karşı değerlendirildi.
