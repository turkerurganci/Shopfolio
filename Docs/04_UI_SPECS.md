# Shopfolio — UI Specifications

**Versiyon: v0.2** | **Bağımlılıklar:** `02_PRODUCT_REQUIREMENTS.md`, `03_USER_FLOWS.md`, `10_MVP_SCOPE.md` | **Son güncelleme:** 2026-10-04

> **Aşama:** 3 — UI/UX Tasarım · **Rol:** Senior Product Designer / UX Architect
> **Traceability zorunlu: EVET** — §1 tamamlanmadan §3'e (ekran envanteri) geçilmez.
> **Düzey:** Wireframe — bilgi mimarisi ve etkileşim odaklı, pixel-perfect değil.

> **Aşama 1'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36.
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **K-06 · K-97 · Aktör envanteri:** Ekran × rol matrisi dört aktör üzerinden kurulur — ziyaretçi · misafir alıcı · üye müşteri · firma yöneticisi (`02 §1.3`). Yönetim tarafı **tek roldür** (çoklu kullanıcı, aynı yetki). Misafir alıcıyı K-97 ekledi ve K-06'nın *"üyeliksiz sipariş kararına bağlıdır"* diye açık bıraktığı envanteri kapattı; satır Aşama 2'nin çakışma taramasında hizalandı (2026-10-04).
> - **K-14 · K-574 · Firma kimlik bilgileri:** Firma tipine göre değişen zorunlu kimlik seti ve — firma ETBİS doğrulama bilgisini girdiyse — doğrulama bandı sitede **sürekli erişilebilir** olur; alan boşsa band görünmez (K-574). **Tam yerleşim bu dokümanın kararıdır** — hangi bilgi hangi ekranda ve hangi alanda görünecek.
> - **K-27 · Ana sayfa kompozisyonu:** İki hazır düzen tasarlanır — **tanıtım öncelikli** ve **mağaza öncelikli**. Taban kural: her iki düzende de kurumsal tanıtım ile ürün vitrini ana sayfada **birlikte** bulunur; değişen yalnız ağırlık ve sıradır. Serbest sayfa kurgusu (page builder) kapsam dışıdır.

> **Aşama 2'den park edilen girdiler** — kaynak: [`PRODUCT_DISCOVERY_STATUS.md`](PRODUCT_DISCOVERY_STATUS.md) §2, mekanizma: K-36 (yeri: §8.3 AK0-04).
> Bu satırlar **talimattır, karar değildir** — bağlayıcı olan kaynak karardır; bu aşamada karara bağlanacak olan, talimatın nasıl uygulanacağıdır.
>
> - **Dizin — Aşama 2 kararlarının bu dokümana devirleri:** karar kaydında K-647…K-718'in etki sütunlarında bu dokümanı gösteren otuz beş atıf (otuz beş karar) ve Kullanıcı Akışları'nın (`03`) gövdesinde bu dokümana iş bırakan cümleler [`CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md`](CHECKPOINT_REPORTS/PHASE2_CONFLICT_SCAN.md) §6'dadır; etki sütununda yalnız doküman numarası taşıyan sekiz atfın işinin adını dizin verir. Bir iş yalnız `03`'ün gövdesinde yaşar, karar satırı yoktur: yeniden gönderilebilen e-postaların kapsamı ve firma bildirimlerinin "e-posta ulaşmadı" işaretinin biçimi (`03 §7.1.37`, §8.9.2, §10.1.2.2). Devir taraması (`checklists/document-stage.md` §4) buradan başlar; devrin evi karar satırıdır, dizin onu ikinci kez kaydetmez. Aşama 1'in dizini: `PHASE1_CONFLICT_SCAN.md` §6.

---

## 1. Traceability Matrix (ÖNCE BU)

> **Ne yazılır:** `02` gereksinimleri + `03` akış adımları → ekran eşlemesi. İleri ve geri izlenebilirlik.
> Eşlenmeyen kaynak madde = **GAP**. GAP'ler proje sahibine sunulur, karar alınır, **sonra** ekran tanımlarına geçilir.

**Alt bölüm haritası (K-728).** Matris üç oturumda kurulur: **1a** (2026-10-04) envanteri, aday ekran listesini ve müşteri tarafının ileri izlenebilirliğini yazdı; **1b** firma tarafını ve kalanı yazar; **2** geri izlenebilirliği, GAP ve gerekçesiz ekleme listesini ve GAP kararlarını yazar. Şablonun §1.1, §1.2 ve §1.3 numaraları değişmez; alt düzey aşağıdaki tabloyla sabittir. Bir oturum bir alt bölümü bölmek ya da birleştirmek zorunda kalırsa bu tabloyu ve ona atıf yapan satırları aynı PR'da günceller (K-670'in kalıbı).

| Alt bölüm | İçerik | Oturum | Durum |
|---|---|---|---|
| §1.1.1 | Kaynak envanteri ve sayım — aile × bölüm, betik komutları | 1a | ✓ |
| §1.1.2 | Aday ekran listesi (`E-nn`) ve ekran dışı değer kümesi | 1a; 1b ve 2 listenin sonuna aday ekleyebilir | ✓ (1b ekleme yaparsa günceller) |
| §1.1.3 | MVP Kapsamı — `10 §2` KP satırları, tamamı (77) | 1a | ✓ |
| §1.1.4 | Kullanıcı Akışları — müşteri tarafı: `03 §1.6.1`, §1.11.1–§1.11.14, §2, §3.1–§3.4, §4, §5, §6.1–§6.2, §9 (378 satır) | 1a | ✓ |
| §1.1.5 | Ürün Gereksinimleri — kural düzeyi, vitrin ve site: `02 §3.27`–§3.30, §3.32–§3.34, §12.4 ve `04`'e iş bırakan cümleler (87 satır) | 1a | ✓ |
| §1.1.6 | Ürün Gereksinimleri — alt bölüm düzeyi (97 alt bölümün 79'u; panel alt bölümleri §1.1.9'da) | 1a | ✓ |
| §1.1.7 | Devir dizinleri ve park satırları — müşteri tarafı (59 satır) | 1a | ✓ |
| §1.1.8 | Kullanıcı Akışları — firma tarafı ve kalan: `03 §1`'in kalan satırları, §3.5, §6.3, §7, §8, §10 (415 satır) | 1b | ⬚ |
| §1.1.9 | Ürün Gereksinimleri — panel tarafı: `02 §3.31`, §3.3.4, §10.1, §10.6, §10.8 kural düzeyinde; 18 panel alt bölümü | 1b | ⬚ |
| §1.1.10 | Devir dizinleri — panel tarafı (Aşama 1: 45, Aşama 2: 17, gövde: 4) | 1b | ⬚ |
| §1.1.11 | GAP adayları — karara bağlanacak | 1a açtı; 1b ekler; 2 karara bağlar | 7 aday |
| §1.2 | Geri izlenebilirlik (ekran → kaynak); gerekçesiz ekleme listesi | 2 | ⬚ |
| §1.3 | Boşluklar (GAP) ve kararlar; geri besleme satırları (K-726) | 2 | ⬚ |

### 1.1 İleri izlenebilirlik (kaynak → ekran)

Her satır dört sütun taşır: **Kaynak ID · Kaynak özeti · Ekran · Durum**. "Ekran" sütununun değer kümesi §1.1.2'dedir. "Durum" üç değerden biridir: **eşlendi** (en az bir aday ekran ya da ortak bileşen), **ekran dışı** (§1.1.2'nin adıyla yazılmış ekran dışı değerlerinden biri — GAP değildir), **GAP adayı** (§1.1.11'de numarasıyla durur). Atıflar K-669 ve K-701'in okunuşuyla yazılır.

#### 1.1.1 Kaynak envanteri ve sayım

Birim K-727'dir; taraf ayrımı ve toplu satırlar K-730'dadır. Sayımlar `02` v0.55, `03` v0.13 ve `10` v0.37 üzerinde, 2026-10-04'te alındı.

| Aile | Betiğin saydığı | 1a | 1b | Matrise girmeyen |
|---|---|---|---|---|
| `10 §2` KP satırları | 77 | 77 | — | — |
| `03` numaralı satırlar (`n.m.k` ve daha derin) | 793 | 378 | 415 | `03 §11`'in 45 satırı (`n.m` biçimli; `02`'ye geri besleme tablosu) |
| `02` kuralları — kural düzeyi (K-727'nin saydığı bölümler ve `04`'e iş bırakan cümleler) | 101 | 87 | 14 | `02 §1.1`'in `04`'ü anan gerekçe cümlesi (iş bırakmaz; §1.1.6'nın `02 §1.1` satırında) |
| `02` alt bölümleri — alt bölüm düzeyi | 97 | 79 | 18 | — |
| Aşama 1 devir dizini — `04` | 80 | 35 | 45 | — |
| Aşama 2 devir dizini — `04` | 35 | 18 | 17 | — |
| `03`'ün gövdesinde `04`'e iş bırakan cümleler | 7 | 3 | 4 | — |
| `04` park satırları | 3 | 3 | — | Aşama 2 dizin satırı (dizinin kendisine işaret eder) |
| **Toplam** | **1.193** | **680** | **513** | |

**`03` — bölüm bazında** (793): §1 98 (§1.4 13 · §1.5 7 · §1.6 12 · §1.7 17 · §1.10 8 · §1.11 41) · §2 99 · §3 147 (§3.1 6 · §3.2 28 · §3.3 36 · §3.4 17 · §3.5 60) · §4 59 (§4.1 47 · §4.2 12) · §5 33 · §6 52 (§6.1 22 · §6.2 21 · §6.3 9) · §7 131 (§7.1 54 · §7.2 30 · §7.3 47) · §8 106 · §9 35 · §10 33. **1a:** §1.6.1 8 + §1.11.1–§1.11.14 14 + §2 99 + §3.1–§3.4 87 + §4 59 + §5 33 + §6.1–§6.2 43 + §9 35 = 378. **1b:** §1'in kalanı 76 + §3.5 60 + §6.3 9 + §7 131 + §8 106 + §10 33 = 415.

**`02` — kural envanteri.** Kalın numaralı kural (`**n.m.k `) 436'dır: §3 257 · §4 5 · §5 18 · §6 6 · §7 42 · §8 28 · §9 10 · §10 42 · §12 28; `02 §6`'nın numaralı tablo satırı 141'dir ve her biri `03 §3`'ün bir satırının kaynağıdır (`03 §3` girişi). Öteki kimlik aileleri: B- 16 · F- 6 · Z- 47 · L- 9 · P- 49 · H- 4 · S 11 · Ö 5 — `03`'ün satırları üzerinden eşlenir (Z- → `03 §4.1`, L- → `03 §6.1.1`, B- ve F- → `03 §7`). **Kural düzeyinde matrise giren 101 satır:** `02 §3.27`–§3.34 76 kural (§3.27 28 · §3.28 6 · §3.29 6 · §3.30 7 · §3.31 2 · §3.32 10 · §3.33 9 · §3.34 8) + §10.1 5 + §10.6 3 + §10.8 2 + §12.4 1 = 87; bu bölümlerin dışında `04`'e iş bırakan 10 kural (§3.1.4, §3.3.4, §3.9.2, §3.13.3, §3.16.4, §3.19.5, §3.20.1, §3.24.1, §3.24.2, §3.24.6) ve kural numarası taşımayan 4 cümle (§3 girişi, §6 girişi, §10.6.2'nin "sıfır payda" maddesi, §11'in "envantere girmeyenler" notu). `02`'de `04`'ü anan satır 21'dir: 16'sı kural (6'sı sayılan bölümlerin içinde: §3.27.25, §3.28.3, §3.28.4, §3.29.5, §3.34.1, §10.6.2), 5'i kural dışı cümle. Aşama 1 dizininin §6.2'sindeki `04` hedefli 21 gövde cümlesi bu 21 satırın kendisidir ve matrise **bir kez**, `02` kimliğiyle girer. **1a:** 74 + 1 + 9 kural + 3 cümle = 87. **1b:** §3.31 2 + §10.1 5 + §10.6 3 + §10.8 2 + §3.3.4 + §10.6.2'nin maddesi = 14. `02 §1.1`'in gerekçe cümlesi `04`'ü anar ama iş bırakmaz; ayrı satır olmaz.

**Betik komutları** (Git Bash; depo kökünden). Sayımlar yukarıdaki sayılarla aynı çıkmalıdır:

```sh
# 10 §2 — KP satırları (77)
grep -cE '^\| KP-[0-9]+ ' Docs/10_MVP_SCOPE.md
# 03 — numaralı satırlar, üst bölüm ve alt bölüm bazında (793; {2,} §11'in n.m satırlarını dışarıda bırakır)
grep -oE '^\| [0-9]+(\.[0-9]+){2,} \|' Docs/03_USER_FLOWS.md \
  | awk '{split($2,a,"."); c[a[1]]++; d[a[1]"."a[2]]++} END{for(k in c) print "S"k, c[k]; for(k in d) print k, d[k]}' | sort -V
# 03 — yinelenen satır kimliği (boş dönmeli)
grep -oE '^\| [0-9]+(\.[0-9]+){2,} \|' Docs/03_USER_FLOWS.md | sort | uniq -d
# 02 — kalın numaralı kurallar, üst bölüm bazında (436) ve K-727'nin bölümleri
grep -oE '^\*\*[0-9]+\.[0-9]+\.[0-9]+ ' Docs/02_PRODUCT_REQUIREMENTS.md | tr -d '*' \
  | awk -F. '{c[$1]++; d[$1"."$2]++} END{for(k in c) print "S"k, c[k]; for(k in d) print k, d[k]}' | sort -V
# 02 — §6'nın numaralı tablo satırları (141) ve alt bölüm başlıkları (97)
grep -cE '^\| [0-9]+\.[0-9]+\.[0-9]+ \|' Docs/02_PRODUCT_REQUIREMENTS.md
grep -cE '^### [0-9]+\.[0-9]+ ' Docs/02_PRODUCT_REQUIREMENTS.md
# 02 — `04`'ü anan satırlar (21)
grep -cE '`04[` ]|`04_' Docs/02_PRODUCT_REQUIREMENTS.md
# bu dokümanın matris satırları — aile bazında sayım ve yinelenen kaynak kimliği (boş dönmeli)
awk -F'|' '/^#### 1\.1\./{s=$0} /^\| (`0[23] §|KP-|K-[0-9]|Park )/{c[s]++} END{for(k in c) print c[k], k}' Docs/04_UI_SPECS.md | sort -k3 -V
awk -F'|' '/^#### 1\.1\.[3-7]/{s=substr($0,6,6)} /^\| (`0[23] §|KP-|K-[0-9]|Park )/{print s $2}' Docs/04_UI_SPECS.md | sort | uniq -d
```

**Yanlış pozitif denetimi (1a).** `03` kalıbı yalnız satır başındaki `| n.m.k |` hücresini yakalar: tarih, `S1…S11`, `Ö1…Ö5` ve öteki kimlik aileleri ilk hücrede bu biçimde durmaz (ilk hücre aileleri sayıldı: `n.m.k` 480 · `n.m.k.j` 313 · `n.m` 45 · `Sn` 11 · `Ön` 5 · başlık ve durum adları). `02` kalıbı yalnız satır başındaki kalın `n.m.k`'yı yakalar; dört düzeyli kalın kural yoktur (sıfır eşleşme). Yinelenen satır kimliği yoktur.

#### 1.1.2 Aday ekran listesi ve ekran dışı değer kümesi

Bu liste matrisin "Ekran" sütununun değer kümesidir; **nihai ekran envanteri değildir** — envanter `04 §4`'te, yazım turunda doğar (UI4-01, UI4-02) ve bir adayı bölebilir, birleştirebilir ya da adını değiştirebilir. Kimlik `E-nn`'dir (K-725); adayın numarası envantere kadar değişmez, yeni aday listenin **sonuna** eklenir (K-729). 1b panel satırlarını eşlerken yeni aday eklerse bu tabloyu ve haritayı günceller.

| # | Aday ekran | Taraf | Aktör(ler) | Amaç |
|---|---|---|---|---|
| E-01 | Ana sayfa | Müşteri | Ziyaretçi · Müşteri | Firmanın seçtiği düzende kurumsal tanıtımı ve ürün vitrinini birlikte gösterir. |
| E-02 | Kategori sayfası | Müşteri | Ziyaretçi · Müşteri | Bir kategorinin ve alt dallarının ürünlerini tek listede gösterir ve süzdürür. |
| E-03 | Arama sonuçları | Müşteri | Ziyaretçi · Müşteri | Ürün adında yapılan aramanın sonucunu kategori sayfasının düzeniyle gösterir. |
| E-04 | Ürün sayfası | Müşteri | Ziyaretçi · Müşteri | Ürünün bilgisini gösterir, varyantı seçtirir ve sepete ekletir. |
| E-05 | Arşivlenmiş ürün sayfası | Müşteri | Ziyaretçi · Müşteri | "Bu ürün artık satılmıyor" bilgisini fiyatsız gösterir. |
| E-06 | "Sayfa bulunamadı" sayfası | Müşteri | Ziyaretçi · Müşteri | Var olmayan ya da ziyaretçiye kapalı adreste dönüş yollarını gösterir. |
| E-07 | İçerik liste sayfası | Müşteri | Ziyaretçi · Müşteri | Hizmet tanıtımlarını ya da referans işleri firmanın sırasıyla listeler. |
| E-08 | İçerik sayfası | Müşteri | Ziyaretçi · Müşteri | Hakkımızda'yı, bir hizmet tanıtımını, bir referans işi ya da bir genel sayfayı gösterir. |
| E-09 | Sık sorulan sorular sayfası | Müşteri | Ziyaretçi · Müşteri | Bütün soruları ve cevapları tek sayfada listeler. |
| E-10 | İletişim sayfası ve formu | Müşteri | Ziyaretçi · Müşteri | Firmanın iletişim ve yasal kimlik bilgilerini, şubeleri ve iletişim formunu — gönderim sonrası hâliyle — taşır. |
| E-11 | Yasal metin sayfası | Müşteri | Ziyaretçi · Müşteri | Aydınlatma metnini, çerez politikasını ya da "İşlem rehberi"ni gösterir. |
| E-12 | Sepet | Müşteri | Ziyaretçi · Müşteri | Kalemleri güncel fiyat ve satın alınabilirlikle gösterir ve ödeme adımına geçirir. |
| E-13 | Ödeme adımı | Müşteri | Müşteri | E-postayı, adresleri, kuponu ve ödeme yöntemini alır; onay özetini ve onay kutularını gösterir ve siparişi onaylatır. |
| E-14 | Sipariş teyit ve bekleme ekranı | Müşteri | Müşteri | Siparişin alındığını, numarasını, havalede IBAN'ı ve kartta ödemenin beklendiğini gösterir. |
| E-15 | Sipariş takibi girişi | Müşteri | Misafir alıcı | Sipariş numarası ve e-postayla sipariş sayfasını açtırır. |
| E-16 | Sipariş sayfası | Müşteri | Müşteri | Siparişin durumunu ve bütün bilgisini gösterir; o anda açık işlemleri sunar. |
| E-17 | İptal ve gecikme feshi ekranı | Müşteri | Müşteri | Siparişi ya da kalemi iptal ettirir ya da gecikme nedeniyle feshettirir; havalede IBAN'ı alır. |
| E-18 | Cayma beyanı ekranı | Müşteri | Müşteri | Kalemi seçtirir, iade adresini ve yükümlülüğü gösterir, beyanı ve havalede IBAN'ı alır. |
| E-19 | Ayıp talebi ekranı | Müşteri | Müşteri | Kalemi seçtirir ve sorunun açıklamasını alır; talebi yeniden açtırır. |
| E-20 | Kayıt ekranı | Müşteri | Ziyaretçi | Ad, e-posta ve şifreyle hesap açtırır; doğrulama bağlantısını yeniden istetir. |
| E-21 | Doğrulama bağlantısının iniş ekranı | Müşteri | Kullanıcı | Kayıt ya da yeni e-posta doğrulamasının sonucunu — geçersiz bağlantı dahil — söyler. |
| E-22 | Müşteri girişi | Müşteri | Ziyaretçi · Üye | E-posta ve şifreyle ya da Google ile giriş yaptırır. |
| E-23 | Şifre sıfırlama ekranları | Müşteri | Kullanıcı | Sıfırlama bağlantısını istetir ve yeni şifreyi kurdurur. |
| E-24 | Yeniden doğrulama ekranı | Müşteri | Üye | Şifre, e-posta değişikliği ve hesap silmeden önce kimliği yeniden doğrular. |
| E-25 | Hesap — profil ve güvenlik | Müşteri | Üye | Adı, e-postayı ve şifreyi değiştirtir; hesabı sildirir. |
| E-26 | Adres defteri | Müşteri | Üye | Kayıtlı adresleri ekletir, düzenletir ve sildirir. |
| E-27 | Sipariş geçmişi | Müşteri | Üye | Hesaba bağlı siparişleri listeler ve sipariş sayfasına götürür. |
| E-28 | E-posta değişikliğini geri alma ekranı | Müşteri | Kullanıcı | "Bu değişikliği ben yapmadım" bağlantısının sonucunu söyler ve şifreyi yeniden belirletir. |
| E-29 | Panel girişi ve şifre sıfırlama | Panel | Yönetici | Yöneticiyi şifreyle panele alır; panel şifresini sıfırlatır. |
| E-30 | Yönetici daveti kabul ekranı | Panel | Kullanıcı | Davet bağlantısından yönetici hesabını açtırır. |
| E-31 | Panel ana sayfası | Panel | Yönetici | Bekleyen işleri sayaçlarla, kurulum kontrol listesini ve kanal uyarısını gösterir. |
| E-32 | Ürün listesi | Panel | Yönetici | Ürünleri yayın durumlarıyla listeler. |
| E-33 | Ürün formu | Panel | Yönetici | Ürünü, varyantlarını, stoğunu, görsellerini, indirimini, dijital dosyasını ve cayma istisnasını düzenletir. |
| E-34 | Kategori ağacı | Panel | Yönetici | Kategori ağacını kurdurur ve dalları taşıtır. |
| E-35 | Kuponlar | Panel | Yönetici | Kuponları listeler ve düzenletir. |
| E-36 | Sipariş listesi | Panel | Yönetici | Siparişleri aratır ve durumuna göre süzdürür. |
| E-37 | Sipariş ayrıntısı | Panel | Yönetici | Siparişin bütün bilgisini gösterir; yürütüm işlemlerini ve müdahaleleri yaptırır. |
| E-38 | Ayıp talepleri | Panel | Yönetici | Ayıp taleplerini listeler, çözüldü işaretletir ve yeniden açtırır. |
| E-39 | İletişim talepleri | Panel | Yönetici | İletişim taleplerini listeler, okutur, kapattırır ve yeniden açtırır. |
| E-40 | Üye kaydı görünümü | Panel | Yönetici | Bir üyeyi e-postayla aratır, kaydını salt okunur gösterir ve talep üzerine sildirir. |
| E-41 | Kurumsal içerik | Panel | Yönetici | İçerik tiplerinin, genel sayfaların ve duyurunun kayıtlarını listeler ve düzenletir. |
| E-42 | Ana sayfa, menü ve sosyal bağlantılar | Panel | Yönetici | Ana sayfa düzenini seçtirir, menü adlarını ve sosyal bağlantıları düzenletir. |
| E-43 | Marka | Panel | Yönetici | Logoyu, marka adını, site simgesini, marka rengini ve platform imzasını ayarlatır. |
| E-44 | Firma kimliği | Panel | Yönetici | Firma tipine göre yasal kimlik ve iletişim bilgilerini girdirir. |
| E-45 | Ödeme yöntemleri | Panel | Yönetici | Havaleyi açtırır, kapattırır ve IBAN'ı girdirir. |
| E-46 | Kargo, teslimat, süreler, eşikler ve iade adresi | Panel | Yönetici | Kargo ücretini, teslimat illerini, firma ayarı olan süreleri ve eşikleri ve iade adresini düzenletir. |
| E-47 | Yasal metinler | Panel | Yönetici | Aydınlatma metnini ve çerez politikasını düzenletip yayımlatır; üretilen iki metni gösterir. |
| E-48 | Satışın geçici kapatılması | Panel | Yönetici | Satışı geçici olarak kapattırır ve yeniden açtırır. |
| E-49 | Yönetici hesapları | Panel | Yönetici | Yöneticileri listeler; davet ettirir, daveti geri çektirir ve yönetici kaldırtır. |
| E-50 | Yöneticinin kendi hesabı | Panel | Yönetici | Yöneticinin kendi e-postasını ve şifresini değiştirtir. |
| E-51 | Satış özeti | Panel | Yönetici | Seçilen dönemin satış ölçülerini gösterir. |
| E-52 | İşlem izi | Panel | Yönetici | Yönetici işlemlerinin izini tarih ve yönetici süzgeciyle okutur. |
| E-53 | Dışa aktarma | Panel | Yönetici | Siparişleri, üye listesini ve iletişim taleplerini dosya olarak dışa aktartır. |

Aday sayısı 53'tür: müşteri tarafı 28 (E-01…E-28), panel 25 (E-29…E-53). Aktör adları `03 §0.3.2`'nin adlarıdır (K-700).

**Ekran dışı değer kümesi (K-729).** Ekran doğurmayan bir kaynak satırı GAP değildir; "Ekran" sütununa aşağıdaki sabit değerlerden biri, adıyla yazılır. Bir satır hem bir ekrana hem bir ekran dışı değere eşlenebilir (` · ` ile); en az bir ekran ya da ortak bileşen taşıyan satırın durumu "eşlendi"dir.

| Değer | Ne zaman yazılır | Gerekçesi |
|---|---|---|
| ortak bileşen: *ad* | Satır tek bir ekranın değil, birden çok ekranda yinelenen bir kalıbın işidir | Kalıp `04 §2`'de bir kez tanımlanır (UI2); durumu "eşlendi"dir. 1a'nın kullandığı adlar: *çerçeve* (vitrinin üst bölümü, menüsü, duyuru şeridi, altbilgisi) · *mesaj* (hata, uyarı, bilgi ve nötr limit mesajının kalıbı) · *onay* (geri alınamaz işlemin onay penceresi) · *taban* (tasarım tabanı: kırılma noktaları, marka rengi, erişilebilirlik) · *kart* (ürün kartı) · *adres* (adres formu) · *IBAN* (IBAN alanı) · *rozet* (durum rozetleri ve panel işaretleri) · *süre* (sayaç ve süre göstergeleri) · *taslak* ("Taslak" önizleme bandı) · *satış-kapalı* (satış kapalıyken site hâli). Adlar adaydır; bileşen kimliği yazım turunda verilir (UI0-05) |
| ekran dışı — e-posta (`08`) | Satırın çıktısı bir e-postadır | E-postanın metni ve şablonu `08`'in işidir; `04` yalnız e-postanın ekranda bıraktığı izi taşır (karar kaydı §10.2, "`04`'ün kapsamı") |
| ekran dışı — arka plan | Sistemin kendiliğinden yaptığı işlem ya da zamanlayıcı: ayırma, donma, kendiliğinden iptal, imha | Kullanıcının karşısına bir ekran çıkmaz; sonucu bir ekranda görünüyorsa o ekran da yazılır |
| ekran dışı — ürünün dışında | Ödeme sağlayıcısının sayfası, banka, kargo, firmanın kendi yazışması, merci | Ürün o adımı göstermez ve izlemez (`03 §0.3.2`'nin dış olayı; UI4-03) |
| ekran dışı — mimari (`05`) | Arama motoru verileri, adres üretimi, veri kaybı toleransı, genel istek hızı sınırı | Karşılığı mimaridedir (`03 §0.1.4`) |
| ekran dışı — doğrulama (`12`) | Tarayıcı tabanı ve erişilebilirlik denetimi gibi yalnız doğrulanan kurallar | Ekrana değil doğrulama ölçütüne dönüşür (`03 §0.1.4`) |
| ekran dışı — kurulum (`10 §4.1`) | Kurulum tarafının panelin dışındaki işi (ÖK-1…ÖK-7) | Ekran doğurmaz (karar kaydı §10.3, tamlık taraması) — 1a'da kullanılmadı, 1b'nin `03 §10` satırları için |
| ekran dışı — yazım konvansiyonu | Sözlük, aktör listesi, atıf biçimi | `04`'ün başlık notuna yazılır (K-726; UI0-05), ekran değildir |
| yoktur — kapsam dışı | Kaynağın "yoktur" dediği yetenek (`03 §0.1.3`; `10 §3`) | Ekrana girmez; ilgili ekran tanımında "yoktur" diye anılır. Yokluk bir ekranın görünümünü değiştiriyorsa (sunulmayan düğme) satır o ekrana eşlenir |

#### 1.1.3 MVP Kapsamı — `10 §2` KP satırları (77)

Kapsam ağıdır: her satır en az bir ekrana ya da adıyla yazılmış bir ekran dışı değere eşlenir (K-727). Sıra `10 §2`'nin sırasıdır.

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| KP-1 | Üç seviyeli kategori ağacında gezinme; boş kategori menüde görünmez | E-02 · ortak bileşen: çerçeve | eşlendi |
| KP-2 | Ürün adında arama; Türkçe karaktere duyarsız | E-03 · ortak bileşen: çerçeve | eşlendi |
| KP-3 | Fiyat aralığı ve stok durumuna göre süzme; tek sabit düzen | E-02 · E-03 | eşlendi |
| KP-4 | Varyant seçimi; tükenmiş ya da açılmamış kombinasyon seçilemez | E-04 | eşlendi |
| KP-5 | Ürün sayfası: görsel, açıklama, KDV dahil fiyat, fiyat tarihi, üretim yeri | E-04 | eşlendi |
| KP-6 | Üç ürün tipi aynı katalogda ve aynı sepette | E-04 · E-12 · E-13 | eşlendi |
| KP-7 | Arşivlenmiş ürün sayfası; taslak ve silinmiş ürün "sayfa bulunamadı" | E-05 · E-06 | eşlendi |
| KP-8 | Hesap açmadan sipariş; misafirin e-postası özette görünür ve düzeltilir | E-13 · E-37 | eşlendi |
| KP-9 | Sepete ekleme; canlı fiyat; stok ve ürün sınırı | E-04 · E-12 | eşlendi |
| KP-10 | Üyenin sepeti hesapta, misafirinki tarayıcıda; girişte birleşir | E-12 | eşlendi |
| KP-11 | Kupon kodu; sipariş tutarını sıfıra indiremez | E-13 (GA-2) | **GAP adayı** |
| KP-12 | Teslimat ve fatura adresi; adres defterinden seçim, deftere kaydetme | E-13 · E-26 · ortak bileşen: adres | eşlendi |
| KP-13 | Kargo ücreti, ücretsiz kargoya kalan tutar, asgari tutar, teslimat illeri | E-12 · E-13 | eşlendi |
| KP-14 | Onay özeti, Ön Bilgilendirme Formu, onay kutuları, ödeme yükümlülüğü düğmesi | E-13 | eşlendi |
| KP-15 | Kart (sağlayıcının sayfasında, 3D Secure) ya da havale; ekranda sipariş numarası ve IBAN | E-14 · E-16 · ekran dışı — ürünün dışında | eşlendi |
| KP-16 | Stok, kontenjan ve kupon hakkı ödeme süresince ayrılır | ekran dışı — arka plan | ekran dışı |
| KP-17 | Tahmin edilemez sipariş numarası; sipariş anında donan değerler | E-14 · E-16 · ekran dışı — arka plan | eşlendi |
| KP-18 | Sipariş sayfasına üç giriş yolu: geçmiş, numara ve e-posta, erişim anahtarı | E-15 · E-16 · E-27 | eşlendi |
| KP-19 | Kargoya verme süresi vitrinde; takip bilgisi ve "Takip et" sipariş sayfasında | E-04 · E-16 | eşlendi |
| KP-20 | Dijital ürünün sipariş sayfasından indirilmesi | E-16 | eşlendi |
| KP-21 | Müşterinin kalem ya da sipariş iptali, firma onayı beklemeden | E-16 · E-17 | eşlendi |
| KP-22 | Sipariş sayfasından cayma — fiziksel ve hizmet kalemi | E-16 · E-18 | eşlendi |
| KP-23 | Sipariş sayfasından ayıp talebi | E-16 · E-19 | eşlendi |
| KP-24 | Tek kalemin iptali ya da iadesi; para kısmen döner | E-16 · E-17 · E-18 · E-37 | eşlendi |
| KP-25 | Ad, e-posta ve şifreyle hesap açma; doğrulanana kadar çalışmaz | E-20 · E-21 | eşlendi |
| KP-26 | Google hesabıyla giriş | E-22 · E-20 · ekran dışı — ürünün dışında | eşlendi |
| KP-27 | "Beni hatırla", şifre sıfırlama ve değiştirme, ad ve e-posta değiştirme, adres defteri, geri alma | E-22 · E-23 · E-25 · E-26 · E-28 | eşlendi |
| KP-28 | Misafir siparişlerinin hesaba düşmesi; e-posta düzeltmesinde bağın güncellenmesi | E-27 · E-37 · ekran dışı — arka plan | eşlendi |
| KP-29 | Üyenin hesabını silmesi ya da firmanın talep üzerine silmesi | E-25 · E-40 | eşlendi |
| KP-30 | Kişisel veri başvurusu: iletişim formunun "KVKK talebi" tipi | E-10 · E-11 | eşlendi |
| KP-77 | Panelin salt okunur üye kaydı görünümü ve talep üzerine hesap silme | E-40 | eşlendi |
| KP-31 | Ana sayfada kurumsal tanıtım ve ürün vitrini birlikte | E-01 | eşlendi |
| KP-32 | Hakkımızda, hizmet tanıtımı, referans iş, SSS ve genel sayfalar; "İlgili ürünler" | E-07 · E-08 · E-09 | eşlendi |
| KP-33 | İletişim sayfası: iletişim bilgileri, şubeler, "Haritada aç", form | E-10 | eşlendi |
| KP-34 | İletişim formundan girişsiz talep | E-10 · E-39 | eşlendi |
| KP-35 | Yasal kimlik ve iletişim bilgileri her sayfadan erişilebilir; ETBİS bandı; "İşlem rehberi" | ortak bileşen: çerçeve · E-10 · E-11 | eşlendi |
| KP-36 | Sosyal medya ve WhatsApp bağlantıları, duyuru şeridi, "Paylaş" düğmesi | ortak bileşen: çerçeve · E-04 · E-08 | eşlendi |
| KP-37 | Arama motoru verileri otomatik; kapalı sayfalar | ekran dışı — mimari (`05`) | ekran dışı |
| KP-38 | Satış kapalıyken vitrin: ürünler görünür, sepete ekleme ve ödeme kapalı | ortak bileşen: satış-kapalı · E-04 · E-12 · E-31 | eşlendi |
| KP-39 | Ürünün ve varyantlarının tanımlanıp yayına alınması | E-32 · E-33 | eşlendi |
| KP-40 | Ürünün taslak, yayında, arşiv arasında taşınması; "Taslak" bandıyla önizleme; kalıcı silme | E-32 · E-33 · ortak bileşen: taslak | eşlendi |
| KP-41 | Kategori ağacı; ürünün birden çok kategoriye asılması | E-34 · E-33 | eşlendi |
| KP-42 | Ürün görselleri, alternatif metin, metin biçimi seti | E-33 | eşlendi |
| KP-43 | Tarihli yüzde indirim; referans fiyatı sistem hesaplar | E-33 · E-04 | eşlendi |
| KP-44 | Dijital dosyanın yüklenmesi; indirme hakkının yenilenmesi | E-33 · E-37 | eşlendi |
| KP-45 | Cayma hakkı istisnası işareti ve kapalı listeden sebep | E-33 · E-13 · E-16 | eşlendi |
| KP-46 | Sipariş listesinde arama ve süzme; "ödendi", kargoya verme, teslim, hizmet tamamlama | E-36 · E-37 | eşlendi |
| KP-47 | Firma iptali — kapalı listeden sebep; "stokta bulunamadı" uyarısı | E-37 · ortak bileşen: onay | eşlendi |
| KP-48 | Sipariş müdahaleleri: adres, kalem çıkarma, takip, tarih, durum düzeltmesi, iç not, başka kanal kaydı, e-posta düzeltmesi | E-37 | eşlendi |
| KP-49 | Ayıp talebini çözme ve yeniden açma; iletişim taleplerini listeleme ve kapatma | E-38 · E-39 | eşlendi |
| KP-50 | Panel ana sayfasında altı sayaç ve listeleri | E-31 · E-36 | eşlendi |
| KP-51 | Manuel adım bütçesi: sipariş başına en fazla üç zorunlu elle adım | E-37 | eşlendi |
| KP-52 | Siparişlerin CSV olarak dışa aktarılması | E-53 | eşlendi |
| KP-53 | Satış özeti: dönem, sipariş sayısı, ciro, oranlar | E-51 | eşlendi |
| KP-54 | Kurumsal içerik: taslak, önizleme, yayın, silme | E-41 · ortak bileşen: taslak | eşlendi |
| KP-55 | Ana sayfa düzeni seçimi, menü adları, sosyal bağlantılar | E-42 | eşlendi |
| KP-56 | Logo, marka adı, site simgesi, marka rengi, platform imzası | E-43 | eşlendi |
| KP-57 | Duyurunun taslakta hazırlanıp tarihli yayımlanması | E-41 | eşlendi |
| KP-58 | Firma kimliği — firma tipine göre zorunlu alanlar | E-44 | eşlendi |
| KP-59 | Havale ve IBAN; kargo ücreti, eşik, teslimat illeri, süreler, iade adresi | E-45 · E-46 | eşlendi |
| KP-60 | Satışın geçici kapatılması | E-48 | eşlendi |
| KP-61 | Aydınlatma metni ve çerez politikasının düzenlenmesi; üretilen iki metin | E-47 | eşlendi |
| KP-62 | Yönetici daveti, geri çekme, kaldırma | E-49 · E-30 | eşlendi |
| KP-63 | İşlem izinin okunması | E-52 | eşlendi |
| KP-64 | İşletme parametrelerinin panelden değiştirilmesi | E-46 | eşlendi |
| KP-65 | Kurulum kontrol listesi; site boş kurulur | E-31 | eşlendi |
| KP-66 | Müşteriye on altı olayda e-posta (B-1…B-16) | ekran dışı — e-posta (`08`) | ekran dışı |
| KP-67 | Firmaya ve yöneticilere bildirim e-postaları (F-1…F-6) | ekran dışı — e-posta (`08`) | ekran dışı |
| KP-68 | Yanıt adresi; yeniden deneme; panelde "e-posta ulaşmadı" işareti ve kanal uyarısı | ekran dışı — e-posta (`08`) · E-37 · E-31 | eşlendi |
| KP-69 | Duyarlı tek site; panelin her işi telefondan | ortak bileşen: taban | eşlendi |
| KP-70 | Tarayıcı tabanı — son iki büyük sürüm | ekran dışı — doğrulama (`12`) | ekran dışı |
| KP-71 | Erişilebilirlik tabanı — Kontrol Listesi A Seviyesi ve WCAG 2.2 | ortak bileşen: taban | eşlendi |
| KP-72 | Deneme limitleri — nötr mesaj | ortak bileşen: mesaj | eşlendi |
| KP-73 | Çerez onayı yoktur; yalnız zorunlu çerezler | yoktur — kapsam dışı · E-11 | eşlendi |
| KP-74 | Aydınlatma metni bağlantısı her kişisel veri ekranında; periyodik imha | E-20 · E-22 · E-13 · E-10 · E-16 · E-17 · E-18 · E-19 · ekran dışı — arka plan | eşlendi |
| KP-75 | Sipariş tarafında veri kaybı toleransı sıfır | ekran dışı — mimari (`05`) | ekran dışı |
| KP-76 | Eşzamanlı düzenleme uyarısı; sipariş işlemleri güncel hâle karşı | E-33 · E-41 · E-37 · ortak bileşen: mesaj | eşlendi |

#### 1.1.4 Kullanıcı Akışları — müşteri tarafı (378)

`03`'ün satırları satır kimliğiyle, tek tek (K-727). Özet satırın kendisinden — aktörün yaptığı ve sistemin cevabı — alınmıştır. Hata, zaman aşımı, itiraz ve kötüye kullanım satırlarının ekranı, bağlandıkları ana akış adımının ekranıdır. Yöneticinin müşteri akışını ilerleten adımları panel adayına (çoğu E-37) eşlenmiştir; panelin kendi akış satırları (`03 §8`) §1.1.8'dedir. `03 §2.10.2`–§2.10.5 anlatılardır, numaralı satır taşımaz ve işaret ettikleri satırlar üzerinden eşlenir (K-730).

**§1.6.1 Geri alınamaz onay isteyen işlemler (8)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §1.6.1.1` | Havale "ödendi" işaretinin onayı; dijital kalemde düzeltilemeyeceği önceden söylenir | E-37 · ortak bileşen: onay | eşlendi |
| `03 §1.6.1.2` | Kargoya verme ve yeniden gönderim onayı | E-37 · ortak bileşen: onay | eşlendi |
| `03 §1.6.1.3` | Firma iptali onayı; "stokta bulunamadı" sebebinde yasal uyarı | E-37 · ortak bileşen: onay | eşlendi |
| `03 §1.6.1.4` | Geri ödemenin işlenmesi onayı — tutar bazlı kısmi geri ödeme dahil | E-37 · ortak bileşen: onay | eşlendi |
| `03 §1.6.1.5` | Kart hattında karta para gönderen kalem çıkarma onayı | E-37 · ortak bileşen: onay | eşlendi |
| `03 §1.6.1.6` | İade reddi onayı — sonucu tek cümleyle söyler | E-37 · ortak bileşen: onay | eşlendi |
| `03 §1.6.1.7` | Başka kanaldan gelen cayma ya da gecikme feshi bildiriminin kaydı onayı | E-37 · ortak bileşen: onay | eşlendi |
| `03 §1.6.1.8` | Üye hesabının talep üzerine silinmesi onayı | E-40 · ortak bileşen: onay | eşlendi |

**§1.11 Durum × rol × işlem — müşteri, sipariş sayfası (14)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §1.11.1` | Siparişi bütünüyle iptal etmek — Alındı + Bekliyor | E-16 · E-17 | eşlendi |
| `03 §1.11.2` | Fiziksel kalemi iptal etmek — ödeme onayından kargoya verilene kadar | E-16 · E-17 | eşlendi |
| `03 §1.11.3` | Hizmet kalemini iptal etmek — "tamamlandı" işaretine kadar | E-16 · E-17 | eşlendi |
| `03 §1.11.4` | Dijital kalemi iptal etmek — yoktur, düğme sunulmaz | E-16 | eşlendi |
| `03 §1.11.5` | Fiziksel kalemden caymak — Kargoya verildi'den sonra | E-16 · E-18 | eşlendi |
| `03 §1.11.6` | Hizmet kaleminden caymak — ödeme onayından sonra | E-16 · E-18 | eşlendi |
| `03 §1.11.7` | Dijital kalemden caymak — yoktur, düğme sunulmaz | E-16 | eşlendi |
| `03 §1.11.8` | Gecikme nedeniyle fesih — Z-11 geçmiş, teslim tarihi girilmemiş | E-16 · E-17 | eşlendi |
| `03 §1.11.9` | Ayıp talebi açmak ("sorun bildir") | E-16 · E-19 | eşlendi |
| `03 §1.11.10` | Ayıp talebini yeniden açmak — Çözüldü'yken, Z-18 içinde | E-16 · E-19 | eşlendi |
| `03 §1.11.11` | Geri ödeme IBAN'ını girmek — havale hattında | E-16 · E-17 · E-18 · ortak bileşen: IBAN | eşlendi |
| `03 §1.11.12` | Girilmiş IBAN'ı düzeltmek — geri ödeme işlenene kadar | E-16 · ortak bileşen: IBAN | eşlendi |
| `03 §1.11.13` | Havale bilgisini görmek — donmuş IBAN, Alındı + Bekliyor | E-14 · E-16 | eşlendi |
| `03 §1.11.14` | Dijital dosyayı indirmek — indirme hakkı kaldıkça | E-16 | eşlendi |

**§2.1 Ziyaretçi: vitrinde gezinme ve ürünü bulma (11)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §2.1.1` | Ana sayfa: firmanın seçtiği düzen; duyuru şeridi her sayfanın üstünde | E-01 · ortak bileşen: çerçeve | eşlendi |
| `03 §2.1.2` | Menüden kategoriye giriş; kategori sayfası alt dallarla tek liste, kırıntı yolu | E-02 · ortak bileşen: çerçeve | eşlendi |
| `03 §2.1.3` | Ürün adıyla arama; sonuç kategori sayfasının düzeniyle | E-03 · ortak bileşen: çerçeve | eşlendi |
| `03 §2.1.4` | Listeyi süzme: fiyat aralığı ve stok durumu | E-02 · E-03 | eşlendi |
| `03 §2.1.5` | Ürün kartından ürün sayfası: görsel, fiyat, üretim yeri, indirim, kargoya verme süresi | E-04 · ortak bileşen: kart | eşlendi |
| `03 §2.1.6` | Varyant seçimi; tükenmiş ve açılmamış kombinasyon görünür, seçilemez | E-04 | eşlendi |
| `03 §2.1.7` | Bütün varyantları tükenmiş ürün "Tükendi" işaretiyle kalır | E-04 · ortak bileşen: kart | eşlendi |
| `03 §2.1.8` | Varyantı adediyle sepete ekleme | E-04 · E-12 | eşlendi |
| `03 §2.1.9` | Satış kapalıyken gezme: ürünler görünür, sepete ekleme ve ödeme kapalı | E-04 · E-12 · ortak bileşen: satış-kapalı | eşlendi |
| `03 §2.1.10` | Arşivlenmiş ürünün adresi: "Bu ürün artık satılmıyor" sayfası | E-05 | eşlendi |
| `03 §2.1.11` | Taslak ya da silinmiş ürünün adresi: "Sayfa bulunamadı"; yöneticiye "Taslak" bandı | E-06 · ortak bileşen: taslak | eşlendi |

**§2.2 Ziyaretçi: kurumsal içerik ve iletişim formu (10)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §2.2.1` | Ana sayfanın kurumsal blokları: Hakkımızda, işaretli hizmet ve referanslar | E-01 | eşlendi |
| `03 §2.2.2` | Menüden içerik tipine giriş; liste sayfaları elle sırayla | E-07 · ortak bileşen: çerçeve | eşlendi |
| `03 §2.2.3` | İçerik sayfası; SSS tek sayfa; hizmet tanıtımında "Bize ulaşın"; video bağlantısı | E-08 · E-09 | eşlendi |
| `03 §2.2.4` | "İlgili ürünler"den ürün sayfasına geçiş | E-08 · ortak bileşen: kart | eşlendi |
| `03 §2.2.5` | İletişim sayfası: iletişim ve yasal kimlik bilgileri, şubeler, form | E-10 | eşlendi |
| `03 §2.2.6` | Her sayfanın üst ve alt bölümü: sosyal bağlantılar, altbilgi bağlantıları, platform imzası, ETBİS bandı | ortak bileşen: çerçeve | eşlendi |
| `03 §2.2.7` | İletişim formunu açma; aydınlatma kapısı; üyede ön dolu alanlar | E-10 | eşlendi |
| `03 §2.2.8` | Formu doldurup gönderme; kapalı beş konu tipi | E-10 | eşlendi |
| `03 §2.2.9` | Cevabı bekleme: gönderene numara ve takip sayfası verilmez | E-10 | eşlendi |
| `03 §2.2.10` | Taslak ya da silinmiş içeriğin adresi: "Sayfa bulunamadı" | E-06 | eşlendi |

**§2.3 Müşteri: sepet (8)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §2.3.1` | Sepete ekleme; stok ve "bir siparişte en fazla" sınırı mesajları | E-04 · E-12 · ortak bileşen: mesaj | eşlendi |
| `03 §2.3.2` | Sepeti açma; canlı fiyat, "fiyatı değişti" işareti | E-12 | eşlendi |
| `03 §2.3.3` | Adet değiştirme ve kalem çıkarma | E-12 | eşlendi |
| `03 §2.3.4` | Satın alınamaz kalem sepette kalır: "Tükendi", "Bu adette stok yok", sınır mesajı | E-12 | eşlendi |
| `03 §2.3.5` | Vitrinden kalkan kalem sepetten çıkar, bir kez söylenir | E-12 · ortak bileşen: mesaj | eşlendi |
| `03 §2.3.6` | Girişte iki sepetin birleşmesi; eklenen ürünler bir kez adıyla söylenir | E-12 · E-22 · ortak bileşen: mesaj | eşlendi |
| `03 §2.3.7` | Çıkışta sepet hesapla gider, tarayıcıda boş görünür | E-12 · ortak bileşen: çerçeve | eşlendi |
| `03 §2.3.8` | Ödeme adımına geçiş; asgari sipariş tutarı ve satış kapısı denetimi | E-12 · E-13 | eşlendi |

**§2.4 Müşteri: ödeme adımı ve sipariş onayı (9)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §2.4.1` | Ödeme adımının açılışı; misafirin e-postası; "bu e-posta kayıtlı" giriş hatırlatması | E-13 (GA-5) | **GAP adayı** |
| `03 §2.4.2` | Teslimat adresi: defterden seçim ya da yeni adres, deftere kaydetme | E-13 · ortak bileşen: adres | eşlendi |
| `03 §2.4.3` | Fatura adresi: varsayılan aynı, "fatura adresim farklı" | E-13 · ortak bileşen: adres | eşlendi |
| `03 §2.4.4` | Kupon kodu girme, değiştirme, kaldırma | E-13 (GA-2) | **GAP adayı** |
| `03 §2.4.5` | Ödeme yöntemi seçimi; kart "şu an kullanılamıyor", havale tavanı (L-8) | E-13 | eşlendi |
| `03 §2.4.6` | Onay özeti: kalemler, döküm, toplam, misafirin e-postası, Ön Bilgilendirme Formu | E-13 | eşlendi |
| `03 §2.4.7` | Onay kutuları: iki kutu; dijital ve hizmet kaleminde ek kutular | E-13 | eşlendi |
| `03 §2.4.8` | "Siparişi onayla — ödeme yükümlülüğü doğar"; onay anında yeniden değerlendirme | E-13 | eşlendi |
| `03 §2.4.9` | Sipariş oluşur: numara, donma, ayırma | E-14 · ekran dışı — arka plan | eşlendi |

**§2.5 Müşteri: ödeme ve teslim (17)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §2.5.1.1` | Kart bilgisi sağlayıcının sayfasında ya da çerçevesinde girilir | ekran dışı — ürünün dışında | ekran dışı |
| `03 §2.5.1.2` | 3D Secure sonrası dönüş sipariş sayfasına; ödeme bekleniyor hâli | E-14 · E-16 | eşlendi |
| `03 §2.5.1.3` | Kart reddedilir: sipariş sayfası ödemenin gerçekleşmediğini söyler; sepet durur | E-16 · E-12 | eşlendi |
| `03 §2.5.1.4` | Ödeme yarıda kalır; Z-7 dolunca son sorgu | E-16 · ekran dışı — arka plan | eşlendi |
| `03 §2.5.2.1` | Havaleyle onay: ekranda donmuş IBAN ve sipariş numarası | E-14 · E-16 | eşlendi |
| `03 §2.5.2.2` | Müşteri bankasından havale yapar | ekran dışı — ürünün dışında | ekran dışı |
| `03 §2.5.2.3` | Havale ödeme hatırlatması (B-3) | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §2.5.2.4` | Yönetici "ödendi" işaretler; panel donmuş IBAN'ı gösterir | E-37 | eşlendi |
| `03 §2.5.2.5` | Süre dolunca kendiliğinden iptal | E-16 · ekran dışı — arka plan | eşlendi |
| `03 §2.5.2.6` | Ödeme beklenirken vazgeçme: sipariş bütünüyle iptal | E-16 · E-17 | eşlendi |
| `03 §2.5.3.1` | Ödeme onayının işlenmesi: stok düşer, süreler başlar, dijital teslim | E-16 · ekran dışı — arka plan | eşlendi |
| `03 §2.5.3.2` | Dijital dosyanın sipariş sayfasından indirilmesi; hak dolunca iletişim formu | E-16 | eşlendi |
| `03 §2.5.3.3` | Kargoya verme: sipariş sayfasında şirket, takip numarası, "Takip et" | E-37 · E-16 | eşlendi |
| `03 §2.5.3.4` | "Teslim edilemedi": gönderi firmaya döner | E-37 · E-16 | eşlendi |
| `03 §2.5.3.5` | Teslim işareti ve teslim tarihi; cayma penceresi başlar | E-37 · E-16 | eşlendi |
| `03 §2.5.3.6` | Hizmet kalemi "tamamlandı" işareti | E-37 · E-16 | eşlendi |
| `03 §2.5.3.7` | Fiziksel kalemsiz siparişin izlenmesi: Alındı → Teslim edildi | E-16 | eşlendi |

**§2.6 Müşteri: sipariş takibi (7)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §2.6.1` | Üye sipariş geçmişinden siparişi açar | E-27 · E-16 | eşlendi |
| `03 §2.6.2` | Misafir alıcı sipariş numarası ve e-postayla sorgular (L-5) | E-15 · E-16 | eşlendi |
| `03 §2.6.3` | Sipariş e-postasındaki erişim anahtarlı bağlantı | E-16 | eşlendi |
| `03 §2.6.4` | Sipariş sayfasının içeriği: iki eksen, kalemler, adresler, takip, indirme, IBAN, metin sürümleri, açık işlemler | E-16 | eşlendi |
| `03 §2.6.5` | E-posta ulaşmaz: bilgi sipariş sayfasında; panelde "e-posta ulaşmadı" | E-16 · E-37 | eşlendi |
| `03 §2.6.6` | Hesap silinir ya da e-posta değişir: sipariş misafir yolundan sürer | E-15 · E-16 | eşlendi |
| `03 §2.6.7` | Misafir e-postasını yanlış yazmış: firmaya ulaşır, yönetici düzeltir | E-10 · E-37 | eşlendi |

**§2.7 Müşteri: iptal ve gecikme feshi (8)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §2.7.1` | Ödeme beklenirken siparişin bütünüyle iptali | E-16 · E-17 (GA-1) | **GAP adayı** |
| `03 §2.7.2` | Ödenmiş siparişte fiziksel kalem iptali; havalede IBAN girişi | E-16 · E-17 | eşlendi |
| `03 §2.7.3` | Ödenmiş siparişte hizmet kalemi iptali | E-16 · E-17 | eşlendi |
| `03 §2.7.4` | Ödenmiş dijital kalemin iptali yoktur | E-16 | eşlendi |
| `03 §2.7.5` | Geri ödeme IBAN'ının girilmesi ve düzeltilmesi; aydınlatma bağlantısı | E-16 · E-17 · E-18 · ortak bileşen: IBAN | eşlendi |
| `03 §2.7.6` | İptalin geri ödemesi: kartta kendiliğinden, havalede yönetici | E-37 · E-16 · ekran dışı — arka plan | eşlendi |
| `03 §2.7.7` | Gecikme nedeniyle fesih; havalede IBAN | E-16 · E-17 | eşlendi |
| `03 §2.7.8` | Feshin geri ödemesi; kargodaki mal firmaya döner | E-37 · E-16 | eşlendi |

**§2.8 Müşteri: cayma ve iade (15)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §2.8.1.1` | Cayma düğmesinin görünürlüğü; koşullu istisnada koşul metni | E-16 · E-18 | eşlendi |
| `03 §2.8.1.2` | Cayma beyanı: kalem seçimi, iade adresi, gönderme yükümlülüğü, IBAN | E-18 (GA-1) | **GAP adayı** |
| `03 §2.8.1.3` | Teslim beyanla aynı güne işaretlenir: panel sırayı sorar | E-37 | eşlendi |
| `03 §2.8.1.4` | Müşteri malı karşı ödemeli gönderir | ekran dışı — ürünün dışında | ekran dışı |
| `03 §2.8.1.5` | İade malının teslim alınması ve ulaşma tarihi | E-37 · E-16 | eşlendi |
| `03 §2.8.1.6` | Koruyucu ambalajı açılmış mal: iade reddi | E-37 · E-16 | eşlendi |
| `03 §2.8.1.7` | Kullanılmış ya da hasarlı dönen mal: ret ve kesinti yoktur | E-37 | eşlendi |
| `03 §2.8.1.8` | Mal hiç gönderilmez: "iade malı bekleniyor", "mal dönmedi" kapanışı | E-37 · E-16 | eşlendi |
| `03 §2.8.2.1` | Hizmet kaleminin cayma düğmesinin görünürlüğü | E-16 | eşlendi |
| `03 §2.8.2.2` | Hizmet kaleminden cayma: iade adresi gösterilmez; IBAN | E-18 | eşlendi |
| `03 §2.8.3.1` | Dijital kalemden cayma yoktur | E-16 | eşlendi |
| `03 §2.8.4.1` | Caymanın geri ödemesinin işlenmesi | E-37 · E-16 | eşlendi |
| `03 §2.8.4.2` | Geri ödeme havalesi gerçekleşmez: IBAN alanı yeniden açılır | E-37 · E-16 | eşlendi |
| `03 §2.8.4.3` | Kart iadesi gerçekleşmez: havale yolu açılır, sipariş sayfası söyler | E-37 · E-16 | eşlendi |
| `03 §2.8.4.4` | Başka kanaldan cayma bildirimi: yönetici kaydeder; IBAN alanı | E-37 · E-16 | eşlendi |

**§2.9 Müşteri: ayıp talebi (6)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §2.9.1` | "Sorun bildir"in görünürlüğü ve süresi (Z-18) | E-16 | eşlendi |
| `03 §2.9.2` | Kalem seçimi ve sorunun açıklaması; iade adresi; aydınlatma bağlantısı | E-19 | eşlendi |
| `03 §2.9.3` | Talep açıkken sipariş sayfası talebin durumunu gösterir | E-16 | eşlendi |
| `03 §2.9.4` | Çözüm sistemin dışında yürür | ekran dışı — ürünün dışında | ekran dışı |
| `03 §2.9.5` | Yönetici talebi "çözüldü" işaretler | E-38 · E-37 · E-16 | eşlendi |
| `03 §2.9.6` | Müşteri talebi yeniden açar | E-16 · E-19 | eşlendi |

**§2.10 Uçtan uca anlatılar — 2.10.1'in adım tablosu (8)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §2.10.1.1` | Ürünü bulma ve varyant seçimi (2.1.2–2.1.6) | E-02 · E-03 · E-04 | eşlendi |
| `03 §2.10.1.2` | Sepete ekleme (2.3.1) | E-04 · E-12 | eşlendi |
| `03 §2.10.1.3` | Ödeme adımında adres, kupon, yöntem (2.4.1–2.4.5) | E-13 | eşlendi |
| `03 §2.10.1.4` | Özet, form, iki kutu ve onay (2.4.6–2.4.8) | E-13 | eşlendi |
| `03 §2.10.1.5` | Sipariş oluşur (2.4.9) | E-14 | eşlendi |
| `03 §2.10.1.6` | Ödeme: kartta 3D Secure, havalede "ödendi" | E-14 · E-16 · E-37 | eşlendi |
| `03 §2.10.1.7` | Hazırlama ve kargoya verme | E-37 · E-16 | eşlendi |
| `03 §2.10.1.8` | Teslim işareti ve tarihi | E-37 · E-16 | eşlendi |

**§3.1 Bütün akışlara uygulanan ilkeler (6)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §3.1.1` | Hiçbir akış bir e-postanın ulaşmasına bağlı değildir | E-16 · E-20 · E-23 · E-37 | eşlendi |
| `03 §3.1.2` | Sistem müşterinin ödediği siparişi kendiliğinden iptal etmez | E-16 · E-37 · ekran dışı — arka plan | eşlendi |
| `03 §3.1.3` | Onaylanan özet ile oluşan sipariş birebir aynıdır | E-13 | eşlendi |
| `03 §3.1.4` | Sepet hatalarda korunur | E-12 | eşlendi |
| `03 §3.1.5` | Limit aşımı nötr konuşur | ortak bileşen: mesaj | eşlendi |
| `03 §3.1.6` | Hata ve uyarının anlamı yalnız renkle taşınmaz; biçimi `04`'ün işi | ortak bileşen: mesaj · ortak bileşen: taban | eşlendi |

**§3.2.1 Satın alma hataları (22)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §3.2.1.1` | 3D Secure başarısız ya da kart reddi | E-16 · E-12 | eşlendi |
| `03 §3.2.1.2` | Banka ekranı kapanır ya da ödeme yarıda kalır | E-16 · E-12 | eşlendi |
| `03 §3.2.1.3` | Onay anında özet değişmiş: güncel özet, yeniden onay | E-13 | eşlendi |
| `03 §3.2.1.4` | Sepetteki kalem tükenmiş ya da kontenjan dolmuş | E-12 · E-13 | eşlendi |
| `03 §3.2.1.5` | Sepetteki ürün arşive ya da taslağa alınmış, silinmiş | E-12 | eşlendi |
| `03 §3.2.1.6` | Stoktan fazla adet: "Bu adette stok yok" | E-04 · E-12 | eşlendi |
| `03 §3.2.1.7` | "Bir siparişte en fazla" sınırı aşılır | E-04 · E-12 | eşlendi |
| `03 §3.2.1.8` | Aynı dijital varyant ikinci kez: "Bu ürün zaten sepetinde" | E-04 · E-12 | eşlendi |
| `03 §3.2.1.9` | Önceden alınmış dijital varyant: "Bu ürünü daha önce aldınız" | E-04 · E-12 | eşlendi |
| `03 §3.2.1.10` | Teslimat yapılmayan il: adres kabul edilmez | E-13 · ortak bileşen: adres | eşlendi |
| `03 §3.2.1.11` | Asgari sipariş tutarının altı: eksik tutar gösterilir | E-12 | eşlendi |
| `03 §3.2.1.12` | Kupon kabul edilmez | E-13 (GA-2) | **GAP adayı** |
| `03 §3.2.1.13` | Misafir e-postasını yanlış yazar: özet gösterir, onaydan sonra firma düzeltir | E-13 · E-10 · E-37 | eşlendi |
| `03 §3.2.1.14` | Hesabı olan kişi misafir sipariş verir: giriş hatırlatması | E-13 | eşlendi |
| `03 §3.2.1.15` | Ödeme sağlayıcısına erişilemez | E-13 · E-12 | eşlendi |
| `03 §3.2.1.16` | İptal edilmiş siparişe geç gelen ödeme: kendiliğinden geri ödeme | E-16 · E-37 · ekran dışı — arka plan | eşlendi |
| `03 §3.2.1.17` | Satış kapalıdır | E-04 · E-12 · ortak bileşen: satış-kapalı | eşlendi |
| `03 §3.2.1.18` | Art arda geçersiz kupon (L-6): kupon alanı kapanır | E-13 (GA-2) · ortak bileşen: mesaj | **GAP adayı** |
| `03 §3.2.1.19` | L-7 eşiği: yeni sipariş onaylanamaz | E-13 · ortak bileşen: mesaj | eşlendi |
| `03 §3.2.1.20` | Sipariş notu alanı yoktur; yol iletişim formu | E-13 · E-10 | eşlendi |
| `03 §3.2.1.21` | L-8 doluyken havale seçilemez | E-13 · ortak bileşen: mesaj | eşlendi |
| `03 §3.2.1.22` | Harcama itirazı (ters ibraz): ürün bilmez | ekran dışı — ürünün dışında | ekran dışı |

**§3.2.2 Sipariş takibi hataları (6)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §3.2.2.1` | Sipariş e-postası ulaşmaz: bilgi sipariş sayfasında | E-15 · E-16 | eşlendi |
| `03 §3.2.2.2` | Yanlış numara ya da e-posta: sayfa açılmaz, L-5 | E-15 · ortak bileşen: mesaj | eşlendi |
| `03 §3.2.2.3` | Hesap silinmiş, sipariş yürüyor: misafir yolu | E-15 · E-16 | eşlendi |
| `03 §3.2.2.4` | Kendiliğinden iptal edilmiş ödenmemiş sipariş listede görünmez | E-27 | eşlendi |
| `03 §3.2.2.5` | Misafir siparişinin e-postası sahibi istemeden düzeltilir: B-15, yeniden düzeltme | E-37 · ekran dışı — e-posta (`08`) | eşlendi |
| `03 §3.2.2.6` | Teslimat adresi sahibi istemeden düzeltilir: B-10, geri düzeltme | E-37 · ekran dışı — e-posta (`08`) | eşlendi |

**§3.3 İptal, cayma ve iade hataları (36)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §3.3.1` | Kargodan sonra iptal istenir: iptal düğmesi yok, yol cayma | E-16 | eşlendi |
| `03 §3.3.2` | Z-10 ya da Z-11 aşılır: iptal ve fesih düğmeleri; panelde kanuni faiz uyarısı | E-16 · E-17 · E-37 | eşlendi |
| `03 §3.3.3` | Ödenmiş dijital kalemde iptal ve cayma düğmesi sunulmaz | E-16 | eşlendi |
| `03 §3.3.4` | Cayma istisnalı kalem: mutlakta düğme yok, koşulluda koşul metni | E-16 · E-18 | eşlendi |
| `03 §3.3.5` | Tamamlanmış hizmetten cayma: düğme sunulmaz | E-16 | eşlendi |
| `03 §3.3.6` | Cayma penceresi geçmiş: düğme kapanır | E-16 | eşlendi |
| `03 §3.3.7` | Havale geri ödemesinde IBAN beyanla girilir; öteki hâllerde istenir | E-16 · E-17 · E-18 · ortak bileşen: IBAN | eşlendi |
| `03 §3.3.8` | Geri ödeme süresi mal ulaşana kadar başlamaz; panel kalan süreyi gösterir | E-37 · ortak bileşen: süre | eşlendi |
| `03 §3.3.9` | Müşteri kaynaklı teslim edilememe: gidiş kargosu ödenmez | E-37 | eşlendi |
| `03 §3.3.10` | Kısmi iptalden sonra kupon ve kargo yeniden hesaplanmaz | E-16 · E-37 | eşlendi |
| `03 §3.3.11` | Kargodayken vazgeçme: cayma beyanı açık | E-16 · E-18 | eşlendi |
| `03 §3.3.12` | Geç işaretlenen teslimde pencere dolmuş: düğme kapalı | E-16 | eşlendi |
| `03 §3.3.13` | Tek kalemden caymada kargo ücreti geri ödemeye girmez | E-18 · E-37 | eşlendi |
| `03 §3.3.14` | İade kargosu karşı ödemeli gönderilir | E-18 · ekran dışı — ürünün dışında | eşlendi |
| `03 §3.3.15` | Çözülmüş ayıp tekrarlar: talep yeniden açılır | E-16 · E-19 · E-38 | eşlendi |
| `03 §3.3.16` | Mal hiç gönderilmez: "iade malı bekleniyor", "mal dönmedi" kapanışı | E-37 | eşlendi |
| `03 §3.3.17` | Teslim alma işareti gecikir: tarih geçmişe dönük girilir | E-37 | eşlendi |
| `03 §3.3.18` | "Mal dönmedi"den sonra mal ulaşır: kalem yeniden açılır | E-37 | eşlendi |
| `03 §3.3.19` | IBAN silindikten sonra mal ulaşır: sipariş sayfasında IBAN alanı açılır | E-16 · E-37 · ortak bileşen: IBAN | eşlendi |
| `03 §3.3.20` | IBAN geç girilir: süre durmaz, alan e-postadan bağımsız görünür | E-16 | eşlendi |
| `03 §3.3.21` | Tamamlanmamış hizmet kaleminden cayma: kalem kapanır | E-18 · E-16 | eşlendi |
| `03 §3.3.22` | Kargodayken cayılan gönderi firmaya döner: teslim alma adımı | E-37 | eşlendi |
| `03 §3.3.23` | Ödeme onayından önce kalem iptali sunulmaz | E-16 · E-17 | eşlendi |
| `03 §3.3.24` | Kart geri ödemesi başarısız: "geri ödeme gerçekleşmedi" işareti | E-37 · E-31 | eşlendi |
| `03 §3.3.25` | Başka kanaldan cayma bildirimi: yönetici teyit eder ve kaydeder | E-37 | eşlendi |
| `03 §3.3.26` | Yeniden istenen IBAN hiç girilmez: "IBAN bekleniyor" listesi | E-31 · E-37 | eşlendi |
| `03 §3.3.27` | Yanlış IBAN ya da reddedilen havale: düzeltme, "havale gerçekleşmedi" | E-16 · E-37 · ortak bileşen: IBAN | eşlendi |
| `03 §3.3.28` | Kısmen ifa edilmiş hizmetten cayma: tam geri ödeme | E-18 · E-37 | eşlendi |
| `03 §3.3.29` | Fiziksel kalemler teslimden önce kapanır: hizmetin düğmesi sürer | E-16 | eşlendi |
| `03 §3.3.30` | İade gönderisi yolda kaybolur: ürünün dışında | ekran dışı — ürünün dışında | ekran dışı |
| `03 §3.3.31` | Kargodan önce başka kanaldan cayma: "müşteriyle anlaşıldı" iptali | E-37 | eşlendi |
| `03 §3.3.32` | Pencere dışı ya da mutlak istisnada cayma isteği: düğme açılmaz | E-16 · E-37 | eşlendi |
| `03 §3.3.33` | Sahte cayma bildirimi: siparişin kanalından teyit | E-37 | eşlendi |
| `03 §3.3.34` | Beyan ve teslim aynı gün: panel sırayı sorar, beyan saatini gösterir | E-37 | eşlendi |
| `03 §3.3.35` | Ambalajı açılmış mal: iade reddi seçimi | E-37 | eşlendi |
| `03 §3.3.36` | Kullanılmış mal: ret ve kesinti yoktur | E-37 | eşlendi |

**§3.4 Üyelik hataları (17)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §3.4.1` | Doğrulama bağlantısının süresi dolmuş: "bağlantı geçersiz, yeniden kayıt olun" | E-21 | eşlendi |
| `03 §3.4.2` | Doğrulamadan giriş denemesi: beklendiği söylenir, yeniden isteme yolu | E-22 | eşlendi |
| `03 §3.4.3` | Kayıtta e-posta zaten kayıtlı | E-20 | eşlendi |
| `03 §3.4.4` | Sıfırlamada kayıtlı olmayan e-posta: nötr mesaj | E-23 · ortak bileşen: mesaj | eşlendi |
| `03 §3.4.5` | Sıfırlama bağlantısı kullanılmış ya da süresi dolmuş | E-23 | eşlendi |
| `03 §3.4.6` | Zayıf ya da yaygın şifre reddedilir | E-20 · E-23 · E-25 | eşlendi |
| `03 §3.4.7` | Google e-postayı doğrulanmamış verir: kayda yönlendirme | E-22 · E-20 | eşlendi |
| `03 §3.4.8` | Şifresiz Google hesabında "şifremi unuttum" şifre belirler | E-23 | eşlendi |
| `03 §3.4.9` | Yeni e-posta doğrulanmaz: değişiklik geçerli olmaz | E-25 (GA-4) | **GAP adayı** |
| `03 §3.4.10` | Yürüyen siparişle hesap silme engellenmez | E-25 | eşlendi |
| `03 §3.4.11` | L-1, L-2, L-3 aşımı: geçici engel, nötr mesaj | E-20 · E-22 · E-23 · ortak bileşen: mesaj | eşlendi |
| `03 §3.4.12` | Aydınlatma metni tamamlanmamış: kayıt ve ilk Google girişi kapalı | E-20 · E-22 | eşlendi |
| `03 §3.4.13` | E-posta sahibinin haberi olmadan değişir: geri alma bağlantısı | E-28 · ekran dışı — e-posta (`08`) | eşlendi |
| `03 §3.4.14` | Doğrulama bağlantısı defalarca istenir (L-9) | E-20 · ortak bileşen: mesaj | eşlendi |
| `03 §3.4.15` | Geri alma bağlantısı açıkken hesap silinemez: sebep ve bitiş tarihi | E-25 · E-40 | eşlendi |
| `03 §3.4.16` | Google e-postası bekleyen kayda eşleşir: kayıt devralınmaz | E-22 | eşlendi |
| `03 §3.4.17` | Yeniden doğrulamada art arda yanlış şifre (L-1) | E-24 · ortak bileşen: mesaj | eşlendi |

**§4.1 Süre envanterinden zaman aşımı akışları (47)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §4.1.1` | Z-1 doğrulama bağlantısının ömrü | E-21 · ekran dışı — arka plan | eşlendi |
| `03 §4.1.2` | Z-2 şifre sıfırlama bağlantısının ömrü | E-23 · ekran dışı — arka plan | eşlendi |
| `03 §4.1.3` | Z-3 yönetici daveti bağlantısının ömrü | E-30 · E-49 | eşlendi |
| `03 §4.1.4` | Z-4 kısa oturum: oturum kapanır | E-22 · ekran dışı — arka plan | eşlendi |
| `03 §4.1.5` | Z-5 uzun oturum — "beni hatırla" | E-22 · ekran dışı — arka plan | eşlendi |
| `03 §4.1.6` | Z-6 sepetin ömrü: dolmaz | E-12 | eşlendi |
| `03 §4.1.7` | Z-7 kart ödeme süresi: son sorgu, kendiliğinden iptal | E-16 · ekran dışı — arka plan | eşlendi |
| `03 §4.1.8` | Z-8 havale ödeme süresi: kendiliğinden iptal | E-16 · ortak bileşen: süre · ekran dışı — arka plan | eşlendi |
| `03 §4.1.9` | Z-9 havale ödeme hatırlatması (B-3) | ekran dışı — e-posta (`08`) | ekran dışı |
| `03 §4.1.10` | Z-10 kargoya verme süresi: iptal düğmesi açık, panelde faiz uyarısı | E-04 · E-16 · E-37 | eşlendi |
| `03 §4.1.11` | Z-11 yasal teslim üst sınırı: gecikme feshi düğmesi açılır | E-16 | eşlendi |
| `03 §4.1.12` | Z-12 teslim tarihi girilmezse sipariş Kargoya verildi'de kalır | E-37 · E-16 | eşlendi |
| `03 §4.1.13` | Z-39 ifa süresi: iptal düğmesi açık, panelde faiz uyarısı | E-16 · E-37 | eşlendi |
| `03 §4.1.14` | Z-13 cayma penceresi — fiziksel: düğme kapanır | E-16 · ortak bileşen: süre | eşlendi |
| `03 §4.1.15` | Z-14 cayma penceresi — hizmet | E-16 | eşlendi |
| `03 §4.1.16` | Z-15 dijital kalemde hak üçüncü kutuyla düşer | E-13 | eşlendi |
| `03 §4.1.17` | Z-16 caymanın geri ödeme süresi | E-37 · ortak bileşen: süre | eşlendi |
| `03 §4.1.18` | Z-42 iade malını gönderme süresi: "mal dönmedi" kapanışı açılır | E-18 · E-37 | eşlendi |
| `03 §4.1.19` | Z-17 iptalin geri ödeme süresi | E-37 · ortak bileşen: süre | eşlendi |
| `03 §4.1.20` | Z-47 ifanın imkânsızlaştığının bildirimi: sayaç tutulmaz | E-37 · ekran dışı — e-posta (`08`) | eşlendi |
| `03 §4.1.21` | Z-18 ayıp talebinin süresi: kanal kapanır | E-16 | eşlendi |
| `03 §4.1.22` | Z-19 indirme hakkı: adet dolunca indirme kapanır | E-16 | eşlendi |
| `03 §4.1.23` | Z-20 indirimin tarih aralığı | E-33 · E-04 · ekran dışı — arka plan | eşlendi |
| `03 §4.1.24` | Z-21 referans fiyatın geriye bakış penceresi | ekran dışı — arka plan | ekran dışı |
| `03 §4.1.25` | Z-22 kuponun tarih aralığı | E-35 · E-13 | eşlendi |
| `03 §4.1.26` | Z-23 duyurunun tarih aralığı | ortak bileşen: çerçeve · E-41 | eşlendi |
| `03 §4.1.27` | Z-24 ileri tarihli yayın | E-33 · E-41 | eşlendi |
| `03 §4.1.28` | Z-25 KVKK başvurusuna cevap: sayaç tutulmaz | ekran dışı — ürünün dışında | ekran dışı |
| `03 §4.1.29` | Z-26 e-postanın yeniden denenmesi: "e-posta ulaşmadı" işareti | E-37 · ekran dışı — arka plan | eşlendi |
| `03 §4.1.30` | Z-27 deneme limitlerinin penceresi | ortak bileşen: mesaj · ekran dışı — arka plan | eşlendi |
| `03 §4.1.31` | Z-28 sipariş verisinin periyodik imhası | ekran dışı — arka plan | ekran dışı |
| `03 §4.1.32` | Z-41 ödemesi alınmamış siparişin kişisel verisinin imhası | ekran dışı — arka plan | ekran dışı |
| `03 §4.1.33` | Z-29 ayıp talebi kaydının imhası | ekran dışı — arka plan | ekran dışı |
| `03 §4.1.34` | Z-30 işlem izinin imhası | E-52 · ekran dışı — arka plan | eşlendi |
| `03 §4.1.35` | Z-31 hesap verisi silmeyle birlikte silinir | E-25 · ekran dışı — arka plan | eşlendi |
| `03 §4.1.36` | Z-32 sepet hesapla birlikte silinir | ekran dışı — arka plan | ekran dışı |
| `03 §4.1.37` | Z-33 iletişim talebinin silinmesi | E-39 · ekran dışı — arka plan | eşlendi |
| `03 §4.1.38` | Z-34 bildirim gönderim kaydı — hesap, yönetim, firma | ekran dışı — arka plan | ekran dışı |
| `03 §4.1.39` | Z-43 bildirim gönderim kaydı — B-1…B-16 | ekran dışı — arka plan | ekran dışı |
| `03 §4.1.40` | Z-35 geri ödeme IBAN'ının silinmesi | E-16 · ekran dışı — arka plan | eşlendi |
| `03 §4.1.41` | Z-36 periyodik imha: panelde düğme yoktur | ekran dışı — arka plan | ekran dışı |
| `03 §4.1.42` | Z-40 giriş kaydının imhası | ekran dışı — arka plan | ekran dışı |
| `03 §4.1.43` | Z-44 imha kaydının silinmesi | ekran dışı — arka plan | ekran dışı |
| `03 §4.1.44` | Z-45 geri alma bağlantısının ömrü | E-28 · E-25 | eşlendi |
| `03 §4.1.45` | Z-46 tanınan tarayıcı işareti | ekran dışı — arka plan | ekran dışı |
| `03 §4.1.46` | Z-37 veri ihlalinde Kurul'a bildirim: sayaç tutulmaz | ekran dışı — ürünün dışında | ekran dışı |
| `03 §4.1.47` | Z-38 malı dönmeyen caymada IBAN'ın kendiliğinden silinmesi | E-16 · E-37 · ekran dışı — arka plan | eşlendi |

**§4.2 Kendiliğinden işleyen anlar (12)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §4.2.1` | Sipariş onayı: numara, donma, ayırma | E-14 · ekran dışı — arka plan | eşlendi |
| `03 §4.2.2` | Ödeme onayı: kesin düşme, süreler, dijital teslim | E-16 · ekran dışı — arka plan | eşlendi |
| `03 §4.2.3` | Kendiliğinden iptal | E-16 · ekran dışı — arka plan | eşlendi |
| `03 §4.2.4` | İptal, çıkarma ve feshte stok ve kupon dönüşü | ekran dışı — arka plan | ekran dışı |
| `03 §4.2.5` | İadede stok ve kontenjan dönüşü | E-37 · ekran dışı — arka plan | eşlendi |
| `03 §4.2.6` | Kısmi iptal ve iadede kupon ve kargo | ekran dışı — arka plan | ekran dışı |
| `03 §4.2.7` | Kart hattında kendiliğinden geri ödeme; onay penceresi yoktur | E-37 · ekran dışı — arka plan | eşlendi |
| `03 §4.2.8` | Malı dönmeyen caymada IBAN'ın silinmesi | ekran dışı — arka plan | ekran dışı |
| `03 §4.2.9` | Periyodik imha ve imha kaydı | ekran dışı — arka plan | ekran dışı |
| `03 §4.2.10` | E-postanın yeniden denenmesi; panel ana sayfasında kanal uyarısı | E-31 · ekran dışı — arka plan | eşlendi |
| `03 §4.2.11` | Sitenin kesintisinde süreler işler | ekran dışı — arka plan | ekran dışı |
| `03 §4.2.12` | Havale süresi kesintide dolarsa iptal ertelenir | E-37 · ekran dışı — arka plan | eşlendi |

**§5.1 Üç yol ve itirazın yeri (7)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §5.1.1` | Sipariş sayfası üç yolu ayrı düğmelerle sunar: iptal, cayma, ayıp talebi | E-16 | eşlendi |
| `03 §5.1.2` | Üç yola girmeyen konu: iletişim formu, "Sipariş hakkında" | E-10 | eşlendi |
| `03 §5.1.3` | Yönetici talebi okur, cevabı e-postayla verir; işlem panelin var olan işlemiyle | E-39 · E-37 | eşlendi |
| `03 §5.1.4` | Çözülmüş ayıp tekrarlar: talep yeniden açılır; süre dolduysa form | E-16 · E-19 · E-10 | eşlendi |
| `03 §5.1.5` | Müşteri cevabı kabul etmez: uyuşmazlık yolları donmuş formda ve sözleşmede | E-16 · ekran dışı — ürünün dışında | eşlendi |
| `03 §5.1.6` | Merci müşteri lehine karar verir: tutar bazlı geri ödeme, IBAN isteği | E-37 · E-16 | eşlendi |
| `03 §5.1.7` | Yönetici kanıt hazırlar: donmuş metinler, gönderim kaydı, işlem izi | E-37 · E-52 | eşlendi |

**§5.2 Ürünün dışında çözülen anlaşmazlıklar (17)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §5.2.1.1` | Ters ibraz: ürün bilmez | ekran dışı — ürünün dışında | ekran dışı |
| `03 §5.2.1.2` | Yönetici itirazı sağlayıcının panelinde cevaplar; sonucu iç nota yazar | ekran dışı — ürünün dışında · E-37 | eşlendi |
| `03 §5.2.1.3` | Geri ödeme adımından önce ters ibraza bakma uyarısı | E-37 | eşlendi |
| `03 §5.2.1.4` | Ters ibrazlı işlemde kendiliğinden iade: fazla ürünün dışında çözülür | ekran dışı — ürünün dışında | ekran dışı |
| `03 §5.2.2.1` | İade gönderisi ulaşmaz: "iade malı bekleniyor" | E-37 · E-16 | eşlendi |
| `03 §5.2.2.2` | Müşteri kaybı iletişim formundan bildirir; dosya eki yoktur | E-10 | eşlendi |
| `03 §5.2.2.3` | "Mal dönmedi" kapanışı | E-37 | eşlendi |
| `03 §5.2.2.4` | Firma ödemeye karar verir: tutar bazlı geri ödeme, IBAN isteği | E-37 · E-16 | eşlendi |
| `03 §5.2.2.5` | Mal sonradan ulaşır: kalem yeniden açılır | E-37 | eşlendi |
| `03 §5.2.3.1` | Eksik bilgilendirme iddiasıyla cayma isteği: düğme açılmaz | E-16 · E-10 | eşlendi |
| `03 §5.2.3.2` | Yönetici pencere dışı kaydı ayrıca onaylar; panel uyarır | E-37 · ortak bileşen: onay | eşlendi |
| `03 §5.2.3.3` | Kayıttan sonra olağan cayma hattı | E-37 · E-16 | eşlendi |
| `03 §5.2.3.4` | Yönetici kayıt yapmaz: cevap e-postayla | ekran dışı — ürünün dışında | ekran dışı |
| `03 §5.2.4.1` | Teslim tarihine itiraz: düğme açılmaz | E-16 | eşlendi |
| `03 §5.2.4.2` | Müşteri caymayı başka kanaldan bildirir | ekran dışı — ürünün dışında · E-37 | eşlendi |
| `03 §5.2.4.3` | Teslim tarihinin düzeltilmesi ya da bildirimin kaydı; sıra sorusu | E-37 | eşlendi |
| `03 §5.2.4.4` | Yönetici tarihi doğru bulur: ürün karar vermez | ekran dışı — ürünün dışında | ekran dışı |

**§5.3 İade reddi ve değer kaybı (9)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §5.3.1.1` | Koşullu istisnalı kalemde beyan ekranı ambalaj koşulunu yazar | E-18 | eşlendi |
| `03 §5.3.1.2` | Teslim alma adımında "iade reddedildi" seçimi ve geri alınamaz onay | E-37 · ortak bileşen: onay | eşlendi |
| `03 §5.3.1.3` | Reddin sonuçları: geri ödeme yok, IBAN silinir, sayaçtan düşer | E-37 · E-16 · E-31 | eşlendi |
| `03 §5.3.1.4` | Mal müşteriye geri gönderilir: sistemin dışında | ekran dışı — ürünün dışında | ekran dışı |
| `03 §5.3.1.5` | Müşteri reddi kabul etmez: iletişim formu ve uyuşmazlık yolu | E-10 · ekran dışı — ürünün dışında | eşlendi |
| `03 §5.3.1.6` | Karardan dönülür: tutar bazlı geri ödeme, IBAN isteği | E-37 · E-16 | eşlendi |
| `03 §5.3.2.1` | Hasarlı dönen malda ret seçeneği sunulmaz | E-37 | eşlendi |
| `03 §5.3.2.2` | Geri ödeme tam işlenir; kesinti alanı yoktur | E-37 | eşlendi |
| `03 §5.3.2.3` | Değer kaybı talebi ürünün dışında; iç not | ekran dışı — ürünün dışında · E-37 | eşlendi |

**§6.1 Deneme limitleri (22)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §6.1.1.1` | L-1 başarısız şifreli giriş ve yeniden doğrulama | E-22 · E-24 · E-29 · ortak bileşen: mesaj | eşlendi |
| `03 §6.1.1.2` | L-2 şifre sıfırlama talebi | E-23 · E-29 · ortak bileşen: mesaj | eşlendi |
| `03 §6.1.1.3` | L-3 hesap kaydı | E-20 · ortak bileşen: mesaj | eşlendi |
| `03 §6.1.1.4` | L-4 iletişim formu gönderimi; tuzak alan sessizce düşer | E-10 · ortak bileşen: mesaj | eşlendi |
| `03 §6.1.1.5` | L-5 başarısız misafir sipariş takibi sorgusu | E-15 · ortak bileşen: mesaj | eşlendi |
| `03 §6.1.1.6` | L-6 geçersiz kupon kodu | E-13 · ortak bileşen: mesaj | eşlendi |
| `03 §6.1.1.7` | L-7 kendiliğinden iptal edilen ödenmemiş sipariş | E-13 · ortak bileşen: mesaj | eşlendi |
| `03 §6.1.1.8` | L-8 aynı anda açık ödenmemiş sipariş | E-13 · ortak bileşen: mesaj | eşlendi |
| `03 §6.1.1.9` | L-9 doğrulama bağlantısının yeniden istenmesi | E-20 · E-25 · ortak bileşen: mesaj | eşlendi |
| `03 §6.1.2.1` | Eşik aşılır: yalnız o işlem geçici engellenir, mesaj nötr | ortak bileşen: mesaj | eşlendi |
| `03 §6.1.2.2` | Engel kendiliğinden açılır | ekran dışı — arka plan | ekran dışı |
| `03 §6.1.2.3` | Yönetici limite takılır: muafiyet yoktur | E-29 · ortak bileşen: mesaj | eşlendi |
| `03 §6.1.2.4` | Ürün düzeyinde limiti olmayan işlem: `05`'in işi | ekran dışı — mimari (`05`) | ekran dışı |
| `03 §6.1.3.1` | Hedefin e-postasıyla şifre denemesi: e-posta ekseni engeller | ekran dışı — arka plan | ekran dışı |
| `03 §6.1.3.2` | Tanınan tarayıcı e-posta ekseninde engellenmez | ekran dışı — arka plan | ekran dışı |
| `03 §6.1.3.3` | Yeni tarayıcıdan giremeyen kullanıcı şifre sıfırlamayı kullanır | E-22 · E-23 | eşlendi |
| `03 §6.1.3.4` | Sıfırlama talepleri de doldurulur (L-2): kalan risk | E-23 · ortak bileşen: mesaj | eşlendi |
| `03 §6.1.3.5` | Google ile giriş L-1 engelinden etkilenmez | E-22 | eşlendi |
| `03 §6.1.4.1` | Ödemeden art arda sipariş: kendiliğinden iptal ve L-7 | ekran dışı — arka plan · E-13 | eşlendi |
| `03 §6.1.4.2` | Havale siparişleriyle stok kilitleme: L-8 | E-13 | eşlendi |
| `03 §6.1.4.3` | Çok IP'den misafir havale siparişi: firmanın incelemesi | E-31 · E-36 | eşlendi |
| `03 §6.1.4.4` | Paylaşılan IP'de gerçek müşteri engellenir: kalan risk | ortak bileşen: mesaj | eşlendi |

**§6.2 Teyit ve kandırılma akışları (21)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §6.2.1.1` | Siparişin iletişim e-postasından gelen talep teyittir | E-37 | eşlendi |
| `03 §6.2.1.2` | Başka kanaldan gelen talep: sipariş bulunur, siparişin kanalına dönülür | E-36 · E-37 | eşlendi |
| `03 §6.2.1.3` | Misafir e-postası düzeltmesinde kimlik siparişteki bilgilerle teyit edilir | E-37 | eşlendi |
| `03 §6.2.1.4` | Sahibine ulaşılamaz: değerlendirme firmanın | ekran dışı — ürünün dışında | ekran dışı |
| `03 §6.2.2.1` | Adres çevirtme girişimi: teyit kalıbı | E-37 | eşlendi |
| `03 §6.2.2.2` | Adres düzeltmesi işlem izine yazılır | E-37 · E-52 | eşlendi |
| `03 §6.2.2.3` | Gerçek sahip B-10'u görür: adres geri düzeltilir | E-37 · ekran dışı — e-posta (`08`) | eşlendi |
| `03 §6.2.3.1` | Sahte cayma ya da iptal bildirimi: teyit | E-37 | eşlendi |
| `03 §6.2.3.2` | Teyit atlanır: bildirimdeki IBAN aktarılmaz | E-37 | eşlendi |
| `03 §6.2.3.3` | Yanlış iptal düzeltilir; cayma beyanı geri alınmaz | E-37 | eşlendi |
| `03 §6.2.4.1` | Misafir e-postasını çevirtme girişimi: teyit | E-37 | eşlendi |
| `03 §6.2.4.2` | E-posta düzeltilir: yeni anahtar, "e-posta düzeltildi" işareti | E-37 | eşlendi |
| `03 §6.2.4.3` | Kandıran sipariş sayfasına girer ve işlem yapar | E-16 | eşlendi |
| `03 §6.2.4.4` | Gerçek sahip B-15'i görür: yeniden düzeltme | E-37 · ekran dışı — e-posta (`08`) | eşlendi |
| `03 §6.2.5.1` | Hesabın e-postası ele geçirilmiş oturumdan değiştirilir: yeniden doğrulama | E-24 · E-25 | eşlendi |
| `03 §6.2.5.2` | Gerçek sahip geri alma bağlantısını kullanır | E-28 | eşlendi |
| `03 §6.2.5.3` | Z-45 dolmuş: geri alma yolu yok; siparişlere e-postadaki bağlantıyla girilir | E-16 | eşlendi |
| `03 §6.2.6.1` | Havale IBAN'ı ele geçirilmiş yönetici hesabıyla değiştirilir | E-45 · E-52 | eşlendi |
| `03 §6.2.6.2` | Yönetici F-5'i görür, IBAN'ı düzeltir, şifresini değiştirir | E-45 · E-50 · ekran dışı — e-posta (`08`) | eşlendi |
| `03 §6.2.6.3` | Yanlış IBAN'lı siparişler firma iptaliyle kapatılır | E-37 | eşlendi |
| `03 §6.2.6.4` | Öteki yöneticiler kaldırılır: herkese bildirim; son yönetici kaldırılamaz | E-49 · ekran dışı — e-posta (`08`) | eşlendi |

**§9.1 Kayıt, e-posta doğrulama ve Google ile ilk giriş (7)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §9.1.1` | Kayıt ekranı; aydınlatma kapısı ve bağlantısı; "Google ile giriş" düğmesi | E-20 | eşlendi |
| `03 §9.1.2` | Ad, e-posta ve şifreyle kayıt; şifre politikası; "bu e-posta zaten kayıtlı" | E-20 | eşlendi |
| `03 §9.1.3` | Doğrulama bağlantısını ekrandan yeniden isteme (L-9) | E-20 · E-22 | eşlendi |
| `03 §9.1.4` | Doğrulama bağlantısı açılır: hesap doğrulanır, misafir siparişleri hesaba düşer | E-21 · E-27 | eşlendi |
| `03 §9.1.5` | Bağlantı süresinde açılmaz: kayıt silinir, "bağlantı geçersiz" | E-21 | eşlendi |
| `03 §9.1.6` | Google ile ilk giriş: hesap doğrulanmış ve şifresiz doğar | E-22 · ekran dışı — ürünün dışında | eşlendi |
| `03 §9.1.7` | Google e-postayı doğrulanmamış verir: kayda yönlendirme | E-22 · E-20 | eşlendi |

**§9.2 Giriş, oturum, şifre sıfırlama ve yeniden doğrulama (8)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §9.2.1` | E-posta ve şifreyle giriş; "beni hatırla" | E-22 | eşlendi |
| `03 §9.2.2` | Google ile giriş mevcut hesaba bağlanır; hesap iki giriş yolu taşır | E-22 · E-25 (GA-3) | **GAP adayı** |
| `03 §9.2.3` | Çıkış ya da oturumun kendiliğinden kapanması | ortak bileşen: çerçeve · E-22 | eşlendi |
| `03 §9.2.4` | "Şifremi unuttum": nötr mesaj; şifresiz hesapta "şifre belirle" | E-23 | eşlendi |
| `03 §9.2.5` | Bağlantıdan yeni şifre kurma; bütün oturumlar kapanır | E-23 | eşlendi |
| `03 §9.2.6` | Yeniden doğrulama isteyen işlemler: şifre, e-posta, hesap silme | E-24 | eşlendi |
| `03 §9.2.7` | Yönetici panele girer: ayrı giriş, yalnız şifre | E-29 | eşlendi |
| `03 §9.2.8` | Yönetici panel şifresini sıfırlar | E-29 | eşlendi |

**§9.3 Profil ve adres defteri (7)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §9.3.1` | Adın değiştirilmesi | E-25 | eşlendi |
| `03 §9.3.2` | Adres defteri: ekleme, adlandırma, düzenleme, silme | E-26 · ortak bileşen: adres | eşlendi |
| `03 §9.3.3` | Şifre değiştirme | E-25 · E-24 | eşlendi |
| `03 §9.3.4` | E-posta değiştirme isteği: yeni adres, doğrulanana kadar geçersiz | E-25 · E-24 (GA-4) | **GAP adayı** |
| `03 §9.3.5` | Yeni adresteki bağlantı açılır: değişiklik geçerli olur | E-21 · ekran dışı — e-posta (`08`) | eşlendi |
| `03 §9.3.6` | Eski adresteki geri alma bağlantısı açılır | E-28 | eşlendi |
| `03 §9.3.7` | Yönetici kendi e-posta adresini değiştirir | E-50 | eşlendi |

**§9.4 Hesabın silinmesi ve kişisel veri başvurusu (7)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §9.4.1` | Üye hesabını silmek ister: yeniden doğrulama; geri alma bağlantısı açıkken engel | E-25 · E-24 (GA-1) | **GAP adayı** |
| `03 §9.4.2` | Sistem hesabı siler | ekran dışı — arka plan | ekran dışı |
| `03 §9.4.3` | Silinmiş hesabın siparişi misafir yolundan izlenir | E-15 · E-16 | eşlendi |
| `03 §9.4.4` | Kişisel veri başvurusu: iletişim formunun "KVKK talebi" tipi | E-10 | eşlendi |
| `03 §9.4.5` | Yönetici üye kaydı görünümünde e-postayla arar | E-40 | eşlendi |
| `03 §9.4.6` | Yönetici talep üzerine hesabı siler: geri alınamaz onay | E-40 · ortak bileşen: onay | eşlendi |
| `03 §9.4.7` | Aynı e-postayla yeniden kayıt: yeni hesap | E-20 | eşlendi |

**§9.5 Tercih yönetimi (6)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §9.5.1` | Bildirim tercihi ekranı yoktur | yoktur — kapsam dışı | ekran dışı |
| `03 §9.5.2` | Pazarlama onayı alanı yoktur | yoktur — kapsam dışı | ekran dışı |
| `03 §9.5.3` | Dil seçimi yoktur | yoktur — kapsam dışı | ekran dışı |
| `03 §9.5.4` | Çerez onay bandı yoktur; çerez politikası altbilgide | yoktur — kapsam dışı | ekran dışı |
| `03 §9.5.5` | Uygulama içi veri indirme yoktur | yoktur — kapsam dışı | ekran dışı |
| `03 §9.5.6` | Panelde bildirim tercihi ve ayrı bildirim adresi yoktur | yoktur — kapsam dışı | ekran dışı |

#### 1.1.5 Ürün Gereksinimleri — kural düzeyi: vitrin ve site kuralları, `04`'e iş bırakan cümleler (87)

Akışa girmeyen görünüm ve site kuralları ile `04`'e açıkça iş bırakan cümleler kural kimliğiyle (K-727). İki yüzü olan kural (vitrin ve panel) burada iki tarafın adayına birlikte eşlenmiştir; yalnız panele bakan `02 §3.31`, §10.1, §10.6 ve §10.8 §1.1.9'dadır.

**`04`'e açıkça iş bırakan cümleler — §3.27–§3.34 dışındakiler (12)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `02 §3 (giriş)` | Bir kuralın biçimini, metnini ya da yerini `04`'e bırakan ifade arayüz kararını `04`'e bırakır | ortak bileşen: taban | eşlendi |
| `02 §3.1.4` | Yasal kimlik bilgileri sürekli erişilebilir, "İletişim" başlığı altında; yerleşimi `04`'ün işi; ETBİS bandı | ortak bileşen: çerçeve · E-10 | eşlendi |
| `02 §3.9.2` | İndirimin başlangıç ve bitiş tarihi ürün sayfasında ve kartta; biçimi `04`'ün işi | E-04 · ortak bileşen: kart | eşlendi |
| `02 §3.13.3` | "Bu e-posta kayıtlı" giriş hatırlatması; biçimi `04`'ün işi | E-13 | eşlendi |
| `02 §3.16.4` | Sepet birleşme mesajı; metni ve yeri `04`'ün işi | E-12 · ortak bileşen: mesaj | eşlendi |
| `02 §3.19.5` | "Ücretsiz kargoya … kaldı" ve "Kargo ücretsiz"; biçimi ve yeri `04`'ün işi | E-12 · E-13 | eşlendi |
| `02 §3.20.1` | Teslimat illeri kısıtı adres adımından önce görünür; biçimi `04`'ün işi | E-04 · E-12 · E-13 | eşlendi |
| `02 §3.24.1` | Ödeme yükümlülüğü yazan düğme; metin ve biçim `04`'ün işi | E-13 | eşlendi |
| `02 §3.24.2` | Dijital kalemde üçüncü onay kutusu; nihai metin `04`'ün işi | E-13 | eşlendi |
| `02 §3.24.6` | Onay kutularının kaydı sipariş sayfasında görünür; kutu metni `04`'ün işi | E-16 · E-13 | eşlendi |
| `02 §6 (giriş)` | Kullanıcıya gösterilen hata metinlerinin biçimi ve yeri `04`'ün işi | ortak bileşen: mesaj | eşlendi |
| `02 §11 (envantere girmeyenler)` | Kırılma noktaları parametre değildir, `04`'ün işidir | ortak bileşen: taban | eşlendi |

**§3.27 Kurumsal içerik (28)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `02 §3.27.1` | Kurumsal içerik hazır tiplerle girilir; sayfa kurucu yoktur | E-41 · E-08 | eşlendi |
| `02 §3.27.2` | Beş hazır tip, genel sayfa ve duyuru; ayrı galeri tipi yoktur | E-41 | eşlendi |
| `02 §3.27.3` | Hakkımızda tek kayıttır, silinmez | E-41 · E-08 | eşlendi |
| `02 §3.27.4` | Hizmet tanıtımı fiyatsızdır; "Bize ulaşın" düğmesi | E-08 · E-07 · E-10 | eşlendi |
| `02 §3.27.5` | Referans iş: başlık, kısa açıklama, metin, görseller, kendi sayfası | E-08 · E-07 | eşlendi |
| `02 §3.27.6` | SSS tek sayfada, başlıksız listelenir | E-09 | eşlendi |
| `02 §3.27.7` | Şubeler İletişim sayfasında listelenir; "Haritada aç" | E-10 · E-41 | eşlendi |
| `02 §3.27.8` | İletişim sayfası ayrı içerik tipi değildir: kimlik, şubeler, form | E-10 | eşlendi |
| `02 §3.27.9` | Genel sayfa: başlık, metin, görseller; düzeni sabit | E-08 · E-41 | eşlendi |
| `02 §3.27.10` | Bağlı ürünler "İlgili ürünler" başlığıyla vitrin kartı olarak | E-08 · E-41 · ortak bileşen: kart | eşlendi |
| `02 §3.27.11` | Listeler panelde elle sıralanır; vitrin bu sırayı izler | E-41 · E-07 | eşlendi |
| `02 §3.27.12` | Sosyal medya bağlantıları ve WhatsApp sitenin üst ve alt bölümünde | ortak bileşen: çerçeve · E-42 | eşlendi |
| `02 §3.27.13` | Hiçbir içerik tipi zorunlu değildir; yayın kapısının zorunlu alanları | E-41 | eşlendi |
| `02 §3.27.14` | Hakkımızda boşken kurumsal blok marka adını ve logoyu gösterir | E-01 | eşlendi |
| `02 §3.27.15` | Görsel ve kısa alan kuralları; alternatif metin | E-41 · E-08 | eşlendi |
| `02 §3.27.16` | Uzun metinler kapalı metin biçimi setini kullanır | E-41 · E-08 | eşlendi |
| `02 §3.27.17` | Gömülü video oynatıcı yoktur; video bağlantıdır | E-08 | eşlendi |
| `02 §3.27.18` | Dosya eki yoktur | yoktur — kapsam dışı | ekran dışı |
| `02 §3.27.19` | Müşteri görüşleri ayrı tip değildir; dönen alıntı bloğu yoktur | yoktur — kapsam dışı | ekran dışı |
| `02 §3.27.20` | Blog yoktur | yoktur — kapsam dışı | ekran dışı |
| `02 §3.27.21` | Duyuru şeridi: tek, kısa, tarihli; her sayfanın en üstünde | ortak bileşen: çerçeve · E-41 | eşlendi |
| `02 §3.27.22` | Site boş kurulur; boş sitenin görünüşü kurallarla tanımlıdır | E-01 · ortak bileşen: çerçeve | eşlendi |
| `02 §3.27.23` | Her içerik kaydı Taslak ya da Yayında | E-41 | eşlendi |
| `02 §3.27.24` | Taslak yalnız adla kaydedilir; yayın zorunlu alanları ister | E-41 | eşlendi |
| `02 §3.27.25` | Taslak içerik yöneticiye "Taslak" bandıyla; sayfasız kaydın önizlemesi `04`'ün işi | ortak bileşen: taslak · E-06 · E-41 | eşlendi |
| `02 §3.27.26` | İçerikte sürüm geçmişi yoktur | E-41 | eşlendi |
| `02 §3.27.27` | Kalıcı silme geri alınamaz işlem uyarısıyla | E-41 · ortak bileşen: onay | eşlendi |
| `02 §3.27.28` | Kurumsal içerikte ileri tarihli yayın yoktur | yoktur — kapsam dışı | ekran dışı |

**§3.28 Ana sayfa ve menü (6)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `02 §3.28.1` | İki hazır ana sayfa düzeni: tanıtım öncelikli, mağaza öncelikli | E-01 · E-42 | eşlendi |
| `02 §3.28.2` | Kurumsal blok her zaman Hakkımızda'dır | E-01 | eşlendi |
| `02 §3.28.3` | "Ana sayfada göster" işaretli kayıtların blokları; blok yeri ve kayıt sayısı `04`'ün işi | E-01 · E-41 | eşlendi |
| `02 §3.28.4` | Menü sabit iskeletten kurulur; adlar değişir; sıra ve görünüm `04`'ün işi | ortak bileşen: çerçeve · E-42 | eşlendi |
| `02 §3.28.5` | Genel sayfalar altbilgide; "menüde göster" işareti | ortak bileşen: çerçeve · E-41 | eşlendi |
| `02 §3.28.6` | Boş kategori menüde görünmez; adresinde boş kategori sayfası | ortak bileşen: çerçeve · E-02 | eşlendi |

**§3.29 Marka kimliği (6)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `02 §3.29.1` | Logo, marka adı, site simgesi ve tek marka rengi | E-43 · ortak bileşen: taban | eşlendi |
| `02 §3.29.2` | Logo yoksa marka adı yazıyla | ortak bileşen: çerçeve · E-01 | eşlendi |
| `02 §3.29.3` | Marka adı zorunludur; üst bölümde, sekmede, e-postalarda | ortak bileşen: çerçeve · E-43 | eşlendi |
| `02 §3.29.4` | Site simgesi isteğe bağlı; yoksa baş harf | E-43 · ortak bileşen: taban | eşlendi |
| `02 §3.29.5` | Marka rengi okunurluğu bozamaz; rengin yerleri `04`'ün işi | ortak bileşen: taban | eşlendi |
| `02 §3.29.6` | "Shopfolio ile kuruldu" imzası altbilgide; panelden kapatılır | ortak bileşen: çerçeve · E-43 | eşlendi |

**§3.30 Sayfa adresleri, arama motorları ve paylaşım (7)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `02 §3.30.1` | Ürün ve kategori adresi addan üretilir | ekran dışı — mimari (`05`) | ekran dışı |
| `02 §3.30.2` | Adres ilk yayında sabitlenir | ekran dışı — mimari (`05`) | ekran dışı |
| `02 §3.30.3` | Yönlendirme sistemi yoktur | yoktur — kapsam dışı | ekran dışı |
| `02 §3.30.4` | "Sayfa bulunamadı" sayfası sitenin görünümünde, dönüş yollarıyla | E-06 | eşlendi |
| `02 §3.30.5` | Arama motoru bilgileri otomatik üretilir | ekran dışı — mimari (`05`) | ekran dışı |
| `02 §3.30.6` | Arama motorlarına kapalı sayfalar | ekran dışı — mimari (`05`) | ekran dışı |
| `02 §3.30.7` | Ürün ve içerik sayfasında tek "Paylaş" düğmesi | E-04 · E-08 | eşlendi |

**§3.32 İletişim talebi (10)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `02 §3.32.1` | İletişim formu girişsiz açıktır; üyede ön dolu | E-10 | eşlendi |
| `02 §3.32.2` | Formun alanları; dosya eki yoktur | E-10 | eşlendi |
| `02 §3.32.3` | Kapalı beş konu tipi | E-10 | eşlendi |
| `02 §3.32.4` | Talebin iki durumu: Açık, Kapatıldı | E-39 | eşlendi |
| `02 §3.32.5` | Sistem talebi kaydeder ve iletir; panelde yanıt yazma ekranı yoktur | E-39 · ekran dışı — e-posta (`08`) | eşlendi |
| `02 §3.32.6` | Müşteri talebini takip etmez; numara ve "taleplerim" sayfası yoktur | E-10 | eşlendi |
| `02 §3.32.7` | Görünmez tuzak alan ve gönderim limiti; captcha yoktur | E-10 | eşlendi |
| `02 §3.32.8` | Aydınlatma metni tamamlanmadan form kapalıdır | E-10 | eşlendi |
| `02 §3.32.9` | Ayrı şikâyet kanalı ve canlı destek yoktur | yoktur — kapsam dışı | ekran dışı |
| `02 §3.32.10` | İndirme hakkı yenileme ve siparişe dair özel istek bu formdan | E-10 | eşlendi |

**§3.33 Yasal metinler (9)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `02 §3.33.1` | Dört sürümlü yasal metin | E-47 · E-11 · E-13 | eşlendi |
| `02 §3.33.2` | Firmanın düzenlediği iki metin zorunludur | E-47 | eşlendi |
| `02 §3.33.3` | Sürüm kendiliğinden artar | E-47 · ekran dışı — arka plan | eşlendi |
| `02 §3.33.4` | Metin değişince yeniden onay alınmaz; onay adımı açıkken güncel metin | E-13 | eşlendi |
| `02 §3.33.5` | Aydınlatma metni bağlantısı kişisel veri alanı açan her ekranda | E-20 · E-22 · E-13 · E-16 · E-17 · E-18 · E-19 · E-10 | eşlendi |
| `02 §3.33.6` | Form ve sözleşme sipariş onay e-postasının gövdesinde | ekran dışı — e-posta (`08`) | ekran dışı |
| `02 §3.33.7` | Uyuşmazlık bilgisi iki metnin zorunlu parçası | E-13 · E-16 | eşlendi |
| `02 §3.33.8` | Ayarlarla çelişen serbest metin firmanın sorumluluğunda | E-41 · E-47 | eşlendi |
| `02 §3.33.9` | Dört metin tamamlanmadan satış açılmaz; panel eksik metni adıyla gösterir | E-31 · E-47 | eşlendi |

**§3.34 Site geneli (8)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `02 §3.34.1` | Tek site, duyarlı tasarım; kırılma noktaları `04`'ün işi | ortak bileşen: taban | eşlendi |
| `02 §3.34.2` | Vitrin mobil öncelikli, panel masaüstü öncelikli; panelde hiçbir işlev mobilde kapanmaz | ortak bileşen: taban | eşlendi |
| `02 §3.34.3` | Tarayıcı tabanı son iki büyük sürüm | ekran dışı — doğrulama (`12`) | ekran dışı |
| `02 §3.34.4` | En dar desteklenen ekran genişliği bir parametredir | ortak bileşen: taban | eşlendi |
| `02 §3.34.5` | Erişilebilirlik tabanı: Kontrol Listesi A Seviyesi ve WCAG 2.2 | ortak bileşen: taban | eşlendi |
| `02 §3.34.6` | Ziyaretçi ölçümü yoktur | yoktur — kapsam dışı | ekran dışı |
| `02 §3.34.7` | Çalışma süresi taahhüdü ve planlı bakım penceresi yoktur | yoktur — kapsam dışı | ekran dışı |
| `02 §3.34.8` | Sipariş tarafında veri kaybı toleransı sıfır | ekran dışı — mimari (`05`) | ekran dışı |

**§12.4 Erişilebilirlik (1)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `02 §12.4.1` | Erişilebilirlik yasal yükümlülüktür; uyumluluk beyanı verilmez | ortak bileşen: taban · ekran dışı — doğrulama (`12`) | eşlendi |

#### 1.1.6 Ürün Gereksinimleri — alt bölüm düzeyi (79)

`02`'nin öteki kuralları alt bölüm düzeyinde eşlenir: adım adım hâlleri `03`'ün satırlarıdır ve §1.1.4 ile §1.1.8'de tek tek durur (K-727). Özet sütunu alt bölümün başlığıdır; eşleme, alt bölümün `03`'teki adımlarının ekranlarından türetilmiştir. Yalnız panele bakan 18 alt bölüm (`02 §3.31`, §6.6–§6.9, §8.5, §9.3, §10.1–§10.8, §11.1–§11.3) §1.1.9'dadır.

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `02 §1.1` | Sözlük kuralı | ekran dışı — yazım konvansiyonu | ekran dışı |
| `02 §1.2` | Terim sözlüğü | ekran dışı — yazım konvansiyonu | ekran dışı |
| `02 §1.3` | Aktörler | ekran dışı — yazım konvansiyonu | ekran dışı |
| `02 §2.1` | Akış envanteri | akış dizini — adımları `03 §2`, §8, §9 satırlarında (§1.1.4, §1.1.8) | eşlendi |
| `02 §2.2` | Uçtan uca ana akış | akış dizini — adımları `03 §2.10.1` satırlarında (§1.1.4) | eşlendi |
| `02 §2.3` | Kurumsal içerik akışı | akış dizini — adımları `03 §2.2`, §8.6 satırlarında (§1.1.4, §1.1.8) | eşlendi |
| `02 §3.1` | Firma, kurulum ve satış kapısı | E-44 · E-31 · ortak bileşen: satış-kapalı · ortak bileşen: çerçeve | eşlendi |
| `02 §3.2` | Satış modeli ve ürün tipleri | E-04 · E-33 | eşlendi |
| `02 §3.3` | Ürün, varyant ve seçenek | E-04 · E-33 | eşlendi |
| `02 §3.4` | Kategori | E-02 · E-34 · ortak bileşen: çerçeve | eşlendi |
| `02 §3.5` | Arama, süzgeç ve liste düzeni | E-02 · E-03 | eşlendi |
| `02 §3.6` | Stok ve tükenme | E-04 · E-12 · E-33 | eşlendi |
| `02 §3.7` | Ürünün yayın durumu, arşiv ve silme | E-05 · E-06 · E-32 · E-33 · ortak bileşen: taslak | eşlendi |
| `02 §3.8` | Fiyat ve KDV | E-04 · E-33 | eşlendi |
| `02 §3.9` | İndirim ve referans fiyat | E-04 · ortak bileşen: kart · E-33 | eşlendi |
| `02 §3.10` | Kupon | E-13 · E-35 | eşlendi |
| `02 §3.11` | Ürün görseli ve metni | E-04 · E-33 | eşlendi |
| `02 §3.12` | Dijital ürün | E-16 · E-33 · E-37 | eşlendi |
| `02 §3.13` | Üyelik, e-posta doğrulama ve oturum | E-20 · E-21 · E-22 · E-23 · E-24 · E-13 | eşlendi |
| `02 §3.14` | Adres | E-13 · E-26 · ortak bileşen: adres | eşlendi |
| `02 §3.15` | Hesabın silinmesi ve kişisel veri başvurusu | E-25 · E-40 · E-10 | eşlendi |
| `02 §3.16` | Sepet | E-12 | eşlendi |
| `02 §3.17` | Sipariş onayı ve stok ayırma | E-13 · ekran dışı — arka plan | eşlendi |
| `02 §3.18` | Asgari sipariş tutarı | E-12 · E-46 | eşlendi |
| `02 §3.19` | Kargo ücreti ve ücretsiz kargo eşiği | E-12 · E-13 · E-46 | eşlendi |
| `02 §3.20` | Teslim: bölge, yol, kargoya verme ve takip | E-04 · E-13 · E-16 · E-37 · E-46 | eşlendi |
| `02 §3.21` | Ödeme | E-13 · E-14 · E-37 · E-45 · ekran dışı — ürünün dışında | eşlendi |
| `02 §3.22` | Sipariş numarası ve takip erişimi | E-15 · E-16 · E-27 | eşlendi |
| `02 §3.23` | Sipariş anında donan değerler | E-16 · ekran dışı — arka plan | eşlendi |
| `02 §3.24` | Onay adımı ve yasal metinler | E-13 · E-16 | eşlendi |
| `02 §3.25` | Fatura | E-13 · E-53 · ekran dışı — ürünün dışında | eşlendi |
| `02 §3.26` | İptal, cayma, iade ve ayıp talebi | E-16 · E-17 · E-18 · E-19 | eşlendi |
| `02 §3.27` | Kurumsal içerik | kural düzeyinde — §1.1.5 | eşlendi |
| `02 §3.28` | Ana sayfa ve menü | kural düzeyinde — §1.1.5 | eşlendi |
| `02 §3.29` | Marka kimliği | kural düzeyinde — §1.1.5 | eşlendi |
| `02 §3.30` | Sayfa adresleri, arama motorları ve paylaşım | kural düzeyinde — §1.1.5 | eşlendi |
| `02 §3.32` | İletişim talebi | kural düzeyinde — §1.1.5 | eşlendi |
| `02 §3.33` | Yasal metinler | kural düzeyinde — §1.1.5 | eşlendi |
| `02 §3.34` | Site geneli: cihaz, tarayıcı, erişilebilirlik ve süreklilik | kural düzeyinde — §1.1.5 | eşlendi |
| `02 §4.1` | Sayım kuralları | ortak bileşen: süre · ekran dışı — arka plan | eşlendi |
| `02 §4.2` | Süreler | süre düzeyinde — `03 §4.1` satırlarında (§1.1.4) | eşlendi |
| `02 §4.3` | Kendiliğinden işleyen anlar | an düzeyinde — `03 §4.2` satırlarında (§1.1.4) | eşlendi |
| `02 §5.1` | Ürün ve varyantın yayın durumu | E-04 · E-05 · E-32 · E-33 · ortak bileşen: rozet | eşlendi |
| `02 §5.2` | Kurumsal içeriğin yayın durumu | E-08 · E-41 · ortak bileşen: rozet | eşlendi |
| `02 §5.3` | Sipariş: iki eksen ve oluşma anı | E-16 · E-37 · ortak bileşen: rozet | eşlendi |
| `02 §5.4` | Sevkiyat ekseni — Sipariş durumu | E-16 · E-37 · ortak bileşen: rozet | eşlendi |
| `02 §5.5` | Ödeme ekseni — Ödeme durumu | E-16 · E-37 · ortak bileşen: rozet | eşlendi |
| `02 §5.6` | İki eksenin bağı ve kendiliğinden geçişler | E-16 · E-37 · ekran dışı — arka plan | eşlendi |
| `02 §5.7` | Tipe göre sevkiyat hattı | E-16 · E-37 | eşlendi |
| `02 §5.8` | Sipariş kalemi düzeyindeki kayıtlar | E-16 · E-37 · ortak bileşen: rozet | eşlendi |
| `02 §5.9` | Geri alınamaz geçişler | E-37 · E-40 · ortak bileşen: onay | eşlendi |
| `02 §5.10` | Ayıp talebinin durumu | E-16 · E-38 · ortak bileşen: rozet | eşlendi |
| `02 §5.11` | İndirim ve kuponun zamana ve kullanıma bağlı geçerliliği | E-33 · E-35 · ekran dışı — arka plan | eşlendi |
| `02 §5.12` | Hesap ve oturum | E-22 · E-25 · ekran dışı — arka plan | eşlendi |
| `02 §5.13` | İletişim talebinin durumu | E-39 · ortak bileşen: rozet | eşlendi |
| `02 §6.1` | Bütün akışlara uygulanan ilkeler | satır düzeyinde — `03 §3.1` satırlarında (§1.1.4) | eşlendi |
| `02 §6.2` | Satın alma (akış 1) | satır düzeyinde — `03 §3.2.1` satırlarında (§1.1.4) | eşlendi |
| `02 §6.3` | Sipariş takibi (akış 2) | satır düzeyinde — `03 §3.2.2` satırlarında (§1.1.4) | eşlendi |
| `02 §6.4` | İptal, cayma ve iade (akış 3) | satır düzeyinde — `03 §3.3` satırlarında (§1.1.4) | eşlendi |
| `02 §6.5` | Üyelik (akış 4) | satır düzeyinde — `03 §3.4` satırlarında (§1.1.4) | eşlendi |
| `02 §7.1` | Üç yol ve ortak ilkeler | E-16 | eşlendi |
| `02 §7.2` | İptal | E-16 · E-17 · E-37 | eşlendi |
| `02 §7.3` | Cayma | E-16 · E-18 · E-37 | eşlendi |
| `02 §7.4` | İade ve geri ödeme | E-16 · E-18 · E-37 · ortak bileşen: IBAN | eşlendi |
| `02 §7.5` | Ayıp talebi | E-16 · E-19 · E-38 | eşlendi |
| `02 §7.6` | Geri alma sınırları | E-16 · E-37 · E-41 · ortak bileşen: onay | eşlendi |
| `02 §8.1` | Kimlik doğrulama tabanı | E-20 · E-22 · E-23 · E-24 · E-29 | eşlendi |
| `02 §8.2` | Deneme limitleri | ortak bileşen: mesaj — satır düzeyinde `03 §6.1` (§1.1.4) | eşlendi |
| `02 §8.3` | Form ve site güvenliği | E-10 · E-13 · E-16 · E-37 · ekran dışı — mimari (`05`) | eşlendi |
| `02 §8.4` | Veri asgariliği | E-16 · E-20 · E-37 · yoktur — kapsam dışı | eşlendi |
| `02 §9.1` | Kanal ve ilkeler | ekran dışı — e-posta (`08`) · E-37 | eşlendi |
| `02 §9.2` | Müşteriye giden bildirimler | ekran dışı — e-posta (`08`) · E-16 | eşlendi |
| `02 §9.4` | Hesap ve yönetim e-postaları | ekran dışı — e-posta (`08`) · E-20 · E-23 · E-25 · E-28 | eşlendi |
| `02 §9.5` | Olmayan bildirimler | yoktur — kapsam dışı | ekran dışı |
| `02 §12.1` | Ticari çerçeve ve tüketici rejimi | E-04 · E-13 · E-16 · ortak bileşen: çerçeve · ekran dışı — ürünün dışında | eşlendi |
| `02 §12.2` | Kişisel verilerin korunması | E-11 · E-10 · E-47 · yoktur — kapsam dışı · ekran dışı — arka plan | eşlendi |
| `02 §12.3` | Veri sahipliği ve süreklilik | ekran dışı — ürünün dışında | ekran dışı |
| `02 §12.4` | Erişilebilirlik | kural düzeyinde — §1.1.5 | eşlendi |
| `02 §12.5` | Firmanın yükümlülükleri | ekran dışı — ürünün dışında | ekran dışı |

#### 1.1.7 Devir dizinleri ve park satırları — müşteri tarafı (59)

Devrin evi karar satırıdır; dizin onu ikinci kez kaydetmez (`PHASE1_CONFLICT_SCAN.md` §6, `PHASE2_CONFLICT_SCAN.md` §6). Özet, dizinin verdiği iş adıdır; dizinin yalnız doküman numarasıyla işaretlediği Aşama 1 kararlarında kararın konu hücresinden alınmıştır. Aşama 1 dizininin §6.2'sindeki gövde cümleleri `02` kimliğiyle §1.1.5'tedir. Karşılıksız kalan devir GAP adayıdır (UI1-06).

**Park satırları (3)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| Park K-06 · K-97 | Ekran × rol matrisi dört aktör üzerinden; yönetim tek rol | §1.1.2'nin aktör sütunu · ekran dışı — yazım konvansiyonu | eşlendi |
| Park K-14 · K-574 | Firma kimlik bilgileri ve ETBİS bandı sitede sürekli erişilebilir; yerleşim `04`'ün kararı (UI3-01) | ortak bileşen: çerçeve · E-10 · E-44 | eşlendi |
| Park K-27 | İki hazır ana sayfa düzeni tasarlanır (UI5-01) | E-01 · E-42 | eşlendi |

**Aşama 1 dizini — müşteri tarafı (35 / 80)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| K-493 | Ön bilgilendirmede iade taşıyıcısı bilgisi — "form metni" | E-13 (GA-7) | **GAP adayı** |
| K-494 | Cayma beyanı ekranı; "iade malı bekleniyor" görünümü | E-18 · E-16 · E-37 | eşlendi |
| K-497 | Malı dönmeyen caymada IBAN'ın yeniden istenmesi | E-16 · E-37 | eşlendi |
| K-498 | Sipariş sayfasında yeniden açılan IBAN alanı; panelde "IBAN bekleniyor" | E-16 · E-31 · ortak bileşen: IBAN | eşlendi |
| K-499 | Gecikme feshi: kargodaki siparişte sınırın aşılması ve kanuni faiz | E-16 · E-17 · E-37 | eşlendi |
| K-503 | Hizmette cayma: onay kutusu, ifa süresi, karışık siparişte pencere | E-13 · E-16 · E-18 | eşlendi |
| K-505 | Ödenmemiş siparişte kısmi iptal yoktur | E-16 · E-17 | eşlendi |
| K-509 | Cayma istisnası sebeplerinin kapalı listesi | E-33 · E-13 · E-18 | eşlendi |
| K-511 | Fiyat etiketinin zorunlu bilgileri: üretim yeri, birim fiyat, fiyat tarihi | E-04 · E-33 | eşlendi |
| K-541 | Platform imzasının kuralı | ortak bileşen: çerçeve · E-43 | eşlendi |
| K-544 | Onaydan hemen önce ödeme yükümlülüğü bilgisi | E-13 | eşlendi |
| K-545 | İndirimin tarihlerinin vitrinde gösterilmesi | E-04 · ortak bileşen: kart | eşlendi |
| K-547 | Sözleşme öncesi bilgiler ve meslek odası | E-11 · E-10 · E-44 | eşlendi |
| K-550 | Üçüncü onay kutusunun metni | E-13 | eşlendi |
| K-553 | Aydınlatma bağlantısının göründüğü yerler ve başvuru kanalları | E-20 · E-22 · E-13 · E-16 · E-10 | eşlendi |
| K-561 | Adres formu | ortak bileşen: adres · E-13 · E-26 | eşlendi |
| K-569 | Sipariş sayfasının IBAN alanı | E-16 · ortak bileşen: IBAN | eşlendi |
| K-574 | Firma kimliği formu; ETBİS bandının yerleşimi | E-44 · ortak bileşen: çerçeve | eşlendi |
| K-575 | Sipariş sayfasının IBAN alanı; panel adımı | E-16 · E-37 | eşlendi |
| K-576 | Satış kapısı uyarıları | ortak bileşen: satış-kapalı · E-31 | eşlendi |
| K-578 | Hizmet kaleminin cayma ekranı | E-18 | eşlendi |
| K-586 | Sipariş sayfasında e-posta geçmişi | E-16 | eşlendi |
| K-588 | İki giriş ekranı — müşteri ve yönetici | E-22 · E-29 | eşlendi |
| K-589 | Sipariş sayfasında geç ödeme ve geri ödemesi | E-16 | eşlendi |
| K-591 | Onay adımında metin değişikliği satırı; kutuların yeniden işaretlenmesi | E-13 | eşlendi |
| K-593 | Hizmet kaleminin cayma düğmesi | E-16 | eşlendi |
| K-594 | Boş sepet mesajı | E-12 | eşlendi |
| K-595 | Sipariş sayfasında onay kutusunun kaydı | E-16 | eşlendi |
| K-599 | Hesabın e-postası değişince bildirim metni ve geri alma ekranı | E-28 · ekran dışı — e-posta (`08`) (GA-6) | **GAP adayı** |
| K-602 | Takip formunun nötr mesajı | E-15 · ortak bileşen: mesaj | eşlendi |
| K-605 | Kayıt formunda ad alanı; hesapta adı değiştirme | E-20 · E-25 | eşlendi |
| K-610 | Ürün sayfasında yerli üretim logosunun yeri | E-04 | eşlendi |
| K-625 | Erişilebilirlik tabanı — tasarım sistemi | ortak bileşen: taban | eşlendi |
| K-626 | "İşlem rehberi" sayfası ve altbilgi bağlantısı | E-11 · ortak bileşen: çerçeve | eşlendi |
| K-627 | "İletişim" başlığının bilgileri ve yerleşimi; kimlik formu | E-10 · ortak bileşen: çerçeve · E-44 | eşlendi |

**Aşama 2 dizini — müşteri tarafı (18 / 35)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| K-657 | Sipariş sayfasında reddedilen iade kaleminin görünümü | E-16 | eşlendi |
| K-659 | Doğrulama bağlantısını yeniden isteme mesajı | E-20 · E-22 · ortak bileşen: mesaj | eşlendi |
| K-663 | Sipariş sayfasında ve "ödendi" işaretinde donmuş IBAN; IBAN değişikliği onayında sayı | E-16 · E-14 · E-37 · E-45 | eşlendi |
| K-665 | Beyan ekranında güncel iade adresi; sipariş sayfasında beyanın adresi | E-18 · E-16 · E-46 | eşlendi |
| K-669 | İzlenebilirlik matrisinin atıf biçimi | ekran dışı — yazım konvansiyonu | ekran dışı |
| K-673 | Sipariş sayfasında hizmet cayma düğmesinin görünürlüğü | E-16 | eşlendi |
| K-676 | Sipariş sayfasında ayıp talebinin görünümü | E-16 | eşlendi |
| K-678 | İletişim formunun gönderim sonrası ekranı | E-10 | eşlendi |
| K-679 | Kupon alanı: kodu değiştirme ve kaldırma | E-13 (GA-2) | **GAP adayı** |
| K-680 | Ödeme adımında yeni adresi deftere kaydetme seçimi | E-13 | eşlendi |
| K-686 | Öteki hâllerde havale geri ödemesi için IBAN isteği | E-16 · E-37 | eşlendi |
| K-690 | Firma iptalinde ve kalem çıkarmada IBAN alanı | E-16 · E-37 · ortak bileşen: IBAN | eşlendi |
| K-697 | Google e-postayı doğrulanmamış verince yönlendirme mesajı | E-22 · E-20 | eşlendi |
| K-698 | Geri alma bağlantısı açıkken hesap silme engelinin mesajı | E-25 · E-40 | eşlendi |
| K-700 | Aktör listesi | ekran dışı — yazım konvansiyonu | ekran dışı |
| K-701 | Matriste `03` atıflarının okunuşu | ekran dışı — yazım konvansiyonu | ekran dışı |
| K-702 | Aktörü olmayan dış olayın yazımı | ekran dışı — yazım konvansiyonu | ekran dışı |
| K-704 | Teyit ve bekleme ekranı | E-14 · E-16 | eşlendi |

**Gövde cümleleri — müşteri tarafı (3 / 7)**

| Kaynak ID | Kaynak özeti | Ekran | Durum |
|---|---|---|---|
| `03 §0.1.4` | KP-37, KP-69, KP-70, KP-71'in karşılığı ekran tasarımında ve mimaride | ortak bileşen: taban · ekran dışı — mimari (`05`) · ekran dışı — doğrulama (`12`) | eşlendi |
| `03 §2.1.2` | İndirimdeki ürünün vitrin kartının düzeni | ortak bileşen: kart · E-02 | eşlendi |
| `03 §3.1.6` | Hata mesajlarının biçimi | ortak bileşen: mesaj | eşlendi |

#### 1.1.8 Kullanıcı Akışları — firma tarafı ve kalan (415)

**Kapı: matrisin 1b oturumu.** Kapsam: `03 §1.4` 13 · §1.5 7 · §1.6.2 4 · §1.7 17 · §1.10 8 · §1.11.15–§1.11.41 27 (= §1'in kalan 76 satırı) · §3.5 60 · §6.3 9 · §7 131 · §8 106 · §10 33. `03 §7`'nin bildirim satırları toplu olarak "ekran dışı — e-posta (`08`)" diye eşlenir; yalnız panelde ya da sipariş sayfasında iz bırakanlar ("e-posta ulaşmadı" işareti, e-posta geçmişi) ekrana eşlenir (K-727). Sütunlar ve değer kümesi §1.1'in başındaki ve §1.1.2'deki gibidir.

#### 1.1.9 Ürün Gereksinimleri — panel tarafı

**Kapı: matrisin 1b oturumu.** Kural düzeyinde 14 satır: `02 §3.31.1`, §3.31.2 · §3.3.4 · §10.1 (5) · §10.6 (3) · §10.6.2'nin "sıfır payda" maddesi · §10.8 (2). Alt bölüm düzeyinde 18 satır: `02 §3.31`, §6.6–§6.9, §8.5, §9.3, §10.1–§10.8, §11.1–§11.3.

#### 1.1.10 Devir dizinleri — panel tarafı

**Kapı: matrisin 1b oturumu.** Aşama 1 dizininden 45 karar: K-480 · K-484 · K-485 · K-491 · K-495 · K-496 · K-510 · K-512 · K-514 · K-522 · K-525 · K-534 · K-535 · K-536 · K-537 · K-538 · K-540 · K-554 · K-564 · K-565 · K-566 · K-570 · K-573 · K-577 · K-580 · K-581 · K-584 · K-585 · K-587 · K-590 · K-592 · K-596 · K-600 · K-601 · K-603 · K-604 · K-606 · K-607 · K-608 · K-609 · K-611 · K-612 · K-613 · K-616 · K-643. Aşama 2 dizininden 17 karar: K-653 · K-654 · K-655 · K-656 · K-660 · K-662 · K-664 · K-674 · K-675 · K-677 · K-703 · K-706 · K-709 · K-712 · K-714 · K-715 · K-717. `03`'ün gövdesinden 4 cümle: `03 §7.1.37`, §8.2.11, §8.9.2, §10.1.2.2 — üçü karar satırı olmadan devredilen iştir (yeniden gönderilebilen e-postaların kapsamı ve firma bildirimlerinin "e-posta ulaşmadı" işaretinin biçimi; UI9-01).

#### 1.1.11 GAP adayları — karara bağlanacak

**Kapı: matrisin 2. oturumu.** Aşağıdakiler **adaydır, karar değildir**: 2. oturum her adayı kaynakların tam metnine karşı doğrular, düşeni gerekçesiyle düşürür, kalanı proje sahibine sunulacak GAP listesine alır ve kararını §1.3'e ve karar kaydının §3'üne yazar (UI1-05). 1b bulduğu adayı bu tabloya ekler. Bir adayın satırları §1.1'in tablolarında "GAP adayı" durumunu ve adayın numarasını taşır.

| # | Aday | Dayandığı satırlar | Not |
|---|---|---|---|
| GA-1 | Müşterinin geri alınamaz işlemlerinde onay adımı var mı — siparişin iptali, cayma beyanı, gecikme feshi, hesabın silinmesi | `03 §2.7.1`, §2.8.1.2, §9.4.1 | `03 §1.6.1` ve `02 §7.6.1` onay penceresini yalnız yöneticinin işlemleri için sayar; `02 §7.6.7` cayma beyanının geri alınmadığını, `03 §9.4.1` silmenin geri alınmadığını söyler, müşteriye onay sorulup sorulmadığını söylemez |
| GA-2 | Kupon alanı sepette mi, ödeme adımında mı | `03 §2.4.4`, §3.2.1.12, §3.2.1.18 · KP-11 · K-679 | `03 §2.4.4` kuponu ödeme adımında yazar; `03 §3.2.1.18` "o sepetin kupon alanı" der; karar kaydının konu planı kupon alanını sepet ekranının konusunda sayar (§10.3 UI5-05). `02 §3.10` 1a'da satır satır okunmadı — 2. oturum okur |
| GA-3 | Üye, hesabına bağlanmış Google girişini kaldırabilir mi | `03 §9.2.2` | Hesap iki giriş yolu taşır; bağın kalktığı tek yer e-posta değişikliğinin geri alınmasıdır (`03 §9.3.6`). Hesap ekranının giriş yollarını gösterip göstermediği ve bağın kaldırılıp kaldırılamadığı yazılı değil |
| GA-4 | Bekleyen e-posta değişikliği hesap ekranında nasıl görünür; üye onu kendisi iptal edebilir mi | `03 §9.3.4`, §3.4.9 | Değişiklik yeni adres doğrulanana kadar geçerli olmaz ve bağlantının ömrüyle düşer; arada üyenin vazgeçme yolu yazılı değil |
| GA-5 | Ödeme adımındaki giriş hatırlatmasından girişe giden müşteri girişten sonra nereye döner | `03 §2.4.1` | Sepetlerin girişte birleştiği yazılı (`03 §2.3.6`); dönüş yeri ve o ana kadar yazılan e-posta ile adresin korunup korunmadığı yazılı değil |
| GA-6 | Devri `04`'e yazılmış bildirim metinleri `04`'ün kapsamında değil | K-599 ("bildirim metni") — panel tarafında K-601 ("B-10 metni"), K-616 ("bildirim metni") | E-postanın metni `08`'in işidir (karar kaydı §10.2); üç devir `08`'in park bloğuna taşınmalı ya da `04`'te yalnız ekrandaki izi kalmalı. K-601 ve K-616'yı 1b eşler |
| GA-7 | Devri `04`'e yazılmış yasal metin içeriği bir ekran değil | K-493 ("form metni" — Ön Bilgilendirme Formu'nda iade taşıyıcısı bilgisi) | Üretilen metnin içeriği `02 §3.33`'ün ve metin taslağının işidir; `04` yalnız metnin gösterildiği yeri (E-13) taşır. Panel tarafında aynı sınıftan K-603 (çerez politikası taslağında beşinci çerez) var — 1b eşler |

### 1.2 Geri izlenebilirlik (ekran → kaynak)

**Kapı: matrisin 2. oturumu.** Tablo §1.1.2'nin aday ekranlarından ve §1.1'in satırlarından betikle kurulur; kaynağı olmayan aday gerekçesiz eklemedir ve listelenir (UI1-04). 1a'nın ara denetimi: 53 adayın tamamı §1.1.3–§1.1.7'nin en az bir satırında geçer.

| Ekran | Beslendiği kaynak(lar) | Durum |
|---|---|---|

### 1.3 Boşluklar (GAP) ve kararlar

**Kapı: matrisin 2. oturumu.** GAP kararları ve üst dokümanlara geri besleme satırları buraya yazılır (K-726; UI1-05, UI1-07). Adaylar §1.1.11'dedir.

| # | Boşluk | Proje sahibi kararı | Nereye yansıdı |
|---|---|---|---|

> **Vaka:** Bu matris bir referans projede 7 boşluk yakaladı — hiçbiri o ana kadar hiçbir dokümanda adreslenmemiş ama arayüzde cevap gerektiren sorulardı.

---

## 2. Ortak bileşen kütüphanesi

> **Ne yazılır:** Tekrar eden UI kalıpları — durum rozetleri, modal'lar, geri sayım göstergeleri, boş/yükleniyor/hata durumları.
> **Ekran tanımlarından ÖNCE gelir.** Sonradan çıkarılırsa ekranlar tutarsız yazılır.

| Bileşen | Ne gösterir | Varyantlar | Kullanıldığı ekranlar |
|---|---|---|---|

## 3. Navigasyon haritası

> **Ne yazılır:** Ekranlar arası geçişler. **Ekran tanımlarından önce** konumlandırılır.

## 4. Ekran envanteri

| # | Ekran | Aktör(ler) | Amaç |
|---|---|---|---|

## 5. Ekran tanımları

### S<NN> — <Ekran adı>

- **Aktör:** ·  **Giriş noktası:** ·  **Çıkış noktaları:**
- **Kullanıcı buraya geldiğinde ilk ne görmeli:**
- **Bilgi hiyerarşisi:** (ne, nerede, hangi öncelikte)
- **Aksiyonlar:** (her aksiyon → hangi akış adımı, hangi doğrulama)
- **Validasyonlar:**
- **Durum × rol varyantları:** (bkz. §6)
- **Boş / yükleniyor / hata durumları:**
- **Responsive notları:**

## 6. Durum × Rol matrisi

> **Ne yazılır:** Her ekran için {ekran × rol × durum} kombinasyonları. Eksik kombinasyonlar burada yakalanır — bir referans projede tek bir detay ekranı 13 durum × 3 rol = ~52 varyant gerektirdi.

| Ekran | Rol | Durum | Ne gösterilir | Hangi aksiyonlar aktif |
|---|---|---|---|---|

## 7. Form ve validasyon envanteri

## 8. Lokalizasyon etkileri

> **Ne yazılır:** Çoklu dil desteğinin **bilgi mimarisine** etkisi — metin uzunluk farkları, tarih/sayı biçimleri, yazı yönü. Bu yüzeysel bir konu değildir; layout ve bileşen boyutlarını etkiler.

## 9. Yönetim (admin) ekranları

> **Not:** Yönetim ekranları toplam ekranların yarısı kadar olabilir. İkincil endişe olarak ele alınmaz.

---

*Shopfolio — UI Specifications v0.2*
