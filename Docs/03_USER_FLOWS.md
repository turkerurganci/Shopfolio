# Shopfolio — User Flows

**Versiyon: v0.2** | **Bağımlılıklar:** `01_PROJECT_VISION.md`, `02_PRODUCT_REQUIREMENTS.md`, `10_MVP_SCOPE.md` (kapsam), `PRODUCT_DISCOVERY_STATUS.md` (Aşama 2 kararları) | **Son güncelleme:** 2026-10-04

> **Aşama:** 2 — Kullanıcı Akışları · **Rol:** Product Owner / Business Analyst
> **Traceability zorunlu:** Hayır (doğrudan türetim) — ama akışlar yazıldıktan sonra `02`'ye **geri dönülür**, tutarsızlık varsa düzeltilir.

> **Yazım durumu (K-28, K-30, K-432):** doküman Aşama 2'nin **tek yazım turunda**, beş oturumda yazılır; sıra ve konular `PRODUCT_DISCOVERY_STATUS.md` §8.2–§8.3'tedir. Girdi karar kaydı, bu dokümanın şablonu ve üst dokümanlardır — workshop'un sohbet geçmişi değil.
> - **1. oturum (2026-10-04, v0.2):** §0 — yazım konvansiyonları ve bölüm haritası (blok AK0) · §1 — durum makinesi (blok AK1) · §11 — geri besleme tablosunun ilk satırları. Oturum dokuz karar aldı: ikisi yazım konvansiyonu (K-669, K-670), yedisi yazımın bulduğu boşluk (K-671…K-677); yedisi `02`'ye, biri `10`'a da döndü (§11).
> - **Sıradaki oturumlar:** §2 (AK2) · §3–§6 (AK3–AK6) · §7 ve §8 (AK7, AK8) · §9–§11 (AK9–AK11). Kalite döngüsü tur bittikten sonra başlar (K-432).

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
| §2 Ana akışlar — müşteri tarafı | 2.1 Ziyaretçi: vitrinde gezinme ve ürünü bulma · 2.2 Ziyaretçi: kurumsal içerik ve iletişim formu · 2.3 Müşteri: sepet · 2.4 Müşteri: ödeme adımı ve sipariş onayı · 2.5 Müşteri: ödeme ve teslim · 2.6 Müşteri: sipariş takibi (akış 2) · 2.7 Müşteri: iptal ve gecikme feshi · 2.8 Müşteri: cayma ve iade · 2.9 Müşteri: ayıp talebi · 2.10 Uçtan uca anlatılar | AK2-01…AK2-10 | 2 |
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
| S9 | Teslim edilemedi → İptal edildi | **Yönetici** sebep seçerek iptal eder · **Kapanış** — açık kalem kalmamışsa yönetici S7'den sonra siparişi sebepsiz kapatır (`02 §5.8`) | §8.3 | B-7 → müşteri; kapanışta gitmez (B-9) |
| S10 | Hazırlanıyor → Teslim edildi | **Kapanış kuralını tamamlayan son olay** (§1.4.3) — son açık fiziksel kalemin iptali, çıkarılması ya da gecikme feshi; son hizmet kaleminin "tamamlandı" işareti ya da cayma beyanı | §2.7, §2.8, §8.2–§8.4 | Olayın kendi bildirimi |
| S11 | Teslim edilemedi → Teslim edildi | **Kapanış kuralını tamamlayan son olay** (§1.4.3) — S7'nin kendisi dahil: geri dönen fiziksel kalemler iptal, cayma beyanı ya da fesihle kapanmışsa | §8.2, §8.3 | Olayın kendi bildirimi (S7'de B-6) |

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
| Çözüldü (`Resolved`) | Firmanın çözüldü işaretlediği talep | Açık | Müşteri ya da yönetici yeniden açar — kalemin iki yıllık süresi (Z-18) içinde | Müşterinin yeniden açmasında F-3 → firma; yöneticininkinde — (`02 §9.2`) |

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
| 1.11.10 | Ayıp talebini yeniden açmak | Talep Çözüldü'yken, Z-18 içinde | Çözüldü → Açık · F-3 | `02 §5.10` |
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

### 2.x <Aktör> — <Akış adı>
> **Ne yazılır:** Adım adım. Her adımda: kullanıcı ne yapar · sistem ne kontrol eder · durum ne olur · kime bildirim gider.

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

---

*Shopfolio — User Flows v0.2*
