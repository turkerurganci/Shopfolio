# Cross-Review — 10 MVP Scope (Tur 13)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/10_MVP_SCOPE.md` v0.23 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde; yalnız satır sonu boşlukları atıldı). İstem önceki turlarla aynıdır: daraltılmış talimat (K-615, K-644), yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: KP-22
> Alıntı: “firma iade taşıyıcısı belirlemez, her iade öngörülenin dışında bir taşıyıcıyla yapılır”
> Sorun: Taşıyıcı hiç belirtilmiyorsa tüketicinin herhangi bir taşıyıcıyla yaptığı iadede geri ödeme süresi malın kargoya verildiği tarihte başlar. Doküman ise süreyi malın firmaya ulaşmasına bağlayarak sistematik geç iade ve yaptırım riski yaratır. [Ticaret Bakanlığı açıklaması](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)
> Öneri: Ya ön bilgilendirmede iade taşıyıcısı tanımlansın ya da taşıyıcı belirtilmeyecekse geri ödeme sayacı müşterinin gönderiyi taşıyıcıya teslim ettiği tarihte başlatılsın.
>
> BULGU-2
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: KP-39
> Alıntı: “alan yayın koşulu değildir; doldurmak firmanın yükümlülüğüdür, panel bunu hatırlatır ve alan boşsa birim fiyat gösterilmez”
> Sorun: Ölçüyle satılan malın birim fiyatı gösterilmeden yayına ve satışa izin veriliyor. Bu, Fiyat Etiketi Yönetmeliği’nin perakende satışta birim fiyat gösterme zorunluluğuyla çelişir; firma için idari yaptırım ve tüketici için yanıltıcı fiyat riski doğurur. [Ticaret Bakanlığı bilgisi](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/fiyat-etiketleri-hakkinda-bilgilendirme)
> Öneri: Ölçüyle satış beyanını ve ölçü birimi/net miktar alanlarını zorunlu, doğrulanabilir bir yayın koşulu yapın; birim fiyat hesaplanamıyorsa ürün satın alınabilir olmamalı.
>
> BULGU-3
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: KD-28
> Alıntı: “sistem ne satıldığını denetlemez”
> Sorun: Doküman aynı satırda alkol ve tütünün tüketiciye internetten satışının yasak olduğunu kabul ettiği hâlde, genel katalog ve ödeme akışı bu ürünleri engellemeden satışa açabiliyor. Bu, yasak satışın gerçekleşmesine ve firmaya ağır yaptırım riskine doğrudan yol açar.
> Öneri: Yasak ürün kategorileri için yayınlama ve satın alma engeli konulsun; ürün türü/satış kategorisi beyanı üzerinden denetlenebilir bir kontrol oluşturulsun.
> ```

Model sonuca varmadan önce dört konuyu web aramasıyla kontrol etti (koşum kaydında görünür, çıktıya girmedi): m.12'nin 1/1/2026'dan beri uygulanan metni, 6502 sayılı Kanun'da ayıplı hizmette zamanaşımı, Fiyat Etiketi Yönetmeliği'nde birim fiyat ve ETBİS kayıt yükümlülüğü. Üç bulgu yazdı; üçü de önceki turlarda gelip reddedilen konulardır. Yalnız ikisine kaynak gösterdi; ikisi de Bakanlığın önceki turlarda açılmış rehberleridir.

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ❌ RET | **Konu yedinci kez geliyor ve doküman doğru** (5., 6., 8., 9., 10., 11. ve bu tur). Sonuç ve öneri 6., 8., 9. ve 11. turla aynıdır: taşıyıcı belirtilmeyince süre kargoya verilişten başlamalı ya da firma taşıyıcı belirlemeli. **Dayanak yine Bakanlık rehberidir ve rehber bu kuralı koymaz:** bu turda yeniden açılan rehber genel kuralı *"Bu süre, tüketicinin malı kargoya teslim ettiği tarihinden itibaren başlar"*, istisnayı *"İade işlemi, ön bilgilendirmede belirtilenden farklı bir kargo ile yapılmışsa bu süre, ürünün satıcıya ulaştığı tarihte başlar"* diye anlatır. Taşıyıcı belirtilmeyen hâl için yalnız masrafı söyler, süreye ayrı bir cümle ayırmaz. Yönetmelik de bu hâlin süresini düzenlemez: m.12/1 iki hâli, m.12/5 taşıyıcısız hâlde yalnız masrafı düzenler (11. turda RG 32909'un resmî metninden kontrol edildi; bu turda m.12'yi sonradan değiştiren bir metin bulunmadı). **Modelin "süre kargoya verildiği tarihte başlar" cümlesi bir hüküm değil, KP-22'de yazılı kalan risktir:** model gerekçenin *"firma iade taşıyıcısı belirlemez, her iade öngörülenin dışında bir taşıyıcıyla yapılır"* cümlesini alıntıladı; aynı gerekçenin iki cümle sonraki *"Kalan risk bilinçlidir: taşıyıcı hiç belirtilmediğinde bu sonucun açık hükmü yoktur — tüketici lehine bir yorum başlangıcı malın kargoya verildiği güne çekebilir; firma geri ödemeyi iade gönderisinin kargoya verildiği günden on dört gün içinde yaparsa iki okumada da süreyi kaçırmaz"* cümlesini almadı. "Sistematik geç iade" iddiası bu cümleyle karşılanmıştır. **Öneri elenmiş seçenektir:** taşıyıcı belirlemek K-293 ve K-491'de elendi; m.12/5 ve m.5/1-g taşıyıcı belirtmemeyi açıkça öngörür (K-493); sistem kargo entegrasyonu olmadan kargoya veriliş tarihini öğrenemez (K-132). **Dönüşün sebebi metinde bir belirsizlik değil:** satır kuralı, dayanağı, taşıyıcının zorunlu olmadığını, elenen seçeneği ve kalan riski yazıyor. Rehberi kalan riskin kaynağı olarak gerekçeye adıyla eklemek düşünüldü ve yapılmadı: 11. turda yazıldığı gibi dönüşü kesmez, satırı büyütür ve yazılı kalan riskle aynı şeyi söyler. | Uygulanmadı. |
| BULGU-2 | ❌ RET | **Konu beşinci kez geliyor ve doküman doğru** (2., 5., 7., 9. ve bu tur). **Öneri elenmiş seçenektir:** ölçüyle satış beyanını ya da alanı yayın koşulu yapmak K-573'te iki adla elendi: *"Ölçüyle satılan üründe alan zorunlu yayın koşulu olsun"* (beyanı yine firma verir ve bu beyan alanı doldurmakla aynı şeydir) ve *"Her fiziksel üründe satış birimi (adet ya da ölçü) seçimi zorunlu olsun"* (adetle satılan ürünlerin çoğuna her kayıtta bir adım ekler). KP-39 ikisini ve kalan riski yazar: *"Kalan risk bilinçlidir: firma ölçüyle sattığı ürünü alanı doldurmadan yayına alırsa birim fiyat gösterilmez ve bunun yasal sonucu satıcı olan firmadadır (K-573)."* **Hukuki iddia:** modelin gösterdiği Bakanlık fiyat etiketi rehberi bu turda yeniden açıldı. Rehber birim fiyatı *"satıcı tarafından perakende satışa sunulan mallar"* için ister ve yükümlülüğü satıcıya yazar; yazılıma ya da platforma yükleyen bir cümle yoktur. Fiyat Etiketi Yönetmeliği'nin değişiklikleri (RG 33044, 33153) 7. turda kontrol edildi. **Dokümanda çelişki yoktur:** alan yayın koşulu değildir, doldurmak firmanın yükümlülüğüdür ve panel bunu hatırlatır; "yayına izin veriliyor" sözü kuralın kendisini anlatır, bir aykırılık göstermez. Sistem bir ürünün ölçüyle satıldığını kayıttan bilemez (K-84). Model bu kez de gerekçeyi değil, yalnız Özellik sütununu alıntıladı. | Uygulanmadı. |
| BULGU-3 | ❌ RET | **Konu üçüncü kez geliyor ve doküman doğru** (6., 12. ve bu tur). Bulgu 12. turun BULGU-2'sinin aynısıdır; bu kez kaynak da göstermiyor. **Öneri elenmiş seçenektir:** yasak kategoriler için yayın ve satın alma engeli K-84'te elendi (*"Engelleme mekanizması, yasaklı kategori listesi ve ürün başına mevzuat belgesi alanı yoktur"*); KD-28'in bugünkü cümlesi K-641'dir. **Dokümanda çelişki yoktur:** KD-28 sistemin satışı denetlemediğini, gerekçesi ise alkol ve tütünün internetten tüketiciye satışının yasak olduğunu ve *"Satılan ürünün mevzuata uygunluğu satıcı olan firmanın yükümlülüğüdür"* olduğunu birlikte yazar. Yasağı bilen ve uygulamayı satıcıya bırakan kural kendi içinde tutarlıdır. **Hukuki dayanak 12. turda resmî metinden okundu:** 4733 sayılı Kanun m.8/5-k cezayı bu ürünleri elektronik ticaret araçlarıyla satmak üzere *"satış sistemi kuran veya faaliyette bulunanlara"* verir; muhatap satıcı firmadır (K-12). Bu turda yeni bir dayanak gösterilmedi. | Uygulanmadı. |

**Dağılım:** 0 KABUL · 0 KISMİ · 3 RET. Bu turdaki yüzde yüz ret körü körüne verilmiş bir onay değildir. Üç bulgu da dokümanda elenen seçenek, kalan risk ya da yükümlüsü yazılmış kural olarak duran kararları (K-491, K-573, K-84, K-641) yeni bir resmî dayanak olmadan yeniden açıyor. Modelin gösterdiği iki Bakanlık rehberi bu turda yeniden açıldı; ikisi de modelin iddia ettiği kuralı koymuyor. Hiçbir bulgu dokümanın iki yeri arasında bir çelişki ya da olgusal bir hata göstermiyor. Üçü de satırın kuralını alıntılıyor, aynı satırın gerekçesindeki kalan riski ya da yükümlüyü okumadan sorun olarak sunuyor.

## 3. Ek bulgular

- **Mekanik tarama:** sürüm başlığı, sürüm notu ve alt bilgi aynı sürümü gösteriyor (v0.24). Kapsam satırı sayısı değişmedi (77), kapsam dışı satırı sayısı değişmedi (63).
- **Tekrarlayan konular:** KP-22'nin geri ödeme başlangıcı yedi turda (5., 6., 8., 9., 10., 11., 13.), KP-39'un ölçü birimi alanı beş turda (2., 5., 7., 9., 13.), KD-28'in satış engeli üç turda (6., 12., 13.) geldi. Bu turda yeni bir konu gelmedi. Üç satır da kuralı, elenen seçeneği ve kalan riski ya da yükümlüyü adıyla yazıyor; model üç bulguda da gerekçenin bu kısmını alıntılamadı. Ciddi sorun sınıfında doküman doygunluğa ulaşmış görünüyor: son beş turda (9.–13.) kabul edilen bulgu yok ve gelen her bulgu kayıtlı bir kararın tekrarı. Bu konular yeni bir resmî dayanak (mevzuat değişikliği ya da Bakanlık kararı) olmadan yeniden gelirse doğrudan RET olarak kalır.
- **Ciddi bir sorun görülmedi.** Bu turda ikinci modelin listesinin dışında ciddi bir sorun saptanmadı. Modelin aradığı diğer konularda (ayıplı hizmette zamanaşımı, ETBİS kaydı) bulgu yazılmadı.
- **Önceki turlardan devreden notlar** yerinde duruyor: "İşlem izi" sözlük satırı, §8.5.2, §3.34.8 ve §12.1.12'ye 4733 m.8/5-k dayanağı (isteğe bağlı); `02 §3.1.4`'teki "kayıt yükümlülüğü her firmada doğmaz" gerekçesi (8. tur, etki yansıtmaya devredildi).

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`10` v0.24; yalnız sürüm notu). Bu turda yeni karar satırı açılmadı. K-84, K-491, K-573 ve K-641 kayıtlı kararlardır; bu tur hiçbirini değiştirmedi.

- [x] BULGU-1 (ret: yedinci kez gelen konu; Yönetmelik de Bakanlık rehberi de taşıyıcı belirtilmeyen hâlde sürenin başlangıcını düzenlemez, iddia KP-22'de ve K-491'de kalan risk olarak yazılı; taşıyıcı belirleme K-293 ve K-491'de elendi. Doküman değişmedi)
- [x] BULGU-2 (ret: beşinci kez gelen konu; ölçüyle satış beyanını ya da alanı yayın koşulu yapmak K-573'te elendi; birim fiyat yükümlülüğü Bakanlık rehberinde satıcıya aittir, kalan risk KP-39'da yazılı. Doküman değişmedi)
- [x] BULGU-3 (ret: üçüncü kez gelen konu, 12. turun BULGU-2'si ile aynı; satış engeli K-84'te elendi, KD-28'in dili K-641'dir, 4733 m.8/5-k'nin muhatabı satıcıdır. Doküman değişmedi)

**Hukuki kontrol** (2026-10-03):
- **Ticaret Bakanlığı tüketici rehberi — mesafeli sözleşmeler** (bu turda yeniden açıldı): *"Bu süre, tüketicinin malı kargoya teslim ettiği tarihinden itibaren başlar."* · *"İade işlemi, ön bilgilendirmede belirtilenden farklı bir kargo ile yapılmışsa bu süre, ürünün satıcıya ulaştığı tarihte başlar."* Taşıyıcısız hâl için yalnız masraf: tüketicinin malı *"herhangi bir kargo şirketiyle geri göndermesi halinde tüketici iadeye ilişkin masraflardan sorumlu tutulamaz."* Bu hâlde sürenin başlangıcını ayrıca düzenleyen bir cümle yoktur.
- **Mesafeli Sözleşmeler Yönetmeliği m.12** (11. turda RG 24/5/2025-32909'un resmî metni ve konsolide metin okundu): m.12/1 belirtilen taşıyıcıya teslimi ve öngörülenin dışındaki taşıyıcıyı düzenler; m.12/5 taşıyıcı belirtilmeyen hâlde yalnız masrafı düzenler. **Sonraki değişiklik taraması** bu turda yinelendi: 24/5/2025 tarihli değişiklikten sonra m.12'yi değiştiren yeni bir Resmî Gazete metni bulunmadı.
- **Ticaret Bakanlığı tüketici rehberi — fiyat etiketleri** (bu turda yeniden açıldı): birim fiyat *"satıcı tarafından perakende satışa sunulan malların"* fiyat etiketinde yer alır; yükümlülük satıcıya ve işletmeye yazılır, platforma ya da yazılım sağlayıcısına yükleyen bir cümle yoktur. Rehberde internet satışına özgü ayrı bir birim fiyat kuralı da yoktur.
- **4733 sayılı Kanun m.8/5-k** (12. turda mevzuat.gov.tr konsolide metinden okundu): ceza bu ürünleri elektronik ticaret araçlarıyla satmak üzere *"satış sistemi kuran veya faaliyette bulunanlara"* verilir.

**Kaynaklar (hukuki kontrol):**
- [Ticaret Bakanlığı — Mesafeli Sözleşmeler Hakkında Bilgilendirme](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)
- [Ticaret Bakanlığı — Fiyat Etiketleri Hakkında Bilgilendirme](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/fiyat-etiketleri-hakkinda-bilgilendirme)
- [Resmî Gazete 24/5/2025-32909 — Mesafeli Sözleşmeler Yönetmeliğinde Değişiklik Yapılmasına Dair Yönetmelik](https://www.resmigazete.gov.tr/eskiler/2025/05/20250524-2.htm)
- [Mesafeli Sözleşmeler Yönetmeliği — mevzuat.gov.tr konsolide metin (PDF)](https://mevzuat.gov.tr/File/GeneratePdf?mevzuatNo=20237&mevzuatTur=KurumVeKurulusYonetmeligi&mevzuatTertip=5)
- [4733 sayılı Kanun — mevzuat.gov.tr konsolide metin (PDF)](https://www.mevzuat.gov.tr/MevzuatMetin/1.5.4733.pdf)
