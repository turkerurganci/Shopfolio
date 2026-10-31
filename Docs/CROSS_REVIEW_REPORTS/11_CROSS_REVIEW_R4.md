# Cross-Review — 11 Implementation Planı (Tur 4)

**Tarih:** 2026-10-31 · **Hedef:** `Docs/11_IMPLEMENTATION_PLAN.md` v0.41 (girdi commit'i `d3f4f34`, 759 622 bayt girdi, 7 761 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren, mekanizma okuma paragrafıyla (K-1399; `11_CROSS_REVIEW.md` §1) · **Koşum:** iki koşum — tek parça çağrı ve beş parçalı kontrol koşumu · **Sonuç:** **kabul edilen bulgu yok** — 10 ham bulgu, 8 ayrı bulgu; sekizi de reddedildi (dördü tekrar eden RET); belge içeriği değişmedi, yalnız sürüm yükseldi (v0.41 → v0.42).

## 1. Koşum

Betikler, istem, parça notu, şablon ve **parça sınırları 1. turdakinin aynısıdır** (`11_CROSS_REVIEW.md` §1; K-1478 — sınır başlıkla tanımlıdır, değişmez). İstem tek karakter değiştirilmeden kullanıldı. Girdi `build_inputs.py` ile kuruldu (`uncovered lines: none`); `codex login status` → "Logged in using ChatGPT". Altı çağrı tek Bash komutunda paralel koştu (`run_codex.sh r4/in r4/out c1 p1 p2 p3 p4 p5`; her çağrının kendi boş çalışma klasörü); kullanım sınırı dolmadı, çıktısız dönen çağrı olmadı, yedek modele geçilmedi. Çalışma dizini: `C:\Users\teknodeveloper\.claude\helper-state\shopfolio\kd-cross\r4\` (girdiler `in/`, ham çıktılar `out/`, doğrulayıcı paketleri `dogrulayici_<k>.md`, hükümler `hukum_D<k>.md`, talimat `talimat_dogrulayici.md`, önceki turların ilgili RET hükümleri `onceki_hukumler.md`).

| Çağrı | Bölümler | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|
| Tek parça | tüm doküman | 759 622 bayt | 430 596 | 103 sn | 3 bulgu |
| Parça 1 | §1–§4 · §5 Task yapısı · F0 · F1 (satır 65–866) | 96 343 bayt | 42 862 | 90 sn | 2 bulgu |
| Parça 2 | F2 · F3 T301–T345 (867–2841) | 197 064 bayt | 122 993 | 300 sn | 2 bulgu |
| Parça 3 | F3 T346–T390 · F4 (2842–4219) | 150 962 bayt | 126 100 | 307 sn | 2 bulgu |
| Parça 4 | F5 · F6 · §6 (4220–5633) | 149 635 bayt | 72 387 | 170 sn | TEMİZ |
| Parça 5 | §7 · §8 · §9 (5634–dosya sonu) | 254 332 bayt | 129 292 | 47 sn | 1 bulgu |

Web araması satırları (çağrı günlükleri): tek parça 8, parça 1 0, parça 2 8, parça 3 12, parça 4 2, parça 5 2. **İki koşumun ölçümü:** tek parça 3 bulgu, parçalı koşum 7 bulgu; örtüşme `Down` yasağı ↔ geri alma (iki koşum) ve ETBİS (tek parça B-1, parça 2 B-2) iddialarında; öteki beşi yalnız parçalı koşumda göründü. İki koşum bu turda da sürdü.

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**(a) Tek parça çağrı:**

> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §5, T330 kabul kriteri 7
> Alıntı: "ETBİS kaydı yapılmaz; kayıt bilgisi isteğe bağlı alandır, satış kapısını tutmaz ve doluysa doğrulama bandı gösterilir (`02 §3.1.4`, §12.5)."
> Sorun: Kendi e-ticaret ortamında tüketiciye satış yapan hizmet sağlayıcının faaliyete başlamadan ETBİS’e kaydı zorunludur. Plan kaydı bilinçli biçimde satış kapısından çıkararak firmayı idari yaptırım riskine sokar. Bu, yürürlükteki mevzuata aykırıdır (1). [Ticaret Bakanlığı](https://ticaret.gov.tr/ic-ticaret/sikca-sorulan-sorular/elektronik-ticaret)
> Öneri: ETBİS kaydını F0/F6 dış ön koşulu ve T331 satış kapısının zorunlu koşulu yapın; kayıt kanıtı olmadan satış açılmamasını kabul kriteri ve gate kontrolü olarak ekleyin.
>
> BULGU-2
> Kriter: Teknik doğruluk
> Seviye: Orta
> Yer: §3 F1 çıkış ürünü; §5, T04 kabul kriteri 5; T103–T119
> Alıntı: "Migration dry-run komutu göç demetini boş veritabanına ve son yayımlanan sürümün şemasına uygular ve geri alma sınamasını koşar; sürüm yoksa yalnız ilki koşar, atlama CI çıktısında yazılır; göçsüz model değişikliği adımı düşürür (`09 §21.4` madde 3, §10.6; K-1356)."
> Sorun: F1 çıkışı göçlerin “geri alınabilir” olmasını ister ve T04 geri alma sınaması koşar; buna karşılık veri göçü task’larının kabul kriterleri `Down` adımını yasaklar. Plan, geri alma mekanizmasını başka bir yolla tanımlamadığından aynı güvence hem zorunlu hem de uygulanamaz durumdadır. Bu, faz kapısı ile task kabul kriterlerinin çelişmesidir (4).
> Öneri: Tek bir geri alma stratejisi belirleyin: ya `Down`/ters göçleri tanımlayıp sınayın ya da ileri-yönlü genişlet–daralt yaklaşımını açıkça seçip F1 ve T04’teki “geri alma” şartını uygulama geri dönüşü ve kurtarma provasına göre değiştirin.
>
> BULGU-3
> Kriter: Teknik doğruluk
> Seviye: Orta
> Yer: §3.1; F3 yürütme sırası; §5, T331 kabul kriteri 8
> Alıntı: "Kapı kararı tek yerde hesaplanır ve karşılanmayan koşulları adıyla verir; vitrin, sepet, ödeme ve panelin kurulum kontrol listesi aynı sonucu okur; ödeme yöntemi koşulunu T352 ve T353, yasal metin koşulunu T366 bağlar, bağlanmamış koşul karşılanmamış sayılır (`02 §6.1.4`, §10.8.2)."
> Sorun: F3 yürütme sırasında T331, T366’dan ve T352/T353’ten önce gelir; ancak kendi kabul kriteri bu sonraki task’ların çıktılarının bağlanmasına dayanır. Planın “task başlamadan bütün girdileri hazır olmalı” ve yürütme sırasının bağımlılıkları bozmaması güvencesi ihlal edilir (4).
> Öneri: T331’i T352, T353 ve T366’dan sonraya taşıyıp bağımlılıklarını ekleyin; alternatif olarak erken task’ı yalnız kapı arayüzüyle sınırlayıp bağlama işini bu üç task’a bağımlı ayrı bir task’a bölün.
>
> SONUÇ: 3 BULGU

**(b) Parça 1 (§1–§4 · §5 Task yapısı · F0 · F1):**

> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §5, T07 kabul kriteri 7 / §3.1 faz zinciri
> Alıntı: “koşumu kurulumdadır (T601)”
> Sorun: T07, F0’ın kapanması için gereken T09’un bağımlılığıdır; fakat kendi kabulünün gerçek kurtarma provasını F6’daki T601’e bırakır. §3.1’e göre F6, F0 gate’i geçmeden başlayamaz. Böylece T07’nin kabulü T601’i, T601 ise kapanmış F0’ı bekler: doğrulanamayan faz geçiş döngüsü oluşur. (4)
> Öneri: T07’yi yalnız yedek düzeni ve runbook adımının hazırlanmasıyla sınırlayın; gerçek dış-depo kurtarma provasını, kanıtını ve kabul kriterini tamamen T601’e taşıyın.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §3 F1 çıkış ürünü / §5, T103 ve F1 göç task’ları
> Alıntı: “Göç tek konudur; `Down` ve işlem dışı adım yoktur”
> Sorun: F1 çıkışı göçlerin “geri alınabilir” olmasını şart koşarken, göç task’ları `Down` işlemini sistematik olarak yasaklıyor. Plan, `Down` olmadan geri almanın hangi veri-korumalı mekanizmayla yapılacağını ve bunun hangi task/testte kanıtlanacağını tanımlamıyor. Bu nedenle F1 çıkış ölçütü uygulanamaz; başarısız bir üretim göçünde veri ve sipariş kaybına yol açabilecek belirsiz bir geri dönüş süreci vardır. (2), (4)
> Öneri: Ya her göç için doğrulanabilir geri alma işlemi tanımlayın ya da F1 çıkışını ileri-uyumlu düzeltme göçü ve açıkça tanımlanmış/kurtarma provasıyla sınırlandırın; seçilen yolun T04 rollback testinde somut olarak sınandığını yazın.
>
> SONUÇ: 2 BULGU

**(c) Parça 2 (F2 · F3 T301–T345):**

> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Orta
> Yer: T312, kabul kriteri 8 / doğrulama kontrol listesi KK8
> Alıntı: “Parça 5 MB'ı aşarsa 413, `Content-Range` yoksa ya da toplamı aşarsa 400 alır” / “uymayan başlangıçlı ve toplamı aşan parça 409 alır.”
> Sorun: Aynı “toplamı aşan parça” için kabul kriteri 400, doğrulama ise 409 bekliyor. Parçalı-yükleme istemcisi hata koduna göre sürdürme ya da durma kararı vereceğinden, bu çelişki istemcinin yanlış davranmasına ve yüklemenin tamamlanamamasına yol açar (4).
> Öneri: “Toplamı aşma” ve “alınan bayttan farklı başlangıç” durumlarını açıkça ayırın; her biri için tek HTTP kodu belirleyip T312 ve T548’de aynı sözleşmeyi kullanın.
>
> BULGU-2
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: T330, kabul kriteri 7
> Alıntı: “ETBİS kaydı yapılmaz; kayıt bilgisi isteğe bağlı alandır, satış kapısını tutmaz”
> Sorun: Kendi e-ticaret ortamında tüketiciye sipariş ve sözleşme imkânı veren hizmet sağlayıcının faaliyete başlamadan ETBİS’e kaydı zorunludur. Plan, kaydı isteğe bağlı yaparak satışın kayıtsız açılmasına izin veriyor; bu yürürlükteki yükümlülüğe aykırıdır ve idari yaptırım riski doğurur (1). [Ticaret Bakanlığı ETBİS SSS](https://ticaret.gov.tr/ic-ticaret/sikca-sorulan-sorular/elektronik-ticaret)
> Öneri: ETBİS kaydını uygulama içi isteğe bağlı alan olmaktan çıkarın; doğrulanmış kayıt/bildirim bilgisini yayına alma ve satış kapısı için zorunlu operasyonel ön koşul yapın.
>
> SONUÇ: 2 BULGU

**(d) Parça 3 (F3 T346–T390 · F4):**

> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: T370, Kabul kriteri 3
> Alıntı: “Süre iptalde iptalden, teslimden önceki caymada ve hizmette beyandan, teslimden sonraki caymada malın ulaştığı tarihle beyandan geç olanından başlar”
> Sorun: Teslim sonrası caymada iade süresini malın firmaya fiilen ulaşmasına bağlar. Tüketici malı kargoya zamanında vermiş olsa bile taşıyıcı gecikmesi firmaya ek süre sağlar; bu, iadenin gönderim/teslim kanıtı temelinde başlaması gereken yasal korumayı zedeler ve müşterinin parasını geç almasına yol açar. Ticaret Bakanlığı’nın güncel açıklaması iade süresini malın kargoya teslimiyle ilişkilendirir. [Resmî bilgilendirme](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)
> Öneri: Teslim sonrası caymada süre başlangıcını, müşterinin malı belirtilen ya da kabul edilen taşıyıcıya verdiği an/kanıt tarihi olarak değiştirin; T373’e bu tarih veya sevk kanıtının kaydı, T370 testlerine de taşıyıcı gecikmesi senaryosunu ekleyin.
>
> BULGU-2
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: T368, Kabul kriteri 6
> Alıntı: “Panel fesih satırında ve taahhüt süresi sonrası iptalde kanuni faiz uyarısı gösterir; sistem faiz hesaplamaz.”
> Sorun: Teslim taahhüdü ihlalinde tüketicinin fesih üzerine tüm ödemelerini kanuni faiziyle geri alma hakkı vardır. Plan yalnız uyarı koyup faiz hesaplama, zorunlu manuel tutar girişi, ödeme ve kanıt kaydı tanımlamadığından faizli iadenin hiç yapılmaması riski doğurur. Bu, müşterinin parasal hak kaybına ve mevzuata aykırılığa yol açar. [Ticaret Bakanlığı açıklaması](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)
> Öneri: Fesih iadesinde kanuni faizi hesaplayan ya da doğru faiz tutarı girilmeden iadeyi tamamlatmayan zorunlu bir akış tanımlayın; toplam iade tutarını, faiz oranı/tarih dayanağını ve ödeme kanıtını kaydedip test edin.
>
> SONUÇ: 2 BULGU

**(e) Parça 4 (F5 · F6 · §6):**

> SONUÇ: TEMİZ

**(f) Parça 5 (§7 · §8 · §9):**

> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §7, Aday task tablosu / §7.1.2 / §7.2 — T549
> Alıntı: "| T549 | F5 | … | `04` E-05; `07` UC-04 (K-1437) |"
> Sorun: T549’un kaynak birimi `07 UC-04` olarak tanımlanmış; ancak ileri matriste UC-04 yalnız T503’e bağlı, geri matriste de T549 için UC-04 yoktur. Bu, planın çift yönlü izlenebilirlik güvencesiyle çelişir (4). T549’u uygulayan/doğrulayan ajan ürün adresi uç sözleşmesini zorunlu kaynak olarak görmeyebilir; arşiv ürün davranışı mevcut ürün adresinde eksik kalabilir.
> Öneri: `07 UC-04` satırına T549’u ekleyin ve T549’un §7.2 satırına `07 UC-04`ü ekleyin; ardından iki yönlü matris denetimini yeniden çalıştırın.
>
> SONUÇ: 1 BULGU

## 3. Bağımsız değerlendirme

Ham bulgular numaralandı: tek parça B-1…B-3, parça 1 B-1…B-2, parça 2 B-1…B-2, parça 3 B-1…B-2, parça 5 B-1 (10 ham). İki çift aynı iddiadır (ETBİS: tek parça B-1 + parça 2 B-2 → F5; `Down`: tek parça B-2 + parça 1 B-2 → F6); öteki altı bulgu ayrıdır → 8 ayrı bulgu (F1–F8). Sekiz `[dogrulama]` Opus / high doğrulayıcıya verildi: Yüksek bulguların hepsi tek tek (D1: F1; D3: F3; D5: F5; D6: F6; D7: F7), Orta F2, F4, F8 de birbirinden uzak bölümlerde (F3 yürütme sırası, §7 matrisi, T312) olduğundan tek tek (D2, D4, D8); yeni bilgi sınaması için toplam 8 doğrulayıcı (bekçi sınırı 20). Önceki turlarda reddedilen iddianın dönüşünde (F3, F4, F5, F6, F7, F8) paketle birlikte önceki hükmün rapor satırı verildi (`onceki_hukumler.md`) ve talimata "yeni bilgi yoksa kısa gerekçeli RET" eklendi. Yasal iddia taşıyan F3'te doğrulayıcı resmî metni (RG 23/8/2022, 31932, Mesafeli Sözleşmeler Yönetmeliği m.12/1) yeniden okudu; F5 ve F7'de ikinci modelin getirdiği Ticaret Bakanlığı sayfaları türetilmiş kaynak olduğundan resmî metin ve proje kararlarıyla (`02 §3.1.4`, §12.5, §7.2.3; K-628, K-574) sınandı. Hüküm dosyaları `11_audit_hukum_denetle.py` ile denetlendi: sorun 0.

| # | Bulgu (kaynak) | Karar | Gerekçe | Uygulama |
|---|---|---|---|---|
| F1 | T07 k7 "koşumu kurulumdadır (T601)" → F0 T601'i, T601 kapanmış F0'ı bekler (parça 1 B-1; §5 T07, §3.1) | ❌ RET | Döngü yok: T07 k7 ve KK7 yalnız runbook'taki prova **adımının içeriğini** kabul eder; "koşumu kurulumdadır (T601)" koşumun yerini gösteren işarettir, kabul şartı değildir. F0 kapısı (11:5624) kurtarma provası istemez; T601'in bağımlılığı yoktur (11:5551) ve gerçek provayı kendi k7'sinde koşar (11:5560, 11:5572). Önerilen ayrım (T07'de adım, T601'de koşum ve kanıt) metinde zaten var; `05 §7.6.8`, MK-27 de provayı kurulumda ve güncellemede koşturur. Seviye Yüksek → Düşük. | Yok |
| F2 | T331 k8: ödeme/yasal metin koşullarını sonraki T352/T353/T366 bağlar → sıra bağımlılığı bozulur (tek parça B-3; F3 sırası, T331 k8) | ❌ RET | Bağımlılık yönü ileri ve zincir bozulmuyor: T331 `Bağımlılık: T330, T380` (11:2457), ikisi sırada önce (11:1677); T366 T331'e açıkça bağımlı (11:3299), T352 → T348 → T331, T353 → T352 geçişli olarak T331'den sonra gelir. k8 sonraki task çıktısını kullanmaz: kapıyı kurar, koşulları bağlama işini sonraki task'lara bırakır ve bağlanmamış koşul karşılanmamış sayılır (11:2467), yani T331 tek başına sınanır. İkinci modelin ikinci önerisi metinde zaten var; dayandığı "task başlamadan bütün girdileri hazır olmalı" cümlesi `11`'de yok. Seviye Orta → Düşük. | Yok |
| F3 | T370 k3 iade süresi teslimden sonra "malın ulaştığı tarih"e bağlanmış; taşıyıcıya teslim tarihi olmalı (parça 3 B-1) | ❌ RET (2. kez; ilk RET 2. tur F2) | Resmî metin (RG 23/8/2022, 31932, m.9 ile değişen m.12/1) süreyi ön bilgilendirmede belirtilen taşıyıcıya teslim tarihine, öngörülenin haricinde bir taşıyıcıyla iadede satıcıya ulaşma tarihine bağlar. Firma iade taşıyıcısı belirlemez ve Ön Bilgilendirme Formu bunu yazar (`02 §7.4.2`, §7.4.4; K-293, K-492, K-493), bu yüzden teslim sonrası her iade belirtilmemiş taşıyıcıyla yapılır ve süre ulaşma tarihinden işler (`02 §7.4.1`; K-491, K-554). T370 k3 kaynak cümleyi aynen tüketir; taşıyıcı öncüllü durum T373 k2'de kodludur. İkinci modelin getirdiği Bakanlık sayfası türetilmiş kaynaktır. Seviye Yüksek → Düşük. | Yok |
| F4 | T549 aday kaynağı `07` UC-04, §7.1'de yalnız T503'te; §7.2 T549'da yok (parça 5 B-1; 1. tur F9'un aynısı) | ❌ RET (2. kez; ilk RET 1. tur F9) | UC-04 satırının yalnız T503'te kalması K-1437 (3)'ün açık hükmüdür ("`04:E-05` satırının Task hücresi T503'ten T549'a geçti; UC-04 satırı T503'te kalır", `PRODUCT_DISCOVERY_STATUS` satır 1505, değişmedi); matris 1. turdan beri aynı, iki yön birbirine eşit (11:6084, 11:6325, 11:7719). "Ajan uç sözleşmesini görmeyebilir" kaygısı geçersiz: T549'un `Dokümanlar` alanında `07 UC-04` ve §4.5 var (11:5524) ve k1, k3, k6, k7 `07 §4.5.1`'e bağlı. Seviye Orta → Düşük. | Yok |
| F5 | T330 k7 "ETBİS kaydı yapılmaz … satış kapısını tutmaz" mevzuata aykırı (tek parça B-1 + parça 2 B-2) | ❌ RET (tekrar eden: 2. tur F6, 3. tur F3) | Aynı iddia, aynı gerekçe, üçüncü dönüş. "ETBİS kaydı yapılmaz" **ürünün** kayıt yapmadığını söyler; kayıt firmanın yükümlülüğüdür ve alan satış kapısını ve kurulum listesini tutmaz (`02 §3.1.4`, satır 505: "kayıt satış yapacak her firmanın yükümlülüğüdür ve sonucu firmadadir"; K-628, K-574). Planda karşılığı vardır: T601 k9, ÖK-1…ÖK-12 kurulum rehberi. T330 k7 kaynak cümleyle birebir. İkinci modelin kaynağı yine Bakanlık SSS sayfası (3. turda görülmüştü); `02` son kez #265'te değişti, 2. ve 3. turdan önce. Seviye Yüksek → Düşük. | Yok |
| F6 | F1 kapısı / T04 geri alma sınaması ↔ F1 göç task'larında `Down` yasağı (tek parça B-2 + parça 1 B-2) | ❌ RET (tekrar eden: 2. tur F1, 3. tur F1) | Çelişki yok: projede "geri alma" şemayı `Down` ile geri almak değil **önceki imaja dönmektir**; şema geri alınmaz ve bu yol CI göç adımında sınanır (son yayımlanan sürümün imajı yeni şemaya karşı) — `05` MK-74 (05:1520), `05 §8.7.1`, `09 §10.6` madde 1, `09 §21.4` madde 3; 11:106 F1 çıkışı ve 11:257 T04 KK5 bu kaynakları tüketir. `05` ve `09` ile `11`'in F1/T04/`Down` cümleleri 2. turdan beri değişmedi. Önerinin iki kolu da MK-74 ve `09 §10.6` ile çelişirdi; ikinci kol (genişlet–daralt) zaten seçilmiş yoldur. Seviye Yüksek/Orta → Düşük. | Yok |
| F7 | T368 k6 yalnız kanuni faiz uyarısı gösterir, faiz hesaplamaz; Yönetmelik m.16 (parça 3 B-2) | ❌ RET (tekrar eden: 2. tur F9, 3. tur F5) | Aynı iddia, yeni bilgi yok: `02` ve `06` o turlardan beri değişmedi; T368 k6 kaynak cümlenin aynısıdır — "sistem faizi hesaplamaz; panel uyarı gösterir ve faiz firmanın yükümlülüğüdür" (`02 §7.2.3`, satır 1624; §12.5 tablosu satır 2212). Faiz yükümlülüğü ve hesabı firmadadır; ürün tutarı ödeyen taraf değildir. İkinci modelin Bakanlık sayfası türetilmiş kaynaktır. Seviye Yüksek → Düşük. | Yok |
| F8 | T312 k8 "Content-Range yoksa ya da toplamı aşarsa 400" ↔ KK8 "toplamı aşan parça 409" (parça 2 B-1) | ❌ RET (tekrar eden: 1. tur F3, 3. tur F10) | Metin değişmedi: k8 (11:1987), KK8 (11:1999), `07` UC-83 (07:4886) aynı. k8 toplam aşımını tek başına 400'e bağlar; KK8'in 409 örneği "denetim sırası:" altında iki koşulu birden taşıyan parçayı (uymayan başlangıç **ve** toplam aşımı) sınar; `07`'de sıra denetimi 409'u boy aşımı 400'ünden önce verdiği için o parça 409 alır. İki cümle birbirini tamamlar. Seviye Orta → Düşük. | Yok |

**Tekrar eden RET (turun sonucunu belirlemez):** F5, F6, F7 ve F8 — önceki turlarda en az iki kez gerekçeyle reddedilen iddialar aynı yer ve aynı iddiayla döndü. F3 (2. tur F2) ve F4 (1. tur F9) bir kez reddedilmişti, bu turda ikinci kez aynı gerekçeyle reddedildi; F1 ve F2 yeni iddialardır ve içerikten reddedildi. **Kısmi kalış:** yok.

## 4. Ek bulgular

- **Yok.** Doğrulayıcıların okurken fark ettiği, `11`'i etkileyen ek bir tutarsızlık bildirilmedi. 3. turun EK-1'i (`scripts/doc-scans/11_ozet_ayrinti_grupla.py` satır 36 paket metni) betik `11` olmadığından değişmeden etki yansıtmaya kalır.

## 5. Kullanıcı onay checklist'i

- [ ] F1 — RET (uygulanmadı)
- [ ] F2 — RET (uygulanmadı)
- [ ] F3 — RET, ikinci kez (uygulanmadı)
- [ ] F4 — RET, ikinci kez (uygulanmadı)
- [ ] F5 — RET, tekrar eden (uygulanmadı)
- [ ] F6 — RET, tekrar eden (uygulanmadı)
- [ ] F7 — RET, tekrar eden (uygulanmadı)
- [ ] F8 — RET, tekrar eden (uygulanmadı)

Gösterilmeden kaydedilen ⚠ karar yok; karar gerektiren bulgu yok; çok kritik soru yok. Önerilen `00`/`CLAUDE.md`/`SETUP.md` değişikliği yok.

## Sonuç

**SONUÇ: kabul edilen bulgu yok — 8 ayrı bulgu (10 ham), sekizi RET; şartname çifti 0.** Skill'in çıkış kuralı: bütün çağrılar aynı anda TEMİZ dönmedi (altı çağrının beşi bulgu getirdi, parça 4 TEMİZ); ancak (a) Faz 4 madde 5'in birinci kuralı gereği önceki turlarda en az iki kez reddedilmiş dört iddia (F5, F6, F7, F8) "tekrar eden RET" diye kaydedildi ve turun sonucunu belirlemez; (b) kalan dört bulgunun hiçbiri kabul edilmedi (F1, F2 yeni iddialar olarak içerikten, F3 ve F4 ikinci kez reddedildi). Mevzuat, para ya da hak kaybı sınıfından kabul yok; 11'in içeriğinde değişiklik yok. Kabuller yine aynı sınıfa düşmedi (kabul 0). Döngü ölçüsü: kabul sıfır olduğundan **tur 4 bulgusuz kapandı**; sıradaki adım **etki yansıtma** (Faz 5). Çıkış kuralı (madde 5, 5. tur) uygulanmadı — gerekmedi.

Sürüm: `11` v0.41 → **v0.42** (içerik değişmeden, tur kaydı); karar kaydı v0.268 → v0.269; K-1482.
