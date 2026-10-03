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

- **AŞAMA 2 AÇIK — Kullanıcı akışları → Kullanıcı Akışları (`03`), 2026-10-04** (`00 §C.1`; rol Product Owner / Business Analyst; girdi `01`, `02`). Aşama 1 2026-10-03'te kapandı (K-437'nin altı adımı, PR #39–#55).
- **Son tamamlanan — açılış (karar kaydı v0.54):** devir §7'nin iki sorusu kapandı. **K-647:** karar kaydı dönem boyu tek dosyadır (`00 §B`, §G.1) — Aşama 2 kararları `PRODUCT_DISCOVERY_STATUS.md` §2'ye K-647'den, açık detaylar §4'e A-19'dan; aşamanın planı §8. Aşama 1'in kayıtları (K-01…K-646, A-01…A-18, §6, §7) salt okunur, tek istisna geri işaret; dosyanın tamamı dönemin son aşamasında arşivlenir (K-436'nın kapsamı daraltıldı, checklist §7'nin 5. ve 6. adımı hizalandı). **K-648:** ⚠ öneriyle kayıt ve `docs:` PR'larında CI yeşilse merge yetkisi Aşama 2 boyunca açık. **Öğrenim terfisi adayları** aşama boyunca tracker §8.1'e yazılır (Aşama 1'de §6.1); iki aday kayıtlı — şablonun karar kaydı başlığındaki iki okumaya açık cümle · devir notundaki sorunun şablona karşı taranmadan sorulması (v0.55). **K-649:** project-playbook'a geri gidecek her öğrenim `Docs/PLAYBOOK_FEEDBACK.md`'de toplanır (39 satır); proje tamamlanınca proje sahibi gönderir (v0.56).
- **Son adım — girdilerin okunması ve konu planı (karar kaydı v0.57):** `01`, `02` (tamamı), `10`, devir §7 ve beş devir kararı okundu; plan tracker §8.2–§8.3'te **taslak**: 12 blok (`AK0`…`AK11`, `03` şablonunun bölümlerine bire bir), **62 konu** — 6 workshop (W), 3 plan önerisi (P), 53 türetim (T); 12 ★. Konu kimliği `AKn-mm` (Aşama 1'in `Bn-mm`'siyle ve `02`'nin B-1…B-15'iyle çakışmaz). W konuları: AK3-06 ayrılmış adede dokunan katalog işlemi · AK5-03 ⚠ iade malının reddi ve değer kaybı · AK6-04 doğrulama bağlantısının yeniden istenmesine limit · AK8-09 ⚠ bekleyen yönetici davetinin geri çekilmesi · AK8-10 ⚠ donmayan ayarların (havale IBAN'ı, iade adresi) yürüyen siparişe etkisi · AK10-03 ⚠ site kesintisinde süreler. Öğrenim adayları 3–4 → PF-40, PF-41. Plan kabul edildi ve §8.1'in ikinci maddesi işaretlendi (2026-10-04); üç plan önerisi öneriyle kaydedildi: **K-650** iskelet (§2 müşteri tarafı, §8 firma ve panel, §9 üyelik) · **K-651** uçtan uca anlatılar §2'nin son alt bölümü · **K-652** `02`'ye geri besleme K satırı + aynı PR'da sürüm artışıyla; ✓ ve kalite döngüsü yeniden açılmaz.
- **SIRADA — workshop oturumu, altı W konusu** (tracker §8.3'teki sırayla): AK3-06 · AK5-03 ⚠ · AK6-04 · AK8-09 ⚠ · AK8-10 ⚠ · AK10-03 ⚠ — K-648 modu; her öneri listesi yeni elle adımı bütçeyle okur (AK8-05). Ardından tek yazım turu (tahmin: 5 oturum, durum makinesi önce).
- **Proje sahibine öneri — avukat teyidi (kalan hukuki risk):** K-491 (teslimden sonraki caymada, taşıyıcı belirtilmediğinde geri ödeme süresinin başlangıcı) ve K-627 (KEP üç tipte zorunlu). Önerilen kapı: ilk gerçek kurulumdan (`10` ÖK-12) önce.
- **Dokümanlar:** Proje Vizyonu (`01`) **v0.32** · Ürün Gereksinimleri (`02`) **v0.47** · MVP Kapsamı (`10`) **v0.29** — üçü ✓ Tamamlandı. Kullanıcı Akışları (`03`) ⏳ v0.1 (şablon). Karar kaydı **v0.57**, K-01…K-652. Metodoloji (`00`) **v1.0.4**.
- **Oturum kapanışı:** kural `INSTRUCTIONS §7`'de (K-33, K-38) — her oturum ve kapanış adımı bu bloğu kendi `docs:` PR'ında günceller, alt ajan dahil.
- **Gate durumu:** Implementation başlamadı, faz yok. Doküman: **01, 02, 10 ✓** · 03 ⏳ · 04–09, 11–12 ⬚. Aşama 1 blok planı: **0–9 ✓** — 111 konunun 111'i kapandı. Aşama 2 konu planı: kabul edildi (tracker §8.2–§8.3, 62 konu; K-650…K-652) — workshop sırada.
- **İleriye bağlayıcı kayıtlar:** **K-352** — `B9-05` ölçüm eklerse K-348 ve K-349 yeniden açılır (koşul gerçekleşmedi, K-397) · **K-400** — `B9-10` yalnız sipariş verisinden türetir · **K-420** — yol haritası yalnız `10 §3`'ten beslenir, `01 §7`'den asla · **K-424**'ün üç bağımlılık zinciri (öneri→analitik→çerez bandı · stokta haber ver ve terk edilmiş sepet→İYS onayı · SMS→yeni dış ön koşul; K-473 kargo ve havale otomasyonunu aynı kalıba koydu) · **K-439** — `01 §7`'nin sınırı ile aynı alandaki yol haritası adayı çelişmez: sınır kimliktir, aday özellik · **K-474** — tetikleyici gerçekleşince kargo entegrasyonu ve havale eşleştirmesi öne çekilir.
- **Aşağı dokümanlara devirler:** devir girdilerinin tek tablosu karar kaydının §7'sinde; kararların tam dizini çakışma taramasının §6'sında (`Docs/CHECKPOINT_REPORTS/PHASE1_CONFLICT_SCAN.md`) — kalite döngüsünün 147 kararı `03`–`08`, `12` ve `DEPLOY_RUNBOOK`'a 292 devir; daha önceki devirler tracker'ın etki sütunlarında.
- **Son güncelleme:** 2026-10-04

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
| Dokümanlar adıyla anılır; özetler kısa ve sade | `INSTRUCTIONS.md` §2 (PR #37) | 2026-10-02 |
| Toplu onay ve ⚠ öneriyle kayıt — iki mod, işaretleri ve gösterilmeyen kararların listesinin kapısı | `INSTRUCTIONS.md` §2 · `GUARDRAILS.md` §6 · `checklists/document-stage.md` §1, §7 | 2026-10-03 |
| Soru açmadan önce kural katmanları taranır; öncül önce anlatılır | `INSTRUCTIONS.md` §2 | 2026-10-03 |
| CI'da sessizlik yeşil değildir — check-run sayısı doğrulanır | `INSTRUCTIONS.md` §3.2 | 2026-10-03 |
| Her oturum ve kapanış adımı PR'ı Güncel Durum'u taşır (K-38); K numarası tekildir | `INSTRUCTIONS.md` §7 · `checklists/document-stage.md` §3 | 2026-10-03 |
| Yazım tek bağlamda; çok mercekli denetim kalite döngüsünde | `checklists/document-stage.md` §4 · `skills/audit` | 2026-10-03 |
| Aşama 1'in dokuz deseni; döngü yakınsamıyorsa ciddiyet ölçüsü | `00` §N.1, §C.5 | 2026-10-03 |
| Aşama 1'in süreç kararları (K-28, K-30, K-33…K-38, K-429…K-432, K-437) ve Aşama 1 öğrenimleri | `checklists/document-stage.md` §3, §4, §6, §7 · `skills/cross-review`, `audit`, `deep-review` · `scripts/git-hooks/pre-commit` | 2026-10-03 |
