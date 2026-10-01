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

- **Son tamamlanan:** **Yazım turu — `02` oturumu ✓ (2026-10-02, `02` v0.8, `01` v0.7).** §4 ve §8–§13 ilk kez yazıldı (süreler, kötüye kullanım, bildirimler, yönetim, kırk iki parametre, yasal çerçeve, açık kararların kuralı). İşaretli **150 satır** (Blok 8: 97 · Blok 9: 52 · `01` oturumu: 1) §1–§3 ve §5–§7'ye işlendi; §3'e üç grup girdi (iletişim talebi, yasal metinler, site geneli); sözlük 78 → **86**. **A-05** (K-455) ve **A-12** (K-445, K-456, K-457) kapandı. Anlamsal tarama **dokuz işaret** ve **elli anlamsal K-29 kaçağı** buldu — en ağırı K-340: cayma sipariş onayından açık, sözlük "teslimattan sonra" diyordu. **Yazım yirmi üç boşluk buldu → K-447…K-469**; proje sahibi tek mesajlık listeye itiraz etmedi. ⚠ sekizli: firma ayarı varsayılanları (kargo 0 TL) · süre çitleri · saklamanın başlangıcı ve günlük imha · fatura adresi "Kargoya verildi"ye kadar · altıncı iptal sebebi ve kargo bedeli · kargoya verilmemiş üründe yol iptal · üyelik sözleşmesi yok · karışık siparişte bütçe hat başına. K-454 ve K-457 `01 §6`'ya dokundu → `01` v0.7. Ayrıntı: tracker §6.1.
- **Önceki:** Yazım turu `01` oturumu ✓ (2026-10-01, `01` v0.6; K-439…K-446, A-02…A-04 kapandı). Blok 9 ✓ (2026-09-30) — Aşama 1'in workshop turu bitti. Ayrıntı: tracker §6.1.
- **SIRADA — yazım turunun `10` oturumu (K-432, K-437'nin 1. adımı).** `10 §1–§5`'in ilk yazımı; girdi karar kaydı, `10` şablonu, `01` v0.7 ve `02` v0.8 — K-30 gereği workshop'tan ayrı oturum. **A-11** orada kapanır: K-25 ile K-435'in gerilimi, "Kalkmaz" satırları, K-25 cevabı yazılı olmayan `10 §3` satırları. `10 §1` çıtayı K-22'nin cümlesiyle yazar ve ölçüler için `01 §6`'ya işaret eder (K-434); `10 §2` her satırı bir karara bağlar (K-435) ve §11'in özet satırını taşır (K-411); `10 §4`'ün dış ön koşulları K-12, K-14, K-103, K-358, K-360, K-365, K-378'dedir — panelin kurulum kontrol listesi (K-469) ayrıdır. **İki oturumun öğrenimi taşınır:** kayıt yalnız işaretli satırlarla değil tamamına karşı anlamsal taranır (`01`: 22, `02`: 50 kaçak) ve bir konuya devredilmiş her parça devralan konunun kararlarında aranır (`02`'de §11 değerleri ve ilk kurulum rehberi kapanmamış devirlerdi).
- **Sonra:** kalite döngüsü (K-430, K-431) — `01` → `02` → `10`, doküman başına bir kez, **biri TEMİZ olmadan sonrakine geçilmez**; audit ve deep-review **çok mercekli** koşulur ve bulgular karşı-doğrulanır, cross-review `cursor-agent` ile. Ardından çakışma taraması (K-429), checkpoint, öğrenim terfisi, arşiv işareti (K-436) ve Aşama 2'ye devir — sıra K-437'de kilitli.
- **Taslaklar:** `01` **v0.7** (§1–§8) · `02` **v0.8** (§1–§13) · `10` henüz yazılmadı.
- **Yöntem (`INSTRUCTIONS §2`):** konu başına tek mesaj, numaralı öneri listesi, ⚠ işaretli üç grup; tamlık taraması liste hazırlanırken koşar, blok sonunda bloğun tamamına genişler. Yazım oturumunda bulunan boşluklar da aynı biçimle tek mesajda sunuldu — ⚠ maddeler önce, K numaraları mesaj sırasıyla (`01`: sekiz madde, `02`: yirmi üç madde; ikisinde de itiraz sıfır).
- **İleriye bağlayıcı kayıtlar:** **K-352** — `B9-05` ölçüm eklerse K-348 ve K-349 yeniden açılır (koşul gerçekleşmedi, K-397) · **K-400** — `B9-10` yalnız sipariş verisinden türetir · **K-420** — `B9-13` yalnız `10 §3`'ten beslenir, `01 §7`'den asla · **K-424**'ün üç bağımlılık zinciri (öneri→analitik→çerez bandı · stokta haber ver ve terk edilmiş sepet→İYS onayı · SMS→yeni dış ön koşul) · **K-439** — `01 §7`'nin sınırı ile aynı alandaki yol haritası adayı çelişmez: sınır kimliktir, aday özellik.
- **Aşağı dokümanlara devirler:** `04`–`09`, `12` ve `DEPLOY_RUNBOOK` satırları tracker'ın etki sütunlarındadır. **Blok 9'un ekledikleri:** `04`'e satış özeti ekranı (K-398), "bekleyen işlerim" sayaç bloğu (K-405), sipariş dışa aktarma (K-426) · `05`'e genel istek hızı (K-331), yedekleme ve kurtarma (K-407), erişim kayıtları (K-397) · `10 §5` yol haritasını alır (K-422) · `DEPLOY_RUNBOOK`'a yedek erişimi ve çıkış hakkı (K-427). **Yazım turu `01`'in ekledikleri:** `10 §3`–`§5`'e aday listesi yirmiye çıktı (K-446) ve S-10'un "Kalkmaz" satırı (K-439, A-11) · `12`'ye ETBİS ve firma kimliği senaryoları (K-444). **Yazım turu `02`'nin ekledikleri:** `04`'e cayma düğmesinin kargoya verilince açılması (K-452), panel ana sayfasında kurulum kontrol listesi (K-469) ve altı sayaç (K-467), iptal sebebinde altı seçenek (K-451) ve düzeltme sebep seçici (K-466) · `05`'e kayan pencereli limitler (K-460), günlük imha işi (K-449), SVG'nin dışarıda kalması (K-461) · `06`'ya sipariş–sepet bağı (K-457 ölçüsü için) ve sürümlü yasal metin kaydı (K-458) · `12`'ye satış kapısının dört koşulu (K-465) ve §11'in değerleri (K-447, K-448, K-459…K-462).
- **Açık kararlar:** **A-11** — `10 §3`–`§5`'in yazım biçimi (K-25 ile K-435, "Kalkmaz" satırları, cevabı yazılı olmayan kapsam dışı satırları); vadesi `10` oturumu. **A-05 ve A-12 kapandı** (K-455; K-445, K-456, K-457); A-02…A-04 `01` oturumunda kapanmıştı. `02 §13`'ün tablosu boştur — satırını aşama kapanışının ataması açar (K-428). Ayrıntı: tracker §4.
- **Gate durumu:** Implementation başlamadı, faz yok. Doküman: **01 ⏳ (v0.7)** · **02 ⏳ (v0.8)** · 10 ⬚ · 03–09, 11–12 ⬚. Blok planı: **0–9 ✓ — tamamı kapandı.** Plandaki **111 konunun 111'i** kapandı; 2026-08-17'den bu yana **469 karar**.
- **Oturum kapanışı (K-38):** doküman döneminde bu blok `/handoff` beklenmeden oturum sonu `docs:` PR'ının içinde güncellenir. Üç kez kaçırıldı (Blok 3 oturum 1 ve 4, Blok 5 oturum 1), üçünü de INSTRUCTIONS §3.0 yakaladı. Blok 6, 7, `02` yazım oturumu, Blok 8, Blok 9 ve yazım turunun `01` ve **`02`** oturumları kurala uygun kapandı.
- **Son güncelleme:** 2026-10-02

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
