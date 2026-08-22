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

- **Son tamamlanan:** **Aşama 1 · Blok 0, 1 ve 2 kapandı** — Blok 1 → **K-08…K-17** (PR #4) · Blok 2 → **K-18…K-27** (PR #5) · Blok 0 → **K-28…K-38** (PR #7, #8). **Aşama 1'de açık karar yok** (A-01 → K-18). Kalan yedi blok (3–9) hiç açılmadı.
- **Sırada:** **Blok 3 workshop'u DEĞİL** — K-30 gereği önce **`01 §1–§5` taslak yazımı**, ayrı chat'te. Girdisi: karar kaydı §2 ve §4, `01` şablonu, 00 §C.6 ürün bağlamı katmanı — **workshop sohbet geçmişi değil**. Ardından **Blok 3** (katalog, stok, fiyatlandırma — 12 konu); `B3-06` KDV/yuvarlama ve `B3-09` fiyat dondurma sonradan düzeltilmesi pahalı kararlar.
- **Açık PR / bekleyen CI:** yok — #7 ve #8 merge edildi, main CI + Publish yeşil.
- **Gate durumu:** Implementation başlamadı, faz yok. Doküman: 01 ⏳ · 02 ⏳ · 10 ⏳ · 03–09, 11–12 ⬚
- **Oturum kapanışı (K-38):** Doküman döneminde bu blok, `/handoff` beklenmeden **oturum sonu `docs:` PR'ının içinde** güncellenir. Bayat kalırsa bunu yakalayacak başka kapı yok.
- **Son güncelleme:** 2026-08-22

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
