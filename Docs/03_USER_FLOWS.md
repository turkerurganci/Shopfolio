# Shopfolio — User Flows

**Versiyon: v0.3** | **Bağımlılıklar:** `01_PROJECT_VISION.md`, `02_PRODUCT_REQUIREMENTS.md`, `10_MVP_SCOPE.md` (kapsam), `PRODUCT_DISCOVERY_STATUS.md` (Aşama 2 kararları) | **Son güncelleme:** 2026-10-04

> **Aşama:** 2 — Kullanıcı Akışları · **Rol:** Product Owner / Business Analyst
> **Traceability zorunlu:** Hayır (doğrudan türetim) — ama akışlar yazıldıktan sonra `02`'ye **geri dönülür**, tutarsızlık varsa düzeltilir.

> **Yazım durumu (K-28, K-30, K-432):** doküman Aşama 2'nin **tek yazım turunda**, beş oturumda yazılır; sıra ve konular `PRODUCT_DISCOVERY_STATUS.md` §8.2–§8.3'tedir. Girdi karar kaydı, bu dokümanın şablonu ve üst dokümanlardır — workshop'un sohbet geçmişi değil.
> - **1. oturum (2026-10-04, v0.2):** §0 — yazım konvansiyonları ve bölüm haritası (blok AK0) · §1 — durum makinesi (blok AK1) · §11 — geri besleme tablosunun ilk satırları. Oturum dokuz karar aldı: ikisi yazım konvansiyonu (K-669, K-670), yedisi yazımın bulduğu boşluk (K-671…K-677); yedisi `02`'ye, biri `10`'a da döndü (§11).
> - **2. oturum (2026-10-04, v0.3):** §2 — müşteri tarafının ana akışları ve uçtan uca anlatılar (blok AK2) · §11'e dört satır. Oturum yazımın bulduğu dört boşluğu karara bağladı (K-678…K-681); dördü `02`'ye, ikisi `10`'a da döndü (§11). Aynı PR'da §1 hizalandı: ayıp talebinin müşteri tarafından yeniden açılmasının bildirimi (§1.9.1, §1.11.10 — K-681) ve S9 ile S11'in "Akış" sütunu.
> - **Sıradaki oturumlar:** §3–§6 (AK3–AK6) · §7 ve §8 (AK7, AK8) · §9–§11 (AK9–AK11). Kalite döngüsü tur bittikten sonra başlar (K-432).

---

## 0. Nasıl yazılır

- **Aktör bazlı ilerleme:** Her aktör için ayrı akış seti — yerleşim §0.2'dedir.
- **Normal akış + hata akışları:** Önce "her şey yolunda", sonra *"ne ters gidebilir?"* ile hata ve alternatif akışlar. **Hata akışları happy path kadar önemlidir.**
- **Durum makinesi mantığı:** Her adım bir durumdan diğerine geçiştir. Durum isimleri `02 §5` ile birebir aynı; durum makinesi §1'dedir ve akışlar ona atıf yapar.
- **Bildirim entegrasyonu:** Her adımda *"burada kime bildirim gider?"* sorulur; tetikleyiciler akışa gömülür ve §7'de özetlenir.
- **Narrative gerektiren senaryolar:** if/then ile anlatılamayan uçtan uca senaryolar §2'nin son alt bölümünde — §2.10 "Uçtan uca anlatılar" — yazılır (K-651).

### 0.1 Kapsam ve kaynaklar

**0.1.1 Bu doküman kural koymaz; `02`'nin kurallarını dört aktörün adım adım deneyimine çevirir.** Aktörler `02 §1.3`'tedir: ziyaretçi · misafir alıcı · üye müşteri · firma yöneticisi. Kural, sayı ve kapsam kararlarının otoriter kaynağı Ürün Gereksinimleri (`02`) ve MVP Kapsamı'dır (`10`); Aşama 1'in kayıtları salt okunurdur ve dokümanla ayrışırsa doküman kazanır (K-647).

**0.1.2 `02`'nin ya da `10 §2`'nin saymadığı bir yetenek akışa girmez.** Akış yazılırken bir yetenek, kural ya da terim eksik çıkarsa bu bir **boşluktur:** sessizce eklenmez ve uydurulmaz; karar kaydında bir K satırıyla karara bağlanır, `02`'ye — `10`'un satırına dokunuyorsa `10`'a da — aynı PR'da sürüm artışıyla yazılır ve §11'e satır girer (K-652, §0.6).

**0.1.3 Kapsam dışı kalemler "yoktur" diye anılır.** Şablonun ya da akışın beklediği ama ürünün bilinçli olarak sunmadığı bir adım — bildirim tercihi, elle sipariş, bakım modu — akışta tek cümleyle ve `02` ya da `10 §3` atfıyla "yoktur" diye yazılır; boş bırakılmaz.

### 0.2 Bölüm haritası

Şablonun bölümleri korunur; alt bölüm numaraları aşağıdaki tabloyla sabittir (K-650, K-651, K-670). Bir yazım oturumu bir alt bölümü bölmek ya da birleştirmek zorunda kalırsa tabloyu ve ona atıf yapan satırları aynı PR'da günceller. Tablonun "Oturum" sütunu yazım turunun planıdır (`PRODUCT_DISCOVERY_STATUS.md` §8.2).

| Bölüm | Alt bölümler | Plan konuları | Oturum |
|---|---|---|---|
| §1 Durum makinesi | 1.1 Envanter · 1.2 Sevkiyat ekseni · 1.3 Ödeme ekseni · 1.4 İki eksenin bağı · 1.5 Tipe göre hatlar · 1.6 Geri alınamaz geçişler ve düzeltme · 1.7 Kalem kayıtları ve panel işaretleri · 1.8 Yayın durumları · 1.9 Talep durumları · 1.10 Durum makinesi olmayan varlıklar · 1.11 Durum × rol × işlem | AK1-01…AK1-09 | 1 ✓ |
| §2 Ana akışlar — müşteri tarafı | 2.1 Ziyaretçi: vitrinde gezinme ve ürünü bulma · 2.2 Ziyaretçi: kurumsal içerik ve iletişim formu · 2.3 Müşteri: sepet · 2.4 Müşteri: ödeme adımı ve sipariş onayı · 2.5 Müşteri: ödeme ve teslim · 2.6 Müşteri: sipariş takibi (akış 2) · 2.7 Müşteri: iptal ve gecikme feshi · 2.8 Müşteri: cayma ve iade · 2.9 Müşteri: ayıp talebi · 2.10 Uçtan uca anlatılar | AK2-01…AK2-10 | 2 ✓ |
| §3 Hata akışları | 3.1 Bütün akışlara uygulanan ilkeler · 3.2 Satın alma ve sipariş takibi · 3.3 İptal, cayma ve iade · 3.4 Üyelik · 3.5 Firma tarafı | AK3-01…AK3-06 | 3 |
| §4 Zaman aşımı yönetimi | 4.1 Süre envanterinden zaman aşımı akışları · 4.2 Kendiliğinden işleyen anlar | AK4-01, AK4-02 | 3 |
| §5 İtiraz ve anlaşmazlık | 5.1 Üç yol ve itirazın yeri · 5.2 Ürünün dışında çözülen anlaşmazlıklar · 5.3 İade reddi ve değer kaybı | AK5-01…AK5-03 | 3 |
| §6 Kötüye kullanım ve inceleme | 6.1 Deneme limitleri · 6.2 Teyit ve kandırılma akışları · 6.3 Firmanın inceleme akışları | AK6-01…AK6-04 | 3 |
| §7 Bildirim haritası | 7.1 Müşteriye giden bildirimler · 7.2 Firmaya giden bildirimler ve hesap e-postaları · 7.3 Geçiş ve olay bazlı eşleme | AK7-01…AK7-03 | 4 |
| §8 Yönetim akışları — firma tarafı | 8.1 Katalog yönetimi (akış 5) · 8.2 Sipariş yürütümü: olağan hat (akış 6) · 8.3 Firma iptali, iade teslim alma ve geri ödeme · 8.4 Sipariş müdahaleleri · 8.5 Manuel adım bütçesi · 8.6 Kurumsal içerik, marka, duyuru ve iletişim talepleri (akış 7'nin panel yüzü) · 8.7 Mağaza ayarları, satış kapısı ve kurulum kontrol listesi (akış 8) · 8.8 Yönetici hesapları ve davet · 8.9 Bekleyen işler, satış özeti, işlem izi ve dışa aktarma | AK8-01…AK8-11 | 4 |
| §9 Destekleyici akışlar — üyelik (akış 4) | 9.1 Kayıt, e-posta doğrulama ve Google ile ilk giriş · 9.2 Giriş, oturum, şifre sıfırlama ve yeniden doğrulama · 9.3 Profil ve adres defteri · 9.4 Hesabın silinmesi ve kişisel veri başvurusu · 9.5 Tercih yönetimi | AK9-01…AK9-05 | 5 |
| §10 Operasyonel akışlar | 10.1 Dış servis kesintileri · 10.2 Bakım ve güncelleme · 10.3 Sitenin kesintisi · 10.4 Panele erişimin kaybı | AK10-01…AK10-04 | 5 |
| §11 `02`'ye geri besleme | tek tablo | AK11-01 | her oturum satır ekler; 5. oturum kapatır |

Akış 6'nın yönetici müdahaleleri §8'de akışın parçası olarak kalır (K-468, K-650); fiziksel siparişin uçtan uca ana akışı (akış 1 + 6, `02 §2.2`) §2.10'da birleşir (K-651).

### 0.3 Adlar ve kimlikler

**0.3.1 Adlar `02` ile birebirdir.** Durum adları `02 §5`'ten, terimler ve İngilizce kod karşılıkları `02 §1.2`'den alınır; eş anlamlı kullanılmaz (K-17). Kod karşılığı yalnız `02`'nin verdiği yerde ve yalnız §1'in tablolarında yazılır — bu doküman kod adı **icat etmez**; yeni bir terim gerekirse §0.1.2'nin boşluğudur. Panel işaretlerinin metni (*"e-posta ulaşmadı"* gibi) `02`'deki tırnaklı biçimiyle yazılır.

**0.3.2 Aktör yazımı.** Adım tablolarının aktörü şu adlardan biridir: **Ziyaretçi** · **Misafir alıcı** · **Üye** (üye müşterinin kısaltması) · **Müşteri** (misafir alıcı ya da üye — `02 §1.3`) · **Yönetici** (firma yöneticisinin paneldeki işlemi) · **Sistem** (ürünün kendiliğinden yaptığı işlem). "Firma" satıcının kendisidir: yükümlülük ve sorumluluk cümlelerinde kullanılır, panel adımının aktörü değildir.

**0.3.3 `02`'nin kimlikleri olduğu gibi kullanılır** (K-532): sevkiyat geçişleri **S1…S11**, ödeme geçişleri **Ö1…Ö5** (`02 §5.4`, §5.5) · müşteri bildirimleri **B-1…B-16**, firma bildirimleri **F-1…F-5** (`02 §9`) · süreler **Z-1…Z-47** (`02 §4.2`) · deneme limitleri **L-1…L-9** (`02 §8.2`) · parametreler **P-1…P-49** (`02 §11`) · hacim kabulleri **H-** (`02 §11.3`). `10`'un kimlikleri: kapsam satırları **KP-**, kapsam dışı satırları **KD-**, dış ön koşullar **ÖK-**, kısıtlar ve kabuller **SK-**, yol haritası adayları **YH-** (`10` başlık notu). Proje Vizyonu'nun (`01`) kimliklerine atıf her yerde `01 §…` diye nitelenir.

**0.3.4 Bu dokümanın kendi satır kimlikleri bölüm numaralıdır** (K-669): bir alt bölümün adım ya da satır tablosu `n.m.k` biçiminde numaralanır ve dışarıdan `03 §n.m.k` diye anılır — ör. §1.11'in on dokuzuncu satırı `03 §1.11.19` — `02 §6`'nın satır kalıbı (6.4.16). Harf önekli yeni bir kimlik ailesi açılmaz. Bir alt bölüm ya adım tablosu ya da kendi alt başlıklarını taşır, ikisini birden taşımaz. Bu doküman içinde önsüz `§` bu dokümanı gösterir; başka dokümana her atıf numarasıyla yazılır (`02 §7.3.4`).

**0.3.5 Durum yazımı.** Siparişin iki ekseni birlikte `sevkiyat + ödeme` diye yazılır: *Alındı + Bekliyor*. Geçiş, kimliğiyle birlikte yazılır: *→ Hazırlanıyor + Ödendi (S1, Ö1)*. Kalem düzeyindeki değişiklik kaydın adıyla yazılır (§1.7): *kalem: cayma beyanı*.

### 0.4 Akış adımının biçimi

**0.4.1 Ana akışlar (§2, §8, §9) adım tablosuyla yazılır.** Şablonun dört sorusu sütundur:

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|

- **#** — §0.3.4'ün numarası.
- **Durum** — değişiklik yoksa "—"; varsa §0.3.5'in biçimi.
- **Bildirim** — kimlik ve alıcı: *B-5 → müşteri*, *F-1 → firma*; bildirim yoksa "—". Bildirim üretmeyen bir olay `02 §9.2`'nin "bildirim üretmeyen olaylar" listesindeyse "— (`02 §9.2`)" yazılır. Müşteri bildirimi siparişin iletişim e-postasına, firma bildirimi firmanın iletişim e-postasına gider; tek istisna F-5'tir (`02 §9.3`).
- **Kaynak** — `02 §…`; Aşama 2'nin kararlarında K numarası da (§0.5.2).

**0.4.2 Hata akışları (§3) tabloyla yazılır** ve ana akışın adımına bağlanır:

| # | Akış adımı | Ne ters gider | Sistemin cevabı | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|---|

"Akış adımı" ana akışın satır numarasıdır (`§n.m.k`); Kaynak `02 §6`'nın satırını (`02 §6.2.15`) gösterir. Her hata satırı bir ana akış adımına bağlanır; bağlanamayan hata bir boşluktur (§0.1.2).

**0.4.3 Zaman aşımları (§4) süre kimliğiyle yazılır:** süre (Z-) · başlangıç · varsa uyarı · sona erişte sistemin işlemi · normale dönüş. Değer yazılmaz, kimlik yazılır (§0.5.3).

**0.4.4 Her adımın durumu §1'e uyar.** Bir adım §1'in izin vermediği bir geçiş ya da birleşim üretiyorsa ya akış yanlıştır ya §1; ikisi aynı PR'da hizalanır ve fark `02`'ye dokunuyorsa §0.1.2 işler.

**0.4.5 Yeni elle adım bütçeyle okunur.** Akış yazılırken yöneticiye sipariş başına yeni bir zorunlu elle adım doğuyorsa öneri `02 §10.5`'in bütçesiyle birlikte okunur ve sayıyı geçirecekse karar kaydına döner (K-404, K-454; §8.5).

### 0.5 Kural ayrıntısı ve kaynak gösterme

**0.5.1 Kural ayrıntısı evinde kalır.** Bir adım bir kurala dayanıyorsa kuralın koşullarını kopyalamaz; ne olduğunu bir cümleyle yazar ve kurala bölüm numarasıyla işaret eder. Ayrıntıyı kopyalayan metin `02` ile ayrıştığı anda iki doğru üretir (`checklists/document-stage.md` §4).

**0.5.2 Kaynak önce dokümandır.** Aşama 1'in kararları `02 §…` ve `10` KP- atfıyla gösterilir — K numarası gerekmez, kararın evi dokümandır (K-647). **Aşama 2'nin kararları** (K-647'den sonraki) K numarasıyla gösterilir; kararın `02`'ye döndüğü yerde `02 §…` de yanına yazılır.

**0.5.3 Sayısal değer yazılmaz, kimlik yazılır.** Süre, limit ve parametre değerleri `02 §4`, §8.2 ve §11'de yaşar; akış *"Z-8 dolunca"*, *"L-1 aşılırsa"*, *"P-8 kadar"* diye yazar. Yalnız yasal sabit — cayma penceresinin on dört günü gibi, `02 §11`'e girmeyen — metin içinde adıyla anılır ve süre kimliği yanına yazılır.

**0.5.4 Belirsiz ifade kullanılmaz.** *"Muhtemelen"*, *"belki"*, *"gerekirse karar verilir"* yazılmaz; yetkili bir seçimi anlatan cümle *"… yapabilir"* yerine işlemin kendisini ve koşulunu yazar (`GUARDRAILS §5`).

### 0.6 Boşluk, geri besleme ve devir

**0.6.1 Boşluk oturumda karara bağlanır.** Yazımın bulduğu boşluk K satırıyla, `checklists/document-stage.md` §3'ün satır biçimiyle ve K-648'in kayıt moduyla kapanır; ⚠ grubundaki karar `PRODUCT_DISCOVERY_STATUS.md` §8.1'in ⚠ listesine girer. Bu dokümanda "açık" işaretli adım bırakılmaz.

**0.6.2 Açık kalem ve devir.** Bu dokümanın şablonunda açık kararlar bölümü yoktur: karara bağlanmış bir varlığın yalnız **detayı** açık kalabilir ve tracker §4'te **A-19**'dan sürer, gözlemlenebilir bir kapıyla (K-37, `00 §N.1`). Sonraki aşamaya talimat bırakan karar hedef dokümanın başındaki **"Aşama 2'den park edilen girdiler"** bloğuna satır olarak girer — blok yoksa `05`'teki biçimiyle açılır (K-36; ilk örnek K-666, K-667). Aşama içi ileri referans karar satırının etki sütununda `(downstream)` olarak kalır.

**0.6.3 Geri besleme tablosu §11'dir.** `02`'ye ya da `10`'a dönen her karar §11'e bir satırla girer: bulgu, karar numarası, değişen bölümler ve sürüm.

---

## 1. Durum makinesi

> **Ne yazılır:** Tüm durumlar, izin verilen geçişler, yasak geçişler, her geçişin tetikleyicisi. **UI tasarımından önce netleşmelidir** — ekran varyantları buradan türetilir.

Durum adları, kodları ve geçişlerin koşulları `02 §5`'tedir ve burada tekrarlanmaz. Bu bölüm o makineyi akışa bağlar: her geçişi **kimin** tetiklediğini, **hangi akışta** gerçekleştiğini ve **kime bildirim** gittiğini yazar; durum çiftlerini, tip hatlarını, kalem kayıtlarını ve her rolün hangi durumda hangi işlemi yapabildiğini (§1.11) tek yerde toplar. "Akış" sütunları §0.2'nin alt bölümlerini gösterir.

### 1.1 Envanter

| Varlık | Durum seti | Kaynak | Bu bölümde |
|---|---|---|---|
| Sipariş — sevkiyat ekseni (`OrderStatus`) | Alındı · Hazırlanıyor · Kargoya verildi · Teslim edildi · Teslim edilemedi · İptal edildi | `02 §5.4` | §1.2 |
| Sipariş — ödeme ekseni (`PaymentStatus`) | Bekliyor · Ödendi · Başarısız · Kısmen geri ödendi · Geri ödendi | `02 §5.5` | §1.3 |
| Ürün ve varyant (`PublishStatus`) | Taslak · Yayında · Arşiv | `02 §5.1` | §1.8 |
| Kurumsal içerik kaydı (`PublishStatus`) | Taslak · Yayında | `02 §5.2` | §1.8 |
| Ayıp talebi (`DefectClaimStatus`) | Açık · Çözüldü | `02 §5.10` | §1.9 |
| İletişim talebi — KVKK talebi dahil (`ContactRequestStatus`) | Açık · Kapatıldı | `02 §5.13` | §1.9 |

**Durum makinesi taşımayanlar:** sipariş kalemi — kayıtlar taşır (§1.7) · indirim, kupon, stok ayırma, duyuru, hesap, oturum ve yönetici daveti — zamana ya da kullanıma bağlı geçerlilik taşır (§1.10) · mağazanın satış açıklığı — dört koşullu bir kapıdır, varlık durumu değildir (`02 §3.1.5`, §5).

### 1.2 Sevkiyat ekseni — Sipariş durumu

| Durum | Anlamı | Sonraki olası durumlar | Tetikleyici |
|---|---|---|---|
| Alındı (`Placed`) | Sipariş müşterinin onayıyla doğdu; ödeme ya da — fiziksel açık kalemi olmayan siparişte — kalemlerin teslim işaretleri bekleniyor | Hazırlanıyor (S1) · Teslim edildi (S2) · İptal edildi (S3) | Ödeme onayı · son teslim işareti ya da kapanış · iptal |
| Hazırlanıyor (`Preparing`) | Ödenmiş, fiziksel açık kalem taşıyan sipariş hazırlanıyor; kargoya verme süresi (Z-10) işliyor | Kargoya verildi (S5) · İptal edildi (S4) · Teslim edildi (S10) | Yöneticinin kargoya vermesi · iptal ya da kapanış · fiziksel kalemlerin kapanması |
| Kargoya verildi (`Shipped`) | Gönderi yolda; fiziksel kalemin iptal yolu kapandı, cayma düğmesi açıldı | Teslim edildi (S6) · Teslim edilemedi (S7) | Yöneticinin işareti |
| Teslim edilemedi (`DeliveryFailed`) | Gönderi firmaya geri döndü | Kargoya verildi (S8) · İptal edildi (S9) · Teslim edildi (S11) | Yöneticinin yeniden gönderimi · iptal ya da kapanış · kalemlerin kapanması |
| Teslim edildi (`Delivered`) | **Terminal.** Fiziksel siparişte teslim işareti ve tarihi; fiziksel açık kalemi olmayan siparişte açık kalemlerin tamamının teslim işareti | — | Yalnız yöneticinin düzeltmesi (§1.6) |
| İptal edildi (`Cancelled`) | **Terminal.** Teslimattan önce iptal edilmiş ya da açık kalemi kalmadan ve hiçbir kalemi teslim edilmeden kapanmış sipariş | — | Yalnız yöneticinin düzeltmesi (§1.6) |

**Geçişler — kim tetikler, hangi akışta, kime bildirim gider.** Koşullar `02 §5.4`'ün beyaz listesindedir; listede olmayan her geçiş yasaktır.

| # | Geçiş | Tetikleyen | Akış | Bildirim |
|---|---|---|---|---|
| S1 | Alındı → Hazırlanıyor | **Sistem**, ödeme onayıyla aynı anda (Ö1) — kartta sağlayıcının başarı bildirimi, havalede yöneticinin "ödendi" işareti; sipariş en az bir fiziksel kalem taşır | §2.5, §8.2 | B-4 → müşteri (Ö1'in); Hazırlanıyor'a geçiş ayrıca bildirim üretmez (`02 §9.2`) |
| S2 | Alındı → Teslim edildi | **Kapanış kuralını tamamlayan son olay** (§1.4.3) — yalnız dijital siparişte ödeme onayı (sistem); yöneticinin son hizmet kalemini "tamamlandı" işaretlemesi; son açık kalemin müşteri ya da yönetici tarafından iptali, çıkarılması ya da cayma beyanıyla kapanması | §2.5, §2.7, §2.8, §8.2–§8.4 | Ödeme onayında B-4; teslim işareti bildirim üretmez (`02 §9.2`); kapanışta olayın kendi bildirimi (B-7, B-9, B-11) |
| S3 | Alındı → İptal edildi | **Müşteri** siparişi iptal eder — ödeme beklenirken bütünüyle, ödenmişse son açık kalemi · **Yönetici** sebep seçerek iptal eder · **Sistem:** ödeme süresi dolar (Z-7, Z-8), kart ödemesi başarısız olur ya da aynı sepetten yeni sipariş onaylanır · **Kapanış** — son açık kalem cayma beyanıyla ya da çıkarmayla kapanır ve hiçbir kalem teslim edilmemiştir (K-672) | §2.4, §2.5, §2.7, §4.1, §8.3 | B-7 → müşteri; kapanışta B-7 gitmez — müşteri olayın kendi bildirimini almıştır (B-9, B-11) |
| S4 | Hazırlanıyor → İptal edildi | **Müşteri** ya da **yönetici** son açık kalemi iptal eder · **Kapanış** — gecikme feshi siparişte açık kalem bırakmaz (`02 §5.8`) ya da son açık kalem cayma beyanıyla veya çıkarmayla kapanır (K-672); kapanış sistemin, olayla aynı anda | §2.7, §2.8, §8.3, §8.4 | B-7 → müşteri; kapanışta gitmez (B-9, B-11) |
| S5 | Hazırlanıyor → Kargoya verildi | **Yönetici** kargoya verir — geri alınamaz onayıyla (§1.6); yalnız açık fiziksel kalemler gider | §8.2 | B-5 → müşteri |
| S6 | Kargoya verildi → Teslim edildi | **Yönetici** teslim işaretini ve teslim tarihini girer; kendiliğinden geçiş yoktur | §8.2 | — (`02 §9.2`) |
| S7 | Kargoya verildi → Teslim edilemedi | **Yönetici** geri dönen gönderiyi işaretler | §8.2 | B-6 → müşteri |
| S8 | Teslim edilemedi → Kargoya verildi | **Yönetici** açık fiziksel kalemleri yeniden gönderir — geri alınamaz onayıyla (K-677) | §8.2 | B-5 → müşteri, yeniden |
| S9 | Teslim edilemedi → İptal edildi | **Yönetici** sebep seçerek iptal eder · **Kapanış** — açık kalem kalmamışsa yönetici S7'den sonra siparişi sebepsiz kapatır (`02 §5.8`) | §2.7, §2.10, §8.3 | B-7 → müşteri; kapanışta gitmez (B-9) |
| S10 | Hazırlanıyor → Teslim edildi | **Kapanış kuralını tamamlayan son olay** (§1.4.3) — son açık fiziksel kalemin iptali, çıkarılması ya da gecikme feshi; son hizmet kaleminin "tamamlandı" işareti ya da cayma beyanı | §2.7, §2.8, §8.2–§8.4 | Olayın kendi bildirimi |
| S11 | Teslim edilemedi → Teslim edildi | **Kapanış kuralını tamamlayan son olay** (§1.4.3) — S7'nin kendisi dahil: geri dönen fiziksel kalemler iptal, cayma beyanı ya da fesihle kapanmışsa | §2.7, §2.8, §2.10, §8.2, §8.3 | Olayın kendi bildirimi (S7'de B-6) |

**Yasak geçişler:** geri yönlü her geçiş — Kargoya verildi → Hazırlanıyor, Teslim edildi'den ve İptal edildi'den herhangi bir duruma — müşteri akışında yoktur (`02 §5.4`, §7.6.3). Yöneticinin düzeltmesi beyaz listenin dışındadır ve sınırları §1.6'dadır.

### 1.3 Ödeme ekseni — Ödeme durumu

| Durum | Anlamı | Sonraki olası durumlar | Tetikleyici |
|---|---|---|---|
| Bekliyor (`Pending`) | Ödeme gelmedi | Ödendi (Ö1) · Başarısız (Ö2) | Ödeme onayı · ödenmeden iptal |
| Ödendi (`Paid`) | Ödeme onaylandı | Kısmen geri ödendi (Ö3) · Geri ödendi (Ö4) | Geri ödeme |
| Başarısız (`Failed`) | **Terminal.** Ödeme alınmadan kapandı — sebep iptal kaydındadır | — | — |
| Kısmen geri ödendi (`PartiallyRefunded`) | Paranın bir kısmı geri ödendi | Geri ödendi (Ö5) | Geri ödeme |
| Geri ödendi (`Refunded`) | **Terminal.** Paranın tamamı geri ödendi | — | — |

| # | Geçiş | Tetikleyen | Akış | Bildirim |
|---|---|---|---|---|
| Ö1 | Bekliyor → Ödendi | **Sistem** — kartta sağlayıcının başarı bildirimi ya da ödeme süresi dolduğunda son sorgunun olumlu cevabı (`02 §6.1.2`) · **Yönetici** — havalede "ödendi" işareti, geri alınamaz onayıyla. Eşzamanlı geçişler §1.4.2'de | §2.5, §8.2 | B-4 → müşteri |
| Ö2 | Bekliyor → Başarısız | S3 ile aynı anda — ödenmemiş siparişin her iptali: sistemin, müşterinin ya da yöneticinin | §2.5, §2.7, §4.1, §8.3 | S3'ün bildirimi (B-7) |
| Ö3 | Ödendi → Kısmen geri ödendi | **Sistem** — iptal edilen ya da gecikme feshine uğrayan ödenmiş kart kaleminin iadesi sağlayıcıda gerçekleşir · **Yönetici** — havale hattında geri ödemeyi işler; caymada iki hatta da işler; ayıp talebinin para gerektiren çözümünde ve tutar bazlı kısmi geri ödemede işler — geri alınamaz onayıyla | §2.7–§2.9, §8.3, §8.4 | B-8 → müşteri; sistemin kendiliğinden yaptığı kart iadesinde F-4 → firma |
| Ö4 | Ödendi → Geri ödendi | Ö3'ün tetikleyenleri; siparişin parasının tamamı geri ödenir | §2.7–§2.9, §8.3 | B-8 → müşteri; sistemin kart iadesinde F-4 → firma |
| Ö5 | Kısmen geri ödendi → Geri ödendi | Ö3'ün tetikleyenleri; kalan paranın tamamı geri ödenir | §2.7–§2.9, §8.3 | B-8 → müşteri; sistemin kart iadesinde F-4 → firma |

**Ekseni değiştirmeyen olaylar** (`02 §5.5`): kart iadesinin sağlayıcıda başarısız olması — panel işareti düşer (§1.7) · iptal edilmiş siparişe gelen kart ödemesi ve sistemin onu kendiliğinden geri ödemesi — ödeme kaydında durur, F-4 → firma (`02 §6.2.16`) · ters ibraz — ürünün dışındadır (`02 §3.21.11`) · cayma beyanı, gecikme feshi ve iade malının teslim alınması — para fiilen gönderilene kadar (`02 §5.6.6`). **Yasak geçiş:** Başarısız → Ödendi. Ödeme ekseninin tek düzeltmesi yanlış konmuş havale "ödendi" işaretidir (§1.6).

### 1.4 İki eksenin bağı

**1.4.1 Birleşim tablosu.** Her siparişin her an iki durumu vardır (`02 §5.3.1`). ✓ izinli, ✗ ulaşılamaz ya da yasak çifttir; harfler tablonun altındadır.

| Sevkiyat ↓ · Ödeme → | Bekliyor | Ödendi | Başarısız | Kısmen geri ödendi | Geri ödendi |
|---|---|---|---|---|---|
| Alındı | ✓ (a) | ✓ (b) | ✗ (e) | ✓ (b) | ✓ (f) |
| Hazırlanıyor | ✗ (c) | ✓ | ✗ (e) | ✓ | ✓ (f) |
| Kargoya verildi | ✗ (c) | ✓ | ✗ (e) | ✓ | ✓ (f) |
| Teslim edilemedi | ✗ (c) | ✓ | ✗ (e) | ✓ | ✓ (f) |
| Teslim edildi | ✗ (d) | ✓ | ✗ (d) | ✓ | ✓ |
| İptal edildi | ✗ (g) | ✓ (h) | ✓ | ✓ (h) | ✓ |

(a) Yeni sipariş; ödeme bekleniyor. (b) Yalnız fiziksel açık kalemi olmayan sipariş — kalemlerin teslim işaretleri bekleniyor; fiziksel kalemli sipariş ödeme onayıyla aynı anda Hazırlanıyor'a geçer (S1). (c) Kapı koşulu: Hazırlanıyor'a ancak ödeme onaylanınca geçilir; ödeme Bekliyor'a yalnız havale işaretinin düzeltilmesiyle ve sevkiyatla birlikte döner (`02 §5.6.1`; §1.6). (d) `02 §5.6.1`'in adıyla yasakladığı iki birleşim. (e) Başarısız yalnız ödenmeden iptalle, İptal edildi'ye geçişle aynı anda doğar (`02 §5.6.3`, §5.6.5). (f) Yalnız açık kalem varken tutar bazlı geri ödeme ödenmiş tutarın tamamını geri öderse — tutar ödenmiş ve geri ödenmemiş tutarı aşamaz (`02 §5.5`). (g) Ödenmeden iptal ödemeyi Başarısız yapar (`02 §5.6.5`); kart son sorgusu yanıtsız kaldığında sipariş iptal edilmez, Alındı + Bekliyor'da kalır (`02 §6.1.2`). (h) Geri ödemesi henüz gerçekleşmemiş iptal ya da kapanış — havalede yöneticinin işlemesi bekleniyor, kartta iade sağlayıcıda gerçekleşmedi; yanlış iptalin düzeltilmesi yalnız bu hâlde yapılır (`02 §10.4.6`).

**1.4.2 Eşzamanlı geçişler** — iki ekseni ya da ekseni ve kalemi aynı anda değiştiren olaylar (`02 §5.6`):

| # | Olay | Aynı anda olan |
|---|---|---|
| 1.4.2.1 | Ödeme onayı (Ö1) | Fiziksel açık kalemi olan siparişte S1 · yalnız dijital siparişte S2 · her siparişte dijital kalemlerin teslim işareti — fiziksel kalemsiz siparişte kapanış kuralı (§1.4.3) |
| 1.4.2.2 | Ödenmemiş siparişin iptali — sistemin, müşterinin ya da yöneticinin | S3 + Ö2 |
| 1.4.2.3 | Teslim işareti almamış son açık kalemin kapanması ya da teslim işareti alması | Kapanış kuralı (§1.4.3) |
| 1.4.2.4 | Havale "ödendi" işaretinin düzeltilmesi (yönetici) | Ödendi → Bekliyor ve Hazırlanıyor → Alındı birlikte (§1.6) |

**1.4.3 Kapanış kuralı.** "Açık kalem" `02 §5.8`'in tanımıdır: iptal edilmemiş, çıkarılmamış, cayma beyanıyla ya da gecikme feshiyle kapanmamış kalem. Fiziksel açık kalem taşıyan sipariş fiziksel hattı izler ve Teslim edildi'ye S6 ile geçer; dijital ve hizmet kalemleri onu beklemez (`02 §5.7`). **Fiziksel açık kalemi olmayan siparişte** — baştan fiziksel kalemsiz ya da fiziksel kalemleri kapanmış — teslim işareti almamış son açık kalem kapandığı ya da teslim işaretini aldığı anda sipariş hattını bitirir:

- **En az bir kalem teslim edilmişse** → Teslim edildi: Alındı'dan S2, Hazırlanıyor'dan S10, Teslim edilemedi'den S11 (`02 §5.4`, §5.6.6).
- **Hiçbir kalem teslim edilmemişse** → İptal edildi. Son olay müşterinin ya da yöneticinin kalem iptaliyse geçiş o iptaldir (S3, S4, S9) ve B-7 gider. Son olay cayma beyanı, gecikme feshi ya da çıkarmaysa geçiş bir **kapanıştır**: sebep seçilmez, iptal kaydı ve ikinci bir geri ödeme açılmaz, B-7 gitmez. Alındı ve Hazırlanıyor'da kapanış sistemin, olayla aynı anda işler (S3, S4 — `02 §5.8`, K-672); Teslim edilemedi'de yöneticinin S7'den sonraki adımıdır (S9 — `02 §5.8`).

Kart son sorgusu yanıtsızken ve kesintide dolan havale süresinde iptal ertelenir; o arada sipariş Alındı + Bekliyor'da kalır (`02 §6.1.2`, §3.21.8; K-667).

**1.4.4 Geçişlerin sayaçlara etkisi** — stok, hizmet kontenjanı ve kupon hakkı (`02 §4.3`):

| An | Stok ve hizmet kontenjanı | Kupon hakkı | Kaynak |
|---|---|---|---|
| Sipariş oluşur (Alındı + Bekliyor) | Ayrılır | Ayrılır | `02 §3.17.3`, §5.11.3 |
| Ödeme onayı (Ö1) | Ayrılan kesin düşer | Kullanılmış sayılır | `02 §4.3` |
| Ödenmeden iptal (S3 + Ö2) | Serbest kalır | Geri döner | `02 §5.6.3` |
| Ödenmiş kalemin iptali ve çıkarma | Kalemin adedi stoğa, kontenjan havuza kendiliğinden döner | Yalnız siparişin tamamı iptal edildiğinde döner | `02 §7.2.6`, §10.4.3 |
| Gecikme feshi | Kargoya verilmemiş kalemde kendiliğinden döner; kargodan dönen mal yöneticinin teslim alma adımıyla eklenir | Feshedilen kalem siparişin kapanışında iptal edilmiş sayılır | `02 §5.8`, §7.2.3 |
| Cayma ve iade | Fiziksel mal yöneticinin teslim alıp eklemesiyle döner, reddedilen mal dönmez; hizmet kontenjanı caymada döner | Yalnız siparişin tamamı iade edildiğinde döner | `02 §7.4.7`–§7.4.9 |
| Yanlış iptalin düzeltilmesi | Yeniden ayrılır; yetmezse düzeltme yapılamaz | Yeniden kullanılmış sayılır | `02 §10.4.6` |
| Havale işaretinin düzeltilmesi | Tutulu kalır | Tutulu kalır | `02 §10.4.6` |
| Varyantın kalıcı silinmesi | Silinen varyantın ayırması düşer | — | `02 §3.7.7` |

### 1.5 Tipe göre hatlar

Hatların tanımı `02 §5.7`'dedir; tipler için ayrı durum seti yoktur. Tablo her hattın olağan dizisini, adımı kimin attığını ve yöneticinin zorunlu elle adım sayısını gösterir (`02 §10.5.1`; bütçe §8.5).

| Hat | Olağan dizi | Zorunlu elle adım |
|---|---|---|
| Fiziksel — kart | Alındı + Bekliyor → *(sistem: Ö1, S1)* Hazırlanıyor + Ödendi → *(yönetici: S5)* Kargoya verildi → *(yönetici: S6)* Teslim edildi | 2 |
| Fiziksel — havale | Kart hattı; Ö1'i yöneticinin "ödendi" işareti tetikler | 3 |
| Yalnız dijital | Alındı + Bekliyor → *(Ö1, S2 — kartta sistem, havalede yönetici)* Teslim edildi + Ödendi | 0 · havalede 1 |
| Yalnız hizmet | Alındı + Bekliyor → *(Ö1)* Alındı + Ödendi → *(yönetici: son "tamamlandı", S2)* Teslim edildi | 1 · havalede 2 |
| Dijital + hizmet | Alındı + Bekliyor → *(Ö1; dijital kalemler teslim)* Alındı + Ödendi → *(yönetici: son "tamamlandı", S2)* Teslim edildi | 1 · havalede 2 |
| Karışık, fiziksel kalemli | Fiziksel hat; dijital kalem ödeme onayında, hizmet kalemi "tamamlandı" işaretinde kalem düzeyinde teslim edilir | Hatların adımları toplanır |
| Fiziksel kalemleri kapanmış karışık | Bulunduğu durumda — Alındı, Hazırlanıyor ya da Teslim edilemedi — kapanış kuralıyla biter (§1.4.3) | Kalan hatların adımları |

### 1.6 Geri alınamaz geçişler, onay pencereleri ve düzeltme

**1.6.1 Geri alınamaz onay isteyen işlemler.** Yönetici işlemi onaylamadan önce sonucunu tek cümleyle görür; panel "geri al" düğmesi sunmaz (`02 §5.9`, §7.6.1).

| # | İşlem | Geçiş ya da kayıt | Akış |
|---|---|---|---|
| 1.6.1.1 | Havale "ödendi" işareti | Ö1 ve eşzamanlı geçişleri; dijital kalemli siparişte işaretin düzeltilemeyeceği önceden söylenir (`02 §10.4.6`) | §8.2 |
| 1.6.1.2 | Kargoya verme ve yeniden gönderim | S5, S8 (K-677) | §8.2 |
| 1.6.1.3 | Firma iptali — sipariş ya da kalem | S3, S4, S9 ya da kalemin iptal kaydı; "stokta bulunamadı" sebebinde yasal uyarı (`02 §7.2.4`) | §8.3 |
| 1.6.1.4 | Geri ödemenin yönetici tarafından işlenmesi — tutar bazlı kısmi geri ödeme dahil | Ö3–Ö5 | §8.3, §8.4 |
| 1.6.1.5 | Kart hattında karta para gönderen kalem çıkarma | Çıkarma kaydı ve sistemin kart iadesi | §8.4 |
| 1.6.1.6 | İade reddi | Kalemin iade reddi kaydı — onay sonucunu tek cümleyle söyler (`02 §7.4.9`) | §8.3 |

Sistemin iptalde kendiliğinden başlattığı kart iadesi bir onay penceresi değildir; firmaya F-4 ile bildirilir (`02 §5.9`). Siparişe dokunmayan onaylar — kalıcı silme, yönetici kaldırma, ayar değişiklikleri — kendi akışlarındadır (§8.1, §8.7, §8.8).

**1.6.2 Düzeltme.** Yanlış yapılmış geçiş yöneticinin panelden, kapalı listeden sebep seçerek yaptığı **yeni bir geçişle** düzeltilir; hem yanlış geçiş hem düzeltme işlem izinde kalır, dış dünyaya çıkmış sonuç — gönderilen e-posta, yola çıkan paket, karta gönderilen para — geri alınmaz ve her düzeltme müşteriye B-13 ile bildirilir (`02 §10.4.6`). Akış: §8.4.

| # | Düzeltilen | Sipariş nereye döner | Koşul |
|---|---|---|---|
| 1.6.2.1 | Erken ya da yanlış konmuş teslim işareti | Teslim edildi'den bir önceki duruma; teslim işareti kalkar, cayma penceresi ve ayıp süresi yeni tarihe kadar işlemez | `02 §10.4.6` |
| 1.6.2.2 | Yanlış iptal | İptal edildi'den iptalden önceki duruma | Ödeme Ödendi'de kalmışsa; ayırmalar yeniden yapılır, yetmezse düzeltme yapılamaz — `02 §10.4.6` |
| 1.6.2.3 | Yanlış konmuş Kargoya verildi ya da Teslim edilemedi işareti | Yanlış geçişten önceki duruma | Eksen bağını bozmaz — `02 §5.4`, §10.4.6 |
| 1.6.2.4 | Para gelmeden konmuş havale "ödendi" işareti | Ödendi → Bekliyor ve Hazırlanıyor → Alındı, birlikte | Kargoya verilmemiş, geri ödeme yapılmamış, hiçbir kalem teslim işareti almamış; Z-8 düzeltme anından yeniden başlar — `02 §10.4.6` |

Kart hattında ödeme ekseninin düzeltmesi yoktur. Teslim tarihinin, adresin ve takip bilgisinin düzeltilmesi geçiş değil müdahaledir (§1.11, §8.4).

### 1.7 Kalem kayıtları ve panel işaretleri

**1.7.1 Kalemin kayıtları.** Kalem durum makinesi taşımaz; aşağıdaki kayıtlar onun yolunu anlatır (`02 §5.8`). "Hatta sayılışı" kaydın sevkiyat ekseninin kapanış kuralına (§1.4.3) etkisidir.

| # | Kayıt | Doğduğu an — kim | Hatta sayılışı | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 1.7.1.1 | Teslim işareti ve teslim tarihi | Fiziksel: yöneticinin S6'sı, sipariş düzeyinde bir tarih · dijital: ödeme onayı, sistem · hizmet: yöneticinin "tamamlandı" işareti | Teslim edilmiş | Dijitalde B-4; öteki hâllerde — (`02 §9.2`) | `02 §5.8`, §3.20.9 |
| 1.7.1.2 | İptal kaydı | Müşteri ya da yönetici — yöneticide sebepli | Kapanmış | B-7 | `02 §7.2` |
| 1.7.1.3 | Çıkarma kaydı | Yönetici | İptal edilmiş gibi | B-11 | `02 §10.4.3` |
| 1.7.1.4 | Gecikme feshi | Müşteri; teslim edilmemiş fiziksel kalemlerin tamamına | Kapanmış — kargoya verilmez, yeniden gönderilmez, firma iptali uygulanmaz | B-9 → müşteri · F-3 → firma | `02 §5.8`, §7.2.3 |
| 1.7.1.5 | Cayma beyanı | Müşteri sipariş sayfasından ya da yönetici başka kanaldan gelen bildirimi kaydederek; fiziksel kalemde iade adresini taşır | Teslim işaretsiz kalemi kapatır; teslim edilmiş kalemde iade sürecini başlatır | B-9 → müşteri · F-3 → firma | `02 §5.8`, §7.3, §10.4.10 |
| 1.7.1.6 | İade teslim alma | Yönetici — ulaşma tarihiyle; stoğa ekleme ayrı adımdır | — | — · IBAN silinmişse B-14 | `02 §7.4.7` |
| 1.7.1.7 | İade reddi | Yönetici — koşullu istisna kaleminde, teslim alma adımında | — | B-16 | `02 §7.4.9` |
| 1.7.1.8 | "Mal dönmedi" kapanışı | Yönetici — Z-42 geçtikten sonra | — | — (`02 §9.2`) | `02 §10.4.9` |
| 1.7.1.9 | IBAN isteği | Sistem — yöneticinin teslim alma adımıyla (IBAN silinmişse), "havale gerçekleşmedi" bildirimiyle, IBAN'sız cayma kaydıyla ya da kart iadesinde havale yolunu açmasıyla | — | B-14 | `02 §5.8`, §7.4.5 |
| 1.7.1.10 | Ayıp talebi | Müşteri | Hattı etkilemez; durumu §1.9 | B-9 → müşteri · F-3 → firma | `02 §5.8`, §7.5 |
| 1.7.1.11 | İndirme sayacı | Sistem her indirmede; yönetici yeniler | — | — | `02 §3.12.5`, §3.12.6 |

**1.7.2 Kalemin yolu tipe göre.**

- **Fiziksel kalem:** oluşur → ödemeden sonra iptal ya da çıkarma — Kargoya verildi'ye kadar → kargoya verilir → kargodayken gecikme feshi ya da cayma beyanı → teslim işareti → pencere içinde cayma beyanı → iade teslim alma · iade reddi · "mal dönmedi" kapanışı — kapanıştan sonra ulaşan mal teslim almayla kalemi yeniden açar. Ayıp talebi Kargoya verildi'den iki yıl (Z-18) açıktır.
- **Dijital kalem:** oluşur → ödeme onayında teslim işareti ve indirme. İptal ve cayma yoktur; ayıp talebi açıktır.
- **Hizmet kalemi:** oluşur → ödemeden sonra iptal, çıkarma ya da cayma beyanı — "tamamlandı" işaretine kadar → "tamamlandı" işareti. Ayıp talebi tamamlamadan sonra açılır.

**1.7.3 Panel işaretleri.** İşaret bir durum değildir; doğuran koşul sürdükçe görünür ve koşul kalkınca kalkar (K-675).

| # | İşaret | Nerede | Düşer | Kalkar | Kaynak |
|---|---|---|---|---|---|
| 1.7.3.1 | "e-posta ulaşmadı" | Siparişin, talebin ya da davetin satırı | Üç yeniden deneme başarısız olunca (Z-26) | Aynı e-posta panelden yeniden gönderilip gönderim başarılı olunca; yeniden gönderilmezse kalır | `02 §9.1.6`, §9.4 · K-675 |
| 1.7.3.2 | "ödeme sonucu alınamadı" | Siparişin satırı | Kart son sorgusu yanıtsız kalınca | Sorgunun sonucu gelince — olağan geçiş (Ö1 ya da S3 + Ö2) işler | `02 §6.1.2` · K-675 |
| 1.7.3.3 | "geri ödeme gerçekleşmedi" | Siparişin satırı | Kart iadesi sağlayıcıda başarısız olunca | Kart iadesi gerçekleşince, müşterinin IBAN'ına geri ödeme işlenince ya da yanlış iptal düzeltilince | `02 §5.5`, §7.2.8, §10.4.6 · K-675 |
| 1.7.3.4 | "IBAN bekleniyor" | Kalem — panelde liste | IBAN isteğiyle | Müşteri IBAN'ı sipariş sayfasından girince | `02 §7.4.5` |
| 1.7.3.5 | "iade malı bekleniyor" | Kalem | Teslimden sonraki cayma beyanıyla | İade teslim alma, iade reddi ya da "mal dönmedi" kapanışıyla | `02 §7.4.1` · K-675 |
| 1.7.3.6 | "e-posta düzeltildi" | Siparişin satırı | Misafir siparişinin e-postası düzeltilince | Kalkmaz — siparişin e-posta geçmişiyle birlikte imha edilir | `02 §10.4.11` · K-675 |

Bekleyen işler sayaçları — hangi kalemin "iade ve geri ödeme bekleyen" sayılıp hangisinin sayılmadığı — `02 §10.6.1`'dedir; akışı §8.9.

### 1.8 Yayın durumları

**1.8.1 Ürün ve varyant.** Ürün ve varyant üç değeri ayrı ayrı taşır; bütün geçişler yöneticinin panel işlemidir, bildirim üretmez ve işlem izine yazılır (`02 §3.7.1`, §5.1).

| Durum | Anlamı | Sonraki olası durumlar | Tetikleyici |
|---|---|---|---|
| Taslak (`Draft`) | Vitrinde yok; adresi ziyaretçiye "sayfa bulunamadı" döner, yöneticiye "Taslak" bandıyla görünür | Yayında · Arşiv | Yönetici — Yayında'ya geçiş yayın kapısına bağlıdır (`02 §3.7.3`, §3.7.4) |
| Yayında (`Published`) | Vitrinde, listelerde ve aramada; tükenmişlik ayrı bir durum değildir | Taslak · Arşiv | Yönetici — yayın kapısını bozan geçiş engellenir (1.8.2) |
| Arşiv (`Archived`) | Vitrinden çıkmış, kaydı korunan ürün; adresi "Bu ürün artık satılmıyor" sayfası döner | Taslak · Yayında | Yönetici — Yayında'ya geçiş yayın kapısına bağlıdır |

**1.8.2 Yayın kapısını bozan işlem engellenir;** panel işlemi durdurur, sebebini söyler ve yöneticiyi önce ürünü taslağa ya da arşive almaya yönlendirir: yayındaki ürünün son Yayında varyantının arşive ya da taslağa alınması · dijital üründe yayındaki bir varyantı dosyasız bırakacak dosya silme · **yayındaki ürünün son Yayında varyantının kalıcı silinmesi** (`02 §3.7.1`, §3.7.4, §3.7.8; K-674). Ürünün kendisinin kalıcı silinmesi bir durum değildir: kayıt katalogdan kalkar, açık siparişler donmuş kalemleriyle sürer ve panel onaydan önce onların sayısını söyler (`02 §3.7.7`). Akış: §8.1.

**1.8.3 Etkileri.** Vitrinden kalkan — arşive ya da taslağa alınan, kalıcı silinen — kalem sepetten çıkar ve müşteriye bir kez söylenir (`02 §3.16.7`; akış §2.3). Yayın durumu siparişi etkilemez; satışın geçici kapatılması yayın durumunu değiştirmez (`02 §5.1`).

**1.8.4 Kurumsal içerik kaydı.** İki değer kullanılır; arşiv, ileri tarihli yayın ve sürüm geçmişi yoktur (`02 §5.2`). Akış: §2.2 (ziyaretçi), §8.6 (yönetici).

| Durum | Anlamı | Sonraki olası durumlar | Tetikleyici |
|---|---|---|---|
| Taslak (`Draft`) | Sitede yok; yönetici "Taslak" bandıyla önizler | Yayında | Yönetici — zorunlu alanlar doluysa (`02 §3.27.13`) |
| Yayında (`Published`) | Sitede görünür; düzenleme kaydedildiği anda yayındadır | Taslak | Yönetici — yayın kapısını bozan düzenleme kaydedilmez (`02 §5.2`) |

Duyuru yayında olsa bile tarihleri girilmişse yalnız aralığında görünür (Z-23). Hakkımızda silinmez, yalnız taslağa alınır; öteki kayıtların silinmesi geri alınamaz uyarısıyla yapılır (`02 §5.2`, §7.6.4).

### 1.9 Talep durumları

**1.9.1 Ayıp talebi.** Kalem başına tek ayıp talebi kaydı vardır (K-676): kalemde talep yokken müşteri "sorun bildir"i kullanır; talep Açık'ken yeni talep açılmaz; talep Çözüldü'yken müşteri ve yönetici onu yeniden açar ve yeni sorun da aynı kayıtta bildirilir. Akış: §2.9 (müşteri), §8.4 (yönetici).

| Durum | Anlamı | Sonraki olası durumlar | Tetikleyici | Bildirim |
|---|---|---|---|---|
| Açık (`Open`) | Müşterinin bildirdiği, firmanın henüz çözmediği talep; talep bu durumda doğar | Çözüldü | Yönetici "çözüldü" işaretler | Doğuşta B-9 → müşteri, F-3 → firma; çözüldü işareti — (`02 §9.2`) |
| Çözüldü (`Resolved`) | Firmanın çözüldü işaretlediği talep | Açık | Müşteri ya da yönetici yeniden açar — kalemin iki yıllık süresi (Z-18) içinde | Müşterinin yeniden açmasında B-9 → müşteri ve F-3 → firma (K-681); yöneticininkinde — (`02 §9.2`) |

Seçimlik hakların yürütümü sistem dışıdır; para gerektiren çözüm §1.3'ün geri ödeme geçişleriyle işler (`02 §7.5.3`).

**1.9.2 İletişim talebi — KVKK talebi dahil.** Ayrı durum makinesi ve süre sayacı yoktur (`02 §5.13`). Akış: §2.2 (ziyaretçi), §8.6 (yönetici).

| Durum | Anlamı | Sonraki olası durumlar | Tetikleyici | Bildirim |
|---|---|---|---|---|
| Açık (`Open`) | Formdan gelen, firmanın kapatmadığı talep; talep bu durumda doğar | Kapatıldı | Yönetici kapatır | Doğuşta F-2 → firma |
| Kapatıldı (`Closed`) | Firmanın kapattığı talep | Açık | Yönetici yeniden açar | — |

Saklama süresi son kapatılıştan, hiç kapatılmamış talepte açılıştan işler (Z-33).

### 1.10 Durum makinesi olmayan varlıklar

Bu varlıkların durumu yoktur; geçerlilikleri zamana ya da kullanıma bağlıdır ve aşağıdaki olaylarla değişir.

| # | Varlık | Geçerliliği | Değiştiren olaylar | Kaynak · akış |
|---|---|---|---|---|
| 1.10.1 | İndirim | Tarih aralığının içinde (Z-20) | Sistem başlangıçta başlatır, bitişte bitirir; onay anında başlaması ya da bitmesi özet farkıdır | `02 §5.11.1` · §8.1 |
| 1.10.2 | Kupon | Tarih aralığının içinde (Z-22) ve kullanım hakkı kaldıkça | Kullanım adedi kullanılmış ve ayrılmış hakların altına indirilemez | `02 §5.11.2`, §3.10.6 · §8.1 |
| 1.10.3 | Kupon hakkı ve stok ayırma | Ayrılır → kesin düşer ya da kullanılmış sayılır → serbest kalır ya da geri döner | §1.4.4'ün anları | `02 §3.17.3`, §5.11.3 |
| 1.10.4 | Duyuru | Yayında ve — tarihleri girilmişse — aralığında (Z-23) | Yöneticinin yayına alması ya da taslağa çekmesi | `02 §5.2` · §8.6 |
| 1.10.5 | Hesap — müşteri ve yönetici | Doğrulanmamış kayıt bir bekleme aşamasıdır: e-posta doğrulanana kadar giriş ve sipariş yoktur; doğrulanmış hesap silinene ya da kaldırılana kadar | Doğrulama; doğrulama bağlantısının ömrünün dolması (Z-1) — kayıt silinir, yeniden istenen bağlantı ömrü uzatmaz (L-9; K-659); e-posta değişikliği yeni adres doğrulanana kadar bekler — bekleyen değişiklik ilk bağlantının ömrüyle düşer (K-659) —, geçerli olduktan sonra eski adresteki bağlantıyla Z-45 boyunca geri alınır; müşteri hesabının silinmesi; yöneticinin kaldırılması. Deneme limiti hesabın durumunu değiştirmez | `02 §5.12`, §3.13, §10.2 · §9, §8.8 |
| 1.10.6 | Oturum | "Beni hatırla" seçimine göre Z-4 ya da Z-5 | Şifre sıfırlama, hesabın silinmesi, e-posta değişikliğinin geri alınması ve yöneticinin kaldırılması hesabın bütün oturumlarını kapatır | `02 §5.12.2` · §9.2 |
| 1.10.7 | Yönetici daveti | Gönderiminden Z-3 boyunca, kullanılana kadar; tek kullanımlık | Kullanılınca yönetici hesabı doğar · geçersizleşir: Z-3 dolunca, geri çekilince, aynı adrese yeni davet gidince, gönderen yönetici kaldırılınca. Gönderim ve geri çekme işlem izine yazılır | `02 §10.2.1`, §10.2.2, §10.2.6 · K-660, K-661, K-662 · §8.8 |
| 1.10.8 | Mağazanın satış açıklığı | Dört koşulun hepsi sağlanırken | Bir koşulun düşmesi ya da geçici kapatma yeni siparişi durdurur; açık siparişleri etkilemez | `02 §3.1.5`, §3.1.6 · K-664 · §8.7 |

### 1.11 Durum × rol × işlem

Her rolün hangi durumda hangi işlemi yapabildiği. Ekran varyantları bu tablodan türetilir; işlemin kuralı "Kaynak"tadır. Bir işlemin açık olduğu aralığın dışında sipariş sayfası ve panel o işlemi **sunmaz**.

**Müşteri — sipariş sayfası** (akış §2.6–§2.9)

| # | İşlem | Açık olduğu aralık | Sonucu | Kaynak |
|---|---|---|---|---|
| 1.11.1 | Siparişi bütünüyle iptal etmek | Alındı + Bekliyor; ödeme onaylanınca kalem düzeyine geçer | S3 + Ö2 · B-7 | `02 §7.2.2` |
| 1.11.2 | Fiziksel kalemi iptal etmek | Ödeme onaylandıktan sonra, sipariş Kargoya verildi'ye geçene kadar | İptal kaydı · kapanış kuralı · geri ödeme Z-17 · B-7 | `02 §7.2.1`, §7.2.8 |
| 1.11.3 | Hizmet kalemini iptal etmek | Ödeme onaylandıktan sonra, kalem "tamamlandı" işaretini alana kadar — siparişin sevkiyat durumundan bağımsız | İptal kaydı · kapanış kuralı · B-7 | `02 §7.2.5` |
| 1.11.4 | Dijital kalemi iptal etmek | Yoktur — kalem ödeme onayında teslim edilir; önce yalnız 1.11.1 | — | `02 §7.2.5` |
| 1.11.5 | Fiziksel kalemden caymak | Sipariş Kargoya verildi'ye geçtikten sonra — Kargoya verildi, Teslim edilemedi, Teslim edildi — teslim tarihinden on dört gün dolana kadar (Z-13); teslim tarihi girilmemişse açık. Mutlak istisnada yoktur, koşullu istisnada koşulla açıktır | Cayma beyanı · teslim işaretsiz kalemde kapanış kuralı · B-9 | `02 §7.3.1`, §7.3.4, §7.3.7 |
| 1.11.6 | Hizmet kaleminden caymak | Ödeme onaylandıktan sonra (K-673), "tamamlandı" işaretine ya da Z-14'ün dolmasına kadar — hangisi önce gelirse | Cayma beyanı · kalem kapanır, kapanış kuralı · B-9 | `02 §7.3.3` |
| 1.11.7 | Dijital kalemden caymak | Yoktur — hak ödeme onayında üçüncü onay kutusuyla düşer | — | `02 §7.3.2` |
| 1.11.8 | Gecikme nedeniyle fesih | Siparişin firmaya ulaşmasından Z-11 geçmiş ve teslim tarihi girilmemiş fiziksel kalemi varken — Hazırlanıyor, Kargoya verildi, Teslim edilemedi | Gecikme feshi kaydı · Hazırlanıyor'da açık kalem kalmazsa S4 kapanışı · B-9 | `02 §7.2.3`, §5.8 |
| 1.11.9 | Ayıp talebi açmak ("sorun bildir") | Fiziksel kalemde sipariş Kargoya verildi'ye geçtikten, dijitalde ödeme onayından, hizmette "tamamlandı" işaretinden sonra; kalemde talep yokken (K-676); teslim işaretinin tarihinden iki yıl (Z-18) — teslim tarihi girilmemişse süre işlemez | Ayıp talebi — Açık · B-9 | `02 §7.5.1`, §7.5.2 |
| 1.11.10 | Ayıp talebini yeniden açmak | Talep Çözüldü'yken, Z-18 içinde | Çözüldü → Açık · B-9 · F-3 | `02 §5.10`, §9.2 · K-681 |
| 1.11.11 | Geri ödeme IBAN'ını girmek | Havale hattında iptal, gecikme feshi ya da cayma beyanı sırasında; IBAN isteğinde ("IBAN bekleniyor"); kart iadesi gerçekleşmeyip yönetici havale yolunu açtığında — müşterinin seçimi | IBAN isteği kalkar | `02 §7.4.5`, §7.2.8 |
| 1.11.12 | Girilmiş IBAN'ı düzeltmek | IBAN girildikten sonra, yönetici geri ödemeyi işleyene kadar | — | `02 §7.4.5` |
| 1.11.13 | Havale bilgisini görmek | Havale siparişinde Alındı + Bekliyor — siparişe donmuş IBAN | — | `02 §3.21.5`, §3.22.4 · K-663 |
| 1.11.14 | Dijital dosyayı indirmek | Ödeme onaylandıktan sonra, kalemin indirme hakkı (P-8) kaldıkça | İndirme sayacı | `02 §3.12.5` |

**Yönetici — panel** (akış §8.2–§8.4)

| # | İşlem | Açık olduğu aralık | Sonucu | Kaynak |
|---|---|---|---|---|
| 1.11.15 | Havale "ödendi" işareti | Havale siparişinde Alındı + Bekliyor; iptalden sonra yoktur | Ö1 ve eşzamanlı geçişler · geri alınamaz · B-4 | `02 §3.21`, §5.9 |
| 1.11.16 | Kargoya vermek | Hazırlanıyor, açık fiziksel kalem varken | S5 · geri alınamaz · B-5 | `02 §5.4` |
| 1.11.17 | Teslim işaretini ve tarihini koymak | Kargoya verildi | S6 · — | `02 §3.20.11` |
| 1.11.18 | "Teslim edilemedi" işaretlemek | Kargoya verildi | S7 · B-6 | `02 §5.4` |
| 1.11.19 | Yeniden göndermek | Teslim edilemedi, açık fiziksel kalem varken | S8 · geri alınamaz (K-677) · B-5 | `02 §5.4` |
| 1.11.20 | Hizmet kalemini "tamamlandı" işaretlemek | Ödeme onaylanmışken — Bekliyor ve Başarısız dışında (K-671) —, kalem açıkken | Teslim işareti · kapanış kuralı | `02 §3.20.9` |
| 1.11.21 | Siparişi ya da kalemi iptal etmek (firma iptali) | Ödeme beklenirken yalnız siparişin tamamı (S3); ödemeden sonra kalem düzeyinde, kalem tipinin iptal sınırı içinde (1.11.2–1.11.4); Teslim edilemedi'de S9. Cayma beyanlı ve feshedilmiş kaleme uygulanmaz | İptal kaydı, sebep · geri alınamaz · B-7 | `02 §7.2.4`, §5.4 |
| 1.11.22 | Kalem çıkarmak ya da adedini azaltmak | Ödeme onaylandıktan sonra; fiziksel kalemde Kargoya verildi'ye kadar, dijital ve hizmet kaleminde teslim işaretine kadar | Çıkarma kaydı · kart hattında geri alınamaz · B-11 | `02 §10.4.3` |
| 1.11.23 | Adresi düzeltmek | Sipariş Kargoya verildi'ye geçene kadar; fiziksel kalemsiz siparişte fatura adresi Teslim edildi'ye kadar | B-10 | `02 §10.4.2` |
| 1.11.24 | Kargo şirketini ve takip numarasını düzeltmek | Sipariş kargoya verildikten sonra | B-12 | `02 §10.4.4` |
| 1.11.25 | Teslim tarihini düzeltmek | Teslim işaretinden sonra | Pencereler yeni tarihten işler · — | `02 §10.4.5` |
| 1.11.26 | Durum geçişini düzeltmek | §1.6.2'nin satırları | B-13 | `02 §10.4.6` |
| 1.11.27 | İç not yazmak | Her durumda | — | `02 §10.4.7` |
| 1.11.28 | İade malını teslim almak | Cayma beyanlı ya da feshedilmiş fiziksel kalemin malı ulaşınca — "mal dönmedi" ile kapatılmış kalemde de (kalemi yeniden açar) | İade teslim alma · IBAN silinmişse IBAN isteği ve B-14 | `02 §7.4.7`, §10.4.9 |
| 1.11.29 | İade malını reddetmek | Koşullu istisna kaleminde, teslim alma adımında | İade reddi · geri alınamaz · B-16 | `02 §7.4.9` |
| 1.11.30 | "Mal dönmedi" kapanışı | Teslimden sonraki caymada Z-42 geçmiş ve mal ulaşmamışken | Kapanış · — | `02 §10.4.9` |
| 1.11.31 | Geri ödemeyi işlemek | Havale hattında IBAN varken; caymada iki hatta; ayıp talebinin para gerektiren çözümünde; tutar bazlı kısmi geri ödemede ödenmiş ve geri ödenmemiş tutar kaldıkça | Ö3–Ö5 · geri alınamaz · B-8 | `02 §5.5`, §7.4 |
| 1.11.32 | Kart iadesini yeniden denemek ya da havale yolunu açmak | "geri ödeme gerçekleşmedi" işaretliyken; müşteri IBAN girdikten sonra kart iadesi yeniden denenmez | İşaret kalkar ya da IBAN isteği · B-14 | `02 §7.2.8` |
| 1.11.33 | "Havale gerçekleşmedi" bildirmek | Geri ödeme havalesi bankada gerçekleşmediğinde, geri ödeme işlenmeden önce | IBAN silinir, IBAN isteği · B-14 | `02 §7.4.5` |
| 1.11.34 | Başka kanaldan gelen caymayı kaydetmek | Fiziksel kalemde sipariş Kargoya verildi'ye geçtikten sonra — önce "müşteriyle anlaşıldı" iptaliyle; hizmet kaleminde ödeme onaylandıktan sonra — önce siparişin bütünüyle iptaliyle (K-673). Pencere dışında ve mutlak istisnada ayrıca onayla; pencerenin bitiminden bir yıl sonrasına tarihli kayıt reddedilir | Cayma beyanı · B-9 · IBAN'sızsa B-14 | `02 §10.4.10` |
| 1.11.35 | Misafir siparişinin e-postasını düzeltmek | Giriş yapılmadan verilmiş siparişte, kişisel veriler imha edilene kadar | B-1 → yeni adres · B-15 → eski adres | `02 §10.4.11` |
| 1.11.36 | E-postayı yeniden göndermek | "e-posta ulaşmadı" işaretli satırda | İşaret kalkar (§1.7.3) | `02 §9.1.6` |
| 1.11.37 | Ayıp talebini "çözüldü" işaretlemek ya da yeniden açmak | Açık ya da Çözüldü talepte | — | `02 §5.10` |
| 1.11.38 | İndirme hakkını yenilemek | Dijital kalemde | Sayaç sıfırlanır | `02 §3.12.6` |

## 2. Ana akışlar (aktör bazlı)

> **Ne yazılır:** Adım adım. Her adımda: kullanıcı ne yapar · sistem ne kontrol eder · durum ne olur · kime bildirim gider.

Bu bölüm müşteri tarafının akışlarını yazar: satın alma (akış 1), sipariş takibi (akış 2), iptal, cayma ve iade (akış 3) ve kurumsal içeriğin vitrin yüzü (akış 7) — `02 §2.1` (K-650). Üyelik (akış 4) §9'da, firma tarafının akışları §8'dedir. Fiziksel siparişin uçtan uca ana akışı (akış 1 + 6) ve if/then'e sığmayan senaryolar §2.10'dadır (K-651).

- **Biçim:** adım tablosu §0.4.1'dir. Her adımın durumu §1'e uyar (§0.4.4); bir işlemin açık olduğu aralık §1.11'dedir ve burada tekrarlanmaz.
- **Aktör:** §2.1 ve §2.2'nin adımları giriş gerektirmez; aktörleri Ziyaretçi'dir ve adımlar müşteriye de aynen işler. §2.3–§2.9'un aktörü Müşteri'dir; üyenin farkı — hesaptaki sepet, adres defteri, sipariş geçmişi, ön dolu form — satır içinde yazılır (K-650). Yöneticinin müşteri akışını ilerleten adımları burada tek satırla anılır; akışları §8'dedir.
- **Dallar:** her alt bölüm olağan yolu ve müşterinin önüne çıkan dalları yazar; hataların tamamı §3'te bu bölümün satır numaralarına bağlanır (§0.4.2). Zaman aşımları §4'te, bildirimlerin özeti §7'dedir.

### 2.1 Ziyaretçi: vitrinde gezinme ve ürünü bulma

Akış 1'in başıdır (`02 §2.2` adım 1; K-221). Satış kapısı kapalıyken vitrin ve ürünler görünür kalır; yalnız sepete ekleme ve ödeme kapanır (2.1.9).

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 2.1.1 | Ziyaretçi ana sayfayı açar | Firmanın seçtiği düzeni gösterir — tanıtım öncelikli ya da mağaza öncelikli; iki düzende de kurumsal tanıtım ve ürün vitrini birlikte durur (kurumsal yüzü §2.2). Duyuru şeridi her sayfanın en üstündedir; tarihleri girilmişse yalnız aralığında görünür (Z-23) | — | — | `02 §3.28.1`, §3.27.21 |
| 2.1.2 | Ziyaretçi menüden bir kategoriye girer | Menü sabit iskeletten kurulur; yayında ürünü olmayan kategori — alt dallarında da yoksa — menüde görünmez. Kategori sayfası kendisine asılı ürünleri ve bütün alt dallarındakileri tek listede, en yeni önce gösterir; ziyaretçiye sıralama seçeneği sunulmaz. Kırıntı yolu ürünün ana kategorisinden üretilir. Boş kategorinin adresi boş kategori sayfası döner | — | — | `02 §3.4.3`, §3.4.4, §3.5.3, §3.28.4, §3.28.6 |
| 2.1.3 | Ziyaretçi ürün adıyla arar | Yalnız ürün adında, büyük-küçük harfe ve Türkçe karaktere duyarsız arar; açıklama, kategori adı ve seçenek değerleri aranmaz. Sonuç kategori sayfasının sabit düzeniyle gelir | — | — | `02 §3.5.1`, §3.5.3 |
| 2.1.4 | Ziyaretçi listeyi süzer | İki sabit eksen vardır: fiyat aralığı ve stok durumu; stok süzgecinin varsayılanı tükenmiş ürünleri de gösterir. Seçenek değerine göre süzme yoktur | — | — | `02 §3.5.2` |
| 2.1.5 | Ziyaretçi ürün kartından ürün sayfasını açar | Ürün başına tek kart vardır. Sayfa görselleri, açıklamayı, KDV dahil fiyatı ve fiyatın uygulanmaya başladığı tarihi, fiziksel üründe üretim yerini — Türkiye'yse yerli üretim logosunu —, doluysa birim fiyatı, indirimdeyse referans fiyatı ve indirimin tarihlerini, fiziksel üründe kargoya verme süresini, hizmette ifa süresini gösterir. Stok adedi hiçbir yerde gösterilmez. Yorum, puan ve öneri bloğu yoktur; sayfa "Paylaş" düğmesini taşır | — | — | `02 §3.2.2`, §3.3.1, §3.3.6, §3.6.5, §3.8.5, §3.9.2, §3.20.4, §3.30.7 |
| 2.1.6 | Ziyaretçi varyantı seçer — en fazla iki seçenek boyutu | Tükenmiş varyant "Tükendi" işaretiyle görünür ve seçilemez; firmanın açmadığı kombinasyon da görünür ve seçilemez; taslak varyant görünmez. Satın alınabilirlik ayrılmış adetler düşülerek hesaplanır: ödemesi beklenen bir siparişin ayırdığı son parça başka müşteriye "Tükendi" görünür ve o ödeme gerçekleşmezse yeniden satın alınabilir olur | — | — | `02 §3.3.2`, §3.3.3, §3.6.3, §3.7.5 |
| 2.1.7 | Ziyaretçi bütün varyantları tükenmiş bir ürüne gelir | Ürün vitrinde "Tükendi" işaretiyle kalır ve sepete eklenemez. "Stokta haber ver" ve istek listesi yoktur | — | — | `02 §3.6.4`, §3.16.13, §9.5 |
| 2.1.8 | Ziyaretçi varyantı adediyle sepete ekler | Sepet akışına geçer (§2.3) | — | — | `02 §2.2` adım 2 |
| 2.1.9 | Ziyaretçi satış kapalıyken gezer | Kapının dört koşulundan biri sağlanmıyorsa ya da firma satışı geçici olarak kapattıysa ürünler ve kurumsal içerik görünür; sepete ekleme ve ödeme kapalıdır ve ziyaretçi satışın kapalı olduğunu görür. Var olan sepet korunur ve içeriği görünür. Bakım modu yoktur | — | — | `02 §3.1.5`, §3.1.6 |
| 2.1.10 | Ziyaretçi arşivlenmiş bir ürünün adresine gelir — paylaşılmış bir bağlantıyla | "Bu ürün artık satılmıyor" sayfası döner: ürün adı, görseli ve durum bilgisi; fiyat ve sepete ekleme yoktur. Arşivlenmiş ürün listelerde, kategori sayfalarında ve aramada görünmez | — | — | `02 §3.7.6` |
| 2.1.11 | Ziyaretçi taslak ya da kalıcı silinmiş bir ürünün adresine gelir | "Sayfa bulunamadı" sayfası döner — sitenin görünümünde, ana sayfaya ve ürünlere dönüş yoluyla. Giriş yapmış yöneticiye taslak ürün "Taslak" bandıyla görünür (§8.1) | — | — | `02 §3.7.5`, §3.7.7, §3.30.4 |

### 2.2 Ziyaretçi: kurumsal içerik ve iletişim formu

Akış 7'nin vitrin yüzü ve üçüncü kolu — iletişim talebi (`02 §2.3`; K-285, K-468). Kurumsal taraf satış kapısına bağlı değildir. Yayın durumları §1.8.4'te, iletişim talebinin durumu §1.9.2'de, yönetimi §8.6'dadır.

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 2.2.1 | Ziyaretçi ana sayfanın kurumsal bloklarını görür | Kurumsal blok her zaman Hakkımızda'dır: kısa tanıtımı ve ana görseli. Hakkımızda boşken ya da yayında değilken blok marka adını ve logoyu gösterir; boş kutu görünmez. "Ana sayfada göster" işaretli hizmet tanıtımları ve referans işler kendi bloklarında, elle sıralarıyla görünür; işaretli kayıt yoksa blok hiç görünmez | — | — | `02 §3.27.14`, §3.28.2, §3.28.3 |
| 2.2.2 | Ziyaretçi menüden bir içerik tipine girer | Üst menü ürünlerin yanında içerik tiplerini — Hizmetlerimiz, Referanslarımız, Hakkımızda, SSS — ve İletişim'i taşır; yayında kaydı olmayan tip menüde görünmez. Genel sayfalar altbilgide, "menüde göster" işaretliyse üst menüde de durur. Liste sayfaları firmanın elle sırasını izler | — | — | `02 §3.27.11`, §3.28.4, §3.28.5 |
| 2.2.3 | Ziyaretçi bir içerik sayfasını okur | Hakkımızda, hizmet tanıtımı, referans iş ve genel sayfa kendi sayfasındadır; sık sorulan sorular tek sayfada, başlıksız listelenir. Hizmet tanıtımı fiyatsızdır ve satın alınmaz; sayfası "Bize ulaşın" düğmesini taşır (2.2.7). Video kendi platformunda açılan bir bağlantıdır; gömülü video, dosya eki ve blog yoktur | — | — | `02 §3.27.2`–§3.27.6, §3.27.9, §3.27.17, §3.27.18, §3.27.20 |
| 2.2.4 | Ziyaretçi hizmet tanıtımında ya da referans işte "İlgili ürünler"den bir ürüne geçer | Bağlı ürünlerden yalnız yayındakiler vitrin kartı olarak görünür; kart ürün sayfasını açar (2.1.5). Ürün sayfasında ters yön — "bu ürünün geçtiği referanslar" — yoktur | — | — | `02 §3.27.10` |
| 2.2.5 | Ziyaretçi İletişim sayfasını açar | Firma kimliğinin iletişim bilgileri ve yasal kimlik bilgileri — KEP adresi dahil, "İletişim" başlığı altında —, şubeler ve iletişim formu bir aradadır; firma ayrıca metin girmez. Şube "Haritada aç" bağlantısı taşır, gömülü harita yoktur | — | — | `02 §3.1.4`, §3.27.7, §3.27.8 |
| 2.2.6 | Ziyaretçi her sayfanın üst ve alt bölümünü kullanır | Sosyal medya bağlantıları ve WhatsApp bağlantısı üst ve alt bölümdedir; canlı destek ve sohbet penceresi yoktur. Altbilgi aydınlatma metninin, çerez politikasının ve "İşlem rehberi"nin bağlantılarını, genel sayfaları ve — firma kapatmadıysa — platform imzasını taşır. ETBİS doğrulama bilgisi girilmişse doğrulama bandı her sayfada görünür. Çerez onay bandı yoktur | — | — | `02 §3.1.4`, §3.24.8, §3.27.12, §3.29.6, §3.33.5, §8.4.4 |
| 2.2.7 | Ziyaretçi ya da müşteri iletişim formunu açar — İletişim sayfasından ya da bir hizmet tanıtımının "Bize ulaşın" düğmesinden | Form girişsiz açıktır ve aydınlatma metninin bağlantısını gösterir. Aydınlatma metni tamamlanıp yayına alınmadıysa form kapalıdır; kurumsal sayfalar yayında kalır. Üye girişliyse ad ve e-posta ön dolu ve düzenlemeye açık gelir | — | — | `02 §3.32.1`, §3.32.8, §3.33.5 |
| 2.2.8 | Ziyaretçi ad, e-posta, konu tipi ve mesajı — isteğe bağlı telefonu — yazar ve gönderir | Konu tipi kapalı beş değerden seçilir: Genel soru · Sipariş hakkında · Ürün hakkında · KVKK talebi · Diğer. Fiyat sorusu, indirme hakkının yenilenmesi ve siparişe dair özel istek de bu formdan gelir; ayıp talebi listede yoktur — sipariş sayfasından gider (§2.9). Dosya eki yoktur; sipariş numarası mesaja yazılır. Görünmez tuzak alan doluysa gönderim hata vermeden sessizce düşer; gönderim IP başına L-4 ile limitlidir. Geçen gönderim bir iletişim talebi kaydı açar | İletişim talebi: Açık | F-2 → firma; gönderene alındı e-postası gitmez (K-678) | `02 §3.2.1`, §3.12.6, §3.32.2, §3.32.3, §3.32.7, §3.32.10, §5.13, §9.5 · K-678 |
| 2.2.9 | Ziyaretçi cevabı bekler | Talep yalnız firmanın panelinde yaşar: gönderene numara, "taleplerim" sayfası ya da durum sorgusu verilmez. Firma cevabı sistemin dışında e-postayla verir ve talebi kapatır (§8.6) | Firma kapatınca İletişim talebi: Kapatıldı | — | `02 §3.32.5`, §3.32.6, §5.13 |
| 2.2.10 | Ziyaretçi taslak ya da silinmiş bir içeriğin adresine gelir | "Sayfa bulunamadı" sayfası döner; taslak kayıt menüde, ana sayfada ve listelerde görünmez | — | — | `02 §3.27.25`, §3.27.27, §3.30.4 |

### 2.3 Müşteri: sepet

Sepet kendiliğinden boşalmaz (Z-6); sepet hatırlatması ve istek listesi yoktur (`02 §3.16.13`).

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 2.3.1 | Müşteri varyantı adediyle sepete ekler | Satış kapalıysa eklenmez (2.1.9). Adet stoğu — ayrılmış adetler düşülerek — aşarsa eklenmez ve "Bu adette stok yok" yazar; adet söylenmez. Ürünün "bir siparişte en fazla" sınırı (P-9) bütün varyantlarının toplamına uygulanır; aşılırsa eklenmez ve sınır sayısıyla söylenir — hem sınır hem stok aşılıyorsa sınır söylenir. Dijital üründe adet 1'dir ve aynı dijital varyant ikinci kez eklenmez ("Bu ürün zaten sepetinde"). Üye daha önce aldığı dijital varyantı eklerse "Bu ürünü daha önce aldınız" uyarısını görür ve isterse yine de alır; misafirde uyarı yoktur. Sepete eklemek stok ayırmaz; kalem varyantın ilk eklendiği andaki birim fiyatı hatırlar | — | — | `02 §3.12.4`, §3.12.9, §3.16.5, §3.16.8, §3.16.9, §3.17.3 |
| 2.3.2 | Müşteri sepeti açar | Üyenin sepeti hesabındadır ve her cihazda aynıdır; misafirinki tarayıcıya bağlıdır. Kalem fiyatını ve satın alınabilirliğini güncel üründen okur; güncel fiyat ilk eklenme anındakinden farklıysa satırda "Sepete eklediğinden beri fiyatı değişti" yazar — eski fiyat ve değişimin yönü gösterilmez. Sepette fiziksel kalem varsa: firma eşik tanımladıysa ücretsiz kargoya kalan tutar ya da "Kargo ücretsiz" yazar; asgari sipariş tutarı tanımlıysa ve sepet altındaysa eksik tutar yazar. Teslimat il kısıtı varsa adres adımından önce görünür. Taksit bilgisi gösterilmez | — | — | `02 §3.16.1`, §3.16.5, §3.18.1, §3.18.3, §3.19.5, §3.20.1, §3.21.9 |
| 2.3.3 | Müşteri adedi değiştirir ya da kalemi çıkarır | Adet artırma 2.3.1'in denetiminden geçer | — | — | `02 §3.16.8` |
| 2.3.4 | Sistem sepetteki bir kalemi satın alınamaz bulur — kalem tükenmiş, kontenjanı dolmuş, stok sepetteki adedin altına düşmüş ya da firma ürünün sınırını düşürmüş | Kalem sepette kalır ve siparişe girmez: tükenende "Tükendi", stoğun altındakinde "Bu adette stok yok", sınırı aşanda sınır mesajı yazar; adet kendiliğinden düşürülmez. Stok geri gelince ya da müşteri adedi düşürünce kalem yeniden siparişe girer. Siparişe girmeyen kalem tutara, kuponun asgari tutarına, kargo hesabına ve eşiklere girmez | — | — | `02 §3.16.6`–§3.16.8, §3.16.10 |
| 2.3.5 | Sistem vitrinden kalkan kalemi bulur — ürün ya da varyant arşive ya da taslağa alınmış veya kalıcı silinmiş | Kalem sepetten çıkar ve müşteriye bir kez söylenir (§1.8.3) | — | — | `02 §3.16.7` |
| 2.3.6 | Misafir alıcı sepetle giriş yapar | İki sepet tek sepete iner: aynı varyantta adetler toplanmaz, büyük olan kalır; farklı varyantlar yan yana durur. Birleşme müşterinin o ziyarette görmediği bir ürünü ya da adedi eklediyse bu bir kez, ürünler adıyla söylenir. Birleşen sepet 2.3.1'in adet, stok ve sınır kurallarından geçer; geçersiz kupon sayacı (L-6) iki sepetin büyüğünü taşır | — | — | `02 §3.16.3`, §3.16.4, §8.2 L-6 |
| 2.3.7 | Üye çıkış yapar | Sepet hesapla gider; o tarayıcıda sepet boş görünür | — | — | `02 §3.16.1`, §3.16.2 |
| 2.3.8 | Müşteri ödeme adımına geçer | Siparişe girebilecek en az bir kalem olmalıdır; yoksa ödeme adımı açılmaz ve müşteri sebebini sepette görür. Satış kapalıysa geçilemez. Sepette fiziksel kalem varsa asgari sipariş tutarı aranır — taban siparişe giren kalemlerin kupondan önceki, indirimli toplamıdır. Geçiş §2.4'e | — | — | `02 §3.1.6`, §3.16.7, §3.18.1–§3.18.3 |

### 2.4 Müşteri: ödeme adımı ve sipariş onayı

Ödeme adımı misafir alıcıya ve üyeye aynı adımlarla açılır; giriş duvarı yoktur (`02 §3.13.1`, §3.13.3). Sipariş onay anında doğar (`02 §5.3.2`).

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 2.4.1 | Müşteri ödeme adımını açar | Üye girişliyse siparişin iletişim e-postası hesabın doğrulanmış e-postasıdır. Misafir alıcı e-postasını yazar; adres doğrulama koduyla sınanmaz ve ikinci kez yazdırılmaz. Yazılan e-posta kayıtlı bir müşteri hesabınınsa "bu e-posta kayıtlı, giriş yaparsanız adresleriniz dolu gelir" hatırlatması görünür; müşteri giriş yaparsa sepetler 2.3.6'ya göre birleşir | — | — | `02 §3.13.3`, §3.24.5, §10.4.11 |
| 2.4.2 | Müşteri teslimat adresini girer — sepette fiziksel kalem varsa | Üye adres defterinden seçer ya da yeni bir adres yazar; yeni adresi isterse aynı adımda deftere kaydeder (K-680). Misafir alıcı adresini her siparişte yazar. Alanlar: alıcının adı ve soyadı, il — 81 ilden —, ilçe, açık adres ve zorunlu telefon; posta kodu ve T.C. kimlik numarası istenmez. Telefonu olmayan defter kaydı seçilirse telefon burada istenir. Firmanın teslimat yapmadığı ildeki adres kabul edilmez ve sebebi söylenir; defterdeki o adres seçilemez. Sepette fiziksel kalem yoksa teslimat adresi alanları gösterilmez ve il denetimi yapılmaz | — | — | `02 §3.14.1`, §3.14.4, §3.14.6, §3.20.1 · K-680 |
| 2.4.3 | Müşteri fatura adresini girer | Fatura adresi varsayılan olarak teslimat adresiyle aynıdır; "fatura adresim farklı" ile ayrı adres seçilir ya da yazılır. Fiziksel kalemsiz siparişte de istenir. Telefon taşımaz; kurumsal fatura alanı yoktur | — | — | `02 §3.14.2`, §3.14.4, §3.14.6, §3.25.1 |
| 2.4.4 | Müşteri kupon kodu girer — isteğe bağlı | Siparişe en fazla bir kod uygulanır; müşteri kodu değiştirir ya da kaldırır (K-679). Kod tarih aralığındaysa (Z-22), kullanım hakkı — kullanılmış ve ayrılmış haklar düşülerek — kalmışsa, sabit tutarlı kuponda asgari sepet tutarı karşılanmışsa ve kupon toplamı sıfıra indirmiyorsa kabul edilir; payı kalemlere dağıtılır ve özet güncellenir. Kupon ücretsiz kargo eşiğinin ve asgari sipariş tutarının tabanını düşürmez. Art arda geçersiz kod denemesi L-6 ile limitlidir | — | — | `02 §3.10`, §3.18.2, §3.19.4, §8.2 L-6 · K-679 |
| 2.4.5 | Müşteri ödeme yöntemini seçer | Kart, kurulumda sağlayıcı anahtarları tanımlıysa görünür; sağlayıcıya erişilemiyorsa "şu an kullanılamıyor" olarak görünür ve seçilemez. Havale/EFT, firma açtıysa görünür; aynı IP ya da — giriş yapmış üyede — aynı e-posta için açık ödenmemiş sipariş tavanı (L-8) doluysa seçilemez, mesaj nötrdür ve kart yolu açık kalır. Kapıda ödeme yoktur; taksit yalnız sağlayıcının kart ekranındadır | — | — | `02 §3.21.2`–§3.21.5, §3.21.9, §6.2.15, §8.2 L-8 |
| 2.4.6 | Müşteri onay özetini okur | Özet siparişe girecek kalemleri, tutarın dökümünü — KDV, indirim, kuponun payı, kargo ücreti ya da ücretsiz kargo — ve ödenecek toplamı gösterir. Misafir alıcıda siparişin gideceği e-posta adresi açıkça yazar ("Siparişiniz şu adrese gönderilecek: …") ve onaydan önce düzeltmeye açıktır. Ön Bilgilendirme Formu özetle birebir aynıdır ve üzerine firmanın kimliğini, teslimat il kısıtını, kargoya verme süresini, cayma hakkını — istisna işaretli kalemde istisnayı, sebebini ve koşulunu —, iade kargo bedelinin firmada olduğunu, iade adresini, hizmette ifa süresini ve uyuşmazlık yollarını ekler. Sabit "sipariş nasıl kurulur" metni ve aydınlatma metninin bağlantısı görünür | — | — | `02 §3.17.2`, §3.24.3, §3.24.5, §3.33.5 |
| 2.4.7 | Müşteri onay kutularını işaretler | Her siparişte iki kutu: Ön Bilgilendirme Formu'nun okunduğu ve Mesafeli Satış Sözleşmesi'nin onayı. Sepette dijital kalem varsa üçüncü kutu — indirmenin ödeme onaylandığında açılacağı ve cayma hakkının düşeceği; hizmet kalemi varsa bir kutu daha — ifanın cayma süresi dolmadan başlaması ve ifa tamamlanınca hakkın düşmesi. Kutular işaretlenmeden sipariş onaylanamaz. KVKK rıza kutusu, yaş beyanı ve sipariş notu alanı yoktur | — | — | `02 §3.24.1`, §3.24.2, §3.24.4, §3.24.7 |
| 2.4.8 | Müşteri "Siparişi onayla — ödeme yükümlülüğü doğar" düğmesine basar | Kullanıcı L-7'nin eşiğindeyse yeni sipariş onaylanamaz; mesaj nötrdür. Sepet onay anında yeniden değerlendirilir: ödenecek tutarı, dökümü ya da siparişe girecek kalemleri değiştiren her fark — fiyat, indirimin başlaması ya da bitmesi, kuponun geçersizleşmesi, ayrılamayan kalem, kargo ücreti, eşik, teslimat illeri — ve Ön Bilgilendirme Formu'nun ya da sözleşmenin sürüm artışı siparişin oluşmasını durdurur. Müşteri neyin değiştiğini söyleyen güncel özeti görür ve yeniden onaylar — metin değiştiyse kutuları yeniden işaretler; ayrılamayan kalem için sayı söylenmez. Siparişe girebilecek kalem kalmadıysa sipariş oluşmaz ve müşteri sepete döner | — · fark varsa sipariş doğmaz | — | `02 §3.16.6`, §3.16.7, §3.17.2, §3.24.1, §5.3.3, §8.2 L-7 |
| 2.4.9 | Sistem siparişi oluşturur — özet değişmemişse | Tahmin edilemez sipariş numarası verilir. Kalemler — tutarları, kupon payları, ürün tipi, cayma istisnası ve ifa süresi dahil —, adresler, iletişim e-postası, firmanın yasal kimliği, onaylanan metin sürümleri, onay kutularının kaydı, havalede firmanın IBAN'ı ve kargoya verme sözü donar. Stok, hizmet kontenjanı ve kupon hakkı ayrılır; ödeme süresi işlemeye başlar (Z-7, Z-8). Aynı sepete bağlı ödenmemiş önceki sipariş kendiliğinden iptal edilir ve ayırması serbest kalır. Giriş yapılmadan verilen sipariş, e-postası doğrulanmış bir müşteri hesabınınsa o hesaba anında düşer. Kartta müşteri sağlayıcının sayfasına geçer (§2.5.1), havalede IBAN'ı görür (§2.5.2) | Alındı + Bekliyor · önceki sipariş: → İptal edildi + Başarısız (S3, Ö2) | B-1 → müşteri — iki yasal metin gövdede · havalede B-2 → müşteri · F-1 → firma · önceki siparişte B-7 → müşteri (§1.2 S3) | `02 §3.13.3`, §3.17.1, §3.17.3, §3.17.7, §3.22.1, §3.23, §3.33.6, §5.3.2, §9.2, §9.3 · K-663 |

### 2.5 Müşteri: ödeme ve teslim

Ödemenin onaylandığı an iki ekseni ve kalemleri birlikte değiştirir (§1.4.2.1); tipe göre hatlar §1.5'tedir. Yöneticinin teslim adımları §8.2'dedir.

#### 2.5.1 Kart ödemesi

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 2.5.1.1 | Müşteri sağlayıcının sayfasında ya da çerçevesinde kart bilgisini girer | Kart bilgisi sağlayıcıda girilir; ürün kart verisini görmez, saklamaz, loglamaz. Her ödeme istisnasız 3D Secure'dan geçer. Taksit sağlayıcının ekranındadır; sipariş tek tutar taşır | Alındı + Bekliyor | — | `02 §3.21.1`, §3.21.3, §3.21.9 |
| 2.5.1.2 | Müşteri 3D Secure'ı tamamlar ve siteye döner | Ödeme sağlayıcının başarı bildirimiyle onaylanır; sonuçları §2.5.3'tedir | → Ödendi (Ö1) | B-4 → müşteri | `02 §3.21.3`, §5.5 |
| 2.5.1.3 | Doğrulama başarısız olur ya da kart reddedilir | Ödeme gerçekleşmez; sipariş kendiliğinden iptal edilir ve ayrılanlar aynı anda serbest kalır. Sipariş müşterinin listesinde görünmez, panelde görünür ve L-7'ye sayılır. Sepet olduğu gibi durur; müşteri sepetten yeni bir siparişle yeniden dener (2.4.8). Ayrıntı §3.2 | → İptal edildi + Başarısız (S3, Ö2) | B-7 → müşteri (§1.2 S3) | `02 §3.16.12`, §3.17.5, §3.17.6, §3.17.8, §6.2.1 |
| 2.5.1.4 | Müşteri banka ekranını kapatır ya da ödeme yarıda kalır | Sepet olduğu gibi durur. Kart ödeme süresi (Z-7) dolunca, iptalden önce sağlayıcıya sonuç son bir kez sorulur: ödeme alınmışsa onaylanır; alınmamışsa sipariş kendiliğinden iptal edilir; sorgu yanıtsızsa sipariş iptal edilmez ve panelde "ödeme sonucu alınamadı" işaretiyle bekler. Müşteri o arada aynı sepetten yeni sipariş onaylarsa önceki sipariş 2.4.9'a göre iptal edilir. Ayrıntı §3.2, §4 | → Ödendi (Ö1) ya da → İptal edildi + Başarısız (S3, Ö2) ya da Alındı + Bekliyor | Ö1'de B-4 · iptalde B-7 → müşteri | `02 §5.6.3`, §6.1.2, §6.2.2 |

#### 2.5.2 Havale/EFT ödemesi

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 2.5.2.1 | Müşteri siparişi havaleyle onaylar | Ekranda firmanın siparişe donan IBAN'ı ve sipariş numarası görünür; aynı bilgi B-2'de ve sipariş sayfasındadır. Havale ödeme süresi (Z-8) sipariş onayından işler | Alındı + Bekliyor | B-2 → müşteri (2.4.9'da) | `02 §3.21.6`, §3.22.4 · K-663 |
| 2.5.2.2 | Müşteri bankasından havale yapar | Sistem dışıdır: ürün gelen tutarı bilmez ve karşılaştırmaz; eksik ya da fazla gelen havalede kararı firma ürünün dışında verir | — | — | `02 §3.21.7` |
| 2.5.2.3 | Sistem hatırlatma gönderir | Ödeme süresinin son iş gününün başında, bir kez (Z-9) | — | B-3 → müşteri | `02 §3.21.6` |
| 2.5.2.4 | Yönetici parayı hesabında görür ve "ödendi" işaretler — panel siparişin donmuş IBAN'ını gösterir, onay geri alınamaz (§8.2) | Ödeme onaylanır; sonuçları §2.5.3'tedir | → Ödendi (Ö1) | B-4 → müşteri | `02 §3.21.5`, §3.21.6, §5.9 · K-663 |
| 2.5.2.5 | Sistem süre dolduğunda ödemeyi işaretlenmemiş bulur | Sipariş kendiliğinden iptal edilir ve ayrılanlar serbest kalır; iptal edilmiş sipariş sonradan "ödendi" yapılamaz — gelen parayı firma geri öder ya da müşteriden yeni sipariş ister. Süre sitenin kesintisinde dolduysa iptal ertelenir (§4; K-667) | → İptal edildi + Başarısız (S3, Ö2) | B-7 → müşteri | `02 §3.17.5`, §3.21.8, §4.2 Z-8 · K-667 |
| 2.5.2.6 | Müşteri ödeme beklenirken vazgeçer ya da kartla ödemek ister | Siparişi bütünüyle iptal eder (2.7.1); kalem düzeyinde iptal yoktur. Kartla ödemek isteyen müşteri aynı sepetten yeni sipariş verir; önceki sipariş kendiliğinden iptal edilir (2.4.9) | — | — | `02 §3.17.7`, §7.2.2 |

#### 2.5.3 Ödeme onayı ve teslim

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 2.5.3.1 | Sistem ödeme onayını işler | Ayrılan stok ve kontenjan kesin düşer, kupon hakkı kullanılmış sayılır; siparişe giren kalemler sepetten çıkar — ödeme sürerken sepete eklenen kalem kalır. Fiziksel kalemli siparişte kargoya verme süresi (Z-10), hizmet kaleminde ifa süresi (Z-39) başlar. Dijital kalemler teslim edilir | Fiziksel kalem varsa → Hazırlanıyor + Ödendi (S1) · yalnız dijitalse → Teslim edildi + Ödendi (S2) · öteki hâllerde Alındı + Ödendi · dijital kalem: teslim işareti | B-4 → müşteri — dijital kalem varsa indirmenin hazır olduğu | `02 §3.16.12`, §3.20.6, §4.3, §5.6, §5.7 |
| 2.5.3.2 | Müşteri dijital dosyayı sipariş sayfasından indirir | Kalemin indirme hakkı (P-8) kaldıkça indirir; süre sınırı yoktur. Firma dosyayı güncellediyse yeni hâli iner ve o da haktan düşer. Hakkı dolan müşteri iletişim formundan "Sipariş hakkında" tipiyle başvurur ve yönetici hakkı yeniler (§1.11.38); sipariş sayfasında "hakkımı yenile" düğmesi yoktur | kalem: indirme sayacı | — | `02 §3.12.5`–§3.12.8, §3.32.10 |
| 2.5.3.3 | Yönetici siparişi kargoya verir — kargo şirketi ve takip numarasıyla ya da "kendi aracımızla teslim" beyanıyla (§8.2) | Yalnız açık fiziksel kalemler gider; sipariş tek parça ve tek takip numarası taşır. Sipariş sayfası şirketi, takip numarasını ve listedeki şirkette "Takip et" bağlantısını ya da araç beyanını gösterir. Fiziksel kalemin iptal yolu kapanır; cayma düğmesi ve "sorun bildir" açılır | → Kargoya verildi (S5) | B-5 → müşteri | `02 §3.20.3`, §3.20.7, §3.20.8, §7.3.7, §7.5.2 |
| 2.5.3.4 | Kargo malı ulaştıramaz; yönetici "teslim edilemedi" işaretler (§8.2) | Gönderi firmaya döner; yönetici yeniden gönderir ya da iptal eder (§8.2, §8.3). Kargodayken cayılmış kalemde §2.10.3 işler | → Teslim edilemedi (S7) · ardından S8, S9 ya da S11 | B-6 → müşteri · yeniden gönderimde B-5 | `02 §5.4` |
| 2.5.3.5 | Mal müşteriye ulaşır; yönetici teslim işaretini ve teslim tarihini girer (§8.2) | Tarih teslimin gerçekleştiği gündür ve işaret gününden önce de olur; kargoya verildiği günden önceki ve ileri tarih reddedilir. Tarih fiziksel kalemlerin tamamına yazılır. Kendiliğinden geçiş yoktur: işaretlenmeyen sipariş Kargoya verildi'de kalır ve cayma penceresi başlamaz. Cayma penceresi (Z-13) ve ayıp talebinin süresi (Z-18) bu tarihten işler | → Teslim edildi (S6) | — (`02 §9.2`) | `02 §3.20.11`, §5.4 |
| 2.5.3.6 | Yönetici hizmet kalemini "tamamlandı" işaretler — ödeme onaylanmışken (§8.2) | Kalem teslim edilmiş olur; hizmetten cayma hakkı düşer, "sorun bildir" açılır. Randevu ve takvim yoktur; ifa süresi aşılırsa kendiliğinden bir işlem başlamaz. Fiziksel açık kalemi olmayan siparişte son açık kalemse sipariş kapanış kuralıyla hattını bitirir (§1.4.3) | kalem: teslim işareti · son açık kalemse → Teslim edildi (S2, S10, S11) | — (`02 §9.2`) | `02 §3.20.9`, §7.3.3, §5.4 · K-671 |
| 2.5.3.7 | Müşteri fiziksel kalemi olmayan siparişini izler | Sipariş Hazırlanıyor ve Kargoya verildi'yi kullanmaz; Alındı'da bekler ve açık kalemlerin tamamı teslim işaretini aldığında Teslim edildi'ye geçer (§1.5) | Alındı + Ödendi → Teslim edildi (S2) | — | `02 §3.20.12`, §5.7 |

### 2.6 Müşteri: sipariş takibi (akış 2)

Sipariş sayfasına üç yoldan girilir ve sayfa her bilgiyi taşır; hiçbir akış bir e-postanın ulaşmasına bağlı değildir (`02 §3.22.3`, §6.1.1; K-187).

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 2.6.1 | Üye giriş yapıp sipariş geçmişinden siparişi açar | Liste hesaba bağlı siparişleri — hesaba düşmüş misafir siparişleri dahil — gösterir. Ödemesi hiç alınmamış ve kendiliğinden iptal edilmiş sipariş listede görünmez; müşterinin ya da firmanın iptal ettiği ödenmemiş sipariş görünür | — | — | `02 §3.13.2`, §3.17.8, §3.22.3 |
| 2.6.2 | Misafir alıcı sipariş numarası ve e-postasıyla sorgular | İkisi eşleşirse sipariş sayfası açılır; numara tek başına açmaz. Başarısız sorgular aynı IP'den ve sorguya yazılan aynı e-postaya yönelik olarak ayrı ayrı limitlidir (L-5); e-posta ekseninde engellenen gerçek sahip e-postadaki bağlantıdan girmeye devam eder | — | — | `02 §3.22.3`, §3.22.5, §8.2.2 |
| 2.6.3 | Müşteri sipariş e-postasındaki bağlantıyı açar | Bağlantının erişim anahtarı sayfayı e-posta sormadan açar. Anahtarın kendi ömrü yoktur — siparişin kişisel verileri imha edilene kadar çalışır; hesabın silinmesinden ve hesabın e-posta değişikliğinden etkilenmez. Dijital ürünün indirme bağlantısı aynı kapıdır | — | — | `02 §3.22.3`, §3.22.5 |
| 2.6.4 | Müşteri sipariş sayfasını okur | Sayfa iki eksenin durumunu, donmuş kalemleri ve tutarları, adresleri, takip bilgisini, indirme düğmesini, havalede siparişe donmuş IBAN'ı ve son ödeme gününü, onaylanan Ön Bilgilendirme Formu ve sözleşme sürümlerini, onay kutularının kaydını ve o anda açık olan işlemleri (§1.11.1–§1.11.14) taşır. Fatura sayfada yoktur — firma onu kendi kanalından iletir. Sayfa arama motorlarına kapalıdır | — | — | `02 §3.22.4`, §3.23.3, §3.24.6, §3.25.2, §3.30.6, §4.2 Z-8 · K-663 |
| 2.6.5 | Müşteriye bir sipariş e-postası ulaşmaz | Müşteri her bilgiye sipariş sayfasından ulaşır. Üç başarısız yeniden denemeden (Z-26) sonra siparişin panel satırına "e-posta ulaşmadı" işareti düşer ve yönetici e-postayı panelden yeniden gönderir (§1.11.36) | — | — | `02 §6.1.1`, §9.1.6 |
| 2.6.6 | Üye sipariş yürürken hesabını siler ya da hesabının e-postasını değiştirir (§9) | Sipariş kendi kaydıyla sürer. Hesap silindiyse takip, iptal, cayma ve iade misafir yolundan — sipariş numarası ve siparişe donmuş e-posta — ya da e-postadaki bağlantıyla yürür; silinmiş hesabın siparişleri hiçbir hesaba bağlanmaz. E-posta değiştiyse bağlanmış siparişler hesapta kalır | — | — | `02 §3.13.14`, §3.15.2, §3.15.3 |
| 2.6.7 | Misafir alıcı e-postasını yanlış yazdığını onaydan sonra fark eder | Sipariş sayfasına giden iki yol da o adrese bağlıdır; müşteri firmaya telefonla, iletişim formuyla ya da e-postayla ulaşır ve yönetici kimliği teyit edip siparişin e-postasını düzeltir. Akış §2.10.5'tedir | — | B-1 → yeni adres · B-15 → eski adres | `02 §6.2.13`, §10.4.11 |

### 2.7 Müşteri: iptal ve gecikme feshi

Satın almadan sonraki üç yolun ilkidir (`02 §7.1.1`). İptal firma onayı beklemez; açık olduğu aralıklar §1.11.1–§1.11.4 ve §1.11.8'dedir. Müşterinin kendi iptali firmaya e-postayla bildirilmez — firma onu panelin sayaçlarında görür (`02 §9.5`).

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 2.7.1 | Müşteri ödeme beklenirken siparişi iptal eder | Ödeme onayından önce iptal yalnız sipariş bütünüyledir — dijital kalem dahil. Ayrılan stok, kontenjan ve kupon hakkı serbest kalır. Sipariş müşterinin listesinde görünmeye devam eder | → İptal edildi + Başarısız (S3, Ö2) | B-7 → müşteri | `02 §3.17.8`, §5.6.5, §7.2.2 |
| 2.7.2 | Müşteri ödenmiş siparişte bir fiziksel kalemi iptal eder — sipariş Kargoya verildi'ye geçene kadar | Kalem iptal kaydını alır ve stoğu kendiliğinden döner; kalan kalemler yoluna devam eder. Kupon hakkı yalnız siparişin tamamı iptal edilirse döner. Havale hattında müşteri geri ödeme IBAN'ını bu adımda girer (2.7.5). Kargoya verme süresi (Z-10) aşıldıktan sonraki iptal gecikmede feshin yoludur: panel firmaya kanuni faiz uyarısını gösterir | kalem: iptal kaydı · son açık kalemse kapanış kuralı (§1.4.3) — → İptal edildi (S4) ya da → Teslim edildi (S10) | B-7 → müşteri | `02 §7.2.1`–§7.2.3, §7.2.6, §7.2.7 |
| 2.7.3 | Müşteri ödenmiş siparişte hizmet kalemini iptal eder — kalem "tamamlandı" işaretini alana kadar | Siparişin sevkiyat durumundan bağımsızdır; kontenjan havuza döner. İfa süresi (Z-39) aşıldıktan sonraki iptalde panel firmaya kanuni faiz uyarısını gösterir | kalem: iptal kaydı · son açık kalemse kapanış kuralı (§1.4.3) | B-7 → müşteri | `02 §3.20.9`, §7.2.5, §7.2.6 |
| 2.7.4 | Müşteri ödenmiş dijital kalemi iptal etmek ister | Yoktur: kalem ödeme onayında teslim edilmiştir ve cayma hakkı ön onayla düşmüştür. Sebepli sorun ayıp talebi yolundan yürür (§2.9) | — | — | `02 §7.2.5`, §7.3.2 |
| 2.7.5 | Müşteri havale hattında geri ödeme IBAN'ını girer — iptal ya da fesih sırasında | Sipariş sayfası IBAN'ı biçim ve sağlama basamağıyla denetler ve aydınlatma metninin bağlantısını gösterir. IBAN geri ödeme işlenene kadar sipariş sayfasında düzeltmeye açıktır. Kart hattında IBAN istenmez | — | — | `02 §3.33.5`, §7.4.5 |
| 2.7.6 | Sistem ya da yönetici iptalin geri ödemesini yapar | Para ödemenin geldiği yoldan, iptalden itibaren on dört gün içinde (Z-17) gider: kart hattında sistem iadeyi kendiliğinden başlatır — onay penceresi yoktur; havale hattında yönetici müşterinin IBAN'ına gönderip panelden işler (§8.3). Siparişin fiziksel kalemlerinin tamamı kargodan önce iptal edildiyse ödenmiş kargo ücreti son iptalin geri ödemesine eklenir. Kart iadesi sağlayıcıda gerçekleşmezse 2.8.4.3 işler | → Kısmen geri ödendi ya da Geri ödendi (Ö3, Ö4, Ö5) | B-8 → müşteri · kartta F-4 → firma | `02 §5.5`, §7.2.8, §7.2.9 |
| 2.7.7 | Müşteri gecikme nedeniyle fesheder — siparişin firmaya ulaşmasından Z-11 geçmiş ve teslim tarihi girilmemiş fiziksel kalemi varken | Fesih teslim edilmemiş fiziksel kalemlerin tamamına uygulanır — cayma istisnası ve kişiye özel üretim işaretli kalem dahil; teslim edilmiş dijital ve hizmet kalemleri ile tamamlanmamış hizmet kalemi yoluna devam eder. Havale hattında IBAN fesih sırasında girilir (2.7.5). Feshedilen kalem kargoya verilmez, yeniden gönderilmez ve firma iptaline konu olmaz; kargoya verilmemişse stoğu kendiliğinden döner. Panel fesih satırında firmaya kanuni faiz uyarısını gösterir | kalem: gecikme feshi · Hazırlanıyor'da açık kalem kalmazsa → İptal edildi (S4 kapanışı) · teslim edilmiş kalem varsa → Teslim edildi (S10) · gönderi kargodaysa 2.7.8 | B-9 → müşteri · F-3 → firma | `02 §5.8`, §7.2.3 · §1.11.8 |
| 2.7.8 | Sistem ya da yönetici feshin geri ödemesini yapar; kargodaki mal firmaya döner | Feshedilen kalemlerin ödenmiş bedeli ve kargo ücreti, fesih bildiriminin tarih damgasından itibaren on dört gün içinde ödemenin geldiği yoldan geri ödenir — kartta sistem kendiliğinden başlatır, havalede yönetici işler. Kargodaki mal firmanın iade adresine döner, masrafı firmadadır; yönetici dönen malı teslim alma adımıyla işler ve stoğa ekler — adım geri ödemeyi başlatmaz. Açık kalem kalmadıysa yönetici siparişi S7'den sonra S9'un kapanışıyla kapatır (§8.3) | Ö3–Ö5 · kargodaysa → Teslim edilemedi (S7) → İptal edildi (S9 kapanışı) ya da → Teslim edildi (S11) | B-8 → müşteri · kartta F-4 → firma · S7'de B-6 → müşteri; kapanışta B-7 gitmez | `02 §5.8`, §7.2.3, §7.2.9 |

### 2.8 Müşteri: cayma ve iade

Sebepsiz ikinci yoldur (`02 §7.3`, §7.4). Düğmelerin aralıkları §1.11.5–§1.11.7'de, kalemin kayıtları §1.7.1'dedir. Cayma beyanı hiçbir ekseni değiştirmez; ödeme ekseni para gönderildiğinde değişir (`02 §5.6.6`). Yöneticinin iade ve geri ödeme adımları §8.3'tedir.

#### 2.8.1 Fiziksel kalem

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 2.8.1.1 | Müşteri sipariş sayfasında cayma düğmesini görür | Düğme sipariş Kargoya verildi'ye geçtiği andan teslim tarihinden on dört gün (Z-13) dolana kadar görünür; teslim tarihi girilmemişse açıktır — mal yoldayken de. Kargoya verilmeden önce yol iptaldir (2.7.2). Mutlak istisna işaretli kalemde düğme yoktur; koşullu istisnada açıktır ve beyan ekranı "koruyucu ambalajı açılmamışsa cayabilirsiniz" koşulunu yazar. İstisna, sebebi ve koşulu kaleme donmuştur | — | — | `02 §3.23.1`, §7.3.1, §7.3.4, §7.3.7 |
| 2.8.1.2 | Müşteri kalemi seçip cayma beyanında bulunur | Beyan ekranı firmanın güncel iade adresini ve malı on dört gün içinde gönderme yükümlülüğünü gösterir; adres beyanla birlikte kayda yazılır ve sonradan değişmez. Havale hattında müşteri IBAN'ını girer (2.7.5). Beyan tarih damgasıyla kayda geçer; pencerenin içinde olup olmadığı damgadan okunur. Teslim edilmiş kalemde iade süreci başlar ve panel kalemi "iade malı bekleniyor" gösterir; teslim işaretini almamış kalemi — kargodayken — beyan kapatır (§2.10.3) | kalem: cayma beyanı · eksenler değişmez | B-9 → müşteri — beyanın tarihi ve iade adresi · F-3 → firma | `02 §5.6.6`, §5.8, §7.3.5, §7.4.4 · K-665 |
| 2.8.1.3 | Yönetici teslimi beyanla aynı güne işaretler — ya da teslim tarihi sonradan o güne düzeltilir (§8.2) | Panel teslimin beyandan önce mi sonra mı olduğunu sorar; cevap beyanın teslimden önce mi sonra mı sayılacağını ve geri ödeme süresinin başlangıcını belirler | — | — | `02 §3.20.11`, §7.4.1 |
| 2.8.1.4 | Müşteri malı gönderir — beyandan itibaren Z-42 içinde | Taşıyıcıyı müşteri seçer ve malı firmanın iade adresine karşı ödemeli gönderir; masrafı teslim alırken firma öder. Ürün iade etiketi üretmez, taşıyıcı ya da takip numarası istemez ve gönderimi denetlemez | — | — | `02 §7.4.2`, §7.4.4 |
| 2.8.1.5 | Yönetici iade malını teslim alır ve ulaşma tarihini girer (§8.3) | Teslimden sonraki caymada geri ödemenin on dört günü (Z-16) ulaşma tarihinden — mal beyandan önce ulaştıysa beyandan — işler. Stok, yönetici malı kontrol edip eklediğinde döner; kupon hakkının dönüşü §1.4.4'tedir. "İade malı bekleniyor" kalkar | kalem: iade teslim alma | — (`02 §9.2`) | `02 §7.4.1`, §7.4.7 |
| 2.8.1.6 | Koşullu istisna kaleminin malı koruyucu ambalajı açılmış olarak döner | Yönetici teslim alma adımında iade reddini seçer (§8.3): geri ödeme yapılmaz, havale hattında IBAN silinir, mal stoğa girmez ve firmanın bedeliyle siparişteki teslimat adresine geri gönderilir; ret geri alınmaz. İtirazın yeri §5.3 | kalem: iade reddi | B-16 → müşteri | `02 §7.4.9` · K-656, K-657 |
| 2.8.1.7 | Mal kullanılmış, hasarlı ya da eksik döner — kalem reddedilebilecek bir kalem değildir | Ret ve kesinti yoktur; kalemin ödenmiş bedeli tam geri ödenir. Değer kaybı talebi ürünün dışındadır (§5.3) | — | — | `02 §7.4.10` · K-668 |
| 2.8.1.8 | Müşteri malı hiç göndermez | Geri ödeme süresi başlamaz; panel kalemi beyandan geçen gün ve gönderme süresiyle gösterir. Z-42 geçtikten sonra yöneticiye isteğe bağlı "mal dönmedi" kapanışı açılır (§1.11.30); havale hattında IBAN kapatmayla ya da Z-38 dolunca silinir. Mal sonradan ulaşırsa kalem yeniden açılır — §2.10.4 | kalem: "mal dönmedi" kapanışı | — (`02 §9.2`) | `02 §7.4.1`, §7.4.5, §10.4.9 |

#### 2.8.2 Hizmet kalemi

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 2.8.2.1 | Müşteri hizmet kaleminin cayma düğmesini görür | Düğme ödeme onayından sonra açılır ve kalem "tamamlandı" işaretini alana ya da Z-14 dolana kadar görünür — hangisi önce gelirse. Pencere sipariş tarihinden, onay anında fiziksel kalem taşıyan siparişte siparişin teslim tarihinden işler; teslim tarihi girilmemişse ya da fiziksel kalemlerin tamamı teslimden önce kapandıysa hak tamamlanma işaretine kadar sürer. Ödeme beklenirken yol siparişin bütünüyle iptalidir (2.7.1) | — | — | `02 §7.3.3` · K-673 |
| 2.8.2.2 | Müşteri hizmet kaleminden cayar | Beyan ekranı iade adresi ve gönderme yükümlülüğü göstermez — geri gönderilecek mal yoktur. Havale hattında IBAN girilir (2.7.5). Ürün ifanın başladığını izlemez: kısmen ifa edilmiş hizmetten cayma da tam caymadır ve kalemin ödenmiş bedelinin tamamı geri ödenir. Beyan kalemi kapatır; kontenjan havuza döner; geri ödemenin on dört günü beyandan işler (Z-16) | kalem: cayma beyanı · son açık kalemse kapanış kuralı (§1.4.3) — → Teslim edildi (S2, S10, S11) ya da hiçbir kalem teslim edilmemişse → İptal edildi (S3, S4) | B-9 → müşteri · F-3 → firma | `02 §5.6.6`, §7.3.3, §7.3.5, §7.4.1, §7.4.8 · K-672 |

#### 2.8.3 Dijital kalem

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 2.8.3.1 | Müşteri dijital kalemden caymak ister | Yoktur: hak, onay adımındaki üçüncü kutuyla ödeme onaylandığı anda düşmüştür. Ayıplı dosyada yol ayıp talebidir (§2.9) | — | — | `02 §3.24.2`, §7.3.2 |

#### 2.8.4 Geri ödeme ve başka kanaldan cayma

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 2.8.4.1 | Yönetici caymanın geri ödemesini işler — kart ve havale hattında (§8.3) | Para ödemenin geldiği yoldan gider: karta sağlayıcı üzerinden, havalede müşterinin IBAN'ına; havaleyi yönetici bankada gerçekleştikten sonra işler ve onay geri alınamaz. Teslim alma işareti geri ödemeyi kendiliğinden başlatmaz. Kargoyla gönderilen fiziksel kalemlerin tamamından cayıldıysa — ardışık beyanlarla da — kargo ücreti son caymanın geri ödemesine eklenir; bir kalem müşteride kalıyorsa eklenmez. Kupon hakkı yalnız siparişin tamamı iade edilince döner | → Kısmen geri ödendi ya da Geri ödendi (Ö3, Ö4, Ö5) | B-8 → müşteri | `02 §5.5`, §7.4.5, §7.4.6, §7.4.8 |
| 2.8.4.2 | Geri ödeme havalesi bankada gerçekleşmez | Yönetici geri ödemeyi işlemez, panelden "havale gerçekleşmedi" der: IBAN silinir ve sipariş sayfasında IBAN alanı yeniden açılır; müşteri yeni IBAN'ı girer. Geri ödemenin süresi durmaz | kalem: IBAN isteği | B-14 → müşteri | `02 §7.4.5` |
| 2.8.4.3 | Kart iadesi sağlayıcıda gerçekleşmez — iptalde, fesihte ya da caymada | Ödeme ekseni değişmez; panelde "geri ödeme gerçekleşmedi" işareti düşer. Yönetici yeniden dener ya da müşteriye havale yolunu açar; açınca sipariş sayfası kart iadesinin gerçekleşmediğini söyler ve IBAN alanı açar — IBAN girmek müşterinin seçimidir. IBAN girildikten sonra kart iadesi yeniden denenmez; süre durmaz | havale yolu açılınca kalem: IBAN isteği | Havale yolu açılınca B-14 → müşteri | `02 §5.5`, §7.2.8 |
| 2.8.4.4 | Müşteri caymayı sipariş sayfası yerine e-postayla, mektupla ya da örnek cayma formuyla bildirir | Yönetici bildirimin siparişin sahibinden geldiğini teyit eder ve panelden kaydeder (§8.4); beyanın tarihi bildirimin firmaya ulaştığı tarihtir. Havale hattında IBAN yalnız siparişin iletişim e-postasından gelen bildirimden aktarılır; öteki hâllerde kayıt IBAN'sız yapılır ve sipariş sayfasında IBAN alanı açılır. Kargoya verilmemiş fiziksel kalemde ve ödemesi beklenen siparişin hizmet kaleminde cayma kaydı açılmaz — yönetici "müşteriyle anlaşıldı" sebebiyle iptal eder (§2.7). Pencere kapandıktan sonra ya da mutlak istisna kaleminde eksik bilgilendirme gerekçesiyle gelen bildirim firmanın değerlendirmesidir | kalem: cayma beyanı | B-9 → müşteri · IBAN'sız kayıtta B-14 → müşteri; firmaya — (`02 §9.3.2`) | `02 §7.3.5`, §8.3.8, §9.3.2, §10.4.10 · K-673 |

### 2.9 Müşteri: ayıp talebi

Sebepli üçüncü yoldur (`02 §7.5`). Talebin durumları §1.9.1'de, firmanın adımları §8.4'tedir.

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 2.9.1 | Müşteri sipariş sayfasında "sorun bildir"i görür | Fiziksel kalemde sipariş Kargoya verildi'ye geçtiği andan, dijital kalemde ödeme onayından, hizmet kaleminde "tamamlandı" işaretinden sonra ve kalemde talep yokken görünür. Süre kalemin teslim işaretinin tarihinden iki yıldır (Z-18); fiziksel kalemde teslim tarihi girilmemişse işlemez. Süre dolunca kanal kapanır; hak iletişim formundan sürer. Ayıp talebi iletişim formunun konu tiplerinde yoktur | — | — | `02 §7.5.1`, §7.5.2 · K-676 |
| 2.9.2 | Müşteri kalemi seçer ve sorunu açıklar | Fotoğraf ve dosya yüklenmez — firma kanıtı e-postayla ister. Fiziksel kalemde ekran firmanın güncel iade adresini gösterir ve adres talebe yazılır. Ekran aydınlatma metninin bağlantısını gösterir. Talep kalem ve sipariş bağlamıyla panele düşer | kalem: ayıp talebi — Açık · sevkiyat hattı değişmez | B-9 → müşteri — talebin tarihi ve iade adresi · F-3 → firma | `02 §3.1.7`, §3.33.5, §5.8, §7.5.2 · K-665 |
| 2.9.3 | Müşteri talep açıkken aynı kalemde yeni bir sorun bildirmek ister | Yeni talep açılmaz; sipariş sayfası "sorun bildir" yerine talebin durumunu gösterir. Ek bilgi firmanın e-postasına yazılır | — | — | `02 §5.10` · K-676 |
| 2.9.4 | Firma çözümü müşteriyle sistemin dışında yürütür | Seçimlik haklar — onarım, değişim, bedel indirimi, sözleşmeden dönme — ürünün dışındadır. Geri gönderim gerekiyorsa müşteri malı iade adresine karşı ödemeli gönderir; masraf firmanındır. Para gerekiyorsa sözleşmeden dönmede kalemin iade hattı, bedel indiriminde tutar bazlı kısmi geri ödeme işler (§8.4). Ayıplı dijital üründe doğal çözüm dosya güncellemesidir | Para gönderilirse Ö3–Ö5 | Geri ödemede B-8 → müşteri | `02 §7.4.3`, §7.4.4, §7.5.3, §7.5.4 |
| 2.9.5 | Yönetici talebi "çözüldü" işaretler | Çözümü firma müşteriye kendi cevabıyla yazmıştır | Ayıp talebi: Açık → Çözüldü | — (`02 §9.2`) | `02 §5.10` |
| 2.9.6 | Müşteri çözüm tutmadığında talebi yeniden açar — Z-18 içinde | Yeni talep açılmaz; ayıbın geçmişi tek kayıtta kalır. Aynı kalemin yeni sorunu da bu yolla bildirilir | Ayıp talebi: Çözüldü → Açık | B-9 → müşteri — yeniden açmanın tarihi (K-681) · F-3 → firma | `02 §5.10`, §9.2, §9.3 · K-681 |

### 2.10 Uçtan uca anlatılar

If/then'e sığmayan senaryolar ve fiziksel siparişin uçtan uca ana akışı (K-651). Anlatı önceki alt bölümlerin satırlarına işaret eder; kural ve süre tekrarlanmaz.

#### 2.10.1 Fiziksel siparişin ana akışı (akış 1 + 6)

`02 §2.2`'nin sekiz adımı, aktörü ve durumuyla. Kart hattında yöneticinin zorunlu elle adımı iki, havale hattında üçtür (§1.5; `02 §10.5.1`).

| # | Aktör ne yapar | Sistem ne denetler, ne yapar | Durum | Bildirim | Kaynak |
|---|---|---|---|---|---|
| 2.10.1.1 | Ziyaretçi ürünü kategori gezinmesiyle ya da aramayla bulur ve varyantını seçer | 2.1.2–2.1.6 | — | — | `02 §2.2` adım 1 |
| 2.10.1.2 | Müşteri varyantı adediyle sepete ekler | Sepet canlıdır; stok ayrılmaz (2.3.1) | — | — | `02 §2.2` adım 2 |
| 2.10.1.3 | Müşteri ödeme adımında adresleri, varsa kuponu ve ödeme yöntemini girer | 2.4.1–2.4.5 | — | — | `02 §2.2` adım 3 |
| 2.10.1.4 | Müşteri özeti ve Ön Bilgilendirme Formu'nu görür, iki kutuyu işaretler ve onaylar | Onay anında yeniden değerlendirme (2.4.6–2.4.8) | — | — | `02 §2.2` adım 4 |
| 2.10.1.5 | Sistem siparişi oluşturur | Numara, donma, ayırma (2.4.9) | Alındı + Bekliyor | B-1 → müşteri — havalede B-2 de · F-1 → firma | `02 §2.2` adım 5 |
| 2.10.1.6 | Müşteri öder — kartta 3D Secure'la; havalede yönetici "ödendi" işaretler | Ödeme onayı (2.5.1, 2.5.2, 2.5.3.1); kargoya verme süresi başlar | → Hazırlanıyor + Ödendi (Ö1, S1) | B-4 → müşteri | `02 §2.2` adım 6 |
| 2.10.1.7 | Yönetici siparişi hazırlar ve kargoya verir | 2.5.3.3; §8.2 | → Kargoya verildi (S5) | B-5 → müşteri | `02 §2.2` adım 7 |
| 2.10.1.8 | Mal ulaşır; yönetici teslim işaretini ve tarihini girer | 2.5.3.5; cayma penceresi ve ayıp talebinin süresi bu tarihten işler | → Teslim edildi (S6) | — (`02 §9.2`) | `02 §2.2` adım 8 |

Müşteri her adımı sipariş sayfasından izler (§2.6). Sipariş Teslim edildi'de kalır; bundan sonraki yollar cayma (§2.8) ve ayıp talebidir (§2.9).

#### 2.10.2 Karışık sipariş — fiziksel, dijital ve hizmet kalemi

Üye bir masa lambası (fiziksel), lambanın kurulum kılavuzu (dijital) ve evde montaj hizmeti (hizmet) alır ve kartla öder.

1. **Onay.** Sepette dijital ve hizmet kalemi olduğu için iki kutunun yanında iki kutu daha çıkar (2.4.7). Sipariş Alındı + Bekliyor doğar; B-1 gider (2.4.9).
2. **Ödeme onayı.** Sağlayıcının başarı bildirimiyle Ö1 ve S1 birlikte işler: sipariş Hazırlanıyor + Ödendi olur. Kılavuz o anda teslim edilir ve B-4 indirmenin hazır olduğunu söyler; kılavuzun iptal ve cayma yolu kapanır. Kargoya verme süresi lamba için, ifa süresi montaj için başlar (2.5.3.1; §1.4.2.1).
3. **Montajın cayma penceresi.** Sipariş onay anında fiziksel kalem taşıdığı için montajın penceresi siparişin teslim tarihinden işler; teslim tarihi girilene kadar hak açıktır (2.8.2.1).
4. **Olağan yol.** Yönetici lambayı kargoya verir (S5, B-5) ve teslimi işaretler (S6): sipariş Teslim edildi olur. Montaj kalemi siparişin hattını beklemez; yönetici montajı yapınca "tamamlandı" işaretler ve montajdan cayma hakkı düşer (2.5.3.6).
5. **Dal — lamba kargodan önce iptal edilir.** Müşteri sipariş Hazırlanıyor'dayken lambayı iptal eder (2.7.2): B-7 gider; sistem kart iadesini kendiliğinden başlatır (F-4) ve fiziksel kalemlerin tamamı kargodan önce iptal edildiği için kargo ücreti de iadeye eklenir; ödeme Kısmen geri ödendi olur (Ö3, B-8). Kupon hakkı dönmez — sipariş yaşamaktadır. Sipariş fiziksel kalemsiz hatta düşer ve Hazırlanıyor'da bekler (§1.5). Teslim tarihi hiç oluşmayacağı için montajın penceresi sipariş tarihine geri çekilmez: hak "tamamlandı" işaretine kadar sürer (`02 §7.3.3`). Yönetici montajı yapıp işaretler — ödeme Kısmen geri ödendi'deyken de (K-671) —; montaj son açık kalemdir ve kılavuz teslim edilmiş olduğundan sipariş Teslim edildi'ye geçer (S10). Müşteri montajdan caysaydı sipariş yine Teslim edildi'ye — beyanla aynı anda — geçerdi (§1.4.3).

#### 2.10.3 Kargodayken cayılıp geri dönen gönderi

Misafir alıcı tek fiziksel kalemli siparişini havaleyle ödemiştir; sipariş Kargoya verildi + Ödendi.

1. **Beyan.** Müşteri mal yoldayken sipariş sayfasından cayar (2.8.1.2): iptal yolu kapanmıştır, cayma düğmesi açıktır. Beyan IBAN'ı ve iade adresini kayda yazar; B-9 müşteriye, F-3 firmaya gider. Kalem teslim işareti almadığı için beyan kalemi kapatır ve geri ödemenin on dört günü beyandan işler — malın dönüşü beklenmez (`02 §7.4.1`). Açık kalem kalmamıştır ama gönderi kargodadır: sipariş Kargoya verildi'de kalır.
2. **Dönüş.** Müşteri malı teslim almaz ve kargo onu firmaya döndürür. Yönetici "teslim edilemedi" işaretler (S7, B-6). Açık kalem kalmadığı için yönetici siparişi S9'un kapanışıyla kapatır: sebep seçmez, iptal kaydı ve ikinci bir geri ödeme açılmaz, B-7 gitmez (§1.4.3). Siparişte teslim edilmiş bir kalem — ör. bir dijital kalem — olsaydı S7 kapanışı tamamlar ve sipariş Teslim edildi'ye geçerdi (S11).
3. **Teslim alma.** Yönetici dönen malı teslim alma adımıyla işler — teslim tarihi olmadığı için ulaşma tarihinin alt sınırı uygulanmaz — ve kontrol edip stoğa ekler. Firma iptali bu kaleme uygulanmaz (§1.11.21).
4. **Geri ödeme.** Yönetici parayı müşterinin IBAN'ına gönderir ve panelden işler: kargoyla gönderilen fiziksel kalemlerin tamamından cayıldığı için kargo ücreti de eklenir; ödeme Geri ödendi olur (Ö4) ve B-8 gider. Süre beyandan işlemektedir; teslim alma adımı onu başlatmaz ve geri ödeme malın dönüşünü beklemez.

#### 2.10.4 IBAN silindikten sonra ulaşan iade malı

Üye havaleyle ödediği fiziksel kalemin tesliminden sonra cayar ve IBAN'ını beyanla girer.

1. **Beyan.** Kalem "iade malı bekleniyor" görünür; geri ödeme süresi mal ulaşmadan başlamaz (2.8.1.2, 2.8.1.8).
2. **Mal gelmez.** Müşteri malı Z-42 içinde göndermez; sistem kendiliğinden bir işlem başlatmaz. Yönetici kalemi "mal dönmedi" gerekçesiyle kapatırsa IBAN kapatmayla silinir, kalem bekleyen işlerden düşer ve müşteriye bildirim gitmez; kapatmazsa IBAN Z-38 dolunca kendiliğinden silinir, kalem açık kalır ve cayma geçerliliğini korur (`02 §7.4.5`, §10.4.9).
3. **Mal ulaşır.** Yönetici teslim alma adımında ulaşma tarihini girer; kapatılmış kalem yeniden açılır. IBAN silinmiş olduğu için adımla birlikte IBAN isteği düşer: müşterinin sipariş sayfasında o kalem için IBAN alanı açılır ve B-14 gider. Yönetici IBAN'ı panelden girmez.
4. **Bekleme.** Geri ödemenin on dört günü ulaşma tarihinden işler ve IBAN beklenirken durmaz; panel kalemi "IBAN bekleniyor" olarak, kalan süreyle gösterir.
5. **Kapanış.** Müşteri IBAN'ı sipariş sayfasından girer; yönetici geri ödemeyi işler (Ö3–Ö5, B-8) ve IBAN geri ödeme tamamlanınca silinir. Firma "mal dönmedi" kapanışından sonra müşterinin bildirimi üzerine ödemeye karar vermişse ödediği tutar kalemin geri ödemesinden düşülür (`02 §10.4.9`). Müşteri IBAN'ı hiç girmezse kalem kapanmaz ve borç sürer; kalem "IBAN bekleniyor" listesinde görünür ama bekleyen işler sayacında sayılmaz (`02 §7.4.5`).

#### 2.10.5 Misafir siparişinin e-posta düzeltmesi

Misafir alıcı ödeme adımında e-postasını yanlış yazar ve havaleyle sipariş verir.

1. **Yanlış adres.** Onay özeti adresi açıkça gösterir ama müşteri hatayı görmez (2.4.6). B-1 ve B-2 yanlış adrese gider; yanlış adres bir müşteri hesabınınsa sipariş o hesaba düşer (`02 §3.13.3`).
2. **Ödeme.** Müşteri IBAN'ı ve sipariş numarasını ekranda görmüştür ve havaleyi yapar; yönetici "ödendi" işaretler (2.5.2.4) ve B-4 de yanlış adrese gider. Sipariş sayfasına giden iki yol da yanlış adrese bağlı olduğu için müşteri sayfayı açamaz (2.6.7).
3. **Teyit.** Müşteri firmaya telefonla, iletişim formuyla ya da e-postayla ulaşır. Yönetici kimliği siparişteki bilgilerle — ad, teslimat telefonu, kalemler ve tutar — teyit eder; sipariş numarası tek başına teyit değildir (§6.2, §8.4).
4. **Düzeltme.** Yönetici e-postayı düzeltir: yeni erişim anahtarı üretilir ve eski bağlantılar geçersizleşir; yeni adrese B-1 — donmuş sürümüyle ve yeni bağlantıyla —, eski adrese B-15 gider. Sipariş yanlış adresin hesabına düşmüşse bağ kesilir; yeni adres doğrulanmış bir müşteri hesabınınsa sipariş o hesaba düşer. Panel siparişin satırında "e-posta düzeltildi" işaretini gösterir; eski ve yeni adres siparişin e-posta geçmişinde durur. Durumlar, süreler, kalemler ve IBAN değişmez (`02 §10.4.11`).
5. **Devam.** Müşteri yeni bağlantıyla sipariş sayfasına girer ve akışı sürdürür. Düzeltmeyi gerçek sahip istemediyse B-15 onu uyarır; yönetici e-postayı yeniden düzeltir, ama arada yapılan işlemler — iptal, cayma, IBAN girişi — geri alınmaz ve işlem izinden ve e-posta geçmişinden okunur; kandırılmanın sonucu firmadadır (§6.2; `02 §8.3.6`).

## 3. Hata akışları

> **Ne yazılır:** Timeout, iptal, doğrulama hatası, dış servis hatası senaryoları.

## 4. Zaman aşımı yönetimi

> **Ne yazılır:** Tüm timeout akışları **tek bölümde** — aktör akışlarına gömülmez. Uyarı, dondurma ve normale dönüş adımları dahil.

## 5. İtiraz / anlaşmazlık akışları

## 6. Kötüye kullanım ve inceleme akışları

## 7. Bildirim haritası

> **Ne yazılır:** Akışlardan çıkarılmış, tek tabloda özetlenmiş bildirim listesi. Bildirim sistemi tasarımını bu tablo besler.

| # | Tetikleyici (durum geçişi / olay) | Alıcı | İçerik özeti | Kanal |
|---|---|---|---|---|

## 8. Yönetim (admin) akışları

> **Ne yazılır:** Kullanıcı akışlarıyla **aynı derinlikte**. Yönetim akışları genellikle beklenenden karmaşıktır.

## 9. Destekleyici akışlar

> **Ne yazılır:** Kayıt, profil yönetimi, hesap silme, tercih yönetimi.

## 10. Operasyonel akışlar

> **Ne yazılır:** Platform bakımı, dış servis kesintisi, bakım modu. Kesinti sırasında sürelerin dondurulması, kullanıcı bilgilendirmesi ve normale dönüş adımları.

## 11. `02`'ye geri besleme

> **Ne yazılır:** Akışlar yazılırken fark edilen gereksinim boşlukları ve `02`'de yapılan düzeltmeler.

Her satır bir karar grubudur; karar kaydı satırı ayrıntıyı, sürüm notu değişen yerleri taşır (K-652).

| # | Bulgu | `02`'de yapılan değişiklik |
|---|---|---|
| 11.1 | Workshop, AK3-06: yöneticinin ayrılmış adede dokunan katalog işlemlerinin sonucu yazılı değildi (K-653, K-654, K-655) | v0.48 — yeni `02 §3.6.7`, §3.10.6, §3.7.7, yeni 6.6.14…6.6.16 · `10` v0.30 KP-39, KP-40, KP-43 |
| 11.2 | Workshop, AK5-03 ⚠: iade malının reddi ve değer kaybıyla dönen mal yazılı değildi (K-656, K-657, K-668) | v0.48 — yeni `02 §7.4.9`, §7.4.10, §5.8, §10.1.2, §10.3.1, §10.5.2, §10.6, yeni B-16 · `10` v0.30 KP-47, KP-66 |
| 11.3 | Workshop, AK6-04: doğrulama bağlantısının yeniden istenmesinin limiti yoktu (K-658, K-659) | v0.48 — `02 §3.13.5`, L-9, P-49, Z-1, yeni 6.5.14 · `10` v0.30 KP-64, KP-72 |
| 11.4 | Workshop, AK8-09 ⚠: bekleyen yönetici davetinin geri çekilmesi, ikinci davet ve gönderenin kaldırılması yazılı değildi (K-660, K-661, K-662) | v0.48 — yeni `02 §10.2.6`, §10.2.2, §10.3.1, Z-3 · `10` v0.30 KP-62, KP-63 |
| 11.5 | Workshop, AK8-10 ⚠: donmayan ayarların — havale IBAN'ı, satış kapısı, iade adresi — yürüyen siparişe etkisi yazılı değildi (K-663, K-664, K-665) | v0.48 — `02 §3.21.5`, §3.23.3, §3.1.6, §3.1.7, B-2, B-9, yeni 6.9.14…6.9.16 · `10` v0.30 KP-59, KP-60 |
| 11.6 | Workshop, AK10-03 ⚠: sitenin kesintisinde sürelerin ve kendiliğinden işlerin kaderi yazılı değildi (K-666, K-667) | v0.48 — `02 §3.34.7`, §4.3, §3.21.8, Z-8, yeni 6.7.17 · `05` park satırı |
| 11.7 | Yazım, §1.2: hizmet tamamlamanın ve S2'nin "ödeme Ödendi iken" koşulu, kısmi geri ödemeden sonra hizmetin tamamlanmasını kapatıyor ve siparişi çıkışsız bırakıyordu (K-671) | v0.49 — `02 §3.20.9`, §5.4, §5.6.1 (S2) |
| 11.8 | Yazım, §1.4.3: Alındı ya da Hazırlanıyor'da son açık kalem cayma beyanıyla ya da çıkarmayla kapandığında, hiçbir kalemi teslim edilmemiş siparişin kapanışı yazılı değildi — sipariş çıkışsız kalıyordu (K-672) | v0.49 — `02 §5.4`, §5.6.6, §5.8, §9.2 (S3, S4, B-7) |
| 11.9 | Yazım, §1.11: hizmet kaleminin cayma düğmesinin ödeme beklenirken açık olup olmadığı yazılı değildi (K-673, ⚠) | v0.49 — `02 §7.3.3`, §10.4.10 · `10` v0.31 KP-22 |
| 11.10 | Yazım, §1.8: yayındaki ürünün son Yayında varyantının kalıcı silinmesi yayın kapısını bozuyordu (K-674) | v0.49 — `02 §3.7.1`, §3.7.8, §5.1 |
| 11.11 | Yazım, §1.7: panel işaretlerinin ne zaman kalktığı yazılı değildi (K-675) | v0.49 — `02 §6.1.1`, §6.1.2, §7.2.8, §7.4.1, §10.4.11 |
| 11.12 | Yazım, §1.9: bir kalemde ikinci ayıp talebinin açılıp açılamayacağı yazılı değildi (K-676) | v0.49 — `02 §5.10` |
| 11.13 | Yazım, §1.6: yeniden gönderim (S8) paketi yola çıkarır ve takip bilgisi gönderir ama geri alınamaz onayı yalnız S5'te yazılıydı (K-677) | v0.49 — `02 §5.9` |
| 11.14 | Yazım, §2.2: iletişim formunu gönderen kişiye alındı e-postası gidip gitmediği yazılı değildi — olay `02 §9.2`'nin matrisinde de "bildirim üretmeyen olaylar"da da yoktu (K-678, ⚠) | v0.50 — `02 §3.32.6`, §9.5 |
| 11.15 | Yazım, §2.4: bir siparişe birden çok kupon kodunun uygulanıp uygulanamayacağı yazılı değildi; sipariş düzleminde tek "kupon kodu" alanı vardı (K-679) | v0.50 — `02 §3.10.1` |
| 11.16 | Yazım, §2.4: üyenin ödeme adımında yazdığı yeni adresin adres defterine girip girmediği yazılı değildi (K-680, ⚠) | v0.50 — `02 §3.14.1` · `10` v0.32 KP-12 |
| 11.17 | Yazım, §2.9: müşterinin çözülmüş ayıp talebini yeniden açmasında kendisine kayıt kopyası gidip gitmediği yazılı değildi — firmaya F-3 gidiyordu (K-681, ⚠) | v0.50 — `02 §9.2` B-9 · `10` v0.32 KP-66 |

---

*Shopfolio — User Flows v0.3*
