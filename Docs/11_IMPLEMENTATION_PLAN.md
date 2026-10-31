# Shopfolio — Implementation Plan

**Versiyon: v0.3** | **Bağımlılıklar:** `02`–`10` | **Son güncelleme:** 2026-10-31

> **Aşama:** 9 — Implementation Planı · **Rol:** Delivery Lead
> **Traceability zorunlu: EVET** — §7 tamamlanmadan §5'e (task listesi) geçilmez.
> **Not:** Bu doküman **tüketici**dir — `02`–`10`'u tüketir, onlara geriye etki yaratmaz. Buradaki düzeltmeler yalnız bu dokümanı hizalar.

> **Aşama 8'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §20.1, yazım turu Y-6b).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-1302 (4) · K-1357 · Okunacak `09` bölümleri:** Kodlama Kılavuzu (`09`) 100 KB'ı aştığı için her görevde bütün okunmaz (`INSTRUCTIONS §9` (7)); `00 §C.6` ("her kodlama görevinde") değişmedi, okunma biçimi bölüm bölümdür. Bu doküman her task satırında okunacak `09` bölümlerini yazar: **çekirdek yedi bölüm** (§1, §2, §5, §6, §23, §24, §25 — toplamı 100 KB'ın altındadır) her satırda "çekirdek" diye anılır ve ayrıca sayılmaz; çekirdeğe **ek** bölümler task'ın dokunduğu alana göre `09` başlık notunun **okuma tablosundan** alınır (ör. göç yazan task §10 ve §12; dış servis çağıran task §11 ve §15; C# yazan her task §3.1 ve §3.2). Task satırının kabul kriterleri ilgili bölümlerin Test maddelerine ve §25'in ilgili satırlarına dayanır; yapım PR'ı okunan bölümleri açıklamasında yazar (`09 §21.3`, `§24` madde 1).
> - **K-1356 · D-01 — eşitlik denetimlerinin Lint adımına girmesi:** `09 §21.4` madde 1'in beş eşitlik denetimi (kütüphane tablosu, parametre envanteri, hata kodu envanteri, geçiş listesi, işaretli işlem listesi) `UnitTests/Consistency/` klasöründe `Consistency` özelliğiyle işaretli birim testleridir; Lint adımı onları koşar, Unit adımı dışarıda bırakır. Özelliğe göre süzen komut D-01'in işidir ve `.github/ci-commands.env`'e yazılır (`05 §2.3.5`); komutların değerini `09` ikinci kez yazmaz.
> - **K-1356 · D-01 — yama sürümü denetiminin `publish.yml`'e girmesi:** "Yama sürümü göç taşımaz" denetimi yayın akışında, etiket imaja basılmadan önce koşar (`09 §21.4` madde 2; `§10.6` madde 7); PR'da koşmaz, çünkü PR sürüm etiketini bilmez. İstisna — veri bozan bir göçün düzeltmesi — etiket mesajındaki `migration-fix: K-<numara>` satırıdır. İlk yayından önce son yayımlanan sürüm bulunmadığından geri alma sınaması atlanır ve atlandığı CI çıktısında yazılır (`09 §21.4` madde 3).
> - **K-1355 · Test projeleri — sahtelerin paylaşımı:** `UnitTests/Fakes/` altındaki sahteleri `IntegrationTests` bağlantılı dosya olarak derler (ikinci kopya yok, iki projeyi bağlayan proje başvurusu yok); çalıştırılabilir bir test projesine başvuru xUnit v3'te sorun çıkarabilir (bilgim tarihli) — test altyapısını kuran ilk task'ta sınanır (`09 §19.1`).
> - **K-1354 · Yükleme betiği, kayıt biçimleyicisi ve maskeleme testleri:** Kodlama Kılavuzu (`09`) üç yapım işi bırakır ve her birinin bu planda bir task karşılığı bulunur: (1) parçalı yükleme betiği — başlatma–parça–durum akışını, 409/404/400/413 yanıtlarını ve ağ hatasında üç kez sürdürmeyi kapsar (`09 §16.2`; `07 §4.31.2`). (2) Uygulama kayıtlarının üç yazıcısı ve satır biçimleyicisi `ApplicationLogFormatter` — alan adlarını ve kapalı olay listesini (`LogEvents`) kapsar (`09 §17`, `§18.1`, `§18.2`; `05 §9.1`). (3) Maskelemenin üç katmanının testleri: yazım, `PathMasker`, biçimleyici ve `ExceptionMasker` (`09 §18.2`). Bu işler izlenebilirlik matrisinin (§7.1) kaynak öğelerinde adıyla geçmeyebilir; task'ın kaynağı `09` bölümüdür. Task'ın nasıl bölünüceğine bu aşama karar verir.

---

## 1. Amaç ve kapsam

> **Ne yazılır:** Tüm işi küçük, bağımsız, sıralı task'lara bölmek. Her task'ın ne olduğu, neye bağımlı olduğu, hangi dokümanlarla yapılacağı ve nasıl kabul edileceği.

## 2. Kaynak envanteri özeti

> **Ne yazılır:** Her kaynak dokümandan çıkarılan öğe sayısı. Traceability matrisinin denetlenebilirliği bu sayılara dayanır.

| Doküman | Öğe sayısı | Kapsam |
|---|---|---|

## 3. Faz tanımları

> **Ne yazılır:** Fazlar ve task aralıkları. Faz sayısı projeye göre değişir; **şekil sabittir**: iskelet → veri → çekirdek servisler → iş mantığı → entegrasyonlar → arayüz → uçtan uca doğrulama.

| Faz | Ad | Kapsam | Task aralığı |
|---|---|---|---|
| F0 | Proje iskeleti | | |
| F1 | Veri katmanı | | |
| F2 | Çekirdek servisler | | |
| F3 | İş mantığı | | |
| F4 | Entegrasyonlar | | |
| F5 | Kullanıcı arayüzü | | |
| F6 | Uçtan uca doğrulama | | |

### 3.1 Faz bağımlılıkları

> **Ne yazılır:** Zincir ve varsa kontrollü paralellik istisnaları.

## 4. Hata sınıflandırması ve çözüm akışı

| Seviye | Tanım | Tespit |
|---|---|---|
| **S1 — Sapma** | Task tamamlandı ama dokümanla uyumsuz | Task doğrulama |
| **S2 — Kırılma** | Bir task başka bir task'ın çıktısını bozuyor | Regresyon (gate check) |
| **S3 — Eksik** | Eşlenmiş öğe implement edilmemiş | Boşluk taraması (gate check) |

**Kritik kural:** Düzeltme task'ı bir sonraki faza ertelenmez — hatanın oluştuğu fazda çözülür.

## 5. Task listesi

### Task yapısı

```
Task TXX: [Task adı]
  Bağımlılık:              [Önceden tamamlanmış olması gereken task'lar]
  Dokümanlar:              [Ajana verilecek dosyalar ve bölümler]
  Kabul kriterleri:        [Ne olduğunda "tamam"]
  Test beklentisi:         [Unit / Integration / Contract / E2E / Yok]
  Doğrulama kontrol listesi: [Validator'ın neye bakacağı]
```

> **Numaralandırma:** Araya task gerektiğinde `TXXa`/`TXXb` (faz aralığını bozmadan). Aynı task'ın devam turu `TXX.1`. Konvansiyonların tamamı: `00_PROJECT_METHODOLOGY.md` §J.
> **Forward pointer:** Henüz yazılmamış bir dokümana verilen referans **açıkça** öyle işaretlenir — bu bir eksik bağımlılık değildir.

### F0 — <Faz adı>

#### T01: <Task adı>
- **Bağımlılık:**
- **Dokümanlar:**
- **Kabul kriterleri:**
- **Test beklentisi:**
- **Doğrulama kontrol listesi:**

## 6. Faz geçiş kapıları

### 6.1 Genel gate check (tüm fazlarda)

> Detaylı prosedür: `.claude/skills/gate-check/SKILL.md`. Burada faz-spesifik ek kontroller listelenir.

### 6.2 Faz-spesifik kontroller

| Faz | Ek kontrol |
|---|---|

## 7. Traceability Matrix (ÖNCE BU)

> **Ne yazılır:** Matris **çift yönlüdür** (00 §C.3). Tek yön yeterli değildir — ileri yön eksik işi, geri yön gereksiz işi yakalar.

### 7.1 İleri izlenebilirlik (kaynak öğe → task)

Tüm önceki doküman çıktıları (entity, endpoint, ekran, entegrasyon, iş kuralı) → task eşlemesi.
**Eşlenmeyen çıktı = eksik task.**

**Alt bölüm haritası (K-1412).** Matris yedi oturumda kurulur (K-1404); O1 haritayı sabitledi. Her alt bölüm `####` başlığıdır; yeni bölüm numarası açılmaz (K-1406 (2)(c)).

| Alt bölüm | İçerik | Kaynak ailesi (birim) | Oturum | Durum |
|---|---|---|---|---|
| 7.1.1 | Veri modeli → task | `06` (117: entity 79 · enum alt bölümü 6 · alt bölüm 32) | O1 | ✓ |
| 7.1.2 | API → task | `07` (208: uç 187 · alt bölüm 21) | O2 | ✓ |
| 7.1.3 | Entegrasyon → task | `08` (30: entegrasyon 3 · çağrısı olmayan bağlantı 1 · alt bölüm 26) | O1 | ✓ |
| 7.1.4 | Arayüz → task | `04` (102: ekran 53 · ortak bileşen 20 · alt bölüm 29) ve `03` (55 alt bölüm) | O3 | ⬚ |
| 7.1.5 | İş kuralı → task — kalın etiketli beş grup: **`02`** · **`05`** · **`09`** · **`10`** · **Devir satırları** | `02 §1`–§3 (297) O4a; `02 §4`–§13 (369) O4b; `05` (136) · `09` (25) · `10` (143) · devir (19) O5 | O4a, O4b, O5 | ⬚ |
| 7.2, §8 | Geri izlenebilirlik, gerekçesiz eklemeler, GAP kararları | task → kaynak | O6 | ⬚ |

**Kaynak kimliğinin biçimi (K-1412).** Satırın ilk hücresi kimlikle başlar; kimliğin ardından başlık ya da ad gelir (en çok 500 bayt/satır). Kapsama betiği yalnız kimliği okur.

| Aile | Kimlik | Örnek |
|---|---|---|
| `06` entity | `06:§4.<n> <Entity>` — `n` §3.1 tablosunun "§4" sütunundan; aynı alt bölümü paylaşan entity'ler adla ayrılır | `06:§4.17 Order` |
| `06` enum alt bölümü ve alt bölüm | `06:§<n.m> <başlık>` | `06:§5.2 Durum setleri ve geçiş kimlikleri` |
| `08` entegrasyon, çağrısı olmayan bağlantı, alt bölüm | `08:§<n.m[.k]> <başlık>` | `08:§2.1.4 …`, `08:§3.1 …` |
| Sonraki aileler | `07 UC-nn`, `04 E-nn`, `OB-nn`, `05 MK-nn`, `02 §n.m.k`, `10 KP-n`, `09 §n`, devir `K-n` (K-1406 (3)) — biçimi kendi oturumu betiğe ekler | |

**Satır sütunları (K-1412).** **Kaynak öğe** (kimlik + ad) · **Task** (aday task kimliği; birden çok ise virgülle; task gerektirmeyen birim `—`) · **Implement edildi** (boş — gate'te dolar) · **Kanıt** (boş — gate'te dolar). Kapsama betiği son iki sütunun bu aşamada boş olduğunu da denetler.

**Task gerektirmeyen birim.** `08:§5.1` — `07`'ye geri yansıyan uçların kaydı: yeni uç çıkmamıştır ve üç mevcut ucun (UC-29, UC-30, UC-80) işi `07` satırlarının task'larındadır (O2: UC-29 ve UC-30 T510'da, UC-80 T311'de; ikisi de `08:§2.1.4` ve `08:§2.1.5` satırlarına eklendi). GAP değildir.

**Aday task kimliği (K-1412).** "Task" sütunu aday kimliktir; adı ve gövdesi `§5`'te yazılır. Kurallar: (1) kimlik K-1407'nin faz aralığından gelir ve aşağıdaki aday tablosunda tanımlıdır (kapsama betiği denetler). (2) F1'in kimlikleri baştan ayrıldı; öteki fazlarda her matris oturumu kendi satırlarının gerek duyduğu kimliği sayaçtaki sonraki boş numaradan alır ve tabloya ekler. (3) Kimlikler faz içi sıra gözetmeden verilebilir; sıra `§5`'in işidir, araya `TXXa` girer (`00 §J`); gerekirse O6 betikli tek bir yeniden numaralama yapar ve kapsama betiği öncesini ve sonrasını koşar. (4) Var olan satıra aday kimlik eklenebilir (ör. UC-80'in F3 task'ı `08:§2.1.5` satırına girer); satır silinmez. (5) F1'in sırası modül akış sırasını (K-1410) bağların kurduğu sırayla daraltır: bir göç, bağlandığı tabloların göçünden sonra gelir (`06 §7.2`'nin 130 bağı betikle sınandı — sıra ihlali 0); Yönetim ve Üyelik ile iptal–cayma–iade ve Ödeme bu yüzden akış sırasından ayrılır.

| Faz | Son ayrılan | Sonraki boş |
|---|---|---|
| F0 | T01 | T02 |
| F1 | T120 | T121 |
| F2 | T217 | T218 |
| F3 | T314 | T315 |
| F4 | T410 | T411 |
| F5 | T539 | T540 |
| F6 | T601 | T602 |

**Aday task tablosu (O1).** F1 eksiksiz ayrıldı (79 entity, en çok 8 entity/göç — K-1409; sıra yukarıdaki (5)); öteki fazlarda yalnız `06`, `07` ve `08` satırlarının gerek duyduğu task'lar vardır (O2 55 aday ekledi: F0 1, F2 12, F3 4, F5 38; toplam 102) — sonraki oturumlar ekler.

| Aday | Faz | Kısa ad | Modül | Kaynak birimi |
|---|---|---|---|---|
| T01 | F0 | Canlılık ucu: uygulama ve veritabanı yanıtı, 200 ya da 503, gövdesiz | Ortak altyapı | `07` UC-81 (1) |
| T101 | F1 | Veri modeli temeli: anahtarlar, türler, adlandırma, sürüm sütunu, metin karşılaştırma, enum biçimi, bağların silme davranışı | Ortak | `06 §2.1`–§2.6, §2.8–§2.10, §5.1 (10) |
| T102 | F1 | Enum tanımları ve değer dönüşümü | Ortak | `06 §5.2`–§5.6 (5) |
| T103 | F1 | Başlangıç listeleri, firma ve yasal metin sürümü: Province, Country, Company, CompanySettings, CompanyDeliveryProvince, LegalTextVersion | Ayarlar | 6 entity |
| T104 | F1 | Ürün çekirdeği ve görsel: Image, Product, Option, OptionValue, Variant, Category, ProductCategory, ProductImage | Katalog | 8 entity |
| T105 | F1 | Fiyat, kupon ve dijital dosya: PriceHistory, Discount, Coupon, DigitalFile | Katalog | 4 entity |
| T106 | F1 | Yönetici hesabı, davet ve işlem izi: Admin, AdminInvitation, AuditLog | Yönetim ve işlem izi | 3 entity |
| T107 | F1 | Müşteri hesabı: Customer, AddressBook, PendingEmailChange | Üyelik | 3 entity |
| T108 | F1 | Oturum, bağlantı, deneme ve giriş kayıtları: Session, LinkRecord, AttemptRecord, RecognizedBrowserMark, LoginRecord | Ortak altyapı | 5 entity |
| T109 | F1 | Sepet ve sipariş çekirdeği: Cart, CartItem, Order, OrderItem, TransitionRecord, InternalNote, CheckboxAcceptance, CheckboxAcceptanceOrderItem | Sepet ve sipariş | 8 entity |
| T110 | F1 | Ayırma ve düzeltme geçmişleri: StockReservation, OrderEmailHistory, OrderAddressHistory | Sepet ve sipariş | 3 entity |
| T111 | F1 | Kalem kayıtları: RefundIban, DefectClaim, Cancellation, ItemRemoval, LateDeliveryTermination, Withdrawal, Return, NotReturnedClosure | İptal, cayma ve iade | 8 entity |
| T112 | F1 | Ödeme ve geri ödeme kayıtları: PaymentRecord, Refund, RefundRecord, InboundNotificationRecord | Ödeme | 4 entity |
| T113 | F1 | İade reddi ve IBAN isteği: ReturnRejection, RefundIbanRequest | İptal, cayma ve iade | 2 entity |
| T114 | F1 | İçerik kayıtları ve marka: AboutPage, Offering, PortfolioItem, FaqItem, CustomPage, Announcement, SocialLink, Branding | Kurumsal içerik | 8 entity |
| T115 | F1 | Şube, içerik–ürün bağları ve iletişim talebi: Branch, OfferingProduct, PortfolioItemProduct, ContactRequest | Kurumsal içerik | 4 entity |
| T116 | F1 | Galeri bağları: AboutPageImage, OfferingImage, PortfolioItemImage, FaqItemImage, CustomPageImage | Kurumsal içerik | 5 entity |
| T117 | F1 | Gönderim kaydı ve kanal hâli: DispatchRecord, ChannelStateRecord | Bildirim | 2 entity |
| T118 | F1 | Zamanlayıcı, yedek, imha ve anahtar kayıtları: Job, HeartbeatRecord, OutageRecord, BackupStatusRecord, DataProtectionKey, DisposalRecord | Ortak altyapı | 6 entity |
| T119 | F1 | Başlangıç verisi göçü ve seed olmayanların sınırı | Ortak | `06 §8.1`, §8.4 (2) |
| T120 | F1 | Model eşitlik denetimleri: ilişki diyagramı, tekillik, yabancı anahtar, kontrol kısıtı, dizin, denormalizasyon envanteri | Ortak | `06 §2.7`, §6.1–§6.6, §7.1–§7.6 (13) |
| T201 | F2 | İmha düzenleyicisi, süre ayarları ve gecelik temizlik | Ortak altyapı | `06 §9.1`, §9.4, §9.5 (3) |
| T202 | F2 | Dış servislerin bağlantı katmanı: ortak yüz, hata sınıfı, zaman aşımı, sahte sağlayıcılar | Ortak altyapı | `08 §3.1` |
| T203 | F2 | İmzalı makine ucu altyapısı: imza denetimi, gövde tavanı | Ortak altyapı | `08 §3.2` |
| T204 | F2 | Dış servis çökmesi davranışı ve sahtelerle sınama | Ortak altyapı | `08 §3.3` |
| T205 | F2 | Açılışta veritabanı fail-fast denetimi | Ortak altyapı | `06 §8.2` |
| T206 | F2 | Adres yapısı, önekler ve sayfa–form işleyici kalıbı: gönder–yönlendir–göster, statik dosya sürüm izi | Ortak altyapı | `07 §2.1`, `§2.2`, `§2.10` (3) |
| T207 | F2 | Kimlik şemaları ve erişim reddi: panel ve müşteri şeması, oturumsuz geliş, sahiplik ve erişim anahtarlı kök | Ortak altyapı | `07 §2.3` (1) |
| T208 | F2 | Sürümün ve gösterilen değerin formda taşınması, koşullu güncelleme | Ortak altyapı | `07 §2.4` (1) |
| T209 | F2 | Tek kullanımlık değerler, tekrar gönderim, sahteciliğe karşı koruma ve güvenlik başlıkları | Ortak altyapı | `07 §2.5` (1) |
| T210 | F2 | Liste kalıbı: sayfalama, süzgeç parametreleri, geçersiz değer, arama motorlarına kapalı sayfa | Ortak altyapı | `07 §2.6` (1) |
| T211 | F2 | Model bağlama kalıbı: form girdi, sorgu ve makine ucu modeli türleri | Ortak altyapı | `07 §5.1` (1) |
| T212 | F2 | Hata gösterimi ve HTTP durumları: sayfa, form ve makine ucu; özel hâller | Ortak altyapı | `07 §2.7`, `§6.1`, `§6.4` (3) |
| T213 | F2 | Hata kodu kayıt defteri | Ortak altyapı | `07 §6.2` (1) |
| T214 | F2 | Kısa süreli bildirim kodları | Ortak altyapı | `07 §6.3` (1) |
| T215 | F2 | Hız sınırı, deneme limitleri ve gövde tavanı | Ortak altyapı | `07 §2.8` (1) |
| T216 | F2 | Tarih, sayı ve para biçimi: istek ve gösterim | Ortak altyapı | `07 §2.9` (1) |
| T217 | F2 | Yapılabilir işlemlerin sunucuda üretimi ve POST işleyicisinde yeniden denetim | Ortak altyapı | `07 §8.1` (1) |
| T301 | F3 | Siparişin imha parçası | Sepet ve sipariş | `06 §9.2` |
| T302 | F3 | Öteki kayıtların süreyle imhası | Modüller | `06 §9.3` |
| T303 | F3 | İlk yönetici yönetim komutu | Yönetim ve işlem izi | `06 §8.3` |
| T304 | F3 | Alıcı başına bildirim gönderim sınırı | Bildirim | `08 §2.2.4` |
| T305 | F3 | E-posta ortak kalıbı ve gövde üretimi | Bildirim | `08 §2.2.5` |
| T306 | F3 | Müşteri bildirimleri B-1…B-8 | Bildirim | `08 §2.2.6` (yarısı) |
| T307 | F3 | Müşteri bildirimleri B-9…B-16 | Bildirim | `08 §2.2.6` (yarısı) |
| T308 | F3 | Firma bildirimleri F-1…F-6 | Bildirim | `08 §2.2.7` |
| T309 | F3 | Hesap ve yönetim e-postaları | Bildirim | `08 §2.2.8` |
| T310 | F3 | Kargo şirketi listesi ve takip bağlantısı kalıpları | Sepet ve sipariş | `08 §2.4` |
| T311 | F3 | Ödeme sağlayıcısının bildirim ucu: imza, alan eşlemesi ve durum makinesine aktarım | Ödeme | `07` UC-80 (1) |
| T312 | F3 | Dosya uçları: imzalı indirme ucu ve parçalı yükleme ucu | Katalog | `07` UC-37, UC-83 (2) · `07 §5.4` (1) |
| T313 | F3 | Site haritası ve robots.txt | Ortak altyapı | `07` UC-79 (1) |
| T314 | F3 | Durum ucu: uyarı koşulları ve 500 sayacı | Ortak altyapı | `07` UC-82 (1) |
| T401 | F4 | PayTR: anahtarlar, ödemenin başlatılması, dönüş ve çerçeve | Ödeme | `08 §2.1`, §2.1.1, §2.1.2, §2.1.4 |
| T402 | F4 | PayTR: bildirim imzası ve alan eşlemesi | Ödeme | `08 §2.1.5` |
| T403 | F4 | PayTR: son sorgu ve tek tahsilat | Ödeme | `08 §2.1.6`, §2.1.7 |
| T404 | F4 | PayTR: bağlantı hatası ve ayrım | Ödeme | `08 §2.1.8` |
| T405 | F4 | PayTR: kart iadesi | Ödeme | `08 §2.1.9` |
| T406 | F4 | PayTR: 3D Secure kurulum şartı, test ve canlı kanıt | Ödeme | `08 §2.1.3`, §2.1.10 |
| T407 | F4 | SES: gönderim ve sonuç sınıfları | Bildirim | `08 §2.2`, §2.2.1, §2.2.2 |
| T408 | F4 | SES: geri dönen adreslerin öğrenilmesi | Bildirim | `08 §2.2.3` |
| T409 | F4 | Google: giriş, kayıt ve yeniden doğrulama bağlayıcısı | Üyelik | `08 §2.3` |
| T410 | F4 | Dış olgu varsayımlarının doğrulama kanıtları (DV) | Ortak | `08 §4.1`, §5.2 |
| T501 | F5 | Yerli üretim logosu varlığı ve gösterimi | Katalog | `08 §4.3` |
| T502 | F5 | Vitrin: ana sayfa ve kategori sayfası (E-01, E-02) | Katalog | `07` UC-01, UC-02 (2) |
| T503 | F5 | Vitrin: arama sonuçları, ürün sayfası ve sepete ekleme (E-03, E-04; arşivdeki E-05 aynı ucun hâlidir) | Katalog | `07` UC-03…UC-05 (3) |
| T504 | F5 | Hata sayfaları: bulunamayan adres ve beklenmeyen hata (E-06, 500) | Ortak altyapı | `07` UC-06, UC-78 (2) |
| T505 | F5 | İçerik liste ve içerik sayfaları (E-07, E-08) | Kurumsal içerik | `07` UC-07…UC-12 (6) |
| T506 | F5 | Sık sorulan sorular ve iletişim sayfası ve formu (E-09, E-10) | Kurumsal içerik | `07` UC-13…UC-15 (3) |
| T507 | F5 | Yasal metin sayfaları ve panelde yönetimi (E-11, E-47) | Ayarlar | `07` UC-16…UC-18, UC-102, UC-173, UC-174 (6) |
| T508 | F5 | Sepet (E-12) | Sepet ve sipariş | `07` UC-19…UC-21 (3) |
| T509 | F5 | Ödeme adımı (E-13) | Sepet ve sipariş | `07` UC-22…UC-27 (6) |
| T510 | F5 | Sipariş teyit ve bekleme ekranı, ödeme sağlayıcısına geçiş ve dönüş (E-14) | Ödeme | `07` UC-28…UC-30 (3) |
| T511 | F5 | Sipariş takibi girişi, sipariş sayfası ve indirme bağlantısı (E-15, E-16) | Sepet ve sipariş | `07` UC-31…UC-34, UC-36 (5) |
| T512 | F5 | İptal ve gecikme feshi ekranı, geri ödeme IBAN girişi (E-17, E-16 IBAN formu) | İptal, cayma ve iade | `07` UC-35, UC-38…UC-41 (5) |
| T513 | F5 | Cayma beyanı ve ayıp talebi ekranları (E-18, E-19) | İptal, cayma ve iade | `07` UC-42…UC-46 (5) |
| T514 | F5 | Kayıt ve doğrulama bağlantısının inişi (E-20, E-21) | Üyelik | `07` UC-47…UC-49, UC-52, UC-53 (5) |
| T515 | F5 | Müşteri girişi ve Google ile giriş (E-22) | Üyelik | `07` UC-50, UC-51, UC-54, UC-55 (4) |
| T516 | F5 | Şifre sıfırlama ve yeniden doğrulama (E-23, E-24) | Üyelik | `07` UC-56…UC-62 (7) |
| T517 | F5 | Hesap — profil ve güvenlik (E-25) | Üyelik | `07` UC-63…UC-69 (7) |
| T518 | F5 | Adres defteri ve sipariş geçmişi (E-26, E-27) | Üyelik | `07` UC-70…UC-74 (5) |
| T519 | F5 | E-posta değişikliğini geri alma ve müşteri çıkışı (E-28) | Üyelik | `07` UC-75…UC-77 (3) |
| T520 | F5 | Panel girişi, şifre sıfırlama, davet kabulü ve yönetici çıkışı (E-29, E-30) | Yönetim ve işlem izi | `07` UC-84, UC-85, UC-109…UC-113 (7) |
| T521 | F5 | Panel ana sayfası ve yönetici hesabının doğrulama ekranları (E-31, E-54) | Yönetim ve işlem izi | `07` UC-86, UC-108, UC-185…UC-187 (5) |
| T522 | F5 | Yönetici hesapları ve işlem izi (E-49, E-52) | Yönetim ve işlem izi | `07` UC-103, UC-106, UC-175…UC-177 (5) |
| T523 | F5 | Yöneticinin kendi hesabı (E-50) | Yönetim ve işlem izi | `07` UC-104, UC-178…UC-181 (5) |
| T524 | F5 | Satış özeti ve dışa aktarma (E-51, E-53) | Yönetim ve işlem izi | `07` UC-105, UC-107, UC-182…UC-184 (5) |
| T525 | F5 | Ürün listesi ve ürün formu, görsel yükleme (E-32, E-33) | Katalog | `07` UC-87, UC-88, UC-114…UC-117 (6) |
| T526 | F5 | Kategori ağacı ve kuponlar (E-34, E-35) | Katalog | `07` UC-89, UC-90, UC-118…UC-122 (7) |
| T527 | F5 | Sipariş listesi, sipariş ayrıntısı ve durum geçişleri (E-36, E-37) | Sepet ve sipariş | `07` UC-91, UC-92, UC-123…UC-128 (8) |
| T528 | F5 | Sipariş ayrıntısı: firma iptali, bildirim kaydı, kalem çıkarma, iade malı ve ayıp talebi (E-37) | İptal, cayma ve iade | `07` UC-129, UC-130, UC-138, UC-140…UC-142, UC-144, UC-145 (8) |
| T529 | F5 | Sipariş ayrıntısı: geri ödeme işlemleri (E-37) | Ödeme | `07` UC-131…UC-137 (7) |
| T530 | F5 | Sipariş ayrıntısı: adres, kargo, teslim tarihi, durum geçişi ve e-posta düzeltmeleri (E-37) | Sepet ve sipariş | `07` UC-146…UC-151 (6) |
| T531 | F5 | Sipariş ayrıntısı: hizmet kalemi, indirme hakkı, iç not ve e-posta yeniden gönderim (E-37) | Sepet ve sipariş | `07` UC-139, UC-143, UC-152…UC-154 (5) |
| T532 | F5 | Talepler ve iletişim talebi ayrıntısı (E-38, E-39) | Kurumsal içerik | `07` UC-93, UC-94, UC-155, UC-156 (4) |
| T533 | F5 | Üye kaydı görünümü (E-40) | Üyelik | `07` UC-95, UC-157 (2) |
| T534 | F5 | Kurumsal içerik ve marka yönetimi (E-41, E-43) | Kurumsal içerik | `07` UC-96, UC-98, UC-158…UC-161, UC-163 (7) |
| T535 | F5 | Ana sayfa ve menü, firma kimliği ve satış (E-42, E-44) | Ayarlar | `07` UC-97, UC-99, UC-162, UC-164…UC-166 (6) |
| T536 | F5 | Ödeme yöntemleri, kargo, süreler ve varsayılanlar (E-45, E-46) | Ayarlar | `07` UC-100, UC-101, UC-167…UC-172 (8) |
| T537 | F5 | Vitrin çerçevesinin modeli: marka, menü, duyuru şeridi, hesap alanı, sepet sayısı | Ortak altyapı | `07 §5.5` (1) |
| T538 | F5 | Uç ve model eşitlik denetimleri: uç envanteri, form girdi ve sorgu modelleri | Ortak altyapı | `07 §5.2`, `§5.3` (2) |
| T539 | F5 | Betiksiz yol: JavaScript kapalıyken temel akışlar ve betikli iyileştirme | Ortak altyapı | `07 §8.2` (1) |
| T601 | F6 | `DEPLOY_RUNBOOK` provası, dış sistemde kayıtlı adresler ve yansıyan tablolar | Ortak | `06 §8.5`, `08 §4.2` |

**Kapsama komutları (Git Bash; depo kökünden).** Sayımlar aşağıdaki sayılarla aynı çıkmalı, her iki betik de çıkış kodu 0 vermelidir (eksik, fazla, yinelenen 0).

```sh
python -I scripts/doc-scans/11_matris_envanter.py . /tmp/envanter.txt    # kaynak envanteri: 06 117, 07 208, 08 30 (ve öteki aileler)
python -I scripts/doc-scans/11_matris_kapsama.py . /tmp/kapsama.txt      # matris satırları ↔ beklenen küme (dolu aileler: 06, 07, 08)
```

**Okuma derinliği (O1).** `06`: §3.1 entity tablosu bütün, §2/§5–§9 alt bölüm başlıkları ve ilk paragrafları, §8 bütün, §7.2'nin 130 bağı betikle; §4 entity tanımları okunmadı — kaynak birim §3.1'in satırıdır. `08`: §1 bütün, §2–§5 başlıkları ve her başlığın ilk iki paragrafı (satır başına ilk 300–380 karakter; kesilerek okundu).

#### 7.1.1 Veri modeli → task

| Kaynak öğe | Task | Implement edildi | Kanıt |
|---|---|---|---|
| 06:§2.1 Şablonun altı dersi ve bu projedeki yeri | T101 | | |
| 06:§2.2 Silme kategorileri ve bağların silme davranışı | T101 | | |
| 06:§2.3 Anahtarlar ve dışa görünen kimlikler | T101 | | |
| 06:§2.4 Adlandırma ve şema | T101 | | |
| 06:§2.5 Türler ve sayısal ölçek | T101 | | |
| 06:§2.6 Eşzamanlılık — sürüm değeri ve koşullu güncelleme | T101 | | |
| 06:§2.7 Denormalizasyon envanteri | T120 | | |
| 06:§2.8 Metin karşılaştırma, sıralama, oluşturulma anı ve boş değer | T101 | | |
| 06:§2.9 Enum'ların biçimi | T101 | | |
| 06:§2.10 Uzun metnin saklanışı | T101 | | |
| 06:§4.1 Company | T103 | | |
| 06:§4.2 CompanySettings | T103 | | |
| 06:§4.2 CompanyDeliveryProvince | T103 | | |
| 06:§4.3 Province | T103 | | |
| 06:§4.4 Country | T103 | | |
| 06:§4.5 Product | T104 | | |
| 06:§4.6 Variant | T104 | | |
| 06:§4.7 Option | T104 | | |
| 06:§4.8 OptionValue | T104 | | |
| 06:§4.9 Category | T104 | | |
| 06:§4.10 ProductCategory | T104 | | |
| 06:§4.11 PriceHistory | T105 | | |
| 06:§4.12 Discount | T105 | | |
| 06:§4.13 Coupon | T105 | | |
| 06:§4.14 StockReservation | T110 | | |
| 06:§4.15 Cart | T109 | | |
| 06:§4.16 CartItem | T109 | | |
| 06:§4.17 Order | T109 | | |
| 06:§4.18 OrderItem | T109 | | |
| 06:§4.19 TransitionRecord | T109 | | |
| 06:§4.20 InternalNote | T109 | | |
| 06:§4.21 CheckboxAcceptance | T109 | | |
| 06:§4.21 CheckboxAcceptanceOrderItem | T109 | | |
| 06:§4.22 Cancellation | T111 | | |
| 06:§4.23 ItemRemoval | T111 | | |
| 06:§4.24 LateDeliveryTermination | T111 | | |
| 06:§4.25 Withdrawal | T111 | | |
| 06:§4.26 Return | T111 | | |
| 06:§4.27 ReturnRejection | T113 | | |
| 06:§4.28 NotReturnedClosure | T111 | | |
| 06:§4.29 RefundIbanRequest | T113 | | |
| 06:§4.30 DefectClaim | T111 | | |
| 06:§4.31 RefundIban | T111 | | |
| 06:§4.32 PaymentRecord | T112 | | |
| 06:§4.33 Refund | T112 | | |
| 06:§4.34 RefundRecord | T112 | | |
| 06:§4.35 InboundNotificationRecord | T112 | | |
| 06:§4.36 Customer | T107 | | |
| 06:§4.37 Admin | T106 | | |
| 06:§4.38 AdminInvitation | T106 | | |
| 06:§4.39 Session | T108 | | |
| 06:§4.40 PendingEmailChange | T107 | | |
| 06:§4.41 RecognizedBrowserMark | T108 | | |
| 06:§4.42 LinkRecord | T108 | | |
| 06:§4.43 AttemptRecord | T108 | | |
| 06:§4.44 LoginRecord | T108 | | |
| 06:§4.45 AddressBook | T107 | | |
| 06:§4.46 AboutPage | T114 | | |
| 06:§4.47 Offering | T114 | | |
| 06:§4.48 PortfolioItem | T114 | | |
| 06:§4.49 FaqItem | T114 | | |
| 06:§4.50 Branch | T115 | | |
| 06:§4.51 CustomPage | T114 | | |
| 06:§4.52 Announcement | T114 | | |
| 06:§4.53 OfferingProduct | T115 | | |
| 06:§4.53 PortfolioItemProduct | T115 | | |
| 06:§4.54 Branding | T114 | | |
| 06:§4.55 SocialLink | T114 | | |
| 06:§4.56 LegalTextVersion | T103 | | |
| 06:§4.57 ContactRequest | T115 | | |
| 06:§4.58 DispatchRecord | T117 | | |
| 06:§4.59 Job | T118 | | |
| 06:§4.60 ChannelStateRecord | T117 | | |
| 06:§4.61 OrderEmailHistory | T110 | | |
| 06:§4.62 OrderAddressHistory | T110 | | |
| 06:§4.63 AuditLog | T106 | | |
| 06:§4.64 DisposalRecord | T118 | | |
| 06:§4.65 HeartbeatRecord | T118 | | |
| 06:§4.66 OutageRecord | T118 | | |
| 06:§4.67 BackupStatusRecord | T118 | | |
| 06:§4.68 DataProtectionKey | T118 | | |
| 06:§4.69 Image | T104 | | |
| 06:§4.70 DigitalFile | T105 | | |
| 06:§4.71 ProductImage | T104 | | |
| 06:§4.71 AboutPageImage | T116 | | |
| 06:§4.71 OfferingImage | T116 | | |
| 06:§4.71 PortfolioItemImage | T116 | | |
| 06:§4.71 FaqItemImage | T116 | | |
| 06:§4.71 CustomPageImage | T116 | | |
| 06:§5.1 Biçim, kaynak ve okunuş | T101 | | |
| 06:§5.2 Durum setleri ve geçiş kimlikleri | T102 | | |
| 06:§5.3 Sözlüğün listeleri | T102 | | |
| 06:§5.4 Teknik kayıtların listeleri | T102 | | |
| 06:§5.5 Kimlikli ürün listeleri | T102 | | |
| 06:§5.6 Kayıt düzeyindeki kapalı değerler | T102 | | |
| 06:§6.1 Okunuş | T120 | | |
| 06:§6.2 Firma, ayarlar ve katalog | T120 | | |
| 06:§6.3 Sipariş, kalem kayıtları ve ödeme | T120 | | |
| 06:§6.4 Hesaplar ve oturum | T120 | | |
| 06:§6.5 İçerik, yasal metinler ve dosyalar | T120 | | |
| 06:§6.6 Bağsız sistem kayıtları | T120 | | |
| 06:§7.1 Tekillikler | T120 | | |
| 06:§7.2 Yabancı anahtarlar ve silme davranışları | T120 | | |
| 06:§7.3 Kontrol kısıtları | T120 | | |
| 06:§7.4 Koşullu güncellemelerin dayandığı kısıtlar | T120 | | |
| 06:§7.5 Sorgu dizinleri | T120 | | |
| 06:§7.6 Hacim kabulleri | T120 | | |
| 06:§8.1 Seed | T119 | | |
| 06:§8.2 Fail-fast zorunlu | T205 | | |
| 06:§8.3 Kuruluma özgü | T303 | | |
| 06:§8.4 Veritabanında olmayanlar | T119 | | |
| 06:§8.5 `DEPLOY_RUNBOOK`'a yansıma | T601 | | |
| 06:§9.1 Sürelerin başlangıç değerleri | T201 | | |
| 06:§9.2 Siparişin imha parçası — alan listesi | T301 | | |
| 06:§9.3 Öteki kayıtların süreyle imhası | T302 | | |
| 06:§9.4 Anlık silmeler ve gecelik temizlik | T201 | | |
| 06:§9.5 Arşiv tablosu yoktur; yedeklerdeki kopyalar | T201 | | |

#### 7.1.2 API → task

> **Eşleme kuralı (K-1415, K-1416).** Sayfa ve form ucu (181 uç) kendi ekranının F5 task'ına girer: en çok 2 ekran ve 8 uç, tek modül (K-1409); sipariş ayrıntısı (E-37) uç sınırı ve modül ölçütü yüzünden beş task'a bölünür (T527–T531). Sayfasız uçlar F3'tedir (T311–T314), canlılık ucu F0'dadır (T01; K-1407 F0 çıkışı). `§2`, `§5` ve `§6`'nın ortak kalıpları F2'dedir (T206–T217); `§2.10` T206'ya, `§5.2`–`§5.3` eşitlik denetimi T538'e girer. İş mantığı bu satırlarda değil `02` satırlarındadır (O4a, O4b).

**Okuma derinliği (O2).** `07 §3.2`: 187 uç satırı bütün (her hücre). `§3.3`, `§3.4` ve `§4`'ün girişi bütün. `§4.33`, `§4.34` bütün. `§2`, `§5`, `§6`, `§8`: alt bölüm başlıkları, ilk paragraflar ve kalın maddelerin başlıkları (kesilerek okundu); `§2.10` bütün. **Okunmadı:** `§4` ve `§9`'un uç tanımları (`§4.33` ve `§4.34` dışında), `§5.2`–`§5.5` ve `§6.2`–`§6.3` tablolarının satırları — kaynak birim `§3.2`'nin satırı ve alt bölümün başlığıdır; sonraki oturum örneklemle doğrular.

| Kaynak öğe | Task | Implement edildi | Kanıt |
|---|---|---|---|
| 07:§2.1 Adres yapısı ve önekler | T206 | | |
| 07:§2.2 Sayfa ve form ucunun biçimi | T206 | | |
| 07:§2.3 Kimlik doğrulama ve erişim reddi | T207 | | |
| 07:§2.4 Sürümün ve gösterilen değerin formda taşınması | T208 | | |
| 07:§2.5 Tek kullanımlık değerler, tekrar gönderim ve sahteciliğe karşı koruma | T209 | | |
| 07:§2.6 Listeler | T210 | | |
| 07:§2.7 Hata gösterimi ve HTTP durumları | T212 | | |
| 07:§2.8 Hız sınırı ve gövde tavanı | T215 | | |
| 07:§2.9 Tarih, sayı ve para biçimi | T216 | | |
| 07:§2.10 Sürümleme | T206 | | |
| 07 UC-01 GET `/` — Ana sayfa | T502 | | |
| 07 UC-02 GET `/kategori/{Slug}` — Kategori sayfası | T502 | | |
| 07 UC-03 GET `/arama` — Arama sonuçları | T503 | | |
| 07 UC-04 GET `/urun/{Slug}` — Ürün adresi | T503 | | |
| 07 UC-05 POST `/urun/{Slug}?handler=AddCartItem` — Sepete ekleme | T503 | | |
| 07 UC-06 — her isteğin yanıtı (404; §2.7.1) — Bulunamayan adres | T504 | | |
| 07 UC-07 GET `/hizmetler` — Hizmetlerimiz listesi | T505 | | |
| 07 UC-08 GET `/referanslar` — Referanslarımız listesi | T505 | | |
| 07 UC-09 GET `/hakkimizda` — Hakkımızda | T505 | | |
| 07 UC-10 GET `/hizmet/{Slug}` — Hizmet tanıtımı sayfası | T505 | | |
| 07 UC-11 GET `/referans/{Slug}` — Referans iş sayfası | T505 | | |
| 07 UC-12 GET `/sayfa/{Slug}` — Genel sayfa | T505 | | |
| 07 UC-13 GET `/sss` — Sık sorulan sorular | T506 | | |
| 07 UC-14 GET `/iletisim` — İletişim sayfası | T506 | | |
| 07 UC-15 POST `/iletisim?handler=SendContactRequest` — İletişim formunun gönderimi | T506 | | |
| 07 UC-16 GET `/aydinlatma-metni` — Aydınlatma metni | T507 | | |
| 07 UC-17 GET `/cerez-politikasi` — Çerez politikası | T507 | | |
| 07 UC-18 GET `/islem-rehberi` — İşlem rehberi | T507 | | |
| 07 UC-19 GET `/sepet` — Sepet | T508 | | |
| 07 UC-20 POST `/sepet?handler=UpdateCartItem` — Sepet kaleminin adedini değiştirme | T508 | | |
| 07 UC-21 POST `/sepet?handler=RemoveCartItem` — Sepet kalemini çıkarma | T508 | | |
| 07 UC-22 GET `/odeme` — Ödeme adımı | T509 | | |
| 07 UC-23 POST `/odeme?handler=ApplyCoupon` — Kuponu uygulama ya da değiştirme | T509 | | |
| 07 UC-24 POST `/odeme?handler=RemoveCoupon` — Kuponu kaldırma | T509 | | |
| 07 UC-25 POST `/odeme?handler=Recalculate` — Ödeme adımının ara gönderimi | T509 | | |
| 07 UC-26 POST `/odeme?handler=Leave` — Ödeme adımından girişe ya da sepete | T509 | | |
| 07 UC-27 POST `/odeme?handler=PlaceOrder` — Siparişi onaylama | T509 | | |
| 07 UC-28 GET `{kök}/teyit` — Sipariş teyit ve bekleme ekranı | T510 | | |
| 07 UC-29 POST `{kök}/teyit?handler=StartPayment` — Ödeme sağlayıcısına geçiş | T510 | | |
| 07 UC-30 GET `/odeme/donus/{erişim anahtarı}` — Ödeme sağlayıcısından dönüş | T510 | | |
| 07 UC-31 GET `/siparis-takibi` — Sipariş takibi girişi | T511 | | |
| 07 UC-32 POST `/siparis-takibi?handler=FindOrder` — Sipariş takibi sorgusu | T511 | | |
| 07 UC-33 GET `/siparis/{sipariş numarası}` — Sipariş sayfası | T511 | | |
| 07 UC-34 GET `/siparis/erisim/{erişim anahtarı}` — Erişim anahtarlı sipariş adresi | T511 | | |
| 07 UC-35 POST `{kök}?handler=SaveRefundIban` — Geri ödeme IBAN'ının girişi ve düzeltmesi | T512 | | |
| 07 UC-36 POST `{kök}?handler=DownloadDigitalFile` — İndirme bağlantısının üretimi | T511 | | |
| 07 UC-37 GET `/indir/{imzalı değer}` — İndirme ucu | T312 | | |
| 07 UC-38 GET `{kök}/iptal` ve 1 adres daha — İptal ve gecikme feshi ekranı | T512 | | |
| 07 UC-39 POST `{kök}/iptal?handler=CancelOrder` — Siparişin iptali | T512 | | |
| 07 UC-40 POST `{kök}/iptal?handler=CancelOrderItems` — Kalem iptali | T512 | | |
| 07 UC-41 POST `{kök}/gecikme-feshi?handler=SubmitLateDeliveryTermination` — Gecikme feshi | T512 | | |
| 07 UC-42 GET `{kök}/cayma` — Cayma beyanı ekranı | T513 | | |
| 07 UC-43 POST `{kök}/cayma?handler=SubmitWithdrawal` — Cayma beyanı | T513 | | |
| 07 UC-44 GET `{kök}/ayip-talebi` — Ayıp talebi ekranı | T513 | | |
| 07 UC-45 POST `{kök}/ayip-talebi?handler=SubmitDefectClaim` — Ayıp talebinin gönderimi | T513 | | |
| 07 UC-46 POST `{kök}/ayip-talebi?handler=ReopenDefectClaim` — Ayıp talebinin yeniden açılması | T513 | | |
| 07 UC-47 GET `/kayit` — Kayıt ekranı | T514 | | |
| 07 UC-48 POST `/kayit?handler=Register` — Kayıt | T514 | | |
| 07 UC-49 POST `/kayit?handler=ResendVerificationLink` ve 1 adres daha — Kayıt doğrulama bağlantısını yeniden isteme | T514 | | |
| 07 UC-50 POST `/kayit?handler=StartSocialLogin` ve 1 adres daha — Google ile giriş | T515 | | |
| 07 UC-51 GET `/giris/google` — Google dönüşü | T515 | | |
| 07 UC-52 GET `/dogrulama/{anahtar}` — Doğrulama bağlantısının inişi | T514 | | |
| 07 UC-53 POST `/dogrulama/{anahtar}?handler=VerifyEmail` — Doğrulama | T514 | | |
| 07 UC-54 GET `/giris` — Müşteri girişi ekranı | T515 | | |
| 07 UC-55 POST `/giris?handler=SignIn` — Müşteri girişi | T515 | | |
| 07 UC-56 GET `/sifre-sifirlama` — Şifre sıfırlama | T516 | | |
| 07 UC-57 POST `/sifre-sifirlama?handler=RequestPasswordReset` — Şifre sıfırlama isteği | T516 | | |
| 07 UC-58 GET `/sifre-sifirlama/{anahtar}` — Sıfırlama bağlantısının inişi | T516 | | |
| 07 UC-59 POST `/sifre-sifirlama/{anahtar}?handler=ResetPassword` — Yeni şifrenin kurulması | T516 | | |
| 07 UC-60 GET `/hesap/yeniden-dogrulama` — Yeniden doğrulama ekranı | T516 | | |
| 07 UC-61 POST `/hesap/yeniden-dogrulama?handler=Reauthenticate` — Şifreyle yeniden doğrulama | T516 | | |
| 07 UC-62 POST `/hesap/yeniden-dogrulama?handler=StartSocialReauthentication` — Google ile yeniden doğrulama | T516 | | |
| 07 UC-63 GET `/hesap` — Hesap | T517 | | |
| 07 UC-64 POST `/hesap?handler=ChangeName` — Adı değiştirme | T517 | | |
| 07 UC-65 POST `/hesap?handler=ChangePassword` — Şifreyi değiştirme | T517 | | |
| 07 UC-66 POST `/hesap?handler=ChangeEmail` — E-postayı değiştirme | T517 | | |
| 07 UC-67 POST `/hesap?handler=ResendEmailChangeLink` — Bekleyen e-posta değişikliğinin bağlantısını yeniden isteme | T517 | | |
| 07 UC-68 POST `/hesap?handler=DeleteAccount` — Hesabı silme | T517 | | |
| 07 UC-69 GET `/hesap/silindi` — Hesap silmenin sonuç hâli | T517 | | |
| 07 UC-70 GET `/hesap/adresler` — Adres defteri | T518 | | |
| 07 UC-71 POST `/hesap/adresler?handler=CreateAddressBook` — Adres ekleme | T518 | | |
| 07 UC-72 POST `/hesap/adresler?handler=UpdateAddressBook` — Adres düzenleme | T518 | | |
| 07 UC-73 POST `/hesap/adresler?handler=DeleteAddressBook` — Adres silme | T518 | | |
| 07 UC-74 GET `/hesap/siparisler` — Sipariş geçmişi | T518 | | |
| 07 UC-75 GET `/geri-al/{anahtar}` — Geri alma bağlantısının inişi | T519 | | |
| 07 UC-76 POST `/geri-al/{anahtar}?handler=RevertPendingEmailChange` — E-posta değişikliğini geri alma | T519 | | |
| 07 UC-77 POST `/cikis?handler=SignOut` — Müşteri çıkışı | T519 | | |
| 07 UC-78 — sayfa isteğinin yanıtı (500; §2.7.1) — Beklenmeyen hata sayfası | T504 | | |
| 07 UC-79 GET `/sitemap.xml` ve 1 adres daha — Site haritası ve robots.txt | T313 | | |
| 07 UC-80 POST `/sistem/odeme-bildirimi` — Ödeme sağlayıcısının bildirim ucu | T311 | | |
| 07 UC-81 GET `/sistem/canlilik` — Canlılık ucu | T01 | | |
| 07 UC-82 GET `/sistem/durum` — Durum ucu | T314 | | |
| 07 UC-83 POST `/panel/yukleme` (başlatma) ve 2 adres daha — Parçalı dosya yükleme ucu | T312 | | |
| 07 UC-84 GET `/panel/giris` ve 2 adres daha — Panel girişi ve şifre sıfırlama | T520 | | |
| 07 UC-85 GET `/panel/davet/{anahtar}` — Yönetici daveti kabul ekranı | T520 | | |
| 07 UC-86 GET `/panel` — Panel ana sayfası | T521 | | |
| 07 UC-87 GET `/panel/urunler` — Ürün listesi | T525 | | |
| 07 UC-88 GET `/panel/urunler/yeni` ve 1 adres daha — Ürün formu | T525 | | |
| 07 UC-89 GET `/panel/kategoriler` — Kategori ağacı | T526 | | |
| 07 UC-90 GET `/panel/kuponlar` ve 2 adres daha — Kuponlar | T526 | | |
| 07 UC-91 GET `/panel/siparisler` — Sipariş listesi | T527 | | |
| 07 UC-92 GET `/panel/siparisler/{iç anahtar}` — Sipariş ayrıntısı | T527 | | |
| 07 UC-93 GET `/panel/talepler` — Talepler | T532 | | |
| 07 UC-94 GET `/panel/talepler/{iç anahtar}` — İletişim talebi ayrıntısı | T532 | | |
| 07 UC-95 GET `/panel/uyeler` — Üye kaydı görünümü | T533 | | |
| 07 UC-96 GET `/panel/kurumsal-icerik` ve 2 adres daha — Kurumsal içerik | T534 | | |
| 07 UC-97 GET `/panel/ana-sayfa-ve-menu` — Ana sayfa ve menü | T535 | | |
| 07 UC-98 GET `/panel/marka` — Marka | T534 | | |
| 07 UC-99 GET `/panel/firma-kimligi-ve-satis` — Firma kimliği ve satış | T535 | | |
| 07 UC-100 GET `/panel/odeme-yontemleri` — Ödeme yöntemleri | T536 | | |
| 07 UC-101 GET `/panel/kargo-sureler-ve-varsayilanlar` — Kargo, süreler ve varsayılanlar | T536 | | |
| 07 UC-102 GET `/panel/yasal-metinler` — Yasal metinler | T507 | | |
| 07 UC-103 GET `/panel/yonetici-hesaplari` — Yönetici hesapları | T522 | | |
| 07 UC-104 GET `/panel/hesabim` — Yöneticinin kendi hesabı | T523 | | |
| 07 UC-105 GET `/panel/satis-ozeti` — Satış özeti | T524 | | |
| 07 UC-106 GET `/panel/islem-izi` — İşlem izi | T522 | | |
| 07 UC-107 GET `/panel/disa-aktarma` — Dışa aktarma | T524 | | |
| 07 UC-108 GET `/panel/yeniden-dogrulama` ve 2 adres daha — Yönetici hesabının doğrulama ekranları | T521 | | |
| 07 UC-109 POST `/panel/cikis?handler=SignOut` — Yönetici çıkışı | T520 | | |
| 07 UC-110 POST `/panel/giris?handler=SignIn` — Panel girişi | T520 | | |
| 07 UC-111 POST `/panel/sifre-sifirlama?handler=RequestPasswordReset` — Panel şifre sıfırlama isteği | T520 | | |
| 07 UC-112 POST `/panel/sifre-sifirlama/{anahtar}?handler=ResetPassword` — Panelde yeni şifrenin kurulması | T520 | | |
| 07 UC-113 POST `/panel/davet/{anahtar}?handler=AcceptAdminInvitation` — Davetle yönetici hesabını açma | T520 | | |
| 07 UC-114 POST `/panel/urunler/yeni?handler=SaveProduct` ve 1 adres daha — Ürünü kaydetme | T525 | | |
| 07 UC-115 POST `/panel/urunler/{iç anahtar}?handler=ChangeProductPublishStatus` ve 1 adres daha — Ürünün ya da varyantın yayın geçişi | T525 | | |
| 07 UC-116 POST `/panel/urunler/{iç anahtar}?handler=DeleteProduct` ve 1 adres daha — Ürünü ya da varyantı kalıcı silme | T525 | | |
| 07 UC-117 POST `…?handler=UploadImage` — Görsel yükleme | T525 | | |
| 07 UC-118 POST `/panel/kategoriler?handler=CreateCategory` — Kategori ekleme | T526 | | |
| 07 UC-119 POST `/panel/kategoriler?handler=RenameCategory` — Kategoriyi yeniden adlandırma | T526 | | |
| 07 UC-120 POST `/panel/kategoriler?handler=MoveCategory` — Kategoriyi taşıma | T526 | | |
| 07 UC-121 POST `/panel/kategoriler?handler=DeleteCategory` — Kategoriyi silme | T526 | | |
| 07 UC-122 POST `/panel/kuponlar/yeni?handler=SaveCoupon` ve 1 adres daha — Kuponu kaydetme | T526 | | |
| 07 UC-123 POST `/panel/siparisler/{iç anahtar}?handler=MarkPaid` — Havale siparişini "Ödendi" işaretleme | T527 | | |
| 07 UC-124 POST `/panel/siparisler/{iç anahtar}?handler=MarkShipped` — Kargoya verme | T527 | | |
| 07 UC-125 POST `/panel/siparisler/{iç anahtar}?handler=MarkReshipped` — Kargoya yeniden verme | T527 | | |
| 07 UC-126 POST `/panel/siparisler/{iç anahtar}?handler=MarkDelivered` — Teslim işareti ve tarihi | T527 | | |
| 07 UC-127 POST `/panel/siparisler/{iç anahtar}?handler=MarkDeliveryFailed` — "Teslim edilemedi" işareti | T527 | | |
| 07 UC-128 POST `/panel/siparisler/{iç anahtar}?handler=CloseDeliveryFailed` — Teslim edilemedi siparişini kapatma | T527 | | |
| 07 UC-129 POST `/panel/siparisler/{iç anahtar}?handler=CancelOrderByCompany` — Firma iptali | T528 | | |
| 07 UC-130 POST `/panel/siparisler/{iç anahtar}?handler=RecordWithdrawal` ve 1 adres daha — Başka kanaldan gelen cayma ya da gecikme feshi bildiriminin kaydı | T528 | | |
| 07 UC-131 POST `/panel/siparisler/{iç anahtar}?handler=ProcessRefund` — Geri ödemeyi işleme | T529 | | |
| 07 UC-132 POST `/panel/siparisler/{iç anahtar}?handler=MarkBankTransferRefundFailed` — "Havale gerçekleşmedi" | T529 | | |
| 07 UC-133 POST `/panel/siparisler/{iç anahtar}?handler=ProcessAmountBasedRefund` — Tutar bazlı kısmi geri ödeme | T529 | | |
| 07 UC-134 POST `/panel/siparisler/{iç anahtar}?handler=RequestRefundIban` — IBAN isteme | T529 | | |
| 07 UC-135 POST `/panel/siparisler/{iç anahtar}?handler=RetryCardRefund` — Kart iadesini yeniden deneme | T529 | | |
| 07 UC-136 POST `/panel/siparisler/{iç anahtar}?handler=SwitchRefundToBankTransfer` — Havale yolunu açma | T529 | | |
| 07 UC-137 POST `/panel/siparisler/{iç anahtar}?handler=MarkCardRefundFailed` — Sonucu beklenen kart iadesini "gerçekleşmedi" işaretleme | T529 | | |
| 07 UC-138 POST `/panel/siparisler/{iç anahtar}?handler=RemoveOrderItem` — Kalem çıkarma ya da adet azaltma | T528 | | |
| 07 UC-139 POST `/panel/siparisler/{iç anahtar}?handler=CompleteServiceItem` — Hizmet kalemini tamamlama | T531 | | |
| 07 UC-140 POST `/panel/siparisler/{iç anahtar}?handler=ReceiveReturn` ve 1 adres daha — İade malını teslim alma ya da reddetme | T528 | | |
| 07 UC-141 POST `/panel/siparisler/{iç anahtar}?handler=RestockReturn` — İade malını stoğa ekleme | T528 | | |
| 07 UC-142 POST `/panel/siparisler/{iç anahtar}?handler=CloseNotReturned` — "Mal dönmedi" kapanışı | T528 | | |
| 07 UC-143 POST `/panel/siparisler/{iç anahtar}?handler=ResetDownloadCount` — İndirme hakkını yenileme | T531 | | |
| 07 UC-144 POST `/panel/siparisler/{iç anahtar}?handler=ResolveDefectClaim` — Ayıp talebini çözüldü işaretleme | T528 | | |
| 07 UC-145 POST `/panel/siparisler/{iç anahtar}?handler=ReopenDefectClaim` — Ayıp talebini yeniden açma | T528 | | |
| 07 UC-146 POST `/panel/siparisler/{iç anahtar}?handler=CorrectAddress` — Adres düzeltmesi | T530 | | |
| 07 UC-147 POST `/panel/siparisler/{iç anahtar}?handler=CorrectCarrier` — Kargo şirketi ve takip numarası düzeltmesi | T530 | | |
| 07 UC-148 POST `/panel/siparisler/{iç anahtar}?handler=CorrectDeliveryDate` — Teslim tarihi düzeltmesi | T530 | | |
| 07 UC-149 POST `/panel/siparisler/{iç anahtar}?handler=CorrectArrivalDate` — Ulaşma tarihi düzeltmesi | T530 | | |
| 07 UC-150 POST `/panel/siparisler/{iç anahtar}?handler=CorrectTransition` — Durum geçişi düzeltmesi | T530 | | |
| 07 UC-151 POST `/panel/siparisler/{iç anahtar}?handler=CorrectOrderEmail` — Misafir siparişinin e-postasını düzeltme | T530 | | |
| 07 UC-152 POST `/panel/siparisler/{iç anahtar}?handler=SaveInternalNote` — İç notu kaydetme | T531 | | |
| 07 UC-153 POST `/panel/siparisler/{iç anahtar}?handler=ResendNotification` — Ulaşmayan e-postayı yeniden gönderme | T531 | | |
| 07 UC-154 POST `/panel/siparisler/{iç anahtar}?handler=ResendAllNotifications` — Ulaşmayan bütün e-postaları yeniden gönderme | T531 | | |
| 07 UC-155 POST `/panel/talepler/{iç anahtar}?handler=CloseContactRequest` — İletişim talebini kapatma | T532 | | |
| 07 UC-156 POST `/panel/talepler/{iç anahtar}?handler=ReopenContactRequest` — İletişim talebini yeniden açma | T532 | | |
| 07 UC-157 POST `/panel/uyeler?handler=DeleteCustomer` — Üye hesabını silme | T533 | | |
| 07 UC-158 POST `/panel/kurumsal-icerik/{tip}/yeni?handler=SaveContent` ve 3 adres daha — İçerik kaydını kaydetme | T534 | | |
| 07 UC-159 POST `/panel/kurumsal-icerik/{tip}/{iç anahtar}?handler=ChangePublishStatus` — İçerik kaydının yayın geçişi | T534 | | |
| 07 UC-160 POST `/panel/kurumsal-icerik/{tip}/{iç anahtar}?handler=DeleteContent` — İçerik kaydını kalıcı silme | T534 | | |
| 07 UC-161 POST `/panel/kurumsal-icerik?handler=ReorderContent` — İçerik kaydının elle sırası | T534 | | |
| 07 UC-162 POST `/panel/ana-sayfa-ve-menu?handler=SaveHomepage` — Ana sayfa ve menü ayarlarını kaydetme | T535 | | |
| 07 UC-163 POST `/panel/marka?handler=SaveBranding` — Marka ayarlarını kaydetme | T534 | | |
| 07 UC-164 POST `/panel/firma-kimligi-ve-satis?handler=SaveCompany` — Firma kimliğini kaydetme | T535 | | |
| 07 UC-165 POST `/panel/firma-kimligi-ve-satis?handler=SuspendSales` — Satışı geçici olarak kapatma | T535 | | |
| 07 UC-166 POST `/panel/firma-kimligi-ve-satis?handler=ResumeSales` — Satışı yeniden açma | T535 | | |
| 07 UC-167 POST `/panel/odeme-yontemleri?handler=SavePaymentMethods` — Ödeme yöntemlerini kaydetme | T536 | | |
| 07 UC-168 POST `/panel/kargo-sureler-ve-varsayilanlar?handler=SaveShippingFee` — Kargo ücreti ve eşikleri kaydetme | T536 | | |
| 07 UC-169 POST `/panel/kargo-sureler-ve-varsayilanlar?handler=SaveDeliveryProvinces` — Teslimat illerini kaydetme | T536 | | |
| 07 UC-170 POST `/panel/kargo-sureler-ve-varsayilanlar?handler=SavePeriods` — Süreleri kaydetme | T536 | | |
| 07 UC-171 POST `/panel/kargo-sureler-ve-varsayilanlar?handler=SaveProductDefaults` — Ürün varsayılanlarını kaydetme | T536 | | |
| 07 UC-172 POST `/panel/kargo-sureler-ve-varsayilanlar?handler=SaveReturnAddress` — İade adresini kaydetme | T536 | | |
| 07 UC-173 POST `/panel/yasal-metinler?handler=SaveLegalTextDraft` — Yasal metnin taslağını kaydetme | T507 | | |
| 07 UC-174 POST `/panel/yasal-metinler?handler=PublishLegalText` — Yasal metni yayına alma | T507 | | |
| 07 UC-175 POST `/panel/yonetici-hesaplari?handler=SendAdminInvitation` — Yönetici daveti gönderme | T522 | | |
| 07 UC-176 POST `/panel/yonetici-hesaplari?handler=RevokeAdminInvitation` — Daveti geri çekme | T522 | | |
| 07 UC-177 POST `/panel/yonetici-hesaplari?handler=RemoveAdmin` — Yöneticiyi kaldırma | T522 | | |
| 07 UC-178 POST `/panel/hesabim?handler=ChangeName` — Yöneticinin adını değiştirme | T523 | | |
| 07 UC-179 POST `/panel/hesabim?handler=ChangeEmail` — Yöneticinin e-postasını değiştirme | T523 | | |
| 07 UC-180 POST `/panel/hesabim?handler=ResendEmailChangeLink` — Yönetici e-posta değişikliğinin bağlantısını yeniden isteme | T523 | | |
| 07 UC-181 POST `/panel/hesabim?handler=ChangePassword` — Yöneticinin şifresini değiştirme | T523 | | |
| 07 UC-182 POST `/panel/disa-aktarma?handler=ExportOrders` — Siparişlerin dışa aktarılması | T524 | | |
| 07 UC-183 POST `/panel/disa-aktarma?handler=ExportCustomers` — Üye listesinin dışa aktarılması | T524 | | |
| 07 UC-184 POST `/panel/disa-aktarma?handler=ExportContactRequests` — İletişim taleplerinin dışa aktarılması | T524 | | |
| 07 UC-185 POST `/panel/yeniden-dogrulama?handler=Reauthenticate` — Yöneticinin şifreyle yeniden doğrulaması | T521 | | |
| 07 UC-186 POST `/panel/dogrulama/{anahtar}?handler=VerifyEmail` — Yöneticinin yeni adresinin doğrulanması | T521 | | |
| 07 UC-187 POST `/panel/geri-al/{anahtar}?handler=RevertPendingEmailChange` — Yöneticinin e-posta değişikliğini geri alması | T521 | | |
| 07:§5.1 Okunuş ve bağlama | T211 | | |
| 07:§5.2 Form girdi modelleri | T538 | | |
| 07:§5.3 Sorgu modelleri | T538 | | |
| 07:§5.4 Makine uçlarının gövdeleri | T312, T538 | | |
| 07:§5.5 Çerçevenin modeli | T537 | | |
| 07:§6.1 Okunuş — kodun biçimi | T212 | | |
| 07:§6.2 Hata kodları | T213 | | |
| 07:§6.3 Kısa süreli bildirim kodları | T214 | | |
| 07:§6.4 Özel hâller | T212 | | |
| 07:§8.1 Yapılabilir işlemler sunucuda | T217 | | |
| 07:§8.2 JavaScript kapalıyken temel akışlar | T539 | | |

#### 7.1.3 Entegrasyon → task

| Kaynak öğe | Task | Implement edildi | Kanıt |
|---|---|---|---|
| 08:§2.1 Ödeme sağlayıcısı — PayTR | T401 | | |
| 08:§2.1.1 Sağlayıcının seçimi | T401 | | |
| 08:§2.1.2 Anahtarlar ve değişim | T401 | | |
| 08:§2.1.3 3D Secure | T406 | | |
| 08:§2.1.4 Ödemenin başlatılması, dönüş ve çerçeve | T401, T510 | | |
| 08:§2.1.5 Bildirimin kabulü ve alan eşlemesi | T402, T311 | | |
| 08:§2.1.6 Son sorgu | T403 | | |
| 08:§2.1.7 Tek tahsilat | T403 | | |
| 08:§2.1.8 Bağlantı hatası ve ayrım | T404 | | |
| 08:§2.1.9 Kart iadesi | T405 | | |
| 08:§2.1.10 Test ve canlı kanıt | T406 | | |
| 08:§2.2 E-posta gönderim altyapısı — Amazon SES | T407 | | |
| 08:§2.2.1 Servisin seçimi | T407 | | |
| 08:§2.2.2 Gönderim ve sonuç sınıfları | T407 | | |
| 08:§2.2.3 Geri dönen adreslerin öğrenilmesi | T408 | | |
| 08:§2.2.4 Alıcı başına gönderim sınırı ve kaçış | T304 | | |
| 08:§2.2.5 Ortak kalıp | T305 | | |
| 08:§2.2.6 Müşteri bildirimleri B-1…B-16 | T306, T307 | | |
| 08:§2.2.7 Firma bildirimleri F-1…F-6 | T308 | | |
| 08:§2.2.8 Hesap ve yönetim e-postaları | T309 | | |
| 08:§2.3 Google ile giriş, kayıt ve yeniden doğrulama | T409 | | |
| 08:§2.4 Çağrısı olmayan dış bağlantılar — kargo şirketi listesi | T310 | | |
| 08:§3.1 Bağlantı katmanının ortak yüzü | T202 | | |
| 08:§3.2 İmzalı makine uçlarının ortak ilkesi | T203 | | |
| 08:§3.3 Bir dış servis çökerse | T204 | | |
| 08:§4.1 Dış olgu varsayımları | T410 | | |
| 08:§4.2 Dış sistemde kayıtlı adresler | T601 | | |
| 08:§4.3 Ürünle gelen dış kaynaklı varlık — yerli üretim logosu | T501 | | |
| 08:§5.1 `07`'ye geri yansıyan uçlar | — | | |
| 08:§5.2 Üst dokümanlara geri besleme | T410 | | |

#### 7.1.4 Arayüz → task

> Doldurulacak: O3 (`04` 102 + `03` 55 = 157 birim).

| Kaynak öğe | Task | Implement edildi | Kanıt |
|---|---|---|---|

#### 7.1.5 İş kuralı → task

> Doldurulacak: O4a, O4b, O5. Beş kalın etiketli grup, her birinin kendi tablosu: **`02`** (O4a: §1–§3; O4b: §4–§13) · **`05`** · **`09`** · **`10`** · **Devir satırları** (O5).

| Kaynak öğe | Task | Implement edildi | Kanıt |
|---|---|---|---|

### 7.2 Geri izlenebilirlik (task → kaynak öğe)

Her task → hangi kaynak öğeden besleniyor?
**Kaynağı olmayan task = gerekçesiz iş / sessiz kapsam genişlemesi.**

| Task | Beslendiği kaynak öğe(ler) | Durum |
|---|---|---|

> **Neden geri yön de gerekli:** İleri yön yalnız **eksik** task'ı yakalar (S3). Planın kendisine sızmış, hiçbir gereksinime dayanmayan işi ise yalnız geri yön yakalar — ve o iş bir kez plana girdiğinde faz boyunca "planda var" gerekçesiyle sorgulanmadan yapılır.

> **Gate check kullanımı:** "Implement edildi" kolonu her faz sonunda kontrol edilir; eşlenip implement edilmeyen öğe **S3 Eksik** bulgusudur. Kaynağı olmayan task gate'te **kapsam bulgusu** olarak raporlanır.

## 8. Boşluk raporu

| # | Boşluk | Durum | Kapatan task |
|---|---|---|---|

## 9. Post-MVP parkı

> **Ne yazılır:** Plan yazılırken ortaya çıkan ama MVP'ye girmeyen işler. Ertelenen işlerden farklıdır — bunlar hiç başlanmamış, bilinçli olarak kapsam dışı bırakılmış kalemlerdir.

---

*Shopfolio — Implementation Plan v0.3*
