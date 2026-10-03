# Cross-Review — 10 MVP Scope (Tur 11)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/10_MVP_SCOPE.md` v0.21 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde; yalnız satır sonu boşlukları atıldı). İstem önceki turlarla aynıdır: daraltılmış talimat (K-615, K-644), yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §2 / KP-22
> Alıntı: "teslimden sonraki caymada malın firmaya ulaştığı tarihten itibaren"
> Sorun: Firma iade taşıyıcısı belirlemediği hâlde teslim sonrası caymada geri ödeme süresi malın firmaya ulaşmasına bağlanmış. Güncel kuralda, taşıyıcı belirtilmemişse süre tüketicinin malı taşıyıcıya teslim ettiği tarihte başlar. Bu kural, firmayı yasal sürenin dışında iadeye ve tüketiciyi hak kaybına sürükler.
> Öneri: KP-22’yi, taşıyıcı belirlenmediğinde geri ödemenin tüketicinin malı taşıyıcıya teslim ettiği tarihten itibaren on dört gün içinde yapılacağı şekilde düzeltin; malın firmaya ulaşmasını başlangıç kabul etmeyin.
> ```

Model sonuca varmadan önce dört konuyu web aramasıyla kontrol etti (koşum kaydında görünür, çıktıya girmedi): Ticaret Bakanlığı sitesinde 1/1/2026'dan beri uygulanan m.12 ve geri ödemede taşıyıcı, Erişilebilirlik tebliğinin 21/6/2027 tarihi, 1/1/2026'dan beri telefon, tablet ve bilgisayarda cayma hakkı, e-ticaret hizmet sağlayıcısı yönetmeliğinde KEP adresi. Bulguya bir kaynak bağlantısı koymadı; KP-22 dışındaki konularda bulgu yazmadı.

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ❌ RET | **Konu altıncı kez geliyor ve doküman doğru** (5., 6., 8., 9., 10. ve bu tur). Sonuç ve öneri 6., 8. ve 9. turla aynıdır: taşıyıcı belirtilmeyince süre kargoya verilişten başlamalı. **Bu turun dayanağı olgusal olarak yanlıştır:** model *"güncel kuralda, taşıyıcı belirtilmemişse süre tüketicinin malı taşıyıcıya teslim ettiği tarihte başlar"* diyor ve kaynak göstermiyor. Böyle bir hüküm yoktur. m.12/1 yalnız iki hâli düzenler: ön bilgilendirmede belirtilen taşıyıcıya teslim (süre teslimden başlar) ve öngörülenin dışındaki taşıyıcı (süre malın satıcıya ulaşmasından başlar). 1/1/2026'da yürürlüğe giren değişiklik (RG 24/5/2025-32909) m.12'de yalnız 4. ve 5. fıkraya dokunur; m.12/5 taşıyıcı belirtilmeyen hâl için yalnız masrafı düzenler, süreyi düzenlemez. Ticaret Bakanlığı'nın tüketici rehberi genel kuralı *"Bu süre, tüketicinin malı kargoya teslim ettiği tarihten itibaren başlar"* diye, istisnayı *"ön bilgilendirmede belirtilenden farklı bir kargo ile yapılmışsa"* diye anlatır. Taşıyıcısız hâl için süreye ayrı bir cümle ayırmaz; o hâl için yalnız masrafı söyler. Rehber modelin okumasına yakındır ama bir kural koymaz; bu fark 6. turda görüldü ve K-491'in kalan riski olarak yazıldı. **İddia KP-22'de kayıtlı kalan risktir:** gerekçe *"taşıyıcı hiç belirtilmediğinde bu sonucun açık hükmü yoktur — tüketici lehine bir yorum başlangıcı malın kargoya verildiği güne çekebilir"* diyor. **"Hak kaybı" iddiası da karşılanmış:** gerekçe firmanın geri ödemeyi iade gönderisinin kargoya verildiği günden on dört gün içinde yaparsa iki okumada da süreyi kaçırmadığını yazar; iki okuma arasındaki fark yalnız kargonun yolda geçirdiği günlerdir, müşterinin alacağı tutar değişmez. **Öneri elenmiş seçenektir:** sistem kargo entegrasyonu olmadan kargoya veriliş tarihini öğrenemez (K-132); taşıyıcı belirleme K-293 ve K-491'de elendi; proje sahibi K-491'de bu riski bilerek kuralı seçti. **Metinde dönüşe yol açan yeni bir boşluk görülmedi:** satır kuralı, dayanağını, taşıyıcı belirlemenin zorunlu olmadığını, elenen seçeneği ve kalan riski yazıyor. Model bu kez satırın gerekçesini değil, Özellik sütunundaki kuralı alıntıladı ve kalan riski kural diye sundu. Gerekçeye bir cümle daha eklemek dönüşü kesmez, satırı büyütür. | Uygulanmadı. Doküman yalnız sürüm notu aldı (v0.22). |

**Dağılım:** 0 KABUL · 0 KISMİ · 1 RET. Tek bulgunun reddi körü körüne verilmiş bir onay değildir. Bulgunun dayanağı olan "güncel kural" Yönetmelik'te yoktur: m.12/1'in konsolide metni ve 1/1/2026'da yürürlüğe giren değişikliğin resmî metni bu turda yeniden kontrol edildi. Bakanlık rehberinin bu okumaya yakın anlatımı 6. turda görüldü ve kalan risk olarak yazıldı. Bulgu dokümanın iki yeri arasında bir çelişki göstermiyor.

## 3. Ek bulgular

- **Mekanik tarama:** sürüm başlığı, sürüm notu ve alt bilgi aynı sürümü gösteriyor (v0.22). Kapsam satırı sayısı değişmedi (77), kapsam dışı satırı sayısı değişmedi (63).
- **Tekrarlayan konular:** KP-22'nin geri ödeme başlangıcı altı turda (5., 6., 8., 9., 10., 11.) geldi. Dayanak her turda değişti: m.12/1'in 2014 lafzı, Bakanlık rehberi, m.5/1-g ve bu turda kaynaksız bir "güncel kural". Hiçbir turda Yönetmelik'te taşıyıcısız hâlin süresini düzenleyen bir hüküm gösterilmedi. Konu yeni bir resmî dayanak olmadan yeniden gelirse doğrudan RET olarak kalır. Konuyu yeniden açacak tek şey taşıyıcısız hâlin süresini düzenleyen yeni bir mevzuat değişikliği ya da Bakanlık kararıdır; bu turda böyle bir metin bulunmadı.
- **Ciddi bir sorun görülmedi.** Bu turda ikinci modelin listesinin dışında ciddi bir sorun saptanmadı. Modelin aradığı diğer konularda (telefon, tablet ve bilgisayarda cayma hakkı, KEP adresi, erişilebilirlik tarihi) bulgu yazılmadı.
- **Önceki turlardan devreden notlar** yerinde duruyor: "İşlem izi" sözlük satırı, §8.5.2, §3.34.8 ve §12.1.12'ye 4733 m.8/5-k dayanağı (isteğe bağlı); `02 §3.1.4`'teki "kayıt yükümlülüğü her firmada doğmaz" gerekçesi (8. tur, etki yansıtmaya devredildi).

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`10` v0.22). Bu turda yeni karar satırı açılmadı. K-132, K-293 ve K-491 kayıtlı kararlardır; bu tur hiçbirini değiştirmedi.

- [x] BULGU-1 (ret: altıncı kez gelen konu; Yönetmelik'te taşıyıcısız hâlin süresini kargoya verilişten başlatan bir hüküm yoktur, m.12/5 bu hâl için yalnız masrafı düzenler; iddia K-491'de ve KP-22'de kalan risk olarak yazılı; öneri K-132, K-293 ve K-491 gereği elenmiş seçenektir. Doküman değişmedi)

**Hukuki kontrol** (2026-10-03):
- **Mesafeli Sözleşmeler Yönetmeliği'nde Değişiklik Yapılmasına Dair Yönetmelik** (RG 24/5/2025-32909, yürürlük 1/1/2026; resmî metin bu turda yeniden açıldı): m.12'de yalnız 4. fıkradaki *"13 üncü maddenin üçüncü fıkrası hükmü saklı kalmak üzere"* ibaresini kaldırır ve 5. fıkrayı değiştirir. Taşıyıcının belirtilmediği hâlde geri ödeme süresinin başlangıcına dair bir hüküm içermez. m.12/5: *"… Satıcının ön bilgilendirmede iade için herhangi bir taşıyıcıyı belirtmediği durumda ise tüketiciden iade masrafına ilişkin herhangi bir bedel talep edilemez. …"*
- **Mesafeli Sözleşmeler Yönetmeliği m.12/1** (konsolide metin, MevzuatNo 20237; önceki turlarda okundu): *"Satıcı, cayma hakkına konu malın, iade için ön bilgilendirmede belirtilen taşıyıcıya teslim edildiği tarihten itibaren on dört gün içinde … iade etmekle yükümlüdür. Ancak tüketicinin malı, iade için öngörülenin haricinde bir taşıyıcı ile iade etmesi durumunda söz konusu yükümlülük malın satıcıya ulaştığı tarihten itibaren başlar."*
- **Ticaret Bakanlığı tüketici rehberi — mesafeli sözleşmeler** (bu turda yeniden açıldı): *"Bu süre, tüketicinin malı kargoya teslim ettiği tarihten itibaren başlar. İade işlemi, ön bilgilendirmede belirtilenden farklı bir kargo ile yapılmışsa bu süre, ürünün satıcıya ulaştığı tarihte başlar."* Taşıyıcısız hâl için yalnız masraf: *"… ön bilgilendirmede iade için bir kargo şirketi belirtilmemesi nedeniyle tüketicinin malı herhangi bir kargo şirketiyle geri göndermesi halinde tüketici iadeye ilişkin masraflardan sorumlu tutulamaz."*
- **Sonraki değişiklik taraması:** 24/5/2025 tarihli değişiklikten sonra Yönetmelik'in m.12'sini değiştiren yeni bir Resmî Gazete metni bulunmadı.

**Kaynaklar (hukuki kontrol):**
- [Resmî Gazete 24/5/2025-32909 — Mesafeli Sözleşmeler Yönetmeliğinde Değişiklik Yapılmasına Dair Yönetmelik](https://www.resmigazete.gov.tr/eskiler/2025/05/20250524-2.htm)
- [Mesafeli Sözleşmeler Yönetmeliği — mevzuat.gov.tr konsolide metin (PDF)](https://mevzuat.gov.tr/File/GeneratePdf?mevzuatNo=20237&mevzuatTur=KurumVeKurulusYonetmeligi&mevzuatTertip=5)
- [Ticaret Bakanlığı — Mesafeli Sözleşmeler Hakkında Bilgilendirme](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)
