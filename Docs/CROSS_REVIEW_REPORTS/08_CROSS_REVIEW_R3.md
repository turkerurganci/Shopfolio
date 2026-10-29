# Cross-Review — 08 Integration Spec (Tur 3)

**Tarih:** 2026-10-29 · **Hedef:** `Docs/08_INTEGRATION_SPEC.md` v0.12 (girdi commit'i `92ae7a3`) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Koşum:** 1. ve 2. turla aynı iki koşum (tek parça çağrı `c1` ve altı parçalı `p1`…`p6`), aynı istem, aynı parça sınırları · **Sonuç:** **TEMİZ DEĞİL** — yedi ayrı bulgu; ikisi kısmen kabul edilip uygulandı, beşi reddedildi.

## 1. Koşum

Betikler, istem ve parça sınırları 1. turun raporundadır (`08_CROSS_REVIEW.md` §1); bu turda aynıları kullanıldı: `C:\Users\TEKNOD~1\AppData\Local\Temp\claude\c--projects-Shopfolio\ab472d0d-cc27-40b7-9288-b725735c9991\scratchpad\cr08\` — `run_codex.sh` (`bash run_codex.sh in3 out3 c1 p1 p2 p3 p4 p5 p6`), `prompt.txt`, `partnote.txt`, `build_inputs.py`. Girdiler `in3\` (v0.12'den kuruldu). Yedi çağrı paralel koştu, hepsi `rc=0` döndü (süreler 87–336 sn); kullanım sınırı dolmadı, yeniden koşum olmadı. Yasal iddialar resmî metne karşı okundu (Mesafeli Sözleşmeler Yönetmeliği m.12, m.16 ve 6563 m.4, Elektronik Ticaret Yönetmeliği m.9, 6502; mevzuat.gov.tr ve Resmî Gazete, okuma tarihi 2026-10-29); PayTR bildirim alan tablosu `dev.paytr.com/iframe-api/iframe-api-2-adim` belgesinden yeniden okundu (2026-10-29).

| Çağrı | Sonuç |
|---|---|
| `c1` tek parça | 2 bulgu |
| `p1` | 1 bulgu |
| `p2` | 2 bulgu |
| `p3` | 3 bulgu |
| `p4` | 1 bulgu |
| `p5` | TEMİZ |
| `p6` | 1 bulgu |

Ham on bir bulgu yedi ayrı bulguya indi: G1 iki çağrıdan (`c1`-1, `p1`-1), G2 üç çağrıdan (`c1`-2, `p2`-2, `p6`-1) geldi.

## 2. Ham bulgular

İkinci modelin çıktıları düzenlenmeden (alıntı blokları içinde).

**(a) Tek parça çağrı:**
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §2.1.2 Anahtarlar ve değişim
> Alıntı: “Anahtarlar kaldırıldıktan sonra kartla ödenmiş siparişin geri ödemesi … bağlantı katmanı istek yapmadan reddeder, geri ödeme kaydı gerçekleşmedi olur … müşterinin seçimiyle havale yolu.”
> Sorun: Kart anahtarlarını kaldırmak, geçmiş kart ödemelerinden doğan cayma/iade borcunu ortadan kaldırmaz. Belge, müşterinin IBAN vermesine bağlı havaleyi tek çıkış yaparak kartla yapılması gereken iadenin gerçekleşmemesine izin veriyor. Bu, müşteri için para ve hak kaybı; Mesafeli Sözleşmeler Yönetmeliği m.12’deki satın alırken kullanılan ödeme aracına uygun, masrafsız ve tek seferde geri ödeme yükümlülüğüyle çelişir. [Ticaret Bakanlığı açıklaması](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)
> Öneri: Anahtar kaldırma/sağlayıcıdan çıkış, tüm yasal iade yükümlülükleri için kart iadesini sürdürecek sözleşmesel ve teknik yol kurulmadan yasaklanmalı. “Gerçekleşmedi” kaydı, müşterinin IBAN tercihiyle kapanmamalı; kart iadesi sağlayıcı veya zorunlu firma operasyonu üzerinden tamamlanmalıdır.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §2.1.5 Tutar karşılaştırması; §4.1 DV-10; §5.2 satır 15
> Alıntı: “Fark saklanmadığı için (K-1255 (4)) ürün onu iade edemez.”
> Sorun: Doküman, taksit farkıyla müşteriden `Order.Total` üstünde tahsilat yapılabildiğini kabul ediyor; buna rağmen farkı saklamıyor ve yalnız `Order.Total` tutarında iade tasarlıyor. Farkın banka/PayTR tarafından dönmediği DV-10 sonucu ancak sonradan görülecek; kart hattının açılmasını engelleyen bir kapı da yok. Bu durumda caymada tahsil edilen tutarın bir kısmı müşteride kalır; belge bunun m.12/4’e aykırı olduğunu kendisi de kabul ediyor. Bu, doğrudan para ve hak kaybıdır.
> Öneri: DV-10’u kart yönteminin canlıya alma kapısı yapın. Tam tahsilatın otomatik olarak aynı ödeme aracına döndüğü doğrulanana kadar kart anahtarları tanımlanmasın; aksi durumda tahsil edilen toplam ve iade edilecek toplam saklanıp farkı da kapsayan uyumlu iade akışı kurulmalıdır.
>
> SONUÇ: 2 BULGU

**Parça 1:**
> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §2.1.2 Anahtarlar ve değişim
> Alıntı: “Anahtarlar kaldırıldıktan sonra kartla ödenmiş siparişin geri ödemesi … sağlayıcıya gönderilmez … müşterinin seçimiyle havale yolu”
> Sorun: Cayma da açıkça bu akışa dahildir. Kartla tahsil edilmiş bedelin, anahtar kaldırıldığı için yalnız havale yoluna zorlanması Mesafeli Sözleşmeler Yönetmeliği m.12/4’teki iadenin tüketicinin satın alırken kullandığı ödeme aracına uygun, masrafsız ve tek seferde yapılması zorunluluğunu karşılamaz. Bu, tüketicinin bedel iadesi hakkının kaybına yol açar. [Ticaret Bakanlığı açıklaması](https://tuketici.ticaret.gov.tr/yayinlar/tuketici-bilgi-rehberi/mesafeli-sozlesmeler-hakkinda-bilgilendirme)
> Öneri: Sağlayıcı anahtarlarını, kartla tahsil edilen siparişlerin kanuni ve sözleşmesel iade yükümlülüğü bitene kadar iade çağrısı yapabilecek şekilde koruyun; bu sağlanamıyorsa anahtar kaldırmayı engelleyin. Havale ancak tüketicinin özgür ve yazılı talebiyle ek seçenek olabilir.
>
> SONUÇ: 1 BULGU

**Parça 2:**
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Orta
> Yer: §2.1.5, “Bildirimin sınıflanması”
> Alıntı: "`status` = `success`/`failed` değil · `payment_type` ≠ `card` · `currency` ≠ `TL` · canlı kurulumda `test_mode` = `1`"
> Sorun: PayTR, başarısız bildirimlerde `currency` alanını göndermez; bu nedenle .NET karşılaştırmasında eksik değer `TL` değildir ve geçerli `failed` bildirimi 4. satırda işaretliye düşer. Aynı tablonun 5. satırındaki normal başarısız-ödeme işleme kolu çalışmaz. PayTR’ye `OK` dönüldüğü için bildirim yeniden gelmeyebilir. Bu, belgenin kendi sonuç sınıflandırmasıyla çelişir ve ödeme başarısızlığının beklenen akışının uygulanmamasına yol açar. [PayTR bildirim alanları](https://dev.paytr.com/iframe-api/iframe-api-2-adim)
> Öneri: `currency` denetimini yalnız `status = success` için uygulayın; `failed` bildiriminde eksik `currency` ve `payment_amount` alanlarını beklenen kabul edin.
>
> BULGU-2
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §2.1.5 “İadeye etkisi”; §2.1.9 “İstek” ve “Sıklık”
> Alıntı: "Geç gelen ödemede `Order.Total`'dır; taksit farkı girmez."
> Sorun: Tahsil edilen `total_amount` taksit farkıyla `Order.Total`’dan yüksek olabiliyorken iade isteği her durumda yalnız `Order.Total` ile kuruluyor. Doküman da farkın ürün tarafından iade edilmediğini kabul ediyor; buna rağmen kartın canlıya alınmasını DV-10’un sonucuna bağlamıyor ve aynı ödeme aracıyla, tek seferde tamamlayacak somut bir dış süreç tanımlamıyor. Bu, cayma ve firma iptali hâllerinde tüketiciden tahsil edilen tutarın eksik iadesine yol açabilir; Mesafeli Sözleşmeler Yönetmeliği m.12 tahsil edilen tüm ödemelerin, tüketiciye masraf yüklemeden ve kullanılan ödeme aracına uygun biçimde iadesini gerektirir. [Resmî Gazete, m.12](https://resmigazete.gov.tr/eskiler/2022/08/20220823-2.htm)
> Öneri: DV-10 doğrulanana kadar kart hattını canlıya kapatın. PayTR tahsil edilen toplamı aynı araçtan iade edemiyorsa sağlayıcıyı veya ödeme/taksit modelini değiştirin; edebiliyorsa `Refund.Amount` ve iade tavanını tahsil edilen toplamı kapsayacak biçimde yeniden tasarlayın.
>
> SONUÇ: 2 BULGU

**Parça 3:**
> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §2.2.4 — “Aşım” ve “Yasal bedeli”
> Alıntı: "Eşik dolduktan sonra aynı adrese oluşan müşteri bildirimi gönderilmez"
> Sorun: İlk B-1 de sınıra tabidir; saldırı veya yoğunlukta sipariş teyidi hiç gönderilmez ve yalnız panel kullanıcısının sonradan elle yeniden göndermesine bırakılır. Bu, dokümanın da belirttiği üzere işlem ortamına ek elektronik teyit yükümlülüğünü gecikmeksizin sağlamaz; 6563 m.4/1-b ve Yönetmelik m.9/1 bakımından mevzuata aykırılık riski doğurur. [6563 sayılı Kanun, m.4](https://www.lexpera.com.tr/resmi-gazete/metin/elektronik-ticaretin-duzenlenmesi-hakkinda-kanun-29166-6563)
> Öneri: B-1’in kota nedeniyle gönderilmeden kapanmasına izin vermeyin; B-1’i sınırdan muaf tutun veya sınır aşıldığında gecikmeksizin otomatik ikinci bir uygun elektronik kanaldan teyit gönderimini zorunlu kılın.
>
> BULGU-2
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §2.2.2 — “Askıdaki alıcının ön sorgusu”
> Alıntı: "başka her yanıt (429, kimlik ya da izin hatası, zaman aşımı) gönderimi durdurmaz: SendEmail yapılır, sorgunun hatası kanal hâline yazılmaz."
> Sorun: Doküman, askıdaki bir adrese SES’in iletiyi kabul edip göndermediğini açıkça söylüyor. Ön sorgu hata verdiğinde önceden askıda bulunan adrese SendEmail çağrısı yapılır; 200 dönse bile ileti teslim edilmez. Adres yeni askıya alınmadıysa 24 saatlik sorgu da bunu sonradan işaretlemez. Böylece özellikle B-1 “gönderildi” görünürken hiç ulaşmayabilir; firma ve müşteri için çıkış/uyarı oluşmaz. Bu, gönderim sonucunun güvenilir sınıflanması güvencesini bozar ve sipariş teyidi yükümlülüğünün kaçmasına yol açar.
> Öneri: Askı listesi ön sorgusu belirsiz kaldığında SendEmail çağrısı yapmayın; sonucu `ChannelError` olarak kaydedip yeniden deneyin ve kanal uyarısı mekanizmasına dahil edin. `Sent` yalnız ön sorgu olumlu sonuçlandıktan sonra SES kabulüyle yazılmalıdır.
>
> BULGU-3
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §2.2.2 — “Hata eşlemesi”
> Alıntı: "`MessageRejected` (400) — diğer hâller: ileti geçersiz içerik nedeniyle gönderilemiyor | alıcı hatası — `RecipientError`"
> Sorun: Geçersiz içerik, şablon veya istek üretim hatası alıcıya özgü değildir; bütün e-postaları etkileyebilir. Buna rağmen alıcı hatası sayılması kanal uyarısını engeller. Doküman ayrıca bu durumda “her iletiyi reddedebilir” demesine rağmen bunu alıcı hatası diye sınıflandırmaktadır. Sonuçta tüm B-1 ve hesap e-postaları sessizce `Undelivered` olabilir; müşterinin teyit veya hesap erişimi için güvenilir bir çıkışı kalmaz.
> Öneri: Şablon, gövde üretimi, başlık ve sağlayıcıya gönderilen isteğin geçersizliği gibi alıcıdan bağımsız hataları `ChannelError` ya da kanal uyarısını açan ayrı bir uygulama-hatası sınıfına eşleyin; `RecipientError` yalnız kanıtlanmış alıcı/adres kaynaklı hatalarda kullanılmalıdır.
>
> SONUÇ: 3 BULGU

**Parça 4:**
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §2.2.6, B-7
> Alıntı: "Stok sebebinde de aynı metin gider; sebep müşteriye aynen yazılır."
> Sorun: Bu, `OutOfStock` ile siparişin yönetici tarafından iptalini Mesafeli Sözleşmeler Yönetmeliği m.16/4 kapsamındaymış gibi işletir. Oysa stokta bulunmama, anılan maddede açıkça imkânsızlık sayılmaz; yalnız iade bildirimi gönderip bedeli iade etmek siparişin bu sebeple tek taraflı iptalini hukuka uygun kılmaz. Müşterinin sözleşmenin ifası hakkının kaybına yol açabilecek mevzuat aykırılığıdır. [Resmî metin](https://tuketici.ticaret.gov.tr/data/5e81982d13b876a1b04c7a42/2023-6502%20Say%C4%B1l%C4%B1%20T%C3%BCketicinin%20Korunmas%C4%B1%20Hakk%C4%B1nda%20Kanun.pdf)
> Öneri: `OutOfStock`u tek başına yönetici iptal nedeni olarak kaldırın; gerçekten ifa imkânsızsa sebebi ayrı tanımlayın, bildirimi öğrenmeden itibaren üç gün içinde gönderin ve teslimat masrafları dâhil iadeyi bildirimden itibaren en geç on dört gün içinde yapın.
>
> SONUÇ: 1 BULGU

**Parça 5:**
> SONUÇ: TEMİZ

**Parça 6:**
> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §4.1 DV-10; §5.2 satır 15
> Alıntı: “DV-10 sonucu bekleniyor; sonuç farkın dönmediğini gösterirse karar kaydıyla işlenir.”
> Sorun: Kart hattının açılmasını engelleyen bir kapı yoktur. Taksit farkı müşteriden tahsil edilip PayTR/banka tarafından geri dönmezse, ürün yalnız `Refund.Amount` iade ederek tahsil edilen toplam tutarı eksik iade eder. Doküman bunun Mesafeli Sözleşmeler Yönetmeliği m.12’deki “tahsil edilen tüm ödemeler” yükümlülüğüne aykırı olduğunu da kabul ediyor; sonucu sonradan karara bırakmak müşterinin hak kaybına yol açar.
> Öneri: DV-10 doğrulanana ve tam tahsilatın aynı ödeme aracına eksiksiz iadesi kanıtlanana kadar kart anahtarlarının tanımlanmasını engelleyin. Fark otomatik dönmüyorsa, farkın iadesi için uygulanabilir yöntemi ve tekilleştirmesini karar kaydıyla/`02` ile tamamlayıp kabul kanıtını zorunlu kılın.
>
> SONUÇ: 1 BULGU

## 3. Bağımsız değerlendirme

Yedi ayrı bulgunun her biri bir `[dogrulama]` Opus / high doğrulayıcıya verildi (yedi doğrulayıcı). G2 ve G4, 1. turda kısmen kabul edilip 2. turda reddedilmiş iddialardır (F1, F5 — G2/G3); yeni bilgi taşımadıkları için RET'le kapandı; "tekrar eden RET" sayılmaz (her biri yalnız bir kez reddedilmişti).

| # | Bulgu (kaynak) | Karar | Gerekçe | Uygulama |
|---|---|---|---|---|
| G1 | Anahtarlar kaldırılınca kart iadesi yalnız havaleye düşer (`c1`-1, `p1`-1; §2.1.2) | KISMİ ⚠ | **Gerçek kısım:** kaldırma şartı yalnız "açık kart işi"ni sayıyor; cayma süresi dolmamış ya da açık kalemli kart siparişlerinin beklenen iadesi kendiliğinden "gerçekleşmedi" oluyor ve müşteri havaleye zorlanıyor — Mesafeli Sözleşmeler Yönetmeliği m.12/4 (okuma 2026-10-29) ve `02 §7.2.8`'in "başka yol ancak müşterinin tercihiyle" dayanağıyla çelişir; (1)/(4). Konu R1/R2'de gelmedi. **Reddedilen kısım:** "iade yükümlülüğü bitene kadar mutlak engel" kurulamaz (ayıp hakkı yıllarca sürer; PayTR bir yıldan eski işlemi iade etmez, `err_no` `011`; sağlayıcıyı sürdürmeye zorlanamaz, K-1235). | K-1287 ⚠: §2.1.2 kaldırma şartına "geri ödemesi doğabilecek kart siparişi kalmamalı"; §5.2 satır 20 |
| G2 | Taksit farkı iade edilmiyor; DV-10 kapı değil (`c1`-2, `p2`-2, `p6`-1; §2.1.5, §2.1.9, DV-10) | RET | R1 F1 (kısmi, K-1275) ve R2 G2 (ret) ile aynı yer ve iddia; K-1275 "kart hattını ya da taksidi kapatmak ürün kararıdır" diye bu yolu açıkça eledi. Belgede çelişki yok: genel kapı kuralı ÖK-4'te sayılan yeteneklere bağlı, DV-10'un kendi "yanlış çıkarsa" hükmü ayrıca işler. Firmanın çıkışı var (taksiti sağlayıcı panelinden firma açar, `no_installment` alanı). | — |
| G3 | PayTR `failed` bildirimde `currency` göndermez; geçerli `failed` yanlış sınıflanır (`p2`-1; §2.1.5) | RET | Olgusal dayanak yok: `dev.paytr.com/iframe-api/iframe-api-2-adim` (okuma 2026-10-29, iki yoldan) `currency`'yi Zorunlu = Evet yazar; koşullu olanlar yalnız `failed_reason_code`, `failed_reason_msg` ve `test_mode`. 08 §2.1.5 "yedi alan her zaman" diyerek belgeyle birebir uyumlu. | — |
| G4 | Alıcı sınırına düşen B-1 gönderilmez; 6563 m.4/1-b, Yönetmelik m.9/1 (`p3`-1; §2.2.4) | RET | R1 F5 (kısmi, K-1278) ve R2 G3 (ret) ile aynı yer ve iddia. Yeni olan yalnız 6563 m.4/1-b atfı: bu kol sipariş sayfasının numarayla gecikmesiz teyidiyle karşılanır (`02 §3.17.1`); ikinci kanalı isteyen m.9/1'dir ve açığı 08 zaten yazıyor, telafisi firma yükümlülüğü (K-1278, `02 §12.5`). Çelişki yok; iki öneri K-1242, K-1271, K-1278'de elendi. | — |
| G5 | Askı ön sorgusu hata verirse SendEmail yapılır, `Sent` yazılır (`p3`-2; §2.2.2) | RET | K-1271 (3) "sorgu hatasında gönderim sürer (fail-open)" bilinçli karar; kalan risk §2.2.3 "Yan etki", DV-22 ve §2.2.1 Doğrulanacaklar (7)'de yazılı. Belge ön sorgunun her zaman yakaladığını söylemiyor. Önerilen fail-closed ön sorgu kalıcı hatasında (ör. izin eksik) bütün e-postaları durdurur. Gerçek açık yalnız geçici sorgu hatası ile askının çakıştığı nadir durum. | — |
| G6 | `MessageRejected` "diğer hâller" (geçersiz içerik) alıcı hatası sayılıyor (`p3`-3; §2.2.2) | KISMİ | **Gerçek kısım:** `05 §10.2.2` alıcı hatasını "yalnız o alıcıya ulaşmayan ileti" diye tanımlar; geçersiz içerik/şablon hatası alıcıya özgü değildir ve satırın kendi gerekçesi çelişir ("tek iletiyi ilgilendirir" / "her iletiyi reddedebilir"); tablonun "tabloda olmayan her yanıt kanal hatasıdır" ilkesi ve `BadRequestException` gerekçesiyle de çelişir — (4). **Reddedilen kısım:** "sessizce Undelivered" abartılı: satırda "ulaşmadı" işareti düşer, F-1…F-4 de reddedilince panelde firma bildirimleri uyarısı açılır. | K-1288: tablo satırı `ChannelError`; alıcı adresi biçimi reddi ayrı satır olabilir (DV-23'e genişletme); sonraki cümle ve DV-23 |
| G7 | B-7 stok sebebi m.16/4 kapsamında işleniyor (`p4`-1; §2.2.6) | RET | R2 G7 ile aynı yer ve iddia. m.16/4 son cümlesi doğru (okuma 2026-10-29) ve `02 §7.2.4` / K-609 aynı tespiti zaten yazar: stok yokluğu imkânsızlık sayılmaz, iptalin sonucu firmadadır (panel uyarısı K-798 (1)); "OutOfStock'u kaldır" önerisi K-373'te elendi. 08 B-7 paragrafı imkânsızlık iddia etmiyor, yalnız 14 günlük iadeyi taahhüt eder. | — |

## 4. Ek bulgular

- **EK-1 (G2 doğrulayıcısı):** DV-10'un "kabul: canlı kanıt" yolu (§2.1.10) küçük tutarlı siparişi taksitli olmaya zorlamıyor; yazılı teyit alınamazsa DV-10 gözlenmeden kabul edilmiş sayılabilir. Ölçü dışı (iyileştirme); bu turda uygulanmadı, etki yansıtma ya da 4. turda ele alınır.
- **EK-2 (G1 doğrulayıcısı):** anahtarlar kaldırıldıktan sonra firma iadeyi PayTR panelinden elle yapsa da üründe kart iadesini "gerçekleşti" diye işleyecek bir yol yok; K-1283'ün komutu yalnız ödeme sonucunu kapsıyor. Ölçü dışı (K-1287 ile süre dışı hâl dar); 4. turda ele alınabilir.
- **EK-3 (G7 doğrulayıcısı):** 6502 m.48/5 dipnotunda Anayasa Mahkemesi iptali (E.2024/229, K.2026/35; iptal 30/1/2027'de yürürlükte) var; 02 ve 08 bu fıkraya atıf yapmıyor, etkisi yok.

## 5. Kullanıcı onay checklist'i

- [x] G1 — anahtar kaldırma şartı (K-1287 ⚠; öneriyle kaydedildi)
- [ ] G2 — RET
- [ ] G3 — RET
- [ ] G4 — RET
- [ ] G5 — RET
- [x] G6 — `MessageRejected` içerik reddi (K-1288)
- [ ] G7 — RET

## Sonuç

**SONUÇ: TEMİZ DEĞİL — 7 ayrı bulgu (11 ham), 2 kısmi kabul, 5 ret.** Skill'in çıkış kuralı: kabul edilen bulgulardan G1 para/hak kaybı sınıfındadır (1)/(4); beşinci tur eşiği gelmedi; hiçbir ret "iki kez reddedilmiş" olmadığından tekrar eden RET sayılmadı. Sonraki tur: 4. tur (`08_CROSS_REVIEW_R4.md`), `08` v0.13 üzerinde. Etki yansıtma (Faz 5) döngü TEMİZ olunca ayrı adımdır; üst doküman yansımaları `08 §5.2` satır 19 ve 20'de bekler.
