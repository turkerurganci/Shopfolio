# Cross-Review — 06 Data Model (Tur 2)

**Tarih:** 2026-10-18 · **Hedef:** `Docs/06_DATA_MODEL.md` v0.19 (girdi commit'i `230c89c`, 1 159 544 bayt, 6 012 satır) · **İkinci model:** Codex CLI — ChatGPT hesabı, `gpt-5.6-terra`, düşünme düzeyi yüksek (salt okuma) · **Ciddiyet ölçüsü:** 1. turdan itibaren uygulanır, mekanizma okuma paragrafıyla (K-1125) · **Koşum:** 1. turun biçimiyle (K-1126) — (a) tek parça çağrının yerine **en büyük iki birleşim**, (b) **yedi parçalı kontrol koşumu**; istem, parça notu, parça ve birleşim sınırları değişmedi; turun sonucu ikisinin birleşimidir · **Girdi:** yalnız doküman ve boş şablonu (`e74883f`), stdin'den tek metin, her çağrıda boş ve izole bir çalışma klasöründe; karar kaydı, öteki dokümanlar ve önceki turun raporu verilmedi (K-431) · **Sonuç:** TEMİZ değil — sekiz ayrı bulgu, 1 KABUL · 3 KISMİ · 4 RET; dördü uygulandı (`06` v0.20, Ürün Gereksinimleri v0.86; bir ⚠ karar — K-1127)

## 1. Koşum

**Betikler** 1. turun betikleridir (`06_CROSS_REVIEW.md` §1), değiştirilmeden kopyalandı: `C:\Users\TEKNOD~1\AppData\Local\Temp\claude\c--projects-Shopfolio\3a694994-cfb4-4b7e-a92e-eea67b22c07d\scratchpad\cr2\` — `build_inputs.py`, `run_codex.sh`, `prompt.txt`, `partnote.txt`; girdiler `cr2/in/`, çıktılar ve koşum kayıtları `cr2/out/`. SHA-256 (ilk 16 hane, betiğin hesabı): istem `7c31919b40b3209c`, parça notu `2b150c8107b170bb`, şablon `5d3e5023c4926bfe` — üçü 1. turla aynı —, doküman (v0.19) `0657f5edef34f161`. Kapsanmayan satır yok (betiğin denetimi). Koşumdan önce çağrı kısa bir deneme metniyle doğrulandı (oturum canlı).

**Birleşim 1'in sınır payı.** Doküman 1. turdan bu yana 1 409 bayt büyüdü; birleşim 1 975 381 karakterdir — aracın 1 048 576 karakterlik sınırının 73 195 karakter altında (1. turda 74 397). Birleşim değişmeden sığar; tek parça girdi 1 068 389 karakterdir ve yine sınırın üstündedir — koşulmadı (K-1126).

| Çağrı | Bölümler | Girdi | Token | Süre | Sonuç |
|---|---|---|---|---|---|
| Birleşim 1 | parça 2–7 — §1.1.5–§9 | 1 066 801 bayt (975 381 karakter) | 371 001 | 140 sn | 1 bulgu |
| Birleşim 2 | parça 1, 4–7 — §1'in girişi, §1.1.1–§1.1.4, §2–§9 | 722 104 bayt (662 368 karakter) | 257 333 | 180 sn | 1 bulgu |
| Parça 1 | §1'in girişi · §1.1.1–§1.1.4 | 157 144 bayt | 68 902 | 72 sn | `SONUÇ: TEMİZ` |
| Parça 2 | §1.1.5–§1.1.8 | 249 405 bayt | 83 787 | 104 sn | `SONUÇ: TEMİZ` |
| Parça 3 | §1.1.9–§1.3 | 307 975 bayt | 110 613 | 109 sn | 2 bulgu |
| Parça 4 | §2 · §3 | 126 730 bayt | 46 715 | 113 sn | 1 bulgu |
| Parça 5 | §4'ün girişi · §4.1–§4.35 | 259 507 bayt | 97 708 | 212 sn | 1 bulgu |
| Parça 6 | §4.36–§4.72 | 207 033 bayt | 79 362 | 134 sn | 1 bulgu |
| Parça 7 | §5 · §6 · §7 · §8 · §9 | 194 222 bayt | 86 042 | 235 sn | 2 bulgu |

**Web araması:** dokuz çağrının hiçbiri web araması yapmadı (koşum kayıtlarında `web search` satırı yok). Yasal dayanak anan iki bulgu (parça 6 · BULGU-1 — 6563 m.11/3; parça 7 · BULGU-1 — KVKK başvuru hakkı) bir web kaynağına bağlanmadı; hukuki kontrol §5'te.

## 2. Ham bulgular

İkinci modelin çıktıları, satır sonu boşlukları dışında düzenlenmeden (alıntı blokları içinde).

**(a) Tek parça çağrı:** koşmadı — girdi sınırı (§1; K-1126). Yerine:

**Birleşim 1 — §1.1.5–§9:**

> ```text
> BULGU-1
> Kriter: güvenlik
> Seviye: Yüksek
> Yer: §4.17 `Order.ConfirmationKey`; §9.2.1
> Alıntı: “aynı anahtarla ikinci gönderim yeni sipariş açmaz, var olan siparişin sayfasına götürür”
> Sorun: `ConfirmationKey` sipariş sayfasına erişim sağlayan değer olarak tanımlanmış; ancak imhada boşaltılmayıp kalan ticari kayıtta tutuluyor. Aynı bölüm, `OrderAccessKey` boşaltılınca “sipariş sayfasına hiçbir yoldan girilemez” diyor. Bu iki ifade çelişir. Anahtarın sızması veya eski ödeme formunun yeniden kullanımı, sipariş kişisel verileri dururken yetkisiz sipariş erişimine yol açabilir. (4; ayrıca kişisel verinin yetkisiz açılması riskiyle 1)
> Öneri: `ConfirmationKey`i yalnız kısa ömürlü, oturum/istek bağlamına bağlı idempotency anahtarı yapın; sipariş oluşturulduktan sonra sayfa erişimi vermesin. Sipariş sayfası yalnız tanımlı erişim yollarıyla açılmalı; anahtar imhada da boşaltılmalıdır.
>
> SONUÇ: 1 BULGU
> ```

**Birleşim 2 — §1'in girişi, §1.1.1–§1.1.4, §2–§9:**

> ```text
> BULGU-1
> Kriter: Teknik doğruluk
> Seviye: Yüksek
> Yer: §4.34 RefundRecord; §7.1, §7.4
> Alıntı: “firmanın yeniden denemesi ve havale yolu yalnız "gerçekleşmedi" hâlinde açılır” / “sonuç siparişe işlenmez”
> Sorun: Yönetici bir iade isteğini “gerçekleşmedi” diye işaretledikten sonra geç başarılı sonuç gelirse kayıt `Failed` hâlinde kalır. Bu hâl yeniden deneme veya havale yolunu açar; `LateSuccessAt` ise yeniden ödeme isteğini engelleyen tekil kısıtlara dahil değildir. Böylece para gerçekte iade edilmişken ikinci iade başlatılabilir. Bu, müşteriye/firmaya para kaybı riski doğurur (sınıf 2).
> Öneri: `LateSuccessAt` dolduğunda geri ödemeyi yeniden denemeye ve havale yoluna kapatan bir koşul ekleyin; ayrıca geç başarıyı geri ödemenin gönderilmiş/sonuçlanmış hâline güvenli biçimde yansıtın veya zorunlu mutabakat tamamlanana dek ikinci ödeme isteğini veritabanı koşuluyla engelleyin.
>
> SONUÇ: 1 BULGU
> ```

**Parça 1 — başlık notu, §1'in girişi, §1.1.1–§1.1.4:**

> ```text
> SONUÇ: TEMİZ
> ```

**Parça 2 — başlık notu, §1.1.5–§1.1.8:**

> ```text
> SONUÇ: TEMİZ
> ```

**Parça 3 — başlık notu, §1.1.9–§1.3:**

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §1.1.9 / `03 §4.1.34`; Aşama 4 park girdisi K-875–K-877
> Alıntı: “Z-30 işlem izi — yılın sonundan; değiştirilemez, silinemez” / “süresi dolan satırı veritabanındaki bir imha fonksiyonu siler”
> Sorun: İşlem izi için biri mutlak silinmezlik, diğeri süre dolunca fiziksel silme öngörüyor. İz satırlarının yönetici adı ve e-posta adresi taşıdığı da belirtilmiş. Mutlak yorum, süreli imhayı engelleyerek kişisel verinin gereğinden uzun saklanmasına yol açar (1).
> Öneri: “Silinemez” ifadesini uygulama kullanıcısı için sınırlandırın: “uygulama tarafından değiştirilemez veya silinemez; Z-30 sonunda yalnız yetkili imha fonksiyonu fiziksel siler.”
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: Aşama 4 park girdisi K-891…K-985; §1.1.9 / `03 §1.7.3.3`
> Alıntı: “sonradan gelen başarılı sonuç … siparişe işlenmez, gelen bildirim kaydında ya da geri ödeme kaydında işaretli kalır” / “iade gerçekleşince … kalkar”
> Sorun: Yönetici iadeyi “gerçekleşmedi” diye işaretledikten sonra geç gelen başarılı sağlayıcı sonucu siparişe işlenmeyecek ve işaretli kalacak deniyor; aynı işaretin iade gerçekleşince kalkacağı da söyleniyor. Bu çelişki, fiilen yapılmış kart iadesinin başarısız görünmesine ve yeniden iade ile mükerrer ödeme ya da müşterinin alacağının yanlış yönetilmesine yol açabilir (2).
> Öneri: Geç başarı sonucunun tekil geri ödeme kaydını idempotent biçimde uzlaştırıp uzlaştırmayacağını açıkça belirleyin. Uzlaştırılacaksa iade kaydını başarılı yapmalı, işareti kaldırmalı ve yeni iade isteğini engellemelidir; uzlaştırılmayacaksa işaretin kalkma koşulundan geç başarıyı açıkça ayırıp zorunlu uzlaştırma kaydı tanımlanmalıdır.
>
> SONUÇ: 2 BULGU
> ```

**Parça 4 — başlık notu, §2–§3:**

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §2.2.1; §3.1, `Customer` ve `PendingEmailChange`
> Alıntı: “Olayla silinir — hesabın silinmesi (Z-31) ya da bekleyen kaydın ömrü (Z-1) veya adresinin değişiklikte doğrulanması ya da geri alınması”
> Sorun: Bu ifade `Customer` kaydının e-posta değişikliği doğrulanınca veya geri alınınca silineceğini söylüyor. Buna karşılık `PendingEmailChange` satırı, doğrulamada silinmesi gereken kaydın bekleyen değişiklik kaydı olduğunu belirtir. Başarılı e-posta değişikliğinin müşteri hesabını, oturumlarını ve bağlı verilerini silmesi müşterinin hesabını kaybetmesine ve akışın çıkışsız kalmasına yol açar. (2) ve (3).
> Öneri: `Customer` için silme tetiklerini hesap silinmesi ve varsa doğrulanmamış ilk kayıt ömrüyle sınırlayın. E-posta değişikliğinin doğrulanması veya geri alınmasında yalnız `PendingEmailChange` silinsin; müşteri hesabı korunarak e-posta alanları güncellensin.
>
> SONUÇ: 1 BULGU
> ```

**Parça 5 — başlık notu, §4'ün girişi, §4.1–§4.35:**

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §4.27, §4.28, §4.31
> Alıntı: “aynı beyanla girilen IBAN'ı birden çok kalem kullanır” / “havale hattında müşterinin IBAN'ı retle silinir” / “havale hattında müşterinin IBAN'ı kapanışla silinir”
> Sorun: Aynı IBAN birden çok kalemin bekleyen havale iadesine bağlı olabilirken, bir kalemdeki iade reddi veya “mal dönmedi” kapanışı IBAN kaydını fiziksel olarak silmektedir. Gelen bağlar boşaltıldığı için diğer kalemin karşılanmış IBAN isteği IBAN’sız kalır; yeniden IBAN istemini açan zorunlu bir işlem de tanımlanmamıştır. Bu, müşteriye ait başka bir geri ödemenin yapılamamasına yol açabilir (para/hak kaybı).
> Öneri: İade reddi ve “mal dönmedi” kapanışında IBAN’ı yalnız başka bekleyen geri ödeme veya kalem bağı yoksa silin. Kullanım sürüyorsa IBAN’ı koruyun; silinmesi zorunluysa etkilenen her bekleyen geri ödeme için aynı işlemde yeni açık IBAN isteği oluşturun.
>
> SONUÇ: 1 BULGU
> ```

**Parça 6 — başlık notu, §4.36–§4.72:**

> ```text
> BULGU-1
> Kriter: Tutarlılık
> Seviye: Yüksek
> Yer: §4.58 DispatchRecord — saklama başlangıcı / `SentAt`
> Alıntı: “Saklamanın başlangıç değeri — Z-34 ve Z-43'ün "gönderim anı" — satırın oluşturulma anıdır”
> Sorun: Aynı bölüm, gerçek gönderim anını servis iletinin kabul edildiğinde yazılan `SentAt` olarak tanımlarken saklama süresini kuyruk satırının daha erken oluştuğu andan başlatıyor. Kuyruk gecikmesi veya yeniden deneme sonunda gönderilen müşteri bildirimi, fiilî gönderiminden on yıl dolmadan silinebilir. Bu, dokümanın on yıllık elektronik ticaret kaydı saklama güvencesiyle çelişir ve mevzuata aykırılık riski doğurur (1).
> Öneri: Z-34/Z-43 için saklama başlangıcını gerçek gönderim anına bağlayın: `SentAt` bu amaçla kullanılabiliyorsa onu, değilse ilk başarılı gönderimde doldurulan ayrı ve değişmez bir saklama başlangıç alanını kullanın. Hiç gönderilemeyen satırların saklama başlangıcı ayrıca tanımlansın; oluşturulma anı “gönderim anı” olarak adlandırılmasın.
>
> SONUÇ: 1 BULGU
> ```

**Parça 7 — başlık notu, §5–§9:**

> ```text
> BULGU-1
> Kriter: Eksiklik
> Seviye: Yüksek
> Yer: §9.3.1 — `ContactRequest`
> Alıntı: "`yeniden açılıp açık duran talep son kapatılıştan sayılır ve açıkken de silinir`"
> Sorun: KVKK talebi de bu kaydın konu tiplerinden biridir. Açık durumdaki talebin kapatılmadan silinmesi, başvurunun sonuçlandırılmadan kaybolmasına yol açar; ilgili kişinin başvuru ve yanıt hakkı ihlal edilir.
> Öneri: Açık talepleri imha kapsamı dışında tutun; özellikle KVKK talebinin yanıtı/sonuçlandırması kayda geçmeden silinmesini engelleyin.
>
> BULGU-2
> Kriter: Tutarlılık
> Seviye: Orta
> Yer: §7.2, §7.3.2, §9.3.1
> Alıntı: "`Refund | DefectClaimId | DefectClaim | Hayır | boşaltılır`"; "`Refund | Türe göre bağlar`"; "`DefectClaim ... tümüyle silinir ... geri ödeme bağları silmeyle boşalır`"
> Sorun: Ayıp talebinden doğan geri ödemenin türüne göre bağı zorunluyken, ayıp talebi imhası bu bağı boşaltır. Bu, ya imhanın kontrol kısıtına takılıp kişisel veriyi silememesine ya da kalıcı geri ödeme kaydının dayanağını kaybetmesine neden olur. Bu doküman içi çelişkidir.
> Öneri: Ayıp talebi imhasından sonra `DefectClaimRescission` geri ödemesi için bağın boş kalmasını açıkça izinli hâle getirin ve kalıcı finansal kayıtta gerekli kişisel-verisiz dayanak bilgisini saklayın; ilgili kontrol kısıtını buna göre güncelleyin.
>
> SONUÇ: 2 BULGU
> ```

## 3. Bağımsız değerlendirme

Her bulgu dokümanın tamamına (v0.19) ve ilgili yerde Ürün Gereksinimleri'ne (`02` v0.85), Kullanıcı Akışları'na (`03` v0.34), Teknik Mimari'ye (`05` v0.22), karar kaydına ve önceki turun raporuna karşı okundu — ikinci model bunları görmedi, parçalar dokümanın öteki bölümlerini de görmedi. Birleşim 2'nin BULGU-1'i ile parça 3'ün BULGU-2'si aynı kuralın (firmanın "gerçekleşmedi" işaretinden sonra gelen geç başarılı sonuç) iki yüzüdür — tek satırda değerlendirildi.

| # | Karar | Gerekçe | Uygulanan düzeltme |
|---|---|---|---|
| B1 · BULGU-1 | ❌ RET | **Bilinçli karar; çelişki yok.** Onay anahtarının imhada kalması dokümanın yazılı seçimidir (§4.17 `ConfirmationKey` — *"kişiye götürmez — imhada kalır"*; `05 §6.4.5`). Anahtar bir erişim yetkisi değil tekrar anahtarıdır (`05 §4.5.1`, §4.2.2 (0)): ödeme adımında yalnız o müşterinin formuna konur; aynı anahtarla ikinci gönderim yeni sipariş açmaz ve yönlendirir — yönlendirilen sayfanın kendi erişim kuralı vardır. İmhadan sonra sayfaya "hiçbir yoldan girilemez" (§4.17 `OrderAccessKey`) ve satırda kişisel veri kalmaz (K-582) — açılacak kişisel veri yoktur; imhadan önce veritabanını okuyan kişi erişim anahtarını zaten açık okur (kalan risk, K-1114; `05 §6.4.9`). Yıllar sonra eski formun yeniden gönderilmesi ölçünün dışında kalan nadir kenar durumudur. Önerilen kısa ömür ve boşaltma Teknik Mimari'nin mekanizmasını değiştirir. | — |
| B2 · BULGU-1 = P3 · BULGU-2 | ❌ RET | **Bilinçli ürün kararı ve adıyla yazılmış kalan risk.** Firmanın "gerçekleşmedi" işaretinden sonra gelen geç başarılı sonucun siparişe işlenmemesi ve panelde kalıcı uyarı olması Ürün Gereksinimleri'nin kuralıdır (`02 §7.2.8`; K-940, K-978): *"Firmanın işaretinden sonra sağlayıcıdan iadenin gerçekleştiği bilgisi gelirse siparişe işlenmez, panelde firmaya uyarı olarak görünür"*; ikinci ödemenin riski firmadadır (K-705) ve Teknik Mimari onu kalan risk diye yazar (`05 §4.6.2` — *"iki ödemeyi ürün kendisi çözmez"*). Veri Modeli bunu `LateSuccessAt` ve kalıcı uyarıyla taşır (§4.34). Parça 3'ün çelişki okuması iki ayrı işareti karıştırır: "işaretli kalır" geri ödeme kaydının ya da gelen bildirim kaydının geç sonuç işaretidir (`LateSuccessAt`; `05 §4.5.5`); "iade gerçekleşince kalkar" panelin "geri ödeme gerçekleşmedi" işaretidir ve iadenin sisteme işlenmesine — geri ödemenin gönderilmesine — bağlıdır (§4.34 İlişkiler). Geç sonuç işlenmediği için panel işareti kalır ve uyarı yanında görünür — iki cümle tutarlıdır. Önerilen veritabanı engeli K-940'ı tersine çevirir. | — |
| P3 · BULGU-1 | ⚠️ KISMİ | **Mevzuat iddiası tutmuyor; matrisin üç özeti kaynağın "süre boyunca" sözünü düşürmüştü.** Doküman işlem izini süre dolunca imha eder (§4.63 Silme davranışı; §9.3.3 *"İşlem izi süre boyunca değiştirilemez ve silinemez … süresi dolan satırı … imha fonksiyonu siler"*) — kişisel verinin süresiz saklanması yoktur, (1). sınıf düşer. Gerçek olan: matrisin `02 §4.2` Z-30, `02 §10.3.2` ve `03 §4.1.34` satırları kaynağı "değiştirilemez, silinemez" diye mutlak özetliyordu; kaynak "süre boyunca" der (Z-30; `02 §10.3.2`; `03` 4.1.34) — özet park satırının ve §9.3.3'ün imhasıyla çelişir gibi okunuyordu (ölçünün (4). sınıfı, düşük şiddet). Aynı kalıp `06`'nın tamamında tarandı (§4). | Matrisin üç hücresi: "süre boyunca değiştirilemez, silinemez; süre dolunca imha edilir" (kaynağın sözü). |
| P4 · BULGU-1 | ⚠️ KISMİ | **Hesap silinmiyor; §3.1'in hücresi iki tetikleyiciyi aynı özneye bağlıyordu.** §4.36'nın silme maddesi açıktır: e-posta değişikliğinin doğrulanmasında ya da geri alınmasında silinen, adresi o değişiklikte tutan **başka** bir doğrulanmamış bekleyen kayıttır (K-1080; `02 §3.13.14`); doğrulanmış hesap silinmez. Parça 4 §4.36'yı görmedi; ama §3.1'in hücresi *"bekleyen kaydın ömrü (Z-1) veya adresinin değişiklikte doğrulanması"* diyordu — "adresinin"in öznesi belirsizdi ve hücre müşteri hesabının kendi e-posta değişikliğiyle silindiği diye okunabiliyordu (ölçünün (4). sınıfı, düşük şiddet). Önerilen yeniden yazım gerekmez — kural §4.36'da doğru. | §3.1'in müşteri hesabı satırı: "doğrulanmamış bekleyen kayıt ayrıca ömrü dolunca (Z-1) ya da adresi başka bir hesabın e-posta değişikliğinde doğrulanınca veya geri alınınca (K-1080; §4.36)". |
| P5 · BULGU-1 | ✅ KABUL | **Gerçek kural boşluğu — K-1110'un ilkesi ret ve kapanış anlarına yazılmamıştı.** §4.31 IBAN'ı onu kullanan **son** bekleyen geri ödeme tamamlanana kadar tutar ve Z-38'in kendiliğinden silmesinde aynı istisnayı uygular (K-1110); ama §4.27 IBAN'ı retle, §4.28 kapanışla koşulsuz siler — Ürün Gereksinimleri de (`02 §7.4.9`, §10.4.9, Z-35). Yol olağandır: ret ve kapanış adetle yapılır (K-794) — aynı kalemin bir adedi teslim alınıp geri ödemesi beklerken öteki adedi reddedilir ya da "mal dönmedi" ile kapatılır; aynı beyandaki iki kalemden biri reddedilir. Bekleyen geri ödeme IBAN'sız ve açık isteksiz kalır. Firmanın IBAN isteği açması (`AdminRequest`; K-686) bir çıkıştır — akış çıkışsız değildir —, ama müşteriye ikinci giriş, firmaya elle adım yükler ve yasal on dört gün IBAN beklenirken durmaz: (2). sınıf, orta şiddet. Ürün kuralına dokunur → **K-1127 (öneriyle kaydedildi — ⚠)**; ikinci modelin ilk önerisi seçildi, ikinci kolu (silip yeniden istemek) elendi. | §4.27 ve §4.28'in girişleri, §4.31'in "Uygulamada" cümlesi, matrisin `02 §4.2` Z-35, §7.4.9, §10.4.9 hücreleri; geri besleme `02 §7.4.5`, §7.4.9, §10.4.9, Z-35 (v0.86); Kaynak satırları. |
| P6 · BULGU-1 | ❌ RET | **1. turda reddedilen bulgunun aynısı (P6 · BULGU-2; K-1066) — bu ikinci reddidir.** Saklamanın başlangıcını satırın oluşturulma anına bağlamak bilinçli karardır ve gerekçesi metindedir (§4.58): satır işlemi doğuran olayla aynı veritabanı işleminde yazılır, oluşturulma anı işlemin anıdır; 6563 m.11/3'ün on yılı elektronik ticaret işleminin kaydına bağlıdır, teslimine değil (K-951; 1. turun hukuki kontrolü). Bulgu yeni bilgi taşımıyor. Bir tur daha aynı yer ve aynı iddiayla dönerse "tekrar eden RET" olur (skill Faz 4 madde 5). | — |
| P7 · BULGU-1 | ❌ RET | **Bilinçli karar ve adıyla yazılmış kalan sonuç; hukuki iddia tutmuyor.** Açık talebin de süre sonunda silinmesi Ürün Gereksinimleri'nin kuralıdır (Z-33 — *"kapatılmamış açık talep de"*; `02 §3.32.4`; K-507 — firmanın kapatmayı unutması veriyi süresiz bırakmaz); yeniden açılıp açık duran talebin son kapatılıştan P-40 sonra silinmesi audit'te güvence merceğiyle kararlaştırıldı ve kalan sonuç olarak yazıldı (K-1099; §4.57, §9.3.1); 1. turda aynı mekanizmanın önerisi reddedilmişti (B1 · BULGU-1). KVKK başvurusu veri sorumlusunca en geç otuz günde sonuçlandırılır (6698 m.13/2); KVKK talebinin saklaması P-40'ın genel süresidir — iki yıl. Silme, başvuru otuz gün içinde yanıtlanmamışken iki yıl sonra gerçekleşir; yükümlülüğü ortadan kaldırmaz. Önerilen "açık talepleri imhanın dışında tutmak" Z-33'ü kırar. | — |
| P7 · BULGU-2 | ⚠️ KISMİ | **Kısıt boşluğa izin verir; §7.3.2'nin özeti istisnayı söylemiyordu.** §4.33'ün türe göre bağlar tablosu `DefectClaimRescission`'da `DefectClaimId`'yi "talep silinene kadar" diye yazar ve altındaki cümle açıktır: *"kısıt `DefectClaimId`'nin (talep silinince boşalır) ve `ReturnId`'nin boşluğuna izin verir"*; geri ödeme satırı tür, kalem, adet ve tutarla ticari kayıt olarak kalır (§4.33 Silme davranışı) — imha takılmaz, dayanak kaybolmaz. Parça 7 §4.33'ü görmedi; ama §7.3.2'nin satırı yalnız "Türe göre bağlar (§4.33'ün tablosu…)" diyor ve istisnayı söylemiyordu — özet ile ayrıntının farkı (ölçünün (4). sınıfı, düşük şiddet). | §7.3.2'nin `Refund` satırı: "kısıt `DefectClaimId`'nin — talep silinince boşalır — ve `ReturnId`'nin boşluğuna izin verir". |

**Dağılım:** sekiz ayrı bulgu (dokuz ham bulgu; bir çift aynı) — 1 KABUL · 3 KISMİ · 4 RET. **%100 kabul değil, %100 ret değil:** dört RET'in üçü bilinçli kararla (onay anahtarının imhada kalması — `05 §4.5.1`, K-1114; geç başarılı sonuç — K-940, K-705; açık talebin silinmesi — Z-33, K-1099), biri 1. turun reddinin tekrarıyla (K-1066) düştü; üç KISMİ'nin üçünde ikinci modelin önerisi reddedildi (mutlak silinmezlik kaydı, hesabın silme tetiklerinin yeniden yazımı, kısıtın değiştirilmesi) ve gerçek metin farkı en küçük düzeltmeyle kapandı. **Sınıf:** üç KISMİ düşük şiddette iki yerin metin farkıdır — ikisi özet ile ayrıntının farkı (matrisin üç Z-30 özeti, §7.3.2), biri §3.1'in belirsiz hücresi; KABUL orta şiddette bir para ve hak sınıfı bulgusudur ve ürün kuralına dokunur (K-1127 ⚠, Ürün Gereksinimleri v0.86). Park satırına dokunan düzeltme yok; bağ, dizin ve silme kategorisi değişmedi.

## 4. Ek bulgular

- **Aynı kalıbın taraması — "değiştirilemez, silinemez" (P3 · BULGU-1).** `06`'daki on üç eşleşme okundu: işlem izinin üç matris özeti düzeltildi; KP-63 ve `02 §1.2` `AuditLog` "on yıl değiştirilemez" der, `02 §8.5.2`'nin satırı süre dolunca imhayı yazar, §9.3.3 "süre boyunca" der — doğru; öteki eşleşmeler başka kurallardır (dolu kategori, firmanın kaydı, yayındaki son varyant, davetin adresi, bağlı görsel).
- **Aynı kalıbın taraması — paylaşılan IBAN'ın silinme anları (P5 · BULGU-1).** IBAN'ın silindiği her an §4.31'in listesinde okundu: geri ödemenin tamamlanması ve Z-38 K-1110 ile, ret ve kapanış K-1127 ile korunur; "havale gerçekleşmedi" bildirimi IBAN'ın kendisi geçersiz olduğu için siler ve yeni istek düşer (K-575) — aynı geçersiz IBAN'ın başka bir geri ödemede kullanılması beklenmez; düzeltilen firma iptalinde IBAN o iptalin isteği için girilmiştir (K-1056). Ürün Gereksinimleri ve öteki dokümanlardaki yüzlerin taraması `06 §1.3`'ün geri besleme taramasındadır.
- **Aynı kalıbın taraması — özet satırının istisnası (P7 · BULGU-2).** §7.3.2'nin satırları §4'ün "izin verir" cümlelerine karşı okundu: `Refund`'unki tek eksikti.
- **Mekanik tarama.** Gövdeye eklenen K numaraları (K-1127; §3.1'de K-1080) bulundukları bölümün Kaynak satırında (§4.27, §4.28, §4.31; §3; `02 §4`, §7, §10); matris satırı eklenmedi ya da çıkmadı — kümeler değişmez; bağ ve dizin değişmedi — §6/§7 karşılaştırması gerekmedi. Başlık sürümü, yazım durumu notu ve dosya sonu dipnotu v0.20'yi (Ürün Gereksinimleri v0.86) gösterir. Değişen dosyalarda CR ve kontrol karakteri sıfır, eklenen satırlarda `$` artığı yok; karar kaydında çift numara yok.
- **Deep review'ın uygulanmayan bulguları (karar kaydı §14.1):** bu turun hiçbir bulgusu onların yolunu (L6a-1, L6b-4, L7-1 ve kısmen uygulananların kolları) yeniden açmadı; madde cross-review TEMİZ döndüğünde kapanır.
- **Ciddi bir sorun görülmedi** — ikinci modelin listesi dışında ölçünün dört sınıfından birine giren bir sorun saptanmadı.
- **Önceki turlardan açık kalanlar:** yok. 1. turun reddettiği üç bulgudan biri döndü (K-1066 — P6 · BULGU-1, ikinci ret); S2 satırı ve silinen varyantın ayırması (K-655) dönmedi. 1. turun B1 · BULGU-1'inin mekanizma kolu bu turda KVKK gerekçesiyle döndü (P7 · BULGU-1) — kayıt K-1099'dur.

## 5. Kullanıcı onay checklist'i

K-992 kararıyla (Veri modeli aşaması boyunca ⚠ öneriyle kayıt, CI yeşilse `docs:` PR merge yetkisi ve yönetici + alt ajan düzeni) alt ajan değerlendirdi ve uyguladı (`06` v0.20). Bir karar öneriyle kaydedildi: **K-1127 (⚠)** — iade reddinde ve "mal dönmedi" kapanışında müşterinin IBAN'ı, onu kullanan başka bir bekleyen geri ödeme varken silinmez; son geri ödeme tamamlanınca silinir. Karar kaydı §14.1'in ⚠ listesine eklendi. Geri besleme: Ürün Gereksinimleri v0.86 (✓ korunur).

- [x] B1 · BULGU-1 (ret — onay anahtarı tekrar anahtarıdır; imhada kalması bilinçli)
- [x] B2 · BULGU-1 = P3 · BULGU-2 (ret — K-940, K-705; geç sonuç işlenmez, kalan risk yazılı)
- [x] P3 · BULGU-1 (kısmi — matrisin üç işlem izi özeti "süre boyunca")
- [x] P4 · BULGU-1 (kısmi — §3.1'in müşteri hesabı hücresi)
- [x] P5 · BULGU-1 (kabul — K-1127 ⚠; Ürün Gereksinimleri v0.86)
- [x] P6 · BULGU-1 (ret — K-1066; ikinci ret)
- [x] P7 · BULGU-1 (ret — Z-33, K-1099; KVKK m.13/2)
- [x] P7 · BULGU-2 (kısmi — §7.3.2'nin geri ödeme satırı)
- [ ] Etki yansıtma (Faz 5) — döngü TEMİZ döndükten sonra, son turun raporunda

**Hukuki kontrol** (2026-10-18). Yasal dayanak anan iki bulgu var; ikisini getiren çağrılar web araması yapmadı. **P6 · BULGU-1 (6563 sayılı Kanun m.11/3):** 1. turun kontrolü geçerlidir — metin Teknik Mimari'nin kalite döngüsünde resmî kaynaktan okunmuştu (K-951) ve iddia yeni bilgi taşımıyor. **P7 · BULGU-1 (KVKK başvuru hakkı):** 6698 sayılı Kanun m.13/2 düzenleyici kurumun sitesinden okundu (kvkk.gov.tr, Kanun'un resmî İngilizce çevirisi — Türkçe metin esastır; mevzuat.gov.tr'nin PDF'i araçla çözülemedi): veri sorumlusu başvuruyu talebin niteliğine göre en kısa sürede ve en geç otuz gün içinde ücretsiz sonuçlandırır. Kanun başvurunun kaydını süresiz saklamayı istemez; iki yıllık saklama otuz günlük sürenin çok ötesindedir — mevzuata aykırılık yoktur.

## 6. Sonuç ve sonraki tur

**Sonuç: TEMİZ değil** — kabul edilen dört bulgunun biri orta şiddette (K-1127). Çıkış kuralı (skill Faz 4 madde 5) beşinci turdan önce uygulanmaz.

**Yakınsama ölçüsü:** ayrı bulgu 7 → 8; kabul ya da kısmi 4 → 4; turun yeni ürün kararı 0 → 1 (süreç kararları K-1125, K-1126 hariç); tekrar eden RET 0 → 0; TEMİZ çağrı 3 → 2 (1. turda parça 2, 5, 7; bu turda parça 1, 2).

**3. tur için not:** aynı istem, parça notu ve betiklerle (`06_CROSS_REVIEW.md` §1; K-1126). Birleşim 1'in payı 73 195 karakterdir ve bu turda 1 202 karakter daraldı; pay 10 000 karakterin altına inerse birleşim en küçük değişiklikle yeniden kurulur ve kaydedilir. P6 · BULGU-1 (saklamanın başlangıcı — K-1066) aynı yer ve aynı iddiayla dönerse "tekrar eden RET"tir. P7 · BULGU-1'in yolu (açık talebin silinmesi — Z-33, K-1099) iki turda iki farklı gerekçeyle geldi; dönerse ret gerekçesi bu raporun §3'ündedir. Bu turun iki kısmi bulgusu (P4, P7 · BULGU-2) parçanın göremediği ayrıntının özet hücrede eksik yazılmasından doğdu — özet hücrelerinin istisnayı taşıması aynı sınıfı sonraki turlarda azaltır.
