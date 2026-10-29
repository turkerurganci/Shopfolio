# Deep Review Raporu — 08 Integration Spec

**Tarih:** 2026-10-29 · **Hedef:** `Docs/08_INTEGRATION_SPEC.md` v0.8 → v0.9 (bulgular v0.8'de üretildi, v0.9'da işlendi; taban `main` = 2e945b1) · **Bağlam:** `02`, `03`, `04`, `05`, `06`, `07` (bulgular bunlar üzerinde üretildi); `PRODUCT_DISCOVERY_STATUS.md` §2 (K-1233…K-1268), §18.2 ve §18.3; [`08_AUDIT.md`](08_AUDIT.md) · **Odak:** Tam analiz — sekiz katman on mercekte; katman 1–2 tek hafif mercekte (audit'in kararlarının doğurduğu yeni tutarsızlıklar; `deep-review` skill'i, kural 7)

### Koşum biçimi (K-431, K-1232; `deep-review` ve `audit` skill'leri, "Koşum biçimi")

Dokümanı yazan bağlamda koşmadı. Adım ajanı mercekleri, doğrulayıcıları ve uygulayıcıyı **ön planda** başlattı (`run_in_background: false`). Her mercek bulgusunu bulduğu anda kendi dosyasına yazdı; her mercek ve doğrulayıcı önce kendi dilim dosyasını (satır numaralı `08` kopyası) okudu, karar kaydı hiçbir ajana bütünüyle okutulmadı.

- **On mercek, iki beşli dalgada, hepsi `[ozet]` Sonnet / low.** Dalga 1: **L12** (katman 1–2, hafif) · **L3a** (§2.1) · **L3b** (§2.2) · **L3c** (§2.3–§5) · **L4a** (güvenlik, ödeme + §3.2). Dalga 2: **L4b** (güvenlik, e-posta + Google + §3.1) · **L5** (hata modu) · **L6** (veri akışı — kişisel verinin dış servise giden her alanı) · **L7** (ölçeklenebilirlik — kota, hız sınırı) · **L8** (bağımlılık riski — sağlayıcı değişimi, yedek Sipay). Katman 4 OWASP Top 10:2025'e göre koştu (bildirim imzası, dönüş adresi, çerçeve/CSP, anahtar saklama, e-posta enjeksiyonu özellikle okundu).
- **Audit'le çakışma yasağı.** Her mercek `08_AUDIT.md`'yi okudu ve audit'in uyguladığı bulguyu yeniden açmadı; audit'in dört yan gözlemi (Y-2…Y-5) merceklere bulgu olarak verilmedi, doğrudan doğrulayıcıya gitti.
- **Doğrulama.** Hüküm birimi başına bir `[dogrulama]` Opus / high, ön planda: **71 doğrulayıcı** (dört dalga: 20 + 19 + 19 + 13; her mesajda en çok yirmi çağrı — iki dalgada yirminci çağrıyı araç eşzamanlı alt ajan sınırı diye reddetti, o doğrulayıcılar bir sonraki dalgada yeniden başlatıldı, iş tekrarlanmadı). Aynı olguyu birden çok mercek bulduğunda tek doğrulayıcıya verildi: **GA** (L12-1 + L12-3 + L3c-9), **GC** (L3a-5 + L6-6), **GD** (L3a-8 + L6-5), **GE** (L3a-6 + L3a-7 + L6-2), **GF** (L3a-9 + L5-11), **GG** (L5-7 + L4b-8), **GH** (L5-12 + L8-3), **GI** (L5-5 + L7-1), **GJ** (L5-8 + L3c-13), **GK** (L7-2 + L3b-6), **GL** (L4b-9 + L3c-12); **Y-3** doğrulayıcısı L4a-5 ve L4a-7'yi de aldı. Doğrulayıcı düzeltmeyi bulguyla birlikte 'eski metin → yeni metin' çiftleri olarak bıraktı ve dış olguyu (PayTR, AWS SES, Google, OIDC, CSP3) resmî sayfadan okuyup tarih ve kaynakla yazdı.
- **Uygulama.** 232 çift, **betikle** (`parse_pairs.py`: her çiftin eski metni dosyada tam bir kez geçmeli): 210'u doğrudan uygulandı; 17'si başka bir çiftle ya da DV numaralamasıyla çakıştığı için bir `[ozet]` Sonnet / low uygulayıcıya (1 ajan) ve yürütücüye kaldı; kalanı "karşılandı" ya da başka bir doğrulayıcının aynı yerdeki çiftidir. DV numaraları yürütücü elindeydi: dört doğrulayıcı aynı anda "DV-39" önerdi ve iki doğrulayıcı aynı olguya ayrı DV yazdı (L3a-4 ve L5-6) — DV-39 (IPv6) · DV-40 (anahtar yenileme) · DV-41 (ayrı test mağazası) · DV-42 (`failed` bildirimi deneme başına gelir mi); kopya satır silindi. Betiğin ilk sürümü doğrulayıcıların "doğrulandı" notlarını da yeni metne taşıdı; fark taramasıyla (eklenen satırlarda `ESKİ`, `grep`, `Yürütücü` sözcükleri) bulundu, dosyalar `git checkout` ile sıfırlanıp betik düzeltilerek yeniden uygulandı.
- **Mekanik denetimler (betikle).** `08`'in tablolarında eklenen satırların sütun sayısı (`tablecheck.py`: dört dosyada yeni uyumsuz satır yok); CR, kontrol karakteri ve `\$[0-9]` taramaları; K numarasının tekilliği; DV tablosu: 42 satır, DV-1…DV-42 kesintisiz, sayım cümlesi "42 satır" ve "yedi ek" (`DV-35, DV-36, DV-38…DV-42`).
- **Skor (skill).** Katman skoru ✓ / ⚠ / ✗; katman birden çok mercekten birleşiyorsa herhangi biri ⚠ ise katman ⚠'dir. Skor **mercek skorudur**; düzeltmeler bu adımda uygulandığı için son sütun uygulama sonrası durumu verir.

**Karşı-doğrulama sonucu:** 82 ham bulgu + audit'in 4 yan gözlemi = **71 hüküm birimi**: **25 doğrulandı · 46 kısmen doğrulandı · 0 tümden çürütüldü.** Kısmen doğrulananların çoğunda bulgunun özü doğru, önerilen çare ya da bir alt iddia çürüdü (aşağıda). Doğrulanmış seviye, hüküm birimi başına: **High 15 · Medium 29 · Low 27**; Critical yok. **Hepsi uygulandı** (71 birim); uygulanmayan yok. **Çok kritik soru çıkmadı.**

### Özet Skor Tablosu

| # | Katman | Ham | Skor | Kritik bulgu | Uygulama sonrası | Güçlü yönler (özet) |
|---|---|---|---|---|---|---|
| 1 | Kapsam | 1 | ✓ | Yok. L12-1 (Low): Y-1 satırı hâlâ "karar kaydı bekliyor" diyordu | ✓ | Audit'in on kararı gövdede yerinde; K-1261 `user_phone`, ilk e-postanın satır kimliği, on alanlı bildirim `07` ile uyumlu |
| 2 | Tutarlılık | 2 | ⚠ | Yok (Low). L12-2: altbilgide audit sayısı (49 hüküm grubu ≠ 52 bulgu); K-1259'un üst-doküman yansıması yarım | ✓ | K-1260…K-1268 cümleleri birbiriyle ve `05`, `06`, `07` ile çelişmiyor; DV sayımı tablo ile tutuyor |
| 3 | Teknik derinlik | 35 | ⚠ | L3a-2 (High): bildirimin başarı koşulları dört yere dağınık; L3b-1, L3b-2, L3b-3 (High): yeniden deneme anlarının tabanı, gövdesi üretilemeyen denemenin sınıfı, `{beyan}` değerleri; L3c-1 (High): Google'da doğan hesapta bağ yazımı | ✓ | İmza girdileri ve hata kodu tabloları uygulanabilir; gönderim çağrısı alan alan eşlenmiş; OIDC parametreleri ve çerez nitelikleri eksiksiz; bilinmeyenler uydurulmadan DV'ye bağlı |
| 4 | Güvenlik (OWASP) | 18 | ⚠ | L4a-1 (High): canlı anahtarlar geliştirme ortamına dağılıyor; L4a-2 (High, kısmen): sızıntıda eski anahtarın en son kapatılması; L4b-6: SES hesabının itibar koruması yok | ✓ | Bildirim imzası, sabit zamanlı karşılaştırma, tekrar saldırısı sonuçsuz; dönüş ucu durum yazmıyor, anahtar biçimi denetleniyor; Google akışı `state`+`nonce`+PKCE, `__Host-` çerezi, `email_verified` zorunlu; bağlantılarda ham anahtar yok |
| 5 | Hata modu | 12 | ⚠ | L5-1 (High): PayTR kesintisinde "kart seçilemez" mekanizması yok; L5-2 (High): çift tahsilat sessiz; L5-3: çift iade çakışması görünmüyor | ✓ | "Kayıt önce, istek sonra, belirsizde yeniden gönderme yok" ilkesi tutarlı; belirsiz son sorgu siparişi iptal etmiyor; geç ödeme otomatik iadeye bağlanıyor |
| 6 | Veri akışı | 7 | ⚠ | L6-3 (High): taksit farkının iadesi ürünün dışında kalıyor (tüketici hukuku yüzü); L6-1 ve L6-2 (mercek: High, hüküm: Medium): `user_phone` biçimi `06` saklamasıyla çelişiyor, PayTR alan sınırları ürün girişinde yok | ✓ | Sağlayıcıya giden alan kümesi `02 §12.2.8` ile örtüşüyor; tutar zinciri tek yönlü; gelen kayıt kişisel alan taşımıyor |
| 7 | Ölçeklenebilirlik | 3 | ⚠ | Yok (Medium). L7-1: son sorgu düz beş dakika, kalıcı hatada sınırsız | ✓ | 350 e-posta/gün tepe hesabı K-1236'dan türetilmiş; sorgu yükleri sayılı; sayaç kayan pencere, tablolar süreyle siliniyor |
| 8 | Bağımlılık | 4 | ⚠ | L8-1 (High): sağlayıcıyı bırakma / anahtar kaldırma eski kart siparişlerinin iadesini karşılamıyor | ✓ | Envanter tam; §3.3 "bir dış servis çökerse" tablosu her servis için etki, süre, uyarı ve alternatifi veriyor; dış olgular tarihli |

Merceklerin Özet bölümüyle karşılaştırma: L12 (katman 1 ✓, katman 2 ⚠), L3a ⚠, L3b ⚠, L3c ⚠, L4a ⚠, L4b ⚠, L5 ⚠, L6 ⚠, L7 ⚠, L8 ⚠. Katman 3 üç mercekten, katman 4 iki mercekten birleşir; ayrışan skor yok.

**Genel:** ⚠ İyileştirme gerekli — düzeltmeler bu adımda uygulandı; Critical yok.

### Sayılar

- **Ham:** 82 bulgu (mercek önerisi: High 15 · Medium 36 · Low 31) + audit'in 4 yan gözlemi. Dağılım: L12 3 · L3a 9 · L3b 13 · L3c 13 · L4a 8 · L4b 10 · L5 12 · L6 7 · L7 3 · L8 4. Betikle denetim: mercek dosyalarındaki bulgu başlıkları toplamı 82; hüküm dosyaları 71 (11 grup, 1 Y-3 grubu, 3 tekil Y, 56 tekil bulgu); gruplanan bulgu 26 (24 + L4a-5, L4a-7) + 56 = 82.
- **Hüküm:** 71 birim — **25 doğrulandı · 46 kısmen · 0 çürütüldü**; seviye **High 15 · Medium 29 · Low 27**. Seviyesi değişenler (mercek → doğrulayıcı): L3a-2 Medium → High · L5-4 Medium → High · L3a-1 High → Medium (biçim belge örnek kodundan okundu) · L6-1 High → Medium · L6-2 High → Medium · L4b-6 High → Medium · L5-3 High → Low (çakışma K-978 ve K-1238 (6)'nın kabul ettiği kalan risk) · L3a-3 Medium → Low · L3c-9 Medium → Low.
- **Alt iddiası çürütülen bulgular** (öneri ya da gerekçenin bir kısmı): L3a-4 ("yalnız `6` ve `99` terminal" — ürün kuralıdır), L4a-2 ("eski anahtarla sahte bildirim" — bildirimi ürün denetler), L3b-10 ("kural ölü" — kural `02 §3.29.3`'te canlı), L3b-9 ("iptal kaydı kalem başına mı belli değil" — `06 §4.22` kalem başına), L3c-5 ("`claims` reddedilir" — Google kabul eder), L3b-5 (imza/HTTP ayrıntıları), L5-1 (B-1'i ödeme sonrasına almak), L5-4 (iptal sonrası ek sorgu), L4a-3 (zorunlu teyit sorgusu), L4b-6 (itibar yoklaması), L4b-7 (iki önerilen çare), L4b-10 ("yeniden doğrulama riski azaltır"), L6-3 ("iade tutarı tanımsız" — `Refund.Amount` tanımlı), L6-5 (kaynak olarak `Order.IpAddress`), L6-7 (PayTR yüzü), L7-1 (geri çekilme), L7-3 ("hatırlatma yok"), L4b-3 ve L4b-4 (karakter kesmesi, yıllık döndürme), L3c-4 (ömrün dakika olarak sabitlenmesi).
- **Kararlar:** K-1269…K-1274 (6 karar, hepsi ⚠ öneriyle kaydedildi).
- **Audit yan gözlemleri (Y-2…Y-5):** dördü de doğrulandı ve uygulandı — aşağıda.

### Audit'in yan gözlemleri — hükümler

| Gözlem | Seviye | Hüküm | Uygulama |
|---|---|---|---|
| **Y-2** `failed_reason_code` değeri `3` | Low | Doğrulandı. PayTR belgesi (dev.paytr.com iFrame API 2. adım, okuma 2026-10-29): `3` "Güvenlik kontrolü sonrası onay verilmedi veya kontrol yapılamadı" — PayTR'nin kendi güvenlik kontrolüdür, 3D doğrulamayla ilgisi yoktur; `08` §2.1.3 madde 2 `1`, `2` ya da `3`'ü 3D başarısızlığı sayıyordu, §2.1.3 giriş paragrafı ve DV-3 (`1`, `2`, `10`) doğruydu. Davranış etkisi yok (`failed_reason_code` saklanmaz). | §2.1.3 madde 2 ve §2.1 kod listesi düzeltildi (2 çift); K-1269 (Y-2) |
| **Y-3** `frame-src`'nin `'self'` taşımaması | High | Doğrulandı. Audit'in "K-1259 zaten karşılıyor" yorumu yanlıştı: `frame-ancestors 'self'` geçiş sayfasının yanıtında çerçevelenmeye izin verir, `frame-src` ise çerçeveleyen sayfanın (ödeme sayfası) politikasıdır ve çerçeveye yüklenen her adresi — çerçevenin kendi gezinmesini ve yönlendirmesini de — sınar (CSP3 §6.1.5, okuma 2026-10-29). `'self'` olmadan çerçevede yüklenen geçiş sayfası engellenir. 3D Secure'da banka sayfasının hangi çerçeveye yüklendiği PayTR belgesinde yazılı değil (doğrulanamadı). L4a-5 doğrulandı (öneri kısmen), L4a-7 doğrulandı (öneri kısmen: `allow-top-navigation-by-user-activation` betikli geçişi bozar). | §2.1.4 madde 2 ve 3, DV-5, §5.2 satır 14 (6 çift); `05 §6.5.4` etki yansıtmada; K-1274 |
| **Y-4** `06 §4.58` grup tanımı ↔ "grubun en son satırı" | High | Doğrulandı. Eski tanım "aynı tür, sipariş ve ilgili kayıt"tı; K-1262'den sonra aynı tür altında birden çok ayrı e-posta var (ikinci havale hatırlatması, ikinci geri ödeme…) — birinci geri ödemenin B-8'i ulaşmadı, ikincisi ulaştı ise grubun en son satırı "gönderildi" olur ve firma birincinin ulaşmadığını görmezdi (`02 §9.1.6`, K-761, `04` 2.5.3.1 ile çelişir). K-1262'nin ilk satır kimliği `EventValues`'taydı ve sorguya girmez; siparişler arası işaret süzgeci sorgu ister. | `06 §4.58` yeni değer sütunu `OriginalDispatchId`, grup tanımı, `EventValues`, `NextAttemptAt`, §7.2.3; `05 §4.4.3`; `07` yan etki; `08` §2.2.4, §2.2.5 (11 çift); K-1273 |
| **Y-5** iç alan `Amount` ↔ `06 §4.17` `Total` | Low | Doğrulandı. Karışma yalnız §2.1.6 son sorgu alan eşlemesinde (`payment_total` → `Amount`): `Order`'da `Amount` alanı yok, son sorgu gelen bildirim kaydı yazmaz; konvansiyon 2 (adlar `06`'dan birebir) ihlali. §2.1.5, §2.1.7 ve iade eşlemesi doğru. | Hücre `—` (saklanmaz; `06 §4.32`), karşılaştırma hedefi `Order.Total` (1 çift); K-1269 |

### Bulgu → hüküm → uygulama

Seviye doğrulayıcının hükmüdür; "Çift" doğrulayıcının bıraktığı 'eski → yeni' çiftlerinin sayısıdır (232). "Karar" sütunu bulgunun karar kaydındaki yeridir; "—" yazım ya da devir düzeltmesi, yeni karar gerekmemiştir. Tümü uygulandı.

| Bulgu | Seviye | Hüküm | Konu | Çift | Karar |
|---|---|---|---|---|---|
| L3a-2 | High | Kısmen | bildirimin sınıflama sırası; `failed`'de tutar | 5 | K-1269 |
| L3b-1 | High | Doğrulandı | P-38 aralıklarının hesap tabanı | 3 | K-1271 |
| L3b-2 | High | Doğrulandı | gövdesi üretilemeyen denemenin sonuç sınıfı | 1 | K-1271 |
| L3b-3 | High | Doğrulandı | `{beyan}` değişkeninin değerleri | 2 | — |
| L3c-1 | High | Doğrulandı | Google'da doğan hesapta `GoogleLinkedAt` yazımı | 4 | K-1272 |
| L4a-1 | High | Doğrulandı | canlı anahtarların geliştirme ortamına dağılımı | 7 | K-1270 |
| L4a-2 | High | Kısmen | anahtar sızıntısında kapatma sırası | 4 | K-1270 |
| L5-1 | High | Kısmen | "kart seçilemez" mekanizması | 4 | K-1270 |
| L5-2 | High | Kısmen | çift tahsilat sessiz | 2 | K-1270 |
| L5-4 | High | Kısmen | `timeout_limit`; süre dolumundan sonra ödeme | 3 | K-1269 |
| L5-7 + L4b-8 | High | Doğrulandı | SES askıdaki adres: sessiz "Gönderildi", güvenlik e-postaları | 13 | K-1271 |
| L6-3 | High | Kısmen | taksit farkı ve iade tutarı | 6 | K-1270 |
| L8-1 | High | Doğrulandı | anahtarlar kaldırılınca eski siparişlerin iadesi | 4 | K-1270 |
| Y-3 + L4a-5 + L4a-7 | High | Doğrulandı | `frame-src`'nin `'self'` taşımaması; sandbox kalan riski | 6 | K-1274 |
| Y-4 | High | Doğrulandı | `06 §4.58` grup tanımı ↔ "grubun en son satırı" | 11 | K-1273 |
| L3a-1 | Medium | Doğrulandı | `user_basket` biçimi | 4 | — |
| L3a-4 | Medium | Kısmen | `failed` bildirimi deneme başına gelir mi | 4 | K-1269 |
| L3a-5 + L6-6 | Medium | Doğrulandı | tutar biçimleri ve ayrıştırma; `return_amount` | 8 | K-1269 |
| L3a-6 + L3a-7 + L6-2 | Medium | Doğrulandı | PayTR alan sınırları (ad, adres, e-posta) | 5 | K-1269 |
| L3a-8 + L6-5 | Medium | Kısmen | `user_ip`: IPv4 / 39 karakter, IPv6, kaynak | 3 | K-1269 |
| L3b-4 | Medium | Doğrulandı | `MessageRejected` eşlemesi | 5 | — |
| L3b-7 | Medium | Kısmen | logo adresi kalıbı | 1 | K-1271 |
| L3b-8 | Medium | Kısmen | ilk satır kimliğinin yeri | 3 | K-1273 |
| L3b-9 | Medium | Kısmen | B-7 tam iptalde kalem listesi | 2 | — |
| L3c-2 | Medium | Kısmen | Google için bağlantı katmanı arayüzü | 2 | — |
| L3c-3 | Medium | Kısmen | Google uç adresleri: sabit mi, keşif mi | 4 | K-1272 |
| L3c-4 | Medium | Doğrulandı | `state`'in iki rolü | 2 | K-1272 |
| L3c-5 | Medium | Kısmen | `auth_time` ayarı (Session age claims) | 5 | K-1272 |
| L3c-6 | Medium | Doğrulandı | Google kurulumu `DEPLOY_RUNBOOK`'a devri | 2 | — |
| L3c-7 | Medium | Kısmen | Google ve e-posta testleri `12`'ye devri | 2 | — |
| L3c-8 | Medium | Kısmen | Google kalıbı `09`'a devri | 4 | — |
| L4a-3 | Medium | Kısmen | bildirim imzasının tek teyit kanalı olması | 2 | K-1270 |
| L4b-1 | Medium | Doğrulandı | kaçış kuralının kapsamı (yönetici ve firma girdileri) | 2 | K-1271 |
| L4b-2 | Medium | Doğrulandı | B-1 yasal metinlerinin biçimi | 2 | — |
| L4b-3 | Medium | Kısmen | gönderen adında başlık güvenliği | 3 | K-1271 |
| L4b-4 | Medium | Kısmen | IAM: kaynak ve koşul sınırı | 4 | K-1271 |
| L4b-5 | Medium | Kısmen | DMARC alt sınırı | 2 | K-1271 |
| L4b-6 | Medium | Kısmen | SES hesabının itibarı | 3 | K-1271 |
| L5-12 + L8-3 | Medium | Doğrulandı | PayTR kalıcı bozukluğunda firmaya uyarı | 7 | K-1270 |
| L5-5 + L7-1 | Medium | Kısmen | son sorgu: kalıcı hata kodları ve sınırsız döngü | 2 | K-1270 |
| L5-6 | Medium | Kısmen | "başarısız bildirim sonludur" varsayımı | 3 | K-1269 |
| L6-1 | Medium | Kısmen | `user_phone` biçimi | 3 | K-1269 |
| L6-4 | Medium | Doğrulandı | `OrderReference` dönüşümü | 1 | — |
| L7-2 + L3b-6 | Medium | Kısmen | SES saniyelik gönderim hızı ve ileti boyu | 7 | K-1271 |
| L12-1 + L12-3 + L3c-9 | Low | Kısmen | K-1259 yansıması: "karar kaydı bekliyor" ifadeleri ve etki yansıtmaya not | 14 | — |
| L12-2 | Low | Doğrulandı | altbilgideki audit sayısı (49 hüküm grubu ≠ 52 bulgu) | 1 | — |
| L3a-3 | Low | Kısmen | aynı `merchant_oid` ile yeniden başlatma geri düşüşü | 2 | — |
| L3a-9 + L5-11 | Low | Kısmen | iade sorgusu basamakları ve reddin nedeni | 4 | — |
| L3b-10 | Low | Kısmen | marka adı boşsa alan adı kuralı | 2 | — |
| L3b-11 | Low | Doğrulandı | "kuruşlu biçim" ifadesi | 2 | — |
| L3b-12 | Low | Kısmen | altbilgide firma unvanı alanı | 1 | — |
| L3b-13 | Low | Kısmen | geri dönme eşleşmesi ulaşmış satırı da işaretler | 1 | K-1271 |
| L3b-5 | Low | Kısmen | SDK'nın kendi yeniden denemesi | 2 | K-1271 |
| L3c-10 | Low | Doğrulandı | `12` park bloğunda DV sayısı | 1 | — |
| L3c-11 | Low | Kısmen | Google DV satırlarında "yanlış çıkarsa" | 2 | — |
| L4a-4 | Low | Kısmen | iade tavanı ve iadeyi tetikleme yetkisi | 1 | — |
| L4a-6 | Low | Kısmen | imzasız bildirim alanlarının güvence sayılması | 1 | — |
| L4a-8 | Low | Kısmen | geçiş sayfası hedef adresinin korumaları | 2 | K-1274 |
| L4b-10 | Low | Doğrulandı | Google `sub` saklanmaması kalan riski | 1 | K-1272 |
| L4b-7 | Low | Kısmen | alıcı sayacının kötüye kullanımı | 2 | K-1271 |
| L4b-9 + L3c-12 | Low | Kısmen | Google kimlik belirteci doğrulama listesi | 6 | K-1272 |
| L5-10 | Low | Kısmen | geri dönme sorgusu sessiz hatası | 1 | — |
| L5-3 | Low | Kısmen | çift iade çakışması | 1 | K-1270 |
| L5-8 + L3c-13 | Low | Kısmen | Google dönüşünde toplam bekleme | 3 | — |
| L5-9 | Low | Kısmen | SES yeniden deneme ufku | 2 | — |
| L6-7 | Low | Kısmen | SES askı listesi ve imha politikası | 2 | K-1271 |
| L7-3 | Low | Kısmen | iade sorgusunun süresizliği | 1 | — |
| L8-2 | Low | Kısmen | Sipay geçişinin etki alanı | 1 | — |
| L8-4 | Low | Kısmen | değişim riskini izleme sahibi | 1 | — |
| Y-2 | Low | Doğrulandı | `failed_reason_code` değeri `3` | 2 | K-1269 |
| Y-5 | Low | Doğrulandı | iç alan `Amount` ↔ `06 §4.17` `Total` | 1 | K-1269 |

### Notlar

- **Ölçüm (model dağılımı, K-1232).** Mercekler 10 · Sonnet / low · ~1,5 M belirteç; doğrulayıcılar 71 · Opus / high · ~6,3 M belirteç; uygulayıcı 1 · Sonnet / low · ~0,1 M belirteç. Toplam 82 ajan. Hiçbir ajan hook tarafından engellenmedi; etiket ya da model değiştirilmedi. Betik: çift ayrıştırıcı, tablo sütun denetimi, DV renumaralama, sayım ve rapor tablosu (ajansız).
- **Eşzamanlı alt ajan sınırı.** Doğrulama dalgalarının ikisinde tek mesaja yirmi çağrı yazıldı; araç yirmincisini ("Concurrent subagent limit reached") reddetti, o doğrulayıcı bir sonraki mesajda başlatıldı. Sınırın pratikte on dokuz olduğu görüldü; ileride yirmi yerine on dokuz yazılması önerilir.
- **Kararlar (hepsi ⚠, öneriyle).** K-1269 ödemenin başlatılması ve bildirimin alan eşlemesi (telefon biçimi, `user_ip` ve IPv6, alan sınırları ve firma e-postası yedeği, tutar biçimleri, `failed` bildirimi) · K-1270 ödeme kesintisi ve anahtarlar (kart seçilemez hâli iki dakika bellekte, jeton hata dizisinin durum ucu uyarısı, kalıcı kurulum hatalarının sınırı, çift tahsilat ve iade çakışmasının kalan riski, anahtarlar kaldırılınca iade, anahtar ortam sınırı, sızıntıda kapatma sırası, taksit farkı) · K-1271 e-posta altyapısı (yeniden deneme anları, gövdesi üretilemeyen deneme, askıdaki adres ön sorgusu ve üçüncü IAM izni, IAM kaynak koşulu, başlık güvenliği, DMARC açılışı, itibar alarmı, logo) · K-1272 Google ile giriş (Google'da doğan hesapta bağ yazımı, sabit adresler, kimlik belirteci reddi, `auth_time` ayarı, `sub` kalan riski) · K-1273 gönderim kaydının grubu ve `OriginalDispatchId` · K-1274 `frame-src 'self'` ve çerçevenin kalan riski. **Aşama sonunda proje sahibine gösterilecek ⚠ listesinde ayrıca öne çıkarılacak:** K-1270 (10) taksit farkının iadesi — ürün yalnız `Refund.Amount` iade eder; fark müşteriye dönmüyorsa caymada ve iptalde müşteri masraf taşır (Mesafeli Sözleşmeler Yönetmeliği m.13/2); DV-10'un sonucuna bağlandı, olumsuzsa karar kaydıyla `02`'ye gider.
- **Hükme bağlanmayan gözlemler** (doğrulayıcıların ayrı bulgu adayı diye bıraktıkları; yeni K gerekmeden bir sonraki turda ele alınır): `05 §4.3.4` ve 11.1.21'deki iş kuyruğu varsayılanı "1, 5, 15, 60, 240" iade sorgusu basamaklarıyla aynı kalıpta (GF; `05`'in kararı, `09`/implementation'a park) · Google dönüşünde art arda üç 15 sn'lik çağrının toplam süre tavanı yeni bir tasarım değeri (GJ; `05 §10.1.1` MK-61'in işi) · SES'in gönderen adındaki `"` ve `\` için döndürdüğü hata doğrulanamadı (L4b-3) · PayTR'nin iki anahtar çiftini birlikte geçerli tutup tutmadığı ve 3D Secure sayfasının çerçeve düzeni belgede yazılı değil — DV-40 ve DV-5'e bağlandı.
- **Doğrulanamayan dış olgular.** PayTR: anahtar yenileme biçimi, `failed` bildiriminin deneme başına gelişi, IPv6 kabulü (DV-39, DV-40, DV-42); AWS: `MessageRejected` hata ayrımı, itibar eşikleri; Google: yok (Session age claims ve `claims` davranışı güvenlik paketi sayfasından okundu, 2026-10-29). Yasal: Mesafeli Sözleşmeler Yönetmeliği m.13/2 atfı `02`'deki kullanıma dayanıyor, mevzuat.gov.tr'de bu turda açılmadı ("doğrulanamadı"; audit'in 6563 ve KVKK doğrulamaları yeniden açılmadı).
- **Çok kritik soru, yönetici onayı bekleyen karar yok.**
