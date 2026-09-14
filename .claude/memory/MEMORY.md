# Proje Hafızası

> **Bu dosya indeks + güncel durum snapshot'ıdır.** İçerik gömülmez — her hafıza ayrı bir dosyadır.
> Ne buraya girer / ne L1–L5'e terfi eder: [`README.md`](README.md).
> **Otoriter kaynak değildir** — task durumu için [`../../Docs/IMPLEMENTATION_STATUS.md`](../../Docs/IMPLEMENTATION_STATUS.md).

---

## Proje Özeti

- **Ad:** Shopfolio
- **Tanım:** Herhangi bir firmanın kendi ürünlerini sergileyip çevrimiçi satabileceği ve kurumsal tanıtımını yapabileceği; üyelik, Google/Facebook ile giriş, sipariş, ödeme ve sipariş takibi içeren web uygulaması.
- **Dönem:** Doküman üretimi
- **Doküman dili:** Türkçe · **Kod dili:** İngilizce

---

## Güncel Durum

> Kısa tut. Tarihsel detay `MEMORY_ARCHIVE.md`'ye taşınır (00 §G.2).

- **Son tamamlanan:** **Blok 3 · oturum 4** — bir konu kapandı, dört karar (**K-77…K-80**): `B3-09` sipariş anında fiyat dondurma (snapshot). Kalem, tutarı üreten her değerin yanında ürünün o anki **tanımını** da dondurur — ad, seçenek değerleri, görsel (K-77); donan her şey kalemde **düz** yaşar, ürün düzlemli alanlar (KDV oranı, indirim oranı) kaleme kopyalanır, ara gruplama katmanı yoktur (K-78); indirim beyanının **referans fiyatı** da donar, canlı indirim kaydına bağlanmaz (K-79); snapshot **sipariş oluştuğu anda** alınır — müşterinin onayladığı tutar bağlayıcıdır, ödeme sonucundan bağımsızdır ve bu K-76'dan (kupon sayacı ödemede sayılır) **bilinçli ayrışmadır** (K-80). Öncesinde **Blok 3 · oturum 3** — üç konu, on dokuz karar (**K-58…K-76**, PR #13): `B3-06` fiyat rejimi (ürün başına KDV oranı · oran üründe, fiyat varyantta · fiyat KDV **dahil** girilir · yuvarlama tek noktada, kalem KDV'sinde · fiyat iki ondalık) · `B3-07` indirim (**MVP'de var**, referansı sistem 30 günlük geçmişten hesaplar · ürün düzleminde, yüzde, tarihli · kısa geçmişte yayına girişten beri en düşük · referansın altına inmeyen indirim engellenir · arşiv sayfasında fiyat gösterilmez) · `B3-08` kupon (**MVP'de var**, sepet toplamına iner · yüzde **ve** sabit tutar · sabit tutar kalemlere orantılı dağıtılır, artık en büyük kaleme · sepeti aşamaz, asgari sepet tutarı alanı · indirimli fiyattan iner · tarih + toplam kullanım adedi · ödeme başarılı olunca sayılır). **Üç konuda da ajanın önerisi reddedildi** (indirim yok / kupon yok / yalnız yüzde) ve kapsam bilinçli olarak genişletildi. Daha öncesinde oturum 2 → K-42…K-57 (PR #12), oturum 1 → K-39…K-41 (PR #11), `01 §1–§5` taslağı (PR #10).
- **Sırada:** **Blok 3 · oturum 5** — `B3-10` (özel izin/mevzuat gerektiren ürünler ve hizmet/dijital ürün sınırı) · `B3-11` (katalog ölçeği ve asgari yayın alan seti) · ★`B3-12` (ürün ve içerik medyası: görsel adedi, ana görsel, sıralama, boyut/format, alternatif metin, ad/açıklama uzunluk sınırları) — **üç konu, ayrı chat; bloğun son oturumu olması beklenir** (bittiğinde `✓` düşer, K-33). **K-39 gereği** `B3-11` ve `B3-12` **iki düzlemli** sorulur (ürün mü, varyant mı). `B3-11` iki kararı devralır: K-11 fiyatı yayın için **zorunlu alan** yaptı, K-40 seçenek boyutu sınırını (en fazla iki) B3-11'e downstream yazdı. `B3-12`, K-77'nin snapshot kapsamını devralır — görsel sipariş kalemine kopyalanacaktır. `01`'in §6–§8'i ve kalite döngüsü Blok 9'a kadar açılmaz (K-28); `02 §1` taslağı da Blok 3 kapanmadan yazılamaz (K-41, K-54 ve K-58 oraya yazıyor).
- **Blok 3'ün sonraki bloklara devirleri:** `B5-02` (kupon sayacında eşzamanlılık, stok rezervasyonuyla aynı rejim) · `B5-03` (sepet canlıdır, kalem sipariş anında donar — sepet tarafının davranışı orada) · `B6-04` (kısmi iptal/iade **kalem** düzeyinde ele alınacak, K-78 ara katman açmadı) · `B6-05` (yarıda kalan ödemede siparişin kaderi) · `B6-10` (iade edilen siparişte kupon adedi geri gelir mi) · `B6-11` (fatura kararı — K-78 oranları kaleme yazdırdı ki geçmiş sipariş geriye dönük üretilebilsin) · `B6-15` (aynı eşzamanlılık, hata envanteri) · `B8-06` (indirimin tarihli başlaması/bitmesi — saat dilimi ve "gün sonu" tanımı) · `B8-10` (fiyat geçmişinin saklama süresi; K-79 indirim kanıtını bu kaydın ömrüne bağlamamak için referansı kaleme dondurdu) · `B9-08` (KDV oran listesinin kaynağı ve runtime ayarlanabilirliği) · Blok 8 yasal uyum turu (indirim duyurusunda süre beyanının tam yükümlülüğü).
- **Açık kararlar:** **A-02, A-03, A-04** — yazım oturumunun kayıtta bulduğu üç boşluk (matris derece değerleri · aktör tablosunun "neden geri döner" hücreleri · §1'in sayısal somutluğu). Üçü de **detay**, varlık kararı açık değil; vadeleri `01 §6–§8` yazım oturumu. Ayrıntı: tracker §4.
- **Gate durumu:** Implementation başlamadı, faz yok. Doküman: 01 ⏳ (v0.2, §1–§5 taslak) · 02 ⏳ · 10 ⏳ · 03–09, 11–12 ⬚
- **Oturum kapanışı (K-38):** Doküman döneminde bu blok, `/handoff` beklenmeden **oturum sonu `docs:` PR'ının içinde** güncellenir. Bayat kalırsa bunu yakalayacak başka kapı yok. **İki kez kaçırıldı:** Blok 3 oturum 1 (K-39…K-41, **altı gün** commit'siz) ve oturum 4 (K-77…K-80, **sekiz gün** commit'siz); ikisini de INSTRUCTIONS §3.0 açılış kapısı yakaladı, ikisinde de kapanış geriye dönük koşturuldu. Oturum 2 ve 3'te kural yerinde koştu. **Gözlenen desen:** oturum PR'a varmadan kesildiğinde K-38 hiç koşmuyor — bu K-38'in metninde **kabul edilmiş** açıktır, kapatan mekanizma §3.0'dır. Karşı ilaç oturum 3'te işe yaradı: kararlar **konu bittikçe** kayda yazıldı, oturum sonuna biriktirilmedi.
- **Son güncelleme:** 2026-09-14

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
