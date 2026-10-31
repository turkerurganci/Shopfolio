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


## Hedefli mercek (Faz 4 madde 6)

**Tarih:** 2026-10-31 · **Hedef:** `Docs/11_IMPLEMENTATION_PLAN.md` v0.40 → v0.41 (girdi commit'i `60f5c52`) · **Sınıf:** task bloğunun kendi içindeki fark — kabul kriteri ↔ doğrulama kontrol listesi (KK) ↔ test beklentisi ↔ `Dokümanlar` alanı ↔ bağımlılık (2. ve 3. turların orta kabulleri: R2 F4, F8, EK-1; R3 F2, F7, F8, F11). Önceki bölümdeki "Sınıf taraması" betikle yapısal farkı taradı ve anlamsal merceği 4. tur öncesine bıraktı; bu bölüm o merceği koşar. Sonuç: **ikinci modele (Codex) çıkılmadı**; mercek ve doğrulayıcılar alt ajandır.

### Koşum

`paketle.py` ile 214 task bloğunun tamamı (yalnız `#### Txxx` blokları + başlık notunun yazım konvansiyonları satırı + `## 5` girişi; satır numaralı `11:<n>|`) faz dilimlerine bölündü (her paket ≤ 85 KB; F3 üç dilim, F5 iki dilim). Her dilime bir `[ozet]` Sonnet / low mercek (talimat 3,5 KB'ın altında): yalnız bu sınıf, örnekler, önceki turlarda RET edilen iddiaların listesi ("yeniden getirme"), bulgu biçimi (seviye, yer, iki satırın aynen alıntısı, öneri), bulguyu bulduğu anda kendi dosyasına yazar, dolgu yok. İki beşli dalga, ön planda. Kapsanmayan task yok (betik: 214 / 214).

| Dilim | Task | Task sayısı | Ham bulgu (Orta · Düşük) |
|---|---|---|---|
| F0_01 | T01–T10 | 10 | 5 (0 · 5) |
| F1_01 | T101–T120 | 20 | 2 (1 · 1) |
| F2_01 | T201–T233 | 33 | 24 (1 · 23) |
| F3_01 | T301–T331 | 31 | 16 (10 · 6) |
| F3_02 | T332–T361 | 30 | 10 (1 · 9) |
| F3_03 | T362–T390 | 29 | 11 (0 · 11) |
| F4_01 | T401–T410 | 10 | 7 (0 · 7) |
| F5_01 | T501–T529 | 29 | 24 (0 · 24) |
| F5_02 | T530–T549 | 20 | 8 (0 · 8) |
| F6_01 | T601–T602 | 2 | 2 (0 · 2) |
| **Toplam** | | **214** | **109 (13 · 96)** |

**Ham → tekil:** 109 ham bulgu = 109 tekil bulgu (yinelenen yok; `11_audit_hukum_denetle.py` "yinelenen bulgu hükmü: yok"). Mercek seviyeleri: Yüksek 0, Orta 13, Düşük 96. Bulguların büyük çoğunluğu sınıfın "kriter ↔ `Dokümanlar`" ayağındadır (kriterin dayandığı bölüm `Dokümanlar` alanında yok); "kriter ↔ bağımlılık" ayağı Orta kümenin çoğunu verdi (T303, T321, T322, T346, T349, T215). Mercek ajanı (Sonnet low) 13 yerde Orta dedi; doğrulayıcıda üçü Düşük'e indi, hiçbiri yükselmedi.

### Doğrulayıcılar

17 `[dogrulama]` Opus / high doğrulayıcı (bekçi sınırı 20; bekçi olayı yok), kademeli: Orta 13 bulgu beş doğrulayıcıda (D1 T103; D2 T215; D3 T303·T305·T309·T310; D4 T315·T316·T320; D5 T321·T322·T324·T346 — aynı fazın aynı bölümünden 1–4'lü), Düşük 96 bulgu on iki doğrulayıcıda (D6–D17, sekizerli, task sırasıyla). Doğrulayıcı talimatı: `kd-cross/r3/talimat_dogrulayici.md` biçimi, mercek ajanına uyarlanmış (ölçü dört sınıf; `Dokümanlar` için okuma kuralı ve R2 F4 emsali). Hükümler `11_audit_hukum_denetle.py` ile sınandı; ilk koşumda iki şartnamenin ESKI satırı (`- **Bağımlılık:** —` ve `- **Bağımlılık:** T315`) dosyada birden çok geçtiği için iki satırlık ESKI (başlık + satır) yazımına çevrildi (sorun 2 → 0). Yükselen yok (`yeniden.md` boş); sığmayan yok (`dogrulanmadi.md`: 0).

| D | Kademe | Bulgu | Doğru | Kısmen | Çürük |
|---|---|---|---|---|---|
| D1 | Orta | 1 | 0 | 0 | 1 |
| D2 | Orta | 1 | 0 | 1 | 0 |
| D3 | Orta | 4 | 3 | 1 | 0 |
| D4 | Orta | 3 | 2 | 1 | 0 |
| D5 | Orta | 4 | 2 | 1 | 1 |
| D6 | Düşük | 8 | 6 | 0 | 2 |
| D7 | Düşük | 8 | 0 | 3 | 5 |
| D8 | Düşük | 8 | 1 | 2 | 5 |
| D9 | Düşük | 8 | 5 | 2 | 1 |
| D10 | Düşük | 8 | 5 | 0 | 3 |
| D11 | Düşük | 8 | 0 | 0 | 8 |
| D12 | Düşük | 8 | 3 | 1 | 4 |
| D13 | Düşük | 8 | 6 | 2 | 0 |
| D14 | Düşük | 8 | 1 | 0 | 7 |
| D15 | Düşük | 8 | 4 | 3 | 1 |
| D16 | Düşük | 8 | 0 | 1 | 7 |
| D17 | Düşük | 8 | 1 | 2 | 5 |

**Hüküm: 39 doğru · 20 kısmen · 50 çürük** (109). Mercek seviyesi Orta olan 13 bulgunun 10'u Orta kaldı (7 doğru, 3 kısmen), 3'ü Düşük'e indi (M06 çürük; M34 kısmen; M40 çürük). Düşük 96 bulgunun 49'u kabul (32 doğru · 17 kısmen), 47'si çürük. **Yüksek bulgu yok; mevzuata aykırılık, para ya da hak kaybı doğuran bulgu yok:** kabullerin hepsi sınıf (4) (dokümanın iki yerinin farkı; `Dokümanlar` alanı eksikliği, bağımlılık sırası, KK ↔ kriter). Çürüklerin çoğu "ölçü dışı": atıf yapılan bölüm genel okuma kuralıyla ya da task'ın §7.2 matris satırı üzerinden zaten okunuyor, ya da kural ayrıntısı kaynak cümlede kimlikle işaretli (K-1445) ve `Dokümanlar` alanı alana özgü birimi zaten taşıyor.

### Uygulanan

59 bulgu kabul/kısmen (**78 şartname çifti**, `11_audit_uygula.py`, sorun 0; **+ 1 elle**: T377 bloğu 2.564 bayta çıktığı için k5'in "kart açık" yarım cümlesi kaldırıldı — aynı anlam "yalnız havaleyi kapatır" cümlesinde ve KK5'te duruyor, kayıp K-1481'de). Betikle sayım: **59 task bloğu değişti** — 50'sinde `Dokümanlar` alanına eksik kaynak bölüm eklendi (T03, T04, T06, T09, T119, T202, T204, T210, T213, T214, T220, T223, T226–T229, T301, T302, T305, T306, T309, T310, T312, T315, T316, T320, T345, T377, T384, T388, T401–T405, T407, T410, T501, T502, T511–T517, T537, T540, T543, T601; bir kısmında ayrıca kriter/KK cümlesi hizalandı); 4'ünde bağımlılık (T303 → T342, T322 → T308, T346 → T338, T349); 5'inde yalnız kriter/KK metni (T215 k6–k7 ve KK7, T230, T314, T321 KK5, T507). **Blok bayt:** `11_task_blok_denetle.py --tam` TOPLAM SORUN 0 (sınır 2.560); T377 son hâli sınır içinde. Matris (§7.1, §7.2) değişmedi.

**Üst doküman notları (uygulanmadı, 11 tüketicidir):** (1) `06 §8.2.3` zorunlu ayar listesi `05 §8.4.6` ile eşitlenmeli (hesap bağlantılarının türetme sırrı); (2) `09 §18.1` madde 8 denetim yazıcısı için düzey yükseltilince satır yazımı. İkisi de `00`/`CLAUDE.md`/`SETUP.md` değişikliği değildir; etki yansıtmada (Faz 5) ele alınır.

### Hüküm tablosu

Sütunlar: mercek seviyesi → hüküm (KABUL = doğru, KISMI = kısmen, RET = çürük) → doğrulayıcının seviyesi; `Çift` = o bulgunun şartname çifti sayısı.

| No | Dilim | Task | Mercek seviyesi | Hüküm | Seviye | Çift | Bulgu (özet) | Gerekçe (özet) |
|---|---|---|---|---|---|---|---|---|
| M01 | D6 | T03 | Dusuk | KABUL | Dusuk | 1 | k5 `09 §10.6`'ya atıf yapar, `Dokümanlar`'da `09` §10 yok. | T03 k5 göç demetini ve veritabanı bağlamını açar ve `09 §10.6`'ya dayanır (11:231); `Dokümanlar` (11:225) ve §7.2 ... |
| M02 | D6 | T04 | Dusuk | KABUL | Dusuk | 1 | k1 `05 §2.3.5` ve k4 (11:256) `05 §2.3.6` atıf yapar; `Dokümanlar`'da `05 §2.3` yok. | k1 `05 §2.3.5`'e, k4 yalnız `05 §2.3.6`'ya dayanır (11:253, 11:256); `Dokümanlar` (11:251) ve §7.2 satırı (11:7511) `05 ... |
| M03 | D6 | T06 | Dusuk | KABUL | Dusuk | 1 | k8 `05 §9.4.1`'e atıf yapar, `Dokümanlar`'da `05 §9.4` yok. | k8'in "canlılık ucu 200" ve uygulamanın veritabanına bağlanma koşulu `05 §9.4.1`'dedir (05 §9.4.1: uygulama ve ... |
| M04 | D6 | T09 | Dusuk | KABUL | Dusuk | 1 | k4 `07 UC-81`'e atıf yapar, `Dokümanlar`'da `07` girdisi yok. | k4 referans kurulumda canlılık ucunu sınar ve `07 UC-81`'e dayanır (11:374); ucun adresi (`GET /sistem/canlilik`) ... |
| M05 | D6 | T09 | Dusuk | RET | Dusuk | 0 | k5 `05 §8.6.3`'e atıf yapar, `Dokümanlar`'da `05 §8.6` yok. | k5'in kuralının tamamı `05 §8.2.3`'tedir — "Güncelleme turu referans kurulumda denetlenen imajların özetini kaydeder ve ... |
| M06 | D1 | T103 | Orta | RET | Dusuk | 0 | K7 `09 §10.8`'e atıf yapıyor ama `Dokümanlar` yalnız `09` §10 (§10.6) listeliyor; §10.8 ... | T103 `Dokumanlar` alani `09` §10'u zaten yaziyor (11:461); konvansiyona gore ek bolumler okuma tablosunun birimiyle ... |
| M07 | D6 | T119 | Dusuk | KABUL | Dusuk | 1 | K8 `06 §8.3`'e atıf yapıyor, `Dokümanlar` yalnız `06 §8.1` ve `§8.4 (2)` listeliyor. | k8'in "ilk yönetici ve kurulum ayarları kuruluma aittir" kolu `06 §8.3`'tendir (06:5899, §8.3.1–§8.3.2) ve kriter onu ... |
| M08 | D2 | T215 | Orta | KISMI | Orta | 3 | k6 limitlerin ve eksenlerin evini sonraki fazdaki T376–T377 yapar, k7/KK7 ise o ... | T215 F2'dedir (11:107), siparis ve L-7/L-8 sayimi F3'tedir (T348 11:2886, T376–T377 11:3562, 11:3588); k6 eksenlerin ... |
| M09 | D6 | T202 | Dusuk | KABUL | Dusuk | 1 | k5 `08 §2.1.8`'e atıf yapar, Dokümanlar yalnız `08 §3.1` sayar. | k5'in "ödemeyi başlatmada bağlantı hatası" kolu `08 §3.1` kural 3'ün genel "sonucu bilinmeyen" kuralının istisnasıdır; ... |
| M10 | D6 | T203 | Dusuk | RET | Dusuk | 0 | k9 `07` bölümlerine atıf yapar, Dokümanlar'da hiç `07` girdisi yok. | k9'un dayandığı kural `08 §3.2` ilke 5'tedir ve `Dokümanlar`'da sayılıdır (11:925): sahteciliğe karşı koruma istisnası, ... |
| M11 | D7 | T204 | Dusuk | KISMI | Dusuk | 1 | k2, k3, k4, k6 `08 §3.1`, `§2.1.8`, `§2.2.2`, `§2.3` bölümlerine atıf yapar; Dokümanlar ... | k2'nin "ikinci jeton reddi" sonuc sinifi, k3'un "katman siparise dokunmaz"i (kural 4) ve k4'un alici/kanal hatasi ... |
| M12 | D7 | T205 | Dusuk | RET | Dusuk | 0 | k4 `05 §9.1.3`'e, k7 `05 §8.4.6`'ya atıf yapar; Dokümanlar'da `05` girdisi yok. | Iki atif da baska task'in evine isarettir, kriter onlarin icerigine dayanmaz: k4'te `05 §9.1.3` (05:1191, uygulama ... |
| M13 | D7 | T206 | Dusuk | RET | Dusuk | 0 | k7 `07 §2.7.1`'e atıf yapar, Dokümanlar'da `07 §2.7` yok. | k7'nin "durum kodlari `07 §2.7.1`'dedir" ibaresi okunan `07 §2.2.2`'nin kendi geri atfidir (07:3695 "(durum kodlari ... |
| M14 | D7 | T208 | Dusuk | RET | Dusuk | 0 | k1 `06 §4`'e atıf yapar, Dokümanlar'da `06` girdisi yok. | k1 okunan `07 §2.4.1`'in (07:3737) kopyasidir; kaynak cumle sutun adini kendisi verir ("kaydin `Version`'i, ayarlarda ... |
| M15 | D7 | T210 | Dusuk | KISMI | Dusuk | 1 | k8 `05 §3.7.4`'e atıf yapar, Dokümanlar'da `05` girdisi yok (k5 ayrıca `04` 7.3.2.12 için ... | k5/KK5'in "tarih araliginin alan kurali" icerigi — bitis baslangictan once olamaz; ileri tarih indirim, kupon ve ... |
| M16 | D7 | T211 | Dusuk | RET | Dusuk | 0 | k4 ve k6 `06 §4`, `§5`'e atıf yapar, Dokümanlar'da `06` girdisi yok. | k6 okunan `07 §5.1.2`'nin (07:4954) kopyasidir — kural ("alanin adi `06 §4`'te karsiligi varsa o alanin adidir; kapali ... |
| M17 | D7 | T212 | Dusuk | RET | Dusuk | 0 | k4 `07 §2.4.6` ve `§2.4.3`'e atıf yapar, Dokümanlar'da `07 §2.4` yok. | k4'un kurali — 409 ve iki istisnanin 303'u — okunan `07 §2.7.1`'de eksiksiz yazilidir (07:3807 "... bunun disindadir: ... |
| M18 | D7 | T213 | Dusuk | KISMI | Dusuk | 1 | k7 `02 §6`'ya, k2 `06 §5`'e atıf yapar; Dokümanlar'da `02` ve `06` girdisi yok. | Mercegin iki onerisi gereksiz: `02 §6`'yi KK7'nin kapsama betigi mekanik okur (07:5146), uygulayici 141 satiri okumaz; ... |
| M19 | D8 | T214 | Dusuk | KABUL | Dusuk | 1 | k2, k3 `07 §2.2.3`'e, k5 `07 §6.1.1`'e atıf yapar; Dokümanlar yalnız `07 §6.3` sayar. | T214 k3'ün tek dayanağı `07 §2.2.3`tür (07:3697 "yeni çerez ve sunucuda geçici kayıt açılmaz"); bu cümle `07 §6.3`te ... |
| M20 | D8 | T215 | Dusuk | RET | Dusuk | 0 | k8 `07 §2.9.7`'ye atıf yapar, Dokümanlar'da `07 §2.9` yok. | Alıntı doğru, ama `07 §2.9.7`nin içeriği (07:3861, tek cümle: değerler `02 §11`den okunur, uçta yeniden yazılmaz, ... |
| M21 | D8 | T220 | Dusuk | KISMI | Dusuk | 1 | k2, k9 `05 §3.6.3`'e, k2 `06 §4.59`'a, k6 `06 §9.4.4`'e atıf yapar; Dokümanlar'da `05 ... | Üç atıftan yalnız biri gerçek boşluktur: k2'nin "kilit sütunu yoktur" kuralı yalnız `06 §4.59` Job'un giriş ... |
| M22 | D8 | T221 | Dusuk | RET | Dusuk | 0 | k5 `06 §9.1.3`'e atıf yapar, Dokümanlar'da `06` girdisi yok. | k5'in kuralı ("işin zamanı kendi kaydında, ayrı takvim yok") okunan `05 §5.1.1`de aynen vardır (05:585 "İşin zamanı ... |
| M23 | D8 | T222 | Dusuk | RET | Dusuk | 0 | k6 `07 §2.5.3`'e, k3 `05 §6.8.2`'ye atıf yapar; Dokümanlar'da `07` yok ve `05` için ... | k6'nın kuralı okunan `05 §6.4.6`dadır (05:720 "tek bir koşullu güncellemeyle tüketilir … güncellenen satır yoksa … ... |
| M24 | D8 | T223 | Dusuk | KISMI | Dusuk | 1 | k1 `09 §19.1`'e, k3/k8 P-22/P-23/P-26'ya (`02 §11.2`), k9 `06 §9.1`'e atıf yapar; ... | Gerçek boşluk P satırlarıdır: k3 ve k8 P-22, P-23, P-26'ya dayanır, okunan `05 §6.6.1`/§6.6.3 (05:755, 05:759) yalnız ... |
| M25 | D8 | T224 | Dusuk | RET | Dusuk | 0 | k3 `02 §12.2.5`'e atıf yapar, Dokümanlar'da `02` girdisi yok. | k3'ün kapalı envanteri okunan `05 §6.7`nin tablosunda satır satır vardır (05:773–779 ve 6.7.12: iki oturum çerezi, form ... |
| M26 | D8 | T225 | Dusuk | RET | Dusuk | 0 | k3 `05 §6.8.3`'e atıf yapar, Dokümanlar'da `05 §6.8` yok. | k3'ün yasaklı alan listesinin tamamı okunan `05 §9.1.5`tedir (05:1194; `05 §9.1` Dokümanlar'da, 11:1471) ve k3 önce ona ... |
| M27 | D9 | T226 | Dusuk | KISMI | Dusuk | 2 | k4 `09 §18.1`'e, k5 `06 §8.2.3`'e atıf yapar; Dokümanlar'da `09 §18` ve `06` yok. | k4 `09 §18.1` madde 1'e dayanir (ILogger<T> kurali, 09:1904) ve §18 Dokumanlar'da (11:1493) da aday matris satirinda ... |
| M28 | D9 | T227 | Dusuk | KABUL | Dusuk | 1 | k6 ve k7 `02 §3.8.3`'e atıf yapar, Dokümanlar'da `02` girdisi yok. | k6 ve k7 (11:1520, 11:1521) `02 §3.8.3`'e atif yapar; Dokumanlar (11:1513) ve matris satiri (11:7564) `02` girdisi ... |
| M29 | D9 | T228 | Dusuk | KABUL | Dusuk | 1 | k3 `06 §2.6.4`'e, k9 `05 §3.6.3`'e atıf yapar; Dokümanlar'da `06` ve `05 §3.6` yok. | k3 `06 §2.6.4`'e (siparis surumunun kapsami ve artirmayan sistem yazimlarinin kapali listesi, 06:2759), k9 `05 ... |
| M30 | D9 | T229 | Dusuk | KABUL | Dusuk | 1 | k6 `05 §4.1.5`'e, k1 `05 §3.1.2`'ye atıf yapar; Dokümanlar'da `05 §4.1` ve `§3.1` yok. | k1 `05 §3.1.2`'ye (tek ornek; onbellegin dayanagi, 05:348), k6 `05 §4.1.5`'e (satis kapisi her istekte hesaplanir, ... |
| M31 | D9 | T230 | Dusuk | KISMI | Dusuk | 1 | k3 boyut sınırını sayar, KK3 yalnız dosya sayısını sınar; k4 "denetim ve komut yazıcısı ... | KK3 ve KK4 `09 §18.1` madde 8'in Test maddesini aynen izler (09:1924); boyut siniri gunluk 1 GB'ta devreye girer, tek ... |
| M32 | D3 | T303 | Orta | KABUL | Orta | 1 | Kriter 6 T342'nin işlevini kullanır; T342 bağımlılıkta yok. | T303 k6 şifre politikasını T342'nin kurduğu işlevle uygular (11:1740; `09 §13.2` madde 4, 09:1503 "web'in kullandığı ... |
| M33 | D5 | T322 | Orta | KABUL | Orta | 1 | F-2 T308'de doğar; T322 T308'e bağımlı değil (T320 aynı bildirim türlerinde T307/T308'i ... | T322 madde 8 ve KK8 (11:2251, 11:2262) dogusta F-2'nin yazilmasini sinar; kaynak da bildirimi talep durumuna baglar ... |
| M34 | D5 | T321 | Orta | KISMI | Dusuk | 1 | Kriter 5 ve KK5 (kapıyı çağırır, red durumu değiştirmez) T335'te doğan kapıya dayanır; ... | Gecerli yuz: T321 KK5 (11:2233) "red durumu degistirmez" der, ama reddin gercek kosullari T335 madde 3-4'tedir ... |
| M35 | D4 | T315 | Orta | KABUL | Orta | 1 | Kriter 1 `06 §5.2`'ye, kriter 3 `03 §1.2`'ye atıf yapar; ikisi de Dokümanlar'da yok. | Kriter 1 enum kod adlarini ve icerigin iki degerli alt kumesini `06 §5.2`'ye (06:4719-4787, 5.2.3 alt kume cumlesi) ... |
| M36 | D4 | T316 | Orta | KABUL | Orta | 1 | Kriter 1 `03 §1.6.2` ve `05 MK-35`'e atıf yapar; Dokümanlar'da ne `03 §1.6` ne `05` var. | Kriter 1'in "aynı birimin adlı işlemi" kismi `05 §4.1.1`/MK-35'e (05:485, 05:1481) dayanir ve T316 hicbir `05` birimi ... |
| M37 | D4 | T320 | Orta | KISMI | Orta | 1 | Kriter 7 `02` Z-18'e, kriter 1 `05 §4.1.2`'ye atıf yapar; Dokümanlar'da Z-18 ya da `05` ... | Iki atif da okuma kumesinde yok (11:2188): Z-18 (02:1140; sahibi T325, T374 — 11:6815) KK7'nin red sonucunu ("çözülmüş ... |
| M38 | D3 | T305 | Orta | KABUL | Orta | 2 | Kriter 7 `04 §8.2`'ye dayanır; Dokümanlar'da `04` hiç yok. | T305 k7 biçimi `04 §8.2`'ye bağlar (11:1793; 08:466 "`04 §8.2`'nin biçimleridir, aynen"), `Dokümanlar` (11:1785) ve ... |
| M39 | D3 | T309 | Orta | KISMI | Orta | 1 | Kriter 7 bağlantı ömrü değerini `02 §11`'den okutur; Dokümanlar'da §11 yok. | T309 k7 değeri `02 §11`'e bağlar (11:1903; 08 §2.2.8, 08:643), `Dokümanlar` (11:1895) ve §7.2 satırı (11:7579) §11'i ... |
| M40 | D5 | T324 | Orta | RET | Dusuk | 0 | Kriter 8 `02 §11.1` ve `§11.2`'ye atıf yapar; Dokümanlar'da `02 §11` yok. | Kriter 8'in dayandigi birimler `02 §11.1` ve §11.2'nin tablo satirlaridir ve bu satirlar kimlikleriyle Dokumanlar ... |
| M41 | D3 | T310 | Orta | KABUL | Orta | 1 | Kriter 3 `08 §4.1`'e, kriter 1 `06 §8.4.1`'e, kriter 6 `04` 5.16.8 ve `08 §2.2.6` B-5'e ... | Dört atıf da bloğun `Dokümanlar` alanında (11:1923) yok: k1 `06 §8.4.1` (06:5909 kargo listesi koddadır; §7.2 satırı ... |
| M42 | D9 | T310 | Dusuk | RET | Dusuk | 0 | Kriter 3 gerçek tarayıcı denemesi ister; Test beklentisi yalnız Unit. | Tarayici denemesi otomatik test degil, veri dogrulamasidir; sonucu DV-32 ve DV-33 satirina yazilir (11:1927) ve KK3 ... |
| M43 | D9 | T312 | Dusuk | KABUL | Dusuk | 1 | Kriter 10 `07 §4.32.2`'ye atıf yapar; Dokümanlar'da yok. | k10 `X-Robots-Tag: noindex` kuralini `07 §4.32.2`'ye dayandirir (11:1989); kuralin evi orasidir (07:4904), UC-83 metni ... |
| M44 | D9 | T306 | Dusuk | KABUL | Dusuk | 2 | Kriter 7 `08 §2.4`'e atıf yapar; Dokümanlar'da yalnız `08 §2.2.6` var. | k7 `08 §2.4`'e atif yapar (11:1819); B-5'in kargo blogu baglanti ve listeden cikmis sirket kuralini §2.4'e baglar ... |
| M45 | D10 | T301 | Dusuk | KABUL | Dusuk | 2 | Kriter 9 `06 §9.3.1`'e, kriter 1 `06 §9.1`'e atıf yapar; Dokümanlar'da yalnız `06 §9.2` ... | Alintilar dogru (11:1681, 11:1683, 11:1691); k1 surelerin baslangic degerini (Z-28 yil sonu, Z-41 O2 gecis ani) `06 ... |
| M46 | D10 | T302 | Dusuk | KABUL | Dusuk | 1 | Kriter 1 `06 §4.64`'e, kriter 2 `06 §9.1` Z-43'e atıf yapar; Dokümanlar'da yok. | Alintilar dogru (11:1707, 11:1709, 11:1710); T302'nin butun surelerinin baslangic degeri ve dizini (Z-29, Z-33 ... |
| M47 | D10 | T314 | Dusuk | KABUL | Dusuk | 1 | Kriter 9'un "en eski satır on beş dakikadan eski" kolu KK9'da sayılmaz. | `05 §9.4.16` kosulu iki kollu: zamani gelmis en eski islenmemis satir on bes dakikadan eski YA DA "basarisiz" satir var ... |
| M48 | D5 | T346 | Orta | KABUL | Orta | 4 | KK7 kargo, eşik ve kupon asgarisi davranışını sınar; kargo ve eşik T349'da (T346'ya ... | T346 KK7 (11:2861) Tukendi kalemin kargoya, esige ve kupon asgarisine girmedigini sinar (kaynak 02:761 "kuponun asgari ... |
| M49 | D10 | T349 | Dusuk | KABUL | Dusuk | 1 | Kriter ve KK2/KK7 kuponla sınanır; kupon T338'de doğar ve T349 bağımlılığında T338 yok. | k2, k7, KK2, KK7 kuponla sinanir (11:2915, 11:2920, 11:2926, 11:2931); kupon T338'dedir (11:2634) ve T349'un ... |
| M50 | D10 | T335 | Dusuk | RET | Dusuk | 0 | Varyantın dosyaya bağı T340 k2'de doğar ("varyant kendi dosyasını taşıyabilir") ve T340, ... | Dosya bagi T340'ta dogmaz: `DigitalFile`'in urun/varyant bagi F1'de T105'te kurulur (11:514 k6, matris 11:6027 ... |
| M51 | D10 | T336 | Dusuk | RET | Dusuk | 0 | KK8 ülkenin fiziksel ürüne zorunlu, dijitale isteğe bağlı olduğunu sınar; k8 zorunluluk ... | KK8'in "fiziksel urunde ulke zorunlu, dijitalde istege bagli" ifadesi `02 §3.8.5`'in cumlesidir (02:605); k8 ayni ... |
| M52 | D10 | T345 | Dusuk | KABUL | Dusuk | 1 | k1 `03 §9.4.2`'ye atıf yapar; `Dokümanlar`'da `03` hiç yok. | k1'in "taninan tarayici isaretleri gecersizlesir, bekleyen e-posta degisikligi duser" kolu `02 §3.15.1`'de yoktur ... |
| M53 | D10 | T347 | Dusuk | RET | Dusuk | 0 | k5 `05 §11.1.2`'ye atıf yapar; `Dokümanlar`'da `05` yok. | k5'teki `05 §11.1.2` atfi isaret cumlesidir: "elli kalem sinir degil performans kabuludur; degeri `05 §11.1.2`, ... |
| M54 | D11 | T350 | Dusuk | RET | Dusuk | 0 | k5 `04` 5.4.5'e atıf yapar; `Dokümanlar`'da `04` yok. | k5 ve KK5'in dayandigi bicim okunan `02 §3.20.4`'tedir: "vitrinde '2 iş günü içinde kargoya verilir' biçiminde ... |
| M55 | D11 | T351 | Dusuk | RET | Dusuk | 0 | k2 `02 §5.6.1`'e atıf yapar; `Dokümanlar`'da §5.8 var, §5.6 yok. | k2'nin tamami okunan `02 §3.20.9`'un cumlesidir — "İşaret ancak ödeme onaylanmışken konur — ödeme Bekliyor ya da ... |
| M56 | D11 | T353 | Dusuk | RET | Dusuk | 0 | k1 `02 §3.1.5`'e, k2 (satır 3013) L-8'e atıf yapar; ikisi de `Dokümanlar`'da yok. | `02 §3.1.5` satis kapisinin evidir ve T331'e aittir (11:2458, 11:5790); T331 k8 baglamayi T352 ve T353'e birakir ve ... |
| M57 | D11 | T361 | Dusuk | RET | Dusuk | 0 | k2 `02 §3.27.14`'e atıf yapar; `Dokümanlar`'da §3.27.x yok. | k2 bos hali acikca T359'a devreder ("hâli T359'dadır", 11:3193); `02 §3.27.14` T359'un alanindadir ... |
| M58 | D11 | T362 | Dusuk | RET | Dusuk | 0 | K5 `02 §3.34.5`'e atif yapar, `Dokumanlar`'da §3.34.5 yok. | k5 ve KK5'in butun icerigi — siyah/beyaz secimi, WCAG kontrasti yuksek taraf, her renkte en az 4,5:1, anlam renklerinin ... |
| M59 | D11 | T370 | Dusuk | RET | Dusuk | 0 | K2 `03 §2.8.4.1`'e atif yapar, `Dokumanlar`'da `03` yalniz §1.7. | k2'nin "caymada firma panelden işler" kolu okunan `02 §5.5`'tedir: "havale hattında, cayma ve iade sonrasında ... geri ... |
| M60 | D11 | T372 | Dusuk | RET | Dusuk | 0 | K4 ve K5 (`03 §7.3.24`) `03` bolumlerine atif yapar, `Dokumanlar`'da hic `03` girdisi yok. | k4/KK4'un icerigi okunan `02 §5.6.6`'dadir — beyan ekseni degistirmez, teslim isaretsiz kalemde cayilan adetleri ... |
| M61 | D11 | T372 | Dusuk | RET | Dusuk | 0 | KK6 "pencere dışı onay" ve "bir yıl" der; K6 pencere dışı kayıt için onay ya da bir yıl ... | KK6'nin "pencere dışı onay" ve "bir yıl" maddeleri k6'nin isaret ettigi `02 §10.4.10`'un cumleleridir: "firmadan kaydı ... |
| M62 | D12 | T374 | Dusuk | RET | Dusuk | 0 | K8 `02 §5.5`'e atif yapar, `Dokumanlar`'da §5.5 yok. | k8'in kurali okunan `02 §7.5.3`'te eksiksizdir (02:1689 "bedel indiriminde ise tutar bazli kismi geri odeme kullanilir ... |
| M63 | D12 | T377 | Dusuk | KISMI | Dusuk | 2 | K4 `02 §3.17.8`, K8 `06 §4.41`'e atif yapar; `Dokumanlar` (`02 §8.2.2`…§8.2.6, §8.3.5, ... | `02 §3.17.8` kismi curuktur: k4'un saydigi kume okunan §8.2 tablosunun L-7 satirinda (02:1749 "kendiliginden iptal ... |
| M64 | D12 | T384 | Dusuk | KABUL | Dusuk | 1 | K1 `04` 9.24.3'e atif yapar, `Dokumanlar`'da `04` yok. | k1'in "aralik siparisin olustugu gundur" kurali okunan `02 §10.7.1`'de yoktur (02:2012 yalniz "tarih araligi secip" ve ... |
| M65 | D12 | T385 | Dusuk | RET | Dusuk | 0 | K1 `08 §2.2`, K5 `02 §9.4`, K9 `06 §4.58`'e atif yapar; uclu de `Dokumanlar`'da yok. | Uc atif da okunan birimde karsilanir ya da baska task'in evidir. k1: "ilk denemeden 5, 30 dk, 2 saat" — aralıklarin ... |
| M66 | D12 | T386 | Dusuk | RET | Dusuk | 0 | K7 `02 §7.4.7`'ye atif yapar, `Dokumanlar`'da §7.4.7 yok (yalniz §7.4.1). | k7'nin uc siniri okunan `02 §10.4.5`'te eksiksizdir (02:1954 "kargoya verildigi gunden once ve ileri tarihli olamaz ve ... |
| M67 | D12 | T388 | Dusuk | KABUL | Dusuk | 1 | K3 `02 §3.34.8`'e atif yapar, `Dokumanlar`'da `02` girdisi yok. | k3 sayimin kapsamini "sipariş tarafinin kayitlarinin (`02 §3.34.8`) her tablosu" diye tanimlar; okunan `05 §8.5.11` ... |
| M68 | D12 | T390 | Dusuk | RET | Dusuk | 0 | K10 `03 §2.10.1`'e atif yapar, `Dokumanlar` §2.10.2'den baslar. | k10'daki `03 §2.10.1` atfi "ana akis T329'dadir" isaretinin yeridir; T390 ana akisi sinamaz, kriterin kurali ("her ... |
| M69 | D12 | T401 | Dusuk | KABUL | Dusuk | 1 | k9 `08 §4.1`'e atıf yapar; `Dokümanlar`'da `08` için §4.1 yok. | k9 DV-11, DV-14, DV-38, DV-39'un gozlenip satira yazilmasini ve gelistirme ortaminin DV-41'e gore kurulmasini ister; ... |
| M70 | D13 | T402 | Dusuk | KABUL | Dusuk | 1 | k8 `08 §2.1.10`'a atıf yapar; `Dokümanlar`'da §2.1.10 yok. | 11:4005 k8 "kayıt referans kurulumun işaretli kaydından okunur" kuralinin kaynagi `08 §2.1.10`'dur (08:345-351, "DV-1, ... |
| M71 | D13 | T403 | Dusuk | KABUL | Dusuk | 1 | k9 `08 §4.1`'e, k4 `05 §4.3.4`'e atıf yapar; ikisi de `Dokümanlar`'da yok (`05` için ... | 11:4030 k9 `08 §4.1`'e (DV tablosu, 08:837), 11:4025 k4 `05 §4.3.4`'e (varsayilan yeniden deneme kurali ve "yanıtsız ... |
| M72 | D13 | T404 | Dusuk | KABUL | Dusuk | 1 | k3 `02 §8.2`'ye, k7 `05 §9.4.18`'e atıf yapar; ikisi de `Dokümanlar`'da yok (`02` hiç ... | 11:4050 k3 L-7 sayiminin kaynagi `02 §8.2` L-7 satiridir (02:1749, "ödeme sayfası hiç açılmamış sipariş sayılmaz"); ... |
| M73 | D13 | T405 | Dusuk | KABUL | Dusuk | 1 | k10 `08 §5.2`'ye atıf yapar; `Dokümanlar`'da §5.2 yok. | 11:4081 k10 DV-10 sonucunun `02`'ye geri beslemesini `08 §5.2`'ye baglar (08:929-931, "Tek satır bekler: satır 15, ... |
| M74 | D13 | T407 | Dusuk | KABUL | Dusuk | 1 | k9 `08 §4.1`'e atıf yapar; `Dokümanlar`'da `08` için §4.1 yok. | 11:4132 k9 DV satirlarini `08 §4.1`'e yazdirir; 11:4122 `Dokümanlar` `08` icin §2.2, §2.2.1, §2.2.2 sayar, §4.1 yok. ... |
| M75 | D13 | T410 | Dusuk | KABUL | Dusuk | 1 | k3 `08 §2.1.5`, k4 `08 §2.1.10`, §2.2, §2.3, k9 `08 §2.1.2` atıf yapar; hicbiri ... | 11:4200 k3 `08 §2.1.5`'e (imza dogrulamasi), 11:4201 k4 `08 §2.1.10`, §2.2, §2.3'e (PayTR test modu 08:345; SES ... |
| M76 | D13 | T501 | Dusuk | KISMI | Dusuk | 1 | k6 `08 §4.1`'e atıf yapıyor, Dokümanlar'da `08` yalnız §4.3. | 11:4233 k6 DV-34 satirini `08 §4.1`'e yazdirir; 11:4226 `Dokümanlar` `08` icin yalniz §4.3 sayar — bulgu gecerli, ... |
| M77 | D13 | T502 | Dusuk | KISMI | Dusuk | 1 | k4 `05 §3.7.2`, k9 (11:4258) `07 UC-06` atıf yapıyor; ikisi de Dokümanlar'da yok. | 11:4253 k4 `05 §3.7.2`'ye (yapilandirilmis veri, 05:453), 11:4258 k9 `07 UC-06`'ya (07:464) atif yapar; 11:4248 ... |
| M78 | D14 | T503 | Dusuk | RET | Dusuk | 0 | k8 `03 §3.2.1`, k1 (11:4276) `05 §7.9` atıf yapıyor; ikisi de Dokümanlar'da yok. | Alinti dogru, iki bolum de `Dokümanlar`'da yok (11:4274); ama kriterlerin icerigi okunan birimlerdedir: k8'in butun ... |
| M79 | D14 | T503 | Dusuk | RET | Dusuk | 0 | k2 süzgeç/sayfalama/sıra davranışını kategori sayfasına (T502 k6, 11:4255) eşitliyor; ... | Alinti dogru, `Bağımlılık` T502'yi saymaz (11:4273); ama sira guvencesi bozulmaz: F5 `Yürütme sırası` T502 → T503'tur ... |
| M80 | D14 | T504 | Dusuk | RET | Dusuk | 0 | k7 `05 §9.4.8`'e atıf yapıyor, Dokümanlar'da `05` yalnız §10.3. | Alinti dogru, `Dokümanlar`'da `05` icin yalniz §10.3 vardir (11:4300); ama k7'nin dayandigi cumle okunan `05 ... |
| M81 | D14 | T506 | Dusuk | RET | Dusuk | 0 | k8 `03 §3.5.3.8`, k9 `02 §3.32.7`, k2 `05 §3.7.2` atıf yapıyor; üçü de Dokümanlar'da yok. | Alinti dogru, uc bolum `Dokümanlar`'da yok (11:4348); ama k8 ve k9'un butun icerigi okunan `07 UC-15`'tedir: 429 L-4 ... |
| M82 | D14 | T507 | Dusuk | RET | Dusuk | 0 | k8 `02 §3.33.3`, k6 `05 §6.5.3`, k10 `05 §6.7.10` atıf yapıyor; Dokümanlar'da yok. | Alinti dogru, uc bolum `Dokümanlar`'da yok (11:4376); ama kriterlerin icerigi okunan `07 UC-173`/`UC-174`'tedir: bicim ... |
| M83 | D14 | T507 | Dusuk | KABUL | Dusuk | 2 | k9 eşzamanlı ikinci yayında 409 sayıyor; KK9 yalnız 422 kolunu sınıyor, KK7'nin 409'u ... | k9 iki sonuc sayar — 422 ve eszamanli ikinci yayinda 409 (11:4386; kaynak 07:6208 "eşzamanlı ikinci yayın birincinin ... |
| M84 | D14 | T509 | Dusuk | RET | Dusuk | 0 | k1 `05 §4.2.1`'e atıf yapıyor, Dokümanlar'da `05` yalnız §4.2.2 ve §6.7.9. | Alinti dogru, `Dokümanlar`'da `05` icin §4.2.2 ve §6.7.9 vardir (11:4432); ama k1'in icerigi — her gosterimde yeni ... |
| M85 | D14 | T510 | Dusuk | RET | Dusuk | 0 | k10 gerçek tarayıcıda ve gerçek sağlayıcıyla gözlem ister (pencere, yükseklik, 3D ... | DV gozlemi task'in kriteri, kaniti DV satiridir; bu, planin yerlesik kalibidir: F4'un her gozlem task'i DV gozlemini ... |
| M86 | D15 | T511 | Dusuk | KISMI | Dusuk | 1 | k9 `05 §7.7.4`, k7 (11:4496) `03 §1.11` atıf yapıyor; Dokümanlar'da yok. | k7 `03 §1.11`'e (11:4496) dayanir; `03 §1.11` ne `Dokumanlar`'da (11:4488) ne T511'in §7.2 satirinda (11:7681: `03` ... |
| M87 | D15 | T512 | Dusuk | KABUL | Dusuk | 2 | k7 `05 §4.1`, k9 (11:4526) `05 §4.6` atıf yapıyor; Dokümanlar'da hiç `05` yok. | k7 `05 §4.1` (05:483 "Durum makinesinin yeri — gecis birimi"), k9 `05 §4.6` (05:551 "Karta para gonderen istegin tek ... |
| M88 | D15 | T513 | Dusuk | KABUL | Dusuk | 1 | k3 `06 §4.25` ve `02 §7.6.7`, k9 (11:4554) `03 §1.11.9` atıf yapıyor; Dokümanlar'da yok ... | k3 `06 §4.25` (06:3663 Withdrawal) ve `02 §7.6.7` (02:1709 "Cayma beyani geri alinmaz"), k9 `03 §1.11.9` (03:417 ayip ... |
| M89 | D15 | T514 | Dusuk | KABUL | Dusuk | 1 | k5 `06 §4.36`, k6 (11:4579) `05 §6.4.6`, k10 (11:4583) `02 §3.13.14` atıf yapıyor; ... | k5 `06 §4.36` (06:3930 Customer), k6 `05 §6.4.6` (05:720 baglantinin hali), k10 `02 §3.13.14` (02:703) atif yapar; uc ... |
| M90 | D15 | T515 | Dusuk | KISMI | Dusuk | 4 | k3 `05 §6.1.1`, k6 `05 §6.7.12`, k7 `06 §4.36`, k5 `UC-49` atıf yapıyor; Dokümanlar'da ... | k3 `05 §6.1.1` (05:663 MK-45), k6 `05 §6.7.12` (05:779 akis cerezi), k7 `06 §4.36` (06:3930) T515 `Dokumanlar`'inda ... |
| M91 | D15 | T516 | Dusuk | KISMI | Dusuk | 3 | k2 `02 §8.1.4`, k6 `05 §6.1.2`, k10 `05 §6.1.5` atıf yapıyor; Dokümanlar'da yok. | k6 `05 §6.1.2` (05:665) ve k10 `05 §6.1.5` (05:671) T516 `Dokumanlar`'inda (11:4626) ve §7.2 satirinda (11:7686) yok — ... |
| M92 | D15 | T517 | Dusuk | KABUL | Dusuk | 2 | k2 `05 §6.1.5`'e atıf yapıyor, Dokümanlar'da `05` yok. | k2 `05 §6.1.5`'e (05:671 yeniden dogrulama izni ve tek kullanimlik deger) dayanir; T517 `Dokumanlar`'inda (11:4654) ve ... |
| M93 | D15 | T517 | Dusuk | RET | Dusuk | 0 | KK10 "ikinci istek ana sayfaya iner" diyor; k10 bu davranışı yazmıyor, yalnız "sonuç hâli ... | k10'un "sonuc hali bir kez gorunur" sozu `07 UC-69`'a isaret eder ve UC-69'un tanimi "bir kez"in olcusunu tam KK10'un ... |
| M94 | D16 | T519 | Dusuk | RET | Dusuk | 0 | k5 `02 §8.2.6`, k9 (11:4720) `05 §6.1.2` atıf yapıyor; Dokümanlar'da yok. | k5'in iki kolu da okunan `07 UC-76`'nın yan etkilerinde aynen yazılı: "hesabın bütün oturumları silinir … (`05 ... |
| M95 | D16 | T521 | Dusuk | RET | Dusuk | 0 | k2 `02 §10.8.2`'ye atıf yapıyor, Dokümanlar'da `02` hiç yok. | Kurulum kontrol listesinin maddeleri okunan `04` E-31'in 9.3.4 maddesinde `02 §10.8.2`'ye dayanarak tek tek yazılıdır ... |
| M96 | D16 | T523 | Dusuk | RET | Dusuk | 0 | k1 `05 §6.1.5`'e atıf yapıyor, Dokümanlar'da `05` yok. | k1'in kuralı okunan `07 UC-104`'te aynen vardır: "İşlemin formu yalnız oturumun izni o işlem içinse ve değerin özeti ... |
| M97 | D16 | T524 | Dusuk | RET | Dusuk | 0 | k9 `02 §10.3.1`'e atıf yapıyor, Dokümanlar'da `02` yalnız §10.6 ve §10.7. | k9'un kuralı okunan `07 UC-182` yan etkilerinde tamdır: "iz satırı dosya üretildikten sonra, gönderimden önce yazılır — ... |
| M98 | D16 | T527 | Dusuk | RET | Dusuk | 0 | k8 `03 §1.4.3`'e atıf yapıyor, Dokümanlar'da `03` yalnız §8.2. | k8'in dayandığı sonuçlar okunan `07 UC-127` (S11 koşulu: "teslim edilmiş bir kalem varken açık kalem bırakmıyorsa ... |
| M99 | D16 | T529 | Dusuk | RET | Dusuk | 0 | KK1 "yedi bağ" sayıyor; k1 kaydın hangi yedi türden olduğunu saymıyor, yalnız "geri ... | k1 `07 UC-131`'e işaret eder; UC-131'in isteği "`06 §4.33`'ün türe göre bağlarından tam biri" diye yedi bağı sayar (07 ... |
| M100 | D16 | T536 | Dusuk | RET | Dusuk | 0 | KK10 onay hâlinde "beklenen iade sayısı"na bakar; k10 onay hâlinin içeriğini saymaz. | k10 "değişikliği onay hâliyle yazar (`07 UC-172`…)" der; UC-172'nin onay hâli "'iade malı bekleniyor' kalemlerin ... |
| M101 | D16 | T537 | Dusuk | KISMI | Dusuk | 1 | k9 `07 §4.34.3`'e atıf yapar, `Dokümanlar` yalnız `07 §5.5` sayar; k7 de `05 §4.1.5`'e ... | `07 §4.34.3` kolu geçerli: k9'un "bir kez söylenir" kuralının mekanizması — `IsMergeNoticePending`'i okuma, aynı ... |
| M102 | D17 | T540 | Dusuk | KISMI | Dusuk | 1 | k8 `07 §5.5.1`'e (ve k4 `02 §8.3.2, §8.3.3`'e, 11:5289) atıf yapar; `Dokümanlar`'da `07` ... | T540 k8 `PanelFrame`'i kurar ve modelin tanimina `07 §5.5.1` ile dayanir (11:5293); modelin icerigi ve kaynaklari ... |
| M103 | D17 | T543 | Dusuk | KABUL | Dusuk | 1 | k6 `04` 5.26.5'e (ekran bölümü) atıf yapar; `Dokümanlar`'da `04` yalnız OB-12…OB-15 ve ... | k6'nin "defter varyanti o ildeki kaydi tutar" kolu ve KK6'nin "defter varyantinda tutulur" sinamasi yalniz `04` ... |
| M104 | D17 | T544 | Dusuk | RET | Dusuk | 0 | k8'in dayandığı parmak izi kuralı T509'da doğar, T509 bağımlılıkta yok (aynı fazdaki ... | Bagimlilik ters yondedir: T509 T544'e baglidir (11:4431 "T508, T537, T540, T541, T543, T544") ve yurutme sirasinda ... |
| M105 | D17 | T546 | Dusuk | RET | Dusuk | 0 | k6 `04 §3.1` ve §3.2'ye dayanır, `Dokümanlar` bu iki bölümü saymaz. | `04 §3.1` ve §3.2 T546'nin §7.2 satirinda (11:7716 "`04` §3.1, §3.2, §3.3, …") ve aday tablosunda (11:5903 "`04 ... |
| M106 | D17 | T547 | Dusuk | RET | Dusuk | 0 | k7 `02 §11`'e atıf yapar, `Dokümanlar`'da `02` yalnız §10.1'dir. | k7'nin kurali okunan `09 §3.5` madde 1'dedir (09:312 "`ProductConstants`'taki her sabit `02 §11.2`'nin bir P satirina ... |
| M107 | D17 | T549 | Dusuk | RET | Dusuk | 0 | OB-07 T541'in kapsamıdır (T541 başlığı), T541 bağımlılıkta yok. | T541 T549'un gecisli bagimliligidir: T549 T503'e (11:5523), T503 T541'e baglidir (11:4273 "T501, T537, T540, T541, ... |
| M108 | D17 | T601 | Dusuk | KISMI | Dusuk | 2 | k4 `07`'ye (ve k1 `00 §L.1`'e) atıf yapar; `Dokümanlar`'da `07` ve `00` yok. | k4 `08 §4.2` tablosunun `07` ile esitligini ister (11:5557); `07`'nin dis sistemde kayitli dort adresi yalniz `07 ... |
| M109 | D17 | T602 | Dusuk | RET | Dusuk | 0 | k7 `05 §8.3.12`'ye, k2 (satır 5583) `§8.8.3`'e atıf yapar; ikisi de `Dokümanlar`'da yok ... | k2'nin acil yama istisnasi okunan `05 §8.6.2`'nin kendi cumlesidir (05:1110 "Acil güvenlik yaması referans kurulumda ... |

### Sonuç (mercek)

Sınıf dokümanın tamamında tarandı: 214 task, 109 tekil bulgu, 59'u (78 çift + 1 elle) uygulandı; hiçbiri Yüksek değil, kalan risk yok. **4. tur (`11_CROSS_REVIEW_R4.md`) bu mercekten sonra, `11` v0.41 üzerinde, aynı betikler ve parça sınırlarıyla koşar;** merceğin kapattığı sınıf (Orta KK ↔ kriter ↔ `Dokümanlar` ↔ bağımlılık) ikinci modelin 4. turunda dönerse "sınıf kapanmadı" diye raporlanır. Etki yansıtma (Faz 5) döngü TEMİZ olunca ayrı adımdır.
