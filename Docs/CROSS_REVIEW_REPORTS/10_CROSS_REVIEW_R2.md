# Cross-Review — 10 MVP Scope (Tur 2)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/10_MVP_SCOPE.md` v0.12 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki tur ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde). İstem 1. turla aynıdır: daraltılmış talimat (K-615, K-644), yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §2 / KP-5, KP-39
> Alıntı: "ölçüyle satılan üründe birim fiyatı da görür." / "ölçüyle satılan üründe ölçü birimi ile net miktarı girer — alan yayın koşulu değildir"
> Sorun: Ölçü birimi ve net miktar girilmeden ürün yayına alınabiliyor; bu veriler olmadan birim fiyat hesaplanıp gösterilemez. Buna rağmen KP-5 birim fiyatı zorunlu kılıyor. Sonuç, zorunlu fiyat bilgisini göstermeyen ölçülü ürün satışı olabilir.
> Öneri: Ölçüyle satılan ürünlerde ölçü birimi ve net miktarı yayın koşulu yapın; bu alanlar yoksa ürün yayına alınamasın ve satışa kapalı kalsın.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §2 / KP-63, KP-75
> Alıntı: "iz on yıllık saklama süresi boyunca değiştirilemez ve silinemez, süre dolunca kendiliğinden imha edilir" / "Onaylanmış sipariş, ödemesi, durum geçişleri, kalem düzeyindeki kayıtlar ... ve işlem izi kaybolmaz"
> Sorun: İşlem izinin on yıl sonunda imha edilmesi ile hiç kaybolmaması aynı anda sağlanamaz. Bu, saklama-imha davranışını ve doğrulama kriterini belirsiz ve çelişkili bırakır.
> Öneri: KP-75’i, işlem izinin “uygulanabilir saklama süresi boyunca kaybolmayacağı” şeklinde düzeltin; on yıl sonundaki imhayı açık istisna olarak belirtin.
> ```

Model sonuca varmadan önce dört konuyu web aramasıyla kontrol etti (koşum kaydında görünür, çıktıya girmedi): Mesafeli Sözleşmeler Yönetmeliği'nde cayma, geri ödeme ve malın geri gönderildiğinin ispatı; ETBİS kayıt yükümlülüğü; Fiyat Etiketi Yönetmeliği'nde indirimli satışın referans fiyatı. Bunlar için bulgu yazmadı.

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ⚠️ KISMİ | **Önerinin kendisi bilinçli olarak elenmiş bir seçenektir.** K-573 *"ölçüyle satılan üründe alan zorunlu yayın koşulu olsun"* seçeneğini eledi: sistem bir ürünün kilo, litre ya da metreyle satıldığını ürün kaydından bilemez. Ürünün ölçüyle satıldığını yine firmanın beyan etmesi gerekir ve bu beyan alanı doldurmakla aynı şeydir. Fiyat Etiketi Yönetmeliği'nin yükümlüsü satıcıdır (m.4, "satıcı" tanımı), yani firmadır (K-14). Sorun `02`'nin 4. turunda da gelmişti ve K-573 ile kapanmıştı. **Ama okumanın bir sebebi `10`'un metnindeydi.** KP-5 ziyaretçinin birim fiyatı koşulsuz *"görür"* olduğunu yazıyor. KP-39 ise alanın yayın koşulu olmadığını söylüyor ama gerekçeyi ve boş alanın sonucunu yazmıyor. İki satır yan yana okununca sistem bir güvence veriyor, sonra onu delik bırakıyor gibi duruyordu. K-511 birim fiyatın yalnız alan doluysa türetildiğini söyler. | KP-5: birim fiyat *"firmanın girdiği ölçü birimi ve net miktardan (KP-39)"* türer; Kaynak sütununa K-573 eklendi. KP-39: *"sistem ürünün ölçüyle satıldığını bilemediği için alan yayın koşulu değildir; doldurmak firmanın yükümlülüğüdür, panel bunu hatırlatır ve alan boşsa birim fiyat gösterilmez"*. Yeni karar yok: K-511 ve K-573'ün sözü yazıldı. Yayın koşulu önerisi uygulanmadı. |
| BULGU-2 | ✅ KABUL | **Çelişki 1. turun düzeltmesinin yan etkisidir.** 1. tur KP-63'e izin on yıl sonra imha edildiğini yazdı (K-355). KP-75 ise izi, süre belirtmeden *"kaybolmaz"* listesinde sayıyordu. Sıfır kayıp kararı (K-407) kazaya ve arızaya karşıdır, saklama süresini uzatmaz: K-407 kendi gerekçesinde K-115'in ve K-353'ün yasal saklama sürelerine dayanır, süre dolunca kişisel veri K-354 ile imha edilir. Aynı okuma yalnız iz için değil, listedeki öteki kayıtlar için de mümkündü: sipariş ve iletişim talebi de süre sonunda KP-74'ün imhasına girer. Bu yüzden düzeltme bütün listeye uygulandı. `§1`'deki kabul cümlesi "veri kaybına tolerans" başlığı altında durduğu için aynı okumaya açık değildir. | KP-75: liste *"saklama süreleri boyunca kaybolmaz — süre sonundaki imha KP-74'ündür —"*. Kaynak sütununa K-354 eklendi. Yeni karar yok. |

**Dağılım:** 1 KABUL · 1 KISMİ · 0 RET.

## 3. Ek bulgular

- **Mekanik tarama:** `10`'da anılan bütün karar numaraları karar kaydında satır olarak var; bu turda eklenen K-354 ve K-573 de var. Sürüm başlığı ve alt bilgi aynı sürümü gösteriyor (v0.13).
- **Ciddi bir sorun görülmedi.** Bu turda ikinci modelin listesinin dışında ciddi bir sorun saptanmadı.
- **Etki yansıtmaya devredilen:** `02 §3.34.8` de sıfır kayıp listesini süre belirtmeden *"kaybolamaz"* diye yazıyor. `02`'de çelişki yok: madde "veri kaybı toleransı" başlığını taşır ve imha §12.2'de ayrıca yazılıdır; `02`'nin cross-review'ı TEMİZ döndü. KP-75'in yeni ifadesiyle hizalanıp hizalanmayacağı etki yansıtma adımında değerlendirilir. 1. turdan devreden "İşlem izi" sözlük satırı ve §8.5.2 notu yerinde duruyor.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`10` v0.13). Bu turda yeni karar satırı açılmadı.

- [x] BULGU-1 (kısmi — KP-5 ve KP-39 birim fiyatın firmanın girdiği alandan türediğini ve alanın neden yayın koşulu olmadığını yazar; yayın koşulu önerisi K-573 gereği uygulanmadı)
- [x] BULGU-2 (kabul — KP-75'in sıfır kayıp listesi saklama süreleri içindir, süre sonundaki imha KP-74'ündür)

**Hukuki kontrol** (2026-10-03): Fiyat Etiketi Yönetmeliği m.4 birim fiyatı *"malın türüne uygun olan ölçü biriminin net miktarıyla ifade edilen tüm vergiler dâhil fiyat"* diye, satıcıyı *"ticari veya mesleki amaçlarla tüketiciye mal sunan"* kişi diye tanımlar; m.5/6'ya göre birim fiyat satış fiyatıyla aynıysa belirtilmesi gerekmez. Yükümlülük satıcıdadır ve satıcı firmadır. 11 Ekim 2025 tarihli değişiklik m.5'in birim fiyat hükmüne dokunmaz; m.11'de mal satışlarında indirimin referansı olarak *"indirimin uygulandığı tarihten önceki on gün içinde uygulanan en düşük fiyat"* esasını korur, KP-43 ile uyumludur.

**Kaynaklar (hukuki kontrol):**
- [Fiyat Etiketi Yönetmeliği — 28.06.2014 tarihli Resmî Gazete metni (alomaliye.com yayımı)](https://alomaliye.com/2014/06/28/fiyat-etiketi-yonetmeligi/)
- [Fiyat Etiketi Yönetmeliğinde Değişiklik Yapılmasına Dair Yönetmelik — Resmî Gazete, 11.10.2025](https://resmigazete.gov.tr/eskiler/2025/10/20251011-6.htm)
- [Fiyat Etiketi Yönetmeliğinde Değişiklik — Resmî Gazete, 18.09.2018](https://www.resmigazete.gov.tr/eskiler/2018/09/20180918-1.htm)
