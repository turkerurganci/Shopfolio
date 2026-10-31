# Cross-Review — 11 Implementation Planı (Tur 8)

**Tarih:** 2026-10-31 · **Hedef:** `Docs/11_IMPLEMENTATION_PLAN.md` v0.45 (girdi commit'i `71e9c53`, 760 020 bayt girdi, 7 761 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma); betiğin `status.txt` satırı: `model=codex gpt-5.6-terra high` · **Ciddiyet ölçüsü:** 1. turdan itibaren, mekanizma okuma paragrafıyla (K-1399; `11_CROSS_REVIEW.md` §1) · **Koşum:** iki koşum — tek parça çağrı ve beş parçalı kontrol koşumu · **Sonuç:** **TEMİZ DEĞİL** — 12 ham bulgu, 11 ayrı bulgu; 0 doğru · 3 kısmen · 8 çürük (beşi önceki turlarda reddedilmiş iddianın dönüşü); 6 şartname çifti uygulandı (T118 KK1, T224 k2 ve KK2, T370 KK6 ve KK7, T390 KK8; v0.45 → v0.46); üç kabul de Orta (kabul kriteri ↔ doğrulama kontrol listesi: kriterin sınanmadığı ya da KK'nin kriteri izlemediği üç yer), Faz 4 madde 5 çıkış kuralı uygulanamaz; tur 9 gerekir.

## 1. Koşum

Betikler, istem, parça notu, şablon ve **parça sınırları 1. turdakinin aynısıdır** (`11_CROSS_REVIEW.md` §1; K-1478 — sınır başlıkla tanımlıdır, değişmez). İstem tek karakter değiştirilmeden kullanıldı. Girdi `build_inputs.py` ile kuruldu (`uncovered lines: none`). İkinci model çağrısı 6. ve 7. turdaki gibi tek betikle yapıldı (K-1484): `bash scripts/doc-scans/cross_review_ikinci_model.sh in out c1 p1 p2 p3 p4 p5`; betik asıl modeli (Codex `gpt-5.6-terra`, yüksek) sağlık çağrısıyla seçti (`used: codex gpt-5.6-terra`), altı çağrının hepsi aynı modelle, tek komutta, ilk denemede `rc=0` döndü (çıkış kodu 0; çıktı dosyaları ayrıca okundu). Yedek model (Cursor) devreye girmedi, tur bölünmedi, kullanım sınırı dolmadı. Çalışma dizini: `C:\Users\teknodeveloper\.claude\helper-state\shopfolio\kd-cross\r8\` (girdiler `in/`, ham çıktılar `out/`, doğrulayıcı paketleri `dogrulayici_<k>.md`, hükümler `hukum_D<k>.md`, talimat `talimat_dogrulayici.md`; paket betiği `betik/paket8.py`).

| Çağrı | Bölümler | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|
| Tek parça | tüm doküman | 760 020 bayt | 415 416 | 239 sn | 3 bulgu |
| Parça 1 | §1–§4 · §5 Task yapısı · F0 · F1 (satır 65–866) | 96 620 bayt | 52 670 | 167 sn | 3 bulgu |
| Parça 2 | F2 · F3 T301–T345 (867–2841) | 197 174 bayt | 114 504 | 201 sn | 3 bulgu |
| Parça 3 | F3 T346–T390 · F4 (2842–4219) | 150 958 bayt | 82 343 | 305 sn | 2 bulgu |
| Parça 4 | F5 · F6 · §6 (4220–5633) | 149 650 bayt | 95 302 | 84 sn | 1 bulgu |
| Parça 5 | §7 · §8 · §9 (5634–dosya sonu) | 254 332 bayt | 119 798 | 94 sn | TEMİZ |

Parça 5 TEMİZ döndü; öteki beş çağrı bulgu getirdi. İki koşum bu turda da sürdü; iki koşum arasındaki örtüşme: tek parça B-2 = parça 1 B-2 (T117 KK3). Tek parça B-1 (T312) ve B-3 (T371), parça 2 ve 3 ile parça 4'ün bulguları yalnız kendi çağrılarında çıktı; parça 1 B-1 (geri alma/`Down`) tek parça çağrıda bu kez çıkmadı. 7. turun kabul ettiği dört yer (T119 k8, T201 k5/k9, T302 k9/KK9, T366 KK3) bu turda hiçbir çağrıda bulgu üretmedi; T201 yalnız eski sıra iddiasıyla (parça 2 B-1) anıldı.

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**(a) Tek parça çağrı:**

> BULGU-1
> Kriter: teknik doğruluk
> Seviye: Orta
> Yer: T312, kabul kriteri 8 / doğrulama kontrol listesi KK8
> Alıntı: “`Content-Range` yoksa ya da toplamı aşarsa 400 alır” / “uymayan başlangıçlı ve toplamı aşan parça 409 alır.”
> Sorun: Aynı “toplamı aşan parça” durumu kabul kriterinde 400, doğrulama listesinde 409’dur. Validator iki sonucu aynı anda doğrulayamaz; bu, kabul kriteri ile test beklentisinin çelişmesidir (4).
> Öneri: Toplam boyutu aşma için tek bir durum kodu belirleyin; kabul kriterini ve KK8’i aynı koda göre güncelleyin.
>
> BULGU-2
> Kriter: teknik doğruluk
> Seviye: Orta
> Yer: T117, kabul kriteri 3 / doğrulama kontrol listesi KK3
> Alıntı: “`Undelivered`'da yalnız gönderildikten sonra geri dönen satırda doludur” / “gönderim anı dolu da boş da `Undelivered` satır kabul edilir.”
> Sorun: Kabul kriteri, `Undelivered` durumunu gönderimden sonra gelen bir sonuç olarak tanımlarken test boş gönderim anını da geçerli saymaktadır. Bu, gönderilmemiş iletiyi teslim edilemedi diye işaretlemeye izin verir ve kabul kriteri–test çelişkisidir (4).
> Öneri: `Undelivered` için gönderim anının zorunlu olup olmadığını netleştirin; seçilen kuralı hem kısıta hem KK3’e aynı biçimde yazın.
>
> BULGU-3
> Kriter: eksiklik
> Seviye: Yüksek
> Yer: T371, kabul kriterleri 5–6
> Alıntı: “Dijital kalemde cayma hakkı yoktur; hak ödeme başarısında düşer ve ayrı onay kutusu zorunludur” / “Hizmette hak pencere dolunca ya da "tamamlandı" işaretinde, hangisi önce gelirse düşer”
> Sorun: Ödeme başarısı veya tek başına “tamamlandı” işareti, cayma hakkını ortadan kaldıran hukuki koşul değildir. Hizmet istisnası, cayma süresi dolmadan tüketicinin onayıyla ifaya başlanmasına; elektronik içerik istisnası ise anında elektronik ifa/teslime dayanır. Plan, bu koşulları ve kanıtını zorunlu kılmadan hakkı düşürüyor; tüketicinin cayma hakkı kaybına ve mevzuata aykırılığa yol açar (1, 2). [Ticaret Bakanlığı’nın güncel açıklaması](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)
> Öneri: Dijital içerikte hakkın ancak ilgili istisnanın fiilen doğduğu anda; hizmette ise süre dolmadan ifaya başlanmasına yönelik tüketici onayı alındığında düşmesini kabul kriteri yapın. Onayın metni, zamanı ve ifaya başlangıç anı sipariş kaydında kanıtlanabilir biçimde saklansın.
>
> SONUÇ: 3 BULGU

**(b) Parça 1 (§1–§4 · §5 Task yapısı · F0 · F1):**

> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §3 F1; §5 T04, T103–T119
> Alıntı: "**Çıkış ürünü:** göçler ileri, idempotent ve geri alınabilir" / "Göç tek konudur; `Down` ve işlem dışı adım yoktur"
> Sorun: F1 kapısı göçlerin geri alınabilir olmasını ister; T04 da geri alma sınaması koşar. Buna karşılık veri göçü task’ları `Down` bulunmamasını kabul kriteri yapar. Plan, geri almanın hangi uygulanabilir mekanizmayla sağlanacağını tanımlamadığı için kendi faz kapısı ve kabul kriterleri çelişir (4).
> Öneri: Geri alma yaklaşımını tekleştirin: ya her göç için doğrulanabilir geri alma tanımlayın ya da F1/T04’teki “geri alınabilir/geri alma sınaması” ifadelerini açıkça yedekten dönüş veya işlem geri sarma yöntemiyle değiştirip ilgili testi yazın.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §5 T117, kabul kriteri 3 ve KK3
> Alıntı: "`Undelivered`'da yalnız gönderildikten sonra geri dönen satırda dolu olur" / "gönderim anı dolu da boş da `Undelivered` satır kabul edilir"
> Sorun: Kabul kriteri, `Undelivered` kaydının ancak gönderimden sonra dönmüş olması hâlinde gönderim anı taşımasını isterken doğrulama listesi gönderim anı boş `Undelivered` kaydını da kabul eder. Böylece kriteri ihlal eden gönderim durumu testlerden geçer (4).
> Öneri: KK3’ü, `Undelivered` için gönderim anı boşsa reddedilecek; doluysa ancak gönderilmiş kaydın dönüşü koşulu sağlanacak biçimde düzeltin.
>
> BULGU-3
> Kriter: Güvenlik
> Seviye: Orta
> Yer: §5 T118, kabul kriteri 6 ve doğrulama kontrol listesi
> Alıntı: "`DataProtectionKey` paketin şemasıdır: tamsayı anahtar, oluşturulma anı yok"
> Sorun: Bu kabul kriterini sınayan hiçbir KK maddesi yoktur. KK1–KK8 tablo varlığını, iş/izinleri ve göç yapısını denetler; anahtar türünü ve oluşturulma anının yokluğunu doğrulamaz. Bu, dokümanın “kabul kriteri başına bir madde” güvencesini sağlamaz (4) ve veri koruma anahtarı şemasındaki hatanın doğrulanmadan kalmasına yol açar.
> Öneri: PostgreSQL şemasında `DataProtectionKey` birincil anahtarının tamsayı olduğunu ve oluşturulma zamanı sütunu bulunmadığını denetleyen ayrı bir entegrasyon testi ve KK maddesi ekleyin.
>
> SONUÇ: 3 BULGU

**(c) Parça 2 (F2 · F3 T301–T345):**

> BULGU-1
> Kriter: teknik doğruluk
> Seviye: Yüksek
> Yer: F2 T201; F3 T301–T302
> Alıntı: "`- **Bağımlılık:** T221, T226`" / "`imha parçaları F3'tedir (T301, T302)`"
> Sorun: T201’in kabulü ve entegrasyon testleri, F3’te uygulanacağı belirtilen imha parçalarının gerçek silme davranışına dayanıyor; fakat T201 bu task’lara bağımlı değil ve F2’de tamamlanmış kabul ediliyor. Bu, task’ın girdileri hazır olmadan kabul edilmesini sağlayan bir sıra/bağımlılık çelişkisidir (4).
> Öneri: T201’i yalnız zamanlayıcı ve imha orkestrasyonu ile sınırlandırıp T301–T302’yi ona bağımlı yapın; ya da T301–T302’yi F2’ye alıp T201’in açık bağımlılıkları yapın. İmha davranışını sınayan KK2, KK5 vb. testleri parçaların task’larına taşıyın.
>
> BULGU-2
> Kriter: güvenlik
> Seviye: Yüksek
> Yer: F2 T224, kabul kriteri 2; KK1–KK2
> Alıntı: "`Çerezler `__Host-` önekli ve kendi alan adına kilitlidir`"
> Sorun: `__Host-` önekli çerezin geçerli olması için `Secure`, `Path=/` ve `Domain` niteliğinin bulunmaması birlikte zorunludur; aksi durumda uyumlu tarayıcı çerezi reddeder. Plan yalnız üç niteliği ve öneki test ediyor; `Path=/` ile `Domain` yokluğunu ne kabul kriteri ne de KK doğruluyor. Bu yüzden oturum, sepet ve antiforgery çerezleri tarayıcıda hiç oluşmadan task başarılı sayılabilir; kullanıcı giriş ve sipariş akışında takılır (3, 4). [HTTP cookie standardı](https://datatracker.ietf.org/doc/html/draft-ietf-httpbis-rfc6265bis)
> Öneri: T224’e tüm `__Host-` çerezleri için açıkça `Secure; HttpOnly; SameSite=Lax; Path=/` ve `Domain` yokluğu kuralını ekleyin; KK1’i her çerezin gerçek `Set-Cookie` başlığında bu koşulları doğrulayacak şekilde genişletin.
>
> BULGU-3
> Kriter: teknik doğruluk
> Seviye: Yüksek
> Yer: F3 T330, kabul kriteri 7
> Alıntı: "`ETBİS kaydı yapılmaz; kayıt bilgisi isteğe bağlı alandır, satış kapısını tutmaz`"
> Sorun: Ürün kendi elektronik ticaret ortamında tüketiciye satış yaptığı için elektronik ticaret hizmet sağlayıcısıdır. Bu işletmelerin faaliyete başlamadan önce ETBİS’e kayıt olması gerekir. Planın kaydı hiç yaptırmaması ve satış kapısından çıkarması yürürlükteki yükümlülüğe aykırıdır; idari para cezası ve faaliyet riski doğurur (1, 2). [Ticaret Bakanlığı güncel SSS](https://ticaret.gov.tr/ic-ticaret/sikca-sorulan-sorular/elektronik-ticaret)
> Öneri: Uygulama içi ETBİS entegrasyonu gerektirmeden, yayına/satışa geçiş kapısına “ETBİS kaydı doğrulandı” operasyonel kontrolünü ve sorumlusunu ekleyin; kayıt bilgilerindeki değişikliklerin de yasal süre içinde bildirilmesini planlayın.
>
> SONUÇ: 3 BULGU

**(d) Parça 3 (F3 T346–T390 · F4):**

> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: T370 kabul kriterleri 6–7; doğrulama KK6–KK7
> Alıntı: "Firma iptalinde IBAN isteği iptal kaydıyla düşer, B-14 gider ve süre IBAN beklenirken durmaz" / "Firma iptalinde alan açılır, B-14."
> Sorun: Yasal geri ödeme süresinin IBAN beklerken işlemeye devam etmesi kabul kriteridir; ancak KK6 ve KK7 yalnız alanın açılmasını ve bildirimi denetler. Sürenin durmadığını sınayan hiçbir test yoktur. Task, iade süresini fiilen durduran bir uygulamayla geçebilir; bu, tüketicinin yasal iade süresini ihlal eder. (1) ve (4).
> Öneri: KK6 ve KK7’ye sahte saatli Unit/Integration senaryoları ekleyin: IBAN istenmesi veya havale hatası sonrasında başlangıç tarihi değişmemeli, son tarih uzamamalı ve süre dolunca işlem yasal zamanında gecikmiş sayılmalıdır.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: T404 kabul kriteri 1; doğrulama KK1
> Alıntı: "bağlanılamama, 15 sn aşımı, 5xx, `failed` + `reason` ve tablosuz yanıt `ConnectionFailed`'dır" / "başlatma yanıtı tablosunun altı satırı birer test."
> Sorun: Kabul kriteri beş hata sınıfı tanımlarken doğrulama kontrolü altı tablo satırının test edilmesini ister. Altıncı satırın hangi olay ve beklenen sonuç olduğu tanımsızdır; test beklentisi kabul kriterini aşmaktadır. (4).
> Öneri: Altıncı durumu kabul kriterinde açıkça tanımlayın veya KK1’i beş tanımlı hata sınıfına göre düzeltin.
>
> SONUÇ: 2 BULGU

**(e) Parça 4 (F5 · F6 · §6):**

> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: T501 kabul kriteri 2
> Alıntı: "Logo yalnız üretim yeri Türkiye olan fiziksel ürünün sayfasında, fiyatın yanında görünür; dijital üründe ve hizmette görünmez"
> Sorun: Dijital ürünü kategorik olarak kapsam dışı bırakmak mevzuata aykırıdır (1). Yerli Üretim Logosu Tebliği, “mal”a elektronik ortamda kullanılmak üzere hazırlanan yazılım, ses, görüntü ve benzeri gayri maddi malları da dahil eder; Türkiye’de üretilmiş sayılan mallarda logo zorunludur. Hizmetin dışarıda tutulması mümkündür; ancak Türkiye’de üretilmiş sayılan dijital malların otomatik dışlanması hukuka aykırı etiketlemeye yol açar. [Resmî Tebliğ](https://tuketici.ticaret.gov.tr/data/5e81982d13b876a1b04c7a42/2023-6502%20Say%C4%B1l%C4%B1%20T%C3%BCketicinin%20Korunmas%C4%B1%20Hakk%C4%B1nda%20Kanun.pdf)
> Öneri: Kriteri, fiziksel/dijital ayrımı yerine Tebliğ m. 6’ya göre “yerli üretim sayılan her mal” olarak değiştirin; hizmetleri hariç tutun. Dijital ürünlerde de üretim yeri Türkiye ve yerli üretim koşulları sağlanıyorsa logoyu gösterin.
>
> SONUÇ: 1 BULGU

**(f) Parça 5 (§7 · §8 · §9):**

> SONUÇ: TEMİZ

## 3. Bağımsız değerlendirme

Ham bulgular numaralandı: tek parça B-1…B-3, parça 1 B-1…B-3, parça 2 B-1…B-3, parça 3 B-1…B-2, parça 4 B-1, parça 5 TEMİZ (12 ham). Bir çift aynı iddiadır (tek parça B-2 = parça 1 B-2, T117 KK3) → 11 ayrı bulgu (F1–F11; sıra: doğrulayıcı sırası). On `[dogrulama]` Opus / high doğrulayıcıya verildi: Yüksek bulgular tek tek (D3: F4, D4: F5, D5: F6, D6: F7, D7: F8, D8: F9, D10: F11), Orta bulgular ayrı bölümlerden olduğu için tek tek ya da aynı bölümden gruplu (D1: F1; D2: F2 + F3, ikisi de F1 bölümünde, üç ham satır; D9: F10); toplam 10 doğrulayıcı (bekçi sınırı 20; bekçi olayı yok). Önceki turlarda reddedilen iddianın olabileceği bulgularda (F1, F2, F5, F6, F8) paketle birlikte önceki hükmün tam dosyası verildi ve talimata "yeni bilgi yoksa kısa gerekçeyle tekrar eden RET" kuralı yazıldı; 7. turda değişen T119 k8, T201 k5/k9, T302 k9/KK9, T366 KK3 cümleleri F6 paketine (T201, T301, T302 blokları) konuldu. Yasal iddialı üç bulgu (F4 cayma, F8 ETBİS, F11 yerli üretim logosu) resmî metinle okundu. Hükümler `11_audit_hukum_denetle.py` ile denetlendi: sorun 0.

| # | Bulgu (kaynak) | Karar | Gerekçe | Uygulama |
|---|---|---|---|---|
| F1 | T312 kabul kriteri 8 "toplamı aşan parça 400" ↔ KK8 "uymayan başlangıçlı ve toplamı aşan parça 409" (tek parça B-1; T312) | ❌ RET (tekrar eden: 1. tur D4 #3, 3. tur D7 #10; Orta → Düşük) | Yeni bilgi yok; 11:1987 ve 11:1999 o turlardaki metinle aynı. Kriter 8 toplamı aşan parçaya tek başına 400 der; KK8'in 409 örneği hem uymayan başlangıçlı hem toplamı aşan parçadır ve `07` denetim sırası (07:4886, 07:5305–5308) 409'u boy aşımı 400'ünden önce koyar. Bulgu iki ayrı durumu aynı sanıyor; çelişki yok. | Yok |
| F2 | T117 kriter 3 ↔ KK3: `Undelivered`'da gönderim anı hem dolu hem boş kabul edilir (tek parça B-2, parça 1 B-2; T117) | ❌ RET (tekrar eden: 1. tur D3 #2, 3. tur #6, 5. tur F10, 6. tur F8, 7. tur F12; Orta → Düşük) | Yeni bilgi yok; metin değişmedi (11:781, 11:789). `06 §4.58` Kısıtlar ve §5 kısıt özeti `Undelivered`'da `SentAt`'i iki hâl için açık bırakır: hiç gönderilmemiş satırda boş, gönderilip geri dönen satırda dolu (06:4388, 06:4420, 06:5727; K-1120). KK3 kuralın doğrudan karşılığıdır; "boşsa reddet" önerisi `06 §4.58`'e aykırı olurdu. | Yok |
| F3 | T118 kriter 6 "`DataProtectionKey` paketin şemasıdır: tamsayı anahtar, oluşturulma anı yok"; hiçbir KK bunu sınamaz (parça 1 B-3; T118) | KISMİ (Orta) | Olgu doğru: T118'in sekiz kriteri ile sekiz KK'si sayıca eşleşir, ama k2 iki maddeye (KK2, KK3) yayılmıştır ve k6 hiçbir KK'de sınanmaz; KK1 yalnız tablo sayısına bakar. `11` §5 konvansiyon 7 (11:168) "kabul kriteri başına bir madde" ister (4). Kaynak `06 §4.68` (`Id` tamsayı, `CreatedAt` yoktur — §2.8.4'ün tek istisnası). Öneri daraltıldı: ayrı entegrasyon testi gereksiz (test beklentisi zaten Integration, 11:807); düzeltme KK1'in sonuna ekleme. | T118 KK1 (1 şartname) |
| F4 | T371 k5–k6: dijital kalemde hak ödeme başarısında, hizmette pencere dolunca ya da "tamamlandı" işaretinde düşer; hukuki koşul (onay, ifaya başlama) kanıtlanmıyor (tek parça B-3; T371) | ❌ RET (Yüksek → Düşük) | Resmî metin: Mesafeli Sözleşmeler Yönetmeliği (RG 27/11/2014 sayı 29188, değişik 23/8/2022) m.15/1 mevzuat.gov.tr PDF'inden okundu — (ğ) elektronik ortamda anında ifa edilen hizmet ya da anında teslim edilen gayrimaddi mal; (h) cayma süresi dolmadan tüketicinin onayıyla ifasına başlanan hizmet; (ı) hizmetle ilgisiz. T371 k5 `02 §7.3.2`'yi (K-204, K-550), k6 `02 §7.3.3`'ü (K-205, K-578) ve Z-14'ü doğru tüketir: dijitalde hak, indirmenin açıldığı ödeme başarısı anında düşer ("anında teslim"in kendisi, `02` §7.2.5) ve ayrıca zorunlu onay kutusu vardır; hizmette onay kutusu sipariş anında pencere içinde alınır, "tamamlandı" işareti ifanın başladığını gösterir ve hak (h)'nin izin verdiğinden daha geç bir ana bağlıdır (tüketici lehine). Onay kanıtı zaten saklanır: `02 §3.24.6` (K-595) kutu, gösterilen metin ve kapsadığı kalemler siparişe donar; evi `11` T356 k2 ve k7 (11:3075, 11:3080). "İfaya başlangıç anının kaydı" önerisi `02 §7.3.3`'te gerekçesiyle bilerek elenmiştir (K-205, K-578). İkinci modelin kaynağı Bakanlık bilgilendirme sayfası (derleme). Mevzuata aykırılık, hak kaybı ya da iç çelişki yok. | Yok |
| F5 | F1 çıkışı ve T04 "geri alınabilir" der, T103 ve veri göçü task'ları `Down` yasaklar (parça 1 B-1; F1, T04, T103–T119) | ❌ RET (tekrar eden: 2. tur F1, 3. tur F1, 4. tur F1, 6. tur #1–#2, 7. tur F1; Yüksek → Düşük) | Yeni bilgi yok; `05` ve `09` son değişikliğinden (#266) beri aynı, 7. turda T04 KK5 satırına dokunulmadı. "Geri alma" şemayı `Down` ile geri almak değil önceki imaja dönmektir ve CI göç adımında sınanır: `05` MK-74, §8.7; `09` §10.6 madde 1, §21.4 madde 3. Bulgunun ikinci kolu (`Down` eklemek) MK-74 ve `09` §10.6 ile çelişirdi. | Yok |
| F6 | T201 imha parçaları F3'te (T301, T302) olduğu hâlde T201 bunlara bağımlı değil ve F2'de kabul ediliyor (parça 2 B-1; T201, T301, T302) | ❌ RET (tekrar eden: 5. tur F1; Yüksek → Düşük) | Yeni bilgi yok. Fazlar zincirdir, `Bağımlılık` yalnız aynı fazı yazar, sonraki faz task'ı gate'le karşılanır (11:119, K-1408 (1)). Bulgunun yeni kolu da tutmaz: T201'in testleri (KK2, KK5 dahil) F3'teki parçalara dayanmaz — T201 gecelik temizliği (`06 §9.4.3`) ve kayıt dosyalarını (`06 §9.3.4`) F2'de kendi parçalarıyla karşılar; bu parçaların kullandığı entity'ler F1'de oluşur (11:106). 7. turun kabulü (T201 k5/k9) bu sıra iddiasını değiştirmedi. | Yok |
| F7 | T224 k2 `__Host-` önekli çerez: `Path=/` ve `Domain` yokluğunu ne kriter ne KK sınar (parça 2 B-2; T224) | KISMİ (Yüksek → Orta) | Teknik iddia doğru (RFC 6265bis: `__Host-` öneki `Secure`, `Path=/` ve `Domain` yokluğunu birlikte ister) ve kaynak bunu yazar: `09` §15.1 madde 6 (09:1599, `CookieDefaults.Apply`: `Path = "/"`, `Domain` boş) ve `05` §6.7.1 (05:769). Boşluk `11`'dedir: kriter 2 (11:1448) "kendi alan adına kilitlidir" der, KK1 üç niteliği, KK2 yalnız adın önekini sınar; `Path=/` yalnız akış çerezinde (KK8) sınanır, `Domain` yokluğu hiç sınanmaz — kabul kriteri ↔ kontrol listesi farkı (4). Seviye Orta: doğru kod kuralı `09`'da yazılı (yanlış davranış kodlanmaz), eksik olan doğrulamadır; çatının tek kökten farklı yol varsayılanı (dış giriş akışı) KK8'de kapalı. Öneri daraltıldı: nitelik listesi kritere kopyalanmaz (K-1445), `09`'a işaret edilir. İkinci modelin gösterdiği kaynak taslak standarttır. | T224 k2, KK2 (2 şartname) |
| F8 | T330 k7 "ETBİS kaydı yapılmaz, satış kapısını tutmaz": ETBİS kaydı yasal yükümlülük (parça 2 B-3; T330) | ❌ RET (tekrar eden: 2. tur F6, 3. tur F3, 4. tur F1, 6. tur #1; Yüksek → Düşük) | Yeni bilgi yok; kaynak yine Ticaret Bakanlığı SSS sayfası (türetilmiş), `02` ve `10` son kez #265'te değişti, önceki hükümlerden önce. Kaydı firma yaptırır; ürün kaydı göremediği için satışı ona bağlamaz (`02` §3.1.4, §12.1.4; `10` ÖK-5; K-574, K-628). Önerinin "satıştan önce kaydol" ön koşulu planda T601 k9'da (ÖK-1…ÖK-12) karşılanır; "bilgi değişikliğini yasal sürede bildir" `02 §12.5` tablosunda firmanın yükümlülüğü olarak yazılıdır. | Yok |
| F9 | T370 k6–k7 "süre IBAN beklenirken durmaz" bu davranışı KK6 ve KK7 sınamıyor (parça 3 B-1; T370) | KISMİ (Yüksek → Orta) | Olgu doğru: k6 (11:3409) ve k7 (11:3410) "süre durmaz" der (kaynak `02 §7.2.8` K-690, §7.4.5 "geri ödemenin süresi durmaz"; Yönetmelik durma hâli tanımaz, m.12/1–3), KK6 yalnız alan ve B-14'ü, KK7 yalnız alanın yeniden açılmasını ölçer; KK7 B-14'ü de ölçmez — kriter ↔ kontrol listesi farkı, konvansiyon 7 (4). Seviye Orta: doğru davranış kriterde kodlanmış, eksik olan doğrulamadır. Öneri daraltıldı: "süre dolunca işlem gecikmiş sayılsın" kolu kaynakta yok (Z-16/Z-17'de süre dolunca kendiliğinden işlem ya da durum doğmaz), sahte saat kurgusu kural ayrıntısıdır. Aynı eksik T390 k8 ↔ KK8'de de vardı (üçüncü şartname). Paketteki "m.15" notu yanlış: geri ödeme süresinin dayanağı m.12/1–3'tür. | T370 KK6, KK7; T390 KK8 (3 şartname) |
| F10 | T404 k1 beş hata sınıfı sayar, KK1 başlatma yanıtı tablosunun "altı satırını" ister; altıncı satır tanımsız (parça 3 B-2; T404) | ❌ RET (Orta → Düşük) | İki sayı farklı şeyi sayıyor: KK1'in "tablo"su `08 §2.1.8` başlatma yanıtı tablosudur (08:312–319), gerçekten altı satırlı; kriter 1'deki beş öğe ayrı hata sınıfı değil — ilk üçü (bağlanılamama, 15 sn aşımı, 5xx) tablonun aynı satırında. Kriter 1 tablonun 1., 2. ve 6. satırlarını, kriter 2 3. satırı (ikinci jeton reddi) karşılar; 4. satır başarı, 5. satır başarısız ödemedir ve "ayrım yalnız başlatma yanıtında yapılır" cümlesinin testidir. Test kabul kriterini aşmıyor; (4) yok. | Yok |
| F11 | T501 k2 yerli üretim logosu dijital üründe görünmez; Tebliğe göre dijital mallar da kapsanır (parça 4 B-1; T501) | ❌ RET (Yüksek → Düşük) | Kaynak karar bilinçli ve `11` doğru tüketir: T501 k2 (11:4229) `02 §3.8.5`'i aynen taşır — logo yalnız üretim yeri Türkiye olan fiziksel üründe görünür (K-610); dijital üründe üretim yerinin isteğe bağlı olması K-511'in proje sahibince gözden geçirilmiş kararıdır; hizmetin dışarıda kalması Yönetmelik m.5/7'ye dayanır. Yasal iddia resmî metinle doğrulanmadı: ikinci modelin bağlantısı 6502 sayılı Kanun PDF'idir, "Tebliğ m.6" hükmünü gösteremedi. Resmî metin: RG 27.07.2019 sayı 30844, Fiyat Etiketi Yönetmeliği m.4/1-l "yerli üretim"i Sanayi Sicili Kanunu m.1/1 işletmelerinin ürettiği mallar, el ve ev sanatları ürünleri ve Gümrük Kanunu m.18 ürünleri olarak tanımlar — üçü de fiziksel üretimdir; Ticaret Bakanlığı duyurusunda dijital mal ya da yazılım geçmez. "Mal" tanımının gayrimaddi malı kapsaması logonun dijitalde zorunlu olduğunu doğurmaz. Doğrulayıcı Tebliğin kendi tam metnini okuyamadı (Resmî Gazete arşiv bağlantısı 404 verdi); kalan yorum riski K-511'in "Kalan risk" notunda kayıtlıdır. | Yok |

**Tekrar eden RET (turun sonucunu belirlemez):** F1 (1., 3. tur), F2 (1., 3., 5., 6., 7. tur), F5 (2., 3., 4., 6., 7. tur), F6 (5. tur), F8 (2., 3., 4., 6. tur). **Seviye düşenler:** Yüksek → Düşük: F4, F5, F6, F8, F11; Yüksek → Orta: F7, F9; Orta → Düşük: F1, F2, F10. **Kısmi kalış:** F3 (öneri daraltıldı: ayrı test gereksiz, KK1 ekine yetti); F7 (nitelik listesi kritere kopyalanmadı, `09`'a işaret); F9 (gecikme kolu ve sahte saat kurgusu alınmadı; T390 KK8 eklendi).

**Çıkış değerlendirmesi (Faz 4 madde 4–5).** Sonuç TEMİZ değil: tek parça ve parça 1–4 bulgu getirdi (parça 5 TEMİZ). Madde 5'in çıkış kuralı işlemez: üç kabulün üçü de Orta şiddette (kabul kriteri ↔ kontrol listesi farkı: kriter sınanmıyor ya da KK kriteri izlemiyor; mevzuata aykırılık, para ya da hak kaybı, çıkışsız akış değil ama "düşük metin farkı" sınıfında da değil) ve kural yalnız kabullerin hepsi düşük metin farkıysa kapatır. Tekrar eden RET kuralı F1, F2, F5, F6, F8'i turun sonucundan ayırır. **Tur 9 gerekir.** Gözlem (Faz 4 madde 6): üç Orta kabul aynı sınıftadır (kabul kriteri ↔ KK; hedefli mercekle — `11_CROSS_REVIEW_R3.md` sonu, KD-cross-m — dokümanın tamamında taranan sınıf); 7. turun Orta kabulü (T201 k5, kapsamsız kriter ↔ kaynak karar) bu sınıfta değildi, bu yüzden "art arda iki tur aynı sınıf" koşulu henüz sağlanmadı; 9. turun kabulü yine bu sınıftan Orta çıkarsa mercek yeniden değerlendirilir.

**F3, F7, F9 — uygulama ve gözlem.** Altı çift `11_audit_uygula.py` ile uygulandı. Doğrulayıcıların bayt tahmini (T370 yaklaşık 2.546) yanlış çıktı: uygulama sonrası `--tam` T370 için 2.677 ve T390 için 2.611 bayt (sınır 2.560) verdi; ekler elle kısaltıldı — KK6, KK7 ve KK8'e "süre durmaz" yazıldı, T370 k6 ve T390 k8'deki "IBAN beklenirken/beklerken" sözü kısaltıldı (anlam k7'nin ve KK'lerin içinde, aynı task bloğunda kalır), T370 KK7'ye "IBAN silinir" eklenmedi. Blok boyutları: T118 2.234, T224 2.207, T370 2.560, T390 2.560 bayt (sınır 2.560). Yazım kapanışı, matris kapsama, task blok denetimi (`--tam`), geri yön denetimi ve geri işaret taraması sorun 0. Gözlem: T370 ve T390 tam sınırdadır (ayrıca T301 ve T302 sınıra yakın, 7. tur raporu); bu bloklara sonradan eklenecek metin bloğu bölmeyi (K-1409 (2)) ya da kısaltmayı gerektirir.

## 4. Ek bulgular

- Doğrulayıcılardan `11`'i ya da üst dokümanları etkileyen ek bir tutarsızlık bildirilmedi. Üst doküman notu yok; `00`/`CLAUDE.md`/`SETUP.md` önerisi yok.
- F11 için kayıt: yerli üretim logosu Tebliğinin kendi tam metni okunamadı (Resmî Gazete arşiv bağlantısı 404); hüküm Fiyat Etiketi Yönetmeliği m.4/1-l (RG 30844) ve Ticaret Bakanlığı duyurusuna ve K-511/K-610 kararına dayanır. Hükmün dayandığı yer değişmedi; kalan yorum riski K-511'de kayıtlıdır, yeni bir ⚠ doğurmaz.

## 5. Kullanıcı onay checklist'i

- [ ] F1 — RET, tekrar eden (uygulanmadı)
- [ ] F2 — RET, tekrar eden (uygulanmadı)
- [ ] F3 — KISMİ (uygulandı: T118 KK1)
- [ ] F4 — RET (uygulanmadı)
- [ ] F5 — RET, tekrar eden (uygulanmadı)
- [ ] F6 — RET, tekrar eden (uygulanmadı)
- [ ] F7 — KISMİ (uygulandı: T224 k2, KK2)
- [ ] F8 — RET, tekrar eden (uygulanmadı)
- [ ] F9 — KISMİ (uygulandı: T370 KK6, KK7; T390 KK8; bayt sınırı için T370 k6 ve T390 k8 kısaltıldı)
- [ ] F10 — RET (uygulanmadı)
- [ ] F11 — RET (uygulanmadı)

⚠ karar: yok. Önerilen `00`/`CLAUDE.md`/`SETUP.md` değişikliği yok.
