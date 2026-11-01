# Cross-Review — 11 Implementation Planı (Tur 10)

**Tarih:** 2026-11-01 · **Hedef:** `Docs/11_IMPLEMENTATION_PLAN.md` v0.49 (girdi commit'i `9b37dd6`, 762 460 bayt tek parça girdi, 7 760 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma); betiğin `status.txt` satırı: `model=codex gpt-5.6-terra high` · **Ciddiyet ölçüsü:** 1. turdan itibaren, mekanizma okuma paragrafıyla (K-1399; `11_CROSS_REVIEW.md` §1) · **Koşum:** iki koşum — tek parça çağrı ve beş parçalı kontrol koşumu · **Sonuç:** **TEMİZ DEĞİL** — 9 ham bulgu, 8 ayrı bulgu (tek parça B-1 ile parça 1 B-1 aynı iddia); 0 doğru · 3 kısmen · 5 çürük (üçü önceki turlarda reddedilmiş iddianın dönüşü); 5 şartname çifti uygulandı (T103 KK6; T514 k10 ve KK10; T504 k4 ve KK4); ayrıca 8. turdan kalan T370 KK7 "IBAN silinir" eki tamamlandı.

## 1. Koşum

Betikler, istem, parça notu, şablon ve **parça sınırları 1. turdakinin aynısıdır** (`11_CROSS_REVIEW.md` §1; K-1478 — sınır başlıkla tanımlıdır, değişmez). İstem tek karakter değiştirilmeden kullanıldı. Girdi `build_inputs.py` ile kuruldu (`uncovered lines: none`). **Bekleme ve yeniden koşum:** ilk deneme (aynı gün) kullanım sınırına takıldı; betik çıkış 1 verdi (`model=none`: Codex sınırlı, cursor sağlık çağrısını geçmedi), çağrıların bir kısmı kısa ve eksik çıktı döndürdüğü için hiçbiri kullanılmadı (kısmi çıktılar `kismi-codex-1/` altında kaldı, rapora girmedi); sınır 03:06'da açıldıktan sonra altı çağrının **tamamı aynı girdilerle baştan** koşuldu (tur bölünmedi, yedek modele geçilmedi). İkinci model çağrısı 6.–9. turdaki gibi tek betikle yapıldı (K-1484): `bash scripts/doc-scans/cross_review_ikinci_model.sh in out c1 p1 p2 p3 p4 p5`; betik asıl modeli (Codex `gpt-5.6-terra`, yüksek) seçti (`used: codex gpt-5.6-terra`), altı çağrının hepsi `rc=0` döndü (çıkış kodu 0; çıktı dosyaları ayrıca okundu: beşi bulgu içeriyor, parça 5 `SONUÇ: TEMİZ`). Web araması: parça 2, 3, 4 ve 5 çağrıları web araması yaptı; bulgu kaynakları: F4 ve F6 Ticaret Bakanlığı sayfası ve 6502 sayılı Kanun PDF'i (resmî metin değil, derleme; Faz 2'de Resmî Gazete/mevzuat.gov.tr metnine karşı okundu).

| Çağrı | Bölümler | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|
| Tek parça | tüm doküman | 762 460 bayt | 302 285 | 179 sn | 1 bulgu |
| Parça 1 | §1–§4 · §5 Task yapısı · F0 · F1 (satır 65–866) | 97 439 bayt | 52 133 | 162 sn | 2 bulgu |
| Parça 2 | F2 · F3 T301–T345 (867–2841) | 197 923 bayt | 115 595 | 246 sn | 1 bulgu |
| Parça 3 | F3 T346–T390 · F4 (2842–4219) | 151 797 bayt | 113 113 | 272 sn | 3 bulgu |
| Parça 4 | F5 · F6 · §6 (4220–5633) | 149 683 bayt | 96 100 | 79 sn | 2 bulgu |
| Parça 5 | §7 · §8 · §9 (5634–dosya sonu) | 254 332 bayt | 129 095 | 71 sn | TEMİZ |

Parça 5 TEMİZ döndü; öteki beş çağrı bulgu getirdi. İki koşum arasında bir iddia çıktı: tek parça B-1 ile parça 1 B-1 aynı bulgudur (F1: F1 çıkışı "geri alınabilir" ↔ `Down` yasağı); dokuz ham bulgu sekiz ayrı bulgudur. 9. turun kabul ettiği iki yer (T07 KK6, T336 KK8) ve hedefli mercek 2'nin değiştirdiği bloklardan yalnız T103 (F2) ve T370 (F4, F5) bu turda anıldı.

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**(a) Tek parça çağrı:**

> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §3 F1 çıkış ürünü; §6.2 F1 gate’i; T103 ve F1 göç task’ları; T602 kriter 7
> Alıntı: “Göçler ileri, idempotent ve geri alınabilir”; “Göç tek konudur; `Down` ve işlem dışı adım yoktur”; “Geri alma önceki imaja dönmektir; şema geri alınmaz”
> Sorun: F1 kapısı göçlerin geri alınabilir olmasını ve CI’ın geri alma sınamasını zorunlu kılıyor; F1 task’ları `Down` işlemini yasaklıyor, T602 ise şema geri alınmaz diyor. Bu, F1 gate’inin tanımlı task çıktılarıyla geçilememesine ve göç geri alma kanıtının neyi sınadığının belirsiz kalmasına yol açan doküman içi çelişkidir (4).
> Öneri: Seçilen ileriye-dönük göç yaklaşımını koruyarak F1 çıkışı, F1 gate’i ve T04’teki “geri alma” ifadelerini şema geri alma yerine yedekten dönüş / önceki imajla uyumluluk doğrulaması olarak hizalayın; ya da gerçekten geri alınabilir göç isteniyorsa `Down` yasağını kaldırıp ilgili task ve testleri güncelleyin.
>
> SONUÇ: 1 BULGU

**(b) Parça 1:**

> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §3 F1 çıkış ürünü; T04 kabul kriteri 5; T103–T120 göç kriterleri
> Alıntı: “Göçler ileri, idempotent ve geri alınabilir” / “geri alma sınamasını koşar” / “`Down` … yoktur”
> Sorun: F1 kapısı ve T04, göçlerin geri alınabilmesini ve bunun sınanmasını zorunlu tutarken F1’deki her veri göçü `Down` içermeyecek şekilde tanımlanmıştır. Alternatif bir geri alma mekanizması da tanımlı değildir. Bu, faz kapısının sağlanamayacağı veya sözde çalıştığı hâlde geri alma güvencesinin uygulanmayacağı bir iç çelişkidir (4).
> Öneri: Tek bir geri alma yaklaşımı belirleyin: Her göç için sınanabilir geri alma işlemi tanımlayın; veya ileri-yönlü düzeltme modeli seçilmişse F1 çıkış ölçütü ile T04’teki “geri alma sınaması”nı bu modele uygun, açık bir sürüm-uyumluluk/geri dönüş prosedürüyle değiştirin.
>
> BULGU-2
> Kriter: Teknik doğruluk
> Seviye: Orta
> Yer: T103 kabul kriteri 6; T103 doğrulama kontrol listesi KK6
> Alıntı: “yayımlanmış sürüm satırı uygulamada güncellenmez ve silinmez” / “KK6: il silme, bağlı satır varken reddedilir.”
> Sorun: Yayımlanmış `LegalTextVersion` kaydının uygulama üzerinden değiştirilememesi veya silinememesi kabul kriteridir; ancak buna ilişkin hiçbir doğrulama maddesi ya da test beklentisi yoktur. KK6 yalnız il silinmesini denetler. Böylece task, kendi kabul kriterinin bu kısmı uygulanmadan geçebilir; yayımlanmış yasal metin sürümü değiştirilebilir hâlde kalabilir (4).
> Öneri: T103’e, yayımlanmış bir `LegalTextVersion` için uygulama/veri erişim katmanı üzerinden güncelleme ve silme denemelerinin reddedildiğini doğrulayan açık Integration testi ve kontrol-listesi maddesi ekleyin.
>
> SONUÇ: 2 BULGU

**(c) Parça 2:**

> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: T204 madde 4; T304 madde 6 ve 9
> Alıntı: “alıcı hatası da kanal hatası da ilk denemeden 5 dk, 30 dk ve 2 saat sonraki deneme anlarını kurar”; “Aşan satır ilk denemede `Undelivered` ve `RecipientError` olur; servise çağrı yapılmaz”; “Sınır ilk denemede bir kez sorulur, yeniden denemelerde sorulmaz”
> Sorun: Kota aşımı tam olarak `RecipientError` olarak sınıflanıyor. T204 bu sınıftaki her hatayı yeniden denemeye zorunlu kılıyor; T304 ise yeniden denemede kota kontrolünü kaldırıyor. Böylece kota nedeniyle gönderilmeyen ileti sonraki denemede sağlayıcıya gönderilebilir ve alıcı sınırı aşılır. Gönderilmezse de T204’ün kabul kriteri bozulur. Bu, test beklentileri birlikte sağlanamayan bir plan içi çelişkidir (4).
> Öneri: Kota aşımını `RecipientError` yerine ayrı, terminal ve yeniden-denenmez bir sonuç olarak tanımlayın; T204’te bu sonuç için açık istisna, T304’te de sağlayıcı çağrısı yapılmayacağını tüm denemeler bakımından belirten kabul ve test kriterleri ekleyin.
>
> SONUÇ: 1 BULGU

**(d) Parça 3:**

> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: T370, kabul kriteri 3
> Alıntı: “teslimden sonraki caymada malın ulaştığı tarihle beyandan geç olanından başlar”
> Sorun: Cayma iadesi süresini malın fiilen firmaya ulaşmasına bağlamak, tüketici malı kargoya zamanında teslim ettiği hâlde taşıma gecikmesi yüzünden iadenin gecikmesine yol açar. Firma iade taşıyıcısı belirlemediği için müşteri herhangi bir taşıyıcı seçebilmektedir. Bu, caymada iade süresinin malın kargoya teslimiyle başlaması kuralıyla çelişir ve tüketicinin parasal hakkını geciktirir. [Ticaret Bakanlığı açıklaması](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)
> Öneri: Fiziksel caymada süre başlangıcını, müşteri malı taşıyıcıya teslim ettiğinde doğrulanabilir gönderim kanıtının oluştuğu an yapın; yalnız mevzuatın açıkça izin verdiği istisnalarda fiilî ulaşma anını kullanın.
>
> BULGU-2
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: T370, kabul kriteri 9; T405, kabul kriteri 7
> Alıntı: “Sonuç beklenirken yeniden deneme ve havale yolu kapalı”
> Sorun: Kart iadesinin sonucu bilinmediğinde alternatif ödeme yolu kapatılıyor; buna karşılık iade sorgusu “üst sınırı yoktur” denilerek süresiz sürüyor. Bu akış, sonucun hiç kesinleşmemesi hâlinde tüketiciye yapılması gereken iadenin yasal süresi içinde tamamlanmasını sağlayan hiçbir çıkış bırakmıyor. Bu, müşterinin para hakkının belirsiz süreyle askıda kalmasına neden olur.
> Öneri: Sonucu bilinmeyen iadeler için yasal iade süresi dolmadan işletilecek, çift iade riskini denetleyen zorunlu uzlaştırma/çözüm yolunu tanımlayın; farklı ödeme yoluna geçilecekse tüketicinin açık onayını ve sonuçta tek ödeme güvencesini kabul kriteri ile testlere ekleyin.
>
> BULGU-3
> Kriter: Eksiklik
> Seviye: Orta
> Yer: T368, kabul kriteri 6
> Alıntı: “Panel fesih satırında ve taahhüt süresi sonrası iptalde kanuni faiz uyarısı gösterir; sistem faiz hesaplamaz”
> Sorun: Teslim/ifa yükümlülüğünün yerine getirilmemesi nedeniyle fesihte tahsil edilen bedellerin yasal faizle iadesi zorunludur. Plan yalnız uyarı üretip faizin hesaplanması ve iade tutarına eklenmesi için hiçbir işlem tanımlamıyor; bu da eksik iade ve mevzuata aykırılık riski doğurur. [Mesafeli Sözleşmeler Yönetmeliği m.16](https://tuketici.ticaret.gov.tr/data/5e81982d13b876a1b04c7a42/2023-6502%20Say%C4%B1l%C4%B1%20T%C3%BCketicinin%20Korunmas%C4%B1%20Hakk%C4%B1nda%20Kanun.pdf)
> Öneri: Fesih iadesinde yasal faizin hesaplanıp iade tutarına eklenmesini veya ürün dışı zorunlu ödeme sürecinde hesaplanıp ödendiğinin denetlenmesini açık kabul kriteri ve testle güvenceye alın.
>
> SONUÇ: 3 BULGU

**(e) Parça 4:**

> BULGU-1
> Kriter: Kullanıcı deneyimi
> Seviye: Yüksek
> Yer: T514, kabul kriterleri 5 ve 10; T516, kabul kriteri 3
> Alıntı: “yeni adres başkasına geçtiyse 422 ve tüketim geri döner”
> Sorun: Bu yarış durumunda doğrulanmamış kayıt oturum açamaz; aynı adresle yeniden kayıt yasaktır; bekleyen kayda şifre sıfırlama e-postası da gönderilmez. E-posta artık başka hesaba ait olduğundan doğrulama bağlantısını yeniden yollamak da çözüm değildir. Kullanıcının kayıt sürecinden çıkışı yoktur. Bu, çıkışı olmayan akış sınıfındadır (3).
> Öneri: Bu durumda bekleyen kaydı atomik olarak iptal edip nötr bir “kayıt sürecini yeniden başlatın” akışı açın veya doğrulanmamış kaydın e-postasını değiştirebileceği güvenli bir akış tanımlayın.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: T504 kabul kriteri 4; T540 kabul kriteri 8; T510 kabul kriteri 7
> Alıntı: “Beklenmeyen hata GET'te vitrinde vitrinin, panelde panelin çerçevesinde 500 hata sayfasıdır”
> Sorun: Kural bütün panel GET’lerini panel çerçevesine bağlarken T540 panel girişi ve davet kabulünü çerçevesiz tanımlar. Ayrıca T510 ödeme dönüş sayfasını çerçevesiz zorunlu kılar. Bu uçlarda oluşan 500 yanıtının hangi çerçeveyi kullanacağı çelişkilidir; plan aynı davranış için iki farklı zorunluluk koymaktadır (4).
> Öneri: T504’e öncelik kuralı ekleyin: giriş, davet, ödeme dönüşü ve diğer çerçevesiz iniş uçlarında 500 yanıtı da çerçevesiz olsun; yalnız yetkili `/panel` ekranlarında panel çerçevesi kullanılsın. Bunu doğrulama listesine ekleyin.
>
> SONUÇ: 2 BULGU

**(f) Parça 5:**

> SONUÇ: TEMİZ

## 3. Bağımsız değerlendirme

Ham bulgular numaralandı: tek parça B-1 (F1, parça 1 B-1 ile aynı), parça 1 B-1…B-2 (F1–F2), parça 2 B-1 (F3), parça 3 B-1…B-3 (F4–F6), parça 4 B-1…B-2 (F7–F8), parça 5 TEMİZ (9 ham, 8 ayrı bulgu; sıra: doğrulayıcı sırası). Sekiz `[dogrulama]` Opus / high doğrulayıcıya verildi, her biri tek bulgu (D1…D8 = F1…F8: Yüksek bulgular F1, F3, F4, F5, F6, F7 tek; Orta F2, F8 tek — hiçbiri gruplanmadı); toplam 8 doğrulayıcı (bekçi sınırı 20; bekçi olayı yok). Önceki turlarda reddedilen iddianın olabileceği bulgularda (F1, F4, F6 ve F2, F3, F5, F7, F8 için de ilgili önceki hükümler) paketle birlikte önceki hükmün ilgili bölümleri verildi ve talimata "yeni bilgi yoksa kısa gerekçeyle tekrar eden RET" kuralı yazıldı. Yasal iddia taşıyan F4, F5, F6 resmî metne (Mesafeli Sözleşmeler Yönetmeliği, RG 27/11/2014-29188, değişik 23/8/2022-31932; mevzuat.gov.tr) karşı okundu.

| # | Bulgu (kaynak) | Karar | Gerekçe | Uygulama |
|---|---|---|---|---|
| F1 | F1 çıkışı ve T04 k5 "göçler geri alınabilir, geri alma sınaması" ↔ T103–T120 `Down` yasağı ↔ T602 "şema geri alınmaz" (tek parça B-1, parça 1 B-1; F1, T04, T103, T602) | ❌ RET (tekrar eden: 2. tur F1, 3. tur F1–F2, 4. tur F1, 6. tur #1–#2, 7. tur #1, 8. tur #6, 9. tur #3; Yüksek → Düşük) | Yeni bilgi yok. `05` ve `09` son değişikliği #266'dır; 9. turdan sonra 11'de geri alma ve `Down` cümlelerinden yalnız T09 k2 (11:372) değişti ve değişiklik mekanizmayı güçlendirir (yayından sonraki ilk CI koşusunda geri alma sınaması koşar). Projede "geri alma" şemayı `Down` ile geri almak değil önceki imaja dönmektir ve CI göç adımında sınanır: `05` §8.7.3 MK-74 (05:1132), `09` §10.6 madde 1 (09:1031); 11:257 (T04 KK5) ve 11:5625 bunu kurar. `Down` yasağı aynı kuralın parçasıdır; çelişki yok. | Yok |
| F2 | T103 k6 "yayımlanmış sürüm satırı uygulamada güncellenmez ve silinmez" ↔ KK6 yalnız il silme (parça 1 B-2; T103) | KISMİ (Orta → Düşük) | Olgu doğru: k6 (11:468) iki sonuç ister, KK6 (11:477) yalnız il silmeyi sınar; 11 §5 konvansiyon 7 (11:168) "kabul kriteri başına bir madde" ister ve kardeş şemalar aynı uygulama düzeyi kuralını KK'ya bağlar (T108 k6 → 11:588, T110 k6 → 11:635); T103 bu kalıptan sapar (4). Kural kaynakta veritabanı kısıtı değil uygulama kuralıdır (`06` §2.2.3, 06:4348), bu yüzden önerinin "reddedildiğini doğrulayan Integration testi" kolu daraltıldı: kardeş task'lardaki "güncelleme ve silme yolu kodda yoktur" biçimi yeter. Seviye Düşük: `validate` Faz 1 adım 5 her kabul kriterini KK'dan bağımsız kanıtla sınar. Blok 2 372 bayt. | T103 KK6 (1 şartname) |
| F3 | T204 k4 alıcı hatası da kanal hatası da yeniden denenir ↔ T304 k6/k9 kota aşımı `RecipientError`, yeniden denemede sınır sorulmaz (parça 2 B-1; T204, T304) | ❌ RET (Yüksek → Düşük) | Alıntı T204 k4'ü yarıda keser; cümlenin devamı "yeniden denenmeyen sınır aşımı T385'tedir" der (11:956), yani sınır aşımı açık bir istisnadır; T385 k4 (11:3810) "yeniden denenmeden" der. Kaynakta aşan satır ilk denemede `Undelivered` olur ve servise çağrı yapılmaz (08:452); `Undelivered` son haldir (08:410); "yeniden denemelerde sınır sorulmaz" (08:456) yalnız sınırdan geçip başka nedenle başarısız olan satırlar içindir. Aşan satır yeniden deneme kuyruğuna hiç girmez, sağlayıcıya gidemez. Yakın bir hal hedefli mercek 2'de kabul edilip KK6'ya işlenmişti (11:1777). | Yok |
| F4 | T370 k3 teslimden sonraki caymada iade süresi malın ulaştığı tarihten başlar (parça 3 B-1; T370) | ❌ RET (tekrar eden: 2. tur D2 #2, 4. tur D3 #1, 5. tur D6 #6; Yüksek → Düşük) | Yeni bilgi yok. Mesafeli Sözleşmeler Yönetmeliği m.12/1 resmî metni Resmî Gazete'den (23/8/2022, 31932) bu turda yeniden okundu: mal iade için belirtilen taşıyıcıya verilirse süre teslim tarihinden, başka taşıyıcıyla gönderilirse malın satıcıya ulaştığı tarihten başlar. Firma iade taşıyıcısı belirlemez ve Ön Bilgilendirme Formu da bunu yazar (`02` §7.4.2, §7.4.4; K-293, K-492, K-493); bu yüzden teslimden sonraki her iadede süre ulaşma tarihinden başlar (`02` §7.4.1; K-491, K-554). Bulgunun "müşteri herhangi bir taşıyıcı seçebilir" öncülü ulaşma tarihi kuralını tetikleyen durumdur; ikinci modelin dayandığı sayfa resmî metin değil, derlemedir. T370 k3 (11:3406) kaynak cümleyi aynen uygular; T373 k2 (11:3485) tutarlı. | Yok |
| F5 | T370 k9 / T405 k7 sonucu bilinmeyen kart iadesi sorgusunun üst sınırı yok, beklerken yeniden deneme ve havale yolu kapalı (parça 3 B-2; T370, T405) | ❌ RET (Yüksek → Düşük) | Çıkışsızlık yok: sonuç beklenirken firma sağlayıcının panelinde bakıp iadeyi "gerçekleşmedi" diye işaretleyebilir; işaret sorguyu durdurur, yeniden deneme ve havale yolunu açar (`02` §7.2.8, K-940; `05` §4.6.2; 08:341; 11'de T529 k10 `MarkCardRefundFailed`, T405 k8/KK8, T112 k4). Sorgunun üst sınırının olmaması bilinçli karardır (K-1238 (3)); havaleye geçiş müşterinin IBAN'ı girmesiyle olur (K-569), işaretten sonra ilk isteğin geç gerçekleşmesi kabul edilmiş kalan risktir ve firmaya aittir (K-978, K-1238 (6)). Yönetmelik m.12/1–4 resmî metni (mevzuat.gov.tr) ödeme aracına uygun tek seferde iadeyi satıcıya yükler, üründe zorunlu uzlaştırma mekanizması istemez. 9. turun F15'i başka bir konuydu (T512 IBAN alanı). | Yok |
| F6 | T368 k6 "sistem faiz hesaplamaz": yasal faizli iade (parça 3 B-3; T368) | ❌ RET (tekrar eden: 2. tur F9, 3. tur F5, 4. tur F1, 6. tur #6, 7. tur #7, 9. tur #12; Yüksek → Düşük; Orta → Düşük) | Yeni bilgi yok; `02`, `04`, `06`, `07` son değişikliği #266'dır. T368 k6 (11:3359) `02` §7.2.3'ün (02:1624) aynısıdır: "sistem faizi hesaplamaz … faiz firmanın yükümlülüğüdür"; `02` §12.5 tablosu faizi hesaplayıp ödemeyi firmaya yazar (02:2212; K-499, K-571). Önerinin iki kolu da üst dokümanla kapalıdır: `06` "faiz alanı yok" (06:1735); `04` (04:4905) ve `07` (07:1399) gelen tutar alanını panelde olmayanlar arasında sayar. m.16/3 faizli iadeyi satıcıya (firmaya) yükler, ürün araçtır. İkinci modelin kaynağı 6502 sayılı Kanun'un PDF'idir ve gösterdiği "m.16" atfı o metne ait değildir. | Yok |
| F7 | T514 k5/k10 ve T516 k3: yeni e-posta adresi başkasına geçtiyse 422 ve tüketim geri döner; doğrulanmamış kayıt için çıkış yok (parça 4 B-1; T514, T516) | KISMİ (Yüksek → Orta) | Çıkışsızlık yok: UC-53'ün 422'si yalnız yeni adresin doğrulamasında çıkar (07:4607); 422'yi alan müşterinin doğrulanmış hesabı eski adresiyle çalışmaya devam eder (07:4607; 11:4662). Bekleyen kayıt da takılı kalmaz: adres başka hesaba doğrulanarak geçerse onu tutan bekleyen kayıt aynı işlemde silinir (07:4608; K-1080, 02:703), Google girişinde de silinir (11:2770), aksi hâlde Z-1'de silinir ve "bağlantı geçersiz, yeniden kayıt olun" (02:685); T516 k3 `07` UC-57 ile uyumlu. Ama önerinin ilk yarısı (bekleyen kaydı atomik silme) üst dokümanda K-1080 kuralıdır ve 11'de UC-53 yolu için hiçbir kriterde yok: T514 k9 yalnız kayıt dalını, k10 yalnız 422'yi taşır, T343 k7 silmeyi anmaz; aynı kural UC-76 için T519 k6'da kriterdir (11:4717) — asimetri (4). Silme atlanırsa adresin sahibi başkasının açtığı bekleyen kaydı doğrulayabilir ya da aynı e-posta iki kayıtta kalır. T514 bloğu 2 557 bayt (sınır 2 560); T343 bloğu dolu olduğu için eklenmedi. | T514 k10 ve KK10 (2 şartname) |
| F8 | T504 k4 panel GET 500'ü panel çerçevesinde ↔ T540 k8 panel girişi ve davet kabulü çerçevesiz ↔ T510 k7 ödeme dönüşü çerçevesiz (parça 4 B-2; T504, T540, T510) | KISMİ (Orta → Düşük) | T504 k4 (11:4305) `04` 2.7.3.2'yi (04:2555; 05:1321; 07 §4.34.2) aynen taşır ve panel GET'inin 500'ünü panel çerçevesine bağlar; `04` 2.2.9 (04:2431) ise E-29, E-30 ve E-54'ün bağlantıyla açılan adımlarını üst satır ve menü taşımayan panel ekranları olarak ayrı tanımlar, T540 k8 (11:5293) giriş ve davet kabulünü çerçevesiz sayar; `PanelFrame` yönetici oturum verisi istediği için bu ekranlarda kurulamaz, E-54 için `04` 9.25.11 hata sayfasını açıkça ister — iki kural aynı yanıt için bağdaşmaz (4). Daraltma: T510 k7 kolu ölçü dışı (UC-30 durum yazmayan geçiş sayfasıdır, K-1259 (1); 500'ü para ya da hak kaybı doğurmaz, üst dokümanda çerçeve kuralı yoktur); önerinin "öteki çerçevesiz iniş uçları" listesi 11'de yeniden kurulmaz, `04` 2.2.9'a işaret edilir. Blok 2 415 bayt. | T504 k4 ve KK4 (2 şartname) |

**Tekrar eden RET (turun sonucunu belirlemez):** F1 (2., 3., 4., 6., 7., 8., 9. tur), F4 (2., 4., 5. tur), F6 (2., 3., 4., 6., 7., 9. tur). **Seviye düşenler:** Yüksek → Düşük: F1, F3, F4, F5, F6; Yüksek → Orta: F7; Orta → Düşük: F2, F8. **Kısmi kalış:** F2 (öneri daraltıldı: Integration testi alınmadı, kardeş task'lardaki "yolu kodda yoktur" biçimi), F7 (çıkışsızlık iddiası çürük, yalnız bekleyen kaydın silinmesinin kriterde olmayışı kabul), F8 (T510 k7 kolu ölçü dışı).

**Çıkış değerlendirmesi (Faz 4 madde 4–5).** Sonuç TEMİZ değil: tek parça ve parça 1–4 bulgu getirdi (parça 5 TEMİZ). Madde 5'in çıkış kuralı işlemez: F7 Orta şiddettedir (üst dokümandaki bir silme kuralı 11'de UC-53 yolunda kriter olmamıştır; salt iki yerin metin farkı değil) ve kural yalnız kabullerin hepsi düşük metin farkıysa kapatır. Tekrar eden RET kuralı F1, F4, F6'yı turun sonucundan ayırır. **Gözlem (Faz 4 madde 6):** F2 (T103 k6 ↔ KK6) hedefli mercek 2'nin sınıfındadır (kriter bir davranış istiyor, KK sınamıyor) ve Düşük şiddettedir; F7 ve F8 başka sınıflardır (üst dokümanın kuralının kriterde bulunmayışı; iki task'ın aynı yanıt için çelişen çerçeve kuralı). Orta kabul tek (F7) ve ardışık iki turda aynı sınıftan Orta kabul yok; hedefli mercek gerekmez. Sıradaki adım **cross-review 11. tur**.

**F2, F7 ve F8 — uygulama.** Beş çift `11_audit_uygula.py` ile uygulandı: T103 KK6'ya "yayımlanmış `LegalTextVersion` satırına güncelleme ve silme yolu kodda yoktur" eklendi (blok 2 372 bayt); T514 k10 "yeni adres başkasınınsa 422, tüketim döner, değilse onu tutan bekleyen kayıt silinir (K-1080)" ve KK10 "yarış; başka hesap; kayıt silme" oldu (blok 2 557 bayt); T504 k4 "çerçevesiz panel ekranlarında hata sayfası da çerçevesizdir (`04` 2.7.3.2, 2.2.9)" ve KK4 "panel girişinde 500 çerçevesiz" aldı (blok 2 415 bayt).

**Ek iş — T370 KK7 ("IBAN silinir").** 8. turda (K-1487) T370 ve T390 elle kısaltılırken doğrulanmış şartnamedeki "IBAN silinir" eki T370 KK7'ye konmamıştı (şartname: `kd-cross/r8/hukum_D8.md`, #10, "KK7: havale hatasında IBAN silinir, alan yeniden açılır, B-14; süre başlangıcı değişmez"). T370 k7 "IBAN'ı siler" der, KK7 silmeyi sınamıyordu. KK7 şimdi "havale hatasında IBAN silinir, alan açılır, B-14; süre durmaz" der (+14 bayt). T370 bloğu sınırı aştı (2 573 bayt) ve anlam kaybı olmadan iki yerde kısaltıldı: k8 "IBAN girildikten sonra deneme kapalıdır" → "IBAN girilince deneme kapalıdır", k9 "sonuç beklenirken yeniden deneme ve havale yolu kapalı" → "bu sürede yeniden deneme ve havale yolu kapalı" (yalnız sözcük kısaltması, kural aynı; kayıp yok). Blok 2 556 bayt (sınır 2 560); bölme gerekmedi.

Yazım kapanışı, matris kapsama, task blok denetimi (`--tam`), geri yön denetimi ve geri işaret taraması sorun 0.

## 4. Ek bulgular

- D8 doğrulayıcısı bir **üst doküman** uyumsuzluğu bildirdi (uygulanmadı; 11 tüketicidir, üst doküman bu turda değişmez): `04` 2.7.3.2 "panelde panelin çerçevesi içinde bir hata sayfası" cümlesi çerçevesiz panel ekranları (`04` 2.2.9: E-29, E-30, E-54) için istisna içermiyor; aynı hizalama `05` §10.3.2 ve `07` §4.34.2'de de gerekir. Önerilen düzeltme: "…panelde panelin çerçevesi içinde — çerçevesi olmayan panel ekranlarında (2.2.9) çerçevesiz, yalnız panel başlığıyla — bir hata sayfası". KD-etki'de (cross-review Faz 5) karşılanır; karar kaydında K-1491 etki sütununa bağlandı.
- Doğrulayıcılardan başka tutarsızlık bildirilmedi. `00`/`CLAUDE.md`/`SETUP.md` önerisi yok.

## 5. Kullanıcı onay checklist'i

- [ ] F1 — RET, tekrar eden (uygulanmadı)
- [ ] F2 — KISMİ (uygulandı: T103 KK6)
- [ ] F3 — RET (uygulanmadı)
- [ ] F4 — RET, tekrar eden (uygulanmadı)
- [ ] F5 — RET (uygulanmadı)
- [ ] F6 — RET, tekrar eden (uygulanmadı)
- [ ] F7 — KISMİ (uygulandı: T514 k10, KK10)
- [ ] F8 — KISMİ (uygulandı: T504 k4, KK4)
- [ ] Ek iş — T370 KK7 "IBAN silinir" (uygulandı)

⚠ karar: yok. Önerilen `00`/`CLAUDE.md`/`SETUP.md` değişikliği yok.
