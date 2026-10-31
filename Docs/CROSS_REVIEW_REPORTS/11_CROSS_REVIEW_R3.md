# Cross-Review — 11 Implementation Planı (Tur 3)

**Tarih:** 2026-10-31 · **Hedef:** `Docs/11_IMPLEMENTATION_PLAN.md` v0.39 (girdi commit'i `0deb2d0`, 748 836 bayt, 7 760 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren, mekanizma okuma paragrafıyla (K-1399; `11_CROSS_REVIEW.md` §1) · **Koşum:** iki koşum — tek parça çağrı ve beş parçalı kontrol koşumu · **Sonuç:** **TEMİZ DEĞİL** — 14 ham bulgu, 13 ayrı bulgu; beşi (iki doğru, üç kısmi) uygulandı (7 şartname çifti), sekizi reddedildi (altısı önceki turlarda reddedilmiş iddianın dönüşü).

## 1. Koşum

Betikler, istem, parça notu, şablon ve **parça sınırları 1. turdakinin aynısıdır** (`11_CROSS_REVIEW.md` §1; K-1478 — sınır başlıkla tanımlıdır, değişmez). İstem tek karakter değiştirilmeden kullanıldı. Girdi `build_inputs.py` ile kuruldu (`uncovered lines: none`); `codex login status` → "Logged in using ChatGPT". Altı çağrı tek Bash komutunda paralel koştu (`run_codex.sh r3/in r3/out c1 p1 p2 p3 p4 p5`; çalışma klasörü her çağrının kendi boş klasörü); kullanım sınırı dolmadı, çıktısız dönen çağrı olmadı, yedek modele geçilmedi. Çalışma dizini: `C:\Users\teknodeveloper\.claude\helper-state\shopfolio\kd-cross\r3\` (girdiler `in/`, ham çıktılar `out/`, doğrulayıcı paketleri `dogrulayici_<k>.md`, hükümler `hukum_D<k>.md`, talimat `talimat_dogrulayici.md`; paket betiği `betik/paket3.py`, sınıf taraması `sinif_tara.py`).

| Çağrı | Bölümler | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|
| Tek parça | tüm doküman | 758 381 bayt | 401 588 | 182 sn | 2 bulgu |
| Parça 1 | §1–§4 · §5 Task yapısı · F0 · F1 (satır 65–866) | 96 063 bayt | 53 106 | 207 sn | 5 bulgu |
| Parça 2 | F2 · F3 T301–T345 (867–2841) | 196 444 bayt | 101 251 | 210 sn | 3 bulgu |
| Parça 3 | F3 T346–T390 · F4 (2842–4219) | 150 564 bayt | 97 896 | 248 sn | 2 bulgu |
| Parça 4 | F5 · F6 · §6 (4220–5633) | 149 338 bayt | 82 591 | 211 sn | 2 bulgu |
| Parça 5 | §7 · §8 · §9 (5634–dosya sonu) | 254 214 bayt | 138 594 | 154 sn | TEMİZ |

Web araması satırları (çağrı günlükleri): tek parça 2, parça 1 0, parça 2 4, parça 3 12, parça 4 4, parça 5 6. **İki koşumun ölçümü:** tek parça 2 bulgu, parçalı koşum 12 bulgu; örtüşme yalnız F1'de (iki koşum da `Down` yasağı ↔ geri alma iddiasını getirdi); T312 iki koşumda ayrı iddialarla göründü (tek parça: 400/409; parça 2: KK1). İki koşum 4. turda da sürer.

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**(a) Tek parça çağrı:**

> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §3 F1 çıkış ürünü; §6.2 F1 kapısı; T04; T103–T119
> Alıntı: "Göçler ileri, idempotent ve geri alınabilir" / "Göç tek konudur; Down ve işlem dışı adım yoktur"
> Sorun: F1 kapısı ve T04, göçlerin geri alınmasını ve bunun CI’da sınanmasını zorunlu kılıyor; F1’deki göç task’ları ise `Down` uygulanmasını yasaklıyor. EF Core göçlerinde geri alma sınaması için uygulanabilir bir geri dönüş yolu gerekir. Plan, kendi faz-kapısı güvencesini karşılayamaz (4).
> Öneri: Tek yaklaşımı seçin ve tüm blokları hizalayın: geri alınabilir göç hedefi korunacaksa her göçte sınanabilir `Down` tanımlayın; ileri-yönlü göç politikası korunacaksa F1/T04’teki geri alma gereğini kaldırıp bunun yerine geri yükleme ve düzeltici ileri göç provasını zorunlu kılın.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: T312 kabul kriteri 8 ve doğrulama kontrol listesi KK8; T548 yükleme betiği davranışı
> Alıntı: "Content-Range yoksa ya da toplamı aşarsa 400 alır" / "uymayan başlangıçlı ve toplamı aşan parça 409 alır."
> Sorun: Aynı hata durumu için kabul kriteri 400, doğrulama listesi 409 bekliyor. Yükleme betiği 409’da alınan bayttan sürdürür, 400’de durur; bu çelişki hatalı parçanın sürdürülmesine veya kullanıcıya düzgün hata gösterilememesine yol açar (3, 4).
> Öneri: Durum kodunu tekleştirin: geçersiz `Content-Range` toplamı için 400 ve sürdürme bilgisi yok; yalnız geçerli biçimdeki yanlış ofset için 409 ve `receivedBytes` kullanın. T312 testlerini ve T548 istemci akışını buna göre eşleyin.
>
> SONUÇ: 2 BULGU

**(b) Parça 1 (§1–§4, F0, F1):**

> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §3 F1; §5 T103–T118
> Alıntı: “Çıkış ürünü: göçler ileri, idempotent ve geri alınabilir” / “Göç tek konudur; `Down` ve işlem dışı adım yoktur”
> Sorun: F1’in zorunlu çıkışı göçlerin geri alınabilmesini isterken, veri göç task’ları `Down` adımını sistematik olarak yasaklıyor. Bu ayrıca T04’teki geri alma sınamasını uygulanamaz bırakır. Bu bir iç çelişkidir (4. sınıf).
> Öneri: Ya her göç için doğrulanabilir geri alma yaklaşımını (`Down` veya açıkça tanımlı geri-yükleme yöntemi) ekleyin; ya da F1 çıkışı ve T04 geri alma denetimini, kabul edilen ileri-yönlü göç stratejisiyle tutarlı biçimde değiştirin.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §5 T117, kabul kriteri 3 ve KK3
> Alıntı: “`Undelivered`'da yalnız gönderildikten sonra geri dönen satırda doludur” / “gönderim anı dolu da boş da `Undelivered` satır kabul edilir”
> Sorun: Kabul kriteri, teslim edilemeyen kaydın önceden gönderilmiş olmasını gerektirirken doğrulama listesi gönderim anı boş kaydı da geçerli sayıyor. Böylece validator, task’ın kabul etmediği bir kanal durumunu onaylayabilir. Bu bir iç çelişkidir (4. sınıf).
> Öneri: KK3’ü, `Undelivered` kaydında gönderim anının zorunlu olduğunu doğrulayacak şekilde düzeltin.
>
> BULGU-3
> Kriter: Teknik doğruluk
> Seviye: Orta
> Yer: §5 T102
> Alıntı: “**Test beklentisi:** Unit” / “izinli olmayan metin kısıtı ihlal eder”
> Sorun: Veritabanı `CHECK` kısıtının gerçekten ihlal edilmesini sınamak PostgreSQL üzerinde çalışan entegrasyon testini gerektirir; yalnız Unit beklentisi bu kabul kriterini sınayamaz. Kabul kriteri ile test beklentisi uyuşmuyor (4. sınıf).
> Öneri: T102’nin test beklentisini `Unit, Integration` yapın ve geçersiz enum metninin PostgreSQL tarafından reddedildiği entegrasyon testini zorunlu kılın.
>
> BULGU-4
> Kriter: Güvenlik
> Seviye: Orta
> Yer: §5 T05, kabul kriterleri 2 ve 4; KK2 ve KK4
> Alıntı: “ters vekil ve yedek aracı veritabanına ulaşamaz” / “ters vekilden veritabanına bağlantı reddedilir”
> Sorun: Ağ ayrımı kriteri hem ters vekilin hem yedek aracının veritabanına erişememesini ister; doğrulama yalnız ters vekili sınar. Aynı şekilde dışa çıkış kriteri yedek aracını da kapsarken KK4 yalnız veritabanı konteynerini sınar. Zorunlu ağ yalıtımı doğrulanmadan kalır (4. sınıf).
> Öneri: Ayrı doğrulama maddeleri ekleyin: yedek aracından veritabanı erişiminin reddi ile hem veritabanı hem yedek aracının hedef-adresli dışa çıkış kurallarının doğrulanması.
>
> BULGU-5
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: Başlık notu; §5 T02, kabul kriteri 8
> Alıntı: “Bu doküman **tüketici**dir — `02`–`10`'u tüketir, onlara geriye etki yaratmaz.” / “çıkan fark ilgili `09` satırına aynı PR'da yazılır”
> Sorun: Plan kendisini kaynak dokümanlara geri etki etmeyen tüketici olarak tanımlarken T02, uygulama bulgusuyla kaynak Kodlama Kılavuzu’nun aynı PR’da değiştirilmesini emrediyor. Kaynak yanlışsa §8’e GAP girileceği kuralıyla da çelişir (4. sınıf).
> Öneri: T02’deki farkı §8’e GAP olarak kaydetmeyi ve kaynak doküman yetkili biçimde güncellendikten sonra planı hizalamayı şart koşun.
>
> SONUÇ: 5 BULGU

**(b) Parça 2 (F2; F3 T301–T345):**

> BULGU-1
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: T312, Kabul kriterleri 1–2; Doğrulama kontrol listesi KK1
> Alıntı: “İndirme ucu imzalı değerle açılır” / “Ömrü dolmuş, imzası tutmayan ya da kalemi indirilemez değer 404 alır” / “KK1: değer imzasız ya da oturumsuz açılır.”
> Sorun: KK1, imzasız indirmenin açılacağını söyleyerek kabul kriterleriyle çelişiyor. Bu, ücretli dijital dosyanın yetkisiz indirilmesine ve firmaya doğrudan gelir kaybına yol açabilir (2 ve 4).
> Öneri: KK1’i “Geçerli imzalı değer oturumsuz açılır” olarak değiştirin; imzasız, bozuk, süresi geçmiş ve indirilemez kaleme ait değerlerin her biri için 404 testi ekleyin.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: T209, Kabul kriteri 1; Doğrulama kontrol listesi KK1
> Alıntı: “güncel hâlle karşılaştırılmayan işlemler (sepete ekleme, iletişim talebi, adres ekleme, indirme bağlantısı, panelin yeni kayıt formları, dışa aktarma) ikinci kez işlenir” / “KK1: iki kez gönderilen aynı formun etkisi tektir.”
> Sorun: Genel KK1, açıkça iki kez işlenmesi istenen işlemleri de tek etkili olmaya zorlar. Aynı task için karşılanamaz iki kabul ölçütü oluşuyor; doğrulayan ajan hangi davranışı kabul edeceğini bilemez (4).
> Öneri: KK1’i yalnız güncel hâlle karşılaştırılan işlemlerle sınırlandırın; sayılan istisnalar için ikinci gönderimin ikinci etkiyi oluşturduğunu doğrulayan ayrı testler yazın.
>
> BULGU-3
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: T330, Kabul kriteri 7
> Alıntı: “ETBİS kaydı yapılmaz; kayıt bilgisi isteğe bağlı alandır, satış kapısını tutmaz”
> Sorun: Kendi sitesinde tüketiciye satış yapan bu hizmet sağlayıcı için faaliyete başlamadan ETBİS kaydı zorunludur. Plan, kaydı açıkça dışlayıp satışın açılmasına izin veriyor; bu mevzuata aykırılık ve idari para cezası riski doğurur (1 ve 2). Ticaret Bakanlığı, kendine ait e-ticaret ortamında faaliyet gösterenlerin ETBİS’e faaliyetten önce kaydolması gerektiğini ve ihlal için idari para cezasını belirtir. [Ticaret Bakanlığı SSS](https://ticaret.gov.tr/ic-ticaret/sikca-sorulan-sorular/elektronik-ticaret)
> Öneri: ETBİS kaydını yayın öncesi zorunlu operasyonel gate olarak ekleyin; satış kapısının, doğrulanmış kayıt tamamlanmadan açılmamasını sağlayın.
>
> SONUÇ: 3 BULGU

**(b) Parça 3 (F3 T346–T390; F4):**

> BULGU-1
> Kriter: Yasal uyum
> Seviye: Yüksek
> Yer: T372, Kabul kriteri 8
> Alıntı: "Kargoya verilmemiş fiziksel kalemde cayma düğmesi ve kaydı yoktur; başka kanaldan gelen bildirim "müşteriyle anlaşıldı" iptaliyle işlenir."
> Sorun: Bu, teslim öncesi caymayı ayrı bir tüketici hakkı olarak işletmeyip firmaya bağlı “müşteriyle anlaşıldı” iptaline çeviriyor. Mesafeli Sözleşmeler Yönetmeliği m.9/2 tüketicinin sözleşmenin kurulmasından teslimata kadar cayma hakkını kullanabileceğini; m.11 ise açık yazılı/kalıcı veri saklayıcısı beyanının yeterli olduğunu düzenler. Tüketicinin kanuni cayma hakkı engellenebilir veya yanlış işlenebilir.
> Öneri: Kargoya verilmemiş fiziksel kalem için de cayma beyanını kanuni cayma kaydı olarak kabul edip otomatik etkilerini ve geri ödeme hattını işletin; başka kanaldan gelen geçerli beyanı “müşteriyle anlaşıldı” firma iptaline dönüştürmeyin.
>
> BULGU-2
> Kriter: Yasal uyum
> Seviye: Yüksek
> Yer: T368, Kabul kriteri 6
> Alıntı: "Panel fesih satırında ve taahhüt süresi sonrası iptalde kanuni faiz uyarısı gösterir; sistem faiz hesaplamaz."
> Sorun: Gecikme nedeniyle fesihte yalnız uyarı verilmesi, kanuni faizin geri ödemeye eklenmesini sağlayan hiçbir task veya kabul kriteri bırakmıyor. Mesafeli Sözleşmeler Yönetmeliği m.16/3, fesihte tahsil edilen tüm ödemelerin teslimat masraflarıyla birlikte kanuni faizle iadesini zorunlu kılar. Bu, tüketicinin parasal hakkının eksik ödenmesine yol açar.
> Öneri: T368, T369 ve T370’ye kanuni faizi uygulanabilir bir geri ödeme bileşeni olarak ekleyin; faizli toplamın ödeme yoluna gönderildiğini ve yasal süre/rate kuralının test edildiğini kabul kriterleriyle doğrulayın.
>
> SONUÇ: 2 BULGU

**(b) Parça 4 (F5, F6, §6):**

> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Orta
> Yer: Başlık notu — “Okuma tablosu ve boyut”
> Alıntı: "`grep -n -A 30 '^#### T123: ' <dosya>` — tek blok ... sonraki `####` başlığında biter."
> Sorun: `grep -A 30`, sonraki `####` başlığında durmaz; sabit olarak 30 sonraki satırı da basar. Örneğin T501 bloğu 30 satırdan kısa olduğundan bu komut T502’nin başını da döndürür. Böylece “yalnız kendi bloğu okunur” güvencesi sağlanmaz; task ajanı yanlış task girdisini de tüketir. Bu, dokümanın koyduğu blok-okuma mekanizmasının kendi güvencesini sağlamamasıdır (4).
> Öneri: Komutu sonraki `####` başlığından önce kesen bir aralık okumasıyla değiştirin; ayrıca task bloğu denetimine bu komutun yalnız hedef bloğu döndürdüğünü sınayan bir test ekleyin.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: Başlık notu — “Okuma tablosu ve boyut”; T509
> Alıntı: "**Boyut hedefi:** bütün dosya 1,5 MB'ın altında; task bloğu en çok 2,5 KB"
> Sorun: T509’un sağlanan tam bloğu, tek başına 2,5 KB üst sınırını aşmaktadır. Buna rağmen aynı başlık notu, blok boyutu denetiminin çalıştığını ve "`11_task_blok_denetle.py --tam` sorunsuz koşar" der. Bu, task-okuma boyut güvencesi ile belgenin fiilî içeriği arasında çelişkidir (4); hedef task’ın sınırlı bağlamda güvenilir biçimde okunması garanti edilemez.
> Öneri: T509’u sınır içinde kalacak bağımsız alt task’lara bölün veya tekrar etmeyen doğrulama metnini ortak kurala taşıyın; ardından boyut denetimini gerçek UTF-8 bayt uzunluğunu ölçerek güncelleyin.
>
> SONUÇ: 2 BULGU

**(b) Parça 5 (§7–§9):**

> SONUÇ: TEMİZ

## 3. Bağımsız değerlendirme

Ham bulgular numaralandı: tek parça B-1…B-2, parça 1 B-1…B-5, parça 2 B-1…B-3, parça 3 B-1…B-2, parça 4 B-1…B-2 (14 ham). Tek parçanın B-1'i ile parça 1'in B-1'i aynı iddiadır (F1'de birleşti); öteki on iki bulgu ayrıdır → 13 ayrı bulgu (F1–F13). Sekiz `[dogrulama]` Opus / high doğrulayıcıya verildi: Yüksek bulguların hepsi tek tek (D1: F1; D2: F2; D3: F3; D4: F4; D5: F5); Orta F6–F9 aynı bölümden (F0/F1) dörtlü (D6); Orta F10 ve F11 aynı task kümesinden (T312/T209) ikili (D7); Orta F12 ve F13 başlık notu / T509 kümesinden ikili (D8). Tur 1–2'de reddedilen iddiaların (F1, F3, F5, F6, F9, F10) paketine önceki hüküm dosyası da konuldu ("yeni bilgi var mı"); F11 paketine T209'un 2. tur kısmi hükmü ve F10'a 1. tur F3 hükmü taşındı. Hükümler `11_audit_hukum_denetle.py` ile sınandı (sorun 0). Tablo: iki doğru, üç kısmi, sekiz çürük. Seviyeler yeniden okundu: F1, F3, F4, F5 Yüksek → Düşük; F2 Yüksek → Orta; F6, F9, F10, F11, F12, F13 Orta → Düşük; F7 ve F8 Orta kaldı.

| # | Bulgu (kaynak) | Karar | Gerekçe | Uygulama |
|---|---|---|---|---|
| F1 | F1 kapısı / T04 geri alma sınaması ↔ F1 göç task'larında `Down` yasağı (tek parça B-1 + parça 1 B-1; §3 F1, §6.2, T04, T103–T119) | ❌ RET (tekrar eden: 2. tur F1) | Çelişki yok: projede "geri alma" şemayı `Down` ile geri almak değil **önceki imaja dönmektir**; şema geri alınmaz ve bu yol CI adım 6'da sınanır — `05 §8.3.9`, `05` MK-74, `09 §10.6` madde 1, `09 §21.4` madde 3. `05` ve `09` 2. turdan beri değişmedi; yeni bilgi yok. Önerinin iki kolu da (her göçe `Down` ya da "geri alınabilir"i kaldırmak) MK-74 ve `09 §10.6` ile çelişirdi. Seviye Yüksek → Düşük. | Yok |
| F2 | T312 KK1 "değer imzasız ya da oturumsuz açılır" ↔ k1–k2 / KK2 (parça 2 B-1) | ⚠️ KISMİ | Alıntı doğru: KK1 kabul kriteri 1–2 ve KK2 ile çelişir; kaynak (`07 UC-37`) uç imzalı değerle açılır, imzası tutmayan 404 alır (`07` §4.31.1, `DownloadLinkInvalid`). Önerinin ikinci yarısı (her 404 nedeni için ayrı test) gereksiz: KK2 üç 404 nedenini zaten ayrı sınar. Kabul kriterleri doğru davranışı açıkça yazdığı için yanlış kod yazılmaz, yalnız doğrulama tıkanır; seviye Yüksek → Orta. Yan yüz yok. | 1 çift: T312 KK1 "imzalı değer oturumsuz açılır" (T312 2.540 bayt) |
| F3 | T330 k7 "ETBİS kaydı yapılmaz … satış kapısını tutmaz" mevzuata aykırı (parça 2 B-3) | ❌ RET (tekrar eden: 2. tur F6) | Aynı iddia, aynı gerekçe; ikinci modelin getirdiği Ticaret Bakanlığı sayfası yükümlülüğün hizmet sağlayıcıya (firmaya) ait olduğunu teyit eder, bu `02`de ve `10`da zaten böyle yazılıdır: `02 §3.1.4` (kayıt firmanın yükümlülüğüdür; alan satış kapısını ve kurulum listesini tutmaz, ürün kaydın varlığını denetleyemez), `02 §12.1.4`, `02 §12.5` tablosu, `10` ÖK-5; K-14, K-574, K-628. Firmanın ön koşulu T601 KK9'da (ÖK-1…ÖK-12) plandadır. Satışı ETBİS'e bağlamak `02 §3.1.4`e aykırı olurdu. Seviye Yüksek → Düşük. | Yok |
| F4 | T372 k8 kargoya verilmemiş kalemde cayma düğmesi yok, bildirim "müşteriyle anlaşıldı" iptaliyle işlenir; Mesafeli Sözleşmeler Yönetmeliği m.9/2, m.11 (parça 3 B-1) | ❌ RET | T372 k8, `02`'nin bilinçli kararını aynen taşır (K-604; `02 §7.3.7`, `§10.4.10`, `§6.4.31`; `04` ve `06`'da eşlenik). Doğrulayıcı güncel Yönetmelik metnini (m.9/2, m.11/1, m.12/2) mevzuat.gov.tr'den okudu: hak kaybı yok — iptal firmanın takdirine bağlı değil, ödenen para kargo ücretiyle iade edilir, yasal 14 gün bildirimin ulaştığı tarihten işler. Mevzuata aykırılık yok. Seviye Yüksek → Düşük. | Yok |
| F5 | T368 k6 yalnız kanuni faiz uyarısı gösterir, faiz hesaplanmaz; Yönetmelik m.16/3 (parça 3 B-2) | ❌ RET (tekrar eden: 2. tur F9) | Aynı iddia; yeni bilgi yok. Faiz yükümlülüğü satıcı firmaya aittir ve üst doküman bunu bilinçli karar olarak firmaya verir: sistem faizi hesaplamaz, yalnız uyarır (`02 §7.2.3`, `§12.5` tablosu; K-499, K-571; `06` faiz alanı yok). T368 k6 kaynak cümlenin aynısıdır; T369/T370'e faiz hesabı eklemek üst dokümana aykırı kod yazdırırdı. Seviye Yüksek → Düşük. | Yok |
| F6 | T117 k3 "`Undelivered`'da yalnız gönderildikten sonra geri dönen satırda doludur" ↔ KK3 "dolu da boş da kabul edilir" (parça 1 B-2) | ❌ RET (tekrar eden: 1. tur F2) | Aynı iddia; yeni bilgi yok. k3 iki hâli birlikte tarif eder (hiç gönderilmemiş `Undelivered` satırda boş, gönderilip geri dönende dolu); `06 §4.58` tam olarak bunu yazar; KK3 ikisini sınar. Önerinin "gönderim anı zorunlu" kolu `06 §4.58`e aykırı olurdu. Seviye Orta → Düşük. | Yok |
| F7 | T102 "Test beklentisi: Unit" ↔ KK2/KK3 PostgreSQL ister (parça 1 B-3) | ✅ KABUL | KK2 (örnek kayıt veritabanında metin) ve KK3 (izinli olmayan metin kısıtı ihlal eder) gerçek veritabanı ister; `09 §10.1` madde 5 "Enum testi (gerçek veritabanı)" ve `09 §12` madde 8 enum-kısıt eşleşmesini entegrasyona koyar, `09` `UnitTests` veritabanına dokunmaz. Kabul kriteri ↔ test beklentisi uyumsuzluğu (4). KK5 (kilit dosyası) Unit kalır. Yan yüz: 13 `Unit` satırı tarandı, T07/T08/T120/T213 veritabanı kısıtı sınamaz. Seviye Orta. | 2 çift: T102 `Test beklentisi: Unit, Integration` (satır tekil değil, blok bağlamıyla elle); KK3 sonuna "KK2 ve KK3 gerçek PostgreSQL'de entegrasyon testidir" (T102 1.725 bayt) |
| F8 | T05 k2/k4 yedek aracını da kapsar, KK2/KK4 yalnız ters vekili / veritabanı konteynerini sınar (parça 1 B-4) | ✅ KABUL | k2 "ters vekil ve yedek aracı veritabanına ulaşamaz", k4 "veritabanı ve yedek aracının dışarıya çıkışı `DOCKER-USER` zincirinde hedef adresli kuralla sınırlıdır" (`05 §3.2.5`); KK2 yalnız ters vekili, KK4 yalnız veritabanı konteynerini sınar. Blok konvansiyon 7 (satır 168) her kabul kriteri için doğrulama maddesi ister; kriterin yedek aracı yarısı doğrulanmadan kalıyordu (4). Düzeltme yalnız iki KK satırındadır. Seviye Orta. | 2 çift: T05 KK2 ve KK4 yedek aracını sayar (T05 2.548 bayt) |
| F9 | T02 k8 kaynak `09`un aynı PR'da değiştirilmesini emreder ↔ "tüketici, geriye etki yaratmaz" (parça 1 B-5) | ❌ RET (tekrar eden: 1. tur F4) | Aynı iddia; yeni bilgi yok. Başlık notu planın kendi düzeltmelerini kapsar; "aynı PR'da düzelt" yükümü planın değil kaynağın kendi buyruğudur (`09 §4.1` madde 1, yazım konvansiyonu 3); tüketici plan kaynağın kuralını işaretler. Öneri 11'i `09 §4.1` ile çelişir kılardı. Seviye Orta → Düşük. | Yok |
| F10 | T312 k8 "Content-Range yoksa ya da toplamı aşarsa 400" ↔ KK8 "toplamı aşan parça 409" (tek parça B-2) | ❌ RET (tekrar eden: 1. tur F3) | Aynı iddia; yeni bilgi yok. k8 toplam aşımını tek başına 400'e bağlar; KK8'in 409 örneği "denetim sırası:" altında iki koşulu birden taşıyan parçayı (uymayan başlangıç **ve** toplam aşımı) sınar ve `07`'de 409 denetimi boy aşımı 400'ünden önce gelir (`07` satır 4886). Önerilen ayrım ("geçersiz toplam 400") kaynakla da uyuşmaz: `Content-Range` toplam alanı yok sayılır (`07` satır 5308). T548 409'da sürer, 400'de durur — `07 UC-83` ile aynı. Seviye Orta → Düşük. | Yok |
| F11 | T209 KK1 "iki kez gönderilen aynı formun etkisi tektir" ↔ k1'in "ikinci kez işlenir" saydığı işlemler (parça 2 B-2) | ⚠️ KISMİ | 2. turda sepet kolu KK1'den çıkarılınca KK1 koşulsuz kaldı; k1 ve `07 §2.5.1`'in ikinci kez işlenir dediği işlemlerle (dört işlem, panelin yeni kayıt formları, dışa aktarma) harfiyen çelişir (4). Önerinin ikinci kolu (istisnalar için T209'da ayrı testler) çürük: k1 bu işlemleri kendi task'larına devreder ve orada sınanır (T503, T506, T518, T526); blok 2.559 bayttı, ekleme sığmazdı. Yanlış davranış kodlanmaz (k1 açık), yalnız doğrulayıcının ölçütü belirsizdir; seviye Orta → Düşük. | 1 çift: T209 KK1 "k1 listesi dışındaki formun etkisi tektir" (T209 2.556 bayt) |
| F12 | Başlık notu "Okuma tablosu": `grep -n -A 30 '^#### T123: '` sonraki `####` başlığında durmaz, sonraki task'ın satırlarını da basar (parça 4 B-1) | ⚠️ KISMİ | Alıntı doğru: `grep -A` sabit 30 satır basar; T509 (27 satır) komutla T510'un başlık, Bağımlılık ve Dokümanlar satırlarını da getirir (denendi) — başlık notunun "tek blok, sonraki `####` başlığında biter" ve "yalnız kendi bloğu okunur" cümleleriyle çelişir (4). Kesilme riski yok (en uzun blok 27 satır) ve sızan satırlar başlıkla ayrılır; etki düşük. Test ekleme önerisi ölçü dışı (derinlikte savunma). Seviye Orta → Düşük. | 1 çift: satır 34 komutu `awk` ile değişti (sonraki `##`…`####` başlığında durur, satır numarasını basar; T509 ve T01'de denendi) |
| F13 | T509 bloğu 2,5 KB sınırını aşar ↔ "blok denetimi sorunsuz koşar" (parça 4 B-2) | ❌ RET | T509 bloğu 2.556 bayttır; sınır 2.560 bayt (2,5 × 1024) ve `11_task_blok_denetle.py` zaten UTF-8 bayt ölçer; `--tam` "TOPLAM SORUN: 0" verdi. İkinci model 2,5 KB'ı 2.500 bayt saymış olmalı: birim yorumu, ölçü dışı. Seviye Orta → Düşük. | Yok |

**Kısmi kalış:** F2 önerisinin yalnız KK1 kolu uygulandı (ikinci yarı gereksiz); F11'de yalnız KK1 yeniden yazıldı (T209'a ayrı test eklenmedi); F12'de yalnız komut değişti (test eklenmedi). **Tekrar eden RET (turun sonucunu belirlemez):** F1 (2. tur F1), F3 (2. tur F6), F5 (2. tur F9) — 2. turda gerekçeyle reddedilen üç iddia aynı yer ve aynı iddiayla döndü; F6 (1. tur F2), F9 (1. tur F4) ve F10 (1. tur F3) 1. turda reddedilmişti ve bu turda ikinci kez aynı gerekçeyle reddedildi; böylece altı iddia önceki turlarda gerekçeyle reddedilmişti, hepsi aynı yer ve aynı iddiayla döndü. Sekiz retten yedisi doğrulayıcıda kaynakla kanıtlandı (`05 §8.7.3`/MK-74, `09 §10.6`, `02 §3.1.4`/K-628, `02` K-604, `02 §7.2.3`/K-499, `06 §4.58`, `07` sıra satırları, `09 §4.1`); "ben böyle düşünüyorum" gerekçesi yoktur. F4'te yasal iddia resmî metne (mevzuat.gov.tr, Mesafeli Sözleşmeler Yönetmeliği m.9/2, m.11/1, m.12/2) karşı okundu.

## 4. Ek bulgular

- **EK-1 (D8'in yan gözlemi; `11`'i etkilemez, uygulanmadı):** `scripts/doc-scans/11_ozet_ayrinti_grupla.py` satır 36'daki paket metni de `grep -n -A 30 '^#### T123: '` komutunu taşır (F12'nin eski komutu). Betik `11` değildir ve kalite döngüsünün geçmiş paketlerini üretmiştir; döngü bitince etki yansıtmada gözden geçirilir.

## 5. Kullanıcı onay checklist'i

- [ ] F1 — RET, tekrar eden (uygulanmadı)
- [x] F2 — T312 KK1 imzalı değeri sınar (K-1480)
- [ ] F3 — RET, tekrar eden (uygulanmadı)
- [ ] F4 — RET (uygulanmadı)
- [ ] F5 — RET, tekrar eden (uygulanmadı)
- [ ] F6 — RET, tekrar eden (uygulanmadı)
- [x] F7 — T102 test beklentisi Unit, Integration; KK3 entegrasyonu işaret eder (K-1480)
- [x] F8 — T05 KK2 ve KK4 yedek aracını da sınar (K-1480)
- [ ] F9 — RET, tekrar eden (uygulanmadı)
- [ ] F10 — RET, tekrar eden (uygulanmadı)
- [x] F11 — T209 KK1 k1 listesi dışındaki formla sınırlı (K-1480)
- [x] F12 — okuma tablosu komutu `awk` ile sonraki başlıkta durur (K-1480)
- [ ] F13 — RET (uygulanmadı)

Gösterilmeden kaydedilen ⚠ karar yok; karar gerektiren bulgu yok; çok kritik soru yok. Önerilen `00`/`CLAUDE.md`/`SETUP.md` değişikliği yok.

## Sınıf taraması (Faz 4 madde 6)

Art arda iki turda kabul edilen orta bulgular aynı sınıftaydı: 2. tur F8 (T209 KK8) ve EK-1 (T212 KK5), 3. tur F2 (T312 KK1), F7 (T102 test beklentisi), F8 (T05 KK2/KK4) ve F11 (T209 KK1) — hepsi **doğrulama kontrol listesi (KK) ile kabul kriterinin / test beklentisinin kapsam ve çelişki farkı**. `sinif_tara.py` ile 215 task bloğu tarandı: (a) kabul kriteri sayısı ile KK sayısı farkı — yalnız şablon bloğu (`TXX`, bilinçli); (b) "Test beklentisi: Unit" olup kriter ya da KK'sinde veritabanı sözcüğü geçen task — 3 aday (T120, T538, T546), üçü de olumsuz cümle ("veritabanına dokunmaz", "kontrol kısıtı eksilince test kırılır" birim testidir), sorun 0. Betik yalnız yapısal biçimi yakalar; "kriter bir varlığı sayar, KK onu saymaz" (T05) türü anlamsal fark betikle taranamaz. **Kalan risk:** anlamsal mercek (fazlara bölünmüş, her faz için bir Sonnet `low` ajanı) bu turda koşmadı — tek ajan 750 KB'lık dokümanı bağlamına sığdıramaz; 4. tur koşmadan önce faz başına mercek koşturulması önerilir (aşağıdaki sıradaki adım).

## Sonuç

**SONUÇ: TEMİZ DEĞİL — 13 ayrı bulgu (14 ham); 2 kabul + 3 kısmi (7 şartname çifti), 8 ret (altısı önceki turlarda reddedilmiş iddianın dönüşü, tekrar eden RET).** Skill'in çıkış kuralı: bütün çağrılar aynı anda TEMİZ dönmedi (altı çağrının beşi bulgu getirdi, parça 5 TEMİZ); kabul edilen bulgular arasında mevzuata aykırılık ya da para/hak kaybı sınıfından olan yok (hepsi sınıf (4), seviye Düşük/Orta; Orta olan üçü KK ↔ kriter sınıfındandır); 5. tur eşiği henüz gelmedi. Sonraki tur: 4. tur (`11_CROSS_REVIEW_R4.md`), aynı betikler ve aynı parça sınırlarıyla, `11` v0.40 üzerinde; koşmadan önce KK ↔ kriter sınıfı için faz başına anlamsal mercek önerilir. Etki yansıtma (Faz 5) döngü TEMİZ olunca ayrı adımdır; bu turda yapılmadı.
