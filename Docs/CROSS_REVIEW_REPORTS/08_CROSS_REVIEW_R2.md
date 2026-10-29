# Cross-Review — 08 Integration Spec (Tur 2)

**Tarih:** 2026-10-29 · **Hedef:** `Docs/08_INTEGRATION_SPEC.md` v0.11 (girdi commit'i `c7c7962`) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Koşum:** 1. turla aynı iki koşum (tek parça çağrı `c1` ve altı parçalı `p1`…`p6`), aynı istem, aynı parça sınırları · **Sonuç:** **TEMİZ DEĞİL** — dokuz ayrı bulgu; dördü kısmen kabul edilip uygulandı, beşi reddedildi.

## 1. Koşum

Betikler, istem ve parça sınırları 1. turun raporundadır (`08_CROSS_REVIEW.md` §1); bu turda aynıları kullanıldı: `C:\Users\TEKNOD~1\AppData\Local\Temp\claude\c--projects-Shopfolio\ab472d0d-cc27-40b7-9288-b725735c9991\scratchpad\cr08\` — `run_codex.sh` (`bash run_codex.sh in2 out2 c1 p1 p2 p3 p4 p5 p6`), `prompt.txt`, `partnote.txt`, `build_inputs.py`. Girdiler `in2\` (v0.11'den kuruldu). Yedi çağrı paralel koştu, hepsi `rc=0` döndü (süreler 67–307 sn); kullanım sınırı bu koşumda dolmadı (yürütücünün kesintisi doğrulama aşamasındaydı; Codex yeniden koşulmadı). Yasal iddialar resmî metne karşı okundu (Mesafeli Sözleşmeler Yönetmeliği indirilmiş metni, Resmî Gazete, okuma tarihi 2026-10-29); PayTR alan değerleri `dev.paytr.com/durum-sorgu` belgesinden yeniden okundu (2026-10-29).

| Çağrı | Sonuç |
|---|---|
| `c1` tek parça | 3 bulgu |
| `p1` | 1 bulgu |
| `p2` | 2 bulgu |
| `p3` | 1 bulgu |
| `p4` | 2 bulgu |
| `p5` | 1 bulgu |
| `p6` | 1 bulgu |

Ham on bir bulgu dokuz ayrı bulguya indi: G2 iki çağrıdan (`c1`-2, `p2`-1), G3 iki çağrıdan (`c1`-3, `p3`-1) geldi.

## 2. Ham bulgular

İkinci modelin çıktıları düzenlenmeden (alıntı blokları içinde).

**(a) Tek parça çağrı:**
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §2.1.7 (2), DV-11
> Alıntı: "Ürün ikinci tahsilatı görmez"
> Sorun: Sağlayıcı aynı `merchant_oid` için ikinci tahsilatı kabul ederse, ikinci `success` bildirimi aynı üçlü nedeniyle tekrar sayılıp yok sayılır; ikinci para tahsilatı ne görünür ne de iade edilir. DV-11 bu riski kapatan bir canlıya-alma kapısı değildir. Bu, müşteriye doğrudan para kaybı doğurur.
> Öneri: DV-11 doğrulanmadan kart yöntemini açmayın; ikinci tahsilat mümkünse ikinci tahsilatı güvenilir biçimde ayırıp otomatik iade edecek bir sağlayıcı eşlemesi kurun veya sağlayıcıyı değiştirin.
>
> BULGU-2
> Kriter: Mevzuata aykırılık
> Seviye: Yüksek
> Yer: §2.1.5 “İadeye etkisi”, §2.1.9
> Alıntı: "Fark saklanmadığı için ... ürün onu iade edemez."
> Sorun: Taksit farkıyla müşteriden `Order.Total` üstünde tahsil edilen `total_amount` saklanmıyor ve iade yalnız `Order.Total` üzerinden yapılıyor. Fark sağlayıcı veya banka tarafından dönmezse müşteri tahsil edilen tutarın bir kısmını kaybeder. Bu, cayma ve iptalde tahsil edilen tüm ödemelerin, tüketiciye yük getirmeden ve tek seferde iadesini zorunlu tutan Mesafeli Sözleşmeler Yönetmeliği m.12/4 ile çelişir. [Resmî Gazete](https://resmigazete.gov.tr/eskiler/2025/05/20250524-2.htm)
> Öneri: DV-10 kapanana kadar taksitli kart ödemesini açmayın; farkın kim tarafından ve hangi kanalla iade edildiğini doğrulayın. Ürün iade sorumluluğundaysa tahsil edilen toplamı ve farkı saklayıp tam iade akışını tanımlayın.
>
> BULGU-3
> Kriter: Mevzuata aykırılık
> Seviye: Orta
> Yer: §2.2.4 “Aşım” ve “Kalan risk”
> Alıntı: "eşik dolduktan sonra aynı adrese oluşan müşteri bildirimi gönderilmez"
> Sorun: Misafir siparişiyle bir adres için limit kasıtlı olarak doldurulduğunda, gerçek müşterinin B-1 sipariş teyidi hiç gönderilmez; yalnız paneldeki firmaya manuel yeniden gönderim görevi kalır. Bu, sipariş alındısının elektronik ticaret ortamına ek olarak e-posta, kısa mesaj veya telefon gibi araçlardan biriyle gecikmeksizin bildirilmesi yükümlülüğünü karşılamaz. [Elektronik Ticaret Yönetmeliği m.9](https://resmigazete.gov.tr/eskiler/2022/12/20221229-5.htm)
> Öneri: B-1’i alıcı başına gönderim sınırından muaf tutun veya limit aşımında gecikmeksizin çalışan, kayıtlı ve otomatik alternatif teyit kanalını tanımlayın.
>
> SONUÇ: 3 BULGU

**Parça 1:**
> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §2.1.2 — Anahtarlar ve değişim
> Alıntı: “sorulamıyorsa o işlemlerin son sorgusu yanıt alamaz, sipariş **iptal edilmez** […] kurulumu yapan sonucu PayTR panelinden okuyup firmaya bildirir.”
> Sorun: Zorunlu anahtar yenilemesinde eski işlem sorgulanamıyorsa ve eski imzalı bildirim de reddediliyorsa, açık kart siparişinin sonucu üründe çözümsüz kalır. Panelden firmaya bilgi verilmesi, siparişi Ödendi/Başarısız durumuna alma veya tahsil edilmiş tutarı iade etme yolu tanımlamaz. Bu, müşterinin ödeme sonucu belirsiz ve çıkışsız kalmasına; tahsil edilmiş siparişin teslim edilmemesine yol açabilir (3).
> Öneri: Anahtar yenileme sonrası eski işlem sorgulanamazsa uygulanacak yetkili mutabakat yolunu tanımlayın: PayTR panelindeki kanıtla hangi rolün hangi sipariş durumunu nasıl kapatacağı, başarıda teslimatın nasıl başlatılacağı ve belirsiz/başarısız sonuçta müşteriye iade ya da alternatif ödeme seçeneğinin nasıl sunulacağı açıkça yazılsın.
>
> SONUÇ: 1 BULGU

**Parça 2:**
> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §2.1.5, “İadeye etkisi”
> Alıntı: “Geç gelen ödemenin geri ödemesinde de tutar ödenen toplamdır, yani `Order.Total` … `total_amount` değildir.”
> Sorun: Taksit/vade farkıyla müşteriden `total_amount` olarak daha yüksek tutar tahsil edildiğinde sistem yalnız `Order.Total` iade eder; farkın müşteriye dönmesi garanti edilmemiştir. Metin de bunun müşteriye yük kalabileceğini kabul ediyor. Bu, tahsil edilen tüm ödemelerin kullanılan ödeme aracına uygun ve tüketiciye masraf yüklemeden iadesi yükümlülüğüyle çelişir; müşteri hak ve para kaybıdır. [Mesafeli Sözleşmeler Yönetmeliği m.12](https://tuketici.ticaret.gov.tr/data/5e81982d13b876a1b04c7a42/2023-6502%20Say%C4%B1l%C4%B1%20T%C3%BCketicinin%20Korunmas%C4%B1%20Hakk%C4%B1nda%20Kanun.pdf)
> Öneri: Kart yöntemini/taksiti canlıya açmadan önce, tahsil edilen toplam tutarın eksiksiz ve aynı ödeme aracından iadesini sağlayan PayTR davranışını doğrulayın; sağlanmıyorsa bu taksit akışını kapatın veya farkı eksiksiz iade eden doğrulanmış bir ödeme/iade akışı tanımlayın.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §2.1.5 “Bildirimin sınıflanması” ve §2.1.6 “Sorgu”
> Alıntı: “`currency` ≠ `TL` … bildirim işaretli kaydedilir” / “`status` | ödemenin sonucu | `success` → başarılı ödeme var”
> Sorun: Bildirim yolunda TL dışındaki ödeme uygulanmazken, son sorgu yanıtındaki `currency` alanı hiç doğrulanmadan `payment_amount`, `payment_total`, `odeme_tipi` ve `test_mode` koşullarıyla ödeme başarılı uygulanabilir. Böylece aynı yabancı para işlemi bildirimde reddedilip sorguda Ödendi yapılabilir; yanlış tutarlı tahsilat veya ödenmemiş siparişin teslimi doğar.
> Öneri: §2.1.6 alan eşlemesine `currency` ekleyin ve başarı uygulamasını, başlatılan işlemde beklenen para birimiyle (bu tasarımda TL) tam eşleşme koşuluna bağlayın; eşleşmeyeni işaretli kaydedip ödeme uygulamayın.
>
> SONUÇ: 2 BULGU

**Parça 3:**
> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §2.2.4 — Alıcı başına gönderim sınırı ve kaçış
> Alıntı: "**Yasal bedeli:** siparişin alındığının işlem ortamının yanında e-postayla da bildirilmesi Hizmet Sağlayıcılar Yönetmeliği m.9/1'in ikinci koludur (K-704); sınıra düşen ilk B-1'de bu kol, firma e-postayı panelden yeniden gönderene kadar karşılanmaz — ürünün ikinci kanalı yoktur (`02 §9.1.1`)."
> Sorun: Doküman, B-1’in 20 ileti sınırında hiç gönderilmemesini ve e-posta teyidinin belirsiz süreyle manuel işleme kalmasını öngörüyor. Bu, siparişin alındığının gecikmeksizin elektronik ortam dışında en az bir araçla bildirilmesi yükümlülüğünü doğrudan karşılamaz; hukuki yükümlülük ihlalidir.
> Öneri: B-1’i alıcı başına gönderim sınırından muaf tutun veya sınır aşıldığında gecikmeksizin gönderilecek ikinci bildirim kanalını tanımlayın; manuel panel yeniden gönderimini uyumun tek telafisi olmaktan çıkarın.
>
> SONUÇ: 1 BULGU

**Parça 4:**
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §2.2.7, F-5
> Alıntı: "IBAN'ın son dört hanesi `Order.BankTransferIban`'ın ya da `CompanySettings`'in IBAN alanının yazılışından alınır"
> Sorun: F-5, bir şirket ayarı değişikliğini bildirir; belirli bir siparişe bağlı değildir. Ayrıca güncel `CompanySettings` değeri, e-posta kuyrukta beklerken ikinci kez değişmiş olabilir. Bu durumda e-posta, bildirilen değişikliğin değil sonraki IBAN’ın son dört hanesini gösterir. Güvenlik bildirimi yanlış değişikliği işaretleyerek yetkisiz IBAN değişikliğinin fark edilmesini geciktirebilir; firmaya para kaybı riski doğurur.
> Öneri: IBAN’ın yalnız son dört hanesini, değişiklik anında F-5 gönderim kaydının olay değerine yazın; e-posta gövdesini yalnız bu donmuş değerden üretin. `Order.BankTransferIban` ve gönderim anındaki güncel `CompanySettings` okumasını kaldırın.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §2.2.6, B-7
> Alıntı: "Yönetici iptalinde ek paragraf (Mesafeli Sözleşmeler Yönetmeliği m.16/4'ün bildirimi...)" ve "Stok sebebinde de aynı metin gider"
> Sorun: Stokta bulunmama, Mesafeli Sözleşmeler Yönetmeliği m.16/4 bakımından açıkça ifanın imkânsızlaşması sayılmaz. Buna rağmen şablon, stok yokluğu kaynaklı iptali m.16/4 bildirimi gibi ele alıp aynı metni zorunlu kılıyor. Bu, tüketicinin sözleşmenin ifasına ilişkin hakkını fiilen yalnız iade vaadine indirgeme riski taşır. [Yönetmeliğin güncel değişikliği](https://resmigazete.gov.tr/eskiler/2022/08/20220823-2.htm) stok yokluğunun imkânsızlık sayılmadığını düzenler.
> Öneri: M.16/4 metnini yalnız gerçek ifa imkânsızlığı durumuna bağlayın. `OutOfStock` için B-7 metnini ve tetik koşulunu, ürün gereksinimlerindeki hukuka uygun çözüm ve tüketici onayıyla uyumlu ayrı bir akışa bağlayın.
>
> SONUÇ: 2 BULGU

**Parça 5:**
> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: Aşama 4 park girdileri · K-892 / K-893 / K-894 / K-897 / K-940 / K-941 / K-959 / K-970 / K-974 / K-978
> Alıntı: "e-postanın gövdesi gönderim anında olayın değerlerinden ve siparişin donmuş verisinden üretilir ve saklanmaz"
> Sorun: B-1’in gövdesi Ön Bilgilendirme Formu ve Mesafeli Satış Sözleşmesini içerir. Bu gövdenin hiç saklanmaması, işlem anındaki hukuki metnin ve tüketiciye iletilen sürümün sonradan aynen ispatını imkânsızlaştırır. Mesafeli Sözleşmeler Yönetmeliği m.20/1, bu yükümlülüklere ilişkin her işleme ait bilgi ve belgelerin üç yıl saklanmasını zorunlu kılar. Bu, yürürlükteki mevzuata aykırılık ve uyuşmazlıkta firma ile tüketici bakımından hak kaybı riskidir.
> Öneri: “Gövde saklanmaz” kuralını, B-1’in işlem anındaki değişmez hukuki metin sürümü, doldurulmuş değerleri ve gönderim kanıtı için istisnalandırın; bunları erişimi sınırlı şekilde en az üç yıl saklayın. Diğer e-posta gövdeleri için saklamama kuralı ayrı kalabilir.
>
> SONUÇ: 1 BULGU

**Parça 6:**
> BULGU-1
> Kriter: Kullanıcı deneyimi
> Seviye: Yüksek
> Yer: §5.2, satır 14
> Alıntı: "`frame-ancestors 'self'` sipariş sayfasından geçiş sayfasına taşınır ... `frame-src`'si `'self'` ve `https://www.paytr.com`'dur (çerçevede yüklenen geçiş sayfası için)"
> Sorun: Geçiş sayfası PayTR çerçevesi içinde yükleniyorsa üst zincirde PayTR alan adı bulunur. `frame-ancestors 'self'` yalnız Shopfolio kökenli tüm üst çerçevelere izin verir; PayTR üst çerçevesini reddeder. Bu nedenle geçiş sayfası hiç çalışamaz ve müşterinin ödeme dönüşünde sipariş sayfasına üst pencereden geçişi engellenir. Dokümanın kendi tarif ettiği dönüş mekanizması çalışmadığından kullanıcı ödeme sonrası çıkışsız kalabilir (3) ve §5.1’deki çerçeve dönüşüyle çelişir (4).
> Öneri: DV-5 ile gerçek üst pencere/çerçeve zincirini doğrulayın. Dönüş çerçevede gerçekleşiyorsa geçiş sayfasının `frame-ancestors` politikasına gerekli PayTR kökenini de ekleyin (ör. `'self' https://www.paytr.com`); üst pencerede gerçekleşiyorsa geçiş sayfasını çerçevede yükletmeyin. Seçilen yolu §5.1, `05`, `07` ve kabul senaryosunda aynı anda netleştirin.
>
> SONUÇ: 1 BULGU

## 3. Bağımsız değerlendirme

Dokuz ayrı bulgunun her biri bir `[dogrulama]` Opus / high doğrulayıcıya verildi (dokuz doğrulayıcı). G2 ve G3, 1. turda değerlendirilmiş iddialardır (F1, F5); yeni bilgi taşımadıkları için RET'le kapandı.

| # | Bulgu (kaynak) | Karar | Gerekçe | Uygulama |
|---|---|---|---|---|
| G1 | Aynı `merchant_oid` ile ikinci tahsilat görülmez (`c1`-1; §2.1.7 (2), DV-11) | KISMİ ⚠ | **Gerçek kısım:** DV-3, DV-6, DV-7'nin taşıdığı "kart anahtarları tanımlanmaz" kapısı DV-11'de yoktu; `02 §6.2.16`/K-1211 iade güvencesi PayTR'de doğmuyor. **Reddedilen kısım:** "sağlayıcıyı değiştirin" ve tespit mekanizmasını şimdi kurmak K-1270 (4)'ün bilinçli kararına itiraz; "doğrudan para kaybı" abartılı (fazla tahsilat ekstrede görünür). | K-1284 ⚠: DV-11 kapısı, §2.1.7 (1)–(2), DV-11 kanıtı |
| G2 | Taksit farkı iade edilmiyor (`c1`-2, `p2`-1; §2.1.5, §2.1.9) | RET | 1. turun F1'i; güncel metin m.12/4'ü ve firmanın yükümlülüğünü zaten yazıyor (K-1275). Yeni olgu yok. "Kart hattını açmayın" ürün kararıdır, bilinçli elendi. | — |
| G3 | Alıcı sınırına düşen B-1 (`c1`-3, `p3`-1; §2.2.4) | RET | 1. turun F5'i; m.9/1 (RG 29.12.2022/32058, okuma 2026-10-29) ikinci kolunun açığı ve firmanın yeniden gönderme yükümlülüğü zaten yazılı (K-1278, `02 §12.5`); iki öneri K-1242, K-1271, K-1278'de elendi. | — |
| G4 | Anahtar yenilemesi sonrası sorgulanamayan kart siparişinin sonucu çözümsüz (`p1`-1; §2.1.2) | KISMİ ⚠ | **Gerçek kısım:** mutabakat yolu hiçbir yerde tanımlı değildi; kartta Bekliyor'dan Ödendi'ye tek tetikleyici sağlayıcının bildirimi, panelde başarılı görünen sipariş teslimata gidemiyor — (3). **Reddedilen kısım:** başarısız/belirsiz dal çıkışsız değil (firma siparişi iptal eder). | K-1283 ⚠: §2.1.2 mutabakat cümlesi; `05 §8.5` komutu ve `02`/`03`/`04`/`DEPLOY_RUNBOOK` yansıması etki yansıtmaya (§5.2 satır 19) |
| G5 | Sorguda `currency` doğrulanmıyor (`p2`-2; §2.1.6) | KISMİ | **Gerçek kısım:** alan belgede var, bildirim yolu koşulu arıyor, sorgu yolu aramıyor — (4). **Reddedilen kısım:** "Yüksek/yanlış tutarlı tahsilat" abartılı (jeton isteğinde `currency` sabit `TL`); önerideki "yalnız `TL`" yanlış (sorgu `TL`/`TRY` döner). **Yan gözlem (doğrulandı):** sorgunun `odeme_tipi` değeri `KART`, 08 `CART` yazıyordu. | K-1286: §2.1.6 `currency` satırı, `KART`, `auth_code`/`auth_date`, DV-12 |
| G6 | F-5'in IBAN son dört hanesi `Order.BankTransferIban`'dan/güncel ayardan alınır (`p4`-1; §2.2.7) | KISMİ | **Gerçek kısım:** cümle yanlış kaynağı anıyor (F-5 siparişe bağlı değil) ve zamanı söylemiyor; `06` eşlemesi "izden" diyor. **Reddedilen kısım:** donmuş değer ilkesi `05 §4.4.1`'de ve 08 "Değerlerin saklanışı"nda zaten var; kuyruktaki e-posta ikinci değişiklikten etkilenmez. | K-1285: F-5 değer cümlesi; `06` eşlemesi etki yansıtmaya |
| G7 | B-7 stok iptalini m.16/4 bildirimi sayıyor (`p4`-2; §2.2.6) | RET | m.16/4 son cümlesi doğru ("stokta bulunmaması imkânsızlık sayılmaz") ama bu 02 §7.2.4'ün (K-609) bilinçli kararıdır; paragraf imkânsızlık bildirmez, yalnız iade süresini yazar. 08 ile 02 tutarlı. | — |
| G8 | B-1 gövdesi saklanmıyor (m.20/1) (`p5`-1; başlık notu) | RET | m.20/1 belge ve bilgiyi üç yıl saklamayı ister, e-postanın kendisini değil; iki yasal metin onay anında siparişe donmuş yazılıyor (`06 §4.17`, K-1027), sürüm kaydı silinmez, gönderim kaydı on yıl saklanır. | — |
| G9 | Geçiş sayfasının `frame-ancestors 'self'` ayarı PayTR çerçevesinde çalışmaz (`p6`-1; §5.2 satır 14) | RET | Mekanizma yanlış: `frame-ancestors` yüklenen belgenin çerçevesini içeren üst çerçevelerin o anki belgelerine bakar; dönüşte PayTR belgesi geçiş sayfasıyla yer değiştirir, tek ata Shopfolio'dur. Doküman çelişmiyor (§2.1.4 madde 3, K-1259, K-1274). Önerilen genişletme zararlı olurdu. | — (DV-5'in iç içe çerçeve durumu doğrulayıcının düşük öncelikli yan gözlemidir; uygulanmadı) |

## 4. Ek bulgular

- **EK-1 (G1 doğrulayıcısı):** `12` park bloğundaki "farklı işlem kimliğiyle gelen ikinci başarılı ödeme iade edilir" doğrulaması PayTR'de üretilemez (işlem kimliği `merchant_oid`'dir) — `08 §5.2` satır 19'a yazıldı; `12` etki yansıtmada güncellenir.
- **EK-2 (G5 doğrulayıcısı):** `odeme_tipi` `CART` → `KART` düzeltildi (K-1286).
- **EK-3 (G4 doğrulayıcısı):** anahtar yenilendikten sonra eski işlemin sorgusunun `001`/`003` mü `004` mü döneceği yazılı değil; `004` dönerse ödenmiş sipariş iptal olur. DV-15'in "yanlış çıkarsa" hükmünde bu ayrımın yazılması 3. turda ya da etki yansıtmada ele alınır (bu turda uygulanmadı — doğrulanmamış varsayım).

## 5. Kullanıcı onay checklist'i

- [x] G1 — DV-11 kart hattı kapısı (K-1284 ⚠; öneriyle kaydedildi)
- [ ] G2 — RET
- [ ] G3 — RET
- [x] G4 — ödeme sonucunu elle uygulama komutu (K-1283 ⚠; öneriyle kaydedildi — yeni komut para durumunu elle değiştirir, aşama sonu ⚠ listesinde gösterilir)
- [x] G5 — sorguda `currency`, `KART` (K-1286)
- [x] G6 — F-5 IBAN kaynağı (K-1285)
- [ ] G7 — RET
- [ ] G8 — RET
- [ ] G9 — RET

## Sonuç

**SONUÇ: TEMİZ DEĞİL — 9 ayrı bulgu (11 ham), 4 kısmi kabul, 5 ret.** Skill'in çıkış kuralı: kabul edilen bulgular arasında para/hak kaybı sınıfından olanlar (G1, G4) var ve bütün çağrılar TEMİZ dönmedi; beşinci tur eşiği gelmedi. Sonraki tur: 3. tur (`08_CROSS_REVIEW_R3.md`), `08` v0.12 üzerinde. Etki yansıtma (Faz 5) döngü TEMİZ olunca ayrı adımdır; üst doküman yansımaları `08 §5.2` satır 19'da bekler.
