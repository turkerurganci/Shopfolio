# Cross-Review — 11 Implementation Planı (Tur 2)

**Tarih:** 2026-10-31 · **Hedef:** `Docs/11_IMPLEMENTATION_PLAN.md` v0.38 (girdi commit'i `86abe6d`, 748 201 bayt, 7 760 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren, mekanizma okuma paragrafıyla (K-1399; `11_CROSS_REVIEW.md` §1) · **Koşum:** iki koşum — tek parça çağrı ve beş parçalı kontrol koşumu · **Sonuç:** **TEMİZ DEĞİL** — on ayrı bulgu (on ham) ve bir ek bulgu (EK-1, 1. turdan taşınan); üçü (bir doğru, iki kısmi) ve EK-1 uygulandı (16 şartname çifti), yedisi reddedildi.

## 1. Koşum

Betikler, istem, parça notu, şablon ve **parça sınırları 1. turdakinin aynısıdır** (`11_CROSS_REVIEW.md` §1; K-1478 — sınır başlıkla tanımlıdır, değişmez). İstem tek karakter değiştirilmeden kullanıldı. Girdi `build_inputs.py` ile kuruldu (`uncovered lines: none`); `codex login status` → "Logged in using ChatGPT". Altı çağrı tek Bash komutunda paralel koştu (`run_codex.sh in out c1 p1 p2 p3 p4 p5`; çalışma klasörü her çağrının kendi boş klasörü); kullanım sınırı dolmadı, çıktısız dönen çağrı olmadı, yedek modele geçilmedi. Çalışma dizini: `C:\Users\teknodeveloper\.claude\helper-state\shopfolio\kd-cross\r2\` (girdiler `in/`, ham çıktılar `out/`, doğrulayıcı paketleri `dogrulayici_<k>.md`, hükümler `hukum_D<k>.md`, talimat `talimat_dogrulayici.md`; paket betiği `betik/paket2.py`).

| Çağrı | Bölümler | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|
| Tek parça | tüm doküman | 757 746 bayt | 415 765 | 323 sn | 2 bulgu |
| Parça 1 | §1–§4 · §5 Task yapısı · F0 · F1 (satır 65–866) | 95 573 bayt | 56 108 | 318 sn | 3 bulgu |
| Parça 2 | F2 · F3 T301–T345 (867–2841) | 196 015 bayt | 117 912 | 275 sn | 3 bulgu |
| Parça 3 | F3 T346–T390 · F4 (2842–4219) | 150 250 bayt | 110 266 | 278 sn | 1 bulgu |
| Parça 4 | F5 · F6 · §6 (4220–5633) | 149 039 bayt | 60 459 | 189 sn | 1 bulgu |
| Parça 5 | §7 · §8 · §9 (5634–dosya sonu) | 253 915 bayt | 131 800 | 48 sn | TEMİZ |

Web araması satırları (çağrı günlükleri): tek parça 3, parça 1 1, parça 2 5, parça 3 4, parça 4 0, parça 5 1. **İki koşumun ölçümü:** tek parça 2 bulgu (F1–F2), parçalı koşum 8 bulgu (F3–F10); iki koşumun bulguları örtüşmedi — tek parça çağrının getirdiği F1 (F1 kapısı ↔ `Down` yasağı) ve F2 (T370 cayma süresi) parçalı koşumda yok; parçalı koşumun altı bulgusu tek parça çağrıda yok. İki koşum 3. turda da sürer.

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**(a) Tek parça çağrı:**
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §3 F1; §6.2 F1; T04 KK5; T103 KK7
> Alıntı: “**Çıkış ürünü:** göçler ileri, idempotent ve geri alınabilir” / “Göç tek konudur; `Down` ve işlem dışı adım yoktur”
> Sorun: F1 kapısı ve CI geri alma sınamasını zorunlu tutarken F1 göç task’ları `Down` yöntemini yasaklıyor. Böylece ilk gerçek göçlerden sonra geri alma sınaması gerçekleştirilemez ve faz kendi kapısını geçemez. Bu, planın kendi güvence mekanizmasında çelişkidir (4).
> Öneri: Kaynak kuralı `Down`’ı yasaklıyorsa F1 ve T04’ü ileri-yönlü göç modeline hizalayın: “geri alınabilir” ve migration rollback sınamasını kaldırın; bunun yerine sürümleme, genişlet–daralt uyumluluğu ve yedekten geri dönüş provasını açık kabul kriterleri yapın.
>
> BULGU-2
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: T370, kabul kriteri 3
> Alıntı: “Süre iptalde iptalden, teslimden önceki caymada ve hizmette beyandan, teslimden sonraki caymada malın ulaştığı tarihle beyandan geç olanından başlar”
> Sorun: Teslim edilmiş malda caymaya ilişkin iade süresini koşulsuz biçimde malın satıcıya ulaşmasına bağlıyor. Oysa tüketicinin ön bilgilendirmede belirtilen taşıyıcıya malı teslim ettiği durumda süre o teslim tarihinde başlar; satıcıya ulaşma tarihi yalnız farklı taşıyıcı kullanımında önem kazanır. Bu kural, tüketicinin bedel iadesini hukuken izin verilenden geç almasına yol açar (1). [Ticaret Bakanlığı güncel bilgilendirmesi](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)
> Öneri: Kriteri üç ayrı duruma bölün: belirtilen taşıyıcıya teslimde taşıyıcıya teslim tarihi; başka taşıyıcıda satıcıya ulaşma tarihi; teslim edilmemiş mal ve hizmette cayma bildirimi tarihi. Her durum için ayrı sınır-tarihi testi ekleyin.
>
> SONUÇ: 2 BULGU

**(b) Parça 1 (§1–§4, F0, F1):**
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Orta
> Yer: Başlık notu “Okunacak `09` bölümleri”; §5 F1, T101–T120
> Alıntı: “C# yazan her task §3.1 ve §3.2.”
> Sorun: T101–T120 EF Core göçü/model kodu yazdığı hâlde `Dokümanlar` alanlarında `09 §3.1` ve `§3.2` yoktur. Bu, planın her C# task’ına zorunlu kıldığı okuma girdisiyle çelişir; task’ın başlamadan tüm kodlama girdilerine sahip olacağı güvencesi sağlanmamaktadır (4).
> Öneri: T101–T120’nin ve C# üreten diğer tüm task’ların `Dokümanlar` alanına `09 §3.1, §3.2` eklenmeli; denetim betiği bunu zorunlu kılmalıdır.
>
> BULGU-2
> Kriter: Teknik doğruluk
> Seviye: Orta
> Yer: §5, T01, T05, T101, T102
> Alıntı: “**Dokümanlar:** çekirdek; `09` §8, §10 (§10.7), §19; …” / “Uç `Endpoints/` altındadır … (`09 §4.1`, §8).”
> Sorun: Kabul kriterlerinde zorunlu tutulan bazı `09` bölümleri task’ın okunacak dokümanları arasında yoktur: T01’de `§4.1`, T05’te `§15`, T101’de `§4.1`, T102’de `§9.2`. Okuma tablosu task ajanını yalnız `Dokümanlar` alanındaki ek bölümlere yönlendirdiği için agent, kabul edildiği kuralı okumadan implementasyon yapabilir. Bu, task girdilerinin tamam olacağı mekanizmayı bozar (4).
> Öneri: İlgili bölümleri her task’ın `Dokümanlar` alanına ekleyin: T01/T101 için `09 §4.1`, T05 için `09 §15`, T102 için `09 §9 (§9.2)`.
>
> BULGU-3
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §5, T08 — Sır düzeni
> Alıntı: “`05 §6.x`”
> Sorun: `§6.x` ve kriterlerdeki `§6.x.1`…`§6.x.19` çözümlenebilir kaynak kimlikleri değildir; dokümanın kendi atıf ve okuma kuralları kaynak biriminin başlığıyla bulunmasını gerektirir. Sır sahipliği, konteynere bağlama ve en az yetki tablosunu uygulayan task, hangi kesin kaynak kuralları okuyacağını belirleyememektedir. Bu, sırların yanlış bileşene verilmesi veya korumaların atlanması riskini doğurur; task girdilerinin hazır olması güvencesi sağlanmaz (4).
> Öneri: `x` yer tutucularını `05`teki gerçek bölüm numaralarıyla değiştirin; T08’in `Dokümanlar` alanında ilgili tablo ve tüm gerekli alt bölümleri açıkça listeleyin.
>
> SONUÇ: 3 BULGU

**(b) Parça 2 (F2; F3 T301–T345):**
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: T330, kabul kriteri 7
> Alıntı: “ETBİS kaydı yapılmaz; kayıt bilgisi isteğe bağlı alandır, satış kapısını tutmaz”
> Sorun: Kendi sitesinde tüketiciye sipariş alan firma, faaliyete başlamadan ETBİS’e kayıt olmak zorundadır. Plan kayıt yapılmamasını ve satışın bundan bağımsız açılmasını öngörerek mevzuata aykırı satışa izin verir. [Ticaret Bakanlığı açıklaması](https://ticaret.gov.tr/ic-ticaret/bilgi-sistemleri/elektronik-ticaret-bilgi-sistemi-etbis-ve-e-ticaret-bilgi-platformu)
> Öneri: ETBİS kaydı ve alan adı bilgisinin doğrulanmasını yayına/satış kapısına ön koşul yapan bir operasyon task’ı ve faz geçiş kapısı ekleyin.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: T224 kabul kriteri 6; T331 kabul kriteri 3
> Alıntı: “Çerez politikası ilk kez yayına alınana kadar vitrin çerez yazmaz; ara katman çerez başlıklarını yanıttan çıkarır” / “mevcut hesapların girişi etkilenmez”
> Sorun: Vitrin yanıtlarından tüm `Set-Cookie` başlıkları çıkarılırsa müşteri oturumu ve sahteciliğe karşı koruma çerezleri kurulamaz; mevcut kullanıcı giriş yapamaz. Bu, “giriş etkilenmez” kuralıyla çelişir ve kullanıcıyı çıkışsız bırakır.
> Öneri: Çerez politikasının yayımlanmasını müşteri yüzeyinin yayına açılmasının ön koşulu yapın; alternatif olarak müşteri kimlik doğrulama ve sahteciliğe karşı koruma çerezleri için açık, test edilen bir istisna tanımlayın.
>
> BULGU-3
> Kriter: Eksiklik
> Seviye: Orta
> Yer: T209, kabul kriterleri 1 ve 8; KK1 ve KK8
> Alıntı: “iletişim talebi, adres ekleme, indirme bağlantısı, panelin yeni kayıt formları, dışa aktarma) ikinci kez işlenir” / “Kişisel veri, oturum, sepet ya da erişim anahtarı taşıyan yanıt `Cache-Control: no-store` taşır” / “KK1: iki kez gönderilen aynı formun etkisi tektir; sepete eklemede ikinci gönderim ikinci eklemedir.” / “KK8: oturumlu yanıtta `no-store` var.”
> Sorun: Kabul kriterleri altı tekrar-gönderim türünü ve dört hassas yanıt sınıfını kapsıyor; doğrulama yalnız sepeti ve oturumlu yanıtı sınar. Başka task’a açık bir test devri de yoktur. Bu, planın kabul kriterlerinin sınanabilir olacağı güvencesini sağlamaz.
> Öneri: Her sayılan tekrar-gönderim türü için tablo güdümlü testler; kişisel veri, sepet ve erişim anahtarlı yanıtlar için de ayrı `no-store` testleri ekleyin.
>
> SONUÇ: 3 BULGU

**(b) Parça 3 (F3 T346–T390; F4):**
> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §5 / T368 kabul kriteri 6
> Alıntı: "Panel fesih satırında ve taahhüt süresi sonrası iptalde kanuni faiz uyarısı gösterir; sistem faiz hesaplamaz."
> Sorun: Mevzuata aykırıdır. Mesafeli Sözleşmeler Yönetmeliği m.16/3, fesihte tahsil edilen tüm ödemelerin teslimat masraflarıyla birlikte 14 gün içinde kanuni faiziyle iadesini zorunlu kılar. Yalnız uyarı gösterip faizi hesaplamamak, otomatik geri ödeme hattının eksik iade yapmasına ve tüketicinin parasal hak kaybına yol açar. [Resmî metin](https://tuketici.ticaret.gov.tr/data/5e819a8e13b876a1b04c7a42/Mesafeli%20S%C3%B6zle%C5%9Fmeler%20Y%C3%B6netmeli%C4%9Fi.pdf)
> Öneri: T368, T369 ve T370’ye fesihte uygulanacak kanuni faizin mevzuattaki başlangıç ve oranıyla hesaplanıp geri ödeme tutarına eklenmesini ekleyin; tutar, 14 günlük süre ve ödeme yolunda faizli iade için Unit ve Integration kabul testleri tanımlayın.
>
> SONUÇ: 1 BULGU

**(b) Parça 4 (F5, F6, §6):**
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §5, F5 / T508 KK2 ve KK7
> Alıntı: “Kalem fiyatı ve satın alınabilirliği güncel üründen okunur” / “ilk eklenme fiyatı değişmez”
> Sorun: Aynı sepet kaleminin fiyatının güncel ürün fiyatından mı okunacağı, yoksa sepete ilk ekleme anındaki fiyat olarak mı sabit kalacağı çelişkili. KK2’nin doğrulaması fiyat değişince satırın güncellenmesini isterken, KK7 ilk eklenme fiyatını sabit tutar. Bu, müşteriden yanlış tutar alınmasına veya firmanın yanlış fiyatla satış yapmasına yol açabilecek (2) türünde para kaybı riski ve (4) iç çelişkidir.
> Öneri: Sepet fiyatının açıkça tek bir kaynağını belirleyin; örneğin “sepet görüntüsü ve sipariş özeti güncel fiyatı kullanır” ya da “kalem ilk ekleme fiyatını taşır, ödeme öncesinde güncel fiyatla yeniden fiyatlanır.” Buna göre KK2, KK7 ve test kontrol listelerini aynı davranışı sınayacak biçimde hizalayın.
>
> SONUÇ: 1 BULGU

**(b) Parça 5 (§7–§9):**
> SONUÇ: TEMİZ

## 3. Bağımsız değerlendirme

Ham bulgular numaralandı: tek parça F1–F2, parça 1 F3–F5, parça 2 F6–F8, parça 3 F9, parça 4 F10. On ham bulgu on ayrı bulguydu (yinelenen yok; F3 ve F4 aynı kalıptır — task `Dokümanlar` alanı eksikliği — ama ayrı task kümeleridir). Dokuz `[dogrulama]` Opus / high doğrulayıcıya verildi: Yüksek bulguların hepsi tek tek (D1: F1; D2: F2; D4: F5; D5: F6; D6: F7; D8: F9; D9: F10); Orta F3 ve F4 aynı bölümden (F0/F1) ikili (D3); Orta F8, 1. turun ek notu EK-1 ile F2 fazından ikili (D7). Doğrulayıcı paketleri F2'nin ve F8'in 1. tur hükmünü bağlam olarak taşıdı (F2 için 1. tur F1 "cayma uzaması", F8 için 1. tur F5/F6). Hükümler `11_audit_hukum_denetle.py` ile sınandı (sorun 0). Tablo: bir doğru, iki kısmi, yedi çürük (+ EK-1 doğru). Seviyeler yeniden okundu: F1, F5, F6, F7, F9, F10 Yüksek → Düşük; F3, F4 Orta → Düşük; F2 Yüksek kaldı (çürük); F8 Orta kaldı.

| # | Bulgu (kaynak) | Karar | Gerekçe | Uygulama |
|---|---|---|---|---|
| F1 | F1 kapısı ve CI geri alma sınaması ↔ T04/T103 "`Down` yok" (tek parça B-1; §3 F1, T04 KK5, T103 KK7) | ❌ RET | Çelişki yok: projede "geri alma" şemayı `Down` ile geri almak değil, **önceki imaja dönmektir**; şema geri alınmaz ve bu yol CI göç adımında sınanır (son yayımlanan sürümün imajı yeni şemaya karşı çalıştırılır) — `05 §8.7.3`, MK-74 ("geri alma önceki imajdır ve CI'da sınanır"), `09 §10.6` madde 1 ("`Down` yöntemi silinir — şema geri alınmaz"), `09 §21.4` madde 3. İlk yayın F0'da T09'dadır; F1 göçleri geldiğinde sınamanın koşacağı sürüm vardır. Önerinin "geri alınabilir'i kaldırın" kolu MK-74 ile çelişirdi. Seviye Düşük. | Yok |
| F2 | T370 k3 iade süresi teslimden sonra "malın ulaştığı tarih"e bağlanmış; belirtilen taşıyıcıya teslimde teslim tarihi olmalı (tek parça B-2; T370 k3) | ❌ RET | Hukuki tespit doğru (Mesafeli Sözleşmeler Yönetmeliği m.12/1, RG 23/8/2022 31932 metninden okundu: belirtilen taşıyıcıya teslimde teslim tarihi, başka taşıyıcıda satıcıya ulaşma tarihi) — ama ürün sonucu değiştirmez: firma iade taşıyıcısı belirlemez ve Ön Bilgilendirme Formu taşıyıcı belirlenmediğini yazar (`02 §7.4.4`, §7.4.2; K-293, K-493), bu yüzden teslimden sonraki her iade "öngörülenin haricinde bir taşıyıcı" ile yapılır ve süre malın ulaştığı tarihten başlar (`02 §7.4.1`, Z-16; K-491). T370 k3 bu kuralı aynen tüketir, taşıyıcı öncülü T373 k2'de kodlanır. Aynı iddia `02` 5. tur BULGU-1'de resmî metne karşı reddedilmişti (`02_CROSS_REVIEW_R5.md`). İkinci modelin kaynağı (Ticaret Bakanlığı sayfası) türetilmiştir ve ürünün taşıyıcı belirlememe kararını görmez. 1. tur F1 hükmüyle ilgisi yok; yeni bilgi yok. Seviye Yüksek (değişmedi, çürük). | Yok |
| F3 | Okuma kuralı "C# yazan her task §3.1 ve §3.2"; T101–T120 `Dokümanlar` alanında yok (parça 1 B-1; başlık notu, §5 F1) | ⚠️ KISMİ | Alıntı doğru: 215 `Dokümanlar` satırından yalnız 7'si `09 §3.1/§3.2`yi, test yazan 209 task'ın 193'ü §19'u yazar; 11'in konvansiyon metni (satır 12, 41, 165) ile uygulama çelişir (4). Ama okuma güvencesi bozulmaz: `09 §24` madde 1 okunacak kümeyi "okuma tablosu + task satırı, ikisinin birleşimi" yapar (K-1357 (2)); tablonun her C#/teste düşen satırları task satırında yinelenmek zorunda değildir. Öneri (her task'a ekle; ~200 satır, denetim betiği) gereksiz ve blok/150 KB sınırını zorlar; düzeltme 11'in üç konvansiyon cümlesini üst dokümanın birleşim kuralına hizalamaktır. Seviye Orta → Düşük. **KARAR (toplu onayla kaydedildi):** (a) ~200 satıra ekle · (b) konvansiyon cümlelerini `09 §24` madde 1'e hizala — öneri (b). | 3 çift: satır 12, 41, 165 (konvansiyon cümleleri birleşim kuralına hizalandı) |
| F4 | T01/T05/T101/T102 kabul kriterleri `09 §4.1`, `§15`, `§9.2`ye atıf yapar; `Dokümanlar` alanında yok (parça 1 B-2) | ✅ KABUL | Dört yer de doğru (T01 KK6 `09 §4.1`; T05 KK8 `09 §15` madde 3; T101 KK1 `09 §4.1` madde 2; T102 KK4 `09 §9.2` madde 2): bunlar tablonun alana özgü satırlarıdır (klasör → §4, güvenlik başlığı → §15, işlem izi → §9; `09` okuma tablosu), her C# task'a düşen satır değildir; F3'ün birleşim cümlesi bunları kapsamaz ve blok konvansiyon 4 (satır 165) ile çelişir (4). T05 KK8'in atfı da eksik: HSTS cümlesi `09 §15.2` madde 3'tedir. Aynı eksik T201, T205, T218, T381'de de var (yan yüz). Seviye Orta → Düşük (kriter blokta atıfla görünür ve `09 §24` madde 1 alan satırını birleştirir). | 9 çift: T01, T05 (+ KK8 atfı), T101, T102, T201, T205, T218, T381 `Dokümanlar` alanları |
| F5 | T08 "Sır düzeni" `05 §6.x` ve `§6.x.1`…`§6.x.19` çözümlenemez kaynak kimlikleridir (parça 1 B-3) | ❌ RET | `§6.x` yer tutucu değildir: `05` içinde bu bölümün gerçek başlığı `### 6.x Secret yönetimi`dir (`05` satır 821) ve T08'in andığı alt kimlikler (6.x.1, 6.x.2 tablosu ve satırları, 6.x.8, 6.x.9, 6.x.12, 6.x.16, 6.x.18, 6.x.19) `05`te tanımlıdır; 11 başlığın anılış biçimini de yazar (satır 5678). Çözülemeyen atıf ve çelişki yok. Seviye Yüksek → Düşük. | Yok |
| F6 | T330 k7 "ETBİS kaydı yapılmaz … satış kapısını tutmaz" mevzuata aykırı; satış ETBİS'e bağlanmalı (parça 2 B-1) | ❌ RET | "ETBİS kaydı yapılmaz" **ürünün** kayıt yapmadığını söyler, firmanın kaydolmayacağını değil: kayıt firmanın yükümlülüğüdür, ürün kaydın varlığını denetleyemez, alan satış kapısını ve kurulum listesini tutmaz (`02 §3.1.4`, §12.1.4, §12.5 tablosu "satışa başlamadan önce kaydı yaptırır — Tebliğ m.5"; `10` ÖK-5, K-628, K-574). T330 k7 kaynak cümleyle birebirdir. Önerilen "operasyon task'ı" zaten vardır: T601 k9 kurulum tarafı satırlarına ÖK-1…ÖK-12'yi (ÖK-5 ETBİS kaydı dahil) dağıtım kılavuzuna bağlar. Satışı ETBİS kaydına bağlamak `02 §3.1.4`e aykırı olurdu. Yükümlülüğün kendisi (Tebliğ m.5/1-a) doğrudur ve planda karşılanmıştır; resmî metinle ayrıca sınanmadı (ret projenin kendi kaynaklarına dayanır). Seviye Yüksek → Düşük. | Yok |
| F7 | T224 k6 çerez politikası yayınlanana kadar vitrin `Set-Cookie` çıkarır ↔ T331 k3 "mevcut hesapların girişi etkilenmez" (parça 2 B-2) | ❌ RET | Çelişki yok: müşteri hesabı yalnız veri toplayan girişlerin kapısı açıkken (çerez politikası yayında) açılır (`02 §3.1.6`, `02 §12.2.5`) ve çerez politikasının yayını geri alınmaz — kapı yayın anından sonra açılır, geri kapanmaz (`05 §6.7.10`, MK-57). Çerez politikası hiç yayımlanmamışken "mevcut hesap" yoktur ve zorunlu çerez isteyen işlev yoktur (`02 §12.2.5`: "bu sürede satış da veri toplayan girişler de kapalıdır"). T331 k3'ün "mevcut hesaplar etkilenmez"i kapının firma kimliği alanının boşalmasıyla kapandığı durum içindir; o anda politika zaten yayındadır ve T224 k6 süzgeci devrede değildir; panel yanıtları da süzülmez. İki kriter üst dokümanın sadık tüketicisidir. Seviye Yüksek → Düşük. | Yok |
| F8 | T209 k1/k8: altı tekrar-gönderim türü ve dört hassas yanıt sınıfı sayılıyor; KK1 yalnız sepeti, KK8 yalnız oturumlu yanıtı sınıyor, test devri yok (parça 2 B-3) | ⚠️ KISMİ | k1 kolu çürük: k1 listedeki işlemleri açıkça "kendi task'larındadır" diye devreder ve o task'lar sınar (sepete ekleme T503 KK9; iletişim talebi T506; adres ekleme T518; yeni kategori T526). k8 kolu geçerli: `05 §6.5.4` önbellek yasağını dört sınıfa koyar (panel, hesap, sepet ve ödeme adımı, erişim anahtarlı sipariş sayfası), KK8 yalnız "oturumlu yanıt"ı sınar — oturuma bağlı bir süzgeç KK8'den geçer, misafirin erişim anahtarlı sipariş sayfası (kişisel veri) önbelleğe alınabilir kalır; 11'de bu sınıf için başka test yok. Sınıf (4): kabul kriteri ↔ test uyumu. Seviye Orta. Blok 2.555 bayttı (sınır 2.560): KK1'in sepet kolu (T503 KK9'da zaten sınanır) kısaltıldı. | 2 çift: T209 KK8 dört sınıfı sayar; KK1 sepet kolu kısaltıldı (T209 2.554 bayt) |
| F9 | T368 k6 yalnız kanuni faiz uyarısı gösterir, faiz hesaplamaz; m.16/3 faizli iadeyi zorunlu kılar (parça 3 B-1) | ❌ RET | Yönetmeliğin faiz yükümlülüğü doğru ama **satıcı firmaya** aittir; üst doküman bunu bilinçli karar olarak firmaya verir: sistem faizi hesaplamaz, yalnız uyarı gösterir (`02 §7.2.3`); kanuni faizi hesaplayıp müşteriye ödeyen firmadır (`02 §12.5` tablosu); K-499 ve K-571 Yönetmeliğin güncel metnine karşı doğrulanmıştır; `06` faiz alanı ve `07` faiz hesabı yoktur (tutarlı). T368 k6 kaynak cümlenin aynısıdır; T369 ve T370'e faiz hesabı eklemek üst dokümana aykırı kod yazdırırdı. Ürün araçtır, mevzuata aykırı değildir. Seviye Yüksek → Düşük. | Yok |
| F10 | T508 KK2 "fiyat güncel üründen okunur" ↔ KK7 "ilk eklenme fiyatı değişmez" (parça 4 B-1) | ❌ RET | Çelişki yok; iki cümle aynı kaynağın iki kavramını anlatır: kalem fiyatı ve satın alınabilirliği güncel üründen okunur, "ilk eklenme fiyatı" yalnız "fiyatı değişti" işaretinin referansıdır ve adet değişiminde yerinde kalır (`02 §3.16.5`, K-131; `06` `FirstAddedUnitPrice`; `07` UC-20 yan etkiler). 11 referans rolünü kendisi tanımlar (T346 KK5: "ilk eklenme fiyatından farklıysa tek metinle, yönsüz işaretlenir; adet artışı referansı değiştirmez"). Müşteriden alınan tutar sipariş anında donan kalemden gelir (`02` K-80), sepet referans fiyatından değil. Seviye Yüksek → Düşük. | Yok |

**Kısmi kalış:** F3 ikinci modelin önerisinden farklı çözüldü (her task'a ekleme yerine konvansiyon cümleleri üst dokümanın birleşim kuralına hizalandı); F8'de yalnız k8 kolu uygulandı. Yedi ret, doğrulayıcıda kaynakla (`05 §8.7.3`/MK-74, `09 §10.6`, `02 §7.4.1`/K-491/K-493, `02 §3.1.4`/K-628, `02 §12.2.5`, `02 §7.2.3`/K-499, `02 §3.16.5`/K-131) kanıtlandı; "ben böyle düşünüyorum" gerekçesi yoktur. **Tekrar eden RET:** yok — 1. turun beş reddi (T117/T312/T02/T548/T549) bu turda dönmedi; F2 (cayma süresi taşıyıcı) 1. tur F1'e komşu ama farklı iddiadır (yeni bilgi: `02` 5. tur BULGU-1'de aynı iddianın resmî metne karşı reddi).

## 4. Ek bulgular

- **EK-1 (1. tur ek notu; D7'de ayrı hükme bağlandı) — ✅ KABUL, Orta:** T212 k5 ve KK5 "limit aşımı 429" T215'ten önce yürür (T215 T212'ye bağımlı; 429'u üreten genel sınır ve L-1…L-9 T215 madde 5–6'da, KK5–KK6'da sınanır); T212 k5 yalnız 413 ve 400'ü devreder, KK5 T212 yapılırken sınanamaz — bağımlılık sırası ile kabul kriteri ↔ test uyumu çelişir (4). T203 k9/KK9 (1. tur F6) ile aynı kalıp. Yan yüz: T212 k6/KK6'nın makine ucu 429/413'ü parantezle T215/T203'e işaretlidir, değişiklik gerekmez. **2 çift:** T212 k5 429'u T215 madde 5–6'ya işaret eder; KK5 "T215 KK5–KK6'da sınanır" (T212 ~2.2 KB).
- **EK-2 (D2'nin yan gözlemi; `11`'i etkilemez, uygulanmadı):** `02 §7.4.1` Dayanak cümlesi "Mesafeli Sözleşmeler Yönetmeliği m.12/1–3 (23/8/2022 tarihli değişiklik, 1/1/2026'dan beri uygulanır)" der; doğrulayıcının okuduğu değişiklik metninde m.12'nin yürürlüğü 1/10/2022'dir (dördüncü fıkranın son cümlesi 1/1/2023) ve geçici madde yoktur; 1/1/2026 başka bir değişiklikten gelmiş olabilir. Tarih `11`de geçmez. **Üst doküman (`02`) tarih doğrulaması önerilir** (ürün davranışını değiştirmez); bu turda `02` değişmez.

## 5. Kullanıcı onay checklist'i

- [ ] F1 — RET (uygulanmadı)
- [ ] F2 — RET (uygulanmadı)
- [x] F3 — okuma konvansiyonu üst dokümanın birleşim kuralına hizalandı (K-1479; KARAR öneriyle, toplu onayla kaydedildi)
- [x] F4 — dokuz `Dokümanlar` alanı tamamlandı (K-1479)
- [ ] F5 — RET (uygulanmadı)
- [ ] F6 — RET (uygulanmadı)
- [ ] F7 — RET (uygulanmadı)
- [x] F8 — T209 KK8 dört önbellek sınıfını sınar (K-1479)
- [ ] F9 — RET (uygulanmadı)
- [ ] F10 — RET (uygulanmadı)
- [x] EK-1 — T212 k5/KK5 429'u T215'e işaret eder (K-1479)

Gösterilmeden kaydedilen ⚠ karar yok; çok kritik soru yok (F3 KARAR'ı doküman yazım biçimidir, ürün/sözleşme/hukuk etkisi yoktur). Önerilen `00`/`CLAUDE.md`/`SETUP.md` değişikliği yok.

## Sonuç

**SONUÇ: TEMİZ DEĞİL — 10 ayrı bulgu (10 ham) + EK-1; 1 kabul + 2 kısmi + EK-1 kabul (16 şartname çifti), 7 ret.** Skill'in çıkış kuralı: bütün çağrılar aynı anda TEMİZ dönmedi (altı çağrının beşi bulgu getirdi, parça 5 TEMİZ); kabul edilen bulgular arasında mevzuata aykırılık ya da para/hak kaybı sınıfından olan yok (hepsi sınıf (4), seviye Düşük/Orta); 5. tur eşiği (beşinci turdan itibaren çıkış kuralı) henüz gelmedi. Sonraki tur: 3. tur (`11_CROSS_REVIEW_R3.md`), aynı betikler ve aynı parça sınırlarıyla, `11` v0.39 üzerinde. Etki yansıtma (Faz 5) döngü TEMİZ olunca ayrı adımdır; bu turda yapılmadı.
