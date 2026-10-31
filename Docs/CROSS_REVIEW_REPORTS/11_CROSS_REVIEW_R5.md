# Cross-Review — 11 Implementation Planı (Tur 5)

**Tarih:** 2026-10-31 · **Hedef:** `Docs/11_IMPLEMENTATION_PLAN.md` v0.42 (girdi commit'i `a66a39d`, 759 622 bayt girdi, 7 761 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren, mekanizma okuma paragrafıyla (K-1399; `11_CROSS_REVIEW.md` §1) · **Koşum:** iki koşum — tek parça çağrı ve beş parçalı kontrol koşumu · **Sonuç:** **TEMİZ DEĞİL** — 13 ham bulgu, 13 ayrı bulgu; 2 doğru · 2 kısmen · 9 çürük (beşi önceki turlarda reddedilmiş iddianın dönüşü); 6 şartname çifti uygulandı (v0.42 → v0.43); tur 6 gerekir.

## 1. Koşum

Betikler, istem, parça notu, şablon ve **parça sınırları 1. turdakinin aynısıdır** (`11_CROSS_REVIEW.md` §1; K-1478 — sınır başlıkla tanımlıdır, değişmez). İstem tek karakter değiştirilmeden kullanıldı. Girdi `build_inputs.py` ile kuruldu (`uncovered lines: none`); `codex login status` → "Logged in using ChatGPT". Altı çağrı iki Codex dalgasında koştu, ikisi de aynı model ve aynı efor (`gpt-5.6-terra`, yüksek): **c1, p1, p2 ve p5 18:3x koşumunda; p3 ve p4 22:xx koşumunda** (önceki dalga kullanım sınırına takılmıştı; aynı model, aynı tur — tur bölünmüş sayılmaz). Her çağrının kendi boş çalışma klasörü vardır (`run_codex.sh r5/in r5/out <ad>...`); kullanım sınırı ikinci dalgada dolmadı, çıktısız dönen çağrı kalmadı.

**Cursor denemesi kullanılmadı.** Turun başında tüm çağrılar `cursor-agent` ile denendi; iki engel çıktı: (1) hesap ücretsiz planda, `gpt-5.6-sol-high` "Model unavailable on Free" dedi (açık olanlar yalnız Grok ve `auto`; `auto` modeli Cursor seçtiği için bağımsızlığı bozar, yasak); (2) `cursor-agent` kullanıcı düzeyindeki `~/.claude` hook'larını yükleyip PowerShell sözdizimli hook komutunu bash'te çalıştırdığından `TALIMAT.md`'yi okuyamadı (altı çağrının altısı da "okuyamıyorum" döndü, gerçek inceleme yok). O çıktılar `out-cursor-hatali/` altına ayrıldı ve hiçbir yerde kullanılmadı; rapordaki bütün bulgular Codex çıktısıdır. Çalışma dizini: `C:\Users\teknodeveloper\.claude\helper-state\shopfolio\kd-cross\r5\` (girdiler `in/`, ham çıktılar `out/`, doğrulayıcı paketleri `dogrulayici_<k>.md`, hükümler `hukum_D<k>.md`, talimat `talimat_dogrulayici.md`).

| Çağrı | Bölümler | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|
| Tek parça | tüm doküman | 759 622 bayt | 414 540 | 111 sn | 1 bulgu |
| Parça 1 | §1–§4 · §5 Task yapısı · F0 · F1 (satır 65–866) | 96 343 bayt | 54 893 | 251 sn | 3 bulgu |
| Parça 2 | F2 · F3 T301–T345 (867–2841) | 197 064 bayt | 88 615 | 285 sn | 2 bulgu |
| Parça 3 | F3 T346–T390 · F4 (2842–4219) | 150 962 bayt | 85 418 | 217 sn | 3 bulgu |
| Parça 4 | F5 · F6 · §6 (4220–5633) | 149 635 bayt | 73 821 | 77 sn | 2 bulgu |
| Parça 5 | §7 · §8 · §9 (5634–dosya sonu) | 254 332 bayt | 111 538 | 171 sn | 2 bulgu |

Web araması satırları (çağrı günlükleri): tek parça 6, parça 1 0, parça 2 4, parça 3 4, parça 4 4, parça 5 0. **İki koşumun ölçümü:** tek parça 1 bulgu, parçalı koşum 12 bulgu; iki koşum arasında örtüşen iddia yok (çerez iki bulgusu — tek parça B-1 ve parça 4 B-2 — aynı konuda ama ayrı çelişkilerdir). Hiçbir çağrı TEMİZ dönmedi. İki koşum bu turda da sürdü.

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**(a) Tek parça çağrı:**

> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: T224 kabul kriteri 6–7; T331 kabul kriteri 3
> Alıntı: “Çerez politikası ilk kez yayına alınana kadar vitrin çerez yazmaz; ara katman çerez başlıklarını yanıttan çıkarır.”
> Sorun: T331, aynı durumda mevcut hesapların girişinin etkilenmeyeceğini söyler. Ancak giriş formunun sahtecilik önleme çerezi ve başarılı girişin oturum çerezi vitrin yanıtındaki `Set-Cookie` başlığıyla kurulur; T224 bunları kaldırdığı için giriş tamamlanamaz. Bu, belgenin iki yerinin çelişmesidir (4).
> Öneri: T224’te, çerez politikası yayımlanmadan önce yalnız kimlik doğrulama akışında zorunlu sahtecilik önleme ve oturum çerezlerine açık istisna tanımlayın; diğer vitrin yanıtlarında çerezleri kaldırmaya devam edin. Her iki davranışı da T224/T331 kontrol listelerine test olarak ekleyin.
>
> SONUÇ: 1 BULGU

**(b) Parça 1 (§1–§4 · §5 Task yapısı · F0 · F1):**

> BULGU-1
> Kriter: Eksiklik
> Seviye: Orta
> Yer: §5 “Task yapısı” Konvansiyon 4; T119
> Alıntı: "`Dokümanlar:` çekirdek; `09` §10 (§10.6), §12; `06 §8.1`, §8.3, §8.4 (2)" / "`CompanySettings` varsayılanları `02 §11.1`'dendir"
> Sorun: T119’un kabul kriteri doğrudan `02 §11.1` girdisine dayanırken bu kaynak `Dokümanlar` alanında yoktur. Bu, task’ın bütün girdilerinin başlamadan hazır ve okunabilir olması güvencesini bozar (4); ajan varsayılan ayarları kaynak kuralını okumadan uygulayabilir.
> Öneri: T119’un `Dokümanlar` alanına `02 §11.1` ekleyin; §7 ileri ve geri izlenebilirlik satırlarını aynı PR’da doğrulayın.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: T117 — Kabul kriteri 3; doğrulama kontrol listesi KK3
> Alıntı: "`Undelivered`'da yalnız gönderildikten sonra geri dönen satırda doludur" / "`gönderim anı dolu da boş da `Undelivered` satır kabul edilir`"
> Sorun: Kabul kriteri, `Undelivered` kaydında gönderim anını yalnız gönderimden sonra geri dönen kayıtta izinli kılar; KK3 ise bu alan dolu veya boşken kaydı kabul eder. Doğrulama, yasaklanmış durumu yeşil yapabildiği için kabul kriteri ile test beklentisi çelişir (4).
> Öneri: `Undelivered` için gönderim anının zorunlu olup olmadığını tek kurala bağlayın; kabul kriterini, veritabanı kısıtını ve KK3’ü aynı kurala göre güncelleyin.
>
> BULGU-3
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §5 “Task yapısı” Konvansiyon 8; T04
> Alıntı: "`Blok en çok 2,5 KB`" / "`11_task_blok_denetle.py --tam sorunsuz koşar (Y8: sorun 0)`"
> Sorun: T04 bloğu, dokuz kabul kriteri ve dokuz kontrol maddesiyle tek başına 2,5 KB sınırını aşmaktadır. Bu, blok boyutu güvencesi ile “sorunsuz” denetim iddiasını çeliştirir (4); hedef task’ı sınırlı bağlamla okuma mekanizması güvenilir değildir.
> Öneri: T04’ü `T04` ve `T04a` olarak bölün veya metni sınırın altına indirin; yürütme sırası, §7 aday/matris satırları ve blok denetimi sonucunu güncelleyin.
>
> SONUÇ: 3 BULGU

**(c) Parça 2 (F2 · F3 T301–T345):**

> BULGU-1
> Kriter: teknik doğruluk
> Seviye: Yüksek
> Yer: F2 T201; F3 T301–T302
> Alıntı: “İmha işi her gece Türkiye saatiyle 03:00'te çalışır ve süre envanterinin satırlarını sırayla işler”; “Siparişin imha parçası”; “Öteki kayıtların süreyle imhası”
> Sorun: T201, tüm süre envanterini işleyen imha işini F2’de tamamlanacak kabul eder; sipariş ve diğer kayıtların gerçek imha parçaları ise sonraki fazdaki T301–T302’dedir ve T201’in bağımlılıklarında yoktur. F2, bu imha parçaları olmadan kapanabilir. Bu, planın task sırası güvence mekanizmasını bozar ve kişisel veri/saklama süresi imhasının uygulanmadan kalmasına yol açabilir.
> Öneri: T301 ve T302’yi T201’den önce tamamlanacak F2 task’ları yapın veya T201’i yalnız zamanlayıcı/çalıştırıcı iskeletiyle sınırlandırın; tüm envanteri işleme kabulünü, tüm imha parçalarına bağlı sonraki bir task’a taşıyın.
>
> BULGU-2
> Kriter: teknik doğruluk
> Seviye: Yüksek
> Yer: F3 T332 kabul kriteri 9; T337 kabul kriteri 6
> Alıntı: “Fiziksel üründe isteğe bağlı cayma istisnası işareti ve sebebi ürün düzlemindedir”; “hizmette ve çabuk bozulan (cayma istisnası) üründe indirimden hemen önceki fiyattır”
> Sorun: Plan, genel “cayma istisnası” işaretini çabuk bozulabilir mal istisnasıyla eşitlemektedir. Cayma hakkı istisnası olan her mal çabuk bozulabilir mal değildir. Bu nedenle çabuk bozulmayan bir istisna malında da “hemen önceki fiyat” referans alınabilir; indirimli satış reklamlarında uygulanacak referans fiyat kuralı ihlal edilir. Bakanlığın yürürlükteki açıklaması, bu istisnayı yalnız çabuk bozulabilen mallar ve hizmetler için tanır. [Resmî açıklama](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/fiyat-etiketleri-hakkinda-bilgilendirme)
> Öneri: Cayma istisnasından ayrı, yalnız mevzuattaki çabuk bozulabilir mal ölçütünü temsil eden kapalı bir sınıflandırma ekleyin. “Hemen önceki fiyat” kuralını yalnız bu sınıf ve hizmetler için uygulayın; diğer mallarda son on gündeki en düşük fiyatı zorunlu kılın.
>
> SONUÇ: 2 BULGU

**(d) Parça 3 (F3 T346–T390 · F4):**

> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: T368, Kabul kriteri 2
> Alıntı: "Düğme, firmaya ulaşmasından otuz gün geçmiş, teslim tarihi girilmemiş, işleme açık adedi kalan fiziksel kalemli siparişte görünür"
> Sorun: Gecikme nedeniyle fesih hakkını sabit 30 gün ve panelde teslim tarihi bulunmaması koşuluna bağlar. Satıcının taahhüt ettiği ifa süresini aşması fesih için yeterlidir; teslim tarihinin sisteme girilmemesi ya da 30 günün dolması beklenemez. Bu, tüketicinin yasal fesih hakkını geciktirir veya fiilen engeller. [Ticaret Bakanlığı’nın Mesafeli Sözleşmeler Yönetmeliği m.16 açıklaması](https://tuketici.ticaret.gov.tr/data/5e819a8e13b876a1b04c7a4a/Mesafeli%20S%C3%B6zle%C5%9Fmeler%20Y%C3%B6netmeli%C4%9Fi.pdf)
> Öneri: Düğmeyi, siparişin taahhüt edilen kargoya verme/ifa süresinin aşılmasıyla açın; 30 günlük yasal üst sınırı ayrı doğrulayın. Teslim kaydını yalnız delil olarak kullanın, hakkın ön koşulu yapmayın.
>
> BULGU-2
> Kriter: Hukuki uygunluk
> Seviye: Yüksek
> Yer: T372, Kabul kriterleri 7–8
> Alıntı: "Siparişin e-postasından gelmeyen bildirimi firma teyit eder"
> Sorun: Sipariş e-postası dışından gelen cayma bildirimini firma teyidine bağlayarak, teslim edilmemiş mal için de caymayı “müşteriyle anlaşıldı” iptali olarak işletiyor. Cayma için tüketicinin satıcıya yönelttiği açık beyan yeterlidir; kimlik doğrulama ihtiyacı bildirimin etkisini veya ulaştığı tarihi askıya alamaz. Bu tasarım tüketicinin süresinde kullandığı cayma hakkını kaybetmesine yol açabilir. [Ticaret Bakanlığı bilgilendirmesi](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)
> Öneri: Kanal veya e-posta eşleşmese de açık cayma beyanını ulaştığı anda cayma kaydı olarak etkili kılın. Gerekliyse teyidi yalnız kötüye kullanım incelemesi için ayrı ve sonradan yürüyen bir süreç yapın; fesih/cayma tarihini ve hak doğumunu değiştirmesin.
>
> BULGU-3
> Kriter: Hukuki uygunluk
> Seviye: Yüksek
> Yer: T386, Kabul kriteri 8
> Alıntı: "Cayma penceresi ve ayıp süresi yeni tarihten işler"
> Sorun: Panelde sonradan değiştirilen teslim tarihini cayma ve ayıp sürelerinin yeni başlangıcı yapar. Teslim tarihinin geriye çekilmesi, tüketicinin fiilî teslimden itibaren sahip olduğu cayma süresini sonradan kısaltabilir; ileri alınması da hak süresini güvenilmez hâle getirir. Cayma süresi fiziksel malın fiilî teslimiyle başlar. [Ticaret Bakanlığı bilgilendirmesi](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)
> Öneri: Teslim tarihi düzeltmesini yalnız kanıtlanmış fiilî teslim hatası için mümkün kılın; eski tarih, düzeltme kaydı ve tüketicinin düzeltmeden önce doğmuş hakları saklansın. Düzeltme tüketicinin zaten açık olan cayma veya ayıp başvuru süresini daraltmasın.
>
> SONUÇ: 3 BULGU

**(e) Parça 4 (F5 · F6 · §6):**

> BULGU-1
> Kriter: eksiklik
> Seviye: Yüksek
> Yer: T528 kabul kriterleri 7, 9
> Alıntı: "`ReceiveReturn` her teslim almayı ayrı kaydeder..." / "`CloseNotReturned` Z-42 geçince ulaşmamış adedi geri ödemesiz kapatır"
> Sorun: İade yalnız satıcının fiilen teslim alma kaydıyla ilerliyor ve mal ulaşmazsa geri ödeme kapatılıyor. Tüketicinin ön bilgilendirmede belirtilen taşıyıcıya malı teslim ettiği durumda iade yükümlülüğünün süresi bu teslimden başlar; satıcıya ulaşıp ulaşmaması koşul yapılamaz. Bu mekanizma tüketicinin bedel iadesi hakkını kaybetmesine yol açar (1). [Ticaret Bakanlığı açıklaması](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)
> Öneri: Belirtilen taşıyıcıya teslimi/ispatı kaydeden bir akış ekleyin; bu durumda geri ödeme süresini teslim tarihinden başlatın. Satıcının teslim almasını yalnız belirtilmeyen taşıyıcıyla gönderim gibi kanunen izin verilen durumlarda bekleme koşulu yapın.
>
> BULGU-2
> Kriter: tutarlılık
> Seviye: Orta
> Yer: T507 kabul kriteri 2; T540 kabul kriteri 6
> Alıntı: "Hiç yayına alınmamış aydınlatma metni ve çerez politikası 404 verir" / "Altbilgi hiçbir vitrin ekranında kaldırılmaz; ... aydınlatma metni ve çerez politikası bağlantıları vardır"
> Sorun: İlk yayın öncesinde vitrin görünür kalırken, zorunlu altbilgi bağlantıları 404 veren adreslere yönelir. T540 bağlantıları koşulsuz zorunlu tutarken T507 aynı durumdaki hedefleri yok saymaktadır; bu doğrudan doküman içi çelişkidir (4).
> Öneri: T540’ı, bağlantıların yalnız yayındaki metinler için gösterileceğini açıkça söyleyecek şekilde düzeltin; ilk yayın öncesinde bağlantıları gizleyin.
>
> SONUÇ: 2 BULGU

**(f) Parça 5 (§7 · §8 · §9):**

> BULGU-1
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §7.1.2, Aday task tablosu T549, §7.2 T549
> Alıntı: "`T549 ... | 04 E-05; `07` UC-04 (K-1437)`"
> Sorun: T549 aday tablosunda UC-04’e dayanıyor; ancak ileri matriste UC-04 yalnız T503’e, geri matriste T549 ise UC-04 dışındaki üç kaynağa bağlanmış. Bu, planın çift yönlü izlenebilirlik güvencesini bozar; arşiv ürün sayfasının aynı ürün ucundaki davranışı gate’te T549 için doğrulanmadan kalabilir.
> Öneri: `07 UC-04` satırını `T503, T549` yapın ve §7.2’de T549 kaynaklarına UC-04’ü ekleyin.
>
> BULGU-2
> Kriter: Eksiklik
> Seviye: Orta
> Yer: §7.1.2, Aday task tablosu T548, §7.2 T548
> Alıntı: "`T548 ... | `09 §16.2`; `07 §4.31.2`; K-1354`"
> Sorun: T548, `07 §4.31.2`deki parçalı yükleme yanıtlarını ve yeniden sürdürmeyi kaynak gösteriyor; buna karşılık API ileri matrisinde UC-83 yalnız T312’ye bağlı, T548’in geri yön satırında da bu API kaynağı yok. Böylece 409/404/400/413 ve ağ hatasında sürdürme sözleşmesinin istemci betiğinde uygulanması matrisin kapsam ve gate denetiminden çıkıyor.
> Öneri: UC-83/§4.31.2 kaynağını ileri matriste T312 ve T548’e bağlayın; T548’in §7.2 satırına aynı kaynağı ekleyin.
>
> SONUÇ: 2 BULGU

## 3. Bağımsız değerlendirme

Ham bulgular numaralandı: tek parça B-1, parça 1 B-1…B-3, parça 2 B-1…B-2, parça 3 B-1…B-3, parça 4 B-1…B-2, parça 5 B-1…B-2 (13 ham). Hiçbir çift aynı iddia değildir → 13 ayrı bulgu (F1–F13; sıra: parça 2 B-1, B-2; parça 3 B-1…B-3; parça 4 B-1; tek parça B-1 ve parça 4 B-2; parça 1 B-1…B-3; parça 5 B-1, B-2). Dokuz `[dogrulama]` Opus / high doğrulayıcıya verildi: Yüksek bulguların hepsi tek tek (D1: F1; D2: F2; D3: F3; D4: F4; D5: F5; D6: F6), Orta bulgular aynı bölümden gruplandı (D7: F7 + F8, yasal metin bağlantısı ve çerez; D8: F9, F10, F11, parça 1'in üç bulgusu; D9: F12 + F13, §7 matrisi); toplam 9 doğrulayıcı (bekçi sınırı 20). Önceki turlarda reddedilen iddianın olabileceği bulgularda (F4, F6, F10, F12, F13; F3 yan konu) paketle birlikte önceki hükmün tam dosyası verildi ve talimata "yeni bilgi yoksa kısa gerekçeli RET" eklendi. Yasal iddia taşıyan F2'de doğrulayıcı resmî metni (Fiyat Etiketi Yönetmeliği m.11/1, RG 11/10/2025-33044) Resmî Gazete'den okudu; F3, F4, F5, F6'da ikinci modelin getirdiği kaynaklar Ticaret Bakanlığı bilgilendirme sayfaları olduğundan (türetilmiş) proje kararlarıyla ve `02` kaynak cümleleriyle sınandı. Hüküm dosyaları `11_audit_hukum_denetle.py` ile denetlendi: sorun 0.

| # | Bulgu (kaynak) | Karar | Gerekçe | Uygulama |
|---|---|---|---|---|
| F1 | T201 k1 imha işi tüm envanteri işler, ama sipariş ve öteki kayıtların imha parçaları F3'te (T301–T302); F2 imha uygulanmadan kapanabilir (parça 2 B-1; T201, T301, T302) | KISMİ (Yüksek → Düşük) | Sıra iddiası tutmaz: fazlar zincirdir, `Bağımlılık` yalnız aynı fazı yazar, sonraki faz task'ı gate'le karşılanır (11:119, K-1408 (1)); eşlenmiş ama yapılmamış öğe gate'in boşluk taramasında S3'tür ve faza ertelenmez (11:128–131); sipariş ve öteki kayıtlar F3'te doğar, F2'de imha edilecek veri yoktur, T301/T302'yi F2'ye almak F2 kapsamını bozar. Geçerli çekirdek (4): T201 k1 "envanterin satırlarını işler" der ama F2'de doğrulanamaz (KK1 yalnız tetiği sınar, 11:886) ve parçaların F3'te işe eklendiği iki blokta da yazmaz. | T201 k1 daraltıldı, parçalar T301/T302'ye işaret eder (1 çift) |
| F2 | T332 k9 / T337 k6 genel "cayma istisnası" işareti "çabuk bozulan"la eşitlenmiş; "hemen önceki fiyat" yalnız çabuk bozulabilen mal ve hizmette olmalı (parça 2 B-2) | KISMİ (Yüksek) + KARAR | Genel iddia `02` §3.9.3 ve Z-21'de karşılanmıştır (kural istisna sebebi "çabuk bozulan" olana bağlıdır). İki geçerli yüz: (1) 11:2617 ifadesi her istisna işaretini eşitleyen okumaya açık, KK6 başka sebeple istisnalı malda 10 gün kuralını sınamaz; (2) asıl mevzuat açığı üst dokümanda: Fiyat Etiketi Yönetmeliği m.11/1 (RG 11/10/2025-33044) istisnayı yalnız "meyve ve sebze gibi çabuk bozulabilen mallar ile hizmetler"e tanır; `02` §7.3.4 (c) ve `06` `Perishable` ise MSY m.15/1-(c)'nin "çabuk bozulabilen **veya son kullanma tarihi geçebilecek** mallar" bendini tek değer yapar — yalnız son kullanma tarihi sebebiyle (c) işaretli, çabuk bozulmayan mal (takviye gıda, kozmetik) yanlış referans fiyat alır (ölçü (1)). Ayrı sınıflandırma 11'de kurulamaz (11 tüketicidir, K-1406 (2)(d)). | T337 k6 ve KK6 (2 çift) daraltıldı; üst doküman notu ve KARAR (aşağıda) |
| F3 | T368 k2 gecikme feshi düğmesi 30 gün + teslim tarihinin girilmemiş olmasına bağlı; taahhüt aşımı yeterli olmalı (parça 3 B-1) | ❌ RET (Yüksek → Düşük) | Taahhüt aşımıyla doğan fesih T368 k1'de zaten karşılanır ("kargoya verme süresi aşılınca fesih mevcut iptal düğmesiyle kullanılır", `02 §7.2.3`, K-198, K-499); k2 yalnız ikinci yolu (30 günlük yasal sınır, Z-11) anlatır. "Teslim tarihi girilmemiş" koşulu hakkı daraltmaz: MSY m.16 fesih hakkını teslimin yapılmamasına bağlar, teslim edilmiş kalemde gecikme feshi yoktur; tarihi girmemenin riski firmadadır (`02 §12.5`, K-542); işlemeyecek yasal yolu müşteriye göstermek K-1140'ta bilinçli elenmiştir. | Yok |
| F4 | T372 k7–8 sipariş e-postası dışından gelen cayma bildirimini firma teyidine bağlar; açık beyan ulaştığı anda etkili olmalı (parça 3 B-2) | ❌ RET (tekrar eden: 3. tur #4; Yüksek → Düşük) | Aynı iddia, yeni bilgi yok; 11:3464–3465 değişmedi ve `02`'yi birebir tüketir: "teyit kaydın tarihini değiştirmez — tarih bildirimin firmaya ulaştığı tarihtir", "teyit için geçen gün geri ödeme süresinden düşer ve gecikmenin riski firmadadır" (`02` §8.3.8, §10.4.10, §7.3.7, §6.4.31; K-604, K-607, K-709). MSY m.11/1 ve m.12/2 ile çelişki yok, tüketicide hak kaybı doğmaz. Önerinin "hemen kaydet, teyidi sonradan yürüt" yolu `02 §10.4.10` "Elenen yollar"da gerekçeyle elenmiştir. | Yok |
| F5 | T386 k8 teslim tarihi düzeltmesi cayma ve ayıp süresini yeni tarihten işletir; tüketicinin süresini kısaltabilir (parça 3 B-3) | ❌ RET (Yüksek → Düşük) | `02 §10.4.5` itirazı açıkça tartışıp karara bağlamıştır: düzeltme pencereyi kısaltabilir, "bu bilinçli bir seçimdir"; yasal pencere fiilî teslimden işler (MSY m.9/2), pencere ne düzeltme anından yeniden başlar ne eski tarihle korunur; yanlış girilmiş tarihin riski ve ispat yükü firmadadır; tarihe itiraz eden müşteri caymayı başka kanaldan bildirir ve firma kaydeder (§10.4.10). Düzeltmeden önce yapılmış beyan geçerli kalır (11:3841), eski ve yeni tarih işlem izinde durur (11:3842). "Yalnız kanıtlanmış hatada düzeltme" yazılımla sınanamaz. | Yok |
| F6 | T528 k7, k9 iadeyi satıcının fiilen teslim almasına bağlar, mal ulaşmazsa geri ödeme kapanır; belirtilen taşıyıcıya teslimde süre teslimden başlar (parça 4 B-1) | ❌ RET (tekrar eden: 2. tur F2, 4. tur F3; Yüksek) | Aynı m.12/1 öncülü T370 k3'ten T528'e taşınmış, yeni bilgi yok. Ürün iade taşıyıcısı belirlemez, müşteri seçer (`02 §7.4.4`; K-493), bu yüzden teslim sonrası her iade "öngörülenin haricinde" taşıyıcıyla yapılır ve süre malın ulaşmasıyla başlar (`02 §7.4.1`, Z-16; K-491). "Mal dönmedi" kapanışı geri ödeme borcuna karar vermez; mal sonradan ulaşırsa teslim almayla kalem yeniden açılır (`02 §6.4.18`; K-496, K-596) — T528 k7 bunu kodlar (11:4968), k9 `07` UC-142'yi aynen tüketir. | Yok |
| F7 | T224 k6–7 çerez politikası yayınlanana kadar vitrin çerez yazmaz; T331 k3 "mevcut hesapların girişi etkilenmez" der, giriş çerezsiz tamamlanamaz (tek parça B-1; T224, T331) | ❌ RET (Orta → Düşük) | Çelişki oluşamaz: çerez politikası ilk kez yayına alınana kadar veri toplayan girişlerin kapısı kapalıdır — hesap kaydı ve yeni hesap açan Google girişi kapalı, satışın dördüncü koşulu çerez politikasının yayınını ister (`02 §3.1.5`, §3.13.19); bu pencerede hiçbir müşteri hesabı ve sipariş var olamaz. T331'in "mevcut hesapların girişi etkilenmez" hâli yalnız politika yayınlandıktan sonra kapının başka sebeple kapandığı durumdur; o anda ara katman kalkmıştır ve geri kapanmaz (`05 §6.7.10`; `04` K-850). Panel girişi `/panel` altındadır ve süzgeç dışıdır (T224 KK7). | Yok |
| F8 | T507 k2 yayına alınmamış metin 404 verir; T540 k6 altbilgide aydınlatma metni ve çerez politikası bağlantılarını koşulsuz zorunlu tutar (parça 4 B-2) | DOĞRU (Orta) | Üst doküman koşulu koyar: firmanın düzenlediği metin hiç yayına alınmamışsa altbilgide o metnin bağlantısı yoktur (`04` 5.11.9, 3.1.19, 6.2.11.2; K-770). T537 modeli doğru yazar (11:5216), T507 404'ü verir (11:4379), ama T540 k6 ve KK6 koşulsuz bağlantı ister — 11'in iki yeri ve `04` ile çelişki (4). | T540 k6 ve KK6 (2 çift) |
| F9 | T119 k4, k7 `CompanySettings` varsayılanlarını `02 §11.1`'den ister; `Dokümanlar` alanında yok (parça 1 B-1) | DOĞRU (Orta) | 11:41 task okunurken yalnız bloğun, kaynak birimlerinin ve `Dokümanlar` alanının okunacağını söyler; T119'un kaynakları `06 §8.1`, §8.4 ve `09 §10`'dur, `02 §11.1` ne kaynak ne `Dokümanlar`'da; KK4 ve KK7 değerleri ister ve `06 §8.1.1` değerleri yazmaz, `02 §11.1`'e bırakır (`06` satır 5869). Okuma listesi KK'nin girdisini taşımıyor — blok konvansiyonunun güvencesi sağlanmıyor (4). Matris bağları ve sayımlar etkilenmez. | T119 `Dokümanlar` (1 çift) |
| F10 | T117 k3 `Undelivered`'da gönderim anı yalnız gönderim sonrası geri dönen satırda dolu; KK3 dolu da boş da kabul eder (parça 1 B-2) | ❌ RET (tekrar eden: 1. tur D3 #2; Orta → Düşük) | Aynı iddia, yeni bilgi yok; metin değişmedi. `06 §4.58` `Undelivered` satırında `SentAt`'i iki hâl için açık bırakır — hiç gönderilmeden denemeleri tükenen satırda boş, gönderildikten sonra geri dönen satırda dolu (K-1120); veritabanı kısıtı serbesttir ve KK3 kriterin ve kaynağın doğrudan karşılığıdır. Önerinin "alanı zorunlu kıl" kolu `06 §4.58`'e aykırı olurdu. | Yok |
| F11 | T04 bloğu dokuz kabul kriteri ve dokuz kontrol maddesiyle 2,5 KB sınırını aşar (parça 1 B-3) | ❌ RET (Orta → Düşük) | Olgu yanlış: T04 bloğu (11:249–273) denetim betiğinin ölçüsüyle 2 559 bayttır, sınır 2 560 (`11_task_blok_denetle.py` MAKS_BAYT, K-1406 (4); 11:169); `--tam` koşusu TOPLAM SORUN 0; madde sayısı (9) 10 sınırının altında (11:166). Konvansiyon 8 ile "sorunsuz" iddiası çelişmez. | Yok |
| F12 | T549 aday kaynağı `07` UC-04; §7.1'de UC-04 yalnız T503'te, §7.2 T549'da yok (parça 5 B-1) | ❌ RET (tekrar eden: 1. tur F9, 4. tur F4; Orta → Düşük) | Üçüncü dönüş, yeni bilgi yok. UC-04 satırının yalnız T503'te kalması K-1437 (3)'ün açık hükmüdür ("UC-04 satırı T503'te kalır (uç tek, E-04'ün gövdesi)", karar kaydı satır 1505, değişmedi); `07` E-05 için ayrı uç tanımlamaz, T549'un özgün birimi `04:E-05`'tir, §7.2 satırı §7.1 ile eşittir (11:6084, 11:6325, 11:7719; eşitlik betikle denetli). Aday tablodaki `07 UC-04 (K-1437)` bölmenin kaynağına işarettir. Gate kaygısı geçersiz: T549 `Dokümanlar` alanı `07 UC-04` ve §4.5'i taşır (11:5524), KK1/KK3/KK6/KK7 `07 §4.5.1`'e bağlıdır. | Yok |
| F13 | T548 `07 §4.31.2` parçalı yükleme yanıtlarını kaynak gösterir; UC-83 yalnız T312'ye bağlı, T548'in geri yön satırında API kaynağı yok (parça 5 B-2) | ❌ RET (tekrar eden: 1. tur F8; Orta → Düşük) | Paket "yeni" diye işaretlemişti; doğrulayıcı aynı iddianın 1. turda reddedildiğini buldu (K-1478 (4); `r1/hukum_D7` #8) — işaretleme hatası bizim, hüküm tekrar eden RET. `07`'nin matris birimi uçtur; §4.31.2'nin birimi UC-83 satırıdır ve T312'ye eşlidir (11:6163; K-1415/K-1416). T548'in `09 §16` + devir ile bağlanması K-1435'in açık hükmüdür ve gerekçesiz ekleme taraması T548'i gerekçeli sayar (11:5937, 11:7718). 409/404/400/413 ve ağ hatasında sürdürme gate'ten düşmez: T548 KK6, KK7, KK9 bunları tek tek sınar ve `Dokümanlar` alanı `07 §4.31.2`'yi taşır (11:5496). | Yok |

**Tekrar eden RET (turun sonucunu belirlemez):** F4 (3. tur #4), F6 (2. tur F2, 4. tur F3), F10 (1. tur D3 #2), F12 (1. tur F9, 4. tur F4), F13 (1. tur F8) — aynı yer ve aynı iddiayla döndü. **Seviye düşenler:** Yüksek → Düşük: F1, F3, F4, F5; Orta → Düşük: F7, F10, F11, F12, F13 (F6 çürük olduğu hâlde Yüksek işaretiyle kaldı). **Kısmi kalış:** F1 (daraltma) ve F2 (11'de ifade, sınıf üst dokümanda).

**F2 — KARAR ve üst doküman notu (uygulanmadı; Faz 5 / etki yansıtma):** (A) `02 §7.3.4` (c) sebebini iki liste değerine böl — "çabuk bozulabilen mal" ve "son kullanma tarihi geçebilecek mal", ikisi de mutlak cayma istisnası kalır; "hemen önceki fiyat" yalnız birincide ve hizmette uygulanır (yeni alan yok, `06` enum'una bir değer). (B) fiziksel ürünün tamamında 10 gün kuralı — alan eklemez ama çabuk bozulan malda Yönetmeliğin "esas alınır" dediği referanstan sapar. Öneri (A) (toplu onay modu, K-1472). Etkilenen birimler: `02 §7.3.4`, §3.9.3, §1.2 Referans fiyat, Z-21; `06` `WithdrawalExemptionReason.Perishable` ve K-508 satırı. Üst doküman bu turda değişmez (11 tüketicidir); 11'de ifade daraltıldı ve olumsuz durum sınanır.

## 4. Ek bulgular

- **F2'nin üst doküman bulgusu** (yukarıda): ölçü (1) sınıfında gerçek bir açık, kaynağı `02`/`06`; 11 bu turda yalnız ifadesini daralttı. Etki yansıtmada `02` ve `06` düzeltmesi + karar satırı gerekir.
- Öteki doğrulayıcılardan `11`'i etkileyen ek bir tutarsızlık bildirilmedi. 3. turun EK-1'i (`scripts/doc-scans/11_ozet_ayrinti_grupla.py` satır 36 paket metni) betik `11` olmadığından değişmeden etki yansıtmaya kalır.

## 5. Kullanıcı onay checklist'i

- [ ] F1 — KISMİ (uygulandı: T201 k1)
- [ ] F2 — KISMİ (uygulandı: T337 k6, KK6; ⚠ KARAR (A), üst doküman Faz 5)
- [ ] F3 — RET (uygulanmadı)
- [ ] F4 — RET, tekrar eden (uygulanmadı)
- [ ] F5 — RET (uygulanmadı)
- [ ] F6 — RET, tekrar eden (uygulanmadı)
- [ ] F7 — RET (uygulanmadı)
- [ ] F8 — KABUL (uygulandı: T540 k6, KK6)
- [ ] F9 — KABUL (uygulandı: T119 `Dokümanlar`)
- [ ] F10 — RET, tekrar eden (uygulanmadı)
- [ ] F11 — RET (uygulanmadı)
- [ ] F12 — RET, tekrar eden (uygulanmadı)
- [ ] F13 — RET, tekrar eden (uygulanmadı)

⚠ karar: F2 (KARAR (A), toplu onayla kaydedildi, K-1483). Önerilen `00`/`CLAUDE.md`/`SETUP.md` değişikliği yok.

## Sonuç

**SONUÇ: TEMİZ DEĞİL — 13 ayrı bulgu (13 ham); 2 doğru · 2 kısmen · 9 çürük; 6 şartname çifti uygulandı.** Çıkış kuralı (Faz 4 madde 5, 5. turdan itibaren) **uygulanamaz:** kabuller arasında Yüksek şiddetli F2 (ölçü (1): fiyat etiketi mevzuatına aykırı referans fiyat, kaynağı üst dokümanda) ve Orta şiddetli F8, F9 var; kuralın "kabullerin hepsi düşük şiddette metin farkı" koşulu sağlanmaz. Tekrar eden RET kuralı beş bulguyu (F4, F6, F10, F12, F13) turun sonucundan ayırır ama kabulleri etkilemez. F9 hedefli mercek sınıfındandır (kriter ↔ `Dokümanlar`); F8 iki task'ın birbiriyle çelişmesidir (T507 ↔ T540); ikisi de tek yerde kaldı. Hedefli mercek koşulu (art arda iki tur orta kabul) tur 4 kabulsüz olduğu için oluşmadı. **Tur 6 gerekir** (aynı betikler, aynı parça sınırları); F2'nin üst doküman düzeltmesi etki yansıtmaya kalır.

Sürüm: `11` v0.42 → **v0.43**; karar kaydı v0.269 → v0.270; K-1483.
