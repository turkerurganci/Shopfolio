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

- **Son tamamlanan:** **Yazım turu — `01` oturumu ✓ (2026-10-01, `01` v0.6).** §6–§8 ilk kez yazıldı: §6 iki eksenli tek tablo (Ü-1…Ü-4 kabul kapısı, M-1…M-4 kapı değil, dört ölçülemeyen), §7 on kalıcı kimlik sınırı (S-1…S-10), §8 yedi ürün varsayımı (V-1…V-7). §1'in dört K-29 satırı (K-417, K-419, K-421, K-433) işlendi. **Yirmi iki anlamsal K-29 kaçağı** §1–§5'e işlendi — etki sütunu `01`'i göstermediği için mekanik taramanın göremediği kararlar; en ağırı misafir alıcının "neden geri döner" hücresinin K-100 ile çelişmesiydi. A-02, A-03, A-04 kapandı. **Yazım sekiz boşluk buldu → K-439…K-446**; proje sahibi tek mesajlık listeye itiraz etmedi. ⚠ üçlü: çok dillilik kalıcı sınır değil, yol haritası adayı; SaaS sınır değil, aday; satış özeti mağaza düzeyinin dört ölçüsünü gösterir. Yeni açıklar: **A-11** (`10`), **A-12** (`02`). Tarama çok ajanlıydı (altı blok dilimi, 89 aday); oturum limiti doğrulama aşamasını kesti, proje sahibi iş akışını durdurdu ve adaylar yazıcı tarafından kayda karşı elendi. Ayrıntı: tracker §6.1.
- **Önceki:** Blok 9 ✓ (2026-09-30) — Aşama 1'in workshop turu bitti, elli dört karar (K-385…K-438); toplu onayla kaydedilen yirmi iki karar 2026-10-01'de gözden geçirildi, itiraz yok. Blok 8 ✓ (2026-09-26). Ayrıntı: tracker §6.1.
- **SIRADA — yazım turunun `02` oturumu (K-432, K-437'nin 1. adımı).** `02 §1–§3, §5–§7`'nin K-29 güncellemeleri (Blok 8'den 95, Blok 9'dan 50 satır; bu oturumun K-441'i de `02 §3`'e işaret taşır) ve `02 §4, §8–§13`'ün ilk yazımı. Sözlüğe Blok 8'den altı terim girecek (`ContactRequest`, `ContactRequestStatus`, `Closed`, `AdminInvitation`, `AuditLog`, `Notification`). **A-05** ve **A-12** orada kapanır. **`01`'den taşınan yöntem:** taslağı yazılmış bölümler yalnız işaretli satırlarla değil, kaydın tamamına karşı anlamsal olarak taranır — `01`'de yirmi iki kaçak bu yolla bulundu. Ardından `10` oturumu (`10 §1–§5`; **A-11**). K-30 gereği her oturum workshop'tan ayrıdır, girdi karar kaydıdır.
- **Sonra:** kalite döngüsü (K-430, K-431) — `01` → `02` → `10`, doküman başına bir kez, **biri TEMİZ olmadan sonrakine geçilmez**; audit ve deep-review **çok mercekli** koşulur ve bulgular karşı-doğrulanır, cross-review `cursor-agent` ile. Ardından çakışma taraması (K-429), checkpoint, öğrenim terfisi, arşiv işareti (K-436) ve Aşama 2'ye devir — sıra K-437'de kilitli.
- **Taslaklar:** `01` **v0.6** (§1–§8) · `02` **v0.7** (§1–§3, §5–§7) · `10` henüz yazılmadı.
- **Yöntem (`INSTRUCTIONS §2`):** konu başına tek mesaj, numaralı öneri listesi, ⚠ işaretli üç grup; tamlık taraması liste hazırlanırken koşar, blok sonunda bloğun tamamına genişler. Yazım oturumunda bulunan boşluklar da aynı biçimle tek mesajda sunuldu (`01`: sekiz madde, itiraz sıfır).
- **İleriye bağlayıcı kayıtlar:** **K-352** — `B9-05` ölçüm eklerse K-348 ve K-349 yeniden açılır (koşul gerçekleşmedi, K-397) · **K-400** — `B9-10` yalnız sipariş verisinden türetir · **K-420** — `B9-13` yalnız `10 §3`'ten beslenir, `01 §7`'den asla · **K-424**'ün üç bağımlılık zinciri (öneri→analitik→çerez bandı · stokta haber ver ve terk edilmiş sepet→İYS onayı · SMS→yeni dış ön koşul) · **K-439** — `01 §7`'nin sınırı ile aynı alandaki yol haritası adayı çelişmez: sınır kimliktir, aday özellik.
- **Aşağı dokümanlara devirler:** `04`–`09`, `12` ve `DEPLOY_RUNBOOK` satırları tracker'ın etki sütunlarındadır. **Blok 9'un ekledikleri:** `04`'e satış özeti ekranı (K-398), "bekleyen işlerim" sayaç bloğu (K-405), sipariş dışa aktarma (K-426) · `05`'e genel istek hızı (K-331), yedekleme ve kurtarma (K-407), erişim kayıtları (K-397) · `10 §5` yol haritasını alır (K-422) · `DEPLOY_RUNBOOK`'a yedek erişimi ve çıkış hakkı (K-427). **Yazım turu `01`'in ekledikleri:** `10 §3`–`§5`'e aday listesi yirmiye çıktı (K-446 — SaaS, çoklu dil, Facebook girişi, toplu içe aktarma) ve S-10'un "Kalkmaz" satırı (K-439, A-11) · `02 §3` ve `04`'e satış özetinin dört ölçüsü (K-441) · `12`'ye ETBİS ve firma kimliği senaryoları (K-444).
- **Açık kararlar:** **A-05** — sözlükte 36 türetilmiş İngilizce karşılık; vadesi `02`'nin son yazım oturumu. **A-11** — `10 §3`–`§5`'in yazım biçimi (K-25 ile K-435, "Kalkmaz" satırları, cevabı yazılı olmayan kapsam dışı satırları); vadesi `10` oturumu. **A-12** — `02`'ye kalan üç parça (K-147'nin elle adımı, K-08'in kalan riski, mağaza düzeyi ölçülerinin tanımı); vadesi `02` oturumu. **A-02…A-04 kapandı** (K-442, K-443, `01 §1`). Ayrıntı: tracker §4.
- **Gate durumu:** Implementation başlamadı, faz yok. Doküman: **01 ⏳ (v0.6)** · **02 ⏳ (v0.7)** · 10 ⬚ · 03–09, 11–12 ⬚. Blok planı: **0–9 ✓ — tamamı kapandı.** Plandaki **111 konunun 111'i** kapandı; 2026-08-17'den bu yana **446 karar**.
- **Oturum kapanışı (K-38):** doküman döneminde bu blok `/handoff` beklenmeden oturum sonu `docs:` PR'ının içinde güncellenir. Üç kez kaçırıldı (Blok 3 oturum 1 ve 4, Blok 5 oturum 1), üçünü de INSTRUCTIONS §3.0 yakaladı. Blok 6, 7, `02` yazım oturumu, Blok 8, Blok 9 ve **yazım turu `01` oturumu** kurala uygun kapandı.
- **Son güncelleme:** 2026-10-01

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
| Tamlık taraması liste hazırlanırken koşulur; blok sonunda bloğun tamamına genişler | `INSTRUCTIONS.md` §2 · `checklists/document-stage.md` §3 | 2026-09-19 |
| Taslağı yazılmış bölüme dokunan karar `(taslak güncellenecek)` işaretini alır; bloğun son konusunda işaretsiz eşleşme taranır (K-29) | `checklists/document-stage.md` §3 | 2026-09-19 |
