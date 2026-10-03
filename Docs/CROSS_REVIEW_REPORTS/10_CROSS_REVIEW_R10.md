# Cross-Review — 10 MVP Scope (Tur 10)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/10_MVP_SCOPE.md` v0.20 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde; yalnız satır sonu boşlukları atıldı). İstem önceki turlarla aynıdır: daraltılmış talimat (K-615, K-644), yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: KP-22
> Alıntı: "firma iade taşıyıcısı belirlemez, her iade öngörülenin dışında bir taşıyıcıyla yapılır; taşıyıcı belirlemek bilinçli olarak elendi"
> Sorun: Ön Bilgilendirme Formu’nda satıcının iade için öngördüğü taşıyıcı bilgisinin yer alması zorunludur. Taşıyıcı hiç belirlenmezse, ürünün firmaya ulaşmasını geri ödeme süresinin başlangıcı sayan “öngörülenin dışındaki taşıyıcı” istisnasına dayanılmaz; bu kural, ancak belirtilmiş taşıyıcı dışındaki bir taşıyıcı kullanıldığında uygulanır. Mevcut kurgu müşterinin geri ödemesini hukuka aykırı biçimde geciktirebilir. [Mesafeli Sözleşmeler Yönetmeliği değişikliği](https://resmigazete.gov.tr/eskiler/2025/05/20250524-2.htm)
> Öneri: Ön Bilgilendirme Formu’nda iade taşıyıcısını belirtin; müşteri bu taşıyıcıya teslim ettiğinde geri ödeme süresini teslim tarihinden başlatın. Farklı taşıyıcı kullanılırsa süre malın firmaya ulaştığı tarihten başlayabilir.
> ```

Model sonuca varmadan önce dört konuyu web aramasıyla kontrol etti (koşum kaydında görünür, çıktıya girmedi): Mesafeli Sözleşmeler Yönetmeliği m.12'nin 1/1/2026'dan beri uygulanan metni, ön bilgilendirmede belirtilen iade taşıyıcısı, Fiyat Etiketi Yönetmeliği'nin internet satışında üretim yeri bilgisi ve indirimli satış kuralı. Fiyat etiketi konularında bulgu yazmadı.

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ❌ RET | **Konu beşinci kez geliyor ve doküman doğru** (5., 6., 8., 9. ve bu tur). Sonuç ve öneri önceki turlarla aynıdır: taşıyıcı belirtilmeyince süre kargoya verilişten başlamalı ya da firma taşıyıcı belirlemeli. **Yeni olan dayanaktır ve olgusal olarak yanlıştır:** model, ön bilgilendirmede iade taşıyıcısının belirtilmesinin *zorunlu* olduğunu söylüyor. Yönetmelik m.5/1-g (RG 24/5/2025-32909 ile değişik, yürürlük 1/1/2026) ön bilgilendirmenin *"satıcının iade için öngördüğü taşıyıcıya ilişkin bilgiler"*i içermesini ister; satıcıya taşıyıcı öngörme yükümlülüğü getirmez. Aynı değişiklikle yazılan m.12/5 taşıyıcı belirtilmeyen hâli açıkça öngörür: *"Satıcının ön bilgilendirmede iade için herhangi bir taşıyıcıyı belirtmediği durumda ise tüketiciden iade masrafına ilişkin herhangi bir bedel talep edilemez."* Belirtmemek bir aykırılık olsaydı Yönetmelik bu hâle ayrı bir sonuç bağlamazdı. Bendin istediği bilgi de veriliyor: Ön Bilgilendirme Formu iade için taşıyıcı belirlenmediğini ve müşterinin malı istediği taşıyıcıyla karşı ödemeli gönderdiğini yazar (K-493; `02 §3.24.3`, §7.4.4). **Süre bakımından iddia kayıtlı kalan risktir:** m.12/1'in ikinci cümlesinin yalnız belirtilmiş bir taşıyıcı varken uygulanacağı okuması KP-22'nin gerekçesinde *"taşıyıcı hiç belirtilmediğinde bu sonucun açık hükmü yoktur — tüketici lehine bir yorum başlangıcı malın kargoya verildiği güne çekebilir"* diye yazılı. Gerekçe, firma geri ödemeyi kargoya verme gününden on dört gün içinde yaparsa iki okumada da süreyi kaçırmadığını da yazıyor; "hukuka aykırı biçimde geciktirebilir" iddiası bu yüzden karşılanmış (K-491). **Öneri elenmiş seçenektir:** "firma iade taşıyıcısı belirler" K-293 ve K-491'de elendi; satır bunu adıyla yazıyor. **Dönmesinin sebebi bu kez metinde bir boşluktu:** KP-22 taşıyıcının belirlenmediğini ve bunun bilinçli olduğunu yazıyordu, ama bunun ön bilgilendirme yükümlülüğüne neden aykırı olmadığını yazmıyordu; K-493 yalnız Kaynak sütunundaydı. Model bu boşluğu "zorunlu bilgi eksik" diye okudu. Gerekçeye bir cümle eklendi, kural değişmedi. | KP-22 Gerekçe: *"… taşıyıcı belirlemek bilinçli olarak elendi (K-293, K-491). Taşıyıcı belirlemek zorunlu değildir: m.12/5 satıcının ön bilgilendirmede taşıyıcı belirtmediği hâli açıkça öngörür ve Ön Bilgilendirme Formu m.5/1-g'nin istediği bilgiyi taşıyıcı belirlenmediğini yazarak verir (K-493). Kalan risk bilinçlidir: …"* K-493 Kaynak sütununda zaten vardı. Öneri uygulanmadı. |

**Dağılım:** 0 KABUL · 0 KISMİ · 1 RET. Tek bulgunun reddi körü körüne verilmiş bir onay değildir: bulgunun yeni dayanağı ("taşıyıcı belirtmek zorunlu") Yönetmelik'in aynı değişiklikle yazılmış m.12/5 hükmüyle çürüyor; süre iddiası ise K-491'de ve KP-22'de kalan risk olarak yazılı. Bulgu dokümanın iki yeri arasında bir çelişki göstermiyor. Dönmesine yol açan boşluk için gerekçeye bir cümle eklendi.

## 3. Ek bulgular

- **Mekanik tarama:** K-493 KP-22'nin Kaynak sütununda var. Sürüm başlığı, sürüm notu ve alt bilgi aynı sürümü gösteriyor (v0.21). Kapsam satırı sayısı değişmedi (77), kapsam dışı satırı sayısı değişmedi (63).
- **`02` ile hizalı:** `02 §3.24.3`, §7.4.2, §7.4.4 ve §12.1.9 formun taşıyıcı belirlenmediğini yazdığını ve m.12/5'in taşıyıcısız hâlini zaten anlatıyor; bu turun eklediği cümle bir gerekçedir, kural değil. Çelişki doğmadı.
- **Tekrarlayan konular:** KP-22'nin geri ödeme başlangıcı beş turda (5., 6., 8., 9., 10.) geldi; her turda dayanak değişti (m.12/1'in lafzı, Bakanlık rehberi, bu turda m.5/1-g). Satır artık kuralı, dayanağını, taşıyıcı belirlemenin zorunlu olmadığını, elenen seçeneği ve kalan riski yazıyor. Konu yeni bir dayanak olmadan yeniden gelirse doğrudan RET olarak kalır. KP-39 ve KP-47 bu turda gelmedi.
- **Ciddi bir sorun görülmedi.** Bu turda ikinci modelin listesinin dışında ciddi bir sorun saptanmadı.
- **Önceki turlardan devreden notlar** yerinde duruyor: "İşlem izi" sözlük satırı, §8.5.2, §3.34.8 ve §12.1.12'ye 4733 m.8/5-k dayanağı (isteğe bağlı); `02 §3.1.4`'teki "kayıt yükümlülüğü her firmada doğmaz" gerekçesi (8. tur, etki yansıtmaya devredildi).

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`10` v0.21). Bu turda yeni karar satırı açılmadı. K-293, K-491 ve K-493 kayıtlı kararlardır; bu tur hiçbirini değiştirmedi.

- [x] BULGU-1 (ret: beşinci kez gelen konu; m.12/5 taşıyıcı belirtilmeyen hâli öngördüğü için taşıyıcı belirtmek zorunlu değildir, form m.5/1-g'nin bilgisini K-493 gereği verir, kalan risk K-491'de kayıtlı ve KP-22'de yazılı, taşıyıcı belirleme K-293 ve K-491'de elendi. KP-22'nin gerekçesine taşıyıcı belirlemenin zorunlu olmadığı yazıldı)

**Hukuki kontrol** (2026-10-03):
- **Mesafeli Sözleşmeler Yönetmeliği'nde Değişiklik Yapılmasına Dair Yönetmelik** (RG 24/5/2025-32909, yürürlük 1/1/2026; modelin gösterdiği resmî metin bu turda yeniden açıldı): m.5/1-g *"Cayma hakkının olduğu durumlarda, bu hakkın kullanılma şartları, süresi, usulü ve satıcının iade için öngördüğü taşıyıcıya ilişkin bilgiler,"*. m.12/5 *"… satıcının iade için belirttiği taşıyıcı aracılığıyla malın geri gönderilmesi halinde, tüketici iadeye ilişkin masraflardan sorumlu tutulamaz. Satıcının ön bilgilendirmede iade için herhangi bir taşıyıcıyı belirtmediği durumda ise tüketiciden iade masrafına ilişkin herhangi bir bedel talep edilemez. …"*
- **Mesafeli Sözleşmeler Yönetmeliği m.12/1** (konsolide metin, MevzuatNo 20237; önceki turlarda okundu, bu turda yeniden tarandı): *"Satıcı, cayma hakkına konu malın, iade için ön bilgilendirmede belirtilen taşıyıcıya teslim edildiği tarihten itibaren on dört gün içinde … iade etmekle yükümlüdür. Ancak tüketicinin malı, iade için öngörülenin haricinde bir taşıyıcı ile iade etmesi durumunda söz konusu yükümlülük malın satıcıya ulaştığı tarihten itibaren başlar."*
- **Sonraki değişiklik taraması:** 24/5/2025 tarihli değişiklikten sonra Yönetmelik'i değiştiren yeni bir Resmî Gazete metni bulunmadı.

**Kaynaklar (hukuki kontrol):**
- [Resmî Gazete 24/5/2025-32909 — Mesafeli Sözleşmeler Yönetmeliğinde Değişiklik Yapılmasına Dair Yönetmelik](https://www.resmigazete.gov.tr/eskiler/2025/05/20250524-2.htm)
- [Mesafeli Sözleşmeler Yönetmeliği — mevzuat.gov.tr konsolide metin (PDF)](https://www.mevzuat.gov.tr/MevzuatMetin/yonetmelik/7.5.20237.pdf)
