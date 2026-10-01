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

- **Son tamamlanan:** **Yazım turu — `10` oturumu ✓ (2026-10-02, `10` v0.2, `01` v0.8, `02` v0.9). Yazım turu tamamlandı (K-432).** `10 §1–§5` ilk kez yazıldı: §1 çıta (K-22'nin cümlesi, ölçüler `01 §6`'da — K-434) ve dört kabul · §2 yetmiş altı kapsam satırı (KP-) · §3 altmış kapsam dışı satırı — yirmi iki "Evet", otuz sekiz "Açık" (KD-) · §4.1 on iki maddelik kurulum kontrol listesi (ÖK-) · §4.2 on kısıt, üçü "Kalkmaz" (SK-) · §5 yirmi iki adaylık yol haritası (YH-). **A-11 kapandı** (K-470…K-472): §3 "Hayır" kullanmaz, kalıcı sınırlar `01 §7`'ye işaretle anılır; "Kalkmaz" satırları `01 §7`'deki satırı gösterir; aday listesi dışındaki her §3 satırı "Açık". Anlamsal tarama `10`'u göstermeyen **on bir kararı** §3'e işledi (K-27, K-158, K-169, K-227, K-269, K-288, K-301, K-305, K-313, K-398, K-427); devir taraması K-403/K-423'ün "otomasyon"unun adsız kaldığını buldu. **Yazım altı boşluk buldu → K-473…K-478, hepsi öneriyle (⚠ grubuna giren madde yok):** otomasyonun adı — kargo şirketi entegrasyonu ve havale eşleştirmesi, aday 20 → 22 · hacim tetikleyicisi (üç aylık dönemin en az iki ayında en yoğun gün 50'yi aşarsa) · §4.2'nin diğer tetikleyicileri · §4'ün iki parçası · Ü-1'e alan adı · Google uygulaması kurulum ayarıdır, tanımlı değilse düğme görünmez. Ayrıntı: tracker §6.1.
- **Önceki:** Yazım turu `02` oturumu ✓ (2026-10-02, `02` v0.8; K-447…K-469) ve `01` oturumu ✓ (2026-10-01, `01` v0.6; K-439…K-446). Blok 9 ✓ (2026-09-30) — Aşama 1'in workshop turu bitti. Ayrıntı: tracker §6.1.
- **SIRADA — kalite döngüsü (K-430, K-431; K-437'nin 2. adımı), `01` ile başlar.** `01` → `02` → `10`, doküman başına bir kez, **biri TEMİZ olmadan sonrakine geçilmez**; audit ve deep-review **çok mercekli** koşulur ve bulgular karşı-doğrulanır, cross-review `cursor-agent` ile yalnız dokümanın kendisi ve şablonuyla koşar (karar kaydı verilmez). Ardından çakışma taraması (K-429), checkpoint, öğrenim terfisi — K-438'in dört adayı ve yazım turunun öğrenimleri (tracker §6.1) —, arşiv işareti (K-436) ve Aşama 2'ye devir; sıra K-437'de kilitli.
- **Taslaklar:** `01` **v0.8** (§1–§8) · `02` **v0.9** (§1–§13) · `10` **v0.2** (§1–§5).
- **Yöntem (`INSTRUCTIONS §2`):** konu başına tek mesaj, numaralı öneri listesi, ⚠ işaretli üç grup; tamlık taraması liste hazırlanırken koşar, blok sonunda bloğun tamamına genişler. Yazım oturumunda bulunan boşluklar da aynı biçimle sunulur — ⚠ maddeler önce, K numaraları mesaj sırasıyla (`01`: sekiz, `02`: yirmi üç, `10`: altı madde; üçünde de ⚠ maddeye itiraz yok). `10`'da ⚠ grubuna giren madde çıkmadığı için altı madde öneriyle kaydedildi ve oturum sonu PR'ında bildirildi.
- **İleriye bağlayıcı kayıtlar:** **K-352** — `B9-05` ölçüm eklerse K-348 ve K-349 yeniden açılır (koşul gerçekleşmedi, K-397) · **K-400** — `B9-10` yalnız sipariş verisinden türetir · **K-420** — yol haritası yalnız `10 §3`'ten beslenir, `01 §7`'den asla · **K-424**'ün üç bağımlılık zinciri (öneri→analitik→çerez bandı · stokta haber ver ve terk edilmiş sepet→İYS onayı · SMS→yeni dış ön koşul; K-473 kargo ve havale otomasyonunu aynı kalıba koydu) · **K-439** — `01 §7`'nin sınırı ile aynı alandaki yol haritası adayı çelişmez: sınır kimliktir, aday özellik · **K-474** — tetikleyici gerçekleşince kargo entegrasyonu ve havale eşleştirmesi öne çekilir.
- **Aşağı dokümanlara devirler:** `04`–`09`, `12` ve `DEPLOY_RUNBOOK` satırları tracker'ın etki sütunlarındadır. **Blok 9'un ekledikleri:** `04`'e satış özeti ekranı (K-398), "bekleyen işlerim" sayaç bloğu (K-405), sipariş dışa aktarma (K-426) · `05`'e genel istek hızı (K-331), yedekleme ve kurtarma (K-407), erişim kayıtları (K-397) · `DEPLOY_RUNBOOK`'a yedek erişimi ve çıkış hakkı (K-427). **Yazım turunun ekledikleri:** `01` oturumu — `12`'ye ETBİS ve firma kimliği senaryoları (K-444) · `02` oturumu — `04`'e cayma düğmesinin kargoya verilince açılması (K-452), panel ana sayfasında kurulum kontrol listesi (K-469) ve altı sayaç (K-467), iptal sebebinde altı seçenek (K-451) ve düzeltme sebep seçici (K-466); `05`'e kayan pencereli limitler (K-460), günlük imha işi (K-449), SVG'nin dışarıda kalması (K-461); `06`'ya sipariş–sepet bağı ve sürümlü yasal metin kaydı (K-457, K-458); `12`'ye satış kapısının dört koşulu (K-465) ve §11'in değerleri · `10` oturumu — `04`'e Google düğmesinin kurulumda tanımlıysa görünmesi (K-478); `DEPLOY_RUNBOOK`'a `10 §4.1`'in kurulum adımları (K-476); `12`'ye `10 §2`'nin yetmiş altı satırı senaryo olarak (K-24) ve Ü-1'in alan adı ön koşulu (K-477).
- **Açık kararlar:** tracker §4'te **açık satır yok** — A-11 kapandı (K-470…K-472); A-02…A-05 ve A-12 önceki yazım oturumlarında kapanmıştı. `02 §13`'ün tablosu boştur. `01` ve `10`'un şablonunda açık kararlar bölümü yok — K-428/K-429 ona işaret ediyor; öğrenim adayı olarak tracker §6.1'de.
- **Gate durumu:** Implementation başlamadı, faz yok. Doküman: **01 ⏳ (v0.8)** · **02 ⏳ (v0.9)** · **10 ⏳ (v0.2)** · 03–09, 11–12 ⬚. Blok planı: **0–9 ✓ — tamamı kapandı.** Plandaki **111 konunun 111'i** kapandı; 2026-08-17'den bu yana **478 karar**.
- **Oturum kapanışı (K-38):** doküman döneminde bu blok `/handoff` beklenmeden oturum sonu `docs:` PR'ının içinde güncellenir. Üç kez kaçırıldı (Blok 3 oturum 1 ve 4, Blok 5 oturum 1), üçünü de INSTRUCTIONS §3.0 yakaladı. Blok 6, 7, `02` yazım oturumu, Blok 8, Blok 9 ve yazım turunun `01`, `02` ve **`10`** oturumları kurala uygun kapandı.
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
