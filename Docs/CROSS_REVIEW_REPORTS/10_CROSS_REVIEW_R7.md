# Cross-Review — 10 MVP Scope (Tur 7)

**Tarih:** 2026-10-03 · **Hedef:** `Docs/10_MVP_SCOPE.md` v0.17 · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), yeni ve izole bir çalışma alanında; karar kaydı, önceki turlar ve raporlar verilmedi (K-431)

## 1. Ham bulgular

İkinci modelin çıktısı, düzenlenmeden (alıntı bloğu içinde). İstem önceki turlarla aynıdır: daraltılmış talimat (K-615, K-644), yedi kriter, bulgu biçimi ve "bilinçli karara yalnız katılmadığın için itiraz etme" cümlesi.

> ```text
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Orta
> Yer: §4.2 / SK-3; §2 / KP-51
> Alıntı: "havale ile ödenen fiziksel + hizmet siparişi dört adımdır."
> Sorun: KP-51, karma fiziksel + hizmet havale siparişinde dört manuel adım tanımlar. SK-3 ise havaleli fiziksel siparişin üç adımını “en ağır” durum olarak tanımlar. Manuel adım bütçesinin kabul kapısındaki anlamı iki bölümde farklıdır.
> Öneri: SK-3’ü tek tipli siparişlerle sınırlandırın veya karma siparişlerin dört adıma çıkabildiğini açıkça yazın.
>
> BULGU-2
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §2 / KP-39
> Alıntı: "sistem ürünün ölçüyle satıldığını bilemediği için alan yayın koşulu değildir; doldurmak firmanın yükümlülüğüdür, panel bunu hatırlatır ve alan boşsa birim fiyat gösterilmez"
> Sorun: Ölçü birimi ve net miktar boşken ölçüyle satılan ürün yayımlanabilir; dolayısıyla zorunlu birim fiyat gösterilmeden satış yapılabilir. Birim fiyatın bu mallarda görünür biçimde gösterilmesi zorunludur. [Ticaret Bakanlığı açıklaması](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/fiyat-etiketleri-hakkinda-bilgilendirme)
> Öneri: Ölçüyle satış beyanını ürün kaydında zorunlu ve doğrulanabilir hâle getirin; beyan edilen ürünlerde ölçü birimi ve net miktar olmadan yayını engelleyin.> ```

Model sonuca varmadan önce üç konuyu web aramasıyla kontrol etti (koşum kaydında görünür): Mesafeli Sözleşmeler Yönetmeliği'nde geri ödeme süresi ve iade taşıyıcısı, cayma istisnalarından kitap ve bilgisayar sarf malzemesi, Fiyat Etiketi Yönetmeliği'nde birim fiyat. İlk ikisi için bulgu yazmadı.

## 2. Bağımsız değerlendirme

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| BULGU-1 | ✅ KABUL | **Çelişki lafızda tam değil, ama okuma yerinde.** SK-3 *"en ağır hat"* diyor; "hat" K-454'ün terimidir ve bütçe tek tipli siparişin hattına uygulanır. KP-51, `01 §6` Ü-3 ve `02 §10.5.1` ve §10.5.4 aynı kuralı yazıyor: tek tipli siparişte tavan üç, karışık siparişte hatların adımları toplanır, havale ile ödenen fiziksel + hizmet siparişi dört adımdır. **Ama SK-3 bunun yalnız ilk yarısını taşıyordu.** Satır §4.2'de, kabul kapısının kriterine (Ü-3) bağlı bir kısıt olarak tek başına okunuyor. Karışık siparişi anmadığı için "en ağır hat üç adımdır" cümlesi siparişin tavanı gibi okunuyordu. KP-51 ile yan yana konunca iki satır kapının ölçtüğü şeyi farklı söylüyor gibiydi. Önerinin ikinci seçeneği ("karma siparişlerin dört adıma çıkabildiğini açıkça yazın") K-454'ün dediğidir ve aynen uygulandı. Birinci seçenek (SK-3'ü tek tipli siparişle sınırlamak) aynı sonuca daha belirsiz bir yoldan varırdı. | SK-3 Özellik: *"… en ağır hat — havale ile ödenen fiziksel sipariş — üç adımdır. Bütçe hat başınadır: karışık siparişte hatların adımları toplanır, havale ile ödenen fiziksel + hizmet siparişi dört adımdır (KP-51)."* K-454 Gerekçe sütununda zaten vardı. Kural, tetikleyici ve sayılar değişmedi. |
| BULGU-2 | ❌ RET | **Aynı konu üçüncü kez geliyor ve doküman doğru.** 2. turda BULGU-1, 5. turda BULGU-2 olarak geldi; öneri her seferinde aynı: ölçüyle satışı beyan ettirip alanı yayın koşulu yapmak. **Öneri bilinçli olarak elendi ve doküman bunu yazıyor:** K-573 hem "ölçüyle satılan üründe alan yayın koşulu olsun" hem de "her fiziksel üründe satış birimi seçimi zorunlu olsun" seçeneklerini eledi. KP-39 bu elemeyi ve nedenini (5. turda eklendi) taşıyor. Sistem bir ürünün kiloyla, litreyle ya da metreyle satıldığını ürün kaydından bilemez; beyanı yine firma verir ve beyan alanı doldurmakla aynı şeydir. **Hukuki iddia ürüne değil satıcıya yöneliktir.** Fiyat Etiketi Yönetmeliği birim fiyatı göstermeyi satıcıya yükler (m.4 "satıcı" tanımı, m.5; Ticaret Bakanlığı rehberi de *"Satıcı … birim fiyatı göstermekle yükümlüdür"* diyor). Satıcı firmadır (K-14). Ürün yükümlülüğü kaldırmıyor; alan panelde var, panel doldurmayı hatırlatıyor ve alan doluysa birim fiyatı kendisi hesaplayıp gösteriyor (KP-5, KP-39). **Dönmesinin sebebi:** bulgunun dayandığı alıntı satırın kendisiydi, ama satır seçimi "kabul edilmiş risk" olarak adlandırmıyor ve K-573'ün *"boş alanla yayına alınan ürünün sonucu firmadadır"* yarısını taşımıyordu. Talimattaki "bilinçli karar, elenen seçenek ya da kabul edilmiş risk" ayrımı bu yüzden satıra açıkça uymuyordu. Bu belirsizlik giderildi; kural değişmedi. | KP-39 Gerekçe: *"… adetle satılan her ürüne bir adım ekler. Kalan risk bilinçlidir: firma ölçüyle sattığı ürünü alanı doldurmadan yayına alırsa birim fiyat gösterilmez ve bunun yasal sonucu satıcı olan firmadadır (K-573)."* K-573 Kaynak sütununda zaten vardı. Öneri uygulanmadı. |

**Dağılım:** 1 KABUL · 0 KISMİ · 1 RET. BULGU-1 yeni bir konudur ve satır kararın yarısını taşımıyordu. BULGU-2 üçüncü kez gelen, kayıtlı bir kararla elenmiş bir öneridir; satıra yalnız kararın kalan riski yazıldı.

## 3. Ek bulgular

- **Mekanik tarama:** düzeltmelerde anılan karar numaraları ilgili satırlarda var (K-454 SK-3'ün Gerekçe sütununda, K-573 KP-39'un Kaynak sütununda). SK-3'ün yeni cümlesi KP-51 ve `02 §10.5.1` ile aynı sayıyı söylüyor (dört). Sürüm başlığı ve alt bilgi aynı sürümü gösteriyor (v0.18). Kapsam satırı sayısı değişmedi (77), kapsam dışı satırı sayısı değişmedi (63).
- **`02` ve `01` ile hizalı:** `02 §10.5.1` ve §10.5.4 ile `01 §6` Ü-3 bütçenin hat başına olduğunu ve karışık siparişin toplamını zaten yazıyor; `02 §3.8.5` ve §12.1 K-573'ün kalan riskini yazıyor. Çelişki doğmadı, etki yansıtmaya devredilen yeni iş yok.
- **Tekrarlayan konular:** KP-39'un birim fiyat alanı üç turda geldi (2., 5., 7.). Satır artık seçimi kalan risk olarak adlandırıyor ve sonucun kimde olduğunu söylüyor. Konu yeni bir dayanak olmadan yeniden gelirse doğrudan RET olarak kalır.
- **Ciddi bir sorun görülmedi.** Bu turda ikinci modelin listesinin dışında ciddi bir sorun saptanmadı.
- **Etki yansıtmaya devreden notlar:** önceki turlardan devreden `02` notları yerinde duruyor: "İşlem izi" sözlük satırı, §8.5.2, §3.34.8 ve §12.1.12'ye 4733 m.8/5-k dayanağı (isteğe bağlı).

## 4. Kullanıcı onay checklist'i

Proje sahibinin talimatıyla ("kolay soruları bana sorma sen karar ver, çok çok kritik konuları sadece sor") yönetici onayladı ve uygulandı (`10` v0.18). Bu turda yeni karar satırı açılmadı. K-454 ve K-573 kayıtlı kararlardır; bu tur ikisini de değiştirmedi, yalnız satırlara eksik yarılarını taşıdı.

- [x] BULGU-1 (kabul: SK-3 bütçenin hat başına olduğunu ve havale ile ödenen fiziksel + hizmet siparişinin dört adım olduğunu yazar — K-454, KP-51)
- [x] BULGU-2 (ret: üçüncü kez gelen konu; yayın koşulu ve ayrı "ölçüyle satılır" beyanı K-573'te elendi, yükümlülük satıcıdadır. KP-39'un gerekçesine kararın kalan riski yazıldı)

**Hukuki kontrol** (2026-10-03):
- **Fiyat Etiketi Yönetmeliği** (RG 28/6/2014-29044): 2. turda okunan m.4 ve m.5 hükümleri geçerli. Ticaret Bakanlığı'nın tüketici rehberi: *"Bu malların türüne göre uygun olan adet, uzunluk, ağırlık, alan veya hacim gibi ölçü birimlerinden herhangi birinin net miktarı cinsinden ifade edilen fiyatı olan birim fiyatının kolaylıkla görülebilir ve okunabilir şekilde fiyat etiketine yazılması zorunludur"*; yükümlü satıcıdır; birim fiyat satış fiyatıyla aynıysa gösterilmesi gerekmez. 11/10/2025 tarihli değişiklik (RG 33044) birim fiyat hükmüne dokunmadı (2. tur). 30/1/2026 tarihli değişiklik (RG 33153) yalnız m.8/6'yı değiştirdi (yeme-içme yerlerinde servis ve kuver ücreti yasağı); birim fiyata ve elektronik ticarette fiyat gösterimine dokunmuyor.

**Kaynaklar (hukuki kontrol):**
- [Ticaret Bakanlığı — Fiyat etiketleri hakkında bilgilendirme](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/fiyat-etiketleri-hakkinda-bilgilendirme)
- [Fiyat Etiketi Yönetmeliğinde Değişiklik — Resmî Gazete, 11.10.2025](https://resmigazete.gov.tr/eskiler/2025/10/20251011-6.htm)
- [Restoranlar artık servis ücreti ve kuver talep edemeyecek — Hukuki Haber (RG 30.01.2026, 33153)](https://www.hukukihaber.net/restoranlar-artik-servis-ucreti-ve-kuver-talep-edemeyecek)
