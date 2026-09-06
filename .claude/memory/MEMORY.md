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

- **Son tamamlanan:** **Blok 3 · oturum 2** — dört konu kapandı, on altı karar (**K-42…K-57**): `B3-02` kategori (üç seviyeli ağaç · ürün her düğüme asılır · üst kategori alt dalları toplar · çoklu kategori + ana kategori · dolu kategori silinemez) · `B3-03` arama-filtre-sıralama (yalnız ürün adında arama · fiyat + stok filtresi · sıralama seçeneği yok, sabit düzen "en yeni") · `B3-04` stok (varyantta **sayısal** stok · tükenmiş varyant ve ürün "Tükendi" olarak görünür · adet ziyaretçiye gösterilmez) · `B3-05` yayın (üçlü durum `Draft`/`Published`/`Archived`, **her iki düzlemde** · arşiv adresi "artık satılmıyor" sayfası döner · taslak yalnız yöneticiye görünür). Öncesinde Blok 3 · oturum 1 → **K-39…K-41** (PR #11), `01 §1–§5` taslağı (`01` v0.2, PR #10), Blok 0/1/2 kapanışı (PR #4, #5, #7, #8, #9).
- **Sırada:** **Blok 3 · oturum 3** — `B3-06`…`B3-12`, yedi konu, ayrı chat. `B3-06` KDV/yuvarlama ve `B3-09` fiyat dondurma sonradan düzeltilmesi pahalı kararlardır; `B3-06` ile başlanır. **K-39 gereği** `B3-09`, `B3-11`, `B3-12` **iki düzlemli** sorulur (ürün mü, varyant mı). `B3-07`'ye iki devir var: K-56 arşiv sayfasında fiyat gösterilip gösterilmeyeceğini, K-11 indirim beyanının referans fiyatını oraya bıraktı. `01`'in §6–§8'i ve kalite döngüsü Blok 9'a kadar açılmaz (K-28); `02 §1` taslağı da Blok 3 kapanmadan yazılamaz (K-41 ve K-54 oraya yazıyor).
- **Açık kararlar:** **A-02, A-03, A-04** — yazım oturumunun kayıtta bulduğu üç boşluk (matris derece değerleri · aktör tablosunun "neden geri döner" hücreleri · §1'in sayısal somutluğu). Üçü de **detay**, varlık kararı açık değil; vadeleri `01 §6–§8` yazım oturumu. Ayrıntı: tracker §4.
- **Gate durumu:** Implementation başlamadı, faz yok. Doküman: 01 ⏳ (v0.2, §1–§5 taslak) · 02 ⏳ · 10 ⏳ · 03–09, 11–12 ⬚
- **Oturum kapanışı (K-38):** Doküman döneminde bu blok, `/handoff` beklenmeden **oturum sonu `docs:` PR'ının içinde** güncellenir. Bayat kalırsa bunu yakalayacak başka kapı yok. **Bir kez kaçırıldı:** Blok 3 oturum 1 kapanış koşmadan bitti, K-39…K-41 altı gün commit'siz kaldı; INSTRUCTIONS §3.0 açılış kapısı yakaladı ve kapanış 2026-08-29'da geriye dönük koşturuldu (tracker §6.2 Blok 3 notu). **Oturum 2'de kural yerinde koştu** — kapanış PR'ın içinde yapıldı, gecikme tekrarlanmadı.
- **Son güncelleme:** 2026-08-30

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
