# Cross-Review — 10 MVP Scope (Tur 5)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/10_MVP_SCOPE.md` v0.15 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde). İstem önceki turlarla aynıdır: daraltılmış talimat (K-615, K-644), yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1  
> Kriter: Teknik doğruluk  
> Seviye: Yüksek  
> Yer: KP-22  
> Alıntı: “teslimden sonraki caymada malın firmaya ulaştığı tarihten itibaren”  
> Sorun: Teslim sonrası caymada geri ödeme süresini malın firmaya ulaşmasına bağlar. Mesafeli Sözleşmeler Yönetmeliği m.12 uyarınca satıcının geri ödeme yükümlülüğü cayma bildiriminin ulaşmasıyla başlar; bu kural müşterinin iade taşımasının gecikmesi nedeniyle süreyi uzatacak şekilde kurulamaz.  
> Öneri: Teslim sonrası caymada geri ödeme süresini cayma bildiriminin firmaya ulaştığı tarihten başlatın; iade taşıma ve malın teslim alınması akışını bundan ayrı yönetin.
>
> BULGU-2  
> Kriter: Tutarlılık  
> Seviye: Yüksek  
> Yer: KP-5 / KP-39  
> Alıntı: “ölçüyle satılan üründe … birim fiyatı da görür.” / “alan yayın koşulu değildir; … alan boşsa birim fiyat gösterilmez”  
> Sorun: KP-5 ölçüyle satılan üründe birim fiyat gösterimini zorunlu kapsam olarak tanımlarken KP-39, aynı ürünün ölçü birimi ve net miktar olmadan yayımlanmasına izin veriyor. Böylece ziyaretçiye zorunlu birim fiyat gösterilmeden ölçüyle ürün satılabilir; bu hem iki satırla çelişir hem de fiyat etiketi yükümlülüğü bakımından uyumsuzdur.  
> Öneri: Ölçüyle satılan ürün için açık bir ürün niteliği seçimi ve ölçü birimi/net miktarı yayın ön koşulu yapın; bu alanlar yoksa ürün ölçüyle satılan olarak yayımlanamasın.
>
> BULGU-3  
> Kriter: Kullanıcı deneyimi  
> Seviye: Yüksek  
> Yer: KP-47  
> Alıntı: “bildirim siparişin iletişim e-postasından gelmediyse … teyit eder.” / “sahibine ulaşılamadığı için kaydedilmeyen gerçek bir bildirimin sonucu firmadadır.”  
> Sorun: E-posta veya mektupla süresinde yapılmış gerçek cayma bildirimi, müşteri eski iletişim kanalından teyit edilemezse sisteme hiç kaydedilmiyor. Oysa teyit, cayma hakkının geçerlilik şartı değildir. Bu durumda müşteri iade ve geri ödeme akışına giremez; firma da yasal süreyi işletemez.  
> Öneri: Bildirimi ulaştığı anda gerçek zaman damgasıyla kaydedin ve cayma/geri ödeme süresini başlatın. Kimlik şüphesi ayrı bir inceleme durumu olabilir; kaydı ve müşterinin hakkını bloke etmemelidir.
> ```

Model sonuca varmadan önce web aramasıyla dört konuya baktı (koşum kaydında görünür, çıktıya girmedi): cayma ve iade masrafı, Fiyat Etiketi Yönetmeliği'nin yerli üretim logosu, ETBİS kayıt tebliği, Mesafeli Sözleşmeler Yönetmeliği m.12. Son aramaya rağmen BULGU-1 m.12'nin 2022 değişikliğinden önceki metnine dayanıyor (§4).

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ❌ RET | **Hukuki iddia güncel metne uymuyor.** Mesafeli Sözleşmeler Yönetmeliği m.12, 23/8/2022 tarihli ve 31932 sayılı Resmî Gazete ile değişti; Geçici Madde 1 gereği yeni metin 1/1/2026'dan beri uygulanır. m.12/1 teslimden sonraki caymada süreyi malın ön bilgilendirmede belirtilen taşıyıcıya teslim edildiği tarihten, başka bir taşıyıcıyla iade edilen malda *"malın satıcıya ulaştığı tarihten"* başlatır. Süreyi bildirime bağlayan kural yalnız teslimden önceki caymada (m.12/2) ve hizmette (m.12/3) kalmıştır. Firma iade taşıyıcısı belirlemez (K-293), bu yüzden teslimden sonraki her iade "öngörülenin haricinde bir taşıyıcı" ile yapılır. KP-22 kuralı tam olarak buna göre yazıyor. Karar K-491 ile kaynaklı olarak alındı; K-209'un "beyandan say" kuralı o kararla değişti. Codex'in okuması 2014 metnidir. **Okumanın bir nedeni de metindeydi:** KP-22'nin gerekçe sütunu kuralın dayanağını yazmıyordu. Eski kural yaygın bilindiği için satır bir hata gibi okunabiliyordu. Gerekçeye dayanak tek cümleyle eklendi; kural değişmedi. | KP-22 Gerekçe: *"Teslimden sonraki caymada geri ödeme süresinin malın firmaya ulaşmasıyla başlaması Mesafeli Sözleşmeler Yönetmeliği m.12/1'in 1/1/2026'dan beri uygulanan kuralıdır: firma iade taşıyıcısı belirlemediği için her iade öngörülenin dışında bir taşıyıcıyla yapılır."* K-491 ve K-293 Kaynak sütununda zaten vardı. Öneri uygulanmadı. |
| BULGU-2 | ❌ RET | **Aynı konu ikinci kez geliyor ve doküman doğru.** 2. turda BULGU-1 olarak geldi; o turda KP-5 birim fiyatın firmanın girdiği alandan türediğini, KP-39 alanın neden yayın koşulu olmadığını yazar oldu. **Çelişki yok:** KP-5 birim fiyatı *"firmanın girdiği ölçü birimi ve net miktardan (KP-39)"* türetir, KP-39 alan boşsa birim fiyatın gösterilmediğini söyler. İki satır aynı kuralın iki yüzüdür. **Mevzuat sorunu da yok:** birim fiyat yükümlülüğü satıcınındır. Ürün yükümlülüğü kaldırmıyor, alanın doldurulmasını firmaya bırakıp panelde hatırlatıyor (K-573, K-14). Önerinin yayın koşulu kısmı K-573'te bilinçli olarak elendi; dokümanın kendisi de bunu yazıyor. **Dönmesinin sebebi:** önerinin öbür yarısı, "ölçüyle satılan ürün" için ayrı bir seçim, K-573'te de elenmişti ama `10` bunun nedenini yazmıyordu. Model bu yüzden onu yeni bir çözüm olarak getirdi. KP-39'un gerekçesine elemenin nedeni tek cümleyle eklendi. | KP-39 Gerekçe: *"Ürünün ölçüyle satıldığını ayrı bir seçimle beyan ettirmek de elendi: o beyan alanı doldurmakla aynı şeydir ve adetle satılan her ürüne bir adım ekler (K-573)."* K-573 Kaynak sütununda zaten vardı. Öneri uygulanmadı. |
| BULGU-3 | ⚠️ KISMİ | **Önerinin kendisi bilinçli olarak elenmiş bir seçenektir.** "Hemen kaydet, kimlik şüphesini ayrı bir inceleme durumunda tut" önerisi 4. turda da geldi. K-607 bu seçeneği eledi: yeni bir kalem durumu ve elle adım açar, geçmişe dönük tarih bildirimin tarihini zaten korur. Teyidin caymanın yasal şartı olmadığı 4. turda KP-47'nin gerekçesine yazıldı. **Ama "çıkışı olmayan akış" okumasının nedeni `10`'un metnindeydi.** 4. turda yazılan cümle *"sahibine ulaşılamadığı için kaydedilmeyen gerçek bir bildirimin sonucu firmadadır"* idi. Bu cümle, sahibine ulaşılamayınca kaydın yapılamayacağını, teyidin kaydı kilitlediğini ima ediyordu. K-607 bunu söylemiyor: *"Sahibine ulaşılamazsa kaydın yapılıp yapılmayacağı firmanın değerlendirmesidir"* (aynı ifade `02 §10.4.10`'da). Satır kararın bu yarısını taşımıyordu. Müşterinin kendi yolu da kapalı değildir: pencere açıkken sipariş sayfasından cayar (KP-22) ve orada teyit istenmez. | KP-47 Özellik: *"Teyit kaydın tarihini kaydırmaz ve teyitte geçen gün geri ödeme süresinden düşer. Sahibine ulaşılamazsa kaydı yapıp yapmamak firmanın değerlendirmesidir — teyit kaydı kilitlemez; kaydedilmeyen gerçek bir bildirimin sonucu firmadadır."* K-607 Kaynak sütununda zaten vardı. Yeni karar yok: K-607'nin sözü yazıldı. Teyitten önce kaydetme önerisi uygulanmadı. |

**Dağılım:** 0 KABUL · 1 KISMİ · 2 RET.

## 3. Ek bulgular

- **Mekanik tarama:** düzeltmelerde anılan karar numaraları (K-293, K-491, K-573, K-607) ilgili satırların Kaynak sütununda zaten var. Sürüm başlığı ve alt bilgi aynı sürümü gösteriyor (v0.16). Kapsam satırı sayısı değişmedi (77).
- **`02` ile hizalı:** `02 §7.4` (m.12/1–3'ün dayanağı ve K-293 gerekçesi), §3.8.5 (K-573) ve §10.4.10 (K-607'nin "firmanın değerlendirmesidir" ifadesi) bu turun üç düzeltmesini zaten yazıyor. `02`'de değişiklik gerekmiyor.
- **Tekrarlayan konular:** KP-47'nin teyidi iki tur üst üste geldi (4. ve 5. tur). İki turda da sebep satırın K-607'yi eksik taşımasıydı; bu turla satır kararın bütün parçalarını taşıyor. KP-5/KP-39'un birim fiyatı ikinci kez geldi. İkisi bir sonraki turda yine gelirse ve metin değişmediyse doğrudan RET olarak kalır.
- **Ciddi bir sorun görülmedi.** Bu turda ikinci modelin listesinin dışında ciddi bir sorun saptanmadı.
- **Etki yansıtmaya devredilen:** yeni bir iş yok. 1. ve 2. turdan devreden `02` notları yerinde duruyor: "İşlem izi" sözlük satırı, §8.5.2 ve §3.34.8.

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`10` v0.16). Bu turda yeni karar satırı açılmadı.

- [x] BULGU-1 (ret: m.12/1'in 1/1/2026'dan beri uygulanan metni teslimden sonraki caymada süreyi malın ulaşmasıyla başlatır, K-491. KP-22'nin gerekçesine dayanak yazıldı, kural değişmedi)
- [x] BULGU-2 (ret: tekrar eden konu; KP-5 ile KP-39 çelişmiyor, yayın koşulu K-573'te elendi. KP-39'un gerekçesine ayrı "ölçüyle satılır" seçiminin neden elendiği yazıldı)
- [x] BULGU-3 (kısmi: KP-47 sahibine ulaşılamayan bildirimde kaydı yapıp yapmamanın firmanın değerlendirmesi olduğunu ve teyidin kaydı kilitlemediğini yazar. "Teyit bekliyor" durumu önerisi K-607 gereği uygulanmadı)

**Hukuki kontrol** (2026-10-03): Mesafeli Sözleşmeler Yönetmeliği'nin mevzuat.gov.tr'deki konsolide metni (MevzuatNo 20237) okundu. m.12 başlığı *"(Değişik: RG-23/8/2022-31932)"* taşır. m.12/1: *"Satıcı, cayma hakkına konu malın, iade için ön bilgilendirmede belirtilen taşıyıcıya teslim edildiği tarihten itibaren on dört gün içinde, varsa malın tüketiciye teslim masrafları da dahil olmak üzere tahsil edilen tüm ödemeleri iade etmekle yükümlüdür. Ancak tüketicinin malı, iade için öngörülenin haricinde bir taşıyıcı ile iade etmesi durumunda söz konusu yükümlülük malın satıcıya ulaştığı tarihten itibaren başlar."* m.12/2: malın tesliminden önce caymada süre *"cayma hakkının kullanıldığına ilişkin bildirimin kendisine ulaştığı tarihten itibaren"* işler; m.12/3 hizmette aynı. Geçici Madde 1/1: 31932 ile değiştirilen *"b) 12 nci maddesi"* hükümleri *"(Değişik ibare: RG-10/8/2024-32628) 1/1/2026 tarihine kadar uygulanmaz"*. Bugün (2026-10-03) yeni metin uygulanıyor. Resmî Gazete'nin 23/8/2022 tarihli değişiklik metni de m.12/1–3'ü aynı sözle veriyor. Codex'in *"m.12 uyarınca … bildirimin ulaşmasıyla başlar"* okuması teslimden sonraki cayma için 1/1/2026'dan önceki metindir.

**Kaynaklar (hukuki kontrol):**
- [Mesafeli Sözleşmeler Yönetmeliği — mevzuat.gov.tr konsolide metin (PDF)](https://www.mevzuat.gov.tr/MevzuatMetin/yonetmelik/7.5.20237.pdf)
- [Mesafeli Sözleşmeler Yönetmeliğinde Değişiklik Yapılmasına Dair Yönetmelik — Resmî Gazete 23/8/2022, 31932](https://www.resmigazete.gov.tr/eskiler/2022/08/20220823-2.htm)
