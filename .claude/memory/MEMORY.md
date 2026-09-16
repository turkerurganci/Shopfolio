# Proje Hafızası

> **Bu dosya indeks + güncel durum snapshot'ıdır.** İçerik gömülmez — her hafıza ayrı bir dosyadır.
> Ne buraya girer / ne L1–L5'e terfi eder: [`README.md`](README.md).
> **Otoriter kaynak değildir** — task durumu için [`../../Docs/IMPLEMENTATION_STATUS.md`](../../Docs/IMPLEMENTATION_STATUS.md).

---

## Proje Özeti

- **Ad:** Shopfolio
- **Tanım:** Herhangi bir firmanın kendi ürünlerini sergileyip çevrimiçi satabileceği ve kurumsal tanıtımını yapabileceği; üyelik (sipariş için zorunlu değil), Google ile giriş, sipariş, ödeme ve sipariş takibi içeren web uygulaması.
- **Dönem:** Doküman üretimi
- **Doküman dili:** Türkçe · **Kod dili:** İngilizce

---

## Güncel Durum

> Kısa tut. Tarihsel detay `MEMORY_ARCHIVE.md`'ye taşınır (00 §G.2).

- **Son tamamlanan:** **Blok 4 ✓ · oturum 1 — blok bitti.** On konu, tek oturum, **yirmi altı karar** (K-97…K-122; 2026-09-15/16, oturum gece yarısını geçti). Omurga: **misafir alışverişi var** ve misafir alıcı **dördüncü aktör** (K-97) · doğrulanmış e-postayla geçmiş misafir siparişleri hesaba düşer (K-98), e-posta değişiminde de (K-110), silinen hesabınkiler **düşmez** (K-117) · e-posta doğrulaması **sert kapı** (K-101), doğrulanmamış hesap geçicidir (K-102) · sosyal giriş **yalnız Google** — kurucu tanım ilk kez daraltıldı, Facebook `10 §3`'e (K-103) · aynı e-posta iki yoldan gelirse otomatik tek hesap (K-104) · şifre sıfırlamada tüm oturumlar düşer (K-106) · "beni hatırla" var (K-108), yeniden doğrulama **dar set**: şifre, e-posta, hesap silme (K-109) · adres defteri + ayrı fatura adresi (K-111, K-112), sipariş adresini **dondurur** (K-113) · hesap silinir, sipariş kaydı yasal süre boyunca kalır (K-115), yürüyen sipariş silmeyi engellemez (K-116) · KVKK başvurusu `B8-01` formuna bir tip, indirme düğmesi yok (K-118, K-119) · şifre: uzunluk tabanı + yaygın şifre listesi (K-120) · **yönetici hesapları aynı kimlik doğrulama rejiminde**, `B8-02` yalnız sıkılaştırır (K-122). **Tamlık taraması altı boşluk buldu** (K-99 · K-107 · K-110 · K-113 · K-117 · K-122), yeni konu kimliği açılmadı, plan **111 konu**. **Yirmi beş sorunun tamamında öneri kabul edildi** (Blok 3'te desen tersiydi) — gözlem, sebep kayıtta yok. Ayrıntı: tracker §6.2.
- **Taslak güncellemeleri (K-29) aynı PR'da:** `01` → v0.3 (§2, §3.2) · `02` → v0.3 (sözlük 25 → **31 terim**, §1.3 dört aktör) · `.claude/CONTEXT.md §1` tanım cümlesi düzeltildi (K-103); `README.md` bilerek dokunulmadı. `(taslak güncellenecek)` işareti dört satırda **`(taslak güncellendi — v0.3)`** yapıldı — K-29 kapanmış biçimi tanımlamıyordu, bu ilk uygulamasıdır. §6.2'nin "Hedef doküman" sütunu gerçek etki birleşimiyle hizalandı (Blok 4: `01 §2–§3` · `02 §10` · `10 §4` eklendi).
- **`02 §1` Blok 8'den önce kapanmaz** — sözlük bloklar arası bir bölümdür; Blok 5'ten **Sepet**, Blok 6'dan **Sipariş kalemi** ve sipariş durum adları bekleniyor.
- **Sırada:** **Blok 5** — sepet, kargo ve teslimat; yedi konu (`B5-01`…★`B5-07`), tahmin bir oturum. `B5-01` (misafir sepeti ve giriş sonrası birleştirme) **K-97'nin doğrudan devamıdır**; K-99'un "giriş duvarı yok" hattıyla hizalı cevaplanmalı.
- **Açık devirler (Blok 3 + Blok 4):** `B5-01` (misafir sepeti — K-97) · `B5-02` (kupon sayacı **ve** hizmet kontenjanı, stok rezervasyonuyla aynı rejim) · `B5-03` (sepet canlı, kalem sipariş anında donar; **silinen ürünün açık sepetteki kalemi**) · `B5-04`, `B5-06` (fiziksel-olmayan kalemde kargo ve dijital teslim) · `B6-03` (dijital/hizmet siparişinin durum makinesi) · `B6-04` (kısmi iptal/iade kalem düzeyinde) · `B6-05` (yarıda kalan ödeme) · `B6-06` (sipariş no + e-posta ile takip — K-97) · `B6-07` (onay adımı — K-114'ün yaş maddesi buna yaslanır) · `B6-08`/`B6-10` (K-99'un kabul edilen riski: yanlış e-postayla bağlanan siparişte yabancının iptal/iade yetkisi) · `B6-09` (**tipe göre cayma istisnası** — K-81; pencere teslimattan sonra başlar — K-116) · `B6-11` (fatura) · `B6-15` · `B7-03` (K-96 zengin metin seti) · `B7-05` (404 ve yönlendirme) · **`B8-01` bağlayıcı** ("KVKK talebi" tipi — K-118) · **`B8-02` bağlayıcı** (kimlik doğrulama tabanı, yalnız sıkılaştırır — K-122; silme işleminin işlem izi) · `B8-03` · `B8-05` (deneme limitleri — K-107'nin kalan riski; yeniden gönderme ve sıfırlama sıklığı) · `B8-06` · `B8-07`/`B8-08` (üyeliksiz veri işleme — K-97) · `B8-10` (saklama süreleri — K-115/K-116/K-117) · **`B8-12` bağlayıcı** (yaş maddesi metinlere girer — K-114) · `B8-14` · `B9-04` (erişilebilirlik — K-93) · `B9-08` (KDV listesi · K-40 boyut sınırı · görsel format/boyut/adet · ad tavanı · **doğrulama ve sıfırlama linki ömrü · iki oturum süresi · asgari şifre uzunluğu**) · `B9-14` (toplu içe aktarma — K-88 SKU) · Blok 8 yasal uyum turu. **`10 §4` dış ön koşul:** kurulum başına Google OAuth uygulaması (K-103). **`04`'e:** görselsiz kart (K-86) · "yok"/"Tükendi" (K-87) · SKU ön doldurma (K-88) · ana görsel geri düşüşü (K-91) · ödeme adımında "bu e-posta kayıtlı" hatırlatması (K-99) · adres yönetim ekranı (K-111) · "fatura adresim farklı" (K-112). **`05`'e:** doğrulanmamış hesap temizlik işi (K-102). **`06`'ya:** sipariş başlığı donmuş adresleri **ve iletişim e-postasını** taşır (K-113, K-116).
- **Açık kararlar:** **A-02, A-03, A-04** — `01` yazım oturumunun bulduğu boşluklar; **A-03'ün kapsamı dört satıra çıktı** (misafir alıcı hücresi, K-100). **A-05** — sözlükte **20 türetilmiş** İngilizce karşılık (31 terimin 11'i kayıtta); vadesi `02`'nin son yazım oturumu, `03`/`04`'ten önce. Dördü de **detay**. Ayrıntı: tracker §4.
- **Gate durumu:** Implementation başlamadı, faz yok. Doküman: **01 ⏳ (v0.3)** · **02 ⏳ (v0.3)** · 10 ⏳ · 03–09, 11–12 ⬚. Blok planı: 0 ✓ · 1 ✓ · 2 ✓ · 3 ✓ · **4 ✓** · 5–9 ⬚.
- **Oturum kapanışı (K-38):** Doküman döneminde bu blok `/handoff` beklenmeden **oturum sonu `docs:` PR'ının içinde** güncellenir. İki kez kaçırıldı (Blok 3 oturum 1 ve 4), ikisini de INSTRUCTIONS §3.0 yakaladı. **Karşı ilaç dört oturumdur işliyor:** kararlar konu bittikçe kayda yazılır — bu oturumda yirmi parti.
- **Son güncelleme:** 2026-09-16

---

## Task Changelog (son N task)

| Task | Durum | Özet | Commit | PR |
|---|---|---|---|---|
| | | | | |

> Bu tablo şişmeye başladığında (yaklaşık 20 satırı geçince) eski satırlar `MEMORY_ARCHIVE.md`'ye taşınır.

---

## Proje

- `<project_*.md dosyaları buraya tek satır olarak listelenir>`

## Kullanıcı

- `<user_*.md>`

## Çalışma Tercihleri (feedback)

- `<feedback_*.md>`

## Referanslar

- `<reference_*.md>`

---

## Terfi Edenler

> Bir zamanlar hafızada yaşayıp artık L1–L5'te olan kurallar. Buraya yalnız işaretçi yazılır.

| Kural | Terfi ettiği yer | Tarih |
|---|---|---|
| Bir konunun alt parçaları da tek tek sorulur | `INSTRUCTIONS.md` §2 | 2026-08-18 |
| Seçenekler sade dille, somut sonuç üzerinden yazılır | `INSTRUCTIONS.md` §2 | 2026-08-18 |
| Sorular ve seçenekler numaralandırılır | `INSTRUCTIONS.md` §2 | 2026-08-30 |
| Konu kapanışında tamlık taraması | `INSTRUCTIONS.md` §2 | 2026-09-15 |
