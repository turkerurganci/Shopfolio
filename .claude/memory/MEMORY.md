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

- **Son tamamlanan:** **Kalite döngüsü — Proje Vizyonu (`01`) ✓ (2026-10-02/03, `01` v0.26, `02` v0.10, `10` v0.3).** Audit ve deep review: dokuz mercek, 65 bulgunun 59'u doğrulandı. Cross-review 18 turda TEMİZ: 1–7. tur `cursor-agent`, 8. turda Cursor kullanım limiti doldu, proje sahibinin kararıyla 8–18. tur ChatGPT hesabıyla Codex CLI (`gpt-5.6-terra`); 63 bulgu, 58 kabul/kısmi, 5 ret. Etki yansıtması Ürün Gereksinimleri ve MVP Kapsamı'nda tamam. **Proje sahibinin kararları:** K-479 tek depo kalıcı sınır · K-480 üç firma tipi (esnaf, gerçek kişi tacir, tüzel kişi) · K-481 abonelik bitince uyum firmada · K-482 bakımda geliştirici veri işleyen · K-489 misafir siparişlerinin e-postayla bağlanması kalır, adres devri riski kabul. **Öneriyle:** K-483…K-488, K-490. **Açık:** A-13 (MVP Kapsamı §5 eşiği), A-14 (K-209'un geri ödeme kuralı güncel Mesafeli Sözleşmeler Yönetmeliği'ne karşı — 13. madde 2025/2026'da değişmiş). Ayrıntı: tracker §6.1.
- **Önceki:** Yazım turu tamamlandı (2026-10-02, K-432): `01` (v0.6), `02` (v0.8) ve `10` (v0.2) oturumları. Blok 9 ✓ (2026-09-30) — Aşama 1'in workshop turu bitti. Ayrıntı: tracker §6.1.
- **SIRADA — Ürün Gereksinimleri'nin (`02`) kalite döngüsü (K-430, K-431).** Audit ve deep review çok mercekli ve karşı-doğrulamalı; **önce A-14** (yasal mercek K-209'u güncel Yönetmelik metnine karşı doğrular). Cross-review ikinci modeli: Codex CLI, ChatGPT hesabı (`gpt-5.6-terra`) — `cursor-agent` limiti dolduysa; talimatta "bilinçli karara yalnız katılmadığın için bulgu yazma" cümlesi olsun. Proje Vizyonu döngüsünün dersi: kural ayrıntısı evinde kalır, başka dokümana işaret edilir. Sıra K-437'de kilitli: döngü (`01` ✓ → `02` → `10`) → çakışma taraması → checkpoint → öğrenim terfisi (K-438'in dört adayı + tracker §6.1'deki beş yeni aday) → arşiv işareti → Aşama 2.
- **Taslaklar:** `01` **v0.26** (kalite döngüsü ✓, checkpoint aşama sonunda) · `02` **v0.10** (§1–§13) · `10` **v0.3** (§1–§5).
- **Yöntem (`INSTRUCTIONS §2`):** konu başına tek mesaj, numaralı öneri listesi, ⚠ işaretli üç grup; tamlık taraması liste hazırlanırken koşar, blok sonunda bloğun tamamına genişler. Yazım oturumunda bulunan boşluklar da aynı biçimle sunulur — ⚠ maddeler önce, K numaraları mesaj sırasıyla (`01`: sekiz, `02`: yirmi üç, `10`: altı madde; üçünde de ⚠ maddeye itiraz yok). `10`'da ⚠ grubuna giren madde çıkmadığı için altı madde öneriyle kaydedildi ve oturum sonu PR'ında bildirildi.
- **İleriye bağlayıcı kayıtlar:** **K-352** — `B9-05` ölçüm eklerse K-348 ve K-349 yeniden açılır (koşul gerçekleşmedi, K-397) · **K-400** — `B9-10` yalnız sipariş verisinden türetir · **K-420** — yol haritası yalnız `10 §3`'ten beslenir, `01 §7`'den asla · **K-424**'ün üç bağımlılık zinciri (öneri→analitik→çerez bandı · stokta haber ver ve terk edilmiş sepet→İYS onayı · SMS→yeni dış ön koşul; K-473 kargo ve havale otomasyonunu aynı kalıba koydu) · **K-439** — `01 §7`'nin sınırı ile aynı alandaki yol haritası adayı çelişmez: sınır kimliktir, aday özellik · **K-474** — tetikleyici gerçekleşince kargo entegrasyonu ve havale eşleştirmesi öne çekilir.
- **Aşağı dokümanlara devirler:** `04`–`09`, `12` ve `DEPLOY_RUNBOOK` satırları tracker'ın etki sütunlarındadır. **Blok 9'un ekledikleri:** `04`'e satış özeti ekranı (K-398), "bekleyen işlerim" sayaç bloğu (K-405), sipariş dışa aktarma (K-426) · `05`'e genel istek hızı (K-331), yedekleme ve kurtarma (K-407), erişim kayıtları (K-397) · `DEPLOY_RUNBOOK`'a yedek erişimi ve çıkış hakkı (K-427). **Yazım turunun ekledikleri:** `01` oturumu — `12`'ye ETBİS ve firma kimliği senaryoları (K-444) · `02` oturumu — `04`'e cayma düğmesinin kargoya verilince açılması (K-452), panel ana sayfasında kurulum kontrol listesi (K-469) ve altı sayaç (K-467), iptal sebebinde altı seçenek (K-451) ve düzeltme sebep seçici (K-466); `05`'e kayan pencereli limitler (K-460), günlük imha işi (K-449), SVG'nin dışarıda kalması (K-461); `06`'ya sipariş–sepet bağı ve sürümlü yasal metin kaydı (K-457, K-458); `12`'ye satış kapısının dört koşulu (K-465) ve §11'in değerleri · `10` oturumu — `04`'e Google düğmesinin kurulumda tanımlıysa görünmesi (K-478); `DEPLOY_RUNBOOK`'a `10 §4.1`'in kurulum adımları (K-476); `12`'ye `10 §2`'nin yetmiş altı satırı senaryo olarak (K-24) ve Ü-1'in alan adı ön koşulu (K-477). · **Kalite döngüsünün `01` turu:** `04`/`06`/`12`'ye üç firma tipi (K-480) · `05` ve `DEPLOY_RUNBOOK`'a güncellemenin kuruluma ulaşma yolu ile veri işleme sözleşmesi (K-482) · `12`'ye Ü-2'nin geçme kuralı (yalnız panel arayüzü) ve Ü-3'ün hat başına senaryoları · `02`'nin döngüsüne §7.3.3'ün K-205'i ürün politikası olarak anlatması.
- **Açık kararlar:** tracker §4'te **iki açık satır** — A-13 (MVP Kapsamı §5'in sıralama eşiği; vadesi `10`'un döngüsü) ve A-14 (K-209'un güncel mevzuata uyumu; vadesi `02`'nin döngüsü, yasal mercek). `02 §13`'ün tablosu boştur.
- **Gate durumu:** Implementation başlamadı, faz yok. Doküman: **01 ⏳ (v0.26 — kalite döngüsü ✓)** · **02 ⏳ (v0.10)** · **10 ⏳ (v0.3)** · 03–09, 11–12 ⬚. Blok planı: **0–9 ✓ — tamamı kapandı.** Plandaki **111 konunun 111'i** kapandı; 2026-08-17'den bu yana **490 karar**.
- **Oturum kapanışı (K-38):** doküman döneminde bu blok `/handoff` beklenmeden oturum sonu `docs:` PR'ının içinde güncellenir. Üç kez kaçırıldı (Blok 3 oturum 1 ve 4, Blok 5 oturum 1), üçünü de INSTRUCTIONS §3.0 yakaladı. Blok 6, 7, `02` yazım oturumu, Blok 8, Blok 9 ve yazım turunun `01`, `02`, `10` oturumları ve kalite döngüsünün `01` oturumu kurala uygun kapandı.
- **Son güncelleme:** 2026-10-03

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
