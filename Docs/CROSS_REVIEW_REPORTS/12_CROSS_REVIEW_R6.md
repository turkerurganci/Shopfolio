# Cross-Review — 12 Doğrulama Protokolü (Tur 6)

**Tarih:** 2026-11-02 · **Hedef:** `Docs/12_VALIDATION_PROTOCOL.md` v0.29 (girdi commit'i `c8d26f4`, 839 366 bayt, 2 639 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren (K-1399 (1); mekanizma okuma paragrafıyla) · **Sonuç:** TEMİZ — çıkış kuralıyla, 6 tur — 9 ham bulgu = 9 ayrı bulgu (altı çağrıdan): 1 doğru (Düşük, iki yerin metin farkı) · 2 çürük · 6 doğrulayıcısız tekrar eden iddia; 1'i uygulandı (1 şartname çifti).

## 1. Koşum

Betikler, istem, parça notu ve **parça sınırları değişmedi** (`12_CROSS_REVIEW.md` §1; kayıt K-1561): `C:\\Users\\teknodeveloper\\.claude\\helper-state\\shopfolio\\a10-kd-cross\\betik\\` — `build_inputs.py` (`python -I build_inputs.py <doküman> template12.md prompt.txt partnote.txt <girdi klasörü>`; kapsanmayan satır: yok), `prompt.txt`, `partnote.txt`, `template12.md`. Koşum: `bash scripts/doc-scans/cross_review_ikinci_model.sh <girdi> <çıktı> c1 p1 p2 p3 p4 p5` (K-1484). Altı çağrı tek komutta, ilk denemede çıkış 0, hepsi çıktı verdi; kullanım sınırı dolmadı, yedek modele geçilmedi (`status.txt`: `model=codex gpt-5.6-terra high`). Girdi `12` v0.29: 5. turun uyguladığı `mock` satırların final kapısı (§6.3, §7.4, §11, §11.1 madde 4), VAL-A826 ve üst/alt bilgi dışında önceki turdan farkı yoktur.

| Çağrı | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|
| tek parça (c1) | 856 849 bayt | 392 942 | 208 sn | 2 bulgu |
| P1 (§1–§4.2, §5–§11) | 136 807 bayt | 92 638 | 194 sn | 2 bulgu |
| P2 (§4.3–§4.12) | 280 629 bayt | 145 348 | 219 sn | 2 bulgu |
| P3 (§4.13–§4.20) | 336 919 bayt | 138 714 | 64 sn | 1 bulgu |
| P4 (§12 girişi, §12.1–§12.7) | 202 454 bayt | 135 455 | 213 sn | 1 bulgu |
| P5 (§12.8–§12.13) | 216 279 bayt | 101 525 | 133 sn | 1 bulgu |

**İki koşumun ölçümü.** Tek parça çağrı iki bulgu getirdi (VAL-A075 taşıyıcı; T601 kriter numarası); parçalı koşum yedi ham bulgu getirdi (P1 2, P2 2, P3 1, P4 1, P5 1). Bu tur altı bulgu daha önce hükme bağlanmış iddianın yeniden gelişidir (VAL-A075 taşıyıcı, VAL-A080/A081/A231, CSP `frame-src`, VAL-A985, VAL-A913, 6563 m.11 saklama); üç bulgu yenidir. Yasal dayanak taşıyan yeni iddia (F3) doğrulayıcıda resmî metne karşı okundu (okuma tarihi 2026-11-02): Mesafeli Sözleşmeler Yönetmeliği m.12/1 (RG 23/8/2022-31932, madde 9 ile değişik) ve RG 24/5/2025-32909 değişikliği.

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**(a) Tek parça çağrı:**
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §4.6 `VAL-A075`; §12.9 `VAL-A903`
> Alıntı: “Teslimden sonraki caymada geri ödeme süresi malın firmaya ulaştığı tarihten itibaren on dört gündür.”
> Sorun: İade için ön bilgilendirmede belirtilen taşıyıcı kullanılmışsa güncel kural, 14 günlük iade süresini malın satıcıya ulaşmasından değil, tüketicinin taşıyıcıya tesliminden başlatır; yalnız farklı taşıyıcıda satıcıya ulaşma tarihi esas alınır. Bu satır ve parça iade senaryosu, firmaya ulaşmayı her durumda başlangıç sayarak iade süresini uzatır. Bu, (1) yürürlükteki mevzuata aykırılık ve (2) tüketicinin bedeline geç ulaşmasına yol açan para/hak kaybıdır. [Ticaret Bakanlığı açıklaması](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)
> Öneri: `VAL-A075` ve `VAL-A903`ü iki dala ayırın: belirtilen taşıyıcıda teslim tarihi; başka taşıyıcıda satıcıya ulaşma tarihi. Her dal için 14 gün sınırını ve kısmi iade senaryolarını bu başlangıçlarla doğrulayın.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §12.1
> Alıntı: “K-1453 (canlı kanıtın task'ı T601'dir; kabul kriteri 8)” / “`11` T601'in kabul kriteri 10 bu bölümün kimliklerini taşır”
> Sorun: Aynı canlı kabul kanıtı iki farklı T601 kabul kriterine bağlanmış. Task kapanışında hangi kabul kriterinin F6 zincirini zorunlu tuttuğu mekanik olarak belirlenemez; kanıt doğru satırlara bağlanmadan taskın kapatılması mümkündür. Bu, doküman içi çelişkidir (4).
> Öneri: Doğru T601 kabul kriterini tekil olarak belirleyin; §12.1, başlık notu ve `11`deki geri yazımı aynı kriter numarası ve `VAL-F001`–`VAL-F012` eşlemesiyle hizalayın.
>
> SONUÇ: 2 BULGU

**(b) Parça 1:**
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: Aşama 4’ten park edilen girdiler — “Elektronik ticaret kayıtlarının on yıllık saklaması”
> Alıntı: “siparişin kişisel verisi üç yılda silinir, kişisel verisiz kaydı on yıl dolmadan silinmez”
> Sorun: Bu, e-ticaret işlemine ilişkin kişisel verilerin üç yılda topluca silinmesini doğrulatıyor. Oysa 6563 sayılı Kanun m. 11/3, hizmet sağlayıcının kanun kapsamındaki iş ve işlemlere ilişkin bilgi, belge, defter ve elektronik kayıtlarını işlem tarihinden itibaren on yıl saklamasını zorunlu kılar. İşlem kaydının alıcı ve olay değerleri boşaltılarak denetlenemez hâle gelmesi, firmayı saklama ve ispat yükümlülüğü bakımından ihlal riskiyle bırakır.
> Öneri: Üç yıllık imhayı genel kural olmaktan çıkarın; on yıllık e-ticaret saklama yükümlülüğü için zorunlu kişisel veri ve belge alanlarını on yıl boyunca, erişimi kısıtlanmış biçimde saklayın. Yalnız hukuken saklanması gerekmeyen alanlar için ayrı ve gerekçeli imha senaryosu yazın.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §6.1, §6.2 ve §6.3
> Alıntı: “Task'ın kabul kriterlerine ve kontrol listesine bağlı bütün satırlar PASS ya da `KABUL EDİLMİŞ RİSK`” / “okunan satır `PASS` ya da kaydı bulunan `KABUL EDİLMİŞ RİSK` (§8.4) değilse gate FAIL'dir” / “`BEKLEMEDE` kalan DÜŞÜK satır final çıkışı bloklamaz”
> Sorun: DÜŞÜK bir satır task veya faz kapısında okunuyorsa `BEKLEMEDE` kalması task/gate’i FAIL yapar; final çıkış ise aynı durumun bloklamadığını söylüyor. Bu, düşük severity’de ön koşulu sağlanamamış satırla MVP’nin geçip geçmeyeceğini mekanik olarak belirsiz bırakır.
> Öneri: Tek bir kural belirleyin: DÜŞÜK `BEKLEMEDE` satırlar kapılarda da açıkça istisna tutulacaksa raporlama ve kayıt şartını yazın; tutulmayacaksa §6.3’teki “final çıkışı bloklamaz” hükmünü kaldırın.
>
> SONUÇ: 2 BULGU

**(c) Parça 2:**
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: VAL-A081; VAL-A080, VAL-A231
> Alıntı: "kabul ya da ret yalnız girilen tarihin pencereye ve bir yıl sınırına göre yerine bağlıdır"
> Sorun: VAL-A081 gönderim tarihini tutmadan ve yalnız girilen tarihe göre ret öngörürken, VAL-A080 süre dışında ulaşmış ama süresinde gönderilmiş caymanın kabulünü; VAL-A231 ise uygunluğun gönderim zamanıyla ölçülmesini ister. Aynı talep iki satırda hem kabul hem ret sonucuna bağlanmıştır. Bu, tüketicinin süresinde yönelttiği cayma hakkının kaybına yol açar (sınıf 1 ve 4).
> Öneri: Cayma kaydına beyanın yöneltildiği tarih ile dayanağını ekleyin; sistem bu olguyu kesinleştiremiyorsa otomatik ret yerine inceleme/onay akışına alın. Süre uygunluğunu ulaşma değil yöneltme tarihine göre değerlendirin.
>
> BULGU-2
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: VAL-A075
> Alıntı: "Teslimden sonraki caymada geri ödeme süresi malın firmaya ulaştığı tarihten itibaren on dört gündür."
> Sorun: Geri ödeme süresini malın firmaya ulaşmasıyla yeniden başlatmak, cayma bildiriminin ulaşmasından itibaren işleyen yasal üst süreyi aşabilir; mal örneğin bildirimin son günlerinde ulaşırsa müşteri fazladan 14 gün bekletilir. Bu, bedel iadesinin gecikmesine ve mevzuata aykırılığa yol açar (sınıf 1). Mesafeli Sözleşmeler Yönetmeliği’nin iade kuralı bildirimin ulaşmasını esas alır; 2025 değişikliği de yürürlüktedir. [Ticaret Bakanlığı metni](https://ticaret.gov.tr/data/5d42aa1613b87632542a2db3/575c6563fca6dc205ef582989cec3272.pdf), [Resmî Gazete değişikliği](https://www.resmigazete.gov.tr/eskiler/2025/05/20250524-2.htm)
> Öneri: Beklenen sonucu, geri ödemenin cayma bildiriminin firmaya ulaştığı tarihten itibaren yasal azami sürede tamamlanması olarak düzeltin; malın dönüşü için hukuken uygulanabilir bekletme varsa bunu süreyi yeniden başlatmadan ayrı koşul olarak modelleyin.
>
> SONUÇ: 2 BULGU

**(d) Parça 3:**
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §4.16 VAL-E067; §4.20 VAL-A980
> Alıntı: “Ödeme sayfasının politikasında çerçeve izni `frame-src 'self' https://www.paytr.com`'dur” / “Her yanıtta içerik güvenlik politikası (`default-src 'self'` …) vardır”
> Sorun: VAL-A980 her yanıtta `frame-src` içermeyen `default-src 'self'` politikasını zorunlu kılar. CSP’de `frame-src` yoksa çerçeve yükleme `default-src` kuralına düşer; dolayısıyla PayTR çerçevesi engellenir. Bu, VAL-E067’nin zorunlu tuttuğu PayTR ödeme çerçevesiyle çelişir ve kartla ödeme akışını çalışamaz hâle getirir. İç çelişki (4) ve ödeme kaybı riski (2).
> Öneri: VAL-A980’e ödeme sayfası için açık bir istisna ekleyin: bu sayfanın tek CSP politikası `frame-src 'self' https://www.paytr.com` içersin. Genel politika ile ayrı, kısıtlayıcı iki CSP başlığı gönderilmemeli; ödeme sayfasında tekleştirilmiş politika üretilmelidir.
>
> SONUÇ: 1 BULGU

**(e) Parça 4:**
> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §12.6 / VAL-A985
> Alıntı: “Sahte sağlayıcıya istek gitmez (istek sayısı 0); geri ödeme kaydı "gerçekleşmedi" olur; … havale yolu yalnız müşterinin seçimiyle açılır”
> Sorun: Kabul edilmiş ayıp talebinde, kart anahtarları kaldırılmışsa sistem geri ödemeyi hiç yapmamayı PASS koşulu sayıyor. Müşteri havale yolunu seçmezse iade çıkışsız kalır. Bu, tüketicinin sözleşmeden dönme seçimlik hakkı seçildiğinde satıcının bedeli derhâl iade yükümlülüğüne aykırıdır. [Ticaret Bakanlığı açıklaması](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/ayipli-mal-ve-hizmetler-hakkinda-bilgilendirme)
> Öneri: Anahtar kaldırma kapısı, ayıp sorumluluğu süresindeki kartlı siparişlerden doğabilecek iadeleri de kapsamalıdır. Bu mümkün değilse firma tarafından başlatılan, müşterinin ek seçimine bağlı olmayan hukuken uygun alternatif iade yolu tanımlanmalı; VAL-A985 bu yolun fiilen tamamlandığını doğrulamalıdır.
>
> SONUÇ: 1 BULGU

**(f) Parça 5:**
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §12.10 / VAL-A913
> Alıntı: "bağlantı üretilince kalan hak bir azalır ve bağlantıyla gelen gönderim ikinci hak düşürmez."
> Sorun: Bu, aynı bölümün kaynak kuralındaki “dosyanın gönderimi başladığında” koşuluyla çelişir. Bağlantı üretimi ile dosya isteğinin kabulü ayrı adımlardır; aradaki yönlendirme/ağ kopmasında müşteri dosyayı hiç almadan ücretli indirme hakkını kaybeder. Bu müşteri hak ve para kaybına yol açar (2).
> Öneri: Hakkı bağlantı üretiminde değil, bağlantıyla gelen dosya isteği kabul edilip aktarım başlatılırken atomik olarak düşürün. Bağlantı üretimi başarısız veya kullanılmamışsa hak değişmesin; test satırı bu ayrımı doğrulasın.
>
> SONUÇ: 1 BULGU

## 3. Bağımsız değerlendirme

Dokuz ham bulgu dokuz ayrı bulgudur. Üçü için üç Opus/high doğrulayıcı (D1 F2; D2 F1; D3 F3; **adım sınırı 3/20**). Öteki altısı (T1–T6) yeni kanıt taşımadığı için doğrulayıcıya gitmeden "tekrar eden RET" olarak kaydedildi (hüküm dayanakları tabloda). Hüküm dosyaları: `C:\\Users\\teknodeveloper\\.claude\\helper-state\\shopfolio\\a10-kd-cross\\r6\\hukum_D1.md`…`hukum_D3.md` (`12_audit_hukum_denetle.py`: hüküm doğru 1 · çürük 2, çift 1, uygulanabilir çiftte sorun 0).

| # | Parça · Yer | Konu | Hüküm | Seviye (model → doğrulayıcı) | Gerekçe ve şartname |
|---|---|---|---|---|---|
| F1 | P1 · §6.1, §6.2, §6.3 | DÜŞÜK `BEKLEMEDE` satır: kapılar "PASS ya da KER değilse FAIL" ↔ §6.3 "final çıkışı bloklamaz" | **doğru** | Orta → Düşük | **Yeni kanıt** (2. tur M13 yalnız `FAIL` alan DÜŞÜK satırı düzeltti; §6.1/§6.2/§5 ile ilişkiyi sınamadı): 12:1736 "`BEKLEMEDE` kalan DÜŞÜK satır final çıkışı bloklamaz" ↔ 12:1718 "okunan satır `PASS` ya da kaydı bulunan `KABUL EDİLMİŞ RİSK` (§8.4) değilse gate FAIL'dir" + 12:1688 "Final çıkış — Yedi fazın hepsinin gate'i PASS'tır" + 12:1694 (gate-check/SKILL.md:67 "`BEKLEMEDE` dahil — verdict FAIL"). Örnek: F4 gate'inin okuduğu `sandbox` DÜŞÜK E satırı (VAL-E018, E022, E036, E038) `BEKLEMEDE` kalırsa F4 gate FAIL olur ve final açılamaz, oysa §6.3 lafzen ayni satırın finali bloklamadığını söyler. Sonuç §5/§6.2'den belli olduğu için düzeltme istisnanın alanını daraltmaktır (öneri "hükmü kaldır" gereksiz). Kaynak yok, para/mevzuat/çıkışsız akış değil: iki yerin metin farkı (4), Düşük. **Uygulandı (1 çift):** istisna task çıkışının (§6.1) ya da faz gate'inin (§6.2) okuduğu satırı kapsamaz; yalnız o kapılardan sonra `BEKLEMEDE`'ye dönen (§11.1 madde 4) ve F6 gate'inde okunmayan satır içindir. Not: istisnanın fiili alanı F6 gate'inin "E `gerçek` — K-23'ün zinciri" okumasına bağlıdır (VAL-E030, E031, E033, E034, E042); alan boş çıkarsa istisna bilgi notu olarak kalır (checkpoint'e). |
| F2 | tek parça · §12.1 K-1453 ↔ `11` T601 | Canlı kanıt "kabul kriteri 8" ve "kriter 10" diye iki farklı kriterle anılıyor | **çürük** | Orta → Düşük | İki kriter ayrı işi taşır: `11` T601 kriter 8 canlı kanıtın kendisi, kriter 10 kanıtın `VAL-F001`…`VAL-F012` satırlarına bağlanışıdır (K-1453, K-1537; 12:1936 "kabul kriteri 8", 12:1942 ve 12:1960 "kriter 10 … eşlenmiştir"; VAL-F012 kaynağı 12:1957 "11 T601 (8, 10)"). Çelişki yok. |
| F3 | P2 · §4.6 VAL-A075 | Geri ödeme süresi malın ulaşmasıyla başlıyor; yasal üst süre bildirimin ulaşmasından | **çürük** | Yüksek → Düşük | Mesafeli Sözleşmeler Yönetmeliği m.12/1 (RG 23/8/2022-31932, madde 9 ile değişik, resmigazete.gov.tr/eskiler/2022/08/20220823-2.htm): satıcı malı iade için ön bilgilendirmede belirtilen taşıyıcıya teslim tarihinden itibaren 14 gün içinde iade eder; tüketici iade için öngörülenin haricinde bir taşıyıcı kullanırsa yükümlülük malın satıcıya ulaştığı tarihten başlar. "Bildirimin ulaştığı tarih" başlangıcı yalnız teslimden önceki caymada (m.12/2) ve hizmette (m.12/3) geçerlidir. RG 24/5/2025-32909 m.12'de yalnız 4. fıkradaki ibareyi ve 5. fıkrayı değiştirir, m.12/1'e dokunmaz. İkinci modelin dayandığı Bakanlık PDF'i 8/12/2014 tarihli ilk metindir. 4. tur F2 ile aynı sonuç (firma iade taşıyıcısı belirlemediği için `02 §7.4.1` ve K-491, K-493 uyarınca satıcıya ulaşma tarihi esastır). |
| T1 | tek parça · §4.6 VAL-A075, §12.9 VAL-A903 | İade süresi koşulsuz malın firmaya ulaşmasına bağlanıyor (taşıyıcı) | **tekrar eden RET (doğrulayıcısız)** | Yüksek → Düşük | **Önceki hüküm:** 4. tur F2 (çürük; m.12/1, K-491, K-493). Aynı yer, aynı iddia; yeni resmî metin yok (bu turun F3 doğrulayıcısı m.12/1'i yeniden okudu, sonuç aynı). |
| T2 | P1 · "Aşama 4'ten park edilen girdiler" | E-ticaret kayıtlarının on yıllık saklaması; kişisel veri üç yılda siliniyor | **tekrar eden RET (doğrulayıcısız)** | Yüksek → Düşük | **Önceki hüküm:** 4. tur F5 (VAL-D091; K-951 (2); `02 §12.2.7` "kişisel veri: 3 yıl · kişisel verisiz kayıt: en az 10 yıl"; 6563 m.11/3 için bilinçli karar). Aynı iddia, yeni kanıt yok. |
| T3 | P2 · VAL-A081; VAL-A080, VAL-A231 | Cayma tarihi: kabul ya da ret yalnız girilen tarihe göre | **tekrar eden RET (doğrulayıcısız)** | Yüksek → Düşük | **Önceki hüküm:** audit 95, deep-2a 23, 2. tur F3 (K-1561), 3. tur F1 (K-1565, K-1566); K-1563 kaynak düzeltmesi etki yansıtmaya devredildi (VAL-A081/A236/A532/B142). Aynı yer, aynı iddia, yeni kanıt yok. |
| T4 | P3 · §4.16 VAL-E067; §4.20 VAL-A980 | `default-src 'self'` PayTR çerçevesini engeller | **tekrar eden RET (doğrulayıcısız)** | Yüksek → Düşük | **Önceki hüküm:** 1. tur F6 (K-1561), 2. tur F4, 5. tur F3 (üçüncü kez). VAL-E067'nin çerçeve izni ödeme sayfasının politikasındadır ve DV-5 geçiş sayfasının engellenmemesini sınar. Yeni kanıt yok. |
| T5 | P4 · §12.6 VAL-A985 | Kart anahtarları kaldırılınca ayıp iadesi hiç gönderilmez | **tekrar eden RET (doğrulayıcısız)** | Yüksek → Düşük | **Önceki hüküm:** 1. tur F7 (K-1561): iki nokta kaynakta bilinçli kararla kapatılmıştır, `12` kaynağı birebir taşır (K-652). Yeni kanıt yok. |
| T6 | P5 · §12.10 VAL-A913 | Hak bağlantı üretilince düşer, dosya gönderimi başlayınca değil | **tekrar eden RET (doğrulayıcısız)** | Yüksek → Düşük | **Önceki hüküm:** 1. tur F8, 3. tur F6: satır `05 §7.7.4` ilk cümlesini birebir taşır (K-1225 (1)); hak bağlantının üretildiği anda düşer. Yeni kanıt yok. |

**Hüküm dağılımı:** 1 doğru (F1) · 2 çürük (F2, F3) · 6 doğrulayıcısız tekrar eden RET (T1–T6); seviyesi yükselen yok.

**Sonuç sınıfı ve çıkış.** Kabul edilen tek bulgu F1'dir: düşük şiddette iki yerin metin farkı (4); mevzuata aykırılık, para ya da hak kaybı ya da çıkışsız akış değildir. Skill Faz 4 madde 5: 5. turdan itibaren kabullerin hepsi bu sınıftaysa bulgular uygulanır, aynı kalıp tarandı ve döngü o turda kapanır — **sonuç TEMİZ — çıkış kuralıyla, 6 tur**. Aynı kalıp ("istisna cümlesi ↔ kapı okuması"): `12`'de "bloklamaz" geçen tek yer §6.3'tür (12:1736); `00` ve `.claude/skills/` altında `DÜŞÜK … bloklamaz` anlatımı yoktur. Hedefli mercek (madde 6) gerekmez: bu turun kabulü Düşüktür. Sınıfın kalan riski aşamanın checkpoint'ine devredildi: karar kaydının açık süreç maddesi (K-1569) sınıfın adını ve 4.–6. tur örneklerini taşır; checkpoint'in iç tutarlılık merceği "kapı/istisna cümlesi ↔ kapı satırı" ve "satır ↔ satır" farkını `12`'nin tamamında arar.

## 4. Ek bulgular ve devirler

- **Etki yansıtmaya (F1):** istisnanın fiili alanı F6 gate'inin "E `gerçek` — K-23'ün zinciri" okumasına bağlıdır (12:1728; VAL-E030, E031, E033, E034, E042 `gerçek` DÜŞÜK satırları); okuma bütün `gerçek` E satırlarını kapsıyorsa istisna yalnız bilgi notudur — checkpoint ya da etki yansıtma bu hizayı tek cümleyle kapatabilir.
- **Etki yansıtmaya (önceki turlardan sürenler):** K-1568 ⚠ meslek odası üç tipte zorunlu (kaynak: `02 §3.1.3`, `§3.1.5`, `10` KP-35/KP-58, `04 §7.2.19.6`, `§9.16`, `06` ChamberName, `11`; sonra `12` VAL-A854, A855, A285 deseniyle yeni satır, A286, A126, A125, A856); K-1517 (5) kayıt notu (çekirdek E satırları = Kanıt ortamı `sandbox`/`gerçek`); `12` P7 ↔ `12_kapsam_hizasi.py` söz hizası (4. tur F4); `02 §7.3.5` notu (K-1565); `10` KP-31 ↔ `02 §3.28.1`; K-1545 (1) yük satırları `mock` ↔ K-1518 tanımı kayıt notu; `skills/validate` PASS tanımı ↔ `12 §6.1` istisnası; K-1563 kaynak düzeltmesi ve VAL-A081/A236/A532/B142; K-1566 kaynak değişikliği ve VAL-A900, A905, A687, `06 §4.30`.
- **Hedefli mercek:** gerekmez (§3 son paragraf).
- **`00`, `CLAUDE.md`, `SETUP.md`:** değişiklik önerilmedi.

## 5. Kullanıcı onay checklist'i

Toplu onay modu açık (K-1504); karar **(toplu onayla kaydedildi)**; ⚠ taşıyan yeni karar yok.

- [x] F1 — §6.3 DÜŞÜK `BEKLEMEDE` istisnası (doğru, Düşük) — 1 çift uygulandı
- [ ] F2, F3 — çürük, uygulama yok
- [ ] T1–T6 — tekrar eden RET, uygulama yok
- [x] Sayımlar: satır eklenmedi — matris 1.502 satır, ORTA 733, KRİTİK 637, DÜŞÜK 132, `[güvenlik]` 63; `12_matris_denetle.py --kapsam 1-77`, `12_kapsam_hizasi.py`, `12_atif_denetle.py` hata 0

**Sonuç: TEMİZ — çıkış kuralıyla, 6 tur; cross-review döngüsü kapandı.** Sıradaki: etki yansıtma (A10-KD-etki).

## 6. Etki yansıtma (A10-KD-etki; cross-review Faz 5; K-1570; `12` v0.30 → v0.31)

Kalite döngüsünün bütün geri beslemeleri (audit, audit-2, deep review, deep-2, cross-review 1–6) kümeler hâlinde `01`–`11`, L4 ve `12`'ye yansıtıldı. İş iki işçiye bölündü (1/2: K-1568…K-1566 kaynakları, P7, KP-31, F6 istisnası, L4; 2/2: aşağıdakiler).

1. **Deep review #130 (seyreltme penceresi):** karara bağlandı. Seyreltmenin penceresi `07 §2.8.2`'nin altmış saniyelik kayan penceresidir; IP başına ilk imzası tutmayan istek bir `SignatureFailureSampled` satırı doğurur, sonraki istekler altmış saniye satır doğurmaz ve sayılır, sayı süre dolduktan sonraki ilk satırın alanıdır. Yazıldı: `05 §4.5.2`, `§6.3.5` ("pencerenin türü `07`'nin işidir" cümlesi kapandı), `§9.1.3`; `07 §2.8.2`, `§4.30` UC-80 hata satırı, §1.1.11 ve §1.1.12 eşleme satırları; `09 §11.1` madde 5, `§18.2` olay listesi; `12` VAL-E045 (pencereyi sınayan beklenen sonuç).
2. **`[güvenlik]` ORTA taraması (VAL-A190 emsali):** 12 matris satırı (görevdeki "13" başlık notundaki bir anmayı da sayıyordu); 4 Opus/high doğrulayıcı (adım sınırı 4/20): D1 (VAL-A352, A353, A358, A360) 2 doğru · 2 çürük; D2 (VAL-A354…A357) 4 çürük; D3 (VAL-A617, A685) 1 doğru · 1 çürük; D4 (VAL-A628, A629) 2 çürük. **KRİTİK'e yükseltilen 3 satır:** VAL-A353 ve VAL-A358 (L-5'in e-posta ekseni; başkasının siparişine dağıtık deneme), VAL-A617 (başka oturumun değeriyle gelen formda yazım olmamalı; VAL-A618/A619 KRİTİK). Yan bulgu: VAL-A489 `Beklenen` hücresindeki "numara ekseni" `02 §8.2.2` ile çelişiyordu, "sorguya yazılan e-posta ekseni" yapıldı; satırın severity'si (L-5'in 07 hata kodu) ORTA kaldı, doğrulayıcı hükmü yok — checkpoint'in iç tutarlılık merceğine devredildi. Sayımlar: KRİTİK 638 → 641, ORTA 733 → 730 (`[güvenlik]` 63 aynı).
3. **Kayıt notları:** K-1517 (5) ve K-1545 (1) satırlarına "Kayıt notu, K-1570" eklendi.
4. **Kaynak satırı taraması:** değişen bölümlerin `*Kaynak:` satırlarına K-1568, K-1563, K-1565, K-1566, K-1570 betikle eklendi (01, 02, 04, 05, 06, 07, 09, 10; `03` ve `11`'de Kaynak satırı konvansiyonu yok).
5. **Park blokları ve yeni alan taraması:** meslek odası, bir yıl sınırı ve ayıplı adet kalıpları 01–11'de arandı; başka etkilenen yer çıkmadı (`ChamberName`/`ProfessionalConductRules` yalnız `06`, `07` SaveCompanyInput ve `04` 7.2.19.6'dadır).
6. **Sürümler:** `01` v0.34 · `02` v0.113 · `03` v0.48 · `04` v0.54 · `05` v0.51 · `06` v0.47 · `07` v0.41 · `09` v0.30 · `10` v0.63 · `11` v0.63 · `12` v0.31; karar kaydı v0.326.
7. **L4:** `skills/gate-check` (F6 final çıkış `12 §6.3` adımı, "bu MVP'nin son uygulama fazı F6'dir") ve `skills/validate` uygulandı.
8. **Kapılar:** `12_kapsam_hizasi.py`, `12_matris_denetle.py --kapsam 1-77`, `12_atif_denetle.py` hata 0.

`00`, `CLAUDE.md` ve `SETUP.md` değişikliği önerilmedi. **Kalite döngüsü tamam.**

