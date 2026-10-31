# Shopfolio — Implementation Plan

**Versiyon: v0.2** | **Bağımlılıklar:** `02`–`10` | **Son güncelleme:** 2026-10-31

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
| 7.1.2 | API → task | `07` (208: uç 187 · alt bölüm 21) | O2 | ⬚ |
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

**Task gerektirmeyen birim.** `08:§5.1` — `07`'ye geri yansıyan uçların kaydı: yeni uç çıkmamıştır ve üç mevcut ucun (UC-29, UC-30, UC-80) işi `07` satırlarının task'larındadır (O2). GAP değildir.

**Aday task kimliği (K-1412).** "Task" sütunu aday kimliktir; adı ve gövdesi `§5`'te yazılır. Kurallar: (1) kimlik K-1407'nin faz aralığından gelir ve aşağıdaki aday tablosunda tanımlıdır (kapsama betiği denetler). (2) F1'in kimlikleri baştan ayrıldı; öteki fazlarda her matris oturumu kendi satırlarının gerek duyduğu kimliği sayaçtaki sonraki boş numaradan alır ve tabloya ekler. (3) Kimlikler faz içi sıra gözetmeden verilebilir; sıra `§5`'in işidir, araya `TXXa` girer (`00 §J`); gerekirse O6 betikli tek bir yeniden numaralama yapar ve kapsama betiği öncesini ve sonrasını koşar. (4) Var olan satıra aday kimlik eklenebilir (ör. UC-80'in F3 task'ı `08:§2.1.5` satırına girer); satır silinmez. (5) F1'in sırası modül akış sırasını (K-1410) bağların kurduğu sırayla daraltır: bir göç, bağlandığı tabloların göçünden sonra gelir (`06 §7.2`'nin 130 bağı betikle sınandı — sıra ihlali 0); Yönetim ve Üyelik ile iptal–cayma–iade ve Ödeme bu yüzden akış sırasından ayrılır.

| Faz | Son ayrılan | Sonraki boş |
|---|---|---|
| F0 | — | T01 |
| F1 | T120 | T121 |
| F2 | T205 | T206 |
| F3 | T310 | T311 |
| F4 | T410 | T411 |
| F5 | T501 | T502 |
| F6 | T601 | T602 |

**Aday task tablosu (O1).** F1 eksiksiz ayrıldı (79 entity, en çok 8 entity/göç — K-1409; sıra yukarıdaki (5)); öteki fazlarda yalnız `06` ve `08` satırlarının gerek duyduğu task'lar vardır — sonraki oturumlar ekler.

| Aday | Faz | Kısa ad | Modül | Kaynak birimi (O1) |
|---|---|---|---|---|
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
| T601 | F6 | `DEPLOY_RUNBOOK` provası, dış sistemde kayıtlı adresler ve yansıyan tablolar | Ortak | `06 §8.5`, `08 §4.2` |

**Kapsama komutları (Git Bash; depo kökünden).** Sayımlar aşağıdaki sayılarla aynı çıkmalı, her iki betik de çıkış kodu 0 vermelidir (eksik, fazla, yinelenen 0).

```sh
python -I scripts/doc-scans/11_matris_envanter.py . /tmp/envanter.txt    # kaynak envanteri: 06 117, 08 30 (ve öteki aileler)
python -I scripts/doc-scans/11_matris_kapsama.py . /tmp/kapsama.txt      # matris satırları ↔ beklenen küme (dolu aileler: 06, 08)
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

> Doldurulacak: O2 (`07`, 208 birim). Şablon: aşağıdaki tablo; kimlik biçimi yukarıdadır.

| Kaynak öğe | Task | Implement edildi | Kanıt |
|---|---|---|---|

#### 7.1.3 Entegrasyon → task

| Kaynak öğe | Task | Implement edildi | Kanıt |
|---|---|---|---|
| 08:§2.1 Ödeme sağlayıcısı — PayTR | T401 | | |
| 08:§2.1.1 Sağlayıcının seçimi | T401 | | |
| 08:§2.1.2 Anahtarlar ve değişim | T401 | | |
| 08:§2.1.3 3D Secure | T406 | | |
| 08:§2.1.4 Ödemenin başlatılması, dönüş ve çerçeve | T401 | | |
| 08:§2.1.5 Bildirimin kabulü ve alan eşlemesi | T402 | | |
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

*Shopfolio — Implementation Plan v0.2*
