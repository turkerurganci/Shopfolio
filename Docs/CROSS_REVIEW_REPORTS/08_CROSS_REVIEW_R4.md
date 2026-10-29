# Cross-Review — 08 Integration Spec (Tur 4)

**Tarih:** 2026-10-29 · **Hedef:** `Docs/08_INTEGRATION_SPEC.md` v0.13 (girdi commit'i `8879b27`) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Koşum:** 1.–3. turla aynı iki koşum (tek parça çağrı `c1` ve altı parçalı `p1`…`p6`), aynı istem (ciddiyet ölçüsü ve mekanizma okuma paragrafıyla), aynı parça sınırları · **Sonuç:** **TEMİZ** (ciddiyet ölçüsünde) — altı ayrı bulgunun hepsi reddedildi.

## 1. Koşum

Betikler, istem ve parça sınırları 1. turun raporundadır (`08_CROSS_REVIEW.md` §1); bu turda aynıları kullanıldı: `C:\Users\TEKNOD~1\AppData\Local\Temp\claude\c--projects-Shopfolio\ab472d0d-cc27-40b7-9288-b725735c9991\scratchpad\cr08\` — `run_codex.sh` (`bash run_codex.sh in4 out4 c1 p1 p2 p3 p4 p5 p6`), `prompt.txt`, `partnote.txt`, `build_inputs.py`. Girdiler `in4\` (v0.13'ten kuruldu). Yedi çağrı paralel koştu, hepsi `rc=0` döndü (süreler 151–339 sn); kullanım sınırı dolmadı, yeniden koşum olmadı. Yasal iddialar resmî metne karşı okundu (Mesafeli Sözleşmeler Yönetmeliği m.12/4 — mevzuat.gov.tr; Elektronik Ticaret Aracı Hizmet Sağlayıcı ve Elektronik Ticaret Hizmet Sağlayıcılar Hakkında Yönetmelik m.9/1 ve m.34 — Resmî Gazete 29.12.2022; okuma tarihi 2026-10-29).

| Çağrı | Sonuç |
|---|---|
| `c1` tek parça | 2 bulgu |
| `p1` | TEMİZ |
| `p2` | 2 bulgu |
| `p3` | 1 bulgu |
| `p4` | TEMİZ |
| `p5` | 1 bulgu |
| `p6` | 3 bulgu |

Ham dokuz bulgu altı ayrı bulguya indi: H1 üç çağrıdan (`c1`-1, `p2`-1, `p6`-1), H2 iki çağrıdan (`c1`-2, `p5`-1) geldi.

## 2. Ham bulgular

İkinci modelin çıktıları düzenlenmeden (alıntı blokları içinde).

**(a) Tek parça çağrı:**
> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §2.1.5, “Tutar karşılaştırması” / DV-10
> Alıntı: “Fark saklanmadığı için (K-1255 (4)) ürün onu iade edemez.”
> Sorun: Taksit farkıyla müşteriden `Order.Total`’dan fazla tahsilat kabul ediliyor, fakat iade yalnız `Refund.Amount`/`Order.Total` üzerinden tasarlanıyor. Farkın dönmediği durumda müşteri tahsil edilmiş tutarın bir kısmını kaybeder; bu, caymada tahsil edilen tüm ödemenin masrafsız ve tek seferde iadesini zorunlu kılan Mesafeli Sözleşmeler Yönetmeliği m.12’ye aykırıdır. [Resmî metin](https://tuketici.ticaret.gov.tr/data/5e81982d13b876a1b04c7a42/2023-6502%20Say%C4%B1l%C4%B1%20T%C3%BCketicinin%20Korunmas%C4%B1%20Hakk%C4%B1nda%20Kanun.pdf)
> Öneri: DV-10 doğrulanmadan kart hattını canlıya açmayın. Tahsil edilen gerçek toplamı iade için izleyin ve sağlayıcının tam tutarı karta iade edeceğini doğrulayın; sağlayamıyorsa taksitli tahsilatı kapatın veya bu tutarı eksiksiz iade edebilen sağlayıcı/akış kullanın.
>
> BULGU-2
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §2.3, “Alan eşlemesi” — `sub`, `iss`, `aud`, `exp`, `iat`
> Alıntı: “Bir adres başka bir Google hesabına geçerse (süresi dolup yeniden alınan alan adı, kurumda yeniden verilen adres), o hesap aynı e-postalı mağaza hesabına girer: bağ ilk girişte kurulur (şifreli hesapta da), Google ile yeniden doğrulama da aynı eşleşmeyle tutar. Ürün bunu ayırt etmez.”
> Sorun: Doküman, Google’ın kalıcı ve tekrar kullanılmayan kimliği olan `sub`’ı saklamayı reddedip girişi değişebilir e-posta adresine bağlıyor. E-posta yeniden tahsis edilirse yeni Google hesabı mevcut müşteri hesabına erişir; sipariş ve kişisel verilere yetkisiz erişim doğar. Google da `sub`’ın saklanarak hesapla ilişkilendirilmesini, e-postanın kimlik olarak kullanılmamasını belirtir. [Google doğrulama rehberi](https://developers.google.com/identity/gsi/web/guides/verify-google-id-token)
> Öneri: İlk bağda doğrulanmış `iss` + `sub` çiftini saklayın; sonraki Google girişlerinde bu çiftin eşleşmesini zorunlu tutun. E-posta yalnız mevcut hesabı bulmak için kullanılmalı; eski e-posta-temelli bağlar güvenli bir yeniden bağlama akışıyla taşınmalıdır.
>
> SONUÇ: 2 BULGU

**Parça 1:**
> SONUÇ: TEMİZ

**Parça 2:**
> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §2.1.5 — Tutar karşılaştırması / iade etkisi
> Alıntı: "Geç gelen ödemede `Order.Total`'dır; taksit farkı girmez"
> Sorun: Sistem iade tutarını müşteriden tahsil edilen `total_amount` yerine `Order.Total` ile sınırlandırıyor. Taksit farkı müşteriden tahsil edilmişse bu farkın iade edilmemesi, tahsil edilen tüm ödemelerin kullanılan ödeme aracına uygun ve tek seferde iadesini zorunlu kılan [Mesafeli Sözleşmeler Yönetmeliği m.12/4](https://tuketici.ticaret.gov.tr/data/5e819a8e13b876a1b04c7a4a/Mesafeli%20S%C3%B6zle%C5%9Fmeler%20Y%C3%B6netmeli%C4%9Fi.pdf) ile çelişir. Metin bunu firmanın ürün dışı yükümlülüğü sayıyor; ancak entegrasyon, farkın iadesini gerçekleştiren veya güvenilir biçimde takip eden bir akış tanımlamıyor. Bu müşterinin eksik iade almasına yol açar.
> Öneri: DV-10 sonuçlanana kadar kartlı canlı açılışı engellensin; tahsil edilen toplamın iade edilmesi gerekiyorsa farkı da aynı kart aracına iade edecek sağlayıcı/manuel operasyon eşlemesi, kayıt ve sonuç doğrulaması açıkça tanımlansın.
>
> BULGU-2
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §2.1.9 — Sıklık
> Alıntı: "firmanın yolu her nedende aynıdır — yeniden dener ya da havale yolunu açar"
> Sorun: Kartla yapılan tüketici ödemesinin iadesinde havale, satın alımda kullanılan ödeme aracına uygun bir iade yöntemi değildir. Bu alternatif, özellikle sağlayıcının geçici ya da kalıcı retlerinde müşteriye kart iadesi yerine farklı araçla iade uygulanmasına izin verir; [Mesafeli Sözleşmeler Yönetmeliği m.12/4](https://tuketici.ticaret.gov.tr/data/5e819a8e13b876a1b04c7a4a/Mesafeli%20S%C3%B6zle%C5%9Fmeler%20Y%C3%B6netmeli%C4%9Fi.pdf) ile çelişir ve müşterinin iade hakkını riske atar.
> Öneri: Havale seçeneğini kartla ödenmiş siparişlerin yasal iade yolu olmaktan çıkarın. Sağlayıcı reddinde kart iadesinin yeniden denenmesi veya kart aracına uygun, tam tutarlı ve masrafsız alternatifin ne olduğu somut olarak tanımlansın.
>
> SONUÇ: 2 BULGU

**Parça 3:**
> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §2.2.4 — Alıcı başına gönderim sınırı ve kaçış
> Alıntı: "Yasal bedeli: siparişin alındığının işlem ortamının yanında e-postayla da bildirilmesi Hizmet Sağlayıcılar Yönetmeliği m.9/1'in ikinci koludur ... sınıra düşen ilk B-1'de bu kol, firma e-postayı panelden yeniden gönderene kadar karşılanmaz"
> Sorun: B-1, sipariş teyidi ve yasal metinlerin taşıyıcısı olduğu hâlde 20 iletilik sınıra takıldığında otomatik olarak gönderilmemekte; yalnız belirsiz süreli bir manuel yeniden gönderime bırakılmaktadır. Bu, teyit/yasal metinlerin zamanında sağlanmamasına izin verir; mevzuata aykırılık riskidir (1). Ayrıca dayanak gösterilen 2015 Yönetmeliği yürürlükten kaldırılmıştır ([RG 2022, m.34](https://resmigazete.gov.tr/eskiler/2022/12/20221229-5.htm)).
> Öneri: B-1’i sayaçtan muaf tutun veya sınır aşıldığında aynı anda tanımlı başka uygun kanaldan teyidi gönderin; yasal metin/teyit gönderilmeden sevkiyata izin verilmeyeceğini açıkça belirtin ve güncel mevzuat dayanağını yazın.
>
> SONUÇ: 1 BULGU

**Parça 4:**
> SONUÇ: TEMİZ

**Parça 5:**
> BULGU-1
> Kriter: Güvenlik
> Seviye: Yüksek
> Yer: §2.3, Alan eşlemesi — `sub`, `iss`, `aud`, `exp`, `iat`
> Alıntı: "Bir adres başka bir Google hesabına geçerse (süresi dolup yeniden alınan alan adı, kurumda yeniden verilen adres), o hesap aynı e-postalı mağaza hesabına girer: bağ ilk girişte kurulur (şifreli hesapta da), Google ile yeniden doğrulama da aynı eşleşmeyle tutar. Ürün bunu ayırt etmez."
> Sorun: Farklı bir Google hesabına yeniden atanmış e-posta, mevcut müşterinin hesabına ve sipariş/adres gibi kişisel verilerine erişim sağlar. Bu, açıkça öngörülen yetkisiz erişimdir; KVKK m.12’deki hukuka aykırı erişimi önleyecek uygun tedbir yükümlülüğüyle bağdaşmaz. Ayrıca “`sub` saklansa da kapanmazdı” değerlendirmesi teknik olarak yanlıştır: ilk bağdan sonra saklanan `sub`, yeni hesabın farklı `sub` değeriyle girişini engeller.
> Öneri: Google bağı ilk kurulduğunda `sub` değerini (gerekirse anahtarlı özet olarak) saklayın; bağlı hesaba sonraki Google girişlerinde `sub` eşleşmesini zorunlu kılın. Eşleşmezse oturum ve yeniden doğrulama izni vermeyin; güvenli hesap kurtarma/bağ teyidi akışına yönlendirin. Bu, `02` ve `06`daki `sub` saklamama kararını K kaydıyla güncellemeyi gerektirir.
>
> SONUÇ: 1 BULGU

**Parça 6:**
> BULGU-1
> Kriter: eksiklik
> Seviye: Yüksek
> Yer: §5.2, satır 15 / DV-10
> Alıntı: "DV-10 sonucu bekleniyor; sonuç farkın dönmediğini gösterirse karar kaydıyla işlenir"
> Sorun: Kartların veya taksitli kart ödemelerinin açılmasını engelleyen bir kapı yoktur. Taksit farkı müşteriden tahsil edilip iade edilemezse, karar sonradan alınana kadar müşteri eksik iade alır. Bu, Mesafeli Sözleşmeler Yönetmeliği m.12 kapsamındaki tüm tahsil edilen ödemelerin, masrafsız ve tek seferde iadesi yükümlülüğüne aykırıdır. [Ticaret Bakanlığı metni](https://tuketici.ticaret.gov.tr/data/5e81982d13b876a1b04c7a42/2023-6502%20Say%C4%B1l%C4%B1%20T%C3%BCketicinin%20Korunmas%C4%B1%20Hakk%C4%B1nda%20Kanun.pdf)
> Öneri: DV-10 olumlu kanıtlanana veya farkın firma tarafından aynı ödeme aracına iadesinin yöntemi karara bağlanana kadar taksitli kart ödemesini devreye alma yasağını açıkça yazın.
>
> BULGU-2
> Kriter: kullanıcı deneyimi
> Seviye: Yüksek
> Yer: §5.2, satır 19
> Alıntı: "Ödeme sonucunu elle uygulama komutu ... Bekliyor — etki yansıtma."
> Sorun: Sağlayıcı sonucu alınamayan kart siparişi için öngörülen tek çıkış yolu henüz ilgili dokümanlara yansıtılmamıştır. Müşterinin kartı tahsil edilmişken sipariş sonuçsuz kalabilir; tanımlı bir çözüm akışı olmadan müşteri çıkışsız, firma da tahsilatı güvenle uygulayamaz durumdadır.
> Öneri: Komutu ve sınırlarını ilgili spesifikasyonlara yansıtın: yalnız işaretli sipariş, PayTR paneliyle doğrulanmış sonuç, yetkili rol, değiştirilemez denetim kaydı ve müşteriye sonuç bildirimi.
>
> BULGU-3
> Kriter: para kaybı
> Seviye: Yüksek
> Yer: §5.2, satır 20
> Alıntı: "Anahtar kaldırma şartı ... Bekliyor — etki yansıtma."
> Sorun: PayTR anahtarlarının, geri ödemesi doğabilecek kart siparişleri varken kaldırılmasını engelleyen koşul henüz uygulanmamıştır. Anahtarlar kaldırılırsa gereken kart iadesi dışarıdan yapılamaz; müşterinin iade hakkı ve firmanın ödeme yükümlülüğü zarar görür.
> Öneri: Anahtar kaldırmayı, tüm olası geri ödeme yükümlülükleri sonuçlanana kadar teknik olarak engelleyin; sayaç ve kaldırma runbook’u bu engeli zorunlu ön koşul olarak taşısın.
>
> SONUÇ: 3 BULGU

## 3. Bağımsız değerlendirme

Altı ayrı bulgunun her biri bir `[dogrulama]` Opus / high doğrulayıcıya verildi (altı doğrulayıcı); hükümler `cr08\hukum4\` altında. Doğrulayıcılar önceki turların §3 tablolarını okudu. Taksit farkı (H1) ve alıcı sınırlı B-1 (H4) dördüncü kez geliyor ve R2 ile R3'te gerekçeyle iki kez reddedilmişti: **tekrar eden RET**, turun sonucunu belirlemez.

| # | Bulgu (kaynak) | Karar | Gerekçe | Uygulama |
|---|---|---|---|---|
| H1 | Taksit farkı iade edilmiyor; DV-10 kart hattı kapısı olmalı (`c1`-1, `p2`-1, `p6`-1; §2.1.5, DV-10, §5.2 satır 15) | RET — tekrar eden RET | R1 F1 (kısmi, K-1275), R2 G2 (ret), R3 G2 (ret) ile aynı yer ve iddia; yeni olgu, yeni dayanak ya da çelişki yok. `08` §2.1.5 farkın dönmemesinin m.12/1, m.12/2, m.12/4 ile çeliştiğini kendisi yazar; "DV-10 kapanana kadar fark firmanın yükümlülüğü" ve "dönmüyorsa karar kaydıyla `02`'ye döner" yazılıdır. K-1275 "kart hattını ya da taksidi kapatmak ürün kararıdır" diyerek kapı yolunu eledi. Ölçü dışı. | — |
| H2 | Google `sub` saklanmıyor, bağ e-postaya; yeniden tahsis edilen adres hesaba girer (`c1`-2, `p5`-1; §2.3) | RET | R1 F6'nın (kısmi, K-1279) ikinci gelişi. Yetkili olmayan adreste bağ artık teyit bağlantısıyla kurulur (K-1281, K-1282); kalan risk (yetkili adreste devir) bilinçli karardır ve `08` §2.3, `02` §3.13.7 "Kalan risk", `02` §8.1.2'de aynı sözle yazılı (K-900, K-1061, K-1272 (6)). `sub` saklamak, posta kutusunu ele geçirene açılan şifre sıfırlama yolunu (`02` §3.13.8, §3.13.9) daraltmaz — ek koruma katmanı. Çelişki yok (`06` §4.36 `sub` alanı taşımaz, `05` §6.1.4 aynı kuralı yazar). | — (yan gözlem K-1290) |
| H3 | Havale kartlı siparişin iade yolu olamaz (`p2`-2; §2.1.9) | RET | Bu yer için ilk gelişi. Cümle `02` §7.2.8'i özetler: havale yolu müşterinin seçimidir (K-569), IBAN alanını yalnız sipariş sayfası açar; Mesafeli Sözleşmeler Yönetmeliği m.12/4 (okuma 2026-10-29) "ödeme aracına uygun" ister ve metin kart iadesini varsayılan yol tutar (§2.1.2, B-14 metni). Havaleyi kaldırmak kalıcı hata kodlarında (`008`, `011`) müşterinin parasını alabileceği tek yolu kapatır — çıkışsız akış; K-1287'de aynı gerekçeyle elendi. | — |
| H4 | Alıcı sınırına düşen B-1 gönderilmez; "2015 Yönetmeliği yürürlükten kalktı" (`p3`-1; §2.2.4) | RET — tekrar eden RET | R1 F5 (kısmi, K-1278), R2 G3, R3 G4 ile aynı yer ve iddia. Açık `08` §2.2.4'te yazılıdır; telafisi firma yükümlülüğü (`02` §12.5, K-1278 ⚠); sayaçtan muafiyet K-1242 (1) ve K-1271'de, ikinci kanal `02` §9.1.1 ve K-1278'de elendi. Yeni olan yön olgusal olarak yanlış: `08`'in "Hizmet Sağlayıcılar Yönetmeliği m.9/1" atfı 2015 metni değil RG 29.12.2022/32058 Yönetmeliğidir (m.9/1 ikinci kol doğru okunmuş); m.34/1 yalnız 2015 metnini kaldırır. | — |
| H5 | Elle sonuç uygulama komutu üst dokümanlara yansıtılmamış (`p6`-2; §5.2 satır 19) | RET | Komut `08` §2.1.2'de tanımlı (kapsam, dayanak, kayıt; K-1283); "Bekliyor — etki yansıtma" izlenen bir parktır, çelişki ya da çıkışsız akış değil (kalite döngüsü TEMİZ olunca Faz 5 ayrı adımdır; R2 ve R3 aynı cümleyi taşır). | — |
| H6 | Anahtar kaldırma şartı uygulanmamış (`p6`-3; §5.2 satır 20) | RET | Şart `08` §2.1.2'de eksiksiz yazılıdır (K-1287); `05` §8.5.6 sayacı etki yansıtmada. "Teknik olarak engelle" önerisi uygulanamaz (anahtarlar kurulumu yapanın sırrıdır, K-911, K-1239 (2)) ve "mutlak engel" K-1287'de elendi; ek koruma katmanı. | — |

## 4. Ek bulgular

Doğrulayıcıların yan gözlemleri:

- **EK-1 (H5 ve H6 doğrulayıcıları, iki ayrı doğrulayıcı):** §5.2 giriş cümlesi "İki satır bekler: satır 14 …, satır 15 …" diyordu; tabloda satır 19 ve 20 de "Bekliyor" — çelişki (4), düşük şiddet. **Uygulandı (K-1290):** "Dört satır bekler".
- **EK-2 (H2 doğrulayıcısı):** §2.3 `sub` satırındaki "`sub` saklansa da kapanmazdı — bağ ilk girişte e-postayla kurulurdu" gerekçesi bağ kurulduktan sonrası için olgusal olarak yanlış (saklanan `sub` farklı hesabı reddederdi); asıl neden şifre sıfırlamayla girişin açık olması (`02` §3.13.9). **Uygulandı (K-1290):** gerekçe düzeltildi, sonuç (kalan risk) değişmedi. Bu, `p5` bulgusunun reddedilen kısmının ardındaki gerçek bir metin hatasıdır.
- **EK-3 (H3 doğrulayıcısı):** §2.1.9 cümlesine "müşterinin seçimine" eklemek okunurluk iyileştirmesi; ölçü dışı, uygulanmadı.
- **EK-4 (H3 doğrulayıcısı):** §2.1.9 `010` (net bakiye yetersiz) kodunu kalıcı sayıyor, oysa bakiye dolunca düzelebilir; panel hata nedenini ayırmadığı ve yeniden deneme açık olduğu için ölçü dışı; uygulanmadı.
- **EK-5 (H4 doğrulayıcısı):** "Hizmet Sağlayıcılar Yönetmeliği" kısa adına RG tarih ve sayısı eklemek üslup; uygulanmadı.
- **EK-6 (H1 doğrulayıcısı):** R3 EK-1 (DV-10 "canlı kanıt" yolu taksitli test işlemini zorunlu kılmıyor) hâlâ açık; ölçü dışı, etki yansıtmada ele alınabilir.

## 5. Kullanıcı onay checklist'i

- [ ] H1 — RET (tekrar eden)
- [ ] H2 — RET
- [ ] H3 — RET
- [ ] H4 — RET (tekrar eden)
- [ ] H5 — RET
- [ ] H6 — RET
- [x] EK-1, EK-2 — iki metin farkı (K-1290)

## Sonuç

**SONUÇ: TEMİZ — ciddiyet ölçüsünde, 4. tur.** Altı ayrı bulgunun (dokuz ham) hepsi reddedildi: kabul ve kısmi yok. Skill'in çıkış kuralı: H1 ve H4 tekrar eden RET'tir ve turun sonucunu belirlemez; kalan dört bulgu ölçü dışı ya da izlenen parktır (mevzuata aykırılık, para ya da hak kaybı doğuran boşluk, çıkışsız akış ya da çelişki yoktur). İki düşük şiddetli metin farkı doğrulayıcıların yan gözleminden bulundu ve uygulandı (`08` v0.14, K-1289, K-1290). Kapanmayan not: yok — R3 EK-1 ve R4 EK-3…EK-6 ölçü dışıdır, etki yansıtmada gözden geçirilebilir. Sıradaki adım Faz 5 etki yansıtma (üst doküman yansımaları `08` §5.2 satır 14, 19 ve 20'de bekler); döngü bu turla kapandı.
