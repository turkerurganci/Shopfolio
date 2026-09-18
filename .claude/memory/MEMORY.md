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

- **Son tamamlanan:** **Blok 6 ✓ — tek oturum, yetmiş sekiz karar (K-159…K-236), 2026-09-18.** Tahmin iki oturumdu. Omurga: **iki ödeme yöntemi** — kart ve havale/EFT (K-159); kapıda ödeme yok (K-160); taksiti ürün bilmez, sağlayıcı ekranında kalır (K-164) · havalede stok ayrılı kalır, süre firma ayarı + üst çit (K-165, K-166), kargoya verme süresi "ödendi" işaretinden başlar (K-168) · **iki eksenli durum makinesi** (K-170): sevkiyat — Alındı · Hazırlanıyor · Kargoya verildi · Teslim edildi · Teslim edilemedi · İptal edildi; ödeme — Bekliyor · Ödendi · Başarısız · Kısmen geri ödendi · Geri ödendi · parçalı gönderim yok (K-177), **iptal ve iade kalem düzeyinde** (K-178) · ödeme süresi dolunca otomatik iptal, tekrar deneme yeni sipariş (K-180, K-181) · sipariş numarası tahmin edilemez (K-185) · **iki ayrı onay kutusu** (K-188), sözleşme sürümü ve firma kimliği siparişe donar (K-189, K-190) · müşteri "Kargoya verildi"ye kadar kendi iptal eder (K-196) · dijitalde cayma yok — üçüncü onay kutusuyla (K-204), fiziksel üründe istisna işareti (K-206) · **iade kargosu firmaya ait** (K-210), iadede stok elle döner (K-215) · **ürün fatura kesmez**, veri dışa aktarılır (K-217) · satışı geçici kapatma anahtarı (K-235). Ayrıntı: tracker §6.2.
- **Yöntem değişti — konu başına tek mesaj (2026-09-18, `INSTRUCTIONS §2`):** konunun bütün kararları numaralı öneri listesi olarak tek mesajda sunulur, sorulmaya devam eden üç grup (varlık · para/yasal hak · önceki karardan ayrılma) ⚠ ile işaretlenir, proje sahibi yalnız itiraz eder. Sebep: proje sahibi Blok 6'nın ilk dört konusunda iki kez gereksiz soru sorulduğunu söyledi; on dört sorunun üçü öneriyle kayıt kapsamındaydı. Yeni biçimle kalan on bir konu dokuz mesajda kapandı, itiraz sıfır. Yetmiş sekiz kararın elli üçü `(öneriyle kaydedildi)` taşır — sorulmamış ya da ⚠ ile gösterilmemiş karar.
- **Taslak güncellemeleri (K-29) yapıldı:** `01` → **v0.4** (§2: "kartla ya da havale/EFT ile öder") · `02` → **v0.5**, sözlük 34 → **60** terim; on üçü † aldı, **A-05 20 → 33**. **"İade" ayrıldı:** İade → `Return` (malın dönüşü), Geri ödeme → `Refund` (paranın dönüşü); ödeme durumları buna hizalandı. **"Sipariş" terimi sözlükte hiç yoktu**, eklendi.
- **`02 §1` Blok 8'den önce kapanmaz** — Blok 7'nin folio terimleri ve Blok 8'in işlem izi terimleri bekleniyor.
- **Sırada:** **Blok 7** — kurumsal içerik ve marka kimliği (folio tarafı); **beş konu** (★`B7-01`…`B7-05`), tahmin bir oturum. Yeni biçimle açılır: konu başına tek mesaj.
- **Açık devirler (Blok 3–6):** `B7-03` (K-96 zengin metin seti) · `B7-05` (404 ve yönlendirme) · **`B8-01` bağlayıcı** ("KVKK talebi" tipi — K-118; indirme hakkı yenileme talebinin kanalı — K-147) · **`B8-02` bağlayıcı** (kimlik doğrulama tabanı, yalnız sıkılaştırır — K-122; yönetici hesabının vitrinde sepet ve sipariş sahibi olması — K-123) · `B8-03` (takip, teslim, gecikme — K-139, K-141, K-143; hak yenileme ve dosya güncelleme — K-147, K-148; **iptal bildirimi — K-180, K-199; havale hatırlatmasının sıklığı — K-167; e-posta yeniden deneme — K-234**) · `B8-04` (terk edilmiş sepet) · `B8-05` (deneme limitleri — K-107; stok kilitleme — K-128, **K-165**; stok yoklama — K-150; sepet şişirme — K-154; **ödenmemiş iptal siparişlerin ölçümü — K-184; takip formu limiti — K-185, K-187**) · `B8-06` (iş günü; kargoya verme çiti — K-139; **havale süresi ve iade çatısının sayımı — K-166, K-209**) · `B8-07`/`B8-08` (üyeliksiz veri işleme — K-97) · `B8-09` (misafir sepetinin çerez rejimi) · `B8-10` (saklama — K-115…K-117, K-126, K-148; **IBAN — K-214; ayıp talebi — K-227**) · **`B8-12` bağlayıcı** (yaş maddesi — K-114; 30 günlük üst sınır — K-139; **metin sürümlemesi — K-189; cayma metni — K-203**) · `B8-14` (gecikme, takip no düzeltme — K-139, K-141; hak yenileme — K-147; **satıcı iptal sebep listesi — K-199; yanlış geçişin düzeltilmesi — K-225; geç havalede yetki sınırı — K-183; iade paneli — K-209**) · `B9-04` (erişilebilirlik — K-93) · **`B9-06`** (manuel adımlar: havale işareti — K-159, hizmet tamamlama — K-175, iadenin stoğa eklenmesi — K-215) · `B9-07` (çok kalemli sepet — K-154) · `B9-08` (KDV listesi · K-40 boyut sınırı · görsel · ad tavanı · link ömürleri · oturum süreleri · şifre uzunluğu · kart ödeme süresi — K-128 · kargoya verme çiti — K-139 · dosya boyutu — K-143 · indirme hakkı — K-146 · asgari sipariş tutarı — K-155 · kargo ücreti ve eşik — K-132, K-135 · **havale süresi ve çiti — K-166 · sipariş no uzunluğu — K-185 · kargo KDV oranı — K-219**) · **`B9-13`** (post-MVP adayları: **kapıda ödeme — K-160 · e-Arşiv — K-217 · fatura yükleme — K-218**) · `B9-14` (toplu içe aktarma — K-88; **sipariş dışa aktarma fatura verisini taşır — K-217**) · Blok 8 yasal uyum turu (rakamsız fiyat işareti — K-131 · kargo KDV'si — K-132, K-219 · ücretsiz kargonun indirim dışı okunması — K-135). **`10 §4` dış ön koşul:** kurulum başına Google OAuth uygulaması (K-103). **`04`'e:** görselsiz kart (K-86) · "yok"/"Tükendi" (K-87) · SKU ön doldurma (K-88) · ana görsel geri düşüşü (K-91) · ödeme adımında "bu e-posta kayıtlı" hatırlatması (K-99) · adres yönetim ekranı (K-111) · "fatura adresim farklı" (K-112) · birleştirme mesajı (K-125) · panelde ayrılmış adet (K-128) · "ne değişti" özeti (K-129) · sepette "Tükendi" işareti ve çıkarılma mesajı (K-130) · "fiyatı değişti" işareti (K-131) · kargo ücreti, il seçimi, eşik ayarları ve kalan tutar mesajı (K-132, K-133, K-135, K-138) · kargoya verme süresi gösterimi ve ürün formundaki alan (K-139, K-140) · kargoya verme formu ve "Takip et" (K-141, K-142) · indirme düğmesi, dosya alanı, dosyası eksik varyant uyarısı (K-143…K-145) · sipariş sayfasında indirme hakkını yenileme (K-147) · dosya değiştirme ekranında değişikliğin eski alıcılara da ulaşması (K-148) · "bu adette stok yok" ve sipariş başına adet sınırı mesajları (K-150, K-151, K-152) · dijital üründe adet seçici yok (K-153) · asgari tutar ayarı ve eksik tutar mesajı (K-155, K-157) · **Blok 6:** ödeme yöntemleri ayar ekranı ve IBAN (K-161) · havale onay işlemi, tutar alanı yok (K-169) · iki eksenli durumun takip sayfası ve panel listesi (K-170) · hizmet "tamamlandı" ve geri dönen gönderi işlemleri (K-175, K-176) · onay özetinde e-posta satırı (K-193) · iki onay kutusu, dijital kalemde üçüncüsü (K-188, K-204) · dijital/hizmet siparişinde adres alanlarının gizlenmesi (K-194) · iptal, cayma, ayıp talebi yolları (K-195, K-207, K-227) · ürün formunda cayma istisnası (K-206) · panelde iade sayacı ve stoğa ekleme (K-209, K-215) · havale hattında IBAN alanı (K-214) · dört geri alınamaz onay penceresi (K-225) · satışı geçici kapatma anahtarı ve vitrin gösterimi (K-235) · yeniden alım uyarısı (K-236). **`05`'e:** doğrulanmamış hesap temizlik işi (K-102) · kabul edilen dosya türleri ve yüklenen dosyanın güvenliği (K-143) · indirmenin sayıldığı an (K-146) · çok kalemli sepetin performansı (K-154). **`06`'ya:** sipariş başlığı donmuş adresleri **ve iletişim e-postasını** taşır (K-113, K-116) · sepet ve sepet kalemi, fiyat referansıyla (K-123, K-131) · stok ayırma (K-128) · adreste il kapalı listeden (K-133) · dijital dosya ürün/varyant düzleminde (K-144) · sipariş kaleminde indirme sayacı (K-146, K-149) · kalem dosyayı varyantın güncel hâlinden okur (K-148) · üründe bir siparişte azami adet alanı (K-151, K-152) · **Blok 6:** firma ayarında havale anahtarı + IBAN (K-161) · siparişte iki durum alanı (K-170) · kalemde teslim işareti (K-179) · sipariş numarası (K-185) · iki onay kaydı, donmuş sözleşme sürümü ve firma kimliği (K-188…K-190) · kalem düzeyinde teslim tarihi (K-203) · üründe cayma istisnası + sebep (K-206) · cayma beyanı ve tarih damgası (K-207) · ayıp talebi ve iki durumu (K-228). **`08`'e:** kargo şirketi listesi ve takip bağlantısı kalıpları (K-142); kargo şirketi entegrasyonu yok (K-132) · **Blok 6:** sağlayıcının erişilemezliğinin tespiti (K-231) · ödeme sonucunun son sorgusu (K-232) · otomatik iade çağrısı (K-233).
- **Açık kararlar:** **A-02, A-03, A-04** — `01` yazım oturumunun bulduğu boşluklar (A-03'ün kapsamı dört satır, K-100). **A-05** — sözlükte **33 türetilmiş** İngilizce karşılık (60 terimin 27'si kayıtta); vadesi `02`'nin son yazım oturumu, `03`/`04`'ten önce. Dördü de **detay**. Ayrıntı: tracker §4.
- **Gate durumu:** Implementation başlamadı, faz yok. Doküman: **01 ⏳ (v0.4)** · **02 ⏳ (v0.5)** · 10 ⏳ · 03–09, 11–12 ⬚. Blok planı: 0–6 ✓ · 7–9 ⬚. Plandaki 111 konunun **72'si** kapandı; kalan 39 konu (Blok 7: 5 · Blok 8: 16 · Blok 9: 18).
- **Oturum kapanışı (K-38):** doküman döneminde bu blok `/handoff` beklenmeden oturum sonu `docs:` PR'ının içinde güncellenir. Üç kez kaçırıldı (Blok 3 oturum 1 ve 4, Blok 5 oturum 1), üçünü de INSTRUCTIONS §3.0 yakaladı. **Blok 6 kurala uygun kapandı:** kararlar konu bittikçe kayda yazıldı, kapanış adımları oturum sonu PR'ının içinde koştu.
- **Son güncelleme:** 2026-09-18

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
| Kritik olmayan workshop soruları öneriyle kaydedilir | `00` §C.2 · `INSTRUCTIONS.md` §2 · `GUARDRAILS.md` §6 · `checklists/document-stage.md` §3 | 2026-09-17 |
| PR base'i her zaman `main`'dir | `INSTRUCTIONS.md` §3.2 · `.github/workflows/ci.yml` (base filtresi kaldırıldı) | 2026-09-17 |
| Workshop konusu tek mesajda, numaralı öneri listesi olarak sunulur; kritik maddeler ⚠ ile işaretlenir | `INSTRUCTIONS.md` §2 · `checklists/document-stage.md` §3 | 2026-09-18 |
