# Cross-Review — 02 Product Requirements (Tur 13)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/02_PRODUCT_REQUIREMENTS.md` v0.28 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde; satır sonlarındaki boşluklar atıldı). İstem önceki turlarınkiyle aynıdır: yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: eksiklik
> Seviye: Yüksek
> Yer: §3.24.2 Onay adımı ve yasal metinler
> Alıntı: "Sepette dijital kalem varsa üçüncü bir kutu çıkar"
> Sorun: Dijital ürün ve hizmet için cayma hakkının düşmesine dayanak olan ayrı onayların siparişe hangi kanıtlarla kaydedileceği ve saklanacağı yazılmamış. Siparişe donan değerler listesi bu kutuların metnini, kabul anını veya kabul kaydını kapsamıyor.
> Öneri: Her ilgili kalem için onay metni/sürümü, kabul zaman damgası ve kabul kaydının siparişe donacağını; sipariş saklama süresi boyunca korunacağını açıkça ekleyin.
>
> BULGU-2
> Kriter: edge case
> Seviye: Yüksek
> Yer: §10.4.9 Malı dönmemiş cayma kalemi
> Alıntı: "mal firmaya ulaşmamışsa firma kalemi panelden kapatır"
> Sorun: Müşteri malı cayma beyanından itibaren 14 gün içinde kargoya vermiş, ancak taşıyıcı gecikmiş veya gönderiyi kaybetmiş olabilir. Kural yalnız firmanın malı teslim alıp almadığına bakarak geri ödemesiz kapatmaya izin veriyor; zamanında gönderim kanıtının nasıl değerlendirileceği yok.
> Öneri: Takip/teslim belgesiyle zamanında gönderimi kanıtlanan kalemlerin “mal dönmedi” gerekçesiyle kapatılamayacağını; taşıyıcı uyuşmazlığı için ayrı manuel inceleme ve sonuç kuralını tanımlayın.
>
> BULGU-3
> Kriter: tutarlılık
> Seviye: Orta
> Yer: §7.2.3 Teslimat gecikmesinde fesih
> Alıntı: "cayma istisnası işaretli ürünü de kapsar"
> Sorun: Bu kural 30 günlük gecikme feshi düğmesini kişiye özel üretilen mallar dahil tüm istisnalı fiziksel ürünlere açıyor. Oysa §3.20.4, 30 günlük yasal teslim üst sınırını “kişiye özel üretilen mallar dışında” diye tanımlıyor.
> Öneri: Kişiye özel üretilen ürünleri 30 günlük yasal fesih tetikleyicisinden çıkarın; daha geniş bir müşteri hakkı bilinçli olarak verilecekse bunu yasal hak diye değil, firmanın gönüllü ürün politikası olarak açıkça belirtin.
>
> SONUÇ: 3 BULGU
> ```

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ✅ KABUL | Doğru. K-204 ve K-503(a) dijital ve hizmet kalemindeki kutuyu cayma istisnasının *kurucu koşulu* yaptı. K-204 *"ispat yükü satıcıdadır"* dedi, ama siparişe donan değerler (§3.23.3, §3.24.6) yalnız Ön Bilgilendirme Formu'nun ve sözleşmenin sürümlerini sayıyordu. Kutunun kaydı yalnız `06`'ya yönlendirilmişti. Kutunun metni firmanın sürümlediği bir metin değil, `04`'ün yazdığı sabit bir metindir. Ürünün yeni bir sürümüyle değişebilir, bu yüzden "kutu işaretlendi" bilgisi tek başına müşterinin neyi kabul ettiğini göstermez. Yönetmelik m.20/1 cayma ve bilgilendirmeye ilişkin her işlemin belgesini üç yıl saklatır. Codex'in "kalem başına kabul zaman damgası" önerisi gerekmiyor: kutu sipariş düzeyindedir ve işaretlenmeden sipariş oluşmaz, kabulün anı siparişin oluştuğu andır. | K-595 `(öneriyle kaydedildi — ⚠)`. Siparişe hangi kutunun işaretlendiği, kutunun gösterilen metni ve kapsadığı kalemler donar. Kayıt Z-28'i izler, veri kaybı toleransı sıfır olan kayıtlara girer ve sipariş sayfasında görünür. Güncellenen yerler: §3.23.3, §3.24.6, §3.34.8, §4.2 Z-28, §7.3.2, §7.3.3, §12.1.7. |
| BULGU-2 | ⚠️ KISMİ | Sorun gerçek, önerilen çözüm uygun değil. K-496 malı *hiç gönderilmeyen* kalemi düşünerek kuruldu. Malın süresinde gönderilip yolda kaybolduğu durum yazılı değildi. §10.4.9'un *"geri ödeme yapılmaz"* cümlesi de kapatmayı uyuşmazlığın sonucu gibi okutuyordu. Ama Codex'in önerisi (zamanında gönderimi kanıtlanan kalem kapatılamasın, ayrı bir inceleme akışı kurulsun) alınmadı. Ürün taşıyıcı belgesini almaz ve doğrulayamaz: iletişim formunda dosya eki yoktur (K-301). Kapatma yalnız bekleyen işler sayacını temizler, cayma beyanını geçersiz kılmaz ve geri alınabilir; bunlar K-496'nın proje sahibince onaylanmış sonuçlarıdır. Taşıyıcı belirlenmediğinde Yönetmelik m.12/1 süreyi malın satıcıya ulaşmasına bağlar, kayıp gönderinin riskini ise açıkça düzenlemez. Ürün bu sonucu uyduramaz. | K-596 `(öneriyle kaydedildi — ⚠)`. Kapatma geri ödeme borcuna karar vermez. Kayıp gönderi bildirimi ürün dışında çözülür. Firma ödemeye karar verirse kaleme bağlı tutar bazlı geri ödemeyle (K-539) işler. Mal sonradan ulaşıp kalem yeniden açılırsa ödenmiş kısım kalemin geri ödemesinden düşülür. Riskin kimde olduğu kalan risk olarak yazıldı. Güncellenen yerler: §5.5, §10.4.9, yeni satır 6.4.30. |
| BULGU-3 | ⚠️ KISMİ | Çelişki gerçek. K-499 düğmeyi *"cayma istisnası işaretli ürünü de kapsar"* diye kurdu. Aynı audit turunda K-549 otuz günlük sınırın metnini *"kişiye özel üretilen mallar dışında"* diye düzeltti (m.16/1), ama düğmenin kapsamını bu düzeltmeyle uzlaştırmadı. Doküman otuz günü "yasal" diye anarken düğmeyi yasanın dışarıda bıraktığı mala da açıyordu. Codex'in birinci önerisi (kişiye özel malı düğmeden çıkar) elendi. Ürünün ayrı bir teslim süresi taahhüdü yoktur ve iptal düğmesi gönderi kargoya verildikten sonra kapanır. Kargoda geciken kişiye özel malda müşterinin sistem içinde yolu kalmazdı; K-499'un gerekçesi aynen işler. İkinci öneri (geniş hakkı ürün politikası olarak yaz) alındı. | K-597 `(öneriyle kaydedildi — ⚠)`. Düğme kişiye özel üretim istisnası işaretli üründe de otuz günde açılır. Bu yasal bir sınır değil, Mesafeli Satış Sözleşmesi'nin firma adına yazdığı taahhüttür. Sonuçları aynıdır. Güncellenen yerler: §1.2 (Gecikme feshi), §3.20.4, §4.2 Z-11, §6.4.2, §7.2.3, §12.1.8. |

**Dağılım:** 1 KABUL · 2 KISMİ · 0 RET.

Üç bulgunun hiçbiri bilinçli bir kararın elediği seçeneğe yönelmiyor ve hiçbiri önceki turlarda gelmedi:

- **BULGU-1:** K-204'ün ispat gerekçesi siparişe donan değerlere taşınmamıştı.
- **BULGU-2:** K-496 malın yolda kaybolduğu durumu kapsamıyordu. Codex'in önerdiği akış K-496'nın onaylanmış sonuçlarıyla çakıştığı için alınmadı; belirsizlik metinde giderildi.
- **BULGU-3:** K-499 ile K-549 aynı turda yazıldı ama birbirine hizalanmadı.

## 3. Ek bulgular

- **Fesihten sonra kargonun malı yine de teslim etmesi (11. turdan açık, işlenmedi):** Kargodaki siparişte fesih yapıldıktan sonra kargo malı firmaya döndürmeyip müşteriye teslim ederse ne olacağı hâlâ yazılı değil. Bu turda Codex bu konuyu getirmedi. Etki yansıtmada ya da sonraki turda bakılmalı.
- **Etki yansıtma için not:** `10 §2`'de K-597 (kişiye özel malda gecikme feshi taahhüdü) ve K-596 (kayıp iade gönderisi) KP satırlarına yansıtılmalı. `12`'nin sözleşme şablonu kişiye özel mal için otuz günlük taahhüdü yazmalı. `06` onay kutusu kaydını (K-595) taşımalı. `01`'de bu üç karar geçmiyor. Bu turda `01` ve `10`'a dokunulmadı.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`02` v0.29). Üç karar satırı açıldı:

- K-595 `(öneriyle kaydedildi — ⚠)`: yasal hakka dokunuyor (cayma istisnasının ispatı).
- K-596 `(öneriyle kaydedildi — ⚠)`: paraya dokunuyor ve K-539'un kullanımını genişletiyor.
- K-597 `(öneriyle kaydedildi — ⚠)`: yasal hakka ve paraya dokunuyor; firmaya yasal asgarinin üstünde bir taahhüt yazıyor.

- [x] BULGU-1 · [x] BULGU-2 · [x] BULGU-3

**Hukuki kontrol** (Mesafeli Sözleşmeler Yönetmeliği'nin konsolide metnine karşı, mevzuat.gov.tr, 2026-10-03):

- **BULGU-1:** m.20/1 satıcıya cayma hakkı ve bilgilendirmeyle ilgili her işlemin bilgi ve belgesini üç yıl saklatır. m.10/1 cayma hakkı konusunda bilgilendirmenin ispatını satıcıya verir. Siparişin on yıllık süresi (Z-28) bunu kapsar.
- **BULGU-2:** m.12/1'e göre tüketici malı ön bilgilendirmede belirtilenden başka bir taşıyıcıyla iade ederse geri ödeme süresi malın satıcıya ulaşmasıyla başlar. m.12/5 taşıyıcı belirtilmediğinde tüketiciden iade masrafı istenmesini yasaklar. m.13/1 tüketiciye on dört gün içinde gönderme yükü verir. Yolda kaybolan iade malının riskini açıkça düzenleyen bir hüküm yok; ürün bu sonuca karar vermiyor.
- **BULGU-3:** m.16/1 otuz günlük üst sınırı *"tüketicinin isteği veya kişisel ihtiyaçları doğrultusunda hazırlanan mallara ilişkin sözleşmeler haricinde"* uygular. m.16/2 taahhüt edilen sürenin aşılmasına bu mallarda da fesih hakkı bağlar. Ürünün otuz günü bu mallarda taahhüt olarak yazması yasal asgarinin altına düşmüyor.
