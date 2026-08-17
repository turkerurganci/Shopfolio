# Shopfolio — Product Discovery Status

**Versiyon: v0.2** | **Son güncelleme:** 2026-08-17

> **Doküman üretim döneminin karar kaydıdır.** Her karar **alındığı anda** buraya yazılır — hiçbir karar kaybolmaz.
> Bu dosya sürecin sonunda arşivlenir; alınan kararların nihai hâli `01`, `02` ve `10`'da yaşar.

---

## 1. Doküman durumu

| No | Doküman | Durum | Versiyon | Audit | Deep review | Cross-review | Checkpoint |
|---|---|---|---|---|---|---|---|
| 01 | Project Vision | ⏳ | — | ⬚ | ⬚ | ⬚ | ⬚ |
| 02 | Product Requirements | ⏳ | — | ⬚ | ⬚ | ⬚ | ⬚ |
| 03 | User Flows | ⬚ | — | ⬚ | ⬚ | ⬚ | ⬚ |
| 04 | UI Specs | ⬚ | — | ⬚ | ⬚ | ⬚ | ⬚ |
| 05 | Technical Architecture | ⬚ | — | ⬚ | ⬚ | ⬚ | ⬚ |
| 06 | Data Model | ⬚ | — | ⬚ | ⬚ | ⬚ | ⬚ |
| 07 | API Design | ⬚ | — | ⬚ | ⬚ | ⬚ | ⬚ |
| 08 | Integration Spec | ⬚ | — | ⬚ | ⬚ | ⬚ | ⬚ |
| 09 | Coding Guidelines | ⬚ | — | ⬚ | ⬚ | ⬚ | ⬚ |
| 10 | MVP Scope | ⏳ | — | ⬚ | ⬚ | ⬚ | ⬚ |
| 11 | Implementation Plan | ⬚ | — | ⬚ | ⬚ | ⬚ | ⬚ |
| 12 | Validation Protocol | ⬚ | — | ⬚ | ⬚ | ⬚ | ⬚ |

**Lejant:** ⬚ Başlanmadı · ⏳ Devam ediyor · ✓ Tamamlandı

---

## 2. Karar kaydı

> Her karar: konu · seçenekler · **alınan karar** · gerekçe · tarih · etkilediği dokümanlar.

| # | Tarih | Konu | Karar | Gerekçe | Etkilediği dokümanlar |
|---|---|---|---|---|---|
| K-01 | 2026-08-17 | Kiracılık modeli — Shopfolio kimin sitesi? | **Tek bir firmanın kendi sitesi.** Birden çok satıcının kendi mağazasını açtığı pazaryeri / platform modeli **kapsam dışıdır**. | Proje sahibi kararı. Pazaryeri modeli MVP'ye satıcı onboarding'i, satıcı bazlı yetkilendirme, komisyon hesabı ve satıcı mutabakatı akışlarını ekler; ürün tanımı bunları kapsamaz. | 01 §1, §3, §7 · 02 §1, §10 · 10 §3 |
| K-02 | 2026-08-17 | "Tek firma sitesi" — ısmarlama iş mi, yeniden kurulabilir ürün mü? | **Yeniden kurulabilir ürün; kurulum başına bir firma.** Firma kimliği, kurumsal içerik ve sayısal parametreler koda gömülmez — ayardan ve yönetim panelinden gelir. | `.claude/CONTEXT.md` §1'deki "**herhangi bir** firmanın" tanımıyla tutarlı; ikinci müşteriye kurulabilir. Kurumsal tanıtım tarafı zaten yönetilebilir içerik gerektirdiğinden, ısmarlamaya kıyasla ek maliyetin büyük bölümü kapsamda hâlihazırda var. | 01 §1, §3, §5, §6 · 02 §0, §10, §11 · 10 §2, §4 |
| K-03 | 2026-08-17 | Hedef kitle — Shopfolio'yu kuracak firma kim? | **Sektör bağımsız KOBİ / küçük işletme.** Hem kurumsal tanıtım yüzüne hem küçük ölçekli çevrimiçi satışa ihtiyaç duyan işletmeler. **Ölçek varsayımı:** birkaç yüz ürüne kadar katalog, tek depo. | "Kurumsal tanıtım + satış" ikilisi tam bu segmentin ihtiyacıdır. Dikey daraltma (moda/dekorasyon) varyant ve iade akışını MVP'de zorunlu kılarak kapsamı büyütürdü; B2B/bayi modeli ise açık fiyat + sepet + kartla ödeme akışını teklif ve cari mutabakatla değiştirip mevcut ürün tanımıyla çelişirdi. | 01 §3, §5, §6, §8 · 02 §1 · 10 §1 |
| K-04 | 2026-08-17 | Birincil farklılaşma ekseni (konumlandırma) | **Kurumsal tanıtım sitesi ile mağazanın tek üründe eşit ağırlıkta buluşması.** Hazır e-ticaret paketleri mağaza odaklıdır ve kurumsal katmanı sonradan eklenmiştir; ajans siteleri ise satış yapmaz. | Ürün adıyla (Shop + folio) ve K-03 segmentiyle tutarlı; gerçek bir boşluğu hedefler. "Sahiplik/maliyet" ve "kasıtlı sadelik" eksenleri **destekleyici** kalır, birincil değildir — ikisi de MVP'de "en iyi çalışması gereken şey"e dönüşmez. | 01 §1, §4, §5, §6 · 10 §1, §2 |
| K-05 | 2026-08-17 | Projenin amacı ve başarı ölçüsü | **Ticari ürün.** Baştan itibaren birden çok KOBİ'ye kurulmak üzere geliştirilir; ilk müşteri henüz belirli değil. | K-02 (yeniden kurulabilir ürün) ile en tutarlı seçenek; ürün kararları tek bir firmanın tercihine esir olmaz. **Bedeli bilinçlidir:** gerçek kullanıcı geri bildirimi olmadan ilerlenir — bu yüzden 01 §8 varsayım listesi uzun olacak ve her varsayımın "nasıl doğrulanacak" alanı doldurulacaktır. | 01 §1, §4, §6, §8 · 10 §1 |
| K-06 | 2026-08-17 | Aktör envanteri ve yönetim tarafının rol derinliği | **Üç aktör:** ziyaretçi · üye müşteri · firma yöneticisi (admin). **Yönetim tarafı tek rol, çoklu kullanıcı** — firma birden fazla yönetici hesabı açabilir, hepsi aynı yetkiye sahiptir. Platform operatörü uygulama içi aktör **değildir**. Misafir alıcı **henüz aktör değildir** — üyeliksiz sipariş kararına bağlıdır. | Tek rol, yetki matrisini MVP'den çıkarır: 04'te {ekran × rol × durum} matrisi tek role iner, 07'de endpoint yetkilendirmesi basitleşir, 12'de senaryolar ikiye katlanmaz. Ayrı hesaplar korunduğu için "kim ne değiştirdi" izlenebilirliği kaybolmaz — paylaşılan tek hesap seçeneğinin asıl bedeli buydu. Platform operatörü K-01 gereği aktör değildir: kurulum bir deploy işidir (Aşama 4 + `DEPLOY_RUNBOOK.md`), uygulamaya operatör paneli koymak kiracılığı arka kapıdan geri getirirdi. | 01 §3 · 02 §1, §10 · 04, 06, 07, 12 (downstream) |
| K-07 | 2026-08-17 | Alıcının hukuki sıfatı — MVP'de kime satılır? | **Yalnız tüketici.** Alıcı, kişisel ihtiyacı için alan gerçek kişidir; 6502 sayılı Kanun'un tüketici korumaları **istisnasız her siparişte** uygulanır. Ticari/kurumsal alıcıya özel hukuki rejim MVP'de **yoktur**. | Tek hukuki rejim: cayma hakkı, ön bilgilendirme formu ve mesafeli satış sözleşmesi dallanmadan uygulanır; 02 §7 ve §12 tek senaryoya göre yazılır, doğrulama senaryoları ikiye katlanmaz. **Bu boşluğu 2026-08-17 tamlık taraması yakaladı:** agendanın tüm yasal omurgası alıcının tüketici olduğunu varsayıyordu ama hiçbir konu bunu sormuyordu. | 02 §1, §7, §12 · 10 §3, §4 · 01 §3 |

---

## 3. Traceability boşlukları (GAP) ve kararları

> `04`, `06`, `07`, `11` matrislerinden çıkan boşluklar ve verilen kararlar.

| # | Kaynak matris | Boşluk | Karar | Nereye yansıdı |
|---|---|---|---|---|

---

## 4. Açık kararlar (detaylandırılacak konular)

> **Kural:** Burada yalnız **detay** açık kalabilir. "Olacak mı olmayacak mı" kararı asla açık bırakılmaz.

| # | Konu | Ne belirsiz | Ne zaman karara bağlanacak | Durum |
|---|---|---|---|---|
| A-01 | Ticari model | K-05 ile "ürün satılacak" **varlık kararı** alındı; ücretin **nasıl** alınacağı (tek seferlik kurulum bedeli / lisans / bakım aboneliği) açık. Yazılım kapsamını etkilemez — K-01 gereği üründe faturalandırma modülü yoktur. | 01 §4 (değer önerisi) ve §6 (başarı kriterleri) yazılmadan önce — Aşama 1 içinde | Açık |

---

## 5. Checkpoint log

| # | Tarih | Aşama | Genel durum | Aksiyon maddeleri | Rapor |
|---|---|---|---|---|---|

---

## 6. Aşama 1 blok planı ve sonraki adımlar

### 6.1 Açık süreç maddeleri

- [x] **Aşama 1 — Product Discovery** oturumu `.claude/checklists/document-stage.md` ile başlatıldı — 2026-08-17
- [x] `Docs/00_PROJECT_METHODOLOGY.md` proje sahibiyle gözden geçirildi — **2026-08-17**, 12 madde, tamamı onaylandı → **B0-01 ✓** (kayıt: `SETUP.md` §7)
- [ ] Aşama 1'in cross-review turu `cursor-agent` ile koşuldu, çıktı `Docs/CROSS_REVIEW_REPORTS/` altına yazıldı → **B9-16**

### 6.2 Blok planı

> **Kaynak:** 2026-08-17 hazırlık taraması. Yedi paralel ajan 137 ham konu üretti; sentez turu bunları bağımlılık sırasına dizdi; tamlık eleştirisi 01/02/10'un **26 şablon bölümünü** tek tek tarayıp **15 kapsama boşluğu** buldu. Boşluktan eklenen konular **★** ile işaretlidir.
>
> **Plan toplam 111 konu taşır** (15'i ★). Bu sayı §6.2 tablosunun toplamı ile §6.3 gövdesindeki konu kimliklerinin sayısına eşittir — ikisi ayrıldığında tablo değil gövde esastır.
>
> **Blok 7 neden ayrı:** Tamlık eleştirisi, K-04'ün "kurumsal tanıtım ile mağaza eşit ağırlıkta" kararıyla agendanın orantısız olduğunu tespit etti — folio tarafı agendanın yalnız 4 konusuna düşüyor ve yasal uyum bloğunun içinde eriyordu. Kurumsal içerik kendi bloğuna ayrıldı; yasal metinler Blok 8'de kaldı.
>
> **Bölünme kuralı:** 12'den fazla konu taşıyan blok iki oturuma bölünür.

| # | Blok | Hedef doküman | Konu | Durum |
|---|---|---|---|---|
| 0 | Oturum çerçevesi ve plan disiplini (süreç — ürün kararı içermez) | SETUP §7 · bu dosya | 9 | ⏳ |
| 1 | Ürün kimliği, ticari rol ve pazar çerçevesi | 01 §1–§2 · 02 §1, §3, §12 · 10 §1, §4 | 10 | ⏳ |
| 2 | Değer önerisi, konumlandırma ve MVP çerçevesi | 01 §4–§5 · 10 §1–§4 | 9 | ⬚ |
| 3 | Katalog, stok ve fiyatlandırma | 02 §3, §5, §11 · 10 §2–§4 | 12 | ⬚ |
| 4 | Üyelik, kimlik doğrulama ve hesap yaşam döngüsü | 02 §3–§5, §8, §12 · 10 §2–§3 | 10 | ⬚ |
| 5 | Sepet, kargo ve teslimat | 02 §3, §4, §6, §11 | 7 | ⬚ |
| 6 | Ödeme, sipariş durum makinesi, iptal ve iade | 02 §2, §3, §5–§7, §12 · 10 §2–§4 | 15 | ⬚ |
| 7 | **Kurumsal içerik ve marka kimliği (folio tarafı)** | 02 §3, §5, §10 · 10 §2–§3 | 5 | ⬚ |
| 8 | Yönetim, bildirim, kötüye kullanım ve yasal uyum | 02 §4, §8–§10, §12 | 16 | ⬚ |
| 9 | MVP kapsam kapanışı, parametreler ve aşama kapanışı | 10 §1–§5 · 01 §6–§8 · 02 §11, §13 | 18 | ⬚ |

### 6.3 Blok içerikleri

**Blok 0 — Oturum çerçevesi ve plan disiplini**
`B0-01` Metodolojinin proje sahibiyle gözden geçirilmesi (SETUP §7) · `B0-02` Doküman üretim sırası: katı 01→02→10 mı, çekirdek-önce döngü mü · `B0-03` Workshop ile doküman yazımının ayrılması · `B0-04` Oturum sayısı ve blok mimarisinin onayı · `B0-05` İnkremental karar kaydının işletimi ve blok sonu `docs:` PR'ı · `B0-06` Karar satırı biçimi ve hedef bölüm eşlemesi · `B0-07` Aşama dışı konular için park mekanizması · `B0-08` Varlık kararı / detay ayrımının işletilmesi · `B0-09` Oturum kapanış ve devir protokolü

**Blok 1 — Ürün kimliği, ticari rol ve pazar çerçevesi**
`B1-01` Firma kimlik kaydı, kurulum sınırı ve çok kiracılığın nereye yazılacağı · `B1-02` Satış modeli: doğrudan ödemeli satış mı, teklif/talep mi · `B1-03` Ticari rol ve para akışı: satıcı kim, tahsilat kimde, kart verisi ve 3D Secure ilkesi · `B1-04` ETBİS kaydı ve sitede sürekli gösterilecek firma kimlik bilgileri · `B1-05` Birincil problem ve bugün nasıl çözüldüğü (01 §1'in somut cümlesi) · `B1-06` Gelir modeli — **A-01 ile açık** · `B1-07` Pazar, coğrafya, dil ve para birimi sınırı · `B1-08` Terim sözlüğü kuralı ve adlandırma konvansiyonu · `B1-09` Aktör envanteri (**✓ K-06**; `01` §3 aktör tablosuna "geri dönme nedeni" sütunu eklenecek) · ★`B1-10` Alıcının hukuki sıfatı (**✓ K-07**)

**Blok 2 — Değer önerisi, konumlandırma ve MVP çerçevesi**
`B2-01` Her aktör için "bu ürün olmasaydı ne olurdu" · `B2-02` Marka görünürlüğü: alan adı ve platform imzası · `B2-03` Alternatif envanteri ve farklılaşma ekseni (**✓ K-04**; alternatif envanteri yazılacak) · `B2-04` MVP başarı tanımı: en dardan en genişe üç seçenek · `B2-05` MVP "bitti" kabul çizgisi: test ortamı mı, canlıda gerçek parayla gerçek sipariş mi · `B2-06` 10 §2/§3 satırlarının doğrulanabilir yazım biçimi · `B2-07` 10 §3 "post-MVP'de düşünülecek mi" sütununun sabit cevap sözlüğü · `B2-08` 10 §4 "ne zaman kalkar" sütununun ölçülebilir tetikleyicisi · ★`B2-09` Ana sayfa kompozisyonu: ziyaretçi hangi kapıdan girer, tanıtım ile vitrin nerede bağlanır

**Blok 3 — Katalog, stok ve fiyatlandırma**
`B3-01` Satılabilir birim: ürün mü varyant mı · `B3-02` Kategori yapısı ve derinliği · `B3-03` Arama, filtreleme, sıralama derinliği · `B3-04` Stok takibi ve tükenme davranışı · `B3-05` Ürün yayın durumu (taslak / yayında / arşiv) · `B3-06` Para birimi, KDV gösterim rejimi ve yuvarlamanın tek noktası · `B3-07` İndirim/kampanya varlık kararı ve indirim beyanının referans fiyatı · `B3-08` Kupon kodu varlık kararı · `B3-09` Sipariş anında fiyat dondurma (snapshot) · `B3-10` Özel izin/mevzuat gerektiren ürünler ve hizmet/dijital ürün sınırı · `B3-11` Katalog ölçeği ve asgari yayın alan seti · ★`B3-12` Ürün ve içerik medyası: görsel adedi, ana görsel, sıralama, boyut/format, alternatif metin, ad/açıklama uzunluk sınırları

**Blok 4 — Üyelik, kimlik doğrulama ve hesap yaşam döngüsü**
`B4-01` Misafir alışverişi var mı, misafir siparişinin hesaba devri · `B4-02` E-posta doğrulama zorunluluğu ve link ömrü · `B4-03` Sosyal giriş kapsamı ve aynı e-posta çakışmasında hesap bağlama · `B4-04` Şifre sıfırlama linkinin ömrü, tek kullanımlığı, sonrasında oturumların kapanması · `B4-05` Oturum ömrü, "beni hatırla" ve hassas işlemde yeniden doğrulama · `B4-06` Adres defteri, fatura/teslimat adresi ayrımı · `B4-07` Yaş sınırı ve ehliyet beyanı · `B4-08` Hesap silme, pasifleştirme, anonimleştirme · `B4-09` İlgili kişi hakları: başvuru kanalı, 30 günlük cevap, veri indirme · ★`B4-10` Şifre politikası: asgari güç, şifre değiştirme akışı, sosyal girişle açılmış hesabın şifre durumu

**Blok 5 — Sepet, kargo ve teslimat**
`B5-01` Misafir sepeti, sepet ömrü, giriş sonrası birleştirme · `B5-02` Stok rezervasyonu varlık kararı: an, süre, süre dolunca davranış · `B5-03` Ödeme öncesi sepet yeniden değerlendirme ve değişiklik onayı · `B5-04` Kargo ücreti hesaplama modeli ve teslimat bölgesi kısıtı · `B5-05` Ücretsiz kargo eşiği ve karşılaştırma tabanı · `B5-06` Teslimat süresi taahhüdü, 30 günlük üst sınır, takip numarası · ★`B5-07` Sepet sınırları: azami kalem sayısı, kalem başına azami adet, adedin stokla ilişkisi

**Blok 6 — Ödeme, sipariş durum makinesi, iptal ve iade**
`B6-01` Ödeme yöntemi envanteri (kart, havale/EFT, kapıda ödeme) · `B6-02` Havale/EFT bekleme süresi, hatırlatma ve yönetici onayı · `B6-03` Sipariş durum makinesi: eksen sayısı, durum isimleri, geçiş koşulları · `B6-04` Sipariş bütünlüğü: kısmi sevkiyat, kısmi iptal, kalem granülaritesi · `B6-05` Ödeme durum geçişleri ve yarıda kalan ödemede siparişin kaderi · `B6-06` Sipariş numarası formatı ve takip erişimi · `B6-07` Ön Bilgilendirme Formu + Mesafeli Satış Sözleşmesi: onay adımı ve sürüm kaydı · `B6-08` İptal yolları: kullanıcı, satıcı kaynaklı, caymadan ayrılması · `B6-09` Cayma hakkı penceresi, başlangıç anı, ürün bazlı istisna · `B6-10` İade ve geri ödeme yürütümü: 14 günlük çatı, iade kargo bedeli · `B6-11` Fatura: e-Arşiv/e-Fatura karşısında MVP kapsam kararı · `B6-12` Temel akış omurgası: kaç akış, hangi adımlarla · ★`B6-13` Yasak geçişler, terminal durumlar ve geri alınamaz geçişlerin işaretlenmesi · ★`B6-14` Ayıplı mal ve garanti talebi: caymadan ayrılması, kanal, otomasyon derecesi · ★`B6-15` **Hata ve istisna senaryosu envanteri:** dış servis erişilemediğinde, bildirim gönderilemediğinde, mükerrer işlem oluştuğunda, sepet–stok–fiyat tutarsızlaştığında ürünün cevabı; "satış kapalı / bakım modu" varlık kararı

**Blok 7 — Kurumsal içerik ve marka kimliği (folio tarafı)**
★`B7-01` Kurumsal içerik tipleri envanteri: hakkımızda, hizmetler, referans/portfolyo, SSS, galeri, şube — her tip için alan seti, yayın durumu, zorunluluk · ★`B7-02` Marka ve görsel kimlik ayarları: logo, renk, tipografi — neresi ayar, neresi kod (K-02'nin folio tarafındaki doğrudan sonucu) · `B7-03` İçerik yönetimi rejimi: panelden düzenlenir mi, blog MVP'de var mı · `B7-04` İçerik yayın durumu ve sürümleme · `B7-05` Arama motoru görünürlüğü tabanı ve kalıcı adres/yönlendirme kuralı

**Blok 8 — Yönetim, bildirim, kötüye kullanım ve yasal uyum**
`B8-01` İletişim/talep formu, spam koruması ve talep durumları · `B8-02` Yönetim yetki modeli: çoklu hesap, hesap açma yolu, işlem izi (**kısmen ✓ K-06**) · `B8-03` Bildirim matrisi: hangi durum geçişinde kime · `B8-04` Ticari elektronik ileti ve İYS: pazarlama iletisi MVP'de var mı · `B8-05` Kötüye kullanım envanteri: deneme limitleri, form limiti, açık checkout limiti · `B8-06` Saat dilimi, iş günü tanımı, süre parametrelerinin alt/üst çitleri · `B8-07` KVKK aydınlatma metni: veri sorumlusu kimliği ve gösterim noktaları · `B8-08` Açık rıza mimarisi: aydınlatma ile rızanın ayrılması · `B8-09` Çerez politikası ve onay bandı davranışı · `B8-10` Veri saklama süreleri ve periyodik imha (ticari kayıt ↔ kişisel veri çatışması) · `B8-11` Kişisel verinin yurt dışına aktarımı · `B8-12` Yasal metinlerin sürümlenmesi ve değişiklikte yeniden onay · `B8-13` Uyuşmazlık çözümü ve şikâyet kanalının sistemdeki yeri · ★`B8-14` Yöneticinin sipariş üzerindeki müdahale yetkileri: adres/kalem düzeltme, elle sipariş, iç not, elle durum geçişi, her müdahalenin işlem izine yazılması · ★`B8-15` E-posta kanalının ürün kararları: gönderen kimliği, işlem bildiriminin kapatılamazlığı, e-posta kurulumunun 10 §4'e dış ön koşul olarak yazılması · ★`B8-16` Veri ihlali bildirimi: ürünün hangi kaydı tutması gerektiği

**Blok 9 — MVP kapsam kapanışı, parametreler ve aşama kapanışı**
`B9-01` MVP kapsam listesi (10 §2) ve gerekçeli kapsam dışı listesi (10 §3) · `B9-02` Kapsam dışı aday paket: ürün yorumu, istek listesi, ürün önerisi, canlı destek · `B9-03` Mobil uygulama kararı ve mobil uyumluluk tabanı · `B9-04` Cihaz, tarayıcı ve erişilebilirlik tabanı · `B9-05` Analitik ve ölçüm kapsamı · `B9-06` Manuel operasyon adımları bütçesi ve günlük sipariş dayanağı · `B9-07` Hizmet seviyesi, hacim ve süreklilik kabulleri · `B9-08` Sayısal parametre envanteri ve runtime ayarlanabilirlik rejimi · `B9-09` **Ürün düzeyi** başarı kriterleri: kurulabilirlik ve firmanın kendi kendine yetmesi · `B9-10` **Mağaza düzeyi** başarı kriterleri: satış hunisi + folio tarafının ölçüsü · `B9-11` 01 §7 sınırlar — Shopfolio'nun ne OLMADIĞI · `B9-12` 01 §8 ürün varsayımları, yanlış çıkarsa sonucu, doğrulama yöntemi · `B9-13` Post-MVP yol haritası ve sıralama kriteri · ★`B9-14` Toplu veri işlemleri: ürün içe aktarma, toplu fiyat/stok güncelleme, sipariş dışa aktarma · ★`B9-15` 02 §13 açık kararlar tablosunun derlenmesi ve bu dosyanın §4'üyle çakışma taraması · `B9-16` Kalite döngüsünün işletim birimi ve cross-review'ın `cursor-agent` ile koşumu · `B9-17` Aşama kapanışı: checkpoint, öğrenim terfisi (00 §K), Aşama 2'ye devir · `B9-18` 01 §6'nın iki eksenli tablo olarak finalize edilmesi

> **Kurulum notu (2026-08-11).** Bu dosya kurulumda açıldı; §1 tablosu hazır, §2–§5 bilinçli olarak **boş**. Kurulum kararları (proje adı, dal koruma rejimi, ikinci AI, §2/§4 ertelemesi) buraya kopyalanmadı — evleri `SETUP.md` parametre tablosu, `CI_CD_SETUP.md §3.3` ve `DEFERRED_BACKLOG.md` D-01'dir. Bu dosyanın §2'si **ürün** kararlarının kaydıdır ve ilk satırını Aşama 1'de alır.
