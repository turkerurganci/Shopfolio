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

- **Son tamamlanan:** **Aşama 1 · Blok 1 ve Blok 2 kapandı** — Blok 1 → **K-08…K-17** (`125cb46`, PR #4) · Blok 2 → **K-18…K-27** (`7fbe4b6`, PR #5). **A-01 kapandı (K-18)**; Aşama 1'de açık karar yok.
- **Sırada:** **Blok 0'ı kapat** — `B0-02`…`B0-09` hiç işaretlenmemiş ama çoğu fiilen işletildi (B0-05 iki blokta uygulandı, B0-06 27 satırda, B0-07 üç parkta, B0-08 A-01'de, B0-09 handoff'ta). `B0-02` ve `B0-03` gerçekten konuşulmadı. Ardından **Blok 3** — katalog, stok ve fiyatlandırma (12 konu); `B3-06` KDV/yuvarlama ve `B3-09` fiyat dondurma sonradan düzeltilmesi pahalı kararlar.
- **Açık PR / bekleyen CI:** yok — #4 ve #5 merge edildi, main CI + Publish yeşil.
- **Gate durumu:** Implementation başlamadı, faz yok. Doküman: 01 ⏳ · 02 ⏳ · 10 ⏳ · 03–09, 11–12 ⬚
- **Son güncelleme:** 2026-08-18

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
