# Cross-Review — 10 MVP Scope (Tur 15)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/10_MVP_SCOPE.md` v0.25 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde; yalnız satır sonu boşlukları atıldı). İstem önceki turlarla aynıdır: daraltılmış talimat (K-615, K-644), yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §2 / KP-22
> Alıntı: "teslimden sonraki caymada malın firmaya ulaştığı tarihten itibaren"
> Sorun: Firma iade taşıyıcısı belirlemediği hâlde, cayma bedel iadesi süresini malın firmaya ulaştığı tarihten başlatıyor. Güncel kuralda taşıyıcı belirtilmemişse tüketicinin malı taşıyıcıya teslim ettiği tarih esas alınır. Bu kural, iade gecikirse tüketicinin bedel iadesini kanuni süresinden geç almasına yol açar.
> Öneri: Taşıyıcı belirtilmeyen iadelerde geri ödeme için on dört günlük süreyi müşterinin malı taşıyıcıya teslim ettiği tarihten başlatın; bu tarihi ispatlayacak takip/teslim verisinin operasyon akışını tanımlayın.
> ```

Model sonuca varmadan önce üç web araması yaptı (koşum kaydında görünür, çıktıya girmedi): Ticaret Bakanlığı sitesinde Yönetmelik m.12'nin 1/1/2026'dan beri uygulanan metni (iki kez) ve Resmî Gazete'de 4733 sayılı Kanun m.8/5-k. Tek bulgu yazdı; önceki turlarda gelip reddedilen konudur. Bulgu kaynak göstermiyor.

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ❌ RET | **Konu dokuzuncu kez geliyor ve doküman doğru** (5., 6., 8., 9., 10., 11., 13., 14. ve bu tur). **İddia edilen "güncel kural" resmî metinde yok:** Mesafeli Sözleşmeler Yönetmeliği'nin mevzuat.gov.tr konsolide metni bu turda yeniden okundu. m.12/1 iki hâli düzenler: *"iade için ön bilgilendirmede belirtilen taşıyıcıya teslim edildiği tarihten itibaren on dört gün içinde"* ve *"Ancak tüketicinin malı, iade için öngörülenin haricinde bir taşıyıcı ile iade etmesi durumunda söz konusu yükümlülük malın satıcıya ulaştığı tarihten itibaren başlar."* Taşıyıcı hiç belirtilmeyen hâlde sürenin taşıyıcıya teslimden başladığını söyleyen bir cümle yoktur; m.12/5 bu hâlde yalnız masrafı düzenler (*"Satıcının ön bilgilendirmede iade için herhangi bir taşıyıcıyı belirtmediği durumda ise tüketiciden iade masrafına ilişkin herhangi bir bedel talep edilemez."*). Model kaynak göstermedi; 14. turda gösterdiği Bakanlık rehberi de bu kuralı koymaz (14. turda yeniden açıldı). **İddia KP-22'de kalan risk olarak yazılı:** satırın gerekçesi *"taşıyıcı hiç belirtilmediğinde bu sonucun açık hükmü yoktur — tüketici lehine bir yorum başlangıcı malın kargoya verildiği güne çekebilir; firma geri ödemeyi iade gönderisinin kargoya verildiği günden on dört gün içinde yaparsa iki okumada da süreyi kaçırmaz"* der. Model bu cümleyi alıntılamadı; alıntısı aynı satırın kural hücresinden. **Öneri elenmiş seçenektir:** taşıyıcı belirlemek K-293 ve K-491'de elendi; kargoya veriliş tarihini ispatlayan bir takip akışı kargo entegrasyonu ister ve MVP dışıdır (K-132). **Dönüşün sebebi metindeki bir belirsizlik değildir:** kural, dayanağı ve kalan risk aynı satırda yazılı; model kuralı okuyup gerekçeyi okumadan aynı itirazı tekrar ediyor. 13. ve 14. turun gerekçesi geçerlidir. | Uygulanmadı. |

**Dağılım:** 0 KABUL · 0 KISMİ · 1 RET. Tek bulgu kayıtlı bir kararı (K-491) yeni bir resmî dayanak olmadan dokuzuncu kez yeniden açıyor. %100 ret bu turda şüphe gerekçesi değildir: bulgu tektir ve iddiası resmî metne karşı tek tek kontrol edildi.

## 3. Ek bulgular

- **Mekanik tarama:** sürüm başlığı, sürüm notu ve alt bilgi aynı sürümü gösteriyor (v0.26). Kapsam satırı sayısı değişmedi (77), kapsam dışı satırı sayısı değişmedi (63).
- **Konsolide metinde yeni bir iz — etkisi yok:** mevzuat.gov.tr metni m.15/1'in (ı), (j) ve (k) bentlerini *"Danıştay Onuncu Dairesinin 6/5/2026 tarihli ve E.:2022/5534; K.:2026/2753 sayılı kararı ile iptal bent"* diye gösteriyor (tescili zorunlu taşınırlar, canlı müzayede, kurulumu ya da montajı yapılan mallar). Ürünün cayma istisnası listesi yalnız (b), (c), (ç) ve (e) bentlerini kullanır (K-509; KP-22); iptal edilen bentlerin hiçbiri `10`'da, `02`'de ya da `01`'de anılmıyor. Düzeltme gerekmez; etki yansıtmaya devreden iş yok.
- **Tekrarlayan konular:** KP-22'nin geri ödeme başlangıcı dokuz turda (5., 6., 8., 9., 10., 11., 13., 14., 15.) geldi. Bu tur KP-39, KD-28 ve KP-63 gelmedi. Ciddi sorun sınıfında son yedi turda (9.–15.) kabul edilen bir kural değişikliği yok.
- **Ciddi bir sorun görülmedi.** Bu turda ikinci modelin listesinin dışında ciddi bir sorun saptanmadı.
- **Önceki turlardan devreden notlar** yerinde duruyor: "İşlem izi" sözlük satırı, §8.5.2, §3.34.8 ve §12.1.12'ye 4733 m.8/5-k dayanağı (isteğe bağlı); `02 §3.1.4`'teki "kayıt yükümlülüğü her firmada doğmaz" gerekçesi (8. tur, etki yansıtmaya devredildi).

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`10` v0.26 — yalnız sürüm notu). Bu turda yeni karar satırı açılmadı. K-293, K-491 ve K-509 kayıtlı kararlardır; bu tur hiçbirini değiştirmedi.

- [x] BULGU-1 (ret: dokuzuncu kez gelen konu; Yönetmelik m.12/1'in konsolide metni taşıyıcı belirtilmeyen hâlde sürenin başlangıcını düzenlemez, iddia KP-22'de ve K-491'de kalan risk olarak yazılı; taşıyıcı belirleme K-293 ve K-491'de elendi. Doküman değişmedi)

**Hukuki kontrol** (2026-10-03):
- **Mesafeli Sözleşmeler Yönetmeliği m.12** (bu turda mevzuat.gov.tr konsolide metni okundu): m.12/1 belirtilen taşıyıcıya teslimi ve öngörülenin haricindeki taşıyıcıyı düzenler; m.12/5 (RG 24/5/2025-32909 ile değişik, yürürlük 1/1/2026) taşıyıcı belirtilmeyen hâlde yalnız masrafı düzenler. RG 32909'un resmî metni de yeniden açıldı; m.12/5'in metni ve 1/1/2026 yürürlük tarihi konsolide metinle aynıdır.
- **Sonraki değişiklik taraması:** konsolide metin m.12 için 23/8/2022-31932 ve 24/5/2025-32909 dışında bir değişiklik göstermiyor; Resmî Gazete araması da yeni bir değişiklik yönetmeliği vermedi. Metindeki tek yeni iz m.15/1'in üç bendinin Danıştay kararıyla iptalidir (§3); m.12'ye dokunmaz.

**Kaynaklar (hukuki kontrol):**
- [Mesafeli Sözleşmeler Yönetmeliği — mevzuat.gov.tr konsolide metin (PDF)](https://mevzuat.gov.tr/File/GeneratePdf?mevzuatNo=20237&mevzuatTur=KurumVeKurulusYonetmeligi&mevzuatTertip=5)
- [Resmî Gazete 24/5/2025-32909 — Mesafeli Sözleşmeler Yönetmeliğinde Değişiklik Yapılmasına Dair Yönetmelik](https://www.resmigazete.gov.tr/eskiler/2025/05/20250524-2.htm)
- [Ticaret Bakanlığı — Mesafeli Sözleşmeler Hakkında Bilgilendirme](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme) (14. turda okundu)
