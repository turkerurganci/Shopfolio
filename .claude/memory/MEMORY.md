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

- **Son tamamlanan:** **Kalite döngüsü — `01` turu, kısmen (2026-10-02, `01` v0.16, `02` v0.10, `10` v0.3).** Audit ve deep review ✓: dokuz mercek, her bulguya iki şüpheci; 65 bulgunun 59'u doğrulandı; raporlar `Docs/AUDIT_REPORTS/01_AUDIT.md`, `01_DEEP_REVIEW.md`. **Dört ⚠ karar proje sahibine soruldu:** K-479 tek depo kalıcı sınır · K-480 firma tipi üç (esnaf, gerçek kişi tacir, tüzel kişi — öneri iki tip + isteğe bağlı alandı, proje sahibi üç tipi seçti) · K-481 aboneliği biten kurulumun uyumu firmada · K-482 bakım ve destekte geliştirici veri işleyen. **Öneriyle:** K-483…K-488 (ölçülerin kaynağı, M-3/M-4 tanımı, ölçüm penceresi ve M-4 okunma anı, §8 karar kuralları). **A-13 açıldı** (`10 §5`, K-423'ün üçüncü ölçütünün eşiği). **Cross-review 7 tur, TEMİZ değil:** 33 bulgunun 32'si işlendi, 1 ret; 8. tur `cursor-agent`'ın Cursor kullanım limitine takıldı. Ayrıntı: tracker §6.1.
- **Önceki:** Yazım turu tamamlandı (2026-10-02, K-432): `01` (v0.6), `02` (v0.8) ve `10` (v0.2) oturumları. Blok 9 ✓ (2026-09-30) — Aşama 1'in workshop turu bitti. Ayrıntı: tracker §6.1.
- **SIRADA — `01`'in cross-review'ı, 8. turdan (K-430, K-431).** Cursor kullanım limiti açılınca `cursor-agent` ile; girdi yalnız doküman ve şablon. TEMİZ dönünce etki yansıtması (Faz 5 — `02` v0.10 ve `10` v0.3'e şimdiye kadarki yansıma yapıldı, yalnız 8. turdan sonraki değişiklikler taranır), sonra `02`'nin kalite döngüsü. **Biri TEMİZ olmadan sonrakine geçilmez.** Sıra K-437'de kilitli: döngü (`01` → `02` → `10`) → çakışma taraması (K-429) → checkpoint → öğrenim terfisi (K-438'in dört adayı + tracker §6.1'deki yeni üç aday) → arşiv işareti (K-436) → Aşama 2.
- **Taslaklar:** `01` **v0.16** (§1–§8; audit ✓, deep review ✓, cross-review ⏳) · `02` **v0.10** (§1–§13) · `10` **v0.3** (§1–§5).
- **Yöntem (`INSTRUCTIONS §2`):** konu başına tek mesaj, numaralı öneri listesi, ⚠ işaretli üç grup; tamlık taraması liste hazırlanırken koşar, blok sonunda bloğun tamamına genişler. Yazım oturumunda bulunan boşluklar da aynı biçimle sunulur — ⚠ maddeler önce, K numaraları mesaj sırasıyla (`01`: sekiz, `02`: yirmi üç, `10`: altı madde; üçünde de ⚠ maddeye itiraz yok). `10`'da ⚠ grubuna giren madde çıkmadığı için altı madde öneriyle kaydedildi ve oturum sonu PR'ında bildirildi.
- **İleriye bağlayıcı kayıtlar:** **K-352** — `B9-05` ölçüm eklerse K-348 ve K-349 yeniden açılır (koşul gerçekleşmedi, K-397) · **K-400** — `B9-10` yalnız sipariş verisinden türetir · **K-420** — yol haritası yalnız `10 §3`'ten beslenir, `01 §7`'den asla · **K-424**'ün üç bağımlılık zinciri (öneri→analitik→çerez bandı · stokta haber ver ve terk edilmiş sepet→İYS onayı · SMS→yeni dış ön koşul; K-473 kargo ve havale otomasyonunu aynı kalıba koydu) · **K-439** — `01 §7`'nin sınırı ile aynı alandaki yol haritası adayı çelişmez: sınır kimliktir, aday özellik · **K-474** — tetikleyici gerçekleşince kargo entegrasyonu ve havale eşleştirmesi öne çekilir.
- **Aşağı dokümanlara devirler:** `04`–`09`, `12` ve `DEPLOY_RUNBOOK` satırları tracker'ın etki sütunlarındadır. **Blok 9'un ekledikleri:** `04`'e satış özeti ekranı (K-398), "bekleyen işlerim" sayaç bloğu (K-405), sipariş dışa aktarma (K-426) · `05`'e genel istek hızı (K-331), yedekleme ve kurtarma (K-407), erişim kayıtları (K-397) · `DEPLOY_RUNBOOK`'a yedek erişimi ve çıkış hakkı (K-427). **Yazım turunun ekledikleri:** `01` oturumu — `12`'ye ETBİS ve firma kimliği senaryoları (K-444) · `02` oturumu — `04`'e cayma düğmesinin kargoya verilince açılması (K-452), panel ana sayfasında kurulum kontrol listesi (K-469) ve altı sayaç (K-467), iptal sebebinde altı seçenek (K-451) ve düzeltme sebep seçici (K-466); `05`'e kayan pencereli limitler (K-460), günlük imha işi (K-449), SVG'nin dışarıda kalması (K-461); `06`'ya sipariş–sepet bağı ve sürümlü yasal metin kaydı (K-457, K-458); `12`'ye satış kapısının dört koşulu (K-465) ve §11'in değerleri · `10` oturumu — `04`'e Google düğmesinin kurulumda tanımlıysa görünmesi (K-478); `DEPLOY_RUNBOOK`'a `10 §4.1`'in kurulum adımları (K-476); `12`'ye `10 §2`'nin yetmiş altı satırı senaryo olarak (K-24) ve Ü-1'in alan adı ön koşulu (K-477). · **Kalite döngüsünün `01` turu:** `04`/`06`/`12`'ye üç firma tipi (K-480) · `05` ve `DEPLOY_RUNBOOK`'a güncellemenin kuruluma ulaşma yolu ile veri işleme sözleşmesi (K-482) · `12`'ye Ü-2'nin geçme kuralı (yalnız panel arayüzü) ve Ü-3'ün hat başına senaryoları · `02`'nin döngüsüne §7.3.3'ün K-205'i ürün politikası olarak anlatması.
- **Açık kararlar:** tracker §4'te **bir açık satır var — A-13** (`10 §5`'in sıralama ölçütündeki "düşük/yüksek" eşiği; vadesi `10`'un kalite döngüsünde audit başlamadan önce). `02 §13`'ün tablosu boştur. `01` ve `10`'un şablonunda açık kararlar bölümü yok — öğrenim adayı olarak tracker §6.1'de.
- **Gate durumu:** Implementation başlamadı, faz yok. Doküman: **01 ⏳ (v0.16 — audit ✓, deep review ✓, cross-review ⏳)** · **02 ⏳ (v0.10)** · **10 ⏳ (v0.3)** · 03–09, 11–12 ⬚. Blok planı: **0–9 ✓ — tamamı kapandı.** Plandaki **111 konunun 111'i** kapandı; 2026-08-17'den bu yana **488 karar**.
- **Oturum kapanışı (K-38):** doküman döneminde bu blok `/handoff` beklenmeden oturum sonu `docs:` PR'ının içinde güncellenir. Üç kez kaçırıldı (Blok 3 oturum 1 ve 4, Blok 5 oturum 1), üçünü de INSTRUCTIONS §3.0 yakaladı. Blok 6, 7, `02` yazım oturumu, Blok 8, Blok 9 ve yazım turunun `01`, `02`, `10` oturumları ve kalite döngüsünün `01` oturumu kurala uygun kapandı.
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
