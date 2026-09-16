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

- **Son tamamlanan:** **Blok 5 ⏳ · oturum 1** — beş konu kapandı, `B5-06` yarıda kaldı; **yirmi üç karar** (K-123…K-145, 2026-09-16). Omurga: **üyenin sepeti hesapta, misafirinki tarayıcıda** (K-123) · girişte sepetler birleşir, **aynı varyant toplanmaz** (K-124), görülmemiş ekleme bir kez söylenir (K-125) · sepet kendiliğinden boşalmaz (K-126), **ödeme başarılı olunca** boşalır (K-127) · **stok ayırma sipariş onayında başlar** ve ödeme süresiyle sınırlıdır; hizmet kontenjanı ve kupon aynı rejimde (K-128) · onaylanan özet ile oluşan sipariş **birebir aynı**, her fark siparişi durdurur (K-129) · satın alınamaz kalem vitrinin kuralını izler: tükenen işaretli kalır, vitrinden kalkan sepetten çıkar (K-130) · sepette fiyat değişikliği **rakamsız** işaretlenir (K-131) · **sipariş başına sabit kargo ücreti** (K-132), il düzeyinde teslimat kısıtı (K-133), **tek teslimat yolu kargo** — mağazadan teslim `10 §3`'e (K-134) · ücretsiz kargo eşiği var (K-135), taban kupondan önceki tutar ve tüm kalemler (K-136, K-137), eşiğe kalan tutar gösterilir (K-138) · teslimat sözü **kargoya verme süresidir**, panel üst çit koyar (K-139), firma varsayılanı + ürün düzlemi (K-140) · takip numarası zorunlu, "kendi aracımızla teslim" istisnası (K-141), kargo şirketi ürünle gelen listeden, "Takip et" bağlantısı (K-142) · **dijital ürünü sistem teslim eder** (K-143), dosya ürün düzleminde, varyant ezebilir, tek dosya (K-144), dosyasız dijital ürün yayına alınamaz (K-145). **Tamlık taraması üç boşluk buldu** (K-131 · K-134 · K-138), plan **111 konu**. Cevaplanan yirmi dört sorunun ikisinde öneri reddedildi (ikisi de K-131). Oturum PR'a varmadan kesildi; kapanış bir sonraki oturumun açılışında geriye dönük koşturuldu. Öncesinde **Blok 4 ✓** (K-97…K-122, PR #20). Ayrıntı: tracker §6.2.
- **Taslak güncellemeleri (K-29):** Blok 4'ünkiler PR #20'de yapıldı (`01` v0.3, `02` v0.3, sözlük **31 terim**). **Blok 5'in üç sözlük satırı bekliyor:** Sepet → `Cart` (K-123) · Stok ayırma → `StockReservation` (K-128) · Sepet kalemi → `CartItem` (K-131) — satırlar `(taslak güncellenecek)` işaretli, **bloğun son PR'ında** yazılır. Aynı PR §6.2'nin "Hedef doküman" sütununu hizalar: Blok 5 satırları şimdiden `02 §5`, `§9`, `§12` ve `10 §2–§3`'e dokunuyor.
- **`02 §1` Blok 8'den önce kapanmaz** — sözlük bloklar arası bir bölümdür; Blok 5'in üç terimi kayıtta, blok kapanışında yazılır; Blok 6'dan **Sipariş kalemi** ve sipariş durum adları bekleniyor.
- **Sırada:** **Blok 5 · oturum 2** — `B5-06`'nın kalanı, sonra ★`B5-07`. `B5-06`'nın son sorusu (Soru 25: dijital ürün dosyası kaç kez / ne kadar süre indirilebilir) oturum 1'de cevapsız kaldı; **cevap oturum 2'nin ilk mesajında geldi**, o oturumun kaydına girer. Ardından `B5-06` kapanış taraması ve `B5-07` (sepet sınırları: azami kalem, kalem başına azami adet, adedin stokla ilişkisi — K-124'ün birleşen sepeti ve K-130'un "stok sıfır değil ama sepetteki adetten az" devri buraya bağlanır). Blok bitince: `02 §1` taslak güncellemesi + Hedef doküman sütunu hizalaması.
- **Açık devirler (Blok 3 + 4 + 5):** `B6-02` (havale/EFT beklemesinde stok ayırma — K-128) · `B6-03` (dijital/hizmet siparişinin durum makinesi; dijitalde teslim kendiliğinden — K-143; "kargoya verildi" geçişi — K-141) · `B6-04` (kısmi iptal/iade kalem düzeyinde; **parçalı gönderim** ve birden fazla takip numarası — K-140, K-141) · `B6-05` (yarıda kalan ödeme: ödenmemiş siparişin kaderi, tekrar denemenin biçimi, süre dolduktan sonra gelen ödeme — K-127, K-128) · `B6-06` (sipariş no + e-posta ile takip — K-97) · `B6-07` (onay adımı — K-114'ün yaş maddesi; onaylanan form K-129'un özetidir; teslimat kısıtı ve kargoya verme süresi ön bilgilendirmede — K-133, K-139) · `B6-08`/`B6-10` (K-99'un kabul edilen riski; **iptal ve iadede stok, hizmet kontenjanı ve kupon adedinin geri dönüşü birlikte** — K-128; gecikmede fesih — K-139; kısmi iadede eşiğin altına düşen siparişin kargo ücreti — K-137) · `B6-09` (**tipe göre cayma istisnası** — K-81; pencere teslimattan sonra başlar — K-116; dijitalde istisnanın anı ödeme başarısı — K-143) · `B6-11` (fatura; kargo ücretinin KDV'si — K-132) · `B6-15` (mükerrer işlem — K-127; satış kapalıyken sepet — K-130) · `B7-03` (K-96 zengin metin seti) · `B7-05` (404 ve yönlendirme) · **`B8-01` bağlayıcı** ("KVKK talebi" tipi — K-118) · **`B8-02` bağlayıcı** (kimlik doğrulama tabanı, yalnız sıkılaştırır — K-122; silme işleminin işlem izi; yönetici hesabının vitrinde sepet ve sipariş sahibi olması — K-123) · `B8-03` (takip bilgisi, teslim e-postası, gecikme bildirimi — K-139, K-141, K-143) · `B8-05` (deneme limitleri — K-107; onaylayıp ödemeyerek stok kilitleme — K-128) · `B8-06` (iş günü tanımı, kargoya verme çitinin sınırları — K-139) · `B8-07`/`B8-08` (üyeliksiz veri işleme — K-97) · `B8-10` (saklama süreleri — K-115/K-116/K-117; üye sepeti — K-126) · **`B8-12` bağlayıcı** (yaş maddesi — K-114; yasal 30 günlük teslim üst sınırı sözleşme metninde — K-139) · `B8-14` (gecikmenin panelde yansıması, yanlış takip numarasının düzeltilmesi — K-139, K-141) · `B9-04` (erişilebilirlik — K-93) · `B9-08` (KDV listesi · K-40 boyut sınırı · görsel format/boyut/adet · ad tavanı · doğrulama ve sıfırlama linki ömrü · iki oturum süresi · asgari şifre uzunluğu · **ödeme süresi — K-128 · kargoya verme süresi üst çiti — K-139 · dijital dosya boyutu tavanı — K-143**; kargo ücreti ve eşik ayarları — K-132, K-135) · `B9-14` (toplu içe aktarma — K-88 SKU) · Blok 8 yasal uyum turu (rakamsız fiyat işareti — K-131 · kargo ücretinin KDV okuması — K-132 · ücretsiz kargonun indirim kuralı dışında okunması — K-135). **`10 §4` dış ön koşul:** kurulum başına Google OAuth uygulaması (K-103). **`04`'e:** görselsiz kart (K-86) · "yok"/"Tükendi" (K-87) · SKU ön doldurma (K-88) · ana görsel geri düşüşü (K-91) · ödeme adımında "bu e-posta kayıtlı" hatırlatması (K-99) · adres yönetim ekranı (K-111) · "fatura adresim farklı" (K-112) · birleştirme mesajı (K-125) · panelde ayrılmış adet (K-128) · "ne değişti" özeti (K-129) · sepette "Tükendi" işareti ve çıkarılma mesajı (K-130) · "fiyatı değişti" işareti (K-131) · kargo ücreti, il seçimi, eşik ayarları ve kalan tutar mesajı (K-132, K-133, K-135, K-138) · kargoya verme süresi gösterimi ve ürün formundaki alan (K-139, K-140) · kargoya verme formu ve "Takip et" (K-141, K-142) · indirme düğmesi, dosya alanı, dosyası eksik varyant uyarısı (K-143…K-145). **`05`'e:** doğrulanmamış hesap temizlik işi (K-102) · kabul edilen dosya türleri ve yüklenen dosyanın güvenliği (K-143). **`06`'ya:** sipariş başlığı donmuş adresleri **ve iletişim e-postasını** taşır (K-113, K-116) · sepet ve sepet kalemi, fiyat referansıyla (K-123, K-131) · stok ayırma (K-128) · adreste il kapalı listeden (K-133) · dijital dosya ürün/varyant düzleminde (K-144). **`08`'e:** kargo şirketi listesi ve takip bağlantısı kalıpları (K-142); kargo şirketi entegrasyonu yok (K-132).
- **Açık kararlar:** **A-02, A-03, A-04** — `01` yazım oturumunun bulduğu boşluklar; **A-03'ün kapsamı dört satıra çıktı** (misafir alıcı hücresi, K-100). **A-05** — sözlükte **20 türetilmiş** İngilizce karşılık (31 terimin 11'i kayıtta); vadesi `02`'nin son yazım oturumu, `03`/`04`'ten önce. Dördü de **detay**. Ayrıntı: tracker §4.
- **Gate durumu:** Implementation başlamadı, faz yok. Doküman: **01 ⏳ (v0.3)** · **02 ⏳ (v0.3)** · 10 ⏳ · 03–09, 11–12 ⬚. Blok planı: 0 ✓ · 1 ✓ · 2 ✓ · 3 ✓ · 4 ✓ · **5 ⏳** · 6–9 ⬚.
- **Oturum kapanışı (K-38):** Doküman döneminde bu blok `/handoff` beklenmeden **oturum sonu `docs:` PR'ının içinde** güncellenir. **Üç kez kaçırıldı** (Blok 3 oturum 1 ve 4, Blok 5 oturum 1), üçünü de INSTRUCTIONS §3.0 yakaladı. Kararlar konu bittikçe kayda yazıldığı için Blok 5 oturum 1'de de kayıp olmadı. **Bu kez kesinti bir soru açıkken oldu:** proje sahibi cevabı yeni oturumun ilk mesajında verdi; açılış kapısı cevabın kaydından önce koştu, önceki oturumun son sorusu transcript'ten bulundu.
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
